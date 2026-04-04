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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class inv_view_project_cash_gap_summary_Site

        'Declare view and table name
		Dim viewName As String = "XFC_INV_VIEW_project_cash_gap_summary_Site"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
					Dim upQuery As String = $"
					IF OBJECT_ID(N'dbo.{viewName}', N'V') IS NULL
					BEGIN
					    EXEC('
					        CREATE VIEW dbo.{viewName} AS
							WITH ActualData AS (
							    SELECT 
							        COALESCE(p.company_id, y.company_id) AS company_id,
									COALESCE(p.type, y.type) AS type,
									COALESCE(p.site, y.site) AS site,
							        COALESCE(p.month, y.month) AS month,
							        COALESCE(p.year, y.year) AS year,
							        COALESCE(p.scenario, y.scenario) AS scenario,
									COALESCE(p.amount_periodic, 0) AS actual_periodic,
							        COALESCE(y.amount_ytd, 0) AS actual_ytd
							    FROM 
							        (
							            SELECT 
							                company_id,
							                p.type,
											p.site,
											year,
							                scenario,
							                month,
							                SUM(amount) AS amount_periodic
							            FROM 
							                XFC_INV_FACT_project_cash pc
										INNER JOIN
											XFC_INV_MASTER_project p
										ON
											pc.project_id = p.project
							            WHERE scenario = ''Actual''
										GROUP BY
							            company_id,
										p.type,
										p.site,
							            year, 
							            scenario, 
							            month
							        ) p
							    FULL OUTER JOIN (
							        SELECT 
							            company_id,
										p.type,
										p.site,
							            year, 
							            scenario, 
							            CONVERT(INT, REPLACE(cash_month, ''cash_m'', '''')) AS month,
							            SUM(amount_ytd) AS amount_ytd
							        FROM 
							            XFC_INV_VIEW_project_cash_ytd
							        UNPIVOT (
							            amount_ytd FOR cash_month IN (cash_m1, cash_m2, cash_m3, cash_m4, cash_m5, cash_m6, 
							                                          cash_m7, cash_m8, cash_m9, cash_m10, cash_m11, cash_m12)
							        ) AS pc
									INNER JOIN
										XFC_INV_MASTER_project p
									ON
										pc.project_id = p.project
						            WHERE scenario = ''Actual''
									GROUP BY
							            company_id,
										p.type,
										p.site,
							            year, 
							            scenario, 
							            CONVERT(INT, REPLACE(cash_month, ''cash_m'', ''''))
							    ) y
							    ON 
							        p.company_id = y.company_id AND
									p.type = y.type AND
									p.site = y.site AND
							        p.year = y.year AND 
							        p.month = y.month AND 
							        p.scenario = y.scenario
							),
							ScenarioData AS (
							     SELECT 
							        COALESCE(p.company_id, y.company_id) AS company_id,
									COALESCE(p.type, y.type) AS type,
									COALESCE(p.site, y.site) AS site,
							        COALESCE(p.month, y.month) AS month,
							        COALESCE(p.year, y.year) AS year,
							        COALESCE(p.scenario, y.scenario) AS scenario,
									COALESCE(p.amount_periodic, 0) AS scenario_periodic,
							        COALESCE(y.amount_ytd, 0) AS scenario_ytd
							    FROM 
							        (
							            SELECT 
							                company_id,
											p.type,
											p.site,
							                year,
							                scenario,
							                month,
							                SUM(amount) AS amount_periodic
							            FROM 
							                XFC_INV_FACT_project_cash pc
										INNER JOIN
											XFC_INV_MASTER_project p
										ON
											pc.project_id = p.project
							            WHERE scenario != ''Actual''
										GROUP BY
							            company_id,
										p.type,
										p.site,
							            year, 
							            scenario, 
							            month
							        ) p
							    FULL OUTER JOIN (
							        SELECT 
							            company_id,
										p.type,
										p.site,
							            year, 
							            scenario, 
							            CONVERT(INT, REPLACE(cash_month, ''cash_m'', '''')) AS month,
							            SUM(amount_ytd) AS amount_ytd
							        FROM 
							            XFC_INV_VIEW_project_cash_ytd
							        UNPIVOT (
							            amount_ytd FOR cash_month IN (cash_m1, cash_m2, cash_m3, cash_m4, cash_m5, cash_m6, 
							                                          cash_m7, cash_m8, cash_m9, cash_m10, cash_m11, cash_m12)
							        ) AS pc
									INNER JOIN
										XFC_INV_MASTER_project p
									ON
										pc.project_id = p.project
						            WHERE scenario != ''Actual''
									GROUP BY
							            company_id,
										p.type,
										p.site,
							            year, 
							            scenario, 
							            CONVERT(INT, REPLACE(cash_month, ''cash_m'', ''''))
							    ) y
							    ON 
							        p.company_id = y.company_id AND
									p.type = y.type AND
									p.site = y.site AND
							        p.year = y.year AND 
							        p.month = y.month AND 
							        p.scenario = y.scenario
							),
							Scenarios AS (
								SELECT DISTINCT scenario
								FROM XFC_INV_FACT_project_cash
							),
							AllData AS (
								SELECT 
								    COALESCE(ad.company_id, sd.company_id) AS company_id,
									COALESCE(ad.type, sd.type) AS type,
									COALESCE(ad.site, sd.site) AS site,
								    COALESCE(ad.year, sd.year) AS year,
								    COALESCE(ad.month, sd.month) AS month,
								    COALESCE(s.scenario, sd.scenario) AS scenario,
								    COALESCE(ad.actual_periodic, 0) AS actual_periodic,
								    COALESCE(sd.scenario_periodic, 0) AS scenario_periodic,
									COALESCE(ad.actual_periodic, 0) - COALESCE(sd.scenario_periodic, 0) AS gap_periodic,
									COALESCE(ad.actual_ytd, 0) AS actual_ytd, 
								    COALESCE(sd.scenario_ytd, 0) AS scenario_ytd, 		   
								    COALESCE(ad.actual_ytd, 0) - COALESCE(sd.scenario_ytd, 0) AS gap_ytd
								FROM 
								    ActualData ad
								CROSS JOIN
									Scenarios s
								FULL OUTER JOIN 
								    ScenarioData sd
								ON 
								    ad.company_id = sd.company_id AND
									s.scenario = sd.scenario AND
									ad.type = sd.type AND
									ad.site = sd.site AND
									ad.year = sd.year AND
								    ad.month = sd.month
							),
							AllDataGrouped AS (
								SELECT 
								    COALESCE(company_id, 0) AS company_id,
									COALESCE(type, ''All Types'') AS type,
									COALESCE(site, ''All Sites'') AS site,
								    year,
								    month,
								    scenario,
								    SUM(actual_periodic) AS actual_periodic,
								    SUM(scenario_periodic) AS scenario_periodic,
									SUM(gap_periodic) AS gap_periodic,
									SUM(actual_ytd) AS actual_ytd, 
								    SUM(scenario_ytd) AS scenario_ytd, 		   
								    SUM(gap_ytd) AS gap_ytd
								FROM 
								    AllData
								GROUP BY
									GROUPING SETS (
								        (year, month, scenario, company_id, type, site),
								        (year, month, scenario, company_id, type),
								        (year, month, scenario, company_id, site),
								        (year, month, scenario, type, site),
								        (year, month, scenario, company_id),
								        (year, month, scenario, type),
								        (year, month, scenario, site),
								        (year, month, scenario)
								    )
							)
							SELECT
							    ad.company_id,
								CASE
									WHEN ad.company_id = 0 THEN 0
									ELSE 1
								END AS company_order,
							    COALESCE(c.company_name, ''All Companies'') AS company_name,
								ad.type,
								CASE
									WHEN ad.type = ''All Types'' THEN 0
									ELSE 1
								END AS type_order,
								ad.site,
								CASE
									WHEN ad.site = ''All Sites'' THEN 0
									ELSE 1
								END AS site_order,
							    ad.year,
							    ad.month,
							    ad.scenario,
							    ad.actual_periodic,
							    ad.scenario_periodic,
								ad.gap_periodic,
								ad.actual_ytd, 
							    ad.scenario_ytd, 		   
							    ad.gap_ytd, 
							    gc.comment_Actual,
								gc.comment_Planning
							FROM 
							    AllDataGrouped ad
							LEFT JOIN (
								SELECT DISTINCT company_id, company_name
								FROM XFC_INV_MASTER_project
							) c
							ON
								ad.company_id = c.company_id
							LEFT JOIN 
							    XFC_INV_AUX_comment_gap gc
							ON 
							    ad.company_id = gc.company_id AND 
							    ad.year = gc.year AND 
							    ad.month = gc.month AND 
							    ad.scenario = gc.scenario
								AND ad.type = gc.type
							')
						END;
					"


				'Down
				Dim downQuery As String = $"
					DROP VIEW IF EXISTS {viewName};
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Get Population Query"

        Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					
				"
				
				'Down
				Dim downQuery As String = $"
					
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace