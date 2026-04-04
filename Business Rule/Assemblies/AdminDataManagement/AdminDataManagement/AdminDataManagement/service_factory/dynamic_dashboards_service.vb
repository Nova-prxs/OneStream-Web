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

        Public Function GetEmbeddedDynamicDashboard(ByVal si As SessionInfo, ByVal api As IWsasDynamicDashboardsApiV800, ByVal workspace As DashboardWorkspace, _
            ByVal maintUnit As DashboardMaintUnit, ByVal parentDynamicComponentEx As WsDynamicComponentEx, ByVal storedDashboard As Dashboard, _
            ByVal customSubstVarsAlreadyResolved As Dictionary(Of String, String)) As WsDynamicDashboardEx Implements IWsasDynamicDashboardsV800.GetEmbeddedDynamicDashboard
            Try
                If (api IsNot Nothing) Then
					
					#Region "Table Import"
					
					If storedDashboard.Name.XFEqualsIgnoreCase("00_AdminDataManagement_Component_TableImport") Then
						
						'Declare dictionary of template parameters
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String)
						
						'Get parameters
						Dim paramTableName As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_import_table_name")
						Dim paramTableFilters As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_table_filters")
						Dim paramCheckViews As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_check_views")
						Dim paramCheckDashboards As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_check_dashboards")
						Dim paramIsEditable As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_is_editable")
						Dim paramHasFile As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_has_file")
						Dim paramOnlyEdit As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_only_edit")
						Dim paramEndpoint As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_import_endpoint")
						
						'Set default params
						parentDynamicComponentEx.TemplateSubstVars("p_status") = "Check"
						parentDynamicComponentEx.TemplateSubstVars("p_user") = "No data"
						parentDynamicComponentEx.TemplateSubstVars("p_last_update_date") = "No data"
						parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = "Mappings required"
						parentDynamicComponentEx.TemplateSubstVars("p_table_pop_up") = "SeeTable"
						parentDynamicComponentEx.TemplateSubstVars("p_file_buttons_dashboard") = "FileButtons_ImportData"
						parentDynamicComponentEx.TemplateSubstVars("p_table_filters") = ""
						
						' Configure table filters if provided
						If Not String.IsNullOrEmpty(paramTableFilters) Then
							parentDynamicComponentEx.TemplateSubstVars("p_table_filters") = "WHERE " & paramTableFilters
						End If
						
						If String.IsNullOrEmpty(paramTableName) Then _
							Throw New Exception("Error loading table import component: no table name specified")
						
						If Not String.IsNullOrEmpty(paramEndpoint) Then _
							parentDynamicComponentEx.TemplateSubstVars("p_file_buttons_dashboard") = If(
								paramHasFile = "1",
								"FileButtons_ImportData_RESTAPI_Download",
								"FileButtons_ImportData_RESTAPI"
							)
							
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							Dim dbParamInfos As New List(Of DbParamInfo) From {
								New DbParamInfo("paramTable", paramTableName)
							}
							Dim dt As New DataTable
						
							'Get table import information
							Try
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									"
									SELECT *
									FROM XFC_MAIN_MASTER_TableImportInfo WITH(NOLOCK)
									WHERE table_name = @paramTable
									",
									dbParamInfos,
									False
								)
							
								' Control if table has been uploaded
								If dt Is Nothing OrElse dt.Rows.Count = 0 Then
									parentDynamicComponentEx.TemplateSubstVars("p_status") = "Error"
									parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = "Import information for this table not found" & vbCrLf &
										"Please, import a file."
									GoTo AfterViewCheck
								Else
									parentDynamicComponentEx.TemplateSubstVars("p_user") = dt.Rows(0)("username").ToString
									parentDynamicComponentEx.TemplateSubstVars("p_last_update_date") = Format(dt.Rows(0)("last_import_date"), "yyyy/MM/dd HH:mm")
									parentDynamicComponentEx.TemplateSubstVars("p_last_imported_file_full_name") = dt.Rows(0)("folder_path").ToString & "/" &
									dt.Rows(0)("file_name").ToString
								End If
								
							Catch ex As Exception
								parentDynamicComponentEx.TemplateSubstVars("p_status") = "Error"
								Dim errorString As String = ex.ToString
								If errorString.Contains("Invalid object name") Then errorString = $"Information Table does not exist"
								parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = $"Error getting table info: {errorString}" & vbCrLf &
									"Please, talk with an administrator."
								GoTo AfterViewCheck
							End Try
							
							'Get view if provided
							If Not String.IsNullOrEmpty(paramCheckViews) Then
								Dim checkViewArray = paramCheckViews.Split(",")
								Dim checkDashboardArray = paramCheckDashboards.Split(",")
								
								'Check number of check views and check dashboards
								If checkViewArray.Count > checkDashboardArray.Count OrElse String.IsNullOrEmpty(paramCheckDashboards) Then
									parentDynamicComponentEx.TemplateSubstVars("p_status") = "Error"
									parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = $"There are more Check Views than Check Dashboards." & vbCrLf &
										"Please, talk with an administrator."
									GoTo AfterViewCheck
								End If
								Dim i As Integer = 0
								'Check if there are any transformations not found
								For Each checkView In checkViewArray
									Try
										dt = BRApi.Database.ExecuteSql(
											dbConn,
											$"
											SELECT *
											FROM {checkView.Trim()} WITH(NOLOCK)
											",
											dbParamInfos,
											False
										)
										If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
											Dim checkDashboard = checkDashboardArray(i).Trim()
											parentDynamicComponentEx.TemplateSubstVars("p_status") = "Warning"
											parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = $"Transformations not found." & vbCrLf &
												$"Click to go to check dashboard."
											parentDynamicComponentEx.TemplateSubstVars("p_check_dashboard") = checkDashboard
											Exit For
										End If
										
										i += 1
									Catch ex As Exception
										parentDynamicComponentEx.TemplateSubstVars("p_status") = "Error"
										Dim errorString As String = ex.ToString
										If errorString.Contains("Invalid object name") Then errorString = $"Check view '{checkView}' does not exist"
										parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = $"Error getting check view: {errorString}" & vbCrLf &
											"Please, talk with an administrator."
										Exit For
									End Try
								Next
							End If
							AfterViewCheck:
						End Using
						
						' If it is editable, set edit pop up button instead of see table
						If Not String.IsNullOrEmpty(paramIsEditable) AndAlso paramIsEditable = "1" Then
							parentDynamicComponentEx.TemplateSubstVars("p_table_pop_up") = "EditTable"
						End If
						
						' If it has no file, set file buttons dashboard to transparent
						If Not String.IsNullOrEmpty(paramOnlyEdit) AndAlso paramOnlyEdit = "1" Then
							parentDynamicComponentEx.TemplateSubstVars("p_file_buttons_dashboard") = "Transparent"
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
					
					#Region "Check"
					
					Else If storedDashboard.Name.XFEqualsIgnoreCase("00_AdminDataManagement_Component_Check") Then
						
						'Declare dictionary of template parameters
						Dim nextLevelTemplateSubstVarsToAdd As New Dictionary(Of String, String)
						
						'Get parameters
						Dim paramTableName As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_import_table_name")
						Dim paramCheckView As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_check_view")
						Dim paramTableFilters As String = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_table_filters")
						
						'Set default params
						parentDynamicComponentEx.TemplateSubstVars("p_status") = "Check"
						parentDynamicComponentEx.TemplateSubstVars("p_user") = "No data"
						parentDynamicComponentEx.TemplateSubstVars("p_last_update_date") = "No data"
						parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = "Mappings required"
						
						' Configure table filters if provided
						If Not String.IsNullOrEmpty(paramTableFilters) Then
							paramTableFilters = "WHERE " & paramTableFilters
						End If
						
						If String.IsNullOrEmpty(paramTableName) Then _
							Throw New Exception("Error loading table import component: no table name specified")
							
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							Dim dbParamInfos As New List(Of DbParamInfo) From {
								New DbParamInfo("paramTable", paramTableName)
							}
							Dim dt As New DataTable
						
							'Get table import information
							Try
								dt = BRApi.Database.ExecuteSql(
									dbConn,
									"
									SELECT *
									FROM XFC_MAIN_MASTER_TableImportInfo WITH(NOLOCK)
									WHERE table_name = @paramTable
									",
									dbParamInfos,
									False
								)
							
								' Control if table has been uploaded
								If dt Is Nothing OrElse dt.Rows.Count = 0 Then
									parentDynamicComponentEx.TemplateSubstVars("p_status") = "Error"
									parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = "Import information for this table not found" & vbCrLf &
										"Please, import a file."
									GoTo AfterViewCheckSecond
								End If
								
							Catch ex As Exception
								parentDynamicComponentEx.TemplateSubstVars("p_status") = "Error"
								Dim errorString As String = ex.ToString
								If errorString.Contains("Invalid object name") Then errorString = $"Information Table does not exist"
								parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = $"Error getting table info: {errorString}" & vbCrLf &
									"Please, talk with an administrator."
								GoTo AfterViewCheckSecond
							End Try
							
							'Get view if provided
							If Not String.IsNullOrEmpty(paramCheckView) Then
								Dim i As Integer = 0
								'Check if there are any transformations not found
								Try
									dt = BRApi.Database.ExecuteSql(
										dbConn,
										$"
										SELECT *
										FROM {paramCheckView.Trim()} WITH(NOLOCK)
										{paramTableFilters};
										",
										dbParamInfos,
										False
									)
									If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
										parentDynamicComponentEx.TemplateSubstVars("p_status") = "Warning"
										parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = $"Transformations not found." & vbCrLf &
											$"Click to open check dashboard."
									End If
									
									i += 1
								Catch ex As Exception
									parentDynamicComponentEx.TemplateSubstVars("p_status") = "Error"
									Dim errorString As String = ex.ToString
									If errorString.Contains("Invalid object name") Then errorString = $"Check view '{paramCheckView}' does not exist"
									parentDynamicComponentEx.TemplateSubstVars("p_warning_message") = $"Error getting check view: {errorString}" & vbCrLf &
										"Please, talk with an administrator."
								End Try
							End If
							AfterViewCheckSecond:
						End Using
						
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
					
					Else
                    	Return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, String.Empty, Nothing, TriStateBool.Unknown, WsDynamicItemStateType.Unknown)
					End If
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
