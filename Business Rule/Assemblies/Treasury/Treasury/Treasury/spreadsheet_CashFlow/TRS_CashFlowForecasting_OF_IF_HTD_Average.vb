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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashFlowForecasting_OF_IF_HTD_Average
	Public Class MainClass
		'Reference Solution Helper Business Rule - HTD Average Version (Filtered for HTD entities only)
		'Shows only Operating and Investment Flows
		
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
						Return Me.GetCashFlowForecastingOFIFAverageReport(si, args)
						
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
		
	#Region "Cash Flow Forecasting OF/IF Average Report - HTD"

		Private Function GetCashFlowForecastingOFIFAverageReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
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
				If Not Integer.TryParse(paramWeek, currentWeek) OrElse currentWeek < 1 Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView($"Error: Invalid week number '{paramWeek}'. Must be >= 1.")
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
					
					' Get maximum weeks for the selected year
					Dim maxWeeksSql As String = TRS_SQL_Repository.GetMaxWeekInYearQuery(paramYear)
					Dim maxWeeksDt As DataTable = BRApi.Database.ExecuteSql(dbConn, maxWeeksSql, False)
					Dim maxWeeks As Integer = 52 ' Default to 52 if query fails
					If maxWeeksDt IsNot Nothing AndAlso maxWeeksDt.Rows.Count > 0 AndAlso Not IsDBNull(maxWeeksDt.Rows(0)("MaxWeek")) Then
						maxWeeks = Convert.ToInt32(maxWeeksDt.Rows(0)("MaxWeek"))
					End If
					
					' Create the dynamic TableView
					Dim tv As New TableView()
					tv.CanModifyData = False ' Read-only view
					
				' ====================================
				' STEP 1: Define columns structure (2 columns)
				' ====================================
				tv.Columns.Add(New TableViewColumn() With {.Name = "Entity", .Value = "Entity", .IsHeader = True})
				tv.Columns.Add(New TableViewColumn() With {.Name = "Average", .Value = "Average", .IsHeader = True, .DataType = XFDataType.Decimal})
				
				' ====================================
				' STEP 1.5: Add header row with titles
				' ====================================
				Dim headerRow As New TableViewRow() With {.IsHeader = True}
				headerRow.Items.Add("Entity", New TableViewColumn() With {.Value = "Accum Operational + Investment Flows", .IsHeader = True})
				headerRow.Items.Add("Average", New TableViewColumn() With {.Value = "Average", .IsHeader = True})
				tv.Rows.Add(headerRow)
					Dim allDataSql As String = TRS_SQL_Repository.GetCashFlowActualGroupedByEntityWeekQuery(paramYear, currentWeek, False)
					
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
				Next					' ====================================
					' STEP 4: Calculate Accumulated Cashflow for each HTD entity and week
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
					
					' Process rows directly from query results
					' Each row represents aggregated Actual data for one week
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
						
						' Initialize "Actual" dictionary if not exists
						If Not weeklyTotals(entity).ContainsKey("Actual") Then
							weeklyTotals(entity)("Actual") = New Dictionary(Of Integer, Decimal)
							For i As Integer = 1 To maxWeeks
								weeklyTotals(entity)("Actual")(i) = 0
							Next
						End If
						
						' Accumulate amount directly
						weeklyTotals(entity)("Actual")(weekNumber) += amount
					Next
					
				' Calculate ACCUMULATED values for each HTD entity - ONLY for current year (paramYear)
				' No recursive calculation from previous years
				For Each kvp As KeyValuePair(Of String, Integer) In allEntities
					Dim entity As String = kvp.Key
					
					Dim previousWeekAccumulated As Decimal = 0
				
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
			Next
				For Each kvp As KeyValuePair(Of String, Integer) In allEntities
					Dim entity As String = kvp.Key
					
					' Calculate average from week 1 to currentWeek (parameter)
					Dim sum As Decimal = 0
					Dim count As Integer = 0
					For weekNum As Integer = 1 To currentWeek
						If entityData(entity).ContainsKey(weekNum) Then
							sum += entityData(entity)(weekNum)
							count += 1
						End If
					Next
					
					Dim average As Decimal = If(count > 0, sum / count, 0)
					Dim formattedAverage As String = Me.FormatDecimalValue(average)
					
					Dim entityRow As New TableViewRow() With {.IsHeader = False}
					entityRow.Items.Add("Entity", New TableViewColumn() With {.Value = entity, .IsHeader = False})
					entityRow.Items.Add("Average", New TableViewColumn() With {
						.Value = formattedAverage,
						.IsHeader = False,
						.DataType = XFDataType.Decimal
					})
					
					tv.Rows.Add(entityRow)
				Next
				
				' ====================================
				' STEP 5: Add "Total Horse Group" row
				' ====================================
				Dim totalHorseGroupSum As Decimal = 0
				Dim totalHorseGroupCount As Integer = 0
				
				For weekNum As Integer = 1 To currentWeek
					Dim weekTotal As Decimal = 0
					For Each entity As String In horseGroupEntities
						If entityData.ContainsKey(entity) AndAlso entityData(entity).ContainsKey(weekNum) Then
							weekTotal += entityData(entity)(weekNum)
						End If
					Next
					totalHorseGroupSum += weekTotal
					totalHorseGroupCount += 1
				Next
				
				Dim totalHorseGroupAverage As Decimal = If(totalHorseGroupCount > 0, totalHorseGroupSum / totalHorseGroupCount, 0)
			Dim formattedTotalAverage As String = Me.FormatDecimalValue(totalHorseGroupAverage)
			
			Dim totalHorseGroupRow As New TableViewRow() With {.IsHeader = False}
			totalHorseGroupRow.Items.Add("Entity", New TableViewColumn() With {.Value = "Total Horse Group", .IsHeader = True})
			totalHorseGroupRow.Items.Add("Average", New TableViewColumn() With {
				.Value = formattedTotalAverage,
				.IsHeader = False,
				.DataType = XFDataType.Decimal
			})
			tv.Rows.Add(totalHorseGroupRow)
					
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
				result("AccumulatedOFIF") = 0
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
				Dim yearSql As String = TRS_SQL_Repository.GetCashFlowYearEntityQuery(targetYear, entityFilter, maxWeeks, False)
				Dim yearDt As DataTable = BRApi.Database.ExecuteSql(dbConn, yearSql, False)
				Dim entityWeekData As New Dictionary(Of String, Dictionary(Of Integer, Decimal))
				
			If yearDt.Rows.Count = 0 Then
				If paramEntity = "HTD" Then
					Dim allEntities As String() = {"Horse Aveiro", "OYAK HORSE", "Horse Romania", "Horse Brazil", "Horse Chile", "Horse Argentina", "Horse Spain", "Horse Powertrain Solution"}
					For Each entityName As String In allEntities
							result($"{entityName}_OF_IF") = 0
						Next
					End If
					Return result
				End If
				
				For Each row As DataRow In yearDt.Rows
					Dim entity = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
					Dim weekNum = If(IsDBNull(row("ProjectionWeekNumber")), 0, Convert.ToInt32(row("ProjectionWeekNumber")))
					Dim amount = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
					If Not entityWeekData.ContainsKey(entity) Then entityWeekData(entity) = New Dictionary(Of Integer, Decimal)
					If Not entityWeekData(entity).ContainsKey(weekNum) Then entityWeekData(entity)(weekNum) = 0
					entityWeekData(entity)(weekNum) += amount
				Next
				
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
				Return New Dictionary(Of String, Decimal) From {{"AccumulatedOFIF", 0}}
			End Try
		End Function
		
		#End Region
		
		#Region "Save Data"
		
		Private Function SaveCashFlowForecastChanges(ByVal si As SessionInfo, ByVal tableView As TableView) As Boolean
		    Try

		        Return True
		    Catch ex As Exception
		        Return False
		    End Try
		End Function
		
		#End Region

	End Class
End Namespace
