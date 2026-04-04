' MetadataImporter.vb — EXTENDER assembly, OCE workspace
' Invoked via the da_MetadataImport adapter (GetAdoDataSetForAdapter).
' Receives Base64-encoded metadata XML, parses it, and writes
' business rules and workspace assembly files to the database.
'
' CustomSubstVars / NameValuePairs keys:
'   Base64Xml   — Base64-encoded UTF-8 XML string
'   LogDetails  — 'true' | 'false' (optional, controls verbose logging)
'
' Returns DataTable IMPORT_RESULTS with columns:
'   IsOK (Boolean), Message (String), UpdatedCount (Integer), RootElement (String)

Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Globalization
Imports System.Text
Imports System.Xml
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.MetadataImporter

    Public Class MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
            Select Case args.FunctionType
                Case Is = DashboardDataSetFunctionType.GetDataSetNames
                    Return New List(Of String) From {"ImportMetadata"}
                Case Is = DashboardDataSetFunctionType.GetDataSet
            End Select

            Dim resultDs As New DataSet()
            Dim resultTable As New DataTable("IMPORT_RESULTS")
            resultTable.Columns.Add("IsOK", GetType(Boolean))
            resultTable.Columns.Add("Message", GetType(String))
            resultTable.Columns.Add("UpdatedCount", GetType(Integer))
            resultTable.Columns.Add("RootElement", GetType(String))
            resultDs.Tables.Add(resultTable)

            Try
                ' --- Read both dictionaries ---
                Dim csvars As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
                If args IsNot Nothing AndAlso args.CustomSubstVars IsNot Nothing Then
                    For Each kvp In args.CustomSubstVars
                        csvars(kvp.Key) = kvp.Value
                    Next
                End If
                If args IsNot Nothing AndAlso args.NameValuePairs IsNot Nothing Then
                    For Each kvp In args.NameValuePairs
                        If Not csvars.ContainsKey(kvp.Key) Then
                            csvars(kvp.Key) = kvp.Value
                        End If
                    Next
                End If

                ' --- Decode Base64 XML ---
                Dim base64Value As String = csvars.XFGetValue("Base64Xml")
                If String.IsNullOrWhiteSpace(base64Value) Then
                    Throw New Exception("'Base64Xml' not found in CustomSubstVars or NameValuePairs.")
                End If
                Dim decodedXml As String = Encoding.UTF8.GetString(Convert.FromBase64String(base64Value))
                If String.IsNullOrWhiteSpace(decodedXml) Then
                    Throw New Exception("Decoded XML is empty.")
                End If

                ' --- Parse XML ---
                Dim doc As New XmlDocument()
                doc.LoadXml(decodedXml)
                Dim rootElement As String = If(doc.DocumentElement IsNot Nothing, doc.DocumentElement.Name, "unknown")

                Dim verbose As Boolean = String.Equals(csvars.XFGetValue("LogDetails"), "true", StringComparison.OrdinalIgnoreCase)
                Dim updatedCount As Integer = 0
                Dim messages As New List(Of String)

                ' --- Import standalone business rules ---
                Dim ruleNodes As XmlNodeList = doc.SelectNodes("//businessRule")
                If ruleNodes IsNot Nothing AndAlso ruleNodes.Count > 0 Then
                    Dim dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        For Each ruleNode As XmlElement In ruleNodes
                            Dim ruleName As String = ruleNode.GetAttribute("name")
                            Dim ruleType As String = ruleNode.GetAttribute("businessRuleType")
                            Dim srcNode As XmlNode = ruleNode.SelectSingleNode("sourceCode")
                            If String.IsNullOrWhiteSpace(ruleName) OrElse srcNode Is Nothing Then Continue For

                            Dim sourceCode As String = srcNode.InnerText
                            Dim safeName As String = ruleName.Replace("'", "''")
                            Dim safeCode As String = sourceCode.Replace("'", "''")

                            ' Check if rule exists
                            Dim checkSql As String = "SELECT COUNT(*) AS Cnt FROM BusinessRule WHERE Name = '" & safeName & "'"
                            Dim checkDt As DataTable = BRApi.Database.ExecuteSqlUsingReader(dbConn, checkSql, False)
                            Dim exists As Boolean = False
                            If checkDt IsNot Nothing AndAlso checkDt.Rows.Count > 0 Then
                                exists = Convert.ToInt32(checkDt.Rows(0)("Cnt")) > 0
                            End If

                            If exists Then
                                Dim updateSql As String = "UPDATE BusinessRule SET SourceCode = N'" & safeCode & "' WHERE Name = '" & safeName & "'"
                                BRApi.Database.ExecuteSql(dbConn, updateSql, False)
                                updatedCount += 1
                                messages.Add("Updated rule: " & ruleName)
                            Else
                                messages.Add("Rule not found (skipped): " & ruleName & " — create it in OneStream first")
                            End If

                            If verbose Then
                                BRApi.ErrorLog.LogMessage(si, "MetadataImporter: " & If(exists, "Updated", "Skipped") & " rule '" & ruleName & "' (" & ruleType & "), " & sourceCode.Length.ToString() & " chars")
                            End If
                        Next
                    Finally
                        If dbConn IsNot Nothing Then dbConn.Close()
                    End Try
                End If

                ' --- Import workspace assembly files ---
                Dim asmNodes As XmlNodeList = doc.SelectNodes("//workspaceAssembly")
                If asmNodes IsNot Nothing AndAlso asmNodes.Count > 0 Then
                    Dim dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        For Each asmNode As XmlElement In asmNodes
                            Dim asmName As String = asmNode.GetAttribute("name")
                            If String.IsNullOrWhiteSpace(asmName) Then Continue For

                            Dim fileNodes As XmlNodeList = asmNode.SelectNodes(".//file")
                            If fileNodes Is Nothing Then Continue For

                            For Each fileNode As XmlElement In fileNodes
                                Dim fileName As String = fileNode.GetAttribute("name")
                                Dim srcNode As XmlNode = fileNode.SelectSingleNode("sourceCode")
                                If String.IsNullOrWhiteSpace(fileName) OrElse srcNode Is Nothing Then Continue For

                                Dim sourceCode As String = srcNode.InnerText
                                Dim safeFName As String = fileName.Replace("'", "''")
                                Dim safeAName As String = asmName.Replace("'", "''")
                                Dim safeCode As String = sourceCode.Replace("'", "''")

                                ' Update assembly file source code
                                Dim updateSql As String = "UPDATE waf SET waf.SourceCode = N'" & safeCode & "' " &
                                    "FROM WsAssemblyFile waf " &
                                    "INNER JOIN WsAssembly wa ON waf.WsAssemblyID = wa.UniqueID " &
                                    "WHERE waf.FileName = '" & safeFName & "' AND wa.Name = '" & safeAName & "'"
                                BRApi.Database.ExecuteSql(dbConn, updateSql, False)

                                ' Check if row was actually updated (verify file exists)
                                Dim checkSql As String = "SELECT COUNT(*) AS Cnt FROM WsAssemblyFile waf " &
                                    "INNER JOIN WsAssembly wa ON waf.WsAssemblyID = wa.UniqueID " &
                                    "WHERE waf.FileName = '" & safeFName & "' AND wa.Name = '" & safeAName & "'"
                                Dim checkDt As DataTable = BRApi.Database.ExecuteSqlUsingReader(dbConn, checkSql, False)
                                Dim fileExists As Boolean = False
                                If checkDt IsNot Nothing AndAlso checkDt.Rows.Count > 0 Then
                                    fileExists = Convert.ToInt32(checkDt.Rows(0)("Cnt")) > 0
                                End If

                                If fileExists Then
                                    updatedCount += 1
                                    messages.Add("Updated assembly file: " & asmName & "/" & fileName)
                                Else
                                    messages.Add("Assembly file not found (skipped): " & asmName & "/" & fileName)
                                End If

                                If verbose Then
                                    BRApi.ErrorLog.LogMessage(si, "MetadataImporter: " & If(fileExists, "Updated", "Skipped") & " assembly file '" & asmName & "/" & fileName & "', " & sourceCode.Length.ToString() & " chars")
                                End If
                            Next
                        Next
                    Finally
                        If dbConn IsNot Nothing Then dbConn.Close()
                    End Try
                End If

                ' --- Build result ---
                Dim row As DataRow = resultTable.NewRow()
                row("IsOK") = True
                row("Message") = If(updatedCount > 0,
                    updatedCount.ToString() & " item(s) updated. " & String.Join("; ", messages),
                    "No items to update. " & String.Join("; ", messages))
                row("UpdatedCount") = updatedCount
                row("RootElement") = rootElement
                resultTable.Rows.Add(row)

            Catch ex As Exception
                Dim errorRow As DataRow = resultTable.NewRow()
                errorRow("IsOK") = False
                errorRow("Message") = "MetadataImporter error: " & ex.Message
                errorRow("UpdatedCount") = 0
                errorRow("RootElement") = String.Empty
                resultTable.Rows.Add(errorRow)
            End Try

            Return resultDs
        End Function

    End Class

End Namespace
