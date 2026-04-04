Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Namespace OneStream.BusinessRule.Finance.CCAA_DynamicAccount
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		
		Public Sub Acc8D4(ByVal si As SessionInfo, ByVal consomember As String, ByVal sTime As String)
			
		
			'Get account's value for Spain
 			Dim cruce_spain As String = "E#R1301:S#Statutory:C#" & consomember &":A#8D4_AC:T#" & sTime & ":O#AdjInput:V#YTD:I#None:F#None:U1#None:U2#None:U3#None:U4#PACKN1:U5#None:U6#None:U7#None:U8#None"
			Dim value_spain = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "Horse", cruce_spain).DataCellEx.DataCell.CellAmount
			
			'Set it for Holding
			Dim cruce_holding As String = "E#R1300:S#Statutory:C#" & consomember &":A#8D4_AC:T#" & sTime & ":O#AdjInput:V#YTD:I#None:F#None:U1#None:U2#None:U3#None:U4#PACKN1:U5#None:U6#None:U7#None:U8#None"
			Dim ListMemberAndValue As New List(Of MemberScriptAndValue)
			Dim MemberAndValue As New MemberScriptAndValue
			MemberAndValue.Amount = value_spain 
			MemberAndValue.Script = cruce_holding
			MemberAndValue.IsNoData = False
			ListMemberAndValue.Add(MemberAndValue)

			Dim objXFResult As XFResult = brapi.Finance.Data.SetDataCellsUsingMemberScript(si, ListMemberAndValue)


			
			
			
		End Sub
		
		
		
		
	End Class
End Namespace