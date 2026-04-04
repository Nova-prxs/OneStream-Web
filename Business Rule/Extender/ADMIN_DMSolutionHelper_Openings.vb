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

Namespace OneStream.BusinessRule.Extender.ADMIN_DMSolutionHelper_Openings
	Public Class MainClass
		
		'Reference business rule to get functions
		'Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			
							
				'Get the forecast year and month parameters
				Dim ParamForecastYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
				Dim ParamForecastMonth As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month")
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)							
					
					'Query to insert historical data
					Dim sQuery_Insert_HistoricalData As String = $"
						DECLARE @max_day INTEGER;
						SELECT
							@max_day = MAX(DAY(date))
						FROM XFC_RawSales
						WHERE
							YEAR(date) = {ParamForecastYear}
							AND MONTH(date) = {ParamForecastMonth};
					
						SET @max_day = 16
						
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
						AND unit_code in (	SELECT DISTINCT cebe 
											FROM XFC_ComparativeCEBES 
											WHERE desc_annualcomparability IN ('Operativas Primer Año','Operativas Segundo Año')
											AND Year(date) = {ParamForecastYear})
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
											
				End Using
				
				Return Nothing

		End Function
		
	End Class
	
End Namespace