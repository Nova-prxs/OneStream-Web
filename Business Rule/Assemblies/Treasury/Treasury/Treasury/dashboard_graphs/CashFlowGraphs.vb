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
    Public Class CashFlowGraphs

        #Region "Cash Flow Projection Graph"
        ''' <summary>
        ''' Genera gráfico combinado de barras + línea para Cash Flow Projection
        ''' Inflow = suma de Operating + Investment Inflows (BARRA AZUL)
        ''' Outflow = suma de Operating + Investment Outflows (BARRA NARANJA)
        ''' Net Position = Inflow - Outflow (LÍNEA EN EJE SECUNDARIO)
        ''' Usa mix de Actual (semanas pasadas) y Projection (semanas futuras)
        ''' </summary>
        Public Shared Function GetCashFlowProjectionGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
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

                ' uploadWeek es weekNumberTo (última semana seleccionada)
                Dim uploadWeek As Integer = weekNumberTo

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL CON PARÁMETROS
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
                dbParams.Insert(2, New DbParamInfo("@uploadWeek", uploadWeek))
                dbParams.Insert(3, New DbParamInfo("@year", treasuryYear))

                Dim sql As String = TRS_SQL_Repository.GetCashFlowProjectionQuery(entityFilter)

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
                Dim inflowsData As New List(Of GraphDataPoint)()
                Dim outflowsData As New List(Of GraphDataPoint)()
                Dim netPositionData As New List(Of GraphDataPoint)()

                For Each row As DataRow In rawData.Rows
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim dateLabel As String = $"W{weekNum.ToString("00")}"
                    Dim inflows As Decimal = If(IsDBNull(row("Inflows")), 0, Convert.ToDecimal(row("Inflows")))
                    Dim outflows As Decimal = If(IsDBNull(row("Outflows")), 0, Convert.ToDecimal(row("Outflows")))

                    ' Calcular posición neta = Inflows + Outflows (Outflows ya son negativos)
                    Dim netPosition As Decimal = inflows + outflows

                    inflowsData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = inflows})
                    outflowsData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = outflows})
                    netPositionData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = netPosition})
                Next

                ' ===================================================================
                ' 5. CREAR SERIES DEL GRÁFICO (BARRAS APILADAS + LÍNEA EN EJE SECUNDARIO)
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Inflows como barra azul apilada (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Inflows", GraphsSharedHelpers.COLOR_BLUE, inflowsData, False, True)

                ' Serie 2: Outflows como barra naranja apilada (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Outflows", GraphsSharedHelpers.COLOR_ORANGE, outflowsData, False, True)

                ' Serie 3: Net Position como línea con colores condicionales (verde si >=0, rojo si <0) en eje secundario
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Net Position", "#FFF5C71C", netPositionData, True, True, "#FF70AD47", "#FFCC0000")

                ' ===================================================================
                ' 6. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

        #Region "Cash Flow Projection Current Year Graph"
        ''' <summary>
        ''' Genera gráfico combinado de barras + línea para Cash Flow Projection del año actual
        ''' Inflow = suma de Operating + Investment Inflows (BARRA AZUL)
        ''' Outflow = suma de Operating + Investment Outflows (BARRA NARANJA)
        ''' Net Position = Inflow - Outflow (LÍNEA CON COLORES CONDICIONALES: verde si >0, rojo si <0)
        ''' Eje X = Número de semana (W1, W2, W3...)
        ''' Rango: Semana 1 hasta semana actual (prm_Treasury_WeekNumber)
        ''' </summary>
        Public Shared Function GetCashFlowProjectionCurrentYearGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' ===================================================================
                ' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
                ' ===================================================================
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                ' Parámetros requeridos (mismo patrón que CashDebtCurrentYear)
                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
                Dim currentWeekNumber As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber")
                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' Forzar rango de semanas: siempre desde semana 1 hasta la semana actual
                Dim weekNumberFrom As Integer = 1
                Dim weekNumberTo As Integer = currentWeekNumber
                Dim uploadWeek As Integer = currentWeekNumber

                ' ===================================================================
                ' 2. CONSTRUIR CONSULTA SQL CON PARÁMETROS
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
                dbParams.Insert(2, New DbParamInfo("@uploadWeek", uploadWeek))
                dbParams.Insert(3, New DbParamInfo("@year", treasuryYear))

                Dim sql As String = TRS_SQL_Repository.GetCashFlowProjectionQuery(entityFilter, False)

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
                ' 4. PREPARAR DATOS PARA GRÁFICOS (EJE X = NÚMERO DE SEMANA)
                ' ===================================================================
                Dim inflowsData As New List(Of GraphDataPoint)()
                Dim outflowsData As New List(Of GraphDataPoint)()
                Dim netPositionData As New List(Of GraphDataPoint)()

                For Each row As DataRow In rawData.Rows
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim weekLabel As String = $"W{weekNum}"
                    Dim inflows As Decimal = If(IsDBNull(row("Inflows")), 0, Convert.ToDecimal(row("Inflows")))
                    Dim outflows As Decimal = If(IsDBNull(row("Outflows")), 0, Convert.ToDecimal(row("Outflows")))

                    ' Calcular posición neta = Inflows + Outflows (Outflows ya son negativos)
                    Dim netPosition As Decimal = inflows + outflows

                    inflowsData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = inflows})
                    outflowsData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = outflows})
                    netPositionData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = weekLabel, .Amount = netPosition})
                Next

                ' ===================================================================
                ' 5. CREAR SERIES DEL GRÁFICO (BARRAS + LÍNEA CON COLORES CONDICIONALES)
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Inflows como barra azul (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Inflows", GraphsSharedHelpers.COLOR_BLUE, inflowsData, False)

                ' Serie 2: Outflows como barra naranja (eje principal)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Outflows", GraphsSharedHelpers.COLOR_ORANGE, outflowsData, False)

                ' Serie 3: Net Position como línea con colores condicionales (verde si >=0, rojo si <0) en eje secundario
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Net Position", "#FFF5C71C", netPositionData, True, True, "#FF70AD47", "#FFCC0000")

                ' ===================================================================
                ' 6. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

        #Region "Cash Flow 4 Week Projection Graph"
        ''' <summary>
        ''' Genera gráfico de líneas para Cash Flow 4 Week Projection (Ventana Móvil de 4 Semanas)
        ''' Cada punto representa la suma de 4 semanas consecutivas empezando desde esa semana
        ''' LÓGICA ESPECIAL: Para cada punto del eje X, usa las proyecciones de la semana actual (paramWeek) cuando sea necesario
        ''' Ejemplo si paramWeek=45:
        '''   - W43: Actual W43 + Actual W44 + Proyección W45 (FW45) + Proyección W46 (FW45)
        '''   - W45: Proyección W45 (FW45) + Proyección W46 (FW45) + Proyección W47 (FW45) + Proyección W48 (FW45)
        '''   - W48: Proyección W48 (FW45) + Proyección W49 (FW45) + Proyección W50 (FW45) + Proyección W51 (FW45)
        ''' Inflows = suma de Operating + Investment Inflows (LÍNEA AZUL)
        ''' Outflows = suma de Operating + Investment Outflows (LÍNEA NARANJA)
        ''' Eje X = Desde semana 1 hasta paramWeek + 3
        ''' </summary>
        Public Shared Function GetCashFlow4WeekProjectionGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
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

                ' ===================================================================
                ' 2. CALCULAR RANGO DE SEMANAS EN EJE X (1 hasta currentWeek + 3)
                ' ===================================================================
                Dim maxWeekInGraph As Integer = currentWeekNumber + 3

                ' ===================================================================
                ' 3. OBTENER TODOS LOS DATOS NECESARIOS
                ' ===================================================================
                ' Necesitamos:
                ' - Actual: semanas < currentWeek (uploadados en semana siguiente a la actual)
                ' - Projection: todas las semanas >= currentWeek (uploadados en currentWeek)
                '   hasta currentWeek + 6 (para cubrir ventana de 4 semanas del último punto)
                ' - Si currentWeek está cerca del final del año, necesitamos también datos del año siguiente

                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                ' Obtener el número máximo de semanas del año actual y del siguiente
                Dim maxWeeksInYear As Integer = GraphsSharedHelpers.GetWeeksInYear(si, treasuryYear)
                Dim maxWeeksNextYear As Integer = GraphsSharedHelpers.GetWeeksInYear(si, treasuryYear + 1)

                dbParams.Add(New DbParamInfo("@year", treasuryYear))
                dbParams.Add(New DbParamInfo("@nextYear", treasuryYear + 1))
                dbParams.Add(New DbParamInfo("@currentWeek", currentWeekNumber))
                dbParams.Add(New DbParamInfo("@maxWeekCurrentYear", maxWeeksInYear))

                ' Calcular cuántas semanas del año siguiente necesitamos
                Dim weeksNeededNextYear As Integer = Math.Max(0, (currentWeekNumber + 6) - maxWeeksInYear)

                Dim sql As String = TRS_SQL_Repository.GetCashFlow4WeekProjectionFullQuery(entityFilter, weeksNeededNextYear)

                ' ===================================================================
                ' 4. EJECUTAR CONSULTA Y CARGAR DATOS
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
                ' 5. CONSTRUIR DICCIONARIO DE DATOS POR AÑO Y SEMANA
                ' ===================================================================
                Dim actualData As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
                Dim projectionData As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
                Dim projectionDataNextYear As New Dictionary(Of Integer, Dictionary(Of String, Decimal))

                For Each row As DataRow In rawData.Rows
                    Dim projYear As Integer = Convert.ToInt32(row("ProjectionYear"))
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim projType As String = row("ProjectionType").ToString()
                    Dim inflows As Decimal = If(IsDBNull(row("Inflows")), 0, Convert.ToDecimal(row("Inflows")))
                    Dim outflows As Decimal = If(IsDBNull(row("Outflows")), 0, Convert.ToDecimal(row("Outflows")))

                    Dim dataDict As New Dictionary(Of String, Decimal) From {
                        {"Inflows", inflows},
                        {"Outflows", outflows}
                    }

                    If projYear = treasuryYear Then
                        If projType = "Actual" Then
                            actualData(weekNum) = dataDict
                        Else ' Projection
                            projectionData(weekNum) = dataDict
                        End If
                    Else ' projYear = treasuryYear + 1
                        projectionDataNextYear(weekNum) = dataDict
                    End If
                Next

                ' ===================================================================
                ' 6. CALCULAR VENTANAS DE 4 SEMANAS PARA CADA PUNTO DEL GRÁFICO
                ' ===================================================================
                Dim inflowsData As New List(Of GraphDataPoint)()
                Dim outflowsData As New List(Of GraphDataPoint)()

                ' Para cada semana en el eje X (1 hasta maxWeekInGraph = currentWeek + 3)
                For weekStart As Integer = 1 To maxWeekInGraph
                    Dim weekLabel As String = $"W{weekStart}"

                    ' Sumar las 4 semanas consecutivas (weekStart hasta weekStart+3)
                    Dim inflowsSum As Decimal = 0
                    Dim outflowsSum As Decimal = 0

                    For offset As Integer = 0 To 3
                        Dim targetWeek As Integer = weekStart + offset
                        Dim targetYear As Integer = treasuryYear

                        ' Si la semana objetivo excede el máximo del año actual, pasa al año siguiente
                        If targetWeek > maxWeeksInYear Then
                            targetWeek = targetWeek - maxWeeksInYear
                            targetYear = treasuryYear + 1
                        End If

                        ' Decidir si usar Actual o Projection
                        ' Si estamos en el año actual Y targetWeek < currentWeekNumber -> usar Actual
                        ' En cualquier otro caso -> usar Projection

                        If targetYear = treasuryYear AndAlso targetWeek < currentWeekNumber Then
                            ' Usar datos Actual del año actual
                            If actualData.ContainsKey(targetWeek) Then
                                inflowsSum += actualData(targetWeek)("Inflows")
                                outflowsSum += actualData(targetWeek)("Outflows")
                            End If
                        ElseIf targetYear = treasuryYear Then
                            ' Usar datos Projection del año actual (uploadados en currentWeek)
                            If projectionData.ContainsKey(targetWeek) Then
                                inflowsSum += projectionData(targetWeek)("Inflows")
                                outflowsSum += projectionData(targetWeek)("Outflows")
                            End If
                        Else
                            ' Usar datos Projection del año siguiente (uploadados en currentWeek del año actual)
                            If projectionDataNextYear.ContainsKey(targetWeek) Then
                                inflowsSum += projectionDataNextYear(targetWeek)("Inflows")
                                outflowsSum += projectionDataNextYear(targetWeek)("Outflows")
                            End If
                        End If
                    Next

                    ' Aplicar valor absoluto a las sumas
                    inflowsData.Add(New GraphDataPoint With {
                        .WeekNumber = weekStart,
                        .DateLabel = weekLabel,
                        .Amount = Math.Abs(inflowsSum)
                    })

                    outflowsData.Add(New GraphDataPoint With {
                        .WeekNumber = weekStart,
                        .DateLabel = weekLabel,
                        .Amount = Math.Abs(outflowsSum)
                    })
                Next

                ' ===================================================================
                ' 7. CREAR SERIES DEL GRÁFICO (DOS LÍNEAS)
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                ' Serie 1: Inflows - Línea azul
                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, "Inflows", GraphsSharedHelpers.COLOR_BLUE, inflowsData)

                ' Serie 2: Outflows - Línea naranja
                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, "Outflows", GraphsSharedHelpers.COLOR_ORANGE, outflowsData)

                ' ===================================================================
                ' 8. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                '✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

        #Region "Cash Flow 4 Week Projection Test Graph (Direct Parameters)"
        ''' <summary>
        ''' Genera gráfico de líneas para Cash Flow 4 Week Projection (Ventana Móvil de 4 Semanas)
        ''' VERSIÓN TEST: Lee parámetros directamente de args.NameValuePairs sin depender de p_query_params
        ''' Uso: {Workspace.Current.Treasury.DataAdapterGraphs}{CashFlow4WeekProjection_Test}{prm_Treasury_Year=[|!prm_Treasury_Year!|],prm_Treasury_WeekNumber=[|!prm_Treasury_WeekNumber!|],prm_Treasury_CompanyNames=[|!prm_Treasury_CompanyNames!|]}
        ''' 
        ''' Cada punto representa la suma de 4 semanas consecutivas empezando desde esa semana
        ''' LÓGICA ESPECIAL: Para cada punto del eje X, usa las proyecciones de la semana actual (paramWeek) cuando sea necesario
        ''' Inflows = suma de Operating + Investment Inflows (LÍNEA AZUL)
        ''' Outflows = suma de Operating + Investment Outflows (LÍNEA NARANJA)
        ''' Eje X = Desde semana 1 hasta paramWeek + 3
        ''' </summary>
        Public Shared Function GetCashFlow4WeekProjectionTestGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                ' ===================================================================
                ' 1. OBTENER PARÁMETROS DIRECTAMENTE DE args.NameValuePairs
                ' ===================================================================
                ' Lee los parámetros directamente sin depender de p_query_params
                Dim yearStr As String = args.NameValuePairs.GetValueOrDefault("prm_Treasury_Year", "")
                Dim weekStr As String = args.NameValuePairs.GetValueOrDefault("prm_Treasury_WeekNumber", "")
                Dim companyNames As String = args.NameValuePairs.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' Validar parámetros requeridos
                If String.IsNullOrEmpty(yearStr) OrElse Not IsNumeric(yearStr) Then
                    Throw New XFException(si, New ArgumentException("Parameter 'prm_Treasury_Year' is required and must be numeric"))
                End If
                If String.IsNullOrEmpty(weekStr) OrElse Not IsNumeric(weekStr) Then
                    Throw New XFException(si, New ArgumentException("Parameter 'prm_Treasury_WeekNumber' is required and must be numeric"))
                End If

                Dim treasuryYear As Integer = Convert.ToInt32(yearStr)
                Dim currentWeekNumber As Integer = Convert.ToInt32(weekStr)

                ' ===================================================================
                ' 2. CALCULAR RANGO DE SEMANAS EN EJE X (1 hasta currentWeek + 3)
                ' ===================================================================
                Dim maxWeekInGraph As Integer = currentWeekNumber + 3

                ' ===================================================================
                ' 3. OBTENER TODOS LOS DATOS NECESARIOS
                ' ===================================================================
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                Dim maxWeeksInYear As Integer = GraphsSharedHelpers.GetWeeksInYear(si, treasuryYear)
                Dim maxWeeksNextYear As Integer = GraphsSharedHelpers.GetWeeksInYear(si, treasuryYear + 1)

                dbParams.Add(New DbParamInfo("@year", treasuryYear))
                dbParams.Add(New DbParamInfo("@nextYear", treasuryYear + 1))
                dbParams.Add(New DbParamInfo("@currentWeek", currentWeekNumber))
                dbParams.Add(New DbParamInfo("@maxWeekCurrentYear", maxWeeksInYear))

                Dim weeksNeededNextYear As Integer = Math.Max(0, (currentWeekNumber + 6) - maxWeeksInYear)

                Dim sql As String = TRS_SQL_Repository.GetCashFlow4WeekProjectionFullQuery(entityFilter, weeksNeededNextYear)

                ' ===================================================================
                ' 4. EJECUTAR CONSULTA Y CARGAR DATOS
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
                ' 5. CONSTRUIR DICCIONARIO DE DATOS POR AÑO Y SEMANA
                ' ===================================================================
                Dim actualData As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
                Dim projectionData As New Dictionary(Of Integer, Dictionary(Of String, Decimal))
                Dim projectionDataNextYear As New Dictionary(Of Integer, Dictionary(Of String, Decimal))

                For Each row As DataRow In rawData.Rows
                    Dim projYear As Integer = Convert.ToInt32(row("ProjectionYear"))
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))
                    Dim projType As String = row("ProjectionType").ToString()
                    Dim inflows As Decimal = If(IsDBNull(row("Inflows")), 0, Convert.ToDecimal(row("Inflows")))
                    Dim outflows As Decimal = If(IsDBNull(row("Outflows")), 0, Convert.ToDecimal(row("Outflows")))

                    Dim dataDict As New Dictionary(Of String, Decimal) From {
                        {"Inflows", inflows},
                        {"Outflows", outflows}
                    }

                    If projYear = treasuryYear Then
                        If projType = "Actual" Then
                            actualData(weekNum) = dataDict
                        Else ' Projection
                            projectionData(weekNum) = dataDict
                        End If
                    Else ' projYear = treasuryYear + 1
                        projectionDataNextYear(weekNum) = dataDict
                    End If
                Next

                ' ===================================================================
                ' 6. CALCULAR VENTANAS DE 4 SEMANAS PARA CADA PUNTO DEL GRÁFICO
                ' ===================================================================
                Dim inflowsData As New List(Of GraphDataPoint)()
                Dim outflowsData As New List(Of GraphDataPoint)()

                For weekStart As Integer = 1 To maxWeekInGraph
                    Dim weekLabel As String = $"W{weekStart}"

                    Dim inflowsSum As Decimal = 0
                    Dim outflowsSum As Decimal = 0

                    For offset As Integer = 0 To 3
                        Dim targetWeek As Integer = weekStart + offset
                        Dim targetYear As Integer = treasuryYear

                        If targetWeek > maxWeeksInYear Then
                            targetWeek = targetWeek - maxWeeksInYear
                            targetYear = treasuryYear + 1
                        End If

                        If targetYear = treasuryYear AndAlso targetWeek < currentWeekNumber Then
                            If actualData.ContainsKey(targetWeek) Then
                                inflowsSum += actualData(targetWeek)("Inflows")
                                outflowsSum += actualData(targetWeek)("Outflows")
                            End If
                        ElseIf targetYear = treasuryYear Then
                            If projectionData.ContainsKey(targetWeek) Then
                                inflowsSum += projectionData(targetWeek)("Inflows")
                                outflowsSum += projectionData(targetWeek)("Outflows")
                            End If
                        Else
                            If projectionDataNextYear.ContainsKey(targetWeek) Then
                                inflowsSum += projectionDataNextYear(targetWeek)("Inflows")
                                outflowsSum += projectionDataNextYear(targetWeek)("Outflows")
                            End If
                        End If
                    Next

                    inflowsData.Add(New GraphDataPoint With {
                        .WeekNumber = weekStart,
                        .DateLabel = weekLabel,
                        .Amount = Math.Abs(inflowsSum)
                    })

                    outflowsData.Add(New GraphDataPoint With {
                        .WeekNumber = weekStart,
                        .DateLabel = weekLabel,
                        .Amount = Math.Abs(outflowsSum)
                    })
                Next

                ' ===================================================================
                ' 7. CREAR SERIES DEL GRÁFICO (DOS LÍNEAS)
                ' ===================================================================
                Dim oSeriesCollection As New XFSeriesCollection()

                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, "Inflows", GraphsSharedHelpers.COLOR_BLUE, inflowsData)
                GraphsSharedHelpers.AddLineSeries(oSeriesCollection, "Outflows", GraphsSharedHelpers.COLOR_ORANGE, outflowsData)

                ' ===================================================================
                ' 8. RETORNAR DATASET PARA EL DASHBOARD
                ' ===================================================================
                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

        #Region "Free Operating Cashflow Monthly Graph"
        ''' <summary>
        ''' Genera gráfico de Free Operating Cashflow con desglose semanal acumulado por mes
        ''' Muestra Inflows y Outflows acumulados desde el inicio de cada mes
        ''' El acumulado se reinicia al cambiar de mes
        ''' Rango: Definido por prm_Treasury_WeekNumber_from y prm_Treasury_WeekNumber_to
        ''' </summary>
        Public Shared Function GetFreeOperatingCashflowMonthlyGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
            Try
                Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass

                ' 1. Obtener parámetros
                Dim queryParams As String = args.NameValuePairs("p_query_params")
                Dim parameterDict As Dictionary(Of String, String) =
                    UTISharedFunctionsBR.SplitQueryParams(si, queryParams)

                Dim treasuryYear As Integer = GraphsSharedHelpers.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")

                ' Configurar rangos de visualización
                Dim weekNumberFrom As Integer = 1
                Dim weekNumberTo As Integer = GraphsSharedHelpers.GetWeeksInYear(si, treasuryYear) ' Default al final del año

                ' Buscar weekNumberFrom (case insensitive)
                Dim keyFrom As String = parameterDict.Keys.FirstOrDefault(Function(k) k.Equals("prm_Treasury_WeekNumber_from", StringComparison.OrdinalIgnoreCase))
                If Not String.IsNullOrEmpty(keyFrom) AndAlso IsNumeric(parameterDict(keyFrom)) Then
                    weekNumberFrom = Convert.ToInt32(parameterDict(keyFrom))
                End If

                ' Buscar weekNumberTo (case insensitive)
                Dim keyTo As String = parameterDict.Keys.FirstOrDefault(Function(k) k.Equals("prm_Treasury_WeekNumber_to", StringComparison.OrdinalIgnoreCase))
                If Not String.IsNullOrEmpty(keyTo) AndAlso IsNumeric(parameterDict(keyTo)) Then
                    weekNumberTo = Convert.ToInt32(parameterDict(keyTo))
                End If

                ' Determinar uploadWeek (Punto de corte Actual/Projection)
                ' Por defecto usamos weekNumberTo (igual que CashFlowProjection) para ver la foto a esa fecha
                Dim uploadWeek As Integer = weekNumberTo

                ' Si tenemos la semana actual real, hacemos clamp para no buscar Actuals en el futuro (si weekNumberTo > currentWeek)
                Dim keyCurrent As String = parameterDict.Keys.FirstOrDefault(Function(k) k.Equals("prm_Treasury_WeekNumber", StringComparison.OrdinalIgnoreCase))
                If Not String.IsNullOrEmpty(keyCurrent) AndAlso IsNumeric(parameterDict(keyCurrent)) Then
                    Dim currentWeek As Integer = Convert.ToInt32(parameterDict(keyCurrent))
                    If currentWeek < uploadWeek Then
                        uploadWeek = currentWeek
                    End If
                End If

                Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")

                ' Para calcular correctamente el acumulado mensual (MTD), necesitamos datos desde el inicio del año
                ' Consultamos desde semana 1 hasta weekNumberTo
                Dim queryWeekFrom As Integer = 1
                Dim queryWeekTo As Integer = weekNumberTo

                ' 3. Construir consulta
                Dim dbParams As New List(Of DbParamInfo)()
                Dim entityFilter As String = GraphsSharedHelpers.BuildEntityFilter(si, companyNames, dbParams)

                dbParams.Insert(0, New DbParamInfo("@weekFrom", queryWeekFrom))
                dbParams.Insert(1, New DbParamInfo("@weekTo", queryWeekTo))
                dbParams.Insert(2, New DbParamInfo("@uploadWeek", uploadWeek))
                dbParams.Insert(3, New DbParamInfo("@year", treasuryYear))

                Dim sql As String = TRS_SQL_Repository.GetFreeOperatingCashflowMonthlyQuery(entityFilter)

                ' 4. Ejecutar y procesar
                Dim rawData As DataTable = Nothing
                Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)
                End Using

                If rawData Is Nothing Then rawData = New DataTable()

                Dim inflowsData As New List(Of GraphDataPoint)()
                Dim outflowsData As New List(Of GraphDataPoint)()
                Dim netPositionData As New List(Of GraphDataPoint)()

                Dim cultureEs As New CultureInfo("es-ES")

                ' Variables para acumulación mensual
                Dim currentMonth As Integer = -1
                Dim accumInflows As Decimal = 0
                Dim accumOutflows As Decimal = 0

                For Each row As DataRow In rawData.Rows
                    Dim weekDate As DateTime = Convert.ToDateTime(row("weekStartDate"))
                    Dim monthNum As Integer = weekDate.Month
                    Dim weekNum As Integer = Convert.ToInt32(row("ProjectionWeekNumber"))

                    ' Reiniciar acumulados si cambia el mes
                    If monthNum <> currentMonth Then
                        accumInflows = 0
                        accumOutflows = 0
                        currentMonth = monthNum
                    End If

                    Dim inflows As Decimal = If(IsDBNull(row("Inflows")), 0, Convert.ToDecimal(row("Inflows")))
                    Dim outflows As Decimal = If(IsDBNull(row("Outflows")), 0, Convert.ToDecimal(row("Outflows")))

                    ' Acumular
                    accumInflows += inflows
                    accumOutflows += outflows

                    ' Solo añadir al gráfico si está dentro del rango solicitado
                    If weekNum >= weekNumberFrom Then
                        Dim netPosition As Decimal = accumInflows + accumOutflows

                        ' Etiqueta: N.W01 (Mes.Semana)
                        Dim dateLabel As String = $"{monthNum}.W{weekNum.ToString("00")}"

                        inflowsData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = accumInflows})
                        outflowsData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = accumOutflows})
                        netPositionData.Add(New GraphDataPoint With {.WeekNumber = weekNum, .DateLabel = dateLabel, .Amount = netPosition})
                    End If
                Next

                ' 5. Crear series
                Dim oSeriesCollection As New XFSeriesCollection()
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Inflows", GraphsSharedHelpers.COLOR_BLUE, inflowsData, False, True)
                GraphsSharedHelpers.AddBarSeries(oSeriesCollection, "Outflows", GraphsSharedHelpers.COLOR_ORANGE, outflowsData, False, True)
                GraphsSharedHelpers.AddLineSeriesSecondaryAxis(oSeriesCollection, "Net Position", "#FFF5C71C", netPositionData, True, True, "#FF70AD47", "#FFCC0000")

                Return oSeriesCollection.CreateDataSet(si)

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        #End Region

    End Class
End Namespace
