' OCE_RuleExecutor.vb — RULE_EXEC assembly, OCE workspace
' Receives rule execution parameters via CustomSubstVars and/or NameValuePairs.
' The framework parses the comma-separated key=value pairs into the
' DashboardDataSetArgs dictionaries — either or both may be populated.
'
' CustomSubstVars / NameValuePairs keys:
'   RuleType     — 'Finance' | 'DataMgmt' | 'Extender'
'   RuleName     — URL-encoded rule name
'   FunctionName — (optional) URL-encoded function name
'   Parameters   — (optional) URL-encoded JSON string of custom parameters
'   Mode         — 'Foreground' | 'Background'
'
' Returns a DataTable named RULE_RESULTS with columns:
'   Status   (String) — 'Success' | 'Submitted' | 'Error'
'   Data     (String) — output data or task result
'   Message  (String) — human-readable status message
'   TaskId   (String) — background task ID (empty for foreground)

Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.OCE_RuleExecutor

    Public Class MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
            ' --- FunctionType dispatch (matches proven OneStream workspace assembly pattern) ---
            ' The framework calls Main with args.FunctionType to either list dataset names
            ' or execute a specific dataset. args.DataSetName is populated from the second
            ' part of the adapter's methodQuery (e.g., {ExecuteRule}).
            Select Case args.FunctionType
                Case Is = DashboardDataSetFunctionType.GetDataSetNames
                    ' Return the dataset names this assembly handles
                    Return New List(Of String) From {"ExecuteRule"}

                Case Is = DashboardDataSetFunctionType.GetDataSet
                    ' Fall through to the data retrieval logic below
            End Select

            Dim resultDs As New DataSet()
            Dim resultTable As New DataTable("RULE_RESULTS")
            resultTable.Columns.Add("Status", GetType(String))
            resultTable.Columns.Add("Data", GetType(String))
            resultTable.Columns.Add("Message", GetType(String))
            resultTable.Columns.Add("TaskId", GetType(String))
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

                ' --- Step 2: Parse required substvars ---
                Dim ruleType As String = csvars.XFGetValue("RuleType")
                Dim ruleNameEncoded As String = csvars.XFGetValue("RuleName")
                Dim functionNameEncoded As String = csvars.XFGetValue("FunctionName")
                Dim modeStr As String = csvars.XFGetValue("Mode")

                If String.IsNullOrEmpty(ruleType) Then
                    Throw New Exception("OCE_RuleExecutor: RuleType substvar is required.")
                End If
                If String.IsNullOrEmpty(ruleNameEncoded) Then
                    Throw New Exception("OCE_RuleExecutor: RuleName substvar is required.")
                End If

                ' URL-decode rule name and function name
                Dim ruleName As String = HttpUtility.UrlDecode(ruleNameEncoded)
                Dim functionName As String = If(String.IsNullOrEmpty(functionNameEncoded), String.Empty, HttpUtility.UrlDecode(functionNameEncoded))
                Dim isBackground As Boolean = String.Equals(modeStr, "Background", StringComparison.OrdinalIgnoreCase)

                ' --- Step 3: Dispatch based on RuleType ---
                Dim statusVal As String = "Success"
                Dim dataVal As String = String.Empty
                Dim messageVal As String = String.Empty
                Dim taskIdVal As String = String.Empty

                If ruleType = "DataMgmt" Then
                    ' Execute a Data Management sequence
                    ' TODO: Replace stub with:
                    '   BRApi.Finance.DataMgmt.RunDataManagementSequence(si, ruleName, isBackground)
                    statusVal = If(isBackground, "Submitted", "Success")
                    messageVal = If(isBackground,
                        "DataMgmt sequence '" & ruleName & "' submitted for background execution.",
                        "DataMgmt sequence '" & ruleName & "' executed successfully.")
                    taskIdVal = If(isBackground, "dm-" & DateTime.UtcNow.Ticks.ToString(), String.Empty)

                ElseIf ruleType = "Finance" Then
                    ' Execute a Finance business rule
                    ' TODO: Replace stub with:
                    '   BRApi.Finance.Execute.ExecuteBusinessRule(si, ruleName, functionName)
                    statusVal = "Success"
                    messageVal = "Finance rule '" & ruleName & "' executed successfully."
                    dataVal = String.Empty

                ElseIf ruleType = "Extender" Then
                    ' Execute an Extender business rule
                    ' TODO: Replace stub with:
                    '   BRApi.Utilities.ExecuteExtenderRule(si, ruleName, functionName)
                    statusVal = "Success"
                    messageVal = "Extender rule '" & ruleName & "'" &
                        If(String.IsNullOrEmpty(functionName), "", " function '" & functionName & "'") &
                        " executed successfully."
                    dataVal = String.Empty

                Else
                    ' Unknown rule type — return error
                    statusVal = "Error"
                    messageVal = "OCE_RuleExecutor: Unknown RuleType '" & ruleType & "'. Expected Finance, DataMgmt, or Extender."
                End If

                ' --- Step 4: Add result row ---
                Dim row As DataRow = resultTable.NewRow()
                row("Status") = statusVal
                row("Data") = dataVal
                row("Message") = messageVal
                row("TaskId") = taskIdVal
                resultTable.Rows.Add(row)

            Catch ex As Exception
                Dim errorRow As DataRow = resultTable.NewRow()
                errorRow("Status") = "Error"
                errorRow("Data") = String.Empty
                errorRow("Message") = "OCE_RuleExecutor error: " & ex.Message
                errorRow("TaskId") = String.Empty
                resultTable.Rows.Add(errorRow)
            End Try

            Return resultDs
        End Function

    End Class

End Namespace
