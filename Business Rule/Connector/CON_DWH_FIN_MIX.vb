Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Connector.CON_DWH_FIN_MIX
	Public Class MainClass
		'--------------------------------------------------------------------------------------------------------------------
		' Connector Business Rule: CON_DWH_FIN_MIX
		' Purpose: Retrieve data from internal Application DB table XFC_DWH_FIN_MIX
		'
		' Features:
		' - Dynamic field list from SQL schema (INFORMATION_SCHEMA.COLUMNS)
		' - Filters by Entity (from Workflow Profile Text1) and Time (from Workflow Period)
		' - DrillBack support for transaction detail
		' - ETL Integration: Executes LANDING → DWH transformation before reading data
		'
		' Created: December 2025
		' Updated: January 2026 - Added ETL integration for Landing → DWH
		'--------------------------------------------------------------------------------------------------------------------
		
		Private Const TABLE_NAME As String = "XFC_DWH_FIN_MIX"
		Private Const LANDING_TABLE_NAME As String = "XFC_LANDING_FIN_MIX"
		Private Const LOG_TABLE_NAME As String = "XFC_STG_ETL_LOAD_LOG"
		
		#Region "ETL Column Mappings"
		
		''' <summary>
		''' Base column mappings: SourceExpression → TargetColumn
		''' These are the default direct mappings from Landing to DWH.
		''' Columns with dynamic rules in XFC_ETL_RULES will override these.
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
		''' </summary>
		Private Shared ReadOnly TARGET_COLUMNS As String() = {
			"Entity", "ReferenceTransaction", "PostingDate", "Time", "IC",
			"UD1", "UD4", "UD5", "UD8", "Flow", "Ledger",
			"Account", "Customer", "Vendor", "Amount"
		}
		
		#End Region
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
			Try
				Select Case args.ActionType
					Case Is = ConnectorActionTypes.GetFieldList
						' Return exact field list matching the SELECT in GetSourceDataSQL
						' CRITICAL: Column count and order must match GetData exactly for drill-down to work
						Dim fields As New List(Of String) From {
							"Entity", "ReferenceTransaction", "PostingDate", "Time", "IC",
							"UD1", "UD4", "UD5", "UD8", "Flow", "Ledger",
							"Account", "Customer", "Vendor", "Amount"
						}
						Return fields
						
					Case Is = ConnectorActionTypes.GetData
						' 1. Get Workflow context for ETL filtering
						Dim wfClusterPK As WorkflowUnitClusterPk = api.WorkflowUnitPk.CreateWorkflowUnitClusterPk
						Dim iTimeID As Integer = wfClusterPK.TimeKey
						Dim iYear As String = BRApi.Finance.Time.GetYearFromId(si, iTimeID).ToString()
						Dim iPeriod As String = BRApi.Finance.Time.GetPeriodNumFromId(si, iTimeID).ToString()
						Dim sPeriod As String = $"{iYear}M{iPeriod}"
						
						Dim wfText1 As String = api.WorkflowProfile.GetAttributeValue(ScenarioTypeId.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text1)
						Dim entityInClause As String = BuildEntityInClause(wfText1)

						' 2. Execute LANDING → DWH transformation
						ExecuteLandingToDWH(si, sPeriod, entityInClause)
						
						' 3. Read from DWH using ProcessDataTable (Application DB - no external connection needed)
						Dim sourceDataSQL As String = GetSourceDataSQL(si, globals, api)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, sourceDataSQL, Nothing, Nothing, False)
							api.Parser.ProcessDataTable(si, dt, False, api.ProcessInfo)
						End Using
						Return Nothing
						
					Case Is = ConnectorActionTypes.GetDrillBackTypes
						Return GetDrillBackTypeList(si, globals, api, args)
						
					Case Is = ConnectorActionTypes.GetDrillBack
						Return GetDrillBack(si, globals, api, args)
				End Select
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "GetFieldListSQL"
		''' <summary>
		''' Dynamically retrieves column names from the table schema using INFORMATION_SCHEMA
		''' </summary>
		Private Function GetFieldListSQL(ByVal si As SessionInfo) As List(Of String)
			Try
				Dim fields As New List(Of String)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT COLUMN_NAME 
						FROM INFORMATION_SCHEMA.COLUMNS 
						WHERE TABLE_NAME = '{TABLE_NAME}'
						ORDER BY ORDINAL_POSITION"
					
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					For Each row As DataRow In dt.Rows
						fields.Add(row("COLUMN_NAME").ToString())
					Next
					
					End Using
				
				Return fields
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "GetSourceDataSQL"
		''' <summary>
		''' Builds the SQL query to retrieve data filtered by Entity and Time from Workflow context
		''' </summary>
		Private Function GetSourceDataSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer) As String
			Try
				' Get current Workflow Cluster for Time
				Dim wfClusterPK As WorkflowUnitClusterPk = api.WorkflowUnitPk.CreateWorkflowUnitClusterPk
				Dim iTimeID As Integer = wfClusterPK.TimeKey
				Dim iYear As String = BRApi.Finance.Time.GetYearFromId(si, iTimeID).ToString()
				Dim iPeriod As String = BRApi.Finance.Time.GetPeriodNumFromId(si, iTimeID).ToString()
				
				' Format period as OneStream format: 2025M1, 2025M12
				Dim sPeriod As String = $"{iYear}M{iPeriod}"

				' Get list of Entities from Workflow Profile Text1
				Dim wfText1 As String = api.WorkflowProfile.GetAttributeValue(ScenarioTypeId.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text1)
				Dim entityInClause As String = BuildEntityInClause(wfText1)

				' Build SQL - Must return ALL columns declared in GetFieldList
				' Mismatch between GetFieldList and GetData causes Stage storage issues
				Dim sql As String = $"
					SELECT
						[Entity],
						[ReferenceTransaction],
						[PostingDate],
						[Time],
						[IC],
						[UD1],
						[UD4],
						[UD5],
						[UD8],
						[Flow],
						[Ledger],
						[Account],
						[Customer],
						[Vendor],
						[Amount]
					FROM {TABLE_NAME}
					WHERE [Entity] IN ({entityInClause})
					  AND [Time] = '{sPeriod}'"

				Return sql
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		''' <summary>
		''' Builds the IN clause for Entity filter from comma-separated string
		''' </summary>
		Private Function BuildEntityInClause(ByVal entityList As String) As String
			If String.IsNullOrEmpty(entityList) Then
				Return "''"
			End If
			
			Dim elements As String() = entityList.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
			Dim sb As New System.Text.StringBuilder()
			
			For i As Integer = 0 To elements.Length - 1
				If i > 0 Then sb.Append(", ")
				sb.Append("'")
				sb.Append(elements(i).Trim())
				sb.Append("'")
			Next
			
			Return sb.ToString()
		End Function
