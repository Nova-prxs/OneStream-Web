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

Namespace OneStream.BusinessRule.Extender.UTI_SQLQuerys3
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)											
					
					Dim sQueryComponentConfig As String =
					"
					DROP TABLE XFC_HR_AMD_ComponentConfig
					"

				BRApi.Database.ExecuteSql(dbcon, sQueryComponentConfig, False)


				End Using					

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace