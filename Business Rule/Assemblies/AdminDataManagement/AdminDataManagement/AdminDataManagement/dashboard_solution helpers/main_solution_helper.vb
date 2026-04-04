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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_solution_helper
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement Load Dashboard logic here.
							
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
								loadDashboardTaskResult.ModifiedCustomSubstVars = Nothing
								Return loadDashboardTaskResult
								End If
						End If
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						#Region "Check Complete PnL Workspace"
						
						If args.FunctionName.XFEqualsIgnoreCase("CheckCompletePnLWorkspace") Then
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult
							selectionChangedTaskResult.IsOK = True
								
							' Get views
							Dim paramCheckViews As String = args.NameValuePairs.XFGetValue("p_check_views")
							If String.IsNullOrEmpty(paramCheckViews) Then Return Nothing
							Dim checkViewArray = paramCheckViews.Split(",")
							
							' Get wf info
							Dim paramWFParentProfileKey As String = args.NameValuePairs.XFGetValue("p_wf_parent_profile_key").Trim("'")
							Dim entityBeginning As String = BRApi.Workflow.Metadata.GetProfile(si, New Guid(paramWFParentProfileKey)).Description
							Dim paramWFTime As String = args.NameValuePairs.XFGetValue("p_wf_time")
							Dim paramYear As Integer = Left(paramWFTime, 4)
							Dim paramMonth As Integer = paramWFTime.Split("M")(1)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								
								' Check if there are any transformations not found
								For Each checkView In checkViewArray
									Try
										Dim dt = BRApi.Database.ExecuteSql(
											dbConn,
											$"
											WITH company AS (
												SELECT TOP 1 company_id
												FROM XFC_MAIN_MASTER_ProfitCenters WITH(NOLOCK)
												WHERE LEFT(entity_id, 5) = @entityBeginning
											)
											
											SELECT v.*
											FROM {checkView.Trim()} v WITH(NOLOCK)
											JOIN company c ON v.company_id = c.company_id;
											",
											New List(Of DbParamInfo) From {
												New DbParamInfo("entityBeginning", entityBeginning)
											},
											False
										)
										If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
											selectionChangedTaskResult.IsOK = False
											selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
											selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
											selectionChangedTaskResult.ShowMessageBox = True
											selectionChangedTaskResult.Message = "Transformations not found." & vbCrLf &
												"Please, check and finish the mappings."
											Return selectionChangedTaskResult
										End If
									Catch ex As Exception
										Dim errorString As String = ex.ToString
										If errorString.Contains("Invalid object name") Then errorString = $"Check view '{checkView}' does not exist"
										selectionChangedTaskResult.IsOK = False
										selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
										selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
										selectionChangedTaskResult.ShowMessageBox = True
										selectionChangedTaskResult.Message = $"Error getting check view: {errorString}" & vbCrLf &
											"Please, talk with an administrator."
										Return selectionChangedTaskResult
									End Try
								Next
								
								' Get the company id for the profit center
								Dim companyDt = BRApi.Database.ExecuteSql(
									dbConn,
									$"
										SELECT TOP 1 company_id
										FROM XFC_MAIN_MASTER_ProfitCenters
										WHERE entity_id LIKE '{entityBeginning}%';
									",
									False
								)
								
								If companyDt Is Nothing OrElse companyDt.Rows.Count = 0 Then
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = True
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"No company found for entity id: {entityBeginning}"
									Return selectionChangedTaskResult
								End If
								
								Dim dbParamInfos As New List(Of DbParamInfo) From {
									New DbParamInfo("paramCompany", companyDt.Rows(0)("company_id")),
									New DbParamInfo("paramYear", paramYear),
									New DbParamInfo("paramMonth", paramMonth)
								}
								
								BRApi.Database.ExecuteActionQuery(
									dbConn,
									"
										UPDATE XFC_MAIN_MASTER_PnLHasChanged
										SET fpna = 0
										WHERE company_id = @paramCompany
											AND year = @paramYear
											AND month = @paramMonth;
									",
									dbParamInfos,
									False,
									False
								)
							End Using
							
							'Get current Workflow Cluster Pk
							Dim wfClusterPK As WorkflowUnitClusterPk = si.WorkflowClusterPk
							
							'Complete Workspace
							BRApi.Workflow.Status.SetWorkflowStatus(si, wfClusterPK, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, "Workspace Completed", "", "User clicked complete workflow", Guid.Empty)
						
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
							Return selectionChangedTaskResult
						
						End If
						
						#End Region
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						
						#Region "Update Table Info"
						
						If args.FunctionName.XFEqualsIgnoreCase("UpdateTableInfo") Then
							
							' Get table name
							Dim paramTableName As String = args.NameValuePairs("p_table_name")
							
							' Update table info and transformation rules
							Me.UpdateTableInfo(si, args, paramTableName)
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.CancelDefaultSave = False
							Return saveDataTaskResult
						
						#End Region
						
						#Region "Custom Save Data"
							
						Else If args.FunctionName.XFEqualsIgnoreCase("CustomSaveData")
							
							' Get table name
							Dim paramTableName As String = args.NameValuePairs("p_table_name")
							Dim paramAuxTableName As String = args.NameValuePairs("p_aux_table_name")
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							'Create a datatable from modified rows
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)
							
							'Declare column mapping dict
							Dim columnMappingDict As Dictionary(Of String, String) = Me.GetColumnMappingDict(si, paramAuxTableName)
							
							'Map and filter columns in DataTable
							dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
							
							'Save rows in aux table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, paramAuxTableName, dt, "replace")
							
							' Run queries, update table info and transformation rules
							Me.RunPopulationQueries(si, paramAuxTableName)
							Me.UpdateTableInfo(si, args, paramTableName)
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.CancelDefaultSave = True
							Return saveDataTaskResult
							
						#End Region
						
						End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
		#Region "Update Table Info"
		
		Public Sub UpdateTableInfo(si As SessionInfo, args As DashboardExtenderArgs, tableName As String)
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramTable", tableName),
					New DbParamInfo("paramUsername", si.UserName)
				}
			
				BRApi.Database.ExecuteSql(
					dbConn,
					"
						MERGE INTO XFC_MAIN_MASTER_TableImportInfo AS target
						USING (
						    SELECT
						        @paramTable AS table_name,
						        @paramUsername AS username,
						        GETUTCDATE() AS last_import_date
						) AS source
						ON target.table_name = source.table_name
						WHEN MATCHED THEN
						    UPDATE SET
						        username = source.username,
						        last_import_date = source.last_import_date
						WHEN NOT MATCHED THEN
						    INSERT (table_name, username, last_import_date)
						    VALUES (source.table_name, source.username, source.last_import_date);
					",
					dbParamInfos,
					False
				)
			End Using
			
			' Change processing logic depending on the table name
			If tableName = "XFC_MAIN_MASTER_CostCentersOldToOneStreamMember" Then
				Me.ProcessCostCenterTransformationRule(si, args.SqlTableEditorSaveDataTaskInfo.EditedDataRows)
				
			Else If tableName = "XFC_MAIN_MASTER_FlowMappings" Then
				Me.ProcessFlowTransformationRule(si, args.SqlTableEditorSaveDataTaskInfo.EditedDataRows)
				
			Else If tableName = "XFC_MAIN_RAW_AccountBSToConso" OrElse tableName = "XFC_MAIN_RAW_AccountPLToConso" Then
				Me.ProcessAccountRubricTransformationRule(si, args.SqlTableEditorSaveDataTaskInfo.EditedDataRows)
				
			Else If tableName = "XFC_MAIN_RAW_CustomersToUD1IC" Then
				Me.ProcessUD1ICTransformationRule(si, args.SqlTableEditorSaveDataTaskInfo.EditedDataRows)
				
			Else If tableName = "XFC_MAIN_RAW_ProfitCenterMappings" Then
				Me.ProcessCebeToEntityTransformationRule(si, args.SqlTableEditorSaveDataTaskInfo.EditedDataRows)
			End If
		End Sub
		
		#End Region
		
		#Region "Process Transformation Rules"
		
		#Region "Process Cost Center Transformation Rule"
		
		Private Sub ProcessCostCenterTransformationRule(si As SessionInfo, editedDataRows As List(Of XFEditedDataRow))
		
			' Edit rule group depending on each data row
			Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "TR_CostCenter", True)
			
			' Generate a dictionary with each source value and its id
			Dim sourceValueToIdDictionary As New Dictionary(Of String, Guid)
			For Each rule In ruleGroup.Rules
				sourceValueToIdDictionary.Add(rule.RuleName, rule.UniqueID)
			Next
			
			For Each editedDataRow In editedDataRows
				
				Select Case editedDataRow.InsertUpdateOrDelete
					
				Case DbInsUpdateDelType.Insert
					Dim newRule As New TransformRuleInfo(
						guid.NewGuid(),
						ruleGroup.UniqueID,
						editedDataRow.ModifiedDataRow("id_old"),
						1,
						"NA",
						editedDataRow.ModifiedDataRow("onestream_member_name"),
						False,
						"",
						Nothing,
						Nothing,
						Nothing,
						If(editedDataRow.ModifiedDataRow("id_old") = "*", 1, 0),
						0
					)		
					BRApi.Import.Metadata.CreateRule(si, newRule)
					
				Case DbInsUpdateDelType.Update
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id_old")) Then
						Dim newRule As New TransformRuleInfo(
							sourceValueToIdDictionary(editedDataRow.ModifiedDataRow("id_old")),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("id_old"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("onestream_member_name"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("id_old") = "*", 1, 0),
							0
						)
						BRApi.Import.Metadata.UpdateRule(si, newRule)
					Else
						Dim newRule As New TransformRuleInfo(
							guid.NewGuid(),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("id_old"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("onestream_member_name"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("id_old") = "*", 1, 0),
							0
						)		
						BRApi.Import.Metadata.CreateRule(si, newRule)
					End If
					
				Case DbInsUpdateDelType.Delete
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id_old")) Then _
						BRApi.Import.Metadata.DeleteRule(si, sourceValueToIdDictionary(editedDataRow.OriginalDataRow("id_old")))
				End Select
			Next
		
		End Sub
		
		#End Region
		
		#Region "Process Flow Transformation Rule"
		
		Private Sub ProcessFlowTransformationRule(si As SessionInfo, editedDataRows As List(Of XFEditedDataRow))
		
			' Edit rule group depending on each data row
			Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_CIM_Flow", True)
			
			' Generate a dictionary with each source value and its id
			Dim sourceValueToIdDictionary As New Dictionary(Of String, Guid)
			For Each rule In ruleGroup.Rules
				sourceValueToIdDictionary.Add(rule.RuleName, rule.UniqueID)
			Next
			
			For Each editedDataRow In editedDataRows
				
				Select Case editedDataRow.InsertUpdateOrDelete
					
				Case DbInsUpdateDelType.Insert
					Dim newRule As New TransformRuleInfo(
						guid.NewGuid(),
						ruleGroup.UniqueID,
						editedDataRow.ModifiedDataRow("sap_flow"),
						1,
						"NA",
						editedDataRow.ModifiedDataRow("flow"),
						False,
						"",
						Nothing,
						Nothing,
						Nothing,
						If(editedDataRow.ModifiedDataRow("sap_flow") = "*", 1, 0),
						0
					)		
					BRApi.Import.Metadata.CreateRule(si, newRule)
					
				Case DbInsUpdateDelType.Update
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("sap_flow")) Then
						Dim newRule As New TransformRuleInfo(
							sourceValueToIdDictionary(editedDataRow.ModifiedDataRow("sap_flow")),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("sap_flow"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("flow"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("sap_flow") = "*", 1, 0),
							0
						)
						BRApi.Import.Metadata.UpdateRule(si, newRule)
					Else
						Dim newRule As New TransformRuleInfo(
							guid.NewGuid(),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("sap_flow"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("flow"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("sap_flow") = "*", 1, 0),
							0
						)		
						BRApi.Import.Metadata.CreateRule(si, newRule)
					End If
					
				Case DbInsUpdateDelType.Delete
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("sap_flow")) Then _
						BRApi.Import.Metadata.DeleteRule(si, sourceValueToIdDictionary(editedDataRow.OriginalDataRow("sap_flow")))
				End Select
			Next
		
		End Sub
		
		#End Region
		
		#Region "Process Account Rubric Transformation Rule"
		
		Private Sub ProcessAccountRubricTransformationRule(si As SessionInfo, editedDataRows As List(Of XFEditedDataRow))
		
			' Edit rule group depending on each data row
			Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_BSGL_Account", True)
			
			' Generate a dictionary with each source value and its id
			Dim sourceValueToIdDictionary As New Dictionary(Of String, Guid)
			For Each rule In ruleGroup.Rules
				sourceValueToIdDictionary.Add(rule.RuleName, rule.UniqueID)
			Next
			
			For Each editedDataRow In editedDataRows
				
				If editedDataRow.InsertUpdateOrDelete = DbInsUpdateDelType.Delete Then
					If Not editedDataRow.OriginalDataRow("cost_center_class_id").ToString = "none" Then Continue For
				Else
					If Not editedDataRow.ModifiedDataRow("cost_center_class_id").ToString = "none" Then Continue For
				End If
				
				Select Case editedDataRow.InsertUpdateOrDelete
					
				Case DbInsUpdateDelType.Insert
					Dim newRule As New TransformRuleInfo(
						guid.NewGuid(),
						ruleGroup.UniqueID,
						editedDataRow.ModifiedDataRow("account_name"),
						1,
						"NA",
						editedDataRow.ModifiedDataRow("conso_rubric"),
						If(
							editedDataRow.ModifiedDataRow("conso_rubric").ToString.StartsWith("2") OrElse
							editedDataRow.ModifiedDataRow("conso_rubric").ToString.StartsWith("3") OrElse
							editedDataRow.ModifiedDataRow("conso_rubric").ToString.Contains("1VDIV"),
							True,
							False
						),
						"",
						Nothing,
						Nothing,
						Nothing,
						If(editedDataRow.ModifiedDataRow("account_name") = "*", 1, 0),
						0
					)		
					BRApi.Import.Metadata.CreateRule(si, newRule)
					
				Case DbInsUpdateDelType.Update
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("account_name")) Then
						Dim newRule As New TransformRuleInfo(
							sourceValueToIdDictionary(editedDataRow.ModifiedDataRow("account_name")),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("account_name"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("conso_rubric"),
							If(
								editedDataRow.ModifiedDataRow("conso_rubric").ToString.StartsWith("2") OrElse
								editedDataRow.ModifiedDataRow("conso_rubric").ToString.StartsWith("3") OrElse
								editedDataRow.ModifiedDataRow("conso_rubric").ToString.Contains("1VDIV"),
								True,
								False
							),
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("account_name") = "*", 1, 0),
							0
						)
						BRApi.Import.Metadata.UpdateRule(si, newRule)
					Else
						Dim newRule As New TransformRuleInfo(
							guid.NewGuid(),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("account_name"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("conso_rubric"),
							If(
								editedDataRow.ModifiedDataRow("conso_rubric").ToString.StartsWith("2") OrElse
								editedDataRow.ModifiedDataRow("conso_rubric").ToString.StartsWith("3") OrElse
								editedDataRow.ModifiedDataRow("conso_rubric").ToString.Contains("1VDIV"),
								True,
								False
							),
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("account_name") = "*", 1, 0),
							0
						)		
						BRApi.Import.Metadata.CreateRule(si, newRule)
					End If
					
				Case DbInsUpdateDelType.Delete
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("account_name")) Then _
						BRApi.Import.Metadata.DeleteRule(si, sourceValueToIdDictionary(editedDataRow.OriginalDataRow("account_name")))
				End Select
			Next
		
		End Sub
		
		#End Region
		
		#Region "Process UD1 IC Transformation Rule"
		
		Private Sub ProcessUD1ICTransformationRule(si As SessionInfo, editedDataRows As List(Of XFEditedDataRow))
		
			' Edit rule group depending on each data row
			Dim UD1RuleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_Customer_UD1", True)
			
			' Generate a dictionary with each source value and its id
			Dim UD1SourceValueToIdDictionary As New Dictionary(Of String, Guid)
			For Each rule In UD1RuleGroup.Rules
				UD1SourceValueToIdDictionary.Add(rule.RuleName, rule.UniqueID)
			Next
			
			Dim ICRuleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_Customer_IC", True)
			
			' Generate a dictionary with each source value and its id
			Dim ICSourceValueToIdDictionary As New Dictionary(Of String, Guid)
			For Each rule In ICRuleGroup.Rules
				ICSourceValueToIdDictionary.Add(rule.RuleName, rule.UniqueID)
			Next
			
			For Each editedDataRow In editedDataRows
				
				Select Case editedDataRow.InsertUpdateOrDelete
					
				Case DbInsUpdateDelType.Insert
					
					Dim newRule As New TransformRuleInfo
					
					If Not String.IsNullOrEmpty(editedDataRow.ModifiedDataRow("customer_member_name").ToString) Then
						newRule = New TransformRuleInfo(
							guid.NewGuid(),
							UD1RuleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("id"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("customer_member_name"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
							0
						)		
						BRApi.Import.Metadata.CreateRule(si, newRule)
					End If
								
					' If no IC map, continue for
					If String.IsNullOrEmpty(editedDataRow.ModifiedDataRow("ic_member_name").ToString) OrElse
						editedDataRow.ModifiedDataRow("ic_member_name").ToString.ToLower = "none" Then _
						Continue For
					
					newRule = New TransformRuleInfo(
						guid.NewGuid(),
						ICRuleGroup.UniqueID,
						editedDataRow.ModifiedDataRow("id"),
						1,
						"NA",
						editedDataRow.ModifiedDataRow("ic_member_name"),
						False,
						"",
						Nothing,
						Nothing,
						Nothing,
						If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
						0
					)		
					BRApi.Import.Metadata.CreateRule(si, newRule)
					
				Case DbInsUpdateDelType.Update
					If Not String.IsNullOrEmpty(editedDataRow.ModifiedDataRow("customer_member_name").ToString) Then
						If UD1SourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id")) Then
							Dim newRule As New TransformRuleInfo(
								UD1SourceValueToIdDictionary(editedDataRow.ModifiedDataRow("id")),
								UD1RuleGroup.UniqueID,
								editedDataRow.ModifiedDataRow("id"),
								1,
								"NA",
								editedDataRow.ModifiedDataRow("customer_member_name"),
								False,
								"",
								Nothing,
								Nothing,
								Nothing,
								If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
								0
							)
							BRApi.Import.Metadata.UpdateRule(si, newRule)
						Else
							Dim newRule As New TransformRuleInfo(
								guid.NewGuid(),
								UD1RuleGroup.UniqueID,
								editedDataRow.ModifiedDataRow("id"),
								1,
								"NA",
								editedDataRow.ModifiedDataRow("customer_member_name"),
								False,
								"",
								Nothing,
								Nothing,
								Nothing,
								If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
								0
							)		
							BRApi.Import.Metadata.CreateRule(si, newRule)
						End If
					Else
						If UD1SourceValueToIdDictionary.ContainsKey(editedDataRow.ModifiedDataRow("id")) Then _
							BRApi.Import.Metadata.DeleteRule(si, UD1SourceValueToIdDictionary(editedDataRow.ModifiedDataRow("id")))
					End If
								
					' If no IC map, continue for
					If String.IsNullOrEmpty(editedDataRow.ModifiedDataRow("ic_member_name").ToString) OrElse
						editedDataRow.ModifiedDataRow("ic_member_name").ToString.ToLower = "none" Then
						
						' Delete transformation rule if exists
						If ICSourceValueToIdDictionary.ContainsKey(editedDataRow.ModifiedDataRow("id")) Then _
							BRApi.Import.Metadata.DeleteRule(si, ICSourceValueToIdDictionary(editedDataRow.ModifiedDataRow("id")))
							
						Continue For
					End If
					
					If ICSourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id")) Then
						Dim newRule = New TransformRuleInfo(
							ICSourceValueToIdDictionary(editedDataRow.ModifiedDataRow("id")),
							ICRuleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("id"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("ic_member_name"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
							0
						)		
						BRApi.Import.Metadata.UpdateRule(si, newRule)
					Else
						Dim newRule = New TransformRuleInfo(
							guid.NewGuid(),
							ICRuleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("id"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("ic_member_name"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
							0
						)		
						BRApi.Import.Metadata.CreateRule(si, newRule)
					End If
					
				Case DbInsUpdateDelType.Delete
					If UD1SourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id")) Then _
						BRApi.Import.Metadata.DeleteRule(si, UD1SourceValueToIdDictionary(editedDataRow.OriginalDataRow("id")))
					If ICSourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id")) Then _
						BRApi.Import.Metadata.DeleteRule(si, ICSourceValueToIdDictionary(editedDataRow.OriginalDataRow("id")))
				End Select
			Next
		
		End Sub
		
		#End Region
		
		#Region "Process Cebe To Entity Transformation Rule"
		
		Private Sub ProcessCebeToEntityTransformationRule(si As SessionInfo, editedDataRows As List(Of XFEditedDataRow))
		
			' Edit rule group depending on each data row
			Dim ruleGroup = BRApi.Import.Metadata.GetRuleGroup(si, "S4_CeBe_Entity", True)
			
			' Generate a dictionary with each source value and its id
			Dim sourceValueToIdDictionary As New Dictionary(Of String, Guid)
			For Each rule In ruleGroup.Rules
				sourceValueToIdDictionary.Add(rule.RuleName, rule.UniqueID)
			Next
			
			For Each editedDataRow In editedDataRows
				
				Select Case editedDataRow.InsertUpdateOrDelete
					
				Case DbInsUpdateDelType.Insert
					Dim newRule As New TransformRuleInfo(
						guid.NewGuid(),
						ruleGroup.UniqueID,
						editedDataRow.ModifiedDataRow("id"),
						1,
						"NA",
						editedDataRow.ModifiedDataRow("entity_id"),
						False,
						"",
						Nothing,
						Nothing,
						Nothing,
						If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
						0
					)		
					BRApi.Import.Metadata.CreateRule(si, newRule)
					
				Case DbInsUpdateDelType.Update
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id")) Then
						Dim newRule As New TransformRuleInfo(
							sourceValueToIdDictionary(editedDataRow.ModifiedDataRow("id")),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("id"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("entity_id"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
							0
						)
						BRApi.Import.Metadata.UpdateRule(si, newRule)
					Else
						Dim newRule As New TransformRuleInfo(
							guid.NewGuid(),
							ruleGroup.UniqueID,
							editedDataRow.ModifiedDataRow("id"),
							1,
							"NA",
							editedDataRow.ModifiedDataRow("entity_id"),
							False,
							"",
							Nothing,
							Nothing,
							Nothing,
							If(editedDataRow.ModifiedDataRow("id") = "*", 1, 0),
							0
						)		
						BRApi.Import.Metadata.CreateRule(si, newRule)
					End If
					
				Case DbInsUpdateDelType.Delete
					If sourceValueToIdDictionary.ContainsKey(editedDataRow.OriginalDataRow("id")) Then _
						BRApi.Import.Metadata.DeleteRule(si, sourceValueToIdDictionary(editedDataRow.OriginalDataRow("id")))
				End Select
			Next
		
		End Sub
		
		#End Region
		
		#End Region
		
		#Region "Get Column Mapping Dictionary"
		
		Private Function GetColumnMappingDict(ByVal si As SessionInfo, ByVal tableName As String) As Dictionary(Of String, String)
			'Declare new dictionary
			Dim columnMappingDict As Dictionary(Of String, String)
		
			'Control table column mapping dictionaries
			
			#Region "Check PnL Accounts"
			
			If tableName = "XFC_MAIN_AUX_CHECK_PnLAccounts" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"account_name", "account_name"},
			        {"cost_center_class_id", "cost_center_class_id"},
					{"conso_rubric", "conso_rubric"}
				}
				
			#End Region
			
			#Region "Check BS Accounts"
			
			Else If tableName = "XFC_MAIN_AUX_CHECK_BSAccounts" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"account_name", "account_name"},
					{"conso_rubric", "conso_rubric"}
				}
				
			#End Region
			
			#Region "Check PnL Cost Centers"
			
			Else If tableName = "XFC_MAIN_AUX_CHECK_PnLCostCenters" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"id", "id"},
					{"id_old", "id_old"}
				}
				
			#End Region
			
			Else
				Throw ErrorHandler.LogWrite(si, New XFException($"There is no column mapping dictionary for table {tableName}"))
			End If
			
			Return columnMappingDict
				
		End Function
		
		#End Region
		
		#Region "Run Population Queries"
		
		Private Sub RunPopulationQueries(ByVal si As SessionInfo, ByVal tableName As String)
			'Get migration queries
			Dim populationQueries As New List(Of String)
			'Control table name
			
			#Region "Check PnL Accounts"
			
			If tableName = "XFC_MAIN_AUX_CHECK_PnLAccounts" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_AccountRubrics AS target
					USING (
					    SELECT account_name, cost_center_class_id, conso_rubric
						FROM XFC_MAIN_AUX_CHECK_PnLAccounts
					) AS source
					ON target.account_name = source.account_name
					AND target.cost_center_class_id = source.cost_center_class_id
					WHEN MATCHED THEN
					    UPDATE SET
					        conso_rubric = source.conso_rubric
					WHEN NOT MATCHED THEN
					    INSERT (account_name, cost_center_class_id, conso_rubric)
					    VALUES (source.account_name, source.cost_center_class_id, source.conso_rubric);
					")
				
			#End Region
			
			#Region "Check BS Accounts"
			
			Else If tableName = "XFC_MAIN_AUX_CHECK_BSAccounts" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_AccountRubrics AS target
					USING (
					    SELECT account_name, 'none' AS cost_center_class_id, conso_rubric
						FROM XFC_MAIN_AUX_CHECK_BSAccounts
					) AS source
					ON target.account_name = source.account_name
					AND target.cost_center_class_id = source.cost_center_class_id
					WHEN MATCHED THEN
					    UPDATE SET
					        conso_rubric = source.conso_rubric
					WHEN NOT MATCHED THEN
					    INSERT (account_name, cost_center_class_id, conso_rubric)
					    VALUES (source.account_name, source.cost_center_class_id, source.conso_rubric);
					")
				
			#End Region
			
			#Region "Check PnL Cost Centers"
			
			Else If tableName = "XFC_MAIN_AUX_CHECK_PnLCostCenters" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_CostCentersOldToNew AS target
					USING (
					    SELECT id, id_old
						FROM XFC_MAIN_AUX_CHECK_PnLCostCenters
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        id_old = source.id_old
					WHEN NOT MATCHED THEN
					    INSERT (id, id_old)
					    VALUES (source.id, source.id_old);
					")
				
			#End Region
			
			End If
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Populate tables and update table info
				For Each query In populationQueries
					BRApi.Database.ExecuteSql(dbConn, query, False)
				Next
			End Using
		End Sub
		
		Private Function GetPopulationQueries(ByVal si As SessionInfo, ByVal tables As List(Of Object)) As List(Of String)
			'Declare list of queries
			Dim queries As New List(Of String)
			
			'Loop through all the tables to get the queries
			For Each table In tables
				queries.Add(table.GetPopulationQuery(si, "up"))
			Next
			
            Return queries
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
