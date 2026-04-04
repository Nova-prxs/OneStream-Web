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
						
						#Region "Import Work Orders"
						
						If paramFunction = "ImportWorkOrders" Then

							'Get parameters (optional — p_query_params may not be passed yet)
							Dim paramEntity As String = ""
							Dim paramYear As Integer = 0
							If args.NameValuePairs.ContainsKey("p_query_params") Then
								Dim paramQueryParams As String = args.NameValuePairs("p_query_params")
								Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, paramQueryParams)
								paramEntity = parameterDict("paramEntity")
								paramYear = CInt(parameterDict("paramYear"))
							End If

							'Get file names in the imported data capital folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Services/Imported Data/Work Orders"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							If fileNames.Count = 0 Then Throw New Exception("Error importing template: template file not found")
							
							'Loop through the folder files to populate the raw data table (it must be just one)
							For Each fileName As NameAndAccessLevel In fileNames
								'Check that the file has the correct name
								If Not fileName.Name.Contains("Master_Of_Contracts") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check if the name contains 'Master_Of_Contracts'."))
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
									'Clear folder and exit throw error
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
							
								'Create processed folder if necessary
								BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
								
								'Add today's date to the end of the file name
								Dim today As Date = Date.Now()
								Dim copyFileName As String = fileName.Name.Split("/").Last().Replace(".xlsx", $" {today.Year}-{today.Month}-{today.Day}.xlsx")
								
								'Copy file to processed folder
								UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, copyFileName, targetPath)
								
								'Clear folder and exit the loop
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Exit For
							Next
							
							'Run warning validations for work order codes (only if params available)
							If paramEntity <> "" AndAlso paramYear > 0 Then
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim warningDbParams As New List(Of DbParamInfo) From {
									New DbParamInfo("paramEntity", paramEntity),
									New DbParamInfo("paramYear", paramYear)
								}
								BRApi.Database.ExecuteSql(
									dbConn,
									"
									-- Clean previous work_order warnings for this entity and year
									DELETE FROM XFC_RES_AUX_template_warning
									WHERE import_type = 'work_order'
									  AND entity = @paramEntity
									  AND year = @paramYear;

									-- Technology warning: technologies not found in aux table
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT 'work_order', 'work_order', r.entity, '', @paramYear,
										'technology', COALESCE(r.technology, '(empty)'), CONCAT('wo_id=', r.id)
									FROM XFC_RES_RAW_work_order r
									WHERE r.entity = @paramEntity
										AND r.technology NOT IN (SELECT id FROM XFC_RES_AUX_technology)
										AND r.technology IS NOT NULL AND LTRIM(RTRIM(r.technology)) <> '';

									-- Service type warning: serv_type not found in aux table
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT 'work_order', 'work_order', r.entity, '', @paramYear,
										'serv_type', COALESCE(r.serv_type, '(empty)'), CONCAT('wo_id=', r.id)
									FROM XFC_RES_RAW_work_order r
									WHERE r.entity = @paramEntity
										AND r.serv_type NOT IN (SELECT id FROM XFC_RES_AUX_serv_type)
										AND r.serv_type IS NOT NULL AND LTRIM(RTRIM(r.serv_type)) <> '';

									-- Revenue type warning: rev_type not found in aux table
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT 'work_order', 'work_order', r.entity, '', @paramYear,
										'rev_type', COALESCE(r.rev_type, '(empty)'), CONCAT('wo_id=', r.id)
									FROM XFC_RES_RAW_work_order r
									WHERE r.entity = @paramEntity
										AND r.rev_type NOT IN (SELECT id FROM XFC_RES_AUX_rev_type)
										AND r.rev_type IS NOT NULL AND LTRIM(RTRIM(r.rev_type)) <> '';
									",
									warningDbParams,
									False
								)
							End Using
							End If

							'Run population queries for rnd raw project table
							Me.RunPopulationQueries(si, "XFC_RES_RAW_work_order")
							
						#End Region
						
						#Region "Import Project Planning Template"
						
						Else If paramFunction = "ImportProjectPlanningTemplate" Then
							
							'Get parameters
							Dim paramQueryParams As String = args.NameValuePairs("p_query_params")
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, paramQueryParams)
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, paramQueryParams)
							'Add next year to db param infos
							dbParamInfos.Add(New DbParamInfo("paramNextYear", CInt(parameterDict("paramYear")) + 1))
							dbParamInfos.Add(New DbParamInfo("paramP1Year", CInt(parameterDict("paramYear")) + 2))
							dbParamInfos.Add(New DbParamInfo("paramP2Year", CInt(parameterDict("paramYear")) + 3))
							dbParamInfos.Add(New DbParamInfo("paramP3Year", CInt(parameterDict("paramYear")) + 4))
							
							'Get file names in the imported data capital folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Services/Imported Data/Project Planning"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							If fileNames.Count = 0 Then Throw New Exception("Error importing template: template file not found")
							
							Try
								
								'Loop through the folder files to populate the raw data table (it must be just one)
								For Each fileName As NameAndAccessLevel In fileNames
									'Check that the file has the correct name
									If Not fileName.Name.Contains("Project_Planning_Template") Then
										'Clear folder
										UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
										Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check if the name contains 'Project_Planning_Template'."))
									End If
									'Check correct file for entity, year, scenario and technology
									For Each check In {"paramEntity", "paramYear", "paramScenario", "paramTechnology"}
										If Not fileName.Name.Contains(parameterDict(check)) Then
											'Clear folder
											UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
											Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check if the name contains '{parameterDict(check)}'."))
										End If
									Next
									
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
									
									'Add today's date to the end of the file name
									Dim today As Date = Date.Now()
									Dim copyFileName As String = fileName.Name.Split("/").Last().Replace(".xlsx", $" {today.Year}-{today.Month}-{today.Day}.xlsx")
									
									'Copy file to processed folder
									UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, copyFileName, targetPath)
									
									'Clear folder and exit the loop
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Exit For
								Next
							Catch ex As Exception
								'Clear folder and exit throw error
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
							End Try
							
							'Run warning validations and population query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								' Insert template warnings
								BRApi.Database.ExecuteSql(
									dbConn,
									"
									DECLARE @templateType VARCHAR(10) = CASE WHEN @paramScenario = 'BUD' THEN 'annual' ELSE 'monthly' END;

									-- Clean previous warnings for this entity/scenario/year/template_type
									DELETE FROM XFC_RES_AUX_template_warning
									WHERE entity = @paramEntity
										AND scenario = @paramScenario
										AND year = @paramYear
										AND import_type = 'project'
										AND template_type = @templateType;

									-- 6a. Account warning: accounts not found in dimension (exclude OE_ virtual accounts)
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'project', @paramEntity, @paramScenario, @paramYear,
										'account', COALESCE(wof.account, '(empty)'), CONCAT('wo_id=', wof.work_order_id, ': Not uploaded')
									FROM XFC_RES_RAW_work_order_figures wof
									WHERE wof.account NOT IN (SELECT MemberName FROM XFC_RES_DIM_Account)
										AND wof.account NOT LIKE 'OE_%'
										AND wof.account IS NOT NULL AND LTRIM(RTRIM(wof.account)) <> '';

									-- 6b. Intercompany warning: empty intercompany
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'project', @paramEntity, @paramScenario, @paramYear,
										'intercompany', COALESCE(NULLIF(LTRIM(RTRIM(wof.intercompany)), ''), '(empty)'),
										CONCAT('wo_id=', wof.work_order_id, ' account=', wof.account, ': Updated to None')
									FROM XFC_RES_RAW_work_order_figures wof
									WHERE (wof.intercompany IS NULL OR LTRIM(RTRIM(wof.intercompany)) = '')
										AND (wof.account IN (SELECT MemberName FROM XFC_RES_DIM_Account) OR wof.account LIKE 'OE_%')
										AND EXISTS (
											SELECT 1 FROM XFC_RES_MASTER_work_order wo
											WHERE wo.id = wof.work_order_id AND wo.entity = wof.entity
										);

									-- 6d. Work order warning: work orders not in master
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'project', @paramEntity, @paramScenario, @paramYear,
										'work_order', COALESCE(wof.work_order_id, '(empty)'), CONCAT('entity=', wof.entity, ': Not uploaded')
									FROM XFC_RES_RAW_work_order_figures wof
									WHERE NOT EXISTS (
										SELECT 1 FROM XFC_RES_MASTER_work_order wo
										WHERE wo.id = wof.work_order_id AND wo.entity = wof.entity
									);

									-- 6e. Technology warning: work order technology not in aux table (row not uploaded)
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'project', @paramEntity, @paramScenario, @paramYear,
										'technology', COALESCE(wo.technology, '(empty)'), CONCAT('wo_id=', wof.work_order_id, ': Not uploaded')
									FROM XFC_RES_RAW_work_order_figures wof
									INNER JOIN XFC_RES_MASTER_work_order wo ON wof.work_order_id = wo.id AND wof.entity = wo.entity
									WHERE wo.technology NOT IN (SELECT id FROM XFC_RES_AUX_technology)
										AND wo.technology IS NOT NULL AND LTRIM(RTRIM(wo.technology)) <> '';
									",
									dbParamInfos,
									False
								)

								' Check if year is confirmed
								Dim isConfirmedDt As DataTable = BRApi.Database.ExecuteSql(
									dbConn,
									"
										SELECT is_confirmed
										FROM XFC_RES_AUX_year_scenario_confirm
										WHERE  (
												(@paramScenario = 'BUD' AND year = @paramNextYear)
												OR (@paramScenario <> 'BUD' AND year = @paramYear)
											) AND scenario = @paramScenario
											AND is_confirmed = 1
									",
									dbParamInfos,
									False
								)
								If isConfirmedDt IsNot Nothing AndAlso isConfirmedDt.Rows.Count > 0 Then
									Throw New Exception("Could not import template. Year is confirmed.")
								End If
								BRApi.Database.ExecuteSql(
									dbConn,
									"
									DECLARE @refScenario VARCHAR(50);
									DECLARE @firstForecastedMonth INTEGER;

									IF @paramScenario = 'BUD'
									BEGIN
									    SELECT TOP 1 @refScenario = ref_scenario
									    FROM XFC_RES_RAW_work_order_figures;
									END
									ELSE
									BEGIN
									    SET @refScenario = @paramScenario;
									END

									SET @firstForecastedMonth = CONVERT(INTEGER, RIGHT(@refScenario, 2)) + 1;

									DELETE wof
									FROM XFC_RES_FACT_work_order_figures wof
									LEFT JOIN XFC_RES_MASTER_work_order wo
									ON wof.wo_id = wo.id
										AND wof.entity = wo.entity
									WHERE wof.entity = @paramEntity
										AND (
												(
													@paramScenario = 'BUD'
													AND (
														(wof.scenario = 'BUD' AND wof.year = @paramNextYear)
														OR (wof.scenario = 'PLN1' AND wof.year = @paramP1Year)
														OR (wof.scenario = 'PLN2' AND wof.year = @paramP2Year)
														OR (wof.scenario = 'PLN3' AND wof.year = @paramP3Year)
													)
												)
											OR (
												wof.scenario = @refScenario
												AND wof.year = @paramYear
												AND wof.month >= @firstForecastedMonth
											)
										) AND (
							                @paramTechnology = 'All'
							                OR wo.id IS NULL
							                OR (@paramTechnology <> 'Other' AND wo.technology LIKE @paramTechnology)
							                OR (@paramTechnology = 'Other' AND wo.technology NOT LIKE 'AM%' AND wo.technology NOT LIKE 'OM%')
							            ) AND NOT EXISTS (
											SELECT 1
											FROM XFC_RES_AUX_year_scenario_confirm ysc
											WHERE ysc.year = wof.year
												AND ysc.scenario = wof.scenario
												AND ysc.is_confirmed = 1
										);
									
									INSERT INTO XFC_RES_FACT_work_order_figures (
									    entity,
									    wo_id,
									    scenario,
										year,
									    month,
									    account,
									    intercompany,
										ud3,
									    amount
									
									)
									SELECT 
									    entity,
									    wo_id,
									    scenario,
										year,
									    month,
									    account,
									    intercompany,
										ud3,
									    SUM(amount) AS amount
									FROM (
										SELECT 
										    wof.entity,
										    wof.work_order_id AS wo_id,
										    CASE
												WHEN v.scenario = 'ACT' THEN @paramScenario
												ELSE v.scenario
											END AS scenario,
											CASE
												WHEN v.scenario = 'BUD' THEN @paramNextYear
												WHEN v.scenario = 'PLN1' THEN @paramP1Year
												WHEN v.scenario = 'PLN2' THEN @paramP2Year
												WHEN v.scenario = 'PLN3' THEN @paramP3Year
												ELSE @paramYear
											END AS year,
										    v.month,
											wof.account,
										    COALESCE(NULLIF(LTRIM(RTRIM(wof.intercompany)), ''), 'None') AS intercompany,
											'COGS' AS ud3,
										    v.amount
											-- añadido para que evite refScenario vacío por error. ANTES: FROM XFC_RES_RAW_work_order_figures wof
											FROM (
												SELECT *
												FROM XFC_RES_RAW_work_order_figures
												WHERE ref_scenario IS NOT NULL AND LTRIM(RTRIM(ref_scenario)) <> ''
											) wof
											INNER JOIN XFC_RES_MASTER_work_order wo
												ON wof.work_order_id = wo.id 
												AND wof.entity = wo.entity									
										CROSS APPLY (
										    VALUES
										        (wof.ref_scenario,  1, wof.m1),  (wof.ref_scenario,  2, wof.m2),  (wof.ref_scenario,  3, wof.m3),
												(wof.ref_scenario,  4, wof.m4), (wof.ref_scenario,  5, wof.m5),  (wof.ref_scenario,  6, wof.m6),
												(wof.ref_scenario,  7, wof.m7),  (wof.ref_scenario,  8, wof.m8), (wof.ref_scenario,  9, wof.m9),
												(wof.ref_scenario, 10, wof.m10), (wof.ref_scenario, 11, wof.m11), (wof.ref_scenario, 12, wof.m12),
										
										        ('BUD',  1, wof.bm1), ('BUD',  2, wof.bm2), ('BUD',  3, wof.bm3), ('BUD',  4, wof.bm4),
										        ('BUD',  5, wof.bm5), ('BUD',  6, wof.bm6), ('BUD',  7, wof.bm7), ('BUD',  8, wof.bm8),
										        ('BUD',  9, wof.bm9), ('BUD', 10, wof.bm10), ('BUD', 11, wof.bm11), ('BUD', 12, wof.bm12),
										
												('PLN1', 1, wof.p1m1), ('PLN1', 2, wof.p1m2), ('PLN1', 3, wof.p1m3), ('PLN1', 4, wof.p1m4),
												('PLN1', 5, wof.p1m5), ('PLN1', 6, wof.p1m6), ('PLN1', 7, wof.p1m7), ('PLN1', 8, wof.p1m8),
												('PLN1', 9, wof.p1m9), ('PLN1', 10, wof.p1m10), ('PLN1', 11, wof.p1m11), ('PLN1', 12, wof.p1m12),
												
												('PLN2', 1, wof.p2m1), ('PLN2', 2, wof.p2m2), ('PLN2', 3, wof.p2m3), ('PLN2', 4, wof.p2m4),
												('PLN2', 5, wof.p2m5), ('PLN2', 6, wof.p2m6), ('PLN2', 7, wof.p2m7), ('PLN2', 8, wof.p2m8),
												('PLN2', 9, wof.p2m9), ('PLN2', 10, wof.p2m10), ('PLN2', 11, wof.p2m11), ('PLN2', 12, wof.p2m12),

										        ('PLN3', 12, wof.p3)
									
										) v(scenario, month, amount)
										WHERE v.amount IS NOT NULL AND v.amount <> 0
											AND wof.entity = @paramEntity
											AND (
												@paramScenario = 'BUD'
												OR v.scenario IN (@paramScenario, 'ACT')
											) AND (
								                @paramTechnology = 'All'
								                OR (@paramTechnology <> 'Other' AND wo.technology LIKE @paramTechnology)
								                OR (@paramTechnology = 'Other' AND wo.technology NOT LIKE 'AM%' AND wo.technology NOT LIKE 'OM%')
								            )
													AND wof.account IN (SELECT DISTINCT MemberName FROM XFC_RES_DIM_Account)
									) AS subquery
									WHERE NOT (
										scenario = @refScenario
										AND month < @firstForecastedMonth
									) AND NOT EXISTS (
										SELECT 1
										FROM XFC_RES_AUX_year_scenario_confirm ysc
										WHERE ysc.year = subquery.year
											AND ysc.scenario = subquery.scenario
											AND ysc.is_confirmed = 1
									)
									GROUP BY
									    entity,
									    wo_id,
									    scenario,
										year,
									    month,
									    account,
									    intercompany,
										ud3;
									",
									dbParamInfos,
									False
								)
							End Using
							
						#End Region
						
						#Region "Import Structure Planning Template"
						
						Else If paramFunction = "ImportStructurePlanningTemplate" Then
							
							'Get parameters
							Dim paramQueryParams As String = args.NameValuePairs("p_query_params")
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, paramQueryParams)
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, paramQueryParams)
							'Add next year to db param infos
							dbParamInfos.Add(New DbParamInfo("paramNextYear", CInt(parameterDict("paramYear")) + 1))
							dbParamInfos.Add(New DbParamInfo("paramP1Year", CInt(parameterDict("paramYear")) + 2))
							dbParamInfos.Add(New DbParamInfo("paramP2Year", CInt(parameterDict("paramYear")) + 3))
							
							'Get file names in the imported data capital folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Services/Imported Data/Structure Planning"
							Dim targetFinalPath As String = "Processed"
							Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
							Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
								si,
								FileSystemLocation.ApplicationDatabase,
								sourcePath,
								XFFileType.All,
								False, False, False
							)
							
							If fileNames.Count = 0 Then Throw New Exception("Error importing template: template file not found")
							
							Try
								
								'Loop through the folder files to populate the raw data table (it must be just one)
								For Each fileName As NameAndAccessLevel In fileNames
									'Check that the file has the correct name
									If Not fileName.Name.Contains("Structure_Planning_Template") Then
										'Clear folder
										UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
										Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check if the name contains 'Project_Planning_Template'."))
									End If
									'Check correct file for entity, year, scenario and technology
									For Each check In {"paramEntity", "paramYear", "paramScenario", "paramDepartment"}
										If Not fileName.Name.Contains(parameterDict(check)) Then
											'Clear folder
											UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
											Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check if the name contains '{parameterDict(check)}'."))
										End If
									Next
									
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
									
									'Add today's date to the end of the file name
									Dim today As Date = Date.Now()
									Dim copyFileName As String = fileName.Name.Split("/").Last().Replace(".xlsx", $" {today.Year}-{today.Month}-{today.Day}.xlsx")
									
									'Copy file to processed folder
									UTISharedFunctionsBR.CopyXLSXFile(si, fileName.Name.Split("/").Last(), sourcePath, copyFileName, targetPath)
									
									'Clear folder and exit the loop
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Exit For
								Next
							Catch ex As Exception
								'Clear folder and exit throw error
								UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
								Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
							End Try
							
							'Run warning validations and population query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								' Insert template warnings
								BRApi.Database.ExecuteSql(
									dbConn,
									"
									DECLARE @templateType VARCHAR(10) = CASE WHEN @paramScenario = 'BUD' THEN 'annual' ELSE 'monthly' END;

									-- Clean previous warnings for this entity/scenario/year/template_type
									DELETE FROM XFC_RES_AUX_template_warning
									WHERE entity = @paramEntity
										AND scenario = @paramScenario
										AND year = @paramYear
										AND import_type = 'structure'
										AND template_type = @templateType;

									-- 6a. Account warning: accounts not found in dimension
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'structure', @paramEntity, @paramScenario, @paramYear,
										'account', COALESCE(sf.account, '(empty)'), CONCAT('ud3=', sf.ud3, ' ud1=', sf.ud1, ': Not uploaded')
									FROM XFC_RES_RAW_structure_figures sf
									WHERE sf.account NOT IN (SELECT MemberName FROM XFC_RES_DIM_Account)
										AND sf.account IS NOT NULL AND LTRIM(RTRIM(sf.account)) <> '';

									-- 6b. Intercompany warning: empty intercompany
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'structure', @paramEntity, @paramScenario, @paramYear,
										'intercompany', COALESCE(NULLIF(LTRIM(RTRIM(sf.intercompany)), ''), '(empty)'),
										CONCAT('ud3=', sf.ud3, ' account=', sf.account, ': Updated to None')
									FROM XFC_RES_RAW_structure_figures sf
									WHERE (sf.intercompany IS NULL OR LTRIM(RTRIM(sf.intercompany)) = '')
										AND sf.account IN (SELECT MemberName FROM XFC_RES_DIM_Account)
										AND (sf.ud3 IS NULL OR LTRIM(RTRIM(sf.ud3)) = '' OR sf.ud3 IN (SELECT cc FROM XFC_RES_AUX_department));

									-- 6c. Cost center warning: ud3 not in department table
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'structure', @paramEntity, @paramScenario, @paramYear,
										'cost_center', COALESCE(sf.ud3, '(empty)'), CONCAT('account=', sf.account, ': Not uploaded')
									FROM XFC_RES_RAW_structure_figures sf
									WHERE sf.ud3 NOT IN (SELECT cc FROM XFC_RES_AUX_department)
										AND sf.ud3 IS NOT NULL AND LTRIM(RTRIM(sf.ud3)) <> '';

									-- 6c2. Cost center warning: empty ud3 (converted to None)
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'structure', @paramEntity, @paramScenario, @paramYear,
										'cost_center', '(empty)', CONCAT('account=', sf.account, ': Updated to None')
									FROM XFC_RES_RAW_structure_figures sf
									WHERE (sf.ud3 IS NULL OR LTRIM(RTRIM(sf.ud3)) = '')
										AND sf.account IN (SELECT MemberName FROM XFC_RES_DIM_Account);

									-- 6d. Technology warning: ud1 not in technology table (row not uploaded)
									INSERT INTO XFC_RES_AUX_template_warning
										(template_type, import_type, entity, scenario, year, warning_type, source_value, row_detail)
									SELECT DISTINCT @templateType, 'structure', @paramEntity, @paramScenario, @paramYear,
										'technology', COALESCE(sf.ud1, '(empty)'), CONCAT('ud3=', sf.ud3, ' account=', sf.account, ': Not uploaded')
									FROM XFC_RES_RAW_structure_figures sf
									WHERE sf.ud1 NOT IN (SELECT id FROM XFC_RES_AUX_technology)
										AND sf.ud1 IS NOT NULL AND LTRIM(RTRIM(sf.ud1)) <> '';
									",
									dbParamInfos,
									False
								)

								' Check if year is confirmed
								Dim isConfirmedDt As DataTable = BRApi.Database.ExecuteSql(
									dbConn,
									"
										SELECT is_confirmed
										FROM XFC_RES_AUX_year_scenario_confirm
										WHERE  (
												(@paramScenario = 'BUD' AND year = @paramNextYear)
												OR (@paramScenario <> 'BUD' AND year = @paramYear)
											) AND scenario = @paramScenario
											AND is_confirmed = 1
									",
									dbParamInfos,
									False
								)
								If isConfirmedDt IsNot Nothing AndAlso isConfirmedDt.Rows.Count > 0 Then
									Throw New Exception("Could not import template. Year is confirmed.")
								End If
								BRApi.Database.ExecuteSql(
									dbConn,
									"
									DECLARE @refScenario VARCHAR(50);
									DECLARE @firstForecastedMonth INTEGER;

									IF @paramScenario = 'BUD'
									BEGIN
									    SELECT TOP 1 @refScenario = ref_scenario
									    FROM XFC_RES_RAW_structure_figures;
									END
									ELSE
									BEGIN
									    SET @refScenario = @paramScenario;
									END
									
									SET @firstForecastedMonth = CONVERT(INTEGER, RIGHT(@refScenario, 2)) + 1;
									
									DELETE sf
									FROM XFC_RES_FACT_structure_figures sf
									LEFT JOIN XFC_RES_AUX_department d ON sf.ud3 = d.cc
									WHERE entity = @paramEntity
										AND (
												(
													@paramScenario = 'BUD'
													AND (
														(scenario = 'BUD' AND year = @paramNextYear)
														OR (scenario = 'PLN1' AND year = @paramP1Year)
														OR (scenario = 'PLN2' AND year = @paramP2Year)
													)
												)
											OR (
												scenario = @refScenario
												AND year = @paramYear
												AND month >= @firstForecastedMonth
											)
										) AND (
							                @paramDepartment = 'All'
							                OR d.department_description = @paramDepartment
							                OR d.cc IS NULL
							            ) AND NOT EXISTS (
											SELECT 1
											FROM XFC_RES_AUX_year_scenario_confirm ysc
											WHERE ysc.year = sf.year
												AND ysc.scenario = sf.scenario
												AND ysc.is_confirmed = 1
										);
									
									INSERT INTO XFC_RES_FACT_structure_figures (
									    entity,
									    ud1,
									    scenario,
										year,
									    month,
									    account,
									    intercompany,
										ud3,
									    amount
									)
									SELECT 
									    entity,
									    ud1,
									    scenario,
										year,
									    month,
									    account,
									    intercompany,
										ud3,
									    SUM(amount) AS amount
									FROM (
										SELECT 
										    entity,
										    ud1,
										    CASE
												WHEN v.scenario = 'ACT' THEN @paramScenario
												ELSE v.scenario
											END AS scenario,
											CASE
												WHEN v.scenario = 'BUD' THEN @paramNextYear
												WHEN v.scenario = 'PLN1' THEN @paramP1Year
												WHEN v.scenario = 'PLN2' THEN @paramP2Year
												ELSE @paramYear
											END AS year,
										    v.month,
											account,
										    COALESCE(NULLIF(LTRIM(RTRIM(intercompany)), ''), 'None') AS intercompany,
											COALESCE(NULLIF(LTRIM(RTRIM(ud3)), ''), 'None') AS ud3,
										    v.amount
										-- añadido para que evite refScenario vacío por error. ANTES:FROM XFC_RES_RAW_structure_figures sf
										FROM (
											SELECT *
											FROM XFC_RES_RAW_structure_figures
											WHERE ref_scenario IS NOT NULL AND LTRIM(RTRIM(ref_scenario)) <> ''
										) sf									
										CROSS APPLY (
										    VALUES
										        (sf.ref_scenario,  1, sf.m1),  (sf.ref_scenario,  2, sf.m2),  (sf.ref_scenario,  3, sf.m3),
												(sf.ref_scenario,  4, sf.m4), (sf.ref_scenario,  5, sf.m5),  (sf.ref_scenario,  6, sf.m6),
												(sf.ref_scenario,  7, sf.m7),  (sf.ref_scenario,  8, sf.m8), (sf.ref_scenario,  9, sf.m9),
												(sf.ref_scenario, 10, sf.m10), (sf.ref_scenario, 11, sf.m11), (sf.ref_scenario, 12, sf.m12),
										
										        ('BUD',  1, sf.bm1),  ('BUD',  2, sf.bm2),  ('BUD',  3, sf.bm3),
										        ('BUD',  4, sf.bm4),  ('BUD',  5, sf.bm5),  ('BUD',  6, sf.bm6),
										        ('BUD',  7, sf.bm7),  ('BUD',  8, sf.bm8),  ('BUD',  9, sf.bm9),
										        ('BUD', 10, sf.bm10), ('BUD', 11, sf.bm11), ('BUD', 12, sf.bm12),
																				
										        ('PLN1',  1, sf.p1m1),  ('PLN1',  2, sf.p1m2),  ('PLN1',  3, sf.p1m3),
										        ('PLN1',  4, sf.p1m4),  ('PLN1',  5, sf.p1m5),  ('PLN1',  6, sf.p1m6),
										        ('PLN1',  7, sf.p1m7),  ('PLN1',  8, sf.p1m8),  ('PLN1',  9, sf.p1m9),
										        ('PLN1', 10, sf.p1m10), ('PLN1', 11, sf.p1m11), ('PLN1', 12, sf.p1m12),
										        ('PLN2',  1, sf.p2m1),  ('PLN2',  2, sf.p2m2),  ('PLN2',  3, sf.p2m3),
										        ('PLN2',  4, sf.p2m4),  ('PLN2',  5, sf.p2m5),  ('PLN2',  6, sf.p2m6),
										        ('PLN2',  7, sf.p2m7),  ('PLN2',  8, sf.p2m8),  ('PLN2',  9, sf.p2m9),
										        ('PLN2', 10, sf.p2m10), ('PLN2', 11, sf.p2m11), ('PLN2', 12, sf.p2m12)
									
										) v(scenario, month, amount)
										LEFT JOIN XFC_RES_AUX_department d ON sf.ud3 = d.cc
										WHERE v.amount IS NOT NULL AND v.amount <> 0
											AND entity = @paramEntity
											AND (
												@paramScenario = 'BUD'
												OR v.scenario IN (@paramScenario, 'ACT')
											) AND (
								                @paramDepartment = 'All'
							                	OR d.department_description = @paramDepartment
							                	OR ud3 IS NULL OR LTRIM(RTRIM(ud3)) = ''
								            ) AND (
								            	ud3 IS NULL OR LTRIM(RTRIM(ud3)) = '' OR d.cc IS NOT NULL
								            ) AND account IN (SELECT DISTINCT MemberName FROM XFC_RES_DIM_Account)
											AND (sf.ud1 IS NULL OR LTRIM(RTRIM(sf.ud1)) = '' OR sf.ud1 IN (SELECT id FROM XFC_RES_AUX_technology))
									) AS subquery
									WHERE NOT (
										scenario = @refScenario
										AND month < @firstForecastedMonth
									) AND NOT EXISTS (
										SELECT 1
										FROM XFC_RES_AUX_year_scenario_confirm ysc
										WHERE ysc.year = subquery.year
											AND ysc.scenario = subquery.scenario
											AND ysc.is_confirmed = 1
									)
									GROUP BY
									    entity,
									    ud1,
									    scenario,
										year,
									    month,
									    account,
									    intercompany,
										ud3;
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
			If tableName = "XFC_RES_RAW_work_order" Then
				tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.work_order)
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
