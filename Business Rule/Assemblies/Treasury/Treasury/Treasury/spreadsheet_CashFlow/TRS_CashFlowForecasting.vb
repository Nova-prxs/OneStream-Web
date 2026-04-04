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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashFlowForecasting
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
		Private Const MAX_WEEK As Integer = 53  ' Week 53 = Week 1 of next year (for viewing full year as Actual)
		
		' Row names
		Private Const ROW_TOTAL_GENERAL As String = "Total General"
		Private Const ROW_ACCUMULATED_ALL As String = "Accumulated All Cashflow + Cash Balance"
		Private Const ROW_ACCUMULATED_OPERINV As String = "Accumulated Oper Inv Cashflow"
		Private Const ROW_OPENING_CASH As String = "Opening Cash"
		Private Const ROW_CLOSING_CASH As String = "Closing Cash"
		
		' Formatting
		Private Const NUMBER_FORMAT As String = "F2"
		Private Const ZERO_VALUE As String = "0.00"
		
		Private Function GetAccountOrder() As String()
			Return {
				ACCOUNT_OPERATING,
				ACCOUNT_INVESTMENT,
				ACCOUNT_FINANCING,
				ACCOUNT_CF_NOT_INCL,
				ACCOUNT_OTHER_FLOWS
			}
		End Function
		
		Private Function GetOperInvAccounts() As String()
			Return {ACCOUNT_OPERATING, ACCOUNT_INVESTMENT}
		End Function
		
		Private Function FormatDecimalValue(ByVal value As Decimal) As String
			Return If(value = 0, ZERO_VALUE, value.ToString(NUMBER_FORMAT, CultureInfo.InvariantCulture))
		End Function
		
		#End Region
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						
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
						Return Me.GetCashFlowForecastingReport(si, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						Try
							Dim saveResult As Boolean = Me.SaveCashFlowForecastChanges(si, args.TableView)
							Return saveResult
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Cash Flow Forecasting Report"

	Private Function GetYearAccumulatedValues(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal paramEntity As String, ByVal targetYear As String) As Dictionary(Of String, Decimal)
		Try
			Dim result As New Dictionary(Of String, Decimal)
			result("AccumulatedAll") = 0
			result("AccumulatedOperInv") = 0
			
			' Validate year
			Dim yearInt As Integer
			If Not Integer.TryParse(targetYear, yearInt) Then
				Return result
			End If
			
			' Build entity filter
			Dim entityFilter As String = ""
			If paramEntity <> "HTD" Then
				entityFilter = $"AND Entity = '{paramEntity}'"
			End If
			
			' Query: Get all data for weeks 1-52 of target year
			Dim yearSql As String = TRS_SQL_Repository.GetCashFlowYearAccumulatedQuery(targetYear, entityFilter)
			
			Dim yearDt As DataTable = BRApi.Database.ExecuteSql(dbConn, yearSql, False)
			
			If yearDt.Rows.Count = 0 Then
				Return result
			End If
			
			' Build optimized flow data dictionary: Account|Flow|Year|Week -> Amount
			Dim flowData As New Dictionary(Of String, Decimal)
			
			For Each row As DataRow In yearDt.Rows
				Dim account As String = row("Account").ToString().ToUpper()
				Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString().ToUpper())
				Dim projYear As Integer = If(IsDBNull(row("ProjectionYear")), 0, Convert.ToInt32(row("ProjectionYear")))
				Dim weekNum As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
				Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
				
				' Key format: Account|Flow|Year|Week
				Dim key As String = $"{account}|{flow}|{projYear}|{weekNum}"
				flowData(key) = amount
			Next
			
		' Get account order
		Dim accountOrder As String() = Me.GetAccountOrder()
		Dim operInvAccounts As String() = Me.GetOperInvAccounts()
		
		' Calculate accumulated values for THIS YEAR ONLY (weeks 1-52)
		' NO recursion - just sum all weeks of the target year
		Dim accumulatedAll As Decimal = 0
		Dim accumulatedOperInv As Decimal = 0
		
		For weekNum As Integer = MIN_WEEK To MAX_WEEK
			Dim weekTotal As Decimal = 0
			Dim weekOperInvTotal As Decimal = 0
			
			' Sum all accounts for this week using optimized TryGetValue
			For Each account As String In accountOrder
				Dim inflowsKey As String = $"{account}|{FLOW_INFLOWS}|{targetYear}|{weekNum}"
				Dim outflowsKey As String = $"{account}|{FLOW_OUTFLOWS}|{targetYear}|{weekNum}"
				
				Dim inflowValue As Decimal = 0
				Dim outflowValue As Decimal = 0
				
				' Use TryGetValue for better performance (single lookup instead of ContainsKey + access)
				flowData.TryGetValue(inflowsKey, inflowValue)
				flowData.TryGetValue(outflowsKey, outflowValue)
				
				weekTotal += inflowValue + outflowValue
				
				' Add to OperInv total if applicable
				If operInvAccounts.Contains(account) Then
					weekOperInvTotal += inflowValue + outflowValue
				End If
			Next
			
			' Accumulate (add to previous year's carryover)
			accumulatedAll += weekTotal
			accumulatedOperInv += weekOperInvTotal
		Next
		
		result("AccumulatedAll") = accumulatedAll
		result("AccumulatedOperInv") = accumulatedOperInv
			
			Return result
			
		Catch ex As Exception
			Return New Dictionary(Of String, Decimal) From {
				{"AccumulatedAll", 0},
				{"AccumulatedOperInv", 0}
			}
		End Try
	End Function

	''' <summary>
	''' Gets the Closing Cash from a specific year (W52) using only Actual data.
	''' RECURSIVE: Closing Cash = Opening Cash (from previous year) + Sum of all flows for the year
	''' This ensures proper carryover across multiple years.
	''' </summary>
	''' <param name="si">Session info</param>
	''' <param name="dbConn">Database connection</param>
	''' <param name="targetYear">The year to get the closing cash from (e.g., 2024)</param>
	''' <param name="entityFilter">Entity filter clause (e.g., "AND Entity = 'HORSE SPAIN'")</param>
	''' <param name="baseYear">The base year where Opening Cash = 0 (default: 2024, first year with data)</param>
	''' <returns>The closing cash value (Opening Cash + sum of all flows for the entire year)</returns>
	Private Function GetPreviousYearClosingCash(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal targetYear As Integer, ByVal entityFilter As String, Optional ByVal baseYear As Integer = 2024) As Decimal
		Try
			' BASE CASE: If targetYear is before baseYear, return 0 (no data exists)
			If targetYear < baseYear Then
				Return 0
			End If
			
			' STEP 1: Get the Opening Cash for this year
			' Opening Cash = Closing Cash of the previous year (RECURSIVE)
			Dim openingCash As Decimal = 0
			If targetYear > baseYear Then
				' Recursively get previous year's closing cash
				openingCash = GetPreviousYearClosingCash(si, dbConn, targetYear - 1, entityFilter, baseYear)
			End If
			' If targetYear = baseYear, openingCash remains 0 (first year starts at 0)
			
			' STEP 2: Get the sum of all Actual flows for this year (W1-W52)
			Dim yearFlowsSql As String = $"
				SELECT ISNULL(SUM(Amount), 0) AS YearFlows
				FROM {TABLE_MASTER_CASHFLOW}
				WHERE ProjectionYear = {targetYear}
				AND ProjectionWeekNumber BETWEEN 1 AND 52
				AND ProjectionType = 'Actual'
				AND UPPER(Account) IN ('{ACCOUNT_OPERATING}', '{ACCOUNT_INVESTMENT}', '{ACCOUNT_FINANCING}', '{ACCOUNT_CF_NOT_INCL}', '{ACCOUNT_OTHER_FLOWS}')
				{entityFilter}
			"
			
			Dim resultDt As DataTable = BRApi.Database.ExecuteSql(dbConn, yearFlowsSql, False)
			
			Dim yearFlows As Decimal = 0
			If resultDt.Rows.Count > 0 AndAlso Not IsDBNull(resultDt.Rows(0)("YearFlows")) Then
				yearFlows = Convert.ToDecimal(resultDt.Rows(0)("YearFlows"))
			End If
			
			' STEP 3: Closing Cash = Opening Cash + Year Flows
			Dim closingCash As Decimal = openingCash + yearFlows
			
			Return closingCash
			
		Catch ex As Exception
			' If any error occurs, return 0 to avoid breaking the report
			Return 0
		End Try
	End Function

	Private Function GetCashFlowForecastingReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
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
					
					' Build WHERE clause - handle HTD case for entity filtering
					Dim entityFilter As String = ""
					If paramEntity <> "HTD" Then
						entityFilter = $"AND Entity = '{paramEntity}'"
					End If
					
					' Create the dynamic TableView
					Dim tv As New TableView()
					tv.CanModifyData = True
					
			' Step 1: Add columns dynamically - First column for descriptions
			tv.Columns.Add(New TableViewColumn() With {.Name = "Description", .Value = "Million of EUR", .IsHeader = True})
			
			' Calculate total weeks to display with cross-year support
			' LOGIC: 
			' - If currentWeek = 53: Show all 52 weeks of paramYear as Actual + W1 of next year as Forecast
			' - Otherwise: Show weeks 1 to currentWeek-1 as Actual, currentWeek to 52 as Forecast
			' - Extension: If 8-week forecast extends into next year, add those next year weeks too
			Dim totalWeeks As New List(Of Dictionary(Of String, Object))
			Dim currentYearInt As Integer = Integer.Parse(paramYear)
			
			' CROSS-YEAR SUPPORT: Week 53 means viewing from W1 of next year
			' This allows showing ALL 52 weeks of paramYear as Actual
			Dim displayYear As Integer = currentYearInt
			Dim effectiveCurrentWeek As Integer = currentWeek
			
			If currentWeek = 53 Then
				' Week 53 = W1 of next year, so all 52 weeks of paramYear are Actual
				effectiveCurrentWeek = 53 ' This means W1-W52 of paramYear are all Actual
			End If
			
			' Add Actual weeks (1 to effectiveCurrentWeek - 1) - all in paramYear
			' If effectiveCurrentWeek = 53, this adds W1-W52 as Actual
			Dim actualWeeksEnd As Integer = Math.Min(effectiveCurrentWeek - 1, 52)
			For i As Integer = 1 To actualWeeksEnd
			    totalWeeks.Add(New Dictionary(Of String, Object) From {
			        {"Year", currentYearInt},
			        {"Week", i},
			        {"Type", "Actual"}
			    })
			Next
			
			' Add Forecast weeks
			If currentWeek = 53 Then
				' Week 53 selected: Forecast starts at W1 of next year
				Dim forecastWeeksToShow As Integer = 8
				For i As Integer = 1 To forecastWeeksToShow
				    totalWeeks.Add(New Dictionary(Of String, Object) From {
				        {"Year", currentYearInt + 1},
				        {"Week", i},
				        {"Type", "Forecast"}
				    })
				Next
			Else
				' Normal case: Forecast from currentWeek to week 52 of current year
				For i As Integer = currentWeek To 52
				    totalWeeks.Add(New Dictionary(Of String, Object) From {
				        {"Year", currentYearInt},
				        {"Week", i},
				        {"Type", "Forecast"}
				    })
				Next
				
				' Check if 8-week forecast extends into next year
				' If currentWeek + 8 > 52, we need to add weeks from next year
				Dim forecastWeeksToShow As Integer = 8
				Dim lastForecastWeek As Integer = currentWeek + forecastWeeksToShow - 1
				
				If lastForecastWeek > 52 Then
				    ' Calculate how many weeks spill into next year
				    Dim nextYearWeeks As Integer = lastForecastWeek - 52
				    
				    ' Add those weeks from next year
				    For i As Integer = 1 To nextYearWeeks
				        totalWeeks.Add(New Dictionary(Of String, Object) From {
				            {"Year", currentYearInt + 1},
				            {"Week", i},
				            {"Type", "Forecast"}
				        })
				    Next
				End If
			End If				' Add all week columns
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
				
			' *** SOLUCIÓN: Guardar todos los parámetros en UNA SOLA celda serializada ***
			' Esto permite que todas las columnas de semana tengan DataType.Decimal limpio
			Dim paramRow As New TableViewRow() With {.IsHeader = False}
			
			' Columna de descripción: Serializar parámetros + metadata de semanas en formato comprimido
			' Formato: PARAMS|entity|year|week|totalWeeks|[y1:w1,y2:w2,...]
			Dim weekMetadata As New System.Text.StringBuilder()
			For i As Integer = 0 To totalWeeks.Count - 1
			    Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
			    Dim weekYear As Integer = CInt(weekInfo("Year"))
			    Dim weekNum As Integer = CInt(weekInfo("Week"))
			    If i > 0 Then weekMetadata.Append(",")
			    weekMetadata.Append($"{weekYear}:{weekNum}")
			Next
			
			Dim paramValue As String = $"PARAMS|{paramEntityId}|{paramYear}|{paramWeek}|{totalWeeks.Count}|{weekMetadata.ToString()}"
			
			paramRow.Items.Add("Description", New TableViewColumn() With {
				.Value = paramValue,
				.IsHeader = False
			})
			
			' ✅ CRITICAL: Llenar columnas de semana con valores vacíos Y DataType.Decimal
			' Ahora SIN parámetros en las celdas = formato limpio para números
			For i As Integer = 0 To totalWeeks.Count - 1
			    Dim columnName As String = $"Week{i + 1}"
			    paramRow.Items.Add(columnName, New TableViewColumn() With {
			        .Value = "",
			        .IsHeader = False,
			        .DataType = XFDataType.Decimal
			    })
			Next					' Add empty CashDebtActual cell to param row
					paramRow.Items.Add("CashDebtActual", New TableViewColumn() With {
					    .Value = "",
					    .IsHeader = False
					})
					
					tv.Rows.Add(paramRow)
					
					' Step 2: Add 3-level headers (ahora comienzan los headers reales)
					
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
				tv.Rows.Add(mondayDateRow)					' *** GET CASH AVAILABLE VALUE from CashDebtPosition (normalized Master table) ***
					Dim cashAvailableValue As Decimal = 0
					
					Try
						' CRITICAL: CashDebtActual debe buscar en la SEMANA ANTERIOR
						' Si estoy en semana 52, busco semana 51
						' Si estoy en semana 13, busco semana 12
						Dim previousWeek As Integer = currentWeek - 1
						Dim previousYear As String = paramYear
						
						' Si estamos en semana 1, buscar semana 52 del año anterior
						If currentWeek = 1 Then
							previousWeek = 52
							previousYear = (currentYearInt - 1).ToString()
						End If
						
						Dim previousTimeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, previousYear, previousWeek.ToString())
						Dim expectedScenario As String = "FW" + previousWeek.ToString().PadLeft(2, "0"c)
						
						Dim cashDebtSql As String = $"
							SELECT SUM(Amount) AS CashAvailable
							FROM {TABLE_MASTER_CASHDEBT} 
							WHERE UploadTimekey = '{previousTimeKey}' 
							{entityFilter}
							AND Scenario = '{expectedScenario}'
							AND ProjectionType = 'StartWeek'
							AND Account = 'CASH AND FINANCING BALANCE'
						"
						
						Dim cashDebtDt As DataTable = BRApi.Database.ExecuteSql(dbConn, cashDebtSql, False)
						
						If cashDebtDt.Rows.Count > 0 AndAlso Not IsDBNull(cashDebtDt.Rows(0)("CashAvailable")) Then
							cashAvailableValue = Convert.ToDecimal(cashDebtDt.Rows(0)("CashAvailable"))
						End If
						
			Catch ex As Exception
			End Try				' *** ADD DATA ROW with CashDebtActual value ***
				Dim cashDebtActualDataRow As New TableViewRow() With {.IsHeader = False}
				cashDebtActualDataRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = False})
				
				' Add empty cells for all week columns
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim columnName As String = $"Week{i + 1}"
					cashDebtActualDataRow.Items.Add(columnName, New TableViewColumn() With {.Value = "", .IsHeader = False, .DataType = XFDataType.Decimal})
				Next					' Add the actual value in CashDebtActual column
					Dim formattedCashValue As String = Me.FormatDecimalValue(cashAvailableValue)
					cashDebtActualDataRow.Items.Add("CashDebtActual", New TableViewColumn() With {
						.Value = formattedCashValue,
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
					
					tv.Rows.Add(cashDebtActualDataRow)
					
			' Step 3: Get individual flow data supporting cross-year projections
			' LOGIC: 
			' - ACTUAL weeks: Use ProjectionType='Actual' from data uploaded in the NEXT week
			' - FORECAST weeks: Use ProjectionType='Projection' from data uploaded in CURRENT week
			'   (can span into next year for weeks 51-52 with 8-week forecast)
			
			' Build list of years we need to query
			Dim yearsToQuery As New List(Of Integer)
			For Each weekInfo As Dictionary(Of String, Object) In totalWeeks
				Dim weekYear As Integer = CInt(weekInfo("Year"))
				If Not yearsToQuery.Contains(weekYear) Then
					yearsToQuery.Add(weekYear)
				End If
			Next
			Dim yearsList As String = String.Join(",", yearsToQuery)
			
			Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowEntityFullYearQuery(yearsList, paramYear, currentWeek, currentYearInt + 1, entityFilter)					
			Dim allDataDt As DataTable = BRApi.Database.ExecuteSql(dbConn, allDataSql, False)

			
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
					Dim accountOrder As String() = Me.GetAccountOrder()
					
				For Each account As String In accountOrder
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
					
					' ====================================
					' STEP 5: ADD SUMMARY ROWS
					' ====================================
					
					' Storage for cumulative calculations
					Dim accumulatedAllCashflow As New Dictionary(Of Integer, Decimal)
					Dim accumulatedOperInv As New Dictionary(Of Integer, Decimal)
					
				' Initialize accumulated values - ONLY for current year (paramYear)
				' No recursive calculation from previous years
				Dim initialAccumulatedAll As Decimal = 0
				Dim initialAccumulatedOperInv As Decimal = 0			' Step 2: Add current year's weeks 1 to (currentWeek - 1) to the accumulated values
					' This ensures continuity: if viewing from Week 13, we include Weeks 1-12 of current year
					Dim firstWeekInfo As Dictionary(Of String, Object) = totalWeeks(0)
					Dim firstWeekNum As Integer = CInt(firstWeekInfo("Week"))
					
					' Declare operInvAccounts here for use in recursive accumulation if needed
					Dim operInvAccounts As String() = Me.GetOperInvAccounts()
					
					If firstWeekNum > 1 Then
						' We're NOT starting from Week 1, so we need to add weeks 1 to (firstWeekNum - 1)
						
						' Get data for weeks 1 to (firstWeekNum - 1) of current year
						Dim currentYearPreviousWeeksSql As String = $"
							SELECT Account, Flow, ProjectionYear, ProjectionWeekNumber, Amount
							FROM {TABLE_MASTER_CASHFLOW} 
							WHERE UploadYear = {paramYear}
							{entityFilter}
							AND UPPER(Account) IN ('{ACCOUNT_OPERATING}', '{ACCOUNT_INVESTMENT}', '{ACCOUNT_FINANCING}', '{ACCOUNT_CF_NOT_INCL}', '{ACCOUNT_OTHER_FLOWS}')
							AND ProjectionType IN ('Actual', 'Projection')
							AND ProjectionYear = {paramYear}
							AND ProjectionWeekNumber BETWEEN 1 AND {firstWeekNum - 1}
							ORDER BY ProjectionWeekNumber
						"
						
						Dim currentYearPrevDt As DataTable = BRApi.Database.ExecuteSql(dbConn, currentYearPreviousWeeksSql, False)
						
						' Build flow data for previous weeks of current year
						Dim currentYearPrevFlowData As New Dictionary(Of String, Decimal)
						For Each row As DataRow In currentYearPrevDt.Rows
							Dim account As String = row("Account").ToString().ToUpper()
							Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString().ToUpper())
							Dim projYear As Integer = If(IsDBNull(row("ProjectionYear")), currentYearInt, Convert.ToInt32(row("ProjectionYear")))
							Dim weekNum As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
							Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
							
							Dim key As String = $"{account}|{flow}|{projYear}|{weekNum}"
							currentYearPrevFlowData(key) = amount
						Next
						
						' Calculate accumulated values for weeks 1 to (firstWeekNum - 1)
						' Note: accountOrder and operInvAccounts already declared in outer scope
						
						For weekNum As Integer = 1 To (firstWeekNum - 1)
							Dim weekTotal As Decimal = 0
							Dim weekOperInvTotal As Decimal = 0
							
							' Sum all accounts for this week
							For Each account As String In accountOrder
								Dim inflowsKey As String = $"{account}|{FLOW_INFLOWS}|{paramYear}|{weekNum}"
								Dim outflowsKey As String = $"{account}|{FLOW_OUTFLOWS}|{paramYear}|{weekNum}"
								
								Dim inflowValue As Decimal = 0
								Dim outflowValue As Decimal = 0
								
								currentYearPrevFlowData.TryGetValue(inflowsKey, inflowValue)
								currentYearPrevFlowData.TryGetValue(outflowsKey, outflowValue)
								
								weekTotal += inflowValue + outflowValue
								
								' Add to OperInv total if applicable
								If operInvAccounts.Contains(account) Then
									weekOperInvTotal += inflowValue + outflowValue
								End If
							Next
							
							' Accumulate
							initialAccumulatedAll += weekTotal
							initialAccumulatedOperInv += weekOperInvTotal
					Next
				End If				' Note: Do NOT pre-initialize accumulatedAllCashflow(0) or accumulatedOperInv(0)
				' The recursive initialization will be applied in the first iteration of the loop (i=0)
				' by using initialAccumulatedAll/initialAccumulatedOperInv as previousWeekAccumulated
				
				' ROW 1: Total General (sum of all main flows for each week)
				Dim totalGeneralRow As New TableViewRow() With {.IsHeader = False}
				totalGeneralRow.Items.Add("Description", New TableViewColumn() With {.Value = ROW_TOTAL_GENERAL, .IsHeader = False})
				
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
					Dim weekYear As Integer = CInt(weekInfo("Year"))
					Dim weekNum As Integer = CInt(weekInfo("Week"))
					Dim columnName As String = $"Week{i + 1}"
					Dim weekTotal As Decimal = 0
					
					' Sum all main account flows
					For Each account As String In accountOrder
						Dim inflowsKey As String = $"{account.ToUpper()}|{FLOW_INFLOWS}|{weekYear}|{weekNum}"
						Dim outflowsKey As String = $"{account.ToUpper()}|{FLOW_OUTFLOWS}|{weekYear}|{weekNum}"
						
						Dim inflowValue As Decimal = 0
						Dim outflowValue As Decimal = 0
						
						flowData.TryGetValue(inflowsKey, inflowValue)
					flowData.TryGetValue(outflowsKey, outflowValue)
					
					weekTotal += inflowValue + outflowValue
				Next
			
			Dim formattedTotal As String = Me.FormatDecimalValue(weekTotal)
			totalGeneralRow.Items.Add(columnName, New TableViewColumn() With {.Value = formattedTotal, .IsHeader = False, .DataType = XFDataType.Decimal})
		Next' Add empty CashDebtActual cell to Total General row
		totalGeneralRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
		
		tv.Rows.Add(totalGeneralRow)
		
				' ROW 2: Accumulated All Cashflow + Cash Balance (cumulative sum)
				Dim accumulatedAllRow As New TableViewRow() With {.IsHeader = False}
				accumulatedAllRow.Items.Add("Description", New TableViewColumn() With {.Value = ROW_ACCUMULATED_ALL, .IsHeader = False})		
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
					Dim weekYear As Integer = CInt(weekInfo("Year"))
					Dim weekNum As Integer = CInt(weekInfo("Week"))
					Dim columnName As String = $"Week{i + 1}"
					Dim weekTotal As Decimal = 0
					
					' Get current week's Total General
					For Each account As String In accountOrder
						Dim inflowsKey As String = $"{account.ToUpper()}|{FLOW_INFLOWS}|{weekYear}|{weekNum}"
						Dim outflowsKey As String = $"{account.ToUpper()}|{FLOW_OUTFLOWS}|{weekYear}|{weekNum}"
						
						Dim inflowValue As Decimal = 0
						Dim outflowValue As Decimal = 0
						
						flowData.TryGetValue(inflowsKey, inflowValue)
						flowData.TryGetValue(outflowsKey, outflowValue)
						
						weekTotal += inflowValue + outflowValue
					Next
					
				' Add previous week's accumulated value
				Dim previousWeekAccumulated As Decimal = 0
				If i > 0 Then
					previousWeekAccumulated = accumulatedAllCashflow(i - 1)
				Else
					' First displayed week: start from initialization (includes current year weeks 1 to firstWeek-1 only)
					previousWeekAccumulated = initialAccumulatedAll
				End If
				
			Dim currentAccumulated As Decimal = previousWeekAccumulated + weekTotal
			accumulatedAllCashflow(i) = currentAccumulated
			
			Dim formattedAccumulated As String = Me.FormatDecimalValue(currentAccumulated)
				accumulatedAllRow.Items.Add(columnName, New TableViewColumn() With {.Value = formattedAccumulated, .IsHeader = False, .DataType = XFDataType.Decimal})
			Next			' Add empty CashDebtActual cell to Accumulated All row
					accumulatedAllRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
					
					tv.Rows.Add(accumulatedAllRow)
					
					' ROW 3: Accumulated Oper Inv Cashflow (cumulative Operating + Investment flows)
					Dim accumulatedOperInvRow As New TableViewRow() With {.IsHeader = False}
					accumulatedOperInvRow.Items.Add("Description", New TableViewColumn() With {.Value = ROW_ACCUMULATED_OPERINV, .IsHeader = False})
					
				For i As Integer = 0 To totalWeeks.Count - 1
					Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
					Dim weekYear As Integer = CInt(weekInfo("Year"))
					Dim weekNum As Integer = CInt(weekInfo("Week"))
					Dim columnName As String = $"Week{i + 1}"
					Dim weekOperInvTotal As Decimal = 0
					
					' Sum only Operating Flows and Investment Flows
					' Note: operInvAccounts already declared in outer scope (line 664)
					
					For Each account As String In operInvAccounts
						Dim inflowsKey As String = $"{account.ToUpper()}|{FLOW_INFLOWS}|{weekYear}|{weekNum}"
						Dim outflowsKey As String = $"{account.ToUpper()}|{FLOW_OUTFLOWS}|{weekYear}|{weekNum}"
						
						Dim inflowValue As Decimal = 0
						Dim outflowValue As Decimal = 0
						
						flowData.TryGetValue(inflowsKey, inflowValue)
						flowData.TryGetValue(outflowsKey, outflowValue)
						
						weekOperInvTotal += inflowValue + outflowValue
					Next
					
				' Add previous week's accumulated value
				Dim previousWeekAccumulated As Decimal = 0
				If i > 0 Then
					previousWeekAccumulated = accumulatedOperInv(i - 1)
				Else
					' First displayed week: start from initialization (includes current year weeks 1 to firstWeek-1 only)
					previousWeekAccumulated = initialAccumulatedOperInv
				End If
				
			Dim currentAccumulated As Decimal = previousWeekAccumulated + weekOperInvTotal
				accumulatedOperInv(i) = currentAccumulated
				
				Dim formattedAccumulated As String = Me.FormatDecimalValue(currentAccumulated)
				accumulatedOperInvRow.Items.Add(columnName, New TableViewColumn() With {.Value = formattedAccumulated, .IsHeader = False, .DataType = XFDataType.Decimal})
			Next			' Add empty CashDebtActual cell to Accumulated Oper Inv row
					accumulatedOperInvRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
					
					tv.Rows.Add(accumulatedOperInvRow)
					
					' ROW 4: Opening Cash (Closing Cash from previous week)
					Dim openingCashRow As New TableViewRow() With {.IsHeader = False}
					openingCashRow.Items.Add("Description", New TableViewColumn() With {.Value = ROW_OPENING_CASH, .IsHeader = False})
					
					' Storage for closing cash values
					Dim closingCashValues As New Dictionary(Of Integer, Decimal)
					
					' Get previous year's closing cash for W1 cross-year carryover
					Dim previousYearClosingCash As Decimal = 0
					Dim firstWeekOfDisplay As Dictionary(Of String, Object) = totalWeeks(0)
					Dim firstDisplayWeekNum As Integer = CInt(firstWeekOfDisplay("Week"))
					Dim firstDisplayYear As Integer = CInt(firstWeekOfDisplay("Year"))
					
					' If the first displayed week is W1, we need to get previous year's closing cash
					If firstDisplayWeekNum = 1 Then
						previousYearClosingCash = Me.GetPreviousYearClosingCash(si, dbConn, firstDisplayYear - 1, entityFilter)
					End If
					
					For i As Integer = 0 To totalWeeks.Count - 1
						Dim weekInfo As Dictionary(Of String, Object) = totalWeeks(i)
						Dim weekYear As Integer = CInt(weekInfo("Year"))
						Dim weekNum As Integer = CInt(weekInfo("Week"))
						Dim columnName As String = $"Week{i + 1}"
						Dim openingCash As Decimal = 0
						
						' Opening Cash = Closing Cash from previous week
						' CROSS-YEAR SUPPORT: For W1 of any year, use previous year's closing cash
						If i = 0 AndAlso weekNum = 1 Then
							' First column is W1: Use previous year's closing cash (sum of all Actual flows)
							openingCash = previousYearClosingCash
						ElseIf i > 0 AndAlso weekNum = 1 Then
							' Mid-display W1 (cross-year in forecast): Get previous year closing from calculated values
							' This happens when forecast spans from Dec (W51/52) into Jan (W1) of next year
							closingCashValues.TryGetValue(i - 1, openingCash)
						ElseIf i > 0 Then
				closingCashValues.TryGetValue(i - 1, openingCash)
			End If
			
			Dim formattedOpening As String = Me.FormatDecimalValue(openingCash)
			openingCashRow.Items.Add(columnName, New TableViewColumn() With {.Value = formattedOpening, .IsHeader = False, .DataType = XFDataType.Decimal})					' Calculate and store Closing Cash for this week (needed for next iteration)
					Dim weekTotal As Decimal = 0						' Get current week's Total General
						For Each account As String In accountOrder
							Dim inflowsKey As String = $"{account.ToUpper()}|{FLOW_INFLOWS}|{weekYear}|{weekNum}"
							Dim outflowsKey As String = $"{account.ToUpper()}|{FLOW_OUTFLOWS}|{weekYear}|{weekNum}"
							
							Dim inflowValue As Decimal = 0
							Dim outflowValue As Decimal = 0
							
							flowData.TryGetValue(inflowsKey, inflowValue)
							flowData.TryGetValue(outflowsKey, outflowValue)
							
							weekTotal += inflowValue + outflowValue
						Next
						
						' Closing Cash = Opening Cash + Total General
						Dim closingCash As Decimal = openingCash + weekTotal
						closingCashValues(i) = closingCash
					Next
			
			' Add empty CashDebtActual cell to Opening Cash row
					openingCashRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
					
					tv.Rows.Add(openingCashRow)
					
					' ROW 5: Closing Cash (Opening Cash + Total General)
					Dim closingCashRow As New TableViewRow() With {.IsHeader = False}
					closingCashRow.Items.Add("Description", New TableViewColumn() With {.Value = ROW_CLOSING_CASH, .IsHeader = False})
					
					For i As Integer = 0 To totalWeeks.Count - 1
						Dim columnName As String = $"Week{i + 1}"
						Dim closingCash As Decimal = 0
						
					closingCashValues.TryGetValue(i, closingCash)
				
				Dim formattedClosing As String = Me.FormatDecimalValue(closingCash)
				closingCashRow.Items.Add(columnName, New TableViewColumn() With {.Value = formattedClosing, .IsHeader = False, .DataType = XFDataType.Decimal})
			Next					' Add empty CashDebtActual cell to Closing Cash row
					closingCashRow.Items.Add("CashDebtActual", New TableViewColumn() With {.Value = "", .IsHeader = False})					
					tv.Rows.Add(closingCashRow)
					
					' Note: No formatting applied to maintain user's Excel formatting preferences
					
					Return tv
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Save Data"
		
		Private Function SaveCashFlowForecastChanges(ByVal si As SessionInfo, ByVal tableView As TableView) As Boolean
		    Try
	        ' *** Recuperar parámetros de la primera fila (formato serializado) ***
	        ' Formato: PARAMS|entity|year|week|totalWeeks|[y1:w1,y2:w2,...]
	        Dim paramEntityId As String = ""
	        Dim paramYear As String = ""
	        Dim paramWeek As String = ""
	        Dim weekMetadataDict As New Dictionary(Of Integer, Tuple(Of Integer, Integer)) ' columnIndex -> (year, week)		        ' La primera fila contiene los parámetros en la columna Description
		        If tableView.Rows.Count > 0 Then
		            Dim paramRow As TableViewRow = tableView.Rows(0)
		            
		            If paramRow.Items.ContainsKey("Description") Then
		                Dim paramValue As String = If(paramRow.Items("Description").Value IsNot Nothing, _
		                    paramRow.Items("Description").Value.ToString(), "")
		                
		                
                ' Parse formato: PARAMS|entity|year|week|totalWeeks|[y1:w1,y2:w2,...]
                If paramValue.StartsWith("PARAMS|") Then
                    Dim parts As String() = paramValue.Split("|"c)
                    If parts.Length >= 6 Then
                        paramEntityId = parts(1)
                        paramYear = parts(2)
                        paramWeek = parts(3)
                        Dim totalWeeksStr As String = parts(4)
                        Dim weekMetadataStr As String = parts(5)
                        
                        ' Parse week metadata: y1:w1,y2:w2,...
                        Dim weekPairs As String() = weekMetadataStr.Split(","c)
                        For i As Integer = 0 To weekPairs.Length - 1
                            Dim pair As String = weekPairs(i)
                            If pair.Contains(":") Then
                                Dim pairParts As String() = pair.Split(":"c)
                                If pairParts.Length = 2 Then
                                    Dim year As Integer = 0
                                    Dim week As Integer = 0
                                    If Integer.TryParse(pairParts(0), year) AndAlso Integer.TryParse(pairParts(1), week) Then
                                        weekMetadataDict(i) = New Tuple(Of Integer, Integer)(year, week)
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
            End If
        End If
        
        ' Validar que los parámetros se recuperaron correctamente
			If String.IsNullOrEmpty(paramEntityId) Then
				Return False
			End If
			
			If String.IsNullOrEmpty(paramYear) Then
				Return False
			End If
			
			If String.IsNullOrEmpty(paramWeek) Then
				Return False
			End If
			
			Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
			Dim currentWeek As Integer
			If Not Integer.TryParse(paramWeek, currentWeek) Then
				Return False
			End If
			
			Dim currentYearInt As Integer
			If Not Integer.TryParse(paramYear, currentYearInt) Then
				Return False
			End If				
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' CHECK IF WEEK IS CONFIRMED - If confirmed, block all saves
					Dim isConfirmed As Boolean = False
					Try
					Dim checkSql As String = TRS_SQL_Repository.GetConfirmationStatusQuery("CashFlow")
					
					Dim checkParams As New List(Of DbParamInfo) From {
						New DbParamInfo("entity", paramEntity),
						New DbParamInfo("week", currentWeek),
						New DbParamInfo("year", currentYearInt)
					}
					
					Dim checkDt As DataTable = BRApi.Database.ExecuteSql(dbConn, checkSql, checkParams, False)						
					
					If checkDt.Rows.Count > 0 Then
							Dim cashFlowBit As Object = checkDt.Rows(0)("CashFlow")
							If Not IsDBNull(cashFlowBit) AndAlso Convert.ToBoolean(cashFlowBit) Then
								isConfirmed = True
					End If
				End If
			Catch ex As Exception
			End Try			' If week is confirmed, prevent any saves
			If isConfirmed Then
				Return False
			End If
						' Counter for tracking updates
				Dim updatesExecuted As Integer = 0
				Dim cellsChecked As Integer = 0
					
					Dim originalValues As New Dictionary(Of String, Decimal)
					
				' Build WHERE clause
				Dim entityFilter As String = If(paramEntity = "HTD", "", $"AND Entity = '{paramEntity}'")
				
				' Only get the current week's TimeKey and Scenario (FORECAST data only)
				Dim currentScenario As String = $"FW{currentWeek.ToString().PadLeft(2, "0"c)}"
				Dim currentTimeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear, paramWeek)
				
				' Calculate which years we need to query (current year + potentially next year for cross-year forecasts)
				Dim yearsToQuery As String = currentYearInt.ToString()
				If currentWeek + 8 > 52 Then
					' Need to include next year data
					yearsToQuery = $"{currentYearInt},{currentYearInt + 1}"
				End If
				
				' Log the SQL query parameters for debugging
				
			Dim originalDataSql As String = $"
				SELECT Account, Flow, Scenario, ProjectionYear, ProjectionWeekNumber, Amount
				FROM {TABLE_MASTER_CASHFLOW} 
				WHERE UploadTimekey = '{currentTimeKey}' 
				{entityFilter}
				AND UPPER(Scenario) = '{currentScenario}'
				AND UPPER(Account) IN ('{ACCOUNT_OPERATING}', '{ACCOUNT_INVESTMENT}', '{ACCOUNT_FINANCING}', '{ACCOUNT_CF_NOT_INCL}', '{ACCOUNT_OTHER_FLOWS}')
				AND ProjectionYear IN ({yearsToQuery})
				AND ProjectionWeekNumber BETWEEN 1 AND 52
			"
			
			Dim originalDataDt As DataTable = BRApi.Database.ExecuteSql(dbConn, originalDataSql, False)
			
			' Build dictionary of original values from normalized Master table
			' Key format: Account|Flow|Scenario|Year|WeekCol
			For Each row As DataRow In originalDataDt.Rows
				Dim account As String = row("Account").ToString().ToUpper()
				Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString().ToUpper())
				Dim scenario As String = row("Scenario").ToString().ToUpper()
				Dim projYear As Integer = If(IsDBNull(row("ProjectionYear")), currentYearInt, Convert.ToInt32(row("ProjectionYear")))
				Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
				Dim weekValue As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
				
				Dim weekCol As String = $"W{weekNum.ToString().PadLeft(2, "0"c)}"
				Dim key As String = $"{account}|{flow}|{scenario}|{projYear}|{weekCol}"
				originalValues(key) = weekValue
			Next				' Track current account for data rows
				Dim currentAccount As String = ""
				Dim validAccounts As String() = Me.GetAccountOrder()
				
				For Each tableViewRow As TableViewRow In tableView.Rows
						If tableViewRow.Items.ContainsKey("Description") Then
							Dim descValue As String = If(tableViewRow.Items("Description").Value IsNot Nothing, _
								tableViewRow.Items("Description").Value.ToString(), "")
							
							' Skip the parameter row (first row with PARAMS| prefix)
							If descValue.StartsWith("PARAMS|") Then
								Continue For
							End If
							
					' If this is an Account row, remember it
					' Use the exact list of accounts from GetAccountOrder to ensure all accounts are recognized
					If validAccounts.Contains(descValue) Then
						currentAccount = descValue
						Continue For
					End If					' STEP 5: Process editable data rows (INFLOWS or OUTFLOWS)
					If tableViewRow.IsHeader = False AndAlso (descValue = FLOW_INFLOWS OrElse descValue = FLOW_OUTFLOWS) Then
						Dim flow As String = descValue
						Dim account As String = currentAccount
						
						' STEP 6: Iterate through row items (cells)
								For Each kvp As KeyValuePair(Of String, TableViewColumn) In tableViewRow.Items
									Dim columnName As String = kvp.Key
									Dim cellColumn As TableViewColumn = kvp.Value
									
									' Only process week columns
									If columnName.StartsWith("Week") Then
										cellsChecked += 1
										
										' Get the new value from the cell FIRST (for logging)
										Dim cellValue As String = If(cellColumn.Value IsNot Nothing, cellColumn.Value.ToString(), "NULL")
										
										' ✅ Get year/week from metadata dictionary using column index
										' Extract column index from "Week1", "Week2", etc.
										Dim columnIndexStr As String = columnName.Substring(4)
										Dim columnIndex As Integer = 0
										If Integer.TryParse(columnIndexStr, columnIndex) Then
											columnIndex -= 1 ' Convert to 0-based index
											
											Dim projectionYear As Integer = currentYearInt ' Default
											Dim weekNum As Integer = 0
											
											' Lookup year/week from metadata dictionary
									If weekMetadataDict.ContainsKey(columnIndex) Then
										Dim yearWeekTuple As Tuple(Of Integer, Integer) = weekMetadataDict(columnIndex)
										projectionYear = yearWeekTuple.Item1
										weekNum = yearWeekTuple.Item2
									Else
									End If											
									If weekNum > 0 Then
											
											' Get the new value from the cell
											Dim newValue As String = If(cellColumn.Value IsNot Nothing, cellColumn.Value.ToString(), "0")
											Dim newValueDecimal As Decimal
											If Not Decimal.TryParse(newValue, NumberStyles.Any, CultureInfo.InvariantCulture, newValueDecimal) Then
												newValueDecimal = 0
											End If
											
										' Determine scenario based on week position
										Dim scenario As String
										Dim timeKey As String
										' CRITICAL: Only Actual weeks (before current week in current year) are read-only
										' Current week and future weeks are editable (Forecast)
										' MODIFICATION: Allow editing the previous week (currentWeek - 1) as well
										' This allows correcting the Actual data uploaded for the immediately preceding week
										Dim isEditableWeek As Boolean = False
										
										If projectionYear > currentYearInt Then
											' Future years are always editable (Forecast)
											isEditableWeek = True
										ElseIf projectionYear = currentYearInt Then
											' Current year: Editable if it's current week, future weeks, OR previous week
											If weekNum >= (currentWeek - 1) Then
												isEditableWeek = True
											End If
										End If
										
								' CRITICAL VALIDATION: Only allow saving FORECAST weeks and PREVIOUS week
								' Older Actual weeks are read-only historical data
								If Not isEditableWeek Then
									' SKIP: Older Actual weeks use different scenarios and should not be modified
									Continue For
								End If										' FORECAST weeks: Use current week scenario and timekey
										scenario = $"FW{currentWeek.ToString().PadLeft(2, "0"c)}"
										timeKey = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear, paramWeek)
										
										' Build the key to lookup original value - include year
										Dim weekCol As String = $"W{weekNum.ToString().PadLeft(2, "0"c)}"
										Dim lookupKey As String = $"{account.ToUpper()}|{flow.ToUpper()}|{scenario.ToUpper()}|{projectionYear}|{weekCol}"
										
											' STEP 7: CRITICAL - Compare with original value
											' IMPORTANT: Only process if the original value exists in the dictionary
											' This prevents saving cells that don't have corresponding DB records
											If originalValues.ContainsKey(lookupKey) Then
												Dim originalValue As Decimal = originalValues(lookupKey)
												
									' Only update if value actually changed (with tolerance for decimal precision)
									Dim valueChanged As Boolean = Math.Abs(newValueDecimal - originalValue) > 0.001D
								
								If valueChanged Then
												' DEBUG: Log the change being made
												
												' Build WHERE clause for UPDATE in normalized Master table
																Dim entityFilterInner As String = If(paramEntity = "HTD", "", $"AND Entity = '{paramEntity}'")
												Dim updateSql As String = $"
													UPDATE {TABLE_MASTER_CASHFLOW} 
													SET Amount = {newValueDecimal.ToString(CultureInfo.InvariantCulture)}
													WHERE UploadTimekey = '{timeKey}' 
													{entityFilterInner}
													AND UPPER(Account) = '{account.ToUpper()}'
													AND UPPER(Flow) = '{flow.ToUpper()}'
													AND UPPER(Scenario) = '{scenario.ToUpper()}'
													AND ProjectionYear = {projectionYear}
													AND ProjectionWeekNumber = {weekNum}
												"
												
												' Execute the update only for changed cells
												If Not String.IsNullOrEmpty(account) AndAlso Not String.IsNullOrEmpty(flow) Then
													Try
														BRApi.Database.ExecuteSql(dbConn, updateSql, False)
														updatesExecuted += 1
													Catch sqlEx As Exception
														' DEBUG: Log SQL errors
													End Try
												Else
												End If
										Else
										End If
									Else
									End If
												' Note: If lookupKey not found in originalValues, we skip the update
												' This cell doesn't have a corresponding DB record to update									
												End If ' weekNum > 0
									End If ' Week number parse check
								End If ' Week column check
							Next ' kvp in tableViewRow.Items
								
							End If ' Editable row check
					End If
			Next tableViewRow				
			Return True				
			End Using
				
			Catch ex As Exception
				Return False
			End Try
		End Function
		
		#End Region

	End Class
End Namespace



