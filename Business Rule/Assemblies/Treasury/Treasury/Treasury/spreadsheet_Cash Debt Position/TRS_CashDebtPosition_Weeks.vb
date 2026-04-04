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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashDebtPosition_Weeks
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						Return Nothing
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							list.Add("prm_Treasury_CompanyNames")
							list.Add("prm_Treasury_Year")
							list.Add("prm_Treasury_WeekNumber_From")
							list.Add("prm_Treasury_WeekNumber_To")
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return Me.GetWeeklyCashReport(si, args)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Get Weekly Cash Report"

		Private Function GetWeeklyCashReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
			Try				
				' STEP 1: Extract parameters from spreadsheet args
				Dim paramEntityId As String = ""
				Dim paramYear As String = ""
				Dim paramWeekFrom As String = ""
				Dim paramWeekTo As String = ""
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_CompanyNames") Then
					paramEntityId = args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year") Then
					paramYear = args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber_From") Then
					paramWeekFrom = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber_From")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber_To") Then
					paramWeekTo = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber_To")
				End If
				
				' STEP 2: EARLY VALIDATION - CRITICAL - BEFORE ANY SQL
				If String.IsNullOrWhiteSpace(paramEntityId) Then
					Return Me.CreateEmptyTableView("Error: Entity parameter is required")
				End If
				
				If String.IsNullOrWhiteSpace(paramYear) OrElse Not IsNumeric(paramYear) Then
					Return Me.CreateEmptyTableView("Error: Valid Year parameter is required")
				End If
				
				If String.IsNullOrWhiteSpace(paramWeekFrom) OrElse Not IsNumeric(paramWeekFrom) Then
					Return Me.CreateEmptyTableView("Error: Valid Week From parameter is required")
				End If
				
				If String.IsNullOrWhiteSpace(paramWeekTo) OrElse Not IsNumeric(paramWeekTo) Then
					Return Me.CreateEmptyTableView("Error: Valid Week To parameter is required")
				End If
				
				Dim weekFrom As Integer = Convert.ToInt32(paramWeekFrom)
				Dim weekTo As Integer = Convert.ToInt32(paramWeekTo)
				
				If weekFrom > weekTo Then
					Return Me.CreateEmptyTableView("Error: Week From cannot be greater than Week To")
				End If
				
				' STEP 3: Convert company ID to name
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
				
				' STEP 4: Execute query to get all weeks data for the year
				Dim weeklyData As DataTable = Me.GetAllWeeksData(si, paramYear, paramEntity, weekFrom, weekTo)
				
				' STEP 5: Build weekly TableView
				Return Me.BuildWeeklyTableView(si, weeklyData, paramEntityId, paramYear, weekFrom, weekTo)
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "SQL Query"

		Private Function GetAllWeeksData(ByVal si As SessionInfo, ByVal year As String, ByVal entity As String, _
										ByVal weekFrom As Integer, ByVal weekTo As Integer) As DataTable
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Build entity filter - handle "HTD" case (empty filter shows all entities)
					Dim entityFilter As String = ""
					If entity <> "HTD" Then
						entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'"
					End If
					
					' Use centralized SQL repository
					Dim sql As String = TRS_SQL_Repository.GetCashDebtWeeklyPositionQuery(year, weekFrom, weekTo, entityFilter)
					
					Dim result As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					
					Return result
				End Using
			Catch ex As Exception

				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Build Weekly TableView"

		Private Function BuildWeeklyTableView(ByVal si As SessionInfo, ByVal weeklyData As DataTable, _
											  ByVal paramEntityId As String, ByVal paramYear As String, _
											  ByVal weekFrom As Integer, ByVal weekTo As Integer) As TableView
			Try
				Dim tv As New TableView()
				tv.CanModifyData = False
				
				' STEP 1: Use weekFrom and weekTo to define range
				Dim minWeek As Integer = weekFrom
				Dim maxWeek As Integer = weekTo
				
				' STEP 2: CRITICAL - Define columns FIRST before adding rows
				' First column: Description
				tv.Columns.Add(New TableViewColumn() With {
					.Name = "Description",
					.Value = $"PARAMS|{paramEntityId}|{paramYear}",
					.IsHeader = True
				})
				
				' Add column for each week in range
				For week As Integer = minWeek To maxWeek
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					tv.Columns.Add(New TableViewColumn() With {
						.Name = weekLabel,
						.Value = weekLabel,
						.IsHeader = True,
						.DataType = XFDataType.Decimal
					})
				Next
				
				' STEP 2.5: Add title row
				Dim titleRow As New TableViewRow() With {.IsHeader = True}
				
				' First column in title row
				titleRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "Weekly Cash Position",
					.IsHeader = True
				})
				
				' Add empty cells to title row
				For week As Integer = minWeek To maxWeek
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					titleRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = "",
						.IsHeader = True
					})
				Next
				
				tv.Rows.Add(titleRow)
				
				' STEP 2.6: Add header row with week labels
				Dim headerRow As New TableViewRow() With {.IsHeader = True}
				
				' First column in header row
				headerRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "Mill EUR",
					.IsHeader = True
				})
				
				' Add week labels to header row
				For week As Integer = minWeek To maxWeek
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					headerRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = weekLabel,
						.IsHeader = True
					})
				Next
				
				tv.Rows.Add(headerRow)
				
				' STEP 3: Build data lookup dictionary (WeekNumber -> Flow -> Amount)
				Dim dataLookup As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
				
				For Each row As DataRow In weeklyData.Rows
					Dim weekNum As Integer = If(IsDBNull(row("WeekNumber")), 0, Convert.ToInt32(row("WeekNumber")))
					Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString())
					Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
					
					If Not dataLookup.ContainsKey(weekNum) Then
						dataLookup(weekNum) = New Dictionary(Of String, Decimal)
					End If
					
					dataLookup(weekNum)(flow) = amount
				Next
				
				' STEP 4: Add data rows (Total Cash, Internal Cash, External Cash)
				
				' Row 1: Total Cash
				Dim totalCashRow As New TableViewRow()
				totalCashRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "Total Cash",
					.IsHeader = False
				})
				
				For week As Integer = minWeek To maxWeek
					Dim internalCash As Decimal = 0
					Dim externalCash As Decimal = 0
					
					If dataLookup.ContainsKey(week) Then
						If dataLookup(week).ContainsKey("INTernal Cash (+)") Then
							internalCash = dataLookup(week)("INTernal Cash (+)")
						End If
						If dataLookup(week).ContainsKey("EXTernal Cash (+)") Then
							externalCash = dataLookup(week)("EXTernal Cash (+)")
						End If
					End If
					
					Dim totalCash As Decimal = internalCash + externalCash
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					
					totalCashRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = totalCash.ToString("F1", CultureInfo.InvariantCulture),
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next
				
				tv.Rows.Add(totalCashRow)
				
				' Row 2: Internal Cash
				Dim internalCashRow As New TableViewRow()
				internalCashRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "Internal Cash",
					.IsHeader = False
				})
				
				For week As Integer = minWeek To maxWeek
					Dim internalCash As Decimal = 0
					
					If dataLookup.ContainsKey(week) Then
						If dataLookup(week).ContainsKey("INTernal Cash (+)") Then
							internalCash = dataLookup(week)("INTernal Cash (+)")
						End If
					End If
					
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					
					internalCashRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = internalCash.ToString("F1", CultureInfo.InvariantCulture),
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next
				
				tv.Rows.Add(internalCashRow)
				
				' Row 3: External Cash
				Dim externalCashRow As New TableViewRow()
				externalCashRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "External Cash",
					.IsHeader = False
				})
				
				For week As Integer = minWeek To maxWeek
					Dim externalCash As Decimal = 0
					
					If dataLookup.ContainsKey(week) Then
						If dataLookup(week).ContainsKey("EXTernal Cash (+)") Then
							externalCash = dataLookup(week)("EXTernal Cash (+)")
						End If
					End If
					
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					
					externalCashRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = externalCash.ToString("F1", CultureInfo.InvariantCulture),
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next
				
				tv.Rows.Add(externalCashRow)

				' Row 4: Total Debt
				Dim totalDebtRow As New TableViewRow()
				totalDebtRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "Total Debt",
					.IsHeader = False
				})
				
				For week As Integer = minWeek To maxWeek
					Dim internalDebt As Decimal = 0
					Dim externalDebt As Decimal = 0
					
					If dataLookup.ContainsKey(week) Then
						If dataLookup(week).ContainsKey("INTernal Debt (-)") Then
							internalDebt = dataLookup(week)("INTernal Debt (-)")
						End If
						If dataLookup(week).ContainsKey("EXTernal Debt (-)") Then
							externalDebt = dataLookup(week)("EXTernal Debt (-)")
						End If
					End If
					
					Dim totalDebt As Decimal = internalDebt + externalDebt
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					
					totalDebtRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = totalDebt.ToString("F1", CultureInfo.InvariantCulture),
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next
				
				tv.Rows.Add(totalDebtRow)
				
				' Row 5: Internal Debt
				Dim internalDebtRow As New TableViewRow()
				internalDebtRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "Internal Debt",
					.IsHeader = False
				})
				
				For week As Integer = minWeek To maxWeek
					Dim internalDebt As Decimal = 0
					
					If dataLookup.ContainsKey(week) Then
						If dataLookup(week).ContainsKey("INTernal Debt (-)") Then
							internalDebt = dataLookup(week)("INTernal Debt (-)")
						End If
					End If
					
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					
					internalDebtRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = internalDebt.ToString("F1", CultureInfo.InvariantCulture),
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next
				
				tv.Rows.Add(internalDebtRow)
				
				' Row 6: External Debt
				Dim externalDebtRow As New TableViewRow()
				externalDebtRow.Items.Add("Description", New TableViewColumn() With {
					.Value = "External Debt",
					.IsHeader = False
				})
				
				For week As Integer = minWeek To maxWeek
					Dim externalDebt As Decimal = 0
					
					If dataLookup.ContainsKey(week) Then
						If dataLookup(week).ContainsKey("EXTernal Debt (-)") Then
							externalDebt = dataLookup(week)("EXTernal Debt (-)")
						End If
					End If
					
					Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"
					
					externalDebtRow.Items.Add(weekLabel, New TableViewColumn() With {
						.Value = externalDebt.ToString("F1", CultureInfo.InvariantCulture),
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next
				
				tv.Rows.Add(externalDebtRow)
				
				' Configure formatting
				tv.HeaderFormat.BackgroundColor = XFColors.Black
				tv.HeaderFormat.IsBold = True
				tv.HeaderFormat.TextColor = XFColors.White
				
				Return tv
				
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		#End Region
		
		#Region "Helper Functions"

		Private Function CreateEmptyTableView(ByVal errorMessage As String) As TableView
			Try
				Dim tv As New TableView()
				tv.CanModifyData = False
				
				tv.Columns.Add(New TableViewColumn() With {.Name = "Error", .Value = "Error", .IsHeader = True})
				
				Dim errorRow As New TableViewRow()
				errorRow.Items.Add("Error", New TableViewColumn() With {.Value = errorMessage, .IsHeader = False})
				tv.Rows.Add(errorRow)
				
				Return tv
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		#End Region

	End Class
End Namespace
