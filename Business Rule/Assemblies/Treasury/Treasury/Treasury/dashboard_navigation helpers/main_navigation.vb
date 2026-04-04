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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_navigation
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					#Region "Load Dashboard"
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("LoadDashboard") Then
							'Get workspace prefix
							Dim WorkspacePrefix As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Treasury.prm_Treasury_Workspace_Prefix")
							'Declare load dashboard task result
							Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
							'Declare current dashboard
							Dim paramDashboard As String
							
							'If first load, get default parameter, else use the subst var from prior run
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize _
								And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								'Get default dashboard
								paramDashboard = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, $"Treasury.prm_{WorkspacePrefix}_MainNavigation_Default")
							Else
								'Get current dashboard parameter from other business rule call
								paramDashboard = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_CurrentDashboard")
							End If
							'Set current dashboard parameter
							loadDashboardTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard
							
							'Loop through each level of the dashboard based on the name
							Dim dashboardSplitted As String() = paramDashboard.Split("_")
							For level As Integer = 1 To dashboardSplitted.Count - 1
								Dim activeDashboardNumber As Integer = CInt(dashboardSplitted(level))
								
								'Set dashboard content and active tab
								Me.SetActiveContent(si, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
								Me.SetActiveTab(si, loadDashboardTaskResult, WorkspacePrefix, level, 20, activeDashboardNumber)
								'Set dashboard parameters
								Me.SetDashboardParametersOnLoad(si, args, loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber)
							Next
							
							Return loadDashboardTaskResult
							
						End If
						
					#End Region
					
					#Region "Component Selection Changed"
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
					#Region "Update Treasury Views (Unified)"
						
					If args.FunctionName.XFEqualsIgnoreCase("UpdateTreasuryView") Then
						'Get corporate name from parameter
						Dim corporateName As String = args.NameValuePairs.XFGetValue("p_corporate_name")
						
						'Get cash/debt selector
						Dim cashDebtSelector As String = args.NameValuePairs.XFGetValue("prm_Treasury_Cash_Debt_Graph_Selector")
						
						'Get current dashboard from NameValuePairs (passed from dashboard component)
						Dim currentDashboard As String = args.NameValuePairs.XFGetValue("p_current_dashboard")
						
						'Determine which dashboard to load based on corporate name and current location
						Dim newDashboard As String = ""
						Dim substVarName As String = ""
						
						'Detect which section we're in based on current dashboard
						If currentDashboard.StartsWith("Treasury_3_1_1_") Then
							'We're in Cash Debt Position section
							substVarName = "prm_treasury_CashdebtPositionDynamic"
							If corporateName.XFEqualsIgnoreCase("Horse HTD") Then
								newDashboard = "Treasury_3_1_1_HTD"
							ElseIf corporateName.XFEqualsIgnoreCase("Horse Global") Then
								newDashboard = "Treasury_3_1_1_Global"
							Else
								newDashboard = "Treasury_3_1_1_HTD"
							End If
							
						ElseIf currentDashboard.StartsWith("Treasury_3_1_2_") Then
							'We're in Cash Flow Forecasting section
							substVarName = "prm_treasury_CashflowForecastingDynamic"
							If corporateName.XFEqualsIgnoreCase("Horse HTD") Then
								newDashboard = "Treasury_3_1_2_HTD"
							ElseIf corporateName.XFEqualsIgnoreCase("Horse Global") Then
								newDashboard = "Treasury_3_1_2_Global"
							Else
								newDashboard = "Treasury_3_1_2_HTD"
							End If
						ElseIf currentDashboard.StartsWith("Treasury_3_1_3_") Then
							'We're in Cash Flow Forecasting section
							substVarName = "prm_treasury_CashflowForecastingOperDynamic"
							If corporateName.XFEqualsIgnoreCase("Horse HTD") Then
								newDashboard = "Treasury_3_1_3_HTD"
							ElseIf corporateName.XFEqualsIgnoreCase("Horse Global") Then
								newDashboard = "Treasury_3_1_3_Global"
							Else
								newDashboard = "Treasury_3_1_3_HTD"
							End If
						ElseIf currentDashboard.StartsWith("Treasury_4_1_5") OrElse currentDashboard.XFEqualsIgnoreCase("Embedded_Treasury_4_1_5_Graph") Then
							'We're in Weekly Cash/Debt Position section
							substVarName = "prm_treasury_CashdebWeeklyDynamic"
							
							'Check for Debt (case insensitive and trimmed)
							If Not String.IsNullOrEmpty(cashDebtSelector) AndAlso cashDebtSelector.Trim().XFEqualsIgnoreCase("Debt") Then
								newDashboard = "Treasury_4_1_5_Debt_Graph"
							Else
								newDashboard = "Treasury_4_1_5_Cash_Graph"
							End If
						End If
						
						'Handle CashDebt Position Report Export Dynamic based on Scenario
						Dim treasuryScenario As String = args.NameValuePairs.XFGetValue("prm_Treasury_Scenario")
						If Not String.IsNullOrEmpty(treasuryScenario) Then
							substVarName = "prm_treasury_CashdebtPositionReportExporDynamic"
							If treasuryScenario.Trim().XFEqualsIgnoreCase("StartWeek") Then
								newDashboard = "Treasury_4_5_1_CashDebtStartWeek"
							ElseIf treasuryScenario.Trim().XFEqualsIgnoreCase("EOM") Then
								newDashboard = "Treasury_4_5_1_CashDebtEOM"
							ElseIf treasuryScenario.Trim().XFEqualsIgnoreCase("EOW") Then
								newDashboard = "Treasury_4_5_1_CashDebtEOW"
							End If
						End If
						
						'Update the appropriate dashboard substitution variable
						If Not String.IsNullOrEmpty(substVarName) AndAlso Not String.IsNullOrEmpty(newDashboard) Then
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
							selectionChangedTaskResult.ModifiedCustomSubstVars(substVarName) = newDashboard
							Return selectionChangedTaskResult
						End If
					End If
					
					#End Region
					
					#Region "Open Dashboard"
						
					If args.FunctionName.XFEqualsIgnoreCase("OpenDashboard")
							'Get dashboard
							Dim paramDashboard As String = args.NameValuePairs("p_dashboard")
							'Set current dashboard
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = True
							selectionChangedTaskResult.ModifiedCustomSubstVars("prm_CurrentDashboard") = paramDashboard
							
							'Control dashboard parameters on click
							Me.SetDashboardParametersOnClick(si, args, selectionChangedTaskResult, paramDashboard)
							
							Return selectionChangedTaskResult
							
						#End Region
						
						End If
						
					#End Region
					
					#Region "SQL Table Editor Save Data"
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement SQL Table Editor Save Data logic here.
							'Save the data rows.
							'Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							'Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName)
								'dbConn.BeginTrans()
								'BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, True, False, False)
								'dbConn.CommitTrans()
							'End Using
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.Message = ""
							saveDataTaskResult.CancelDefaultSave = False 'Note: Use True if we already saved the data rows in this Business Rule.
							Return saveDataTaskResult
						End If
						
						#End Region
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "Set Active Content"
		
		Private Sub SetActiveContent(
			ByVal si As SessionInfo, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Set dynamic content parameter
			'If level is 1, set the initial dashboard, else append to the last level parameter
			If level = 1 Then
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_Content") = $"{WorkspacePrefix}_{activeDashboardNumber}"
			Else
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_Content") = _
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level - 1}_Content") &
					$"_{activeDashboardNumber}"
			End If
		End Sub
		
		#End Region
		
		#Region "Set Active Tab"
		
		Private Sub SetActiveTab(
			ByVal si As SessionInfo, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal quantity As Integer, ByVal activeDashboardNumber As Integer
		)
			'Get colors for each level
			Dim level1Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab1_Active")
			Dim level2Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab2_Active")
			Dim level3Active As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBar_Tab3_Active")
			
			'Set colors for each level
			'Color tuple definition: (ActiveColor, InactiveColor)
			Dim colorDictionary As New Dictionary(Of Integer, Tuple(Of String, String)) From {
				{1, New Tuple(Of String, String)(level1Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBarBG"))},
				{2, New Tuple(Of String, String)(level2Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, Nothing, "Shared.prm_Style_Colors_LeftBarBG"))},
				{3, New Tuple(Of String, String)(level3Active, "White")},
				{4, New Tuple(Of String, String)(level3Active, "White")},
				{5, New Tuple(Of String, String)(level3Active, "White")}
			}
			
			'Loop through all the repeater components
			For i As Integer = 1 To quantity
				'Control left tabs
				If level = 1 Then 
					loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_{WorkspacePrefix}_{i}_LeftTabs") = _
					If(i = activeDashboardNumber, $"{WorkspacePrefix}_{i}_LeftTabs", $"{WorkspacePrefix}_Transparent")
				End If
				loadDashboardTaskResult.ModifiedCustomSubstVars($"prm_Dashboard_Tab_Level{level}_ActiveColor_{i}") = _
					If(i = activeDashboardNumber, colorDictionary(level).Item1, colorDictionary(level).Item2)
			Next
		End Sub
		
		#End Region
		
		#Region "Set Dashboards Parameters On Load"
		
		Private Sub SetDashboardParametersOnLoad(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef loadDashboardTaskResult As XFLoadDashboardTaskResult,
			ByVal WorkspacePrefix As String, ByVal level As Integer, ByVal activeDashboardNumber As Integer
		)
			'Control dashboard parameters
			If level = 1 Then
				'Get available companies depending on security group
				Dim availableCompanies As List(Of Integer) = HelperFunctionsBR.GetCompanyIDsForUser(si)
				
				'Set default company for combobox based on the company ids for user
				If availableCompanies.Contains(0) OrElse availableCompanies.Count = 0 Then
					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_DefaultCompany") = "0"
				ElseIf availableCompanies.Count > 0 Then
					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_DefaultCompany") = availableCompanies(0).ToString
				End If
				
				'Set default week number based on current date
				Dim currentWeekNumber As Integer = Me.GetCurrentWeekNumber(si)
				loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_DefaultWeekNumber") = currentWeekNumber.ToString
				
				If args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_WeekNumber") = "" Then
					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_WeekNumber") = currentWeekNumber.ToString
				End If
				
				'Set dynamic Cash Debt Position dashboard based on corporate name
				Dim defaultCorporateName As String = "Horse HTD"
				
				'Always set default corporate name for ComboBox display
				loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_DefaultCorporateNames") = defaultCorporateName
				
				'Initialize prm_Treasury_CorporateNames if empty
				If args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_CorporateNames") = "" Then
					loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_CorporateNames") = defaultCorporateName
				End If
				
				'Get current corporate name value (either from prior run or just initialized)
				Dim corporateName As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_CorporateNames")
				If String.IsNullOrEmpty(corporateName) Then
					corporateName = defaultCorporateName
				End If
				
			Dim cashdebtPositionDashboard As String
			Dim cashflowForecastingDashboard As String
			Dim cashflowForecastingOperDashboard As String			'Determine dashboards based on corporate name
			If corporateName.XFEqualsIgnoreCase("Horse HTD") Then
				cashdebtPositionDashboard = "Treasury_3_1_1_HTD"
				cashflowForecastingDashboard = "Treasury_3_1_2_HTD"
				cashflowForecastingOperDashboard = "Treasury_3_1_3_HTD"
			ElseIf corporateName.XFEqualsIgnoreCase("Horse Global") Then
				cashdebtPositionDashboard = "Treasury_3_1_1_Global"
				cashflowForecastingDashboard = "Treasury_3_1_2_Global"
				cashflowForecastingOperDashboard = "Treasury_3_1_3_Global"
			Else
				'Default to HTD if value is unexpected
				cashdebtPositionDashboard = "Treasury_3_1_1_HTD"
				cashflowForecastingDashboard = "Treasury_3_1_2_HTD"
				cashflowForecastingOperDashboard = "Treasury_3_1_3_HTD"
			End If			'Set the substitution variables
			loadDashboardTaskResult.ModifiedCustomSubstVars("prm_treasury_CashdebtPositionDynamic") = cashdebtPositionDashboard
			loadDashboardTaskResult.ModifiedCustomSubstVars("prm_treasury_CashflowForecastingDynamic") = cashflowForecastingDashboard
			loadDashboardTaskResult.ModifiedCustomSubstVars("prm_treasury_CashflowForecastingOperDynamic") = cashflowForecastingOperDashboard
			
			'Set dynamic Weekly Cash/Debt Position dashboard based on selector
			Dim defaultCashDebtSelector As String = "Cash"
			
			'Always set default Cash/Debt selector for ComboBox display
			loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_Default_Cash_Debt_Graph_Selector") = defaultCashDebtSelector
			
			'Initialize prm_Treasury_Cash_Debt_Graph_Selector if empty
			If args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_Cash_Debt_Graph_Selector") = "" Then
				loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_Cash_Debt_Graph_Selector") = defaultCashDebtSelector
			End If
			
			'Get current selector value (either from prior run or just initialized)
			Dim cashDebtSelector As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_Cash_Debt_Graph_Selector")
			If String.IsNullOrEmpty(cashDebtSelector) Then
				cashDebtSelector = defaultCashDebtSelector
			End If
			
			Dim cashDebtDashboard As String
			If cashDebtSelector.XFEqualsIgnoreCase("Debt") Then
				cashDebtDashboard = "Treasury_4_1_5_Debt_Graph"
			Else
				cashDebtDashboard = "Treasury_4_1_5_Cash_Graph"
			End If
			
			loadDashboardTaskResult.ModifiedCustomSubstVars("prm_treasury_CashdebWeeklyDynamic") = cashDebtDashboard
			
			'Set dynamic CashDebt Position Report Export dashboard based on Scenario
			Dim defaultScenario As String = "StartWeek"
			
			'Initialize prm_Treasury_Scenario if empty
			If args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_Scenario") = "" Then
				loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_Scenario") = defaultScenario
			End If
			
			'Get current scenario value (either from prior run or just initialized)
			Dim treasuryScenario As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_Scenario")
			If String.IsNullOrEmpty(treasuryScenario) Then
				treasuryScenario = defaultScenario
			End If
			
			Dim cashDebtReportExportDashboard As String
			If treasuryScenario.XFEqualsIgnoreCase("StartWeek") Then
				cashDebtReportExportDashboard = "Treasury_4_5_1_CashDebtStartWeek"
			ElseIf treasuryScenario.XFEqualsIgnoreCase("EOM") Then
				cashDebtReportExportDashboard = "Treasury_4_5_1_CashDebtEOM"
			ElseIf treasuryScenario.XFEqualsIgnoreCase("EOW") Then
				cashDebtReportExportDashboard = "Treasury_4_5_1_CashDebtEOW"
			Else
				'Default to StartWeek if value is unexpected
				cashDebtReportExportDashboard = "Treasury_4_5_1_CashDebtStartWeek"
			End If
			
			loadDashboardTaskResult.ModifiedCustomSubstVars("prm_treasury_CashdebtPositionReportExporDynamic") = cashDebtReportExportDashboard
			
			'Ensure comment file exists for the current parameters
			Dim paramYear As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_Year")
			Dim paramWeekNumber As String = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_Treasury_WeekNumber")
			
			'If year is empty, use current year
			If String.IsNullOrEmpty(paramYear) Then
				paramYear = DateTime.Now.Year.ToString()
				loadDashboardTaskResult.ModifiedCustomSubstVars("prm_Treasury_Year") = paramYear
			End If
			
			'If week number is empty, use current week number
			If String.IsNullOrEmpty(paramWeekNumber) Then
				paramWeekNumber = currentWeekNumber.ToString()
			End If
			
			'Create comment file if it doesn't exist
			Me.EnsureCommentFileExists(si, treasuryScenario, paramWeekNumber, paramYear)
			End If
			
		End Sub
		
		#End Region
		
		#Region "Set Dashboards Parameters On Click"
		
		Private Sub SetDashboardParametersOnClick(
			ByVal si As SessionInfo, ByVal args As DashboardExtenderArgs, ByRef selectionChangedTaskResult As XFSelectionChangedTaskResult,
			ByVal paramDashboard As String
		)
			'Control dashboard parameters
			'EXAMPLE
'			If paramDashboard = "Treasury_1_1" Then _
'				selectionChangedTaskResult.ModifiedCustomSubstVars("prm_Treasury_Request_Id") = ""
			
		End Sub
		
		#End Region
		
		#Region "Ensure Comment File Exists"
		
		''' <summary>
		''' Ensures that the comment file exists for the given parameters.
		''' If the file doesn't exist, it creates it from the template.
		''' </summary>
		Private Sub EnsureCommentFileExists(ByVal si As SessionInfo, ByVal paramScenario As String, ByVal paramWeek As String, ByVal paramYear As String)
			Try
				Dim templatePath As String = "Documents/Public/Treasury/Extensible Document"
				Dim targetBasePath As String = "Documents/Public/Treasury/Extensible Document/Comments"
				Dim filePath As String = targetBasePath & "/" & paramYear & "/" & paramWeek
				Dim fileName As String = paramScenario & "_" & paramWeek & "_" & paramYear & ".docx"
				
				'Check if file already exists
				Dim fileExists As Boolean = BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath & "/" & fileName)
				
				If Not fileExists Then
					'Create folder structure if necessary
					BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetBasePath, paramYear & "/" & paramWeek)
					
					'Get template file
					Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, templatePath & "/commentTemplate.docx", True, True)
					Dim bytesOfFile As Byte() = objXFFileEx.XFFile.ContentFileBytes
					
					'Create file info
					Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, fileName, filePath)
					Dim userName As String = si.AuthToken.UserName
					
					'Set file properties
					fileInfo.ContentFileExtension = "docx"
					fileInfo.Description = "Auto-generated by " & userName
					fileInfo.ContentFileContainsData = True
					fileInfo.XFFileType = True
					
					'Insert the file
					Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, bytesOfFile)
					BRApi.FileSystem.InsertOrUpdateFile(si, fileFile)
				End If
			Catch ex As Exception
				'Log error but don't throw - file creation is not critical for dashboard loading
				ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub
		
		#End Region
		
		#Region "Get Current Week Number"
		
		''' <summary>
		''' Gets the current week number from XFC_TRS_AUX_Date table based on today's date
		''' </summary>
		Private Function GetCurrentWeekNumber(ByVal si As SessionInfo) As Integer
			Try
				Dim currentDate As Date = DateTime.Now.Date
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim selectQuery As String = "
						SELECT TOP 1 weekNumber
						FROM XFC_TRS_AUX_Date
						WHERE @currentDate >= weekStartDate 
							AND @currentDate <= weekEndDate
						ORDER BY weekStartDate
					"
					
					Dim dbParamInfos As New List(Of DbParamInfo) From {
						New DbParamInfo("currentDate", currentDate)
					}
					
					Dim resultDt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, dbParamInfos, False)
					
					If resultDt.Rows.Count > 0 Then
						Return Convert.ToInt32(resultDt.Rows(0)("weekNumber"))
					Else
						' If no week found, return current week of year as fallback
						Dim culture As New CultureInfo("en-US")
						Return culture.Calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
					End If
				End Using
			Catch ex As Exception
				' If any error occurs, return current week of year as fallback
				Dim culture As New CultureInfo("en-US")
				Return culture.Calendar.GetWeekOfYear(DateTime.Now.Date, CalendarWeekRule.FirstDay, DayOfWeek.Monday)
			End Try
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
