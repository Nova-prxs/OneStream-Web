Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Net.WebUtility
Imports System.Net.Http
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class functions
		
		#Region "Create Excel"
		
		Public Function CreateExcel(
			ByVal si As SessionInfo, _
			Optional sql As String			= "",
			Optional factory As String 		= "", 
			Optional folderPath As String 	= "Documents/Public/Times/Templates", 
			Optional destPath As String 	= "Documents/Public/Times/Exports", 
			Optional fileName As String 	= "Export_Times.xlsx", 
			Optional dt As DataTable 		= Nothing
			)
			
				' Generar un excel de cualquiera de la tablas
				
				' --------------- VARIABLES ---------------
				BRAPI.ErrorLog.LogMessage(si, "Entra En el Create")
				fileName = If(factory="", fileName, $"{fileName.Split(".")(0)}_{factory}.{fileName.Split(".")(1)}")
				brapi.ErrorLog.LogMessage(si, fileName)
				' VARIABLES
				'	Fichero
				Dim filePath As String = $"{folderPath}/{fileName}"
				destPath = $"{destPath}/{fileName}"
                Dim modified As Boolean = False

				'	Consulta para poblar el excel
			    Dim startRow As Integer = 2
                Dim endRow As Integer = 3
				
				If dt Is Nothing Then
	            	Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)	                               
	            	    dt = BRAPi.Database.ExecuteSql(dbConnApp, sql, True)					
	            	End Using  
				End If
                endRow = startRow + dt.Rows.Count - 1
			
				Dim excelBytes As Byte() = Nothing

				#Region "Create Excel From Scratch In-Memory"
				
					#Region "File Content"
				
				Dim memoryStream As New MemoryStream()
				
				' Crear un nuevo archivo Excel desde cero en el stream
				Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook)
					
				    ' Añadir partes necesarias
				    Dim workbookPart As WorkbookPart = spreadsheetDocument.AddWorkbookPart()
				    workbookPart.Workbook = New Workbook()
				
				    ' Agregar la hoja "XLSX Import"
				    Dim worksheetPartData As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
				    worksheetPartData.Worksheet = New Worksheet(New SheetData())
				    Dim sheetData As SheetData = worksheetPartData.Worksheet.GetFirstChild(Of SheetData)()
				
				    ' Agregar la hoja "Metadata"
				    Dim worksheetPartMeta As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
				    worksheetPartMeta.Worksheet = New Worksheet(New SheetData())
				    Dim sheetDataMeta As SheetData = worksheetPartMeta.Worksheet.GetFirstChild(Of SheetData)()
				
				    ' Crear estilos
				    Dim stylesPart As WorkbookStylesPart = workbookPart.AddNewPart(Of WorkbookStylesPart)()
				    stylesPart.Stylesheet = CreateStylesheet()
				    stylesPart.Stylesheet.Save()
				
				    ' Agregar hojas al libro de trabajo
				    Dim sheets As Sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(New Sheets())
				    
				    ' Agregar la hoja "XLSX Import"
				    Dim sheetDataSheet As New Sheet() With {
				        .Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPartData),
				        .SheetId = 1,
				        .Name = "XLSX Import"
				    }
				    sheets.Append(sheetDataSheet)
	
				
				    ' ---------------- Insertar datos en la hoja "XLSX Import" ----------------
				    ' Insertar fila de encabezados en la hoja "XLSX Import"
				    Dim headerRow As New Row() With {.RowIndex = CType(startRow - 1, UInt32Value)}
				
				    For colIndex As Integer = 0 To dt.Columns.Count - 1
				        Dim colLetter As String = GetExcelColumnName(colIndex + 1)
				        Dim cellReference As String = colLetter & (startRow - 1).ToString()
				
				        Dim headerCell As New Cell() With {
				            .CellReference = cellReference,
				            .CellValue = New CellValue(dt.Columns(colIndex).ColumnName),
				            .DataType = New EnumValue(Of CellValues)(CellValues.String),
				            .StyleIndex = 1
				        }
				
				        headerRow.Append(headerCell)
				    Next
				
				    sheetData.Append(headerRow)
				
				    ' Insertar datos del DataTable en la hoja "XLSX Import"
				    For rowIndex As Integer = 0 To dt.Rows.Count - 1
				        Dim newRow As New Row() With {.RowIndex = CType(rowIndex + startRow, UInt32Value)}
				
				        For colIndex As Integer = 0 To dt.Columns.Count - 1
				            Dim valor As String = If(IsDBNull(dt.Rows(rowIndex)(colIndex)), "", dt.Rows(rowIndex)(colIndex).ToString())
				            Dim colLetter As String = GetExcelColumnName(colIndex + 1)
				            Dim cellReference As String = colLetter & (rowIndex + startRow).ToString()
				
							Dim stringColumns() As String = {"id_product", "id_component", "id_account"}
 							Dim colName As String = dt.Columns.Item(colIndex).ColumnName
							
				            Dim cell As New Cell() With {.CellReference = cellReference}
				
				            If IsNumeric(valor) And Not stringColumns.Contains(colName) Then
				                cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString().Replace(",", "."))
				                cell.DataType = New EnumValue(Of CellValues)(CellValues.Number)
				            Else
				                cell.CellValue = New CellValue(valor)
				                cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
				            End If
				
				            newRow.Append(cell)
				        Next
				
				        sheetData.Append(newRow)
				    Next
					
				    ' Guardar ambas hojas
				    worksheetPartData.Worksheet.Save()
				
				    ' Guardar el archivo Excel final
				    workbookPart.Workbook.Save()
					
				End Using
	
					
					#End Region

				' Convertir el contenido del MemoryStream a un byte array
				excelBytes = memoryStream.ToArray()	
				
				#End Region
				
				#Region "Create File"
				' --------------- GUARDADO FINAL DEL ARCHIVO ---------------
				
	            ' Actualizar el archivo en OneStream
	            Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, destPath)
	            Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, excelBytes)
				fileFile.FileInfo.ContentFileExtension = "xlsx"
	            brapi.FileSystem.InsertOrUpdateFile(si, fileFile)
				
				#End Region
			
			Return Nothing
		End Function
		
		#End Region
				
		#Region "Helper Functions for Excel"
               ' Función que obtiene el valor de una celda, considerando si es SharedString
        Public Function GetCellValue(si As SessionInfo, document As SpreadsheetDocument, cell As Cell) As String
            If cell Is Nothing OrElse cell.CellValue Is Nothing Then
                Return ""
            End If
 
            Dim value As String = cell.CellValue.InnerText
            If cell.DataType IsNot Nothing AndAlso cell.DataType.Value = CellValues.SharedString Then
                Dim sharedStringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable
                value = sharedStringTable.ChildElements(CInt(value)).InnerText
            End If
 
            Return value
        End Function
 
        ' Función para crear una celda con el texto dado
		Public Function CreateCell(valor As String) As Cell
		    Dim cell As New Cell()
		    
		    If IsNumeric(valor) Then
		        Dim valorNumerico As Double = Convert.ToDouble(valor)
		        cell.CellValue = New CellValue(valorNumerico.ToString(CultureInfo.InvariantCulture))
		        cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
		    Else
		        cell.CellValue = New CellValue(valor)
		        cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
		    End If
		
		    Return cell
		End Function
 
        ' Función para obtener el nombre de la columna en Excel (1 -> A, 2 -> B, etc.)
        Public Function GetExcelColumnName(ByVal columnNumber As Integer) As String
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
 
        ' Función para insertar la celda en orden dentro de la fila
		Public Sub InsertCellInOrder(ByVal row As Row, ByVal newCell As Cell)
		    Dim refCell As Cell = Nothing
		
		    For Each cell As Cell In row.Elements(Of Cell)()
		        If String.Compare(cell.CellReference.Value, newCell.CellReference.Value, True) > 0 Then
		            refCell = cell
		            Exit For
		        End If
		    Next
		
		    If refCell IsNot Nothing Then
		        row.InsertBefore(newCell, refCell)
		    Else
		        row.Append(newCell) ' Si no hay referencia, añadir al final
		    End If
		End Sub
		
		Private Function CreateStylesheet() As Stylesheet
		    Dim stylesheet As New Stylesheet()
		
		    ' Fonts
		    Dim fonts As New Fonts(
		        New Font(), ' Default
		        New Font( ' Font blanco
		            New Color() With {.Rgb = New HexBinaryValue() With {.Value = "FFFFFF"}},
		            New Bold()
		        )
		    )
		
		    ' Fills
		    Dim fills As New Fills(
		        New Fill(New PatternFill() With {.PatternType = PatternValues.None}),
		        New Fill(New PatternFill() With {.PatternType = PatternValues.Gray125}),
		        New Fill( ' Azul oscuro
		            New PatternFill(
		                New ForegroundColor() With {.Rgb = New HexBinaryValue() With {.Value = "156082"}},
		                New BackgroundColor() With {.Indexed = 64}
		            ) With {.PatternType = PatternValues.Solid}
		        )
		    )
		
		    ' Borders
		    Dim borders As New Borders(New Border()) ' Default border
		
		    ' CellFormats
		    Dim cellFormats As New CellFormats(
		        New CellFormat(), ' default
		        New CellFormat With {
		            .FontId = 1,
		            .FillId = 2,
		            .BorderId = 0,
		            .ApplyFont = True,
		            .ApplyFill = True
		        }
		    )
		
		    stylesheet.Append(fonts)
		    stylesheet.Append(fills)
		    stylesheet.Append(borders)
		    stylesheet.Append(cellFormats)
		
		    Return stylesheet
		End Function
		
	#End Region
			
	End Class
End Namespace
