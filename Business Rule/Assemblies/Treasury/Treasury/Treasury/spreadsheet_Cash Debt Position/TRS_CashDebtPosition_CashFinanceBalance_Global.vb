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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashDebtPosition_CashFinanceBalance_Global
	Public Class MainClass
		'Consolidated Global Report: ACTUAL (StartWeek) + END WEEK (EndWeek) + EOM
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = SpreadsheetFunctionType.Unknown
						Return Nothing
						
					Case Is = SpreadsheetFunctionType.GetCustomSubstVarsInUse
						Try
							Dim list As New List(Of String)
							list.Add("prm_Treasury_Year")
							list.Add("prm_Treasury_WeekNumber")
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return Me.GetConsolidatedGlobalReport(si, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						Return Nothing
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Consolidated Global Report"

		Private Function GetConsolidatedGlobalReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
			Try				
				' STEP 1: Get parameters from spreadsheet args
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
				
				' STEP 2: Validate required parameters
				If String.IsNullOrEmpty(paramYear) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Year parameter is missing")
				End If
				
				If String.IsNullOrEmpty(paramWeek) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Week parameter is missing")
				End If
				
				' STEP 3: Calculate TimeKey and other parameters
				Dim timeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear, paramWeek)
				
				' Validate paramWeek for column name usage
				Dim validWeek As Integer
				If Not Integer.TryParse(paramWeek, validWeek) Then
					validWeek = 1
				End If
				
				' Calculate expected scenario based on week number (FW + 2-digit week)
				Dim expectedScenario As String = "FW" + paramWeek.PadLeft(2, "0"c)
				
				' For EOM, get the month number based on the paramWeek
				Dim eomMonth As Integer = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMonthFromWeek(si, paramYear, validWeek)
				
				' Define entity groupings based on company dictionary
				' HTD: All entities EXCEPT HPL China (999), AUROBAY SWEDEN (998), Horse HPL (997)
				Dim htdEntities As String() = {"Horse Powertrain Solution", "Horse Spain", "OYAK HORSE", "Horse Brazil", 
				                                "Horse Chile", "Horse Argentina", "Horse Romania", "Horse Aveiro"}
				
				' Individual entity names
				Dim aurobayChina As String = "HPL China"
				Dim aurobaySweden As String = "AUROBAY SWEDEN"
				Dim jvHqLondon As String = "Horse HPL"
				Dim horseAveiro As String = "Horse Aveiro"
				Dim oyak As String = "OYAK HORSE"
				Dim romania As String = "Horse Romania"
				Dim brazil As String = "Horse Brazil"
				Dim chile As String = "Horse Chile"
				Dim argentina As String = "Horse Argentina"
				Dim spain As String = "Horse Spain"
				Dim holding As String = "Horse Powertrain Solution"
				
				' Calculate previous week
				Dim previousWeek As Integer = If(validWeek > 1, validWeek - 1, 1)
				
				' Calculate forecast week (paramWeek + 4 for EndWeek)
				Dim forecastWeek As Integer = validWeek + 4
				
				' Calculate previous EOM month
				Dim previousEomMonth As Integer = If(eomMonth > 1, eomMonth - 1, 12)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' STEP 4: Execute three separate queries for ACTUAL, END WEEK, and EOM
					Dim actualData As DataTable = Me.GetActualData(si, dbConn, timeKey, expectedScenario, validWeek, previousWeek)
					Dim endWeekData As DataTable = Me.GetEndWeekData(si, dbConn, timeKey, expectedScenario, forecastWeek, validWeek)
					Dim eomData As DataTable = Me.GetEOMData(si, dbConn, timeKey, expectedScenario, eomMonth, previousEomMonth)
					
					' STEP 5: Build consolidated TableView with all three tables
					Dim tv As New TableView()
					tv.CanModifyData = False
					
					' Add columns matching the Excel format (16 columns total)
					tv.Columns.Add(New TableViewColumn() With {.Name = "Description", .Value = "", .IsHeader = True})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HTD", .Value = "HTD (Europe, Latam)", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "AurobayChina", .Value = "Aurobay China (Shanghai HQ)", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "AurobaySweden", .Value = "Aurobay Sweden", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "JVHQLondon", .Value = "JV HQ London", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "Spacer1", .Value = "", .IsHeader = True})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseGroupTotal", .Value = "HORSE GROUP TOTAL", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "Spacer2", .Value = "", .IsHeader = True})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseAveiro", .Value = "Horse Aveiro", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseOYAK", .Value = "Horse Turkey OYAK", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseRomania", .Value = "Horse Romania", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseBrazil", .Value = "Horse Brazil", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseChile", .Value = "Horse Chile", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseArgentina", .Value = "Horse Argentina", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseSpain", .Value = "Horse Spain", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HorseHolding", .Value = "Horse Holding", .IsHeader = True, .DataType = XFDataType.Decimal})
					
					' Get formatted date for display
					Dim mondayDate As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMondayDateForWeek(si, paramYear, paramWeek).ToUpper()
					
					' Process and aggregate data for ACTUAL (includes previous week)
					Dim actualAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(actualData, "WeekValue", htdEntities, aurobayChina, aurobaySweden, jvHqLondon, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					Dim actualPreviousAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(actualData, "PreviousWeekValue", htdEntities, aurobayChina, aurobaySweden, jvHqLondon, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					
					' Process and aggregate data for END WEEK (includes start week for comparison)
					Dim endWeekAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(endWeekData, "WeekValue", htdEntities, aurobayChina, aurobaySweden, jvHqLondon, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					Dim endWeekStartAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(endWeekData, "StartWeekValue", htdEntities, aurobayChina, aurobaySweden, jvHqLondon, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					
					' Process and aggregate data for EOM (includes previous month)
					Dim eomAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(eomData, "EOMValue", htdEntities, aurobayChina, aurobaySweden, jvHqLondon, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					Dim eomPreviousAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(eomData, "PreviousEOMValue", htdEntities, aurobayChina, aurobaySweden, jvHqLondon, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					
				' Build the three tables stacked vertically
				Me.BuildActualTable(tv, actualAggregated, actualPreviousAggregated, mondayDate, paramWeek)
				Me.AddBlankRows(tv, 2)
				
				Me.BuildEndWeekTable(tv, endWeekAggregated, endWeekStartAggregated, mondayDate, paramWeek, timeKey, validWeek)
				Me.AddBlankRows(tv, 2)
				
				Me.BuildEOMTable(tv, eomAggregated, eomPreviousAggregated, mondayDate, paramWeek, timeKey, eomMonth)					' Configure formatting
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
		
		#Region "SQL Queries - Three Separate Queries"

		Private Function GetActualData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, _
		                                ByVal timeKey As String, ByVal scenario As String, _
		                                ByVal validWeek As Integer, ByVal previousWeek As Integer) As DataTable
			Try
				' Calculate previous week timekey and scenario
				Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
				Dim previousDate As DateTime = currentDate.AddDays(-7)
				Dim previousTimeKey As String = previousDate.ToString("yyyyMMdd")
				Dim previousScenario As String = "FW" + previousWeek.ToString().PadLeft(2, "0"c)
				
				' Use centralized SQL repository (no entity filter for Global report)
				Dim sql As String = TRS_SQL_Repository.GetCashDebtActualWithPreviousWeekQuery(timeKey, scenario, previousTimeKey, previousScenario, "")
				
				Return BRApi.Database.ExecuteSql(dbConn, sql, False)
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		Private Function GetEndWeekData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, _
		                                 ByVal timeKey As String, ByVal scenario As String, _
		                                 ByVal forecastWeek As Integer, ByVal validWeek As Integer) As DataTable
			Try
				' Calculate forecast timekey (timekey + 28 days = 4 weeks)
				Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
				Dim forecastDate As DateTime = currentDate.AddDays(28)
				Dim forecastTimeKey As String = forecastDate.ToString("yyyyMMdd")
				
				' Use centralized SQL repository (no entity filter for Global report)
				Dim sql As String = TRS_SQL_Repository.GetCashDebtEndWeekWithStartWeekQuery(timeKey, scenario, forecastTimeKey, "")
				
				Return BRApi.Database.ExecuteSql(dbConn, sql, False)
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		Private Function GetEOMData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, _
		                             ByVal timeKey As String, ByVal scenario As String, _
		                             ByVal eomMonth As Integer, ByVal previousEomMonth As Integer) As DataTable
			Try
				' Calculate previous week timekey and scenario for EOM comparison
				Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
				Dim previousDate As DateTime = currentDate.AddDays(-7)
				Dim previousTimeKey As String = previousDate.ToString("yyyyMMdd")
				
				' Extract week number from scenario (FW50 -> 50)
				Dim weekNum As Integer = Integer.Parse(scenario.Substring(2))
				Dim previousWeek As Integer = If(weekNum > 1, weekNum - 1, 1)
				Dim previousScenario As String = "FW" + previousWeek.ToString().PadLeft(2, "0"c)
				
				' Use centralized SQL repository (no entity filter for Global report)
				Dim sql As String = TRS_SQL_Repository.GetCashDebtEOMWithPreviousQuery(timeKey, scenario, eomMonth, previousTimeKey, previousScenario, "")
				
				Return BRApi.Database.ExecuteSql(dbConn, sql, False)
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Data Aggregation by Entity Groups"

		Private Function AggregateDataByEntityWithWeekValues(ByVal data As DataTable, _
		                                                       ByVal valueColumnName As String, _
		                                                       ByVal htdEntities As String(), _
		                                                       ByVal aurobayChina As String, ByVal aurobaySweden As String, ByVal jvHqLondon As String, _
		                                                       ByVal horseAveiro As String, ByVal oyak As String, ByVal romania As String, _
		                                                       ByVal brazil As String, ByVal chile As String, ByVal argentina As String, _
		                                                       ByVal spain As String, ByVal holding As String) _
		                                                       As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal)))
			Try
				' Structure: Account -> Flow -> Entity -> Amount
				Dim result As New Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal)))
				
				For Each dr As DataRow In data.Rows
					Dim entity As String = If(IsDBNull(dr("Entity")), "", dr("Entity").ToString().Trim())
					Dim account As String = If(IsDBNull(dr("Account")), "", dr("Account").ToString())
					Dim flow As String = If(IsDBNull(dr("Flow")), "", dr("Flow").ToString())
					
					' Extract value from the specified column (WeekValue, PreviousWeekValue, EOMValue, or PreviousEOMValue)
					Dim amount As Decimal = 0
					If data.Columns.Contains(valueColumnName) AndAlso Not IsDBNull(dr(valueColumnName)) Then
						amount = Convert.ToDecimal(dr(valueColumnName))
					End If
					
					' Initialize account dictionary if needed
					If Not result.ContainsKey(account) Then
						result(account) = New Dictionary(Of String, Dictionary(Of String, Decimal))
					End If
					
					' Initialize flow dictionary if needed
					If Not result(account).ContainsKey(flow) Then
						result(account)(flow) = New Dictionary(Of String, Decimal) From {
							{"HTD", 0}, {"AurobayChina", 0}, {"AurobaySweden", 0}, {"JVHQLondon", 0},
							{"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
							{"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
						}
					End If
					
					' Normalize entity name for comparison (case-insensitive)
					Dim entityUpper As String = entity.ToUpper()
					
					' Aggregate values by entity group
					If htdEntities.Any(Function(e) e.ToUpper() = entityUpper) Then
						result(account)(flow)("HTD") += amount
						
						' Also map to individual entities (case-insensitive comparison)
						If String.Equals(entity, horseAveiro, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseAveiro") += amount
						If String.Equals(entity, oyak, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseOYAK") += amount
						If String.Equals(entity, romania, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseRomania") += amount
						If String.Equals(entity, brazil, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseBrazil") += amount
						If String.Equals(entity, chile, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseChile") += amount
						If String.Equals(entity, argentina, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseArgentina") += amount
						If String.Equals(entity, spain, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseSpain") += amount
						If String.Equals(entity, holding, StringComparison.OrdinalIgnoreCase) Then result(account)(flow)("HorseHolding") += amount
						
					ElseIf String.Equals(entity, aurobayChina, StringComparison.OrdinalIgnoreCase) Then
						result(account)(flow)("AurobayChina") += amount
					ElseIf String.Equals(entity, aurobaySweden, StringComparison.OrdinalIgnoreCase) Then
						result(account)(flow)("AurobaySweden") += amount
					ElseIf String.Equals(entity, jvHqLondon, StringComparison.OrdinalIgnoreCase) Then
						result(account)(flow)("JVHQLondon") += amount
					End If
				Next
				
				Return result
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		#End Region
		
		#Region "Build Three Separate Tables"

		' ========== TABLE 1: ACTUAL (StartWeek) ==========
		Private Sub BuildActualTable(ByVal tv As TableView, _
		                              ByVal actualData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))), _
		                              ByVal actualPreviousData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))), _
		                              ByVal mondayDate As String, ByVal paramWeek As String)
			Try
				' Add header rows for ACTUAL table (4 header rows total)
				Me.AddActualHeaderRows(tv, mondayDate, paramWeek)
				
				' ========== ACTUAL TABLE - Section 1: CASH AND FINANCING BALANCE ==========
				Dim cashTotalsCurrent As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "CASH AND FINANCING BALANCE", actualData.GetValueOrDefault("CASH AND FINANCING BALANCE"), "Cash available")
				
				' Add blank row before Flow comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "Flow to last week" comparison row
				Dim cashTotalsPrevious As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(actualPreviousData, "CASH AND FINANCING BALANCE")
				Me.AddFlowToLastWeekRow(tv, "Flow to last week", cashTotalsCurrent, cashTotalsPrevious)
				Me.AddBlankRows(tv, 1)
				
				' ========== ACTUAL TABLE - Section 2: FINANCING - USED LINES ==========
				Dim financingTotalsCurrent As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "FINANCING - USED LINES", actualData.GetValueOrDefault("FINANCING - USED LINES"), "Utilized debt")
				
				' Add blank row before Flow comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "Flow to last week" comparison row
				Dim financingTotalsPrevious As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(actualPreviousData, "FINANCING - USED LINES")
				Me.AddFlowToLastWeekRow(tv, "Flow to last week", financingTotalsCurrent, financingTotalsPrevious)
				Me.AddBlankRows(tv, 1)
				
				' ========== ACTUAL TABLE - Section 3: FINANCING - AVAILABLE LINES ==========
				Dim availableTotalsCurrent As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "FINANCING - AVAILABLE LINES", actualData.GetValueOrDefault("FINANCING - AVAILABLE LINES"), "Available financing")
				
				' Add blank row before Flow comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "Flow to last week" comparison row
				Dim availableTotalsPrevious As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(actualPreviousData, "FINANCING - AVAILABLE LINES")
				Me.AddFlowToLastWeekRow(tv, "Flow to last week", availableTotalsCurrent, availableTotalsPrevious)
				Me.AddBlankRows(tv, 1)
				
				' ========== ACTUAL TABLE - NET FUNDING POSITION ==========
				Dim netFundingCurrent As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
				Dim netFundingPrevious As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
				
				For Each colName As String In cashTotalsCurrent.Keys
					netFundingCurrent(colName) = cashTotalsCurrent(colName) - financingTotalsCurrent(colName)
					netFundingPrevious(colName) = cashTotalsPrevious(colName) - financingTotalsPrevious(colName)
				Next
				
				Me.AddTotalRow(tv, "Net Funding position", netFundingCurrent)
				Me.AddFlowToLastWeekRow(tv, "Flow to last week", netFundingCurrent, netFundingPrevious)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		' ========== HEADER FUNCTIONS FOR EACH TABLE TYPE ==========
		
		Private Sub AddActualHeaderRows(ByVal tv As TableView, ByVal mondayDate As String, ByVal paramWeek As String)
			Try
				' Fila 1: "ACTUAL" en Description
				Dim mainHeaderRow As New TableViewRow() With {.IsHeader = True}
				mainHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				mainHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "" , .IsHeader = True})
				mainHeaderRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "Start Of Period " & mondayDate, .IsHeader = True})
				mainHeaderRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(mainHeaderRow)
				
				' Fila 2: NUEVA - "WEEK X - Start of DD-MMM" en Description
				Dim weekDateRow As New TableViewRow() With {.IsHeader = True}
				weekDateRow.Items.Add("Description", New TableViewColumn() With {.Value = "WEEK " & paramWeek & " - Start of " & mondayDate, .IsHeader = True})
				weekDateRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(weekDateRow)
				
				' Fila 3: "mil EUR"
				Dim milEurHeaderRow As New TableViewRow() With {.IsHeader = True}
				milEurHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "mil EUR", .IsHeader = True})
				For i As Integer = 1 To 15
					milEurHeaderRow.Items.Add(tv.Columns(i).Name, New TableViewColumn() With {.Value = "", .IsHeader = True})
				Next
				tv.Rows.Add(milEurHeaderRow)
				
				' Fila 4: NUEVA - Nombres de entidades en cada columna
				Dim entityNamesRow As New TableViewRow() With {.IsHeader = True}
				entityNamesRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HTD", New TableViewColumn() With {.Value = "HTD (Europe, Latam)", .IsHeader = True})
				entityNamesRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "China (Shanghai HQ)", .IsHeader = True})
				entityNamesRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "Aurobay Sweden", .IsHeader = True})
				entityNamesRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "JV HQ London", .IsHeader = True})
				entityNamesRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "HORSE GROUP TOTAL", .IsHeader = True})
				entityNamesRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "Horse Aveiro", .IsHeader = True})
				entityNamesRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "Horse Turkey OYAK", .IsHeader = True})
				entityNamesRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "Horse Romania", .IsHeader = True})
				entityNamesRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "Horse Brazil", .IsHeader = True})
				entityNamesRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "Horse Chile", .IsHeader = True})
				entityNamesRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "Horse Argentina", .IsHeader = True})
				entityNamesRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "Horse Spain", .IsHeader = True})
				entityNamesRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "Horse Holding", .IsHeader = True})
				tv.Rows.Add(entityNamesRow)
				
				' Fila 5: "ACTUAL" en todas las columnas de datos (antes era Fila 4)
				Dim projectionHeaderRow As New TableViewRow() With {.IsHeader = True}
				projectionHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "ACTUAL", .IsHeader = True})
				tv.Rows.Add(projectionHeaderRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Function AddSectionToTable(ByVal tv As TableView, ByVal sectionTitle As String, _
		                               ByVal accountData As Dictionary(Of String, Dictionary(Of String, Decimal)), _
		                               ByVal totalLabel As String) As Dictionary(Of String, Decimal)
			Try
				' Add section header row with title
				Dim sectionHeaderRow As New TableViewRow()
				sectionHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = sectionTitle, .IsHeader = False})
				For i As Integer = 1 To 15
					sectionHeaderRow.Items.Add(tv.Columns(i).Name, New TableViewColumn() With {.Value = "", .IsHeader = False})
				Next
				tv.Rows.Add(sectionHeaderRow)
				
				' Initialize totals for Internal and External
				Dim internalTotals As New Dictionary(Of String, Decimal) From {
					{"HTD", 0}, {"AurobayChina", 0}, {"AurobaySweden", 0}, {"JVHQLondon", 0}, {"HorseGroupTotal", 0},
					{"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
					{"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
				}
				
				Dim externalTotals As New Dictionary(Of String, Decimal) From {
					{"HTD", 0}, {"AurobayChina", 0}, {"AurobaySweden", 0}, {"JVHQLondon", 0}, {"HorseGroupTotal", 0},
					{"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
					{"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
				}
				
			' Define mandatory flow rows based on section title
			Dim mandatoryFlows As New List(Of String)
			If sectionTitle = "CASH AND FINANCING BALANCE" Then
				mandatoryFlows.Add("INTernal Cash (+)")
				mandatoryFlows.Add("EXTernal Cash (+)")
			ElseIf sectionTitle = "FINANCING - USED LINES" Then
				mandatoryFlows.Add("INTernal Debt (-)")
				mandatoryFlows.Add("EXTernal Debt (-)")
			ElseIf sectionTitle = "FINANCING - AVAILABLE LINES" Then
				mandatoryFlows.Add("INTernal Debt (-)")
				mandatoryFlows.Add("EXTernal Debt (-)")
			End If				' Define column names
				Dim columnNames As String() = {"HTD", "AurobayChina", "AurobaySweden", "JVHQLondon", "Spacer1", "HorseGroupTotal", "Spacer2",
				                                 "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", 
				                                 "HorseArgentina", "HorseSpain", "HorseHolding"}
				
				' Add rows for MANDATORY flows (always visible, even with zeros)
				For Each flow As String In mandatoryFlows
					Dim flowRow As New TableViewRow()
					flowRow.Items.Add("Description", New TableViewColumn() With {.Value = flow, .IsHeader = False})
					
					' Get flow data if exists, otherwise use zeros
					Dim flowData As Dictionary(Of String, Decimal) = Nothing
					If accountData IsNot Nothing AndAlso accountData.ContainsKey(flow) Then
						flowData = accountData(flow)
					End If
					
				For Each colName As String In columnNames
					' Handle spacer columns
					If colName = "Spacer1" OrElse colName = "Spacer2" Then
						flowRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
						Continue For
					End If
					
					' Get value (0 if no data)
					Dim value As Decimal = 0
					If flowData IsNot Nothing AndAlso flowData.ContainsKey(colName) Then
						value = flowData(colName)
					End If
					
					' Special handling for HorseGroupTotal: Calculate as sum of the four main groups
					If colName = "HorseGroupTotal" Then
						value = 0
						If flowData IsNot Nothing Then
							value = If(flowData.ContainsKey("HTD"), flowData("HTD"), 0) + _
							        If(flowData.ContainsKey("AurobayChina"), flowData("AurobayChina"), 0) + _
							        If(flowData.ContainsKey("AurobaySweden"), flowData("AurobaySweden"), 0) + _
							        If(flowData.ContainsKey("JVHQLondon"), flowData("JVHQLondon"), 0)
						End If
					End If
					
					' Accumulate subtotals
					If flow.StartsWith("INTernal") Then
						internalTotals(colName) += value
					ElseIf flow.StartsWith("EXTernal") Then
						externalTotals(colName) += value
					End If
					
					flowRow.Items.Add(colName, New TableViewColumn() With {.Value = value.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
				Next					
				tv.Rows.Add(flowRow)
				Next
				
				' Add rows for any ADDITIONAL flows not in mandatory list (if they exist)
				If accountData IsNot Nothing Then
					' Filter additional flows based on section type
					Dim expectedSign As String = ""
					If sectionTitle = "CASH AND FINANCING BALANCE" Then
						expectedSign = "(+)"
					ElseIf sectionTitle = "FINANCING - USED LINES" OrElse sectionTitle = "FINANCING - AVAILABLE LINES" Then
						expectedSign = "(-)"
					End If
					
					' Only include flows that match the expected sign AND are not in mandatory list
					Dim additionalFlows = accountData.Keys.Where(Function(f) Not mandatoryFlows.Contains(f) AndAlso f.Contains(expectedSign)).OrderBy(Function(f) f)
					
					For Each flow As String In additionalFlows
						Dim flowRow As New TableViewRow()
						flowRow.Items.Add("Description", New TableViewColumn() With {.Value = flow, .IsHeader = False})
						
						Dim flowData As Dictionary(Of String, Decimal) = accountData(flow)
						
						For Each colName As String In columnNames
							' Handle spacer columns
							If colName = "Spacer1" OrElse colName = "Spacer2" Then
								flowRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
								Continue For
							End If
							
							Dim value As Decimal = If(flowData.ContainsKey(colName), flowData(colName), 0)
							
							' Special handling for HorseGroupTotal: Calculate as sum of the four main groups
							If colName = "HorseGroupTotal" Then
								value = If(flowData.ContainsKey("HTD"), flowData("HTD"), 0) + _
								        If(flowData.ContainsKey("AurobayChina"), flowData("AurobayChina"), 0) + _
								        If(flowData.ContainsKey("AurobaySweden"), flowData("AurobaySweden"), 0) + _
								        If(flowData.ContainsKey("JVHQLondon"), flowData("JVHQLondon"), 0)
							End If
							
							' Accumulate subtotals
							If flow.StartsWith("INTernal") Then
								internalTotals(colName) += value
							ElseIf flow.StartsWith("EXTernal") Then
								externalTotals(colName) += value
							End If
							
							flowRow.Items.Add(colName, New TableViewColumn() With {.Value = value.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
						Next
						
						tv.Rows.Add(flowRow)
					Next
				End If
				
			' Calculate Horse Group Total as sum of the four main groups
			For Each colName As String In internalTotals.Keys.ToList()
				If colName = "HorseGroupTotal" Then
					internalTotals("HorseGroupTotal") = internalTotals("HTD") + internalTotals("AurobayChina") + 
					                                     internalTotals("AurobaySweden") + internalTotals("JVHQLondon")
				End If
			Next
			
			For Each colName As String In externalTotals.Keys.ToList()
				If colName = "HorseGroupTotal" Then
					externalTotals("HorseGroupTotal") = externalTotals("HTD") + externalTotals("AurobayChina") + 
					                                     externalTotals("AurobaySweden") + externalTotals("JVHQLondon")
				End If
			Next
			
			' Calculate grand totals
			' For FINANCING - AVAILABLE LINES: use only EXTernal (not INTernal + EXTernal)
			' For other sections: use INTernal + EXTernal
			Dim grandTotals As New Dictionary(Of String, Decimal)
			For Each colName In internalTotals.Keys
				If sectionTitle = "FINANCING - AVAILABLE LINES" Then
					grandTotals(colName) = externalTotals(colName)
				Else
					grandTotals(colName) = internalTotals(colName) + externalTotals(colName)
				End If
			Next
			
			' Add total row immediately after flows (no blank row)
			Me.AddTotalRow(tv, totalLabel, grandTotals)
			
			' Return totals for Flow comparison
			Return grandTotals
			Catch ex As Exception
				Throw ex
			End Try
		End Function

	Private Sub AddTotalRow(ByVal tv As TableView, ByVal description As String, ByVal totals As Dictionary(Of String, Decimal))
		Try
			Dim totalRow As New TableViewRow()
			totalRow.Items.Add("Description", New TableViewColumn() With {.Value = description, .IsHeader = False})
			
			Dim columnNames As String() = {"HTD", "AurobayChina", "AurobaySweden", "JVHQLondon", "Spacer1", "HorseGroupTotal", "Spacer2",
			                                 "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", 
			                                 "HorseArgentina", "HorseSpain", "HorseHolding"}
			
			For Each colName As String In columnNames
				' Handle spacer columns
				If colName = "Spacer1" OrElse colName = "Spacer2" Then
				totalRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
				Continue For
			End If
			
			Dim value As Decimal = If(totals.ContainsKey(colName), totals(colName), 0)
			totalRow.Items.Add(colName, New TableViewColumn() With {.Value = value.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
		Next
		
		tv.Rows.Add(totalRow)
		Catch ex As Exception
			Throw ex
		End Try
	End Sub

		Private Sub AddBlankRows(ByVal tv As TableView, ByVal count As Integer)
			Try
				For i As Integer = 1 To count
					Dim blankRow As New TableViewRow()
					For Each col As TableViewColumn In tv.Columns
						blankRow.Items.Add(col.Name, New TableViewColumn() With {.Value = "", .IsHeader = False})
					Next
					tv.Rows.Add(blankRow)
				Next
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Function CalculateSectionTotals(ByVal data As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))), _
		                                         ByVal accountName As String) As Dictionary(Of String, Decimal)
			Try
				Dim totals As New Dictionary(Of String, Decimal) From {
					{"HTD", 0}, {"AurobayChina", 0}, {"AurobaySweden", 0}, {"JVHQLondon", 0}, {"HorseGroupTotal", 0},
					{"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
					{"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
				}
				
				If Not data.ContainsKey(accountName) Then
					Return totals
				End If
				
				Dim accountData As Dictionary(Of String, Dictionary(Of String, Decimal)) = data(accountName)
				
				For Each flow As String In accountData.Keys
					Dim flowData As Dictionary(Of String, Decimal) = accountData(flow)
					
					For Each colName As String In totals.Keys.ToList()
						If flowData.ContainsKey(colName) Then
							totals(colName) += flowData(colName)
						End If
					Next
				Next
				
				' Calculate Horse Group Total as sum of the four main groups
				totals("HorseGroupTotal") = totals("HTD") + totals("AurobayChina") + totals("AurobaySweden") + totals("JVHQLondon")
				
				Return totals
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		Private Function CalculateSectionTotals_ForFlux(ByVal accountData As Dictionary(Of String, Dictionary(Of String, Decimal))) As Dictionary(Of String, Decimal)
			Try
				Dim totals As New Dictionary(Of String, Decimal) From {
					{"HTD", 0}, {"AurobayChina", 0}, {"AurobaySweden", 0}, {"JVHQLondon", 0}, {"HorseGroupTotal", 0},
					{"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
					{"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
				}
				
				If accountData Is Nothing Then
					Return totals
				End If
				
				For Each flow As String In accountData.Keys
					Dim flowData As Dictionary(Of String, Decimal) = accountData(flow)
					
					For Each colName As String In totals.Keys.ToList()
						If flowData.ContainsKey(colName) Then
							totals(colName) += flowData(colName)
						End If
					Next
				Next
				
				' Calculate Horse Group Total as sum of the four main groups
				totals("HorseGroupTotal") = totals("HTD") + totals("AurobayChina") + totals("AurobaySweden") + totals("JVHQLondon")
				
				Return totals
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		' Add "Flow to last week" comparison row
		Private Sub AddFlowToLastWeekRow(ByVal tv As TableView, ByVal label As String, _
		                                  ByVal currentTotals As Dictionary(Of String, Decimal), _
		                                  ByVal previousTotals As Dictionary(Of String, Decimal))
			Try
				Dim flowRow As New TableViewRow()
				flowRow.Items.Add("Description", New TableViewColumn() With {.Value = label, .IsHeader = False})
				
				Dim columnNames As String() = {"HTD", "AurobayChina", "AurobaySweden", "JVHQLondon", "Spacer1", "HorseGroupTotal", "Spacer2",
				                                 "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", 
				                                 "HorseArgentina", "HorseSpain", "HorseHolding"}
				
				For Each colName As String In columnNames
					' Handle spacer columns
					If colName = "Spacer1" OrElse colName = "Spacer2" Then
						flowRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
						Continue For
					End If
					
					' Calculate difference (current - previous)
					Dim currentValue As Decimal = If(currentTotals.ContainsKey(colName), currentTotals(colName), 0)
					Dim previousValue As Decimal = If(previousTotals.ContainsKey(colName), previousTotals(colName), 0)
					Dim difference As Decimal = currentValue - previousValue
					
					flowRow.Items.Add(colName, New TableViewColumn() With {.Value = difference.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False})
				Next
				
				tv.Rows.Add(flowRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		' ========== TABLE 2: END WEEK (EndWeek Forecast) ==========
		Private Sub BuildEndWeekTable(ByVal tv As TableView, _
		                               ByVal endWeekData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))), _
		                               ByVal endWeekStartData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))), _
		                               ByVal mondayDate As String, ByVal paramWeek As String, _
		                               ByVal timeKey As String, ByVal validWeek As Integer)
			Try
				' Add header rows for END WEEK table
				Me.AddEndWeekHeaderRows(tv, mondayDate, paramWeek, timeKey, validWeek)
				
				' ========== END WEEK TABLE - Section 1: CASH AND FINANCING BALANCE ==========
				Dim cashTotalsForecast As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "CASH AND FINANCING BALANCE", endWeekData.GetValueOrDefault("CASH AND FINANCING BALANCE"), "Cash available")
				
				' Add blank row before Flow comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "Flow to Forecast start" comparison row
				Dim cashTotalsStart As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(endWeekStartData, "CASH AND FINANCING BALANCE")
				Me.AddFlowToLastWeekRow(tv, "Flow to Forecast start of " & mondayDate, cashTotalsForecast, cashTotalsStart)
				Me.AddBlankRows(tv, 1)
				
				' ========== END WEEK TABLE - Section 2: FINANCING - USED LINES ==========
				Dim financingTotalsForecast As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "FINANCING - USED LINES", endWeekData.GetValueOrDefault("FINANCING - USED LINES"), "Utilized debt")
				
				' Add blank row before Flow comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "Flow to Forecast start" comparison row
				Dim financingTotalsStart As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(endWeekStartData, "FINANCING - USED LINES")
				Me.AddFlowToLastWeekRow(tv, "Flow to Forecast start of " & mondayDate, financingTotalsForecast, financingTotalsStart)
				Me.AddBlankRows(tv, 1)
				
				' ========== END WEEK TABLE - Section 3: FINANCING - AVAILABLE LINES ==========
				Dim availableTotalsForecast As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "FINANCING - AVAILABLE LINES", endWeekData.GetValueOrDefault("FINANCING - AVAILABLE LINES"), "Available financing")
				
				' Add blank row before Flow comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "Flow to Forecast start" comparison row
				Dim availableTotalsStart As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(endWeekStartData, "FINANCING - AVAILABLE LINES")
				Me.AddFlowToLastWeekRow(tv, "Flow to Forecast start of " & mondayDate, availableTotalsForecast, availableTotalsStart)
				Me.AddBlankRows(tv, 1)
				
				' ========== END WEEK TABLE - NET FUNDING POSITION ==========
				Dim netFundingForecast As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
				Dim netFundingStart As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
				
				For Each colName As String In cashTotalsForecast.Keys
					netFundingForecast(colName) = cashTotalsForecast(colName) - financingTotalsForecast(colName)
					netFundingStart(colName) = cashTotalsStart(colName) - financingTotalsStart(colName)
				Next
				
				Me.AddTotalRow(tv, "Net Funding position", netFundingForecast)
				Me.AddFlowToLastWeekRow(tv, "Flow to Forecast start of " & mondayDate, netFundingForecast, netFundingStart)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Sub AddEndWeekHeaderRows(ByVal tv As TableView, ByVal mondayDate As String, ByVal paramWeek As String, _
		                                  ByVal timeKey As String, ByVal validWeek As Integer)
			Try
				' Calcular forecastWeek y endWeekDate
				Dim forecastWeek As Integer = validWeek + 4
				Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
				Dim endWeekDate As DateTime = currentDate.AddDays(28)
				Dim endWeekFormatted As String = endWeekDate.ToString("dd-MMM", New CultureInfo("es-ES")).ToUpper()
				
				' Fila 1: "ACTUAL" en Description (misma estructura que AddActualHeaderRows)
				Dim mainHeaderRow As New TableViewRow() With {.IsHeader = True}
				mainHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "END WEEK FORECAST - 4 WEEKS LATER", .IsHeader = True})
				mainHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "Start Of Period " & endWeekFormatted, .IsHeader = True})
				mainHeaderRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(mainHeaderRow)
				' Fila 2: NUEVA - "WEEK X - End Week Forecast DD-MMM" en Description
				Dim weekDateRow As New TableViewRow() With {.IsHeader = True}
				weekDateRow.Items.Add("Description", New TableViewColumn() With {.Value = "WEEK "  & paramWeek & " - Start of " & endWeekFormatted, .IsHeader = True})
				weekDateRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(weekDateRow)
				
				' Fila 3: "mil EUR"
				Dim milEurHeaderRow As New TableViewRow() With {.IsHeader = True}
				milEurHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "mil EUR", .IsHeader = True})
				For i As Integer = 1 To 15
					milEurHeaderRow.Items.Add(tv.Columns(i).Name, New TableViewColumn() With {.Value = "", .IsHeader = True})
				Next
				tv.Rows.Add(milEurHeaderRow)
				
				' Fila 4: NUEVA - Nombres de entidades en cada columna
				Dim entityNamesRow As New TableViewRow() With {.IsHeader = True}
				entityNamesRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HTD", New TableViewColumn() With {.Value = "HTD (Europe, Latam)", .IsHeader = True})
				entityNamesRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "China (Shanghai HQ)", .IsHeader = True})
				entityNamesRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "Aurobay Sweden", .IsHeader = True})
				entityNamesRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "JV HQ London", .IsHeader = True})
				entityNamesRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "HORSE GROUP TOTAL", .IsHeader = True})
				entityNamesRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "Horse Aveiro", .IsHeader = True})
				entityNamesRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "Horse Turkey OYAK", .IsHeader = True})
				entityNamesRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "Horse Romania", .IsHeader = True})
				entityNamesRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "Horse Brazil", .IsHeader = True})
				entityNamesRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "Horse Chile", .IsHeader = True})
				entityNamesRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "Horse Argentina", .IsHeader = True})
				entityNamesRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "Horse Spain", .IsHeader = True})
				entityNamesRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "Horse Holding", .IsHeader = True})
				tv.Rows.Add(entityNamesRow)
				
				' Fila 5: "FORECAST" en todas las columnas de datos (antes era Fila 4)
				Dim projectionHeaderRow As New TableViewRow() With {.IsHeader = True}
				projectionHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				tv.Rows.Add(projectionHeaderRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		' ========== TABLE 3: EOM (End of Month Forecast) ==========
		Private Sub BuildEOMTable(ByVal tv As TableView, _
		                           ByVal eomData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))), _
		                           ByVal eomPreviousData As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))), _
		                           ByVal mondayDate As String, ByVal paramWeek As String, _
		                           ByVal timeKey As String, ByVal eomMonth As Integer)
			Try
				' Add header rows for EOM table
				Me.AddEOMHeaderRows(tv, mondayDate, paramWeek, timeKey, eomMonth)
				
				' ========== EOM TABLE - Section 1: CASH AND FINANCING BALANCE ==========
				Dim cashTotalsCurrent As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "CASH AND FINANCING BALANCE", eomData.GetValueOrDefault("CASH AND FINANCING BALANCE"), "Cash available")
				
				' Add blank row before FLUX comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "FLUX to Forecast start" comparison row
				Dim cashTotalsPrevious As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(eomPreviousData, "CASH AND FINANCING BALANCE")
				Me.AddFluxRow(tv, "FLUX to Forecast start of " & mondayDate, cashTotalsCurrent, cashTotalsPrevious)
				Me.AddBlankRows(tv, 1)
				
				' ========== EOM TABLE - Section 2: FINANCING - USED LINES ==========
				Dim financingTotalsCurrent As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "FINANCING - USED LINES", eomData.GetValueOrDefault("FINANCING - USED LINES"), "Utilized debt")
				
				' Add blank row before FLUX comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "FLUX to Forecast start" comparison row
				Dim financingTotalsPrevious As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(eomPreviousData, "FINANCING - USED LINES")
				Me.AddFluxRow(tv, "FLUX to Forecast start of " & mondayDate, financingTotalsCurrent, financingTotalsPrevious)
				Me.AddBlankRows(tv, 1)
				
				' ========== EOM TABLE - Section 3: FINANCING - AVAILABLE LINES ==========
				Dim availableTotalsCurrent As Dictionary(Of String, Decimal) = Me.AddSectionToTable(tv, "FINANCING - AVAILABLE LINES", eomData.GetValueOrDefault("FINANCING - AVAILABLE LINES"), "Available financing")
				
				' Add blank row before FLUX comparison
				Me.AddBlankRows(tv, 1)
				
				' Calculate and add "FLUX to Forecast start" comparison row
				Dim availableTotalsPrevious As Dictionary(Of String, Decimal) = Me.CalculateSectionTotals(eomPreviousData, "FINANCING - AVAILABLE LINES")
				Me.AddFluxRow(tv, "FLUX to Forecast start of " & mondayDate, availableTotalsCurrent, availableTotalsPrevious)
				Me.AddBlankRows(tv, 1)
				
				' ========== EOM TABLE - NET FINANCING POSITION ==========
				Dim netFinancingCurrent As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
				Dim netFinancingPrevious As Dictionary(Of String, Decimal) = New Dictionary(Of String, Decimal)
				
				For Each colName As String In cashTotalsCurrent.Keys
					netFinancingCurrent(colName) = cashTotalsCurrent(colName) - financingTotalsCurrent(colName)
					netFinancingPrevious(colName) = cashTotalsPrevious(colName) - financingTotalsPrevious(colName)
				Next
				
				Me.AddTotalRow(tv, "Net Financing Position", netFinancingCurrent)
				Me.AddFluxRow(tv, "FLUX to Forecast start of " & mondayDate, netFinancingCurrent, netFinancingPrevious)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Sub AddEOMHeaderRows(ByVal tv As TableView, ByVal mondayDate As String, ByVal paramWeek As String, _
		                              ByVal timeKey As String, ByVal eomMonth As Integer)
			Try
				' Calcular la fecha de fin de mes
				Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
				Dim eomDate As New DateTime(currentDate.Year, eomMonth, DateTime.DaysInMonth(currentDate.Year, eomMonth))
				Dim eomFormatted As String = eomDate.ToString("dd-MMM", New CultureInfo("es-ES")).ToUpper()
				
				' Fila 1: "ACTUAL" en Description (misma estructura que AddActualHeaderRows)
				Dim mainHeaderRow As New TableViewRow() With {.IsHeader = True}
				mainHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "END OF MONTH", .IsHeader = True})
				mainHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "Start Of Period " & eomFormatted, .IsHeader = True})
				mainHeaderRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(mainHeaderRow)
				
				' Fila 2: "WEEK X - End of Month Forecast DD-MMM" en Description
				Dim eomHeaderRow As New TableViewRow() With {.IsHeader = True}
				eomHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "WEEK " & paramWeek & " - Start Of " & eomFormatted, .IsHeader = True})
				eomHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "", .IsHeader = True})
				eomHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(eomHeaderRow)
				
				' Fila 3: "mil EUR"
				Dim milEurHeaderRow As New TableViewRow() With {.IsHeader = True}
				milEurHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "mil EUR", .IsHeader = True})
				For i As Integer = 1 To 15
					milEurHeaderRow.Items.Add(tv.Columns(i).Name, New TableViewColumn() With {.Value = "", .IsHeader = True})
				Next
				tv.Rows.Add(milEurHeaderRow)
				
				' Fila 4: NUEVA - Nombres de entidades en cada columna
				Dim entityNamesRow As New TableViewRow() With {.IsHeader = True}
				entityNamesRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HTD", New TableViewColumn() With {.Value = "HTD (Europe, Latam)", .IsHeader = True})
				entityNamesRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "China (Shanghai HQ)", .IsHeader = True})
				entityNamesRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "Aurobay Sweden", .IsHeader = True})
				entityNamesRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "JV HQ London", .IsHeader = True})
				entityNamesRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "HORSE GROUP TOTAL", .IsHeader = True})
				entityNamesRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "Horse Aveiro", .IsHeader = True})
				entityNamesRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "Horse Turkey OYAK", .IsHeader = True})
				entityNamesRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "Horse Romania", .IsHeader = True})
				entityNamesRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "Horse Brazil", .IsHeader = True})
				entityNamesRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "Horse Chile", .IsHeader = True})
				entityNamesRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "Horse Argentina", .IsHeader = True})
				entityNamesRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "Horse Spain", .IsHeader = True})
				entityNamesRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "Horse Holding", .IsHeader = True})
				tv.Rows.Add(entityNamesRow)
				
				' Fila 5: "FORECAST" en todas las columnas de datos (antes era Fila 4)
				Dim projectionHeaderRow As New TableViewRow() With {.IsHeader = True}
				projectionHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("AurobayChina", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("AurobaySweden", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("JVHQLondon", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseGroupTotal", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("Spacer2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				tv.Rows.Add(projectionHeaderRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		' Add "FLUX to Forecast start" comparison row (for EOM)
		Private Sub AddFluxRow(ByVal tv As TableView, ByVal label As String, _
		                        ByVal currentTotals As Dictionary(Of String, Decimal), _
		                        ByVal previousTotals As Dictionary(Of String, Decimal))
			Try
				Dim fluxRow As New TableViewRow()
				fluxRow.Items.Add("Description", New TableViewColumn() With {.Value = label, .IsHeader = False})
				
				Dim columnNames As String() = {"HTD", "AurobayChina", "AurobaySweden", "JVHQLondon", "Spacer1", "HorseGroupTotal", "Spacer2",
				                                 "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", 
				                                 "HorseArgentina", "HorseSpain", "HorseHolding"}
				
				For Each colName As String In columnNames
					' Handle spacer columns
					If colName = "Spacer1" OrElse colName = "Spacer2" Then
						fluxRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
						Continue For
					End If
					
					' Calculate difference (current - previous)
					Dim currentValue As Decimal = If(currentTotals.ContainsKey(colName), currentTotals(colName), 0)
					Dim previousValue As Decimal = If(previousTotals.ContainsKey(colName), previousTotals(colName), 0)
					Dim difference As Decimal = currentValue - previousValue
					
					fluxRow.Items.Add(colName, New TableViewColumn() With {.Value = difference.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False})
				Next
				
				tv.Rows.Add(fluxRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		#End Region

	End Class
End Namespace
