' SQL_Data.vb — SQL_EXTENDER assembly, OCE workspace
' Dispatches to one of three operations based on the adapter that calls it:
'
'   da_SQL_Execute  → CustomSubstVars contains SQL_QUERY → ExecuteSQL path
'                     Supports chunked queries: SQL_CHUNK_COUNT + SQL_CHUNK_N
'   da_connectionSchema → CustomSubstVars contains ConnectionName (no SQL_QUERY) → GetDatabaseSchema path
'   da_DataSource_List  → CustomSubstVars is empty → GetDatabaseConnections path
'
' CustomSubstVars keys for da_SQL_Execute (short query):
'   SQL_QUERY=<url-encoded-sql>, ConnectionName=<name>, LoggingLevel=<level>
'
' CustomSubstVars keys for da_SQL_Execute (chunked query):
'   SQL_CHUNK_COUNT=<n>, SQL_CHUNK_0=<part0>, SQL_CHUNK_1=<part1>, ..., ConnectionName=<name>, LoggingLevel=<level>
'
' CustomSubstVars keys for da_connectionSchema:
'   ConnectionName=<name>
'
' CustomSubstVars keys for da_DataSource_List:
'   (empty dictionary)
'
' Returns:
'   da_SQL_Execute      → DataTable named QUERY_RESULTS
'   da_connectionSchema → DataTable named SCHEMA_RESULTS
'   da_DataSource_List  → DataTable named OCE_DATASOURCES

Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Web
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.SQL_Data

    Public Class MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
            ' --- FunctionType dispatch (matches proven OneStream workspace assembly pattern) ---
            ' The framework calls Main with args.FunctionType to either list dataset names
            ' or execute a specific dataset. args.DataSetName is populated from the second
            ' part of the adapter's methodQuery (e.g., {ExecuteSQL}, {GetDatabaseSchema}).
            Select Case args.FunctionType
                Case Is = DashboardDataSetFunctionType.GetDataSetNames
                    ' Return the dataset names this assembly handles
                    Return New List(Of String) From {"ExecuteSQL", "GetDatabaseSchema", "GetDatabaseConnections"}

                Case Is = DashboardDataSetFunctionType.GetDataSet
                    ' Fall through to the data retrieval logic below
            End Select

            Dim resultDs As New DataSet()
            Dim csvars As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

            Try
                ' --- Step 1: Read both dictionaries from DashboardDataSetArgs ---
                ' The REST API sends CustomSubstVarsAsCommaSeparatedPairs as a string.
                ' The framework may populate CustomSubstVars, NameValuePairs, or both.
                ' We check both and merge them so dispatch works regardless.

                ' Try CustomSubstVars first
                If args IsNot Nothing AndAlso args.CustomSubstVars IsNot Nothing Then
                    For Each kvp In args.CustomSubstVars
                        csvars(kvp.Key) = kvp.Value
                    Next
                End If

                ' Also merge NameValuePairs (framework may put values here instead)
                If args IsNot Nothing AndAlso args.NameValuePairs IsNot Nothing Then
                    For Each kvp In args.NameValuePairs
                        If Not csvars.ContainsKey(kvp.Key) Then
                            csvars(kvp.Key) = kvp.Value
                        End If
                    Next
                End If

                ' --- Step 2: Dispatch based on args.DataSetName (from methodQuery) ---
                ' Primary dispatch uses DataSetName which the framework sets from the
                ' adapter's methodQuery. Fallback to substvar content analysis for
                ' backward compatibility.
                Dim dataSetName As String = If(args IsNot Nothing AndAlso args.DataSetName IsNot Nothing, args.DataSetName, String.Empty)

                If dataSetName.XFEqualsIgnoreCase("ExecuteSQL") OrElse
                   csvars.ContainsKey("SQL_QUERY") OrElse csvars.ContainsKey("SQL_CHUNK_COUNT") Then
                    ' da_SQL_Execute path
                    Dim dt As DataTable = ExecuteSQL(si, csvars)
                    resultDs.Tables.Add(dt)
                ElseIf dataSetName.XFEqualsIgnoreCase("GetDatabaseSchema") OrElse
                       (csvars.ContainsKey("ConnectionName") AndAlso Not csvars.ContainsKey("SQL_QUERY")) Then
                    ' da_connectionSchema path
                    Dim dt As DataTable = GetDatabaseSchema(si, csvars)
                    resultDs.Tables.Add(dt)
                ElseIf dataSetName.XFEqualsIgnoreCase("GetDatabaseConnections") OrElse
                       String.IsNullOrEmpty(dataSetName) Then
                    ' da_DataSource_List path (empty substvar or explicit name)
                    Dim dt As DataTable = GetDatabaseConnections(si)
                    resultDs.Tables.Add(dt)
                Else
                    ' Unknown DataSetName — attempt substvar-based fallback
                    Dim hasSqlQuery As Boolean = csvars.ContainsKey("SQL_QUERY")
                    Dim hasChunkCount As Boolean = csvars.ContainsKey("SQL_CHUNK_COUNT")
                    Dim hasConnectionName As Boolean = csvars.ContainsKey("ConnectionName")

                    If hasSqlQuery OrElse hasChunkCount Then
                        Dim dt As DataTable = ExecuteSQL(si, csvars)
                        resultDs.Tables.Add(dt)
                    ElseIf hasConnectionName Then
                        Dim dt As DataTable = GetDatabaseSchema(si, csvars)
                        resultDs.Tables.Add(dt)
                    Else
                        Dim dt As DataTable = GetDatabaseConnections(si)
                        resultDs.Tables.Add(dt)
                    End If
                End If

            Catch ex As Exception
                ' Return an error table named QUERY_RESULTS so the client always gets a DataSet
                Dim errorTable As New DataTable("QUERY_RESULTS")
                errorTable.Columns.Add("Error", GetType(String))
                Dim row As DataRow = errorTable.NewRow()
                Dim errMsg As String = "SQL_Data error"
                errMsg &= " [" & ex.GetType().Name & "]"
                errMsg &= ": " & If(String.IsNullOrEmpty(ex.Message), "(no message)", ex.Message)
                If ex.InnerException IsNot Nothing Then
                    errMsg &= " --> Inner: [" & ex.InnerException.GetType().Name & "]: " & ex.InnerException.Message
                End If
                errMsg &= " | DataSetName=" & If(args IsNot Nothing AndAlso args.DataSetName IsNot Nothing, args.DataSetName, "(null)")
                errMsg &= " | SubstVarKeys=" & String.Join(";", csvars.Keys)
                row("Error") = errMsg
                errorTable.Rows.Add(row)
                resultDs.Tables.Add(errorTable)
            End Try

            Return resultDs
        End Function

        ' ── ExecuteSQL ──────────────────────────────────────────────────────────
        ' Executes a SQL query against the specified connection.
        ' Supports both short (SQL_QUERY) and chunked (SQL_CHUNK_COUNT) payloads.
        ' Returns DataTable named QUERY_RESULTS.
        Private Function ExecuteSQL(si As SessionInfo, csvars As Dictionary(Of String, String)) As DataTable
            Dim dt As New DataTable("QUERY_RESULTS")
            Dim diagStep As String = "init"

            Try
                ' Parse ConnectionName
                diagStep = "parseConnName"
                Dim connectionName As String = csvars.XFGetValue("ConnectionName")
                If String.IsNullOrWhiteSpace(connectionName) Then
                    connectionName = "OneStream Database Server"
                End If

                ' Parse SQL query — short or chunked
                diagStep = "parseQuery"
                Dim sqlQuery As String = String.Empty
                Dim chunkCountStr As String = csvars.XFGetValue("SQL_CHUNK_COUNT")
                If Not String.IsNullOrWhiteSpace(chunkCountStr) Then
                    Dim chunkCount As Integer = 0
                    Integer.TryParse(chunkCountStr, chunkCount)
                    Dim sb As New StringBuilder()
                    For i As Integer = 0 To chunkCount - 1
                        Dim chunk As String = csvars.XFGetValue("SQL_CHUNK_" & i.ToString())
                        sb.Append(HttpUtility.UrlDecode(chunk))
                    Next
                    sqlQuery = sb.ToString()
                Else
                    sqlQuery = HttpUtility.UrlDecode(csvars.XFGetValue("SQL_QUERY"))
                End If

                If String.IsNullOrWhiteSpace(sqlQuery) Then
                    Throw New Exception("SQL query is empty.")
                End If

                diagStep = "resolveConn:" & connectionName
                Dim dbConn As DbConnInfo = ResolveDbConnInfo(si, connectionName)
                Try
                    diagStep = "execQuery:" & sqlQuery.Substring(0, Math.Min(80, sqlQuery.Length))
                    dt = BRApi.Database.ExecuteSqlUsingReader(dbConn, sqlQuery, False)
                    dt.TableName = "QUERY_RESULTS"
                    Return dt
                Finally
                    If dbConn IsNot Nothing Then dbConn.Close()
                End Try

            Catch ex As Exception
                Throw New Exception("ExecuteSQL failed at [" & diagStep & "]: " & ex.GetType().Name & ": " & ex.Message, ex)
            End Try
        End Function

        ' ── GetDatabaseSchema ────────────────────────────────────────────────────
        ' Returns table/view/column metadata for the specified connection.
        ' Uses INFORMATION_SCHEMA queries via BRApi.Database.ExecuteSql.
        ' Returns DataTable named SCHEMA_RESULTS.
        Private Function GetDatabaseSchema(si As SessionInfo, csvars As Dictionary(Of String, String)) As DataTable
            Dim dt As New DataTable("SCHEMA_RESULTS")
            dt.Columns.Add("TABLE_NAME", GetType(String))
            dt.Columns.Add("TABLE_TYPE", GetType(String))
            dt.Columns.Add("COLUMN_NAME", GetType(String))
            dt.Columns.Add("DATA_TYPE", GetType(String))
            dt.Columns.Add("IS_NULLABLE", GetType(String))
            dt.Columns.Add("ORDINAL_POSITION", GetType(Integer))

            Dim connectionName As String = csvars.XFGetValue("ConnectionName")
            If String.IsNullOrWhiteSpace(connectionName) Then
                connectionName = "OneStream Database Server"
            End If

            Dim dbConn As DbConnInfo = ResolveDbConnInfo(si, connectionName)
            Try

            ' Get tables and views
            Dim tablesSql As String =
                "SELECT TABLE_NAME, TABLE_TYPE " &
                "FROM INFORMATION_SCHEMA.TABLES " &
                "WHERE TABLE_TYPE IN ('BASE TABLE', 'VIEW') " &
                "ORDER BY TABLE_NAME"
            Dim tablesResult As DataTable = BRApi.Database.ExecuteSql(dbConn, tablesSql, False)
            If tablesResult IsNot Nothing Then
                For Each srcRow As DataRow In tablesResult.Rows
                    Dim row As DataRow = dt.NewRow()
                    row("TABLE_NAME") = srcRow("TABLE_NAME")
                    row("TABLE_TYPE") = srcRow("TABLE_TYPE")
                    row("COLUMN_NAME") = DBNull.Value
                    row("DATA_TYPE") = DBNull.Value
                    row("IS_NULLABLE") = DBNull.Value
                    row("ORDINAL_POSITION") = 0
                    dt.Rows.Add(row)
                Next
            End If

            ' Get columns
            Dim columnsSql As String =
                "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE, ORDINAL_POSITION " &
                "FROM INFORMATION_SCHEMA.COLUMNS " &
                "ORDER BY TABLE_NAME, ORDINAL_POSITION"
            Dim colsResult As DataTable = BRApi.Database.ExecuteSql(dbConn, columnsSql, False)
            If colsResult IsNot Nothing Then
                For Each srcRow As DataRow In colsResult.Rows
                    Dim row As DataRow = dt.NewRow()
                    row("TABLE_NAME") = srcRow("TABLE_NAME")
                    row("TABLE_TYPE") = "COLUMN"
                    row("COLUMN_NAME") = srcRow("COLUMN_NAME")
                    row("DATA_TYPE") = srcRow("DATA_TYPE")
                    row("IS_NULLABLE") = srcRow("IS_NULLABLE")
                    row("ORDINAL_POSITION") = srcRow("ORDINAL_POSITION")
                    dt.Rows.Add(row)
                Next
            End If

            Finally
                If dbConn IsNot Nothing Then dbConn.Close()
            End Try

            Return dt
        End Function

        ' ── GetDatabaseConnections ───────────────────────────────────────────────
        ' Returns available database connections for this application.
        ' Always includes Application and Framework databases.
        ' Queries the external connections table for any user-configured connections.
        ' Returns DataTable named OCE_DATASOURCES.
        Private Function GetDatabaseConnections(si As SessionInfo) As DataTable
            Dim dt As New DataTable("OCE_DATASOURCES")
            dt.Columns.Add("ConnectionName", GetType(String))
            dt.Columns.Add("ApplicationName", GetType(String))
            dt.Columns.Add("IsApplicationDB", GetType(Boolean))
            dt.Columns.Add("Description", GetType(String))

            ' Always include the application database
            Dim appRow As DataRow = dt.NewRow()
            appRow("ConnectionName") = "OneStream Database Server"
            appRow("ApplicationName") = si.AppToken.AppName
            appRow("IsApplicationDB") = True
            appRow("Description") = si.AppToken.AppName & " - Application Database"
            dt.Rows.Add(appRow)

            ' Include Framework database
            Dim fwRow As DataRow = dt.NewRow()
            fwRow("ConnectionName") = "Framework"
            fwRow("ApplicationName") = si.AppToken.AppName
            fwRow("IsApplicationDB") = False
            fwRow("Description") = "OneStream Framework Database"
            dt.Rows.Add(fwRow)

            ' Query external connections from the framework database
            ' The ExternalDbConnection table stores user-configured DB connections.
            Dim dbConnFW As DbConnInfo = Nothing
            Try
                dbConnFW = BRApi.Database.CreateFrameworkDbConnInfo(si)
                Dim extSql As String = "SELECT Name, Description FROM ExternalDbConnection WHERE AppUniqueID = '" & si.AppToken.AppUniqueID.ToString() & "'"
                Dim extResult As DataTable = BRApi.Database.ExecuteSql(dbConnFW, extSql, False)
                If extResult IsNot Nothing Then
                    For Each extRow As DataRow In extResult.Rows
                        Dim connName As String = extRow("Name").ToString()
                        If Not String.IsNullOrWhiteSpace(connName) Then
                            Dim r As DataRow = dt.NewRow()
                            r("ConnectionName") = connName
                            r("ApplicationName") = si.AppToken.AppName
                            r("IsApplicationDB") = False
                            Dim desc As String = If(extRow("Description") IsNot DBNull.Value, extRow("Description").ToString(), connName)
                            r("Description") = If(Not String.IsNullOrWhiteSpace(desc), desc, connName)
                            dt.Rows.Add(r)
                        End If
                    Next
                End If
            Catch
                ' External connections query failed — application and framework rows are sufficient
            Finally
                If dbConnFW IsNot Nothing Then dbConnFW.Close()
            End Try

            Return dt
        End Function

        ' ── Helper: ResolveDbConnInfo ────────────────────────────────────────────
        ' Creates the appropriate DbConnInfo based on connection name.
        ' "Framework" → CreateFrameworkDbConnInfo
        ' "OneStream Database Server" or default → CreateApplicationDbConnInfo
        ' Anything else → CreateExternalDbConnInfo (named external connection)
        Private Function ResolveDbConnInfo(si As SessionInfo, connectionName As String) As DbConnInfo
            If String.IsNullOrWhiteSpace(connectionName) OrElse
               connectionName.Equals("OneStream Database Server", StringComparison.OrdinalIgnoreCase) Then
                Return BRApi.Database.CreateApplicationDbConnInfo(si)
            ElseIf connectionName.Equals("Framework", StringComparison.OrdinalIgnoreCase) Then
                Return BRApi.Database.CreateFrameworkDbConnInfo(si)
            Else
                Return BRApi.Database.CreateExternalDbConnInfo(si, connectionName)
            End If
        End Function

    End Class

End Namespace
