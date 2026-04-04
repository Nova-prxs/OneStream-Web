Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.Diagnostics
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

' {Workspace.Current.PLTView.queries}{}{}
Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.queries
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						'Dim names As New List(Of String)()
						'names.Add("MyDataSet")
						'Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
'													
						Dim shared_queries 	As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
						' shared_queries.AllQueries(si, query, factory, month, year, scenario, scenarioRef, time, view, "Existing", product, currency, monthFCT, accountList, queryVersion)
						
						' Query Variables
						Dim sql 	As String = String.Empty
						Dim dt 		As DataTable = Nothing
						
						
						' VARIABLES													
						Dim factory 		As String = args.NameValuePairs.GetValueOrEmpty("factory")
						Dim scenario 		As String = args.NameValuePairs.GetValueOrEmpty("scenario")
						Dim year 			As String = args.NameValuePairs.GetValueOrEmpty("year")
						Dim month 			As String = args.NameValuePairs.GetValueOrEmpty("month")
						Dim currency 		As String = args.NameValuePairs.GetValueOrEmpty("currency")
						Dim account 		As String = args.NameValuePairs.GetValueOrEmpty("account")
						Dim view 			As String = args.NameValuePairs.GetValueOrEmpty("view")
						Dim product 		As String = args.NameValuePairs.GetValueOrEmpty("product")							
						Dim scenarioref 	As String = args.NameValuePairs.GetValueOrEmpty("scenarioref")
						Dim GM 				As String = args.NameValuePairs.GetValueOrEmpty("GM")
						Dim GM_Function		As String = args.NameValuePairs.GetValueOrEmpty("GM_Function")
						Dim indicator 		As String = args.NameValuePairs.GetValueOrEmpty("indicator")
						Dim firsMonthFCST 	As String = If(scenario.StartsWith("RF"), BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_month_first_forecast"), 1)						
						
						' Dynamic Reporting Variables
						Dim selectDetail As String = args.NameValuePairs.GetValueOrDefault("reportDetail","scenario, id_factory, desc_factory, month, id_product, desc_product, id_component, account_type, variability, id_averagegroup, id_account, REF, impact_parity").Replace(" ", "")
						Dim scenarioOutput As String = String.Empty
						Dim impactParity As String = String.Empty
						
						selectDetail = selectDetail.Replace(" ", "")
						If selectDetail.Contains(",REF")  Then
							selectDetail = selectDetail.Replace(",REF","")
							scenarioOutput = "REF"
						Else If selectDetail.Contains("REF,")
							selectDetail = selectDetail.Replace("REF,","")
							scenarioOutput = "REF"
						End If
						
						If selectDetail.Contains(",impact_parity")  Then
							selectDetail = selectDetail.Replace(",impact_parity","")
							impactParity = "Yes"
						Else If selectDetail.Contains("impact_parity,")
							selectDetail = selectDetail.Replace("impact_parity,","")
							impactParity = "Yes"
						End If
						
						'FILTERS
						Dim itemsAcc() As String = account.Replace("'","").Split(","c)
						Dim sAccountFilter As String = If(account = "All" Or account="" Or account="|!account!|", "", "AND id_account IN( '" & String.Join("','", itemsAcc.Select(Function(s) s.Trim()).ToArray()) & "' )")
						
						Dim itemsProd() As String = product.Replace("'","").Split(","c)
						Dim sProductFilter As String = If(product = "All" Or product="" Or product="|!product!|", "", "AND id_product IN( '" & String.Join("','", itemsProd.Select(Function(s) s.Trim()).ToArray()) & "' )")
						
						Dim sMonthFilter As String = If(month="13", "", $"AND MONTH(date)={month}")
						Dim sMonthMFilter As String = If(month="13", "", $"AND Month={month}")
						Dim sFactoryFilter As String = If(factory="All", "", $"AND id_factory='{factory}'")
						
						Dim topSelect 	As String = args.NameValuePairs.GetValueOrEmpty("topSelect")	
						topSelect = If(topSelect="True"," TOP(10) ", "")
						
						' BIBlend - Queries
						Dim sBiBlendQuery As String = "No"
						
						If args.DataSetName.XFEqualsIgnoreCase("Example") Then
							
							sql = $"
								SELECT 'FACTORY: {factory} | YEAR: {year}' as Message
							UNION ALL
								SELECT 'product: {product} | MONTH {month}' as Message
							UNION ALL
								SELECT 'scenario: {scenario} | currency {currency} | account {account} | view {view}' as Message
							"
							
						#Region "VTUs"	
						
						'PRODUCTION
						
						Else If args.DataSetName.XFEqualsIgnoreCase("AllVTUs") Then
							
							sBiBlendQuery="Yes"							
'							Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_aux_tables", factory, month, year, scenario, "", "", "", "Existing", "", currency,)							
							Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
							
							#Region "All VTUs"
							
							sql = $"
								
								{sqlVTU}
							
								, OwnRowsFilter AS (
									SELECT DISTINCT Month, id_product
									FROM OwnRows
									WHERE VTU_YTD <> 0
								)
								
								/* --- QUERY --- */
							
								SELECT {topSelect}
										scenario
										, id_factory
										, FA.description AS desc_factory
										, month										
										, id_product
										, M.description AS desc_product							
									   	, SUM(VTU_Periodic) AS VTU_Periodic
										, SUM(VTU_YTD) AS VTU_YTD
							
								FROM (
								    SELECT * FROM OwnRows
								    UNION ALL
								    SELECT C.* 
									FROM CompRows C
									INNER JOIN OwnRowsFilter O
										ON C.month = O.month
										AND C.id_product = O.id_product
								) AS Q
							
								LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
								LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
								LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
							
								GROUP BY scenario, id_factory, month, M.description, FA.description, id_product
							
								HAVING SUM(VTU_YTD) <> 0
							
								ORDER BY id_factory, month, id_product
							
								"
								
'							BRAPI.ErrorLog.LogMessage(SI, sql)
							#End Region												
							
						Else If args.DataSetName.XFEqualsIgnoreCase("NatureVaribility") Then
														
							sBiBlendQuery="Yes"							
'							Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_aux_tables", factory, month, year, scenario, "", "", "", "Existing", "", currency)
							Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)							
							
							#Region "VTU Nature - Variability"
							
							sql  = $"
							
								{sqlVTU}
							
								, OwnRowsFilter AS (
									SELECT DISTINCT Month, id_product
									FROM OwnRows
									WHERE VTU_YTD <> 0
								)							
							
								/* --- QUERY --- */
							
								SELECT {topSelect}
										scenario
										, id_factory
										, FA.description AS desc_factory
										, month										
										, id_product
										, M.description AS desc_product
										, CASE WHEN account_type IN (1,2,3,4,5)
								             THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + '-' + ISNULL(N.Description,'')
								             ELSE '99-Others' END AS nature
										, variability
							
									   	, SUM(VTU_Periodic) AS VTU_Periodic
										, SUM(VTU_YTD) AS VTU_YTD
							
								FROM (
								    SELECT * FROM OwnRows
								    UNION ALL
								    SELECT C.* 
									FROM CompRows C
									INNER JOIN OwnRowsFilter O
										ON C.month = O.month
										AND C.id_product = O.id_product
								) AS Q
							
								LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
								LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
								LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
								
								GROUP BY scenario, id_factory, month, M.description, FA.description,
								         id_product, CASE WHEN account_type IN (1,2,3,4,5)
								             THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + '-' + ISNULL(N.Description,'')
								             ELSE '99-Others' END, variability							
							
								HAVING SUM(VTU_YTD) <> 0
							
								ORDER BY id_factory, month, id_product, nature, variability		
							
								"
							
							#End Region								
						
						Else If args.DataSetName.XFEqualsIgnoreCase("NatureGM") Then
														
							sBiBlendQuery="Yes"							
'							Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_aux_tables", factory, month, year, scenario, "", "", "", "Existing", "", currency)
							Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
							
							#Region "NatureGM"
							
							sql = $"
								
								{sqlVTU}
							
								, OwnRowsFilter AS (
									SELECT DISTINCT Month, id_product
									FROM OwnRows
									WHERE VTU_YTD <> 0
								)							
								
								/* --- QUERY --- */
							
								SELECT {topSelect}
									scenario
									, id_factory
									, FA.description AS desc_factory
									, month										
									, id_product
									, M.description AS desc_product
									, id_averagegroup
									, CASE WHEN account_type IN (1,2,3,4,5)
								            THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + '-' + ISNULL(N.Description,'')
								            ELSE '99-Others' END AS nature
									, variability
									, SUM(VTU_Periodic) AS VTU_Periodic
									, SUM(VTU_YTD) AS VTU_YTD
							
								FROM (
								    SELECT * FROM OwnRows
								    UNION ALL
								    SELECT C.* 
									FROM CompRows C
									INNER JOIN OwnRowsFilter O
										ON C.month = O.month
										AND C.id_product = O.id_product
								) AS Q
							
								LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
								LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
								LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
							
								GROUP BY scenario, id_factory, month, M.description, FA.description,
								         id_product, id_averagegroup, CASE WHEN account_type IN (1,2,3,4,5)
								             THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + '-' + ISNULL(N.Description,'')
								             ELSE '99-Others' END, variability
							
								HAVING SUM(VTU_YTD) <> 0
							
								ORDER BY id_factory, month, id_product, id_averagegroup, nature, variability		
								"
							
							
						#End Region
						
						Else If args.DataSetName.XFEqualsIgnoreCase("ComponentsGMNature") Then
							
							sBiBlendQuery="Yes"							
							'Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_aux_tables", factory, month, year, scenario, "", "", "", "Existing", "", currency)
							Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
							
							#Region "ComponentsGMNature"
							
							sql = $"
								
								{sqlVTU}
							
								, OwnRowsFilter AS (
									SELECT DISTINCT Month, id_product
									FROM OwnRows
									WHERE VTU_YTD <> 0
								)							
								
								/* --- QUERY --- */
							
								SELECT {topSelect}
									scenario
									, id_factory
									, FA.description AS desc_factory
									, month										
									, id_product
									, M.description AS desc_product
									, id_component
									, id_averagegroup
									, CASE WHEN account_type IN (1,2,3,4,5)
								            THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + '-' + ISNULL(N.Description,'')
								            ELSE '99-Others' END AS nature
									-- , variability
									
									, SUM(VTU_Periodic) AS VTU_Periodic
									, SUM(VTU_YTD) AS VTU_YTD
							
								FROM (
								    SELECT * FROM OwnRows
								    UNION ALL
								    SELECT C.* 
									FROM CompRows C
									INNER JOIN OwnRowsFilter O
										ON C.month = O.month
										AND C.id_product = O.id_product
								) AS Q
							
								LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
								LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
								LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
								
								-- WHERE (id_factory = '{factory}' OR '{factory}'='All')
								WHERE Q.month = {month}	
							
								GROUP BY scenario, id_factory, month, M.description, FA.description,
								         id_product, id_component, id_averagegroup, CASE WHEN account_type IN (1,2,3,4,5)
								             THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + '-' + ISNULL(N.Description,'')
								             ELSE '99-Others' END -- , variability
							
								-- HAVING SUM(VTU_YTD) <> 0
							
								ORDER BY id_factory, month, id_product, id_averagegroup, nature -- , variability		
								"
							
							#End Region	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("NatureGMPivot") Then
							
							sBiBlendQuery="Yes"							
'							Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_aux_tables", "", month, year, scenario, "", "", "", "Existing", "", currency)
							Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
							
							#Region "NatureGMPivot"
														
							sql = $"
							
							{sqlVTU}
							
							, OwnRowsFilter AS (
									SELECT DISTINCT Month, id_product
									FROM OwnRows
									WHERE VTU_YTD <> 0
							)
							
							SELECT 
								    id_factory AS Factory,
									id_product AS [Id Product],
									desc_product AS [Desc Product],
							
									SUM([01_MOD_Fix]			) AS 	[01_MOD_Fix]						,	
									SUM([01_MOD_Var]			) AS 	[01_MOD_Var]						,
									SUM([02_MOS_Fix]			) AS 	[02_MOS_Fix]						,
									SUM([02_MOS_Var]			) AS 	[02_MOS_Var]						,
									SUM([03_FIP_Fix]			) AS 	[03_FIP_Fix]						,
									SUM([03_FIP_Var]			) AS 	[03_FIP_Var]						,
									SUM([04_Taxe_Fix]			) AS 	[04_Taxe_Fix]						,
									SUM([04_Taxe_Var]			) AS 	[04_Taxe_Var]						,
									SUM([05_Depreciation_Fix]	) AS 	[05_Depreciation_Fix]				,
									SUM([05_Depreciation_Var]	) AS 	[05_Depreciation_Var]				,
									SUM([99_Others_Fix]			) AS 	[99_Others_Fix]						,
									SUM([99_Others_Var]			) AS 	[99_Others_Var]						,
							
									SUM( ISNULL([01_MOD_Fix],0) + ISNULL([01_MOD_Var],0) 
									+ ISNULL([02_MOS_Fix],0) + ISNULL([02_MOS_Var],0) 
									+ ISNULL([03_FIP_Fix],0) + ISNULL([03_FIP_Var],0) 
									+ ISNULL([04_Taxe_Fix],0) + ISNULL([04_Taxe_Var],0)
									+ ISNULL([05_Depreciation_Fix],0) + ISNULL([05_Depreciation_Var],0) 
									+ ISNULL([99_Others_Fix],0) + ISNULL([99_Others_Var],0) 
									) AS Total

								FROM (
	
									/* --- QUERY --- */
								
									SELECT scenario
											, id_factory
											, FA.description AS desc_factory
											, month										
											, id_product
											, M.description AS desc_product
											,CASE 
									            WHEN F.account_type IN (1,2,3,4,5) THEN RIGHT('0' + F.account_type, 2) + '_' + ISNULL(N.Description, '')
									            ELSE '99_Others'
									          END + '_Fix' AS nature		
										   	, SUM(VTU_Periodic) AS VTU_Periodic
											, SUM(VTU_YTD) AS VTU_YTD
								
									FROM (
									    SELECT * FROM OwnRows
									    UNION ALL
									    SELECT C.* 
										FROM CompRows C
										INNER JOIN OwnRowsFilter O
											ON C.month = O.month
											AND C.id_product = O.id_product
									) AS F
								
									LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = F.id_factory
									LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = F.id_product
									LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = F.account_type
									
									WHERE 
										month = {month}
										AND variability = 'F'
							
									GROUP BY scenario, id_factory, month, M.description, FA.description, id_product 
											, CASE 
									            WHEN F.account_type IN (1,2,3,4,5) THEN RIGHT('0' + F.account_type, 2) + '_' + ISNULL(N.Description, '')
									            ELSE '99_Others'
									        END + '_Fix'
								
									HAVING SUM(VTU_YTD) <> 0
							
								UNION ALL
							
									SELECT scenario
											, id_factory
											, FA.description AS desc_factory
											, month										
											, id_product
											, M.description AS desc_product
											,CASE 
									            WHEN F.account_type IN (1,2,3,4,5) THEN RIGHT('0' + F.account_type, 2) + '_' + ISNULL(N.Description, '')
									            ELSE '99_Others'
									          END + '_Var' AS nature		
										   	, SUM(VTU_Periodic) AS VTU_Periodic
											, SUM(VTU_YTD) AS VTU_YTD
								
									FROM (
									    SELECT * FROM OwnRows
									    UNION ALL
									    SELECT C.* 
										FROM CompRows C
										INNER JOIN OwnRowsFilter O
											ON C.month = O.month
											AND C.id_product = O.id_product
									) AS F
								
									LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = F.id_factory
									LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = F.id_product
									LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = F.account_type
									
									WHERE 
										month = {month}
										AND variability = 'V'
							
									GROUP BY scenario, id_factory, month, M.description, FA.description, id_product 
											, CASE 
									            WHEN F.account_type IN (1,2,3,4,5) THEN RIGHT('0' + F.account_type, 2) + '_' + ISNULL(N.Description, '')
									            ELSE '99_Others'
									        END + '_Var'
								
									HAVING SUM(VTU_YTD) <> 0
							
								) AS base
							
								PIVOT (
								    SUM(VTU_Periodic)
								    FOR nature IN (
										[01_MOD_Fix],	
										[01_MOD_Var],
										[02_MOS_Fix],
										[02_MOS_Var],
										[03_FIP_Fix],
										[03_FIP_Var],
										[04_Taxe_Fix],
										[04_Taxe_Var],
										[05_Depreciation_Fix],
										[05_Depreciation_Var],
										[99_Others_Fix],
										[99_Others_Var]
								    )
								) AS pvt
								
								WHERE 1=1
									AND id_factory = '{factory}'
							
								GROUP BY id_factory ,
									id_product,
									desc_product
							
							
								ORDER BY id_factory, Total DESC;
							
								"
							
							#End Region						
						
						Else If args.DataSetName.XFEqualsIgnoreCase("AccountsDetail") Then
							
							sBiBlendQuery="Yes"							
'							Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables_NewTables", factory, month, year, scenario, "", "", "", "Existing", "", currency,"","","Old")
							Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)	
							
							#Region "Accounts Detail"
								
								sql = $"
									
									{sqlVTU}
								
									, OwnRowsFilter AS (
										SELECT DISTINCT Month, id_product
										FROM OwnRows
										WHERE VTU_YTD <> 0
									)								
								
									, finalVTU_Report as (
									/* --- QUERY --- */
									
									SELECT
											scenario
											, id_factory
											, FA.description AS desc_factory
											, month										
											, id_product
											, M.description AS desc_product	
											, variability
											, id_account
										   	, SUM(CAST(COALESCE(VTU_Periodic,0) AS DECIMAL(18,9))) AS VTU_Periodic
											, SUM(CAST(COALESCE(VTU_YTD,0) AS DECIMAL(18,9))) AS VTU_YTD
								
									FROM (
									    SELECT * FROM OwnRows
									    UNION ALL
									    SELECT C.* 
										FROM CompRows C
										INNER JOIN OwnRowsFilter O
											ON C.month = O.month
											AND C.id_product = O.id_product
									) AS Q
								
									LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
									LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
									LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
								
									GROUP BY scenario, id_factory, month, M.description, FA.description, id_product, id_account, variability
								
									-- HAVING SUM(CAST(COALESCE(VTU_YTD,0) AS DECIMAL(18,9))) <> 0
								
									)
								
									SELECT *
									FROM finalVTU_Report
									WHERE 1=1
										AND VTU_YTD <> 0
										AND (id_factory = '{factory}' OR '{factory}'='All')
										AND month = {month}
										{sAccountFilter}
									ORDER BY id_factory, month, id_product, id_account
									"
								
'								BRApi.ErrorLog.LogMessage(si, sql)
								
								#End Region	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("DynamicReport") Then
							
							sBiBlendQuery="Yes"		
							
							#Region "Dynamic Report"
							
							Dim resultTable As DataTable = Nothing
							Dim sScenarioList As New List(Of String)
							sScenarioList.Add(scenario)
							
							If scenarioOutput = "REF" Then sScenarioList.Add(scenarioRef)
								
							Dim impactParityCount As Integer = 0
							Dim lastScenario As String = ""
							Dim sqlVTU As String = String.Empty
							
							For Each scenarioSub In sScenarioList
	
								Dim sw As Stopwatch = Stopwatch.StartNew
								Dim monthF As String = month
								
								If scenarioOutput = "REF" And scenarioSub=scenarioRef  And scenario.StartsWith("Budget") Then
									year = (CInt(year)-1).ToString()
								End If

								sqlVTU = GetSQLDataVTU(si, scenarioSub, month, factory, year, currency)
								
								#Region "All VTUs"
								
								sql = $"
									
									{sqlVTU}
								
									, OwnRowsFilter AS (
										SELECT DISTINCT Month, id_product
										FROM OwnRows
										WHERE VTU_YTD <> 0
									)
									, finalVTU_Report as (
									/* --- QUERY --- */
									
									SELECT
											  Q.scenario
											, Q.id_factory
											, MF.description AS desc_factory
											, Q.month										
											, Q.id_product
											, MP.description AS desc_product	
											, Q.id_component
											, Q.account_type AS id_account_type
											, MN.description AS account_type
											, Q.id_account
											, MA.description AS desc_account
											, Q.variability
											, Q.id_averagegroup
											, MG.description as desc_averagegroup
										   	, SUM(CAST(COALESCE(Q.VTU_Periodic,0) 	AS DECIMAL(18,9))) AS VTU_Periodic
											, SUM(CAST(COALESCE(Q.VTU_YTD,0) 		AS DECIMAL(18,9))) AS VTU_YTD
								
									FROM (
									    SELECT * FROM OwnRows
									    UNION ALL
									    SELECT C.* 
										FROM CompRows C
										INNER JOIN OwnRowsFilter O
											ON C.month = O.month
											AND C.id_product = O.id_product
									) AS Q
								
									LEFT JOIN XFC_PLT_MASTER_Factory 		MF ON MF.id = Q.id_factory
									LEFT JOIN XFC_PLT_MASTER_Product 		MP ON MP.id = Q.id_product
									LEFT JOIN XFC_PLT_MASTER_NatureCost 	MN ON MN.id = Q.account_type
									LEFT JOIN XFC_PLT_MASTER_AverageGroup 	MG ON MG.id = Q.id_averagegroup
									LEFT JOIN XFC_PLT_MASTER_Account		MA ON MA.id = Q.id_account
								
									GROUP BY 
 											  Q.scenario
											, Q.id_factory
											, MF.description
											, Q.month										
											, Q.id_product
											, MP.description
											, Q.id_component
											, Q.account_type
											, MN.description
											, Q.id_account
											, MA.description
											, Q.variability
											, Q.id_averagegroup
											, MG.description
									)
								
									SELECT {selectDetail}
									, SUM(VTU_Periodic) as VTU_Periodic
									, SUM(VTU_YTD) as VTU_YTD
									FROM finalVTU_Report
									WHERE 1=1
										{sAccountFilter}
										{sProductFilter}
									GROUP BY {selectDetail}
									HAVING SUM(VTU_YTD) <> 0
									ORDER BY {selectDetail}
									"
								#End Region
														
'								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'									dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'								End Using	
								brapi.ErrorLog.LogMessage(si, scenarioSub &": "& sql)
								dt = ExecuteSQLBIBlend(si, sql)
								
								If resultTable Is Nothing Then resultTable = dt.Clone()
								resultTable.Merge(dt)
								
								sw.Stop()
								
							Next
							
							If impactParity = "Yes" Then
								
								sqlVTU = GetSQLDataVTU(si, scenario, month, factory, year, currency, impactParity, scenarioRef)
							
								#Region "All VTUs"
								
								sql = $"
									
									{sqlVTU}
								
									, OwnRowsFilter AS (
										SELECT DISTINCT Month, id_product
										FROM OwnRows
										WHERE VTU_YTD <> 0
									)
									, finalVTU_Report as (
									/* --- QUERY --- */
									
									SELECT
											 -- Q.scenario
											 'Parity' as scenario
											, Q.id_factory
											, MF.description AS desc_factory
											, Q.month										
											, Q.id_product
											, MP.description AS desc_product	
											, Q.id_component
											, Q.account_type AS id_account_type
											, Q.account_type
											, Q.id_account
											, MA.description as desc_account
											, Q.variability
											, Q.id_averagegroup
											, MG.description as desc_averagegroup
										   	, SUM(CAST(COALESCE(Q.VTU_Periodic,0) 	AS DECIMAL(18,9))) AS VTU_Periodic
											, SUM(CAST(COALESCE(Q.VTU_YTD,0) 		AS DECIMAL(18,9))) AS VTU_YTD
								
									FROM (
									    SELECT * FROM OwnRows
									    UNION ALL
									    SELECT C.* 
										FROM CompRows C
										INNER JOIN OwnRowsFilter O
											ON C.month = O.month
											AND C.id_product = O.id_product
									) AS Q
								
									LEFT JOIN XFC_PLT_MASTER_Factory 		MF ON MF.id = Q.id_factory
									LEFT JOIN XFC_PLT_MASTER_Product 		MP ON MP.id = Q.id_product
									LEFT JOIN XFC_PLT_MASTER_NatureCost 	MN ON MN.id = Q.account_type
									LEFT JOIN XFC_PLT_MASTER_AverageGroup 	MG ON MG.id = Q.id_averagegroup
									LEFT JOIN XFC_PLT_MASTER_Account		MA ON MA.id = Q.id_account
								
									GROUP BY 
 											  Q.scenario
											, Q.id_factory
											, MF.description
											, Q.month										
											, Q.id_product
											, MP.description
											, Q.id_component
											, Q.account_type
											, Q.account_type
											, Q.id_account
											, MA.description
											, Q.variability
											, Q.id_averagegroup
											, MG.description
									)
								
									SELECT {selectDetail}
									, SUM(VTU_Periodic) as VTU_Periodic
									, SUM(VTU_YTD) as VTU_YTD
									FROM finalVTU_Report
									WHERE 1=1
										{sAccountFilter}
										{sProductFilter}
									GROUP BY {selectDetail}
									HAVING SUM(VTU_YTD) <> 0
									ORDER BY {selectDetail}
									"
								#End Region
								
								brapi.ErrorLog.LogMessage(si, "Impact Parity: "& sql)
															
								dt = ExecuteSQLBIBlend(si, sql)
'								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'									dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'								End Using	
								' Return dt
								resultTable.Merge(dt)
								
							End If
							
							Return resultTable
							
							#End Region
								
						'NOVA - WORKING
							
						#Region "VTU Nova - Working"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("AccountsDetail_Comp") Then
							
							sBiBlendQuery="Yes"							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								Dim scenarios() As String = {scenario, scenarioRef}
								
								For Each scenario_sel As String In scenarios
								
		'							Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables_NewTables", factory, month, year, scenario, "", "", "", "Existing", "", currency,"","","Old")							
									Dim sqlVTU As String = GetSQLDataVTU(si, scenario_sel, month, factory, year, currency)	
									
									#Region "Accounts Detail Comparative"
									
									sql = $"
										
										{sqlVTU}
									
										, finalVTU_Report as (
										/* --- QUERY --- */
										
										SELECT
												scenario
												, id_factory
												, FA.description AS desc_factory
												, month										
												, id_product
												, M.description AS desc_product	
												, id_account
											   	, SUM(CAST(COALESCE(VTU_Periodic,0) AS DECIMAL(18,9))) AS VTU_Periodic
												, SUM(CAST(COALESCE(VTU_YTD,0) AS DECIMAL(18,9))) AS VTU_YTD
									
										FROM (
										    SELECT * FROM OwnRows
										    UNION ALL
										    SELECT * FROM CompRows
										) AS Q
									
										LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
										LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
										LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
									
										GROUP BY scenario, id_factory, month, M.description, FA.description, id_product, id_account
									
										-- HAVING SUM(CAST(COALESCE(VTU_YTD,0) AS DECIMAL(18,9))) <> 0
									
										)
									
										SELECT *
										FROM finalVTU_Report
										WHERE 1=1
											AND VTU_YTD <> 0
											AND (id_factory = '{factory}' OR '{factory}'='All')
											AND month = {month}
										ORDER BY id_factory, month, id_product, id_account
										"
									
										' brapi.ErrorLog.LogMessage(si, sql)
									
										Dim resultTable As DataTable =  BRApi.Database.ExecuteSql(dbcon,sql,True)
										If dt Is Nothing Then dt = resultTable.Clone()
										dt.Merge(resultTable)								
									
									#End Region							
								
								Next
							
							End Using
						
							Return dt				
							
						Else If args.DataSetName.XFEqualsIgnoreCase("AccountsDetail_CC") Then
							
							sBiBlendQuery="Yes"							
							
							#Region "AccountsDetail CC"
							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								
								Dim items() As String = account.Replace("'","").Split(","c)
								' Dim sAccountList As String = String.Empty								
								' sAccountList = If(account = "All", "", "AND id_account IN( '" & String.Join("','", items.Select(Function(s) s.Trim()).ToArray()) & "' )")

								
								Dim sFactoryList() As String = If (factory = "All", {
																					    "R0045106",
																					    "R0483003",
																					    "R0529002",
																					    "R0548913",
																					    "R0548914",
																					    "R0585",
																					    "R0592",
																					    "R0671"
																						} _
																						, factory.Split(","c))								
								For Each sFactory As String In sFactoryList
									
									#Region "Fact VTU - Factory"
									Dim sqlDrop As String = $"
										DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_{sFactory};
										CREATE TABLE XFC_PLT_FACT_Costs_VTU_Report_{sFactory} (
											[scenario] varchar(50)  NOT NULL,
											[year] varchar(50)  NOT NULL,
											[id_factory] varchar(50)  NOT NULL,
											[date] date  NOT NULL,
											[id_product] varchar(50)  NOT NULL,
											[id_averagegroup] varchar(50)  NOT NULL,
											[account_type] varchar(50)  NOT NULL,
											[id_account] varchar(50)  NOT NULL,
											[cost_center] varchar(50)  NOT NULL,
											[cost_fixed] decimal(18,6),
											[cost_variable] decimal(18,6),
											[cost] decimal(18,6),
											[volume] decimal(18,6),
											[activity_UO1] decimal(18,6),
											[activity_UO1_total] decimal(18,6)
										)
									
									"
									
									Dim sql_VTU_data As String = shared_queries.AllQueries(si, "VTU_data_All_Factories_All_Months_AccountDetail", "", month, year, scenario, "", "", "YTD", "Existing", "", currency,)
									sql_VTU_data = sql_VTU_data & 
										$"
									
										INSERT INTO XFC_PLT_FACT_Costs_VTU_Report_{sFactory} 
											(
											scenario
											, year
											, id_factory
											, date
											, id_product
											, id_averagegroup
											, account_type
											, id_account
											, cost_center
											, cost_fixed
											, cost_variable
											, cost
											, volume
											, activity_UO1
											, activity_UO1_total) 
										
										SELECT
											'{scenario}' as scenario
											, YEAR(date) as year
											, id_factory
											, date
											, id_product
											, id_averagegroup
											, account_type
											, id_account
											, cost_center
											, cost_fixed
											, cost_variable
											, cost
											, volume
											, activity_UO1
											, activity_UO1_total 
									
										FROM factVTUCC
										WHERE id_factory = '{sFactory}'
										"
																	
									#End Region
									
'									BRAPI.ErrorLog.LogMessage(SI, sql_VTU_data)
									BRApi.Database.ExecuteSql(dbcon, sqlDrop,True)									
									BRApi.Database.ExecuteSql(dbcon,sql_VTU_data,True)
									
									#Region "VTU Calculation"
									
									Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables_CC", sFactory, month, year, scenario, "", "", "", "Existing", "", currency)
									sql = $"
									
										{sqlVTU}
										
										
										-- SELECT  {topSelect}
										-- 		scenario, id_factory, month, id_product, account_type, variability, id_account
										-- 	   	, SUM(VTU_Periodic) AS VTU_Periodic
										-- 		, SUM(VTU_YTD) AS VTU_YTD
										-- 
										-- FROM (
										--     SELECT * FROM OwnRows
										--     -- UNION ALL
										--     -- SELECT * FROM CompRows
										-- ) AS Q
										-- 
										-- WHERE 1=1

										-- 
										-- GROUP BY scenario, id_factory, month,
										--          id_product, account_type, variability, id_account
										-- 
										-- HAVING SUM(VTU_YTD) <> 0
										-- 
										-- ORDER BY id_factory, month, id_product, id_account;
									
										-- SELECT * FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months WHERE id_product = '100010673R'
										SELECT * FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months WHERE id_product = '8201728970'
									
										"
									
								#End Region
									
									' brapi.ErrorLog.LogMessage(si, sql)
								
									Dim resultTable As DataTable =  BRApi.Database.ExecuteSql(dbcon,sql,True)
									If dt Is Nothing Then dt = resultTable.Clone()
									dt.Merge(resultTable)
									
								Next
								
							End Using
							
							
							#End Region
							
							Return dt
						
						Else If args.DataSetName.XFEqualsIgnoreCase("AccountsDetail_GM") Then
							
							sBiBlendQuery="Yes"							
							
							#Region "AccountsDetail GM"
							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								
								Dim items() As String = account.Replace("'","").Split(","c)
								Dim sAccountList As String = String.Empty								
								sAccountList = If(account = "All", "", "AND id_account IN( '" & String.Join("','", items.Select(Function(s) s.Trim()).ToArray()) & "' )")

								
								Dim sFactoryList() As String = If (factory = "All", {
																					    "R0045106",
																					    "R0483003",
																					    "R0529002",
																					    "R0548913",
																					    "R0548914",
																					    "R0585",
																					    "R0592",
																					    "R0671"
																						} _
																						, factory.Split(","c))								
								For Each sFactory As String In sFactoryList
									
									#Region "Fact VTU - Factory"
									Dim sqlDrop As String = $"
										DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_{sFactory};
										CREATE TABLE XFC_PLT_FACT_Costs_VTU_Report_{sFactory} (
											[scenario] varchar(50)  NOT NULL,
											[year] varchar(50)  NOT NULL,
											[id_factory] varchar(50)  NOT NULL,
											[date] date  NOT NULL,
											[id_product] varchar(50)  NOT NULL,
											[id_averagegroup] varchar(50)  NOT NULL,
											[account_type] varchar(50)  NOT NULL,
											[id_account] varchar(50)  NOT NULL,
											--[cost_center] varchar(50)  NOT NULL,
											[cost_fixed] decimal(18,6),
											[cost_variable] decimal(18,6),
											[cost] decimal(18,6),
											[volume] decimal(18,6),
											[activity_UO1] decimal(18,6),
											[activity_UO1_total] decimal(18,6)
										)
									
									"
									
									Dim sql_VTU_data As String = shared_queries.AllQueries(si, "VTU_data_All_Factories_All_Months_AccountDetail", "", month, year, scenario, "", "", "YTD", "Existing", "", currency,)
									
									sql_VTU_data = sql_VTU_data & 
										$"
										INSERT INTO XFC_PLT_FACT_Costs_VTU_Report_{sFactory} 
											(
											scenario
											, year
											, id_factory
											, date
											, id_product
											, id_averagegroup
											, account_type
											, id_account
											--, cost_center
											, cost_fixed
											, cost_variable
											, cost
											, volume
											, activity_UO1
											, activity_UO1_total) 
										
										SELECT
											'{scenario}' as scenario
											, YEAR(date) as year
											, id_factory
											, date
											, id_product
											, id_averagegroup
											, account_type
											, id_account
											--, cost_center
											, cost_fixed
											, cost_variable
											, cost
											, volume
											, activity_UO1
											, activity_UO1_total 
									
										FROM factVTU
										WHERE id_factory = '{sFactory}'
										"
																	
									#End Region
									
									BRApi.Database.ExecuteSql(dbcon,sqlDrop,True)
									BRApi.Database.ExecuteSql(dbcon,sql_VTU_data,True)
									
									#Region "VTU Calculation"
									
									Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables", sFactory, month, year, scenario, "", "", "", "Existing", "", currency)
									sql = $"
									
										{sqlVTU}
										
										
										SELECT  {topSelect}
												scenario, id_factory, month, id_product, id_averagegroup, account_type, variability, id_account
											   	, SUM(VTU_Periodic) AS VTU_Periodic
												, SUM(VTU_YTD) AS VTU_YTD
									
										FROM (
										    SELECT * FROM OwnRows
										    UNION ALL
										    SELECT * FROM CompRows
										) AS Q
										
										WHERE 1=1
											{sAccountList}
											AND month = {month}
									
										GROUP BY scenario, id_factory, month,
										         id_product, id_averagegroup, account_type, variability, id_account
									
										-- HAVING SUM(VTU_YTD) <> 0
									
										ORDER BY id_factory, month, id_product, id_averagegroup, id_account;
									
										"
									
								#End Region
									
									' brapi.ErrorLog.LogMessage(si, sql)
								
									Dim resultTable As DataTable =  BRApi.Database.ExecuteSql(dbcon,sql,True)
									If dt Is Nothing Then dt = resultTable.Clone()
									dt.Merge(resultTable)
									
								Next
								
							End Using
							
							
							#End Region
							
							Return dt
							
						Else If args.DataSetName.XFEqualsIgnoreCase("ComponentsAccountDetail_REWORK") Then
							
							sBiBlendQuery="Yes"
							
							#Region "ComponentsAccountDetail - REWORK"	
							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
							
										#Region "Fact VTU - Factory"
							
										BRApi.Database.ExecuteSql(dbcon,
										$"
										DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_{factory};
										CREATE TABLE XFC_PLT_FACT_Costs_VTU_Report_{factory} (
											[id_factory] varchar(50)  NOT NULL,
											[date] date  NOT NULL,
											[id_product] varchar(50)  NOT NULL,
											[id_averagegroup] varchar(50)  NOT NULL,
											[account_type] varchar(50)  NOT NULL,
											[id_account] varchar(50)  NOT NULL,
											[cost_fixed] decimal(18,6),
											[cost_variable] decimal(18,6),
											[cost] decimal(18,6),
											[volume] decimal(18,6),
											[activity_UO1] decimal(18,6),
											[activity_UO1_total] decimal(18,6)
										)
							
									",True)
							
									Dim sql_VTU_data As String = shared_queries.AllQueries(si, "VTU_data_All_Factories_All_Months_AccountDetail", "", month, year, scenario, "", "", "YTD", "Existing", "", currency,)
									sql_VTU_data = sql_VTU_data & 
										$"
										INSERT INTO XFC_PLT_FACT_Costs_VTU_Report_{factory} 
											(id_factory
											, date
											, id_product
											, id_averagegroup
											, account_type
											, id_account
											, cost_fixed
											, cost_variable
											, cost
											, volume
											, activity_UO1
											, activity_UO1_total) 
								
										SELECT 
											id_factory
											, date
											, id_product
											, id_averagegroup
											, account_type
											, id_account
											, cost_fixed
											, cost_variable
											, cost
											, volume
											, activity_UO1
											, activity_UO1_total 
							
										FROM factVTU
										WHERE id_factory = '{factory}'
										"
								
'									brapi.ErrorLog.LogMessage(si, sql_VTU_data)	
									
							End Using
							
									#End Region
							
									Dim sqlMonth As String = String.Empty
									Dim sqlWith As String = "WITH dummy as (SELECT 1 as dummy) "
									Dim sqlSelect As String = String.Empty
									Dim sqlFinal As String = String.Empty
									Dim productMapping As String = String.eMPTY 
									
									
							
									For i As Integer = 1 To 12
								
										month = i.ToString
										productMapping = If(i<firsMonthFCST,
													$"
													LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
														ON P.id_component = PI.id_product
														AND PI.scenario = 'Actual'
													",
													$"
													LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
														ON P.id_component = PI.id_product
														AND PI.scenario = '{scenario}'
														AND PI.year = '{year}'
														AND P.id_factory = PI.id_factory")
								
								
										Dim sql_Product_Component_Recursivity As String =  shared_queries.AllQueries(si, "Product_Component_Recursivity", factory, month, year, scenario, "", "", "", "Existing", "", currency,)
								
										sqlMonth = $"	
										-- MES: {month}
										{sql_Product_Component_Recursivity}								
										, data AS (
							
										SELECT '{month}' as Month, id_product, id_component, desc_product, id_account, SUM(VTU_pond) as VTU 
											--'{month}' as Month, id_product, id_component, desc_product, level, exp_coefficient as VTU 
											-- SUM(cost) AS cost, SUM(VTU_pond_unit) AS VTU_Unit, exp_coefficient, SUM(VTU_pond) AS VTU
										FROM (
								
											-- VTU PRODUCTO						
								
										SELECT 
											'--' AS id_product
								    		,P.id AS id_component
											,M.description AS desc_product
											,F.id_account
											,1 AS Level
											,F.cost
											,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
												ELSE F.cost / F.volume
												* F.activity_UO1 / F.activity_UO1_total 
												END AS VTU_pond_unit
											,1 AS exp_coefficient
											,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
												ELSE F.cost / F.volume							
												* F.activity_UO1 / F.activity_UO1_total 
								  				END AS VTU_pond
								
										FROM (SELECT '{product}' AS Id) P
										LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_{factory} F
											ON P.id = F.id_product
											AND MONTH(F.date) = {month}
										LEFT JOIN XFC_PLT_MASTER_Product M
											ON F.id_product = M.id							
								
										UNION ALL
								
										-- VTU COMPONENTS
								
										SELECT 
											P.id_product 
											,P.id_component 
											,M.description AS desc_product	
											,F.id_account
											,P.Level
											,F.cost
											,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
												ELSE F.cost / F.volume								
												* F.activity_UO1 / F.activity_UO1_total 
												END AS VTU_pond_unit
											,P.exp_coefficient
											,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
												ELSE F.cost / F.volume 
												* F.activity_UO1 / F.activity_UO1_total 
												* P.exp_coefficient END AS VTU_pond
								
										FROM (	SELECT * 
												FROM Product_Component_Recursivity
												WHERE id_product_final = '{product}'
											) P
								
										{productMapping}
								
										LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_{factory} F
											ON F.id_product = ISNULL(PI.id_product_mapping, P.id_component)					
											AND MONTH(F.date) = {month}
								
										LEFT JOIN XFC_PLT_MASTER_Product M
											ON P.id_component = M.id	
								
										) AS RES
								
										GROUP BY
											id_product
											, id_component
											, desc_product
											, id_account
											--, level
											--, exp_coefficient
					
										)"
								
										sqlMonth = sqlMonth.Replace("Product_Component_Recursivity", $"Product_Component_Recursivity_M{month}")
										sqlMonth = sqlMonth.Replace("data", $"data_M{month}")
										sqlWith = $"
										{sqlWith}
										{sqlMonth}
										"
										sqlSelect = If (i<12,
														 $"
															{sqlSelect}
															SELECT *
															FROM data_M{month}												
															UNION ALL
												
													
														",
														$"												
															{sqlSelect}
												 			SELECT *
															FROM data_M{month}
														"
													)
								
									Next					
							
									sql = $"
										{sqlWith}
										, finalVTU as (
										{sqlSelect}
										)
										SELECT {topSelect}
								    		id_product,
								    		id_component,
								    		desc_product,
								    		id_account,
								    		[1] AS M01,
								    		[2] AS M02,
								    		[3] AS M03,
								    		[4] AS M04,
								    		[5] AS M05,
								    		[6] AS M06,
								    		[7] AS M07,
								    		[8] AS M08,
								    		[9] AS M09,
								    		[10] AS M10,
								    		[11] AS M11,
								    		[12] AS M12
										FROM (
								    		SELECT 
								        		id_product,
								        		id_component,
								        		desc_product,
								        		id_account,
								       	 		VTU,
								        		Month
								    		FROM finalVTU
										) AS source_table
										PIVOT (
								    	SUM(VTU)
								    	FOR Month IN (
								        	[1], [2], [3], [4], [5], [6], 
								       	 	[7], [8], [9], [10], [11], [12]
								    	)
									) AS pivot_table
									WHERE ISNULL(id_account, '') <> ''
									ORDER BY id_component, id_account asc
								;
								"
								' brapi.errorlog.LogMessage(si, sql)	
							#End Region
							

						Else If args.DataSetName.XFEqualsIgnoreCase("WorkingReport_Insert") Then
							
							sBiBlendQuery="Yes"							
							
							#Region "Insert New VTU Tables"	
							
							Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
								Dim sScenarioList() As String = {"Actual"} ' {"Actual", "RF06", "RF09"}
								Dim sFactoryList() As String = {"R0045106","R0483003", "R0529002","R0548913","R0548914", "R0585","R0592", "R0671"}
								
								Dim dicInsert As New Dictionary (Of String, List(Of String)) From {
									{"New_Local", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail_GM", "Local", 	"XFC_PLT_FACT_Costs_VTU_Final_Local"}}, _
									{"New_Eur", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail_GM", "EUR", 		"XFC_PLT_FACT_Costs_VTU_Final"}} _
									}
								
'									{"Old_Local", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail", 	"Local", 	"XFC_PLT_FACT_Costs_VTU_Report_Account_Local"}}, _
'									{"Old_Eur", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail", 	"EUR", 		"XFC_PLT_FACT_Costs_VTU_Report_Account"}}, _
'									{"New_Local", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail_GM", "Local", 	"XFC_PLT_FACT_Costs_VTU_Final_Local"}}, _
'									{"New_Eur", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail_GM", "EUR", 		"XFC_PLT_FACT_Costs_VTU_Final"}} _
								
								
								Dim sqlTruncate = "								
									-- TRUNCATE TABLE  XFC_PLT_FACT_Costs_VTU_Report_Account_Local
									-- TRUNCATE TABLE  XFC_PLT_FACT_Costs_VTU_Report_Account
									-- TRUNCATE TABLE  XFC_PLT_FACT_Costs_VTU_Final_Local
									-- TRUNCATE TABLE  XFC_PLT_FACT_Costs_VTU_Final							
								"
								Dim sqlDelete = "
								-- DELETE FROM XFC_PLT_FACT_Costs_VTU_Report_Account_Local 	WHERE scenario = 'Actual' AND MONTH(date) = 8 AND id_factory = 'R0671'
								-- DELETE FROM XFC_PLT_FACT_Costs_VTU_Report_Account 			WHERE scenario = 'Actual' AND MONTH(date) = 8 AND id_factory = 'R0671'
								DELETE FROM XFC_PLT_FACT_Costs_VTU_Final_Local	  			WHERE scenario = 'Actual' -- AND MONTH(date) = 9 AND id_factory = 'R0045106'
								DELETE FROM XFC_PLT_FACT_Costs_VTU_Final					WHERE scenario = 'Actual' -- AND MONTH(date) = 9 AND id_factory = 'R0045106'	
								
								"
								' BRApi.Database.ExecuteSql(dbcon,sqlTruncate,True)
								BRApi.Database.ExecuteSql(dbcon,sqlDelete,True)
								
								For Each sScenario As String In sScenarioList
									For Each sFactory As String In sFactoryList
										For Each key As String In dicInsert.Keys
											month = "12"
											Dim sql_VTU_data As String = shared_queries.AllQueries(si, dicInsert(key)(0), "", month, year, sScenario, "", "", "YTD", "Existing", "", dicInsert(key)(1),)
										
											sql_VTU_data = sql_VTU_data & 
												$"
												INSERT INTO {dicInsert(key)(2)}
													(
													scenario
													, year
													, id_factory
													, date
													, id_product
													, id_averagegroup
													, account_type
													, id_account
													, cost_fixed
													, cost_variable
													, cost
													, volume
													, activity_UO1
													, activity_UO1_total) 
												
												SELECT
													'{sScenario}' as scenario
													, YEAR(date) as year
													, id_factory
													, date
													, id_product
													, id_averagegroup
													, account_type
													, id_account
													, cost_fixed
													, cost_variable
													, cost
													, volume
													, activity_UO1
													, activity_UO1_total 
											
												FROM factVTU
												WHERE 1=1
													AND id_factory = '{sFactory}'
												"
												
												BRApi.Database.ExecuteSql(dbcon,sql_VTU_data,True)
'												Brapi.ErrorLog.LogMessage(si, sScenario & sFactory)
											
										' Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables", sFactory, month, year, scenario, "", "", "", "Existing", "", currency)
										Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables",, month, year, scenario, "", "", "", "Existing", "", currency)
										sql = $"
											SELECT '{sScenario}' as Scenario, '{sFactory}'  as Factory, '{key}' as TableInsert, 'Ok' AS Status
										"
										
										Dim resultTable As DataTable =  BRApi.Database.ExecuteSql(dbcon,sql,True)
										If dt Is Nothing Then dt = resultTable.Clone()
										dt.Merge(resultTable)
										
										Next
									Next
								Next	
								
							End Using
							
							#End Region 
							
							Return dt			
							
						Else If args.DataSetName.XFEqualsIgnoreCase("AllVTUsStored") Then
							
							sBiBlendQuery="Yes"							
							
							#Region "All VTUs Stored"
							
							Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_stored", factory, month, year, scenario, "", "", "", "Existing", "", currency,)
							
							sql = $"
								
								{sqlVTU}
							
								SELECT {topSelect}
										scenario
										, id_factory
										, FA.description AS desc_factory
										, month										
										, id_product
										, id_component
										, M.description AS desc_product							
							            , SUM(VTU_unit_per_fix) AS VTU_unit_per_fix
							            , SUM(VTU_unit_per_var) AS VTU_unit_per_var
							            , SUM(VTU_unit_ytd_fix) AS VTU_unit_ytd_fix
							            , SUM(VTU_unit_ytd_var) AS VTU_unit_ytd_var
							
								FROM VTU_Data Q
							
								LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
								LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
								LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
							
								WHERE id_product = '320106758R'
							
								GROUP BY scenario, id_factory, month, M.description, FA.description, id_product, id_component
							
								ORDER BY id_factory, month, id_product
							
								"
							
'								BRApi.ErrorLog.LogMessage(si, sql)

							#End Region		
							
						#End Region
							
						#End Region

						#Region "Production"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Activity") Then							
							
							#Region "Activity"																
							
							Dim GM_Function_All As String = If(GM_Function = "All","All","")
							GM_Function = "'" & GM_Function.Replace(",","','") & "'"
							
							Dim sql_ActivityIndex = shared_queries.AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioref, "All")	
					
							sql = sql_ActivityIndex & $"		
					
							SELECT {topSelect}
								A.id_factory, FA.description AS Factory, month, A.id_averagegroup, MGM.id_function as GM_function, time, Activity_TAJ, Activity_TSO
					
							FROM (
				
					
							SELECT 
								[Id Factory] AS id_factory
					         	, MONTH(A.date) AS month
					         	, [Id GM] AS id_averagegroup
					         	, Time
								, CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS Activity_TAJ							
								, CONVERT(DECIMAL(18,2),SUM([Activity TSO])) AS Activity_TSO
					
							FROM ActivityAll A      
					       
			             	WHERE YEAR(A.date) = {year} 
							AND MONTH(A.date) <= {month}    
							AND ([Id Factory] = '{factory}' OR '{factory}' = 'All')							
				    
				       	 	GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
				
							) AS A
				
					
							--LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
				         	--	ON A.id_averagegroup = X.id_averagegroup
				         	--	AND A.month = MONTH(X.date)
				         	--	AND A.id_factory = X.id_factory
				         	--	AND A.Time = X.uotype
				         	--	AND X.scenario = '{scenario}'
				         	--	AND X.scenario_ref = '{scenarioref.Replace("_PrevYear","")}'
					
							LEFT JOIN XFC_PLT_MASTER_Averagegroup MGM
								ON MGM.id = A.id_averagegroup
							
							LEFT JOIN XFC_PLT_MASTER_Factory FA
								ON FA.id = A.id_factory							
							
							WHERE (MGM.id_function IN ({GM_function}) OR '{GM_Function_All}' = 'All')																												
					
							ORDER BY A.id_factory, month
					
							"			
							
'							BRApi.ErrorLog.LogMessage(si, sql)
						
						#End Region			
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Activity_Reference") Then							
							
							#Region "Activity Reference"																
							
							Dim GM_Function_All As String = If(GM_Function = "All","All","")
							GM_Function = "'" & GM_Function.Replace(",","','") & "'"
							
							Dim sql_ActivityIndex = shared_queries.AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioref, "All")	
					
							sql = sql_ActivityIndex & $"		
					
							SELECT {topSelect}
								A.id_factory, FA.description AS Factory, month, A.id_product, PR.description AS desc_product, time, Volume, Activity_TAJ, Activity_TSO
					
							FROM (
				
					
							SELECT 
								[Id Factory] AS id_factory
					         	, MONTH(A.date) AS month
					         	, [Id Product] AS id_product							
					         	, Time
								, CONVERT(DECIMAL(18,2),SUM([Volume])) AS Volume
								, CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS Activity_TAJ							
								, CONVERT(DECIMAL(18,2),SUM([Activity TSO])) AS Activity_TSO
					
							FROM ActivityAll A      
					       
			             	WHERE YEAR(A.date) = {year} 
							AND MONTH(A.date) <= {month}    
							AND ([Id Factory] = '{factory}' OR '{factory}' = 'All')							
				    
				       	 	GROUP BY [Id Factory], MONTH(A.date), [Id Product], Time
				
							) AS A								
							
							LEFT JOIN XFC_PLT_MASTER_Factory FA
								ON FA.id = A.id_factory							
														
							LEFT JOIN XFC_PLT_MASTER_Product PR
								ON PR.id = A.id_product
					
							ORDER BY A.id_factory, month, id_product
					
							"			
							
'							BRApi.ErrorLog.LogMessage(si, sql)
						
						#End Region									
						
						Else If args.DataSetName.XFEqualsIgnoreCase("ActivityIndex") Then							
							
							#Region "ActivityIndex"																
							
							Dim sql_ActivityIndex = shared_queries.AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioref, "All")	

					
							sql = sql_ActivityIndex & $"		

					
							SELECT {topSelect}
								A.id_factory, month, A.id_averagegroup, MGM.id_function as gm_function, MGM.function_description, time, Activity_TSO, Activity_REF, ActivityIndex, X.value AS activity_index_corrected
					
							FROM (
				
					
							SELECT 
								[Id Factory] AS id_factory
					         	, MONTH(A.date) AS month
					         	, [Id GM] AS id_averagegroup
					         	, Time
								, CONVERT(DECIMAL(18,2),SUM([Activity TSO])) AS Activity_TSO
								, CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) AS Activity_REF
					         	, CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])),0) <> 0 THEN
					           		CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END * 100 AS ActivityIndex 
					        
					
							FROM ActivityAll A      
					       
			             	WHERE YEAR(A.date) = {year} 
							AND MONTH(A.date) <= {month}    
							AND ([Id Factory]='{factory}' OR '{factory}' ='All')
				    
				       	 	GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
				
							) AS A
				
					
							LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
				         		ON A.id_averagegroup = X.id_averagegroup
				         		AND A.month = MONTH(X.date)
				         		AND A.id_factory = X.id_factory
				         		AND A.Time = X.uotype
				         		AND X.scenario = '{scenario}'
				         		AND X.scenario_ref = '{scenarioref.Replace("_PrevYear","")}'
					
							LEFT JOIN XFC_PLT_MASTER_Averagegroup MGM
								ON MGM.id = A.id_averagegroup
															
							WHERE Time = 'UO1'							
						
					
							ORDER BY A.id_factory, month
					
							"				
							 brapi.ErrorLog.LogMessage(si, sql)
						#End Region
					
						Else If args.DataSetName.XFEqualsIgnoreCase("ActivityIndexGM") Then
							
							#Region "ActivityIndexGM"				

							' Dim i As Integer = 1
							Dim monthFilter As String = If(view = "Periodic", $"MONTH(A.date)={month}", $"MONTH(A.date)<={month}")
							
							Dim sql_ActivityIndex = shared_queries.AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioref, "All")	

							sql = sql_ActivityIndex & $"		

								, factActivity AS (
					
					        		SELECT 
											A.id_factory
											, month
											, A.id_averagegroup
											, id_product, time
											, Volume
											-- , Activity_TAJ
											, Allocation_TAJ
											-- , Activity_TSO
											, Allocation_TSO
											-- , Activity_TAJ_Ref
									
									FROM (
					
								        SELECT 
												[Id Factory] AS id_factory
								         		, MONTH(A.date) AS month										
								         		, [Id GM] AS id_averagegroup
												, [Id Product] AS id_product
								         		, Time
										        , CONVERT(DECIMAL(18,2),SUM(Volume)) AS Volume
			
										        , CASE WHEN SUM(Volume) <> 0 THEN
			    							        CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) / CONVERT(DECIMAL(18,2),SUM(Volume))
										        ELSE NULL
										        END AS Allocation_TAJ
			
										        , CASE WHEN SUM(Volume) <> 0 THEN
			    							        CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM(Volume))
										        ELSE NULL
										        END AS Allocation_TSO
										        , CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) AS Activity_TAJ_Ref
							
										        , CASE 
                									WHEN SUM(COALESCE([Activity TAJ Ref], 0)) <> 0 THEN
                								 		SUM(COALESCE([Activity TSO], 0)) * 100.0 / SUM(COALESCE([Activity TAJ Ref], 0)) 
                								    ELSE 0
										        END AS Activity_Index
								
								        
								        	FROM ActivityAll A      
								       
						             		WHERE YEAR(A.date) = {year} 
											AND {monthFilter}
										    AND ([Id Factory]='{factory}' OR '{factory}' ='All')
								
											-- AND [Id GM] = '{GM}'
											AND Time = 'UO1'
							    
							       	 		GROUP BY [Id Factory], MONTH(A.date), [Id GM], [Id Product], Time
									) AS A						
							
								--ORDER BY A.id_factory, month, id_product
					
							)
							
							SELECT {topSelect} 
									*,
									Activity_Index		
							FROM factActivity
							
						"															
				
					#End Region

						Else If args.DataSetName.XFEqualsIgnoreCase("ActivityIndexTopGM") Then
							
							#Region "ActivityIndexTopGM"	
							
							Dim monthFilter As String = If(view = "Periodic", $"MONTH(A.date)={month}", $"MONTH(A.date)<={month}")
							
							Dim sql_ActivityIndex = shared_queries.AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioref, "All")	
                       	
							sql = sql_ActivityIndex & $"
							
							
								, factActivity AS (
					
				        		SELECT A.id_factory, month, A.id_averagegroup, time, Volume, Activity_TAJ, Activity_TSO,  Activity_TAJ_Ref, A.Activity_Index
								
								FROM (
				
								        SELECT 
												[Id Factory] AS id_factory
								         		, MONTH(A.date) AS month										
								         		, [Id GM] AS id_averagegroup
								         		, Time
												, SUM(Volume) AS Volume
												, SUM(COALESCE([Activity TAJ], 0) ) AS Activity_TAJ
												, SUM(COALESCE([Activity TSO], 0) ) AS Activity_TSO 
												, SUM(COALESCE([Activity TAJ Ref], 0)) AS Activity_TAJ_Ref
												, CASE 
        											WHEN SUM(COALESCE([Activity TAJ Ref], 0)) <> 0 
        											THEN SUM(COALESCE([Activity TSO], 0)) * 100.0 / SUM(COALESCE([Activity TAJ Ref], 0)) 
        											ELSE 0
    											  END AS Activity_Index
								        
								        	FROM ActivityAll A      
								       
						             		WHERE YEAR(A.date) = {year} 
											AND {monthFilter}
											AND ([Id Factory]='{factory}' OR '{factory}' ='All')
											AND Time = 'UO1'
							    
							       	 		GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
							
											) AS A						
							)
					        
							, ranked AS (
								SELECT
									id_averagegroup,
							        Activity_TAJ,
							        Activity_TSO,
							        Activity_TAJ_Ref,
							        Activity_Index,
							        month,
									ROW_NUMBER() OVER (PARTITION BY month ORDER BY Activity_Index DESC) AS rn_desc,
									ROW_NUMBER() OVER (PARTITION BY month ORDER BY Activity_Index ASC) AS rn_asc
								FROM factActivity
							)
							
							SELECT {topSelect}
    							month AS Month,
    							id_averagegroup AS GM,
    							Activity_TAJ,
							    Activity_TSO,
							    Activity_TAJ_Ref,
    							Activity_Index
							FROM ranked
							WHERE rn_desc <= 10

							UNION ALL

							SELECT {topSelect}
    							month AS Month,
    							id_averagegroup AS GM,
    							Activity_TAJ,
    							Activity_TSO,
    							Activity_TAJ_Ref,
    							Activity_Index
							FROM ranked
							WHERE rn_asc <= 10

							ORDER BY month, Activity_Index DESC
							
						
						"	

'							brapi.ErrorLog.LogMessage(si, sql)
							#End Region							
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Nomenclature") Then
							
							#Region "Nomenclature"
							
							Dim selectedDate As String

							'JG
							month = If(month = 13, 12, month)
							
							' Si el mes es febrero, asigna 28 (para años no bisiestos) o 29 (para años bisiestos).
							If month = "2" Then
							    ' Comprobamos si el año es bisiesto
							    If DateTime.IsLeapYear(CInt(year)) Then
							        selectedDate = $"{year}-{month}-29"  ' Año bisiesto, 29 de febrero
							    Else
							        selectedDate = $"{year}-{month}-28"  ' Año no bisiesto, 28 de febrero
							    End If
							ElseIf month = "4" OrElse month = "6" OrElse month = "9" OrElse month = "11" Then
							    ' Para los meses con 30 días
							    selectedDate = $"{year}-{month}-30"
							Else
							    ' Para los demás meses (con 31 días)
							    selectedDate = $"{year}-{month}-31"
							End If
							
							' Comprobar si la fecha resultante es válida antes de continuar
							Try
							    Dim finalDate As DateTime = DateTime.ParseExact(selectedDate, "yyyy-MM-dd", Nothing)
							    ' La fecha es válida
							Catch ex As FormatException
							    ' La fecha no es válida, manejar el error según sea necesario
							    Console.WriteLine("La fecha no es válida: " & selectedDate)
							End Try

							
							Dim formattedDate As String
							
							If Not String.IsNullOrEmpty(selectedDate) Then
							    formattedDate = DateTime.Parse(selectedDate).ToString("yyyy-MM-dd") ' Convierte a yyyy-MM-dd
							Else
							    formattedDate = DateTime.Now.ToString("yyyy-MM-dd")
							End If
						
							sql = $"
							-- Verifica si el producto seleccionado es All							
							
							IF '{product}' = 'All'
							BEGIN
							    SELECT 'Select a Product' AS [Message];
							END
							ELSE
							BEGIN
							    -- 1) Definir variables para el producto y la fábrica
							    DECLARE @productID  NVARCHAR(50) = '{product}';
							    DECLARE @factoryID  NVARCHAR(50) = '{factory}';
							    DECLARE @isNew BIT;
							    DECLARE @hoy DATE;
							    
							-- 2) Usar la fecha proporcionada por el usuario, o GETDATE() si no se introduce ninguna fecha
						       SET @hoy = CASE 
						                      WHEN '{formattedDate}' = '' OR '{formattedDate}' IS NULL  -- Si no se proporciona ninguna fecha
						                      THEN CAST(GETDATE() AS DATE)  -- Usar la fecha actual
						                      WHEN ISDATE('{formattedDate}') = 1 
						                      THEN CAST('{formattedDate}' AS DATE)  -- Si la fecha es válida
						                      ELSE CAST(GETDATE() AS DATE)  -- Si la fecha no es válida, usar la fecha actual
						                  END;
							
							    -- 2) Obtener el valor de 'new_product' desde la tabla maestro
							    SELECT @isNew = new_product
							    FROM XFC_PLT_MASTER_Product
							    WHERE [id] = @productID;
							
							    -- 3) Según sea nuevo o existente, se llama a la función correspondiente
							    IF @isNew = 1
							    BEGIN
							        -- ========= PRODUCTO NUEVO =========
							        IF NOT EXISTS (
							            SELECT 1
							            FROM dbo.fn_Product_Nomenclature_New(@productID, @factoryID)
							        )
							        BEGIN
							            SELECT @productID + ' - No Nomenclature in New Products' AS [Message];
							        END
							        ELSE
							        BEGIN
							            SELECT *
							            FROM dbo.fn_Product_Nomenclature_New(@productID, @factoryID)
							            ORDER BY [Level];
							        END
							    END
							    ELSE
							    BEGIN
							        -- ========= PRODUCTO EXISTENTE =========
							        IF NOT EXISTS (
							            SELECT 1
							            FROM dbo.fn_Product_Nomenclature(@productID, @factoryID, @hoy)
							        )
							        BEGIN
							            SELECT @productID + ' - No Nomenclature in Master Data' AS [Message];
							        END
							        ELSE
							        BEGIN
							            SELECT *
							            FROM dbo.fn_Product_Nomenclature(@productID, @factoryID, @hoy)
							            ORDER BY [Level];
							        END
							    END
							END
							"
							
							sql = $"
								SELECT *
								FROM XFC_PLT_HIER_Nomenclature_Date
								WHERE 1=1
									{sFactoryFilter}
								"
							
							#End Region
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Volumes") Then

							#Region "Volumes"	
							
							sql = $"	
								SELECT {topSelect}
									scenario AS [Scenario]
						      		, F.id_factory AS [Id Factory]
									, M.description AS [Factory]
									, F.id_product AS [Id Product]
									, ISNULL(P.description,'No Description') AS [Desc Product]
									, P2.Level AS Status
									, P2.type AS Organ
									, YEAR(date) AS Year   
						          	, MONTH(date) AS Month 
									, uotype AS Uotype
						 	 		, SUM(volume) AS Volume 
						              
						       	FROM XFC_PLT_FACT_Production F
							
								LEFT JOIN XFC_PLT_MASTER_Factory M
						          	ON F.id_factory = M.id     							

								LEFT JOIN XFC_PLT_MASTER_Product P
						          	ON F.id_product = P.id   
							
								LEFT JOIN XFC_PLT_MASTER_Product_Factory P2
						          	ON F.id_product = P2.id_product
									AND F.id_factory = P2.id_factory
						
						       WHERE 1=1
									AND scenario = '{scenario}'
									AND YEAR(F.date) = {year}
						         	AND MONTH(F.date) <= {month}
									AND (F.id_factory = '{factory}' OR '{factory}' ='All')
									AND uotype = 'UO1'
						
						       GROUP BY
						       		scenario
									, F.id_factory
									, M.description
									, F.id_product
									, P.description
									, YEAR(date)
									, MONTH(date)
									, uotype
									, P2.Level
									, P2.type							
							
								ORDER BY F.scenario, F.id_factory, F.id_product, MONTH(F.date), uotype
							"	
							
'							BRApi.ErrorLog.LogMessage(si, sql)							
						
							#End Region
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Production_Dynamic_Report") Then							
							
							#Region "Production Dynamic Report"
							
'								BRApi.ErrorLog.LogMessage(si, "reportDetailProduction: " & args.NameValuePairs.GetValueOrDefault("reportDetailProduction",""))
							
								Dim selectDetailProduction As String = args.NameValuePairs.GetValueOrDefault("reportDetailProduction","scenario, scenario_ref, id_factory, desc_factory, month, id_product, desc_product, id_averagegroup, desc_averagegroup, organ_name, organ_type, family, index, volume, volume_Ref, activity_TAJ, activity_TSO, activity_TAJ_Ref").Replace(" ", "").Replace("index","[index]")
								Dim selectDetailFields As String = selectDetailProduction.Replace(",Volume","SUM(Volume) AS Volume").Replace(",Volume_Ref","SUM(Volume_Ref) AS [Volume Ref]").Replace(",Activity_TAJ","SUM(Activity_TAJ) AS [Activity TAJ]").Replace(",Activity_TSO","SUM(Activity_TSO) AS [Activity TSO]").Replace(",Activity_TAJ_Ref","SUM(Activity_TAJ_Ref) AS [Activity TAJ Ref]")
								Dim selectDetailGroupBy As String = selectDetailProduction.Replace(",Volume","").Replace(",Activity_TAJ","").Replace(",Activity_TSO","").Replace(",Activity_TAJ_Ref","")
								
								Dim GM_Function_All As String = If(GM_Function = "All","All","")
								GM_Function = "'" & GM_Function.Replace(",","','") & "'"
								
								Dim sql_ActivityIndex = shared_queries.AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioref, "All")	
						
								sql = sql_ActivityIndex & $"		
						
								, Production_Data AS (
								
									SELECT 
										'{scenario}' AS scenario
										, '{scenarioref.Replace("_PrevYear","")}' AS scenario_ref
										, A.[Id Factory] AS id_factory
										, FA.description AS desc_factory
										, MONTH(A.date) AS month
										, A.[Id Product] AS id_product	
										, PR.description AS desc_product
										, [Id GM] AS id_averagegroup
										, GM.description AS desc_averagegroup
										, PR.organ_name
										, PR.organ_type
										, PR.family
										, PR.[index] AS [index]
										, A.time AS UO_type
																				        								
										, CONVERT(DECIMAL(18,2),SUM([Volume])) AS Volume
										, CONVERT(DECIMAL(18,2),SUM([Volume Ref])) AS Volume_Ref
										, CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS Activity_TAJ							
										, CONVERT(DECIMAL(18,2),SUM([Activity TSO])) AS Activity_TSO
										, CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) AS Activity_TAJ_Ref
							
									FROM ActivityAll A      
									
									LEFT JOIN XFC_PLT_MASTER_Factory FA
										ON FA.id = A.[Id Factory]							
																
									LEFT JOIN XFC_PLT_MASTER_Product PR
										ON PR.id = A.[Id Product]
								
									LEFT JOIN XFC_PLT_MASTER_Averagegroup GM
										ON GM.id = A.[Id GM]								
							       
					             	WHERE YEAR(A.date) = {year} 
									AND MONTH(A.date) <= {month}    
									AND ([Id Factory] = '{factory}' OR '{factory}' = 'All')							
						    
						       	 	GROUP BY 
										A.[Id Factory]
										, FA.description 
										, MONTH(A.date)
										, A.[Id Product] 	
										, PR.description
										, [Id GM] 
										, GM.description
										, PR.organ_name
										, PR.organ_type
										, PR.family
										, PR.[index]
										, A.time
								
								)
								
								SELECT {topSelect}
									{selectDetailFields}
						
								FROM Production_Data						
								
								GROUP BY {selectDetailGroupBy}						
						
								"			
							
'								BRApi.ErrorLog.LogMessage(si, sql)
								
							#End Region									
							
						#End Region
	
						#Region "Costs"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Masses") Then
							
							#Region "Masses"	
							
							Dim items() As String = account.Replace("'","").Split(","c)
							Dim sAccountList As String = String.Empty								
							sAccountList = If(account = "All" Or account = "", "", "AND [Id Account] IN( '" & String.Join("','", items.Select(Function(s) s.Trim()).ToArray()) & "' )")
'						   	brapi.ErrorLog.LogMessage(si,"REF: "& scenarioref)
							Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis_All_Factories", factory, "12", year, scenario, scenarioref, "", "YTD", "", "", currency)	
							Dim sqlSelectFinal As String = String.Empty
							
							sql = sql_VarianceAnalysis & $",
								finalVarianceAnalysis AS (
							        SELECT 
							              A.id_factory AS [Id Factory]
							            , CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), A.month)), 2)) AS [Month]
							            , A.id_account_type AS [Id Nature]
							
							            , RIGHT('0' + CONVERT(VARCHAR(10), A.id_account_type), 2)
							              + CASE 
							                  WHEN NULLIF(LTRIM(RTRIM(ISNULL(N.description, ''))), '') IS NULL THEN ''
							                  ELSE ' - ' + N.description
							                END AS [Nature]
							
							            , A.id_account AS [Id Account]
							
							            , CONVERT(VARCHAR(50), A.id_account)
							              + CASE 
							                  WHEN NULLIF(LTRIM(RTRIM(ISNULL(M.description, ''))), '') IS NULL THEN ''
							                  ELSE ' - ' + M.description
							                END AS [Account]
							
							            , A.id_costcenter AS [Id Cost Center]
							            , A.nature AS [Nature Cost Center]
							            , A.variability AS [Variability]
							
							            , CASE 
							                WHEN SUM(A.ref_value) = 0 THEN 1
							                ELSE SUM(CAST(A.ref_value_adj_100 AS DECIMAL(18,6))) 
							                     / NULLIF(SUM(CAST(A.ref_value AS DECIMAL(18,6))), 0) 
							              END * 100 AS [Activity Index]
							
							            , SUM(A.ref_value)               * R2.rate AS [Reference]
							            , SUM(A.ref_value_adj_100)       * R2.rate AS [Ref 100 Adjusted]
							            , SUM(A.ref_value_adj_semi)      * R2.rate AS [Ref Semi Adjusted]
							            , SUM(A.ref_value_adj_semi)      * R2.rate AS [Ref Adjusted]
							
							            , (SUM(A.ref_value_adj_semi) * R2.rate)
							              + ((SUM(A.ref_value_adj_semi_parity) * R1.rate * R1.rate_exception))
							              - ((SUM(A.ref_value_adj_semi_parity) * R2.rate)) AS [Ref Adjusted {scenario} at Parity]
							
							            , SUM(A.act_value) * R1.rate AS [{scenario} at {scenario} Parity]
							
							        FROM (
							            SELECT 
							                  A.id_factory
							                , A.month
							                , A.year
							                , A.id_account_type
							                , A.id_account
							                , A.Nature
							                , A.id_costcenter
							                , A.costcenter
							                , A.variability
							                , SUM(A.act_value) AS act_value
							                , SUM(A.ref_value) AS ref_value
							                , SUM(A.ref_value_adj_100) AS ref_value_adj_100
							                , SUM(A.ref_value_adj_semi) AS ref_value_adj_semi
							                , SUM(A.ref_value_adj_semi * ISNULL(P.value, 1)) AS ref_value_adj_semi_parity
							            FROM ActRefFinalData A
							            LEFT JOIN ParityPercentages P
							              ON  A.id_factory = P.id_factory
							              AND A.id_account = P.id_account
							            GROUP BY 
							                  A.id_factory, A.month, A.year, A.id_account_type
							                , A.id_account, A.Nature, A.id_costcenter, A.costcenter
							                , A.variability
							        ) A
							        LEFT JOIN XFC_PLT_MASTER_NatureCost N
							          ON A.id_account_type = N.id
							        LEFT JOIN XFC_PLT_MASTER_Account M
							          ON A.id_account = M.id
							        LEFT JOIN fxRate R1
							          ON  A.id_factory = R1.id_factory
							          AND A.month     = R1.month
							        LEFT JOIN fxRateRef R2
							          ON  A.id_factory = R2.id_factory
							          AND A.month     = R2.month
							        WHERE (A.id_factory = '{factory}' OR '{factory}' = 'All')
							        GROUP BY 
							              A.id_factory, A.month, A.id_account_type, N.description
							            , A.id_account, A.id_costcenter, A.costcenter, A.nature
							            , M.description, R1.rate, R2.rate, R1.rate_exception
							            , A.variability
							    )
							    SELECT *
							    FROM finalVarianceAnalysis
								WHERE 1=1
								{sAccountList}
							
							;"
							#End Region
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Masses_Raw") Then
							
							#Region "Masses Raw"
							
							sql = $"
								DECLARE @ceco VARCHAR(10) = 'All';
								WITH massesRaw As (
									SELECT {topSelect}
										A.scenario																as [Scenario]
										, A.id_factory 															as [Factory]
										, 'M' + RIGHT('0' + CAST(MONTH(A.date) AS VARCHAR), 2) 					as [Month]
										, A.id_costcenter 														as [CC]
										, B.type 																as [CC Type]
										, B.nature 																as [CC Nature]
										, RIGHT('0' + C.account_type,2) 										as [Account Type Id]
										, RIGHT('0' + C.account_type,2) + ' - ' + ISNULL( D.description, '') 	as [Account Type Description]
										, A.id_account 															as [Account]
										, C.description 														as [Account Description]
										, A.id_averagegroup														as [GM]
										, A.id_product															as [Product]
										, SUM(A.value) 															as [Amount]
										-- , C.id_rubric as [OS Account]
									
									FROM XFC_PLT_FACT_CostsDistribution A
									
									LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
										ON A.id_costcenter = B.id
										AND A.id_factory = B.id_factory
										AND B.scenario = 'Actual'
										AND A.date BETWEEN B.start_date AND B.end_date
									
									LEFT JOIN XFC_PLT_MASTER_Account C
										ON A.id_account = C.id
									
									LEFT JOIN XFC_PLT_MASTER_NatureCost D
										ON C.account_type = D.id
									
									WHERE 1=1
										AND A.scenario = '{scenario}'
										AND YEAR(A.date) = {year}
										AND value <> 0
										{sFactoryFilter.Replace("id_factory", "A.id_factory")}
										{sAccountFilter.Replace("id_account", "A.id_account")}
										{sMonthFilter.Replace("date", "A.date")}
							
									GROUP BY
										A.scenario
									 	, A.id_factory 
									 	, MONTH(A.date)
									 	, A.id_account 
									 	, A.id_averagegroup
										, A.id_product
										, B.type
										, B.nature
										, C.description 
										, C.account_type 
										, D.description
									 	-- , C.id_rubric 
										, A.id_costcenter
								) 
							
								SELECT *
								FROM massesRaw
								ORDER BY Month asc
								
							"
							
							#End Region
							
						Else If args.DataSetName.XFEqualsIgnoreCase("CostDetail") Then							
													
							#Region "CostDetail"																							
												
							Dim sql_Rate = shared_queries.AllQueries(si, "RATE_All_Factories", "", month, year, scenario, "", "", "", "", "", currency)	
							
							sql = sql_Rate.Replace(", fxRateRef AS","WITH fxRateRef AS") & $"																						
								
									, CostData AS (
								    SELECT
								        FA.description AS [Factory],
										F.id_costcenter AS [Id_CostCenter],
								        M.account_type AS [Id_Nature],
								        RIGHT('0' + M.account_type,2) + '_' + ISNULL(N.Description, '') AS [Nature],
								        F.id_account AS [Id_MNG_Account],
								        F.id_account + '_' + ISNULL(M.Description, '') AS [MNG_Account],
										F.id_rubric AS [Id_Rubric],
								        CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(F.date))), 2)) AS Month,
								        YEAR(F.date) AS Year,
								        SUM(F.value) * R1.rate AS Value
							
								    FROM XFC_PLT_FACT_Costs F
								    LEFT JOIN XFC_PLT_MASTER_Account M
								        ON F.id_account = M.id  
								    LEFT JOIN XFC_PLT_MASTER_NatureCost N
								        ON M.account_type = N.id  
								    LEFT JOIN fxRate R1
								        ON MONTH(F.date) = R1.month
										AND F.id_factory = R1.id_factory
									LEFT JOIN XFC_PLT_Master_Factory FA
										ON F.id_factory = FA.id
							
								    WHERE 1=1
								        AND (F.id_factory='{factory}' OR '{factory}' ='All')
								        AND YEAR(F.date) = {year}
								        AND F.scenario = '{scenario}'
								        AND F.id_account <> 'Others' 
								        AND F.id_account <> '#'
								    GROUP BY FA.Description, F.id_costcenter, F.date, F.id_account, M.account_type, F.id_rubric, M.Description, N.Description, R1.rate
								)
								SELECT 
								    '{scenario}' as Scenario,
									Factory,
									[Id_CostCenter],
								    [Id_Nature],
								    [Nature],
								    [Id_MNG_Account],
								    [MNG_Account],
									[Id_Rubric],
								    Month,
								    Value AS [Value_Periodic],
								    -- Acumulado por año, centro de costos y cuenta
								    SUM(Value) OVER (
								        PARTITION BY [Id_CostCenter], [Id_Nature], [Nature], [Id_MNG_Account],  [MNG_Account], Year 
								        ORDER BY Month
								        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
								    ) AS [Value YTD]
								FROM CostData
								ORDER BY Factory, [Id_Nature], [Id_MNG_Account], Month;						
					
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)
							
							#End Region
							
						Else If args.DataSetName.XFEqualsIgnoreCase("VarianceAnalysis") Then
							
							#Region "VarianceAnalysis"	
							
							Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis_All_Factories", "", month, year, scenario, scenarioRef, "", "Periodic", "", "", currency)	
						
							sql = sql_VarianceAnalysis & $"
								
								SELECT
							
							       	id_factory AS [Id Factory]
									, F.description AS [Factory]
									, V.month AS [Month]
							       	, id_account_type AS [Id Cost Nature]
									, desc_account_type AS [Cost Nature]
							       	, variability AS [Variability]
									, CASE WHEN SUM([Reference]) <> 0 THEN SUM([Ref. 100% Adjusted]) / SUM([Reference]) * 100 END  [Activity Index]
							       	, SUM([Reference]) AS [Reference]
							       	, SUM([Ref. 100% Adjusted]) AS [Ref 100 Adjusted]
							       	, SUM([Absorption of Fixed Costs]) AS [Absorption of Fixed Costs]
							       	, SUM([Ref. Semi Adjusted]) AS [Ref Semi Adjusted]
							       	, SUM([Scope Change]) AS [Scope Change]
							       	, SUM([Ref. Adjusted]) AS [Ref Adjusted]
							       	, SUM([Impact Parity]) AS [Impact Parity]
							       	, SUM([Ref. Adjusted at Actual Parity]) AS [Ref Adjusted at Scenario Parity]
							       	, SUM([Price/Salary Trend]) AS [Price Salary Trend]
							       	, SUM([Price/Salary Trend AGS effect]) AS [Price Salary Trend AGS effect]
							       	, SUM([Price/Salary Trend Mix population Effect]) AS [Price Salary Trend Mix population Effect]
							       	, SUM([Price/Salary Trend Organization Effect]) AS [Price Salary Trend Organization Effect]
							       	, SUM([Price/Salary Trend Noria Effect]) AS [Price Salary Trend Noria Effect]
							       	, SUM([Price/Salary Trend Wages other]) AS [Price Salary Trend Wages other]
							       	, SUM([Price/Salary Trend Electricity Effect]) AS [Price Salary Trend Electricity Effect]
							       	, SUM([Price/Salary Trend Gaz Effect]) AS [Price Salary Trend Gaz Effect]
							       	, SUM([Price/Salary Trend Price variance other than Elec/Gaz]) AS [Price Salary Trend Price variance other than Elec Gaz]
							       	, SUM([MEC]) AS [MEC]
							       	, SUM([Product/Process Evolution]) AS [Product Process Evolution]
							       	, SUM([Excess Labour]) AS [Excess Labour]
							       	, SUM([Unemployement]) AS [Unemployement]
							       	, SUM([Productivity]) AS [Productivity]
							       	, SUM([Organization Productivity Effect]) AS [Organization Productivity Effect]
							       	, SUM([Hyper Competitiveness Productivity Effect]) AS [Hyper Competitiveness Productivity Effect]
							       	, SUM([COVID 19 Effect]) AS [COVID 19 Effect]
							       	, SUM([Extra Productivity Effect]) AS [Extra Productivity Effect]
							       	, SUM([GAP]) AS [GAP]
							       	, SUM([Actual at Actual Parity]) AS [Scenario at Scenario Parity]						
							
								FROM VarianceAnalysis V
							
								LEFT JOIN XFC_PLT_MASTER_Factory F
									ON V.id_factory = F.id
								
								WHERE id_factory = '{factory}' OR '{factory}' = 'All'
								
								GROUP BY 
									id_factory
									, F.description
									, V.month
						       		, id_account_type
									, desc_account_type
						       		, variability
									
								ORDER BY id_factory, V.month, id_account_type, variability
							"

							#End Region		
						
						Else If args.DataSetName.XFEqualsIgnoreCase("VarianceAnalysis_Detail") Then
							
							#Region "VarianceAnalysisDetail"
							
							Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis_All_Factories", "", "12", year, scenario, scenarioref, "", "YTD", "", "", currency)	
							sql = sql_VarianceAnalysis & $"
							       , VarianceAnalisysDetail as (
									SELECT 
										  A.month, A.id_factory
										, A.id_account_type, A.id_account, A.variability
										, A.id_averagegroup, A.id_costcenter, A.costcenter, A.nature
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi	
										, SUM(ref_value_adj_semi * ISNULL(P.value,1)) AS ref_value_adj_semi_parity
						   			FROM ActRefFinalData A
									LEFT JOIN ParityPercentages P
										ON A.id_factory = P.id_factory
										AND A.id_account = P.id_account
						   			GROUP BY 
										  A.month, A.id_factory
										, A.id_account_type, A.id_account, A.variability
										, A.id_averagegroup, A.id_costcenter, A.costcenter, A.nature
								)
							
								SELECT {topSelect} *
								FROM VarianceAnalisysDetail
								WHERE 1=1
									{sFactoryFilter}
									{sMonthMFilter}
									{sAccountFilter}
							"
							
							#End Region
							
						Else If args.DataSetName.XFEqualsIgnoreCase("ScopeChange") Then
							
							#Region "ScopeChange"
						
							sql = $"
							SELECT 
								MONTH(A.date) AS month
								, A.id_factory
								, A.cost_type
								, CASE WHEN A.cost_type = 1 THEN 'MOD'
								   WHEN A.cost_type = 2 THEN 'MOS' 
								   WHEN A.cost_type = 3 THEN 'FIP' 
								   WHEN A.cost_type = 4 THEN 'IT' 
								   WHEN A.cost_type = 5 THEN 'Amo' 						
								   ELSE A.cost_type END AS cost_type_desc
								, A.variability
								, A.id_indicator AS id_category
								, E.description AS desc_category
								, SUM(value) AS value
						
							FROM XFC_PLT_AUX_EffectsAnalysis A
							
							LEFT JOIN XFC_PLT_MASTER_Effects E		
								ON A.id_indicator = E.id
						
							WHERE 1=1
								AND (A.id_factory = '{factory}' OR '{factory}' = 'All')
								AND A.scenario = '{scenario}'
								AND A.ref_scenario = '{scenarioRef.Replace("_PrevYear","")}'
								AND YEAR(A.date) = {year}
								AND MONTH(A.date) = {month}
						
							GROUP BY 
							MONTH(A.date)
								, A.id_factory
								, A.cost_type
								, A.cost_type
								, A.variability
								, E.description
								, A.id_indicator 
						
							HAVING SUM(value) <> 0
						
							ORDER BY A.id_factory, MONTH(A.date), A.cost_type
								
							"
						
							#End Region
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Costs_Dynamic_Report") Then
							
							#Region "Costs Dynamic Report"	
							
							Dim selectDetailCosts As String = args.NameValuePairs.GetValueOrDefault("reportDetailCosts","scenario, scenario_ref, id_factory, desc_factory, month, id_costcenter, desc_costcenter, id_type_costcenter, desc_type_costcenter, nature_costcenter, id_account, desc_account, id_nature_cost, desc_nature_cost, id_averagegroup, desc_averagegroup, variability, reference, ref_100_adjusted, activity_index_pond, ref_semi_adjusted, ref_adjusted_scenario_parity, scenario_cost").Replace(" ", "")
														
							Dim selectDetailGroupBy As String = selectDetailCosts.Replace(",reference","") _
																.Replace(",ref_100_adjusted","") _
																.Replace(",activity_index_pond","") _
																.Replace(",ref_semi_adjusted","") _
																.Replace(",ref_adjusted_scenario_parity","") _
																.Replace(",scenario_cost","") 
																
							Dim selectDetailFields As String = selectDetailCosts.Replace(",reference",",SUM(reference) AS reference") _
																.Replace(",ref_100_adjusted",",SUM(ref_100_adjusted) AS ref_100_adjusted") _
																.Replace(",ref_semi_adjusted",",SUM(ref_semi_adjusted) AS ref_semi_adjusted") _
																.Replace(",ref_adjusted_scenario_parity",",SUM(ref_adjusted_scenario_parity) AS ref_adjusted_scenario_parity") _
																.Replace(",scenario_cost",",SUM(scenario_cost) AS scenario_cost") _
																.Replace(",activity_index_pond","
																			,CASE 
																				WHEN SUM(reference) = 0 THEN 1
																				ELSE SUM(CAST(ref_100_adjusted AS DECIMAL(18,6))) 
																					/ NULLIF(SUM(CAST(reference AS DECIMAL(18,6))), 0) 
																			END * 100 AS activity_index_pond		")
							
							Dim items() As String = account.Replace("'","").Split(","c)
							Dim sAccountList As String = String.Empty								
							sAccountList = If(account = "All" Or account = "", "", "AND id_account IN( '" & String.Join("','", items.Select(Function(s) s.Trim()).ToArray()) & "' )")
'						   	brapi.ErrorLog.LogMessage(si,"REF: "& scenarioref)
							Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis_All_Factories", factory, "12", year, scenario, scenarioref, "", "YTD", "", "", currency)	
							Dim sqlSelectFinal As String = String.Empty
							
							sql = sql_VarianceAnalysis & $",
								costs_data AS (
										SELECT 
											'{scenario}' AS scenario
											, '{scenarioref.Replace("_PrevYear","")}' AS scenario_ref
											, A.id_factory AS id_factory
											, F.description AS desc_factory
											, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), A.month)), 2)) AS month
											, A.id_costcenter AS id_costcenter
											, ISNULL(A.costcenter,'No Description') AS desc_costcenter
											, A.type AS id_type_costcenter
											, ISNULL(T.description,'No Description') AS desc_type_costcenter
											, A.nature AS nature_costcenter
											, A.id_account AS id_account
											, ISNULL(M.description, 'No Description') AS desc_account
											, A.id_account_type AS id_nature_cost
											, ISNULL(N.description, 'No Description') AS desc_nature_cost
											, A.id_averagegroup AS id_averagegroup
											, ISNULL(G.description, 'No Description') AS desc_averagegroup
											, A.variability AS variability
							
							                , SUM(A.act_value) 				* R1.rate AS scenario_cost
							                , SUM(A.ref_value)           	* R2.rate AS reference
							                , SUM(A.ref_value_adj_100)   	* R2.rate AS ref_100_adjusted
							                , SUM(A.ref_value_adj_semi)  	* R2.rate AS ref_semi_adjusted
							
											, (SUM(A.ref_value_adj_semi) * R2.rate)
							              		+ ((SUM(A.ref_value_adj_semi * ISNULL(P.value, 1)) * R1.rate * R1.rate_exception))
							              		- ((SUM(A.ref_value_adj_semi * ISNULL(P.value, 1)) * R2.rate)) AS ref_adjusted_scenario_parity
							
							            FROM ActRefFinalData A
							
							            LEFT JOIN ParityPercentages P
							              	ON  A.id_factory = P.id_factory
							              	AND A.id_account = P.id_account
							
										LEFT JOIN fxRate R1
								          	ON  A.id_factory = R1.id_factory
								          	AND A.month = R1.month
							
								        LEFT JOIN fxRateRef R2
								          	ON  A.id_factory = R2.id_factory
								          	AND A.month  = R2.month
							
								        LEFT JOIN XFC_PLT_MASTER_Factory F
								          	ON A.id_factory = F.id		
								
								        LEFT JOIN XFC_PLT_MASTER_NatureCost N
								          	ON A.id_account_type = N.id
								
								        LEFT JOIN XFC_PLT_MASTER_Account M
								         	ON A.id_account = M.id
								
								        LEFT JOIN XFC_PLT_MASTER_AverageGroup G
								         	ON A.id_averagegroup = G.id	
											AND A.id_factory = G.id_factory
								
								        LEFT JOIN XFC_PLT_MASTER_CostCenter_Types T
								         	ON A.type = T.id		
							
										WHERE (A.id_factory = '{factory}' OR '{factory}' = 'All')
							
							            GROUP BY 
							                  A.id_factory
											, F.description
							                , A.month
							                , A.year
							                , A.id_account_type
							                , A.id_account
							                , A.id_costcenter
							                , A.costcenter
							                , A.Nature
											, A.type							
											, A.id_averagegroup
							                , A.variability
											, T.description
											, M.description
											, N.description
											, G.description
											, R1.rate
											, R1.rate_exception
											, R2.rate
								)
							
								-- MAIN QUERY
							
						        SELECT {selectDetailFields}					
						
						        FROM costs_data
						
								WHERE 1=1
								{sAccountList}							
													        
						        GROUP BY {selectDetailGroupBy}
							
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)							
							
							#End Region			
							
						Else If args.DataSetName.XFEqualsIgnoreCase("StartupCostsDiffs") Then
							
							#Region "Startup Cost Diffs"
							
								Dim sql_Rate = shared_queries.AllQueries(si, "RATE_All_Factories", "", month, year, scenario, "", "", "", "", "", currency)	
							
								sql = sql_Rate.Replace(", fxRateRef AS","WITH fxRateRef AS") & $"																						
								
									, costs_data AS (
								
									    SELECT
									        FA.description AS [Factory]
											, YEAR(F.date) AS Year
											, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(F.date))), 2)) AS Month
									        , M.account_type AS [Id_Nature]
									        , RIGHT('0' + M.account_type,2) + '_' + ISNULL(N.Description, '') AS [Nature]
									        , SUM(F.value) * R1.rate AS [SAP Cost]
								
									    FROM XFC_PLT_FACT_Costs F
									
										LEFT JOIN XFC_PLT_Master_Factory FA
											ON F.id_factory = FA.id								
									
									    LEFT JOIN XFC_PLT_MASTER_Account M
									        ON F.id_account = M.id  
									
									    LEFT JOIN XFC_PLT_MASTER_NatureCost N
									        ON M.account_type = N.id  
									
									    LEFT JOIN fxRate R1
									        ON MONTH(F.date) = R1.month
											AND F.id_factory = R1.id_factory
	
									    WHERE 1=1
									        AND (F.id_factory='{factory}' OR '{factory}' ='All')
									        AND YEAR(F.date) = {year}
									        AND F.scenario = '{scenario}'
									        AND F.id_account <> 'Others' 
									        AND F.id_account <> '#'
											AND M.account_type IN (51, 52, 53, 54, 55)
								
									    GROUP BY 
											FA.description
											, YEAR(F.date)
											, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(F.date))), 2)) 
									        , M.account_type 
									        , RIGHT('0' + M.account_type,2) + '_' + ISNULL(N.Description, '') 	
											, R1.rate
									)
								
									, projects_data AS (
										
										SELECT Factory, Year, Month, Id_Nature, Nature
											, SUM([Project Cost Capitalisable]) AS [Project Cost Capitalisable]
											, SUM([Project Cost No Capitalisable]) AS [Project Cost No Capitalisable]
										FROM (
										
												SELECT
											        FA.description AS [Factory]
													, YEAR(P.date) AS Year
													, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(P.date))), 2)) AS Month
											        , N.id AS [Id_Nature]
											        , RIGHT('0' + N.id,2) + '_' + ISNULL(N.Description, '') AS [Nature]
											        , CASE WHEN capitalizable = 'Yes' THEN SUM(P.value) * R1.rate END AS [Project Cost Capitalisable]
													, CASE WHEN ISNULL(capitalizable,'') <> 'Yes' THEN SUM(P.value) * R1.rate END AS [Project Cost No Capitalisable]
										
											    FROM XFC_PLT_AUX_Projects P
										
												LEFT JOIN XFC_PLT_Master_Factory FA
													ON P.id_factory = FA.id																	
											
											    LEFT JOIN XFC_PLT_MASTER_NatureCost N
											        ON CASE WHEN P.cost_type IN ('1','2','3','4','5') THEN '5' + P.cost_type ELSE P.cost_type END = N.id  		
										
											    LEFT JOIN fxRate R1
											        ON MONTH(P.date) = R1.month
													AND P.id_factory = R1.id_factory								
												
											    WHERE 1=1
											        AND (P.id_factory='{factory}' OR '{factory}' ='All')
											        AND YEAR(P.date) = {year}
											        AND P.scenario = '{scenario}'
											        AND P.project_file = 'startup'	
													AND P.cost_type <> '#EMPTY#'
										
												GROUP BY 
													FA.description 
													, YEAR(P.date) 
													, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(P.date))), 2)) 
											        , N.id 
											        , RIGHT('0' + N.id,2) + '_' + ISNULL(N.Description, '')
													, R1.rate
													, capitalizable
								
											) AS RES
											GROUP BY Factory, Year, Month, Id_Nature, Nature
									)
								
								
									, final_query AS (
								
										SELECT C.*
											, ISNULL(P.[Project Cost Capitalisable],0) AS [Capitalisable]
											, ISNULL(P.[Project Cost No Capitalisable],0) AS [No Capitalisable]
											, ISNULL(P.[Project Cost Capitalisable],0) + ISNULL(P.[Project Cost No Capitalisable],0) AS [Total Project]
									
										FROM costs_data C
									
										LEFT OUTER JOIN projects_data P
											ON C.Factory = P.Factory
											AND C.Year = P.Year
											AND C.Month = P.Month
											AND C.Id_Nature = P.Id_Nature
											AND C.Nature = P.Nature
									)
								
									, query_temp AS (
								
										SELECT Factory, Nature, SUM([Project Cost Capitalisable]) AS [Project Cost Capitalisable]
										FROM projects_data
										GROUP BY Factory, Nature
									)
								
									SELECT * FROM final_query
					
							"
						
							#End Region	
																					
						Else If args.DataSetName.XFEqualsIgnoreCase("Hourly_Rate") Then
							
							#Region "Hourly Rate"								
							
							Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
							Dim sql_Rate = shared_queries.AllQueries(si, "RATE_All_Factories", "", month, year, scenario, "", "", "", "", "", currency)	
							
							sql = sql_DailyHours & sql_Rate & $"
												
								, factCosts AS (
							
									SELECT
										
										F.id_factory
          								, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(F.date)),2) as month
										, M.account_type AS id_nature
										, F.id_costcenter
										, SUM(F.value) * R.rate as value
										, SUM(F.value) AS value_for_coefficient
								
								    FROM XFC_PLT_FACT_Costs F
								    LEFT JOIN XFC_PLT_MASTER_Account M
								        ON F.id_account = M.id  				
								    LEFT JOIN XFC_PLT_MASTER_NatureCost N
								        ON M.account_type = N.id  	
									LEFT JOIN fxRate R
										ON MONTH(F.date) = R.month	
										AND F.id_factory = R.id_factory
									
									WHERE 1=1
										AND (F.id_factory = '{factory}' OR '{factory}' = 'All')
										AND YEAR(F.date) = {year}
										AND F.scenario = '{scenario}'
										AND M.account_type IN (1,2)
										AND F.id_account <> 'Others' 
									  	AND F.id_account <> '#'							
									
									GROUP BY F.id_factory, F.id_costcenter, F.date, F.id_account, M.account_type, N.Description, M.Description, R.rate
								)
							
								, dataCalculation AS (
										SELECT 
											F.id_factory
											, F.id_nature
											, SUM(F.value) AS costs
											, SUM(F.value_for_coefficient) AS costs_for_coefficient							
											, SUM(NULLIF(P.value,0)) AS paid_hours
											, SUM(F.value) / SUM(NULLIF(P.value,0)) AS value

									    FROM factCosts F										
								
										LEFT JOIN PaidHours P
											ON F.month = P.month
											AND F.id_nature = P.wf_type
											AND F.id_costcenter = P.id_costcenter
							
										GROUP BY F.id_factory, F.id_nature
								)
							
								, coefficient_rate AS (
										SELECT 
											id_factory
											, id_nature
											, costs / costs_for_coefficient AS coefficient

									    FROM dataCalculation																		
								)							
														
								SELECT factory, id_nature, nature, SUM(costs) AS costs, SUM(paid_hours) AS paid_hours, SUM(value_calc) AS value_calc, SUM(value_input) AS value_input
							
								FROM (
										SELECT
											FA.description AS factory
											, id_nature
											, CASE WHEN id_nature = 1 THEN 'MOD' ELSE 'MOS' END AS nature
											, SUM(costs) AS costs
											, SUM(paid_hours) AS paid_hours
											, SUM(value) AS value_calc
											, 0 AS value_input
										FROM dataCalculation D
										LEFT JOIN XFC_PLT_MASTER_Factory FA
								        	ON D.id_factory = FA.id  		
										GROUP BY FA.description, id_nature
								
										UNION ALL
								
										SELECT
											FA.description AS factory
											, I.id_nature
											, CASE WHEN I.id_nature = 1 THEN 'MOD' ELSE 'MOS' END AS nature
											, 0 AS costs
											, 0 AS paid_hours							
											, 0 AS value_calc
											, SUM(value) * coefficient AS value_input
							
										FROM XFC_PLT_AUX_HourlyRate_Input I
										LEFT JOIN coefficient_rate C
											ON I.id_nature = C.id_nature
											AND I.id_factory = C.id_factory
										LEFT JOIN XFC_PLT_MASTER_Factory FA
								        	ON I.id_factory = FA.id  									
										WHERE 1=1
										AND (I.id_factory = '{factory}' OR '{factory}' = 'All')
										AND I.id_factory <> ''
										AND year = {year}
										AND scenario = '{scenario}'
          								GROUP BY FA.description, I.id_nature, coefficient							
															
									) AS RES
							
								GROUP BY factory, id_nature, nature
							
									"
							

							#End Region							
							
						#End Region
							
						#Region "Energies"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("EnergyVariance") Then	
						    
							#Region "EnergyVariance"
							
							Dim i As Integer = 1
							
							Dim sql_EnergyVariance =  shared_queries.AllQueries(si, "EnergyVariance_All_Factories",,, year, scenario, scenarioRef, "All", view,,,currency)

							sql = sql_EnergyVariance & "						

								SELECT *
								FROM EnergyVariance
								ORDER BY month asc, Factory asc, Energy asc
							
							"																
						    #End Region
							
						#End Region			
						
						#Region "HHRR"											
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Hours_Hierarchy") Then
							
                          #Region "Hours Hierarchy"
						  
						  	Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
						  
							sql = sql_DailyHours & $"
							
								, AdditionalInfoReport AS (
										      SELECT DISTINCT id_factory, Month, wf_type, '99' AS orden, 'Surplus Workers/Unemployement' AS Indicator FROM Report
									UNION ALL SELECT DISTINCT id_factory, Month, wf_type, '991' AS orden, '.    Headcounts' AS Indicator FROM Report
									UNION ALL SELECT DISTINCT id_factory, Month, wf_type, '992' AS orden, '.    Costs' AS Indicator FROM Report
								)
								
								, AdditionalInfo AS (
								
									SELECT A.*, AI.value
									FROM AdditionalInfoReport A
									LEFT JOIN (
											SELECT id_factory, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) AS month, wf_type, '22' AS orden, surplus_unemployement_headcount AS value												
											FROM XFC_PLT_AUX_HRAdditionalInfo
											WHERE scenario = '{scenario}'
											AND YEAR(date) = {year}
											AND (UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR id_factory = '{factory}')
							
											UNION ALL
							
											SELECT id_factory, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) AS month, wf_type, '23' AS orden, surplus_unemployement_costs AS value												
											FROM XFC_PLT_AUX_HRAdditionalInfo
											WHERE scenario = '{scenario}'
											AND YEAR(date) = {year}
											AND (UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR id_factory = '{factory}')
								)	AI											
										ON A.id_factory = AI.id_factory
										AND A.month = AI.month
										AND A.wf_type = AI.wf_type
										AND A.orden = AI.orden
										
								)
							
								SELECT  R.Id_Factory, F.description as Factory, R.Month, R.wf_type, R.orden AS id, R.description2 AS Indicator, Value
							
								FROM (		
											SELECT DISTINCT id_factory, month, wf_type, id, description1, description2, orden
											FROM Report
								) R
								
								LEFT JOIN (	
											SELECT Id_Factory, wf_type, Month, Indicator, SUM(Value) AS Value 
											FROM AllHours 
											GROUP BY Id_Factory, Month, Indicator, wf_type
								) AS A
									ON R.id_factory = A.id_factory
									AND R.Month = A.Month
									AND R.wf_type = A.wf_type
									AND R.id = A.indicator
							
								INNER JOIN XFC_PLT_MASTER_Factory F
									ON R.id_factory = F.id
							
								UNION ALL
								
								SELECT  AI.Id_Factory, F.description as Factory, month, AI.wf_type, AI.orden AS id, AI.Indicator, AI.Value
								FROM AdditionalInfo AI
								
								INNER JOIN XFC_PLT_MASTER_Factory F
									ON AI.id_factory = F.id
								
								ORDER BY Id_Factory, Month, wf_type, orden
							"
							
'							BRApi.ErrorLog.LogMessage(si,sql)
							
							#End Region				

						Else If args.DataSetName.XFEqualsIgnoreCase("Hours_CC") Then
							
                          #Region "Hours CC"
						  
						  	Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
						  
							sql = sql_DailyHours & $"
						
								SELECT  R.Id_Factory, F.description as Factory, R.Month, R.wf_type, R.id_costcenter, R.type_costcenter, R.orden AS id, R.description1 AS Indicator, ISNULL(Value,0) AS Value 
							
								FROM (		
											SELECT DISTINCT id_factory, month, wf_type, id_costcenter, type_costcenter, id, description1, description2, orden 
											FROM Report
											WHERE (description1 = '{indicator}' OR '{indicator}' = 'All')
											AND orden <> 18 -- (-) Extra Hours																		
								) R
								
								LEFT JOIN (	
											SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(ISNULL(Value,0)) AS Value 
											FROM AllHours											
											GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
								) AS A
									ON R.id_factory = A.id_factory
									AND R.Month = A.Month
									AND R.wf_type = A.wf_type
									AND R.id = A.indicator
									AND R.id_costcenter = A.id_costcenter
									AND R.type_costcenter = A.type_costcenter															
							
								INNER JOIN XFC_PLT_MASTER_Factory F
									ON R.id_factory = F.id
							
								WHERE NULLIF(value,0) <> 0							
							
								ORDER BY R.Id_Factory, R.Month, R.wf_type, R.id_costcenter, R.orden
							"
							
