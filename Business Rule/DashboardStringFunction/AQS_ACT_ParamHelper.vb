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

Namespace OneStream.BusinessRule.DashboardStringFunction.AQS_ACT_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("FLASH_Forecast") Then
					
					Dim WFTime As String = args.NameValuePairs.XFGetValue("WFTime")
					Dim Month As String = WFtime.substring(4)
					
					If Month = "M1"
						Return "S#FC_1_11"
					ElseIf Month = "M2"
						Return "S#FC_1_11"
					ElseIf Month = "M3"
						Return "S#FC_2_10"
					ElseIf Month = "M4"
						Return "S#FC_3_9"
					ElseIf Month = "M5"
						Return "S#FC_4_8"
					ElseIf Month = "M6"
						Return "S#FC_5_7"
					ElseIf Month = "M7"
						Return "S#FC_6_6"
					ElseIf Month = "M8"
						Return "S#FC_7_5"
					ElseIf Month = "M9"
						Return "S#FC_8_4"
					ElseIf Month = "M10"
						Return "S#FC_9_3"
					ElseIf Month = "M11"
						Return "S#FC_10_2"
					ElseIf Month = "M12"
						Return "S#FC_11_1"
					End If
				ElseIf args.FunctionName.XFEqualsIgnoreCase("HasChildren") Then
					'XFBR(AQS_ACT_ParamHelper, HasChildren, Entity_Select=[|POVEntity|], IC_Select=[|PovIC|] )
					Dim IC_Select As String = args.NameValuePairs.XFGetValue("IC_Select", "None")
					Dim UD2_Select As String = args.NameValuePairs.XFGetValue("UD2_Select", "None")
					Dim Entity_Select As String = args.NameValuePairs.XFGetValue("Entity_Select")
					If Not brapi.Finance.Members.HasChildren(si,
					BRApi.Finance.Dim.GetDimPk(si, "Ent_100_Group"),
					brapi.Finance.Members.GetMemberId(si,dimtype.Entity.Id, Entity_Select))
'					If IC_Select.XFEqualsIgnoreCase("None")
						Return "O#BeforeAdj:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#statutory:"
'					Else
'						Return "O#BeforeAdj:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#statutory:"
'					End If
'						Return Nothing
					End If
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace