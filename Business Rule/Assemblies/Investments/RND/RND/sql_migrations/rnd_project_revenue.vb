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
	Public Class rnd_project_revenue
		
		'Declare table name
		Dim tableName As String = "XFC_RND_FACT_project_revenue"
		
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
					    client VARCHAR(50),
						account VARCHAR(50),
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
							client,
							account,
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
				            company_id,
							project_info_provider,
							project_status,
				            client,
							account,
				            year, 
				            scenario, 
				            CONVERT(INT, REPLACE(expense_month, 'm', '')) AS month,
				            SUM(amount * 1000) AS amount
						FROM (
				            SELECT
								rp.project AS project_id,
								rp.responsible_dir AS project_responsible_dir,
								rp.job AS project_job,
								rp.activity AS project_activity,
								rp.product_family AS project_product_family,
								c.id AS company_id,
				            	rp.client,
								rp.info_provider AS project_info_provider,
								rp.status AS project_status,
								rp.account,
								rp.year,
								rp.scenario,
								m1,
								m2,
								m3,
								m4,
								m5,
								m6,
								m7,
								m8,
								m9,
								m10,
								m11,
								m12
							FROM XFC_RND_RAW_project_revenue rp
							INNER JOIN XFC_INV_MASTER_company c ON rp.company_name = c.name
						) AS subquery
				        UNPIVOT (
				            amount FOR expense_month IN (m1, m2, m3, m4, m5, m6, 
				                                          m7, m8, m9, m10, m11, m12)
				        ) AS pc
						GROUP BY
							project_id,
							project_responsible_dir,
							project_job,
							project_activity,
							project_product_family,
				            company_id,
							project_info_provider,
							project_status,
				            client,
							account,
				            year, 
				            scenario, 
				            CONVERT(INT, REPLACE(expense_month, 'm', ''))
					) AS source
					ON target.project_id = source.project_id
						AND target.project_responsible_dir = source.project_responsible_dir
						AND target.project_job = source.project_job
						AND target.project_activity = source.project_activity
						AND target.project_product_family = source.project_product_family
						AND target.project_info_provider = source.project_info_provider
						AND target.project_status = source.project_status
			            AND target.company_id = source.company_id
			            AND target.client = source.client
			            AND target.account = source.account
			            AND target.year = source.year
			            AND target.scenario = source.scenario
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
							client,
							account,
				            year, 
				            scenario, 
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
							source.client,
							source.account,
				            source.year, 
				            source.scenario, 
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
