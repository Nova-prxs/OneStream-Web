Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.DashboardStringFunction.Automations_XFBR_String
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
				'initialize VariableRepository
				'Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass				
					
				'XFBR(Automations_XFBR_String, Automations_Scenario, A=[|WFScenario|])

				If args.FunctionName.XFEqualsIgnoreCase("Automations_Scenario") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"											
					
					' Obtener el ID y valor del parámetro
					Dim scenario As String = args.NameValuePairs("A")
					Dim result As String = String.Empty
					
					If (Left(scenario,1)) = "B" Then
						
						result = "BDG_All"
					
					Else If (Left(scenario,1)) = "R" Then
						
						result = "FCST_ALL"

					End If
					
		            Return result
				
				'XFBR(Automations_XFBR_String, Automations_Scenario_WCAP, A=[|WFScenario|])
				
				Else If args.FunctionName.XFEqualsIgnoreCase("Automations_Scenario_WCAP") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"											
					
					' Obtener el ID y valor del parámetro
					Dim scenario As String = args.NameValuePairs("A")
					Dim result As String = String.Empty
					
					If (Left(scenario,1)) = "B" Then
						
						result = "BDG_WCAP"
					
					Else If (Left(scenario,1)) = "R" Then
						
						result = "FCST_WCAP"

					End If
					
		            Return result
			
'--------------------------------------------------------------------------------------------------------------------------------------					
					
				'XFBR(Automations_XFBR_String, Automations_Scenario_Template, A=[|WFScenario|])
				

				Else If args.FunctionName.XFEqualsIgnoreCase("Automations_Scenario_Template") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"											
					
					' Obtener el ID y valor del parámetro
					Dim scenario As String = args.NameValuePairs("A")
					Dim result As String = String.Empty
					
					If (Left(scenario,1)) = "B" Then
						
						result = "Template_PC_Budget.xlsx"
					
					Else If (Left(scenario,1)) = "R" Then
						
						result = "Template_PC_Forecast.xlsx"

					End If
					
		            Return result		
					
					
					
				End If
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace