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

Imports System.Collections.Concurrent

Namespace OneStream.BusinessRule.Finance.ORDE_MemberFormulas
		
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object

			Try

				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberList						
						
					Case Is = FinanceFunctionType.Calculate
				
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

		Sub Acct_PedidosHTD (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

			Dim sCalculo As String = "A#Pedidos_Stored = A#Pedidos:V#YTD + A#Pedidos_Stored:T#PovPriorYearM12"
	
			'brapi.ErrorLog.LogMessage(si,sCalculo)
			api.Data.Calculate(sCalculo)
			
		End Sub
		
		Sub Acct_FacturacionHTD (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

			Dim sCalculo As String = "A#Facturacion_Stored = A#Facturacion:V#YTD + A#Facturacion_Stored:T#PovPriorYearM12"
	
			'brapi.ErrorLog.LogMessage(si,sCalculo)
			api.Data.Calculate(sCalculo)			

		End Sub		
		
		Sub Acct_Cartera (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

			Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
			Dim rateTypeClo As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp
			Dim closingRate As Decimal =  api.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)
			Dim closingRateToText As String = closingRate.ToString("G",CultureInfo.InvariantCulture)

			'brapi.ErrorLog.LogMessage(si,closingRateToText)
			api.Data.Calculate("A#Cartera_Stored=(A#Pedidos_Stored:C#Local - A#Facturacion_Stored:C#Local) * " & closingRateToText)			

		End Sub			
	
	End Class

End Namespace