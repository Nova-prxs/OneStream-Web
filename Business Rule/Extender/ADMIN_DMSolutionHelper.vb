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

Namespace OneStream.BusinessRule.Extender.ADMIN_DMSolutionHelper
	Public Class MainClass
		
		'Reference business rule to get functions
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						'Get function name as parameter
						Dim functionName As String = args.NameValuePairs("p_function")
						
						#Region "Copy Actual To Forecast"
						
						If functionName = "CopyActualToForecast" Then
							
							'Get the forecast year and month parameters
							Dim ParamForecastYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
							Dim ParamForecastMonth As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month")
							Dim ParamBudgetYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
							
							'Update Task Activity and check if it's cancelled
							Dim isDMCancelled As Boolean = BRApi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, "Updating Daily Sales table", 0)
							If isDMCancelled Then Return Nothing
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							'Query to clear historical and forecast data before the new forecast month
							Dim sQuery_Delete_HistoricalData As String = $"
								DECLARE @max_day INTEGER;
								SELECT
									@max_day = MAX(DAY(date))
								FROM XFC_RawSales
								WHERE
									YEAR(date) = {ParamForecastYear}
									AND MONTH(date) = {ParamForecastMonth};
							
								DELETE D FROM XFC_DailySales D
							    INNER Join XFC_CEBES CE 
									On D.unit_code = CE.cebe
								WHERE 
									(
										D.Scenario = 'Actual' 
										AND (Year(D.Date) = {ParamForecastYear} OR Year(D.Date) = {ParamForecastYear} - 1)
									)
									OR
									(
										D.Scenario = 'Forecast' 
										AND Year(D.Date) = {ParamForecastYear}
										AND (
											MONTH(D.Date) < {ParamForecastMonth}
											OR (
												MONTH(D.Date) = {ParamForecastMonth}
												AND DAY(D.Date) <= @max_day
											)
										)
									);"
								
							BRApi.Database.ExecuteSql(dbConn, sQuery_Delete_HistoricalData, False)
							
							'Query to insert historical data
							Dim sQuery_Insert_HistoricalData As String = $"
								DECLARE @max_day INTEGER;
								SELECT
									@max_day = MAX(DAY(date))
								FROM XFC_RawSales
								WHERE
									YEAR(date) = {ParamForecastYear}
									AND MONTH(date) = {ParamForecastMonth};
								
								INSERT INTO XFC_DailySales (
								scenario
								,year
								,brand
								,unit_code	
								,unit_description
								,date
								,date_comparable
								,week_num
								,week_day
								,event
								,channel
								,sales
								,sales_comparable
								,customers
								,customers_comparable					
								)					
								
								SELECT 
								'Actual'
								,YEAR(RS.date)
								,CASE
									WHEN CE.brand IN ('SDH', 'Genéricos Grupo') THEN CE.sales_brand
									ELSE CE.brand
								END AS brand
								,RS.unit_code	
								,CE.description
								,RS.date
								,null
								,0					
								,DATENAME(weekday, RS.date) AS week_day
								,''
								,CH.desc_channel1 AS channel	
								,SUM(RS.sales_net) AS sales
								,null
								,SUM(RS.customers) AS customers
								,null				
							
								FROM XFC_RawSales RS
								INNER JOIN XFC_ChannelHierarchy CH 
									ON RS.cod_channel3 = CH.cod_channel3
								INNER JOIN XFC_CEBES CE 
									ON RS.unit_code = CE.cebe
			
								WHERE
								CE.cebe_class = 'R'
								AND (
									(
										year(RS.date) = {ParamForecastYear}
										AND (
											Month(RS.date) < {ParamForecastMonth}
											OR (
												MONTH(RS.date) = {ParamForecastMonth}
												AND DAY(RS.date) <= @max_day
											)
										)
									)
									OR year(RS.date) = {ParamForecastYear} - 1
								)
								--AND unit_code in (	SELECT DISTINCT cebe 
								--					FROM XFC_ComparativeCEBES 
								--					WHERE desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
								--					AND Year(date) = {ParamForecastYear})
								AND (
										(
											CE.brand NOT IN ('SDH', 'Genéricos Grupo')
											AND
											CE.brand IS NOT NULL
										)
										OR
										(
											CE.brand IN ('SDH', 'Genéricos Grupo')
											AND
											CE.sales_brand IS NOT NULL
										)
									)
								
								GROUP BY
								CASE
									WHEN CE.brand IN ('SDH', 'Genéricos Grupo') THEN CE.sales_brand
									ELSE CE.brand
								END
								,RS.unit_code	
								,CE.description
								,RS.date
								,DATENAME(weekday, RS.date) 
								,CH.desc_channel1
								
								ORDER BY
								brand
							 	,RS.date
								,RS.unit_code
								,CH.desc_channel1;"
								
							BRApi.Database.ExecuteSql(dbConn, sQuery_Insert_HistoricalData, False)
							
							'Update Task Activity and check if it's cancelled
							isDMCancelled = BRApi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, "Updating budget in Daily Sales table", 10)
							If isDMCancelled Then Return Nothing
							
							'Query to update the budget sales
							Dim sQuery_Update_BudgetSales As String = $"
								UPDATE ds1
								SET
									ds1.sales = ds2.sales,
									ds1.customers = ds2.customers,
									ds1.sales_comparable = ds2.sales
								FROM XFC_DailySales ds1
								INNER JOIN XFC_DailySales ds2
									ON ds1.unit_code = ds2.unit_code
									AND ds1.date_comparable = ds2.date
									AND ds1.channel = ds2.channel
								WHERE
									ds1.scenario = 'Budget'
									AND ds2.scenario = 'Actual'
									AND YEAR(ds1.date_comparable) = {ParamForecastYear}
							"
								
							BRApi.Database.ExecuteSql(dbConn, sQuery_Update_BudgetSales, False)
							
							'Sales and customers formulas for sql queries
							
'							Dim sSales As String = "
'								(
'							  	-- AT
'			  					(((ISNULL(sales, 0) + ISNULL(rem_sales_adj, 0) + ISNULL(daily_sales_adj, 0)) / NULLIF((ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0)), 0))
'			  					+ ISNULL(week_at_yoyprice_adj, 0) + ISNULL(week_at_newprice_adj, 0) + ISNULL(week_at_prom_adj, 0) + ISNULL(week_at_prodmix_adj, 0) + ISNULL(week_at_gen1_adj, 0) + ISNULL(week_at_gen2_adj, 0) + ISNULL(week_at_trend_adj, 0))
			  
'			  					-- CUSTOMERS
'			  					* (ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0) + ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0) + ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0))
'			  					) "
							
							' NEW 11/07/2025
							Dim sSales As String = "
								(
							  	-- AT
							  	(((ISNULL(sales, 0) + ISNULL(rem_sales_adj, 0) + ISNULL(daily_sales_adj, 0) + ISNULL(month_sales_unit_adj, 0)) / NULLIF((ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0)), 0))
  							 	+ ISNULL(week_at_yoyprice_adj, 0) + ISNULL(week_at_newprice_adj, 0) + ISNULL(week_at_prom_adj, 0) + ISNULL(week_at_prodmix_adj, 0) + ISNULL(week_at_gen1_adj, 0) + ISNULL(week_at_gen2_adj, 0) + ISNULL(week_at_trend_adj, 0))
  
							  	-- CUSTOMERS
							  	* (ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0) + ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0) + ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0) + ISNULL(month_customers_adj, 0))
								) "
							
							Dim sCustomers As String = " ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0) + ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0) + ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0) + ISNULL(month_customers_adj, 0) "	
							
							' Query for update budget sales with special days (holidays and events)
							Dim sQuery_UpdateSalesDates As String =	$"
								DECLARE @max_day INTEGER;
								SELECT
									@max_day = MAX(DAY(date))
								FROM XFC_DailySales
								WHERE
									scenario = 'Actual'
									AND YEAR(date) = {ParamForecastYear}
									AND MONTH(date) = {ParamForecastMonth};
							
							
								UPDATE dbo.XFC_DailySales
								SET 
									XFC_DailySales.sales = EB.sales * (1 + ISNULL(EV.perc_var,0))
									,XFC_DailySales.customers = EB.customers * (1 + ISNULL(EV.perc_var,0))	
								
			     				FROM dbo.XFC_DailySales D
							
								LEFT JOIN XFC_CEBES CE 
									ON D.unit_code = CE.cebe					
							
			     				INNER JOIN (SELECT E.date, E.event_name, EE.state, EE.region, EE.location, EE.channel, EE.unit, EE.id AS eventeffect_id
							
											, CASE WHEN E.event_name LIKE '(Prev%' AND ISNULL(EE.next_year_var,0) <> 0 THEN EE.next_year_var
												   WHEN E.event_name NOT LIKE '(Prev%' AND ISNULL(EE.ef_year_var,0) <> 0 THEN EE.ef_year_var
												   ELSE E.perc_var END AS perc_var
							
											FROM dbo.XFC_EventBehavior E
											INNER JOIN dbo.XFC_EventEffects EE
												ON E.event_id = EE.event_id) EV
								
									ON D.date = EV.date
									AND (CE.state = EV.state OR EV.state = 'All')
									AND (CE.region = EV.region OR EV.region = 'All')
									AND (CE.location = EV.location OR EV.location = 'All')
									AND (D.channel = EV.channel OR EV.channel = 'All')        
									AND (D.unit_code= EV.unit OR EV.unit = 'All')         	
											
			     				INNER JOIN (SELECT ES.eventeffect_id, ES.event_date, D.unit_code, D.channel, AVG({sSales}) AS sales, AVG({sCustomers}) AS customers
											FROM XFC_EventSalesDates ES
											INNER JOIN XFC_DailySales D
												ON D.scenario IN ('Actual','Forecast')
												AND ES.sales_date = D.date
											GROUP BY ES.eventeffect_id, ES.event_date, D.unit_code, D.channel)	EB
									
									ON D.unit_code = EB.unit_code
									AND D.channel = EB.channel
									AND EV.eventeffect_id = EB.eventeffect_id
									AND EV.date = EB.event_date
								
								WHERE D.scenario = 'Budget' 
								AND D.year = {ParamForecastYear} + 1
								AND (
									MONTH(D.comparative_date) < {ParamForecastMonth}
									OR (
										MONTH(D.comparative_date) = {ParamForecastMonth}
										AND DAY(D.comparative_date) <= @max_day
									)
								)"
								
							End Using
							
