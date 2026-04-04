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
					 
					#Region "Cards"
					
					If storedDashboard.Name.Contains("LandingPageDev_Cards_") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						
						Return Me.GetCardRepeaterDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)

					Else If storedDashboard.Name.XFEqualsIgnoreCase("LandingPageDev_Card") AndAlso customSubstVarsAlreadyResolved.Count > 0
						
						'Declare dictionary of template parameters
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String)
						
						'Get data
						Dim firstData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("first_data"))
						Dim secondData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("second_data"))
						
						'Convert title to title case
						If parentDynamicComponentEx.TemplateSubstVars("is_title") = 1 Then
							parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
								parentDynamicComponentEx.TemplateSubstVars("title"),
								vbProperCase
							)
						End If
						
						'Convert measure to title case
						parentDynamicComponentEx.TemplateSubstVars("measure") = StrConv(
							parentDynamicComponentEx.TemplateSubstVars("measure"),
							vbProperCase
						)
						
						'Get if it has variance or not
						If parentDynamicComponentEx.TemplateSubstVars("has_variance") = "1" Then
							'Set variance and variance format depending on variance type
							Dim variance As Decimal
							Dim varianceFormat As String = String.Empty
							If parentDynamicComponentEx.TemplateSubstVars("variance_type") = "%" Then
								variance = If(secondData = 0, 0, (firstData - secondData) / secondData * 100)
								varianceFormat = "{0:0.0\%}"
							Else
								variance = firstData - secondData
								varianceFormat = "{0:" &
									parentDynamicComponentEx.TemplateSubstVars("format").Split("\")(0) &
									"\" & parentDynamicComponentEx.TemplateSubstVars("variance_type") & "}"
							End If
							
							'Get If the color is green or red
							Dim isGreen As Boolean = firstData >= secondData
							'Change if is not green on positive
							isGreen = If(parentDynamicComponentEx.TemplateSubstVars("is_green_on_positive") = 1, isGreen, Not isGreen)
							
							'Set color and variance percent parameters
							nextLevelTemplateSubstVarsToAdd.Add("variance_color", If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|"))
							nextLevelTemplateSubstVarsToAdd.Add("variance_bg_color", If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|"))
							nextLevelTemplateSubstVarsToAdd.Add("variance", String.Format(varianceFormat, variance))
							
							'Set second data formatted
							parentDynamicComponentEx.TemplateSubstVars("second_data") = String.Format(
								"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
								secondData
							)
						Else
							'Set color and variance percent parameters
							nextLevelTemplateSubstVarsToAdd.Add("variance_color", "Transparent")
							nextLevelTemplateSubstVarsToAdd.Add("variance_bg_color", "Transparent")
							nextLevelTemplateSubstVarsToAdd.Add("variance", "")
							
							'Set second data and comment to nothing
							parentDynamicComponentEx.TemplateSubstVars("second_data") = ""
							parentDynamicComponentEx.TemplateSubstVars("comment") = ""
						End If
						
						'Format data to show based on the format parameter
						parentDynamicComponentEx.TemplateSubstVars("first_data") = String.Format(
							"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
							firstData
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
							
					#End Region
					
					#Region "Double Cards"
					
					Else If storedDashboard.Name.Contains("LandingPageDev_DoubleCards_") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						
						Return Me.GetDoubleCardRepeaterDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)

					Else If storedDashboard.Name.XFEqualsIgnoreCase("LandingPageDev_DoubleCard") AndAlso customSubstVarsAlreadyResolved.Count > 0
						'Convert title to title case
						If parentDynamicComponentEx.TemplateSubstVars("is_title") = 1 Then
							parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
								parentDynamicComponentEx.TemplateSubstVars("title"),
								vbProperCase
							)
						End If
						
						'For loop to set the two cards parameters
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String)
						For Each cardNumber In {"first", "second"}
							'Get data
							Dim firstData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_first_data"))
							Dim secondData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_second_data"))
						
							'Get if it has variance or not
							If parentDynamicComponentEx.TemplateSubstVars($"has_variance") = "1" Then
								'Set variance and variance format depending on variance type
								Dim variance As Decimal
								Dim varianceFormat As String = String.Empty
								If parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_variance_type") = "%" Then
									variance = If(secondData = 0, 0, (firstData - secondData) / secondData * 100)
									varianceFormat = "{0:0.0\%}"
								Else
									variance = firstData - secondData
									varianceFormat = "{0:" &
										parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_format").Split("\")(0) &
										"\" & parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_variance_type") & "}"
								End If
								
								'Get If the color is green or red
								Dim isGreen As Boolean = firstData >= secondData
								'Change if is not green on positive
								isGreen = If(parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_is_green_on_positive") = 1, isGreen, Not isGreen)
								
								'Set color and variance percent parameters
								nextLevelTemplateSubstVarsToAdd.Add($"{cardNumber}_card_variance_color", If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|"))
								nextLevelTemplateSubstVarsToAdd.Add($"{cardNumber}_card_variance_bg_color", If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|"))
								nextLevelTemplateSubstVarsToAdd.Add($"{cardNumber}_card_variance", String.Format(varianceFormat, variance))
								
								'Set second data formatted
								parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_second_data") = String.Format(
									"{0:" & parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_format") & "}",
									secondData
								)
							Else
								'Set color and variance percent parameters
								nextLevelTemplateSubstVarsToAdd.Add($"{cardNumber}_card_variance_color", "Transparent")
								nextLevelTemplateSubstVarsToAdd.Add($"{cardNumber}_card_variance_bg_color", "Transparent")
								nextLevelTemplateSubstVarsToAdd.Add($"{cardNumber}_card_variance", "")
								
								'Set second data and comment to nothing
								parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_second_data") = ""
								parentDynamicComponentEx.TemplateSubstVars("comment") = ""
							End If
						
'							'Set variance and variance format depending on variance type
'							Dim variance As Decimal
'							Dim varianceFormat As String = String.Empty
'							If parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_variance_type") = "%" Then
'								variance = If(secondData = 0, 0, (firstData - secondData) / secondData * 100)
'								varianceFormat = "{0:0.0\%}"
'							Else
'								variance = firstData - secondData
'								varianceFormat = "{0:" &
'									parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_format").Split("\")(0) &
'									"\" & parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_variance_type") & "}"
'							End If
						
'							'Get If the color is green or red
'							Dim isGreen As Boolean = firstData >= secondData
'							'Change if is not green on positive
'							isGreen = If(parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_is_green_on_positive") = 1, isGreen, Not isGreen)
							
'							'Set color and variance percent parameters
'							nextLevelTemplateSubstVarsToAdd.Add(
'								$"{cardNumber}_card_variance_color",
'								If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|")
'							)
'							nextLevelTemplateSubstVarsToAdd.Add(
'								$"{cardNumber}_card_variance_bg_color",
'								If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|")
'							)
'							nextLevelTemplateSubstVarsToAdd.Add(
'								$"{cardNumber}_card_variance",
'								String.Format(
'										varianceFormat,
'										variance
'								)
'							)
							
							'Format data to show based on the format parameter
							parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_first_data") = String.Format(
								"{0:" & parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_format") & "}",
								firstData
							)
						Next
				
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
						
					#End Region
					
					#Region "Completion Cards"
					
					Else If storedDashboard.Name.Contains("LandingPageDev_CompletionCards_") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						
						Return Me.GetCompletionCardRepeaterDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)

					Else If storedDashboard.Name.XFEqualsIgnoreCase("LandingPageDev_CompletionCard") AndAlso customSubstVarsAlreadyResolved.Count > 0
						'Get data
						Dim firstData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("first_data"))
						Dim secondData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("second_data"))
						Dim completionPercentage As Decimal = If(secondData = 0, 100, firstData / secondData * 100)
						
						'Convert title to title case
						If parentDynamicComponentEx.TemplateSubstVars("is_title") = 1 Then
							parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
								parentDynamicComponentEx.TemplateSubstVars("title"),
								vbProperCase
							)
						End If
						
						'Get If the color is green or red
						Dim isGreen As Boolean = firstData >= secondData
						'Change if is not green on positive
						isGreen = If(parentDynamicComponentEx.TemplateSubstVars("is_green_on_positive") = 1, isGreen, Not isGreen)
						
						'If is green was set up outside, use that
						isGreen = If(
							parentDynamicComponentEx.TemplateSubstVars.ContainsKey("is_green"),
							parentDynamicComponentEx.TemplateSubstVars("is_green") = 1,
							isGreen
						)
						
						'Set color and variance percent parameters
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
							{"completion_percentage_color", If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|")},
							{"completion_percentage_bg_color", If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|")},
							{"completion_percentage", String.Format(
								"{0:0.0\%}",
								completionPercentage
							)}
						}
						'Format data to show based on the format parameter
						parentDynamicComponentEx.TemplateSubstVars("first_data") = String.Format(
							"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
							firstData
						)
						parentDynamicComponentEx.TemplateSubstVars("second_data") = String.Format(
							"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
							secondData
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
					
					#End Region
						
					#Region "PnL"
					
					Else If storedDashboard.Name.XFEqualsIgnoreCase("LandingPageDev_PnL_Main_Left") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						'Prepare a list of repeated components
						Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
						
						'Get accounts and ud4 to get the data
						Dim accountList As String() = BRApi.Dashboards.Parameters _
							.GetLiteralParameterValue(si, False, "LandingPageDev.LP_param_Accounts_IncomeStatement").Split(",") _
							.Select(Function(x) x.Replace("A#", "")).ToArray()
						
						For Each account As String In accountList
							'Get account Description
							Dim accountDescription As String = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, account).Description
							
							Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
								{"account_description", accountDescription}
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
					
					Else If storedDashboard.Name.XFEqualsIgnoreCase("LandingPageDev_PnL_Main_Right") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						'Prepare a list of repeated components
						Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
						
						'Set a bar max width and declare a bar for the amount to compare
						Dim barMaxWidth As Decimal = 140
						Dim barMaxAmount As Decimal = 0
						
						'Get accounts and ud4 to get the data
						Dim accountList As String() = BRApi.Dashboards.Parameters _
							.GetLiteralParameterValue(si, False, "LandingPageDev.LP_param_Accounts_IncomeStatement").Split(",") _
							.Select(Function(x) x.Replace("A#", "")).ToArray()
						
						'Get time member and view
						Dim timeMember As String = customSubstVarsAlreadyResolved("LP_param_POV_Time")
						Dim viewMember As String = customSubstVarsAlreadyResolved("LP_param_POV_View")
						
						'Declare a dictionary of acount to bar width and color
						Dim accountToBarDictionary As New Dictionary(Of String, Dictionary(Of String, String))
						For Each account In accountList
							accountToBarDictionary(account) = New Dictionary(Of String, String) From {
								{
									"amount",
									BRApi.Finance.Data.GetDataCellUsingMemberScript(
										si,
										"Horse",
										$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:P#:C#Local:S#Actual:T#{timeMember}:V#{viewMember}:A#{account}:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
									).DataCellEx.DataCell.CellAmount
								},
								{"is_green_on_positive", "1"}
							}
						Next
						
						'Set max amount
						barMaxAmount = CDec(accountToBarDictionary(accountList(0))("amount"))
						For Each kvp In accountToBarDictionary
							'Get if it's green
							Dim isGreen As Boolean = CDec(kvp.Value("amount")) >= 0
							isGreen = If(kvp.Value("is_green_on_positive") = 1, isGreen, Not isGreen)
							'Calculate bar width and define the parameters
							Dim barWidth As String = If(
								barMaxAmount = 0,
								0,
								Integer.Max(2, Integer.Abs(CInt(barMaxWidth / barMaxAmount * CDec(kvp.Value("amount")))))
							)
							Dim barRestWidth As String = CInt((barMaxWidth - CDec(barWidth)) / 2)
							Dim barColor As String = If(
								isGreen,
								"|!Shared.prm_Style_Colors_Green!|",
								"|!Shared.prm_Style_Colors_Red!|"
							)
'							Dim amount As String = If(
'								kvp.Key = accountList(0),
'								"",
'								String.Format(
'									"{0:#,##,.0M;(#,##,.0M)}",
'									CDec(kvp.Value("amount"))
'								)
'							)
							Dim amount As String = String.Format(
								"{0:#,##,.0M;(#,##,.0M)}",
								CDec(kvp.Value("amount"))
							)
							Dim percentageAmount As String = String.Format(
								"{0:0.0\%;(0.0\%)}",
								CDec(If(barMaxAmount = 0, 0, kvp.Value("amount")) / barMaxAmount * 100)
							)
							
							Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
								{"amount", $"{amount} {percentageAmount}"},
								{"bar_rest_width", barRestWidth},
								{"bar_width", barWidth},
								{"bar_color", barColor}
							}
							repeatArgs.Add(New WsDynamicComponentRepeatArgs(kvp.Key, nextLevelTemplateSubstVarsToAdd))
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
						
					#End Region
						
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
					If dynamicDashboardEx.DynamicDashboard.Name.Contains("LandingPageDev_Cards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LandingPageDev_DoubleCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LandingPageDev_CompletionCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.XFEqualsIgnoreCase("LandingPageDev_PnL_Main_Left") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.XFEqualsIgnoreCase("LandingPageDev_PnL_Main_Right") Then
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

		#Region "Main Functions"
		
		#Region "Cards"
		
		#Region "Get Card Repeater Dashboard"
		
		Private Function GetCardRepeaterDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx
			'Prepare a list of repeated components
			Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
			
			'Get account parameter based on the dashboard
			Dim dashboardSuffix As String = storedDashboard.Name.Split("_")(storedDashboard.Name.Split("_").Count - 1)
			Dim KPIAccountParameter As String = $"LandingPageDev.LP_param_KPIs_Account_{dashboardSuffix}"
			
			'Set default dimension member dictionary
			Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
				{"P", Me.GetParentFromEntity(si, customSubstVarsAlreadyResolved("LP_param_POV_Entity"))},
				{"A", ""},
				{"C", "Local"},
				{"F", "Top"},
				{"O", "Top"},
				{"I", "Top"},
				{"U1", "Top"},
				{"U2", "Top"},
				{"U3", "Top"},
				{"U4", "Top"},
				{"U5", "Top"},
				{"U6", "Top"},
				{"U7", "None"},
				{"U8", "None"}
			}
			
			'Get aggregation level and set new defaults in the dicitonary
			If Not String.IsNullOrEmpty(customSubstVarsAlreadyResolved("LP_param_POV_Aggregation_Level")) Then
				Dim aggregationLevelMemberList As String() = customSubstVarsAlreadyResolved("LP_param_POV_Aggregation_Level").Split(":")
				For Each aggregationLevelMember In aggregationLevelMemberList
					Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
					defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
				Next
			End If
			
			'Get intersection list to get the data
			Dim intersectionList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, KPIAccountParameter).Split(",")
			
			'Get scenario to compare, time member and view
			Dim scenarioToCompare As String = customSubstVarsAlreadyResolved("LP_param_POV_ReferenceScenario")
			Dim timeMember As String = customSubstVarsAlreadyResolved("LP_param_POV_Time")
			Dim viewMember As String = customSubstVarsAlreadyResolved("LP_param_POV_View")
			Dim scenarioToCompareDescription As String = If(
				String.IsNullOrEmpty(scenarioToCompare),
				"",
				BRApi.Finance.Metadata.GetMember(si, dimTypeId.Scenario, scenarioToCompare).Description
			)
			Dim commentParam As String = $"Vs {scenarioToCompareDescription}"
			
			For Each intersection In intersectionList
				'Get intersection dictionary based on intersection
				Dim intersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, intersection)
				'Get member filter based on intersection
				Dim memberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, intersectionDictionary)
				
				'Get account Description
				Dim accountMember As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A"))
				Dim accountDescription As String = If(
					Not accountMember Is Nothing,
					BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A")).Description,
					"Account Not Found"
				)
				
				'Get data from scenarios
				Dim actualAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
					si,
					"Horse",
					$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:P#:S#Actual:T#{timeMember}:V#{viewMember}{memberFilter}"
				).DataCellEx.DataCell.CellAmount
				Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
					si,
					"Horse",
					$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{memberFilter}"
				).DataCellEx.DataCell.CellAmount
				
				'Set format param based on account description
				Dim formatParam As String = "#,#0,"
				Dim measureParam As String = "M€"
				Dim isGreenOnPositive As Integer = 1
				Dim hasVariance As Integer = 1
				Dim varianceType As String = "%"
				Dim isTitle As Integer = 1
				If accountDescription.Contains("%") Then
					formatParam = "0.0\%"
					varianceType = "pt"
				Else If accountDescription.ToLower.Contains("volume")
					measureParam = "K Un"
				Else If accountDescription.ToLower.Contains("sg&a")
					isTitle = 0
				Else If accountDescription.ToLower.Contains("inventory")
					isGreenOnPositive = 0
				Else If accountDescription.ToLower.Contains("overdues")
					hasVariance = 0
				End If
				
				'Populate template params and add collection component
				Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
					{"title", accountDescription},
					{"measure", measureParam},
					{"first_data", Decimal.Round(actualAmount, 2)},
					{"second_data", Decimal.Round(referenceAmount, 2)},
					{"variance_type", varianceType},
					{"has_variance", hasVariance},
					{"comment", commentParam},
					{"is_green_on_positive", isGreenOnPositive},
					{"is_title", isTitle},
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
			
		End Function
		
		#End Region
		
		#Region "Get Double Card Repeater Dashboard"
		
		Private Function GetDoubleCardRepeaterDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx
			'Prepare a list of repeated components
			Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
			'Get account parameter based on the dashboard
			Dim dashboardSuffix As String = storedDashboard.Name.Split("_")(storedDashboard.Name.Split("_").Count - 1)
			Dim KPIAccountParameter As String = $"LandingPageDev.LP_param_KPIs_Account_{dashboardSuffix}"
			'Set default dimension member dictionary
			Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
				{"P", Me.GetParentFromEntity(si, customSubstVarsAlreadyResolved("LP_param_POV_Entity"))},
				{"A", ""},
				{"C", "Local"},
				{"F", "Top"},
				{"O", "Top"},
				{"I", "Top"},
				{"U1", "Top"},
				{"U2", "Top"},
				{"U3", "Top"},
				{"U4", "Top"},
				{"U5", "Top"},
				{"U6", "Top"},
				{"U7", "None"},
				{"U8", "None"}
			}
			
			'Get aggregation level and set new defaults in the dicitonary
			If Not String.IsNullOrEmpty(customSubstVarsAlreadyResolved("LP_param_POV_Aggregation_Level")) Then
				Dim aggregationLevelMemberList As String() = customSubstVarsAlreadyResolved("LP_param_POV_Aggregation_Level").Split(":")
				For Each aggregationLevelMember In aggregationLevelMemberList
					Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
					defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
				Next
			End If
			
			'Get scenario to compare, time member and view
			Dim scenarioToCompare As String = customSubstVarsAlreadyResolved("LP_param_POV_ReferenceScenario")
			Dim timeMember As String = customSubstVarsAlreadyResolved("LP_param_POV_Time")
			Dim viewMember As String = customSubstVarsAlreadyResolved("LP_param_POV_View")
			Dim scenarioToCompareDescription As String = If(
				String.IsNullOrEmpty(scenarioToCompare),
				"",
				BRApi.Finance.Metadata.GetMember(si, dimTypeId.Scenario, scenarioToCompare).Description
			)
			Dim commentParam As String = $"Vs {scenarioToCompareDescription}"
			
			'Get each double card accounts
			Dim doubleCardList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, KPIAccountParameter).Split(";")
			
			'Loop through all the double cards
			For Each doubleCard In doubleCardList
			
				'Get intersection list to get the data
				Dim intersectionList As String() = doubleCard.Split(",")
				
				Dim accountDescriptionMain As String = String.Empty
				
				'Declare template params and add title and comment
				Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
					{"is_title", 0},
					{"has_variance", 1},
					{"comment", commentParam}
				}
				
				For i As Integer = 0 To intersectionList.Count - 1
					'Get intersection dictionary based on intersection
					Dim intersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, intersectionList(i))
					'Get member filter based on intersection
					Dim memberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, intersectionDictionary)
					
					'Get account Description
					Dim accountMember As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A"))
					Dim accountDescription As String = If(
						Not accountMember Is Nothing,
						BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A")).Description,
						"Account Not Found"
					)
					
					'Set title if it's first loop
					If i = 0 Then
						accountDescriptionMain = accountDescription
						nextLevelTemplateSubstVarsToAdd("title") = accountDescription
					End If
					
					'Set first or second string
					Dim cardNumberString As String = If(i = 0, "first", "second")
					'Get data from scenarios
					Dim actualAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:C#Local:S#Actual:T#{timeMember}:V#{viewMember}{memberFilter}"
					).DataCellEx.DataCell.CellAmount
					Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:C#Local:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{memberFilter}"
					).DataCellEx.DataCell.CellAmount
					
					'Set format param based on account description
					Dim formatParam As String = "#,#0,M€"
					Dim isGreenOnPositive As Integer = 1
					Dim varianceType As String = "%"
					If accountDescription.Contains("%") Then
						formatParam = "0.0\%"
						varianceType = "pt"
					Else If accountDescription.ToLower.Contains("volume")
						formatParam = "#,#0,K"
					Else If accountDescription.ToLower.Contains("inventory")
						isGreenOnPositive = 0
					End If
				
					nextLevelTemplateSubstVarsToAdd.Add($"{cardNumberString}_card_first_data", Decimal.Round(actualAmount, 2))
					nextLevelTemplateSubstVarsToAdd.Add($"{cardNumberString}_card_second_data", Decimal.Round(referenceAmount, 2))
					nextLevelTemplateSubstVarsToAdd.Add($"{cardNumberString}_card_is_green_on_positive", isGreenOnPositive)
					nextLevelTemplateSubstVarsToAdd.Add($"{cardNumberString}_card_variance_type", varianceType)
					nextLevelTemplateSubstVarsToAdd.Add($"{cardNumberString}_card_format", formatParam)
				Next
				
				'Add collection component to repeater
				repeatArgs.Add(New WsDynamicComponentRepeatArgs(accountDescriptionMain, nextLevelTemplateSubstVarsToAdd))
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
			
		End Function
		
		#End Region
		
		#Region "Get Completion Card Repeater Dashboard"
		
		Private Function GetCompletionCardRepeaterDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx
			'Prepare a list of repeated components
			Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
			
			'Get account parameter based on the dashboard
			Dim dashboardSuffix As String = storedDashboard.Name.Split("_")(storedDashboard.Name.Split("_").Count - 1)
			Dim KPIAccountParameter As String = $"LandingPageDev.LP_param_Completion_Account_{dashboardSuffix}"
			
			'Set default dimension member dictionary
			Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
				{"P", Me.GetParentFromEntity(si, customSubstVarsAlreadyResolved("LP_param_POV_Entity"))},
				{"A", ""},
				{"C", "Local"},
				{"F", "Top"},
				{"O", "Top"},
				{"I", "Top"},
				{"U1", "Top"},
				{"U2", "Top"},
				{"U3", "Top"},
				{"U4", "Top"},
				{"U5", "Top"},
				{"U6", "Top"},
				{"U7", "None"},
				{"U8", "None"}
			}
			
			'Get aggregation level and set new defaults in the dicitonary
			If Not String.IsNullOrEmpty(customSubstVarsAlreadyResolved("LP_param_POV_Aggregation_Level")) Then
				Dim aggregationLevelMemberList As String() = customSubstVarsAlreadyResolved("LP_param_POV_Aggregation_Level").Split(":")
				For Each aggregationLevelMember In aggregationLevelMemberList
					Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
					defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
				Next
			End If
			
			'Get intersection list to get the data
			Dim intersectionList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, KPIAccountParameter).Split(",")
			
			'Get scenario to compare, time member and view
			Dim scenarioToCompare As String = customSubstVarsAlreadyResolved("LP_param_POV_ReferenceScenario")
			Dim timeMember As String = customSubstVarsAlreadyResolved("LP_param_POV_Time")
			Dim viewMember As String = customSubstVarsAlreadyResolved("LP_param_POV_View")
			Dim scenarioToCompareDescription As String = If(
				String.IsNullOrEmpty(scenarioToCompare),
				"",
				BRApi.Finance.Metadata.GetMember(si, dimTypeId.Scenario, scenarioToCompare).Description
			)
			
			For Each intersection In intersectionList
				'Get intersection dictionary based on intersection
				Dim intersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, intersection)
				'Get member filter based on intersection
				Dim memberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, intersectionDictionary)
				
				'Get account Description
				Dim accountMember As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A"))
				Dim accountDescription As String = If(
					Not accountMember Is Nothing,
					BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A")).Description,
					"Account Not Found"
				)
				
				'Get data from scenarios
				Dim actualAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
					si,
					"Horse",
					$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:S#Actual:T#{timeMember}:V#{viewMember}{memberFilter}"
				).DataCellEx.DataCell.CellAmount
				Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
					si,
					"Horse",
					$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{memberFilter}"
				).DataCellEx.DataCell.CellAmount
				
				'Set format params based on account description
				Dim formatParam As String = "#,#0,M€"
				Dim isGreenOnPositive As Integer = 1
				If accountDescription.Contains("%") Then
					formatParam = "0.0\%"
				Else If accountDescription.ToLower.Contains("volume")
					formatParam = "#,#0,K"
				Else If accountDescription.ToLower.Contains("inventory")
					isGreenOnPositive = 0
				End If
				
				'Populate template params
				Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
					{"title", accountDescription},
					{"first_data_name", "Actual"},
					{"first_data", Decimal.Round(actualAmount, 2)},
					{"second_data_name", scenarioToCompareDescription},
					{"second_data", Decimal.Round(referenceAmount, 2)},
					{"is_green_on_positive", isGreenOnPositive},
					{"is_title", 1},
					{"format", formatParam}
				}
				
				'If it's expenses set up is_green parameter based on the difference also in revenues income
				If accountDescription.ToLower.Contains("expense") Then
					'Set first percentage and declare second percentage
					Dim expensePercentage As Decimal = If(referenceAmount = 0, 0, actualAmount / referenceAmount)
					
					'Get income intersection
					Dim incomeIntersection As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"LandingPageDev.LP_param_Completion_Account_Income")
					'Get intersection dictionary based on intersection
					Dim incomeIntersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, incomeIntersection)
					'Get member filter based on intersection
					Dim incomeMemberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, incomeIntersectionDictionary)
					
					'Get actual and reference amount for revenues and calculate second percentage
					Dim incomeActualAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:S#Actual:T#{timeMember}:V#{viewMember}{incomeMemberFilter}"
					).DataCellEx.DataCell.CellAmount
					Dim incomeReferenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LP_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{incomeMemberFilter}"
					).DataCellEx.DataCell.CellAmount
					
					'Calculate second percentage and add is_green
					Dim incomePercentage As Decimal = If(incomeReferenceAmount = 0, 0, incomeActualAmount / incomeReferenceAmount)
					nextLevelTemplateSubstVarsToAdd.Add("is_green", If(expensePercentage > incomePercentage, 0, 1))
				End If
				
				'Add collection component
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
			
		End Function
		
		#End Region
		
		#End Region
		
		#Region "Helper Functions"
		
		#Region "Get Parent From Entity"
		
		Private Function GetParentFromEntity(ByVal si As SessionInfo, ByVal entity As String) As String
			'Set parent depending on entity
			Dim parentEntity As String = Left(entity, 5)
			If entity = "Horse_Group" Then
				parentEntity = ""
			Else If entity.Length = 5 Then
				parentEntity = "Horse_Group"
			End If
			
			Return parentEntity
		End Function
		
		#End Region
		
		#Region "Get Intersection Dictionary"
		
		Private Function GetIntersectionDictionary(
			ByVal si As SessionInfo, ByVal intersection As String
		) As Dictionary(Of String, String)
			'Build a dictionary of the intersection
			Dim intersectionDictionary As New Dictionary(Of String, String)
			For Each dimensionMember In intersection.Split(":")
				Dim dimensionMemberSplitted As String() = dimensionMember.Split("#")
				intersectionDictionary(dimensionMemberSplitted(0)) = dimensionMemberSplitted(1)
			Next
			
			Return intersectionDictionary
		End Function
		
		#End Region
		
		#Region "Get Member Filter Based On Intersection"
		
		Private Function GetMemberFilterBasedOnIntersections(
			ByVal si As SessionInfo, ByVal defaultDimensionMemberDictionary As Dictionary(Of String, String),
			ByVal intersectionDictionary As Dictionary(Of String, String)
		) As String
			'Build member filter based on the intersections defined in the parameter
			Dim memberFilter As String = String.Empty
			For Each defaultDimensionMember In defaultDimensionMemberDictionary
				Dim dimensionShorted As String = defaultDimensionMember.Key
				Dim memberSelected As String = If(
					intersectionDictionary.ContainsKey(dimensionShorted),
					intersectionDictionary(dimensionShorted),
					defaultDimensionMember.Value
				)
				memberFilter = $"{memberFilter}:{dimensionShorted}#{memberSelected}"
			Next
			
			Return memberFilter
		End Function
		
		#End Region
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
