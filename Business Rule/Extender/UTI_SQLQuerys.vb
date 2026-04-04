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

Namespace OneStream.BusinessRule.Extender.UTI_SQLQuerys
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
'						UPDATE XFC_AUX_DailySales_MonthlyAdjustments
'						SET value = 0
'						WHERE brand = 'Starbucks' AND year(date) = 2026 AND scenario = 'Budget';
'						"
					Dim Query As String = "
					INSERT INTO XFC_HR_MASTER_GLB_CECO (cebe) VALUES ('SF172456');
					"
					
					
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
					BRApi.Database.ExecuteSql(dbcon,Query,True)


				End Using					

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace