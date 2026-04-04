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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.data_adapter_solution_helper
    
    '============================================================================
    ' Main Business Rule Class
    ' Purpose: Provides two main data retrieval functions:
    '   1. Cost Center Group mapping filtered by entity
    '   2. Staging table validation for unmapped cost centers
    '
    ' CONFIGURATION:
    ' -------------
    ' To change staging filter behavior, modify ENABLE_UD3_SOURCE_FILTER constant below:
    '   - Set to True:  Filter shows UD3_Source != 'None' AND UD3_Target = 'None'
    '   - Set to False: Filter shows only UD3_Target = 'None' (any UD3_Source value)
    '============================================================================
    Public Class MainClass
        
        ' Constants for table and view names
        Private Const TABLE_NAME As String = "XFC_MAIN_MASTER_cost_center_group"
        Private Const STAGING_VIEW As String = "vStageSourceAndTargetDataWithAttributes"
        Private Const SHOW_ALL_RECORDS_ENTITY As String = "HORSE_GROUP"
        
        '============================================================================
        ' STAGING FILTER CONFIGURATION
        ' Set to True: Show only records where UD3_Source != 'None' AND UD3_Target = 'None'
        ' Set to False: Show only records where UD3_Target = 'None' (original behavior)
        '============================================================================
        Private Const ENABLE_UD3_SOURCE_FILTER As Boolean = True  ' <-- CHANGE THIS TO True/False
        
        '============================================================================
        ' Main entry point for the Business Rule
        ' Handles dataset name requests and routes to appropriate functions
        '============================================================================
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
            Try
                Select Case args.FunctionType
                    
                    Case Is = DashboardDataSetFunctionType.GetDataSetNames
                        Dim names As New List(Of String)()
                        names.Add("CostCenter")
                        names.Add("StageTableComparison")
                        Return names
                    
                    Case Is = DashboardDataSetFunctionType.GetDataSet
                        
                        If args.DataSetName.XFEqualsIgnoreCase("CostCenter") Then
                            Return ExecuteCostCenterData(si, globals, api, args)
                            
                        ElseIf args.DataSetName.XFEqualsIgnoreCase("StageTableComparison") Then
                            Return ExecuteStageTableComparison(si, globals, api, args)
                        End If
                        
                End Select
                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        
        #Region "Cost Center Data Functions"
        
        '============================================================================
        ' Retrieves Cost Center data filtered by the current workflow entity
        ' Returns test data if real data is not available
        '============================================================================
        Private Function ExecuteCostCenterData(ByVal si As SessionInfo, ByVal globals As BRGlobals, 
                                              ByVal api As Object, ByVal args As DashboardDataSetArgs) As DataTable
            Try
                ' Create the result DataTable structure
                Dim dataTable As DataTable = CreateCostCenterTable()
                
                ' Get current entity from workflow context
                Dim selectedEntity As String = GetCurrentWorkflowEntity(si)
                Dim entityPrefix As String = GetEntityPrefix(selectedEntity)
                
                ' Attempt to retrieve real data from database
                Dim gotRealData As Boolean = False
                Try
                    gotRealData = GetRealDataFromTable(si, dataTable, selectedEntity, entityPrefix)
                Catch ex As Exception
                    ' If data retrieval fails, add error information
                    AddErrorRow(dataTable, "ERROR", ex.Message, "ERROR")
                End Try
                
                ' If no real data was retrieved, add test data
                If Not gotRealData Then
                    AddTestDataWithEntity(dataTable, selectedEntity, entityPrefix)
                End If
                
                Return dataTable
                
            Catch ex As Exception
                ' In case of total failure, return error table
                Dim errorTable As DataTable = CreateCostCenterTable()
                AddErrorRow(errorTable, "ERROR", ex.Message, "ERROR")
                Return errorTable
            End Try
        End Function
        
        '============================================================================
        ' Creates the structure for Cost Center DataTable
        '============================================================================
        Private Function CreateCostCenterTable() As DataTable
            Dim table As New DataTable("CostCenterTable")
            table.Columns.Add("CostCenter", GetType(String))
            table.Columns.Add("Description", GetType(String))
            table.Columns.Add("CostCenterGroup", GetType(String))
            Return table
        End Function
        
        '============================================================================
        ' Retrieves real data from the cost center group table
        ' Returns True if data was successfully retrieved, False otherwise
        '============================================================================
        Private Function GetRealDataFromTable(ByVal si As SessionInfo, ByVal table As DataTable, 
                                             ByVal selectedEntity As String, ByVal entityPrefix As String) As Boolean
            Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                ' Check if the table exists in the database
                Dim tableExists As Boolean = SqlSecurityHelper.CheckTableExists(dbConnInfo, TABLE_NAME)
                
                If tableExists Then
                    ' Build and execute the query with appropriate filters
                    Dim filterSql As String = BuildFilterQuery(selectedEntity, entityPrefix)
                    Dim realData As DataTable = BRApi.Database.ExecuteSql(dbConnInfo, filterSql, True)
                    
                    If realData IsNot Nothing AndAlso realData.Rows.Count > 0 Then
                        ' Populate table with real data
                        For Each row As DataRow In realData.Rows
                            Dim newRow As DataRow = table.NewRow()
                            newRow("CostCenter") = If(row("CostCenter") Is DBNull.Value, "", row("CostCenter").ToString())
                            newRow("Description") = If(row("Description") Is DBNull.Value, "", row("Description").ToString())
                            newRow("CostCenterGroup") = If(row("CostCenterGroup") Is DBNull.Value, "", row("CostCenterGroup").ToString())
                            table.Rows.Add(newRow)
                        Next
                        Return True
                    Else
                        ' Table exists but no matching data found
                        AddErrorRow(table, "NO_DATA", $"No records found for entity {selectedEntity}", "NO_DATA")
                        Return True
                    End If
                Else
                    ' Table doesn't exist
                    AddErrorRow(table, "TABLE_NOT_EXISTS", 
                               $"Table {TABLE_NAME} does not exist. Run the table creation Business Rule first.", 
                               "ERROR")
                    Return True
                End If
            End Using
        End Function
        
        '============================================================================
        ' Builds SQL query with appropriate filters based on entity
        '============================================================================
        Private Function BuildFilterQuery(ByVal selectedEntity As String, 
                                         ByVal entityPrefix As String) As String
            ' Validate table name for security
            Dim safeTableName As String = TABLE_NAME
            
            ' Check if we should show all records (special case for HORSE_GROUP)
            If ShouldShowAllRecords(selectedEntity) Then
                Return $"SELECT CostCenter, Description, CostCenterGroup FROM {safeTableName} ORDER BY CostCenter"
            End If
            
            ' Apply entity filter if prefix is available
            If Not String.IsNullOrEmpty(entityPrefix) Then
                Dim safePrefix As String = entityPrefix.Replace("'", "''")
                Return $"SELECT CostCenter, Description, CostCenterGroup 
                        FROM {safeTableName} 
                        WHERE entity_id LIKE '{safePrefix}%' 
                        ORDER BY CostCenter"
            Else
                ' No specific filter, return all results
                Return $"SELECT CostCenter, Description, CostCenterGroup 
                        FROM {safeTableName} 
                        ORDER BY CostCenter"
            End If
        End Function
        
        '============================================================================
        ' Adds test data when real data is not available
        '============================================================================
        Private Sub AddTestDataWithEntity(ByVal table As DataTable, ByVal selectedEntity As String, ByVal entityPrefix As String)
            ' Generate test data based on the selected entity
            If Not String.IsNullOrEmpty(entityPrefix) OrElse selectedEntity.ToUpper() = SHOW_ALL_RECORDS_ENTITY Then
                For i As Integer = 1 To 3
                    Dim row As DataRow = table.NewRow()
                    row("CostCenter") = $"CC{selectedEntity}{i:000}"
                    row("Description") = $"Test Cost Center {i} for {selectedEntity}"
                    row("CostCenterGroup") = $"Group_{selectedEntity}_{Chr(64 + i)}" ' A, B, C
                    table.Rows.Add(row)
                Next
            Else
                ' Generic data when no specific entity is detected
                AddErrorRow(table, "CC_GENERIC", 
                           "No specific entity detected - showing generic data", 
                           "GENERIC")
            End If
        End Sub
        
        '============================================================================
        ' Gets the current entity from workflow context
        '============================================================================
        Private Function GetCurrentWorkflowEntity(ByVal si As SessionInfo) As String
            ' Get entities from current workflow profile - this is reliable in dashboard context
            Dim profileEntities As List(Of WorkflowProfileEntityInfo) = 
                BRApi.Workflow.Metadata.GetProfileEntities(si, si.WorkflowClusterPk.ProfileKey)
                
            If profileEntities IsNot Nothing AndAlso profileEntities.Count > 0 Then
                ' Return the first entity from workflow
                Return profileEntities(0).EntityName
            End If
            
            ' Fallback only for preview scenarios
            Return SHOW_ALL_RECORDS_ENTITY
        End Function
        
        '============================================================================
        ' Maps entity names to their prefixes for filtering
        ' Simplified version with dictionary lookup
        '============================================================================
        Private Function GetEntityPrefix(ByVal entity As String) As String
            If String.IsNullOrEmpty(entity) Then Return ""
            
            Dim entityUpper As String = entity.ToUpper().Trim()
            
            ' Define valid prefixes and their lengths
            Dim validPrefixes As New Dictionary(Of String, Integer) From {
                {"R1300", 5},
                {"R1301", 5},
                {"R1302", 5}, 
                {"R1303", 5},
                {"R0671", 5},
                {"R0611", 5},
                {"R0592", 5},
                {"R0585", 5}
            }
            
            ' Find matching prefix
            For Each kvp As KeyValuePair(Of String, Integer) In validPrefixes
                If entityUpper.Length >= kvp.Value AndAlso entityUpper.StartsWith(kvp.Key) Then
                    Return kvp.Key
                End If
            Next
            
            ' Check for exact match as fallback
            If validPrefixes.ContainsKey(entityUpper) Then
                Return entityUpper
            End If
            
            Return ""
        End Function
        
        #End Region
        
        #Region "Stage Table Comparison Functions"
        
        '============================================================================
        ' Compares staging tables to find cost centers without group assignments
        ' Returns a simplified 3-column table with Entity, UD3_Source, and UD3_Target
        ' 
        ' Filter behavior controlled by ENABLE_UD3_SOURCE_FILTER constant:
        '   - When TRUE: Shows records where UD3_Source != 'None' AND UD3_Target = 'None'
        '   - When FALSE: Shows all records where UD3_Target = 'None' (regardless of UD3_Source)
        '============================================================================
        Private Function ExecuteStageTableComparison(ByVal si As SessionInfo, ByVal globals As BRGlobals, 
                                                    ByVal api As Object, ByVal args As DashboardDataSetArgs) As DataTable
            ' Create result table structure
            Dim resultTable As DataTable = CreateStageComparisonTable()
            
            Try
                ' Get context and execute consolidated query
                Dim contextInfo As StageQueryContext = GetStageQueryContext(si)
                Dim queryResult As DataTable = BuildStageComparisonSQL(si, contextInfo)
                
                ' Process and populate results
                PopulateStageResultTable(resultTable, queryResult, contextInfo)
                
            Catch ex As Exception
                AddErrorRow(resultTable, "ERROR", ex.Message, "ERROR")
            End Try
            
            Return resultTable
        End Function
        
        '============================================================================
        ' Creates the structure for Stage Comparison DataTable
        '============================================================================
        Private Function CreateStageComparisonTable() As DataTable
            Dim table As New DataTable("StagingUD3Table")
            table.Columns.Add("Entity", GetType(String))
            table.Columns.Add("UD3_Source", GetType(String))
            table.Columns.Add("UD3_Target", GetType(String))
            Return table
        End Function
        
        '============================================================================
        ' Gets the staging query context from workflow
        '============================================================================
        Private Function GetStageQueryContext(ByVal si As SessionInfo) As StageQueryContext
            Dim context As New StageQueryContext()
            context.ProfileKey = si.WorkflowClusterPk.ProfileKey
            
            ' Get Scenario and Time names directly from BRApi - these are reliable
            context.ScenarioName = BRApi.Finance.Members.GetMemberName(si, DimType.Scenario.Id, si.WorkflowClusterPk.ScenarioKey)
            context.TimeName = BRApi.Finance.Time.GetNameFromID(si, si.WorkflowClusterPk.TimeKey)
            
            ' Get all entities in the profile
            Dim profileEntities As List(Of WorkflowProfileEntityInfo) = 
                BRApi.Workflow.Metadata.GetProfileEntities(si, context.ProfileKey)
                
            If profileEntities IsNot Nothing Then
                For Each entityInfo As WorkflowProfileEntityInfo In profileEntities
                    If entityInfo IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(entityInfo.EntityName) Then
                        context.Entities.Add(entityInfo.EntityName.Trim())
                    End If
                Next
            End If
            
            Return context
        End Function
        
        '============================================================================
        ' Executes consolidated SQL query for stage comparison
        ' All logic consolidated into a single query with view fallback
        '============================================================================
        Private Function BuildStageComparisonSQL(ByVal si As SessionInfo, 
                                                ByVal context As StageQueryContext) As DataTable
            Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                ' Build consolidated SQL query using primary staging view
                Dim sql As String = "SELECT DISTINCT " & vbCrLf &
                                   "    ISNULL(EtT, Et) AS Entity_Name, " & vbCrLf &
                                   "    U3 AS UD3_Source, " & vbCrLf &
                                   "    U3T AS UD3_Target " & vbCrLf &
                                   $"FROM {STAGING_VIEW} " & vbCrLf
                
                ' Apply staging filter based on configuration
                If ENABLE_UD3_SOURCE_FILTER Then
                    ' NEW FILTER: Show only where UD3_Source is NOT 'None' AND UD3_Target IS 'None'
                    sql += "WHERE (U3 != 'None' AND U3 IS NOT NULL) " & vbCrLf &
                           "  AND (U3T = 'None' OR U3T IS NULL) " & vbCrLf
                Else
                    ' ORIGINAL FILTER: Show only where UD3_Target is 'None'
                    sql += "WHERE (U3T = 'None' OR U3T IS NULL) " & vbCrLf
                End If
                
                ' Add scenario filter - ScenarioName is always valid from WFk
                sql += $"  AND (ISNULL(SnT, Sn) = '{SqlSecurityHelper.EscapeString(context.ScenarioName)}') " & vbCrLf
                
                ' Add time filter - TimeName is always valid from WFk
                sql += $"  AND (ISNULL(TmT, Tm) = '{SqlSecurityHelper.EscapeString(context.TimeName)}') " & vbCrLf
                
                ' Add entity filter if entities exist
                If context.Entities.Count > 0 Then
                    Dim safeInClause As String = SqlSecurityHelper.BuildSafeInClause(context.Entities)
                    sql += $"  AND ISNULL(EtT, Et) IN {safeInClause} " & vbCrLf
                End If
                
                sql += "ORDER BY Entity_Name, U3"
                
                ' Execute the consolidated SQL query
                Return BRApi.Database.ExecuteSql(dbConn, sql, True)
            End Using
        End Function
        
        '============================================================================
        ' Populates the result table with query results
        '============================================================================
        Private Sub PopulateStageResultTable(ByVal resultTable As DataTable, 
                                            ByVal queryResult As DataTable, 
                                            ByVal context As StageQueryContext)
            If queryResult IsNot Nothing AndAlso queryResult.Rows.Count > 0 Then
                ' Add each row from query results
                For Each row As DataRow In queryResult.Rows
                    Dim dataRow As DataRow = resultTable.NewRow()
                    dataRow("Entity") = If(row("Entity_Name") Is DBNull.Value, "Unknown", row("Entity_Name").ToString())
                    dataRow("UD3_Source") = If(row("UD3_Source") Is DBNull.Value, "[NULL]", row("UD3_Source").ToString())
                    dataRow("UD3_Target") = If(row("UD3_Target") Is DBNull.Value, "[NULL]", row("UD3_Target").ToString())
                    resultTable.Rows.Add(dataRow)
                Next
            Else
                ' No data found - add informative message
                Dim entities As String = If(context.Entities.Count > 0, 
                                           String.Join(", ", context.Entities.Take(3)) & If(context.Entities.Count > 3, "...", ""), 
                                           "Unknown_Entity")
                
                Dim filterMessage As String = If(ENABLE_UD3_SOURCE_FILTER,
                                                "No records found with UD3_Source != 'None' AND UD3_Target = 'None'",
                                                "No records found with UD3_Target = 'None'")
                
                AddErrorRow(resultTable, entities, filterMessage, "None")
            End If
        End Sub
        
        #End Region
        
        #Region "Helper Functions"
        
        '============================================================================
        ' Checks if entity should show all records (special case)
        '============================================================================
        Private Function ShouldShowAllRecords(ByVal entity As String) As Boolean
            Return Not String.IsNullOrEmpty(entity) AndAlso 
                   entity.ToUpper().Contains(SHOW_ALL_RECORDS_ENTITY.Replace("_", ""))
        End Function
        
        '============================================================================
        ' Adds an error/info row to any table with the standard 3 columns
        '============================================================================
        Private Sub AddErrorRow(ByVal table As DataTable, ByVal col1 As String, 
                               ByVal col2 As String, ByVal col3 As String)
            Dim row As DataRow = table.NewRow()
            row(0) = col1
            row(1) = col2
            row(2) = col3
            table.Rows.Add(row)
        End Sub
        
        #End Region
        
        #Region "Support Classes"
        
        '============================================================================
        ' Context class for staging queries
        '============================================================================
        Private Class StageQueryContext
            Public Property ProfileKey As Guid
            Public Property ScenarioName As String
            Public Property TimeName As String
            Public Property Entities As List(Of String)
            
            Public Sub New()
                Entities = New List(Of String)()
            End Sub
        End Class
        
        '============================================================================
        ' SQL Security Helper Class
        ' Provides methods to prevent SQL injection and validate SQL objects
        '============================================================================
        Public Class SqlSecurityHelper
            
            '========================================================================
            ' Escapes string values to prevent SQL injection
            '========================================================================
            Public Shared Function EscapeString(ByVal value As String) As String
                If String.IsNullOrEmpty(value) Then Return ""
                
                ' Replace single quotes (primary SQL injection vector)
                Return value.Replace("'", "''")
            End Function
            
            '========================================================================
            ' Builds a safe IN clause for SQL queries
            '========================================================================
            Public Shared Function BuildSafeInClause(ByVal values As List(Of String)) As String
                If values Is Nothing OrElse values.Count = 0 Then
                    Return "(NULL)" ' Condition that never matches
                End If
                
                Dim escapedValues = values.Select(Function(v) $"'{EscapeString(v)}'")
                Return $"({String.Join(",", escapedValues)})"
            End Function
            
            '========================================================================
            ' Checks if a table exists in the database
            '========================================================================
            Public Shared Function CheckTableExists(ByVal dbConnInfo As DbConnInfo, ByVal tableName As String) As Boolean
                Try
                    Dim safeName As String = tableName.Replace("'", "''")
                    Dim checkSql As String = $"SELECT COUNT(*) as TableExists FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{safeName}'"
                    Dim result As DataTable = BRApi.Database.ExecuteSql(dbConnInfo, checkSql, True)
                    
                    If result IsNot Nothing AndAlso result.Rows.Count > 0 Then
                        Return Convert.ToInt32(result.Rows(0)("TableExists")) > 0
                    End If
                    Return False
                Catch
                    Return False
                End Try
            End Function
            
        End Class
        
        #End Region
        
    End Class
End Namespace