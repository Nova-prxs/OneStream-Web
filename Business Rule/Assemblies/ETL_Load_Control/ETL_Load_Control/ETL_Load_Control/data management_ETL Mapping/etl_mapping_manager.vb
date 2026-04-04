Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.etl_mapping_manager
	
	''' <summary>
	''' Dashboard Data Adapter for ETL Mapping Manager UI.
	''' Provides CRUD operations and data retrieval for managing ETL transformation rules.
	''' </summary>
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						Dim names As New List(Of String)()
						names.Add("GetAllRules")
						names.Add("GetFilteredRules")
						names.Add("GetRulesByGroup")
						names.Add("GetRuleById")
						names.Add("LoadRuleForEdit")
						names.Add("SaveRule")
						names.Add("SaveRuleFromParams")
						names.Add("DeleteRule")
						names.Add("ToggleRule")
						names.Add("PreviewCaseLogic")
						names.Add("ValidateCondition")
						Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						
						'-------------------------------------------------------------------
						' GetAllRules: Returns all ETL rules for the management grid
						' Parameters: p_include_inactive - Include inactive rules (default: true)
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetAllRules") Then
							Dim includeInactive As Boolean = args.NameValuePairs.XFGetValue("p_include_inactive", "true").XFEqualsIgnoreCase("true")
							Return Me.GetAllRulesDataTable(si, includeInactive)
						End If
						
						'-------------------------------------------------------------------
						' GetFilteredRules: Returns rules filtered by source/target table
						' Parameters: p_source_table, p_target_table, p_include_inactive
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetFilteredRules") Then
							Dim sourceTable As String = args.NameValuePairs.XFGetValue("p_source_table", "")
							Dim targetTable As String = args.NameValuePairs.XFGetValue("p_target_table", "")
							Dim includeInactive As Boolean = args.NameValuePairs.XFGetValue("p_include_inactive", "true").XFEqualsIgnoreCase("true")
							Return Me.GetFilteredRulesDataTable(si, sourceTable, targetTable, includeInactive)
						End If
						
						'-------------------------------------------------------------------
						' GetRulesByGroup: Returns rules for a specific group
						' Parameters: p_rule_group - RuleGroup name
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetRulesByGroup") Then
							Dim ruleGroup As String = args.NameValuePairs.XFGetValue("p_rule_group", "")
							Return Me.GetRulesByGroupDataTable(si, ruleGroup)
						End If
						
						'-------------------------------------------------------------------
						' GetRuleById: Returns a single rule by ID
						' Parameters: p_rule_id - Rule ID
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("GetRuleById") Then
							Dim ruleId As Integer = CInt(args.NameValuePairs.XFGetValue("p_rule_id", "0"))
							Return Me.GetRuleByIdDataTable(si, ruleId)
						End If
						
						'-------------------------------------------------------------------
						' LoadRuleForEdit: Returns rule data parsed for form fields
						' Parameters: p_rule_id - Rule ID
						' Returns: Parsed condition into column/operator/value for form binding
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("LoadRuleForEdit") Then
							Dim ruleId As Integer = CInt(args.NameValuePairs.XFGetValue("p_rule_id", "0"))
							Return Me.LoadRuleForEditDataTable(si, ruleId)
						End If
						
						'-------------------------------------------------------------------
						' SaveRule: Creates or updates a rule (raw condition SQL)
						' Parameters: p_rule_id, p_source_table, p_table_mapping, 
						'             p_target_column, p_priority, p_condition_sql,
						'             p_target_value, p_description, p_is_active
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("SaveRule") Then
							Dim ruleId As Integer = CInt(args.NameValuePairs.XFGetValue("p_rule_id", "0"))
							Dim sourceTable As String = args.NameValuePairs.XFGetValue("p_source_table", "")
							Dim tableMapping As String = args.NameValuePairs.XFGetValue("p_table_mapping", "")
							Dim targetColumn As String = args.NameValuePairs.XFGetValue("p_target_column", "")
							Dim priority As Integer = CInt(args.NameValuePairs.XFGetValue("p_priority", "99"))
							Dim conditionSql As String = args.NameValuePairs.XFGetValue("p_condition_sql", "1=1")
							Dim targetValue As String = args.NameValuePairs.XFGetValue("p_target_value", "")
							Dim description As String = args.NameValuePairs.XFGetValue("p_description", "")
							Dim isActive As Boolean = args.NameValuePairs.XFGetValue("p_is_active", "true").XFEqualsIgnoreCase("true")
							Return Me.SaveRule(si, ruleId, sourceTable, tableMapping, targetColumn, priority, conditionSql, targetValue, description, isActive)
						End If
						
						'-------------------------------------------------------------------
						' SaveRuleFromParams: Creates/updates rule building SQL from params
						' Parameters: p_rule_id, p_source_table, p_table_mapping,
						'             p_target_column, p_priority, p_condition_column,
						'             p_condition_operator, p_condition_value,
						'             p_target_value, p_description, p_is_active
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("SaveRuleFromParams") Then
							Dim ruleId As Integer = CInt(args.NameValuePairs.XFGetValue("p_rule_id", "0"))
							Dim sourceTable As String = args.NameValuePairs.XFGetValue("p_source_table", "")
							Dim tableMapping As String = args.NameValuePairs.XFGetValue("p_table_mapping", "")
							Dim targetColumn As String = args.NameValuePairs.XFGetValue("p_target_column", "")
							Dim priority As Integer = CInt(args.NameValuePairs.XFGetValue("p_priority", "99"))
							Dim conditionColumn As String = args.NameValuePairs.XFGetValue("p_condition_column", "")
							Dim conditionOperator As String = args.NameValuePairs.XFGetValue("p_condition_operator", "Equals")
							Dim conditionValue As String = args.NameValuePairs.XFGetValue("p_condition_value", "")
							Dim targetValue As String = args.NameValuePairs.XFGetValue("p_target_value", "")
							Dim description As String = args.NameValuePairs.XFGetValue("p_description", "")
							Dim isActive As Boolean = args.NameValuePairs.XFGetValue("p_is_active", "true").XFEqualsIgnoreCase("true")
							Return Me.SaveRuleFromParams(si, ruleId, sourceTable, tableMapping, targetColumn, priority, conditionColumn, conditionOperator, conditionValue, targetValue, description, isActive)
						End If
						
						'-------------------------------------------------------------------
						' DeleteRule: Deletes a rule by ID
						' Parameters: p_rule_id - Rule ID
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("DeleteRule") Then
							Dim ruleId As Integer = CInt(args.NameValuePairs.XFGetValue("p_rule_id", "0"))
							Return Me.DeleteRule(si, ruleId)
						End If
						
						'-------------------------------------------------------------------
						' ToggleRule: Activates/deactivates a rule
						' Parameters: p_rule_id - Rule ID, p_is_active - Active state
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("ToggleRule") Then
							Dim ruleId As Integer = CInt(args.NameValuePairs.XFGetValue("p_rule_id", "0"))
							Dim isActive As Boolean = args.NameValuePairs.XFGetValue("p_is_active", "true").XFEqualsIgnoreCase("true")
							Return Me.ToggleRule(si, ruleId, isActive)
						End If
						
						'-------------------------------------------------------------------
						' PreviewCaseLogic: Generates CASE statement preview
						' Parameters: p_rule_group, p_table_mapping, p_target_column, p_default_value
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("PreviewCaseLogic") Then
							Dim ruleGroup As String = args.NameValuePairs.XFGetValue("p_rule_group", "")
							Dim tableMapping As String = args.NameValuePairs.XFGetValue("p_table_mapping", "")
							Dim targetColumn As String = args.NameValuePairs.XFGetValue("p_target_column", "")
							Dim defaultValue As String = args.NameValuePairs.XFGetValue("p_default_value", "")
							Return Me.PreviewCaseLogicDataTable(si, ruleGroup, tableMapping, targetColumn, defaultValue)
						End If
						
						'-------------------------------------------------------------------
						' ValidateCondition: Validates a condition SQL
						' Parameters: p_source_table, p_condition_sql
						'-------------------------------------------------------------------
						If args.DataSetName.XFEqualsIgnoreCase("ValidateCondition") Then
							Dim sourceTable As String = args.NameValuePairs.XFGetValue("p_source_table", "")
							Dim conditionSql As String = args.NameValuePairs.XFGetValue("p_condition_sql", "")
							Return Me.ValidateConditionDataTable(si, sourceTable, conditionSql)
						End If
					
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Rule Retrieval"
		
		''' <summary>
		''' Gets all rules as DataTable
		''' </summary>
		Private Function GetAllRulesDataTable(ByVal si As SessionInfo, ByVal includeInactive As Boolean) As DataTable
			Dim dt As New DataTable("AllRules")
			dt.Columns.Add("RuleId", GetType(Integer))
			dt.Columns.Add("RuleGroup", GetType(String))
			dt.Columns.Add("SourceTable", GetType(String))
			dt.Columns.Add("TableMapping", GetType(String))
			dt.Columns.Add("TargetColumn", GetType(String))
			dt.Columns.Add("Priority", GetType(Integer))
			dt.Columns.Add("ConditionSQL", GetType(String))
			dt.Columns.Add("ConditionDescription", GetType(String))
			dt.Columns.Add("TargetValue", GetType(String))
			dt.Columns.Add("Description", GetType(String))
			dt.Columns.Add("IsActive", GetType(Boolean))
			
			Dim rules As List(Of ETL_RULES.EtlRuleConfig) = ETL_RULES.GetAllRules(si, includeInactive)
			For Each rule As ETL_RULES.EtlRuleConfig In rules
				' Parse condition for human-readable description
				Dim parsedCondition As CONDITION_BUILDER.RuleCondition = CONDITION_BUILDER.ParseConditionSQL(rule.ConditionSQL)
				Dim condDescription As String = CONDITION_BUILDER.GetRuleConditionDescription(parsedCondition)
				
				dt.Rows.Add(
					rule.RuleId,
					rule.RuleGroup,
					rule.SourceTable,
					rule.TableMapping,
					rule.TargetColumn,
					rule.Priority,
					rule.ConditionSQL,
					condDescription,
					rule.TargetValue,
					rule.Description,
					rule.IsActive
				)
			Next
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets rules by group as DataTable
		''' </summary>
		Private Function GetRulesByGroupDataTable(ByVal si As SessionInfo, ByVal ruleGroup As String) As DataTable
			Dim dt As New DataTable("RulesByGroup")
			dt.Columns.Add("RuleId", GetType(Integer))
			dt.Columns.Add("RuleGroup", GetType(String))
			dt.Columns.Add("SourceTable", GetType(String))
			dt.Columns.Add("TableMapping", GetType(String))
			dt.Columns.Add("TargetColumn", GetType(String))
			dt.Columns.Add("Priority", GetType(Integer))
			dt.Columns.Add("ConditionSQL", GetType(String))
			dt.Columns.Add("ConditionDescription", GetType(String))
			dt.Columns.Add("TargetValue", GetType(String))
			dt.Columns.Add("Description", GetType(String))
			dt.Columns.Add("IsActive", GetType(Boolean))
			
			If Not String.IsNullOrEmpty(ruleGroup) Then
				Dim rules As List(Of ETL_RULES.EtlRuleConfig) = ETL_RULES.GetRulesByGroup(si, ruleGroup)
				For Each rule As ETL_RULES.EtlRuleConfig In rules
					Dim parsedCondition As CONDITION_BUILDER.RuleCondition = CONDITION_BUILDER.ParseConditionSQL(rule.ConditionSQL)
					Dim condDescription As String = CONDITION_BUILDER.GetRuleConditionDescription(parsedCondition)
					
					dt.Rows.Add(
						rule.RuleId,
						rule.RuleGroup,
						rule.SourceTable,
						rule.TableMapping,
						rule.TargetColumn,
						rule.Priority,
						rule.ConditionSQL,
						condDescription,
						rule.TargetValue,
						rule.Description,
						rule.IsActive
					)
				Next
			End If
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets rules filtered by source and/or target table
		''' Columns ordered for GridView display: SourceTable, TableMapping, TargetColumn, ConditionColumn, ConditionSQL, TargetValue, Priority, IsActive
		''' </summary>
		Private Function GetFilteredRulesDataTable(ByVal si As SessionInfo, ByVal sourceTable As String, ByVal targetTable As String, ByVal includeInactive As Boolean) As DataTable
			Dim dt As New DataTable("FilteredRules")
			' Ordered columns for GridView display
			dt.Columns.Add("RuleId", GetType(Integer))
			dt.Columns.Add("SourceTable", GetType(String))
			dt.Columns.Add("TableMapping", GetType(String))
			dt.Columns.Add("TargetColumn", GetType(String))
			dt.Columns.Add("ConditionColumn", GetType(String))
			dt.Columns.Add("ConditionSQL", GetType(String))
			dt.Columns.Add("TargetValue", GetType(String))
			dt.Columns.Add("Priority", GetType(Integer))
			dt.Columns.Add("IsActive", GetType(Boolean))
			dt.Columns.Add("Description", GetType(String))
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim filters As New List(Of String)()
					
					If Not String.IsNullOrEmpty(sourceTable) Then
						filters.Add($"SourceTable = '{HelperFunctions.EscapeSql(sourceTable)}'")
					End If
					
					If Not String.IsNullOrEmpty(targetTable) Then
						filters.Add($"TableMapping = '{HelperFunctions.EscapeSql(targetTable)}'")
					End If
					
					If Not includeInactive Then
						filters.Add("IsActive = 1")
					End If
					
					Dim whereClause As String = If(filters.Count > 0, "WHERE " & String.Join(" AND ", filters), "")
					
					Dim query As String = $"
						SELECT RuleId, SourceTable, TableMapping, TargetColumn, ConditionColumn, ConditionSQL, TargetValue, Priority, IsActive, Description
						FROM XFC_ETL_RULES
						{whereClause}
						ORDER BY SourceTable, TableMapping, TargetColumn, Priority
					"
					Dim resultDt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If resultDt IsNot Nothing Then
						For Each row As DataRow In resultDt.Rows
							dt.Rows.Add(
								CInt(row("RuleId")),
								If(IsDBNull(row("SourceTable")), "", row("SourceTable").ToString()),
								If(IsDBNull(row("TableMapping")), "", row("TableMapping").ToString()),
								If(IsDBNull(row("TargetColumn")), "", row("TargetColumn").ToString()),
								If(IsDBNull(row("ConditionColumn")), "", row("ConditionColumn").ToString()),
								If(IsDBNull(row("ConditionSQL")), "", row("ConditionSQL").ToString()),
								If(IsDBNull(row("TargetValue")), "", row("TargetValue").ToString()),
								If(IsDBNull(row("Priority")), 99, CInt(row("Priority"))),
								Not IsDBNull(row("IsActive")) AndAlso CBool(row("IsActive")),
								If(IsDBNull(row("Description")), "", row("Description").ToString())
							)
						Next
					End If
				End Using
			Catch ex As Exception
			End Try
			
			Return dt
		End Function
		
		''' <summary>
		''' Loads a rule for editing - returns parsed condition components for form binding
		''' </summary>
		Private Function LoadRuleForEditDataTable(ByVal si As SessionInfo, ByVal ruleId As Integer) As DataTable
			Dim dt As New DataTable("RuleForEdit")
			dt.Columns.Add("RuleId", GetType(Integer))
			dt.Columns.Add("SourceTable", GetType(String))
			dt.Columns.Add("TableMapping", GetType(String))
			dt.Columns.Add("TargetColumn", GetType(String))
			dt.Columns.Add("Priority", GetType(Integer))
			dt.Columns.Add("ConditionColumn", GetType(String))
			dt.Columns.Add("ConditionOperator", GetType(String))
			dt.Columns.Add("ConditionValue", GetType(String))
			dt.Columns.Add("TargetValue", GetType(String))
			dt.Columns.Add("Description", GetType(String))
			dt.Columns.Add("IsActive", GetType(Boolean))
			dt.Columns.Add("ConditionDescription", GetType(String))
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT RuleId, SourceTable, TableMapping, TargetColumn, Priority, ConditionSQL, TargetValue, Description, IsActive
						FROM XFC_ETL_RULES
						WHERE RuleId = {ruleId}
					"
					Dim resultDt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If resultDt IsNot Nothing AndAlso resultDt.Rows.Count > 0 Then
						Dim row As DataRow = resultDt.Rows(0)
						Dim condSql As String = If(IsDBNull(row("ConditionSQL")), "", row("ConditionSQL").ToString())
						
						' Parse condition SQL into components
						Dim parsedCondition As CONDITION_BUILDER.RuleCondition = CONDITION_BUILDER.ParseConditionSQL(condSql)
						Dim condDescription As String = CONDITION_BUILDER.GetRuleConditionDescription(parsedCondition)
						
						' Extract first single condition for simple form binding
						Dim condColumn As String = ""
						Dim condOperator As String = ""
						Dim condValue As String = ""
						
						' RuleCondition has Groups property - get first condition from first group
						If parsedCondition.Groups IsNot Nothing AndAlso parsedCondition.Groups.Count > 0 Then
							Dim firstGroup As CONDITION_BUILDER.ConditionGroup = parsedCondition.Groups(0)
							If firstGroup.Conditions IsNot Nothing AndAlso firstGroup.Conditions.Count > 0 Then
								Dim firstCond As CONDITION_BUILDER.SingleCondition = firstGroup.Conditions(0)
								condColumn = firstCond.ColumnName
								condOperator = firstCond.CompareOp.ToString()
								condValue = firstCond.Value
							End If
						End If
						
						dt.Rows.Add(
							CInt(row("RuleId")),
							If(IsDBNull(row("SourceTable")), "", row("SourceTable").ToString()),
							If(IsDBNull(row("TableMapping")), "", row("TableMapping").ToString()),
							If(IsDBNull(row("TargetColumn")), "", row("TargetColumn").ToString()),
							If(IsDBNull(row("Priority")), 99, CInt(row("Priority"))),
							condColumn,
							condOperator,
							condValue,
							If(IsDBNull(row("TargetValue")), "", row("TargetValue").ToString()),
							If(IsDBNull(row("Description")), "", row("Description").ToString()),
							Not IsDBNull(row("IsActive")) AndAlso CBool(row("IsActive")),
							condDescription
						)
					End If
				End Using
			Catch ex As Exception
			End Try
			
			Return dt
		End Function
		
		''' <summary>
		''' Saves rule from individual parameters (builds SQL from components)
		''' </summary>
		Private Function SaveRuleFromParams(ByVal si As SessionInfo, ByVal ruleId As Integer, ByVal sourceTable As String, ByVal tableMapping As String, ByVal targetColumn As String, ByVal priority As Integer, ByVal conditionColumn As String, ByVal conditionOperator As String, ByVal conditionValue As String, ByVal targetValue As String, ByVal description As String, ByVal isActive As Boolean) As DataTable
			Dim dt As New DataTable("SaveResult")
			dt.Columns.Add("Success", GetType(Boolean))
			dt.Columns.Add("Message", GetType(String))
			dt.Columns.Add("RuleId", GetType(Integer))
			
			Try
				' Validate required fields
				If String.IsNullOrEmpty(sourceTable) Then
					dt.Rows.Add(False, "Source table is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(tableMapping) Then
					dt.Rows.Add(False, "Target table is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(targetColumn) Then
					dt.Rows.Add(False, "Target column is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(targetValue) Then
					dt.Rows.Add(False, "Target value is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(conditionColumn) Then
					dt.Rows.Add(False, "Condition column is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(conditionOperator) Then
					dt.Rows.Add(False, "Condition operator is required", 0)
					Return dt
				End If
				
				' Build condition SQL from individual components
				Dim singleCondition As New CONDITION_BUILDER.SingleCondition() With {
					.ColumnName = conditionColumn,
					.CompareOp = ParseOperator(conditionOperator),
					.Value = conditionValue
				}
				
				' RuleCondition uses Groups - create a group with single condition
				Dim conditionGroup As New CONDITION_BUILDER.ConditionGroup()
				conditionGroup.Conditions.Add(singleCondition)
				
				Dim ruleCondition As New CONDITION_BUILDER.RuleCondition()
				ruleCondition.Groups.Add(conditionGroup)
				
				Dim conditionSql As String = CONDITION_BUILDER.BuildConditionSQL(ruleCondition)
				
				' Validate condition against source table columns
				Dim columns As List(Of String) = ETL_METADATA.GetColumnNames(si, sourceTable)
				Dim errors As List(Of String) = CONDITION_BUILDER.ValidateRuleCondition(ruleCondition, columns)
				
				If errors.Count > 0 Then
					dt.Rows.Add(False, "Validation errors: " & String.Join("; ", errors), 0)
					Return dt
				End If
				
				' Use the existing SaveRule method
				Return SaveRule(si, ruleId, sourceTable, tableMapping, targetColumn, priority, conditionSql, targetValue, description, isActive)
				
			Catch ex As Exception
				dt.Rows.Add(False, $"Error saving rule: {ex.Message}", 0)
			End Try
			
			Return dt
		End Function
		
		''' <summary>
		''' Gets single rule by ID
		''' </summary>
		Private Function GetRuleByIdDataTable(ByVal si As SessionInfo, ByVal ruleId As Integer) As DataTable
			Dim dt As New DataTable("RuleById")
			dt.Columns.Add("RuleId", GetType(Integer))
			dt.Columns.Add("RuleGroup", GetType(String))
			dt.Columns.Add("SourceTable", GetType(String))
			dt.Columns.Add("TableMapping", GetType(String))
			dt.Columns.Add("TargetColumn", GetType(String))
			dt.Columns.Add("Priority", GetType(Integer))
			dt.Columns.Add("ConditionSQL", GetType(String))
			dt.Columns.Add("TargetValue", GetType(String))
			dt.Columns.Add("Description", GetType(String))
			dt.Columns.Add("IsActive", GetType(Boolean))
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT RuleId, RuleGroup, SourceTable, TableMapping, TargetColumn, Priority, ConditionSQL, TargetValue, Description, IsActive
						FROM XFC_ETL_RULES
						WHERE RuleId = {ruleId}
					"
					Dim resultDt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If resultDt IsNot Nothing AndAlso resultDt.Rows.Count > 0 Then
						Dim row As DataRow = resultDt.Rows(0)
						dt.Rows.Add(
							CInt(row("RuleId")),
							If(IsDBNull(row("RuleGroup")), "", row("RuleGroup").ToString()),
							If(IsDBNull(row("SourceTable")), "", row("SourceTable").ToString()),
							If(IsDBNull(row("TableMapping")), "", row("TableMapping").ToString()),
							If(IsDBNull(row("TargetColumn")), "", row("TargetColumn").ToString()),
							If(IsDBNull(row("Priority")), 99, CInt(row("Priority"))),
							If(IsDBNull(row("ConditionSQL")), "", row("ConditionSQL").ToString()),
							If(IsDBNull(row("TargetValue")), "", row("TargetValue").ToString()),
							If(IsDBNull(row("Description")), "", row("Description").ToString()),
							Not IsDBNull(row("IsActive")) AndAlso CBool(row("IsActive"))
						)
					End If
				End Using
			Catch ex As Exception
			End Try
			
			Return dt
		End Function
		
		#End Region
		
		#Region "Rule Operations"
		
		''' <summary>
		''' Saves (creates or updates) a rule
		''' </summary>
		Private Function SaveRule(ByVal si As SessionInfo, ByVal ruleId As Integer, ByVal sourceTable As String, ByVal tableMapping As String, ByVal targetColumn As String, ByVal priority As Integer, ByVal conditionSql As String, ByVal targetValue As String, ByVal description As String, ByVal isActive As Boolean) As DataTable
			Dim dt As New DataTable("SaveResult")
			dt.Columns.Add("Success", GetType(Boolean))
			dt.Columns.Add("Message", GetType(String))
			dt.Columns.Add("RuleId", GetType(Integer))
			
			Try
				If String.IsNullOrEmpty(sourceTable) Then
					dt.Rows.Add(False, "Source table is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(tableMapping) Then
					dt.Rows.Add(False, "Target table is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(targetColumn) Then
					dt.Rows.Add(False, "Target column is required", 0)
					Return dt
				End If
				
				If String.IsNullOrEmpty(targetValue) Then
					dt.Rows.Add(False, "Target value is required", 0)
					Return dt
				End If
				
				' Validate tables exist
				If Not ETL_METADATA.TableExists(si, sourceTable) Then
					dt.Rows.Add(False, $"Source table '{sourceTable}' does not exist", 0)
					Return dt
				End If
				
				If Not ETL_METADATA.TableExists(si, tableMapping) Then
					dt.Rows.Add(False, $"Target table '{tableMapping}' does not exist", 0)
					Return dt
				End If
				
				' Validate target column exists in target table
				If Not ETL_METADATA.ColumnExists(si, tableMapping, targetColumn) Then
					dt.Rows.Add(False, $"Column '{targetColumn}' does not exist in table '{tableMapping}'", 0)
					Return dt
				End If
				
				If ruleId = 0 Then
					' Create new rule
					Dim newId As Integer = ETL_RULES.InsertRule(
						si, sourceTable, tableMapping, targetColumn,
						priority, conditionSql, targetValue, description, isActive
					)
					
					If newId > 0 Then
						dt.Rows.Add(True, "Rule created successfully", newId)
					Else
						dt.Rows.Add(False, "Failed to create rule", 0)
					End If
				Else
					' Update existing rule
					ETL_RULES.UpdateRule(si, ruleId, priority, conditionSql, targetValue, description, isActive)
					dt.Rows.Add(True, "Rule updated successfully", ruleId)
				End If
				
			Catch ex As Exception
				dt.Rows.Add(False, $"Error saving rule: {ex.Message}", 0)
			End Try
			
			Return dt
		End Function
		
		''' <summary>
		''' Deletes a rule
		''' </summary>
		Private Function DeleteRule(ByVal si As SessionInfo, ByVal ruleId As Integer) As DataTable
			Dim dt As New DataTable("DeleteResult")
			dt.Columns.Add("Success", GetType(Boolean))
			dt.Columns.Add("Message", GetType(String))
			
			Try
				If ruleId <= 0 Then
					dt.Rows.Add(False, "Invalid rule ID")
					Return dt
				End If
				
				ETL_RULES.DeleteRule(si, ruleId)
				dt.Rows.Add(True, "Rule deleted successfully")
				
			Catch ex As Exception
				dt.Rows.Add(False, $"Error deleting rule: {ex.Message}")
			End Try
			
			Return dt
		End Function
		
		''' <summary>
		''' Toggles rule active state
		''' </summary>
		Private Function ToggleRule(ByVal si As SessionInfo, ByVal ruleId As Integer, ByVal isActive As Boolean) As DataTable
			Dim dt As New DataTable("ToggleResult")
			dt.Columns.Add("Success", GetType(Boolean))
			dt.Columns.Add("Message", GetType(String))
			
			Try
				If ruleId <= 0 Then
					dt.Rows.Add(False, "Invalid rule ID")
					Return dt
				End If
				
				ETL_RULES.ToggleRuleActive(si, ruleId, isActive)
				Dim status As String = If(isActive, "activated", "deactivated")
				dt.Rows.Add(True, $"Rule {status} successfully")
				
			Catch ex As Exception
				dt.Rows.Add(False, $"Error toggling rule: {ex.Message}")
			End Try
			
			Return dt
		End Function
		
		#End Region
		
		#Region "Preview and Validation"
		
		''' <summary>
		''' Generates CASE logic preview
		''' </summary>
		Private Function PreviewCaseLogicDataTable(ByVal si As SessionInfo, ByVal ruleGroup As String, ByVal tableMapping As String, ByVal targetColumn As String, ByVal defaultValue As String) As DataTable
			Dim dt As New DataTable("CasePreview")
			dt.Columns.Add("CaseLogic", GetType(String))
			
			Try
				If String.IsNullOrEmpty(ruleGroup) AndAlso Not String.IsNullOrEmpty(tableMapping) AndAlso Not String.IsNullOrEmpty(targetColumn) Then
					' Generate rule group from table/column
					ruleGroup = ETL_RULES.GenerateRuleGroup("", tableMapping, targetColumn)
				End If
				
				If String.IsNullOrEmpty(ruleGroup) Then
					dt.Rows.Add("-- No rule group specified --")
					Return dt
				End If
				
				If String.IsNullOrEmpty(defaultValue) Then
					defaultValue = "DefaultValue"
				End If
				
				Dim caseLogic As String = ETL_RULES.BuildDynamicCase(si, ruleGroup, defaultValue, tableMapping, targetColumn)
				dt.Rows.Add(caseLogic)
				
			Catch ex As Exception
				dt.Rows.Add($"-- Error generating preview: {ex.Message} --")
			End Try
			
			Return dt
		End Function
		
		''' <summary>
		''' Validates a condition SQL against source table columns
		''' </summary>
		Private Function ValidateConditionDataTable(ByVal si As SessionInfo, ByVal sourceTable As String, ByVal conditionSql As String) As DataTable
			Dim dt As New DataTable("ValidationResult")
			dt.Columns.Add("IsValid", GetType(Boolean))
			dt.Columns.Add("Message", GetType(String))
			dt.Columns.Add("ParsedDescription", GetType(String))
			
			Try
				If String.IsNullOrEmpty(sourceTable) Then
					dt.Rows.Add(False, "Source table is required for validation", "")
					Return dt
				End If
				
				If String.IsNullOrEmpty(conditionSql) Then
					dt.Rows.Add(False, "Condition SQL is required", "")
					Return dt
				End If
				
				' Get available columns
				Dim columns As List(Of String) = ETL_METADATA.GetColumnNames(si, sourceTable)
				
				If columns.Count = 0 Then
					dt.Rows.Add(False, $"Could not retrieve columns for table '{sourceTable}'", "")
					Return dt
				End If
				
				' Parse and validate condition
				Dim parsedCondition As CONDITION_BUILDER.RuleCondition = CONDITION_BUILDER.ParseConditionSQL(conditionSql)
				Dim errors As List(Of String) = CONDITION_BUILDER.ValidateRuleCondition(parsedCondition, columns)
				
				If errors.Count > 0 Then
					dt.Rows.Add(False, String.Join("; ", errors), "")
				Else
					Dim description As String = CONDITION_BUILDER.GetRuleConditionDescription(parsedCondition)
					dt.Rows.Add(True, "Condition is valid", description)
				End If
				
			Catch ex As Exception
				dt.Rows.Add(False, $"Validation error: {ex.Message}", "")
			End Try
			
			Return dt
		End Function
		
		#End Region
		
		#Region "Helper Methods"
		
		''' <summary>
		''' Parses operator string to ComparisonOperator enum
		''' </summary>
		Private Function ParseOperator(ByVal operatorStr As String) As CONDITION_BUILDER.ComparisonOperator
			If String.IsNullOrEmpty(operatorStr) Then
				Return CONDITION_BUILDER.ComparisonOperator.Equals
			End If
			
			Select Case operatorStr.ToLower()
				Case "equals", "="
					Return CONDITION_BUILDER.ComparisonOperator.Equals
				Case "notequals", "<>"
					Return CONDITION_BUILDER.ComparisonOperator.NotEquals
				Case "greaterthan", ">"
					Return CONDITION_BUILDER.ComparisonOperator.GreaterThan
				Case "lessthan", "<"
					Return CONDITION_BUILDER.ComparisonOperator.LessThan
				Case "greaterorequal", ">="
					Return CONDITION_BUILDER.ComparisonOperator.GreaterOrEqual
				Case "lessorequal", "<="
					Return CONDITION_BUILDER.ComparisonOperator.LessOrEqual
				Case "contains", "like"
					Return CONDITION_BUILDER.ComparisonOperator.Contains
				Case "startswith"
					Return CONDITION_BUILDER.ComparisonOperator.StartsWith
				Case "endswith"
					Return CONDITION_BUILDER.ComparisonOperator.EndsWith
				Case "isnull"
					Return CONDITION_BUILDER.ComparisonOperator.IsNull
				Case "isnotnull"
					Return CONDITION_BUILDER.ComparisonOperator.IsNotNull
				Case "inlist", "in"
					Return CONDITION_BUILDER.ComparisonOperator.InList
				Case Else
					' Try parsing as enum directly
					Dim result As CONDITION_BUILDER.ComparisonOperator
					If [Enum].TryParse(Of CONDITION_BUILDER.ComparisonOperator)(operatorStr, True, result) Then
						Return result
					End If
					Return CONDITION_BUILDER.ComparisonOperator.Equals
			End Select
		End Function
		
		#End Region
		
	End Class
End Namespace
