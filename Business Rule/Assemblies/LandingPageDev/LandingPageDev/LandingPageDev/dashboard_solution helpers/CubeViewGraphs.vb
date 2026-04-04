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
						    Dim scenarioComparison As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "LandingPageDev.prm_LandingPageDev_Cash_Scenario_Graph")
						
						If (args.DataSetName.XFEqualsIgnoreCase("CNS_SUM_Dashboard_PL_HorseGroup")) Then
							Dim oSeriesCollection As New XFSeriesCollection()
							
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "LandingPageDev", "da_Revenues_EBIT_Monthly_Graph_CV",
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
							
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "LandingPageDev", "da_OperationalFCF_Graph_CV",
							xfdataRowListType.AllRows, "0,1", XFChartCubeViewDataPointLegendType.Default, False,
							XFChart2seriesType.BarSideBySide, args.CustomSubstVars)
							
							For Each oSeries As XFSeries In oSeriesCollection.Series
								
								' Formato default
								If oSeries.UniqueName.Equals("YTD") Then
									oSeries.Type = XFChart2SeriesType.Line
									oSeries.UseSecondaryAxis = False
									oSeries.Color = secondaryColor
									oSeries.ShowMarkers = 1
									oSeries.LineThickness = 3
									oSeries.MarkerSize = 8
									oSeries.LineStyleType = XFChart2LineStyleType.Solid
									
								Else
									For Each point In oSeries.Points
										If point.Value > 0 Then
											point.Color = green
										Else 
											point.Color = red
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
							        "LandingPageDev",
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
							        "LandingPageDev",
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
														
							oSeriesCollection.FillUsingCubeViewDataAdapter(si, False, "LandingPageDev", "da_Expenses_Vs_Budget_CV",
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
						
						
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
