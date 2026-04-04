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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.CubeViewGraphs
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
									
					Case Is = DashboardDataSetFunctionType.GetDataSet
							' Get configuration colors and comparison scenario
						    Dim primaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Primary")
						    Dim secondaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Secondary")
						    Dim terciaryColor As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Terciary")
						    Dim green As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Green")
						    Dim red As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Shared.prm_Style_Colors_Red")
						    Dim scenarioComparison As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "LPW.prm_LPW_Cash_Scenario_Graph")
							
					
					#Region "Overdues Comparison"
					
					If args.DataSetName.XFEqualsIgnoreCase("OverduesComparison") Then
					    Try
					        Dim queryParams As String = args.NameValuePairs("p_query_params")
					        Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
					        Dim povEntity As String = parameterDict("paramEntity")
					        Dim povScenario As String = parameterDict("paramScenario")
					        Dim povView As String = parameterDict("paramView")
					
					        Dim resultTable As New DataTable("OverduesComparisonSourceData")
					        resultTable.Columns.Add("Month", GetType(String))
					        resultTable.Columns.Add("Matured2024", GetType(Object))
					        resultTable.Columns.Add("Matured2025", GetType(Object))
					        resultTable.Columns.Add("Percent2024", GetType(Object))
					        resultTable.Columns.Add("Percent2025", GetType(Object))
					
					        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture("es-ES")
					
					        For i As Integer = 1 To 12
					            ' Usamos el prefijo numérico para forzar el orden
					            Dim monthString As String = i.ToString("00") & "-" & New DateTime(2024, i, 1).ToString("MMM", culture).ToUpper()
					
					            ' Obtener todos los valores
					            Dim matured2024 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
					            Dim totalMaturity2024 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
					            Dim percentage2024 As Decimal = If(totalMaturity2024 <> 0, (matured2024 / totalMaturity2024) * 100, 0)
					
					            Dim matured2025 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
					            Dim totalMaturity2025 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
					            Dim percentage2025 As Decimal = If(totalMaturity2025 <> 0, (matured2025 / totalMaturity2025) * 100, 0)
								
					            resultTable.Rows.Add(
					                monthString,
					                If(matured2024 <> 0, CObj(matured2024), DBNull.Value),
					                If(matured2025 <> 0, CObj(matured2025), DBNull.Value),
					                If(percentage2024 <> 0, CObj(percentage2024), DBNull.Value),
					                If(percentage2025 <> 0, CObj(percentage2025), DBNull.Value)
					            )
					        Next
					
					        ' PARTE 2: CONSTRUCCIÓN DE SERIES
					        Dim oSeriesCollection As New XFSeriesCollection()
					        Dim orangeColor As String = "#FFf37321" 
					        Dim greyColor As String = "#FF6d7f8c"
					
					        oSeriesCollection.Series.Add(Me.CreateSeries(si, "Matured 2024 (€ Million) ", XFChart2SeriesType.BarSideBySide, greyColor, resultTable, "Matured2024", "Month", barWidth:=0.9, modelType:=XFChart2ModelType.Bar2DBorderlessSimple))
					        oSeriesCollection.Series.Add(Me.CreateSeries(si, "Matured 2025 (€ Million) ", XFChart2SeriesType.BarSideBySide, orangeColor, resultTable, "Matured2025", "Month", barWidth:=0.9, modelType:=XFChart2ModelType.Bar2DBorderlessSimple))
					        'oSeriesCollection.Series.Add(Me.CreateSeries(si, "% Overdues 2024", XFChart2SeriesType.Line, greyColor, resultTable, "Percent2024", "Month",useSecondaryAxis:=True,  showMarkers:=True, markerSize:=6, lineThickness:=2))
					        'oSeriesCollection.Series.Add(Me.CreateSeries(si, "% Overdues 2025", XFChart2SeriesType.Line, orangeColor, resultTable, "Percent2025", "Month",useSecondaryAxis:=True,  showMarkers:=True, markerSize:=6, lineThickness:=2))
					
					        Return oSeriesCollection.CreateDataSet(si)
					
					    Catch ex As Exception
					        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
					    End Try
					
					#End Region
					
					#Region "Overdues Comparison Percentage"
					
				Else If args.DataSetName.XFEqualsIgnoreCase("OverduesComparisonPercentage") Then
					    Try
					        Dim queryParams As String = args.NameValuePairs("p_query_params")
					        Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
					        Dim povEntity As String = parameterDict("paramEntity")
					        Dim povScenario As String = parameterDict("paramScenario")
					        Dim povView As String = parameterDict("paramView")
					
					        Dim resultTable As New DataTable("OverduesComparisonSourceData")
					        resultTable.Columns.Add("Month", GetType(String))
					        resultTable.Columns.Add("Matured2024", GetType(Object))
					        resultTable.Columns.Add("Matured2025", GetType(Object))
					        resultTable.Columns.Add("Percent2024", GetType(Object))
					        resultTable.Columns.Add("Percent2025", GetType(Object))
					
					        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture("es-ES")
					
					        For i As Integer = 1 To 12
					            ' Usamos el prefijo numérico para forzar el orden
					            Dim monthString As String = i.ToString("00") & "-" & New DateTime(2024, i, 1).ToString("MMM", culture).ToUpper()
					
					            ' Obtener todos los valores
					            Dim matured2024 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
					            Dim totalMaturity2024 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
					            Dim percentage2024 As Decimal = If(totalMaturity2024 <> 0, (matured2024 / totalMaturity2024) * 100, 0)
					
					            Dim matured2025 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
					            Dim totalMaturity2025 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
					            Dim percentage2025 As Decimal = If(totalMaturity2025 <> 0, (matured2025 / totalMaturity2025) * 100, 0)
								
					            resultTable.Rows.Add(
					                monthString,
					                If(matured2024 <> 0, CObj(matured2024), DBNull.Value),
					                If(matured2025 <> 0, CObj(matured2025), DBNull.Value),
					                If(percentage2024 <> 0, CObj(percentage2024), DBNull.Value),
					                If(percentage2025 <> 0, CObj(percentage2025), DBNull.Value)
					            )
					        Next
					
					        ' PARTE 2: CONSTRUCCIÓN DE SERIES
					        Dim oSeriesCollection As New XFSeriesCollection()
					        Dim orangeColor As String = "#FFf37321" 
					        Dim greyColor As String = "#FF6d7f8c"
					
					        'oSeriesCollection.Series.Add(Me.CreateSeries(si, "Matured 2024", XFChart2SeriesType.BarSideBySide, greyColor, resultTable, "Matured2024", "Month", barWidth:=0.7, modelType:=XFChart2ModelType.Bar2DBorderlessSimple))
					        'oSeriesCollection.Series.Add(Me.CreateSeries(si, "Matured 2025", XFChart2SeriesType.BarSideBySide, orangeColor, resultTable, "Matured2025", "Month", barWidth:=0.7, modelType:=XFChart2ModelType.Bar2DBorderlessSimple))
					        oSeriesCollection.Series.Add(Me.CreateSeries(si, "% Overdues 2024    ", XFChart2SeriesType.Line, greyColor, resultTable, "Percent2024", "Month", showMarkers:=True, markerSize:=6, lineThickness:=2))
					        oSeriesCollection.Series.Add(Me.CreateSeries(si, "% Overdues 2025    ", XFChart2SeriesType.Line, orangeColor, resultTable, "Percent2025", "Month",showMarkers:=True, markerSize:=6, lineThickness:=2))
					        Return oSeriesCollection.CreateDataSet(si)
					
					    Catch ex As Exception
					        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
					    End Try
					
					#End Region
					
					#Region "Overdues Comparison Percentage Test"

Else If args.DataSetName.XFEqualsIgnoreCase("OverduesComparisonPercentage_test") Then
    Try
        Dim queryParams As String = args.NameValuePairs("p_query_params")
        Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
        Dim povEntity As String = parameterDict("paramEntity")
        Dim povScenario As String = parameterDict("paramScenario")
        Dim povView As String = parameterDict("paramView")

        Dim resultTable As New DataTable("OverduesComparisonSourceData")
        resultTable.Columns.Add("Month", GetType(String))
        resultTable.Columns.Add("Matured2024", GetType(Object))
        resultTable.Columns.Add("Matured2025", GetType(Object))
        resultTable.Columns.Add("Percent2024", GetType(Object))
        resultTable.Columns.Add("Percent2025", GetType(Object))

        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture("es-ES")

        For i As Integer = 1 To 12
            ' Usamos el prefijo numérico para forzar el orden
            Dim monthString As String = i.ToString("00") & "-" & New DateTime(2024, i, 1).ToString("MMM", culture).ToUpper()

            ' Obtener todos los valores
            Dim matured2024 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
            Dim totalMaturity2024 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
            Dim percentage2024 As Decimal = If(totalMaturity2024 <> 0, (matured2024 / totalMaturity2024) * 100, 0)

            Dim matured2025 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
            Dim totalMaturity2025 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
            Dim percentage2025 As Decimal = If(totalMaturity2025 <> 0, (matured2025 / totalMaturity2025) * 100, 0)
            
            resultTable.Rows.Add(
                monthString,
                If(matured2024 <> 0, CObj(matured2024), DBNull.Value),
                If(matured2025 <> 0, CObj(matured2025), DBNull.Value),
                If(percentage2024 <> 0, CObj(percentage2024), DBNull.Value),
                If(percentage2025 <> 0, CObj(percentage2025), DBNull.Value)
            )
        Next

        ' PARTE 2: CONSTRUCCIÓN DE SERIES - Solución simplificada sin solapamiento
        Dim oSeriesCollection As New XFSeriesCollection()
        Dim orangeColor As String = "#FFf37321" 
        Dim greyColor As String = "#FF6d7f8c"

        ' Serie 2024 - línea principal
        Dim series2024 As XFSeries = Me.CreateSeries(si, "% Overdues 2024", XFChart2SeriesType.Line, greyColor, resultTable, "Percent2024", "Month", showMarkers:=True, markerSize:=6, lineThickness:=2)
        oSeriesCollection.Series.Add(series2024)
        
        ' Serie 2025 - línea principal en eje secundario para separar visualmente las líneas
        Dim series2025 As XFSeries = Me.CreateSeries(si, "% Overdues 2025", XFChart2SeriesType.Line, orangeColor, resultTable, "Percent2025", "Month", useSecondaryAxis:=True, showMarkers:=True, markerSize:=6, lineThickness:=2)
        oSeriesCollection.Series.Add(series2025)
        
        Return oSeriesCollection.CreateDataSet(si)

    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try

#End Region

					#Region "Overdues Comparison Percentage Double"
										
									Else If args.DataSetName.XFEqualsIgnoreCase("OverduesComparisonPercentage_2024") Then
										    Try
										        Dim queryParams As String = args.NameValuePairs("p_query_params")
										        Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
										        Dim povEntity As String = parameterDict("paramEntity")
										        Dim povScenario As String = parameterDict("paramScenario")
										        Dim povView As String = parameterDict("paramView")
										
										        Dim resultTable As New DataTable("OverduesComparisonSourceData")
										        resultTable.Columns.Add("Month", GetType(String))
										        resultTable.Columns.Add("Matured2024", GetType(Object))
										        resultTable.Columns.Add("Matured2025", GetType(Object))
										        resultTable.Columns.Add("Percent2024", GetType(Object))
										        resultTable.Columns.Add("Percent2025", GetType(Object))
										
										        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture("es-ES")
										
										        For i As Integer = 1 To 12
										            ' Usamos el prefijo numérico para forzar el orden
										            Dim monthString As String = i.ToString("00") & "-" & New DateTime(2024, i, 1).ToString("MMM", culture).ToUpper()
										
										            ' Obtener todos los valores
										            Dim matured2024 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
										            Dim totalMaturity2024 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
										            Dim percentage2024 As Decimal = If(totalMaturity2024 <> 0, (matured2024 / totalMaturity2024) * 100, 0)
										
										            Dim matured2025 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
										            Dim totalMaturity2025 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
										            Dim percentage2025 As Decimal = If(totalMaturity2025 <> 0, (matured2025 / totalMaturity2025) * 100, 0)
													
										            resultTable.Rows.Add(
										                monthString,
										                If(matured2024 <> 0, CObj(matured2024), DBNull.Value),
										                If(matured2025 <> 0, CObj(matured2025), DBNull.Value),
										                If(percentage2024 <> 0, CObj(percentage2024), DBNull.Value),
										                If(percentage2025 <> 0, CObj(percentage2025), DBNull.Value)
										            )
										        Next
										
										        ' PARTE 2: CONSTRUCCIÓN DE SERIES
										        Dim oSeriesCollection As New XFSeriesCollection()
										        Dim orangeColor As String = "#FFf37321" 
										        Dim greyColor As String = "#FF6d7f8c"
										
										        oSeriesCollection.Series.Add(Me.CreateSeries(si, "% Overdues 2024    ", XFChart2SeriesType.Line, greyColor, resultTable, "Percent2024", "Month", showMarkers:=True, markerSize:=6, lineThickness:=2))
										        Return oSeriesCollection.CreateDataSet(si)
										
										    Catch ex As Exception
										        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
										    End Try
											
											Else If args.DataSetName.XFEqualsIgnoreCase("OverduesComparisonPercentage_2025") Then
										    Try
										        Dim queryParams As String = args.NameValuePairs("p_query_params")
										        Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
										        Dim povEntity As String = parameterDict("paramEntity")
										        Dim povScenario As String = parameterDict("paramScenario")
										        Dim povView As String = parameterDict("paramView")
										
										        Dim resultTable As New DataTable("OverduesComparisonSourceData")
										        resultTable.Columns.Add("Month", GetType(String))
										        resultTable.Columns.Add("Matured2024", GetType(Object))
										        resultTable.Columns.Add("Matured2025", GetType(Object))
										        resultTable.Columns.Add("Percent2024", GetType(Object))
										        resultTable.Columns.Add("Percent2025", GetType(Object))
										
										        Dim culture As CultureInfo = CultureInfo.CreateSpecificCulture("es-ES")
										
										        For i As Integer = 1 To 12
										            ' Usamos el prefijo numérico para forzar el orden
										            Dim monthString As String = i.ToString("00") & "-" & New DateTime(2024, i, 1).ToString("MMM", culture).ToUpper()
										
										            ' Obtener todos los valores
										            Dim matured2024 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
										            Dim totalMaturity2024 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2024, i) / 1000)
										            Dim percentage2024 As Decimal = If(totalMaturity2024 <> 0, (matured2024 / totalMaturity2024) * 100, 0)
										
										            Dim matured2025 As Decimal = (Me.GetOverduesMaturedValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
										            Dim totalMaturity2025 As Decimal = (Me.GetOverduesTotalMaturityValue(si, povEntity, povScenario, povView, 2025, i) / 1000)
										            Dim percentage2025 As Decimal = If(totalMaturity2025 <> 0, (matured2025 / totalMaturity2025) * 100, 0)
													
										            resultTable.Rows.Add(
										                monthString,
										                If(matured2024 <> 0, CObj(matured2024), DBNull.Value),
										                If(matured2025 <> 0, CObj(matured2025), DBNull.Value),
										                If(percentage2024 <> 0, CObj(percentage2024), DBNull.Value),
										                If(percentage2025 <> 0, CObj(percentage2025), DBNull.Value)
										            )
										        Next
										
										        ' PARTE 2: CONSTRUCCIÓN DE SERIES
										        Dim oSeriesCollection As New XFSeriesCollection()
										        Dim orangeColor As String = "#FFf37321" 
										        Dim greyColor As String = "#FF6d7f8c"
										
						        				oSeriesCollection.Series.Add(Me.CreateSeries(si, "% Overdues 2025    ", XFChart2SeriesType.Line, orangeColor, resultTable, "Percent2025", "Month",showMarkers:=True, markerSize:=6, lineThickness:=2))
										        Return oSeriesCollection.CreateDataSet(si)
										
										    Catch ex As Exception
										        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
										    End Try
										
								#End Region
					
					
					#Region "Old"
					
					Else If (args.DataSetName.XFEqualsIgnoreCase("CNS_SUM_Dashboard_PL_HorseGroup")) Then
							Dim oSeriesCollection As New XFSeriesCollection()
							
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "LandingPage", "da_Revenues_EBIT_Monthly_Graph_CV",
							xfdataRowListType.RowIndexList, "0,1", XFChartCubeViewDataPointLegendType.Default, False,
							XFChart2seriesType.BarSideBySide, args.CustomSubstVars)
							
							For Each oSeries As XFSeries In oSeriesCollection.Series
								' Formato default
								If oSeries.UniqueName.ToLower.Contains("revenue") Then
									oSeries.ModelType = XFChart2ModelType.Bar2DBorderlessSimple
									oSeries.Color = primaryColor
									oSeries.BarWidth = 0.6
									
								Else
									oSeries.Type = XFChart2SeriesType.Line
									oSeries.UseSecondaryAxis = True
									oSeries.Color = secondaryColor
									oSeries.ShowMarkers = 1
									oSeries.LineThickness = 3
									oSeries.MarkerSize = 8
									oSeries.LineStyleType = XFChart2LineStyleType.Solid
								End If
							Next
							Return oSeriesCollection.CreateDataSet(si)
						
	
						Else If (args.DataSetName.XFEqualsIgnoreCase("CNS_SUM_Dashboard_PL_HorseGroup_test")) Then
							Dim oSeriesCollection As New XFSeriesCollection()
							
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "LandingPage", "da_Revenues_EBIT_Monthly_Graph_CV_Test",
							xfdataRowListType.RowIndexList, "0,1", XFChartCubeViewDataPointLegendType.Default, False,
							XFChart2seriesType.BarSideBySide, args.CustomSubstVars)
							
							For Each oSeries As XFSeries In oSeriesCollection.Series
								' Formato default
								If oSeries.UniqueName.ToLower.Contains("revenues") Then
									oSeries.ModelType = XFChart2ModelType.Bar2DBorderlessSimple
									oSeries.Color = primaryColor
									oSeries.BarWidth = 0.6
								
								Else If oSeries.UniqueName.Contains("INCOME") Then
										oSeries.Type = XFChart2SeriesType.Line
										oSeries.UseSecondaryAxis = True
										oSeries.Color = secondaryColor
										oSeries.ShowMarkers = 1
										oSeries.LineThickness = 3
										oSeries.MarkerSize = 8
										oSeries.LineStyleType = XFChart2LineStyleType.Solid
								
								Else If oSeries.UniqueName.Contains("revenues1") Then
										oSeries.ModelType = XFChart2ModelType.Bar2DBorderlessSimple
										oSeries.Color = secondaryColor
										oSeries.BarWidth = 0.6
									
								Else 
									oSeries.Type = XFChart2SeriesType.Line
									oSeries.UseSecondaryAxis = True
									oSeries.Color = "Transparent"
									oSeries.ShowMarkers = 1
									oSeries.LineThickness = 3
									oSeries.MarkerSize = 8
									oSeries.LineStyleType = XFChart2LineStyleType.Solid
								End If
							Next
							Return oSeriesCollection.CreateDataSet(si)
						
						Else If (args.DataSetName.XFEqualsIgnoreCase("CNS_SUM_Dashboard_PL_Individual")) Then
							Dim oSeriesCollection As New XFSeriesCollection()
							
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "Horse", "da_PLIndividual",
							xfdataRowListType.RowIndexList, "0,1", XFChartCubeViewDataPointLegendType.Default, False,
							XFChart2seriesType.BarSideBySide, args.CustomSubstVars)
							
							For Each oSeries As XFSeries In oSeriesCollection.Series
								' Formato default
								If oSeries.UniqueName.Equals("Revenues") Then
									oSeries.ModelType = XFChart2ModelType.Bar2DBorderlessSimple
									oSeries.Color = "Gray"
									oSeries.BarWidth = 0.8
									
								Else
									oSeries.Type = XFChart2SeriesType.Line
									oSeries.UseSecondaryAxis = True
									oSeries.Color = "CornflowerBlue"
									oSeries.ShowMarkers = 1
									oSeries.LineThickness = 3
									oSeries.MarkerSize = 8
									oSeries.LineStyleType = XFChart2LineStyleType.Solid
								End If
							Next
							Return oSeriesCollection.CreateDataSet(si)
							
						Else If (args.DataSetName.XFEqualsIgnoreCase("OperationalFCF")) Then
							Dim oSeriesCollection As New XFSeriesCollection()
							
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "LandingPage", "da_OperationalFCF_Graph_CV",
							xfdataRowListType.AllRows, "0,1", XFChartCubeViewDataPointLegendType.Default, False,
							XFChart2seriesType.BarSideBySide, args.CustomSubstVars)
							
							For Each oSeries As XFSeries In oSeriesCollection.Series
								
								' Formato default
								If oSeries.UniqueName.Equals("YTD") Then
									oSeries.Type = XFChart2SeriesType.Line
									oSeries.UseSecondaryAxis = False
									oSeries.Color = "#FF002343"
									oSeries.ShowMarkers = 1
									oSeries.LineThickness = 3
									oSeries.MarkerSize = 8
									oSeries.LineStyleType = XFChart2LineStyleType.Solid
									
								Else
									For Each point In oSeries.Points
										If point.Value > 0 Then
											point.Color = "#FF095A3C"
										Else 
											point.Color = "#FFC00000"
										End If
									Next
									oSeries.ModelType = XFChart2ModelType.Bar2DBorderlessSimple
									oSeries.BarWidth = 0.6
									
								End If
							Next
							Return oSeriesCollection.CreateDataSet(si)
							
							ElseIf (args.DataSetName.XFEqualsIgnoreCase("ExpensesVsBudgetTest")) Then							    
							    
								Dim oSeriesCollection As New XFSeriesCollection()
							
							    ' "0,1" => Toma la fila 0 y la fila 1
							    oSeriesCollection.FillUsingCubeViewDataAdapter(si,
							        False,
							        "LPW",
							        "da_Expenses_Vs_Budget_CV",
							        xfdataRowListType.AllRows,
							        "0,1",
							        XFChartCubeViewDataPointLegendType.Default,
							        False,
							        XFChart2SeriesType.PieAndDonut,
							        args.CustomSubstVars)
							
							    ' Recorremos cada serie
							    For Each oSeries As XFSeries In oSeriesCollection.Series
							        
							        ' Configuramos la serie como donut
							        oSeries.ModelType = XFChart2ModelType.Pie2DSimple
							        oSeries.Type = XFChart2SeriesType.PieAndDonut
							        oSeries.PieHoleRadiusPercent = 70
									oSeries.UniqueName =" "
							
							        ' Recorremos los "puntos" de esa serie; 
							        ' por lo general, si detecta 2 filas => 2 puntos
							        Dim pointIndex As Integer = 0
							        For Each point In oSeries.Points
							            If pointIndex = 0 Then
							                point.Color = primaryColor  ' la 1ª fila (row 0)
							            ElseIf pointIndex = 1 Then
							                point.Color = "White"    ' la 2ª fila (row 1)
							            Else
							                point.Color = "Grey"
							            End If
							            pointIndex += 1
							        Next
							
							    Next
							
							    Return oSeriesCollection.CreateDataSet(si)
											
								ElseIf (args.DataSetName.XFEqualsIgnoreCase("IncomeVsBudgetTest")) Then							    
							    
								Dim oSeriesCollection As New XFSeriesCollection()
							
							    ' "0,1" => Toma la fila 0 y la fila 1
							    oSeriesCollection.FillUsingCubeViewDataAdapter(si,
							        False,
							        "LPW",
							        "da_Income_Vs_Budget_CV",
							        xfdataRowListType.AllRows,
							        "0,1",
							        XFChartCubeViewDataPointLegendType.Default,
							        False,
							        XFChart2SeriesType.PieAndDonut,
							        args.CustomSubstVars)
							
							    ' Recorremos cada serie
							    For Each oSeries As XFSeries In oSeriesCollection.Series
							        
							        ' Configuramos la serie como donut
							        oSeries.ModelType = XFChart2ModelType.Pie2DSimple
							        oSeries.Type = XFChart2SeriesType.PieAndDonut
							        oSeries.PieHoleRadiusPercent = 70
									
							
							        ' Recorremos los "puntos" de esa serie; 
							        ' por lo general, si detecta 2 filas => 2 puntos
							        Dim pointIndex As Integer = 0
							        For Each point In oSeries.Points
							            If pointIndex = 0 Then
							                point.Color = primaryColor  ' la 1ª fila (row 0)
							            ElseIf pointIndex = 1 Then
							                point.Color = "White"    ' la 2ª fila (row 1)
							            Else
							                point.Color = "Grey"
							            End If
							            pointIndex += 1
							        Next
							
							    Next
							
							    Return oSeriesCollection.CreateDataSet(si)



							
							Else If (args.DataSetName.XFEqualsIgnoreCase("ExpensesVsBudget")) Then
							Dim oSeriesCollection As New XFSeriesCollection()
														
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "LPW", "da_Expenses_Vs_Budget_CV",
							xfdataRowListType.AllRows,"0,1", XFChartCubeViewDataPointLegendType.Default, False,
							XFChart2seriesType.PieAndDonut , args.CustomSubstVars)
							
							For Each oSeries As XFSeries In oSeriesCollection.Series
								' Formato default
								If oSeries.UniqueName.Equals("Expenses") Then
									oSeries.ModelType = XFChart2ModelType.Pie2DFlat
									oSeries.Type = XFChart2SeriesType.PieAndDonut
									oSeries.Color = primaryColor
									oSeries.ShowMarkers = 1
									oSeries.LineThickness = 3
									oSeries.MarkerSize = 8
									oSeries.LineStyleType = XFChart2LineStyleType.Solid
									oSeries.PieHoleRadiusPercent = 55
									

										
								Else
									For Each point In oSeries.Points
										If point.Value > 0 Then
											point.Color = "Grey"
										Else 
											point.Color = "Grey"
										End If
									Next
									
								End If
							Next
							
							Return oSeriesCollection.CreateDataSet(si)							
																				
						End If
						#End Region
						
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Get Overdues Matured Value"
		
		Private Function GetOverduesMaturedValue(ByVal si As SessionInfo, ByVal povEntity As String, ByVal povScenario As String, ByVal povView As String, ByVal year As Integer, ByVal month As Integer) As Decimal
		    Try
		        Dim scenarioToUse As String
		        If year = 2024 AndAlso month >= 1 AndAlso month <= 6 Then
		            scenarioToUse = "Historical"
		        Else
		            scenarioToUse = povScenario
		        End If
		
		        'Dim memberScript As String = $"E#{povEntity}:P#:C#Local:S#{scenarioToUse}:T#{year}M{month.ToString().PadLeft(2, "0"c)}:V#{povView}:A#1541000C:F#D51:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
		         Dim memberScript As String = $"E#{povEntity}:P#:C#Local:S#{scenarioToUse}:T#{year}M{month.ToString().PadLeft(2, "0"c)}:V#{povView}:A#02AC03:F#D51:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
		        
		        Dim dataCell As DataCell = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "Horse", memberScript).DataCellEx.DataCell
		        
		        ' --- LOG MEJORADO: Ahora registra el cruce y el valor encontrado (o la ausencia de valor) ---
		        If dataCell IsNot Nothing Then
		            Dim cellValue As Decimal = Convert.ToDecimal(dataCell.CellAmount)
		            BRApi.ErrorLog.LogMessage(si, $"DEBUG: Value [{cellValue}] found for intersection: {memberScript}")
		            Return cellValue
		        Else
		            BRApi.ErrorLog.LogMessage(si, $"DEBUG: No data found for intersection: {memberScript}")
		            Return 0
		        End If
		        
		    Catch ex As Exception
		        BRApi.ErrorLog.LogMessage(si, $"Error in GetOverduesMaturedValue for {year}M{month}: {ex.Message}")
		        Return 0
		    End Try
		End Function
		
		#End Region
		
		#Region "Get Overdues Total Maturity Value"

		Private Function GetOverduesTotalMaturityValue(ByVal si As SessionInfo, ByVal povEntity As String, ByVal povScenario As String, ByVal povView As String, ByVal year As Integer, ByVal month As Integer) As Decimal
		    Try
		        ' Construir el member script para obtener el valor de Total Maturity (F#[Total Maturity])
		        'Dim memberScript As String = $"E#{povEntity}:P#:C#Local:S#{povScenario}:T#{year}M{month.ToString().PadLeft(2, "0"c)}:V#{povView}:A#1541000C:F#[Total Maturity]:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
		         Dim memberScript As String = $"E#{povEntity}:P#:C#Local:S#{povScenario}:T#{year}M{month.ToString().PadLeft(2, "0"c)}:V#{povView}:A#02AC03:F#[Total Maturity]:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
		        ' Obtener el valor usando GetDataCellUsingMemberScript
		        Dim dataCell As DataCell = BRApi.Finance.Data.GetDataCellUsingMemberScript(si, "Horse", memberScript).DataCellEx.DataCell
		        
		        ' Retornar el valor (convertir a decimal)
		        If dataCell IsNot Nothing Then
		            Return Convert.ToDecimal(dataCell.CellAmount)
		        End If
		        
		        Return 0
		        
		    Catch ex As Exception
		        BRApi.ErrorLog.LogMessage(si, $"Error in GetOverduesTotalMaturityValue for {year}M{month}: {ex.Message}")
		        Return 0
		    End Try
		End Function
		
		#End Region
		
		
		
		#Region "Create Series"
		
			Private Function CreateSeries(ByVal si As SessionInfo, _
			                              name As String, _
			                              type As XFChart2SeriesType, _ 
			                              color As String, _
			                              seriesData As DataTable, _
			                              valueField As String, _
			                              argumentField As String, _
			                              Optional useSecondaryAxis As Boolean = False, _
			                              Optional showMarkers As Boolean = False, _
			                              Optional markerSize As Integer = 0, _
			                              Optional lineThickness As Integer = 0, _
			                              Optional modelType As XFChart2ModelType = Nothing, _
			                              Optional barWidth As Double = 0, _
			                              Optional parameterField As String = "NoIndicado", _
			                              Optional parameterType As String = "NoIndicado", _
			                              Optional useCustomPointColors As Boolean = False) As XFSeries
			    Try
			        Dim series As New XFSeries()
			        series.UniqueName = name
			        series.Type = type
			        series.Color = color
			        series.UseSecondaryAxis = useSecondaryAxis
			
			        If type = XFChart2SeriesType.Line Then
			            series.SeriesAnimationType = XFChart2SeriesAnimationType.Line2DUnwrapHorizontally
			            If showMarkers Then
			                series.ShowMarkers = 1
			                series.MarkerSize = markerSize
			            End If
			            If lineThickness > 0 Then
			                series.LineThickness = lineThickness
			            End If
			        ElseIf type = XFChart2SeriesType.BarSideBySide Then
			            series.ModelType = modelType
			            series.BarWidth = barWidth
			        End If
			
			        For Each row As DataRow In seriesData.Rows
			            Dim point As New XFSeriesPoint()
			            point.Argument = row(argumentField)
			
			            ' --- CAMBIO FINAL: Usar Double.NaN para valores nulos ---
			            If Not IsDBNull(row(valueField)) Then
			                ' Si hay valor, lo asignamos.
			                point.Value = Convert.ToDouble(row(valueField))
			            Else
			                ' Si el valor es nulo, asignamos "Not a Number".
			                ' El gráfico interpretará esto como un punto vacío.
			                point.Value = Double.NaN
			            End If
			            
			            series.AddPoint(point)
			        Next
			
			        Return series
			    Catch ex As Exception
			        ' La lógica para los parámetros se ha omitido en esta corrección final para simplificar,
			        ' ya que no se estaba usando en las llamadas a la función.
			        Throw New ApplicationException("Error creating the series: " & ex.Message)
			    End Try
			End Function

		
		#End Region
		
	End Class
End Namespace