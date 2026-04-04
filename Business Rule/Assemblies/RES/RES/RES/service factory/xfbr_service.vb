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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class xfbr_service
		Implements IWsasXFBRStringV800

        Public Function GetXFBRString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
			ByVal args As DashboardStringFunctionArgs) As String Implements IWsasXFBRStringV800.GetXFBRString
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
                    If args.FunctionName.XFEqualsIgnoreCase("GetTemplateUploadWarnings") Then
                        Dim paramEntity As String = args.NameValuePairs("paramEntity")
                        Dim paramScenario As String = args.NameValuePairs("paramScenario")
                        Dim paramYear As Integer = CInt(args.NameValuePairs("paramYear"))
                        Dim importType As String = args.NameValuePairs("importType")
                        Dim templateType As String = args.NameValuePairs("templateType")
                        ' For Annual, we also need the FCT scenario to check current year data
                        Dim paramScenarioFct As String = ""
                        If args.NameValuePairs.ContainsKey("paramScenarioFct") Then
                            paramScenarioFct = args.NameValuePairs("paramScenarioFct")
                        End If

                        ' Determine the FACT table based on import type
                        Dim factTable As String = If(importType = "project",
                            "XFC_RES_FACT_work_order_figures",
                            "XFC_RES_FACT_structure_figures")

                        ' For Annual (BUD), the data adapter shows:
                        '   Current year with FCT scenario + BUD(year+1) + PLN1(year+2) + PLN2(year+3) + PLN3(year+4)
                        ' For Monthly (FCT), data is stored with the same scenario/year
                        Dim sql As String = $"
                            SELECT
                                (SELECT COUNT(*) FROM XFC_RES_AUX_template_warning
                                 WHERE entity = @paramEntity
                                   AND scenario = @paramScenario
                                   AND year = @paramYear
                                   AND import_type = @importType
                                   AND template_type = @templateType) AS warning_count,
                                (SELECT COUNT(*) FROM {factTable}
                                 WHERE entity = @paramEntity
                                   AND (
                                        (@paramScenario = 'BUD' AND (
                                            (@paramScenarioFct <> '' AND scenario = @paramScenarioFct AND year = @paramYear)
                                            OR (scenario = 'BUD' AND year = @paramYear + 1)
                                            OR (scenario = 'PLN1' AND year = @paramYear + 2)
                                            OR (scenario = 'PLN2' AND year = @paramYear + 3)
                                            OR (scenario = 'PLN3' AND year = @paramYear + 4)
                                        ))
                                        OR (@paramScenario <> 'BUD' AND scenario = @paramScenario AND year = @paramYear)
                                   )) AS data_count"

                        Dim dbParams As New List(Of DbParamInfo) From {
                            New DbParamInfo("paramEntity", paramEntity),
                            New DbParamInfo("paramScenario", paramScenario),
                            New DbParamInfo("paramYear", paramYear),
                            New DbParamInfo("importType", importType),
                            New DbParamInfo("templateType", templateType),
                            New DbParamInfo("paramScenarioFct", paramScenarioFct)
                        }

                        Dim dt As DataTable
                        Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                            dt = BRApi.Database.ExecuteSql(dbConn, sql, dbParams, False)
                        End Using

                        Dim warningCount As Integer = 0
                        Dim dataCount As Integer = 0
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            warningCount = CInt(dt.Rows(0)("warning_count"))
                            dataCount = CInt(dt.Rows(0)("data_count"))
                        End If

                        If warningCount > 0 Then
                            Return $"{warningCount} rows with warning please check Warning Table"
                        End If

                        If dataCount > 0 Then
                            Return "Import completed successfully"
                        End If

                        Return "Not uploaded yet"
                	End If

                    If args.FunctionName.XFEqualsIgnoreCase("GetWorkOrderUploadWarnings") Then
                        Dim paramEntity As String = args.NameValuePairs("paramEntity")

                        Dim sql As String = "
                            SELECT
                                (SELECT COUNT(*) FROM XFC_RES_AUX_template_warning
                                 WHERE entity = @paramEntity
                                   AND import_type = 'work_order') AS warning_count,
                                (SELECT COUNT(*) FROM XFC_RES_MASTER_work_order
                                 WHERE entity = @paramEntity) AS data_count"

                        Dim dbParams As New List(Of DbParamInfo) From {
                            New DbParamInfo("paramEntity", paramEntity)
                        }

                        Dim dt As DataTable
                        Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                            dt = BRApi.Database.ExecuteSql(dbConn, sql, dbParams, False)
                        End Using

                        Dim warningCount As Integer = 0
                        Dim dataCount As Integer = 0
                        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                            warningCount = CInt(dt.Rows(0)("warning_count"))
                            dataCount = CInt(dt.Rows(0)("data_count"))
                        End If

                        If warningCount > 0 Then
                            Return $"{warningCount} rows with warning please check Warning Table"
                        End If

                        If dataCount > 0 Then
                            Return "Import completed successfully"
                        End If

                        Return "Not uploaded yet"
                    End If

                    If args.FunctionName.XFEqualsIgnoreCase("GetSendToCubeConfirmMessage") Then
                        Dim paramScenario As String = args.NameValuePairs("paramScenario")
                        Dim paramYear As String = args.NameValuePairs("paramYear")
                        Return $"Are you sure you want to send data for {paramScenario} of {paramYear} to the Cube?"
                    End If
	            End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace
