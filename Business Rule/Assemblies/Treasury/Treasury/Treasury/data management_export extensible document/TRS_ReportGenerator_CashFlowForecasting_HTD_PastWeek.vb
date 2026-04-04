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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.TRS_ReportGenerator_CashFlowForecasting_HTD_PastWeek

	' ==================================================================================
	' TRS_ReportGenerator_CashFlowForecasting_HTD_PastWeek
	' ==================================================================================
	' This business rule is a copy of TRS_ReportGenerator_CashFlowForecasting_HTD
	' that uses (paramWeek - 1) instead of paramWeek.
	'
	' TEMPLATE: Documents/Public/Treasury/Extensible Document/CashFlowSummary_HTD.xlsx
	' OUTPUT:   Documents/Public/Treasury/CashFlowSummary_HTD_PastWeek.xlsx
	'
	' This follows the same pattern as TRS_ReportGenerator_CashDebt_HTD_PastWeek
	' ==================================================================================

	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						Dim functionName As String = String.Empty
						If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("p_function") Then
							functionName = args.NameValuePairs("p_function")
						End If
						If Not String.IsNullOrEmpty(functionName) Then
							If functionName.Equals("GenerateCashFlowForecastingReportPastWeek", StringComparison.InvariantCultureIgnoreCase) Then
								Return GenerateCashFlowForecastingReportPastWeek(si, args)
							End If
						End If
				End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Function GenerateCashFlowForecastingReportPastWeek(ByVal si As SessionInfo, ByVal args As ExtenderArgs) As Object
			Try
				' 1. Get Parameters
				Dim paramWeek As Integer = 0
				Dim paramYear As Integer = 0
				If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("paramWeek") Then
					Integer.TryParse(args.NameValuePairs("paramWeek"), paramWeek)
				End If
				If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey("paramYear") Then
					Integer.TryParse(args.NameValuePairs("paramYear"), paramYear)
				End If
				If paramWeek = 0 OrElse paramYear = 0 Then
					Throw New Exception("Parameters paramWeek and paramYear are required and must be numeric.")
				End If

				' ==========================================================================================
				' 2. CALCULATE TARGET WEEK - USE PREVIOUS WEEK (paramWeek - 1)
				' ==========================================================================================
				Dim targetWeek As Integer = If(paramWeek > 1, paramWeek - 1, 1)
				
				' Log the entry point
				BRApi.ErrorLog.LogMessage(si, $"GenerateCashFlowForecastingReportPastWeek: Entrada con paramWeek={paramWeek}, usando targetWeek={targetWeek} del año {paramYear}")

				' Calculate scenario: FW + week (2 digits with zero padding)
				Dim scenario As String = "FW" + targetWeek.ToString().PadLeft(2, "0"c)

				' 2. Define HTD entities (Horse Powertrain Solution replaces Horse Holding)
				Dim htdEntities As New List(Of String) From {
					"Horse Aveiro", "OYAK HORSE", "Horse Romania", "Horse Brazil",
					"Horse Chile", "Horse Argentina", "Horse Spain", "Horse Powertrain Solution"
				}
				
				' 3. Calculate 9-week window (4 before, current, 4 after) - USING targetWeek
				Dim weekWindow As New List(Of Integer)
				For i As Integer = -4 To 4
					Dim w As Integer = targetWeek + i
					If w >= 1 Then
						weekWindow.Add(w)
					End If
				Next
				
				' Ensure we have at most 9 weeks and they are valid
				weekWindow = weekWindow.Take(9).ToList()
				
				' Log the week range being processed
				BRApi.ErrorLog.LogMessage(si, $"Rango de semanas: Semana {weekWindow.First()} a Semana {weekWindow.Last()} del año {paramYear}")

				' 4. Get Data using CF_CB_HTD logic (accumulated cashflow)
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					' Get max weeks for the year
					Dim maxWeeksSql As String = TRS_SQL_Repository.GetMaxWeekInYearQuery(paramYear)
					Dim maxWeeksDt As DataTable = BRApi.Database.ExecuteSql(dbConn, maxWeeksSql, False)
					Dim maxWeeks As Integer = 52
					If maxWeeksDt IsNot Nothing AndAlso maxWeeksDt.Rows.Count > 0 AndAlso Not IsDBNull(maxWeeksDt.Rows(0)("MaxWeek")) Then
						maxWeeks = Convert.ToInt32(maxWeeksDt.Rows(0)("MaxWeek"))
					End If
					
					' Query using CF_CB_HTD logic: Actual data with UploadWeekNumber = ProjectionWeekNumber + 1
					' All 5 Account types for complete Cash Balance
					' NOTE: Uses targetWeek instead of paramWeek
					Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowActualGroupedByEntityWeekQuery(paramYear, targetWeek, True)
					
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, allDataSql, False)
					If dt.Rows.Count = 0 Then
						BRApi.ErrorLog.LogMessage(si, "Datos obtenidos de XFC_TRS_Master_CashflowForecasting: 0 registros encontrados")
					Else
						BRApi.ErrorLog.LogMessage(si, $"Datos obtenidos de XFC_TRS_Master_CashflowForecasting: {dt.Rows.Count} registros encontrados")
					End If
					
					' Build weekly totals dictionary: Entity -> Week -> Amount
					Dim weeklyTotals As New Dictionary(Of String, Dictionary(Of Integer, Decimal))
					For Each entity As String In htdEntities
						weeklyTotals(entity) = New Dictionary(Of Integer, Decimal)
						For weekNum As Integer = 1 To maxWeeks
							weeklyTotals(entity)(weekNum) = 0
						Next
					Next
					
					' Process query results
					For Each row As DataRow In dt.Rows
						Dim entity As String = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
						Dim weekNumber As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
						Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
						
						' Only process HTD entities
						If Not htdEntities.Contains(entity) Then Continue For
						If weekNumber < 1 OrElse weekNumber > maxWeeks Then Continue For
						
						weeklyTotals(entity)(weekNumber) += amount
					Next
					
					' Calculate ACCUMULATED values for each entity (progressive sum from Week 1)
					Dim entityData As New Dictionary(Of String, Dictionary(Of Integer, Decimal))
					For Each entity As String In htdEntities
						entityData(entity) = New Dictionary(Of Integer, Decimal)
						Dim previousWeekAccumulated As Decimal = 0
						
						For weekNum As Integer = 1 To maxWeeks
							Dim weekTotal As Decimal = 0
							If weeklyTotals.ContainsKey(entity) AndAlso weeklyTotals(entity).ContainsKey(weekNum) Then
								weekTotal = weeklyTotals(entity)(weekNum)
							End If
							
							' Accumulation: add weekTotal to previous accumulated value
							Dim currentWeekAccumulated As Decimal = previousWeekAccumulated + weekTotal
							entityData(entity)(weekNum) = currentWeekAccumulated
							previousWeekAccumulated = currentWeekAccumulated
						Next
					Next

					' 5. Load File (template) - SAME TEMPLATE AS ORIGINAL
					Dim templatePath As String = "Documents/Public/Treasury/Extensible Document/CashFlowSummary_HTD.xlsx"
					' OUTPUT PATH IS DIFFERENT - PastWeek version
					Dim outputPath As String = "Documents/Public/Treasury/CashFlowSummary_HTD_PastWeek.xlsx"
					Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, templatePath, True, False)
					If fileInfo Is Nothing OrElse fileInfo.XFFile.ContentFileBytes Is Nothing Then
						Throw New Exception($"Template file not found at {templatePath}")
					End If
					Dim fileBytes As Byte() = fileInfo.XFFile.ContentFileBytes

					Using ms As New MemoryStream()
						ms.Write(fileBytes, 0, fileBytes.Length)
						ms.Position = 0
						
						Using doc As SpreadsheetDocument = SpreadsheetDocument.Open(ms, isEditable:=True)
							If doc.WorkbookPart.CalculationChainPart IsNot Nothing Then
								doc.WorkbookPart.DeletePart(doc.WorkbookPart.CalculationChainPart)
							End If
							
							Dim workbookPart As WorkbookPart = doc.WorkbookPart
							Dim sheetName As String = "Accum CashFlow + Cash Balance"
							Dim sheet As Sheet = workbookPart.Workbook.Descendants(Of Sheet)().Where(Function(s) s.Name = sheetName).FirstOrDefault()
							If sheet Is Nothing Then
								Throw New Exception($"Sheet '{sheetName}' not found in file.")
							End If
							
							Dim worksheetPart As WorksheetPart = CType(workbookPart.GetPartById(sheet.Id), WorksheetPart)
							Dim sheetData As SheetData = worksheetPart.Worksheet.Elements(Of SheetData)().First()

							' Ajuste de columnas y filas
							Dim columns As Columns = worksheetPart.Worksheet.GetFirstChild(Of Columns)()
							If columns IsNot Nothing Then
								For Each col As Column In columns.Elements(Of Column)()
									col.CustomWidth = True
								Next
							End If

							' --- Set Title, Week Label, and Date ---
							Dim titleRow As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = 1)
							If titleRow IsNot Nothing Then
								SetCellStringValue(titleRow, "A", "HORSE HTD - Cashflow monitoring report SUMMARY (Past Week)", True)
								titleRow.Height = 21
								titleRow.CustomHeight = True
							End If

							' --- Set week labels and dates for 9 columns (B-J) ---
							Dim headerRow As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = 2)
							Dim dateRow As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = 3)
							
							For colIdx As Integer = 0 To weekWindow.Count - 1
								Dim colLetter As String = Chr(66 + colIdx) ' B=66, C=67, etc.
								Dim weekNum As Integer = weekWindow(colIdx)
								Dim weekLabel As String = $"Week {weekNum}"
								Dim mondayDate As DateTime = FirstMondayOfWeek(paramYear, weekNum)
								
								If headerRow IsNot Nothing Then
									SetCellStringValue(headerRow, colLetter, weekLabel, True)
								End If
								If dateRow IsNot Nothing Then
									SetCellStringValue(dateRow, colLetter, mondayDate.ToString("dd-MMM", CultureInfo.InvariantCulture), True)
								End If
							Next
							
							If headerRow IsNot Nothing Then
								headerRow.Height = 20
								headerRow.CustomHeight = True
							End If

							' --- Entities list including Total Horse Group ---
							Dim entities As New List(Of String)(htdEntities)
							entities.Add("Total Horse Group")
							BRApi.ErrorLog.LogMessage(si, $"Entidades a procesar: {String.Join(", ", entities)}")

							' Limpiar datos antiguos (desde fila 4)
							Dim rowsToRemove As List(Of Row) = sheetData.Elements(Of Row)().Where(Function(r) r.RowIndex.Value >= 4).ToList()
							For Each r As Row In rowsToRemove
								r.Remove()
							Next

							' Create entity rows
							Dim entityRows As New Dictionary(Of String, Row)
							For i As Integer = 0 To entities.Count - 1
								Dim rowIndex As UInteger = 4 + CUInt(i)
								Dim entity = entities(i)
								Dim entityRow As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) r.RowIndex.Value = rowIndex)
								If entityRow Is Nothing Then
									entityRow = CType(sheetData.Elements(Of Row)().Last().CloneNode(True), Row)
									entityRow.RowIndex = New UInt32Value(rowIndex)
									For Each cell As Cell In entityRow.Elements(Of Cell)()
										Dim col As String = GetColumnName(cell.CellReference.Value)
										cell.CellReference = New StringValue(col & rowIndex.ToString())
									Next
									sheetData.Append(entityRow)
									BRApi.ErrorLog.LogMessage(si, $"Fila creada para entidad '{entity}' en fila {rowIndex}")
								End If
								entityRows(entity) = entityRow
							Next

							' Fill entity names and accumulated data
							For i As Integer = 0 To entities.Count - 1
								Dim entity = entities(i)
								Dim entityRow = entityRows(entity)
								SetCellStringValue(entityRow, "A", entity, True)
								BRApi.ErrorLog.LogMessage(si, $"Escribiendo nombre de entidad en celda A{entityRow.RowIndex}: '{entity}'")
								
								If entity = "Total Horse Group" Then
									' Sum all HTD entities for each week in window
									For colIdx As Integer = 0 To weekWindow.Count - 1
										Dim colLetter As String = Chr(66 + colIdx)
										Dim weekNum As Integer = weekWindow(colIdx)
										Dim totalValue As Decimal = 0
										
										For Each e As String In htdEntities
											If entityData.ContainsKey(e) AndAlso entityData(e).ContainsKey(weekNum) Then
												totalValue += entityData(e)(weekNum)
											End If
										Next
										
										BRApi.ErrorLog.LogMessage(si, $"Entity: {entity} | Week: {weekNum} | Amount: {totalValue} | Scenario: {scenario}")
										SetCellNumericValue(entityRow, colLetter, totalValue, True)
									Next
								Else
									' Individual entity accumulated data
									For colIdx As Integer = 0 To weekWindow.Count - 1
										Dim colLetter As String = Chr(66 + colIdx)
										Dim weekNum As Integer = weekWindow(colIdx)
										Dim accumulatedValue As Decimal = 0
										
										If entityData.ContainsKey(entity) AndAlso entityData(entity).ContainsKey(weekNum) Then
											accumulatedValue = entityData(entity)(weekNum)
										End If
										
										BRApi.ErrorLog.LogMessage(si, $"Entity: {entity} | Week: {weekNum} | Amount: {accumulatedValue} | Scenario: {scenario}")
										SetCellNumericValue(entityRow, colLetter, accumulatedValue, True)
									Next
								End If
								
								' Clear excess columns (from column after last week to column 50)
								For colIdx As Integer = weekWindow.Count + 2 To 50
									Dim colLetter As String = GetExcelColumnLetter(colIdx)
									SetCellStringValue(entityRow, colLetter, "", True)
								Next
							Next

							' Clear excess columns in header and date rows
							If headerRow IsNot Nothing Then
								For colIdx As Integer = weekWindow.Count + 2 To 50
									Dim colLetter As String = GetExcelColumnLetter(colIdx)
									SetCellStringValue(headerRow, colLetter, "", True)
								Next
							End If
							
							If dateRow IsNot Nothing Then
								For colIdx As Integer = weekWindow.Count + 2 To 50
									Dim colLetter As String = GetExcelColumnLetter(colIdx)
									SetCellStringValue(dateRow, colLetter, "", True)
								Next
							End If

							worksheetPart.Worksheet.Save()
							workbookPart.Workbook.Save()
						End Using

						' DELETE AND SAVE TO NEW OUTPUT PATH (PastWeek)
						BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, outputPath)
						Dim newFileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, "CashFlowSummary_HTD_PastWeek.xlsx", "Documents/Public/Treasury")
						Dim newFile As New XFFile(newFileInfo, String.Empty, ms.ToArray())
						BRApi.FileSystem.InsertOrUpdateFile(si, newFile)
					End Using
				End Using
				
				Return True
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

	Private Function GetColumnName(ByVal cellReference As String) As String
		Dim regex As New System.Text.RegularExpressions.Regex("[A-Za-z]+")
		Dim match As System.Text.RegularExpressions.Match = regex.Match(cellReference)
		Return match.Value
	End Function

	Private Sub SetCellStringValue(ByVal row As Row, ByVal colName As String, ByVal value As String, Optional ByVal preserveStyle As Boolean = False)
		Dim cellReference As String = colName & row.RowIndex.ToString()
		Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
		
		If cell IsNot Nothing Then
			Dim currentStyleIndex As UInteger? = cell.StyleIndex
			cell.DataType = CellValues.InlineString
			cell.RemoveAllChildren()
			
			Dim inlineString As New InlineString()
			inlineString.Append(New Text(value))
			cell.Append(inlineString)
			
			If preserveStyle AndAlso currentStyleIndex.HasValue Then
				cell.StyleIndex = currentStyleIndex
			End If
		End If
	End Sub

	Private Sub SetCellNumericValue(ByVal row As Row, ByVal colName As String, ByVal value As Decimal, Optional ByVal preserveStyle As Boolean = False)
		Dim cellReference As String = colName & row.RowIndex.ToString()
		Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
		
		If cell IsNot Nothing Then
			Dim currentStyleIndex As UInteger? = cell.StyleIndex
			cell.DataType = CellValues.Number
			cell.RemoveAllChildren()
			
			' Round to integer (no decimals)
			Dim roundedValue As Long = CLng(Math.Round(value, 0, MidpointRounding.AwayFromZero))
			Dim cellValue As New CellValue(roundedValue.ToString(CultureInfo.InvariantCulture))
			cell.Append(cellValue)
			
			If preserveStyle AndAlso currentStyleIndex.HasValue Then
				cell.StyleIndex = currentStyleIndex
			End If
		End If
	End Sub

	' Helper: Get first Monday of a given year/week (ISO 8601 format)
	Private Function FirstMondayOfWeek(year As Integer, week As Integer) As DateTime
		' ISO 8601: Week 1 is the week with the first Thursday of the year
		Dim jan4 As DateTime = New DateTime(year, 1, 4)
		Dim daysFromMonday As Integer = (jan4.DayOfWeek - DayOfWeek.Monday + 7) Mod 7
		Dim firstMonday As DateTime = jan4.AddDays(-daysFromMonday)
		
		' Calculate the Monday of the requested week
		Return firstMonday.AddDays((week - 1) * 7)
	End Function

	' Helper: Get Excel column letter from index (1-based)
	Private Function GetExcelColumnLetter(colIdx As Integer) As String
		Dim dividend As Integer = colIdx
		Dim columnName As String = String.Empty
		
		While dividend > 0
			Dim modulo As Integer = (dividend - 1) Mod 26
			columnName = Chr(65 + modulo) & columnName
			dividend = (dividend - modulo - 1) \ 26
		End While
		
		Return columnName
	End Function
End Class
End Namespace
