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
	Public Class inv_asset_depreciation
		
		'Declare table name
		Dim tableName As String = "XFC_INV_FACT_asset_depreciation"

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
							asset_id VARCHAR(255) NOT NULL,
							project_id VARCHAR(255) NOT NULL,
							scenario VARCHAR(255) NOT NULL,
							year INTEGER NOT NULL,
							month INTEGER NOT NULL,
							type VARCHAR(255) NOT NULL,
							amount DECIMAL(18, 2) NOT NULL,
							CONSTRAINT PK_{tableName} PRIMARY KEY (asset_id, project_id, scenario, year, month, type)
						);
				
						CREATE INDEX idx_year_scenario ON XFC_INV_FACT_asset_depreciation (year, scenario);
						CREATE INDEX idx_project_id_asset_id_type ON XFC_INV_FACT_asset_depreciation (project_id, asset_id, type);
						CREATE INDEX idx_project_id_asset_id_year_scenario ON XFC_INV_FACT_asset_depreciation (project_id, asset_id, year, scenario);
						CREATE INDEX idx_asset_id_project_id ON XFC_INV_FACT_asset_depreciation (asset_id, project_id);
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
		
'		Changes
'		DECLARE @constraintName sysname;
'		Declare @sql nvarchar(200);
		
'		-- Asignar el nombre de la restricción a la variable
'		Select @constraintName = name 
'		From sys.key_constraints 
'		Where type = 'PK' AND parent_object_id = OBJECT_ID('XFC_INV_FACT_asset_depreciation');
		
'		-- Construir la cadena de comando dinámico
'		Set @sql = 'ALTER TABLE XFC_INV_FACT_asset_depreciation DROP CONSTRAINT ' + QUOTENAME(@constraintName);
		
'		-- Ejecutar la cadena de comando
'		EXECUTE sp_executesql @sql;

'		ALTER TABLE XFC_INV_FACT_asset_depreciation
'		ADD project_id VARCHAR(255);

'---------------------------------------------------------------------------------------

'		UPDATE ad
'		Set project_id = a.project_id
'		From XFC_INV_FACT_asset_depreciation ad
'		INNER Join XFC_INV_MASTER_asset a
'		On ad.asset_id = a.id;

'---------------------------------------------------------------------------------------

'		DELETE FROM XFC_INV_FACT_asset_depreciation
'		WHERE project_id IS NULL;

'---------------------------------------------------------------------------------------

'		ALTER TABLE XFC_INV_FACT_asset_depreciation
'		ALTER COLUMN project_id VARCHAR(255) NOT NULL;

'---------------------------------------------------------------------------------------

'		ALTER TABLE XFC_INV_FACT_asset_depreciation
'		ADD CONSTRAINT PK_XFC_INV_FACT_asset_depreciation
'		PRIMARY KEY (asset_id, project_id, scenario, year, month, type);

'---------------------------------------------------------------------------------------
'		CREATE INDEX idx_year_scenario ON XFC_INV_FACT_asset_depreciation (year, scenario);
'		CREATE INDEX idx_asset_id_project_id ON XFC_INV_FACT_asset_depreciation (asset_id, project_id);
'		CREATE INDEX idx_project_id_asset_id_type On XFC_INV_FACT_asset_depreciation (project_id, asset_id, type);
'		CREATE INDEX idx_project_id_asset_id_year_scenario On XFC_INV_FACT_asset_depreciation (project_id, asset_id, year, scenario);

	End Class
End Namespace
