Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.Extender.Update_Paremeter_Analytics_AñoObjetivo
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				
				Dim origenParametro As String = "Analytics_AñoObjetivo"
				Dim destinoParametro As String = "Analytics_Year"
				'origenParametro = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, origenParametro)
				
				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, destinoParametro, 
					BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, origenParametro))
				

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace