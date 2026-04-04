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

Namespace OneStream.BusinessRule.Finance.XFW_FSK_OpeningScenario
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				'Not in use
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		Public Function fctOpeningScenario(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
		
			Dim openingScenario As String = api.Scenario.Text(1)
			If openingScenario = "" Then
				openingScenario = api.Pov.Scenario.Name
			End If
			Return openingScenario
			
	
		End Function	
	End Class
End Namespace