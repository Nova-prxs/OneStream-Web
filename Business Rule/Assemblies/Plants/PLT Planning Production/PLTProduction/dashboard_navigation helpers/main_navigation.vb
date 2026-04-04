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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_navigation
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try				
				
				Select Case args.FunctionType									
					
					#Region "Load Dashboard"
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						
						If args.FunctionName.XFEqualsIgnoreCase("LoadDashboard") Then
							'Get workspace prefix
							Dim WorkspacePrefix As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLTProduction_Workspace_Prefix")
							'Declare load dashboard task result
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
							'Declare current dashboard
							Dim paramDashboard As String

							'If first load, get default parameter, else use the subst var from prior run
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize _
								And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								'Get default dashboard
								paramDashboard = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLTProduction_MainNavigation_Default")
'								BRApi.ErrorLog.LogMessage(si, "1. Current dashboard: " & paramDashboard)
							Else
								'Get current dashboard parameter from other business rule call
								paramDashboard = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_CurrentDashboard")
'								BRApi.ErrorLog.LogMessage(si, "2. Current dashboard: " & paramDashboard)
							End If
							'Set current dashboard parameter
							loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard									
							
							'Get Scenario Selected
							Dim scenarioSelected As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLTProduction_scenario_selected")

'							BRApi.ErrorLog.LogMessage(si, "scenario selected: " & scenarioSelected)

							If (paramDashboard.Contains("PLTProduction_1_")) Then
								
								loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_scenario_list") = "Actual"								
								loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_scenario") = "Actual"								
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_scenario_selected") = "Actual"
								
								If (args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLTProduction_year_selected") = "" Or  scenarioSelected.Contains("Actual")=False)
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_year_selected") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_year_N")
								End If	
							
							Else If (paramDashboard.Contains("PLTProduction_2_")) Then
								
								Dim monthFirstFcst As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_month_first_forecast")
								Me.setGridViewParameters(si, monthFirstFcst, loadDashboardTaskResult)																

								If (scenarioSelected = "" Or scenarioSelected.Contains("RF") = False)
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_scenario_selected_PopUp") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_forecast")
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_scenario_selected") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_forecast")
								End If
								
								loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_scenario_list") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"prm_PLTProduction_scenario_selected")
								loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_scenario") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"prm_PLTProduction_scenario_selected")
								
								
								' Link descarga
								Me.setDownloadLink(si, paramDashboard, "PLTProduction_2_2_1", loadDashboardTaskResult, args)								

								If (args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLTProduction_year_selected") = "" Or scenarioSelected.Contains("R")=False)
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_year_selected") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_year_N")
									loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_year") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,  $"Plants.prm_PLT_year_N")
								End If		
								
							Else

								Me.setGridViewParameters(si, "1", loadDashboardTaskResult)

								If (scenarioSelected = "" Or scenarioSelected.Contains("B") = False)
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_scenario_selected_PopUp") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_budget")
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_scenario_selected") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_budget")
								End If
								
								loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_scenario_list") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"prm_PLTProduction_scenario_selected")
								loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_scenario") = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"prm_PLTProduction_scenario_selected")

								' Link descarga
								Me.setDownloadLink(si, paramDashboard, "PLTProduction_3_2_1", loadDashboardTaskResult, args)	
								
								If (args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLTProduction_year_selected") = "" Or scenarioSelected.Contains("B") = False)
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_year_selected") = (BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_year_N") + 1).ToString
									loadDashboardTaskResult.ModifiedCustomSubstVars("Plants.PLT_LoadFiles_prm_year") = (BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,  $"Plants.prm_PLT_year_N") + 1).ToString
								End If	
								
							End If	

							'Loop through each level of the dashboard based on the name
							Dim dashboardSplitted As String() = paramDashboard.Split("_")
							For level As Integer = 1 To dashboardSplitted.Count - 1
								Dim activeDashboardNumber As Integer = CInt(dashboardSplitted(level))

								'Set dashboard content and active tab
								Me.SetActiveContent(si, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
								Me.SetActiveTab(si, loadDashboardTaskResult, WorkspacePrefix, level, 20, activeDashboardNumber)
								'Set dashboard parameters
								Me.SetDashboardParameters(si, args, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
							Next
							
							If (args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLT_factory") = "")	
								Dim sFirstValue As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_factory_default")
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLT_factory") = sFirstValue																															
							End If							
							
							Return loadDashboardTaskResult
							
						End If
						
					#End Region
					
					#Region "Component Selection Changed"
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						#Region "Open Dashboard"
						
'						BRApi.ErrorLog.LogMessage(si, "args.FunctionName: " & args.FunctionName)
						
						If args.FunctionName.XFEqualsIgnoreCase("OpenDashboard")
							'Get dashboard
							Dim paramDashboard As String = args.NameValuePairs("p_dashboard")
							'Set current dashboard
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
							
							selectionChangedTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard
																					
							Return selectionChangedTaskResult
							
						#End Region
													
						End If
						
					#End Region
					
					#Region "SQL Table Editor Save Data"
					
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
						
						#End Region
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "Set Active Content"
		
		Private Sub SetActiveContent(
			ByVal si As SessionInfo, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Set dynamic content parameter
			'If level is 1, set the initial dashboard, else append to the last level parameter
			If level = 1 Then
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_Content") = $"{WorkspacePrefix}_{activeDashboardNumber}"
			Else
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_Content") = _
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level - 1}_Content") &
					$"_{activeDashboardNumber}"
			End If
		End Sub
		
		#End Region
		
		#Region "Set Active Tab"
		
		Private Sub SetActiveTab(
			ByVal si As SessionInfo, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal quantity As Integer, ByVal activeDashboardNumber As Integer
		)
			'Get colors for each level
			Dim level1Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_LeftBar_Tab1_Active")
			Dim level2Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_LeftBar_Tab2_Active")
			Dim level3Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_LeftBar_Tab3_Active")
			
			'Set colors for each level
			'Color tuple definition: (ActiveColor, InactiveColor)
			Dim colorDictionary As New Dictionary(Of Integer, Tuple(Of String, String)) From {
				{1, New Tuple(Of String, String)(level1Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_LeftBarBG"))},
				{2, New Tuple(Of String, String)(level2Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_LeftBarBG"))},
				{3, New Tuple(Of String, String)(level3Active, "White")},
				{4, New Tuple(Of String, String)(level3Active, "White")},
				{5, New Tuple(Of String, String)(level3Active, "White")}
			}
			
			'Loop through all the repeater components
			For i As Integer = 1 To quantity
				'Control left tabs
				If level = 1 Then 
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_{WorkspacePrefix}_{i}_LeftTabs") = _
					If(i = activeDashboardNumber, $"{WorkspacePrefix}_{i}_LeftTabs", $"{WorkspacePrefix}_Transparent")
				End If
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_ActiveColor_{i}") = _
					If(i = activeDashboardNumber, colorDictionary(level).Item1, colorDictionary(level).Item2)
			Next
		End Sub
		
		#End Region
		
		#Region "Set Dashboards Parameters"
		
		Private Sub SetDashboardParameters(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Control dashboard parameters
			'EXAMPLE
'			If level = 1 Then
'				If activeDashboardNumber = 3 Then
'					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Main") = "Planning"
'					If args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue($"prm_Investments_Scenario") = "Actual" Then _
'						loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario") = "Budget"
'				Else
'					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Main") = "Actual"
'					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario") = "Actual"
'				End If
'			End If
			
		End Sub
		
		#End Region
		
		#Region "Set Grid View Parameters"
		
		Private Sub setGridViewParameters(ByVal si As SessionInfo, ByVal ActiveMonth As String, ByVal loadDashboardTaskResult As XFLoadDashboardTaskResult)
			
		For month As Integer = 1 To 12
				If month < CInt(ActiveMonth)
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_M{month}") = "AllowUpdates = False, BackgroundColor = WhiteSmoke"
				Else
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_M{month}") = "AllowUpdates = True, BackgroundColor = White"
				End If
			Next
			
		End Sub
		
		#End Region		
		
		#Region "Set Download Link"
		
		Private Sub setDownloadLink (
								ByVal si As SessionInfo, ByVal paramDashboard As String, ByVal dashboardNameFilter As String, _
								ByVal loadDashboardTaskResult As XFLoadDashboardTaskResult, ByVal args As DashboardExtenderArgs, _
								Optional templateNameFilter As String = "XFC_PLT_PLAN_NewProducts_Simulation.xlsx"
							)
							
			If paramDashboard.Contains(dashboardNameFilter) Then
		
				Dim factory As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLT_factory")				
				Dim scenario As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLTProduction_scenario_selected")
				Dim year As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLTProduction_year_selected")
				
				Dim rootPath As String = "Documents/Public/Plants"
				Dim fullFilePath As String = $"{rootPath}/Import/{factory}/{year}/{scenario}/{templateNameFilter}"
				Dim templateFilePath As String = $"{rootPath}/Templates/{templateNameFilter}"
				
				If (BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, fullFilePath))			
					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_Link") = $"[{fullFilePath}]"
				Else 
					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLTProduction_Link") = $"[{templateFilePath}]"
				End If
				
			End If
			
		End Sub
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