#End Region

#Region "ETL: Landing to DWH Transformation"
		
		''' <summary>
		''' Executes the LANDING → DWH transformation for the specified period and entities.
		''' Applies dynamic transformation rules from XFC_ETL_RULES.
		''' Uses DELETE + INSERT pattern filtered by period and entity.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="period">Period in OneStream format (e.g., 2025M1)</param>
		''' <param name="entityInClause">Entity filter in SQL format (e.g., 'E001', 'E002')</param>
		Private Sub ExecuteLandingToDWH(ByVal si As SessionInfo, ByVal period As String, ByVal entityInClause As String)
			Dim executionStart As DateTime = DateTime.Now
			Dim recordCount As Integer = 0
			
			' Insert log entry with RUNNING status (LoadId=0 for Import flows)
			InsertLogEntry(si, period, entityInClause, executionStart)
			
			Try
				' Get dynamic CASE logic for all columns with rules
				Dim dynamicCaseLogic As Dictionary(Of String, String) = GetAllDynamicCaseLogic(si, LANDING_TABLE_NAME, TABLE_NAME)
				
				' Build SELECT expressions with dynamic overrides
				Dim selectExpressions As New List(Of String)()
				Dim groupByExpressions As New List(Of String)()
				Const SOURCE_ALIAS As String = "st"
				
				For Each targetCol As String In TARGET_COLUMNS
					If targetCol = "Amount" Then
						' Amount is always SUM, not in GROUP BY
						selectExpressions.Add($"SUM({SOURCE_ALIAS}.[amountInLocalCurrency]) AS [Amount]")
					Else
						Dim selectExpr As String
						Dim groupByExpr As String
						
						If dynamicCaseLogic.ContainsKey(targetCol) AndAlso targetCol <> "UD5" Then
							' Use dynamic CASE logic
							Dim caseLogic As String = ApplyTableAlias(dynamicCaseLogic(targetCol), SOURCE_ALIAS)
							selectExpr = $"{caseLogic} AS [{targetCol}]"
							groupByExpr = caseLogic
						Else
							' Use base mapping
							Dim sourceExpr As String = GetSourceExpressionForTarget(targetCol)
							Dim aliasedExpr As String = ApplyTableAlias(sourceExpr, SOURCE_ALIAS)
							selectExpr = $"{aliasedExpr} AS [{targetCol}]"
							groupByExpr = aliasedExpr
						End If
						
						selectExpressions.Add(selectExpr)
						groupByExpressions.Add(groupByExpr)
					End If
				Next
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					' Step 1: DELETE existing records for this period and entities
					Dim deleteQuery As String = $"DELETE FROM {TABLE_NAME} WHERE [Time] = '{period}' AND [Entity] IN ({entityInClause})"
					BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)

					' Step 2: INSERT aggregated data from Landing
					Dim insertQuery As String = $"
						INSERT INTO {TABLE_NAME} (
							{String.Join(", ", TARGET_COLUMNS.Select(Function(c) $"[{c}]"))}
						)
						SELECT 
							{String.Join("," & vbCrLf & "							", selectExpressions)}
						FROM {LANDING_TABLE_NAME} st
						LEFT JOIN (
							SELECT DISTINCT LEFT(CCTreatmentCode, 3) AS CODE
							FROM XFC_DWH_FIN_UD5_FILTER
						) ud5f ON st.[wbsElement] LIKE ud5f.CODE + '%' AND LEN(st.[wbsElement]) >= 3
						WHERE st.[Time] = '{period}'
						  AND st.[companyCode] IN ({entityInClause})
						GROUP BY 
							{String.Join("," & vbCrLf & "							", groupByExpressions)}
					"
					BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
					
					' Count records inserted
					Dim countQuery As String = $"SELECT COUNT(*) as RecordCount FROM {TABLE_NAME} WHERE [Time] = '{period}' AND [Entity] IN ({entityInClause})"
					Dim dtCount As DataTable = BRApi.Database.GetDataTable(dbConn, countQuery, Nothing, Nothing, False)
					If dtCount.Rows.Count > 0 Then
						recordCount = CInt(dtCount.Rows(0)("RecordCount"))
					End If
					
					Dim duration As Double = (DateTime.Now - executionStart).TotalSeconds
					BRApi.ErrorLog.LogMessage(si, $"[ETL] LANDING → DWH completed: {recordCount} records for Period {period} in {duration:F2}s")
				End Using
				
				' Update log entry with OK status
				Dim finalDuration As Double = (DateTime.Now - executionStart).TotalSeconds
				UpdateLogEntry(si, period, entityInClause, recordCount, "OK", "", finalDuration)
				
			Catch ex As Exception
				' Update log entry with ERROR status
				Dim errorDuration As Double = (DateTime.Now - executionStart).TotalSeconds
				UpdateLogEntry(si, period, entityInClause, recordCount, "ERROR", ex.Message, errorDuration)
				
				BRApi.ErrorLog.LogMessage(si, $"[ETL ERROR] Failed to transform LANDING → DWH: {ex.Message}")
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		
		''' <summary>
		''' Gets all dynamic CASE logic for columns with rules in XFC_ETL_RULES.
		''' </summary>
		Private Function GetAllDynamicCaseLogic(ByVal si As SessionInfo, ByVal sourceTable As String, ByVal targetTable As String) As Dictionary(Of String, String)
			Dim caseLogicDict As New Dictionary(Of String, String)()
			
			Try
				' Get all columns with dynamic rules
				Dim dynamicColumns As List(Of String) = GetDynamicMappingColumns(si, sourceTable, targetTable)
				
				' Build CASE logic for each column
				For Each targetColumn As String In dynamicColumns
					Dim ruleGroup As String = $"{sourceTable}__{targetTable}__{targetColumn}"
					Dim defaultValue As String = If(DEFAULT_VALUES.ContainsKey(targetColumn), DEFAULT_VALUES(targetColumn), "")
					Dim caseLogic As String = BuildDynamicCase(si, ruleGroup, defaultValue, targetTable, targetColumn)
					caseLogicDict(targetColumn) = caseLogic
				Next
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get dynamic case logic: {ex.Message}")
			End Try
			
			Return caseLogicDict
		End Function
		
		''' <summary>
		''' Gets all target columns that have dynamic mapping rules.
		''' </summary>
		Private Function GetDynamicMappingColumns(ByVal si As SessionInfo, ByVal sourceTable As String, ByVal targetTable As String) As List(Of String)
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
		''' Builds a dynamic SQL CASE statement from rules in XFC_ETL_RULES table.
		''' </summary>
		Private Function BuildDynamicCase(ByVal si As SessionInfo, ByVal ruleGroup As String, ByVal defaultValue As String, ByVal tableMapping As String, ByVal targetColumn As String) As String
			Dim rules As List(Of EtlRuleConfig) = GetRulesByGroup(si, ruleGroup, tableMapping, targetColumn)
			
			' If no rules found, return simple default value
			If rules Is Nothing OrElse rules.Count = 0 Then
				Return FormatTargetValue(defaultValue)
			End If
			
			' Check if there are any non-catch-all rules (rules with actual conditions, not just "1=1")
			Dim hasNonCatchAllRules As Boolean = rules.Any(Function(r) r.ConditionSQL.Trim() <> "1=1")
			
			' If ALL rules are catch-all (1=1), don't generate CASE - just return the catch-all value
			If Not hasNonCatchAllRules Then
				Dim catchAllRule As EtlRuleConfig = rules.FirstOrDefault(Function(r) r.ConditionSQL.Trim() = "1=1")
				Dim directValue As String = If(catchAllRule IsNot Nothing, catchAllRule.TargetValue, defaultValue)
				Return FormatTargetValue(directValue)
			End If
			
			Dim sb As New StringBuilder()
			sb.AppendLine("CASE")
			
			For Each rule As EtlRuleConfig In rules
				' Skip the catch-all rule (1=1) - we'll handle it as ELSE
				If rule.ConditionSQL.Trim() = "1=1" Then
					Continue For
				End If
				
				Dim formattedValue As String = FormatTargetValue(rule.TargetValue)
				sb.AppendLine($"    WHEN {rule.ConditionSQL} THEN {formattedValue}")
			Next
			
			' Find the catch-all rule (1=1) for ELSE, or use defaultValue
			Dim catchAllElseRule As EtlRuleConfig = rules.FirstOrDefault(Function(r) r.ConditionSQL.Trim() = "1=1")
			Dim elseValue As String = If(catchAllElseRule IsNot Nothing, catchAllElseRule.TargetValue, defaultValue)
			
			sb.AppendLine($"    ELSE {FormatTargetValue(elseValue)}")
			sb.Append("END")
			
			Return sb.ToString()
		End Function
		
		''' <summary>
		''' Gets rules from XFC_ETL_RULES table for a specific rule group.
		''' </summary>
		Private Function GetRulesByGroup(ByVal si As SessionInfo, ByVal ruleGroup As String, ByVal tableMapping As String, ByVal targetColumn As String) As List(Of EtlRuleConfig)
			Dim rules As New List(Of EtlRuleConfig)()
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT RuleGroup, Priority, ConditionSQL, TargetValue, Description
						FROM XFC_ETL_RULES
						WHERE RuleGroup = '{EscapeSql(ruleGroup)}'
						  AND IsActive = 1
						ORDER BY Priority ASC
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							Dim rule As New EtlRuleConfig()
							rule.RuleGroup = If(IsDBNull(row("RuleGroup")), "", row("RuleGroup").ToString())
							rule.Priority = If(IsDBNull(row("Priority")), 99, CInt(row("Priority")))
							rule.ConditionSQL = If(IsDBNull(row("ConditionSQL")), "1=1", row("ConditionSQL").ToString())
							rule.TargetValue = If(IsDBNull(row("TargetValue")), "", row("TargetValue").ToString())
							rules.Add(rule)
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get rules for group {ruleGroup}: {ex.Message}")
			End Try
			
			Return rules
		End Function
		
		''' <summary>
		''' Simple class to hold ETL rule configuration
		''' </summary>
		Private Class EtlRuleConfig
			Public Property RuleGroup As String
			Public Property Priority As Integer
			Public Property ConditionSQL As String
			Public Property TargetValue As String
		End Class
		
		''' <summary>
		''' Formats a target value for use in SQL.
		''' </summary>
		Private Function FormatTargetValue(ByVal value As String) As String
			If String.IsNullOrEmpty(value) Then
				Return "''"
			End If
			
			Dim trimmedValue As String = value.Trim()
			
			' Check if it's a column reference [columnName]
			If trimmedValue.StartsWith("[") AndAlso trimmedValue.EndsWith("]") Then
				Return trimmedValue
			End If
			
			' Check if it looks like a SQL expression
			If trimmedValue.ToUpper().Contains("COALESCE(") OrElse 
			   trimmedValue.ToUpper().Contains("ISNULL(") OrElse
			   trimmedValue.ToUpper().Contains("NULLIF(") OrElse
			   trimmedValue.ToUpper().Contains("TRY_CAST(") OrElse
			   trimmedValue.ToUpper().Contains("CASE ") OrElse
			   trimmedValue.ToUpper().Contains("CAST(") OrElse
			   trimmedValue.ToUpper().Contains("CONVERT(") OrElse
			   trimmedValue.Contains("[") OrElse
			   (trimmedValue.Contains("+") AndAlso (trimmedValue.Contains("[") OrElse trimmedValue.Contains("'"))) Then
				Return trimmedValue
			End If
			
			' It's a literal value - wrap in quotes
			Return $"'{EscapeSql(trimmedValue)}'"
		End Function
		
		''' <summary>
		''' Gets the source expression for a target column from BASE_COLUMN_MAPPINGS.
		''' </summary>
		Private Function GetSourceExpressionForTarget(ByVal targetColumn As String) As String
			For Each kvp As KeyValuePair(Of String, String) In BASE_COLUMN_MAPPINGS
				If kvp.Value = targetColumn Then
					Return kvp.Key
				End If
			Next
			Return $"[{targetColumn}]"
		End Function
		
		''' <summary>
		''' Applies a table alias to column references in a SQL expression.
		''' </summary>
		Private Shared Function ApplyTableAlias(ByVal expression As String, ByVal tableAlias As String) As String
			If String.IsNullOrEmpty(expression) OrElse String.IsNullOrEmpty(tableAlias) Then
				Return expression
			End If
			
			Dim pattern As String = "(?<![.\w])(\[[a-zA-Z_][a-zA-Z0-9_]*\])"
			Dim replacement As String = $"{tableAlias}.$1"
			
			Return Regex.Replace(expression, pattern, replacement)
		End Function
		
		''' <summary>
		''' Escapes single quotes for SQL injection prevention.
		''' </summary>
		Private Shared Function EscapeSql(ByVal value As String) As String
			If String.IsNullOrEmpty(value) Then Return ""
			Return value.Replace("'", "''")
		End Function
		
#End Region

#Region "GetDrillBackTypeList"
		''' <summary>
		''' Returns available drill back types for the connector
		''' </summary>
		Private Function GetDrillBackTypeList(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As List(Of DrillBackTypeInfo)
			Dim drillTypes As New List(Of DrillBackTypeInfo)

			If String.Equals(args.DrillCode, StageConstants.TransformationGeneral.DrillCodeDefaultValue, StringComparison.InvariantCultureIgnoreCase) Then
				drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid, New NameAndDesc("TransactionDetail", "Transaction Detail")))
			End If

			Return drillTypes
		End Function
#End Region

#Region "GetDrillBack"
		''' <summary>
		''' Executes the drill back query and returns results
		''' </summary>
		Private Function GetDrillBack(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As DrillBackResultInfo
			Try
				If String.Equals(args.DrillBackType.NameAndDescription.Name, "TransactionDetail", StringComparison.InvariantCultureIgnoreCase) Then
					Dim drillBackSQL As String = GetSourceDataSQL_DrillBack(si, globals, api, args)
					Dim drillBackInfo As New DrillBackResultInfo
					drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid

					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, drillBackSQL, Nothing, Nothing, False)
						drillBackInfo.DataTable = New XFDataTable(si, dt, Nothing, args.PageSize, args.PageNumber)
					End Using

					Return drillBackInfo
				Else
					Return Nothing
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "GetSourceDataSQL_DrillBack"
		''' <summary>
		''' Builds the SQL query for drill back with additional transaction detail columns
		''' </summary>
		Private Function GetSourceDataSQL_DrillBack(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As String
			Try
				' Get field values from the selected source data row
				Dim sourceValues As Dictionary(Of String, Object) = api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)

				Dim entity As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.Entity, "Entity")
				Dim account As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.Attribute7, "Account")
				Dim flow As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.Flow, "Flow")
				Dim ic As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.IC, "IC")
				Dim ud1 As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.Attribute1, "UD1")
				Dim ud4 As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.Attribute4, "UD4")
				Dim ud5 As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.Attribute5, "UD5")
				Dim ud8 As String = SafeGetValue(sourceValues, StageConstants.MasterDimensionNames.Attribute8, "UD8")
				
				' Get Time from the source row - try multiple key options
				Dim sPeriod As String = ""
				If sourceValues.ContainsKey(StageTableFields.StageSourceData.DimWorkflowTimeKey) AndAlso sourceValues(StageTableFields.StageSourceData.DimWorkflowTimeKey) IsNot Nothing Then
					Dim timeKey As String = sourceValues(StageTableFields.StageSourceData.DimWorkflowTimeKey).ToString()
					Dim year As String = TimeDimHelper.GetYearFromId(timeKey)
					Dim month As String = TimeDimHelper.GetSubComponentsFromId(timeKey).Month.ToString()
					sPeriod = $"{year}M{month}"
				ElseIf sourceValues.ContainsKey("Time") AndAlso sourceValues("Time") IsNot Nothing Then
					' Fallback: use Time column directly from source data
					sPeriod = sourceValues("Time").ToString()
				End If
				
				' Build SQL with escaped values to prevent SQL injection
				Dim sql As String = $"
					SELECT 
						[Entity],
						[ReferenceTransaction],
						[PostingDate],
						[Time],
						[IC],
						[UD1],
						[UD4],
						[UD5],
						[UD8],
						[Flow],
						[Ledger],
						[Account],
						[Customer],
						[Vendor],
						[Amount]
					FROM {TABLE_NAME}
					WHERE [Entity] = '{EscapeSql(entity)}'
					  AND [Time] = '{EscapeSql(sPeriod)}'
					  AND ISNULL(NULLIF(NULLIF([Account], 'None'), ''), '') = '{EscapeSql(account)}'
					  AND ISNULL(NULLIF(NULLIF([Flow], 'None'), ''), '') = '{EscapeSql(flow)}'
					  AND ISNULL(NULLIF(NULLIF([IC], 'None'), ''), '') = '{EscapeSql(ic)}'
					  AND ISNULL(NULLIF(NULLIF([UD1], 'None'), ''), '') = '{EscapeSql(ud1)}'
					  AND ISNULL(NULLIF(NULLIF([UD4], 'None'), ''), '') = '{EscapeSql(ud4)}'
					  AND ISNULL(NULLIF(NULLIF([UD5], 'None'), ''), '') = '{EscapeSql(ud5)}'
					  AND ISNULL(NULLIF(NULLIF([UD8], 'None'), ''), '') = '{EscapeSql(ud8)}'
					ORDER BY [PostingDate], [ReferenceTransaction]"

				Return sql
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		''' <summary>
		''' Safely gets a value from the source values dictionary with fallback key.
		''' Tries primaryKey first, then fallbackKey. Returns empty string if both miss or value is null/"None".
		''' </summary>
		Private Function SafeGetValue(ByVal sourceValues As Dictionary(Of String, Object), ByVal primaryKey As String, Optional ByVal fallbackKey As String = Nothing) As String
			' Try primary key
			If sourceValues.ContainsKey(primaryKey) AndAlso sourceValues(primaryKey) IsNot Nothing Then
				Dim value As String = sourceValues(primaryKey).ToString()
				If value <> "None" Then Return value
			End If
			' Try fallback key
			If fallbackKey IsNot Nothing AndAlso sourceValues.ContainsKey(fallbackKey) AndAlso sourceValues(fallbackKey) IsNot Nothing Then
				Dim value As String = sourceValues(fallbackKey).ToString()
				If value <> "None" Then Return value
			End If
			Return ""
		End Function
