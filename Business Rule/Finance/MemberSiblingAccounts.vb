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
'------------------------------------------------------------------------------------------------------------
'Reference Code: 	MemberSiblingAccounts
'
'Description:		Use a business rule to return the sibling accounts from Fictisious accounts (ends with CF).
'
'Usage:				'Use the following on the cube view  A#Member.[Name of Business Rule, Name of List in Business Rule]
'
'Created By:
'
'Date Created:		06/06/2024
'------------------------------------------------------------------------------------------------------------	

Namespace OneStream.BusinessRule.Finance.MemberSiblingAccounts
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				'Use the following on the cube view I#Member.[Name of Business Rule, Name of List in Business Rule]
				'A#Root.[MemberListICEntities, IC_List]
				Dim sParent As String 
				'Read in the account from args
				Dim sAccount As String = args.MemberListArgs.NameValuePairs("AccountCF")
				Dim dpkAccountDimPK As DimPk = api.Pov.AccountDim.DimPk
				Dim iAccountDimTypeID As Integer = api.Pov.AccountDim.DimPk.DimTypeId
				Dim iAccountId As Integer = api.Members.GetMemberId(iAccountDimTypeID,sAccount) 
				
				
'				brapi.errorlog.LogMessage (si, "AccountCF "& sAccount )
							
				Dim objParentsID As list(Of Member) = api.Members.GetParents(dpkAccountDimPK,iAccountId,False)
				If Not objParentsID Is Nothing Then
		        	Dim objParent As Member
	                For Each objParent In objParentsID
	                    If (api.Members.IsDescendant(dpkAccountDimPK,api.Members.GetMemberId(iAccountDimTypeID,"BS"),api.Members.GetMemberId(iAccountDimTypeID,objParent.Name))) Or _
							(api.Members.IsDescendant(dpkAccountDimPK,api.Members.GetMemberId(iAccountDimTypeID,"PL"),api.Members.GetMemberId(iAccountDimTypeID,objParent.Name))) Then
							
'							brapi.errorlog.LogMessage (si, "Parent "& objParent.Name)
							 sParent = objParent.Name 
			
							'Name of the List in the Business Rule
							Dim Memberlistname As String = "Sibling_List"
							
							'Starting point for Account selection
							Dim sSiblingMembers As String = "A#"& sParent &".Base"
							Select Case api.FunctionType      
								Case Is = FinanceFunctionType.MemberList
						            If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase (Memberlistname) Then
										
'										brapi.errorlog.LogMessage (si, "Sibling formula "& sSiblingMembers)
					                    Dim objMemberListHeader = New MemberListHeader(args.MemberListArgs.MemberListName)
					                    
										'Read the Account members (siblings)
										Dim objMembersAccounts As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dpkAccountDimPK, sSiblingMembers, Nothing)
			
					                    'Return
					                    Return New MemberList(objMemberListHeader, objMembersAccounts)
										
						            End If
							End Select
							Return Nothing
						End If
					Next
				End If
			Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace