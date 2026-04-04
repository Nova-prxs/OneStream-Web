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
						
						#Region "Import Projects"
						
						If paramFunction = "ImportProjects" Then
							'Get parameters
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Get file names in the imported data capital folder and build path to save the processed files
							Dim sourcePath As String = "Documents/Public/Investments/Imported Data/RnD/Projects"
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
								If Not fileName.Name.Contains("Projects") Then
									'Clear folder
									UTISharedFunctionsBR.DeleteAllFilesInFolder(si, sourcePath, FileSystemLocation.ApplicationDatabase)
									Throw ErrorHandler.LogWrite(si, New XFException($"Uploaded file is not correct. Check that the name begins with 'Projects'."))
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
							
							'Run population queries for rnd raw project table
							Me.RunPopulationQueries(si, "XFC_INV_RAW_project")
							
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
			If tableName = "XFC_INV_RAW_project" Then
				'tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.project)
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
