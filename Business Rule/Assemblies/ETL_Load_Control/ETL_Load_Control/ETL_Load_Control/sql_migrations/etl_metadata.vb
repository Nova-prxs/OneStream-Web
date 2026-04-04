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
	''' Auto-discovery utilities for ETL Mapping Manager.
	''' Provides metadata about Landing/DWH tables and their columns.
	''' Uses INFORMATION_SCHEMA for dynamic discovery.
	''' </summary>
	Public Class ETL_METADATA
		
		#Region "Constants"
		
		''' <summary>Table prefixes for auto-discovery</summary>
		Private Shared ReadOnly LANDING_PREFIX As String = "XFC_LANDING_"
		Private Shared ReadOnly DWH_PREFIX As String = "XFC_DWH_"
		Private Shared ReadOnly STG_PREFIX As String = "XFC_STG_"
		
		#End Region
		
		#Region "Table Info Class"
		
		''' <summary>
		''' Represents table metadata
		''' </summary>
		Public Class TableInfo
			Public Property TableName As String = String.Empty
			Public Property TableType As String = String.Empty   ' LANDING, DWH, STG, OTHER
			Public Property ColumnCount As Integer = 0
			Public Property Description As String = String.Empty
		End Class
		
		''' <summary>
		''' Represents column metadata
		''' </summary>
		Public Class ColumnInfo
			Public Property ColumnName As String = String.Empty
			Public Property DataType As String = String.Empty
			Public Property MaxLength As Integer = 0
			Public Property IsNullable As Boolean = True
			Public Property OrdinalPosition As Integer = 0
		End Class
		
		#End Region
		
		#Region "Get Landing Tables"
		
		''' <summary>
		''' Gets all Landing tables (XFC_LANDING_*) from the database
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>List of TableInfo for Landing tables</returns>
		Public Shared Function GetLandingTables(ByVal si As SessionInfo) As List(Of TableInfo)
			Return GetTablesByPrefix(si, LANDING_PREFIX, "LANDING")
		End Function
		
		#End Region
		
		#Region "Get DWH Tables"
		
		''' <summary>
		''' Gets all DWH tables (XFC_DWH_*) from the database
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>List of TableInfo for DWH tables</returns>
		Public Shared Function GetDWHTables(ByVal si As SessionInfo) As List(Of TableInfo)
			Return GetTablesByPrefix(si, DWH_PREFIX, "DWH")
		End Function
		
		#End Region
		
		#Region "Get Staging Tables"
		
		''' <summary>
		''' Gets all Staging tables (XFC_STG_*) from the database
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>List of TableInfo for Staging tables</returns>
		Public Shared Function GetStagingTables(ByVal si As SessionInfo) As List(Of TableInfo)
			Return GetTablesByPrefix(si, STG_PREFIX, "STG")
		End Function
		
		#End Region
		
		#Region "Get All ETL Tables"
		
		''' <summary>
		''' Gets all ETL-related tables (LANDING, DWH, STG)
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>List of all ETL tables</returns>
		Public Shared Function GetAllETLTables(ByVal si As SessionInfo) As List(Of TableInfo)
			Dim allTables As New List(Of TableInfo)()
			allTables.AddRange(GetLandingTables(si))
			allTables.AddRange(GetDWHTables(si))
			allTables.AddRange(GetStagingTables(si))
			Return allTables.OrderBy(Function(t) t.TableType).ThenBy(Function(t) t.TableName).ToList()
		End Function
		
		#End Region
		
		#Region "Get Tables By Prefix"
		
		''' <summary>
		''' Gets tables matching a specific prefix
		''' </summary>
		Private Shared Function GetTablesByPrefix(ByVal si As SessionInfo, ByVal prefix As String, ByVal tableType As String) As List(Of TableInfo)
			Dim tables As New List(Of TableInfo)()
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT 
							t.TABLE_NAME,
							(SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS c WHERE c.TABLE_NAME = t.TABLE_NAME) AS ColumnCount
						FROM INFORMATION_SCHEMA.TABLES t
						WHERE t.TABLE_TYPE = 'BASE TABLE'
						  AND t.TABLE_NAME LIKE '{prefix}%'
						ORDER BY t.TABLE_NAME
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							Dim info As New TableInfo()
							info.TableName = row("TABLE_NAME").ToString()
							info.TableType = tableType
							info.ColumnCount = CInt(row("ColumnCount"))
							info.Description = GetTableDescription(info.TableName, tableType)
							tables.Add(info)
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get {tableType} tables: {ex.Message}")
			End Try
			
			Return tables
		End Function
		
		''' <summary>
		''' Generates a friendly description based on table name
		''' </summary>
		Private Shared Function GetTableDescription(ByVal tableName As String, ByVal tableType As String) As String
			Select Case tableType
				Case "LANDING"
					Return $"Source landing table for {tableName.Replace("XFC_LANDING_", "").Replace("_", " ")}"
				Case "DWH"
					Return $"Data warehouse table for {tableName.Replace("XFC_DWH_", "").Replace("_", " ")}"
				Case "STG"
					Return $"Staging table for {tableName.Replace("XFC_STG_", "").Replace("_", " ")}"
				Case Else
					Return tableName
			End Select
		End Function
		
		#End Region
		
		#Region "Get Table Columns"
		
		''' <summary>
		''' Gets all columns for a specific table
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="tableName">Table name to get columns for</param>
		''' <returns>List of ColumnInfo</returns>
		Public Shared Function GetTableColumns(ByVal si As SessionInfo, ByVal tableName As String) As List(Of ColumnInfo)
			Dim columns As New List(Of ColumnInfo)()
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT 
							COLUMN_NAME,
							DATA_TYPE,
							ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) AS MaxLength,
							CASE WHEN IS_NULLABLE = 'YES' THEN 1 ELSE 0 END AS IsNullable,
							ORDINAL_POSITION
						FROM INFORMATION_SCHEMA.COLUMNS
						WHERE TABLE_NAME = '{HelperFunctions.EscapeSql(tableName)}'
						ORDER BY ORDINAL_POSITION
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							Dim info As New ColumnInfo()
							info.ColumnName = row("COLUMN_NAME").ToString()
							info.DataType = row("DATA_TYPE").ToString()
							info.MaxLength = CInt(row("MaxLength"))
							info.IsNullable = CBool(row("IsNullable"))
							info.OrdinalPosition = CInt(row("ORDINAL_POSITION"))
							columns.Add(info)
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get columns for {tableName}: {ex.Message}")
			End Try
			
			Return columns
		End Function
		
		#End Region
		
		#Region "Get Column Names"
		
		''' <summary>
		''' Gets just the column names for a table (simplified for dropdowns)
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="tableName">Table name</param>
		''' <returns>List of column names</returns>
		Public Shared Function GetColumnNames(ByVal si As SessionInfo, ByVal tableName As String) As List(Of String)
			Return GetTableColumns(si, tableName).Select(Function(c) c.ColumnName).ToList()
		End Function
		
		#End Region
		
		#Region "Get Distinct Column Values"
		
		''' <summary>
		''' Gets distinct values from a specific column (useful for condition builders).
		''' Limited to prevent performance issues.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="tableName">Table name</param>
		''' <param name="columnName">Column name</param>
		''' <param name="maxValues">Maximum values to return (default 100)</param>
		''' <returns>List of distinct values as strings</returns>
		Public Shared Function GetDistinctColumnValues(ByVal si As SessionInfo, ByVal tableName As String, ByVal columnName As String, Optional ByVal maxValues As Integer = 100) As List(Of String)
			Dim values As New List(Of String)()
			
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT DISTINCT TOP {maxValues} CAST({HelperFunctions.EscapeSql(columnName)} AS NVARCHAR(255)) AS Val
						FROM {HelperFunctions.EscapeSql(tableName)}
						WHERE {HelperFunctions.EscapeSql(columnName)} IS NOT NULL
						ORDER BY Val
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					If dt IsNot Nothing Then
						For Each row As DataRow In dt.Rows
							If Not IsDBNull(row("Val")) Then
								values.Add(row("Val").ToString())
							End If
						Next
					End If
				End Using
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"[WARNING] Failed to get distinct values for {tableName}.{columnName}: {ex.Message}")
			End Try
			
			Return values
		End Function
		
		#End Region
		
		#Region "Validate Table Exists"
		
		''' <summary>
		''' Checks if a table exists in the database
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="tableName">Table name to validate</param>
		''' <returns>True if table exists</returns>
		Public Shared Function TableExists(ByVal si As SessionInfo, ByVal tableName As String) As Boolean
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT COUNT(*) 
						FROM INFORMATION_SCHEMA.TABLES 
						WHERE TABLE_NAME = '{HelperFunctions.EscapeSql(tableName)}'
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					Return dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso CInt(dt.Rows(0)(0)) > 0
				End Using
			Catch ex As Exception
				Return False
			End Try
		End Function
		
		#End Region
		
		#Region "Validate Column Exists"
		
		''' <summary>
		''' Checks if a column exists in a table
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="tableName">Table name</param>
		''' <param name="columnName">Column name to validate</param>
		''' <returns>True if column exists</returns>
		Public Shared Function ColumnExists(ByVal si As SessionInfo, ByVal tableName As String, ByVal columnName As String) As Boolean
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = $"
						SELECT COUNT(*) 
						FROM INFORMATION_SCHEMA.COLUMNS 
						WHERE TABLE_NAME = '{HelperFunctions.EscapeSql(tableName)}'
						  AND COLUMN_NAME = '{HelperFunctions.EscapeSql(columnName)}'
					"
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					Return dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso CInt(dt.Rows(0)(0)) > 0
				End Using
			Catch ex As Exception
				Return False
			End Try
		End Function
		
		#End Region
		
		#Region "Get Source-Target Pairs"
		
		''' <summary>
		''' Gets common Landing->DWH table pairs based on naming convention.
		''' Assumes XFC_LANDING_X maps to XFC_DWH_X
		''' </summary>
		''' <param name="si">Session info</param>
		''' <returns>Dictionary of Landing->DWH table pairs</returns>
		Public Shared Function GetSourceTargetPairs(ByVal si As SessionInfo) As Dictionary(Of String, String)
			Dim pairs As New Dictionary(Of String, String)()
			
			Dim landingTables As List(Of TableInfo) = GetLandingTables(si)
			Dim dwhTables As List(Of TableInfo) = GetDWHTables(si)
			
			For Each landing As TableInfo In landingTables
				' Extract suffix (e.g., FIN_MIX from XFC_LANDING_FIN_MIX)
				Dim suffix As String = landing.TableName.Replace(LANDING_PREFIX, "")
				Dim expectedDWH As String = DWH_PREFIX & suffix
				
				' Check if matching DWH table exists
				Dim matchingDWH As TableInfo = dwhTables.FirstOrDefault(Function(d) d.TableName = expectedDWH)
				If matchingDWH IsNot Nothing Then
					pairs.Add(landing.TableName, matchingDWH.TableName)
				End If
			Next
			
			Return pairs
		End Function
		
		#End Region
		
		#Region "Get Common Columns Between Tables"
		
		''' <summary>
		''' Gets columns that exist in both source and target tables
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="sourceTable">Source table name</param>
		''' <param name="targetTable">Target table name</param>
		''' <returns>List of common column names</returns>
		Public Shared Function GetCommonColumns(ByVal si As SessionInfo, ByVal sourceTable As String, ByVal targetTable As String) As List(Of String)
			Dim sourceColumns As List(Of String) = GetColumnNames(si, sourceTable)
			Dim targetColumns As List(Of String) = GetColumnNames(si, targetTable)
			Return sourceColumns.Intersect(targetColumns).ToList()
		End Function
		
		''' <summary>
		''' Gets columns that exist only in the target table (potential mapping targets)
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="sourceTable">Source table name</param>
		''' <param name="targetTable">Target table name</param>
		''' <returns>List of target-only column names</returns>
		Public Shared Function GetTargetOnlyColumns(ByVal si As SessionInfo, ByVal sourceTable As String, ByVal targetTable As String) As List(Of String)
			Dim sourceColumns As List(Of String) = GetColumnNames(si, sourceTable)
			Dim targetColumns As List(Of String) = GetColumnNames(si, targetTable)
			Return targetColumns.Except(sourceColumns).ToList()
		End Function
		
		#End Region
		
	End Class
End Namespace
