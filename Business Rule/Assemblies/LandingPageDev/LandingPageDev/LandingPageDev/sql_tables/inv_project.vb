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
	Public Class inv_project
		
		'Declare table name
		Dim tableName As String = "XFC_INV_MASTER_project"
		
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
					    func VARCHAR(255),
						label_position_1 VARCHAR(255),
					    label_position_2 VARCHAR(255),
					    label_position_3 VARCHAR(255),
					    label_position_4 VARCHAR(255),
					    label_position_5 VARCHAR(255),
					    region VARCHAR(255),
					    company_name VARCHAR(255),
					    company_id VARCHAR(255),
					    type VARCHAR(255),
						country VARCHAR(255),
					    branch VARCHAR(255),
					    site VARCHAR(255),
					    professional_area VARCHAR(255),
					    poi_poe VARCHAR(255),
					    cc VARCHAR(255),
					    cc_name VARCHAR(255),
					    cpi VARCHAR(255),
					    cpi_name VARCHAR(255),
					    project VARCHAR(255) NOT NULL PRIMARY KEY,
					    project_name VARCHAR(255),
					    decision_criteria VARCHAR(255),
					    aggregate VARCHAR(255),
					    libre VARCHAR(255),
					    strategic_axis_name VARCHAR(255),
					    project_status VARCHAR(255),
					    cpil_name VARCHAR(255),
					    program_position VARCHAR(255),
					    dpci_analyst VARCHAR(255),
					    reason VARCHAR(255),
					    start_date DATE,
					    end_date DATE,
						budget_owner VARCHAR(255),
						ma_date DATE,
						is_real BIT,
						total_requirement DECIMAL(18, 2)
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
					        func, 
					        label_position_1, 
					        label_position_2, 
					        label_position_3, 
					        label_position_4, 
					        label_position_5, 
					        type,
							region, 
					        company_name, 
					        company_id, 
					        country, 
					        branch, 
					        site, 
					        professional_area, 
					        poi_poe, 
					        cc, 
					        cc_name, 
					        cpi, 
					        cpi_name, 
					        project, 
					        project_name, 
					        decision_criteria, 
					        aggregate, 
					        libre, 
					        strategic_axis_name, 
					        project_status, 
					        cpil_name, 
					        program_position, 
					        dpci_analyst, 
					        reason,
					        start_date,
					        end_date,
					        budget_owner,
							ma_date,
							total_requirement
					    FROM XFC_INV_RAW_project
					) AS source
					ON target.project = source.project
					WHEN MATCHED THEN
					    UPDATE SET
					        func = source.func,
					        label_position_1 = source.label_position_1,
					        label_position_2 = source.label_position_2,
					        label_position_3 = source.label_position_3,
					        label_position_4 = source.label_position_4,
					        label_position_5 = source.label_position_5,
					        region = source.region,
					        type = CASE
								WHEN source.type IS NOT NULL OR source.type <> '' THEN source.type
								ELSE target.type
							END,
							company_name = source.company_name,
					        company_id = source.company_id,
					        country = source.country,
					        branch = source.branch,
					        site = source.site,
					        professional_area = source.professional_area,
					        poi_poe = source.poi_poe,
					        cc = source.cc,
					        cc_name = source.cc_name,
					        cpi = source.cpi,
					        cpi_name = source.cpi_name,
					        project_name = source.project_name,
							decision_criteria = source.decision_criteria,
					        aggregate = source.aggregate,
					        libre = source.libre,
					        strategic_axis_name = source.strategic_axis_name,
					        project_status = source.project_status,
					        cpil_name = source.cpil_name,
					        program_position = source.program_position,
					        dpci_analyst = source.dpci_analyst,
					        reason = source.reason,
					        start_date = source.start_date,
					        end_date = source.end_date,
					        budget_owner = source.budget_owner,
					        ma_date = CASE
								WHEN source.ma_date IS NOT NULL OR YEAR(source.ma_date) > 2000 THEN source.ma_date
								ELSE target.ma_date
							END,
							is_real = 1,
							total_requirement = source.total_requirement
					WHEN NOT MATCHED THEN
					    INSERT (
					        func, 
					        label_position_1, 
					        label_position_2, 
					        label_position_3, 
					        label_position_4, 
					        label_position_5, 
					        region, 
					        type,
							company_name, 
					        company_id, 
					        country, 
					        branch, 
					        site, 
					        professional_area, 
					        poi_poe, 
					        cc, 
					        cc_name, 
					        cpi, 
					        cpi_name, 
					        project, 
					        project_name, 
					        decision_criteria, 
					        aggregate, 
					        libre, 
					        strategic_axis_name, 
					        project_status, 
					        cpil_name, 
					        program_position, 
					        dpci_analyst,
					        reason,
					        start_date,
					        end_date,
					        budget_owner,
							ma_date,
							is_real,
							total_requirement
					    )
					    VALUES (
					        source.func,
					        source.label_position_1,
					        source.label_position_2,
					        source.label_position_3,
					        source.label_position_4,
					        source.label_position_5,
					        source.region,
					        source.type,
							source.company_name,
					        source.company_id,
					        source.country,
					        source.branch,
					        source.site,
					        source.professional_area,
					        source.poi_poe,
					        source.cc,
					        source.cc_name,
					        source.cpi,
					        source.cpi_name,
					        source.project,
					        source.project_name,
					        source.decision_criteria,
					        source.aggregate,
					        source.libre,
					        source.strategic_axis_name,
					        source.project_status,
					        source.cpil_name,
					        source.program_position,
					        source.dpci_analyst,
					        source.reason,
					        source.start_date,
					        source.end_date,
					        source.budget_owner,
							source.ma_date,
							1,
							source.total_requirement
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
