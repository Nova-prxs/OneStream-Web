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

' {Workspace.Current.PLTCosts.queries}{}{}
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
						
						Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries																		
						
						
#Region "Data Detail OLD"						
						If args.DataSetName.XFEqualsIgnoreCase("MngAccount_Detail_Old") Then
							
							Dim factory As String = args.NameValuePairs("factory")
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							Dim account_costcenter As String = args.NameValuePairs("account_costcenter")
							Dim mngAccount As String = String.Empty
							Dim costcenter As String = String.Empty
							
							If Not account_costcenter = String.Empty Then
								
								mngAccount = account_costcenter.Split(" | ")(0)
								costcenter = account_costcenter.Split(" | ")(1)
								
							End If
							
							Dim sql As String = String.Empty
							Dim dt As New DataTable
							
							sql = $"

					      		SELECT 
									A.*						
	
					      		FROM (	SELECT * 
										FROM XFC_PLT_RAW_CostsMonthly 
										WHERE 1=1
										AND month = {month}
										AND year = {year}
										AND id_costcenter = '{costcenter}'
										AND CASE
								      		WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
								        	WHEN CONCAT('R',id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
								        	WHEN CONCAT('R',id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
									        WHEN CONCAT('R',id_factory, entity) = 'R1301003' THEN 'R0548913'
									        WHEN CONCAT('R',id_factory, entity) = 'R1301002' THEN 'R0548914'
									        WHEN CONCAT('R',id_factory, entity) LIKE '%R0585%' THEN 'R0585'
									        WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
									        WHEN CONCAT('R',id_factory, entity) LIKE '%R0592%' THEN 'R0592'
								      	ELSE 'TBD' END = '{factory}' ) A
							
							
					            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
					             	ON A.id_costcenter = C.id
					          		AND C.scenario = 'Actual'
					          		AND DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) BETWEEN C.start_date AND C.end_date
																		
					      		LEFT JOIN (	SELECT DISTINCT mng_account, cnt_account, costcenter_type, costcenter_nature	
											FROM XFC_PLT_Mapping_Accounts) B
					       			ON A.num_cpte = B.cnt_account
									AND C.type = B.costcenter_type
									AND C.nature = B.costcenter_nature								
							
								WHERE mng_account = '{mngAccount}' 
								OR ('{mngAccount}' = 'Mapear Cuenta' AND B.cnt_account IS NULL)
												
							"
							
							'BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt
							
						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_All") Then
							
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							Dim factory As String = args.NameValuePairs("factory").Replace("'", "")
							' Dim mngAccount As String = args.NameValuePairs("mngAccount")
							
							Dim sql As String = String.Empty
							Dim dt As New DataTable
							
							sql = $"
							WITH factData as (
							
								SELECT
									id_costcenter,
									id_account,
									MONTH(date) as month,
									YEAR(date) as date,
									SUM(value) as TotalAmount,
									CONCAT(id_account, ' | ', id_costcenter ) as account_costcenter
								
								FROM XFC_PLT_FACT_Costs
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
									AND scenario = 'Actual'
								
								GROUP BY id_costcenter, date, id_account
							)
							
							SELECT *
							
							FROM factData						
							"
							'BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
#End Region

#Region "Data Detail"						
						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_Detail") Then
							
							Dim factory As String = args.NameValuePairs("factory")
							Dim year As String = args.NameValuePairs("year")
							Dim account_costcenter As String = args.NameValuePairs("account_costcenter")
							Dim mngAccount As String = String.Empty
							Dim costcenter As String = String.Empty
							
							Dim sql As String = "SELECT 1"
							Dim dt As New DataTable							
							
							If Not account_costcenter = String.Empty Then
								
								mngAccount = account_costcenter.Split(" | ")(0)
								costcenter = (account_costcenter.Split(" | ")(1)).Replace(" - No Master","").Replace(" - No Perimeter","")
																				
								sql = $"
									SELECT *
									FROM (
									    SELECT 
									        A.*, 
									        'M' + RIGHT('0' + CAST(A.month AS VARCHAR), 2) AS month_format
									    FROM (  
									        SELECT * 
									        FROM XFC_PLT_RAW_CostsMonthly 
									        WHERE 1=1
									            AND year = {year}
									            AND id_costcenter = '{costcenter}'
									            AND CASE
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301003' THEN 'R0548913'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301002' THEN 'R0548914'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0585%' THEN 'R0585'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0671%' THEN 'R0671'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0592%' THEN 'R0592'
									                ELSE 'TBD' 
									            END = '{factory}' 
									    ) A
								
									    LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
									        ON A.id_costcenter = C.id
									        AND C.scenario = 'Actual'
									        AND DATEFROMPARTS(CAST(A.year AS INT), CAST(A.month AS INT), 1) 
									            BETWEEN C.start_date AND C.end_date
								
										LEFT JOIN (
													SELECT DISTINCT 
														cnt_account
														, mng_account
														, costcenter_type
														, costcenter_nature
														, docF
			
													FROM XFC_PLT_Mapping_Accounts_docF 
												) as B					
							       			ON A.num_cpte = B.cnt_account
											AND C.type = B.costcenter_type
											AND C.nature = B.costcenter_nature
											AND A.domFonc = B.docF		 
								
									    WHERE mng_account = '{mngAccount}' 
									        OR ('{mngAccount}' = 'Mapear Cuenta' AND B.cnt_account IS NULL)
								
									) AS SourceTable
									PIVOT (
									    SUM(value_intern) 
									    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
									) AS PivotTable;
				
								"
							
							End If
							
							'BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt

#End Region

#Region "Data Detail Admin Check"			

						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_Detail_Admin") Then
							
							Dim rubrics_allowed 
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim dtrubric As DataTable = BRApi.Database.ExecuteSql(dbConn, "SELECT id FROM XFC_PLT_MASTER_Rubric WHERE included = '1' ORDER BY id", True)
								For Each dr As DataRow In dtrubric.Rows
									rubrics_allowed = rubrics_allowed & ",'" & dr(0) & "'"
								Next								
							End Using	
							rubrics_allowed = rubrics_allowed.Substring(1)
'							BRApi.ErrorLog.LogMessage(si, rubrics_allowed)
							
							Dim year As String = args.NameValuePairs("year")			
							Dim mngAccount As String = String.Empty
							Dim costcenter As String = String.Empty
							
							Dim dt As New DataTable							
																		
								Dim sql As String = $"
									SELECT factory_desc, ISNULL(mng_account,'Others') AS mng_account, costcenter, CONCAT(factory, ' | ', ISNULL(mng_account,'Others'), ' | ', costcenter) AS account_costcenter
										, SUM([M01]) AS [M01]
										, SUM([M02]) AS [M02]
										, SUM([M03]) AS [M03]
										, SUM([M04]) AS [M04]
										, SUM([M05]) AS [M05]
										, SUM([M06]) AS [M06]
										, SUM([M07]) AS [M07]
										, SUM([M08]) AS [M08]
										, SUM([M09]) AS [M09]
										, SUM([M10]) AS [M10]
										, SUM([M11]) AS [M11]
										, SUM([M12]) AS [M12]
									FROM (
									    SELECT 
									        A.*, 
											CASE WHEN id_costcenter IS NULL OR id_costcenter = '' THEN 'No Cost Center' ELSE id_costcenter END AS costcenter,
											F.description AS factory_desc,
											B.mng_account,											
									        'M' + RIGHT('0' + CAST(A.month AS VARCHAR), 2) AS month_format
									    FROM (  
									        SELECT *
												, CASE
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301003' THEN 'R0548913'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301002' THEN 'R0548914'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0585%' THEN 'R0585'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0671%' THEN 'R0671'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0592%' THEN 'R0592'
												END As factory 
								
									        FROM XFC_PLT_RAW_CostsMonthly 
									        WHERE year = {year}
											AND rubirc IN ({rubrics_allowed})
											AND value_intern <> 0

									    ) A
								
										INNER JOIN XFC_PLT_MASTER_Factory F
											ON A.factory = F.id
								
									    INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
									        ON A.id_costcenter = C.id
									        AND C.scenario = 'Actual'
									        AND DATEFROMPARTS(CAST(A.year AS INT), CAST(A.month AS INT), 1) 
									            BETWEEN C.start_date AND C.end_date
								
										LEFT JOIN (
													SELECT DISTINCT 
														cnt_account
														, mng_account
														, costcenter_type
														, costcenter_nature
														, docF
			
													FROM XFC_PLT_Mapping_Accounts_docF 
												) as B					
							       			ON A.num_cpte = B.cnt_account
											AND C.type = B.costcenter_type
											AND C.nature = B.costcenter_nature
											AND A.domFonc = B.docF	
								
									    WHERE mng_account IS NULL										
								
									) AS SourceTable
									PIVOT (
									    SUM(value_intern) 
									    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
									) AS PivotTable
								
									GROUP BY
										factory
										, factory_desc
										, mng_account
										, costcenter
								
									ORDER BY factory_desc, mng_account, costcenter;
				
								"
							
'							BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt

#End Region

#Region "Data Detail Admin Check (FACT)"			

						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_Detail_Admin_Fact") Then
							
							Dim rubrics_allowed 
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim dtrubric As DataTable = BRApi.Database.ExecuteSql(dbConn, "SELECT id FROM XFC_PLT_MASTER_Rubric WHERE included = '1' ORDER BY id", True)
								For Each dr As DataRow In dtrubric.Rows
									rubrics_allowed = rubrics_allowed & ",'" & dr(0) & "'"
								Next								
							End Using	
							rubrics_allowed = rubrics_allowed.Substring(1)
'							BRApi.ErrorLog.LogMessage(si, rubrics_allowed)
							
							Dim year As String = args.NameValuePairs("year")			
							Dim mngAccount As String = String.Empty
							Dim costcenter As String = String.Empty
							
							Dim dt As New DataTable							
																		
								Dim sql As String = $"
									SELECT factory_desc, mng_account, costcenter, CONCAT(id_factory, ' | ', mng_account, ' | ', costcenter) AS account_costcenter
										, SUM([M01]) AS [M01]
										, SUM([M02]) AS [M02]
										, SUM([M03]) AS [M03]
										, SUM([M04]) AS [M04]
										, SUM([M05]) AS [M05]
										, SUM([M06]) AS [M06]
										, SUM([M07]) AS [M07]
										, SUM([M08]) AS [M08]
										, SUM([M09]) AS [M09]
										, SUM([M10]) AS [M10]
										, SUM([M11]) AS [M11]
										, SUM([M12]) AS [M12]
								
									FROM (
									    SELECT 
									        A.*, 
											CASE WHEN id_costcenter IS NULL OR id_costcenter = '' THEN 'No Cost Center' ELSE id_costcenter END AS costcenter,
											F.description AS factory_desc,
											id_account AS mng_account
								
									    FROM (  
										        SELECT *			
													, 'M' + RIGHT('0' + CAST(MONTH(date) AS VARCHAR), 2) AS month_format
										        FROM XFC_PLT_FACT_Costs
										        WHERE YEAR(date) = {year}
												AND scenario = 'Actual'
												AND id_account = 'Others'
												AND id_rubric IN ({rubrics_allowed})
									    	) A
								
										INNER JOIN XFC_PLT_MASTER_Factory F
											ON A.id_factory = F.id
								
									    INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
									        ON A.id_costcenter = C.id
									        AND C.scenario = 'Actual'
									        AND A.date BETWEEN C.start_date AND C.end_date
									) AS SourceTable
									PIVOT (
									    SUM(value) 
									    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
									) AS PivotTable	
								
									GROUP BY
										id_factory
										, factory_desc
										, mng_account
										, costcenter
								
									ORDER BY factory_desc, mng_account, costcenter;
				
								"
							
'							BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt

#End Region

#Region "Data Detail Admin Check Detail"			

						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_Detail_Admin_Detail") Then							

							Dim rubrics_allowed 
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim dtrubric As DataTable = BRApi.Database.ExecuteSql(dbConn, "SELECT id FROM XFC_PLT_MASTER_Rubric WHERE included = '1' ORDER BY id", True)
								For Each dr As DataRow In dtrubric.Rows
									rubrics_allowed = rubrics_allowed & ",'" & dr(0) & "'"
								Next								
							End Using	
							rubrics_allowed = rubrics_allowed.Substring(1)
'							BRApi.ErrorLog.LogMessage(si, rubrics_allowed)							
							
							Dim year As String = args.NameValuePairs("year")
							Dim account_costcenter As String = args.NameValuePairs("account_costcenter")
							Dim factory As String = String.Empty							
							Dim mngAccount As String = String.Empty
							Dim costcenter As String = String.Empty
							
							Dim sql As String = "SELECT 1"
							Dim dt As New DataTable							
							
'							BRApi.ErrorLog.LogMessage(si, account_costcenter)
							
							If Not account_costcenter = String.Empty And Not account_costcenter.StartsWith("|") Then
								
								factory = account_costcenter.Split(" | ")(0)
								mngAccount = account_costcenter.Split(" | ")(1)
								costcenter = (account_costcenter.Split(" | ")(2))
																				
								sql = $"
									SELECT *
									FROM (
									    SELECT 
									        A.*, 
									        'M' + RIGHT('0' + CAST(A.month AS VARCHAR), 2) AS month_format
									    FROM (  
									        SELECT * 
									        FROM XFC_PLT_RAW_CostsMonthly 
									        WHERE 1=1
									            AND year = {year}
												AND rubirc IN ({rubrics_allowed})
									            AND CASE 
													WHEN id_costcenter IS NULL OR id_costcenter = '' THEN 'No Cost Center' 
													ELSE id_costcenter END = '{costcenter}'
									            AND CASE
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301003' THEN 'R0548913'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301002' THEN 'R0548914'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0585%' THEN 'R0585'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0671%' THEN 'R0671'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0592%' THEN 'R0592'
									                ELSE 'TBD' END = '{factory}' 
												AND value_intern <> 0
									    ) A
								
									    INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
									        ON A.id_costcenter = C.id
									        AND C.scenario = 'Actual'
									        AND DATEFROMPARTS(CAST(A.year AS INT), CAST(A.month AS INT), 1) 
									            BETWEEN C.start_date AND C.end_date													
								
										LEFT JOIN (
													SELECT DISTINCT 
														cnt_account
														, mng_account
														, costcenter_type
														, costcenter_nature
														, docF
			
													FROM XFC_PLT_Mapping_Accounts_docF 
												) as B					
							       			ON A.num_cpte = B.cnt_account
											AND C.type = B.costcenter_type
											AND C.nature = B.costcenter_nature
											AND (A.domFonc = B.docF OR '{factory}' = 'R0671')	
								
									    WHERE mng_account IS NULL																	
								
									) AS SourceTable
									PIVOT (
									    SUM(value_intern) 
									    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
									) AS PivotTable;
				
								"
							
							End If
							
'							BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt

#End Region

#Region "Data Detail Factory Check (FACT)"			

						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_Detail_Factory_Fact") Then
							
							Dim rubrics_allowed 
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim dtrubric As DataTable = BRApi.Database.ExecuteSql(dbConn, "SELECT id FROM XFC_PLT_MASTER_Rubric WHERE included = '1' ORDER BY id", True)
								For Each dr As DataRow In dtrubric.Rows
									rubrics_allowed = rubrics_allowed & ",'" & dr(0) & "'"
								Next								
							End Using	
							rubrics_allowed = rubrics_allowed.Substring(1)
'							BRApi.ErrorLog.LogMessage(si, rubrics_allowed)
							
							Dim factory As String = args.NameValuePairs("factory")
							Dim year As String = args.NameValuePairs("year")			
							Dim mngAccount As String = String.Empty
							Dim costcenter As String = String.Empty
							
							Dim dt As New DataTable							
																		
								Dim sql As String = $"
									SELECT factory_desc, mng_account, costcenter, CONCAT(id_factory, ' | ', mng_account, ' | ', costcenter) AS account_costcenter
										, SUM([M01]) AS [M01]
										, SUM([M02]) AS [M02]
										, SUM([M03]) AS [M03]
										, SUM([M04]) AS [M04]
										, SUM([M05]) AS [M05]
										, SUM([M06]) AS [M06]
										, SUM([M07]) AS [M07]
										, SUM([M08]) AS [M08]
										, SUM([M09]) AS [M09]
										, SUM([M10]) AS [M10]
										, SUM([M11]) AS [M11]
										, SUM([M12]) AS [M12]
								
									FROM (
									    SELECT 
									        A.*, 
											CASE WHEN id_costcenter IS NULL OR id_costcenter = '' THEN 'No Cost Center' ELSE id_costcenter END AS costcenter,
											F.description AS factory_desc,
											id_account AS mng_account
								
									    FROM (  
										        SELECT *			
													, 'M' + RIGHT('0' + CAST(MONTH(date) AS VARCHAR), 2) AS month_format
										        FROM XFC_PLT_FACT_Costs
										        WHERE YEAR(date) = {year}
												AND scenario = 'Actual'
												AND id_factory = '{factory}'
												AND id_account = 'Others'
												AND id_rubric IN ({rubrics_allowed})
									    	) A
								
										INNER JOIN XFC_PLT_MASTER_Factory F
											ON A.id_factory = F.id
								
									    INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
									        ON A.id_costcenter = C.id
									        AND C.scenario = 'Actual'
									        AND A.date BETWEEN C.start_date AND C.end_date
									) AS SourceTable
									PIVOT (
									    SUM(value) 
									    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
									) AS PivotTable	
								
									GROUP BY
										id_factory
										, factory_desc
										, mng_account
										, costcenter
								
									ORDER BY factory_desc, mng_account, costcenter;
				
								"
							
'							BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt

#End Region

#Region "Data Detail Factory Check Detail"			

						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_Detail_Factory_Detail") Then							

							Dim rubrics_allowed 
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim dtrubric As DataTable = BRApi.Database.ExecuteSql(dbConn, "SELECT id FROM XFC_PLT_MASTER_Rubric WHERE included = '1' ORDER BY id", True)
								For Each dr As DataRow In dtrubric.Rows
									rubrics_allowed = rubrics_allowed & ",'" & dr(0) & "'"
								Next								
							End Using	
							rubrics_allowed = rubrics_allowed.Substring(1)
'							BRApi.ErrorLog.LogMessage(si, rubrics_allowed)							
							
							Dim year As String = args.NameValuePairs("year")
							Dim account_costcenter As String = args.NameValuePairs("account_costcenter")
							Dim factory As String = String.Empty							
							Dim mngAccount As String = String.Empty
							Dim costcenter As String = String.Empty
							
							Dim sql As String = "SELECT 1"
							Dim dt As New DataTable							
							
'							BRApi.ErrorLog.LogMessage(si, account_costcenter)
							
							If Not account_costcenter = String.Empty And Not account_costcenter.StartsWith("|") Then
								
								factory = account_costcenter.Split(" | ")(0)
								mngAccount = account_costcenter.Split(" | ")(1)
								costcenter = (account_costcenter.Split(" | ")(2))
																				
								sql = $"
									SELECT *
									FROM (
									    SELECT 
									        A.*, 
									        'M' + RIGHT('0' + CAST(A.month AS VARCHAR), 2) AS month_format
									    FROM (  
									        SELECT * 
									        FROM XFC_PLT_RAW_CostsMonthly 
									        WHERE 1=1
									            AND year = {year}
												AND rubirc IN ({rubrics_allowed})
									            AND CASE 
													WHEN id_costcenter IS NULL OR id_costcenter = '' THEN 'No Cost Center' 
													ELSE id_costcenter END = '{costcenter}'
									            AND CASE
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301003' THEN 'R0548913'
									                WHEN CONCAT('R', id_factory, entity) = 'R1301002' THEN 'R0548914'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0585%' THEN 'R0585'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0671%' THEN 'R0671'
									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0592%' THEN 'R0592'
									                ELSE 'TBD' END = '{factory}' 
												AND value_intern <> 0
									    ) A
								
									    INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
									        ON A.id_costcenter = C.id
									        AND C.scenario = 'Actual'
									        AND DATEFROMPARTS(CAST(A.year AS INT), CAST(A.month AS INT), 1) 
									            BETWEEN C.start_date AND C.end_date													
								
										LEFT JOIN (
													SELECT DISTINCT 
														cnt_account
														, mng_account
														, costcenter_type
														, costcenter_nature
														, docF
			
													FROM XFC_PLT_Mapping_Accounts_docF 
												) as B					
							       			ON A.num_cpte = B.cnt_account
											AND C.type = B.costcenter_type
											AND C.nature = B.costcenter_nature
											AND A.domFonc = B.docF	
								
									    WHERE mng_account IS NULL																	
								
									) AS SourceTable
									PIVOT (
									    SUM(value_intern) 
									    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
									) AS PivotTable;
				
								"
							
							End If
							
'							BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt

#End Region

#Region "Check S4H"

						Else If args.DataSetName.XFEqualsIgnoreCase("CheckS4H") Then
							
							' Define error string
							Dim errorString As String = "Error loading Data Adapter 'CheckS4H': "
							
							' Get parameters
							Dim factory As String = args.NameValuePairs.XFGetValue("factory")
							Dim year As String = args.NameValuePairs.XFGetValue("year")
							Dim month As Integer = args.NameValuePairs.XFGetValue("month")
							
							' Validate parameters
							'------------------------------------------------------------------------------------------------------
							' Strings not empty
							If String.IsNullOrEmpty(factory) Then _
								Return NewErrorDataTable(errorString & "parameter 'factory' is empty.")
							If String.IsNullOrEmpty(year) Then _
								Return NewErrorDataTable(errorString & "parameter 'year' is empty.")
							If String.IsNullOrEmpty(month) Then _
								Return NewErrorDataTable(errorString & "parameter 'month' is empty.")
								
							' Correct integers
							Dim yearInt As Integer
							If Not Integer.TryParse(year, yearInt) Then _
								Return NewErrorDataTable(errorString & "parameter 'year' is not a correct integer.")
							Dim monthInt As Integer
							If Not Integer.TryParse(month, monthInt) Then _
								Return NewErrorDataTable(errorString & "parameter 'month' is not a correct integer.")
								
							' Correct values
							If yearInt < 2000 OrElse yearInt > 2100 Then _
								Return NewErrorDataTable(errorString & "parameter 'year' must be between 2000 and 2100.")
							If monthInt < 1 OrElse monthInt > 12 Then _
								Return NewErrorDataTable(errorString & "parameter 'month' must be between 1 and 12.")
								
							' Factory formatting
							Dim factoryR As String = String.Empty	
							Dim entityR As String = String.Empty	
							Dim companyR As String = String.Empty
							
							Select Case factory 
							'Cacia
							Case "R0671"
								factoryR = "0671"
								entityR = "STE"
								companyR = "PT10"
							End Select
							
							' Get data
							Dim dt As New DataTable
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim dbParamInfos As New List(Of DbParamInfo) From {
									New DbParamInfo("paramFactory", factoryR),
									New DbParamInfo("paramEntity", entityR),
									New DbParamInfo("paramCompany", companyR),
									New DbParamInfo("paramYear", yearInt),
									New DbParamInfo("paramMonth", monthInt)
								}
								
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									$"
									WITH filtered_cost_centers AS (
										SELECT *
										FROM XFC_MAIN_MASTER_CostCenters WITH(NOLOCK)
										WHERE end_date > GETDATE() AND (is_blocked <> 1 OR is_blocked IS NULL)
									), filtered_account_rubrics AS (
										SELECT
											cc.id AS cost_center_id, ccon.id_old AS cost_center_id_old, ar.cost_center_class_id,
											ar.account_name, ar.conso_rubric
										FROM filtered_cost_centers cc
										LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew ccon WITH(NOLOCK)
										ON cc.id = ccon.id
										LEFT JOIN XFC_MAIN_MASTER_AccountRubrics ar WITH(NOLOCK)
										ON cc.class_id = ar.cost_center_class_id
									), transactions_mapped_by_cost_center_class AS (
										-- First we check if the account has a rubric mapping with a specific cost center class
										SELECT 
											CONCAT(ft.year, 'M', ft.month) AS time,
											ft.profit_center_id, ft.cost_center_id,
											ft.account_name, far.cost_center_class_id, far.conso_rubric,
											ft.business_partner_id, ft.order_id, ft.product_id,
											ft.amount
										FROM (
											SELECT 
												year, month,
												account_name,
												cost_center_id, profit_center_id,
												business_partner_id, order_id, product_id,
												amount
											FROM XFC_MAIN_FACT_PnLTransactions pnlt WITH(NOLOCK)
											WHERE year = @paramYear AND month = @paramMonth AND company_id = @paramCompany
										) AS ft
										LEFT JOIN filtered_account_rubrics AS far
										ON ft.account_name = far.account_name AND ft.cost_center_id = far.cost_center_id
									), transactions_pnl_mapped AS (
										-- Second we check if the account has a rubric mapping with 'none' as cost center class
										SELECT 
											tmbccc.time,
											tmbccc.profit_center_id, tmbccc.cost_center_id,
											tmbccc.account_name,
											tmbccc.cost_center_class_id, COALESCE(tmbccc.conso_rubric, ar.conso_rubric) AS conso_rubric,
											tmbccc.business_partner_id, tmbccc.order_id, tmbccc.product_id,
											tmbccc.amount
										FROM transactions_mapped_by_cost_center_class tmbccc
										LEFT JOIN XFC_MAIN_MASTER_AccountRubrics ar WITH(NOLOCK)
										ON tmbccc.account_name = ar.account_name AND tmbccc.conso_rubric IS NULL AND ar.cost_center_class_id = 'none'
									), transactions_pct_mapped AS (
										SELECT
										    F.time,
											F.profit_center_id, F.cost_center_id,
											F.account_name, R.description AS account_description,
											F.cost_center_class_id, F.conso_rubric,
											F.business_partner_id, F.order_id, F.product_id,
											A.name_old,
											B.mng_account,
											MA.account_type,
											F.amount,
											CASE 
												WHEN F.conso_rubric IS NOT NULL THEN 
													CASE
														WHEN NULLIF(B.mng_account,'') IS NULL AND A.name_old IS NOT NULL THEN ISNULL(F.account_name,'') + ' - ' + ISNULL(A.name_old,'') + ' - No Mapping Mng Account'
														WHEN A.name_old IS NULL THEN F.account_name + ' - No Mapping'
														ELSE NULL
													END 
												ELSE F.account_name + ' - Ignore' 
											END AS [num_cpte_reason],
											CASE 
												WHEN F.cost_center_id = 'none' THEN ' - No Cost Center'
												WHEN F.cost_center_id IS NOT NULL AND C.id_old IS NULL THEN F.cost_center_id + ' - No Mapping'
												ELSE NULL
											END AS [id_costcenter_reason]
							
										FROM transactions_pnl_mapped F
									
										LEFT JOIN XFC_MAIN_MASTER_AccountsOldToNew A WITH(NOLOCK) ON A.name = F.account_name		
										
										LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew C WITH(NOLOCK) ON C.id = F.cost_center_id
									
							            LEFT JOIN (
												SELECT *
												FROM XFC_PLT_MASTER_CostCenter_Hist WITH(NOLOCK)
												WHERE scenario = 'Actual'
													AND DATEFROMPARTS(@paramYear, @paramMonth, 1) BETWEEN start_date AND end_date
										) C2 ON C2.id = C.id_old					
									
										LEFT JOIN (
											SELECT name, description
											FROM XFC_MAIN_MASTER_Accounts WITH(NOLOCK)
										) R ON R.name = F.account_name
									
										LEFT JOIN (
											SELECT DISTINCT cnt_account
											FROM XFC_PLT_MAPPING_Accounts_DocF WITH(NOLOCK)
										) M ON A.name_old = M.cnt_account
									
										LEFT JOIN (
														SELECT  
															cnt_account																
															, costcenter_type
															, costcenter_nature
															, MAX(mng_account) AS mng_account
														FROM XFC_PLT_Mapping_Accounts_docF 
														GROUP BY cnt_account, costcenter_type, costcenter_nature		
													) as B					
								       			ON A.name_old = B.cnt_account
												AND C2.type = B.costcenter_type
												AND C2.nature = B.costcenter_nature		
									
										LEFT JOIN XFC_PLT_MASTER_Account MA
											ON B.mng_account = MA.id
									)
									
									SELECT
										time,
										profit_center_id, cost_center_id,
										account_name, account_description,
										cost_center_class_id, name_old, mng_account, account_type, conso_rubric,
										--business_partner_id, order_id, product_id,
										SUM(amount) AS amount,
										CAST(
											CASE
												WHEN conso_rubric IS NULL THEN 0
												ELSE 1 
											END
										AS BIT) AS is_pnl,
										CAST(
											CASE
												WHEN num_cpte_reason IS NULL AND id_costcenter_reason IS NULL THEN 1
												ELSE 0
											END
										AS BIT) AS is_pct,
										num_cpte_reason,
										id_costcenter_reason
									FROM transactions_pct_mapped
									
									GROUP BY time,
										profit_center_id, cost_center_id,
										account_name, account_description,
										cost_center_class_id, name_old, mng_account, account_type, conso_rubric,
																				CAST(
											CASE
												WHEN conso_rubric IS NULL THEN 0
												ELSE 1 
											END
										AS BIT),
										CAST(
											CASE
												WHEN num_cpte_reason IS NULL AND id_costcenter_reason IS NULL THEN 1
												ELSE 0
											END
										AS BIT),
										num_cpte_reason,
										id_costcenter_reason;
									",
									dbParamInfos,
									False
								)
							End Using
							
							Return dt

#End Region

#Region "Loaded Costs"								
							
						Else If args.DataSetName.XFEqualsIgnoreCase("MngAccount_All") Then
							
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							Dim factory As String = args.NameValuePairs("factory").Replace("'", "")
							' Dim mngAccount As String = args.NameValuePairs("mngAccount")
							
							Dim sql As String = String.Empty
							Dim dt As New DataTable
							
							sql = $"
							WITH factData as (
							
								SELECT
									F.id_costcenter,
							        M.account_type AS id_nature,
							        M.account_type + ' - ' + ISNULL(N.Description, '') AS nature,							
									F.id_account,
							        F.id_account + ' - ' + ISNULL(M.Description, '') AS account,							
									MONTH(F.date) as month,
									YEAR(F.date) as date,
									SUM(F.value) as TotalAmount,
									CONCAT(F.id_account, ' | ', F.id_costcenter ) as account_costcenter
							
							    FROM XFC_PLT_FACT_Costs F
							    LEFT JOIN XFC_PLT_MASTER_Account M
							        ON F.id_account = M.id  
							    LEFT JOIN XFC_PLT_MASTER_NatureCost N
							        ON M.account_type = N.id  
							    LEFT JOIN fxRate R1
							        ON MONTH(F.date) = R1.month							
								
								WHERE 1=1
									AND F.id_factory = '{factory}'
									AND MONTH(F.date) = {month}
									AND YEAR(F.date) = {year}
									AND F.scenario = 'Actual'
									AND F.id_account <> 'Others' 
								  	AND F.id_account <> '#'							
								
								GROUP BY F.id_costcenter, F.date, F.id_account, M.account_type, N.Description, M.Description
							)
							
							SELECT *
							
							FROM factData						
							"
							'BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
#End Region

#Region "Hourly Rate"								
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Hourly_Rate") Then
							
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim factory As String = args.NameValuePairs("factory").Replace("'", "")
							Dim currency As String = args.NameValuePairs("currency")
							
							Dim sql As String = String.Empty
							Dim dt As New DataTable
							
							Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
						  	Dim sql_Rate = shared_queries.AllQueries(si, "RATE", factory, "", year, scenario, "", "", "", "", "", currency)
							
							sql = sql_DailyHours & sql_Rate & $"
												
								, factCosts AS (
							
									SELECT
          								'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(F.date)),2) as month
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
									
									WHERE 1=1
										AND F.id_factory = '{factory}'
										AND YEAR(F.date) = {year}
										AND F.scenario = '{scenario}'
										AND M.account_type IN (1,2)
										AND F.id_account <> 'Others' 
									  	AND F.id_account <> '#'							
									
									GROUP BY F.id_costcenter, F.date, F.id_account, M.account_type, N.Description, M.Description, R.rate
								)
							
								, dataCalculation AS (
										SELECT 
											F.id_nature
											, SUM(F.value) AS costs
											, SUM(F.value_for_coefficient) AS costs_for_coefficient							
											, SUM(NULLIF(P.value,0)) AS paid_hours
											, SUM(F.value) / SUM(NULLIF(P.value,0)) AS value

									    FROM factCosts F										
								
										LEFT JOIN PaidHours P
											ON F.month = P.month
											AND F.id_nature = P.wf_type
											AND F.id_costcenter = P.id_costcenter
							
										GROUP BY F.id_nature
								)
							
								, coefficient_rate AS (
										SELECT 
											id_nature
											, costs / costs_for_coefficient AS coefficient

									    FROM dataCalculation																		
								)							
														
								SELECT id_nature, nature, SUM(costs) AS costs, SUM(paid_hours) AS paid_hours, SUM(value_calc) AS value_calc, SUM(value_input) AS value_input
							
								FROM (
										SELECT
											id_nature
											, CASE WHEN id_nature = 1 THEN 'MOD' ELSE 'MOS' END AS nature
											, SUM(costs) AS costs
											, SUM(paid_hours) AS paid_hours
											, SUM(value) AS value_calc
											, 0 AS value_input
										FROM dataCalculation
										GROUP BY id_nature
								
										UNION ALL
								
										SELECT
											I.id_nature
											, CASE WHEN I.id_nature = 1 THEN 'MOD' ELSE 'MOS' END AS nature
											, 0 AS costs
											, 0 AS paid_hours							
											, 0 AS value_calc
											, SUM(value) * coefficient AS value_input
							
										FROM XFC_PLT_AUX_HourlyRate_Input I
										LEFT JOIN coefficient_rate C
											ON I.id_nature = C.id_nature						
										WHERE id_factory = '{factory}'
										AND year = {year}
										AND scenario = '{scenario}'
          								GROUP BY I.id_nature, coefficient							
															
									) AS RES
							
								GROUP BY id_nature, nature
							
									"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRAPI.ErrorLog.LogMessage(si, sql)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
#End Region

#Region "Hourly Rate Detail"								
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Hourly_Rate_Detail") Then
							
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim factory As String = args.NameValuePairs("factory").Replace("'", "")
							Dim currency As String = args.NameValuePairs("currency")
							
							Dim sql As String = String.Empty
							Dim dt As New DataTable
							
							Dim sql_DailyHours As String =  shared_queries.AllQueries(si, "DailyHours",factory,, year, scenario, "", "", "",,,"")
						  	Dim sql_Rate = shared_queries.AllQueries(si, "RATE", factory, "", year, scenario, "", "", "", "", "", currency)
							
							sql = sql_DailyHours & sql_Rate & $"
												
								, factCosts AS (
							
									SELECT
          								'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(F.date)),2) as month
										, M.account_type AS id_nature
										, F.id_costcenter
										, SUM(F.value) * R.rate as value
								
								    FROM XFC_PLT_FACT_Costs F
								    LEFT JOIN XFC_PLT_MASTER_Account M
								        ON F.id_account = M.id  
								    LEFT JOIN XFC_PLT_MASTER_NatureCost N
								        ON M.account_type = N.id  
									LEFT JOIN fxRate R
										ON MONTH(F.date) = R.month							
									
									WHERE 1=1
										AND F.id_factory = '{factory}'
										AND YEAR(F.date) = {year}
										AND F.scenario = '{scenario}'
										AND M.account_type IN (1,2)
										AND F.id_account <> 'Others' 
									  	AND F.id_account <> '#'							
									
									GROUP BY F.id_costcenter, F.date, F.id_account, M.account_type, N.Description, M.Description, R.rate
								)
							
								SELECT 
									F.month
									, F.id_nature
									, CASE WHEN id_nature = 1 THEN 'MOD' ELSE 'MOS' END AS nature
									, F.id_costcenter
									, F.value AS costs
									, P.value AS paid_hours
									, F.value / NULLIF(P.value,0) AS value

							    FROM factCosts F										
						
								LEFT JOIN PaidHours P
									ON F.month = P.month
									AND F.id_nature = P.wf_type
									AND F.id_costcenter = P.id_costcenter
							
								ORDER BY F.id_nature, F.month, F.id_costcenter

									"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'								BRAPI.ErrorLog.LogMessage(si, sql)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
#End Region

#Region "Costs Pivot"						
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Costs_Pivot") Then
							
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim factory As String = args.NameValuePairs("factory").Replace("'", "")
							Dim currency As String = args.NameValuePairs("currency")
														
							Dim dt As New DataTable
														
							Dim sql_Rate = shared_queries.AllQueries(si, "RATE", factory, "", year, scenario, "", "", "", "", "", currency)								
							
							Dim sql As String = sql_Rate.Replace(", fxRateRef AS","WITH fxRateRef AS") & $"							
							
								, CostData AS (
								    SELECT
								        F.id_costcenter AS [Id Cost Center],
								        M.account_type AS [Id Nature],
								        RIGHT('0' + M.account_type,2) + ' - ' + ISNULL(N.Description, '') AS [Nature],
								        F.id_account AS [Id MNG Account],
								        F.id_account + ' - ' + ISNULL(M.Description, '') AS [MNG Account],
										F.id_rubric AS [Id Rubric],
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
							
								    WHERE 1=1
								        AND F.id_factory = '{factory}'
								        AND YEAR(F.date) = {year}
								        AND F.scenario = 'Actual'
								        AND F.id_account <> 'Others' 
								        AND F.id_account <> '#'
								    GROUP BY F.id_costcenter, F.date, F.id_account, M.account_type, F.id_rubric, M.Description, N.Description, R1.rate
								)
								SELECT 
								    [Id Cost Center],
								    [Id Nature],
								    [Nature],
								    [Id MNG Account],
								    [MNG Account],
									[Id Rubric],
								    Month,
								    Value AS [Value Periodic],
								    -- Acumulado por año, centro de costos y cuenta
								    SUM(Value) OVER (
								        PARTITION BY [Id Cost Center], [Id Nature], [Nature], [Id MNG Account],  [MNG Account], Year 
								        ORDER BY Month
								        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
								    ) AS [Value YTD]
								FROM CostData
								ORDER BY [Id Nature], [Id MNG Account], Month;

							"
'							BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
#End Region

#Region "Costs Distribution Pivot"						
							
						Else If args.DataSetName.XFEqualsIgnoreCase("Costs_Distribution_Pivot") Then
							
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim factory As String = args.NameValuePairs("factory").Replace("'", "")
							Dim currency As String = args.NameValuePairs("currency")
														
							Dim dt As New DataTable
														
							Dim sql_Rate = shared_queries.AllQueries(si, "RATE", factory, "", year, scenario, "", "", "", "", "", currency)								
							
							Dim sql As String = sql_Rate.Replace(", fxRateRef AS","WITH fxRateRef AS") & $"							
							
								, CostData AS (
								    SELECT
								        F.id_costcenter AS [Id Cost Center],
								        M.account_type AS [Id Nature],
								        RIGHT('0' + M.account_type,2) + ' - ' + ISNULL(N.Description, '') AS [Nature],
								        F.id_account AS [Id MNG Account],
								        F.id_account + ' - ' + ISNULL(M.Description, '') AS [MNG Account],
										F.id_average_group AS [Id GM],
										F.id_average_group + ' - ' + ISNULL(A.Description, '') AS [GM],
										--F.id_product AS [Id Product],
										--F.id_product + ' - ' + ISNULL(P.Description, '') AS [Product],							
								        CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(F.date))), 2)) AS Month,
								        YEAR(F.date) AS Year,
								        SUM(F.value) * R1.rate AS Value
							
								    FROM XFC_PLT_FACT_CostsDistribution F
								    LEFT JOIN XFC_PLT_MASTER_Account M
								        ON F.id_account = M.id  
								    LEFT JOIN XFC_PLT_MASTER_NatureCost N
								        ON M.account_type = N.id  
								    LEFT JOIN XFC_PLT_MASTER_AverageGroup A
								        ON F.id_average_group = A.id 			
								    --LEFT JOIN XFC_PLT_MASTER_Product P
								    --    ON F.id_product = P.id 								
								    LEFT JOIN fxRate R1
								        ON MONTH(F.date) = R1.month
							
								    WHERE 1=1
								        AND F.id_factory = '{factory}'
								        AND YEAR(F.date) = {year}
								        AND F.scenario = 'Actual'
								        AND F.id_account <> 'Others' 
								        AND F.id_account <> '#'
								    GROUP BY F.id_costcenter, F.date, F.id_account, M.account_type, M.Description, N.Description, R1.rate
								)
								SELECT 
								    [Id Cost Center],
								    [Id Nature],
								    [Nature],
								    [Id MNG Account],
								    [MNG Account],
									[Id GM]
									[GM]
									--[Id Product],
									--[Product],
								    Month,
								    Value
								FROM CostData
								ORDER BY [Id Nature], [Id MNG Account], Month;

							"
'							BRAPI.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
#End Region

#Region "Costs Distribution"

	#Region "Consulta General"							

						Else If args.DataSetName.XFEqualsIgnoreCase("SharedCosts") Then
							
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							Dim factory As String = args.NameValuePairs("factory")
							Dim cecoType As String = args.NameValuePairs("cecoType")
							If cecoType = "All" Then
								cecoType = "CP', 'CAT', 'CAA"
							End If
							Dim sql As String = String.Empty
							Dim sql2 As String = String.Empty
							Dim dt As New DataTable
								
							sql = shared_queries.AllQueries(si, "DistribucionCostes", factory, month, year, scenario)	
							sql = sql & $"
 										SELECT
											'CAA_CAT' as Nature
											, B.id_averagegroup as id_averagegroup
											, B.id_costcenter as id_costcenter
											, B.id_account as id_account
											, B.account_type as account_type
											, COALESCE(SUM(B.GM_SharedCosts),SUM(TotalAmount)) as TotalAmount
									        
									  	FROM catSharedCostsGM B
										
										LEFT JOIN (
												SELECT id_averagegroup, '1' as Exist					
												FROM catWeights					
												GROUP BY id_averagegroup
											) as D
											ON B.destination = D.id_averagegroup
										
										WHERE 1=1
											AND ((D.Exist IS NULL) OR B.filterAcc = '-1')											
																
										GROUP BY B.id_averagegroup, B.id_costcenter, B.id_account, B.account_type
									
							"
							
							
							'BRApi.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
	#End Region	
	
	#Region "Costes por CeCo y Cost Type"			
							
						Else If args.DataSetName.XFEqualsIgnoreCase("SharedCosts_CeCo_CostType") Then
							
							Dim scenario As String = args.NameValuePairs.GetValueOrEmpty("scenario")
							Dim month As String = args.NameValuePairs.GetValueOrEmpty("month")
							Dim year As String = args.NameValuePairs.GetValueOrEmpty("year")
							Dim factory As String = args.NameValuePairs.GetValueOrEmpty("factory")
							Dim tso_taj As String = args.NameValuePairs.GetValueOrEmpty("tso_taj")

							
							Dim sql As String = String.Empty
							Dim dt As New DataTable
							Dim dbParamInfos As New List(Of DbParamInfo) From {
								New DbParamInfo("paramScenario", scenario),
								New DbParamInfo("paramMonth", month),
								New DbParamInfo("paramYear", year),
								New DbParamInfo("paramFactory", factory),
								New DbParamInfo("paramTSOTAJ", tso_taj)
							}
								
							sql = $"
							
								SELECT *
								FROM (
								    SELECT 
								        A.id_costcenter
										, CASE 
											WHEN C.nature IN ('CP', 'CAT', 'CAA') THEN C.nature
											ELSE 'OTH'
										END as nature
										, CASE
											WHEN LEN(B.account_type) > 1 THEN CONCAT(B.account_type,' - ', N.description) 
											ELSE CONCAT('0',B.account_type,' - ', N.description)
										END AS account_nature
								        , CONCAT(A.id_costcenter, ' | ', B.account_type) AS filterCol
								        , 'M' + RIGHT('0' + CAST(MONTH(A.date) AS VARCHAR), 2) AS month_format -- Formatea el mes como M01, M02, etc.
								        , A.value
							
								    FROM XFC_PLT_FACT_CostsDistribution A
							
								    LEFT JOIN XFC_PLT_MASTER_Account B
								        ON A.id_account = B.id
							
									LEFT JOIN XFC_PLT_MASTER_NatureCost N
										ON B.account_type = N.id
							
									INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
										ON A.id_costcenter = C.id
										AND A.date BETWEEN C.start_date AND C.end_date
										AND C.scenario = 'Actual'
							
								    WHERE 1=1
								        AND A.scenario = @paramScenario
								        AND YEAR(A.date) = @paramYear
								        AND A.id_factory = @paramFactory
								        AND A.type_tso_taj = @paramTSOTAJ
										
							
								) AS SourceTable
								PIVOT (
								    SUM(value) 
								    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
								) AS PivotTable
							
								WHERE 1=1
									
								ORDER BY account_nature, nature,  id_costcenter;
							
							"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, dbParamInfos, True)
							End Using	
							
							Return dt	
	#End Region	
							
	#Region "Costes GM - Reference"							
						Else If args.DataSetName.XFEqualsIgnoreCase("SharedCosts_GM_Reference") Then
							
							Dim year As String = args.NameValuePairs.GetValueOrDefault("year", "2024")
							Dim scenario As String = args.NameValuePairs.GetValueOrDefault("scenario", "2024")
							Dim factory As String = args.NameValuePairs.GetValueOrDefault("factory", "R0671")
							Dim tso_taj As String = args.NameValuePairs.GetValueOrDefault("tso_taj", "TAJ")
							Dim costcenter_accountType As String = args.NameValuePairs.GetValueOrDefault("account_costcenter", "CECO | TYPE") & " | "
							
'							If 							
								Dim costcenter As String = costcenter_accountType.Split(" | ")(0)
								Dim account_type As String = costcenter_accountType.Split(" | ")(1)

							Dim sql As String = String.Empty
							Dim dt As New DataTable
							
							'BRAPI.ErrorLog.LogMessage(si, costcenter_accountType)
	
							sql = $"
								WITH masterAccounts AS (
								    SELECT 
								        id AS mng_account,
								        account_type
								    FROM XFC_PLT_MASTER_Account
								)
								SELECT *
								FROM (
								    SELECT 
								        A.id_averagegroup,
								        A.id_product,
								        'M' + RIGHT('0' + CAST(MONTH(A.date) AS VARCHAR), 2) AS month_format, -- Formatea el mes como M01, M02, etc.
								        A.value
								    FROM XFC_PLT_FACT_CostsDistribution A
								    LEFT JOIN masterAccounts B
								        ON A.id_account = B.mng_account
								    WHERE 1=1
								        AND YEAR(A.date) = {year}
								        AND id_factory = '{factory}'
								        AND type_tso_taj = '{tso_taj}'
								        AND id_costcenter = '{costcenter}'
								        AND account_type = '{account_type}'
										AND scenario = '{scenario}'
										AND A.value <> 0
								) AS SourceTable
								PIVOT (
								    SUM(value) 
								    FOR month_format IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
								) AS PivotTable
								ORDER BY id_averagegroup, id_product;

							"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
							
	#End Region	
	
	#Region "Validation Variables All"							
						Else If args.DataSetName.XFEqualsIgnoreCase("SharedCosts_Validation") Then
							
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							Dim factory As String = args.NameValuePairs("factory")
							Dim cecoType As String = args.NameValuePairs("ceco")
							
							Dim sql As String = String.Empty
							Dim sql2 As String = String.Empty
							Dim dt As New DataTable
							
							Dim tableSelected As String = "-"
							
							sql = "SELECT 'Select a CeCo Type' as Message"
'							brapi.ErrorLog.LogMessage(si, cecoType)
							
							If cecoType = "All" Then 
								sql = "SELECT 'Select a CeCo Type' as Message"
							Else
								If cecoType = "CP" Then
									tableSelected = "CP"
								Else If cecoType = "CAT" Then
									tableSelected = "CAT"
								Else If cecoType = "CAA" Then
									tableSelected = "CAA"
								End If								
								
								#Region "Old"
									sql = shared_queries.AllQueries(si, "DistribucionCostes", factory, month, year, scenario)	
									sql = sql & $"
									
									, porGM as (
										SELECT 
											nature
											, Origin
											, CeCo
											, id_account as Account
											, account_type as Type
											, Destination
											, TotalAmount as InitialCosts
											, SUM(ActivityWeight_TAJ) as CP_Key
											, KeyAllocation as CAT_Key
											, AuxiliarWeight_TAJ as CAA_Key
											, SUM(SharedCost_TAJ) as SharedCosts
									
										FROM lastTable
										
										WHERE nature = '{tableSelected}'
										
										GROUP BY nature
												, Origin
												, CeCo
												, id_account
												, account_type
												, Destination
												, TotalAmount
												, KeyAllocation
												, AuxiliarWeight_TAJ
									)
									
										SELECT 
											nature
											, Origin
											, CeCo
											, Account as Account
											, Type as Type
											, InitialCosts as InitialCosts
											, AVG(CP_Key) as CP_Key
											, CASE 
												WHEN nature = 'CAT' THEN SUM(CAT_Key)
												WHEN nature = 'CAA' or nature = 'CP_CAA' or nature = 'CAT_CAA' THEN SUM(CAA_Key)
												WHEN nature = 'CP' THEN 1
												ELSE -1000000
											END as OTH_Key
											, SUM(SharedCosts) as SharedCosts
											, InitialCosts-SUM(SharedCosts) as Diff
									
										FROM porGM
										
										GROUP BY nature
												, Origin
												, CeCo
												, Account
												, Type
												, InitialCosts
									"
								#End Region
								
								sql = shared_queries.AllQueries(si, $"CostsDistribution_{cecoType}", factory, month, year, scenario)	
								sql = sql & $"
								SELECT *
								
								FROM costsDistributionInsert
								"
							End If
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
	#End Region	
	
	#Region "Validación Insert"
						Else If args.DataSetName.XFEqualsIgnoreCase("SharedCosts_Validation_Insert") Then
							
							' Dim scenario As String = args.NameValuePairs("scenario")
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							Dim factory As String = args.NameValuePairs("factory")
							 Dim cecoType As String = args.NameValuePairs("ceco")
							
							Dim sql As String = String.Empty
							Dim sql2 As String = String.Empty
							Dim tableSelected As String = "(SELECT 'Select a CeCo Type' as Message) as msg"
							If cecoType = "CP" Then
								tableSelected = "cpSharedCosts"
							Else If cecoType = "CAT" Then
								tableSelected = "catSharedCosts"
							Else If cecoType = "CAA" Then
								tableSelected = "caaSharedCosts"
							Else
								tableSelected = "(SELECT 'Select a CeCo Type' as Message) as msg"
							End If
							
							
							Dim dt As New DataTable		
							
							#Region "OLD"
							sql = shared_queries.AllQueries(si, "DistribucionCostes", factory, month, year, scenario)														
							sql = sql & 
									$"								
									 	SELECT *
										
										FROM {tableSelected}
									"	
							#End Region
							
							sql = shared_queries.AllQueries(si, $"CostsDistribution_{cecoType}", factory, month, year, scenario)	
								sql = sql & $"
								SELECT *
								
								FROM {tableSelected}
								"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt	
	#End Region
	
	#Region "Migration Data"
							
						Else If args.DataSetName.XFEqualsIgnoreCase("SharedCosts_Migration") Then
							
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim year As String = args.NameValuePairs("year")
							Dim month As String = args.NameValuePairs("month")
							Dim factory As String = args.NameValuePairs("factory")
							
							Dim sql As String = String.Empty
							Dim dt As New DataTable
							
							sql = shared_queries.AllQueries(si, "DistribucionCostes", factory, month, year, scenario)	
							sql = sql & $"
											SELECT 
												'{scenario}' as scenario,
												DATEFROMPARTS({year}, {month}, 1) as date,
												'{factory}' as id_factory,
												id_account as id_account,
												CeCo as id_costcenter,
												destination as id_averagegroup,
												CASE 
													WHEN id_product IS NULL THEN '-1' 
													ELSE id_product
												END as id_product,
												SUM(SharedCost_TAJ) as value,
												'TAJ' as type_tso_taj
												
											FROM lastTable			
											
											GROUP BY id_account, CeCo, destination, id_product 							
							"
							
							sql = "
								SELECT *
								FROM XFC_PLT_FACT_CostsDistribution
							"
								
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	
							
							Return dt							

	#End Region		
	
#End Region

#Region "ActivityIndex"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("ActivityIndex") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = "Actual"					
						Dim scenario_ref As String = args.NameValuePairs("scenario_ref")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_IndexActivity = shared_queries.AllQueries(si, "ActivityIndexActual", factory, month, year, scenario, scenario_ref)													
							
						sql = sql_IndexActivity & $"
							
							SELECT *
							FROM activityIndex
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
												
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						Return dt
#End Region

#Region "Variance Analysis"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("VarianceAnalysis") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = args.NameValuePairs("scenario")				
						Dim scenario_ref As String = args.NameValuePairs("scenario_ref")
						Dim view As String = args.NameValuePairs("view")
						Dim currency As String = args.NameValuePairs("currency")
						Dim type As String = args.NameValuePairs.GetValueOrEmpty("type")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis", factory, month, year, scenario, scenario_ref, "", view, type, "", currency)	
						
						sql = sql_VarianceAnalysis & $"
							
							SELECT
						
						       	id_factory
						       	, id_account_type
								, desc_account_type
						       	, variability
								, CASE WHEN SUM([Reference]) <> 0 THEN SUM([Ref. 100% Adjusted]) / SUM([Reference]) * 100 END  [Activity Index]
						       	, SUM([Reference]) AS [Reference]
						       	, SUM([Ref. 100% Adjusted]) AS [Ref. 100% Adjusted]
						       	, SUM([Absorption of Fixed Costs]) AS [Absorption of Fixed Costs]
						       	, SUM([Ref. Semi Adjusted]) AS [Ref. Semi Adjusted]
						       	, SUM([Scope Change]) AS [Scope Change]
						       	, SUM([Ref. Adjusted]) AS [Ref. Adjusted]
						       	, SUM([Impact Parity]) AS [Impact Parity]
						       	, SUM([Ref. Adjusted at Actual Parity]) AS [Ref. Adjusted at {scenario} Parity]
						       	, SUM([Price/Salary Trend]) AS [Price/Salary Trend]
						       	, SUM([Price/Salary Trend AGS effect]) AS [Price/Salary Trend AGS effect]
						       	, SUM([Price/Salary Trend Mix population Effect]) AS [Price/Salary Trend Mix population Effect]
						       	, SUM([Price/Salary Trend Organization Effect]) AS [Price/Salary Trend Organization Effect]
						       	, SUM([Price/Salary Trend Noria Effect]) AS [Price/Salary Trend Noria Effect]
						       	, SUM([Price/Salary Trend Wages other]) AS [Price/Salary Trend Wages other]
						       	, SUM([Price/Salary Trend Electricity Effect]) AS [Price/Salary Trend Electricity Effect]
						       	, SUM([Price/Salary Trend Gaz Effect]) AS [Price/Salary Trend Gaz Effect]
						       	, SUM([Price/Salary Trend Price variance other than Elec/Gaz]) AS [Price/Salary Trend Price variance other than Elec/Gaz]
						       	, SUM([MEC]) AS [MEC]
						       	, SUM([Product/Process Evolution]) AS [Product/Process Evolution]
						       	, SUM([Excess Labour]) AS [Excess Labour]
						       	, SUM([Unemployement]) AS [Unemployement]
						       	, SUM([Productivity]) AS [Productivity]
						       	, SUM([Organization Productivity Effect]) AS [Organization Productivity Effect]
						       	, SUM([Hyper Competitiveness Productivity Effect]) AS [Hyper Competitiveness Productivity Effect]
						       	, SUM([COVID 19 Effect]) AS [COVID 19 Effect]
						       	, SUM([Extra Productivity Effect]) AS [Extra Productivity Effect]
						       	, SUM([GAP]) AS [GAP]
						       	, SUM([Actual at Actual Parity]) AS [{scenario} at {scenario} Parity]					
						
							FROM VarianceAnalysis
						
							GROUP BY 
								id_factory
					       		, id_account_type
								, desc_account_type
					       		, variability
								
							ORDER BY id_account_type, variability
						"		
						
						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						Return dt

	
#End Region

#Region "Variance Analysis Graficos"
		
		#Region "Completo y Gap"
		
					Else If args.DataSetName.XFEqualsIgnoreCase("VarianceAnalysisGraph") Then
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = args.NameValuePairs("scenario")					
						Dim scenario_ref As String = args.NameValuePairs("scenario_ref")
						Dim view As String = args.NameValuePairs("view")
						Dim currency As String = args.NameValuePairs("currency")						
						Dim account_type As String = args.NameValuePairs("account_type")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis", factory, month, year, scenario, scenario_ref, "", view, "", "", currency)	
							
						sql = sql_VarianceAnalysis & $"
							SELECT 
							       SUM(COALESCE([Reference], 0)) - SUM(COALESCE([Reference], 0)) AS [Reference]
								
								, (SUM(COALESCE([Reference], 0)) - SUM(COALESCE([Ref. Semi Adjusted], 0))) AS [Activity Effect]
								, (SUM(COALESCE([Ref. Semi Adjusted], 0)) - SUM(COALESCE([Ref. Adjusted at {scenario} Parity], 0))) AS [Parity Effect]
						
							    , SUM(COALESCE([Price/Salary Trend], 0)) AS [Price/Salary Trend]
							    , SUM(COALESCE([Price/Salary Trend AGS effect], 0)) AS [Price/Salary Trend AGS effect]
							    , SUM(COALESCE([Price/Salary Trend Mix population Effect], 0)) AS [Price/Salary Trend Mix population Effect]
							    , SUM(COALESCE([Price/Salary Trend Organization Effect], 0)) AS [Price/Salary Trend Organization Effect]
							    , SUM(COALESCE([Price/Salary Trend Noria Effect], 0)) AS [Price/Salary Trend Noria Effect]
							    , SUM(COALESCE([Price/Salary Trend Wages other], 0)) AS [Price/Salary Trend Wages other]
							    , SUM(COALESCE([Price/Salary Trend Electricity Effect], 0)) AS [Price/Salary Trend Electricity Effect]
							    , SUM(COALESCE([Price/Salary Trend Gaz Effect], 0)) AS [Price/Salary Trend Gaz Effect]
							    , SUM(COALESCE([Price/Salary Trend Price variance other than Elec/Gaz], 0)) AS [Price/Salary Trend Price variance other than Elec/Gaz]
							    , SUM(COALESCE([MEC], 0)) AS [MEC]
							    , SUM(COALESCE([Product/Process Evolution], 0)) AS [Product/Process Evolution]
							    , SUM(COALESCE([Excess Labour], 0)) AS [Excess Labour]
							    , SUM(COALESCE([Unemployement], 0)) AS [Unemployement]
							    , SUM(COALESCE([Productivity], 0)) AS [Productivity]
							    , SUM(COALESCE([Organization Productivity Effect], 0)) AS [Organization Productivity Effect]
							    , SUM(COALESCE([Hyper Competitiveness Productivity Effect], 0)) AS [Hyper Competitiveness Productivity Effect]
							    , SUM(COALESCE([COVID 19 Effect], 0)) AS [COVID 19 Effect]
							    , SUM(COALESCE([Extra Productivity Effect], 0)) AS [Extra Productivity Effect]
							    , SUM(COALESCE([GAP], 0)) AS [GAP]	
						
							    , (SUM(COALESCE([Reference], 0)) - SUM(COALESCE([Actual at Actual Parity], 0))) AS [{scenario} at {scenario} Parity]
						
							FROM VarianceAnalysis
						
							WHERE id_account_type = {account_type} OR {account_type} = 0
						"			
						
'						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						' return dt
						Dim seriesCollection As New XFSeriesCollection()
						Dim series As New XFSeries()
						
						series.UniqueName = "Variance Analisys"
						series.Type = XFChart2SeriesType.Waterfall
						series.BarWidth = 0.95					

						For Each row As DataRow In dt.Rows
							For Each column As DataColumn In dt.Columns
								Dim point As New XFSeriesPoint()
								If (IsDBNull(row(column.ColumnName)) OrElse row(column.ColumnName) = 0) Then
									If column.ColumnName = "Reference" Then
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
			#End Region
			
		#Region "Cost Type"
		
					Else If args.DataSetName.XFEqualsIgnoreCase("VarianceAnalysisGraph_CostType") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = args.NameValuePairs("scenario")				
						Dim scenario_ref As String = args.NameValuePairs("scenario_ref")
						Dim account_type As String = args.NameValuePairs("account_type")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_IndexActivity = shared_queries.AllQueries(si, "ActivityIndex", factory, month, year, scenario, scenario_ref)	
						Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysisOLD", factory, month, year, scenario, scenario_ref,"","")	
							
						sql = sql_IndexActivity	& sql_VarianceAnalysis & $"
							SELECT 
								id_account_type,
								desc_account_type,
								-- Cálculo del GAP
								SUM(COALESCE([Actual at Actual Parity], 0)) -
							    SUM(COALESCE([Reference], 0) 
							        - COALESCE([Price/Salary Trend], 0) 
							        - COALESCE([Price/Salary Trend AGS effect], 0) 
							        - COALESCE([Price/Salary Trend Mix population Effect], 0) 
							        - COALESCE([Price/Salary Trend Organization Effect], 0) 
							        - COALESCE([Price/Salary Trend Noria Effect], 0) 
							        - COALESCE([Price/Salary Trend Wages other], 0) 
							        - COALESCE([Price/Salary Trend Electricity Effect], 0) 
							        - COALESCE([Price/Salary Trend Gaz Effect], 0) 
							        - COALESCE([Price/Salary Trend Price variance other than Elec/Gaz], 0) 
							        - COALESCE([MEC], 0) 
							        - COALESCE([Product/Process Evolution], 0) 
							        - COALESCE([Excess Labour], 0) 
							        - COALESCE([Unemployement], 0) 
							        - COALESCE([Productivity], 0) 
							        - COALESCE([Organization Productivity Effect], 0) 
							        - COALESCE([Hyper Competitiveness Productivity Effect], 0) 
							        - COALESCE([COVID 19 Effect], 0) 
							        - COALESCE([Extra Productivity Effect], 0) 
							        - COALESCE([Productivity Other Effect], 0)
							    )  AS [GAP]
						
							FROM VarianceAnalysis
						
							WHERE id_account_type = {account_type} OR {account_type} = 0
						
							GROUP BY id_account_type, desc_account_type
						"			
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						' return dt
						Dim seriesCollection As New XFSeriesCollection()						
						Dim filasRepresentadas As New Dictionary(Of String, String) From {
							{"MOD","#FF002343"},
							{"MOS","#FF243B81"},
							{"FIP","#FF93A6E1"},
							{"Taxe","#FF7FC5F3"},
							{"Depreciation", "#FF1DA5B6"}
						}
						
						Dim sumRefAdj As Decimal = 0
						Dim sumAct As Decimal = 0
						
						For Each column As DataColumn In dt.Columns
							
							If column.ColumnName = "desc_account_type" Then						
																	
								For Each row As DataRow In dt.Rows
									If filasRepresentadas.ContainsKey(row(column.ColumnName)) Then
	
										Dim series As New XFSeries()
								
										series.UniqueName = row(column.ColumnName)
										series.Type = XFChart2SeriesType.BarSideBySideFullStacked
										series.BarWidth = 0.7
										series.ModelType = XFChart2ModelType.Bar2DFlatGlass
										series.Color = filasRepresentadas(row(column.ColumnName))
										series.LineThickness = 0
										
										Dim point1 As New XFSeriesPoint()						
										Dim point2 As New XFSeriesPoint()
										
										point1.Argument = "Ref. Adjusted at Actual Parity"
										point1.Value =  row("Ref. Adjusted at Actual Parity")
										series.AddPoint(point1)	
										
										point2.Argument = "{scenario} at Actual Parity"
										point2.Value =  row("Actual at Actual Parity")
										series.AddPoint(point2)
										
					
	
										seriesCollection.AddSeries(series)
										
									Else
										sumAct += row("Actual at Actual Parity")
										sumRefAdj += row("Ref. Adjusted at Actual Parity")
										
									End If
								Next
								
								If sumAct > 0 Or sumRefAdj > 0 Then
									Dim series As New XFSeries()
								
									series.UniqueName = "Others"
									series.Type = XFChart2SeriesType.BarSideBySideFullStacked
									series.BarWidth = 0.8
									series.Color = "#FF777777"
									
									Dim point1 As New XFSeriesPoint()				
									Dim point2 As New XFSeriesPoint()	
									
									point1.Argument = "Ref. Adjusted at Actual Parity"
									point1.Value =  sumRefAdj
									series.AddPoint(point2)	
									
									point2.Argument = "Actual at Actual Parity"
									point2.Value =  sumAct
									series.AddPoint(point1)									
									
									seriesCollection.AddSeries(series)
									
								End If
								
							End If
						Next

						
						Return seriesCollection.CreateDataSet(si)
						
		#End Region
		
#End Region

#Region "Masses"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("Masses") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = args.NameValuePairs("scenario")					
						Dim scenario_ref As String = args.NameValuePairs("scenario_ref")
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						'BRApi.ErrorLog.LogMessage(si, "currency: " & currency)
						
						Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis", factory, "12", year, scenario, scenario_ref, "", "YTD", "", "", currency)	
							
						sql = sql_VarianceAnalysis & $"
							
 							SELECT 
					       	A.id_factory AS [Id Factory]
					       	, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), A.month)), 2)) AS Month
					       	, A.id_account_type AS [Id Nature]
							, RIGHT('0' + CONVERT(VARCHAR,id_account_type),2) + ' - ' + N.description AS [Account Nature]
							, A.id_account AS [Id Account]
							, A.id_account + ' - ' + ISNULL(M.description,'No Description') AS [Account Description]
							, A.id_costcenter AS [Id Cost Center]
							--, A.id_costcenter + ' - ' + ISNULL(A.costcenter,'No Description') AS [Cost Center Description]
							, A.nature AS [Cost Center Nature]
							, C.type AS [Cost Center Type]
					       	, A.variability AS [Variability]
							, CASE WHEN SUM(ref_value) = 0 THEN 1
								ELSE SUM(ref_value_adj_100) / SUM(ref_value) END * 100 AS [Activity Index]
					       	, SUM(ref_value) * R2.rate AS [Reference]
					       	, SUM(ref_value_adj_100) * R2.rate AS [Ref 100 Adjusted]
					       	, SUM(ref_value_adj_semi) * R2.rate AS [Ref Semi Adjusted]
					       	, SUM(ref_value_adj_semi) * R2.rate AS [Ref Adjusted]
						
							, (SUM(ref_value_adj_semi) * R2.rate) 							
							+ ((SUM(ref_value_adj_semi_parity) * R1.rate * R1.rate_exception))							
								- ((SUM(ref_value_adj_semi_parity) * R2.rate)) AS [Ref Adjusted at {scenario} Parity]
						
					       	, SUM(act_value) * R1.rate AS [{scenario} at {scenario} Parity]
					       
							FROM (
									SELECT A.id_factory, A.month, A.year, A.id_account_type, A.id_account, A.Nature, A.id_costcenter, A.costcenter, A.variability
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi	
										, SUM(ref_value_adj_semi * ISNULL(P.value,1)) AS ref_value_adj_semi_parity
						   			FROM ActRefFinalData A
									LEFT JOIN ParityPercentages P
										ON A.id_factory = P.id_factory
										AND A.id_account = P.id_account
						   			GROUP BY A.id_factory, A.month, A.year, A.id_account_type, A.id_account, A.Nature, A.id_costcenter, A.costcenter, A.variability
								) A	

			             	LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON A.id_account_type = N.id   
						
			             	LEFT JOIN XFC_PLT_MASTER_Account M
			         			ON A.id_account = M.id  						
						
							LEFT JOIN fxRate R1
								ON A.month = R1.month
							LEFT JOIN fxRateRef R2
								ON A.month = R2.month	
						
							LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON A.id_factory  = C.id_factory
									AND A.id_costcenter = C.id
									AND C.scenario = 'Actual'
									AND EOMONTH(DATEFROMPARTS(A.year, A.Month, 1)) BETWEEN C.start_date AND C.end_date

        					GROUP BY A.id_factory, A.month, id_account_type, N.description, A.id_account, A.id_costcenter, A.costcenter, A.nature, M.description, R1.rate, R2.rate, R1.rate_exception, A.variability, C.type
						    
							ORDER BY A.id_factory, month, CONVERT(VARCHAR,id_account_type) + ' - ' + N.description, variability, id_account

						"
						
						BRApi.ErrorLog.LogMessage(si, sql)
												
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						Return dt

	
#End Region

#Region "Masses GM"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("MassesGM") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = args.NameValuePairs("scenario")				
						Dim scenario_ref As String = args.NameValuePairs("scenario_ref")
						Dim view As String = args.NameValuePairs("view")
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						Dim sql_VarianceAnalysis = shared_queries.AllQueries(si, "VarianceAnalysis", factory, month, year, scenario, scenario_ref, "", view, "", "", currency)	
							
						sql = sql_VarianceAnalysis & $"
							
 							SELECT 
					       	A.id_factory AS [Id Factory]
					       	, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), A.month)), 2)) AS Month
							, RIGHT('0' + CONVERT(VARCHAR,ISNULL(id_account_type,'99')),2) + ' - ' + ISNULL(N.description,'No Nature') AS [Nature]
							, A.id_averagegroup + ' - ' + ISNULL(M.description,'No Description') AS [GM]
							, CASE WHEN SUM(ref_value) = 0 THEN 1
								ELSE SUM(ref_value_adj_100) / SUM(ref_value) END * 100 AS [Activity Index]
					       	, SUM(ref_value) * R2.rate AS [Reference]
					       	, SUM(ref_value_adj_100) * R2.rate AS [Ref 100 Adjusted]
					       	, SUM(ref_value_adj_semi) * R2.rate AS [Ref Semi Adjusted]
					       	, SUM(ref_value_adj_semi) * R2.rate AS [Ref Adjusted]
						
							, (SUM(ref_value_adj_semi) * R2.rate) 							
							+ ((SUM(ref_value_adj_semi_parity) * R1.rate * R1.rate_exception))							
								- ((SUM(ref_value_adj_semi_parity) * R2.rate)) AS [Ref Adjusted at {scenario} Parity]
						
					       	, SUM(act_value) * R1.rate AS [{scenario} at {scenario} Parity]
					       
							FROM (
									SELECT A.id_factory, A.month, A.year, A.id_account_type, A.id_averagegroup
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi	
										, SUM(ref_value_adj_semi * ISNULL(P.value,1)) AS ref_value_adj_semi_parity
						   			FROM ActRefFinalData A
									LEFT JOIN ParityPercentages P
										ON A.id_factory = P.id_factory
										AND A.id_account = P.id_account
						   			GROUP BY A.id_factory, A.month, A.year, A.id_account_type, A.id_averagegroup
								) A	

			             	LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON A.id_account_type = N.id   
						
			             	LEFT JOIN XFC_PLT_MASTER_AverageGroup M
			         			ON A.id_averagegroup = M.id  						
						
							LEFT JOIN fxRate R1
								ON A.month = R1.month
							LEFT JOIN fxRateRef R2
								ON A.month = R2.month						

        					GROUP BY 
								A.id_factory
								, A.month
								, RIGHT('0' + CONVERT(VARCHAR,ISNULL(id_account_type,'99')),2) + ' - ' + ISNULL(N.description,'No Nature')
								, A.id_averagegroup + ' - ' + ISNULL(M.description,'No Description')
								, R1.rate
								, R2.rate
								, R1.rate_exception
						    
							ORDER BY 
								A.id_factory
								, A.month
								, RIGHT('0' + CONVERT(VARCHAR,ISNULL(id_account_type,'99')),2) + ' - ' + ISNULL(N.description,'No Nature')
								, A.id_averagegroup + ' - ' + ISNULL(M.description,'No Description')

						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
												
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						Return dt

	
#End Region

#Region "Masses Actual GM"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("Masses_GM") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = args.NameValuePairs("scenario")									
						Dim scenario_ref As String = args.NameValuePairs("scenario_ref")
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						'BRApi.ErrorLog.LogMessage(si, "currency: " & currency)
						
						sql = $"
							
 							SELECT 
								F.id_factory
							 	, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(F.date))), 2)) AS Month
								, F.id_averagegroup AS [Id GM]
								, F.id_averagegroup + ' - ' + ISNULL(G.description,'No Description') AS [GM]
								, F.id_product AS [Id Product]
								, SUM(F.value) AS Value 					      
								     
							FROM XFC_PLT_FACT_CostsDistribution F				
						
			             	LEFT JOIN XFC_PLT_MASTER_AverageGroup G
			         			ON F.id_averagegroup = G.id  												
						
							WHERE 1=1
							AND F.scenario = 'Actual'
							AND YEAR(F.date) = {year} 
							AND F.id_factory = '{factory}'		
				          	--AND C.type = 'A'						

        					GROUP BY 
								F.id_factory
						    	, CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(F.date))), 2))
								, F.id_averagegroup
								, G.description
								, F.id_product
						
							ORDER BY F.id_factory, Month, F.id_averagegroup, F.id_product
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
												
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						Return dt

	
#End Region

#Region "Energy Variance"
					Else If args.DataSetName.XFEqualsIgnoreCase("EnergyVariance") Then
						
						Dim dt_graph As String = args.NameValuePairs.GetValueOrDefault("dt_graph", "dt")
						
						Dim factory As String = args.NameValuePairs("factory")
						Dim year As String = args.NameValuePairs("year")					
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim scenarioRef As String = args.NameValuePairs("scenarioRef")
						Dim view As String = args.NameValuePairs("view")
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						sql = shared_queries.AllQueries(si, "EnergyVariance", factory, , year, scenario, scenarioRef, "All", view,,,currency) & "						
						
							SELECT *
							FROM EnergyVariance
							ORDER BY month asc, Energy asc
						"
''						brapi.ErrorLog.LogMessage(si, sql)
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						
						return dt
#End Region

#Region "Change Perim"

					Else If args.DataSetName.XFEqualsIgnoreCase("ChangePerim") Then
						
						Dim dt_graph As String = args.NameValuePairs.GetValueOrDefault("dt_graph", "dt")
						
						Dim factory As String = args.NameValuePairs("factory")
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")						
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim scenarioRef As String = args.NameValuePairs("scenarioRef")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
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
						
						WHERE A.id_factory = '{factory}'
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
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						
						Return dt
#End Region

#Region "VTU - Main"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Main") Then		
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")						
						Dim currency As String = args.NameValuePairs("currency")
												
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VTU = If(view = "Last_12_Months","VTU_data_aux_tables","VTU_data_aux_tables")	
						
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, factory, month, year, scenario, "", "", view, type, "", currency)																														
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
						
						Dim view_col As String = If(view = "Periodic","VTU_Periodic","VTU_YTD")
						
						sql = $"	
							
							{sql_VTU_data}
						
								, OwnRowsFilter AS (
									SELECT DISTINCT Month, id_product
									FROM OwnRows
									WHERE {view_col} <> 0
								)							

								/* --- QUERY --- */
							
								SELECT id_product, desc_product, SUM(VTU) AS VTU, SUM(VTU_Fixed) AS VTU_Fixed, SUM(VTU_Variable) AS VTU_Variable FROM (
						
								SELECT id_product
									, M.description AS desc_product
									, SUM({view_col}) AS VTU
									, CASE WHEN Q.variability = 'F' THEN SUM({view_col}) END AS VTU_Fixed
									, CASE WHEN Q.variability = 'V' THEN SUM({view_col}) END AS VTU_Variable
							
								FROM (
								    SELECT * FROM OwnRows WHERE Month = {month}
								    UNION ALL
								    SELECT C.* 
									FROM CompRows C
									INNER JOIN OwnRowsFilter O
										ON C.month = O.month
										AND C.id_product = O.id_product
									WHERE C.Month = {month}
								) AS Q
							
								LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
							
								GROUP BY Q.id_product, M.description, Q.variability
								
								HAVING SUM({view_col}) <> 0 ) AS RES
						
								GROUP BY id_product, desc_product
						
								ORDER BY VTU DESC	
						"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region		

#Region "VTU - Components"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Components") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")							
						Dim currency As String = args.NameValuePairs("currency")						
						Dim product As String = args.NameValuePairs("product")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						Dim sql_VTU = If(view = "Last_12_Months","VTU_data_aux_tables","VTU_data_aux_tables")						
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, factory, month, year, scenario, "", "", view, type, "", currency)																								
						
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency, "Yes")
						
						' Dim view_VTU As String = If(view = "Periodic","VTU_Periodic","VTU_YTD")
						Dim view_VTU As String = If(view = "Periodic","VTU_Periodic","VTU_YTD")
						' Dim view_Cost As String = If(view = "Periodic","ISNULL(sum_fix_per,0) + ISNULL(sum_var_per,0)","ISNULL(sum_fix_ytd,0) + ISNULL(sum_var_ytd,0)")
						Dim view_Cost As String = If(view = "Periodic","ISNULL(cost_fixed_no_coef_per,0) + ISNULL(cost_variable_no_coef_per,0)","ISNULL(sum_fix_ytd,0) + ISNULL(sum_var_ytd,0)")
						
						sql = $"	
							
							{sql_VTU_data}
						

								/* --- QUERY --- */
							
								, data AS (
						
									SELECT id_product, id_component, desc_product, level, SUM(cost) AS cost, SUM(VTU_pond_unit) AS VTU_Unit, exp_coefficient, SUM(VTU_pond) AS VTU FROM (
							
									SELECT -- Q.id_product
									    CASE WHEN CHARINDEX('_', REVERSE(Q.id_component_parents)) - 1 = -1 THEN Q.id_component_parents
	       									   ELSE RIGHT(Q.id_component_parents, CHARINDEX('_', REVERSE(Q.id_component_parents)) - 1) END AS id_product
										, Q.id_component
										, M.description AS desc_product
										, Q.level
										, {view_Cost} AS cost
										-- , {view_VTU}_Unit AS VTU_pond_Unit		
										, {view_VTU} / Q.exp_coefficient AS VTU_pond_Unit		
										, Q.exp_coefficient
										, {view_VTU} AS VTU_pond
								
									FROM (
									    SELECT * FROM OwnRows WHERE Month = {month} AND id_product = '{product}'
									    UNION ALL
									    SELECT * FROM CompRows WHERE Month = {month} AND id_product = '{product}'
									) AS Q
								
									LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = ISNULL(Q.id_component,Q.id_product)						
									
									) AS RES
							
									GROUP BY id_product, id_component, desc_product, level, exp_coefficient 
									HAVING SUM(VTU_pond) <> 0	
								)	
						
								SELECT 
									'TOTAL:' AS id_product, NULL AS id_component, NULL AS desc_product, NULL AS level
									, NULL AS cost 
									, NULL AS volume, NULL AS VTU_Unit
									, NULL AS exp_coefficient, SUM(VTU) AS VTU  
								FROM data
							
								UNION ALL
							
								SELECT 
									D.id_product, D.id_component, D.desc_product, D.level
									, D.cost/4 
									, D.cost /4 / D.VTU_Unit AS Volume, D.VTU_Unit
									, D.exp_coefficient, D.VTU 
								FROM data D
								ORDER BY level, id_component
						"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region

#Region "VTU - GM"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_GM") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")							
						Dim currency As String = args.NameValuePairs("currency")						
						Dim product As String = args.NameValuePairs("product")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VTU = If(view = "Last_12_Months","VTU_data_aux_tables","VTU_data_aux_tables")						
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, factory, month, year, scenario, "", "", view, type, "", currency)																								
						
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency, "Yes")
						
						Dim view_VTU As String = If(view = "Periodic","VTU_Periodic","VTU_YTD")
						Dim view_Activity As String = If(view = "Periodic","activity_UO1_per","activity_UO1_ytd")
						
						sql = sql_VTU_data & $"	
							
								/* --- QUERY --- */

								, data AS (
						
									SELECT 
										id_product, id_averagegroup, description 
										, SUM(activity) AS activity
										, SUM(VTU_pond) AS VTU_pond
									FROM (
						
									SELECT Q.id_component AS id_product
								       	, Q.id_averagegroup
								       	, A.description
										, {view_Activity} AS activity
										, {view_VTU} AS VTU_pond
								
									FROM (
									    SELECT * FROM OwnRows WHERE Month = {month} AND id_product = '{product}'
									    UNION ALL
									    SELECT * FROM CompRows WHERE Month = {month} AND id_product = '{product}'
									) AS Q
								
									LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = ISNULL(Q.id_component,Q.id_product)						
									LEFT JOIN XFC_PLT_MASTER_averagegroup A ON A.id	= Q.id_averagegroup AND A.id_factory = '{factory}' 
									
									) AS RES
							
									GROUP BY id_product, id_averagegroup, description
									HAVING SUM(VTU_pond) <> 0	
						
								)
							
								SELECT 'TOTAL:' AS id_product, NULL AS id_averagegroup, NULL AS description, NULL AS activity, SUM(VTU_pond) AS VTU_pond, 1 AS orden
								FROM data
							
								UNION ALL
							
								SELECT id_product, id_averagegroup, description, activity, VTU_Pond, 2 AS orden
								FROM data 
								ORDER BY orden, id_product, id_averagegroup			

						"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region

#Region "VTU - GM Without Activity"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_GM_WO_Act") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")							
						Dim currency As String = args.NameValuePairs("currency")						
						Dim product As String = args.NameValuePairs("product")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VTU = If(view = "Last_12_Months","VTU_data_aux_tables","VTU_data_aux_tables")						
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, factory, month, year, scenario, "", "", view, type, "", currency)																								
						
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
						
						Dim view_VTU As String = If(view = "Periodic","VTU_Periodic","VTU_YTD")
						
						sql = sql_VTU_data & $"	
							
								/* --- QUERY --- */
							
								, data AS (
						
									SELECT id_averagegroup, description, SUM(VTU_pond) AS VTU_pond
									FROM (
						
									SELECT Q.id_product
								       	, Q.id_averagegroup
								       	, A.description
										, {view_VTU} AS VTU_pond
								
									FROM (
									    SELECT * FROM OwnRows WHERE Month = {month} AND id_product = '{product}'
									    UNION ALL
									    SELECT * FROM CompRows WHERE Month = {month} AND id_product = '{product}'
									) AS Q
								
									LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = ISNULL(Q.id_component,Q.id_product)						
									LEFT JOIN XFC_PLT_MASTER_averagegroup A ON A.id	= Q.id_averagegroup AND A.id_factory = '{factory}' 
									
									) AS RES
							
									GROUP BY id_averagegroup, description
									HAVING SUM(VTU_pond) <> 0	
						
								)
							
								SELECT 'TOTAL:' AS id_averagegroup, NULL AS description, SUM(VTU_pond) AS VTU_pond, 1 AS orden
								FROM data
							
								UNION ALL
							
								SELECT id_averagegroup, description, VTU_Pond, 2 AS orden
								FROM data 
								ORDER BY orden, id_averagegroup			

						"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region

#Region "VTU - GM & Nature"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_GM_Nature") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")							
						Dim currency As String = args.NameValuePairs("currency")						
						Dim product As String = args.NameValuePairs("product")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VTU = IIf(view = "Last_12_Months","VTU_12_Months","VTU_data")					
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, factory, month, year, scenario, "", "", view, type, "", currency)
							
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
						
						sql = sql_VTU_data & $"	
							
							, data AS (
						
							-- MAIN QUERY
							
							SELECT id_product, id_averagegroup, description, account_type, SUM(VTU) AS VTU, activity, SUM(VTU_pond) AS VTU_pond
							FROM (
							
							-- VTU PRODUCTO
							
							SELECT
							      F.id_product 
							      ,F.id_averagegroup
							      ,A.description
								  ,F.account_type
							      ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0
									 ELSE F.cost / F.volume END AS VTU
							      ,F.activity_UO1 AS activity
							      ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							 		 ELSE F.cost / F.volume
									* F.activity_UO1 / F.activity_UO1_total 
								END AS VTU_pond
							
							FROM (SELECT '{product}' AS Id) P
							LEFT JOIN factVTU F
								ON P.id = F.id_product						
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON F.id_product = M.id	
							LEFT JOIN XFC_PLT_MASTER_averagegroup A
								ON F.id_averagegroup = A.id		
								AND A.id_factory = '{factory}' 
							
							UNION ALL
							
							-- VTU COMPONENTS
							
							SELECT   
						          	F.id_product 
						          	,F.id_averagegroup
						          	,A.description
									,F.account_type
						          	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							 			  ELSE F.cost / F.volume 
										* P.exp_coefficient 
										END AS VTU 
						          	,F.activity_UO1 AS allocation
						          	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							  		      ELSE F.cost / F.volume	
										* P.exp_coefficient
							  			* F.activity_UO1 / F.activity_UO1_total 
							 			END AS VTU_pond
							
							FROM (	SELECT * 
									FROM Product_Component_Recursivity
									WHERE id_product_final = '{product}') P						
							LEFT JOIN factVTU F
								ON F.id_product = P.id_component					
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON F.id_product = M.id	
							LEFT JOIN XFC_PLT_MASTER_averagegroup A
								ON F.id_averagegroup = A.id
								AND A.id_factory = '{factory}' 
							
							) AS RES														
							
							WHERE VTU <> 0
						
							GROUP BY id_product, id_averagegroup, account_type,description, activity
						
							)
						
							-- SELECT 'TOTAL:' AS id_product, NULL AS id_averagegroup, NULL AS description, NULL AS account_type, SUM(VTU) AS VTU, NULL AS activity, SUM(VTU_pond) AS VTU_pond, 1 AS orden
							-- FROM data
							-- 
							-- UNION ALL
						
							SELECT id_product, id_averagegroup, description, account_type, VTU, activity, VTU_Pond, 2 AS orden
							FROM data 
							ORDER BY orden, id_product, id_averagegroup, CAST(account_type AS INT)				

						"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region

#Region "VTU - Nature"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Nature") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						'Dim type As String = args.NameValuePairs("type")
						Dim type As String = "TAJ"
						Dim scenario As String = args.NameValuePairs("scenario")	
						Dim currency As String = args.NameValuePairs("currency")
						Dim product As String = args.NameValuePairs("product")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VTU = IIf(view = "Last_12_Months","VTU_12_Months","VTU_data")						
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, factory, month, year, scenario, "", "", view, type, "", currency)
						
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
						
						sql = sql_VTU_data & $"	
							
							-- MAIN QUERY
						
							,main AS (
						
								SELECT account_type, SUM(VTU_pond) AS VTU
								FROM (
								
								-- VTU PRODUCTO
								
								SELECT 
								  	account_type
								    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								          ELSE F.cost / F.volume
								       			
								       	  * F.activity_UO1 / F.activity_UO1_total 
								 	END AS VTU_pond
								
								FROM (SELECT '{product}' AS Id) P
								LEFT JOIN factVTU F
									ON P.id = F.id_product						
								
								UNION ALL
								
								-- VTU COMPONENTS
								
								SELECT   
								   F.account_type
								  ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								       	ELSE F.cost / F.volume								    
								       	* F.activity_UO1 / F.activity_UO1_total 
								 		* P.exp_coefficient END AS VTU_pond
						
								FROM (	SELECT * 
										FROM Product_Component_Recursivity
										WHERE id_product_final = '{product}') P						
								LEFT JOIN factVTU F
									ON F.id_product = P.id_component					
																
								) AS RES
								
								GROUP BY account_type 
							)

							SELECT 
								account_type AS id_account_type
								, CASE WHEN account_type = 1 THEN 'MOD' 
									WHEN account_type = 2 THEN 'MOS' 
							  		WHEN account_type = 3 THEN 'FIP' 
							    		WHEN account_type = 4 THEN 'IT' 
							    		WHEN account_type = 5 THEN 'Amo' 
									ELSE '- '
								END AS account_type
								, VTU
							FROM main
							WHERE VTU > 0
							ORDER BY id_account_type 

						"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region

#Region "VTU - Cost Detail"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_CostDetail") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")	
						Dim currency As String = args.NameValuePairs("currency")
						Dim product As String = args.NameValuePairs("product")
	
						Dim month_first_forecast As String = If(scenario.StartsWith("RF"), BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_month_first_forecast"), 1)
												
						Dim filterMonth As String = If(view = "Periodic",$"AND MONTH(date) = {month}",$"AND MONTH(date) <= {month}")						
						Dim view_col As String = If(view = "Periodic","VTU_Periodic","VTU_YTD")
						Dim sql As String = String.Empty
						Dim dt As New DataTable									
													
						#Region "VTU Calculation"
												
						' Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables_cc", factory, month, year, scenario, "", "", "", "Existing", product, currency)
						
						Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency,"No","Yes")
						
						sql = $"					
						
							{sqlVTU}
						
					       , PercVarAccount AS (
					       
								SELECT DISTINCT MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								{filterMonth}
							
								AND YEAR(V.date) = {year} 						
								AND id_factory = '{factory}'
								AND scenario = '{scenario}'
					       )							
							
							
							,factCost as (
								   
								SELECT
									CASE WHEN MONTH(F.date) >= 6 THEN ISNULL(PI.id_product_mapping, F.id_product) ELSE F.Id_Product END AS id_product
									,F.id_averagegroup
									,F.id_account
									,F.id_costcenter
									,C.nature AS nature_cc
									,SUM(F.value * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * ISNULL(P.value,0)) / 100 AS cost_variable
									,SUM(F.value) as cost 																	
						
						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
						        INNER JOIN XFC_PLT_FACT_CostsDistribution F 
						           	ON F.id_costcenter = C.id										
						
								LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
									ON  PI.id_product = F.id_product
									AND PI.scenario   = '{scenario}'
									AND PI.year       = '{year}'
									AND PI.id_factory = '{factory}'						
						
								LEFT JOIN XFC_PLT_MASTER_Account A
									ON F.id_account = A.id     				
						
								LEFT JOIN PercVarAccount P
									ON F.id_account = P.id_account						          	
									AND MONTH(F.date) = P.month					
								
								WHERE 1=1
									AND F.scenario = '{scenario}'		
									AND F.id_factory = '{factory}'
									AND YEAR(F.date) = {year}		
									{filterMonth}
									AND F.value <> 0
								
								GROUP BY 
									CASE WHEN MONTH(F.date) >= 6 THEN ISNULL(PI.id_product_mapping, F.id_product) ELSE F.Id_Product END
									,F.id_averagegroup
									,F.id_account
									,F.id_costcenter
									,C.nature
							)
						
							, factCC AS (
								SELECT
								    C.id_product,
								    C.id_account,
								    C.id_averagegroup,
								    C.Id_costcenter,
								    C.nature_cc,
								    SUM(C.cost_fixed) AS cost_fixed,
								    SUM(C.cost_variable) AS cost_variable,
								    SUM(C.cost) AS cost,
								    -- Agregación de centros de coste
								    SUM(SUM(C.cost_fixed)) OVER (PARTITION BY C.id_product, C.id_account, C.id_averagegroup) AS cost_fixed_total,
								    SUM(SUM(C.cost_variable)) OVER (PARTITION BY C.id_product, C.id_account, C.id_averagegroup) AS cost_variable_total,
								    SUM(SUM(C.cost)) OVER (PARTITION BY C.id_product, C.id_account, C.id_averagegroup) AS cost_total
								
								FROM factCost C
								
								GROUP BY
								    C.id_product,
								    C.id_account,
								    C.id_averagegroup,
								    C.Id_costcenter,
									C.nature_cc
							) 
						
							, factCCPerc AS (
								SELECT
						            id_product,
						            id_account,
						            id_averagegroup,
						            Id_costcenter,
						            nature_cc,      
									CASE WHEN cost_fixed_total <> 0 THEN cost_fixed / cost_fixed_total END AS perc_fixed,
									CASE WHEN cost_variable_total <> 0 THEN cost_variable / cost_variable_total END AS perc_variable,
									CASE WHEN cost_total <> 0 THEN cost / cost_total END AS perc
								FROM factCC 
							)
						
							SELECT
								Q.id_product AS [Id Product]
								, Q.id_component AS [Id Component]
								, Q.id_averagegroup AS [Id GM]
								, Q.id_account AS [Id Account]
								, Q.id_account + ' - ' + ISNULL(M.description,'No Description') AS [Account]						
						       	, Q.account_type AS [Id Nature]
								, RIGHT('0' + CONVERT(VARCHAR,Q.account_type),2) + ' - ' + N.description AS [Nature]
								, C.id_costcenter AS [Id CostCenter]
								, C.nature_cc AS [Nature Cost Center]										
						
								, SUM({view_col}) * C.perc AS VTU
								, CASE WHEN Q.variability = 'F' THEN SUM({view_col}) * C.perc_fixed END AS [VTU Fixed]
								, CASE WHEN Q.variability = 'V' THEN SUM({view_col}) * C.perc_variable END AS [VTU Variable]
      
							FROM (
							    SELECT * FROM OwnRows
							    UNION ALL
							    SELECT * FROM CompRows
							) AS Q
						
							LEFT JOIN factCCPerc C
								ON C.id_product = Q.id_component
								AND C.id_averagegroup = Q.id_averagegroup						
								AND C.id_account = Q.id_account	
						
							LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON N.id = Q.account_type
						
			             	LEFT JOIN XFC_PLT_MASTER_Account M
			         			ON M.id = Q.id_account						
							
							WHERE 1=1
								AND Q.id_product = '{product}'
								AND month = {month}
						
       						GROUP BY 
								Q.id_product
								, Q.id_component
								, Q.id_account
								, Q.account_type
								, Q.id_averagegroup
								, C.id_costcenter
								, C.nature_cc
								, Q.variability 
								, N.description
								, M.description
								, C.perc
								, C.perc_fixed
								, C.perc_variable
						
							HAVING SUM({view_col}) <> 0
						
							ORDER BY Q.id_product, Q.id_component, Q.id_account;
						
							"
						
					#End Region						
						
						' dt = ExecuteSQLBIBlend(si, sql) -- Traer la tabla de distribución de costes antes de descomentar
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using					
						
						Return dt						
						
#End Region

#Region "VTU - Cost Detail OLD"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_CostDetail_OLD") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")	
						Dim currency As String = args.NameValuePairs("currency")
						Dim product As String = args.NameValuePairs("product")
						
						Dim month_first_forecast As String = If(scenario.StartsWith("RF"), BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_month_first_forecast"), 1)
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
													
						Dim sql_VTU = IIf(view = "Last_12_Months","VTU_12_Months","VTU_data_Month")							
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, factory, month, year, scenario, "", "", view, type, product, currency)
						
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)						
						Dim filterMonth As String = $"AND MONTH(date) = {month}"
						
						#Region "Old Calc"
						sql = sql_VTU_data & $"								
						
							, PivotMapping2 AS (
						
								SELECT P.*, M.MonthPivot 						
								FROM XFC_PLT_AUX_Product_Pivot P
						
								CROSS JOIN (	SELECT 6 As MonthPivot UNION ALL SELECT 7  UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M  
						
								WHERE scenario = '{scenario}'
								AND year = '{year}'
								AND id_factory = '{factory}' 
							)		
						
							, NomenclaturePivotMapping AS (
						
								SELECT N.*, ISNULL(P.id_product_mapping, N.id_component) AS id_product_component						
       							FROM XFC_PLT_HIER_Nomenclature_Date_Report N
						
								CROSS JOIN (	SELECT 1 As MonthForPivot UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5 UNION ALL SELECT 6 UNION ALL SELECT 7  UNION ALL SELECT 8 UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M  
						
								LEFT JOIN PivotMapping2 P
									ON P.id_product = N.id_component
									AND P.MonthPivot = M.MonthForPivot 
						
								WHERE N.scenario = 'Actual' 
								AND N.year = '{year}' 
								AND N.id_factory = '{factory}' 
								AND N.id_product_final = '{product}'
								AND MONTH(N.date) = {month}
							)		
						
							, Nomenclature AS (
						
								SELECT N.*			
       							FROM XFC_PLT_HIER_Nomenclature_Date_Report N
								WHERE N.scenario = 'Actual' 
								AND N.year = '{year}' 
								AND N.id_factory = '{factory}' 
								AND N.id_product_final = '{product}'
								AND MONTH(N.date) = {month}
							)	
						
							, PivotMapping AS (
						
								SELECT P.*				
								FROM XFC_PLT_AUX_Product_Pivot P
						
								WHERE scenario = '{scenario}'
								AND year = '{year}'
								AND id_factory = '{factory}' 
							)							
						
						
							-- MAIN QUERY
						
							SELECT 
						       	RES.account_type AS [Id Nature]
								, RIGHT('0' + CONVERT(VARCHAR,RES.account_type),2) + ' - ' + N.description AS [Nature]
								, RES.id_account AS [Id Account]
								, RES.id_account + ' - ' + ISNULL(M.description,'No Description') AS [Account]
								, cost_center AS [Id Cost Center]
								, type_cc AS [Type Cost Center]
								, nature_cc AS [Nature Cost Center]
								, SUM(VTU_fixed) AS [VTU Fixed]
								, SUM(VTU_variable) AS [VTU Variable]
								, SUM(VTU_fixed + VTU_variable) AS VTU
							FROM (
							
							-- VTU PRODUCTO
							
								SELECT 
								    F.account_type 
								 	,F.id_account
								 	,F.nature
								 	,F.cost_center
									,F.type_cc
									,F.nature_cc
							        ,F.id_product
							        ,F.id_averagegroup						
								 	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								       	  ELSE F.cost_fixed / F.volume
								       	* F.activity_UO1 / F.activity_UO1_total 
								 	END AS VTU_fixed
								 	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								       	  ELSE F.cost_variable / F.volume
								       	* F.activity_UO1 / F.activity_UO1_total 
								 	END AS VTU_variable
							
								
								FROM (SELECT '{product}' AS Id) P
								LEFT JOIN factVTU F
									ON P.id = F.id_product						
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON F.id_product = M.id	
								
								UNION ALL
								
								-- VTU COMPONENTS
								
								SELECT   
								    F.account_type
								  	,F.id_account
								  	,F.nature
								  	,F.cost_center
									,F.type_cc
									,F.nature_cc	
							        ,F.id_product
							        ,F.id_averagegroup						
								    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								       	  ELSE F.cost_fixed / F.volume
								       	* F.activity_UO1 / F.activity_UO1_total 
								 		* P.exp_coefficient 
									END AS VTU_fixed
								    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								       	  ELSE F.cost_variable / F.volume
								       	* F.activity_UO1 / F.activity_UO1_total 
								 		* P.exp_coefficient 
									END AS VTU_variable						
						
								FROM Nomenclature P				
							
								LEFT JOIN PivotMapping PI
									ON P.id_component = PI.id_product	
						
								LEFT JOIN factVTU F
									ON F.id_product = ISNULL(PI.id_product_mapping, P.id_component)	
						
									--ON F.id_product = P.id_product_component		
									--AND MONTH(P.date) = F.month		
						
									--ON F.id_product = CASE WHEN F.month >= 6
							        --                       THEN ISNULL(PI.id_product_mapping, P.id_component)
							        --                            ELSE P.id_component END
							        --AND MONTH(P.date) = F.month						
										
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON F.id_product = M.id	
								
								) AS RES
						
							LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON RES.account_type = N.id   
						
			             	LEFT JOIN XFC_PLT_MASTER_Account M
			         			ON RES.id_account = M.id  	
							
							GROUP BY RES.account_type, id_account, nature, cost_center, type_cc, nature_cc, M.description, N.description

						"
						#End Region
						
						sql = $"SELECT 'Working on Changes' AS Message"
							
						sql = $"
							
							{sql_VTU_data}
						
							, finalVTU_Report as (
							/* --- QUERY --- */
							
							SELECT
									scenario
									, id_factory
									, FA.description AS desc_factory
									, month										
									, id_product
									, M.description AS desc_product	
									, id_component
									, account_type
									, variability
									, id_averagegroup
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
						
							GROUP BY 
									scenario
									, id_factory
									, month
									, M.description
									, FA.description
									, id_product
									, id_component
									, account_type
									, variability
									, id_averagegroup 
									, id_account

							)
						
							SELECT *
							FROM finalVTU_Report
							WHERE 1=1
								AND VTU_YTD <> 0
								AND id_product = '{product}'
							ORDER BY id_factory, month, id_product
							"
						
						' dt = ExecuteSQLBIBlend(si, sql)
							
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using					
						
						Return dt						
						
#End Region

#Region "VTU - All Factories"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Main_All_Factories") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim view As String = args.NameValuePairs("view")
						'Dim type As String = args.NameValuePairs("type")
						Dim type As String = "TAJ"
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim factory As String = String.Empty
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VTU = IIf(view = "Last_12_Months","VTU_12_Months","VTU_data_All_Factories")							
						' Dim sql_VTU_data As String = shared_queries.AllQueries(si, sql_VTU, "", month, year, scenario, "", "", view, type, "", currency)
						
						Dim sql_VTU_data As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
						
						sql = sql_VTU_data & $"	
							
							-- MAIN QUERY
							
							SELECT id_factory, id_product, desc_product, SUM(VTU) AS VTU, SUM(VTU_pond) AS VTU_pond
							FROM (
							
							-- VTU PRODUCTO
							
							SELECT
								F.id_factory
								,F.id_product AS id_product
								,M.description AS desc_product
							    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							   		ELSE F.cost / F.volume
								END AS VTU				
							    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							   		ELSE F.cost / F.volume
							   		* F.activity_UO1 / F.activity_UO1_total 
								END AS VTU_pond
							
							FROM factVTU F
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON F.id_product = M.id						
						
							UNION ALL
							
							-- VTU COMPONENTS
							
							SELECT  
								P.id_factory
								,P.id_product_final AS id_product
							 	,M.description AS desc_product
							    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							   		ELSE F.cost / F.volume
									* P.exp_coefficient
								END AS VTU									
							    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							     	ELSE F.cost / F.volume
							     	* F.activity_UO1 / F.activity_UO1_total 
									* P.exp_coefficient 
								END AS VTU_pond
							
							FROM Product_Component_Recursivity P
							LEFT JOIN factVTU F
								ON F.id_product = P.id_component 
								AND F.id_factory = P.id_factory
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON P.id_product_final = M.id								
						
							) AS RES
							
							GROUP BY id_factory, id_product, desc_product
							HAVING SUM(VTU_pond) <> 0
							ORDER BY id_factory, id_product
						"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region	

#Region "VTU - Monthly - Comp/GM/Nat"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Monthly_Comp_GM_Nat") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim dt As New DataTable
						' Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_aux_tables", "", "", year, scenario, "", "", "", "Existing", "", currency)
						Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
						Dim sql As String = $"
							
							{sqlVTU}
							
							/* --- QUERY --- */
						
							SELECT
								scenario
								, Q.id_factory AS [Id Factory]
								, FA.description AS [Desc Factory]
								, month	AS Month								
								, id_product AS [Id Product]
								, id_product + ' - ' + ISNULL(M.description,'') AS [Product]
								, id_component AS [Id Component]
								, id_component + ' - ' + ISNULL(C.description,'') AS [Component]
								, id_averagegroup AS [Id GM]
								, id_averagegroup + ' - ' + ISNULL(A.description,'') AS [GM]
								, CASE WHEN account_type IN (1,2,3,4,5)
							            THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + ' - ' + ISNULL(N.Description,'')
							            ELSE '99 - Others' END AS Nature
								
								, SUM(VTU_Periodic) AS [VTU Periodic]
								, SUM(VTU_YTD) AS [VTU YTD]
						
							FROM (
							    SELECT * FROM OwnRows
							    UNION ALL
							    SELECT * FROM CompRows
							) AS Q
						
							LEFT JOIN XFC_PLT_MASTER_Factory FA ON FA.id = Q.id_factory
							LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
							LEFT JOIN XFC_PLT_MASTER_Product C ON C.id  = Q.id_component
							LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type
							LEFT JOIN XFC_PLT_MASTER_AverageGroup A ON A.id = Q.id_averagegroup AND A.id_factory = '{factory}'
						
							WHERE Q.id_factory = '{factory}'
							
							GROUP BY scenario, Q.id_factory, month, M.description, FA.description,
							         id_product, id_component, C.description, id_averagegroup, A.description, 
										CASE WHEN account_type IN (1,2,3,4,5)
							            THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + ' - ' + ISNULL(N.Description,'')
							            ELSE '99 - Others' END 
						
							HAVING SUM(VTU_YTD) <> 0
						
							ORDER BY [Id Factory], Month, [Id Product], [Id GM], Nature
							"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region	

#Region "VTU - Monthly - Nat/Var/Acc"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Monthly_Nat_Var_Acc") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = 12
						Dim factory As String = args.NameValuePairs("factory")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim currency As String = args.NameValuePairs("currency")
						
						#Region "Fact VTU - Factory"
						
'						Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
'							BRApi.Database.ExecuteSql(dbcon,
'							$"
'								DROP TABLE IF EXISTS XFC_PLT_FACT_Costs_VTU_Report_{factory};
'								CREATE TABLE XFC_PLT_FACT_Costs_VTU_Report_{factory} (
'									[scenario] varchar(50)  NOT NULL,
'									[year] varchar(50)  NOT NULL,
'									[id_factory] varchar(50)  NOT NULL,
'									[date] date  NOT NULL,
'									[id_product] varchar(50)  NOT NULL,
'									[id_averagegroup] varchar(50)  NOT NULL,
'									[account_type] varchar(50)  NOT NULL,
'									[id_account] varchar(50)  NOT NULL,
'									[cost_fixed] decimal(18,6),
'									[cost_variable] decimal(18,6),
'									[cost] decimal(18,6),
'									[volume] decimal(18,6),
'									[activity_UO1] decimal(18,6),
'									[activity_UO1_total] decimal(18,6)
'								)
							
'							",True)						
						
'							Dim sql_VTU_data As String = shared_queries.AllQueries(si, "VTU_data_All_Factories_All_Months_AccountDetail", factory, month, year, scenario, "", "", "YTD", "Existing", "", currency)
''							BRApi.ErrorLog.LogMessage(si,sql_VTU_data)
'							BRApi.Database.ExecuteSql(dbcon,sql_VTU_data & 
'								$"
'								INSERT INTO XFC_PLT_FACT_Costs_VTU_Report_{factory} 
'									(
'									scenario
'									, year
'									, id_factory
'									, date
'									, id_product
'									, id_averagegroup
'									, account_type
'									, id_account
'									, cost_fixed
'									, cost_variable
'									, cost
'									, volume
'									, activity_UO1
'									, activity_UO1_total) 
								
'								SELECT
'									'{scenario}' as scenario
'									, YEAR(date) as year
'									, id_factory
'									, date
'									, id_product
'									, id_averagegroup
'									, account_type
'									, id_account
'									, cost_fixed
'									, cost_variable
'									, cost
'									, volume
'									, activity_UO1
'									, activity_UO1_total 
							
'								FROM factVTU
'								WHERE id_factory = '{factory}'								
'							",True)	
										
'							End Using
							
						#End Region						
						
						Dim dt As New DataTable
						' Dim sqlVTU As String = shared_queries.AllQueries(si, "VTU_data_accountDetail_aux_tables", factory, "", year, scenario, "", "", "", "Existing", "", currency)
						Dim sqlVTU As String = GetSQLDataVTU(si, scenario, month, factory, year, currency)
						Dim sql As String = $"
							
							{sqlVTU}
							
							SELECT  
									scenario
									, id_factory AS [Id Factory]
									, month AS Month
									, id_product AS [Id Product]
									, id_product + ' - ' + ISNULL(M.description,'') AS [Product]
									, CASE WHEN account_type IN (1,2,3,4,5)
							            THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + ' - ' + ISNULL(N.Description,'')
							            ELSE '99 - Others' END AS Nature
									, variability AS Variability
									, id_account AS [Id Account]
								   	, SUM(VTU_Periodic) AS [VTU Periodic]
									, SUM(VTU_YTD) AS [VTU YTD]
						
							FROM (
							    SELECT * FROM OwnRows
							    UNION ALL
							    SELECT * FROM CompRows
							) AS Q
						
							LEFT JOIN XFC_PLT_MASTER_Product M ON M.id  = Q.id_product
							LEFT JOIN XFC_PLT_MASTER_NatureCost N ON N.id = Q.account_type						
							
							WHERE 1=1
						
							GROUP BY scenario
									, id_factory
									, month
							        , id_product
									, M.description
									, CASE WHEN account_type IN (1,2,3,4,5)
							            THEN RIGHT('0' + CAST(account_type AS varchar(2)),2) + ' - ' + ISNULL(N.Description,'')
							            ELSE '99 - Others' END
									, variability
									, id_account
						
							HAVING SUM(VTU_YTD) <> 0
						
							ORDER BY id_factory, month, id_product, Nature, variability, id_account;
							"
						
						dt = ExecuteSQLBIBlend(si, sql)
						
'						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
'						End Using					
						
						Return dt						
						
#End Region	
						
#Region "Plan - Initial Version"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("Plan_Initial_Version") Then
						
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim factory As String = args.NameValuePairs("factory")
						Dim year As String = args.NameValuePairs("year")
						Dim account_type As String = args.NameValuePairs("account_type")
						Dim currency As String = args.NameValuePairs("currency")						
						
						Dim dt As DataTable
						Dim sql As String = String.Empty
						
						Dim sql_Rate = shared_queries.AllQueries(si, "RATE", factory, "0", year, scenario, "", "", "", "", "", currency)	
							
						sql = sql_Rate.Replace(", fxRateRef AS","WITH fxRateRef AS") & $"					
							SELECT
								 id_factory AS [Id Factory] 
								, 'M' + RIGHT('0' + CAST(MONTH(date) AS VARCHAR), 2) AS Month
								, scenario_ref AS [Scenario Ref] 
								, id_costcenter AS [Id Cost Center] 
								, id_averagegroup AS [Id GM] 
								, id_account AS [Id Account] 
								, id_account + ' - ' + ISNULL(A.description,'No Description') AS [Account]
								, value_ref * R.rate AS [Reference]
								, activity_index AS [Activity Index] 
								, value_100_adjusted * R.rate AS [{scenario} 100 Adjusted]
								, fixed_cost_absorption * R.rate AS [Fixed Cost Abs]
								, value_semi_adjusted * R.rate AS [{scenario} Semi Adj]
								, value_semi_adjusted_f * R.rate AS [{scenario} Semi Adj Fixed]
								, value_semi_adjusted_v * R.rate AS [{scenario} Semi Adj Variable]
							
							FROM XFC_PLT_AUX_Costs_Init_GM C
							
							LEFT JOIN XFC_PLT_MASTER_Account A
								ON C.id_account = A.id
						
							LEFT JOIN fxRate R
								ON MONTH(C.date) = R.month	
							
							WHERE scenario = '{scenario}' 
							AND YEAR(date) = {year} 
							AND id_factory = '{factory}' 
							AND A.account_type = '{account_type}'
							AND value_ref <> 0
							
							ORDER BY date, id_costcenter, id_averagegroup, id_account							
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using						
						
						Return dt

#End Region

#Region "Plan - Initial Version - Export"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("Plan_Initial_Version_Export") Then
						
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim factory As String = args.NameValuePairs("factory")
						Dim year As String = args.NameValuePairs("year")
						Dim currency As String = args.NameValuePairs("currency")						
						
						Dim dt As DataTable
						Dim sql As String = String.Empty
						
						Dim sql_Rate = shared_queries.AllQueries(si, "RATE", factory, "0", year, scenario, "", "", "", "", "", currency)	
							
						sql = sql_Rate.Replace(", fxRateRef AS","WITH fxRateRef AS") & $"					
							SELECT
								 id_factory AS [Id Factory] 
								, 'M' + RIGHT('0' + CAST(MONTH(date) AS VARCHAR), 2) AS Month
								, scenario_ref AS [Scenario Ref] 
								, id_costcenter AS [Id Cost Center] 
								, id_averagegroup AS [Id GM] 
								, id_account AS [Id Account] 
								, id_account + ' - ' + ISNULL(A.description,'No Description') AS [Account]
								, value_ref * R.rate AS [Reference]
								, activity_index AS [Activity Index] 
								, value_100_adjusted * R.rate AS [Ref 100 Adjusted]
								, fixed_cost_absorption * R.rate AS [Fixed Cost Abs]
								, value_semi_adjusted * R.rate AS [Ref Semi Adj]
								, value_semi_adjusted_f * R.rate AS [Ref Semi Adj Fixed]
								, value_semi_adjusted_v * R.rate AS [Ref Semi Adj Variable]
							
							FROM XFC_PLT_AUX_Costs_Init_GM C
							
							LEFT JOIN XFC_PLT_MASTER_Account A
								ON C.id_account = A.id
						
							LEFT JOIN fxRate R
								ON MONTH(C.date) = R.month	
							
							WHERE scenario = '{scenario}' 
							AND YEAR(date) = {year} 
							AND id_factory = '{factory}' 							
							AND value_ref <> 0
							
							ORDER BY date, id_costcenter, id_averagegroup, id_account							
						"
												
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using						
						
						Return dt

#End Region

#Region "Plan - Input"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("Plan_Input") Then
						
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim factory As String = args.NameValuePairs("factory")
						Dim year As String = args.NameValuePairs("year")
						Dim account_type As String = args.NameValuePairs("account_type")
						Dim ceco_nature As String = args.NameValuePairs("ceco_nature")
						Dim currency As String = args.NameValuePairs("currency")						
						
						Dim dt As DataTable
						Dim sql As String = String.Empty
						
						Dim sql_Rate = shared_queries.AllQueries(si, "RATE", factory, "0", year, scenario, "", "", "", "", "", currency)	
							
						sql = sql_Rate.Replace(", fxRateRef AS","WITH fxRateRef AS") & $"					
							SELECT
								 C.id_factory AS [Id Factory] 
								, 'M' + RIGHT('0' + CAST(MONTH(C.date) AS VARCHAR), 2) AS [Month] 
								, C.id_costcenter AS [Id Cost Center] 
								, M.description AS [CostCenter Description] 
								, M.type AS [CostCenter Type] 
								, M.nature AS [CostCenter Nature] 
								, C.id_account
						        , C.id_account AS [Id MNG Account]
						        , C.id_account + ' - ' + ISNULL(A.Description, '') AS [MNG Account]
								, RIGHT('0' + A.account_type,2) + ' - ' + ISNULL(N.Description, '') AS [Nature]
								, C.value * R.rate AS [Amount]
							
							FROM XFC_PLT_FACT_Costs C
							
							LEFT JOIN XFC_PLT_MASTER_Account A
								ON C.id_account = A.id
						
						    LEFT JOIN XFC_PLT_MASTER_NatureCost N
						        ON A.account_type = N.id  						
						
							LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist M
								ON C.id_costcenter = M.id
								AND M.scenario = 'Actual'
								AND C.date BETWEEN M.start_date AND M.end_date
						
							LEFT JOIN fxRate R
								ON MONTH(C.date) = R.month	
							
							WHERE 1=1
								AND C.scenario = '{scenario}' 
								AND YEAR(C.date) = {year} 
								AND C.id_factory = '{factory}' 
								AND A.account_type = '{account_type}'
								AND (M.nature = '{ceco_nature}' OR '{ceco_nature}' = 'All')
								AND value <> 0
							
							ORDER BY date, id_costcenter, id_averagegroup, id_account							
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using						
						
						Return dt

#End Region

#Region "Plan - Export PCT to FPA"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("ExportPCTtoFPA") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim factory As String = args.NameValuePairs("factory")
						Dim scenario As String = args.NameValuePairs("scenario")					

						Dim sql As String = String.Empty
						Dim dt As New DataTable
							
						sql = $"
							
							SELECT 
								F.id_account AS [id Account]
								, F.id_costcenter AS [id Cost Center]
								, C.type AS [Type Cost Center]
								, C.nature AS [Nature Cost Center]
								, B.id_rubric AS [id Rubric] 
								, F.id_product AS [id Product]
								, SUM(F.value) AS Value
							FROM XFC_PLT_FACT_CostsDistribution F
						
							LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								ON F.id_costcenter = C.id
								AND C.scenario = 'Actual'
								AND F.date BETWEEN C.start_date AND C.end_date
						
							LEFT JOIN (
										SELECT DISTINCT 
											mng_account AS id_account
											, costcenter_type
											, costcenter_nature
											, MAX(os_account) AS id_rubric
										FROM XFC_PLT_Mapping_Accounts_docF 
										GROUP BY mng_account, costcenter_type, costcenter_nature
									) AS B					
				       			ON F.id_account = B.id_account
								AND C.type = B.costcenter_type
								AND C.nature = B.costcenter_nature

							WHERE F.scenario = '{scenario}'
							AND (F.id_factory = '{factory}' OR '{factory}' = 'All')
							AND YEAR(F.date) = {year}
						
							GROUP BY 
								F.id_account
								, F.id_costcenter
								, C.type 
								, C.nature
								, B.id_rubric
								, F.id_product						
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
												
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						Return dt

	
#End Region

#Region "OLD - Forecast Index"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("Forecast_Index") Then
						
						Dim scenarioFcst As String = args.NameValuePairs("scenarioFCST")
						Dim scenarioRef As String = args.NameValuePairs("scenarioREF")
						Dim factory As String = args.NameValuePairs("factory")
						Dim year As String = args.NameValuePairs("year")
						
						Dim dt As DataTable
						Dim sql As String = String.Empty
						
						sql = $"
							WITH scenarioFCST as (
								SELECT
									MONTH(date) as month
									, id_averagegroup
									, id_costcenter
									, id_product
									, uotype
									, SUM(activity_taj) as activity_taj
									, SUM(activity_tso) as activity_tso
						
								FROM XFC_PLT_FACT_Production
							
								WHERE 1=1
									AND scenario = '{scenarioFcst}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
								GROUP BY id_averagegroup, id_costcenter, id_product, uotype, MONTH(date)
						
							),
						
							scenarioREF as (
								SELECT
									MONTH(date) as month
									, id_averagegroup
									, id_costcenter
									, id_product
									, uotype
									, SUM(activity_taj) as activity_taj
									, SUM(activity_tso) as activity_tso
								FROM XFC_PLT_FACT_Production
							
								WHERE 1=1
									AND scenario = '{scenarioRef.Replace("_PrevYear","")}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
								GROUP BY id_averagegroup, id_costcenter, id_product, uotype, MONTH(date)
								
							)
						
							SELECT 
								A.month
								, A.id_averagegroup
								, A.id_costcenter
								, A.id_product
								, B.uotype
								, B.activity_taj as [Reference Activity]
								, A.activity_taj as [Forecast Activity]
								, CASE 
									WHEN B.activity_taj = 0 OR B.activity_taj is null Then 1
									ELSE A.activity_taj / B.activity_taj 
								END as [Adjustment Index]
						
							FROM scenarioFCST A
						
							LEFT JOIN scenarioREF B
								ON A.month = B.month
								AND A.id_averagegroup = B.id_averagegroup
								AND A.id_costcenter = B.id_costcenter
								AND A.id_product = B.id_product
							
						"
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						
						Return dt

#End Region

#Region "OLD - Forecast Costs Init"
	
					Else If args.DataSetName.XFEqualsIgnoreCase("Forecast_CostsInit") Then
						
						Dim scenarioFcst As String = args.NameValuePairs("scenarioFCST")
						Dim scenarioRef As String = args.NameValuePairs("scenarioREF")
						Dim factory As String = args.NameValuePairs("factory")
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs.GetValueOrDefault("month", "10")
						
						Dim dt As DataTable
						Dim sql As String = String.Empty
						
						sql = $"
							WITH scenarioFCST as (
								SELECT
									MONTH(date) as month
									, id_averagegroup
									, id_costcenter
									, uotype
									, SUM(activity_taj) as activity_taj
									, SUM(activity_tso) as activity_tso
						
								FROM XFC_PLT_FACT_Production
							
								WHERE 1=1
									AND scenario = '{scenarioFcst}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
								GROUP BY id_averagegroup, id_costcenter, uotype, MONTH(date)
						
							),
						
							scenarioREF as (
								SELECT
									MONTH(date) as month
									, id_averagegroup
									, id_costcenter
									, uotype
									, SUM(activity_taj) as activity_taj
									, SUM(activity_tso) as activity_tso
								FROM XFC_PLT_FACT_Production
							
								WHERE 1=1
									AND scenario = '{scenarioRef.Replace("_PrevYear","")}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
								GROUP BY id_averagegroup, id_costcenter, uotype, MONTH(date)
								
							),
						
						adjustmentIndex as (						
						
							SELECT 
								A.month
								, A.id_averagegroup
								, A.id_costcenter
								, B.uotype
								, B.activity_taj as [Reference Activity]
								, A.activity_taj as [Forecast Activity]
								, CASE 
									WHEN B.activity_taj = 0 Then 1
									ELSE A.activity_taj / B.activity_taj 
								END as [Adjustment Index]
							FROM scenarioFCST A
						
							LEFT JOIN scenarioREF B
								ON A.month = B.month
								AND A.id_averagegroup = B.id_averagegroup
								AND A.id_costcenter = B.id_costcenter
						),
						
						accountMaster as (
									SELECT 
										mng_account
										, account_type
						
									FROM XFC_PLT_MAPPING_Accounts
						
									GROUP BY mng_account, account_type
						),
						
						startCosts as (
							SELECT
								MONTH(A.date) as month
								, B.nature
								, B.averagegroup as id_averagegroup
								, A.id_costcenter 
								, A.id_account
								, C.account_type
								, SUM(A.value) as TotalAmount
						
							FROM XFC_PLT_FACT_Costs A
						
							LEFT JOIN XFC_PLT_MASTER_CostCenter B
								ON A.id_costcenter = B.id
						
							LEFT JOIN accountMaster C
								ON A.id_account = C.mng_account
						
							WHERE 1=1
								AND YEAR(A.date) = {year}
								AND A.scenario = '{scenarioRef.Replace("_PrevYear","")}'
								AND A.id_factory = '{factory}'
						
							GROUP BY MONTH(A.date), B.nature , B.averagegroup, A.id_costcenter, A.id_account, C.account_type
						),
						
						cp_Weights as (
								SELECT
									MONTH(date) as month
									, A.id_averagegroup
									,A.id_costcenter
									,A.id_product
									,A.uotype
									,SUM(A.activity_taj) as activity_taj
									,SUM(A.activity_tso) as activity_tso
									,COALESCE((SUM(A.activity_taj)) / NULLIF(SUM(SUM(A.activity_taj)) OVER (PARTITION BY MONTH(date), A.id_averagegroup, A.id_costcenter, uotype), 0), 0) AS activity_weight_taj
									,COALESCE((SUM(A.activity_tso)) / NULLIF(SUM(SUM(A.activity_tso)) OVER (PARTITION BY MONTH(date), A.id_averagegroup, A.id_costcenter, uotype), 0), 0) AS activity_weight_tso
							
								FROM XFC_PLT_FACT_Production A
							
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenarioFcst}'
							
								GROUP BY A.id_averagegroup, A.id_costcenter, A.id_product, MONTH(date), uotype						
						),
						
						startCosts_CP_weight as (
						
							SELECT
								A.month
								, A.nature
								,A.id_averagegroup as Origin 
								,A.id_costcenter 
								,A.id_account
								,A.account_type
								,A.TotalAmount 	
								,A.id_averagegroup as destination
								,B.id_product
								,'0' as KeyAllocation
								,B.activity_weight_taj as ActivityWeight_TAJ 
								-- ,B.activity_weight_tso as ActivityWeight_TSO
								,A.TotalAmount*B.activity_weight_taj as SharedCost_TAJ
								-- ,A.TotalAmount*B.activity_weight_tso as SharedCost_TSO
						
							FROM startCosts A
						
							LEFT JOIN cp_Weights B
								ON A.id_averagegroup = B.id_averagegroup
								AND A.id_costcenter = B.id_costcenter
								AND A.month = B.month
						
							WHERE 1=1
								AND A.nature = 'CP'
								AND (((A.account_type = 'MOD' and B.uotype = 'HRS') OR (A.account_type <> 'MOD' and B.uotype = 'H M')) OR B.uotype is null OR B.uotype = '-1')					
						
						),
						
						cat_Weights as (
							SELECT
								MONTH(date) as month
								, A.id_averagegroup
								, A.id_product
								, A.uotype
								, COALESCE((SUM(A.activity_taj)) / NULLIF(SUM(SUM(A.activity_taj)) OVER (PARTITION BY MONTH(date), A.id_averagegroup, uotype), 0), 0) AS activity_weight_cat_taj
								, COALESCE((SUM(A.activity_tso)) / NULLIF(SUM(SUM(A.activity_tso)) OVER (PARTITION BY MONTH(date), A.id_averagegroup, uotype), 0), 0) AS activity_weight_cat_tso
						
							FROM XFC_PLT_FACT_Production A
						
							WHERE 1=1
								AND A.id_factory = '{factory}'
								AND YEAR(A.date) = {year}
								AND A.scenario = '{scenarioFcst}'
						
							GROUP BY A.id_averagegroup, A.id_product, uotype, MONTH(date)
						),
						
						startCost_CAT_KeysAlloc as (
							SELECT
								A.month
								, A.nature
								, A.id_averagegroup
								, A.id_costcenter
								, A.id_account
								, A.account_type
								, A.TotalAmount
								, B.id_averagegroup as destination
								, B.percentage/100 as percentage
								, A.TotalAmount*(B.percentage/100) as GM_SharedCosts
														
							FROM startCosts A
							
							LEFT JOIN XFC_PLT_AUX_AllocationKeys B
								ON A.id_costcenter = B.id_costcenter
								AND A.account_type = B.costnature
								AND A.month = MONTH(B.date)
																
							WHERE 1=1
								AND A.nature = 'CAT'
								AND B.scenario = '{scenarioFcst}'
								AND B.id_factory = '{factory}'
								-- AND YEAR(B.date) = {year} Faltan las key allocations de este año
						),
						
						startCosts_CAT_weight as (
							SELECT 
								A.month
								, A.nature
								, A.id_averagegroup as Origin
								, A.id_costcenter as CeCo
								, A.id_account
								, A.account_type
								, A.TotalAmount
								, A.destination
								, B.id_product
								, A.percentage as KeyAllocation -- GM_Weight_TAJ o TSO
								, B.activity_weight_cat_taj as ActivityWeight_TAJ  -- GM | REF
								-- ,B.activity_weight_cat_tso as ActivityWeight_TSO -- GM | REF
								, A.TotalAmount*A.percentage*B.activity_weight_cat_taj as SharedCosts_TAJ
								-- ,A.TotalAmount*A.percentage*B.activity_weight_cat_tso as SharedCosts_TSO
						
							FROM startCost_CAT_KeysAlloc A
						
							LEFT JOIN cat_Weights B
								ON A.destination = B.id_averagegroup
								AND A.month = B.month
						
							WHERE 1=1
								AND (((A.account_type = 'MOD' and B.uotype = 'HRS') OR (A.account_type <> 'MOD' and B.uotype = 'H M')) OR B.uotype is null OR B.uotype = '-1')					
								-- AND A.month = 10
						),
						
						startCosts_CAA_aux1 as (
							SELECT 	
								month
								, nature
								, id_averagegroup
								, id_costcenter
								, id_account
								, account_type
								, SUM(TotalAmount) as TotalAmount
														
							FROM (
							-- CAA SharedCosts
								SELECT
									A.month
									, A.nature
									, A.id_averagegroup
									, A.id_costcenter
									, A.id_account
									, A.account_type
									, A.TotalAmount
															
								FROM startCosts A
																																						
								WHERE 1=1
									AND A.nature = 'CAA'
								
							
								UNION
							
							-- CAT costs to be Shared
						        SELECT
									B.month
									, 'CAA' as Nature
									, B.Origin as id_averagegroup
									, B.CeCo as id_costcenter
									, B.id_account as id_account
									, B.account_type as account_type
									, SUM(B.TotalAmount*B.KeyAllocation) as TotalAmount
							        
							  	FROM startCosts_CAT_weight B
								
								WHERE 1=1
									AND (B.id_product IS NULL)
														
								GROUP BY B.month, B.Origin, B.CeCo, B.id_account, B.account_type
								) as caa_cat
								
							GROUP BY month, nature, id_averagegroup, id_costcenter,id_account, account_type						
						    
						),
						
						caa_Weights as (
							SELECT
								month
								, destination as id_averagegroup
								, SUM(SharedCost_TAJ) / SUM(SUM(SharedCost_TAJ)) OVER() as caa_weight_taj
								-- , SUM(SharedCost_TSO) / SUM(SUM(SharedCost_TSO)) OVER() as caa_weight_tso
							
							FROM (
								SELECT *
																
								FROM startCosts_CP_weight A
								
								WHERE account_type <> 'Amo'
								UNION
							
								SELECT *
								
								FROM startCosts_CAT_weight B
						
								WHERE 1=1
									AND account_type <> 'Amo'
									AND (B.id_product IS NOT NULL)
									
						
							) as cp_cat
						
							GROUP BY month, destination
								
						),						
						
						startCosts_CAA_weight as (
							SELECT 
								A.month
								, A.nature
								, A.id_averagegroup as Origin
								, A.id_costcenter as CeCo
								, A.id_account
								, A.account_type
								, A.TotalAmount
								, B.id_averagegroup as Destination
								, C.id_product
								, B.caa_weight_taj as AuxiliarWeight_TAJ
								-- ,B.caa_weight_taj as AuxiliarWeight_TSO
								, C.activity_weight_cat_taj as ActivityWeight_TAJ
								-- ,C.activity_weight_cat_tso as ActivityWeight_TSO
								, A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_taj as SharedCosts_TAJ
								-- ,A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_tso as SharedCosts_TSO									
						
							FROM startCosts_CAA_aux1 A
						
							LEFT JOIN caa_Weights B
								ON A.month = B.month
						
							LEFT JOIN cat_Weights C
								ON B.id_averagegroup = C.id_averagegroup
								AND A.month = C.month
								AND B.month = C.month
													
							WHERE 1=1
								AND (((A.account_type = 'MOD' and C.uotype = 'HRS') OR (A.account_type <> 'MOD' and C.uotype = 'H M')) OR C.uotype is null OR C.uotype = '-1')					
																	
						),
								
						weightsTable_aux as (
							SELECT *							
							FROM startCosts_CP_weight												

							UNION
							
							SELECT *							
							FROM startCosts_CAT_weight
							WHERE 1=1
								AND (id_product IS NOT NULL)									
						
							UNION
							
							SELECT *							
							FROM startCosts_CAA_weight											
						),
						
						weightsTable as (
							SELECT 
								'{scenarioFcst}' as scenario
								, month
								, '{factory}' as id_factory
								, id_account as id_account
								, account_type
								, id_costcenter
								, destination as id_averagegroup
								, CASE 
									WHEN id_product IS NULL THEN '-1' 
									ELSE id_product
								END as id_product
								, SharedCost_TAJ as SharedCost
						
							FROM weightsTable_aux	

						), 
						
						fv_Costs as (
							SELECT
								MONTH(date) as month
								, id_account
								, (value/100) as percentage
							
							FROM XFC_PLT_AUX_FixedVariableCosts
						
							WHERE 1=1
								AND id_factory = '{factory}'
								AND scenario = '{scenarioFcst}'
								-- AND YEAR(A.date) = {year} Faltan los datos de 2024				
						
						),
						
						sharedCosts_VF as (
							
							SELECT 
								A.*
								, 'V' as Type
								,  COALESCE(B.percentage, 0) AS percentage
								, A.SharedCost * COALESCE(B.percentage, 0) AS Costs
						
							FROM weightsTable A
						
							LEFT JOIN fv_Costs B
								ON A.month = B.month
								AND A.id_account = B.id_account
						
							UNION ALL
						
							SELECT 
								A.*
								, 'F' as Type
								, (1- COALESCE(B.percentage, 0)) as Percentage
								, A.SharedCost * (1- COALESCE(B.percentage, 0)) as Costs
						
							FROM weightsTable A
						
							LEFT JOIN fv_Costs B
								ON A.month = B.month
								AND A.id_account = B.id_account	
							
						),
						
						sharedCosts_Index as (
							SELECT
								A.scenario as [FCST Scenario]
								, A.id_factory as [Factory]
								, A.month as [Month]
								, A.id_account as [Account]
								, A.account_type as [Account Type]
								, A.id_costcenter as [Cost Center]
								, A.id_averagegroup as [GM]
								, A.id_product as [Product]
								, SUM(A.SharedCost) as [Shared Cost]
								, CASE
									WHEN A.Type = 'F' Then SUM(A.SharedCost * A.percentage  + A.SharedCost * (1-A.percentage) * B.[Adjustment Index])
								END AS [Semi-Adjusted Cost]
								, CASE
									WHEN A.Type = 'F' Then SUM(A.SharedCost * A.percentage * B.[Adjustment Index] + A.SharedCost * (1-A.percentage) * B.[Adjustment Index])
								END AS [Adjusted Cost]
							
							FROM sharedCosts_VF A
							
							LEFT JOIN adjustmentIndex B
								ON A.month = B.month
								AND A.id_averagegroup = B.id_averagegroup
								-- AND A.id_product = B.id_product No ESTÁ CON ESTE DETALLE EN ESTA CONSULTA AÚN
							
							WHERE 1=1
								AND A.Type = 'F'
								-- añadir el UOType en función de MOD, MOS...
							
							GROUP BY 
								A.scenario
							    , A.id_factory
							    , A.month
							    , A.id_account
							    , A.account_type
							    , A.id_costcenter
							    , A.id_averagegroup
							    , A.id_product
							    , Type -- Necesario porque se usa en el CASE
							    , A.percentage
							    , [Adjustment Index]
							),
						
						monthlyView as (
							SELECT
								Month
								, Account
								, SUM([Shared Cost]) as [Shared Cost]
								, SUM([Semi-Adjusted Cost]) as [Semi-Adjusted Cost]
								, SUM([Adjusted Cost]) as [Adjusted Cost]
						
							FROM sharedCosts_Index
						
							WHERE 1=1
								AND month = {month}
						
							GROUP BY Month, Account					
						)
							
						SELECT *
						
						FROM monthlyView
						
						ORDER BY Account
							"
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using
						
						
						Return dt

#End Region

#Region "OLD - VTU - Monthly"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Monthly") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						'Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")
						
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						If (scenario.StartsWith("RF") Or scenario.StartsWith("B"))
							month = 12
						End If
														
						Dim monthStart As String 
						Dim yearStart As String												
						
						If (month <> "12")  Then
							yearStart = (CInt(year)-1).ToString
							monthStart = (CInt(month)+1).ToString
						Else
							yearStart = year
							monthStart = "01"
						End If	
						
						Dim fecha As New DateTime(Integer.Parse(yearStart), Integer.Parse(monthStart), 1)
						Dim cabeceras As String = String.Empty												
						
				        For i As Integer = 0 To 11
				            Dim fechaMes As DateTime = fecha.AddMonths(i)
				            cabeceras = cabeceras + "[" + fechaMes.ToString("MM-yy") + "], " 
				        Next
						cabeceras = cabeceras.Substring(0, cabeceras.Length - 2)
										
'						BRApi.ErrorLog.LogMessage(si, "cabeceras: " & cabeceras)
						
						Dim sql_VTU_data = shared_queries.AllQueries(si, "VTU_data_monthly", factory, month, year, scenario, "", "", "", type, "", currency)								
						
						sql = sql_VTU_data & $"	
							
							SELECT id_product, desc_product, {cabeceras}
							FROM (
						
								-- MAIN QUERY
								
								SELECT month, id_product, desc_product, SUM(CAST(VTU AS DECIMAL(18,2))) AS VTU
								FROM (
								
								-- VTU PRODUCTO
								
								SELECT
									RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) + '-' + RIGHT(CONVERT(VARCHAR,YEAR(date)),2) AS month
									,F.id_product AS id_product
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
									RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) + '-' + RIGHT(CONVERT(VARCHAR,YEAR(date)),2) AS month
									,P.id_product_final AS id_product
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
								
								GROUP BY month, id_product, desc_product
								--HAVING SUM(VTU_pond) <> 0
						
							) AS SourceTable
							PIVOT (
							    SUM(VTU) 
							    FOR month IN ({cabeceras})
							) AS PivotTable
						
							ORDER BY desc_product;
												
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using					
						
						Return dt						
						
