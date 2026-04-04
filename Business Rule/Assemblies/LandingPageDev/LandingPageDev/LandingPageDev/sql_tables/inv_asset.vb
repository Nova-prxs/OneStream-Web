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
	Public Class inv_asset
		
		'Declare table name
		Dim tableName As String = "XFC_INV_MASTER_asset"

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
							id VARCHAR(255) NOT NULL PRIMARY KEY,
							is_real BIT NOT NULL,
							cost_center_id VARCHAR(255) NOT NULL,
							project_id VARCHAR(255) NOT NULL,
							designation VARCHAR(255) NOT NULL,
							category_id VARCHAR(255) NOT NULL,
							activation_date DATE,
							initial_value DECIMAL(18,2) NOT NULL
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
							a.id,
							1 AS is_real,
							a.cost_center_id,
							a.project_id,
							a.designation,
							a.category_id,
							a.activation_date,
							a.initial_value
						FROM XFC_INV_RAW_asset a
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        cost_center_id = source.cost_center_id,
							is_real = source.is_real,
							project_id = source.project_id,
							designation = source.designation,
							category_id = source.category_id,
							activation_date = source.activation_date,
							initial_value = CASE
								WHEN source.initial_value IS NOT NULL OR source.initial_value <> '' THEN source.initial_value
								ELSE target.initial_value
							END
					WHEN NOT MATCHED THEN
					    INSERT (
					        id, is_real, cost_center_id, project_id, designation, category_id, activation_date, initial_value
					    )
					    VALUES (
					        source.id, source.is_real, source.cost_center_id, source.project_id, source.designation, source.category_id, source.activation_date, source.initial_value
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
