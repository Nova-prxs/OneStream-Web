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

Namespace OneStream.BusinessRule.Finance.CONS_CustomList_SSGG_DN
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
						
					Case Is = FinanceFunctionType.MemberList
						
						'Lo comentamos para que no sume tiempo de retrieve
						'If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesDN") Then

							Dim sVariableDN = args.MemberListArgs.NameValuePairs("DN")						
							'brapi.ErrorLog.LogMessage(si,sVariableDN)
							
							Dim UPMembers As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Pov.UD1Dim.DimPk, "U1#[" & sVariableDN & ".].Parents.Remove(AGG_Entity,Legal_DN)", Nothing)
																					
							Dim sUPList As String = String.Empty
							For Each UPMember In UPMembers
								sUPList = sUPList & "E#" & UPMember.Description & ","					
							Next
							
							sUPList = sUPList.Substring(0,sUPList.Length-1)
							'brapi.ErrorLog.LogMessage(si,sUPList)			
							
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sUPList, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							
							Return objMemberList
							
						'End If
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace