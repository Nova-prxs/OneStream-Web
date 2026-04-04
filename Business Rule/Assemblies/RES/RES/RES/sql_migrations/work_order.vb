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
	Public Class work_order
		
		'Declare table name
		Dim tableName As String = "XFC_RES_MASTER_work_order"
		
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
                        entity VARCHAR(255),
                        id VARCHAR(255),
                        description VARCHAR(255),
                        project_id VARCHAR(255),
                        project_description VARCHAR(255),
                        technology VARCHAR(255),
                        client VARCHAR(255),
                        rev_type VARCHAR(255),
                        serv_type VARCHAR(255),
                        phase VARCHAR(255),
                        start_date DATE,
                        break_clause_date_1 DATE,
                        break_clause_date_2 DATE,
                        break_clause_date_3 DATE,
                        end_date DATE,
                        mwh DECIMAL(18, 2),
                        as_sold_contract_value DECIMAL(18, 2),
                        as_sold_direct_cost DECIMAL(18, 2),
                        contract_backlog_value DECIMAL(18, 2),
                        comment VARCHAR(MAX),
						CONSTRAINT PK_{tableName} PRIMARY KEY (entity, id)
                    );
				
					CREATE INDEX idx_entity ON {tableName}(entity);
					CREATE INDEX idx_client ON {tableName}(client);
					CREATE INDEX idx_break_clause_date_1 ON {tableName}(break_clause_date_1);
					CREATE INDEX idx_end_date ON {tableName}(end_date);
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
	                        entity,
	                        id,
	                        description,
							project_id,
							project_description,
	                        technology,
	                        client,
							rev_type,
							serv_type,
	                        phase,
							CASE 
								WHEN start_date = '190001' THEN NULL
								ELSE DATEFROMPARTS(LEFT(start_date, 4), RIGHT(start_date, 2), 1)
							END AS start_date,
							CASE 
								WHEN break_clause_date_1 = '190001' THEN NULL
								ELSE DATEFROMPARTS(LEFT(break_clause_date_1, 4), RIGHT(break_clause_date_1, 2), 1)
							END AS break_clause_date_1,
							CASE 
								WHEN break_clause_date_2 = '190001' THEN NULL
								ELSE DATEFROMPARTS(LEFT(break_clause_date_2, 4), RIGHT(break_clause_date_2, 2), 1)
							END AS break_clause_date_2,
							CASE 
								WHEN break_clause_date_3 = '190001' THEN NULL
								ELSE DATEFROMPARTS(LEFT(break_clause_date_3, 4), RIGHT(break_clause_date_3, 2), 1)
							END AS break_clause_date_3,
							CASE 
								WHEN end_date = '190001' THEN NULL
								ELSE DATEFROMPARTS(LEFT(end_date, 4), RIGHT(end_date, 2), 1)
							END AS end_date,
	                        mwh,
	                        as_sold_contract_value,
	                        as_sold_direct_cost,
	                        contract_backlog_value,
							comment
						FROM XFC_RES_RAW_work_order
					) AS source
					ON target.entity = source.entity
						AND target.id = source.id
					WHEN MATCHED THEN
						UPDATE SET
							description = source.description,
							project_id = source.project_id,
							project_description = source.project_description,
							technology = source.technology,
							client = source.client,
							rev_type = source.rev_type,
							serv_type = source.serv_type,
	                        phase = source.phase,
	                        start_date = source.start_date,
	                        break_clause_date_1 = source.break_clause_date_1,
	                        break_clause_date_2 = source.break_clause_date_2,
	                        break_clause_date_3 = source.break_clause_date_3,
	                        end_date = source.end_date,
	                        mwh = source.mwh,
	                        as_sold_contract_value = source.as_sold_contract_value,
	                        as_sold_direct_cost = source.as_sold_direct_cost,
	                        contract_backlog_value = source.contract_backlog_value,
	                        comment = source.comment
					WHEN NOT MATCHED THEN
						INSERT (
							entity,
	                        id,
	                        description,
							project_id,
							project_description,
	                        technology,
	                        client,
							rev_type,
							serv_type,
	                        phase,
	                        start_date,
	                        break_clause_date_1,
	                        break_clause_date_2,
	                        break_clause_date_3,
	                        end_date,
	                        mwh,
	                        as_sold_contract_value,
	                        as_sold_direct_cost,
	                        contract_backlog_value,
							comment
						)
						VALUES (
							source.entity,
	                        source.id,
	                        source.description,
							source.project_id,
							source.project_description,
	                        source.technology,
	                        source.client,
							source.rev_type,
							source.serv_type,
	                        source.phase,
	                        source.start_date,
	                        source.break_clause_date_1,
	                        source.break_clause_date_2,
	                        source.break_clause_date_3,
	                        source.end_date,
	                        source.mwh,
	                        source.as_sold_contract_value,
	                        source.as_sold_direct_cost,
	                        source.contract_backlog_value,
	                        source.comment
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
