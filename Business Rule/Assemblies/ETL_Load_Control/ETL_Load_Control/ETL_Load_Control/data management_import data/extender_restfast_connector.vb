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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_restfast_connector
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
					
				#Region "Execute RestFast Requests"
				
				'Get function parameter to determine which function to execute
				Dim functionName As String = args.NameValuePairs.XFGetValue("p_function")
				
				If functionName.XFEqualsIgnoreCase("ExecuteRestFastRequest") Then
					'Execute all enabled RestFast requests from control table
					Me.ExecuteAllRestFastRequests(si)
					
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
		
	#Region "Execute All RestFast Requests"
	
	Private Sub ExecuteAllRestFastRequests(ByVal si As SessionInfo)
		Try
			'Get all enabled endpoints from control table using HelperFunctions
			Dim dtEndpoints As DataTable = HelperFunctions.GetAllEnabledEtlEndpoints(si)
			
			If dtEndpoints Is Nothing OrElse dtEndpoints.Rows.Count = 0 Then
				BRApi.ErrorLog.LogMessage(si, "[WARNING] No enabled endpoints found in XFC_STG_ETL_LOAD_CONTROL")
				Return
			End If
			
			'Loop through each enabled endpoint
			For Each row As DataRow In dtEndpoints.Rows
				Dim endpointName As String = row("EndPoint").ToString()
				Try
					Me.ExecuteSingleRestFastRequest(si, endpointName)
				Catch ex As Exception
					BRApi.ErrorLog.LogMessage(si, $"[ERROR] Failed to process endpoint '{endpointName}': {ex.Message}")
				End Try
			Next
			
		Catch ex As Exception
			Throw New XFException(si, ex)
		End Try
	End Sub
	
	#End Region
	
	#Region "Execute Single RestFast Request"
	
	Private Sub ExecuteSingleRestFastRequest(
		ByVal si As SessionInfo,
		ByVal requestName As String
	)
		Try
			'Get control table configuration using HelperFunctions
			Dim config As HelperFunctions.EtlLoadConfig = HelperFunctions.GetEtlLoadConfig(si, requestName)
			If config Is Nothing Then
				Throw New XFException(si, New Exception($"No enabled record found in XFC_STG_ETL_LOAD_CONTROL for EndPoint: {requestName}"))
			End If
			
			'Validate TargetTable (StgTable)
			If String.IsNullOrEmpty(config.TargetTable) Then
				Throw New XFException(si, New Exception($"TargetTable is empty in XFC_STG_ETL_LOAD_CONTROL for EndPoint: {requestName}"))
			End If
			
			'Derive LandingTable from StgTable using HelperFunctions
			Dim landingTable As String = HelperFunctions.GetLandingTableFromStg(config.TargetTable)
			
			'Check for running process using LogTable class
			If LogTable.HasRunningProcess(si, config.LoadId) Then
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Load skipped for {requestName}. A previous instance is still RUNNING.")
				Return
			End If
			
			If String.IsNullOrEmpty(config.ReferenceDate) Then
				Throw New XFException(si, New Exception("ReferenceDate is empty. Cannot execute incremental load."))
			End If
			
			'Determine Start Date using HelperFunctions
			Dim currentFromDate As DateTime? = HelperFunctions.ParseTimestamp(config.ReferenceDate)
			If Not currentFromDate.HasValue Then
				Throw New XFException(si, New Exception($"Invalid ReferenceDate format: {config.ReferenceDate}"))
			End If
			
			'Determine Target Date
			Dim targetDate As DateTime
			Dim cleanEndDate As String = config.EndDate.Trim()
			If cleanEndDate = "0" OrElse String.IsNullOrEmpty(cleanEndDate) Then
				targetDate = DateTime.Now
			Else
				Dim parsedEndDate As DateTime? = HelperFunctions.ParseTimestamp(cleanEndDate)
				If Not parsedEndDate.HasValue Then
					Throw New XFException(si, New Exception($"Invalid EndDate format: {cleanEndDate}"))
				End If
				targetDate = parsedEndDate.Value
			End If
			
			'Loop Execution
			Dim isFinished As Boolean = False
			Dim fromDate As DateTime = currentFromDate.Value
			
			While Not isFinished
				Dim currentToDate As DateTime
				
				If config.TimeGap > 0 Then
					currentToDate = fromDate.AddMinutes(config.TimeGap)
					'Cap at targetDate
					If currentToDate > targetDate Then currentToDate = targetDate
				Else
					'Single pass
					currentToDate = targetDate
				End If
				
				'Safety check: if from >= to, we are done
				If fromDate >= currentToDate Then
					isFinished = True
					Continue While
				End If
				
				Dim fromString As String = HelperFunctions.DateTimeToTimestamp(fromDate)
				Dim toString As String = HelperFunctions.DateTimeToTimestamp(currentToDate)
				
				Dim executionStart As DateTime = DateTime.Now
				
				'Log START using LogTable class
				LogTable.InsertLogEntry(si, config.LoadId, config.LoadType, config.TargetTable, executionStart, fromString)
				
				Try
					'1. Extraction (RestFast -> STG)
					Dim recordsLoaded As Integer = Me.ExecuteRequestWithTimeWindow(si, requestName, config.TargetTable, fromString, toString)
					
					'Log SUCCESS for STG extraction
					Dim stgExecutionEnd As DateTime = DateTime.Now
					Dim stgDuration As Double = (stgExecutionEnd - executionStart).TotalSeconds
					LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, config.TargetTable, stgExecutionEnd, recordsLoaded, "OK", "", stgDuration)
					
					'2. Transfer (STG -> LANDING)
					Dim landingExecutionStart As DateTime = DateTime.Now
					LogTable.InsertLogEntry(si, config.LoadId, config.LoadType, landingTable, landingExecutionStart, fromString)
					
					Dim recordsInserted As Integer = Me.TransferToLandingTable(si, config.TargetTable, landingTable, fromString, toString, config.ReferenceColumn)
					
					'Log SUCCESS for LANDING transfer
					Dim landingExecutionEnd As DateTime = DateTime.Now
					Dim landingDuration As Double = (landingExecutionEnd - landingExecutionStart).TotalSeconds
					LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, landingTable, landingExecutionEnd, recordsInserted, "OK", "", landingDuration)
					
					'3. Update ReferenceDate (with Overlap) using HelperFunctions
						'Calculate overlap as percentage of TimeGap (e.g., 50% of 120 min = 60 min overlap)
						Dim overlapMinutes As Double = config.TimeGap * (config.OverlapPercentage / 100D)
						Dim checkpointDate As DateTime = currentToDate.AddMinutes(-overlapMinutes)
					Dim checkpointString As String = HelperFunctions.DateTimeToTimestamp(checkpointDate)
					HelperFunctions.UpdateControlTableReferenceDate(si, config.LoadId, checkpointString)
					
				Catch ex As Exception
					'Log ERROR using LogTable class - update both STG and LANDING entries if they exist
					Dim executionEnd As DateTime = DateTime.Now
					Dim duration As Double = (executionEnd - executionStart).TotalSeconds
					
					'Split error into multiple short log lines to avoid OneStream UI truncation
					Dim shortError As String = If(ex.Message IsNot Nothing AndAlso ex.Message.Length > 200, ex.Message.Substring(0, 200) & "...", ex.Message)
					Dim innerError As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, "")
					Dim errorMsg As String = $"Error processing batch {fromString} to {toString}: {shortError}"
					
					BRApi.ErrorLog.LogMessage(si, $"[ETL ERROR] LoadId={config.LoadId} Batch={fromString}-{toString} Type={ex.GetType().Name}")
					BRApi.ErrorLog.LogMessage(si, $"[ETL ERROR] Message: {shortError}")
					If Not String.IsNullOrEmpty(innerError) Then
						Dim shortInner As String = If(innerError.Length > 200, innerError.Substring(0, 200) & "...", innerError)
						BRApi.ErrorLog.LogMessage(si, $"[ETL ERROR] InnerException: {shortInner}")
					End If
					
					'Full error stored in database (NVARCHAR(MAX), no truncation)
					Dim fullErrorMsg As String = $"Error processing batch {fromString} to {toString}: {ex.Message}" &
						If(ex.InnerException IsNot Nothing, $" | InnerException: {ex.InnerException.Message}", "")
					
					'Update STG entry if still RUNNING
					LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, config.TargetTable, executionEnd, 0, "ERROR", fullErrorMsg, duration)
					'Update LANDING entry if still RUNNING (may not exist if error occurred before LANDING phase)
					LogTable.UpdateLogEntryWithTarget(si, config.LoadId, fromString, landingTable, executionEnd, 0, "ERROR", fullErrorMsg, duration)
					
					Throw New XFException(si, New Exception(errorMsg, ex))
				End Try
				
				'Prepare next iteration
				fromDate = currentToDate
				
				'Exit if single pass or reached target
				If config.TimeGap = 0 OrElse fromDate >= targetDate Then
					isFinished = True
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
	''' Transfers data from STG table to Landing table using DELETE + INSERT pattern.
	''' Detects extra columns in LANDING (not present in STG) and auto-computes them:
	'''   - [Time]: Derived from [postingDate] as 'yyyyMm' (e.g., '2025M6')
	'''   - Other extra columns: Set to NULL
	''' </summary>
	''' <param name="stgTable">Source STG table (e.g., XFC_STG_FIN_MIX)</param>
	''' <param name="landingTable">Target Landing table (e.g., XFC_LANDING_FIN_MIX)</param>
	''' <param name="fromDate">From timestamp for DELETE range</param>
	''' <param name="toDate">To timestamp for DELETE range</param>
	''' <param name="referenceColumn">Column used for DELETE range filter</param>
	''' <returns>Number of records inserted into Landing table</returns>
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
				
				'Get columns from STG table
				Dim stgColumnQuery As String = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{HelperFunctions.EscapeSql(stgTable)}' ORDER BY ORDINAL_POSITION"
				Dim dtStgColumns As DataTable = BRApi.Database.GetDataTable(dbConn, stgColumnQuery, Nothing, Nothing, False)
				
				If dtStgColumns Is Nothing OrElse dtStgColumns.Rows.Count = 0 Then
					Throw New XFException(si, New Exception($"Could not retrieve columns from {stgTable}"))
				End If
				
				Dim stgColumns As New List(Of String)()
				For Each colRow As DataRow In dtStgColumns.Rows
					stgColumns.Add(colRow("COLUMN_NAME").ToString())
				Next
				
				'Get columns from LANDING table to detect extra computed columns
				Dim landingColumnQuery As String = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{HelperFunctions.EscapeSql(landingTable)}' ORDER BY ORDINAL_POSITION"
				Dim dtLandingColumns As DataTable = BRApi.Database.GetDataTable(dbConn, landingColumnQuery, Nothing, Nothing, False)
				
				Dim landingColumns As New List(Of String)()
				If dtLandingColumns IsNot Nothing Then
					For Each colRow As DataRow In dtLandingColumns.Rows
						landingColumns.Add(colRow("COLUMN_NAME").ToString())
					Next
				End If
				
				'Find extra columns in LANDING that are not in STG
				Dim extraColumns As New List(Of String)()
				For Each col As String In landingColumns
					If Not stgColumns.Any(Function(s) s.Equals(col, StringComparison.OrdinalIgnoreCase)) Then
						extraColumns.Add(col)
					End If
				Next
				
				'Build INSERT column list (all LANDING columns) and SELECT expressions
				Dim insertColumns As New List(Of String)()
				Dim selectExpressions As New List(Of String)()
				
				'Add STG columns (direct mapping)
				For Each col As String In stgColumns
					insertColumns.Add($"[{col}]")
					selectExpressions.Add($"[{col}]")
				Next
				
				'Add extra LANDING columns with computed expressions
				For Each col As String In extraColumns
					insertColumns.Add($"[{col}]")
					selectExpressions.Add(Me.GetComputedColumnExpression(col, stgColumns))
				Next
				
				Dim insertColumnList As String = String.Join(", ", insertColumns)
				Dim selectColumnList As String = String.Join(", ", selectExpressions)
				
				'Insert from STG to Landing with computed extra columns
				Dim insertQuery As String = $"INSERT INTO {landingTable} ({insertColumnList}) SELECT {selectColumnList} FROM {stgTable}"
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
	
	''' <summary>
	''' Returns a SQL expression for computed LANDING columns that don't exist in STG.
	''' Known computed columns:
	'''   - [Time]: Derived from [postingDate] → 'yyyyMm' format (e.g., '2025M6')
	''' Unknown columns default to NULL.
	''' </summary>
	Private Function GetComputedColumnExpression(ByVal columnName As String, ByVal stgColumns As List(Of String)) As String
		Select Case columnName.ToLower()
			Case "time"
				'Compute [Time] from [postingDate] if available: yyyyMm format (e.g., 2025M6)
				If stgColumns.Any(Function(c) c.Equals("postingDate", StringComparison.OrdinalIgnoreCase)) Then
					Return "CAST(LEFT(CAST([postingDate] AS NVARCHAR(8)), 4) AS NVARCHAR(4)) + 'M' + " &
					       "CAST(CAST(SUBSTRING(CAST([postingDate] AS NVARCHAR(8)), 5, 2) AS INT) AS NVARCHAR(2)) AS [Time]"
				Else
					Return "NULL AS [Time]"
				End If
			Case Else
				'Unknown extra column - default to NULL
				Return $"NULL AS [{columnName}]"
		End Select
	End Function
	
	#End Region
	
	#End Region
		
	End Class
End Namespace
