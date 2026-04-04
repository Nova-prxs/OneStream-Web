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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DataAdapterGraphs
    Public Class GraphsSharedHelpers

        ' Constants for colors (centralized)
        Public Const COLOR_BLUE As String = "#FF4472C4"
        Public Const COLOR_ORANGE As String = "#FFED7D31"
        Public Const COLOR_GRAY As String = "#FF7F7F7F"
        Public Const LINE_THICKNESS As Integer = 3
        Public Const MARKER_SIZE As Integer = 5

        ''' <summary>
        ''' Estructura de datos simple para puntos de gráfico
        ''' </summary>
        Public Class GraphDataPoint
            Public WeekNumber As Integer
            Public DateLabel As String
            Public Amount As Decimal
        End Class

        ''' <summary>
        ''' Obtiene el número máximo de semanas de un año específico
        ''' Un año puede tener 52 o 53 semanas dependiendo de cómo caigan los días
        ''' </summary>
        Public Shared Function GetWeeksInYear(ByVal si As SessionInfo, ByVal year As Integer) As Integer
            Try
                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Dim query As String = "
						SELECT MAX(weekNumber) as MaxWeek
						FROM XFC_TRS_AUX_Date
						WHERE year = @year
					"
                    Dim dbParams As New List(Of DbParamInfo) From {
                        New DbParamInfo("year", year)
                    }

                    Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, query, dbParams, False)

                    If dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0)("MaxWeek")) Then
                        Return Convert.ToInt32(dt.Rows(0)("MaxWeek"))
                    Else
                        ' Fallback: la mayoría de años tienen 52 semanas
                        Return 52
                    End If
                End Using
            Catch ex As Exception
                Return 52 ' Fallback seguro
            End Try
        End Function

        ''' <summary>
        ''' Obtiene el número máximo de semanas de un año específico (52 o 53)
        ''' Usa ISO 8601 week date system
        ''' </summary>
        Public Shared Function GetWeeksInYear(ByVal year As Integer) As Integer
            ' Crear la última fecha del año
            Dim lastDayOfYear As New DateTime(year, 12, 31)

            ' Obtener el número de semana ISO para el último día del año
            ' Si es 1, entonces el año tiene 52 semanas (la última semana pertenece al siguiente año)
            ' Si es 52 o 53, ese es el número de semanas del año
            Dim cal As Calendar = CultureInfo.InvariantCulture.Calendar
            Dim weekNum As Integer = cal.GetWeekOfYear(lastDayOfYear, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)

            ' Si la semana es 1, significa que el 31 de diciembre pertenece a la semana 1 del año siguiente
            ' En ese caso, verificamos el 28 de diciembre que siempre está en la última semana del año
            If weekNum = 1 Then
                Dim dec28 As New DateTime(year, 12, 28)
                weekNum = cal.GetWeekOfYear(dec28, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday)
            End If

            Return weekNum
        End Function

        ''' <summary>
        ''' Obtiene la última semana de un mes específico
        ''' Regla: Un mes incluye las semanas cuyo StartDate cae en ese mes
        ''' </summary>
        Public Shared Function GetLastWeekOfMonth(ByVal si As SessionInfo, ByVal year As Integer, ByVal month As Integer) As Integer
            Try
                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    Dim query As String = "
						SELECT MAX(weekNumber) as MaxWeek
						FROM XFC_TRS_AUX_Date
						WHERE year = @year AND MONTH(weekStartDate) = @month
					"
                    Dim dbParams As New List(Of DbParamInfo) From {
                        New DbParamInfo("year", year),
                        New DbParamInfo("month", month)
                    }

                    Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, query, dbParams, False)

                    If dt IsNot Nothing AndAlso dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0)("MaxWeek")) Then
                        Return Convert.ToInt32(dt.Rows(0)("MaxWeek"))
                    Else
                        Return 52 ' Fallback
                    End If
                End Using
            Catch ex As Exception
                Return 52
            End Try
        End Function

        ''' <summary>
        ''' Valida y extrae un parámetro entero requerido del diccionario
        ''' Lanza excepción si no existe o no es válido
        ''' Búsqueda insensible a mayúsculas/minúsculas
        ''' </summary>
        Public Shared Function GetRequiredIntParameter(ByVal si As SessionInfo, ByVal paramDict As Dictionary(Of String, String), ByVal paramName As String) As Integer
            Try
                ' Búsqueda insensible a mayúsculas/minúsculas
                Dim key As String = paramDict.Keys.FirstOrDefault(Function(k) k.Equals(paramName, StringComparison.OrdinalIgnoreCase))

                If String.IsNullOrEmpty(key) Then
                    Throw New ArgumentException($"Parámetro requerido '{paramName}' no encontrado")
                End If

                Dim paramValue As String = paramDict(key)
                If Not Integer.TryParse(paramValue, CInt(0)) Then
                    Throw New ArgumentException($"Parámetro '{paramName}' debe ser un número válido. Valor: {paramValue}")
                End If

                Return Convert.ToInt32(paramValue)

            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

        ''' <summary>
        ''' Construye filtro de entidades seguro contra inyección SQL
        ''' Devuelve string vacío si no hay filtro
        ''' Los parámetros se agregan a la lista dbParams
        ''' </summary>
        Public Shared Function BuildEntityFilter(ByVal si As SessionInfo, ByVal companyNames As String, ByRef dbParams As List(Of DbParamInfo)) As String
            Dim entityFilter As String = ""

            ' Si es "HTD" (global), "0", o vacío -> no filtrar (mostrar todas las entidades)
            If String.IsNullOrEmpty(companyNames) OrElse _
               companyNames.XFEqualsIgnoreCase("HTD") OrElse _
               companyNames.XFEqualsIgnoreCase("0") Then
                Return ""
            End If

            ' Determinar si es ID numérico o nombre de entidad
            Dim entityId As Integer
            If Integer.TryParse(companyNames, entityId) Then
                ' Es un Entity_Id numérico - usar parámetro
                dbParams.Add(New DbParamInfo("@entityId", entityId))
                entityFilter = "AND m.Entity_Id = @entityId"
            Else
                ' Es un nombre de entidad - usar parámetro
                dbParams.Add(New DbParamInfo("@entityName", companyNames))
                entityFilter = "AND m.Entity = @entityName"
            End If

            Return entityFilter
        End Function

        ''' <summary>
        ''' Construye una serie de línea y la agrega a la colección
        ''' Reutilizable para todos los gráficos
        ''' </summary>
        Public Shared Sub AddLineSeries(ByVal collection As XFSeriesCollection, ByVal seriesName As String, ByVal color As String, ByVal dataPoints As List(Of GraphDataPoint))
            Dim series As New XFSeries()
            series.UniqueName = seriesName
            series.Type = XFChart2SeriesType.Line
            series.Color = color
            series.LineThickness = LINE_THICKNESS
            series.ShowMarkers = 1
            series.MarkerSize = MARKER_SIZE
            series.LineStyleType = XFChart2LineStyleType.Solid

            For Each dataPoint In dataPoints
                Dim point As New XFSeriesPoint()
                point.Argument = dataPoint.DateLabel
                point.Value = Convert.ToDouble(dataPoint.Amount)
                series.AddPoint(point)
            Next

            collection.Series.Add(series)
        End Sub

        ''' <summary>
        ''' Construye una serie de barras y la agrega a la colección
        ''' Utilizado para gráficos de barras
        ''' </summary>
        Public Shared Sub AddBarSeries(ByVal collection As XFSeriesCollection, ByVal seriesName As String, ByVal color As String, ByVal dataPoints As List(Of GraphDataPoint), Optional ByVal useSecondaryAxis As Boolean = False, Optional ByVal useStacked As Boolean = False)
            Dim series As New XFSeries()
            series.UniqueName = seriesName
            series.Type = If(useStacked, XFChart2SeriesType.BarStacked, XFChart2SeriesType.BarSideBySide)
            series.Color = color
            series.UseSecondaryAxis = useSecondaryAxis
            series.ModelType = XFChart2ModelType.Bar2DBorderlessSimple
            series.BarWidth = 0.8

            For Each dataPoint In dataPoints
                Dim point As New XFSeriesPoint()
                point.Argument = dataPoint.DateLabel
                point.Value = Convert.ToDouble(dataPoint.Amount)
                series.AddPoint(point)
            Next

            collection.Series.Add(series)
        End Sub

        ''' <summary>
        ''' Construye una serie de línea con eje secundario y la agrega a la colección
        ''' Utilizado para métricas complementarias en gráficos combinados
        ''' </summary>
        Public Shared Sub AddLineSeriesSecondaryAxis(ByVal collection As XFSeriesCollection, ByVal seriesName As String, ByVal color As String, ByVal dataPoints As List(Of GraphDataPoint), Optional ByVal useSecondaryAxis As Boolean = True, Optional ByVal useConditionalColors As Boolean = False, Optional ByVal positiveColor As String = "", Optional ByVal negativeColor As String = "#FFCC0000")
            Dim series As New XFSeries()
            series.UniqueName = seriesName
            series.Type = XFChart2SeriesType.Line
            series.Color = color
            series.LineThickness = LINE_THICKNESS
            series.ShowMarkers = 1
            series.MarkerSize = MARKER_SIZE
            series.LineStyleType = XFChart2LineStyleType.Solid
            series.UseSecondaryAxis = useSecondaryAxis

            ' Si no se especifica color positivo, usar verde por defecto
            Dim colorPositivo As String = If(String.IsNullOrEmpty(positiveColor), "#FF70AD47", positiveColor)

            For Each dataPoint In dataPoints
                Dim point As New XFSeriesPoint()
                point.Argument = dataPoint.DateLabel
                point.Value = Convert.ToDouble(dataPoint.Amount)

                ' Aplicar colores condicionales si está habilitado
                If useConditionalColors Then
                    If dataPoint.Amount >= 0 Then
                        point.Color = colorPositivo  ' Verde para valores positivos o cero
                    Else
                        point.Color = negativeColor  ' Rojo para valores negativos
                    End If
                End If

                series.AddPoint(point)
            Next

            collection.Series.Add(series)
        End Sub

    End Class

    ''' <summary>
    ''' Error handler for logging exceptions
    ''' </summary>
    Public Class ErrorHandler
        Public Shared Function LogWrite(ByVal si As SessionInfo, ByVal ex As Exception) As Exception
            Return ex
        End Function
    End Class
End Namespace
