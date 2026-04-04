Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Net
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports Workspace.RESTAPIFramework.RESTAPIRequestManager.Classes
Imports Newtonsoft.Json.Linq

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_incremental_loader
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
	Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		Try
			Select Case args.FunctionType
				
				Case Is = ExtenderFunctionType.Unknown
					
				Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
					
				#Region "Execute Incremental Load with Gaps"
				
				'Get function parameter to determine which function to execute
				Dim functionName As String = args.NameValuePairs.XFGetValue("p_function")
				
				If functionName.XFEqualsIgnoreCase("ExecuteIncrementalLoad") Then
					'Execute incremental load with time gaps
					Dim requestName As String = args.NameValuePairs.XFGetValue("p_requestName")
					Dim loadTo As String = args.NameValuePairs.XFGetValue("p_LoadTo", "")
					
					Me.ExecuteIncrementalLoadWithGaps(si, requestName, loadTo)
					
					Return Nothing
				End If
				
				#End Region
				
				Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
					
			End Select
			
			Return Nothing
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try
	End Function
	
    #Region "Helper Functions"
		
	#Region "Execute Incremental Load With Gaps"
	
	Private Sub ExecuteIncrementalLoadWithGaps(
		ByVal si As SessionInfo,
		ByVal requestName As String,
		ByVal loadTo As String
	)
		Try
			
			'Fixed tables: RestFast always writes to STG, we always transfer to LANDING
			Dim stgTable As String = "XFC_STG_FIN_MIX"
			Dim landingTable As String = "XFC_LANDING_FIN_MIX"
			
			'Get control table configuration
			Dim loadId As Integer = 0
			Dim targetTable As String = String.Empty  'Only for logging purposes
			Dim referenceDate As String = String.Empty
			Dim loadType As String = String.Empty
			Dim timeGap As Integer = 0  'Gap in minutes
			Dim referenceColumn As String = "timestamp" 'Default
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim query As String = $"SELECT LoadId, TargetTable, ReferenceDate, LoadType, TimeGap, ReferenceColumn FROM XFC_STG_ETL_LOAD_CONTROL WHERE EndPoint = '{requestName}' AND Enable = 1"
				
				Dim dtControl As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
				
				If dtControl.Rows.Count = 0 Then
					Throw New XFException(si, New Exception($"No enabled record found in XFC_STG_ETL_LOAD_CONTROL for EndPoint: {requestName}"))
				End If
				
				Dim row As DataRow = dtControl.Rows(0)
				loadId = CInt(row("LoadId"))
				targetTable = If(IsDBNull(row("TargetTable")), stgTable, row("TargetTable").ToString())  'For logging only
				referenceDate = If(IsDBNull(row("ReferenceDate")), "", row("ReferenceDate").ToString())
				loadType = If(IsDBNull(row("LoadType")), "", row("LoadType").ToString())
				timeGap = CInt(row("TimeGap"))  'Gap in minutes from control table
				referenceColumn = If(IsDBNull(row("ReferenceColumn")) OrElse String.IsNullOrEmpty(row("ReferenceColumn").ToString()), "timestamp", row("ReferenceColumn").ToString())
			End Using
			
			'Check if there is already a running process for this LoadId
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim checkQuery As String = $"SELECT COUNT(*) as RunningCount FROM XFC_STG_ETL_LOAD_LOG WHERE LoadId = {loadId} AND LoadStatus = 'RUNNING'"
				Dim dtCheck As DataTable = BRApi.Database.GetDataTable(dbConn, checkQuery, Nothing, Nothing, False)
				
				If dtCheck.Rows.Count > 0 AndAlso CInt(dtCheck.Rows(0)("RunningCount")) > 0 Then
					BRApi.ErrorLog.LogMessage(si, $"[WARNING] Incremental Load skipped for {requestName}. A previous instance is still RUNNING.")
					Return
				End If
			End Using
			
			'If no reference date, cannot proceed with incremental load
			If String.IsNullOrEmpty(referenceDate) Then
				Throw New XFException(si, New Exception("ReferenceDate is empty. Cannot execute incremental load with gaps."))
			End If
			
			
			'Determine LoadTo limit
			Dim loadToString As String
			If String.IsNullOrEmpty(loadTo) Then
				'If no LoadTo provided, use current date/time
				Dim now As DateTime = DateTime.Now
				loadToString = now.ToString("yyyyMMddHHmm") & "00"  'Add seconds as 00
			Else
				'Validate and format LoadTo (should be yyyyMMddHHmm)
				If loadTo.Length = 12 Then
					loadToString = loadTo & "00"  'Add seconds
				Else
					loadToString = loadTo  'Assume it's already in yyyyMMddHHmmss format
				End If
			End If
			
			'Validate TimeGap
			If timeGap <= 0 Then
				Throw New XFException(si, New Exception($"TimeGap must be greater than 0. Current value: {timeGap}"))
			End If
			
			'Execute load in chunks until we reach LoadTo
			Dim currentFrom As String = referenceDate
			Dim iterationCount As Integer = 0
			Dim totalRecords As Integer = 0
			Dim hasMoreData As Boolean = True
			
		
		While hasMoreData
			iterationCount += 1
			Dim executionStart As DateTime = DateTime.Now
			Dim currentStepStart As DateTime = executionStart
		Dim fromDateTime As DateTime
		Dim toDateTime As DateTime
		Dim toString As String = ""
		Dim currentIterationFrom As String = currentFrom
		
			
			Try
				'Parse current FROM date to calculate TO
				If Not DateTime.TryParseExact(currentFrom, "yyyyMMddHHmmss", Nothing, DateTimeStyles.None, fromDateTime) Then
					Throw New XFException(si, New Exception($"Invalid date format: {currentFrom}"))
				End If
				
				'Calculate TO date (FROM + TimeGap minutes)
				toDateTime = fromDateTime.AddMinutes(timeGap)
				toString = toDateTime.ToString("yyyyMMddHHmmss")
				
				
				'Check if we've reached or passed LoadTo - if so, set TO to LoadTo and this will be the last iteration
				If toString >= loadToString Then
					toString = loadToString
					'IMPORTANT: Update toDateTime to the actual cut-off time so the next ReferenceDate is calculated correctly
					DateTime.TryParseExact(toString, "yyyyMMddHHmmss", Nothing, DateTimeStyles.None, toDateTime)
				End If
				
				'Log START execution (Extraction)
				Me.LogExecution(si, requestName, stgTable, loadType, executionStart, DateTime.MinValue, 0, "RUNNING", "", currentIterationFrom, 0)

				'Execute request with current time window (RestFast writes to STG automatically)
				Dim recordsLoaded As Integer = Me.ExecuteRequestWithTimeWindow(si, requestName, stgTable, currentFrom, toString)
				
				totalRecords += recordsLoaded
				
				'Log successful execution (Extraction)
				Dim executionEndExtraction As DateTime = DateTime.Now
				Dim durationExtraction As Double = (executionEndExtraction - executionStart).TotalSeconds
				Me.LogExecution(si, requestName, stgTable, loadType, executionStart, executionEndExtraction, recordsLoaded, "OK", "", currentIterationFrom, durationExtraction)

				'--- TRANSFER STEP ---
				Dim executionStartTransfer As DateTime = DateTime.Now
				currentStepStart = executionStartTransfer
				
				'Log START execution (Transfer)
				Me.LogExecution(si, requestName, landingTable, loadType, executionStartTransfer, DateTime.MinValue, 0, "RUNNING", "", currentIterationFrom, 0)

				'Always transfer data from STG to LANDING table (even if 0 records)
				Me.TransferToLandingTable(si, stgTable, currentFrom, referenceColumn)
				
				'Log successful execution (Transfer)
				Dim executionEndTransfer As DateTime = DateTime.Now
				Dim durationTransfer As Double = (executionEndTransfer - executionStartTransfer).TotalSeconds
				Me.LogExecution(si, requestName, landingTable, loadType, executionStartTransfer, executionEndTransfer, recordsLoaded, "OK", "", currentIterationFrom, durationTransfer)
				
			Catch chunkEx As Exception
				
				Dim errorMessage As String = chunkEx.Message
				If chunkEx.InnerException IsNot Nothing Then
					errorMessage &= " | Inner: " & chunkEx.InnerException.Message
				End If
				
				'Log error execution (use current FROM before any updates)
				Dim executionEndError As DateTime = DateTime.Now
				Dim durationError As Double = (executionEndError - currentStepStart).TotalSeconds
				Me.LogExecution(si, requestName, stgTable, loadType, currentStepStart, executionEndError, 0, "ERROR", errorMessage, currentFrom, durationError)
				
				'Do not throw - continue to next iteration even on error
			End Try
			
			'CRITICAL: Always calculate next FROM and update hasMoreData flag OUTSIDE Try-Catch
			'This ensures the loop continues even if there was an error in the iteration
			
			
			'Check if we've reached or passed LoadTo
			If toString >= loadToString OrElse String.IsNullOrEmpty(toString) Then
				hasMoreData = False
			Else
				'Calculate next FROM: TO - 1 minute (to have 1 minute overlap between windows)
				Dim nextFromDateTime As DateTime = toDateTime.AddMinutes(-1)
				currentFrom = nextFromDateTime.ToString("yyyyMMddHHmmss")
				
				'Update ReferenceDate to new FROM (for next execution or in case of interruption)
				If loadId > 0 Then
					Try
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							Dim updateQuery As String = $"UPDATE XFC_STG_ETL_LOAD_CONTROL SET ReferenceDate = '{currentFrom}' WHERE LoadId = {loadId}"
							BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
						End Using
					Catch updateEx As Exception
						'Log but don't stop execution
					End Try
				End If
			End If
			
			'Safety check: prevent infinite loops (max 1000 iterations)
			If iterationCount >= 1000 Then
				hasMoreData = False
			End If
		End While
		
		
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Sub
	
	#End Region
	
	#Region "Execute Request With Time Window"
	
	Private Function ExecuteRequestWithTimeWindow(
		ByVal si As SessionInfo,
		ByVal requestName As String,
		ByVal stgTable As String,
		ByVal fromDate As String,
		ByVal toDate As String
	) As Integer
		Try
			'Truncate XFC_STG_FIN_MIX table before loading new data
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteSql(dbConn, "TRUNCATE TABLE XFC_STG_FIN_MIX", False)
			End Using
			
			'Get the saved RestFast request
			Dim utils As New Workspace.RESTAPIFramework.RESTAPIRequestManager.Utils()
			Dim request As RESTAPIRequest = utils.GetSavedRequest(si, requestName)
			
			'Get record count BEFORE loading (RestFast writes to STG automatically)
			Dim recordCountBefore As Integer = 0
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim countQueryBefore As String = $"SELECT COUNT(*) as RecordCount FROM {stgTable}"
				Dim dtCountBefore As DataTable = BRApi.Database.GetDataTable(dbConn, countQueryBefore, Nothing, Nothing, False)
				If dtCountBefore.Rows.Count > 0 Then
					recordCountBefore = CInt(dtCountBefore.Rows(0)("RecordCount"))
				End If
			End Using
			
			'Build variables dictionary with FROM and TO
			Dim customVars As New Dictionary(Of String, String)()
			customVars("from") = fromDate
			customVars("to") = toDate
			
			
			'Execute PopulateTable with variables
			request.PopulateTable(si, customVars)
			
			'Get record count AFTER loading
			Dim recordCountAfter As Integer = 0
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim countQuery As String = $"SELECT COUNT(*) as RecordCount FROM {stgTable}"
				Dim dtCount As DataTable = BRApi.Database.GetDataTable(dbConn, countQuery, Nothing, Nothing, False)
				If dtCount.Rows.Count > 0 Then
					recordCountAfter = CInt(dtCount.Rows(0)("RecordCount"))
				End If
			End Using
			
			'Calculate new records loaded (RestFast uses "replace" mode, so all records are new)
			Dim newRecords As Integer = recordCountAfter
			
			Return newRecords
			
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Function
	
	#End Region
	
	#Region "Log Execution"
		
		Private Sub LogExecution(
			ByVal si As SessionInfo,
			ByVal requestName As String,
			ByVal targetTable As String,
			ByVal loadType As String,
			ByVal executionStart As DateTime,
			ByVal executionEnd As DateTime,
			ByVal recordCount As Integer,
			ByVal loadStatus As String,
			ByVal errorMessage As String,
			ByVal referenceDate As String,
			Optional ByVal executionDuration As Double = 0
		)
			Try
				'Get LoadId from control table
				Dim loadId As Integer = 0
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"SELECT LoadId FROM XFC_STG_ETL_LOAD_CONTROL WHERE EndPoint = '{requestName}'"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					If dt.Rows.Count > 0 Then
						loadId = CInt(dt.Rows(0)("LoadId"))
					End If
				End Using
				
				If loadStatus.XFEqualsIgnoreCase("RUNNING") Then
					'INSERT new log entry
					Dim logDt As New DataTable()
					logDt.Columns.Add("LoadId", GetType(Integer))
					logDt.Columns.Add("LoadType", GetType(String))
					logDt.Columns.Add("TargetTable", GetType(String))
					logDt.Columns.Add("ExecutionStart", GetType(DateTime))
					logDt.Columns.Add("ExecutionEnd", GetType(DateTime))
					logDt.Columns.Add("Records", GetType(Integer))
					logDt.Columns.Add("LoadStatus", GetType(String))
					logDt.Columns.Add("ErrorMessage", GetType(String))
					logDt.Columns.Add("ReferenceDate", GetType(String))
					logDt.Columns.Add("ExecutedBy", GetType(String))
					logDt.Columns.Add("ExecutionDuration", GetType(Double))
					
					Dim logRow As DataRow = logDt.NewRow()
					logRow("LoadId") = loadId
					logRow("LoadType") = If(String.IsNullOrEmpty(loadType), DBNull.Value, loadType)
					logRow("TargetTable") = If(String.IsNullOrEmpty(targetTable), DBNull.Value, targetTable)
					logRow("ExecutionStart") = executionStart
					logRow("ExecutionEnd") = executionStart
					logRow("Records") = 0
					logRow("LoadStatus") = loadStatus
					logRow("ErrorMessage") = DBNull.Value
					logRow("ReferenceDate") = If(String.IsNullOrEmpty(referenceDate), DBNull.Value, referenceDate)
					logRow("ExecutedBy") = si.UserName
					logRow("ExecutionDuration") = 0
					logDt.Rows.Add(logRow)
					
					'Insert log entry
					UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_STG_ETL_LOAD_LOG", logDt, "Append")
					
				Else
					'UPDATE existing log entry (OK or ERROR)
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim safeErrorMessage As String = If(String.IsNullOrEmpty(errorMessage), "", errorMessage.Replace("'", "''"))
						Dim safeLoadStatus As String = loadStatus
						Dim safeExecutionEnd As String = executionEnd.ToString("yyyy-MM-dd HH:mm:ss.fff")
						'Format duration with dot as decimal separator for SQL
					Dim safeDuration As String = executionDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)
						
						Dim updateQuery As String = $"UPDATE XFC_STG_ETL_LOAD_LOG 
													 SET LoadStatus = '{safeLoadStatus}', 
														 ExecutionEnd = '{safeExecutionEnd}', 
														 Records = {recordCount}, 
														 ErrorMessage = '{safeErrorMessage}',
														 ExecutionDuration = {safeDuration}
													 WHERE LoadId = {loadId} 
													   AND ReferenceDate = '{referenceDate}' 
													   AND LoadStatus = 'RUNNING'"
						
						BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
					End Using
				End If
				
			Catch ex As Exception
				'Don't throw exception here, just log the error
			End Try
		End Sub
		
	#End Region
	
	#Region "Transfer To Landing Table"
		
		Private Sub TransferToLandingTable(
			ByVal si As SessionInfo,
			ByVal stgTable As String,
			ByVal referenceDate As String,
			ByVal referenceColumn As String
		)
			Try
				Dim landingTable As String = "XFC_LANDING_FIN_MIX"
				
				
				'Convert referenceDate from yyyyMMddHHmmss to BIGINT timestamp format
				'The timestamp in the table is BIGINT, so we need to convert the reference date
				Dim timestampThreshold As Long = 0
				If Not String.IsNullOrEmpty(referenceDate) Then
					timestampThreshold = CLng(referenceDate)
				Else
				End If
				
				Dim finalCount As Integer = 0
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					'Step 1: Delete records from LANDING where referenceColumn >= referenceDate
					Dim deleteQuery As String
					If timestampThreshold > 0 Then
						deleteQuery = $"DELETE FROM {landingTable} WHERE [{referenceColumn}] >= {timestampThreshold}"
					Else
						deleteQuery = $"TRUNCATE TABLE {landingTable}"
					End If
					
					BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
					
					'Step 2: Insert ALL records from STG to LANDING
					Dim insertQuery As String = $"
						INSERT INTO {landingTable} (
							[companyCode], [referenceTransaction], [documentType], [postingDate],
							[documentNumber], [tradingPartner], [customerNumber], [vendorNumber],
							[itemText], [profitCenter], [costCenter], [wbsElement],
							[amountInLocalCurrency], [postingKey], [transactionType], [lineItemNumber],
							[ledger], [amountInTransactionCurrency], [transactionCurrency], [createdBy],
							[glAccountLineItem], [timestamp], [wbsUserField], [projectProfile],
							[balanceSheetAccountGroup], [customerAccountGroup], [vendorAccountGroup]
						)
						SELECT 
							[companyCode], [referenceTransaction], [documentType], [postingDate],
							[documentNumber], [tradingPartner], [customerNumber], [vendorNumber],
							[itemText], [profitCenter], [costCenter], [wbsElement],
							[amountInLocalCurrency], [postingKey], [transactionType], [lineItemNumber],
							[ledger], [amountInTransactionCurrency], [transactionCurrency], [createdBy],
							[glAccountLineItem], [timestamp], [wbsUserField], [projectProfile],
							[balanceSheetAccountGroup], [customerAccountGroup], [vendorAccountGroup]
						FROM {stgTable}
					"
					
					BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
					
					'Step 3: Get final count in LANDING table
					Dim countQuery As String = $"SELECT COUNT(*) as RecordCount FROM {landingTable}"
					Dim dtCount As DataTable = BRApi.Database.GetDataTable(dbConn, countQuery, Nothing, Nothing, False)
					If dtCount.Rows.Count > 0 Then
						finalCount = CInt(dtCount.Rows(0)("RecordCount"))
					End If
					
					
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
	#End Region
	
	#End Region
		
	End Class
End Namespace
