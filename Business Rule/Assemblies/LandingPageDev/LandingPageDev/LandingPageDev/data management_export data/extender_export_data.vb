Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_export_data
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get parameters and create full path and declare column mapping dictionary
						Dim sheetName As String = args.NameValuePairs("p_sheet")
						Dim parentPath As String = $"Documents/Users/{si.UserName}"
						Dim childPath As String = "Exports"
						Dim fullPath As String = $"{parentPath}/{childPath}"
						Dim queryParams As String = args.NameValuePairs("p_query_params")
						Dim WSPrefix As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "LandingPageDev.prm_Workspace_Prefix")
						
						'Create folder if necessary
						BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, childPath)
						
						'Create data table depending on the table parameter
						Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromSheetName(si, sheetName, queryParams, WSPrefix)
						
						'Create file
						UTISharedFunctionsBR.GenerateExcelFileFromDataTable(si, dt, sheetName, FileSystemLocation.ApplicationDatabase, fullPath)
						
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
