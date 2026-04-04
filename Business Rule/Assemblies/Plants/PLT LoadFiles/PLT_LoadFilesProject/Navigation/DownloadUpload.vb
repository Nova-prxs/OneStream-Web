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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.DownloadUpload
	
	Public Class MainClass
		
		Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
		
			Try
				Select Case args.FunctionType
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
		#Region "Upload Files"
		
						If args.FunctionName.XFEqualsIgnoreCase("UploadFile") Then
							
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim shared_BiBlend As New OneStream.BusinessRule.Extender.UTI_SharedBiBlend.shared_BiBlend	
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							Dim user As String = si.UserName
							
							' VARIABLES
							Dim filePath As String = "Documents/Public/Plants/Import"
							
							Dim factory As String = args.NameValuePairs("Factory")							
							Dim fileName As String = args.NameValuePairs("FileName")
							Dim scenario As String = args.NameValuePairs("Scenario")
							Dim year As String = args.NameValuePairs("Year")
							Dim month As String = args.NameValuePairs("Month")	

							Dim FileNameAlone As String = String.Empty
							Dim extension As String = String.Empty
							
							If fileName.Contains(".") Then
								
								FileNameAlone = fileName.Split(".")(0)
								extension = fileName.Split(".")(1)
								
								' Reemplazo de un fichero ya cargado con Mxx 
								If FileNameAlone.Contains("M") And FileNameAlone.Contains("MASTER") = False Then
									FileNameAlone = FileNameAlone.Split("_M")(0)
								End If	
								
							Else 
								
								FileNameAlone = FileName
								extension = ""
								
							End If						
							
							Dim fileNamesOpenList As New List(Of String) From {"XFC_PLT_ACT_Nomenclature", "XFC_PLT_HR_DailyHours_PCT", "XFC_PLT_HR_TimePresence", "XFC_PLAN_Headcount","XFC_PLT_HR_Balancing","XFC_PLT_HR_Calendar","XFC_PLT_COST_StartUp"}
							' Para habilitar un escenario y fábrica concretas
'							If (uti_sharedqueries.ValidScenarioPeriod(si, scenario, year, month)) Or fileNamesOpenList.Contains(FileNameAlone) Or (scenario = "Budget_V2" And factory="R0529002")Then							
							If (uti_sharedqueries.ValidScenarioPeriod(si, scenario, year, month)) Or fileNamesOpenList.Contains(FileNameAlone) Then							
							
								' CARGA de los datos en la tabla final
								Dim loadResults As List(Of TableRangeContent) = WriteFileToTable(si, $"{filePath}/{factory}/{year}/{scenario}/{fileName}", FileNameAlone, factory, year, month, extension, scenario)
								
								'Errores en la carga
								If loadResults Is Nothing Then
									Return PopUp($"❌ Invalid File Name{vbCrLf}CORRECT Name - {fileName}")								
								Else If loadResults.Count = 1 Then 									
									Return PopUp( $"⚠️ Advertencia: Falta generar el INSERT")								
								End If
							
							Else 
								
								Return If(scenario = "Actual", PopUp($"Invalid upload - Month {month}/{year} closed."), PopUp("Invalid upload - Scenario closed."))
								
							End If
							
							' GUARDADO DEL FICHERO
							' 1- Recogemos la información del fichero cargado
							Dim infoFile As XFFileInfo = BRApi.FileSystem.GetFileInfo(si,FileSystemLocation.ApplicationDatabase,$"{filePath}/{factory}/{year}/{scenario}/{fileName}",True).XFFileInfo
							Dim file As XFFile = BRApi.FileSystem.GetFile(si,FileSystemLocation.ApplicationDatabase,$"{filePath}/{factory}/{year}/{scenario}/{fileName}",True, True).XFFile
							
							' 2- Creación de un archivo igual con el nombre deseado
							' Falta crear las reglas en función de si es Actual, Forecast o Budget
							If scenario = "Actual" Then
								Dim fileName0 As String = fileName.Split(".")(0)
								Dim fileExt As String = fileName.Split(".")(1)
								file.FileInfo.Name = $"{fileName0}_M{month}.{fileExt}"
								file.FileInfo.Description = $"Modify by {user}"
								
								Brapi.FileSystem.InsertOrUpdateFile(si, file)
								Brapi.FileSystem.DeleteFile(si,FileSystemLocation.ApplicationDatabase, $"{filePath}/{factory}/{year}/{scenario}/{fileName}")
						
							Else
								Dim fileName0 As String = fileName.Split(".")(0)
								Dim fileExt As String = fileName.Split(".")(1)
								file.FileInfo.Name = $"{fileName0}.{fileExt}"
								file.FileInfo.Description = $"Modify by {user}"
								
								Brapi.FileSystem.InsertOrUpdateFile(si, file)
							End If
							
							' Actualizacion de Tablas auxiliares para la carga de Produccion y Costes
							Dim fileNamesList As New List(Of String) From {"XFC_PLT_PROD_Data", "Añadir mas"}
							If fileNamesList.Contains(fileNameAlone) Then
 								' uti_sharedqueries.update_FactVTU_Report_NewTables(si, scenario, year, month)
							End If 
							
							Dim fileNamesNomenclatureList As New List(Of String) From {"XFC_PLT_PLAN_Nomenclature", "XFC_PLT_ACT_Nomenclature"}
							If fileNamesNomenclatureList.Contains(fileNameAlone) Then
		
								uti_sharedqueries.update_Nomenclature_Report(si, scenario, year, month,,factory)									
								shared_BiBlend.CopyTable(si, "XFC_PLT_HIER_Nomenclature_Date_Report", factory, "12", year, scenario)
								
	
							End If 
								
							Return PopUp($"{vbTab}{vbTab}-------- {factory} --------{vbCrLf}{vbTab}File: {fileNameAlone}{vbCrLf}{vbTab}{scenario}: {year} - {month}{vbCrLf}Loaded successfully{vbCrLf}{vbTab}{vbTab}------------------------")								

							
#End Region

		#Region "Upload Files Admin"
		
						Else If args.FunctionName.XFEqualsIgnoreCase("UploadFileAdmin") Then
							brapi.ErrorLog.LogMessage(si, "1 entra")
							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries																		
							Dim shared_BiBlend As New OneStream.BusinessRule.Extender.UTI_SharedBiBlend.shared_BiBlend
							
'							Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							Dim user As String = si.UserName
							
							brapi.ErrorLog.LogMessage(si, "2 entra")
'							' VARIABLES
							Dim filePath As String = "Documents/Public/Plants/Admin"
							
							Dim fileName As String = args.NameValuePairs("FileName")
							
							Dim year As String = args.NameValuePairs("Year")
							Dim month As String = args.NameValuePairs("Month")
							Dim factory As String = args.NameValuePairs.GetValueOrDefault("Factory", "")							
							Dim scenario As String = args.NameValuePairs("Scenario")	

							Dim FileNameAlone As String = String.Empty
							Dim extension As String = String.Empty
							
							brapi.ErrorLog.LogMessage(si, "3 entra")
							
							If fileName.Contains(".") Then
								
								FileNameAlone = fileName.Split(".")(0).Replace(" ", "")
								extension = fileName.Split(".")(1)
								
							Else 
								
								FileNameAlone = FileName
								extension = ""
								
							End If	
							
							brapi.ErrorLog.LogMessage(si, "4 entra")
							
							WriteFileToTable(si, $"{filePath}/{fileName}", FileNameAlone,factory,year,month,extension,scenario)
							
							brapi.ErrorLog.LogMessage(si, "5 entra")

							' GUARDADO DEL FICHERO
							' 1- Recogemos la información del fichero cargado
							Dim infoFile As XFFileInfo = BRApi.FileSystem.GetFileInfo(si,FileSystemLocation.ApplicationDatabase,$"{filePath}/{fileName}",True).XFFileInfo
							Dim file As XFFile = BRApi.FileSystem.GetFile(si,FileSystemLocation.ApplicationDatabase,$"{filePath}/{fileName}",True, True).XFFile
							
							' 2- Creación de un archivo igual con el nombre deseado
							Dim fileName0 As String = fileName.Split(".")(0)
							Dim fileExt As String = fileName.Split(".")(1)
							file.FileInfo.Name = $"{fileName0}_M{month}.{fileExt}"
							file.FileInfo.Description = $"Modify by {user}"
							
							Brapi.FileSystem.InsertOrUpdateFile(si, file)
							Brapi.FileSystem.DeleteFile(si,FileSystemLocation.ApplicationDatabase, $"{filePath}/{fileName}")
							
							brapi.ErrorLog.LogMessage(si, "6 entra")
							
							' Actualizacion de Tablas auxiliares para la carga de Produccion y Costes
							Dim fileNamesList As New List(Of String) From {"XFC_PLT_ADMIN_Nomenclature", "Añadir mas"}
							If fileNamesList.Contains(fileNameAlone) Then
		
								uti_sharedqueries.update_Nomenclature_Report(si, scenario, year, month,,"")									
								shared_BiBlend.CopyTable(si, "XFC_PLT_HIER_Nomenclature_Date_Report", "", "12", year, scenario)
								
	
							End If 
							brapi.ErrorLog.LogMessage(si, "7 entra")
									
							Return PopUp($"{vbTab}{vbTab}-------- {month} / {year} --------{vbCrLf}{vbTab}{fileNameAlone}{vbCrLf}{vbTab}Loaded successfully{vbCrLf}{vbTab}{vbTab}------------------------")								

							
#End Region

		#Region "Download Files"
		
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadFile") Then
							
							'Almacenamos los parámetros pasados desde el boton del dashboard para generar el fichero
							Dim filepath As String = "Documents/Public/Plants/Import"
							Dim fileName As String = args.NameValuePairs("FileName")							
							Dim scenario As String = args.NameValuePairs("Scenario")
							Dim factory As String = args.NameValuePairs("Factory")
							Dim year As String = args.NameValuePairs("Year")
							Dim month As String = args.NameValuePairs("Month")							
							
							Dim finalPath As String = String.Empty							
							Dim fileNameMonth As String = If(scenario = "Actual", $"{fileName.Split(".")(0)}_M{month}.{fileName.Split(".")(1)}",$"{fileName}")
							
							' Final Path dependiendo de si ya ha sido cargado o no dato para el escenario y mes seleccionado
							If (BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, $"{filePath}/{factory}/{year}/{scenario}/{fileNameMonth}") And fileName.Contains("txt") = False)			
							
								finalPath =  $"[Documents/Public/Plants/Import/{factory}/{year}/{scenario}/{fileNameMonth}]"
								fileName = 	fileNameMonth
								
							Else
								
								finalPath =  $"[Documents/Public/Plants/Templates/{fileName}]"
								
							End If			
							
							brapi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase,$"{filePath}/{factory}",$"{year}/{scenario}" )
							
							' Cambio del parámetro de Path para descargar el fichero seleccionado
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult With {.ChangeCustomSubstVarsInDashboard = True}
							
							args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_downloadFilePath") = finalPath							
							args.SelectionChangedTaskInfo.CustomSubstVars("DownloadInfo") = "Download Template: " & fileName
							
							selectionChangedTaskResult.ModifiedCustomSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars	
							
							Return selectionChangedTaskResult
							
#End Region

		#Region "Download Files Admin"
		
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadFileAdmin") Then
							
							'Almacenamos los parámetros pasados desde el boton del dashboard para generar el fichero
							Dim filepath As String = "Documents/Public/Plants/Admin"
							Dim fileName As String = args.NameValuePairs("FileName")
							Dim year As String = args.NameValuePairs("Year")
							Dim month As String = args.NameValuePairs("Month")							
							
							Dim finalPath As String = String.Empty							
							Dim fileNameMonth As String = $"{fileName.Split(".")(0)}_M{month}.{fileName.Split(".")(1)}"
							
							' Final Path dependiendo de si ya ha sido cargado o no dato para el escenario y mes seleccionado
							If (BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, $"{filePath}/{fileNameMonth}") And fileName.Contains("txt") = False)			
							
								finalPath =  $"[Documents/Public/Plants/Admin/{fileNameMonth}]"
								fileName = 	fileNameMonth
								
							Else
								
								finalPath =  $"[Documents/Public/Plants/Templates/Admin/{fileName}]"
								
							End If			
							
							' Cambio del parámetro de Path para descargar el fichero seleccionado
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult With {.ChangeCustomSubstVarsInDashboard = True}
							
							args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_downloadFilePath") = finalPath							
							args.SelectionChangedTaskInfo.CustomSubstVars("DownloadInfo") = "Download Template: " & fileName
							
							selectionChangedTaskResult.ModifiedCustomSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars	
							
							Return selectionChangedTaskResult
							
		#End Region

		#Region "Download Files OneShot"
		
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadFile_OneShot") Then
							
							'Almacenamos los parámetros pasados desde el boton del dashboard para generar el fichero
							Dim filepath As String = "Documents/Public/Plants/Import"
							Dim fileName As String = args.NameValuePairs("FileName")							
							Dim scenario As String = args.NameValuePairs("Scenario")
							Dim factory As String = args.NameValuePairs("Factory")
							Dim year As String = args.NameValuePairs("Year")
							Dim month As String = args.NameValuePairs("Month")							
							
							Dim finalPath As String = String.Empty
							
							' Final Path dependiendo de si ya ha sido cargado o no dato para el escenario y mes seleccionado
							If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, $"{filePath}/{factory}/{FileName}")			
							
								finalPath =  $"[Documents/Public/Plants/Import/{factory}/{FileName}]"
								
							Else
								
								finalPath =  $"[Documents/Public/Plants/Templates/OneShot/{fileName}]"
								
							End If			
							
							' Cambio del parámetro de Path para descargar el fichero seleccionado
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult With {.ChangeCustomSubstVarsInDashboard = True}
							
							args.SelectionChangedTaskInfo.CustomSubstVars("PLT_LoadFiles_prm_downloadFilePath") = finalPath							
							args.SelectionChangedTaskInfo.CustomSubstVars("DownloadInfo") = "Download Template: " & fileName
							
							selectionChangedTaskResult.ModifiedCustomSubstVars = args.SelectionChangedTaskInfo.CustomSubstVars	
							
							Return selectionChangedTaskResult
							
		#End Region
		
		#Region "Insert Data - SAP4H"
		
						' Para execute functions
						Else If args.FunctionName.XFEqualsIgnoreCase("InsertIntoTables") Then
							
							' Dim filePath As String = "Documents/Public/Plants/Import"
							' Dim fileName As String = args.NameValuePairs("FileName")
							' Dim extension As String = fileName.Split(".")(1)
							' Dim scenario As String = args.NameValuePairs("Scenario")
							
							Dim factory As String = args.NameValuePairs("Factory")							
							Dim year As String = args.NameValuePairs("Year")
							Dim month As String = args.NameValuePairs("Month")													
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ShowMessageBox = True
							
							If (factory = "R0671") Then
								
								Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
								shared_queries.InsertCostsS4H(si, factory, year, month)
								
								uti_sharedqueries.update_Log(si, "Actual", factory, "XFC_PLT_FACT_Costs", "Update")
								
								selectionChangedTaskResult.IsOK = True
								selectionChangedTaskResult.Message = "Costs correctly updated."
							Else
								
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.Message = "Update from S4H not allowed."
								
							End If
							
							Return selectionChangedTaskResult								
							
		#End Region
	
		#Region "Insert Data - OLD"
		
						' Para execute functions
						Else If args.FunctionName.XFEqualsIgnoreCase("InsertIntoTables_OLD") Then
							
							Dim filePath As String = "Documents/Public/Plants/Import"
							Dim factory As String = args.NameValuePairs("Factory")							
							Dim fileName As String = args.NameValuePairs("FileName")
							' Dim extension As String = fileName.Split(".")(1)
							Dim year As String = args.NameValuePairs("Year")
							' Dim scenario As String = args.NameValuePairs("Scenario")
							Dim month As String = args.NameValuePairs("Month")
							
							' CARGA de los datos en la tabla final
							Me.WriteFileToTable(si, $"{fileName}", "",factory, year, month, "")
							
	#End Region

		#Region "Copy Last Month"
		
						' Para execute functions
						Else If args.FunctionName.XFEqualsIgnoreCase("CopyMonth") Then
							
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							Dim user As String = si.UserName
							
							' VARIABLES
							Dim filePath As String = "Documents/Public/Plants/Import"
							
							' Declaración de variables
							Dim scenario As String = args.NameValuePairs("Scenario")
							Dim factory As String = args.NameValuePairs("Factory")
							Dim fileName As String = args.NameValuePairs("FileName")							
							Dim year As String = args.NameValuePairs("Year")
							Dim month As Integer = args.NameValuePairs("Month")
							
							' Último mes a copiar en base al seleccionado
							Dim monthCopy = String.Empty
							Dim yearCopy = String.Empty
							
							If month = 1 Then 
								monthCopy = 12
								yearCopy = year-1
							Else 
								monthCopy = month-1
								yearCopy = year
							End If
							
							' Tablas de Origen y Destino del copiado
							Dim tableName As String = String.Empty
							Dim tableDest As String = String.Empty
							
							Dim sql As String = String.Empty
							
							#Region "Key Allocations"
	
							If fileName = "XFC_PLT_COST_KeyAllocations.xlsx" Then

								tableName = "XFC_PLT_AUX_AllocationKeys"
								tableDest = "XFC_PLT_AUX_AllocationKeys"
								
								sql = $"
									-- 1. Previous Clear
								
									DELETE FROM {tableDest}
									WHERE 1=1
										AND scenario = '{scenario}'
										AND id_factory = '{factory}'
										AND YEAR(date) = {year}
										AND MONTH(date) = {month}	
								
									-- 2. Insert
								
									INSERT INTO {tableDest} (date, id_averagegroup, id_costcenter, percentage, scenario, costnature, id_factory)
									
									SELECT
										DATEADD(MONTH, 1, date) as New_Date
										, id_averagegroup
										, id_costcenter
										, percentage
										, scenario
										, costnature
										, id_factory
								
									FROM {tableName}
									
									WHERE 1=1
										AND scenario = '{scenario}'
										AND MONTH(date) = {monthCopy}
										AND YEAR(date) = {yearCopy}								
								"
	#End Region
	
							#Region "Fixed Variable Costs"
	
							Else If fileName = "XFC_PLT_COST_FixedVariable.xlsx" Then
								
								tableName = "XFC_PLT_AUX_FixedVariableCosts"
								tableDest = "XFC_PLT_AUX_FixedVariableCosts"
								
								sql = $"
									-- 1 - Previous Clear
								
									DELETE FROM {tableDest}
									WHERE 1=1
										AND scenario = '{scenario}'
										AND id_factory = '{factory}'
										AND YEAR(date) = {year}
										AND MONTH(date) = {month}	
								
									-- 2- Insert
									
									INSERT INTO {tableDest} (date, scenario, id_factory, id_account, costnature, value)
									
									SELECT
										DATEADD(MONTH, 1, date) as New_Date
										, scenario
										, id_factory
										, id_account
										, costnature
										, value
								
									FROM {tableName}
									
									WHERE 1=1
										AND scenario = '{scenario}'
										AND MONTH(date) = {monthCopy}
										AND YEAR(date) = {yearCopy}								
								"
								
							End If

	#End Region							
							
							If sql <> String.Empty Then
								Me.ExecuteSql(si, sql)
							End If
							
							#Region "Modificación de Fichero"
							
							' GUARDADO DEL FICHERO
							' 1- Recogemos la información del fichero cargado
							Dim infoFile As XFFileInfo = BRApi.FileSystem.GetFileInfo(si,FileSystemLocation.ApplicationDatabase,$"{filePath}/{factory}/{year}/{scenario}/{fileName.Split(".")(0)}._M{month-1}.{fileName.Split(".")(1)}",True).XFFileInfo
							Dim file As XFFile = BRApi.FileSystem.GetFile(si,FileSystemLocation.ApplicationDatabase,$"{filePath}/{factory}/{year}/{scenario}/{fileName}",True, True).XFFile
							
							' 2- Creación de un archivo igual con el nombre deseado
							' Falta crear las reglas en función de si es Actual, Forecast o Budget
							If scenario = "Actual" Then
								Dim fileName0 As String = fileName.Split(".")(0)
								Dim fileExt As String = fileName.Split(".")(1)
								file.FileInfo.Name = $"{fileName0}_M{month}.{fileExt}"
								file.FileInfo.Description = $"Modify by {user}"
								
								Brapi.FileSystem.InsertOrUpdateFile(si, file)
								Brapi.FileSystem.DeleteFile(si,FileSystemLocation.ApplicationDatabase, $"{filePath}/{factory}/{year}/{scenario}/{fileName}")
						
							Else
								Dim fileName0 As String = fileName.Split(".")(0)
								Dim fileExt As String = fileName.Split(".")(1)
								file.FileInfo.Name = $"{fileName0}.{fileExt}"
								file.FileInfo.Description = $"Modify by {user}"
								
								Brapi.FileSystem.InsertOrUpdateFile(si, file)
							End If
							
							#End Region
							
						
		#End Region

						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "ETL"		
		
		Private Function WriteFileToTable(	ByVal si As SessionInfo, _
											ByVal filePath As String, _
											Optional FileNameAlone As String = "", _
											Optional factory As String = "", _
											Optional year As String = "", _
											Optional month As String = "", _
											Optional extension As String = "", _
											Optional scenario As String = "" _
										) As List(Of TableRangeContent)
		
			Try
					
				' Año y mes de referencia
				Dim yearAct As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_year_N")
				Dim monthAct As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_month_closing")
				Dim scenarioFcs As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_scenario_forecast")
				Dim monthFcts As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_month_first_forecast")
				Dim scenarioBud As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_PLT_scenario_budget")
				
				#Region "Month Filter - Common"
				' Opción de crear un filtro común
				Dim monthFilter As String = If( scenario = "Actual", $" MONTH(date) = {month} ", 
											If( scenario.Contains("RF"), $" MONTH(date) >= {monthFcts} ", 
											If( scenario.Contains("Bud"), $" MONTH(date) >= 1", ""))
											)
												 
				Dim monthFilterCast As String = If( scenario = "Actual", $" CAST(SUBSTRING(mes, 2, 2) AS INT) = {month} ", 
												If( scenario.Contains("RF"), $" CAST(SUBSTRING(mes, 2, 2) AS INT) >= {monthFcts} ", 
												If( scenario.Contains("Bud"), $" CAST(SUBSTRING(mes, 2, 2) AS INT) >= 1 ", ""))
												)
												 
				Dim monthFilterCastDate As String = If( scenario = "Actual", $" MONTH(CAST(Date As Date)) = {month}", 
													If( scenario.Contains("RF"), $" MONTH(CAST(Date As Date)) >= {monthFcts} ", 
													If( scenario.Contains("Bud"), $" MONTH(CAST(Date As Date)) >= 1 ", ""))
													)
				Dim monthFilterConvertDate As String = 	If( scenario = "Actual", $" MONTH(CONVERT(DATE, date, 112)) = {month} ", 
														If( scenario.Contains("RF"), $" MONTH(CONVERT(DATE, date, 112)) >= {monthFcts} ", 
								 						If( scenario.Contains("Bud"), $" MONTH(CONVERT(DATE, date, 112)) >= 1 ", ""))
								 						)			 
				#End Region

				' Modificar el parámetro de mes Forecast
				monthFcts  = If( scenario = "Actual", $"NA", 
								If (scenario.Contains("RF"), $"{monthFcts}", 
								 If(scenario.Contains("Bud"), $"1", "NA"))
								 )
				
				' Variables para el fichero
				Dim correctFileName As Boolean = False
				Dim loadResults As New List(Of TableRangeContent)
				Dim fileInfo As XFFileEx = Nothing
				
				'Get the upload "TEMP" file from the database file system
				If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath) Then
					fileInfo = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
				Else
					Return Nothing
				End If
				
				'Config. for Load a Delimited File
				Dim dbLocation As String = "Application"
				Dim loadMethod As String = "Replace"	 ' Use an expression for partial replace -->  Merge:[(ProductName = 'SomeProduct')] 								
				Dim fieldTokens As New List(Of String)
				
				Dim tableName As String = String.Empty
				Dim tableDest As String = String.Empty
				
				Dim sqlRaw As String = String.Empty
				Dim sqlMap As String = String.Empty
		
		#Region "USERS"
		
		#Region "PRODUCTION"
		
			#Region "PRODUCTION - Actual Data"
		
				If fileNameAlone = $"XFC_PLT_PROD_Data" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_Production"	
					tableDest = "XFC_PLT_FACT_Production"	
					
					sqlRaw = $"
						DROP TABLE IF EXISTS XFC_PLT_RAW_Production;
						DROP TABLE IF EXISTS XFC_PLT_RAW_Production_SAP;
						
						CREATE TABLE {tableName} 
							 	(
								[id_factory] varchar(50),
								[date] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_product] varchar(50) ,
								[uocat] varchar(50) ,
								[uotype] varchar(50) ,
								[volume] decimal(18,4),
								[allocation_taj] decimal(18,7),
								[activity_taj] decimal(18,4),
								[allocation_tso] decimal(18,7),
								[activity_tso] decimal(18,4) 
							 	);
							CREATE TABLE XFC_PLT_RAW_Production_SAP
							 	(
								[id_company] varchar(50),
								[id_factory] varchar(50),
								[date] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_product] varchar(50) ,
								[uocat] varchar(50) ,
								[volume] decimal(18,4),
								[activity_taj] decimal(18,4)
							 	);
					"					
					
					#Region "Mapping General"
					
					' Mapping General
					sqlMap = $"	
								-- 1- Previous clear
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND MONTH(date) = {month}
						
					
								-- 2- Insert statement
								
								INSERT INTO {tableDest} (id_factory, date, scenario, id_averagegroup, id_workcenter, id_costcenter, id_product, uotype, volume, allocation_taj, activity_taj, allocation_tso, activity_tso)
								
								SELECT *
								FROM (
								SELECT 
									'{factory}' as id_factory 
									, CONVERT(DATE, A.date, 112) as date
									, 'Actual' as scenario 
									, A.id_averagegroup AS id_averagegroup
									, '-1' as id_workcenter 
									, A.id_costcenter 
									, A.id_product
									, CASE 
										WHEN A.uocat = 'UO1' AND A.uotype <> 'P P' AND A.uotype <> 'CUR' THEN 'UO1'
										WHEN A.uocat = 'UO2' AND A.uotype <> 'P P' AND A.uotype <> 'CUR' THEN 'UO2'
										ELSE '-1' 
									END AS uotype
									, sum(A.volume) as volume 
									, CASE 
										WHEN SUM(A.volume) = 0 THEN 0 
										ELSE SUM(A.volume * A.allocation_taj) / sum(volume) 
									END as allocation_taj
									, sum(A.activity_taj) as activity_taj
									, CASE 
										WHEN SUM(A.volume) = 0 THEN 0 
										ELSE SUM(A.volume * A.allocation_tso) / sum(A.volume) 
									END as allocation_tso
									, sum(A.activity_tso) as activity_tso
					
					
								FROM XFC_PLT_RAW_Production A
					
								WHERE 1=1
									AND MONTH(CONVERT(DATE, A.date, 112)) = {month}						
									AND YEAR(CONVERT(DATE, A.date, 112)) = {year}						
					
								GROUP BY A.id_factory, CONVERT(DATE, A.date, 112), A.id_costcenter, A.id_averagegroup, A.id_product, A.uocat, A.uotype
								
								) as ext
					
								WHERE 1=1
									AND ext.uotype <> '-1'
					"
					
'					sqlMap = $"	
'								-- 1- Previous clear
'								DELETE FROM {tableDest}
'								WHERE 1=1
'									AND scenario = '{scenario}'
'									AND id_factory = '{factory}'
'									AND YEAR(date) = {year}
'									AND MONTH(date) = {month}
						
					
'								-- 2- Insert statement
								
'								INSERT INTO {tableDest} (id_factory, date, scenario, id_averagegroup, id_workcenter, id_costcenter, id_product, uotype, volume, allocation_taj, activity_taj, allocation_tso, activity_tso)
								
'								SELECT *
'								FROM (
'								SELECT 
'									'{factory}' as id_factory 
'									, CONVERT(DATE, date, 112) as date
'									, 'Actual' as scenario 
'									, id_averagegroup 
'									, '-1' as id_workcenter 
'									, id_costcenter 
'									, id_product
'									, CASE 
'										WHEN uocat = 'UO1' AND uotype <> 'P P' AND uotype <> 'CUR' THEN 'UO1'
'										WHEN uocat = 'UO2' AND uotype <> 'P P' AND uotype <> 'CUR' THEN 'UO2'
'										ELSE '-1' 
'									END AS uotype
'									, sum(volume) as volume 
'									, CASE 
'										WHEN SUM(volume) = 0 THEN 0 
'										ELSE SUM(volume * allocation_taj) / sum(volume) 
'									END as allocation_taj
'									, sum(activity_taj) as activity_taj
'									, CASE 
'										WHEN SUM(volume) = 0 THEN 0 
'										ELSE SUM(volume * allocation_tso) / sum(volume) 
'									END as allocation_tso
'									, sum(activity_tso) as activity_tso
					
					
'								FROM XFC_PLT_RAW_Production 
					
