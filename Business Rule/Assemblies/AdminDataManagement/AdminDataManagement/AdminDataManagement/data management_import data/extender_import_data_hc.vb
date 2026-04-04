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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_data_hc
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep

						Dim tableName As String = args.NameValuePairs("p_table")
						Dim method As String = args.NameValuePairs("p_method")
						Dim filesType As String = args.NameValuePairs("p_files_type")
						Dim delimiter As String = String.Empty
						If filesType.ToLower = "delimited" Then delimiter = args.NameValuePairs("p_delimiter")
						Dim location = FileSystemLocation.ApplicationDatabase
						Dim sourcePath As String = args.NameValuePairs("p_folder")
						Dim targetFinalPath As String = "Actual"
						Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
						Dim columnMappingDict As New Dictionary(Of String, String)
						Dim fileName = args.NameValuePairs("p_file_name")
						Dim isFileInFolder As Boolean = False
						Dim fileFullName As String = String.Empty						
						
						'Get file names in the delimited files folder
						Dim folderFileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(si, location, sourcePath, XFFileType.All, False, False, False)
						
						'Handle number of files
						If folderFileNames.Count < 1 Then
							Throw New Exception($"No files found on Path: '{sourcePath}'")
						End If						
						
						Try
						
						For Each folderFileName As NameAndAccessLevel In folderFileNames
							If Not folderFileName.Name.ToLower.Contains(fileName.ToLower) Then Continue For
												
								Dim fieldTokens As New List(Of String)
							
								fieldTokens.Add("xfText#:[employee_id]::-")		
								fieldTokens.Add("xfText#:[worker_name]::-")	
								fieldTokens.Add("xfText#:[HR_cluster]::-")	
								fieldTokens.Add("xfText#:[location_country]::-")	
								fieldTokens.Add("xfText#:[company_code]::-")	
								fieldTokens.Add("xfText#:[company]::-")	
								fieldTokens.Add("xfText#:[location]::-")	
								fieldTokens.Add("xfText#:[costcenter_id]::-")	
								fieldTokens.Add("xfText#:[shared_services_center]::-")									
								fieldTokens.Add("xfText#:[gender]::-")	
								fieldTokens.Add("xfText#:[job_category]::-")	
								fieldTokens.Add("xfText#:[global_function_organization]::-")	
								fieldTokens.Add("xfText#:[sup_org_from_top_1]::-")
								fieldTokens.Add("xfText#:[sup_org_from_top_2]::-")
								fieldTokens.Add("xfText#:[sup_org_from_top_3]::-")
								fieldTokens.Add("xfText#:[sup_org_from_top_4]::-")
								fieldTokens.Add("xfText#:[sup_org_from_top_5]::-")
								fieldTokens.Add("xfText#:[sup_org_from_top_6]::-")							
								fieldTokens.Add("xfInt#:[headcount]::0")	
								
								'Dim newContentBytes As Byte() = BRApi.FileSystem.GetFile(si,location,folderFileName.Name,True, True).XFFile.ContentFileBytes
								
								Dim filePath As String = $"{folderFileName.Name}"
								Dim fileNameN As String = folderFileName.Name.Split("/").Last()
								Dim year As String = fileNameN.Substring(0,4)
								Dim month As String = fileNameN.Substring(5,2)
								
'								BRApi.ErrorLog.LogMessage(si, "year: " & year)
'								BRApi.ErrorLog.LogMessage(si, "month: " & month)
'								BRApi.ErrorLog.LogMessage(si, "filePath: " & filePath)
'								BRApi.ErrorLog.LogMessage(si, "fileNameN: " & fileNameN)
								
								Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase,filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
									
'								BRApi.ErrorLog.LogMessage(si, "1")
								
								'1. Insert RAW TABLE 
								Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", "Application", tableName, "Replace", fieldTokens, True)
								
'								BRApi.ErrorLog.LogMessage(si, "2")
								
'								BRApi.ErrorLog.LogMessage(si, dt.Columns(0).ToString)

								isFileInFolder = True
								fileFullName = folderFileName.Name
								fileName = folderFileName.Name.Split("/").Last()

								'2. Insert FACT TABLE
								Me.InsertFactHeadcount(si, tableName, targetPath, fileName, year, month)
								
'								BRApi.ErrorLog.LogMessage(si, "3")
						Next
						
						If Not isFileInFolder Then Throw New Exception("File for the import not found, make sure that you uploaded the correct file." & vbCrLf &
							$"Name must contain '{fileName}'.")						
							

						'3. Move processed file to FOLDER ACTUAL
						BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
						UTISharedFunctionsBR.CopyFile(si, location, fileName, sourcePath, fileName, targetPath)
						BRApi.FileSystem.DeleteFile(si, location, fileFullName)
						
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						selectionChangedTaskResult.ShowMessageBox = True
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.Message = "Headcount file uploaded succesfully."
						
						Catch ex As Exception
							BRApi.FileSystem.DeleteFile(si, location, fileFullName)
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try						
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function				
		
		#Region "Insert Fact Headcount"
		
		Private Sub InsertFactHeadcount(ByVal si As SessionInfo, ByVal tableName As String, ByVal folderPath As String, ByVal fileName As String, ByVal year As String, ByVal month As String)

			Dim sql As String = 
			$"
				DELETE FROM XFC_MAIN_FACT_Headcount
				WHERE year = {year}
				AND month = {month}
				AND scenario = 'Actual';
		
			
				INSERT INTO XFC_MAIN_FACT_Headcount (
					scenario,year,month,company_id,company_desc,employee_id,employee_name,costcenter_id,job_category,location,location_country,gender,HR_cluster,shared_services_center,global_function_organization,sup_org_from_top_1,sup_org_from_top_2,sup_org_from_top_3,sup_org_from_top_4,sup_org_from_top_5,sup_org_from_top_6,headcount
				)
				SELECT 'Actual',{year},{month},company_code,company,employee_id,worker_name,costcenter_id,job_category,location,location_country,gender,HR_cluster,shared_services_center,global_function_organization,sup_org_from_top_1,sup_org_from_top_2,sup_org_from_top_3,sup_org_from_top_4,sup_org_from_top_5,sup_org_from_top_6,headcount				
				FROM XFC_MAIN_RAW_Headcount
			"

			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)

				BRApi.ErrorLog.LogMessage(si,sql)
				BRApi.Database.ExecuteSql(dbConn, sql, False)
				
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramTable", tableName),
					New DbParamInfo("paramUsername", si.UserName),
					New DbParamInfo("paramFolderPath", folderPath),
					New DbParamInfo("paramFileName", fileName)
				}
				BRApi.Database.ExecuteSql(
					dbConn,
					"
					MERGE INTO XFC_MAIN_MASTER_TableImportInfo AS target
					USING (
					    SELECT
					        @paramTable AS table_name,
					        @paramUsername AS username,
							@paramFolderPath AS folder_path,
							@paramFileName AS file_name,
					        GETUTCDATE() AS last_import_date
					) AS source
					ON target.table_name = source.table_name
					WHEN MATCHED THEN
					    UPDATE SET
					        username = source.username,
					        last_import_date = source.last_import_date,
							folder_path = source.folder_path,
							file_name = source.file_name
					WHEN NOT MATCHED THEN
					    INSERT (table_name, username, last_import_date, folder_path, file_name)
					    VALUES (source.table_name, source.username, source.last_import_date, source.folder_path, source.file_name);
					",
					dbParamInfos,
					False
				)
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
	
	End Class
	
End Namespace
