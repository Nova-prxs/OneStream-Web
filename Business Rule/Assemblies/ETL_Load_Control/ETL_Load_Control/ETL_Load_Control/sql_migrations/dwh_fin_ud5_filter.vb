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
	''' Filter table for UD5 dimension mappings.
	''' Maps CCTreatmentCode to AccTreatmentName for UD5 transformations.
	''' </summary>
	Public Class dwh_fin_ud5_filter
		
		'Declare table name
		Private Shared ReadOnly TABLE_NAME As String = "XFC_DWH_FIN_UD5_FILTER"
		
		#Region "Table Info"
		
		''' <summary>
		''' Returns the table name for this class
		''' </summary>
		Public Shared Function GetTableName() As String
			Return TABLE_NAME
		End Function
		
		#End Region
		
		#Region "Filter Config Class"
		
		''' <summary>
		''' Configuration object for UD5 filter entries
		''' </summary>
		Public Class Ud5FilterConfig
			Public Property CCTreatmentCode As String = String.Empty
			Public Property AccTreatmentName As String = String.Empty
		End Class
		
		#End Region
		
		#Region "Get Migration Query"

		Public Function GetMigrationQuery(ByVal si As SessionInfo, ByVal type As String) As String
			Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				
				'Up - Create table with primary key on CCTreatmentCode
				Dim upQuery As String = $"
				IF OBJECT_ID(N'{TABLE_NAME}', N'U') IS NULL
				BEGIN
					CREATE TABLE {TABLE_NAME} (
						[CCTreatmentCode] NVARCHAR(10) NOT NULL PRIMARY KEY,
						[AccTreatmentName] NVARCHAR(50) NOT NULL
					);
					
					-- Index on AccTreatmentName for reverse lookups
					CREATE NONCLUSTERED INDEX IX_DWH_FIN_UD5_FILTER_AccTreatmentName 
					ON {TABLE_NAME} ([AccTreatmentName]);
				END;
				"
				
				'Down - Drop table
				Dim downQuery As String = $"
					DROP TABLE IF EXISTS {TABLE_NAME};
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
				
				'Up - Seed initial data (add your mappings here)
				Dim upQuery As String = $"
				-- Seed initial UD5 filter mappings
				-- Example:
				-- IF NOT EXISTS (SELECT 1 FROM {TABLE_NAME} WHERE CCTreatmentCode = 'CODE1')
				-- BEGIN
				--     INSERT INTO {TABLE_NAME} (CCTreatmentCode, AccTreatmentName) VALUES ('CODE1', 'Treatment Name 1');
				-- END;
				"
				
				'Down - Truncate table
				Dim downQuery As String = $"
					TRUNCATE TABLE {TABLE_NAME};
				"
				
				Return If(type.ToLower = "up", upQuery, downQuery)
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Get All Filters"
		
		''' <summary>
		''' Gets all filter entries from the table
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>List of Ud5FilterConfig</returns>
		Public Shared Function GetAllFilters(ByVal si As SessionInfo) As List(Of Ud5FilterConfig)
			Dim filters As New List(Of Ud5FilterConfig)()
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT CCTreatmentCode, AccTreatmentName
						FROM {TABLE_NAME}
						ORDER BY CCTreatmentCode
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							Dim config As New Ud5FilterConfig()
							config.CCTreatmentCode = row("CCTreatmentCode").ToString()
							config.AccTreatmentName = row("AccTreatmentName").ToString()
							filters.Add(config)
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get UD5 filters: {ex.Message}")
			End Try
			
			Return filters
		End Function
		
		#End Region
		
		#Region "Get Treatment Name By Code"
		
		''' <summary>
		''' Gets the AccTreatmentName for a specific CCTreatmentCode
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="ccTreatmentCode">The treatment code to look up</param>
		''' <returns>AccTreatmentName or empty string if not found</returns>
		Public Shared Function GetTreatmentNameByCode(ByVal si As SessionInfo, ByVal ccTreatmentCode As String) As String
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT AccTreatmentName
						FROM {TABLE_NAME}
						WHERE CCTreatmentCode = '{HelperFunctions.EscapeSql(ccTreatmentCode)}'
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
						Return dt.Rows(0)("AccTreatmentName").ToString()
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get treatment name for code '{ccTreatmentCode}': {ex.Message}")
			End Try
			
			Return String.Empty
		End Function
		
		#End Region
		
		#Region "Build Lookup Dictionary"
		
		''' <summary>
		''' Builds a dictionary for fast in-memory lookups: CCTreatmentCode → AccTreatmentName
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>Dictionary for lookups</returns>
		Public Shared Function BuildLookupDictionary(ByVal si As SessionInfo) As Dictionary(Of String, String)
			Dim lookup As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
			
			Dim filters As List(Of Ud5FilterConfig) = GetAllFilters(si)
			For Each filter As Ud5FilterConfig In filters
				If Not lookup.ContainsKey(filter.CCTreatmentCode) Then
					lookup.Add(filter.CCTreatmentCode, filter.AccTreatmentName)
				End If
			Next
			
			Return lookup
		End Function
		
		#End Region
		
		#Region "CRUD Operations"
		
		''' <summary>
		''' Inserts or updates a filter entry (upsert)
		''' </summary>
		Public Shared Sub UpsertFilter(ByVal si As SessionInfo, ByVal ccTreatmentCode As String, ByVal accTreatmentName As String)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						MERGE INTO {TABLE_NAME} AS target
						USING (SELECT '{HelperFunctions.EscapeSql(ccTreatmentCode)}' AS CCTreatmentCode, 
						              '{HelperFunctions.EscapeSql(accTreatmentName)}' AS AccTreatmentName) AS source
						ON target.CCTreatmentCode = source.CCTreatmentCode
						WHEN MATCHED THEN
							UPDATE SET AccTreatmentName = source.AccTreatmentName
						WHEN NOT MATCHED THEN
							INSERT (CCTreatmentCode, AccTreatmentName)
							VALUES (source.CCTreatmentCode, source.AccTreatmentName);
					"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to upsert UD5 filter: {ex.Message}", ex))
			End Try
		End Sub
		
		''' <summary>
		''' Deletes a filter entry by CCTreatmentCode
		''' </summary>
		Public Shared Sub DeleteFilter(ByVal si As SessionInfo, ByVal ccTreatmentCode As String)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"DELETE FROM {TABLE_NAME} WHERE CCTreatmentCode = '{HelperFunctions.EscapeSql(ccTreatmentCode)}'"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to delete UD5 filter: {ex.Message}", ex))
			End Try
		End Sub
		
		''' <summary>
		''' Checks if a CCTreatmentCode exists in the filter table
		''' </summary>
		Public Shared Function FilterExists(ByVal si As SessionInfo, ByVal ccTreatmentCode As String) As Boolean
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"SELECT COUNT(*) FROM {TABLE_NAME} WHERE CCTreatmentCode = '{HelperFunctions.EscapeSql(ccTreatmentCode)}'"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					Return dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso CInt(dt.Rows(0)(0)) > 0
				End Using
			Catch ex As Exception
				Return False
			End Try
		End Function
		
		#End Region
		
		#Region "Bulk Operations"
		
		''' <summary>
		''' Bulk inserts multiple filter entries
		''' </summary>
		Public Shared Sub BulkInsertFilters(ByVal si As SessionInfo, ByVal filters As List(Of Ud5FilterConfig))
			If filters Is Nothing OrElse filters.Count = 0 Then Return
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim values As New List(Of String)()
					For Each filter As Ud5FilterConfig In filters
						values.Add($"('{HelperFunctions.EscapeSql(filter.CCTreatmentCode)}', '{HelperFunctions.EscapeSql(filter.AccTreatmentName)}')")
					Next
					
					Dim query As String = $"
						INSERT INTO {TABLE_NAME} (CCTreatmentCode, AccTreatmentName)
						VALUES {String.Join(", ", values)}
					"
					BRApi.Database.ExecuteSql(dbConn, query, False)
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to bulk insert UD5 filters: {ex.Message}", ex))
			End Try
		End Sub
		
		''' <summary>
		''' Replaces all filters with a new set (truncate + insert)
		''' </summary>
		Public Shared Sub ReplaceAllFilters(ByVal si As SessionInfo, ByVal filters As List(Of Ud5FilterConfig))
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					' Truncate existing data
					BRApi.Database.ExecuteSql(dbConn, $"TRUNCATE TABLE {TABLE_NAME}", False)
					
					' Bulk insert new data
					If filters IsNot Nothing AndAlso filters.Count > 0 Then
						BulkInsertFilters(si, filters)
					End If
				End Using
			Catch ex As Exception
				Throw New XFException(si, New Exception($"Failed to replace UD5 filters: {ex.Message}", ex))
			End Try
		End Sub
		
		#End Region
		
	End Class
End Namespace
