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

Namespace OneStream.BusinessRule.Finance.CONT_MemberFormulas
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
		
Sub CalcTAM_ValorBruto (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
	api.Data.ClearCalculatedData("A#TAM_ValorBruto",True,True,True)
		
	'Cubo + Parent + Entity + Cons + Scenario + Origin + U5 a U8	
	'Acc + ICP + Flow + U1 a U4 + Time + View 
	api.Data.Calculate("A#TAM_ValorBruto:I#None:F#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POV:V#Periodic + 
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior1:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior2:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior3:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior4:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior5:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior6:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior7:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior8:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior9:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior10:V#Periodic +
	A#Valor_Bruto:I#Top:F#Top:U1#Top:U2#Top:U3#[Total Clientes]:U4#None:U5#None:U6#None:U7#None:U8#Top:T#POVPrior11:V#Periodic")
	
	End Sub
	
	End Class
End Namespace