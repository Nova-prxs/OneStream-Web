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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_CashDebtPosition_CashFinanceBalance_StartWeek
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

		#Region "Get Report - Start of Week Only"

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
				
				' STEP 4: Execute optimized query for StartWeek data only
				Dim actualData As DataTable = Me.GetActualData(si, timeKey, paramEntity, expectedScenario)
				
				' STEP 5: Build TableView (even if no data, show structure with zeros)
				Return Me.BuildTableView(actualData)
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function

		#End Region
		
		#Region "Optimized SQL Query"

		Private Function GetActualData(ByVal si As SessionInfo, ByVal timeKey As String, ByVal entity As String, ByVal scenario As String) As DataTable
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Build WHERE clause - handle "HTD" case
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

		#End Region
		
		#Region "Build TableView"

		Private Function BuildTableView(ByVal actualData As DataTable) As TableView
			Try
				Dim tv As New TableView()
				tv.CanModifyData = False
				
				' Define columns: Description, Value, Bank
				tv.Columns.Add(New TableViewColumn() With {.Name = "DescActual", .Value = "Mill EUR", .IsHeader = True})
				tv.Columns.Add(New TableViewColumn() With {.Name = "Actual", .Value = "ACTUAL", .IsHeader = True, .DataType = XFDataType.Decimal})
				tv.Columns.Add(New TableViewColumn() With {.Name = "BankActual", .Value = "BANK", .IsHeader = True})
				
				' Add first header row with period title
				Dim firstHeaderRow As New TableViewRow() With {.IsHeader = True}
				firstHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
				firstHeaderRow.Items.Add("Actual", New TableViewColumn() With {.Value = "Start of week", .IsHeader = True})
				firstHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
				tv.Rows.Add(firstHeaderRow)
				
				' Add second header row with CASH AND FINANCING BALANCE section and column labels
				Dim secondHeaderRow As New TableViewRow() With {.IsHeader = True}
				secondHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "CASH AND FINANCING BALANCE", .IsHeader = True})
				secondHeaderRow.Items.Add("Actual", New TableViewColumn() With {.Value = "Actual", .IsHeader = True})
				secondHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "Bank", .IsHeader = True})
				tv.Rows.Add(secondHeaderRow)
				
				' Build lookup dictionary for efficient data retrieval
				Dim actualDict As New Dictionary(Of String, List(Of DataRow))
				
				For Each dr As DataRow In actualData.Rows
					Dim key As String = $"{dr("Account")}|{dr("Flow")}"
					If Not actualDict.ContainsKey(key) Then
						actualDict(key) = New List(Of DataRow)
					End If
					actualDict(key).Add(dr)
				Next
				
				' Process all three sections with detail rows including bank column
				Dim cashAvailableActual As Decimal = 0
				Dim utilizedDebtActual As Decimal = 0
				Dim availableFinancingActual As Decimal = 0
				
				' Section 1: CASH AND FINANCING BALANCE
				Me.AddSectionRows(tv, "CASH AND FINANCING BALANCE", actualDict, "Cash available", cashAvailableActual)
				Me.AddSeparatorRow(tv)
				
				' Section 2: FINANCING - USED LINES
				Me.AddSectionRows(tv, "FINANCING - USED LINES", actualDict, "Utilized debt", utilizedDebtActual)
				Me.AddSeparatorRow(tv)
				
				' Section 3: FINANCING - AVAILABLE LINES
				Me.AddSectionRows(tv, "FINANCING - AVAILABLE LINES", actualDict, "Available financing", availableFinancingActual)
				Me.AddSeparatorRow(tv)
				
				' Add Net Financial Position (Cash available - Utilized debt)
				Dim netFinancialPositionActual As Decimal = cashAvailableActual - utilizedDebtActual
				Me.AddSubtotalRow(tv, "Net Financial Position", netFinancialPositionActual)
				
				' Configure formatting
				tv.HeaderFormat.BackgroundColor = XFColors.Black
				tv.HeaderFormat.IsBold = True
				tv.HeaderFormat.TextColor = XFColors.White
				
				Return tv
				
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		Private Sub AddSectionTotalsOnly(ByVal tv As TableView, ByVal accountName As String, _
										 ByVal actualDict As Dictionary(Of String, List(Of DataRow)), _
										 ByVal totalLabel As String, _
										 ByRef totalActualOut As Decimal)
			Try
				' Add section header row ONLY for FINANCING sections
				If accountName <> "CASH AND FINANCING BALANCE" Then
					Dim sectionHeaderRow As New TableViewRow() With {.IsHeader = True}
					sectionHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = accountName, .IsHeader = True})
					sectionHeaderRow.Items.Add("Actual", New TableViewColumn() With {.Value = "", .IsHeader = True})
					tv.Rows.Add(sectionHeaderRow)
				End If
				
				' Calculate Internal and External totals
				Dim totalActualInternal As Decimal = 0
				Dim totalActualExternal As Decimal = 0
				
				For Each kvp As KeyValuePair(Of String, List(Of DataRow)) In actualDict
					Dim keyParts As String() = kvp.Key.Split("|"c)
					If keyParts.Length >= 2 AndAlso keyParts(0) = accountName Then
						Dim flowName As String = keyParts(1)
						
						For Each dr As DataRow In kvp.Value
							Dim amount As Decimal = If(IsDBNull(dr("Amount")), 0, Convert.ToDecimal(dr("Amount")))
							
							' Accumulate based on flow type
							If flowName.StartsWith("INTernal") Then
								totalActualInternal += amount
							ElseIf flowName.StartsWith("EXTernal") Then
								totalActualExternal += amount
							End If
						Next
					End If
				Next
				
				' Add Total Internal row
				Me.AddSubtotalRow(tv, "Total INTernal", totalActualInternal)
				
				' Add Total External row
				Me.AddSubtotalRow(tv, "Total EXTernal", totalActualExternal)
				
				' Add section total row
				Dim totalActual As Decimal = totalActualInternal + totalActualExternal
				Me.AddSubtotalRow(tv, totalLabel, totalActual)
				
				' Return total via ByRef parameter
				totalActualOut = totalActual
				
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Function CalculateSectionTotal(ByVal actualDict As Dictionary(Of String, List(Of DataRow)), ByVal accountName As String) As Decimal
			Try
				Dim total As Decimal = 0
				
				' Sum all amounts for this account
				For Each kvp As KeyValuePair(Of String, List(Of DataRow)) In actualDict
					Dim keyParts As String() = kvp.Key.Split("|"c)
					If keyParts.Length >= 2 AndAlso keyParts(0) = accountName Then
						For Each dr As DataRow In kvp.Value
							Dim amount As Decimal = If(IsDBNull(dr("Amount")), 0, Convert.ToDecimal(dr("Amount")))
							total += amount
						Next
					End If
				Next
				
				Return total
			Catch ex As Exception
				Throw ex
			End Try
		End Function

		Private Sub AddSectionRows(ByVal tv As TableView, ByVal accountName As String, _
								   ByVal actualDict As Dictionary(Of String, List(Of DataRow)), _
								   ByVal totalLabel As String, _
								   Optional ByRef totalActualOut As Decimal = 0)
			Try
				' Add section header row ONLY for FINANCING sections (not for CASH AND FINANCING BALANCE)
				If accountName <> "CASH AND FINANCING BALANCE" Then
					Dim sectionHeaderRow As New TableViewRow() With {.IsHeader = True}
					sectionHeaderRow.Items.Add("DescActual", New TableViewColumn() With {.Value = accountName, .IsHeader = True})
					sectionHeaderRow.Items.Add("Actual", New TableViewColumn() With {.Value = "", .IsHeader = True})
					sectionHeaderRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = True})
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
				
				' Get all unique Flow|Bank combinations from the dataset for this account
				Dim allFlowBankCombos As New HashSet(Of String)
				
				' Collect all Flow|Bank combinations that EXIST in the data
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
				
				' Add mandatory flows ONLY if they don't already exist in the data
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
				
				' Sort Flow|Bank combinations: INTernal first, then EXTernal
				Dim sortedFlowBanks As List(Of String) = allFlowBankCombos.OrderBy(Function(fb) If(fb.Split("|"c)(0).StartsWith("INTernal"), 1, If(fb.Split("|"c)(0).StartsWith("EXTernal"), 2, 3))).ThenBy(Function(fb) fb).ToList()
				
				' Accumulators for subtotals
				Dim totalActualInternal As Decimal = 0
				Dim totalActualExternal As Decimal = 0
				
				' Process each flow|bank combination
				For Each flowBank As String In sortedFlowBanks
					Dim fbParts As String() = flowBank.Split("|"c)
					If fbParts.Length < 2 Then Continue For
					
					Dim flow As String = fbParts(0)
					Dim bank As String = fbParts(1)
					
					Dim row As New TableViewRow()
					
					Dim key As String = $"{accountName}|{flow}"
					
					' Get values from dataset for this specific bank
					Dim actualValue As Decimal = 0
					Dim bankActualValue As String = bank
					
					' Get Actual data for this specific bank
					If actualDict.ContainsKey(key) Then
						For Each drActual As DataRow In actualDict(key)
							Dim drBank As String = If(IsDBNull(drActual("Bank")), "", drActual("Bank").ToString())
							If drBank = bank Then
								actualValue = If(IsDBNull(drActual("Amount")), 0, Convert.ToDecimal(drActual("Amount")))
								bankActualValue = drBank
								Exit For
							End If
						Next
					End If
					
					' Accumulate subtotals
					If flow.StartsWith("INTernal") Then
						totalActualInternal += actualValue
					ElseIf flow.StartsWith("EXTernal") Then
						totalActualExternal += actualValue
					End If
					
					' Add row with 3 columns (Description + Value + Bank)
					row.Items.Add("DescActual", New TableViewColumn() With {.Value = flow, .IsHeader = False})
					row.Items.Add("Actual", New TableViewColumn() With {.Value = actualValue.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
					row.Items.Add("BankActual", New TableViewColumn() With {.Value = bankActualValue, .IsHeader = False})
					
					tv.Rows.Add(row)
				Next
				
				' Add INTernal subtotal row
				Me.AddSubtotalRow(tv, "Total INTernal", totalActualInternal)
				
				' Add EXTernal subtotal row
				Me.AddSubtotalRow(tv, "Total EXTernal", totalActualExternal)
				
				' Add total row
				' For FINANCING - AVAILABLE LINES: use only EXTernal (not INTernal + EXTernal)
				' For other sections: use INTernal + EXTernal
				Dim totalActual As Decimal
				If accountName = "FINANCING - AVAILABLE LINES" Then
					totalActual = totalActualExternal
				Else
					totalActual = totalActualInternal + totalActualExternal
				End If
				Me.AddSubtotalRow(tv, totalLabel, totalActual)
				
				' Return totals via ByRef parameters
				totalActualOut = totalActual
				
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Sub AddSubtotalRow(ByVal tv As TableView, ByVal label As String, ByVal actualTotal As Decimal)
			Try
				Dim subtotalRow As New TableViewRow()
				
				subtotalRow.Items.Add("DescActual", New TableViewColumn() With {.Value = label, .IsHeader = False})
				subtotalRow.Items.Add("Actual", New TableViewColumn() With {.Value = actualTotal.ToString("F1", CultureInfo.InvariantCulture), .IsHeader = False, .DataType = XFDataType.Decimal})
				subtotalRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
				
				tv.Rows.Add(subtotalRow)
			Catch ex As Exception
				Throw ex
			End Try
		End Sub

		Private Sub AddSeparatorRow(ByVal tv As TableView)
			Try
				Dim blankRow As New TableViewRow()
				
				blankRow.Items.Add("DescActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
				blankRow.Items.Add("BankActual", New TableViewColumn() With {.Value = "", .IsHeader = False})
				blankRow.Items.Add("Actual", New TableViewColumn() With {.Value = "", .IsHeader = False})
				
				tv.Rows.Add(blankRow)
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
