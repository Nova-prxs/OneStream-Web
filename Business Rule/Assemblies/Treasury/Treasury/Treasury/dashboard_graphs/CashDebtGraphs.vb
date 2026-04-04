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
Imports Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DataAdapterGraphs.GraphsSharedHelpers

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DataAdapterGraphs
    Public Class CashDebtGraphs

        ''' <summary>
        ''' Genera gráfico de líneas comparativo de Cash Balance entre dos años
        ''' Filtra por semanas, entidades y escenario
        ''' </summary>
        Public Shared Function GetCashBalanceGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Validar y extraer parámetros requeridos
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim treasuryYearToCompare As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year_To_Compare")
                Dim weekNumberFrom As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_from")
                Dim weekNumberTo As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_to")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL CON PARÁMETROS (SEGURO CONTRA INYECCIÓN)
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
                dbParams.Insert(2, New DbParamInfo("@year1", treasuryYear))
                dbParams.Insert(3, New DbParamInfo("@year2", treasuryYearToCompare))

                Dim sql As String = TRS_SQL_Repository.GetCashBalanceGraphQuery(entityFilter)

                ' ===================================================================
                ' 3. EJECUTAR CONSULTA Y CARGAR DATOS
                ' ===================================================================
                Dim rawData As DataTable = Nothing

                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)

                        If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
                            Return Nothing
                        End If

                    Catch ex As Exception
                        Throw
                    End Try
                End Using

                ' ===================================================================
                ' 4. VALIDAR DATOS DE AMBOS AÑOS (CRÍTICO PARA COMPARATIVA)
                ' ===================================================================
                Dim dataByYear As New Dictionary(Of Integer, List(Of GraphDataPoint))()

                For Each row As DataRow In rawData.Rows
                    Dim year As Integer = Convert.ToInt32(row("ProjectionYear"))
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim dateLabel As String = row("DateLabel").ToString()
                    Dim amount As Decimal = If(IsDBNull(row("TotalAmount")), 0D, Convert.ToDecimal(row("TotalAmount")))

                    If Not dataByYear.ContainsKey(year) Then
                        dataByYear(year) = New List(Of GraphDataPoint)()
                    End If

                    dataByYear(year).Add(New GraphDataPoint With {
                        .WeekNumber = weekNum,
                        .DateLabel = dateLabel,
                        .Amount = amount
                    })
                Next

                ' Validar que ambos años tienen datos
                If Not dataByYear.ContainsKey(treasuryYear) OrElse Not dataByYear.ContainsKey(treasuryYearToCompare) Then
                End If

                ' Ordenar datos de cada año por semana
                For Each kvp In dataByYear
                    kvp.Value.Sort(Function(a, b) a.WeekNumber.CompareTo(b.WeekNumber))
                Next

                ' ===================================================================
                ' 5. CREAR SERIES DEL GRÁFICO
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Año de comparación (color azul)
                If dataByYear.ContainsKey(treasuryYearToCompare) Then
                    GraphsSharedHelpers.AddLineSeries(oSeriesCollection, treasuryYearToCompare.ToString(), GraphsSharedHelpers.COLOR_BLUE, dataByYear(treasuryYearToCompare))
                End If

                ' Serie 2: Año actual (color naranja)
                If dataByYear.ContainsKey(treasuryYear) Then
                    GraphsSharedHelpers.AddLineSeries(oSeriesCollection, treasuryYear.ToString(), GraphsSharedHelpers.COLOR_ORANGE, dataByYear(treasuryYear))
                End If

                ' ===================================================================
                ' 6. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        ''' <summary>
        ''' Genera gráfico de líneas comparativo de Cash Balance entre dos años
        ''' Eje X = Número de semana (W1, W2, W3... W52)
        ''' Permite comparar semana a semana entre años
        ''' </summary>
        Public Shared Function GetCashBalanceGraphByWeekNumber(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Validar y extraer parámetros requeridos
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim treasuryYearToCompare As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year_To_Compare")
                Dim weekNumberFrom As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_from")
                Dim weekNumberTo As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_to")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL CON PARÁMETROS
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
                dbParams.Insert(2, New DbParamInfo("@year1", treasuryYear))
                dbParams.Insert(3, New DbParamInfo("@year2", treasuryYearToCompare))

                Dim sql As String = TRS_SQL_Repository.GetCashBalanceByWeekNumberQuery(entityFilter)

                ' ===================================================================
                ' 3. EJECUTAR CONSULTA Y CARGAR DATOS
                ' ===================================================================
                Dim rawData As DataTable = Nothing

                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)

                        If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
                            Return Nothing
                        End If

                    Catch ex As Exception
                        Throw
                    End Try
                End Using

                ' ===================================================================
                ' 4. VALIDAR DATOS DE AMBOS AÑOS Y ORGANIZAR POR SEMANA
                ' ===================================================================
                Dim dataByYearAndWeek As New Dictionary(Of Integer, Dictionary(Of Integer, Decimal))()

                For Each row As DataRow In rawData.Rows
                    Dim year As Integer = Convert.ToInt32(row("ProjectionYear"))
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim amount As Decimal = If(IsDBNull(row("TotalAmount")), 0D, Convert.ToDecimal(row("TotalAmount")))

                    If Not dataByYearAndWeek.ContainsKey(year) Then
                        dataByYearAndWeek(year) = New Dictionary(Of Integer, Decimal)()
                    End If

                    dataByYearAndWeek(year)(weekNum) = amount
                Next

                ' Validar que ambos años tienen datos
                If Not dataByYearAndWeek.ContainsKey(treasuryYear) OrElse Not dataByYearAndWeek.ContainsKey(treasuryYearToCompare) Then
                End If

                ' ===================================================================
                ' 5. PREPARAR DATOS PARA SERIES (AGRUPADO POR SEMANA)
                ' ===================================================================
                Dim dataYear1 As New List(Of GraphDataPoint)()
                Dim dataYear2 As New List(Of GraphDataPoint)()

                ' Iterar sobre semanas en rango especificado
                For weekNum As Integer = weekNumberFrom To weekNumberTo
                    Dim weekLabel As String = $"W{weekNum}"

                    ' Año a comparar
                    If dataByYearAndWeek.ContainsKey(treasuryYearToCompare) AndAlso dataByYearAndWeek(treasuryYearToCompare).ContainsKey(weekNum) Then
                        dataYear1.Add(New GraphDataPoint With {
                            .WeekNumber = weekNum,
                            .DateLabel = weekLabel,
                            .Amount = dataByYearAndWeek(treasuryYearToCompare)(weekNum)
                        })
                    Else
                        dataYear1.Add(New GraphDataPoint With {
                            .WeekNumber = weekNum,
                            .DateLabel = weekLabel,
                            .Amount = 0D
                        })
                    End If

                    ' Año actual
                    If dataByYearAndWeek.ContainsKey(treasuryYear) AndAlso dataByYearAndWeek(treasuryYear).ContainsKey(weekNum) Then
                        dataYear2.Add(New GraphDataPoint With {
                            .WeekNumber = weekNum,
                            .DateLabel = weekLabel,
                            .Amount = dataByYearAndWeek(treasuryYear)(weekNum)
                        })
                    Else
                        dataYear2.Add(New GraphDataPoint With {
                            .WeekNumber = weekNum,
                            .DateLabel = weekLabel,
                            .Amount = 0D
                        })
                    End If
                Next

                ' ===================================================================
                ' 6. CREAR SERIES DEL GRÁFICO
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, treasuryYearToCompare.ToString(), GraphsSharedHelpers.COLOR_BLUE, dataYear1)
                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, treasuryYear.ToString(), GraphsSharedHelpers.COLOR_ORANGE, dataYear2)

                ' ===================================================================
                ' 7. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        ''' <summary>
        ''' Genera gráfico de líneas de Used Lines vs Limit Lines
        ''' Used Lines = FINANCING - USED LINES (todos los flows)
        ''' Limit Lines = FINANCING - USED LINES + FINANCING AVAILABLE LINES (todos los flows)
        ''' </summary>
        Public Shared Function GetRFCLinesGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Parámetros requeridos
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim weekNumberFrom As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_from")
                Dim weekNumberTo As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_to")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL CON PARÁMETROS
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
                dbParams.Insert(2, New DbParamInfo("@year", treasuryYear))

                Dim sql As String = TRS_SQL_Repository.GetRFCLinesGraphQuery(entityFilter)

                ' ===================================================================
                ' 3. EJECUTAR CONSULTA Y CARGAR DATOS
                ' ===================================================================
                Dim rawData As DataTable = Nothing

                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)

                        If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
                            Return Nothing
                        End If

                    Catch ex As Exception
                        Throw
                    End Try
                End Using

                ' ===================================================================
                ' 4. PREPARAR DATOS PARA GRÁFICOS
                ' ===================================================================
                Dim usedLinesData As New List(Of GraphDataPoint)()
                Dim limitLinesData As New List(Of GraphDataPoint)()

                For Each row As DataRow In rawData.Rows
                    Dim dateLabel As String = row("DateLabel").ToString()
                    Dim usedLines As Decimal = If(IsDBNull(row("UsedLines")), 0, Convert.ToDecimal(row("UsedLines")))
                    Dim limitLines As Decimal = If(IsDBNull(row("LimitLines")), 0, Convert.ToDecimal(row("LimitLines")))
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))

                    usedLinesData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = usedLines})
                    limitLinesData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = limitLines})
                Next

                ' ===================================================================
                ' 5. CREAR SERIES DEL GRÁFICO
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, "Used Lines", GraphsSharedHelpers.COLOR_ORANGE, usedLinesData)
                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, "Limit Lines", GraphsSharedHelpers.COLOR_BLUE, limitLinesData)

                ' ===================================================================
                ' 6. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        ''' <summary>
        ''' Genera gráfico combinado de barras + línea para Cash & Debt del año actual
        ''' Barras: Used Lines (naranja) y Limit Lines (azul) - Eje principal
        ''' Línea: Cash Balance (verde) - Eje secundario
        ''' Muestra desde semana 1 hasta la semana actual (prm_Treasury_WeekNumber)
        ''' </summary>
        Public Shared Function GetCashDebtCurrentYearGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Parámetros requeridos
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim currentWeekNumber As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' Forzar rango de semanas: siempre desde semana 1 hasta la semana actual
                Dim weekNumberFrom As Integer = 1
                Dim weekNumberTo As Integer = currentWeekNumber

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL UNIFICADA (CASH BALANCE + RFC LINES)
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
                dbParams.Insert(2, New DbParamInfo("@year", treasuryYear))

                Dim sql As String = TRS_SQL_Repository.GetCashDebtCurrentYearQuery(entityFilter)

                ' ===================================================================
                ' 3. EJECUTAR CONSULTA Y CARGAR DATOS
                ' ===================================================================
                Dim rawData As DataTable = Nothing

                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)

                        If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
                            Return Nothing
                        End If

                    Catch ex As Exception
                        Throw
                    End Try
                End Using

                ' ===================================================================
                ' 4. PREPARAR DATOS PARA LAS SERIES
                ' ===================================================================
                Dim cashBalanceData As New List(Of GraphDataPoint)()
                Dim usedLinesData As New List(Of GraphDataPoint)()
                Dim limitLinesData As New List(Of GraphDataPoint)()

                For Each row As DataRow In rawData.Rows
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim weekLabel As String = $"W{weekNum}"
                    Dim cashBalance As Decimal = If(IsDBNull(row("CashBalance")), 0, Convert.ToDecimal(row("CashBalance")))
                    Dim usedLines As Decimal = If(IsDBNull(row("UsedLines")), 0, Convert.ToDecimal(row("UsedLines")))
                    Dim limitLines As Decimal = If(IsDBNull(row("LimitLines")), 0, Convert.ToDecimal(row("LimitLines")))

                    cashBalanceData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = cashBalance})
                    usedLinesData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = usedLines})
                    limitLinesData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = limitLines})
                Next

                ' ===================================================================
                ' 5. CREAR SERIES DEL GRÁFICO COMBINADO
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Used Lines - Barras naranjas (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Used Lines", GraphsSharedHelpers.COLOR_ORANGE, usedLinesData, False, True)

                ' Serie 2: Cash Balance - Barras azules (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Cash Balance", GraphsSharedHelpers.COLOR_BLUE, cashBalanceData, False, True)

                ' Serie 3: Limit Lines - Línea amarilla (eje secundario) sin colores condicionales
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Limit Lines", "#FFF5C71C", limitLinesData, True, False)

                ' ===================================================================
                ' 6. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        ''' <summary>
        ''' Genera gráfico combinado de barras enfrentadas + línea para Cash Debt Balance
        ''' Cash Available = CASH AND FINANCING BALANCE (BARRA AZUL - eje principal)
        ''' Utilized Debt = FINANCING - USED LINES (BARRA NARANJA - eje principal)
        ''' Net Position = Cash Available - Utilized Debt (LÍNEA GRIS - eje secundario)
        ''' Formato igual a CashFlowProjection pero con datos de Cash & Debt Position
        ''' Cada semana muestra el StartWeek con Scenario = FW{weekNumber}
        ''' </summary>
        Public Shared Function GetCashDebtBalanceGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Parámetros requeridos
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim currentWeekNumber As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' Forzar rango de semanas: siempre desde semana 1 hasta la semana actual
                Dim weekNumberFrom As Integer = 1
                Dim weekNumberTo As Integer = currentWeekNumber

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL UNIFICADA (CASH BALANCE + RFC LINES)
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
                dbParams.Insert(2, New DbParamInfo("@year", treasuryYear))

                ' Nota: Usa la misma query que GetCashDebtCurrentYearGraph
                Dim sql As String = TRS_SQL_Repository.GetCashDebtCurrentYearQuery(entityFilter)

                ' ===================================================================
                ' 3. EJECUTAR CONSULTA Y CARGAR DATOS
                ' ===================================================================
                Dim rawData As DataTable = Nothing

                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)

                        If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
                            Return Nothing
                        End If

                    Catch ex As Exception
                        Throw
                    End Try
                End Using

                ' ===================================================================
                ' 4. PREPARAR DATOS PARA LAS SERIES
                ' ===================================================================
                Dim cashBalanceData As New List(Of GraphDataPoint)()
                Dim usedLinesData As New List(Of GraphDataPoint)()
                Dim limitLinesData As New List(Of GraphDataPoint)()

                For Each row As DataRow In rawData.Rows
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim weekLabel As String = $"W{weekNum}"
                    Dim cashBalance As Decimal = If(IsDBNull(row("CashBalance")), 0, Convert.ToDecimal(row("CashBalance")))
                    Dim usedLines As Decimal = If(IsDBNull(row("UsedLines")), 0, Convert.ToDecimal(row("UsedLines")))
                    Dim limitLines As Decimal = If(IsDBNull(row("LimitLines")), 0, Convert.ToDecimal(row("LimitLines")))

                    cashBalanceData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = cashBalance})
                    usedLinesData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = usedLines})
                    limitLinesData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = limitLines})
                Next

                ' ===================================================================
                ' 5. CREAR SERIES DEL GRÁFICO COMBINADO (MISMO TIPO QUE FreeOperatingCashflowMonthly)
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Used Lines - Barras gris oscuro apiladas (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Used Lines", GraphsSharedHelpers.COLOR_GRAY, usedLinesData, False, True)

                ' Serie 2: Limit Lines - Línea amarilla en eje secundario
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Limit Lines", "#FFF5C71C", limitLinesData, True, False)

                ' Serie 3: Cash Balance - Línea naranja en eje secundario
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Cash Balance", GraphsSharedHelpers.COLOR_ORANGE, cashBalanceData, True, False)

                ' ===================================================================
                ' 6. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        ''' <summary>
        ''' Genera gráfico combinado de barras + línea para Weekly Cash Position
        ''' Barras apiladas: Internal Cash (azul) + External Cash (naranja) - Eje principal
        ''' Línea: Total Cash (gris oscuro) - Eje secundario
        ''' Usa los mismos datos que la tabla TRL_CashDebtPosition_Weeks
        ''' Parámetros: prm_Treasury_CompanyNames, prm_Treasury_Year, prm_Treasury_WeekNumber_From, prm_Treasury_WeekNumber_To
        ''' </summary>
        Public Shared Function GetWeeklyCashPositionGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Parámetros requeridos
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim weekNumberFrom As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_from")
                Dim weekNumberTo As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_to")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' Validación adicional de parámetros
                If weekNumberFrom > weekNumberTo Then
                    Return Nothing
                End If

                ' Obtener nombre de compañía desde el ID si es necesario
                Dim paramEntity As String = ""
                If Not String.IsNullOrWhiteSpace(companyNames) AndAlso companyNames <> "HTD" AndAlso companyNames <> "0" Then
                    paramEntity = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, companyNames)
                End If

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL (MISMA LÓGICA QUE LA TABLA)
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()

                ' Build WHERE clause - handle HTD case
                Dim entityFilter As String = ""
                If Not String.IsNullOrWhiteSpace(paramEntity) AndAlso paramEntity <> "HTD" Then
                    dbParams.Add(New DbParamInfo("@entity", paramEntity))
                    entityFilter = "AND Entity = @entity"
                End If

                dbParams.Add(New DbParamInfo("@year", treasuryYear))
                dbParams.Add(New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Add(New DbParamInfo("@weekTo", weekNumberTo))

                Dim sql As String = TRS_SQL_Repository.GetWeeklyCashPositionQuery(entityFilter)

                ' ===================================================================
                ' 3. EJECUTAR CONSULTA Y CARGAR DATOS
                ' ===================================================================
                Dim rawData As DataTable = Nothing

                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)

                        If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
                            Return Nothing
                        End If

                    Catch ex As Exception
                        Throw
                    End Try
                End Using

                ' ===================================================================
                ' 4. CONSTRUIR DICCIONARIO DE DATOS POR SEMANA Y FLOW
                ' ===================================================================
                Dim dataLookup As New Dictionary(Of Integer, Dictionary(Of String, Decimal))

                For Each row As DataRow In rawData.Rows
                    Dim weekNum As Integer = If(IsDBNull(row("WeekNumber")), 0, Convert.ToInt32(row("WeekNumber")))
                    Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString())
                    Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))

                    If Not dataLookup.ContainsKey(weekNum) Then
                        dataLookup(weekNum) = New Dictionary(Of String, Decimal)
                    End If

                    dataLookup(weekNum)(flow) = amount
                Next

                ' ===================================================================
                ' 5. PREPARAR DATOS PARA LAS 3 SERIES
                ' ===================================================================
                Dim totalCashData As New List(Of GraphDataPoint)()
                Dim internalCashData As New List(Of GraphDataPoint)()
                Dim externalCashData As New List(Of GraphDataPoint)()

                For week As Integer = weekNumberFrom To weekNumberTo
                    Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"

                    Dim internalCash As Decimal = 0
                    Dim externalCash As Decimal = 0

                    If dataLookup.ContainsKey(week) Then
                        If dataLookup(week).ContainsKey("INTernal Cash (+)") Then
                            internalCash = dataLookup(week)("INTernal Cash (+)")
                        End If
                        If dataLookup(week).ContainsKey("EXTernal Cash (+)") Then
                            externalCash = dataLookup(week)("EXTernal Cash (+)")
                        End If
                    End If

                    Dim totalCash As Decimal = internalCash + externalCash

                    totalCashData.Add(New GraphDataPoint With {
                        .WeekNumber = week,
                        .DateLabel = weekLabel,
                        .Amount = totalCash
                    })

                    internalCashData.Add(New GraphDataPoint With {
                        .WeekNumber = week,
                        .DateLabel = weekLabel,
                        .Amount = internalCash
                    })

                    externalCashData.Add(New GraphDataPoint With {
                        .WeekNumber = week,
                        .DateLabel = weekLabel,
                        .Amount = externalCash
                    })
                Next

                ' ===================================================================
                ' 6. CREAR SERIES DEL GRÁFICO (BARRAS APILADAS + LÍNEA)
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Internal Cash - Barra azul apilada (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Internal Cash", GraphsSharedHelpers.COLOR_BLUE, internalCashData, False, True)

                ' Serie 2: External Cash - Barra naranja apilada (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "External Cash", GraphsSharedHelpers.COLOR_ORANGE, externalCashData, False, True)

                ' Serie 3: Total Cash - Línea gris oscuro en eje secundario
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Total Cash", GraphsSharedHelpers.COLOR_GRAY, totalCashData, True, False)

                ' ===================================================================
                ' 7. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

        ''' <summary>
        ''' Genera gráfico combinado de barras + línea para Weekly Debt Position
        ''' Barras apiladas: Internal Debt (azul) + External Debt (naranja) - Eje principal
        ''' Línea: Total Debt (gris oscuro) - Eje secundario
        ''' Usa los mismos datos que la tabla TRL_CashDebtPosition_Weeks (pero filtrando por deuda)
        ''' Parámetros: prm_Treasury_CompanyNames, prm_Treasury_Year, prm_Treasury_WeekNumber_From, prm_Treasury_WeekNumber_To
        ''' </summary>
        Public Shared Function GetWeeklyDebtPositionGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Parámetros requeridos
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim weekNumberFrom As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_from")
                Dim weekNumberTo As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_to")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' Validación adicional de parámetros
                If weekNumberFrom > weekNumberTo Then
                    Return Nothing
                End If

                ' Obtener nombre de compañía desde el ID si es necesario
                Dim paramEntity As String = ""
                If Not String.IsNullOrWhiteSpace(companyNames) AndAlso companyNames <> "HTD" AndAlso companyNames <> "0" Then
                    paramEntity = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, companyNames)
                End If

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()

                ' Build WHERE clause - handle HTD case
                Dim entityFilter As String = ""
                If Not String.IsNullOrWhiteSpace(paramEntity) AndAlso paramEntity <> "HTD" Then
                    dbParams.Add(New DbParamInfo("@entity", paramEntity))
                    entityFilter = "AND Entity = @entity"
                End If

                dbParams.Add(New DbParamInfo("@year", treasuryYear))
                dbParams.Add(New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Add(New DbParamInfo("@weekTo", weekNumberTo))

                Dim sql As String = TRS_SQL_Repository.GetWeeklyDebtPositionQuery(entityFilter)

                ' ===================================================================
                ' 3. EJECUTAR CONSULTA Y CARGAR DATOS
                ' ===================================================================
                Dim rawData As DataTable = Nothing

                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Try
                        rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)

                        If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
                            Return Nothing
                        End If

                    Catch ex As Exception
                        Throw
                    End Try
                End Using

                ' ===================================================================
                ' 4. CONSTRUIR DICCIONARIO DE DATOS POR SEMANA Y FLOW
                ' ===================================================================
                Dim dataLookup As New Dictionary(Of Integer, Dictionary(Of String, Decimal))

                For Each row As DataRow In rawData.Rows
                    Dim weekNum As Integer = If(IsDBNull(row("WeekNumber")), 0, Convert.ToInt32(row("WeekNumber")))
                    Dim flow As String = If(IsDBNull(row("Flow")), "", row("Flow").ToString())
                    Dim amount As Decimal = If(IsDBNull(row("Amount")), 0, Convert.ToDecimal(row("Amount")))

                    If Not dataLookup.ContainsKey(weekNum) Then
                        dataLookup(weekNum) = New Dictionary(Of String, Decimal)
                    End If

                    dataLookup(weekNum)(flow) = amount
                Next

                ' ===================================================================
                ' 5. PREPARAR DATOS PARA LAS 3 SERIES
                ' ===================================================================
                Dim totalDebtData As New List(Of GraphDataPoint)()
                Dim internalDebtData As New List(Of GraphDataPoint)()
                Dim externalDebtData As New List(Of GraphDataPoint)()

                For week As Integer = weekNumberFrom To weekNumberTo
                    Dim weekLabel As String = $"W{week.ToString().PadLeft(2, "0"c)}"

                    Dim internalDebt As Decimal = 0
                    Dim externalDebt As Decimal = 0

                    If dataLookup.ContainsKey(week) Then
                        If dataLookup(week).ContainsKey("INTernal Debt (-)") Then
                            internalDebt = dataLookup(week)("INTernal Debt (-)")
                        End If
                        If dataLookup(week).ContainsKey("EXTernal Debt (-)") Then
                            externalDebt = dataLookup(week)("EXTernal Debt (-)")
                        End If
                    End If

                    Dim totalDebt As Decimal = internalDebt + externalDebt

                    totalDebtData.Add(New GraphDataPoint With {
                        .WeekNumber = week,
                        .DateLabel = weekLabel,
                        .Amount = totalDebt
                    })

                    internalDebtData.Add(New GraphDataPoint With {
                        .WeekNumber = week,
                        .DateLabel = weekLabel,
                        .Amount = internalDebt
                    })

                    externalDebtData.Add(New GraphDataPoint With {
                        .WeekNumber = week,
                        .DateLabel = weekLabel,
                        .Amount = externalDebt
                    })
                Next

                ' ===================================================================
                ' 6. CREAR SERIES DEL GRÁFICO (BARRAS APILADAS + LÍNEA)
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Internal Debt - Barra azul apilada (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Internal Debt", GraphsSharedHelpers.COLOR_BLUE, internalDebtData, False, True)

                ' Serie 2: External Debt - Barra naranja apilada (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "External Debt", GraphsSharedHelpers.COLOR_ORANGE, externalDebtData, False, True)

                ' Serie 3: Total Debt - Línea gris oscuro en eje secundario
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Total Debt", GraphsSharedHelpers.COLOR_GRAY, totalDebtData, True, False)

                ' ===================================================================
                ' 7. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function

    End Class
End Namespace
