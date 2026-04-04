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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Spreadsheet.TRS_Treasury_Monitoring
	Public Class MainClass
		
		' Get Treasury Monitoring data filtered by dashboard parameters
		Private Function GetTreasuryMonitoringData(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As TableView
			Try
				' Get parameter values from spreadsheet args
				Dim entityId As String = ""
				Dim weekStarting As String = ""
				Dim year As String = ""
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_CompanyNames") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")) Then
					entityId = args.CustSubstVarsAlreadyResolved("prm_Treasury_CompanyNames")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")) Then
					weekStarting = args.CustSubstVarsAlreadyResolved("prm_Treasury_WeekNumber")
				End If
				
				If args.CustSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year") AndAlso 
				   Not String.IsNullOrEmpty(args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")) Then
					year = args.CustSubstVarsAlreadyResolved("prm_Treasury_Year")
				End If
				
				' Validate required parameters - Allow HTD as valid value
				If String.IsNullOrEmpty(entityId) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Company parameter is missing")
				End If
				
				' Log parameter values for debugging
				If String.IsNullOrEmpty(weekStarting) OrElse Not IsNumeric(weekStarting) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Week parameter is missing or invalid")
				End If
				
				If String.IsNullOrEmpty(year) OrElse Not IsNumeric(year) Then
					Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Year parameter is missing or invalid")
				End If
				
				' Build SQL query to retrieve treasury monitoring data
				' Handle HTD filter - OneStream sends "0" when HTD is selected in dashboard
				Dim whereClause As String = ""
				Dim trimmedEntityId As String = entityId.Trim()
				
				' Check if it's HTD - can be literal "HTD" or "0" from dashboard
				If trimmedEntityId = "HTD" OrElse trimmedEntityId = "0" Then
					whereClause = $"WHERE week_starting = {weekStarting} AND Year = {year}"
				Else
					' Validate entityId is numeric when not HTD
					If Not IsNumeric(trimmedEntityId) Then
						Return Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.CreateErrorTableView("Error: Company parameter must be numeric or HTD")
					End If
					whereClause = $"WHERE Entity_Id = {trimmedEntityId} AND week_starting = {weekStarting} AND Year = {year}"
				End If
				
				Dim sql As String = $"
					SELECT 
						Entity_Id,
						Timekey,
						Date,
						Year,
						Scenario,
						Entity,
						week_starting,
						alert_level_status,
						CAST(alert_level AS NVARCHAR(50)) AS alert_level,
						issue,
						analysis,
						solution
					FROM XFC_TRS_MASTER_Treasury_Monitoring
					{whereClause}
					ORDER BY Entity_Id, Date, Scenario
				"
				
				' Execute query and get DataTable
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql, False)
					
					' Log the number of rows retrieved
					' Create TableView
					Dim tv As New TableView()
					tv.CanModifyData = True ' Allow editing
					
			' Add columns for the table
			tv.Columns.Add(New TableViewColumn() With {.Name = "AsOf", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "WeekStarting", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "BusinessUnit", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "DebtPositionStatus", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "CashFlowStatus", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "AlertLevel", .Value = "", .IsHeader = True, .DataType = XFDataType.Int16})
			tv.Columns.Add(New TableViewColumn() With {.Name = "AlertStatus", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "Issue", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "Analysis", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "Solution", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
			tv.Columns.Add(New TableViewColumn() With {.Name = "HiddenKeys", .Value = "", .IsHeader = True, .DataType = XFDataType.Text})
					
					' Add header row with column titles
					Dim headerRow As New TableViewRow() With {.IsHeader = True}
					headerRow.Items.Add("AsOf", New TableViewColumn() With {.Value = "As of", .IsHeader = True})
					headerRow.Items.Add("WeekStarting", New TableViewColumn() With {.Value = "Week Starting", .IsHeader = True})
					headerRow.Items.Add("BusinessUnit", New TableViewColumn() With {.Value = "Business Unit", .IsHeader = True})
					headerRow.Items.Add("DebtPositionStatus", New TableViewColumn() With {.Value = "Debt Position Status", .IsHeader = True})
					headerRow.Items.Add("CashFlowStatus", New TableViewColumn() With {.Value = "CashFlow Status", .IsHeader = True})
					headerRow.Items.Add("AlertLevel", New TableViewColumn() With {.Value = "Alert Level", .IsHeader = True})
					headerRow.Items.Add("AlertStatus", New TableViewColumn() With {.Value = "Alert Status", .IsHeader = True})
					headerRow.Items.Add("Issue", New TableViewColumn() With {.Value = "Issue", .IsHeader = True})
					headerRow.Items.Add("Analysis", New TableViewColumn() With {.Value = "Analysis", .IsHeader = True})
					headerRow.Items.Add("Solution", New TableViewColumn() With {.Value = "Solution", .IsHeader = True})
					headerRow.Items.Add("HiddenKeys", New TableViewColumn() With {.Value = "", .IsHeader = True})
					tv.Rows.Add(headerRow)
					
					' Log before processing rows
					' Add data rows from DataTable
					Dim rowsAdded As Integer = 0
					For Each dataRow As DataRow In dt.Rows
						Dim tvRow As New TableViewRow() With {.IsHeader = False}
						
						' Get values
						Dim entityIdValue As String = dataRow("Entity_Id").ToString()
						Dim timekeyValue As String = dataRow("Timekey").ToString()
						Dim dateValue As String = If(dataRow("Date") IsNot DBNull.Value, Convert.ToDateTime(dataRow("Date")).ToString("dd/MM/yyyy"), "")
						Dim entityName As String = If(dataRow("Entity") IsNot DBNull.Value, dataRow("Entity").ToString(), "")
						
						' Get confirmation status for THIS specific entity
						Dim cashPositionStatus As String = "Not uploaded"
						Dim cashFlowStatus As String = "Not uploaded"
						
						Try
							Dim checkSql As String = "
								SELECT CashDebt, CashFlow
								FROM XFC_TRS_AUX_TreasuryWeekConfirm
								WHERE Entity = @entity AND Week = @week AND Year = @year
							"
							
							Dim checkParams As New List(Of DbParamInfo) From {
								New DbParamInfo("entity", entityName),
								New DbParamInfo("week", Integer.Parse(weekStarting)),
								New DbParamInfo("year", Integer.Parse(year))
							}
							
							Dim checkDt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, checkSql, checkParams, False)
							
							If checkDt.Rows.Count > 0 Then
								' Check CashDebt status
								Dim cashDebtBit As Object = checkDt.Rows(0)("CashDebt")
								If Not IsDBNull(cashDebtBit) AndAlso Convert.ToBoolean(cashDebtBit) Then
									cashPositionStatus = "Confirmed"
								Else
									cashPositionStatus = "Unconfirmed"
								End If
								
								' Check CashFlow status
								Dim cashFlowBit As Object = checkDt.Rows(0)("CashFlow")
								If Not IsDBNull(cashFlowBit) AndAlso Convert.ToBoolean(cashFlowBit) Then
									cashFlowStatus = "Confirmed"
								Else
									cashFlowStatus = "Unconfirmed"
								End If
							End If
						Catch ex As Exception
						End Try
						
						' As Of column
						tvRow.Items.Add("AsOf", New TableViewColumn() With {
							.Value = dateValue,
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' Week Starting column
						tvRow.Items.Add("WeekStarting", New TableViewColumn() With {
							.Value = If(dataRow("week_starting") IsNot DBNull.Value, dataRow("week_starting").ToString(), ""),
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' Business Unit column
						tvRow.Items.Add("BusinessUnit", New TableViewColumn() With {
							.Value = entityName,
							.IsHeader = True,
							.DataType = XFDataType.Text
						})
						
						' Debt Position Status column (renamed from CashPositionStatus)
						tvRow.Items.Add("DebtPositionStatus", New TableViewColumn() With {
							.Value = cashPositionStatus,
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' CashFlow Status column
						tvRow.Items.Add("CashFlowStatus", New TableViewColumn() With {
							.Value = cashFlowStatus,
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' Alert Level column - shows alert_level_status (INT) - EDITABLE
						Dim alertLevelInt As Integer = 0
						If dataRow("alert_level_status") IsNot DBNull.Value Then
							Integer.TryParse(dataRow("alert_level_status").ToString(), alertLevelInt)
						End If
						
						tvRow.Items.Add("AlertLevel", New TableViewColumn() With {
							.Value = alertLevelInt,
							.IsHeader = False,
							.DataType = XFDataType.Int16
						})
						
						' Alert Status column - shows alert_level (●) - READ-ONLY
						tvRow.Items.Add("AlertStatus", New TableViewColumn() With {
							.Value = If(dataRow("alert_level") IsNot DBNull.Value, dataRow("alert_level").ToString(), "●"),
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' Issue column
						tvRow.Items.Add("Issue", New TableViewColumn() With {
							.Value = If(dataRow("issue") IsNot DBNull.Value, dataRow("issue").ToString(), ""),
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' Analysis column
						tvRow.Items.Add("Analysis", New TableViewColumn() With {
							.Value = If(dataRow("analysis") IsNot DBNull.Value, dataRow("analysis").ToString(), ""),
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' Solution column
						tvRow.Items.Add("Solution", New TableViewColumn() With {
							.Value = If(dataRow("solution") IsNot DBNull.Value, dataRow("solution").ToString(), ""),
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
						
						' Hidden column with all keys for saving: KEYS|entityId|timekey|week|year
						tvRow.Items.Add("HiddenKeys", New TableViewColumn() With {
							.Value = $"KEYS|{entityIdValue}|{timekeyValue}|{weekStarting}|{year}",
							.IsHeader = False,
							.DataType = XFDataType.Text
						})
					
					tv.Rows.Add(tvRow)
					rowsAdded += 1
					Next
					
					' Log rows added
					' Configure formatting
					tv.HeaderFormat.BackgroundColor = XFColors.Black
					tv.HeaderFormat.IsBold = True
					tv.HeaderFormat.TextColor = XFColors.White
					
					Return tv
				End Using
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		' Save changes to Treasury Monitoring data
		Private Function SaveTreasuryMonitoringData(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As SpreadsheetArgs) As Boolean
			Try
				Dim tableView As TableView = args.TableView
				
				If tableView Is Nothing OrElse tableView.Rows.Count = 0 Then
					Return False
				End If
				
				' Counter for tracking updates
				Dim updatesExecuted As Integer = 0
				
				' Get database connection
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Iterate through rows
					For Each row As TableViewRow In tableView.Rows
						' Skip header rows
						If row.IsHeader Then
							Continue For
						End If
						
						' Extract Entity_Id, Timekey, Week and Year from the HiddenKeys column (format: KEYS|entityId|timekey|week|year)
						If Not row.Items.ContainsKey("HiddenKeys") Then
							Continue For
						End If
						
						Dim hiddenKeysValue As String = If(row.Items("HiddenKeys").Value IsNot Nothing, row.Items("HiddenKeys").Value.ToString(), "")
						
						' Skip rows with empty HiddenKeys
						If String.IsNullOrEmpty(hiddenKeysValue) Then
							Continue For
						End If
						
						' Skip if not in expected format
						If Not hiddenKeysValue.StartsWith("KEYS|") Then
							Continue For
						End If
						
						' Parse the keys: KEYS|entityId|timekey|week|year
						Dim parts As String() = hiddenKeysValue.Split("|"c)
						If parts.Length < 5 Then
							Continue For
						End If
						
						Dim entityId As String = parts(1)
						Dim timekey As String = parts(2)
						Dim weekNum As String = parts(3)
						Dim yearNum As String = parts(4)
						
						' Get editable field values from the row
						' AlertLevel contains the INT value (alert_level_status)
						Dim alertLevelStr As String = If(row.Items.ContainsKey("AlertLevel") AndAlso row.Items("AlertLevel").Value IsNot Nothing, _
							row.Items("AlertLevel").Value.ToString(), "1")
						
						' Convert alert_level_status to INT, default to 1 if invalid
						Dim alertLevelStatus As Integer = 1
						If Not Integer.TryParse(alertLevelStr, alertLevelStatus) Then
							alertLevelStatus = 1
						End If
						
						' Get text fields with proper SQL escaping
						Dim issue As String = If(row.Items.ContainsKey("Issue") AndAlso row.Items("Issue").Value IsNot Nothing, _
							row.Items("Issue").Value.ToString(), "")
						
						Dim analysis As String = If(row.Items.ContainsKey("Analysis") AndAlso row.Items("Analysis").Value IsNot Nothing, _
							row.Items("Analysis").Value.ToString(), "")
						
						Dim solution As String = If(row.Items.ContainsKey("Solution") AndAlso row.Items("Solution").Value IsNot Nothing, _
							row.Items("Solution").Value.ToString(), "")
						
						' Build UPDATE query with proper SQL escaping
						' alert_level_status is INT (from AlertLevel column), alert_level is static '●'
						Dim updateSql As String = $"
							UPDATE XFC_TRS_MASTER_Treasury_Monitoring
							SET 
								alert_level_status = {alertLevelStatus},
								alert_level = N'●',
								issue = N'{issue.Replace("'", "''")}',
								analysis = N'{analysis.Replace("'", "''")}',
								solution = N'{solution.Replace("'", "''")}'
							WHERE Entity_Id = {entityId}
								AND Timekey = '{timekey}'
						"
						
						' Execute update
						Try
							BRApi.Database.ExecuteSql(dbConnApp, updateSql, False)
							updatesExecuted += 1
						Catch sqlEx As Exception
						End Try
					Next
					Return True
				End Using
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
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
						Return GetTreasuryMonitoringData(si, globals, api, args)
						
					Case Is = SpreadsheetFunctionType.SaveTableView
						Return SaveTreasuryMonitoringData(si, globals, api, args)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
