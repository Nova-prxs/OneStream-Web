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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashFlowForecasting_CF_CB_HTD
	Public Class MainClass
		'Reference Solution Helper Business Rule - HTD Version (Filtered for HTD entities only)
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							' Only Year and Week parameters needed for HTD view
							list.Add("prm_Treasury_Year")
							list.Add("prm_Treasury_WeekNumber")
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

		Private Function FormatDecimalValue(ByVal value As Decimal) As String
			Return If(value = 0, "0.00", value.ToString("F2", CultureInfo.InvariantCulture))
		End Function

		Private Function GetYearAccumulatedValues(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal paramEntity As String, ByVal targetYear As String) As Dictionary(Of String, Decimal)
			Try
				Dim result As New Dictionary(Of String, Decimal)
				result("AccumulatedAll") = 0
				result("AccumulatedOperInv") = 0
				
				Dim yearInt As Integer
				If Not Integer.TryParse(targetYear, yearInt) Then Return result
				
				' Get max weeks for the target year
				Dim maxWeeksSql As String = TRS_SQL_Repository.GetMaxWeekInYearQuery(targetYear)
				Dim maxWeeksDt As DataTable = BRApi.Database.ExecuteSql(dbConn, maxWeeksSql, False)
				Dim maxWeeks As Integer = 52 ' Default to 52
				If maxWeeksDt IsNot Nothing AndAlso maxWeeksDt.Rows.Count > 0 AndAlso Not IsDBNull(maxWeeksDt.Rows(0)("MaxWeek")) Then
					maxWeeks = Convert.ToInt32(maxWeeksDt.Rows(0)("MaxWeek"))
				End If
				
				Dim entityFilter As String = If(paramEntity <> "HTD", $"AND Entity = '{paramEntity}'", "")
				Dim yearSql As String = TRS_SQL_Repository.GetCashFlowYearEntityQuery(targetYear, entityFilter, maxWeeks, True)
				Dim yearDt As DataTable = BRApi.Database.ExecuteSql(dbConn, yearSql, False)
				
				If yearDt.Rows.Count = 0 Then
					Return result
				End If
				
				Dim operInvAccounts As String() = {"OPERATING FLOWS", "INVESTMENT FLOWS"}
				
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
				Dim accountOrder As String() = {"OPERATING FLOWS", "INVESTMENT FLOWS", "FINANCING FLOWS", "CF NOT INCL. IN OP FCF (INCL. DIVIDENDS)", "OTHER FLOWS (DIV)"}
				
				' Calculate accumulated values for THIS YEAR ONLY (weeks 1-52)
				' Sum all weeks of the target year
				Dim accumulatedAll As Decimal = 0
				Dim accumulatedOperInv As Decimal = 0
				
				For weekNum As Integer = 1 To maxWeeks
					Dim weekTotal As Decimal = 0
					Dim weekOperInvTotal As Decimal = 0
					
					' Sum all accounts for this week using optimized TryGetValue
					For Each account As String In accountOrder
						Dim inflowsKey As String = $"{account}|INFLOWS|{targetYear}|{weekNum}"
						Dim outflowsKey As String = $"{account}|OUTFLOWS|{targetYear}|{weekNum}"
						
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
					
					' Accumulate
					accumulatedAll += weekTotal
					accumulatedOperInv += weekOperInvTotal
				Next
				
				result("AccumulatedAll") = accumulatedAll
				result("AccumulatedOperInv") = accumulatedOperInv
				
				Return result
				
			Catch ex As Exception
				Return New Dictionary(Of String, Decimal) From {{"AccumulatedAll", 0}, {"AccumulatedOperInv", 0}}
			End Try
		End Function

		#Region "Cash Flow Forecasting Report - HTD"

		Private Function GetCashFlowForecastingReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
			Try
				' Validate input parameters
				If args Is Nothing OrElse args.CustSubstVarsAlreadyResolved Is Nothing Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: No parameters provided")
				End If
				
				' Get parameters from spreadsheet args
				Dim paramYear As String = ""
				Dim paramWeek As String = ""
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")) Then
					paramYear = args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")) Then
					paramWeek = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")
				End If
				
				' Validate required parameters
				If String.IsNullOrEmpty(paramYear) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Year parameter is missing")
				End If
				
				If String.IsNullOrEmpty(paramWeek) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Week parameter is missing")
				End If
				
				' Validate paramWeek for processing (will validate against maxWeeks after query)
				Dim currentWeek As Integer
				If Not Integer.TryParse(paramWeek, currentWeek) OrElse currentWeek < 1 Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Invalid week number '{paramWeek}'. Must be at least 1.")
				End If
				
			' HTD VERSION: Define ONLY HTD entities (exclude Aurobay Sweeden, HPL, Aurobay China)
			Dim allEntities As New Dictionary(Of String, Integer) From {
				{"Horse Aveiro", 671},
				{"OYAK HORSE", 1302},
				{"Horse Romania", 611},
					{"Horse Brazil", 1303},
					{"Horse Chile", 585},
					{"Horse Argentina", 592},
					{"Horse Spain", 1301},
					{"Horse Powertrain Solution", 1300}
				}
				
			' HTD entities for Total Horse Group (same as allEntities in HTD version)
			Dim horseGroupEntities As String() = {
				"Horse Aveiro", "OYAK HORSE", "Horse Romania", 
				"Horse Brazil", "Horse Chile", "Horse Argentina",
					"Horse Spain", "Horse Powertrain Solution"
				}
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Get max week number for the selected year from XFC_TRS_AUX_Date
					Dim maxWeekSql As String = TRS_SQL_Repository.GetMaxWeekInYearQuery(paramYear)
					Dim maxWeekDt As DataTable = BRApi.Database.ExecuteSql(dbConn, maxWeekSql, False)
					Dim maxWeeks As Integer = 52 ' Default value
					
					If maxWeekDt IsNot Nothing AndAlso maxWeekDt.Rows.Count > 0 AndAlso Not IsDBNull(maxWeekDt.Rows(0)("MaxWeek")) Then
						maxWeeks = Convert.ToInt32(maxWeekDt.Rows(0)("MaxWeek"))
					End If
					
					' Create the dynamic TableView
					Dim tv As New TableView()
					tv.CanModifyData = False ' Read-only view
					
					' ====================================
					' STEP 1: Define columns structure
					' ====================================
					' First column: Entity names
					tv.Columns.Add(New TableViewColumn() With {.Name = "Entity", .Value = "Accum Cashflows + Cash Balance", .IsHeader = True})
					
					' Calculate total weeks to display (from week 1 to maxWeeks)
					Dim totalWeeks As New List(Of Integer)
					For i As Integer = 1 To maxWeeks
					    totalWeeks.Add(i)
					Next
					
					' Add week columns (Week 1 through Week 52)
					For Each weekNum As Integer In totalWeeks
					    tv.Columns.Add(New TableViewColumn() With {
					        .Name = $"Week{weekNum}", 
					        .Value = $"Week {weekNum}", 
					        .IsHeader = True, 
					        .DataType = XFDataType.Decimal
					    })
					Next
					
					' ====================================
					' STEP 2: Add header rows (week numbers and dates)
					' ====================================
					' Header Row 1: Week numbers with "Million of EUR"
					Dim weekNumberRow As New TableViewRow() With {.IsHeader = True}
					weekNumberRow.Items.Add("Entity", New TableViewColumn() With {.Value = "Million of EUR", .IsHeader = True})
					For Each weekNum As Integer In totalWeeks
						weekNumberRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {.Value = $"Week {weekNum}", .IsHeader = True})
					Next
					tv.Rows.Add(weekNumberRow)
					
					' Header Row 2: Monday dates for each week with "Accumulated Cashflows + Cash Balance"
					Dim mondayDateRow As New TableViewRow() With {.IsHeader = True}
					mondayDateRow.Items.Add("Entity", New TableViewColumn() With {.Value = "Accum Cashflows + Cash Balance", .IsHeader = True})
					For Each weekNum As Integer In totalWeeks
						Dim mondayDate As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMondayDateForWeek(si, paramYear, weekNum.ToString())
						mondayDateRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {.Value = mondayDate, .IsHeader = True})
					Next
					tv.Rows.Add(mondayDateRow)
					
					' ====================================
					' STEP 4: Storage for cumulative calculations
					' ====================================
					Dim accumulatedAllCashflow As New Dictionary(Of Integer, Decimal)
					
					' ====================================
					' STEP 4: Get flow data for HTD entities and ALL weeks
					' ====================================
					' CHANGED: Query updated to use new unpivoted table structure
					' The table now has ProjectionWeekNumber and Amount columns instead of W01-W52
					' CRITICAL LOGIC: UploadWeekNumber = ProjectionWeekNumber + 1 for Actual data
					' Use centralized SQL repository
					Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowActualAllEntitiesQuery(paramYear, currentWeek, True)
					
					Dim allDataDt As DataTable = BRApi.Database.ExecuteSql(dbConn, allDataSql, False)
					
					' Build nested dictionary: Entity -> Week -> Accumulated Cashflow value
					' Structure: entityData(EntityName)(WeekNumber) = Decimal
					Dim entityData As New Dictionary(Of String, Dictionary(Of Integer, Decimal))
					
					' Initialize dictionary for all HTD entities
					For Each kvp As KeyValuePair(Of String, Integer) In allEntities
						Dim entity As String = kvp.Key
						entityData(entity) = New Dictionary(Of Integer, Decimal)
						' Initialize all weeks to 0
						For weekNum As Integer = 1 To maxWeeks
							entityData(entity)(weekNum) = 0
						Next
					Next
					
					' ====================================
					' STEP 5: Calculate Accumulated Cashflow for each HTD entity and week
					' ====================================
					' CHANGED: Data is already unpivoted, no manual unpivot needed
					' Process database results and calculate accumulated cashflows
					
					' Dictionary to store: Entity -> UploadTimekey+Scenario -> Week -> Total flow value
					' Structure simplified since data is already in long format
					Dim weeklyTotals As New Dictionary(Of String, Dictionary(Of String, Dictionary(Of Integer, Decimal)))
					
					' Initialize structure for HTD entities only
					For Each kvp As KeyValuePair(Of String, Integer) In allEntities
						Dim entity As String = kvp.Key
						weeklyTotals(entity) = New Dictionary(Of String, Dictionary(Of Integer, Decimal))
					Next
					
					' CHANGED: Process rows directly from unpivoted structure
					' Build structure: Entity -> ProjectionWeekNumber -> Total amount
					For Each row As DataRow In allDataDt.Rows
						Dim entity As String = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
						Dim weekNumber As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
						Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
						

						
						' HTD VERSION: Only process HTD entities
						If Not allEntities.ContainsKey(entity) Then
							Continue For
						End If
						
						' Validate week number
						If weekNumber < 1 OrElse weekNumber > maxWeeks Then
							Continue For
						End If
						
						' Accumulate amounts directly by ProjectionWeekNumber
						If Not weeklyTotals(entity).ContainsKey("Actual") Then
							weeklyTotals(entity)("Actual") = New Dictionary(Of Integer, Decimal)
							For i As Integer = 1 To maxWeeks
								weeklyTotals(entity)("Actual")(i) = 0
							Next
						End If
						
							weeklyTotals(entity)("Actual")(weekNumber) += amount
							

					Next
				
				' Calculate ACCUMULATED values for each HTD entity - ONLY for current year (paramYear)
				' No recursive calculation from previous years
				
				Dim accountOrder As String() = {"OPERATING FLOWS", "INVESTMENT FLOWS", "FINANCING FLOWS", "CF NOT INCL. IN OP FCF (INCL. DIVIDENDS)", "OTHER FLOWS (DIV)"}
				Dim operInvAccounts As String() = {"OPERATING FLOWS", "INVESTMENT FLOWS"}
				
				For Each kvp As KeyValuePair(Of String, Integer) In allEntities
					Dim entity As String = kvp.Key
					
					Dim previousWeekAccumulated As Decimal = 0
					
					For weekNum As Integer = 1 To maxWeeks
						' Get week total from Actual data
						Dim weekTotal As Decimal = 0
						
						If weeklyTotals.ContainsKey(entity) AndAlso 
						   weeklyTotals(entity).ContainsKey("Actual") AndAlso 
						   weeklyTotals(entity)("Actual").ContainsKey(weekNum) Then
							weekTotal = weeklyTotals(entity)("Actual")(weekNum)
						End If
						
						' Accumulation WITH historical carryover: add weekTotal to previous accumulated value
						Dim currentWeekAccumulated As Decimal = previousWeekAccumulated + weekTotal
						entityData(entity)(weekNum) = currentWeekAccumulated
						
						' Update for next iteration
						previousWeekAccumulated = currentWeekAccumulated
					Next
				Next
					
					' ====================================
					' STEP 6: Add HTD entity rows with accumulated cashflow data
					' ====================================
					
				' HTD VERSION: Show all HTD entities (no exclusions in HTD view)
				For Each kvp As KeyValuePair(Of String, Integer) In allEntities
					Dim entity As String = kvp.Key
					
					Dim entityRow As New TableViewRow() With {.IsHeader = False}
					entityRow.Items.Add("Entity", New TableViewColumn() With {.Value = entity, .IsHeader = False})
					
				For Each weekNum As Integer In totalWeeks
					Dim accumulatedValue As Decimal = entityData(entity)(weekNum)
					Dim formattedValue As String = Me.FormatDecimalValue(accumulatedValue)
					
					entityRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
						.Value = formattedValue,
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next					
				tv.Rows.Add(entityRow)
				Next					' ====================================
					' STEP 7: Add "Total Horse Group" row (sum of all HTD entities)
					' ====================================
				Dim totalHorseGroupRow As New TableViewRow() With {.IsHeader = False}
				totalHorseGroupRow.Items.Add("Entity", New TableViewColumn() With {.Value = "Total Horse Group", .IsHeader = True})
				
			For Each weekNum As Integer In totalWeeks
				Dim totalValue As Decimal = 0
				
				' Sum values from all HTD entities
				For Each entity As String In horseGroupEntities
					If entityData.ContainsKey(entity) AndAlso entityData(entity).ContainsKey(weekNum) Then
						totalValue += entityData(entity)(weekNum)
					End If
				Next
				
				Dim formattedTotal As String = Me.FormatDecimalValue(totalValue)
				totalHorseGroupRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
					.Value = formattedTotal,
					.IsHeader = True,
					.DataType = XFDataType.Decimal
				})
			Next				
			tv.Rows.Add(totalHorseGroupRow)					
				tv.HeaderFormat.IsBold = True
					tv.HeaderFormat.TextColor = XFColors.White
					
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
		        ' This view is READ-ONLY - no save functionality needed
		        ' All data is calculated dynamically from the master data
		        Return True
		    Catch ex As Exception
		        Return False
		    End Try
		End Function
		
		#End Region

	End Class
End Namespace
