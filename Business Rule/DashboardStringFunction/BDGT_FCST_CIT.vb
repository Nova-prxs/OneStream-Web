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

Namespace OneStream.BusinessRule.DashboardStringFunction.BDGT_FCST_CIT
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
				'initialize VariableRepository
				'Dim rep As New OneStream.BusinessRule.Finance.VariableRepository.MainClass				
					
				'XFBR(Automations_XFBR_String, Automations_Scenario, A=[|WFScenario|])

				If args.FunctionName.XFEqualsIgnoreCase("CORP TAX INSTALLMENTS") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"											
					
					' Obtener el ID y valor del parámetro
					Dim entity As String = args.NameValuePairs("A")
					Dim sYear As String = args.NameValuePairs("Time")
					Dim sName As String = "EBT Without Warranty Cost"
					
					
					Dim result As String = String.Empty
					
					If entity = "R0585" Then
						result = "A#CIT:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:V#Periodic:O#BeforeAdj:I#Top:U5#None:U6#None:U7#None:U8#None, A#541000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top:I#Top:U5#None:U6#None:U7#None:U8#None, A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None, A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None"
					Else If entity = "R1300" Or entity = "R1301"
						result = "A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#None,A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#None,GetDataCell(A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#None+A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#None):Name(""" & sName & """), A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None, A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None" 
					Else If entity = "R0592"
						result = "A#CIT:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:V#Periodic:O#BeforeAdj:I#Top:U5#None:U6#None:U7#None:U8#None, A#4111:C#Local:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U8#PY:V#YTD:O#Top:I#Top,A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None, A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None"
					End If
					
		            Return result
					
				Else If args.FunctionName.XFEqualsIgnoreCase("CORP TAX SETTLEMENT") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"
					
					' Obtener el ID y valor del parámetro
					Dim entity As String = args.NameValuePairs("A")
					Dim sYear As String = args.NameValuePairs("Time")
					Dim sName As String = "EBT Without Warranty Cost"

						

					Dim result As String = String.Empty
					
					If entity = "R0585" Or entity = "R0592" Then
						result = "A#1561040:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top,A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#YTD:O#Import:I#None,A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#YTD:O#Import:I#None"
					Else If entity = "R1300" Or entity = "R1301"
						result =  "A#CIT:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:V#Periodic:O#BeforeAdj:I#Top:U5#None:U6#None:U7#None:U8#None:T#"& sYear &"M7,A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#PY,A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#PY,GetDataCell(A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#PY+A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:U5#None:U6#None:U7#None:U8#PY):Name(""" & sName & """),A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None,A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None"
					End If
					Return result
					
'''						For Each sMes As String In New List(Of String) From {"1", "2", "3","4","5","6","7","8","9","10","11","12"}
'''							'result =  "E#"& entity &":S#RF09:A#Corp_Tax:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:V#Periodic:O#BeforeAdj:I#Top:T#"& (sYear -1) &", A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:T#"& (sYear -1) &"M" &(sMes) & ":U5#None:U6#None:U7#None:U8#None,A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None,A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None"
'''							 result &= "E#" & entity & ":S#RF09:A#Corp_Tax:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:V#Periodic:O#BeforeAdj:I#Top:T#" & (sYear - 1) & ", " & _
'''                      			"A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#YTD:O#Top:I#Top:T#" & (sYear - 1) & "M" & sMes & ":U5#None:U6#None:U7#None:U8#None," & _
'''                      			"A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None," & _
'''                      			"A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None"
								
'''							BRApi.ErrorLog.LogMessage(si, result)	
						
'''						Next


					
				Else If args.FunctionName.XFEqualsIgnoreCase("VALIDATION CORP TAX INSTALLMENTS") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"
						
					' Obtener el ID y valor del parámetro
					Dim entity As String = args.NameValuePairs("A")
					Dim sYear As String = args.NameValuePairs("Time")
					Dim sName As String = "Validation"
					
					Dim result As String = String.Empty
					
					If entity = "R0585" Or entity = "R0592" Then
							result= "A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None + 
							A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None"
					Else If entity = "R1300" Or entity = "R1301"
							result= "A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None -
							A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:V#Periodic:O#Import:I#None:U5#None:U6#None:U7#None:U8#None"

					End If

					Return $"GetDataCell("& result &"):Name(""" & sName & """)"
					
				Else If args.FunctionName.XFEqualsIgnoreCase("VALIDATION CORP TAX SETTLEMENTS") Then
					If args.NameValuePairs.Count < 1 Then Return "Error"
					' Obtener el ID y valor del parámetro
					Dim entity As String = args.NameValuePairs("A")
					Dim sYear As String = args.NameValuePairs("Time")
					
					Dim sName As String = "Validation"
					
					Dim result As String = String.Empty
					
					If entity = "R0585" Or entity = "R0592" Then
						result = "A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#YTD:O#Import:I#None -
						A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#YTD:O#Import:I#None"
					Else If entity = "R1300" Or entity = "R1301"
						result =  "A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None -
						A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:V#Periodic:O#Import:I#None"
					End If
					
					Return $"GetDataCell("& result &"):Name(""" & sName & """)"
				
				'XFBR(Automations_XFBR_String, Automations_Scenario_WCAP, A=[|WFScenario|])
				
'				Else If args.FunctionName.XFEqualsIgnoreCase("Automations_Scenario_WCAP") Then
'					If args.NameValuePairs.Count < 1 Then Return "Error"											
					
'					' Obtener el ID y valor del parámetro
'					Dim scenario As String = args.NameValuePairs("A")
'					Dim result As String = String.Empty
					
'					If (Left(scenario,1)) = "B" Then
						
'						result = "BDG_WCAP"
					
'					Else If (Left(scenario,1)) = "R" Then
						
'						result = "FCST_WCAP"

'					End If
					
'		            Return result
			
''--------------------------------------------------------------------------------------------------------------------------------------					
					
'				'XFBR(Automations_XFBR_String, Automations_Scenario_Template, A=[|WFScenario|])
				

'				Else If args.FunctionName.XFEqualsIgnoreCase("Automations_Scenario_Template") Then
'					If args.NameValuePairs.Count < 1 Then Return "Error"											
					
'					' Obtener el ID y valor del parámetro
'					Dim scenario As String = args.NameValuePairs("A")
'					Dim result As String = String.Empty
					
'					If (Left(scenario,1)) = "B" Then
						
'						result = "Template_PC_Budget.xlsx"
					
'					Else If (Left(scenario,1)) = "R" Then
						
'						result = "Template_PC_Forecast.xlsx"

'					End If
					
'		            Return result		
					
					
					
				End If
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace