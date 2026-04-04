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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.TRS_ReportGenerator_TreasuryMonitoring_PastWeek
    Public Class MainClass
        
        ' ==================================================================================
        ' TRS_ReportGenerator_TreasuryMonitoring_PastWeek
        ' ==================================================================================
        ' This business rule generates the Treasury Monitoring Report for the PREVIOUS week
        ' (paramWeek - 1) instead of the current week.
        ' 
        ' NOTE: This is a copy of TRS_ReportGenerator_TreasuryMonitoring that uses
        '       (paramWeek - 1) instead of paramWeek.
        '
        ' OUTPUT: Treasury_Monitoring_Report_PastWeek.xlsx
        ' ==================================================================================
        
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Try
                Select Case args.FunctionType
                    Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
                        Dim functionName As String = String.Empty
                        If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("p_function") Then
                            functionName = args.NameValuePairs("p_function")
                        End If
                        
                        If Not String.IsNullOrEmpty(functionName) Then
                            If functionName.Equals("GenerateMonitoringReportPastWeek", StringComparison.InvariantCultureIgnoreCase) Then
                                Return GenerateMonitoringReportPastWeek(si, args)
                            End If
                        End If
                End Select

                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Private Function GenerateMonitoringReportPastWeek(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As Object
            Try
                ' 1. Get Parameters
                Dim paramWeek As String = String.Empty
                If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("paramWeek") Then
                    paramWeek = args.NameValuePairs("paramWeek")
                End If
                
                Dim paramYear As String = String.Empty
                If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("paramYear") Then
                    paramYear = args.NameValuePairs("paramYear")
                End If
                If String.IsNullOrEmpty(paramWeek) OrElse String.IsNullOrEmpty(paramYear) Then
                    Throw New Exception("Parameters paramWeek and paramYear are required.")
                End If

                ' 2. Calculate Target Week (paramWeek - 1)
                Dim validWeek As Integer
                If Not Integer.TryParse(paramWeek, validWeek) Then
                    validWeek = 1
                End If
                
                ' Calculate the target week (paramWeek - 1)
                Dim targetWeek As Integer = If(validWeek > 1, validWeek - 1, 1)
                Dim targetWeekStr As String = targetWeek.ToString()

                ' 3. Get Data for Previous Week
                Dim dt As DataTable = GetTreasuryData(si, targetWeekStr, paramYear)
                
                If dt.Rows.Count = 0 Then
                End If

                ' 4. Load File (Using the TEMPLATE as source, not the output)
                Dim templatePath As String = "Documents/Public/Treasury/Extensible Document/Treasury_Monitoring_Report.xlsx"
                Dim outputPath As String = "Documents/Public/Treasury/Treasury_Monitoring_Report_PastWeek.xlsx"
                
                Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, templatePath, True, False)
                
                If fileInfo Is Nothing OrElse fileInfo.XFFile.ContentFileBytes Is Nothing Then
                    Throw New Exception($"Template file not found at {templatePath}")
                End If

                Dim fileBytes As Byte() = fileInfo.XFFile.ContentFileBytes

                ' 5. Populate Excel
                Using ms As New MemoryStream()
                    ms.Write(fileBytes, 0, fileBytes.Length)
                    ms.Position = 0

                    Using doc As SpreadsheetDocument = SpreadsheetDocument.Open(ms, isEditable:=True)
                        ' Eliminar Calculation Chain para evitar errores de corrupción al modificar filas
                        If doc.WorkbookPart.CalculationChainPart IsNot Nothing Then
                            doc.WorkbookPart.DeletePart(doc.WorkbookPart.CalculationChainPart)
                        End If

                        Dim workbookPart As WorkbookPart = doc.WorkbookPart
                        Dim sheetName As String = "Sheet1"
                        Dim sheet As Sheet = workbookPart.Workbook.Descendants(Of Sheet)().Where(Function(s) s.Name = sheetName).FirstOrDefault()

                        If sheet Is Nothing Then
                            Throw New Exception($"Sheet '{sheetName}' not found in file.")
                        End If

                        Dim worksheetPart As WorksheetPart = CType(workbookPart.GetPartById(sheet.Id), WorksheetPart)
                        Dim sheetData As SheetData = worksheetPart.Worksheet.Elements(Of SheetData)().First()
                        
                        ' NOTE: Column widths and row heights are preserved from template - no manual overrides needed
                        ' The template file should have the correct formatting already configured
                        
                        ' Identificar fila plantilla (Fila 3 = primera fila de datos en template con título)
                        ' Si el template es "Good" (sin título), sería Fila 2
                        Dim templateRowIndex As UInteger = 3
                        Dim templateRow As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = templateRowIndex)
                        
                        ' Si no existe fila 3, intentar con fila 2 (template sin título)
                        If templateRow Is Nothing Then
                            templateRowIndex = 2
                            templateRow = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = templateRowIndex)
                        End If

                        If templateRow Is Nothing Then
                            Throw New Exception("El archivo debe tener una fila de datos (fila 2 o 3) para ser usada como base.")
                        End If

                        ' CREAR ESTILOS DE COLORES PARA ALERT LEVELS
                        Dim stylesPart As WorkbookStylesPart = doc.WorkbookPart.WorkbookStylesPart
                        Dim greenStyleId As UInteger? = Nothing
                        Dim yellowStyleId As UInteger? = Nothing
                        Dim orangeStyleId As UInteger? = Nothing
                        Dim redStyleId As UInteger? = Nothing
                        
                        Dim greenBgStyleId As UInteger? = Nothing
                        Dim yellowBgStyleId As UInteger? = Nothing
                        Dim orangeBgStyleId As UInteger? = Nothing
                        Dim redBgStyleId As UInteger? = Nothing

                        If stylesPart IsNot Nothing AndAlso stylesPart.Stylesheet IsNot Nothing Then
                            Dim fonts As Fonts = stylesPart.Stylesheet.Fonts
                            Dim fills As Fills = stylesPart.Stylesheet.Fills
                            Dim cellFormats As CellFormats = stylesPart.Stylesheet.CellFormats

                            If fonts IsNot Nothing AndAlso fills IsNot Nothing AndAlso cellFormats IsNot Nothing Then
                                ' Obtener estilo base de columna C para preservar bordes
                                Dim baseBorderId As UInteger = 0
                                Dim baseFillId As UInteger = 0
                                
                                Dim templateCellC As Cell = templateRow.Elements(Of Cell)().FirstOrDefault(Function(c) GetColumnName(c.CellReference.Value) = "C")
                                If templateCellC IsNot Nothing AndAlso templateCellC.StyleIndex IsNot Nothing Then
                                    Dim templateXfIndex As UInteger = templateCellC.StyleIndex.Value
                                    If templateXfIndex < cellFormats.ChildElements.Count Then
                                        Dim templateXf As CellFormat = CType(cellFormats.ChildElements(CInt(templateXfIndex)), CellFormat)
                                        If templateXf.BorderId IsNot Nothing Then baseBorderId = templateXf.BorderId.Value
                                        If templateXf.FillId IsNot Nothing Then baseFillId = templateXf.FillId.Value
                                    End If
                                End If

                                ' Helper para crear estilos de círculos coloreados
                                Dim CreateCircleStyle As Func(Of String, UInteger) = Function(hexColor)
                                    Dim font As New Font()
                                    font.Append(New Color() With {.Rgb = hexColor})
                                    font.Append(New FontSize() With {.Val = 40})
                                    fonts.Append(font)
                                    Dim fontId As UInteger = CUInt(fonts.ChildElements.Count - 1)

                                    Dim xf As New CellFormat() With {
                                        .FontId = fontId,
                                        .FillId = baseFillId,
                                        .BorderId = baseBorderId,
                                        .ApplyFont = True,
                                        .ApplyFill = True,
                                        .ApplyBorder = True,
                                        .Alignment = New Alignment() With {.Horizontal = HorizontalAlignmentValues.Center, .Vertical = VerticalAlignmentValues.Center}
                                    }
                                    cellFormats.Append(xf)
                                    Return CUInt(cellFormats.ChildElements.Count - 1)
                                End Function

                                greenStyleId = CreateCircleStyle("FF9BBB59")  ' 1: Green
                                yellowStyleId = CreateCircleStyle("FFFFC000") ' 2: Yellow
                                orangeStyleId = CreateCircleStyle("FFED7D31") ' 3: Orange
                                redStyleId = CreateCircleStyle("FFFF0000")    ' 4: Red

                                ' Obtener estilo base de columna B para preservar bordes
                                Dim baseFontIdB As UInteger = 0
                                Dim baseBorderIdB As UInteger = 0
                                
                                Dim templateCellB As Cell = templateRow.Elements(Of Cell)().FirstOrDefault(Function(c) GetColumnName(c.CellReference.Value) = "B")
                                If templateCellB IsNot Nothing AndAlso templateCellB.StyleIndex IsNot Nothing Then
                                    Dim templateXfIndex As UInteger = templateCellB.StyleIndex.Value
                                    If templateXfIndex < cellFormats.ChildElements.Count Then
                                        Dim templateXf As CellFormat = CType(cellFormats.ChildElements(CInt(templateXfIndex)), CellFormat)
                                        If templateXf.FontId IsNot Nothing Then baseFontIdB = templateXf.FontId.Value
                                        If templateXf.BorderId IsNot Nothing Then baseBorderIdB = templateXf.BorderId.Value
                                    End If
                                End If

                                ' Helper para crear estilos de fondo coloreado
                                Dim CreateBackgroundStyle As Func(Of String, UInteger) = Function(hexColor)
                                    Dim fill As New Fill()
                                    Dim bgPatternFill As New PatternFill() With {.PatternType = PatternValues.Solid}
                                    bgPatternFill.Append(New ForegroundColor() With {.Rgb = hexColor})
                                    bgPatternFill.Append(New BackgroundColor() With {.Indexed = 64})
                                    fill.Append(bgPatternFill)
                                    fills.Append(fill)
                                    Dim fillId As UInteger = CUInt(fills.ChildElements.Count - 1)
                                    fills.Count = UInt32Value.FromUInt32(CUInt(fills.ChildElements.Count))

                                    Dim xf As New CellFormat() With {
                                        .FontId = baseFontIdB,
                                        .FillId = fillId,
                                        .BorderId = baseBorderIdB,
                                        .ApplyFont = True,
                                        .ApplyFill = True,
                                        .ApplyBorder = True,
                                        .Alignment = New Alignment() With {.Horizontal = HorizontalAlignmentValues.Center, .Vertical = VerticalAlignmentValues.Center, .WrapText = True}
                                    }
                                    cellFormats.Append(xf)
                                    Return CUInt(cellFormats.ChildElements.Count - 1)
                                End Function

                                greenBgStyleId = CreateBackgroundStyle("FF9BBB59")
                                yellowBgStyleId = CreateBackgroundStyle("FFFFC000")
                                orangeBgStyleId = CreateBackgroundStyle("FFED7D31")
                                redBgStyleId = CreateBackgroundStyle("FFFF0000")

                                fonts.Count = UInt32Value.FromUInt32(CUInt(fonts.ChildElements.Count))
                                cellFormats.Count = UInt32Value.FromUInt32(CUInt(cellFormats.ChildElements.Count))
                                
                                stylesPart.Stylesheet.Save()
                            End If
                        End If

                        ' LIMPIAR DATOS ANTIGUOS (Filas > templateRowIndex)
                        Dim rowsToRemove As List(Of Row) = sheetData.Elements(Of Row)().Where(Function(r) r.RowIndex.Value > templateRowIndex).ToList()
                        For Each r As Row In rowsToRemove
                            r.Remove()
                        Next

                        Dim rowIndex As UInteger = templateRowIndex
                        
                        For Each row As DataRow In dt.Rows
                            Dim currentRow As Row
                            
                            If rowIndex = templateRowIndex Then
                                currentRow = templateRow
                                ' Template row already has correct height from file
                            Else
                                ' Clonar fila plantilla para preservar formato (bordes, fuentes, altura)
                                currentRow = CType(templateRow.CloneNode(True), Row)
                                currentRow.RowIndex = New UInt32Value(rowIndex)
                                ' Height is inherited from cloned template row - no manual override needed
                                
                                ' Actualizar referencias de celdas
                                For Each cell As Cell In currentRow.Elements(Of Cell)()
                                    Dim col As String = GetColumnName(cell.CellReference.Value)
                                    cell.CellReference = New StringValue(col & rowIndex.ToString())
                                Next
                                

                                sheetData.Append(currentRow)
                            End If
                            
                            ' Escribir datos (sin tocar estilos ni anchos)
                            SetCellValue(currentRow, "A", If(row("Date") IsNot DBNull.Value, Convert.ToDateTime(row("Date")).ToString("dd/MM/yyyy"), ""), True)
                            SetCellValue(currentRow, "B", If(row("Entity") IsNot DBNull.Value, row("Entity").ToString(), ""), True)
                            
                            ' Column C: Colored Circles for 1, 2, 3, 4
                            Dim alertVal As Object = row("alert_level_status")
                            Dim isCircle As Boolean = False
                            
                            If alertVal IsNot DBNull.Value Then
                                Dim valStr As String = alertVal.ToString()
                                
                                ' 1. Apply Background Color to Column B (Entity)
                                Dim bgStyleId As UInteger? = Nothing
                                Select Case valStr
                                    Case "1" : bgStyleId = greenBgStyleId
                                    Case "2" : bgStyleId = yellowBgStyleId
                                    Case "3" : bgStyleId = orangeBgStyleId
                                    Case "4" : bgStyleId = redBgStyleId
                                End Select
                                
                                If bgStyleId.HasValue Then
                                    Dim cellB As Cell = currentRow.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = "B" & rowIndex.ToString())
                                    If cellB IsNot Nothing Then cellB.StyleIndex = bgStyleId.Value
                                End If

                                ' 2. Apply Circle to Column C
                                Dim circleStyleId As UInteger? = Nothing
                                Select Case valStr
                                    Case "1" : circleStyleId = greenStyleId
                                    Case "2" : circleStyleId = yellowStyleId
                                    Case "3" : circleStyleId = orangeStyleId
                                    Case "4" : circleStyleId = redStyleId
                                End Select
                                
                                If circleStyleId.HasValue Then
                                    SetCellValue(currentRow, "C", "●", False)
                                    Dim cellC As Cell = currentRow.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = "C" & rowIndex.ToString())
                                    If cellC IsNot Nothing Then cellC.StyleIndex = circleStyleId.Value
                                    isCircle = True
                                End If
                            End If

                            If Not isCircle Then
                                If alertVal IsNot DBNull.Value AndAlso IsNumeric(alertVal) Then
                                    SetCellNumberValue(currentRow, "C", Convert.ToDouble(alertVal), True)
                                Else
                                    SetCellValue(currentRow, "C", If(alertVal IsNot DBNull.Value, alertVal.ToString(), ""), True)
                                End If
                            End If

                            SetCellValue(currentRow, "D", If(row("issue") IsNot DBNull.Value, row("issue").ToString(), ""), True)
                            SetCellValue(currentRow, "E", If(row("week_starting") IsNot DBNull.Value, row("week_starting").ToString(), ""), True)
                            SetCellValue(currentRow, "F", If(row("analysis") IsNot DBNull.Value, row("analysis").ToString(), ""), True)
                            SetCellValue(currentRow, "G", If(row("solution") IsNot DBNull.Value, row("solution").ToString(), ""), True)
                            
                            rowIndex += 1
                        Next

                        ' ACTUALIZAR RANGOS DE FORMATO CONDICIONAL (Crucial para mantener los colores/semáforos)
                        Dim cfLastRow As UInteger = rowIndex - 1
                        Dim startRow As UInteger = templateRowIndex
                        
                        If cfLastRow > startRow Then
                            Dim conditionalFormattings As IEnumerable(Of ConditionalFormatting) = worksheetPart.Worksheet.Elements(Of ConditionalFormatting)()
                            For Each cf As ConditionalFormatting In conditionalFormattings
                                Dim newRefs As New List(Of String)
                                For Each ref As String In cf.SequenceOfReferences.Items.Select(Function(s) s.Value)
                                    Dim cleanRef As String = ref.Replace("$", "")
                                    If cleanRef.Contains(":") Then
                                        Dim parts As String() = cleanRef.Split(":"c)
                                        If parts.Length = 2 Then
                                            Dim startRefPart As String = parts(0)
                                            Dim endRefPart As String = parts(1)
                                            Dim startRowStr As String = New String(startRefPart.SkipWhile(Function(c) Char.IsLetter(c)).ToArray())
                                            Dim startRowVal As Integer
                                            If Integer.TryParse(startRowStr, startRowVal) AndAlso startRowVal = startRow Then
                                                Dim endCol As String = New String(endRefPart.TakeWhile(Function(c) Char.IsLetter(c)).ToArray())
                                                Dim originalStart As String = ref.Split(":"c)(0)
                                                newRefs.Add($"{originalStart}:${endCol}{cfLastRow}")
                                            Else
                                                newRefs.Add(ref)
                                            End If
                                        Else
                                            newRefs.Add(ref)
                                        End If
                                    Else
                                        Dim rowStr As String = New String(cleanRef.SkipWhile(Function(c) Char.IsLetter(c)).ToArray())
                                        Dim rowVal As Integer
                                        If Integer.TryParse(rowStr, rowVal) AndAlso rowVal = startRow Then
                                            Dim colStr As String = New String(cleanRef.TakeWhile(Function(c) Char.IsLetter(c)).ToArray())
                                            newRefs.Add($"{ref}:{colStr}{cfLastRow}")
                                        Else
                                            newRefs.Add(ref)
                                        End If
                                    End If
                                Next
                                cf.SequenceOfReferences = New ListValue(Of StringValue)(newRefs.Select(Function(s) New StringValue(s)))
                            Next
                        End If

                        ' ==========================================================================================
                        ' ACTUALIZAR NAMED RANGE: TreasuryMonitoring_Report_PastWeek - Dinámico según filas de datos
                        ' ==========================================================================================
                        Dim lastDataRow As UInteger = rowIndex - 1 ' Última fila con datos
                        
                        ' Determinar fila inicial del rango según el template usado
                        Dim rangeStartRow As UInteger = If(templateRowIndex = 3, 2, 1)
                        If lastDataRow < rangeStartRow Then lastDataRow = rangeStartRow
                        
                        ' Construir referencia del rango dinámico
                        Dim rangeReference As String = $"'Sheet1'!$A${rangeStartRow}:$G${lastDataRow}"
                        Dim namedRangeName As String = "TreasuryMonitoring_Report_PastWeek"
                        
                        ' Acceder al Workbook directamente
                        Dim workbook As Workbook = workbookPart.Workbook
                        
                        ' Obtener o crear DefinedNames
                        Dim definedNames As DefinedNames = workbook.GetFirstChild(Of DefinedNames)()
                        
                        If definedNames Is Nothing Then
                            definedNames = New DefinedNames()
                            Dim sheetsElement As Sheets = workbook.GetFirstChild(Of Sheets)()
                            If sheetsElement IsNot Nothing Then
                                workbook.InsertAfter(definedNames, sheetsElement)
                            Else
                                workbook.AppendChild(definedNames)
                            End If
                        End If
                        
                        ' Actualizar solo los Named Ranges necesarios (TreasuryMonitoring_Report_PastWeek y Print_Area)
                        Dim existingMainRange As DefinedName = definedNames.Elements(Of DefinedName)().FirstOrDefault(
                            Function(d) d.Name IsNot Nothing AndAlso d.Name.Value = namedRangeName)
                        If existingMainRange IsNot Nothing Then
                            existingMainRange.Text = rangeReference
                        Else
                            Dim newDefinedName As New DefinedName() With {
                                .Name = namedRangeName,
                                .Text = rangeReference
                            }
                            definedNames.AppendChild(newDefinedName)
                        End If
                        
                        ' Actualizar Print Area
                        Dim printAreaName As String = "_xlnm.Print_Area"
                        Dim existingPrintArea As DefinedName = definedNames.Elements(Of DefinedName)().FirstOrDefault(
                            Function(d) d.Name IsNot Nothing AndAlso d.Name.Value = printAreaName)
                        If existingPrintArea IsNot Nothing Then
                            existingPrintArea.Text = rangeReference
                        Else
                            Dim newPrintArea As New DefinedName() With {
                                .Name = printAreaName,
                                .LocalSheetId = 0,
                                .Text = rangeReference
                            }
                            definedNames.AppendChild(newPrintArea)
                        End If

                        ' Guardar explícitamente la hoja y el libro
                        worksheetPart.Worksheet.Save()
                        workbookPart.Workbook.Save()
                    End Using

                    ' 6. Save Output (Save to output path)
                    Try
                        BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, outputPath)
                    Catch deleteEx As Exception
                        ' File may not exist, continue
                    End Try
                    Dim newFileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, "Treasury_Monitoring_Report_PastWeek.xlsx", "Documents/Public/Treasury")
                    Dim newFile As New XFFile(newFileInfo, String.Empty, ms.ToArray())
                    BRApi.FileSystem.InsertOrUpdateFile(si, newFile)
                End Using
                Return True

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        Private Function GetTreasuryData(ByVal si As SessionInfo, ByVal week As String, ByVal year As String) As DataTable
            ' Validate that week and year are numeric values
            Dim weekNum As Integer
            Dim yearNum As Integer
            If Not Integer.TryParse(week, weekNum) Then
                Throw New Exception($"Invalid week parameter: '{week}' - must be a numeric value")
            End If
            If Not Integer.TryParse(year, yearNum) Then
                Throw New Exception($"Invalid year parameter: '{year}' - must be a numeric value")
            End If
            
            Dim sql As String = $"
                SELECT 
                    Entity_Id,
                    Timekey,
                    Date,
                    Year,
                    Scenario,
                    Entity,
                    week_starting,
                    alert_level_status,
                    CAST(alert_level AS NVARCHAR(50)) AS alert_level,
                    ISNULL(issue, '') AS issue,
                    ISNULL(analysis, '') AS analysis,
                    ISNULL(solution, '') AS solution
                FROM XFC_TRS_MASTER_Treasury_Monitoring
                WHERE week_starting = {weekNum} AND Year = '{yearNum}'
                ORDER BY Entity_Id, Date, Scenario
            "
            Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                Return BRApi.Database.ExecuteSql(dbConnApp, sql, True)
            End Using
        End Function

        Private Function GetColumnName(ByVal cellReference As String) As String
            Dim regex As New System.Text.RegularExpressions.Regex("[A-Za-z]+")
            Dim match As System.Text.RegularExpressions.Match = regex.Match(cellReference)
            Return match.Value
        End Function

        Private Sub SetCellValue(ByVal row As Row, ByVal colName As String, ByVal value As String, Optional ByVal preserveStyle As Boolean = False)
            Dim cellReference As String = colName & row.RowIndex.ToString()
            Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
            
            If cell IsNot Nothing Then
                ' Guardar el estilo actual si se solicita preservar
                Dim currentStyleIndex As UInteger? = cell.StyleIndex

                cell.DataType = CellValues.InlineString
                cell.RemoveAllChildren()
                Dim inlineString As New InlineString()
                inlineString.Append(New Text(value))
                cell.Append(inlineString)

                ' Restaurar el estilo si se perdió al asignar valor
                If preserveStyle AndAlso currentStyleIndex.HasValue Then
                    cell.StyleIndex = currentStyleIndex
                End If
            End If
        End Sub

        Private Sub SetCellNumberValue(ByVal row As Row, ByVal colName As String, ByVal value As Double, Optional ByVal preserveStyle As Boolean = False)
            Dim cellReference As String = colName & row.RowIndex.ToString()
            Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
            
            If cell IsNot Nothing Then
                ' Guardar el estilo actual si se solicita preservar
                Dim currentStyleIndex As UInteger? = cell.StyleIndex

                cell.DataType = CellValues.Number
                cell.CellValue = New CellValue(value.ToString(System.Globalization.CultureInfo.InvariantCulture))

                ' Restaurar el estilo si se perdió al asignar valor
                If preserveStyle AndAlso currentStyleIndex.HasValue Then
                    cell.StyleIndex = currentStyleIndex
                End If
            End If
        End Sub
    End Class
End Namespace
