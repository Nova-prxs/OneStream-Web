Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Threading
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Extender.ETL_Extender_Export_Tables
	
	''' <summary>
	''' Business Rule to export/import Custom Tables to/from CSV files.
	''' 
	''' Usage:
	''' - p_function=Download : Exports all configured tables to CSV files (fixed names)
	''' - p_function=Import   : Imports all CSV files back to tables (TRUNCATE + INSERT)
	''' 
	''' Files are saved/read from: Documents/Users/{targetUser}/Exports/{tableName}.csv
	''' </summary>
	Public Class MainClass
		
		#Region "User Configuration Parameters"
		
		'===============================================================================
		' USER CONFIGURATION - Modify these values as needed
		'===============================================================================
		
		''' <summary>
		''' List of all tables to export/import.
		''' </summary>
		Private Shared ReadOnly EXPORT_TABLES As String() = New String() {
			"XFC_DWH_LOAD_CONTROL",
			"XFC_ETL_RULES",
			"XFC_STG_ETL_LOAD_CONTROL",
			"XFC_REST_MASTER_request",
			"XFC_REST_MASTER_request_auth",
			"XFC_REST_MASTER_request_body_kvp",
			"XFC_REST_MASTER_request_column_mapping",
			"XFC_REST_MASTER_request_header",
			"XFC_REST_MASTER_request_param",
			"XFC_REST_MASTER_request_test",
			"XFC_REST_MASTER_request_variable",
			"XFC_DWH_FIN_UD5_Filter"
		}
		
		''' <summary>
		''' Default target user folder. If empty, uses the current logged-in user.
		''' Examples: "AurelioSantos", "AdminUser", "" (for current user)
		''' </summary>
		Private Const DEFAULT_TARGET_USER As String = ""
		
		''' <summary>
		''' Subfolder name inside the user's Documents folder where exports are saved.
		''' </summary>
		Private Const EXPORT_SUBFOLDER As String = "Exports"
		
		''' <summary>
		''' CSV field delimiter character.
		''' Using semicolon (;) instead of comma (,) to avoid conflicts with
		''' European decimal format where comma is the decimal separator.
		''' Example: 2031,01 (European decimal) would break a comma-delimited CSV.
		''' </summary>
		Private Const CSV_DELIMITER As Char = ";"c
		
		''' <summary>
		''' Tables with IDENTITY columns that need special handling (SET IDENTITY_INSERT ON).
		''' </summary>
		Private Shared ReadOnly IDENTITY_COLUMNS As New Dictionary(Of String, String) From {
			{"XFC_DWH_LOAD_CONTROL", "LoadId"},
			{"XFC_ETL_RULES", "RuleId"},
			{"XFC_STG_ETL_LOAD_CONTROL", "LoadId"},
			{"XFC_REST_MASTER_request", "id"}
		}
		
		'===============================================================================
		' DIRECT QUERY CONFIGURATION - Edit this query as needed
		'===============================================================================
		
		''' <summary>
		''' Query to execute with p_function=download_direct
		''' Edit this query directly - no character limit!
		''' </summary>
		Private Const DIRECT_QUERY As String = "SELECT * FROM XFC_DWH_FIN_MIX WHERE Account IN ('403000','411000','421000','430100','451000','620000','620050','620090','621000','621050','621090','622000','622050','622090','623000','731100','811800','930100')"
		
		''' <summary>
		''' Output filename for direct query export (without .csv extension)
		''' </summary>
		Private Const DIRECT_QUERY_OUTPUT As String = "DirectQueryExport"
		
		'===============================================================================
		
		#End Region
		
		#Region "Main Entry Point"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			'Force InvariantCulture to ensure consistent number/date formatting
			'regardless of the server's regional settings (prevents . vs , decimal issues)
			Dim originalCulture As CultureInfo = Thread.CurrentThread.CurrentCulture
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture
			Try
				'Get function parameter
				Dim functionParam As String = args.NameValuePairs.XFGetValue("p_function", String.Empty).Trim().ToLower()
				'Get target user - remove spaces to convert "Aurelio Santos" to "AurelioSantos"
				Dim targetUser As String = If(String.IsNullOrEmpty(DEFAULT_TARGET_USER), si.UserName.Replace(" ", ""), DEFAULT_TARGET_USER.Replace(" ", ""))
				
				Select Case functionParam
					
					Case "download"
						'Export all tables to CSV
						Return Me.DownloadAllTables(si, targetUser)
						
					Case "import"
						'Import all CSV files back to tables
						Return Me.ImportAllTables(si, targetUser)

					Case "download_query"
						'Export custom query to CSV
						Return Me.DownloadCustomQuery(si, targetUser, args)
					
					Case "download_direct"
						'Export using the hardcoded DIRECT_QUERY
						Return Me.DownloadDirectQuery(si, targetUser)
					
					Case "import_landing"
						'Import CSV to XFC_LANDING_FIN_MIX (APPEND mode - no delete)
						Return Me.ImportLandingFromCSV(si, targetUser, args)
					
					'Case "download_cube"
						'DISABLED: GetDataBufferUsingMemberScript is not available from Extender context (only from Finance Rules api.Data).
						'Use download_stage instead to extract source-specific data from StageSourceData.
						'Return Me.DownloadCubeData(si, targetUser, args)
					
					Case "download_stage"
						'Export Stage source data to CSV filtered by connector source (iScala/SAP)
						Return Me.DownloadStageData(si, targetUser, args)
						
					Case Else
						'Default: show usage instructions
						Return "Usage: p_function=Download | Import | download_query | download_direct | import_landing | download_stage (p_period, p_source)"
						
				End Select
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			Finally
				'Restore original culture
				Thread.CurrentThread.CurrentCulture = originalCulture
			End Try
		End Function
		
		#End Region
		
		#Region "Download All Tables"
		
		''' <summary>
		''' Exports all configured tables to CSV files with fixed names.
		''' </summary>
		Private Function DownloadAllTables(ByVal si As SessionInfo, ByVal targetUser As String) As String
			Dim results As New StringBuilder()
			Dim successCount As Integer = 0
			Dim errorCount As Integer = 0
			
			results.AppendLine("=== Tables CSV Export ===")
			results.AppendLine($"Target folder: Documents/Users/{targetUser}/{EXPORT_SUBFOLDER}")
			results.AppendLine("")
			
			For Each tableName As String In EXPORT_TABLES
				Try
					Dim rowCount As Integer = Me.ExportTableToCSV(si, tableName, targetUser)
					results.AppendLine($"✓ {tableName}: {rowCount} rows exported")
					successCount += 1
				Catch ex As Exception
					results.AppendLine($"✗ {tableName}: ERROR - {ex.Message}")
					errorCount += 1
				End Try
			Next
			
			results.AppendLine("")
			results.AppendLine($"=== Summary: {successCount} exported, {errorCount} errors ===")
			
			BRApi.ErrorLog.LogMessage(si, results.ToString())
			Return results.ToString()
		End Function
		
		#End Region
		
		#Region "Import All Tables"
		
		''' <summary>
		''' Imports all CSV files from the exports folder back to their corresponding tables.
		''' Performs DELETE (reverse order) then INSERT (forward order) to handle Foreign Keys.
		''' </summary>
		Private Function ImportAllTables(ByVal si As SessionInfo, ByVal targetUser As String) As String
			Dim results As New StringBuilder()
			Dim successCount As Integer = 0
			Dim errorCount As Integer = 0
			Dim skippedCount As Integer = 0
			
			Dim folderPath As String = $"Documents/Users/{targetUser}/{EXPORT_SUBFOLDER}"
			
			results.AppendLine("=== Tables CSV Import ===")
			results.AppendLine($"Source folder: {folderPath}")
			results.AppendLine("")
			
			'---------------------------------------------------------------------------
			' PASS 1: DELETE DATA (Reverse Order)
			'---------------------------------------------------------------------------
			results.AppendLine("--- Phase 1: Cleaning Tables (Reverse Order) ---")
			
			'Iterate in reverse to respect FK dependencies (Children first, then Parent)
			For i As Integer = EXPORT_TABLES.Length - 1 To 0 Step -1
				Dim tableName As String = EXPORT_TABLES(i)
				Try
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						'Use DELETE instead of TRUNCATE to avoid FK constraint errors
						Dim deleteSql As String = $"DELETE FROM {tableName}"
						BRApi.Database.ExecuteSql(dbConn, deleteSql, False)
						results.AppendLine($"✓ {tableName}: Cleaned")
					End Using
				Catch ex As Exception
					results.AppendLine($"✗ {tableName}: CLEANUP ERROR - {ex.Message}")
					errorCount += 1
				End Try
			Next
			
			results.AppendLine("")
			
			'---------------------------------------------------------------------------
			' PASS 2: INSERT DATA (Forward Order)
			'---------------------------------------------------------------------------
			results.AppendLine("--- Phase 2: Importing Data (Forward Order) ---")
			
			For Each tableName As String In EXPORT_TABLES
				Try
					Dim fileName As String = $"{tableName}.csv"
					Dim filePath As String = $"{folderPath}/{fileName}"
					
					'Check if file exists and read it
					Dim xfFileEx As XFFileEx = Nothing
					Try
						xfFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False)
					Catch
						'File not found
						xfFileEx = Nothing
					End Try
					
					If xfFileEx Is Nothing OrElse xfFileEx.XFFile Is Nothing OrElse xfFileEx.XFFile.ContentFileBytes Is Nothing Then
						results.AppendLine($"⊘ {tableName}: File not found, skipped")
						skippedCount += 1
						Continue For
					End If
					
					'Read CSV file
					Dim fileBytes As Byte() = xfFileEx.XFFile.ContentFileBytes
					Dim csvContent As String = Encoding.UTF8.GetString(fileBytes)
					
					'Import to table (Skip truncate, just insert)
					Dim rowCount As Integer = Me.ImportCSVToTable(si, tableName, csvContent, False)
					results.AppendLine($"✓ {tableName}: {rowCount} rows imported")
					successCount += 1
					
				Catch ex As Exception
					results.AppendLine($"✗ {tableName}: IMPORT ERROR - {ex.Message}")
					errorCount += 1
				End Try
			Next
			
			results.AppendLine("")
			results.AppendLine($"=== Summary: {successCount} imported, {skippedCount} skipped, {errorCount} errors ===")
			
			BRApi.ErrorLog.LogMessage(si, results.ToString())
			Return results.ToString()
		End Function
		
		#End Region

		#Region "Download Custom Query"

		''' <summary>
		''' Exports a SQL query to CSV file.
		''' The QueryId is retrieved from:
		''' 1. p_query_id argument
		''' 2. Dashboard parameter 'prm_ETL_Load_Control_download_query'
		''' The query text is fetched from XFC_QUERY_DOWNLOAD table by that ID.
		''' </summary>
		Private Function DownloadCustomQuery(ByVal si As SessionInfo, ByVal targetUser As String, ByVal args As ExtenderArgs) As String
			Try
				'Get QueryId from multiple sources
				Dim queryIdStr As String = String.Empty
				
				'Source 1: Direct argument p_query_id
				queryIdStr = args.NameValuePairs.XFGetValue("p_query_id", String.Empty)
				BRApi.ErrorLog.LogMessage(si, $"[DEBUG] DownloadCustomQuery - p_query_id from args: '{queryIdStr}'")
				
				'Source 2: Try dashboard parameter
				If String.IsNullOrWhiteSpace(queryIdStr) Then
					Try
						queryIdStr = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "prm_ETL_Load_Control_download_query")
						BRApi.ErrorLog.LogMessage(si, $"[DEBUG] DownloadCustomQuery - prm from dashboard: '{queryIdStr}'")
					Catch ex As Exception
						BRApi.ErrorLog.LogMessage(si, $"[DEBUG] DownloadCustomQuery - Dashboard parameter error: {ex.Message}")
					End Try
				End If
				
				'Source 3: Check all args for any value that looks like a QueryId
				If String.IsNullOrWhiteSpace(queryIdStr) Then
					'Log all available arguments for debugging
					Dim allArgs As String = String.Join("; ", args.NameValuePairs.Keys.Select(Function(k) $"{k}={args.NameValuePairs(k)}"))
					BRApi.ErrorLog.LogMessage(si, $"[DEBUG] DownloadCustomQuery - All args: {allArgs}")
				End If
				
				If String.IsNullOrWhiteSpace(queryIdStr) Then
					Throw New XFException(si, New Exception("No QueryId provided. Please select a query from the table and try again. Pass p_query_id=<number> or configure the bound parameter."))
				End If
				
				'Parse QueryId
				Dim queryId As Integer
				If Not Integer.TryParse(queryIdStr.Trim(), queryId) Then
					Throw New XFException(si, New Exception($"Invalid QueryId: '{queryIdStr}'. Must be a number."))
				End If
				
				BRApi.ErrorLog.LogMessage(si, $"[INFO] DownloadCustomQuery - Looking up QueryId: {queryId}")
				
				'Fetch query from XFC_QUERY_DOWNLOAD table
				Dim sqlQuery As String = String.Empty
				Dim outputFileName As String = "CustomQuery"
				Dim queryName As String = String.Empty
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim lookupSql As String = "SELECT QueryName, QueryText, OutputFileName" & vbCrLf & _
						"FROM XFC_QUERY_DOWNLOAD" & vbCrLf & _
						"WHERE QueryId = " & queryId.ToString(CultureInfo.InvariantCulture)
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, lookupSql, Nothing, Nothing, False)
					
					If dt Is Nothing OrElse dt.Rows.Count = 0 Then
						Throw New XFException(si, New Exception($"Query not found. QueryId: {queryId}"))
					End If
					
					Dim row As DataRow = dt.Rows(0)
					queryName = If(IsDBNull(row("QueryName")), "", row("QueryName").ToString())
					sqlQuery = If(IsDBNull(row("QueryText")), "", row("QueryText").ToString())
					outputFileName = If(IsDBNull(row("OutputFileName")), queryName, row("OutputFileName").ToString())
					
					If String.IsNullOrEmpty(outputFileName) Then
						outputFileName = $"Query_{queryId}"
					End If
				End Using
				
				If String.IsNullOrWhiteSpace(sqlQuery) Then
					Throw New XFException(si, New Exception($"Query text is empty for QueryId: {queryId}"))
				End If
				
				'Log the query for debugging
				Dim queryLog As String = If(sqlQuery.Length > 500, sqlQuery.Substring(0, 500) & "...", sqlQuery)
				BRApi.ErrorLog.LogMessage(si, $"[INFO] DownloadCustomQuery - Executing '{queryName}' (len={sqlQuery.Length}): {queryLog}")
				
				'Clean the query
				sqlQuery = CleanSqlQuery(sqlQuery)
				
				If String.IsNullOrWhiteSpace(sqlQuery) Then
					Throw New XFException(si, New Exception("No valid SELECT statement found in query after cleaning."))
				End If
				
				'Export to CSV
				Dim rowCount As Integer = Me.ExportSqlToCSV(si, sqlQuery, outputFileName, targetUser)
				
				Dim message As String = $"✓ Query '{queryName}' exported: {rowCount} rows to {outputFileName}.csv"
				BRApi.ErrorLog.LogMessage(si, message)
				Return message
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Exports using the hardcoded DIRECT_QUERY constant.
		''' Usage: p_function=download_direct
		''' Edit DIRECT_QUERY constant at the top of this file to change the query.
		''' No character limits - write any query you need!
		''' </summary>
		Private Function DownloadDirectQuery(ByVal si As SessionInfo, ByVal targetUser As String) As String
			Try
				'Use the hardcoded query from DIRECT_QUERY constant
				Dim sqlQuery As String = DIRECT_QUERY
				
				If String.IsNullOrWhiteSpace(sqlQuery) Then
					Throw New XFException(si, New Exception("DIRECT_QUERY constant is empty. Please edit the query in the Business Rule code."))
				End If
				
				'Clean the query (remove extra whitespace, comments, etc.)
				sqlQuery = CleanSqlQuery(sqlQuery)
				
				If String.IsNullOrWhiteSpace(sqlQuery) Then
					Throw New XFException(si, New Exception("No valid SELECT statement found in DIRECT_QUERY after cleaning."))
				End If
				
				'Log the query
				Dim queryLog As String = If(sqlQuery.Length > 500, sqlQuery.Substring(0, 500) & "...", sqlQuery)
				BRApi.ErrorLog.LogMessage(si, $"[INFO] DownloadDirectQuery - Executing query (len={sqlQuery.Length}): {queryLog}")
				
				'Export to CSV using configured output name
				Dim rowCount As Integer = Me.ExportSqlToCSV(si, sqlQuery, DIRECT_QUERY_OUTPUT, targetUser)
				
				Dim message As String = $"✓ Direct Query exported: {rowCount} rows to {DIRECT_QUERY_OUTPUT}.csv"
				BRApi.ErrorLog.LogMessage(si, message)
				Return message
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Cleans a SQL query by removing comments, SET statements, and other non-SELECT content.
		''' Only allows SELECT statements for security.
		''' </summary>
		Private Function CleanSqlQuery(ByVal sqlQuery As String) As String
			If String.IsNullOrWhiteSpace(sqlQuery) Then
				Return String.Empty
			End If
			
			Dim cleaned As String = sqlQuery
			
			'Step 1: Remove multi-line comments /* ... */
			cleaned = Regex.Replace(cleaned, "/\*[\s\S]*?\*/", " ", RegexOptions.Multiline)
			
			'Step 2: Remove single-line comments -- ...
			cleaned = Regex.Replace(cleaned, "--[^\r\n]*", " ", RegexOptions.Multiline)
			
			'Step 3: Remove any SET statements anywhere in the query (SET ... ON/OFF or SET ... ;)
			'This handles: SET NOCOUNT ON, SET ANSI_NULLS ON, SET QUOTED_IDENTIFIER ON, etc.
			cleaned = Regex.Replace(cleaned, "\bSET\s+\w+\s+(ON|OFF)\s*;?\s*", " ", RegexOptions.IgnoreCase)
			cleaned = Regex.Replace(cleaned, "\bSET\s+[^;]+;\s*", " ", RegexOptions.IgnoreCase)
			
			'Step 4: Normalize whitespace (multiple spaces/newlines to single space)
			cleaned = Regex.Replace(cleaned, "\s+", " ").Trim()
			
			'Step 5: If there are multiple statements separated by semicolons, extract only the SELECT
			If cleaned.Contains(";") Then
				'Split by semicolon and find the SELECT statement
				Dim statements As String() = cleaned.Split(";"c)
				For Each stmt As String In statements
					Dim trimmedStmt As String = stmt.Trim()
					If trimmedStmt.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) Then
						cleaned = trimmedStmt
						Exit For
					End If
				Next
			End If
			
			'Step 6: Remove trailing semicolons
			cleaned = cleaned.TrimEnd(";"c).Trim()
			
			'Step 7: Security check - only allow SELECT statements
			'Find the SELECT and extract from there
			Dim selectIndex As Integer = cleaned.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase)
			If selectIndex >= 0 Then
				cleaned = cleaned.Substring(selectIndex)
			Else
				Return String.Empty 'No SELECT found
			End If
			
			Return cleaned
		End Function

		#End Region

		#Region "Import Landing from CSV"

		''' <summary>
		''' Imports a CSV file to XFC_LANDING_FIN_MIX table in APPEND mode (no delete).
		''' Usage: p_function=import_landing, p_filename=20250101000000-20250116000000
		''' The file is read from Documents/Users/{targetUser}/Exports/{filename}
		''' </summary>
		Private Function ImportLandingFromCSV(ByVal si As SessionInfo, ByVal targetUser As String, ByVal args As ExtenderArgs) As String
			Try
				Const TARGET_TABLE As String = "XFC_LANDING_FIN_MIX"
				
				'Get filename from arguments
				Dim fileName As String = args.NameValuePairs.XFGetValue("p_filename", String.Empty).Trim()
				
				If String.IsNullOrWhiteSpace(fileName) Then
					Throw New XFException(si, New Exception("No filename provided. Please pass 'p_filename' argument (e.g., p_filename=20250101000000-20250116000000)"))
				End If
				
				'Add extension if not present (files from split don't have .csv)
				Dim fileNameWithExt As String = fileName
				If Not fileName.ToLower().EndsWith(".csv") Then
					' Try without extension first (as exported from split script)
					fileNameWithExt = fileName
				End If
				
				Dim folderPath As String = $"Documents/Users/{targetUser}/{EXPORT_SUBFOLDER}"
				Dim filePath As String = $"{folderPath}/{fileNameWithExt}"
				
				BRApi.ErrorLog.LogMessage(si, $"[INFO] ImportLandingFromCSV - Loading file: {filePath}")
				
				'Check if file exists and read it
				Dim xfFileEx As XFFileEx = Nothing
				Try
					xfFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, False)
				Catch
					xfFileEx = Nothing
				End Try
				
				If xfFileEx Is Nothing OrElse xfFileEx.XFFile Is Nothing OrElse xfFileEx.XFFile.ContentFileBytes Is Nothing Then
					Throw New XFException(si, New Exception($"File not found: {filePath}"))
				End If
				
				'Read CSV file
				Dim fileBytes As Byte() = xfFileEx.XFFile.ContentFileBytes
				Dim csvContent As String = Encoding.UTF8.GetString(fileBytes)
				
				BRApi.ErrorLog.LogMessage(si, $"[INFO] ImportLandingFromCSV - File loaded, size: {fileBytes.Length / (1024*1024):F2} MB")
				
				'Import to table WITHOUT truncating (APPEND mode)
				Dim rowCount As Integer = Me.ImportCSVToLandingTable(si, TARGET_TABLE, csvContent)
				
				Dim message As String = $"✓ {fileName}: {rowCount:N0} rows imported to {TARGET_TABLE} (APPEND mode)"
				BRApi.ErrorLog.LogMessage(si, message)
				Return message
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Imports CSV content to Landing table in APPEND mode.
		''' Optimized for large files with batch inserts.
		''' </summary>
		Private Function ImportCSVToLandingTable(
			ByVal si As SessionInfo,
			ByVal tableName As String,
			ByVal csvContent As String
		) As Integer
			Try
				'Parse CSV content
				Dim lines As List(Of String) = ParseCSVLines(csvContent)
				
				If lines.Count < 2 Then
					Return 0
				End If
				
				'Get headers from first line
				Dim headers As String() = ParseCSVRow(lines(0))
				
				BRApi.ErrorLog.LogMessage(si, $"[INFO] ImportCSVToLandingTable - Headers: {String.Join(", ", headers)}")
				BRApi.ErrorLog.LogMessage(si, $"[INFO] ImportCSVToLandingTable - Total lines to process: {lines.Count - 1:N0}")
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					'NO TRUNCATE - APPEND mode
					
					'Build INSERT statements in batches
					Dim insertCount As Integer = 0
					Dim batchSize As Integer = 500  'Larger batch for performance
					Dim valuesList As New List(Of String)
					Dim batchNum As Integer = 0
					
					For i As Integer = 1 To lines.Count - 1
						If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
						
						Dim rowValues As String() = ParseCSVRow(lines(i))
						
						'Build values clause
						Dim sqlValues As New List(Of String)
						Try
							For idx As Integer = 0 To headers.Length - 1
								If idx >= rowValues.Length Then
									sqlValues.Add("NULL")
									Continue For
								End If
								
								Dim val As String = rowValues(idx)
								Dim headerName As String = headers(idx)
								
								If String.IsNullOrEmpty(val) Then
									sqlValues.Add("NULL")
								Else
									sqlValues.Add(ConvertValueForSQL(val, headerName))
								End If
							Next
						Catch ex As Exception
							BRApi.ErrorLog.LogMessage(si, $"[ERROR] Row {i} parsing error: {ex.Message}")
							BRApi.ErrorLog.LogMessage(si, $"[ERROR] Row data: {lines(i).Substring(0, Math.Min(500, lines(i).Length))}")
							Throw
						End Try
						
						valuesList.Add($"({String.Join(",", sqlValues)})")
						insertCount += 1
						
						'Execute batch when size reached
						If valuesList.Count >= batchSize Then
							batchNum += 1
							ExecuteInsertBatch(dbConn, tableName, headers, valuesList)
							valuesList.Clear()
							
							'Log progress every 10 batches
							If batchNum Mod 10 = 0 Then
								BRApi.ErrorLog.LogMessage(si, $"[INFO] Progress: {insertCount:N0} rows inserted...")
							End If
						End If
					Next
					
					'Execute remaining rows
					If valuesList.Count > 0 Then
						ExecuteInsertBatch(dbConn, tableName, headers, valuesList)
					End If
					
					BRApi.ErrorLog.LogMessage(si, $"[INFO] ImportCSVToLandingTable - Completed: {insertCount:N0} rows inserted")
					Return insertCount
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Export Table to CSV"
		
		''' <summary>
		''' Exports a single table to CSV file with fixed name {tableName}.csv
		''' Uses INVARIANT culture for portable format (decimals with dot, ISO dates)
		''' </summary>
		''' <returns>Number of rows exported</returns>
		Private Function ExportTableToCSV(
			ByVal si As SessionInfo,
			ByVal tableName As String,
			ByVal targetUser As String
		) As Integer
			Dim query As String = $"SELECT * FROM {tableName}"
			Return Me.ExportSqlToCSV(si, query, tableName, targetUser)
		End Function

		''' <summary>
		''' Exports the result of a SQL query to a CSV file.
		''' </summary>
		Private Function ExportSqlToCSV(
			ByVal si As SessionInfo,
			ByVal sqlQuery As String,
			ByVal fileNameWithoutExtension As String,
			ByVal targetUser As String
		) As Integer
			Try
				Dim fileName As String = $"{fileNameWithoutExtension}.csv"
				Dim parentPath As String = $"Documents/Users/{targetUser}"
				Dim folderPath As String = $"{parentPath}/{EXPORT_SUBFOLDER}"
				
				'Get data from database
				Dim dt As DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					dt = BRApi.Database.GetDataTable(dbConn, sqlQuery, Nothing, Nothing, False)
				End Using
				
				If dt Is Nothing Then
					Return 0
				End If
				
				'Build CSV content using INVARIANT culture
				Dim csvBuilder As New StringBuilder()
				
				'Add header row
				Dim columnNames As New List(Of String)
				For Each col As DataColumn In dt.Columns
					columnNames.Add(col.ColumnName)
				Next
				csvBuilder.AppendLine(String.Join(CSV_DELIMITER, columnNames))
				
				'Add data rows with INVARIANT formatting
				For Each row As DataRow In dt.Rows
					Dim values As New List(Of String)
					For Each col As DataColumn In dt.Columns
						Dim value As String
						
						If IsDBNull(row(col)) Then
							value = ""
						Else
							'Format based on data type using INVARIANT culture
							Dim cellValue As Object = row(col)
							
							If TypeOf cellValue Is DateTime Then
								'Format dates as ISO 8601
								value = DirectCast(cellValue, DateTime).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
							ElseIf TypeOf cellValue Is Boolean Then
								'Format booleans as 1/0 for SQL compatibility
								value = If(DirectCast(cellValue, Boolean), "1", "0")
							ElseIf TypeOf cellValue Is IFormattable Then
								'Handles ALL numeric types: Decimal, Double, Single, Int32, Int64, etc.
								'Using IFormattable ensures InvariantCulture (dot as decimal separator)
								'regardless of server regional settings
								value = DirectCast(cellValue, IFormattable).ToString(Nothing, CultureInfo.InvariantCulture)
							Else
								'All other types: use invariant culture
								value = Convert.ToString(cellValue, CultureInfo.InvariantCulture)
							End If
						End If
						
						'Escape commas, quotes, and line breaks in values per CSV RFC 4180
						'Escape delimiter, quotes, and line breaks in values
						If value.Contains(CSV_DELIMITER) OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
							value = """" & value.Replace("""", """""") & """"
						End If
						values.Add(value)
					Next
					csvBuilder.AppendLine(String.Join(CSV_DELIMITER, values))
				Next
				
				'Convert to bytes
				Dim csvBytes As Byte() = Encoding.UTF8.GetBytes(csvBuilder.ToString())
				
				'Create folder if necessary
				BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, EXPORT_SUBFOLDER)
				
				'Create XFFile object and set properties via FileInfo
				Dim xfFile As New XFFile()
				xfFile.FileInfo.Name = fileName
				xfFile.FileInfo.FolderFullName = folderPath
				xfFile.FileInfo.FileSystemLocation = FileSystemLocation.ApplicationDatabase
				xfFile.ContentFileBytes = csvBytes
				
				'Save file
				BRApi.FileSystem.InsertOrUpdateFile(si, xfFile)
				
				Return dt.Rows.Count
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Imports CSV content to a table.
		''' Handles IDENTITY columns, BIT values, dates, and European decimals.
		''' </summary>
		''' <param name="si">SessionInfo</param>
		''' <param name="tableName">Target table name</param>
		''' <param name="csvContent">CSV content as string</param>
		''' <param name="truncateFirst">Whether to truncate table before insert (default: True)</param>
		''' <returns>Number of rows imported</returns>
		Private Function ImportCSVToTable(
			ByVal si As SessionInfo,
			ByVal tableName As String,
			ByVal csvContent As String,
			Optional ByVal truncateFirst As Boolean = True
		) As Integer
			Try
				'Parse CSV content
				Dim lines As List(Of String) = ParseCSVLines(csvContent)
				
				If lines.Count < 2 Then
					'No data (only header or empty)
					Return 0
				End If
				
				'Get headers from first line
				Dim allHeaders As String() = ParseCSVRow(lines(0))
				
				'Check if table has IDENTITY column
				Dim hasIdentity As Boolean = IDENTITY_COLUMNS.ContainsKey(tableName)
				
				'Use all headers (do not exclude identity column)
				Dim headers As New List(Of String)
				Dim headerIndexes As New List(Of Integer)
				For idx As Integer = 0 To allHeaders.Length - 1
					headers.Add(allHeaders(idx))
					headerIndexes.Add(idx)
				Next
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					'TRUNCATE table first (if requested)
					If truncateFirst Then
						Dim truncateSql As String = $"TRUNCATE TABLE {tableName}"
						BRApi.Database.ExecuteSql(dbConn, truncateSql, False)
					End If
					
					'Enable IDENTITY_INSERT if needed
					If hasIdentity Then
						Try
							BRApi.Database.ExecuteSql(dbConn, $"SET IDENTITY_INSERT {tableName} ON", False)
						Catch
							'Ignore if fails
						End Try
					End If
					
					'Build INSERT statements in batches for better performance
					Dim insertCount As Integer = 0
					Dim batchSize As Integer = 100
					Dim valuesList As New List(Of String)
					
					For i As Integer = 1 To lines.Count - 1
						If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
						
						Dim allValues As String() = ParseCSVRow(lines(i))
						
						'Build values clause
						Dim sqlValues As New List(Of String)
						For Each idx As Integer In headerIndexes
							If idx >= allValues.Length Then
								sqlValues.Add("NULL")
								Continue For
							End If
							
							Dim val As String = allValues(idx)
							Dim headerName As String = allHeaders(idx)
							
							If String.IsNullOrEmpty(val) Then
								sqlValues.Add("NULL")
							Else
								'Convert value based on type
								sqlValues.Add(ConvertValueForSQL(val, headerName))
							End If
						Next
						
						valuesList.Add($"({String.Join(",", sqlValues)})")
						insertCount += 1
						
						'Execute batch when size reached
						If valuesList.Count >= batchSize Then
							ExecuteInsertBatch(dbConn, tableName, headers.ToArray(), valuesList)
							valuesList.Clear()
						End If
					Next
					
					'Execute remaining rows
					If valuesList.Count > 0 Then
						ExecuteInsertBatch(dbConn, tableName, headers.ToArray(), valuesList)
					End If
					
					'Disable IDENTITY_INSERT if needed
					If hasIdentity Then
						Try
							BRApi.Database.ExecuteSql(dbConn, $"SET IDENTITY_INSERT {tableName} OFF", False)
						Catch
							'Ignore
						End Try
					End If
					
					Return insertCount
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Converts a CSV value to proper SQL format.
		''' Expects INVARIANT format from export (decimals with dot, ISO dates, 1/0 for booleans)
		''' Also handles legacy European format for backwards compatibility.
		''' IMPORTANT: Values with leading zeros are treated as strings (SAP codes like 0000009100)
		''' </summary>
		Private Function ConvertValueForSQL(ByVal val As String, ByVal headerName As String) As String
			'Clean whitespace
			val = val.Trim()
			
			If String.IsNullOrEmpty(val) Then
				Return "NULL"
			End If
			
			Dim lowerHeader As String = headerName.ToLower()
			
			'Handle Boolean values (True/False -> 1/0, or already 1/0)
			If val.Equals("True", StringComparison.OrdinalIgnoreCase) Then
				Return "1"
			ElseIf val.Equals("False", StringComparison.OrdinalIgnoreCase) Then
				Return "0"
			End If
			
			'Known numeric columns - EXACT MATCH only (not contains)
			'Based on actual table schema - ONLY columns that are INT, BIGINT, DECIMAL in DB
			'XFC_LANDING_FIN_MIX schema: most columns are NVARCHAR(30), only these are numeric:
			'  - postingDate: INT
			'  - timestamp: BIGINT
			'  - amountInLocalCurrency: DECIMAL(18,2)
			'  - amountInTransactionCurrency: DECIMAL(18,2)
			'NOTE: customerNumber, documentNumber, postingKey, lineItemNumber, glAccountLineItem, 
			'      balanceSheetAccountGroup are ALL NVARCHAR - they must be strings!
			Dim numericColumns As String() = {
				"timestamp", "loadtimestamp", 
				"postingdate", "clearingdate",
				"amount", "amountinlocalcurrency", "amountintransactioncurrency", "taxbaseamount",
				"loadid", "ruleid", "id", "priority"
			}
			
			Dim isNumericColumn As Boolean = numericColumns.Contains(lowerHeader)
			
			If isNumericColumn Then
				'For known numeric columns, parse as number
				'This handles values with leading zeros like "0000009100" -> 9100
				Dim cleanVal As String = val
				'Remove any non-numeric characters except minus and dot
				cleanVal = Regex.Replace(cleanVal, "[^\d\.\-]", "")
				If String.IsNullOrEmpty(cleanVal) Then
					Return "NULL"
				End If
				'Verify it's a valid number and convert (removes leading zeros)
				Dim testNum As Decimal
				If Decimal.TryParse(cleanVal, NumberStyles.Any, CultureInfo.InvariantCulture, testNum) Then
					'Return the parsed number (this removes leading zeros)
					Return testNum.ToString(CultureInfo.InvariantCulture)
				End If
				Return "NULL"
			End If
			
			'Handle European decimal format (comma as decimal separator) - for backwards compatibility
			Dim europeanDecimalPattern As String = "^-?\d+,\d+$"
			If Regex.IsMatch(val, europeanDecimalPattern) Then
				'This is a decimal number with European format
				Return val.Replace(",", ".")
			End If
			
			'Handle ISO date format (yyyy-MM-dd) - new format from export
			Dim isoDatePattern As String = "^\d{4}-\d{2}-\d{2}"
			If Regex.IsMatch(val, isoDatePattern) Then
				Return $"'{val}'"
			End If
			
			'Handle legacy European date format (d/M/yyyy or with time)
			If val.Contains("/") Then
				Dim dateVal As DateTime
				Dim dateFormats As String() = {
					"d/M/yyyy H:mm:ss",
					"d/M/yyyy HH:mm:ss", 
					"d/M/yyyy",
					"dd/MM/yyyy H:mm:ss",
					"dd/MM/yyyy HH:mm:ss",
					"dd/MM/yyyy",
					"M/d/yyyy H:mm:ss",
					"M/d/yyyy"
				}
				
				If DateTime.TryParseExact(val, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, dateVal) Then
					Return $"'{dateVal.ToString("yyyy-MM-dd HH:mm:ss")}'"
				End If
			End If
			
			'For non-numeric columns: Check for leading zeros -> must be string (SAP codes)
			'Values like "0000009100", "000001" are SAP identifiers, NOT numbers
			If val.Length > 1 AndAlso val.StartsWith("0") AndAlso Regex.IsMatch(val, "^\d+$") Then
				'Has leading zeros and is all digits = SAP code, treat as string
				val = val.Replace("'", "''")
				Return $"N'{val}'"
			End If
			
			'REMOVED: Generic numeric pattern check
			'All non-numeric columns should be treated as strings, even if they look like numbers
			'This is because columns like documentNumber, postingKey are NVARCHAR in the DB
			
			'Default: treat as string (this covers documentNumber, postingKey, etc.)
			val = val.Replace("'", "''")
			Return $"N'{val}'"
		End Function
		
		''' <summary>
		''' Executes a batch INSERT statement.
		''' </summary>
		Private Sub ExecuteInsertBatch(
			ByVal dbConn As DbConnInfo,
			ByVal tableName As String,
			ByVal headers As String(),
			ByVal valuesList As List(Of String)
		)
			If valuesList.Count = 0 Then Return
			
			Dim columnList As String = String.Join(",", headers.Select(Function(h) $"[{h}]"))
			Dim insertSql As String = $"INSERT INTO {tableName} ({columnList}) VALUES {String.Join(",", valuesList)}"
			
			Try
				BRApi.Database.ExecuteSql(dbConn, insertSql, False)
			Catch ex As Exception
				'Build comprehensive error message
				Dim errorDetails As New StringBuilder()
				errorDetails.AppendLine($"SQL Error in batch insert to {tableName}")
				errorDetails.AppendLine($"Rows in batch: {valuesList.Count}")
				
				'Get full exception chain
				Dim currentEx As Exception = ex
				Dim exLevel As Integer = 0
				While currentEx IsNot Nothing
					errorDetails.AppendLine($"Exception[{exLevel}]: {currentEx.GetType().Name}: {currentEx.Message}")
					currentEx = currentEx.InnerException
					exLevel += 1
				End While
				
				'Log first row for debugging
				If valuesList.Count > 0 Then
					Dim firstRow As String = valuesList(0)
					If firstRow.Length > 800 Then firstRow = firstRow.Substring(0, 800) & "..."
					errorDetails.AppendLine($"First row: {firstRow}")
				End If
				
				'Log SQL statement (truncated)
				If insertSql.Length > 500 Then
					errorDetails.AppendLine($"SQL (truncated): {insertSql.Substring(0, 500)}...")
				End If
				
				Throw New Exception(errorDetails.ToString())
			End Try
		End Sub
		
		#End Region
		
		#Region "Download Cube Data (DISABLED)"
		
		' ══════════════════════════════════════════════════════════════════════════════
		' DISABLED: DownloadCubeData, ResolveMemberName, _memberNameCache
		'
		' Reason: BRApi.Finance.Data.GetDataBufferUsingMemberScript does not exist.
		'         DataBuffer APIs (GetDataBufferUsingFormula, GetDataBuffer) are only 
		'         available from Finance Rules context (api.Data), not from Extenders.
		'
		' Alternative: Use download_stage to extract source-specific data from 
		'              StageSourceData filtered by Workflow Profile (iScala vs SAP).
		' ══════════════════════════════════════════════════════════════════════════════
		
		''' <summary>
		''' Escapes a CSV value (handles delimiter, quotes, line breaks).
		''' Used by DownloadStageData and other CSV export functions.
		''' </summary>
		Private Function EscapeCsvValue(ByVal value As String) As String
			If String.IsNullOrEmpty(value) Then
				Return ""
			End If
			If value.Contains(CSV_DELIMITER) OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
				Return """" & value.Replace("""", """""") & """"
			End If
			Return value
		End Function
		
		#End Region
		
		#Region "Download Stage Data"
		
		''' <summary>
		''' Returns an ordered array of field names for the given source connector.
		''' These correspond to F0, F1, F2... columns in StageSourceData.
		''' </summary>
		Private Function GetConnectorFieldNames(ByVal source As String) As String()
			Select Case source.ToLower()
				Case "iscala"
					Return New String() {"PERIOD", "Entity", "Intercompany", "Account", "Movement", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD8", "Amount", "LocalAccount", "TransactionNo"}
				Case "sap"
					Return New String() {"iScalaLineItemsID", "PERIOD", "Entity", "RunningCount", "Intercompany", "Account", "Movement", "UD1", "UD2", "UD3", "UD4", "UD5", "UD6", "UD8", "Amount", "TransactionCurrency", "TransactionNo", "TransactionText", "PLReference", "CompanyCode", "SourceCompany", "InvoiceNumber", "Supplier", "SupplierName", "EntryDate", "EntryUser", "TransactionDate", "LocalAccount", "GroupCodeDescription", "CompanyTypeDescription", "SourceCompanyTypeDescription", "CostCenterAllocation", "Customer", "CustomerName"}
				Case Else
					Return Nothing
			End Select
		End Function
		
		''' <summary>
		''' Exports data from the StageSourceData table to CSV, filtered by connector source.
		''' This allows extracting ONLY iScala data or ONLY SAP data from the stage area,
		''' since the cube mixes both sources.
		'''
		''' The function identifies Workflow Profiles associated with each source type:
		'''   - iScala: profiles with AutoLoad=True that do NOT end in '2_Automatic_Import_SAP'
		'''   - SAP:    profiles with AutoLoad=True that end in '2_Automatic_Import_SAP'
		'''
		''' Required parameters:
		'''   p_period  = OneStream period name (e.g., 2025M06)
		'''
		''' Optional parameters:
		'''   p_source  = iscala | sap | all (default: iscala)
		'''   p_scenario = Scenario name (default: Actual)
		'''   p_output  = Output filename without extension (default: StageExport_{source}_{period})
		'''   p_field_names = Comma-separated column aliases for F0,F1,F2... (overrides defaults)
		'''   p_max_fields  = Max number of Fn columns to read (default: auto-detect)
		''' </summary>
		Private Function DownloadStageData(ByVal si As SessionInfo, ByVal targetUser As String, ByVal args As ExtenderArgs) As String
			Try
				' ── Get Parameters ──
				Dim period As String = args.NameValuePairs.XFGetValue("p_period", String.Empty).Trim()
				Dim source As String = args.NameValuePairs.XFGetValue("p_source", "iscala").Trim().ToLower()
				Dim scenario As String = args.NameValuePairs.XFGetValue("p_scenario", "Actual").Trim()
				Dim outputName As String = args.NameValuePairs.XFGetValue("p_output", String.Empty).Trim()
				Dim fieldNamesParam As String = args.NameValuePairs.XFGetValue("p_field_names", String.Empty).Trim()
				Dim maxFieldsParam As String = args.NameValuePairs.XFGetValue("p_max_fields", String.Empty).Trim()
				
				' ── Validate Required Parameters ──
				If String.IsNullOrWhiteSpace(period) Then
					Throw New XFException(si, New Exception("p_period is required (e.g., p_period=2025M06)"))
				End If
				
				If source <> "iscala" AndAlso source <> "sap" AndAlso source <> "all" Then
					Throw New XFException(si, New Exception("p_source must be: iscala | sap | all"))
				End If
				
				' Default output name
				If String.IsNullOrWhiteSpace(outputName) Then
					outputName = "StageExport_" & source & "_" & period
				End If
				
				BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Period: " & period & ", Source: " & source & ", Scenario: " & scenario)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' ══════════════════════════════════════════════════════════════════════
					' STEP 1: Discover StageSourceData columns dynamically
					' ══════════════════════════════════════════════════════════════════════
					Dim schemaSql As String = "SELECT COLUMN_NAME, ORDINAL_POSITION" & vbCrLf & _
						"FROM INFORMATION_SCHEMA.COLUMNS" & vbCrLf & _
						"WHERE TABLE_NAME = 'StageSourceData'" & vbCrLf & _
						"ORDER BY ORDINAL_POSITION"
					
					Dim dtSchema As DataTable = BRApi.Database.GetDataTable(dbConn, schemaSql, Nothing, Nothing, False)
					
					If dtSchema.Rows.Count = 0 Then
						Throw New XFException(si, New Exception("StageSourceData table not found in INFORMATION_SCHEMA"))
					End If
					
					' Separate key columns from data columns
					Dim keyColumns As New List(Of String)()
					keyColumns.Add("Wfk")
					keyColumns.Add("Wsk")
					keyColumns.Add("Wtk")
					keyColumns.Add("Ri")
					Dim dataColumns As New List(Of String)()
					Dim allColumns As New List(Of String)()
					
					For Each row As DataRow In dtSchema.Rows
						Dim colName As String = row("COLUMN_NAME").ToString()
						allColumns.Add(colName)
						Dim isKeyCol As Boolean = False
						For Each kc As String In keyColumns
							If String.Equals(kc, colName, StringComparison.OrdinalIgnoreCase) Then
								isKeyCol = True
								Exit For
							End If
						Next
						If Not isKeyCol Then
							dataColumns.Add(colName)
						End If
					Next
					
					Dim colPreview As String = String.Join(", ", dataColumns.Take(20).ToArray())
					If dataColumns.Count > 20 Then colPreview = colPreview & "..."
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Schema discovered: " & allColumns.Count.ToString() & " total columns, " & dataColumns.Count.ToString() & " data columns")
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Data columns: " & colPreview)
					' Determine how many data columns to include
					Dim maxFields As Integer = dataColumns.Count
					If Not String.IsNullOrWhiteSpace(maxFieldsParam) Then
						Integer.TryParse(maxFieldsParam, maxFields)
						maxFields = Math.Min(maxFields, dataColumns.Count)
					End If
					
					' ══════════════════════════════════════════════════════════════════════
					' STEP 2: Find Workflow Profile Keys for the specified source type
					' ══════════════════════════════════════════════════════════════════════
					Dim sapSuffix As String = "2_Automatic_Import_SAP"
					Dim profileFilter As String = ""
					If source = "iscala" Then
						profileFilter = "AND wpfh.ProfileName NOT LIKE '%" & sapSuffix & "'"
					ElseIf source = "sap" Then
						profileFilter = "AND wpfh.ProfileName LIKE '%" & sapSuffix & "'"
					End If
					
					' Query Workflow Profile metadata to find relevant profiles
					' Same logic as GetAutoLoadWorkflowProfiles in Workflow_Year_Autoload.vb:
					'   - CubeName = Cube_100_Group
					'   - ProfileType = 50 (import profiles)
					'   - Active = 1 (AttributeIndex 1300)
					'   - Text2 = AutoLoad (AttributeIndex 20000)
					Dim profileSql As String = "SELECT wpfh.ProfileKey, wpfh.ProfileName" & vbCrLf & _
						" FROM dbo.WorkflowProfileHierarchy wpfh" & vbCrLf & _
						" INNER JOIN dbo.WorkflowProfileAttributes wpa ON wpfh.ProfileKey = wpa.ProfileKey" & vbCrLf & _
						" WHERE wpfh.CubeName = 'Cube_100_Group'" & vbCrLf & _
						" AND wpfh.ProfileType = '50'" & vbCrLf & _
						" AND wpa.ScenarioTypeID = '0'" & vbCrLf & _
						" " & profileFilter & vbCrLf & _
						" GROUP BY wpfh.ProfileKey, wpfh.ProfileName" & vbCrLf & _
						" HAVING SUM(CASE WHEN wpa.AttributeIndex = '20000' AND wpa.ProfileAttributeValue = 'AutoLoad' THEN 1 ELSE 0 END) > 0" & vbCrLf & _
						" AND SUM(CASE WHEN wpa.AttributeIndex = '1300' AND wpa.ProfileAttributeValue = '1' THEN 1 ELSE 0 END) > 0" & vbCrLf & _
						" ORDER BY wpfh.ProfileName"
					
					Dim dtProfiles As DataTable = BRApi.Database.GetDataTable(dbConn, profileSql, Nothing, Nothing, False)
					
					If dtProfiles.Rows.Count = 0 Then
						Return "No workflow profiles found for source '" & source & "'. Check profile configuration."
					End If
					
					' Collect profile keys and names
					' ProfileKey/Wfk can be Integer or Guid depending on environment.
					Dim profileKeys As New List(Of String)()
					Dim profileNames As New List(Of String)()
					For Each row As DataRow In dtProfiles.Rows
						Dim profileKeyValue As String = If(row.IsNull("ProfileKey"), String.Empty, row("ProfileKey").ToString().Trim())
						Dim profileNameValue As String = If(row.IsNull("ProfileName"), String.Empty, row("ProfileName").ToString())
						If String.IsNullOrWhiteSpace(profileKeyValue) Then
							Throw New XFException(si, New Exception("Workflow profile key is empty for profile '" & profileNameValue & "'."))
						End If
						profileKeys.Add(profileKeyValue)
						profileNames.Add(profileNameValue)
					Next
					
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Found " & profileKeys.Count.ToString() & " profiles for source '" & source & "': " & String.Join(", ", profileNames.ToArray()))
					
					' ══════════════════════════════════════════════════════════════════════
					' STEP 3: Resolve Time Key from period name
					' ══════════════════════════════════════════════════════════════════════
					' Use the first profile to get the WorkflowUnitClusterPk which contains TimeKey
					Dim firstProfile As String = profileNames(0)
					Dim wfCluster As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(
						si, firstProfile, scenario, period)
					
					Dim timeKey As Integer = wfCluster.TimeKey
					Dim scenarioKey As Integer = wfCluster.ScenarioKey
					
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Resolved keys: TimeKey=" & timeKey.ToString() & ", ScenarioKey=" & scenarioKey.ToString() & " (from profile '" & firstProfile & "', scenario '" & scenario & "', period '" & period & "')")
					
					' ══════════════════════════════════════════════════════════════════════
					' STEP 4: Build and execute the Stage query
					' ══════════════════════════════════════════════════════════════════════
					Dim escapedProfileKeys As New List(Of String)()
					For Each pk As String In profileKeys
						escapedProfileKeys.Add("'" & pk.Replace("'", "''") & "'")
					Next
					Dim profileKeysList As String = String.Join(",", escapedProfileKeys.ToArray())
					
					' Build SELECT clause with data columns (limited by maxFields)
					Dim selectColumns As New List(Of String)()
					selectColumns.Add("[Wfk]")
					selectColumns.Add("[Wtk]")
					selectColumns.Add("[Ri]")
					For i As Integer = 0 To maxFields - 1
						selectColumns.Add("[" & dataColumns(i) & "]")
					Next
					
					Dim stageSql As String = "SELECT " & String.Join(", ", selectColumns.ToArray()) & vbCrLf & _
						" FROM dbo.StageSourceData" & vbCrLf & _
						" WHERE [Wfk] IN (" & profileKeysList & ")" & vbCrLf & _
						" AND [Wtk] = " & timeKey.ToString() & vbCrLf & _
						" ORDER BY [Wfk], [Ri]"
					
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Executing query with " & selectColumns.Count.ToString() & " columns, Wfk IN (" & profileKeysList & "), Wtk=" & timeKey.ToString())
					
					Dim dtStage As DataTable = BRApi.Database.GetDataTable(dbConn, stageSql, Nothing, Nothing, False)
					
					If dtStage.Rows.Count = 0 Then
						Return "No stage data found for source '" & source & "', period '" & period & "'. ProfileKeys: " & profileKeysList & ", TimeKey: " & timeKey.ToString()
					End If
					
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Query returned " & dtStage.Rows.Count.ToString() & " rows")
					
					' ══════════════════════════════════════════════════════════════════════
					' STEP 5: Determine column name aliases for the CSV header
					' ══════════════════════════════════════════════════════════════════════
					Dim headerNames As New List(Of String)()
					
					' User-provided field names take priority
					If Not String.IsNullOrWhiteSpace(fieldNamesParam) Then
						Dim customNames As String() = fieldNamesParam.Split(New Char() {","c}, StringSplitOptions.None)
						For i As Integer = 0 To maxFields - 1
							If i < customNames.Length AndAlso Not String.IsNullOrWhiteSpace(customNames(i).Trim()) Then
								headerNames.Add(customNames(i).Trim())
							Else
								headerNames.Add(dataColumns(i))
							End If
						Next
					Else
						' Try known field mappings for the connector type
						Dim knownFields As String() = GetConnectorFieldNames(source)
						If knownFields IsNot Nothing Then
							For i As Integer = 0 To maxFields - 1
								If i < knownFields.Length Then
									headerNames.Add(knownFields(i))
								Else
									headerNames.Add(dataColumns(i))
								End If
							Next
						Else
							' Use raw column names (F0, F1, ... or whatever the table has)
							For i As Integer = 0 To maxFields - 1
								headerNames.Add(dataColumns(i))
							Next
						End If
					End If
					
					' ══════════════════════════════════════════════════════════════════════
					' STEP 6: Build CSV output
					' ══════════════════════════════════════════════════════════════════════
					Dim csvBuilder As New StringBuilder()
					
					' Build header: ProfileName, Period + data field names
					Dim csvHeader As New List(Of String)()
					csvHeader.Add("WorkflowProfile")
					csvHeader.Add("Period")
					csvHeader.Add("RecordIndex")
					csvHeader.AddRange(headerNames)
					Dim escapedHeader As New List(Of String)()
					For Each h As String In csvHeader
						escapedHeader.Add(EscapeCsvValue(h))
					Next
					csvBuilder.AppendLine(String.Join(CSV_DELIMITER, escapedHeader.ToArray()))
					
					' Build a lookup: ProfileKey -> ProfileName for readable output
					Dim profileLookup As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
					For idx As Integer = 0 To profileKeys.Count - 1
						If Not profileLookup.ContainsKey(profileKeys(idx)) Then
							profileLookup.Add(profileKeys(idx), profileNames(idx))
						End If
					Next
					
					' Process each row
					Dim rowCount As Integer = 0
					For Each row As DataRow In dtStage.Rows
						Dim values As New List(Of String)()
						
						' Add profile name (resolve Wfk to profile name)
						Dim wfk As String = If(row.IsNull("Wfk"), String.Empty, row("Wfk").ToString().Trim())
						Dim profName As String = If(profileLookup.ContainsKey(wfk), profileLookup(wfk), wfk)
						values.Add(EscapeCsvValue(profName))
						
						' Add period name
						values.Add(EscapeCsvValue(period))
						
						' Add record index
						values.Add(row("Ri").ToString())
						
						' Add data columns (use InvariantCulture to preserve decimal separators)
						For i As Integer = 0 To maxFields - 1
							Dim colName As String = dataColumns(i)
							Dim cellValue As String
							If row.IsNull(colName) Then
								cellValue = ""
							ElseIf TypeOf row(colName) Is IFormattable Then
								cellValue = DirectCast(row(colName), IFormattable).ToString(Nothing, CultureInfo.InvariantCulture)
							Else
								cellValue = Convert.ToString(row(colName), CultureInfo.InvariantCulture)
							End If
							values.Add(EscapeCsvValue(cellValue))
						Next
						
						csvBuilder.AppendLine(String.Join(CSV_DELIMITER, values.ToArray()))
						rowCount += 1
					Next
					
					' ══════════════════════════════════════════════════════════════════════
					' STEP 7: Save CSV file
					' ══════════════════════════════════════════════════════════════════════
					Dim fileName As String = outputName & ".csv"
					Dim parentPath As String = "Documents/Users/" & targetUser
					Dim folderPath As String = parentPath & "/" & EXPORT_SUBFOLDER
					
					Dim csvBytes As Byte() = Encoding.UTF8.GetBytes(csvBuilder.ToString())
					Dim fileSizeMB As Double = csvBytes.Length / (1024 * 1024)
					
					BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, parentPath, EXPORT_SUBFOLDER)
					
					Dim xfFile As New XFFile()
					xfFile.FileInfo.Name = fileName
					xfFile.FileInfo.FolderFullName = folderPath
					xfFile.FileInfo.FileSystemLocation = FileSystemLocation.ApplicationDatabase
					xfFile.ContentFileBytes = csvBytes
					
					BRApi.FileSystem.InsertOrUpdateFile(si, xfFile)
					
					Dim message As String = "Stage data exported: " & rowCount.ToString("N0") & " rows from " & profileKeys.Count.ToString() & " profiles (" & source & ") for period " & period & " (" & fileSizeMB.ToString("F2") & " MB) to " & folderPath & "/" & fileName
					BRApi.ErrorLog.LogMessage(si, message)
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Profiles used: " & String.Join(", ", profileNames.ToArray()))
					
					' Log column mapping
					Dim mappingParts As New List(Of String)()
					Dim mappingLimit As Integer = Math.Min(maxFields, headerNames.Count)
					For mi As Integer = 0 To mappingLimit - 1
						mappingParts.Add(dataColumns(mi) & "->" & headerNames(mi))
					Next
					BRApi.ErrorLog.LogMessage(si, "[INFO] DownloadStageData - Column mapping: " & String.Join(", ", mappingParts.ToArray()))
					
					Return message
					
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "CSV Parsing Helpers"
		
		''' <summary>
		''' Splits CSV content into lines, handling quoted values with line breaks.
		''' </summary>
		Private Function ParseCSVLines(ByVal csvContent As String) As List(Of String)
			Dim lines As New List(Of String)
			Dim currentLine As New StringBuilder()
			Dim inQuotes As Boolean = False
			
			For Each c As Char In csvContent
				If c = """"c Then
					inQuotes = Not inQuotes
					currentLine.Append(c)
				ElseIf (c = vbCr(0) OrElse c = vbLf(0)) AndAlso Not inQuotes Then
					If currentLine.Length > 0 Then
						lines.Add(currentLine.ToString())
						currentLine.Clear()
					End If
				Else
					currentLine.Append(c)
				End If
			Next
			
			'Add last line if any
			If currentLine.Length > 0 Then
				lines.Add(currentLine.ToString())
			End If
			
			Return lines
		End Function
		
		''' <summary>
		''' Parses a single CSV row into values, handling quoted fields per RFC 4180.
		''' Uses CSV_DELIMITER (;) as field separator.
		''' </summary>
		Private Function ParseCSVRow(ByVal line As String) As String()
			Dim values As New List(Of String)
			Dim currentValue As New StringBuilder()
			Dim inQuotes As Boolean = False
			Dim i As Integer = 0
			
			While i < line.Length
				Dim c As Char = line(i)
				
				If inQuotes Then
					If c = """"c Then
						'Check for escaped quote
						If i + 1 < line.Length AndAlso line(i + 1) = """"c Then
							currentValue.Append(""""c)
							i += 1
						Else
							inQuotes = False
						End If
					Else
						currentValue.Append(c)
					End If
				Else
					If c = """"c Then
						inQuotes = True
					ElseIf c = CSV_DELIMITER Then
						values.Add(currentValue.ToString())
						currentValue.Clear()
					Else
						currentValue.Append(c)
					End If
				End If
				
				i += 1
			End While
			
			'Add last value
			values.Add(currentValue.ToString())
			
			Return values.ToArray()
		End Function
		
		#End Region
		
	End Class
End Namespace