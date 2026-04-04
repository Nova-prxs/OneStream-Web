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
	Public Class master_cashflowforecasting
		
		'Declare table name
		Dim tableName As String = "XFC_TRS_Master_CashflowForecasting"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				
				'Up - DROP and CREATE to ensure clean state
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL
				BEGIN
					DROP TABLE {tableName};
				END
				
				CREATE TABLE {tableName} (
					[RowId] INT IDENTITY(1,1) PRIMARY KEY,
					[UploadTimekey] NVARCHAR(8) NOT NULL,
					[UploadYear] INT NOT NULL,
					[UploadWeekNumber] INT NOT NULL,
					[Entity_Id] INT NOT NULL,
					[Entity] NVARCHAR(255) NOT NULL,
					[Scenario] NVARCHAR(255) NOT NULL,
					[Account] NVARCHAR(255) NOT NULL,
					[Flow] NVARCHAR(255),
					[ProjectionType] NVARCHAR(20) NOT NULL,
					[ProjectionTimekey] NVARCHAR(8) NOT NULL,
					[ProjectionYear] INT NOT NULL,
					[ProjectionWeekNumber] INT NOT NULL,
					[ProjectionDate] DATE NOT NULL,
					[WeekOffset] INT NOT NULL,
					[Amount] DECIMAL(18,2)
				);
				
				CREATE INDEX IX_Master_CashFlow_Upload ON {tableName} (UploadTimekey, Entity_Id);
				CREATE INDEX IX_Master_CashFlow_Projection ON {tableName} (ProjectionYear, ProjectionWeekNumber);
				CREATE INDEX IX_Master_CashFlow_Type ON {tableName} (ProjectionType);
				CREATE INDEX IX_Master_CashFlow_Scenario ON {tableName} (Scenario, Entity_Id);
				"
				
				'Down
				Dim downQuery As String = $"
					IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL
					BEGIN
						DROP TABLE {tableName};
						PRINT 'Dropped {tableName} table';
					END
					ELSE
					BEGIN
						PRINT 'Table {tableName} does not exist';
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
                
                'Up - OPTIMIZED with CORRECT week mapping logic (ABSOLUTE weeks)
                Dim upQuery As String = $"
                    DECLARE @StartTime DATETIME = GETDATE();
                    DECLARE @DeletedRows INT = 0;
                    DECLARE @InsertedRows INT = 0;
                    DECLARE @ValidationMsg NVARCHAR(MAX);

                    IF NOT EXISTS (
                        SELECT 1 FROM XFC_TRS_RAW_CashFlowForecasting 
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
                        SELECT 1 FROM XFC_TRS_RAW_CashFlowForecasting 
                        WHERE Account IS NOT NULL 
                        AND UPPER(LTRIM(RTRIM(Entity))) NOT IN (
                            'HORSE POWERTRAIN SOLUTION', 'HORSE SPAIN', 'OYAK HORSE', 'HORSE BRAZIL',
                            'HORSE CHILE', 'HORSE ARGENTINA', 'HORSE ROMANIA', 'HORSE AVEIRO',
                            'HORSE HPL', 'HPL CHINA', 'AUROBAY SWEDEN'
                        )
                    )
                    BEGIN
                        SELECT TOP 1 @ValidationMsg = 'Invalid Entity: ' + Entity 
                        FROM XFC_TRS_RAW_CashFlowForecasting 
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
                        SELECT 1 FROM XFC_TRS_RAW_CashFlowForecasting 
                        WHERE Account IS NOT NULL
                        AND (Entity IS NULL OR LTRIM(RTRIM(Entity)) = '' 
                           OR Year IS NULL OR Timekey IS NULL OR LTRIM(RTRIM(Timekey)) = '')
                    )
                    BEGIN
                        RAISERROR('Error: Required fields missing. Account, Entity, Year, and Timekey are mandatory.', 16, 1);
                        RETURN;
                    END

                    IF EXISTS (
                        SELECT 1
                        FROM XFC_TRS_RAW_CashFlowForecasting
                        WHERE Account IS NOT NULL
                        GROUP BY Timekey, Entity, Account, Flow
                        HAVING COUNT(*) > 1
                    )
                    BEGIN
                        SELECT TOP 1 @ValidationMsg = 
                            'Timekey=' + Timekey + ', Entity=' + Entity + 
                            ', Account=' + Account + ', Flow=' + ISNULL(Flow, 'NULL') +
                            ' (Count: ' + CAST(COUNT(*) AS VARCHAR(5)) + ')'
                        FROM XFC_TRS_RAW_CashFlowForecasting
                        WHERE Account IS NOT NULL
                        GROUP BY Timekey, Entity, Account, Flow
                        HAVING COUNT(*) > 1;
                        
                        RAISERROR('Error: Duplicate rows detected. %s', 16, 1, @ValidationMsg);
                        RETURN;
                    END
                    
                    DELETE m
                    FROM {tableName} m
                    INNER JOIN (
                        SELECT DISTINCT Timekey, UPPER(LTRIM(RTRIM(Entity))) AS Entity
                        FROM XFC_TRS_RAW_CashFlowForecasting 
                        WHERE Account IS NOT NULL
                    ) raw
                    ON m.UploadTimekey = raw.Timekey 
                    AND UPPER(LTRIM(RTRIM(m.Entity))) = raw.Entity;
                    
                    SET @DeletedRows = @@ROWCOUNT;

                    IF OBJECT_ID('tempdb..#WeekMapping') IS NOT NULL DROP TABLE #WeekMapping;
                    
                    CREATE TABLE #WeekMapping (
                        UploadTimekey NVARCHAR(8) NOT NULL,
                        WeekCol VARCHAR(10) NOT NULL,
                        WeekOffset INT NOT NULL,
                        UploadYear INT NOT NULL,
                        UploadWeekNumber INT NOT NULL,
                        ProjectionYear INT NOT NULL,
                        ProjectionWeekNumber INT NOT NULL,
                        ProjectionTimekey VARCHAR(8) NOT NULL,
                        ProjectionDate DATE NOT NULL,
                        ProjectionType VARCHAR(20) NOT NULL,
                        
                        INDEX IX_WeekMapping_Clustered CLUSTERED (UploadTimekey, WeekCol)
                    );

                    INSERT INTO #WeekMapping
                    SELECT 
                        raw.Timekey AS UploadTimekey,
                        'Actual' AS WeekCol,
                        -1 AS WeekOffset,
                        uploadDate.year AS UploadYear,
                        uploadDate.weekNumber AS UploadWeekNumber,
                        actualDate.year AS ProjectionYear,
                        actualDate.weekNumber AS ProjectionWeekNumber,
                        CONVERT(VARCHAR(8), actualDate.weekStartDate, 112) AS ProjectionTimekey,
                        actualDate.weekStartDate AS ProjectionDate,
                        'Actual' AS ProjectionType
                    FROM (SELECT DISTINCT Timekey FROM XFC_TRS_RAW_CashFlowForecasting WHERE Account IS NOT NULL) raw
                    INNER JOIN XFC_TRS_AUX_Date uploadDate
                        ON CONVERT(VARCHAR(8), uploadDate.weekStartDate, 112) = raw.Timekey
                        AND uploadDate.fulldate = uploadDate.weekStartDate
                    INNER JOIN XFC_TRS_AUX_Date actualDate
                        ON CONVERT(VARCHAR(8), actualDate.weekStartDate, 112) < raw.Timekey
                        AND actualDate.fulldate = actualDate.weekStartDate
                        AND actualDate.weekStartDate = (
                            SELECT TOP 1 weekStartDate 
                            FROM XFC_TRS_AUX_Date 
                            WHERE CONVERT(VARCHAR(8), weekStartDate, 112) < raw.Timekey
                            AND fulldate = weekStartDate
                            ORDER BY weekStartDate DESC
                        );
                    
                    INSERT INTO #WeekMapping
                    SELECT 
                        raw.Timekey AS UploadTimekey,
                        cols.WeekCol,
                        cols.WeekOffset,
                        uploadDate.year AS UploadYear,
                        uploadDate.weekNumber AS UploadWeekNumber,
                        proj.year AS ProjectionYear,
                        proj.weekNumber AS ProjectionWeekNumber,
                        CONVERT(VARCHAR(8), proj.weekStartDate, 112) AS ProjectionTimekey,
                        proj.weekStartDate AS ProjectionDate,
                        'Projection' AS ProjectionType
                    FROM (SELECT DISTINCT Timekey FROM XFC_TRS_RAW_CashFlowForecasting WHERE Account IS NOT NULL) raw
                    INNER JOIN XFC_TRS_AUX_Date uploadDate
                        ON CONVERT(VARCHAR(8), uploadDate.weekStartDate, 112) = raw.Timekey
                        AND uploadDate.fulldate = uploadDate.weekStartDate
                    CROSS JOIN (
                        SELECT 'W01' AS WeekCol, 0 AS WeekOffset, 1 AS WeekNum UNION ALL
                        SELECT 'W02', 1, 2 UNION ALL SELECT 'W03', 2, 3 UNION ALL SELECT 'W04', 3, 4 UNION ALL
                        SELECT 'W05', 4, 5 UNION ALL SELECT 'W06', 5, 6 UNION ALL SELECT 'W07', 6, 7 UNION ALL
                        SELECT 'W08', 7, 8 UNION ALL SELECT 'W09', 8, 9 UNION ALL SELECT 'W10', 9, 10 UNION ALL
                        SELECT 'W11', 10, 11 UNION ALL SELECT 'W12', 11, 12 UNION ALL SELECT 'W13', 12, 13 UNION ALL
                        SELECT 'W14', 13, 14 UNION ALL SELECT 'W15', 14, 15 UNION ALL SELECT 'W16', 15, 16 UNION ALL
                        SELECT 'W17', 16, 17 UNION ALL SELECT 'W18', 17, 18 UNION ALL SELECT 'W19', 18, 19 UNION ALL
                        SELECT 'W20', 19, 20 UNION ALL SELECT 'W21', 20, 21 UNION ALL SELECT 'W22', 21, 22 UNION ALL
                        SELECT 'W23', 22, 23 UNION ALL SELECT 'W24', 23, 24 UNION ALL SELECT 'W25', 24, 25 UNION ALL
                        SELECT 'W26', 25, 26 UNION ALL SELECT 'W27', 26, 27 UNION ALL SELECT 'W28', 27, 28 UNION ALL
                        SELECT 'W29', 28, 29 UNION ALL SELECT 'W30', 29, 30 UNION ALL SELECT 'W31', 30, 31 UNION ALL
                        SELECT 'W32', 31, 32 UNION ALL SELECT 'W33', 32, 33 UNION ALL SELECT 'W34', 33, 34 UNION ALL
                        SELECT 'W35', 34, 35 UNION ALL SELECT 'W36', 35, 36 UNION ALL SELECT 'W37', 36, 37 UNION ALL
                        SELECT 'W38', 37, 38 UNION ALL SELECT 'W39', 38, 39 UNION ALL SELECT 'W40', 39, 40 UNION ALL
                        SELECT 'W41', 40, 41 UNION ALL SELECT 'W42', 41, 42 UNION ALL SELECT 'W43', 42, 43 UNION ALL
                        SELECT 'W44', 43, 44 UNION ALL SELECT 'W45', 44, 45 UNION ALL SELECT 'W46', 45, 46 UNION ALL
                        SELECT 'W47', 46, 47 UNION ALL SELECT 'W48', 47, 48 UNION ALL SELECT 'W49', 48, 49 UNION ALL
                        SELECT 'W50', 49, 50 UNION ALL SELECT 'W51', 50, 51 UNION ALL SELECT 'W52', 51, 52 UNION ALL
                        SELECT 'W53', 52, 53 UNION ALL SELECT 'W54', 53, 54 UNION ALL SELECT 'W55', 54, 55 UNION ALL
                        SELECT 'W56', 55, 56 UNION ALL SELECT 'W57', 56, 57 UNION ALL SELECT 'W58', 57, 58 UNION ALL
                        SELECT 'W59', 58, 59 UNION ALL SELECT 'W60', 59, 60 UNION ALL SELECT 'W61', 60, 61
                    ) AS cols
                    INNER JOIN XFC_TRS_AUX_Date proj 
                        ON proj.weekNumber = cols.WeekNum
                        AND proj.year = CASE
                            WHEN cols.WeekNum > uploadDate.weekNumber AND cols.WeekNum <= 53 THEN uploadDate.year
                            WHEN cols.WeekNum < uploadDate.weekNumber THEN uploadDate.year + 1
                            ELSE uploadDate.year
                        END
                        AND proj.fulldate = proj.weekStartDate
                    WHERE cols.WeekCol IN (
                        SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'XFC_TRS_RAW_CashFlowForecasting' 
                        AND COLUMN_NAME LIKE 'W[0-9][0-9]'
                    );
                    
                    DECLARE @MappingRows INT = @@ROWCOUNT;

                    WITH UnpivotedData AS (
                        SELECT 
                            Timekey, Year, Entity, Account, Flow, WeekCol, Amount
                        FROM (
                            SELECT 
                                Timekey, Year, Entity, Account, Flow,
                                Actual,W01,W02,W03,W04,W05,W06,W07,W08,W09,W10,W11,W12,W13,W14,W15,W16,W17,W18,W19,W20,
                                W21,W22,W23,W24,W25,W26,W27,W28,W29,W30,W31,W32,W33,W34,W35,W36,W37,W38,W39,W40,
                                W41,W42,W43,W44,W45,W46,W47,W48,W49,W50,W51,W52,W53,W54,W55,W56,W57,W58,W59,W60,W61
                            FROM XFC_TRS_RAW_CashFlowForecasting
                            WHERE Account IS NOT NULL
                        ) AS src
                        UNPIVOT (
                            Amount FOR WeekCol IN (
                                Actual,W01,W02,W03,W04,W05,W06,W07,W08,W09,W10,W11,W12,W13,W14,W15,W16,W17,W18,W19,W20,
                                W21,W22,W23,W24,W25,W26,W27,W28,W29,W30,W31,W32,W33,W34,W35,W36,W37,W38,W39,W40,
                                W41,W42,W43,W44,W45,W46,W47,W48,W49,W50,W51,W52,W53,W54,W55,W56,W57,W58,W59,W60,W61
                            )
                        ) AS u
                    ),
                    EnrichedData AS (
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
                            wm.ProjectionType,
                            wm.ProjectionTimekey,
                            wm.ProjectionYear,
                            wm.ProjectionWeekNumber,
                            wm.ProjectionDate,
                            wm.WeekOffset,
                            unpvt.Amount
                        FROM UnpivotedData unpvt
                        INNER JOIN #WeekMapping wm 
                            ON unpvt.Timekey = wm.UploadTimekey 
                            AND unpvt.WeekCol = wm.WeekCol
                    )
                    INSERT INTO {tableName} (
                        UploadTimekey, UploadYear, UploadWeekNumber,
                        Entity_Id, Entity, Scenario, Account, Flow,
                        ProjectionType, ProjectionTimekey, ProjectionYear, ProjectionWeekNumber, 
                        ProjectionDate, WeekOffset, Amount
                    )
                    SELECT * FROM EnrichedData;

                    SET @InsertedRows = @@ROWCOUNT;

                    IF OBJECT_ID('tempdb..#WeekMapping') IS NOT NULL DROP TABLE #WeekMapping;
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