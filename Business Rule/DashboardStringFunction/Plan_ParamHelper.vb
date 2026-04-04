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

Namespace OneStream.BusinessRule.DashboardStringFunction.Plan_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
			Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
			Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
			Dim mbrTimeMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey)				
			Dim scountry As String = ""
			If sWorkflowName.Contains("_") Then 
				scountry = sWorkflowName.Split("_")(0) 
			Else
				scountry = "" 
			End If 
				
			#Region "GetUsefulLife"
			 If args.FunctionName.XFEqualsIgnoreCase("GetUsefulLife") Then
				Dim SqlQuery As String = $"
									select Useful_life from XFC_VAT_Plan
											where Country = '{scountry}'
											AND WFScenario='{mbrScenarioMember.Name}' 
											AND WFTime = '{mbrTimeMember.name}'
					;"
				Dim dt As New DataTable
				Using dbconn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					 dt = BRApi.Database.ExecuteSql(dbconn, SqlQuery.ToString, False)
				End Using	
				Dim usefulLife As Decimal = 0
				If dt.Rows.Count > 0 Then
					usefulLife=dt.Rows(0).Item("Useful_life").ToString
				End If
				Return usefulLife
			#End Region 'GetUsefulLife
			
			#Region "GetVAT"
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetVAT") Then
					Dim SqlQuery As String = $"
									select VAT from XFC_VAT_Plan
											where Country = '{scountry}'
											AND WFScenario='{mbrScenarioMember.Name}' 
											AND WFTime = '{mbrTimeMember.name}'
					
					;"
						Dim dt As New DataTable
						Using dbconn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							 dt = BRApi.Database.ExecuteSql(dbconn, SqlQuery.ToString, False)
						End Using
					
						Dim Vat As Decimal = 0
						If dt.Rows.Count > 0 Then
							Vat=dt.Rows(0).Item("VAT").ToString
						End If
					Return Vat
				End If
			#End Region 'GetVAT
			
			#Region "GetWorkflowProfile"
				If args.FunctionName.XFEqualsIgnoreCase("GetWorkflowProfile") Then
						If scountry = "Czech" Then
							scountry= "Czech_Republic"
						Else If scountry = "Northern" Then
							scountry= "Northern_Ireland"
						End If
					Return "'"& scountry &"'"
				End If 
			#End Region 'GetWorkflowProfile
			
			#Region "GetUD1_Capex"
				If args.FunctionName.XFEqualsIgnoreCase("GetUD1_Capex") Then
					Dim ud1filter As String = "U1#|!prm_GetCountry_Plan!|_region.Base"
					If scountry = "Ireland" Then
						ud1filter= "U1#Republic_of_Ireland.Base"
					Else If scountry = "Northern" Then
						ud1filter= "U1#Northern_Ireland.Base"
					Else If scountry = "UK" Then
						ud1filter= "U1#United_Kingdom_region.Base"
					End If
					Return ud1filter
				End If
			#End Region 'GetUD1_Capex
			
			#Region "GetUD6_Capex"
				If args.FunctionName.XFEqualsIgnoreCase("GetUD6_Capex") Then
					Dim ud6filter As String = "U6#None,U6#|!prm_GetCountry_Plan!|.Base"
					If scountry = "Northern" Then
						ud6filter= "U6#None,U6#Ireland.Base"
					Else If scountry = "UK" Then
						ud6filter= "U6#None,U6#United_Kingdom.Base"
					End If
					Return ud6filter
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetUD6_Capex_CountryReport") Then
					Dim ud6filter As String = "U6#None,U6#|!prm_GetCountry_Plan!|,U6#|!prm_GetCountry_Plan!|.Base"
					If scountry = "Northern" Then
						ud6filter= "U6#None,U6#Ireland,U6#Ireland.Base"
					Else If scountry = "UK" Then
						ud6filter= "U6#None,U6#United_Kingdom,U6#United_Kingdom.Base"
					End If
					Return ud6filter
				End If
				
			#End Region 'GetUD6_Capex
			
			#Region "GetUD5_Capex"	
				If args.FunctionName.XFEqualsIgnoreCase("Get_UD5_Capex") Then
'					XFBR(Plan_ParamHelper, Get_UD5_Capex,PMC=|!prm_Select_PMC_Plan!|)
					
					Dim UD5Filter As String=""
					Dim PMC As String = args.NameValuePairs.XFGetValue("PMC","Top_BS")
'					Brapi.ErrorLog.LogMessage(si,PMC)
					If Not PMC="" Then
						If PMC = "Top_BS" Then
							UD5Filter="U5#Top_BS.Base,U5#MO,U5#PO"
						Else
							UD5Filter= "U5#" & PMC
						End If
					Else
						UD5Filter="U5#Top_BS.Base,U5#MO,U5#PO"
					End If
					
					Return UD5Filter
				End If 
			#End Region 'GetUD5_Capex
			
			#Region "GetCountry"	
				If args.FunctionName.XFEqualsIgnoreCase("GetCountry") Then
					'used to retrieve parent entity for consolidated EUR reports like IC output cube view
					If scountry = "Czech" Then
						scountry= "Czech_Republic"
					Else If scountry = "Northern" Then
						scountry= "Northern_ireland"
'					Else If scountry = "Corporate"
'						scountry = "SubCons_Corp"
'					Else If scountry = "Croatia"
'						scountry = "Croatia_EUR"
'					Else If scountry = "UK"
'						scountry = "United_Kingdom"
						
					End If
					
					Return scountry

				End If 
			#End Region 'GetCountry
			
			#Region "GetCountry"	
				If args.FunctionName.XFEqualsIgnoreCase("GetCountry_IC") Then
					'used to retrieve parent entity for consolidated EUR reports like IC output cube view
					If scountry = "Czech" Then
						scountry= "Czech_Republic"
					Else If scountry = "Northern" Then
						scountry= "North_Ireland_EUR"
					Else If scountry = "Corporate"
						scountry = "SubCons_Corp"
					Else If scountry = "Croatia"
						scountry = "Croatia_EUR"
					Else If scountry = "UK"
						scountry = "United_Kingdom"
						
					End If
					
					Return scountry

				End If 
			#End Region 'GetCountry
			
			#Region "GetUD1Target_Tax"
				If args.FunctionName.XFEqualsIgnoreCase("GetUD1Target_Tax") Then
					
					
					If scountry = "Czech" Then
						scountry= "Czech_Republic"
					Else If scountry  = "Northern" Then
						scountry= "Northern_Ireland"
					End If
							
					Dim Selection As String = args.NameValuePairs.XFGetValue("Selection")
					Dim UD1MemberList = $"U1#{scountry}.base"
					Dim UD1PK As String = $"UD1_200_{scountry}"
					Dim TopUd1 As String = scountry
					
					If scountry = "Ireland" Then
						UD1MemberList = "U1#Republic_of_Ireland.base"
					Else If scountry = "Northern_Ireland" Then
						UD1PK= "UD1_200_Ireland"
						UD1MemberList = "U1#Northern_Ireland.base"
					Else If scountry = "UK" Then
						UD1MemberList = "U1#United_Kingdom.base"
					End If		 

					
					Dim UD1DimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, UD1PK)			
					Dim UD1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1DimPK, UD1MemberList, True)
					Dim FilteredUD1List As String = ""

						For Each UD1Member In UD1List

							Dim UD1Membername As String = UD1Member.Member.Name	
							Dim UD1Id As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,UD1Membername)
							Dim BudgetId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Scenario.Id,"Budget")
'							Dim ScenarioId As Integer = api.Scenario.GetScenarioType(api.Members.GetMemberId(dimtype.Scenario.Id, "Budgetv1")).id 'api.Scenario.GetScenarioTypeId(api.Members.GetMemberId(dimtype.Scenario.Id, "Budgetv1")).id
							Dim scenarioid As Integer = BRApi.Finance.Scenario.GetScenarioType(si, BRApi.Finance.Members.GetMemberId(si,dimtype.Scenario.Id,"Budgetv1")).id
'							Dim UD1Text2_Budget As String = BRApi.Finance.UD.Text(si, dimtype.UD1.Id, UD1Id,2,BRApi.Workflow.General.GetScenarioTypeId(si, si.WorkflowClusterPk),si.WorkflowClusterPk.TimeKey)
							Dim UD1Text2_Budget As String = BRApi.Finance.UD.Text(si, dimtype.UD1.Id, UD1Id,2,ScenarioId,si.WorkflowClusterPk.TimeKey)
'							brapi.errorlog.LogMessage(si, "UD1Text2_Budget = " & " " & UD1membername & " "& UD1Text2_Budget)
							
							If UD1Text2_Budget.Contains("LE_")
								
'							Dim LegalEntity As String = Ud1text2_budget
     						Dim LegalEntityDesc As String = BRApi.Finance.Metadata.GetMember(si, dimtype.UD1.Id, UD1Text2_Budget).Member.Description
     						Dim LegalEntityCode As String = BRApi.Finance.Metadata.GetMember(si, dimtype.UD1.Id, UD1Text2_Budget).Member.Name
'							filteredUD1List= filteredUD1List & ", UD1#" & UD1Membername & ":Name(" & LegalEntityDesc &")" 
									If Selection = "Display"
'									filteredUD1List= filteredUD1List & ", " & LegalEntityDesc.Replace(",",";") 
									filteredUD1List= filteredUD1List & ", " & LegalEntityCode.Replace("LE_","") & " - " & LegalEntityDesc.Replace(",",";") 
									Else 
									filteredUD1List = filteredUD1List & ", " & UD1Membername
									End If
							
							
							End If
						
						Next
						
						If filteredUD1List.Length > 0
							filteredUD1List = filteredUD1List.Remove(0,1)
						End If	
						
						
					Return filteredUD1List
				
				End If
			#End Region 'GetUD1Target_Tax
					
			#Region "taxListofLegalentitiesUD1s"		
				If args.FunctionName.XFEqualsIgnoreCase("taxListofLegalentitiesUD1s") Then
					If scountry = "Czech" Then
						scountry= "Czech_Republic"
					Else If scountry  = "Northern" Then
						scountry= "Northern_Ireland"
					End If
							
					Dim Selection As String = args.NameValuePairs.XFGetValue("Selection")
					Dim UD1MemberList = $"U1#{scountry}.base"
					Dim UD1PK As String = $"UD1_200_{scountry}"
					Dim TopUd1 As String = scountry
					
					If scountry = "Ireland" Then
						UD1MemberList = "U1#Republic_of_Ireland.base"
					Else If scountry = "Northern_Ireland" Then
						UD1PK= "UD1_200_Ireland"
						UD1MemberList = "U1#Northern_Ireland.base"
					Else If scountry = "UK" Then
						UD1MemberList = "U1#United_Kingdom.base"
					End If		 

					
					Dim UD1DimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, UD1PK)			
					Dim UD1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1DimPK, UD1MemberList, True)
					Dim FilteredUD1List As String = ""

						For Each UD1Member In UD1List

							Dim UD1Membername As String = UD1Member.Member.Name	
							Dim UD1Id As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,UD1Membername)
							Dim BudgetId As Integer = BRApi.Finance.Members.GetMemberId(si, dimtype.Scenario.Id,"Budget")
'							Dim ScenarioId As Integer = api.Scenario.GetScenarioType(api.Members.GetMemberId(dimtype.Scenario.Id, "Budgetv1")).id 'api.Scenario.GetScenarioTypeId(api.Members.GetMemberId(dimtype.Scenario.Id, "Budgetv1")).id
							Dim scenarioid As Integer = BRApi.Finance.Scenario.GetScenarioType(si, BRApi.Finance.Members.GetMemberId(si,dimtype.Scenario.Id,"Budgetv1")).id
'							Dim UD1Text2_Budget As String = BRApi.Finance.UD.Text(si, dimtype.UD1.Id, UD1Id,2,BRApi.Workflow.General.GetScenarioTypeId(si, si.WorkflowClusterPk),si.WorkflowClusterPk.TimeKey)
							Dim UD1Text2_Budget As String = BRApi.Finance.UD.Text(si, dimtype.UD1.Id, UD1Id,2,ScenarioId,si.WorkflowClusterPk.TimeKey)
'							brapi.errorlog.LogMessage(si, "UD1Text2_Budget = " & " " & UD1membername & " "& UD1Text2_Budget)
							
							If UD1Text2_Budget.Contains("LE_")
								
'							Dim LegalEntity As String = Ud1text2_budget
     						Dim LegalEntityDesc As String = BRApi.Finance.Metadata.GetMember(si, dimtype.UD1.Id, UD1Text2_Budget).Member.Description
     						Dim LegalEntityCode As String = BRApi.Finance.Metadata.GetMember(si, dimtype.UD1.Id, UD1Text2_Budget).Member.Name
'							filteredUD1List= filteredUD1List & ", UD1#" & UD1Membername & ":Name(" & LegalEntityDesc &")" 
'									If Selection = "Display"
'									filteredUD1List= filteredUD1List & ", " & LegalEntityDesc.Replace(",",";") 
'									filteredUD1List= filteredUD1List & ",  & LegalEntityCode.Replace("LE_","") & " - " & LegalEntityDesc.Replace(",",";") 
'									Else 
									filteredUD1List = filteredUD1List & ",ud1#" & UD1Membername &":NAME("&LegalEntityCode & "-" & LegalEntityDesc & ")"
