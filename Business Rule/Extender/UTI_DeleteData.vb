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

Namespace OneStream.BusinessRule.Extender.UTI_DeleteData
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
					Dim Query As String

						
						Query = $"UPDATE dbo.XFC_DailySales
										 SET week_customers_prom_adj = 0,
											week_customers_camp_adj = 0,
											week_customers_growth_adj = 0,
											week_customers_remgrowth_adj = 0,
											week_customers_gen1_adj = 0,
											week_customers_gen2_adj = 0,
											week_at_yoyprice_adj = 0,
											week_at_newprice_adj = 0,
											week_at_prom_adj = 0,
											week_at_prodmix_adj = 0,
											week_at_gen1_adj = 0,
											week_at_gen2_adj = 0
										 WHERE scenario = 'Forecast' AND brand = 'Foster''s Hollywood';
									"
						BRApi.Database.ExecuteSql(dbcon,Query,False)
						

				
'					Dim Query As String =  "TRUNCATE TABLE XFC_DailySales"

					'BRAPI.ErrorLog.LogMessage(si, "Query: " & Query)
'					BRApi.Database.ExecuteSql(dbcon,Query,False)

				End Using					

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace