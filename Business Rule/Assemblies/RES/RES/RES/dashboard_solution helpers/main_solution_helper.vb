Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Diagnostics
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_solution_helper
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		'Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
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
						
						#Region "Download Excel Template"
						
						If args.FunctionName.XFEqualsIgnoreCase("DownloadExcelTemplate") Then
							' Get parameters
							Dim type As String = args.NameValuePairs.XFGetValue("p_type")
			                Dim entity As String = args.NameValuePairs.XFGetValue("p_entity")
			                Dim refScenario As String = args.NameValuePairs.XFGetValue("p_reference_scenario")
			                Dim scenario As String = args.NameValuePairs.XFGetValue("p_scenario")
			                Dim year As Integer = CInt(args.NameValuePairs.XFGetValue("p_year"))
							Dim technology As String = args.NameValuePairs.XFGetValue("p_technology") ' Puede ser All, AM%, OM%, Other (engloba el resto)
							Dim department As String = args.NameValuePairs.XFGetValue("p_department") ' Puede ser All, AM%, OM%, Other (engloba el resto)
			
			                Me.GenerateExcelTemplate(si, type, entity, scenario, year, technology, department, refScenario)
							
						#End Region
						
						#Region "Copy Scenario Figures"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("CopyScenarioFigures") Then
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = True
							selectionChangedTaskResult.Message = "Scenario copied successfully."
							
							' Get parameters
			                Dim scenarioSource As String = args.NameValuePairs("p_scenario_source")
			                Dim scenarioTarget As String = args.NameValuePairs("p_scenario_target")
			                Dim yearTarget As Integer = CInt(args.NameValuePairs("p_year_target"))
			                Dim yearSource As Integer = CInt(args.NameValuePairs("p_year_source"))
							
							Me.CopyScenarioFigures(si, scenarioSource, scenarioTarget, yearTarget, yearSource)
							
							Return selectionChangedTaskResult
						
						#End Region
						
						#Region "Confirm Year Scenario"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("ConfirmYearScenario") Then
							'Get parameters
							Dim paramQueryParams As String = args.NameValuePairs("p_query_params")
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, paramQueryParams)
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, paramQueryParams)
							
							' Update year scenario confirm table
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
									dbConn,
									"
										MERGE INTO XFC_RES_AUX_year_scenario_confirm AS target
										USING (
											SELECT
												@paramYear AS year,
												@paramScenario AS scenario,
												@paramIsConfirmed AS is_confirmed
										) AS source
										ON target.year = source.year
											AND target.scenario = source.scenario
										WHEN MATCHED THEN
											UPDATE SET
												is_confirmed = @paramIsConfirmed
										WHEN NOT MATCHED THEN
											INSERT (year, scenario, is_confirmed)
											VALUES (source.year, source.scenario, source.is_confirmed);
									",
									dbParamInfos,
									False
								)
							End Using
						
						#End Region
						
						#Region "Download All Contracts"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadAllContracts") Then
							Dim entity As String = args.NameValuePairs.XFGetValue("p_entity")
							Dim year As Integer = CInt(args.NameValuePairs.XFGetValue("p_year"))
							Me.GenerateExcelTemplate(si, "contracts_all", "", "", year, "", "")
						
						#End Region
						
						End If
					
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
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
		#Region "Main Functions"
		
		#Region "Generate Excel Template"
		
		''' <summary>
		''' Generates planning excel template for the entity, scenario and year.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="type">Type of planning template: project, structure or contracts</param>
		''' <param name="entity">Entity</param>
		''' <param name="scenario">Scenario</param>
		''' <param name="year">Year</param>
		''' <param name="technology">Technology (For Project Planning)</param>
		''' <param name="department">Department (For Structure Planning)</param>
		''' <param name="refScenario">Optional: Reference Scenario
		''' Default: ""</param>
		''' <param name="sourceFolderPath">Optional: Source folder path where the source file is located.
		''' Default: "Documents/Public/Services/Templates"</param>
		''' <param name="sourceFileName">Optional: Source file name
		''' Default: "" (based on type)</param>
		''' <param name="targetFolderPath">Optional: Target folder path where the new file will be created
		''' Default: "Documents/Public/Services/Templates/Temporal"</param>
        Public Sub GenerateExcelTemplate(
			ByVal si As SessionInfo, ByVal type As String,
			ByVal entity As String, ByVal scenario As String,
			ByVal year As Integer, ByVal technology As String,
			ByVal department As String,
			Optional refScenario As String = "",
			Optional sourceFolderPath As String = "Documents/Public/Services/Templates",
			Optional sourceFileName As String = "",
			Optional targetFolderPath As String = "Documents/Public/Services/Templates/Temporal"
		)
			' Create a Dictionary for type of templates and it's template name
			Dim templateTypes As New Dictionary(Of String, String) From {
				{"project", "Project_Planning_Template.xlsx"},
				{"structure", "Structure_Planning_Template.xlsx"},
				{"contracts", "Master_Of_Contracts_Template.xlsx"},
				{"contracts_all", "Master_Of_Contracts_Template.xlsx"}
			}
		
			' Validate inputs
			If Not templateTypes.ContainsKey(type) Then Throw New Exception("Error generating planning template: Planning type must be 'project', 'structure' or 'contracts'")
		
			' Set initial parameters
			Dim firstForecastedMonth As Integer = If(scenario.ToLower.Contains("fct"), CInt(Right(scenario, 2)) + 1, 13)
			If sourceFileName = "" Then
				sourceFileName = templateTypes(type)
			End If
			
			' Define the last identifier of the file name depending on type
			Dim technologyOrDepartment As String = If(type = "structure", department, technology)
            ' Set source file path and target file name and path
            Dim sourceFileFullName As String = $"{sourceFolderPath}/{sourceFileName}"
			Dim targetFileName As String
			Dim targetEntity As String
			If type = "contracts_all" Then
				targetEntity = "ALL"
				targetFileName = $"{targetEntity}_{year}_{sourceFileName}"
			Else
				targetEntity = entity
				targetFileName = $"{entity}_{year}"
				' Generate file name based on parameters
				targetFileName += If(String.IsNullOrEmpty(scenario), "", $"_{scenario}")
				targetFileName += $"_{technologyOrDepartment}_{sourceFileName}"
			End If
           	Dim targetFileFullName As String = $"{targetFolderPath}/Temporal_{targetEntity}/{targetFileName}"

			' Check that the source file exists
            If Not brapi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, sourceFileFullName) Then
                Throw New Exception($"Error generating planning template: File '{sourceFileName}' was not found in the Path '{sourceFolderPath}'")
            End If

			' Get data table to populate excel template
            Dim dt As New DataTable
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				' Set db param infos
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramEntity", entity),
					New DbParamInfo("paramTechnology", technology),
					New DbParamInfo("paramDepartment", department),
					New DbParamInfo("paramYear", year),
					New DbParamInfo("paramLastYear", year - 1),
					New DbParamInfo("paramNextYear", year + 1),
					New DbParamInfo("paramP1Year", year + 2),
					New DbParamInfo("paramP2Year", year + 3),
					New DbParamInfo("paramP3Year", year + 4),
					New DbParamInfo("paramActualYear", year),
					New DbParamInfo("paramScenario", scenario),
					New DbParamInfo("paramRefScenario", refScenario),
					New DbParamInfo("paramActualScenario", If(scenario = "BUD", refScenario, scenario)),
					New DbParamInfo("paramFirstForecastedMonth", firstForecastedMonth)
				}
				' Check if year is confirmed (skip for contracts_all, no year context)
				If type <> "contracts_all" Then
					Dim isConfirmedDt As DataTable = BRApi.Database.ExecuteSql(
						dbConn,
						"
							SELECT is_confirmed
							FROM XFC_RES_AUX_year_scenario_confirm
							WHERE (
									(@paramScenario = 'BUD' AND year = @paramNextYear)
									OR (@paramScenario <> 'BUD' AND year = @paramYear)
								)
								AND scenario = @paramScenario
								AND is_confirmed = 1
						",
						dbParamInfos,
						False
					)
					If isConfirmedDt IsNot Nothing AndAlso isConfirmedDt.Rows.Count > 0 Then
						Throw New Exception("Could not generate template. Year is confirmed.")
					End If
				End If
				
				dt = BRApi.Database.ExecuteSql(
				    dbConn,
					Me.GetPlanningTemplateQuery(si, type),
				    dbParamInfos,
				    False
				)
				
			End Using

			' Check that dt has values
'            If dt Is Nothing OrElse dt.Rows.Count = 0 Then
'                Throw New Exception("Error generating planning template: No data available")
'            End If
			
			' Check that dt has values
			' Project planning template depends on updated Master Of Contracts data.
			' If no data is found, generation is blocked and user is prompted to review Master of Contracts.
			If (dt Is Nothing OrElse dt.Rows.Count = 0) AndAlso type.ToLower() = "project" Then 
			    Throw New Exception("Error generating planning template: No data available. Please check the Master of Contracts and update it if necessary.")
			End If


			' Get excel template
            Dim fileBytes As Byte() = brapi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, sourceFileFullName, True, True).XFFile.ContentFileBytes
            If fileBytes Is Nothing OrElse fileBytes.Length = 0 Then Throw New Exception("Error generating planning template: Template file empty")
            Using memoryStream As New MemoryStream()
			    ' Escribir los bytes de la plantilla en el stream expandible
			    memoryStream.Write(fileBytes, 0, fileBytes.Length)
				memoryStream.Position = 0
				
                Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Open(memoryStream, True)
					' Get excel "XLSX Import Template" sheet
                    Dim workbookPart As WorkbookPart = spreadsheetDocument.WorkbookPart
                    Dim sheet As Sheet = workbookPart.Workbook.Sheets.Elements(Of Sheet)().FirstOrDefault(Function(s) s.Name.Value = "XLSX Import Template")
                    If sheet Is Nothing Then
                        Throw New Exception("Error generating planning template: Sheet 'XLSX Import Template' not found.")
                    End If
                    Dim worksheetPart As WorksheetPart = CType(workbookPart.GetPartById(sheet.Id.Value), WorksheetPart)
                    Dim templateWorksheet As Worksheet = worksheetPart.Worksheet
					
					' Clear data for the template
					UTISharedFunctionsBR.ClearWorksheetData(templateWorksheet, 11)
					' Set initial data based on the query
					UTISharedFunctionsBR.PopulateWorksheet(templateWorksheet, dt, 11, 1)
					' Hide columns depending on scenario and type
					Dim startHiddenColumn As Integer = If(type = "project", 29, 23)
					Dim endHiddenColumn As Integer = If(type = "project", 65, 58)
					If scenario.ToLower.Contains("fct") Then UTISharedFunctionsBR.HideColumns(templateWorksheet, startHiddenColumn, endHiddenColumn)
					
                    worksheetPart.Worksheet.Save()
                    workbookPart.Workbook.Save()
                End Using
				
				' Save file
			    Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetFileFullName)
			    Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, memoryStream.ToArray())
			    fileFile.FileInfo.ContentFileExtension = "xlsx"
			    BRApi.FileSystem.InsertOrUpdateFile(si, fileFile)
            End Using
        End Sub
		
		#End Region
		
		#Region "Copy Scenario Figures"
		
		''' <summary>
		''' Copy scenario figures for work order and structure fact tables from a source to a target scenario.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="scenarioSource">Source scenario</param>
		''' <param name="scenarioTarget">Target scenario</param>
		''' <param name="yearTarget">Target year</param>
		Private Sub CopyScenarioFigures(si As SessionInfo, scenarioSource As String, scenarioTarget As String, yearTarget As Integer, yearSource As Integer)
		    ' Determine the last month to include, based on scenario logic
			Dim lastMonth As Integer = If(scenarioTarget <> "BUD" AndAlso scenarioSource = "ACT" AndAlso Left(scenarioTarget, 3) = "FCT", CInt(Right(scenarioTarget, 2)), 12)
		
		    Dim cubeTimeList As String = String.Empty
		    For i As Integer = 1 To lastMonth
		        cubeTimeList &= If(String.IsNullOrEmpty(cubeTimeList), $"{yearTarget}M{i}", $",{yearTarget}M{i}")
		    Next
			
			' Define substitution variables for the BR data management sequence
		    Dim customSubstVars As New Dictionary(Of String, String) From {
		        {"p_scenario_source", scenarioSource},
		        {"p_year_source", yearSource.ToString()},
		        {"p_scenario_target", scenarioTarget},
		        {"p_time_list", cubeTimeList}
		    }
			
			' Trigger BR data management sequence to copy cube data
		    BRApi.Utilities.QueueDataMgmtSequence(
		        si,
		        BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "RES"),
		        "RES_CopyScenarioCube", customSubstVars)
		
		    Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		        Dim dbParamInfos As New List(Of DbParamInfo) From {
		            New DbParamInfo("paramScenarioSource", scenarioSource),
		            New DbParamInfo("paramScenarioTarget", scenarioTarget),
		            New DbParamInfo("paramYearSource", yearSource),
		            New DbParamInfo("paramYearTarget", yearTarget),
		            New DbParamInfo("paramLastMonth", lastMonth)
		        }
				
				' Check if target year and scenario are confirmed and locked
		        Dim isConfirmedQuery As String = $"
		            SELECT is_confirmed 
		            FROM XFC_RES_AUX_year_scenario_confirm 
		            WHERE year = @paramYearTarget 
		            AND scenario = @paramScenarioTarget 
		            AND is_confirmed = 1"
		
		        Dim isConfirmedDt As DataTable = BRApi.Database.ExecuteSql(dbConn, isConfirmedQuery, dbParamInfos, False)
		        If isConfirmedDt IsNot Nothing AndAlso isConfirmedDt.Rows.Count > 0 Then
		            Throw New Exception("Cannot copy Scenarios: Year is confirmed.")
		        End If
				
				' Prevent copying to self
		        If scenarioSource = scenarioTarget AndAlso yearSource = yearTarget Then
		            Throw New Exception("Cannot copy Scenarios: Source and Target Scenario/Year are the same.")
		        End If
				
				' Determine if monthly expansion logic should apply
		        Dim useMonthExpansion As Boolean = (scenarioSource = "PLN3")
		        Dim workOrderTargetMonth As String = If(scenarioTarget = "PLN3", "12 AS month", If(useMonthExpansion, "m.month", "month"))
		        Dim structureTargetMonth As String = workOrderTargetMonth
			
        		' Distribute amount evenly across 12 months with rounding to 2 decimals.
				' Adjust the last month's value to absorb any rounding differences, ensuring total remains accurate.				
				Dim workOrderAmountExpr As String = If(useMonthExpansion, "CASE WHEN m.month < 12 THEN ROUND(amount / 12.0, 2) ELSE amount - ROUND(amount / 12.0, 2) * 11 END", "amount")
		        Dim structureAmountExpr As String = workOrderAmountExpr
				
				Dim workOrderDeleteCondition As String = If(scenarioSource = "ACT","AND account NOT LIKE 'OE_%'","")

				' Delete existing data for the target scenario and year (up to lastMonth)
				Dim deleteQuery As String = $"
					DELETE FROM XFC_RES_FACT_work_order_figures 
					WHERE scenario = @paramScenarioTarget 
					AND year = @paramYearTarget 
					AND month <= @paramLastMonth
					{workOrderDeleteCondition};

					DELETE FROM XFC_RES_FACT_structure_figures 
					WHERE scenario = @paramScenarioTarget 
					AND year = @paramYearTarget 
					AND month <= @paramLastMonth;"
				
				' Insert work order figures with or without monthly expansion
		        Dim woInsertQuery As String
		        If useMonthExpansion Then
					' Expand data across months with corrected distribution
		            woInsertQuery = $"
		                WITH months AS (
		                    SELECT 1 AS month
		                    UNION ALL
		                    SELECT month + 1 FROM months WHERE month < 12
		                ), 
		                monthly_wo_figures AS (
		                    SELECT 
		                        entity, 
		                        wo_id, 
		                        m.month AS month, 
		                        account, 
		                        intercompany, 
		                        ud3, 
		                        {workOrderAmountExpr} AS amount
		                    FROM XFC_RES_FACT_work_order_figures sf 
		                    CROSS JOIN months m
		                    WHERE sf.scenario = @paramScenarioSource 
		                    AND sf.year = @paramYearSource 
		                    AND sf.month = 12 
		                    AND sf.amount <> 0
		                )
		                INSERT INTO XFC_RES_FACT_work_order_figures (
		                    entity, 
		                    wo_id, 
		                    scenario, 
		                    year, 
		                    month, 
		                    account, 
		                    intercompany, 
		                    ud3, 
		                    amount
		                )
		                SELECT 
		                    entity, 
		                    wo_id, 
		                    @paramScenarioTarget AS scenario, 
		                    @paramYearTarget AS year, 
		                    month, 
		                    account, 
		                    intercompany, 
		                    ud3, 
		                    SUM(amount) AS amount
		                FROM monthly_wo_figures
		                GROUP BY 
		                    entity, 
		                    wo_id, 
		                    month, 
		                    account, 
		                    intercompany, 
		                    ud3;"
		        Else
		            ' Direct copy without month expansion
					woInsertQuery = $"
		                INSERT INTO XFC_RES_FACT_work_order_figures (
		                    entity, 
		                    wo_id, 
		                    scenario, 
		                    year, 
		                    month, 
		                    account, 
		                    intercompany, 
		                    ud3, 
		                    amount
		                )
		                SELECT 
		                    entity, 
		                    wo_id, 
		                    @paramScenarioTarget AS scenario, 
		                    @paramYearTarget AS year, 
		                    {workOrderTargetMonth}, 
		                    account,
		                    intercompany, 
		                    ud3, 
		                    SUM({workOrderAmountExpr}) AS amount
		                FROM XFC_RES_FACT_work_order_figures
		                WHERE scenario = @paramScenarioSource 
		                AND year = @paramYearSource 
		                AND month <= @paramLastMonth
		                GROUP BY 
		                    entity, 
		                    wo_id, 
		                    {workOrderTargetMonth}, 
		                    account, 
		                    intercompany, 
		                    ud3;"
		        End If
				' Insert structure figures with or without monthly expansion
		        Dim structureInsertQuery As String
		        If useMonthExpansion Then
		            structureInsertQuery = $"
		                WITH months AS (
		                    SELECT 1 AS month
		                    UNION ALL
		                    SELECT month + 1 FROM months WHERE month < 12
		                ), 
		                monthly_structure_figures AS (
		                    SELECT 
		                        entity, 
		                        ud3, 
		                        m.month AS month, 
		                        account, 
		                        intercompany, 
		                        ud1, 
		                        {structureAmountExpr} AS amount
		                    FROM XFC_RES_FACT_structure_figures sf 
		                    CROSS JOIN months m
		                    WHERE sf.scenario = @paramScenarioSource 
		                    AND sf.year = @paramYearSource 
		                    AND sf.month = 12 
		                    AND sf.amount <> 0
		                )
		                INSERT INTO XFC_RES_FACT_structure_figures (
		                    entity, 
		                    ud3, 
		                    scenario, 
		                    year, 
		                    month, 
		                    account, 
		                    intercompany, 
		                    ud1, 
		                    amount
		                )
		                SELECT 
		                    entity, 
		                    ud3, 
		                    @paramScenarioTarget AS scenario, 
		                    @paramYearTarget AS year, 
		                    month,
		                    account, 
		                    intercompany, 
		                    ud1, 
		                    SUM(amount) AS amount
		                FROM monthly_structure_figures
		                GROUP BY 
		                    entity, 
		                    ud3, 
		                    month, 
		                    account, 
		                    intercompany, 
		                    ud1;"
		        Else
		            structureInsertQuery = $"
		                INSERT INTO XFC_RES_FACT_structure_figures (
		                    entity, 
		                    ud3, 
		                    scenario, 
		                    year, 
		                    month, 
		                    account, 
		                    intercompany, 
		                    ud1, 
		                    amount
		                )
		                SELECT 
		                    entity, 
		                    ud3, 
		                    @paramScenarioTarget AS scenario, 
		                    @paramYearTarget AS year, 
		                    {structureTargetMonth},
		                    account, 
		                    intercompany, 
		                    ud1, 
		                    SUM({structureAmountExpr}) AS amount
		                FROM XFC_RES_FACT_structure_figures
		                WHERE scenario = @paramScenarioSource 
		                AND year = @paramYearSource 
		                AND month <= @paramLastMonth
		                GROUP BY 
		                    entity, 
		                    ud3, 
		                    {structureTargetMonth}, 
		                    account, 
		                    intercompany, 
		                    ud1;"
		        End If
				' Execute all SQL statements
		        BRApi.Database.ExecuteSql(dbConn, deleteQuery, dbParamInfos, False)
		        BRApi.Database.ExecuteSql(dbConn, woInsertQuery, dbParamInfos, False)
		        BRApi.Database.ExecuteSql(dbConn, structureInsertQuery, dbParamInfos, False)
		    End Using
		End Sub

		#End Region
		
		#End Region
		
		#Region "Helper Functions"
		
		#Region "Get Planning Template Query"
		
		''' <summary>
		''' Returns a sql query to generate a planning template.
		''' </summary>
		''' <param name="si">Session info</param>
		''' <param name="type">Defines the planning template type: project or structure</param>
		Private Function GetPlanningTemplateQuery(si As SessionInfo, type As String) As String
			Select type
				
				#Region "Project"

				Case "project"
					Return "
					-- Get last year's last forecast
					-- DECLARE @lastYearLastForecast VARCHAR(5);
					
					-- SELECT @lastYearLastForecast = COALESCE(scenario, 'FCT12')
					-- FROM (
					-- 	SELECT TOP 1 scenario
					-- 	FROM XFC_RES_FACT_work_order_figures WITH(NOLOCK)
					-- 	WHERE entity = @paramEntity AND year = @paramLastYear AND scenario LIKE 'FCT%'
					-- 	ORDER BY CAST(RIGHT(scenario, 2) AS INT) DESC
					-- ) AS subquery;
					
					-- Step 1: Create and populate a temporary table with the filtered work orders.
					CREATE TABLE #work_order (
					    entity VARCHAR(255),
					    project_id VARCHAR(255),
					    work_order_id VARCHAR(255),
					    work_order_description VARCHAR(255),
					    technology VARCHAR(255),
					    start_date VARCHAR(10),
					    end_date VARCHAR(10),
					    rev_type VARCHAR(255),
					    serv_type VARCHAR(255),
					    client VARCHAR(255),
					    intercompany VARCHAR(255)
					);
					
					-- Use a CTE to resolve unique work order/intercompany combinations
					-- to prevent duplicate key errors.
					WITH FilteredWorkOrders AS (
						SELECT *
						FROM XFC_RES_MASTER_work_order WITH(NOLOCK)
						WHERE entity = @paramEntity
					), WorkOrderIntercompanies AS (
					    -- 1. Get explicit intercompanies from the fact table.
					    SELECT DISTINCT
					        entity,
					        wo_id,
					        intercompany
					    FROM XFC_RES_FACT_work_order_figures WITH(NOLOCK)
					    WHERE entity = @paramEntity AND intercompany IS NOT NULL
					
					    UNION
					
					    -- 2. Get intercompanies from the client, but ONLY for work orders
					    -- that do not have an explicit intercompany in the fact table.
					    SELECT DISTINCT
					        woi.entity,
					        woi.id AS wo_id,
					        COALESCE(ec.entity, 'None') AS intercompany
					    FROM FilteredWorkOrders woi
					    LEFT JOIN XFC_RES_AUX_entity_currency ec WITH(NOLOCK) ON woi.client = ec.entity
					    WHERE NOT EXISTS (
					            SELECT 1
					            FROM XFC_RES_FACT_work_order_figures wof_check WITH(NOLOCK)
					            WHERE wof_check.entity = woi.entity
					              AND wof_check.wo_id = woi.id
					              AND wof_check.intercompany IS NOT NULL
					        )
					)
					INSERT INTO #work_order (
					    entity, project_id, work_order_id, work_order_description,
					    technology, start_date, end_date, rev_type, serv_type, client, intercompany
					)
					SELECT
					    woi.entity,
					    woi.project_id,
					    woi.id AS work_order_id,
					    woi.description AS work_order_description,
					    woi.technology,
					    FORMAT(woi.start_date, 'yyyyMM') AS start_date,
					    FORMAT(woi.end_date, 'yyyyMM') AS end_date,
					    woi.rev_type,
					    woi.serv_type,
					    woi.client,
					    woic.intercompany
					FROM FilteredWorkOrders woi
					JOIN WorkOrderIntercompanies woic ON woi.entity = woic.entity AND woi.id = woic.wo_id
					WHERE (
					        @paramTechnology = 'All'
					        OR (@paramTechnology = 'Other' AND woi.technology NOT LIKE 'AM%' AND woi.technology NOT LIKE 'OM%')
					        OR woi.technology LIKE @paramTechnology
					    )
					    -- Date and data existence filtering logic.
					    AND (
					        woi.end_date IS NULL
					        OR woi.end_date >= DATEFROMPARTS(@paramYear, 1, 1)
					        OR EXISTS (
					            SELECT 1
					            FROM XFC_RES_FACT_work_order_figures wofs WITH(NOLOCK)
					            WHERE wofs.year = @paramActualYear
					              AND wofs.scenario = 'ACT'
					              AND wofs.entity = woi.entity
					              AND wofs.wo_id = woi.id
					        )
					    );
					
					-- Step 2: Use CTEs to organize the final query logic.
					WITH
					-- CTE to define accounts and their display order.
					account AS (
					    SELECT MemberName AS account, MemberDescription AS account_description, 1 AS order_flag
					    FROM XFC_RES_DIM_Account WITH(NOLOCK)
					    WHERE MemberName LIKE 'A4%' AND Level = 0
					    UNION ALL
					    SELECT 'OE_Index', 'Order Entry - Index', 2
					    UNION ALL
					    SELECT 'OE_ContractRenewal', 'Order Entry - Contract Renewal', 3
					    UNION ALL
					    SELECT 'OE_NewContract', 'Order Entry - New Contract', 4
					    UNION ALL
					    SELECT 'OE_AdditionalServices', 'Order Entry - Additional Services', 5
					    UNION ALL
					    SELECT 'OE_Other', 'Order Entry - Other', 6
					    UNION ALL
					    SELECT 'OE_DirectCost', 'Order Entry - Direct Cost', 7					
					    UNION ALL
					    SELECT MemberName, MemberDescription, 8
					    FROM XFC_RES_DIM_Account WITH(NOLOCK)
					    WHERE MemberName LIKE 'A5%' AND Level = 0
					), filtered_work_order_figures_horizontal AS (
						SELECT entity, wo_id, intercompany, account,
					           year, scenario, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12
					    FROM XFC_RES_VIEW_work_order_figures_horizontal WITH(NOLOCK)
					    WHERE entity = @paramEntity
					),
					
					-- CTE to unify all figures data from different years/scenarios.
					all_figures AS (
					    -- Current year data
					    SELECT entity, wo_id, intercompany, account,
					           year, scenario, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12
					    FROM filtered_work_order_figures_horizontal
					    WHERE year = @paramActualYear AND scenario = @paramActualScenario
					    UNION ALL
					    -- Years before data
					    SELECT entity, wo_id, intercompany, account,
					           year, 'PPY', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, SUM(amount) AS m12
					    FROM XFC_RES_FACT_work_order_figures WITH(NOLOCK)
					    WHERE entity = @paramEntity AND year < @paramLastYear AND (
							(
								account NOT LIKE 'OE_%' AND
								scenario = 'ACT'
								-- scenario = @paramActualScenario
							) OR (
								account LIKE 'OE_%' AND
								scenario = 'FCT12'
							)
						)
					    GROUP BY entity, wo_id, intercompany, account, year
					    UNION ALL
					    -- Last year data
					    SELECT entity, wo_id, intercompany, account,
					           year, 'PY', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, SUM(amount) AS m12
					    FROM XFC_RES_FACT_work_order_figures WITH(NOLOCK)
					    WHERE entity = @paramEntity AND year = @paramLastYear AND (
							(
								account NOT LIKE 'OE_%' AND
								scenario = 'ACT'
								-- scenario = @paramActualScenario
							) OR (
								account LIKE 'OE_%' AND
								scenario = 'FCT12'
								-- scenario = @lastYearLastForecast
							)
						)
					    GROUP BY entity, wo_id, intercompany, account, year
					    UNION ALL
					    -- Next year data (BUD)
					    SELECT entity, wo_id, intercompany, account,
					           year, scenario, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12
					    FROM filtered_work_order_figures_horizontal
					    WHERE year = @paramNextYear AND scenario = 'BUD'
					    UNION ALL
					    -- PLN1 data
					    SELECT entity, wo_id, intercompany, account,
					           year, scenario, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12
					    FROM filtered_work_order_figures_horizontal
					    WHERE year = @paramP1Year AND scenario = 'PLN1'
					    UNION ALL
					    -- PLN2 data
					    SELECT entity, wo_id, intercompany, account,
					           year, scenario, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12
					    FROM filtered_work_order_figures_horizontal
					    WHERE year = @paramP2Year AND scenario = 'PLN2'
					    UNION ALL
					    -- PLN3 data
					    SELECT entity, wo_id, intercompany, account,
					           year, scenario, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, SUM(amount) AS m12
					    FROM XFC_RES_FACT_work_order_figures WITH(NOLOCK)
					    WHERE entity = @paramEntity AND year = @paramP3Year AND scenario = 'PLN3' AND month = 12
					    GROUP BY entity, wo_id, intercompany, account, year, scenario
					),
					
					-- CTE to pivot the data into one row per work_order/account.
					pivoted_figures AS (
					    SELECT
					        entity, wo_id, intercompany, account,
					        -- Conditional sums to pivot data by year and scenario.
					
					        SUM(CASE WHEN scenario = 'PPY' THEN m12 ELSE 0 END) AS ppy,
					
					        SUM(CASE WHEN scenario = 'PY' THEN m12 ELSE 0 END) AS py,
					
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m1 ELSE 0 END) AS m1,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m2 ELSE 0 END) AS m2,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m3 ELSE 0 END) AS m3,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m4 ELSE 0 END) AS m4,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m5 ELSE 0 END) AS m5,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m6 ELSE 0 END) AS m6,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m7 ELSE 0 END) AS m7,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m8 ELSE 0 END) AS m8,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m9 ELSE 0 END) AS m9,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m10 ELSE 0 END) AS m10,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m11 ELSE 0 END) AS m11,
					        SUM(CASE WHEN year = @paramActualYear AND scenario = @paramActualScenario THEN m12 ELSE 0 END) AS m12,
					        
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m1 ELSE 0 END) AS bm1,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m2 ELSE 0 END) AS bm2,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m3 ELSE 0 END) AS bm3,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m4 ELSE 0 END) AS bm4,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m5 ELSE 0 END) AS bm5,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m6 ELSE 0 END) AS bm6,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m7 ELSE 0 END) AS bm7,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m8 ELSE 0 END) AS bm8,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m9 ELSE 0 END) AS bm9,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m10 ELSE 0 END) AS bm10,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m11 ELSE 0 END) AS bm11,
					        SUM(CASE WHEN year = @paramNextYear AND scenario = 'BUD' THEN m12 ELSE 0 END) AS bm12,
					
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m1 ELSE 0 END) AS p1m1,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m2 ELSE 0 END) AS p1m2,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m3 ELSE 0 END) AS p1m3,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m4 ELSE 0 END) AS p1m4,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m5 ELSE 0 END) AS p1m5,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m6 ELSE 0 END) AS p1m6,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m7 ELSE 0 END) AS p1m7,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m8 ELSE 0 END) AS p1m8,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m9 ELSE 0 END) AS p1m9,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m10 ELSE 0 END) AS p1m10,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m11 ELSE 0 END) AS p1m11,
					        SUM(CASE WHEN year = @paramP1Year AND scenario = 'PLN1' THEN m12 ELSE 0 END) AS p1m12,
					
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m1 ELSE 0 END) AS p2m1,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m2 ELSE 0 END) AS p2m2,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m3 ELSE 0 END) AS p2m3,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m4 ELSE 0 END) AS p2m4,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m5 ELSE 0 END) AS p2m5,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m6 ELSE 0 END) AS p2m6,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m7 ELSE 0 END) AS p2m7,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m8 ELSE 0 END) AS p2m8,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m9 ELSE 0 END) AS p2m9,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m10 ELSE 0 END) AS p2m10,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m11 ELSE 0 END) AS p2m11,
					        SUM(CASE WHEN year = @paramP2Year AND scenario = 'PLN2' THEN m12 ELSE 0 END) AS p2m12,
					        
					        SUM(CASE WHEN year = @paramP3Year AND scenario = 'PLN3' THEN m12 ELSE 0 END) AS p3
					    FROM all_figures
					    GROUP BY entity, wo_id, intercompany, account
					)
					
					-- Step 3: Final SELECT joining work orders, accounts, and financial data.
					SELECT
					    wo.entity,
					    wo.project_id,
					    wo.work_order_id,
					    wo.work_order_description,
					    wo.technology,
					    wo.start_date,
					    wo.end_date,
					    wo.rev_type,
					    wo.serv_type,
					    wo.client,
					    wo.intercompany,
					    a.account,
					    a.account_description,
						COALESCE(pf.ppy, 0) AS ppy,
						COALESCE(pf.py, 0) AS py,
					    @paramRefScenario AS ref_scenario,
					    -- Use COALESCE to display 0 instead of NULL for numeric values.
					    COALESCE(pf.m1, 0) AS m1, COALESCE(pf.m2, 0) AS m2, COALESCE(pf.m3, 0) AS m3, COALESCE(pf.m4, 0) AS m4,
					    COALESCE(pf.m5, 0) AS m5, COALESCE(pf.m6, 0) AS m6, COALESCE(pf.m7, 0) AS m7, COALESCE(pf.m8, 0) AS m8,
					    COALESCE(pf.m9, 0) AS m9, COALESCE(pf.m10, 0) AS m10, COALESCE(pf.m11, 0) AS m11, COALESCE(pf.m12, 0) AS m12,
					    COALESCE(pf.bm1, 0) AS bm1, COALESCE(pf.bm2, 0) AS bm2, COALESCE(pf.bm3, 0) AS bm3, COALESCE(pf.bm4, 0) AS bm4,
					    COALESCE(pf.bm5, 0) AS bm5, COALESCE(pf.bm6, 0) AS bm6, COALESCE(pf.bm7, 0) AS bm7, COALESCE(pf.bm8, 0) AS bm8,
					    COALESCE(pf.bm9, 0) AS bm9, COALESCE(pf.bm10, 0) AS bm10, COALESCE(pf.bm11, 0) AS bm11, COALESCE(pf.bm12, 0) AS bm12,
					    COALESCE(pf.p1m1, 0) AS p1m1, COALESCE(pf.p1m2, 0) AS p1m2, COALESCE(pf.p1m3, 0) AS p1m3, COALESCE(pf.p1m4, 0) AS p1m4,
					    COALESCE(pf.p1m5, 0) AS p1m5, COALESCE(pf.p1m6, 0) AS p1m6, COALESCE(pf.p1m7, 0) AS p1m7, COALESCE(pf.p1m8, 0) AS p1m8,
					    COALESCE(pf.p1m9, 0) AS p1m9, COALESCE(pf.p1m10, 0) AS p1m10, COALESCE(pf.p1m11, 0) AS p1m11, COALESCE(pf.p1m12, 0) AS p1m12,
					    COALESCE(pf.p2m1, 0) AS p2m1, COALESCE(pf.p2m2, 0) AS p2m2, COALESCE(pf.p2m3, 0) AS p2m3, COALESCE(pf.p2m4, 0) AS p2m4,
					    COALESCE(pf.p2m5, 0) AS p2m5, COALESCE(pf.p2m6, 0) AS p2m6, COALESCE(pf.p2m7, 0) AS p2m7, COALESCE(pf.p2m8, 0) AS p2m8,
					    COALESCE(pf.p2m9, 0) AS p2m9, COALESCE(pf.p2m10, 0) AS p2m10, COALESCE(pf.p2m11, 0) AS p2m11, COALESCE(pf.p2m12, 0) AS p2m12,
					    COALESCE(pf.p3, 0) AS p3
					FROM
					    #work_order wo
					-- CROSS JOIN to combine each work order with each account type.
					CROSS JOIN
					    account a
					-- LEFT JOIN to bring in the numerical data.
					LEFT JOIN
					    pivoted_figures pf ON wo.entity = pf.entity
					                      AND wo.work_order_id = pf.wo_id
					                      AND wo.intercompany = pf.intercompany
					                      AND a.account = pf.account
					WHERE a.account LIKE 'OE_%' OR pf.account IS NOT NULL
					ORDER BY
					    wo.entity,
					    wo.work_order_id,
					    wo.intercompany,
					    a.order_flag,
					    a.account;
					
					-- Final cleanup.
					DROP TABLE #work_order;

				    "
					
				#End Region
				
				#Region "Structure"
				
				Case "structure"
					Return "
					WITH actual_year_figures AS (
					    SELECT
					        entity, ud1, intercompany, account, ud3,
					        SUM(m1) AS m1, SUM(m2) AS m2,
					        SUM(m3) AS m3, SUM(m4) AS m4,
					        SUM(m5) AS m5, SUM(m6) AS m6,
					        SUM(m7) AS m7, SUM(m8) AS m8,
					        SUM(m9) AS m9, SUM(m10) AS m10,
					        SUM(m11) AS m11, SUM(m12) AS m12
					    FROM XFC_RES_VIEW_structure_figures_horizontal
					    WHERE year = @paramActualYear
					        AND entity = @paramEntity
					        AND scenario = @paramActualScenario
					    GROUP BY entity, ud1, intercompany, account, ud3
					),
					next_year_figures AS (
					    SELECT
					        entity, ud1, intercompany, account, ud3,
					        SUM(m1) AS bm1, SUM(m2) AS bm2,
					        SUM(m3) AS bm3, SUM(m4) AS bm4,
					        SUM(m5) AS bm5, SUM(m6) AS bm6,
					        SUM(m7) AS bm7, SUM(m8) AS bm8,
					        SUM(m9) AS bm9, SUM(m10) AS bm10,
					        SUM(m11) AS bm11, SUM(m12) AS bm12
					    FROM XFC_RES_VIEW_structure_figures_horizontal WITH(NOLOCK)
					    WHERE year = @paramNextYear
					        AND entity = @paramEntity
					        AND scenario = 'BUD'
					    GROUP BY entity, ud1, intercompany, account, ud3
					),
					p1_figures AS (
					    SELECT
					        entity, ud1, intercompany, account, ud3,
					        SUM(m1) AS p1m1, SUM(m2) AS p1m2,
					        SUM(m3) AS p1m3, SUM(m4) AS p1m4,
					        SUM(m5) AS p1m5, SUM(m6) AS p1m6,
					        SUM(m7) AS p1m7, SUM(m8) AS p1m8,
					        SUM(m9) AS p1m9, SUM(m10) AS p1m10,
					        SUM(m11) AS p1m11, SUM(m12) AS p1m12
					    FROM XFC_RES_VIEW_structure_figures_horizontal WITH(NOLOCK)
					    WHERE year = @paramP1Year
					        AND entity = @paramEntity
					        AND scenario = 'PLN1'
					    GROUP BY entity, ud1, intercompany, account, ud3
					),
					p2_figures AS (
					    SELECT
					        entity, ud1, intercompany, account, ud3,
					        SUM(m1) AS p2m1, SUM(m2) AS p2m2,
					        SUM(m3) AS p2m3, SUM(m4) AS p2m4,
					        SUM(m5) AS p2m5, SUM(m6) AS p2m6,
					        SUM(m7) AS p2m7, SUM(m8) AS p2m8,
					        SUM(m9) AS p2m9, SUM(m10) AS p2m10,
					        SUM(m11) AS p2m11, SUM(m12) AS p2m12
					    FROM XFC_RES_VIEW_structure_figures_horizontal WITH(NOLOCK)
					    WHERE year = @paramP2Year
					        AND entity = @paramEntity
					        AND scenario = 'PLN2'
					    GROUP BY entity, ud1, intercompany, account, ud3
					),
					full_figures AS (
					    SELECT
					        COALESCE(ayf.entity, nyf.entity, p1f.entity, p2f.entity) AS entity,
					        COALESCE(ayf.ud1, nyf.ud1, p1f.ud1, p2f.ud1) AS ud1,
					        COALESCE(ayf.intercompany, nyf.intercompany, p1f.intercompany, p2f.intercompany) AS intercompany,
					        COALESCE(ayf.account, nyf.account, p1f.account, p2f.account) AS account,
					        COALESCE(ayf.ud3, nyf.ud3, p1f.ud3, p2f.ud3) AS ud3,
					
					        ayf.m1, ayf.m2, ayf.m3, ayf.m4, ayf.m5, ayf.m6,
					        ayf.m7, ayf.m8, ayf.m9, ayf.m10, ayf.m11, ayf.m12,
					
					        nyf.bm1, nyf.bm2, nyf.bm3, nyf.bm4, nyf.bm5, nyf.bm6,
					        nyf.bm7, nyf.bm8, nyf.bm9, nyf.bm10, nyf.bm11, nyf.bm12,
					
					        p1f.p1m1, p1f.p1m2, p1f.p1m3, p1f.p1m4, p1f.p1m5, p1f.p1m6,
					        p1f.p1m7, p1f.p1m8, p1f.p1m9, p1f.p1m10, p1f.p1m11, p1f.p1m12,
					
					        p2f.p2m1, p2f.p2m2, p2f.p2m3, p2f.p2m4, p2f.p2m5, p2f.p2m6,
					        p2f.p2m7, p2f.p2m8, p2f.p2m9, p2f.p2m10, p2f.p2m11, p2f.p2m12
					
					    FROM actual_year_figures ayf
					    FULL OUTER JOIN next_year_figures nyf ON ayf.entity = nyf.entity
					        AND ayf.ud1 = nyf.ud1 AND ayf.intercompany = nyf.intercompany
					        AND ayf.account = nyf.account AND ayf.ud3 = nyf.ud3
					    FULL OUTER JOIN p1_figures p1f ON ayf.entity = p1f.entity
					        AND ayf.ud1 = p1f.ud1 AND ayf.intercompany = p1f.intercompany
					        AND ayf.account = p1f.account AND ayf.ud3 = p1f.ud3
					    FULL OUTER JOIN p2_figures p2f ON ayf.entity = p2f.entity
					        AND ayf.ud1 = p2f.ud1 AND ayf.intercompany = p2f.intercompany
					        AND ayf.account = p2f.account AND ayf.ud3 = p2f.ud3
					)
					
					SELECT
					    ff.entity,
					    COALESCE(d.department_description,ff.ud3),
						COALESCE(d.subdepartment_description,ff.ud3),
					    ff.ud3,
					    COALESCE(d.cc_description,ff.ud3),
					    ff.ud1,
					    ff.intercompany,
					    ff.account,
					    a.MemberDescription AS account_description,
					    @paramRefScenario AS ref_scenario,
					
					    -- REF
					    SUM(ff.m1) AS m1, SUM(ff.m2) AS m2, SUM(ff.m3) AS m3,
					    SUM(ff.m4) AS m4, SUM(ff.m5) AS m5, SUM(ff.m6) AS m6,
					    SUM(ff.m7) AS m7, SUM(ff.m8) AS m8, SUM(ff.m9) AS m9,
					    SUM(ff.m10) AS m10, SUM(ff.m11) AS m11, SUM(ff.m12) AS m12,
					
					    -- BUD
					    SUM(ff.bm1) AS bm1, SUM(ff.bm2) AS bm2, SUM(ff.bm3) AS bm3,
					    SUM(ff.bm4) AS bm4, SUM(ff.bm5) AS bm5, SUM(ff.bm6) AS bm6,
					    SUM(ff.bm7) AS bm7, SUM(ff.bm8) AS bm8, SUM(ff.bm9) AS bm9,
					    SUM(ff.bm10) AS bm10, SUM(ff.bm11) AS bm11, SUM(ff.bm12) AS bm12,
					
					    -- PLN1
					    SUM(ff.p1m1) AS p1m1, SUM(ff.p1m2) AS p1m2, SUM(ff.p1m3) AS p1m3,
					    SUM(ff.p1m4) AS p1m4, SUM(ff.p1m5) AS p1m5, SUM(ff.p1m6) AS p1m6,
					    SUM(ff.p1m7) AS p1m7, SUM(ff.p1m8) AS p1m8, SUM(ff.p1m9) AS p1m9,
					    SUM(ff.p1m10) AS p1m10, SUM(ff.p1m11) AS p1m11, SUM(ff.p1m12) AS p1m12,
					
					    -- PLN2
					    SUM(ff.p2m1) AS p2m1, SUM(ff.p2m2) AS p2m2, SUM(ff.p2m3) AS p2m3,
					    SUM(ff.p2m4) AS p2m4, SUM(ff.p2m5) AS p2m5, SUM(ff.p2m6) AS p2m6,
					    SUM(ff.p2m7) AS p2m7, SUM(ff.p2m8) AS p2m8, SUM(ff.p2m9) AS p2m9,
					    SUM(ff.p2m10) AS p2m10, SUM(ff.p2m11) AS p2m11, SUM(ff.p2m12) AS p2m12
					
					FROM full_figures ff
					LEFT JOIN XFC_RES_AUX_department d ON ff.ud3 = d.cc
					LEFT JOIN XFC_RES_DIM_Account a WITH (NOLOCK) ON ff.account = a.MemberName
					WHERE @paramDepartment = 'All'
					    OR d.department_description = @paramDepartment
					GROUP BY
					    ff.entity,
					    d.department_description,
						d.subdepartment_description,
					    ff.ud3,
					    d.cc_description,
					    ff.ud1,
					    ff.intercompany,
					    ff.account,
					    a.MemberDescription
					ORDER BY
					    ff.entity,
					    CASE 
					        WHEN d.department_description LIKE 'DCOGS%' THEN 1
					        WHEN d.department_description LIKE 'ICOGS%' THEN 2
					        WHEN d.department_description LIKE 'OHD%' THEN 3
					        ELSE 4
					    END,
					    d.department_description,
					    ff.ud3, ff.ud1, ff.intercompany, ff.account, a.MemberDescription;
					"
					
				#End Region
				
				#Region "Contracts"
				
				Case "contracts"
					Return "
					SELECT
						entity,
						project_id,
						project_description,
						id,
						description,
						technology,
						client,
						rev_type,
						serv_type,
						phase,
						CONCAT(YEAR(start_date), FORMAT(MONTH(start_date), '00')) AS start_date,
						CONCAT(YEAR(break_clause_date_1), FORMAT(MONTH(break_clause_date_1), '00')) AS break_clause_date_1,
						CONCAT(YEAR(break_clause_date_2), FORMAT(MONTH(break_clause_date_2), '00')) AS break_clause_date_2,
						CONCAT(YEAR(break_clause_date_3), FORMAT(MONTH(break_clause_date_3), '00')) AS break_clause_date_3,
						CONCAT(YEAR(end_date), FORMAT(MONTH(end_date), '00')) AS end_date,
						mwh,
						as_sold_contract_value,
						as_sold_direct_cost,
						contract_backlog_value,
						comment
					FROM XFC_RES_MASTER_work_order
					WHERE
						@paramEntity = 'All'
						OR (
							entity = @paramEntity
							AND (
								(end_date >= DATEFROMPARTS(@paramYear, 1, 1))
								OR end_date IS NULL
							)
							AND (
								@paramTechnology = 'All'
								OR (@paramTechnology <> 'Other' AND technology LIKE @paramTechnology)
								OR (@paramTechnology = 'Other' AND technology NOT LIKE 'AM%' AND technology NOT LIKE 'OM%')
							)
						);
					"
				
				#End Region
				
				#Region "Contracts All"
				
				Case "contracts_all"
					Return "
					SELECT
						entity,
						project_id,
						project_description,
						id,
						description,
						technology,
						client,
						rev_type,
						serv_type,
						phase,
						CONCAT(YEAR(start_date), FORMAT(MONTH(start_date), '00')) AS start_date,
						CONCAT(YEAR(break_clause_date_1), FORMAT(MONTH(break_clause_date_1), '00')) AS break_clause_date_1,
						CONCAT(YEAR(break_clause_date_2), FORMAT(MONTH(break_clause_date_2), '00')) AS break_clause_date_2,
						CONCAT(YEAR(break_clause_date_3), FORMAT(MONTH(break_clause_date_3), '00')) AS break_clause_date_3,
						CONCAT(YEAR(end_date), FORMAT(MONTH(end_date), '00')) AS end_date,
						mwh,
						as_sold_contract_value,
						as_sold_direct_cost,
						contract_backlog_value,
						comment
					FROM XFC_RES_MASTER_work_order
					WHERE
						end_date >= DATEFROMPARTS(@paramYear, 1, 1)
						OR end_date IS NULL;
					"
				
				#End Region
				
			End Select
			
			Throw New Exception($"No planning template query for '{type}'")
		
		End Function
		
		#End Region
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
