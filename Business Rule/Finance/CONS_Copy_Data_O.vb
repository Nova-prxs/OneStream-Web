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

Namespace OneStream.BusinessRule.Finance.CONS_Copy_Data_O
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			
			Try
				
				Dim sEscenarioOrigen As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("EscenarioOrigen")
				Dim sEscenarioDestino As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("EscenarioDestino")
				
				If (api.Pov.Scenario.Name = sEscenarioDestino)
					Dim sCalculo As String = "S#" & sEscenarioDestino & " = S#" & sEscenarioOrigen
					api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,,,,, )	
					api.Data.Calculate(sCalculo,True)
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace