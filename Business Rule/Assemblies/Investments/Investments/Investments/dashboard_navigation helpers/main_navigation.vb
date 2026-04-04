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
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					#Region "Load Dashboard"
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("LoadDashboard") Then
							'Get workspace prefix
							Dim WorkspacePrefix As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Investments.prm_Workspace_Prefix")
							'Declare load dashboard task result
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
							'Declare current dashboard
							Dim paramDashboard As String
							
							'If first load, get default parameter, else use the subst var from prior run
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize _
								And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								'Get default dashboard
								paramDashboard = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"{WorkspacePrefix}.prm_{WorkspacePrefix}_MainNavigation_Default")
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
								Me.SetDashboardParameters(si, args, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
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
							Return selectionChangedTaskResult
							
						#End Region
						
						#Region "Conditional Navigation"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("ConditionalNavigation")
						    'Get parameters
						    Dim scenarioName As String = args.NameValuePairs("prm_Investments_Scenario")
						    Dim rfDashboard As String = args.NameValuePairs("p_rf_dashboard")
						    Dim budDashboard As String = args.NameValuePairs("p_bud_dashboard") 
						    
						    Dim targetDashboard As String
						    
						    'Determine dashboard based on scenario
						    If scenarioName.ToUpper().StartsWith("RF") Then
						        targetDashboard = rfDashboard
						    ElseIf scenarioName.ToUpper().StartsWith("BUD") Then
						        targetDashboard = budDashboard

						    End If
						    
						    'Set current dashboard
						    Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						    selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
						    selectionChangedTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = targetDashboard
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
		
		#Region "Set Dashboard Parameters"
		
		Private Sub SetDashboardParameters(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Control dashboard parameters
			
			#Region "Level 1"
			
			If level = 1 Then
				'Set visible admin dashboard if user is admin
				loadDashboardTaskResult.ModifiedCustomSubstVars("p_left_tabs_admin_dashboard") = ""
				If BRApi.Security.Authorization.IsUserInGroup(si,"Horse Group") Then _
					loadDashboardTaskResult.ModifiedCustomSubstVars("p_left_tabs_admin_dashboard") = "010101_Investments_MainNavigation_Main_Left_Tabs_Admin"
				
				'Get available companies depending on security group
				Dim availableCompanies As List(Of Integer) = HelperFunctionsBR.GetCompanyIDsForUser(si)
				
				'Get default forecast and budget scenarios
				Dim defaultForecast As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Investments_Scenario_DefaultForecast")
				Dim defaultBudget As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Investments_Scenario_DefaultBudget")
				
				'Set default company for combobox based on the company ids for user
				If availableCompanies.Contains(0) OrElse availableCompanies.Count = 0 Then
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_DefaultCompany") = "0"
				Else If availableCompanies.Count > 0
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_DefaultCompany") = availableCompanies(0).ToString
				End If
				'Set scenario based on the previous selected dashboard
				If activeDashboardNumber = 3 Then
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Main") = "Planning"
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Planning") = "Forecast"
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Navigation_Header_Right") = "Investments_Forecast_Navigation_Header_Right"
					If Not args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue($"prm_Investments_Scenario").StartsWith("RF") Then _
						loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario") = defaultForecast
				Else If activeDashboardNumber = 4 Then
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Main") = "Planning"
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Planning") = "Budget"
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Navigation_Header_Right") = "Investments_Budget_Navigation_Header_Right"
					If Not args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue($"prm_Investments_Scenario").StartsWith("Budget") Then _
						loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario") = defaultBudget
				Else
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Main") = "Actual"
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario_Planning") = "Actual"
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario") = "Actual"
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Navigation_Header_Right") = "Investments_Actual_Navigation_Header_Right"
				End If
				
			#End Region
			
			#Region "Level 2"
			
			Else If level = 2 Then
				'Control if user is able to modify months depending on scenario and confirmation
				'Get colors
				Dim editableColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Table_Editable")
				Dim NotEditableColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Table_NotEditable")
				Dim confirmedColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Table_Confirmed")
				
				'Get year
				Dim paramYear As String = If(
					loadDashboardTaskResult.ModifiedCustomSubstVars.ContainsKey($"prm_Investments_Year"),
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Year"),
					args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue($"prm_Investments_Year")
				)
				If String.IsNullOrEmpty(paramYear) Then paramYear = "2024"
				'Get scenario
				Dim paramScenario As String = If(
					loadDashboardTaskResult.ModifiedCustomSubstVars.ContainsKey($"prm_Investments_Scenario"),
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_Scenario"),
					args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue($"prm_Investments_Scenario")
				)
				If String.IsNullOrEmpty(paramScenario) Then paramScenario = "Actual"
				'Get company
				Dim paramCompanyString As String = If(
					loadDashboardTaskResult.ModifiedCustomSubstVars.ContainsKey($"prm_ProjectsCompanyNames"),
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_ProjectsCompanyNames"),
					If(
						args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.ContainsKey($"prm_ProjectsCompanyNames"),
						args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue($"prm_ProjectsCompanyNames"),
						"0"
					)
				)
				Dim paramCompany As Integer = If(
					String.IsNullOrEmpty(paramCompanyString) OrElse Not Integer.TryParse(paramCompanyString, Nothing),
					0,
					CInt(paramCompanyString)
				)
				
				'Get if scenario is confirmed
				Dim isScenarioYearConfirmed As Boolean = HelperFunctionsBR.CheckScenarioYearConfirmation(
					si, paramCompany, paramScenario, paramYear
				)
				
				'If no company selected or confirmed scenario year, they can't edit cells
				If paramCompany = 0 OrElse isScenarioYearConfirmed Then
					For i As Integer = 1 To 12
						loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}") = "False"
						loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}_Color") = NotEditableColor
					Next
					'Define last rf. Only select last rf if we are in budget capital or cash.
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_LastRF") = If(
						paramScenario.ToLower.Contains("budget") AndAlso (activeDashboardNumber = 3 OrElse activeDashboardNumber = 4),
						"Last RF: " & HelperFunctionsBR.GetLastRF(
							si,
							If(activeDashboardNumber = 3,
								"dna",
								"cash"
							),
							paramCompany, paramScenario, paramYear
						),
						""
					)
				Else
					'For actual, use the confirmed months, else rf months
					If paramScenario = "Actual" Then
							'Get confirmed months
							Dim dt As DataTable = HelperFunctionsBR.GetConfirmedMonths(si, "cash", paramCompany, paramScenario, paramYear)
							'If no data table, every column is editable
							If dt.Rows.Count < 1 Then
								For i As Integer = 1 To 12
									loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}") = "True"
									loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}_Color") = editableColor
								Next
							Else
								For i As Integer = 1 To 12
									If dt.Rows(0)($"M{i}").ToString = "True" Then
										loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}") = "False"
										loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}_Color") = confirmedColor
									Else
										loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}_Color") = editableColor
									End If
								Next
							End If
					Else
						'Define last rf. Only select last rf if we are in budget capital or cash.
						loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_LastRF") = If(
							paramScenario.ToLower.Contains("budget") AndAlso (activeDashboardNumber = 3 OrElse activeDashboardNumber = 4),
							"Last RF: " & HelperFunctionsBR.GetLastRF(
								si,
								If(activeDashboardNumber = 3,
									"dna",
									"cash"
								),
								paramCompany, paramScenario, paramYear
							),
							""
						)
						
						'Get forecast month based on scenario
						Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, paramScenario)
						For i As Integer = 1 To 12
							If i <= paramForecastMonth Then
								loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}") = "False"
								loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}_Color") = NotEditableColor
							Else
								loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}") = "True"
								loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Investments_IsEditable_{i}_Color") = editableColor
							End If
						Next
					End If
				End If
				
				#End Region
				
			End If
			
		End Sub
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
