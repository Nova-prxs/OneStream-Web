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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_restfast_connector_load_test
	''' <summary>
	''' TEST VERSION - Single Batch Per Execution
	''' ==========================================
	''' This version processes ONLY ONE BATCH per Task Schedule execution.
	''' Use this to test real concurrency scenarios where:
	''' - Task Schedule runs every 5 minutes
	''' - Each execution processes one TimeGap window
	''' - ReferenceDate is updated after each batch
	''' - Next scheduler picks up from updated ReferenceDate
	''' 
	''' FLOW:
	''' Scheduler 5min → Batch 1 (from→from+TimeGap) → Update ReferenceDate → EXIT
	''' Scheduler 5min → Batch 2 (from→from+TimeGap) → Update ReferenceDate → EXIT
	''' ...until ReferenceDate catches up to Now
	''' 
	''' This allows testing:
	''' - HasRunningProcess concurrency check
	''' - Multiple LoadIds running in parallel
	''' - Stale entry cleanup behavior
	''' 
	''' CONFIGURATION:
	''' Uses same XFC_STG_ETL_LOAD_CONTROL table but with test tables:
	''' - XFC_STG_FIN_MIX_Test / XFC_LANDING_FIN_MIX_Test
	''' - Different LoadIds for isolation
	''' </summary>
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		'Reference LOG table class
		Dim LogTable As New Workspace.__WsNamespacePrefix.__WsAssemblyName.STG_ETL_LOAD_LOG()
		
	Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		Try
			Select Case args.FunctionType
				
				Case Is = ExtenderFunctionType.Unknown
					
				Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
					
				#Region "Execute RestFast Requests - SINGLE BATCH MODE"
				
				'Get function parameter to determine which function to execute
				Dim functionName As String = args.NameValuePairs.XFGetValue("p_function")
				
				If functionName.XFEqualsIgnoreCase("ExecuteRestFastRequest") Then
					'Execute all enabled RestFast requests - ONE BATCH PER ENDPOINT
					Me.ExecuteAllRestFastRequestsSingleBatch(si)
					Return Nothing
				End If
				
				'Optional: Execute specific endpoint only (for isolated testing)
				If functionName.XFEqualsIgnoreCase("ExecuteSingleEndpoint") Then
					Dim endpointName As String = args.NameValuePairs.XFGetValue("p_endpoint")
					If Not String.IsNullOrEmpty(endpointName) Then
						Me.ExecuteSingleBatch(si, endpointName)
					End If
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
		
	#Region "Execute All RestFast Requests - Single Batch Mode"
	
	''' <summary>
	''' Processes ONE BATCH for each enabled endpoint, then exits.
	''' Next scheduler execution will continue from updated ReferenceDate.
	''' </summary>
	Private Sub ExecuteAllRestFastRequestsSingleBatch(ByVal si As SessionInfo)
		Try
			'Get all enabled endpoints from control table
			Dim dtEndpoints As DataTable = HelperFunctions.GetAllEnabledEtlEndpoints(si)
			
			If dtEndpoints Is Nothing OrElse dtEndpoints.Rows.Count = 0 Then
				BRApi.ErrorLog.LogMessage(si, "[TEST] No enabled endpoints found in XFC_STG_ETL_LOAD_CONTROL")
				Return
			End If
			
			BRApi.ErrorLog.LogMessage(si, $"[TEST] Starting single-batch execution for {dtEndpoints.Rows.Count} endpoint(s)")
			
			'Loop through each enabled endpoint - process ONE BATCH each
			For Each row As DataRow In dtEndpoints.Rows
				Dim endpointName As String = row("EndPoint").ToString()
				Try
					Me.ExecuteSingleBatch(si, endpointName)
				Catch ex As Exception
					BRApi.ErrorLog.LogMessage(si, $"[TEST][ERROR] Failed to process endpoint '{endpointName}': {ex.Message}")
					'Continue with next endpoint instead of stopping
				End Try
			Next
			
			BRApi.ErrorLog.LogMessage(si, "[TEST] Single-batch execution completed for all endpoints")
			
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Sub
	
	#End Region
	
	#Region "Execute Single Batch"
	
	''' <summary>
	''' Executes ONLY ONE BATCH for the specified endpoint, then returns.
	''' Key difference from production version:
	''' - No While loop - processes exactly one TimeGap window
	''' - Updates ReferenceDate after successful batch
	''' - Next scheduler execution continues from new ReferenceDate
	''' </summary>
	Private Sub ExecuteSingleBatch(
		ByVal si As SessionInfo,
		ByVal requestName As String
	)
		Try
			'Get control table configuration
			Dim config As HelperFunctions.EtlLoadConfig = HelperFunctions.GetEtlLoadConfig(si, requestName)
			If config Is Nothing Then
				BRApi.ErrorLog.LogMessage(si, $"[TEST] No enabled record found for EndPoint: {requestName}")
				Return
			End If
			
			'Validate TargetTable (StgTable)
			If String.IsNullOrEmpty(config.TargetTable) Then
				Throw New XFException(si, New Exception($"[TEST] TargetTable is empty for EndPoint: {requestName}"))
			End If
			
			'Derive LandingTable from StgTable
			Dim landingTable As String = HelperFunctions.GetLandingTableFromStg(config.TargetTable)
			
			'=== CONCURRENCY CHECK ===
			'This is the key test point - check if another instance is RUNNING
			If LogTable.HasRunningProcess(si, config.LoadId) Then
				BRApi.ErrorLog.LogMessage(si, $"[TEST][SKIP] LoadId={config.LoadId} ({requestName}) - Another instance is RUNNING. Skipping this execution.")
				Return
			End If
			
			'Validate ReferenceDate
			If String.IsNullOrEmpty(config.ReferenceDate) Then
				Throw New XFException(si, New Exception($"[TEST] ReferenceDate is empty for EndPoint: {requestName}"))
			End If
			
			'Parse dates
			Dim fromDate As DateTime? = HelperFunctions.ParseTimestamp(config.ReferenceDate)
			If Not fromDate.HasValue Then
				Throw New XFException(si, New Exception($"[TEST] Invalid ReferenceDate format: {config.ReferenceDate}"))
			End If
			
			'Determine Target Date (EndDate or Now)
			Dim targetDate As DateTime
			Dim cleanEndDate As String = config.EndDate.Trim()
			If cleanEndDate = "0" OrElse String.IsNullOrEmpty(cleanEndDate) Then
				targetDate = DateTime.Now
			Else
				Dim parsedEndDate As DateTime? = HelperFunctions.ParseTimestamp(cleanEndDate)
				If Not parsedEndDate.HasValue Then
					Throw New XFException(si, New Exception($"[TEST] Invalid EndDate format: {cleanEndDate}"))
				End If
				targetDate = parsedEndDate.Value
			End If
			
			'=== CHECK IF ALREADY CAUGHT UP ===
			If fromDate.Value >= targetDate Then
				BRApi.ErrorLog.LogMessage(si, $"[TEST][CAUGHT_UP] LoadId={config.LoadId} ({requestName}) - ReferenceDate ({config.ReferenceDate}) >= TargetDate. Nothing to process.")
				Return
			End If
			
			'=== CALCULATE SINGLE BATCH WINDOW ===
			Dim currentToDate As DateTime
			If config.TimeGap > 0 Then
				currentToDate = fromDate.Value.AddMinutes(config.TimeGap)
				'Cap at targetDate
				If currentToDate > targetDate Then currentToDate = targetDate
			Else
				'Single pass mode (TimeGap=0)
				currentToDate = targetDate
			End If
			
			Dim fromString As String = HelperFunctions.DateTimeToTimestamp(fromDate.Value)
			Dim toString As String = HelperFunctions.DateTimeToTimestamp(currentToDate)
			
			BRApi.ErrorLog.LogMessage(si, $"[TEST][START] LoadId={config.LoadId} ({requestName}) - Processing batch: {fromString} → {toString}")
			
			Dim executionStart As DateTime = DateTime.Now
			
			'=== LOG START (RUNNING status) ===
			LogTable.InsertLogEntry(si, config.LoadId, config.LoadType, config.TargetTable, executionStart, fromString)
			
			Try
				'=== 1. EXTRACTION (RestFast -> STG) ===
				Dim recordsLoaded As Integer = Me.ExecuteRequestWithTimeWindow(si, requestName, config.TargetTable, fromString, toString)
				
				'Log SUCCESS for STG
				Dim stgExecutionEnd As DateTime = DateTime.Now
				Dim stgDuration As Double = (stgExecutionEnd - executionStart).TotalSeconds
				LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, config.TargetTable, stgExecutionEnd, recordsLoaded, "OK", "", stgDuration)
				
				BRApi.ErrorLog.LogMessage(si, $"[TEST][STG] LoadId={config.LoadId} - Extracted {recordsLoaded} records in {stgDuration:F1}s")
				
				'=== 2. TRANSFER (STG -> LANDING) ===
				Dim landingExecutionStart As DateTime = DateTime.Now
				LogTable.InsertLogEntry(si, config.LoadId, config.LoadType, landingTable, landingExecutionStart, fromString)
				
				Dim recordsInserted As Integer = Me.TransferToLandingTable(si, config.TargetTable, landingTable, fromString, toString, config.ReferenceColumn)
				
				'Log SUCCESS for LANDING
				Dim landingExecutionEnd As DateTime = DateTime.Now
				Dim landingDuration As Double = (landingExecutionEnd - landingExecutionStart).TotalSeconds
				LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, landingTable, landingExecutionEnd, recordsInserted, "OK", "", landingDuration)
				
				BRApi.ErrorLog.LogMessage(si, $"[TEST][LANDING] LoadId={config.LoadId} - Inserted {recordsInserted} records in {landingDuration:F1}s")
				
				'=== 3. UPDATE REFERENCE DATE (with Overlap) ===
				Dim overlapMinutes As Double = config.TimeGap * (config.OverlapPercentage / 100D)
				Dim checkpointDate As DateTime = currentToDate.AddMinutes(-overlapMinutes)
				Dim checkpointString As String = HelperFunctions.DateTimeToTimestamp(checkpointDate)
				HelperFunctions.UpdateControlTableReferenceDate(si, config.LoadId, checkpointString)
				
				Dim totalDuration As Double = (DateTime.Now - executionStart).TotalSeconds
				BRApi.ErrorLog.LogMessage(si, $"[TEST][COMPLETE] LoadId={config.LoadId} ({requestName}) - Batch complete. New ReferenceDate: {checkpointString}. Total: {totalDuration:F1}s")
				
			Catch ex As Exception
				'=== LOG ERROR ===
				Dim executionEnd As DateTime = DateTime.Now
				Dim duration As Double = (executionEnd - executionStart).TotalSeconds
				Dim errorMsg As String = $"[TEST] Error processing batch {fromString} to {toString}: {ex.Message}"
				BRApi.ErrorLog.LogMessage(si, errorMsg)
				
				'Update STG entry if still RUNNING
				LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, config.TargetTable, executionEnd, 0, "ERROR", errorMsg, duration)
				'Update LANDING entry if exists
				LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, landingTable, executionEnd, 0, "ERROR", errorMsg, duration)
				
				Throw New XFException(si, New Exception(errorMsg, ex))
			End Try
			
			'=== EXIT AFTER SINGLE BATCH ===
			'No While loop - next scheduler execution will continue from updated ReferenceDate
			
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
			'Truncate STG table
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				BRApi.Database.ExecuteSql(dbConn, $"TRUNCATE TABLE {stgTable}", False)
			End Using
			
			'Get request
			Dim utils As New Workspace.RESTAPIFramework.RESTAPIRequestManager.Utils()
			Dim request As RESTAPIRequest = utils.GetSavedRequest(si, requestName)
			
			'Set variables
			Dim customVars As New Dictionary(Of String, String)()
			customVars("from") = fromDate
			customVars("to") = toDate
			
			'Execute
			request.PopulateTable(si, customVars)
			
			'Count records
			Dim recordCount As Integer = 0
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim countQuery As String = $"SELECT COUNT(*) as RecordCount FROM {stgTable}"
				Dim dtCount As DataTable = BRApi.Database.GetDataTable(dbConn, countQuery, Nothing, Nothing, False)
				If dtCount.Rows.Count > 0 Then
					recordCount = CInt(dtCount.Rows(0)("RecordCount"))
				End If
			End Using
			
			Return recordCount
			
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Function
	
	#End Region
	
	#Region "Transfer To Landing Table"
	
	''' <summary>
	''' Transfers data from STG table to Landing table using DELETE + INSERT pattern
	''' </summary>
	Private Function TransferToLandingTable(
		ByVal si As SessionInfo,
		ByVal stgTable As String,
		ByVal landingTable As String,
		ByVal fromDate As String,
		ByVal toDate As String,
		ByVal referenceColumn As String
	) As Integer
		Try
			Dim fromTimestamp As Long = 0
			Dim toTimestamp As Long = 0
			
			If Not String.IsNullOrEmpty(fromDate) Then fromTimestamp = CLng(fromDate)
			If Not String.IsNullOrEmpty(toDate) Then toTimestamp = CLng(toDate)
			
			Dim recordsInserted As Integer = 0
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
				'Delete records in the time range (allows reprocessing)
				Dim deleteQuery As String = $"DELETE FROM {landingTable} WHERE [{referenceColumn}] >= {fromTimestamp} AND [{referenceColumn}] <= {toTimestamp}"
				BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
				
				'Insert all records from STG to Landing
				Dim insertQuery As String = $"INSERT INTO {landingTable} SELECT * FROM {stgTable}"
				BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
				
				'Count records inserted
				Dim countQuery As String = $"SELECT COUNT(*) as RecordCount FROM {landingTable} WHERE [{referenceColumn}] >= {fromTimestamp} AND [{referenceColumn}] <= {toTimestamp}"
				Dim dtCount As DataTable = BRApi.Database.GetDataTable(dbConn, countQuery, Nothing, Nothing, False)
				If dtCount.Rows.Count > 0 Then
					recordsInserted = CInt(dtCount.Rows(0)("RecordCount"))
				End If
				
			End Using
			
			Return recordsInserted
			
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Function
	
	#End Region
	
	#End Region
		
	End Class
End Namespace
