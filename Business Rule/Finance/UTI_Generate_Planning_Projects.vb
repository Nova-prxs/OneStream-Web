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
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Namespace OneStream.BusinessRule.Extender.UTI_Generate_Planning_Projects
    Public Class MainClass

        Public Function CreateExcel(
            ByVal si As SessionInfo, _
            Optional factory As String = "", Optional year As Integer = 0, Optional month As Integer = 0, _
            Optional scenario As String = "", Optional time As String = "", _
            Optional folderPath As String = "Documents/Public/Templates_Load", Optional fileName As String = "Project_Planning_Template.xlsx", Optional destPath As String = "", _
            Optional sql As String = "", Optional dt As DataTable = Nothing
        )
            folderPath = "Documents/Public/Templates_Load"
            Dim filePath As String = $"{folderPath}/{fileName}"
            If destPath = "" Then destPath = filePath
            Dim modified As Boolean = False

            Dim startRow As Integer = 2
            Dim endRow As Integer = 3

#Region "Templates"
            Dim templateFile As Boolean = False

            If fileName = "Project_Planning_Template.xlsx" Then
                templateFile = True
                startRow = 11
                sql = $"
                    SELECT 
                        entity, 
                        project_id, 
                        project_description, 
                        technology, 
                        start_date, 
                        break_close_date_1, 
                        end_date
                    FROM 
                        XFC_Project_Backlog
                    WHERE 
                        Active = 1
                "
            End If
#End Region

#Region "DataTable"
            If sql <> "" Or templateFile = True Then
                Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    dt = BRApi.Database.ExecuteSql(dbConnApp, sql, True)
                End Using
            End If
#End Region

            endRow = startRow + dt.Rows.Count - 1

#Region "Modify Excel"
            Dim fileExists As Boolean = brapi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath)
            If Not fileExists Then
                brapi.ErrorLog.LogMessage(si, "El archivo no se encontró en la ruta: " & filePath)
                Return Nothing
            End If

            Dim fileBytes As Byte() = brapi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, True).XFFile.ContentFileBytes
            If fileBytes Is Nothing OrElse fileBytes.Length = 0 Then Return Nothing

            Dim memoryStream As New MemoryStream()
            memoryStream.Write(fileBytes, 0, fileBytes.Length)
            memoryStream.Position = 0

            Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Open(memoryStream, True)
                Dim workbookPart As WorkbookPart = spreadsheetDocument.WorkbookPart

                ' Evitar warning creando variable local antes del lambda
                Dim sheetsList = workbookPart.Workbook.Sheets.Elements(Of Sheet)()
                Dim sheet As Sheet = sheetsList.FirstOrDefault(Function(s)
                                                                  Dim localName = s.Name.Value
                                                                  Return localName = "XLSX Import"
                                                              End Function)
                If sheet Is Nothing Then
                    brapi.ErrorLog.LogMessage(si, "No se encontró la hoja: XLSX Import")
                    Return Nothing
                End If

                Dim worksheetPart As WorksheetPart = CType(workbookPart.GetPartById(sheet.Id.Value), WorksheetPart)
                Dim sheetData As SheetData = worksheetPart.Worksheet.Elements(Of SheetData)().FirstOrDefault()

                ' Limpiar celdas existentes
                For Each row As Row In sheetData.Elements(Of Row)()
                    If CInt(row.RowIndex.Value) >= startRow Or templateFile = False Then
                        For Each cell As Cell In row.Elements(Of Cell)()
                            cell.CellValue = Nothing
                            cell.DataType = Nothing
                        Next
                    End If
                Next

                ' Insertar los nuevos datos
                For rowIndex As Integer = startRow To endRow
                    Dim dtRowIndex As Integer = rowIndex - startRow
                    Dim row As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) CInt(r.RowIndex.Value) = rowIndex)

                    If row Is Nothing Then
                        row = New Row() With {.RowIndex = CType(rowIndex, UInt32)}
                        sheetData.Append(row)
                    End If

                    For colIndex As Integer = 1 To dt.Columns.Count
                        Dim valor As String = If(IsDBNull(dt.Rows(dtRowIndex)(colIndex - 1)), String.Empty, dt.Rows(dtRowIndex)(colIndex - 1).ToString())
                        Dim colLetter As String = GetExcelColumnName(colIndex)
                        Dim cellReference As String = colLetter & rowIndex.ToString()

                        Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
                        If cell Is Nothing Then
                            cell = New Cell With {.CellReference = cellReference}
                            If IsNumeric(valor) Then
                                cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString(CultureInfo.InvariantCulture))
                                cell.DataType = New EnumValue(Of CellValues)(CellValues.Number)
                            Else
                                cell.CellValue = New CellValue(valor)
                                cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
                            End If
                            row.Append(cell)
                        Else
                            If IsNumeric(valor) Then
                                cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString(CultureInfo.InvariantCulture))
                                cell.DataType = New EnumValue(Of CellValues)(CellValues.Number)
                            Else
                                cell.CellValue = New CellValue(valor)
                                cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
                            End If
                        End If
                    Next
                Next

                If modified Then
                    worksheetPart.Worksheet.Save()
                    workbookPart.Workbook.Save()
                Else
                    brapi.ErrorLog.LogMessage(si, "No se encontró ninguna celda para modificar.")
                End If
            End Using
#End Region

#Region "Create File"
            Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, destPath)
            Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, memoryStream.ToArray())
            fileFile.FileInfo.ContentFileExtension = "xlsx"
            brapi.FileSystem.InsertOrUpdateFile(si, fileFile)
#End Region

            Return Nothing
        End Function

        Private Function GetExcelColumnName(ByVal columnNumber As Integer) As String
            Dim dividend As Integer = columnNumber
            Dim columnName As String = String.Empty
            Dim modulo As Integer

            While dividend > 0
                modulo = (dividend - 1) Mod 26
                columnName = Chr(65 + modulo) & columnName
                dividend = CInt((dividend - modulo) \ 26)
            End While

            Return columnName
        End Function

    End Class
End Namespace