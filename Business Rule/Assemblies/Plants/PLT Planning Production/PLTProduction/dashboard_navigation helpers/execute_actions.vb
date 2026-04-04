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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.execute_actions
	
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object				
		
			Try
				Select Case args.FunctionType
	
					Case Is = DashboardExtenderFunctionType.LoadDashboard

					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
							
						#Region "UPLOAD Budget"
						If args.FunctionName.XFEqualsIgnoreCase("Upload_XLXS_Budget_ActivityTimes") Then
							Dim factory As String = args.NameValuePairs("factoryOS")
							
							Me.ImportXLSXFileProductionBudget(si, factory)
							
							Return Nothing
						#End Region								

						#Region "Activity Index Correction Popup"							
						
						Else If args.FunctionName.XFEqualsIgnoreCase("activity_index_popup") Then																		
						
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()							
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True	
							
'							Dim sCurrentDashboard As String = args.NameValuePairs("prm_CurrentDashboard")	
'							BRApi.ErrorLog.LogMessage(si, "sCurrentDashboard: " & sCurrentDashboard)
							
							Dim sScenario As String = args.NameValuePairs("scenario")		
							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")		
							Dim sYear As String = args.NameValuePairs("year")	
							
'							BRApi.ErrorLog.LogMessage(si, "scenario: " & sScenario)
'							BRApi.ErrorLog.LogMessage(si, "year: " & sYear)	

							brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_PLTProduction_scenario_selected_PopUp",  sScenario)
							brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_PLTProduction_scenario_ref_PopUp",  sScenarioRef)
							brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_PLTProduction_year_selected_PopUp",  sYear)
							
							Dim sRowSelected As String = args.NameValuePairs("row_selected")
							
							If (sRowSelected.Equals("")) Then
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = "A row must be selected."								
								Return selectionChangedTaskResult
							Else	
'								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_CurrentDashboard", sCurrentDashboard)
'								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTProduction_scenario_selected", sScenario)
'								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTProduction_year_selected", sYear)
								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLT_factory", args.NameValuePairs("factory"))														
								
								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTProduction_row_selected_month", sRowSelected.Split(":")(0))
								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTProduction_row_selected_gm", sRowSelected.Split(":")(1))
								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTProduction_row_selected_time", sRowSelected.Split(":")(2))
								
								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTProduction_value", String.Empty)							
								selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTProduction_comment", String.Empty)									
								
								Return selectionChangedTaskResult
							End If		
							
'							BRApi.ErrorLog.LogMessage(si, "scenario: " & sScenario)
							
						#End Region																			
													
						#Region "Activity Index Correction Save"		
						
						Else If args.FunctionName.XFEqualsIgnoreCase("activity_index_save") Then
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()							
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True																										
							selectionChangedTaskResult.ShowMessageBox = True
													
'							Dim sScenario As String = args.NameValuePairs("scenario")		
'							Dim sYear As String = args.NameValuePairs("year")		
'							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")	
							
							Dim sScenario As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLTProduction_scenario_selected_PopUp")		
							Dim sScenarioRef As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLTProduction_scenario_ref_PopUp")	
							Dim sYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLTProduction_year_selected_PopUp")
							
							Dim sFactory As String = args.NameValuePairs("factory")							
							Dim sMonth As String = args.NameValuePairs("month")	
							Dim sGM As String = args.NameValuePairs("gm")																
							Dim sTime As String = args.NameValuePairs("time")	
							Dim sComment As String = args.NameValuePairs("comment")	
							Dim sValue As String = args.NameValuePairs("value")					

'							BRApi.ErrorLog.LogMessage(si, "sScenario: " & sScenario)
'							BRApi.ErrorLog.LogMessage(si, "sYear: " & sYear)
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear, sMonth)) Then																														
								
								Dim sql As String = $"
								
								-- 1. Previous clear
								
								DELETE FROM XFC_PLT_AUX_ActivityIndex_Adjust
								WHERE 1=1
								AND scenario = '{sScenario}' 
								AND scenario_ref = '{sScenarioRef}'
								AND YEAR(date) = {sYear}							
								AND MONTH(date) = {sMonth}	
								AND id_factory = '{sFactory}'
								AND id_averagegroup = '{sGM}'
								AND uotype = '{sTime}'
	
								-- 2. Initialization forecast months
								
								INSERT INTO XFC_PLT_AUX_ActivityIndex_Adjust
								(
									scenario
									,scenario_ref
									,Date
									,id_factory
									,id_averagegroup
									,uotype
									,value
									,comment
								)
								VALUES
								(
									'{sScenario}' 
									,'{sScenarioRef}' 
									,DATEFROMPARTS({sYear},{sMonth},1)
									,'{sFactory}'
									,'{sGM}'
									,'{sTime}'
									,{sValue}
									,'{sComment}'
								)							
								"
								
'								BRApi.ErrorLog.LogMessage(si, sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using		
								
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(Si, False, "prm_PLTProduction_value", String.Empty)
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Activity Index modified succesfully."		
							
							Else
								
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(Si, False, "prm_PLTProduction_value", String.Empty)
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return selectionChangedTaskResult									
							
						#End Region
						
						#Region "Plan - Calc Activity"		
						
						Else If args.FunctionName.XFEqualsIgnoreCase("PLAN_calculate_activity") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ShowMessageBox = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then													
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)							
								
								Dim sql As String = $"													
										
								-- 1. Previous clear
								
								DELETE FROM XFC_PLT_FACT_Production
								WHERE 1=1
								AND scenario = '{sScenario}'
								AND YEAR(date) = {sYear}
								AND id_factory = '{sFactory}'	
								AND MONTH(date) >= {sMonthFirst};
								
								-- 2. Insert closed months												
	
								INSERT INTO XFC_PLT_FACT_Production
									( scenario
									, date
									, id_factory
									, id_costcenter
									, id_averagegroup
									, id_workcenter
									, id_product
									, uotype
									, volume
									, allocation_taj
									, activity_taj
									, allocation_tso
									, activity_tso )							
										
								SELECT
								    A.scenario AS scenario
									, DATEFROMPARTS({sYear}, MONTH(A.date), 1) AS date
								    , A.id_factory AS id_factory
								    , A.id_costcenter AS id_costcenter
								    , A.id_averagegroup AS id_averagegroup
									, -1 AS id_workcenter
								    , A.id_product AS id_product
								    , ISNULL(B.uotype, B2.uotype) AS time
								    , A.value AS volume
									, ISNULL(B.value, B2.value) / 60 AS allocation_taj
									, A.value * ISNULL(B.value, B2.value) / 60 AS activity_taj
									, NULL AS allocation_tso
									, NULL AS activity_tso	                 							           
								
							    FROM (	SELECT scenario, id_factory, date, id_product, id_costcenter, id_averagegroup, SUM(value) AS value
										FROM (
								
										SELECT scenario, id_factory, date, id_product, id_costcenter, id_averagegroup, value
										FROM XFC_PLT_AUX_Production_Planning_Volumes
										WHERE scenario = '{sScenario}'
										AND YEAR(date) = {sYear}
										AND id_factory = '{sFactory}'
										AND MONTH(date) >= {sMonthFirst}
										AND value <> 0
								
										UNION ALL
								
										SELECT scenario, id_factory, date, id_product, id_costcenter, id_averagegroup, value 
										FROM XFC_PLT_AUX_Production_Planning_Volumes_Dist
										WHERE scenario = '{sScenario}'
										AND YEAR(date) = {sYear}
										AND id_factory = '{sFactory}'	
										AND value <> 0
								
										) VOL
										GROUP BY scenario, id_factory, date, id_product, id_costcenter, id_averagegroup
									) A									
								
								-- Actual times
								LEFT JOIN XFC_PLT_AUX_Production_Planning_Times B
									ON A.date = B.date
									AND A.scenario = B.scenario
									AND A.id_factory = B.id_factory
									AND (A.id_costcenter = B.id_costcenter OR B.id_costcenter = '-1') 
									AND A.id_averagegroup = B.id_averagegroup
							        AND A.id_product = B.id_product			
								
								-- Reference times
								LEFT JOIN XFC_PLT_AUX_Production_Planning_Times B2
									ON A.date = B2.date
									AND A.scenario = B2.scenario + '_Ref'
									AND A.id_factory = B2.id_factory
									AND (A.id_costcenter = B2.id_costcenter OR B.id_costcenter = '-1')
									AND A.id_averagegroup = B2.id_averagegroup
							        AND A.id_product = B2.id_product									
													
								WHERE B.value IS NOT NULL OR B2.value IS NOT NULL
										
								ORDER BY B.uotype, B2.uotype, id_averagegroup, id_costcenter, id_product, MONTH(A.date)								
									
								"
											
