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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class xfbr_main_service
		Implements IWsasXFBRStringV800
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions

        Public Function GetXFBRString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
			ByVal args As DashboardStringFunctionArgs) As String Implements IWsasXFBRStringV800.GetXFBRString
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
                    If args.FunctionName.XFEqualsIgnoreCase("GetEntityMemberFromCompanyId") Then
                        Dim result As String = HelperFunctionsBR.GetBaseEntity(si, args.NameValuePairs("p_company"))
                        ' Añadir "(including Entities)" si la entidad es "Horse Group"
                        If result.XFEqualsIgnoreCase("Horse Group") Then
                            result = result & " (including Intercompanies)"
                        End If
                        Return result
                    Else If args.FunctionName.XFEqualsIgnoreCase("GetParentMemberFromCompanyId") Then
                        Dim result As String = HelperFunctionsBR.GetParentEntity(si, args.NameValuePairs("p_company"))
                        ' Añadir "(including Entities)" si la entidad es "Horse Group"
                        If result.XFEqualsIgnoreCase("Horse Group") Then
                            result = result & " (including Entities)"
                        End If
                        Return result
					Else If args.FunctionName.XFEqualsIgnoreCase("GetMemberDescription") Then
				    ' Declarar variables FUERA del Try para que estén disponibles en el Catch
				    Dim dimName As String = String.Empty
				    Dim memberName As String = String.Empty
				    
				    Try
				        ' Obtener parámetros
				        dimName = args.NameValuePairs("p_dimName")
				        memberName = args.NameValuePairs("p_memberName")
				        
				        ' Validaciones básicas
				        If String.IsNullOrEmpty(dimName) OrElse String.IsNullOrEmpty(memberName) Then
				            Return memberName
				        End If
				        
				        ' Verificar si el memberName contiene "Error"
				        If memberName.Contains("Error") Then
				            Return String.Empty
				        End If
				        
				        ' Determinar el DimType basado en el nombre de la dimensión
				        Dim dimTypeId As Integer = -1
				        Select Case dimName.ToLower()
				            Case "entity"
				                dimTypeId = DimType.Entity.Id
				            Case "account"
				                dimTypeId = DimType.Account.Id
				            Case "scenario"
				                dimTypeId = DimType.Scenario.Id
				            Case "time"
				                dimTypeId = DimType.Time.Id
				            Case "view"
				                dimTypeId = DimType.View.Id
				            Case "consolidation"
				                dimTypeId = DimType.Consolidation.Id
				            Case "flow"
				                dimTypeId = DimType.Flow.Id
				            Case "origin"
				                dimTypeId = DimType.Origin.Id
				            Case "ic"
				                dimTypeId = DimType.IC.Id
				            Case "ud1"
				                dimTypeId = DimType.UD1.Id
				            Case "ud2"
				                dimTypeId = DimType.UD2.Id
				            Case "ud3"
				                dimTypeId = DimType.UD3.Id
				            Case "ud4"
				                dimTypeId = DimType.UD4.Id
				            Case "ud5"
				                dimTypeId = DimType.UD5.Id
				            Case "ud6"
				                dimTypeId = DimType.UD6.Id
				            Case "ud7"
				                dimTypeId = DimType.UD7.Id
				            Case "ud8"
				                dimTypeId = DimType.UD8.Id
				            Case Else
				                Return memberName ' Dimensión no reconocida, devolver nombre original
				        End Select
				        
				        ' Obtener el miembro
				        Dim member As Member = BRApi.Finance.Members.GetMember(si, dimTypeId, memberName)
				        
				        Dim result As String = String.Empty
				        If member IsNot Nothing AndAlso Not String.IsNullOrEmpty(member.Description) Then
				            result = member.Description
				        Else
				            result = memberName ' Devolver nombre original si no hay descripción
				        End If
				        
				        ' Añadir "(including Intercompanies)" si es una entidad y el resultado es "Horse Group"
				        If dimName.ToLower() = "entity" AndAlso result.XFEqualsIgnoreCase("Horse Group") Then
				            result = result & " (including Intercompanies)"
				        End If
				        
				        Return result
				        
				    Catch ex As Exception
				        ' Ahora memberName está disponible aquí porque fue declarado fuera del Try
				        Return If(String.IsNullOrEmpty(memberName), "Error", memberName)
				    End Try
					Else If args.FunctionName.XFEqualsIgnoreCase("GetDynamicUD3") Then
						Return Me.GetDynamicUD3(si, globals, workspace, args)
                	End If
	            End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		Public Function GetDynamicUD3(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, ByVal args As DashboardStringFunctionArgs) As String
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