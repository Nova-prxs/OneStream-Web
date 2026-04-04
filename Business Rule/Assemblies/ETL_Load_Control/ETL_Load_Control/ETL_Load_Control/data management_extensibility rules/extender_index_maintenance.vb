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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_index_maintenance
	Public Class MainClass
		
		#Region "Constants"
		
		''' <summary>
		''' Fragmentation thresholds
		''' </summary>
		Private Const REORGANIZE_THRESHOLD As Double = 10.0  ' 10% - start reorganizing
		Private Const REBUILD_THRESHOLD As Double = 30.0     ' 30% - need full rebuild
		
		#End Region
		
		#Region "Data Classes"
		
		''' <summary>
		''' Index fragmentation information
		''' </summary>
		Public Class IndexFragmentationInfo
			Public Property TableName As String
			Public Property IndexName As String
			Public Property IndexType As String
			Public Property FragmentationPercent As Double
			Public Property PageCount As Long
			Public Property Recommendation As String
		End Class
		
		''' <summary>
		''' Maintenance operation result
		''' </summary>
		Public Class MaintenanceResult
			Public Property TableName As String
			Public Property IndexName As String
			Public Property Operation As String
			Public Property Success As Boolean
			Public Property Message As String
			Public Property DurationSeconds As Double
		End Class
		
		#End Region
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						#Region "Execute Index Maintenance Functions"
						
						' Get function parameter (same pattern as RestFast connector)
						Dim functionName As String = args.NameValuePairs.XFGetValue("p_function", String.Empty).ToUpper()
						Dim tableParam As String = args.NameValuePairs.XFGetValue("p_table", String.Empty).Trim()
						
						' If p_table = "ALL" or empty, process all tables; otherwise process specific table
						Dim tableName As String = If(String.IsNullOrEmpty(tableParam) OrElse tableParam.ToUpper() = "ALL", String.Empty, tableParam)
						
						Select Case functionName
							Case "DIAGNOSE"
								' Diagnose fragmentation levels for all or specific table
								Me.DiagnoseFragmentation(si, tableName)
								Return Nothing
								
							Case "REORGANIZE"
								' Light maintenance - good for weekly runs with daily loads
								Me.ReorganizeIndexes(si, tableName)
								Return Nothing
								
							Case "REBUILD"
								' Heavy maintenance - good for monthly runs
								Me.RebuildIndexes(si, tableName)
								Return Nothing
								
							Case "AUTO"
								' Automatic maintenance based on fragmentation levels
								Me.CreateIndexesIfNotExist(si)
								Me.AutoMaintenance(si, tableName)
								Return Nothing
								
							Case "CREATE_INDEXES"
								' Create indexes if they don't exist
								Me.CreateIndexesIfNotExist(si)
								Return Nothing
								
							Case "FORCE_REBUILD"
								' Force rebuild ALL indexes regardless of fragmentation level
								Me.CreateIndexesIfNotExist(si)
								Me.ForceRebuildIndexes(si, tableName)
								Return Nothing
								
							Case Else
								BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] Unknown function: {functionName}. Available: DIAGNOSE, REORGANIZE, REBUILD, AUTO, CREATE_INDEXES, FORCE_REBUILD")
								Return Nothing
						End Select
						
						#End Region
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "DIAGNOSE - Check Fragmentation Levels"
		
		''' <summary>
		''' Diagnoses fragmentation levels for indexes on specified tables.
		''' </summary>
		Private Sub DiagnoseFragmentation(ByVal si As SessionInfo, ByVal tableName As String)
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Build table filter
					Dim tableFilter As String
					If Not String.IsNullOrEmpty(tableName) Then
						tableFilter = $"OBJECT_NAME(ips.object_id) = '{tableName}'"
					Else
						Dim tableList As String = String.Join(",", Me.GetAllTableNames().Select(Function(t) $"'{t}'"))
						tableFilter = $"OBJECT_NAME(ips.object_id) IN ({tableList})"
					End If
					
					Dim query As String = $"
						SELECT 
							OBJECT_NAME(ips.object_id) AS TableName,
							i.name AS IndexName,
							ips.index_type_desc AS IndexType,
							ROUND(ips.avg_fragmentation_in_percent, 2) AS FragmentationPercent,
							ips.page_count AS PageCount,
							CASE 
								WHEN ips.avg_fragmentation_in_percent < {REORGANIZE_THRESHOLD} THEN 'OK - No action needed'
								WHEN ips.avg_fragmentation_in_percent < {REBUILD_THRESHOLD} THEN 'REORGANIZE recommended'
								ELSE 'REBUILD recommended'
							END AS Recommendation
						FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
						INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
						WHERE {tableFilter}
							AND i.name IS NOT NULL
							AND ips.page_count > 0
						ORDER BY ips.avg_fragmentation_in_percent DESC
					"
					
					Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
					
					' Log each result to XFC_INDEX_LOG
					For Each row As DataRow In dt.Rows
						INDEX_LOG.InsertLogEntry(
							si,
							"DIAGNOSE",
							row("TableName").ToString(),
							row("IndexName").ToString(),
							row("IndexType").ToString(),
							CDbl(row("FragmentationPercent")),
							-1,
							CLng(row("PageCount")),
							"OK",
							row("Recommendation").ToString(),
							0
						)
					Next
					
					' Log summary
					Dim highFragCount As Integer = dt.AsEnumerable().Where(Function(r) CDbl(r("FragmentationPercent")) >= REBUILD_THRESHOLD).Count()
					Dim medFragCount As Integer = dt.AsEnumerable().Where(Function(r) CDbl(r("FragmentationPercent")) >= REORGANIZE_THRESHOLD AndAlso CDbl(r("FragmentationPercent")) < REBUILD_THRESHOLD).Count()
					
					BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] Diagnosis complete: {dt.Rows.Count} indexes analyzed. High fragmentation: {highFragCount}, Medium: {medFragCount}")
					
				End Using
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
		#Region "REORGANIZE - Light Maintenance (Weekly)"
		
		''' <summary>
		''' Reorganizes indexes - lightweight, online operation.
		''' </summary>
		Private Sub ReorganizeIndexes(ByVal si As SessionInfo, ByVal tableName As String)
			Try
				Dim startTime As DateTime = DateTime.Now
				Dim successCount As Integer = 0
				Dim totalCount As Integer = 0
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					Dim indexes As List(Of IndexFragmentationInfo) = Me.GetIndexesToMaintain(si, dbConn, tableName, REORGANIZE_THRESHOLD)
					totalCount = indexes.Count
					
					For Each idx As IndexFragmentationInfo In indexes
						Dim opStart As DateTime = DateTime.Now
						
						Try
							Dim reorganizeQuery As String = $"ALTER INDEX [{idx.IndexName}] ON [{idx.TableName}] REORGANIZE"
							BRApi.Database.ExecuteSql(dbConn, reorganizeQuery, False)
							
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							successCount += 1
							
							INDEX_LOG.InsertLogEntry(
								si,
								"REORGANIZE",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								-1,
								idx.PageCount,
								"OK",
								$"Reorganized successfully (was {idx.FragmentationPercent:F2}% fragmented)",
								duration
							)
							
						Catch ex As Exception
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							INDEX_LOG.InsertLogEntry(
								si,
								"REORGANIZE",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								-1,
								idx.PageCount,
								"ERROR",
								$"Error: {ex.Message}",
								duration
							)
						End Try
					Next
					
				End Using
				
				Dim totalDuration As Double = (DateTime.Now - startTime).TotalSeconds
				BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] REORGANIZE complete: {successCount}/{totalCount} indexes processed in {totalDuration:F2}s")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
		#Region "REBUILD - Heavy Maintenance (Monthly)"
		
		''' <summary>
		''' Rebuilds indexes - heavier operation but more thorough.
		''' </summary>
		Private Sub RebuildIndexes(ByVal si As SessionInfo, ByVal tableName As String)
			Try
				Dim startTime As DateTime = DateTime.Now
				Dim successCount As Integer = 0
				Dim totalCount As Integer = 0
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					Dim indexes As List(Of IndexFragmentationInfo) = Me.GetIndexesToMaintain(si, dbConn, tableName, REORGANIZE_THRESHOLD)
					totalCount = indexes.Count
					
					For Each idx As IndexFragmentationInfo In indexes
						Dim opStart As DateTime = DateTime.Now
						
						Try
							Dim rebuildQuery As String = $"ALTER INDEX [{idx.IndexName}] ON [{idx.TableName}] REBUILD"
							BRApi.Database.ExecuteSql(dbConn, rebuildQuery, False)
							
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							successCount += 1
							
							INDEX_LOG.InsertLogEntry(
								si,
								"REBUILD",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								0,
								idx.PageCount,
								"OK",
								$"Rebuilt successfully (was {idx.FragmentationPercent:F2}% fragmented)",
								duration
							)
							
						Catch ex As Exception
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							INDEX_LOG.InsertLogEntry(
								si,
								"REBUILD",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								-1,
								idx.PageCount,
								"ERROR",
								$"Error: {ex.Message}",
								duration
							)
						End Try
					Next
					
				End Using
				
				Dim totalDuration As Double = (DateTime.Now - startTime).TotalSeconds
				BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] REBUILD complete: {successCount}/{totalCount} indexes processed in {totalDuration:F2}s")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
		#Region "AUTO - Automatic Maintenance Based on Fragmentation"
		
		''' <summary>
		''' Automatically chooses REORGANIZE or REBUILD based on fragmentation level.
		''' </summary>
		Private Sub AutoMaintenance(ByVal si As SessionInfo, ByVal tableName As String)
			Try
				Dim startTime As DateTime = DateTime.Now
				Dim successCount As Integer = 0
				Dim reorganizeCount As Integer = 0
				Dim rebuildCount As Integer = 0
				Dim totalCount As Integer = 0
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					Dim indexes As List(Of IndexFragmentationInfo) = Me.GetIndexesToMaintain(si, dbConn, tableName, REORGANIZE_THRESHOLD)
					totalCount = indexes.Count
					
					If totalCount = 0 Then
						INDEX_LOG.InsertLogEntry(
							si,
							"AUTO",
							If(String.IsNullOrEmpty(tableName), "ALL", tableName),
							"-",
							"-",
							0,
							0,
							0,
							"OK",
							"Health check passed: No indexes require maintenance (< 10% fragmentation)",
							(DateTime.Now - startTime).TotalSeconds
						)
					End If
					
					For Each idx As IndexFragmentationInfo In indexes
						Dim opStart As DateTime = DateTime.Now
						Dim operation As String = If(idx.FragmentationPercent >= REBUILD_THRESHOLD, "REBUILD", "REORGANIZE")
						
						Try
							Dim maintenanceQuery As String = $"ALTER INDEX [{idx.IndexName}] ON [{idx.TableName}] {operation}"
							BRApi.Database.ExecuteSql(dbConn, maintenanceQuery, False)
							
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							successCount += 1
							If operation = "REORGANIZE" Then reorganizeCount += 1 Else rebuildCount += 1
							
							INDEX_LOG.InsertLogEntry(
								si,
								"AUTO",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								If(operation = "REBUILD", 0, -1),
								idx.PageCount,
								"OK",
								$"Auto-maintenance action: {operation} (Fragmentation: {idx.FragmentationPercent:F2}%)",
								duration
							)
							
						Catch ex As Exception
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							INDEX_LOG.InsertLogEntry(
								si,
								"AUTO",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								-1,
								idx.PageCount,
								"ERROR",
								$"Error during {operation}: {ex.Message}",
								duration
							)
						End Try
					Next
					
				End Using
				
				Dim totalDuration As Double = (DateTime.Now - startTime).TotalSeconds
				BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] AUTO complete: {successCount}/{totalCount} indexes ({reorganizeCount} reorganized, {rebuildCount} rebuilt) in {totalDuration:F2}s")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
		#Region "CREATE_INDEXES - Create Indexes If Not Exist"
		
		''' <summary>
		''' Creates all required indexes if they don't exist.
		''' Gets index definitions dynamically from each table class.
		''' </summary>
		Private Sub CreateIndexesIfNotExist(ByVal si As SessionInfo)
			Try
				Dim successCount As Integer = 0
				Dim totalCount As Integer = 0
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Get index definitions dynamically from each table class
					Dim indexDefinitions As List(Of Tuple(Of String, String, String)) = Me.GetAllIndexDefinitions()
					
					totalCount = indexDefinitions.Count
					
					For Each indexDef In indexDefinitions
						Dim tblName As String = indexDef.Item1
						Dim idxName As String = indexDef.Item2
						Dim columns As String = indexDef.Item3
						
						Try
							' Check if index exists first to avoid log noise
							Dim checkQuery As String = $"SELECT 1 FROM sys.indexes WHERE name = '{idxName}' AND object_id = OBJECT_ID('{tblName}')"
							Dim dtCheck As DataTable = BRApi.Database.GetDataTable(dbConn, checkQuery, Nothing, Nothing, False)
							
							If dtCheck.Rows.Count = 0 Then
								Dim createQuery As String = $"CREATE NONCLUSTERED INDEX [{idxName}] ON [{tblName}] ({columns})"
								Dim opStart As DateTime = DateTime.Now
								BRApi.Database.ExecuteSql(dbConn, createQuery, False)
								
								Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
								successCount += 1
								
								INDEX_LOG.InsertLogEntry(
									si,
									"CREATE",
									tblName,
									idxName,
									"NONCLUSTERED",
									0,
									0,
									0,
									"OK",
									"Index created successfully",
									duration
								)
							Else
								' Index already exists - do not log to keep history clean
								successCount += 1
							End If
							
						Catch ex As Exception
							BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] Error creating index {idxName}: {ex.Message}")
						End Try
					Next
					
				End Using
				
				BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] CREATE_INDEXES complete: {successCount}/{totalCount} indexes verified/created")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
		#Region "FORCE_REBUILD - Force Rebuild All Indexes"
		
		''' <summary>
		''' Forces rebuild of ALL indexes regardless of fragmentation level.
		''' Use this after massive data loads or when you want to ensure optimal index performance.
		''' </summary>
		Private Sub ForceRebuildIndexes(ByVal si As SessionInfo, ByVal tableName As String)
			Try
				Dim startTime As DateTime = DateTime.Now
				Dim successCount As Integer = 0
				Dim totalCount As Integer = 0
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					
					' Get ALL indexes (threshold = 0 means all indexes)
					Dim indexes As List(Of IndexFragmentationInfo) = Me.GetAllIndexes(si, dbConn, tableName)
					totalCount = indexes.Count
					
					For Each idx As IndexFragmentationInfo In indexes
						Dim opStart As DateTime = DateTime.Now
						
						Try
							Dim rebuildQuery As String = $"ALTER INDEX [{idx.IndexName}] ON [{idx.TableName}] REBUILD"
							BRApi.Database.ExecuteSql(dbConn, rebuildQuery, False)
							
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							successCount += 1
							
							INDEX_LOG.InsertLogEntry(
								si,
								"FORCE_REBUILD",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								0,
								idx.PageCount,
								"OK",
								$"Force rebuilt successfully (was {idx.FragmentationPercent:F2}% fragmented)",
								duration
							)
							
						Catch ex As Exception
							Dim duration As Double = (DateTime.Now - opStart).TotalSeconds
							INDEX_LOG.InsertLogEntry(
								si,
								"FORCE_REBUILD",
								idx.TableName,
								idx.IndexName,
								idx.IndexType,
								idx.FragmentationPercent,
								-1,
								idx.PageCount,
								"ERROR",
								$"Error: {ex.Message}",
								duration
							)
						End Try
					Next
					
				End Using
				
				Dim totalDuration As Double = (DateTime.Now - startTime).TotalSeconds
				BRApi.ErrorLog.LogMessage(si, $"[INDEX_MAINTENANCE] FORCE_REBUILD complete: {successCount}/{totalCount} indexes rebuilt in {totalDuration:F2}s")
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Sub
		
		#End Region
		
		#Region "GetIndexesToMaintain"
		
		''' <summary>
		''' Gets list of indexes that need maintenance based on fragmentation threshold.
		''' </summary>
		Private Function GetIndexesToMaintain(
			ByVal si As SessionInfo,
			ByVal dbConn As DbConnInfo,
			ByVal tableName As String,
			ByVal fragmentationThreshold As Double
		) As List(Of IndexFragmentationInfo)
			
			Dim results As New List(Of IndexFragmentationInfo)()
			
			' Build table filter
			Dim tableFilter As String
			If Not String.IsNullOrEmpty(tableName) Then
				tableFilter = $"OBJECT_NAME(ips.object_id) = '{tableName}'"
			Else
				Dim tableList As String = String.Join(",", Me.GetAllTableNames().Select(Function(t) $"'{t}'"))
				tableFilter = $"OBJECT_NAME(ips.object_id) IN ({tableList})"
			End If
			
			Dim query As String = $"
				SELECT 
					OBJECT_NAME(ips.object_id) AS TableName,
					i.name AS IndexName,
					ips.index_type_desc AS IndexType,
					ROUND(ips.avg_fragmentation_in_percent, 2) AS FragmentationPercent,
					ips.page_count AS PageCount
				FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
				INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
				WHERE {tableFilter}
					AND i.name IS NOT NULL
					AND ips.page_count > 0
					AND ips.avg_fragmentation_in_percent >= {fragmentationThreshold}
				ORDER BY ips.avg_fragmentation_in_percent DESC
			"
			
			Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
			
			For Each row As DataRow In dt.Rows
				Dim info As New IndexFragmentationInfo()
				info.TableName = row("TableName").ToString()
				info.IndexName = row("IndexName").ToString()
				info.IndexType = row("IndexType").ToString()
				info.FragmentationPercent = CDbl(row("FragmentationPercent"))
				info.PageCount = CLng(row("PageCount"))
				results.Add(info)
			Next
			
			Return results
		End Function
		
		''' <summary>
		''' Gets ALL indexes regardless of fragmentation level (for FORCE_REBUILD).
		''' </summary>
		Private Function GetAllIndexes(
			ByVal si As SessionInfo,
			ByVal dbConn As DbConnInfo,
			ByVal tableName As String
		) As List(Of IndexFragmentationInfo)
			
			Dim results As New List(Of IndexFragmentationInfo)()
			
			' Build table filter
			Dim tableFilter As String
			If Not String.IsNullOrEmpty(tableName) Then
				tableFilter = $"OBJECT_NAME(ips.object_id) = '{tableName}'"
			Else
				Dim tableList As String = String.Join(",", Me.GetAllTableNames().Select(Function(t) $"'{t}'"))
				tableFilter = $"OBJECT_NAME(ips.object_id) IN ({tableList})"
			End If
			
			' Query WITHOUT fragmentation threshold - gets ALL indexes
			Dim query As String = $"
				SELECT 
					OBJECT_NAME(ips.object_id) AS TableName,
					i.name AS IndexName,
					ips.index_type_desc AS IndexType,
					ROUND(ips.avg_fragmentation_in_percent, 2) AS FragmentationPercent,
					ips.page_count AS PageCount
				FROM sys.dm_db_index_physical_stats(DB_ID(), NULL, NULL, NULL, 'LIMITED') ips
				INNER JOIN sys.indexes i ON ips.object_id = i.object_id AND ips.index_id = i.index_id
				WHERE {tableFilter}
					AND i.name IS NOT NULL
					AND ips.page_count > 0
				ORDER BY ips.avg_fragmentation_in_percent DESC
			"
			
			Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, query, Nothing, Nothing, False)
			
			For Each row As DataRow In dt.Rows
				Dim info As New IndexFragmentationInfo()
				info.TableName = row("TableName").ToString()
				info.IndexName = row("IndexName").ToString()
				info.IndexType = row("IndexType").ToString()
				info.FragmentationPercent = CDbl(row("FragmentationPercent"))
				info.PageCount = CLng(row("PageCount"))
				results.Add(info)
			Next
			
			Return results
		End Function
		
		#End Region
		
		#Region "GetAllIndexDefinitions"
		
		''' <summary>
		''' Gets all index definitions dynamically from each table class.
		''' This allows each table class to define its own indexes.
		''' </summary>
		Private Function GetAllIndexDefinitions() As List(Of Tuple(Of String, String, String))
			Dim allIndexes As New List(Of Tuple(Of String, String, String))()
			
			' Get indexes from each table class
			allIndexes.AddRange(LANDING_FIN_MIX.GetIndexDefinitions())
			allIndexes.AddRange(dwh_fin_mix.GetIndexDefinitions())
			allIndexes.AddRange(STG_ETL_LOAD_LOG.GetIndexDefinitions())
			
			Return allIndexes
		End Function
		
		''' <summary>
		''' Gets all table names dynamically from each table class.
		''' </summary>
		Private Function GetAllTableNames() As String()
			Return New String() {
				LANDING_FIN_MIX.GetTableName(),
				dwh_fin_mix.GetTableName(),
				STG_ETL_LOAD_LOG.GetTableName()
			}
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
