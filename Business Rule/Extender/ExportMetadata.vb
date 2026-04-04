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

Namespace OneStream.BusinessRule.Extender.ExportMetadata
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType     
    Case Is = ExtenderFunctionType.Unknown, ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
        'Prepare the Stage Data Extract File path
        Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
        Dim folderPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\Analytics"
        If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)
   
        Dim filePath As String = folderPath & "\" & si.AppToken.AppName & "_" & DateTime.UtcNow.ToString("yyyyMMdd") & ".zip"        
        If File.Exists(filePath) Then File.Delete(filePath)
   
        'Set the extract options
        Dim xmlOptions As New XmlExtractOptions
        'xmlOptions.ExtractAllItems = True
		'xmlOptions.ExtractUniqueIDs = True
		xmlOptions.ExtractFileName = "Metadata" 
      
        'Execute the Metadata Extract
        Using dbConnFW As DBConnInfo = BRAPi.Database.CreateFrameworkDbConnInfo(si)
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                Dim zipBytes As Byte() = ApplicationZipFileHelper.Extract(dbConnFW, dbConnApp, Nothing, xmlOptions)
                'Append the contents of this workflow profile to the extract file               
                Using FS As New FileStream(filePath, FileMode.Append, FileAccess.Write)   
                    'Create a binary writer, and write all bytes to the FileStream at once
                    Using BW As New BinaryWriter(FS)
                        BW.Write(zipBytes)
                    End Using
                End Using
            End Using
        End Using
            
End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace