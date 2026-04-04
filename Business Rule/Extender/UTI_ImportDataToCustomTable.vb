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

Namespace OneStream.BusinessRule.Extender.UTI_ImportDataToCustomTable
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get parameters and create full path and declare column mapping dictionary
						Dim tableName As String = args.NameValuePairs("p_table")
						Dim sectionName As String = args.NameValuePairs("p_section")
						Dim filesType As String = args.NameValuePairs("p_files_type")
						Dim delimiter As String = String.Empty
						If filesType = "Delimited" Then delimiter = args.NameValuePairs("p_delimiter")
						Dim filesFolderName As String = args.NameValuePairs("p_folder")
						Dim method As String = args.NameValuePairs("p_method")
						Dim fullPath As String = $"Documents/Public/{sectionName}/{filesType} Files/{filesFolderName}"
						Dim columnMappingDict As New Dictionary(Of String, String)
						
						'Create data table from files folder
						Dim dt As New DataTable 
						If filesType = "Excel" Then
							dt = UTISharedFunctionsBR.CreateDataTableFromExcelFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase)
						Else If filesType = "Delimited" Then
							dt = UTISharedFunctionsBR.CreateDataTableFromDelimitedFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase, delimiter)
						Else
							Throw ErrorHandler.LogWrite(si, New XFException("Files type must me 'Excel' or 'Delimited'"))
						End If
						
						'Remove all files in folder
						UTISharedFunctionsBR.DeleteAllFilesInFolder(si, fullPath, FileSystemLocation.ApplicationDatabase)
						
						'Load data table to custom table
						UTISharedFunctionsBR.LoadDataTableToCustomTable(si, tableName, dt, method)
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
	End Class
End Namespace