'							BRApi.ErrorLog.LogMessage(si,sql)
							
							#End Region		
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Hours_CC_Only_VT") Then
							
                          #Region "Hours CC Only VT"
						  
						  	Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
						  
							sql = sql_DailyHours & $"
						
								SELECT  R.Id_Factory, F.description as Factory, R.Month, R.wf_type, R.id_costcenter, R.type_costcenter, R.orden AS id, R.description1 AS Indicator, ISNULL(Value,0) AS Value 
							
								FROM (		
											SELECT DISTINCT id_factory, month, wf_type, id_costcenter, type_costcenter, id, description1, description2, orden 
											FROM Report
											WHERE (description1 = '{indicator}' OR '{indicator}' = 'All')
											AND type_costcenter = 'A'
											AND orden <> 18 -- (-) Extra Hours																		
								) R
								
								LEFT JOIN (	
											SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(ISNULL(Value,0)) AS Value 
											FROM AllHours											
											GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
								) AS A
									ON R.id_factory = A.id_factory
									AND R.Month = A.Month
									AND R.wf_type = A.wf_type
									AND R.id = A.indicator
									AND R.id_costcenter = A.id_costcenter
									AND R.type_costcenter = A.type_costcenter															
							
								INNER JOIN XFC_PLT_MASTER_Factory F
									ON R.id_factory = F.id
							
								WHERE NULLIF(value,0) <> 0							
							
								ORDER BY R.Id_Factory, R.Month, R.wf_type, R.id_costcenter, R.orden
							"
							
							BRApi.ErrorLog.LogMessage(si,sql)
							
							#End Region										
		
						Else If args.DataSetName.XFEqualsIgnoreCase("OLD_Hours_ZCAL") Then
							
                          #Region "OLD - Hours ZCAL"
							sql = $"
								WITH DataHours AS (
							
									SELECT D.*, CA.id_indicator AS Type_Hour, type AS Type_Costcenter
									
									FROM XFC_PLT_AUX_DailyHours D
								
									INNER JOIN (
									            SELECT id, type, id_factory, start_date, end_date
									            FROM XFC_PLT_MASTER_CostCenter_Hist 
									            WHERE scenario = 'Actual'  
									            --AND type = 'A' 
												) AS C
								 		ON D.id_costcenter = C.id
										AND D.id_factory = C.id_factory
										AND DATEFROMPARTS({year},MONTH(D.date),1) BETWEEN start_date AND end_date
							
									LEFT JOIN XFC_PLT_AUX_Calendar CA
										ON CA.scenario = '{scenario}'
										AND CA.id_factory = D.id_factory
										AND CA.date = D.date
										AND CA.id_template = D.wf_type
								
								 	WHERE D.scenario = '{scenario}'
								 	AND YEAR(D.date) = {year}
									AND ( UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR D.id_factory = '{factory}' )							   
								)
							
								, StoredHours AS (
								
									-- STORED Data
							
								 	SELECT id_factory
										, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) AS Month
										, wf_type AS WF_Type
										, Type_Costcenter

										, CASE 
											
												-- Block 1
							
												WHEN id_indicator = 'daily_working_hours' 			THEN '1.1.  Heures Travail Théoriques (HTT)'
												
												-- Block 2
							
												WHEN id_indicator = 'unemployement' 				THEN '2.1.    (-) Chômage' 							
												WHEN id_indicator = '				' 				THEN '2.2.    (-) Férié' 	
												WHEN id_indicator = '				' 				THEN '2.3.    (-) Franchise' 	
												WHEN id_indicator = 'collective_leave'             	THEN '2.4.    (-) Congés collectif'	
							
												-- Block 3
							
												WHEN id_indicator = 'no_or_suspended_contract'		THEN '3.1.    (-) Hors activité'
												WHEN id_indicator = ' 				' 				THEN '3.2.    (-) Prêts internes'
												WHEN id_indicator = '				' 				THEN '3.3.    (+) Emprunts internes'
												WHEN id_indicator = '				' 				THEN '3.4.    (-) Prêts Externes'
												WHEN id_indicator = '				' 				THEN '3.5.    (+) Emprunts Externes'
												WHEN id_indicator = '				' 				THEN '3.6.    (-) Faisant fonction MOD vers MOS  (MOD)'
												WHEN id_indicator = '				' 				THEN '3.7.    (+) Faisant fonction MOD vers MOS  (MOS)'
												WHEN id_indicator = '				' 				THEN '3.8.    (+) Faisant fonction MOS vers MOD  (MOD)'
												WHEN id_indicator = '				' 				THEN '3.9.    (-) Faisant fonction MOS vers MOD  (MOS)'
							
												-- Block 4
							
												WHEN id_indicator = 'extra_hours' 					THEN '4.1.    (+) Heures supp' 
												WHEN id_indicator = '' 								THEN '4.2.    (+) Heures Supp démarrage'
												WHEN id_indicator = '' 								THEN '4.3.    (+) Heures Sup Mec'
												WHEN id_indicator = 'days_off' 						THEN '4.4.    (-) Congés individuels'
												WHEN id_indicator = 'strike' 						THEN '4.5.    (-) Grève'
												WHEN id_indicator = 'not_paid_non_working_hours' 	THEN '4.6.    (-) Heures non trav non payées' 
										
												-- Block 5
							
												WHEN id_indicator = 'sick_leave'					THEN '5.1.    (-) Maladie Accidents de travail' 							
							
												-- Block 6
							
												WHEN id_indicator = 'paid_non_working_hours' 		THEN '6.1.    (-) Heures non trav payées' 							
												WHEN id_indicator = 'labor_relation_hours' 			THEN '6.1.    (-) Heures non trav payées'							
												WHEN id_indicator = '' 								THEN '6.2.    (-) Démarrage' 								
 								
												-- Block 8
							
												WHEN id_indicator = 'training'						THEN '8.1.   Inclus Formation'	
							
												ELSE id_indicator
											END AS Indicator
							
										, CASE 							
												WHEN id_indicator IN ('daily_working_hours') THEN '1'
												WHEN id_indicator IN ('unemployement','collective_leave') THEN '2'
											   	WHEN id_indicator IN ('no_or_suspended_contract') THEN '3'
												WHEN id_indicator IN ('extra_hours','days_off','strike','not_paid_non_working_hours') THEN '4'
											   	WHEN id_indicator IN ('sick_leave') THEN '5'	
											   	WHEN id_indicator IN ('paid_non_working_hours','labor_relation_hours') THEN '6'									
												WHEN id_indicator IN ('training') THEN '8'
												ELSE '99' 
											END AS Block	
							
										, CASE WHEN id_indicator IN ('unemployement','collective_leave','no_or_suspended_contract','not_paid_non_working_hours','days_off','sick_leave','paid_non_working_hours','labor_relation_hours') THEN value * -1 
											   ELSE value 
											END AS Value
							
								    	FROM DataHours
							
								)
							
								-- CALCULATED Data

								, HeuresSuppDemarrageNegative AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '6.3.    (-) Heures Supp démarrage' AS Indicator 
										, SUM(value) * -1 AS Value
										, '6' AS Block
											
								    FROM StoredHours 
									WHERE indicator LIKE '%(+) Heures Supp démarrage' 
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)	
							
								, HeuresSuppNegative AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '7.1.    (-) Heures Supp' AS Indicator 
										, SUM(value) * -1 AS Value
										, '7' AS Block
											
								    FROM StoredHours 
									WHERE indicator LIKE '%(+) Heures Supp' 
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)							
							
								, AllHeuresStored AS (
							
												SELECT id_factory, Month, wf_type, Type_Costcenter, Indicator, Block, Value FROM StoredHours
									UNION ALL	SELECT id_factory, Month, wf_type, Type_Costcenter, Indicator, Block, Value FROM HeuresSuppDemarrageNegative		
									UNION ALL	SELECT id_factory, Month, wf_type, Type_Costcenter, Indicator, Block, Value FROM HeuresSuppNegative		
								)
							
								, HTTNettes AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '2T.  HTT nettes' AS Indicator 
										, SUM(value) AS Value
										, '2T' AS Block
											
								    FROM AllHeuresStored 
									WHERE Block IN ('1','2')
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)		
							
								, HeuresNecessaires AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '3T.  Heures Nécessaires' AS Indicator 
										, SUM(value) AS Value
										, '3T' AS Block
											
								    FROM AllHeuresStored 
									WHERE Block IN ('1','2','3')
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)		
							
								, HeuresPayeesGestion AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '4T.  Heures Payées Gestion' AS Indicator 
										, SUM(value) AS Value
										, '4T' AS Block
											
								    FROM AllHeuresStored 
									WHERE Block IN ('1','2','3','4')
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)								
							
								, HeuresHarbour AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '5T.  Heures Harbour' AS Indicator 
										, SUM(value) AS Value	
										, '5T' AS Block										
											
								    FROM AllHeuresStored 
									WHERE Block IN ('1','2','3','4','5')
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)
							
								, HeuresTravaillees AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '6T.  Heures Travaillées' AS Indicator 
										, SUM(value) AS Value
										, '6T' AS Block
											
								    FROM AllHeuresStored 
									WHERE Block IN ('1','2','3','4','5','6')
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)		
							
								, HeuresTravailleesHorsHeuresSup AS (
									SELECT id_factory, Month, wf_type, Type_Costcenter
										, '7T.  Heures Travaillées hors Heures Sup' AS Indicator 
										, SUM(value) AS Value
										, '7T' AS Block
											
								    FROM AllHeuresStored 
									WHERE Block IN ('1','2','3','4','5','6','7')
									GROUP BY id_factory, Month, wf_type, Type_Costcenter
								)																											

						
								SELECT  Id_Factory, description as Factory, wf_type, Month, Indicator, Type_Costcenter, Value
								FROM (
						
											  SELECT Id_Factory, wf_type, Month, Indicator, Type_Costcenter, SUM(Value) AS Value FROM AllHeuresStored GROUP BY Id_Factory, Month, Indicator, wf_type, Type_Costcenter
									UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, Type_Costcenter, SUM(Value) AS Value FROM HTTNettes GROUP BY Id_Factory, Month, Indicator, wf_type, Type_Costcenter
									UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, Type_Costcenter, SUM(Value) AS Value FROM HeuresNecessaires GROUP BY Id_Factory, Month, Indicator, wf_type, Type_Costcenter
									UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, Type_Costcenter, SUM(Value) AS Value FROM HeuresPayeesGestion GROUP BY Id_Factory, Month, Indicator, wf_type, Type_Costcenter
									UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, Type_Costcenter, SUM(Value) AS Value FROM HeuresHarbour GROUP BY Id_Factory, Month, Indicator, wf_type, Type_Costcenter
									UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, Type_Costcenter, SUM(Value) AS Value FROM HeuresTravaillees GROUP BY Id_Factory, Month, Indicator, wf_type, Type_Costcenter
									UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, Type_Costcenter, SUM(Value) AS Value FROM HeuresTravailleesHorsHeuresSup GROUP BY Id_Factory, Month, Indicator, wf_type, Type_Costcenter
							
								) AS A
							
								INNER JOIN XFC_PLT_MASTER_Factory F
									ON A.id_factory = F.id
							
								ORDER BY Id_Factory, Month, wf_type, Type_Costcenter, Indicator
							"
							#End Region	
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Hours") Then
							
                          #Region "OLD - Hours"
							sql = $"
							WITH DataHours AS (
							    SELECT 
							        D.*,
									FA.description AS Factory,
							        UPPER(LTRIM(RTRIM(D.id_indicator)))   AS Hour_Indicator,
							        CA.id_indicator AS Type_Hour,
							        CA.[value]      AS Type_Hour_Value
									
							    FROM XFC_PLT_AUX_DailyHours D
							    INNER JOIN (
							        SELECT id, [type], id_factory, start_date, end_date
							        FROM XFC_PLT_MASTER_CostCenter_Hist 
							        WHERE scenario = 'Actual'
							    ) AS C
							        ON  D.id_costcenter = C.id
							        AND D.id_factory    = C.id_factory
							        AND DATEFROMPARTS({year}, MONTH(D.[date]), 1) BETWEEN C.start_date AND C.end_date
							    LEFT JOIN XFC_PLT_AUX_Calendar CA
							        ON CA.scenario    = '{scenario}'
							       AND CA.id_factory  = D.id_factory
							       AND CA.[date]      = D.[date]
							       AND CA.id_template = D.wf_type
								LEFT JOIN XFC_PLT_MASTER_Factory FA
									ON FA.id = D.id_factory							
							    WHERE D.scenario = '{scenario}'
							      AND YEAR(D.[date]) = {year}
							      AND ( UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR D.id_factory = '{factory}' )
							),
							
							StoredHours AS (
							    SELECT 
							        id_factory,
									Factory,
							        'M' + RIGHT('0' + CONVERT(VARCHAR(2), MONTH([date])), 2) AS Month,
							        wf_type AS WF_Type,
							        Type_Hour,
							        CASE 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'DAILY_WORKING_HOURS'         						THEN 'Heures Travail Théoriques (HTT)'
							            WHEN WF_Type = 1 AND Hour_Indicator = 'COLLECTIVE_LEAVE'             						THEN 'Congés Collective'														
							            WHEN WF_Type = 1 AND Hour_Indicator = 'no_or_suspended_contract'       						THEN 'Hors activité'							
							            WHEN WF_Type = 1 AND Hour_Indicator = 'NOT_PAID_NON_WORKING_HOURS'  						THEN 'Heures non trav non payées' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'STRIKE'                       						THEN 'Grève' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'DAYS_OFF'                     						THEN 'Congés individuels'
							            WHEN WF_Type = 1 AND Hour_Indicator = 'EXTRA_HOURS'                  						THEN 'Heures supp' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'SICK_LEAVE'                   						THEN 'Maladie Accidents de travail'   
							            WHEN WF_Type = 1 AND Hour_Indicator IN ('PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS') 	THEN 'Heures non trav payées' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'UNEMPLOYEMENT'										THEN 'Chômage'
							            WHEN WF_Type = 1 AND Hour_Indicator = 'TRAINING'                     						THEN 'Inclus Formation'
							            WHEN WF_Type = 1                                                   							THEN 'Autres Heures'
							
							            WHEN WF_Type = 2 AND Hour_Indicator = 'DAILY_WORKING_HOURS'									THEN 'Heures Travail Théoriques (HTT)'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'COLLECTIVE_LEAVE'            						THEN 'Congés Collective'							
							            WHEN WF_Type = 2 AND Hour_Indicator = 'no_or_suspended_contract'       						THEN 'Hors activité'							
							            WHEN WF_Type = 2 AND Hour_Indicator = 'NOT_PAID_NON_WORKING_HOURS'  						THEN 'Heures non trav non payées' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'STRIKE'                      						THEN 'Grève' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'DAYS_OFF'                    						THEN 'Congés individuels'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'EXTRA_HOURS'                 						THEN 'Heures supp' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'SICK_LEAVE'                  						THEN 'Maladie Accidents de travail'  
							            WHEN WF_Type = 2 AND Hour_Indicator IN ('PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS') 	THEN 'Heures non trav payées' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'UNEMPLOYEMENT'										THEN 'Chômage'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'GIFT_CLOSING_HOURS'									THEN 'Ferie'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'TRAINING'                    						THEN 'Inclus Formation'						
							            WHEN WF_Type = 2                                                   							THEN 'Autres Heures'
							        END AS Indicator,
							
							        -- Grupos base para métricas derivadas
							        CASE 
							            WHEN Hour_Indicator IN ('DAILY_WORKING_HOURS','NOT_PAID_NON_WORKING_HOURS','STRIKE','DAYS_OFF','EXTRA_HOURS','COLLECTIVE_LEAVE', 'UNEMPLOYEMENT', 'GIFT_CLOSING_HOURS','no_or_suspended_contract')
							                THEN 1
							            WHEN Hour_Indicator = 'SICK_LEAVE'
							                THEN 2
							            WHEN Hour_Indicator IN ('PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS','DAILY_WORKING_HOURS')
							                THEN 3
							            WHEN Hour_Indicator = 'TRAINING'
							                THEN 5
							            ELSE 3
							        END AS BaseGroup,
							
							        -- Flag para horas extra
							        CASE WHEN Hour_Indicator = 'EXTRA_HOURS' THEN 1 ELSE 0 END AS IsExtraHours,
							
							        -- Valor con signo y ponderación
							        (CASE 
							            WHEN Hour_Indicator IN ('NOT_PAID_NON_WORKING_HOURS','DAYS_OFF','SICK_LEAVE','PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS','COLLECTIVE_LEAVE', 'UNEMPLOYEMENT','GIFT_CLOSING_HOURS','no_or_suspended_contract')
							                THEN [value] * -1 
							            ELSE [value] 
							         END) * COALESCE(Type_Hour_Value, 1) AS Value
							    FROM DataHours
							),
							
							-- Métricas calculadas (nombres sin prefijos)
							PaidHours AS (
							    SELECT id_factory, Factory, Month, WF_Type, Type_Hour,
							           'Heures Payées Gestion' AS Indicator,
							           Value
							    FROM StoredHours 
							    WHERE BaseGroup = 1
							),
							HarbourHours AS (
							    SELECT id_factory, Factory, Month, WF_Type, Type_Hour,
							           'Heures Harbour' AS Indicator,
							           Value
							    FROM StoredHours 
							    WHERE BaseGroup IN (1,2)
							),
							SuppDemarrageNegative AS (
							    SELECT id_factory, Factory, Month, WF_Type, Type_Hour,
							           'Heures Supp démarrage' AS Indicator,
							           Value * -1 AS Value
							    FROM StoredHours 
							    WHERE 1 = 0  -- Ajusta esta condición si existe indicador específico de démarrage
							),
							WorkedHours AS (
							    SELECT id_factory, Factory, Month, WF_Type, Type_Hour,
							           'Heures Travaillées' AS Indicator,
							           Value
							    FROM StoredHours 
							    WHERE BaseGroup IN (1,2,3)
							),
							ExtraHoursNegative AS (
							    SELECT id_factory, Factory, Month, WF_Type, Type_Hour,
							           'Heures supp' AS Indicator,
							           Value * -1 AS Value
							    FROM StoredHours 
							    WHERE IsExtraHours = 1
							),
							WorkedHoursWOSup AS (
							    SELECT id_factory, Factory, Month, WF_Type, Type_Hour,
							        'Heures Travaillées hors Heures Sup' AS Indicator,
							        Value
							    FROM StoredHours 
							    WHERE BaseGroup IN (1,2,3)
							)
							
							-- SALIDA
							SELECT 
							    Z.id_factory AS Id_Factory,
								Z.Factory,
							    Z.Month,
							    Z.WF_Type,
							    Z.Indicator,
							    CASE 
							      WHEN NULLIF(LTRIM(RTRIM(Z.Type_Hour)),'') IS NULL 
							           THEN 'No Calendar'
							      ELSE Z.Type_Hour
							    END AS Type_Hour,
							    FORMAT(Z.Value, 'N2', 'es-ES') AS Value
							FROM (
							    SELECT id_factory, Factory, Month, WF_Type, Indicator, Type_Hour, SUM(Value) AS Value
							    FROM StoredHours
							    GROUP BY id_factory, Factory, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM PaidHours
							    GROUP BY id_factory, Factory, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM HarbourHours
							    GROUP BY id_factory, Factory, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM WorkedHours
							    GROUP BY id_factory, Factory, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM WorkedHoursWOSup
							    GROUP BY id_factory, Factory, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM ExtraHoursNegative
							    GROUP BY id_factory, Factory, Month, WF_Type, Indicator, Type_Hour
							) Z
							WHERE Z.WF_Type IN (1,2)   -- Incluir WF_Type 1 y 2
							  AND (
							        '{indicator}' IN ('ALL','')
							        OR EXISTS (
							             SELECT 1
							             FROM STRING_SPLIT('{indicator}', ',') s
							             WHERE UPPER(Z.Indicator) LIKE '%' + UPPER(LTRIM(RTRIM(s.value))) + '%'
							           )
							      )
							ORDER BY Z.Month, Z.id_factory, Z.WF_Type, Z.Indicator;
							"
							#End Region		

						Else If args.DataSetName.XFEqualsIgnoreCase("Hours_VT") Then
							
                          #Region "OLD - Hours_VT"
							sql = $"
							WITH DataHours AS (
							    SELECT 
							        D.*,
									FA.description AS Factory,
							        UPPER(LTRIM(RTRIM(D.id_indicator)))   AS Hour_Indicator,
							        CA.id_indicator AS Type_Hour,
							        CA.[value]      AS Type_Hour_Value
									
							    FROM XFC_PLT_AUX_DailyHours D
							    INNER JOIN (
							        SELECT id, [type], id_factory, start_date, end_date
							        FROM XFC_PLT_MASTER_CostCenter_Hist 
							        WHERE scenario = 'Actual'
									AND type = 'A'
							    ) AS C
							        ON  D.id_costcenter = C.id
							        AND D.id_factory    = C.id_factory
							        AND DATEFROMPARTS({year}, MONTH(D.[date]), 1) BETWEEN C.start_date AND C.end_date
							    LEFT JOIN XFC_PLT_AUX_Calendar CA
							        ON CA.scenario    = '{scenario}'
							       AND CA.id_factory  = D.id_factory
							       AND CA.[date]      = D.[date]
							       AND CA.id_template = D.wf_type
								LEFT JOIN XFC_PLT_MASTER_Factory FA
									ON FA.id = D.id_factory							
							    WHERE D.scenario = '{scenario}'
							      AND YEAR(D.[date]) = {year}
							      AND ( UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR D.id_factory = '{factory}' )
							),
							
							StoredHours AS (
							    SELECT 
							        id_factory,
									Factory,
									id_costcenter,
							        'M' + RIGHT('0' + CONVERT(VARCHAR(2), MONTH([date])), 2) AS Month,
							        wf_type AS WF_Type,
							        Type_Hour,
							        CASE 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'DAILY_WORKING_HOURS'         						THEN 'Heures Nécessaires'
							            WHEN WF_Type = 1 AND Hour_Indicator = 'COLLECTIVE_LEAVE'             						THEN 'Congés Collective'														
							            WHEN WF_Type = 1 AND Hour_Indicator = 'no_or_suspended_contract'       						THEN 'Hors activité'							
							            WHEN WF_Type = 1 AND Hour_Indicator = 'NOT_PAID_NON_WORKING_HOURS'  						THEN 'Heures non trav non payées' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'STRIKE'                       						THEN 'Grève' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'DAYS_OFF'                     						THEN 'Congés individuels'
							            WHEN WF_Type = 1 AND Hour_Indicator = 'EXTRA_HOURS'                  						THEN 'Heures supp' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'SICK_LEAVE'                   						THEN 'Maladie Accidents de travail'   
							            WHEN WF_Type = 1 AND Hour_Indicator IN ('PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS') 	THEN 'Heures non trav payées' 
							            WHEN WF_Type = 1 AND Hour_Indicator = 'UNEMPLOYEMENT'										THEN 'Chômage'
							            WHEN WF_Type = 1 AND Hour_Indicator = 'TRAINING'                     						THEN 'Inclus Formation'
							            WHEN WF_Type = 1                                                   							THEN 'Autres Heures'
							
							            WHEN WF_Type = 2 AND Hour_Indicator = 'DAILY_WORKING_HOURS'									THEN 'Heures Nécessaires'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'COLLECTIVE_LEAVE'            						THEN 'Congés Collective'							
							            WHEN WF_Type = 2 AND Hour_Indicator = 'no_or_suspended_contract'       						THEN 'Hors activité'							
							            WHEN WF_Type = 2 AND Hour_Indicator = 'NOT_PAID_NON_WORKING_HOURS'  						THEN 'Heures non trav non payées' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'STRIKE'                      						THEN 'Grève' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'DAYS_OFF'                    						THEN 'Congés individuels'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'EXTRA_HOURS'                 						THEN 'Heures supp' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'SICK_LEAVE'                  						THEN 'Maladie Accidents de travail'  
							            WHEN WF_Type = 2 AND Hour_Indicator IN ('PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS') 	THEN 'Heures non trav payées' 
							            WHEN WF_Type = 2 AND Hour_Indicator = 'UNEMPLOYEMENT'										THEN 'Chômage'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'GIFT_CLOSING_HOURS'									THEN 'Ferie'
							            WHEN WF_Type = 2 AND Hour_Indicator = 'TRAINING'                    						THEN 'Inclus Formation'						
							            WHEN WF_Type = 2                                                   							THEN 'Autres Heures'
							        END AS Indicator,
							
							        -- Grupos base para métricas derivadas
							        CASE 
							            WHEN Hour_Indicator IN ('DAILY_WORKING_HOURS','NOT_PAID_NON_WORKING_HOURS','STRIKE','DAYS_OFF','EXTRA_HOURS','COLLECTIVE_LEAVE', 'UNEMPLOYEMENT','GIFT_CLOSING_HOURS','no_or_suspended_contract')
							                THEN 1
							            WHEN Hour_Indicator = 'SICK_LEAVE'
							                THEN 2
							            WHEN Hour_Indicator IN ('PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS','DAILY_WORKING_HOURS')
							                THEN 3
							            WHEN Hour_Indicator = 'TRAINING'
							                THEN 5
							            ELSE 3
							        END AS BaseGroup,
							
							        -- Flag para horas extra
							        CASE WHEN Hour_Indicator = 'EXTRA_HOURS' THEN 1 ELSE 0 END AS IsExtraHours,
							
							        -- Valor con signo y ponderación
							        (CASE 
							            WHEN Hour_Indicator IN ('NOT_PAID_NON_WORKING_HOURS','DAYS_OFF','SICK_LEAVE','PAID_NON_WORKING_HOURS','LABOR_RELATION_HOURS','COLLECTIVE_LEAVE', 'UNEMPLOYEMENT','GIFT_CLOSING_HOURS','no_or_suspended_contract') 
							                THEN [value] * -1 
							            ELSE [value] 
							         END) * COALESCE(Type_Hour_Value, 1) AS Value
							    FROM DataHours
							),
							
							-- Métricas calculadas (nombres sin prefijos)
							PaidHours AS (
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Type_Hour,
							           'Heures Payées Gestion' AS Indicator,
							           Value
							    FROM StoredHours 
							    WHERE BaseGroup = 1
							),
							HarbourHours AS (
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Type_Hour,
							           'Heures Harbour' AS Indicator,
							           Value
							    FROM StoredHours 
							    WHERE BaseGroup IN (1,2)
							),
							SuppDemarrageNegative AS (
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Type_Hour,
							           'Heures Supp démarrage' AS Indicator,
							           Value * -1 AS Value
							    FROM StoredHours 
							    WHERE 1 = 0  -- Ajusta esta condición si existe indicador específico de démarrage
							),
							WorkedHours AS (
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Type_Hour,
							           'Heures Travaillées' AS Indicator,
							           Value
							    FROM StoredHours 
							    WHERE BaseGroup IN (1,2,3)
							),
							ExtraHoursNegative AS (
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Type_Hour,
							           'Heures supp' AS Indicator,
							           Value * -1 AS Value
							    FROM StoredHours 
							    WHERE IsExtraHours = 1
							),
							WorkedHoursWOSup AS (
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Type_Hour,
							        'Heures Travaillées hors Heures Sup' AS Indicator,
							        Value
							    FROM StoredHours 
							    WHERE BaseGroup IN (1,2,3)
							)
							
							-- SALIDA
							SELECT 
							    Z.id_factory AS Id_Factory,
								Z.Factory,
								Z.id_costcenter,
							    Z.Month,
							    Z.WF_Type,
							    Z.Indicator,
							    CASE 
							      WHEN NULLIF(LTRIM(RTRIM(Z.Type_Hour)),'') IS NULL 
							           THEN 'No Calendar'
							      ELSE Z.Type_Hour
							    END AS Type_Hour,
							    FORMAT(Z.Value, 'N2', 'es-ES') AS Value
							FROM (
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour, SUM(Value) AS Value
							    FROM StoredHours
							    GROUP BY id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM PaidHours
							    GROUP BY id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM HarbourHours
							    GROUP BY id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM WorkedHours
							    GROUP BY id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM WorkedHoursWOSup
							    GROUP BY id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour
							
							    UNION ALL
							    SELECT id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour, SUM(Value)
							    FROM ExtraHoursNegative
							    GROUP BY id_factory, Factory, id_costcenter, Month, WF_Type, Indicator, Type_Hour
							) Z
							WHERE Z.WF_Type IN (1,2)   -- Incluir WF_Type 1 y 2
							  AND (
							        UPPER('{indicator}') = 'ALL'                                  -- sin filtro SOLO si es ALL
							        OR (                                                          -- si NO es ALL, debe filtrar
							            NULLIF(LTRIM(RTRIM('{indicator}')),'') IS NOT NULL
							            AND EXISTS (
							                  SELECT 1
							                  FROM STRING_SPLIT('{indicator}', ',') s
							                  WHERE LTRIM(RTRIM(s.value)) <> ''
							                    AND UPPER(Z.Indicator) LIKE '%' + UPPER(LTRIM(RTRIM(s.value))) + '%'
							            )
							        )
							      )
							ORDER BY Z.Month, Z.id_factory, Z.id_costcenter, Z.WF_Type, Z.Indicator;
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)	
							
							#End Region		
							
						Else If args.DataSetName.XFEqualsIgnoreCase("HRForCosting") Then	
							
						  #Region "OLD - HRForCosting"
								
							sql = $"
											
								WITH DailyHours AS (
								
								SELECT id_factory
								,  SUM(paid_hours_MOD) AS paid_hours_MOD
								, SUM(paid_hours_MOS) AS paid_hours_MOS
								FROM (
								
								SELECT  id_factory
								 , CASE WHEN wf_type = 1 THEN 
								 (ISNULL(daily_working_hours,0) + ISNULL(extra_hours,0))
								 - (ISNULL(training,0) + ISNULL(paid_non_working_hours,0) + ISNULL(sick_leave,0) + ISNULL(days_off,0))
								END AS paid_hours_MOD
								 , CASE WHEN wf_type = 2 THEN 
								  (ISNULL(daily_working_hours,0) + ISNULL(extra_hours,0))
								  - (ISNULL(training,0) + ISNULL(paid_non_working_hours,0) + ISNULL(sick_leave,0) + ISNULL(days_off,0))
								END AS paid_hours_MOS
								
								FROM (
								 SELECT D.id_factory, wf_type, id_indicator, value
								     FROM XFC_PLT_AUX_DailyHours D
								
								INNER JOIN (
								            SELECT id, id_factory
								            FROM XFC_PLT_MASTER_CostCenter_Hist 
								            WHERE scenario = 'Actual'  
								            AND type = 'A'
								            AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date ) AS C
								 ON D.id_costcenter = C.id
								AND D.id_factory = C.id_factory
								
								 WHERE scenario = '{scenario}'
								 AND YEAR(date) = {year}
								         AND MONTH(date) = {month}
								         AND id_indicator IN ('daily_working_hours','extra_hours','paid_non_working_hours','training','sick_leave','days_off')
								) AS SourceTable
								
								PIVOT (
								    SUM(Value)
								    FOR id_indicator IN ([daily_working_hours],[extra_hours],[paid_non_working_hours],[training],[sick_leave],[days_off])
								) AS PivotTable
								
								) AS RES
								
								GROUP BY id_factory
								)
								
								
								SELECT Factory
								, SUM(Activity_TAJ) AS Activity_TAJ
								, SUM(Paid_Hours_MOD) AS Paid_Hours_MOD
								, SUM(Paid_Hours_MOS) AS Paid_Hours_MOS
								, SUM(Headcount_MOS) AS Headcount_MOS
								
								FROM  (
								
								-- ACTIVITY
								
								SELECT 
									F.Description AS Factory
									, CONVERT(DECIMAL(18,2),P.Activity_TAJ) AS Activity_TAJ
									, 0 AS Paid_Hours_MOD
									, 0 AS Paid_Hours_MOS
									, 0 AS Headcount_MOS
								
								FROM XFC_PLT_FACT_Production P
								INNER JOIN XFC_PLT_MASTER_Factory F
								 ON P.id_factory = F.id
								
								WHERE scenario = '{scenario}'
								AND YEAR(P.date) = {year}
								AND MONTH(P.date) = {month}
								AND uotype = 'UO1'
								
								UNION ALL
								
								-- HOURS
								
								SELECT 
								 	F.Description AS Factory
								 	, 0 AS Activity_TAJ
								 	, Paid_Hours_MOD
								 	, Paid_Hours_MOS
								 	, 0 AS Headcount_MOS
								
								FROM DailyHours D
								INNER JOIN XFC_PLT_MASTER_Factory F
								 ON D.id_factory = F.id
								
								UNION ALL
								
								 SELECT
								  	F.Description AS Factory
								  	, 0 AS Activity_TAJ
								  	, 0 AS Paid_Hours_MOD
								 	, 0 AS Paid_Hours_MOS
								  	, value AS Headcount_MOS
								
								FROM XFC_PLT_AUX_Workforce W
								
								INNER JOIN (
								            SELECT id, id_factory
								            FROM XFC_PLT_MASTER_CostCenter_Hist 
								            WHERE scenario = 'Actual' 
								            AND type = 'A'
								            AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date ) AS C
								 ON W.id_costcenter = C.id
								AND W.id_factory = C.id_factory
								
								INNER JOIN XFC_PLT_MASTER_Factory F
								 ON W.id_factory = F.id
								
								 WHERE scenario = '{scenario}'
								 AND YEAR(date) = {year}
								 AND MONTH(date) = {month}
								 AND wf_type = 2
								
								) AS FIN
								
								GROUP BY Factory
								ORDER BY Factory


								
									"
							
							#End Region
						
						Else If args.DataSetName.XFEqualsIgnoreCase("HRforCosting-CostCenter") Then		
							
						  #Region "OLD - HRforCosting-CostCenter"
							
							sql = $"
											
								WITH DailyHours AS (
								
								SELECT id_factory
									, CostCenter
									, Type
									, SUM(paid_hours_MOD) AS paid_hours_MOD
									, SUM(paid_hours_MOS) AS paid_hours_MOS
								FROM (
								
								SELECT  id_factory
									, costcenter AS CostCenter
									, type AS Type
								 	, CASE WHEN wf_type = 1 THEN 
								 		(ISNULL(daily_working_hours,0) + ISNULL(extra_hours,0))
								 		- (ISNULL(training,0) + ISNULL(paid_non_working_hours,0) + ISNULL(sick_leave,0) + ISNULL(days_off,0))
									END AS paid_hours_MOD
								 	, CASE WHEN wf_type = 2 THEN 
								  		(ISNULL(daily_working_hours,0) + ISNULL(extra_hours,0))
								  		- (ISNULL(training,0) + ISNULL(paid_non_working_hours,0) + ISNULL(sick_leave,0) + ISNULL(days_off,0))
									END AS paid_hours_MOS
								
								FROM (
								 	SELECT D.id_factory, C.id as costcenter, C.type, wf_type, id_indicator, value
								    FROM XFC_PLT_AUX_DailyHours D
								
									INNER JOIN (
									            SELECT id, type, id_factory
									            FROM XFC_PLT_MASTER_CostCenter_Hist 
									            WHERE scenario = 'Actual'  
									            --AND type = 'A'
									            AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date ) AS C
								 		ON D.id_costcenter = C.id
										AND D.id_factory = C.id_factory
								
								 	WHERE scenario = '{scenario}'
								 	AND YEAR(date) = {year}
								 	AND MONTH(date) = {month}
								    AND id_indicator IN ('daily_working_hours','extra_hours','paid_non_working_hours','training','sick_leave','days_off')
								
								) AS SourceTable
								
								PIVOT (
								    SUM(Value)
								    FOR id_indicator IN ([daily_working_hours],[extra_hours],[paid_non_working_hours],[training],[sick_leave],[days_off])
								) AS PivotTable
								
								) AS RES
								
								GROUP BY id_factory, costcenter, type
								)
								
								
								SELECT Factory
								, CostCenter
								, Type
								, SUM(Activity_TAJ) AS Activity_TAJ
								, SUM(Paid_Hours_MOD) AS Paid_Hours_MOD
								, SUM(Paid_Hours_MOS) AS Paid_Hours_MOS
								, SUM(Headcount_MOS) AS Headcount_MOS
								
								FROM  (
								
								-- ACTIVITY
								
									SELECT 
										F.Description AS Factory
										, C.id AS CostCenter
										, C.type AS Type
										, CONVERT(DECIMAL(18,2),P.Activity_TAJ) AS Activity_TAJ
										, 0 AS Paid_Hours_MOD
										, 0 AS Paid_Hours_MOS
										, 0 AS Headcount_MOS
									
									FROM XFC_PLT_FACT_Production P
									INNER JOIN XFC_PLT_MASTER_Factory F
									 	ON P.id_factory = F.id
								
									LEFT JOIN (
									            SELECT id, type, id_factory
									            FROM XFC_PLT_MASTER_CostCenter_Hist 
									            WHERE scenario = 'Actual' 
									            --AND type = 'A'
									            AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date ) AS C
									 	ON P.id_costcenter = C.id
										AND P.id_factory = C.id_factory							
									
									WHERE scenario = '{scenario}'
									AND YEAR(P.date) = {year}
									AND MONTH(P.date) = {month}
									AND uotype = 'UO1'
								
								UNION ALL
								
								-- HOURS
								
									SELECT 
									 	F.Description AS Factory
										, costcenter AS CostCenter
										, type AS Type
									 	, 0 AS Activity_TAJ
									 	, Paid_Hours_MOD
									 	, Paid_Hours_MOS
									 	, 0 AS Headcount_MOS
									
									FROM DailyHours D
									INNER JOIN XFC_PLT_MASTER_Factory F
									 	ON D.id_factory = F.id
								
								UNION ALL
							
								-- HEADCOUNT
								
									 SELECT
									  	F.Description AS Factory
										, C.id AS CostCenter
										, C.type AS Type
									  	, 0 AS Activity_TAJ
									  	, 0 AS Paid_Hours_MOD
									 	, 0 AS Paid_Hours_MOS
									  	, value AS Headcount_MOS
									
									FROM XFC_PLT_AUX_Workforce W
									
									INNER JOIN (
									            SELECT id, type, id_factory
									            FROM XFC_PLT_MASTER_CostCenter_Hist 
									            WHERE scenario = 'Actual' 
									            --AND type = 'A'
									            AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date ) AS C
									 	ON W.id_costcenter = C.id
										AND W.id_factory = C.id_factory
									
									INNER JOIN XFC_PLT_MASTER_Factory F
									 	ON W.id_factory = F.id
									
									WHERE scenario = '{scenario}'
									AND YEAR(date) = {year}
									AND MONTH(date) = {month}
									AND wf_type = 2
								
								) AS FIN
								
								GROUP BY Factory, CostCenter, Type
								ORDER BY Factory, CostCenter, Type


								
									"
							#End Region									
														
						Else If args.DataSetName.XFEqualsIgnoreCase("ETP") Then
							
                          #Region "OLD - ETP"
							sql = $"
								SELECT 
									D.id_factory
									, F.description AS Factory
									, 'M' + RIGHT('0' + CONVERT(VARCHAR,D.month),2) AS month
									, D.wf_type
									, D.id_costcenter
									, type AS Type_Costcenter		
									, D.value AS daily_working_hours
									, CA.value AS calendar_days
									, T.value AS time_by_turn
									, D.value / NULLIF(CA.value,0) / NULLIF(T.value,0) AS ETP
																
								FROM 	  (	SELECT id_factory, YEAR(date) AS year, MONTH(date) AS month, wf_type, id_costcenter, SUM(value) AS value
											FROM XFC_PLT_AUX_DailyHours				 	
											WHERE scenario = '{scenario}'
											AND YEAR(date) = {year}
											AND ( UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR id_factory = '{factory}' )		
											AND id_indicator = 'daily_working_hours'				
											GROUP BY id_factory, YEAR(date), MONTH(date), wf_type, id_costcenter) D
							
								LEFT JOIN (	SELECT id_factory, MONTH(date) AS month, id_template, SUM(value) AS value 
											FROM XFC_PLT_AUX_Calendar
											WHERE scenario = '{scenario}'
											AND YEAR(date) = {year}
											AND id_indicator = 'JTNO'
											GROUP BY id_factory, MONTH(date), id_template) CA
									ON CA.id_factory = D.id_factory
									AND CA.month = D.month
									AND CA.id_template = D.wf_type
									
								LEFT JOIN XFC_PLT_Aux_TimePresence T
									ON T.scenario = '{scenario}'
									AND T.id_factory = D.id_factory
									AND YEAR(T.date) = D.year
									AND MONTH(T.date) = D.month
									AND T.wf_type = D.wf_type
									AND T.id_costcenter = D.id_costcenter
									
								LEFT JOIN XFC_PLT_MASTER_Factory F
									ON D.id_factory = F.id
									
								LEFT JOIN (
											SELECT id, type, id_factory, start_date, end_date
											FROM XFC_PLT_MASTER_CostCenter_Hist 
											WHERE scenario = 'Actual'  
											--AND type = 'A' 
											) AS C
									ON D.id_costcenter = C.id
									AND D.id_factory = C.id_factory
									AND DATEFROMPARTS({year},D.month,1) BETWEEN start_date AND end_date	
							
								ORDER BY D.id_factory, D.month, D.wf_type, D.id_costcenter							
							"
							#End Region		
							
						Else If args.DataSetName.XFEqualsIgnoreCase("FTE") Then
							
                          #Region "FTE"
						  
						  	Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHoursShift",factory,, year, scenario, "", "", "",,,"")
						  
							sql = sql_DailyHours & $"				  
						  				
								, calendar AS (
							
									SELECT id_factory, month, wf_type, shift, id_costcenter, SUM(value) AS value FROM (
							
										SELECT 
											id_factory
											, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) AS month
											, id_template AS wf_type
											, CASE WHEN id_indicator = 'JTNO' THEN '2x8 B' ELSE 'Weekend' END AS shift
											, id_costcenter
											, value AS value 
										FROM XFC_PLT_AUX_Calendar
										WHERE scenario = '{scenario}'
										AND YEAR(date) = {year}
										AND id_indicator IN ('JTNO','JNTA_WE')
							
									) C
									GROUP BY id_factory, month, wf_type, shift, id_costcenter
							
								)
							
								, time_presence AS (
								
									SELECT 
										id_factory
										, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) AS month
										, wf_type
										, id_costcenter
										, shift
										, value
									FROM XFC_PLT_Aux_TimePresence
									WHERE scenario = '{scenario}'
									AND YEAR(date) = {year}
								)
							
								SELECT 
									F.description AS Factory
									, D.month AS Month
									, D.wf_type AS [Wf Type]
									, D.id_costcenter AS [Id Cost Center]
									, D.Indicator AS [Hours Indicator]
									, D.shift AS Shift
									, C.type AS [Type Cost Center]		
									, D.value AS [Hours]									
									, CA.value AS [Worked Days]									
									, T.value AS [Hours by Shift]
									, D.value / NULLIF(CA.value,0) / NULLIF(T.value,0) AS FTE
																
								FROM 	  (	SELECT id_factory, month, wf_type, id_costcenter, Indicator, shift, SUM(value) AS value
											FROM AllHours				 		
											WHERE Indicator <> 'extra_hours_negative'
											GROUP BY id_factory, month, wf_type, id_costcenter, Indicator, shift) D
							
								LEFT JOIN calendar CA
									ON CA.id_factory = D.id_factory
									AND CA.month = D.month
									AND CA.wf_type = D.wf_type
									AND CA.shift = D.shift
									AND CA.id_costcenter = D.id_costcenter
									
								LEFT JOIN time_presence T
									ON T.id_factory = D.id_factory
									AND T.month = D.month
									AND T.wf_type = D.wf_type
									AND T.id_costcenter = D.id_costcenter
									AND T.shift = D.shift
									
								LEFT JOIN XFC_PLT_MASTER_Factory F
									ON D.id_factory = F.id
									
								LEFT JOIN (
											SELECT id, type, id_factory, start_date, end_date
											FROM XFC_PLT_MASTER_CostCenter_Hist 
											WHERE scenario = 'Actual'  
											--AND type = 'A' 
											) AS C
									ON D.id_costcenter = C.id
									AND D.id_factory = C.id_factory
									AND DATEFROMPARTS({year},REPLACE(D.month,'M',''),1) BETWEEN start_date AND end_date	
							
								ORDER BY F.Description, D.month, D.wf_type, D.Indicator, D.id_costcenter						
							"
							
'							BRApi.ErrorLog.LogMessage(si, sql)
							
							#End Region										
							
						#End Region
						
						#Region "Masters"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_CostCenter") Then
							sql = $"
							SELECT {topSelect} f.description as Factory, c.* 
							FROM XFC_PLT_MASTER_CostCenter_Hist c
							JOIN XFC_PLT_MASTER_Factory f
							   ON c.id_factory = f.id
							WHERE 1=1
								{sFactoryFilter}
								AND c.scenario = '{scenario}'
								AND ('{month}'= 13 OR ('{month}' BETWEEN MONTH(c.start_date) AND MONTH(c.end_date)))
								AND '{year}' BETWEEN YEAR(c.start_date) AND YEAR(c.end_date)
							"
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_GroupMoyene") Then
							sql = $"
							SELECT {topSelect} * 
							FROM XFC_PLT_MASTER_AverageGroup
							WHERE 1=1
								{sFactoryFilter}
							"
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_Accounts") Then
							sql = $"
							SELECT {topSelect} a.*, n.description as AccountTypeDescription 
							FROM XFC_PLT_MASTER_Account a
							JOIN XFC_PLT_MASTER_NatureCost n
							    ON a.account_type = n.id
							"							
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_FixedVariable") Then
							sql = $"
							SELECT {topSelect} * 
							FROM XFC_PLT_AUX_FixedVariableCosts
							WHERE 1=1
								AND scenario = '{scenario}' 
								{sFactoryFilter}
								AND ('{month}'= 13 OR '{month}' = MONTH(date))
								AND '{year}' = YEAR(date)
							"							
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_Nomenclature_Date") Then
							sql = $"
							SELECT {topSelect} * 
							FROM XFC_PLT_HIER_Nomenclature_Date 
							WHERE 1=1
								{sFactoryFilter}
								{sProductFilter}
								AND ('{month}'= 13 OR ('{month}' BETWEEN MONTH(start_date) AND MONTH(end_date)))
								AND '{year}' BETWEEN YEAR(start_date) AND YEAR(end_date)
							"
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_Nomenclature_Date_Report") Then
							sql = $"
							SELECT {topSelect} * 
							FROM XFC_PLT_HIER_Nomenclature_Date_Report 
							WHERE 1=1
								{sFactoryFilter}
								{sProductFilter}
								AND scenario = '{scenario}'
								AND ('{month}'='All' OR '{month}' = MONTH(date))
								AND '{year}' = YEAR(date)
							"
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_Product") Then
							sql = $"
							SELECT {topSelect} * 
							FROM XFC_PLT_MASTER_Product p
							JOIN XFC_PLT_MASTER_Product_Factory f
							   ON p.id = f.id_product
							WHERE 1=1
								{sFactoryFilter}
							"
						Else If args.DataSetName.XFEqualsIgnoreCase("Master_Allocation") Then
							sql = $"
							SELECT {topSelect} *  
							FROM XFC_PLT_AUX_AllocationKeys
							WHERE 1=1
								AND scenario = '{scenario}' 
								{sFactoryFilter}
								AND ('{month}'= 13 OR '{month}' = MONTH(date))
								AND '{year}' = YEAR(date)
							"
						#End Region
						
						#Region "All Files Info"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("FileInfo") Then
							
							Dim dtFile = New DataTable()
							
							Dim datasetName As String = args.NameValuePairs.GetValueOrDefault("dataSetName", "dataSetName")
							
							Dim infoDictionary As New Dictionary(Of String, List(Of String)) From {
								{"dataSetName"			, New List(Of String) From {"File Filters", "File Details"}},
								{"AllVTUs"				, New List(Of String) From {
													"Scenario, Factory, Year, Currency, Month" _
													, "Scenario, Id Factory, Desc Factory, Month, Id Product, Desc Product, VTU Periodic, VTU YTD"}},
								{"NatureVaribility"		, New List(Of String) From {
													"Scenario, Factory, Month, Year, Currency" _
													, "Scenario, Id Factory, Desc Factory, Month, Id Product, Desc Product, Nature, Variability, VTU Periodic, VTU YTD"}},
							    {"NatureGM"				, New List(Of String) From {
													"Scenario, Factory, Month, Year, Currency" _
													, "Scenario, Id Factory, Desc factory, Month, Id Product, Desc Product, Id Averagegroup, Nature, Variability, VTU Periodic, VTU YTD"}},
								{"AccountsDetail"		, New List(Of String) From {
													"Scenario, Factory, Month, Year, Currency" _
													, "Scenario, Id Factory, Month, Id Product, Account_Type, Variability, Id Account, VTU Periodic, VTU YTD"}},
								{"AccountsDetail_GM"		, New List(Of String) From {
													"Scenario, Factory, Month, Year, Currency" _
													, "Scenario, Id Factory, Month, Id Product, Id GM, Account_Type, Variability, Id Account, VTU Periodic, VTU YTD"}},													
								{"ComponentsAccountDetail", New List(Of String) From {
													"Scenario, Factory, Month, Year, Currency" _
													, "Scenario, Factory, Reference, Month, Product, Date, GM, Product, Account_type, Account, Cost_fixed, Cost_variable, Cost, Volume, Activity_UO1, Activity_UO1_Total, Component, Description, VTU_Pond, level, Exp_Coefficient, VTU_Pond_Unit"}},
							    {"ComponentsGMNature"		, New List(Of String) From {
													"Scenario, Factory, Month, Year, Currency" _
													, "Scenario, Id Factory, Desc Factory, Month, Id Product, Desc Product, Id Component, Id Averagegroup, Nature, VTU Periodic, VTU YTD"}},
							    {"NatureGMPivot"			, New List(Of String) From {
													"Scenario, Month, Year, Currency" _
													, "Factory, Id Product, Desc Product, ---"}},
								{"Volumes"					, New List(Of String) From {
													"Factory, Month, Year" _
													, "Scenario, Factory, Product, Year, Month, UO Type, Volume"}},
								{"Activity"				, New List(Of String) From {
													"Scenario, Factory, Month, Year, Time, GM Functions" _
													, "Factory, Month, GM, GM Function, Time, Activity TAJ, Activity TSO"}},													
								{"Activity_Reference"	, New List(Of String) From {
													"Scenario, Factory, Month, Year, Time, GM Functions" _
													, "Factory, Month, Id_Product, Desc_Product, Time, Volume, Activity TAJ, Activity TSO"}},																										
								{"ActivityIndex"			, New List(Of String) From {
													"Scenario, Scenario ref, Factory, Month, Year, Time" _
													, "Factory, Month, GM, GM Function,Time, Activity TSO, Activity TAJ REF, ActivityIndex, Activity Index Corrected"}},
							    {"ActivityIndexGM"			, New List(Of String) From {
													"Scenario, Factory, Month, Year" _
													, "Scenario, Id Factory, Factory, Id Product, Desc Product, Year, Month, Uotype, Volume"}},
								{"ActivityIndexTopGM"		, New List(Of String) From {
													"Scenario, Scenario ref, Factory, Month, Year, Time" _
													, "Month, GM, Time, Activity TAJ, Activity TSO, Activity TAJ Ref, Activity Index"}},
								{"Nomenclature"				, New List(Of String) From {
													"Product, Factory, Date" _
													, "Product, Component, Coefficient, Prorata, Level"}},				
								{"HRForCosting"		, New List(Of String) From {
													"Factory, Type, Indicator, Value, Activity TAJ, Paid Hours MOD, Paid Hours MOS, Headcount MOS, Description " _
													, "Factory, Activity TAJ, Paid Hours MOD, Paid Hours MOS, Headcount MOS"}},
								{"HRforCosting-CostCenter"	, New List(Of String) From {
													"Factory, Cost Center, Type, Paid_Hours_MOD, Paid Hours MOS, Indicator, Value, Activity TAJ, Headcount MOS, Description, Headcount MOS" _
													, "Factory, Cost Center, Type, Activity TAJ, Paid Hours MOD, Paid Hours MOS, Headcount MOS"}},
							    {"Masses"			, New List(Of String) From {
													"Factory, Year, Scenario, Scenario_ref, Currency" _
													, "Id Factory, Month, Id Nature, Nature, Id Account, Account, Id Cost Center, Nature Cost Center, Variability, Activity Index, Reference, Ref 100 Adjusted, Ref Adjusted, Actual at Actual Parity"}},
							    {"MassesRaw"		, New List(Of String) From {
													"Factory, Year, Scenario, Scenario_ref, Currency" _
													, "Id Factory, Month, Id Nature, Nature, Id Account, Account, Id Cost Center, Nature Cost Center, Variability, Activity Index, Reference, Ref 100 Adjusted, Ref Adjusted, Actual at Actual Parity"}},													
							   	{"StartupCostsDiffs", New List(Of String) From {
													"Factory, Year, Scenario" _
													, "Factory, Year, Month, Id_Nature, Nature, SAP Cost, Project Cost, Diffs"}},													
								{"MassesGM"			, New List(Of String) From {
													"Factory, Month, Year, Scenario, Scenario_ref, View, Currency" _
													, "Id Factory, Month, Nature, GM, Activity Index, Reference, Ref 100 Adjusted, Ref Semi Adjusted, Ref Adjusted, Ref Adjusted at Actual Parity, Actual at Actual Parity"}},
							    {"ScopeChange"		, New List(Of String) From {
													"Factory, Scenario, Scenario_ref, Year, Month" _
													, "Month, Id Factory, Cost type, Variability, Id Category, Desc Category, Value"}},
							    {"VarianceAnalysis"	, New List(Of String) From {
													"Month, Year, Scenario, Scenario_ref, Currency" _
													, "Id Factory, Factory, Month, Id Cost Nature, Cost Nature, Variability, Activity Index, Reference, Absorption of Fixed Costs, Scope Change, Impact Parity, Price, Product Process Evolution, Excess Labour, Unemployement, Productivity, COVID 19 Effect, GAP"}},
							    {"EnergyVariance"				, New List(Of String) From {
													"Year, Scenario, Scenario_ref, View, Currency" _
													, "Month, Factory, Energy, MWh in parity Analysis, Actual A Conso, FIP, Vairance at parity Analysis, Reference Price MWh at parity Ref, Conso Ref, Total Variance in Keur"}},
							    {"CostDetail"		, New List(Of String) From {
													"Year, Scenario, Factory, Currency" _
													, "Factory, Id Cost Center, Id Nature, Nature, Id MNG Account, MNG Account, Id Rubric,Month, Value Periodic, Value YTD"}},												
								{"Hours_CC"			, New List(Of String) From {
													"Factory, Scenario, Year, Indicator" _
													,"Id_Factory, Factory, WF_Type, Month, Indicator, Id_CostCenter, Type_CostCenter, Value"}},													
								{"Hours_CC_Only_VT"	, New List(Of String) From {
													"Factory, Scenario, Year, Indicator" _
													,"Id_Factory, Factory, WF_Type, Month, Indicator, Id_CostCenter, Type_CostCenter, Value"}},																										
								{"Hours_Hierarchy"	, New List(Of String) From {
													"Factory, Scenario, Year" _
													,"Id_Factory, Factory, WF_Type, Month, Indicator, Value"}},													
								{"FTE"				, New List(Of String) From {
													"Factory, Scenario, Year" _
													,"Factory, Month, WF Type, Id Cost Center, Hours Indicator, Shift, Type Cost Center, Hours, Worked Days, Hours by Shift, FTE"}},																										
								{"WorkingReport"	, New List(Of String) From {
													"Please do not execute this report" _
													,"Please do not execute this report"}} _
								}
						
						
						
							dtFile.Columns.Add("FileFilter")
							dtFile.Columns.Add("DataDetail")
							
							If infoDictionary.Keys.Contains(datasetName) Then
								dtFile.Rows.Add(infoDictionary(datasetName)(0), infoDictionary(datasetName)(1))
							Else
								dtFile.Rows.Add("No Filters", "No Detail")
							End If
														
							Return dtFile
							
						#End Region
						
						End If
						
						If sql = String.Empty Then
							sql = "SELECT 'Missing Reporting' AS Message"
						End If
						
						Return If(sBiBlendQuery="No", ExecuteSQL(si, sql), ExecuteSQLBIBlend(si, sql))
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using	
							
'						Return dt
						
					End Select
					
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		Public Function GetSQLDataVTU (
				ByVal si As SessionInfo, 
				ByVal scenario As String, ByVal month As String, ByVal factory As String, ByVal year As String, ByVal currency As String, 
				Optional impactParity As String="", Optional scenarioRef As String ="") As String
			
			Dim sqlVTU As String = String.Empty
			Dim sqlVersion As String = "Old"		
			
			If (scenario = "RF09" And CInt(month) >= 9) Or (CInt(year) >= 2026 )Then
				sqlVersion = "New"
			ElseIf (scenario = "Actual" And CInt(month) >= 9) Then
				sqlVersion = "New"			
			End If
			
			Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
			Return shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables_NewTables", factory, month, year, scenario, scenarioRef, "", "", "Existing", "", currency,"","",sqlVersion, impactParity)			
			
		End Function
		
		Private Function ExecuteSQLBIBlend(ByVal si As SessionInfo, ByVal sqlCmd As String)
			Dim dt As DataTable = Nothing
			'Use the name of the database, used in OneStream Server Configuration Utility >> App Server Config File >> Database Server Connections
			Dim extConnName As String = "OneStream BI Blend"
            Using dbConnApp As DBConnInfo = BRApi.Database.CreateExternalDbConnInfo(si, extConnName) 
				dt = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)
            End Using				
			Return dt
        End Function	
		
		Private Function ExecuteSql(ByVal si As SessionInfo, ByVal sql As String)        
			Dim dt As DataTable = Nothing				
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
			End Using  
			Return dt	
        End Function	
		
	End Class
	
End Namespace
