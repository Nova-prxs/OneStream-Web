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

Namespace OneStream.BusinessRule.Finance.AQS_Memberlist_PLAN
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.MemberListHeaders
						'Dim objMemberListHeaders As New List(Of MemberListHeader)
						'objMemberListHeaders.Add(new MemberListHeader("Sample Member List"))
						'Return objMemberListHeaders
						
					Case Is = FinanceFunctionType.MemberList
'Example: UD1#root.CustomMemberList(BRName=AQS_Memberlist_PLAN, MemberListName=[SelectUD1], prm_selectMedicalCenter=[|!prm_selectMedicalCenter!|])
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("SelectUD1") Then
							Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
							Dim scountry As String = sWorkflowName.Split("_")(0)
							
							If scountry = "Czech" Then
								scountry= "Czech_Republic"
							Else If scountry  = "Northern" Then
								scountry= "Northern_Ireland"
							End If 
							
							Dim UD1PK As String = $"UD1_200_{scountry}"
							Dim TopUd1 As String = scountry
							
							If scountry = "Ireland" Then
								UD1PK= $"UD1_200_{scountry}"
								TopUd1= "Republic_of_Ireland"
							Else If scountry = "Northern_Ireland" Then
								UD1PK= "UD1_200_Ireland"
								TopUd1= "Northern_Ireland"
							Else If scountry = "UK" Then
								UD1PK= $"UD1_200_{scountry}"
								TopUd1= "United_Kingdom"
							End If

							Dim customDimPK As DimPk = api.Dimensions.GetDim(UD1PK).DimPk
							Dim prm_selectMedicalCenter As String = args.MemberListArgs.NameValuePairs("prm_selectMedicalCenter")
'Brapi.ErrorLog.LogMessage(si, "AN: " & prm_selectMedicalCenter)
							If prm_selectMedicalCenter ="" Or 
							prm_selectMedicalCenter = Nothing Or
							prm_selectMedicalCenter.XFContainsIgnoreCase("None")
'prm_selectMedicalCenter = scountry & ".Base.Where(name doesnotcontain 'LOC_')"
								prm_selectMedicalCenter = TopUd1 & ".Base.Where(name doesnotcontain 'LOC_')"
							Else
'								'ElseIf Not BRApi.Finance.Members.IsBase(si, customDimPK, brapi.Finance.Members.GetMemberId(Si, dimtype.UD1.Id, TopUd1), brapi.Finance.Members.GetMemberId(Si, dimtype.UD1.Id,prm_selectMedicalCenter))
								prm_selectMedicalCenter = prm_selectMedicalCenter & ".Base.Where(name doesnotcontain 'LOC_')"
							End If
'Brapi.ErrorLog.LogMessage(si, "AN: " & prm_selectMedicalCenter & " - " & BRApi.Finance.Members.IsBase(si, api.Dimensions.GetDim(UD1PK).DimPk, brapi.Finance.Members.GetMemberId(Si, dimtype.UD1.Id,"Top"), brapi.Finance.Members.GetMemberId(Si, dimtype.UD1.Id, prm_selectMedicalCenter)) )
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(customDimPK, "UD1#" &  prm_selectMedicalCenter, Nothing)' & prm_selectMedicalCenter
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							Return objMemberList
						ElseIf args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("SelectUD2") Then
'Example: UD2#root.CustomMemberList(BRName=AQS_Memberlist_PLAN, MemberListName=[SelectUD2], prm_selectModality=[|!prm_selectModality!|])
							Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
							Dim scountry As String = sWorkflowName.Split("_")(0)
							Dim prm_selectModality As String = args.MemberListArgs.NameValuePairs("prm_selectModality")
'Brapi.ErrorLog.LogMessage(si, "AN: " & prm_selectModality)
							If prm_selectModality = "" Or
							prm_selectModality = Nothing Or
							prm_selectModality.XFContainsIgnoreCase("None")
								prm_selectModality = "Top.Base.Remove(UD2_DummyBucket,None,19HQ, BD, CL, FI,HQ,HR,IN,IT,LE,MA,PT,QA,TR)"
							Else
'							ElseIf Not BRApi.Finance.Members.IsBase(si, args.MemberListArgs.DimPk, brapi.Finance.Members.GetMemberId(Si, dimtype.UD2.Id,"Top"), brapi.Finance.Members.GetMemberId(Si, dimtype.UD2.Id,prm_selectModality))
								prm_selectModality = prm_selectModality & ".Base.Remove(UD2_DummyBucket,None,19HQ, BD, CL, FI,HQ,HR,IN,IT,LE,MA,PT,QA,TR)"
							End If
							
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, "UD2#" & prm_selectModality, Nothing)
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