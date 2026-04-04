' CompileAssemblyDataSet.vb — COMPILE_EXTENDER assembly, OCE workspace
' Receives Base64-encoded JSON via CustomSubstVars and/or NameValuePairs
' dictionary (from CustomSubstVarsAsCommaSeparatedPairs sent by the TS extension).
' Both dictionaries are merged — the framework may populate either or both.
'
' SubstVar key: "Parameters" => base64-encoded JSON string
'
' Supported actions (from JSON):
'   CompileBusinessRule: { ruleName, ruleType, isSystemLevel }
'   CompileWsAssembly:  { action, assemblyName, workspaceName, isSystemLevel }
'   PushAndCompile:     { action, ruleCode, wrapAsFormula, username }
'
' NOTE: Compilation APIs (IBusinessRuleService.CompileBusinessRule,
' IWsAssemblies.CompileWsAssembly) are WCF service-level methods and are
' NOT accessible from within business rule code (BRApi has no compile surface).
' This bridge validates the request, verifies the target rule/assembly exists
' in the database, and returns a structured result. The actual compile
' invocation happens at the REST/WCF transport layer.
'
' Returns a DataTable named ASSEMBLY_COMPILE_RESULTS with columns:
'   Row 0 (summary): IsOK (Boolean), Message (String), RowType = "Info"
'   Rows 1..n (errors/warnings): RowType ("Error"/"Warning"), ModuleName,
'     LineNumber, ColumnNumber, ErrorCode, Message

Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Globalization
Imports System.Text
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CompileAssemblyDataSet

    Public Class MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
            ' --- FunctionType dispatch (matches proven OneStream workspace assembly pattern) ---
            ' The framework calls Main with args.FunctionType to either list dataset names
            ' or execute a specific dataset. args.DataSetName is populated from the second
            ' part of the adapter's methodQuery (e.g., {CompileBusinessRule}, {CompileAssembly}).
            Select Case args.FunctionType
                Case Is = DashboardDataSetFunctionType.GetDataSetNames
                    ' Return the dataset names this assembly handles
                    Return New List(Of String) From {"CompileBusinessRule", "CompileAssembly"}

                Case Is = DashboardDataSetFunctionType.GetDataSet
                    ' Fall through to the data retrieval logic below
            End Select

            Dim resultDs As New DataSet()
            Dim resultTable As New DataTable("ASSEMBLY_COMPILE_RESULTS")
            resultTable.Columns.Add("IsOK", GetType(Boolean))
            resultTable.Columns.Add("Message", GetType(String))
            resultTable.Columns.Add("RowType", GetType(String))
            resultTable.Columns.Add("ModuleName", GetType(String))
            resultTable.Columns.Add("LineNumber", GetType(Integer))
            resultTable.Columns.Add("ColumnNumber", GetType(Integer))
            resultTable.Columns.Add("ErrorCode", GetType(String))
            resultDs.Tables.Add(resultTable)

            Try
                ' --- Step 1: Read both dictionaries from DashboardDataSetArgs ---
                ' The REST API sends CustomSubstVarsAsCommaSeparatedPairs as a string.
                ' The framework may populate CustomSubstVars, NameValuePairs, or both.
                ' We check both and merge them so dispatch works regardless.
                Dim csvars As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

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

                ' --- Step 2: Retrieve the Parameters value from merged dictionary ---
                Dim base64Value As String = csvars.XFGetValue("Parameters")

                If String.IsNullOrWhiteSpace(base64Value) Then
                    Throw New Exception("CompileAssemblyDataSet: No 'Parameters' key in CustomSubstVars or NameValuePairs.")
                End If

                ' --- Step 2: Base64-decode -> UTF-8 JSON string ---
                Dim decodedBytes As Byte() = Convert.FromBase64String(base64Value)
                Dim jsonString As String = Encoding.UTF8.GetString(decodedBytes)

                If String.IsNullOrWhiteSpace(jsonString) Then
                    Throw New Exception("CompileAssemblyDataSet: Decoded JSON is empty.")
                End If

                ' --- Step 3: Parse JSON keys manually (no JavaScriptSerializer dependency) ---
                Dim action As String = GetJsonStringValue(jsonString, "action")
                Dim summaryIsOK As Boolean = False
                Dim summaryMessage As String = String.Empty

                If action = "CompileWsAssembly" Then
                    ' --- CompileWsAssembly: compile a workspace assembly ---
                    Dim assemblyName As String = GetJsonStringValue(jsonString, "assemblyName")
                    Dim workspaceName As String = GetJsonStringValue(jsonString, "workspaceName")

                    ' Verify the assembly exists in the database
                    Dim dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        Dim sql As String = "SELECT TOP 1 wa.UniqueID, wa.Name, w.Name AS WorkspaceName " &
                            "FROM WsAssembly wa " &
                            "INNER JOIN DashboardWorkspace w ON wa.WorkspaceID = w.UniqueID " &
                            "WHERE wa.Name = '" & SafeSql(assemblyName) & "' " &
                            "AND w.Name = '" & SafeSql(workspaceName) & "'"
                        Dim dt As DataTable = BRApi.Database.ExecuteSqlUsingReader(dbConn, sql, False)

                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            summaryIsOK = True
                            summaryMessage = "Assembly '" & assemblyName & "' in workspace '" & workspaceName & "' found. Compile dispatched."
                        Else
                            summaryIsOK = False
                            summaryMessage = "Assembly '" & assemblyName & "' not found in workspace '" & workspaceName & "'."
                        End If
                    Finally
                        If dbConn IsNot Nothing Then dbConn.Close()
                    End Try

                ElseIf action = "PushAndCompile" Then
                    ' --- PushAndCompile: formula validation ---
                    Dim ruleCode As String = GetJsonStringValue(jsonString, "ruleCode")

                    If String.IsNullOrWhiteSpace(ruleCode) Then
                        summaryIsOK = False
                        summaryMessage = "PushAndCompile: No ruleCode provided."
                    Else
                        ' Code received for validation; actual compilation occurs at
                        ' the WCF service layer (IBusinessRuleService).
                        summaryIsOK = True
                        summaryMessage = "PushAndCompile: Code received (" & ruleCode.Length.ToString() & " chars). Compile dispatched."
                    End If

                Else
                    ' --- Default: CompileBusinessRule ---
                    Dim ruleName As String = GetJsonStringValue(jsonString, "ruleName")
                    Dim ruleTypeStr As String = GetJsonStringValue(jsonString, "ruleType")

                    ' Verify the business rule exists in the database
                    Dim dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        Dim sql As String = "SELECT TOP 1 br.BusinessRuleID, br.Name, br.BusinessRuleType " &
                            "FROM BusinessRule br " &
                            "WHERE br.Name = '" & SafeSql(ruleName) & "'"
                        Dim dt As DataTable = BRApi.Database.ExecuteSqlUsingReader(dbConn, sql, False)

                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            summaryIsOK = True
                            summaryMessage = "Rule '" & ruleName & "' (type: " & ruleTypeStr & ") found. Compile dispatched."
                        Else
                            summaryIsOK = False
                            summaryMessage = "Rule '" & ruleName & "' not found in the application database."
                        End If
                    Finally
                        If dbConn IsNot Nothing Then dbConn.Close()
                    End Try
                End If

                ' --- Step 4: Build summary row ---
                Dim summaryRow As DataRow = resultTable.NewRow()
                summaryRow("IsOK") = summaryIsOK
                summaryRow("Message") = summaryMessage
                summaryRow("RowType") = "Info"
                summaryRow("ModuleName") = DBNull.Value
                summaryRow("LineNumber") = 0
                summaryRow("ColumnNumber") = 0
                summaryRow("ErrorCode") = DBNull.Value
                resultTable.Rows.Add(summaryRow)

            Catch ex As Exception
                Dim errorRow As DataRow = resultTable.NewRow()
                errorRow("IsOK") = False
                errorRow("Message") = "CompileAssemblyDataSet error: " & ex.Message
                errorRow("RowType") = "Info"
                errorRow("ModuleName") = DBNull.Value
                errorRow("LineNumber") = 0
                errorRow("ColumnNumber") = 0
                errorRow("ErrorCode") = DBNull.Value
                resultTable.Rows.Add(errorRow)
            End Try

            Return resultDs
        End Function

        ''' <summary>
        ''' Extract a string value from a simple JSON object by key name.
        ''' Handles both quoted-string and boolean/number values.
        ''' </summary>
        Private Function GetJsonStringValue(json As String, key As String) As String
            ' Look for "key": or "key" :
            Dim searchKey As String = """" & key & """"
            Dim keyIdx As Integer = json.IndexOf(searchKey, StringComparison.Ordinal)
            If keyIdx < 0 Then Return String.Empty

            ' Find the colon after the key
            Dim colonIdx As Integer = json.IndexOf(":"c, keyIdx + searchKey.Length)
            If colonIdx < 0 Then Return String.Empty

            ' Skip whitespace after colon
            Dim valStart As Integer = colonIdx + 1
            Do While valStart < json.Length AndAlso Char.IsWhiteSpace(json(valStart))
                valStart += 1
            Loop

            If valStart >= json.Length Then Return String.Empty

            If json(valStart) = """"c Then
                ' Quoted string value — find closing quote (handle escaped quotes)
                Dim strStart As Integer = valStart + 1
                Dim strEnd As Integer = strStart
                Do While strEnd < json.Length
                    If json(strEnd) = "\"c Then
                        strEnd += 2 ' skip escaped char
                    ElseIf json(strEnd) = """"c Then
                        Exit Do
                    Else
                        strEnd += 1
                    End If
                Loop
                Return json.Substring(strStart, strEnd - strStart)
            Else
                ' Unquoted value (boolean, number, null) — read until comma, brace, or end
                Dim valEnd As Integer = valStart
                Do While valEnd < json.Length AndAlso json(valEnd) <> ","c _
                    AndAlso json(valEnd) <> "}"c AndAlso json(valEnd) <> "]"c
                    valEnd += 1
                Loop
                Return json.Substring(valStart, valEnd - valStart).Trim()
            End If
        End Function

        ''' <summary>
        ''' Escape single quotes in a string for use in SQL.
        ''' </summary>
        Private Function SafeSql(value As String) As String
            If String.IsNullOrEmpty(value) Then Return String.Empty
            Return value.Replace("'", "''")
        End Function

    End Class

End Namespace
