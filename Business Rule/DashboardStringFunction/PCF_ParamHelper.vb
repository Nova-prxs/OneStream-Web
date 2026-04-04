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

Namespace OneStream.BusinessRule.DashboardStringFunction.PCF_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("ReferenceScenario") Then
							
					' Declare valid scenarios
					Dim validScenarios As New List(Of String) From {"forecast", "budget"}
					
					' Get parameters
					Dim sScenario As String = args.NameValuePairs("p_scenario")
					
					' Validate Input and get reference scenario
					If Not validScenarios.Contains(sScenario.ToLower) Then Throw New Exception($"error getting reference scenario: {sScenario} is not a valid scenario.")
					Dim refScenarioName As String = If(sScenario.ToLower = "forecast", "Actual", "Forecast")
					
					Return refScenarioName
					
				Else If args.FunctionName.XFEqualsIgnoreCase("ReferencePeriodList") Then
					
					' Declare valid types
					Dim validTypes As New List(Of String) From {"forecast", "actual"}
					
					' Get parameters
					Dim sYear As Integer = CInt(args.NameValuePairs.XFGetValue("p_year")) - 1
					Dim sType As String = args.NameValuePairs.XFGetValue("p_type")
					Dim forecastMonth As Integer = CInt(BRApi.Dashboards.Parameters.GetLiteralParameterValue(
						si,
						False,
						BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "Default"),
						"prm_Forecast_Month"
					))
					
					' Validate input
					If Not validTypes.Contains(sType) Then _
						Throw New Exception($"error getting period list: {sType} is not a valid type.")
						
					Dim startMonth As Integer = If(sType = "actual", 1, forecastMonth)
					Dim endMonth As Integer = If(sType = "actual", forecastMonth - 1, 12)
					Dim periodListString As String = String.Empty
					For month As Integer = startMonth To endMonth
						periodListString &= If(String.IsNullOrEmpty(periodListString), $"{sYear}M{month}", $",{sYear}M{month}")
					Next
					
					Return periodListString
					
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace