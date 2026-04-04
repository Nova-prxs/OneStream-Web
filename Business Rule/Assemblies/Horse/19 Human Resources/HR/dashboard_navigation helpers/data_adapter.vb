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
Imports Workspace.__WsNamespacePrefix.__WsAssemblyName.shared_queries

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.data_adapter
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try

				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						' Dim allSQL As New Workspace.__WsNamespacePrefix.__WsAssemblyName.shared_queries
						Dim allSQL As New shared_queries
						
						' VARIABLES
						Dim dt As DataTable = Nothing
						Dim sql As String = String.Empty
						
						' PARAMETROS
						' 0 - Main Queries
						Dim sTestParam As String = args.NameValuePairs.GetValueOrDefault("testParam", "NO FUNCIONÓ")						
						Dim sYear As String = args.NameValuePairs.GetValueOrDefault("year", "2025")
						Dim sScenario As String = args.NameValuePairs.GetValueOrDefault("scenario", "Actual")
						Dim sScenarioRef As String = args.NameValuePairs.GetValueOrEmpty("scenarioRef")
						Dim sMonth As String = args.NameValuePairs.GetValueOrEmpty("month")						
						Dim sMonthRef As String = args.NameValuePairs.GetValueOrEmpty("monthRef")		
						' 1 - Data Adapter
						Dim sEntity As String = args.NameValuePairs.GetValueOrEmpty("entity")
						Dim sCurrency As String = args.NameValuePairs.GetValueOrEmpty("currency")
						Dim sView As String = args.NameValuePairs.GetValueOrEmpty("view")
						Dim sAccount As String = args.NameValuePairs.GetValueOrEmpty("account")
						
						Dim sColumnsGroupBy As String = args.NameValuePairs.GetValueOrEmpty("detail")
						
						Dim sFunctionGroup As String = args.NameValuePairs.GetValueOrEmpty("functionGroup")
						
						If args.DataSetName.XFEqualsIgnoreCase("Test") Then
							
							sql = SharedQuery(si, "Test", "Month", "Year", "Scenario", "ScenarioRef", sTestParam)
							dt = ExecuteSQL(si, sql)
						
						#Region "Aux"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("ALL_TR") Then
							
							sql = SharedQuery(si, "ALL_TR")
							sql = $"							
								{sql}
								
								SELECT *
								FROM PIVOT_ALL_TR
							"
							
						Else If args.DataSetName.XFEqualsIgnoreCase("RAW_HC") Then
							
							sql = SharedQuery(si, "RAW_HC", sMonth, sYear) ' sMonth ya no filtra en shared
							sql = $"							
								{sql}
								
								SELECT *
								FROM RAW_HC
								WHERE 1=1
									AND month = {sMonth}
							"
						Else If args.DataSetName.XFEqualsIgnoreCase("FTE_2") Then
							
							sql = SharedQuery(si, "FTE",, sYear, sScenario,)
							sql = $"							
								{sql}
								
								SELECT *							
							
								FROM FTE_2
								WHERE 1=1
									-- AND (FTE_1<>0 OR FTE_2<>0)
									-- AND (FTE_2<>0)
									-- AND Factory='Portugal'
							"
							
						Else If args.DataSetName.XFEqualsIgnoreCase("FTE_RAW") Then
							sql = SharedQuery(si, "FTE_RAW",, sYear, sScenario,)
							sql = $"							
								{sql}
								
								SELECT *
								FROM FTE_RAW
								WHERE 1=1
									-- AND Factory='Portugal'
									AND month = {sMonth}
							"
'							bRAPI.ErrorLog.LogMessage(si, "RAW FTE: "&sql)
							
						#End Region
							
						#Region "Scenario Comparison"	
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenario_Comparison") Then
							
							sql = SharedQuery(si, "Scenario_Comparison", sMonth, sYear, sScenario, sScenarioRef)
							sql = $"							
								{sql}
								
								SELECT MF.function_group, MF.function_lvl_one, SC.* 
								FROM Scenario_Comparison SC
								LEFT JOIN XFC_HR_MASTER_Function MF
									ON MF.id = SC.CCGroup
								WHERE 1=1
									AND MF.function_group='{sFunctionGroup}'
									AND SC.company_id = '{sEntity}'
									
							"
							
						#End Region
						
						#Region "Scenario Comparison - Factory"	
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenario_Comparison_Factory") Then
														
							sql = SharedQuery(si, "Scenario_Comparison", sMonth, sYear, sScenario, sScenarioRef)
							sql = $"							
								{sql}
								
								SELECT company_desc as Company, company_id, SUM(Actual_Headcount) AS Actual, SUM(Ref_Headcount) AS {sScenarioRef}, SUM(Actual_Headcount)-SUM(Ref_Headcount) AS Diff
								FROM Scenario_Comparison								
								GROUP BY company_desc, company_id
							"
						
						#End Region
						
						#Region "Scenario Comparison - Function Group"	
													
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenario_Comparison_Function_Group") Then
														
							sql = SharedQuery(si, "Scenario_Comparison", sMonth, sYear, sScenario, sScenarioRef)
							sql = $"							
								{sql}
								
								SELECT function_group as [Function Group], SUM(Actual_Headcount) AS Actual, SUM(Ref_Headcount) AS {sScenarioRef}, SUM(Actual_Headcount)-SUM(Ref_Headcount) AS Diff
								FROM Scenario_Comparison SC
								LEFT JOIN XFC_HR_MASTER_Function MF
									ON MF.id = SC.CCGroup							
								GROUP BY function_group
							"
						
						#End Region	
						
						#Region "Scenario Comparison - Payroll Analysis"	
												
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenario_Comparison_PayrollAnalysis") Then
								
'							BRApi.ErrorLog.LogMessage(si, "1 --> " & DateTime.Now.ToString("HH:mm:ss"))
							
							Dim sNumMonths = If (sView = "Periodic", 1, sMonth)
							
							Dim sql_data As String = SharedQuery(si, "Payroll_Analysis", sMonth, sYear, sScenario, sScenarioRef, sEntity, sCurrency, sView, sAccount, sMonthRef)
							
							sql = $"
							
								TRUNCATE TABLE XFC_MAIN_AUX_PayrollAnalysis;
							
								{sql_data}															
							
								INSERT INTO XFC_MAIN_AUX_PayrollAnalysis
									(costcenter, id_costcenter, CC_Type_Id, CC_Type, CC_Group1, CC_Group2, Cost, Cost_Ref, Cost_Per, Cost_Per_Ref, FTE, FTE_Ref)
								SELECT 
									costcenter
									, id_costcenter
									, CC_Type_Id
									, CC_Type
									, CC_Group1
									, CC_Group2
									, Cost
									, Cost_Ref 
									, Cost_Per
									, Cost_Per_Ref 					
									, FTE
									, FTE_Ref 
								FROM All_Data;
							
								WITH query_final AS (
							
									SELECT 
										  SUM(Cost) AS [Cost] 
										, SUM(Cost_Ref) AS [Cost Ref] 
	 
										, SUM(Cost_Per) / SUM(NULLIF(FTE,0)) AS [Unit. Cost] 
										, SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0)) AS [Unit. Cost Ref]
								
										, SUM(Cost) / (SUM(Cost_Per) / SUM(NULLIF(FTE,0))) / {sNumMonths} AS [FTE] 
										, SUM(Cost_Ref) / (SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0))) / {sNumMonths} AS [FTE Ref] 																													
									
									--FROM All_Data							
									FROM XFC_MAIN_AUX_PayrollAnalysis
								)							
							
								SELECT 'Cost Ref:' AS indicator, SUM([Cost Ref]) AS Value 
								FROM query_final
								
								UNION ALL
							
								SELECT 'FTE Impact:' AS indicator, ([FTE] - [FTE Ref]) * ([Cost Ref] / [FTE Ref]) AS Value 
								FROM query_final
								
								UNION ALL	
							
								SELECT 'Price Impact:' AS indicator, (([Cost] / [FTE]) - ([Cost Ref] / [FTE Ref])) * [FTE] AS Value 
								FROM query_final
								
								UNION ALL								
								
								SELECT 'Cost:' AS indicator, SUM([Cost] ) AS Value 
								FROM query_final


							"
							BRApi.ErrorLog.LogMessage(si, sql)
						
						#End Region							
																
						#Region "Scenario Comparison - Payroll Analysis Graph"	
												
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenario_Comparison_PayrollAnalysis_Graph") Then
							
'							BRApi.ErrorLog.LogMessage(si, "2 --> " & DateTime.Now.ToString("HH:mm:ss"))
							
							Dim sNumMonths = If (sView = "Periodic", 1, sMonth)
							
							Dim sql_data As String '= SharedQuery(si, "Payroll_Analysis", sMonth, sYear, sScenario, sScenarioRef, sEntity, sCurrency, sView, sAccount, )														
							
							sql = $"
							
								{sql_data}
											
								WITH query_final AS (
							
									SELECT 
										  SUM(Cost) AS [Cost] 
										, SUM(Cost_Ref) AS [Cost Ref] 
	 
										, SUM(Cost_Per) / SUM(NULLIF(FTE,0)) AS [Unit. Cost] 
										, SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0)) AS [Unit. Cost Ref]
								
										, SUM(Cost) / (SUM(Cost_Per) / SUM(NULLIF(FTE,0))) / {sNumMonths} AS [FTE] 
										, SUM(Cost_Ref) / (SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0))) / {sNumMonths} AS [FTE Ref] 
							
									--FROM All_Data
									FROM XFC_MAIN_AUX_PayrollAnalysis
								)
							
								SELECT 
									[Cost Ref] AS [Cost Ref] 
							
									, ([FTE] - [FTE Ref]) * ([Cost Ref] / [FTE Ref]) AS [FTE Impact] 
									, (([Cost] / [FTE]) - ([Cost Ref] / [FTE Ref])) * [FTE] AS [Price Impact]
									, [Cost]
										- [Cost Ref]
										- ISNULL( ([FTE] - [FTE Ref]) * ([Cost Ref] / [FTE Ref]) ,0)
										- ISNULL( (([Cost] / [FTE]) - ([Cost Ref] / [FTE Ref])) * [FTE] ,0) AS GAP
							
									, [Cost] AS [Cost]
							
								FROM query_final
							"							
									
							dt = ExecuteSQL(si, sql)
							
							' return dt
							Dim seriesCollection As New XFSeriesCollection()
							Dim series As New XFSeries()
							
							series.UniqueName = "Payroll Analisys"
							series.Type = XFChart2SeriesType.Waterfall
							series.BarWidth = 0.95					
	
							For Each row As DataRow In dt.Rows
								For Each column As DataColumn In dt.Columns
									Dim point As New XFSeriesPoint()
									If (IsDBNull(row(column.ColumnName)) OrElse row(column.ColumnName) = 0) Then
										If column.ColumnName = "Cost Ref" Then
											'brapi.ErrorLog.LogMessage(si, "Bien")
											point.Argument = column.ColumnName										
											point.Value = 0														
											series.AddPoint(point)
											
										End If
										
									Else 
										
										point.Argument = column.ColumnName
										point.Value = row(column.ColumnName)
										series.AddPoint(point)
										
									End If
								Next
							Next
							seriesCollection.AddSeries(series)
							
							Return seriesCollection.CreateDataSet(si)							
							
'							BRApi.ErrorLog.LogMessage(si, sql)
						
						#End Region		

						#Region "Scenario Comparison - Payroll Analysis Detail"	
												
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenario_Comparison_PayrollAnalysis_Detail") Then
							
'							BRApi.ErrorLog.LogMessage(si, "3 --> " & DateTime.Now.ToString("HH:mm:ss"))
							
							Dim sColumnsSelect As String = sColumnsGroupBy.Replace("entity","M.name + ' - ' + ISNULL(M.description,'') AS Entity").Replace("costcenter","costcenter AS [Cost Center]").Replace("CCGroup_Desc","CCGroup_Desc AS [CC Group]").Replace("CC_Type","CC_Type AS [CC Type]").Replace("CC_Group1","CC_Group1 AS [CC Group 1]").Replace("CC_Group2","CC_Group2 AS [CC Group 2]")						
							Dim sColumnsSelectTotal As String = "'TOTAL:'" & sColumnsGroupBy.Replace("entity","'' AS [Entity]").Replace("costcenter","'' AS [Cost Center]").Replace("CCGroup_Desc","'' AS [CC Group]").Replace("CC_Type","'' AS [CC Type]").Replace("CC_Group1","'' AS [CC Group 1]").Replace("CC_Group2","'' AS [CC Group 2]").Substring(2)													
							sColumnsGroupBy = sColumnsGroupBy.Replace("entity","M.name, M.description")
							
'							BRAPI.ErrorLog.LogMessage(si, "sColumnsGroupBy: " & sColumnsGroupBy)
'							BRAPI.ErrorLog.LogMessage(si, "sColumnsSelect: " & sColumnsSelect)
'							BRAPI.ErrorLog.LogMessage(si, "sColumnsSelectTotal: " & sColumnsSelectTotal)
							
							Dim sNumMonths = If (sView = "Periodic", 1, sMonth)
																					
							Dim sql_data As String '= SharedQuery(si, "Payroll_Analysis", sMonth, sYear, sScenario, sScenarioRef, sEntity, sCurrency, sView, sAccount,)
							
							Dim sql_TR As String = SharedQuery(si, "ALL_TR")'.Replace("WITH ", ", ")
							
							sql = $"
							
								{sql_data}
							
								{sql_TR}
											
								, query_final AS (
							
									SELECT 
										{sColumnsSelectTotal}
							
										, SUM(Cost) AS [Cost] 
										, SUM(Cost_Ref) AS [Cost Ref] 	 
								
										, SUM(Cost) / (SUM(Cost_Per) / SUM(NULLIF(FTE,0))) / {sNumMonths} AS [FTE] 
										, SUM(Cost_Ref) / (SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0))) / {sNumMonths} AS [FTE Ref] 
							
										, (SUM(Cost) / (SUM(Cost_Per) / SUM(NULLIF(FTE,0))) / {sNumMonths})
										 	- (SUM(Cost_Ref) / (SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0))) / {sNumMonths}) AS [FTE Diff] 
							
										, SUM(Cost_Per) / SUM(NULLIF(FTE,0)) AS [Unit. Cost] 
										, SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0)) AS [Unit. Cost Ref]							
																			
										, 1 AS orden					
									
									--FROM All_Data
									FROM XFC_MAIN_AUX_PayrollAnalysis
								
									UNION ALL
								
									SELECT 
										{sColumnsSelect}
								
										, SUM(Cost) AS [Cost] 
										, SUM(Cost_Ref) AS [Cost Ref] 	 
								
										, SUM(Cost) / (SUM(Cost_Per) / SUM(NULLIF(FTE,0))) / {sNumMonths} AS [FTE] 
										, SUM(Cost_Ref) / (SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0))) / {sNumMonths} AS [FTE Ref] 
							
										, (SUM(Cost) / (SUM(Cost_Per) / SUM(NULLIF(FTE,0))) / {sNumMonths})
										 	- (SUM(Cost_Ref) / (SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0))) / {sNumMonths}) AS [FTE Diff] 
							
										, SUM(Cost_Per) / SUM(NULLIF(FTE,0)) AS [Unit. Cost] 
										, SUM(Cost_Per_Ref) / SUM(NULLIF(FTE_Ref,0)) AS [Unit. Cost Ref]							
								
										, 2 AS orden
								
									--FROM All_Data F
									FROM XFC_MAIN_AUX_PayrollAnalysis F
	
									LEFT JOIN PIVOT_ALL_TR TR
										ON TR.id_costcenter = F.id_costcenter
							
									LEFT JOIN XFC_INV_MASTER_Cost_Center C
										ON F.id_costcenter = C.id
								
									LEFT JOIN Member M
										ON C.entity_member = M.name		
										AND M.DimTypeId = 0							
								
									GROUP BY {sColumnsGroupBy}
							
								)
							
								SELECT *
									, ([FTE] - [FTE Ref]) * ([Cost Ref] / [FTE Ref]) AS [FTE Impact] 
									, (([Cost] / [FTE]) - ([Cost Ref] / [FTE Ref])) * [FTE] AS [Price Impact]
														
								FROM query_final
							"
							
							BRApi.ErrorLog.LogMessage(si, sql)
						
						#End Region									
						
						#Region "Headcount"	
												
						Else If args.DataSetName.XFEqualsIgnoreCase("Headcount") Then
																					
							sql = $"
																
								SELECT
									year
									, 'M' + RIGHT('0' + CONVERT(VARCHAR,month),2) AS Month
									, CONVERT(INT,company_id) AS [Company Id]
									, company_desc AS [Company]
							
									, M.name + ' - ' + ISNULL(M.description,'') AS Entity
							
									--, employee_id AS [Employee Id]
									--, employee_name AS [Employee]
									, costcenter_id AS [Cost Center Id]
									, costcenter_id + ' - ' + ISNULL(MC.description,'No description') AS [Cost Center]
									
									, ISNULL(T.id,'') AS [Cost Center Type Id]
									, T.description AS [Cost Center Type]
									, T.group1 AS [Cost Center Group 1]
									, T.group2 AS [Cost Center Group 2]
									
									, job_category AS [Job Category]
									, location AS Location
									, location_country AS [Location Country]
									, gender AS [Gender]
									, HR_cluster AS [HR Cluster]
									, shared_services_center AS [Shared Services Center]
									, global_function_organization AS [Global Function Organization]
									, sup_org_from_top_1 AS [Sup org from top level 1]
									, sup_org_from_top_2 AS [Sup org from top level 2]
									, sup_org_from_top_3 AS [Sup org from top level 3]
									, sup_org_from_top_4 AS [Sup org from top level 4]
									, sup_org_from_top_5 AS [Sup org from top level 5]
									, sup_org_from_top_6 AS [Sup org from top level 6]
									, SUM(Headcount) AS Headcount
							
								FROM XFC_MAIN_FACT_Headcount H
							
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist MC
									ON MC.id = H.costcenter_id
						 			AND CONVERT(VARCHAR(50), H.year) + '-' + CONVERT(VARCHAR(50), H.month) + '-01' BETWEEN start_date AND end_date
												
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Types T
									ON MC.type = T.id			
							
								LEFT JOIN XFC_INV_MASTER_Cost_Center C
									ON H.costcenter_id = C.id
								
								LEFT JOIN Member M
									ON C.entity_member = M.name			
									AND M.DimTypeId = 0
							
								WHERE H.scenario = '{sScenario}'
							
								GROUP BY
									year
									, 'M' + RIGHT('0' + CONVERT(VARCHAR,month),2)
									, company_id
									, company_desc 
									, M.name
									, M.description
									--, employee_id
									--, employee_name
									, costcenter_id
									, costcenter_id + ' - ' + ISNULL(MC.description,'No description')
									, ISNULL(T.id,'')
									, T.description
									, T.group1
									, T.group2							
									, job_category
									, location
									, location_country
									, gender
									, HR_cluster
									, shared_services_center
									, global_function_organization
									, sup_org_from_top_1
									, sup_org_from_top_2
									, sup_org_from_top_3
									, sup_org_from_top_4
									, sup_org_from_top_5
									, sup_org_from_top_6							

							"
							
							BRApi.ErrorLog.LogMessage(si, sql)
						
						#End Region				
						
						#Region "Headcount Employee"	
												
						Else If args.DataSetName.XFEqualsIgnoreCase("Headcount_Employee") Then
																					
							sql = $"
																
								SELECT
									year
									, 'M' + RIGHT('0' + CONVERT(VARCHAR,month),2) AS Month
									, company_id AS [Company Id]
									, company_desc AS [Company]
									, employee_id AS [Employee Id]
									, employee_name AS [Employee]
									, costcenter_id AS [Cost Center Id]
									, costcenter_id + ' - ' + ISNULL(MC.description,'No description') AS [Cost Center]
									, ISNULL(T.id,'') AS [Cost Center Type Id]
									, T.description AS [Cost Center Type]
									, T.group1 AS [Cost Center Group 1]
									, T.group2 AS [Cost Center Group 2]							
									, job_category AS [Job Category]
									, location AS Location
									, location_country AS [Location Country]
									, gender AS [Gender]
									, HR_cluster AS [HR Cluster]
									, shared_services_center AS [Shared Services Center]
									, global_function_organization AS [Global Function Organization]
									, sup_org_from_top_1 AS [Sup org from top level 1]
									, sup_org_from_top_2 AS [Sup org from top level 2]
									, sup_org_from_top_3 AS [Sup org from top level 3]
									, sup_org_from_top_4 AS [Sup org from top level 4]
									, sup_org_from_top_5 AS [Sup org from top level 5]
									, sup_org_from_top_6 AS [Sup org from top level 6]
									, Headcount
							
								FROM XFC_MAIN_FACT_Headcount H
							
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist MC
									ON MC.id = H.costcenter_id
						 			AND CONVERT(VARCHAR(50), H.year) + '-' + CONVERT(VARCHAR(50), H.month) + '-01' BETWEEN start_date AND end_date
							
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Types T
									ON MC.type = T.id									
												
								WHERE H.scenario = '{sScenario}'												

							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)
						
						#End Region	
						
						#Region "FTE"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("FTE") Then
							
							Dim queries_PCT As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql_FTE As String = queries_PCT.AllQueries(si, "FTE","","", sYear, sScenario, "", "", "",,,"")
					  				  
							sql = $"				  				  			
					
								{sql_FTE}
							
								SELECT 
									M.name + ' - ' + ISNULL(M.description,'') AS Entity
									, F.month AS Month
									, CASE WHEN F.wf_type = 1 THEN 'Directo' ELSE 'Indirecto' END AS [WF Type]
									, F.id_costcenter AS [Cost Center Id]
									, F.id_costcenter + ' - ' + ISNULL(MC.description,'No description') AS [Cost Center]
									, ISNULL(T.id,'') AS [Cost Center Type Id]
									, T.description AS [Cost Center Type]
									, T.group1 AS [Cost Center Group 1]
									, T.group2 AS [Cost Center Group 2]								
									, F.description1 AS [Hours Indicator]
									, F.shift AS Shift
									, F.hours AS [Hours]									
									, F.worked_days AS [Days by Shift]
									, F.hours_shift AS [Hours by Shift]
									, F.FTE
						
								FROM FTE_data F																

								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist MC
									ON MC.id = F.id_costcenter
						 			AND '{sYear}-' + CONVERT(VARCHAR(50), REPLACE(F.month,'M','')) + '-01' BETWEEN start_date AND end_date
							
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Types T
									ON MC.type = T.id		
							
								LEFT JOIN XFC_INV_MASTER_Cost_Center C
									ON F.id_costcenter = C.id
								
								LEFT JOIN Member M
									ON C.entity_member = M.name		
									AND M.DimTypeId = 0
												
								"
						
						#End Region
						
						#Region "HC vs FTE"	
						
						Else If args.DataSetName.XFEqualsIgnoreCase("HC_FTE_Comparison") Then
							
							sql = SharedQuery(si, "HC_FTE_Comparison",, sYear, sScenario,)
							sql = $"							
								{sql}
								
								, HC_FTE_Comparison_JC as (
									SELECT 
										  month AS Month
										, company_id AS [Company Id]
										, company_desc AS [Company]
										, costcenter_id AS [CC Id]
										, cc_description AS [CC]							
										, CCGroup [CC Group]							
										, type AS [CC Type]
										, nature AS [CC Nature]
										, job_category AS [Job Category]
										, HCType AS [HC Type]
										, CostType AS [Cost Type]
										, headcount AS HeadCounts 
										, HoursIndicator AS [Hours Indicator]
										, CASE WHEN job_category = 'Direct'		THEN FTE_1 ELSE 0 END AS [FTE Direct]
										, CASE WHEN job_category = 'Indirect' 	THEN FTE_2 ELSE 0 END AS [FTE Indirect]
										
								FROM HC_FTE_Comparison
								WHERE 1=1	
									AND month = {sMonth}
								)
							
								SELECT 
									  *
									, [FTE Direct] + [FTE Indirect] AS FTE
									, [FTE Direct] - CASE WHEN [Job Category] = 'Direct' 	THEN HeadCounts ELSE 0 END AS [Diff Direct]
									, [FTE Indirect] - CASE WHEN [Job Category] = 'Indirect' 	THEN HeadCounts ELSE 0 END AS [Diff Indirect]
									, [FTE Direct] + [FTE Indirect] - HeadCounts AS Diff
								FROM HC_FTE_Comparison_JC							

							"
							
							BRApi.ErrorLog.LogMessage(si, sql)

						Else If args.DataSetName.XFEqualsIgnoreCase("HC_FTE_Comparison_Aggregated") Then
							
							sql = SharedQuery(si, "HC_FTE_Comparison", sMonth, sYear, sScenario, sScenarioRef)
							sql = $"							
								{sql}
								
								SELECT   
									  month
									, company_id
									, company_desc
									, type
									, nature
									, HCType
									, CostType
									, CCGroup
									, costcenter_id
									, headcount as HeadCounts 
									, FTE_1
									, FTE_2
								FROM HC_FTE_Comparison
								WHERE 1=1									
							"
							
						#End Region
						
						#Region "Historical Data"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Historical_Data") Then
							
							sql = SharedQuery(si, "Historical_Data", sMonth, sYear, sScenario, sScenarioRef)
							sql = $"
								{sql}
							
								SELECT scenario, year, id_entity, UD2_Labour_Cost_Type, UD3_CostCenter_Group
								FROM Historical_Data
								WHERE id_costcenter IS NULL
							"
							
						#End Region
							
						End If
						
						dt = ExecuteSQL(si, sql)
						Return dt
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		Private Function ExecuteSQL(ByVal si As SessionInfo, ByVal sqlCmd As String)        
			Dim dt As DataTable = Nothing
	        Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)                           
	            dt = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)			
	        End Using   
			Return dt	
	    End Function
		
		#End Region
		
		#Region "IntelliSense"
		''' <summary>
		''' Ejecuta la función query() de shared_queries con IntelliSense.
		''' </summary>
		''' <param name="si">Información de sesión actual.</param>
		''' <param name="queryName">Nombre de la query.</param>
		''' <param name="sMonth">Mes (opcional).</param>
		''' <param name="sYear">Año (opcional).</param>
		''' <param name="sScenario">Escenario (opcional).</param>
		''' <param name="sScenarioRef">Escenario de referencia (opcional).</param>		
		''' <param name="sEntity">Entity (opcional).</param>
		''' <param name="sCurrency">Currency (opcional).</param>
		''' <param name="sView">View (opcional).</param>''' 
		''' <param name="sAccount">Account (opcional).</param>''' 
		''' <param name="sMonthRef">Mes de referencia (opcional).</param>'''		
		''' <param name="sTestParam">Parámetro de prueba (opcional).</param>''' 
		''' <returns>SQL With con el nombre del queryName.</returns>
		Public Function SharedQuery(
		    ByVal si As SessionInfo,
		    ByVal queryName As String,
		    Optional sMonth As String = Nothing,
		    Optional sYear As String = Nothing,
		    Optional sScenario As String = Nothing,
		    Optional sScenarioRef As String = Nothing,
			Optional sEntity As String = Nothing,
			Optional sCurrency As String = Nothing,	
			Optional sView As String = Nothing,	
			Optional sAccount As String = Nothing,	
		    Optional sMonthRef As String = Nothing,			
		    Optional sTestParam As String = Nothing			
		) As String
		
		    Dim sq As New Workspace.__WsNamespacePrefix.__WsAssemblyName.shared_queries
		    Return sq.query(si, queryName, sMonth, sYear, sScenario, sScenarioRef, sEntity, sCurrency, sView, sAccount, sMonthRef, sTestParam)
		
		End Function
		
		#End Region
		
	End Class
End Namespace
