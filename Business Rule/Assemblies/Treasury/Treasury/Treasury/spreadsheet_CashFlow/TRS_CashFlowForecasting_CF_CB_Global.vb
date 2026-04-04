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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashFlowForecasting_CF_CB_Global
	Public Class MainClass
		'Reference Solution Helper Business Rule
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							' Only Year and Week parameters needed for global view
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

	' Helper function to get accumulated values for a specific year
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
			Dim yearSql As String = TRS_SQL_Repository.GetCashFlowYearEntityQuery(targetYear, entityFilter, 52, True)
			
			Dim yearDt As DataTable = BRApi.Database.ExecuteSql(dbConn, yearSql, False)
			
			' Get Operating/Investment accounts for OperInv calculation
			Dim operInvAccounts As String() = {"OPERATING FLOWS", "INVESTMENT FLOWS"}
			
			' Build dictionary by Entity -> Week -> totals
			Dim entityWeekData As New Dictionary(Of String, Dictionary(Of Integer, Dictionary(Of String, Decimal)))
			
			' If no data, check if we're in multi-entity mode and initialize all entities with 0
			If yearDt.Rows.Count = 0 Then
				If paramEntity = "HTD" Then
					' Multi-entity mode: Initialize all known entities with 0
					Dim allEntities As String() = {
						"Horse Aveiro", "OYAK HORSE", "Horse Romania", 
						"Horse Brazil", "Horse Chile", "Horse Argentina", 
						"Horse Spain", "Horse Powertrain Solution", "AUROBAY SWEDEN", "Horse HPL", "HPL China"
					}
					
					For Each entityName As String In allEntities
						result($"{entityName}_All") = 0
						result($"{entityName}_OperInv") = 0
					Next
					Return result
				Else
					' Single entity mode: return default keys
					Return result
				End If
			End If
			
			For Each row As DataRow In yearDt.Rows
				Dim entity As String = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
				Dim account As String = If(IsDBNull(row("Account")), "", row("Account").ToString().ToUpper())
				Dim weekNum As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
				Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
				
				If Not entityWeekData.ContainsKey(entity) Then
					entityWeekData(entity) = New Dictionary(Of Integer, Dictionary(Of String, Decimal))
				End If
				
				If Not entityWeekData(entity).ContainsKey(weekNum) Then
					entityWeekData(entity)(weekNum) = New Dictionary(Of String, Decimal) From {
						{"All", 0},
						{"OperInv", 0}
					}
				End If
				
				entityWeekData(entity)(weekNum)("All") += amount
				
				If operInvAccounts.Contains(account) Then
					entityWeekData(entity)(weekNum)("OperInv") += amount
				End If
			Next
			
			' Return ACCUMULATED values by entity (week by week accumulation)
			' Format: "EntityName_All" -> accumulated value at Week 52, "EntityName_OperInv" -> accumulated value at Week 52
			For Each entityKvp In entityWeekData
				Dim entityName As String = entityKvp.Key
				Dim accumulatedAll As Decimal = 0
				Dim accumulatedOperInv As Decimal = 0
				
				' Accumulate week by week (like the main file does)
				For weekNum As Integer = 1 To 52
					If entityKvp.Value.ContainsKey(weekNum) Then
						' Add this week's flow to accumulated total
						accumulatedAll += entityKvp.Value(weekNum)("All")
						accumulatedOperInv += entityKvp.Value(weekNum)("OperInv")
					End If
					' If no data for this week, accumulated stays the same (carry forward)
				Next
				
				' Store the FINAL accumulated value (at Week 52)
				result($"{entityName}_All") = accumulatedAll
				result($"{entityName}_OperInv") = accumulatedOperInv
			Next
			
			Return result
			
		Catch ex As Exception
			Return New Dictionary(Of String, Decimal) From {
				{"AccumulatedAll", 0},
				{"AccumulatedOperInv", 0}
			}
		End Try
	End Function

	#Region "Cash Flow Forecasting Report"

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
				
				' Validate paramWeek for processing
				Dim currentWeek As Integer
				If Not Integer.TryParse(paramWeek, currentWeek) OrElse currentWeek < 1 OrElse currentWeek > 52 Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Invalid week number '{paramWeek}'. Must be between 1 and 52.")
				End If
				
				' Define all entities with their company IDs (nombres EXACTOS de XFC_TRS_Master_CashflowForecasting)
				Dim allEntities As New Dictionary(Of String, Integer) From {
					{"Horse Aveiro", 671},
					{"OYAK HORSE", 1302},
					{"Horse Romania", 611},
					{"Horse Brazil", 1303},
					{"Horse Chile", 585},
					{"Horse Argentina", 592},
					{"Horse Spain", 1301},
					{"Horse Powertrain Solution", 1300},
					{"AUROBAY SWEDEN", 998},
					{"Horse HPL", 997},
					{"HPL China", 999}
				}
				
				' Entities to sum for Total Horse Group (all except Aurobay and HPL)
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
				' STEP 3: Get flow data for ALL entities and ALL weeks
				' ====================================
				' Get current year as integer
				Dim currentYearInt As Integer
				If Not Integer.TryParse(paramYear, currentYearInt) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Invalid year '{paramYear}'")
				End If
				
				' CHANGED: Query updated to use new unpivoted table structure
				' The table now has ProjectionWeekNumber and Amount columns instead of W01-W52
				' ADDED: Filter by UploadYear and ProjectionYear to get only current year data
				' FILTER: Only ProjectionType = 'Actual' to show only historical weeks
				' CRITICAL LOGIC (same as TRS_CashFlowForecasting):
				'   - In week 38, we upload Actual data for week 37: UploadWeekNumber=38, ProjectionWeekNumber=37
				'   - In week 37, we upload Actual data for week 36: UploadWeekNumber=37, ProjectionWeekNumber=36
				'   - Relationship: UploadWeekNumber = ProjectionWeekNumber + 1
				'   - Filter: UploadWeekNumber <= {currentWeek} AND UploadWeekNumber = ProjectionWeekNumber + 1
				' Use centralized SQL repository
				Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowActualAllEntitiesQuery(paramYear, currentWeek, True)
				
				Dim allDataDt As DataTable = BRApi.Database.ExecuteSql(dbConn, allDataSql, False)
					
					' Build nested dictionary: Entity -> Week -> Accumulated Cashflow value
					' Structure: entityData(EntityName)(WeekNumber) = Decimal
					Dim entityData As New Dictionary(Of String, Dictionary(Of Integer, Decimal))
					
					' Initialize dictionary for all entities
					For Each kvp As KeyValuePair(Of String, Integer) In allEntities
						Dim entity As String = kvp.Key
						entityData(entity) = New Dictionary(Of Integer, Decimal)
						' Initialize all weeks to 0
						For weekNum As Integer = 1 To maxWeeks
							entityData(entity)(weekNum) = 0
						Next
					Next
					
					' ====================================
					' STEP 4: Calculate Accumulated Cashflow for each entity and week
					' ====================================
					' CHANGED: Data is already unpivoted, no manual unpivot needed
					' Process database results and calculate accumulated cashflows
					
					' Dictionary to store: Entity -> UploadTimekey+Scenario -> Week -> Total flow value
					' Structure simplified since data is already in long format
					Dim weeklyTotals As New Dictionary(Of String, Dictionary(Of String, Dictionary(Of Integer, Decimal)))
					
					' Initialize structure for all entities
					For Each kvp As KeyValuePair(Of String, Integer) In allEntities
						Dim entity As String = kvp.Key
						weeklyTotals(entity) = New Dictionary(Of String, Dictionary(Of Integer, Decimal))
					Next
					
					' CHANGED: Process rows directly from unpivoted structure
					' Each row already represents one week's data
					' Build structure: Entity -> ProjectionWeekNumber -> Total amount
					For Each row As DataRow In allDataDt.Rows
						Dim entity As String = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
						Dim weekNumber As Integer = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
						Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
						
						' Only process entities we care about
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
					
			' Calculate accumulated values for each entity (current year only - no historical carryover)
			For Each kvp As KeyValuePair(Of String, Integer) In allEntities
				Dim entity As String = kvp.Key
				
				' Start from 0 (no historical carryover)
				Dim initialAccumulatedAll As Decimal = 0
				
				' Track the accumulated value as we iterate through weeks
				Dim previousWeekAccumulated As Decimal = initialAccumulatedAll
				
				For weekNum As Integer = 1 To maxWeeks
					' Get week total from Actual data (already filtered by ProjectionType = 'Actual')
					Dim weekTotal As Decimal = 0
					
					If weeklyTotals.ContainsKey(entity) AndAlso 
					   weeklyTotals(entity).ContainsKey("Actual") AndAlso 
					   weeklyTotals(entity)("Actual").ContainsKey(weekNum) Then
						weekTotal = weeklyTotals(entity)("Actual")(weekNum)
					End If
					
					' SIMPLE LOGIC (same as TRS_CashFlowForecasting):
					' Add weekTotal to previous week's accumulated value
					' If weekTotal is 0, the accumulated value stays the same (carry forward)
					Dim currentWeekAccumulated As Decimal = previousWeekAccumulated + weekTotal
					
					' Store this week's accumulated value
					entityData(entity)(weekNum) = currentWeekAccumulated
					
					' Update previous week accumulated for next iteration
					previousWeekAccumulated = currentWeekAccumulated
				Next
			Next
					
					' ====================================
					' STEP 5: Add entity rows with accumulated cashflow data
					' ====================================
					
				' Entities to exclude from first loop (shown after Total Horse Group)
				Dim entitiesToShowLater As String() = {"AUROBAY SWEDEN", "Horse HPL", "HPL China"}
				
				' Add a row for each individual entity (excluding those shown later)
				For Each kvp As KeyValuePair(Of String, Integer) In allEntities
					Dim entity As String = kvp.Key
					
					' Skip entities that will be shown after Total Horse Group
					If entitiesToShowLater.Contains(entity) Then
						Continue For
					End If
					
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
		Next		' ====================================
		' STEP 6: Add "Total Horse Group" row (sum of specific entities)
		' ====================================
		Dim totalHorseGroupRow As New TableViewRow() With {.IsHeader = False}
		totalHorseGroupRow.Items.Add("Entity", New TableViewColumn() With {.Value = "Total Horse Group", .IsHeader = True})				
		For Each weekNum As Integer In totalWeeks
					Dim totalValue As Decimal = 0
					
					' Sum values from all Horse Group entities
					For Each entity As String In horseGroupEntities
						If entityData.ContainsKey(entity) AndAlso entityData(entity).ContainsKey(weekNum) Then
							totalValue += entityData(entity)(weekNum)
						End If
					Next
					
					Dim formattedTotal As String = Me.FormatDecimalValue(totalValue)
					totalHorseGroupRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
						.Value = formattedTotal,
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
			Next
			
			tv.Rows.Add(totalHorseGroupRow)
			
			' ====================================
			' STEP 7: Add additional entity rows (Aurobay Sweden, Horse HPL, HPL China)
			' ====================================
			Dim additionalEntities As String() = {"AUROBAY SWEDEN", "Horse HPL", "HPL China"}
			
			For Each entity As String In additionalEntities
				If allEntities.ContainsKey(entity) Then
					Dim entityRow As New TableViewRow() With {.IsHeader = False}
					entityRow.Items.Add("Entity", New TableViewColumn() With {.Value = entity, .IsHeader = False})
					
					For Each weekNum As Integer In totalWeeks
						Dim accumulatedValue As Decimal = 0
						If entityData.ContainsKey(entity) AndAlso entityData(entity).ContainsKey(weekNum) Then
							accumulatedValue = entityData(entity)(weekNum)
						End If
						
						Dim formattedValue As String = Me.FormatDecimalValue(accumulatedValue)
						entityRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
							.Value = formattedValue,
							.IsHeader = False,
							.DataType = XFDataType.Decimal
						})
					Next
					
					tv.Rows.Add(entityRow)
				End If
			Next					' Configure formatting
					tv.HeaderFormat.BackgroundColor = XFColors.Black
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
