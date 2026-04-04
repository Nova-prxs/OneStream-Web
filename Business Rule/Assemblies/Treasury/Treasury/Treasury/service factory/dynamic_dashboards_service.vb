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
	Public Class dynamic_dashboards_service
		Implements IWsasDynamicDashboardsV800

		''' <summary>
		''' Nombre del grupo de administración con permisos especiales
		''' </summary>
		Private Shared ReadOnly AdminSecurityGroup As String = "F_TRS_Super"
		Private Shared ReadOnly UserSecurityGroup As String = "F_TRS_User"

        Public Function GetEmbeddedDynamicDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx Implements IWsasDynamicDashboardsV800.GetEmbeddedDynamicDashboard
            Try
                If (api IsNot Nothing) Then
					 
					#Region "Cards"
					
					If storedDashboard?.Name.Contains("Treasury_Cards_") AndAlso customSubstVarsAlreadyResolved.Count > 0 Then

						Return Me.GetCardRepeaterDashboard(
							si, api, workspace, maintUnit, parentDynamicComponentEx, storedDashboard, customSubstVarsAlreadyResolved
						)
						
					End If
					
					#End Region
						
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
                    
                    ' SEGURIDAD ACTIVADA
                    Dim userGroupsList As List(Of String) = GetUserGroups(si)
                    Dim dashboardName As String = dynamicDashboardEx.DynamicDashboard.Name
					Dim repeatArgsList As List(Of WsDynamicComponentRepeatArgs) = dynamicDashboardEx.DynamicDashboard.Tag
					
                    Select Case True
                        Case dashboardName.XFEqualsIgnoreCase("EDIT_Treasury_MainNavigation_Main_Left_Tabs") OrElse dashboardName.Contains("_LeftTabs_Tabs")
                            Return FilterComponentsByTemplateSubstVars(si, api, workspace, dynamicDashboardEx, userGroupsList)
						
						Case dynamicDashboardEx.DynamicDashboard.Name.Contains("Treasury_Cards")
						Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
                        
                        Case Else
                            ' Para dashboards sin filtrado especial
                            Return api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
                    End Select
                End If

                Return Nothing
            Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
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
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
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
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
        End Function

        Public Function GetDynamicParametersForDynamicComponent(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal dynamicComponentEx As WsDynamicComponentEx, ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) _
            As WsDynamicParameterCollection Implements IWsasDynamicDashboardsV800.GetDynamicParametersForDynamicComponent
            Try
                If (api IsNot Nothing) Then
                    Return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
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
		        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		    End Try
		End Function

		#Region "Filtrado Usuarios"

		''' <summary>
		''' Obtiene los grupos de seguridad del usuario actual
		''' Incluye validaciones defensivas para evitar excepciones nulas
		''' </summary>
		Private Function GetUserGroups(si As SessionInfo) As List(Of String)
			Dim userGroupsList As New List(Of String)
			Try
				If si IsNot Nothing AndAlso Not String.IsNullOrEmpty(si.UserName) Then
					Dim user = BRApi.Security.Admin.GetUser(si, si.UserName)
					If user IsNot Nothing AndAlso user.AncestorGroups IsNot Nothing Then
						For Each kvp As KeyValuePair(Of Guid, Group) In user.AncestorGroups
							If kvp.Value IsNot Nothing Then
								userGroupsList.Add(kvp.Value.Name)
							End If
						Next
					End If
				End If
			Catch ex As Exception
				ErrorHandler.LogWrite(si, ex)
			End Try
			Return userGroupsList
		End Function

		''' <summary>
		''' Filtra componentes por variables de sustitución de template
		''' Verifica si el usuario pertenece al grupo de seguridad requerido (securitygroup)
		''' </summary>
		Private Function FilterComponentsByTemplateSubstVars(si As SessionInfo, api As IWsasDynamicDashboardsApiV800, _
																workspace As DashboardWorkspace, dynamicDashboardEx As WsDynamicDashboardEx, _
																userGroupsList As List(Of String)) As WsDynamicComponentCollection
			Try
				Dim comps As WsDynamicComponentCollection = api.GetDynamicComponentsForDynamicDashboard(si, workspace, _
																				dynamicDashboardEx, String.Empty, Nothing, TriStateBool.Unknown, _
																				WsDynamicItemStateType.Unknown)
				
				Dim repeatArgsList As New List(Of WsDynamicComponentRepeatArgs)
				
				' Verificar si el usuario es Super o User (tienen acceso total a tabs)
				Dim hasFullAccess As Boolean = userGroupsList.Any(Function(g) String.Equals(g, AdminSecurityGroup, StringComparison.OrdinalIgnoreCase) OrElse String.Equals(g, UserSecurityGroup, StringComparison.OrdinalIgnoreCase))
				
				For Each component As WsDynamicDbrdCompMemberEx In comps.Components
					Dim wsComponent As WsDynamicComponentEx = component.DynamicComponentEx
					Dim templateVars = wsComponent.TemplateSubstVars
					Dim shouldInclude As Boolean = True
					
					' Procesar variable "securitygroup" si existe
					If templateVars.ContainsKey("securitygroup") Then
						If hasFullAccess Then
							shouldInclude = True
						Else
							Dim requiredGroup As String = templateVars("securitygroup")
							shouldInclude = userGroupsList.Any(Function(g) String.Equals(g, requiredGroup, StringComparison.OrdinalIgnoreCase))
						End If
					End If
					
					' Agregar componente si cumple con seguridad
					If shouldInclude Then
						repeatArgsList.Add(New WsDynamicComponentRepeatArgs(wsComponent.AncestorNameSuffix, templateVars))
					End If
				Next
				
				Return api.GetDynamicComponentsRepeatedForDynamicDashboard(si, workspace, dynamicDashboardEx, repeatArgsList, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
			Catch ex As Exception
				ErrorHandler.LogWrite(si, ex)
				Return Nothing
			End Try
		End Function

		#End Region

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
		    Dim KPIAccountParameter As String = $"Treasury.prm_Treasury_KPIs_Account_{dashboardSuffix}"
		    Dim entityMember As String = String.Empty

		    If customSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_POV_Entity") Then
		        entityMember = customSubstVarsAlreadyResolved("prm_Treasury_POV_Entity")
		    Else
		        entityMember = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_POV_Entity")
		    End If
		    
		    'Get time from parameter or custom vars
		    Dim timeMember As String = String.Empty
		    If customSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_POV_Time") Then
		        timeMember = customSubstVarsAlreadyResolved("prm_Treasury_POV_Time")
		    Else
		        timeMember = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_POV_Time")
		    End If
		
		    'Set default dimension member dictionary
		    Dim defaultDimensionMemberDictionary As New Dictionary(Of String, String) From {
		        {"P", Me.GetParentFromEntity(si, entityMember)},
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
		    If Not String.IsNullOrEmpty(BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_POV_Aggregation_Level")) Then
		        Dim aggregationLevelMemberList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_POV_Aggregation_Level").Split(":")
		        For Each aggregationLevelMember In aggregationLevelMemberList
		            Dim aggregationLevelMemberSplitted As String() = aggregationLevelMember.Split("#")
		            defaultDimensionMemberDictionary(aggregationLevelMemberSplitted(0)) = aggregationLevelMemberSplitted(1)
		        Next
		    End If
		
		    'Get intersection list to get the data
		    Dim intersectionList As String() = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, KPIAccountParameter).Split(",")
		
		    'Get scenario to compare and view
		    Dim scenarioBase As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_POV_BaseScenario")
		    Dim scenarioToCompare As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False,"Treasury.prm_Treasury_POV_ReferenceScenario")
		    Dim viewMember As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_POV_View")
		    Dim scenarioToCompareDescription As String = If(
		        String.IsNullOrEmpty(scenarioToCompare),
		        "",
		        BRApi.Finance.Metadata.GetMember(si, dimTypeId.Scenario, scenarioToCompare).Description
		    )
		    
		    ' Cargar KPIs por tipo de dashboard
		    Dim paramYear As String = If(
		        customSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_Year"),
		        customSubstVarsAlreadyResolved("prm_Treasury_Year"),
		        BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_Year")
		    )
		    
		    Dim paramWeek As String = If(
		        customSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_WeekNumber"),
		        customSubstVarsAlreadyResolved("prm_Treasury_WeekNumber"),
		        BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_WeekNumber")
		    )
		    
		    Dim paramEntity As String = If(
		        customSubstVarsAlreadyResolved.ContainsKey("prm_Treasury_CompanyNames"),
		        customSubstVarsAlreadyResolved("prm_Treasury_CompanyNames"),
		        BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "Treasury.prm_Treasury_CompanyNames")
		    )
		    
		    ' Obtener datos de KPIs
		    Dim allKPIData As Dictionary(Of String, Decimal) = Nothing
		    
		    If dashboardSuffix.XFEqualsIgnoreCase("CashDebtStartWeek") Then
		        allKPIData = Me.GetAllCashDebtStartWeekKPIs(si, paramYear, paramWeek, paramEntity)
		    Else If dashboardSuffix.XFEqualsIgnoreCase("CashDebtEOM") Then
		        allKPIData = Me.GetAllCashDebtEOMKPIs(si, paramYear, paramWeek, paramEntity)
		    Else If dashboardSuffix.XFEqualsIgnoreCase("CashFlow") Then
		        allKPIData = Me.GetAllCashFlowKPIs(si, paramYear, paramWeek, paramEntity)
		    End If
		    
		    ' Iterar sobre las intersecciones para generar los componentes
		    For Each intersection In intersectionList
	        
	        ' Declare unique variables for each dashboard type to prevent any sharing
	        Dim actualAmount As Decimal = 0
	        Dim referenceAmount As Decimal = 0
	        Dim accountDescription As String = ""
	        
	        ' Check dashboard type
	        If dashboardSuffix.XFEqualsIgnoreCase("CashDebtStartWeek") Then
	            
	            ' Obtener datos del diccionario
	            Dim kpiType As String = intersection.Trim().ToLower()
	            
	            ' Map KPI type to dictionary keys and get values
	            Select Case kpiType
	                Case "cash"
	                    actualAmount = allKPIData("cash_current")
	                    referenceAmount = allKPIData("cash_previous")
	                    accountDescription = "Cash Available"
	                Case "usedlines"
	                    actualAmount = allKPIData("usedlines_current")
	                    referenceAmount = allKPIData("usedlines_previous")
	                    accountDescription = "Utilized Debt"
	                Case "availablelines"
	                    actualAmount = allKPIData("availablelines_current")
	                    referenceAmount = allKPIData("availablelines_previous")
	                    accountDescription = "Available Financing"
	                Case "netposition"
	                    ' NetPosition = Cash - UsedLines
	                    actualAmount = allKPIData("cash_current") - allKPIData("usedlines_current")
	                    referenceAmount = allKPIData("cash_previous") - allKPIData("usedlines_previous")
	                    accountDescription = "Net Financial Position"
	                Case Else
	                    accountDescription = kpiType
	            End Select

	        Else If dashboardSuffix.XFEqualsIgnoreCase("CashDebtEOM") Then

	            ' Obtener datos del diccionario
	            Dim kpiType As String = intersection.Trim().ToLower()
	            
	            ' Map KPI type to dictionary keys and get values
	            Select Case kpiType
	                Case "cash"
	                    actualAmount = allKPIData("cash_current")
	                    referenceAmount = allKPIData("cash_previous")
	                    accountDescription = "Cash Available"
	                Case "usedlines"
	                    actualAmount = allKPIData("usedlines_current")
	                    referenceAmount = allKPIData("usedlines_previous")
	                    accountDescription = "Utilized Debt"
	                Case "availablelines"
	                    actualAmount = allKPIData("availablelines_current")
	                    referenceAmount = allKPIData("availablelines_previous")
	                    accountDescription = "Available Financing"
	                Case "netposition"
	                    ' NetPosition = Cash - UsedLines
	                    actualAmount = allKPIData("cash_current") - allKPIData("usedlines_current")
	                    referenceAmount = allKPIData("cash_previous") - allKPIData("usedlines_previous")
	                    accountDescription = "Net Financial Position"
	                Case Else
	                    accountDescription = kpiType
	            End Select
            
	        Else If dashboardSuffix.XFEqualsIgnoreCase("CashFlow") Then
	            
	            ' Obtener datos del diccionario
	            Dim kpiType As String = intersection.Trim().ToLower()
	            
	            ' Map KPI type to dictionary keys and get values
	            Select Case kpiType
	                Case "operatingflows"
	                    actualAmount = allKPIData("operatingflows_current")
	                    referenceAmount = allKPIData("operatingflows_previous")
	                    accountDescription = "Operating Flows"
	                Case "investmentsflows"
	                    actualAmount = allKPIData("investmentsflows_current")
	                    referenceAmount = allKPIData("investmentsflows_previous")
	                    accountDescription = "Investments Flows"
	                Case "netposition"
	                    actualAmount = allKPIData("netposition_current")
	                    referenceAmount = allKPIData("netposition_previous")
	                    accountDescription = "Net Position"
	                Case "totalcash"
	                    actualAmount = allKPIData("totalcash_current")
	                    referenceAmount = allKPIData("totalcash_previous")
	                    accountDescription = "Total Cash"
	                Case Else
	                    accountDescription = kpiType
	            End Select
	            
	        Else

	            ' Cube-based: Original logic
	            'Get intersection dictionary based on intersection
	            Dim intersectionDictionary As Dictionary(Of String, String) = Me.GetIntersectionDictionary(si, intersection)
	            
	            'Get account Description
	            Dim accountMember As MemberInfo = BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A"))
	            accountDescription = If(
	                Not accountMember Is Nothing,
	                BRApi.Finance.Metadata.GetMember(si, dimTypeId.Account, intersectionDictionary("A")).Description,
	                "Account Not Found"
	            )			
		            ' Generate member filter from intersection
		            Dim memberFilter As String = Me.GetMemberFilterBasedOnIntersections(si, defaultDimensionMemberDictionary, intersectionDictionary)
		            
		            'Get data from scenarios using the specific filters - Only actualAmount for BS cards
		            actualAmount = BRApi.Finance.Data.GetDataCellUsingMemberScript(
		                si,
		                "Horse",
		                $"E#{entityMember}:P#:S#{scenarioBase}:T#{timeMember}:V#{viewMember}{memberFilter}"
		            ).DataCellEx.DataCell.CellAmount
		            
		            ' BS cards don't need reference amount (no variance)
		            referenceAmount = 0
	        End If
	        
	        'Set style mode
		        Dim styleMode As String = ""
		        Dim iconSuffix As String = String.Empty
		        If styleMode.ToLower.Contains("dark") Then iconSuffix = "_White"
		
		        'Set format params and icon based on account description
		        ' Default for SQL-based cards (values already in millions)
		        Dim isSQLBased As Boolean = dashboardSuffix.XFEqualsIgnoreCase("CashDebtStartWeek") OrElse dashboardSuffix.XFEqualsIgnoreCase("CashDebtEOM") OrElse dashboardSuffix.XFEqualsIgnoreCase("CashFlow")
		        Dim formatParam As String = If(isSQLBased, "#,#0.0", "#,#0,.0")
		        Dim measureParam As String = "M€"
		        Dim isButton As Integer = 0
		        Dim isDialog As Integer = 0
		        Dim dashboardToOpen As String = String.Empty
		        Dim isGreenOnPositive As Integer = 1
		        ' CashDebtStartWeek, CashDebtEOM and CashFlow cards have variance, BS cards don't
		        Dim hasVariance As Integer = If(isSQLBased, 1, 0)
		        ' Always use percentage variance
		        Dim varianceType As String = "%"
		        Dim isTitle As Integer = 1
		        Dim iconParam As String = String.Empty
		        Dim commentParam As String = $""
		        
		        ' Configuración específica por tipo de KPI
		        ' CashDebtStartWeek KPIs: Cash, UsedLines, AvailableLines, NetPosition
		        If accountDescription.ToLower.Contains("cash available") OrElse accountDescription.ToLower.Contains("cash position") Then
		            iconParam = $"dollar_sign{iconSuffix}.png"
		            isGreenOnPositive = 1  ' Cash up is good
		            
		            ' Configurar navegación según el tipo de dashboard
		            If dashboardSuffix.XFEqualsIgnoreCase("CashDebtEOM") Then
		                isButton = 1
		                isDialog = 1
		                dashboardToOpen = "Treasury_4_1_3_EOM_Dialog"
		            Else
		                isButton = 1  ' Hacer la carta clickeable
		                isDialog = 1  ' Abrir en modal
		                dashboardToOpen = "Treasury_4_1_3_StartWeek_Dialog"  ' Dashboard a abrir
		            End If
		        Else If accountDescription.ToLower.Contains("utilized debt") Then
		            iconParam = $"banknote{iconSuffix}.png"
		            isGreenOnPositive = 0  ' Debt down is good
		        Else If accountDescription.ToLower.Contains("available financing") Then
		            iconParam = $"hand_coins{iconSuffix}.png"
		            isGreenOnPositive = 1  ' More financing available is good
		        Else If accountDescription.ToLower.Contains("net financial position") Then
		            iconParam = $"chart_pie{iconSuffix}.png"
		            isGreenOnPositive = 1  ' Net position up is good
		        ' CashFlow KPIs: OperatingFlows, InvestmentsFlows, OpeningCash, ClosingCash
		        Else If accountDescription.ToLower.Contains("operating flows") Then
		            iconParam = $"car{iconSuffix}.png"
		            isGreenOnPositive = 1  ' Positive operating flows is good
		        Else If accountDescription.ToLower.Contains("investments flows") Then
		            iconParam = $"laptop_minimal_check{iconSuffix}.png"
		            isGreenOnPositive = 0  ' Less investment outflow is good
		        Else If accountDescription.ToLower.Contains("net position") AndAlso Not accountDescription.ToLower.Contains("financial") Then
		            iconParam = $"chart_pie{iconSuffix}.png"
		            isGreenOnPositive = 1  ' Positive net position is good
		            isButton = 1  ' Hacer la carta clickeable
		            isDialog = 1  ' Abrir en modal
		            dashboardToOpen = "Treasury_4_1_3_CashFlowCurrentWeekProjection_dialog"  ' Dashboard a abrir
		        Else If accountDescription.ToLower.Contains("total cash") Then
		            iconParam = $"dollar_sign{iconSuffix}.png"
		            isGreenOnPositive = 1  ' More total cash is good
		        ' BS KPIs: Free CashFlow, CashPosition, Overdues
		        Else If accountDescription.ToLower.Contains("cash flow") Then
		            iconParam = $"hand_coins{iconSuffix}.png"
		            isTitle = 0
		        Else If accountDescription.ToLower.Contains("overdues") Then
		            iconParam = $"banknote{iconSuffix}.png"
		            isGreenOnPositive = 0  ' Overdues down is good
		        End If
		
		        If Not customSubstVarsAlreadyResolved.ContainsKey("LPW_test") Then
		            customSubstVarsAlreadyResolved.Add("LPW_test", "")
		        End If
		
		        'Populate template params and add collection component
		        Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String) From {
		            {"title", accountDescription},
		            {"measure", measureParam},
		            {"first_data", Decimal.Round(actualAmount, 2).ToString()},
		            {"second_data", Decimal.Round(referenceAmount, 2).ToString()},
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
        
		        ' Use unique key by combining description with dashboard type to prevent collisions
		        Dim uniqueKey As String = $"{accountDescription}_{dashboardSuffix}"
		        repeatArgs.Add(New WsDynamicComponentRepeatArgs(uniqueKey, nextLevelTemplateSubstVarsToAdd))
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

		#End Region

		#Region "Helper Functions"
		
		#Region "Batch KPI Data Functions"
		
		''' <summary>
		''' Obtiene TODOS los KPIs de CashDebt StartWeek en UNA SOLA query SQL
		''' Retorna diccionario con claves: cash_current, cash_previous, usedlines_current, usedlines_previous, availablelines_current, availablelines_previous
		''' </summary>
		Private Function GetAllCashDebtStartWeekKPIs(
			ByVal si As SessionInfo,
			ByVal year As String,
			ByVal currentWeek As String,
			ByVal entityId As String
		) As Dictionary(Of String, Decimal)
			Dim result As New Dictionary(Of String, Decimal) From {
				{"cash_current", 0}, {"cash_previous", 0},
				{"usedlines_current", 0}, {"usedlines_previous", 0},
				{"availablelines_current", 0}, {"availablelines_previous", 0}
			}
			
			Try
				' Validación de parámetros
				If String.IsNullOrEmpty(year) OrElse year.Contains("|!") Then Return result
				If String.IsNullOrEmpty(currentWeek) OrElse currentWeek.Contains("|!") Then Return result
				If String.IsNullOrEmpty(entityId) OrElse entityId.Contains("|!") Then Return result
				
				' Obtener nombre de entidad
				Dim entity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, entityId)
				
				' Calcular semana anterior
				Dim prevWeekInfo As Dictionary(Of String, String) = Me.GetPreviousWeek(si, year, currentWeek)
				Dim prevYear As String = prevWeekInfo("year")
				Dim prevWeek As String = prevWeekInfo("week")
				
				' Obtener timeKeys y scenarios
				Dim currentTimeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, year, currentWeek)
				Dim previousTimeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, prevYear, prevWeek)
				Dim currentScenario As String = "FW" + currentWeek.PadLeft(2, "0"c)
				Dim previousScenario As String = "FW" + prevWeek.PadLeft(2, "0"c)
				
				' Construir filtro de entidad
				Dim entityFilter As String = ""
				If entity <> "HTD" Then
					entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'" 
				End If
				
				' UNA SOLA QUERY para obtener todos los KPIs
				Dim sql As String = $"
					SELECT 
						Account,
						UploadTimekey,
						Scenario,
						SUM(Amount) AS Total
					FROM XFC_TRS_Master_CashDebtPosition
					WHERE ProjectionType = 'StartWeek'
					{entityFilter}
					AND (
						(UploadTimekey = '{currentTimeKey}' AND Scenario = '{currentScenario.Replace("'", "''")}')
						OR
						(UploadTimekey = '{previousTimeKey}' AND Scenario = '{previousScenario.Replace("'", "''")}')
					)
					AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
					GROUP BY Account, UploadTimekey, Scenario
				"
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					
					For Each row As DataRow In dt.Rows
						Dim account As String = row("Account").ToString()
						Dim timeKey As String = row("UploadTimekey").ToString()
						Dim amount As Decimal = If(IsDBNull(row("Total")), 0D, Convert.ToDecimal(row("Total")))
						
						Dim isCurrent As Boolean = (timeKey = currentTimeKey)
						Dim suffix As String = If(isCurrent, "_current", "_previous")
						
						Select Case account
							Case "CASH AND FINANCING BALANCE"
								result("cash" & suffix) = amount
							Case "FINANCING - USED LINES"
								result("usedlines" & suffix) = amount
							Case "FINANCING - AVAILABLE LINES"
								result("availablelines" & suffix) = amount
						End Select
					Next
				End Using
				
				Return result
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Obtiene TODOS los KPIs de CashDebt EOM en UNA SOLA query SQL
		''' </summary>
		Private Function GetAllCashDebtEOMKPIs(
			ByVal si As SessionInfo,
			ByVal year As String,
			ByVal currentWeek As String,
			ByVal entityId As String
		) As Dictionary(Of String, Decimal)
			Dim result As New Dictionary(Of String, Decimal) From {
				{"cash_current", 0}, {"cash_previous", 0},
				{"usedlines_current", 0}, {"usedlines_previous", 0},
				{"availablelines_current", 0}, {"availablelines_previous", 0}
			}
			
			Try
				' Validación de parámetros
				If String.IsNullOrEmpty(year) OrElse year.Contains("|!") Then Return result
				If String.IsNullOrEmpty(currentWeek) OrElse currentWeek.Contains("|!") Then Return result
				If String.IsNullOrEmpty(entityId) OrElse entityId.Contains("|!") Then Return result
				
				' Obtener nombre de entidad
				Dim entity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, entityId)
				
				' Calcular mes EOM para semana actual y anterior
				Dim weekNumber As Integer = CInt(currentWeek)
				Dim eomMonthCurrent As Integer = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMonthFromWeek(si, year, weekNumber)
				
				Dim previousWeek As Integer = weekNumber - 1
				Dim previousYear As String = year
				If previousWeek < 1 Then
					previousWeek = 52
					previousYear = (CInt(year) - 1).ToString()
				End If
				Dim eomMonthPrevious As Integer = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMonthFromWeek(si, previousYear, previousWeek)
				
				' Obtener timeKeys y scenarios
				Dim currentTimeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, year, currentWeek)
				Dim previousTimeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, previousYear, previousWeek.ToString())
				Dim currentScenario As String = "FW" + currentWeek.PadLeft(2, "0"c)
				Dim previousScenario As String = "FW" + previousWeek.ToString().PadLeft(2, "0"c)
				
				' Construir filtro de entidad
				Dim entityFilter As String = ""
				If entity <> "HTD" Then
					entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'" 
				End If
				
				' UNA SOLA QUERY para obtener todos los KPIs EOM
				Dim sql As String = $"
					SELECT 
						Account,
						UploadTimekey,
						SUM(Amount) AS Total
					FROM XFC_TRS_Master_CashDebtPosition
					WHERE ProjectionType = 'EOM'
					{entityFilter}
					AND (
						(UploadTimekey = '{currentTimeKey}' AND Scenario = '{currentScenario.Replace("'", "''")}' AND ProjectionMonthNumber = {eomMonthCurrent})
						OR
						(UploadTimekey = '{previousTimeKey}' AND Scenario = '{previousScenario.Replace("'", "''")}' AND ProjectionMonthNumber = {eomMonthPrevious})
					)
					AND Account IN ('CASH AND FINANCING BALANCE', 'FINANCING - USED LINES', 'FINANCING - AVAILABLE LINES')
					GROUP BY Account, UploadTimekey
				"
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					
					For Each row As DataRow In dt.Rows
						Dim account As String = row("Account").ToString()
						Dim timeKey As String = row("UploadTimekey").ToString()
						Dim amount As Decimal = If(IsDBNull(row("Total")), 0D, Convert.ToDecimal(row("Total")))
						
						Dim isCurrent As Boolean = (timeKey = currentTimeKey)
						Dim suffix As String = If(isCurrent, "_current", "_previous")
						
						Select Case account
							Case "CASH AND FINANCING BALANCE"
								result("cash" & suffix) = amount
							Case "FINANCING - USED LINES"
								result("usedlines" & suffix) = amount
							Case "FINANCING - AVAILABLE LINES"
								result("availablelines" & suffix) = amount
						End Select
					Next
				End Using
				
				Return result
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		''' <summary>
		''' Obtiene TODOS los KPIs de CashFlow en UNA SOLA query SQL
		''' Retorna: operatingflows_current/previous, investmentsflows_current/previous, netposition_current/previous, totalcash_current/previous
		''' </summary>
		Private Function GetAllCashFlowKPIs(
			ByVal si As SessionInfo,
			ByVal year As String,
			ByVal currentWeek As String,
			ByVal entityId As String
		) As Dictionary(Of String, Decimal)
			Dim result As New Dictionary(Of String, Decimal) From {
				{"operatingflows_current", 0}, {"operatingflows_previous", 0},
				{"investmentsflows_current", 0}, {"investmentsflows_previous", 0},
				{"netposition_current", 0}, {"netposition_previous", 0},
				{"totalcash_current", 0}, {"totalcash_previous", 0}
			}
			
			Try
				' Validación de parámetros
				If String.IsNullOrEmpty(year) OrElse year.Contains("|!") Then Return result
				If String.IsNullOrEmpty(currentWeek) OrElse currentWeek.Contains("|!") Then Return result
				If String.IsNullOrEmpty(entityId) OrElse entityId.Contains("|!") Then Return result
				
				' Obtener nombre de entidad
				Dim entity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, entityId)
				
				' Calcular semana anterior
				Dim prevWeekInfo As Dictionary(Of String, String) = Me.GetPreviousWeek(si, year, currentWeek)
				Dim actualYear As String = prevWeekInfo("year")
				Dim actualWeek As String = prevWeekInfo("week")
				
				' Obtener timeKey y scenario
				Dim timeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, year, currentWeek)
				Dim scenario As String = "FW" + currentWeek.PadLeft(2, "0"c)
				
				' Construir filtro de entidad
				Dim entityFilter As String = ""
				If entity <> "HTD" Then
					entityFilter = $"AND Entity = '{entity.Replace("'", "''")}'" 
				End If
				
				' UNA SOLA QUERY para obtener todos los flujos agrupados por Account, ProjectionType, Flow
				Dim sql As String = $"
					SELECT 
						UPPER(Account) AS Account,
						ProjectionType,
						ProjectionYear,
						ProjectionWeekNumber,
						UPPER(Flow) AS Flow,
						SUM(Amount) AS Total
					FROM XFC_TRS_Master_CashflowForecasting
					WHERE UploadTimekey = '{timeKey}'
					{entityFilter}
					AND UPPER(Scenario) = '{scenario.Replace("'", "''")}'
					AND UPPER(Flow) IN ('INFLOWS', 'OUTFLOWS')
					AND (
						(ProjectionType = 'Projection' AND ProjectionYear = {year} AND ProjectionWeekNumber = {currentWeek})
						OR
						(ProjectionType = 'Actual' AND ProjectionYear = {actualYear} AND ProjectionWeekNumber = {actualWeek})
					)
					GROUP BY Account, ProjectionType, ProjectionYear, ProjectionWeekNumber, Flow
				"
				
				' Acumuladores temporales para calcular netposition y totalcash
				Dim opInvInflowsCurrent As Decimal = 0
				Dim opInvOutflowsCurrent As Decimal = 0
				Dim opInvInflowsPrevious As Decimal = 0
				Dim opInvOutflowsPrevious As Decimal = 0
				Dim allInflowsCurrent As Decimal = 0
				Dim allOutflowsCurrent As Decimal = 0
				Dim allInflowsPrevious As Decimal = 0
				Dim allOutflowsPrevious As Decimal = 0
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
					
					For Each row As DataRow In dt.Rows
						Dim account As String = row("Account").ToString()
						Dim projType As String = row("ProjectionType").ToString()
						Dim flow As String = row("Flow").ToString()
						Dim amount As Decimal = If(IsDBNull(row("Total")), 0D, Convert.ToDecimal(row("Total")))
						
						Dim isCurrent As Boolean = (projType = "Projection")
						Dim isOperatingOrInvestment As Boolean = (account = "OPERATING FLOWS" OrElse account = "INVESTMENT FLOWS")
						
						' Acumular para Operating Flows específico
						If account = "OPERATING FLOWS" Then
							If isCurrent Then
								result("operatingflows_current") += amount
							Else
								result("operatingflows_previous") += amount
							End If
						End If
						
						' Acumular para Investment Flows específico
						If account = "INVESTMENT FLOWS" Then
							If isCurrent Then
								result("investmentsflows_current") += amount
							Else
								result("investmentsflows_previous") += amount
							End If
						End If
						
						' Acumular para NetPosition (Operating + Investment)
						If isOperatingOrInvestment Then
							If isCurrent Then
								If flow = "INFLOWS" Then opInvInflowsCurrent += amount Else opInvOutflowsCurrent += amount
							Else
								If flow = "INFLOWS" Then opInvInflowsPrevious += amount Else opInvOutflowsPrevious += amount
							End If
						End If
						
						' Acumular para TotalCash (todas las cuentas)
						If isCurrent Then
							If flow = "INFLOWS" Then allInflowsCurrent += amount Else allOutflowsCurrent += amount
						Else
							If flow = "INFLOWS" Then allInflowsPrevious += amount Else allOutflowsPrevious += amount
						End If
					Next
				End Using
				
				' Calcular NetPosition y TotalCash
				result("netposition_current") = opInvInflowsCurrent + opInvOutflowsCurrent
				result("netposition_previous") = opInvInflowsPrevious + opInvOutflowsPrevious
				result("totalcash_current") = allInflowsCurrent + allOutflowsCurrent
				result("totalcash_previous") = allInflowsPrevious + allOutflowsPrevious
				
				Return result
				
			Catch ex As Exception
				Throw New XFException(si, ex)
			End Try
		End Function
		
		#End Region
		
		#Region "Helper Functions"
		
		''' <summary>
		''' Calcula la semana anterior manejando el cambio de año
		''' </summary>
		Private Function GetPreviousWeek(
			ByVal si As SessionInfo,
			ByVal year As String,
			ByVal currentWeek As String
		) As Dictionary(Of String, String)
			Try
				Dim result As New Dictionary(Of String, String)
				Dim weekNum As Integer = CInt(currentWeek)
				Dim yearNum As Integer = CInt(year)
				
				If weekNum > 1 Then
					' Simple case: previous week in same year
					result("year") = year
					result("week") = (weekNum - 1).ToString()
				Else
					' Week 1: go to last week of previous year
					Dim prevYear As Integer = yearNum - 1
					result("year") = prevYear.ToString()
					
					' Determine if previous year has 52 or 53 weeks via SQL
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Dim sql As String = $"
							SELECT MAX(weekNumber) AS MaxWeek
							FROM XFC_TRS_AUX_Date
							WHERE year = {prevYear}
						"
						
						Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
						
						If dt.Rows.Count > 0 AndAlso Not IsDBNull(dt.Rows(0)("MaxWeek")) Then
							result("week") = dt.Rows(0)("MaxWeek").ToString()
						Else
							' Fallback: assume 52 weeks
							result("week") = "52"
						End If
					End Using
				End If
				
				Return result
				
			Catch ex As Exception
				' Fallback to week 52 of previous year
				Return New Dictionary(Of String, String) From {
					{"year", (CInt(year) - 1).ToString()},
					{"week", "52"}
				}
			End Try
		End Function
		
		#End Region
		
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