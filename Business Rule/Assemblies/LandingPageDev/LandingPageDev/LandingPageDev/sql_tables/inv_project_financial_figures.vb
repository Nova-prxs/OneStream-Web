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
	Public Class inv_project_financial_figures
		
		'Declare table name
		Dim tableName As String = "XFC_INV_FACT_project_financial_figures"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					IF OBJECT_ID(N'{tableName}', N'U') IS NULL
					BEGIN
						CREATE TABLE {tableName} (
							project_id VARCHAR(255) NOT NULL,
							scenario VARCHAR(255) NOT NULL,
							year INTEGER NOT NULL,
							month INTEGER NOT NULL,
							type VARCHAR(255) NOT NULL,
							amount_ytd DEC(18,2),
							CONSTRAINT PK_{tableName} PRIMARY KEY (project_id, scenario, year, month, type)
						)
					END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE IF EXISTS {tableName};
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
					MERGE INTO {tableName} AS target
					USING (
						SELECT 
							project,
							'Actual' AS scenario,
							CASE
								WHEN LOWER(type) LIKE '%_cy' THEN
									YEAR(
									    DATEADD(
									        MONTH,
									        -1,
									        DATEADD(
									            DAY,
									            -1,
									            date_of_data_extraction
									        )
									    )
									)
								ELSE
									YEAR(date_of_data_extraction)
									+ CONVERT(INT, RIGHT(type, 1))
							END AS year,
							CASE
								WHEN LOWER(type) LIKE '%_cy' THEN
									MONTH(
									    DATEADD(
									        MONTH,
									        -1,
									        DATEADD(
									            DAY,
									            -1,
									            date_of_data_extraction
									        )
									    )
									)
								ELSE 12
							END AS month,
							CASE
						        WHEN LOWER(type) LIKE '%requirement%' THEN 'Requirement'
						        WHEN LOWER(type) LIKE '%decided%' THEN 'Decided'
						        WHEN LOWER(type) LIKE '%ordered%' THEN 'Ordered'
						        WHEN LOWER(type) LIKE '%delivered%' THEN 'Delivered'
						        ELSE type
					       	END AS type,
					       	amount_ytd
						FROM 
						(
							SELECT 
								project,
								requirement_year_cy, decided_year_cy, ordered_year_cy, delivered_ytd_year_cy,
								requirement_year_ny_1, decided_year_ny_1, ordered_year_ny_1,
					    		requirement_year_ny_2, decided_year_ny_2, ordered_year_ny_2,
							    requirement_year_ny_3, decided_year_ny_3,
					    		requirement_year_ny_4, decided_year_ny_4,
								date_of_data_extraction
						    FROM XFC_INV_RAW_project WITH(NOLOCK)
						) AS SourceTable
						UNPIVOT
						(
						    amount_ytd FOR type IN (
								requirement_year_cy, decided_year_cy, ordered_year_cy, delivered_ytd_year_cy,
								requirement_year_ny_1, decided_year_ny_1, ordered_year_ny_1,
					    		requirement_year_ny_2, decided_year_ny_2, ordered_year_ny_2,
							    requirement_year_ny_3, decided_year_ny_3,
					    		requirement_year_ny_4, decided_year_ny_4
							)
						) AS UnpivotedTable
					) AS source
					ON target.project_id = source.project
					AND target.scenario = source.scenario
					AND target.year = source.year
					AND target.month = source.month
					AND target.type = source.type
					WHEN MATCHED THEN
					    UPDATE SET
					        amount_ytd = source.amount_ytd
					WHEN NOT MATCHED THEN
					    INSERT (
					        project_id, scenario, year, month, type, amount_ytd
					    )
					    VALUES (
					        source.project, source.scenario, source.year, source.month, source.type, source.amount_ytd
					    );
				
					MERGE INTO {tableName} AS target
					USING (
						SELECT 
							project,
							'Actual' AS scenario,
							YEAR(
							    DATEADD(
							        MONTH,
							        -1,
							        DATEADD(
							            DAY,
							            -1,
							            date_of_data_extraction
							        )
							    )
							) AS year,
							12 AS month,
							CASE
						        WHEN LOWER(type) LIKE '%requirement%' THEN 'Requirement'
						        WHEN LOWER(type) LIKE '%decided%' THEN 'Decided'
						        WHEN LOWER(type) LIKE '%ordered%' THEN 'Ordered'
						        WHEN LOWER(type) LIKE '%delivered%' THEN 'Delivered'
						        ELSE type
					       	END AS type,
					       	amount_ytd
						FROM 
						(
							SELECT 
								project,
								requirement_year_cy, decided_year_cy, ordered_year_cy, delivered_ytd_year_cy,
								date_of_data_extraction
						    FROM XFC_INV_RAW_project WITH(NOLOCK)
						) AS SourceTable
						UNPIVOT
						(
						    amount_ytd FOR type IN (
								requirement_year_cy, decided_year_cy, ordered_year_cy, delivered_ytd_year_cy
							)
						) AS UnpivotedTable
					) AS source
					ON target.project_id = source.project
					AND target.scenario = source.scenario
					AND target.year = source.year
					AND target.month = source.month
					AND target.type = source.type
					WHEN MATCHED THEN
					    UPDATE SET
					        amount_ytd = source.amount_ytd
					WHEN NOT MATCHED THEN
					    INSERT (
					        project_id, scenario, year, month, type, amount_ytd
					    )
					    VALUES (
					        source.project, source.scenario, source.year, source.month, source.type, source.amount_ytd
					    );
				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {tableName};
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
