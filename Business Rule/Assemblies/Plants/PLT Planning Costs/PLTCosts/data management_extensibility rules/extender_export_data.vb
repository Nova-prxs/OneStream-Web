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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_export_data
	Public Class MainClass

		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get parameters and create full path and declare column mapping dictionary
						Dim sheetName As String = args.NameValuePairs.GetValueOrDefault("p_sheet", "EmptyName")
						Dim parentPath As String = $"Documents/Users/{si.UserName}"
						Dim childPath As String = "Exports"
						Dim fullPath As String = $"{parentPath}/{childPath}"
						
						Dim factory As String = args.NameValuePairs("factory")
						Dim month As String = args.NameValuePairs("month")
						Dim year As String = args.NameValuePairs("year")
						Dim scenario As String = args.NameValuePairs("scenario")
						
						'Create folder if necessary
						BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, childPath)
						
						'Create data table depending on the table parameter
						Dim dt As New DataTable
						Dim sql As String = String.Empty
						sql = shared_queries.AllQueries(si, "DistribucionCostes", factory, month, year, scenario)	
							sql = sql & $"
							
								SELECT *
								FROM lastTable
							"
							BRApi.ErrorLog.LogMessage(si, sql)
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using
						
						'Create file
						UTISharedFunctionsBR.GenerateExcelFileFromDataTable(si, dt, sheetName, FileSystemLocation.ApplicationDatabase, fullPath)
					
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
