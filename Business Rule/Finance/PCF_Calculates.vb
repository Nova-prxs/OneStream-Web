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

Namespace OneStream.BusinessRule.Finance.PCF_Calculates
	Public Class MainClass
		
		'Reference business rule to get functions
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate
						
						#Region "Calculate PCF"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CalculatePCF") Then
							' Get parent entity
							Dim parentEntity = args.CustomCalculateArgs.NameValuePairs("p_parent_entity")
							
							' Set variables
							Dim year = CInt(Left(api.Pov.Time.Name, 4))
							Dim month = CInt(Right(api.Pov.Time.Name, 2).Replace("M", ""))
							Dim parentEntityInput = parentEntity & "_Input"
							api.Data.FormulaVariables.SetDecimalVariable("daysInMonth", DateTime.DaysInMonth(year, month))
							Dim forecastMonth = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim scenario = api.Pov.Scenario.Name
							If scenario = "Budget" Then forecastMonth = 1
							Dim actualAccountsToClear = If(month >= forecastMonth, "A#FR_SAL, A#FR_VAC, A#FR_OPR, A#FR_VAR, A#FR_SS,", "")
							
							' Clear and calculate costs and productive hours if entity is open
							Dim dataBufferToClear = api.Data.GetDataBufferUsingFormula(
								$"FilterMembers(F#None,
								A#fte_number, A#productive_hs_total, A#PCF_productivity, A#Productivity, {actualAccountsToClear}
								U2#France.Base, U2#None)",,False
							)
							SharedFunctionsBR.ClearDataBuffer(si, api, dataBufferToClear)
							
							'Get if it's closed
							Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, year, month, api.Pov.Entity.Name)
							If isClosed Then Return Nothing
							
							' Calculate general costs
							If month >= forecastMonth Then api.Data.Calculate(
								$"V#Periodic:O#All:A#All:F#None = RemoveZeros(
										V#Periodic:O#All:A#total_hs:F#None * (E#{parentEntityInput}:T#PovYearM12:V#YTD:O#Top:A#All:F#Perc_PCF_Input / 100)
										* E#{parentEntityInput}:T#PovYearM12:V#YTD:O#Top:A#HourCosts:F#None * -1
									)",
								"A#Root.List(FR_SAL,FR_OPR,FR_VAR,FR_SS)",, "O#Root.List(Import, Forms)", "I#None",
								"U1#None", "U2#France.Base", "U3#None", "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							' Calculate vacation costs
							If month >= forecastMonth Then api.Data.Calculate(
								$"A#FR_VAC:V#Periodic:O#All:F#None:U2#All = RemoveZeros(
										A#FR_SAL:V#Periodic:O#All:F#None:U2#All * (E#{parentEntityInput}:A#FR_VAC:V#Periodic:O#Top:F#Perc_PCF_Input:U2#None / 100)
									)",
								"A#Root.List(FR_SAL,FR_VAC)",, "O#Root.List(Import, Forms)", "I#None",
								"U1#None", "U2#France.Base", "U3#None", "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							' Calculate productive hs
							api.Data.Calculate(
								$"V#Periodic:O#Import:A#productive_hs_total = RemoveZeros(
										V#Periodic:O#Top:A#total_hs * (E#{parentEntityInput}:T#PovYearM12:V#YTD:O#Top:A#productive_hs_perc / 100)
									)",
								, "F#None", , "I#None",
								"U1#None", "U2#France.Base", "U3#None", "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							
							' Calculate FTE
							api.Data.Calculate(
								$"A#fte_number:V#Periodic:O#Import = RemoveZeros(
										A#total_hs:V#Periodic:O#Top / (E#{parentEntityInput}:T#PovYearM12:O#Top:A#weekly_hs:V#YTD / 7 * $daysInMonth)
									)",
								, "F#None",, "I#None",
								"U1#None", "U2#France.Base", "U3#None", "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							
							' Calculate productivity
							api.Data.Calculate(
								$"A#Productivity:V#Periodic:O#Import:I#None:F#None:U1#None:U2#None = RemoveZeros(
										A#Customers:V#Periodic:O#Top:I#None:F#Top:U1#Top:U2#None
										/ A#productive_hs_total:V#Periodic:O#Top:I#None:F#None:U1#None:U2#France
									)",
								,,, "I#None",
								,, "U3#None", "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							
						#End Region
						
						#Region "Apply Adjustments"
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ApplyAdjustments") Then
							
							' Clear and apply adjustment to imported total hs
							Dim dataBufferToClear = api.Data.GetDataBufferUsingFormula(
								"FilterMembers(F#None:A#total_hs, U2#France.Base)",,False
							)
							SharedFunctionsBR.ClearDataBuffer(si, api, dataBufferToClear)
							
							'Get if it's closed
							Dim year = CInt(Left(api.Pov.Time.Name, 4))
							Dim month = CInt(Right(api.Pov.Time.Name, 2).Replace("M", ""))
							Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, year, month, api.Pov.Entity.Name)
							If isClosed Then Return Nothing
							
							' Get forecast month to select a source scenario
							Dim forecastMonth As Integer = CInt(BRApi.Dashboards.Parameters.GetLiteralParameterValue(
								si,
								False,
								BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "Default"),
								"prm_Forecast_Month"
							))
							
							' If the calculated scenario is forecast and pov month is smaller than forecast month, just copy data
							If api.Pov.Scenario.Name = "Forecast" AndAlso month < forecastMonth Then
							api.Data.Calculate(
								$"V#Periodic:O#Import:F#None = RemoveZeros(
									S#Actual:V#Periodic:O#Top:F#None)
								)",
								"A#total_hs",,, "I#None",
								"U1#None", "U2#France.Base", "U3#None", "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
								Return Nothing
							End If
							' Get parent entity
							Dim parentEntity = args.CustomCalculateArgs.NameValuePairs("p_parent_entity")
							
							' Set variable
							Dim parentEntityInput = parentEntity & "_Input"
							Dim sourceScenario As String = If(
								month < forecastMonth,
								"Actual",
								If(api.Pov.Scenario.Name.ToLower = "forecast", "Actual", "Forecast")
							)
							api.Data.Calculate(
								$"V#Periodic:O#Import:F#None = RemoveZeros(
									S#{sourceScenario}:T#PovPrior12:V#Periodic:O#Top:F#None * (1 + (E#{parentEntityInput}:V#Periodic:O#BeforeAdj:F#Perc_Variation_Input / 100))
								)",
								"A#total_hs",,, "I#None",
								"U1#None", "U2#France.Base", "U3#None", "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							
							' Get data buffer and round hours
							Dim dataBufferToRound = api.Data.GetDataBufferUsingFormula("FilterMembers(A#total_hs, U2#France.Base)",, False)
							Dim destinationInfo = api.Data.GetExpressionDestinationInfo("")
							Dim dataBufferRounded As New DataBuffer()
							For Each DataBufferCell In dataBufferToRound.DataBufferCells.Values
								Dim newDataBufferCell As New DataBufferCell(DataBufferCell)
								newDataBufferCell.CellAmount = math.round(newDataBufferCell.CellAmount)
								dataBufferRounded.SetCell(si, newDataBufferCell)
							Next
							api.Data.SetDataBuffer(dataBufferRounded, destinationInfo,,,,,,,,,,,,,True)
							
						#End Region
						
						#Region "Calculate Vacation Percentage"
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CalculateVacationPercentage") Then
							' Get parent entity
							Dim parentEntity = args.CustomCalculateArgs.NameValuePairs("p_parent_entity")
							' Get source scenario based on calculated scenario
							Dim sourceScenario As String = If(api.Pov.Scenario.Name.ToLower.Contains("forecast"), "Actual", "Forecast")
							
							' Clear and calculate percentage
							Dim dataBufferToClear = api.Data.GetDataBufferUsingFormula(
								"FilterMembers(F#Perc_PCF_Input,
								A#FR_VAC,
								U2#None)",,False
							)
							SharedFunctionsBR.ClearDataBuffer(si, api, dataBufferToClear)
							api.Data.Calculate(
								$"A#FR_VAC:V#Periodic:O#Import:F#Perc_PCF_Input:U1#None:U2#None:U3#None = RemoveZeros(
										E#{parentEntity}:S#{sourceScenario}:T#POVPrior12:C#Aggregated:A#FR_VAC:V#Periodic:O#Top:F#Top:U1#Top:U2#Top:U3#Top
										/ E#{parentEntity}:S#{sourceScenario}:T#POVPrior12:C#Aggregated:A#FR_SAL:V#Periodic:O#Top:F#Top:U1#Top:U2#Top:U3#Top * 100
									)",
								"A#Root.List(FR_SAL,FR_VAC)",,, "I#None",
								, , , "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							
						#End Region
						
						#Region "Calculate Parent KPIs"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CalculateParentKPIs") Then
							' Get parent entity
							Dim parentEntity = args.CustomCalculateArgs.NameValuePairs("p_parent_entity")
							
							' Clear and calculate costs and productive hours
							Dim dataBufferToClear = api.Data.GetDataBufferUsingFormula(
								"FilterMembers(F#None, A#PCF_productivity, A#Productivity)",
								, False
							)
							SharedFunctionsBR.ClearDataBuffer(si, api, dataBufferToClear)
							
							' Calculate productivity
							api.Data.Calculate(
								$"A#Productivity:V#Periodic:O#Import:I#None:F#None:U1#None:U2#None:U3#None = RemoveZeros(
										E#{parentEntity}:C#Aggregated:A#Customers:V#Periodic:O#Top:I#None:F#Top:U1#Top:U2#None:U3#[Total Comparables and Closings]
										/ E#{parentEntity}:C#Aggregated:A#productive_hs_total:V#Periodic:O#BeforeAdj:I#None:F#None:U1#None:U2#France:U3#[Total Comparables and Closings]
									)",
								,,, "I#None",
								,,, "U4#None", "U5#None", "U6#None", "U7#None", "U8#None",,, True
							)
							
						#End Region
							
						End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

	End Class
End Namespace