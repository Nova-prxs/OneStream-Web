Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
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
	
	''' <summary>
	''' Manages the XFC_QUERY_DOWNLOAD table for storing reusable SQL queries.
	''' Users can define queries in this table and execute them by ID from the dashboard.
	''' </summary>
	Public Class QUERY_DOWNLOAD
		
		'Declare table name
		Private Shared ReadOnly TABLE_NAME As String = "XFC_QUERY_DOWNLOAD"
		
		#Region "Get Table Name"
		
		''' <summary>
		''' Returns the table name for this class
		''' </summary>
		Public Shared Function GetTableName() As String
			Return TABLE_NAME
		End Function
		
		#End Region
		
		#Region "Query Configuration Class"
		
		''' <summary>
		''' Configuration object for stored queries
		''' </summary>
		Public Class QueryConfig
			Public Property QueryId As Integer = 0
			Public Property QueryName As String = String.Empty
			Public Property QueryText As String = String.Empty
			Public Property OutputFileName As String = String.Empty
		End Class
		
		#End Region
		
		#Region "Get Migration Query"

		Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
			Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				
				'Up
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{TABLE_NAME}', N'U') IS NULL
				BEGIN
					CREATE TABLE {TABLE_NAME} (
						QueryId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
						QueryName NVARCHAR(100) NOT NULL,
						QueryText NVARCHAR(MAX) NOT NULL,
						OutputFileName NVARCHAR(100) NULL
					);
					
					-- Unique index on QueryName for quick lookups
					CREATE UNIQUE NONCLUSTERED INDEX IX_QUERY_DOWNLOAD_Name 
					ON {TABLE_NAME} (QueryName);
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP TABLE {TABLE_NAME};
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
				
				'Up - Seed example queries
				Dim upQuery As String = $"
				-- Seed example queries
				IF NOT EXISTS (SELECT 1 FROM {TABLE_NAME} WHERE QueryName = 'DWH_Accounts_Export')
				BEGIN
					INSERT INTO {TABLE_NAME} (QueryName, QueryText, OutputFileName)
					VALUES 
						('DWH_Accounts_Export', 
						 'SELECT * FROM XFC_DWH_FIN_MIX WHERE Account IN (''403000'',''411000'',''421000'',''430100'',''451000'',''620000'',''620050'',''620090'',''621000'',''621050'',''621090'',''622000'',''622050'',''622090'',''623000'',''731100'',''811800'',''930100'')',
						 'DWH_Accounts_Export'),
						('Landing_Portugal', 
						 'SELECT * FROM XFC_LANDING_FIN_MIX WHERE companyCode LIKE ''PT%''',
						 'Landing_Portugal');
				END;
				"
				
				'Down
				Dim downQuery As String = $"
					TRUNCATE TABLE {TABLE_NAME};
				"
				
				Return If(type.ToLower = "up", upQuery, downQuery)
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Get Query By Id"
		
		''' <summary>
		''' Gets a query configuration by its ID
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="queryId">The query ID</param>
		''' <returns>QueryConfig object or Nothing if not found</returns>
		Public Shared Function GetQueryById(ByVal si As SessionInfo, ByVal queryId As Integer) As QueryConfig
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT QueryId, QueryName, QueryText, OutputFileName
						FROM {TABLE_NAME}
						WHERE QueryId = {queryId}
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Return ParseQueryRow(dt.Rows(0))
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get query by ID {queryId}: {ex.Message}")
			End Try
			
			Return Nothing
		End Function
		
		#End Region
		
		#Region "Get Query By Name"
		
		''' <summary>
		''' Gets a query configuration by its name
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="queryName">The query name</param>
		''' <returns>QueryConfig object or Nothing if not found</returns>
		Public Shared Function GetQueryByName(ByVal si As SessionInfo, ByVal queryName As String) As QueryConfig
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT QueryId, QueryName, QueryText, OutputFileName
						FROM {TABLE_NAME}
						WHERE QueryName = '{HelperFunctions.EscapeSql(queryName)}'
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Return ParseQueryRow(dt.Rows(0))
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get query by name '{queryName}': {ex.Message}")
			End Try
			
			Return Nothing
		End Function
		
		#End Region
		
		#Region "Get All Queries"
		
		''' <summary>
		''' Gets all queries (for management UI / SQL Table Editor)
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>List of QueryConfig objects</returns>
		Public Shared Function GetAllQueries(ByVal si As SessionInfo) As List(Of QueryConfig)
			Dim queries As New List(Of QueryConfig)()
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT QueryId, QueryName, QueryText, OutputFileName
						FROM {TABLE_NAME}
						ORDER BY QueryName ASC
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							queries.Add(ParseQueryRow(row))
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get all queries: {ex.Message}")
			End Try
			
			Return queries
		End Function
		
		#End Region
		
		#Region "Parse Row Helper"
		
		''' <summary>
		''' Parses a DataRow into QueryConfig object
		''' </summary>
		Private Shared Function ParseQueryRow(ByVal row As DataRow) As QueryConfig
			Dim config As New QueryConfig()
			config.QueryId = CInt(row("QueryId"))
			config.QueryName = If(IsDBNull(row("QueryName")), "", row("QueryName").ToString())
			config.QueryText = If(IsDBNull(row("QueryText")), "", row("QueryText").ToString())
			config.OutputFileName = If(IsDBNull(row("OutputFileName")), "", row("OutputFileName").ToString())
			Return config
		End Function
		
		#End Region
		
		#Region "CRUD Operations"
		
		''' <summary>
		''' Inserts a new query into XFC_QUERY_DOWNLOAD
		''' </summary>
		Public Shared Function InsertQuery(
			ByVal si As SessionInfo,
			ByVal queryName As String,
			ByVal queryText As String,
			Optional ByVal outputFileName As String = ""
		) As Integer
			Try
				If String.IsNullOrEmpty(outputFileName) Then
					outputFileName = queryName
				End If
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						INSERT INTO {TABLE_NAME} (QueryName, QueryText, OutputFileName)
						OUTPUT INSERTED.QueryId
						VALUES (
							'{HelperFunctions.EscapeSql(queryName)}',
							'{HelperFunctions.EscapeSql(queryText)}',
							'{HelperFunctions.EscapeSql(outputFileName)}'
						)
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Return CInt(dt.Rows(0)(0))
					End If
				End Using
				Return 0
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to insert query: {ex.Message}", ex))
			End Try
		End Function
		
		''' <summary>
		''' Updates an existing query
		''' </summary>
		Public Shared Sub UpdateQuery(
			ByVal si As SessionInfo,
			ByVal queryId As Integer,
			ByVal queryName As String,
			ByVal queryText As String,
			Optional ByVal outputFileName As String = ""
		)
			Try
				If String.IsNullOrEmpty(outputFileName) Then
					outputFileName = queryName
				End If
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						UPDATE {TABLE_NAME}
						SET QueryName = '{HelperFunctions.EscapeSql(queryName)}',
							QueryText = '{HelperFunctions.EscapeSql(queryText)}',
							OutputFileName = '{HelperFunctions.EscapeSql(outputFileName)}'
						WHERE QueryId = {queryId}
					"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to update query {queryId}: {ex.Message}", ex))
			End Try
		End Sub
		
		''' <summary>
		''' Deletes a query by QueryId
		''' </summary>
		Public Shared Sub DeleteQuery(ByVal si As SessionInfo, ByVal queryId As Integer)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"DELETE FROM {TABLE_NAME} WHERE QueryId = {queryId}"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to delete query {queryId}: {ex.Message}", ex))
			End Try
		End Sub
		
		#End Region

	End Class
End Namespace
