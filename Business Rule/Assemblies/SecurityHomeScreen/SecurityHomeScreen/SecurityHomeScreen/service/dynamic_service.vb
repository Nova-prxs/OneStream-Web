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
	Public Class dynamic_service
		Implements IWsasDynamicDashboardsV800

        Public Function GetEmbeddedDynamicDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx Implements IWsasDynamicDashboardsV800.GetEmbeddedDynamicDashboard
            Try
                If (api IsNot Nothing) Then
                    Return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

		Public Function GetDynamicComponentsForDynamicDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
		    ByVal maintUnit As DashboardMaintUnit, ByVal dynamicDashboardEx As WsDynamicDashboardEx, ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) _
		    As WsDynamicComponentCollection Implements IWsasDynamicDashboardsV800.GetDynamicComponentsForDynamicDashboard
		    Try
		        If (api IsNot Nothing) Then
		
		            ' Log el nombre del dashboard para debugging
		            ' BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Processing dashboard: '{dynamicDashboardEx.DynamicDashboard.Name}'")
		
		            ' Verificar si es un dashboard que necesita filtrado por seguridad
		            ' Usar Contains para ser más flexible con los nombres
		            If dynamicDashboardEx.DynamicDashboard.Name.XFEqualsIgnoreCase("EDIT_SecurityHomeScreen_MainNavigation_Main_Left_Tabs") OrElse _
		               dynamicDashboardEx.DynamicDashboard.Name.Contains("_LeftTabs_Tabs") Then
		
		                ' BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Dashboard '{dynamicDashboardEx.DynamicDashboard.Name}' matches security filter - processing components")
		
		                Dim comps As WsDynamicComponentCollection = api.GetDynamicComponentsForDynamicDashboard(si, workspace, _
		                                                                        dynamicDashboardEx, String.Empty, Nothing, TriStateBool.Unknown, _
		                                                                        WsDynamicItemStateType.Unknown)
		                Dim repeatArgsList As New List(Of WsDynamicComponentRepeatArgs)
		                
		                Dim userName As String = si.UserName
		                Dim userGroupsList As New List(Of String)
		                For Each kvp As KeyValuePair(Of Guid, Group) In BRApi.Security.Admin.GetUser(si, si.UserName).AncestorGroups
		                    userGroupsList.Add(kvp.Value.Name)
		                Next
		                
		                ' Log información del usuario y sus grupos
'		                BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] User: '{userName}' belongs to groups: {String.Join(", ", userGroupsList)}")
		                
		                For Each component As WsDynamicDbrdCompMemberEx In comps.Components
		                    
		                    Dim wsComponent As WsDynamicComponentEx = component.DynamicComponentEx
		                    Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String)
		                    Dim shouldCreateComponent As Boolean = True ' Flag para determinar si crear el componente
		                    
		                    For Each variable As String In wsComponent.TemplateSubstVars.Keys
		                        If variable = "securitygroup" Then
		                            Dim securityGroup As String = wsComponent.TemplateSubstVars(variable)
		                            
		                            ' Log información básica
'		                            BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Checking if user belongs to securitygroup='{securityGroup}'")
		                            
		                            ' Verificar si el usuario pertenece al grupo de seguridad (usando la jerarquía de OneStream)
		                            Dim userBelongsToGroup As Boolean = userGroupsList.Any(Function(grp) String.Equals(grp, securityGroup, StringComparison.OrdinalIgnoreCase))
		                            
		                            If userBelongsToGroup Then
		                                nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
		                                shouldCreateComponent = True
'		                                BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Component will be CREATED - user '{userName}' belongs to group '{securityGroup}'")
		                            Else
		                                ' NO crear el componente para este usuario
		                                shouldCreateComponent = False
'		                                BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Component will be SKIPPED - user '{userName}' does NOT belong to group '{securityGroup}'")
		                            End If
		                        Else
		                            nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
		                        End If
		                    Next
		                    
		                    ' Solo agregar el componente si el usuario tiene permisos
		                    If shouldCreateComponent Then
		                        repeatArgsList.Add(New WsDynamicComponentRepeatArgs(component.DynamicComponentEx.AncestorNameSuffix, nextLevelTemplateSubstVarsToAdd))
'		                        BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Component ADDED to repeatArgsList")
		                    Else
'		                        BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Component NOT ADDED to repeatArgsList - will not be created")
		                    End If
		                    
		                Next
		                
		                Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
		            End If
		            
		            ' Para dashboards que no necesitan filtrado por seguridad
'		            BRApi.ErrorLog.LogMessage(si, $"[SecurityHomeScreen] Dashboard '{dynamicDashboardEx.DynamicDashboard.Name}' does NOT match security filter - using default processing")
		            Return api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
		        End If
		
		        Return Nothing
		    Catch ex As Exception
		        Throw New XFException(si, ex)
		    End Try
		End Function

        Public Function GetDynamicAdaptersForDynamicComponent(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal dynamicComponentEx As WsDynamicComponentEx, ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) _
            As WsDynamicAdapterCollection Implements IWsasDynamicDashboardsV800.GetDynamicAdaptersForDynamicComponent
            Try
                If (api IsNot Nothing) Then
                    Return api.GetDynamicAdaptersForDynamicComponent(si, workspace, dynamicComponentEx, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

        Public Function GetDynamicCubeViewForDynamicAdapter(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal dynamicAdapterEx As WsDynamicAdapterEx, ByVal storedCubeViewItem As CubeViewItem, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicCubeViewEx Implements IWsasDynamicDashboardsV800.GetDynamicCubeViewForDynamicAdapter
            Try
                If (api IsNot Nothing) Then
                    Return api.GetDynamicCubeViewForDynamicAdapter(si, workspace, dynamicAdapterEx, storedCubeViewItem, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
                End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

        Public Function GetDynamicParametersForDynamicComponent(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal dynamicComponentEx As WsDynamicComponentEx, ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) _
            As WsDynamicParameterCollection Implements IWsasDynamicDashboardsV800.GetDynamicParametersForDynamicComponent
            Try
                If (api IsNot Nothing) Then
                    Return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx, String.Empty, Nothing, TriStateBool.FalseValue, WsDynamicItemStateType.Unknown)
                End If

                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
        End Function
		
		Public Function GetDynamicCubeViewForDynamicComponent(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
			ByVal maintUnit As DashboardMaintUnit, ByVal dynamicComponentEx As WsDynamicComponentEx, ByVal storedCubeViewItem As CubeViewItem, _
			ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicCubeViewEx Implements IWsasDynamicDashboardsV800.GetDynamicCubeViewForDynamicComponent
		    Try
		        If api IsNot Nothing Then
		            Return api.GetDynamicCubeViewForDynamicComponent(si, workspace, dynamicComponentEx, storedCubeViewItem, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
		        End If
		        Return Nothing
		    Catch ex As Exception
		        Throw New XFException(si, ex)
		    End Try
		End Function

	End Class
	
End Namespace
