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

Namespace OneStream.BusinessRule.Finance.XFR_MemberListAlphabetical
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 	XFR_MemberListAlphabetical
		'
		'Description:		Use a business rule to sort a member list in Alphabetical order
		'
		'Usage:				This will put a member list of a dimension in Alphabetical order. 
		'					Use the following on the cube view  E#Member.[Name of Business Rule, Name of List in Business Rule]
		'					E#Root.[XFR_MemberListAlphabetical, EntityAlphabetical]
		'
		'
		'Created By:		Robert Powers (put in XF Ref by John Von Allmen)
		'
		'Date Created:		5-24-2013
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try

				'This will put a member list of a dimension in Alphabetical order. 
				'Use the following on the cube view  E#Member.[Name of Business Rule, Name of List in Business Rule]
				'E#Root.[XFR_MemberListAlphabetical, EntityAlphabetical]								
				Dim Memberlistname As String = "EntityAlphabetical"
				Dim MemberListstart As String = "E#Input_Plan.base"
			
				Select Case api.FunctionType      
					Case Is = FinanceFunctionType.MemberList
			            If args.MemberListArgs.MemberListName = Memberlistname Then
		                    Dim objMemberListHeader = New MemberListHeader(args.MemberListArgs.MemberListName)
		                    
		                    'Read the members
		                    Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, MemberListstart, Nothing)

		                    'Sort the members
		                    Dim objMembers As List(Of Member) = Nothing
		                    If Not objMemberInfos Is Nothing Then
		                    	objMembers = (From memberInfo In objMemberInfos Order By memberInfo.Member.Name Ascending Select memberInfo.Member).ToList()
		                    End If
'		                    api.LogMessage("check" & objMembers.ToString)
		                    'Return
		                    Return New MemberList(objMemberListHeader, objMembers)
			            End If
				End Select
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace