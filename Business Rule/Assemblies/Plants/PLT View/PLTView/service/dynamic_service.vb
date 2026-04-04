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

					If dynamicDashboardEx.DynamicDashboard.Name.XFEqualsIgnoreCase("EDIT_PLTView_MainNavigation_Main_Left_Tabs") Then

						Dim comps As WsDynamicComponentCollection = api.GetDynamicComponentsForDynamicDashboard(si, workspace, _
																                        dynamicDashboardEx, String.Empty, Nothing, TriStateBool.Unknown, _
																                        WsDynamicItemStateType.Unknown)																	
						Dim repeatArgsList As New List (Of WsDynamicComponentRepeatArgs)
						
						Dim userName As String = si.UserName
						Dim userGroupsList As New List(Of String)
						For Each kvp As KeyValuePair(Of Guid, Group) In BRApi.Security.Admin.GetUser(si, si.UserName).ParentGroups 								
							userGroupsList.Add(kvp.Value.Name)																	
						Next
						
						For Each component As WsDynamicDbrdCompMemberEx In comps.Components	
							
							Dim wsComponent As WsDynamicComponentEx = component.DynamicComponentEx
							Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String)
							
							For Each variable As String In wsComponent.TemplateSubstVars.Keys								
								If variable = "securitygroup" Then
									Dim securityGroup As String = wsComponent.TemplateSubstVars(variable)
									
'									If userGroupsList.Contains(securityGroup)
'										' Grupos de seguridad de los usuarios
'									End If
									
'									If brapi.Security.Authorization.IsUserInGroup(si, si.UserName, securityGroup, True) Then
'									 	' Con función directamente
'										nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
'										nextLevelTemplateSubstVarsToAdd.Add("view", "True")
'									Else
										
'										nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
'										nextLevelTemplateSubstVarsToAdd.Add("view", "False")
'									End If
									
									' Ejemplo con los nombres
									If securityGroup = userName Then
										' brapi.ErrorLog.LogMessage(si, wsComponent.AncestorNameSuffix & " - No mostrar")
										nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
										nextLevelTemplateSubstVarsToAdd.Add("view", "True")
										
									Else 
										' brapi.ErrorLog.LogMessage(si, wsComponent.AncestorNameSuffix & " - Mostrar")
										nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
										nextLevelTemplateSubstVarsToAdd.Add("view", "False")
										nextLevelTemplateSubstVarsToAdd.Add("height", "1")
										nextLevelTemplateSubstVarsToAdd.Add("width", "1")
									End If
								' Else If variable <> "view"
									' nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
								Else 
									nextLevelTemplateSubstVarsToAdd.Add(variable, wsComponent.TemplateSubstVars(variable))
								End If							
							Next
							
							repeatArgsList.Add(New WsDynamicComponentRepeatArgs(component.DynamicComponentEx.AncestorNameSuffix, nextLevelTemplateSubstVarsToAdd))																									
							
						Next
						
                    	Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si,workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					End If
					
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
