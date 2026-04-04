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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_navigation
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					#Region "Load Dashboard"
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("LoadDashboard") Then
							'Get workspace prefix
							Dim WorkspacePrefix As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "CCAA.prm_CCAA_Workspace_Prefix")
							'Declare load dashboard task result
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
							'Declare current dashboard
							Dim paramDashboard As String
							
							'If first load, get default parameter, else use the subst var from prior run
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize _
								And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								'Get default dashboard
								paramDashboard = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, $"CCAA.prm_{WorkspacePrefix}_MainNavigation_Default")
							Else
								'Get current dashboard parameter from other business rule call
								paramDashboard = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_CurrentDashboard")
							End If
							'Set current dashboard parameter
							loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard
							
							'Loop through each level of the dashboard based on the name
							Dim dashboardSplitted As String() = paramDashboard.Split("_")
							For level As Integer = 1 To dashboardSplitted.Count - 1
								Dim activeDashboardNumber As Integer = CInt(dashboardSplitted(level))
								
								'Set dashboard content and active tab
								Me.SetActiveContent(si, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
								Me.SetActiveTab(si, loadDashboardTaskResult, WorkspacePrefix, level, 20, activeDashboardNumber)
								'Set dashboard parameters
								Me.SetDashboardParametersOnLoad(si, args, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
							Next
							
							#Region "Define country and table"
							'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "CCAA.prm_StatusTableName", "XFC_CCAA_CubeView_Status_Argentina")
							
							' Obtain Workflow Profile
							Dim ProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk).Name
							'brapi.ErrorLog.LogMessage(si, $"{ProfileName}")
							
							
							Dim pais As String
							Dim codigo As String
							
							' Obtain country/Horse_Group and code through Workflow Profile
							If ProfileName = "WP_Horse_Brazil.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Brazil"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Brazil"
								pais = "Brazil"
								codigo = "R1303"
								'brapi.ErrorLog.LogMessage(si, $"entra")
							Else If ProfileName = "WP_Horse_Argentina.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Argentina"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Argentina"
								pais = "Argentina"
								codigo = "R0592"
							Else If ProfileName = "WP_Horse_Chile.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Chile"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Chile"
								pais = "Chile"
								codigo = "R0585"
							Else If ProfileName = "WP_Horse_Holding.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Holding"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Holding"
								pais = "Holding"
								codigo = "R1300"
							Else If ProfileName = "WP_Horse_Spain.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								'brapi.ErrorLog.LogMessage(si, $"{pais}")
								codigo = "R1301"
							Else If ProfileName = "WP_Horse_Portugal.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Portugal"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Portugal"
								pais = "Portugal"
								codigo = "R0671"
							Else If ProfileName = "WP_Horse_Romania.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Romania"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Romania"
								pais = "Romania"
								codigo = "R0611"
							Else If ProfileName = "WP_Horse_Turkey.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Turkey"
								codigo = "R1302"
							Else If ProfileName = "WP_Horse Group.Statutory"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Turkey"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Turkey"
								pais = "Group"
								codigo = ""
							Else
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = "XFC_CCAA_CubeView_Status_Spain"
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusShortTableName") = "XFC_CCAA_CubeView_StatusShort_Spain"
								pais = "Spain"
								codigo = "R1301"
							End If
							
							'Obtain Workflow Time and Define Status Table Name 
							Dim WorkflowTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
							Dim tabla As String = "XFC_CCAA_CubeView_Status_" & pais & "_" & WorkflowTime
							
							#Region "Create Status Table if Needed"
							
							' When a Workflow Profile (country/Horse_Group + Year-Month) is opened for the first time, the status table associated is created
							
							Dim notesList As New List(Of Tuple(Of String, String)) From {
							    New Tuple(Of String, String)("16B", "Detail of other liabilities"),
							    New Tuple(Of String, String)("17C", "Derivatives on financial operations"),
							    New Tuple(Of String, String)("18-D", "Breakdown by maturity, current"),
							    New Tuple(Of String, String)("18-D_NC", "Breakdown by maturity, non-current"),
							    New Tuple(Of String, String)("18C_A", "Conditions of borrowings and other interest-bearing liabilities from credit institutions"),
							    New Tuple(Of String, String)("18C_B", "Off-balance sheet commitments received"),
							    New Tuple(Of String, String)("18Ea", "Average period of payment to suppliers"),
							    New Tuple(Of String, String)("18Eb", "Average period of payment to suppliers"),
							    New Tuple(Of String, String)("18Ec", "Average period of payment to suppliers"),
							    New Tuple(Of String, String)("19B", "Risk Management"),
							    New Tuple(Of String, String)("20D", "Net change in financial liabilities"),
							    New Tuple(Of String, String)("22-A", "Off-balance sheet commitments. Ordinary operations"),
							    New Tuple(Of String, String)("23B", "Breakdown of employees by category and gender, average"),
							    New Tuple(Of String, String)("23C", "Directors and Senior Management"),
							    New Tuple(Of String, String)("24A", "Audit Fees.1"),
							    New Tuple(Of String, String)("24B", "Audit Fees.2"),
							    New Tuple(Of String, String)("24C", "Audit Fees.3"),
							    New Tuple(Of String, String)("8-B", "Breakdown of the tax charge"),
							    New Tuple(Of String, String)("8-D4", "Detail of tax rates applied by tax jurisdiction"),
							    New Tuple(Of String, String)("8D_b", "Breakdown of net deferred tax assets (liabilities) by nature"),
							    New Tuple(Of String, String)("8D_C", "Breakdown of deferred taxes on tax losses by expiry date")
							}
							
							' If Workflow Profile is Horse_Group, a table with all notes for each country is created if needed
							If pais = "Group" Then
								
								CreateHorseGroupStatusTable(si, tabla, notesList)
								
							' If Workflow Profile is a country, both the country table and the table with all countries are created if needed
							Else
								Dim tabla_Group As String = "XFC_CCAA_CubeView_Status_Group_" & WorkflowTime
								
								CreateHorseGroupStatusTable(si, tabla_Group, notesList)
								
								CreateCountryStatusTable(si, tabla, notesList, codigo, pais)
								

							End If

  							
														
							#End Region
							
							' Update Table Name parameter for the SQL Table Editor Components
							loadDashboardTaskResult.ModifiedCustomSubstVars("prm_StatusTableName") = $"{tabla}"
							
							' Update Table Column Name parameter depending on country or Horse_Group Workflow Profile
							If pais = "Group"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_StatusTable_TableColumnNames") = "country, note_name, send_status, check_status"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_DetailTable_TableColumnNames") = "country, note_name, last_save_name, last_save_date, send_status, send_name, send_date, check_status, check_name, check_date"
							Else
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_StatusTable_TableColumnNames") = "note_name, send_status, check_status"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_DetailTable_TableColumnNames") = "note_name, last_save_name, last_save_date, send_status, send_name, send_date, check_status, check_name, check_date"
							End If
							
							loadDashboardTaskResult.ModifiedCustomSubstVars("param_POV_Entity_001") = $"{codigo}001"
							#End Region
							
							#Region "SECURITY & Define CubeView code & C# for Automatic Notes"
							
							' Obtain the cube view code based on the dashboard hierarchy for manual notes
							' Establish which cube view is shown based on the country/Horse Group Workflow Profile
							' Establish security levels for the check button
							
							loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "False"
							loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "False"
							
							Dim userName As String = si.UserName.Replace("'", "''")
							
							' CORPORATE TAX
							If paramDashboard = "CCAA_1_1_1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "8-B")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note13_8B")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8B_Breakdown of the tax charge"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8B_Input"
								End If
								
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_CorporateTax", True) 
										
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
										
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										
									End If
								End If
								
								
							Else If paramDashboard = "CCAA_1_1_2" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "8D_b")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note17_8Db")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8D_b_Breakdown of net deferred tax assets (liabilities) by nature"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8D_b_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_CorporateTax", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If
								
							Else If paramDashboard = "CCAA_1_1_3" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "8D_C")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note18_8DC")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8D_C_Breakdown of deferred taxes on tax losses by expiry date"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8D_C_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_CorporateTax", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If
								
							Else If paramDashboard = "CCAA_1_1_4" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "8-D4")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note19_8D4")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8D_4_Detail of tax rates applied by tax jurisdiction"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "8D_4_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_CorporateTax", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If					
							
							' FINANCE AND TREASURY
							Else If paramDashboard = "CCAA_1_2_1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "17C")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note36_17C")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "17C_Derivatives on financial operations"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "17C_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_FinanceAndTreasury", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_2_2" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "18C_A")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note39_18CA")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18C_A_Changes in financial liabilities"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18C_A_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_FinanceAndTreasury", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_2_3" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "18C_B")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note40_18CB")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18C_B_Off balance sheet commitments received"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18C_B_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_FinanceAndTreasury", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_2_4" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "18-D")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note41_18D")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18D_Curr_Breakdown by maturity"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18D_Curr_input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_FinanceAndTreasury", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_2_5" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "18-D_NC")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note42_18D")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18D_NonCurr_Breakdown by maturity"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18D_NonCurr_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_FinanceAndTreasury", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_2_6" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "19B")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note62_19B")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "19B_Risk Management"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "19B_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_FinanceAndTreasury", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
							
							' OTHER INFORMATION
							Else If paramDashboard = "CCAA_1_3_1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "18Ea")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note43_18Ea")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18E_a_Average period of payment to suppliers"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18E_a_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_3_2" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "18Eb")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note44_18Eb")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18E_b_Average period of payment to suppliers"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18E_b_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_3_3" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "18Ec")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note45_18Ec")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18E_c_Average period of payment to suppliers"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "18E_c_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_3_4" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "22-A")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note53_22A")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "22A_Ordinary operations"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "22A_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
							
							' FINANCIAL STATEMENTS
							Else If paramDashboard = "CCAA_1_4_1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "20D")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note50_20D")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "20D_Net change in financial liabilities"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "20D_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_FinancialStatements", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
							
							' STAFF AND PERSONNEL
							Else If paramDashboard = "CCAA_1_5_1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "23C")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note56_23C")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "23C_Directors and Senior Management"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "23C_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_5_2" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "23B")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note55_23B")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "23B_Breakdown Of employees By category And gender_average"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "23B_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
							
							' PL INFORMATION
							Else If paramDashboard = "CCAA_1_6_1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "24A")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note57_24A")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "24A_Fees concerning audit services incurred by the consolidated Group´s audit firm"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "24A_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_6_2" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "24B")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note58_24B")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "24B_Fees concerning audit services incurred by other firms affiliated to"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "24B_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_1_6_3" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "24C")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note59_24C")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "24C_Professional fees related to audit services incurred by other audit firms"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "24C_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
							
							' OTHER LIABILITIES
							Else If paramDashboard = "CCAA_1_7_1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "16B")
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note33_16B")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "16B_Detail of other liabilities"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "16B_Input"
								End If
								
								If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_" + pais, True)
									If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Sections", True) 
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
									Else If BRapi.Security.Authorization.IsUserIngroup(si, userName, "CCAA_Writers", True)
										loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "True"
									End If
								End If	
								
							Else If paramDashboard = "CCAA_3_2_2" Then
								'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode", "16B")
								'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewComponent", "cv_CCAA_Note33_16B")
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "6_1b Return on capital employed HG"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "6_1b Return on capital employed"
								End If
								' This note is automatic, so we fix the Consolidation Member
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_LocalEuro") = "Local"
							Else If paramDashboard = "CCAA_3_1_4" Then
								If pais = "Group"
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "4_Changes in consolidated equity HG"
								Else
									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Cubeview") = "4_Changes in consolidated shareholders equity"
								End If
								' This note is automatic, so we fix the Consolidation Member
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_LocalEuro") = "Local"
								
							' The note is automatic
							Else
								' Fix Consolidation Member to Local
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_LocalEuro") = "Local"
							End If
							
							Dim cubeview As String
							cubeview = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_CCAA_CubeViewCode")
							
							
							#End Region
							
							#Region "Define status labels"
							
							' Obtain the cube view status from the database tables
							
							If Not pais = "Group"
								Dim sql As String = "
									SELECT send_status
									FROM " & tabla & "
									WHERE id = '" & codigo & " - " & cubeview & "';
								"			
								'brapi.ErrorLog.LogMessage(si, "GUARDADO")
								Dim send_statusDT As DataTable
								Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
									send_statusDT = BRAPi.Database.ExecuteSql(dbConnApp, sql, True)
								End Using
								
								Dim send_status As String = send_statusDT.Rows(0)("send_status").ToString()
								'brapi.ErrorLog.LogMessage(si, $"{send_status}")
								
								Dim sql2 As String = "
									SELECT check_status
									FROM " & tabla & "
									WHERE id = '" & codigo & " - " & cubeview & "';
								"			
								'brapi.ErrorLog.LogMessage(si, "GUARDADO")
								Dim check_statusDT As DataTable
								Using dbConnApp2 As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
									check_statusDT = BRAPi.Database.ExecuteSql(dbConnApp2, sql2, True)
								End Using
								
								Dim check_status As String = check_statusDT.Rows(0)("check_status").ToString()
								'brapi.ErrorLog.LogMessage(si, $"{check_status}")
								
								' Set the status label
								
								If check_status = "Checked" Then
									brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "Checked")
									brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_ColorCheck", "Green")
									
								Else If check_status = "Not Started" Then
									If send_status = "Not Started" Then
										brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "")
									Else If send_status = "Submitted" Then
										brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "Ready to check")
										brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_ColorCheck", "Red")
									End If
								End If				
								
	'							If send_status = "Not Started" Then
	'								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "")
	'								'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "")
	'							Else If send_status = "Submitted" Then
	'								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "Ready to check")
	'							End If
	'							If check_status = "Not Started" Then
	'								'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Submitted", "")
	'								'brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "")
	'							Else If check_status = "Checked" Then
	'								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Checked", "Checked")
	'								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_Submitted", "")
	'							End If
							End If
							#End Region
							
							#Region "ScenarioTime"
							
							' When in a Half-Year environment, change the period H1/H2 for M6/M12 in labels
							
							'Dim WorkflowTime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
							'BRApi.ErrorLog.LogMessage(si, $"{WorkflowTime}")
							
							Dim year As String = WorkflowTime.Substring(0,4)
							Dim half_year As String = WorkflowTime.Substring(4,2)
							
							If half_year = "H1" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_LabelScenarioTime_Time", $"{year}M6")
							Else If half_year = "H2" Then
								brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_CCAA_LabelScenarioTime_Time", $"{year}M12")
							End If
							#End Region
							
							#Region "Horse_Group Profile"
							
							' Change visibility of components based on country/Horse Group Workflow Profile
							' Change Consolidation Member based on country/Horse Group Workflow Profile
							
							'' LOCAL/EURO
							
							If pais = "Group"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_LocalEuro") = "Local"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Components") = "False"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_ComboBox") = "False"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "False"
							Else
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_ComboBox") = "True"		
'								If BRApi.Security.Authorization.IsUserInAdminGroup(si)
'									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "True"
'								Else 
'									loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_IsVisible_Check") = "False"
'								End If
							End If
							
							'' HEADERS
							If pais = "Group" 
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_AutomaticContentHeader") = "CCAA_Automatic_Content_Header_Group"
							Else
								'loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_AutomaticContentHeader") = "CCAA_Automatic_Content_Header_Country"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_AutomaticContentHeader") = "CCAA_Automatic_Content_Header_Group"
							End If
							
							
							#End Region
							
							#Region "Origin if Local or Euro"
							
							' Change Adj Origin depending on the Consolidation Member
							
							'Dim conso_member As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_CCAA_LocalEuro")
							Dim conso_member As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_CCAA_LocalEuro")
							'brapi.ErrorLog.LogMessage(si, $"{conso_member}")
							
							If conso_member = "Local"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_AdjOrigin") = "AdjInput"
							Else If conso_member = "EUR"
								loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_AdjOrigin") = "AdjConsolidated"
							End If
							
							#End Region
							

							
							Return loadDashboardTaskResult
							
						End If
						
					#End Region
					
					#Region "Component Selection Changed"
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						#Region "Open Dashboard"
						
						If args.FunctionName.XFEqualsIgnoreCase("OpenDashboard")
							'Get dashboard
							Dim paramDashboard As String = args.NameValuePairs("p_dashboard")
							'Set current dashboard
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
							selectionChangedTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard
							
							'Control dashboard parameters on click
							Me.SetDashboardParametersOnClick(si, args, selectionChangedTaskResult, paramDashboard)
							
							Return selectionChangedTaskResult
							
						#End Region
						
						End If
						
					#End Region
					
					#Region "SQL Table Editor Save Data"
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement SQL Table Editor Save Data logic here.
							'Save the data rows.
							'Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							'Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName)
								'dbConn.BeginTrans()
								'BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, True, False, False)
								'dbConn.CommitTrans()
							'End Using
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.Message = ""
							saveDataTaskResult.CancelDefaultSave = False 'Note: Use True if we already saved the data rows in this Business Rule.
							Return saveDataTaskResult
						End If
						
						#End Region
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Create Status Tables SQL"
		
		Private Sub CreateHorseGroupStatusTable(
			ByVal si As SessionInfo, ByVal tabla As String, ByVal notesList As List(Of Tuple(Of String, String))
		)
			Dim createSql As String = "
			IF OBJECT_ID('" & tabla & "','U') IS NULL
			BEGIN    
			    CREATE TABLE " & tabla & " (
			        id VARCHAR(150) PRIMARY KEY,
			        note_name VARCHAR(150),
			        country VARCHAR(150),
			        last_save_name VARCHAR(150),
			        last_save_date DATETIME,
			        send_status VARCHAR(150),
			        send_name VARCHAR(150),
			        send_date DATETIME,
			        check_status VARCHAR(150),
			        check_name VARCHAR(150),
			        check_date DATETIME
			    )
			
			    "

			Dim entityList As New List(Of Tuple(Of String, String)) From {
			    New Tuple(Of String, String)("Argentina", "R0592"),
			    New Tuple(Of String, String)("Brazil", "R1303"),
			    New Tuple(Of String, String)("Chile", "R0585"),
			    New Tuple(Of String, String)("Holding", "R1300"),
				New Tuple(Of String, String)("Spain", "R1301"),
			    New Tuple(Of String, String)("Portugal", "R0671"),
			    New Tuple(Of String, String)("Romania", "R0611"),
			    New Tuple(Of String, String)("Turkey", "R1302")
			}

			
			Dim insertValues As New StringBuilder()
			
			For Each entityPair In entityList
			    Dim country As String = entityPair.Item1
			    Dim entityCode As String = entityPair.Item2
			    
			    For Each notePair In notesList
			        Dim code As String = notePair.Item1
			        Dim name As String = notePair.Item2
			        
			        insertValues.AppendFormat("    
			        ('" & entityCode & " - " & code & "', '" & code & ": " & name & "', '" & country & "', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00')
			        ,")
			    Next
			Next
			

			If insertValues.Length > 0 Then
			    insertValues.Remove(insertValues.Length - 1, 1) 
			End If
			
			
			createSql &= " 
			    INSERT INTO " & tabla & " (id, note_name, country, last_save_name, last_save_date, send_status, send_name, send_date, check_status, check_name, check_date) VALUES
			    " & insertValues.ToString() & "
			END
			"
			
			'brapi.ErrorLog.LogMessage(si, $"{createSql}")
			

			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				                               
					                BRAPi.Database.ExecuteActionQuery(dbConnApp, createSql, True, True)
								
			End Using

		
			
		End Sub
		
		
		Private Sub CreateCountryStatusTable(
			ByVal si As SessionInfo, ByVal tabla As String, ByVal notesList As List(Of Tuple(Of String, String)), ByVal codigo As String, ByVal pais As String
		)
			Dim createSql As String = "
			IF OBJECT_ID('" & tabla & "','U') IS NULL
			BEGIN	
				CREATE TABLE " & tabla & " (
			            id VARCHAR(150) PRIMARY KEY,
			            note_name VARCHAR(150),
			            country VARCHAR(150),
			            last_save_name VARCHAR(150),
			            last_save_date DATETIME,
			            send_status VARCHAR(150),
			            send_name VARCHAR(150),
			            send_date DATETIME,
			            check_status VARCHAR(150),
			            check_name VARCHAR(150),
			            check_date DATETIME
				)
			"
			
			Dim insertValues As New StringBuilder()
			

			    
		    For Each notePair In notesList
		        Dim code As String = notePair.Item1
		        Dim name As String = notePair.Item2
		        
		        insertValues.AppendFormat("    
		        ('" & codigo & " - " & code & "', '" & code & ": " & name & "', '" & pais & "', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00')
		        ,")
		    Next
			

			If insertValues.Length > 0 Then
			    insertValues.Remove(insertValues.Length - 1, 1) 
			End If
			
			
			createSql &= " 
			    INSERT INTO " & tabla & " (id, note_name, country, last_save_name, last_save_date, send_status, send_name, send_date, check_status, check_name, check_date) VALUES
			    " & insertValues.ToString() & "
			END
			"
			'brapi.ErrorLog.LogMessage(si, $"{createSql}")

			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				                               
					                BRAPi.Database.ExecuteActionQuery(dbConnApp, createSql, True, True)
								
			End Using

		
			
		End Sub
		
		
		
		#End Region
		
		#Region "Helper Functions"
		
		#Region "Set Active Content"
		
		Private Sub SetActiveContent(
			ByVal si As SessionInfo, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Set dynamic content parameter
			'If level is 1, set the initial dashboard, else append to the last level parameter
			If level = 1 Then
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_Content") = $"{WorkspacePrefix}_{activeDashboardNumber}"
			Else
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_Content") = _
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level - 1}_Content") &
					$"_{activeDashboardNumber}"
			End If
		End Sub
		
		#End Region
		
		#Region "Set Active Tab"
		
		Private Sub SetActiveTab(
			ByVal si As SessionInfo, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal quantity As Integer, ByVal activeDashboardNumber As Integer
		)
			'Get colors for each level
			Dim level1Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab1_Active")
			Dim level2Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab2_Active")
			Dim level3Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab3_Active")
			
			'Set colors for each level
			'Color tuple definition: (ActiveColor, InactiveColor)
			Dim colorDictionary As New Dictionary(Of Integer, Tuple(Of String, String)) From {
				{1, New Tuple(Of String, String)(level1Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBarBG"))},
				{2, New Tuple(Of String, String)(level2Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBarBG"))},
				{3, New Tuple(Of String, String)(level3Active, "White")},
				{4, New Tuple(Of String, String)(level3Active, "White")},
				{5, New Tuple(Of String, String)(level3Active, "White")}
			}
			
			'Loop through all the repeater components
			For i As Integer = 1 To quantity
				'Control left tabs
				If level = 1 Then 
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_{WorkspacePrefix}_{i}_LeftTabs") = _
					If(i = activeDashboardNumber, $"{WorkspacePrefix}_{i}_LeftTabs", $"{WorkspacePrefix}_Transparent")
				End If
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_ActiveColor_{i}") = _
					If(i = activeDashboardNumber, colorDictionary(level).Item1, colorDictionary(level).Item2)
			Next
		End Sub
		
		#End Region
		
		#Region "Set Dashboards Parameters On Load"
		
		Private Sub SetDashboardParametersOnLoad(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Control dashboard parameters
			'EXAMPLE
'			If level = 1 Then
'				If activeDashboardNumber = 3 Then
'					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Scenario_Main") = "Planning"
'					If args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_CCAA_Scenario") = "Actual" Then _
'						loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Scenario") = "Budget"
'				Else
'					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Scenario_Main") = "Actual"
'					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CCAA_Scenario") = "Actual"
'				End If
'			End If
			Dim wftime As String = BRApi.Finance.Time.GetNameFromId(si, si.WorkflowClusterPk.TimeKey)
			
			loadDashboardTaskResult.ModifiedCustomSubstVars("param_POV_Time") = wftime
			
		End Sub
		
		#End Region
		
		#Region "Set Dashboards Parameters On Click"
		
		Private Sub SetDashboardParametersOnClick(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef selectionChangedTaskResult As XFSelectionChangedTaskResult,
			ByVal paramDashboard As String
		)
			'Control dashboard parameters
			'EXAMPLE
'			If paramDashboard = "CCAA_1_1" Then _
'				selectionChangedTaskResult.ModifiedCustomSubstVars("prm_CCAA_Request_Id") = ""
			
		End Sub
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
