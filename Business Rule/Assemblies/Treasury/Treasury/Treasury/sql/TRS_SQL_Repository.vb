Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	''' <summary>
	''' Centralized SQL Repository for Treasury module
	''' Contains all SQL queries organized by functional module and business domain
	''' All queries use parameterized placeholders for security
	''' </summary>
	Public Class TRS_SQL_Repository
		
		#Region "Table Constants"
		
		''' <summary>Master table for Cash & Debt Position data</summary>
		Public Const TABLE_CASH_DEBT As String = "XFC_TRS_Master_CashDebtPosition"
		
		''' <summary>Master table for Cash Flow Forecasting data</summary>
		Public Const TABLE_CASH_FLOW As String = "XFC_TRS_Master_CashflowForecasting"
		
		''' <summary>Auxiliary table for date dimension</summary>
		Public Const TABLE_AUX_DATE As String = "XFC_TRS_AUX_Date"
		
		''' <summary>Auxiliary table for week confirmation status</summary>
		Public Const TABLE_AUX_WEEK_CONFIRM As String = "XFC_TRS_AUX_TreasuryWeekConfirm"
		
		''' <summary>Master table for Treasury Monitoring</summary>
		Public Const TABLE_TREASURY_MONITORING As String = "XFC_TRS_Master_TreasuryMonitoring"
		
		''' <summary>Master table for Banks</summary>
		Public Const TABLE_BANKS As String = "XFC_TRS_Master_Banks"
		
		''' <summary>Master table for Companies</summary>
		Public Const TABLE_COMPANIES As String = "XFC_TRS_Master_Companies"
		
		#End Region
		
		#Region "SQL Fragment Constants"
		
		''' <summary>Operating + Investment accounts filter (for OF_IF views)</summary>
		Public Const ACCOUNTS_OPER_INV As String = "'OPERATING FLOWS', 'INVESTMENT FLOWS'"
		
		''' <summary>All CashFlow accounts filter (for CF_CB views)</summary>
		Public Const ACCOUNTS_ALL_CASHFLOW As String = "'OPERATING FLOWS', 'INVESTMENT FLOWS', 'FINANCING FLOWS', 'CF NOT INCL. IN OP FCF (INCL. DIVIDENDS)', 'OTHER FLOWS (DIV)'"
		
		''' <summary>CashDebt main accounts filter</summary>
		Public Const ACCOUNTS_CASH_DEBT As String = "'CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES'"
		
		''' <summary>Flow order CASE expression for consistent sorting</summary>
		Public Const FLOW_ORDER_CASE As String = "CASE WHEN Flow LIKE 'INTernal%' THEN 1 WHEN Flow LIKE 'EXTernal%' THEN 2 ELSE 3 END"
		
		#End Region
		
		#Region "SQL Builder Helpers"
		
		''' <summary>
		''' Builds Spanish date label expression (e.g., "DIC-05")
		''' </summary>
		Public Shared Function BuildDateLabel(dateColumn As String) As String
			Return $"UPPER(FORMAT({dateColumn}, 'MMM', 'es-ES')) + '-' + RIGHT('0' + CAST(DAY({dateColumn}) AS VARCHAR(2)), 2)"
		End Function
		
		''' <summary>
		''' Builds Actual/Projection filter for CashFlow queries
		''' Used when mixing Actual data (past weeks) with Projection data (current/future weeks)
		''' </summary>
		Public Shared Function BuildActualProjectionFilter(uploadWeekParam As String) As String
			Return $"
				(ProjectionType = 'Actual' AND UploadWeekNumber = ProjectionWeekNumber + 1 AND ProjectionWeekNumber < {uploadWeekParam})
				OR
				(ProjectionType = 'Projection' AND UploadWeekNumber = {uploadWeekParam} AND ProjectionWeekNumber >= {uploadWeekParam})
			"
		End Function
		
		''' <summary>
		''' Escapes single quotes in SQL string values to prevent injection
		''' </summary>
		Public Shared Function EscapeSql(value As String) As String
			If String.IsNullOrEmpty(value) Then Return value
			Return value.Replace("'", "''")
		End Function
		
		''' <summary>
		''' Gets account filter string based on account type
		''' </summary>
		Public Shared Function GetAccountsFilter(useAllAccounts As Boolean) As String
			If useAllAccounts Then
				Return ACCOUNTS_ALL_CASHFLOW
			Else
				Return ACCOUNTS_OPER_INV
			End If
		End Function
		
		#End Region
		
		#Region "DynamicDashboard"
		
		#Region "CashDebt KPIs"
		
		''' <summary>
		''' Gets SQL for single KPI value from CashDebt table (StartWeek projection)
		''' Parameters: @timeKey, @scenario, @accountName
		''' Optional entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtKPIQuery(entityFilter As String) As String
			Return $"
				SELECT SUM(Amount) AS Total, COUNT(*) AS RecordCount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey = @timeKey
				{entityFilter}
				AND Scenario = @scenario
				AND ProjectionType = 'StartWeek'
				AND Account = @accountName
			"
		End Function
		
		''' <summary>
		''' Gets SQL for single KPI value from CashDebt table (EOM projection)
		''' Parameters: @timeKey, @scenario, @accountName, @projectionMonth
		''' Optional entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtEOMKPIQuery(entityFilter As String) As String
			Return $"
				SELECT SUM(Amount) AS Total, COUNT(*) AS RecordCount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey = @timeKey
				{entityFilter}
				AND Scenario = @scenario
				AND ProjectionType = 'EOM'
				AND ProjectionMonthNumber = @projectionMonth
				AND Account = @accountName
			"
		End Function
		
		#End Region
		
		#Region "CashFlow KPIs"
		
		''' <summary>
		''' Gets SQL for flows total (INFLOWS + OUTFLOWS) for a specific account
		''' Parameters: @timeKey, @scenario, @account, @projectionType, @projectionYear, @projectionWeek
		''' Optional entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashFlowFlowsTotalQuery(entityFilter As String) As String
			Return $"
				SELECT SUM(Amount) AS Total
				FROM {TABLE_CASH_FLOW}
				WHERE UploadTimekey = @timeKey
				{entityFilter}
				AND UPPER(Scenario) = @scenario
				AND UPPER(Account) = @account
				AND ProjectionType = @projectionType
				AND ProjectionYear = @projectionYear
				AND ProjectionWeekNumber = @projectionWeek
				AND UPPER(Flow) IN ('INFLOWS', 'OUTFLOWS')
			"
		End Function
		
		''' <summary>
		''' Gets SQL for account group flows (Operating+Investment or All)
		''' Parameters: @timeKey, @scenario, @projectionType, @projectionYear, @projectionWeek, @flowType
		''' accountFilter: "AND UPPER(Account) IN (...)"
		''' Optional entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashFlowAccountGroupFlowsQuery(entityFilter As String, accountFilter As String) As String
			Return $"
				SELECT SUM(Amount) AS Total
				FROM {TABLE_CASH_FLOW}
				WHERE UploadTimekey = @timeKey
				{entityFilter}
				AND UPPER(Scenario) = @scenario
				{accountFilter}
				AND ProjectionType = @projectionType
				AND ProjectionYear = @projectionYear
				AND ProjectionWeekNumber = @projectionWeek
				AND UPPER(Flow) = @flowType
			"
		End Function
		
		#End Region
		
		#Region "Date Utilities"
		
		''' <summary>
		''' Gets SQL to find max week number in a year
		''' Parameters: year (embedded in query)
		''' Used by: Multiple CashFlow TableViews for determining max weeks
		''' </summary>
		Public Shared Function GetMaxWeekInYearQuery(year As String) As String
			Return $"
				SELECT MAX(weekNumber) AS MaxWeek
				FROM {TABLE_AUX_DATE}
				WHERE year = {year}
			"
		End Function
		
		#End Region
		
		#End Region
		
		#Region "Graphs"
		
		#Region "CashDebt Graphs"
		
		''' <summary>
		''' Gets SQL for Cash Balance Graph comparing two years
		''' Parameters: @weekFrom, @weekTo, @year1, @year2
		''' Entity filter should be appended
		''' </summary>
		Public Shared Function GetCashBalanceGraphQuery(entityFilter As String) As String
			Return $"
				WITH CashBalanceData AS (
					SELECT 
						m.ProjectionYear,
						m.ProjectionWeekNumber,
						d.weekStartDate,
						SUM(m.Amount) AS TotalAmount
					FROM {TABLE_CASH_DEBT} m
					INNER JOIN {TABLE_AUX_DATE} d
						ON CONVERT(VARCHAR(8), d.weekStartDate, 112) = m.ProjectionTimekey
						AND d.fulldate = d.weekStartDate
					WHERE 
						m.Account = 'CASH AND FINANCING BALANCE'
						AND m.ProjectionType = 'StartWeek'
						AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
						AND m.ProjectionYear IN (@year1, @year2)
						{entityFilter}
					GROUP BY 
						m.ProjectionYear,
						m.ProjectionWeekNumber,
						d.weekStartDate
				)
				SELECT 
					ProjectionYear,
					ProjectionWeekNumber,
					weekStartDate,
					TotalAmount,
					UPPER(FORMAT(weekStartDate, 'MMM', 'es-ES')) + '-' + 
					RIGHT('0' + CAST(DAY(weekStartDate) AS VARCHAR(2)), 2) AS DateLabel
				FROM CashBalanceData
				ORDER BY ProjectionWeekNumber, ProjectionYear
			"
		End Function
		
		''' <summary>
		''' Gets SQL for Cash Balance Graph by week number (simplified, no date join)
		''' Parameters: @weekFrom, @weekTo, @year1, @year2
		''' Entity filter should be appended
		''' </summary>
		Public Shared Function GetCashBalanceByWeekNumberQuery(entityFilter As String) As String
			Return $"
				SELECT 
					m.ProjectionYear,
					m.ProjectionWeekNumber,
					SUM(m.Amount) AS TotalAmount
				FROM {TABLE_CASH_DEBT} m
				WHERE 
					m.Account = 'CASH AND FINANCING BALANCE'
					AND m.ProjectionType = 'StartWeek'
					AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
					AND m.ProjectionYear IN (@year1, @year2)
					{entityFilter}
				GROUP BY 
					m.ProjectionYear,
					m.ProjectionWeekNumber
				ORDER BY m.ProjectionYear, m.ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for RFC Lines Graph (Used Lines vs Limit Lines)
		''' Parameters: @weekFrom, @weekTo, @year
		''' Entity filter should be appended
		''' </summary>
		Public Shared Function GetRFCLinesGraphQuery(entityFilter As String) As String
			Return $"
				WITH RFCData AS (
					SELECT 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Account,
						SUM(m.Amount) AS TotalAmount
					FROM {TABLE_CASH_DEBT} m
					INNER JOIN {TABLE_AUX_DATE} d
						ON CONVERT(VARCHAR(8), d.weekStartDate, 112) = m.ProjectionTimekey
						AND d.fulldate = d.weekStartDate
					WHERE 
						m.ProjectionType = 'StartWeek'
						AND m.ProjectionYear = @year
						AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
						AND m.Account IN ('FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
						AND m.Flow = 'EXTernal Debt (-)'
						{entityFilter}
					GROUP BY 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Account
				)
				SELECT 
					ProjectionWeekNumber,
					weekStartDate,
					UPPER(FORMAT(weekStartDate, 'MMM', 'es-ES')) + '-' + 
					RIGHT('0' + CAST(DAY(weekStartDate) AS VARCHAR(2)), 2) AS DateLabel,
					ISNULL(SUM(CASE WHEN Account = 'FINANCING - USED LINES' THEN TotalAmount ELSE 0 END), 0) AS UsedLines,
					ISNULL(
						SUM(CASE WHEN Account = 'FINANCING - USED LINES' THEN TotalAmount ELSE 0 END) +
						SUM(CASE WHEN Account = 'FINANCING - AVAILABLE LINES' THEN TotalAmount ELSE 0 END),
						0
					) AS LimitLines
				FROM RFCData
				GROUP BY ProjectionWeekNumber, weekStartDate
				ORDER BY ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt Current Year Graph (Cash Balance + RFC Lines combined)
		''' Parameters: @weekFrom, @weekTo, @year
		''' Entity filter should be appended
		''' </summary>
		Public Shared Function GetCashDebtCurrentYearQuery(entityFilter As String) As String
			Return $"
				WITH CashBalanceData AS (
					SELECT 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						SUM(m.Amount) AS CashBalance
					FROM {TABLE_CASH_DEBT} m
					INNER JOIN {TABLE_AUX_DATE} d
						ON CONVERT(VARCHAR(8), d.weekStartDate, 112) = m.ProjectionTimekey
						AND d.fulldate = d.weekStartDate
					WHERE 
						m.Account = 'CASH AND FINANCING BALANCE'
						AND m.ProjectionType = 'StartWeek'
						AND m.ProjectionYear = @year
						AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
						{entityFilter}
					GROUP BY 
						m.ProjectionWeekNumber,
						d.weekStartDate
				),
				RFCData AS (
					SELECT 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Account,
						SUM(m.Amount) AS TotalAmount
					FROM {TABLE_CASH_DEBT} m
					INNER JOIN {TABLE_AUX_DATE} d
						ON CONVERT(VARCHAR(8), d.weekStartDate, 112) = m.ProjectionTimekey
						AND d.fulldate = d.weekStartDate
					WHERE 
						m.ProjectionType = 'StartWeek'
						AND m.ProjectionYear = @year
						AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
						AND m.Account IN ('FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
						AND m.Flow = 'EXTernal Debt (-)'
						{entityFilter}
					GROUP BY 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Account
				)
				SELECT 
					COALESCE(cb.ProjectionWeekNumber, rfc.ProjectionWeekNumber) AS ProjectionWeekNumber,
					COALESCE(cb.weekStartDate, rfc.weekStartDate) AS weekStartDate,
					UPPER(FORMAT(COALESCE(cb.weekStartDate, rfc.weekStartDate), 'MMM', 'es-ES')) + '-' + 
					RIGHT('0' + CAST(DAY(COALESCE(cb.weekStartDate, rfc.weekStartDate)) AS VARCHAR(2)), 2) AS DateLabel,
					ISNULL(cb.CashBalance, 0) AS CashBalance,
					ISNULL(SUM(CASE WHEN rfc.Account = 'FINANCING - USED LINES' THEN rfc.TotalAmount ELSE 0 END), 0) AS UsedLines,
					ISNULL(
						SUM(CASE WHEN rfc.Account = 'FINANCING - USED LINES' THEN rfc.TotalAmount ELSE 0 END) +
						SUM(CASE WHEN rfc.Account = 'FINANCING - AVAILABLE LINES' THEN rfc.TotalAmount ELSE 0 END),
						0
					) AS LimitLines
				FROM CashBalanceData cb
				FULL OUTER JOIN RFCData rfc
					ON cb.ProjectionWeekNumber = rfc.ProjectionWeekNumber
					AND cb.weekStartDate = rfc.weekStartDate
				GROUP BY 
					COALESCE(cb.ProjectionWeekNumber, rfc.ProjectionWeekNumber),
					COALESCE(cb.weekStartDate, rfc.weekStartDate),
					cb.CashBalance
				ORDER BY COALESCE(cb.ProjectionWeekNumber, rfc.ProjectionWeekNumber)
			"
		End Function
		
		''' <summary>
		''' Gets SQL for Weekly Cash Position Graph
		''' Parameters: @year, @weekFrom, @weekTo
		''' Entity filter should be appended separately
		''' </summary>
		Public Shared Function GetWeeklyCashPositionQuery(entityFilter As String) As String
			Return $"
				SELECT 
					CAST(SUBSTRING(Scenario, 3, 2) AS INT) AS WeekNumber,
					Flow,
					SUM(Amount) AS Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey LIKE CAST(@year AS VARCHAR(4)) + '%'
				{entityFilter}
				AND Scenario LIKE 'FW%'
				AND ProjectionType = 'StartWeek'
				AND Account = 'CASH AND FINANCING BALANCE'
				AND Flow IN ('INTernal Cash (+)', 'EXTernal Cash (+)')
				AND CAST(SUBSTRING(Scenario, 3, 2) AS INT) BETWEEN @weekFrom AND @weekTo
				GROUP BY SUBSTRING(Scenario, 3, 2), Flow
				ORDER BY CAST(SUBSTRING(Scenario, 3, 2) AS INT), Flow
			"
		End Function
		
		''' <summary>
		''' Gets SQL for Weekly Debt Position Graph
		''' Parameters: @year, @weekFrom, @weekTo
		''' Entity filter should be appended separately
		''' </summary>
		Public Shared Function GetWeeklyDebtPositionQuery(entityFilter As String) As String
			Return $"
				SELECT 
					CAST(SUBSTRING(Scenario, 3, 2) AS INT) AS WeekNumber,
					Flow,
					SUM(Amount) AS Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey LIKE CAST(@year AS VARCHAR(4)) + '%'
				{entityFilter}
				AND Scenario LIKE 'FW%'
				AND ProjectionType = 'StartWeek'
				AND Account = 'FINANCING - USED LINES'
				AND Flow IN ('INTernal Debt (-)', 'EXTernal Debt (-)')
				AND CAST(SUBSTRING(Scenario, 3, 2) AS INT) BETWEEN @weekFrom AND @weekTo
				GROUP BY SUBSTRING(Scenario, 3, 2), Flow
				ORDER BY CAST(SUBSTRING(Scenario, 3, 2) AS INT), Flow
			"
		End Function
		
		#End Region
		
		#Region "CashFlow Graphs"
		
		''' <summary>
		''' Gets SQL for CashFlow Projection Graph (Inflows vs Outflows with mix of Actual/Projection)
		''' Parameters: @weekFrom, @weekTo, @uploadWeek, @year
		''' Entity filter should be appended
		''' </summary>
		''' <param name="includeDateLabel">True to include DateLabel column, False for simplified output</param>
		Public Shared Function GetCashFlowProjectionQuery(entityFilter As String, Optional includeDateLabel As Boolean = True) As String
			Dim dateLabel As String = ""
			If includeDateLabel Then
				dateLabel = $"{BuildDateLabel("weekStartDate")} AS DateLabel,"
			End If
			
			Return $"
				WITH CashFlowData AS (
					SELECT 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Flow,
						SUM(m.Amount) AS TotalAmount
					FROM {TABLE_CASH_FLOW} m
					INNER JOIN {TABLE_AUX_DATE} d
						ON d.year = m.ProjectionYear
						AND d.weekNumber = m.ProjectionWeekNumber
						AND d.fulldate = d.weekStartDate
					WHERE 
						m.UploadYear = @year
						AND m.ProjectionYear = @year
						AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
						AND UPPER(m.Account) IN ({ACCOUNTS_OPER_INV})
						AND UPPER(m.Flow) IN ('INFLOWS', 'OUTFLOWS')
						AND ({BuildActualProjectionFilter("@uploadWeek")})
						{entityFilter}
					GROUP BY 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Flow
				)
				SELECT 
					ProjectionWeekNumber,
					weekStartDate,
					{dateLabel}
					ISNULL(SUM(CASE WHEN UPPER(Flow) = 'INFLOWS' THEN TotalAmount ELSE 0 END), 0) AS Inflows,
					ISNULL(SUM(CASE WHEN UPPER(Flow) = 'OUTFLOWS' THEN TotalAmount ELSE 0 END), 0) AS Outflows
				FROM CashFlowData
				GROUP BY ProjectionWeekNumber, weekStartDate
				ORDER BY ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow 4-Week Rolling Projection (current year data)
		''' Parameters: @year, @currentWeek, @maxWeekCurrentYear
		''' Entity filter should be appended
		''' </summary>
		Public Shared Function GetCashFlow4WeekProjectionCurrentYearQuery(entityFilter As String) As String
			Return $"
				SELECT 
					m.ProjectionYear,
					m.ProjectionWeekNumber,
					m.ProjectionType,
					m.UploadWeekNumber,
					m.Flow,
					SUM(m.Amount) AS TotalAmount
				FROM {TABLE_CASH_FLOW} m
				WHERE 
					m.UploadYear = @year
					AND m.ProjectionYear = @year
					AND m.ProjectionWeekNumber BETWEEN 1 AND @maxWeekCurrentYear
					AND UPPER(m.Account) IN ({ACCOUNTS_OPER_INV})
					AND UPPER(m.Flow) IN ('INFLOWS', 'OUTFLOWS')
					AND ({BuildActualProjectionFilter("@currentWeek")})
					{entityFilter}
				GROUP BY 
					m.ProjectionYear,
					m.ProjectionWeekNumber,
					m.ProjectionType,
					m.UploadWeekNumber,
					m.Flow
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow 4-Week Rolling Projection (next year data)
		''' Parameters: @year, @nextYear, @currentWeek, @weeksNeededNextYear
		''' Entity filter should be appended
		''' </summary>
		Public Shared Function GetCashFlow4WeekProjectionNextYearQuery(entityFilter As String, weeksNeededNextYear As Integer) As String
			Return $"
				SELECT 
					m.ProjectionYear,
					m.ProjectionWeekNumber,
					m.ProjectionType,
					m.UploadWeekNumber,
					m.Flow,
					SUM(m.Amount) AS TotalAmount
				FROM {TABLE_CASH_FLOW} m
				WHERE 
					m.UploadYear = @year
					AND m.ProjectionYear = @nextYear
					AND m.ProjectionType = 'Projection'
					AND m.UploadWeekNumber = @currentWeek
					AND m.ProjectionWeekNumber <= {weeksNeededNextYear}
					AND UPPER(m.Account) IN ({ACCOUNTS_OPER_INV})
					AND UPPER(m.Flow) IN ('INFLOWS', 'OUTFLOWS')
					{entityFilter}
				GROUP BY 
					m.ProjectionYear,
					m.ProjectionWeekNumber,
					m.ProjectionType,
					m.UploadWeekNumber,
					m.Flow
			"
		End Function
		
		''' <summary>
		''' Gets FULL SQL for CashFlow 4-Week Rolling Projection (combined CTE with UNION ALL for both years)
		''' Parameters: @year, @nextYear, @currentWeek, @maxWeekCurrentYear
		''' Entity filter should be appended
		''' </summary>
		Public Shared Function GetCashFlow4WeekProjectionFullQuery(entityFilter As String, weeksNeededNextYear As Integer) As String
			Return $"
				WITH CashFlowData AS (
					-- Datos del año actual
					SELECT 
						m.ProjectionYear,
						m.ProjectionWeekNumber,
						m.ProjectionType,
						m.UploadWeekNumber,
						m.Flow,
						SUM(m.Amount) AS TotalAmount
					FROM {TABLE_CASH_FLOW} m
					WHERE 
						m.UploadYear = @year
						AND m.ProjectionYear = @year
						AND m.ProjectionWeekNumber BETWEEN 1 AND @maxWeekCurrentYear
						AND UPPER(m.Account) IN ({ACCOUNTS_OPER_INV})
						AND UPPER(m.Flow) IN ('INFLOWS', 'OUTFLOWS')
						AND ({BuildActualProjectionFilter("@currentWeek")})
						{entityFilter}
					GROUP BY 
						m.ProjectionYear,
						m.ProjectionWeekNumber,
						m.ProjectionType,
						m.UploadWeekNumber,
						m.Flow
					
					UNION ALL
					
					-- Datos del año siguiente (solo Projection uploadados en currentWeek del año actual)
					SELECT 
						m.ProjectionYear,
						m.ProjectionWeekNumber,
						m.ProjectionType,
						m.UploadWeekNumber,
						m.Flow,
						SUM(m.Amount) AS TotalAmount
					FROM {TABLE_CASH_FLOW} m
					WHERE 
						m.UploadYear = @year
						AND m.ProjectionYear = @nextYear
						AND m.ProjectionType = 'Projection'
						AND m.UploadWeekNumber = @currentWeek
						AND m.ProjectionWeekNumber <= {weeksNeededNextYear}
						AND UPPER(m.Account) IN ({ACCOUNTS_OPER_INV})
						AND UPPER(m.Flow) IN ('INFLOWS', 'OUTFLOWS')
						{entityFilter}
					GROUP BY 
						m.ProjectionYear,
						m.ProjectionWeekNumber,
						m.ProjectionType,
						m.UploadWeekNumber,
						m.Flow
				)
				SELECT 
					ProjectionYear,
					ProjectionWeekNumber,
					ProjectionType,
					UploadWeekNumber,
					ISNULL(SUM(CASE WHEN UPPER(Flow) = 'INFLOWS' THEN TotalAmount ELSE 0 END), 0) AS Inflows,
					ISNULL(SUM(CASE WHEN UPPER(Flow) = 'OUTFLOWS' THEN TotalAmount ELSE 0 END), 0) AS Outflows
				FROM CashFlowData
				GROUP BY ProjectionYear, ProjectionWeekNumber, ProjectionType, UploadWeekNumber
				ORDER BY ProjectionYear, ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for Free Operating Cashflow Monthly Graph
		''' Parameters: @weekFrom, @weekTo, @uploadWeek, @year
		''' Entity filter should be appended
		''' Note: Similar to GetCashFlowProjectionQuery but orders by weekStartDate instead of ProjectionWeekNumber
		''' </summary>
		Public Shared Function GetFreeOperatingCashflowMonthlyQuery(entityFilter As String) As String
			Return $"
				WITH CashFlowData AS (
					SELECT 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Flow,
						SUM(m.Amount) AS TotalAmount
					FROM {TABLE_CASH_FLOW} m
					INNER JOIN {TABLE_AUX_DATE} d
						ON d.year = m.ProjectionYear
						AND d.weekNumber = m.ProjectionWeekNumber
						AND d.fulldate = d.weekStartDate
					WHERE 
						m.UploadYear = @year
						AND m.ProjectionYear = @year
						AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
						AND UPPER(m.Account) IN ({ACCOUNTS_OPER_INV})
						AND UPPER(m.Flow) IN ('INFLOWS', 'OUTFLOWS')
						AND ({BuildActualProjectionFilter("@uploadWeek")})
						{entityFilter}
					GROUP BY 
						m.ProjectionWeekNumber,
						d.weekStartDate,
						m.Flow
				)
				SELECT 
					ProjectionWeekNumber,
					weekStartDate,
					ISNULL(SUM(CASE WHEN UPPER(Flow) = 'INFLOWS' THEN TotalAmount ELSE 0 END), 0) AS Inflows,
					ISNULL(SUM(CASE WHEN UPPER(Flow) = 'OUTFLOWS' THEN TotalAmount ELSE 0 END), 0) AS Outflows
				FROM CashFlowData
				GROUP BY ProjectionWeekNumber, weekStartDate
				ORDER BY weekStartDate
			"
		End Function
		
		#End Region
		
		#End Region
		
		#Region "Shared"
		
		#Region "Entity Filters"
		
		''' <summary>
		''' Builds entity filter clause for SQL queries
		''' Returns empty string for HTD (all entities), otherwise returns "AND Entity = @entity"
		''' </summary>
		Public Shared Function BuildEntityFilterClause(entity As String) As String
			If String.IsNullOrEmpty(entity) OrElse entity.Equals("HTD", StringComparison.OrdinalIgnoreCase) Then
				Return ""
			End If
			Return "AND Entity = @entity"
		End Function
		
		''' <summary>
		''' Builds entity filter clause using Entity_Id column
		''' Returns empty string for HTD, otherwise returns "AND m.Entity_Id = @entityId"
		''' </summary>
		Public Shared Function BuildEntityIdFilterClause(entityId As String) As String
			If String.IsNullOrEmpty(entityId) OrElse entityId.Equals("HTD", StringComparison.OrdinalIgnoreCase) OrElse entityId = "0" Then
				Return ""
			End If
			Return "AND m.Entity_Id = @entityId"
		End Function
		
		#End Region
		
		#End Region
		
		#Region "TableViews"
		
		#Region "TableViews.CashDebt"
		
		''' <summary>
		''' Gets SQL for CashDebt StartWeek data (simple query without comparison)
		''' Used by: TRS_CashDebtPosition_CashFinanceBalance_StartWeek
		''' Parameters: timeKey, scenario (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtStartWeekSimpleQuery(timeKey As String, scenario As String, entityFilter As String) As String
			Return $"
				SELECT 
					UploadTimekey,
					Entity,
					Scenario,
					Account,
					Flow,
					Bank,
					Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey = '{timeKey}' 
				{entityFilter}
				AND Scenario = '{scenario.Replace("'", "''")}'
				AND ProjectionType = 'StartWeek'
				AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
				ORDER BY 
					Account,
					CASE 
						WHEN Flow LIKE 'INTernal%' THEN 1 
						WHEN Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					Flow,
					Bank
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt Actual data with previous week comparison
		''' Used by: TRS_CashDebtPosition_CashFinanceBalance_Global, TRS_CashDebtPosition_CashFinanceBalance_HTD
		''' Parameters: timeKey, scenario, previousTimeKey, previousScenario (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtActualWithPreviousWeekQuery(timeKey As String, scenario As String, _
		                                                               previousTimeKey As String, previousScenario As String, _
		                                                               entityFilter As String) As String
			Return $"
				SELECT 
					curr.Entity,
					curr.Account,
					curr.Flow,
					curr.Bank,
					ISNULL(curr.Amount, 0) as WeekValue,
					ISNULL(prev.Amount, 0) as PreviousWeekValue
				FROM (
					SELECT Entity, Account, Flow, Bank, Amount
					FROM {TABLE_CASH_DEBT}
					WHERE UploadTimekey = '{timeKey}'
					AND Scenario = '{scenario.Replace("'", "''")}'
					AND ProjectionType = 'StartWeek'
					AND ProjectionTimekey = '{timeKey}'
				) curr
				LEFT JOIN (
					SELECT Entity, Account, Flow, Bank, Amount
					FROM {TABLE_CASH_DEBT}
					WHERE UploadTimekey = '{previousTimeKey}'
					AND Scenario = '{previousScenario}'
					AND ProjectionType = 'StartWeek'
					AND ProjectionTimekey = '{previousTimeKey}'
				) prev
				ON curr.Entity = prev.Entity
				AND curr.Account = prev.Account
				AND ISNULL(curr.Flow, '') = ISNULL(prev.Flow, '')
				AND ISNULL(curr.Bank, '') = ISNULL(prev.Bank, '')
				ORDER BY 
					curr.Entity, 
					curr.Account,
					CASE 
						WHEN curr.Flow LIKE 'INTernal%' THEN 1 
						WHEN curr.Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					curr.Flow,
					curr.Bank
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt EndWeek data with StartWeek comparison
		''' Used by: TRS_CashDebtPosition_CashFinanceBalance_Global, TRS_CashDebtPosition_CashFinanceBalance_HTD
		''' Parameters: timeKey, scenario, forecastTimeKey (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtEndWeekWithStartWeekQuery(timeKey As String, scenario As String, _
		                                                            forecastTimeKey As String, entityFilter As String) As String
			Return $"
				SELECT 
					endweek.Entity,
					endweek.Account,
					endweek.Flow,
					endweek.Bank,
					ISNULL(endweek.Amount, 0) as WeekValue,
					ISNULL(startweek.Amount, 0) as StartWeekValue
				FROM (
					SELECT Entity, Account, Flow, Bank, Amount
					FROM {TABLE_CASH_DEBT}
					WHERE UploadTimekey = '{timeKey}'
					AND Scenario = '{scenario.Replace("'", "''")}'
					AND ProjectionType = 'EndWeek'
					AND ProjectionTimekey = '{forecastTimeKey}'
				) endweek
				LEFT JOIN (
					SELECT Entity, Account, Flow, Bank, Amount
					FROM {TABLE_CASH_DEBT}
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
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt EOM data with previous EOM comparison
		''' Used by: TRS_CashDebtPosition_CashFinanceBalance_Global, TRS_CashDebtPosition_CashFinanceBalance_HTD
		''' Parameters: timeKey, scenario, eomMonth, previousTimeKey, previousScenario (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtEOMWithPreviousQuery(timeKey As String, scenario As String, eomMonth As Integer, _
		                                                        previousTimeKey As String, previousScenario As String, _
		                                                        entityFilter As String) As String
			Return $"
				SELECT 
					curreom.Entity,
					curreom.Account,
					curreom.Flow,
					curreom.Bank,
					ISNULL(curreom.Amount, 0) as EOMValue,
					ISNULL(preveom.Amount, 0) as PreviousEOMValue
				FROM (
					SELECT Entity, Account, Flow, Bank, Amount
					FROM {TABLE_CASH_DEBT}
					WHERE UploadTimekey = '{timeKey}'
					AND Scenario = '{scenario.Replace("'", "''")}'
					AND ProjectionType = 'EOM'
					AND ProjectionMonthNumber = {eomMonth}
				) curreom
				LEFT JOIN (
					SELECT Entity, Account, Flow, Bank, Amount
					FROM {TABLE_CASH_DEBT}
					WHERE UploadTimekey = '{previousTimeKey}'
					AND Scenario = '{previousScenario}'
					AND ProjectionType = 'EOM'
					AND ProjectionMonthNumber = {eomMonth}
				) preveom
				ON curreom.Entity = preveom.Entity
				AND curreom.Account = preveom.Account
				AND ISNULL(curreom.Flow, '') = ISNULL(preveom.Flow, '')
				AND ISNULL(curreom.Bank, '') = ISNULL(preveom.Bank, '')
				ORDER BY 
					curreom.Entity,
					curreom.Account,
					CASE 
						WHEN curreom.Flow LIKE 'INTernal%' THEN 1 
						WHEN curreom.Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					curreom.Flow,
					curreom.Bank
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt multiple weeks data (for week comparison views)
		''' Used by: TRS_CashDebtPosition_Weeks, TRS_CashDebtPosition_HTD_Weeks
		''' Parameters: year, weekFrom, weekTo (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtMultipleWeeksQuery(year As String, weekFrom As Integer, weekTo As Integer, entityFilter As String) As String
			Return $"
				SELECT 
					Entity,
					Account,
					Flow,
					Bank,
					ProjectionWeekNumber,
					Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey LIKE '{year}%'
				{entityFilter}
				AND Scenario LIKE 'FW%'
				AND ProjectionType = 'StartWeek'
				AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
				AND CAST(SUBSTRING(Scenario, 3, 2) AS INT) BETWEEN {weekFrom} AND {weekTo}
				ORDER BY 
					Entity,
					Account,
					CASE 
						WHEN Flow LIKE 'INTernal%' THEN 1 
						WHEN Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					Flow,
					Bank,
					ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for EOM only data (without previous comparison)
		''' Used by: TRS_CashDebtPosition_CashFinanceBalance_EOM
		''' Parameters: timeKey, scenario, eomMonth (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtEOMSimpleQuery(timeKey As String, scenario As String, eomMonth As Integer, entityFilter As String) As String
			Return $"
				SELECT 
					Entity,
					Account,
					Flow,
					Bank,
					Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey = '{timeKey}'
				{entityFilter}
				AND Scenario = '{scenario.Replace("'", "''")}'
				AND ProjectionType = 'EOM'
				AND ProjectionMonthNumber = {eomMonth}
				AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
				ORDER BY 
					Entity,
					Account,
					CASE 
						WHEN Flow LIKE 'INTernal%' THEN 1 
						WHEN Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					Flow,
					Bank
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt EOM data for multiple weeks (by timekey list)
		''' Used by: TRS_CashDebtPosition_CashFinanceBalance_EOM
		''' Parameters: timeKeyList (comma-separated string of timekeys), entityFilter
		''' </summary>
		Public Shared Function GetCashDebtEOMMultipleWeeksQuery(timeKeyList As String, entityFilter As String) As String
			Return $"
				SELECT 
					UploadTimekey,
					Entity,
					Scenario,
					Account,
					Flow,
					Bank,
					Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey IN ('{timeKeyList}') 
				{entityFilter}
				AND ProjectionType = 'EOM'
				AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
				ORDER BY 
					Account,
					CASE 
						WHEN Flow LIKE 'INTernal%' THEN 1 
						WHEN Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					Flow,
					Bank
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt EndWeek data (simple query without comparison)
		''' Used by: TRS_CashDebtPosition_CashFinancingBalance
		''' Parameters: timeKey, scenario (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtEndWeekSimpleQuery(timeKey As String, scenario As String, entityFilter As String) As String
			Return $"
				SELECT 
					UploadTimekey,
					Entity,
					Scenario,
					Account,
					Flow,
					Bank,
					Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey = '{timeKey}' 
				{entityFilter}
				AND Scenario = '{scenario.Replace("'", "''")}'
				AND ProjectionType = 'EndWeek'
				AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
				ORDER BY 
					Account,
					CASE 
						WHEN Flow LIKE 'INTernal%' THEN 1 
						WHEN Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					Flow,
					Bank
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt EOM data by month (simple query without comparison)
		''' Used by: TRS_CashDebtPosition_CashFinancingBalance
		''' Parameters: timeKey, scenario, eomMonth (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtEOMByMonthQuery(timeKey As String, scenario As String, eomMonth As Integer, entityFilter As String) As String
			Return $"
				SELECT 
					UploadTimekey,
					Entity,
					Scenario,
					Account,
					Flow,
					Bank,
					Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey = '{timeKey}' 
				{entityFilter}
				AND Scenario = '{scenario.Replace("'", "''")}'
				AND ProjectionType = 'EOM'
				AND ProjectionMonthNumber = {eomMonth}
				AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
				ORDER BY 
					Account,
					CASE 
						WHEN Flow LIKE 'INTernal%' THEN 1 
						WHEN Flow LIKE 'EXTernal%' THEN 2 
						ELSE 3 
					END,
					Flow,
					Bank
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt weekly position grouped by Flow (for weekly comparison views)
		''' Used by: TRS_CashDebtPosition_Weeks
		''' Parameters: year, weekFrom, weekTo (embedded in query)
		''' Entity filter should be appended if entity != HTD
		''' </summary>
		Public Shared Function GetCashDebtWeeklyPositionQuery(year As String, weekFrom As Integer, weekTo As Integer, entityFilter As String) As String
			Return $"
				SELECT 
					CAST(SUBSTRING(Scenario, 3, 2) AS INT) AS WeekNumber,
					Flow,
					SUM(Amount) AS Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey LIKE '{year}%'
				{entityFilter}
				AND Scenario LIKE 'FW%'
				AND ProjectionType = 'StartWeek'
				AND (
					(Account = 'CASH AND FINANCING BALANCE' AND Flow IN ('INTernal Cash (+)', 'EXTernal Cash (+)'))
					OR
					(Account = 'FINANCING - USED LINES' AND Flow IN ('INTernal Debt (-)', 'EXTernal Debt (-)'))
				)
				AND CAST(SUBSTRING(Scenario, 3, 2) AS INT) BETWEEN {weekFrom} AND {weekTo}
				GROUP BY SUBSTRING(Scenario, 3, 2), Flow
				ORDER BY CAST(SUBSTRING(Scenario, 3, 2) AS INT), Flow
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashDebt HTD weekly data grouped by Entity and Account
		''' Used by: TRS_CashDebtPosition_HTD_Weeks
		''' Parameters: year, weekFrom, weekTo, htdEntities (comma-separated list in SQL format)
		''' </summary>
		Public Shared Function GetCashDebtHTDWeeklyQuery(year As String, weekFrom As Integer, weekTo As Integer, htdEntities As String) As String
			Return $"
				SELECT 
					CAST(SUBSTRING(Scenario, 3, 2) AS INT) AS WeekNumber,
					Entity,
					Account,
					SUM(Amount) AS Amount
				FROM {TABLE_CASH_DEBT}
				WHERE UploadTimekey LIKE '{year}%'
				AND Scenario LIKE 'FW%'
				AND ProjectionType = 'StartWeek'
				AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES')
				AND Flow IN ('INTernal Cash (+)', 'EXTernal Cash (+)', 'INTernal Debt (-)', 'EXTernal Debt (-)')
				AND CAST(SUBSTRING(Scenario, 3, 2) AS INT) BETWEEN {weekFrom} AND {weekTo}
				AND Entity IN ({htdEntities})
				GROUP BY SUBSTRING(Scenario, 3, 2), Entity, Account
				ORDER BY CAST(SUBSTRING(Scenario, 3, 2) AS INT), Entity
			"
		End Function
		
		''' <summary>
		''' Gets SQL for checking confirmation status (CashDebt, CashFlow, or both)
		''' Used by: TRS_CashDebtPositionHeader, TRS_CashFlowForecastingHeader, extender_import_templates
		''' Note: Uses parameterized query with @entity, @week, @year placeholders
		''' </summary>
		''' <param name="columns">Columns to select: "CashDebt", "CashFlow", or "CashDebt, CashFlow"</param>
		Public Shared Function GetConfirmationStatusQuery(columns As String) As String
			Return $"
				SELECT {columns}
				FROM {TABLE_AUX_WEEK_CONFIRM}
				WHERE Entity = @entity AND Week = @week AND Year = @year
			"
		End Function
		
		#End Region
		
		#Region "TableViews.CashFlow"
		
		''' <summary>
		''' Gets SQL for CashFlow data for single entity with full year display (Actual + Projection)
		''' Used by: TRS_CashFlowForecasting
		''' Parameters: yearsList (comma-separated), currentWeek, paramYear, nextYear, entityFilter
		''' CROSS-YEAR SUPPORT: When currentWeek = 53, shows all 52 weeks of paramYear as Actual
		''' Note: Actual data for W52 of year X is uploaded in W1 of year X+1
		''' </summary>
		Public Shared Function GetCashFlowEntityFullYearQuery(yearsList As String, paramYear As String, currentWeek As Integer, nextYear As Integer, entityFilter As String) As String
			' Handle week 53 case: Shows ALL 52 weeks of paramYear as Actual
			' The Actual data for W52 is uploaded when we are in W1 of the next year
			If currentWeek = 53 Then
				Return $"
					SELECT 
						Account, Flow, Scenario, UploadYear, UploadWeekNumber,
						ProjectionType, ProjectionYear, ProjectionWeekNumber, Amount
					FROM {TABLE_CASH_FLOW}
					WHERE ProjectionYear IN ({yearsList})
					{entityFilter}
					AND UPPER(Account) IN ('OPERATING FLOWS', 'INVESTMENT FLOWS', 'FINANCING FLOWS', 
					                       'CF NOT INCL. IN OP FCF (INCL. DIVIDENDS)', 'OTHER FLOWS (DIV)')
					AND (
						-- All Actual weeks W1-W52 of paramYear 
						-- Note: Actual for W(N) is uploaded in W(N+1), so W52 Actual is uploaded in W1 of next year
						(ProjectionType = 'Actual' AND ProjectionYear = {paramYear} 
							AND ProjectionWeekNumber BETWEEN 1 AND 52)
						OR
						-- Forecast weeks W1-W8 of next year (from data uploaded in W1 of next year)
						(ProjectionType = 'Projection' AND UploadYear = {nextYear} 
							AND UploadWeekNumber = 1 
							AND ProjectionYear = {nextYear} AND ProjectionWeekNumber >= 1)
					)
				"
			Else
				' Normal case: currentWeek 1-52
				Return $"
					SELECT 
						Account, Flow, Scenario, UploadYear, UploadWeekNumber,
						ProjectionType, ProjectionYear, ProjectionWeekNumber, Amount
					FROM {TABLE_CASH_FLOW}
					WHERE UploadYear IN ({yearsList})
					{entityFilter}
					AND UPPER(Account) IN ('OPERATING FLOWS', 'INVESTMENT FLOWS', 'FINANCING FLOWS', 
					                       'CF NOT INCL. IN OP FCF (INCL. DIVIDENDS)', 'OTHER FLOWS (DIV)')
					AND (
						(ProjectionType = 'Actual' AND UploadYear = {paramYear} 
							AND UploadWeekNumber = ProjectionWeekNumber + 1 
							AND ProjectionYear = {paramYear} AND ProjectionWeekNumber < {currentWeek})
						OR
						(ProjectionType = 'Projection' AND UploadYear = {paramYear} 
							AND UploadWeekNumber = {currentWeek} 
							AND ProjectionYear = {paramYear} AND ProjectionWeekNumber >= {currentWeek})
						OR
						(ProjectionType = 'Projection' AND UploadYear = {paramYear} 
							AND UploadWeekNumber = {currentWeek} 
							AND ProjectionYear = {nextYear} AND ProjectionWeekNumber >= 1)
					)
				"
			End If
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow data for single entity with 4-week display (Operating + Investment only)
		''' Used by: TRS_CashFlowForecasting_4Week
		''' Parameters: yearsList (comma-separated), entityFilter, dynamicWeekConditions
		''' Note: dynamicWeekConditions is built dynamically for each specific week
		''' </summary>
		Public Shared Function GetCashFlowEntity4WeekQuery(yearsList As String, entityFilter As String, dynamicWeekConditions As String) As String
			Return $"
				SELECT 
					Account, Flow, Scenario, UploadYear, UploadWeekNumber,
					ProjectionType, ProjectionYear, ProjectionWeekNumber, Amount
				FROM {TABLE_CASH_FLOW}
				WHERE UploadYear IN ({yearsList})
				{entityFilter}
				AND UPPER(Account) IN ('OPERATING FLOWS', 'INVESTMENT FLOWS')
				AND ({dynamicWeekConditions})
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow Actual data for all entities
		''' Used by: TRS_CashFlowForecasting_CF_CB_Global, TRS_CashFlowForecasting_CF_CB_HTD,
		'''          TRS_CashFlowForecasting_OF_IF_Global, TRS_CashFlowForecasting_OF_IF_HTD,
		'''          and their Average variants
		''' Parameters: paramYear, currentWeek, useAllAccounts (true=CF_CB, false=OF_IF)
		''' </summary>
		Public Shared Function GetCashFlowActualAllEntitiesQuery(paramYear As String, currentWeek As Integer, useAllAccounts As Boolean) As String
			Dim accountsFilter As String = GetAccountsFilter(useAllAccounts)
			Return $"
				SELECT 
					Entity, Account, Flow, Scenario, UploadTimekey,
					UploadWeekNumber, ProjectionWeekNumber, ProjectionType, Amount
				FROM {TABLE_CASH_FLOW}
				WHERE UploadYear = {paramYear}
				AND ProjectionYear = {paramYear}
				AND ProjectionType = 'Actual'
				AND UploadWeekNumber <= {currentWeek}
				AND UploadWeekNumber = ProjectionWeekNumber + 1
				AND UPPER(Account) IN ({accountsFilter})
				ORDER BY Entity, UploadWeekNumber, ProjectionWeekNumber, Account, Flow
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow Actual data grouped by Entity and Week
		''' Used by: TRS_CashFlowForecasting_OF_IF_HTD, TRS_ReportGenerator_CashFlowForecasting_HTD
		''' Parameters: paramYear, currentWeek, useAllAccounts (true=all accounts, false=OF+IF only)
		''' </summary>
		Public Shared Function GetCashFlowActualGroupedByEntityWeekQuery(paramYear As String, currentWeek As Integer, useAllAccounts As Boolean) As String
			Dim accountsFilter As String = GetAccountsFilter(useAllAccounts)
			Return $"
				SELECT 
					Entity, 
					ProjectionWeekNumber,
					SUM(Amount) AS Amount
				FROM {TABLE_CASH_FLOW}
				WHERE UploadYear = {paramYear}
					AND ProjectionYear = {paramYear}
					AND ProjectionType = 'Actual'
					AND UploadWeekNumber <= {currentWeek}
					AND UploadWeekNumber = ProjectionWeekNumber + 1
					AND UPPER(Account) IN ({accountsFilter})
				GROUP BY Entity, ProjectionWeekNumber
				ORDER BY Entity, ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow Summary data grouped by Entity, Flow and Week (for rolling 4-week calculations)
		''' Used by: TRS_CashflowForecasting_Global_Summary, TRS_CashflowForecasting_HTD_Summary
		''' Parameters: paramYear, uploadWeek (scenario week)
		''' </summary>
		Public Shared Function GetCashFlowSummaryGroupedQuery(paramYear As String, uploadWeek As Integer) As String
			Return $"
				SELECT 
					Entity, Flow, ProjectionWeekNumber, SUM(Amount) AS Amount
				FROM {TABLE_CASH_FLOW}
				WHERE UploadYear = {paramYear}
				AND ProjectionYear = {paramYear}
				AND UPPER(Account) IN ('OPERATING FLOWS', 'INVESTMENT FLOWS')
				AND UPPER(Flow) IN ('INFLOWS', 'OUTFLOWS')
				AND (
					(ProjectionType = 'Actual' AND UploadWeekNumber = ProjectionWeekNumber + 1 
						AND ProjectionWeekNumber < {uploadWeek})
					OR
					(ProjectionType = 'Projection' AND UploadWeekNumber = {uploadWeek} 
						AND ProjectionWeekNumber >= {uploadWeek})
				)
				GROUP BY Entity, Flow, ProjectionWeekNumber
				ORDER BY Entity, Flow, ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow Year Accumulated Values
		''' Used by: Multiple files as helper function for year-end calculations
		''' Parameters: targetYear, entityFilter
		''' </summary>
		Public Shared Function GetCashFlowYearAccumulatedQuery(targetYear As String, entityFilter As String) As String
			Return $"
				SELECT 
					Account, Flow, ProjectionYear, ProjectionWeekNumber, Amount
				FROM {TABLE_CASH_FLOW}
				WHERE UploadYear = {targetYear}
				{entityFilter}
				AND UPPER(Account) IN ('OPERATING FLOWS', 'INVESTMENT FLOWS', 'FINANCING FLOWS', 
				                       'CF NOT INCL. IN OP FCF (INCL. DIVIDENDS)', 'OTHER FLOWS (DIV)')
				AND ProjectionType IN ('Actual', 'Projection')
				AND ProjectionYear = {targetYear}
				AND ProjectionWeekNumber BETWEEN 1 AND 52
				ORDER BY ProjectionWeekNumber
			"
		End Function
		
		''' <summary>
		''' Gets SQL for CashFlow Year Entity data
		''' Used by: TRS_CashFlowForecasting_OF_IF_Global, TRS_CashFlowForecasting_OF_IF_HTD,
		'''          TRS_CashFlowForecasting_CF_CB_HTD, and their Average variants
		''' Parameters: targetYear, entityFilter, maxWeeks, useAllAccounts (true=CF_CB, false=OF_IF)
		''' </summary>
		Public Shared Function GetCashFlowYearEntityQuery(targetYear As String, entityFilter As String, maxWeeks As Integer, useAllAccounts As Boolean) As String
			Dim accountsFilter As String = GetAccountsFilter(useAllAccounts)
			Return $"
				SELECT Entity, Account, Flow, ProjectionYear, ProjectionWeekNumber, Amount 
				FROM {TABLE_CASH_FLOW} 
				WHERE UploadYear = {targetYear} 
				{entityFilter} 
				AND UPPER(Account) IN ({accountsFilter}) 
				AND ProjectionType IN ('Actual', 'Projection') 
				AND ProjectionYear = {targetYear} 
				AND ProjectionWeekNumber BETWEEN 1 AND {maxWeeks} 
				ORDER BY Entity, ProjectionWeekNumber
			"
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
