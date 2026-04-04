Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Wordprocessing
Imports DocumentFormat.OpenXml.Spreadsheet
'------------------------------------------------------------------------------------------------------------
' TRS_ReportGenerator_Comment
' Business Rule Type: Extender (Data Management Step)
' Purpose: Creates snapshot copies of weekly comment DOCX files to fixed locations for PPT XFDoc
'          Also creates Excel files with the comment text for PPT rendering
'          Processes 3 independent scenarios: StartWeek, EOM, EOW
'
' Data Management Step Configuration:
' p_function=SnapshotComment, paramWeek=[|!p_week!|], paramYear=[|!p_year!|]
' p_function=SnapshotCommentPastWeek, paramWeek=[|!p_week!|], paramYear=[|!p_year!|]  (uses paramWeek - 1)
'
' This rule reads the comment DOCX from:
'   Documents/Public/Treasury/Extensible Document/Comments/{year}/{week}/{scenario}_{week}_{year}.docx
'
' And creates (for each scenario that exists):
'   Current Week:
'     Documents/Public/Treasury/Extensible Document/Export/ActiveComment_StartWeek.docx/.xlsx
'     Documents/Public/Treasury/Extensible Document/Export/ActiveComment_EOM.docx/.xlsx
'     Documents/Public/Treasury/Extensible Document/Export/ActiveComment_EOW.docx/.xlsx
'
'   Past Week (_PastWeek suffix):
'     Documents/Public/Treasury/Extensible Document/Export/ActiveComment_StartWeek_PastWeek.docx/.xlsx
'     Documents/Public/Treasury/Extensible Document/Export/ActiveComment_EOM_PastWeek.docx/.xlsx
'     Documents/Public/Treasury/Extensible Document/Export/ActiveComment_EOW_PastWeek.docx/.xlsx
'
' The PPT XFDoc can reference the Excel files using:
'   {XF}{Application}{ExcelFile}{Documents/Public/Treasury/Extensible Document/Export/ActiveComment_StartWeek.xlsx}
'   {XF}{Application}{ExcelFile}{Documents/Public/Treasury/Extensible Document/Export/ActiveComment_StartWeek_PastWeek.xlsx}
'------------------------------------------------------------------------------------------------------------

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.TRS_ReportGenerator_Comment
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Dim functionName As String = args.NameValuePairs.XFGetValue("p_function", String.Empty)
				
				Select Case functionName.ToUpperInvariant()
					
					Case "SNAPSHOTCOMMENT"
						Return Me.SnapshotComment(si, args)
					
					Case "SNAPSHOTCOMMENTPASTWEEK"
						Return Me.SnapshotCommentPastWeek(si, args)
					
					Case "GENERATEREPORTWEEKINFO"
						Return Me.GenerateReportWeekInfo(si, args)
					
					Case Else
						Return Nothing
						
				End Select
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' SnapshotComment: Copies the weekly comment DOCX files to fixed locations for PPT XFDoc
		' Processes 3 independent scenarios: StartWeek, EOM, EOW
		'
		' Parameters:
		'   paramWeek - The week number (e.g., "37")
		'   paramYear - The year (e.g., "2025")
		'
		' Data Management Step:
		'   p_function=SnapshotComment, paramWeek=[|!p_week!|], paramYear=[|!p_year!|]
		'------------------------------------------------------------------------------------------------------------
		Private Function SnapshotComment(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As Object
			'Get parameters
			Dim paramWeek As String = args.NameValuePairs.XFGetValue("paramWeek", String.Empty)
			Dim paramYear As String = args.NameValuePairs.XFGetValue("paramYear", String.Empty)
			
			'Validate parameters
			If String.IsNullOrEmpty(paramWeek) OrElse String.IsNullOrEmpty(paramYear) Then
				BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: Missing required parameters (paramWeek, paramYear)")
				Return Nothing
			End If
			
			'Define the 3 scenarios to process
			Dim scenarios As String() = {"StartWeek", "EOM", "EOW"}
			
			'Define paths
			Dim basePath As String = "Documents/Public/Treasury/Extensible Document"
			Dim sourcePath As String = basePath & "/Comments/" & paramYear & "/" & paramWeek
			Dim targetPath As String = basePath & "/Export"
			
			'Ensure target folder exists
			BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, basePath, "Export")
			
			'Process each scenario independently
			For Each scenario As String In scenarios
				Me.ProcessScenarioSnapshot(si, scenario, paramWeek, paramYear, sourcePath, targetPath)
			Next
			
			Return Nothing
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' SnapshotCommentPastWeek: Copies the PREVIOUS week's comment DOCX files to fixed locations for PPT XFDoc
		' Uses paramWeek - 1 to get the previous week's comments
		' Processes 3 independent scenarios: StartWeek, EOM, EOW
		'
		' Parameters:
		'   paramWeek - The current week number (will use paramWeek - 1)
		'   paramYear - The year (e.g., "2025")
		'
		' Data Management Step:
		'   p_function=SnapshotCommentPastWeek, paramWeek=[|!p_week!|], paramYear=[|!p_year!|]
		'------------------------------------------------------------------------------------------------------------
		Private Function SnapshotCommentPastWeek(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As Object
			'Get parameters
			Dim paramWeek As String = args.NameValuePairs.XFGetValue("paramWeek", String.Empty)
			Dim paramYear As String = args.NameValuePairs.XFGetValue("paramYear", String.Empty)
			
			'Validate parameters
			If String.IsNullOrEmpty(paramWeek) OrElse String.IsNullOrEmpty(paramYear) Then
				BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: Missing required parameters (paramWeek, paramYear)")
				Return Nothing
			End If
			
			'Calculate target week (paramWeek - 1)
			Dim validWeek As Integer
			If Not Integer.TryParse(paramWeek, validWeek) Then
				validWeek = 1
			End If
			
			Dim targetWeek As Integer = If(validWeek > 1, validWeek - 1, 1)
			Dim targetWeekStr As String = targetWeek.ToString()
			
			'Define the 3 scenarios to process
			Dim scenarios As String() = {"StartWeek", "EOM", "EOW"}
			
			'Define paths
			Dim basePath As String = "Documents/Public/Treasury/Extensible Document"
			Dim sourcePath As String = basePath & "/Comments/" & paramYear & "/" & targetWeekStr
			Dim targetPath As String = basePath & "/Export"
			
			'Ensure target folder exists
			BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, basePath, "Export")
			
			'Process each scenario independently (with _PastWeek suffix)
			For Each scenario As String In scenarios
				Me.ProcessScenarioSnapshotPastWeek(si, scenario, targetWeekStr, paramYear, sourcePath, targetPath)
			Next
			
			BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: PastWeek snapshot completed for week " & targetWeekStr & "/" & paramYear)
			
			Return Nothing
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' GenerateReportWeekInfo: Creates Excel files with week information for PPT XFDoc
		' Creates two files:
		'   - ReportWeekInfo.xlsx: Contains "Week {paramWeek} {Year}" in cell A1
		'   - ReportWeekDate.xlsx: Contains the week start date (e.g., "29/01/2025") in cell A1
		'
		' Parameters:
		'   paramWeek - The week number (e.g., "49")
		'   paramYear - The year (e.g., "2025")
		'
		' Data Management Step:
		'   p_function=GenerateReportWeekInfo, paramWeek=[|!p_week!|], paramYear=[|!p_year!|]
		'
		' Usage in PPT XFDoc:
		'   {XF}{Application}{ExcelFile}{Documents/Public/Treasury/Extensible Document/Export/ReportWeekInfo.xlsx}
		'   {XF}{Application}{ExcelFile}{Documents/Public/Treasury/Extensible Document/Export/ReportWeekDate.xlsx}
		'------------------------------------------------------------------------------------------------------------
		Private Function GenerateReportWeekInfo(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As Object
			'Get parameters
			Dim paramWeek As String = args.NameValuePairs.XFGetValue("paramWeek", String.Empty)
			Dim paramYear As String = args.NameValuePairs.XFGetValue("paramYear", String.Empty)
			
			'Validate parameters
			If String.IsNullOrEmpty(paramWeek) OrElse String.IsNullOrEmpty(paramYear) Then
				BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: GenerateReportWeekInfo - Missing required parameters (paramWeek, paramYear)")
				Return Nothing
			End If
			
			'Define paths
			Dim basePath As String = "Documents/Public/Treasury/Extensible Document"
			Dim targetPath As String = basePath & "/Export"
			
			'Ensure target folder exists
			BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, basePath, "Export")
			
			'------------------------------------------------------------------------------------------------------------
			' FILE 1: ReportWeekInfo.xlsx - "TREASURY WEEKLY MONITORING Week {paramWeek} {Year}"
			'------------------------------------------------------------------------------------------------------------
			Dim weekInfoText As String = "TREASURY WEEKLY MONITORING Week " & paramWeek & " " & paramYear
			Dim weekInfoBytes As Byte() = GenerateSimpleExcel(weekInfoText, "ReportWeekInfo")
			
			BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: GenerateReportWeekInfo - weekInfoText = " & weekInfoText)
			
			If weekInfoBytes IsNot Nothing AndAlso weekInfoBytes.Length > 0 Then
				Dim weekInfoFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, "ReportWeekInfo.xlsx", targetPath)
				weekInfoFileInfo.ContentFileExtension = "xlsx"
				weekInfoFileInfo.Description = "Report Week Info - Week " & paramWeek & "/" & paramYear & " - Updated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
				weekInfoFileInfo.ContentFileContainsData = True
				weekInfoFileInfo.XFFileType = True
				
				Dim weekInfoFile As XFFile = New XFFile(weekInfoFileInfo, String.Empty, weekInfoBytes)
				BRApi.FileSystem.InsertOrUpdateFile(si, weekInfoFile)
			End If
			
			'------------------------------------------------------------------------------------------------------------
			' FILE 2: ReportWeekDate.xlsx - Week start date (e.g., "29/01/2025")
			' Creates two cells: A1 (white text), A2 (black text) for different PPT backgrounds
			'------------------------------------------------------------------------------------------------------------
			Dim weekStartDate As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayDate(si, paramYear, paramWeek)
			Dim weekDateText As String = New String(" "c, 15) & weekStartDate.ToString("dd/MM/yyyy")
			Dim weekDateBytes As Byte() = GenerateDateExcelWithColors(weekDateText)
			
			If weekDateBytes IsNot Nothing AndAlso weekDateBytes.Length > 0 Then
				Dim weekDateFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, "ReportWeekDate.xlsx", targetPath)
				weekDateFileInfo.ContentFileExtension = "xlsx"
				weekDateFileInfo.Description = "Report Week Date - Week " & paramWeek & "/" & paramYear & " - Updated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
				weekDateFileInfo.ContentFileContainsData = True
				weekDateFileInfo.XFFileType = True
				
				Dim weekDateFile As XFFile = New XFFile(weekDateFileInfo, String.Empty, weekDateBytes)
				BRApi.FileSystem.InsertOrUpdateFile(si, weekDateFile)
			End If
			
			BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: GenerateReportWeekInfo completed for week " & paramWeek & "/" & paramYear)
			
			Return Nothing
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' GenerateSimpleExcel: Creates a simple Excel file with a single cell containing text
		' The cell is formatted with Arial 14pt, centered
		'------------------------------------------------------------------------------------------------------------
		Private Function GenerateSimpleExcel(ByVal cellText As String, ByVal namedRangeName As String) As Byte()
			Try
				Using memStream As New MemoryStream()
					Using spreadsheet As SpreadsheetDocument = SpreadsheetDocument.Create(memStream, SpreadsheetDocumentType.Workbook)
						' Create workbook
						Dim workbookPart As WorkbookPart = spreadsheet.AddWorkbookPart()
						workbookPart.Workbook = New Workbook()
						
						' Create stylesheet
						Dim stylesPart As WorkbookStylesPart = workbookPart.AddNewPart(Of WorkbookStylesPart)()
						stylesPart.Stylesheet = CreateSimpleStylesheet()
						stylesPart.Stylesheet.Save()
						
						' Create worksheet
						Dim worksheetPart As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
						Dim sheetData As New SheetData()
						
						' Create columns with specific width
						Dim columns As New DocumentFormat.OpenXml.Spreadsheet.Columns()
						columns.Append(New DocumentFormat.OpenXml.Spreadsheet.Column() With {
							.Min = 1,
							.Max = 1,
							.Width = 60,
							.CustomWidth = True
						})
						
						' Create worksheet with columns
						worksheetPart.Worksheet = New Worksheet()
						worksheetPart.Worksheet.Append(columns)
						worksheetPart.Worksheet.Append(sheetData)
						
						' Add single row with the text
						Dim row As New Row() With {.RowIndex = 1}
						row.Height = 20
						row.CustomHeight = True
						
						Dim cell As New Cell() With {
							.CellReference = "A1",
							.DataType = CellValues.String,
							.CellValue = New CellValue(cellText),
							.StyleIndex = 0
						}
						
						row.Append(cell)
						sheetData.Append(row)
						
						' Create sheets collection
						Dim sheets As New Sheets()
						Dim sheet As New Sheet() With {
							.Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
							.SheetId = 1,
							.Name = "Data"
						}
						sheets.Append(sheet)
						workbookPart.Workbook.Append(sheets)
						
						' Create Named Range for the cell
						Dim definedNames As New DefinedNames()
						Dim definedName As New DefinedName() With {
							.Name = namedRangeName,
							.Text = "Data!$A$1"
						}
						definedNames.Append(definedName)
						workbookPart.Workbook.Append(definedNames)
						
						workbookPart.Workbook.Save()
					End Using
					
					Return memStream.ToArray()
				End Using
			Catch ex As Exception
				Return Nothing
			End Try
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' CreateSimpleStylesheet: Creates a simple stylesheet with Arial 14pt font
		'------------------------------------------------------------------------------------------------------------
		Private Function CreateSimpleStylesheet() As Stylesheet
			Dim stylesheet As New Stylesheet()
			
			' Create font - Arial 14pt, black
			Dim fonts As New DocumentFormat.OpenXml.Spreadsheet.Fonts()
			Dim font As New DocumentFormat.OpenXml.Spreadsheet.Font()
			font.Append(New DocumentFormat.OpenXml.Spreadsheet.FontSize() With {.Val = 14})
			font.Append(New DocumentFormat.OpenXml.Spreadsheet.FontName() With {.Val = "Arial"})
			font.Append(New DocumentFormat.OpenXml.Spreadsheet.Color() With {.Rgb = "FF000000"})
			fonts.Append(font)
			fonts.Count = 1
			
			' Fills
			Dim fills As New Fills(
				New Fill(New PatternFill() With {.PatternType = PatternValues.None}),
				New Fill(New PatternFill() With {.PatternType = PatternValues.Gray125})
			)
			
			' Borders
			Dim borders As New DocumentFormat.OpenXml.Spreadsheet.Borders(New DocumentFormat.OpenXml.Spreadsheet.Border())
			
			' Cell format
			Dim cellFormats As New CellFormats()
			Dim cf As New CellFormat() With {
				.FontId = 0,
				.FillId = 0,
				.BorderId = 0,
				.ApplyFont = True,
				.ApplyAlignment = True
			}
			cf.Alignment = New Alignment() With {
				.Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center,
				.Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Left
			}
			cellFormats.Append(cf)
			cellFormats.Count = 1
			
			stylesheet.Append(fonts)
			stylesheet.Append(fills)
			stylesheet.Append(borders)
			stylesheet.Append(cellFormats)
			
			Return stylesheet
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' GenerateDateExcelWithColors: Creates an Excel file with date in two cells (white and black text)
		' A1 = White text (for dark backgrounds), A2 = Black text (for light backgrounds)
		' Named ranges: ReportWeekDateWhite (A1), ReportWeekDateBlack (A2)
		'------------------------------------------------------------------------------------------------------------
		Private Function GenerateDateExcelWithColors(ByVal cellText As String) As Byte()
			Try
				Using memStream As New MemoryStream()
					Using spreadsheet As SpreadsheetDocument = SpreadsheetDocument.Create(memStream, SpreadsheetDocumentType.Workbook)
						' Create workbook
						Dim workbookPart As WorkbookPart = spreadsheet.AddWorkbookPart()
						workbookPart.Workbook = New Workbook()
						
						' Create stylesheet with white and black fonts
						Dim stylesPart As WorkbookStylesPart = workbookPart.AddNewPart(Of WorkbookStylesPart)()
						stylesPart.Stylesheet = CreateDateStylesheet()
						stylesPart.Stylesheet.Save()
						
						' Create worksheet
						Dim worksheetPart As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
						Dim sheetData As New SheetData()
						
						' Create columns with specific width
						Dim columns As New DocumentFormat.OpenXml.Spreadsheet.Columns()
						columns.Append(New DocumentFormat.OpenXml.Spreadsheet.Column() With {
							.Min = 1,
							.Max = 1,
							.Width = 20,
							.CustomWidth = True
						})
						
						' Create worksheet with columns
						worksheetPart.Worksheet = New Worksheet()
						worksheetPart.Worksheet.Append(columns)
						worksheetPart.Worksheet.Append(sheetData)
						
						' Row 1: White text (StyleIndex = 0)
						Dim row1 As New Row() With {.RowIndex = 1}
						row1.Height = 20
						row1.CustomHeight = True
						Dim cellWhite As New Cell() With {
							.CellReference = "A1",
							.DataType = CellValues.String,
							.CellValue = New CellValue(cellText),
							.StyleIndex = 0
						}
						row1.Append(cellWhite)
						sheetData.Append(row1)
						
						' Row 2: Black text (StyleIndex = 1)
						Dim row2 As New Row() With {.RowIndex = 2}
						row2.Height = 20
						row2.CustomHeight = True
						Dim cellBlack As New Cell() With {
							.CellReference = "A2",
							.DataType = CellValues.String,
							.CellValue = New CellValue(cellText),
							.StyleIndex = 1
						}
						row2.Append(cellBlack)
						sheetData.Append(row2)
						
						' Create sheets collection
						Dim sheets As New Sheets()
						Dim sheet As New Sheet() With {
							.Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
							.SheetId = 1,
							.Name = "Data"
						}
						sheets.Append(sheet)
						workbookPart.Workbook.Append(sheets)
						
						' Create Named Ranges for both cells
						Dim definedNames As New DefinedNames()
						
						' White text named range
						Dim definedNameWhite As New DefinedName() With {
							.Name = "ReportWeekDateWhite",
							.Text = "Data!$A$1"
						}
						definedNames.Append(definedNameWhite)
						
						' Black text named range
						Dim definedNameBlack As New DefinedName() With {
							.Name = "ReportWeekDateBlack",
							.Text = "Data!$A$2"
						}
						definedNames.Append(definedNameBlack)
						
						workbookPart.Workbook.Append(definedNames)
						
						workbookPart.Workbook.Save()
					End Using
					
					Return memStream.ToArray()
				End Using
			Catch ex As Exception
				Return Nothing
			End Try
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' CreateDateStylesheet: Creates a stylesheet with white text on black background (index 0) and black text (index 1)
		'------------------------------------------------------------------------------------------------------------
		Private Function CreateDateStylesheet() As Stylesheet
			Dim stylesheet As New Stylesheet()
			
			' Create fonts
			Dim fonts As New DocumentFormat.OpenXml.Spreadsheet.Fonts()
			
			' Font 0: White text
			Dim fontWhite As New DocumentFormat.OpenXml.Spreadsheet.Font()
			fontWhite.Append(New DocumentFormat.OpenXml.Spreadsheet.FontSize() With {.Val = 14})
			fontWhite.Append(New DocumentFormat.OpenXml.Spreadsheet.FontName() With {.Val = "Arial"})
			fontWhite.Append(New DocumentFormat.OpenXml.Spreadsheet.Color() With {.Rgb = "FFFFFFFF"})
			fonts.Append(fontWhite)
			
			' Font 1: Black text
			Dim fontBlack As New DocumentFormat.OpenXml.Spreadsheet.Font()
			fontBlack.Append(New DocumentFormat.OpenXml.Spreadsheet.FontSize() With {.Val = 14})
			fontBlack.Append(New DocumentFormat.OpenXml.Spreadsheet.FontName() With {.Val = "Arial"})
			fontBlack.Append(New DocumentFormat.OpenXml.Spreadsheet.Color() With {.Rgb = "FF000000"})
			fonts.Append(fontBlack)
			
			fonts.Count = 2
			
			' Fills
			Dim fills As New Fills()
			' Fill 0: None (required)
			fills.Append(New Fill(New PatternFill() With {.PatternType = PatternValues.None}))
			' Fill 1: Gray125 (required)
			fills.Append(New Fill(New PatternFill() With {.PatternType = PatternValues.Gray125}))
			' Fill 2: Black background
			Dim blackFill As New Fill()
			Dim blackPattern As New PatternFill() With {.PatternType = PatternValues.Solid}
			blackPattern.ForegroundColor = New ForegroundColor() With {.Rgb = "FF000000"}
			blackPattern.BackgroundColor = New BackgroundColor() With {.Indexed = 64}
			blackFill.PatternFill = blackPattern
			fills.Append(blackFill)
			fills.Count = 3
			
			' Borders
			Dim borders As New DocumentFormat.OpenXml.Spreadsheet.Borders(New DocumentFormat.OpenXml.Spreadsheet.Border())
			
			' Cell formats
			Dim cellFormats As New CellFormats()
			
			' CellFormat 0: White font on black background
			Dim cfWhite As New CellFormat() With {
				.FontId = 0,
				.FillId = 2,
				.BorderId = 0,
				.ApplyFont = True,
				.ApplyFill = True,
				.ApplyAlignment = True
			}
			cfWhite.Alignment = New Alignment() With {
				.Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center,
				.Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Left
			}
			cellFormats.Append(cfWhite)
			
			' CellFormat 1: Black font (no background)
			Dim cfBlack As New CellFormat() With {
				.FontId = 1,
				.FillId = 0,
				.BorderId = 0,
				.ApplyFont = True,
				.ApplyAlignment = True
			}
			cfBlack.Alignment = New Alignment() With {
				.Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center,
				.Horizontal = DocumentFormat.OpenXml.Spreadsheet.HorizontalAlignmentValues.Left
			}
			cellFormats.Append(cfBlack)
			
			cellFormats.Count = 2
			
			stylesheet.Append(fonts)
			stylesheet.Append(fills)
			stylesheet.Append(borders)
			stylesheet.Append(cellFormats)
			
			Return stylesheet
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' ProcessScenarioSnapshotPastWeek: Processes a single scenario snapshot for PAST WEEK
		' Creates files with _PastWeek suffix to differentiate from current week
		'------------------------------------------------------------------------------------------------------------
		Private Sub ProcessScenarioSnapshotPastWeek(ByVal si As SessionInfo, ByVal scenario As String, ByVal paramWeek As String, _
			ByVal paramYear As String, ByVal sourcePath As String, ByVal targetPath As String)
			
			'Build source file path
			Dim sourceFileName As String = scenario & "_" & paramWeek & "_" & paramYear & ".docx"
			Dim sourceFullPath As String = sourcePath & "/" & sourceFileName
			
			'Check if source file exists for this scenario
			Dim sourceExists As Boolean = BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, sourceFullPath)
			
			If Not sourceExists Then
				'File doesn't exist for this scenario - skip silently (this is expected)
				BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: No comment file for scenario " & scenario & " PastWeek - skipping")
				Return
			End If
			
			'Get the source file
			Dim sourceFile As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, sourceFullPath, True, True)
			Dim fileBytes As Byte() = sourceFile.XFFile.ContentFileBytes
			
			'Define target file names with scenario suffix AND _PastWeek suffix
			Dim targetDocxFileName As String = "ActiveComment_" & scenario & "_PastWeek.docx"
			Dim targetXlsxFileName As String = "ActiveComment_" & scenario & "_PastWeek.xlsx"
			
			'------------------------------------------------------------------------------------------------------------
			' STEP 1: Save the DOCX snapshot
			'------------------------------------------------------------------------------------------------------------
			Dim docxFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetDocxFileName, targetPath)
			docxFileInfo.ContentFileExtension = "docx"
			docxFileInfo.Description = "Active Comment Snapshot PastWeek - " & scenario & " Week " & paramWeek & "/" & paramYear & " - Updated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
			docxFileInfo.ContentFileContainsData = True
			docxFileInfo.XFFileType = True
			
			Dim targetDocxFile As XFFile = New XFFile(docxFileInfo, String.Empty, fileBytes)
			BRApi.FileSystem.InsertOrUpdateFile(si, targetDocxFile)
			
			'------------------------------------------------------------------------------------------------------------
			' STEP 2: Extract formatted text from DOCX and save as Excel for PPT
			'------------------------------------------------------------------------------------------------------------
			Dim formattedLines As List(Of FormattedLine) = ExtractFormattedTextFromDocx(fileBytes)
			
			If formattedLines IsNot Nothing AndAlso formattedLines.Count > 0 Then
				'Generate Excel with the formatted comment text (with _PastWeek suffix in Named Range)
				Dim xlsxBytes As Byte() = GenerateCommentExcel(formattedLines, scenario & "_PastWeek")
				
				If xlsxBytes IsNot Nothing AndAlso xlsxBytes.Length > 0 Then
					Dim xlsxFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetXlsxFileName, targetPath)
					xlsxFileInfo.ContentFileExtension = "xlsx"
					xlsxFileInfo.Description = "Active Comment Excel PastWeek - " & scenario & " Week " & paramWeek & "/" & paramYear & " - Updated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
					xlsxFileInfo.ContentFileContainsData = True
					xlsxFileInfo.XFFileType = True
					
					Dim xlsxFile As XFFile = New XFFile(xlsxFileInfo, String.Empty, xlsxBytes)
					BRApi.FileSystem.InsertOrUpdateFile(si, xlsxFile)
				End If
			End If
			
			BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: Successfully created PastWeek snapshot for " & scenario & " at " & targetPath)
		End Sub
		
		'------------------------------------------------------------------------------------------------------------
		' ProcessScenarioSnapshot: Processes a single scenario snapshot
		'------------------------------------------------------------------------------------------------------------
		Private Sub ProcessScenarioSnapshot(ByVal si As SessionInfo, ByVal scenario As String, ByVal paramWeek As String, _
			ByVal paramYear As String, ByVal sourcePath As String, ByVal targetPath As String)
			
			'Build source file path
			Dim sourceFileName As String = scenario & "_" & paramWeek & "_" & paramYear & ".docx"
			Dim sourceFullPath As String = sourcePath & "/" & sourceFileName
			
			'Check if source file exists for this scenario
			Dim sourceExists As Boolean = BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, sourceFullPath)
			
			If Not sourceExists Then
				'File doesn't exist for this scenario - skip silently (this is expected)
				BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: No comment file for scenario " & scenario & " - skipping")
				Return
			End If
			
			'Get the source file
			Dim sourceFile As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, sourceFullPath, True, True)
			Dim fileBytes As Byte() = sourceFile.XFFile.ContentFileBytes
			
			'Define target file names with scenario suffix
			Dim targetDocxFileName As String = "ActiveComment_" & scenario & ".docx"
			Dim targetXlsxFileName As String = "ActiveComment_" & scenario & ".xlsx"
			
			'------------------------------------------------------------------------------------------------------------
			' STEP 1: Save the DOCX snapshot
			'------------------------------------------------------------------------------------------------------------
			Dim docxFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetDocxFileName, targetPath)
			docxFileInfo.ContentFileExtension = "docx"
			docxFileInfo.Description = "Active Comment Snapshot - " & scenario & " Week " & paramWeek & "/" & paramYear & " - Updated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
			docxFileInfo.ContentFileContainsData = True
			docxFileInfo.XFFileType = True
			
			Dim targetDocxFile As XFFile = New XFFile(docxFileInfo, String.Empty, fileBytes)
			BRApi.FileSystem.InsertOrUpdateFile(si, targetDocxFile)
			
			'------------------------------------------------------------------------------------------------------------
			' STEP 2: Extract formatted text from DOCX and save as Excel for PPT
			'------------------------------------------------------------------------------------------------------------
			Dim formattedLines As List(Of FormattedLine) = ExtractFormattedTextFromDocx(fileBytes)
			
			If formattedLines IsNot Nothing AndAlso formattedLines.Count > 0 Then
				'Generate Excel with the formatted comment text
				Dim xlsxBytes As Byte() = GenerateCommentExcel(formattedLines, scenario)
				
				If xlsxBytes IsNot Nothing AndAlso xlsxBytes.Length > 0 Then
					Dim xlsxFileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetXlsxFileName, targetPath)
					xlsxFileInfo.ContentFileExtension = "xlsx"
					xlsxFileInfo.Description = "Active Comment Excel - " & scenario & " Week " & paramWeek & "/" & paramYear & " - Updated: " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
					xlsxFileInfo.ContentFileContainsData = True
					xlsxFileInfo.XFFileType = True
					
					Dim xlsxFile As XFFile = New XFFile(xlsxFileInfo, String.Empty, xlsxBytes)
					BRApi.FileSystem.InsertOrUpdateFile(si, xlsxFile)
				End If
			End If
			
			BRApi.ErrorLog.LogMessage(si, "TRS_ReportGenerator_Comment: Successfully created snapshot for " & scenario & " at " & targetPath)
		End Sub
		
		'------------------------------------------------------------------------------------------------------------
		' FormattedLine: Holds text and formatting information for a paragraph
		'------------------------------------------------------------------------------------------------------------
		Private Class FormattedLine
			Public Property Text As String
			Public Property ColorHex As String  ' Color in hex format (e.g., "FF0000" for red)
			Public Property IsBold As Boolean
			Public Property IsUnderline As Boolean
			Public Property IsBullet As Boolean
			
			Public Sub New()
				Text = String.Empty
				ColorHex = "000000"  ' Default black
				IsBold = False
				IsUnderline = False
				IsBullet = False
			End Sub
		End Class
		
		'------------------------------------------------------------------------------------------------------------
		' ExtractFormattedTextFromDocx: Extracts text with formatting information from DOCX
		' Captures: Color, Bold, Underline, and Bullet points
		'------------------------------------------------------------------------------------------------------------
		Private Function ExtractFormattedTextFromDocx(ByVal docxBytes As Byte()) As List(Of FormattedLine)
			Dim lines As New List(Of FormattedLine)()
			
			Try
				Using memStream As New MemoryStream(docxBytes)
					Using doc As WordprocessingDocument = WordprocessingDocument.Open(memStream, False)
						Dim body As Body = doc.MainDocumentPart.Document.Body
						
						For Each para As Paragraph In body.Elements(Of Paragraph)()
							Dim line As New FormattedLine()
							Dim textBuilder As New StringBuilder()
							Dim firstRunProcessed As Boolean = False
							
							' Get paragraph properties
							Dim paraProps = para.ParagraphProperties
							
							' Check if paragraph has numbering/bullets
							If paraProps IsNot Nothing Then
								Dim numProps = paraProps.NumberingProperties
								If numProps IsNot Nothing Then
									' Paragraph has bullet/numbering
									line.IsBullet = True
								End If
							End If
							
							For Each run As DocumentFormat.OpenXml.Wordprocessing.Run In para.Elements(Of DocumentFormat.OpenXml.Wordprocessing.Run)()
								' Get run properties for formatting
								Dim runProps = run.RunProperties
								
								If runProps IsNot Nothing AndAlso Not firstRunProcessed Then
									' Get color from run
									Dim colorElement = runProps.Color
									If colorElement IsNot Nothing AndAlso colorElement.Val IsNot Nothing Then
										line.ColorHex = colorElement.Val.Value
									End If
									
									' Check for bold
									If runProps.Bold IsNot Nothing Then
										line.IsBold = True
									End If
									
									' Check for underline
									Dim underlineElement = runProps.Underline
									If underlineElement IsNot Nothing AndAlso underlineElement.Val IsNot Nothing Then
										' Check if it's not "none"
										If underlineElement.Val.Value <> DocumentFormat.OpenXml.Wordprocessing.UnderlineValues.None Then
											line.IsUnderline = True
										End If
									End If
									
									firstRunProcessed = True
								End If
								
								' Also check subsequent runs for underline (in case first run has no formatting)
								If runProps IsNot Nothing AndAlso Not line.IsUnderline Then
									Dim underlineElement = runProps.Underline
									If underlineElement IsNot Nothing AndAlso underlineElement.Val IsNot Nothing Then
										If underlineElement.Val.Value <> DocumentFormat.OpenXml.Wordprocessing.UnderlineValues.None Then
											line.IsUnderline = True
										End If
									End If
								End If
								
								' Extract text
								For Each child In run.ChildElements
									If TypeOf child Is DocumentFormat.OpenXml.Wordprocessing.Text Then
										textBuilder.Append(DirectCast(child, DocumentFormat.OpenXml.Wordprocessing.Text).Text)
									ElseIf TypeOf child Is DocumentFormat.OpenXml.Wordprocessing.TabChar Then
										textBuilder.Append(vbTab)
									End If
								Next
							Next
							
							' Build final text with bullet prefix if needed
							Dim finalText As String = textBuilder.ToString()
							If line.IsBullet AndAlso Not String.IsNullOrWhiteSpace(finalText) Then
								line.Text = "• " & finalText
							Else
								line.Text = finalText
							End If
							
							' Only add non-empty lines
							If Not String.IsNullOrWhiteSpace(line.Text) Then
								lines.Add(line)
							Else
								' Add empty line to preserve spacing
								lines.Add(New FormattedLine() With {.Text = " ", .ColorHex = "000000"})
							End If
						Next
					End Using
				End Using
			Catch ex As Exception
				' Return empty list on error
			End Try
			
			Return lines
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' StyleKey: Represents a unique combination of formatting options
		'------------------------------------------------------------------------------------------------------------
		Private Class StyleKey
			Public Property ColorHex As String
			Public Property IsUnderline As Boolean
			Public Property IsBold As Boolean
			
			Public Sub New(ByVal color As String, ByVal underline As Boolean, ByVal bold As Boolean)
				ColorHex = If(String.IsNullOrEmpty(color), "000000", color.ToUpper())
				IsUnderline = underline
				IsBold = bold
			End Sub
			
			Public Overrides Function GetHashCode() As Integer
				Return (ColorHex & "_" & IsUnderline.ToString() & "_" & IsBold.ToString()).GetHashCode()
			End Function
			
			Public Overrides Function Equals(obj As Object) As Boolean
				If obj Is Nothing OrElse Not TypeOf obj Is StyleKey Then Return False
				Dim other As StyleKey = DirectCast(obj, StyleKey)
				Return Me.ColorHex = other.ColorHex AndAlso Me.IsUnderline = other.IsUnderline AndAlso Me.IsBold = other.IsBold
			End Function
			
			Public Function ToKey() As String
				Return ColorHex & "_" & IsUnderline.ToString() & "_" & IsBold.ToString()
			End Function
		End Class
		
		'------------------------------------------------------------------------------------------------------------
		' GenerateCommentExcel: Creates an Excel file with formatted text (colors, underline, bold)
		'------------------------------------------------------------------------------------------------------------
		Private Function GenerateCommentExcel(ByVal formattedLines As List(Of FormattedLine), ByVal scenario As String) As Byte()
			Try
				Using memStream As New MemoryStream()
					Using spreadsheet As SpreadsheetDocument = SpreadsheetDocument.Create(memStream, SpreadsheetDocumentType.Workbook)
						' Create workbook
						Dim workbookPart As WorkbookPart = spreadsheet.AddWorkbookPart()
						workbookPart.Workbook = New Workbook()
						
						' Collect unique style combinations (color + underline + bold)
						Dim styleKeys As New List(Of StyleKey)()
						Dim styleKeyMap As New Dictionary(Of String, Integer)()
						
						' Always add default black style first
						Dim defaultKey As New StyleKey("000000", False, False)
						styleKeys.Add(defaultKey)
						styleKeyMap.Add(defaultKey.ToKey(), 0)
						
						' Collect unique style combinations from all lines
						For Each line In formattedLines
							Dim key As New StyleKey(line.ColorHex, line.IsUnderline, line.IsBold)
							If Not styleKeyMap.ContainsKey(key.ToKey()) Then
								styleKeyMap.Add(key.ToKey(), styleKeys.Count)
								styleKeys.Add(key)
							End If
						Next
						
						' Create stylesheet with dynamic fonts for each style combination
						Dim stylesPart As WorkbookStylesPart = workbookPart.AddNewPart(Of WorkbookStylesPart)()
						stylesPart.Stylesheet = CreateStylesheetWithFormats(styleKeys)
						stylesPart.Stylesheet.Save()
						
						' Create worksheet
						Dim worksheetPart As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
						Dim sheetData As New SheetData()
						
						' Create columns with specific width
						Dim columns As New DocumentFormat.OpenXml.Spreadsheet.Columns()
						columns.Append(New DocumentFormat.OpenXml.Spreadsheet.Column() With {
							.Min = 1,
							.Max = 1,
							.Width = 80,
							.CustomWidth = True
						})
						
						' Create worksheet with columns
						worksheetPart.Worksheet = New Worksheet()
						worksheetPart.Worksheet.Append(columns)
						worksheetPart.Worksheet.Append(sheetData)
						
						' Add each line as a row
						Dim rowIndex As UInteger = 1
						For Each line In formattedLines
							Dim row As New Row() With {.RowIndex = rowIndex}
							row.Height = 18
							row.CustomHeight = True
							
							' Find the style index for this formatting combination
							Dim key As New StyleKey(line.ColorHex, line.IsUnderline, line.IsBold)
							Dim styleIndex As UInteger = 0
							If styleKeyMap.ContainsKey(key.ToKey()) Then
								styleIndex = CUInt(styleKeyMap(key.ToKey()))
							End If
							
							Dim cell As New Cell() With {
								.CellReference = "A" & rowIndex.ToString(),
								.DataType = CellValues.String,
								.CellValue = New CellValue(line.Text),
								.StyleIndex = styleIndex
							}
							
							row.Append(cell)
							sheetData.Append(row)
							rowIndex += 1UI
						Next
						
						' Create sheets collection
						Dim sheets As New Sheets()
						Dim sheet As New Sheet() With {
							.Id = spreadsheet.WorkbookPart.GetIdOfPart(worksheetPart),
							.SheetId = 1,
							.Name = "Comment"
						}
						sheets.Append(sheet)
						workbookPart.Workbook.Append(sheets)
						
						' Create Named Range for all content (with scenario suffix)
						Dim definedNames As New DefinedNames()
						Dim definedName As New DefinedName() With {
							.Name = "CommentText_" & scenario,
							.Text = "Comment!$A$1:$A$" & (rowIndex - 1UI).ToString()
						}
						definedNames.Append(definedName)
						workbookPart.Workbook.Append(definedNames)
						
						workbookPart.Workbook.Save()
					End Using
					
					Return memStream.ToArray()
				End Using
			Catch ex As Exception
				Return Nothing
			End Try
		End Function
		
		'------------------------------------------------------------------------------------------------------------
		' CreateStylesheetWithFormats: Creates a stylesheet with fonts for each unique style combination
		' Supports: Color, Underline, Bold
		'------------------------------------------------------------------------------------------------------------
		Private Function CreateStylesheetWithFormats(ByVal styleKeys As List(Of StyleKey)) As Stylesheet
			Dim stylesheet As New Stylesheet()
			
			' Create fonts - one for each style combination
			Dim fonts As New DocumentFormat.OpenXml.Spreadsheet.Fonts()
			
			For Each styleKey In styleKeys
				Dim font As New DocumentFormat.OpenXml.Spreadsheet.Font()
				font.Append(New DocumentFormat.OpenXml.Spreadsheet.FontSize() With {.Val = 14})
				font.Append(New DocumentFormat.OpenXml.Spreadsheet.FontName() With {.Val = "Arial"})
				font.Append(New DocumentFormat.OpenXml.Spreadsheet.Color() With {.Rgb = "FF" & styleKey.ColorHex})
				
				' Add bold if needed
				If styleKey.IsBold Then
					font.Append(New DocumentFormat.OpenXml.Spreadsheet.Bold())
				End If
				
				' Add underline if needed
				If styleKey.IsUnderline Then
					font.Append(New DocumentFormat.OpenXml.Spreadsheet.Underline() With {.Val = DocumentFormat.OpenXml.Spreadsheet.UnderlineValues.Single})
				End If
				
				fonts.Append(font)
			Next
			
			fonts.Count = CUInt(styleKeys.Count)
			
			' Fills
			Dim fills As New Fills(
				New Fill(New PatternFill() With {.PatternType = PatternValues.None}),
				New Fill(New PatternFill() With {.PatternType = PatternValues.Gray125})
			)
			
			' Borders
			Dim borders As New DocumentFormat.OpenXml.Spreadsheet.Borders(New DocumentFormat.OpenXml.Spreadsheet.Border())
			
			' Cell formats - one for each style combination
			Dim cellFormats As New CellFormats()
			
			For i As Integer = 0 To styleKeys.Count - 1
				Dim cf As New CellFormat() With {
					.FontId = CUInt(i),
					.FillId = 0,
					.BorderId = 0,
					.ApplyFont = True,
					.ApplyAlignment = True
				}
				cf.Alignment = New Alignment() With {
					.Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Center
				}
				cellFormats.Append(cf)
			Next
			
			cellFormats.Count = CUInt(styleKeys.Count)
			
			stylesheet.Append(fonts)
			stylesheet.Append(fills)
			stylesheet.Append(borders)
			stylesheet.Append(cellFormats)
			
			Return stylesheet
		End Function
		
	End Class
End Namespace
