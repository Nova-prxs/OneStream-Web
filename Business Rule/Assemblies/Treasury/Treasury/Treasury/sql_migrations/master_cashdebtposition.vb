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
	Public Class master_cashdebtposition
		
		'Declare table name
		Dim tableName As String = "XFC_TRS_Master_CashDebtPosition"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				
				'Up - Normalized structure matching RAW table
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
						[RowId] INT IDENTITY(1,1) PRIMARY KEY,
						
					-- Upload/Source Information
					[UploadTimekey] NVARCHAR(8) NOT NULL,
					[UploadYear] INT NOT NULL,
					[UploadWeekNumber] INT NOT NULL,
					
					-- Entity/Account Information
						[Entity_Id] INT NOT NULL,
						[Entity] NVARCHAR(255) NOT NULL,
						[Scenario] NVARCHAR(255) NOT NULL,
						[Account] NVARCHAR(255) NOT NULL,
						[Flow] NVARCHAR(255),
						[Bank] NVARCHAR(255),
						
						-- Projection Information
						[ProjectionType] NVARCHAR(20) NOT NULL,        -- 'StartWeek', 'EndWeek', 'EOM'
						[ProjectionTimekey] NVARCHAR(8) NOT NULL,      -- Timekey of projected week/month
						[ProjectionYear] INT NOT NULL,
						[ProjectionWeekNumber] INT,                     -- Week number for Actual/EndWeek
						[ProjectionMonthNumber] INT,                    -- Month number for EOM
						[ProjectionDate] DATE NOT NULL,
						[WeekOffset] INT NOT NULL,                      -- 0 for Actual, 4 for EndWeek, varies for EOM
						
						-- Value
						[Amount] DECIMAL(18,2),
						
						-- Unique constraint to prevent duplicates
						CONSTRAINT UQ_Master_CashDebt UNIQUE (
							UploadTimekey, Entity_Id, Account, Flow, Bank, 
							ProjectionType, ProjectionTimekey
						)
					);
					
					-- Create indexes for performance
					CREATE INDEX IX_Master_CashDebt_Upload ON {tableName} (UploadTimekey, Entity_Id);
					CREATE INDEX IX_Master_CashDebt_Projection ON {tableName} (ProjectionYear, ProjectionWeekNumber);
					CREATE INDEX IX_Master_CashDebt_Type ON {tableName} (ProjectionType);
					CREATE INDEX IX_Master_CashDebt_Scenario ON {tableName} (Scenario, Entity_Id);
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL
					BEGIN
						DROP TABLE {tableName};
					END
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
				
				Dim upQuery As String = $"
					DECLARE @StartTime DATETIME = GETDATE();
					DECLARE @DeletedRows INT = 0;
					DECLARE @InsertedRows INT = 0;
					DECLARE @ValidationMsg NVARCHAR(MAX);

					IF NOT EXISTS (
					    SELECT 1 FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL
					)
					BEGIN
					    RAISERROR('Error: RAW table is empty or has no valid data (Account IS NULL).', 16, 1);
					    RETURN;
					END

					IF NOT EXISTS (SELECT 1 FROM XFC_TRS_AUX_Date)
					BEGIN
					    RAISERROR('Error: XFC_TRS_AUX_Date table is empty. Please populate date dimension first.', 16, 1);
					    RETURN;
					END
					
					IF EXISTS (
					    SELECT 1 FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL 
					    AND UPPER(LTRIM(RTRIM(Entity))) NOT IN (
					        'HORSE POWERTRAIN SOLUTION', 'HORSE SPAIN', 'OYAK HORSE', 'HORSE BRAZIL',
					        'HORSE CHILE', 'HORSE ARGENTINA', 'HORSE ROMANIA', 'HORSE AVEIRO',
					        'HORSE HPL', 'HPL CHINA', 'AUROBAY SWEDEN'
					    )
					)
					BEGIN
					    SELECT TOP 1 @ValidationMsg = 'Invalid Entity: ' + Entity 
					    FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL 
					    AND UPPER(LTRIM(RTRIM(Entity))) NOT IN (
					        'HORSE POWERTRAIN SOLUTION', 'HORSE SPAIN', 'OYAK HORSE', 'HORSE BRAZIL',
					        'HORSE CHILE', 'HORSE ARGENTINA', 'HORSE ROMANIA', 'HORSE AVEIRO',
					        'HORSE HPL', 'HPL CHINA', 'AUROBAY SWEDEN'
					    );
					    RAISERROR('Error: %s. Please check Entity values in Excel template.', 16, 1, @ValidationMsg);
					    RETURN;
					END

					IF EXISTS (
					    SELECT 1 FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL
					    AND (Entity IS NULL OR LTRIM(RTRIM(Entity)) = '' 
					       OR Year IS NULL OR Timekey IS NULL OR LTRIM(RTRIM(Timekey)) = '')
					)
					BEGIN
					    RAISERROR('Error: Required fields missing. Account, Entity, Year, and Timekey are mandatory.', 16, 1);
					    RETURN;
					END
					
					-- Validate that Debt flows (INTernal/EXTernal Debt) contain only positive values
					IF EXISTS (
					    SELECT 1 FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL
					    AND Flow IN ('INTernal Debt (-)', 'EXTernal Debt (-)', 'INTernal Debt (+)', 'EXTernal Debt (+)')
					    AND (
					        (StartWeek IS NOT NULL AND StartWeek < 0) 
					        OR (EndWeek IS NOT NULL AND EndWeek < 0) 
					        OR (EOM IS NOT NULL AND EOM < 0)
					    )
					)
					BEGIN
					    SELECT TOP 1 @ValidationMsg = 
					        'Invalid negative value for Debt flow: ' + Flow + 
					        ' in Entity: ' + Entity + 
					        ' (Account: ' + Account + ')' +
					        CASE 
					            WHEN StartWeek < 0 THEN ' - StartWeek: ' + CAST(StartWeek AS NVARCHAR)
					            WHEN EndWeek < 0 THEN ' - EndWeek: ' + CAST(EndWeek AS NVARCHAR)
					            WHEN EOM < 0 THEN ' - EOM: ' + CAST(EOM AS NVARCHAR)
					            ELSE ''
					        END
					    FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL
					    AND Flow IN ('INTernal Debt (-)', 'EXTernal Debt (-)', 'INTernal Debt (+)', 'EXTernal Debt (+)')
					    AND (
					        (StartWeek IS NOT NULL AND StartWeek < 0) 
					        OR (EndWeek IS NOT NULL AND EndWeek < 0) 
					        OR (EOM IS NOT NULL AND EOM < 0)
					    );
					    
					    RAISERROR('Error: %s. Debt flows must contain positive values only. Please check your Excel template.', 16, 1, @ValidationMsg);
					    RETURN;
					END
					
					-- Delete only records for the specific entities being uploaded in this batch
					-- This allows multiple entities to coexist for the same week/scenario
					DELETE FROM {tableName} 
					WHERE UploadTimekey IN (
					    SELECT DISTINCT Timekey 
					    FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL
					)
					AND Entity_Id IN (
					    SELECT DISTINCT 
					        CASE UPPER(LTRIM(RTRIM(Entity)))
					            WHEN 'HORSE POWERTRAIN SOLUTION' THEN 1300 
					            WHEN 'HORSE SPAIN' THEN 1301
					            WHEN 'OYAK HORSE' THEN 1302 
					            WHEN 'HORSE BRAZIL' THEN 1303
					            WHEN 'HORSE CHILE' THEN 585 
					            WHEN 'HORSE ARGENTINA' THEN 592
					            WHEN 'HORSE ROMANIA' THEN 611 
					            WHEN 'HORSE AVEIRO' THEN 671
					            WHEN 'HORSE HPL' THEN 997
					            WHEN 'HPL CHINA' THEN 999 
					            WHEN 'AUROBAY SWEDEN' THEN 998 
					            ELSE 0
					        END AS Entity_Id
					    FROM XFC_TRS_RAW_CashDebtPosition 
					    WHERE Account IS NOT NULL
					);
					
					SET @DeletedRows = @@ROWCOUNT;

					IF OBJECT_ID('tempdb..#WeekMapping') IS NOT NULL DROP TABLE #WeekMapping;
					
					CREATE TABLE #WeekMapping (
						UploadTimekey NVARCHAR(8) NOT NULL,
						WeekCol VARCHAR(10) NOT NULL,
						UploadYear INT NOT NULL,
						UploadWeekNumber INT NOT NULL,
						ProjectionType VARCHAR(20) NOT NULL,
						ProjectionYear INT NOT NULL,
						ProjectionWeekNumber INT NOT NULL,
						ProjectionTimekey VARCHAR(8) NOT NULL,
						ProjectionDate DATE NOT NULL,
						INDEX IX_WeekMapping_Clustered CLUSTERED (UploadTimekey, WeekCol)
					);
					
					INSERT INTO #WeekMapping
					SELECT 
						raw.Timekey AS UploadTimekey,
						'StartWeek' AS WeekCol,
						uploadDate.year AS UploadYear,
						uploadDate.weekNumber AS UploadWeekNumber,
						'StartWeek' AS ProjectionType,
						uploadDate.year AS ProjectionYear,
						uploadDate.weekNumber AS ProjectionWeekNumber,
						CONVERT(VARCHAR(8), uploadDate.weekStartDate, 112) AS ProjectionTimekey,
						uploadDate.weekStartDate AS ProjectionDate
					FROM (SELECT DISTINCT Timekey FROM XFC_TRS_RAW_CashDebtPosition WHERE Account IS NOT NULL) raw
					INNER JOIN XFC_TRS_AUX_Date uploadDate
						ON CONVERT(VARCHAR(8), uploadDate.weekStartDate, 112) = raw.Timekey
						AND uploadDate.fulldate = uploadDate.weekStartDate;
					
					INSERT INTO #WeekMapping
					SELECT 
						raw.Timekey AS UploadTimekey,
						'EndWeek' AS WeekCol,
						uploadDate.year AS UploadYear,
						uploadDate.weekNumber AS UploadWeekNumber,
						'EndWeek' AS ProjectionType,
						endWeekDate.year AS ProjectionYear,
						endWeekDate.weekNumber AS ProjectionWeekNumber,
						CONVERT(VARCHAR(8), endWeekDate.weekStartDate, 112) AS ProjectionTimekey,
						endWeekDate.weekStartDate AS ProjectionDate
					FROM (SELECT DISTINCT Timekey FROM XFC_TRS_RAW_CashDebtPosition WHERE Account IS NOT NULL) raw
					INNER JOIN XFC_TRS_AUX_Date uploadDate
						ON CONVERT(VARCHAR(8), uploadDate.weekStartDate, 112) = raw.Timekey
						AND uploadDate.fulldate = uploadDate.weekStartDate
					INNER JOIN XFC_TRS_AUX_Date endWeekDate
						ON endWeekDate.weekStartDate = DATEADD(WEEK, 4, uploadDate.weekStartDate)
						AND endWeekDate.fulldate = endWeekDate.weekStartDate;
					
					DECLARE @MappingRows INT = @@ROWCOUNT + (SELECT COUNT(*) FROM #WeekMapping WHERE WeekCol = 'StartWeek');

					IF OBJECT_ID('tempdb..#EOMDates') IS NOT NULL DROP TABLE #EOMDates;
					
					CREATE TABLE #EOMDates (
						year INT NOT NULL,
						monthNumber INT NOT NULL,
						eomDate DATE NOT NULL,
						PRIMARY KEY CLUSTERED (year, monthNumber)
					);
					
					INSERT INTO #EOMDates
					SELECT 
						year, 
						monthNumber, 
						MAX(fulldate) AS eomDate
					FROM XFC_TRS_AUX_Date
					WHERE isMonthEnd = 1
					GROUP BY year, monthNumber;
					
					DECLARE @EOMDateRows INT = @@ROWCOUNT;

					IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RAW_CashDebt_Timekey_Account' AND object_id = OBJECT_ID('XFC_TRS_RAW_CashDebtPosition'))
					BEGIN
						CREATE INDEX IX_RAW_CashDebt_Timekey_Account 
							ON XFC_TRS_RAW_CashDebtPosition (Timekey, Account)
							INCLUDE (Entity, Flow, Bank, StartWeek, EndWeek);
					END
					
					IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RAW_CashDebt_Timekey_Entity' AND object_id = OBJECT_ID('XFC_TRS_RAW_CashDebtPosition'))
					BEGIN
						CREATE INDEX IX_RAW_CashDebt_Timekey_Entity
							ON XFC_TRS_RAW_CashDebtPosition (Timekey, Entity);
					END;

					WITH UnpivotedWeekData AS (
						SELECT 
							Timekey, Entity, Account, Flow, Bank, WeekCol, Amount
						FROM (
							SELECT 
								Timekey, Entity, Account, Flow, Bank,
								StartWeek, EndWeek
							FROM XFC_TRS_RAW_CashDebtPosition
							WHERE Account IS NOT NULL
						) AS src
						UNPIVOT (
							Amount FOR WeekCol IN (StartWeek, EndWeek)
						) AS u
					),
					EnrichedWeekData AS (
						SELECT 
							wm.UploadTimekey,
							wm.UploadYear,
							wm.UploadWeekNumber,
							CASE UPPER(LTRIM(RTRIM(unpvt.Entity)))
								WHEN 'HORSE POWERTRAIN SOLUTION' THEN 1300 WHEN 'HORSE SPAIN' THEN 1301
								WHEN 'OYAK HORSE' THEN 1302 WHEN 'HORSE BRAZIL' THEN 1303
								WHEN 'HORSE CHILE' THEN 585 WHEN 'HORSE ARGENTINA' THEN 592
								WHEN 'HORSE ROMANIA' THEN 611 WHEN 'HORSE AVEIRO' THEN 671
								WHEN 'HORSE HPL' THEN 997
								WHEN 'HPL CHINA' THEN 999 WHEN 'AUROBAY SWEDEN' THEN 998 ELSE 0
							END AS Entity_Id,
							unpvt.Entity,
							'FW' + RIGHT('0' + CAST(wm.UploadWeekNumber AS VARCHAR(2)), 2) AS Scenario,
							unpvt.Account,
							unpvt.Flow,
							unpvt.Bank,
							wm.ProjectionType,
							wm.ProjectionTimekey,
							wm.ProjectionYear,
							wm.ProjectionWeekNumber,
							NULL AS ProjectionMonthNumber,
							wm.ProjectionDate,
							wm.ProjectionWeekNumber - wm.UploadWeekNumber AS WeekOffset,
							unpvt.Amount
						FROM UnpivotedWeekData unpvt
						INNER JOIN #WeekMapping wm 
							ON unpvt.Timekey = wm.UploadTimekey 
							AND unpvt.WeekCol = wm.WeekCol
					)
					INSERT INTO {tableName} (
						UploadTimekey, UploadYear, UploadWeekNumber,
						Entity_Id, Entity, Scenario, Account, Flow, Bank,
						ProjectionType, ProjectionTimekey, ProjectionYear, ProjectionWeekNumber, 
						ProjectionMonthNumber, ProjectionDate, WeekOffset, Amount
					)
					SELECT * FROM EnrichedWeekData;
					
					DECLARE @StartEndWeekRows INT = @@ROWCOUNT;
					DECLARE @EOMStartTime DATETIME = GETDATE();
					
					INSERT INTO {tableName} (
						UploadTimekey, UploadYear, UploadWeekNumber,
						Entity_Id, Entity, Scenario, Account, Flow, Bank,
						ProjectionType, ProjectionTimekey, ProjectionYear, ProjectionWeekNumber, 
						ProjectionMonthNumber, ProjectionDate, WeekOffset, Amount
					)
					SELECT 
						raw.Timekey AS UploadTimekey,
						uploadDate.year AS UploadYear,
						uploadDate.weekNumber AS UploadWeekNumber,
						CASE UPPER(LTRIM(RTRIM(raw.Entity)))
							WHEN 'HORSE POWERTRAIN SOLUTION' THEN 1300 WHEN 'HORSE SPAIN' THEN 1301
							WHEN 'OYAK HORSE' THEN 1302 WHEN 'HORSE BRAZIL' THEN 1303
							WHEN 'HORSE CHILE' THEN 585 WHEN 'HORSE ARGENTINA' THEN 592
							WHEN 'HORSE ROMANIA' THEN 611 WHEN 'HORSE AVEIRO' THEN 671
							WHEN 'HORSE HPL' THEN 997
							WHEN 'HPL CHINA' THEN 999 WHEN 'AUROBAY SWEDEN' THEN 998 ELSE 0
						END AS Entity_Id,
						raw.Entity,
						'FW' + RIGHT('0' + CAST(uploadDate.weekNumber AS VARCHAR(2)), 2) AS Scenario,
						raw.Account,
						raw.Flow,
						raw.Bank,
						'EOM' AS ProjectionType,
						CONVERT(VARCHAR(8), eomDate.eomDate, 112) AS ProjectionTimekey,
						eomDate.year AS ProjectionYear,
						NULL AS ProjectionWeekNumber,
						uploadDate.monthNumber AS ProjectionMonthNumber,
						eomDate.eomDate AS ProjectionDate,
						0 AS WeekOffset,
						raw.EOM AS Amount
					FROM XFC_TRS_RAW_CashDebtPosition raw
					INNER JOIN XFC_TRS_AUX_Date uploadDate
						ON CONVERT(VARCHAR(8), uploadDate.weekStartDate, 112) = raw.Timekey
						AND uploadDate.fulldate = uploadDate.weekStartDate
					INNER JOIN #EOMDates eomDate
						ON eomDate.monthNumber = uploadDate.monthNumber
						AND eomDate.year = uploadDate.year
					WHERE raw.Account IS NOT NULL
						AND raw.EOM IS NOT NULL;
					
					DECLARE @EOMRows INT = @@ROWCOUNT;
					DECLARE @EOMExecutionTime INT = DATEDIFF(SECOND, @EOMStartTime, GETDATE());

					IF OBJECT_ID('tempdb..#WeekMapping') IS NOT NULL DROP TABLE #WeekMapping;
					IF OBJECT_ID('tempdb..#EOMDates') IS NOT NULL DROP TABLE #EOMDates;
					
					SET @InsertedRows = @StartEndWeekRows + @EOMRows;
					DECLARE @EntityCount INT;
					DECLARE @AccountCount INT;
					DECLARE @ContextYear INT;
					DECLARE @ContextWeek INT;
					DECLARE @ExecutionTime INT = DATEDIFF(SECOND, @StartTime, GETDATE());
					
					SELECT TOP 1
					    @EntityCount = (SELECT COUNT(DISTINCT Entity_Id) FROM {tableName} 
					                   WHERE UploadTimekey IN (SELECT DISTINCT Timekey FROM XFC_TRS_RAW_CashDebtPosition WHERE Account IS NOT NULL)),
					    @AccountCount = (SELECT COUNT(DISTINCT Account) FROM {tableName} 
					                    WHERE UploadTimekey IN (SELECT DISTINCT Timekey FROM XFC_TRS_RAW_CashDebtPosition WHERE Account IS NOT NULL)),
					    @ContextYear = UploadYear,
					    @ContextWeek = UploadWeekNumber
					FROM {tableName}
					WHERE UploadTimekey IN (SELECT DISTINCT Timekey FROM XFC_TRS_RAW_CashDebtPosition WHERE Account IS NOT NULL);
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