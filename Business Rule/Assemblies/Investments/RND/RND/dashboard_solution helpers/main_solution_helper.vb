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
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						#Region "Check Set Internal Expense Percentage"
						
						If args.FunctionName.XFEqualsIgnoreCase("CheckSetInternalExpensePercentage") Then
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
							
							'Perform a merge to create rows for the selected scenario and year if they don't exist yet
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								BRApi.Database.ExecuteSql(
									dbConn,
									$"
										MERGE INTO XFC_RND_FACT_project_internal_expense_proportion AS target
										USING (
											SELECT
												@paramYear AS year,
												@paramScenario AS scenario,
												1300 AS company_id,
												0 AS proportion
										) AS source
										ON target.year = source.year
										AND target.scenario = source.scenario
										AND target.company_id = source.company_id
										WHEN NOT MATCHED THEN
											INSERT (
												year, scenario, company_id, proportion
											)
											VALUES (
												source.year, source.scenario, source.company_id, source.proportion
											);
									",
									dbParamInfos,
									False
								)
							End Using
						
						#End Region
						
						#Region "Download Projects Cash Out Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadProjectsCashOutTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramUsername As String = si.UserName
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = "RnDProjectsCashOutTemplate.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"RnDProjectsCashOut.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
						
						#End Region
						
						#Region "Download Projects Capitalization Template"
						
						Else If args.FunctionName.XFEqualsIgnoreCase("DownloadProjectsCapitalizationTemplate") Then
							'Declare selection changed task result
							Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
							selectionChangedTaskResult.IsOK = True
							selectionChangedTaskResult.ShowMessageBox = False
							
							'Get parameters
							Dim paramUsername As String = si.UserName
							
							'Define path and file names of source and target
							Dim sourcePath As String = "Documents/Public/Investments/Templates"
							Dim sourceFile As String = "RnDProjectsCapitalizationTemplate.xlsx"
							Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
							Dim targetFinalPath As String = $"Import Templates"
							Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
							Dim targetFile As String = $"RnDProjectsCapitalization.xlsx"
							
							'Create folder if necessary
							BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
							
							'Clear folder
							UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
							
							'Generate file
							UTISharedFunctionsBR.CopyXLSXFile(si, sourceFile, sourcePath, targetFile, targetPath)
						
						#End Region
						
						End If
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						
						#Region "Save Expense Internal Percentage"
						
						If args.FunctionName.XFEqualsIgnoreCase("SaveExpenseInternalPercentage") Then
							'Get parameters for queries
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							
							'Get save data task info
							Dim saveDataTaskInfo As XFSqlTableEditorSaveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo
							
							'Create a datatable from modified rows to insert in cash table
							Dim dt As DataTable = UTISharedFunctionsBR.CreateDataTableFromEditedDataRows(si, saveDataTaskInfo.EditedDataRows)
							BRApi.ErrorLog.LogMessage(si, "Tabla", UTISharedFunctionsBR.GetFirstDataTableRows(dt, 1))
							'Save rows in aux internal percentage table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_RND_AUX_project_internal_expense_proportion", dt, "replace")
							
							'Save rows in internal percentage table
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								dbConn.BeginTrans()
								Try
									BRApi.Database.ExecuteSql(
										dbConn,
										$"
										MERGE INTO XFC_RND_FACT_project_internal_expense_proportion AS target
										USING (
											SELECT 
												year,
												scenario,
												company_id,
										       	proportion
											FROM 
											(
												SELECT year, scenario, [1300], [1301], [1302], [1303], [585], [592], [611], [671]
											    FROM XFC_RND_AUX_project_internal_expense_proportion
											) AS SourceTable
											UNPIVOT
											(
											    proportion FOR company_id IN ([1300], [1301], [1302], [1303], [585], [592], [611], [671])
											) AS UnpivotedTable
											WHERE proportion <= 100
										) AS source
										ON target.year = source.year
										AND target.scenario = source.scenario
										AND target.company_id = source.company_id
										WHEN MATCHED THEN
										    UPDATE SET
										        proportion = source.proportion
										WHEN NOT MATCHED THEN
										    INSERT (
										        year, scenario, company_id, proportion
										    )
										    VALUES (
										        source.year, source.scenario, source.company_id, source.proportion
										    );
										",
										dbParamInfos, False
									)
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
