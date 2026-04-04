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

Namespace OneStream.BusinessRule.Extender.UTI_UpdateComparabilityUnit
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
					
					Dim sUnit As String = "'SF016350','F001412','F001646'"
					Dim sComparability As String = "Operativas Primer Año"
					Dim sYear As String = "2025"

					Dim sQuery As String = $"

						UPDATE XFC_ComparativeCEBES
							SET desc_annualcomparability = '{sComparability}'						
						WHERE Cebe IN ({sUnit})
						AND YEAR(date) = {sYear}
					
						UPDATE XFC_ComparativeCEBESAux
							SET desc_annualcomparability = '{sComparability}'						
						WHERE Cebe IN ({sUnit})
						AND year = {sYear}					
											
					"				

					'BRAPI.ErrorLog.LogMessage(si, "sQuery: " & sQuery)
					BRApi.Database.ExecuteSql(dbcon, sQuery, False)


				End Using					

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace