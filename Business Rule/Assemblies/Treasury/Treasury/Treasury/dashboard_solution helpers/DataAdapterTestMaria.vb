
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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DataAdapterTestMaria
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		' Constants for colors (centralized)
		Private Const COLOR_BLUE As String = "#FF4472C4"
		Private Const COLOR_ORANGE As String = "#FFED7D31"
		Private Const LINE_THICKNESS As Integer = 3
		Private Const MARKER_SIZE As Integer = 5
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						Dim names As New List(Of String)()
						names.Add("CashBalanceGraph")
						Return names

					Case Is = DashboardDataSetFunctionType.GetDataSet
						If args.DataSetName.XFEqualsIgnoreCase("CashBalanceGraph") Then
							Return Me.GetCashBalanceGraph(si, args)
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Classes"
		
		''' <summary>
		''' Estructura de datos simple para puntos de gráfico
		''' </summary>
		Private Class GraphDataPoint
			Public WeekNumber As Integer
			Public DateLabel As String
			Public Amount As Decimal
		End Class
		
		#End Region
		
		#Region "Helper Methods"
		
		''' <summary>
		''' Valida y extrae un parámetro entero requerido del diccionario
		''' Lanza excepción si no existe o no es válido
		''' </summary>
		Private Function GetRequiredIntParameter(ByVal si As SessionInfo, ByVal paramDict As Dictionary(Of String, String), ByVal paramName As String) As Integer
			Try
				If Not paramDict.ContainsKey(paramName) Then
					Throw New ArgumentException($"Parámetro requerido '{paramName}' no encontrado")
				End If
				
				Dim paramValue As String = paramDict(paramName)
				If Not Integer.TryParse(paramValue, CInt(0)) Then
					Throw New ArgumentException($"Parámetro '{paramName}' debe ser un número válido. Valor: {paramValue}")
				End If
				
				Return Convert.ToInt32(paramValue)
				
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"ERROR GetRequiredIntParameter: {ex.Message}")
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Construye filtro de entidades seguro contra inyección SQL
		''' Devuelve string vacío si no hay filtro
		''' Los parámetros se agregan a la lista dbParams
		''' </summary>
		Private Function BuildEntityFilter(ByVal si As SessionInfo, ByVal companyNames As String, ByRef dbParams As List(Of DbParamInfo)) As String
			Dim entityFilter As String = ""
			
			' Si es "Any", "0", o vacío -> no filtrar
			If String.IsNullOrEmpty(companyNames) OrElse _
			   companyNames.XFEqualsIgnoreCase("Any") OrElse _
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
		Private Sub AddLineSeries(ByVal collection As XFSeriesCollection, ByVal seriesName As String, ByVal color As String, ByVal dataPoints As List(Of GraphDataPoint))
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
	Private Sub AddBarSeries(ByVal collection As XFSeriesCollection, ByVal seriesName As String, ByVal color As String, ByVal dataPoints As List(Of GraphDataPoint), Optional ByVal paneIndex As Integer = 0)
		Dim series As New XFSeries()
		series.UniqueName = seriesName
		series.Type = XFChart2SeriesType.BarSideBySide
		series.Color = color
		series.UseSecondaryAxis = (paneIndex > 0)
		
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
	Private Sub AddLineSeriesSecondaryAxis(ByVal collection As XFSeriesCollection, ByVal seriesName As String, ByVal color As String, ByVal dataPoints As List(Of GraphDataPoint), Optional ByVal paneIndex As Integer = 1)
		Dim series As New XFSeries()
		series.UniqueName = seriesName
		series.Type = XFChart2SeriesType.Line
		series.Color = color
		series.LineThickness = LINE_THICKNESS
		series.ShowMarkers = 1
		series.MarkerSize = MARKER_SIZE
		series.LineStyleType = XFChart2LineStyleType.Solid
		series.UseSecondaryAxis = (paneIndex > 0)
		
		For Each dataPoint In dataPoints
			Dim point As New XFSeriesPoint()
			point.Argument = dataPoint.DateLabel
			point.Value = Convert.ToDouble(dataPoint.Amount)
			series.AddPoint(point)
		Next
		
		collection.Series.Add(series)
	End Sub
		
		#End Region
		
		#Region "CashBalanceGraph"
		
		''' <summary>
		''' Genera gráfico de líneas comparativo de Cash Balance entre dos años
		''' Filtra por semanas, entidades y escenario
		''' </summary>
		Private Function GetCashBalanceGraph(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
			Try
				' ===================================================================
				' 1. OBTENER Y VALIDAR PARÁMETROS DEL DASHBOARD
				' ===================================================================
				Dim queryParams As String = args.NameValuePairs("p_query_params")
				Dim parameterDict As Dictionary(Of String, String) = 
					UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
				
				' Validar y extraer parámetros requeridos
				Dim treasuryYear As Integer = Me.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year")
				Dim treasuryYearToCompare As Integer = Me.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_Year_To_Compare")
				Dim weekNumberFrom As Integer = Me.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_from")
				Dim weekNumberTo As Integer = Me.GetRequiredIntParameter(si, parameterDict, "prm_Treasury_WeekNumber_to")
				Dim companyNames As String = parameterDict.GetValueOrDefault("prm_Treasury_CompanyNames", "")
				
				' Log para debugging
				BRApi.ErrorLog.LogMessage(si, $"DEBUG CashBalanceGraph: Year={treasuryYear}, CompareYear={treasuryYearToCompare}, Weeks={weekNumberFrom}-{weekNumberTo}, Companies={companyNames}")
				
				' ===================================================================
				' 2. CONSTRUIR CONSULTA SQL CON PARÁMETROS (SEGURO CONTRA INYECCIÓN)
				' ===================================================================
				Dim dbParams As New List(Of DbParamInfo)()
				Dim entityFilter As String = Me.BuildEntityFilter(si, companyNames, dbParams)
				
				dbParams.Insert(0, New DbParamInfo("@weekFrom", weekNumberFrom))
				dbParams.Insert(1, New DbParamInfo("@weekTo", weekNumberTo))
				dbParams.Insert(2, New DbParamInfo("@year1", treasuryYear))
				dbParams.Insert(3, New DbParamInfo("@year2", treasuryYearToCompare))
				
				Dim sql As String = $"
					WITH CashBalanceData AS (
						SELECT 
							m.ProjectionYear,
							m.ProjectionWeekNumber,
							d.weekStartDate,
							SUM(m.Amount) AS TotalAmount
						FROM XFC_TRS_Master_CashDebtPosition m
						INNER JOIN XFC_TRS_AUX_Date d
							ON CONVERT(VARCHAR(8), d.weekStartDate, 112) = m.ProjectionTimekey
							AND d.fulldate = d.weekStartDate
						WHERE 
							m.Account = 'CASH AND FINANCING BALANCE'
							AND m.ProjectionType = 'StartWeek'
							AND m.ProjectionWeekNumber BETWEEN @weekFrom AND @weekTo
							AND m.ProjectionYear IN (@year1, @year2)
							{entityFilter}
						GROUP BY 
							m.ProjectionYear,
							m.ProjectionWeekNumber,
							d.weekStartDate
					)
			SELECT 
				ProjectionYear,
				ProjectionWeekNumber,
				weekStartDate,
				TotalAmount,
				'Week ' + RIGHT('0' + CAST(ProjectionWeekNumber AS VARCHAR(2)), 2) AS DateLabel
			FROM CashBalanceData
			ORDER BY ProjectionYear, ProjectionWeekNumber
				"
				
				' Log SQL para debugging
				BRApi.ErrorLog.LogMessage(si, $"DEBUG CashBalanceGraph SQL executed with {dbParams.Count} parameters")
				
				' ===================================================================
				' 3. EJECUTAR CONSULTA Y CARGAR DATOS
				' ===================================================================
				Dim rawData As DataTable = Nothing
				
				Using dbConnInfo As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Try
						rawData = BRApi.Database.ExecuteSql(dbConnInfo, sql, dbParams, True)
						
						If rawData Is Nothing OrElse rawData.Rows.Count = 0 Then
							BRApi.ErrorLog.LogMessage(si, "WARNING CashBalanceGraph: No data returned from query")
							Return Nothing
						End If
						
						BRApi.ErrorLog.LogMessage(si, $"DEBUG CashBalanceGraph: Retrieved {rawData.Rows.Count} rows")
						
					Catch ex As Exception
						BRApi.ErrorLog.LogMessage(si, $"ERROR executing SQL: {ex.Message}")
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
					BRApi.ErrorLog.LogMessage(si, $"WARNING: Missing data for comparison. Year {treasuryYear}: {If(dataByYear.ContainsKey(treasuryYear), "OK", "MISSING")}, Year {treasuryYearToCompare}: {If(dataByYear.ContainsKey(treasuryYearToCompare), "OK", "MISSING")}")
				End If
				
			' Ordenar datos: primero por año, luego por semana dentro de cada año
			' Crear diccionario ordenado por año
			Dim sortedDataByYear = dataByYear.OrderBy(Function(x) x.Key).ToDictionary(Function(x) x.Key, Function(x) x.Value)
			
			' Ordenar cada año por número de semana
			For Each kvp In sortedDataByYear
				kvp.Value.Sort(Function(a, b) a.WeekNumber.CompareTo(b.WeekNumber))
			Next
			
			' Reemplazar dataByYear con los datos ordenados
			dataByYear = sortedDataByYear
			
			' ===================================================================
			' 5. CREAR DOS GRÁFICOS SEPARADOS (UNO AZUL, UNO NARANJA)
			' ===================================================================
			
			' Combinar todos los datos en una sola lista ordenada por año y semana
			Dim allDataPoints As New List(Of GraphDataPoint)()
			For Each kvp In sortedDataByYear
				For Each dataPoint In kvp.Value
					allDataPoints.Add(dataPoint)
				Next
			Next
			
			' Crear dos series separadas - una para cada año
			Dim oSeriesCollection As New XFSeriesCollection()
			
			' Serie 1: Año de comparación (color AZUL)
			' Contiene solo los datos del año de comparación
			Dim serieAzul As New XFSeries()
			serieAzul.UniqueName = treasuryYearToCompare.ToString()
			serieAzul.Type = XFChart2SeriesType.Line
			serieAzul.Color = COLOR_BLUE
			serieAzul.LineThickness = LINE_THICKNESS
			serieAzul.ShowMarkers = 1
			serieAzul.MarkerSize = MARKER_SIZE
			serieAzul.LineStyleType = XFChart2LineStyleType.Solid
			
			' Agregar puntos del año de comparación
			If dataByYear.ContainsKey(treasuryYearToCompare) Then
				For Each dataPoint In dataByYear(treasuryYearToCompare)
					Dim labelConAno As String = dataPoint.DateLabel & " "
					Dim point As New XFSeriesPoint()
					point.Argument = labelConAno
					point.Value = Convert.ToDouble(dataPoint.Amount)
					serieAzul.AddPoint(point)
				Next
			End If
			
			oSeriesCollection.Series.Add(serieAzul)
			
			' Serie 2: Año actual (color NARANJA)
			' Contiene solo los datos del año actual
			Dim serieNaranja As New XFSeries()
			serieNaranja.UniqueName = treasuryYear.ToString()
			serieNaranja.Type = XFChart2SeriesType.Line
			serieNaranja.Color = COLOR_ORANGE
			serieNaranja.LineThickness = LINE_THICKNESS
			serieNaranja.ShowMarkers = 1
			serieNaranja.MarkerSize = MARKER_SIZE
			serieNaranja.LineStyleType = XFChart2LineStyleType.Solid
			
			' Agregar puntos del año actual
			If dataByYear.ContainsKey(treasuryYear) Then
				For Each dataPoint In dataByYear(treasuryYear)
					Dim labelConAno As String = dataPoint.DateLabel 
					Dim point As New XFSeriesPoint()
					point.Argument = labelConAno
					point.Value = Convert.ToDouble(dataPoint.Amount)
					serieNaranja.AddPoint(point)
				Next
			End If
			
			oSeriesCollection.Series.Add(serieNaranja)
			
			BRApi.ErrorLog.LogMessage(si, $"DEBUG CashBalanceGraph: Created {oSeriesCollection.Series.Count} series with year labels")
			
			' ===================================================================
			' 6. RETORNAR DATASET PARA EL DASHBOARD
			' ===================================================================
			'✅ VALIDACIÓN: XFSeriesCollectionExtensions.CreateDataSet verificado en OneStream.Shared.Wcf
			Return oSeriesCollection.CreateDataSet(si)
				
			Catch ex As Exception
				BRApi.ErrorLog.LogMessage(si, $"ERROR CashBalanceGraph: {ex.Message}")
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
	End Class
End Namespace