'								WHERE 1=1
'									AND MONTH(CONVERT(DATE, date, 112)) = {month}						
'									AND YEAR(CONVERT(DATE, date, 112)) = {year}						
					
'								GROUP BY id_factory, CONVERT(DATE, date, 112), id_costcenter, id_averagegroup, id_product, uocat, uotype
'								) as ext
					
'								WHERE 1=1
'									AND uotype <> '-1'
'					"
					#End Region
					
					#Region "SAP4H - Aveiro"
					
					If factory = "R0671" And CInt(month) >= 7 And CInt(year) >= 2025 Then						
						
						' Mapping General
						sqlMap = $"	
									-- 1- Previous clear
									DELETE FROM {tableDest}
									WHERE 1=1
										AND scenario = '{scenario}'
										AND id_factory = '{factory}'
										AND YEAR(date) = {year}
										AND MONTH(date) = {month};
							
						
									-- 2- Insert statement
									; WITH XFC_PLT_RAW_Production_CC as (
										
										SELECT 
											-- CONVERT(DATE, A.date, 112) as date
						 					DATEADD(DAY, A.date - 2, '1900-01-01') AS date
											, COALESCE(B.id, A.id_costcenter+'_NEW') as id_costcenter
											, A.id_product
											, A.volume
											, A.activity_taj
						
										FROM XFC_PLT_RAW_Production_SAP A
							
										LEFT JOIN XFC_PLT_MAPPING_CostCenter_SAP B
											ON B.id_factory = '{factory}'
											AND B.id_sap = A.id_costcenter
										
										WHERE 1=1
											-- AND MONTH(CONVERT(DATE, A.date, 112)) = {month}						
											-- AND YEAR(CONVERT(DATE, A.date, 112)) = {year}
											AND MONTH(DATEADD(DAY, A.date - 2, '1900-01-01')) = {month}
											AND YEAR(DATEADD(DAY, A.date - 2, '1900-01-01')) = {year}
											AND A.uocat = 'MF1.00'					
									)
						
									INSERT INTO {tableDest} (id_factory, date, scenario, id_averagegroup, id_workcenter, id_costcenter, id_product, uotype, volume, allocation_taj, activity_taj, allocation_tso, activity_tso)
									
									SELECT *
									
									FROM (
								
						-- UO1
										SELECT 						
											'{factory}' as id_factory 
											, date
											, 'Actual' as scenario 
											, COALESCE(C.id_averagegroup, '-1') AS id_averagegroup
											, '-1' as id_workcenter 
											, A.id_costcenter 
											, A.id_product
											, 'UO1' AS uotype
											, sum(A.volume) as volume 
											, CASE 
												WHEN SUM(A.volume) = 0 THEN 0 
												ELSE SUM(A.activity_taj) / sum(A.volume) 
											  END as allocation_taj
											, sum(A.activity_taj) as activity_taj
											, 0 as allocation_tso
											, 0 as activity_tso
							
							
										FROM XFC_PLT_RAW_Production_CC A
										
										LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
											ON C.id = A.id_costcenter
											AND C.id_factory = '{factory}'
											AND A.date BETWEEN C.start_date AND C.end_date						
							
										GROUP BY A.date, A.id_costcenter, C.id_averagegroup, A.id_product
								
								UNION ALL
						
						-- UO2	
										SELECT 						
											'{factory}' as id_factory 
											, date
											, 'Actual' as scenario 
											, C.id_averagegroup
											, '-1' as id_workcenter 
											, A.id_costcenter 
											, A.id_product
											, 'UO2' AS uotype
											, sum(A.volume) as volume 
											, AVG(D.value) as allocation_taj
											, sum(A.volume*D.value) as activity_taj
											, 0 as allocation_tso
											, 0 as activity_tso
							
							
										FROM XFC_PLT_RAW_Production_CC A
										
										LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
											ON C.id = A.id_costcenter
											AND C.id_factory = '{factory}'
											AND A.date BETWEEN C.start_date AND C.end_date
									
										LEFT JOIN XFC_PLT_AUX_Production_Actual_Times D
											ON D.id_factory = '{factory}'
											AND D.uotype = 'UO2'
											AND D.id_costcenter = A.id_costcenter
											AND D.id_averagegroup = C.id_averagegroup
											AND D.id_product = A.id_product
											AND A.date BETWEEN D.start_date AND D.end_date
							
										GROUP BY A.date, A.id_costcenter, C.id_averagegroup, A.id_product						
										
									
									) as ext
						
						"
						
'						sqlMap = $"	
'									-- 1- Previous clear
'									DELETE FROM {tableDest}
'									WHERE 1=1
'										AND scenario = '{scenario}'
'										AND id_factory = '{factory}'
'										AND YEAR(date) = {year}
'										AND MONTH(date) = {month};
							
						
'									-- 2- Insert statement
'									; WITH XFC_PLT_RAW_Production_CC as (
										
'										SELECT 
'											-- CONVERT(DATE, A.date, 112) as date
'						 					DATEADD(DAY, A.date - 2, '1900-01-01') AS date
'											, COALESCE(B.id, A.id_costcenter+'_NEW') as id_costcenter
'											, A.id_product
'											, A.volume
'											, A.activity_taj
						
'										FROM XFC_PLT_RAW_Production_SAP A
							
'										LEFT JOIN XFC_PLT_MAPPING_CostCenter_SAP B
'											ON B.id_factory = '{factory}'
'											AND B.id_sap = A.id_costcenter
										
'										WHERE 1=1
'											-- AND MONTH(CONVERT(DATE, A.date, 112)) = {month}						
'											-- AND YEAR(CONVERT(DATE, A.date, 112)) = {year}
'											AND MONTH(DATEADD(DAY, A.date - 2, '1900-01-01')) = {month}
'											AND YEAR(DATEADD(DAY, A.date - 2, '1900-01-01')) = {year}
'											AND A.uocat = 'MF1.00'					
'									)
						
'									INSERT INTO {tableDest} (id_factory, date, scenario, id_averagegroup, id_workcenter, id_costcenter, id_product, uotype, volume, allocation_taj, activity_taj, allocation_tso, activity_tso)
									
'									SELECT *
									
'									FROM (
								
'						-- UO1
'										SELECT 						
'											'{factory}' as id_factory 
'											, date
'											, 'Actual' as scenario 
'											, COALESCE(C.id_averagegroup, '-1') AS id_averagegroup
'											, '-1' as id_workcenter 
'											, A.id_costcenter 
'											, A.id_product
'											, 'UO1' AS uotype
'											, sum(A.volume) as volume 
'											, CASE 
'												WHEN SUM(A.volume) = 0 THEN 0 
'												ELSE SUM(A.activity_taj) / sum(A.volume) 
'											  END as allocation_taj
'											, sum(A.activity_taj) as activity_taj
'											, 0 as allocation_tso
'											, 0 as activity_tso
							
							
'										FROM XFC_PLT_RAW_Production_CC A
										
'										LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
'											ON C.id = A.id_costcenter
'											AND C.id_factory = '{factory}'
'											AND A.date BETWEEN C.start_date AND C.end_date						
							
'										GROUP BY A.date, A.id_costcenter, C.id_averagegroup, A.id_product
								
'								UNION ALL
						
'						-- UO2	
'										SELECT 						
'											'{factory}' as id_factory 
'											, date
'											, 'Actual' as scenario 
'											, C.id_averagegroup
'											, '-1' as id_workcenter 
'											, A.id_costcenter 
'											, A.id_product
'											, 'UO2' AS uotype
'											, sum(A.volume) as volume 
'											, AVG(D.value) as allocation_taj
'											, sum(A.volume*D.value) as activity_taj
'											, 0 as allocation_tso
'											, 0 as activity_tso
							
							
'										FROM XFC_PLT_RAW_Production_CC A
										
'										LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
'											ON C.id = A.id_costcenter
'											AND C.id_factory = '{factory}'
'											AND A.date BETWEEN C.start_date AND C.end_date
									
'										LEFT JOIN XFC_PLT_AUX_Production_Actual_Times D
'											ON D.id_factory = '{factory}'
'											AND D.uotype = 'UO2'
'											AND D.id_costcenter = A.id_costcenter
'											AND D.id_averagegroup = C.id_averagegroup
'											AND D.id_product = A.id_product
'											AND A.date BETWEEN D.start_date AND D.end_date
							
'										GROUP BY A.date, A.id_costcenter, C.id_averagegroup, A.id_product						
										
									
'									) as ext
						
'						"
						
					End If
					
					#End Region
					
			#End Region		
			
			#Region "PRODUCTION - Forecast/Budget"					

				#Region "Volumes"
					
					#Region "Volumes General"
				Else If fileNameAlone = "XFC_PLT_PLAN_VolumesInput" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_Production_Planning_Volumes"
					tableDest = "XFC_PLT_AUX_Production_Planning_Volumes"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_averagegroup] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_product] varchar(50) ,
								[product] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"					
					
					sqlMap = $"
					-- 1. Delete previo
					DELETE FROM {tableDest}
					
					WHERE 1=1
						AND scenario = '{scenario}'
						AND id_factory = '{factory}'
						AND YEAR(date) = {year}
						AND MONTH(date) >= {monthFcts}
					
					-- 2. Insert final
					INSERT INTO {tableDest} (scenario, id_factory, id_averagegroup, id_costcenter,id_product, date, value)

					    SELECT 
							'{scenario}' as scenario
							, '{factory}' as id_factory
					        , id_averagegroup
					        , COALESCE(NULLIF(id_costcenter,''), '-1') AS id_costcenter 
					        , id_product
					        , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
					        , SUM(valor) as valor
					    FROM (
					        SELECT id_averagegroup, id_costcenter, id_product, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M02', M02 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M03', M03 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M04', M04 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M05', M05 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M06', M06 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M07', M07 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M08', M08 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M09', M09 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M10', M10 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M11', M11 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, 'M12', M12 FROM {tableName}
					    ) AS expanded	
						
						WHERE 1=1
							AND MONTH(DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01')) >= {monthFcts}
					
						GROUP BY 
							id_averagegroup
							, id_costcenter
							, id_product
							, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01')
					"	
					#End Region
					
					
				#End Region			
			
				#Region "Allocations"
				
				Else If fileNameAlone = "XFC_PLT_PLAN_AllocationsInput" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_Production_Planning_Times"
					tableDest = "XFC_PLT_AUX_Production_Planning_Times"
					
					sqlRaw = $"CREATE TABLE {tableName} 
								(
								[id_averagegroup] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_product] varchar(50) ,
								[uotype] varchar(50) ,
								[M01] decimal(18,9),
								[M02] decimal(18,9),
								[M03] decimal(18,9),
								[M04] decimal(18,9),
								[M05] decimal(18,9),
								[M06] decimal(18,9),
								[M07] decimal(18,9),
								[M08] decimal(18,9),
								[M09] decimal(18,9),
								[M10] decimal(18,9),
								[M11] decimal(18,9),
								[M12] decimal(18,9),
								[R01] decimal(18,9),
								[R02] decimal(18,9),
								[R03] decimal(18,9),
								[R04] decimal(18,9),
								[R05] decimal(18,9),
								[R06] decimal(18,9),
								[R07] decimal(18,9),
								[R08] decimal(18,9),
								[R09] decimal(18,9),
								[R10] decimal(18,9),
								[R11] decimal(18,9),
								[R12] decimal(18,9)
								)"			
					
					sqlMap = $"
					-- 1. Delete previo
					DELETE FROM {tableDest}
					
					WHERE 1=1
						AND id_factory = '{factory}'
						AND (scenario = '{scenario}' OR scenario = '{scenario}_Ref')
						AND YEAR(date) = {year}
						AND uotype In (Select Distinct uotype From {tableName})
						AND MONTH(date) >= {monthFcts} 
					
					-- 2. Insert de datos
					INSERT INTO {tableDest} (scenario, id_factory, date, id_averagegroup, id_costcenter, id_product, uotype, value)
					
					SELECT *					
										
					FROM (
					
					    SELECT DISTINCT
							'{scenario}' as scenario
							, '{factory}' as id_factory
					        , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
							, id_averagegroup
							, COALESCE(NULLIF(id_costcenter,''),'-1') AS id_costcenter
							, id_product
							, uotype
							, valor 
					
					    FROM (
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M01' AS mes, M01 AS valor, R01 as valorRef FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M02', M02, R02 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M03', M03, R03 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M04', M04, R04 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M05', M05, R05 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M06', M06, R06 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M07', M07, R07 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M08', M08, R08 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M09', M09, R09 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M10', M10, R10 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M11', M11, R11 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M12', M12, R12 FROM {tableName}
					    ) AS expanded
						
						GROUP BY 
							DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01')
							, id_averagegroup
							, COALESCE(NULLIF(id_costcenter,''),'-1')
							, id_product
							, uotype
							, valor 
					
					
						UNION ALL
						
						SELECT DISTINCT
						 	'{scenario}_Ref' as scenario
						 	, '{factory}' as id_factory
					        , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
						 	, id_averagegroup
						 	, COALESCE(NULLIF(id_costcenter,''),'-1') AS id_costcenter
						 	, id_product
						 	, uotype
						 	, valorRef
						 
					    FROM (
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M01' AS mes, M01 AS valor, R01 as valorRef FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M02', M02, R02 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M03', M03, R03 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M04', M04, R04 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M05', M05, R05 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M06', M06, R06 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M07', M07, R07 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M08', M08, R08 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M09', M09, R09 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M10', M10, R10 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M11', M11, R11 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_costcenter, id_product, uotype, 'M12', M12, R12 FROM {tableName}
					   	) AS expanded	
					
						GROUP BY 
							DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01')
							, id_averagegroup
							, COALESCE(NULLIF(id_costcenter,''),'-1')
							, id_product
							, uotype
							, valorRef 
					
					) AS grouped_data
					
					WHERE 1=1
						AND MONTH(date) >= {monthFcts} 
					"
					
					' brapi.ErrorLog.LogMessage(si, sqlMap)
				#End Region			
				
				#Region "Insert FCST/BUD into FACTS - TO DO"
				Else If filePath.Contains("Exec - TopFabBudgetProduction") Then
					
					correctFileName = True
					sqlMap = $"
						DELETE FROM XFC_PLT_FACT_Production
							WHERE 1=1
								AND scenario = 'Budget'
								AND id_factory = '{factory}'
								AND YEAR(date) = {year}
					
						INSERT INTO XFC_PLT_FACT_Production (scenario, date, id_factory, id_costcenter, id_averagegroup, id_workcenter, id_product, uotype, volume, allocation_taj, allocation_tso, activity_taj, activity_tso)
							
						SELECT DISTINCT
							'Budget' as scenario
							, A.date
							, A.id_factory 
							, '-1' as id_costcenter
							, A.id_averagegroup
							, '-1' as id_workcenter
							, A.id_product_family
							, CASE 
					      		WHEN uotype = 'UOE1' THEN 'A10'
					     		WHEN uotype = 'UOE2' THEN 'A20'
					     		ELSE 'TBD' 
							END as uotype
							, B.volume AS volume
							, A.allocation_taj as allocation_taj
							, A.allocation_tso as allocation_tso
							, A.allocation_taj * B.volume as ActividadTAJ 
							, A.allocation_tso * B.volume as ActividadTSO
						
						FROM XFC_PLT_AUX_Production_TopFabBudget_Volumes B
						
						INNER JOIN XFC_PLT_AUX_Production_TopFabBudget_Allocations A
							ON A.id_averagegroup = B.id_averagegroup
							AND A.id_product_family = B.id_product_family
							AND A.id_factory = B.id_factory 
							AND MONTH(A.date) = MONTH(B.date)
							AND YEAR(A.date) = YEAR(B.date)
						
						WHERE 1=1
							AND YEAR(A.date) = {year}
							AND A.id_factory = '{factory}'
					"		
				#End Region
				
			#End Region
				
		#End Region				
				
		#Region "COSTS"				
			
			#Region "COSTS (Monthly) - Actual Data"
			
				#Region "ZSYPI"
				
				Else If fileNameAlone = "ZSYPI" Then
					
					tableDest = "XFC_PLT_fact_Costs"
					
					correctFileName = True
					Dim fullPath As String = FileSystemLocation.ApplicationDatabase.ToString & filePath
					Dim contentFileBytes As Byte() = fileInfo.XFFile.ContentFileBytes
					
					Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
					shared_queries.InsertCosts(si, factory, year, month, contentFileBytes)
					
				#Region "Inser into RAW Old"					
'''					correctFileName = True
'''					Dim factoryR As String = String.Empty
'''					Dim entityR As String = String.Empty
'''					Dim replaceDecimals As Boolean = False
'''					Dim colExtra As Boolean = False
					
'''					'BRApi.ErrorLog.LogMessage(si, "factory: " & factory)				
					
'''					Select Case factory 
'''						'Cacia
'''						Case "R0671"
'''							factoryR = "0671"
'''							entityR = "STE"
'''						'Sevilla
'''						Case "R0548913"
'''							factoryR = "1301"
'''							entityR = "003"						
'''						'Valladolid
'''						Case "R0548914"
'''							factoryR = "1301"
'''							entityR = "002"
'''						'Curitiba
'''						Case "R0483003"
'''							factoryR = "1303"
'''							entityR = "%"
'''						'Bursa Meca
'''						Case "R0529002"
'''							factoryR = "1302"
'''							entityR = "%"		
'''							replaceDecimals = True
'''							colExtra = True
'''						'Pitesti Meca
'''						Case "R0045106"
'''							factoryR = "0611"
'''							entityR = "%"	
'''						'Los Andes
'''						Case "R0585"
'''							factoryR = "0585"							
'''							entityR = "%"	
'''							replaceDecimals = True
'''						'Argentina
'''						Case "R0592"
'''							factoryR = "0592"
'''							entityR = "%"
'''							replaceDecimals = True
'''					End Select										
					
'''					Dim fullPath As String = FileSystemLocation.ApplicationDatabase.ToString & filePath
					
'''					' Verificar si el archivo existe
'''					If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath) Then
					
'''						Dim contentFileBytes As Byte() = fileInfo.XFFile.ContentFileBytes
'''						Dim cont As Integer = 0
'''						Dim filteredLines As New List(Of String)
						
'''						' Paso 1: Convertir los bytes a texto
'''						Using ms As New MemoryStream(contentFileBytes)
'''							Dim reader As New StreamReader(ms, Encoding.UTF8)
'''					            ' Leer el archivo línea por línea
'''					            Dim line As String

'''								'BRApi.ErrorLog.LogMessage(si, "Comienza")
								
'''					            While (reader.Peek() >= 0)
'''					                line = reader.ReadLine()	
									
'''									If line.Contains("|") And Not line.Contains("Exerc") And Not line.Contains("Total") Then
''''										If(replaceDecimals)
''''											line = line.Replace(".","").Replace(",",".")
''''										End If	
'''										filteredLines.Add(line)
										
'''									End If

'''					            End While
								
'''								'BRApi.ErrorLog.LogMessage(si, "Finaliza")

'''					    End Using
						
'''						' Paso 4: Unir las líneas filtradas en un solo texto
'''						Dim newContent As String = String.Join(Environment.NewLine, filteredLines)
						
'''						' Paso 5: Convertir el nuevo contenido de texto a bytes
'''						Dim newContentBytes As Byte() = Encoding.UTF8.GetBytes(newContent)
						
						
'''						fieldTokens.Add("xfText#:[dummy1]")
'''						fieldTokens.Add("xfText#:[year]")
'''						fieldTokens.Add("xfText#:[month]")
'''						fieldTokens.Add("xfText#:[id_factory]")
'''						fieldTokens.Add("xfText#:[entity]")
'''						fieldTokens.Add("xfText#:[domFonc]")
'''						fieldTokens.Add("xfText#:[code]")
'''						fieldTokens.Add("xfText#:[rubirc]")
'''						fieldTokens.Add("xfText#:[activity]")
'''						fieldTokens.Add("xfText#:[society]")
'''						fieldTokens.Add("xfText#:[cbl_account]")
						
'''						If(colExtra)
'''							fieldTokens.Add("xfText#:[dummy]::0")
'''						Else
'''							fieldTokens.Add("xfText#:[value_intern]::0")
'''						End If
'''						fieldTokens.Add("xfText#:[value_transact]::0")
'''						If(colExtra)
'''							fieldTokens.Add("xfText#:[value_intern]")
'''						End If	
'''						fieldTokens.Add("xfText#:[dev]")
'''						fieldTokens.Add("xfText#:[documentF]")
'''						fieldTokens.Add("xfText#:[num_cpte]")
'''						fieldTokens.Add("xfText#:[id_costcenter]")
'''						fieldTokens.Add("xfText#:[num_ordre]")
'''						fieldTokens.Add("xfText#:[elm_OTP]")
						
'''						tableName = "XFC_PLT_RAW_CostsMonthly_Raw"	
						
''''						loadMethod = $"Replace:[id_factory = '{factoryR}', entity = '{entityR}', year = '{year}', month = '0{month}']"
''''						loadMethod = "Append"			
'''						loadMethod = "Replace"													
						
'''						Dim sqlRawCreate As String = "
'''						IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='XFC_PLT_RAW_CostsMonthly_Raw' AND xtype='U')
'''						BEGIN
						
'''							CREATE TABLE XFC_PLT_RAW_CostsMonthly_Raw
'''								(
'''								[year] varchar(50) ,
'''								[month] varchar(50) ,
'''								[id_factory] varchar(50) ,
'''								[entity] varchar(50) ,
'''								[domFonc] varchar(50) ,
'''								[code] varchar(50) ,
'''								[rubirc] varchar(50) ,
'''								[activity] varchar(50) ,
'''								[society] varchar(50) ,
'''								[cbl_account] varchar(50) ,
'''								[value_intern] varchar(50) ,
'''								[value_transact] varchar(50) ,
'''								[dev] varchar(50) ,
'''								[documentF] varchar(500) ,
'''								[num_cpte] varchar(50) ,
'''								[id_costcenter] varchar(50) ,
'''								[num_ordre] varchar(50) ,
'''								[elm_OTP] varchar(50) 								
'''								);
'''						END
'''						"					
						
'''						'BRApi.ErrorLog.LogMessage(si, sqlRawCreate)
'''						Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
'''							BRApi.Database.ExecuteSql(oDbConn, sqlRawCreate, False)												
'''						End Using
											
'''						Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, newContentBytes, "|", dbLocation, tableName, loadMethod, fieldTokens, True)					
						
'''						Dim sqlDecimal As String = "
'''							SELECT TOP(1) value_intern FROM XFC_PLT_RAW_CostsMonthly_Raw
'''						"
						
'''						Dim valor As String = String.Empty
'''						Dim caracter As Char = Nothing
'''						Dim formato As String = String.Empty
						
'''						Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)							
'''							 valor = (BRApi.Database.ExecuteSql(oDbConn, sqlDecimal, False))(0)(0)												
'''						End Using
						
'''						If valor.Contains("-") Then
'''							caracter = StrReverse(valor)(3)
'''						Else
'''							caracter = StrReverse(valor)(2)
'''						End If
						
'''						Dim value_intern As String = If(caracter = ",","REPLACE(REPLACE(value_intern, '.', ''),',','.')","REPLACE(value_intern, ',', '')")
'''						Dim value_transact As String = If(caracter = ",","REPLACE(REPLACE(value_transact, '.', ''),',','.')","REPLACE(value_transact, ',', '')")
						
'''						Dim sqlRawTransfer As String = $"
'''							DELETE 
'''							FROM XFC_PLT_RAW_CostsMonthly
'''							WHERE id_factory = '{factoryR}'
'''							AND entity LIKE '{entityR}'
'''							AND year = '{year}'
'''							AND month = '00{month}';
						
'''							INSERT INTO XFC_PLT_RAW_CostsMonthly
'''							([year],[month],[id_factory],[entity],[domFonc],[code],[rubirc],[activity],[society],[cbl_account],[value_intern],[value_transact],[dev],[documentF],[num_cpte],[id_costcenter],[num_ordre],[elm_OTP])
							
'''							SELECT
'''								[year],[month],[id_factory],[entity],[domFonc],[code],[rubirc],[activity],[society],[cbl_account]	
'''								, CAST(
'''						    		CASE 
'''						    		  WHEN RIGHT({value_intern}, 1) = '-' THEN '-' + LEFT({value_intern}, LEN({value_intern}) - 1)
'''						    		  ELSE {value_intern}
'''									END AS DECIMAL(18,2)
'''						  		) AS [value_intern]
'''								,CAST(
'''						    		CASE 
'''						    		  WHEN RIGHT({value_transact}, 1) = '-' THEN '-' + LEFT({value_transact}, LEN({value_transact}) - 1)
'''						    		  ELSE {value_transact}
'''									END AS DECIMAL(18,2)
'''						  		) AS [value_transact]
'''								,[dev],[documentF],[num_cpte],[id_costcenter],[num_ordre],[elm_OTP]
'''							FROM XFC_PLT_RAW_CostsMonthly_Raw
'''							WHERE id_factory = '{factoryR}'
'''								AND entity LIKE '{entityR}';
							
'''							DROP TABLE XFC_PLT_RAW_CostsMonthly_Raw;
'''						"
'''						brapi.ErrorLog.LogMessage(si, sqlRawTransfer)
'''						'BRApi.ErrorLog.LogMessage(si, sqlRawTransfer)
'''						Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
'''							BRApi.Database.ExecuteSql(oDbConn, sqlRawTransfer, False)												
'''						End Using						
						
'''					End If
					
'''					sqlMap = $" 
'''								-- 1- Previous clear
					
'''								DELETE FROM XFC_PLT_RAW_CostsMonthly
'''								WHERE 1=1
'''									AND CAST([month] as INT) = 16					
					
'''								DELETE FROM XFC_PLT_FACT_Costs
'''								WHERE 1=1
'''									AND (id_factory = '{factory}' OR id_factory = 'TBD')
'''									AND scenario = 'Actual'
'''									AND YEAR(date) = {year}						
'''									AND MONTH(date) = {month}
					
'''								-- 2- Insert statement				
					
'''								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_factory, date, id_account, id_rubric, id_costcenter, value, currency)
					
'''								SELECT *
					
'''								FROM (     
'''									SELECT  
'''										'Actual' as scenario
'''									    , id_factory_map AS id_factory
'''									    , DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date
'''									    , mng_account as id_account
'''										, rubirc
'''									    , id_costcenter as id_costcenter
'''									    , SUM(value_intern) as value
'''									    , 'Local' as currency
					
'''									FROM (
					
'''									      		SELECT A.*
'''														, COALESCE(B.mng_account,'Others') as mng_account
					
'''								                FROM (	SELECT *, 
'''															CASE
'''												                WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
'''												                WHEN CONCAT('R',id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
'''												                WHEN CONCAT('R',id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
'''												                WHEN CONCAT('R',id_factory, entity) = 'R1301003' THEN 'R0548913'
'''												                WHEN CONCAT('R',id_factory, entity) = 'R1301002' THEN 'R0548914'
'''												                WHEN CONCAT('R',id_factory, entity) LIKE '%R0585%' THEN 'R0585'
'''												                WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
'''												                WHEN CONCAT('R',id_factory, entity) LIKE '%R0592%' THEN 'R0592'
'''												                ELSE 'TBD' END AS id_factory_map
								
'''														FROM XFC_PLT_RAW_CostsMonthly) A
					
					
'''									            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
'''									             	ON A.id_costcenter = C.id
'''									          		AND C.scenario = 'Actual'
'''									          		AND DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) BETWEEN C.start_date AND C.end_date
															
'''												LEFT JOIN (
'''															SELECT DISTINCT costcenter_type
'''															FROM XFC_PLT_Mapping_Accounts_docF 
'''														) as E
'''													ON C.type = E.costcenter_type	
												
'''												LEFT JOIN (
'''															SELECT DISTINCT 
'''																cnt_account
'''																, mng_account
'''																, costcenter_type
'''																, costcenter_nature
'''																, docF
					
'''															FROM XFC_PLT_Mapping_Accounts_docF 
'''														) as B					
'''									       			ON A.num_cpte = B.cnt_account
'''													AND C.type = B.costcenter_type
'''													AND C.nature = B.costcenter_nature
'''													AND A.domFonc = B.docF	

'''									      		WHERE 1=1
'''													AND CAST(A.[month] as INT) = {month}										
'''													AND A.year = {year}	
'''												    AND id_factory_map = '{factory}'					
					
'''									      		)  AS ext
										
'''									      GROUP BY id_factory_map, [year], [month], mng_account, rubirc, id_costcenter
					
'''									) AS mapeo	
					
'''									WHERE 1=1
											
'''									"
				#End Region	
				
				#End Region
				
				#Region "SAP4H - Aveiro - KSB1"
				Else If fileNameAlone = "KSB1" And factory = "R0671" And CInt(month) >= 7 And scenario = "Actual"  Then
					
					Dim factoryR As String = String.Empty
					Dim	entityR  As String = String.Empty
					Dim	companyR As String = String.Empty
					
					If factory = "R0671" Then
						factoryR = "0671"
						entityR = "STE"
						companyR = "PT10"
					End If
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_KSB1_Raw"	
					tableDest = "XFC_PLT_RAW_CostsMonthly"	
					
					sqlRaw = $"
							DROP TABLE IF EXISTS XFC_PLT_RAW_KSB1_Raw;
							DROP TABLE IF EXISTS XFC_PLT_RAW_KSB1;
					
							CREATE TABLE XFC_PLT_RAW_KSB1_Raw
								(
								[year] datetime,  
								[month] integer,  
								[company_id] varchar(50),  
								[account_name] varchar(50) ,
								[costcenter_id] varchar(50) ,
								[profitcenter_id] varchar(50) ,
								[business_partner_id] varchar(50) ,
								[order_id] varchar(50) ,
								[product_id] varchar(50) ,
								[amount] decimal(18,9)
								);
					
							CREATE TABLE XFC_PLT_RAW_KSB1
								(
								[year] 					datetime,  
								[month] 				integer,  
								[company_id] 			varchar(50),  
								[account_name] 			varchar(50) ,
								[costcenter_id] 		varchar(50) ,
								[profitcenter_id] 		varchar(50) ,
								[business_partner_id] 	varchar(50) ,
								[order_id] 				varchar(50) ,
								[product_id] 			varchar(50) ,
								[amount] 				decimal(18,9)
								);"
																		
					sqlMap = $"	
											
								
								-- 0 - Insert Raw -> PnL Format
								INSERT INTO XFC_PLT_RAW_KSB1 (
									[year] 				
									, [month] 			
									, [company_id] 		
									, [account_name] 		
									, [costcenter_id] 	
									, [profitcenter_id] 	
									, [business_partner_id]
									, [order_id] 			
									, [product_id] 		
									, [amount] 		
								)
					
								SELECT 
									--YEAR(year) 				as [year] 				
									2025 						as [year] 
									, [month] 					as month 			
									, '{companyR}' 				as [company_id] 		
									, account_name 				as [account_name] 		
									, [costcenter_id] 			as [costcenter_id] 	
									, NULL 						as [profitcenter_id]
									, NULL 						as [business_partner_id]
									, NULL 						as [order_id] 			
									, NULL 						as [product_id] 		
									, SUM([amount]) 			as amount 		
					
								FROM XFC_PLT_RAW_KSB1_Raw 
								WHERE 1=1
									AND YEAR(year) = {year}									
									AND month = {month}	
									AND ISNULL(account_name,'') <> ''
								GROUP BY month, YEAR(year), account_name, costcenter_id ;
					
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_RAW_CostsMonthly
								WHERE 1=1
									--AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND year = {year}
									AND num_cpte IN (SELECT DISTINCT account_name FROM XFC_PLT_RAW_KSB1);
								
								-- 2- Insert statement Raw PnL -> Raw
		
								INSERT INTO XFC_PLT_RAW_CostsMonthly
									([year]
									,[month]
									,[id_factory]
									,[entity]
									,[domFonc]
									,[code]
									,[rubirc]
									,[activity]
									,[society]
									,[cbl_account]
									,[value_intern]
									,[value_transact]
									,[dev]
									,[documentF]
									,[num_cpte]
									,[id_costcenter]
									,[num_ordre]
									,[elm_OTP])
								
								SELECT
								    2025
								    ,F.month
								    ,'{factoryR}' AS [id_factory]
								    ,'{entityR}' AS [entity]
									,NULL AS [domFonc]
									,NULL AS [code]
									--,ISNULL(R2.conso_rubric,'-1') AS [rubirc]
									,'-1' AS [rubirc]
									,NULL AS [activity]
									,NULL AS [society]
									,NULL AS [cbl_account]	
									,F.amount AS [value_intern]
									,F.amount AS [value_transact]
									,NULL AS [dev]		
									,NULL AS [documentF]	
								
									,CASE 
										WHEN R.account_name IS NOT NULL THEN 
											CASE WHEN M.cnt_account IS NULL AND A.name_old IS NOT NULL THEN F.account_name + ' - ' + A.name_old + ' - No Mapping Mng Account'
											ELSE ISNULL(A.name_old, F.account_name + ' - No Mapping') 
											END 
										ELSE F.account_name + ' - Ignore' 
										END AS [num_cpte]	
								
								    ,CASE 
										WHEN F.costcenter_id = 'None' THEN NULL
										WHEN F.costcenter_id IS NOT NULL THEN ISNULL(C.id_old, F.costcenter_id + ' - No Mapping') 
										END AS [id_costcenter]
									
									,NULL AS [num_ordre]
									,NULL AS [elm_OTP]
								
								    --,F.profit_center_id
								    --,F.business_partner_id
								
								FROM XFC_PLT_RAW_KSB1 F
								
								LEFT JOIN XFC_MAIN_MASTER_AccountsOldToNew A
									ON A.name = F.account_name		
								
								LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew C
									ON C.id = F.costcenter_id
								
					            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C2
					             	ON C2.id = C.id_old
					          		AND C2.scenario = 'Actual'
					          		AND DATEFROMPARTS({year},{month}, 1) BETWEEN C2.start_date AND C2.end_date						
								
								LEFT JOIN (SELECT DISTINCT account_name FROM XFC_MAIN_MASTER_AccountRubrics) R
									ON R.account_name = F.account_name
								
								--LEFT JOIN (SELECT account_name, cost_center_class_id, conso_rubric FROM XFC_MAIN_MASTER_AccountRubrics) R2
								--	ON R2.account_name = F.account_name
								--	AND R2.cost_center_class_id = C2.type
								
								LEFT JOIN (SELECT DISTINCT cnt_account FROM XFC_PLT_MAPPING_Accounts_DocF) M
									ON A.name_old = M.cnt_account		
								
								WHERE F.company_id = '{companyR}'
											
							"
					
'							BRApi.ErrorLog.LogMessage(si,sqlMap)

							Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
							shared_queries.InsertCostsS4H_KSB1(si, factory, year, month)
							
					#End Region
				
			#End Region				
			
			#Region "Allocation Keys"
			
				Else If fileNameAlone = $"XFC_PLT_COST_KeyAllocations" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_CostsAllocations"	
					tableDest = "XFC_PLT_AUX_AllocationKeys"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[costnature] varchar(50) ,
								[M01] decimal(18,9),
								[M02] decimal(18,9),
								[M03] decimal(18,9),
								[M04] decimal(18,9),
								[M05] decimal(18,9),
								[M06] decimal(18,9),
								[M07] decimal(18,9),
								[M08] decimal(18,9),
								[M09] decimal(18,9),
								[M10] decimal(18,9),
								[M11] decimal(18,9),
								[M12] decimal(18,9)
								)"
																		
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
						
					
								-- 2- Insert statement
													
								INSERT INTO {tableDest} (scenario, id_factory, id_costcenter, id_averagegroup, costnature, date, percentage)
								SELECT
								    '{scenario}' as scenario
									, '{factory}' As factory
								    , id_costcenter
								    , id_averagegroup
								    , costnature
								    , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS percentage
								FROM
								    (
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M01' AS mes, M01 AS valor FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M02', M02 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M03', M03 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M04', M04 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M05', M05 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M06', M06 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M07', M07 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M08', M08 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M09', M09 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M10', M10 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M11', M11 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M12', M12 FROM XFC_PLT_RAW_CostsAllocations
								    ) AS expanded	
												
								WHERE 1=1
									AND {monthFilterCast} 	
									AND valor <> 0
							"
		
			#End Region
			
			#Region "Fixed Variable Costs"
			
				Else If fileNameAlone = $"XFC_PLT_COST_FixedVariable" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_CostsFixedVariable"	
					tableDest = "XFC_PLT_AUX_FixedVariableCosts"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
								[id_account] varchar(50) ,
								[costnature] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
						
					
								-- 2- Insert statement
																					
								INSERT INTO {tableDest} (scenario, id_factory, id_account, costnature, date, value)
								SELECT
								    '{scenario}' as scenario
									, '{factory}' As factory
								    , id_account
									, CASE WHEN costnature = 'Non affecté' 
										THEN -1
										ELSE costnature
									END AS costnature
								    , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS percentage
								FROM
								    (
								        SELECT scenario, id_account, costnature, 'M01' AS mes, M01 AS valor FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT scenario, id_account, costnature, 'M12', M12 FROM {tableName}
								    ) AS expanded	
					
								WHERE 1=1
									AND {monthFilterCast}
					
								-- 3- Copy next months
					
								DELETE FROM {tableDest} 
								WHERE 1=1
									AND scenario = '{scenario}' AND '{scenario}' = 'Actual' 
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND MONTH(date) > {month};
					
								WITH Months AS (
						            SELECT DATEFROMPARTS({year}, {month}, 1) AS date, {month} as month_start  
						            UNION ALL
						            SELECT DATEADD(MONTH, 1, date) as date, month_start + 1 as month_start
						            FROM Months
						            WHERE MONTH(date) >= {month}
						        )								

								INSERT INTO {tableDest} (scenario, id_factory, id_account, costnature, date, value)
								SELECT scenario, id_factory, id_account, costnature, B.date, value
								FROM {tableDest} A
								CROSS JOIN (SELECT * FROM Months WHERE MONTH(date) <> {month} AND YEAR(date) = {year}) B
								WHERE 1=1
									AND scenario = '{scenario}' AND '{scenario}' = 'Actual' 
									AND id_factory = '{factory}'
									AND YEAR(A.date) = {year}
									AND MONTH(A.date) = {month}					
							"
					
'					brapi.ErrorLog.LogMessage(si, sqlMap)
		
			#End Region		
			
			#Region "Effects Analysis"
			
				Else If fileNameAlone = $"XFC_PLT_COST_Effects" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_EffectsAnalysis"	
					tableDest = "XFC_PLT_AUX_EffectsAnalysis"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ref_scenario] varchar(50) ,
								[cost_type] varchar(50) ,
								[id_indicator] varchar(50) ,
								[variability] varchar(50) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
						
					
								-- 2- Insert statement
					
								INSERT INTO {tableDest} (ref_scenario, cost_type, id_indicator, variability, date, value, id_factory, scenario)
								SELECT
									CASE WHEN ref_scenario = 'BUD' THEN 'Escenario Budget Mal Indicado'
										 WHEN ref_scenario = 'R-1' THEN 'Actual-1' 
										 ELSE ref_scenario END AS ref_scenario
									, cost_type
									, id_indicator
									, variability
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
									, '{scenario}' as scenario
								FROM
								    (
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M02', M02 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M03', M03 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M04', M04 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M05', M05 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M06', M06 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M07', M07 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M08', M08 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M09', M09 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M10', M10 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M11', M11 FROM {tableName} UNION ALL
										SELECT ref_scenario, cost_type, id_indicator, variability, 'M12', M12 FROM {tableName}
								    ) AS expanded	
					
								WHERE 1=1
									AND {monthFilterCast}
							"		
			#End Region			
			
			#Region "Energy Variance"
			
				Else If filePath.Contains($"XFC_PLT_COST_Energy") Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_EnergyVariance"	
					tableDest = "XFC_PLT_AUX_EnergyVariance"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
								[energy_type] varchar(50) ,
								[indicator] varchar(50) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
						
					
								-- 2- Insert statement
																					
								INSERT INTO {tableDest} (scenario, energy_type, indicator, date, value, id_factory)
								SELECT
								    '{scenario}' as scenario
									, energy_type
									, indicator
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
								FROM
								    (
										SELECT scenario, energy_type, indicator, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M02', M02 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M03', M03 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M04', M04 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M05', M05 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M06', M06 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M07', M07 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M08', M08 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M09', M09 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M10', M10 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M11', M11 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1
									AND {monthFilterCast}				
							"

			#End Region			
			
			#Region "StartUp Costs"
			
				Else If filePath.Contains($"XFC_PLT_COST_StartUp") Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_StartUpCosts"	
					tableDest = "XFC_PLT_AUX_Projects"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ProjectCode] varchar(50) NOT NULL,
								[Offer] varchar(50) ,
								[Resonsibility] varchar(50) ,
								[CostType] varchar(50) ,
								[ProjectTask] varchar(50) NOT NULL,
								[ProjecttaskText] varchar(50) ,
								[Currency] varchar(50) ,
								[Capitalizable] varchar(20) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
									AND project_file = 'startup'
						
					
								-- 2- Insert statement
																					
								INSERT INTO {tableDest} (scenario, id_project_code, offer, responsibility, cost_type, project_task, project_task_text, currency, capitalizable, date, value, id_factory, project_file)
								SELECT
								    '{scenario}' as scenario
									, ProjectCode
									, Offer
									, Resonsibility
									, CostType
									, ProjectTask
									, ProjecttaskText
									, Currency
									, Capitalizable
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
									, 'startup' As project_file
								FROM
								    (
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M02', M02 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M03', M03 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M04', M04 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M05', M05 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M06', M06 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M07', M07 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M08', M08 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M09', M09 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M10', M10 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M11', M11 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1
									AND {monthFilterCast}					
							"

			#End Region	
			
			#Region "StartUp Costs - Planning"
			
				Else If filePath.Contains($"XFC_PLT_PLAN_StartUp") Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_StartUpCosts"	
					tableDest = "XFC_PLT_AUX_Projects"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ProjectCode] varchar(50) NOT NULL,
								[Offer] varchar(50) ,
								[Resonsibility] varchar(50) ,
								[CostType] varchar(50) NOT NULL,
								[ProjectTask] varchar(50) NOT NULL,
								[ProjecttaskText] varchar(50) ,
								[Currency] varchar(50) ,
								[Capitalizable] varchar(50) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND project_file = 'startup'
						
					
								-- 2- Insert statement
																					
								INSERT INTO {tableDest} (scenario, id_project_code, offer, responsibility, cost_type, project_task, project_task_text, currency, capitalizable, date, value, id_factory, project_file)
								SELECT
								    '{scenario}' as scenario
									, ProjectCode
									, Offer
									, Resonsibility
									, CostType
									, ProjectTask
									, ProjecttaskText
									, Currency
									, Capitalizable
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
									, 'startup' As project_file
								FROM
								    (
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M02', M02 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M03', M03 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M04', M04 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M05', M05 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M06', M06 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M07', M07 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M08', M08 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M09', M09 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M10', M10 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M11', M11 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, Capitalizable, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1				
							"

			#End Region	
		
			#Region "RampUp Costs"
			
				Else If filePath.Contains($"XFC_PLT_COST_RampUp") Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_RampUpCosts"	
					tableDest = "XFC_PLT_AUX_Projects"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ProjectCode] varchar(50) NOT NULL,
								[Offer] varchar(50) ,
								[Resonsibility] varchar(50) ,
								[CostType] varchar(50) NOT NULL,
								[ProjectTask] varchar(50) NOT NULL,
								[ProjecttaskText] varchar(50) ,
								[Currency] varchar(50) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
									AND project_file = 'rampup'
						
					
								-- 2- Insert statement
																					
								INSERT INTO {tableDest} (scenario, id_project_code, offer, responsibility, cost_type, project_task, project_task_text, currency, date, value, id_factory, project_file)
								SELECT
								    '{scenario}' as scenario
									, ProjectCode
									, Offer
									, Resonsibility
									, CostType
									, ProjectTask
									, ProjecttaskText
									, Currency
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
									, 'rampup' as project_file
								FROM
								    (
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M02', M02 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M03', M03 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M04', M04 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M05', M05 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M06', M06 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M07', M07 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M08', M08 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M09', M09 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M10', M10 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M11', M11 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1
									AND {monthFilterCast}					
							"

			#End Region	
			
			#Region "RampUp Costs - Planning"
			
				Else If filePath.Contains($"XFC_PLT_PLAN_RampUp") Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_RampUpCosts"	
					tableDest = "XFC_PLT_AUX_Projects"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ProjectCode] varchar(50) NOT NULL,
								[Offer] varchar(50) ,
								[Resonsibility] varchar(50) ,
								[CostType] varchar(50) NOT NULL,
								[ProjectTask] varchar(50) NOT NULL,
								[ProjecttaskText] varchar(50) ,
								[Currency] varchar(50) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND project_file = 'rampup'
						
					
								-- 2- Insert statement
																					
								INSERT INTO {tableDest} (scenario, id_project_code, offer, responsibility, cost_type, project_task, project_task_text, currency, date, value, id_factory, project_file)
								SELECT
								    '{scenario}' as scenario
									, ProjectCode
									, Offer
									, Resonsibility
									, CostType
									, ProjectTask
									, ProjecttaskText
									, Currency
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
									, 'rampup' as project_file
								FROM
								    (
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M02', M02 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M03', M03 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M04', M04 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M05', M05 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M06', M06 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M07', M07 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M08', M08 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M09', M09 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M10', M10 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M11', M11 FROM {tableName} UNION ALL
										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1				
							"

			#End Region	
			
			#Region "COSTS (Monthly) - Forecast/Budget"
				
			#Region "COSTS (Monthly) - Forecast/Budget - Insert All"
			
				Else If fileNameAlone = "XFC_PLT_PLAN_CostsInput" Then
										
					correctFileName = True
					tableName = "XFC_PLT_RAW_CostsInput"
					tableDest = "XFC_PLT_FACT_Costs"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_account] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"

					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")
					
					sqlMap = $"	
								DELETE FROM {tableDest}
								WHERE 1=1
									AND id_factory = '{factory}'
									AND scenario = '{scenario}'
									AND YEAR(date) = {year}
									AND MONTH(date) >= {monthFcts}
						
								INSERT INTO {tableDest} (id_account, id_costcenter, date, value, id_factory, scenario)
								
								SELECT
								    id_account
								    , id_costcenter
								    , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
									, '{scenario}' As scenario
								FROM
								    (
								        SELECT id_account, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded	
					
								WHERE 1=1
									AND MONTH(DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01')) >= {monthFcts}
									AND valor <> 0
							"						
				#End Region
				
				#Region "COSTS (Monthly) - Forecast/Budget - Insert some lines"
			
				Else If fileNameAlone = "XFC_PLT_PLAN_CostsInput_Lines" Then
										
					correctFileName = True
					tableName = "XFC_PLT_RAW_CostsInput"
					tableDest = "XFC_PLT_FACT_Costs"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_account] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"

					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")
					
					sqlMap = $"						
								INSERT INTO {tableDest} (id_account, id_costcenter, date, value, id_factory, scenario)
								
								SELECT
								    id_account
								    , id_costcenter
								    , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
									, '{scenario}' As scenario
								FROM
								    (
								        SELECT id_account, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded	
					
								WHERE 1=1
									AND MONTH(DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01')) >= {monthFcts}
									AND valor <> 0
							"						
				#End Region
			
			#End Region		
			
		#End Region	
		
		#Region "HR"
		
			#Region "Time of Presence"
				Else If fileNameAlone = $"XFC_PLT_HR_TimePresence" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_TimePresence"	
					tableDest = "XFC_PLT_AUX_TimePresence"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[wf_type] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[dummy3] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_TimePresence
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
						
					
								-- 2- Insert statement					
					
								INSERT INTO XFC_PLT_AUX_TimePresence (wf_type, id_costcenter, date, value, id_factory, scenario, shift)
								SELECT
								    wf_type,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory,
									'{scenario}' As scenario,
									shift
								FROM
								    (
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, dummy3 AS shift, 'M12', M12 FROM {tableName}
								    ) AS expanded
					
								WHERE 1=1
									AND {monthFilterCast}
							"			
			#End Region
			
			#Region "Daily Hours"
				Else If fileNameAlone = $"XFC_PLT_HR_DailyHours_PCT" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_DailyHours_Stored"	
					tableDest = "XFC_PLT_AUX_DailyHours_Stored"	
					
					Dim indicatorsList() As String = {
							"collective_day_off",	"extra_hours",					"illness_and_accident",
							"individual_days_off",	"internal_transfert_in",		"internal_transfert_out",
							"not_employed",			"paid_not_working",				"public_holiday",
							"strike",				"theoretical_working_hours",	"training",
							"unemployment",			"unpaid_not_working"
					}
					
					
					sqlRaw = $"
							    wf_type 						VARCHAR(50)
							    , id_costcenter 				VARCHAR(50)
							    , Shift 						VARCHAR(50)
							    , date 							VARCHAR(50)
					"
					Dim numInd As String = 0								
					For Each sIndicator As String In indicatorsList
						
						sqlRaw = $"
							{sqlRaw}
							, {sIndicator} DECIMAL(18,2)
						"
						
						If indicatorsList.Count-1 = numInd
							sqlMap = $"
							
									{sqlMap}
							
									SELECT wf_type, id_costcenter, shift, CAST(date AS DATE) AS date, {sIndicator} AS value, '{sIndicator}' AS id_indicator
							        FROM {tableName}
							        WHERE {monthFilterCastDate}
							"
						Else
							sqlMap = $"
							
									{sqlMap}
							
									SELECT wf_type, id_costcenter, shift, CAST(date AS DATE) AS date, {sIndicator} AS value, '{sIndicator}' AS id_indicator
							        FROM {tableName}
							        WHERE {monthFilterCastDate}
							
									UNION ALL
							
							"
							
							numInd = numInd + 1
						End If
						
					Next
					
					BRApi.ErrorLog.LogMessage(si,sqlMap)
					
					sqlRaw = $"
					CREATE TABLE {tableName}
					(
						{sqlRaw}
					)"
					
					
					sqlMap = $"
						    -- 1- Previous clear
						    DELETE FROM {tableDest}
						    WHERE 1=1
						        AND scenario = '{scenario}'
						        AND id_factory = '{factory}'
						        AND YEAR(date) = {year}
						        AND {monthFilter};
						
						    -- 2- Insert statement
						    INSERT INTO {tableDest} (wf_type, id_costcenter, shift, date, value, id_factory, scenario, id_indicator)
						    SELECT 
						        wf_type,
						        id_costcenter,
								shift,
						        date,
						        value,
						        '{factory}' AS id_factory,
						        '{scenario}' AS scenario,
						        id_indicator
						    FROM (
									{sqlMap}
						    ) AS expanded
					
						WHERE 1=1
							AND value <> 0
						"
					
				Else If fileNameAlone = $"XFC_PLT_HR_DailyHours" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_DailyHours"	
					
					sqlRaw = $"	CREATE TABLE {tableName} 
							(
							    wf_type VARCHAR(50),
							    id_costcenter VARCHAR(50),
							    Shift VARCHAR(50),
							    date VARCHAR(50),
							    daily_working_hours DECIMAL(18,2),
							    unemployment DECIMAL(18,2),
							    gift_closing_hours DECIMAL(18,2),
							    collective_leave DECIMAL(18,2),
							    extra_hours DECIMAL(18,2),
							    no_or_suspended_contract DECIMAL(18,2),
							    training DECIMAL(18,2),
							    sick_leave DECIMAL(18,2),
							    paid_non_working_hours DECIMAL(18,2),
							    labor_relation_hours DECIMAL(18,2),
							    bank_of_hours DECIMAL(18,2),
							    days_off DECIMAL(18,2),
							    strike DECIMAL(18,2),
							    not_paid_non_working_hours DECIMAL(18,2),
							    holiday DECIMAL(18,2)
							)"
													
					
					#Region "Mapeo Largo"
					sqlMap = $"
						    -- 1- Previous clear
						    DELETE FROM XFC_PLT_AUX_DailyHours
						    WHERE 1=1
						        AND scenario = '{scenario}'
						        AND id_factory = '{factory}'
						        AND YEAR(date) = {year}
						        AND {monthFilter};
						
						    -- 2- Insert statement
						    INSERT INTO XFC_PLT_AUX_DailyHours (wf_type, id_costcenter, date, value, id_factory, scenario, id_indicator)
						    SELECT 
						        wf_type,
						        id_costcenter,
						        date,
						        value,
						        '{factory}' AS id_factory,
						        '{scenario}' AS scenario,
						        id_indicator
						    FROM (
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, daily_working_hours AS value, 'daily_working_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, unemployment AS value, 'unemployment' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, gift_closing_hours AS value, 'gift_closing_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, collective_leave AS value, 'collective_leave' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, extra_hours AS value, 'extra_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, no_or_suspended_contract AS value, 'no_or_suspended_contract' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE (MONTH(CAST(date AS DATE)) = {month} OR '{scenario}'<>'Actual')
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, training AS value, 'training' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, sick_leave AS value, 'sick_leave' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, paid_non_working_hours AS value, 'paid_non_working_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, labor_relation_hours AS value, 'labor_relation_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, bank_of_hours AS value, 'bank_of_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, days_off AS value, 'days_off' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, strike AS value, 'strike' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, not_paid_non_working_hours AS value, 'not_paid_non_working_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, holiday AS value, 'holiday' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE  {monthFilterCastDate}
						    ) AS expanded
					
						WHERE 1=1
							AND value <> 0
						"

					#End Region
								
			#End Region	
			
			#Region "Workforce"
				Else If fileNameAlone = $"XFC_PLT_HR_Workforce" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_Workforce"	
					tableDest = "XFC_PLT_AUX_Workforce"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[wf_type] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_function]  decimal(18,0),
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_Workforce
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
						
					
								-- 2- Insert statement	
					
					
								INSERT INTO XFC_PLT_AUX_Workforce (wf_type, id_function, id_costcenter, date, value, id_factory, scenario)
								SELECT
								    wf_type,
								    id_function,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory,
									'{scenario}' As scenario
								FROM
								    (
								        SELECT wf_type, id_function, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
					
								WHERE 1=1
									AND {monthFilterCast}				
							"			
			#End Region	
			
			#Region "Balancing"
				Else If fileNameAlone = $"XFC_PLT_HR_Balancing" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_Balancing"	
					tableDest = "XFC_PLT_AUX_Balancing"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[wf_type] varchar(50) ,
								[bal_code] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_Balancing
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
						
					
								-- 2- Insert statement									
								INSERT INTO XFC_PLT_AUX_Balancing (wf_type, bal_code, id_costcenter, date, value, id_factory, scenario)
								SELECT
								    wf_type,
								    bal_code,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory,
									'{scenario}' As scenario
								FROM
								    (
								        SELECT wf_type, bal_code, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded
					
								WHERE 1=1
									AND {monthFilterCast}						
							"			
			#End Region		
			
			#Region "Calendar"
			
				Else If fileNameAlone = $"XFC_PLT_HR_Calendar" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_Calendar"
					tableDest = "XFC_PLT_AUX_Calendar"
					
					sqlRaw = $"	CREATE TABLE {tableName} 
							(
							    templateID VARCHAR(50),
							    description VARCHAR(50),
							    date VARCHAR(50),
							    JTNO DECIMAL(18,2),
							    SECO DECIMAL(18,2),
							    SESU DECIMAL(18,2),
							    ALHO DECIMAL(18,2),
							    JNTC DECIMAL(18,2),
							    JNTA_WE DECIMAL(18,2),
							    JNTA_FERIE DECIMAL(18,2),
							    JNTA_CONGE DECIMAL(18,2),
							    JNTA_FRANCHISE DECIMAL(18,2)
							)"
													
					
					#Region "Mapeo Largo"
					sqlMap = $"
						    -- 1- Previous clear
						    DELETE FROM XFC_PLT_AUX_Calendar
						    WHERE 1=1
						        AND scenario = '{scenario}'
						        AND id_factory = '{factory}'
						        AND YEAR(date) = {year}
						        AND {monthFilter};
						
						    -- 2- Insert statement
						    INSERT INTO XFC_PLT_AUX_Calendar (id_template, id_costcenter, id_indicator, date, value, id_factory, scenario)
					
						    SELECT 
						        templateID,
								id_costcenter,					
						        id_indicator,
						        date,
						        value,
						        '{factory}' AS id_factory,
						        '{scenario}' AS scenario
					
						    FROM (
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, JTNO AS value, 'JTNO' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
						
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, SECO AS value, 'SECO' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
					
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, SESU AS value, 'SESU' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
					
					
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, ALHO AS value, 'ALHO' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
					
					
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, JNTC AS value, 'JNTC' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
					
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, JNTA_WE AS value, 'JNTA_WE' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
					
					
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, JNTA_FERIE AS value, 'JNTA_FERIE' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
								 		
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, JNTA_CONGE AS value, 'JNTA_CONGE' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}
					
								UNION ALL
					
						        SELECT templateID, description AS id_costcenter, CONVERT(DATE, date, 112) AS date, JNTA_FRANCHISE AS value, 'JNTA_FRANCHISE' AS id_indicator
						        FROM {tableName}
						        WHERE {monthFilterConvertDate}			
					
						    ) AS expanded
					
						WHERE 1=1
							AND value <> 0
						"
					
					#End Region
					
			#End Region	
		
			#Region "Additional Info"
			
				Else If fileNameAlone = $"XFC_PLT_HR_AdditionalInfo" Then
					
					correctFileName = True
					tableName = "XFC_PLT_RAW_HRAdditionalInfo"
					tableDest = "XFC_PLT_AUX_HRAdditionalInfo"
					
					sqlRaw = $"	CREATE TABLE {tableName} 
							(
							    date VARCHAR(50),
							    wf_type VARCHAR(50),
								surplus_unemployement_costs DECIMAL(18,2),
							    surplus_unemployement_headcount DECIMAL(18,2)
							)"
																	
					sqlMap = $"
						    -- 1- Previous clear
						    DELETE FROM XFC_PLT_AUX_HRAdditionalInfo
						    WHERE 1=1
						        AND scenario = '{scenario}'
						        AND id_factory = '{factory}'
						        AND YEAR(date) = {year}
						        AND {monthFilter};
						
						    -- 2- Insert statement
						    INSERT INTO XFC_PLT_AUX_HRAdditionalInfo (scenario, id_factory, date, wf_type, surplus_unemployement_costs, surplus_unemployement_headcount)
					
						    SELECT 
								'{scenario}' AS scenario, 
								'{factory}' AS id_factory, 
								CONVERT(DATE, date, 112)  as date, 
								wf_type, 
								surplus_unemployement_costs, 
								surplus_unemployement_headcount
					
						    FROM {tableName}
						    WHERE {monthFilterConvertDate}			
							AND (surplus_unemployement_costs <> 0
							OR surplus_unemployement_headcount <> 0)
						"				
					
			#End Region				
						
			#Region "Headcount (Plan)"
			
				Else If fileNameAlone = $"XFC_PLT_PLAN_Headcount" Then										
					
					correctFileName = True
					tableName = "XFC_MAIN_RAW_Headcount"	
					tableDest = "XFC_MAIN_FACT_Headcount"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_costcenter] varchar(50) ,
								[job_category] varchar(50) ,
								[global_function_organization] varchar(200) ,
								[M01] decimal(18,9),
								[M02] decimal(18,9),
								[M03] decimal(18,9),
								[M04] decimal(18,9),
								[M05] decimal(18,9),
								[M06] decimal(18,9),
								[M07] decimal(18,9),
								[M08] decimal(18,9),
								[M09] decimal(18,9),
								[M10] decimal(18,9),
								[M11] decimal(18,9),
								[M12] decimal(18,9)
								)"
					
					Dim dFactory As New Dictionary(Of String, String) From {
				        {"R0585"	,"0585"},
						{"R0671" 	,"0671"},
						{"R0592"	,"0592"},
						{"R0045106"	,"0611"},
						{"R0529002"	,"1302"},
						{"R0483003"	,"1303"},
						{"R0548913"	,"1301"},					
						{"R0548914"	,"1301"}						
					}
					
					Dim factoryMap As String = dFactory(factory)									
																		
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = '{scenario}'
									AND year = {year}
									AND company_id = '{factoryMap}'
									AND costcenter_id IN (SELECT DISTINCT id_costcenter FROM {tableName});
					
								-- 2- Insert statement
					
								INSERT INTO XFC_MAIN_FACT_Headcount 
									( scenario
									, year
									, month
									, company_id
									, company_desc
									, employee_id
									, employee_name
									, costcenter_id
									, job_category
									, location
									, location_country
									, gender
									, HR_cluster
									, shared_services_center
									, global_function_organization
									, sup_org_from_top_1
									, sup_org_from_top_2
									, sup_org_from_top_3
									, sup_org_from_top_4
									, sup_org_from_top_5
									, sup_org_from_top_6
									, headcount)
				
								SELECT
								    '{scenario}' as scenario
									, {year} AS year
									, mes AS month
									, '{factoryMap}' AS company_id
									, '' AS company_desc
									, '' AS employee_id
									, '' AS employee_name
									, id_costcenter AS costcenter_id
									, job_category
									, '' AS location
									, '' AS location_country
									, '' AS gender
									, '' AS HR_cluster
									, '' AS shared_services_center
									, '' AS global_function_organization
									, '' AS sup_org_from_top_1
									, '' AS sup_org_from_top_2
									, '' AS sup_org_from_top_3
									, '' AS sup_org_from_top_4
									, '' AS sup_org_from_top_5
									, '' AS sup_org_from_top_6
									, headcount AS headcount
								FROM
								    (
								        SELECT id_costcenter, job_category, global_function_organization, 01 AS mes, M01 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 02 AS mes, M02 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 03 AS mes, M03 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 04 AS mes, M04 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 05 AS mes, M05 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 06 AS mes, M06 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 07 AS mes, M07 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 08 AS mes, M08 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 09 AS mes, M09 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 10 AS mes, M10 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 11 AS mes, M11 AS headcount FROM {tableName} UNION ALL
								        SELECT id_costcenter, job_category, global_function_organization, 12 AS mes, M12 AS headcount FROM {tableName}
								    ) AS expanded	
												
								WHERE 1=1
									AND headcount <> 0
							"
		
				#End Region				
			
		#End Region
		
		#Region "MASTERS (Users)"
		
			#Region "Actual Nomenclature"
			
		Else If fileNameAlone = "XFC_PLT_ACT_Nomenclature" Then
            
										
					correctFileName = True
					tableName = "XFC_PLT_RAW_Nomenclature"	
					tableDest = "XFC_PLT_HIER_Nomenclature_Date"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								 	[id_product] VARCHAR(50) NOT NULL,
								    [description] VARCHAR(50) NULL,
								    [id_component] VARCHAR(50) NULL,
								    [coefficient] DECIMAL(18,5) NOT NULL,
								    [prorata] DECIMAL(18,5) NOT NULL,
								    [start_date] VARCHAR(50) NULL,
								    [end_date] VARCHAR(50) NULL
								)"
					
					 sqlMap = $"	
					 	DELETE FROM {tableDest}					 
					 	WHERE 1=1
					 		AND id_factory = '{factory}'
					 	
					 	INSERT INTO {tableDest} ( id_factory, id_product, id_component, coefficient, prorata, start_date, end_date)
					 
					 	SELECT
					 		'{factory}' as id_factory
					 		, id_product
					 		, id_component
					 		, SUM(coefficient) as coefficient
					 		, AVG(prorata) as prorata
					 		, start_date as start_date
					 		, end_date as end_date
					 	
					 	FROM {tableName}
					 
					 	GROUP BY 
					 		  id_product
					 		, id_component
					 		, start_date
					 		, end_date
					 
					 		"
			#End Region
			
			#Region "Planning Nomenclature"
			
		Else If fileNameAlone = $"XFC_PLT_PLAN_Nomenclature" Then
			
					correctFileName = True
					
					tableName = "XFC_PLT_RAW_Nomenclature_Date_Planning"	
					tableDest = "XFC_PLT_HIER_Nomenclature_Date_Planning"
					
					sqlRaw = $"
								DROP TABLE  IF EXISTS {tableName};
								CREATE TABLE {tableName}
								(
								    id_product		VARCHAR(50) NOT NULL,
								    id_component	VARCHAR(50) NOT NULL,
								    start_date      VARCHAR(50) NOT NULL,
								    end_date      	VARCHAR(50) NOT NULL,
								    coefficient     DECIMAL(18,2) NULL,
								    prorata        	DECIMAL(18,2) NULL
								);
					"
					
					' Faltará hacer un check de que todos los grupos medios estén en el maestro
					sqlMap = $"
					
						-- 1- Actualización de fechas
						DELETE FROM {tableDest}
						WHERE 1=1
							AND id_factory = '{factory}'
							AND scenario = '{scenario}'
							
						-- 2- Insertar solo si no existe exactamente igual
						INSERT INTO {tableDest} (scenario, id_factory, id_product, id_component, start_date, end_date, coefficient, prorata)
						
						SELECT 
							'{scenario}' AS scenario
							, '{factory}' AS id_factory
							, id_product
							, id_component
    						, CONVERT(DATETIME, start_date, 103) AS start_date
    						, CONVERT(DATETIME, end_date, 103)   AS end_date
							, coefficient
							, prorata
					
						FROM {tableName}	
					"
					
				#End Region 
				
			#Region "New Products Simulation"
				
		Else If fileNameAlone = $"XFC_PLT_PLAN_NewProducts_Simulation" Then
					
					correctFileName = True
					
					tableName = "XFC_PLT_RAW_NewProducts_Simulation"	
					tableDest = "XFC_PLT_AUX_NewProducts_Simulation"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								    id_product_final  VARCHAR(50)   NOT NULL,
								    id_product        VARCHAR(50)   NOT NULL,
								    id_component      VARCHAR(50)   NOT NULL,
								    description   VARCHAR(50)   NULL,
								    coefficient       DECIMAL(18,2) NULL,
								    id_averagegroup   VARCHAR(50)   NULL,
								    percentage        DECIMAL(18,2) NULL
								)
					"
					
					' Faltará hacer un check de que todos los grupos medios estén en el maestro
					sqlMap = $"
					
						-- 1- Actualización de fechas
						DELETE FROM XFC_PLT_AUX_NewProducts_Simulation
						WHERE 1=1
							AND id_factory = '{factory}'
							AND scenario = '{scenario}'
							AND year = {year}
							-- AND id_product_final IN (SELECT DISTINCT id_product_final FROM XFC_PLT_RAW_NewProducts_Simulation)
						
						-- 2- Insertar solo si no existe exactamente igual
						INSERT INTO XFC_PLT_AUX_NewProducts_Simulation (scenario, year, id_factory, id_product_final, id_product, id_component, coefficient, id_averagegroup, percentage)
						
						SELECT 
							'{scenario}' AS scenario
							, {year} AS year
							, '{factory}' AS id_factory
							, id_product_final
							, id_product
							, id_component
							, coefficient
							, id_averagegroup
							, percentage/100 as percentage
					
						FROM XFC_PLT_RAW_NewProducts_Simulation	
					
						-- 3- Insertar en la tabla de maestro de productos
						UPDATE MP
						SET
							MP.description = R.description
					
						FROM XFC_PLT_MASTER_Product AS MP
						INNER JOIN XFC_PLT_RAW_NewProducts_Simulation AS R
							ON R.id_component = MP.id
						WHERE 1=1 
							AND NULLIF(R.description,'') IS NOT NULL
							AND new_product = 1
						;
					
						INSERT INTO XFC_PLT_MASTER_Product (id, description, new_product)
					
						SELECT
							id_component
							, description
							, 1 as new_product
					
						FROM XFC_PLT_RAW_NewProducts_Simulation	
					
						WHERE 1=1
							AND id_component NOT IN (SELECT id FROM XFC_PLT_MASTER_Product)		
					
						GROUP BY id_component, description
					"
					
				#End Region
				
		#End Region
		
		#End Region	
		
		#Region "ADMINISTRATOR"
				
			#Region "Cost Center"
			
		Else If fileNameAlone = $"XFC_PLT_ADMIN_CostCenter" Then
					
					correctFileName = True
					
					tableName = "XFC_PLT_RAW_CostCenters_SAP"	
					tableDest = "XFC_PLT_MASTER_CostCenter_Hist_Testing"
					
					sqlRaw = $"
						DROP TABLE IF EXISTS {tableName}
						CREATE TABLE {tableName}
								(
								 	[id] VARCHAR(50) NOT NULL,
								    [id_factory] VARCHAR(50) NOT NULL,
								    [description] VARCHAR(50),
								    [start_date_sap] DATE NOT NULL,
								    [end_date_sap] DATE NOT NULL,
								    [type] VARCHAR(50) NOT NULL,
								    [function] VARCHAR(50),
								    [id_parent] VARCHAR(50)
								)"
					
					sqlMap = "
					-- 0. Delete Previo de la tabla SAP
					-- 	DELETE FROM XFC_PLT_MASTER_CostCenter_Hist_Testing
					-- 	WHERE 
					TRUNCATE TABLE XFC_PLT_MASTER_CostCenter_Hist_Testing;
					
					-- 1. Insert general en tabla Testing
					WITH CTE_CostCenter AS (
					    SELECT
					        'Actual' AS scenario,
					        R.id,
					        R.id_factory,
					        R.description,
					        R.start_date_sap,
					        R.end_date_sap,
					        R.type,
					        R.[function],
					        R.id_parent,
					        CASE 
					            WHEN COALESCE(MCN.start_date, R.start_date_sap) < R.start_date_sap 
					                THEN R.start_date_sap 
					            ELSE COALESCE(MCN.start_date, R.start_date_sap)
					        END AS start_date,
					        CASE 
					            WHEN COALESCE(MCN.end_date, R.end_date_sap) > R.end_date_sap 
					                THEN R.end_date_sap 
					            ELSE COALESCE(MCN.end_date, R.end_date_sap)
					        END AS end_date,
					        COALESCE(MCN.nature, 'TBD') AS nature,
					        COALESCE(MCN.id_averagegroup, 'TBD') AS id_averagegroup
					    FROM XFC_PLT_RAW_CostCenters_SAP R
					    LEFT JOIN XFC_PLT_MASTER_CostCenter_Nature MCN
					        ON MCN.scenario = 'Actual'
					        AND MCN.id_factory = R.id_factory
					        AND MCN.id_costcenter = R.id
					        AND (
					                R.start_date_sap BETWEEN MCN.start_date AND MCN.end_date
					            OR  R.end_date_sap BETWEEN MCN.start_date AND MCN.end_date
					            OR  R.end_date_sap >= MCN.end_date
					        )
					)
					
					INSERT INTO XFC_PLT_MASTER_CostCenter_Hist_Testing (
					    scenario, id, id_factory, description, start_date_sap, end_date_sap,
					    type, [function], id_parent, start_date, end_date, nature, id_averagegroup
					)
					SELECT *
					FROM CTE_CostCenter
					WHERE start_date < end_date;
					
					
					DELETE FROM XFC_PLT_MASTER_CostCenter_Hist
						WHERE 1=1
							AND id_factory IN( SELECT DISTINCT id_factory FROM XFC_PLT_MASTER_CostCenter_Hist_Testing);
					
					INSERT INTO XFC_PLT_MASTER_CostCenter_Hist (scenario, id, id_factory, description, type, [function], id_parent, start_date, end_date,nature, id_averagegroup)
					SELECT scenario, id, id_factory, description, type, [function], id_parent, start_date, end_date,nature, id_averagegroup
					FROM XFC_PLT_MASTER_CostCenter_Hist_Testing;
					
					
						-- INSERT INTO XFC_PLT_MASTER_CostCenter_Hist_Testing (scenario, id, id_factory, description, start_date_sap, end_date_sap, type, [function], id_parent, start_date, end_date,nature, id_averagegroup)
						-- 				
						-- SELECT
						-- 	'Actual' as scenario
						-- 	, R.id
						--     , R.id_factory
						--     , R.description
						--     , R.start_date_sap
						--     , R.end_date_sap
						--     , R.type
						--     , R.[function]
						--     , R.id_parent
						--     , CASE 
						--         WHEN COALESCE(MCN.start_date, R.start_date_sap) < R.start_date_sap THEN R.start_date_sap 
						--         ELSE COALESCE(MCN.start_date, R.start_date_sap)
						--       END AS start_date
						--     , CASE 
						--         WHEN  COALESCE(MCN.end_date,R.end_date_sap) > R.end_date_sap THEN R.end_date_sap 
						--         ELSE COALESCE(MCN.end_date,R.end_date_sap)
						--       END AS end_date
						--     , COALESCE(MCN.nature, 'TBD') as nature
						--     , COALESCE(MCN.id_averagegroup, 'TBD') as id_averagegroup
						-- 
						-- FROM XFC_PLT_RAW_CostCenters_SAP R
						-- 
						-- LEFT JOIN XFC_PLT_MASTER_CostCenter_Nature MCN
						-- 	ON MCN.scenario = 'Actual' -- '{sceneario}'
						-- 	AND MCN.id_factory = R.id_factory
						-- 	AND MCN.id_costcenter = R.id
						-- 	AND (
						-- 		R.start_date_sap BETWEEN MCN.start_date AND MCN.end_date
						-- 		OR
						-- 		R.end_date_sap BETWEEN MCN.start_date AND MCN.end_date
						-- 		OR
						-- 		R.end_date_sap >= MCN.end_date
						-- 		)
						-- 
						-- 	AND (
						-- 		R.start_date_sap BETWEEN MCN.start_date AND MCN.end_date
						-- 		OR
						-- 		R.end_date_sap BETWEEN MCN.start_date AND MCN.end_date
						-- 		OR
						-- 		R.end_date_sap >= MCN.end_date
						-- 		);						
						
						-- 2. Insert en el maestro
						
						-- 		2.1 Update de fechas
						-- UPDATE  finalTable						
						-- SET
						-- 	finalTable.end_date = origin.end_date
						-- 	, finalTable.type  = origin.type
						-- 	, finalTable.nature = origin.nature
						-- 	, finalTable.id_averagegroup = origin.id_averagegroup
						-- 	, finalTable.[function] = origin.[function]
						-- 	, finalTable.id_parent = origin.id_parent
						-- 
						-- FROM XFC_PLT_MASTER_CostCenter_Hist finalTable
						-- INNER JOIN XFC_PLT_MASTER_CostCenter_Hist_Testing origin
						-- 	ON finalTable.id = origin.id
						-- 	AND finalTable.id_factory = origin.id_factory
						-- 	AND finalTable.scenario = origin.scenario
						-- 	AND finalTable.start_date = origin.start_date;
						-- 	
						-- -- 		2.2 Insert Nuevos registros
						-- INSERT INTO XFC_PLT_MASTER_CostCenter_Hist (scenario, id, id_factory, description, type, [function], id_parent, start_date, end_date,nature, id_averagegroup)
						-- 
						-- SELECT 
						-- 	origin.scenario
						-- 	, origin.id
						-- 	, origin.id_factory
						-- 	, origin.description
						-- 	, origin.type
						-- 	, origin.[function]
						-- 	, origin.id_parent
						-- 	, origin.start_date
						-- 	, origin.end_date
						-- 	, origin.nature
						-- 	, origin.id_averagegroup
						-- 
						-- FROM XFC_PLT_MASTER_CostCenter_Hist_Testing origin
						-- 
						-- LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist finalTable
						-- 	ON finalTable.id = origin.id
						-- 	AND finalTable.id_factory = origin.id_factory
						-- 	AND finalTable.scenario = origin.scenario
						-- 	AND finalTable.start_date = origin.start_date
						-- 	AND finalTable.end_date = origin.end_date
						-- 
						-- WHERE 1=1
						-- 	AND finalTable.id IS NULL;

					"
					
			#End Region
			
			#Region "Cost Center - OLD"
			
				Else If fileNameAlone = $"XFC_PLT_ADMIN_CostCenter_OLD" Then
					
					correctFileName = True
					
					tableName = "XFC_PLT_RAW_CostCenter"	
					tableDest = "XFC_PLT_MASTER_CostCenter_Hist"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								 	[id] VARCHAR(50) NOT NULL,
								    [id_factory] VARCHAR(50) NOT NULL,
								    [description] VARCHAR(50) NULL,
								    [start_date] DATE NOT NULL,
								    [end_date] DATE NULL,
								    [type] VARCHAR(50) NOT NULL,
								    [nature] VARCHAR(50) NOT NULL,
								    [id_averagegroup] VARCHAR(50) NULL,
								    [function] VARCHAR(50) NULL,
								    [id_parent] VARCHAR(50) NULL
								)"
					
					sqlMap = "
					-- 0. Delete Previo de la tabla SAP
					TRUNCATE TABLE XFC_PLT_MASTER_CostCenter_Hist_Testing;
					
					-- 1. Insert general en tabla Testing
					
						INSERT INTO XFC_PLT_MASTER_CostCenter_Hist_Testing (scenario, id, id_factory, description, start_date_sap, end_date_sap, type, [function], id_parent, start_date, end_date,nature, id_averagegroup)
										
						SELECT
							'Actual' as scenario
							, R.id
						    , R.id_factory
						    , R.description
						    , R.start_date_sap
						    , R.end_date_sap
						    , R.type
						    , R.[function]
						    , R.id_parent
						    , CASE 
						        WHEN COALESCE(MCN.start_date, R.start_date_sap) < R.start_date_sap THEN R.start_date_sap 
						        ELSE COALESCE(MCN.start_date, R.start_date_sap)
						      END AS start_date
						    , CASE 
						        WHEN  COALESCE(MCN.end_date,R.end_date_sap) > R.end_date_sap THEN R.end_date_sap 
						        ELSE COALESCE(MCN.end_date,R.end_date_sap)
						      END AS end_date
						    , COALESCE(MCN.nature, 'TBD') as nature
						    , COALESCE(MCN.id_averagegroup, 'TBD') as id_averagegroup
					
						FROM XFC_PLT_RAW_CostCenters_SAP R
						
						LEFT JOIN XFC_PLT_MASTER_CostCenter_Nature MCN
							ON MCN.scenario = 'Actual' -- '{sceneario}'
							AND MCN.id_factory = R.id_factory
							AND MCN.id_costcenter = R.id
							AND (
								R.start_date_sap BETWEEN MCN.start_date AND MCN.end_date
								OR
								R.end_date_sap BETWEEN MCN.start_date AND MCN.end_date
								OR
								R.end_date_sap >= MCN.end_date
								);						
						
						-- 2. Insert en el maestro
						
						-- 		2.1 Update de fechas
						UPDATE  finalTable						
						SET
							finalTable.end_date = origin.end_date
							, finalTable.type  = origin.type
							, finalTable.nature = origin.nature
							, finalTable.id_averagegroup = origin.id_averagegroup
							, finalTable.[function] = origin.[function]
							, finalTable.id_parent = origin.id_parent
					
						FROM XFC_PLT_MASTER_CostCenter_Hist finalTable
						INNER JOIN XFC_PLT_MASTER_CostCenter_Hist_Testing origin
							ON finalTable.id = origin.id
							AND finalTable.id_factory = origin.id_factory
							AND finalTable.scenario = origin.scenario
							AND finalTable.start_date = origin.start_date;
							
						-- 		2.2 Insert Nuevos registros
						INSERT INTO XFC_PLT_MASTER_CostCenter_Hist (scenario, id, id_factory, description, type, [function], id_parent, start_date, end_date,nature, id_averagegroup)
						
						SELECT 
							origin.scenario
							, origin.id
							, origin.id_factory
							, origin.description
							, origin.type
							, origin.[function]
							, origin.id_parent
							, origin.start_date
							, origin.end_date
							, origin.nature
							, origin.id_averagegroup
						
						FROM XFC_PLT_MASTER_CostCenter_Hist_Testing origin
						
						LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist finalTable
							ON finalTable.id = origin.id
							AND finalTable.id_factory = origin.id_factory
							AND finalTable.scenario = origin.scenario
							AND finalTable.start_date = origin.start_date
							AND finalTable.end_date = origin.end_date
						
						WHERE 1=1
							AND finalTable.id IS NULL;
					"
					
			#End Region
			
			#Region "Nomenclature"
			
				Else If fileNameAlone = $"XFC_PLT_ADMIN_Nomenclature"Then
										
					correctFileName = True
					tableName = "XFC_PLT_RAW_Nomenclature"	
					tableDest = "XFC_PLT_HIER_Nomenclature_Date"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								    [id_factory] VARCHAR(50) NOT NULL,
								 	[id_product] VARCHAR(50) NOT NULL,
								    [description] VARCHAR(50) NULL,
								    [id_component] VARCHAR(50) NULL,
								    [coefficient] DECIMAL(18,5) NOT NULL,
								    [prorata] DECIMAL(18,5) NOT NULL,
								    [start_date] VARCHAR(50) NULL,
								    [end_date] VARCHAR(50) NULL
								)"
					
					 sqlMap = $"	
					 	DELETE FROM {tableDest}					 
					 	WHERE 1=1
					 		AND id_factory IN(SELECT DISTINCT id_factory FROM {tableName})
					 	
					 	INSERT INTO {tableDest} ( id_factory, id_product, id_component, coefficient, prorata, start_date, end_date)
					 
					 	SELECT
					 		id_factory
					 		, id_product
					 		, id_component
					 		, SUM(coefficient) as coefficient
					 		, AVG(prorata)/100 as prorata
					 		, CONVERT(date, start_date, 103) as start_date
					 		, CONVERT(date, end_date, 103) as end_date
					 	
					 	FROM {tableName}
					 
					 	GROUP BY 
					 		id_factory
					 		, id_product
					 		, id_component
					 		, CONVERT(date, start_date, 103) 
					 		, CONVERT(date, end_date, 103) 
					 
					 		"
					
			#End Region
			
		#End Region
				
				End If				
					
				
			If correctFileName = True Then
				
		#Region "Ejecuciones Finales"
		
				If sqlRaw <> String.Empty Then 
					
					' 1. Creación de tabla temporal RAW
					ExecuteSql(si, "DROP TABLE IF EXISTS " & tableName & "; DROP TABLE IF EXISTS XFC_PLT_RAW_Production_SAP;")
					ExecuteSql(si, sqlRaw) 
					
					' 2. Carga de datos en tabla temporal RAW						
					' 		Si es xlsx serán import de usuario, los csv se van a utilizar para los OneShot files
					If extension = "csv" Then	
						' Nada en principio
						' Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)					
					Else If extension = "xlsx" Then							
						BRapi.Utilities.LoadCustomTableUsingExcel(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes)											
					End If
				
				End If
				
				' 3. Insertar los datos mapeados en la tabla FINAL
				If sqlMap <> String.Empty Then 
					ExecuteSql(si, sqlMap)
					uti_sharedqueries.update_Log(si, scenario, factory, tableDest, "File")
				End If
				
'				 4. Eliminar la tabla temporal RAW
				'If sqlRaw <> String.Empty Then ExecuteSql(si, "DROP TABLE " & tableName & "; DROP TABLE IF EXISTS XFC_PLT_RAW_Production_SAP;")
				
				Return loadResults
				
		#End Region
		
			Else
				
				Dim listaEspecial As New List(Of TableRangeContent)
       			listaEspecial.Add(New TableRangeContent()) ' Un objeto vacío como identificador
        		Return listaEspecial				
				
			End If	
			
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function			
#End Region

		#Region "Helper Functions"
		
        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String) 
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                                   
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using  
			
        End Sub
		
		Private Function GenerateInsertSQLList(ByVal dataTable As DataTable, ByVal tableName As String) As List(Of String)
		
		    Dim insertStatements As New List(Of String)()
		    Dim sb As New StringBuilder()
		
		    ' Validate inputs
		    If dataTable Is Nothing OrElse dataTable.Rows.Count = 0 Then
		        Throw New ArgumentException("The DataTable is empty or null.")
		    End If
		
		    If String.IsNullOrEmpty(tableName) Then
		        Throw New ArgumentException("The table name is null or empty.")
		    End If
		
		    ' Get the column names
		    Dim columnNames As String = String.Join(", ", dataTable.Columns.Cast(Of DataColumn)().[Select](Function(c) c.ColumnName))
		
		    ' Batch size limit
		    Dim batchSize As Integer = 1000
		    Dim currentBatchSize As Integer = 0
		
		    ' Build the initial part of the INSERT INTO statement
		    sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		
		    ' Iterate through each row in the DataTable
		    For i As Integer = 0 To dataTable.Rows.Count - 1
		        Dim rowValues As New List(Of String)()
		
		        For Each column As DataColumn In dataTable.Columns
		            Dim value As Object = dataTable.Rows(i)(column)
		
		            If value Is DBNull.Value Then
		                rowValues.Add("NULL")
		            ElseIf TypeOf value Is String OrElse TypeOf value Is Char Then
		                rowValues.Add($"'{value.ToString().Replace("'", "''").Replace(",", ".")}'") ' Escape single quotes
		            ElseIf TypeOf value Is DateTime Then
		                rowValues.Add($"'{CType(value, DateTime).ToString("yyyy-MM-dd HH:mm:ss.fff")}'")
		            Else
		                rowValues.Add(value.ToString().Replace(",", ".").Replace("'","''"))
		            End If
		        Next
		
		        ' Combine row values into a comma-separated string and add to StringBuilder
		        sb.AppendLine($"({String.Join(", ", rowValues)}){If(currentBatchSize < batchSize - 1 AndAlso i < dataTable.Rows.Count - 1, ",", ";")}")
		        currentBatchSize += 1
		
		        ' If the batch size limit is reached, or it's the last row, finalize the current statement and start a new one
		        If currentBatchSize = batchSize OrElse i = dataTable.Rows.Count - 1 Then
		            insertStatements.Add(sb.ToString())
		            sb.Clear()
		            If i < dataTable.Rows.Count - 1 Then
		                sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		            End If
		            currentBatchSize = 0
		        End If
		    Next
		
		    Return insertStatements
			
		End Function
		
		Public Function PopUp(ByVal message As String, Optional forceExit As String = "Yes")
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = message
			Return If(forceExit = "Yes", selectionChangedTaskResult, Nothing)
			
		End Function
		
		#End Region		
		
	End Class
	
End Namespace
