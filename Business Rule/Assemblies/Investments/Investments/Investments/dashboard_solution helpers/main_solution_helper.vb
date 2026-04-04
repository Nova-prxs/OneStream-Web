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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_solution_helper
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							
							'Implement Load Dashboard logic here.
							
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
								loadDashboardTaskResult.ModifiedCustomSubstVars = Nothing
								Return loadDashboardTaskResult
								End If
						End If
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						#Region "Cash"
						
						#Region "Begin Project Cash"
						
						If args.FunctionName.XFEqualsIgnoreCase("BeginProjectCashYear") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramYear As Integer = CInt(args.NameValuePairs("p_year"))
							
							'Create merge query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								'Declare db param infos
								Dim dbParamInfos As New List(Of DbParamInfo) From {
									New DbParamInfo("paramYear", paramYear)
								}
								Try
									Dim mergeQuery As String = $"
										MERGE INTO XFC_INV_FACT_project_cash AS target
										USING (
											SELECT DISTINCT
												project,
												'Actual' AS scenario,
												@paramYear AS year,
												1 AS month,
												0 AS amount
											FROM XFC_INV_MASTER_project
											WHERE @paramYear BETWEEN YEAR(start_date) - 3 AND YEAR(end_date) + 3
										) AS source
										ON target.project_id = source.project
										AND target.scenario = source.scenario
										AND target.year = source.year
										AND target.month = source.month
										WHEN MATCHED THEN
										    UPDATE SET
										        amount = target.amount
										WHEN NOT MATCHED THEN
										    INSERT (
										        project_id, scenario, year, month, amount
										    )
										    VALUES (
										        source.project, source.scenario, source.year, source.month, source.amount
										    );
									"
									BRApi.Database.ExecuteSql(dbConn, mergeQuery, dbParamInfos, False)
								Catch ex As Exception
									dbConn.RollbackTrans()
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
								dbConn.CommitTrans()
							End Using
						
						#End Region
						
						#Region "Check Clear Projections"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckClearProjections") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramCompany As Integer = CInt(args.NameValuePairs("p_company"))
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If paramCompany = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Confirm Cash Month"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("ConfirmCashMonth") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'Confirm month
							HelperFunctionsBR.SetMonthConfirmation(si, "cash", parameterDict("paramCompany"), parameterDict("paramScenario"),
								parameterDict("paramYear"), parameterDict("paramMonth"), True
							)
						
						#End Region
						
						#Region "Unconfirm Cash Month"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("UnconfirmCashMonth") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							If Not BRApi.Security.Authorization.IsUserInGroup(si,"F_INV_Super")  Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must be an Admin to unconfirm the month."
								Return selectionChangedTaskResult
							End If
							
							'Confirm month
							HelperFunctionsBR.SetMonthConfirmation(si, "cash", parameterDict("paramCompany"), parameterDict("paramScenario"),
								parameterDict("paramYear"), parameterDict("paramMonth"), False
							)
						
						#End Region
						
						#Region "Create Cash Comment"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CreateCashComment") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'Create merge query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								Try
									Dim mergeQuery As String = $"
										MERGE INTO XFC_INV_AUX_comment AS target
										USING (
											SELECT
												@paramType AS type,
												@paramCompany AS company_id,
												@paramScenario AS scenario,
												@paramYear AS year,
												@paramMonth AS month,
												'Text here' AS comment
										) AS source
										ON target.type = source.type
										AND target.company_id = source.company_id
										AND target.scenario = source.scenario
										AND target.year = source.year
										AND target.month = source.month
										WHEN NOT MATCHED THEN
										    INSERT (
										        type, company_id, scenario, year, month, comment
										    )
										    VALUES (
										        source.type, source.company_id, source.scenario, source.year, source.month, source.comment
										    );
									"
									BRApi.Database.ExecuteSql(dbConn, mergeQuery, dbParamInfos, False)
								Catch ex As Exception
									dbConn.RollbackTrans()
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
								dbConn.CommitTrans()
							End Using
						
						#End Region
						
						#Region "Import Cash"
						
						#Region "Check Import Cash"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckImportCash") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Download Cash Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadCashTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramCompany As String = args.NameValuePairs("p_company")
							Dim paramScenario As String = args.NameValuePairs("p_scenario")
							Dim paramYear As String = args.NameValuePairs("p_year")
							Dim paramUsername As String = si.UserName
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = "CashTemplate.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"ProjectCash_{paramCompany}_{paramScenario}_{paramYear}.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
						
						#End Region
						
						#End Region
						
						#Region "Create GAP Comment"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CreateGAPComment") Then
						    'Declare selection changed task result
						    Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						    selectionChangedTaskResult.IsOK = True
						    selectionChangedTaskResult.ShowMessageBox = False
						    
						    'Get parameters for queries
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    						    						    
						    'Create merge query
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        dbConn.BeginTrans()
						        Try
									Dim mergeQuery As String = $"
									            MERGE INTO XFC_INV_AUX_comment_GAP AS target
									            USING (
									                SELECT DISTINCT
									                    m.company_name,
									                    m.company_id,
									                    @paramScenario AS scenario,
									                    @paramYear AS year,
									                    @paramMonth AS month,
									                    'Text here' AS comment
									                FROM XFC_INV_MASTER_project m						                  
									            ) AS source
									            ON target.company_id = source.company_id
									                AND target.company_name = source.company_name
									                AND target.scenario = source.scenario
									                AND target.year = source.year
									                AND target.month = source.month
									            WHEN NOT MATCHED THEN
									                INSERT (
									                    company_id, 
									                    company_name,    -- Añadido company_name
									                    scenario, 
									                    year, 
									                    month, 
									                    comment
									                )
									                VALUES (
									                    source.company_id, 
									                    source.company_name,    -- Añadido source.company_name
									                    source.scenario, 
									                    source.year, 
									                    source.month, 
									                    source.comment
									                );
									        "
						            BRApi.Database.ExecuteSql(dbConn, mergeQuery, dbParamInfos, False)
						        Catch ex As Exception
						            dbConn.RollbackTrans()
						            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						        End Try
						        dbConn.CommitTrans()
						    End Using
												
						
						#End Region

						
						#End Region
						
						#Region "Projects"
						
						#Region "Check Create Project"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckCreateProject") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Create Project"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CreateProject") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							'Add entity member to db param infos to have a generic cost center
							dbParamInfos.Add(
								New DbParamInfo(
									"paramEntityMember",
									HelperFunctionsBR.GetParentEntity(si, parameterDict("paramCompany"))
								)
							)
							
							'Check if all parameters are are fulfilled
							For Each value In parameterDict.Values
								If String.IsNullOrEmpty(value) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"All fields are required"
									Return selectionChangedTaskResult
								End If
							Next
							
							'Check if total requirement is a valid number
							If Not Decimal.TryParse(parameterDict("paramTotalRequirement"), Decimal.Zero) Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"Total Requirement amount not valid." & vbCrLf &
									"Please, enter a valid decimal number."
								Return selectionChangedTaskResult
							End If
							
							'Create insert query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								Try
									Dim insertQuery As String = $"
										--Check Dates
										IF @paramStartDate > @paramEndDate OR @paramStartDate > @paramMADate
										BEGIN
    										RAISERROR('Start date can''t be after End or MA date. Please, check the dates.', 16, 1);
											RETURN;
										END;
									
										INSERT INTO XFC_INV_MASTER_project (
										    func,
										    label_position_1,
										    label_position_2,
										    label_position_3,
										    label_position_4,
										    label_position_5,
										    region,
										    company_name,
										    company_id,
										    type,
										    country,
										    branch,
										    site,
										    professional_area,
										    poi_poe,
										    cc,
										    cc_name,
										    cpi,
										    cpi_name,
										    project,
										    project_name,
										    decision_criteria,
										    aggregate,
										    libre,
										    strategic_axis_name,
										    project_status,
										    cpil_name,
										    program_position,
										    dpci_analyst,
										    reason,
										    start_date,
										    end_date,
										    budget_owner,
										    ma_date,
										    is_real,
											is_closed,
										    total_requirement
										) 
										SELECT
										    '' AS func,
										    '' AS label_position_1,
										    '' AS label_position_2,
										    '' AS label_position_3,
										    '' AS label_position_4,
										    '' AS label_position_5,
										    region,
										    company_name,
										    company_id,
										    @paramType AS type,
										    country,
										    branch,
										    '' AS site,
										    '' AS professional_area,
										    @paramPOIPOE AS poi_poe,
										    '' AS cc,
										    '' AS cc_name,
										    '' AS cpi,
										    '' AS cpi_name,
										    'ONE' + RIGHT('000000' + CAST(
										        COALESCE(
										        	(
														SELECT TOP 1 CAST(SUBSTRING(project, 4, 6) AS INT) + 1
											        	FROM XFC_INV_MASTER_project
														WHERE project LIKE 'ONE%'
											            ORDER BY project DESC
													),
										            1
										        ) AS VARCHAR(6)
										    ), 6) AS project,
										    @paramProjectName AS project_name,
										    '' AS decision_criteria,
										    @paramAggregate AS aggregate,
										    '' AS libre,
										    '' AS strategic_axis_name,
										    '' AS project_status,
										    '' AS cpil_name,
										    '' AS program_position,
										    '' AS dpci_analyst,
										    '' AS reason,
										    @paramStartDate AS start_date,
										    @paramEndDate AS end_date,
										    @paramBudgetOwner AS budget_owner,
										    @paramMADate AS ma_date,
										    0 AS is_real,
											0 AS is_closed,
										    @paramTotalRequirement AS total_requirement
										FROM (
											SELECT TOP 1
											    region,
											    company_name,
											    company_id,
											    country,
											    branch
											FROM XFC_INV_MASTER_project
											WHERE company_id = @paramCompany
										) AS subquery;
									
										--Check if they want to create a generic asset
										IF @paramGenerateAsset = 'True'
										BEGIN
											INSERT INTO XFC_INV_MASTER_asset (
											    id,
												is_real,
												cost_center_id,
												project_id,
												designation,
												category_id,
												activation_date,
												initial_value
											) 
											SELECT
											    'A' + RIGHT('000000' + CAST(
											        COALESCE(
											        	(
															SELECT TOP 1 CAST(SUBSTRING(id, 2, 6) AS INT) + 1
												        	FROM XFC_INV_MASTER_asset
															WHERE id LIKE 'A%'
												            ORDER BY id DESC
														),
											            1
											        ) AS VARCHAR(6)
											    ), 6) AS id,
												0 AS is_real,
											    (
													SELECT TOP 1 id
													FROM XFC_INV_MASTER_cost_center WITH(NOLOCK)
													WHERE entity_member LIKE CONCAT(@paramEntityMember, '%')
													ORDER BY entity_member ASC
												) AS cost_center,
												(
													SELECT TOP 1 project
										        	FROM XFC_INV_MASTER_project WITH(NOLOCK)
													WHERE project LIKE 'ONE%'
														AND company_id = @paramCompany
										            ORDER BY project DESC
												) AS project_id,
												'Generic Asset' AS designation,
												CASE 
													WHEN @paramPOIPOE = 'P.O.I.' THEN '6405'
													WHEN @paramPOIPOE = 'P.O.E.' THEN '50'
												END AS category_id,
										    	@paramMADate AS activation_date,
											    @paramTotalRequirement AS initial_value;
										END;
									"
									BRApi.Database.ExecuteSql(dbConn, insertQuery, dbParamInfos, False)
								Catch ex As Exception
									dbConn.RollbackTrans()
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
								dbConn.CommitTrans()
							End Using
						
						#End Region
						
						#Region "Check Delete Project"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckDeleteProject") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramProject As String = args.NameValuePairs("p_project")
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
							
							'Control if user has selected a project
							If String.IsNullOrEmpty(paramProject) Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a project."
								Return selectionChangedTaskResult
							End If
							
							'Control if project begins with 'ONE'
							If Not paramProject.StartsWith("ONE") Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You can't delete project: {paramProject}." & vbCrlf &
									"Project must have been created in OneStream."
								Return selectionChangedTaskResult
							End If
						
						#End Region
						
						#Region "Delete Project From ProjectInfo"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DeleteProjectFromInfo") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramProject As String = args.NameValuePairs("p_project")
							
							'Control if user has selected a project
							If String.IsNullOrEmpty(paramProject) Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a project."
								Return selectionChangedTaskResult
							End If
														
							'Create delete query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								'Declare db param infos
								Dim dbParamInfos As New List(Of DbParamInfo) From {
									New DbParamInfo("paramProject", paramProject)
								}
								Try
									Dim deleteQuery As String = $"
										DELETE FROM XFC_INV_FACT_project_cash
										WHERE project_id = @paramProject;
									
										DELETE FROM XFC_INV_FACT_project_financial_figures
										WHERE project_id = @paramProject;
									
										DELETE FROM XFC_INV_MASTER_project
										WHERE project = @paramProject;
									
										DELETE ad
										FROM XFC_INV_FACT_asset_depreciation ad
										INNER JOIN XFC_INV_MASTER_asset a
											ON ad.asset_id = a.id
										WHERE a.project_id = @paramProject;
									
										DELETE FROM XFC_INV_MASTER_asset
										WHERE project_id = @paramProject;
									
										DELETE FROM XFC_INV_AUX_modified_projection
										WHERE project_id = @paramProject;
																			
									"
									BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
								Catch ex As Exception
									dbConn.RollbackTrans()
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
								dbConn.CommitTrans()
							End Using
						
						#End Region
						
						#Region "Delete Project"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DeleteProject") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramProject As String = args.NameValuePairs("p_project")
							
							'Control if user has selected a project
							If String.IsNullOrEmpty(paramProject) Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a project."
								Return selectionChangedTaskResult
							End If
							
							'Control if project begins with 'ONE'
							If Not paramProject.StartsWith("ONE") Then
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You can't delete project: {paramProject}." & vbCrlf &
									"Project must be created in OneStream"
								Return selectionChangedTaskResult
							End If
							
							'Create delete query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								'Declare db param infos
								Dim dbParamInfos As New List(Of DbParamInfo) From {
									New DbParamInfo("paramProject", paramProject)
								}
								Try
									Dim deleteQuery As String = $"
										DELETE FROM XFC_INV_FACT_project_cash
										WHERE project_id = @paramProject;
									
										DELETE FROM XFC_INV_FACT_project_financial_figures
										WHERE project_id = @paramProject;
									
										DELETE FROM XFC_INV_MASTER_project
										WHERE project = @paramProject;
									
										DELETE ad
										FROM XFC_INV_FACT_asset_depreciation ad
										INNER JOIN XFC_INV_MASTER_asset a
											ON ad.asset_id = a.id
										WHERE a.project_id = @paramProject;
									
										DELETE FROM XFC_INV_MASTER_asset
										WHERE project_id = @paramProject;
									
										DELETE FROM XFC_INV_AUX_modified_projection
										WHERE project_id = @paramProject;
									"
									BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
								Catch ex As Exception
									dbConn.RollbackTrans()
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
								dbConn.CommitTrans()
							End Using
						
						#End Region
						
						#Region "Check Central Import"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckCentralImport") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") <> 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select (Any) in the Company filter."
								Return selectionChangedTaskResult
							End If
						
						#End Region
						
						#Region "Import RnD Projects"
						
						#Region "Download RnD Projects Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadRnDProjectsTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramCompany As String = args.NameValuePairs("p_company")
							Dim paramScenario As String = args.NameValuePairs("p_scenario")
							Dim paramYear As String = args.NameValuePairs("p_year")
							Dim paramLoadType As String = args.NameValuePairs("p_load_type")
							Dim paramUsername As String = si.UserName
							Dim revenueString As String = If(paramLoadType = "revenue", "Revenue", "")
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = $"RnDProjects{revenueString}Template.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"RnDProjects{revenueString}.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
						
						#End Region
						
						#End Region
						
					    #Region "Download Project Update Info Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadProjectUpdateInfoTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramCompany As String = args.NameValuePairs("p_company")
							Dim paramUsername As String = si.UserName
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = "ProjectUpdateInfoTemplate.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"ProjectUpdateInfo_{paramCompany}.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
						
						#End Region
						
						#End Region
						
						#Region "Capital"
						
						#Region "Check Create Asset"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckCreateAsset") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Create Asset"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CreateAsset") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Check if all parameters are are fulfilled
							For Each value In parameterDict.Values
								If String.IsNullOrEmpty(value) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"All fields are required"
									Return selectionChangedTaskResult
								End If
							Next
							
							'Check if total requirement is a valid number
							If Not Decimal.TryParse(parameterDict("paramInitialValue"), Decimal.Zero) Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"Initial Value amount not valid." & vbCrLf &
									"Please, enter a valid decimal number."
								Return selectionChangedTaskResult
							End If
							
							'Check if selected project is a valid one
							If parameterDict("paramProject").ToLower = "(any)" Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"Please, select a valid project"
								Return selectionChangedTaskResult
							End If
							
							'Create insert query
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								    Try
								        ' >>> PASO 1: Calcular el nuevo Asset ID y guardarlo en una variable <<<
								        ' Esta consulta extrae la lógica que tenías dentro del INSERT
								        Dim nextIdQuery As String = $"
								            SELECT 
								                'A' + RIGHT('000000' + CAST(
								                    COALESCE(
								                        (
								                            SELECT TOP 1 CAST(SUBSTRING(id, 2, 6) AS INT) + 1
								                            FROM XFC_INV_MASTER_asset
								                            WHERE id LIKE 'A%' AND ISNUMERIC(SUBSTRING(id, 2, 6)) = 1
								                            ORDER BY id DESC
								                        ),
								                        1
								                    ) AS VARCHAR(6)
								                ), 6)
								        "
										
								        Dim finalAssetId As String = BRApi.Database.ExecuteSql(dbConn, nextIdQuery, True).Rows(0)(0).ToString()
								
								        dbParamInfos.Add(New DbParamInfo("@finalAssetId", finalAssetId))
								
								
								        Dim insertQuery As String = $"
								            INSERT INTO XFC_INV_MASTER_asset (
								                id,
								                is_real,
								                cost_center_id,
								                project_id,
								                designation,
								                category_id,
								                activation_date,
								                initial_value
								            )
								            VALUES (
								                @finalAssetId, -- Usamos el ID que calculamos
								                0,
								                @paramCostCenter,
								                @paramProject,
								                @paramDesignation,
								                @paramCategory,
								                @paramMADate,
								                @paramInitialValue
								            );
								        "
								        BRApi.Database.ExecuteSql(dbConn, insertQuery, dbParamInfos, False)
								
								
								        ' >>> PASO 3: Usar la misma variable en el SEGUNDO INSERT <<<
								        Dim insertAuxQuery As String = $"
								            INSERT INTO XFC_INV_AUX_Fictitious_Assets (asset_id, project_id, scenario, year)
								            VALUES (@finalAssetId, @paramProject, @paramScenario, @paramYear)
								        "

								        BRApi.Database.ExecuteSql(dbConn, insertAuxQuery, dbParamInfos, False)
								
								    Catch ex As Exception
								        dbConn.RollbackTrans()
								        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								    End Try
								    dbConn.CommitTrans()
								End Using
						
						#End Region
						
						#Region "Import Assets"
						
						#Region "Check Import Assets"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckImportAssets") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Download Asset Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadAssetTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramCompany As String = args.NameValuePairs("p_company")
							Dim paramScenario As String = args.NameValuePairs("p_scenario")
							Dim paramYear As String = args.NameValuePairs("p_year")
							Dim paramUsername As String = si.UserName
							
							'Decide actual or planning template based on scenario
							Dim templateScenario As String = If(paramScenario = "Actual", "Actual", "Planning")
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = $"Asset{templateScenario}Template.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"New_Asset_{paramScenario}_{paramCompany}.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
						#End Region
						
						#End Region
						
						#Region "Import Capitalizations"
						
						#Region "Check Import Capitalizations"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckImportCapitalizations") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Download Capitalizations Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadCapitalizationsTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramCompany As String = args.NameValuePairs("p_company")
							Dim paramScenario As String = args.NameValuePairs("p_scenario")
							Dim paramYear As String = args.NameValuePairs("p_year")
							Dim paramUsername As String = si.UserName
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = "CapitalizationsTemplate.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"ProjectCapitalizations_{paramCompany}_{paramScenario}_{paramYear}.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
						
						#End Region
						
						#End Region
						
						#Region "Check Delete Asset"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckDeleteAsset") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Delete Asset"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DeleteAsset") Then
						    'Declare selection changed task result
						    Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						    selectionChangedTaskResult.IsOK = True
						    selectionChangedTaskResult.ShowMessageBox = False
						    
						    'Get parameters for queries
						    Dim queryParams As String = args.NameValuePairs("p_query_params")
						    Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
						    Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    
						    'Control if user has selected assets
						    If String.IsNullOrEmpty(parameterDict("paramAssetFrom")) OrElse _
						        String.IsNullOrEmpty(parameterDict("paramAssetTo"))Then
						        selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
						        selectionChangedTaskResult.IsOK = False
						        selectionChangedTaskResult.ShowMessageBox = True
						        selectionChangedTaskResult.Message = $"You must select both Asset From and Asset To."
						        Return selectionChangedTaskResult
						    End If
						    
						    'Control if scenario and year parameters are provided
						    If String.IsNullOrEmpty(parameterDict("paramScenario")) OrElse _
						        String.IsNullOrEmpty(parameterDict("paramYear")) Then
						        selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
						        selectionChangedTaskResult.IsOK = False
						        selectionChangedTaskResult.ShowMessageBox = True
						        selectionChangedTaskResult.Message = $"Scenario and Year are required for deletion."
						        Return selectionChangedTaskResult
						    End If
						    
						    'Control if assets begins with 'A'
						    If Not parameterDict("paramAssetFrom").StartsWith("A") OrElse _
						        Not parameterDict("paramAssetTo").StartsWith("A") Then
						        selectionChangedTaskResult.IsOK = False
						        selectionChangedTaskResult.ShowMessageBox = True
						        selectionChangedTaskResult.Message = $"You can't delete assets." & vbCrlf &
						            "Assets must be created in OneStream"
						        Return selectionChangedTaskResult
						    End If
						    
						    'Create delete query
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        dbConn.BeginTrans()
						        Try
						            Dim deleteQuery As String = $"
						                -- Get filtered projects by company
						                WITH filtered_project AS (
						                    SELECT project AS project_id
						                    FROM XFC_INV_MASTER_project WITH(NOLOCK)
						                    WHERE company_id = @paramCompany
						                )
						
						                -- Insert filtered assets to a temporal table (now including scenario and year filter)
						                SELECT a.id AS asset_id, a.project_id
						                INTO #filtered_asset
						                FROM (
						                    SELECT a.id, a.project_id
						                    FROM XFC_INV_MASTER_asset a
						                    INNER JOIN XFC_INV_AUX_Fictitious_Assets aux ON a.id = aux.asset_id
						                    WHERE a.is_real = 0 
						                    AND aux.scenario = @paramScenario
						                    AND aux.year = @paramYear
						                ) a
						                WHERE EXISTS (
						                    SELECT 1
						                    FROM filtered_project fp
						                    WHERE a.project_id = fp.project_id
						                ) AND a.id BETWEEN @paramAssetFrom AND @paramAssetTo;
						
						                -- Delete from auxiliary table first (to avoid foreign key constraints)
						                DELETE aux
						                FROM XFC_INV_AUX_Fictitious_Assets aux
						                WHERE EXISTS (
						                    SELECT 1
						                    FROM #filtered_asset fa
						                    WHERE aux.asset_id = fa.asset_id 
						                    AND aux.scenario = @paramScenario
						                    AND aux.year = @paramYear
						                );
						
						                -- Delete asset depreciation using temporal table as a filter
						                DELETE ad
						                FROM XFC_INV_FACT_asset_depreciation ad
						                WHERE EXISTS (
						                    SELECT 1
						                    FROM #filtered_asset fa
						                    WHERE ad.asset_id = fa.asset_id AND ad.project_id = fa.project_id
						                );
						
						                -- Delete assets using temporal table as a filter
						                DELETE a
						                FROM XFC_INV_MASTER_asset a
						                WHERE EXISTS (
						                    SELECT 1
						                    FROM #filtered_asset fa
						                    WHERE a.id = fa.asset_id AND a.project_id = fa.project_id
						                );
						
						                -- Drop temporal table
						                DROP TABLE #filtered_asset;
						            "
						            BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
						        Catch ex As Exception
						            dbConn.RollbackTrans()
						            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						        End Try
						        dbConn.CommitTrans()
						    End Using
						
						#End Region
						
						#Region "Import Depreciation"
						
						#Region "Check Import Depreciation"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CheckImportDepreciation") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'For planning scenarios, check if it's confirmed
							If parameterDict("paramScenario") <> "Actual" Then
								If HelperFunctionsBR.CheckScenarioYearConfirmation(si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")) Then
									selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
									selectionChangedTaskResult.IsOK = False
									selectionChangedTaskResult.ShowMessageBox = True
									selectionChangedTaskResult.Message = $"You can't modify the data, scenario and year are confirmed."
									Return selectionChangedTaskResult
								End If
							End If
						
						#End Region
						
						#Region "Download Depreciation Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadDepreciationTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramCompany As String = args.NameValuePairs("p_company")
							Dim paramScenario As String = args.NameValuePairs("p_scenario")
							Dim paramYear As String = args.NameValuePairs("p_year")
							Dim paramUsername As String = si.UserName
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = "AssetDepreciationTemplate.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"AssetDepreciation_{paramCompany}_{paramScenario}_{paramYear}.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
							
						#End Region
						
						#End Region
						
						#End Region
						
						#Region "Financial Statements"
						
						#Region "Confirm Scenario Year"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("ConfirmScenarioYear") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'Confirm scenario year
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
									dbConn,
									$"
									MERGE INTO XFC_INV_AUX_confirmed_scenario_year AS target
									USING (
										SELECT
											@paramCompany AS company_id,
											@paramScenario AS scenario,
											@paramYear AS year
									) AS source
									ON target.company_id = source.company_id
									AND target.scenario = source.scenario
									AND target.year = source.year
									WHEN MATCHED THEN
									    UPDATE SET
											is_confirmed = 'Completed'
									WHEN NOT MATCHED THEN
									    INSERT (
									        company_id, scenario, year, is_confirmed
									    )
									    VALUES (
									        source.company_id, source.scenario, source.year, 'Completed'
									    );
									",
									dbParamInfos, False
								)
							End Using
						
						#End Region
						
						#Region "Unconfirm Scenario Year"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("UnconfirmScenarioYear") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Control if user has selected a company
							If parameterDict("paramCompany") = 0 Then
								selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = $"You must select a Company Name."
								Return selectionChangedTaskResult
							End If
							
							'Unconfirm scenario year
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
									dbConn,
									$"
									MERGE INTO XFC_INV_AUX_confirmed_scenario_year AS target
									USING (
										SELECT
											@paramCompany AS company_id,
											@paramScenario AS scenario,
											@paramYear AS year
									) AS source
									ON target.company_id = source.company_id
									AND target.scenario = source.scenario
									AND target.year = source.year
									WHEN MATCHED THEN
									    UPDATE SET
											is_confirmed = 'In Progress'
									WHEN NOT MATCHED THEN
									    INSERT (
									        company_id, scenario, year, is_confirmed
									    )
									    VALUES (
									        source.company_id, source.scenario, source.year, 'In Progress'
									    );
									",
									dbParamInfos, False
								)
							End Using
						
						#End Region
						
						#End Region
						
						#Region "Seed PCT Data"						
						
'						Else If args.FunctionName.XFEqualsIgnoreCase("TuNombreDeFuncion") Then
'						    'Declare selection changed task result
'						    Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
'						    selectionChangedTaskResult.IsOK = True
'						    selectionChangedTaskResult.ShowMessageBox = False
						    
'						    'Dim paramCompany As String = args.NameValuePairs("p_company")
'						    'Dim paramScenario As String = args.NameValuePairs("p_scenario")
'						    'Dim paramYear As String = args.NameValuePairs("p_year")
						    
'						    'Get parameters for queries (si usas el sistema de parámetros existente)
'						    'Dim queryParams As String = args.NameValuePairs("p_query_params")
'						    'Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
'						    'Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
						    
'						    'Validaciones (opcional - ejemplo de validación de company)
'						    'If parameterDict("paramCompany") = 0 Then
'						    '    selectionChangedTaskResult.ChangeSelectionChangedUIActionInDashboard = True
'						    '    selectionChangedTaskResult.IsOK = False
'						    '    selectionChangedTaskResult.ShowMessageBox = True
'						    '    selectionChangedTaskResult.Message = "You must select a Company Name."
'						    '    Return selectionChangedTaskResult
'						    'End If
						    
'						    'Ejecutar tu query SQL
'						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'						        dbConn.BeginTrans()
'						        Try
'						            'Aquí defines tus parámetros si los necesitas
'						            Dim dbParamInfos As New List(Of DbParamInfo) From {
'						                'New DbParamInfo("paramNombre", valorParametro)
'						            }
						            
'						            'Tu query SQL aquí
'						            Dim tuQuery As String = $"
'						                -- Aquí va tu query SQL
'						                -- SELECT * FROM TuTabla WHERE Condicion = @paramNombre
'						            "
						            
'						            'Ejecutar la query
'						            BRApi.Database.ExecuteSql(dbConn, tuQuery, dbParamInfos, False)
						            
'						            'Si quieres mostrar un mensaje de éxito (opcional)
'						            'selectionChangedTaskResult.ShowMessageBox = True
'						            'selectionChangedTaskResult.Message = "Operación completada exitosamente."
						            
'						        Catch ex As Exception
'						            dbConn.RollbackTrans()
'						            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'						        End Try
'						        dbConn.CommitTrans()
'						    End Using
						
						#End Region 
						 						
						End If
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						
						#Region "Save Project Cash"
						
						If args.FunctionName.XFEqualsIgnoreCase("SaveProjectCash") Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
							
							'Throw error if no company was selected
							If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							'Create a datatable from modified rows to insert in cash table
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)
							
							'Declare column mapping dict
							Dim columnMappingDict As New Dictionary(Of String, String) From {
								{"project_id", "project_id"},
								{"scenario", "scenario"},
								{"year", "year"},
								{"cash_m1", "cash_m1"},
								{"cash_m2", "cash_m2"},
								{"cash_m3", "cash_m3"},
								{"cash_m4", "cash_m4"},
								{"cash_m5", "cash_m5"},
								{"cash_m6", "cash_m6"},
								{"cash_m7", "cash_m7"},
								{"cash_m8", "cash_m8"},
								{"cash_m9", "cash_m9"},
								{"cash_m10", "cash_m10"},
								{"cash_m11", "cash_m11"},
								{"cash_m12", "cash_m12"}
							}
							
							'Map and filter columns in DataTable
							dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
							
							'Save rows in aux project cash table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_INV_AUX_project_cash", dt, "replace")
							
							'Save rows in project cash table depending on scenario
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								Try
									Dim mergeQuery As String
									
									If parameterDict("paramScenario") = "Actual" Then
										'For actual only update cash
										mergeQuery = $"
											MERGE INTO XFC_INV_FACT_project_cash AS target
											USING (
												SELECT 
													project_id,
													scenario,
													year,
													CASE month
												        WHEN 'cash_m1' THEN 1
												        WHEN 'cash_m2' THEN 2
												        WHEN 'cash_m3' THEN 3
												        WHEN 'cash_m4' THEN 4
												        WHEN 'cash_m5' THEN 5
												        WHEN 'cash_m6' THEN 6
												        WHEN 'cash_m7' THEN 7
												        WHEN 'cash_m8' THEN 8
												        WHEN 'cash_m9' THEN 9
												        WHEN 'cash_m10' THEN 10
												        WHEN 'cash_m11' THEN 11
												        WHEN 'cash_m12' THEN 12
												        ELSE month
											       	END AS month,
											       	amount
												FROM 
												(
													SELECT project_id, scenario, year, cash_m1, cash_m2, cash_m3, cash_m4, cash_m5, cash_m6, cash_m7, cash_m8, cash_m9, cash_m10, cash_m11, cash_m12
												    FROM XFC_INV_AUX_project_cash
												) AS SourceTable
												UNPIVOT
												(
												    amount FOR month IN (cash_m1, cash_m2, cash_m3, cash_m4, cash_m5, cash_m6, cash_m7, cash_m8, cash_m9, cash_m10, cash_m11, cash_m12)
												) AS UnpivotedTable
											) AS source
											ON target.project_id = source.project_id
											AND target.scenario = source.scenario
											AND target.year = source.year
											AND target.month = source.month
											WHEN MATCHED THEN
											    UPDATE SET
											        amount = source.amount
											WHEN NOT MATCHED THEN
											    INSERT (
											        project_id, scenario, year, month, amount
											    )
											    VALUES (
											        source.project_id, source.scenario, source.year, source.month, source.amount
											    );
										"
									Else
										'For planning, insert modified months and recalculate projections per project
										mergeQuery = $"
											-- Create a temporary table to store the initial cash data
											CREATE TABLE #InitialData (
											    project_id VARCHAR(255),
											    month INT,
											    amount DECIMAL(18,2)
											);
											
											INSERT INTO #InitialData (project_id, month, amount)
											SELECT pc.project_id, pc.month, pc.amount
											FROM XFC_INV_FACT_project_cash pc
											WHERE pc.year = @paramYear AND pc.scenario = @paramScenario;
										
											-- Create a temporary table to store the unpivoted data
											CREATE TABLE #UnpivotedData (
											    project_id VARCHAR(255),
											    month INT,
											    amount DECIMAL(18,2)
											);
											
											-- Populate the temporary table
											INSERT INTO #UnpivotedData (project_id, month, amount)
											SELECT
											    project_id,
											    CASE month
											        WHEN 'cash_m1' THEN 1
											        WHEN 'cash_m2' THEN 2
											        WHEN 'cash_m3' THEN 3
											        WHEN 'cash_m4' THEN 4
											        WHEN 'cash_m5' THEN 5
											        WHEN 'cash_m6' THEN 6
											        WHEN 'cash_m7' THEN 7
											        WHEN 'cash_m8' THEN 8
											        WHEN 'cash_m9' THEN 9
											        WHEN 'cash_m10' THEN 10
											        WHEN 'cash_m11' THEN 11
											        WHEN 'cash_m12' THEN 12
											    END AS month,
											    amount
											FROM 
											(
											    SELECT project_id, cash_m1, cash_m2, cash_m3, cash_m4, cash_m5, cash_m6, cash_m7, cash_m8, cash_m9, cash_m10, cash_m11, cash_m12
											    FROM XFC_INV_AUX_project_cash
											) AS SourceTable
											UNPIVOT
											(
											    amount FOR month IN (cash_m1, cash_m2, cash_m3, cash_m4, cash_m5, cash_m6, cash_m7, cash_m8, cash_m9, cash_m10, cash_m11, cash_m12)
											) AS UnpivotedTable
											-- Filter out actual months
											WHERE REPLACE(month, 'cash_m', '') > @paramForecastMonth;
											
											DECLARE @project VARCHAR(255);
											DECLARE @month INTEGER;
											DECLARE @nextMonth INTEGER;
											DECLARE @amount DEC(18, 2);
									        DECLARE @lastRF VARCHAR(255);
											
											-- Get last RF if param forecast month is 0
											IF @paramForecastMonth = 0
											BEGIN
												SELECT @lastRF = scenario
												FROM (
													SELECT TOP(1) scenario
													FROM XFC_INV_FACT_project_cash pc
													LEFT JOIN XFC_INV_MASTER_project p ON pc.project_id = p.project
													WHERE
														year = @paramYear - 1
														AND scenario LIKE 'RF%'
														AND p.company_id = @paramCompany
													ORDER BY CAST(REPLACE(LEFT(scenario, 4), 'RF', '') AS INTEGER) DESC
												) AS subquery;
												IF @lastRF IS NULL
												BEGIN
		    										RAISERROR('No matching RF scenario found for the previous year.', 16, 1);
													RETURN;
												END
											END;
										
											-- Loop through all the modified projects
											DECLARE project_cursor CURSOR FOR
											SELECT DISTINCT project_id FROM #UnpivotedData;
											
											OPEN project_cursor;
											FETCH NEXT FROM project_cursor INTO @project;
											
											WHILE @@FETCH_STATUS = 0
											BEGIN
											    -- Loop through all the edited months for this project
											    DECLARE month_cursor CURSOR FOR
												SELECT ud.month, ud.amount
												FROM #UnpivotedData ud
												LEFT JOIN #InitialData id ON
												    id.project_id = ud.project_id
												    AND id.month = ud.month
												WHERE
												    ud.project_id = @project
												    AND (
														ud.amount <> id.amount
														OR id.amount IS NULL
													);
										
												-- Insert project into modified projections table
												MERGE INTO XFC_INV_AUX_modified_projection AS target
												USING (VALUES ('cash', @paramYear, @paramScenario, @project)) 
												      AS source (type, year, scenario, project_id)
												ON (target.type = source.type 
												    AND target.year = source.year 
												    AND target.scenario = source.scenario 
												    AND target.project_id = source.project_id)
												WHEN NOT MATCHED THEN
												    INSERT (type, year, scenario, project_id)
												    VALUES (source.type, source.year, source.scenario, source.project_id);
											
											    OPEN month_cursor;
											    FETCH NEXT FROM month_cursor INTO @month, @amount;
											
											    WHILE @@FETCH_STATUS = 0
											    BEGIN
											        -- Process each month for the current project
													-- Update current month amount
													MERGE INTO XFC_INV_FACT_project_cash AS target
													USING (VALUES (@project, @paramScenario, @paramYear, @month, @amount)) 
													      AS source (project_id, scenario, year, month, amount)
													ON (target.project_id = source.project_id 
													    AND target.scenario = source.scenario 
													    AND target.year = source.year 
													    AND target.month = source.month)
													WHEN MATCHED THEN
														UPDATE SET
															amount = source.amount
													WHEN NOT MATCHED THEN
													    INSERT (project_id, scenario, year, month, amount)
													    VALUES (source.project_id, source.scenario, source.year, source.month, source.amount);
										
													-- Update month to the next month
													SET @nextMonth = @month + 1;

													-- Project next month if month is less than 13
													IF @nextMonth < 13
													BEGIN
													WITH projects_months_cte AS (
														SELECT
															p.*,
															@paramYear AS year,
															@nextMonth AS month,
															CASE
																WHEN LOWER(REPLACE(p.poi_poe, '.', '')) = 'poi' THEN p.total_requirement * pcsp.poi_percentage
																WHEN LOWER(REPLACE(p.poi_poe, '.', '')) = 'poe' THEN p.total_requirement * pcsp.poe_percentage
																ELSE 0
															END AS calculated_cash_ytd
													    FROM (
															SELECT project, total_requirement, poi_poe, ma_date
															FROM XFC_INV_MASTER_project
															WHERE project = @project
														) p
														INNER JOIN XFC_INV_MASTER_project_cash_sop_percentage pcsp
															ON DATEDIFF(MONTH, p.ma_date, DATEFROMPARTS(@paramYear, @nextMonth, 1)) = pcsp.sop_month_difference
													)
												
													MERGE INTO XFC_INV_FACT_project_cash AS target
													USING (
														SELECT
															pm.project AS project_id,
															@paramScenario AS scenario,
															pm.year AS year,
															pm.month AS month,
															GREATEST(
																pm.calculated_cash_ytd - COALESCE(pcpy.total_actual_amount, 0),
																0
															) AS amount
														FROM projects_months_cte pm
												
														-- Get accumulated actual cash from prev months
														LEFT JOIN (
															SELECT
																project_id,
																SUM(amount) AS total_actual_amount
															FROM XFC_INV_FACT_project_cash
															WHERE
																(
																	@paramForecastMonth <> 0
																	AND (
																		(
																			year < @paramYear
																			AND scenario = 'Actual'
																		) OR (
																			year = @paramYear
																			AND month < @nextMonth
																			AND scenario = @paramScenario
																		)
																	)
																) OR (
																	@paramForecastMonth = 0
																	AND (
																		(
																			year < @paramYear - 1
																			AND scenario = 'Actual'
																		) OR (
																			year = @paramYear - 1
																			AND scenario = @lastRF
																		) OR (
																			year = @paramYear
																			AND month < @nextMonth
																			AND scenario = @paramScenario
																		)
																	)
																)
															GROUP BY project_id
														) pcpy ON
															pm.project = pcpy.project_id
													) AS source
													ON target.project_id = source.project_id
													AND target.scenario = source.scenario
													AND target.year = source.year
													AND target.month = source.month
													WHEN MATCHED THEN
													    UPDATE SET
													        amount = source.amount
													WHEN NOT MATCHED THEN
													    INSERT (
													        project_id, scenario, year, month, amount
													    )
													    VALUES (
													        source.project_id, source.scenario, source.year, source.month, source.amount
													    );
													END
										
											        FETCH NEXT FROM month_cursor INTO @month, @amount;
											    END
											
											    CLOSE month_cursor;
											    DEALLOCATE month_cursor;
											
											    FETCH NEXT FROM project_cursor INTO @project;
											END
											
											CLOSE project_cursor;
											DEALLOCATE project_cursor;
											
											-- Clean up
											DROP TABLE #UnpivotedData;
											DROP TABLE #InitialData;
										"
									End If
									BRApi.Database.ExecuteSql(dbConn, mergeQuery, dbParamInfos, False)
								Catch ex As Exception
									dbConn.RollbackTrans()
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
								dbConn.CommitTrans()
							End Using
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.CancelDefaultSave = True
							Return saveDataTaskResult
							
							#End Region
							
						#Region "Save Project Cash GAP with Comments"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveProjectCashGap") Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							'Create a datatable from modified rows to insert in cash table
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)
							
							'Declare column mapping dict
							Dim columnMappingDict As New Dictionary(Of String, String) From {
						        {"company_id", "company_id"},
						        {"year", "year"},
						        {"type", "type"},
						        {"scenario", "scenario"},
						        {"month", "month"},
						        {$"comment_{parameterDict("paramScenarioMain")}", $"comment_{parameterDict("paramScenarioMain")}"}
							}
							
							'Map and filter columns in DataTable
							dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
							
							'Save rows in raw project cash table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_INV_RAW_comment_gap", dt, "replace")
						
						    'Merge raw into aux main table
						    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						        dbConn.BeginTrans()
						        Try
						            Dim mergeQuery As String = $"
										MERGE INTO XFC_INV_AUX_comment_gap AS target
										USING (
										    SELECT 
										        company_id,
										        year,
										        type,
										        scenario,
										        month,
										        comment_{parameterDict("paramScenarioMain")}
										    FROM XFC_INV_RAW_comment_gap
										) AS source
										ON 
										    target.company_id = source.company_id
										    AND target.year    = source.year
										    AND target.type    = source.type
										    AND target.scenario= source.scenario
										    AND target.month   = source.month
	
										WHEN MATCHED THEN 
										    UPDATE SET 
										        target.comment_{parameterDict("paramScenarioMain")} = source.comment_{parameterDict("paramScenarioMain")}
										
										WHEN NOT MATCHED THEN 
										    INSERT (company_id, year, type, scenario, month, comment_{parameterDict("paramScenarioMain")})
										    VALUES (source.company_id, source.year, source.type, source.scenario, source.month, source.comment_{parameterDict("paramScenarioMain")});

						            "
									BRApi.ErrorLog.logmessage(si, "query", mergeQuery)
						            BRApi.Database.ExecuteSql(dbConn, mergeQuery, dbParamInfos, False)
						        Catch ex As Exception
						            dbConn.RollbackTrans()
						            Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						        End Try
						
						        dbConn.CommitTrans()
						    End Using
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.CancelDefaultSave = True
							Return saveDataTaskResult
						
						
						#End Region
						
						#Region "Save Project Capitalization"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("SaveProjectCapitalization") Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							dbParamInfos.Add(New DbParamInfo("paramForecastMonth", paramForecastMonth))
							
							'Throw error if no company was selected
							If parameterDict("paramCompany") = 0 Then Throw ErrorHandler.LogWrite(si, New XFException("You must select a Company"))
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							'Create a datatable from modified rows to insert in cash table
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)
							
							'Declare column mapping dict
							Dim columnMappingDict As New Dictionary(Of String, String) From {
								{"project_id", "project_id"},
								{"scenario", "scenario"},
								{"year", "year"},
								{"amount_m1", "amount_m1"},
								{"amount_m2", "amount_m2"},
								{"amount_m3", "amount_m3"},
								{"amount_m4", "amount_m4"},
								{"amount_m5", "amount_m5"},
								{"amount_m6", "amount_m6"},
								{"amount_m7", "amount_m7"},
								{"amount_m8", "amount_m8"},
								{"amount_m9", "amount_m9"},
								{"amount_m10", "amount_m10"},
								{"amount_m11", "amount_m11"},
								{"amount_m12", "amount_m12"}
							}
							
							'Map and filter columns in DataTable
							dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
							
							'Save rows in aux project cash table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_INV_AUX_project_capitalization", dt, "replace")
							
							'Save rows in project cash table depending on scenario
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								Try
									Dim mergeQuery As String =  $"
										-- Insert projects into modified projections table
										MERGE INTO XFC_INV_AUX_modified_projection AS target
										USING (
											SELECT DISTINCT
												'capitalization' AS type,
												year,
												scenario,
												project_id
											FROM XFC_INV_AUX_project_capitalization
										) AS source (type, year, scenario, project_id)
										ON target.type = source.type 
										    AND target.year = source.year 
										    AND target.scenario = source.scenario 
										    AND target.project_id = source.project_id
										WHEN NOT MATCHED THEN
										    INSERT (type, year, scenario, project_id)
										    VALUES (source.type, source.year, source.scenario, source.project_id);
									
										MERGE INTO XFC_INV_FACT_project_capitalization AS target
										USING (
											SELECT 
												project_id,
												scenario,
												year,
												CASE month
											        WHEN 'amount_m1' THEN 1
											        WHEN 'amount_m2' THEN 2
											        WHEN 'amount_m3' THEN 3
											        WHEN 'amount_m4' THEN 4
											        WHEN 'amount_m5' THEN 5
											        WHEN 'amount_m6' THEN 6
											        WHEN 'amount_m7' THEN 7
											        WHEN 'amount_m8' THEN 8
											        WHEN 'amount_m9' THEN 9
											        WHEN 'amount_m10' THEN 10
											        WHEN 'amount_m11' THEN 11
											        WHEN 'amount_m12' THEN 12
											        ELSE month
										       	END AS month,
										       	amount
											FROM 
											(
												SELECT project_id, scenario, year, amount_m1, amount_m2, amount_m3, amount_m4, amount_m5, amount_m6, amount_m7, amount_m8, amount_m9, amount_m10, amount_m11, amount_m12
											    FROM XFC_INV_AUX_project_capitalization
											) AS SourceTable
											UNPIVOT
											(
											    amount FOR month IN (amount_m1, amount_m2, amount_m3, amount_m4, amount_m5, amount_m6, amount_m7, amount_m8, amount_m9, amount_m10, amount_m11, amount_m12)
											) AS UnpivotedTable
										) AS source
										ON target.project_id = source.project_id
										AND target.scenario = source.scenario
										AND target.year = source.year
										AND target.month = source.month
										WHEN MATCHED THEN
										    UPDATE SET
										        amount = source.amount
										WHEN NOT MATCHED THEN
										    INSERT (
										        project_id, scenario, year, month, amount
										    )
										    VALUES (
										        source.project_id, source.scenario, source.year, source.month, source.amount
										    );
									"
									BRApi.Database.ExecuteSql(dbConn, mergeQuery, dbParamInfos, False)
								Catch ex As Exception
									dbConn.RollbackTrans()
									Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
								End Try
								dbConn.CommitTrans()
							End Using
							
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.CancelDefaultSave = True
							Return saveDataTaskResult
							
							#End Region
											
							
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
