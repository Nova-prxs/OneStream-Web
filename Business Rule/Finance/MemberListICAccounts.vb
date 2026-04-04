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

Namespace OneStream.BusinessRule.Finance.MemberListICAccounts
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberListHeaders
						Dim objMemberListHeaders As New List(Of MemberListHeader)
						objMemberListHeaders.Add(New MemberListHeader("GetAccountsUsingPlugAccount"))
						Return objMemberListHeaders
						
					Case Is = FinanceFunctionType.MemberList
						'Example: E#Root.CustomMemberList(BRName=MyBusinessRuleName, MemberListName=[Sample Member List], Name1=Value1)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("GetAccountsUsingPlugAccount") Then

							Dim lstResults As New List(Of MemberInfo)
							
							'Read in the plug account from args
							Dim strPlugAccount As String = args.MemberListArgs.NameValuePairs("PlugAccount")
							
							'Exit if you got garbage
							If strPlugAccount Is Nothing Then Return Nothing

							'Query DB for list of members using that plug account
							Using dbcApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								'Set query
								Dim strQuery As String = "
									SELECT 
										p.[name] AS PlugAccount
										, m.[name] AS Account
									FROM member m
									JOIN MemberProperty mp ON m.MemberId = mp.MemberId
									JOIN member p ON mp.DecimalValue = p.memberid
									WHERE mp.PropertyId = 700
									AND p.[name] = '" & strPlugAccount & "'"

								'Execute query
								Dim dtPlugAndICs As DataTable = BRApi.Database.ExecuteSqlUsingReader(dbcApp, strQuery, False)
								
								'Parse results
								For Each drResult As DataRow In dtPlugAndICs.Rows
									brapi.ErrorLog.LogMessage(si, drResult("Account"))
									Dim temp As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Account, drResult("Account"), True)
									If Not temp Is Nothing Then lstResults.Add(temp)
								Next
							End Using
							
							'Return the memberlist
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberList As New MemberList(objMemberListHeader, lstResults)
							Return objMemberList
						End If
					
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace