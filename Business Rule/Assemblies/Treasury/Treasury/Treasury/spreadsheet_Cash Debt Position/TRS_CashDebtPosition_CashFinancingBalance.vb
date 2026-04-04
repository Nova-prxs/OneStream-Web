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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashDebtPosition_CashFinancingBalance
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
						Return Me.GetConsolidatedCashDebtPositionReport(si, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						Return Me.SaveCashDebtPositionChanges(si, args)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		#Region "Get Consolidated Report"

		Private Function GetConsolidatedCashDebtPositionReport(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As TableView
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
				
				' STEP 2: EARLY VALIDATION - CRITICAL - BEFORE ANY SQL
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
				
				' STEP 3: Convert company ID to name and calculate keys
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
				Dim timeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear, paramWeek)
				Dim expectedScenario As String = "FW" + paramWeek.PadLeft(2, "0"c)
				Dim eomMonth As Integer = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMonthFromWeek(si, paramYear, weekNumber)
				
				' STEP 4: Execute optimized queries separately
				Dim actualData As DataTable = Me.GetActualData(si, timeKey, paramEntity, expectedScenario)
				Dim endWeekData As DataTable = Me.GetEndWeekData(si, timeKey, paramEntity, expectedScenario)
				Dim eomData As DataTable = Me.GetEOMData(si, timeKey, paramEntity, expectedScenario, eomMonth)
				
				' STEP 5: Build consolidated TableView (even if no data, show structure with zeros)
				Return Me.BuildConsolidatedTableView(actualData, endWeekData, eomData, paramEntityId, paramWeek, paramYear)
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Optimized SQL Queries"

	Private Function GetActualData(ByVal si As SessionInfo, ByVal timeKey As String, ByVal entity As String, ByVal scenario As String) As DataTable
		Try
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
				' Build entity filter - handle "HTD" case (empty filter shows all entities)
				Dim entityFilter As String = ""
				If entity <> "HTD" Then
					entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'"
				End If
				
				' Use centralized SQL repository
				Dim sql As String = TRS_SQL_Repository.GetCashDebtStartWeekSimpleQuery(timeKey, scenario, entityFilter)
				
				Return BRApi.Database.ExecuteSql(dbConn, sql, False)
			End Using
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Function	
	
	Private Function GetEndWeekData(ByVal si As SessionInfo, ByVal timeKey As String, ByVal entity As String, ByVal scenario As String) As DataTable
		Try
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
				' Build entity filter - handle "HTD" case (empty filter shows all entities)
				Dim entityFilter As String = ""
				If entity <> "HTD" Then
					entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'"
				End If
				
				' Use centralized SQL repository
				Dim sql As String = TRS_SQL_Repository.GetCashDebtEndWeekSimpleQuery(timeKey, scenario, entityFilter)
				
				Return BRApi.Database.ExecuteSql(dbConn, sql, False)
			End Using
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Function	
	
	Private Function GetEOMData(ByVal si As SessionInfo, ByVal timeKey As String, ByVal entity As String, ByVal scenario As String, ByVal eomMonth As Integer) As DataTable
		Try
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
				' Build entity filter - handle HTD case (empty filter shows all entities)
				Dim entityFilter As String = ""
				If entity <> "HTD" Then
					entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'" 
				End If
				
				' Use centralized SQL repository
				Dim sql As String = TRS_SQL_Repository.GetCashDebtEOMByMonthQuery(timeKey, scenario, eomMonth, entityFilter)
				
				Return BRApi.Database.ExecuteSql(dbConn, sql, False)
			End Using
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Function

	#End Region
		
		#Region "Build Consolidated TableView"

		Private Function BuildConsolidatedTableView(ByVal actualData As DataTable, ByVal endWeekData As DataTable, ByVal eomData As DataTable, _
													ByVal paramEntityId As String, ByVal paramWeek As String, ByVal paramYear As String) As TableView
			Try
		Dim tv As New TableView()
		tv.CanModifyData = True
		
		' Define columns: Description, Value, Bank for each period + FORECAST for projections
		tv.Columns.Add(New TableViewColumn() With {.Name = "DescActual", .Value = "Mill EUR", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "Actual", .Value = "ACTUAL", .IsHeader = True, .DataType = XFDataType.Decimal})
		tv.Columns.Add(New TableViewColumn() With {.Name = "BankActual", .Value = "BANK", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "Separator1", .Value = "", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "Separator2", .Value = "", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "DescEndWeek", .Value = "Mill EUR", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "EndWeek", .Value = "FORECAST", .IsHeader = True, .DataType = XFDataType.Decimal})
		tv.Columns.Add(New TableViewColumn() With {.Name = "BankEndWeek", .Value = "BANK", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "Separator3", .Value = "", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "Separator4", .Value = "", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "DescEOM", .Value = "Mill EUR", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "EOM", .Value = "FORECAST", .IsHeader = True, .DataType = XFDataType.Decimal})
		tv.Columns.Add(New TableViewColumn() With {.Name = "BankEOM", .Value = "BANK", .IsHeader = True})
		tv.Columns.Add(New TableViewColumn() With {.Name = "HiddenKeys", .Value = "", .IsHeader = True})			' CRITICAL: Add first header row with period titles and PARAMS
			Dim firstHeaderRow As New TableViewRow() With {.IsHeader = True}
			firstHeaderRow.Items.Add("DescActual", New TableViewColumn() With {
				.Value = $"PARAMS|{paramEntityId}|{paramWeek}|{paramYear}",
				.IsHeader = True,
				.DataType = XFDataType.Text
			})
			firstHeaderRow.Items.Add("Actual", New TableViewColumn() With {.Value = "Start of week", .IsHeader = True})
			firstHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("Separator1", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("Separator2", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("DescEndWeek", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("EndWeek", New TableViewColumn() With {.Value = "Ending week", .IsHeader = True})
			firstHeaderRow.Items.Add("BankEndWeek", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("Separator3", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("Separator4", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("DescEOM", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("EOM", New TableViewColumn() With {.Value = "End of month", .IsHeader = True})
			firstHeaderRow.Items.Add("BankEOM", New TableViewColumn() With {.Value = "", .IsHeader = True})
			firstHeaderRow.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = "", .IsHeader = True})
			tv.Rows.Add(firstHeaderRow)
			
			' Add second header row with CASH AND FINANCING BALANCE section and column labels
			Dim secondHeaderRow As New TableViewRow() With {.IsHeader = True}
			secondHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "CASH AND FINANCING BALANCE", .IsHeader = True})
			secondHeaderRow.Items.Add("Actual", New TableViewColumn() With {.Value = "Actual", .IsHeader = True})
			secondHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "Bank", .IsHeader = True})
			secondHeaderRow.Items.Add("Separator1", New TableViewColumn() With {.Value = "", .IsHeader = True})
			secondHeaderRow.Items.Add("Separator2", New TableViewColumn() With {.Value = "", .IsHeader = True})
			secondHeaderRow.Items.Add("DescEndWeek", New TableViewColumn() With {.Value = "CASH AND FINANCING BALANCE", .IsHeader = True})
			secondHeaderRow.Items.Add("EndWeek", New TableViewColumn() With {.Value = "Forecast", .IsHeader = True})
			secondHeaderRow.Items.Add("BankEndWeek", New TableViewColumn() With {.Value = "Bank", .IsHeader = True})
			secondHeaderRow.Items.Add("Separator3", New TableViewColumn() With {.Value = "", .IsHeader = True})
			secondHeaderRow.Items.Add("Separator4", New TableViewColumn() With {.Value = "", .IsHeader = True})
			secondHeaderRow.Items.Add("DescEOM", New TableViewColumn() With {.Value = "CASH AND FINANCING BALANCE", .IsHeader = True})
			secondHeaderRow.Items.Add("EOM", New TableViewColumn() With {.Value = "Forecast", .IsHeader = True})
			secondHeaderRow.Items.Add("BankEOM", New TableViewColumn() With {.Value = "Bank", .IsHeader = True})
			secondHeaderRow.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = "", .IsHeader = True})
			tv.Rows.Add(secondHeaderRow)
			
			' Build lookup dictionaries for efficient data retrieval - SUPPORT MULTIPLE BANKS PER FLOW
			Dim actualDict As New Dictionary(Of String, List(Of DataRow))
			Dim endWeekDict As New Dictionary(Of String, List(Of DataRow))
			Dim eomDict As New Dictionary(Of String, List(Of DataRow))
			
			For Each dr As DataRow In actualData.Rows
				Dim key As String = $"{dr("Account")}|{dr("Flow")}"
				If Not actualDict.ContainsKey(key) Then
					actualDict(key) = New List(Of DataRow)
				End If
				actualDict(key).Add(dr)
			Next
			
			For Each dr As DataRow In endWeekData.Rows
				Dim key As String = $"{dr("Account")}|{dr("Flow")}"
				If Not endWeekDict.ContainsKey(key) Then
					endWeekDict(key) = New List(Of DataRow)
				End If
				endWeekDict(key).Add(dr)
			Next
			
			For Each dr As DataRow In eomData.Rows
				Dim key As String = $"{dr("Account")}|{dr("Flow")}"
				If Not eomDict.ContainsKey(key) Then
					eomDict(key) = New List(Of DataRow)
				End If
				eomDict(key).Add(dr)
			Next
			
			' Process all three sections with their own headers
			Dim cashAvailableActual As Decimal = 0
			Dim cashAvailableEndWeek As Decimal = 0
			Dim cashAvailableEOM As Decimal = 0
			
			Dim utilizedDebtActual As Decimal = 0
			Dim utilizedDebtEndWeek As Decimal = 0
			Dim utilizedDebtEOM As Decimal = 0
			
			Me.AddSectionRows(tv, "CASH AND FINANCING BALANCE", actualDict, endWeekDict, eomDict, "Cash available", cashAvailableActual, cashAvailableEndWeek, cashAvailableEOM)
			Me.AddSeparatorRow(tv)
			
			Me.AddSectionRows(tv, "FINANCING - USED LINES", actualDict, endWeekDict, eomDict, "Utilized debt", utilizedDebtActual, utilizedDebtEndWeek, utilizedDebtEOM)
			Me.AddSeparatorRow(tv)
			
			Me.AddSectionRows(tv, "FINANCING - AVAILABLE LINES", actualDict, endWeekDict, eomDict, "Available financing")
			Me.AddSeparatorRow(tv)
			
			' Add Net Financial Position (Cash available - Utilized debt)
			Dim netFinancialPositionActual As Decimal = cashAvailableActual - utilizedDebtActual
			Dim netFinancialPositionEndWeek As Decimal = cashAvailableEndWeek - utilizedDebtEndWeek
			Dim netFinancialPositionEOM As Decimal = cashAvailableEOM - utilizedDebtEOM
			Me.AddSubtotalRow(tv, "Net Financial Position", netFinancialPositionActual, netFinancialPositionEndWeek, netFinancialPositionEOM)
			
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
							   ByVal actualDict As Dictionary(Of String, List(Of DataRow)), _
							   ByVal endWeekDict As Dictionary(Of String, List(Of DataRow)), _
							   ByVal eomDict As Dictionary(Of String, List(Of DataRow)), _
							   ByVal totalLabel As String, _
							   Optional ByRef totalActualOut As Decimal = 0, _
							   Optional ByRef totalEndWeekOut As Decimal = 0, _
							   Optional ByRef totalEOMOut As Decimal = 0)
		Try
			' Add section header row ONLY for FINANCING sections (not for CASH AND FINANCING BALANCE)
			If accountName <> "CASH AND FINANCING BALANCE" Then
				Dim sectionHeaderRow As New TableViewRow() With {.IsHeader = True}
				sectionHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = accountName, .IsHeader = True})
				sectionHeaderRow.Items.Add("Actual", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("Separator1", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("Separator2", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("DescEndWeek", New TableViewColumn() With {.Value = accountName, .IsHeader = True})
				sectionHeaderRow.Items.Add("EndWeek", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("BankEndWeek", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("Separator3", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("Separator4", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("DescEOM", New TableViewColumn() With {.Value = accountName, .IsHeader = True})
				sectionHeaderRow.Items.Add("EOM", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("BankEOM", New TableViewColumn() With {.Value = "", .IsHeader = True})
				sectionHeaderRow.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(sectionHeaderRow)
			End If
			
		' Define mandatory flows based on account type (always shown, even with zeros)
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
		
		' Get all unique Flow|Bank combinations from all three datasets for this account
		Dim allFlowBankCombos As New HashSet(Of String)
		
		' FIRST: Collect all Flow|Bank combinations that EXIST in the data
		For Each kvp As KeyValuePair(Of String, List(Of DataRow)) In actualDict
			Dim keyParts As String() = kvp.Key.Split("|"c)
			If keyParts.Length >= 2 AndAlso keyParts(0) = accountName Then
				Dim flowName As String = keyParts(1)
				For Each dr As DataRow In kvp.Value
					Dim bank As String = If(IsDBNull(dr("Bank")), "", dr("Bank").ToString())
					allFlowBankCombos.Add($"{flowName}|{bank}")
				Next
			End If
		Next
		
		For Each kvp As KeyValuePair(Of String, List(Of DataRow)) In endWeekDict
			Dim keyParts As String() = kvp.Key.Split("|"c)
			If keyParts.Length >= 2 AndAlso keyParts(0) = accountName Then
				Dim flowName As String = keyParts(1)
				For Each dr As DataRow In kvp.Value
					Dim bank As String = If(IsDBNull(dr("Bank")), "", dr("Bank").ToString())
					allFlowBankCombos.Add($"{flowName}|{bank}")
				Next
			End If
		Next
		
		For Each kvp As KeyValuePair(Of String, List(Of DataRow)) In eomDict
			Dim keyParts As String() = kvp.Key.Split("|"c)
			If keyParts.Length >= 2 AndAlso keyParts(0) = accountName Then
				Dim flowName As String = keyParts(1)
				For Each dr As DataRow In kvp.Value
					Dim bank As String = If(IsDBNull(dr("Bank")), "", dr("Bank").ToString())
					allFlowBankCombos.Add($"{flowName}|{bank}")
				Next
			End If
		Next
		
		' SECOND: Add mandatory flows ONLY if they don't already exist in the data
		For Each mandatoryFlow As String In mandatoryFlows
			Dim flowExists As Boolean = False
			
			' Check if this flow exists in any combination (with any bank)
			For Each flowBank As String In allFlowBankCombos
				If flowBank.StartsWith(mandatoryFlow & "|") Then
					flowExists = True
					Exit For
				End If
			Next
			
			' If flow doesn't exist at all, add it with empty bank
			If Not flowExists Then
				allFlowBankCombos.Add($"{mandatoryFlow}|")
			End If
		Next
		
		' Sort Flow|Bank combinations: INTernal first, then EXTernal, then by flow and bank
		Dim sortedFlowBanks As List(Of String) = allFlowBankCombos.OrderBy(Function(fb) If(fb.Split("|"c)(0).StartsWith("INTernal"), 1, If(fb.Split("|"c)(0).StartsWith("EXTernal"), 2, 3))).ThenBy(Function(fb) fb).ToList()			' Accumulators for subtotals
			Dim totalActualInternal As Decimal = 0
			Dim totalActualExternal As Decimal = 0
			Dim totalEndWeekInternal As Decimal = 0
			Dim totalEndWeekExternal As Decimal = 0
			Dim totalEOMInternal As Decimal = 0
			Dim totalEOMExternal As Decimal = 0
			
			' Process each flow|bank combination
			For Each flowBank As String In sortedFlowBanks
				Dim fbParts As String() = flowBank.Split("|"c)
				If fbParts.Length < 2 Then Continue For
				
				Dim flow As String = fbParts(0)
				Dim bank As String = fbParts(1)
				
				Dim row As New TableViewRow()
				
				Dim key As String = $"{accountName}|{flow}"
				
				' Get values from each dataset for this specific bank
				Dim actualValue As Decimal = 0
				Dim endWeekValue As Decimal = 0
				Dim eomValue As Decimal = 0
				Dim bankActualValue As String = bank
				Dim bankEndWeekValue As String = bank
				Dim bankEOMValue As String = bank
				Dim hiddenKeysActual As String = ""
				Dim hiddenKeysEndWeek As String = ""
				Dim hiddenKeysEOM As String = ""
				
				' Get Actual data for this specific bank
				If actualDict.ContainsKey(key) Then
					For Each drActual As DataRow In actualDict(key)
						Dim drBank As String = If(IsDBNull(drActual("Bank")), "", drActual("Bank").ToString())
						If drBank = bank Then
							actualValue = If(IsDBNull(drActual("Amount")), 0, Convert.ToDecimal(drActual("Amount")))
							bankActualValue = drBank
							
							Dim uploadTk As String = If(IsDBNull(drActual("UploadTimekey")), "", drActual("UploadTimekey").ToString())
							Dim entity As String = If(IsDBNull(drActual("Entity")), "", drActual("Entity").ToString())
							Dim scenario As String = If(IsDBNull(drActual("Scenario")), "", drActual("Scenario").ToString())
							hiddenKeysActual = $"KEYS|StartWeek|{uploadTk}|{entity}|{scenario}|{accountName}|{flow}|{bankActualValue}"
							Exit For
						End If
					Next
				End If
				
				' Get EndWeek data for this specific bank
				If endWeekDict.ContainsKey(key) Then
					For Each drEndWeek As DataRow In endWeekDict(key)
						Dim drBank As String = If(IsDBNull(drEndWeek("Bank")), "", drEndWeek("Bank").ToString())
						If drBank = bank Then
							endWeekValue = If(IsDBNull(drEndWeek("Amount")), 0, Convert.ToDecimal(drEndWeek("Amount")))
							bankEndWeekValue = drBank
							
							Dim uploadTk As String = If(IsDBNull(drEndWeek("UploadTimekey")), "", drEndWeek("UploadTimekey").ToString())
							Dim entity As String = If(IsDBNull(drEndWeek("Entity")), "", drEndWeek("Entity").ToString())
							Dim scenario As String = If(IsDBNull(drEndWeek("Scenario")), "", drEndWeek("Scenario").ToString())
							hiddenKeysEndWeek = $"KEYS|EndWeek|{uploadTk}|{entity}|{scenario}|{accountName}|{flow}|{bankEndWeekValue}"
							Exit For
						End If
					Next
				End If
				
				' Get EOM data for this specific bank
				If eomDict.ContainsKey(key) Then
					For Each drEOM As DataRow In eomDict(key)
						Dim drBank As String = If(IsDBNull(drEOM("Bank")), "", drEOM("Bank").ToString())
						If drBank = bank Then
							eomValue = If(IsDBNull(drEOM("Amount")), 0, Convert.ToDecimal(drEOM("Amount")))
							bankEOMValue = drBank
							
							Dim uploadTk As String = If(IsDBNull(drEOM("UploadTimekey")), "", drEOM("UploadTimekey").ToString())
							Dim entity As String = If(IsDBNull(drEOM("Entity")), "", drEOM("Entity").ToString())
							Dim scenario As String = If(IsDBNull(drEOM("Scenario")), "", drEOM("Scenario").ToString())
							hiddenKeysEOM = $"KEYS|EOM|{uploadTk}|{entity}|{scenario}|{accountName}|{flow}|{bankEOMValue}"
							Exit For
						End If
					Next
				End If
				
				' Accumulate subtotals
				If flow.StartsWith("INTernal") Then
					totalActualInternal += actualValue
					totalEndWeekInternal += endWeekValue
					totalEOMInternal += eomValue
				ElseIf flow.StartsWith("EXTernal") Then
					totalActualExternal += actualValue
					totalEndWeekExternal += endWeekValue
					totalEOMExternal += eomValue
				End If
				
				' Add row with 14 columns (Description + Value + Bank for each period + separators + hidden)
				row.Items.Add("DescActual", New TableViewColumn() With {.Value = flow, .IsHeader = False})
				row.Items.Add("Actual", New TableViewColumn() With {.Value = actualValue.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
				row.Items.Add("BankActual", New TableViewColumn() With {.Value = bankActualValue, .IsHeader = False})
				row.Items.Add("Separator1", New TableViewColumn() With {.Value = "", .IsHeader = False})
				row.Items.Add("Separator2", New TableViewColumn() With {.Value = "", .IsHeader = False})
				row.Items.Add("DescEndWeek", New TableViewColumn() With {.Value = flow, .IsHeader = False})
				row.Items.Add("EndWeek", New TableViewColumn() With {.Value = endWeekValue.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
				row.Items.Add("BankEndWeek", New TableViewColumn() With {.Value = bankEndWeekValue, .IsHeader = False})
				row.Items.Add("Separator3", New TableViewColumn() With {.Value = "", .IsHeader = False})
				row.Items.Add("Separator4", New TableViewColumn() With {.Value = "", .IsHeader = False})
				row.Items.Add("DescEOM", New TableViewColumn() With {.Value = flow, .IsHeader = False})
				row.Items.Add("EOM", New TableViewColumn() With {.Value = eomValue.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
				row.Items.Add("BankEOM", New TableViewColumn() With {.Value = bankEOMValue, .IsHeader = False})
				row.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = $"{hiddenKeysActual}|||{hiddenKeysEndWeek}|||{hiddenKeysEOM}", .IsHeader = False, .DataType = XFDataType.Text})
				
				tv.Rows.Add(row)
			Next
			
			' Add INTernal subtotal row
			Me.AddSubtotalRow(tv, "Total INTernal", totalActualInternal, totalEndWeekInternal, totalEOMInternal)
			
			' Add EXTernal subtotal row
			Me.AddSubtotalRow(tv, "Total EXTernal", totalActualExternal, totalEndWeekExternal, totalEOMExternal)
			
			' Add total row
			' For FINANCING - AVAILABLE LINES: use only EXTernal (not INTernal + EXTernal)
			' For other sections: use INTernal + EXTernal
			Dim totalActual As Decimal
			Dim totalEndWeek As Decimal
			Dim totalEOM As Decimal
			If accountName = "FINANCING - AVAILABLE LINES" Then
				totalActual = totalActualExternal
				totalEndWeek = totalEndWeekExternal
				totalEOM = totalEOMExternal
			Else
				totalActual = totalActualInternal + totalActualExternal
				totalEndWeek = totalEndWeekInternal + totalEndWeekExternal
				totalEOM = totalEOMInternal + totalEOMExternal
			End If
			Me.AddSubtotalRow(tv, totalLabel, totalActual, totalEndWeek, totalEOM)
			
			' Return totals via ByRef parameters
			totalActualOut = totalActual
			totalEndWeekOut = totalEndWeek
			totalEOMOut = totalEOM
			
		Catch ex As Exception
			Throw ex
		End Try
	End Sub

	Private Sub AddSubtotalRow(ByVal tv As TableView, ByVal label As String, _
							   ByVal actualTotal As Decimal, ByVal endWeekTotal As Decimal, ByVal eomTotal As Decimal)
		Try
			Dim subtotalRow As New TableViewRow()
			
			subtotalRow.Items.Add("DescActual", New TableViewColumn() With {.Value = label, .IsHeader = False})
			subtotalRow.Items.Add("Actual", New TableViewColumn() With {.Value = actualTotal.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
			subtotalRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
			subtotalRow.Items.Add("Separator1", New TableViewColumn() With {.Value = "", .IsHeader = False})
			subtotalRow.Items.Add("Separator2", New TableViewColumn() With {.Value = "", .IsHeader = False})
			subtotalRow.Items.Add("DescEndWeek", New TableViewColumn() With {.Value = label, .IsHeader = False})
			subtotalRow.Items.Add("EndWeek", New TableViewColumn() With {.Value = endWeekTotal.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
			subtotalRow.Items.Add("BankEndWeek", New TableViewColumn() With {.Value = "", .IsHeader = False})
			subtotalRow.Items.Add("Separator3", New TableViewColumn() With {.Value = "", .IsHeader = False})
			subtotalRow.Items.Add("Separator4", New TableViewColumn() With {.Value = "", .IsHeader = False})
			subtotalRow.Items.Add("DescEOM", New TableViewColumn() With {.Value = label, .IsHeader = False})
			subtotalRow.Items.Add("EOM", New TableViewColumn() With {.Value = eomTotal.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
			subtotalRow.Items.Add("BankEOM", New TableViewColumn() With {.Value = "", .IsHeader = False})
			subtotalRow.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = "", .IsHeader = False, .DataType = XFDataType.Text})				
			tv.Rows.Add(subtotalRow)
		Catch ex As Exception
			Throw ex
		End Try
	End Sub

	Private Sub AddSeparatorRow(ByVal tv As TableView)
		Try
			Dim blankRow As New TableViewRow()
			
			blankRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("Actual", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("Separator1", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("Separator2", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("DescEndWeek", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("EndWeek", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("BankEndWeek", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("Separator3", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("Separator4", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("DescEOM", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("EOM", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("BankEOM", New TableViewColumn() With {.Value = "", .IsHeader = False})
			blankRow.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = "", .IsHeader = False, .DataType = XFDataType.Text})				
			tv.Rows.Add(blankRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Sub AddSectionHeaderRow(ByVal tv As TableView, ByVal sectionTitle As String)
		Try
			Dim headerRow As New TableViewRow()
			
			headerRow.Items.Add("Description", New TableViewColumn() With {.Value = sectionTitle, .IsHeader = False})
			headerRow.Items.Add("Actual", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("Separator1", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("Separator2", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("EndWeek", New TableViewColumn() With {.Value = sectionTitle, .IsHeader = False})
			headerRow.Items.Add("BankEndWeek", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("Separator3", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("Separator4", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("EOM", New TableViewColumn() With {.Value = sectionTitle, .IsHeader = False})
			headerRow.Items.Add("BankEOM", New TableViewColumn() With {.Value = "", .IsHeader = False})
			headerRow.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = "", .IsHeader = False, .DataType = XFDataType.Text})				
			tv.Rows.Add(headerRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		#End Region
		
		#Region "Save Data"

		Private Function SaveCashDebtPositionChanges(ByVal si As SessionInfo, ByVal args As SpreadsheetArgs) As Boolean
			Try
				' Validate TableView exists
				Dim tableView As TableView = args.TableView
				If tableView Is Nothing OrElse tableView.Rows.Count = 0 Then
					Return False
				End If
				
				' CRITICAL: Recover parameters from first row (format: PARAMS|entityId|week|year)
				Dim paramEntityId As String = ""
				Dim paramWeek As String = ""
				Dim paramYear As String = ""
				
				If tableView.Rows.Count > 0 Then
					Dim paramRow As TableViewRow = tableView.Rows(0)
					
					If paramRow.Items.ContainsKey("DescActual") Then
						Dim paramValue As String = If(paramRow.Items("DescActual").Value IsNot Nothing, _
							paramRow.Items("DescActual").Value.ToString(), "")
						
						If paramValue.StartsWith("PARAMS|") Then
							Dim parts As String() = paramValue.Split("|"c)
							If parts.Length >= 4 Then
								paramEntityId = parts(1)
								paramWeek = parts(2)
								paramYear = parts(3)
							End If
						End If
					End If
				End If
				
				' Validate parameters
				If String.IsNullOrEmpty(paramEntityId) OrElse String.IsNullOrEmpty(paramYear) OrElse String.IsNullOrEmpty(paramWeek) Then
					Return False
				End If
				
				' Convert company ID to company name
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramEntityId)
				
				' Calculate TimeKey and scenario
				Dim timeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear, paramWeek)
				Dim expectedScenario As String = "FW" + paramWeek.PadLeft(2, "0"c)
				
				' Get EOM month number
				Dim validWeek As Integer
				If Not Integer.TryParse(paramWeek, validWeek) Then
					validWeek = 1
				End If
				Dim eomMonth As Integer = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMonthFromWeek(si, paramYear, validWeek)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Check if week is confirmed (locked)
					Dim weekInt As Integer
					Dim yearInt As Integer
					If Not Integer.TryParse(paramWeek, weekInt) OrElse Not Integer.TryParse(paramYear, yearInt) Then
						Return False
					End If
					
					Dim confirmCheckSql As String = $"
						SELECT CashDebt 
						FROM XFC_TRS_AUX_TreasuryWeekConfirm 
						WHERE Entity = '{paramEntity.Replace("'", "''")}' 
						AND Week = {weekInt} 
						AND Year = {yearInt}
					"
					
					Dim confirmDt As DataTable = BRApi.Database.ExecuteSql(dbConn, confirmCheckSql, False)
					
					If confirmDt.Rows.Count > 0 Then
						Dim isConfirmed As Boolean = If(IsDBNull(confirmDt.Rows(0)("CashDebt")), False, Convert.ToBoolean(confirmDt.Rows(0)("CashDebt")))
						
						If isConfirmed Then
							Throw New XFException(si, New Exception($"Week {weekInt} of {yearInt} is confirmed. No modifications allowed."))
						End If
					End If
					
				' Define editable flows
				' Note: CASH AND FINANCING BALANCE uses "Cash", FINANCING sections use "Debt"
				Dim editableFlows As New HashSet(Of String) From {
					"INTernal Cash (+)",
					"INTernal Cash (-)",
					"EXTernal Cash (+)",
					"EXTernal Cash (-)",
					"INTernal Debt (+)",
					"INTernal Debt (-)",
					"EXTernal Debt (+)",
					"EXTernal Debt (-)"
				}					' Load original values from database for all projection types
					Dim originalValues As New Dictionary(Of String, Dictionary(Of String, Object))
					
					' Query for all three projection types
					Dim originalDataSql As String = $"
						SELECT 
							UploadTimekey,
							Entity,
							Scenario,
							Account,
							Flow,
							Bank,
							Amount,
							ProjectionType,
							ProjectionMonthNumber
						FROM XFC_TRS_Master_CashDebtPosition 
						WHERE UploadTimekey = '{timeKey}' 
						AND Entity = '{paramEntity.Replace("'", "''")}'
						AND Scenario = '{expectedScenario}'
						AND ProjectionType IN ('StartWeek', 'EndWeek', 'EOM')
						AND (ProjectionType <> 'EOM' OR ProjectionMonthNumber = {eomMonth})
						AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
					"
					
					Dim originalDataDt As DataTable = BRApi.Database.ExecuteSql(dbConn, originalDataSql, False)
					
					' Build dictionary with keys: ProjectionType|UploadTimekey|Entity|Scenario|Account|Flow|Bank
					For Each row As DataRow In originalDataDt.Rows
						Dim projType As String = If(IsDBNull(row("ProjectionType")), "", row("ProjectionType").ToString())
						Dim uploadTk As String = If(IsDBNull(row("UploadTimekey")), "", row("UploadTimekey").ToString())
						Dim entity As String = If(IsDBNull(row("Entity")), "", row("Entity").ToString())
						Dim scenario As String = If(IsDBNull(row("Scenario")), "", row("Scenario").ToString())
						Dim account As String = If(IsDBNull(row("Account")), "", row("Account").ToString())
						Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString())
						Dim bank As String = If(IsDBNull(row("Bank")), "", row("Bank").ToString())
						Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))
						Dim projMonth As Integer = If(IsDBNull(row("ProjectionMonthNumber")), 0, Convert.ToInt32(row("ProjectionMonthNumber")))
						
						Dim key As String = $"{projType}|{uploadTk}|{entity}|{scenario}|{account}|{flow}|{bank}"
						originalValues(key) = New Dictionary(Of String, Object) From {
							{"Amount", amount},
							{"Bank", bank},
							{"ProjectionMonthNumber", projMonth}
						}
					Next
					
					' Process TableView rows using HiddenKeys pattern
					Dim changesDetected As Integer = 0
					
					For Each tvRow As TableViewRow In tableView.Rows
						' Skip header rows
						If tvRow.IsHeader Then Continue For
						
						' Get HiddenKeys column
						If Not tvRow.Items.ContainsKey("HiddenKeys") Then Continue For
						Dim hiddenKeysValueFull As String = tvRow.Items("HiddenKeys").Value?.ToString()
						
						' Skip rows without HiddenKeys (subtotals, totals, blank rows)
						If String.IsNullOrEmpty(hiddenKeysValueFull) Then Continue For
						
						' Split into individual key sets (StartWeek, EndWeek, EOM)
						Dim keySets As String() = hiddenKeysValueFull.Split(New String() {"|||"}, StringSplitOptions.RemoveEmptyEntries)
						
						For Each hiddenKeysValue As String In keySets
							If Not hiddenKeysValue.StartsWith("KEYS|") Then Continue For
							
							' Parse HiddenKeys: KEYS|ProjectionType|uploadTimekey|entity|scenario|account|flow|bank
							Dim keyParts As String() = hiddenKeysValue.Split("|"c)
							If keyParts.Length < 8 Then Continue For
							
							Dim projectionType As String = keyParts(1)
							Dim originalUploadTimekey As String = keyParts(2)
							Dim originalEntity As String = keyParts(3)
							Dim originalScenario As String = keyParts(4)
							Dim originalAccount As String = keyParts(5)
							Dim originalFlow As String = keyParts(6)
							Dim originalBank As String = keyParts(7)
							
							' Only process editable flows
							If Not editableFlows.Contains(originalFlow) Then Continue For
							
							' Determine which column to read based on projection type
							Dim newAmountStr As String = ""
							Dim columnName As String = ""
							Dim bankColumnName As String = ""
							
							Select Case projectionType
								Case "StartWeek"
									columnName = "Actual"
									bankColumnName = "BankActual"
								Case "EndWeek"
									columnName = "EndWeek"
									bankColumnName = "BankEndWeek"
								Case "EOM"
									columnName = "EOM"
									bankColumnName = "BankEOM"
								Case Else
									Continue For
							End Select
							
							' Get new values from TableView
							Dim newBank As String = ""
							
							If tvRow.Items.ContainsKey(columnName) Then
								Dim colValue As Object = tvRow.Items(columnName).Value
								If colValue IsNot Nothing Then
									If TypeOf colValue Is Decimal Then
										newAmountStr = DirectCast(colValue, Decimal).ToString(CultureInfo.InvariantCulture)
									Else
										newAmountStr = colValue.ToString()
									End If
								End If
							End If
							
							If tvRow.Items.ContainsKey(bankColumnName) Then
								newBank = tvRow.Items(bankColumnName).Value?.ToString()
							End If
							
							' Parse new amount
							Dim newAmount As Decimal = 0
							If Not String.IsNullOrEmpty(newAmountStr) Then
								Decimal.TryParse(newAmountStr, NumberStyles.Any, CultureInfo.InvariantCulture, newAmount)
							End If
							
							' Build lookup key
							Dim lookupKey As String = $"{projectionType}|{originalUploadTimekey}|{originalEntity}|{originalScenario}|{originalAccount}|{originalFlow}|{originalBank}"
							
							' Only process if the original value exists
							If originalValues.ContainsKey(lookupKey) Then
								Dim originalData As Dictionary(Of String, Object) = originalValues(lookupKey)
								Dim originalAmount As Decimal = Convert.ToDecimal(originalData("Amount"))
								Dim dbOriginalBank As String = originalData("Bank").ToString()
								
								' Check if amount or bank changed
								Dim amountChanged As Boolean = Math.Abs(originalAmount - newAmount) > 0.001D
								Dim bankChanged As Boolean = (dbOriginalBank <> newBank)
								
								If amountChanged OrElse bankChanged Then
									' Build UPDATE statement
									Dim updateSql As String = ""
									
									If projectionType = "EOM" Then
										updateSql = $"
											UPDATE XFC_TRS_Master_CashDebtPosition 
											SET Amount = {newAmount.ToString(CultureInfo.InvariantCulture)}, 
												Bank = '{newBank.Replace("'", "''")}'
											WHERE UploadTimekey = '{originalUploadTimekey}' 
											AND Entity = '{originalEntity.Replace("'", "''")}'
											AND Scenario = '{originalScenario}'
											AND ProjectionType = '{projectionType}'
											AND ProjectionMonthNumber = {eomMonth}
											AND Account = '{originalAccount.Replace("'", "''")}'
											AND Flow = '{originalFlow.Replace("'", "''")}'
											AND Bank = '{originalBank.Replace("'", "''")}'
										"
									Else
										updateSql = $"
											UPDATE XFC_TRS_Master_CashDebtPosition 
											SET Amount = {newAmount.ToString(CultureInfo.InvariantCulture)}, 
												Bank = '{newBank.Replace("'", "''")}'
											WHERE UploadTimekey = '{originalUploadTimekey}' 
											AND Entity = '{originalEntity.Replace("'", "''")}'
											AND Scenario = '{originalScenario}'
											AND ProjectionType = '{projectionType}'
											AND Account = '{originalAccount.Replace("'", "''")}'
											AND Flow = '{originalFlow.Replace("'", "''")}'
											AND Bank = '{originalBank.Replace("'", "''")}'
										"
									End If
									
									Try
										BRApi.Database.ExecuteSql(dbConn, updateSql, False)
										changesDetected += 1
									Catch ex As Exception
										Throw New XFException(si, ex)
									End Try
								End If
							End If
						Next
					Next
					
				Return True
				
			End Using
			
		Catch ex As Exception
			Throw New XFException(si, ex)
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
