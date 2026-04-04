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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.LPW_TimeHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				brapi.ErrorLog.LogMessage(si , "1")
				If args.FunctionName.XFEqualsIgnoreCase("GetDynamicUD3") Then
					Return Me.GetDynamicUD3(si, globals, api, args)
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetDynamicUD3(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				' Obtener los parámetros de tiempo
				Dim lpwParamPovTime As String = args.NameValuePairs.XFGetValue("LPW_param_POV_Time", String.Empty)
				Dim lpwParamPovTimeRef As String = args.NameValuePairs.XFGetValue("LPW_param_POV_Time_Ref", String.Empty)
				
				' Variable para almacenar el resultado
				Dim ud3Member As String = "None"
				
				' Log para debugging
				BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Time: " & lpwParamPovTime & ", TimeRef: " & lpwParamPovTimeRef)
				
				' Validar que los parámetros no estén vacíos
				If String.IsNullOrEmpty(lpwParamPovTime) OrElse String.IsNullOrEmpty(lpwParamPovTimeRef) Then
					BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Parámetros vacíos, devolviendo None")
					ud3Member = "None"
				Else
					' Obtener los miembros de tiempo
					Dim currentTimeMember As Member = BRApi.Finance.Members.GetMember(si, DimType.Time.Id, lpwParamPovTime)
					Dim refTimeMember As Member = BRApi.Finance.Members.GetMember(si, DimType.Time.Id, lpwParamPovTimeRef)
					
					' Validar que los miembros sean válidos
					If currentTimeMember Is Nothing OrElse refTimeMember Is Nothing Then
						BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Miembros no encontrados, devolviendo None")
						ud3Member = "None"
					Else
						' Obtener los nombres de los períodos (formato: YYYYMX, ej: 2025M2, 2024M2)
						Dim currentTimeName As String = currentTimeMember.Name
						Dim refTimeName As String = refTimeMember.Name
						
						BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Current Time: " & currentTimeName & ", Ref Time: " & refTimeName)
						
						' Lógica de comparación
						If currentTimeName.Equals(refTimeName) Then
							' Si son iguales, devolver None
							BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Son iguales, devolviendo None")
							ud3Member = "None"
						Else
							' Extraer año y período de ambos miembros
							' Formato esperado: YYYYMX (ej: 2025M2)
							Dim posM As Integer = currentTimeName.IndexOf("M")
							Dim posRefM As Integer = refTimeName.IndexOf("M")
							
							If posM > 0 AndAlso posRefM > 0 Then
								' Extraer año y mes de Time
								Dim currentYear As Integer = Integer.Parse(currentTimeName.Substring(0, posM))
								Dim currentPeriod As String = currentTimeName.Substring(posM)
								
								' Extraer año y mes de TimeRef
								Dim refYear As Integer = Integer.Parse(refTimeName.Substring(0, posRefM))
								Dim refPeriod As String = refTimeName.Substring(posRefM)
								
								BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Current: Year=" & currentYear.ToString() & ", Period=" & currentPeriod & " | Ref: Year=" & refYear.ToString() & ", Period=" & refPeriod)
								
								' Verificar si TimeRef está exactamente 1 año antes y el período (MX) es el mismo
								If (currentYear - 1 = refYear) AndAlso (currentPeriod.Equals(refPeriod)) Then
									' Si TimeRef está exactamente 12 meses antes que Time, devolver PY
									BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - TimeRef está 12 meses antes (1 año atrás, mismo período), devolviendo PY")
									ud3Member = "PY"
								Else
									' En cualquier otro caso, devolver NA
									BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Otros casos, devolviendo NA")
									ud3Member = "NA"
								End If
							Else
								' Si no se puede parsear el formato, devolver NA
								BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - No se pudo parsear el formato de tiempo, devolviendo NA")
								ud3Member = "NA"
							End If
						End If
					End If
				End If
				
				' Construir la expresión completa de miembro para U3
				Dim result As String = "U3#" & ud3Member
				BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Resultado final: " & result)
				
				Return result
				
			Catch ex As Exception
				' Log del error
				BRApi.ErrorLog.LogMessage(si, "GetDynamicUD3 - Error: " & ex.Message)
				' En caso de error, devolver NA como valor por defecto
				Return "U3#NA"
			End Try
		End Function
		
	End Class
End Namespace
