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

Namespace OneStream.BusinessRule.Finance.AQS_Memberlist
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
							
					Case Is = FinanceFunctionType.MemberList
						'Example: UD3#Root.CustomMemberList(BRName=AQS_Memberlist, MemberListName=[UD3], selectedEntity=[|!INP_EntitySelection!|])
						
						'Grab the namevalue pairs from the Memberlist
						Dim selectedEntity As String = args.MemberListArgs.NameValuePairs("selectedEntity")
						
							If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("UD3") Then
								Return GetListUD3(si, api,args, selectedEntity)
							End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetListUD3(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal selectedEntity As String) As MemberList
			Try				
						'Define the DimPK
						Dim customDimPK As DimPk = api.Dimensions.GetDim($"UD3_100_Top").DimPk
									
						'Use the DimPk to create the requested list 
						Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)				
						Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(customDimPK, $"UD3#Top.Base", Nothing)
						Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
						Return objMemberList
						
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function	
		
		Public Function GetListUD1(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
							
					Case Is = FinanceFunctionType.MemberList
						
						'Define WFCountry
						Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
						Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
						Dim mbrTimeMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey)				
						Dim scountry As String = ""
						If sWorkflowName.Contains("_") Then 
							scountry = sWorkflowName.Split("_")(0) 
						Else
							scountry = "" 
						End If 
									
						'Create List for UD1
						Dim UD1MemberList = $"U1#{scountry}.base"
						Dim UD1DimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"UD1_200_{scountry}")			
						Dim UD1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1DimPK, UD1MemberList, True)
						
						For Each UD1Member In UD1List
							
							Dim UD1Membername As String = UD1Member.Member.Name							
							
							Dim UD1Id As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,UD1Membername)
							Dim BudgetId As Integer = api.Members.GetMemberId(dimtype.Scenario.Id,"Budgetv1")
							Dim UD1Text2_Budget As String = BRApi.Finance.UD.Text(si, dimtype.UD1.Id, UD1Id)
	'						Dim UD1Text2_Budget As String = api.UD1.Text(UD1Member.memberId,1,args.SubstVarSourceInfo.WFScenario.DimTypeId,args.SubstVarSourceInfo.WFTime.DimTypeId)
							
	'						If UD1Text2_Budget IsNot Nothing Then
	' BRApi.Finance.UD1.Text							
	'						End If
						
						Next
							
'						Return objMemberList
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
	End Class
End Namespace