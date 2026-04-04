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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class STG_ETL_LOAD_LOG
		
		'Declare table name
		Private Shared ReadOnly TABLE_NAME As String = "XFC_STG_ETL_LOAD_LOG"
		Dim tableName As String = TABLE_NAME
		
		#Region "Index Definitions"
		
		''' <summary>
		''' Returns the table name for this class
		''' </summary>
		Public Shared Function GetTableName() As String
			Return TABLE_NAME
		End Function
		
		''' <summary>
		''' Returns index definitions for this table: (TableName, IndexName, Columns)
		''' Used by extender_index_maintenance for dynamic index management
		''' </summary>
		Public Shared Function GetIndexDefinitions() As List(Of Tuple(Of String, String, String))
			Dim indexes As New List(Of Tuple(Of String, String, String))()
			' Index on LoadId (used in all UPDATE and CHECK queries)
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_STG_ETL_LOAD_LOG_LoadId", "[LoadId]"))
			' Composite index on LoadId and LoadStatus (most common query pattern)
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_STG_ETL_LOAD_LOG_LoadId_Status", "[LoadId], [LoadStatus]"))
			' Index on LoadStatus for filtering RUNNING processes
			indexes.Add(Tuple.Create(TABLE_NAME, "IX_STG_ETL_LOAD_LOG_Status", "[LoadStatus]"))
			' Note: Entity column is NVARCHAR(MAX) and cannot be indexed
			Return indexes
		End Function
		
		#End Region
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
					    LogId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
					    LoadId INT NOT NULL,
					    LoadType NVARCHAR(20) NOT NULL,
					    SourceTable NVARCHAR(128) NULL,
					    TargetTable NVARCHAR(128) NULL,
					    ExecutionStart DATETIME NOT NULL,
					    ExecutionEnd DATETIME NOT NULL,
						ExecutionDuration FLOAT NULL,
					    Records INT NULL,
					    LoadStatus NVARCHAR(20) NULL,
					    ErrorMessage NVARCHAR(MAX) NULL,
					    ReferenceDate NVARCHAR(20) NULL,
					    ExecutedBy NVARCHAR(50) NULL,
					    Entity NVARCHAR(MAX) NULL
					);
					
					-- Index on LoadId (used in all UPDATE and CHECK queries)
					CREATE NONCLUSTERED INDEX IX_STG_ETL_LOAD_LOG_LoadId ON {tableName} ([LoadId]);
					
					-- Composite index on LoadId and LoadStatus (most common query pattern)
					CREATE NONCLUSTERED INDEX IX_STG_ETL_LOAD_LOG_LoadId_Status ON {tableName} ([LoadId], [LoadStatus]);
					
					-- Index on LoadStatus for filtering RUNNING processes
					CREATE NONCLUSTERED INDEX IX_STG_ETL_LOAD_LOG_Status ON {tableName} ([LoadStatus]);
					
					-- Note: Entity column is NVARCHAR(MAX) and cannot be indexed
				END;
				
				-- Add Entity column if table already exists (migration for existing tables)
				-- Note: NVARCHAR(MAX) columns cannot be indexed in SQL Server
				IF OBJECT_ID(N'{tableName}', N'U') IS NOT NULL AND NOT EXISTS (
					SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
					WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = 'Entity'
				)
				BEGIN
					ALTER TABLE {tableName} ADD [Entity] NVARCHAR(MAX) NULL;
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE {tableName};
				"
				
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Get Population Query"

        Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"

				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {tableName};
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Insert Log Entry"
		
		''' <summary>
		''' Inserts a new log entry with RUNNING status when a load process starts (ETL version)
		''' </summary>
		''' <param name="referenceDate">For ETL: yyyyMMddHHmmss format. For DWH: OneStream format (e.g., 2025M1)</param>
		Public Sub InsertLogEntry(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			ByVal loadType As String,
			ByVal targetTable As String,
			ByVal executionStart As DateTime,
			ByVal referenceDate As String
		)
			InsertDWHLogEntry(si, loadId, loadType, "", targetTable, executionStart, referenceDate)
		End Sub
		
		''' <summary>
		''' Inserts a new log entry with RUNNING status when a load process starts (DWH version with SourceTable)
		''' </summary>
		''' <param name="sourceTable">Source table name (for DWH loads)</param>
		''' <param name="targetTable">Target table name</param>
		''' <param name="referenceDate">For DWH: OneStream format (e.g., 2025M1)</param>
		''' <param name="entity">Optional: Entity or comma-separated entities being processed (for Import/Autoload flows)</param>
		Public Sub InsertDWHLogEntry(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			ByVal loadType As String,
			ByVal sourceTable As String,
			ByVal targetTable As String,
			ByVal executionStart As DateTime,
			ByVal referenceDate As String,
			Optional ByVal entity As String = ""
		)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim sqlDateTime As String = HelperFunctions.DateTimeToSqlString(executionStart)
					Dim entityValue As String = If(String.IsNullOrEmpty(entity), "NULL", $"'{HelperFunctions.EscapeSql(entity)}'")
					Dim insertQuery As String = $"
						INSERT INTO {tableName} (
							[LoadId], [LoadType], [SourceTable], [TargetTable], [ExecutionStart], [ExecutionEnd],
							[Records], [LoadStatus], [ErrorMessage], [ReferenceDate], [ExecutedBy], [ExecutionDuration], [Entity]
						)
						VALUES (
							{loadId},
							'{HelperFunctions.EscapeSql(loadType)}',
							'{HelperFunctions.EscapeSql(sourceTable)}',
							'{HelperFunctions.EscapeSql(targetTable)}',
							'{sqlDateTime}',
							'{sqlDateTime}',
							0,
							'RUNNING',
							NULL,
							'{HelperFunctions.EscapeSql(referenceDate)}',
							'{si.UserName}',
							0,
							{entityValue}
						)
					"
					BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
				End Using
			Catch ex As Exception
				'Log error but don't throw to avoid interrupting main process
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to insert log entry: {ex.Message}")
			End Try
		End Sub
		
		#End Region
		
		#Region "Update Log Entry"
		
		''' <summary>
		''' Updates an existing log entry when load completes (OK or ERROR) - ETL version
		''' </summary>
		Public Sub UpdateLogEntry(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			ByVal referenceDate As String,
			ByVal executionEnd As DateTime,
			ByVal recordCount As Integer,
			ByVal loadStatus As String,
			ByVal errorMessage As String,
			ByVal executionDuration As Double
		)
			UpdateLogEntryWithTarget(si, loadId, referenceDate, "", executionEnd, recordCount, loadStatus, errorMessage, executionDuration)
		End Sub
		
		''' <summary>
		''' Updates an existing log entry when load completes (OK or ERROR) - with TargetTable filter
		''' </summary>
		Public Sub UpdateLogEntryWithTarget(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			ByVal referenceDate As String,
			ByVal targetTable As String,
			ByVal executionEnd As DateTime,
			ByVal recordCount As Integer,
			ByVal loadStatus As String,
			ByVal errorMessage As String,
			ByVal executionDuration As Double
		)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim safeDuration As String = executionDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)
					
					Dim targetFilter As String = ""
					If Not String.IsNullOrEmpty(targetTable) Then
						targetFilter = $" AND TargetTable = '{HelperFunctions.EscapeSql(targetTable)}'"
					End If
					
					Dim updateQuery As String = $"
						UPDATE {tableName} 
						SET LoadStatus = '{HelperFunctions.EscapeSql(loadStatus)}', 
							ExecutionEnd = '{HelperFunctions.DateTimeToSqlString(executionEnd)}', 
							Records = {recordCount}, 
							ErrorMessage = '{HelperFunctions.EscapeSql(errorMessage)}',
							ExecutionDuration = {safeDuration}
						WHERE LoadId = {loadId} 
						  AND ReferenceDate = '{HelperFunctions.EscapeSql(referenceDate)}' 
						  AND LoadStatus = 'RUNNING'
						  {targetFilter}
					"
					BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
				End Using
			Catch ex As Exception
				'Log error but don't throw to avoid interrupting main process
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to update log entry: {ex.Message}")
			End Try
		End Sub
		
		''' <summary>
		''' Updates an existing log entry when DWH load completes (OK or ERROR)
		''' </summary>
		''' <param name="entity">Optional: Entity filter to match the specific log entry (for Import/Autoload flows)</param>
		Public Sub UpdateDWHLogEntry(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			ByVal sourceTable As String,
			ByVal targetTable As String,
			ByVal executionEnd As DateTime,
			ByVal recordCount As Integer,
			ByVal loadStatus As String,
			ByVal errorMessage As String,
			ByVal executionDuration As Double,
			Optional ByVal entity As String = ""
		)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim safeDuration As String = executionDuration.ToString(System.Globalization.CultureInfo.InvariantCulture)
					
					' Build entity filter: if entity provided, match it; otherwise match NULL or empty
					Dim entityFilter As String = ""
					If Not String.IsNullOrEmpty(entity) Then
						entityFilter = $" AND [Entity] = '{HelperFunctions.EscapeSql(entity)}'"
					End If
					
					Dim updateQuery As String = $"
						UPDATE {tableName} 
						SET LoadStatus = '{HelperFunctions.EscapeSql(loadStatus)}', 
							ExecutionEnd = '{HelperFunctions.DateTimeToSqlString(executionEnd)}', 
							Records = {recordCount}, 
							ErrorMessage = '{HelperFunctions.EscapeSql(errorMessage)}',
							ExecutionDuration = {safeDuration}
						WHERE LoadId = {loadId} 
						  AND SourceTable = '{HelperFunctions.EscapeSql(sourceTable)}'
						  AND TargetTable = '{HelperFunctions.EscapeSql(targetTable)}'
						  AND LoadStatus = 'RUNNING'
						  {entityFilter}
					"
					BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
				End Using
			Catch ex As Exception
				'Log error but don't throw to avoid interrupting main process
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to update log entry: {ex.Message}")
			End Try
		End Sub
		
		#End Region
		
		#Region "Cleanup Stale Running Entries"
		
		''' <summary>
		''' Default timeout in minutes for stale process detection
		''' Processes running longer than this are considered stale/orphaned
		''' </summary>
		Public Shared ReadOnly DEFAULT_STALE_TIMEOUT_MINUTES As Integer = 20
		
		''' <summary>
		''' Marks stale RUNNING entries as CANCELLED.
		''' This handles cases where OneStream cancels a Data Management task
		''' but the log entry remains with RUNNING status, blocking future executions.
		''' </summary>
		''' <param name="staleMinutes">Minutes after which a RUNNING entry is considered stale (default: 60)</param>
		''' <returns>Number of stale entries cleaned up</returns>
		Public Function CleanupStaleRunningEntries(
			ByVal si As SessionInfo,
			Optional ByVal staleMinutes As Integer = 20
		) As Integer
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim updateQuery As String = $"
						UPDATE {tableName} 
						SET LoadStatus = 'CANCELLED', 
							ErrorMessage = 'Auto-cancelled: Process exceeded {staleMinutes} minute timeout (likely cancelled by system)',
							ExecutionEnd = GETDATE(),
							ExecutionDuration = DATEDIFF(SECOND, ExecutionStart, GETDATE())
						WHERE LoadStatus = 'RUNNING'
						  AND DATEDIFF(MINUTE, ExecutionStart, GETDATE()) >= {staleMinutes}
					"
					BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
				End Using
				
				Return 0
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to cleanup stale running entries: {ex.Message}")
				Return 0
			End Try
		End Function
		
		''' <summary>
		''' Marks stale RUNNING entries as CANCELLED for a specific LoadId.
		''' </summary>
		''' <param name="loadId">The LoadId to check for stale entries</param>
		''' <param name="staleMinutes">Minutes after which a RUNNING entry is considered stale (default: 60)</param>
		''' <returns>Number of stale entries cleaned up</returns>
		Public Function CleanupStaleRunningEntriesForLoadId(
			ByVal si As SessionInfo,
			ByVal loadId As Integer,
			Optional ByVal staleMinutes As Integer = 20
		) As Integer
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim updateQuery As String = $"
						UPDATE {tableName} 
						SET LoadStatus = 'CANCELLED', 
							ErrorMessage = 'Auto-cancelled: Process exceeded {staleMinutes} minute timeout (likely cancelled by system)',
							ExecutionEnd = GETDATE(),
							ExecutionDuration = DATEDIFF(SECOND, ExecutionStart, GETDATE())
						WHERE LoadId = {loadId}
						  AND LoadStatus = 'RUNNING'
						  AND DATEDIFF(MINUTE, ExecutionStart, GETDATE()) >= {staleMinutes}
					"
					BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
				End Using
				
				Return 0
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to cleanup stale running entries for LoadId {loadId}: {ex.Message}")
				Return 0
			End Try
		End Function
		
		#End Region
		
		#Region "Check Running Process"
		
		''' <summary>
		''' Checks if there's a running process for a given LoadId.
		''' Automatically cleans up stale entries before checking.
		''' </summary>
		''' <param name="cleanupStaleFirst">If True, cleans up stale entries before checking (default: True)</param>
		''' <param name="staleMinutes">Minutes after which a RUNNING entry is considered stale (default: 60)</param>
		Public Function HasRunningProcess(
			ByVal si As SessionInfo, 
			ByVal loadId As Integer,
			Optional ByVal cleanupStaleFirst As Boolean = True,
			Optional ByVal staleMinutes As Integer = 20
		) As Boolean
			Try
				'Cleanup stale entries first if requested
				If cleanupStaleFirst Then
					CleanupStaleRunningEntriesForLoadId(si, loadId, staleMinutes)
				End If
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					'Check for running process, but ONLY if it started within the staleMinutes window.
					'This prevents infinite blocking if the cleanup fails or if clocks are slightly out of sync but the process is clearly old.
					Dim checkQuery As String = $"SELECT COUNT(*) as RunningCount FROM {tableName} WHERE LoadId = {loadId} AND LoadStatus = 'RUNNING' AND DATEDIFF(MINUTE, ExecutionStart, GETDATE()) < {staleMinutes}"
					Dim dtCheck As DataTable = BRApi.Database.GetDataTable(dbConn, checkQuery, Nothing, Nothing, False)
					If dtCheck.Rows.Count > 0 AndAlso CInt(dtCheck.Rows(0)("RunningCount")) > 0 Then
						Return True
					End If
				End Using
				Return False
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to check running process: {ex.Message}")
				Return False
			End Try
		End Function
		
		''' <summary>
		''' Checks if there's a running process for a given LoadId and table combination (DWH version).
		''' Automatically cleans up stale entries before checking.
		''' </summary>
		''' <param name="cleanupStaleFirst">If True, cleans up stale entries before checking (default: True)</param>
		''' <param name="staleMinutes">Minutes after which a RUNNING entry is considered stale (default: 60)</param>
		Public Function HasRunningProcess(
			ByVal si As SessionInfo, 
			ByVal loadId As Integer,
			ByVal sourceTable As String,
			ByVal targetTable As String,
			Optional ByVal cleanupStaleFirst As Boolean = True,
			Optional ByVal staleMinutes As Integer = 20
		) As Boolean
			Try
				'Cleanup stale entries first if requested
				If cleanupStaleFirst Then
					CleanupStaleRunningEntriesForLoadId(si, loadId, staleMinutes)
				End If
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					'Check for running process, but ONLY if it valid (not stale)
					Dim checkQuery As String = $"
						SELECT COUNT(*) as RunningCount 
						FROM {tableName} 
						WHERE LoadId = {loadId} 
						  AND SourceTable = '{HelperFunctions.EscapeSql(sourceTable)}'
						  AND TargetTable = '{HelperFunctions.EscapeSql(targetTable)}'
						  AND LoadStatus = 'RUNNING'
						  AND DATEDIFF(MINUTE, ExecutionStart, GETDATE()) < {staleMinutes}
					"
					Dim dtCheck As DataTable = BRApi.Database.GetDataTable(dbConn, checkQuery, Nothing, Nothing, False)
					If dtCheck.Rows.Count > 0 AndAlso CInt(dtCheck.Rows(0)("RunningCount")) > 0 Then
						Return True
					End If
				End Using
				Return False
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to check running process: {ex.Message}")
				Return False
			End Try
		End Function
		
		#End Region

	End Class
End Namespace
