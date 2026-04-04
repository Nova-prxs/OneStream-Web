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
	Public Class inv_view_project_cash

        'Declare view and table name
		Dim viewName As String = "XFC_INV_VIEW_project_cash"
		Dim tableName As String = "XFC_INV_FACT_project_cash"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					IF OBJECT_ID(N'{viewName}', N'V') IS NULL
						BEGIN
							EXEC('CREATE VIEW {viewName} AS
								SELECT
							        pma.*,
									CASE
										WHEN mp.project_id IS NOT NULL THEN 1
										ELSE 0
									END AS is_modified,
							        p.project_name AS project_name,
							        p.company_name AS company_name,
									p.company_id AS company_id,
							        p.type AS type,
							        p.aggregate AS aggregate,
							        p.budget_owner AS budget_owner,
									p.ma_date AS ma_date,
									p.start_date AS start_date,
									p.end_date AS end_date,
									p.is_real AS is_real,
									p.poi_poe,
									(
										COALESCE(pma.cash_m1, 0)
										+ COALESCE(pma.cash_m2, 0)
										+ COALESCE(pma.cash_m3, 0)
										+ COALESCE(pma.cash_m4, 0)
										+ COALESCE(pma.cash_m5, 0)
										+ COALESCE(pma.cash_m6, 0)
										+ COALESCE(pma.cash_m7, 0)
										+ COALESCE(pma.cash_m8, 0)
										+ COALESCE(pma.cash_m9, 0)
										+ COALESCE(pma.cash_m10, 0)
										+ COALESCE(pma.cash_m11, 0)
										+ COALESCE(pma.cash_m12, 0)
									) AS cash_year,
							        CASE
							            WHEN LOWER(pma.scenario) LIKE ''%budget%'' THEN
							                COALESCE(
							                    (
													SELECT SUM(amount)
													FROM XFC_INV_FACT_project_cash
													WHERE
														project_id = pma.project_id
														AND (
															year = pma.year - 1
															AND scenario = COALESCE(rfy.scenario, ''Actual'')
														) OR (
															year < pma.year - 1
															AND scenario = ''Actual''
														)
												),
							                    0
											)
							            ELSE
							                COALESCE(
							                    (
													SELECT SUM(amount)
													FROM XFC_INV_FACT_project_cash
													WHERE
														project_id = pma.project_id
														AND year < pma.year
														AND scenario = ''Actual''
												),
							                    0
							                )
							        END AS cash_before,
									p.total_requirement,
									COALESCE(
										(
											SELECT TOP 1 amount_ytd
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year = pma.year
												AND month < pma.forecast_month
												AND type = ''Requirement''
												AND scenario = ''Actual''
											ORDER BY month DESC
										),
										0
									) AS requirement_ytd,
									COALESCE(
										(
											SELECT TOP 1 amount_ytd
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year = pma.year
												AND month < pma.forecast_month
												AND type = ''Decided''
												AND scenario = ''Actual''
											ORDER BY month DESC
										),
										0
									) AS decided_ytd,
									COALESCE(
										(
											SELECT TOP 1 amount_ytd
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year = pma.year
												AND month < pma.forecast_month
												AND type = ''Ordered''
												AND scenario = ''Actual''
											ORDER BY month DESC
										),
										0
									) AS ordered_ytd,
									COALESCE(
										(
											SELECT TOP 1 amount_ytd
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year = pma.year
												AND month < pma.forecast_month
												AND type = ''Delivered''
												AND scenario = ''Actual''
											ORDER BY month DESC
										),
										0
									) AS delivered_ytd,
									COALESCE(
										(
											SELECT SUM(amount_ytd)
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year < pma.year
												AND month = 12
												AND type = ''Requirement''
												AND scenario = ''Actual''
										),
										0
									) AS requirement_before,
									COALESCE(
										(
											SELECT SUM(amount_ytd)
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year < pma.year
												AND month = 12
												AND type = ''Decided''
												AND scenario = ''Actual''
										),
										0
									) AS decided_before,
									COALESCE(
										(
											SELECT SUM(amount_ytd)
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year < pma.year
												AND month = 12
												AND type = ''Ordered''
												AND scenario = ''Actual''
										),
										0
									) AS ordered_before,
									COALESCE(
										(
											SELECT SUM(amount_ytd)
											FROM XFC_INV_FACT_project_financial_figures
											WHERE project_id = pma.project_id
												AND year < pma.year
												AND month = 12
												AND type = ''Delivered''
												AND scenario = ''Actual''
										),
										0
									) AS delivered_before
							    FROM (
							        SELECT 
							            project_id,
							            scenario,
										CASE
											WHEN scenario LIKE ''%RF%'' THEN CAST(REPLACE(LEFT(scenario, 4), ''RF'', '''') AS INTEGER)
											WHEN scenario = ''Actual'' THEN 13
											ELSE 0
										END AS forecast_month,
							            year,
							            COALESCE([1], 0) AS cash_m1,
							            COALESCE([2], 0) AS cash_m2,
							            COALESCE([3], 0) AS cash_m3,
							            COALESCE([4], 0) AS cash_m4,
							            COALESCE([5], 0) AS cash_m5,
							            COALESCE([6], 0) AS cash_m6,
							            COALESCE([7], 0) AS cash_m7,
							            COALESCE([8], 0) AS cash_m8,
							            COALESCE([9], 0) AS cash_m9,
							            COALESCE([10], 0) AS cash_m10,
							            COALESCE([11], 0) AS cash_m11,
							            COALESCE([12], 0) AS cash_m12
							        FROM 
							            (SELECT project_id, scenario, year, month, amount FROM {tableName}) AS SourceTable
							        PIVOT
							        (
							            SUM(amount)
							            FOR month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
							        ) AS PivotTable
							    ) AS pma
							    LEFT JOIN XFC_INV_MASTER_project p ON pma.project_id = p.project
								LEFT JOIN XFC_INV_AUX_modified_projection mp ON pma.project_id = mp.project_id
									AND pma.scenario = mp.scenario
									AND pma.year = mp.year
									AND mp.type = ''cash''
								LEFT JOIN (
									SELECT project_id, year, scenario
									FROM (
										SELECT 
											project_id,
									        year,
									        scenario,
									        ROW_NUMBER() OVER (
												PARTITION BY project_id, year
												ORDER BY CAST(REPLACE(LEFT(scenario, 4), ''RF'', '''') AS INTEGER) DESC
											) AS rn
									    FROM XFC_INV_FACT_project_cash
									    WHERE scenario LIKE ''RF%''
									) AS subquery
									WHERE rn = 1
								) rfy ON
									pma.project_id = rfy.project_id
									AND pma.year - 1 = rfy.year
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