'							'Create time filter string to send a param
							Dim timeFilter As String
							
							For i As Integer = 1 To (ParamForecastMonth - 1)
								
								timeFilter += If(String.IsNullOrEmpty(timeFilter), $"T#{ParamForecastYear}M{i}", $", T#{ParamForecastYear}M{i}")
								
							Next
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																							{"p_time_filter", timeFilter},
																							{"p_scenario", "Forecast"},
																							{"p_year", ParamForecastYear}
																						}
							
							'Update Task Activity and check if it's cancelled
							isDMCancelled = BRApi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, "Populating forecast with actuals in cube", 5)
							If isDMCancelled Then Return Nothing
							
							'Launch the DM
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ADMIN_CopyActualToForecastCube", customSubstVars)
							
							'Update Task Activity and check if it's cancelled
							isDMCancelled = BRApi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, "Updating budget in cube", 50)
							If isDMCancelled Then Return Nothing
								
							'Get list of brands and dictionary of parameters to update budget in cube
							Dim brandList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Brands_LiteralValue").Split(",")
							Dim parameterDict As New Dictionary(Of String, String) From {
								{"p_year", ParamBudgetYear},
								{"p_scenario", "Budget"},
								{"p_forecast_month", ParamForecastMonth}
							}
							
							For Each brandName In brandList
								'Update parameter dict to use the correct brand
								parameterDict("p_brand") = brandName
								'Execute "all recalculate" data management sequence
								BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ALL_MappingBrand", parameterDict)
								'Execute "openings recalculate" data management sequence
								BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ALL_MappingBrand_OP", parameterDict)
							Next
							
						#End Region
						
						#Region "Copy Actual To Forecast Selected Months"
						
						Else If functionName = "CopyActualToForecast_SelectedMonths" Then
							
							'Get the forecast year and month parameters
							Dim ParamForecastYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
							Dim ParamMonthStart As String = args.NameValuePairs("p_month_start")
							Dim ParamMonthEnd As String = args.NameValuePairs("p_month_end")
							
							'Update Task Activity and check if it's cancelled
							Dim isDMCancelled As Boolean = BRApi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, "Updating Daily Sales table", 0)
							If isDMCancelled Then Return Nothing
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							'Query to clear historical and forecast data before the new forecast month
							Dim sQuery_Delete_HistoricalData As String = $"
							
								DELETE D FROM XFC_DailySales D
							    INNER Join XFC_CEBES CE 
									On D.unit_code = CE.cebe
								--WHERE D.Scenario IN ('Actual','Forecast')
								WHERE D.Scenario IN ('Actual')
								AND YEAR(D.Date) = {ParamForecastYear} 
								AND MONTH(D.Date) BETWEEN {ParamMonthStart} AND {ParamMonthEnd}
							"
								
							BRApi.ErrorLog.LogMessage(si, sQuery_Delete_HistoricalData)
							BRApi.Database.ExecuteSql(dbConn, sQuery_Delete_HistoricalData, False)
							
							'Query to insert historical data
							Dim sQuery_Insert_HistoricalData As String = $"							
								INSERT INTO XFC_DailySales (
								scenario
								,year
								,brand
								,unit_code	
								,unit_description
								,date
								,date_comparable
								,week_num
								,week_day
								,event
								,channel
								,sales
								,sales_comparable
								,customers
								,customers_comparable					
								)					
								
								SELECT 
								'Actual'
								,YEAR(RS.date)
								,CASE
									WHEN CE.brand IN ('SDH', 'Genéricos Grupo') THEN CE.sales_brand
									ELSE CE.brand
								END AS brand
								,RS.unit_code	
								,CE.description
								,RS.date
								,null
								,0					
								,DATENAME(weekday, RS.date) AS week_day
								,''
								,CH.desc_channel1 AS channel	
								,SUM(RS.sales_net) AS sales
								,null
								,SUM(RS.customers) AS customers
								,null				
							
								FROM XFC_RawSales RS
								INNER JOIN XFC_ChannelHierarchy CH 
									ON RS.cod_channel3 = CH.cod_channel3
								INNER JOIN XFC_CEBES CE 
									ON RS.unit_code = CE.cebe
			
								WHERE
								CE.cebe_class = 'R'
								AND YEAR(RS.Date) = {ParamForecastYear} 
								AND MONTH(RS.Date) BETWEEN {ParamMonthStart} AND {ParamMonthEnd}
							
								AND (
										(
											CE.brand NOT IN ('SDH', 'Genéricos Grupo')
											AND
											CE.brand IS NOT NULL
										)
										OR
										(
											CE.brand IN ('SDH', 'Genéricos Grupo')
											AND
											CE.sales_brand IS NOT NULL
										)
									)
								
								GROUP BY
								CASE
									WHEN CE.brand IN ('SDH', 'Genéricos Grupo') THEN CE.sales_brand
									ELSE CE.brand
								END
								,RS.unit_code	
								,CE.description
								,RS.date
								,DATENAME(weekday, RS.date) 
								,CH.desc_channel1
								
								ORDER BY
								brand
							 	,RS.date
								,RS.unit_code
								,CH.desc_channel1;"
								
							BRApi.ErrorLog.LogMessage(si, sQuery_Insert_HistoricalData)
							BRApi.Database.ExecuteSql(dbConn, sQuery_Insert_HistoricalData, False)
							
							'Update Task Activity and check if it's cancelled
							isDMCancelled = BRApi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, "Updating budget in Daily Sales table", 10)
							If isDMCancelled Then Return Nothing							
								
							End Using							
							
						#End Region						
						
						#Region "Save Scenario Version"
						
						Else If functionName = "SaveScenarioVersion" Then
							
							'Get the forecast year and month parameters
							Dim ParamForecastYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
							Dim ParamBudgetYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
							Dim ParamScenario As String = args.NameValuePairs("prm_Scenario_Elegible")
							Dim ParamVersion As String = args.NameValuePairs("prm_Version_Elegible")
							Dim ParamScenarioYear As String = If(ParamScenario = "Forecast", ParamForecastYear, ParamBudgetYear)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								'Query to clear version and scenario data for the correct year
								Dim sQuery_Delete_VersionData As String = 	
									$"	DELETE 
										FROM XFC_DailySalesVersions
										WHERE
											year = {ParamScenarioYear}
											AND scenario = '{ParamScenario}'
											AND version = {ParamVersion};"
									
								BRApi.Database.ExecuteSql(dbConn, sQuery_Delete_VersionData, False)
								
								'Query to insert version and scenario data
								Dim sQuery_Insert_VersionData As String =	
									$"INSERT INTO XFC_DailySalesVersions (
									version
									,scenario
									,year
									,brand
									,unit_code	
									,unit_description
									,date
									,date_comparable
									,week_num
									,week_day
									,event
									,channel
									,sales
									,sales_comparable
									,customers
									,customers_comparable
									,rem_sales_adj
									,rem_customers_adj
									,comment
									,daily_sales_adj_date
									,daily_sales_adj
									,daily_customers_adj
									,week_customers_prom_adj
									,week_customers_camp_adj
									,week_customers_remgrowth_adj
									,week_customers_growth_adj
									,week_customers_gen1_adj
									,week_customers_gen2_adj
									,week_customers_trend_adj
									,week_at_yoyprice_adj
									,week_at_newprice_adj
									,week_at_prom_adj
									,week_at_prodmix_adj
									,week_at_gen1_adj
									,week_at_gen2_adj
									,week_at_trend_adj
									,month_sales_adj
									,month_customers_adj
									)					
									
									SELECT
									{ParamVersion}
									,scenario
									,year
									,brand
									,unit_code	
									,unit_description
									,date
									,date_comparable
									,week_num
									,week_day
									,event
									,channel
									,sales
									,sales_comparable
									,customers
									,customers_comparable
									,rem_sales_adj
									,rem_customers_adj
									,comment
									,daily_sales_adj_date
									,daily_sales_adj
									,daily_customers_adj
									,week_customers_prom_adj
									,week_customers_camp_adj
									,week_customers_remgrowth_adj
									,week_customers_growth_adj
									,week_customers_gen1_adj
									,week_customers_gen2_adj
									,week_customers_trend_adj
									,week_at_yoyprice_adj
									,week_at_newprice_adj
									,week_at_prom_adj
									,week_at_prodmix_adj
									,week_at_gen1_adj
									,week_at_gen2_adj
									,week_at_trend_adj
									,month_sales_adj
									,month_customers_adj
								
									FROM XFC_DailySales
									WHERE
										year = {ParamScenarioYear}
										AND scenario = '{ParamScenario}'
										AND channel IS NOT NULL
										AND channel <> '';"
									
								BRApi.Database.ExecuteSql(dbConn, sQuery_Insert_VersionData, False)
								
							End Using
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																							{"p_version", paramVersion},
																							{"p_scenario", ParamScenario},
																							{"p_year", ParamScenarioYear},
																							{"p_time", ParamScenarioYear}
																						}
							
							'Launch the DM
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ADMIN_SaveScenarioVersionCube", customSubstVars)

							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.Message = "Version saved."
							Return selectionChangedTaskResult
							
						#End Region
						
						#Region "Clear Scenario Version"
						
						Else If functionName = "ClearScenarioVersion" Then
							
							'Get the forecast year and month parameters
							Dim ParamForecastYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
							Dim ParamBudgetYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
							Dim ParamScenario As String = args.NameValuePairs("prm_Scenario_Elegible")
							Dim ParamVersion As String = args.NameValuePairs("prm_Version_Elegible")
							Dim ParamScenarioYear As String = If(ParamScenario = "Forecast", ParamForecastYear, ParamBudgetYear)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								'Query to clear version and scenario data for the correct year
								Dim sQuery_Delete_VersionData As String = 	
									$"	DELETE 
										FROM XFC_DailySalesVersions
										WHERE
											year = {ParamScenarioYear}
											AND scenario = '{ParamScenario}'
											AND version = {ParamVersion};"
									
								BRApi.Database.ExecuteSql(dbConn, sQuery_Delete_VersionData, False)
								
							End Using
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																							{"p_version", paramVersion},
																							{"p_scenario", ParamScenario},
																							{"p_year", ParamScenarioYear}
																						}
							
							'Launch the DM
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ADMIN_ClearScenarioVersionCube", customSubstVars)

							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.Message = "Version cleared."
							Return selectionChangedTaskResult
							
						#End Region
						
						#Region "Copy Events Table From Other App"
						
						Else If functionName = "CopyEventsTableFromOtherApp" Then
							'Create Session Info for another app
							Dim oar As New OpenAppResult
							Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Production_BKP_20240920", oar)
							Dim dt As New DataTable
							
							If oar = OpenAppResult.Success Then
								Using mainDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
									'Select all event ids from main db to later selecting the different ids from other app
									Dim selectQuery As String = "
										SELECT DISTINCT event_name
										FROM XFC_Events
									"
									Dim eventDt As DataTable = BRApi.Database.ExecuteSql(mainDbConn, selectQuery, False)
									
									'Create string containing all the ids
									Dim eventString As String
									If eventDt.Rows.Count > 0 Then
										For Each row As DataRow In eventDt.Rows
											eventString += If(String.IsNullOrEmpty(eventString), $"'{row("event_name").Replace("'","''")}'", $", '{row("event_name").Replace("'","''")}'")
										Next
									Else
										eventString = "''"
									End If
									
									'Select all the events in other database that are not those ids
									selectQuery = $"
										SELECT *
										FROM XFC_Events
										WHERE event_name NOT IN ({eventString})
									"
									eventDt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
									
									'Get max event id to reorder events
									selectQuery = "
										SELECT MAX(id) as id
										FROM XFC_Events
									"
									dt = BRApi.Database.ExecuteSql(mainDbConn, selectQuery, False)
									brapi.ErrorLog.LogMessage(si, dt.Rows.Count)
									Dim maxId As Integer = If(
										IsNumeric(dt.Rows(0)("id")),
										dt.Rows(0)("id"),
										0
									)
									brapi.ErrorLog.LogMessage(si, "Paso")
									
									'Create a dictionary to map the events with their new id
									Dim eventIdMappingDict As New Dictionary(Of Integer, Integer)
									
									'Get all the ids of those events to retrieve all the event effects and convert the id
									Dim idString As String
									If eventDt.Rows.Count > 0 Then
										Dim i As Integer = maxId + 1
										'Delete id constraint
										eventDt.PrimaryKey = Nothing
										eventDt.Columns("id").Unique = False
										For Each row As DataRow In eventDt.Rows
											idString += If(String.IsNullOrEmpty(idString), $"{row("id")}", $", {row("id")}")
											eventIdMappingDict(row("id")) = i
											row("id") = i
											i += 1
										Next
									Else
										Return Nothing
									End If
									
									'Get all the event effects from those ids
									selectQuery = $"
										SELECT *
										FROM XFC_EventEffects
										WHERE event_id IN ({idString})
									"
									Dim eventEffectDt As DataTable = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
									
									'Get max event effect id to reorder event effects
									selectQuery = "
										SELECT MAX(id) as id
										FROM XFC_EventEffects
									"
									dt = BRApi.Database.ExecuteSql(mainDbConn, selectQuery, False)
									maxId = If(
										IsNumeric(dt.Rows(0)("id")),
										dt.Rows(0)("id"),
										0
									)
									If eventEffectDt.Rows.Count > 0 Then
										Dim i As Integer = maxId + 1
										'Delete id constraint
										eventEffectDt.PrimaryKey = Nothing
										eventEffectDt.Columns("id").Unique = False
										For Each row As DataRow In eventEffectDt.Rows
											row("event_id") = eventIdMappingDict(row("event_id"))
											row("id") = i
											i += 1
										Next
									End If
									
									'Generate inserts of events
									For Each insertQuery As String In SharedFunctionsBR.GenerateInsertSQLList(eventDt, "XFC_Events")
										BRApi.Database.ExecuteSql(mainDbConn, insertQuery, False)
									Next
									
									'Generate inserts of event effects
									For Each insertQuery As String In SharedFunctionsBR.GenerateInsertSQLList(eventEffectDt, "XFC_EventEffects")
										BRApi.Database.ExecuteSql(mainDbConn, insertQuery, False)
									Next
								
								End Using
								End Using
							End If
						
						#End Region
						
						#Region "Copy DB Table From Other App"
						
						Else If functionName = "CopyDBTableFromOtherApp" Then
							'Create Session Info for another app
							Dim oar As New OpenAppResult
							Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, args.NameValuePairs("p_backup"), oar)
							
							'Get table name and declare dt
							Dim ParamTable As String = args.NameValuePairs("p_table")
							Dim dt As New DataTable
							
							If oar = OpenAppResult.Success Then
								Using mainDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
									'Get all the rows from external table
									Dim selectQuery As String = $"
										SELECT *
										FROM {ParamTable}
									"
									dt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
									
									'Truncate target table
									Dim deleteQuery As String = $"
									TRUNCATE TABLE {ParamTable}
									"
									BRApi.Database.ExecuteSql(mainDbConn, deleteQuery, False)
									
									'Generate inserts
									For Each insertQuery As String In SharedFunctionsBR.GenerateInsertSQLList(dt, ParamTable)
										BRApi.Database.ExecuteSql(mainDbConn, insertQuery, False)
									Next
								
								End Using
								End Using
							End If
						
						#End Region
						
						#Region "Export Daily Sales CSV"
						
						Else If functionName = "ExportDailySalesCSV" Then
							'Get parameters and declare dt
							Dim sParamScenario As String = args.NameValuePairs("p_scenario")
							Dim sYear As String = args.NameValuePairs("p_year")
							Dim sParamBrand As String = args.NameValuePairs("p_brand")
							Dim sParamChannel As String = args.NameValuePairs("p_channel")
							Dim sParamSQLFinalSales As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_SQL_FinalSales")
							Dim sParamForecastMonth As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month")
							Dim dt As DataTable = Nothing
							
							'Create db connection
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Query to get the daily sales
								Dim Query As String = $"
								WITH MonthlyCebeProportions AS (
								    SELECT
								        YEAR(rs.date) AS cte_year,
								        MONTH(rs.date) AS cte_month,
								        rs.unit_code AS cte_unit_code,
								        ch.desc_channel1 AS cte_channel1,
								        ch.cod_channel1 AS cte_channel1_code,
								        ch.desc_channel2 AS cte_channel2,
								        ch.cod_channel2 AS cte_channel2_code,
										-- Calculate proportion by unit
								        ROUND(
											SUM(rs.sales_net)
								        	/ NULLIF(SUM(SUM(rs.sales_net)) OVER (PARTITION BY YEAR(rs.date), MONTH(rs.date), rs.unit_code, ch.desc_channel1), 0),
											2
										) AS channel1_sales_proportion,
								        -- Calculate proportion by brand
										ROUND(
											SUM(SUM(rs.sales_net)) OVER (
									            PARTITION BY
									                YEAR(rs.date),
									                MONTH(rs.date),
									                CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
									                ch.desc_channel2
									            )
									        / NULLIF(SUM(SUM(rs.sales_net)) OVER (
									            PARTITION BY
									                YEAR(rs.date),
									                MONTH(rs.date),
									                CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
									                ch.desc_channel1
									            ), 0),
											2
										) AS channel1_sales_proportion_brand
								    FROM XFC_RawSales rs
								    INNER JOIN XFC_ChannelHierarchy ch ON rs.cod_channel3 = ch.cod_channel3
								    INNER JOIN XFC_CEBES c ON rs.unit_code = c.cebe
								    WHERE
								        ch.desc_channel1 = 'Delivery'
								        AND YEAR(rs.date) BETWEEN {sYear - 2} AND {sYear}
								        AND (
								            (c.brand IN ('SDH', 'Genéricos Grupo') AND c.sales_brand = '{sParamBrand}')
								            OR
								            (c.brand NOT IN ('SDH', 'Genéricos Grupo') AND c.brand = '{sParamBrand}')
								        )
										AND rs.sales_net <> 0
								    GROUP BY
								        YEAR(rs.date),
								        MONTH(rs.date),
								        CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
								        rs.unit_code,
								        ch.desc_channel1,
								        ch.cod_channel1,
								        ch.desc_channel2,
								        ch.cod_channel2
								)
								
							    SELECT
							        ds.unit_code AS [Unidad],
							        --ds.unit_description AS [Unit Description],
									CONVERT(VARCHAR,ds.date,103) AS [Fecha],		
								
							        CASE
										-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN mcp.cte_channel2_code
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.cte_channel2_code IS NOT NULL THEN mcp2.cte_channel2_code
							            WHEN ds.channel = 'Sala' THEN 1
										WHEN ds.channel = 'Take Away' THEN 4
										ELSE 3
							        END AS Sala,
							        
							        CASE
										-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN
							                ({sParamSQLFinalSales}) * mcp.channel1_sales_proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.channel1_sales_proportion_brand IS NOT NULL THEN
							                ({sParamSQLFinalSales}) * mcp2.channel1_sales_proportion_brand
							            ELSE
							                {sParamSQLFinalSales}
							        END AS [Venta Bruta],
								
							        CASE
										-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN
							                ({sParamSQLFinalSales}) * mcp.channel1_sales_proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.channel1_sales_proportion_brand IS NOT NULL THEN
							                ({sParamSQLFinalSales}) * mcp2.channel1_sales_proportion_brand
							            ELSE
							                {sParamSQLFinalSales}
							        END AS [Venta Neta],						
								
							        CASE
										-- In delivery customers, when proportion for unit exists, use unit proportion, else use brand proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN
							                (
							                    ISNULL(ds.customers,0) 
							                    + ISNULL(ds.rem_customers_adj,0) 
							                    + ISNULL(ds.daily_customers_adj, 0)
							                    + ISNULL(ds.week_customers_prom_adj,0) 
							                    + ISNULL(ds.week_customers_camp_adj,0) 
							                    + ISNULL(ds.week_customers_growth_adj, 0)
							                    + ISNULL(ds.week_customers_remgrowth_adj,0) 
							                    + ISNULL(ds.week_customers_gen1_adj,0) 
							                    + ISNULL(ds.week_customers_gen2_adj,0) 
							                    + ISNULL(ds.week_customers_trend_adj, 0) 
							                    + ISNULL(ds.month_customers_adj, 0)
							                )
							                * mcp.channel1_sales_proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.channel1_sales_proportion_brand IS NOT NULL THEN
							                (
							                    ISNULL(ds.customers,0) 
							                    + ISNULL(ds.rem_customers_adj,0) 
							                    + ISNULL(ds.daily_customers_adj, 0)
							                    + ISNULL(ds.week_customers_prom_adj,0) 
							                    + ISNULL(ds.week_customers_camp_adj,0) 
							                    + ISNULL(ds.week_customers_growth_adj, 0)
							                    + ISNULL(ds.week_customers_remgrowth_adj,0) 
							                    + ISNULL(ds.week_customers_gen1_adj,0) 
							                    + ISNULL(ds.week_customers_gen2_adj,0) 
							                    + ISNULL(ds.week_customers_trend_adj, 0) 
							                    + ISNULL(ds.month_customers_adj, 0)
							                )
							                * mcp2.channel1_sales_proportion_brand
							            ELSE
							                ISNULL(ds.customers,0) 
							                + ISNULL(ds.rem_customers_adj,0) 
							                + ISNULL(ds.daily_customers_adj, 0)
							                + ISNULL(ds.week_customers_prom_adj,0) 
							                + ISNULL(ds.week_customers_camp_adj,0) 
							                + ISNULL(ds.week_customers_growth_adj, 0)
							                + ISNULL(ds.week_customers_remgrowth_adj,0) 
							                + ISNULL(ds.week_customers_gen1_adj,0) 
							                + ISNULL(ds.week_customers_gen2_adj,0) 
							                + ISNULL(ds.week_customers_trend_adj, 0) 
							                + ISNULL(ds.month_customers_adj, 0)
							        END AS [Clientes]
								
							    FROM XFC_DailySales ds
							    INNER JOIN XFC_ComparativeDates cd
							        ON cd.date = ds.date
								-- Join for unit proportions when proportion for that unit exists
							    LEFT JOIN MonthlyCebeProportions mcp
							        ON mcp.cte_unit_code = ds.unit_code
							        AND (
							            -- Join condition based on scenario
							            ('{sParamScenario}' = 'Forecast' AND mcp.cte_year = YEAR(cd.date_comparable) AND mcp.cte_month = MONTH(cd.date_comparable))
							            OR ('{sParamScenario}' = 'Budget' AND mcp.cte_year = YEAR(cd.date_comparable_2) AND mcp.cte_month = MONTH(cd.date_comparable_2))
							        )
							        AND mcp.cte_channel1 = ds.channel
								-- Join for brand proportions when proportion for that unit doesn't exist
							    LEFT JOIN (
							        SELECT DISTINCT cte_year, cte_month, cte_channel1, cte_channel1_code, cte_channel2, cte_channel2_code, channel1_sales_proportion_brand
							        FROM MonthlyCebeProportions
							    ) mcp2
							        ON mcp2.cte_channel1 = ds.channel
							        AND (
							            -- Join condition based on scenario
							            ('{sParamScenario}' = 'Forecast' AND mcp2.cte_year = YEAR(cd.date_comparable) AND mcp2.cte_month = MONTH(cd.date_comparable))
							            OR ('{sParamScenario}' = 'Budget' AND mcp2.cte_year = YEAR(cd.date_comparable_2) AND mcp2.cte_month = MONTH(cd.date_comparable_2))
							        )
							        AND mcp.cte_channel2 IS NULL
							    INNER JOIN XFC_CEBES ce 
							        ON ds.unit_code = ce.cebe
							    WHERE ds.channel <> ''
								AND
							        (
							            (
							                ds.scenario = '{sParamScenario}'
							                AND ds.year = {sYear}
							            )
							            OR
							            (
							                ds.scenario = 'Actual'
							                AND MONTH(ds.date) <= {sParamForecastMonth}
							            )
							        )
						        AND ds.brand = '{sParamBrand}'
						        AND YEAR(ds.date) = {sYear}
								AND (ds.channel = '{sParamChannel}' OR '{sParamChannel}' = 'All')
								
								ORDER BY
								    Fecha,
								    Unidad,
								    Channel
								"
							dt = BRApi.Database.ExecuteSql(dbConn, Query, False)
							End Using
							
							'Create filename and path
							Dim fileName As String = $"DailySales_{sParamBrand}_{sParamScenario}_{sYear}_{sParamChannel}"
							Dim filepath As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, $"Documents/Public/Planning/Sales/Daily Sales Exports")
							
							'Use a string builder object
							Dim filesAsString As New StringBuilder()
							
							'Headers as first row
							filesAsString.AppendLine(String.Join(",", dt.Columns.Cast(Of DataColumn).select(Function(x) x.ColumnName)))
							
							'Loop the rows
							For Each dr As DataRow In dt.Rows
								'Use String.join to create a comma separated line
								filesAsString.AppendLine(String.Join(",", dr.ItemArray().Select(Function(item) item.ToString().Replace(",", ".")).ToArray()))
							Next dr
							
							'Convert the string to an array of byte
							Dim fileAsByte() As Byte = system.Text.Encoding.Unicode.GetBytes(filesAsString.ToString)
							'Save fileAsByte
							Dim fileDataInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, $"{fileName}.csv", filePath.XFFolder.FullName)
							Dim fileData As New XFFile(fileDataInfo, String.Empty, fileAsByte)
							BRApi.FileSystem.InsertOrUpdateFile(si, fileData)
							
						#End Region
						
						End If
						
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace