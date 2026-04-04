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

Namespace OneStream.BusinessRule.Extender.UTI_DBQuery
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
						
				'Create connections to both internal and external databases
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
					'Declare relevant variables
					Dim createQuery As String
					Dim dt As DataTable																
							
'							Dim deleteQuery As String = $"	UPDATE XFC_ComparativeCEBESAux
'																SET desc_AnnualComparability = 'Operativas Primer Año'
'															WHERE Cebe IN ('SF016337','Sf303832')
'															AND Year = 2025"


                           
'''							createQuery = $"	UPDATE XFC_ComparativeCEBES
'													SET desc_annualcomparability = CASE 
'													                                WHEN Year(date) = 2023 THEN 'Cerradas' 
'													                                WHEN Year(date) = 2024 THEN 'Operativas Primer Año'
'													                                WHEN Year(date) = 2025 THEN 'Operativas Segundo Año'
'													                              END
'													WHERE cebe IN ('Q0019633','Q0019634','Q0019635','Q0019636') 
'													AND Year(date) IN (2023, 2024, 2025)"
						
							dt = BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
				
				End Using

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace