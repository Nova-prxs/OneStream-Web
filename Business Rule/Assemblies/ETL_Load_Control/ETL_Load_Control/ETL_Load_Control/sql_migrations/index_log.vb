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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	''' <summary>
	''' SQL table definition for Index Maintenance Log.
	''' Stores historical data of index fragmentation diagnoses and maintenance operations.
	''' </summary>
	Public Class INDEX_LOG
		
		'Table name
		Private Shared ReadOnly tableName As String = "XFC_INDEX_LOG"
		
		#Region "Get Migration Query"

        Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				
				'Up - Create table with indexes
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{tableName}', N'U') IS NULL
				BEGIN
					CREATE TABLE {tableName} (
						[LogId] INT IDENTITY(1,1) PRIMARY KEY,
						[ExecutionDate] DATETIME NOT NULL DEFAULT GETDATE(),
						[Operation] NVARCHAR(20) NOT NULL,
						[TableName] NVARCHAR(128) NOT NULL,
						[IndexName] NVARCHAR(128) NOT NULL,
						[IndexType] NVARCHAR(50) NULL,
						[FragmentationBefore] DECIMAL(5,2) NULL,
						[FragmentationAfter] DECIMAL(5,2) NULL,
						[PageCount] BIGINT NULL,
						[Status] NVARCHAR(20) NOT NULL,
						[Message] NVARCHAR(500) NULL,
						[DurationSeconds] DECIMAL(10,2) NULL,
						[ExecutedBy] NVARCHAR(100) NULL
					);
					
					-- Index on ExecutionDate for historical queries
					CREATE NONCLUSTERED INDEX IX_INDEX_LOG_ExecutionDate ON {tableName} ([ExecutionDate] DESC);
					
					-- Index on TableName and Operation for filtered queries
					CREATE NONCLUSTERED INDEX IX_INDEX_LOG_Table_Operation ON {tableName} ([TableName], [Operation]);
				END;
				"
				
				'Down - Drop table
				Dim downQuery As String = $"
					DROP TABLE IF EXISTS {tableName};
				"
				
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Insert Log Entry"
		
		''' <summary>
		''' Inserts a log entry for an index operation (DIAGNOSE, REORGANIZE, REBUILD, CREATE)
		''' </summary>
		Public Shared Sub InsertLogEntry(
			ByVal si As SessionInfo,
			ByVal operation As String,
			ByVal tblName As String,
			ByVal idxName As String,
			ByVal idxType As String,
			ByVal fragBefore As Double,
			ByVal fragAfter As Double,
			ByVal pageCount As Long,
			ByVal status As String,
			ByVal message As String,
			ByVal duration As Double
		)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim userName As String = si.UserName
					
					Dim insertQuery As String = $"
						INSERT INTO {tableName} (
							[ExecutionDate], [Operation], [TableName], [IndexName], [IndexType],
							[FragmentationBefore], [FragmentationAfter], [PageCount],
							[Status], [Message], [DurationSeconds], [ExecutedBy]
						)
						VALUES (
							GETDATE(), '{operation}', '{tblName}', '{idxName}', '{idxType}',
							{fragBefore.ToString("F2", CultureInfo.InvariantCulture)}, 
							{If(fragAfter >= 0, fragAfter.ToString("F2", CultureInfo.InvariantCulture), "NULL")}, 
							{pageCount},
							'{status}', '{message.Replace("'", "''")}', 
							{duration.ToString("F2", CultureInfo.InvariantCulture)}, '{userName}'
						)
					"
					BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
				End Using
			Catch ex As Exception
				' Don't fail the main operation if logging fails - just log to error log
				BRApi.ErrorLog.LogMessage(si, $"[INDEX_LOG] Warning: Failed to insert log entry: {ex.Message}")
			End Try
		End Sub
		
		#End Region
		
		#Region "Get Log History"
		
		''' <summary>
		''' Gets log history for a specific table or all tables
		''' </summary>
		''' <param name="tblName">Optional: specific table name. Empty = all tables.</param>
		''' <param name="daysBack">Number of days to look back. Default = 30</param>
		Public Shared Function GetLogHistory(
			ByVal si As SessionInfo,
			Optional ByVal tblName As String = "",
			Optional ByVal daysBack As Integer = 30
		) As DataTable
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim tableFilter As String = ""
					If Not String.IsNullOrEmpty(tblName) Then
						tableFilter = $"AND [TableName] = '{tblName}'"
					End If
					
					Dim query As String = $"
						SELECT 
							[LogId],
							[ExecutionDate],
							[Operation],
							[TableName],
							[IndexName],
							[IndexType],
							[FragmentationBefore],
							[FragmentationAfter],
							[PageCount],
							[Status],
							[Message],
							[DurationSeconds],
							[ExecutedBy]
						FROM {tableName}
						WHERE [ExecutionDate] >= DATEADD(DAY, -{daysBack}, GETDATE())
						{tableFilter}
						ORDER BY [ExecutionDate] DESC
					"
					
					Return BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Get Latest Status"
		
		''' <summary>
		''' Gets the latest fragmentation status for each index
		''' </summary>
		Public Shared Function GetLatestStatus(ByVal si As SessionInfo) As DataTable
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						WITH LatestDiagnose AS (
							SELECT 
								[TableName],
								[IndexName],
								[FragmentationBefore] AS [CurrentFragmentation],
								[PageCount],
								[ExecutionDate] AS [LastChecked],
								ROW_NUMBER() OVER (PARTITION BY [TableName], [IndexName] ORDER BY [ExecutionDate] DESC) AS rn
							FROM {tableName}
							WHERE [Operation] = 'DIAGNOSE'
						),
						LatestMaintenance AS (
							SELECT 
								[TableName],
								[IndexName],
								[Operation] AS [LastOperation],
								[ExecutionDate] AS [LastMaintenance],
								[Status] AS [LastStatus],
								ROW_NUMBER() OVER (PARTITION BY [TableName], [IndexName] ORDER BY [ExecutionDate] DESC) AS rn
							FROM {tableName}
							WHERE [Operation] IN ('REORGANIZE', 'REBUILD', 'CREATE')
						)
						SELECT 
							d.[TableName],
							d.[IndexName],
							d.[CurrentFragmentation],
							d.[PageCount],
							d.[LastChecked],
							m.[LastOperation],
							m.[LastMaintenance],
							m.[LastStatus],
							CASE 
								WHEN d.[CurrentFragmentation] < 10 THEN 'OK'
								WHEN d.[CurrentFragmentation] < 30 THEN 'REORGANIZE needed'
								ELSE 'REBUILD needed'
							END AS [Recommendation]
						FROM LatestDiagnose d
						LEFT JOIN LatestMaintenance m ON d.[TableName] = m.[TableName] AND d.[IndexName] = m.[IndexName] AND m.rn = 1
						WHERE d.rn = 1
						ORDER BY d.[CurrentFragmentation] DESC
					"
					
					Return BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Cleanup Old Logs"
		
		''' <summary>
		''' Deletes log entries older than specified days
		''' </summary>
		''' <param name="daysToKeep">Number of days to keep. Default = 90</param>
		Public Shared Function CleanupOldLogs(ByVal si As SessionInfo, Optional ByVal daysToKeep As Integer = 90) As Integer
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim deleteQuery As String = $"
						DELETE FROM {tableName}
						WHERE [ExecutionDate] < DATEADD(DAY, -{daysToKeep}, GETDATE())
					"
					
					' Get count before delete
					Dim countQuery As String = $"
						SELECT COUNT(*) FROM {tableName}
						WHERE [ExecutionDate] < DATEADD(DAY, -{daysToKeep}, GETDATE())
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, countQuery, Nothing, Nothing, False)
					Dim deletedCount As Integer = CInt(dt.Rows(0)(0))
					
					If deletedCount > 0 Then
						BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
						BRApi.ErrorLog.LogMessage(si, $"[INDEX_LOG] Cleanup: Deleted {deletedCount} log entries older than {daysToKeep} days")
					End If
					
					Return deletedCount
				End Using
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
	End Class
End Namespace
