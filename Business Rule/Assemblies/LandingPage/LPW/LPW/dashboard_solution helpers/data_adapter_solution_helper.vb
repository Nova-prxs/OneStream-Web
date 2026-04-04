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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.data_adapter_solution_helper
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
			
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						Dim names As New List(Of String)()
						names.Add("ActualFinancialFigures")
						names.Add("Scenarios")
						names.Add("ComparisonActualCash")
						names.Add("OSnReportedCashComparison")
						names.Add("OSnReportedCashComparisonCashGraph")
						names.Add("AssetsGraph")
						names.Add("Decided_Graph")
						names.Add("CompanyNames")
						names.Add("FxRates")
						Return names
					
					Case Is = DashboardDataSetFunctionType.GetDataSet
						
						#Region "Actual Financial Figures"
						
					 If args.DataSetName.XFEqualsIgnoreCase("ActualFinancialFigures") Then
							'Get parameters
							Dim paramProject As String = args.NameValuePairs("p_project")
							
							'Declare select query and parameters
							Dim selectQuery As String
							Dim dbParamInfos As New List(Of DbParamInfo)
							'Control if the user has selected a project
							If String.IsNullOrEmpty(paramProject) Then
								selectQuery = $"
									SELECT
										company_name,
										SUM([Requirement]) AS requirement, 
								  		SUM([Decided]) AS decided,
										SUM([Ordered]) AS ordered, 
								   		SUM([Delivered]) AS delivered, 
								   		SUM(cash_ytd) as cash_ytd
									FROM (
										SELECT
											p.company_name AS company_name,
											pff.year AS year,
											pff.month AS month,
											pff.type AS type,
											pff.amount_ytd AS amount_ytd,
											pc.cash_ytd AS cash_ytd
										FROM XFC_INV_MASTER_project p
										INNER JOIN XFC_INV_FACT_project_financial_figures pff ON p.project = pff.project_id
											AND pff.scenario = 'Actual'
										INNER JOIN (
											SELECT TOP 1
											    year,
											    month
											FROM XFC_INV_FACT_project_financial_figures
											ORDER BY year DESC, month DESC
										) ym ON pff.year = ym.year
											AND pff.month = ym.month
										INNER JOIN (
											SELECT
												project_id,
												year,
												month,
												SUM(amount) OVER (
													PARTITION BY year, project_id
													ORDER BY month
													ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
												) AS cash_ytd
											FROM XFC_INV_FACT_project_cash
											WHERE scenario = 'Actual'
										) pc ON pff.project_id = pc.project_id
										AND pff.year = pc.year
										AND pff.month = pc.month
									)  AS SourceTable
									PIVOT
									(
										MAX(amount_ytd)
										FOR type IN ([Requirement], [Decided], [Ordered], [Delivered])
									) AS PivotTable
									GROUP BY company_name;
								"
							Else
								'Add project to param info list
								dbParamInfos.Add(New DbParamInfo("project", paramProject))
								selectQuery = $"
									SELECT
										year,
										SUM([Requirement]) AS requirement, 
								  		SUM([Decided]) AS decided,
										SUM([Ordered]) AS ordered, 
								   		SUM([Delivered]) AS delivered, 
								   		SUM(cash_ytd) as cash_ytd
									FROM (
										SELECT
											p.project AS project,
											pff.year AS year,
											pff.month AS month,
											pff.type AS type,
											pff.amount_ytd AS amount_ytd,
											pc.cash_ytd AS cash_ytd
										FROM XFC_INV_MASTER_project p
										INNER JOIN XFC_INV_FACT_project_financial_figures pff ON p.project = pff.project_id
											AND p.project = @project
											AND pff.scenario = 'Actual'
										INNER JOIN (
											SELECT
											    year,
											    MAX(month) AS month
											FROM XFC_INV_FACT_project_financial_figures
											GROUP BY year
										) ym ON pff.year = ym.year
											AND pff.month = ym.month
										INNER JOIN (
											SELECT
												project_id,
												year,
												month,
												SUM(amount) OVER (
													PARTITION BY year, project_id
													ORDER BY month
													ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
												) AS cash_ytd
											FROM XFC_INV_FACT_project_cash
											WHERE scenario = 'Actual'
										) pc ON pff.project_id = pc.project_id
										AND pff.year = pc.year
										AND pff.month = pc.month
									)  AS SourceTable
									PIVOT
									(
										MAX(amount_ytd)
										FOR type IN ([Requirement], [Decided], [Ordered], [Delivered])
									) AS PivotTable
									GROUP BY project, year;
								"
							End If
							
							'Return executed query
							Return BRApi.Database.ExecuteSql(BRApi.Database.CreateApplicationDbConnInfo(si), selectQuery, dbParamInfos, False)
							
						#End Region
						
						#Region "Scenarios"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Scenarios") Then
							'Get parameters
							Dim paramScenarioMain As String = args.NameValuePairs.XFGetValue("p_scenario_main")
							
							'Declare DataTable
							Dim dt As New DataTable()
							dt.Columns.Add("scenario")
							dt.Columns.Add("value")
							
							'Declare dictionary of scenarios
							Dim scenarioDict As New Dictionary(Of String, List(Of Tuple(Of String, String))) From {
								{
									"Actual",
									New List(Of Tuple(Of String, String)) From {
										New Tuple(Of String, String)("Actual", "Actual")
									}
								},
								{"Forecast", Me.GetPlanningScenarios(si, "Forecast")},
								{"Budget", Me.GetPlanningScenarios(si, "Budget")}
							}
							
							'Control scenarios to be shown based on the main scenario
							If scenarioDict.Keys.Contains(paramScenarioMain) Then
								For Each scenario As Tuple(Of String, String) In scenarioDict(paramScenarioMain)
									Dim row As DataRow = dt.NewRow()
									row("scenario") = scenario.Item1
									row("value") = scenario.Item2
									dt.Rows.Add(row)
								Next
							Else
								For Each mainScenario In scenarioDict.Values
									For Each scenario As Tuple(Of String, String) In mainScenario
									Dim row As DataRow = dt.NewRow()
									row("scenario") = scenario.Item1
									row("value") = scenario.Item2
									dt.Rows.Add(row)
								Next
								Next
							End If
							
							Return dt
						
						#End Region
						
						#Region "Comparison Cash Actual Graph"
						
							ElseIf args.DataSetName.XFEqualsIgnoreCase("ComparisonActualCash") Then
							    'Retrieve parameters for queries
							    Dim queryParams As String = args.NameValuePairs("p_query_params")
							    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							    'Retrieve color parameters
							    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
							    Dim secondaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Secondary")
							    Dim terciaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Terciary")
							    Dim scenarioGraph As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "LPW.prm_LPW_Cash_Scenario_Graph")
							
							    'Declare new DataTables
							    Dim dt As New DataTable()
							    Dim dt1 As New DataTable()
							    Dim dt2 As New DataTable()
							
							    'Build the whereClause for the FIRST query (Cash_Cumul_Graph).
							    'The original code already includes v.year = @paramYear AND v.scenario = 'Actual' or @paramScenario,
							    'so we only append paramType and paramCompany, if applicable:
							    Dim whereClause As String = ""
							    If Not parameterDict("paramType").Equals("(Any)") Then
							        whereClause += "AND m.type = @paramType "
							    End If
							
							    If Not parameterDict("paramCompany").Equals("0") Then
							        whereClause += "AND m.company_id = @paramCompany "
							    End If
							
							    '-- FIRST QUERY Actual | paraScenario
							    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							        'Construct and execute the query for cumulative cash data
							        Dim selectQuery As String = $"
							            WITH Cash_Cumul_Graph AS (    
							                SELECT
							                    'Actual' AS Scenario,
							                    SUM(cash_m1)  AS Jan,
							                    SUM(cash_m2)  AS Feb,
							                    SUM(cash_m3)  AS Mar,
							                    SUM(cash_m4)  AS Apr,
							                    SUM(cash_m5)  AS May,
							                    SUM(cash_m6)  AS Jun,
							                    SUM(cash_m7)  AS Jul,
							                    SUM(cash_m8)  AS Aug,
							                    SUM(cash_m9)  AS Sep,
							                    SUM(cash_m10) AS Oct,
							                    SUM(cash_m11) AS Nov,
							                    SUM(cash_m12) AS Dec
							                FROM XFC_INV_VIEW_project_cash_ytd v
							                INNER JOIN XFC_INV_MASTER_project m ON v.project_id = m.project
							                WHERE 
							                    v.year = @paramYear
							                    AND v.scenario = 'Actual'
							                    {whereClause}
							
							                UNION ALL
							
							                SELECT
							                    @paramScenario AS Scenario,
							                    SUM(cash_m1)  AS Jan,
							                    SUM(cash_m2)  AS Feb,
							                    SUM(cash_m3)  AS Mar,
							                    SUM(cash_m4)  AS Apr,
							                    SUM(cash_m5)  AS May,
							                    SUM(cash_m6)  AS Jun,
							                    SUM(cash_m7)  AS Jul,
							                    SUM(cash_m8)  AS Aug,
							                    SUM(cash_m9)  AS Sep,
							                    SUM(cash_m10) AS Oct,
							                    SUM(cash_m11) AS Nov,
							                    SUM(cash_m12) AS Dec
							                FROM XFC_INV_VIEW_project_cash_ytd v
							                INNER JOIN XFC_INV_MASTER_project m ON v.project_id = m.project
							                WHERE 
							                    v.year = @paramYear
							                    AND v.scenario = @paramScenario
							                    {whereClause}
							            )
							            SELECT 
							                Scenario,
							                [Month],
							                Amount,
							                CASE [Month]
							                    WHEN 'Jan' THEN 1
							                    WHEN 'Feb' THEN 2
							                    WHEN 'Mar' THEN 3
							                    WHEN 'Apr' THEN 4
							                    WHEN 'May' THEN 5
							                    WHEN 'Jun' THEN 6
							                    WHEN 'Jul' THEN 7
							                    WHEN 'Aug' THEN 8
							                    WHEN 'Sep' THEN 9
							                    WHEN 'Oct' THEN 10
							                    WHEN 'Nov' THEN 11
							                    WHEN 'Dec' THEN 12
							                END AS MonthOrder
							            FROM Cash_Cumul_Graph
							                UNPIVOT
							            (
							                Amount 
							                FOR [Month] IN (
							                    Jan, Feb, Mar, Apr, May, 
							                    Jun, Jul, Aug, Sep, Oct,
							                    Nov, Dec
							                )
							            ) AS unpivoted
							            ORDER BY Scenario, MonthOrder
							        "
							
							        'Execute the query and populate the DataTable
							        dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
							    End Using
							
													
													
													 ' Build the whereClause for the query
							Dim whereClause2 As String = "f.year = @paramYear AND f.scenario = 'Actual' "
							
							If Not parameterDict("paramType").Equals("(Any)") Then
							    whereClause2 += "AND m.type = @paramType "
							End If
							
							If Not parameterDict("paramCompany").Equals("0") Then
							    whereClause2 += "AND m.company_id = @paramCompany "
							End If
							
							' QUERY: Monthly Cash Data
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							    Dim selectQuery1 As String = $"
							        WITH monthlyCash AS (
							            SELECT
							                SUM(CASE WHEN f.month = 1  THEN f.amount ELSE 0 END) AS Jan,
							                SUM(CASE WHEN f.month = 2  THEN f.amount ELSE 0 END) AS Feb,
							                SUM(CASE WHEN f.month = 3  THEN f.amount ELSE 0 END) AS Mar,
							                SUM(CASE WHEN f.month = 4  THEN f.amount ELSE 0 END) AS Apr,
							                SUM(CASE WHEN f.month = 5  THEN f.amount ELSE 0 END) AS May,
							                SUM(CASE WHEN f.month = 6  THEN f.amount ELSE 0 END) AS Jun,
							                SUM(CASE WHEN f.month = 7  THEN f.amount ELSE 0 END) AS Jul,
							                SUM(CASE WHEN f.month = 8  THEN f.amount ELSE 0 END) AS Aug,
							                SUM(CASE WHEN f.month = 9  THEN f.amount ELSE 0 END) AS Sep,
							                SUM(CASE WHEN f.month = 10 THEN f.amount ELSE 0 END) AS Oct,
							                SUM(CASE WHEN f.month = 11 THEN f.amount ELSE 0 END) AS Nov,
							                SUM(CASE WHEN f.month = 12 THEN f.amount ELSE 0 END) AS Dec
							            FROM XFC_INV_FACT_project_cash AS f
							            INNER JOIN XFC_INV_MASTER_project AS m
							                ON f.project_id = m.project
							            WHERE
							                {whereClause2}
							        )
							        SELECT
							            'Monthly Cash' AS Scenario,
							            [Month],
							            Amount,
							            CASE [Month]
							                WHEN 'Jan' THEN 1
							                WHEN 'Feb' THEN 2
							                WHEN 'Mar' THEN 3
							                WHEN 'Apr' THEN 4
							                WHEN 'May' THEN 5
							                WHEN 'Jun' THEN 6
							                WHEN 'Jul' THEN 7
							                WHEN 'Aug' THEN 8
							                WHEN 'Sep' THEN 9
							                WHEN 'Oct' THEN 10
							                WHEN 'Nov' THEN 11
							                WHEN 'Dec' THEN 12
							            END AS MonthOrder
							        FROM monthlyCash
							        UNPIVOT
							        (
							            Amount 
							            FOR [Month] IN (
							                Jan, Feb, Mar, Apr, May, 
							                Jun, Jul, Aug, Sep, Oct,
							                Nov, Dec
							            )
							        ) AS unpivoted
							        ORDER BY MonthOrder
							    "
							
							    ' Execute the first query and populate the DataTable
							    dt1 = BRApi.Database.ExecuteSql(dbConn, selectQuery1, dbParamInfos, False)
							End Using
							
							'QUERY: Budget Cumulative Cash Data
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							    Dim selectQuery2 As String = $"
							        WITH Cash_Cumul_Graph AS (
							            SELECT
							                SUM(cash_m1)  AS Jan,
							                SUM(cash_m2)  AS Feb,
							                SUM(cash_m3)  AS Mar,
							                SUM(cash_m4)  AS Apr,
							                SUM(cash_m5)  AS May,
							                SUM(cash_m6)  AS Jun,
							                SUM(cash_m7)  AS Jul,
							                SUM(cash_m8)  AS Aug,
							                SUM(cash_m9)  AS Sep,
							                SUM(cash_m10) AS Oct,
							                SUM(cash_m11) AS Nov,
							                SUM(cash_m12) AS Dec
							            FROM 
							                XFC_INV_VIEW_project_cash_ytd AS v
							            INNER JOIN 
							                XFC_INV_MASTER_project AS m 
							            ON 
							                v.project_id = m.project
							            WHERE
							                v.year = @paramYear
							                AND v.scenario = 'Budget'
							                AND (
							                    @paramType = '(Any)' 
							                    OR m.type = @paramType
							                )
							                AND (
							                    @paramCompany = 0 
							                    OR m.company_id = @paramCompany
							                )
							        )
							        SELECT
							            [Month],
							            Amount
							        FROM 
							            Cash_Cumul_Graph
							        UNPIVOT
							        (
							            Amount
							            FOR [Month] IN (
							                Jan, Feb, Mar, Apr, May,
							                Jun, Jul, Aug, Sep, Oct,
							                Nov, Dec
							            )
							        ) AS unpivoted
							    "
							
							    ' Execute the second query and populate the DataTable
							    dt2 = BRApi.Database.ExecuteSql(dbConn, selectQuery2, dbParamInfos, False)
							End Using
							
																
							' -- Construction of the series collection --------------------------------
							Dim oSeriesCollection As New XFSeriesCollection()
							
							' Check if the table contains data for the "Actual" scenario
							Dim dtActual As DataTable
							If dt.Select("Scenario = 'Actual'").Length > 0 Then
							    dtActual = dt.Select("Scenario = 'Actual'").CopyToDataTable()
							Else
							    dtActual = dt.Clone() ' Creates an empty table with the same structure
							End If
							
							' Check if the table contains data for the scenario specified by the parameter
							Dim dtScenario As DataTable
							If dt.Select($"Scenario = '{parameterDict("paramScenario")}'").Length > 0 Then
							    dtScenario = dt.Select($"Scenario = '{parameterDict("paramScenario")}'").CopyToDataTable()
							Else
							    dtScenario = dt.Clone() ' Creates an empty table with the same structure
							End If
							
							' Add series for the "Actual Cash Out" scenario
							oSeriesCollection.AddSeries(CreateSeries(si, "Actual Cash Out", XFChart2SeriesType.BarSideBySide, primaryColor, dtActual, "Amount", "Month", False, False, 0, 0, XFChart2ModelType.Bar2DBorderlessSimple, 0.8))
							
							' Add series for the parameter-dependent scenario
							oSeriesCollection.AddSeries(CreateSeries(si, $"{parameterDict("paramScenario")}", XFChart2SeriesType.Line, secondaryColor, dtScenario, "Amount", "Month", False, False, 12, 4, XFChart2ModelType.Bar2DBorderlessSimple, 0.8))
							
							' Add series for "Monthly Cash"
							oSeriesCollection.AddSeries(CreateSeries(si, "Monthly Cash", XFChart2SeriesType.Line, terciaryColor, dt1, "Amount", "Month", True, True, 1, 4, , ))
							
							' Add series for "Budget"
							oSeriesCollection.AddSeries(CreateSeries(si, "Budget", XFChart2SeriesType.Line, "Green", dt2, "Amount", "Month", True, True, 1, 4, , ))
							
							' Return the constructed dataset
							Return oSeriesCollection.CreateDataSet(si)



						#End Region	
								
						#Region "Financial Statements and Project Cash Comparison"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("OSnReportedCashComparison") Then
						    ' Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim operationType As Integer = Convert.ToInt32(parameterDict("operationType"))
						    
						    ' Retrieve the existing DataTable
						    Dim dt As DataTable = GetOSnReportedCashComparison(si, queryParams)
						    
						    ' Return the DataTable or create series for the chart depending on the value of operationType
						    If operationType = 1 Then
						        Return dt
						    ElseIf operationType = 2 Then
						        Dim seriesCollection As XFSeriesCollection = CreateSeriesGraph(si, dt)
						        Return seriesCollection.CreateDataSet(si)
						    Else
						        Throw New ArgumentException("Invalid operationType value. Must be 1 or 2.")
						    End If
						
						#End Region
							
						#Region "Financial Statements and Project Cash Comparison Graph"

						ElseIf args.DataSetName.XFEqualsIgnoreCase("OSnReportedCashComparisonCashGraph") Then
						    ' Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
							'Change company to holding if it's 0
							If queryParams.Contains("paramCompany=[0]") Then queryParams = queryParams.Replace("paramCompany=[0]", "paramCompany=[1300]")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    
						    ' Retrieve the existing DataTable
						    Dim dt As DataTable = GetOSnReportedCashComparison(si, queryParams)
						    
						    ' Retrieve color parameters
						    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
						    Dim secondaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Secondary")
						    Dim terciaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Terciary")
						    
						    ' Create the tables for the series
						    Dim realDataTable As New DataTable()
						    Dim referenceDataTable As New DataTable()
						    Dim differenceDataTable As New DataTable()
						    
						    realDataTable.Columns.Add("Month", GetType(String))
						    realDataTable.Columns.Add("Amount", GetType(Decimal))
						    referenceDataTable.Columns.Add("Month", GetType(String))
						    referenceDataTable.Columns.Add("Amount", GetType(Decimal))
						    differenceDataTable.Columns.Add("Month", GetType(String))
						    differenceDataTable.Columns.Add("Amount", GetType(Decimal))
						    
						    ' Define month names in English
						    Dim monthNames As String() = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}
						    
						    ' Populate the tables based on the available data
						    If dt.Rows.Count > 1 Then
						        Dim realRow As DataRow = dt.AsEnumerable().FirstOrDefault(Function(row) row.Field(Of String)("Source") = "Projects Cash")
						        Dim referenceRow As DataRow = dt.AsEnumerable().FirstOrDefault(Function(row) row.Field(Of String)("Source") = "F.S. PPE Cash")
						        Dim diffRow As DataRow = dt.AsEnumerable().FirstOrDefault(Function(row) row.Field(Of String)("Source") = "Diff.")
						        
						        For i As Integer = 1 To 12
						            Dim monthName As String = monthNames(i - 1)
						            
						            ' Populate "Real" data (SQL data)
						            Dim realAmount As Decimal = If(realRow IsNot Nothing AndAlso Not IsDBNull(realRow($"M{i}")), Convert.ToDecimal(realRow($"M{i}")), 0D)
						            realDataTable.Rows.Add(monthName, realAmount)
						            
						            ' Populate "Reference" data (F.S.)
						            Dim referenceAmount As Decimal = If(referenceRow IsNot Nothing AndAlso Not IsDBNull(referenceRow($"M{i}")), Convert.ToDecimal(referenceRow($"M{i}")), 0D)
						            referenceDataTable.Rows.Add(monthName, referenceAmount)
						            
						            ' Populate "Difference" data (Diff.)
						            Dim differenceAmount As Decimal = If(diffRow IsNot Nothing AndAlso Not IsDBNull(diffRow($"M{i}")), Convert.ToDecimal(diffRow($"M{i}")), 0D)
						            differenceDataTable.Rows.Add(monthName, differenceAmount)
						        Next
						    Else
						        ' If no data is available, populate all tables with months and values set to 0
						        For i As Integer = 1 To 12
						            Dim monthName As String = monthNames(i - 1)
						            realDataTable.Rows.Add(monthName, 0D)
						            referenceDataTable.Rows.Add(monthName, 0D)
						            differenceDataTable.Rows.Add(monthName, 0D)
						        Next
						    End If
						    
						    ' Create the series collection
						    Dim oSeriesCollection As New XFSeriesCollection()
						    
						    ' Add the series to the chart
						    oSeriesCollection.AddSeries(CreateSeries(si, "Reported", XFChart2SeriesType.BarSideBySide, primaryColor, realDataTable, "Amount", "Month", False, False, 0, 0,XFCHart2ModelType.Bar2DBorderlessSimple , 0.8))
						    oSeriesCollection.AddSeries(CreateSeries(si, "OneStream", XFChart2SeriesType.BarSideBySide, secondaryColor, referenceDataTable, "Amount", "Month", False, False, 0, 0,XFCHart2ModelType.Bar2DBorderlessSimple , 0.8))
						    oSeriesCollection.AddSeries(CreateSeries(si, "Difference", XFChart2SeriesType.Line, terciaryColor, differenceDataTable, "Amount", "Month", True, True, 7, , , , , ,True ))
						    
						    ' Return the dataset
						    Return oSeriesCollection.CreateDataSet(si)
						
						#End Region
						
						#Region "Assets info Graph"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("AssetsGraph") Then
							    'Retrieve parameters for queries
							    Dim queryParams As String = args.NameValuePairs("p_query_params")
							    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							    'Retrieve color parameters
							    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
							    Dim secondaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Secondary")
							    Dim terciaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Terciary")
								
								'Declare new DataTables
							    Dim dt As New DataTable()
						
								' SQL query
							    Dim selectQuery As String = $"
									DECLARE @LastRF VARCHAR(18);
									                                            
									SELECT @LastRF = COALESCE(scenario, 'Actual')
									FROM (
									    SELECT TOP(1) scenario
									    FROM XFC_INV_FACT_asset_depreciation ad
									    LEFT JOIN XFC_INV_MASTER_asset a ON ad.asset_id = a.id
									    LEFT JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
									    WHERE
									        [year] = @paramYear - 1
									        AND scenario LIKE 'RF%'
									        AND p.company_id = @paramCompany
									    ORDER BY CAST(REPLACE(LEFT(scenario, 4), 'RF', '') AS INTEGER) DESC
									) AS subquery;
									                                    
									SELECT									   
									    p.type,  -- Añadido para cumplir con GROUP BY
									    a.cost_center_id AS cost_center,          -- Nueva columna añadida: Centro de costo
									    SUM(a.initial_value) AS total_initial_value,
									    SUM(COALESCE(ad.accumulated_amount, 0)) AS total_accumulated_amount,
									    SUM(a.initial_value - COALESCE(ad.accumulated_amount, 0)) AS total_remaining_value
									FROM 
									    XFC_INV_MASTER_asset AS a
									LEFT JOIN
									    (
									        SELECT asset_id, SUM(amount) AS accumulated_amount
									        FROM XFC_INV_FACT_asset_depreciation
									        WHERE
									            (
									                (
									                    @paramScenario = 'Actual'
									                    AND scenario = 'Actual'
									                    AND year * 100 + month < YEAR(GETDATE()) * 100 + MONTH(GETDATE())
									                ) OR (
									                    @paramScenario <> 'Actual'
									                    AND (
									                        year <= @paramYear - 2
									                        AND scenario = 'Actual'
									                    ) OR (
									                       (
									                            @paramScenario <> 'Budget'
									                            AND year = @paramYear - 1
									                            AND scenario = 'Actual'
									                        ) OR (
									                            @paramScenario = 'Budget'
									                            AND year = @paramYear - 1
									                            AND scenario = @LastRF
									                        ) OR (
									                            @paramScenario = 'Budget'
									                            AND year = @paramYear
									                            AND scenario = @paramScenario
									                        )
									                    )
									                )
									            )
									        GROUP BY asset_id
									    ) ad ON a.id = ad.asset_id
									LEFT JOIN 
									    XFC_INV_MASTER_project AS p
									ON 
									    a.project_id = p.project
									WHERE
									   (
									        p.company_id = @paramCompany OR @paramCompany = 0 
									    )
									    AND 
									    (
									        p.aggregate = @paramAggregate OR @paramAggregate = '(Any)'
									    )
									    AND (
									        p.type = @paramType OR @paramType = '(Any)'
									    )
									    AND (
									        (
									            @paramScenario = 'Actual'
									            AND a.is_real = 1
									        ) OR (
									            @paramScenario <> 'Actual'
									        )
									    )
									GROUP BY
									    p.type,  -- Asegurarse de que todas las columnas no agregadas en SELECT están en GROUP BY
									    a.cost_center_id
									ORDER BY
									    p.type,
									    a.cost_center_id;  -- Ordenamiento opcional por tipo y centro de costo


							    "
						    
						    ' Execute the query
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
						    End Using
							
							' Create the series collection
							Dim oSeriesCollection As New XFSeriesCollection()
							
							' Add the series to the chart
							oSeriesCollection.AddSeries(CreateSeries(si, "Acquisition Value", XFChart2SeriesType.BarSideBySide, primaryColor, dt, "total_initial_value","cost_center" , False, False, 0, 0, XFChart2ModelType.Bar2DBorderlessSimple, 0.8))
							oSeriesCollection.AddSeries(CreateSeries(si, "Accumulated Depreciation", XFChart2SeriesType.BarSideBySide, secondaryColor, dt, "total_accumulated_amount", "cost_center", False, False, 0, 0, XFChart2ModelType.Bar2DBorderlessSimple, 0.8))
							oSeriesCollection.AddSeries(CreateSeries(si, "Remaining Value", XFChart2SeriesType.BarSideBySide, TerciaryColor, dt, "total_remaining_value", "cost_center", False, False, 0, 0, XFChart2ModelType.Bar2DBorderlessSimple, 0.8))
							
							Return oSeriesCollection.CreateDataSet(si)
						
						
						
						#End Region 
						
						#Region "Decided/Ordered/Delivered Graph"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("Decided_Graph") Then
						
						    ' 1. GET PARAMETERS
						    
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    
						    ' 2. DEFINE AND EXECUTE THE QUERY
						    
						    Dim dt As New DataTable()
						    
						    Dim selectQuery As String = $"
						        SELECT 
						            f.month        AS [Month],
						            f.type         AS [Type],
						            SUM(f.amount_ytd) AS [Amount]
						        FROM XFC_INV_FACT_project_financial_figures f
						        INNER JOIN XFC_INV_RAW_project r 
						            ON r.project = f.project_id
						        WHERE
						            f.year = @paramYear
						            AND f.scenario = @paramScenario
						            AND f.type IN ('decided', 'ordered', 'delivered')
						            AND (
						                @paramType = '(Any)'
						                OR f.type = @paramType
						            )
						            AND (
						                @paramCompany = 0
						                OR r.company_id = @paramCompany
						            )
						            AND (
						                @paramAggregate = '(Any)'
						                OR r.aggregate = @paramAggregate
						            )
						        GROUP BY 
						            f.month,
						            f.type
						        ORDER BY
						            f.month,
						            CASE f.type
						                WHEN 'decided' THEN 1
						                WHEN 'ordered' THEN 2
						                WHEN 'delivered' THEN 3
						                ELSE 4
						            END
						    "
						
						    ' Execute the query
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
						    End Using
						    
						    ' 3. RETRIEVE COLORS
						    
						    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
						    Dim secondaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Secondary")
						    Dim terciaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Terciary")
						    
						    ' 4. CREATE THE CHART SERIES
						    Dim oSeriesCollection As New XFSeriesCollection()
						
						    Dim desiredOrder As New List(Of String)({"decided", "ordered", "delivered"})
						    Dim colorList As New List(Of String)({primaryColor, secondaryColor, terciaryColor})
						    
						    ' Dictionary to map original names to new names
						    Dim nameMapping As New Dictionary(Of String, String) From {
						        {"decided", "Decided"},
						        {"ordered", "Ordered"},
						        {"delivered", "Delivered"}
						    }
						
						    For i As Integer = 0 To desiredOrder.Count - 1
						        Dim t As String = desiredOrder(i)
						        
						        ' Filter DataView by the current Type
						        Dim filterExpr = "Type = '" & t.Replace("'", "''") & "'"
						        Dim dv As New DataView(dt, filterExpr, "[Month]", DataViewRowState.CurrentRows)
						        
						        ' If there is no data for this type, continue to the next
						        If dv.Count = 0 Then Continue For
						        
						        ' Select a color from the list or a default color
						        Dim chosenColor As String = If(i < colorList.Count, colorList(i), "#808080")
						        
						        ' Create the series name using the mapped name
						        Dim seriesName As String = nameMapping(t)
						        
						        ' Add the series
						        oSeriesCollection.AddSeries(
						            CreateSeries(si,
						                         seriesName,
						                         XFChart2SeriesType.BarSideBySide,
						                         chosenColor,
						                         dv.ToTable(),
						                         "Amount",
						                         "Month",
						                         False,
						                         False,
						                         0,
						                         0,
						                         XFChart2ModelType.Bar2DBorderlessSimple,
						                         0.8)
						        )
						    Next
						
						    ' 5. RETURN THE RESULT TO THE CHART
						    
						    Return oSeriesCollection.CreateDataSet(si)
						
						#End Region
						
						#Region "Company Names"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("CompanyNames") Then
							
							'Declare id to company name dictionary
							Dim IDToCompanyNameDict As New Dictionary(Of Integer, String) From {
								{0, "(Any)"},
								{1300, "Horse Powertrain Solution"},
								{1301, "Horse Spain S.L."},
								{1302, "OYAK HORSE"},
								{1303, "Horse Brasil S.A."},
								{585, "Horse Chile SpA"},
								{592, "Horse Argentina S.A."},
								{611, "Horse Romania S.A."},
								{671, "Horse Aveiro"}
							}
							
							'Get available companies depending on security group and declare data table
							Dim availableCompanies As List(Of Integer) = HelperFunctionsBR.GetCompanyIDsForUser(si)
							Dim companiesDt As New DataTable()
							companiesDt.Columns.Add("id")
							companiesDt.Columns.Add("name")
							
							'Set data table based on the company ids for user
							If availableCompanies.Contains(0) Then
								For Each IDToCompanyName As KeyValuePair(Of Integer, String) In IDToCompanyNameDict
									Dim row As DataRow = companiesDt.NewRow()
									row("id") = IDToCompanyName.Key
									row("name") = IDToCompanyName.Value
									companiesDt.Rows.Add(row)
								Next
							Else
								For Each availableCompany As String In availableCompanies
									Dim row As DataRow = companiesDt.NewRow()
									row("id") = availableCompany
									row("name") = IDToCompanyNameDict(availableCompany)
									companiesDt.Rows.Add(row)
								Next
							End If
							
							Return companiesDt
						
						#End Region
						
						#Region "RND Actual-Budget Comparison Graph"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("RND_Actual_Budget_Comparison_Graph") Then
						
						    ' 1) Read parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						
						    ' Get configuration colors and comparison scenario
						    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
						    Dim secondaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Secondary")
						    Dim terciaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Terciary")
						    Dim scenarioComparison As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "LPW.prm_LPW_Cash_Scenario_Graph")
						
						    ' 2) Declare DataTables for storing data
						    Dim dt As New DataTable()   ' Data for parameter scenario (@paramScenario)
						    Dim dt1 As New DataTable()  ' Data for "Actual"
						
						    ' 3) Build dynamic WHERE clause for the first query (paramScenario)
						    Dim whereClause As String = ""
						    
						    ' Filter by project_activity if not "(Any)"
						    If Not parameterDict("paramProjectActivity").Equals("(Any)") Then
						        whereClause += " AND m.project_activity = @paramProjectActivity "
						    End If
						    
						    ' Filter by company_id if not "0"
						    If Not parameterDict("paramCompany").Equals("0") Then
						        whereClause += " AND m.company_id = @paramCompany "
						    End If
						    
						    ' Filter by client if not "(Any)"
						    If Not parameterDict("paramClient").Equals("(Any)") Then
						        whereClause += "AND e.client = @paramClient "
						    End If
						
						    ' 4) Query for the parameter scenario with YTD calculation and pivot by month
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						    
						        Dim selectQuery As String = $"
						            WITH SummedData AS
						            (
						                SELECT
						                    m.year,
						                    m.scenario,
						                    m.month,
						                    SUM(m.amount) AS total_amount
						                FROM XFC_RND_FACT_project_expense m
						                INNER JOIN XFC_RND_MASTER_project e
						                    ON m.project_id = e.project
											AND m.project_responsible_dir = e.responsible_dir
											AND m.project_job = e.job
											AND m.project_activity = e.activity
											AND m.project_product_family = e.product_family
											AND m.project_info_provider = e.info_provider
											AND m.project_status = e.status
						                WHERE
						                    m.scenario = @paramScenario
						                    AND m.year = @paramYear
						                    {whereClause}
						                GROUP BY
						                    m.year,
						                    m.scenario,
						                    m.month
						            ),
						            CumulativeYTD AS
						            (
						                SELECT
						                    year,
						                    scenario,
						                    month,
						                    SUM(total_amount) OVER
						                    (
						                        PARTITION BY scenario, year
						                        ORDER BY month
						                    ) AS ytd_amount
						                FROM SummedData
						            )
						            SELECT
						                year,
						                scenario,
						                COALESCE([1], 0) AS YTD_M1,
						                COALESCE([2], [1], 0) AS YTD_M2,
						                COALESCE([3], [2], [1], 0) AS YTD_M3,
						                COALESCE([4], [3], [2], [1], 0) AS YTD_M4,
						                COALESCE([5], [4], [3], [2], [1], 0) AS YTD_M5,
						                COALESCE([6], [5], [4], [3], [2], [1], 0) AS YTD_M6,
						                COALESCE([7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M7,
						                COALESCE([8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M8,
						                COALESCE([9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M9,
						                COALESCE([10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M10,
						                COALESCE([11], [10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M11,
						                COALESCE([12], [11], [10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M12
						            FROM
						            (
						                SELECT
						                    year,
						                    scenario,
						                    month,
						                    ytd_amount
						                FROM CumulativeYTD
						            ) AS AggregatedData
						            PIVOT
						            (
						                MAX(ytd_amount)
						                FOR month IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12])
						            ) AS PivotTable;
						        "
						    
						        dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
						    End Using
						
						    ' 5) Build dynamic WHERE clause for the second query (Actual scenario)
						    Dim whereClause2 As String = ""
						    
						    If Not parameterDict("paramProjectActivity").Equals("(Any)") Then
						        whereClause2 += " AND m.project_activity = @paramProjectActivity "
						    End If
						    
						    If Not parameterDict("paramCompany").Equals("0") Then
						        whereClause2 += " AND m.company_id = @paramCompany "
						    End If
						    
						    If Not parameterDict("paramClient").Equals("(Any)") Then
						        whereClause += "AND e.client = @paramClient "
						    End If
						
						    ' 6) Query for the "Actual" scenario with YTD calculation and pivot by month
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						    
						        Dim selectQuery1 As String = $"
						            WITH SummedData AS
						            (
						                SELECT
						                    m.year,
						                    m.scenario,
						                    m.month,
						                    SUM(m.amount) AS total_amount
						                FROM XFC_RND_FACT_project_expense m
						                INNER JOIN XFC_RND_MASTER_project e
						                    ON m.project_id = e.project
											AND m.project_responsible_dir = e.responsible_dir
											AND m.project_job = e.job
											AND m.project_activity = e.activity
											AND m.project_product_family = e.product_family
											AND m.project_info_provider = e.info_provider
											AND m.project_status = e.status
						                WHERE
						                    m.scenario = 'Actual'
						                    AND m.year = @paramYear
						                    {whereClause}
						                GROUP BY
						                    m.year,
						                    m.scenario,
						                    m.month
						            ),
						            CumulativeYTD AS
						            (
						                SELECT
						                    year,
						                    scenario,
						                    month,
						                    SUM(total_amount) OVER
						                    (
						                        PARTITION BY scenario, year
						                        ORDER BY month
						                    ) AS ytd_amount
						                FROM SummedData
						            )
						            SELECT
						                year,
						                scenario,
						                COALESCE([1], 0) AS YTD_M1,
						                COALESCE([2], [1], 0) AS YTD_M2,
						                COALESCE([3], [2], [1], 0) AS YTD_M3,
						                COALESCE([4], [3], [2], [1], 0) AS YTD_M4,
						                COALESCE([5], [4], [3], [2], [1], 0) AS YTD_M5,
						                COALESCE([6], [5], [4], [3], [2], [1], 0) AS YTD_M6,
						                COALESCE([7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M7,
						                COALESCE([8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M8,
						                COALESCE([9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M9,
						                COALESCE([10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M10,
						                COALESCE([11], [10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M11,
						                COALESCE([12], [11], [10], [9], [8], [7], [6], [5], [4], [3], [2], [1], 0) AS YTD_M12
						            FROM
						            (
						                SELECT
						                    year,
						                    scenario,
						                    month,
						                    ytd_amount
						                FROM CumulativeYTD
						            ) AS AggregatedData
						            PIVOT
						            (
						                MAX(ytd_amount)
						                FOR month IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12])
						            ) AS PivotTable;
						        "
						    
						        dt1 = BRApi.Database.ExecuteSql(dbConn, selectQuery1, dbParamInfos, False)
						    End Using
						
						    ' 7) Transform dt and dt1 to chart format (Month, Amount)
						    
						    ' 7.1) Create dtChartScenario for the parameter scenario data
						    Dim dtChartScenario As New DataTable()
						    dtChartScenario.Columns.Add("Month", GetType(Integer))
						    dtChartScenario.Columns.Add("Amount", GetType(Decimal))
						    
						    If dt.Rows.Count > 0 Then
						        ' Assuming dt contains only one row (for the parameter scenario)
						        Dim row As DataRow = dt.Rows(0)
						        For i As Integer = 1 To 12
						            Dim colName As String = "YTD_M" & i
						            If dt.Columns.Contains(colName) Then
						                Dim monthValue As Decimal = If(IsDBNull(row(colName)), 0D, Convert.ToDecimal(row(colName)))
						                dtChartScenario.Rows.Add(i, monthValue)
						            End If
						        Next
						    End If
						
						    ' 7.2) Create dtChartActual for "Actual" data
						    Dim dtChartActual As New DataTable()
						    dtChartActual.Columns.Add("Month", GetType(Integer))
						    dtChartActual.Columns.Add("Amount", GetType(Decimal))
						    
						    If dt1.Rows.Count > 0 Then
						        Dim row As DataRow = dt1.Rows(0)
						        For i As Integer = 1 To 12
						            Dim colName As String = "YTD_M" & i
						            If dt1.Columns.Contains(colName) Then
						                Dim monthValue As Decimal = If(IsDBNull(row(colName)), 0D, Convert.ToDecimal(row(colName)))
						                dtChartActual.Rows.Add(i, monthValue)
						            End If
						        Next
						    End If
						
						    ' 8) Build the chart series
						    Dim oSeriesCollection As New XFSeriesCollection()
						    
						    ' Add parameter scenario series as a bar chart
						    oSeriesCollection.AddSeries( _
						        CreateSeries(si, _
						                     "Actual", _
						                     XFChart2SeriesType.BarSideBySide, _
						                     primaryColor, _
						                     dtChartActual, _
						                     "Amount", _
						                     "Month", _
						                     False, _
						                     False, _
						                     0, _
						                     0, _
						                     XFChart2ModelType.Bar2DBorderlessSimple, _
						                     0.8)
						    )
						    ' Add Actual series as a line chart
						    oSeriesCollection.AddSeries( _
						        CreateSeries(si, _
						                     parameterDict("paramScenario"), _
						                     XFChart2SeriesType.Line, _
						                     secondaryColor, _
						                     dtChartScenario, _
						                     "Amount", _
						                     "Month", _
						                     False, _
						                     False, _
						                     0, _
						                     4, _
						                     , _
						                     0.8)
						    )
						    
						    ' Return the dataset for the chart
						    Return oSeriesCollection.CreateDataSet(si)
						
						#End Region

						#Region "RND Actual vs Scenario by Activity Comparison Graph"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("RND_Actual_Scenario_Activity_Comparison_Graph") Then
						
						    ' 1) Read parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    
						    ' Get the comparison scenario (e.g., Budget)
						    Dim scenarioComparison As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "LPW.prm_LPW_Cash_Scenario_Graph")
						    
						    ' Get series colors
						    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
						    Dim secondaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Secondary")
						    
						    ' 2) Declare DataTables for data storage
						    Dim dtScenario As New DataTable()   ' Data for parameter scenario (e.g., Budget)
						    Dim dtActual As New DataTable()       ' Data for "Actual"
						    
						    ' 3) Build dynamic WHERE clause
						    Dim whereClause As String = ""
						    
						    ' Filter by project_activity if defined
						    If Not parameterDict("paramProjectActivity").Equals("(Any)") Then
						        whereClause += " AND m.project_activity = @paramProjectActivity "
						    End If
						    
						    ' Filter by company_id if not "0"
						    If Not parameterDict("paramCompany").Equals("0") Then
						        whereClause += " AND m.company_id = @paramCompany "
						    End If
						    
						    ' Filter by client if not "(Any)"
						    If Not parameterDict("paramClient").Equals("(Any)") Then
						        whereClause += " AND e.client = @paramClient "
						    End If
						    
						    ' 4) Query for the parameter scenario grouped by activity
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						    
						        Dim selectQueryScenario As String = $"
						            SELECT
						                m.project_activity,
						                SUM(m.amount) AS total_amount
						            FROM XFC_RND_FACT_project_expense m
						            INNER JOIN XFC_RND_MASTER_project e 
						                ON m.project_id = e.project
										AND m.project_responsible_dir = e.responsible_dir
										AND m.project_job = e.job
										AND m.project_activity = e.activity
										AND m.project_product_family = e.product_family
										AND m.project_info_provider = e.info_provider
										AND m.project_status = e.status
						            WHERE
						                m.scenario = @paramScenario
						                AND m.year = @paramYear
						                {whereClause}
						            GROUP BY
						                m.project_activity
						        "
						    
						        dtScenario = BRApi.Database.ExecuteSql(dbConn, selectQueryScenario, dbParamInfos, False)
						    
						    End Using
						    
						    ' 5) Query for "Actual" grouped by activity
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						    
						        Dim selectQueryActual As String = $"
						            SELECT
						                m.project_activity,
						                SUM(m.amount) AS total_amount
						            FROM XFC_RND_FACT_project_expense m
						            INNER JOIN XFC_RND_MASTER_project e 
						                ON m.project_id = e.project
										AND m.project_responsible_dir = e.responsible_dir
										AND m.project_job = e.job
										AND m.project_activity = e.activity
										AND m.project_product_family = e.product_family
										AND m.project_info_provider = e.info_provider
										AND m.project_status = e.status
						            WHERE
						                m.scenario = 'Actual'
						                AND m.year = @paramYear
						                {whereClause}
						            GROUP BY
						                m.project_activity
						        "
						    
						        dtActual = BRApi.Database.ExecuteSql(dbConn, selectQueryActual, dbParamInfos, False)
						    
						    End Using
						    
						    ' 6) Transform data for the chart (Activity, Amount)
						    
						    ' Data for parameter scenario
						    Dim dtChartScenario As New DataTable()
						    dtChartScenario.Columns.Add("Activity", GetType(String))
						    dtChartScenario.Columns.Add("Amount", GetType(Decimal))
						    
						    For Each row As DataRow In dtScenario.Rows
						        Dim activity As String = row("project_activity").ToString()
						        Dim amount As Decimal = If(IsDBNull(row("total_amount")), 0D, Convert.ToDecimal(row("total_amount")))
						        dtChartScenario.Rows.Add(activity, amount)
						    Next
						    
						    ' Data for Actual
						    Dim dtChartActual As New DataTable()
						    dtChartActual.Columns.Add("Activity", GetType(String))
						    dtChartActual.Columns.Add("Amount", GetType(Decimal))
						    
						    For Each row As DataRow In dtActual.Rows
						        Dim activity As String = row("project_activity").ToString()
						        Dim amount As Decimal = If(IsDBNull(row("total_amount")), 0D, Convert.ToDecimal(row("total_amount")))
						        dtChartActual.Rows.Add(activity, amount)
						    Next
						    
						    ' 7) Build chart series
						    Dim oSeriesCollection As New XFSeriesCollection()
						    
						    ' Add parameter scenario series (e.g., Budget) as a bar chart
						    oSeriesCollection.AddSeries( _
						        CreateSeries(si, _
						                     "Actual", _
						                     XFChart2SeriesType.BarSideBySide, _
						                     primaryColor, _
						                     dtChartActual, _
						                     "Amount", _
						                     "Activity", _
						                     False, _
						                     False, _
						                     0, _
						                     0, _
						                     XFChart2ModelType.Bar2DBorderlessSimple, _
						                     0.8) _
						    )
						    
						    ' Add Actual series as a bar chart
						    oSeriesCollection.AddSeries( _
						        CreateSeries(si, _
						                     parameterDict("paramScenario"), _
						                     XFChart2SeriesType.BarSideBySide, _
						                     secondaryColor, _
						                     dtChartScenario, _
						                     "Amount", _
						                     "Activity", _
						                     False, _
						                     False, _
						                     0, _
						                     0, _
						                     XFChart2ModelType.Bar2DBorderlessSimple, _
						                     0.8) _
						    )
						    
						    ' Return the dataset for the chart
						    Return oSeriesCollection.CreateDataSet(si)
						
						#End Region
							
						#Region "Fx Rates"
						
						ElseIf args.DataSetName.XFEqualsIgnoreCase("FxRates") Then
						    ' Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim scenarioMemberId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Scenario, parameterDict("paramScenario"))
							Dim RateTypeRevenueExp As String = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, scenarioMemberId).Name
							dbParamInfos.Add(New DbParamInfo("paramRateTypeRevenueExp", RateTypeRevenueExp))
							Dim RateTypeAssetLiability As String = BRApi.Finance.Scenario.GetFxRateTypeForAssetLiability(si, scenarioMemberId).Name
							dbParamInfos.Add(New DbParamInfo("paramRateTypeAssetLiab", RateTypeAssetLiability))
						    
						    ' Retrieve the existing DataTable
						    Dim dt As New DataTable
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									"
										SELECT
											year,
											type,
											source,
											target,
								            COALESCE([1], 0) AS m1,
								            COALESCE([2], 0) AS m2,
								            COALESCE([3], 0) AS m3,
								            COALESCE([4], 0) AS m4,
								            COALESCE([5], 0) AS m5,
								            COALESCE([6], 0) AS m6,
								            COALESCE([7], 0) AS m7,
								            COALESCE([8], 0) AS m8,
								            COALESCE([9], 0) AS m9,
								            COALESCE([10], 0) AS m10,
								            COALESCE([11], 0) AS m11,
								            COALESCE([12], 0) AS m12
										FROM
							            (
											SELECT year, month, type, source, target, rate
											FROM XFC_MAIN_AUX_fxrate
											WHERE type IN (@paramRateTypeRevenueExp, @paramRateTypeAssetLiab)
												AND year = @paramYear
										) AS SourceTable
							        PIVOT
							        (
							            SUM(rate)
							            FOR month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
							        ) AS PivotTable
									",
									dbParamInfos,
									False
								)
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
		
		#Region "Helper Functions"
		
		#Region "Create Series"
		
		Private Function CreateSeries(ByVal si As SessionInfo, _
		                              name As String, _
		                              type As XFChart2SeriesType, _ 
		                              color As String, _
		                              seriesData As DataTable, _
		                              valueField As String, _
		                              argumentField As String, _
		                              Optional useSecondaryAxis As Boolean = False, _
		                              Optional showMarkers As Boolean = False, _
		                              Optional markerSize As Integer = 0, _
		                              Optional lineThickness As Integer = 0, _
		                              Optional modelType As XFChart2ModelType = Nothing, _
		                              Optional barWidth As Double = 0, _
		                              Optional parameterField As String = "NoIndicado", _
		                              Optional parameterType As String = "NoIndicado", _
		                              Optional useCustomPointColors As Boolean = False) As XFSeries
		    Try
		        Dim series As New XFSeries()
		
		        series.UniqueName = name
		        series.Type = type
		        series.Color = color
		        series.UseSecondaryAxis = useSecondaryAxis
		
		        If type = XFChart2SeriesType.Line Then
		            series.SeriesAnimationType = XFChart2SeriesAnimationType.Line2DUnwrapHorizontally
		            If showMarkers Then
		                series.ShowMarkers = 1
		                series.MarkerSize = markerSize
		            End If
		            If lineThickness > 0 Then
		                series.LineThickness = lineThickness
		            End If
		        ElseIf type = XFChart2SeriesType.BarSideBySide Then
		            series.ModelType = modelType
		            series.BarWidth = barWidth
		        End If
		
		        For Each row As DataRow In seriesData.Rows
		            Dim point As New XFSeriesPoint()
		            point.Argument = If(IsDBNull(row(argumentField)), "N/A", row(argumentField).ToString())
		
		            If Not IsDBNull(row(valueField)) Then
		                point.Value = Convert.ToDouble(row(valueField))
		
		                If type = XFChart2SeriesType.Line Then
		                    If useCustomPointColors Then
		                        ' Color points in red or black based on some condition
		                        point.Color = If(point.Value >= 0, "Green", "Red")
		                    Else
		                        ' Use the line color for the points
		                        point.Color = color
		                    End If
		                End If
		
		                If parameterType = "Texto" Then
		                    point.ParameterValue = If(IsDBNull(row(parameterField)), "No Indicado", "" & row(parameterField) & "")
		                ElseIf parameterType = "Texto2" Then
		                    point.ParameterValue = If(IsDBNull(row(parameterField)), "No Indicado", "'" & row(parameterField) & "'")
		                ElseIf parameterType = "Numero" Then
		                    point.ParameterValue = If(IsDBNull(row(parameterField)), 0, row(parameterField))
		                Else
		                    point.ParameterValue = parameterField
		                End If
		
		                point.Value2 = 100
		                series.AddPoint(point)
		            End If
		        Next
		
		        Return series
		    Catch ex As Exception
		        ' Exception handling
		        Throw New ApplicationException("Error creating the series: " & ex.Message)
		    End Try
		End Function

		
		#End Region

		#Region "Get OS & Reported Cash Comparison"
		
		Private Function GetOSnReportedCashComparison(ByVal si As SessionInfo, ByVal queryParams As String) As DataTable
			Dim dt As New DataTable()
			Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
			Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
			
			'Get company member name
			Dim companyMemberName As String = If(
				parameterDict("paramCompany").ToString.Length < 4,
				$"R0{parameterDict("paramCompany")}",
				$"R{parameterDict("paramCompany")}"
			)
			
			'Get total actual project cash for selected company
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Control param company
				If String.IsNullOrEmpty(parameterDict("paramCompany")) Then
					Return BRApi.Database.ExecuteSql(
						dbConn,
						"SELECT 'No company selected' AS Message",
						False
					)
				End If
				'Return message if no company selected
				If parameterDict("paramCompany") = 0 Then companyMemberName = "Horse_Group"
				
				Dim selectQuery As String = $"
					SELECT
						'Projects Cash' AS Source,
					    MAX(
							CASE
								WHEN @paramCompany = 0 THEN 0
								ELSE company_id
							END
						) AS [Company ID],
					    MAX(
							CASE
								WHEN @paramCompany = 0 THEN 'Horse Group'
								ELSE company_name
							END
						) AS [Company Name],
					    SUM(COALESCE(cash_m1, 0)) AS [M1],
					    SUM(COALESCE(cash_m2, 0)) AS [M2],
					    SUM(COALESCE(cash_m3, 0)) AS [M3],
					    SUM(COALESCE(cash_m4, 0)) AS [M4],
					    SUM(COALESCE(cash_m5, 0)) AS [M5],
					    SUM(COALESCE(cash_m6, 0)) AS [M6],
					    SUM(COALESCE(cash_m7, 0)) AS [M7],
					    SUM(COALESCE(cash_m8, 0)) AS [M8],
					    SUM(COALESCE(cash_m9, 0)) AS [M9],
					    SUM(COALESCE(cash_m10, 0)) AS [M10],
					    SUM(COALESCE(cash_m11, 0)) AS [M11],
					    SUM(COALESCE(cash_m12, 0)) AS [M12],
						SUM(
							COALESCE(
								cash_m1
								+ cash_m2
								+ cash_m3
								+ cash_m4
								+ cash_m5
								+ cash_m6
								+ cash_m7
								+ cash_m8
								+ cash_m9
								+ cash_m10
								+ cash_m11
								+ cash_m12
								, 0
							)
						) AS [Year Total]
					FROM XFC_INV_VIEW_project_cash WITH(NOLOCK)
					WHERE
						(
							company_id = @paramCompany
							OR @paramCompany = 0
						)
						AND year = @paramYear
						AND scenario = 'Actual'
				"
				dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
				'Return message if no project data
				If dt.Rows.Count < 1 Then
					Return BRApi.Database.ExecuteSql(
						dbConn,
						"SELECT 'No project data for selected year' AS Message",
						False
					)
				End If
			End Using
			
			'Add cube data to the datatable
			Dim newRow As DataRow = dt.NewRow()
			'Set initial data
			newRow("Source") = "PPE Cashflow"
			newRow("Company ID") = dt.Rows(0)("Company ID")
			newRow("Company Name") = dt.Rows(0)("Company Name")
			newRow("Year Total") = CDec(0)
			'Set parent and consolidation members based on company id
			Dim parentMember As String = If(parameterDict("paramCompany") = 0, "", "Horse_Group")
			Dim consolidationMember As String = If(parameterDict("paramCompany") = 0, "Local", "Top")
			'Get monthly and year total cash data
			For i As Integer = 1 To 12
				newRow($"M{i}") = Decimal.Round(
					BRApi.Finance.Data.GetDataCellUsingMemberScript(si,
						"Horse",
						$"E#{companyMemberName}:P#{parentMember}:C#{consolidationMember}:S#Actual:T#{parameterDict("paramYear")}M{i}:V#Periodic:A#TINV023_G:F#None:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#Top:U8#None"
					).DataCellEx.DataCell.CellAmount * -1000,
					2
				) - Decimal.Round(
					BRApi.Finance.Data.GetDataCellUsingMemberScript(si,
						"Horse",
						$"E#{companyMemberName}:P#{parentMember}:C#{consolidationMember}:S#Actual:T#{parameterDict("paramYear")}M{i}:V#Periodic:A#TINV0232:F#None:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#CF_AuditTrail:U5#None:U6#None:U7#Top:U8#None"
					).DataCellEx.DataCell.CellAmount * -1000,
					2
				)
				newRow("Year Total") = newRow("Year Total") + newRow($"M{i}")
			Next
			dt.Rows.Add(newRow)
			
			'Generate a row with the difference
			newRow = dt.NewRow()
			'Set initial data
			newRow("Source") = "Diff."
			newRow("Company ID") = dt.Rows(0)("Company ID")
			newRow("Company Name") = dt.Rows(0)("Company Name")
			'Get monthly cash data
			For i As Integer = 1 To 12
				newRow($"M{i}") = Decimal.Round(
					dt.Rows(1)($"M{i}") - If(Decimal.TryParse(dt.Rows(0)($"M{i}").ToString, Nothing), dt.Rows(0)($"M{i}"), 0),
					2
				)
			Next
			'Get year total data
			newRow("Year Total") = Decimal.Round(
					dt.Rows(1)("Year Total") - If(Decimal.TryParse(dt.Rows(0)("Year Total").ToString, Nothing), dt.Rows(0)("Year Total"), 0),
					2
				)
			dt.Rows.Add(newRow)
			Return dt

        End Function
		
		#End Region
		
		#Region "Create Series Graph"

		Private Function CreateSeriesGraph(ByVal si As SessionInfo, ByVal dt As DataTable) As XFSeriesCollection
		    ' Retrieve color parameters
		    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
		    
		    ' Create the table for the chart
		    Dim chartDataTable As New DataTable()
		    chartDataTable.Columns.Add("Month", GetType(String))
		    chartDataTable.Columns.Add("Amount", GetType(Decimal))
		    
		    ' Define month names in English
		    Dim monthNames As String() = {"Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"}
		    
		    ' Check if there is data in the DataTable
		    If dt.Rows.Count > 1 Then
		        ' Filter the row with the "Diff." source
		        Dim diffRow As DataRow = dt.AsEnumerable().FirstOrDefault(Function(row) row.Field(Of String)("Source") = "Diff.")
		        
		        If diffRow IsNot Nothing Then
		            ' Populate the table with the data from the "Diff." row
		            For i As Integer = 1 To 12
		                Dim monthName As String = monthNames(i - 1)
		                Dim amount As Decimal = If(IsDBNull(diffRow($"M{i}")), 0D, Convert.ToDecimal(diffRow($"M{i}")))
		                chartDataTable.Rows.Add(monthName, amount)
		            Next
		        Else
		            ' Handle the case where the "Diff." row is not found
		            Throw New ApplicationException("The row with the 'Diff.' source was not found")
		        End If
		    Else
		        ' If there is no data in dt, populate the table with months and values set to 0
		        For i As Integer = 1 To 12
		            Dim monthName As String = monthNames(i - 1)
		            chartDataTable.Rows.Add(monthName, 0D)
		        Next
		    End If
		    
		    ' Create the series collection
		    Dim oSeriesCollection As New XFSeriesCollection()
		    
		    ' Add the series to the chart
		    oSeriesCollection.AddSeries(CreateSeries(si, "Monthly Diff", XFChart2SeriesType.BarSideBySide, primaryColor, chartDataTable, "Amount", "Month", False, False, 0, 0, XFChart2ModelType.Bar2DBorderlessSimple, 0.8))
		    
		    ' Return the series collection
		    Return oSeriesCollection
		End Function

		#End Region
		
		#Region "Get Planning Scenarios"
		
		Private Function GetPlanningScenarios(ByVal si As SessionInfo, ByVal mainScenario As String) As List(Of Tuple(Of String, String))
			'Get forecast and budget members
			Dim memberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Scenarios", $"S#{mainScenario}.Base", True)
			
			'Declare and populate array of strings (RF02 is not consistent in the dimension)
			Dim memberTuple As New List(Of Tuple(Of String, String))
			If memberList IsNot Nothing AndAlso memberList.Count > 0 Then
				For Each member In memberList
					
					memberTuple.Add(New Tuple(Of String, String)(
						member.Member.Description,
						If(member.Member.Name.ToLower.Contains("forecast_feb"), "RF02", member.Member.Name))
					)
				Next
			End If
			
			Return memberTuple
		End Function
	
		#End Region
		
		
		#End Region
		
	End Class
End Namespace
