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
					
					If (storedDashboard.Name.Contains("LPW_Cards_") OrElse storedDashboard.Name.Contains("LPP_Cards_") OrElse storedDashboard.Name.Contains("LPT_Cards_")) AndAlso customSubstVarsAlreadyResolved.Count > 0 Then

						Return Me.GetCardRepeaterDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)
												
					#End Region
					
					#Region "Double Cards"
					
					Else If (storedDashboard.Name.Contains("LPW_DoubleCards_") OrElse storedDashboard.Name.Contains("LPP_DoubleCards_") OrElse storedDashboard.Name.Contains("LPT_DoubleCards_"))  AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						
						Return Me.GetDoubleCardRepeaterDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)
						
					#End Region
					
					#Region "Completion Cards"
					
					Else If (storedDashboard.Name.Contains("LPW_CompletionCards_") OrElse storedDashboard.Name.Contains("LPP_CompletionCards_") OrElse storedDashboard.Name.Contains("LPT_CompletionCards_")) AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						
						Return Me.GetCompletionCardRepeaterDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)
					
					#End Region
						
					#Region "Tornado Charts"
					
					Else If storedDashboard.Name.Contains("LPW_TornadoChart_") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then
						
						Return Me.GetTornadoChartDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)
						
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
					If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPW_Cards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPP_Cards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPT_Cards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPW_DoubleCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPP_DoubleCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPT_DoubleCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPW_CompletionCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPP_CompletionCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPT_CompletionCards") Then
						Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					Else If dynamicDashboardEx.DynamicDashboard.Name.Contains("LPW_TornadoChart") Then
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
		    Dim KPIAccountParameter As String = $"LandingPage.LPW_param_KPIs_Account_{dashboardSuffix}"
		
		    'Set default dimension member dictionary
		    Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
		        {"P", Me.GetParentFromEntity(si, customSubstVarsAlreadyResolved("LPW_param_POV_Entity"))},
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
		    If Not String.IsNullOrEmpty(customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level")) Then
		        Dim aggregationLevelMemberList As String() = customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level").Split(":")
		        For Each aggregationLevelMember In aggregationLevelMemberList
		            Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
		            defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
		        Next
		    End If
		
		    'Get intersection list to get the data
		    Dim intersectionList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, KPIAccountParameter).Split(",")
		
		    'Get scenario to compare, time member and view
		    Dim scenarioBase As String = customSubstVarsAlreadyResolved("LPW_param_POV_BaseScenario")
		    Dim scenarioToCompare As String = customSubstVarsAlreadyResolved("LPW_param_POV_ReferenceScenario")
		    Dim timeMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_Time")
			Dim timeMemberRef As String = customSubstVarsAlreadyResolved("LPW_param_POV_Time_Ref")
		    Dim viewMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_View")
		    Dim scenarioToCompareDescription As String = If(
		        String.IsNullOrEmpty(scenarioToCompare),
		        "",
		        BRApi.Finance.Metadata.GetMember(si, dimTypeId.Scenario, scenarioToCompare).Description
		    )
		    
		    '--- Main repeater loop ---
		    For Each intersection In intersectionList
		        
		        'Get intersection dictionary based on intersection
		        Dim intersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, intersection)
		        
		        'Get account Description
		        Dim accountMember As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A"))
		        Dim accountDescription As String = If(
		            Not accountMember Is Nothing,
		            BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A")).Description,
		            "Account Not Found"
		        )
		        
		        'Determine cube to use
		        Dim cubeNameToUse As String = "Horse"
				Dim isVolumes As Boolean = accountDescription.ToLower().Contains("volumes")
				
				If isVolumes Then
				    cubeNameToUse = "Analytics"
				End If
		        
		        ' 1. Crear una copia del diccionario base para el escenario ACTUAL
		        Dim actualIntersectionDictionary As New Dictionary(Of String, String)(intersectionDictionary)
		        
		        ' 2. Crear una copia del diccionario base para el escenario de REFERENCIA
		        Dim referenceIntersectionDictionary As New Dictionary(Of String, String)(intersectionDictionary)
				
				' Si es "volumes", siempre establecer U7 = "Type" en ambas copias
				If isVolumes Then
					actualIntersectionDictionary("U1") = "Total_Customer"
					referenceIntersectionDictionary("U1") = "Total_Customer"
				    actualIntersectionDictionary("U7") = "Top"
				    referenceIntersectionDictionary("U7") = "Top"
				End If
				
				' Mantener la lógica anterior para U4: solo cuando el escenario correspondiente es "actual"
				If isVolumes AndAlso scenarioBase.ToLower() = "actual" Then
				    actualIntersectionDictionary("U4") = "Details"
					actualIntersectionDictionary("U1") = "Top"
				End If
				
				If isVolumes AndAlso scenarioToCompare.ToLower() = "actual" Then
				    referenceIntersectionDictionary("U4") = "Details"
					referenceIntersectionDictionary("U1") = "Top"
				End If
		
		        ' Ahora, genera los filtros a partir de los diccionarios modificados (ya no hay duplicación)
		        Dim actualMemberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, actualIntersectionDictionary)
		        Dim referenceMemberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, referenceIntersectionDictionary)		        
		            
		        'Get data from scenarios using the specific filters
		        Dim actualAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
		            si,
		            cubeNameToUse,
		            $"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:P#:S#{scenarioBase}:T#{timeMember}:V#{viewMember}{actualMemberFilter}"
		        ).DataCellEx.DataCell.CellAmount
'		        Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
'		            si,
'		            cubeNameToUse,
'		            $"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{referenceMemberFilter}"
'		        ).DataCellEx.DataCell.CellAmount
				
				Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
		            si,
		            cubeNameToUse,
		            $"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMemberRef}:V#{viewMember}{referenceMemberFilter}"
		        ).DataCellEx.DataCell.CellAmount
		
		        'Set style mode
		        Dim styleMode As String = ""
		        Dim iconSuffix As String = String.Empty
		        If styleMode.ToLower.Contains("dark") Then iconSuffix = "_White"
		
		        'Set format params and icon based on account description
		        Dim formatParam As String = "#,#0,.0"
		        Dim measureParam As String = "M€"
		        Dim isButton As Integer = 0
		        Dim isDialog As Integer = 0
		        Dim dashboardToOpen As String = String.Empty
		        Dim isGreenOnPositive As Integer = 1
		        Dim hasVariance As Integer = 1
		        Dim varianceType As String = "%"
		        Dim isTitle As Integer = 1
		        Dim iconParam As String = String.Empty
		        Dim commentParam As String = $"Vs {scenarioToCompareDescription}"
		        If accountDescription.Contains("%") Then
		            formatParam = "0.0\%"
		            varianceType = "pt"
		        Else If accountDescription.ToLower.Contains("revenue")
		            iconParam = $"dollar_sign{iconSuffix}.png"
		        Else If accountDescription.ToLower.Contains("cash flow")
		            iconParam = $"hand_coins{iconSuffix}.png"
		            isTitle = 0
		            isButton = 1
		            isDialog = 1
		            dashboardToOpen = "LPW_KPI_Dialog_Operational_FreeCashflow"
		        Else If accountDescription.ToLower.Contains("cash position")
		            iconParam = $"landmark{iconSuffix}.png"
		        Else If accountDescription.ToLower.Contains("margin")
		            iconParam = $"chart_pie{iconSuffix}.png"
		        Else If accountDescription.ToLower.Contains("volume")
		            formatParam = "#,#0,"
		            measureParam = "K Un"
		            iconParam = $"car{iconSuffix}.png"
		        Else If accountDescription.ToLower.Contains("sg&a")
		            isTitle = 0
		            iconParam = $"laptop_minimal_check{iconSuffix}.png"
		            isButton = 1
		            isDialog = 1
		            dashboardToOpen = "LPW_KPI_SGA_Dialog"
		        Else If accountDescription.ToLower.Contains("inventory")
		            isGreenOnPositive = 0
		            iconParam = $"package{iconSuffix}.png"
		        Else If accountDescription.ToLower.Contains("overdues") OrElse _
		            accountDescription.ToLower.Contains("debt")
		            hasVariance = 0
		            iconParam = $"banknote{iconSuffix}.png"
					isButton = 1
		            isDialog = 1
					If customSubstVarsAlreadyResolved("LPW_param_POV_Entity") = "Horse_Group" Then
				        dashboardToOpen = "LPW_KPI_Overdues_Dialog_Horse_Group"
				    Else
				        dashboardToOpen = "LPW_KPI_Overdues_Dialog_others"
				    End If
		        End If
		
		        If Not customSubstVarsAlreadyResolved.ContainsKey("LPW_test") Then
		            customSubstVarsAlreadyResolved.Add("LPW_test", "")
		        End If
		
		        'Populate template params and add collection component
		        Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
		            {"title", accountDescription},
		            {"measure", measureParam},
		            {"first_data", Decimal.Round(actualAmount, 2)},
		            {"second_data", Decimal.Round(referenceAmount, 2)},
		            {"has_variance", hasVariance},
		            {"variance_type", varianceType},
		            {"comment", commentParam},
		            {"is_green_on_positive", isGreenOnPositive},
		            {"is_title", isTitle},
		            {"format", formatParam},
		            {"is_button", isButton},
		            {"is_dialog", isDialog},
		            {"dashboard_to_open", dashboardToOpen},
		            {"icon", iconParam},
		            {"bg_color", ""},
		            {"no_variance_bg_color", "1"},
		            {"title_color", ""},
		            {"primary_color", ""},
		            {"secondary_color", ""},
		            {"style_mode", styleMode}
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
			Dim KPIAccountParameter As String = $"LandingPage.LPW_param_KPIs_Account_{dashboardSuffix}"
			'Set default dimension member dictionary
			Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
				{"P", Me.GetParentFromEntity(si, customSubstVarsAlreadyResolved("LPW_param_POV_Entity"))},
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
			If Not String.IsNullOrEmpty(customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level")) Then
				Dim aggregationLevelMemberList As String() = customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level").Split(":")
				For Each aggregationLevelMember In aggregationLevelMemberList
					Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
					defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
				Next
			End If
			
			'Get scenario to compare, time member and view
			Dim scenarioBase As String = customSubstVarsAlreadyResolved("LPW_param_POV_BaseScenario")
			Dim scenarioToCompare As String = customSubstVarsAlreadyResolved("LPW_param_POV_ReferenceScenario")
			Dim timeMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_Time")
			Dim timeMemberRef As String = customSubstVarsAlreadyResolved("LPW_param_POV_Time_Ref")
			Dim viewMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_View")
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
				
				'Set style mode
				Dim styleMode As String = ""
				Dim iconSuffix As String = String.Empty
				If styleMode.ToLower.Contains("dark") Then iconSuffix = "_White"
				
				'Declare template params and add title, comment and button to open a dashboard
				Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
					{"is_title", 0},
					{"has_variance", 1},
					{"icon", $"chart_no_axes_combined_white.png"},
					{"comment", commentParam},
					{"bg_color", "Black"},
					{"no_variance_bg_color", "1"},
					{"title_color", "White"},
					{"primary_color", "White"},
					{"secondary_color", "DarkGray"},
					{"hover_color", "XFDarkGray"},
					{"style_mode", styleMode}
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
					
					'Set parameters if it's first loop
					If i = 0 Then
						Dim isButton As Integer = 0
						Dim dashboardToOpen As String = String.Empty
						If accountDescription.ToLower.Contains("ebit")
							isButton = 0
							dashboardToOpen = "Horse.REPOR_000_MAIN"
						End If
						
						accountDescriptionMain = accountDescription
						nextLevelTemplateSubstVarsToAdd("title") = accountDescription
						nextLevelTemplateSubstVarsToAdd("is_button") = isButton
						nextLevelTemplateSubstVarsToAdd("dashboard_to_open") = dashboardToOpen
					End If
					
					'Set first or second string
					Dim cardNumberString As String = If(i = 0, "first", "second")
					'Get data from scenarios
					Dim actualAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:C#Local:S#{scenarioBase}:T#{timeMember}:V#{viewMember}{memberFilter}"
					).DataCellEx.DataCell.CellAmount
'					Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
'						si,
'						"Horse",
'						$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:C#Local:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{memberFilter}"
'					).DataCellEx.DataCell.CellAmount
					
					Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:C#Local:S#{scenarioToCompare}:T#{timeMemberRef}:V#{viewMember}{memberFilter}"
					).DataCellEx.DataCell.CellAmount
					
					'Set format param based on account description
					Dim formatParam As String = "#,#0,.0M€"
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
			Dim KPIAccountParameter As String = $"LandingPage.LPW_param_Completion_Account_{dashboardSuffix}"
			
			'Set default dimension member dictionary
			Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
				{"P", Me.GetParentFromEntity(si, customSubstVarsAlreadyResolved("LPW_param_POV_Entity"))},
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
			If Not String.IsNullOrEmpty(customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level")) Then
				Dim aggregationLevelMemberList As String() = customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level").Split(":")
				For Each aggregationLevelMember In aggregationLevelMemberList
					Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
					defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
				Next
			End If
			
			'Get intersection list to get the data
			Dim intersectionList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, KPIAccountParameter).Split(",")
			
			'Get scenario to compare, time member and view
			Dim scenarioBase As String = customSubstVarsAlreadyResolved("LPW_param_POV_BaseScenario")
			Dim scenarioToCompare As String = customSubstVarsAlreadyResolved("LPW_param_POV_ReferenceScenario")
			Dim timeMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_Time")
			Dim timeMemberRef As String = customSubstVarsAlreadyResolved("LPW_param_POV_Time_Ref")
			Dim viewMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_View")
			
			Dim scenarioBaseDescription As String = If(
				String.IsNullOrEmpty(scenarioBase),
				"",
				BRApi.Finance.Metadata.GetMember(si, dimTypeId.Scenario, scenarioBase).Description
			)
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
					$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioBase}:T#{timeMember}:V#{viewMember}{memberFilter}"
				).DataCellEx.DataCell.CellAmount
				
'				Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
'					si,
'					"Horse",
'					$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{memberFilter}"
'				).DataCellEx.DataCell.CellAmount
				
				Dim referenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
					si,
					"Horse",
					$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMemberRef}:V#{viewMember}{memberFilter}"
				).DataCellEx.DataCell.CellAmount
				
				
				
				'Set format params based on account description
				Dim formatParam As String = "#,#0,.0M€"
				Dim isGreenOnPositive As Integer = 1
				Dim isButton As Integer = 0
				Dim isDialog As Integer = 0
				Dim dashboardToOpen As String = String.Empty
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
					{"first_data_name", scenarioBaseDescription},
					{"first_data", Decimal.Round(actualAmount, 2)},
					{"second_data_name", scenarioToCompareDescription},
					{"second_data", Decimal.Round(referenceAmount, 2)},
					{"is_green_on_positive", isGreenOnPositive},
					{"is_title", 1},
					{"is_button", isButton},
					{"is_dialog", isDialog},
					{"dashboard_to_open", dashboardToOpen},
					{"format", formatParam},
					{"bg_color", ""},
					{"no_variance_bg_color", "1"},
					{"title_color", ""},
					{"primary_color", ""},
					{"secondary_color", ""},
					{"style_mode", ""}
				}
				
				'If it's expenses set up is_green parameter based on the difference also in revenues income
				If accountDescription.ToLower.Contains("expense") Then
					'Set first percentage and declare second percentage
					Dim expensePercentage As Decimal = If(referenceAmount = 0, 0, actualAmount / referenceAmount)
					
					'Get income intersection
					Dim incomeIntersection As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"LandingPage.LPW_param_Completion_Account_Income")
					'Get intersection dictionary based on intersection
					Dim incomeIntersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, incomeIntersection)
					'Get member filter based on intersection
					Dim incomeMemberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, incomeIntersectionDictionary)
					
					'Get actual and reference amount for revenues and calculate second percentage
					Dim incomeActualAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioBase}:T#{timeMember}:V#{viewMember}{incomeMemberFilter}"
					).DataCellEx.DataCell.CellAmount
					
'					Dim incomeReferenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
'						si,
'						"Horse",
'						$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMember}:V#{viewMember}{incomeMemberFilter}"
'					).DataCellEx.DataCell.CellAmount
					
					Dim incomeReferenceAmount As Decimal = BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:S#{scenarioToCompare}:T#{timeMemberRef}:V#{viewMember}{incomeMemberFilter}"
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
		
		#Region "Tornado Charts"
		
		#Region "Get Tornado Chart Dashboard"
		
		Private Function GetTornadoChartDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx
			'Prepare a list of repeated components
			Dim repeatArgs As New List(Of WsDynamicComponentRepeatArgs)
			
			'Get account parameter based on the dashboard
			Dim dashboardSuffix As String = storedDashboard.Name.Split("_")(storedDashboard.Name.Split("_").Count - 1)
			Dim accountParameter As String = $"LandingPage.LPW_param_TornadoChart_Account_{dashboardSuffix}"
			
			'Set format params based on dashboard suffix
			Dim formatParam As String = "#,#0,\M€"
			Dim isTitle As Integer = 0
			Dim barMaxWidth As Integer = 140
			If dashboardSuffix.Contains("%") Then
				formatParam = "0.0\%"
			End If
			'Populate template params
			Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
				{"title", dashboardSuffix},
				{"is_title", isTitle},
				{"format", formatParam},
				{"bar_max_width", barMaxWidth},
				{"bg_color", ""},
				{"title_color", ""},
				{"primary_color", ""},
				{"secondary_color", ""}
			}
			
			'Set default dimension member dictionary
			Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
				{"P", Me.GetParentFromEntity(si, customSubstVarsAlreadyResolved("LPW_param_POV_Entity"))},
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
			If Not String.IsNullOrEmpty(customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level")) Then
				Dim aggregationLevelMemberList As String() = customSubstVarsAlreadyResolved("LPW_param_POV_Aggregation_Level").Split(":")
				For Each aggregationLevelMember In aggregationLevelMemberList
					Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
					defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
				Next
			End If
			
			'Get intersection list to get the data
			Dim intersectionList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, accountParameter).Split(",")
			
			'Get time member and view
			Dim timeMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_Time")
			Dim viewMember As String = customSubstVarsAlreadyResolved("LPW_param_POV_View")
			
			'Populate lists of accounts, amounts and is green on positives
			Dim accountListString As String = String.Empty
			Dim amountListString As String = String.Empty
			Dim isGreenOnPositiveListString As String = String.Empty
			For i As Integer = 0 To intersectionList.Count - 1
				'Get intersection dictionary based on intersection
				Dim intersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, intersectionList(i))
				'Get member filter based on intersection
				Dim memberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, intersectionDictionary)
				
				'Set is green on positive
				Dim isGreenOnPositive As Integer = 1
				
				'Get account Description
				Dim accountMember As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A"))
				Dim accountDescription As String = If(
					Not accountMember Is Nothing,
					BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A")).Description,
					"Account Not Found"
				)
				
				'Get data from scenarios
				Dim amount As Decimal = Decimal.Round(
					BRApi.Finance.Data.GetDataCellUsingMemberScript(
						si,
						"Horse",
						$"E#{customSubstVarsAlreadyResolved("LPW_param_POV_Entity")}:P#:S#Actual:T#{timeMember}:V#{viewMember}{memberFilter}"
					).DataCellEx.DataCell.CellAmount,
					2
				)
				
				'Populate lists
				accountListString = If(String.IsNullOrEmpty(accountListString), accountDescription, $"{accountListString}|{accountDescription}")
				amountListString = If(String.IsNullOrEmpty(amountListString), amount, $"{amountListString}|{amount}")
				isGreenOnPositiveListString = If(String.IsNullOrEmpty(isGreenOnPositiveListString), isGreenOnPositive, $"{isGreenOnPositiveListString}|{isGreenOnPositive}")
			Next
			
			'Populate template parameters
			nextLevelTemplateSubstVarsToAdd("account_description_list") = accountListString
			nextLevelTemplateSubstVarsToAdd("amount_list") = amountListString
			nextLevelTemplateSubstVarsToAdd("is_green_on_positive_list") = isGreenOnPositiveListString
			
			'Add collection component
			repeatArgs.Add(New WsDynamicComponentRepeatArgs(dashboardSuffix, nextLevelTemplateSubstVarsToAdd))
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
		
	End Class
End Namespace
