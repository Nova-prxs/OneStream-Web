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
	Public Class dynamic_dashboard_main_service
		Implements IWsasDynamicDashboardsV800
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
        Public Function GetEmbeddedDynamicDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx Implements IWsasDynamicDashboardsV800.GetEmbeddedDynamicDashboard
            Try
                If (api IsNot Nothing) Then
					If storedDashboard.Name.XFEqualsIgnoreCase("Investments_Cards_Test") Then
						'Prepare a list of repeated components
						Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
						
						'Get KPIs
						Dim dt As DataTable = BRApi.Import.Data.FdxExecuteCubeView(
							si,
							BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "Investments"),
							"cv_LandingPageSideMenuDev_KPIs",
							"",
							"",
							"",
							"",
							"",
							Nothing,
							False,
							True,
							"",
							8,
							False
						)
						
						'Get last column name to know the scenario we are comparing and set the comment
						Dim lastColumnName As String = dt.Columns(dt.Columns.Count - 1).ColumnName
						Dim scenarioToCompare As String = lastColumnName.Substring(1)
						Dim scenarioToCompareDescription As String = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Scenario, scenarioToCompare).Description
						Dim commentParam As String = $"Vs {scenarioToCompareDescription}"
						
						For Each row As DataRow In dt.Rows
							'Get account Description
							Dim accountDescription As String = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, row("account")).Description
							'Set format param based on account description
							Dim formatParam As String = String.Empty
							If accountDescription.Contains("%") Then
								formatParam = "0.00\%"
							Else
								formatParam = "#,##,.00M"
							End If
							Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
								{"title", accountDescription},
								{"first_data", Decimal.Round(row("VActual"), 2)},
								{"second_data", Decimal.Round(row(lastColumnName), 2)},
								{"comment", commentParam},
								{"is_green_on_positive", 1},
								{"format", formatParam}
							}
							repeatArgs.Add(New WsDynamicComponentRepeatArgs(accountDescription, nextLevelTemplateSubstVarsToAdd))
						Next
						
						'Set up embedded dashboard
						Dim contentDashboard As WsDynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, _
							workspace, parentDynamicComponentEx, storedDashboard, _
							String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown
						)
						
						'Attach our list of repeaters
						contentDashboard.DynamicDashboard.Tag = repeatArgs
						
						'Save the state and return the dashboard
						api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, _
							contentDashboard, WsDynamicItemStateType.NotUsed
						)
						If Not contentDashboard.DynamicDashboard.Dashboard Is Nothing Then Return contentDashboard
							
					Else If storedDashboard.Name.XFEqualsIgnoreCase("Investments_Card")
						'Get data
						Dim firstData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("first_data"))
						Dim secondData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("second_data"))
						Dim diffPercent As Decimal = If(secondData = 0, 0, (firstData - secondData) / secondData * 100)
						'Convert title to title case
						parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
							parentDynamicComponentEx.TemplateSubstVars("title"),
							vbProperCase
						)
						'Get If the color is green or red
						Dim isGreen As Boolean = firstData >= secondData
						'Change if is not green on positive
						isGreen = If(parentDynamicComponentEx.TemplateSubstVars("is_green_on_positive") = 1, isGreen, Not isGreen)
						
						'Set color and variance percent parameters
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
							{"variance_color", If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|")},
							{"variance_bg_color", If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|")},
							{"diff_percentage", String.Format(
								"{0:0.0\%}",
								diffPercent
							)}
						}
						'Format data to show based on the format parameter
						parentDynamicComponentEx.TemplateSubstVars("first_data") = String.Format(
							"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
							CDec(parentDynamicComponentEx.TemplateSubstVars("first_data"))
						)
						parentDynamicComponentEx.TemplateSubstVars("second_data") = String.Format(
							"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
							CDec(parentDynamicComponentEx.TemplateSubstVars("second_data"))
						)
						'Set up embedded dashboard
						Dim contentDashboard As WsDynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, _
							workspace, parentDynamicComponentEx, storedDashboard, _
							String.Empty, nextLevelTemplateSubstVarsToAdd, TriStateBool.Unknown, WsDynamicItemStateType.Unknown
						)
						'Save the state and return the dashboard
						api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, _
							contentDashboard, WsDynamicItemStateType.NotUsed
						)
						If Not contentDashboard.DynamicDashboard.Dashboard Is Nothing Then Return contentDashboard
					End If
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
					If dynamicDashboardEx.DynamicDashboard.Name.XFEqualsIgnoreCase("Investments_Cards_Test") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
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
