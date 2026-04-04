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
'Reference Code: 	MemberLIstICEntities
'
'Description:		Use a business rule to return the intercompany entities (base members) under a parent.
'
'Usage:				'Use the following on the cube view  I#Member.[Name of Business Rule, Name of List in Business Rule]
'
'Created By:
'
'Date Created:		16/04/2024
'------------------------------------------------------------------------------------------------------------	

Namespace OneStream.BusinessRule.Finance.MemberListICEntities
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				'Use the following on the cube view I#Member.[Name of Business Rule, Name of List in Business Rule]
				'I#Root.[MemberListICEntities, IC_List]
				Dim sParent As String 
				Dim sICSibling As String = api.Members.GetMemberName(api.Pov.IC.DimType.Id ,args.MemberListArgs.PovDataCellPk.ICId)
				'brapi.errorlog.LogMessage (si, "Sibling "& sICSibling)
				Dim objParentsID As list(Of Member) = api.Members.GetParents(api.Pov.Entitydim.DimPk,api.Members.GetMemberId(api.Pov.EntityDim.DimPk.DimTypeId,sICSibling),False)
				If Not objParentsID Is Nothing Then
		        	Dim objParent As Member
	                For Each objParent In objParentsID
	                    If Not(objParent.Name.XFEqualsIgnoreCase("Horse_Flat")) Then
							brapi.errorlog.LogMessage (si, "Parent "& objParent.Name)
							 sParent = objParent.Name 
			
							'Name of the List in the Business Rule
							Dim Memberlistname As String = "IC_List"
							
							'Starting point for IC selection
							Dim MemberListstart As String = "I#ICEntities.base"
							
							'Starting point for Entities selection
							Dim MemberLIststart2 As String = "E#"& sParent &".Base"
							Select Case api.FunctionType      
								Case Is = FinanceFunctionType.MemberList
						            If args.MemberListArgs.MemberListName = Memberlistname Then
					                    Dim objMemberListHeader = New MemberListHeader(args.MemberListArgs.MemberListName)
					                    
					                    'Read the members for ICP
					                    Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberListstart, Nothing)
										
										'Read thes members for Entity
										Dim objMemberEntity As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(api.Dimensions.GetDim("Entities").DimPk, MemberListstart2, Nothing)
			
					                    'Select only the ICPs where Entities are LEGAL
					                    Dim objMembers As List(Of Member) = Nothing
																						
					                    If Not objMemberInfos Is Nothing And Not objMemberEntity Is Nothing Then
											objMembers = (From memberInfo In objMemberInfos, memberEntInfo In objMemberEntity
			                                              Where memberInfo.Member.Name = memberEntInfo.Member.Name  								
											              Select memberInfo.Member).Distinct().toList()
					                    End If
										
					                    'Return
					                    Return New MemberList(objMemberListHeader, objMembers)
										
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