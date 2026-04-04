Imports System
Imports System.Globalization
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.TRS_ReportGenerator_PPT_Export
    Public Class MainClass
        Private ReadOnly UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Try
                Select Case args.FunctionType
                    Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
                        Dim functionName As String = String.Empty
                        If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("p_function") Then
                            functionName = args.NameValuePairs("p_function")
                        End If

                        If Not String.IsNullOrEmpty(functionName) Then
                            If functionName.Equals("DownloadTreasuryMonitoring", StringComparison.InvariantCultureIgnoreCase) Then
                                Return Me.CopyTreasuryMonitoring(si, args)
                            End If
                        End If
                End Select

                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Private Function CopyTreasuryMonitoring(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As Object
            Try
                Dim paramWeekStr As String = String.Empty
                Dim paramYearStr As String = String.Empty

                If args.NameValuePairs IsNot Nothing Then
                    If args.NameValuePairs.ContainsKey("paramWeek") Then
                        paramWeekStr = args.NameValuePairs("paramWeek")
                    End If
                    If args.NameValuePairs.ContainsKey("paramYear") Then
                        paramYearStr = args.NameValuePairs("paramYear")
                    End If
                End If

                Dim paramWeek As Integer
                Dim paramYear As Integer
                If Not Integer.TryParse(paramWeekStr, paramWeek) OrElse Not Integer.TryParse(paramYearStr, paramYear) Then
                    Throw New Exception("Parameters paramWeek and paramYear are required.")
                End If

                Dim paramUsername As String = si.UserName
                Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
                Dim targetFinalPath As String = "Import Templates"
                Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
                Dim targetFile As String = $"Treasury_Monitoring_{paramWeek}_{paramYear}.xfdoc.pptx"

                BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)

                ' Clean previous Treasury Monitoring files to avoid accumulation
                UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)

                Dim sourcePath As String = "Documents/Public/Treasury/Treasury_Monitoring.xfdoc.pptx"
                Dim sourceFile As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, sourcePath, True, True)

                If sourceFile Is Nothing OrElse sourceFile.XFFile Is Nothing Then
                    Throw New Exception($"Template file not found at {sourcePath}")
                End If

                Dim bytesOfFile As Byte() = sourceFile.XFFile.ContentFileBytes

                Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetFile, targetPath & "/")
                fileInfo.ContentFileExtension = "pptx"
                fileInfo.ContentFileContainsData = True
                fileInfo.XFFileType = True

                Dim newFile As XFFile = New XFFile(fileInfo, String.Empty, bytesOfFile)
                BRApi.FileSystem.InsertOrUpdateFile(si, newFile)

                Return True
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
    End Class
End Namespace
