Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
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



Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.status
	Public Class MainClass
		
		Dim CCAADynamicAccountBR As New OneStream.BusinessRule.Finance.CCAA_DynamicAccount.MainClass


		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("LoadStatusTable") Then
							
							'Implement Load Dashboard logic here.
							
							'If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
'								Dim entityNameEscaped As String = ""

'								If si IsNot Nothing AndAlso si.WorkflowClusterPk IsNot Nothing Then
'								    entityNameEscaped = si.WorkflowClusterPk.Entity.Replace("'", "''")
'								End If


'								If entityNameEscaped = "R0592 - Horse Argentina"
'									loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
'									loadDashboardTaskResult.ModifiedCustomSubstVars("StatusTableName") = "XFC_CCAA_CubeView_Status_Argentina"
'								Else
								'loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
								'loadDashboardTaskResult.ModifiedCustomSubstVars("StatusTableName") = "XFC_CCAA_CubeView_Status_Argentina"
									
								'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "CCAA.prm_StatusTableName", "XFC_CCAA_CubeView_Status_Argentina")
'								End If
								Return loadDashboardTaskResult
							'End If
						End If
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
													
						'''SAVE BUTTON
						If args.FunctionName.XFEqualsIgnoreCase("UpdateTable") Then
							
							#Region "Define country and table"
							
							' Obtain Workflow Profile, country/Horse_Group and code
							
							Dim ProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk).Name
							Dim pais As String = ""
							Dim codigo As String = ""
							'brapi.ErrorLog.LogMessage(si, $"entra")
							If ProfileName = "WP_Horse_Brazil.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Brazil"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Brazil"
								pais = "Brazil"
								codigo = "R1303"
								'brapi.ErrorLog.LogMessage(si, $"entra")
							Else If ProfileName = "WP_Horse_Argentina.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Argentina"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Argentina"
								pais = "Argentina"
								codigo = "R0592"
							Else If ProfileName = "WP_Horse_Chile.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Chile"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Chile"
								pais = "Chile"
								codigo = "R0585"
							Else If ProfileName = "WP_Horse_Holding.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Holding"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Holding"
								pais = "Holding"
								codigo = "R1300"
							Else If ProfileName = "WP_Horse_Spain.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								'brapi.ErrorLog.LogMessage(si, $"{pais}")
								codigo = "R1301"
							Else If ProfileName = "WP_Horse_Portugal.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Portugal"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Portugal"
								pais = "Portugal"
								codigo = "R0671"
							Else If ProfileName = "WP_Horse_Romania.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Romania"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Romania"
								pais = "Romania"
								codigo = "R0611"
							Else If ProfileName = "WP_Horse_Turkey.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Turkey"
								codigo = "R1302"
							Else If ProfileName = "WP_Horse Group.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Group"
								codigo = ""
							Else
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								codigo = "R1301"
							End If
							
							' Define status table names
							
							Dim WorkflowTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
							Dim tabla As String = "XFC_CCAA_CubeView_Status_" & pais & "_" & WorkflowTime
							Dim tabla_Group As String = "XFC_CCAA_CubeView_Status_Group_" & WorkflowTime
							 
							#End Region
							
							Dim cubeview As String
							cubeview = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode")
										
							'Implement Dashboard Component Selection Changed logic here.
							'brapi.ErrorLog.LogMessage(si, "ENTRA 2")
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							'selectionChangedTaskResult.Message = $"CubeView {action} correctamente"
							'selectionChangedTaskResult.Message = $"CubeView guardado correctamente"
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
							
							
							Dim userNameEscaped As String = si.UserName.Replace("'", "''")
							
							'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Submitted", "")
							'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "")
							
							' Update country table when save button
							
							Dim sql As String = $"
								UPDATE A
								SET A.last_save_date = GETDATE(),
								    A.last_save_name = '" & userNameEscaped & "',
									A.send_status = 'Not Started',
									A.check_status = 'Not Started',
									A.send_name = '-',
									A.check_name = '-'
								FROM " & tabla & " AS A
								WHERE A.id = '" & codigo & " - " & cubeview & "'
							"			
							'brapi.ErrorLog.LogMessage(si, "GUARDADO")
							ExecuteSql(si, sql)
							
							' Update Horse Group table when save button
							
							Dim sql2 As String = $"
								UPDATE A
								SET A.last_save_date = GETDATE(),
								    A.last_save_name = '" & userNameEscaped & "',
									A.send_status = 'Not Started',
									A.check_status = 'Not Started',
									A.send_name = '-',
									A.check_name = '-'
								FROM " & tabla_Group & " AS A
								WHERE A.id = '" & codigo & " - " & cubeview & "'
							"			
							'brapi.ErrorLog.LogMessage(si, "GUARDADO")
							ExecuteSql(si, sql2)
							
							
							' 8D4_AC - When input for Spain, copy for Holding
							
							If (pais = "Spain" And cubeview = "8-D4")
								'brapi.ErrorLog.LogMessage(si, "entra status file")
								
								Dim consomember = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_CCAA_LocalEuro")
								Dim sTime As String = WorkflowTime
								
								CCAADynamicAccountBR.Acc8D4(si, consomember, sTime)

								
							End If
							
							Return selectionChangedTaskResult
							
							
							
						'''CHECK BUTTON
						Else If args.FunctionName.XFEqualsIgnoreCase("UpdateTable_Check") Then
							
							#Region "Define country"
							
							' Obtain Workflow Profile, country/Horse_Group and code
							
							Dim ProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk).Name
							Dim pais As String = ""
							Dim codigo As String = ""
							'brapi.ErrorLog.LogMessage(si, $"entra")
							If ProfileName = "WP_Horse_Brazil.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Brazil"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Brazil"
								pais = "Brazil"
								codigo = "R1303"
								'brapi.ErrorLog.LogMessage(si, $"entra")
							Else If ProfileName = "WP_Horse_Argentina.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Argentina"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Argentina"
								pais = "Argentina"
								codigo = "R0592"
							Else If ProfileName = "WP_Horse_Chile.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Chile"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Chile"
								pais = "Chile"
								codigo = "R0585"
							Else If ProfileName = "WP_Horse_Holding.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Holding"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Holding"
								pais = "Holding"
								codigo = "R1300"
							Else If ProfileName = "WP_Horse_Spain.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								'brapi.ErrorLog.LogMessage(si, $"{pais}")
								codigo = "R1301"
							Else If ProfileName = "WP_Horse_Portugal.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Portugal"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Portugal"
								pais = "Portugal"
								codigo = "R0671"
							Else If ProfileName = "WP_Horse_Romania.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Romania"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Romania"
								pais = "Romania"
								codigo = "R0611"
							Else If ProfileName = "WP_Horse_Turkey.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Turkey"
								codigo = "R1302"
							Else If ProfileName = "WP_Horse Group.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Group"
								codigo = ""
							Else
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								codigo = "R1301"
							End If
							
							' Define status table names
							
							Dim WorkflowTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
							Dim tabla As String = "XFC_CCAA_CubeView_Status_" & pais & "_" & WorkflowTime
							Dim tabla_Group As String = "XFC_CCAA_CubeView_Status_Group_" & WorkflowTime
							
							#End Region
							
							Dim cubeview As String
							cubeview = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode")
							
							'Implement Dashboard Component Selection Changed logic here.
							'brapi.ErrorLog.LogMessage(si, "ENTRA 2")
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
							
							
							Dim userNameEscaped As String = si.UserName.Replace("'", "''")
							
							'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Submitted", "")
							'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "Checked")
							
