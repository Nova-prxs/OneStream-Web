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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_projections
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get function name
						Dim functionName As String = args.NameValuePairs("p_function")
						
						'Control function name
						
						#Region "Cash"
						
						#Region "Project Cash"
						
						If functionName = "ProjectCash" Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim paramOnlyNewProjects As Boolean = True
							Boolean.TryParse(args.NameValuePairs("p_only_new_projects"), paramOnlyNewProjects)
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
							
							'Get if user is projecting only the selected project
							Dim whereProjectClause As String = String.Empty
							If parameterDict.ContainsKey("paramProject") Then whereProjectClause = "AND p.project = @paramProject"
							
							'If user has not selected a company, throw error
							If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
							
							'Check if it's confirmed
							If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
								Throw ErrorHandler.LogWrite(si, New XFException("You can't modify the data, scenario and year are confirmed."))
							End If
							
							'Build query to get the projects based on param only new projects
							Dim projectSelectQuery As String = String.Empty
							If paramOnlyNewProjects AndAlso Not parameterDict.ContainsKey("paramProject") Then
								projectSelectQuery = $"
									SELECT project, poi_poe, ma_date, total_requirement
									FROM XFC_INV_MASTER_project p WITH(NOLOCK)
									LEFT JOIN (
										SELECT DISTINCT project_id
										FROM XFC_INV_FACT_project_cash
										WHERE
											year = @paramYear
											AND scenario = @paramScenario
											AND month > @paramForecastMonth
									) pc ON p.project = pc.project_id
									WHERE
										pc.project_id IS NULL
										AND p.company_id = @paramCompany
										{whereProjectClause}
								"
							Else
								projectSelectQuery = $"
									SELECT project, poi_poe, ma_date, total_requirement
									FROM XFC_INV_MASTER_project p WITH(NOLOCK)
									WHERE
										p.company_id = @paramCompany
										{whereProjectClause}
								"
							End If
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Clear projections and copy all actuals if doing a full projection
								If Not paramOnlyNewProjects AndAlso Not parameterDict.ContainsKey("paramProject") Then BRApi.Database.ExecuteSql(
									dbConn,
									$"
										DELETE pc
										FROM XFC_INV_FACT_project_cash pc
										INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
										WHERE
											pc.year = @paramYear
											AND pc.scenario = @paramScenario
											AND p.company_id = @paramCompany;
									
										INSERT INTO XFC_INV_FACT_project_cash (
											project_id,
											scenario,
											year,
											month,
											amount
										)
										SELECT
											pc.project_id,
											@paramScenario AS scenario,
											pc.year,
											pc.month,
											pc.amount
										FROM XFC_INV_FACT_project_cash pc
										INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
										WHERE p.company_id = @paramCompany
											AND pc.year = @paramYear
											AND pc.scenario = 'Actual'
											AND month <= @paramForecastMonth
									",
									dbParamInfos,
									False
								)
								'Clear projections and copy actuals if doing a single projection or new projects projection
								If paramOnlyNewProjects OrElse parameterDict.ContainsKey("paramProject") Then BRApi.Database.ExecuteSql(
									dbConn,
									$"
										DELETE pc
										FROM XFC_INV_FACT_project_cash pc
										INNER JOIN (
											{projectSelectQuery}
										) p ON pc.project_id = p.project
										WHERE
											pc.year = @paramYear
											AND pc.scenario = @paramScenario;
									
										INSERT INTO XFC_INV_FACT_project_cash (
											project_id,
											scenario,
											year,
											month,
											amount
										)
										SELECT
											pc.project_id,
											@paramScenario AS scenario,
											pc.year,
											pc.month,
											pc.amount
										FROM XFC_INV_FACT_project_cash pc
										INNER JOIN (
											{projectSelectQuery}
										) p ON pc.project_id = p.project
										WHERE  pc.year = @paramYear
											AND pc.scenario = 'Actual'
											AND month <= @paramForecastMonth
									",
									dbParamInfos,
									False
								)
								'Project
								Dim projectionQuery As String = $"
									-- Delete projected projects from modified projections table
									DELETE aux
									FROM XFC_INV_AUX_modified_projection aux
									INNER JOIN (
									    SELECT DISTINCT project
									    FROM (
											{projectSelectQuery}
										) AS subquery
									) pm ON aux.project_id = pm.project
									WHERE aux.type = 'cash'
									  AND aux.year = @paramYear
									  AND aux.scenario = @paramScenario;
								
									DECLARE @lastRF VARCHAR(255);
									
									IF @paramForecastMonth = 0
									BEGIN
										SELECT @lastRF = scenario
										FROM (
											SELECT TOP(1) scenario
											FROM XFC_INV_FACT_project_cash pc
											LEFT JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
											WHERE
												year = @paramYear - 1
												AND scenario LIKE 'RF%'
												AND p.company_id = @paramCompany
											ORDER BY CAST(REPLACE(LEFT(scenario, 4), 'RF', '') AS INTEGER) DESC
										) AS subquery;
										IF @lastRF IS NULL
										BEGIN
    										RAISERROR('No matching RF scenario found for the previous year.', 16, 1);
											RETURN;
										END
									END;
								
									WITH months_cte AS (
										SELECT 1 AS month
										UNION ALL
										SELECT month + 1
										FROM months_cte
										WHERE month < 12
									), projects_cte AS (
										{projectSelectQuery} 
									), projects_months_cte AS (
										SELECT 
											p.*,
											@paramYear AS year,
											m.month AS month,
											CASE
												WHEN LOWER(REPLACE(p.poi_poe, '.', '')) = 'poi' THEN p.total_requirement * pcsp.poi_percentage
												WHEN LOWER(REPLACE(p.poi_poe, '.', '')) = 'poe' THEN p.total_requirement * pcsp.poe_percentage
												ELSE 0
											END AS calculated_cash_ytd
									    FROM projects_cte p
									    CROSS JOIN months_cte m
										INNER JOIN XFC_INV_MASTER_project_cash_sop_percentage pcsp
											ON DATEDIFF(MONTH, p.ma_date, DATEFROMPARTS(@paramYear, m.month, 25)) = pcsp.sop_month_difference
									)
								
									MERGE INTO XFC_INV_FACT_project_cash AS target
									USING (
										SELECT
											pm.project AS project_id,
											@paramScenario AS scenario,
											pm.year AS year,
											pm.month AS month,
											CASE
												WHEN pm.month <= @paramForecastMonth THEN COALESCE(pca.amount, 0)
												WHEN pm.month = @paramForecastMonth + 1 THEN GREATEST(
													pm.calculated_cash_ytd - COALESCE(pcpy.total_actual_amount, 0),
													0
												)
												ELSE GREATEST(
													pm.calculated_cash_ytd - GREATEST(
														LAG(pm.calculated_cash_ytd) OVER(
															PARTITION BY pm.project
															ORDER BY pm.year ASC, pm.month ASC
														),
														COALESCE(pcpy.total_actual_amount, 0)
													),
													0
												)
											END AS amount
										FROM projects_months_cte pm
								
										--Get actual cash for each month of projected year
										LEFT JOIN (
											SELECT project_id, month, amount
											FROM XFC_INV_FACT_project_cash
											WHERE
												year = @paramYear
												AND scenario = 'Actual'
										) pca ON
											pm.project = pca.project_id
											AND pm.month = pca.month
								
										-- Get accumulated actual cash from prev months
										LEFT JOIN (
											SELECT
												project_id,
												SUM(COALESCE(amount, 0)) AS total_actual_amount
											FROM XFC_INV_FACT_project_cash
											WHERE
												(
													@paramForecastMonth <> 0
													AND scenario = 'Actual'
													AND year * 100 + month <= @paramYear * 100 + @paramForecastMonth
												) OR (
													@paramForecastMonth = 0
													AND (
														(
															year < @paramYear - 1
															AND scenario = 'Actual'
														) OR (
															year = @paramYear - 1
															AND scenario = @lastRF
														)
													)
												)
											GROUP BY project_id
										) pcpy ON
											pm.project = pcpy.project_id
									) AS source
									ON target.project_id = source.project_id
									AND target.scenario = source.scenario
									AND target.year = source.year
									AND target.month = source.month
									WHEN MATCHED THEN
									    UPDATE SET
									        amount = source.amount
									WHEN NOT MATCHED THEN
									    INSERT (
									        project_id, scenario, year, month, amount
									    )
									    VALUES (
									        source.project_id, source.scenario, source.year, source.month, source.amount
									    );
								"
								BRApi.Database.ExecuteSql(dbConn, projectionQuery, dbParamInfos, False)
							End Using
						
						#End Region
						
						#Region "Clear Cash Projections"
						
						Else If functionName = "ClearCashProjections" Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
							
							'If user has not selected a company, throw error
							If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
							
							'Check if it's confirmed
							If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
								Throw ErrorHandler.LogWrite(si, New XFException("You can't modify the data, scenario and year are confirmed."))
							End If
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim deleteQuery As String = $"
									-- Delete projections
									DELETE pc
									FROM XFC_INV_FACT_project_cash pc
									INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
									WHERE
										pc.year = @paramYear
										AND pc.scenario = @paramScenario
										AND p.company_id = @paramCompany;
								
									-- Delete modified projections
									DELETE mp
									FROM XFC_INV_AUX_modified_projection mp
									INNER JOIN XFC_INV_MASTER_project p ON mp.project_id = p.project
									WHERE
										mp.type = 'cash'
										AND mp.year = @paramYear
										AND mp.scenario = @paramScenario
										AND p.company_id = @paramCompany;
								"
								BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
							End Using
						
						#End Region
						
						#End Region
						
						#Region "DnA"
						
						#Region "Project DnA"
						
						Else If functionName = "ProjectDnA" Then
						    'Get parameters for queries
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
						    dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
						    
						    'If user has not selected a company, throw error
						    If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
						    
						    'Check if it's confirmed
						    If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
						        Throw ErrorHandler.LogWrite(si, New XFException("You can't modify the data, scenario and year are confirmed."))
						    End If
						    
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        Dim projectionQuery As String = $"
						            WITH max_year_cte AS (
						                SELECT MAX(ac.years_amortization_conso) AS max_year
						                FROM XFC_INV_MASTER_asset a WITH(NOLOCK)
						                INNER JOIN XFC_INV_MASTER_project p WITH(NOLOCK) ON a.project_id = p.project
						                INNER JOIN XFC_INV_MASTER_asset_category ac WITH(NOLOCK) ON a.category_id = ac.id
						                -- JOIN con la tabla auxiliar para filtrar por scenario y año
						                INNER JOIN XFC_INV_AUX_Fictitious_Assets aux WITH(NOLOCK) ON 
						                    a.id = aux.asset_id 
						                    AND a.project_id = aux.project_id
						                    AND aux.scenario = @paramScenario
						                    AND aux.year = @paramYear
						                WHERE p.company_id = @paramCompany
						            ),
						            year_cte AS (
						                SELECT 1 AS asset_year
						                UNION ALL
						                SELECT asset_year + 1
						                FROM year_cte
						                CROSS JOIN max_year_cte
						                WHERE asset_year < max_year_cte.max_year
						            ), month_cte AS (
						                SELECT @paramYear AS year, 1 AS month
						                UNION ALL
						                SELECT @paramYear AS year, month + 1 AS month
						                FROM month_cte
						                WHERE month < 12
						            ), date_dimension_cte AS (
						                SELECT
						                    d.*
						                FROM XFC_INV_AUX_date_dimension d
						                LEFT JOIN XFC_INV_MASTER_company_holiday ch
						                ON d.date = ch.date
						                    AND ch.company_id = @paramCompany
						                WHERE
						                    ch.date IS NULL
						            ), asset_year_cte AS (
						                SELECT
						                    a.asset_id AS asset_id,
						                    a.project_id AS project_id,
						                    a.ma_date AS ma_date,
						                    ycte.asset_year AS asset_year,
						                    a.initial_value / a.years_amortization_conso AS yearly_depreciation,
						                    DATEADD(YEAR, ycte.asset_year - 1, a.ma_date) AS start_date,
						                    DATEADD(DAY, -1, DATEADD(YEAR, ycte.asset_year, a.ma_date)) AS end_date,
						                    (
						                        SELECT
						                            COUNT(is_weekday) AS total_working_days
						                        FROM date_dimension_cte
						                        WHERE
						                            date BETWEEN
						                                DATEADD(YEAR, ycte.asset_year - 1, a.ma_date)
						                                AND DATEADD(DAY, -1, DATEADD(YEAR, ycte.asset_year, a.ma_date))
						                            AND is_weekday = 1
						                    ) AS total_working_days
						                FROM
						                    year_cte ycte
						                CROSS JOIN (
						                    SELECT DISTINCT a.id AS asset_id, a.project_id, a.initial_value, a.activation_date AS ma_date, ac.years_amortization_conso
						                    FROM XFC_INV_MASTER_project p WITH(NOLOCK)
						                    INNER JOIN XFC_INV_MASTER_asset a WITH(NOLOCK) ON a.project_id = p.project
						                    INNER JOIN XFC_INV_MASTER_asset_category ac WITH(NOLOCK) ON a.category_id = ac.id
						                    -- JOIN con la tabla auxiliar para filtrar por scenario y año
						                    INNER JOIN XFC_INV_AUX_Fictitious_Assets aux WITH(NOLOCK) ON 
						                        a.id = aux.asset_id 
						                        AND a.project_id = aux.project_id
						                        AND aux.scenario = @paramScenario
						                        AND aux.year = @paramYear
						                    WHERE p.company_id = @paramCompany
						                        AND YEAR(a.activation_date) > 1901
						                        AND a.is_real = 0
						                ) a
						                WHERE ycte.asset_year <= a.years_amortization_conso
						                    AND @paramYear BETWEEN
						                        YEAR(DATEADD(YEAR, ycte.asset_year - 1, a.ma_date))
						                        AND YEAR(DATEADD(DAY, -1, DATEADD(YEAR, ycte.asset_year, a.ma_date)))
						            ), asset_year_month_cte AS (
						                SELECT
						                    aycte.asset_id,
						                    aycte.project_id,
						                    mcte.year,
						                    mcte.month,
						                    CAST(
						                        (
						                        SELECT
						                            COUNT(is_weekday) AS total_working_days
						                        FROM date_dimension_cte
						                        WHERE
						                            date BETWEEN
						                                GREATEST(
						                                    aycte.start_date,
						                                    DATEFROMPARTS(
						                                        mcte.year,
						                                        mcte.month,
						                                        1
						                                    )
						                                )
						                                AND LEAST(
						                                    aycte.end_date,
						                                    DATEADD(
						                                        DAY,
						                                        -1,
						                                        DATEADD(
						                                            MONTH,
						                                            1,
						                                            DATEFROMPARTS(
						                                                mcte.year,
						                                                mcte.month,
						                                                1
						                                            )
						                                        )
						                                    )
						                                )
						                            AND is_weekday = 1
						                        ) AS DEC(18, 4))
						                    / total_working_days
						                    * yearly_depreciation AS monthly_depreciation
						                FROM asset_year_cte aycte
						                INNER JOIN month_cte mcte ON
						                    mcte.year * 100 + mcte.month BETWEEN
						                        YEAR(aycte.start_date) * 100 + MONTH(aycte.start_date)
						                        AND YEAR(aycte.end_date) * 100 + MONTH(aycte.end_date)
						            )
						            
						            MERGE INTO XFC_INV_FACT_asset_depreciation AS target
						            USING (
						                SELECT asset_id, project_id, @paramScenario AS scenario, year, month, 'conso' AS Type, SUM(monthly_depreciation) AS amount
						                FROM asset_year_month_cte
						                GROUP BY asset_id, project_id, year, month
						            ) AS source
						            ON target.asset_id = source.asset_id
						            AND target.project_id = source.project_id
						            AND target.scenario = source.scenario
						            AND target.year = source.year
						            AND target.month = source.month
						            AND target.type = source.type
						            WHEN MATCHED THEN
						                UPDATE SET
						                    amount = source.amount
						            WHEN NOT MATCHED THEN
						                INSERT (
						                    asset_id, project_id, scenario, year, month, type, amount
						                )
						                VALUES (
						                    source.asset_id, source.project_id, source.scenario, source.year, source.month, source.type, source.amount
						                );
						        "
						        BRApi.Database.ExecuteSql(dbConn, projectionQuery, dbParamInfos, False)
						    End Using
						
						#End Region
						
						#Region "Clear DnA Projections"
						
						Else If functionName = "ClearDnAProjections" Then
						    'Get parameters for queries
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
						    dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
						    
						    'If user has not selected a company, throw error
						    If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
						    
						    'Check if it's confirmed
						    If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
						        Throw ErrorHandler.LogWrite(si, New XFException("You can't modify the data, scenario and year are confirmed."))
						    End If
						    If parameterDict("paramIsReal") = 0 
							    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							        Dim deleteQuery As String = $"
							            DELETE ad
							            FROM XFC_INV_FACT_asset_depreciation ad
							            INNER JOIN XFC_INV_MASTER_asset a ON ad.asset_id = a.id
							            INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
							            INNER JOIN XFC_INV_AUX_Fictitious_Assets aux ON 
							                a.id = aux.asset_id 
							                AND a.project_id = aux.project_id
							                AND aux.scenario = @paramScenario
							                AND aux.year = @paramYear
							            WHERE
							                ad.year = @paramYear
							                AND ad.scenario = @paramScenario
							                AND p.company_id = @paramCompany
							                AND a.is_real = @paramIsReal;
							        "
							        BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
							    End Using
							
							Else 
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									Dim deleteQuery As String = $"
										-- Delete projections
										DELETE ad
										FROM XFC_INV_FACT_asset_depreciation ad
										INNER JOIN XFC_INV_MASTER_asset a ON ad.asset_id = a.id
										INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
										WHERE
											ad.year = @paramYear
											AND ad.scenario = @paramScenario
											AND p.company_id = @paramCompany
											AND a.is_real = @paramIsReal;
									"
									BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
								End Using
								
							End if
						
						#End Region
						
						#End Region
						
						#Region "Capitalizations"
						
						#Region "Project Capitalizations"
						
						Else If functionName = "ProjectCapitalizations" Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim paramOnlyNewProjects As Boolean = True
							Boolean.TryParse(args.NameValuePairs("p_only_new_projects"), paramOnlyNewProjects)
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
							
							'Get if user is projecting only the selected project
							Dim whereProjectClause As String = String.Empty
							If parameterDict.ContainsKey("paramProject") Then whereProjectClause = "AND p.project = @paramProject"
							
							'If user has not selected a company, throw error
							If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
							
							'Check if it's confirmed
							If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
								Throw ErrorHandler.LogWrite(si, New XFException("You can't modify the data, scenario and year are confirmed."))
							End If
							
							'Build queries to get projects and assets based on param only new projects
							Dim projectSelectQuery As String = String.Empty
							Dim assetSelectQuery As String = String.Empty
							If paramOnlyNewProjects AndAlso Not parameterDict.ContainsKey("paramProject") Then
								projectSelectQuery = $"
									SELECT
										project,
										MONTH(
											GREATEST(
												DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1),
												DATEFROMPARTS(YEAR(start_date), MONTH(start_date), 1)
											)
										) AS start_month,
										limit_month,
										is_real,
										(requirement_amount - delivered_amount)
										/ capitalization_months AS capitalization_amount
									FROM (
										SELECT
											p.project,
											p.start_date,
											MONTH(
												LEAST(
													DATEFROMPARTS(YEAR(p.ma_date), MONTH(p.ma_date), 1),
													DATEFROMPARTS(@paramYear, 12, 1)
												)
											) AS limit_month,
											p.is_real,
											COALESCE(
												(
													SELECT TOP 1 amount_ytd
													FROM XFC_INV_FACT_project_financial_figures
													WHERE project_id = p.project
														AND scenario = 'Actual'
														AND year = @paramYear
														AND month = 12
														AND type = 'Requirement'
												),
												0
											) AS requirement_amount,
											COALESCE(
												(
													SELECT TOP 1 amount_ytd
													FROM XFC_INV_FACT_project_financial_figures
													WHERE project_id = p.project
														AND scenario = 'Actual'
														AND year = @paramYear
														AND month = 12
														AND type = 'Delivered'
												),
												0
											) AS delivered_amount,
											DATEDIFF(
												MONTH,
												GREATEST(
													DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1),
													DATEFROMPARTS(YEAR(start_date), MONTH(start_date), 1)
												),
												LEAST(
													DATEFROMPARTS(@paramYear, 12, 1),
													DATEFROMPARTS(YEAR(ma_date), MONTH(ma_date), 1)
												)
											) + 1 AS capitalization_months
										FROM XFC_INV_MASTER_project p WITH(NOLOCK)
										LEFT JOIN (
											SELECT DISTINCT project_id
											FROM XFC_INV_FACT_project_capitalization
											WHERE
												year = @paramYear
												AND scenario = @paramScenario
										) pc ON p.project = pc.project_id
										WHERE
											pc.project_id IS NULL
											AND p.company_id = @paramCompany
											AND DATEFROMPARTS(YEAR(ma_date), MONTH(ma_date), 1)
												>= DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1)
											{whereProjectClause}
									) AS subquery
									WHERE requirement_amount > delivered_amount
										AND DATEFROMPARTS(YEAR(start_date), MONTH(start_date), 1)
											< DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1)
								"
								assetSelectQuery = $"
									SELECT a.project_id, a.activation_date, a.initial_value
									FROM XFC_INV_MASTER_asset a
									INNER JOIN XFC_INV_MASTER_project p
									ON a.project_id = p.project
									LEFT JOIN XFC_INV_FACT_project_capitalization pc
									ON p.project = pc.project_id
										AND pc.year = @paramYear
										AND pc.scenario = @paramScenario
									WHERE pc.project_id IS NULL
										AND p.company_id = @paramCompany
										AND a.is_real = 0
										AND YEAR(a.activation_date) = @paramYear
										AND MONTH(a.activation_date) > @paramForecastMonth
								"
							Else
								projectSelectQuery = $"
									SELECT
										project,
										MONTH(
											GREATEST(
												DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1),
												DATEFROMPARTS(YEAR(start_date), MONTH(start_date), 1)
											)
										) AS start_month,
										limit_month,
										is_real,
										(requirement_amount - delivered_amount)
										/ capitalization_months AS capitalization_amount
									FROM (
										SELECT
											p.project,
											p.start_date,
											MONTH(
												LEAST(
													DATEFROMPARTS(YEAR(p.ma_date), MONTH(p.ma_date), 1),
													DATEFROMPARTS(@paramYear, 12, 1)
												)
											) AS limit_month,
											p.is_real,
											COALESCE(
												(
													SELECT TOP 1 amount_ytd
													FROM XFC_INV_FACT_project_financial_figures
													WHERE project_id = p.project
														AND scenario = 'Actual'
														AND year = @paramYear
														AND month = 12
														AND type = 'Requirement'
												),
												0
											) AS requirement_amount,
											COALESCE(
												(
													SELECT TOP 1 amount_ytd
													FROM XFC_INV_FACT_project_financial_figures
													WHERE project_id = p.project
														AND scenario = 'Actual'
														AND year = @paramYear
														AND month = 12
														AND type = 'Delivered'
												),
												0
											) AS delivered_amount,
											DATEDIFF(
												MONTH,
												GREATEST(
													DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1),
													DATEFROMPARTS(YEAR(start_date), MONTH(start_date), 1)
												),
												LEAST(
													DATEFROMPARTS(@paramYear, 12, 1),
													DATEFROMPARTS(YEAR(ma_date), MONTH(ma_date), 1)
												)
											) + 1 AS capitalization_months
										FROM XFC_INV_MASTER_project p WITH(NOLOCK)
										WHERE p.company_id = @paramCompany
											AND DATEFROMPARTS(YEAR(ma_date),MONTH(ma_date),1)
												>= DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1)
											{whereProjectClause}
									) AS subquery
									WHERE requirement_amount > delivered_amount
										AND DATEFROMPARTS(YEAR(start_date), MONTH(start_date), 1)
											< DATEFROMPARTS(@paramYear, @paramForecastMonth + 1, 1)
								"
								assetSelectQuery = $"
									SELECT a.project_id, a.activation_date, a.initial_value
									FROM XFC_INV_MASTER_asset a
									INNER JOIN XFC_INV_MASTER_project p
									ON a.project_id = p.project
									WHERE p.company_id = @paramCompany
										AND a.is_real = 0
										AND YEAR(a.activation_date) = @paramYear
										AND MONTH(a.activation_date) > @paramForecastMonth
								"
							End If
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Clear projections if doing a full projection
								If Not paramOnlyNewProjects AndAlso Not parameterDict.ContainsKey("paramProject") Then BRApi.Database.ExecuteSql(
									dbConn,
									"
										DELETE pc
										FROM XFC_INV_FACT_project_capitalization pc
										INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
										WHERE
											pc.year = @paramYear
											AND pc.scenario = @paramScenario
											AND p.company_id = @paramCompany;
									",
									dbParamInfos,
									False
								)
								'Clear projections if doing a single projection
								If parameterDict.ContainsKey("paramProject") Then BRApi.Database.ExecuteSql(
									dbConn,
									"
										DELETE pc
										FROM XFC_INV_FACT_project_capitalization pc
										INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
										WHERE
											pc.year = @paramYear
											AND pc.scenario = @paramScenario
											AND p.company_id = @paramCompany
											AND p.project = @paramProject;
									",
									dbParamInfos,
									False
								)
								'Project
								Dim projectionQuery As String = $"
									-- Delete projected projects from modified projections table
									DELETE aux
									FROM XFC_INV_AUX_modified_projection aux
									INNER JOIN (
									    SELECT DISTINCT project
									    FROM (
											{projectSelectQuery}
										) AS subquery
								
										UNION ALL
								
										SELECT DISTINCT project_id AS project
										FROM (
											{assetSelectQuery}
										) AS subquery
									) pm ON aux.project_id = pm.project
									WHERE aux.type = 'capitalization'
									  AND aux.year = @paramYear
									  AND aux.scenario = @paramScenario;
								
									WITH months_cte AS (
										SELECT 1 AS month
										UNION ALL
										SELECT month + 1
										FROM months_cte
										WHERE month < 12
									), projects_cte AS (
										{projectSelectQuery} 
									), projects_months_cte AS (
										SELECT 
											p.*,
											@paramYear AS year,
											m.month AS month
									    FROM projects_cte p
									    CROSS JOIN months_cte m
										WHERE m.month <= p.limit_month
											AND m.month > @paramForecastMonth
											AND m.month >= start_month
									)
								
									MERGE INTO XFC_INV_FACT_project_capitalization AS target
									USING (
										SELECT
											project_id,
											year,
											month,
											scenario,
											SUM(amount) AS amount
										FROM (
											SELECT
												project AS project_id,
												year,
												month,
												@paramScenario AS scenario,
												capitalization_amount AS amount
											FROM projects_months_cte
											WHERE is_real = 1
									
											UNION ALL
											
											SELECT
												project_id,
												YEAR(activation_date) AS year,
												MONTH(activation_date) AS month,
												@paramScenario AS scenario,
												initial_value AS amount
											FROM (
												{assetSelectQuery}
											) AS subquery
										) AS subquery
										GROUP BY
											project_id,
											year,
											month,
											scenario
									) AS source
									ON target.project_id = source.project_id
										AND target.year = source.year
										AND target.month = source.month
										AND target.scenario = source.scenario
									WHEN MATCHED THEN
										UPDATE SET amount = source.amount
									WHEN NOT MATCHED THEN
										INSERT (project_id, year, month, scenario, amount)
										VALUES (source.project_id, source.year, source.month, source.scenario, source.amount);
								"
								BRApi.Database.ExecuteSql(dbConn, projectionQuery, dbParamInfos, False)
							End Using
						
						#End Region
						
						#Region "Clear Capitalization Projections"
						
						Else If functionName = "ClearCapitalizationProjections" Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
							
							'If user has not selected a company, throw error
							If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
							
							'Check if it's confirmed
							If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
								Throw ErrorHandler.LogWrite(si, New XFException("You can't modify the data, scenario and year are confirmed."))
							End If
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim deleteQuery As String = $"
									-- Delete projections
									DELETE pc
									FROM XFC_INV_FACT_project_capitalization pc
									INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
									WHERE
										pc.year = @paramYear
										AND pc.scenario = @paramScenario
										AND p.company_id = @paramCompany;
								
									-- Delete modified projections
									DELETE mp
									FROM XFC_INV_AUX_modified_projection mp
									INNER JOIN XFC_INV_MASTER_project p ON mp.project_id = p.project
									WHERE
										mp.type = 'capitalization'
										AND mp.year = @paramYear
										AND mp.scenario = @paramScenario
										AND p.company_id = @paramCompany;
								"
								BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
							End Using
						
						#End Region
						
						#End Region
						
						#Region "Seed RF Data"
						
						Else If functionName = "SeedRFData" Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
							
							'If user has not selected a company, throw error
							If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
							
							'Check if it's confirmed
							If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
								Throw ErrorHandler.LogWrite(si, New XFException("You can't modify the data, scenario and year are confirmed."))
							End If
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Seed all actuals
								BRApi.Database.ExecuteSql(
									dbConn,
									$"
										DELETE pc
										FROM XFC_INV_FACT_project_cash pc
										INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
										WHERE
											pc.year = @paramYear
											AND pc.scenario = @paramScenario
											AND p.company_id = @paramCompany
											AND month <= @paramForecastMonth;
									
										INSERT INTO XFC_INV_FACT_project_cash (
											project_id,
											scenario,
											year,
											month,
											amount
										)
										SELECT
											pc.project_id,
											@paramScenario AS scenario,
											pc.year,
											pc.month,
											pc.amount
										FROM XFC_INV_FACT_project_cash pc
										INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
										WHERE p.company_id = @paramCompany
											AND pc.year = @paramYear
											AND pc.scenario = 'Actual'
											AND month <= @paramForecastMonth
									",
									dbParamInfos,
									False
								)
							End Using
						
						#End Region
							
						End If
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
