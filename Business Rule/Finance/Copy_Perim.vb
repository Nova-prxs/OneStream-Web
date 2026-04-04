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

Namespace OneStream.BusinessRule.Finance.Copy_Perim
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try

				api.Data.Calculate("A#PctCON=A#PctCON:T#PovPrior1")
				api.Data.Calculate("A#PctOWN=A#PctOWN:T#PovPrior1")
				api.Data.Calculate("A#PctMIN=A#PctMIN:T#PovPrior1")
				api.Data.Calculate("A#Methode=A#Methode:T#PovPrior1")
				api.Data.Calculate("A#Scope=A#Scope:T#PovPrior1")
				api.Data.Calculate("A#Discontinued=A#Discontinued:T#PovPrior1")
				api.Data.Calculate("A#MergedWith=A#MergedWith:T#PovPrior1")
				api.Data.Calculate("A#FXRateEnterPerim=A#FXRateEnterPerim:T#PovPrior1")
				api.Data.Calculate("A#FXRateExitPerim=A#FXRateExitPerim:T#PovPrior1")
				'api.Data.Calculate("A#PERIMETRO.Base=A#PERIMETRO.Base:T#WFPrior1")

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace