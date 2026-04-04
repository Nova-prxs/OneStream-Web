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


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.queries
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						
					Case Is = DashboardDataSetFunctionType.GetDataSet
							
						Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
						Dim QueriesBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.all_queries
						Dim sql As String = String.Empty
						Dim dt As DataTable = Nothing
						
						
						' VARIABLES													
						Dim sFactory 	As String = args.NameValuePairs("factory")
						' Dim sYear 		As String = args.NameValuePairs("year")
						Dim sProduct 	As String = args.NameValuePairs("product")	
						Dim sGM			As String = args.NameValuePairs.GetValueOrEmpty("gm")	
						Dim sCC			As String = args.NameValuePairs.GetValueOrEmpty("cc")	

						Dim sScenarioRef As String = args.NameValuePairs.GetValueOrEmpty("scenarioref")
				
						Dim sTime As String = args.NameValuePairs.GetValueOrDefault("time", "UO2")
						Dim sTimeUnit As String = args.NameValuePairs.GetValueOrDefault("timeUnit", "h")
						
						Dim parameters As New Dictionary(Of String, String)  								
						parameters.Add("Factory", sFactory)
						parameters.Add("Product", sProduct)
						parameters.Add("GM", sGM)
						parameters.Add("CC", sCC)
						parameters.Add("ScenarioRef", sScenarioRef)
						parameters.Add("Time", sTime)
						parameters.Add("TimeUnit", sTimeUnit)
						
						#Region "Colores"
							Dim pastelColor As Integer = 0
							Dim coloresPastel As New List(Of String) From {
							    "#FFFF8C8C",    ' Rojo suave más vivo
							    "#FFFFB579",    ' Naranja suave más vivo
							    "#FFFFD85D",    ' Amarillo suave más vivo
							    "#FFFFA3C1",    ' Rosa suave más vivo
							    "#FFFF9F7D",    ' Coral suave más vivo
							    "#FFFFA3A3",    ' Rojo suave
							    "#FFFFCBA3",    ' Naranja suave
							    "#FFFFFFA3",    ' Amarillo suave
							    "#FFFFB3C1",    ' Rosa suave
							    "#FFFFBFA3"     ' Coral suave
							}
							Dim frioColor As Integer = 0
							Dim coloresFrio As New List(Of String) From {
							    "#FF9E5CFF",    ' Morado suave más vivo
							    "#FFB983FF",    ' Lila suave más vivo
							    "#FF6BC8FF",    ' Azul cielo suave más vivo
							    "#FF8FD9FF",    ' Celeste claro más vivo
							    "#FF9B69FF",     ' Violeta suave más vivo
							    "#FFCAA3FF",    ' Morado suave
							    "#FFD7A3FF",    ' Lila suave
							    "#FFA3CFFF",    ' Azul cielo suave
							    "#FFA3E0FF",    ' Celeste claro
							    "#FFB3A3FF"     ' Violeta suave
							}
							
							Dim coloresCalidosACT As New List(Of String) From {
							    "#FFFF5733",   ' Rojo–naranja intenso
							    "#FFFF8C00",   ' Naranja vivo
							    "#FFFFBD00",   ' Amarillo fuerte
							    "#FFFF6F61",   ' Coral intenso
							    "#FFE63946",   ' Rojo vivo
							    "#FFFFA500"    ' Naranja clásico vibrante
							}
							
							Dim coloresCalidosREF As New List(Of String) From {
							    "#FFFFB3A3",   ' Rojo pastel
							    "#FFFFD1A3",   ' Naranja pastel
							    "#FFFFE7A3",   ' Amarillo pastel
							    "#FFFFC4B3",   ' Coral pastel
							    "#FFFFCC99",   ' Melocotón suave
							    "#FFFFE0B2"    ' Beige cálido
							}
							Dim coloresFriosACT As New List(Of String) From {
							    "#FF0077FF",   ' Azul intenso
							    "#FF00C2FF",   ' Cian brillante
							    "#FF00D18A",   ' Verde turquesa vivo
							    "#FF7A33FF",   ' Violeta intenso
							    "#FF4A90E2",   ' Azul eléctrico
							    "#FF009688"    ' Verde frío vivo
							}
							Dim coloresFriosREF As New List(Of String) From {
							    "#FFA3CFFF",   ' Azul cielo pastel
							    "#FFB3E6FF",   ' Celeste pastel
							    "#FFA3F5D3",   ' Verde turquesa suave
							    "#FFD1B3FF",   ' Lavanda pastel
							    "#FFB3D4FF",   ' Azul hielo
							    "#C8E6C9"    ' Verde suave frío
							}
							
							#End Region
							
						If args.DataSetName.XFEqualsIgnoreCase("Example") Then

							sql = $"
								SELECT 'Example' as Message
							"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dt = BRApi.Database.ExecuteSql(dbConn, sql, True)
							End Using	

							Return dt
							
						#Region "UO1&UO2 - Allocations"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Reporting_UO2") Then
							
							Dim returnTable As String = args.NameValuePairs.GetValueOrEmpty("returnTable")
							
							sql = If( sScenarioRef <> "Actual", QueriesBR.Query("AllocationsReference", parameters), QueriesBR.Query("AllocationsActual", parameters))
							dt = ExecuteSQL(si, sql)
							
							If sProduct = "All" Then Return Nothing
								
							If returnTable = "Si" Then Return dt 
									Return If (sScenarioRef<> "Actual", CreateLineGraph(si, dt, coloresCalidosREF, coloresFriosREF),  CreateLineGraph(si, dt, coloresCalidosACT, coloresFriosACT))
							
							#Region "Construcción del gráfico"
							
