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
							Dim WorkspacePrefix As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "SecurityHomeScreen.prm_SecurityHomeScreen_Workspace_Prefix")
							'Declare load dashboard task result
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
							'Declare current dashboard
							Dim paramDashboard As String
							
							'If first load, get default parameter, else use the subst var from prior run
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize _
								And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								'Get default dashboard
								paramDashboard = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, $"SecurityHomeScreen.prm_{WorkspacePrefix}_MainNavigation_Default")
							Else
								'Get current dashboard parameter from other business rule call
								paramDashboard = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_CurrentDashboard")
							End If
							'Set current dashboard parameter
							loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard
							
							'Loop through each level of the dashboard based on the name
							Dim dashboardSplitted As String() = paramDashboard.Split("_")
							For level As Integer = 1 To dashboardSplitted.Count - 1
								Dim activeDashboardNumber As Integer = CInt(dashboardSplitted(level))
								
								'Set dashboard content and active tab
								Me.SetActiveContent(si, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
								Me.SetActiveTab(si, loadDashboardTaskResult, WorkspacePrefix, level, 20, activeDashboardNumber)
								'Set dashboard parameters
								Me.SetDashboardParametersOnLoad(si, args, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
							Next
							
							Return loadDashboardTaskResult
							
						End If
						
					#End Region
					
					#Region "Component Selection Changed"
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						#Region "Open Dashboard"
						
						If args.FunctionName.XFEqualsIgnoreCase("OpenDashboard")
							'Get dashboard
							Dim paramDashboard As String = args.NameValuePairs("p_dashboard")
							'Set current dashboard
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
							selectionChangedTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard
							
							'Control dashboard parameters on click
							Me.SetDashboardParametersOnClick(si, args, selectionChangedTaskResult, paramDashboard)
							
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
			Dim level1Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab1_Active")
			Dim level2Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab2_Active")
			Dim level3Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab3_Active")
			
			'Set colors for each level
			'Color tuple definition: (ActiveColor, InactiveColor)
			Dim colorDictionary As New Dictionary(Of Integer, Tuple(Of String, String)) From {
				{1, New Tuple(Of String, String)(level1Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBarBG"))},
				{2, New Tuple(Of String, String)(level2Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBarBG"))},
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
		
		#Region "Set Dashboards Parameters On Load"
		
		Private Sub SetDashboardParametersOnLoad(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Control dashboard parameters
			'EXAMPLE
'			If level = 1 Then
'				If activeDashboardNumber = 3 Then
'					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_SecurityHomeScreen_Scenario_Main") = "Planning"
'					If args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_SecurityHomeScreen_Scenario") = "Actual" Then _
'						loadDashboardTaskResult.ModifiedCustomSubstVars("prm_SecurityHomeScreen_Scenario") = "Budget"
'				Else
'					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_SecurityHomeScreen_Scenario_Main") = "Actual"
'					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_SecurityHomeScreen_Scenario") = "Actual"
'				End If
'			End If
			
		End Sub
		
		#End Region
		
		#Region "Set Dashboards Parameters On Click"
		
		Private Sub SetDashboardParametersOnClick(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef selectionChangedTaskResult As XFSelectionChangedTaskResult,
			ByVal paramDashboard As String
		)
			'Control dashboard parameters
			'EXAMPLE
			If paramDashboard = "SecurityHomeScreen_1_1" Then 
				
				Dim wfProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk)
				Dim OpenForecast As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Horse.00_Doc_Open_Forecast")

				
				BRApi.Workflow.General.SetSelectedWorkflowView(si, wfProfileInfo.Name, OpenForecast, BRApi.Finance.Time.GetYearFromId(si, si.WorkflowClusterPk.TimeKey))
				
				selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
			
			Else If paramDashboard = "SecurityHomeScreen_1_2" Then 
			
				Dim wfProfileInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk)
				Dim OpenBudget As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Horse.00_Doc_Open_Budget")
				
				BRApi.Workflow.General.SetSelectedWorkflowView(si, wfProfileInfo.Name, OpenBudget, BRApi.Finance.Time.GetYearFromId(si, si.WorkflowClusterPk.TimeKey))
				
				selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
			
			
			End If
		End Sub
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