'								BRApi.ErrorLog.LogMessage(si,"Insert: "& sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
							
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Calc activity completed."
								
							Else
								
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.Message = "Invalid scenario or period."
							
							End If
							
							uti_sharedqueries.update_FactVTU_Report(si, sScenario, sYear, "12")	
							
							Return selectionChangedTaskResult							
							
						#End Region
						
						#Region "Plan - Initialize Times"		
						
						Else If args.FunctionName.XFEqualsIgnoreCase("PLAN_initialization_times") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ShowMessageBox = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then																										
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								Dim sYearInit As String = If (sScenario.StartsWith("RF"), sYear, (CInt(sYear)-1).ToString)
								
								Dim sql As String = $"													
										
								-- 1. Previous clear
								
								DELETE FROM XFC_PLT_AUX_Production_Planning_TImes
								WHERE 1=1
								AND ((scenario = '{sScenario}' AND MONTH(date) >= {sMonthFirst}) OR scenario = '{sScenario}_Ref')
								AND YEAR(date) = {sYear}
								AND id_factory = '{sFactory}'
								"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
											
								Dim sScenarioInit As String = If (sScenario.StartsWith("RF"), "Actual", sScenarioRef)
								Dim sMonthClosing As String = args.NameValuePairs("month_closing")
								Dim sMonthInit As String = If (sScenario.StartsWith("RF"), (CInt(sMonthClosing)-1).ToString, "12")
								Dim sFilterMonthInitRef As String = If (sScenario.StartsWith("RF"), "", "AND MONTH(date) = 12")
								
'								' Cambiamos esto para el RF06 para que coja Abril en lugar de mayo
								'Dim sMonthLastClosing As String = (CInt(sMonthFirst)-1).ToString							
					
								For iMonth As Integer = sMonthFirst To 12
							 
									sql = $"
									
									-- 2. Initialization Opened Months								
									
									INSERT INTO XFC_PLT_AUX_Production_Planning_Times
									(
										scenario
										,Date
										,id_factory
										,id_costcenter
										,id_averagegroup
										,id_product
										,comment
										,uotype
										,value
									)
									
									SELECT 
										'{sScenario}' AS scenario
										,DATEFROMPARTS({sYear},{iMonth.ToString},1) As Date
										,F.id_factory AS id_factory
										,F.id_costcenter AS id_costcenter
										,F.id_averagegroup AS id_averagegroup
										,F.id_product AS id_product
										,NULL AS comment
										,F.uotype as uotype	
										,SUM(F.activity_taj) / NULLIF(SUM(F.volume),0) * 60 AS allocation
																
									FROM XFC_PLT_FACT_Production F
									WHERE scenario = '{sScenarioInit}'							
									AND YEAR(date) = {sYearInit}
									AND MONTH(date) = {sMonthInit} 		
									AND id_factory = '{sFactory}'
							
									/* ------------------------------------------------
										 09/09/2025
										 Comentado para que si que tenga en cuenta los registros con volumen 0 
										 añadidos para corregir la actividad. 
										 Portugal S4H
									------------------------------------------------ */				
								
									AND volume <> 0 
									AND allocation_taj <> 0				
						
									GROUP BY 
										F.id_factory
										,F.id_costcenter
										,F.id_averagegroup
										,F.id_product				
										,F.uotype									
									
									"
									
									Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
										BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
									End Using	
									
								Next
								
'								BRApi.ErrorLog.LogMessage(si, sql)
									
								Dim sqlRef As String = String.Empty
								
								For iMonth As Integer = 1 To 12
							 
									sqlRef = $"									
									
									-- REFERENCE Times
									
									INSERT INTO XFC_PLT_AUX_Production_Planning_Times
									(
										scenario
										,Date
										,id_factory
										,id_costcenter
										,id_averagegroup
										,id_product
										,comment
										,uotype
										,value
									)
									
									SELECT 
										'{sScenario}' + '_Ref' AS scenario
										,DATEFROMPARTS({sYear},{iMonth.ToString},1) As Date
										,F.id_factory AS id_factory
										,F.id_costcenter AS id_costcenter
										,F.id_averagegroup AS id_averagegroup
										,F.id_product AS id_product
										,NULL AS comment
										,F.uotype as uotype	
										,SUM(F.activity_taj) / NULLIF(SUM(F.volume),0) * 60 AS allocation
																
									FROM XFC_PLT_FACT_Production F
									WHERE scenario = '{sScenarioRef}'							
									AND YEAR(date) = {sYearInit}									
									AND MONTH(date) = {iMonth.ToString()}	
									AND id_factory = '{sFactory}'
									AND id_product <> '-1'
							
									/* ------------------------------------------------
										 09/09/2025
										 Comentado para que si que tenga en cuenta los registros con volumen 0 
										 añadidos para corregir la actividad. 
										 Portugal S4H
									------------------------------------------------ */				
								
									-- AND volume <> 0 
									-- AND allocation_taj <> 0				
						
									GROUP BY 
										F.id_factory
										,F.id_costcenter
										,F.id_averagegroup
										,F.id_product				
										,F.uotype												
									"
																	
									Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
										BRApi.Database.ExecuteActionQuery(dbConn, sqlRef,True, True)
									End Using
									
								Next	
								
								uti_sharedqueries.update_Log(si, sScenario, sFactory, "XFC_PLT_AUX_Production_Planning_Times","Init")
								
'								BRApi.ErrorLog.LogMessage(si, sqlRef)
								
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Times successfully initializated."
								
							Else
								
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return selectionChangedTaskResult									
											
							'BRApi.ErrorLog.LogMessage(si,sql)							
							
						#End Region							
						
						#Region "Plan - Initialize Nomenclature"		
						
						Else If args.FunctionName.XFEqualsIgnoreCase("PLAN_initialization_nomenclature") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ShowMessageBox = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then																										
							
								uti_sharedqueries.update_Nomenclature_Planning(si, sScenario, sYear, sFactory, )
								uti_sharedqueries.update_Nomenclature_Report(si, sScenario, sYear, "", "", sFactory)		
								
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Nomenclature successfully initializated."
								
							Else
								
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return selectionChangedTaskResult									
																	
							
						#End Region	
						
						#Region "Plan - Distribute Volume"				
													
						Else If args.FunctionName.XFEqualsIgnoreCase("PLAN_distribute_volume") Then					
									
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()							
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True																										
							selectionChangedTaskResult.ShowMessageBox = True
													
							Dim sScenario As String = args.NameValuePairs("scenario")		
							Dim sYear As String = args.NameValuePairs("year")
							Dim sMonthFirstForecast As String = If (sScenario.StartsWith("RF"),args.NameValuePairs("month_first_forecast"),1)
							
							Dim sFactory As String = args.NameValuePairs("factory")
							
							Dim sYearClosing As String = If (sScenario.StartsWith("RF"),sYear, CInt(sYear)-1.ToString)							
							Dim sMonthClosing As String = args.NameValuePairs("month_closing")
							
							Dim sMonthLastClosing As String = sMonthClosing
							'Cambiar por esto si se cambia de mes durante el ejercicio de RF o BUD
'							Dim sMonthLastClosing As String = (CInt(sMonthLastClosing)-1).ToString	
							
							Dim dateClosing As Date = New Date(CInt(sYearClosing), CInt(sMonthClosing), 1)
							Dim sDateEnd As String = DateAdd(DateInterval.Day, -1, dateClosing).ToString("yyyy-MM-dd")							
							Dim sDateStart As String = DateAdd(DateInterval.Month, -6, dateClosing).ToString("yyyy-MM-dd")													
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then								

								#Region "Existing Products"
							
								Dim sql_Product_Component_Recursivity As String = uti_sharedqueries.AllQueries(si, "Product_Component_Recursivity", sFactory, sMonthLastClosing, sYear, sScenario, "", "", "", "Existing", "", "Local")
								sql_Product_Component_Recursivity = sql_Product_Component_Recursivity.Replace(", Product_Component_Recursivity","WITH Product_Component_Recursivity")
								sql_Product_Component_Recursivity = sql_Product_Component_Recursivity.Replace("SELECT DISTINCT id_product FROM factProduction" _
																											 ,$"SELECT DISTINCT id_product FROM XFC_PLT_AUX_Production_Planning_Volumes WHERE id_factory = '{sFactory}' AND scenario = '{sScenario}' AND YEAR(date) = {sYear} ")
								
								Dim sql As String = $"		 																		
								
									-- PRODUCT - COMPONENTS																	
								
									DELETE FROM XFC_PLT_AUX_Production_Planning_Volumes_Dist
									WHERE id_factory = '{sFactory}'
									AND scenario = '{sScenario}'
									AND YEAR(date) = {sYear};
									
									" & sql_Product_Component_Recursivity & $"	
							
									, Fact_Activity AS (
							
										SELECT 
											id_product
											, id_product + CASE WHEN pond_suffix IS NOT NULL THEN '_' + pond_suffix ELSE '' END AS id_product_gm
											, id_averagegroup
											, id_costcenter
											, SUM(activity_taj) AS activity
								
										FROM XFC_PLT_FACT_Production P
								
										LEFT JOIN XFC_PLT_MASTER_AverageGroup A
											ON P.id_averagegroup = A.id
											AND P.id_factory = A.id_factory
							
										WHERE date BETWEEN '{sDateStart}' AND '{sDateEnd}'
										AND P.id_factory = '{sFactory}'
										AND scenario = 'Actual'
										AND uotype = 'UO1'
										AND ISNULL(activity_taj,0) > 0
								
										GROUP BY 
											id_product
											, pond_suffix
											, id_averagegroup
											, id_costcenter
									)
								
									, Fact_Activity_Product AS (
							
										SELECT 
											id_product_gm
											, SUM(activity) AS activity
								
										FROM Fact_Activity 							
								
										GROUP BY 
											id_product_gm
									)					
								
									, Fact_Final_Volume AS (
								
										SELECT id_product, date, SUM(value) AS volume							
										FROM XFC_PLT_AUX_Production_Planning_Volumes
										WHERE id_factory = '{sFactory}'
										AND scenario = '{sScenario}'
										AND YEAR(date) = {sYear}
										AND MONTH(date) >= {sMonthFirstForecast}
										AND ISNULL(value,0) <> 0
										AND distribution = 1
										GROUP BY id_product, date
									)
							
									INSERT INTO XFC_PLT_AUX_Production_Planning_Volumes_Dist
										(scenario
										, date
										, id_factory
										, id_costcenter
										, id_averagegroup
										, id_product
										, id_product_parent
										, id_product_final
										, value)
								
									SELECT
										'{sScenario}' AS scenario
										, DATEFROMPARTS({sYear},MONTH(V.date),1) AS date
										, '{sFactory}'
										, F.id_costcenter AS id_costcenter								
										, F.id_averagegroup AS id_averagegroup
										, R.id_component AS id_product
										, R.id_product AS id_product_parent
										, R.id_product_final AS id_product_final
										, SUM(CASE WHEN ISNULL(F2.activity,0) <> 0
											THEN V.Volume * exp_coefficient * (F.activity / F2.activity) END) AS volume
									
									FROM Product_Component_Recursivity R
								
									INNER JOIN Fact_Final_Volume V 
										ON R.id_product_final = V.id_product
								
									INNER JOIN Fact_Activity F
										ON R.id_component = F.id_product
									INNER JOIN Fact_Activity_Product F2
										ON F.id_product_gm = F2.id_product_gm	
								
									WHERE exp_coefficient <> 0
								
									GROUP BY 
										DATEFROMPARTS({sYear},MONTH(V.date),1)
										, F.id_costcenter
										, F.id_averagegroup
										, R.id_component
										, R.id_product
										, R.id_product_final
							
									"
							
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'									BRApi.ErrorLog.LogMessage(si, sql)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using		
								
								#End Region
								
								#Region "New Products"
						
								sql_Product_Component_Recursivity = uti_sharedqueries.AllQueries(si, "Product_Component_Recursivity", sFactory, sMonthLastClosing, sYear, sScenario, "", "", "", "New", "", "Local")															
								
								Dim sqlNew As String = sql_Product_Component_Recursivity.Replace(", Distinct_Nomenclature","WITH Distinct_Nomenclature") & $"		 																										
															
									, Fact_GM as (
										SELECT
								
											N.id_product_final
											, N.id_product
											, N.id_component 
											, N.id_averagegroup
											, N.percentage
										
										FROM XFC_PLT_AUX_NewProducts_Simulation N
										WHERE N.scenario = '{sScenario}'	
										AND N.year = {sYear}	
										AND N.id_factory = '{sFactory}'									
									)
								
									, Fact_GM_Product AS (
							
										SELECT 
											id_product_final
											, id_product
											, id_component 
											, SUM(percentage) AS percentage
								
										FROM Fact_GM 							
								
										GROUP BY 
											id_product_final, id_product, id_component 
									)					
								
									, Fact_Final_Volume as (
								
										SELECT id_product, date, SUM(value) AS volume							
										FROM XFC_PLT_AUX_Production_Planning_Volumes
										WHERE id_factory = '{sFactory}'
										AND scenario = '{sScenario}'
										AND YEAR(date) = {sYear}
										AND MONTH(date) >= {sMonthFirstForecast}
										AND ISNULL(value,0) <> 0
										AND distribution = 1
										GROUP BY id_product, date
									)
							
									INSERT INTO XFC_PLT_AUX_Production_Planning_Volumes_Dist
										(scenario
										, date
										, id_factory
										, id_costcenter
										, id_averagegroup
										, id_product
										, id_product_parent
										, id_product_final
										, value)
								
									SELECT
										'{sScenario}' AS scenario
										, DATEFROMPARTS({sYear},MONTH(V.date),1) AS date
										, '{sFactory}'
										, -1							
										, F.id_averagegroup AS id_averagegroup
										, R.id_component AS id_product
										, R.id_product AS id_product_parent
										, R.id_product_final AS id_product_final
										, CASE WHEN ISNULL(F2.percentage,0) <> 0
											THEN V.Volume * exp_coefficient * (F.percentage / F2.percentage) END AS volume
									
									FROM Product_Component_Recursivity R
								
									--CROSS JOIN Fact_Final_Volume V
									INNER JOIN Fact_Final_Volume V 
										ON R.id_product_final = V.id_product
								
									INNER JOIN Fact_GM F
										ON R.id_product_final = F.id_product_final
										AND R.id_product = F.id_product
										AND R.id_component = F.id_component
									INNER JOIN Fact_GM_Product F2
										ON R.id_product_final = F2.id_product_final
										AND R.id_product = F2.id_product
										AND R.id_component = F2.id_component						
							
									"
							
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									'BRApi.ErrorLog.LogMessage(si, sqlNew)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlNew,True, True)
								End Using		
								
								#End Region								
								
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Volume distributed succesfully."		
							
							Else
								
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.Message = "Invalid scenario"
							
							End If
																	
							Return selectionChangedTaskResult								
									
						#End Region
											
						#Region "Plan - Save Volumes - Check/Uncheck All"				
													
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveVolumesCheckAll") Then					
									
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()							
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True																										
							selectionChangedTaskResult.ShowMessageBox = True
													
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")													
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								
								'Save rows in aux table
								Dim sql As String = $"													
										
								UPDATE P
									SET P.distribution = CASE WHEN A.distribution = 0 THEN 1 ELSE 0 END
								FROM XFC_PLT_AUX_Production_Planning_Volumes P
								CROSS JOIN (SELECT TOP 1 distribution
											FROM XFC_PLT_AUX_Production_Planning_Volumes
											WHERE scenario = '{sScenario}'
											AND id_factory = '{sFactory}'
											AND YEAR(date) = {sYear} )	A						
								WHERE P.scenario = '{sScenario}'
								AND P.id_factory = '{sFactory}'
								AND YEAR(P.date) = {sYear}
											
								"
											
								'BRApi.ErrorLog.LogMessage(si,sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using																
								
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Change successfully completed."
								
							Else
								
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return selectionChangedTaskResult								
									
						#End Region							

							
						End If					
							
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						
						#Region "PLAN - Save Volumes"
						
						If args.FunctionName.XFEqualsIgnoreCase("SaveVolumes") Then
		
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")
													
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.CancelDefaultSave = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								
								'Get save data task info
								Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
								
								'Create a datatable from modified rows
								Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
								
								'Declare column mapping dict
								Dim columnMappingDict As New Dictionary(Of String, String) From {
									{"id_costcenter", "id_costcenter"},
									{"id_averagegroup", "id_averagegroup"},
									{"id_product", "id_product"},
									{"product", "product"},
									{"distribution", "distribution"},
									{"M01", "M01"},
									{"M02", "M02"},
									{"M03", "M03"},
									{"M04", "M04"},
									{"M05", "M05"},
									{"M06", "M06"},
									{"M07", "M07"},
									{"M08", "M08"},
									{"M09", "M09"},
									{"M10", "M10"},
									{"M11", "M11"},
									{"M12", "M12"}
								}
								
								'Map and filter columns in DataTable
								dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
'								Throw New Exception(dt.Columns("M11").DataType.FullName)
								'Convert distribution to numbers
								For Each dataRow In dt.Rows
									dataRow("distribution") = If(dataRow("distribution") = "False", 0, 1)
								Next
'								Throw New Exception(UTISharedFunctionsBR.GetFirstDataTableRows(dt, 10))
								
								Dim sqlRaw As String = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Production_Planning_Volumes')
										DROP TABLE XFC_PLT_RAW_Production_Planning_Volumes													
									
									CREATE TABLE XFC_PLT_RAW_Production_Planning_Volumes 
								 	(
									[id_costcenter] varchar(50) ,							
									[id_averagegroup] varchar(50) ,
									[id_product] varchar(50) ,
									[product] varchar(200) ,
									[distribution] bit ,
									[M01] decimal(18,2),
									[M02] decimal(18,2),
									[M03] decimal(18,2),
									[M04] decimal(18,2),
									[M05] decimal(18,2),
									[M06] decimal(18,2),	
									[M07] decimal(18,2),
									[M08] decimal(18,2),
									[M09] decimal(18,2),
									[M10] decimal(18,2),
									[M11] decimal(18,2),
									[M12] decimal(18,2)	
								 	)"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using			
								
								'Save rows in aux table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_Production_Planning_Volumes", dt, "replace")
								
								'Save rows in aux table
								Dim sql As String = $"													
										
								-- 1. Previous clear
							
								DELETE P 
								FROM XFC_PLT_AUX_Production_Planning_Volumes P
								INNER JOIN XFC_PLT_RAW_Production_Planning_Volumes R
									ON (P.id_costcenter = R.id_costcenter OR ISNULL(P.id_costcenter,'-1') = '-1' OR ISNULL(R.id_costcenter,'-1') = '-1')
									AND P.id_averagegroup = R.id_averagegroup
									AND P.id_product = R.id_product
								WHERE scenario = '{sScenario}'
								AND YEAR(date) = {sYear}
								AND id_factory = '{sFactory}'
								AND MONTH(date) >= {sMonthFirst}
							
								-- 2. Insert open months
							
								INSERT INTO XFC_PLT_AUX_Production_Planning_Volumes
								(
									scenario
									,date
									,id_factory
									,id_costcenter
									,id_averagegroup
									,id_product
									,value
								)					
								
								SELECT scenario, DATEFROMPARTS({sYear}, REPLACE(month,'M',''), 1), id_factory, id_costcenter, id_averagegroup, id_product, value
								FROM (
									SELECT
									    '{sScenario}' AS scenario							
									    , '{sFactory}' AS id_factory
									    , ISNULL(id_costcenter,'-1') AS id_costcenter
									    , id_averagegroup
									    , id_product
										, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12							
		
									FROM XFC_PLT_RAW_Production_Planning_Volumes	
									
									WHERE id_product IS NOT NULL
								) AS SourceTable
								UNPIVOT (
								    value FOR month IN (M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12)
								) AS UnpivotedTable
								
								WHERE REPLACE(month,'M','') >= {sMonthFirst}
									
								
								UPDATE P
									SET P.distribution = A.distribution
								FROM XFC_PLT_AUX_Production_Planning_Volumes P
								INNER JOIN XFC_PLT_RAW_Production_Planning_Volumes A
									ON P.id_product = A.id_product
									AND P.id_averagegroup = A.id_averagegroup
									AND P.id_costcenter = A.id_costcenter
								WHERE P.scenario = '{sScenario}'
								AND P.id_factory = '{sFactory}'
								AND YEAR(P.date) = {sYear}
								-- AND id_product IS NOT NULL
											
								"
											
								'BRApi.ErrorLog.LogMessage(si,sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
								
								sqlRaw = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Production_Planning_Volumes')
										DROP TABLE XFC_PLT_RAW_Production_Planning_Volumes																																
								 	"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using	
								
								uti_sharedqueries.update_Log(si, sScenario, sFactory, "XFC_PLT_AUX_Production_Planning_Volumes","Save")
								
								saveDataTaskResult.IsOK = True
								saveDataTaskResult.Message = "Change successfully completed."
								
							Else
								
								saveDataTaskResult.IsOK = False
								saveDataTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return saveDataTaskResult
							
							#End Region				
							
						#Region "PLAN - Save Volumes Distribution"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveVolumesDistribution") Then
		
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")
													
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.CancelDefaultSave = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								
								'Get save data task info
								Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
								
								'Create a datatable from modified rows
								Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
								
								'Declare column mapping dict
								Dim columnMappingDict As New Dictionary(Of String, String) From {
									{"id_costcenter", "id_costcenter"},
									{"id_averagegroup", "id_averagegroup"},
									{"id_product_final", "id_product_final"},									
									{"id_product_parent", "id_product_parent"},
									{"id_product", "id_product"},
									{"product", "product"},									
									{"M01", "M01"},
									{"M02", "M02"},
									{"M03", "M03"},
									{"M04", "M04"},
									{"M05", "M05"},
									{"M06", "M06"},
									{"M07", "M07"},
									{"M08", "M08"},
									{"M09", "M09"},
									{"M10", "M10"},
									{"M11", "M11"},
									{"M12", "M12"}
								}
								
								'Map and filter columns in DataTable
								dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
								
								Dim sqlRaw As String = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Production_Planning_Volumes_Dist')
										DROP TABLE XFC_PLT_RAW_Production_Planning_Volumes_Dist											
									
									CREATE TABLE XFC_PLT_RAW_Production_Planning_Volumes_Dist 
								 	(
									[id_costcenter] varchar(50) ,							
									[id_averagegroup] varchar(50) ,
									[id_product_final] varchar(50) ,								
									[id_product_parent] varchar(50) ,
									[id_product] varchar(50) ,
									[product] varchar(200) ,
									[M01] decimal(18,2),
									[M02] decimal(18,2),
									[M03] decimal(18,2),
									[M04] decimal(18,2),
									[M05] decimal(18,2),
									[M06] decimal(18,2),	
									[M07] decimal(18,2),
									[M08] decimal(18,2),
									[M09] decimal(18,2),
									[M10] decimal(18,2),
									[M11] decimal(18,2),
									[M12] decimal(18,2)	
								 	)"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using							
								
								'Save rows in aux table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_Production_Planning_Volumes_Dist", dt, "replace")
								
								'Save rows in aux table
								Dim sql As String = $"													
										
								-- 1. Previous clear
							
								DELETE P 
								FROM XFC_PLT_AUX_Production_Planning_Volumes_Dist P
								INNER JOIN XFC_PLT_RAW_Production_Planning_Volumes_Dist R
									ON P.id_costcenter = R.id_costcenter
									AND P.id_averagegroup = R.id_averagegroup
									AND P.id_product = R.id_product
									AND P.id_product_parent = R.id_product_parent
									AND P.id_product_final = R.id_product_final
								WHERE scenario = '{sScenario}'
								AND YEAR(date) = {sYear}
								AND id_factory = '{sFactory}'
								AND MONTH(date) >= {sMonthFirst}
							
								-- 2. Insert closed months
							
								INSERT INTO XFC_PLT_AUX_Production_Planning_Volumes_Dist
								(
									scenario
									,date
									,id_factory
									,id_costcenter
									,id_averagegroup
									,id_product
									,id_product_parent
									,id_product_final			
									,value
								)					
								
								SELECT scenario, DATEFROMPARTS({sYear}, REPLACE(month,'M',''), 1), id_factory, id_costcenter, id_averagegroup, id_product, id_product_parent, id_product_final, value
								FROM (
									SELECT
									    '{sScenario}' AS scenario							
									    , '{sFactory}' AS id_factory
									    , id_costcenter
									    , id_averagegroup
									    , id_product
										, id_product_parent
										, id_product_final
										, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12							
		
									FROM XFC_PLT_RAW_Production_Planning_Volumes_Dist
								
								) AS SourceTable
								UNPIVOT (
								    value FOR month IN (M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12)
								) AS UnpivotedTable
								
								WHERE REPLACE(month,'M','') >= {sMonthFirst}
											
								"
											
								'BRApi.ErrorLog.LogMessage(si,sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
								
								uti_sharedqueries.update_Log(si, sScenario, sFactory, "XFC_PLT_RAW_Production_Planning_Volumes_Dist","Save")
								
								sqlRaw = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Production_Planning_Volumes_Dist')
										DROP TABLE XFC_PLT_RAW_Production_Planning_Volumes_Dist																																
								 	"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using	
								
								saveDataTaskResult.IsOK = True
								saveDataTaskResult.Message = "Change successfully completed."
								
							Else
								
								saveDataTaskResult.IsOK = False
								saveDataTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return saveDataTaskResult
							
							#End Region									
						
						#Region "PLAN - Save Times"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveTimes") Then
		
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.CancelDefaultSave = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then							
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								
								'Get save data task info
								Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
								
								'Create a datatable from modified rows
								Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
								
								'Declare column mapping dict
								Dim columnMappingDict As New Dictionary(Of String, String) From {
									{"id_costcenter", "id_costcenter"},
									{"id_product", "id_product"},
									{"product", "product"},		
									{"id_averagegroup", "id_averagegroup"},									
									{"uotype", "uotype"},
									{"M01", "M01"},
									{"M02", "M02"},
									{"M03", "M03"},
									{"M04", "M04"},
									{"M05", "M05"},
									{"M06", "M06"},
									{"M07", "M07"},
									{"M08", "M08"},
									{"M09", "M09"},
									{"M10", "M10"},
									{"M11", "M11"},
									{"M12", "M12"}
								}
								
								'Map and filter columns in DataTable
								dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
								
								Dim sqlRaw As String = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Production_Planning_Times')
										DROP TABLE XFC_PLT_RAW_Production_Planning_Times													
									
									CREATE TABLE XFC_PLT_RAW_Production_Planning_Times 
								 	(
									[id_costcenter] varchar(50) ,							
									[id_product] varchar(50) ,
									[product] varchar(200) ,
									[id_averagegroup] varchar(50) ,								
									[uotype] varchar(50) ,
									[M01] decimal(18,2),
									[M02] decimal(18,2),
									[M03] decimal(18,2),
									[M04] decimal(18,2),
									[M05] decimal(18,2),
									[M06] decimal(18,2),	
									[M07] decimal(18,2),
									[M08] decimal(18,2),
									[M09] decimal(18,2),
									[M10] decimal(18,2),
									[M11] decimal(18,2),
									[M12] decimal(18,2)
								 	)"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using							
								
								'Save rows in raw table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_Production_Planning_Times", dt, "replace")
								
								'Save rows in aux table
								Dim sql As String = $"													
										
								-- 1. Previous clear
							
								DELETE P 
								FROM XFC_PLT_AUX_Production_Planning_Times P
								INNER JOIN XFC_PLT_RAW_Production_Planning_Times R
									ON (P.id_costcenter = R.id_costcenter OR ISNULL(P.id_costcenter,'-1') = '-1' OR ISNULL(R.id_costcenter,'-1') = '-1')
									AND P.id_averagegroup = R.id_averagegroup
									AND P.id_product = R.id_product
									AND P.uotype = R.uotype
								WHERE scenario = '{sScenario}'
								AND YEAR(date) = {sYear}
								AND id_factory = '{sFactory}'
								AND MONTH(date) >= {sMonthFirst}
							
								-- 2. Insert closed months
							
								INSERT INTO XFC_PLT_AUX_Production_Planning_Times
								(
									scenario
									,date
									,id_factory
									,id_costcenter
									,id_averagegroup
									,id_product
									,uotype
									,comment
									,value
								)					
								
								SELECT scenario, DATEFROMPARTS({sYear}, REPLACE(month,'M',''), 1), id_factory, id_costcenter, id_averagegroup, id_product, uotype, NULL, value
								FROM (
									SELECT
									    '{sScenario}' AS scenario							
									    , '{sFactory}' AS id_factory
									    , ISNULL(id_costcenter,-1) AS id_costcenter
									    , id_averagegroup
									    , id_product
										, uotype
										, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12							
		
									FROM XFC_PLT_RAW_Production_Planning_Times	
								
								) AS SourceTable
								UNPIVOT (
								    value FOR month IN (M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12)
								) AS UnpivotedTable
								
								WHERE REPLACE(month,'M','') >= {sMonthFirst}
											
								"
											
'								BRApi.ErrorLog.LogMessage(si,sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)									
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
								
								uti_sharedqueries.update_Log(si, sScenario, sFactory, "XFC_PLT_RAW_Production_Planning_Times","Save")
																
								sqlRaw = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Production_Planning_Times')
										DROP TABLE XFC_PLT_RAW_Production_Planning_Times																														
								 	"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using								
							
								saveDataTaskResult.IsOK = True
								saveDataTaskResult.Message = "Change successfully completed."
								
							Else
								
								saveDataTaskResult.IsOK = False
								saveDataTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return saveDataTaskResult
							
							#End Region
							
						#Region "ALL - Save Nomenclature"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveNomenclature") Then
				
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sProduct As String = args.NameValuePairs("product")
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.CancelDefaultSave = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then							
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								
								'Get save data task info
								Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
								
								'Create a datatable from modified rows
								Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
								
								'Declare column mapping dict
								Dim columnMappingDict As New Dictionary(Of String, String) From {
									{"id_factory", "id_factory"},
									{"id_product_final", "id_product_final"},
									{"id_product", "id_product"},
									{"id_component", "id_component"},		
									{"coefficient", "coefficient"},
									{"exp_coefficient", "exp_coefficient"},
									{"Level", "Level"}
								}
								
								'Map and filter columns in DataTable
								dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
								
								Dim sqlRaw As String = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Nomenclature')
										DROP TABLE XFC_PLT_RAW_Nomenclature													
									
									CREATE TABLE XFC_PLT_RAW_Nomenclature 
								 	(
									[id_factory] varchar(50) ,							
									[id_product_final] varchar(50) ,
									[id_product] varchar(200) ,
									[id_product] varchar(50) ,								
									[coefficient] decimal(18,2),
									[exp_coefficient] decimal(18,2),
									[Level] varchar(50)								
								 	)"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using							
								
								'Save rows in raw table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_Nomenclature", dt, "replace")
								
								'Save rows in aux table
								Dim sql As String = $"		"											
										
'								-- 1. Previous clear
							
'								DELETE P 
'								FROM XFC_PLT_AUX_Production_Planning_Times P
'								INNER JOIN XFC_PLT_RAW_Production_Planning_Times R
'									ON P.id_costcenter = R.id_costcenter
'									AND P.id_averagegroup = R.id_averagegroup
'									AND P.id_product = R.id_product
'									AND P.uotype = R.uotype
'								WHERE scenario = '{sScenario}'
'								AND YEAR(date) = {sYear}
'								AND id_factory = '{sFactory}'
'								AND MONTH(date) >= {sMonthFirst}
							
'								-- 2. Insert closed months
							
'								INSERT INTO XFC_PLT_AUX_Production_Planning_Times
'								(
'									scenario
'									,date
'									,id_factory
'									,id_costcenter
'									,id_averagegroup
'									,id_product
'									,uotype
'									,comment
'									,value
'								)					
								
'								SELECT scenario, DATEFROMPARTS({sYear}, REPLACE(month,'M',''), 1), id_factory, id_costcenter, id_averagegroup, id_product, uotype, NULL, value
'								FROM (
'									SELECT
'									    '{sScenario}' AS scenario							
'									    , '{sFactory}' AS id_factory
'									    , id_costcenter
'									    , id_averagegroup
'									    , id_product
'										, uotype
'										, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12							
		
'									FROM XFC_PLT_RAW_Production_Planning_Times	
								
'								) AS SourceTable
'								UNPIVOT (
'								    value FOR month IN (M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12)
'								) AS UnpivotedTable
								
'								WHERE REPLACE(month,'M','') >= {sMonthFirst}
											
'								"	
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
								
								sqlRaw = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Production_Planning_Times')
										DROP TABLE XFC_PLT_RAW_Production_Planning_Times																														
								 	"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using								
							
								saveDataTaskResult.IsOK = True
								saveDataTaskResult.Message = "Change successfully completed."
								
							Else
								
								saveDataTaskResult.IsOK = False
								saveDataTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return saveDataTaskResult
							
						#End Region
						
						#Region "ALL - Save New Product"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveNewProduct") Then
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							Dim saveDataTaskResult2 As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.CancelDefaultSave = True
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
'							If saveDataTaskInfo.EditedDataRows.Count > 0 Then
								
'								brapi.ErrorLog.LogMessage(si, "Elimi - " & saveDataTaskInfo.EditedDataRows.Item(0).InsertUpdateOrDelete.ToString)
'								saveDataTaskResult.CancelDefaultSave = False

'								saveDataTaskResult.IsOK = True
'								saveDataTaskResult.Message = "Change successfully completed."					
																
'								saveDataTaskResult
								
