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
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_solution_helper
	Public Class MainClass
		
		#Region "Constants"
		
		' Database constants
		Private Const TABLE_WEEK_CONFIRM As String = "XFC_TRS_AUX_TreasuryWeekConfirm"
		Private Const COL_CASH_DEBT As String = "CashDebt"
		Private Const COL_CASH_FLOW As String = "CashFlow"
		
		' Template types
		Private Const TEMPLATE_CASH_DEBT As String = "CashDebtPosition"
		Private Const TEMPLATE_CASH_FLOW As String = "CashFlowForecasting"
		
		' Account types for bank rows
		Private Const ACCOUNT_CASH_FINANCING As String = "CASH AND FINANCING BALANCE"
		Private Const ACCOUNT_USED_LINES As String = "FINANCING - USED LINES"
		Private Const ACCOUNT_AVAILABLE_LINES As String = "FINANCING - AVAILABLE LINES"
		
		#End Region
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = DashboardExtenderFunctionType.LoadDashboard
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And _
							   args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = False
								loadDashboardTaskResult.ModifiedCustomSubstVars = Nothing
								Return loadDashboardTaskResult
							End If
						End If
					
					Case Is = DashboardExtenderFunctionType.ComponentSelectionChanged
						
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							' Placeholder for test function
							
						#Region "Confirm/Unconfirm Week Functions"
						
						ElseIf args.FunctionName.XFEqualsIgnoreCase("ConfirmWeekCashDebt") Then
							Return Me.ProcessWeekConfirmation(si, args.NameValuePairs, COL_CASH_DEBT, True, TEMPLATE_CASH_DEBT)
							
						ElseIf args.FunctionName.XFEqualsIgnoreCase("ConfirmWeekCashFlow") Then
							Return Me.ProcessWeekConfirmation(si, args.NameValuePairs, COL_CASH_FLOW, True, TEMPLATE_CASH_FLOW)
							
						ElseIf args.FunctionName.XFEqualsIgnoreCase("UnConfirmWeekCashDebt") Then
							Return Me.ProcessWeekConfirmation(si, args.NameValuePairs, COL_CASH_DEBT, False, TEMPLATE_CASH_DEBT)
							
						ElseIf args.FunctionName.XFEqualsIgnoreCase("UnConfirmWeekCashFlow") Then
							Return Me.ProcessWeekConfirmation(si, args.NameValuePairs, COL_CASH_FLOW, False, TEMPLATE_CASH_FLOW)
						
						#End Region
						
						#Region "Download Treasury Template"
						
						ElseIf args.FunctionName.XFEqualsIgnoreCase("DownloadTreasuryTemplateCashDebtPosition") Then
							
							Dim config As New TemplateConfig With {
								.TemplateType = TEMPLATE_CASH_DEBT,
								.SourcePath = "Documents/Public/Treasury/Templates",
								.SourceFile = "TreasuryTemplateCashDebtPosition.xlsx",
								.TargetFileNamePattern = "Treasury_Template_CashDebtPosition_{0}_{1}_{2}.xlsx",
								.HeaderRow = 10,
								.MetadataRow = 10,
								.InfoRows = New Dictionary(Of String, String) From {
									{"B2", "Entity"},
									{"B3", "StartDate"},
									{"B4", "EndDate"},
									{"B5", "EndOfMonth"}
								},
								.HeaderCells = New Dictionary(Of String, String) From {
									{"A10", "xfText#:Account"},
									{"B10", "xfText#:Flow"},
									{"C10", "xfText#:Bank"},
									{"D10", "xfDec#:StartWeek::0"},
									{"E10", "xfDec#:EndWeek::0"},
									{"F10", "xfDec#:EOM::0"}
								},
								.UseWeekColumns = False,
								.UseBankRows = True
							}
							
							Return Me.GenerateTreasuryTemplate(si, config, args.NameValuePairs)
							
						ElseIf args.FunctionName.XFEqualsIgnoreCase("DownloadTreasuryTemplateCashFlowForecasting") Then
							
							Dim config As New TemplateConfig With {
								.TemplateType = TEMPLATE_CASH_FLOW,
								.SourcePath = "Documents/Public/Treasury/Templates",
								.SourceFile = "TreasuryTemplateCashCashFlowForecasting.xlsx",
								.TargetFileNamePattern = "Treasury_Template_CashFlowForecasting_{0}_{1}_{2}.xlsx",
								.HeaderRow = 6,
								.MetadataRow = 10,
								.InfoRows = New Dictionary(Of String, String) From {
									{"B2", "Entity"},
									{"B3", "StartDate"}
								},
								.HeaderCells = Nothing,
								.UseWeekColumns = True,
								.UseBankRows = False
							}
							
							Return Me.GenerateTreasuryTemplate(si, config, args.NameValuePairs)

						ElseIf args.FunctionName.XFEqualsIgnoreCase("DownloadTreasuryMonitoring") Then
							Return Me.GenerateTreasuryMonitoringCopy(si, args.NameValuePairs)
						
						#End Region
						
						End If
					
					Case Is = DashboardExtenderFunctionType.SqlTableEditorSaveData
						
						If args.FunctionName.XFEqualsIgnoreCase("TestFunction") Then
							Dim saveDataTaskResult As New XFSqlTableEditorSaveDataTaskResult()
							saveDataTaskResult.IsOK = True
							saveDataTaskResult.ShowMessageBox = False
							saveDataTaskResult.Message = ""
							saveDataTaskResult.CancelDefaultSave = False
							Return saveDataTaskResult
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Week Confirmation - Centralized & Optimized"
		
		''' <summary>
		''' OPTIMIZED: Generic function that handles all Confirm/Unconfirm operations
		''' Replaces 4 separate functions with identical logic
		''' </summary>
		''' <param name="columnType">Either COL_CASH_DEBT or COL_CASH_FLOW</param>
		''' <param name="isConfirm">True to confirm, False to unconfirm</param>
		''' <param name="templateType">For message display purposes</param>
		Private Function ProcessWeekConfirmation(
			ByVal si As SessionInfo,
			ByVal nameValuePairs As Dictionary(Of String, String),
			ByVal columnType As String,
			ByVal isConfirm As Boolean,
			ByVal templateType As String
		) As XFSelectionChangedTaskResult
			
			Dim result As New XFSelectionChangedTaskResult()
			result.IsOK = True
			result.ShowMessageBox = True
			
			Try
				' 1. Validate and extract parameters
				Dim params As WeekConfirmParams = Me.ValidateWeekConfirmationParams(si, nameValuePairs)
				If params Is Nothing Then
					result.IsOK = False
					result.Message = "Company, Year, and Week parameters are required."
					Return result
				End If
				
				' 2. Security Check for Unconfirm
				If Not isConfirm Then
					If Not BRApi.Security.Authorization.IsUserInGroup(si, "F_TRS_Super") Then
						result.IsOK = False
						result.Message = "You must be an Admin (F_TRS_Super) to unconfirm the week."
						Return result
					End If
				End If
				
				' 3. Process confirmation/unconfirmation
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim recordExists As Boolean = Me.CheckRecordExists(dbConn, params)
					
					If isConfirm Then
						' Confirm: Upsert record with value 1
						Me.UpsertWeekConfirmation(dbConn, params, columnType, 1, recordExists)
						result.Message = $"Week {params.Week} of {params.Year} for {params.Entity} has been confirmed for {templateType}."
					Else
						' Unconfirm: Update to 0 if exists, error if not
						If recordExists Then
							Me.UpsertWeekConfirmation(dbConn, params, columnType, 0, True)
							result.Message = $"Week {params.Week} of {params.Year} for {params.Entity} has been unconfirmed for {templateType}."
						Else
							result.IsOK = False
							result.Message = $"No confirmation record found for Week {params.Week} of {params.Year} for {params.Entity}."
						End If
					End If
				End Using
				
			Catch ex As Exception
				result.IsOK = False
				result.Message = $"Error processing week confirmation: {ex.Message}"
			End Try
			
			Return result
		End Function
		
		''' <summary>
		''' Parameter container for week confirmation
		''' </summary>
		Private Class WeekConfirmParams
			Public Property Entity As String
			Public Property Year As Integer
			Public Property Week As Integer
		End Class
		
		''' <summary>
		''' Validates and extracts parameters for week confirmation
		''' Returns Nothing if validation fails
		''' </summary>
		Private Function ValidateWeekConfirmationParams(ByVal si As SessionInfo, ByVal nameValuePairs As Dictionary(Of String, String)) As WeekConfirmParams
			Try
				Dim paramCompany As String = nameValuePairs("p_company")
				Dim paramYear As String = nameValuePairs("p_year")
				Dim paramWeek As String = nameValuePairs("p_week")
				
				' Check required parameters
				If String.IsNullOrEmpty(paramCompany) OrElse String.IsNullOrEmpty(paramYear) OrElse String.IsNullOrEmpty(paramWeek) Then
					Return Nothing
				End If
				
				' Parse numeric parameters
				Dim yearInt As Integer
				Dim weekInt As Integer
				If Not Integer.TryParse(paramYear, yearInt) OrElse Not Integer.TryParse(paramWeek, weekInt) Then
					Return Nothing
				End If
				
				' Convert company ID to entity name
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramCompany)
				
				Return New WeekConfirmParams With {
					.Entity = paramEntity,
					.Year = yearInt,
					.Week = weekInt
				}
			Catch
				Return Nothing
			End Try
		End Function
		
		''' <summary>
		''' Checks if a confirmation record exists
		''' </summary>
		Private Function CheckRecordExists(ByVal dbConn As DbConnInfo, ByVal params As WeekConfirmParams) As Boolean
			Dim query As String = $"
				SELECT COUNT(*) as RecordCount
				FROM {TABLE_WEEK_CONFIRM}
				WHERE Entity = @entity AND Week = @week AND Year = @year
			"
			
			Dim dbParams As New List(Of DbParamInfo) From {
				New DbParamInfo("entity", params.Entity),
				New DbParamInfo("week", params.Week),
				New DbParamInfo("year", params.Year)
			}
			
			Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, query, dbParams, False)
			Return Convert.ToInt32(dt.Rows(0)("RecordCount")) > 0
		End Function
		
		''' <summary>
		''' Upserts a week confirmation record
		''' </summary>
		Private Sub UpsertWeekConfirmation(
			ByVal dbConn As DbConnInfo,
			ByVal params As WeekConfirmParams,
			ByVal columnType As String,
			ByVal value As Integer,
			ByVal recordExists As Boolean
		)
			Dim query As String
			
			If recordExists Then
				' Update existing record
				query = $"
					UPDATE {TABLE_WEEK_CONFIRM}
					SET {columnType} = @value
					WHERE Entity = @entity AND Week = @week AND Year = @year
				"
			Else
				' Insert new record - set the specified column to value, other to 0
				Dim cashDebtValue As Integer = If(columnType = COL_CASH_DEBT, value, 0)
				Dim cashFlowValue As Integer = If(columnType = COL_CASH_FLOW, value, 0)
				
				query = $"
					INSERT INTO {TABLE_WEEK_CONFIRM} (Entity, Week, Year, {COL_CASH_DEBT}, {COL_CASH_FLOW})
					VALUES (@entity, @week, @year, {cashDebtValue}, {cashFlowValue})
				"
			End If
			
			Dim dbParams As New List(Of DbParamInfo) From {
				New DbParamInfo("entity", params.Entity),
				New DbParamInfo("week", params.Week),
				New DbParamInfo("year", params.Year)
			}
			
			If recordExists Then
				dbParams.Add(New DbParamInfo("value", value))
			End If
			
			BRApi.Database.ExecuteSql(dbConn, query, dbParams, False)
		End Sub
		
		#End Region
		
		#Region "Template Generation - Centralized & Optimized"
		
		''' <summary>
		''' Configuration structure for Treasury templates
		''' </summary>
		Private Structure TemplateConfig
			Public TemplateType As String
			Public SourcePath As String
			Public SourceFile As String
			Public TargetFileNamePattern As String
			Public HeaderRow As Integer
			Public MetadataRow As Integer
			Public InfoRows As Dictionary(Of String, String)
			Public HeaderCells As Dictionary(Of String, String)
			Public UseWeekColumns As Boolean
			Public UseBankRows As Boolean
		End Structure
		
		''' <summary>
		''' Gets 9 week columns starting from week BEFORE upload week
		''' Returns list of tuples: (weekNumber, year, weekKey)
		''' </summary>
		Private Function GetWeekColumnsFromDatabase(
			ByVal si As SessionInfo,
			ByVal paramTimeKey As String
		) As List(Of System.Tuple(Of Integer, String, String))
			Try
				Dim weekDataList As New List(Of System.Tuple(Of Integer, String, String))
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = "
						WITH PreviousWeek AS (
							SELECT TOP 1 weekStartDate
							FROM XFC_TRS_AUX_Date
							WHERE CONVERT(VARCHAR(8), weekStartDate, 112) < @timekey
							AND fulldate = weekStartDate
							ORDER BY weekStartDate DESC
						)
						SELECT TOP 9 d.weekNumber, d.year, CONVERT(VARCHAR(8), d.weekStartDate, 112) AS weekStartDateKey
						FROM XFC_TRS_AUX_Date d
						CROSS JOIN PreviousWeek pw
						WHERE d.weekStartDate >= pw.weekStartDate
						AND d.fulldate = d.weekStartDate
						ORDER BY d.weekStartDate
					"
					Dim dbParams As New List(Of DbParamInfo) From {New DbParamInfo("timekey", paramTimeKey)}
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, query, dbParams, False)
					
					For Each row As DataRow In dt.Rows
						weekDataList.Add(System.Tuple.Create(
							Convert.ToInt32(row("weekNumber")),
							row("year").ToString(),
							row("weekStartDateKey").ToString()
						))
					Next
				End Using
				
				Return weekDataList
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		''' <summary>
		''' Builds template parameters dictionary based on configuration
		''' </summary>
		Private Function BuildTemplateParameters(
			ByVal si As SessionInfo,
			ByVal config As TemplateConfig,
			ByVal paramYear As Integer,
			ByVal paramWeek As Integer,
			ByVal paramCompany As String,
			ByVal paramEntity As String,
			ByVal paramTimeKey As String,
			ByVal paramStartWeek As DateTime,
			ByVal paramEndWeek As DateTime,
			ByVal paramDateEndOfMonth As DateTime
		) As Dictionary(Of String, Object)
			Try
				Dim params As New Dictionary(Of String, Object)
				
				' 1. Add InfoRows (generic based on config)
				If config.InfoRows IsNot Nothing Then
					For Each infoRow In config.InfoRows
						Select Case infoRow.Value
							Case "Entity"
								params.Add(infoRow.Key, paramEntity)
							Case "StartDate"
								params.Add(infoRow.Key, paramStartWeek.ToString("yyyy-MM-dd"))
							Case "EndDate"
								params.Add(infoRow.Key, paramEndWeek.ToString("yyyy-MM-dd"))
							Case "EndOfMonth"
								params.Add(infoRow.Key, paramDateEndOfMonth.ToString("yyyy-MM-dd"))
						End Select
					Next
				End If
				
				' 2. Add static header cells (if not using week columns)
				If config.HeaderCells IsNot Nothing Then
					Dim endOfMonthNumber As String = paramDateEndOfMonth.ToString("MM")
					For Each headerCell In config.HeaderCells
						Dim cellValue As String = headerCell.Value.Replace("{MM}", endOfMonthNumber)
						params.Add(headerCell.Key, cellValue)
					Next
				End If
				
				' 3. If UseWeekColumns: Get weeks from database and populate
				If config.UseWeekColumns Then
					Dim weekDataList As List(Of System.Tuple(Of Integer, String, String)) = Me.GetWeekColumnsFromDatabase(si, paramTimeKey)
					
					' Build week column names (W10, W11, etc.)
					Dim weekColumns As New List(Of String)
					For Each weekData In weekDataList
						weekColumns.Add($"W{weekData.Item1.ToString("D2")}")
					Next
					
					' Add week headers
					params.Add($"C{config.HeaderRow}", "Actual")
					For i As Integer = 1 To Math.Min(weekColumns.Count - 1, 8)
						Dim colLetter As String = Chr(67 + i)
						params.Add($"{colLetter}{config.HeaderRow}", weekColumns(i))
					Next
					params.Add($"N{config.HeaderRow}", "Year")
					params.Add($"O{config.HeaderRow}", "Entity")
					params.Add($"P{config.HeaderRow}", "Timekey")
					
					' Add week metadata
					params.Add($"C{config.MetadataRow}", $"xfDec#:Actual::0")
					For i As Integer = 1 To Math.Min(weekColumns.Count - 1, 8)
						Dim colLetter As String = Chr(67 + i)
						params.Add($"{colLetter}{config.MetadataRow}", $"xfDec#:{weekColumns(i)}::0")
					Next
					params.Add($"N{config.MetadataRow}", $"xfDec#:year::{paramYear}")
					params.Add($"O{config.MetadataRow}", $"xfText#:entity::{paramEntity}")
					params.Add($"P{config.MetadataRow}", $"xfDec#:TimeKey::{paramTimeKey}")
				End If
				
				' 4. If UseBankRows: Get banks and generate dynamic rows
				If config.UseBankRows Then
					Dim banksWithFlows As List(Of System.Tuple(Of String, String)) = Me.GetBanksByEntity(si, paramCompany)
					
					' Add CashDebtPosition specific metadata
					params.Add($"G{config.MetadataRow}", $"xfText#:Year::{paramYear}")
					params.Add($"H{config.MetadataRow}", $"xfText#:Entity::{paramEntity}")
					params.Add($"I{config.MetadataRow}", $"xfText#:Timekey::{paramTimeKey}")
					
					' Add dynamic bank rows starting from row 11
					Dim dynamicBankRows As Dictionary(Of String, Object) = Me.GenerateDynamicBankRows(banksWithFlows, 11)
					For Each kvp In dynamicBankRows
						params.Add(kvp.Key, kvp.Value)
					Next
				End If
				
				Return params
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		''' <summary>
		''' Generates a Treasury template with centralized logic
		''' </summary>
		Private Function GenerateTreasuryTemplate(
			ByVal si As SessionInfo,
			ByVal config As TemplateConfig,
			ByVal nameValuePairs As Dictionary(Of String, String)
		) As XFSelectionChangedTaskResult
			
			Dim result As New XFSelectionChangedTaskResult()
			result.IsOK = True
			result.ShowMessageBox = False
			
			Try
				' 1. Extract and validate parameters
				Dim paramCompany As String = nameValuePairs("p_company")
				Dim paramYear As Integer = CInt(nameValuePairs("p_year"))
				Dim paramWeek As Integer = CInt(nameValuePairs("p_week"))
				Dim paramUsername As String = si.UserName
				
				If String.IsNullOrEmpty(paramCompany) OrElse paramYear = 0 OrElse paramWeek = 0 Then
					result.IsOK = False
					result.ShowMessageBox = True
					result.Message = "Required parameters missing: Company, Year, and Week are required."
					Return result
				End If
				
				' 2. Get entity name
				Dim paramEntity As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, paramCompany)
				
				' 3. Calculate dynamic parameters
				Dim paramStartWeek As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayDate(si, paramYear.ToString(), paramWeek.ToString())
				Dim paramEndWeek As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayDate(si, paramYear.ToString(), (paramWeek + 4).ToString())
				Dim paramDateEndOfMonth As DateTime = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetEndOfCurrentMonth(si, paramYear.ToString(), paramWeek.ToString())
				Dim paramTimeKey As String = Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetWeekMondayTimeKeyString(si, paramYear.ToString(), paramWeek.ToString())
				
				' 4. Define target file paths
				Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
				Dim targetFinalPath As String = "Import Templates"
				Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
				Dim targetFile As String = String.Format(config.TargetFileNamePattern, paramYear, paramWeek, paramCompany)
				
				' 5. Create folder and clear existing files
				BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
				UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
				
				' 6. Get template file
				Dim objXFFileEx As XFFileEx = BRApi.FileSystem.GetFile(
					si, 
					FileSystemLocation.ApplicationDatabase, 
					config.SourcePath & "/" & config.SourceFile, 
					True, 
					True
				)
				
				If objXFFileEx Is Nothing OrElse objXFFileEx.XFFile Is Nothing Then
					result.IsOK = False
					result.ShowMessageBox = True
					result.Message = $"Template file not found at {config.SourcePath}/{config.SourceFile}"
					Return result
				End If
				
				Dim bytesOfFile As Byte() = objXFFileEx.XFFile.ContentFileBytes
				
				' 7. Build parameters dictionary (generic - no if/else needed)
				Dim parametersDict As Dictionary(Of String, Object) = Me.BuildTemplateParameters(
					si, config, paramYear, paramWeek, paramCompany, 
					paramEntity, paramTimeKey, paramStartWeek, paramEndWeek, paramDateEndOfMonth
				)
				
				' 8. Populate Excel with parameters
				Dim modifiedBytes As Byte() = Me.PopulateExcelTemplate(si, bytesOfFile, parametersDict)
				
				' 9. Create and save the modified file
				Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetFile, targetPath & "/")
				fileInfo.ContentFileExtension = "xlsx"
				fileInfo.ContentFileContainsData = True
				fileInfo.XFFileType = True
				
				Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, modifiedBytes)
				BRApi.FileSystem.InsertOrUpdateFile(si, fileFile)
				
				' 10. Return success
				result.IsOK = True
				result.ShowMessageBox = False
				Return result
				
			Catch ex As Exception
				result.IsOK = False
				result.ShowMessageBox = True
				result.Message = $"Error generating {config.TemplateType} template: {ex.Message}"
				Return result
			End Try
		End Function

		''' <summary>
		''' Copies the Treasury Monitoring XFDoc PPTX to the user's Import Templates folder
		''' with the requested week/year suffix without modifying its content.
		''' </summary>
		Private Function GenerateTreasuryMonitoringCopy(
			ByVal si As SessionInfo,
			ByVal nameValuePairs As Dictionary(Of String, String)
		) As XFSelectionChangedTaskResult
			Dim result As New XFSelectionChangedTaskResult()
			result.IsOK = True
			result.ShowMessageBox = False
			Try
				Dim paramWeekStr As String = If(nameValuePairs IsNot Nothing AndAlso nameValuePairs.ContainsKey("p_week"), nameValuePairs("p_week"), "")
				Dim paramYearStr As String = If(nameValuePairs IsNot Nothing AndAlso nameValuePairs.ContainsKey("p_year"), nameValuePairs("p_year"), "")
				
				Dim paramWeek As Integer
				Dim paramYear As Integer
				If Not Integer.TryParse(paramWeekStr, paramWeek) OrElse Not Integer.TryParse(paramYearStr, paramYear) Then
					result.IsOK = False
					result.ShowMessageBox = True
					result.Message = "Required parameters missing or invalid: Week and Year are required."
					Return result
				End If
				
				Dim paramUsername As String = si.UserName
				Dim targetInitialPath As String = $"Documents/Users/{paramUsername}"
				Dim targetFinalPath As String = "Import Templates"
				Dim targetPath As String = $"{targetInitialPath}/{targetFinalPath}"
				Dim targetFile As String = $"Treasury_Monitoring_{paramWeek}_{paramYear}.pptx"
				
				BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, targetInitialPath, targetFinalPath)
				UTISharedFunctionsBR.DeleteAllFilesInFolder(si, targetPath, FileSystemLocation.ApplicationDatabase)
				
				Dim sourcePath As String = "Documents/Public/Treasury/Treasury_Monitoring.xfdoc.pptx"
				Dim sourceFile As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, sourcePath, True, True)
				
				If sourceFile Is Nothing OrElse sourceFile.XFFile Is Nothing Then
					result.IsOK = False
					result.ShowMessageBox = True
					result.Message = $"Template file not found at {sourcePath}"
					Return result
				End If
				
				Dim bytesOfFile As Byte() = sourceFile.XFFile.ContentFileBytes
				
				Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, targetFile, targetPath & "/")
				fileInfo.ContentFileExtension = "pptx"
				fileInfo.ContentFileContainsData = True
				fileInfo.XFFileType = True
				
				Dim newFile As XFFile = New XFFile(fileInfo, String.Empty, bytesOfFile)
				BRApi.FileSystem.InsertOrUpdateFile(si, newFile)
				
				result.IsOK = True
				result.ShowMessageBox = False
				Return result
			Catch ex As Exception
				result.IsOK = False
				result.ShowMessageBox = True
				result.Message = $"Error generating Treasury Monitoring file: {ex.Message}"
				Return result
			End Try
		End Function
		
		#End Region
		
		#Region "Excel Helper Functions"
		
		''' <summary>
		''' Populates Excel template with dynamic parameters using OpenXML
		''' </summary>
		Private Function PopulateExcelTemplate(ByVal si As SessionInfo, ByVal originalBytes As Byte(), ByVal parametersDict As Dictionary(Of String, Object)) As Byte()
			Try
				Using memoryStream As New MemoryStream(originalBytes)
					Using document As SpreadsheetDocument = SpreadsheetDocument.Open(memoryStream, True)
						Dim worksheetPart As WorksheetPart = document.WorkbookPart.WorksheetParts.FirstOrDefault()
						
						If worksheetPart IsNot Nothing Then
							Dim worksheet As Worksheet = worksheetPart.Worksheet
							
							' Populate each cell with its corresponding value
							For Each kvp In parametersDict
								Me.SetCellValue(document, worksheet, kvp.Key, kvp.Value)
							Next
							
							worksheetPart.Worksheet.Save()
						End If
					End Using
					
					Return memoryStream.ToArray()
				End Using
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException($"Error populating Excel template: {ex.Message}"))
			End Try
		End Function
		
		''' <summary>
		''' Sets a cell value in the Excel worksheet
		''' </summary>
		Private Sub SetCellValue(document As SpreadsheetDocument, worksheet As Worksheet, cellReference As String, cellValue As Object)
			Try
				' Find or create the cell
				Dim cell As Cell = Me.GetCell(worksheet, cellReference)
				
				If cell Is Nothing Then
					cell = Me.CreateCell(worksheet, cellReference)
				End If
				
				' Set the value based on type
				If TypeOf cellValue Is String Then
					cell.CellValue = New CellValue(cellValue.ToString())
					cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
				ElseIf TypeOf cellValue Is DateTime Then
					' Convert DateTime to Excel serial number
					Dim excelDate As Double = CType(cellValue, DateTime).ToOADate()
					cell.CellValue = New CellValue(excelDate.ToString())
					cell.DataType = Nothing
				ElseIf IsNumeric(cellValue) Then
					cell.CellValue = New CellValue(cellValue.ToString())
					cell.DataType = Nothing
				Else
					cell.CellValue = New CellValue(cellValue.ToString())
					cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
				End If
				
			Catch ex As Exception
				Throw New XFException($"Error setting cell value for {cellReference}: {ex.Message}")
			End Try
		End Sub
		
		''' <summary>
		''' Gets a cell from the worksheet by reference
		''' </summary>
		Private Function GetCell(worksheet As Worksheet, cellReference As String) As Cell
			Dim sheetData As SheetData = worksheet.GetFirstChild(Of SheetData)()
			
			For Each row As Row In sheetData.Elements(Of Row)()
				For Each cell As Cell In row.Elements(Of Cell)()
					If cell.CellReference IsNot Nothing AndAlso cell.CellReference.Value = cellReference Then
						Return cell
					End If
				Next
			Next
			
			Return Nothing
		End Function
		
		''' <summary>
		''' Creates a new cell in the worksheet at the correct position
		''' </summary>
		Private Function CreateCell(worksheet As Worksheet, cellReference As String) As Cell
			Dim sheetData As SheetData = worksheet.GetFirstChild(Of SheetData)()
			
			' Extract row number from cell reference
			Dim rowIndex As Integer = CInt(System.Text.RegularExpressions.Regex.Replace(cellReference, "[A-Z]", ""))
			
			' Find or create the row
			Dim targetRow As Row = Nothing
			For Each row As Row In sheetData.Elements(Of Row)()
				If row.RowIndex IsNot Nothing AndAlso row.RowIndex.Value = rowIndex Then
					targetRow = row
					Exit For
				End If
			Next
			
			If targetRow Is Nothing Then
				targetRow = New Row() With {.RowIndex = CUInt(rowIndex)}
				sheetData.Append(targetRow)
			End If
			
			' Create new cell
			Dim newCell As New Cell() With {.CellReference = cellReference}
			
			' Insert cell in the correct position
			Dim insertBeforeCell As Cell = Nothing
			For Each existingCell As Cell In targetRow.Elements(Of Cell)()
				If existingCell.CellReference IsNot Nothing AndAlso String.Compare(existingCell.CellReference.Value, cellReference) > 0 Then
					insertBeforeCell = existingCell
					Exit For
				End If
			Next
			
			If insertBeforeCell IsNot Nothing Then
				targetRow.InsertBefore(newCell, insertBeforeCell)
			Else
				targetRow.Append(newCell)
			End If
			
			Return newCell
		End Function
		
		''' <summary>
		''' Helper function to check if a value is numeric
		''' </summary>
		Private Function IsNumeric(value As Object) As Boolean
			If value Is Nothing OrElse value Is DBNull.Value Then Return False
			Return TypeOf value Is Integer OrElse
				   TypeOf value Is Double OrElse
				   TypeOf value Is Decimal OrElse
				   TypeOf value Is Long OrElse
				   TypeOf value Is Short OrElse
				   Double.TryParse(value.ToString(), Nothing)
		End Function
		
		#End Region
		
		#Region "Database Helper Functions"
		
		''' <summary>
		''' Gets the list of active banks with their flows for a specific entity
		''' </summary>
		Private Function GetBanksByEntity(ByVal si As SessionInfo, ByVal entityName As String) As List(Of System.Tuple(Of String, String))
			Try
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Dim query As String = "
						SELECT Bank, Flow 
						FROM XFC_TRS_Master_Banks 
						WHERE entity_id = @entityName AND active = 1
						ORDER BY Bank, Flow
					"
					Dim dbParams As New List(Of DbParamInfo) From {
						New DbParamInfo("entityName", entityName)
					}
					
					Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, query, dbParams, False)
					
					Dim banksWithFlows As New List(Of System.Tuple(Of String, String))
					For Each row As DataRow In dt.Rows
						banksWithFlows.Add(System.Tuple.Create(row("Bank").ToString(), row("Flow").ToString()))
					Next
					
					Return banksWithFlows
				End Using
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		''' <summary>
		''' Generates dynamic dictionary entries for banks grouped by Account type
		''' </summary>
		Private Function GenerateDynamicBankRows(ByVal banksWithFlows As List(Of System.Tuple(Of String, String)), ByVal startingRow As Integer) As Dictionary(Of String, Object)
			Try
				Dim rowDict As New Dictionary(Of String, Object)
				Dim currentRow As Integer = startingRow
				
				' Separate banks by flow type
				Dim banksWithPlusFlows As New List(Of System.Tuple(Of String, String))
				Dim banksWithMinusFlows As New List(Of System.Tuple(Of String, String))
				
				For Each bankFlow In banksWithFlows
					If bankFlow.Item2.Contains("+") Then
						banksWithPlusFlows.Add(bankFlow)
					ElseIf bankFlow.Item2.Contains("-") Then
						banksWithMinusFlows.Add(bankFlow)
					End If
				Next
				
				' Sort by Flow: INTernal before EXTernal
				Dim flowComparer As Comparison(Of System.Tuple(Of String, String)) = Function(x, y)
					If x.Item2.StartsWith("INT") And y.Item2.StartsWith("EXT") Then Return -1
					If x.Item2.StartsWith("EXT") And y.Item2.StartsWith("INT") Then Return 1
					Return String.Compare(x.Item1, y.Item1)
				End Function
				
				banksWithPlusFlows.Sort(flowComparer)
				banksWithMinusFlows.Sort(flowComparer)
				
				' 1. CASH AND FINANCING BALANCE
				For Each bankFlow In banksWithPlusFlows
					rowDict.Add($"A{currentRow}", ACCOUNT_CASH_FINANCING)
					rowDict.Add($"B{currentRow}", bankFlow.Item2)
					rowDict.Add($"C{currentRow}", bankFlow.Item1)
					currentRow += 1
				Next
				
				' 2. FINANCING - USED LINES
				For Each bankFlow In banksWithMinusFlows
					rowDict.Add($"A{currentRow}", ACCOUNT_USED_LINES)
					rowDict.Add($"B{currentRow}", bankFlow.Item2)
					rowDict.Add($"C{currentRow}", bankFlow.Item1)
					currentRow += 1
				Next
				
				' 3. FINANCING - AVAILABLE LINES
				For Each bankFlow In banksWithMinusFlows
					rowDict.Add($"A{currentRow}", ACCOUNT_AVAILABLE_LINES)
					rowDict.Add($"B{currentRow}", bankFlow.Item2)
					rowDict.Add($"C{currentRow}", bankFlow.Item1)
					currentRow += 1
				Next
				
				Return rowDict
			Catch ex As Exception
				Throw New XFException($"Error generating dynamic bank rows: {ex.Message}")
			End Try
		End Function
		
		#End Region
		
	End Class
End Namespace