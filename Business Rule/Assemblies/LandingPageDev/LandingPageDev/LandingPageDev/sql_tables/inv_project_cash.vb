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
	Public Class inv_project_cash
		
		'Declare table name
		Dim tableName As String = "XFC_INV_FACT_project_cash"

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
							amount DEC(18,2),
							CONSTRAINT PK_{tableName} PRIMARY KEY (project_id, scenario, year, month)
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
							'Actual' AS scenario,
							year,
							month,
							amount
						FROM XFC_INV_RAW_project_cash
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
