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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashFlowForecasting_OF_IF_Global
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
				
				' Validate paramWeek for processing (will validate against maxWeeks after query)
				Dim currentWeek As Integer
				If Not Integer.TryParse(paramWeek, currentWeek) OrElse currentWeek < 1 Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Invalid week number '{paramWeek}'. Must be at least 1.")
				End If
				
			' Define all entities with their company IDs
			Dim allEntities As New Dictionary(Of String, Integer) From {
					{"Horse Aveiro", 671},
					{"OYAK HORSE", 1302},
					{"Horse Romania", 611},
					{"Horse Brazil", 1303},
					{"Horse Chile", 585},
					{"Horse Argentina", 592},
					{"Horse Spain", 1301},
					{"Horse Powertrain Solution", 1300},
					{"Aurobay Sweeden", 998},
					{"HPL", 999},
					{"Aurobay China", 999}
				}
				
			' Entities to sum for Total Horse Group (all except Horse Group and HPL)
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
					tv.Columns.Add(New TableViewColumn() With {.Name = "Entity", .Value = "Accum Operational + Investment Flows", .IsHeader = True})
					
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
					
					' Header Row 2: Monday dates for each week with "Accum Operational, Investment Flows"
					Dim mondayDateRow As New TableViewRow() With {.IsHeader = True}
					mondayDateRow.Items.Add("Entity", New TableViewColumn() With {.Value = "Accum Operational + Investment Flows", .IsHeader = True})
					For Each weekNum As Integer In totalWeeks
						Dim mondayDate As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMondayDateForWeek(si, paramYear, weekNum.ToString())
						mondayDateRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {.Value = mondayDate, .IsHeader = True})
					Next
					tv.Rows.Add(mondayDateRow)
					
					' ====================================
					' STEP 3: Get flow data for ALL entities and ALL weeks
					' ====================================
					' CHANGED: Query updated to use new unpivoted table structure
					' The table now has ProjectionWeekNumber and Amount columns instead of W01-W52
					' CRITICAL LOGIC: UploadWeekNumber = ProjectionWeekNumber + 1 for Actual data
					Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowActualAllEntitiesQuery(paramYear, currentWeek, False)
					
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
					' STEP 4: Calculate Accumulated Oper Inv Cashflow for each entity and week
					' ====================================
					' CHANGED: Data is already unpivoted, no manual unpivot needed
					' Process database results and calculate accumulated operational + investment cashflows
					
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
				
				' Calculate ACCUMULATED values for each entity with historical carryover
				For Each kvp As KeyValuePair(Of String, Integer) In allEntities
					Dim entity As String = kvp.Key
					
					' Create delegate for GetYearAccumulatedValues function
					Dim getYearFunc As Func(Of SessionInfo, DbConnInfo, String, String, Dictionary(Of String, Decimal)) = _
						Function(sessInfo As SessionInfo, dbConnInfo As DbConnInfo, entityName As String, year As String) As Dictionary(Of String, Decimal)
							Return Me.GetYearAccumulatedValues(sessInfo, dbConnInfo, entityName, year)
						End Function
					
					' Get historical accumulated values from all previous years for this specific entity
					Dim historicalValues As Dictionary(Of String, Decimal) = _
						Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetHistoricalAccumulatedValuesForEntity(si, paramYear, entity, getYearFunc)
					
					' Use the entity-specific key format
					Dim keyName As String = $"{entity}_OF_IF"
					Dim previousWeekAccumulated As Decimal = If(historicalValues.ContainsKey(keyName), historicalValues(keyName), 0)
					For weekNum As Integer = 1 To maxWeeks
						' Get this week's total flow from Actual data
						Dim weekTotal As Decimal = 0
						If weeklyTotals(entity).ContainsKey("Actual") AndAlso weeklyTotals(entity)("Actual").ContainsKey(weekNum) Then
							weekTotal = weeklyTotals(entity)("Actual")(weekNum)
						End If
						
						' Calculate current week accumulated = previous + current
						Dim currentWeekAccumulated As Decimal = previousWeekAccumulated + weekTotal
						
						' Store accumulated value for this entity and week
						entityData(entity)(weekNum) = currentWeekAccumulated
						
						' Update for next iteration
						previousWeekAccumulated = currentWeekAccumulated
					Next
				Next					' ====================================
					' STEP 5: Add entity rows with accumulated cashflow data
					' ====================================
					
					' Entities to exclude from first loop (shown after Total Horse Group)
					Dim entitiesToShowLater As String() = {"Aurobay Sweeden", "HPL", "Aurobay China"}
					
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
					Next
					
					' ====================================
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
					
					Dim formattedTotalValue As String = Me.FormatDecimalValue(totalValue)
					totalHorseGroupRow.Items.Add($"Week{weekNum}", New TableViewColumn() With {
						.Value = formattedTotalValue,
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
				Next
				
				tv.Rows.Add(totalHorseGroupRow)
					
					' ====================================
					' STEP 7: Add additional entity rows (Aurobay Sweeden, HPL, Aurobay China)
					' ====================================
					Dim additionalEntities As String() = {"Aurobay Sweeden", "HPL", "Aurobay China"}
					
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
					Next
					
					' Configure formatting
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
		
		#Region "Helper Functions"
		
		Private Function GetYearAccumulatedValues(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, ByVal paramEntity As String, ByVal targetYear As String) As Dictionary(Of String, Decimal)
			Try
				Dim result As New Dictionary(Of String, Decimal)
				Dim yearInt As Integer
				If Not Integer.TryParse(targetYear, yearInt) Then
					result($"{paramEntity}_OF_IF") = 0
					Return result
				End If
				
				' Get max weeks for the target year
				Dim maxWeeksSql As String = TRS_SQL_Repository.GetMaxWeekInYearQuery(targetYear)
				Dim maxWeeksDt As DataTable = BRApi.Database.ExecuteSql(dbConn, maxWeeksSql, False)
				Dim maxWeeks As Integer = 52 ' Default to 52
				If maxWeeksDt IsNot Nothing AndAlso maxWeeksDt.Rows.Count > 0 AndAlso Not IsDBNull(maxWeeksDt.Rows(0)("MaxWeek")) Then
					maxWeeks = Convert.ToInt32(maxWeeksDt.Rows(0)("MaxWeek"))
				End If
				
				Dim entityFilter As String = If(paramEntity <> "HTD", $"AND Entity = '{paramEntity}'", "")
				Dim yearSql As String = TRS_SQL_Repository.GetCashFlowYearEntityQuery(targetYear, entityFilter, maxWeeks, False)
				Dim yearDt As DataTable = BRApi.Database.ExecuteSql(dbConn, yearSql, False)
				Dim entityWeekData As New Dictionary(Of String, Dictionary(Of Integer, Decimal))
				
				If yearDt.Rows.Count = 0 Then
					result($"{paramEntity}_OF_IF") = 0
					Return result
				End If
				
				' Process data by entity and week
				For Each row As DataRow In yearDt.Rows
					Dim entity = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
					Dim weekNum = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
					Dim amount = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
					If Not entityWeekData.ContainsKey(entity) Then entityWeekData(entity) = New Dictionary(Of Integer, Decimal)
					If Not entityWeekData(entity).ContainsKey(weekNum) Then entityWeekData(entity)(weekNum) = 0
					entityWeekData(entity)(weekNum) += amount
				Next
				
				' Calculate accumulated values per entity
				For Each entityKvp In entityWeekData
					Dim entityName = entityKvp.Key
					Dim accOFIF As Decimal = 0
					For weekNum As Integer = 1 To maxWeeks
						If entityKvp.Value.ContainsKey(weekNum) Then
							accOFIF += entityKvp.Value(weekNum)
						End If
					Next
					result($"{entityName}_OF_IF") = accOFIF
				Next
				
				Return result
			Catch ex As Exception
				Return New Dictionary(Of String, Decimal) From {{$"{paramEntity}_OF_IF", 0}}
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
