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

Namespace OneStream.BusinessRule.Extender.UTI_SQLQuerys_2
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
					
'					Dim Query As String =  "UPDATE XFC_DailySales
'											SET unit_code = 'M002699'
'											WHERE scenario = 'Budget' 
'											  AND YEAR(Date) = 2025 
'											  AND unit_code = 'M004699';
'											"

'					Dim Query As String =  "
'							UPDATE XFC_DailySales
'								SET week_customers_trend_adj = 0
'									, week_at_trend_adj = 0
'							WHERE scenario = 'Forecast'
'							AND year = 2025
'							AND Unit_Code IN ('SF172597','SF172589','SF172601')
									
'											"

'					Dim Query As String = 
'								"INSERT Into XFC_DailySales (
'									,scenario
'									,year
'									,brand
'									,unit_code	
'									,unit_description
'									,Date
'									,date_comparable
'									,week_num
'									,week_day
'									,Event
'									,channel
'									,sales
'									,sales_comparable
'									,customers
'									,customers_comparable
'									,rem_sales_adj
'									,rem_customers_adj
'									,comment
'									,daily_sales_adj_date
'									,daily_sales_adj
'									,daily_customers_adj
'									,week_customers_prom_adj
'									,week_customers_camp_adj
'									,week_customers_remgrowth_adj
'									,week_customers_growth_adj
'									,week_customers_gen1_adj
'									,week_customers_gen2_adj
'									,week_customers_trend_adj
'									,week_at_yoyprice_adj
'									,week_at_newprice_adj
'									,week_at_prom_adj
'									,week_at_prodmix_adj
'									,week_at_gen1_adj
'									,week_at_gen2_adj
'									,week_at_trend_adj
'									,month_sales_adj
'									,month_customers_adj
'									)					
									
'									Select
'									{ParamVersion}
'									,scenario
'									,year
'									,brand
'									,unit_code	
'									,unit_description
'									,Date
'									,date_comparable
'									,week_num
'									,week_day
'									,Event
'									,channel
'									,sales
'									,sales_comparable
'									,customers
'									,customers_comparable
'									,rem_sales_adj
'									,rem_customers_adj
'									,comment
'									,daily_sales_adj_date
'									,daily_sales_adj
'									,daily_customers_adj
'									,week_customers_prom_adj
'									,week_customers_camp_adj
'									,week_customers_remgrowth_adj
'									,week_customers_growth_adj
'									,week_customers_gen1_adj
'									,week_customers_gen2_adj
'									,week_customers_trend_adj
'									,week_at_yoyprice_adj
'									,week_at_newprice_adj
'									,week_at_prom_adj
'									,week_at_prodmix_adj
'									,week_at_gen1_adj
'									,week_at_gen2_adj
'									,week_at_trend_adj
'									,month_sales_adj
'									,month_customers_adj
								
'									FROM XFC_DailySales
'									WHERE Year = 2025
'										AND scenario = 'Forecast_V3'
'										AND channel Is Not NULL
'										AND channel <> '';
					
'					"
					
					
'					Dim Query2 As String =  "UPDATE XFC_DailySales
																
'											SET week_at_yoyprice_adj= 0
'											, week_at_newprice_adj= 0
'											, week_at_prom_adj= 0
'											, week_at_prodmix_adj= 0
'											, week_at_gen1_adj= 0
'											, week_at_gen2_adj= 0
'											, week_at_trend_adj= 0
'											, month_sales_adj= 0
'											, rem_customers_adj= 0
'											, daily_customers_adj= 0
'											, week_customers_prom_adj= 0
'											, week_customers_camp_adj= 0 
'											, week_customers_growth_adj= 0
'											, week_customers_remgrowth_adj= 0
'											, week_customers_gen1_adj= 0
'											, week_customers_gen2_adj= 0 
'											, week_customers_trend_adj= 0
'											, month_customers_adj= 0					
																
'											WHERE scenario = 'Forecast' 
'											AND year(Date) = 2024 
'											AND Comment = 'Opening'
'											AND Unit_Code IN ('F001572','F001573','F001581','M002554','M002636','Q0019623','Q0019624','Q0019625','Q0019626','Q0019627','Q0019629','Q0019630','Q0019631','Q0019632','Q0019850','Q0019851','Q0019854','Q0019856')		
					
'											AND (week_at_yoyprice_adj <> 0
'											OR week_at_newprice_adj <> 0
'											OR week_at_prom_adj <> 0
'											OR week_at_prodmix_adj <> 0
'											OR week_at_gen1_adj <> 0
'											OR week_at_gen2_adj <> 0
'											OR week_at_trend_adj <> 0
'											OR month_sales_adj <> 0
'											OR rem_customers_adj <> 0
'											OR daily_customers_adj <> 0
'											OR week_customers_prom_adj <> 0
'											OR week_customers_camp_adj <> 0 
'											OR week_customers_growth_adj <> 0
'											OR week_customers_remgrowth_adj <> 0
'											OR week_customers_gen1_adj <> 0
'											OR week_customers_gen2_adj <> 0 
'											OR week_customers_trend_adj <> 0
'											OR month_customers_adj <> 0)
'									"

					'BRAPI.ErrorLog.LogMessage(si, "Query: " & Query)
					
					Dim sQuery As String = 
					"
					UPDATE D
						SET D.sales = EB.sales * (1 + ISNULL(EV.perc_var,0))
							,D.customers = EB.customers * (1 + ISNULL(EV.perc_var,0))	
					
     				FROM (
						SELECT *
						FROM dbo.XFC_DailySales 
						WHERE scenario = 'Budget'
							AND year = 2026
							AND brand = 'Starbucks Holanda'
							--And Year(Date) = 2026
							--And month(Date) = 8
							--And day(Date) = 1
							--And unit_code = 'SF303800'
							--And channel = 'Sala'
					) D
				
					LEFT JOIN XFC_CEBES CE 
						ON D.unit_code = CE.cebe
					INNER JOIN (
						SELECT *
						FROM dbo.XFC_ComparativeCEBESAux
						WHERE year = 2026
							AND desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
					) cca ON D.unit_code = cca.cebe		
				
     				INNER JOIN (SELECT E.date, E.event_name, EE.state, EE.region, EE.location, EE.channel, EE.unit, EE.id AS eventeffect_id
				
								, CASE WHEN E.event_name LIKE '(Prev%' AND ISNULL(EE.next_year_var,0) <> 0 THEN EE.next_year_var
									   WHEN E.event_name NOT LIKE '(Prev%' AND ISNULL(EE.ef_year_var,0) <> 0 THEN EE.ef_year_var
									   ELSE E.perc_var END AS perc_var
				
								FROM (
									SELECT *
									FROM dbo.XFC_EventBehavior
									WHERE brand = 'Starbucks Holanda'
								) E
								INNER JOIN (
									SELECT *
									FROM dbo.XFC_EventEffects
									WHERE brand = 'Starbucks Holanda'
								) EE ON E.event_id = EE.event_id
						) EV
					
						ON D.date = EV.date
						AND (CE.state = EV.state OR EV.state = 'All')
						AND (CE.region = EV.region OR EV.region = 'All')
						AND (CE.location = EV.location OR EV.location = 'All')
						AND (D.channel = EV.channel OR EV.channel = 'All')        
						AND (D.unit_code= EV.unit OR EV.unit = 'All')         	
								
					INNER JOIN (SELECT  TOP 50000  ES.eventeffect_id, ES.event_date, FIL.unit_code, FIL.channel, AVG(
					(
				  	-- AT
  					(((ISNULL(sales, 0) + ISNULL(rem_sales_adj, 0) + ISNULL(daily_sales_adj, 0)) / NULLIF((ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0)), 0))
  					+ ISNULL(week_at_yoyprice_adj, 0) + ISNULL(week_at_newprice_adj, 0) + ISNULL(week_at_prom_adj, 0) + ISNULL(week_at_prodmix_adj, 0) + ISNULL(week_at_gen1_adj, 0) + ISNULL(week_at_gen2_adj, 0) + ISNULL(week_at_trend_adj, 0))
  
  					-- CUSTOMERS
  					* (ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0) + ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0) + ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0) + ISNULL(month_customers_adj, 0))
  					)) AS sales, AVG( ISNULL(customers, 0) + ISNULL(rem_customers_adj, 0) + ISNULL(daily_customers_adj, 0) + ISNULL(week_customers_prom_adj, 0) + ISNULL(week_customers_camp_adj, 0) + ISNULL(week_customers_growth_adj, 0) + ISNULL(week_customers_remgrowth_adj, 0) + ISNULL(week_customers_gen1_adj, 0) + ISNULL(week_customers_gen2_adj, 0) + ISNULL(week_customers_trend_adj, 0) + ISNULL(month_customers_adj, 0) ) AS customers
								FROM (	SELECT * 
										FROM XFC_EventSalesDates
										WHERE brand = 'Starbucks Holanda') ES
								--Clear All Data Inexistent
								CROSS JOIN (SELECT DISTINCT brand, unit_code, channel 
											FROM dbo.XFC_DailySales 
											WHERE brand = 'Starbucks Holanda'
											AND Year = 2026 ) FIL
								LEFT JOIN (
									SELECT *
									FROM XFC_DailySales
									WHERE scenario IN ('Actual','Forecast')
										AND brand = 'Starbucks Holanda'
								) D ON ES.sales_date = D.date
									AND FIL.unit_code = D.unit_code
									AND FIL.channel = D.channel
								GROUP BY ES.eventeffect_id, ES.event_date, FIL.unit_code, FIL.channel)	EB
						
						ON D.unit_code = EB.unit_code
						AND D.channel = EB.channel
						AND EV.eventeffect_id = EB.eventeffect_id
						AND EV.date = EB.event_date
					
					"

Dim sQuery2 As String = 
"
					UPDATE D
						SET D.sales = 500
					
     				FROM (
						SELECT *
						FROM dbo.XFC_DailySales 
						WHERE scenario = 'Budget'
							AND year = 2026
							AND brand = 'Starbucks Holanda'
							AND Year(date) = 2026
							AND month(date) = 8
							AND day(date) = 1
							AND unit_code = 'SF303800'
							AND channel = 'Sala'
					) D
"
					
					BRApi.Database.ExecuteSql(dbcon,sQuery,False)
'					BRApi.Database.ExecuteSql(dbcon,sQuery2,False)

				End Using					

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace