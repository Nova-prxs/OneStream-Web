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

Namespace OneStream.BusinessRule.Finance.ESG_Memberlist
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberList
'						Example: E#Root.CustomMemberList(BRName=ESG_Memberlist, MemberListName=[UD1List])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("UD1List") Then
						
							Dim wfentity As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).GetAttribute(ScenarioTypeID.Sustainability, SharedConstants.WorkflowProfileAttributeIndexes.Text1).ToString
							Dim ud1country As String = ""
							Dim ud1integer As Integer = api.Members.GetMemberId(dimtype.UD1.Id, wfentity)
							If ud1integer <> -1
								ud1country = $"UD1#{wfentity}.Base.Where(Text1 DoesnotContain Country)"
							Else ' DoesNotContain
								ud1country = "UD1#Top"
							End If
'							brapi.ErrorLog.LogMessage(si, " ud1country: " & ud1country)
							Dim dimpkUD1 As DimPk = api.Dimensions.GetDim("UD1_600_ESG").DimPk
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimpkUD1, ud1country, Nothing)
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							Return objMemberList
						ElseIf args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("UD1Country") Then
							Dim wfentity As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).GetAttribute(ScenarioTypeID.Sustainability, SharedConstants.WorkflowProfileAttributeIndexes.Text1).ToString
							wfentity =  $"{wfentity}"
							Dim ud1country As String = ""
							Dim ud1integer As Integer = api.Members.GetMemberId(dimtype.UD1.Id, wfentity)
							If ud1integer <> -1
								ud1country = $"UD1#{wfentity}.base.where(Text1 = Country) "
							Else
								ud1country = "UD1#Root"
							End If
							
							Dim dimpkUD1 As DimPk = api.Dimensions.GetDim("UD1_600_ESG").DimPk
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimpkUD1, ud1country, Nothing)
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
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