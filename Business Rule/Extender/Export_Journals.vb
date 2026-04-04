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

Namespace OneStream.BusinessRule.Extender.Export_Journals
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try

			Dim sprofileName As String = String.Empty
			Dim sscenarioName As String = String.Empty
			Dim stimeMemberFilter As String = String.Empty
			Dim localFolder As String = "Exports"	
				
			sprofileName = args.NameValuePairs.XFGetValue("WFProfileName")
			sscenarioName = args.NameValuePairs.XFGetValue("ScenarioName")
			stimeMemberFilter = args.NameValuePairs.XFGetValue("TimeFilter")
			
			' Recovering File Share dynamic folder
			Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
			Dim rootPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName)
			
			'Define file name based on current date, workflowname and period			        
			Dim fileName As String = "EX_" & DateTime.UtcNow.ToString("yyyyMMdd") & "_" & sprofileName & "_WF" & stimeMemberFilter & ".csv"

			'Define full folder path (root path + local folder)
			Dim fullFolderPath As String = rootPath & "\" & localFolder
			
			'Determine whether the directory exists. 
			If Not Directory.Exists(fullFolderPath) Then
    			'Folder does not exist - create the specified folder
    			Dim di As DirectoryInfo = Directory.CreateDirectory(fullFolderPath)
			End If
			
			'Define full  path (full folder path + file name)
			Dim fullPath As String = fullFolderPath & "\" & fileName
			
			'Delete file if exists
			If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.FileShare, fullPath) Then 
				BRApi.FileSystem.DeleteFile(si, FileSystemLocation.FileShare, fullPath)
			End If
			
			'Export the new file
			Dim csv As String = BRApi.Journals.Data.ExportJournalsToCsv(si, fullPath, sprofileName, sscenarioName, "T#"& stimeMemberFilter, "Posted")
					
			Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace