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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class dwh_fin_mix
		
		'Declare table name
		Private Shared ReadOnly TABLE_NAME As String = "XFC_DWH_FIN_MIX"
		
		'Reference LOG table class (unified with ETL logging)
		Dim LogTable As New Workspace.__WsNamespacePrefix.__WsAssemblyName.STG_ETL_LOAD_LOG()
		
		#Region "Index Definitions"
		
		''' <summary>
		''' Returns the table name for this class
		''' </summary>
		Public Shared Function GetTableName() As String
			Return TABLE_NAME
		End Function
		
		''' <summary>
		''' Returns index definitions for this table: (TableName, IndexName, Columns)
		''' Used by extender_index_maintenance for dynamic index management
		''' </summary>
		Public Shared Function GetIndexDefinitions() As List(Of Tuple(Of String, String, String))
			Dim indexes As New List(Of Tuple(Of String, String, String))()
			' Index on Time column (critical for DELETE and WHERE IN operations)
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_DWH_FIN_MIX_Time", "[Time]"))
			' Composite index on Entity and Time for queries by entity and period
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_DWH_FIN_MIX_Entity_Time", "[Entity], [Time]"))
			Return indexes
		End Function
		
		#End Region
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String, Optional ByVal tableName As String = "XFC_DWH_FIN_MIX") As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
					    [Entity] NVARCHAR(30) NULL,
					    [ReferenceTransaction] NVARCHAR(50) NULL,
					    [PostingDate] DATE NULL,
					    [Time] NVARCHAR(7) NULL,
					    [IC] NVARCHAR(30) NULL,
					    [UD1] NVARCHAR(30) NULL,
						[UD4] NVARCHAR(30) NULL,
					    [UD5] NVARCHAR(30) NULL,
					    [UD8] NVARCHAR(30) NULL,
					    [Flow] NVARCHAR(30) NULL,
					    [Ledger] NVARCHAR(30) NULL,
					    [Account] NVARCHAR(30) NULL,
					    [Customer] NVARCHAR(30) NULL,
					    [Vendor] NVARCHAR(30) NULL,
					    [Amount] DECIMAL(18,2) NULL
					);
					
					-- Index on Time column (critical for DELETE and WHERE IN operations)
					CREATE NONCLUSTERED INDEX IX_DWH_FIN_MIX_Time ON {tableName} ([Time]);
					
					-- Composite index on Entity and Time for queries by entity and period
					CREATE NONCLUSTERED INDEX IX_DWH_FIN_MIX_Entity_Time ON {tableName} ([Entity], [Time]);
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE {tableName};
				"
				
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Get Population Query"

        Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String, Optional ByVal tableName As String = "XFC_DWH_FIN_MIX") As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"

				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {tableName};
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Populate DWH Table"
		
		#Region "Column Mapping Configuration"
		
		''' <summary>
		''' Base column mappings: SourceColumn → TargetColumn
		''' These are the default direct mappings from Landing to DWH.
		''' Columns with dynamic rules in XFC_ETL_RULES will override these.
		''' Note: For columns that map from the same source, use expressions or let dynamic rules handle it.
		''' Note: [Time] column now exists in LANDING table (populated during STG→LANDING insert)
		''' </summary>
		Private Shared ReadOnly BASE_COLUMN_MAPPINGS As New Dictionary(Of String, String) From {
			{"[companyCode]", "Entity"},
			{"[referenceTransaction]", "ReferenceTransaction"},
			{"CONVERT(DATE, CAST([postingDate] AS NVARCHAR(8)), 112)", "PostingDate"},
			{"[Time]", "Time"},
			{"[tradingPartner]", "IC"},
			{"[costCenter]", "UD1"},
			{"COALESCE([vendorAccountGroup], [customerAccountGroup])", "UD4"},
			{"CASE WHEN ud5f.CODE IS NOT NULL THEN [wbsElement] ELSE 'None' END", "UD5"},
			{"[ledger]", "UD8"},
			{"[transactionType]", "Flow"},
			{"CAST([ledger] AS NVARCHAR(30))", "Ledger"},
			{"CASE WHEN TRY_CAST([balanceSheetAccountGroup] AS INT) IS NOT NULL AND TRY_CAST([balanceSheetAccountGroup] AS INT) <> 0 THEN CAST(TRY_CAST([balanceSheetAccountGroup] AS INT) AS NVARCHAR(30)) ELSE [balanceSheetAccountGroup] END", "Account"},
			{"[customerAccountGroup]", "Customer"},
			{"[vendorAccountGroup]", "Vendor"}
		}
		
		''' <summary>
		''' Default values for dynamic mapping columns when no rules match.
		''' Add entries here for columns that need a specific default in their CASE ELSE.
		''' </summary>
		Private Shared ReadOnly DEFAULT_VALUES As New Dictionary(Of String, String) From {
			{"UD8", "Total_IFRS_ledger"},
			{"UD4", ""},
			{"UD1", ""},
			{"UD5", ""},
			{"Flow", ""},
			{"Ledger", ""}
		}
		
		''' <summary>
		''' Ordered list of target columns for INSERT statement.
		''' Must match the order in BASE_COLUMN_MAPPINGS values.
		''' </summary>
		Private Shared ReadOnly TARGET_COLUMNS As String() = {
			"Entity", "ReferenceTransaction", "PostingDate", "Time", "IC",
			"UD1", "UD4", "UD5", "UD8", "Flow", "Ledger",
			"Account", "Customer", "Vendor", "Amount"
		}
		
		#End Region

		''' <summary>
		''' Populates DWH table from Landing for a specific period range using DELETE + INSERT pattern.
		''' Dynamically applies transformation rules from XFC_ETL_RULES table.
		''' Processes data in batches (month by month) to avoid timeouts.
		''' Supports 3 modes:
		''' - Single period: fromTime = "2025M1", toTime = "" → Only January 2025
		''' - Range to now: fromTime = "2025M7", toTime = "" → From July 2025 to current period
		''' - Full range: fromTime = "2025M1", toTime = "2025M6" → January to June 2025
		''' </summary>
		''' <param name="sourceTable">Source landing table name (e.g., XFC_LANDING_FIN_MIX)</param>
		''' <param name="targetTable">Target DWH table name (e.g., XFC_DWH_FIN_MIX)</param>
		''' <param name="fromTime">Start period in OneStream format (e.g., 2025M1)</param>
		''' <param name="toTime">End period in OneStream format. Empty = current period for Range, same as fromTime for Single</param>
		''' <param name="loadId">LoadId from DWH_LOAD_CONTROL for logging</param>
		''' <param name="loadType">Type of load (e.g., 'Single', 'Range')</param>
		Public Sub PopulateDWHTable(
			ByVal si As SessionInfo,
			ByVal sourceTable As String,
			ByVal targetTable As String,
			ByVal fromTime As String,
			Optional ByVal toTime As String = "",
			Optional ByVal loadId As Integer = 0,
			Optional ByVal loadType As String = "Range"
		)
			Try
				BRApi.ErrorLog.LogMessage(si, $"[DEBUG] PopulateDWHTable called - Source: {sourceTable}, Target: {targetTable}, From: {fromTime}, To: {toTime}, LoadId: {loadId}")
				
				Dim executionStart As DateTime = DateTime.Now
				Dim totalRecordCount As Integer = 0
				
				'Determine effective period range
				Dim effectiveToTime As String = toTime
				If String.IsNullOrEmpty(effectiveToTime) Then
					If loadType.Equals("Single", StringComparison.OrdinalIgnoreCase) Then
						effectiveToTime = fromTime
					Else
						effectiveToTime = HelperFunctions.GetCurrentPeriod()
					End If
				End If
				
				BRApi.ErrorLog.LogMessage(si, $"[DEBUG] Effective period range: {fromTime} to {effectiveToTime}")
				
				'Generate list of periods to process
				Dim periods As List(Of String) = HelperFunctions.GeneratePeriodRange(fromTime, effectiveToTime)
				
				BRApi.ErrorLog.LogMessage(si, $"[DEBUG] Generated {periods.Count} periods to process in batches: {String.Join(", ", periods)}")
				
				If periods.Count = 0 Then
					Throw New XFException(si, New Exception($"Invalid period range: {fromTime} to {effectiveToTime}"))
				End If
				
				Dim periodRangeDesc As String = If(fromTime = effectiveToTime, fromTime, $"{fromTime} to {effectiveToTime}")
				
				'Log START if loadId is provided
				If loadId > 0 Then
					LogTable.InsertDWHLogEntry(si, loadId, loadType, sourceTable, targetTable, executionStart, periodRangeDesc)
				End If
				
				Try
					'Get all dynamic CASE logic for columns with rules (only once, reuse for all batches)
					Dim dynamicCaseLogic As Dictionary(Of String, String) = HelperFunctions.GetAllDynamicCaseLogic(si, sourceTable, targetTable, DEFAULT_VALUES)
					
					'Log which columns have dynamic mappings
					If dynamicCaseLogic.Count > 0 Then
						BRApi.ErrorLog.LogMessage(si, $"[INFO] Dynamic mappings found for columns: {String.Join(", ", dynamicCaseLogic.Keys)}")
					End If
					
					'Build SELECT expressions with dynamic overrides (only once, reuse for all batches)
					Dim selectExpressions As New List(Of String)()
					Dim groupByExpressions As New List(Of String)()
					
					'Source table alias for JOIN
					Const SOURCE_ALIAS As String = "st"
					
					For Each targetCol As String In TARGET_COLUMNS
						If targetCol = "Amount" Then
							' Amount is always SUM, not in GROUP BY
							selectExpressions.Add($"SUM({SOURCE_ALIAS}.[amountInLocalCurrency]) AS [Amount]")
						Else
							' Check if this column has dynamic rules
							Dim selectExpr As String
							Dim groupByExpr As String
							
							If dynamicCaseLogic.ContainsKey(targetCol) AndAlso targetCol <> "UD5" Then
								' Use dynamic CASE logic - apply alias to column references
								Dim caseLogic As String = ApplyTableAlias(dynamicCaseLogic(targetCol), SOURCE_ALIAS)
								selectExpr = $"{caseLogic} AS [{targetCol}]"
								groupByExpr = caseLogic
							Else
								' Use base mapping - apply alias to column references
								Dim sourceExpr As String = GetSourceExpressionForTarget(targetCol)
								Dim aliasedExpr As String = ApplyTableAlias(sourceExpr, SOURCE_ALIAS)
								selectExpr = $"{aliasedExpr} AS [{targetCol}]"
								groupByExpr = aliasedExpr
							End If
							
							selectExpressions.Add(selectExpr)
							groupByExpressions.Add(groupByExpr)
						End If
					Next
					
					'Process each period separately to avoid timeout on large tables
					' Uses [Time] column from LANDING (Index Seek instead of Table Scan)
					Dim batchNumber As Integer = 0
					For Each period As String In periods
						batchNumber += 1
						Dim batchStart As DateTime = DateTime.Now
						Dim batchRecordCount As Integer = 0
						
						BRApi.ErrorLog.LogMessage(si, $"[INFO] Processing batch {batchNumber}/{periods.Count}: Period {period}")
						
						Try
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								
								'Step 1: DELETE existing records for this period
								Dim deleteQuery As String = $"DELETE FROM {targetTable} WHERE [Time] = '{period}'"
								BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
								
								'Step 2: INSERT aggregated data from Landing for this period
								' Uses [Time] column from LANDING table (index-optimized, no CAST/SUBSTRING)
								Dim insertQuery As String = $"
									INSERT INTO {targetTable} (
										{String.Join(", ", TARGET_COLUMNS.Select(Function(c) $"[{c}]"))}
									)
									SELECT 
										{String.Join("," & vbCrLf & "										", selectExpressions)}
									FROM {sourceTable} st
									LEFT JOIN (
										SELECT DISTINCT LEFT(CCTreatmentCode, 3) AS CODE
										FROM XFC_DWH_FIN_UD5_FILTER
									) ud5f ON st.[wbsElement] LIKE ud5f.CODE + '%' AND LEN(st.[wbsElement]) >= 3
									WHERE st.[Time] = '{period}'
									GROUP BY 
										{String.Join("," & vbCrLf & "										", groupByExpressions)}
								"
								
								'DEBUG: Log the full INSERT query for troubleshooting
								BRApi.ErrorLog.LogMessage(si, $"[DEBUG-SQL] Insert Query for period {period}:{vbCrLf}{insertQuery}")
								
								BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
								
								'Count records inserted for this period
								Dim countQuery As String = $"SELECT COUNT(*) as RecordCount FROM {targetTable} WHERE [Time] = '{period}'"
								Dim dtCount As DataTable = BRApi.Database.GetDataTable(dbConn, countQuery, Nothing, Nothing, False)
								If dtCount.Rows.Count > 0 Then
									batchRecordCount = CInt(dtCount.Rows(0)("RecordCount"))
								End If
								
							End Using
							
							totalRecordCount += batchRecordCount
							Dim batchDuration As Double = (DateTime.Now - batchStart).TotalSeconds
							BRApi.ErrorLog.LogMessage(si, $"[INFO] Batch {batchNumber}/{periods.Count} completed: {batchRecordCount} records for {period} in {batchDuration:F2}s")
							
						Catch batchEx As Exception
							BRApi.ErrorLog.LogMessage(si, $"[ERROR] Batch {batchNumber} failed for period {period}: {batchEx.Message}")
							Throw
						End Try
					Next
					
					'Log SUCCESS if loadId is provided
					If loadId > 0 Then
						Dim executionEnd As DateTime = DateTime.Now
						Dim duration As Double = (executionEnd - executionStart).TotalSeconds
						LogTable.UpdateDWHLogEntry(si, loadId, sourceTable, targetTable, executionEnd, totalRecordCount, "OK", "", duration)
					End If
					
					BRApi.ErrorLog.LogMessage(si, $"[INFO] DWH load completed: {totalRecordCount} total records for {periods.Count} periods ({periodRangeDesc}) in {(DateTime.Now - executionStart).TotalSeconds:F2}s")
					
				Catch ex As Exception
					'Log ERROR if loadId is provided
					If loadId > 0 Then
						Dim executionEnd As DateTime = DateTime.Now
						Dim duration As Double = (executionEnd - executionStart).TotalSeconds
						Dim errorMsg As String = $"Error populating DWH table for periods {periodRangeDesc}: {ex.Message}"
						LogTable.UpdateDWHLogEntry(si, loadId, sourceTable, targetTable, executionEnd, totalRecordCount, "ERROR", errorMsg, duration)
					End If
					Throw
				End Try
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		''' <summary>
		''' Gets the source expression for a target column from BASE_COLUMN_MAPPINGS.
		''' </summary>
		Private Function GetSourceExpressionForTarget(ByVal targetColumn As String) As String
			For Each kvp As KeyValuePair(Of String, String) In BASE_COLUMN_MAPPINGS
				If kvp.Value = targetColumn Then
					Return kvp.Key
				End If
			Next
			' If not found, return the column name bracketed (assumes same name in source)
			Return $"[{targetColumn}]"
		End Function
		
		''' <summary>
		''' Applies a table alias to column references in a SQL expression.
		''' Converts [columnName] to alias.[columnName]
		''' Handles complex expressions like CAST([col] AS ...), CONVERT(..., [col], ...)
		''' Preserves existing aliases (e.g., ud5f.CODE, other_alias.[col])
		''' </summary>
		Private Shared Function ApplyTableAlias(ByVal expression As String, ByVal tableAlias As String) As String
			If String.IsNullOrEmpty(expression) OrElse String.IsNullOrEmpty(tableAlias) Then
				Return expression
			End If
			
			' Use regex to find [columnName] patterns that are not already prefixed with an alias
			' Pattern: matches [word] that is not preceded by a dot, word character, or closing bracket
			' This ensures we don't add alias to columns that already have one (e.g., alias.[col] or alias.col)
			Dim pattern As String = "(?<![.\w\]])(\[[a-zA-Z_][a-zA-Z0-9_]*\])"
			Dim replacement As String = $"{tableAlias}.$1"
			
			Return System.Text.RegularExpressions.Regex.Replace(expression, pattern, replacement)
		End Function
		
		''' <summary>
		''' Populates DWH table using configuration from DWH_LOAD_CONTROL table
		''' </summary>
		''' <param name="loadId">LoadId from DWH_LOAD_CONTROL</param>
		Public Sub PopulateDWHTableFromConfig(
			ByVal si As SessionInfo,
			ByVal loadId As Integer
		)
			Try
				'Get configuration from control table
				Dim config As HelperFunctions.DwhLoadConfig = HelperFunctions.GetDwhLoadConfigById(si, loadId)
				
				If config Is Nothing Then
					Throw New XFException(si, New Exception($"No enabled configuration found in XFC_DWH_LOAD_CONTROL for LoadId: {loadId}"))
				End If
				
				'Validate tables
				If String.IsNullOrEmpty(config.SourceTable) OrElse String.IsNullOrEmpty(config.TargetTable) Then
					Throw New XFException(si, New Exception($"SourceTable or TargetTable is empty for LoadId: {loadId}"))
				End If
				
				'Validate FromTime
				If String.IsNullOrEmpty(config.FromTime) Then
					Throw New XFException(si, New Exception($"FromTime is required for LoadId: {loadId}"))
				End If
				
				'Execute population
				Me.PopulateDWHTable(
					si,
					config.SourceTable,
					config.TargetTable,
					config.FromTime,
					config.ToTime,
					config.LoadId,
					config.LoadType
				)
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub

		#End Region

	End Class
End Namespace