'							End If
							
							'Create a datatable from modified rows
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
							
							'Declare column mapping dict
							Dim columnMappingDict As New Dictionary(Of String, String) From {
								{"id", "id"},
								{"type", "type"},
								{"description", "description"}
							}
							
							'Map and filter columns in DataTable
							dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
							
							Dim sqlRaw As String = "
								IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_NewProducts')
									DROP TABLE XFC_PLT_RAW_NewProducts													
								
								CREATE TABLE XFC_PLT_RAW_NewProducts 
							 	(
								[id] varchar(50) ,							
								[type] varchar(50) ,
								[description] varchar(200) ,
							 	)"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
							End Using							
							
							'Save rows in raw table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_NewProducts", dt, "replace")
							
							'Save rows in aux table
							Dim sql As String = $"										
						
							-- 1. Insert NEW product
						
							INSERT Into XFC_PLT_MASTER_Product
							(
								id, type, description, new_product
							)					
							
							SELECT 
								id
								, type
								, description
								, 1 as new_product
							
							FROM XFC_PLT_RAW_NewProducts
										
							"	
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
							End Using
							
							sqlRaw = "
								IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_NewProducts')
									DROP TABLE XFC_PLT_RAW_NewProducts																														
							 	"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
							End Using								
						
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.Message = "Change successfully completed."
							
																
							Return saveDataTaskResult
							
						#End Region
							
						#Region "Update Product Type"		
						
						Else If args.FunctionName.XFEqualsIgnoreCase("UpdateProductType") Then
							
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sType As String = args.NameValuePairs("product_type")
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							'Create a datatable from modified rows
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
							
							'Declare column mapping dict
							Dim columnMappingDict As New Dictionary(Of String, String) From {
								{"id_factory", "id_factory"},
								{"id", "id"},
								{"description", "description"},
								{"type", "type"},
								{"modify", "modify"}
							}
							
							'Map and filter columns in DataTable
							dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
							Dim sqlRaw As String = "
								IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_ProductType')
									DROP TABLE XFC_PLT_RAW_ProductType													
								
								CREATE TABLE XFC_PLT_RAW_ProductType 
							 	(
								[id_factory] varchar(50) ,							
								[id] varchar(50) ,
								[description] varchar(50) ,
								[type] varchar(50), 
								[modify] BIT DEFAULT 0
							 	)"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
							End Using													
							
							'Save rows in raw table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_ProductType", dt, "replace")
							
							'Save rows in aux table
							Dim sql As String = $"													
								UPDATE M
							
								SET M.type = CASE 
                								WHEN '{sType}' = 'Clear' THEN NULL 
                								ELSE '{sType}' 
            								END
								
								FROM XFC_PLT_MASTER_Product as M
							
								INNER JOIN XFC_PLT_RAW_ProductType as R
									ON M.id = R.id
							
								
							"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
							End Using
							
							sqlRaw = "
								IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_ProductType')
									DROP TABLE XFC_PLT_RAW_ProductType																																
							 	"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
							End Using								
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.CancelDefaultSave = True
							Return saveDataTaskResult
							
						#End Region		
						
						#Region "ALL - Update Product Mapping"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveProduct_Mapping") Then
							
							' ========== GET SAVE DATA INFO ==========
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							Dim tableName As String = saveDataTaskInfo.SqlTableEditorDefinition.TableName
							Dim dbLocation As String = saveDataTaskInfo.SqlTableEditorDefinition.DbLocation
							Dim externalDBConnName As String = saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName
							Dim editedDataRows As List(Of XFEditedDataRow) = saveDataTaskInfo.EditedDataRows
							Dim columnsInfo As List(Of XFDataColumn) = saveDataTaskInfo.Columns
							
							' ========== CREATE DB CONNECTION ==========
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							' ========== GET ACTUAL TABLE COLUMNS FROM DATABASE ==========
							' Consulta para obtener solo las columnas que REALMENTE existen en la tabla
							Dim actualTableColumns As List(Of String) = GetTableColumnNames(si, dbConn, tableName)
							
							' ========== PROCESS DATA CHANGES ==========
							' Añadir el using para la sql correspondiente
							For Each editedRow As XFEditedDataRow In editedDataRows
								
								Dim rowState As DbInsUpdateDelType = editedRow.InsertUpdateOrDelete
								
								Select Case rowState
									
									Case DbInsUpdateDelType.Insert	
										' InsertProductData(si, dbConn, editedRow, columnsInfo, tableName, actualTableColumns)
									Case DbInsUpdateDelType.Update
										UpdateProductData(si, dbConn, editedRow, columnsInfo, tableName, actualTableColumns)
									Case DbInsUpdateDelType.Delete
										' DeleteProductData(si, dbConn, editedRow, columnsInfo, tableName, actualTableColumns)
								End Select
								
							Next	
							
            				End Using   

							' ========== RETURN SUCCESS RESULT ==========
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.CancelDefaultSave = True
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.Message = "Datos guardados correctamente"
							Return saveDataTaskResult	
							
							#End Region
															
						End If
	
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		
		#Region "METHODS - Insert, Update, Delete"
		
		' =====================================================================
		' PRIVATE METHODS - INSERT, UPDATE, DELETE
		' =====================================================================
		
		''' <summary>
		''' Obtiene el listado de columnas reales de la tabla desde la BD
		''' </summary>
		Private Function GetTableColumnNames(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal tableName As String) As List(Of String)
			
			Try
				
				Dim columnNames As New List(Of String)
				
				' SQL para obtener columnas de SQL Server (ajustar según BD)
				Dim sql As String = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName ORDER BY ORDINAL_POSITION"
				
				Dim parameters As New List(Of DbParamInfo)
				parameters.Add(New DbParamInfo("@TableName", tableName))
				
				' Ejecutar consulta
				Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, parameters, True)
				
				If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
					For Each row As DataRow In dt.Rows
						columnNames.Add(row("COLUMN_NAME").ToString())
					Next
				End If
				
				Return columnNames
				
			Catch ex As Exception
				Throw New Exception($"Error al obtener columnas de la tabla {tableName}: {ex.Message}")
			End Try
			
		End Function
		
		''' <summary>
		''' Inserta una nueva fila en la tabla XFC_PLT_MASTER_Product
		''' Solo procesa columnas que existen en la tabla original
		''' </summary>
		Private Sub InsertProductData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal editedRow As XFEditedDataRow, ByVal columnsInfo As List(Of XFDataColumn), ByVal tableName As String, ByVal actualTableColumns As List(Of String))
			
			Try
				
				' ========== BUILD INSERT SQL ==========
				Dim columnNames As New List(Of String)
				Dim parameterNames As New List(Of String)
				Dim parameters As New List(Of DbParamInfo)
				
				For i As Integer = 0 To columnsInfo.Count - 1
					Dim column As XFDataColumn = columnsInfo(i)
					
					' FILTRO: Solo procesar columnas que existen en la tabla real
					If actualTableColumns.Contains(column.Name) Then
						columnNames.Add($"[{column.Name}]")
						parameterNames.Add($"@{column.Name}")
						' Acceder al valor de la columna usando índice
						Dim cellValue As Object = editedRow.ModifiedDataRow(column.Name)
						parameters.Add(New DbParamInfo($"@{column.Name}", If(cellValue Is Nothing OrElse IsDBNull(cellValue), DBNull.Value, cellValue)))
					End If
					
				Next
				
				Dim insertSql As String = $"INSERT INTO {tableName} ({String.Join(", ", columnNames)}) VALUES ({String.Join(", ", parameterNames)})"
				
				' ========== EXECUTE INSERT ==========
				BRApi.Database.ExecuteActionQuery(dbConn, insertSql, parameters, False, False)
				
			Catch ex As Exception
				Throw New Exception($"Error al insertar producto: {ex.Message}")
			End Try
			
		End Sub
		
		''' <summary>
		''' Actualiza una fila existente en la tabla XFC_PLT_MASTER_Product
		''' Solo procesa columnas que existen en la tabla original
		''' </summary>
		Private Sub UpdateProductData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal editedRow As XFEditedDataRow, ByVal columnsInfo As List(Of XFDataColumn), ByVal tableName As String, ByVal actualTableColumns As List(Of String))
			
			Try
				
				' ========== FIND PRIMARY KEY COLUMN ==========
				Dim pkColumn As XFDataColumn = Nothing
				Dim pkColumnIndex As Integer = -1
				For i As Integer = 0 To columnsInfo.Count - 1
					If columnsInfo(i).IsPrimaryKeyColumn Then
						pkColumn = columnsInfo(i)
						pkColumnIndex = i
						Exit For
					End If
				Next
				
				If pkColumn Is Nothing Then
					Throw New Exception("No se encontró columna de clave primaria")
				End If
				
				' ========== BUILD UPDATE SQL ==========
				Dim setClause As New List(Of String)
				Dim parameters As New List(Of DbParamInfo)
				
				For i As Integer = 0 To columnsInfo.Count - 1
					Dim column As XFDataColumn = columnsInfo(i)
					
					' FILTRO: Solo procesar columnas que existen en la tabla real y no sean PK
					If actualTableColumns.Contains(column.Name) AndAlso Not column.IsPrimaryKeyColumn Then
						If column.Name = "family_index" Then
							setClause.Add($"[{column.Name}] = @family + ' ' + @index")							
						Else	
							setClause.Add($"[{column.Name}] = @{column.Name}")
						End If
						' Usar ModifiedDataRow para valores actuales
						Dim cellValue As Object = editedRow.ModifiedDataRow(column.Name)
						parameters.Add(New DbParamInfo($"@{column.Name}", If(cellValue Is Nothing OrElse IsDBNull(cellValue), DBNull.Value, cellValue)))
					End If
					
				Next
				
				' FILTRO: Verificar que PK está en la tabla
				If Not actualTableColumns.Contains(pkColumn.Name) Then
					Throw New Exception($"La clave primaria '{pkColumn.Name}' no existe en la tabla '{tableName}'")
				End If
				
				' Add primary key parameter from OriginalDataRow para WHERE
				Dim pkValue As Object = editedRow.OriginalDataRow(pkColumn.Name)
				parameters.Add(New DbParamInfo($"@PK_{pkColumn.Name}", If(pkValue Is Nothing OrElse IsDBNull(pkValue), DBNull.Value, pkValue)))
				
				Dim updateSql As String = $"UPDATE {tableName} SET {String.Join(", ", setClause)} WHERE [{pkColumn.Name}] = @PK_{pkColumn.Name}"
				
				Dim paramMessage As String = String.Empty
				For Each paramInfo In parameters
					paramMessage = paramMessage & vbCrLf & paramInfo.Name & ": " & paramInfo.Value
				Next
				
				' BRAPI.ErrorLog.LogMessage(SI, paramMessage & vbCrLf & updateSql)
				' ========== EXECUTE UPDATE ==========
				BRApi.Database.ExecuteActionQuery(dbConn, updateSql, parameters, False, False)
				
			Catch ex As Exception
				Throw New Exception($"Error al actualizar producto: {ex.Message}")
			End Try
			
		End Sub
		
		''' <summary>
		''' Elimina una fila de la tabla XFC_PLT_MASTER_Product
		''' Solo procesa columnas que existen en la tabla original
		''' </summary>
		Private Sub DeleteProductData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal editedRow As XFEditedDataRow, ByVal columnsInfo As List(Of XFDataColumn), ByVal tableName As String, ByVal actualTableColumns As List(Of String))
			
			Try
				
				' ========== FIND PRIMARY KEY COLUMN ==========
				Dim pkColumn As XFDataColumn = Nothing
				Dim pkColumnIndex As Integer = -1
				For i As Integer = 0 To columnsInfo.Count - 1
					If columnsInfo(i).IsPrimaryKeyColumn Then
						pkColumn = columnsInfo(i)
						pkColumnIndex = i
						Exit For
					End If
				Next
				
				If pkColumn Is Nothing Then
					Throw New Exception("No se encontró columna de clave primaria")
				End If
				
				' FILTRO: Verificar que PK está en la tabla
				If Not actualTableColumns.Contains(pkColumn.Name) Then
					Throw New Exception($"La clave primaria '{pkColumn.Name}' no existe en la tabla '{tableName}'")
				End If
				
				' ========== BUILD DELETE SQL ==========
				Dim parameters As New List(Of DbParamInfo)
				' Usar OriginalDataRow para obtener el PK original
				Dim pkValue As Object = editedRow.OriginalDataRow(pkColumnIndex)
				parameters.Add(New DbParamInfo($"@PK_{pkColumn.Name}", If(pkValue Is Nothing OrElse IsDBNull(pkValue), DBNull.Value, pkValue)))
				
				Dim deleteSql As String = $"DELETE FROM {tableName} WHERE [{pkColumn.Name}] = @PK_{pkColumn.Name}"
				
				' ========== EXECUTE DELETE ==========
				BRApi.Database.ExecuteActionQuery(dbConn, deleteSql, parameters, False, False)
				
			Catch ex As Exception
				Throw New Exception($"Error al eliminar producto: {ex.Message}")
			End Try
			
		End Sub
		
		#End Region
				
		
		Private Function ImportXLSXFileProductionBudget(ByVal si As SessionInfo, Optional factory As String = "") As List(Of TableRangeContent)
	
			Try
				'Declaramos la variable que almacenará los datos del fichero a cargar
				Dim loadResults As New List(Of TableRangeContent)
				Dim fileName As String = "Budget_Production.xlsx"
				Dim pathFactory As String = $"Documents/Public/Plants/Import/{factory}"
'				Dim pathTemplate As String = $"Documents/Public/Plants/Templates/{fileName}"
				
				'Declaramos las variables con la ruta al fichero
				
				Dim filePath As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, pathFactory)
				
				'Recuperamos el fichero del FileExplorer
				Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath.XFFolder.FullName & "/" & fileName, True, True)
				
				'Cargamos los datos del fichero
				loadResults = BRApi.Utilities.LoadCustomTableUsingExcel(si, SourceDataOriginTypes.FromFileUpload, filePath.XFFolder.FullName & "/" & fileName, fileInfo.XFFile.ContentFileBytes)

				Return loadResults
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function			
		
	End Class
	
End Namespace
