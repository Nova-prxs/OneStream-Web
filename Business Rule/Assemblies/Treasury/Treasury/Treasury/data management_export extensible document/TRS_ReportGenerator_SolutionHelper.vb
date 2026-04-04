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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.TRS_ReportGenerator_SolutionHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
								loadDashboardTaskResult.ModifiedCustomSubstVars = Nothing
								Return loadDashboardTaskResult
							End If
						End If
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						If args.FunctionName.XFEqualsIgnoreCase("GenerateFile") Then
							'Almacenamos los parámetros pasados desde el botón del dashboard
							Dim paramScenario As String = args.NameValuePairs("paramScenario")
							Dim paramWeek As String = args.NameValuePairs("paramWeek")
							Dim paramYear As String = args.NameValuePairs("paramYear")
							Dim filePath As String = "Documents/Public/Treasury/Extensible Document/Comments/" & paramYear & "/" & paramWeek
							Dim fileName As String = paramScenario & "_" & paramWeek & "_" & paramYear & ".docx"
							brapi.ErrorLog.LogMessage(si,"Entro")
							'Si el fichero ya ha sido creado, no lo volvemos a crear
							Dim fileExists As Boolean = BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath & "/" & fileName)
							If (fileExists = False) Then
								Me.CreateFile(si, paramScenario, paramWeek, paramYear)
							End If
						End If
						
						If args.FunctionName.XFEqualsIgnoreCase("DeleteFile") Then
							'Almacenamos los parámetros pasados desde el botón del dashboard
							Dim paramScenario As String = args.NameValuePairs("paramScenario")
							Dim paramWeek As String = args.NameValuePairs("paramWeek")
							Dim paramYear As String = args.NameValuePairs("paramYear")
							
							Me.DeleteFile(si, paramScenario, paramWeek, paramYear)
						End If
						
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							selectionChangedTaskResult.Message = ""
							selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedUIActionInfo = Nothing
							selectionChangedTaskResult.ChangeSelectionChangedNavigationInDashboard = False
							selectionChangedTaskResult.ModifiedSelectionChangedNavigationInfo = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVars = Nothing
							selectionChangedTaskResult.ChangeCustomSubstVarsInLaunchedDashboard = False
							selectionChangedTaskResult.ModifiedCustomSubstVarsForLaunchedDashboard = Nothing
							Return selectionChangedTaskResult
						End If
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.Message = ""
							saveDataTaskResult.CancelDefaultSave = False
							Return saveDataTaskResult
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		'{TRS_ReportGenerator_SolutionHelper}{GenerateFile}{paramScenario = |!prm_Treasury_Scenario!|, paramWeek = |!prm_Treasury_WeekNumber!|, paramYear = |!prm_Treasury_Year!|}
		
		Public Sub CreateFile(ByVal si As SessionInfo, paramScenario As String, paramWeek As String, paramYear As String)
			Dim templatePath As String = "Documents/Public/Treasury/Extensible Document"
			Dim targetBasePath As String = "Documents/Public/Treasury/Extensible Document/Comments"
			
			'Creamos la estructura de carpetas year/week si no existe
			BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetBasePath, paramYear & "/" & paramWeek)
			
			'Obtenemos el fichero plantilla
			Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, templatePath & "/commentTemplate.docx", True, True)
			Dim bytesOfFile As Byte() = objXFFileEx.XFFile.ContentFileBytes
			
			'Definimos la ruta destino y nombre del fichero
			Dim targetPath As String = targetBasePath & "/" & paramYear & "/" & paramWeek
			Dim fileName As String = paramScenario & "_" & paramWeek & "_" & paramYear & ".docx"
			Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, fileName, targetPath)
			Dim userName As String = si.AuthToken.UserName
			
			'Añadimos detalle adicional del fichero
			fileInfo.ContentFileExtension = "docx"
			fileInfo.Description = "Executed by " & userName
			fileInfo.ContentFileContainsData = True
			fileInfo.XFFileType = True
			
			'Ejecutamos el copiado
			Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, bytesOfFile)
			BRApi.FileSystem.InsertOrUpdateFile(si, fileFile)
		End Sub
		
		'Borrado de archivo
		'{TRS_ReportGenerator_SolutionHelper}{DeleteFile}{paramScenario = |!prm_Treasury_Scenario!|, paramWeek = |!prm_Treasury_WeekNumber!|, paramYear = |!prm_Treasury_Year!|}
		
		Public Sub DeleteFile(ByVal si As SessionInfo, paramScenario As String, paramWeek As String, paramYear As String)
			Dim filePath As String = "Documents/Public/Treasury/Extensible Document/Comments/" & paramYear & "/" & paramWeek
			Dim fileName As String = paramScenario & "_" & paramWeek & "_" & paramYear & ".docx"
			Dim fileExists As Boolean = BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath & "/" & fileName)
			If (fileExists = True) Then
				BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, filePath & "/" & fileName)
			End If
		End Sub
		
	End Class
End Namespace