'							Dim seriesCollection As New XFSeriesCollection()
'							Dim seriesDict As New Dictionary(Of String, Integer)	
							
'							If sProduct <> "All" Then ' Construcción del gráfico para el escenario de referencia
									
'								For Each row As DataRow In dt.Rows
									
'									Dim seriesKey As String = row("Scenario") & ": " & row("id_averagegroup") & " - " & row("uotype")
									
'									Dim point As New XFSeriesPoint()
									
'									point.Argument = row("all_dates") ' DateTime.ParseExact(row("start_date").ToString,"d/M/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture) ' row("start_date") ' DateTime.ParseExact(row("start_date"),"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
'									point.Value = Math.Round(Convert.ToDouble(row("value")),2)
'									point.ParameterValue = row("id_costcenter")
									
'									If seriesDict.Keys.Contains(seriesKey)
										
'										seriesCollection.Series(seriesDict(seriesKey)).AddPoint(point)
										
'									Else
'										Dim series As New XFSeries()
'										series.UniqueName = seriesKey
'										series.Type = XFChart2SeriesType.LineStep
'										series.LineStyleType = XFChart2LineStyleType.Solid
'										series.LineThickness = 2
'										series.ShowMarkers = True
'										series.MarkerSize = 7		
'										series.Color = coloresPastel(pastelColor)
'										pastelColor += 1								
'										series.AddPoint(point)
'										seriesCollection.AddSeries(series)
'										seriesDict.Add(seriesKey, seriesDict.Keys.Count)
'									End If
									
'								Next
'							End If
							
'							Return seriesCollection.CreateDataSet(si)
							
							#End Region
							
						#End Region		
							
						#Region "UO1&UO2 - Allocations - GM"
						
						Else If args.DataSetName.XFEqualsIgnoreCase("Reporting_UO2_GM") Then
							
							Dim returnTable As String = args.NameValuePairs.GetValueOrEmpty("returnTable")

							sql = QueriesBR.Query("AllocationsActual_GM", parameters)
							dt = ExecuteSQL(si, sql)
							
							If returnTable = "Si" Then Return dt 
							
							Return CreateLineGraph(si, dt, coloresCalidosACT, coloresFriosACT)
							
							#Region "Construcción del gráfico"

'							Dim seriesCollection As New XFSeriesCollection()
'							Dim seriesDict As New Dictionary(Of String, Integer)	
							
'							If sProduct <> "All" Then 
									
'								For Each row As DataRow In dt.Rows
									
'									Dim seriesKey As String = row("Scenario") & ": " & row("id_averagegroup") & " - " & row("uotype")
									
'									Dim point As New XFSeriesPoint()
									
'									point.Argument = row("all_dates") ' DateTime.ParseExact(row("start_date").ToString,"d/M/yyyy H:mm:ss", System.Globalization.CultureInfo.InvariantCulture) ' row("start_date") ' DateTime.ParseExact(row("start_date"),"dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)
'									point.Value = Math.Round(Convert.ToDouble(row("value")),2)
'									point.ParameterValue = $"{row("all_dates")}{vbCrLf}{row("comment")}"
									
'									If seriesDict.Keys.Contains(seriesKey)
										
'										seriesCollection.Series(seriesDict(seriesKey)).AddPoint(point)
										
'									Else
'										Dim series As New XFSeries()
'										series.UniqueName = seriesKey
'										series.Type = XFChart2SeriesType.LineStep
'										series.LineStyleType = XFChart2LineStyleType.Solid
'										series.LineThickness = 3
'										series.ShowMarkers = True
'										series.MarkerSize = 7									
'										If sTime = "UO1"
'											series.Color = "#FFFF8C8C"
'											pastelColor += 1
'										Else
'											series.Color = "#FF6BC8FF"
'											frioColor += 1
'										End If										
'										series.AddPoint(point)
'										seriesCollection.AddSeries(series)
'										seriesDict.Add(seriesKey, seriesDict.Keys.Count)
'									End If
									
'								Next
'							End If
							
'							Return seriesCollection.CreateDataSet(si)
							
							#End Region								
							
							#End Region		
							
						Else If args.DataSetName.XFEqualsIgnoreCase("CreateHistoricalData") Then

						End If
						
					End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Function ExecuteSQL(ByVal si As SessionInfo, ByVal sqlCmd As String)
			Dim dt As DataTable = Nothing							
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				dt = BRApi.Database.ExecuteSql(dbConn, sqlCmd, True)
			End Using
			Return dt				
		End Function
		
		Private Function CreateLineGraph(ByVal si As SessionInfo, ByVal dt As DataTable, ByVal colorUO1 As List(Of String), ByVal colorUO2 As List(Of String))
			Dim colorNum1 As Integer = 0
			Dim colorNum2 As Integer = 0
            							
			Dim seriesCollection As New XFSeriesCollection()
			Dim seriesDict As New Dictionary(Of String, Integer)	
			
			For Each row As DataRow In dt.Rows
                
                Dim seriesKey As String = row("Scenario") & ": " & row("id_averagegroup") & " - " & row("uotype")
                
                Dim point As New XFSeriesPoint()
                
                point.Argument = row("all_dates")
                point.Value = Math.Round(Convert.ToDouble(row("value")),4)				
                point.ParameterValue =  If(row.Table.Columns.Contains("comment"), $"{row("all_dates")}{vbCrLf}{row("comment")}", "")
                
                If seriesDict.Keys.Contains(seriesKey)
                    
                    seriesCollection.Series(seriesDict(seriesKey)).AddPoint(point)
                    
                Else
                    Dim series As New XFSeries()
                    series.UniqueName = seriesKey
                    series.Type = XFChart2SeriesType.LineStep
                    series.LineStyleType = XFChart2LineStyleType.Solid
                    series.LineThickness = 3
                    series.ShowMarkers = True
                    series.MarkerSize = 7									
                    If row("uotype") = "UO1"
                        series.Color = colorUO1(colorNum1)
                        colorNum1 += 1
                    Else
                        series.Color = colorUO2(colorNum2)
                        colorNum2 += 1
                    End If										
                    series.AddPoint(point)
                    seriesCollection.AddSeries(series)
                    seriesDict.Add(seriesKey, seriesDict.Keys.Count)
                End If
                
            Next

            Return seriesCollection.CreateDataSet(si)

        End Function
		
	End Class
End Namespace
