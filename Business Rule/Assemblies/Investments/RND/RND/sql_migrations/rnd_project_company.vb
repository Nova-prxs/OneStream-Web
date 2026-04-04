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
	Public Class rnd_project_company
		
		'Declare table name
		Dim tableName As String = "XFC_RND_MASTER_project_company"
		
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
					    company_id INT
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
							rp.project AS project_id,
							rp.responsible_dir AS project_responsible_dir,
							rp.job AS project_job,
							rp.activity AS project_activity,
							rp.product_family AS project_product_family,
							rp.info_provider AS project_info_provider,
							c.id AS company_id
						FROM (
							SELECT DISTINCT company_name, project, responsible_dir, job, activity, product_family, info_provider
							FROM XFC_RND_RAW_project
						) rp
						INNER JOIN XFC_INV_MASTER_company c
						ON rp.company_name = c.name
					) AS source
					ON target.project_id = source.project_id
						AND target.project_responsible_dir = source.project_responsible_dir
						AND target.project_job = source.project_job
						AND target.project_activity = source.project_activity
						AND target.project_product_family = source.project_product_family
						AND target.company_id = source.company_id
						AND target.project_info_provider = source.project_info_provider
					WHEN NOT MATCHED THEN
						INSERT (
							project_id,
							project_responsible_dir,
							project_job,
							project_activity,
							project_product_family,
							company_id,
							project_info_provider
						)
						VALUES (
							source.project_id,
							source.project_responsible_dir,
							source.project_job,
							source.project_activity,
							source.project_product_family,
							source.company_id,
							source.project_info_provider
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
