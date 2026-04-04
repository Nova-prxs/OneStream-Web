Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
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

Namespace OneStream.BusinessRule.Extender.TRS_Extender_Export_CSV
	
	''' <summary>
	''' Business Rule to export/import Treasury tables to/from CSV files.
	''' 
	''' Usage:
	''' - p_function=Download : Exports all Treasury tables to CSV files (fixed names)
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
		''' List of all Treasury tables to export/import.
		''' </summary>
		Private Shared ReadOnly TREASURY_TABLES As String() = New String() {
			"XFC_TRS_AUX_Date",
			"XFC_TRS_AUX_TreasuryWeekConfirm",
			"XFC_TRS_RAW_CashDebtPosition",
			"XFC_TRS_RAW_CashFlowForecasting",
			"XFC_TRS_Master_CashDebtPosition",
			"XFC_TRS_Master_CashflowForecasting",
			"XFC_TRS_MASTER_Treasury_Monitoring",
			"XFC_TRS_Master_Banks",
			"XFC_TRS_MASTER_Companies"
		}
		
		''' <summary>
		''' Default target user folder. If empty, uses the current logged-in user.
		''' Examples: "AurelioSantos", "AdminUser", "" (for current user)
		''' </summary>
		Private Const DEFAULT_TARGET_USER As String = "Aurelio.Nova"
		
		''' <summary>
		''' Subfolder name inside the user's Documents folder where exports are saved.
		''' </summary>
		Private Const EXPORT_SUBFOLDER As String = "Exports"
		
		'===============================================================================
		
		#End Region
		
		#Region "Main Entry Point"
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				'Get function parameter
				Dim functionParam As String = args.NameValuePairs.XFGetValue("p_function", String.Empty).Trim().ToLower()
				Dim targetUser As String = If(String.IsNullOrEmpty(DEFAULT_TARGET_USER), si.UserName, DEFAULT_TARGET_USER)
				
				Select Case functionParam
					
					Case "download"
						'Export all Treasury tables to CSV
						Return Me.DownloadAllTables(si, targetUser)
						
					Case "import"
						'Import all CSV files back to tables
						Return Me.ImportAllTables(si, targetUser)
						
					Case Else
						'Default: show usage instructions
						Return "Usage: p_function=Download (export tables to CSV) | p_function=Import (import CSV to tables)"
						
				End Select
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
		#Region "Download All Tables"
		
		''' <summary>
		''' Exports all Treasury tables to CSV files with fixed names.
		''' </summary>
		Private Function DownloadAllTables(ByVal si As SessionInfo, ByVal targetUser As String) As String
			Dim results As New StringBuilder()
			Dim successCount As Integer = 0
			Dim errorCount As Integer = 0
			
			results.AppendLine("=== Treasury Tables CSV Export ===")
			results.AppendLine($"Target folder: Documents/Users/{targetUser}/{EXPORT_SUBFOLDER}")
			results.AppendLine("")
			
			For Each tableName As String In TREASURY_TABLES
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
		''' Performs TRUNCATE before INSERT for each table.
		''' </summary>
		Private Function ImportAllTables(ByVal si As SessionInfo, ByVal targetUser As String) As String
			Dim results As New StringBuilder()
			Dim successCount As Integer = 0
			Dim errorCount As Integer = 0
			Dim skippedCount As Integer = 0
			
			Dim folderPath As String = $"Documents/Users/{targetUser}/{EXPORT_SUBFOLDER}"
			
			results.AppendLine("=== Treasury Tables CSV Import ===")
			results.AppendLine($"Source folder: {folderPath}")
			results.AppendLine("")
			
			For Each tableName As String In TREASURY_TABLES
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
					
					'Import to table
					Dim rowCount As Integer = Me.ImportCSVToTable(si, tableName, csvContent)
					results.AppendLine($"✓ {tableName}: {rowCount} rows imported")
					successCount += 1
					
				Catch ex As Exception
					results.AppendLine($"✗ {tableName}: ERROR - {ex.Message}")
					errorCount += 1
				End Try
			Next
			
			results.AppendLine("")
			results.AppendLine($"=== Summary: {successCount} imported, {skippedCount} skipped, {errorCount} errors ===")
			
			BRApi.ErrorLog.LogMessage(si, results.ToString())
			Return results.ToString()
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
			Try
				'Fixed filename (no timestamp)
				Dim fileName As String = $"{tableName}.csv"
				Dim parentPath As String = $"Documents/Users/{targetUser}"
				Dim folderPath As String = $"{parentPath}/{EXPORT_SUBFOLDER}"
				
				'Build query - export full table
				Dim query As String = $"SELECT * FROM {tableName}"
				
				'Get data from table
				Dim dt As DataTable
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					dt = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
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
				csvBuilder.AppendLine(String.Join(",", columnNames))
				
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
								value = DirectCast(cellValue, DateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
							ElseIf TypeOf cellValue Is Decimal OrElse TypeOf cellValue Is Double OrElse TypeOf cellValue Is Single Then
								'Format decimals with dot separator
								value = Convert.ToDecimal(cellValue).ToString(CultureInfo.InvariantCulture)
							ElseIf TypeOf cellValue Is Boolean Then
								'Format booleans as 1/0 for SQL compatibility
								value = If(DirectCast(cellValue, Boolean), "1", "0")
							Else
								'All other types: use invariant culture
								value = Convert.ToString(cellValue, CultureInfo.InvariantCulture)
							End If
						End If
						
						'Escape commas, quotes, and line breaks in values per CSV RFC 4180
						If value.Contains(",") OrElse value.Contains("""") OrElse value.Contains(vbCr) OrElse value.Contains(vbLf) Then
							value = """" & value.Replace("""", """""") & """"
						End If
						values.Add(value)
					Next
					csvBuilder.AppendLine(String.Join(",", values))
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
				
				'Save file to OneStream File System
				BRApi.FileSystem.InsertOrUpdateFile(si, xfFile)
				
				Return dt.Rows.Count
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Import CSV to Table"
		
		''' <summary>
		''' Tables with IDENTITY columns that need special handling.
		''' </summary>
		Private Shared ReadOnly IDENTITY_COLUMNS As New Dictionary(Of String, String) From {
			{"XFC_TRS_Master_CashDebtPosition", "RowId"},
			{"XFC_TRS_Master_CashflowForecasting", "RowId"}
		}
		
		''' <summary>
		''' Imports CSV content to a table. Performs TRUNCATE then INSERT.
		''' Handles IDENTITY columns, BIT values, dates, and European decimals.
		''' </summary>
		''' <param name="si">SessionInfo</param>
		''' <param name="tableName">Target table name</param>
		''' <param name="csvContent">CSV content as string</param>
		''' <returns>Number of rows imported</returns>
		Private Function ImportCSVToTable(
			ByVal si As SessionInfo,
			ByVal tableName As String,
			ByVal csvContent As String
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
				
				'Check if table has IDENTITY column to exclude
				Dim identityColumn As String = Nothing
				If IDENTITY_COLUMNS.ContainsKey(tableName) Then
					identityColumn = IDENTITY_COLUMNS(tableName)
				End If
				
				'Filter headers (exclude IDENTITY column)
				Dim headers As New List(Of String)
				Dim headerIndexes As New List(Of Integer)
				For idx As Integer = 0 To allHeaders.Length - 1
					If identityColumn Is Nothing OrElse Not allHeaders(idx).Equals(identityColumn, StringComparison.OrdinalIgnoreCase) Then
						headers.Add(allHeaders(idx))
						headerIndexes.Add(idx)
					End If
				Next
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					'TRUNCATE table first
					Dim truncateSql As String = $"TRUNCATE TABLE {tableName}"
					BRApi.Database.ExecuteSql(dbConn, truncateSql, False)
					
					'Build INSERT statements in batches for better performance
					Dim insertCount As Integer = 0
					Dim batchSize As Integer = 100
					Dim valuesList As New List(Of String)
					
					For i As Integer = 1 To lines.Count - 1
						If String.IsNullOrWhiteSpace(lines(i)) Then Continue For
						
						Dim allValues As String() = ParseCSVRow(lines(i))
						
						'Build values clause (only for non-IDENTITY columns)
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
		''' </summary>
		Private Function ConvertValueForSQL(ByVal val As String, ByVal headerName As String) As String
			'Handle Boolean values (True/False -> 1/0, or already 1/0)
			If val.Equals("True", StringComparison.OrdinalIgnoreCase) OrElse val = "1" Then
				Return "1"
			ElseIf val.Equals("False", StringComparison.OrdinalIgnoreCase) OrElse val = "0" Then
				'Check if it's really a boolean column or just a number 0
				Dim lowerHeader As String = headerName.ToLower()
				If lowerHeader.Contains("active") OrElse lowerHeader.Contains("ismonthend") OrElse 
				   lowerHeader.Contains("cashdebt") OrElse lowerHeader.Contains("cashflow") Then
					Return "0"
				End If
				'Otherwise treat as number
				Return val
			End If
			
			'Handle European decimal format (comma as decimal separator) - for backwards compatibility
			Dim europeanDecimalPattern As String = "^-?\d+,\d+$"
			If Regex.IsMatch(val, europeanDecimalPattern) Then
				Return val.Replace(",", ".")
			End If
			
			'Handle ISO date format (yyyy-MM-dd) - new format from export
			Dim isoDatePattern As String = "^\d{4}-\d{2}-\d{2}$"
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
					Return $"'{dateVal.ToString("yyyy-MM-dd")}'"
				End If
			End If
			
			'Handle pure numbers (integers and decimals with dot)
			Dim decVal As Decimal
			If Decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, decVal) Then
				Return val
			End If
			
			'Default: treat as string
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
			
			BRApi.Database.ExecuteSql(dbConn, insertSql, False)
		End Sub
		
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
					ElseIf c = ","c Then
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