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
Imports Workspace.__WsNamespacePrefix.__WsAssemblyName

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.etl_rule_actions
	
	''' <summary>
	''' Dashboard Extender Business Rule for ETL Rule Management Actions.
	''' Called by Dashboard buttons to perform CRUD operations on ETL rules.
	''' 
	''' FunctionNames:
	'''   SaveRule - Create or update a rule
	'''   DeleteRule - Delete a rule by ID
	'''   ToggleRule - Activate/deactivate a rule
	'''   NewRule - Clear form for new rule entry
	'''   LoadRule - Load a rule for editing
	''' </summary>
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				' DEBUG: Log entry point
				
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						' Button clicks come through ComponentSelectionChanged
						
						Dim result As New XFSelectionChangedTaskResult()
						result.IsOK = True
						result.ShowMessageBox = True
						
						' Get action from NameValuePairs (button sends action parameter) or fallback to FunctionName
						Dim actionName As String = ""
						If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("action") Then
							actionName = args.NameValuePairs("action").ToLower()
						ElseIf Not String.IsNullOrEmpty(args.FunctionName) Then
							actionName = args.FunctionName.ToLower()
						End If
						
						' Route based on action
						Select Case actionName
							Case "saverule"
								Dim saveResult As String = Me.SaveRule(si, args)
								result.Message = saveResult
								result.IsOK = saveResult.StartsWith("Success")
								
								' Reset form fields after successful save for creating new rules
								If result.IsOK Then
									Dim modVars As New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
									modVars.Add("prm_ETL_Load_Control_Selected_Rule_Id", "0")
									modVars.Add("prm_ETL_Load_Control_Target_Value", "")
									modVars.Add("prm_ETL_Load_Control_Description", "")
									modVars.Add("prm_ETL_Load_Control_Priority", "99")
									modVars.Add("prm_ETL_Load_Control_Conditional_Value", "")
									result.ChangeCustomSubstVarsInDashboard = True
									result.ModifiedCustomSubstVars = modVars
								End If
								
							Case "deleterule"
								Dim deleteResult As String = Me.DeleteRule(si, args)
								result.Message = deleteResult
								result.IsOK = deleteResult.StartsWith("Success")
								
								' Reset selected rule ID after delete
								If result.IsOK Then
									Dim modVars As New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
									modVars.Add("prm_ETL_Load_Control_Selected_Rule_Id", "0")
									result.ChangeCustomSubstVarsInDashboard = True
									result.ModifiedCustomSubstVars = modVars
								End If
								
							Case "togglerule"
								Dim toggleResult As String = Me.ToggleRule(si, args)
								result.Message = toggleResult
								result.IsOK = toggleResult.StartsWith("Success")
								
							Case "newrule"
								Dim newResult As String = Me.NewRule(si, args)
								result.Message = newResult
								result.IsOK = newResult.StartsWith("Success")
								
								' Reset all form fields for new rule entry
								If result.IsOK Then
									Dim modVars As New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
									modVars.Add("prm_ETL_Load_Control_Selected_Rule_Id", "0")
									modVars.Add("prm_ETL_Load_Control_Target_Value", "")
									modVars.Add("prm_ETL_Load_Control_Description", "")
									modVars.Add("prm_ETL_Load_Control_Priority", "99")
									modVars.Add("prm_ETL_Load_Control_Conditional_Value", "")
									modVars.Add("prm_ETL_Load_Control_Is_Active", "1")
									result.ChangeCustomSubstVarsInDashboard = True
									result.ModifiedCustomSubstVars = modVars
								End If
								
							Case "loadrule"
								Dim loadResult As String = Me.LoadRule(si, args)
								result.Message = loadResult
								result.IsOK = loadResult.StartsWith("Success")
								
							Case "editrule"
								' Load rule data into form fields for editing
								Dim editResult As Object = Me.EditRule(si, args)
								If TypeOf editResult Is XFSelectionChangedTaskResult Then
									Return editResult
								Else
									result.Message = CStr(editResult)
									result.IsOK = False
								End If
								
							Case Else
								' Default to SaveRule
								Dim defaultResult As String = Me.SaveRule(si, args)
								result.Message = defaultResult
								result.IsOK = defaultResult.StartsWith("Success")
						End Select
						
						Return result
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						' Handle dashboard load if needed
						If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And _
						   args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
							loadDashboardTaskResult.ModifiedCustomSubstVars = Nothing
							Return loadDashboardTaskResult
						End If
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		''' <summary>
		''' Gets a parameter value from the Dashboard NameValuePairs or SelectionChangedTaskInfo
		''' </summary>
		Private Function GetParam(ByVal args As DashboardExtenderArgs, ByVal paramName As String, Optional ByVal defaultValue As String = "") As String
			Try
				' Try to get from NameValuePairs first
				If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey(paramName) Then
					Return args.NameValuePairs(paramName)
				End If
				
				' Try to get from SelectionChangedTaskInfo.CustomSubstVars
				If args.SelectionChangedTaskInfo IsNot Nothing AndAlso _
				   args.SelectionChangedTaskInfo.CustomSubstVars IsNot Nothing AndAlso _
				   args.SelectionChangedTaskInfo.CustomSubstVars.ContainsKey(paramName) Then
					Return args.SelectionChangedTaskInfo.CustomSubstVars(paramName)
				End If
				
				Return defaultValue
			Catch
				Return defaultValue
			End Try
		End Function
		
		''' <summary>
		''' Saves a new or existing rule from Dashboard parameters
		''' </summary>
		Private Function SaveRule(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
			Try
				' Get parameters from Dashboard
				Dim ruleIdStr As String = Me.GetParam(args, "prm_ETL_Load_Control_Selected_Rule_Id", "0")
				Dim ruleId As Integer = 0
				Integer.TryParse(ruleIdStr, ruleId)
				
				Dim sourceTable As String = Me.GetParam(args, "prm_ETL_Load_Control_Source_Table", "")
				Dim tableMapping As String = Me.GetParam(args, "prm_ETL_Load_Control_Target_Table", "")
				Dim targetColumn As String = Me.GetParam(args, "prm_ETL_Load_Control_Target_Column", "")
				
				Dim priorityStr As String = Me.GetParam(args, "prm_ETL_Load_Control_Priority", "99")
				Dim priority As Integer = 99
				Integer.TryParse(priorityStr, priority)
				
				Dim conditionColumn As String = Me.GetParam(args, "prm_ETL_Load_Control_Source_Column", "")
				Dim conditionOperator As String = Me.GetParam(args, "prm_ETL_Load_Control_Operator", "=")
				Dim conditionValue As String = Me.GetParam(args, "prm_ETL_Load_Control_Conditional_Value", "")
				Dim targetValue As String = Me.GetParam(args, "prm_ETL_Load_Control_Target_Value", "")
				Dim description As String = Me.GetParam(args, "prm_ETL_Load_Control_Description", "")
				Dim isActiveStr As String = Me.GetParam(args, "prm_ETL_Load_Control_Is_Active", "1")
				Dim isActive As Boolean = (isActiveStr = "1" OrElse isActiveStr.ToLower() = "true")
				
				' Validation
				If String.IsNullOrEmpty(sourceTable) OrElse sourceTable.StartsWith("--") Then
					Return "Error: Source table is required"
				End If
				
				If String.IsNullOrEmpty(tableMapping) OrElse tableMapping.StartsWith("--") Then
					Return "Error: Target table is required"
				End If
				
				If String.IsNullOrEmpty(targetColumn) OrElse targetColumn.StartsWith("--") Then
					Return "Error: Target column is required"
				End If
				
				' For CUSTOM/1=1 operators, condition column is optional
				Dim isCustomOperator As Boolean = (conditionOperator.ToUpper() = "CUSTOM" OrElse _
				                                    conditionOperator.ToUpper() = "RAW" OrElse _
				                                    conditionOperator.ToUpper() = "SQL" OrElse _
				                                    conditionOperator.ToUpper() = "1=1" OrElse _
				                                    conditionOperator.ToUpper() = "ALWAYS" OrElse _
				                                    conditionOperator.ToUpper() = "ELSE" OrElse _
				                                    conditionOperator.ToUpper() = "DEFAULT")
				
				If Not isCustomOperator AndAlso (String.IsNullOrEmpty(conditionColumn) OrElse conditionColumn.StartsWith("--")) Then
					Return "Error: Condition column is required"
				End If
				
				' For CUSTOM operator, the conditionValue contains the raw SQL
				If isCustomOperator AndAlso String.IsNullOrEmpty(conditionValue) AndAlso conditionOperator.ToUpper() <> "1=1" AndAlso _
				   conditionOperator.ToUpper() <> "ALWAYS" AndAlso conditionOperator.ToUpper() <> "ELSE" AndAlso conditionOperator.ToUpper() <> "DEFAULT" Then
					Return "Error: Custom SQL condition is required in Condition Value field"
				End If
				
				If String.IsNullOrEmpty(targetValue) Then
					Return "Error: Target value is required"
				End If
				
				' Build condition SQL from components
				Dim conditionSql As String = Me.BuildConditionSQL(conditionColumn, conditionOperator, conditionValue)
				
				' Auto-generate RuleGroup from SourceTable, TableMapping and TargetColumn
				Dim ruleGroup As String = ETL_RULES.GenerateRuleGroup(sourceTable, tableMapping, targetColumn)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					If ruleId = 0 Then
						' Insert new rule
						Dim insertSql As String = $"
							INSERT INTO XFC_ETL_RULES 
							(RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, ConditionColumn, ConditionSQL, TargetValue, Description, IsActive)
							VALUES 
							('{HelperFunctions.EscapeSql(ruleGroup)}', '{HelperFunctions.EscapeSql(sourceTable)}', '{HelperFunctions.EscapeSql(tableMapping)}', '{HelperFunctions.EscapeSql(targetColumn)}', 
							 {priority}, '{HelperFunctions.EscapeSql(conditionColumn)}', '{HelperFunctions.EscapeSql(conditionSql)}', '{HelperFunctions.EscapeSql(targetValue)}', 
							 '{HelperFunctions.EscapeSql(description)}', {If(isActive, 1, 0)})
						"
						BRApi.Database.ExecuteActionQuery(dbConn, insertSql, False, True)
						Return "Success: Rule created successfully"
					Else
						' Update existing rule - regenerate RuleGroup
						Dim updateSql As String = $"
							UPDATE XFC_ETL_RULES SET
								RuleGroup = '{HelperFunctions.EscapeSql(ruleGroup)}',
								SourceTable = '{HelperFunctions.EscapeSql(sourceTable)}',
								TableMapping = '{HelperFunctions.EscapeSql(tableMapping)}',
								TargetColumn = '{HelperFunctions.EscapeSql(targetColumn)}',
								Priority = {priority},
								ConditionColumn = '{HelperFunctions.EscapeSql(conditionColumn)}',
								ConditionSQL = '{HelperFunctions.EscapeSql(conditionSql)}',
								TargetValue = '{HelperFunctions.EscapeSql(targetValue)}',
								Description = '{HelperFunctions.EscapeSql(description)}',
								IsActive = {If(isActive, 1, 0)}
							WHERE RuleId = {ruleId}
						"
						BRApi.Database.ExecuteActionQuery(dbConn, updateSql, False, True)
						Return "Success: Rule updated successfully"
					End If
				End Using
				
			Catch ex As Exception
				Return $"Error: {ex.Message}"
			End Try
		End Function

		''' <summary>
		''' Deletes a rule by ID
		''' </summary>
		Private Function DeleteRule(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
			Try
				Dim ruleIdStr As String = Me.GetParam(args, "prm_ETL_Load_Control_Selected_Rule_Id", "0")
				Dim ruleId As Integer = 0
				Integer.TryParse(ruleIdStr, ruleId)
				
				If ruleId <= 0 Then
					Return "Error: Invalid rule ID"
				End If
				
				ETL_RULES.DeleteRule(si, ruleId)
				
				Return "Success: Rule deleted successfully"
				
			Catch ex As Exception
				Return $"Error: {ex.Message}"
			End Try
		End Function
		
		''' <summary>
		''' Toggles rule active state
		''' </summary>
		Private Function ToggleRule(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
			Try
				Dim ruleIdStr As String = Me.GetParam(args, "prm_ETL_Load_Control_Selected_Rule_Id", "0")
				Dim ruleId As Integer = 0
				Integer.TryParse(ruleIdStr, ruleId)
				
				Dim isActiveStr As String = Me.GetParam(args, "prm_ETL_Load_Control_Is_Active", "1")
				Dim isActive As Boolean = (isActiveStr = "1" OrElse isActiveStr.ToLower() = "true")
				
				If ruleId <= 0 Then
					Return "Error: Invalid rule ID"
				End If
				
				ETL_RULES.ToggleRuleActive(si, ruleId, isActive)
				
				Dim status As String = If(isActive, "activated", "deactivated")
				Return $"Success: Rule {status} successfully"
				
			Catch ex As Exception
				Return $"Error: {ex.Message}"
			End Try
		End Function
		
		''' <summary>
		''' Clears form for new rule entry (returns instruction to clear params)
		''' </summary>
		Private Function NewRule(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
			' This action just signals to clear the form
			' The Dashboard should reset parameters on success
			Return "Success: Ready for new rule"
		End Function
		
		''' <summary>
		''' Loads a rule for editing - returns formatted string with rule data
		''' </summary>
		Private Function LoadRule(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As String
			Try
				Dim ruleIdStr As String = Me.GetParam(args, "prm_ETL_Load_Control_Selected_Rule_Id", "0")
				Dim ruleId As Integer = 0
				Integer.TryParse(ruleIdStr, ruleId)
				
				If ruleId <= 0 Then
					Return "Error: Invalid rule ID"
				End If
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT RuleId, RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, 
						       ConditionSQL, TargetValue, Description, IsActive
						FROM XFC_ETL_RULES 
						WHERE RuleId = {ruleId}
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Dim row As DataRow = dt.Rows(0)
						' Return success with data that can be parsed
						Return $"Success: Rule loaded|{row("RuleId")}|{row("SourceTable")}|{row("TableMapping")}|{row("TargetColumn")}|{row("Priority")}|{row("ConditionSQL")}|{row("TargetValue")}|{row("Description")}|{row("IsActive")}"
					Else
						Return "Error: Rule not found"
					End If
				End Using
			Catch ex As Exception
				Return $"Error: {ex.Message}"
			End Try
		End Function
		
		''' <summary>
		''' Loads a rule and populates form fields for editing via ModifiedCustomSubstVars
		''' </summary>
		Private Function EditRule(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs) As Object
			Dim result As New XFSelectionChangedTaskResult()
			result.IsOK = True
			result.ShowMessageBox = True
			
			Try
				' Get rule ID from bound parameter
				Dim ruleIdStr As String = Me.GetParam(args, "p_rule_id", "0")
				If ruleIdStr = "0" OrElse String.IsNullOrEmpty(ruleIdStr) Then
					ruleIdStr = Me.GetParam(args, "prm_ETL_Load_Control_Selected_Rule_Id", "0")
				End If
				
				Dim ruleId As Integer = 0
				Integer.TryParse(ruleIdStr, ruleId)
				
				If ruleId <= 0 Then
					result.IsOK = False
					result.Message = "Error: Please select a rule to edit"
					Return result
				End If
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT RuleId, RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, 
						       ConditionColumn, ConditionSQL, TargetValue, Description, IsActive
						FROM XFC_ETL_RULES 
						WHERE RuleId = {ruleId}
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Dim row As DataRow = dt.Rows(0)
						
						' Populate form fields with rule data
						Dim modVars As New Dictionary(Of String, String)(StringComparer.InvariantCultureIgnoreCase)
						
						' Set Rule ID for UPDATE mode
						modVars.Add("prm_ETL_Load_Control_Selected_Rule_Id", row("RuleId").ToString())
						
						' Set table/column selections
						modVars.Add("prm_ETL_Load_Control_Source_Table", If(IsDBNull(row("SourceTable")), "", row("SourceTable").ToString()))
						modVars.Add("prm_ETL_Load_Control_Target_Table", If(IsDBNull(row("TableMapping")), "", row("TableMapping").ToString()))
						modVars.Add("prm_ETL_Load_Control_Target_Column", If(IsDBNull(row("TargetColumn")), "", row("TargetColumn").ToString()))
						
						' Set condition fields
						Dim conditionColumn As String = If(IsDBNull(row("ConditionColumn")), "", row("ConditionColumn").ToString())
						Dim conditionSql As String = If(IsDBNull(row("ConditionSQL")), "", row("ConditionSQL").ToString())
						
						modVars.Add("prm_ETL_Load_Control_Source_Column", conditionColumn)
						
						' Determine operator and value from ConditionSQL
						Dim operatorValue As String = "="
						Dim conditionalValue As String = ""
						Me.ParseConditionSQL(conditionSql, conditionColumn, operatorValue, conditionalValue)
						
						modVars.Add("prm_ETL_Load_Control_Operator", operatorValue)
						modVars.Add("prm_ETL_Load_Control_Conditional_Value", conditionalValue)
						
						' Set other fields
						modVars.Add("prm_ETL_Load_Control_Target_Value", If(IsDBNull(row("TargetValue")), "", row("TargetValue").ToString()))
						modVars.Add("prm_ETL_Load_Control_Description", If(IsDBNull(row("Description")), "", row("Description").ToString()))
						modVars.Add("prm_ETL_Load_Control_Priority", If(IsDBNull(row("Priority")), "99", row("Priority").ToString()))
						modVars.Add("prm_ETL_Load_Control_Is_Active", If(IsDBNull(row("IsActive")), "1", If(CBool(row("IsActive")), "1", "0")))
						
						result.ChangeCustomSubstVarsInDashboard = True
						result.ModifiedCustomSubstVars = modVars
						result.Message = $"Rule {ruleId} loaded for editing"
						
					Else
						result.IsOK = False
						result.Message = "Error: Rule not found"
					End If
				End Using
				
				Return result
				
			Catch ex As Exception
				Dim errorResult As New XFSelectionChangedTaskResult()
				errorResult.IsOK = False
				errorResult.ShowMessageBox = True
				errorResult.Message = $"Error: {ex.Message}"
				Return errorResult
			End Try
		End Function
		
		''' <summary>
		Private Sub ParseConditionSQL(ByVal conditionSql As String, ByVal columnName As String, ByRef operatorValue As String, ByRef conditionalValue As String)
			Try
				' Handle special cases
				If String.IsNullOrEmpty(conditionSql) Then
					operatorValue = "="
					conditionalValue = ""
					Return
				End If
				
				' Check for catch-all
				If conditionSql.Trim() = "1=1" Then
					operatorValue = "1=1"
					conditionalValue = ""
					Return
				End If
				
				' Check if it's a compound condition (contains AND or OR)
				If conditionSql.ToUpper().Contains(" AND ") OrElse conditionSql.ToUpper().Contains(" OR ") Then
					operatorValue = "CUSTOM"
					conditionalValue = conditionSql
					Return
				End If
				
				' Try to parse simple conditions
				' Format: [column] operator 'value'
				
				' IS NULL / IS NOT NULL
				If conditionSql.ToUpper().Contains("IS NOT NULL") Then
					operatorValue = "IS NOT NULL"
					conditionalValue = ""
					Return
				ElseIf conditionSql.ToUpper().Contains("IS NULL") Then
					operatorValue = "IS NULL"
					conditionalValue = ""
					Return
				End If
				
				' NOT LIKE
				If conditionSql.ToUpper().Contains("NOT LIKE") Then
					operatorValue = "NOT LIKE"
					Dim match As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(conditionSql, "NOT LIKE\s+'%?([^'%]*)%?'", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
					If match.Success Then conditionalValue = match.Groups(1).Value
					Return
				End If
				
				' LIKE
				If conditionSql.ToUpper().Contains(" LIKE ") Then
					operatorValue = "LIKE"
					Dim match As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(conditionSql, "LIKE\s+'%?([^'%]*)%?'", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
					If match.Success Then conditionalValue = match.Groups(1).Value
					Return
				End If
				
				' NOT IN
				If conditionSql.ToUpper().Contains("NOT IN") Then
					operatorValue = "NOT IN"
					Dim match As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(conditionSql, "NOT IN\s*\(([^)]+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
					If match.Success Then
						conditionalValue = match.Groups(1).Value.Replace("'", "").Replace(" ", "")
					End If
					Return
				End If
				
				' IN
				If conditionSql.ToUpper().Contains(" IN ") Then
					operatorValue = "IN"
					Dim match As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(conditionSql, " IN\s*\(([^)]+)\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
					If match.Success Then
						conditionalValue = match.Groups(1).Value.Replace("'", "").Replace(" ", "")
					End If
					Return
				End If
				
				' Standard operators: >=, <=, <>, =, >, <
				Dim operators As String() = {"<>", ">=", "<=", ">", "<", "="}
				For Each op As String In operators
					If conditionSql.Contains(op) Then
						operatorValue = op
						' Extract value between quotes
						Dim valueMatch As System.Text.RegularExpressions.Match = System.Text.RegularExpressions.Regex.Match(conditionSql, $"\{op}\s*'([^']*)'")
						If valueMatch.Success Then
							conditionalValue = valueMatch.Groups(1).Value
						End If
						Return
					End If
				Next
				
				' If we can't parse, treat as CUSTOM
				operatorValue = "CUSTOM"
				conditionalValue = conditionSql
				
			Catch
				operatorValue = "CUSTOM"
				conditionalValue = conditionSql
			End Try
		End Sub
		
		#Region "Helper Methods"
		
		''' <summary>
		''' Builds condition SQL from individual components
		''' For CUSTOM operator, the value field contains the raw SQL condition
		''' </summary>
		Private Function BuildConditionSQL(ByVal column As String, ByVal op As String, ByVal value As String) As String
			' Handle different operator types
			Select Case op.ToUpper()
				Case "CUSTOM", "RAW", "SQL"
					' CUSTOM: Use value directly as raw SQL for compound conditions
					' Example: "[ledger] = '0L' AND [transactionType] = 'REACI'"
					' Note: User is responsible for proper SQL syntax
					Return value
				
				Case "1=1", "ALWAYS", "ELSE", "DEFAULT"
					' Catch-all condition - always true
					Return "1=1"
				
				Case "=", "EQUALS"
					Return $"[{column}] = '{HelperFunctions.EscapeSql(value)}'"
				
				Case "<>", "NOT EQUALS", "NOTEQUALS"
					Return $"[{column}] <> '{HelperFunctions.EscapeSql(value)}'"
				
				Case ">", "GREATER THAN", "GREATERTHAN"
					Return $"[{column}] > '{HelperFunctions.EscapeSql(value)}'"
				
				Case ">=", "GREATER OR EQUAL", "GREATEROREQUAL"
					Return $"[{column}] >= '{HelperFunctions.EscapeSql(value)}'"
				
				Case "<", "LESS THAN", "LESSTHAN"
					Return $"[{column}] < '{HelperFunctions.EscapeSql(value)}'"
				
				Case "<=", "LESS OR EQUAL", "LESSOREQUAL"
					Return $"[{column}] <= '{HelperFunctions.EscapeSql(value)}'"
				
				Case "LIKE", "CONTAINS"
					Return $"[{column}] LIKE '%{HelperFunctions.EscapeSql(value)}%'"
				
				Case "NOT LIKE", "NOT CONTAINS", "NOTLIKE"
					Return $"[{column}] NOT LIKE '%{HelperFunctions.EscapeSql(value)}%'"
				
				Case "IN", "IN LIST", "INLIST"
					' Value should be comma-separated
					Dim values As String() = value.Split(","c)
					Dim quotedValues As String = String.Join(",", values.Select(Function(v) $"'{HelperFunctions.EscapeSql(v.Trim())}'"))
					Return $"[{column}] IN ({quotedValues})"
				
				Case "NOT IN", "NOT IN LIST", "NOTINLIST"
					Dim values2 As String() = value.Split(","c)
					Dim quotedValues2 As String = String.Join(",", values2.Select(Function(v) $"'{HelperFunctions.EscapeSql(v.Trim())}'"))
					Return $"[{column}] NOT IN ({quotedValues2})"
				
				Case "IS NULL", "ISNULL"
					Return $"[{column}] IS NULL"
				
				Case "IS NOT NULL", "ISNOTNULL"
					Return $"[{column}] IS NOT NULL"
				
				Case Else
					' Default to equals
					Return $"[{column}] = '{HelperFunctions.EscapeSql(value)}'"
			End Select
		End Function
		
		#End Region
		
	End Class
End Namespace