#End Region

#Region "ETL Logging Functions"
		
		''' <summary>
		''' Inserts a log entry with RUNNING status when Import process starts.
		''' Uses SQL directly to maintain connector independence from Assembly.
		''' LoadId=0 identifies Import flows vs configured DWH loads.
		''' </summary>
		Private Sub InsertLogEntry(ByVal si As SessionInfo, ByVal period As String, ByVal entityInClause As String, ByVal executionStart As DateTime)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim sqlDateTime As String = executionStart.ToString("yyyy-MM-dd HH:mm:ss")
					Dim safeEntity As String = EscapeSql(entityInClause)
					
					Dim insertQuery As String = $"
						INSERT INTO {LOG_TABLE_NAME} (
							[LoadId], [LoadType], [SourceTable], [TargetTable], [ExecutionStart], [ExecutionEnd],
							[Records], [LoadStatus], [ErrorMessage], [ReferenceDate], [ExecutedBy], [ExecutionDuration], [Entity]
						)
						VALUES (
							0,
							'Import',
							'{EscapeSql(LANDING_TABLE_NAME)}',
							'{EscapeSql(TABLE_NAME)}',
							'{sqlDateTime}',
							'{sqlDateTime}',
							0,
							'RUNNING',
							NULL,
							'{EscapeSql(period)}',
							'{si.UserName}',
							0,
							'{safeEntity}'
						)
					"
					BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
				End Using
			Catch ex As Exception
				' Log error but don't throw to avoid interrupting main process
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to insert log entry: {ex.Message}")
			End Try
		End Sub
		
		''' <summary>
		''' Updates the log entry when Import process completes (OK or ERROR).
		''' Uses SQL directly to maintain connector independence from Assembly.
		''' </summary>
		Private Sub UpdateLogEntry(ByVal si As SessionInfo, ByVal period As String, ByVal entityInClause As String, ByVal recordCount As Integer, ByVal loadStatus As String, ByVal errorMessage As String, ByVal executionDuration As Double)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim sqlDateTime As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
					Dim safeDuration As String = executionDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)
					Dim safeEntity As String = EscapeSql(entityInClause)
					
					Dim updateQuery As String = $"
						UPDATE {LOG_TABLE_NAME} 
						SET LoadStatus = '{EscapeSql(loadStatus)}', 
							ExecutionEnd = '{sqlDateTime}', 
							Records = {recordCount}, 
							ErrorMessage = '{EscapeSql(errorMessage)}',
							ExecutionDuration = {safeDuration}
						WHERE LoadId = 0 
						  AND SourceTable = '{EscapeSql(LANDING_TABLE_NAME)}'
						  AND TargetTable = '{EscapeSql(TABLE_NAME)}'
						  AND ReferenceDate = '{EscapeSql(period)}'
						  AND [Entity] = '{safeEntity}'
						  AND LoadStatus = 'RUNNING'
					"
					BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
				End Using
			Catch ex As Exception
				' Log error but don't throw to avoid interrupting main process
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to update log entry: {ex.Message}")
			End Try
		End Sub
		
#End Region

	End Class
End Namespace