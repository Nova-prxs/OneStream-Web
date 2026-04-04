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
		
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
		'Reference migrations class
		Dim MigrationsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.Migrations

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get function name
						Dim paramFunction As String = args.NameValuePairs("p_function")
						
						'Control function name
						
						#Region "Import Cash Debt Position"
						
						If paramFunction = "ImportCashDebtPosition" Then
							Me.ProcessTemplateImport(si, "CashDebtPosition", args.NameValuePairs("p_query_params"))
							
					#End Region
						
						ElseIf paramFunction = "ImportCashFlowForecasting" Then
						
						#Region "Import CashFlow Forecasting"
						
							Me.ProcessTemplateImport(si, "CashFlowForecasting", args.NameValuePairs("p_query_params"))
							
					#End Region
						
						End If

					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Template Processing - Centralized & Optimized"
		
		''' <summary>
		''' Procesa la importación de un template de Treasury de forma genérica.
		''' Maneja validación, carga de datos, archivado y población de tablas maestras.
		''' </summary>
		''' <param name="si">Información de sesión de OneStream</param>
		''' <param name="templateType">Tipo de template (CashDebtPosition, CashFlowForecasting, etc.)</param>
		''' <param name="queryParams">String con parámetros en formato query: p_year=2025,p_week=03,p_company=ACME</param>
		Private Sub ProcessTemplateImport(
			ByVal si As SessionInfo,
			ByVal templateType As String,
			ByVal queryParams As String
		)
			'Get parameters from query string
			Dim parameterDict As Dictionary(Of String, String) = Me.SplitQueryParams(queryParams)
			
			'Define paths
			Dim sourcePath As String = "Documents/Public/Treasury/Imported Data/Monitoring"
			Dim targetFinalPath As String = "Processed"
			Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
			
			'Get file names in the imported data treasury folder
			Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
				si,
				FileSystemLocation.ApplicationDatabase,
				sourcePath,
				XFFileType.All,
				False, False, False
			)
			
			'Validate template file (throws exception if validation fails)
			Dim validatedFile As NameAndAccessLevel = Me.ValidateAndGetTemplateFile(
				si,
				fileNames,
				templateType,
				parameterDict,
				sourcePath
			)
			
			'Get file bytes
			Dim fileBytes As Byte() = BRApi.FileSystem.GetFile(
				si,
				FileSystemLocation.ApplicationDatabase,
				validatedFile.Name,
				True, True
			).XFFile.ContentFileBytes
			
			'Load excel template into custom tables
			Try
				BRApi.Utilities.LoadCustomTableUsingExcel(
					si,
					SourceDataOriginTypes.FromFileUpload,
					validatedFile.Name,
					fileBytes
				)
			Catch ex As Exception
				'Clear folder and throw error
				Me.DeleteAllFilesInFolder(si, sourcePath)
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
			'Create processed folder if necessary
			BRApi.FileSystem.CreateFullFolderPathIfNecessary(
				si,
				FileSystemLocation.ApplicationDatabase,
				sourcePath,
				targetFinalPath
			)
			
			'Copy file to processed folder for archiving
			Me.CopyXLSXFile(
				si,
				validatedFile.Name.Split("/").Last(),
				sourcePath,
				validatedFile.Name.Split("/").Last(),
				targetPath
			)
			
			'Clear folder after successful processing
			Me.DeleteAllFilesInFolder(si, sourcePath)
			
			'Populate master tables from raw data
			Try
				Me.RunPopulationQueries(si, "")
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Sub
		
		''' <summary>
		''' Valida el archivo de template subido y retorna el archivo si es válido.
		''' Lanza XFException con mensaje claro si la validación falla.
		''' </summary>
		''' <param name="si">Información de sesión de OneStream</param>
		''' <param name="fileNames">Lista de archivos en la carpeta de importación</param>
		''' <param name="templateType">Tipo de template (CashDebtPosition, CashFlowForecasting, etc.)</param>
		''' <param name="parameterDict">Diccionario con parámetros: p_year, p_week, p_company</param>
		''' <param name="sourcePath">Ruta de la carpeta fuente</param>
		''' <returns>El archivo validado listo para procesar</returns>
		Private Function ValidateAndGetTemplateFile(
			ByVal si As SessionInfo,
			ByVal fileNames As List(Of NameAndAccessLevel),
			ByVal templateType As String,
			ByVal parameterDict As Dictionary(Of String, String),
			ByVal sourcePath As String
		) As NameAndAccessLevel
			
			' Get parameters for validation
			Dim expectedYear As String = parameterDict("p_year")
			Dim expectedWeek As String = parameterDict("p_week")
			Dim expectedCompany As String = parameterDict("p_company")
			
			' Validación 0: Verificar que la semana NO esté confirmada
			Me.ValidateWeekNotConfirmed(si, templateType, expectedCompany, expectedWeek, expectedYear, sourcePath)
			
			' Validación 1: Verificar que existe exactamente UN archivo
			If fileNames Is Nothing OrElse fileNames.Count = 0 Then
				Throw ErrorHandler.LogWrite(si, New XFException(
					$"No file found in '{sourcePath}'." & vbCrLf &
					"Please upload a template file before importing."
				))
			End If
			
			If fileNames.Count > 1 Then
				' Limpiar carpeta antes de lanzar error
				Me.DeleteAllFilesInFolder(si, sourcePath)
				Throw ErrorHandler.LogWrite(si, New XFException(
					$"Multiple files found ({fileNames.Count} files)." & vbCrLf &
					"Please upload only ONE template file at a time."
				))
			End If
			
			' Obtener el único archivo
			Dim uploadedFile As NameAndAccessLevel = fileNames(0)
			Dim uploadedFileName As String = uploadedFile.Name
			
			' Validación 2: Verificar que el nombre comienza con el patrón correcto
			Dim expectedPrefix As String = $"Treasury_Template_{templateType}"
			If Not uploadedFileName.Contains(expectedPrefix) Then
				' Limpiar carpeta antes de lanzar error
				Me.DeleteAllFilesInFolder(si, sourcePath)
				Throw ErrorHandler.LogWrite(si, New XFException(
					$"Invalid file name." & vbCrLf &
					$"Expected: '{expectedPrefix}_[Year]_[Week]_[Company].xlsx'" & vbCrLf &
					$"Uploaded: '{uploadedFileName.Split("/"c).Last()}'"
				))
			End If
			
			' Validación 3: Verificar que los parámetros coinciden (año, semana, compañía)
			Dim expectedFileName As String = $"{expectedPrefix}_{expectedYear}_{expectedWeek}_{expectedCompany}"
			
			If Not uploadedFileName.Contains(expectedFileName) Then
				' Limpiar carpeta antes de lanzar error
				Me.DeleteAllFilesInFolder(si, sourcePath)
				
				' Mensaje de error detallado para el usuario
				Dim errorMessage As String = String.Format(
					"Uploaded file does not match the selected parameters." & vbCrLf & vbCrLf &
					"Expected template: {0}_{1}_{2}_{3}.xlsx" & vbCrLf &
					"Uploaded file: {4}" & vbCrLf & vbCrLf &
					"Please verify:" & vbCrLf &
					"  • Year: {1}" & vbCrLf &
					"  • Week: {2}" & vbCrLf &
					"  • Company: {3}",
					templateType,
					expectedYear,
					expectedWeek,
					expectedCompany,
					uploadedFileName.Split("/"c).Last()
				)
				
				Throw ErrorHandler.LogWrite(si, New XFException(errorMessage))
			End If
			
			' Todas las validaciones pasaron - retornar el archivo validado
			Return uploadedFile
			
		End Function
		
		#End Region
		
		#Region "Helper Functions"
		
		''' <summary>
		''' Splits query parameters string into dictionary
		''' </summary>
		Private Function SplitQueryParams(ByVal queryParams As String) As Dictionary(Of String, String)
			Dim paramDict As New Dictionary(Of String, String)
			
			If String.IsNullOrEmpty(queryParams) Then Return paramDict
			
			' Remove brackets if present
			queryParams = queryParams.Trim().TrimStart("["c).TrimEnd("]"c)
			
			' Split by comma and process each parameter
			Dim pairs As String() = queryParams.Split(","c)
			For Each pair In pairs
				If pair.Contains("="c) Then
					Dim parts As String() = pair.Split("="c, 2)
					If parts.Length = 2 Then
						Dim key As String = parts(0).Trim()
						Dim value As String = parts(1).Trim().TrimStart("["c).TrimEnd("]"c)
						paramDict(key) = value
					End If
				End If
			Next
			
			Return paramDict
		End Function
		
		''' <summary>
		''' Creates database parameter info list from parameter dictionary
		''' </summary>
		Private Function CreateQueryParams(ByVal paramDict As Dictionary(Of String, String)) As List(Of DbParamInfo)
			Dim dbParamInfos As New List(Of DbParamInfo)
			
			For Each kvp In paramDict
				dbParamInfos.Add(New DbParamInfo(kvp.Key, kvp.Value))
			Next
			
			Return dbParamInfos
		End Function
		
		''' <summary>
		''' Deletes all files in a folder
		''' </summary>
		Private Sub DeleteAllFilesInFolder(ByVal si As SessionInfo, ByVal folderPath As String)
			Try
				Dim fileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
					si,
					FileSystemLocation.ApplicationDatabase,
					folderPath,
					XFFileType.All,
					False, False, False
				)
				
				For Each fileName In fileNames
					BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, fileName.Name)
				Next
			Catch ex As Exception
				' If folder doesn't exist or is empty, ignore the error
			End Try
		End Sub
		
		''' <summary>
		''' Copies XLSX file from source to target path
		''' </summary>
		Private Sub CopyXLSXFile(ByVal si As SessionInfo, ByVal sourceFileName As String, ByVal sourcePath As String, ByVal targetFileName As String, ByVal targetPath As String)
			Try
				' Get source file
				Dim sourceFile As XFFileEx = BRApi.FileSystem.GetFile(
					si,
					FileSystemLocation.ApplicationDatabase,
					$"{sourcePath}/{sourceFileName}",
					True, True
				)
				
				' Create target file info
				Dim targetFileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetFileName, targetPath & "/")
				targetFileInfo.ContentFileExtension = "xlsx"
				targetFileInfo.ContentFileContainsData = True
				targetFileInfo.XFFileType = True
				
				' Create and save target file
				Dim targetFile As New XFFile(targetFileInfo, String.Empty, sourceFile.XFFile.ContentFileBytes)
				BRApi.FileSystem.InsertOrUpdateFile(si, targetFile)
			Catch ex As Exception
				Throw New XFException($"Error copying file from {sourcePath} to {targetPath}: {ex.Message}")
			End Try
		End Sub
		
		''' <summary>
		''' Validates that the week is not confirmed before allowing upload
		''' </summary>
		Private Sub ValidateWeekNotConfirmed(
			ByVal si As SessionInfo,
			ByVal templateType As String,
			ByVal company As String,
			ByVal week As String,
			ByVal year As String,
			ByVal sourcePath As String
		)
			Try
				' Convert company ID to name using helper function
				Dim companyName As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, company)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					' Check confirmation status in XFC_TRS_AUX_TreasuryWeekConfirm
					Dim checkSql As String = "
						SELECT CashDebt, CashFlow
						FROM XFC_TRS_AUX_TreasuryWeekConfirm
						WHERE Entity = @entity AND Week = @week AND Year = @year
					"
					
					Dim checkParams As New List(Of DbParamInfo) From {
						New DbParamInfo("entity", companyName),
						New DbParamInfo("week", Integer.Parse(week)),
						New DbParamInfo("year", Integer.Parse(year))
					}
					
					Dim checkDt As DataTable = BRApi.Database.ExecuteSql(dbConn, checkSql, checkParams, False)
					
					If checkDt.Rows.Count > 0 Then
						' Check if the relevant type is confirmed
						Dim isConfirmed As Boolean = False
						Dim confirmationType As String = ""
						
						Dim cashDebtValue As Object = checkDt.Rows(0)("CashDebt")
						Dim cashFlowValue As Object = checkDt.Rows(0)("CashFlow")
						
						If templateType = "CashDebtPosition" Then
							If Not IsDBNull(cashDebtValue) AndAlso Convert.ToBoolean(cashDebtValue) Then
								isConfirmed = True
								confirmationType = "Cash Debt Position"
							End If
						ElseIf templateType = "CashFlowForecasting" Then
							If Not IsDBNull(cashFlowValue) AndAlso Convert.ToBoolean(cashFlowValue) Then
								isConfirmed = True
								confirmationType = "Cash Flow Forecasting"
							End If
						End If
						
						' If confirmed, block upload and clean folder
						If isConfirmed Then
							Me.DeleteAllFilesInFolder(si, sourcePath)
							
							Dim errorMessage As String = String.Format(
								"Upload blocked - Week {0} of {1} for {2} is confirmed." & vbCrLf & vbCrLf &
								"Type: {3}" & vbCrLf & vbCrLf &
								"To upload new data, you must first unconfirm this week in the Treasury Monitoring dashboard.",
								week,
								year,
								company,
								confirmationType
							)
							
							Throw ErrorHandler.LogWrite(si, New XFException(errorMessage))
						End If
					End If
				End Using
			Catch ex As XFException
				' Re-throw XFExceptions (already formatted)
				Throw
			Catch ex As Exception
				' Log and rethrow errors
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		
		#Region "Run Population Queries"
				
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
		
		#Region "Run Population Queries"
		
		Private Sub RunPopulationQueries(ByVal si As SessionInfo, ByVal tableName As String)
			'Check what types of accounts we have in the RAW data to determine which tables to populate
			Dim tableList As New List(Of Object)
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				' Check what tables exist to determine which population queries to run
				Dim hasCashDebtData As Boolean = False
				Dim hasCashflowData As Boolean = False
				
				' Check if CashDebtPosition RAW table has data
				Dim cashDebtCheckDt As DataTable = BRApi.Database.ExecuteSql(
					dbConn,
					"IF OBJECT_ID('XFC_TRS_RAW_CashDebtPosition', 'U') IS NOT NULL SELECT COUNT(*) as RecordCount FROM XFC_TRS_RAW_CashDebtPosition ELSE SELECT 0 as RecordCount",
					False
				)
				If cashDebtCheckDt.Rows.Count > 0 AndAlso Convert.ToInt32(cashDebtCheckDt.Rows(0)("RecordCount")) > 0 Then
					hasCashDebtData = True
				End If
				
				' Check if CashFlowForecasting RAW table has data
				Try
					Dim cashFlowCheckDt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"IF OBJECT_ID('XFC_TRS_RAW_CashFlowForecasting', 'U') IS NOT NULL SELECT COUNT(*) as RecordCount FROM XFC_TRS_RAW_CashFlowForecasting ELSE SELECT 0 as RecordCount",
						False
					)
					If cashFlowCheckDt.Rows.Count > 0 AndAlso Convert.ToInt32(cashFlowCheckDt.Rows(0)("RecordCount")) > 0 Then
						hasCashflowData = True
					End If
				Catch ex As Exception
					' Table might not exist yet, ignore
				End Try
				

				
				' Add tables to process based on available data
				If hasCashDebtData Then
					tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_cashdebtposition)
				End If
				
				If hasCashflowData Then
					tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_cashflowforecasting)
				End If
				
				' Always populate TreasuryMonitoring and TreasuryWeekConfirm when ANY data is available
				If hasCashDebtData OrElse hasCashflowData Then
					tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_treasury_monitoring)
					tableList.Add(New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_treasuryweekconfirm)
				End If
				
				'Return if no relevant tables to populate
				If tableList.Count < 1 Then
					BRApi.Database.ExecuteSql(dbConn, "PRINT 'Warning: No data found in RAW tables for Treasury processing. Check XFC_TRS_RAW_CashDebtPosition and XFC_TRS_RAW_CashFlowForecasting tables.'", False)
					Return
				End If
				
				'Get population queries for relevant tables only
				Dim populationQueries As List(Of String) = Me.GetPopulationQueries(si, tableList)
				
				'Populate tables
				For Each query In populationQueries
					BRApi.Database.ExecuteSql(dbConn, query, False)
				Next
			End Using
		End Sub
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
