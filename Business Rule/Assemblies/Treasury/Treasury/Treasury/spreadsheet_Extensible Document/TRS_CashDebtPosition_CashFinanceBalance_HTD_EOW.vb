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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashDebtPosition_CashFinanceBalance_HTD_EOW
	Public Class MainClass
		'HTD Report: END OF WEEK (EndWeek) - 4 Weeks Forecast
		
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
						Return Me.GetEndWeekReport(si, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						Return Nothing
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "EndWeek Report"

		Private Function GetEndWeekReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
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
				
				' Define entity groupings based on company dictionary
				' HTD: All entities EXCEPT HPL China (999), AUROBAY SWEDEN (998), Horse HPL (997)
				Dim htdEntities As String() = {"Horse Powertrain Solution", "Horse Spain", "OYAK HORSE", "Horse Brazil", 
				                                "Horse Chile", "Horse Argentina", "Horse Romania", "Horse Aveiro"}
				
				' Individual entity names
				Dim horseAveiro As String = "Horse Aveiro"
				Dim oyak As String = "OYAK HORSE"
				Dim romania As String = "Horse Romania"
				Dim brazil As String = "Horse Brazil"
				Dim chile As String = "Horse Chile"
				Dim argentina As String = "Horse Argentina"
				Dim spain As String = "Horse Spain"
				Dim holding As String = "Horse Powertrain Solution"
				
				' Calculate forecast week (paramWeek + 4 for EndWeek)
				Dim forecastWeek As Integer = validWeek + 4
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' STEP 4: Execute query for END WEEK data
					Dim endWeekData As DataTable = Me.GetEndWeekData(si, dbConn, timeKey, expectedScenario, forecastWeek, validWeek)
					
					' STEP 5: Build TableView with EndWeek table only
					Dim tv As New TableView()
					tv.CanModifyData = False
					
					' Add columns matching the Excel format (11 columns total - includes spacer)
					tv.Columns.Add(New TableViewColumn() With {.Name = "Description", .Value = "", .IsHeader = True})
					tv.Columns.Add(New TableViewColumn() With {.Name = "HTD", .Value = "HTD (Europe, Latam)", .IsHeader = True, .DataType = XFDataType.Decimal})
					tv.Columns.Add(New TableViewColumn() With {.Name = "Spacer1", .Value = "", .IsHeader = True})
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
					
					' Process and aggregate data for END WEEK (includes start week for comparison)
					Dim endWeekAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(endWeekData, "WeekValue", htdEntities, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					Dim endWeekStartAggregated As Dictionary(Of String, Dictionary(Of String, Dictionary(Of String, Decimal))) = _
						Me.AggregateDataByEntityWithWeekValues(endWeekData, "StartWeekValue", htdEntities, _
						                                        horseAveiro, oyak, romania, brazil, chile, argentina, spain, holding)
					
					' Build the END WEEK table only
					Me.BuildEndWeekTable(tv, endWeekAggregated, endWeekStartAggregated, mondayDate, paramWeek, timeKey, validWeek)
					
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
		
		#Region "SQL Query - EndWeek Data"

		Private Function GetEndWeekData(ByVal si As SessionInfo, ByVal dbConn As DbConnInfo, _
		                                 ByVal timeKey As String, ByVal scenario As String, _
		                                 ByVal forecastWeek As Integer, ByVal validWeek As Integer) As DataTable
			Try
				' Calculate forecast timekey (timekey + 28 days = 4 weeks)
				Dim currentDate As DateTime = DateTime.ParseExact(timeKey, "yyyyMMdd", CultureInfo.InvariantCulture)
				Dim forecastDate As DateTime = currentDate.AddDays(28)
				Dim forecastTimeKey As String = forecastDate.ToString("yyyyMMdd")
				
				Dim sql As String = $"
					SELECT 
						endweek.Entity,
						endweek.Account,
						endweek.Flow,
						endweek.Bank,
						ISNULL(endweek.Amount, 0) as WeekValue,
						ISNULL(startweek.Amount, 0) as StartWeekValue
					FROM (
						SELECT Entity, Account, Flow, Bank, Amount
						FROM XFC_TRS_Master_CashDebtPosition
						WHERE UploadTimekey = '{timeKey}'
						AND Scenario = '{scenario.Replace("'", "''")}'
						AND ProjectionType = 'EndWeek'
						AND ProjectionTimekey = '{forecastTimeKey}'
					) endweek
					LEFT JOIN (
						SELECT Entity, Account, Flow, Bank, Amount
						FROM XFC_TRS_Master_CashDebtPosition
						WHERE UploadTimekey = '{timeKey}'
						AND Scenario = '{scenario.Replace("'", "''")}'
						AND ProjectionType = 'StartWeek'
						AND ProjectionTimekey = '{timeKey}'
					) startweek
					ON endweek.Entity = startweek.Entity
					AND endweek.Account = startweek.Account
					AND ISNULL(endweek.Flow, '') = ISNULL(startweek.Flow, '')
					AND ISNULL(endweek.Bank, '') = ISNULL(startweek.Bank, '')
					ORDER BY 
						endweek.Entity,
						endweek.Account,
						CASE 
							WHEN endweek.Flow LIKE 'INTernal%' THEN 1 
							WHEN endweek.Flow LIKE 'EXTernal%' THEN 2 
							ELSE 3 
						END,
						endweek.Flow,
						endweek.Bank
				"
				
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
					
					' Extract value from the specified column (WeekValue or StartWeekValue)
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
							{"HTD", 0}, {"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
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
					End If
				Next
				
				Return result
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		#End Region
		
		#Region "Build EndWeek Table"

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
				
				' Fila 1: "END WEEK FORECAST - 4 WEEKS LATER" en Description
				Dim mainHeaderRow As New TableViewRow() With {.IsHeader = True}
				mainHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "END WEEK FORECAST - 4 WEEKS LATER", .IsHeader = True})
				mainHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "Start Of Period " & endWeekFormatted, .IsHeader = True})
				mainHeaderRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "", .IsHeader = True})
				mainHeaderRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(mainHeaderRow)
				
				' Fila 2: "WEEK X - Start of DD-MMM" en Description
				Dim weekDateRow As New TableViewRow() With {.IsHeader = True}
				weekDateRow.Items.Add("Description", New TableViewColumn() With {.Value = "WEEK "  & paramWeek & " - Start of " & endWeekFormatted, .IsHeader = True})
				weekDateRow.Items.Add("HTD", New TableViewColumn() With {.Value = "", .IsHeader = True})
				weekDateRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
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
				For i As Integer = 1 To 10
					milEurHeaderRow.Items.Add(tv.Columns(i).Name, New TableViewColumn() With {.Value = "", .IsHeader = True})
				Next
				tv.Rows.Add(milEurHeaderRow)
				
				' Fila 4: Nombres de entidades en cada columna
				Dim entityNamesRow As New TableViewRow() With {.IsHeader = True}
				entityNamesRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HTD", New TableViewColumn() With {.Value = "HTD (Europe, Latam)", .IsHeader = True})
				entityNamesRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				entityNamesRow.Items.Add("HorseAveiro", New TableViewColumn() With {.Value = "Horse Aveiro", .IsHeader = True})
				entityNamesRow.Items.Add("HorseOYAK", New TableViewColumn() With {.Value = "Horse Turkey OYAK", .IsHeader = True})
				entityNamesRow.Items.Add("HorseRomania", New TableViewColumn() With {.Value = "Horse Romania", .IsHeader = True})
				entityNamesRow.Items.Add("HorseBrazil", New TableViewColumn() With {.Value = "Horse Brazil", .IsHeader = True})
				entityNamesRow.Items.Add("HorseChile", New TableViewColumn() With {.Value = "Horse Chile", .IsHeader = True})
				entityNamesRow.Items.Add("HorseArgentina", New TableViewColumn() With {.Value = "Horse Argentina", .IsHeader = True})
				entityNamesRow.Items.Add("HorseSpain", New TableViewColumn() With {.Value = "Horse Spain", .IsHeader = True})
				entityNamesRow.Items.Add("HorseHolding", New TableViewColumn() With {.Value = "Horse Holding", .IsHeader = True})
				tv.Rows.Add(entityNamesRow)
				
				' Fila 5: "FORECAST" en todas las columnas de datos
				Dim projectionHeaderRow As New TableViewRow() With {.IsHeader = True}
				projectionHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = "", .IsHeader = True})
				projectionHeaderRow.Items.Add("HTD", New TableViewColumn() With {.Value = "FORECAST", .IsHeader = True})
				projectionHeaderRow.Items.Add("Spacer1", New TableViewColumn() With {.Value = "", .IsHeader = True})
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

		Private Function AddSectionToTable(ByVal tv As TableView, ByVal sectionTitle As String, _
		                               ByVal accountData As Dictionary(Of String, Dictionary(Of String, Decimal)), _
		                               ByVal totalLabel As String) As Dictionary(Of String, Decimal)
			Try
				' Add section header row with title
				Dim sectionHeaderRow As New TableViewRow()
				sectionHeaderRow.Items.Add("Description", New TableViewColumn() With {.Value = sectionTitle, .IsHeader = False})
				For i As Integer = 1 To 10
					sectionHeaderRow.Items.Add(tv.Columns(i).Name, New TableViewColumn() With {.Value = "", .IsHeader = False})
				Next
				tv.Rows.Add(sectionHeaderRow)
				
				' Initialize totals for Internal and External
				Dim internalTotals As New Dictionary(Of String, Decimal) From {
					{"HTD", 0}, {"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
					{"HorseChile", 0}, {"HorseArgentina", 0}, {"HorseSpain", 0}, {"HorseHolding", 0}
				}
				
				Dim externalTotals As New Dictionary(Of String, Decimal) From {
					{"HTD", 0}, {"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
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
				End If
				
				' Define column names
				Dim columnNames As String() = {"HTD", "Spacer1", "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", 
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
						' Handle spacer column
						If colName = "Spacer1" Then
							flowRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
							Continue For
						End If
						
						' Get value (0 if no data)
						Dim value As Decimal = 0
						If flowData IsNot Nothing AndAlso flowData.ContainsKey(colName) Then
							value = flowData(colName)
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
							' Handle spacer column
							If colName = "Spacer1" Then
								flowRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
								Continue For
							End If
							
							Dim value As Decimal = If(flowData.ContainsKey(colName), flowData(colName), 0)
							
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
				
				Dim columnNames As String() = {"HTD", "Spacer1", "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", 
				                                 "HorseArgentina", "HorseSpain", "HorseHolding"}
				
				For Each colName As String In columnNames
					' Handle spacer column
					If colName = "Spacer1" Then
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
					{"HTD", 0}, {"HorseAveiro", 0}, {"HorseOYAK", 0}, {"HorseRomania", 0}, {"HorseBrazil", 0},
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
				
				Return totals
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		Private Sub AddFlowToLastWeekRow(ByVal tv As TableView, ByVal label As String, _
		                                  ByVal currentTotals As Dictionary(Of String, Decimal), _
		                                  ByVal previousTotals As Dictionary(Of String, Decimal))
			Try
				Dim flowRow As New TableViewRow()
				flowRow.Items.Add("Description", New TableViewColumn() With {.Value = label, .IsHeader = False})
				
				Dim columnNames As String() = {"HTD", "Spacer1", "HorseAveiro", "HorseOYAK", "HorseRomania", "HorseBrazil", "HorseChile", 
				                                 "HorseArgentina", "HorseSpain", "HorseHolding"}
				
				For Each colName As String In columnNames
					' Handle spacer column
					If colName = "Spacer1" Then
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

		#End Region

	End Class
End Namespace
