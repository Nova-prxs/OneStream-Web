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

Namespace OneStream.BusinessRule.DashboardStringFunction.AQS_GetList_PLAN
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
			#Region "Get_SourceScenario"
				If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_SourceScenario") Then
					'XFBR(AQS_GetList_PLAN, AQS_Get_SourceScenario)
					Dim SourceScenario As String = BRApi.Finance.Scenario.Text(si,si.WorkflowClusterPk.ScenarioKey,2)
					
					If Not SourceScenario = "" Then
						Return SourceScenario
					End If
					
				End If

			#End Region 'Get_SourceScenario	
			
			#Region "Get_SourceScenario_5YP"
				If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_SourceScenario_5YP") Then
					'XFBR(AQS_GetList_PLAN, AQS_Get_SourceScenario_5YP, TargetYear = 0)
'					Dim SourceScenario As String = BRApi.Finance.Scenario.Text(si,si.WorkflowClusterPk.ScenarioKey,2)
					Dim TargetYear As String = Args.NameValuePairs.XFGetValue("TargetYear", "1")
					
					If TargetYear = "0"
						Dim SourceScenario As String = BRApi.Finance.Scenario.Text(si,si.WorkflowClusterPk.ScenarioKey,1)
						If Not SourceScenario = "" Then
							Return SourceScenario.Replace("SourceYear0: ", "")
						End If
					ElseIf TargetYear = "1"
						Dim SourceScenario As String = BRApi.Finance.Scenario.Text(si,si.WorkflowClusterPk.ScenarioKey,2)
						If Not SourceScenario = "" Then
							Return SourceScenario.Replace("SourceYear1: ", "")
						End If
					End If	
					
				End If

			#End Region 'Get_SourceScenario
				
			#Region "Get_TimeFilter"
				If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_TimeFilter") Then
					'XFBR(AQS_GetList_PLAN, AQS_Get_TimeFilter)
					Dim Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					If Not Timefilter = ""
						Return Timefilter' brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Else 
						Return "T#2023M1"
					End If 
					
				End If 	
				If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_TimeFilter_Counter") Then
					'XFBR(AQS_GetList_PLAN, AQS_Get_TimeFilter_Counter)
					Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Timefilter As String = ""
					Dim Month As String = ""
					If Not Counter_Timefilter = ""
						For i As Integer = 1 To 12 Step 1
							Month = "M" & i.ToString & ","
							If Not Counter_Timefilter.XFContainsIgnoreCase(Month)
								If Timefilter = ""
									Timefilter = "T#|WFYear|M" & i.ToString
								Else
									Timefilter = Timefilter & ", T#|WFYear|M" & i.ToString
								End If 
							End If 
						Next
'						BRApi.ErrorLog.LogMessage(Si, "AQS_Get_TimeFilter_Counter " & Timefilter)
						Return Timefilter
					Else 
						Return "T#2023M1"
					End If 
				End If	
			#End Region 'Get_TimeFilter		
			
			#Region "Get_TimeFilter_PerScenario"
			
				If args.FunctionName.XFEqualsIgnoreCase("Get_TimeFilter_PerScenario") Then
					'XFBR(AQS_GetList_PLAN, Get_TimeFilter_PerScenario)
					Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Timefilter As String = ""
					Dim Month As String = ""
					Dim ScenarioMember As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
					
					If ScenarioMember.XFContainsIgnoreCase("Budget") Then
						Timefilter= "T#|WFYear|.base"
					
					Else If ScenarioMember.XFContainsIgnoreCase("Q_FCST") Or  ScenarioMember.XFContainsIgnoreCase("FC_") Then
						
						If Not Counter_Timefilter = ""
							For i As Integer = 1 To 12 Step 1
								Month = "M" & i.ToString & ","
								If Not Counter_Timefilter.XFContainsIgnoreCase(Month)
									If Timefilter = ""
										Timefilter = "T#|WFYear|M" & i.ToString
									Else
										Timefilter = Timefilter & ", T#|WFYear|M" & i.ToString
									End If 
								End If 
							Next
						Else 
							Timefilter = "T#2023M1"
						End If
					
					Else If ScenarioMember.XFContainsIgnoreCase("5YP") Then 
						Timefilter = "T#|WFYear|,T#|WFYearNext1|,T#|WFYearNext2|,T#|WFYearNext3|,T#|WFYearNext4|,T#|WFYearNext5|"
						
					End If	
						
							Return Timefilter
							
				End If			
			
			#End Region 'Get_TimeFilter_PerScenario	
			
			#Region "Get_TimeFilter_PerScenario_ConsoYears"
			
				If args.FunctionName.XFEqualsIgnoreCase("Get_TimeFilter_PerScenario_ConsoYears") Then
					'XFBR(AQS_GetList_PLAN, Get_TimeFilter_PerScenario_ConsoYears)
'					Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Timefilter As String = "T#|WFYear|"
'					Dim Month As String = ""
					Dim ScenarioMember As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
					
					If ScenarioMember.XFContainsIgnoreCase("Budget") Or ScenarioMember.XFContainsIgnoreCase("Q_FCST") Or  ScenarioMember.XFContainsIgnoreCase("FC_") Then
						Timefilter= "T#|WFYear|M12"
					
					Else If ScenarioMember.XFContainsIgnoreCase("5YP") Then
						Timefilter = "T#|WFYear|, T#|WFYearNext|, T#|WFYearNext2|, T#|WFYearNext3|, T#|WFYearNext4|, T#|WFYearNext5|"
						
					End If	
						
							Return Timefilter
							
				End If			
			
			#End Region 'Get_TimeFilter_PerScenario_ConsoYears		
			
			#Region "Get_TimeFilter_PerScenario_BSheetColumns"
			
				If args.FunctionName.XFEqualsIgnoreCase("Get_TimeFilter_PerScenario_BSheetColumns") Then
					'XFBR(AQS_GetList_PLAN, Get_TimeFilter_PerScenario_BSheetColumns)
'					Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Timefilter As String = "T#|WFYear|.Base"
'					Dim Month As String = ""
					Dim ScenarioMember As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
					
					If ScenarioMember.XFContainsIgnoreCase("Budget") Or ScenarioMember.XFContainsIgnoreCase("Q_FCST") Or  ScenarioMember.XFContainsIgnoreCase("FC_") Then
						Timefilter= "T#|WFYear|.Base, T#|WFYear|"
					
					Else If ScenarioMember.XFContainsIgnoreCase("5YP") Then
						Timefilter = "T#|WFYear|, T#|WFYearNext|, T#|WFYearNext2|, T#|WFYearNext3|, T#|WFYearNext4|, T#|WFYearNext5|"
						
					End If	
						
							Return Timefilter
							
				End If			
			
			#End Region 'Get_TimeFilter_PerScenario_ConsoYears			
			
			#Region "Get_TimeFilter_%Capitalized_IFRS16"
			
				If args.FunctionName.XFEqualsIgnoreCase("Get_TimeFilter_%Capitalized_IFRS16") Then
					'XFBR(AQS_GetList_PLAN, Get_TimeFilter_%Capitalized_IFRS16)
					Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Timefilter As String = ""
					Dim Month As String = ""
					Dim ScenarioMember As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
					
					'19/09/2024: to facilitate the seeding of IFRS16, the Input month for Budgetv1,Budgetv2, Q_FCST scenarios changed to be M12
					If ScenarioMember.XFContainsIgnoreCase("Budget") Or ScenarioMember.XFContainsIgnoreCase("Q_FCST") Or  ScenarioMember.XFContainsIgnoreCase("FC_")Then
						Timefilter= "T#WFYearM12:Name(|WFYEAR|)"
					
'					Else If ScenarioMember.XFContainsIgnoreCase("Q_FCST") Or  ScenarioMember.XFContainsIgnoreCase("FC_") Then
						
'						If Not Counter_Timefilter = ""
'							For i As Integer = 1 To 12 Step 1
'								Month = "M" & i.ToString & ","
'								If Not Counter_Timefilter.XFContainsIgnoreCase(Month)
'									If Timefilter = ""
'										Timefilter = "T#|WFYear|M" & i.ToString & ":Name(|WFYEAR|)"
''									Else
''										Timefilter = Timefilter & ", T#|WFYear|M" & i.ToString
'									End If 
'								End If 
'							Next
'						Else 
'							Timefilter = "T#2023M1"
'						End If
					
					Else If ScenarioMember.XFContainsIgnoreCase("5YP") Then 
						Timefilter = "T#|WFYear|:Name(|WFYEAR|)"
					End If	
						
							Return Timefilter
				End If
			#End Region 'Get_TimeFilter_%Capitalized_IFRS16	
			
			#Region "Get_SelectedTimeFilter"
				'with the selected Time
				If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_SelectedTimeFilter") Then
					'XFBR(AQS_GetList_PLAN, AQS_Get_SelectedTimeFilter, SelectedTime=|!prm_selectTime!|)
					Dim WFTime As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey).Name
					Dim currentYear As String = DateTime.Now.Year.ToString
					Dim SelectedTime As String = Args.NameValuePairs.XFGetValue("SelectedTime", WFTime)'BRApi.ErrorLog.LogMessage(Si, "SelectedTime :" & SelectedTime)
					If Not WFTime = currentYear And SelectedTime.Length=4 Then SelectedTime = WFTime
				
					If SelectedTime="" Or SelectedTime = WFTime
					
						Dim Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
						If Not Timefilter = ""
							Return Timefilter' brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
						Else 
							Return "T#2023M1"
						End If 
					
					Else
							Return "T#" & SelectedTime
					End If
				End If	
					
				If args.FunctionName.XFEqualsIgnoreCase("AQS_Get_SelectedTimeFilter_Counter") Then
					'XFBR(AQS_GetList_PLAN, AQS_Get_SelectedTimeFilter_Counter, SelectedTime=|!prm_selectTime!|)
					Dim WFTime As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey).Name
					Dim currentYear As String = DateTime.Now.Year.ToString
					Dim SelectedTime As String = Args.NameValuePairs.XFGetValue("SelectedTime", WFTime)'BRApi.ErrorLog.LogMessage(Si, "SelectedTime :" & SelectedTime)
					If Not WFTime = currentYear And SelectedTime.Length=4 Then SelectedTime = WFTime
				
					Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Month As String = ""
					
					If SelectedTime="" Or SelectedTime = WFTime
						
						If Not Counter_Timefilter = ""
							For i As Integer = 1 To 12 Step 1
								Month = "M" & i.ToString & ","
								If Not Counter_Timefilter.XFContainsIgnoreCase(Month)
									If Timefilter = ""
										Timefilter = "T#|WFYear|M" & i.ToString
									Else
										Timefilter = Timefilter & ", T#|WFYear|M" & i.ToString
									End If 
								End If 
							Next
	'						BRApi.ErrorLog.LogMessage(Si, "AQS_Get_TimeFilter_Counter " & Timefilter)
							Return Timefilter
						Else 
							Return "T#2023M1"
						End If 
						
					Else
							Return "T#" & SelectedTime
					End If
				End If
				#End Region 'Get_SelectedTimeFilter 	
				
			#Region "Get_TimeFilter_FCST"
			
				If args.FunctionName.XFEqualsIgnoreCase("Get_TimeFilter_FCST") Then
					'XFBR(AQS_GetList_PLAN, Get_TimeFilter_PerScenario)
					Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
					Dim Month As String = ""
					Dim ScenarioMember As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
					
						
						If Not Counter_Timefilter = ""
							For i As Integer = 12 To 1 Step -1
								Month = "M" & i.ToString & ","
								If Not Counter_Timefilter.XFContainsIgnoreCase(Month)
									If Timefilter = ""
										Timefilter = "T#|WFYear|M" & i.ToString
									Else
										Timefilter = Timefilter & ", T#|WFYear|M" & i.ToString
									End If 
								End If 
							Next
						Else 
							Timefilter = "T#2100"
						End If
					
						
							Return Timefilter
							
				End If			
			
#End Region				
				
			#Region "Cube View Format"	
				If args.FunctionName.XFEqualsIgnoreCase("FormatACT") Then
					'XFBR(AQS_GetList_PLAN, FormatACT, SelectedTime=|!prm_selectTime!|)
					Dim WFTime As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey).Name
					Dim currentYear As String = DateTime.Now.Year.ToString
					Dim SelectedTime As String = Args.NameValuePairs.XFGetValue("SelectedTime", WFTime)'BRApi.ErrorLog.LogMessage(Si, "SelectedTime :" & SelectedTime)
					If Not WFTime = currentYear And SelectedTime.Length=4 Then SelectedTime = WFTime
					
'					BRApi.ErrorLog.LogMessage(Si, "SelectedTime :" & SelectedTime)
					
					Dim format As String=""
					
					If SelectedTime.Length > 4 
						If Not SelectedTime="" Or Not SelectedTime = WFTime
						 
						Dim SelectedTimeComma As String  = SelectedTime.Substring(SelectedTime.IndexOf("M")) & ","' SelectedTime
							Dim Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
							If Not Timefilter = ""
								If Not Timefilter.XFContainsIgnoreCase(SelectedTimeComma)
									format = " IsColumnVisible = False "
								End If
							End If
						End If
					End If
							Return format
				End If
				If args.FunctionName.XFEqualsIgnoreCase("FormatBUDV2") Then
					'XFBR(AQS_GetList_PLAN, FormatBUDV2, SelectedTime=|!prm_selectTime!|)
					Dim WFTime As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey).Name
					Dim currentYear As String = DateTime.Now.Year.ToString
					Dim SelectedTime As String = Args.NameValuePairs.XFGetValue("SelectedTime", WFTime)'BRApi.ErrorLog.LogMessage(Si, "SelectedTime :" & SelectedTime)
					If Not WFTime = currentYear And SelectedTime.Length=4 Then SelectedTime = WFTime
					
'					BRApi.ErrorLog.LogMessage(Si, "SelectedTime :" & SelectedTime)
					
					Dim format As String=""
					
					If SelectedTime.Length > 4 
						If Not SelectedTime="" Or Not SelectedTime = WFTime
						 
						Dim SelectedTimeComma As String  = SelectedTime.Substring(SelectedTime.IndexOf("M")) & ","' SelectedTime
							Dim Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
							If Not Timefilter = ""
								If Timefilter.XFContainsIgnoreCase(SelectedTimeComma)
									format = " IsColumnVisible = False "
								End If
							End If
						End If
					End If
							Return format
				End If	
					
			#End Region 'Cube View Format 
			
			#Region "Get WF Entity"	
				If args.FunctionName.XFEqualsIgnoreCase("Get_WF_Entity") Then
'					XFBR(AQS_GetList_PLAN, Get_WF_Entity)
					Dim strentitiesname As String = "None"
					Dim WFEntitiesList As list(Of BaseNameIntPairInfo) = BRApi.Workflow.Metadata.GetDependentProfileEntities(si, si.WorkflowClusterPk.ProfileKey)
					If WFEntitiesList.Count <> 0
						For Each wfentity As BaseNameIntPairInfo In WFEntitiesList
							strentitiesname = wfentity.Name
						Next
					Else 
						strentitiesname = "Input_Plan.Base"
					End If
					Return strentitiesname
				End If	
			#End Region 'Get WF Entity
			
			#Region "Get Entity Currency"
				If args.FunctionName.XFEqualsIgnoreCase("Get_Entity_Currency") Then
'					XFBR(AQS_GetList_PLAN, Get_Entity_Currency)
					Dim strentitiesname As String = "None"
					Dim Currency As String = "None"
					Dim WFEntitiesList As list(Of BaseNameIntPairInfo) = BRApi.Workflow.Metadata.GetDependentProfileEntities(si, si.WorkflowClusterPk.ProfileKey)
					If WFEntitiesList.Count <> 0
						For Each wfentity As BaseNameIntPairInfo In WFEntitiesList
							'strentitiesname = wfentity.Name
							Currency = "Currency: " & brapi.Finance.Entity.GetLocalCurrency(si, wfentity.UniqueID).Name
						Next
					End If
					Return  Currency
				End If	
					
			#End Region 'Get Entity Currency
				
			#Region "Get WF Country"
				If args.FunctionName.XFEqualsIgnoreCase("Get_WF_Country") Then
					Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
'					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
'					Dim mbrTimeMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey)				
'					Dim prm_selectMedicalCenter As String = args.NameValuePairs.XFGetValue("prm_selectMedicalCenter", String.Empty)
					Dim scountry As String = ""
					If sWorkflowName.Contains("_") Then 
						If sWorkflowName.Split("_")(0) = "Czech" Then
							scountry= "Czech_Republic"
						Else If sWorkflowName.Split("_")(0)  = "Northern" Then
							scountry= "Northern_Ireland"
						Else
							scountry = sWorkflowName.Split("_")(0) 
						End If 
					Else
						scountry = "" 
					End If 
'					brapi.ErrorLog.LogMessage(si,"AN " & scountry & " - " & prm_selectMedicalCenter)
					Return scountry
				End If
			#End Region 'Get WF Country
					
			#Region "GetUD1"
					If args.FunctionName.XFEqualsIgnoreCase("GetUD1_RP") Then
						Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
'						If sWorkflowName.Contains("_")
						Dim scountry As String = sWorkflowName.Split("_")(0)
'						Else
'						Dim scountry As String = sWorkflowName
'						End If
						
						Dim ud1filter As String = "U1#XFBR(AQS_GetList_PLAN, Get_WF_Country)_region,U1#XFBR(AQS_GetList_PLAN, Get_WF_Country)_region.Base"
						If scountry = "Ireland" Then
							ud1filter= "U1#Republic_of_Ireland,U1#Republic_of_Ireland.Base"
						Else If scountry = "Northern" Then
							ud1filter= "U1#Northern_Ireland,U1#Northern_Ireland.Base"
						Else If scountry = "UK" Then
							ud1filter= "U1#United_Kingdom_region,U1#United_Kingdom_region.Base"
						End If

					Return ud1filter
					End If	
					
			#End Region 'GetUD1	
			
			#Region "Get_Time_For_Comment_Per_Scenario"
			
				If args.FunctionName.XFEqualsIgnoreCase("Get_Time_For_Comment_Per_Scenario") Then
					'XFBR(AQS_GetList_PLAN, Get_Time_For_Comment_Per_Scenario)
					
					Dim ScenarioMember As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
					
					If ScenarioMember.XFContainsIgnoreCase("5YP") Then 
						Return "T#|WFYear|"
					Else
						Return "T#|WFYear|M12"
					
					End If
					
				End If			
			
			#End Region 'Get_Time_For_Comment_Per_Scenario	
			
			
#Region "GetUD1 for Statistics Actual"
If args.FunctionName.XFEqualsIgnoreCase("GetUD1_RP_ACT") Then
    ' Get the workflow name
    Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
    
    ' Initialize scountry based on the workflow name
    Dim scountry As String = sWorkflowName.Split(".")(0)
    ' Set the UD1 filter based on the scountry value
'    Dim ud1filter As String = "U1#XFBR(AQS_GetList_PLAN, Get_WF_Country)_region,U1#XFBR(AQS_GetList_PLAN, Get_WF_Country)_region.Base"
    Dim ud1filter As String = "U1#"& scountry &"_region,U1#"& scountry &"_region.Base"
    
    Select Case scountry
        Case "Ireland"
            ud1filter = "U1#Republic_of_Ireland,U1#Republic_of_Ireland.Base"
        Case "Northern"
            ud1filter = "U1#Northern_Ireland,U1#Northern_Ireland.Base"
        Case "UK"
            ud1filter = "U1#United_Kingdom_region,U1#United_Kingdom_region.Base"
        ' Add a case for Portugal if specific logic is needed
        ' Case "Portugal"
        '     ud1filter = "U1#Portugal,U1#Portugal.Base"
    End Select

    Return ud1filter
End If
					
			#End Region 'GetUD1	
		
			
			
			
			#Region "GetUD1_ApprovedBSU"
					If args.FunctionName.XFEqualsIgnoreCase("GetUD1_ApprovedBSU") Then
						Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
						Dim scountry As String = sWorkflowName.Split("_")(0)
						
						Dim ud1filter As String = "U1#XFBR(AQS_GetList_PLAN, Get_WF_Country)_Legal.Descendants.where(name contains 'BSU')"
						If scountry = "Ireland" Then
							ud1filter= "U1#Republic_of_Ireland.Descendants.where(name contains 'BSU')"
						Else If scountry = "Northern" Then
							ud1filter= "U1#Northern_Ireland.Descendants.where(name contains 'BSU')"
						Else If scountry = "UK" Then
							ud1filter= "U1#United_Kingdom_Legal.Descendants.where(name contains 'BSU')"
						End If

					Return ud1filter
					End If	
					
			#End Region 'GetUD1	
			
			#Region "GetUD1_ApprovedBSU_BASE_placeholder"
				If args.FunctionName.XFEqualsIgnoreCase("GetUD1_ApprovedBSU_BASE_placeholder") Then
					Dim selectedBSU As String = Args.NameValuePairs.XFGetValue("GetUD1_ApprovedBSU", "")
					Dim SelectedBSUtext1 As String = BRApi.Finance.UD.Text(si, dimtype.UD1.id, brapi.Finance.Members.GetMemberId(si, dimtype.UD1.Id, selectedBSU), 1,0,0)
					Dim ud1filter As String = Nothing
					If SelectedBSUtext1 <> "" 
						ud1filter = SelectedBSUtext1.Replace("BSU_Base:", "UD1#")
					 Return ud1filter
					 
					Else
						ud1filter = "UD1#None"
					Return ud1filter
					
					End If	
				End If
					
			#End Region 'GetUD1_ApprovedBSU_BASE_placeholder	
			
			#Region "Get Entity dimension for Actual Scenario based on the currency"	
		If args.FunctionName.XFEqualsIgnoreCase("Get_Entity_Actual") Then
'					XFBR(AQS_GetList_PLAN, Get_Entity_Actual,Currency=|!prm_Select_Currency_Plan!|)
					Dim WorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
					
					Dim country As String = WorkflowName.Split("_")(0)
					
					Dim Currency As String = args.NameValuePairs.XFGetValue("Currency","Local")
'brapi.ErrorLog.LogMessage(si,"Test    : " & WorkflowName )
				If Currency = "Local" Then
					
					If country = "Switzerland" Or country = "Bosnia" Or country = "Poland" Or country = "Hungary" Or country = "Turkey" Or country = "Serbia" Then
						country = country & "_LC"
					Else If country = "Czech" Then
						country= "Czech_Republic_LC"
					Else If country = "UK" Then
						country= "United_Kingdom_LC"
					Else If country = "Northern" Then
						country= "North_Ireland_LC"
					Else If country = "Ireland" Then
						country= "South_Ireland"
					Else If country = "Croatia" Then
						country= "Croatia_EUR"
					Else If country = "Corporate" Then
						country= "SubCons_CORP"
					Else If country = "ContingencyCountry" Then
						country= "ContingencyCountry_Input_OP"
					End If
					
				Else If Currency = "EUR" Then
					
					If country = "Czech" Then
						country= "Czech_Republic"
					Else If country = "UK" Then
						country= "United_Kingdom"
					Else If country = "Northern" Then
						country= "North_Ireland_EUR"
					Else If country = "Ireland" Then
						country= "South_Ireland"
					Else If country = "Croatia" Then
						country= "Croatia_EUR"
					Else If country = "Corporate" Then
						country= "SubCons_CORP"
					Else If country = "ContingencyCountry" Then
						country= "ContingencyCountry_Input_OP"
					End If
					
				Else
					country=""
				End If 
				
'				If country = "Spain" Then Country = "E#Spain:getdatacell(A#0:F#F_Property_plant_and_equipment_Mov_1+A#0:F#F_Intangibles_Mov_2):S#Actual:NAME(Purchase - Actual)"
					Return "E#"& country
				End If
			#End Region 'Get Entity dimension for Actual Scenario based on the currency			

			#Region "GetDashboardName"
				'Get Dashboard Name based on the Scenario Type
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_RP"
					Else
						dashboard="2b_QFCSTData_RP"
					End If
					
					Return dashboard
				End If	
					
					
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_COS") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_COS"
					Else
						dashboard="2b_QFCSTData_COS"
					End If
					
					Return dashboard
				End If	
					
					
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_Direct_Fixed_Cost") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_Direct_Fixed_Cost"
					Else
						dashboard="2b_QFCSTData_Direct_Fixed_Cost"
					End If
					
					Return dashboard
				End If	
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_MD") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_MD"
					Else
						dashboard="2b_QFCSTData_MD"
					End If
					
					Return dashboard
				End If	
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_SGA") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_SGA"
					Else
						dashboard="2b_QFCSTData_SGA"
					End If

					Return dashboard
				End If	
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_Center_SGA") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_Center_SGA"
					Else
						dashboard="2b_QFCSTData_Center_SGA"
					End If

					Return dashboard
				End If	
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_BTL") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_BTL"
					Else
						dashboard="2b_QFCSTData_BTL"
					End If
					
					Return dashboard
				End If	
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_Depr") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_Depr"
					Else
						dashboard="2b_QFCSTData_Depr"
					End If
					
					Return dashboard
				End If	
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_IFRS16") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_IFRS16"
					Else
						dashboard="2b_QFCSTData_IFRS16"
					End If
					Return dashboard
				End If	
				If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_QFCST") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String =""
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Then
						dashboard ="2a_BUD_ContentMain_TAX"
					ElseIf mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard="2a_5YP_ContentMain_TAX"
					Else 
						dashboard="2a_QFCST_ContentMain_TAX"
					End If
					
					Return dashboard
				End If	
					
			#End Region 'GetDashboardName
			
			#Region "GetDashboardNameCONT"
			'Get dashboardname for RP contingency
			
			If args.FunctionName.XFEqualsIgnoreCase("GetDashboardNameContingency") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String = ""
					
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1_Input_RP_Contingency"
					End If
					
					Return dashboard
				End If	
			If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_COS_Contingency") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String = ""
					
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1_Input_COS_Contingency"
					End If
					
					Return dashboard
				End If	
					
			If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_DEPR_Contingency") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String = ""
					
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1_Input_DEPR_Contingency"
					End If
					
					Return dashboard
				End If	
					
			If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_MD_Contingency") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String = ""
					
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_MD_Contingency"
					End If
					
					Return dashboard
				End If	
			If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_DFC_Contingency") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String = ""
					
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_Direct_Fixed_Cost_Contingency"
					End If
					
					Return dashboard
				End If	
			If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_SGA_Contingency") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String = ""
					
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_SGA_Contingency"
					End If
					
					Return dashboard					
				End If	
			If args.FunctionName.XFEqualsIgnoreCase("GetDashboardName_BTL_Contingency") Then
					Dim mbrScenarioMember As Member = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey)
					Dim dashboard As String = ""
					
					If mbrScenarioMember.Name.XFContainsIgnoreCase("Budget") Or mbrScenarioMember.Name.XFContainsIgnoreCase("Q_FCST") Or mbrScenarioMember.Name.XFContainsIgnoreCase("5YP") Then
						dashboard ="2a_Budgetv1Data_BTL_Contingency"
					End If
					
					Return dashboard
					
				End If	
					
					
			
			#End Region 
			
			#Region "Reports"
			
				'Capex Purchase and Payment Report
				If args.FunctionName.XFEqualsIgnoreCase("Mapex_Filter_PurchasePayment") Then
					'XFBR(AQS_GetList_PLAN, Mapex_Filter_PurchasePayment,SelectedMbr=|!prm_SelectPurchasePayment_Report_Plan!|)
					Dim SelectedMbr As String = args.NameValuePairs.XFGetValue("SelectedMbr")
					
					If SelectedMbr Is Nothing Or SelectedMbr="" Or  SelectedMbr.Contains("Purchase") Then
						Return "Getdatacell(A#1:U5#Maintenance_P:F#F_Property_plant_and_equipment_Mov_1+ A#1:U5#Maintenance_P:F#F_Intangibles_Mov_2 + A#Tech_Purchases:U5#MO:F#None):C#EUR:Name(|WFYear| - Mapex)"		
					Else 
						Return "Getdatacell(A#2:U5#Maintenance_P:F#F_Leases_ST_LT_Mov_3+ A#Tech_Payments:U5#MO:F#None):C#EUR:Name(|WFYear| - Mapex)"
					End If
				End If	
				'Capex Purchase and Payment Report	
				If args.FunctionName.XFEqualsIgnoreCase("Propex_Filter_PurchasePayment") Then
					'XFBR(AQS_GetList_PLAN, Propex_Filter_PurchasePayment,SelectedMbr=|!prm_SelectPurchasePayment_Report_Plan!|)
					Dim SelectedMbr As String = args.NameValuePairs.XFGetValue("SelectedMbr")
					
					If SelectedMbr Is Nothing Or SelectedMbr="" Or  SelectedMbr.Contains("Purchase") Then
						Return "Getdatacell(A#1:U5#Project_P:F#F_Property_plant_and_equipment_Mov_1+ A#1:U5#Project_P:F#F_Intangibles_Mov_2 + A#Tech_Purchases:U5#PO:F#None):C#EUR:Name(|WFYear| - Propex)"		
					Else 
						Return "Getdatacell(A#2:U5#Project_P:F#F_Leases_ST_LT_Mov_3 + A#Tech_Payments:U5#PO:F#None):C#EUR:Name(|WFYear| - Propex)"
					End If
				End If	
				'Capex by Commodity Report	
				If args.FunctionName.XFEqualsIgnoreCase("Filter_CapexByCommodity") Then
					'XFBR(AQS_GetList_PLAN, Filter_CapexByCommodity,SelectedMbr=|!prm_SelectPurchasePayment_Report_Plan!|,SelectMbr1=|!prm_SelectMapexPropex_Report_Plan!|)
					Dim SelectedMbr As String = args.NameValuePairs.XFGetValue("SelectedMbr")
					Dim SelectedMbr1 As String = args.NameValuePairs.XFGetValue("SelectMbr1")
'					brapi.ErrorLog.LogMessage(si,SelectedMbr & SelectedMbr1)
					If SelectedMbr1 Is Nothing Or SelectedMbr1="" Or SelectedMbr1="Mapex" Then
						If SelectedMbr Is Nothing Or SelectedMbr="" Or  SelectedMbr.Contains("Purchase") Then
							Return "Getdatacell(A#1:U5#Maintenance_P:F#F_Property_plant_and_equipment_Mov_1+ A#1:U5#Maintenance_P:F#F_Intangibles_Mov_2 + A#Tech_Purchases:U5#MO:F#None)"		
						Else 
							Return "Getdatacell(A#2:U5#Maintenance_P:F#F_Leases_ST_LT_Mov_3+ A#Tech_Payments:U5#MO:F#None)"
						End If
					Else
						If SelectedMbr Is Nothing Or SelectedMbr="" Or  SelectedMbr.Contains("Purchase") Then
							Return "Getdatacell(A#1:U5#Project_P:F#F_Property_plant_and_equipment_Mov_1+ A#1:U5#Project_P:F#F_Intangibles_Mov_2 + A#Tech_Purchases:U5#PO:F#None)"		
						Else 
							Return "Getdatacell(A#2:U5#Project_P:F#F_Leases_ST_LT_Mov_3 + A#Tech_Payments:U5#PO:F#None)"
						End If
					End If
				End If	
					'Capex by Commodity Report	
					If args.FunctionName.XFEqualsIgnoreCase("Filter_CapexByCommodityVariance") Then
						'XFBR(AQS_GetList_PLAN, Filter_CapexByCommodityVariance,SelectedMbr=|!prm_SelectPurchasePayment_Report_Plan!|,SelectMbr1=|!prm_SelectMapexPropex_Report_Plan!|,QFSCTScen=|!prm_SelectQFCSTScenario_Report_Plan!|)
						Dim SelectedMbr As String = args.NameValuePairs.XFGetValue("SelectedMbr")
						Dim SelectedMbr1 As String = args.NameValuePairs.XFGetValue("SelectMbr1")
						Dim QFSCTScen As String = args.NameValuePairs.XFGetValue("QFSCTScen")
'						brapi.ErrorLog.LogMessage(si,QFSCTScen )
						If SelectedMbr1 Is Nothing Or SelectedMbr1="" Or SelectedMbr1="Mapex" Then
							If SelectedMbr Is Nothing Or SelectedMbr="" Or  SelectedMbr.Contains("Purchase") Then
								Return $"Getdatacell((S#WF:A#1:U5#Maintenance_P:F#F_Property_plant_and_equipment_Mov_1+ S#WF:A#1:U5#Maintenance_P:F#F_Intangibles_Mov_2 + S#WF:A#Tech_Purchases:U5#MO:F#None) - (S#{QFSCTScen}:A#1:U5#Maintenance_P:F#F_Property_plant_and_equipment_Mov_1+ S#{QFSCTScen}:A#1:U5#Maintenance_P:F#F_Intangibles_Mov_2 + S#{QFSCTScen}:A#Tech_Purchases:U5#MO:F#None))"		
							Else 
								Return $"Getdatacell((S#WF:A#2:U5#Maintenance_P:F#F_Leases_ST_LT_Mov_3+ S#WF:A#Tech_Payments:U5#MO:F#None)-(S#{QFSCTScen}:A#2:U5#Maintenance_P:F#F_Leases_ST_LT_Mov_3+ S#{QFSCTScen}:A#Tech_Payments:U5#MO:F#None))"
							End If
						Else
							If SelectedMbr Is Nothing Or SelectedMbr="" Or  SelectedMbr.Contains("Purchase") Then
								Return $"Getdatacell((S#WF:A#1:U5#Project_P:F#F_Property_plant_and_equipment_Mov_1+ S#WF:A#1:U5#Project_P:F#F_Intangibles_Mov_2 + S#WF:A#Tech_Purchases:U5#PO:F#None)-(S#{QFSCTScen}:A#1:U5#Project_P:F#F_Property_plant_and_equipment_Mov_1+ S#{QFSCTScen}:A#1:U5#Project_P:F#F_Intangibles_Mov_2 + S#{QFSCTScen}:A#Tech_Purchases:U5#PO:F#None))"		
							Else 
								Return $"Getdatacell((S#WF:A#2:U5#Project_P:F#F_Leases_ST_LT_Mov_3 + S#WF:A#Tech_Payments:U5#PO:F#None)-(S#{QFSCTScen}:A#2:U5#Project_P:F#F_Leases_ST_LT_Mov_3 + S#{QFSCTScen}:A#Tech_Payments:U5#PO:F#None))"
							End If
						End If
					End If
					
					'Get Purchase or Payment: Cube view Header for Capex by Commodity Report/Capex Purchase and Payment Report
					If args.FunctionName.XFEqualsIgnoreCase("GetPurchaseOrPayment") Then
						'XFBR(AQS_GetList_PLAN, GetPurchaseOrPayment,SelectedMbr=|!prm_SelectPurchasePayment_Report_Plan!|)
						Dim SelectedMbr As String = args.NameValuePairs.XFGetValue("SelectedMbr")
						If SelectedMbr Is Nothing Or SelectedMbr="" Or  SelectedMbr.Contains("Purchase") Then
							Return "Purchase"
						Else
							Return "Payment"
						End If
					End If
			#End Region 'Reports	
			
				
				
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace