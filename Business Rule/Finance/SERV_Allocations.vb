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

Namespace OneStream.BusinessRule.Finance.SERV_Allocations
    Public Class MainClass
        Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
            Try
                Select Case api.FunctionType
                    Case Is = FinanceFunctionType.CustomCalculate
                        If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ProcessAllocations") Then
                            ProcessAllocations(si, api, args)
                        ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Delete") Then
                            Delete(si, globals, api, args)
                        End If
                End Select

                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
        
        Public Sub ProcessAllocations(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)
            Try
                ' 1. Obtener Parámetro de Tiempo
                If args.CustomCalculateArgs Is Nothing Then
                    Throw New Exception("CustomCalculateArgs es Nothing")
                End If

                Dim paramAllocationsTime As String = String.Empty

                ' Intentar obtener p_time desde p_query_params primero
                If args.CustomCalculateArgs.NameValuePairs.ContainsKey("p_query_params") Then
                    Dim queryParams As String = args.CustomCalculateArgs.NameValuePairs("p_query_params")
                    Dim parameterDict As Dictionary(Of String, String) = Me.ParseQueryParams(si, queryParams)
                    
                    If parameterDict.ContainsKey("p_time") Then
                        paramAllocationsTime = parameterDict("p_time").Trim()
                    End If
                End If

                ' Si no está en p_query_params, intentar directo
                If String.IsNullOrEmpty(paramAllocationsTime) AndAlso args.CustomCalculateArgs.NameValuePairs.ContainsKey("p_time") Then
                    paramAllocationsTime = args.CustomCalculateArgs.NameValuePairs("p_time").Trim()
                End If
                
                ' Fallback: prm_allocations_time (legacy)
                If String.IsNullOrEmpty(paramAllocationsTime) AndAlso args.CustomCalculateArgs.NameValuePairs.ContainsKey("prm_allocations_time") Then
                    paramAllocationsTime = args.CustomCalculateArgs.NameValuePairs("prm_allocations_time").Trim()
                End If

                If String.IsNullOrEmpty(paramAllocationsTime) Then
                    BRApi.ErrorLog.LogMessage(si, "Error: El parámetro p_time es obligatorio. Formato: p_time=2025M1")
                    Return
                End If

                ' 2. Consultar la tabla XFC_RES_SERV_ALLOCATIONS
                Dim sql As String = $"SELECT * FROM XFC_RES_SERV_ALLOCATIONS 
									WHERE enabled = 1 
									AND time = '{paramAllocationsTime}' ORDER BY sequence"
                
                ' Corrección: Crear conexión explícita como en QRN_Test_Epigrafe_OPTIMIZED.vb
                Dim dt As DataTable = Nothing
                Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
                    dt = BRApi.Database.ExecuteSql(dbConn, sql, False)
                End Using
                
                If dt Is Nothing OrElse dt.Rows.Count = 0 Then
                    ' Lanzar excepción para detener el Data Management (icono Rojo) si no hay reglas
                    Throw New Exception($"No se encontraron asignaciones activas para el tiempo seleccionado ({paramAllocationsTime}). No es posible ejecutar el proceso.")
                End If

                ' Preparar DimPks usando api.Pov - ESTILO QRN_TEST
                Dim dimPkAccount As DimPk = api.Pov.AccountDim.DimPk
                Dim dimPkUD1 As DimPk = api.Pov.UD1Dim.DimPk
                Dim dimPkUD3 As DimPk = api.Pov.UD3Dim.DimPk

                ' 3. Iterar por cada regla (fila de la tabla)
                For Each dr As DataRow In dt.Rows
                    Try
                        ' --- LEER CONFIGURACIÓN DE LA FILA ---
                        Dim entitySource As String = dr("entity_source").ToString()
                        Dim entityTarget As String = dr("entity_target").ToString()
                        
                        Dim accountSourceParent As String = dr("account_source").ToString()
                        Dim costCenterSourceParent As String = dr("cost_center_source").ToString()
                        
                        Dim technologyDriverParent As String = dr("technology_driver").ToString()
                        Dim accountDriver As String = dr("account_driver").ToString()
                        
                        ' --- EXPANDIR MIEMBROS A LISTAS (GetBaseMembers) ---
                        ' a) Accounts (Source)
                        ' USAR dimTypeId.Account (Enum) y List(Of Member)
                        Dim accountSourceId As Integer = api.Members.GetMemberId(dimTypeId.Account, accountSourceParent)
                        Dim accountSourceBaseMembers As List(Of Member) = api.Members.GetBaseMembers(dimPkAccount, accountSourceId, Nothing)
                        
                        ' b) Cecos / UD3 (Source)
                        Dim costCenterSourceId As Integer = api.Members.GetMemberId(dimTypeId.UD3, costCenterSourceParent)
                        Dim costCenterSourceBaseMembers As List(Of Member) = api.Members.GetBaseMembers(dimPkUD3, costCenterSourceId, Nothing)

                        ' c) Technology / UD1 (Driver)
                        Dim technologyDriverId As Integer = api.Members.GetMemberId(dimTypeId.UD1, technologyDriverParent)
                        Dim technologyDriverBaseMembers As List(Of Member) = api.Members.GetBaseMembers(dimPkUD1, technologyDriverId, Nothing)

                        If accountSourceBaseMembers.Count = 0 Or costCenterSourceBaseMembers.Count = 0 Or technologyDriverBaseMembers.Count = 0 Then
                            BRApi.ErrorLog.LogMessage(si, $"Advertencia: Alguna lista de miembros base está vacía para Seq {dr("sequence")}. Saltando.")
                            Continue For
                        End If

                        ' --- BUCLES ANIDADOS ---
                        ' 1. Bucle Cuentas
                        For Each accountMember As Member In accountSourceBaseMembers
                            Dim accountMemberName As String = accountMember.Name
                            
                            ' 2. Bucle Cost Centers (UD3)
                            For Each costCenterMember As Member In costCenterSourceBaseMembers
                                Dim costCenterMemberName As String = costCenterMember.Name
                                
                                ' PASO 1: LEER VALOR DEL ORIGEN
                                Dim origenPOV As String = $"Cb#SERV:E#{entitySource}:C#Local:S#ACT:T#{paramAllocationsTime}:V#YTD:A#{accountMemberName}:F#Top:O#Top:I#Top:U1#None:U2#None:U3#{costCenterMemberName}:U4#None:U5#None:U6#None:U7#None:U8#None"
                                
                                Dim valorOrigen As Decimal = api.Data.GetDataCell(origenPOV).CellAmount
                                
                                ' Optimización: Si origen es 0, no iterar drivers
                                If valorOrigen = 0D Then Continue For

                                ' 3. Bucle Technology (Driver UD1)
                                For Each technologyMember As Member In technologyDriverBaseMembers
                                    Dim technologyMemberName As String = technologyMember.Name
                                    
                                    ' PASO 2: LEER PORCENTAJE DEL DRIVER
                                    Dim driverPOV As String = $"Cb#SERV:E#{entitySource}:C#Local:S#BUD:T#{paramAllocationsTime}:V#YTD:A#{accountDriver}:F#Top:O#Top:I#Top:U1#{technologyMemberName}:U2#None:U3#TOT:U4#None:U5#None:U6#None:U7#None:U8#None"
                                    
                                    Dim porcentajeDriver As Decimal = api.Data.GetDataCell(driverPOV).CellAmount
                                    
                                    If porcentajeDriver = 0D Then Continue For
                                    
                                    ' PASO 3: CALCULAR
                                    Dim valorAlocar As Decimal = valorOrigen * porcentajeDriver
                                    
                                    ' PASO 4: ESCRIBIR EN DESTINO
                                    Dim destinoPOV As String = $"Cb#SERV:E#{entityTarget}:S#ACT:T#{paramAllocationsTime}:V#YTD:A#{accountMemberName}:F#None:O#Forms:I#None:U1#{technologyMemberName}:U2#None:U3#{costCenterMemberName}:U4#None:U5#None:U6#None:U7#None:U8#None"
                                    
                                    api.Data.Calculate($"{destinoPOV} = {valorAlocar.ToString("G", CultureInfo.InvariantCulture)}", True)
                                    
                                Next ' Next Driver/Tech
                            Next ' Next UD3
                        Next ' Next Account
                        
                    Catch exRow As Exception
                         BRApi.ErrorLog.LogMessage(si, $"Error procesando secuencia {dr("sequence")}: {exRow.Message}")
                    End Try
                Next ' Next Row

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Sub

        Public Sub Delete(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)
            Try
                ' 1. Obtener Parámetro de Tiempo (Reutilizando lógica robusta)
                Dim paramAllocationsTime As String = String.Empty

                ' Intentar obtener p_query_params
                If args.CustomCalculateArgs.NameValuePairs.ContainsKey("p_query_params") Then
                    Dim queryParams As String = args.CustomCalculateArgs.NameValuePairs("p_query_params")
                    Dim parameterDict As Dictionary(Of String, String) = Me.ParseQueryParams(si, queryParams)
                    
                    If parameterDict.ContainsKey("p_time") Then
                        paramAllocationsTime = parameterDict("p_time").Trim()
                    End If
                End If

                ' Fallbacks
                If String.IsNullOrEmpty(paramAllocationsTime) AndAlso args.CustomCalculateArgs.NameValuePairs.ContainsKey("p_time") Then
                    paramAllocationsTime = args.CustomCalculateArgs.NameValuePairs("p_time").Trim()
                End If
                
                 If String.IsNullOrEmpty(paramAllocationsTime) AndAlso args.CustomCalculateArgs.NameValuePairs.ContainsKey("prm_allocations_time") Then
                    paramAllocationsTime = args.CustomCalculateArgs.NameValuePairs("prm_allocations_time").Trim()
                End If

                If String.IsNullOrEmpty(paramAllocationsTime) Then
                    Throw New Exception("El parámetro p_time es obligatorio para ejecutar el borrado.")
                End If
                
                ' DEBUG: Verificar contexto de entidad
                Dim contextEntity As String = api.Pov.Entity.Name
                BRApi.ErrorLog.LogMessage(si, $"🔍 DEBUG DELETE: Iniciando borrado. Context Entity: '{contextEntity}' | Time: '{paramAllocationsTime}'")

                ' POV para borrado adaptado: Cb#SERV:S#ACT:O#Forms:U1#AM_OTH
                ' Se borra solo en Technology=AM_OTH
                Dim povToDelete As String = $"Cb#SERV:S#ACT:F#None:O#Forms:I#None:U1#AM_OTH:T#{paramAllocationsTime}"

                BRApi.ErrorLog.LogMessage(si, $"Borrando datos en {povToDelete}")
                
                ' Limpiar datos calculados (Data, Comments, Attachments, Annotation)
                api.Data.ClearCalculatedData(povToDelete, True, True, True, True)
                
                BRApi.ErrorLog.LogMessage(si, $"✅ Datos borrados correctamente para {paramAllocationsTime}")
                
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Sub

        Private Function ParseQueryParams(ByVal si As SessionInfo, ByVal queryParams As String) As Dictionary(Of String, String)
            Try
                Dim paramDict As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

                If String.IsNullOrEmpty(queryParams) Then
                    Return paramDict
                End If

                Dim pairs As String() = queryParams.Split(","c)

                For Each pair As String In pairs
                    Dim parts As String() = pair.Split("="c)
                    If parts.Length = 2 Then
                        Dim key As String = parts(0).Trim()
                        Dim value As String = parts(1).Trim()
                        paramDict(key) = value
                    End If
                Next

                Return paramDict

            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try
        End Function
    End Class
End Namespace