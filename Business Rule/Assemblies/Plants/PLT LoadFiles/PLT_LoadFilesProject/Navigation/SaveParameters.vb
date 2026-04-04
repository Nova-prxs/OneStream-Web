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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.SaveParameters
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
		
			Try
				Select Case args.FunctionType
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
		
		#Region "Save All Parameters"
		
						If args.FunctionName.XFEqualsIgnoreCase("SaveAllParameters") Then
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult With {.ChangeCustomSubstVarsInDashboard = True}
							
							' PARÁMETROS ORIGINALES
							Dim sClosingMonth As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_month_closing")
							Dim sScenarioFcst As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_scenario_forecast")
							Dim sScenarioFcst_Month As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_month_first_forecast")
							Dim sScenarioBdgt As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_scenario_budget")
							
							' PARÁMETROS ADMINISTRADOR							
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
							
							For Each paramAdminKey In adminParameters.Keys
								
								Dim newValue As String = args.SelectionChangedTaskInfo.CustomSubstVars(paramAdminKey)
								Dim originalParameterName As String = adminParameters(paramAdminKey)
								
								' Set de el parámetro Original
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, $"Plants.{originalParameterName}", newValue)
								
								' Set de el parámetro Dashboard
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, paramAdminKey, $"|!{originalParameterName}!|" )
								
								
								'SQL para saber el estado de abierto o cerrado para cada escenario
								Dim scenario As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.{adminParameters(paramAdminKey)}")
								Dim month As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_month_closing")
								Dim year As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_year_N")
								
								scenario = If(scenario.Length <= 2,"Actual", scenario)
								Dim dateMod = If(scenario.Contains("Bud"), $"{CInt(year)+1}-01-01", If (scenario = "Actual", $"{year}-{month}-01",$"{year}-01-01"))
								
								
								
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
									
									brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, dicStatusState(paramAdminKey), If(action="Lock","redround.png", "greenround.png"))
								
								End Using
								
							Next
							
		#End Region					
							
		#Region "Lock/Unlock Scenario"
		
						Else If args.FunctionName.XFEqualsIgnoreCase("LockUnlock") Then
							
							Dim scenario As String = args.NameValuePairs.GetValueOrEmpty("scenario")
							Dim action As String = args.NameValuePairs.GetValueOrDefault("action")
							
							Dim dicScenarioParam As New Dictionary(Of String, List(Of String)) From {
								{"Actual", 		New List(Of String ) From {"PLT_LoadFiles_prm_ClosingMonth","PLT_LoadFiles_prm_ClosingMonth_Status"}},
								{"Forecast", 	New List(Of String ) From {"PLT_LoadFiles_prm_ScenarioFcst","PLT_LoadFiles_prm_ScenarioFcst_Status"}},
								{"Budget", 		New List(Of String ) From {"PLT_LoadFiles_prm_ScenarioBdgt","PLT_LoadFiles_prm_ScenarioBdgt_Status"}}
							}
							
							' PARÁMETROS ORIGINALES
							Dim sClosingMonth As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_month_closing")
							Dim sScenarioFcst As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_scenario_forecast")
							Dim sScenarioFcst_Month As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_month_first_forecast")
							Dim sScenarioBdgt As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_scenario_budget")
							Dim paramYear As String = brapi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Plants.prm_PLT_year_N")
							
							' PARÁMETROS MODIFICAR
							Dim descriptionMod As String = If(action="Lock", $"Closed by {si.UserName} at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", $"Opened by {si.UserName} at {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}")
							Dim actualDate As DateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
							Dim scenarioMod As String = If(scenario="Forecast", sScenarioFcst, If(scenario="Budget",sScenarioBdgt,"Actual"))
							Dim dateMod As String = If(scenario="Forecast", $"{paramYear}-01-01", If(scenario="Budget", $"{paramYear+1}-01-01", $"{paramYear}-{sClosingMonth}-01"))
							
							Dim sqlAction As String = $"
							
								MERGE XFC_PLT_AUX_Scenario_Status as tgt
								USING (VALUES ('{scenarioMod}','{dateMod}','{descriptionMod}','{action}'))
									AS src (scenario, date, description, status)
								ON tgt.scenario = src.scenario
								AND ((
										src.scenario = 'Actual'
										AND YEAR(tgt.date) = YEAR(src.date)
										AND MONTH(tgt.date) = MONTH(src.date)
									)
									OR
									(
										src.scenario <> 'Actual'
										AND YEAR(tgt.date)=YEAR(src.date)
									))
							
								WHEN MATCHED THEN
									UPDATE SET
										tgt.status      = src.status,
        								tgt.description = src.description,
        								tgt.date       = src.date 
							
								WHEN NOT MATCHED THEN
    								INSERT (scenario,date,status,description)
    								VALUES (src.scenario,src.date,src.status,src.description);		
							
							"
							'SQL para saber el estado de abierto o cerrado para cada escenario
							Dim sql As String = $"
								SELECT A.status
							
								FROM XFC_PLT_AUX_Scenario_Status A
								
								WHERE 1=1
									AND A.scenario = '{scenarioMod}' 
									AND A.date = '{dateMod}'											
							"		
								
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								BRApi.Database.ExecuteSql(dbcon, sqlAction, True)
								Dim dtTemp As DataTable = BRApi.Database.ExecuteSql(dbcon, sql, True)
							 	Dim action_NEW As String = If(dtTemp.Rows.Count > 0, dtTemp.Rows(0)(0), "Lock")
								
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, dicScenarioParam(scenario)(1), If(action_NEW="Lock","redround.png", "greenround.png"))
							
							End Using
							
							Return Nothing
		#End Region
		
		#Region "Warning Backup"
							
						Else If args.FunctionName.XFEqualsIgnoreCase("WarningBackup") Then
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							
							Dim sql_comprobacion As String = "
							    SELECT TOP 1 date
								FROM XFC_PLT_AUX_Log
								WHERE 1=1
									AND object='XFC_PLT_HIER_Nomenclature_Date'
									AND action='BackUp'
								ORDER BY date DESC"
							
							Dim parametroTiempo As DateTime
							Dim dt As DataTable
							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        dt = brapi.Database.ExecuteSql(dbcon, sql_comprobacion, True)
						    End Using
							
							parametroTiempo = If(dt.Rows.Count > 1, Convert.ToDateTime(dt.Rows(0)("date")),New DateTime(1900, 1, 1))
							
							Dim diferencia As TimeSpan = DateTime.Now - parametroTiempo
							
						    If diferencia.TotalDays < 20 Then
						        ' Si la diferencia es menor que 20 días
						        selectionChangedTaskResult.Message = "No han pasado 20 días desde la última ejecución."
								Return selectionChangedTaskResult
						    Else
						        ' Si la diferencia es mayor o igual a 20 días
						        selectionChangedTaskResult.Message = "Completado."
	
								Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
								
								Dim sql As String = $"
								
									DROP TABLE IF EXISTS XFC_PLT_MASTER_CostCenter_Nature_BackUp;
								
									SELECT * INTO XFC_PLT_MASTER_CostCenter_Nature_BackUp
									FROM XFC_PLT_MASTER_CostCenter_Nature;
								
									DROP TABLE IF EXISTS XFC_PLT_MASTER_CostCenter_Hist_BackUp;
								
									SELECT * INTO XFC_PLT_MASTER_CostCenter_Hist_BackUp
									FROM XFC_PLT_MASTER_CostCenter_Hist;
								
									DROP TABLE IF EXISTS XFC_PLT_HIER_Nomenclature_Date_BackUp;
							
									SELECT * INTO XFC_PLT_HIER_Nomenclature_Date_BackUp
									FROM XFC_PLT_HIER_Nomenclature_Date;
							
									DROP TABLE IF EXISTS XFC_PLT_HIER_Nomenclature_Date_Report_BackUp;
									
									SELECT * INTO XFC_PLT_HIER_Nomenclature_Date_Report_BackUp
									FROM XFC_PLT_HIER_Nomenclature_Date_Report;
								
								"
								Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									brapi.Database.ExecuteSql(dbcon, sql, True)
								End Using
								
								uti_sharedqueries.update_Log(si, "All", "All", "XFC_PLT_HIER_Nomenclature_Date", "BackUp")
								
							End If
							
							Return selectionChangedTaskResult
						
						End If
		#End Region
		
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function	
		
	End Class
End Namespace