#End Region	

#Region "OLD - VTU - Monthly Components"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Monthly_Components") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						'Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim product As String = args.NameValuePairs("product")
						
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						If (scenario.StartsWith("RF") Or scenario.StartsWith("B"))
							month = 12
						End If						
						
						Dim monthStart As String 
						Dim yearStart As String
						
						If month <> "12" Then
							yearStart = (CInt(year)-1).ToString
							monthStart = (CInt(month)+1).ToString
						Else
							yearStart = year
							monthStart = "01"
						End If	
						
						Dim fecha As New DateTime(Integer.Parse(yearStart), Integer.Parse(monthStart), 1)
						Dim cabeceras As String = String.Empty	
						Dim cabecerasSUM As String = String.Empty	
						
				        For i As Integer = 0 To 11
				            Dim fechaMes As DateTime = fecha.AddMonths(i)
				            cabeceras = cabeceras + "[" + fechaMes.ToString("MM-yy") + "], " 
							cabecerasSUM = cabecerasSUM + "SUM([" + fechaMes.ToString("MM-yy") + "]) AS [" + fechaMes.ToString("MM-yy") + "], " 
				        Next
						cabeceras = cabeceras.Substring(0, cabeceras.Length - 2)
						cabecerasSUM = cabecerasSUM.Substring(0, cabecerasSUM.Length - 2)
										
'						BRApi.ErrorLog.LogMessage(si, "cabeceras: " & cabeceras)
						
						Dim sql_VTU_data = shared_queries.AllQueries(si, "VTU_data_monthly", factory, month, year, scenario, "", "", "", type, product, currency)								
						
						sql = sql_VTU_data & $"	
							
							-- MAIN QUERY
							
							, data AS (						
						
							SELECT id_product, id_component, desc_product, Level, {cabeceras}
							FROM (
						
								-- MAIN QUERY
								
								SELECT month, id_product, id_component, desc_product, Level, SUM(CAST(VTU AS DECIMAL(18,2))) AS VTU
								FROM (
								
								-- VTU PRODUCTO
								
								SELECT
									RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) + '-' + RIGHT(CONVERT(VARCHAR,YEAR(date)),2) AS month
							 		,'--' AS id_product
							    	,P.id AS id_component
									,M.description AS desc_product
									,1 AS Level
								    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								   		ELSE F.cost / F.volume
								   		* F.activity_UO1 / F.activity_UO1_total 
									END AS VTU
								
								FROM (SELECT '{product}' AS Id) P
								LEFT JOIN factVTU F
									ON P.id = F.id_product	
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON P.id = M.id						
							
								UNION ALL
								
								-- VTU COMPONENTS
								
								SELECT  
									RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) + '-' + RIGHT(CONVERT(VARCHAR,YEAR(date)),2) AS month
									,P.id_product 
									,P.id_component 
								 	,M.description AS desc_product
									,P.level AS Level
						         	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							     		ELSE F.cost / F.volume
							     		* F.activity_UO1 / F.activity_UO1_total 
										* P.exp_coefficient END AS VTU
								
								FROM Product_Component_Recursivity P
								INNER JOIN factVTU F
									ON F.id_product = P.id_component 
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON P.id_component = M.id									
						
								) AS RES
								
								GROUP BY month, id_product, id_component, desc_product, Level
								--HAVING SUM(VTU_pond) <> 0
						
							) AS SourceTable
							PIVOT (
							    SUM(VTU) 
							    FOR month IN ({cabeceras})
							) AS PivotTable
						
							)
						
							SELECT 'TOTAL:' AS id_product, NULL AS id_component, NULL AS desc_product, NULL AS Level, {cabecerasSUM} 
							FROM data
						
							UNION ALL
						
							SELECT D.id_product, D.id_component, D.desc_product, Level, {cabeceras}  
							FROM data D
							ORDER BY level, id_component
												
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using					
						
						Return dt						
						
#End Region	

#Region "OLD - VTU - Monthly GM"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Monthly_GM") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim month As String = args.NameValuePairs("month")
						Dim factory As String = args.NameValuePairs("factory")
						'Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")
						Dim product As String = args.NameValuePairs("product")
						
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
						
						If (scenario.StartsWith("RF") Or scenario.StartsWith("B"))
							month = 12
						End If						
						
						Dim monthStart As String 
						Dim yearStart As String
						
						If month <> "12" Then
							yearStart = (CInt(year)-1).ToString
							monthStart = (CInt(month)+1).ToString
						Else
							yearStart = year
							monthStart = "01"
						End If	
						
						Dim fecha As New DateTime(Integer.Parse(yearStart), Integer.Parse(monthStart), 1)
						Dim cabeceras As String = String.Empty	
						Dim cabecerasSUM As String = String.Empty	
						
				        For i As Integer = 0 To 11
				            Dim fechaMes As DateTime = fecha.AddMonths(i)
				            cabeceras = cabeceras + "[" + fechaMes.ToString("MM-yy") + "], " 
							cabecerasSUM = cabecerasSUM + "SUM([" + fechaMes.ToString("MM-yy") + "]) AS [" + fechaMes.ToString("MM-yy") + "], " 
				        Next
						cabeceras = cabeceras.Substring(0, cabeceras.Length - 2)
						cabecerasSUM = cabecerasSUM.Substring(0, cabecerasSUM.Length - 2)
										
'						BRApi.ErrorLog.LogMessage(si, "cabeceras: " & cabeceras)
						
						Dim sql_VTU_data = shared_queries.AllQueries(si, "VTU_data_monthly", factory, month, year, scenario, "", "", "", type, product, currency)								
						
						sql = sql_VTU_data & $"	
							
							-- MAIN QUERY
							
							, data AS (						
						
							SELECT id_averagegroup, description, {cabeceras}
							FROM (
						
								-- MAIN QUERY
								
								SELECT month, id_averagegroup, description, SUM(CAST(VTU AS DECIMAL(18,2))) AS VTU
								FROM (
								
								-- VTU PRODUCTO
								
								SELECT
									RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) + '-' + RIGHT(CONVERT(VARCHAR,YEAR(date)),2) AS month
							       	,F.id_product 
							       	,F.id_averagegroup
							       	,A.description
								    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								   		ELSE F.cost / F.volume
								   		* F.activity_UO1 / F.activity_UO1_total 
									END AS VTU
								
								FROM (SELECT '{product}' AS Id) P
								LEFT JOIN factVTU F
									ON P.id = F.id_product	
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON P.id = M.id		
								LEFT JOIN XFC_PLT_MASTER_averagegroup A
									ON F.id_averagegroup = A.id
									AND A.id_factory = '{factory}' 						
							
								UNION ALL
								
								-- VTU COMPONENTS
								
								SELECT  
									RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) + '-' + RIGHT(CONVERT(VARCHAR,YEAR(date)),2) AS month
							       	,F.id_product 
							       	,F.id_averagegroup
							       	,A.description
						         	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
							     		ELSE F.cost / F.volume
							     		* F.activity_UO1 / F.activity_UO1_total 
										* P.exp_coefficient END AS VTU
								
								FROM Product_Component_Recursivity P
								INNER JOIN factVTU F
									ON F.id_product = P.id_component 
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON P.id_component = M.id	
								LEFT JOIN XFC_PLT_MASTER_averagegroup A
									ON F.id_averagegroup = A.id
									AND A.id_factory = '{factory}' 							
						
								) AS RES
								
								GROUP BY month, id_averagegroup, description
								--HAVING SUM(VTU_pond) <> 0
						
							) AS SourceTable
							PIVOT (
							    SUM(VTU) 
							    FOR month IN ({cabeceras})
							) AS PivotTable
						
							)
						
							SELECT 1 AS Orden, 'TOTAL:' AS id_averagegroup, NULL AS description, {cabecerasSUM} 
							FROM data
						
							UNION ALL
						
							SELECT 2 AS Orden, D.id_averagegroup, D.description, {cabeceras}  
							FROM data D
							ORDER BY Orden, id_averagegroup
												
						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using					
						
						Return dt						
						
#End Region	

#Region "OLD - VTU - Monthly Natures"

					Else If args.DataSetName.XFEqualsIgnoreCase("VTU_Monthly_Natures") Then						
						
						Dim year As String = args.NameValuePairs("year")
						Dim factory As String = args.NameValuePairs("factory")
						'Dim view As String = args.NameValuePairs("view")
						Dim type As String = args.NameValuePairs("type")
						Dim scenario As String = args.NameValuePairs("scenario")
						
						Dim currency As String = args.NameValuePairs("currency")
						
						Dim product As String = args.NameValuePairs("product")
						
						Dim sql As String = String.Empty
						Dim dt As New DataTable
												
						Dim sql_VTU_data = shared_queries.AllQueries(si, "VTU_data_monthly", factory, "", year, scenario, "", "", "", type, product, currency)	
							
						sql = sql_VTU_data & $"	
							
							, data AS (
						
							SELECT id_product, desc_product, id_account_type, account_type, [M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12]
							FROM (
						
								-- MAIN QUERY
								
								SELECT month, id_product, desc_product, id_account_type, account_type, SUM(VTU_pond) AS VTU
								FROM (
								
								-- VTU PRODUCTO
								
								SELECT
									month
									,F.id_product AS id_product
									,M.description AS desc_product		
									,N.id AS id_account_type
									,N.description AS account_type
								    ,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								   		ELSE F.cost / F.volume
								   		* F.activity_UO1 / F.activity_UO1_total 
									END AS VTU_pond
								
								FROM (SELECT '{product}' AS Id) P
								LEFT JOIN factVTU F
									ON P.id = F.id_product	
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON F.id_product = M.id		
								LEFT JOIN XFC_PLT_MASTER_NatureCost N
									ON F.account_type = N.id
							
								UNION ALL
								
								-- VTU COMPONENTS
								
								SELECT  
									month
									,P.id_product_final AS id_product
								 	,M.description AS desc_product
									,N.id AS id_account_type
									,N.description AS account_type
								         	,CASE WHEN F.volume = 0 OR F.activity_UO1 = 0 THEN 0 
								     		ELSE F.cost / F.volume
								     		* F.activity_UO1 / F.activity_UO1_total 
											* P.exp_coefficient END AS VTU_pond
								
								FROM (	SELECT * 
										FROM Product_Component_Recursivity
										WHERE id_product_final = '{product}') P
								LEFT JOIN factVTU F
									ON F.id_product = P.id_component 
								LEFT JOIN XFC_PLT_MASTER_Product M
									ON P.id_product_final = M.id	
								LEFT JOIN XFC_PLT_MASTER_NatureCost N
									ON F.account_type = N.id						
								
								) AS RES
								
								GROUP BY month, id_product, desc_product, id_account_type, account_type
								HAVING SUM(VTU_pond) <> 0
						
							) AS SourceTable
							PIVOT (
							    SUM(VTU) 
							    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
							) AS PivotTable
						
							)
						
							SELECT 'TOTAL:' AS id_product, NULL AS desc_product, NULL AS id_account_type, NULL AS account_type, SUM([M01]) AS M01, SUM([M02]) AS M02, SUM([M03]) AS M03, SUM([M04]) AS M04, SUM([M05]) AS M05, SUM([M06]) AS M06, SUM([M07]) AS M07, SUM([M08]) AS M08, SUM([M09]) AS M09, SUM([M10]) AS M10, SUM([M11]) AS M11, SUM([M12]) AS M12
							FROM data
						
							UNION ALL
						
							SELECT id_product, desc_product, id_account_type, account_type, [M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12]
							FROM data 
							ORDER BY id_account_type

						"
						
'						BRApi.ErrorLog.LogMessage(si, sql)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
						End Using					
						
						Return dt						
						
#End Region	
							
						End If
				End Select


				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Helpers"

		Public Function NewErrorDataTable(message As String) As DataTable
			' Generate initial Dt
			Dim errorDt As New DataTable
			errorDt.Columns.Add("Error Message")
			
			' Add row with message
			Dim errorRow = errorDt.NewRow()
			errorRow("Error Message") = message
			errorDt.Rows.Add(errorRow)
			
			Return errorDt
		End Function
		
		Public Function GetSQLDataVTU (ByVal si As SessionInfo, ByVal scenario As String, ByVal month As String, ByVal factory As String, ByVal year As String, ByVal currency As String, Optional activity As String = "No", Optional costDetail As String  = "No") As String
			
			Dim sqlVTU As String = String.Empty
			Dim sqlVersion As String = "Old"		
			
			If (scenario = "RF09" And CInt(month) >= 9 Or CInt(year)>=2026) Then
				sqlVersion = "New"
			ElseIf (scenario = "Actual" And CInt(month) >= 9) Then
				sqlVersion = "New"
			End If
			
			Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
			Dim querySQL As String = If(activity = "Yes", "VTU_data_accountDetail_aux_tables_NewTables_Activity", "VTU_data_accountDetail_aux_tables_NewTables")
			querySQL = If(costDetail="Yes",querySQL & "_Application",querySQL)
			Return shared_queries.AllQueries(si, querySQL, factory, month, year, scenario, "", "", "", "Existing", "", currency,"","",sqlVersion)			
			
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

#End Region
		
	End Class
End Namespace
