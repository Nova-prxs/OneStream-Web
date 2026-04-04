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

Namespace OneStream.BusinessRule.Extender.UTI_ClearFolders
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						' Set the list of folders to clear
						Dim foldersToClear As New List(Of Tuple(Of String, FileSystemLocation)) From {
							New Tuple(Of String, FileSystemLocation)(
								"Documents/Public/Services/Templates/Temporal",
								FileSystemLocation.ApplicationDatabase
							)
						}
						
						' Get all folders in temporal folder
						Dim subfolders = BRApi.FileSystem.GetFoldersInFolder(si, FileSystemLocation.ApplicationDatabase, "Documents/Public/Services/Templates/Temporal", True, True)
						' Add folders to folders to clear
						For Each subfolder In subfolders
							Dim subfolderName As String = subfolder.XFFolder.FullName
							foldersToClear.Add(
								New Tuple(Of String, FileSystemLocation)(
									subfolderName,
									FileSystemLocation.ApplicationDatabase
								)
							)
						Next
						
						' Clear the list of folders
						For Each folder In foldersToClear
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, folder.Item1, folder.Item2)
						Next
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace