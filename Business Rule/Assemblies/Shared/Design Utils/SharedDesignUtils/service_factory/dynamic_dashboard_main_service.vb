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
		
        Public Function GetEmbeddedDynamicDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx Implements IWsasDynamicDashboardsV800.GetEmbeddedDynamicDashboard
            Try
                If (api IsNot Nothing) Then
					 
					#Region "Card"
					
					If (storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_Card") OrElse _
						storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_Card_Phone") OrElse _
						storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_Card_Tablet") _
						) AndAlso customSubstVarsAlreadyResolved.Count > 0
						
						'Declare dictionary of template parameters
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String)
						
						'---------------------------------------------------------- STYLE --------------------------------------------------------
						'Convert title to title case
						If parentDynamicComponentEx.TemplateSubstVars("is_title") = 1 Then
							parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
								parentDynamicComponentEx.TemplateSubstVars("title"),
								vbProperCase
							)
						End If
						
						'Set card width and font size depending on device
						Dim cardWidth As String = "|!Shared.prm_Style_Card_Width_App!|"
						Dim cardHeight As String = "|!Shared.prm_Style_Card_Height_App!|"
						Dim titleFontSize As String = "|!Shared.prm_Style_Card_FontSize_Title_App!|"
						Dim firstDataFontSize As String = "|!Shared.prm_Style_Card_FontSize_FirstData_App!|"
						Dim secondDataFontSize As String = "|!Shared.prm_Style_Card_FontSize_SecondData_App!|"
						Dim commentFontSize As String = "|!Shared.prm_Style_Card_FontSize_Comment_App!|"
						If storedDashboard.Name.EndsWith("Phone") Then
							cardWidth = "|!Shared.prm_Style_Card_Width_Phone!|"
							cardHeight = "|!Shared.prm_Style_Card_Height_Phone!|"
							titleFontSize = "|!Shared.prm_Style_Card_FontSize_Title_Phone!|"
							firstDataFontSize = "|!Shared.prm_Style_Card_FontSize_FirstData_Phone!|"
							secondDataFontSize = "|!Shared.prm_Style_Card_FontSize_SecondData_Phone!|"
							commentFontSize = "|!Shared.prm_Style_Card_FontSize_Comment_Phone!|"
						Else If storedDashboard.Name.EndsWith("Tablet") Then
							cardWidth = "|!Shared.prm_Style_Card_Width_Tablet!|"
							cardHeight = "|!Shared.prm_Style_Card_Height_Tablet!|"
							titleFontSize = "|!Shared.prm_Style_Card_FontSize_Title_Tablet!|"
							firstDataFontSize = "|!Shared.prm_Style_Card_FontSize_FirstData_Tablet!|"
							secondDataFontSize = "|!Shared.prm_Style_Card_FontSize_SecondData_Tablet!|"
							commentFontSize = "|!Shared.prm_Style_Card_FontSize_Comment_Tablet!|"
						End If
						'For width and height control if they are controlled from outside
						parentDynamicComponentEx.TemplateSubstVars("card_width") = If(
							parentDynamicComponentEx.TemplateSubstVars.ContainsKey("card_width") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("card_width")),
							parentDynamicComponentEx.TemplateSubstVars("card_width"),
							cardWidth
						)
						parentDynamicComponentEx.TemplateSubstVars("card_height") = If(
							parentDynamicComponentEx.TemplateSubstVars.ContainsKey("card_height") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("card_height")),
							parentDynamicComponentEx.TemplateSubstVars("card_height"),
							cardHeight
						)
						parentDynamicComponentEx.TemplateSubstVars("title_font_size") = titleFontSize
						parentDynamicComponentEx.TemplateSubstVars("first_data_font_size") = firstDataFontSize
						parentDynamicComponentEx.TemplateSubstVars("second_data_font_size") = secondDataFontSize
						parentDynamicComponentEx.TemplateSubstVars("comment_font_size") = commentFontSize
						
						'Convert measure to title case
						parentDynamicComponentEx.TemplateSubstVars("measure") = StrConv(
							parentDynamicComponentEx.TemplateSubstVars("measure"),
							vbProperCase
						)
						
						'---------------------------------------------------------- COLORS --------------------------------------------------------
						'Set style mode
						Dim styleModeParam As String = String.Empty
						If parentDynamicComponentEx.TemplateSubstVars.ContainsKey("style_mode") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("style_mode")) Then
							styleModeParam = "_" & StrConv(
								parentDynamicComponentEx.TemplateSubstVars("style_mode").Trim(),
								vbProperCase
							)
						End If
						'Set bg color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("bg_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("bg_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("bg_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultBG{styleModeParam}!|"
						End If
						'Set hover color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("hover_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("hover_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("hover_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultHover{styleModeParam}!|"
						End If
						'Set title color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("title_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("title_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("title_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultTitle{styleModeParam}!|"
						End If
						'Set primary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("primary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("primary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("primary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultPrimary{styleModeParam}!|"
						End If
						'Set secondary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("secondary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("secondary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("secondary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultGray{styleModeParam}!|"
						End If
						
						'---------------------------------------------------------- DATA --------------------------------------------------------
						'Get data
						Dim firstData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("first_data"))
						Dim secondData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("second_data"))
						'Get if it has variance or not
						If parentDynamicComponentEx.TemplateSubstVars("has_variance") = "1" Then
							'Get If the color is green or red and change depending on variance type
							Dim isGreen As Boolean = firstData >= secondData
							'Set variance and variance format depending on variance type
							Dim variance As Decimal
							Dim varianceFormat As String = String.Empty
							If parentDynamicComponentEx.TemplateSubstVars("variance_type") = "%" Then
								variance = If(secondData = 0, 0, (firstData - secondData) / secondData * 100)
								varianceFormat = "{0:0.0\%}"
							Else If parentDynamicComponentEx.TemplateSubstVars("variance_type") = "completion" Then
								variance = If(secondData = 0, 100, firstData / secondData * 100)
								varianceFormat = "{0:0.0\%}"
								isGreen = Decimal.Abs(variance) <= 100
							Else
								variance = firstData - secondData
								varianceFormat = "{0:" &
									parentDynamicComponentEx.TemplateSubstVars("format").Split("\")(0) &
									"\" & parentDynamicComponentEx.TemplateSubstVars("variance_type") & "}"
							End If
							'Change if is not green on positive
							isGreen = If(parentDynamicComponentEx.TemplateSubstVars("is_green_on_positive") = 1, isGreen, Not isGreen)
							
							'Set color and variance percent parameters
							'First control dark style
							Dim varianceColorParam As String = "variance_color"
							Dim varianceLightColorParam As String = "variance_bg_color"
							If styleModeParam.Tolower.Contains("dark") Then
								varianceColorParam = "variance_bg_color"
								varianceLightColorParam = "variance_color"
							End If
							nextLevelTemplateSubstVarsToAdd.Add(varianceColorParam, If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|"))
							nextLevelTemplateSubstVarsToAdd.Add(varianceLightColorParam, If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|"))
							nextLevelTemplateSubstVarsToAdd.Add("variance", String.Format(varianceFormat, variance))
							'Control if bg color for variance is deactivated
							If parentDynamicComponentEx.TemplateSubstVars.ContainsKey("no_variance_bg_color") AndAlso _
								parentDynamicComponentEx.TemplateSubstVars("no_variance_bg_color") = 1 Then _
								nextLevelTemplateSubstVarsToAdd("variance_bg_color") = parentDynamicComponentEx.TemplateSubstVars("bg_color")
							
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
						
						'Set button for just first data dashboard
						If parentDynamicComponentEx.TemplateSubstVars("is_button") = "1" Then
							'Set if dialog
							parentDynamicComponentEx.TemplateSubstVars("first_data_dashboard") = If(
								parentDynamicComponentEx.TemplateSubstVars.ContainsKey("is_dialog") AndAlso _
								parentDynamicComponentEx.TemplateSubstVars("is_dialog") = "1",
								"Shared.SharedComponent_Card_03_Content_FirstData_Button_Dialog",
								"Shared.SharedComponent_Card_03_Content_FirstData_Button"
							)
						Else
							parentDynamicComponentEx.TemplateSubstVars("first_data_dashboard") = "Shared.SharedComponent_Card_03_Content_FirstData_Label"
						End If
						
						'---------------------------------------------------------- SET UP DASHBOARD --------------------------------------------------------
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
					
					#Region "Double Card"

					Else If storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_DoubleCard") AndAlso customSubstVarsAlreadyResolved.Count > 0
						'---------------------------------------------------------- STYLE --------------------------------------------------------
						'Convert title to title case
						If parentDynamicComponentEx.TemplateSubstVars("is_title") = 1 Then
							parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
								parentDynamicComponentEx.TemplateSubstVars("title"),
								vbProperCase
							)
						End If
						
						'Set card width and font size depending on device
						Dim cardWidth As String = "|!Shared.prm_Style_DoubleCard_Width_App!|"
						Dim cardHeight As String = "|!Shared.prm_Style_DoubleCard_Height_App!|"
						Dim titleFontSize As String = "|!Shared.prm_Style_DoubleCard_FontSize_Title_App!|"
						Dim firstDataFontSize As String = "|!Shared.prm_Style_Card_FontSize_FirstData_App!|"
						Dim secondDataFontSize As String = "|!Shared.prm_Style_Card_FontSize_SecondData_App!|"
						Dim commentFontSize As String = "|!Shared.prm_Style_Card_FontSize_Comment_App!|"
						If storedDashboard.Name.EndsWith("Phone") Then
							cardWidth = "|!Shared.prm_Style_DoubleCard_Width_Phone!|"
							cardHeight = "|!Shared.prm_Style_DoubleCard_Height_Phone!|"
							titleFontSize = "|!Shared.prm_Style_DoubleCard_FontSize_Title_Phone!|"
							firstDataFontSize = "|!Shared.prm_Style_Card_FontSize_FirstData_Phone!|"
							secondDataFontSize = "|!Shared.prm_Style_Card_FontSize_SecondData_Phone!|"
							commentFontSize = "|!Shared.prm_Style_Card_FontSize_Comment_Phone!|"
						Else If storedDashboard.Name.EndsWith("Tablet") Then
							cardWidth = "|!Shared.prm_Style_DoubleCard_Width_Tablet!|"
							cardHeight = "|!Shared.prm_Style_DoubleCard_Height_Tablet!|"
							titleFontSize = "|!Shared.prm_Style_DoubleCard_FontSize_Title_Tablet!|"
							firstDataFontSize = "|!Shared.prm_Style_Card_FontSize_FirstData_Tablet!|"
							secondDataFontSize = "|!Shared.prm_Style_Card_FontSize_SecondData_Tablet!|"
							commentFontSize = "|!Shared.prm_Style_Card_FontSize_Comment_Tablet!|"
						End If
						'For width and height control if they are controlled from outside
						parentDynamicComponentEx.TemplateSubstVars("card_width") = If(
							parentDynamicComponentEx.TemplateSubstVars.ContainsKey("card_width") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("card_width")),
							parentDynamicComponentEx.TemplateSubstVars("card_width"),
							cardWidth
						)
						parentDynamicComponentEx.TemplateSubstVars("card_height") = If(
							parentDynamicComponentEx.TemplateSubstVars.ContainsKey("card_height") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("card_height")),
							parentDynamicComponentEx.TemplateSubstVars("card_height"),
							cardHeight
						)
						parentDynamicComponentEx.TemplateSubstVars("title_font_size") = titleFontSize
						parentDynamicComponentEx.TemplateSubstVars("first_data_font_size") = firstDataFontSize
						parentDynamicComponentEx.TemplateSubstVars("second_data_font_size") = secondDataFontSize
						parentDynamicComponentEx.TemplateSubstVars("comment_font_size") = commentFontSize
						
						'---------------------------------------------------------- COLORS --------------------------------------------------------
						'Set style mode
						Dim styleModeParam As String = String.Empty
						If parentDynamicComponentEx.TemplateSubstVars.ContainsKey("style_mode") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("style_mode")) Then
							styleModeParam = "_" & StrConv(
								parentDynamicComponentEx.TemplateSubstVars("style_mode").Trim(),
								vbProperCase
							)
						End If
						'Set bg color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("bg_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("bg_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("bg_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultBG{styleModeParam}!|"
						End If
						'Set hover color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("hover_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("hover_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("hover_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultHover{styleModeParam}!|"
						End If
						'Set title color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("title_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("title_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("title_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultTitle{styleModeParam}!|"
						End If
						'Set primary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("primary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("primary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("primary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultPrimary{styleModeParam}!|"
						End If
						'Set secondary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("secondary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("secondary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("secondary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultGray{styleModeParam}!|"
						End If
						
						'---------------------------------------------------------- DATA --------------------------------------------------------
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
								'First control dark style
								Dim varianceColorParam As String = $"{cardNumber}_card_variance_color"
								Dim varianceLightColorParam As String = $"{cardNumber}_card_variance_bg_color"
								If styleModeParam.Tolower.Contains("dark") Then
									varianceColorParam = $"{cardNumber}_card_variance_bg_color"
									varianceLightColorParam = $"{cardNumber}_card_variance_color"
								End If
								nextLevelTemplateSubstVarsToAdd.Add(varianceColorParam, If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|"))
								nextLevelTemplateSubstVarsToAdd.Add(varianceLightColorParam, If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|"))
								nextLevelTemplateSubstVarsToAdd.Add($"{cardNumber}_card_variance", String.Format(varianceFormat, variance))
								'Control if bg color for variance is deactivated
								If parentDynamicComponentEx.TemplateSubstVars.ContainsKey("no_variance_bg_color") AndAlso _
									parentDynamicComponentEx.TemplateSubstVars("no_variance_bg_color") = 1 Then _
									nextLevelTemplateSubstVarsToAdd($"{cardNumber}_card_variance_bg_color") = parentDynamicComponentEx.TemplateSubstVars("bg_color")
								
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
							
							'Format data to show based on the format parameter
							parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_first_data") = String.Format(
								"{0:" & parentDynamicComponentEx.TemplateSubstVars($"{cardNumber}_card_format") & "}",
								firstData
							)
						Next
						
						'---------------------------------------------------------- FUNCTIONALITY --------------------------------------------------------
						'Set button for just first data dashboard
						If parentDynamicComponentEx.TemplateSubstVars("is_button") = "1" Then
							parentDynamicComponentEx.TemplateSubstVars("first_card_first_data_dashboard") = "Shared.SharedComponent_DoubleCard_03_Content_FirstCard_Content_FirstData_Button"
							parentDynamicComponentEx.TemplateSubstVars("second_card_first_data_dashboard") = "Shared.SharedComponent_DoubleCard_03_Content_SecondCard_Content_FirstData_Button"
						Else
							parentDynamicComponentEx.TemplateSubstVars("first_card_first_data_dashboard") = "Shared.SharedComponent_DoubleCard_03_Content_FirstCard_Content_FirstData_Label"
							parentDynamicComponentEx.TemplateSubstVars("second_card_first_data_dashboard") = "Shared.SharedComponent_DoubleCard_03_Content_SecondCard_Content_FirstData_Label"
						End If
				
						'---------------------------------------------------------- SET UP DASHBOARD --------------------------------------------------------
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
					
					#Region "Completion Card"

					Else If (storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_CompletionCard") OrElse _
						storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_CompletionCard_Phone") OrElse _
						storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_CompletionCard_Tablet") _
						) AndAlso customSubstVarsAlreadyResolved.Count > 0
						'---------------------------------------------------------- STYLE --------------------------------------------------------
						'Convert title to title case
						If parentDynamicComponentEx.TemplateSubstVars("is_title") = 1 Then
							parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
								parentDynamicComponentEx.TemplateSubstVars("title"),
								vbProperCase
							)
						End If
						'Set card width depending on device
						Dim cardWidth As String = "|!Shared.prm_Style_CompletionCard_Width_App!|"
						Dim cardHeight As String = "|!Shared.prm_Style_CompletionCard_Height_App!|"
						Dim titleFontSize As String = "|!Shared.prm_Style_CompletionCard_FontSize_Title_App!|"
						Dim completionFontSize As String = "|!Shared.prm_Style_CompletionCard_FontSize_Completion_App!|"
						Dim firstDataFontSize As String = "|!Shared.prm_Style_CompletionCard_FontSize_FirstData_App!|"
						Dim secondDataFontSize As String = "|!Shared.prm_Style_CompletionCard_FontSize_SecondData_App!|"
						If storedDashboard.Name.EndsWith("Phone") Then
							cardWidth = "|!Shared.prm_Style_CompletionCard_Width_Phone!|"
							cardHeight = "|!Shared.prm_Style_CompletionCard_Height_Phone!|"
							titleFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_Title_Phone!|"
							completionFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_Completion_Phone!|"
							firstDataFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_FirstData_Phone!|"
							secondDataFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_SecondData_Phone!|"
						Else If storedDashboard.Name.EndsWith("Tablet") Then
							cardWidth = "|!Shared.prm_Style_CompletionCard_Width_Tablet!|"
							cardHeight = "|!Shared.prm_Style_CompletionCard_Height_Tablet!|"
							titleFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_Title_Tablet!|"
							completionFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_Completion_Tablet!|"
							firstDataFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_FirstData_Tablet!|"
							secondDataFontSize = "|!Shared.prm_Style_CompletionCard_FontSize_SecondData_Tablet!|"
						End If
						'For width and height control if they are controlled from outside
						parentDynamicComponentEx.TemplateSubstVars("card_width") = If(
							parentDynamicComponentEx.TemplateSubstVars.ContainsKey("card_width") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("card_width")),
							parentDynamicComponentEx.TemplateSubstVars("card_width"),
							cardWidth
						)
						parentDynamicComponentEx.TemplateSubstVars("card_height") = If(
							parentDynamicComponentEx.TemplateSubstVars.ContainsKey("card_height") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("card_height")),
							parentDynamicComponentEx.TemplateSubstVars("card_height"),
							cardHeight
						)
						parentDynamicComponentEx.TemplateSubstVars("title_font_size") = titleFontSize
						parentDynamicComponentEx.TemplateSubstVars("completion_font_size") = completionFontSize
						parentDynamicComponentEx.TemplateSubstVars("first_data_font_size") = firstDataFontSize
						parentDynamicComponentEx.TemplateSubstVars("second_data_font_size") = secondDataFontSize
						
						'---------------------------------------------------------- COLORS --------------------------------------------------------
						'Set style mode
						Dim styleModeParam As String = String.Empty
						If parentDynamicComponentEx.TemplateSubstVars.ContainsKey("style_mode") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("style_mode")) Then
							styleModeParam = "_" & StrConv(
								parentDynamicComponentEx.TemplateSubstVars("style_mode").Trim(),
								vbProperCase
							)
						End If
						'Set bg color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("bg_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("bg_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("bg_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultBG{styleModeParam}!|"
						End If
						'Set hover color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("hover_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("hover_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("hover_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultHover{styleModeParam}!|"
						End If
						'Set title color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("title_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("title_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("title_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultTitle{styleModeParam}!|"
						End If
						'Set primary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("primary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("primary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("primary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultPrimary{styleModeParam}!|"
						End If
						'Set secondary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("secondary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("secondary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("secondary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultGray{styleModeParam}!|"
						End If
						
						'---------------------------------------------------------- DATA --------------------------------------------------------
						'Get data
						Dim firstData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("first_data"))
						Dim secondData As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("second_data"))
						Dim completionPercentage As Decimal = If(secondData = 0, 100, firstData / secondData * 100)
						
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
						'First control dark style
						Dim completionColorParam As String = "completion_percentage_color"
						Dim completionLightColorParam As String = "completion_percentage_bg_color"
						If styleModeParam.Tolower.Contains("dark") Then
							completionColorParam = "completion_percentage_bg_color"
							completionLightColorParam = "completion_percentage_color"
						End If
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
							{completionColorParam, If(isGreen, "|!Shared.prm_Style_Colors_Green!|", "|!Shared.prm_Style_Colors_Red!|")},
							{completionLightColorParam, If(isGreen, "|!Shared.prm_Style_Colors_LightGreen!|", "|!Shared.prm_Style_Colors_LightRed!|")},
							{"completion_percentage", String.Format(
								"{0:0.0\%}",
								completionPercentage
							)}
						}
						'Control if bg color for variance is deactivated
						If parentDynamicComponentEx.TemplateSubstVars.ContainsKey("no_variance_bg_color") AndAlso _
							parentDynamicComponentEx.TemplateSubstVars("no_variance_bg_color") = 1 Then _
							nextLevelTemplateSubstVarsToAdd("completion_percentage_bg_color") = parentDynamicComponentEx.TemplateSubstVars("bg_color")
									
						'Format data to show based on the format parameter
						parentDynamicComponentEx.TemplateSubstVars("first_data") = String.Format(
							"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
							firstData
						)
						parentDynamicComponentEx.TemplateSubstVars("second_data") = String.Format(
							"{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}",
							secondData
						)
						
						'---------------------------------------------------------- FUNCTIONALITY --------------------------------------------------------
						'Set button for just first data dashboard
						If parentDynamicComponentEx.TemplateSubstVars("is_button") = "1" Then
							'Set if dialog
							parentDynamicComponentEx.TemplateSubstVars("completion_dashboard") = If(
								parentDynamicComponentEx.TemplateSubstVars.ContainsKey("is_dialog") AndAlso _
								parentDynamicComponentEx.TemplateSubstVars("is_dialog") = "1",
								"Shared.SharedComponent_CompletionCard_02_Variance_Button_Dialog",
								"Shared.SharedComponent_CompletionCard_02_Variance_Button"
							)
						Else
							parentDynamicComponentEx.TemplateSubstVars("completion_dashboard") = "Shared.SharedComponent_CompletionCard_02_Variance"
						End If
						
						'---------------------------------------------------------- SET UP DASHBOARD --------------------------------------------------------
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
						
					#Region "Tornado Charts"
					
					Else If storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_TornadoChart") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						'---------------------------------------------------------- STYLE --------------------------------------------------------
						'Convert title to title case
						If parentDynamicComponentEx.TemplateSubstVars("is_title") = 1 Then
							parentDynamicComponentEx.TemplateSubstVars("title") = StrConv(
								parentDynamicComponentEx.TemplateSubstVars("title"),
								vbProperCase
							)
						End If
						
						'---------------------------------------------------------- COLORS --------------------------------------------------------
						'Set style mode
						Dim styleModeParam As String = String.Empty
						If parentDynamicComponentEx.TemplateSubstVars.ContainsKey("style_mode") AndAlso _
							Not String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("style_mode")) Then
							styleModeParam = "_" & StrConv(
								parentDynamicComponentEx.TemplateSubstVars("style_mode").Trim(),
								vbProperCase
							)
						End If
						'Set bg color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("bg_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("bg_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("bg_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultBG{styleModeParam}!|"
						End If
						'Set hover color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("hover_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("hover_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("hover_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultHover{styleModeParam}!|"
						End If
						'Set title color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("title_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("title_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("title_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultTitle{styleModeParam}!|"
						End If
						'Set primary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("primary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("primary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("primary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultPrimary{styleModeParam}!|"
						End If
						'Set secondary color
						If Not parentDynamicComponentEx.TemplateSubstVars.ContainsKey("secondary_color") OrElse _
							String.IsNullOrEmpty(parentDynamicComponentEx.TemplateSubstVars("secondary_color")) Then
							parentDynamicComponentEx.TemplateSubstVars("secondary_color") = $"|!Shared.prm_Style_Colors_SharedComponent_DefaultGray{styleModeParam}!|"
						End If
						
						'---------------------------------------------------------- SET UP DASHBOARD --------------------------------------------------------
						'Set up embedded dashboard
						Dim contentDashboard As WsDynamicDashboardEx = api.GetEmbeddedDynamicDashboard(si, _
							workspace, parentDynamicComponentEx, storedDashboard, _
							String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown
						)
						
						'Save the state and return the dashboard
						api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent, _
							contentDashboard, WsDynamicItemStateType.NotUsed
						)
						If Not contentDashboard.DynamicDashboard.Dashboard Is Nothing Then Return contentDashboard
					
					Else If storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_TornadoChart_Left") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						'Prepare a list of repeated components
						Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
						'Get account list and set description
						Dim accountDescriptionList As String() = parentDynamicComponentEx.TemplateSubstVars("account_description_list").Split("|")
						For Each account As String In accountDescriptionList
							Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
								{"account_description", account}
							}
							repeatArgs.Add(New WsDynamicComponentRepeatArgs(account, nextLevelTemplateSubstVarsToAdd))
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
					
					Else If storedDashboard.Name.XFEqualsIgnoreCase("SharedComponent_TornadoChart_Right") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						'Prepare a list of repeated components
						Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
						
						'Get account list and format
						Dim accountDescriptionList As String() = parentDynamicComponentEx.TemplateSubstVars("account_description_list").Split("|")
						Dim format As String = "{0:" & parentDynamicComponentEx.TemplateSubstVars("format") & "}"
						
						'Set a bar max width and declare a bar for the amount to compare
						Dim barMaxWidth As Decimal = CDec(parentDynamicComponentEx.TemplateSubstVars("bar_max_width"))
						Dim barMaxAmount As Decimal = 0
						
						'Get amount and is green on positive list
						Dim amountList As String() = parentDynamicComponentEx.TemplateSubstVars("amount_list").Split("|")
						Dim isGreenOnPositiveList As String() = parentDynamicComponentEx.TemplateSubstVars("is_green_on_positive_list").Split("|")
						
						'Declare a dictionary of acount to bar width and color
						Dim accountToBarDictionary As New Dictionary(Of String, Dictionary(Of String, String))
						For i As Integer = 0 To accountDescriptionList.Count - 1
							accountToBarDictionary(accountDescriptionList(i)) = New Dictionary(Of String, String) From {
								{"amount", amountList(i)},
								{"is_green_on_positive", isGreenOnPositiveList(i)}
							}
						Next
						
						'Set max amount
						barMaxAmount = CDec(accountToBarDictionary(accountDescriptionList(0))("amount"))
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
							Dim amount As String = String.Format(
								format,
								CDec(kvp.Value("amount"))
							)
							Dim percentageAmount As String = String.Format(
								"{0:0.0\%;(0.0\%)}",
								CDec(If(barMaxAmount = 0, 0, CDec(kvp.Value("amount")) / barMaxAmount * 100))
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
						brapi.ErrorLog.logmessage(si, "paso right")
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
					If dynamicDashboardEx.DynamicDashboard.Name.Contains("SharedComponent_TornadoChart_Left_dynamic") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						brapi.ErrorLog.LogMessage(si, "Left count", repeatArgsList.Count.ToString)
						brapi.ErrorLog.LogMessage(si, "LLego dynamic left")
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("SharedComponent_TornadoChart_Right_dynamic") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						brapi.ErrorLog.LogMessage(si, "Right count", repeatArgsList.Count.ToString)
						brapi.ErrorLog.LogMessage(si, "LLego dynamic right")
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
		
	End Class
End Namespace
