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
							
							'Get file names in the imported data capital folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/LandingPageDev/Imported Data/Capital/Depreciations"
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
								BRApi.Utilities.LoadCustomTableUsingExcel(
									si,
									SourceDataOriginTypes.FromFileUpload,
									fileName.Name,
									fileBytes
								)
							
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
								 	MERGE INTO XFC_INV_FACT_asset_depreciation AS target
									USING (
										SELECT
								 			asset_id,
								 			scenario,
								 			year,
								 			type,
								            CONVERT(INT, REPLACE(month, 'm', '')) AS month,
								            amount
								 		FROM (
								 			SELECT
												ad.asset_id,
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
											FROM XFC_INV_RAW_asset_depreciation ad
									 		INNER JOIN XFC_INV_MASTER_asset a ON ad.asset_id = a.id
									 			AND a.is_real = 1
									 		INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
									 			AND p.company_id = @paramCompany
									 		WHERE LOWER(TRIM(ad.type)) IN ('conso', 'social')
									 		GROUP BY
									 			ad.asset_id,
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
									AND target.scenario = source.scenario
									AND target.year = source.year
									AND target.month = source.month
									AND target.type = source.type
									WHEN MATCHED THEN
									    UPDATE SET
									        amount = source.amount
									WHEN NOT MATCHED THEN
									    INSERT (
									        asset_id, scenario, year, month, type, amount
									    )
									    VALUES (
									        source.asset_id, source.scenario, source.year, source.month, source.type, source.amount
									    );
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
							Dim sourcePath As String = "Documents/Public/LandingPageDev/Imported Data/Cash"
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
								BRApi.Utilities.LoadCustomTableUsingExcel(
									si,
									SourceDataOriginTypes.FromFileUpload,
									fileName.Name,
									fileBytes
								)
							
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
												pc.project_id,
												@paramScenario AS scenario,
												@paramYear AS year,
												SUM(pc.m1) AS m1,
												SUM(pc.m2) AS m2,
												SUM(pc.m3) AS m3,
												SUM(pc.m4) AS m4,
												SUM(pc.m5) AS m5,
												SUM(pc.m6) AS m6,
												SUM(pc.m7) AS m7,
												SUM(pc.m8) AS m8,
												SUM(pc.m9) AS m9,
												SUM(pc.m10) AS m10,
												SUM(pc.m11) AS m11,
												SUM(pc.m12) AS m12
											FROM XFC_INV_RAW_project_cash pc
									 		INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
									 			AND p.company_id = @paramCompany
									 		GROUP BY
												pc.project_id
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
							Dim sourcePath As String = "Documents/Public/LandingPageDev/Imported Data/Capital/Assets"
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
								If Not fileName.Name.Contains($"Asset{templateScenario}_") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'Asset{templateScenario}_'."))
								End If
								
								'Check that the file is for the selected company
								If Not fileName.Name.Contains($"Asset{templateScenario}_{parameterDict("paramCompany")}") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct." & vbCrLf &
										"Check that you are uploading the correct template for the selected Company."))
								End If
								
								'Get file bytes
								Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
				                    si,
				                    FileSystemLocation.ApplicationDatabase,
				                    fileName.Name,
				                    True, True
				                ).XFFile.ContentFileBytes
								
								'Load excel template
								BRApi.Utilities.LoadCustomTableUsingExcel(
									si,
									SourceDataOriginTypes.FromFileUpload,
									fileName.Name,
									fileBytes
								)
								
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
							
								'Declare insert query depending on scenario
								Dim insertQuery As String = If(
									templateScenario <> "Actual",
									$"
									 	INSERT INTO XFC_INV_MASTER_asset (
									 		id, is_real, cost_center_id, project_id, designation, category_id, activation_date, initial_value
									 	)
										SELECT
										 	'A' + RIGHT('000000' + CAST(
										        COALESCE(
										        	lan.last_asset_number,
										            0
										        ) + ROW_NUMBER() OVER(ORDER BY a.project_id) AS VARCHAR(6)
										    ), 6) AS id,
									 		0 AS is_real,
									 		a.cost_center_id,
									 		a.project_id,
									 		a.designation,
									 		a.category_id,
									 		a.activation_date,
									 		a.initial_value
										FROM XFC_INV_AUX_asset a
								 		INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
								 			AND p.company_id = @paramCompany
									 	LEFT JOIN (
									 		SELECT TOP 1 CAST(SUBSTRING(id, 2, 6) AS INT) AS last_asset_number
								        	FROM XFC_INV_MASTER_asset
											WHERE id LIKE 'A%'
								            ORDER BY id DESC
									 	) lan ON 1 = 1;
									",
									$"
										MERGE INTO XFC_INV_MASTER_asset AS target
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
											INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
												AND p.company_id = @paramCompany
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
								)
								
								BRApi.Database.ExecuteSql(
								 dbConn,
								 insertQuery,
								 dbParamInfos,
								 False
								)
							End Using
							
						#End Region
						
						#Region "Import Capitalizations"
						
						Else If paramFunction = "ImportCapitalizations" Then
							'Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Get file names in the imported data cash folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/LandingPageDev/Imported Data/Capital/Capitalizations"
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
								BRApi.Utilities.LoadCustomTableUsingExcel(
									si,
									SourceDataOriginTypes.FromFileUpload,
									fileName.Name,
									fileBytes
								)
							
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
												pc.project_id,
												@paramScenario AS scenario,
												@paramYear AS year,
												SUM(pc.m1) AS m1,
												SUM(pc.m2) AS m2,
												SUM(pc.m3) AS m3,
												SUM(pc.m4) AS m4,
												SUM(pc.m5) AS m5,
												SUM(pc.m6) AS m6,
												SUM(pc.m7) AS m7,
												SUM(pc.m8) AS m8,
												SUM(pc.m9) AS m9,
												SUM(pc.m10) AS m10,
												SUM(pc.m11) AS m11,
												SUM(pc.m12) AS m12
											FROM XFC_INV_RAW_project_capitalization pc
									 		INNER JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
									 			AND p.company_id = @paramCompany
									 		GROUP BY
												pc.project_id
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
