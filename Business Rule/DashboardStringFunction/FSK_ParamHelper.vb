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

Namespace OneStream.BusinessRule.DashboardStringFunction.FSK_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("Map") Then
					Dim sMap As String = ""
					Dim sDim As String = args.NameValuePairs("Dim")
					Dim sSource As String = args.NameValuePairs("Source")
					Select sDim
						Case "A":
							sMap = "Accounts"
						Case "F":
							sMap = "Flows"
					End Select
					If Not sMap.Equals("") Then
'						brapi.ErrorLog.LogMessage(si, sDim & "#" & BRApi.Utilities.TransformText(si, sSource, sMap, True))
						Return sDim & "#" & BRApi.Utilities.TransformText(si, sSource, sMap, True)
					Else
'						brapi.ErrorLog.LogMessage(si, sDim & "#" & sSource)
						Return sDim & "#" & sSource
					End If
				End If
	If args.FunctionName.XFEqualsIgnoreCase("OwnList") Then
		Dim sQuery As String = args.NameValuePairs("Query")
		Dim sDim As String = args.NameValuePairs("Dim")
'					brapi.ErrorLog.LogMessage(si, "was? " & sQuery & " " & sDim)
		
		Dim oEntDimPK As DimPk = brapi.Finance.Dim.GetDim(si, sDim).DimPk' = 
		Dim oEntMemberInfos As List(Of MemberInfo) = brapi.Finance.Members.GetMembersUsingFilter(si, oEntDimPK, sQuery, True, Nothing)
'					brapi.ErrorLog.LogMessage(si, "wer? " & sQuery & " " & sDim)

		Dim oResult As New Text.StringBuilder
		
		For Each test As MemberInfo In oEntMemberInfos
			Dim oEntMemberInfos2 As List(Of Member) = brapi.Finance.Members.GetChildren(si, oEntDimPK, test.Member.MemberId , Nothing)
			Dim oParMemberInfos2 As List(Of Member) = brapi.Finance.Members.GetParents(si, oEntDimPK, test.Member.MemberId, False, Nothing)
			Dim sParent As String = "None"
			For Each oParent As Member In oParMemberInfos2
				If BRApi.Finance.Members.IsChild(si, oEntDimPK, oParent.MemberId, test.Member.MemberId) Then
					sParent = oParent.Name
					Exit For
				End If	
			Next	
			
			Dim indent As Integer = brapi.Finance.Members.GetAncestors(si, oEntDimPK, test.Member.MemberId, False, Nothing).count

			oResult.Append("I#")
			oResult.Append(sParent)
'			oResult.Append(test.Member.Name)
'			oResult.Append(":E#None")
			oResult.Append(":E#")
			oResult.Append(test.Member.Name)
			oResult.Append(":P#")
			oResult.Append(sParent)
'			oResult.Append(":C#OwnerPreAdj:O#AdjInput")
			oResult.Append(":C#Local:O#AdjInput")
			oResult.Append(":name(")
			For I As Integer = 1 To indent
				oResult.Append("  ")
			Next	
			oResult.Append("⊟ ")
			oResult.Append(test.Member.Name)
			oResult.Append(" owns ")
			oResult.Append("), ")

			
			For Each test2 As Member In oEntMemberInfos2
				If BRApi.Finance.Members.IsBase(si, oEntDimPK, dimconstants.Root, test2.MemberId)
					oResult.Append("I#")
					oResult.Append(test.Member.Name)
					oResult.Append(":E#")
					oResult.Append(test2.Name)
					oResult.Append(":name(")
					For I As Integer = 1 To indent
						oResult.Append("    ")
					Next	
'							oResult.Append(test.Member.Name)
'							oResult.Append("")
					oResult.Append("  ")
					oResult.Append(test2.Name)
					oResult.Append("), ")
					brapi.ErrorLog.LogMessage(si, oResult.ToString)
				End If
			Next
		Next
		
		Return oResult.ToString
	End If
						


				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace