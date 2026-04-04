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

Namespace OneStream.BusinessRule.Finance.CashFlow_MapMembers
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberListHeaders
						Dim objMemberListHeaders As New List(Of MemberListHeader)
						objMemberListHeaders.Add(New MemberListHeader("CFAccounts"))
						objMemberListHeaders.Add(New MemberListHeader("CFFlows"))
						objMemberListHeaders.Add(New MemberListHeader("NoMappedAcc"))
						Return objMemberListHeaders
						
					Case Is = FinanceFunctionType.MemberList
						'Example: E#Root.CustomMemberList(BRName=MyBusinessRuleName, MemberListName=[Sample Member List], Name1=Value1)
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("CFAccounts") Then

							Dim lstResults As New List(Of MemberInfo)
		
							'Query DB for list of members using that plug account
							'Using dbcApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim sbSQLAcc As New Text.StringBuilder()					 
								Dim sOAcc As String 
								Dim sCFAcc As String = args.MemberListArgs.NameValuePairs("CFAccount")
								
								'BRAPI.ErrorLog.LogMessage (si, "CF account " & sCFAcc)
								'Select all the distinct Balance accounts defined in the mapping table for one CF account.
								sbSQLAcc.Append(" SELECT Distinct(Origin_Account)")			
								sbSQLAcc.Append(" FROM XFC_CASHFLOW_TABLE where Destination_Account = '" & sCFAcc & "'")																				
								
								Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQLAcc.ToString,False)											
											
										For Each dr As DataRow In dt.Rows
											
											sOAcc = dr(0).toString
											
											Dim temp As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Account, sOAcc, True)
											If Not temp Is Nothing Then lstResults.Add(temp)
											'brapi.ErrorLog.LogMessage(si, sCFAcc)
											
										Next				
									End Using
								End Using
								
							'Return the memberlist
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberList As New MemberList(objMemberListHeader, lstResults)
							Return objMemberList
						
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("CFFlows") Then

							Dim lstResults As New List(Of MemberInfo)
		
							'Query DB for list of members using that plug account
							'Using dbcApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim sbSQLAcc As New Text.StringBuilder()					
								Dim sCFFlow As String 
								Dim sCFAcc As String = args.MemberListArgs.NameValuePairs("CFAccount")

								'Select all the distinct Flows members defined in the mapping table for one CF account.
								sbSQLAcc.Append(" SELECT Distinct(Origin_Flow)")			
								sbSQLAcc.Append(" FROM XFC_CASHFLOW_TABLE where Destination_Account = '" & sCFAcc & "'")																				
								
								Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQLAcc.ToString,False)											
											
										For Each dr As DataRow In dt.Rows
											
											sCFFlow = dr(0).toString
											Dim temp As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Flow, sCFFlow, True)
											If Not temp Is Nothing Then lstResults.Add(temp)
															
										Next				
									End Using
								End Using
								
							'Return the memberlist
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberList As New MemberList(objMemberListHeader, lstResults)
							Return objMemberList
						
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("NoMappedAcc") Then
							Dim lstResults As New List(Of MemberInfo)
							Dim lstMissingMem As New List(Of MemberInfo)
		
							'Query DB for list of members using that plug account
							'Using dbcApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim sbSQLAcc As New Text.StringBuilder()					 
								Dim sOAcc As String 
								Dim sCFAcc As String = args.MemberListArgs.NameValuePairs("CFAccount")
								
								'BRAPI.ErrorLog.LogMessage (si, "CF account " & sCFAcc)
								'Select all the distinct Balance accounts defined in the mapping table for one CF account.
								sbSQLAcc.Append(" SELECT Distinct(Origin_Account)")			
								sbSQLAcc.Append(" FROM XFC_CASHFLOW_TABLE")																				
								
								Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
									Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQLAcc.ToString,False)											
											
										For Each dr As DataRow In dt.Rows
											
											sOAcc = dr(0).toString
											
											Dim temp As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Account, sOAcc, True)
											Dim AcctBase As Boolean = api.Members.IsBase(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "BS"), Temp.Member.MemberId )
											
											If AcctBase And Not temp.Member.Name.XFEqualsIgnoreCase("BS_Plug") Then
											
												If Not temp Is Nothing Then lstResults.Add(temp)
												'brapi.ErrorLog.LogMessage(si, sCFAcc)
											Else
												For Each baseAcc In api.Members.GetMembersUsingFilter(api.Pov.AccountDim.DimPk, "A#" & temp.Member.Name  & ".base")
													If Not  baseAcc Is Nothing Then lstResults.Add(baseAcc)
												Next
											End If
										Next				
									End Using
								End Using
								
							For Each atest In api.Members.GetMembersUsingFilter(api.Pov.AccountDim.DimPk, "A#BS.base")
								'Dim temp2 As MemberInfo = BRApi.Finance.Members.GetMemberInfo(si, dimtypeid.Account,atest.Member.Name, False)
'								For Each sAcc As List(Of Members) In lstresults.LastIndexOf()
									
'									lstmissingmem.Add(atest)
'								Next
							Next
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