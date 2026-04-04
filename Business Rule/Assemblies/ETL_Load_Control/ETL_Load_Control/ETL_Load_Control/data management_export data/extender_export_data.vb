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
						'Get function parameter to determine which function to execute
						Dim functionName As String = args.NameValuePairs.XFGetValue("p_function")
						
						If functionName.XFEqualsIgnoreCase("ExportToExcel") Then
							'Get parameters and create full path and declare column mapping dictionary
							Dim sheetName As String = args.NameValuePairs("p_sheet")
							Dim parentPath As String = $"Documents/Users/{si.UserName}"
							Dim childPath As String = "Exports"
							Dim fullPath As String = $"{parentPath}/{childPath}"
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim WSPrefix As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "ETL_Load_Control.prm_ETL_Load_Control_Workspace_Prefix")
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, childPath)
							
							'Create data table depending on the table parameter
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromSheetName(si, sheetName, queryParams, WSPrefix)
							
							'Create file
							UTISharedFunctionsBR.GenerateExcelFileFromDataTable(si, dt, sheetName, FileSystemLocation.ApplicationDatabase, fullPath)
						End If
						
						If functionName.XFEqualsIgnoreCase("ExportLandingToCSV") Then
							'Export XFC_LANDING_FIN_MIX table to CSV
							Me.ExportLandingTableToCSV(si)
						End If
						
If functionName.XFEqualsIgnoreCase("ExportDWHToCSV") Then
								'Export XFC_DWH_FIN_MIX table to CSV
								'Optional: p_from and p_to parameters to filter by period
								Dim fromPeriod As String = args.NameValuePairs.XFGetValue("p_from", "")
								Dim toPeriod As String = args.NameValuePairs.XFGetValue("p_to", "")
								Me.ExportDWHTableToCSV(si, fromPeriod, toPeriod)
						End If
						
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
		
		#Region "Export Landing Table to CSV"
		
		Private Sub ExportLandingTableToCSV(ByVal si As SessionInfo)
			Try
				'Configuration
				Dim tableName As String = "XFC_LANDING_FIN_MIX"
				Dim fileName As String = $"LANDING_FIN_MIX_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv"
				Dim parentPath As String = $"Documents/Users/AurelioSantos"
				Dim childPath As String = "Exports"
				Dim folderPath As String = $"{parentPath}/{childPath}"
				
				'Get data from table
				Dim dt As DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"SELECT * FROM {tableName}"
					dt = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
				End Using
				
				If dt Is Nothing OrElse dt.Rows.Count = 0 Then
					BRApi.ErrorLog.LogMessage(si, $"[EXPORT CSV] No data found in {tableName}")
					Return
				End If
				
				'Build CSV content
				Dim csvBuilder As New System.Text.StringBuilder()
				
				'Add header row
				Dim columnNames As New List(Of String)
				For Each col As DataColumn In dt.Columns
					columnNames.Add(col.ColumnName)
				Next
				csvBuilder.AppendLine(String.Join(",", columnNames))
				
				'Add data rows
				For Each row As DataRow In dt.Rows
					Dim values As New List(Of String)
					For Each col As DataColumn In dt.Columns
						Dim value As String = If(IsDBNull(row(col)), "", row(col).ToString())
						'Escape commas and quotes in values
						If value.Contains(",") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
							value = """" & value.Replace("""", """""") & """"
						End If
						values.Add(value)
					Next
					csvBuilder.AppendLine(String.Join(",", values))
				Next
				
				'Convert to bytes
				Dim csvBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())
				
				'Create folder if necessary
				BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, childPath)
				
				'Create XFFile object and set properties via FileInfo
				Dim xfFile As New XFFile()
				xfFile.FileInfo.Name = fileName
				xfFile.FileInfo.FolderFullName = folderPath
				xfFile.FileInfo.FileSystemLocation = FileSystemLocation.ApplicationDatabase
				xfFile.ContentFileBytes = csvBytes
				
				'Save file to OneStream File System
				BRApi.FileSystem.InsertOrUpdateFile(si, xfFile)
				
				BRApi.ErrorLog.LogMessage(si, $"[EXPORT CSV] Successfully exported {dt.Rows.Count} rows to {folderPath}/{fileName}")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
		#Region "Export DWH Table to CSV"
		
		''' <summary>
		''' Exports XFC_DWH_FIN_MIX table to CSV file in OneStream File System.
		''' Optionally filters by period range.
		''' </summary>
		''' <param name="fromPeriod">Optional: Start period filter (e.g., 2025M1)</param>
		''' <param name="toPeriod">Optional: End period filter (e.g., 2025M6)</param>
		Private Sub ExportDWHTableToCSV(ByVal si As SessionInfo, Optional ByVal fromPeriod As String = "", Optional ByVal toPeriod As String = "")
			Try
				'Configuration
				Dim tableName As String = "XFC_DWH_FIN_MIX"
				Dim periodSuffix As String = ""
				If Not String.IsNullOrEmpty(fromPeriod) Then
					periodSuffix = $"_{fromPeriod}"
					If Not String.IsNullOrEmpty(toPeriod) AndAlso toPeriod <> fromPeriod Then
						periodSuffix &= $"_to_{toPeriod}"
					End If
				End If
				Dim fileName As String = $"DWH_FIN_MIX{periodSuffix}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv"
				Dim parentPath As String = $"Documents/Users/AurelioSantos"
				Dim childPath As String = "Exports"
				Dim folderPath As String = $"{parentPath}/{childPath}"
				
				'Build query with optional period filter
				Dim query As String = $"SELECT * FROM {tableName}"
				
				If Not String.IsNullOrEmpty(fromPeriod) Then
					Dim effectiveToPeriod As String = If(String.IsNullOrEmpty(toPeriod), fromPeriod, toPeriod)
					Dim periods As List(Of String) = HelperFunctions.GeneratePeriodRange(fromPeriod, effectiveToPeriod)
					
					If periods.Count > 0 Then
						Dim periodInClause As String = HelperFunctions.BuildPeriodInClause(periods)
						query &= $" WHERE [Time] IN ({periodInClause})"
					End If
				End If
				
				'Get data from table
				Dim dt As DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					dt = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
				End Using
				
				If dt Is Nothing OrElse dt.Rows.Count = 0 Then
					BRApi.ErrorLog.LogMessage(si, $"[EXPORT CSV] No data found in {tableName}" & If(Not String.IsNullOrEmpty(fromPeriod), $" for period {fromPeriod}", ""))
					Return
				End If
				
				'Build CSV content
				Dim csvBuilder As New System.Text.StringBuilder()
				
				'Add header row
				Dim columnNames As New List(Of String)
				For Each col As DataColumn In dt.Columns
					columnNames.Add(col.ColumnName)
				Next
				csvBuilder.AppendLine(String.Join(",", columnNames))
				
				'Add data rows
				For Each row As DataRow In dt.Rows
					Dim values As New List(Of String)
					For Each col As DataColumn In dt.Columns
						Dim value As String = If(IsDBNull(row(col)), "", row(col).ToString())
						'Escape commas and quotes in values
						If value.Contains(",") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
							value = """" & value.Replace("""", """""") & """"
						End If
						values.Add(value)
					Next
					csvBuilder.AppendLine(String.Join(",", values))
				Next
				
				'Convert to bytes
				Dim csvBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString())
				
				'Create folder if necessary
				BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, childPath)
				
				'Create XFFile object and set properties via FileInfo
				Dim xfFile As New XFFile()
				xfFile.FileInfo.Name = fileName
				xfFile.FileInfo.FolderFullName = folderPath
				xfFile.FileInfo.FileSystemLocation = FileSystemLocation.ApplicationDatabase
				xfFile.ContentFileBytes = csvBytes
				
				'Save file to OneStream File System
				BRApi.FileSystem.InsertOrUpdateFile(si, xfFile)
				
				BRApi.ErrorLog.LogMessage(si, $"[EXPORT CSV] Successfully exported {dt.Rows.Count} DWH rows to {folderPath}/{fileName}")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
	End Class
End Namespace
