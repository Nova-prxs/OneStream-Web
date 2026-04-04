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
	Public Class master_treasury_monitoring
		
		'Declare table name
		Dim tableName As String = "XFC_TRS_MASTER_Treasury_Monitoring"
		
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
						[Date] DATE NOT NULL,
						[Year] INT NOT NULL,
						[Scenario] NVARCHAR(255),
						[Entity] NVARCHAR(255),
						[Entity_Id] INT NOT NULL,
						[Timekey] NVARCHAR(255) NOT NULL,
						[week_starting] INT,
						[alert_level_status] INT,
						[alert_level] NVARCHAR(10),
						[issue] NVARCHAR(MAX),
						[analysis] NVARCHAR(MAX),
						[solution] NVARCHAR(MAX),
						PRIMARY KEY ([Entity_Id], [Timekey])
					)
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE {tableName};
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
				-- Validation: Check Entity values in CashDebtPosition - must match the dictionary mapping
				IF EXISTS (
				    SELECT 1 FROM XFC_TRS_RAW_CashDebtPosition 
				    WHERE Entity IS NOT NULL 
				    AND UPPER(LTRIM(RTRIM(Entity))) NOT IN (
				        'HORSE POWERTRAIN SOLUTION',
				        'HORSE SPAIN',
				        'OYAK HORSE',
				        'HORSE BRAZIL',
				        'HORSE CHILE',
				        'HORSE ARGENTINA',
				        'HORSE ROMANIA',
				        'HORSE AVEIRO',
				        'HORSE HPL',
				        'HPL CHINA',
				        'AUROBAY SWEDEN'
				    )
				)
				BEGIN
				    RAISERROR('Error: Invalid Entity value detected in CashDebtPosition table.', 16, 1)
				    RETURN
				END

				-- Validation: Check Entity values in CashFlowForecasting - must match the dictionary mapping
				IF EXISTS (
				    SELECT 1 FROM XFC_TRS_RAW_CashFlowForecasting 
				    WHERE Entity IS NOT NULL 
				    AND UPPER(LTRIM(RTRIM(Entity))) NOT IN (
				        'HORSE POWERTRAIN SOLUTION',
				        'HORSE SPAIN',
				        'OYAK HORSE',
				        'HORSE BRAZIL',
				        'HORSE CHILE',
				        'HORSE ARGENTINA',
				        'HORSE ROMANIA',
				        'HORSE AVEIRO',
				        'HORSE HPL',
				        'HPL CHINA',
				        'AUROBAY SWEDEN'
				    )
				)
				BEGIN
				    RAISERROR('Error: Invalid Entity value detected in CashFlowForecasting table.', 16, 1)
				    RETURN
				END

				-- INSERT approach (only new records, skip existing ones)
				-- Process data from both RAW tables (files are uploaded one at a time)
				-- NOT EXISTS ensures no duplicates when second file is uploaded
				INSERT INTO {tableName} (
				    [Date], 
				    [Year],
				    Scenario, 
				    Entity, 
				    Entity_Id, 
				    Timekey, 
				    week_starting,
				    alert_level_status,
				    alert_level
				)
				SELECT DISTINCT
				    auxDate.weekStartDate AS [Date],
				    CAST(combined.Year AS INT) AS [Year],
				    'FW' + RIGHT('0' + CAST(auxDate.weekNumber AS VARCHAR(2)), 2) AS Scenario,
				    combined.Entity,
				    CASE 
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE POWERTRAIN SOLUTION' THEN 1300
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE SPAIN' THEN 1301
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'OYAK HORSE' THEN 1302
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE BRAZIL' THEN 1303
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE CHILE' THEN 585
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE ARGENTINA' THEN 592
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE ROMANIA' THEN 611
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE AVEIRO' THEN 671
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE HPL' THEN 997
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HPL CHINA' THEN 999
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'AUROBAY SWEDEN' THEN 998
				        ELSE 0
				    END AS Entity_Id,
				    combined.Timekey,
				    auxDate.weekNumber AS week_starting,
				    1 AS alert_level_status,
				    N'●' AS alert_level
				FROM (
				    -- Data from CashDebtPosition (if file was uploaded)
				    SELECT Entity, Year, Timekey 
				    FROM XFC_TRS_RAW_CashDebtPosition 
				    WHERE Entity IS NOT NULL
				    UNION ALL
				    -- Data from CashFlowForecasting (if file was uploaded)
				    SELECT Entity, Year, Timekey 
				    FROM XFC_TRS_RAW_CashFlowForecasting 
				    WHERE Entity IS NOT NULL
				) AS combined
				INNER JOIN XFC_TRS_AUX_Date auxDate 
				    ON CONVERT(VARCHAR(8), auxDate.weekStartDate, 112) = combined.Timekey
				WHERE NOT EXISTS (
				    SELECT 1 
				    FROM {tableName} existing
				    WHERE existing.Entity_Id = CASE 
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE POWERTRAIN SOLUTION' THEN 1300
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE SPAIN' THEN 1301
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'OYAK HORSE' THEN 1302
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE BRAZIL' THEN 1303
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE CHILE' THEN 585
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE ARGENTINA' THEN 592
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE ROMANIA' THEN 611
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE AVEIRO' THEN 671
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HORSE HPL' THEN 997
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'HPL CHINA' THEN 999
				        WHEN UPPER(LTRIM(RTRIM(combined.Entity))) = 'AUROBAY SWEDEN' THEN 998
				        ELSE 0
				    END
				    AND existing.Timekey = combined.Timekey
				);

				-- Log success message
				DECLARE @RowsAffected INT = @@ROWCOUNT;
				PRINT 'TreasuryMonitoring population completed successfully. New rows inserted: ' + CAST(@RowsAffected AS VARCHAR(10));
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
