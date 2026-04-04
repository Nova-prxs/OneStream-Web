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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashFlowForecasting4Week
	Public Class MainClass
		'Reference Solution Helper Business Rule
		
		#Region "Constants and Configuration"
		
		' Flow types
		Private Const FLOW_INFLOWS As String = "INFLOWS"
		Private Const FLOW_OUTFLOWS As String = "OUTFLOWS"
		
		' Account types
		Private Const ACCOUNT_OPERATING As String = "OPERATING FLOWS"
		Private Const ACCOUNT_INVESTMENT As String = "INVESTMENT FLOWS"
		Private Const ACCOUNT_FINANCING As String = "FINANCING FLOWS"
		Private Const ACCOUNT_CF_NOT_INCL As String = "CF NOT INCL. IN OP FCF (INCL. DIVIDENDS)"
		Private Const ACCOUNT_OTHER_FLOWS As String = "OTHER FLOWS (DIV)"
		
		' Projection types
		Private Const PROJECTION_ACTUAL As String = "Actual"
		Private Const PROJECTION_FORECAST As String = "Projection"
		
		' Parameter names
		Private Const PARAM_ENTITY As String = "prm_Treasury_CompanyNames"
		Private Const PARAM_YEAR As String = "prm_Treasury_Year"
		Private Const PARAM_WEEK As String = "prm_Treasury_WeekNumber"
		
		' Table names
		Private Const TABLE_MASTER_CASHFLOW As String = "XFC_TRS_Master_CashflowForecasting"
		Private Const TABLE_MASTER_CASHDEBT As String = "XFC_TRS_Master_CashDebtPosition"
		
		' Validation limits
		Private Const MIN_YEAR As Integer = 2020
		Private Const MAX_YEAR As Integer = 2100
		Private Const MIN_WEEK As Integer = 1
		Private Const MAX_WEEK As Integer = 52
		
		' Formatting
		Private Const NUMBER_FORMAT As String = "F2"
		Private Const ZERO_VALUE As String = "0.00"
		
		Private Function FormatDecimalValue(ByVal value As Decimal) As String
			Return If(value = 0, ZERO_VALUE, value.ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture))
		End Function
		
		#End Region
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						Return Nothing
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							list.Add(PARAM_ENTITY)
							list.Add(PARAM_YEAR)
							list.Add(PARAM_WEEK)
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return Me.GetCashFlowForecasting4WeekReport(si, args)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Cash Flow Forecasting Report"

	Private Function GetCashFlowForecasting4WeekReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
			Try
				' Validate input parameters
				If args Is Nothing OrElse args.CustSubstVarsAlreadyResolved Is Nothing Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: No parameters provided")
				End If
				
				' Get parameters from spreadsheet args - NO DEFAULTS
				Dim paramEntityId As String = ""
				Dim paramYear As String = ""
				Dim paramWeek As String = ""
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_CompanyNames") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")) Then
					paramEntityId = args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")) Then
					paramYear = args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")) Then
					paramWeek = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")
				End If
				
				' Validate required parameters
				If String.IsNullOrEmpty(paramEntityId) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Company parameter is missing")
				End If
				
				If String.IsNullOrEmpty(paramYear) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Year parameter is missing")
				End If
				
				If String.IsNullOrEmpty(paramWeek) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Week parameter is missing")
				End If
				
				' Convert company ID to company name for database filtering
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
				
				' Calculate TimeKey using the same logic as in import templates
				Dim timeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear, paramWeek)
				
				' Validate paramYear
				Dim yearInt As Integer
				If Not Integer.TryParse(paramYear, yearInt) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Invalid Year '{paramYear}'. Must be a valid year.")
				End If
				
				If yearInt < MIN_YEAR OrElse yearInt > MAX_YEAR Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Year {yearInt} is out of valid range ({MIN_YEAR}-{MAX_YEAR}).")
				End If
				
				' Validate paramWeek
				Dim currentWeek As Integer
				If Not Integer.TryParse(paramWeek, currentWeek) OrElse currentWeek < MIN_WEEK OrElse currentWeek > MAX_WEEK Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Invalid week number '{paramWeek}'. Must be between {MIN_WEEK} and {MAX_WEEK}.")
				End If
				
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
						' Build WHERE clause - handle "HTD" case for entity filtering
						Dim entityFilter As String = ""
						If paramEntity <> "HTD" Then
							entityFilter = $"AND Entity = '{paramEntity}'"
						End If
						
						' Create the dynamic TableView (READ-ONLY)
						Dim tv As New TableView()
						tv.CanModifyData = False			' Step 1: Add columns dynamically - First column for descriptions
			tv.Columns.Add(New TableViewColumn() With {.Name = "Description", .Value = "Million of EUR", .IsHeader = True})
			
			' NUEVA LÓGICA: Solo 5 semanas
			' - 1 semana Actual: semana anterior (currentWeek - 1)
			' - 4 semanas Forecast: currentWeek, currentWeek+1, currentWeek+2, currentWeek+3
			Dim totalWeeks As New List(Of Dictionary(Of String, Object))
			Dim currentYearInt As Integer = Integer.Parse(paramYear)
			
			' Calculate previous week (for Actual data)
			Dim previousWeek As Integer = currentWeek - 1
			Dim previousYear As Integer = currentYearInt
			
			' Handle year boundary (if currentWeek = 1, previous week is week 52 of previous year)
			If currentWeek = 1 Then
				previousWeek = 52
				previousYear = currentYearInt - 1
			End If
			
			' Add 1 Actual week (previous week)
			totalWeeks.Add(New Dictionary(Of String, Object) From {
				{"Year", previousYear},
				{"Week", previousWeek},
				{"Type", "Actual"}
			})
			
			' Add 4 Forecast weeks (current week + next 3 weeks)
			For i As Integer = 0 To 3
				Dim forecastWeek As Integer = currentWeek + i
				Dim forecastYear As Integer = currentYearInt
				
				' Handle year boundary (if forecast week > 52, move to next year)
				If forecastWeek > 52 Then
					forecastWeek = forecastWeek - 52
					forecastYear = currentYearInt + 1
				End If
				
				totalWeeks.Add(New Dictionary(Of String, Object) From {
					{"Year", forecastYear},
					{"Week", forecastWeek},
					{"Type", "Forecast"}
				})
			Next				' Add all week columns
				' NOTE: DataType will be Decimal for data rows, but parameter row cells will override to Text
				For i As Integer = 0 To totalWeeks.Count - 1
				    Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
				    Dim weekYear As Integer = CInt(weekInfo("Year"))
				    Dim weekNum As Integer = CInt(weekInfo("Week"))
				    Dim columnName As String = $"Week{i + 1}"
				    Dim columnLabel As String = If(weekYear = currentYearInt, $"Week {weekNum}", $"Week {weekNum} ({weekYear})")
				    tv.Columns.Add(New TableViewColumn() With {
				    	.Name = columnName, 
				    	.Value = columnLabel, 
				    	.IsHeader = True,
				    	.DataType = XFDataType.Decimal
				    })
				Next
				
				' Add CashDebtActual column at the end
				tv.Columns.Add(New TableViewColumn() With {.Name = "CashDebtActual", .Value = "CashDebtActual", .IsHeader = True, .DataType = XFDataType.Decimal})
				
				' Step 2: Add 3-level headers
					
				' Header Level 1: Actual/Forecast
				Dim actualForecastRow As New TableViewRow() With {.IsHeader = True}
				actualForecastRow.Items.Add("Description", New TableViewColumn() With {.Value = "Cashflow Monitoring", .IsHeader = True})
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
					Dim weekType As String = weekInfo("Type").ToString()
					Dim columnName As String = $"Week{i + 1}"
					actualForecastRow.Items.Add(columnName, New TableViewColumn() With {.Value = weekType, .IsHeader = True})
				Next
				' Add empty cell for CashDebtActual in this header row
				actualForecastRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(actualForecastRow)				' Header Level 2: Week numbers
				Dim weekNumberRow As New TableViewRow() With {.IsHeader = True}
				weekNumberRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
					Dim weekNum As Integer = CInt(weekInfo("Week"))
					Dim columnName As String = $"Week{i + 1}"
					weekNumberRow.Items.Add(columnName, New TableViewColumn() With {.Value = $"Week {weekNum}", .IsHeader = True})
				Next
				' Add empty cell for CashDebtActual in this header row
				weekNumberRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(weekNumberRow)				' Header Level 3: Monday dates (day-month format)
				Dim mondayDateRow As New TableViewRow() With {.IsHeader = True}
				mondayDateRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
					Dim weekYear As Integer = CInt(weekInfo("Year"))
					Dim weekNum As Integer = CInt(weekInfo("Week"))
					Dim columnName As String = $"Week{i + 1}"
					Dim mondayDate As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMondayDateForWeek(si, weekYear.ToString(), weekNum.ToString())
					mondayDateRow.Items.Add(columnName, New TableViewColumn() With {.Value = mondayDate, .IsHeader = True})
				Next
				' Add CashDebtActual header
				mondayDateRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "Cash Available", .IsHeader = True})
				tv.Rows.Add(mondayDateRow)
				
			' Step 3: Get individual flow data for 5 weeks (1 Actual + 4 Forecast)
			' SOLO Operating Flows e Investment Flows
			' LOGIC: 
			' - ACTUAL week (previous week): Use ProjectionType='Actual' from data uploaded in CURRENT week
			' - FORECAST weeks (4 weeks): Use ProjectionType='Projection' from data uploaded in CURRENT week
			
			' Build list of years we need to query
			Dim yearsToQuery As New List(Of Integer)
			For Each weekInfo As Dictionary(Of String, Object) In totalWeeks
				Dim weekYear As Integer = CInt(weekInfo("Year"))
				If Not yearsToQuery.Contains(weekYear) Then
					yearsToQuery.Add(weekYear)
				End If
			Next
			Dim yearsList As String = String.Join(",", yearsToQuery)
			
			' Build WHERE conditions for the 5 specific weeks
			Dim weekConditions As New List(Of String)
			For Each weekInfo As Dictionary(Of String, Object) In totalWeeks
				Dim weekYear As Integer = CInt(weekInfo("Year"))
				Dim weekNum As Integer = CInt(weekInfo("Week"))
				Dim weekType As String = weekInfo("Type").ToString()
				
				If weekType = "Actual" Then
					' Actual week: Get 'Actual' data uploaded in CURRENT week for PREVIOUS week
					weekConditions.Add($"(ProjectionType = 'Actual' AND UploadYear = {paramYear} AND UploadWeekNumber = {currentWeek} AND ProjectionYear = {weekYear} AND ProjectionWeekNumber = {weekNum})")
				Else
					' Forecast weeks: Get 'Projection' data uploaded in CURRENT week
					weekConditions.Add($"(ProjectionType = 'Projection' AND UploadYear = {paramYear} AND UploadWeekNumber = {currentWeek} AND ProjectionYear = {weekYear} AND ProjectionWeekNumber = {weekNum})")
				End If
			Next
			
			Dim weekConditionsStr As String = String.Join(" OR ", weekConditions)
			
			Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowEntity4WeekQuery(yearsList, entityFilter, weekConditionsStr)					
			Dim allDataDt As DataTable = BRApi.Database.ExecuteSql(dbConn, allDataSql, False)

			' Define accounts to show (only Operating and Investment)
			Dim accountsToShow As String() = {ACCOUNT_OPERATING, ACCOUNT_INVESTMENT}
			
			' Build flow data dictionary: Account|Flow|Year|WeekNumber -> Value
			' Key includes Year to support cross-year data
			Dim flowData As New Dictionary(Of String, Decimal)
			
			For Each row As DataRow In allDataDt.Rows
				Dim account As String = row("Account").ToString().ToUpper()
				Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString().ToUpper())
				Dim projYear As Integer = If(IsDBNull(row("ProjectionYear")), 0, Convert.ToInt32(row("ProjectionYear")))
				Dim weekNum As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
				Dim weekValue As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
				
				' Create key with Year: Account|Flow|Year|WeekNumber
				Dim key As String = $"{account}|{flow}|{projYear}|{weekNum}"
				
				' Store the value directly
				flowData(key) = weekValue
			Next					' Step 4: Add data rows for each Account type with sub-rows for Inflows/Outflows
					' Use accountsToShow (only Operating and Investment Flows)
					
				For Each account As String In accountsToShow
					' Create gray header row for this account type with calculated totals
					Dim accountRow As New TableViewRow() With {.IsHeader = False}
					accountRow.Items.Add("Description", New TableViewColumn() With {.Value = account, .IsHeader = False})					' Calculate totals for each week column (sum of Inflows + Outflows)
					For i As Integer = 0 To totalWeeks.Count - 1
						Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
						Dim weekYear As Integer = CInt(weekInfo("Year"))
						Dim weekNum As Integer = CInt(weekInfo("Week"))
						Dim columnName As String = $"Week{i + 1}"
						Dim totalValue As Decimal = 0
						
						' Sum Inflows and Outflows for this account
						Dim inflowsKey As String = $"{account.ToUpper()}|{FLOW_INFLOWS}|{weekYear}|{weekNum}"
						Dim outflowsKey As String = $"{account.ToUpper()}|{FLOW_OUTFLOWS}|{weekYear}|{weekNum}"
						
						Dim inflowValue As Decimal = 0
						Dim outflowValue As Decimal = 0
						
						' Use TryGetValue for better performance
						flowData.TryGetValue(inflowsKey, inflowValue)
						flowData.TryGetValue(outflowsKey, outflowValue)
						
						totalValue += inflowValue + outflowValue
						
						Dim formattedTotal As String = Me.FormatDecimalValue(totalValue)
						accountRow.Items.Add(columnName, New TableViewColumn() With {.Value = formattedTotal, .IsHeader = False, .DataType = XFDataType.Decimal})
					Next						' Add empty CashDebtActual cell to account header rows
						accountRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
						
						tv.Rows.Add(accountRow)
						
						' Define flows for this account (Other Flows only has Inflows)
						Dim flowsForAccount As String()
						If account = ACCOUNT_OTHER_FLOWS Then
							flowsForAccount = {FLOW_INFLOWS}
						Else
							flowsForAccount = {FLOW_INFLOWS, FLOW_OUTFLOWS}
						End If
						
					' Add sub-rows for each flow type
					For Each flowType As String In flowsForAccount
						Dim flowRow As New TableViewRow()
						' Use flowType as visible value, but store account info in a way we can retrieve it
						flowRow.Items.Add("Description", New TableViewColumn() With {.Value = flowType, .IsHeader = False})				' Add data for each week column
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
					Dim weekYear As Integer = CInt(weekInfo("Year"))
					Dim weekNum As Integer = CInt(weekInfo("Week"))
					Dim columnName As String = $"Week{i + 1}"
					
					' Use key with year: Account|Flow|Year|WeekNumber
					Dim lookupKey As String = $"{account.ToUpper()}|{flowType.ToUpper()}|{weekYear}|{weekNum}"
					
				Dim flowValue As Decimal = 0
				flowData.TryGetValue(lookupKey, flowValue)
				
				Dim formattedValue As String = Me.FormatDecimalValue(flowValue)
				
				Dim weekColumn As New TableViewColumn() With {
					.Value = formattedValue,
					.IsHeader = False,
					.DataType = XFDataType.Decimal
				}					
				flowRow.Items.Add(columnName, weekColumn)
				Next							' Add empty CashDebtActual cell to flow data rows
							flowRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
							
						tv.Rows.Add(flowRow)
					Next
				Next
				
				' Note: No summary rows - only individual account flows
				' Note: No formatting applied to maintain user's Excel formatting preferences					
				Return tv
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
	End Class
End Namespace
