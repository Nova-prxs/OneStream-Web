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
	Public Class rnd_project_cash_out
		
		'Declare table name
		Dim tableName As String = "XFC_RND_FACT_project_cash_out"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
					    project_id VARCHAR(50),
						project_responsible_dir VARCHAR(100),
						project_job VARCHAR(100),
					    project_activity VARCHAR(100),
					    project_product_family VARCHAR(255),
						project_info_provider VARCHAR(255),
						project_status VARCHAR(50),
					    company_id INT,
						year INT,
						month INT,
						scenario VARCHAR(50),
						amount DEC(18, 2),
						CONSTRAINT PK_{tableName} PRIMARY KEY (
							project_id,
							project_responsible_dir,
							project_job,
							project_activity,
							project_product_family,
							project_info_provider,
							project_status,
						    company_id,
							year,
							month,
							scenario
						)
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
							project_id,
							project_responsible_dir,
							project_job,
							project_activity,
							project_product_family,
							project_info_provider,
							project_status,
						    company_id,
				 			scenario,
				 			year,
				            CONVERT(INT, REPLACE(month, 'm', '')) AS month,
				            amount * 1000 AS amount
				 		FROM (
				 			SELECT
								pc.project_id,
								pc.project_responsible_dir,
								pc.project_job,
								pc.project_activity,
								pc.project_product_family,
								pc.project_info_provider,
								pc.project_status,
							    c.id AS company_id,
								pc.scenario,
								pc.year,
								SUM(pc.m1) AS m1,
								SUM(pc.m2) AS m2,
								SUM(pc.m3) AS m3,
								SUM(pc.m4) AS m4,
								SUM(pc.m5) AS m5,
								SUM(pc.m6) AS m6,
								SUM(pc.m7) AS m7,
								SUM(pc.m8) AS m8,
								SUM(pc.m9) AS m9,
								SUM(pc.m10) AS m10,
								SUM(pc.m11) AS m11,
								SUM(pc.m12) AS m12
							FROM XFC_RND_RAW_project_cash_out pc
							INNER JOIN XFC_INV_MASTER_company c ON pc.company_name = c.name
					 		GROUP BY
								pc.project_id,
								pc.project_responsible_dir,
								pc.project_job,
								pc.project_activity,
								pc.project_product_family,
								pc.project_info_provider,
								pc.project_status,
							    c.id,
								pc.scenario,
								pc.year
				 		) AS subquery
				 		UNPIVOT (
				            amount FOR month IN (
				 				m1, m2, m3, m4, m5, m6, 
				            	m7, m8, m9, m10, m11, m12
				 			)
				        ) AS pc
					) AS source
					ON target.project_id = source.project_id
					AND target.project_responsible_dir = source.project_responsible_dir
					AND target.project_job = source.project_job
					AND target.project_activity = source.project_activity
					AND target.project_product_family = source.project_product_family
					AND target.project_info_provider = source.project_info_provider
					AND target.project_status = source.project_status
					AND target.company_id = source.company_id
					AND target.scenario = source.scenario
					AND target.year = source.year
					AND target.month = source.month
					WHEN MATCHED THEN
					    UPDATE SET
					        amount = source.amount
					WHEN NOT MATCHED THEN
					    INSERT (
							project_id,
							project_responsible_dir,
							project_job,
							project_activity,
							project_product_family,
							project_info_provider,
							project_status,
						    company_id,
				 			scenario,
				 			year,
				            month,
				            amount
					    )
					    VALUES (
							source.project_id,
							source.project_responsible_dir,
							source.project_job,
							source.project_activity,
							source.project_product_family,
							source.project_info_provider,
							source.project_status,
						    source.company_id,
				 			source.scenario,
				 			source.year,
				            source.month,
				            source.amount
					    );
				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {tableName}
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