'							Dim img_status As String
							
'							img_status = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_statusImage")
							
							' Update country table when check button

							Dim sql As String = $"
								UPDATE A
								SET A.check_status = 'Checked',
								    A.check_date = GETDATE(),
								    A.check_name = '" & userNameEscaped & "'
								FROM " & tabla & " AS A
								WHERE A.id = '" & codigo & " - " & cubeview & "'
							"			
							'brapi.ErrorLog.LogMessage(si, "GUARDADO")
							ExecuteSql(si, sql)
							'selectionChangedTaskResult.Message = $"CubeView cerrado"
							
							' Update Horse Group table when check button
							
							Dim sql2 As String = $"
								UPDATE A
								SET A.check_status = 'Checked',
								    A.check_date = GETDATE(),
								    A.check_name = '" & userNameEscaped & "'
								FROM " & tabla_Group & " AS A
								WHERE A.id = '" & codigo & " - " & cubeview & "'
							"			
							'brapi.ErrorLog.LogMessage(si, "GUARDADO")
							ExecuteSql(si, sql2)
							
							Return selectionChangedTaskResult
						
						'''SUBMIT BUTTON
						Else If args.FunctionName.XFEqualsIgnoreCase("UpdateTable_Submit") Then
							
							#Region "Define country"
							
							' Obtain Workflow Profile, country/Horse_Group and code
							
							Dim ProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk).Name
							Dim pais As String = ""
							Dim codigo As String = ""
							'brapi.ErrorLog.LogMessage(si, $"entra")
							If ProfileName = "WP_Horse_Brazil.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Brazil"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Brazil"
								pais = "Brazil"
								codigo = "R1303"
								'brapi.ErrorLog.LogMessage(si, $"entra")
							Else If ProfileName = "WP_Horse_Argentina.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Argentina"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Argentina"
								pais = "Argentina"
								codigo = "R0592"
							Else If ProfileName = "WP_Horse_Chile.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Chile"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Chile"
								pais = "Chile"
								codigo = "R0585"
							Else If ProfileName = "WP_Horse_Holding.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Holding"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Holding"
								pais = "Holding"
								codigo = "R1300"
							Else If ProfileName = "WP_Horse_Spain.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								'brapi.ErrorLog.LogMessage(si, $"{pais}")
								codigo = "R1301"
							Else If ProfileName = "WP_Horse_Portugal.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Portugal"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Portugal"
								pais = "Portugal"
								codigo = "R0671"
							Else If ProfileName = "WP_Horse_Romania.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Romania"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Romania"
								pais = "Romania"
								codigo = "R0611"
							Else If ProfileName = "WP_Horse_Turkey.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Turkey"
								codigo = "R1302"
							Else If ProfileName = "WP_Horse Group.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Group"
								codigo = ""
							Else
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								codigo = "R1301"
							End If	
							
							' Define status table names
							
							Dim WorkflowTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
							Dim tabla As String = "XFC_CCAA_CubeView_Status_" & pais & "_" & WorkflowTime
							Dim tabla_Group As String = "XFC_CCAA_CubeView_Status_Group_" & WorkflowTime
							
							#End Region
							
							Dim cubeview As String
							cubeview = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode")
							
							'Implement Dashboard Component Selection Changed logic here.
							'brapi.ErrorLog.LogMessage(si, "ENTRA 2")
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
							
							
							Dim userNameEscaped As String = si.UserName.Replace("'", "''")
							
							
							'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Submitted", "Ready to check")
							'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "")
							
							' Update country table when submit button
							
							Dim sql As String = $"
								UPDATE A
								SET A.send_status = 'Submitted',
								    A.send_date = GETDATE(),
								    A.send_name = '" & userNameEscaped & "',
									A.check_status = 'Not Started',
									A.check_name = '-'
								FROM " & tabla & " AS A
								WHERE A.id = '" & codigo & " - " & cubeview & "'
							"
			
							ExecuteSql(si, sql)
							
							' Update Horse Group table when submit button
							
							Dim sql2 As String = $"
								UPDATE A
								SET A.send_status = 'Submitted',
								    A.send_date = GETDATE(),
								    A.send_name = '" & userNameEscaped & "',
									A.check_status = 'Not Started',
									A.check_name = '-'
								FROM " & tabla_Group & " AS A
								WHERE A.id = '" & codigo & " - " & cubeview & "'
							"
			
							ExecuteSql(si, sql2)
							
							Return selectionChangedTaskResult
							
						End If
						
						
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement SQL Table Editor Save Data logic here.
							'Save the data rows.
							'Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							'Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName)
								'dbConn.BeginTrans()
								'BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, True, False, False)
								'dbConn.CommitTrans()
							'End Using
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.Message = ""
							saveDataTaskResult.CancelDefaultSave = False 'Note: Use True if we already saved the data rows in this Business Rule.
							Return saveDataTaskResult
						End If
				End Select

				Return Nothing
				
					
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
					        	Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
					                               
					                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
									
					            End Using   
									
		End Sub
							
	End Class
	
	
	
End Namespace
