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

Namespace OneStream.BusinessRule.DashboardStringFunction.AQS_Get_UOM
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_UOM") Then
'					brapi.ErrorLog.LogMessage(si, "------------------------------------------ HEY-----------------------------------")
					Dim ParentAccount As String = args.NameValuePairs.XFGetValue("ParentAccount", String.Empty)
					Dim TXT8 As String = args.NameValuePairs.XFGetValue("TXT8", String.Empty)
					Dim accountsUOM As String = ""
					Dim lstAccounts As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, BRApi.Finance.Dim.GetDimPk(si, "Metrics_600_ESG"), $"A#{ParentAccount}.Base", True)
					For Each mbrAccount As MemberInfo In lstAccounts
						If brapi.Finance.Account.Text(si, mbrAccount.Member.MemberId, 8, -1, -1).XFEqualsIgnoreCase(TXT8)
							accountsUOM = "A#" & mbrAccount.Member.name & ":U8#" & brapi.Finance.Account.Text(si, mbrAccount.Member.MemberId, 7, -1, -1) & "," & accountsUOM
						ElseIf TXT8 = ""
							accountsUOM = "A#" & mbrAccount.Member.name & ":U8#" & brapi.Finance.Account.Text(si, mbrAccount.Member.MemberId, 7, -1, -1) & "," & accountsUOM
						End If
					Next

					If accountsUOM =""
'						brapi.ErrorLog.LogMessage(si, "A#Metrics_GEN:UD8#Root")
						Return "A#Metrics_GEN:UD8#Root"
					Else
'						brapi.ErrorLog.LogMessage(si, "accountsUOM> " & accountsUOM)
						Return accountsUOM'$"A#{ParentAccount}:UD8#M3"
					End If
					
				End If
			
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace