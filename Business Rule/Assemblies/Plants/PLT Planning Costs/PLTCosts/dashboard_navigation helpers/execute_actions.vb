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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.execute_actions
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			
			Try
				
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement Load Dashboard logic here.

						End If
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
											
						Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries						
						Dim shared_functions As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
						Dim sql As String = String.Empty	
						Dim sqlDelete As String = String.Empty	

					
					#Region "Initialization Costs"	
					
						If args.FunctionName.XFEqualsIgnoreCase("PlanInitCosts") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")	
							
							Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)													
							Dim sYearInit As String = If (sScenario.StartsWith("RF"), sYear, (CInt(sYear)-1).ToString)	
							
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql_ActivityIndex = uti_sharedqueries.AllQueries(si, "ActivityALL", sFactory, sMonthFirst, sYear, sScenario, sScenarioRef, "All")								
							
							sql = sql_ActivityIndex  & $"		
				
							, ActivityIndex AS (
								SELECT 
									MONTH(A.date) AS month, [Id GM] AS id_averagegroup, Time
									,ISNULL(SUM(X.value), 
										CONVERT(DECIMAL(18,2),SUM([Activity TSO])) 
										/ CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) * 100) AS activity_index									
							
								FROM (	SELECT [Id Factory], date, [Id GM], time, SUM([Activity TSO]) AS [Activity TSO], SUM([Activity TAJ Ref]) AS [Activity TAJ Ref] 
										FROM ActivityAll 
										WHERE time = 'UO1'
										GROUP BY [Id Factory], date, [Id GM], time
							
										UNION ALL
							
										SELECT [Id Factory], date, 'Global' AS [Id GM], time, SUM([Activity TSO]) AS [Activity TSO], SUM([Activity TAJ Ref]) AS [Activity TAJ Ref] 
										FROM ActivityAll 
										WHERE time = 'UO1'
										GROUP BY [Id Factory], date, time) A								
							
								LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
									ON X.scenario = '{sScenario}'
									AND X.scenario_ref = '{sScenarioRef}'
									AND A.[Id GM] = X.id_averagegroup
									AND A.date = X.date
									AND A.[Id Factory] = X.id_factory
									AND A.Time = X.uotype
							
				             	WHERE YEAR(A.date) = {sYear} 
							 	AND [Id Factory] = '{sFactory}'	
							
								GROUP BY MONTH(A.date), [Id GM], Time
							)																									
							
							, FactData AS (	

								SELECT 
									F.id_factory
									, CASE WHEN '{sYear}' <> '{sYearInit}' THEN DATEADD(YEAR, 1,F.date) ELSE F.date END AS date
									, F.id_costcenter						
									, F.id_averagegroup	
									, F.id_account
									, SUM(F.value) AS value_ref
									, (CASE WHEN C.criteria = 'Activity Index GM' THEN ISNULL(AI.activity_index,0)
										   WHEN C.criteria = 'Activity Index Global' THEN ISNULL(AIG.activity_index,0)
										   WHEN C.Criteria = 'No Criteria' THEN 100 END)
									  
										* (CASE WHEN C.inflation = 1 THEN 1 + (CPI.Value/100) ELSE 1 END)
							
										/ 100 AS activity_index
									, V.value AS variable
							
								FROM XFC_PLT_FACT_CostsDistribution F
							
								LEFT JOIN XFC_PLT_AUX_Costs_Init_GM_Criteria C
									ON C.scenario = '{sScenario}'
									AND C.year = {sYear} 
									AND F.id_factory = C.id_factory
									AND F.id_account = C.id_account
							
								LEFT JOIN XFC_PLT_AUX_CPI CPI
									ON CPI.scenario = '{sScenario}'
									AND CPI.id_factory = '{sFactory}'
									AND CPI.year = {sYear}
									AND CPI.id_indicator = 'CPI'							
							
							   	LEFT JOIN ActivityIndex AI
									ON F.id_averagegroup = AI.id_averagegroup 		
									AND MONTH(F.date) = AI.month
							
							   	LEFT JOIN ActivityIndex AIG
									ON AIG.id_averagegroup = 'Global' 		
									AND MONTH(F.date) = AIG.month					
							
								LEFT JOIN XFC_PLT_AUX_FixedVariableCosts V
									ON F.date = V.date
									AND F.id_factory = V.id_factory
									AND F.id_account = V.id_account
									AND V.scenario = '{sScenarioRef}'
							
								LEFT JOIN XFC_PLT_MASTER_Account A
									ON F.id_account = A.id

								WHERE 1=1
									AND F.scenario = '{sScenarioRef}'
									AND F.id_factory = '{sFactory}'
									AND YEAR(F.date) = {sYearInit}
									AND MONTH(F.date) >= {sMonthFirst}
							
								GROUP BY 
									F.id_factory
									, F.date
									, F.id_costcenter						
									, F.id_averagegroup	
									, F.id_account			
									, A.account_type
									, AI.activity_index
									, AIG.activity_index
									, C.criteria
									, V.value
									, C.inflation
									, CPI.Value
								)							
							
								-- 1. Insert AUX by GM							
							
								INSERT INTO XFC_PLT_AUX_Costs_Init_GM
								(
									scenario
									, scenario_ref
									, id_factory
									, date
									, id_costcenter
									, id_averagegroup
									, id_account
									, value_ref
									, activity_index
									, value_100_adjusted
									, fixed_cost_absorption
									, value_semi_adjusted
									, value_semi_adjusted_f
									, value_semi_adjusted_v
								)
							
								SELECT
									'{sScenario}' AS scenario
									, '{sScenarioRef}' AS scenario_ref
									, F.id_factory
									, F.date
									, F.id_costcenter						
									, F.id_averagegroup	
									, F.id_account
							
									, F.value_ref	
							
									, F.activity_index * 100 AS activity_index
							
									, F.value_ref * F.activity_index AS value_100_adjusted
							
									, (F.value_ref * F.activity_index) 
									- ((F.value_ref * (100 - ISNULL(F.variable,0)) / 100)	 				
									+ (F.value_ref * (ISNULL(F.variable,0) / 100) * F.activity_index)) AS fixed_cost_absorption
							
									, (F.value_ref * (100 - ISNULL(F.variable,0)) / 100)	 				
									+ (F.value_ref * (ISNULL(F.variable,0) / 100) * F.activity_index) AS value_semi_adjusted
							
									, (F.value_ref * (100 - ISNULL(F.variable,0))/ 100) AS value_semi_adjusted_f
							
									, (F.value_ref * (ISNULL(F.variable,0) / 100) * F.activity_index) AS value_semi_adjusted_v
							
								FROM FactData F
							
								
								-- 2. Insert FACT
							
								INSERT INTO XFC_PLT_FACT_Costs
								(
									scenario
									, id_factory
									, date
									, id_costcenter
									, id_account
									, value
									, currency
								)
								
								-- Closed Months
							
									SELECT
										'{sScenario}' + '_Init' AS scenario
										, id_factory
										, date		
										, id_costcenter
										, id_account
										, SUM(value) AS value
										, 'Local' AS currency
								
									FROM XFC_PLT_FACT_Costs
									
									WHERE scenario = 'Actual'
									AND id_factory = '{sFactory}'
									AND YEAR(date) = {sYear}
								    AND MONTH(date) < {sMonthFirst}
									AND id_account <> 'Others'
									
									GROUP BY
										id_factory
										, date		
										, id_costcenter
										, id_account
							
									UNION ALL 
							
									SELECT
										'{sScenario}' AS scenario
										, id_factory
										, date		
										, id_costcenter
										, id_account
										, SUM(value) AS value
										, 'Local' AS currency
								
									FROM XFC_PLT_FACT_Costs
									
									WHERE scenario = 'Actual'
									AND id_factory = '{sFactory}'
									AND YEAR(date) = {sYear}
								    AND MONTH(date) < {sMonthFirst}	
									AND id_account <> 'Others'
							
									GROUP BY
										id_factory
										, date		
										, id_costcenter
										, id_account
							
									UNION ALL
							
								-- Open Months
							
									SELECT
										scenario + '_Init' AS scenario
										, id_factory
										, date		
										, id_costcenter
										, id_account
										, SUM(value_semi_adjusted) AS value
										, 'Local' AS currency
								
									FROM XFC_PLT_AUX_Costs_Init_GM
									
									WHERE scenario = '{sScenario}'
									AND id_factory = '{sFactory}'
									AND YEAR(date) = {sYear}	
									AND MONTH(date) >= {sMonthFirst}
								
									GROUP BY 
										scenario
										, id_factory
										, date		
										, id_costcenter		
										, id_account
								
									UNION ALL
								
									SELECT
										scenario AS scenario
										, id_factory
										, date		
										, id_costcenter
										, id_account
										, SUM(value_semi_adjusted) AS value
										, 'Local' AS currency
								
									FROM XFC_PLT_AUX_Costs_Init_GM
									
									WHERE scenario = '{sScenario}'
									AND id_factory = '{sFactory}'
									AND YEAR(date) = {sYear}	
									AND MONTH(date) >= {sMonthFirst}
								
									GROUP BY 
										scenario
										, id_factory
										, date		
										, id_costcenter			
										, id_account
						
							"
							
'							BRApi.ErrorLog.LogMessage(si, "sql: " & sql)
							
							Dim sqlDel As String = $"								
								DELETE FROM XFC_PLT_AUX_Costs_Init_GM	
								WHERE scenario = '{sScenario}'
								AND id_factory = '{sFactory}'
								AND YEAR(date) = {sYear}		
							
								DELETE FROM XFC_PLT_FACT_Costs	
								WHERE scenario LIKE '{sScenario}'
								AND id_factory = '{sFactory}'
								AND YEAR(date) = {sYear}	
							
								DELETE FROM XFC_PLT_FACT_Costs	
								WHERE scenario LIKE '{sScenario}_Init'
								AND id_factory = '{sFactory}'
								AND YEAR(date) = {sYear}	
								"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(dbConn, sqlDel, True)							
							End Using
							
							uti_sharedqueries.update_Log(si, sScenario, sFactory, "XFC_PLT_FACT_Costs","Init")
							
					#End Region	
					
					#Region "Initialization Criteria"	
					
						Else If args.FunctionName.XFEqualsIgnoreCase("PlanInitCriteria") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sFactory As String = args.NameValuePairs("factory")	
				
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries

							sql = $"		
				
								-- 1. Previous Clear
							
								DELETE FROM XFC_PLT_AUX_Costs_Init_GM_Criteria
								WHERE scenario = '{sScenario}'
								AND year = {sYear}
								AND id_factory = '{sFactory}'	
							
								-- 2. Initialization Criteria
							
								INSERT INTO XFC_PLT_AUX_Costs_Init_GM_Criteria
								(
									scenario
									, year
									, id_factory
									, id_account
									, criteria
									, inflation
								)
											
								SELECT
									'{sScenario}' AS scenario
									, {sYear} AS year
									, '{sFactory}' AS id_factory					
									, A.id
									, 'Activity Index GM' AS Criteria
									, 0 AS inflation									
									
								FROM XFC_PLT_MASTER_Account A
								WHERE A.account_type IN (1,2,3,4,5)
							
								UNION ALL
							
								SELECT
									'{sScenario}' AS scenario
									, {sYear} AS year
									, '{sFactory}' AS id_factory
									, A.id
									, 'Activity Index Global' AS Criteria
									, 0 AS inflation
									
								FROM XFC_PLT_MASTER_Account A
								WHERE A.account_type NOT IN (1,2,3,4,5)								
						
							"
							
					#End Region						
					
					#Region "Generate VTU"	
					
						Else If args.FunctionName.XFEqualsIgnoreCase("GenerateVTU") Then
							
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")
							Dim sMonth As String = args.NameValuePairs("month")
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sView As String = args.NameValuePairs("view")								
							Dim sType As String = "TAJ"
							Dim sCurrency As String = args.NameValuePairs("currency")
							Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)													
							
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim sql_VTU = IIf(sView = "Last_12_Months","VTU_12_Months","VTU_data")	
							Dim sql_VTU_data As String = uti_sharedqueries.AllQueries(si, sql_VTU, sFactory, sMonth, sYear, sScenario, "", "", sView, sType, "", sCurrency)												
							
							Dim sqlDel As String = $"								
								DELETE FROM XFC_PLT_FACT_Costs_VTU	
								WHERE scenario = '{sScenario}'
								AND id_factory = '{sFactory}'
								AND YEAR(date) = {sYear}
								AND MONTH(date) = {sMonth}
								"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(dbConn, sqlDel, True)							
							End Using
							
							sql = sql_VTU_data & $"		

								-- 1. Insert AUX by GM							
							
								INSERT INTO XFC_PLT_FACT_Costs_VTU
								(
									scenario
									, date
									, id_factory
									, id_product
									, VTU
									, VTU_corrected
									, comment
								)
								
								SELECT
									'{sScenario}' AS scenario
									, DATEFROMPARTS({sYear},{sMonth},1) AS date
									, '{sFactory}' AS id_factory
									, id_product
									, SUM(VTU) AS VTU
									, NULL AS VTU_corrected
									, NULL AS comment
								FROM (
								
										-- VTU PRODUCTO
										
										SELECT 
											F.id_product AS id_product
											,M.description AS desc_product
										    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
										   		ELSE F.cost / F.volume
										   		* F.activity_UO1 / F.activity_UO1_total 
											END AS VTU			
										
										FROM factVTU F
										LEFT JOIN XFC_PLT_MASTER_Product M
											ON F.id_product = M.id						
									
										UNION ALL
										
										-- VTU COMPONENTS
										
										SELECT  
											P.id_product_final AS id_product
										 	,M.description AS desc_product
								         	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
									     		ELSE F.cost / F.volume
									     		* F.activity_UO1 / F.activity_UO1_total 
												* P.exp_coefficient END AS VTU					
										
										FROM Product_Component_Recursivity P
										LEFT JOIN factVTU F
											ON F.id_product = P.id_component 
										LEFT JOIN XFC_PLT_MASTER_Product M
											ON P.id_product_final = M.id								
									
								) AS RES
								
								GROUP BY id_product
							
								ORDER BY VTU DESC
							"							
							
