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

Namespace OneStream.BusinessRule.Extender.UTI_RES_Services_HelperQueries
    Public Class MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Try
                Select Case args.FunctionType

                    Case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
                        Dim paramFunction As String = args.NameValuePairs("p_function")

                        If paramFunction = "ClearFactIntersection" Then
                            ' Leer y validar parámetros obligatorios
                            Dim paramEntity As String = args.NameValuePairs("p_entity")
                            Dim paramScenario As String = args.NameValuePairs("p_scenario")
                            Dim paramMonthStr As String = args.NameValuePairs("p_month")
                            Dim paramYearStr As String = args.NameValuePairs("p_year")

                            If String.IsNullOrWhiteSpace(paramEntity) Then
                                Throw New XFException(si, New Exception("El parámetro 'p_entity' es obligatorio."))
                            End If
                            If String.IsNullOrWhiteSpace(paramScenario) Then
                                Throw New XFException(si, New Exception("El parámetro 'p_scenario' es obligatorio."))
                            End If
                            If String.IsNullOrWhiteSpace(paramMonthStr) Then
                                Throw New XFException(si, New Exception("El parámetro 'p_month' es obligatorio."))
                            End If
                            If String.IsNullOrWhiteSpace(paramYearStr) Then
                                Throw New XFException(si, New Exception("El parámetro 'p_year' es obligatorio."))
                            End If

                            Dim paramMonth As Integer
                            Dim paramYear As Integer

                            If Not Integer.TryParse(paramMonthStr, paramMonth) Then
                                Throw New XFException(si, New Exception("El parámetro 'p_month' debe ser un número."))
                            End If

                            If Not Integer.TryParse(paramYearStr, paramYear) Then
                                Throw New XFException(si, New Exception("El parámetro 'p_year' debe ser un número."))
                            End If

                            ' Escape de comillas simples para evitar errores SQL
                            Dim cleanEntity As String = paramEntity.Replace("'", "''")
                            Dim cleanScenario As String = paramScenario.Replace("'", "''")

                            ' Construcción de SQL embebida
                            Dim sqlDeleteWorkOrder As String = $"
                                DELETE FROM XFC_RES_FACT_work_order_figures
                                WHERE entity = '{cleanEntity}'
                                  AND scenario = '{cleanScenario}'
                                  AND year = {paramYear}
                                  AND month = {paramMonth};
                            "

                            Dim sqlDeleteStructure As String = $"
                                DELETE FROM XFC_RES_FACT_structure_figures
                                WHERE entity = '{cleanEntity}'
                                  AND scenario = '{cleanScenario}'
                                  AND year = {paramYear}
                                  AND month = {paramMonth};
                            "

                            ' Ejecutar SQL
                            Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                                BRApi.Database.ExecuteSql(dbConn, sqlDeleteWorkOrder, True)
                                BRApi.Database.ExecuteSql(dbConn, sqlDeleteStructure, True)
                            End Using
							
						ElseIf paramFunction = "CompleteWorkflowSteps" Then
						
						    ' 1) Leer/validar parámetros
						    Dim profileName As String = If(args.NameValuePairs.ContainsKey("p_profile"), args.NameValuePairs("p_profile"), Nothing)
						    Dim scenarioName As String = If(args.NameValuePairs.ContainsKey("p_scenario"), args.NameValuePairs("p_scenario"), Nothing)
						    Dim periodEnd   As String = If(args.NameValuePairs.ContainsKey("p_period"), args.NameValuePairs("p_period"), Nothing)
						
						    If String.IsNullOrWhiteSpace(profileName) Then Throw New XFException(si, New Exception("Missing 'p_profile'."))
						    If String.IsNullOrWhiteSpace(scenarioName) Then Throw New XFException(si, New Exception("Missing 'p_scenario'."))
						    If String.IsNullOrWhiteSpace(periodEnd)   Then Throw New XFException(si, New Exception("Missing 'p_period'."))
						
						    ' 2) Parsear p_period en formato OneStream: YYYYM1..YYYYM12
						    Dim mIdx As Integer = periodEnd.IndexOf("M"c)
						    If mIdx <> 4 Then Throw New XFException(si, New Exception("Invalid 'p_period'. Use 'YYYYM#' (eg. 2025M9 o 2025M12)."))
						
						    Dim yearPart As String = periodEnd.Substring(0, 4)
						    Dim monthPart As String = periodEnd.Substring(5).Trim()
						
						    Dim y As Integer, mEnd As Integer
						    If Not Integer.TryParse(yearPart, y) Then Throw New XFException(si, New Exception("Invalid Year in 'p_period'."))
						    If Not Integer.TryParse(monthPart, mEnd) OrElse mEnd < 1 OrElse mEnd > 12 Then
						        Throw New XFException(si, New Exception("Invalid month in 'p_period' (1..12)."))
						    End If
						
						    ' 3) Steps
						    Dim stepsOrdered As StepClassificationTypes() = {
						        StepClassificationTypes.DataLoadTransform,
						        StepClassificationTypes.ValidateTransform,
						        StepClassificationTypes.ValidateIntersection,
						        StepClassificationTypes.Workspace,
						        StepClassificationTypes.LoadCube,
						        StepClassificationTypes.ProcessCube,
						        StepClassificationTypes.Confirm
						    }
						
						    ' 4) Ejecutar meses en orden ascendente M1..Mx y steps en orden fijo.
						    For mm As Integer = 1 To mEnd
						        Dim timeName As String = $"{y}M{mm}"
						
						        ' Resolver Workflow Cluster para este periodo
						        Dim wfClusterPK As WorkflowUnitClusterPk =
						            BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, profileName, scenarioName, timeName)
						
						        ' Steps en orden
						        For Each stepType As StepClassificationTypes In stepsOrdered
						            If stepType = StepClassificationTypes.Confirm Then
						                ' Ejecuta la confirmación.
						                BRApi.DataQuality.Process.ExecuteConfirmation(si, wfClusterPK)
						            Else
						                ' Marca el step como completado
						                BRApi.Workflow.Status.SetWorkflowStatus(
						                    si,
						                    wfClusterPK,
						                    stepType,
						                    WorkflowStatusTypes.Completed,
						                    $"{stepType} Completed",
						                    "",
						                    "Admin complete workflow",
						                    Guid.Empty
						                )
						            End If
						        Next
						    Next
						End If

                End Select

                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

    End Class
End Namespace