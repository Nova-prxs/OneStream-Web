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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_templates
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get function name
						Dim paramFunction As String = args.NameValuePairs("p_function")
						
						'Control function name
						
						#Region "Import DnA"
						
						If paramFunction = "ImportDnA" Then
							'Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							'Get first child entity
							Dim firstChildEntity As String = HelperFunctionsBR.GetBaseEntity(si, parameterDict("paramCompany")).Split(",")(0)
							dbParamInfos.Add(
								New DbParamInfo("paramFirstChildEntity", firstChildEntity)
							)
							
							'Get file names in the imported data capital folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Capital/Depreciations"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							'Loop through the folder files to populate the raw data table (it must be just one)
							For Each fileName As NameAndAccessLevel In fileNames
								'Check that the file has the correct name
								If Not fileName.Name.Contains("AssetDepreciation_") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'AssetDepreciation_'."))
								End If
								
								'Check that the file is for the selected company, year and scenario
								If Not fileName.Name.Contains($"AssetDepreciation_{parameterDict("paramCompany")}_{parameterDict("paramScenario")}_{parameterDict("paramYear")}") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct." & vbCrLf &
										"Check that you are uploading the correct template for the selected Company, Year And Scenario."))
								End If
								
								'Get file bytes
								Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
				                    si,
				                    FileSystemLocation.ApplicationDatabase,
				                    fileName.Name,
				                    True, True
				                ).XFFile.ContentFileBytes
								
								'Load excel template
								Try
									BRApi.Utilities.LoadCustomTableUsingExcel(
										si,
										SourceDataOriginTypes.FromFileUpload,
										fileName.Name,
										fileBytes
									)
								Catch ex As Exception
									'Clear folder and throw error
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							
								'Create processed folder if necessary
								BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
								
								'Copy file to processed folder
								UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
								
								'Clear folder and exit the loop
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Exit For
							Next
							
							'Merge data into the fact table filtering out the assets that are not in the selected company
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
								 dbConn,
								 $"
									DECLARE @companyName VARCHAR(55)
									SELECT @companyName = name
									FROM (
									    SELECT TOP 1 name
									    FROM XFC_INV_MASTER_company
									    WHERE id = @paramCompany
									) AS subquery;
								
									-- Create a generic project for assets
									IF NOT EXISTS (
										SELECT 1
										FROM XFC_INV_MASTER_project
										WHERE project = CONCAT('ASSET', @paramCompany)
									)
									BEGIN
										INSERT INTO XFC_INV_MASTER_project (
											project,
											project_name,
											company_id,
											company_name,
								 			aggregate,
								 			type,
											is_real,
											is_closed
										) VALUES (
											CONCAT('ASSET', @paramCompany),
											'Generic project for assets',
											@paramCompany,
											@companyName,
								 			'No aggregate',
								 			'No type',
											0,
											1
										)
									END
								 
									-- Create projects in assets that are not created yet
								 	INSERT INTO XFC_INV_MASTER_project (
								 		project,
										project_name,
										company_id,
										company_name,
										is_real,
										is_closed
								 	)
								 	SELECT DISTINCT
								 		TRIM(a.project_id) AS project,
										'Project created from Asset Import' AS project_name,
										@paramCompany AS company_id,
										@companyName AS company_name,
										1 AS is_real,
										0 AS is_closed
									FROM XFC_INV_RAW_asset_depreciation a
								 	LEFT JOIN XFC_INV_MASTER_project p ON TRIM(a.project_id) = p.project
								 	WHERE p.project IS NULL
										AND TRIM(a.project_id) <> ''
										AND a.project_id IS NOT NULL;
								 
								 	-- Create assets if not created
								 
								 	/*
								 	INSERT INTO XFC_INV_MASTER_asset (
								 		id,
										is_real,
										cost_center_id,
										project_id,
										designation,
										category_id,
										initial_value,
								 		is_real
								 	)
								 	SELECT
								 		TRIM(ad.asset_id),
								 		1,
								 		entity_cost_center_id,
								 		CONCAT('ASSET', @paramCompany),
								 		'Asset created automatically from DnA import',
								 		3005,
								 		0,
								 		1
									FROM XFC_INV_RAW_asset_depreciation ad
								 	LEFT JOIN (
								 		SELECT a.id AS asset_id, p.project
								 		FROM XFC_INV_MASTER_asset a
								 		INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
								 			AND p.company_id = @paramCompany
								 	) p ON TRIM(ad.asset_id) = p.asset_id
								 	CROSS JOIN (
								 		SELECT TOP 1
								 			id AS entity_cost_center_id
								 		FROM XFC_INV_MASTER_cost_center
								 		WHERE entity_member = @paramFirstChildEntity
								 	) AS cost_center_subquery
								 	WHERE p.project IS NULL;
								 	*/
								 	
								 	MERGE INTO XFC_INV_MASTER_asset AS target
								 	USING (
								 		SELECT DISTINCT
								 			TRIM(asset_id) AS id, TRIM(project_id) AS project_id, TRIM(designation) AS designation,
								 			TRIM(cost_center_id) AS cost_center_id, TRIM(category_id) AS category_id
								 		FROM XFC_INV_RAW_asset_depreciation
								 	) AS source
								 	ON target.id = source.id
								 		AND target.project_id = source.project_id
								 	WHEN MATCHED THEN
								 		UPDATE SET
								 			designation = source.designation,
								 			cost_center_id = source.cost_center_id,
								 			category_id = source.category_id
								 	WHEN NOT MATCHED THEN
								 		INSERT (id, project_id, designation, cost_center_id, category_id, initial_value, is_real)
								 		VALUES (
								 			source.id, source.project_id, source.designation, source.cost_center_id, source.category_id, 0, 1
								 		);
								 ",
								 dbParamInfos,
								 False
								)
								BRApi.Database.ExecuteSql(
								 dbConn,
								 $"
								 	WITH project_filtered AS (
								 		SELECT project
								 		FROM XFC_INV_MASTER_project
								 		WHERE company_id = @paramCompany
								 	), asset_filtered AS (
								 		SELECT id, project_id
							 			FROM XFC_INV_MASTER_asset a WITH(NOLOCK)
								 		WHERE EXISTS (
								 			SELECT 1
								 			FROM project_filtered pf
								 			WHERE a.project_id = pf.project
								 		)
								 	), asset_depreciation_filtered AS (
								 		SELECT *
								 		FROM XFC_INV_FACT_asset_depreciation ad
								 		WHERE EXISTS (
								 			SELECT 1
								 			FROM asset_filtered af
								 			WHERE ad.asset_id = af.id
								 				AND ad.project_id = af.project_id
								 		)
								 	)
								 
								 	MERGE INTO asset_depreciation_filtered AS target
									USING (
										SELECT
								 			asset_id,
								 			project_id,
								 			scenario,
								 			year,
								 			type,
								            CONVERT(INT, REPLACE(month, 'm', '')) AS month,
								            amount
								 		FROM (
								 			SELECT
												TRIM(ad.asset_id) AS asset_id,
								 				ad.project_id AS project_id,
												@paramScenario AS scenario,
												@paramYear AS year,
									 			LOWER(TRIM(ad.type)) AS type,
												SUM(ad.m1) AS m1,
												SUM(ad.m2) AS m2,
												SUM(ad.m3) AS m3,
												SUM(ad.m4) AS m4,
												SUM(ad.m5) AS m5,
												SUM(ad.m6) AS m6,
												SUM(ad.m7) AS m7,
												SUM(ad.m8) AS m8,
												SUM(ad.m9) AS m9,
												SUM(ad.m10) AS m10,
												SUM(ad.m11) AS m11,
												SUM(ad.m12) AS m12
											FROM (
								 				SELECT *
								 				FROM XFC_INV_RAW_asset_depreciation
								 				WHERE LOWER(TRIM(type)) = 'conso'
								 			) ad
									 		WHERE EXISTS (
								 				SELECT 1
								 				FROM asset_filtered af
								 				WHERE TRIM(ad.asset_id) = af.id
								 			)
									 		GROUP BY
									 			TRIM(ad.asset_id),
								 				ad.project_id,
									 			LOWER(TRIM(ad.type))
								 		) AS subquery
								 		UNPIVOT (
								            amount FOR month IN (
								 				m1, m2, m3, m4, m5, m6, 
								            	m7, m8, m9, m10, m11, m12
								 			)
								        ) AS pc
								 		WHERE amount <> -1
									) AS source
									ON target.asset_id = source.asset_id
									AND target.project_id = source.project_id
									AND target.scenario = source.scenario
									AND target.year = source.year
									AND target.month = source.month
									AND target.type = source.type
									WHEN MATCHED THEN
									    UPDATE SET
									        amount = source.amount
									WHEN NOT MATCHED THEN
									    INSERT (
									        asset_id, project_id, scenario, year, month, type, amount
									    )
									    VALUES (
									        source.asset_id, source.project_id, source.scenario, source.year, source.month, source.type, source.amount
									    );
								 
								 	-- Clear rows with a 0
								 	DELETE FROM XFC_INV_FACT_asset_depreciation
								 	WHERE amount = 0
								 ",
								 dbParamInfos,
								 False
								)
							End Using
							
						#End Region
						
						#Region "Import Cash"
						
						Else If paramFunction = "ImportCash" Then
						    'Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						
						    'Get file names in the imported data cash folder and build path to save the processed files
						    Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Cash"
						    Dim targetFinalPath As String = "Processed"
						    Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
						    Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
						        si,
						        FileSystemLocation.ApplicationDatabase,
						        sourcePath,
						        XFFileType.All,
						        False, False, False
						    )
						
						    'Loop through the folder files to populate the raw data table (it must be just one)
						    For Each fileName As NameAndAccessLevel In fileNames
						        'Check that the file has the correct name
						        If Not fileName.Name.Contains("ProjectCash_") Then
						            'Clear folder
						            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						            Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'ProjectCash_'."))
						        End If
						
						        'Check that the file is for the selected company, year and scenario
						        If Not fileName.Name.Contains($"ProjectCash_{parameterDict("paramCompany")}_{parameterDict("paramScenario")}_{parameterDict("paramYear")}") Then
						            'Clear folder
						            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						            Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct." & vbCrLf &
						                "Check that you are uploading the correct template for the selected Company, Year And Scenario."))
						        End If
						
						        'Get file bytes
						        Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
						            si,
						            FileSystemLocation.ApplicationDatabase,
						            fileName.Name,
						            True, True
						        ).XFFile.ContentFileBytes
						
						        'Load excel template
						        Try
						            BRApi.Utilities.LoadCustomTableUsingExcel(
						                si,
						                SourceDataOriginTypes.FromFileUpload,
						                fileName.Name,
						                fileBytes
						            )
						        Catch ex As Exception
						            'Clear folder and throw error
						            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						        End Try
						
						        'Create processed folder if necessary
						        BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
						
						        'Copy file to processed folder
						        UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
						
						        'Clear folder and exit the loop
						        UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						        Exit For
						    Next
						
						    'Validate that no data is being loaded into a confirmed month
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        ' 1. Get the list of confirmed months for the current POV
						        Dim confirmedMonthsDt As DataTable = BRApi.Database.ExecuteSql(
						            dbConn,
						            $"
						                SELECT M1, M2, M3, M4, M5, M6, M7, M8, M9, M10, M11, M12
						                FROM XFC_INV_AUX_confirmed_month
						                WHERE company_id = @paramCompany 
						                AND year = @paramYear 
						                AND scenario = @paramScenario 
						                AND type = 'Cash'
						            ",
						            dbParamInfos,
						            False
						        )
						
						        ' Only proceed with validation if there is a confirmation entry for the POV
						        If confirmedMonthsDt.Rows.Count > 0 Then
						            Dim confirmedMonthsRow As DataRow = confirmedMonthsDt.Rows(0)
						
						            ' 2. Get the list of months being updated from the raw data table
						            Dim updatedMonthsDt As DataTable = BRApi.Database.ExecuteSql(
						                dbConn,
						                $"
						                    SELECT DISTINCT month
						                    FROM (
						                        SELECT m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12
						                        FROM XFC_INV_RAW_project_cash
						                    ) AS p
						                    UNPIVOT (
						                        amount FOR month IN (m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12)
						                    ) AS unpvt
						                    WHERE amount IS NOT NULL AND amount <> -1
						                ",
						                False
						            )
						
						            If updatedMonthsDt.Rows.Count > 0 Then
						                Dim conflictingMonths As New List(Of String)
						
						                ' 3. Compare updated months with confirmed months
						                For Each row As DataRow In updatedMonthsDt.Rows
						                    Dim monthToCheck As String = row("month").ToString().ToUpper() ' e.g., "M1"
						                    
						                    ' CORRECTED LOGIC: Check for DBNull before converting to Boolean
						                    If Not IsDBNull(confirmedMonthsRow(monthToCheck)) AndAlso CBool(confirmedMonthsRow(monthToCheck)) Then
						                        conflictingMonths.Add(monthToCheck)
						                    End If
						                Next
						
						                ' 4. If there are conflicts, build an error message and throw an exception
						                If conflictingMonths.Count > 0 Then
						                    Dim errorMessage As String = "Data load failed. The following months are confirmed and cannot be modified: " & String.Join(", ", conflictingMonths)
						                    Throw ErrorHandler.LogWrite(si, New XFException(errorMessage))
						                End If
						            End If
						        End If
						
						    	'Merge data into the fact table filtering out the projects that are not in the selected company
								BRApi.Database.ExecuteSql(
								    dbConn,
								    $"
								        MERGE INTO XFC_INV_FACT_project_cash AS target
								        USING (
								            SELECT 
								                project_id,
								                scenario,
								                year,
								                CONVERT(INT, REPLACE(month, 'm', '')) AS month,
								                amount
								            FROM (
								                SELECT
								                    TRIM(pc.project_id) AS project_id,
								                    @paramScenario AS scenario,
								                    @paramYear AS year,
								                    SUM(NULLIF(pc.m1, -1) ) AS m1,
								                    SUM(NULLIF(pc.m2, -1)) AS m2,
								                    SUM(NULLIF(pc.m3, -1)) AS m3,
								                    SUM(NULLIF(pc.m4, -1)) AS m4,
								                    SUM(NULLIF(pc.m5, -1)) AS m5,
								                    SUM(NULLIF(pc.m6, -1)) AS m6,
								                    SUM(NULLIF(pc.m7, -1)) AS m7,
								                    SUM(NULLIF(pc.m8, -1)) AS m8,
								                    SUM(NULLIF(pc.m9, -1)) AS m9,
								                    SUM(NULLIF(pc.m10, -1)) AS m10,
								                    SUM(NULLIF(pc.m11, -1)) AS m11,
								                    SUM(NULLIF(pc.m12, -1)) AS m12
								                FROM XFC_INV_RAW_project_cash pc
								                INNER JOIN XFC_INV_MASTER_project p ON TRIM(pc.project_id) = p.project
								                    AND p.company_id = @paramCompany
								                GROUP BY
								                    TRIM(pc.project_id)
								            ) AS subquery
								            UNPIVOT (
								                amount FOR month IN (
								                    m1, m2, m3, m4, m5, m6, 
								                    m7, m8, m9, m10, m11, m12
								                )
								            ) AS pc
								            -- Ya no necesitas WHERE amount <> -1 porque UNPIVOT excluye NULL
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
								        -- Clear rows with a 0
								        DELETE FROM XFC_INV_FACT_project_cash
								        WHERE amount = 0
								    ",
								    dbParamInfos,
								    False
								)
						    End Using
						
						#End Region
						
						#Region "Import Assets"

Else If paramFunction = "ImportAssets" Then
    'Get parameters
    Dim queryParams As String = args.NameValuePairs("p_query_params")
    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
    
    'Define template scenario depending on the scenario parameter
    Dim templateScenario As String = If(parameterDict("paramScenario") = "Actual", "Actual", "Planning")
    
    'Get file names in the imported data capital folder and build path to save the processed files
    Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Capital/Assets"
    Dim targetFinalPath As String = "Processed"
    Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
    Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
        si,
        FileSystemLocation.ApplicationDatabase,
        sourcePath,
        XFFileType.All,
        False, False, False
    )
    
    'Loop through the folder files to populate the aux data table (it must be just one)
    For Each fileName As NameAndAccessLevel In fileNames
        'Check that the file has the correct name
        If Not fileName.Name.Contains($"New_Asset_") Then
            'Clear folder
            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
            Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'New_Asset_'."))
        End If
        
        'Check that the file is for the selected company
        If Not fileName.Name.Contains($"New_Asset_{parameterDict("paramScenario")}_{parameterDict("paramCompany")}") Then
            'Clear folder
            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
            Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct." & vbCrLf &
                "Check that you are uploading the correct template for the selected Company and Scenario."))
        End If
        
        'Get file bytes
        Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
            si,
            FileSystemLocation.ApplicationDatabase,
            fileName.Name,
            True, True
        ).XFFile.ContentFileBytes
        
        'Load excel template
        Try
            BRApi.Utilities.LoadCustomTableUsingExcel(
                si,
                SourceDataOriginTypes.FromFileUpload,
                fileName.Name,
                fileBytes
            )
        Catch ex As Exception
            'Clear folder and throw error
            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
        
        'Create processed folder if necessary
        BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
        
        'Copy file to processed folder
        UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
        
        'Clear folder and exit the loop
        UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
        
        Exit For
    Next
    
    'Merge data into the master table filtering out the projects that are not in the selected company
    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
        dbConn.BeginTrans()
        Try
            'Declare origin table depending on scenario
            Dim originTable As String = If(templateScenario <> "Actual", "XFC_INV_AUX_asset", "XFC_INV_RAW_asset")
            'Declare query to create projects if not created
            Dim projectCreationQuery As String = $"
                DECLARE @companyName VARCHAR(55)
                SELECT @companyName = name
                FROM (
                    SELECT TOP 1 name
                    FROM XFC_INV_MASTER_company
                    WHERE id = @paramCompany
                ) AS subquery;
            
                -- Create a generic project for assets
                IF NOT EXISTS (
                    SELECT 1
                    FROM XFC_INV_MASTER_project
                    WHERE project = CONCAT('ASSET', @paramCompany)
                )
                BEGIN
                    INSERT INTO XFC_INV_MASTER_project (
                        project,
                        project_name,
                        company_id,
                        company_name,
                        aggregate,
                        type,
                        is_real,
                        is_closed
                    ) VALUES (
                        CONCAT('ASSET', @paramCompany),
                        'Generic project for assets',
                        @paramCompany,
                        @companyName,
                        'No aggregate',
                        'No type',
                        0,
                        1
                    )
                END
            
                -- Create projects in assets that are not created yet
                INSERT INTO XFC_INV_MASTER_project (
                    project,
                    project_name,
                    company_id,
                    company_name,
                    is_real,
                    is_closed
                )
                SELECT DISTINCT
                    TRIM(a.project_id) AS project,
                    'Project created from Asset Import' AS project_name,
                    @paramCompany AS company_id,
                    @companyName AS company_name,
                    1 AS is_real,
                    0 AS is_closed
                FROM {originTable} a
                LEFT JOIN XFC_INV_MASTER_project p ON TRIM(a.project_id) = p.project
                WHERE p.project IS NULL
                    AND TRIM(a.project_id) <> ''
                    AND a.project_id IS NOT NULL;
            "
            
            'Declare insert query depending on scenario
            Dim insertQuery As String = If(
                templateScenario <> "Actual",
                $"
                    {projectCreationQuery}
                
                    -- Create a temporary table to store the new asset IDs and project IDs
                    CREATE TABLE #TempAssetMapping (
                        new_asset_id VARCHAR(10),
                        project_id VARCHAR(50)
                    );
                
                    INSERT INTO XFC_INV_MASTER_asset (
                        id, is_real, cost_center_id, project_id, designation, category_id, activation_date, initial_value
                    )
                    OUTPUT 
                        INSERTED.id,
                        INSERTED.project_id
                    INTO #TempAssetMapping (new_asset_id, project_id)
                    SELECT
                        'A' + RIGHT('000000' + CAST(
                            COALESCE(
                                lan.last_asset_number,
                                0
                            ) + ROW_NUMBER() OVER(ORDER BY TRIM(a.project_id)) AS VARCHAR(6)
                        ), 6) AS id,
                        0 AS is_real,
                        TRIM(a.cost_center_id),
                        CASE
                            WHEN p.project IS NOT NULL THEN TRIM(a.project_id)
                            ELSE CONCAT('ASSET', @paramCompany)
                        END AS project_id,
                        TRIM(a.designation),
                        TRIM(a.category_id),
                        a.activation_date,
                        a.initial_value
                    FROM XFC_INV_AUX_asset a
                    LEFT JOIN XFC_INV_MASTER_project p ON TRIM(a.project_id) = p.project
                        AND p.company_id = @paramCompany
                    LEFT JOIN (
                        SELECT TOP 1 CAST(SUBSTRING(id, 2, 6) AS INT) AS last_asset_number
                        FROM XFC_INV_MASTER_asset
                        WHERE id LIKE 'A%'
                        ORDER BY id DESC
                    ) lan ON 1 = 1;
                
                    -- Insert into XFC_INV_AUX_Fictitious_Assets using the temp table
                    INSERT INTO XFC_INV_AUX_Fictitious_Assets (
                        asset_id, 
                        project_id, 
                        scenario, 
                        year
                    )
                    SELECT 
                        temp.new_asset_id,
                        temp.project_id,
                        @paramScenario,
                        @paramYear
                    FROM #TempAssetMapping temp;
                
                    -- Clean up temp table
                    DROP TABLE #TempAssetMapping;
                ",
                $"
                    {projectCreationQuery}
                    
                    -- Create a filtered master asset target table
                    WITH filtered_asset AS (
                        SELECT a.*
                        FROM XFC_INV_MASTER_asset a
                        INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
                            AND p.company_id = @paramCompany
                    )
                
                    MERGE INTO filtered_asset AS target
                    USING (
                        SELECT 
                            a.id,
                            1 AS is_real,
                            TRIM(a.cost_center_id) AS cost_center_id,
                            CASE
                                WHEN p.project IS NOT NULL THEN TRIM(a.project_id)
                                ELSE CONCAT('ASSET', @paramCompany)
                            END AS project_id,
                            TRIM(a.designation) AS designation,
                            TRIM(a.category_id) AS category_id,
                            a.activation_date,
                            a.initial_value
                        FROM XFC_INV_RAW_asset a
                        LEFT JOIN XFC_INV_MASTER_project p ON TRIM(a.project_id) = p.project
                            AND p.company_id = @paramCompany
                    ) AS source
                    ON target.id = source.id
                    WHEN MATCHED THEN
                        UPDATE SET
                            project_id = source.project_id,
                            cost_center_id = source.cost_center_id,
                            is_real = source.is_real,
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
                    
                    -- Create a filtered fact asset depreciation target table
                    WITH filtered_asset_depreciation AS (
                        SELECT ad.*
                        FROM XFC_INV_FACT_asset_depreciation ad
                        INNER JOIN XFC_INV_MASTER_project p ON ad.project_id = p.project
                            AND p.company_id = @paramCompany
                    )
                
                    MERGE INTO filtered_asset_depreciation AS target
                    USING (
                        SELECT 
                            a.id AS asset_id,
                            TRIM(a.project_id) AS project_id
                        FROM XFC_INV_RAW_asset a
                        LEFT JOIN XFC_INV_MASTER_project p ON TRIM(a.project_id) = p.project
                            AND p.company_id = @paramCompany
                    ) AS source
                    ON target.asset_id = source.asset_id
                    WHEN MATCHED THEN
                        UPDATE SET
                            project_id = source.project_id;
                "
            )
            
            BRApi.Database.ExecuteSql(
                dbConn,
                insertQuery,
                dbParamInfos,
                False
            )
            
            dbConn.CommitTrans()
        Catch ex As Exception
            dbConn.RollbackTrans()
            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
        End Try
    End Using
    
#End Region
						
						#Region "Import Capitalizations"
						
						Else If paramFunction = "ImportCapitalizations" Then
							'Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Get file names in the imported data cash folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Capital/Capitalizations"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							'Loop through the folder files to populate the raw data table (it must be just one)
							For Each fileName As NameAndAccessLevel In fileNames
								'Check that the file has the correct name
								If Not fileName.Name.Contains("ProjectCapitalizations_") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'ProjectCapitalizations_'."))
								End If
								
								'Check that the file is for the selected company, year and scenario
								If Not fileName.Name.Contains($"ProjectCapitalizations_{parameterDict("paramCompany")}_{parameterDict("paramScenario")}_{parameterDict("paramYear")}") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct." & vbCrLf &
										"Check that you are uploading the correct template for the selected Company, Year And Scenario."))
								End If
								
								'Get file bytes
								Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
				                    si,
				                    FileSystemLocation.ApplicationDatabase,
				                    fileName.Name,
				                    True, True
				                ).XFFile.ContentFileBytes
								
								'Load excel template
								Try
									BRApi.Utilities.LoadCustomTableUsingExcel(
										si,
										SourceDataOriginTypes.FromFileUpload,
										fileName.Name,
										fileBytes
									)
								Catch ex As Exception
									'Clear folder and throw error
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							
								'Create processed folder if necessary
								BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
								
								'Copy file to processed folder
								UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
								
								'Clear folder and exit the loop
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Exit For
							Next
							
							'Merge data into the fact table filtering out the projects that are not in the selected company
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
								 dbConn,
								 $"
								 	DECLARE @companyName VARCHAR(55)
									SELECT @companyName = name
									FROM (
									    SELECT TOP 1 name
									    FROM XFC_INV_MASTER_company
									    WHERE id = @paramCompany
									) AS subquery;
								
									-- Create a generic project for assets
									IF NOT EXISTS (
										SELECT 1
										FROM XFC_INV_MASTER_project
										WHERE project = CONCAT('ASSET', @paramCompany)
									)
									BEGIN
										INSERT INTO XFC_INV_MASTER_project (
											project,
											project_name,
											company_id,
											company_name,
								 			aggregate,
								 			type,
											is_real,
											is_closed
										) VALUES (
											CONCAT('ASSET', @paramCompany),
											'Generic project for assets',
											@paramCompany,
											@companyName,
								 			'No aggregate',
								 			'No type',
											0,
											1
										)
									END
								
									-- Create projects that are not created yet
								 	INSERT INTO XFC_INV_MASTER_project (
								 		project,
										project_name,
										company_id,
										company_name,
										is_real,
										is_closed
								 	)
								 	SELECT DISTINCT
								 		TRIM(c.project_id) AS project,
										'Project created from Capitalization Import' AS project_name,
										@paramCompany AS company_id,
										@companyName AS company_name,
										1 AS is_real,
										0 AS is_closed
									FROM XFC_INV_RAW_project_capitalization c
								 	LEFT JOIN XFC_INV_MASTER_project p ON TRIM(c.project_id) = p.project
								 	WHERE p.project IS NULL
										AND TRIM(c.project_id) <> ''
										AND c.project_id IS NOT NULL;
								 
								 	MERGE INTO XFC_INV_FACT_project_capitalization AS target
									USING (
										SELECT 
								 			project_id,
								 			scenario,
								 			year,
								            CONVERT(INT, REPLACE(month, 'm', '')) AS month,
								            amount
								 		FROM (
								 			SELECT
												project_id,
												@paramScenario AS scenario,
												@paramYear AS year,
												SUM(m1) AS m1,
												SUM(m2) AS m2,
												SUM(m3) AS m3,
												SUM(m4) AS m4,
												SUM(m5) AS m5,
												SUM(m6) AS m6,
												SUM(m7) AS m7,
												SUM(m8) AS m8,
												SUM(m9) AS m9,
												SUM(m10) AS m10,
												SUM(m11) AS m11,
												SUM(m12) AS m12
								 			FROM (
									 			SELECT
													CASE
														WHEN p.project IS NOT NULL THEN TRIM(pc.project_id)
														ELSE CONCAT('ASSET', @paramCompany)
													END AS project_id,
													pc.m1 AS m1,
													pc.m2 AS m2,
													pc.m3 AS m3,
													pc.m4 AS m4,
													pc.m5 AS m5,
													pc.m6 AS m6,
													pc.m7 AS m7,
													pc.m8 AS m8,
													pc.m9 AS m9,
													pc.m10 AS m10,
													pc.m11 AS m11,
													pc.m12 AS m12
												FROM XFC_INV_RAW_project_capitalization pc
										 		LEFT JOIN XFC_INV_MASTER_project p ON TRIM(pc.project_id) = p.project
										 			AND p.company_id = @paramCompany
								 			) AS innerquery
									 		GROUP BY project_id
								 		) AS subquery
								 		UNPIVOT (
								            amount FOR month IN (
								 				m1, m2, m3, m4, m5, m6, 
								            	m7, m8, m9, m10, m11, m12
								 			)
								        ) AS pc
								 		WHERE amount <> -1
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
								 
								 	-- Clear rows with a 0
								 	DELETE FROM XFC_INV_FACT_project_capitalization
								 	WHERE amount = 0
								 ",
								 dbParamInfos,
								 False
								)
							End Using
							
						#End Region
						
						#Region "Import Company Holidays"
						
						Else If paramFunction = "ImportCompanyHolidays" Then
							
							'Get file names in the imported data cash folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Aux/Holidays"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							'Loop through the folder files to populate the raw data table (it must be just one)
							For Each fileName As NameAndAccessLevel In fileNames
								'Check that the file has the correct name
								If Not fileName.Name.Contains("CompanyHoliday") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'CompanyHoliday'."))
								End If
								
								'Get file bytes
								Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
				                    si,
				                    FileSystemLocation.ApplicationDatabase,
				                    fileName.Name,
				                    True, True
				                ).XFFile.ContentFileBytes
								
								'Load excel template
								Try
									BRApi.Utilities.LoadCustomTableUsingExcel(
										si,
										SourceDataOriginTypes.FromFileUpload,
										fileName.Name,
										fileBytes
									)
								Catch ex As Exception
									'Clear folder and throw error
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							
								'Create processed folder if necessary
								BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
								
								'Copy file to processed folder
								UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
								
								'Clear folder and exit the loop
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Exit For
							Next
							
							'Merge data into the fact table filtering out the projects that are not in the selected company
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							
							    BRApi.Database.ExecuteSql(
							        dbConn,
							        $"
							            DELETE FROM XFC_INV_MASTER_company_holiday
							            WHERE company_id IN (SELECT DISTINCT company_id FROM XFC_INV_RAW_company_holiday);
							        ",
							        False
							    )
							
							    BRApi.Database.ExecuteSql(
							        dbConn,
							        $"
							            INSERT INTO XFC_INV_MASTER_company_holiday (date, company_id)
							            SELECT DISTINCT date, company_id
							            FROM XFC_INV_RAW_company_holiday;
							        ",
							        False
							    )
							End Using
							
						#End Region
						
						#Region "Import Cost Center Mapping"
						
						Else If paramFunction = "ImportCostCenterMapping" Then
							
							'Get file names in the imported data cash folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Aux/Cost Center"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							'Loop through the folder files to populate the raw data table (it must be just one)
							For Each fileName As NameAndAccessLevel In fileNames
								'Check that the file has the correct name
								If Not fileName.Name.Contains("CostCenterMapping") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'CostCenterMapping'."))
								End If
								
								'Get file bytes
								Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
				                    si,
				                    FileSystemLocation.ApplicationDatabase,
				                    fileName.Name,
				                    True, True
				                ).XFFile.ContentFileBytes
								
								'Load excel template
								Try
									BRApi.Utilities.LoadCustomTableUsingExcel(
										si,
										SourceDataOriginTypes.FromFileUpload,
										fileName.Name,
										fileBytes
									)
								Catch ex As Exception
									'Clear folder and throw error
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							
								'Create processed folder if necessary
								BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
								
								'Copy file to processed folder
								UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
								
								'Clear folder and exit the loop
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Exit For
							Next
							
							'Merge data into the fact table filtering out the projects that are not in the selected company
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
								 dbConn,
								 $"
									MERGE INTO XFC_INV_MASTER_cost_center AS target
									USING (
									    SELECT DISTINCT
									        id,
									        entity_member,
									        cost_center_type
									    FROM XFC_INV_RAW_cost_center
									) AS source
									ON target.id = source.id
									
									WHEN MATCHED THEN
									    UPDATE SET
									        target.entity_member = source.entity_member,
									        target.cost_center_type = source.cost_center_type
									
									WHEN NOT MATCHED BY TARGET THEN
									    INSERT (id, entity_member, cost_center_type)
									    VALUES (source.id, source.entity_member, source.cost_center_type);
								 ",
								 False
								)
							End Using
																												
						#End Region
						
						#Region "Import Cost Center Type Mapping"
						
						Else If paramFunction = "ImportCostCenterTypeMapping" Then
							
							'Get file names in the imported data cash folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Aux/Cost Center Type"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							'Loop through the folder files to populate the raw data table (it must be just one)
							For Each fileName As NameAndAccessLevel In fileNames
								'Check that the file has the correct name
								If Not fileName.Name.Contains("CostCenterTypeMapping") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'CostCenterTypeMapping'."))
								End If
								
								'Get file bytes
								Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
				                    si,
				                    FileSystemLocation.ApplicationDatabase,
				                    fileName.Name,
				                    True, True
				                ).XFFile.ContentFileBytes
								
								'Load excel template
								Try
									BRApi.Utilities.LoadCustomTableUsingExcel(
										si,
										SourceDataOriginTypes.FromFileUpload,
										fileName.Name,
										fileBytes
									)
								Catch ex As Exception
									'Clear folder and throw error
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							
								'Create processed folder if necessary
								BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
								
								'Copy file to processed folder
								UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
								
								'Clear folder and exit the loop
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Exit For
							Next
							
							'Merge data into the fact table filtering out the projects that are not in the selected company
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
								 dbConn,
								 $"
								 MERGE INTO XFC_INV_MASTER_cost_center_type AS target
								USING (
								    SELECT DISTINCT
								        id,
								        description,
								        depreciation_account,
								        asset_account
								    FROM XFC_INV_RAW_cost_center_type
								) AS source
								ON (target.id = source.id)
								WHEN MATCHED THEN
								    UPDATE SET
								        target.description = source.description,
								        target.depreciation_account = source.depreciation_account,
								        target.asset_account = source.asset_account
								
								WHEN NOT MATCHED BY TARGET THEN
								    INSERT (id, description, depreciation_account, asset_account)
								    VALUES (source.id, source.description, source.depreciation_account, source.asset_account);
								 ",
								 False
								)
							End Using
							
						#End Region
						
						#Region "Import Projects Type/Aggregate/BudgetOwner/Site info"
						
						Else If paramFunction = "ImportProjectUpdateInfo" Then
						    
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    
						    'Get file names in the imported data cash folder and build path to save the processed files
						    Dim sourcePath As String = "Documents/Public/Investments/Imported Data/Aux/Project Update Info"
						    Dim targetFinalPath As String = "Processed"
						    Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
						    Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
						        si,
						        FileSystemLocation.ApplicationDatabase,
						        sourcePath,
						        XFFileType.All,
						        False, False, False
						    )
						            
						    
						    'Loop through the folder files to populate the raw data table (it must be just one)
						    For Each fileName As NameAndAccessLevel In fileNames
						        'Check that the file has the correct name
						        If Not fileName.Name.Contains("ProjectUpdateInfo_") Then
						            'Clear folder
						            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						            Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'ProjectUpdateInfo_'."))
						        End If
						        
						        'Check that the file is for the selected company, year and scenario
						        If Not fileName.Name.Contains($"ProjectUpdateInfo_{parameterDict("paramCompany")}") Then
						            'Clear folder
						            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						            Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct." & vbCrLf &
						                "Check that you are uploading the correct template for the selected Company"))
						        End If
						        
						        'Get file bytes
						        Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
						            si,
						            FileSystemLocation.ApplicationDatabase,
						            fileName.Name,
						            True, True
						        ).XFFile.ContentFileBytes
						        
						        'Load excel template
						        Try
						            BRApi.Utilities.LoadCustomTableUsingExcel(
						                si,
						                SourceDataOriginTypes.FromFileUpload,
						                fileName.Name,
						                fileBytes
						            )
						        Catch ex As Exception
						            'Clear folder and throw error
						            UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						        End Try
						    
						        'Create processed folder if necessary
						        BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
						        
						        'Copy file to processed folder
						        UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, fileName.Name.Split("/").Last(), targetPath)
						        
						        'Clear folder and exit the loop
						        UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
						        Exit For
						    Next
						    
						    'Merge data into the fact table filtering out the projects that are not in the selected company
						    'Modified to preserve existing values when Excel columns are blank
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        BRApi.Database.ExecuteSql(
						         dbConn,
						         $"			
						            MERGE INTO XFC_INV_MASTER_project AS target
						            USING (
						                SELECT DISTINCT
						                    project,
						                    type,
						                    aggregate,
						                    budget_owner,
						                    site
						                FROM XFC_INV_AUX_project
						            ) AS source
						            ON (target.project = source.project AND target.company_id = @paramCompany)
						            
						            WHEN MATCHED THEN
						                UPDATE SET
						                    target.type = CASE 
						                        WHEN source.type IS NOT NULL AND LTRIM(RTRIM(source.type)) <> '' 
						                        THEN source.type 
						                        ELSE target.type 
						                    END,
						                    target.aggregate = CASE 
						                        WHEN source.aggregate IS NOT NULL AND LTRIM(RTRIM(source.aggregate)) <> '' 
						                        THEN source.aggregate 
						                        ELSE target.aggregate 
						                    END,
						                    target.budget_owner = CASE 
						                        WHEN source.budget_owner IS NOT NULL AND LTRIM(RTRIM(source.budget_owner)) <> '' 
						                        THEN source.budget_owner 
						                        ELSE target.budget_owner 
						                    END,
						                    target.site = CASE 
						                        WHEN source.site IS NOT NULL AND LTRIM(RTRIM(source.site)) <> '' 
						                        THEN source.site 
						                        ELSE target.site 
						                    END;
						         ",
						         dbParamInfos,
						         False
						        )
						    End Using
						#End Region
						
						End If
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "Run Population Queries"
		
		Private Sub RunPopulationQueries(ByVal si As SessionInfo, ByVal tableName As String)
			'Declare list of objects to populate with tables
			Dim tableList As New List(Of Object)
			'Control table name
			If tableName = "" Then
				
			End If
			
			'Return if no tables
			If tableList.Count < 1 Then Return
			
			'Get migration queries
			Dim populationQueries As List(Of String) = Me.GetPopulationQueries(si, tableList)
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Populate tables
				For Each query In populationQueries
					BRApi.Database.ExecuteSql(dbConn, query, False)
				Next
			End Using
		End Sub
		
		Private Function GetPopulationQueries(ByVal si As SessionInfo, ByVal tables As List(Of Object)) As List(Of String)
			'Declare list of queries
			Dim queries As New List(Of String)
			
			'Loop through all the tables to get the queries
			For Each table In tables
				queries.Add(table.GetPopulationQuery(si, "up"))
			Next
			
            Return queries
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