'									End If
							
							
							End If
						
						Next
						
						If filteredUD1List.Length > 0
							filteredUD1List = filteredUD1List.Remove(0,1)
						End If	
						
						
					Return filteredUD1List
				
				End If
			
			#End Region 'taxListofLegalentitiesUD1s
			
			#Region "ReturnLocalEntity"
				If args.FunctionName.XFEqualsIgnoreCase("ReturnLocalEntity") Then
					
					Dim EntInputOP As String = args.NameValuePairs.XFGetValue("EntityInputOP")
					Dim EntMemberList = $"E#Input_Plan.base.remove(Inter_Country, Intra_Country)"
					Dim EntDimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"Ent_100_Group")		
					Dim EntList As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, EntDimPK, EntMemberList, True)
					
					For Each EntMember In EntList
						
						If EntInputOP = "Bosnia_Input_OP" Then
							
							Return "Bosnia_LC" 
							
						ElseIf EntInputOP = "Corporate_Input_OP"
							
							Return "SubCons_CORP"
							
						ElseIf EntInputOP = "Croatia_Input_OP"
							
							Return "Croatia_EUR"
							
						ElseIf EntInputOP = "Czech_Republic_Input_OP"
							
							Return "Czech_Republic_LC"
							
						ElseIf EntInputOP = "Greece_Input_OP"
							
							Return "Greece"
							
						ElseIf EntInputOP = "Hungary_Input_OP"
							
							Return "Hungary_LC"
							
						ElseIf EntInputOP = "Ireland_Input_OP"
							
							Return "South_Ireland"
							
						ElseIf EntInputOP = "Northern_Ireland_Input_OP"
							
							Return "North_Ireland_LC"
							
						ElseIf EntInputOP = "Italy_Input_OP"
							
							Return "Italy"
							
						ElseIf EntInputOP = "Lithuania_Input_OP"
							
							Return "Lithuania"
							
						ElseIf EntInputOP = "Poland_Input_OP"
							
							Return "Poland_LC"
							
						ElseIf EntInputOP = "Portugal_Input_OP"
							
							Return "Portugal"
							
						ElseIf EntInputOP = "Romania_Input_OP"
							
							Return "Romania_LC"
							
						ElseIf EntInputOP = "Spain_Input_OP"
							
							Return "Spain"
							
						ElseIf EntInputOP = "Serbia_Input_OP"
							
							Return "Serbia_LC"
							
						ElseIf EntInputOP = "Switzerland_Input_OP"
							
							Return "Switzerland_LC"
							
						ElseIf EntInputOP = "Turkey_Input_OP"
							
							Return "Turkey_LC"
							
						ElseIf EntInputOP = "UK_Input_OP"
							
							Return "United_Kingdom_LC"
							
						End If
						
					Next
						
					
				End If 
			#End Region 'ReturnLocalEntity
			
			#Region "Get Contingency per country"
			
				If args.FunctionName.XFEqualsIgnoreCase("Get_Contingency_Member") Then
					
'					XFBR(AQS_GetList_PLAN, Get_Contingency_Member)
					Dim WorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
					Dim country As String = WorkflowName.Split("_")(0)

					If country = "Czech" Then
						country= "Czech_Republic"
					Else If country = "Northern" Then
						country= "Northern_Ireland"
					End If
					
					Return country & "_Contingency"					
				
				End If
			
			
			#End Region 'Return UD1 Contingency member based on the Country WF
				
			#Region "GetWhereCapexSQLEditor"
				If args.FunctionName.XFEqualsIgnoreCase("GetWhere") Then
					Dim ret As String = $"WFScenario='{mbrScenarioMember.Name}' AND WFTime = '{mbrTimeMember.name}' AND Country = |!prm_GetWorkflowProfile_Plan!|"
					
					Return ret
				End If
				
				If args.FunctionName.XFEqualsIgnoreCase("GetWhereAllCountries") Then
					Dim ret As String = $"WFScenario='{mbrScenarioMember.Name}' AND WFTime = '{mbrTimeMember.name}'"
					
					Return ret
				End If
			#End Region 'GetWhereCapexSQLEditor
		
			#Region "GetWhereVATSQLEditor"
				If args.FunctionName.XFEqualsIgnoreCase("GetWhereVAT") Then
						
					Dim ret As String = $"WFScenario='{mbrScenarioMember.Name}' AND WFTime = '{mbrTimeMember.name}' AND Country = |!prm_GetWorkflowProfile_Plan!|"
						
					Return ret
					
				End If
			#End Region 'GetWhereVATSQLEditor
		
			If args.FunctionName.XFEqualsIgnoreCase("GetUD1") Then
				
			Return "UD1_200_" & scountry 
			End If 

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace