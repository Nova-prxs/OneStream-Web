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

Namespace OneStream.BusinessRule.Finance.XFW_FSK_PassifsIntercos
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 	XFW_FSK_PassifsIntercos
		'
		'Description:		To only list IC Accounts with data in OUV:ICEntities or CLO:None 
		'
		'Usage:				This will put a member list of a dimension in Alphabetical order used in a parameter
		'					
		'Created By:		Pascal D.
		'
		'Date Created:		4-18-2018
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
	
				Dim Memberlistname As String = "PassifsIntercos"
				Dim MemberListstart As String = "A#TOTPASSIF.base"
				Dim Entity As String = api.Pov.Entity.Name
				Dim EntityMember As Member = api.Pov.Entity
				Dim Period As String = api.Pov.Time.Name
				
                If args.MemberListArgs.MemberListName = Memberlistname Then
                    Dim objMemberListHeader = New MemberListHeader(args.MemberListArgs.MemberListName)
    
                    'Get all base-level accounts
                    Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, memberliststart ,Nothing)

                    'Filter the list of accounts to only include IC accounts
                    Dim objFilteredMemberInfos As List(Of MemberInfo) = New List(Of MemberInfo)
                    If Not objMemberInfos Is Nothing Then
                        Dim objMemberInfo As MemberInfo
                        For Each objMemberInfo In objMemberInfos
                            If api.Account.IsIC(objMemberInfo.Member.MemberId) Then
								
								Dim AccountName As String = objMemberInfo.Member.Name
								Dim DataOUVD As DataCell = api.Data.GetDataCell("Cb#XFW_FSK:E#" & Entity & ":P#Groupe:C#Local:S#ACTUAL:T#" & Period & ":V#YTD:A#" & AccountName & ":F#OUV:O#Top:I#ICEntities:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
								Dim DataCLOD As DataCell = api.Data.GetDataCell("Cb#XFW_FSK:E#" & Entity & ":P#Groupe:C#Local:S#ACTUAL:T#" & Period & ":V#YTD:A#" & AccountName & ":F#CLO:O#Top:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
								Dim DataOUV As Integer = DataOUVD.CellAmount
								Dim DataCLO As Integer = DataCLOD.CellAmount
								
								If DataOUV<>0 Or DataCLO<>0 Then
                            		objFilteredMemberInfos.Add(objMemberInfo)
								End If	
                            End If
                        Next
                    End If
                    
                    'Sort the filtered members alphabetically.
                    Dim objMembers As List(Of Member)
                    objMembers = (From memberInfo In objFilteredMemberInfos Order By memberInfo.Member.Name Ascending Select memberInfo.Member).ToList()
					
                    Return New MemberList(objMemberListHeader, objMembers)
                End If
						
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace