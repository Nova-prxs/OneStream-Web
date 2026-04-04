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

Namespace OneStream.BusinessRule.Extender.Load_FxRate
    Public Class MainClass

        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
            Try
                Select Case args.FunctionType

                    Case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
                        Dim paramFunction As String = args.NameValuePairs("p_function")
                        brapi.ErrorLog.LogMessage(si, $"Function called: {paramFunction}")

                        If paramFunction = "SetFxRates" Then
                            brapi.ErrorLog.LogMessage(si, "SetFxRates started")

                            Dim paramTime As String = args.NameValuePairs("p_time")
                            Dim paramDefaultCurrency As String = args.NameValuePairs("p_default_currency")
                            Dim paramEntity As String = args.NameValuePairs("p_entity")

                            brapi.ErrorLog.LogMessage(si, $"Params - time: {paramTime}, default currency: {paramDefaultCurrency}, entity: {paramEntity}")

							' Obtener el miembro de la entidad
							Dim entityMbr As MemberInfo = BRApi.Finance.Metadata.GetMember(si, 0, paramEntity)
							Dim entityId As Integer = entityMbr.Member.MemberId
							' Obtener la moneda local de esa entidad
							Dim entityCurrency As String = BRApi.Finance.Entity.GetLocalCurrency(si,entityId).Name
							
							brapi.ErrorLog.LogMessage(si, $"Entity '{paramEntity}' has local currency: {entityCurrency}")

                            Dim fxRateTypeList As New List(Of String) From {"SERV_AverageRate"}

                            ' Crear DataTable en memoria
                            Dim fxRatesDataTable As New DataTable()
                            fxRatesDataTable.Columns.Add("time")
                            fxRatesDataTable.Columns.Add("type")
                            fxRatesDataTable.Columns.Add("source")
                            fxRatesDataTable.Columns.Add("target")
                            fxRatesDataTable.Columns.Add("rate")

                            Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                                For Each fxRateType As String In fxRateTypeList
                                    brapi.ErrorLog.LogMessage(si, $"Getting rate: Type={fxRateType}, Time={paramTime}, Source={paramDefaultCurrency}, Target={entityCurrency}")

                                    Dim fxRatePkUsingNames As New FxRatePkUsingNames(fxRateType, paramTime, paramDefaultCurrency, entityCurrency)
                                    Dim objFxRateUsingNames As FxRateUsingNames = BRApi.Finance.Data.GetStoredFxRate(si, fxRatePkUsingNames)
                                    Dim rate As Decimal = objFxRateUsingNames.Amount

                                    If rate > 0 Then
                                        brapi.ErrorLog.LogMessage(si, $"Rate found: {rate}")

                                        Dim rowFx As DataRow = fxRatesDataTable.NewRow()
                                        rowFx("time") = paramTime
                                        rowFx("type") = fxRateType
                                        rowFx("source") = paramDefaultCurrency
                                        rowFx("target") = entityCurrency
                                        rowFx("rate") = rate
                                        fxRatesDataTable.Rows.Add(rowFx)
                                    Else
                                        brapi.ErrorLog.LogMessage(si, $"Rate not found or zero for: {paramDefaultCurrency} to {entityCurrency}")
                                    End If
                                Next

                                ' MERGE fila por fila
                                For Each row As DataRow In fxRatesDataTable.Rows
                                    Dim mergeSql As String = $"
                                        MERGE INTO XFC_RES_FxRate AS target
                                        USING (SELECT '{row("time")}' AS [time], '{row("type")}' AS [type], '{row("source")}' AS [source], '{row("target")}' AS [target]) AS source
                                        ON target.[time] = source.[time]
                                        AND target.type = source.type
                                        AND target.source = source.source
                                        AND target.target = source.target
                                        WHEN MATCHED THEN
                                            UPDATE SET rate = {row("rate")}
                                        WHEN NOT MATCHED THEN
                                            INSERT ([time], type, source, target, rate)
                                            VALUES ('{row("time")}', '{row("type")}', '{row("source")}', '{row("target")}', {row("rate")});
                                    "
                                    brapi.ErrorLog.LogMessage(si, $"Executing SQL for: {row("source")} to {row("target")}")
                                    BRApi.Database.ExecuteSql(dbConn, mergeSql, False)
                                Next

                                brapi.ErrorLog.LogMessage(si, "FxRates completed successfully")
                            End Using
                        End If
                End Select

                Return Nothing
            Catch ex As Exception
                brapi.ErrorLog.LogMessage(si, $"Error: {ex.Message}")
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
    End Class
End Namespace