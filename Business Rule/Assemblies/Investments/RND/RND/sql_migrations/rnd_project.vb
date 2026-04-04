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
	Public Class rnd_project
		
		'Declare table name
		Dim tableName As String = "XFC_RND_MASTER_project"
		
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
					    project VARCHAR(50),
					    project_name VARCHAR(255),
						product_family VARCHAR(255),
					    responsible_dir VARCHAR(100),
						type VARCHAR(55),
					    job VARCHAR(100),
					    activity VARCHAR(100),
					    business_segment VARCHAR(100),
					    pre_contract_date DATE,
					    ma_date DATE,
					    ip_owner VARCHAR(55),
					    client VARCHAR(50),
					    plant VARCHAR(50),
						magnitude_product_code VARCHAR(50),
					    status VARCHAR(50),
					    info_provider VARCHAR(50),
						is_real BIT,
						CONSTRAINT PK_{tableName} PRIMARY KEY (
							project,
							responsible_dir,
							job,
					    	activity,
					    	product_family,
				            info_provider,
							status
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
					    SELECT DISTINCT
						    project,
						    project_name,
							product_family,
						    responsible_dir,
						    CASE
								WHEN LOWER(responsible_dir) = 'h_pe' AND LOWER(job) <> 'h-pe proto' THEN 'Process'
								ELSE 'Product'
							END AS type,
						    job,
						    activity,
						    business_segment,
							info_provider,
						    pre_contract_date,
						    ma_date,
						    ip_owner,
						    client,
						    plant,
							magnitude_product_code,
							status,
							1 AS is_real
					    FROM XFC_RND_RAW_project
					) AS source
					ON target.project = source.project
						AND target.product_family = source.product_family
						AND target.responsible_dir = source.responsible_dir
						AND target.job = source.job
						AND target.activity = source.activity
				        AND target.info_provider = source.info_provider
				        AND target.status = source.status
					WHEN MATCHED THEN
					    UPDATE SET
					        project_name = source.project_name,
						    business_segment = source.business_segment,
						    type = source.type,
						    pre_contract_date = source.pre_contract_date,
						    ma_date = source.ma_date,
						    ip_owner = source.ip_owner,
						    client = source.client,
						    plant = source.plant,
							magnitude_product_code = source.magnitude_product_code,
							info_provider = source.info_provider,
							is_real = source.is_real
					WHEN NOT MATCHED THEN
					    INSERT (
						    project,
						    project_name,
							product_family,
						    business_segment,
						    responsible_dir,
						    type,
						    job,
						    activity,
						    pre_contract_date,
						    ma_date,
						    ip_owner,
						    client,
						    plant,
							magnitude_product_code,
						    status,
							info_provider,
							is_real
					    )
					    VALUES (
					        source.project,
						    source.project_name,
							source.product_family,
						    source.business_segment,
						    source.responsible_dir,
						    source.type,
						    source.job,
						    source.activity,
						    source.pre_contract_date,
						    source.ma_date,
						    source.ip_owner,
						    source.client,
						    source.plant,
							source.magnitude_product_code,
						    source.status,
							source.info_provider,
							source.is_real
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