'							BRApi.ErrorLog.LogMessage(si, sql)							
							
					#End Region						
							
					#Region "Update Fact Costs"	
					
						Else If args.FunctionName.XFEqualsIgnoreCase("UpdateFactCosts") Then
							
							Dim sYear As String = args.NameValuePairs("year")
							Dim sMonth As String = args.NameValuePairs("month")									
							
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							sql = uti_sharedqueries.AllQueries(si, "Update_Fact_Costs", "ALL", sMonth, sYear, "Actual", "", "", "", "", "", "")																		
							
'							BRApi.ErrorLog.LogMessage(si, sql)							
							
					#End Region		
					
					#Region "Hourly Rate Input Popup"							
						
						Else If args.FunctionName.XFEqualsIgnoreCase("hourly_rate_popup") Then																		
						
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()							
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True	
							
							Dim sNature As String = args.NameValuePairs("nature")
							Dim sFactory As String = args.NameValuePairs("factory")

							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTCosts_nature", sNature)																	
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLT_factory", sFactory)		
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard.Add("prm_PLTCosts_hourly_rate_input", String.Empty)																	
							Return selectionChangedTaskResult	
							
'							BRApi.ErrorLog.LogMessage(si, "scenario: " & sScenario)
							
					#End Region						
					
					#Region "Hourly Rate Input Save"		
						
						Else If args.FunctionName.XFEqualsIgnoreCase("hourly_rate_save") Then
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()							
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = True																										
							selectionChangedTaskResult.ShowMessageBox = True
													
'							Dim sScenario As String = args.NameValuePairs("scenario")		
'							Dim sYear As String = args.NameValuePairs("year")		
'							Dim sScenarioRef As String = args.NameValuePairs("scenario_ref")	
							
							Dim sScenario As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLTProduction_scenario_selected_PopUp")		
							Dim sYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLTProduction_year_selected_PopUp")
							
							Dim sFactory As String = args.NameValuePairs("factory")							
							Dim sNature As String = args.NameValuePairs("nature")
							Dim sNatureId As String = If(sNature = "MOD","1","2")
							Dim sValue As String = args.NameValuePairs("value")																
							sValue = If (sValue = "", "0", sValue)
							
'							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear, 12)) Then																														
								
								sql = $"
								
								-- 1. Previous clear
								
								DELETE FROM XFC_PLT_AUX_HourlyRate_Input
								WHERE 1=1
								AND scenario = '{sScenario}' 
								AND year = {sYear}							
								AND id_factory = '{sFactory}'
								AND id_nature = '{sNatureId}'
	
								-- 2. Insert Input
								
								INSERT INTO XFC_PLT_AUX_HourlyRate_Input
								(
									scenario
									,year
									,id_factory
									,id_nature
									,value
								)
								VALUES
								(
									'{sScenario}' 
									,{sYear}
									,'{sFactory}'
									,'{sNatureId}'
									,{sValue}
								)							
								"
								
								BRApi.ErrorLog.LogMessage(si, sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using		
								
								BRApi.Dashboards.Parameters.SetLiteralParameterValue(Si, False, "prm_PLTProduction_value", String.Empty)
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Hourly rate modified succesfully."		
							
'							Else
								
'								BRApi.Dashboards.Parameters.SetLiteralParameterValue(Si, False, "prm_PLTProduction_value", String.Empty)
'								selectionChangedTaskResult.IsOK = False
'								selectionChangedTaskResult.Message = "Invalid scenario or period."
							
'							End If
																	
							Return selectionChangedTaskResult									
							
						#End Region
						
					
						End If
						
						If sql <> String.Empty Then
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using
						End If
						
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						
						#Region "Save VTU"
						
						If args.FunctionName.XFEqualsIgnoreCase("SaveVTU") Then
		
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")	
							Dim sMonth As String = args.NameValuePairs("month")		
							Dim sFactory As String = args.NameValuePairs("factory")
													
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.CancelDefaultSave = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								
								'Get save data task info
								Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
								
								'Create a datatable from modified rows
								Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
								
								Dim i As Integer = 0
								For Each column As DataColumn In dt.Columns
'									BRApi.ErrorLog.LogMessage(si, column.ColumnName & ": " & dt.Rows(0)(i).ToString)
									i = i+1
								Next
								
								'Declare column mapping dict
								Dim columnMappingDict As New Dictionary(Of String, String) From {
									{"id_factory", "id_factory"},
									{"id_product", "id_product"},
									{"desc_product", "desc_product"},
									{"VTU", "VTU"},
									{"VTU_corrected", "VTU_corrected"},
									{"VTU_final", "VTU_final"},
									{"comment", "comment"}
								}
								
								'Map and filter columns in DataTable
								dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
								
								Dim sqlRaw As String = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Costs_VTU')
										DROP TABLE XFC_PLT_RAW_Costs_VTU													
									
									CREATE TABLE XFC_PLT_RAW_Costs_VTU 
								 	(
									[id_factory] varchar(50) ,							
									[id_product] varchar(50) ,
									[desc_product] varchar(50) ,
									[VTU] decimal(18,6),
									[VTU_corrected] decimal(18,6),
									[VTU_final] decimal(18,6),
									[comment] varchar(500) 
								 	)"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using							
								
								'Save rows in aux table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_Costs_VTU", dt, "replace")
								
								'Save rows in aux table
								Dim sql As String = $"													
									UPDATE F
										SET F.VTU_corrected = R.VTU_corrected
											,F.comment = R.comment
									FROM XFC_PLT_FACT_Costs_VTU F
									INNER JOIN XFC_PLT_RAW_Costs_VTU R
										ON F.id_factory = R.id_factory
										AND F.id_product = R.id_product
									WHERE F.scenario = '{sScenario}'
									AND F.id_factory = '{sFactory}'
									AND YEAR(F.date) = {sYear}
								    AND MONTH(F.date) = {sMonth}		
								
									INSERT INTO XFC_PLT_FACT_Costs_VTU
									(
										scenario
										, date
										, id_factory
										, id_product
										, VTU
										, VTU_corrected
										, comment
									)
									
									SELECT
										'{sScenario}' AS scenario
										, DATEFROMPARTS({sYear},{sMonth},1) AS date
										, '{sFactory}' AS id_factory
										, R.id_product
										, R.VTU AS VTU
										, R.VTU_corrected AS VTU_corrected
										, R.comment AS comment
									FROM XFC_PLT_RAW_Costs_VTU R
									LEFT JOIN XFC_PLT_FACT_Costs_VTU F
										ON F.scenario = '{sScenario}'
										AND F.id_factory = '{sFactory}'
										AND YEAR(F.date) = {sYear}
								    	AND MONTH(F.date) = {sMonth}
										AND R.id_product = F.id_product
									WHERE F.id_product IS NULL
								"
											
								'BRApi.ErrorLog.LogMessage(si,sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
								
								sqlRaw = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Costs_VTU')
										DROP TABLE XFC_PLT_RAW_Costs_VTU																																
								 	"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using	
								
								saveDataTaskResult.IsOK = True
								saveDataTaskResult.Message = "Change successfully completed."
								
							Else
								
								saveDataTaskResult.IsOK = False
								saveDataTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return saveDataTaskResult
							
							#End Region															
							
						#Region "Update Criteria"		
						
						Else If args.FunctionName.XFEqualsIgnoreCase("UpdateCriteria") Then
							
							Dim sFactory As String = args.NameValuePairs("factory")
							Dim sCriteria As String = args.NameValuePairs("criteria")
							
'							BRAPi.ErrorLog.LogMessage(si, "sCriteria: " & sCriteria)
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							'Create a datatable from modified rows
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
							
							'Declare column mapping dict
							Dim columnMappingDict As New Dictionary(Of String, String) From {
								{"scenario", "scenario"},
								{"year", "year"},
								{"id_factory", "id_factory"},
								{"nature", "nature"},	
								{"id_account", "id_account"},									
								{"account", "account"},
								{"criteria", "criteria"},
								{"inflation", "inflation"},								
								{"modify", "modify"}
							}
							
							'Map and filter columns in DataTable
							dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
							Dim sqlRaw As String = "
								IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Costs_Init_GM_Criteria')
									DROP TABLE XFC_PLT_RAW_Costs_Init_GM_Criteria													
								
								CREATE TABLE XFC_PLT_RAW_Costs_Init_GM_Criteria
							 	(
								[scenario] varchar(50) ,	
								[year] varchar(50) ,	
								[id_factory] varchar(50) ,								
								[nature] varchar(50) ,		
								[id_account] varchar(50) ,								
								[account] varchar(50) ,
								[criteria] varchar(50) ,
								[inflation] varchar(50), 
								[modify] BIT DEFAULT 0
							 	)"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
							End Using													
							
							'Save rows in raw table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_Costs_Init_GM_Criteria", dt, "replace")
							
							'Save rows in aux table
							Dim sql As String = $"													
								UPDATE M
							
								SET M.criteria = '{sCriteria}' 
								
								FROM XFC_PLT_AUX_Costs_Init_GM_Criteria as M
							
								INNER JOIN XFC_PLT_RAW_Costs_Init_GM_Criteria as R
									ON M.scenario = R.scenario
									AND M.year = R.year
									AND M.id_factory = R.id_factory
									AND M.id_account = R.id_account									
							"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
							End Using
							
							sqlRaw = "
								IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Costs_Init_GM_Criteria')
									DROP TABLE XFC_PLT_RAW_Costs_Init_GM_Criteria																																
							 	"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
							End Using								
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.CancelDefaultSave = True
							Return saveDataTaskResult
							
						#End Region	
												
						#Region "PLAN - Save Costs"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveCosts") Then
		
							Dim sScenario As String = args.NameValuePairs("scenario")
							Dim sYear As String = args.NameValuePairs("year")							
							Dim sFactory As String = args.NameValuePairs("factory")
													
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.ShowMessageBox = True
							saveDataTaskResult.CancelDefaultSave = True
							
							If (uti_sharedqueries.ValidScenarioPeriod(si, sScenario, sYear)) Then
							
								Dim sMonthFirst As String = If (sScenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)	
								
								'Get save data task info
								Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
								
								'Create a datatable from modified rows
								Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)					
								
								'Declare column mapping dict
								Dim columnMappingDict As New Dictionary(Of String, String) From {
									{"id_factory", "id_factory"},
									{"id_costcenter", "id_costcenter"},
									{"id_account", "id_account"},
									{"account_costcenter", "account_costcenter"},
									{"account", "account"},
									{"M01", "M01"},
									{"M02", "M02"},
									{"M03", "M03"},
									{"M04", "M04"},
									{"M05", "M05"},
									{"M06", "M06"},
									{"M07", "M07"},
									{"M08", "M08"},
									{"M09", "M09"},
									{"M10", "M10"},
									{"M11", "M11"},
									{"M12", "M12"}
								}
								
								'Map and filter columns in DataTable
								dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
								
								Dim sqlRaw As String = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Planning_Costs')
										DROP TABLE XFC_PLT_RAW_Planning_Costs													
									
									CREATE TABLE XFC_PLT_RAW_Planning_Costs
								 	(
									[id_factory] varchar(50) ,							
									[id_costcenter] varchar(50) ,
									[id_account] varchar(50) ,
									[account_costcenter] varchar(200) ,
									[account] varchar(50) ,
									[M01] decimal(18,2),
									[M02] decimal(18,2),
									[M03] decimal(18,2),
									[M04] decimal(18,2),
									[M05] decimal(18,2),
									[M06] decimal(18,2),	
									[M07] decimal(18,2),
									[M08] decimal(18,2),
									[M09] decimal(18,2),
									[M10] decimal(18,2),
									[M11] decimal(18,2),
									[M12] decimal(18,2)	
								 	)"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using			
								
								'Save rows in aux table
								UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_Planning_Costs", dt, "replace")
								
								'Save rows in aux table
								Dim sql As String = $"													
										
								-- 1. Previous clear
							
								DELETE F
								FROM XFC_PLT_FACT_Costs F
								INNER JOIN XFC_PLT_RAW_Planning_Costs R
									ON  F.id_factory = R.id_factory
									AND F.id_costcenter = R.id_costcenter
									AND F.id_account = R.id_account
								WHERE F.scenario = '{sScenario}'
								AND YEAR(F.date) = {sYear}
								AND F.id_factory = '{sFactory}'
								AND MONTH(F.date) >= {sMonthFirst}
							
								-- 2. Insert closed months
							
								INSERT INTO XFC_PLT_FACT_Costs
								(
									scenario
									,date
									,id_factory
									,id_account
									,id_rubric
									,id_costcenter
									,value
									,currency
								)					
								
								SELECT scenario, DATEFROMPARTS({sYear}, REPLACE(month,'M',''), 1) AS date, id_factory, id_account, id_rubric, id_costcenter, value, currency
								FROM (
									SELECT
									    '{sScenario}' AS scenario							
									    , '{sFactory}' AS id_factory
										, id_account As id_account
										, '-1' AS id_rubric
									    , ISNULL(id_costcenter,'-1') AS id_costcenter								
										, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12	
										, '' AS currency
		
									FROM XFC_PLT_RAW_Planning_Costs	
								
								) AS SourceTable
								UNPIVOT (
								    value FOR month IN (M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12)
								) AS UnpivotedTable
								
								WHERE REPLACE(month,'M','') >= {sMonthFirst}
																			
								"
											
								'BRApi.ErrorLog.LogMessage(si,sql)
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sql,True, True)
								End Using
								
								uti_sharedqueries.update_Log(si, sScenario, sFactory, "XFC_PLT_FACT_Costs","Save")
								
								sqlRaw = "
									IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'XFC_PLT_RAW_Planning_Costs')
										DROP TABLE XFC_PLT_RAW_Planning_Costs																																
								 	"
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlRaw,True, True)
								End Using	
								
								saveDataTaskResult.IsOK = True
								saveDataTaskResult.Message = "Change successfully completed."
								
							Else
								
								saveDataTaskResult.IsOK = False
								saveDataTaskResult.Message = "Invalid scenario or period."
							
							End If
																	
							Return saveDataTaskResult
							
							#End Region	
							
						End If						
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function	
		
		#Region "Helper Functions"
		
			#Region "Generate Insert"
			
		Private Function GenerateInsertSQLList(ByVal dataTable As DataTable, ByVal tableName As String) As List(Of String)
		
		    Dim insertStatements As New List(Of String)()
		    Dim sb As New StringBuilder()
		
		    ' Validate inputs
		    If dataTable Is Nothing OrElse dataTable.Rows.Count = 0 Then
		        Throw New ArgumentException("The DataTable is empty or null.")
		    End If
		
		    If String.IsNullOrEmpty(tableName) Then
		        Throw New ArgumentException("The table name is null or empty.")
		    End If
		
		    ' Get the column names
		    Dim columnNames As String = String.Join(", ", dataTable.Columns.Cast(Of DataColumn)().[Select](Function(c) c.ColumnName))
		
		    ' Batch size limit
		    Dim batchSize As Integer = 1000
		    Dim currentBatchSize As Integer = 0
		
		    ' Build the initial part of the INSERT INTO statement
		    sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		
		    ' Iterate through each row in the DataTable
		    For i As Integer = 0 To dataTable.Rows.Count - 1
		        Dim rowValues As New List(Of String)()
		
		        For Each column As DataColumn In dataTable.Columns
		            Dim value As Object = dataTable.Rows(i)(column)
		
		            If value Is DBNull.Value Then
		                rowValues.Add("NULL")
		            ElseIf TypeOf value Is String OrElse TypeOf value Is Char Then
		                rowValues.Add($"'{value.ToString().Replace("'", "''").Replace(",", ".")}'") ' Escape single quotes
		            ElseIf TypeOf value Is DateTime Then
		                rowValues.Add($"'{CType(value, DateTime).ToString("yyyy-MM-dd HH:mm:ss.fff")}'")
		            Else
		                rowValues.Add(value.ToString().Replace(",", ".").Replace("'","''"))
		            End If
		        Next
		
		        ' Combine row values into a comma-separated string and add to StringBuilder
		        sb.AppendLine($"({String.Join(", ", rowValues)}){If(currentBatchSize < batchSize - 1 AndAlso i < dataTable.Rows.Count - 1, ",", ";")}")
		        currentBatchSize += 1
		
		        ' If the batch size limit is reached, or it's the last row, finalize the current statement and start a new one
		        If currentBatchSize = batchSize OrElse i = dataTable.Rows.Count - 1 Then
		            insertStatements.Add(sb.ToString())
		            sb.Clear()
		            If i < dataTable.Rows.Count - 1 Then
		                sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		            End If
		            currentBatchSize = 0
		        End If
		    Next
		
		    Return insertStatements
			
		End Function
		
			#End Region
			
			#Region "PopUp"
			
		Public Function popUp(ByVal message As String) As XFSelectionChangedTaskResult
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()

			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = $"{message}"							
			Return selectionChangedTaskResult
			
		End Function
		
			#End Region
			
			#Region "CostDistribution Insert"
			
		Public Function costsDistributionInsert(ByVal cecoNature As String) As String
			
			Dim sqlInsert As String = String.Empty
			

			If cecoNature = "UNA"
				sqlInsert = "
								MERGE XFC_PLT_FACT_CostsDistribution AS target
								USING (
									SELECT 
										id_factory,
										scenario,
										date,
										id_account,
										id_costcenter,
										id_averagegroup,
										id_product,
										value,
										type_tso_taj,
										[type]
									FROM costsDistributionInsert
									WHERE [type] IN ('NA', 'NoProd')
								) AS source
								ON 
									target.id_factory = source.id_factory AND
									target.scenario = source.scenario AND
									target.date = source.date AND
									target.id_account = source.id_account AND
									target.id_costcenter = source.id_costcenter AND
									target.id_averagegroup = source.id_averagegroup AND
									target.id_product = source.id_product AND
									target.type_tso_taj = source.type_tso_taj
								
								-- 🔁 Solo actualiza si el tipo es 'NoProd'
								WHEN MATCHED AND source.[type] = 'NoProd' THEN
									UPDATE SET target.value = target.value + source.value
								
								-- ➕ Solo inserta si es 'NA' o 'NoProd' y no hay coincidencia
								WHEN NOT MATCHED AND source.[type] IN ('NA', 'NoProd') THEN
									INSERT (
										id_factory,
										scenario,
										date,
										id_account,
										id_costcenter,
										id_averagegroup,
										id_product,
										value,
										type_tso_taj
									)
									VALUES (
										source.id_factory,
										source.scenario,
										source.date,
										source.id_account,
										source.id_costcenter,
										source.id_averagegroup,
										source.id_product,
										source.value,
										source.type_tso_taj
									);
										
								"	
			Else 
				sqlInsert = "
							INSERT INTO XFC_PLT_FACT_CostsDistribution (id_factory, scenario, date, id_account, id_costcenter, id_averagegroup, id_product, value, type_tso_taj)
							
							SELECT 
								id_factory
								, scenario
								, date
								, id_account
								, id_costcenter
								, id_averagegroup
								, id_product
								, value
								, type_tso_taj
							
							FROM costsDistributionInsert
									
						"
			End If
			
			Return sqlInsert
			
		End Function
		
			#End Region
			
		#End Region
		
	End Class
End Namespace
