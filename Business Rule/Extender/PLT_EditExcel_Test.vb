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
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet
 
Namespace OneStream.BusinessRule.Extender.PLT_EditExcel_Test
    Public Class MainClass
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Try
				Dim factory As String = args.NameValuePairs("factory")
				Dim year As String = args.NameValuePairs("year")
				Dim scenario As String = args.NameValuePairs("scenario")
				
				
				' brapi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase,$"Documents/Public/Plants/{factory}", $"{year}/{scenario}")
				
				If  si.UserName = "MiguelTest" And BRApi.FileSystem.DoesFileExist(si,FileSystemLocation.ApplicationDatabase,$"Documents/Public/Plants/{factory}/{year}/{scenario}") Then 
					
					Dim newFolder As New XFFolder
					
					newFolder.FileSystemLocation = FileSystemLocation.ApplicationDatabase
					newFolder.ParentFullName = $"Documents/Public/Plants/{factory}/{year}"
					newFolder.Name = $"{scenario}"
					
					Brapi.FileSystem.InsertOrUpdateFolder(si, newFolder)
					
				End If
				
				CreateExcel(si,args.NameValuePairs("factory"),args.NameValuePairs("year"),args.NameValuePairs.GetValueOrDefault("month", "0"), args.NameValuePairs("scenario"), args.NameValuePairs.GetValueOrDefault("time", ""),,args.NameValuePairs("fileName"))
            
			Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
 
            Return Nothing
        End Function
 
		Public Function CreateExcel(
			ByVal si As SessionInfo, _
			Optional factory As String = "", Optional year As Integer = 0, Optional month As String = "0", _
			Optional scenario As String = "", Optional time As String = "", _
			Optional folderPath As String = "", Optional fileName As String = "Export.xlsx", Optional destPath As String = "", _
			Optional sql As String = "", Optional dtC As DataTable = Nothing
			)
				' Generar un excel de cualquiera de la tablas
				
				' --------------- VARIABLES ---------------
				
				' VARIABLES
				'	Fichero
               	folderPath = "Documents/Public/Plants/Exports"
				
				Dim filePath As String = $"{folderPath}/{fileName}"
				If destPath = "" Then destPath = $"{folderPath}/{fileName}" ' No usado de momento
                Dim modified As Boolean = False
 				
				'	Consulta para poblar el excel
				Dim dt As New DataTable 
				
			#Region "Templates"	
			
				' Definir el rango de filas a modificar (por ejemplo, de la fila 11 a la 100)
				Dim templateFile As Boolean = False
				
                Dim startRow As Integer = 0
                Dim endRow As Integer = 0
				
				If fileName = "XFC_PLT_PLAN_CostsInput.xlsx" Then
					
					templateFile = True
               	 	startRow = 11
					sql = $"
							SELECT id_costcenter, id_account, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12
							
							FROM VIEW_PLT_FACT_Costs
					
							WHERE 1=1
								AND scenario = '{scenario}'
								AND id_factory = '{factory}'
								AND year = {year}
					
					"
				Else If fileName = "XFC_PLT_PLAN_CostsInput_Lines.xlsx" Then
					
					templateFile = True
               	 	startRow = 11
					sql = $"
							SELECT TOP(1) id_costcenter, id_account, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12
							
							FROM VIEW_PLT_FACT_Costs
					
							WHERE 1=1
								AND scenario = '{scenario}'
								AND id_factory = '{factory}'
								AND year = {year}
					
					"	
				Else If fileName = "XFC_PLT_PLAN_VolumesInput.xlsx" Then
					
					templateFile = True
               	 	startRow = 11
					sql = $" 
							SELECT id_costcenter, id_averagegroup, id_product, product, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12
					
							FROM VIEW_PLT_AUX_Production_Planning_Volumes
					
							WHERE 1=1
								AND scenario = '{scenario}' 
								AND id_factory = '{factory}' 
								AND year = {year}		
					
							ORDER BY id_costcenter ASC, id_averagegroup ASC, id_product ASC
					"
					
				Else If fileName = "XFC_PLT_PLAN_VolumesInput_Distribution.xlsx" Then
					
					templateFile = True
               	 	startRow = 11
					sql = $" 
							SELECT  id_product_final, id_product_parent, id_product, product, id_averagegroup, id_costcenter, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12
					
							FROM VIEW_PLT_AUX_Production_Planning_Volumes_Dist
					
							WHERE 1=1
								AND scenario = '{scenario}' 
								AND id_factory = '{factory}' 
								AND year = {year}		
					
							ORDER BY id_costcenter ASC, id_averagegroup ASC, id_product ASC, id_product_parent ASC, id_product_final ASC
					"	
				Else If fileName = "XFC_PLT_PLAN_AllocationsInput.xlsx" Then

					templateFile = True
               	 	startRow = 11
					sql = $" 
							SELECT 
								id_averagegroup
								, '-1' as id_costcenter
								, id_product
								, product
								, uotype
								, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12
								, R01, R02, R03, R04, R05, R06, R07, R08, R09, R10, R11, R12
					
							FROM VIEW_PLT_AUX_Production_Planning_Times
					
							WHERE 1=1
								AND scenario = '{scenario}' 
								AND id_factory = '{factory}' 
								AND year = {year}
					"
									
				Else If fileName = "XFC_PLT_ACT_Nomenclature.xlsx" Then
                    
                    templateFile = True
               	 	startRow = 11
					sql = $" 
							SELECT
								  ISNULL(N.id_product, '') 		AS id_product
								, ISNULL(MP.description, '') 	AS description
								, ISNULL(N.id_component, '') 	AS id_component
								, ISNULL(N.coefficient, 0) 		AS coefficient
								, '' as CodePlusBas
								, '' as NoOrdre
								, '' as NoSeq
								, ISNULL(N.prorata, 0)			AS prorata
								, CONVERT(VARCHAR, N.start_date, 112) as start_dat
								, CONVERT(VARCHAR, N.end_date, 112) as end_date
					
							FROM XFC_PLT_HIER_Nomenclature_Date N
							LEFT JOIN XFC_PLT_MASTER_Product MP
								ON MP.id = id_product
					
							WHERE 1=1								 
								AND id_factory = '{factory}' 								
					"				
					
				Else If fileName = "XFC_PLT_PLAN_Nomenclature.xlsx" Then
					templateFile = True
               	 	startRow = 11
					sql = $" 
							SELECT
								id_product
								, id_component
								, start_date
								, end_date
								, coefficient
								, prorata
					
							FROM XFC_PLT_HIER_Nomenclature_Date_Planning
					
							WHERE 1=1
								AND scenario = '{scenario}' 
								AND id_factory = '{factory}' 
								
					"

				End If	
				
				#Region "FileShare New Scenario"
				
				If templateFile = True Then
					
					brapi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase,$"Documents/Public/Plants/Import/{factory}",$"{year}/{scenario}")

					End If
				
				
				#End Region
				
			#End Region
			
			#Region "DataTable"
				If sql = "" Or templateFile = True Then					
					' Creamos el DataTable con los datos
		            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)	                               
		                dt = BRAPi.Database.ExecuteSql(dbConnApp, sql, True)					
		            End Using  
				Else 
					dt = dtC.Copy()
				End If
				
                endRow = startRow + dt.Rows.Count - 1
			#End Region	
			
			#Region "Modify Excel"
			
				' --------------- MODIFICACIÓN DEL EXCEL ---------------
				
                ' Verificar si el archivo existe
                Dim fileExists As Boolean = brapi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath)
                If Not fileExists Then
                    brapi.ErrorLog.LogMessage(si, "El archivo no se encontró en la ruta: " & filePath)
                    Return Nothing
                End If
				
                ' Obtener el archivo en bytes
                Dim fileBytes As Byte() = brapi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, True).XFFile.ContentFileBytes
                If fileBytes Is Nothing OrElse fileBytes.Length = 0 Then
                    brapi.ErrorLog.LogMessage(si, "El archivo está vacío o no se pudo leer correctamente.")
                    Return Nothing
                End If
 
                ' Cargar el archivo en un MemoryStream
                Dim memoryStream As New MemoryStream()
                memoryStream.Write(fileBytes, 0, fileBytes.Length)
                memoryStream.Position = 0
 				
                Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Open(memoryStream, True)

                    Dim workbookPart As WorkbookPart = spreadsheetDocument.WorkbookPart
 
                    ' Seleccionar la hoja "XLSX Import"
                    Dim targetSheetName As String = "XLSX Import"
                    Dim sheet As Sheet = workbookPart.Workbook.Sheets.Elements(Of Sheet)().FirstOrDefault(Function(s) s.Name.Value = targetSheetName)
                    If sheet Is Nothing Then
                        brapi.ErrorLog.LogMessage(si, "No se encontró la hoja: " & targetSheetName)
                        Return Nothing
                    End If
 
                    Dim worksheetPart As WorksheetPart = CType(workbookPart.GetPartById(sheet.Id.Value), WorksheetPart)
                    Dim sheetData As SheetData = worksheetPart.Worksheet.Elements(Of SheetData)().FirstOrDefault()

					' Eliminar el dato existente - Elegir un método, vaciar el dato o 
					
					' 🔴 PASO 1: Limpiar los valores de las celdas (sin eliminar las filas)
					For Each row As Row In sheetData.Elements(Of Row)()
					    If CInt(row.RowIndex.Value) >= startRow Then
					        For Each cell As Cell In row.Elements(Of Cell)()
					            ' Limpia el contenido de la celda
								
'								cell.CellValue = New CellValue("")
'					            cell.DataType = New EnumValue(Of CellValues)(CellValues.String)

								cell.CellValue = Nothing
					            cell.DataType = Nothing
								modified = True
					        Next
					    End If
					Next
										
					' Guardar la hoja después de limpiar
					worksheetPart.Worksheet.Save()
										
				#Region "Insertando Datos"	
				
					' Insertar los datos nuevos
                    For rowIndex As Integer = startRow To endRow
						Dim dtRowIndex As Integer = rowIndex - startRow
                        ' Obtener la fila; si no existe, se crea y se agrega al SheetData
                        Dim row As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) CInt(r.RowIndex.Value) = rowIndex)
                        If row Is Nothing Then
                            row = New Row() With {.RowIndex = CType(rowIndex, UInt32)}
                            sheetData.Append(row)
                        End If
						
						' Recorrer columnas del DataTable
                        For colIndex As Integer = 1 To dt.Columns.Count
							Dim valor As String = If(IsDBNull(dt.Rows(dtRowIndex)(colIndex-1)), "", dt.Rows(dtRowIndex)(colIndex-1).ToString())									

                            Dim colLetter As String = GetExcelColumnName(colIndex)
                            Dim cellReference As String = colLetter & rowIndex.ToString()
							
							Dim colStringName() As String = {"id_product", "id_component", "id_account", "id_averagegroup", "id_costcenter"}
 							Dim colName As String = dt.Columns.Item(colIndex-1).ColumnName
							
'                            ' Buscar la celda en la fila
                            Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
                            If cell Is Nothing Then
								
'                                 Crear la celda si no existe
								cell = New Cell()
                                cell.CellReference = cellReference
								
								If IsNumeric(valor) And Not colStringName.Contains(colName) Then
									cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString(CultureInfo.InvariantCulture))
									cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
								Else
								    cell.CellValue = New CellValue(valor.ToString())
								    cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
								End If		
'								row.InsertAt(cell, 0)
								row.Append(cell)
								
                            Else
								
                                ' Si ya existe, modificarla - Si es un valor numérico hacemos la conversión
								If IsNumeric(valor) And Not colStringName.Contains(colName) Then
								    cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString(CultureInfo.InvariantCulture))
								    cell.DataType = New EnumValue(Of CellValues)(CellValues.Number)
								Else
								    cell.CellValue = New CellValue(valor)
								    cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
								End If
								
                            End If
                            modified = True
                        Next
						
						#Region "Recorrer Columnas - OLD" 
'                        ' Recorrer columnas del DataTable
'                        For colIndex As Integer = 1 To dt.Columns.Count
							
'							Dim rowDT As Integer = rowIndex - startRow
'							Dim valor As String = If(IsDBNull(dt.Rows(rowDT)(colIndex-1)), String.Empty, dt.Rows(rowDT)(colIndex-1).ToString())	
'							valor = If (valor.ToString() = "0", String.Empty, valor)
								
'                            Dim colLetter As String = GetExcelColumnName(colIndex)
'                            Dim cellReference As String = colLetter & rowIndex.ToString()
 
'                            ' Buscar la celda en la fila
'                            Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
'                            If cell Is Nothing Then
'                                ' Crear la celda si no existe
'                                cell = CreateCell(valor)
'                                cell.CellReference = cellReference
'                                InsertCellInOrder(row, cell)
'                            Else
'                                ' Si ya existe, modificarla - Si es un valor numérico hacemos la conversión
'								If IsNumeric(valor) Then
'								    cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString(CultureInfo.InvariantCulture))
'								    cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
'								Else
'								    cell.CellValue = New CellValue(valor)
'								    cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
'								End If
'                            End If
'                            modified = True
'                        Next
						#End Region
						
                    Next
					
 				#End Region
				
                    If modified Then
                        worksheetPart.Worksheet.Save()
                        workbookPart.Workbook.Save()
                    Else
                        brapi.ErrorLog.LogMessage(si, "No se encontró ninguna celda para modificar.")
                    End If
                End Using
				
 			#End Region

			#Region "Create File"
				' --------------- GUARDADO FINAL DEL ARCHIVO ---------------
				
                ' Actualizar el archivo en OneStream
                Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, destPath)
                Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, memoryStream.ToArray())
				fileFile.FileInfo.ContentFileExtension = "xlsx"
                brapi.FileSystem.InsertOrUpdateFile(si, fileFile)
				
			#End Region
			
			Return Nothing
			
		End Function

		#Region "Excel - Helper Functions"
		
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
		        Dim valorNumerico As Double = Convert.ToDouble(valor, CultureInfo.InvariantCulture)
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
            row.InsertBefore(newCell, refCell)
        End Sub
		
		#End Region
		
    End Class
End Namespace