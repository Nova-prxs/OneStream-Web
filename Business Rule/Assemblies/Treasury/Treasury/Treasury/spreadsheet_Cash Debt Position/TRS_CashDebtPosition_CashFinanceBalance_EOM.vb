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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashDebtPosition_CashFinanceBalance_EOM
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
							list.Add("prm_Treasury_WeekNumber")
							Return list
						Catch ex As Exception
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
						
					Case Is = SpreadsheetFunctionType.GetTableView
						Return Me.GetCashDebtPositionActualReport(si, args)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Get Report - EOM Projection"

		Private Function GetCashDebtPositionActualReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
			Try				
				' STEP 1: Extract parameters from spreadsheet args
				Dim paramEntityId As String = ""
				Dim paramYear As String = ""
				Dim paramWeek As String = ""
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_CompanyNames") Then
					paramEntityId = args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year") Then
					paramYear = args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber") Then
					paramWeek = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")
				End If
				
				' STEP 2: EARLY VALIDATION
				If String.IsNullOrWhiteSpace(paramEntityId) Then
					Return Me.CreateEmptyTableView("Error: Entity parameter is required")
				End If
				
				If String.IsNullOrWhiteSpace(paramYear) OrElse Not IsNumeric(paramYear) Then
					Return Me.CreateEmptyTableView("Error: Valid Year parameter is required")
				End If
				
				If String.IsNullOrWhiteSpace(paramWeek) OrElse Not IsNumeric(paramWeek) Then
					Return Me.CreateEmptyTableView("Error: Valid Week parameter is required")
				End If
				
				Dim weekNumber As Integer = CInt(paramWeek)
				If weekNumber < 1 OrElse weekNumber > 53 Then
					Return Me.CreateEmptyTableView("Error: Week must be between 1 and 53")
				End If
				
				' STEP 3: Calculate Month and Get Weeks
				Dim monthNum As Integer = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMonthFromWeek(si, paramYear, weekNumber)
				
				Dim weeksDt As DataTable = Nothing
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					' Use GROUP BY to ensure unique week numbers and get the first timekey (Monday) for each week
					Dim sqlWeeks As String = $"SELECT weekNumber, MIN(timekey) as timekey FROM XFC_TRS_AUX_Date WHERE year = {paramYear} AND monthNumber = {monthNum} GROUP BY weekNumber ORDER BY weekNumber"
					weeksDt = BRApi.Database.ExecuteSql(dbConn, sqlWeeks, False)
				End Using
				
				If weeksDt.Rows.Count = 0 Then
					Return Me.CreateEmptyTableView($"Error: No weeks found for Year {paramYear} Month {monthNum}")
				End If
				
				Dim weeksList As New List(Of Dictionary(Of String, String))
				Dim timeKeys As New List(Of String)
				
				For Each row As DataRow In weeksDt.Rows
					Dim wInfo As New Dictionary(Of String, String)
					wInfo("Week") = row("weekNumber").ToString()
					wInfo("TimeKey") = row("timekey").ToString()
					weeksList.Add(wInfo)
					timeKeys.Add(row("timekey").ToString())
				Next
				
				' STEP 4: Convert company ID to name
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
				
				' STEP 5: Execute optimized query for EOM data for all weeks
				Dim actualData As DataTable = Me.GetActualData(si, timeKeys, paramEntity)
				
				' STEP 6: Build TableView
				Return Me.BuildTableView(actualData, weeksList, monthNum)
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Optimized SQL Query"

		Private Function GetActualData(ByVal si As SessionInfo, ByVal timeKeys As List(Of String), ByVal entity As String) As DataTable
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Build entity filter - handle "HTD" case (empty filter shows all entities)
					Dim entityFilter As String = ""
					If entity <> "HTD" Then
						entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'" 
					End If
					
					Dim timeKeyList As String = String.Join("','", timeKeys)
					
					' Use centralized SQL repository
					Dim sql As String = TRS_SQL_Repository.GetCashDebtEOMMultipleWeeksQuery(timeKeyList, entityFilter)
					
					Return BRApi.Database.ExecuteSql(dbConn, sql, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Build TableView"

		Private Function BuildTableView(ByVal actualData As DataTable, ByVal weeksList As List(Of Dictionary(Of String, String)), ByVal monthNum As Integer) As TableView
			Try
				Dim tv As New TableView()
				tv.CanModifyData = False
				
				' Define columns: Description, Week Columns..., Bank
				tv.Columns.Add(New TableViewColumn() With {.Name = "DescActual", .Value = "Mill EUR", .IsHeader = True})
				
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					Dim colName As String = "Week" & weekInfo("Week")
					tv.Columns.Add(New TableViewColumn() With {.Name = colName, .Value = "Week " & weekInfo("Week"), .IsHeader = True, .DataType = XFDataType.Decimal})
				Next
				
				tv.Columns.Add(New TableViewColumn() With {.Name = "BankActual", .Value = "BANK", .IsHeader = True})
				
				' Add first header row with period title
				Dim firstHeaderRow As New TableViewRow() With {.IsHeader = True}
				firstHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "End Of Month " & monthNum, .IsHeader = True})
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					Dim colName As String = "Week" & weekInfo("Week")
					firstHeaderRow.Items.Add(colName, New TableViewColumn() With {.Value = "EOM Projection", .IsHeader = True})
				Next
				firstHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(firstHeaderRow)
				
				' Add second header row
				Dim secondHeaderRow As New TableViewRow() With {.IsHeader = True}
				secondHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "CASH AND FINANCING BALANCE", .IsHeader = True})
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					Dim colName As String = "Week" & weekInfo("Week")
					secondHeaderRow.Items.Add(colName, New TableViewColumn() With {.Value = "Week " & weekInfo("Week"), .IsHeader = True})
				Next
				secondHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "Bank", .IsHeader = True})
				tv.Rows.Add(secondHeaderRow)
				
				' Build lookup dictionary
				Dim dataDict As New Dictionary(Of String, Dictionary(Of String, Decimal))
				
				For Each dr As DataRow In actualData.Rows
					Dim account As String = dr("Account").ToString()
					Dim flow As String = dr("Flow").ToString()
					Dim bank As String = If(IsDBNull(dr("Bank")), "", dr("Bank").ToString())
					Dim timeKey As String = dr("UploadTimekey").ToString()
					Dim amount As Decimal = If(IsDBNull(dr("Amount")), 0, Convert.ToDecimal(dr("Amount")))
					
					Dim rowKey As String = $"{account}|{flow}|{bank}"
					
					If Not dataDict.ContainsKey(rowKey) Then
						dataDict(rowKey) = New Dictionary(Of String, Decimal)
					End If
					
					If dataDict(rowKey).ContainsKey(timeKey) Then
						dataDict(rowKey)(timeKey) += amount
					Else
						dataDict(rowKey)(timeKey) = amount
					End If
				Next
				
				' Process sections
				Dim cashAvailableTotals As Dictionary(Of String, Decimal) = Nothing
				Dim utilizedDebtTotals As Dictionary(Of String, Decimal) = Nothing
				Dim availableFinancingTotals As Dictionary(Of String, Decimal) = Nothing
				
				Me.AddSectionRows(tv, "CASH AND FINANCING BALANCE", dataDict, weeksList, "Cash available", cashAvailableTotals)
				Me.AddSeparatorRow(tv, weeksList)
				
				Me.AddSectionRows(tv, "FINANCING - USED LINES", dataDict, weeksList, "Utilized debt", utilizedDebtTotals)
				Me.AddSeparatorRow(tv, weeksList)
				
				Me.AddSectionRows(tv, "FINANCING - AVAILABLE LINES", dataDict, weeksList, "Available financing", availableFinancingTotals)
				Me.AddSeparatorRow(tv, weeksList)
				
				' Add Net Financial Position
				Me.AddNetFinancialPositionRow(tv, weeksList, cashAvailableTotals, utilizedDebtTotals)
				
				' Configure formatting
				tv.HeaderFormat.BackgroundColor = XFColors.Black
				tv.HeaderFormat.IsBold = True
				tv.HeaderFormat.TextColor = XFColors.White
				
				Return tv
				
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		Private Sub AddSectionRows(ByVal tv As TableView, ByVal accountName As String, _
								   ByVal dataDict As Dictionary(Of String, Dictionary(Of String, Decimal)), _
								   ByVal weeksList As List(Of Dictionary(Of String, String)), _
								   ByVal totalLabel As String, _
								   ByRef totalsOut As Dictionary(Of String, Decimal))
			Try
				' Add section header row
				If accountName <> "CASH AND FINANCING BALANCE" Then
					Dim sectionHeaderRow As New TableViewRow() With {.IsHeader = True}
					sectionHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = accountName, .IsHeader = True})
					For Each weekInfo As Dictionary(Of String, String) In weeksList
						Dim colName As String = "Week" & weekInfo("Week")
						sectionHeaderRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = True})
					Next
					sectionHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
					tv.Rows.Add(sectionHeaderRow)
				End If
				
				' Identify all unique Flow|Bank combinations for this account
				Dim sectionKeys As New HashSet(Of String)
				For Each key As String In dataDict.Keys
					If key.StartsWith(accountName & "|") Then
						sectionKeys.Add(key)
					End If
				Next
				
				' Add mandatory flows if missing
				Dim mandatoryFlows As New List(Of String)
				If accountName = "CASH AND FINANCING BALANCE" Then
					mandatoryFlows.Add("INTernal Cash (+)")
					mandatoryFlows.Add("EXTernal Cash (+)")
				ElseIf accountName = "FINANCING - USED LINES" Then
					mandatoryFlows.Add("INTernal Debt (-)")
					mandatoryFlows.Add("EXTernal Debt (-)")
				ElseIf accountName = "FINANCING - AVAILABLE LINES" Then
					mandatoryFlows.Add("INTernal Debt (-)")
					mandatoryFlows.Add("EXTernal Debt (-)")
				End If
				
				For Each flow As String In mandatoryFlows
					Dim found As Boolean = False
					For Each key As String In sectionKeys
						Dim parts As String() = key.Split("|"c)
						If parts.Length >= 2 AndAlso parts(1) = flow Then
							found = True
							Exit For
						End If
					Next
					If Not found Then
						sectionKeys.Add($"{accountName}|{flow}|")
					End If
				Next
				
				' Sort keys
				Dim sortedKeys As List(Of String) = sectionKeys.OrderBy(Function(k) 
					Dim parts As String() = k.Split("|"c)
					Dim flow As String = parts(1)
					If flow.StartsWith("INTernal") Then Return 1
					If flow.StartsWith("EXTernal") Then Return 2
					Return 3
				End Function).ThenBy(Function(k) k).ToList()
				
				' Totals per week
				Dim totalsInternal As New Dictionary(Of String, Decimal)
				Dim totalsExternal As New Dictionary(Of String, Decimal)
				
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					totalsInternal(weekInfo("TimeKey")) = 0
					totalsExternal(weekInfo("TimeKey")) = 0
				Next
				
				' Add rows
				For Each key As String In sortedKeys
					Dim parts As String() = key.Split("|"c)
					Dim flow As String = parts(1)
					Dim bank As String = parts(2)
					
					Dim row As New TableViewRow()
					row.Items.Add("DescActual", New TableViewColumn() With {.Value = flow, .IsHeader = False})
					
					For Each weekInfo As Dictionary(Of String, String) In weeksList
						Dim timeKey As String = weekInfo("TimeKey")
						Dim colName As String = "Week" & weekInfo("Week")
						Dim val As Decimal = 0
						
						If dataDict.ContainsKey(key) AndAlso dataDict(key).ContainsKey(timeKey) Then
							val = dataDict(key)(timeKey)
						End If
						
						row.Items.Add(colName, New TableViewColumn() With {.Value = val.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
						
						If flow.StartsWith("INTernal") Then
							totalsInternal(timeKey) += val
						ElseIf flow.StartsWith("EXTernal") Then
							totalsExternal(timeKey) += val
						End If
					Next
					
					row.Items.Add("BankActual", New TableViewColumn() With {.Value = bank, .IsHeader = False})
					tv.Rows.Add(row)
				Next
				
				' Add Subtotals
				Me.AddSubtotalRow(tv, "Total INTernal", totalsInternal, weeksList)
				Me.AddSubtotalRow(tv, "Total EXTernal", totalsExternal, weeksList)
				
				' Add Grand Total
				' For FINANCING - AVAILABLE LINES: use only EXTernal (not INTernal + EXTernal)
				' For other sections: use INTernal + EXTernal
				Dim grandTotals As New Dictionary(Of String, Decimal)
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					Dim tk As String = weekInfo("TimeKey")
					If accountName = "FINANCING - AVAILABLE LINES" Then
						grandTotals(tk) = totalsExternal(tk)
					Else
						grandTotals(tk) = totalsInternal(tk) + totalsExternal(tk)
					End If
				Next
				Me.AddSubtotalRow(tv, totalLabel, grandTotals, weeksList)
				
				totalsOut = grandTotals
				
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Sub AddSubtotalRow(ByVal tv As TableView, ByVal label As String, ByVal totals As Dictionary(Of String, Decimal), ByVal weeksList As List(Of Dictionary(Of String, String)))
			Try
				Dim subtotalRow As New TableViewRow()
				
				subtotalRow.Items.Add("DescActual", New TableViewColumn() With {.Value = label, .IsHeader = False})
				
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					Dim timeKey As String = weekInfo("TimeKey")
					Dim colName As String = "Week" & weekInfo("Week")
					Dim val As Decimal = 0
					If totals.ContainsKey(timeKey) Then
						val = totals(timeKey)
					End If
					subtotalRow.Items.Add(colName, New TableViewColumn() With {.Value = val.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
				Next
				
				subtotalRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
				
				tv.Rows.Add(subtotalRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Sub AddSeparatorRow(ByVal tv As TableView, ByVal weeksList As List(Of Dictionary(Of String, String)))
			Try
				Dim blankRow As New TableViewRow()
				
				blankRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					Dim colName As String = "Week" & weekInfo("Week")
					blankRow.Items.Add(colName, New TableViewColumn() With {.Value = "", .IsHeader = False})
				Next
				blankRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
				
				tv.Rows.Add(blankRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub
		
		Private Sub AddNetFinancialPositionRow(ByVal tv As TableView, ByVal weeksList As List(Of Dictionary(Of String, String)), ByVal cashTotals As Dictionary(Of String, Decimal), ByVal debtTotals As Dictionary(Of String, Decimal))
			Try
				Dim netTotals As New Dictionary(Of String, Decimal)
				
				For Each weekInfo As Dictionary(Of String, String) In weeksList
					Dim tk As String = weekInfo("TimeKey")
					Dim cash As Decimal = If(cashTotals IsNot Nothing AndAlso cashTotals.ContainsKey(tk), cashTotals(tk), 0)
					Dim debt As Decimal = If(debtTotals IsNot Nothing AndAlso debtTotals.ContainsKey(tk), debtTotals(tk), 0)
					netTotals(tk) = cash - debt
				Next
				
				Me.AddSubtotalRow(tv, "Net Financial Position", netTotals, weeksList)
				
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

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
