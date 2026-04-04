Imports System
Imports System.Collections.Generic
Imports OneStream.Shared.Common
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DataAdapterGraphs.GraphsSharedHelpers

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardDataSet.DataAdapterGraphs
	''' <summary>
	''' Orchestrator for graph generation - delegates to specialized graph modules
	''' This is the main entry point called by OneStream data adapters
	''' </summary>
	Public Class MainClass
		
		' Graph module instances
		Private ReadOnly cashDebtGraphs As New CashDebtGraphs()
		Private ReadOnly cashFlowGraphs As New CashFlowGraphs()
		
		''' <summary>
		''' Main entry point for the data adapter
		''' Routes requests to the appropriate graph module
		''' </summary>
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardDataSetArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardDataSetFunctionType.GetDataSetNames
						' Return all available dataset names
						Dim names As New List(Of String)()
						
						' CashDebt graphs
						names.Add("CashBalanceGraph")
						names.Add("CashBalanceGraphByWeekNumber")
						names.Add("RFCLines")
						names.Add("CashDebtCurrentYear")
						names.Add("CashDebtBalance")
						names.Add("WeeklyCashPosition")
						names.Add("WeeklyDebtPosition")
						
						' CashFlow graphs
						names.Add("CashFlowProjection")
						names.Add("CashFlowProjectionCurrentYear")
						names.Add("CashFlow4WeekProjection")
						names.Add("CashFlow4WeekProjection_Test")
						names.Add("FreeOperatingCashflowMonthly")
						
						Return names

					Case Is = DashboardDataSetFunctionType.GetDataSet
						' Route to appropriate graph module based on dataset name
						Return Me.RouteToGraphModule(si, args)
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		''' <summary>
		''' Routes the graph request to the appropriate module
		''' </summary>
		Private Function RouteToGraphModule(ByVal si As SessionInfo, ByVal args As DashboardDataSetArgs) As Object
			Try
				' CashDebt Position Graphs
				If args.DataSetName.XFEqualsIgnoreCase("CashBalanceGraph") Then
					Return cashDebtGraphs.GetCashBalanceGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CashBalanceGraphByWeekNumber") Then
					Return cashDebtGraphs.GetCashBalanceGraphByWeekNumber(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("RFCLines") Then
					Return cashDebtGraphs.GetRFCLinesGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CashDebtCurrentYear") Then
					Return cashDebtGraphs.GetCashDebtCurrentYearGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CashDebtBalance") Then
					Return cashDebtGraphs.GetCashDebtBalanceGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("WeeklyCashPosition") Then
					Return cashDebtGraphs.GetWeeklyCashPositionGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("WeeklyDebtPosition") Then
					Return cashDebtGraphs.GetWeeklyDebtPositionGraph(si, args)
				
				' CashFlow Forecasting Graphs
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CashFlowProjection") Then
					Return cashFlowGraphs.GetCashFlowProjectionGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CashFlowProjectionCurrentYear") Then
					Return cashFlowGraphs.GetCashFlowProjectionCurrentYearGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CashFlow4WeekProjection") Then
					Return cashFlowGraphs.GetCashFlow4WeekProjectionGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("CashFlow4WeekProjection_Test") Then
					Return cashFlowGraphs.GetCashFlow4WeekProjectionTestGraph(si, args)
					
				ElseIf args.DataSetName.XFEqualsIgnoreCase("FreeOperatingCashflowMonthly") Then
					Return cashFlowGraphs.GetFreeOperatingCashflowMonthlyGraph(si, args)
					
				Else
					Return Nothing
				End If
				
			Catch ex As Exception
				Throw
			End Try
		End Function
		
	End Class
End Namespace
