Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	
	''' <summary>
		''' Helper functions for ETL/DWH operations.
	''' </summary>
	Public Class HelperFunctions
		
		#Region "Table Name Constants"
		
		''' <summary>
		''' Prefix for staging tables
		''' </summary>
		Public Const STG_PREFIX As String = "XFC_STG_"
		
		''' <summary>
		''' Prefix for landing tables
		''' </summary>
		Public Const LANDING_PREFIX As String = "XFC_LANDING_"
		
		''' <summary>
		''' Prefix for data warehouse tables
		''' </summary>
		Public Const DWH_PREFIX As String = "XFC_DWH_"
		
		#End Region
		
		#Region "Table Name Derivation"
		
		''' <summary>
		''' Derives the Landing table name from the STG table name.
		''' Example: XFC_STG_FIN_MIX → XFC_LANDING_FIN_MIX
		''' </summary>
		Public Shared Function GetLandingTableFromStg(ByVal stgTable As String) As String
			If String.IsNullOrEmpty(stgTable) Then Return String.Empty
			Return stgTable.Replace(STG_PREFIX, LANDING_PREFIX)
		End Function
		
		''' <summary>
		''' Derives the DWH table name from the STG table name.
		''' Example: XFC_STG_FIN_MIX → XFC_DWH_FIN_MIX
		''' </summary>
		Public Shared Function GetDwhTableFromStg(ByVal stgTable As String) As String
			If String.IsNullOrEmpty(stgTable) Then Return String.Empty
			Return stgTable.Replace(STG_PREFIX, DWH_PREFIX)
		End Function
		
		''' <summary>
		''' Derives the DWH table name from the Landing table name.
		''' Example: XFC_LANDING_FIN_MIX → XFC_DWH_FIN_MIX
		''' </summary>
		Public Shared Function GetDwhTableFromLanding(ByVal landingTable As String) As String
			If String.IsNullOrEmpty(landingTable) Then Return String.Empty
			Return landingTable.Replace(LANDING_PREFIX, DWH_PREFIX)
		End Function
		
		''' <summary>
		''' Derives the STG table name from the Landing table name.
		''' Example: XFC_LANDING_FIN_MIX → XFC_STG_FIN_MIX
		''' </summary>
		Public Shared Function GetStgTableFromLanding(ByVal landingTable As String) As String
			If String.IsNullOrEmpty(landingTable) Then Return String.Empty
			Return landingTable.Replace(LANDING_PREFIX, STG_PREFIX)
		End Function
		
		''' <summary>
		''' Extracts the base name without prefix.
		''' Example: XFC_STG_FIN_MIX → FIN_MIX
		''' </summary>
		Public Shared Function GetBaseTableName(ByVal tableName As String) As String
			If String.IsNullOrEmpty(tableName) Then Return String.Empty
			Return tableName.Replace(STG_PREFIX, "").Replace(LANDING_PREFIX, "").Replace(DWH_PREFIX, "")
		End Function
		
		''' <summary>
		''' Gets all three table names (STG, LANDING, DWH) from any valid table name.
		''' Returns a Dictionary with keys: "STG", "LANDING", "DWH"
		''' </summary>
		Public Shared Function GetAllTableNames(ByVal tableName As String) As Dictionary(Of String, String)
			Dim result As New Dictionary(Of String, String)()
			Dim baseName As String = GetBaseTableName(tableName)
			
			result("STG") = STG_PREFIX & baseName
			result("LANDING") = LANDING_PREFIX & baseName
			result("DWH") = DWH_PREFIX & baseName
			
			Return result
		End Function
		
		#End Region
		
		#Region "DWH Load Control Helpers"
		
		''' <summary>
		''' Configuration object for DWH Load Control
		''' </summary>
		Public Class DwhLoadConfig
			Public Property LoadId As Integer = 0
			Public Property LoadType As String = "Range"
			Public Property SourceTable As String = String.Empty
			Public Property TargetTable As String = String.Empty
			Public Property FromTime As String = String.Empty
			Public Property ToTime As String = String.Empty
			Public Property Enable As Boolean = True
			Public Property Comments As String = String.Empty
		End Class
		
		''' <summary>
		''' Gets the DWH load configuration from DWH_LOAD_CONTROL table by LoadId.
		''' Returns Nothing if no enabled record found.
		''' </summary>
		Public Shared Function GetDwhLoadConfigById(
			ByVal si As SessionInfo,
			ByVal loadId As Integer
		) As DwhLoadConfig
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT LoadId, LoadType, SourceTable, TargetTable, FromTime, ToTime, Enable, Comments
						FROM XFC_DWH_LOAD_CONTROL 
						WHERE LoadId = {loadId}
						  AND Enable = 1
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Return ParseDwhLoadConfigRow(dt.Rows(0))
					End If
				End Using
				Return Nothing
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get DWH load config: {ex.Message}")
				Return Nothing
			End Try
		End Function
		
		''' <summary>
		''' Gets the DWH load configuration from DWH_LOAD_CONTROL table by SourceTable.
		''' Returns Nothing if no enabled record found.
		''' </summary>
		Public Shared Function GetDwhLoadConfig(
			ByVal si As SessionInfo,
			ByVal sourceTable As String
		) As DwhLoadConfig
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT LoadId, LoadType, SourceTable, TargetTable, FromTime, ToTime, Enable, Comments
						FROM XFC_DWH_LOAD_CONTROL 
						WHERE SourceTable = '{EscapeSql(sourceTable)}' 
						  AND Enable = 1
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Return ParseDwhLoadConfigRow(dt.Rows(0))
					End If
				End Using
				Return Nothing
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get DWH load config: {ex.Message}")
				Return Nothing
			End Try
		End Function
		
		''' <summary>
		''' Gets all enabled DWH load configurations.
		''' </summary>
		Public Shared Function GetAllEnabledDwhLoadConfigs(ByVal si As SessionInfo) As List(Of DwhLoadConfig)
			Dim configs As New List(Of DwhLoadConfig)()
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = "
						SELECT LoadId, LoadType, SourceTable, TargetTable, FromTime, ToTime, Enable, Comments
						FROM XFC_DWH_LOAD_CONTROL 
						WHERE Enable = 1
						ORDER BY LoadId
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							configs.Add(ParseDwhLoadConfigRow(row))
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get DWH load configs: {ex.Message}")
			End Try
			Return configs
		End Function
		
		''' <summary>
		''' Parses a DataRow into DwhLoadConfig object
		''' </summary>
		Private Shared Function ParseDwhLoadConfigRow(ByVal row As DataRow) As DwhLoadConfig
			Dim config As New DwhLoadConfig()
			config.LoadId = CInt(row("LoadId"))
			config.LoadType = If(IsDBNull(row("LoadType")), "Range", row("LoadType").ToString())
			config.SourceTable = If(IsDBNull(row("SourceTable")), "", row("SourceTable").ToString())
			config.TargetTable = If(IsDBNull(row("TargetTable")), "", row("TargetTable").ToString())
			config.FromTime = If(IsDBNull(row("FromTime")), "", row("FromTime").ToString())
			config.ToTime = If(IsDBNull(row("ToTime")), "", row("ToTime").ToString())
			config.Enable = True
			config.Comments = If(IsDBNull(row("Comments")), "", row("Comments").ToString())
			Return config
		End Function
		
		#End Region
		
		#Region "OneStream Period Helpers"
		
		''' <summary>
		''' Parses a OneStream period string (yyyyMm) to year and month.
		''' Example: "2025M1" → (2025, 1), "2025M11" → (2025, 11)
		''' Returns Nothing if parsing fails.
		''' </summary>
		Public Shared Function ParseOneStreamPeriod(ByVal period As String) As Tuple(Of Integer, Integer)
			If String.IsNullOrEmpty(period) Then Return Nothing
			
			Try
				Dim mIndex As Integer = period.IndexOf("M"c)
				If mIndex <= 0 Then Return Nothing
				
				Dim yearStr As String = period.Substring(0, mIndex)
				Dim monthStr As String = period.Substring(mIndex + 1)
				
				Dim year As Integer = 0
				Dim month As Integer = 0
				
				If Integer.TryParse(yearStr, year) AndAlso Integer.TryParse(monthStr, month) Then
					If month >= 1 AndAlso month <= 12 Then
						Return New Tuple(Of Integer, Integer)(year, month)
					End If
				End If
			Catch
				' Parsing failed
			End Try
			
			Return Nothing
		End Function
		
		''' <summary>
		''' Gets the current period in OneStream format.
		''' Example: If today is Nov 28, 2025 → "2025M11"
		''' </summary>
		Public Shared Function GetCurrentPeriod() As String
			Return $"{DateTime.Now.Year}M{DateTime.Now.Month}"
		End Function
		
		''' <summary>
		''' Compares two OneStream periods.
		''' Returns: -1 if period1 &lt; period2, 0 if equal, 1 if period1 &gt; period2
		''' </summary>
		Public Shared Function ComparePeriods(ByVal period1 As String, ByVal period2 As String) As Integer
			Dim p1 As Tuple(Of Integer, Integer) = ParseOneStreamPeriod(period1)
			Dim p2 As Tuple(Of Integer, Integer) = ParseOneStreamPeriod(period2)
			
			If p1 Is Nothing OrElse p2 Is Nothing Then Return 0
			
			If p1.Item1 < p2.Item1 Then Return -1
			If p1.Item1 > p2.Item1 Then Return 1
			
			If p1.Item2 < p2.Item2 Then Return -1
			If p1.Item2 > p2.Item2 Then Return 1
			
			Return 0
		End Function
		
		''' <summary>
		''' Generates a list of all periods between fromPeriod and toPeriod (inclusive).
		''' Example: GeneratePeriodRange("2025M1", "2025M3") → ["2025M1", "2025M2", "2025M3"]
		''' </summary>
		Public Shared Function GeneratePeriodRange(ByVal fromPeriod As String, ByVal toPeriod As String) As List(Of String)
			Dim periods As New List(Of String)()
			
			Dim pFrom As Tuple(Of Integer, Integer) = ParseOneStreamPeriod(fromPeriod)
			Dim pTo As Tuple(Of Integer, Integer) = ParseOneStreamPeriod(toPeriod)
			
			If pFrom Is Nothing OrElse pTo Is Nothing Then Return periods
			
			Dim currentYear As Integer = pFrom.Item1
			Dim currentMonth As Integer = pFrom.Item2
			
			While currentYear < pTo.Item1 OrElse (currentYear = pTo.Item1 AndAlso currentMonth <= pTo.Item2)
				periods.Add($"{currentYear}M{currentMonth}")
				
				currentMonth += 1
				If currentMonth > 12 Then
					currentMonth = 1
					currentYear += 1
				End If
				
				' Safety limit to prevent infinite loops
				If periods.Count > 120 Then Exit While ' Max 10 years
			End While
			
			Return periods
		End Function
		
		''' <summary>
		''' Builds SQL IN clause for a list of periods.
		''' Example: ["2025M1", "2025M2"] → "'2025M1','2025M2'"
		''' </summary>
		Public Shared Function BuildPeriodInClause(ByVal periods As List(Of String)) As String
			If periods Is Nothing OrElse periods.Count = 0 Then Return "''"
			Return String.Join(",", periods.Select(Function(p) $"'{EscapeSql(p)}'"))
		End Function
		
		''' <summary>
		''' Determines the effective FromTime and ToTime based on LoadType and config.
		''' Handles the 3 cases: Single period, From to Now, From to To
		''' </summary>
		Public Shared Function GetEffectivePeriodRange(ByVal config As DwhLoadConfig) As Tuple(Of String, String)
			Dim fromTime As String = config.FromTime
			Dim toTime As String = config.ToTime
			
			' If ToTime is empty, use current period
			If String.IsNullOrEmpty(toTime) Then
				toTime = GetCurrentPeriod()
			End If
			
			' If FromTime is empty, use ToTime (single period)
			If String.IsNullOrEmpty(fromTime) Then
				fromTime = toTime
			End If
			
			Return New Tuple(Of String, String)(fromTime, toTime)
		End Function
		
		#End Region
		
		#Region "ETL Load Control Helpers"
		
		''' <summary>
		''' Configuration object for ETL Load Control
		''' </summary>
		Public Class EtlLoadConfig
			Public Property LoadId As Integer = 0
			Public Property LoadType As String = String.Empty
			Public Property ReferenceDate As String = String.Empty
			Public Property EndDate As String = "0"
			Public Property TimeGap As Integer = 0
			Public Property OverlapPercentage As Decimal = 50D
			Public Property ReferenceColumn As String = "timestamp"
			Public Property TargetTable As String = String.Empty
			Public Property EndPoint As String = String.Empty
			Public Property Enable As Boolean = True
		End Class
		
		''' <summary>
		''' Gets the ETL load configuration from STG_ETL_LOAD_CONTROL table.
		''' Returns Nothing if no enabled record found.
		''' </summary>
		Public Shared Function GetEtlLoadConfig(
			ByVal si As SessionInfo,
			ByVal endpointName As String
		) As EtlLoadConfig
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT LoadId, LoadType, ReferenceDate, EndDate, TimeGap, OverlapPercentage, 
						       ReferenceColumn, TargetTable, EndPoint, Enable
						FROM XFC_STG_ETL_LOAD_CONTROL 
						WHERE EndPoint = '{EscapeSql(endpointName)}' 
						  AND Enable = 1
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Dim row As DataRow = dt.Rows(0)
						Dim config As New EtlLoadConfig()
						
						config.LoadId = CInt(row("LoadId"))
						config.LoadType = If(IsDBNull(row("LoadType")), "", row("LoadType").ToString())
						config.ReferenceDate = If(IsDBNull(row("ReferenceDate")), "", row("ReferenceDate").ToString())
						config.EndDate = If(IsDBNull(row("EndDate")), "0", row("EndDate").ToString())
						config.TimeGap = If(IsDBNull(row("TimeGap")), 0, CInt(row("TimeGap")))
						config.OverlapPercentage = If(IsDBNull(row("OverlapPercentage")), 50D, CDec(row("OverlapPercentage")))
						config.ReferenceColumn = If(IsDBNull(row("ReferenceColumn")) OrElse String.IsNullOrEmpty(row("ReferenceColumn").ToString()), "timestamp", row("ReferenceColumn").ToString())
						config.TargetTable = If(IsDBNull(row("TargetTable")), "", row("TargetTable").ToString())
						config.EndPoint = endpointName
						config.Enable = True
						
						Return config
					End If
				End Using
				Return Nothing
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get ETL load config: {ex.Message}")
				Return Nothing
			End Try
		End Function
		
		''' <summary>
		''' Gets all enabled ETL load configurations.
		''' </summary>
		Public Shared Function GetAllEnabledEtlEndpoints(ByVal si As SessionInfo) As DataTable
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = "
						SELECT EndPoint
						FROM XFC_STG_ETL_LOAD_CONTROL 
						WHERE Enable = 1
						ORDER BY LoadId
					"
					Return BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get ETL endpoints: {ex.Message}")
				Return Nothing
			End Try
		End Function
		
		''' <summary>
		''' Updates the ReferenceDate in the ETL Load Control table.
		''' </summary>
		Public Shared Sub UpdateControlTableReferenceDate(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			ByVal newReferenceDate As String
		)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim updateQuery As String = $"UPDATE XFC_STG_ETL_LOAD_CONTROL SET ReferenceDate = '{EscapeSql(newReferenceDate)}' WHERE LoadId = {loadId}"
					BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to update ReferenceDate for LoadId {loadId}: {ex.Message}", ex))
			End Try
		End Sub
		
		#End Region
		
		#Region "Time Format Helpers"
		
		''' <summary>
		''' Converts a DateTime to SQL datetime string format.
		''' Example: DateTime(2025, 1, 15, 12, 0, 0) → "2025-01-15 12:00:00.000"
		''' </summary>
		Public Shared Function DateTimeToSqlString(ByVal dt As DateTime) As String
			Return dt.ToString("yyyy-MM-dd HH:mm:ss.fff")
		End Function
		
		''' <summary>
		''' Converts a DateTime to timestamp string format (yyyyMMddHHmmss).
		''' Example: DateTime(2025, 1, 15, 12, 0, 0) → "20250115120000"
		''' </summary>
		Public Shared Function DateTimeToTimestamp(ByVal dt As DateTime) As String
			Return dt.ToString("yyyyMMddHHmmss")
		End Function
		
		''' <summary>
		''' Parses a timestamp string (yyyyMMddHHmmss) to DateTime.
		''' Returns Nothing if parsing fails.
		''' </summary>
		Public Shared Function ParseTimestamp(ByVal timestamp As String) As DateTime?
			Dim result As DateTime
			If DateTime.TryParseExact(timestamp, "yyyyMMddHHmmss", Nothing, DateTimeStyles.None, result) Then
				Return result
			End If
			Return Nothing
		End Function
		
		''' <summary>
		''' Converts a timestamp (yyyyMMddHHmmss) to OneStream period format (yyyyMm).
		''' Example: 20250115120000 → 2025M1
		''' </summary>
		Public Shared Function TimestampToOneStreamPeriod(ByVal timestamp As String) As String
			If String.IsNullOrEmpty(timestamp) OrElse timestamp.Length < 6 Then Return String.Empty
			
			Dim year As String = timestamp.Substring(0, 4)
			Dim monthStr As String = timestamp.Substring(4, 2)
			Dim month As Integer = 0
			
			If Integer.TryParse(monthStr, month) Then
				Return $"{year}M{month}"
			End If
			
			Return String.Empty
		End Function
		
		''' <summary>
		''' Converts a DateTime to OneStream period format (yyyyMm).
		''' Example: DateTime(2025, 1, 15) → 2025M1
		''' </summary>
		Public Shared Function DateTimeToOneStreamPeriod(ByVal dt As DateTime) As String
			Return $"{dt.Year}M{dt.Month}"
		End Function
		
		''' <summary>
		''' Converts a timestamp (yyyyMMddHHmmss) to SQL datetime string.
		''' Example: 20250115120000 → 2025-01-15 12:00:00.000
		''' </summary>
		Public Shared Function TimestampToSqlDateTime(ByVal timestamp As String) As String
			If String.IsNullOrEmpty(timestamp) OrElse timestamp.Length < 14 Then Return String.Empty
			
			Dim dt As DateTime
			If DateTime.TryParseExact(timestamp, "yyyyMMddHHmmss", Nothing, DateTimeStyles.None, dt) Then
				Return dt.ToString("yyyy-MM-dd HH:mm:ss.fff")
			End If
			
			Return String.Empty
		End Function
		
		#End Region
		
		#Region "SQL Helpers"
		
		''' <summary>
		''' Escapes single quotes in SQL strings to prevent injection.
		''' </summary>
		Public Shared Function EscapeSql(ByVal value As String) As String
			If String.IsNullOrEmpty(value) Then Return String.Empty
			Return value.Replace("'", "''")
		End Function
		
		''' <summary>
		''' Gets record count from a table.
		''' </summary>
		Public Shared Function GetTableRecordCount(ByVal si As SessionInfo, ByVal tableName As String) As Integer
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"SELECT COUNT(*) as RecordCount FROM {tableName}"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					If dt.Rows.Count > 0 Then
						Return CInt(dt.Rows(0)("RecordCount"))
					End If
				End Using
				Return 0
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get record count: {ex.Message}")
				Return 0
			End Try
		End Function
		
		''' <summary>
		''' Truncates a table.
		''' </summary>
		Public Shared Sub TruncateTable(ByVal si As SessionInfo, ByVal tableName As String)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					BRApi.Database.ExecuteSql(dbConn, $"TRUNCATE TABLE {tableName}", False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to truncate table {tableName}: {ex.Message}", ex))
			End Try
		End Sub
		
		#End Region
		
		#Region "Dynamic ETL Transformation Helpers"
		
		''' <summary>
		''' Gets all target columns that have dynamic mapping rules for a specific source→target table pair.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="sourceTable">Source table name (e.g., 'XFC_LANDING_FIN_MIX')</param>
		''' <param name="targetTable">Target table name (e.g., 'XFC_DWH_FIN_MIX')</param>
		''' <returns>List of target column names that have active rules</returns>
		Public Shared Function GetDynamicMappingColumns(
			ByVal si As SessionInfo,
			ByVal sourceTable As String,
			ByVal targetTable As String
		) As List(Of String)
			Dim columns As New List(Of String)()
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT DISTINCT TargetColumn
						FROM XFC_ETL_RULES
						WHERE SourceTable = '{EscapeSql(sourceTable)}'
						  AND TableMapping = '{EscapeSql(targetTable)}'
						  AND IsActive = 1
						ORDER BY TargetColumn
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							If Not IsDBNull(row("TargetColumn")) Then
								columns.Add(row("TargetColumn").ToString())
							End If
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get dynamic mapping columns: {ex.Message}")
			End Try
			Return columns
		End Function
		
		''' <summary>
		''' Gets all dynamic CASE logic for all columns with rules for a specific source→target mapping.
		''' Returns a dictionary where key = TargetColumn, value = SQL CASE statement.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="sourceTable">Source table name (e.g., 'XFC_LANDING_FIN_MIX')</param>
		''' <param name="targetTable">Target table name (e.g., 'XFC_DWH_FIN_MIX')</param>
		''' <param name="defaultValues">Dictionary of default values per column. If column not in dictionary, uses empty string.</param>
		''' <returns>Dictionary of TargetColumn → CASE SQL</returns>
		''' <example>
		''' Dim defaults As New Dictionary(Of String, String) From {{"UD8", "Total_IFRS_ledger"}, {"UD1", "Default"}}
		''' Dim allCaseLogic As Dictionary(Of String, String) = HelperFunctions.GetAllDynamicCaseLogic(si, "XFC_LANDING_FIN_MIX", "XFC_DWH_FIN_MIX", defaults)
		''' ' Returns: {"UD8" → "CASE WHEN ... END", "UD1" → "CASE WHEN ... END"}
		''' </example>
		Public Shared Function GetAllDynamicCaseLogic(
			ByVal si As SessionInfo,
			ByVal sourceTable As String,
			ByVal targetTable As String,
			Optional ByVal defaultValues As Dictionary(Of String, String) = Nothing
		) As Dictionary(Of String, String)
			Dim caseLogicDict As New Dictionary(Of String, String)()
			
			If defaultValues Is Nothing Then
				defaultValues = New Dictionary(Of String, String)()
			End If
			
			' Get all columns with dynamic rules
			Dim dynamicColumns As List(Of String) = GetDynamicMappingColumns(si, sourceTable, targetTable)
			
			' Build CASE logic for each column
			For Each targetColumn As String In dynamicColumns
				Dim ruleGroup As String = $"{sourceTable}__{targetTable}__{targetColumn}"
				Dim defaultValue As String = If(defaultValues.ContainsKey(targetColumn), defaultValues(targetColumn), "")
				Dim caseLogic As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.ETL_RULES.BuildDynamicCase(si, ruleGroup, defaultValue, targetTable, targetColumn)
				caseLogicDict(targetColumn) = caseLogic
			Next
			
			Return caseLogicDict
		End Function
		
		''' <summary>
		''' Builds a dynamic SQL CASE statement from rules in XFC_ETL_RULES table.
		''' This is a convenience wrapper around ETL_RULES.BuildDynamicCase for backward compatibility.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="ruleGroup">The rule group name (e.g., 'UD8_Map')</param>
		''' <param name="defaultValue">Fallback value if no rules found or as ELSE clause</param>
		''' <param name="tableMapping">Optional: Target table name to filter rules (e.g., 'XFC_DWH_FIN_MIX')</param>
		''' <param name="targetColumn">Optional: Target column name to filter rules (e.g., 'UD8')</param>
		''' <returns>SQL CASE statement string ready to embed in queries</returns>
		''' <example>
		''' Dim ud8Logic As String = HelperFunctions.GetDynamicCaseLogic(si, "UD8_Map", "Total_IFRS_ledger", "XFC_DWH_FIN_MIX")
		''' </example>
		Public Shared Function GetDynamicCaseLogic(
			ByVal si As SessionInfo, 
			ByVal ruleGroup As String, 
			ByVal defaultValue As String,
			Optional ByVal tableMapping As String = "",
			Optional ByVal targetColumn As String = ""
		) As String
			Return Workspace.__WsNamespacePrefix.__WsAssemblyName.ETL_RULES.BuildDynamicCase(si, ruleGroup, defaultValue, tableMapping, targetColumn)
		End Function
		
		#End Region
		
	End Class
End Namespace
