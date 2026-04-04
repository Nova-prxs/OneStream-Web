Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.Extender.MoveFile

	Public Class MainClass
 
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object

			'Prepare the variables related to Workflow and User
			Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
			Dim wfClusterPk As New WorkflowUnitClusterPk
			Dim currentProfileName As WorkflowProfileInfo = BRAPi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
			Dim wfProfileName As String = currentProfileName.Name
			Dim wfYear As Integer = BRApi.Finance.Time.GetYearFromId(si, myWorkflowUnitPk.TimeKey)
			Dim wfMonth As Integer = BRApi.Finance.Time.GetPeriodNumFromId(si, myWorkflowUnitPk.TimeKey)
			Dim scenarioName As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
			Dim userName As String = si.AuthToken.UserName
			
			'Prepare the Stage Data Extract File path
			Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)

			'Update this path
			'Dim sourceDir As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\" & DateTime.UtcNow.ToString("yyyyMMdd") 
			Dim sourceDir As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) 
			
			'Folder to Create
			Dim newPath As String = "ExportJournals" '<-- This is the destination folder
			Dim parentPath As String = "Documents/Users/" & userName '<-- App Folder path where new folder will be added, may be updated
			
			
			'Get full folder path to test
			Dim folderPath As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, parentPath & "/" & newPath)

			'Create Folder if it does not exist
			If folderPath Is Nothing Then
				BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, newPath)
			End If

			'Get Scenario from current Workflow
			'Dim scenarioName As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
			'brapi.ErrorLog.LogMessage(si, "scenarioname = " & scenarioName) ' <-- comment/uncomment for testing/logging

			'Get Time from current Workflow
			'Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
			'Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, myWorkflowUnitPk.TimeKey)
			'brapi.ErrorLog.LogMessage(si, "wfTime = " & wfTime) ' <-- comment/uncomment for testing/logging
			'Dim fileName As String = scenarioName & "_" & wfTime & "Ingeteam_E101_R_2019M12_CubeData.csv" '<-- update this string with the name of the exported file
			Dim fileName As String = "EX_" & DateTime.UtcNow.ToString("yyyyMMdd") & "_" & wfProfileName & "_WF" & wfYear & wfMonth & ".csv" '<-- update this string with the name of the exported file			
			Dim targetDir As String = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, parentPath & "/" & newPath).XFFolder.FullName 
			'brapi.ErrorLog.LogMessage(si, "SourceDir = " & sourceDir) ' <-- comment/uncomment for testing/logging
			'brapi.ErrorLog.LogMessage(si, "TargetDir = " & targetDir) ' <-- comment/uncomment for testing/logging
			'brapi.ErrorLog.LogMessage(si, "FileName = " & fileName) ' <-- comment/uncomment for testing/logging
			Dim fileToMove As String = fileName
			Dim fileBytes As Byte() = File.ReadAllBytes(sourceDir & "\" & fileToMove)
			'brapi.ErrorLog.LogMessage(si, "FiletoMove = " & fileToMove) ' <-- comment/uncomment for testing/logging


			Using dbConnApp As DBConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)

				'Save file in User's Temp folder in application DB
				'(This will be cleaned up automatically be OneStream platform When user's session expires)
				Dim dbFileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, fileToMove, targetDir, XFFileType.Unknown)
				dbFileInfo.ContentFileContainsData = True
				dbFileInfo.ContentFileExtension = dbFileInfo.Extension
				
				Dim dbFile As New XFFile(dbFileInfo, String.Empty, fileBytes)
					BRApi.FileSystem.InsertOrUpdateFile(si, dbFile)

			End Using

		End Function

	End Class

End Namespace