Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Finance.Delete_intersection_ESG
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
		
					Case Is = FinanceFunctionType.CustomCalculate
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DataClearing") Then
						api.Data.SetDataCell("Cb#Cube_100_Group:E#Hungary_Input_OP:C#Local:S#Actual_ESG:T#2023M12:V#YTD:A#M_SOC_019:F#CLO_Value:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#UOM_Top",0, True)
					End If
						
			
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace