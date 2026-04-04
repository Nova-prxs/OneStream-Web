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

' Workspace.PlantsReporting.PlantsReporting.Main_Navigation
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DefaultNavigation
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object			
			
			Try
				Select Case args.FunctionType
			
					Case Is = DashboardExtenderFunctionType.LoadDashboard				
											
						If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
							
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
							
							' CONFIGURAR LA PANTALLA DE INICIO
							args = InactiveActive(si, args, "load","PLT_LoadFiles_prm_colorLeftLevel1_1", "PLT_LoadFiles_prm_colorLeftLevel2_1","PLT_LoadFiles_prm_colorRight1_1")
							args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_Dynamic_ShowLeft") = "PLT_LoadFiles_0_Left"
							args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_Dynamic_Left_Level2") = "PLT_LoadFiles_Left_Level2_000_DynRep"
							args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_Dynamic_Right_Level3") = "PLT_LoadFiles_Right_Level3_000_DynRep"
							args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_dynamicContent") = "PLT_LoadFiles_1"
							args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("table") = "XFC_PLT_FACT_Production"										
							
							loadDashboardTaskResult.ModifiedCustomSubstVars = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun							
							Dim monthCheck As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_month_closing")
							
							' FACTORIA SELECTOR
							If (args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_PLT_factory") = "")	
								Dim sFirstValue As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_factory_default")
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_PLT_factory") = sFirstValue																
							End If			
								Dim sSecondtValue As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_factory_default")
							
							' SCENARIO SELECTOR
							Dim scenarioSelected As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_scenario")	
							
							#Region "Parametros Admin"
							
							' PARÁMETROS DEL ADMINISTRADOR
							Dim adminParameters As New Dictionary(Of String, String) From {
								{"PLT_LoadFiles_prm_ClosingMonth", 			"prm_PLT_month_closing"}, 
								{"PLT_LoadFiles_prm_ScenarioFcst", 			"prm_PLT_scenario_forecast"}, 
								{"PLT_LoadFiles_prm_ScenarioFcst_Month", 	"prm_PLT_month_first_forecast"}, 
								{"PLT_LoadFiles_prm_ScenarioBdgt", 			"prm_PLT_scenario_budget"}
							}			
							
							Dim dicStatusState As New Dictionary(Of String, String) From {
								{"PLT_LoadFiles_prm_ClosingMonth", "PLT_LoadFiles_prm_ClosingMonth_Status"},
								{"PLT_LoadFiles_prm_ScenarioFcst", "PLT_LoadFiles_prm_ScenarioFcst_Status"},
								{"PLT_LoadFiles_prm_ScenarioFcst_Month", "Nothing"},
								{"PLT_LoadFiles_prm_ScenarioBdgt", 	"PLT_LoadFiles_prm_ScenarioBdgt_Status"}
							}
							
							For Each adminParameterKey As String In adminParameters.Keys
								Dim scenario As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.{adminParameters(adminParameterKey)}")
								Dim month As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_month_closing")
								Dim year As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_year_N")
																								
								scenario = If(scenario.Length <= 2,"Actual", scenario)
								Dim dateMod = If(scenario.Contains("Bud"), $"{CInt(year)+1}-01-01", If (scenario = "Actual", $"{year}-{month}-01",$"{year}-01-01"))
								
								
								'Falta ejecutar la sql para saber el estado de abierto o cerrado para cada escenario
								Dim sql As String = $"
									SELECT A.status
								
									FROM XFC_PLT_AUX_Scenario_Status A
									
									WHERE 1=1
										AND A.scenario = '{scenario}' 
										AND A.date = '{dateMod}' 
								"		
								Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								
									Dim dtTemp As DataTable = BRApi.Database.ExecuteSql(dbcon, sql, True)
								 	Dim action As String = If(dtTemp.Rows.Count > 0, dtTemp.Rows(0)(0), "Lock")
									
									brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, dicStatusState(adminParameterKey), If(action="Lock","redround.png", "greenround.png"))
								
								End Using
								
								'Set default value for each scenrio and month
								Dim defaultValue As String = $"|!{adminParameters(adminParameterKey)}!|"
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, adminParameterKey, defaultValue)
								
							Next
							
							#End Region

'							' PARÁMETROS DEL ADMINISTRADOR
'							Dim adminParameters As New Dictionary(Of String, String) From {
'								{"PLT_LoadFiles_prm_ClosingMonth", 			"prm_PLT_month_closing"}, 
'								{"PLT_LoadFiles_prm_ScenarioFcst", 			"prm_PLT_scenario_forecast"}, 
'								{"PLT_LoadFiles_prm_ScenarioFcst_Month", 	"prm_PLT_month_first_forecast"}, 
'								{"PLT_LoadFiles_prm_ScenarioBdgt", 			"prm_PLT_scenario_budget"}
'							}			
							
'							For Each adminParameterKey As String In adminParameters.Keys
								
'								Dim defaultValue As String = $"|!{adminParameters(adminParameterKey)}!|"
'								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, adminParameterKey, defaultValue)
								
'							Next
							
							Return loadDashboardTaskResult
							
						End If

					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
				
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult With {.ChangeCustomSubstVarsInDashboard = True}
						Dim scenarioLabel = args.NameValuePairs("label")
						
						args = InactiveActive(si, args, "nav", args.NameValuePairs("leftActive1"), args.NameValuePairs("leftActive2"), args.NameValuePairs("upActive"), args.NameValuePairs("left"))							
						
						args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_Dynamic_Left_Level2") = args.NameValuePairs("left")								
						args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_Dynamic_Right_Level3") = args.NameValuePairs("up")								
						args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_dynamicContent") = args.NameValuePairs("content")
						
'						If scenarioLabel = "Actual" Then
							
'							args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_scenario") = "Actual"
							
'						Else If scenarioLabel = "One Shot Files" Then
							
'							args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_scenario") = "Actual, Budget"
							
'						Else If scenarioLabel = "Forecast/Budget" Then							
							
'							args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_scenario") = "RF03"
						
'						End If
						
						#Region "Parametros ADMIN"
						
						' SCENARIO SELECTOR
							Dim scenarioSelected As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_scenario")	
							

							' PARÁMETROS DEL ADMINISTRADOR
							Dim adminParameters As New Dictionary(Of String, String) From {
								{"PLT_LoadFiles_prm_ClosingMonth", 			"prm_PLT_month_closing"}, 
								{"PLT_LoadFiles_prm_ScenarioFcst", 			"prm_PLT_scenario_forecast"}, 
								{"PLT_LoadFiles_prm_ScenarioFcst_Month", 	"prm_PLT_month_first_forecast"}, 
								{"PLT_LoadFiles_prm_ScenarioBdgt", 			"prm_PLT_scenario_budget"}
							}			
							
							Dim dicStatusState As New Dictionary(Of String, String) From {
								{"PLT_LoadFiles_prm_ClosingMonth", "PLT_LoadFiles_prm_ClosingMonth_Status"},
								{"PLT_LoadFiles_prm_ScenarioFcst", "PLT_LoadFiles_prm_ScenarioFcst_Status"},
								{"PLT_LoadFiles_prm_ScenarioFcst_Month", "Nothing"},
								{"PLT_LoadFiles_prm_ScenarioBdgt", 	"PLT_LoadFiles_prm_ScenarioBdgt_Status"}
							}
							
							For Each adminParameterKey As String In adminParameters.Keys
								Dim scenario As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.{adminParameters(adminParameterKey)}")
								Dim month As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_month_closing")
								Dim year As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_year_N")
																
								scenario = If(scenario.Length <= 2,"Actual", scenario)
								Dim dateMod = If(scenario.Contains("Bud"), $"{CInt(year)+1}-01-01", If (scenario = "Actual", $"{year}-{month}-01",$"{year}-01-01"))
								
								
								'Falta ejecutar la sql para saber el estado de abierto o cerrado para cada escenario
								Dim sql As String = $"
									SELECT A.status
								
									FROM XFC_PLT_AUX_Scenario_Status A
									
									WHERE 1=1
										AND A.scenario = '{scenario}' 
										AND A.date = '{dateMod}' 
								"		
								
								Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									Dim dtTemp As DataTable = BRApi.Database.ExecuteSql(dbcon, sql, True)
								 	Dim action As String = If(dtTemp.Rows.Count > 0, dtTemp.Rows(0)(0), "Lock")
									
									brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, dicStatusState(adminParameterKey), If(action="Lock","redround.png", "greenround.png"))
								
								End Using
								
								'Set default value for each scenrio and month
								Dim defaultValue As String = $"|!{adminParameters(adminParameterKey)}!|"
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, adminParameterKey, defaultValue)
								
							Next
							
							#End Region
							
        				selectionChangedTaskResult.ModifiedCustomSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars	
						
						Return selectionChangedTaskResult

				End Select
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

#Region "Function ActiveInactive"
		Public Function InactiveActive(ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByVal type As String ,ByVal leftActive1 As String,ByVal leftActive2 As String, ByVal upActive As String, Optional left As String = "") As DashboardExtenderArgs

			Dim activeLeft = "White"
			Dim inactiveLeft = "Black"
			Dim inactiveRight = "White"
			Dim activeRight = "Black"

	#Region "Load"
			If type = "load" Then
				' Para Cambiar los estilos durante la carga				
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel1_1") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel1_2") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel1_3") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel1_4") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel1_5") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel1_6") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel1_7") = inactiveLeft
				
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel2_1") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel2_2") = inactiveLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorLeftLevel2_3") = inactiveLeft
				
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorRight1_1") = inactiveRight
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorRight1_2") = inactiveRight
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_colorRight1_3") = inactiveRight
				
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun(leftActive1) = activeLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun(leftActive2) = activeLeft
				args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun(upActive) = activeRight
				
				If left.Contains("000") Then
					args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_label2Title") = ""
				Else
					args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun("PLT_LoadFiles_prm_label2Title") = ""
				End If				
	#End Region
	
	#Region "Button"
			Else
				' Cambiar los estilos durante la navegación
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel1_1") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel1_2") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel1_3") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel1_4") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel1_5") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel1_6") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel1_7") = inactiveLeft
				
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel2_1") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel2_2") = inactiveLeft
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorLeftLevel2_3") = inactiveLeft
				
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorRight1_1") = inactiveRight
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorRight1_2") = inactiveRight
				args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_colorRight1_3") = inactiveRight
				
				args.SelectionChangedTaskInfo.CustomSubstVars(leftActive1) = activeLeft
				args.SelectionChangedTaskInfo.CustomSubstVars(leftActive2) = activeLeft
				args.SelectionChangedTaskInfo.CustomSubstVars(upActive) = activeRight
				
				If left.Contains("000") Then
					args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_label2Title") = ""
				Else
					args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_label2Title") = ""
				End If
	#End Region
	
			End If
#End Region

			Return args
			
		End Function

	End Class
	
End Namespace
