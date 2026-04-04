#Region "Imports"
Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database
#End Region ' Imports

Namespace OneStream.BusinessRule.Extender.Workflow_Year_Autoload
	Public Class MainClass

#Region "Main"

' =====================================================================================
' Business Rule: Workflow_Year_Autoload
' Description: Executes Import, Validate and Load Cube workflow steps for all months
'              of a given year. Supports single entity or ALL AutoLoad entities.
'              (Process Cube skipped)
'
' Parameters (passed via args.NameValuePairs):
'   - WorkflowProfile: The workflow profile suffix (e.g., "2_Automatic_Import_SAP")
'   - Entity: (Optional) The entity/country name (e.g., "Portugal").
'             If empty or "ALL", processes all workflows with Text2='AutoLoad'.
'   - Scenario: (Optional) The scenario name, defaults to "Actual"
'   - Year: (Optional) If not provided, extracts from global workflow time
'   - Month: (Optional) Up to which month to process (1-12), defaults to 12
'
' Examples:
'   Single entity:  WorkflowProfile=2_Automatic_Import_SAP,Entity=Portugal,Year=2025
'   All entities:   WorkflowProfile=2_Automatic_Import_SAP,Year=2025
'   First semester: WorkflowProfile=2_Automatic_Import_SAP,Year=2025,Month=6
' =====================================================================================

Dim ActiveLogs As Boolean = True
Dim IncludeiScala As Boolean = False ' Set to True to include iScala workflows

Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
	Try
		Select Case args.FunctionType
			Case Is = ExtenderFunctionType.Unknown, ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep

				' Parse parameters from NameValuePairs
				Dim workflowProfile As String = Me.GetParameterValue(args, "WorkflowProfile", "")
				Dim entity As String = Me.GetParameterValue(args, "Entity", "")
				Dim scenario As String = Me.GetParameterValue(args, "Scenario", "Actual")
				Dim yearParam As String = Me.GetParameterValue(args, "Year", "")
				Dim monthParam As String = Me.GetParameterValue(args, "Month", "12")

				' Validate required parameters
				If String.IsNullOrEmpty(workflowProfile) Then
					Throw New Exception("Parameter 'WorkflowProfile' is required. Example: 2_Automatic_Import_SAP")
				End If

				' Get Year: from parameter or from global workflow time
				Dim year As String
				If Not String.IsNullOrEmpty(yearParam) Then
					year = yearParam
				Else
					Dim globalTime As String = BRApi.Workflow.General.GetGlobalTime(si)
					year = Left(globalTime, 4)
				End If

				' Get Month: determines up to which month to process (1-12)
				Dim maxMonth As Integer = 12
				If Not String.IsNullOrEmpty(monthParam) Then
					If Integer.TryParse(monthParam, maxMonth) Then
						If maxMonth < 1 Then maxMonth = 1
						If maxMonth > 12 Then maxMonth = 12
					Else
						maxMonth = 12
					End If
				End If

				' Build list of profiles to process
				Dim profilesToProcess As New List(Of String)()
				Dim isAllEntities As Boolean = String.IsNullOrEmpty(entity) OrElse entity.Equals("ALL", StringComparison.OrdinalIgnoreCase)

				If isAllEntities Then
					' Discover all AutoLoad workflow profiles (same pattern as Workflow_Autoload)
					profilesToProcess = Me.GetAutoLoadWorkflowProfiles(si, workflowProfile)
					If profilesToProcess.Count = 0 Then
						BRApi.ErrorLog.LogMessage(si, "INFO: Workflow_Year_Autoload - No AutoLoad workflows found matching profile '" & workflowProfile & "'")
						Return Nothing
					End If
					BRApi.ErrorLog.LogMessage(si, "INFO: Workflow_Year_Autoload - ALL entities mode. Found " & profilesToProcess.Count.ToString() & " profiles: " & String.Join(", ", profilesToProcess))
				Else
					' Single entity mode
					profilesToProcess.Add(entity & "." & workflowProfile)
				End If

				BRApi.ErrorLog.LogMessage(si, "INFO: Workflow_Year_Autoload - Starting execution. Year: " & year & ", Months: 1-" & maxMonth.ToString() & ", Scenario: " & scenario & ", Profiles: " & profilesToProcess.Count.ToString())

				' Process each profile
				Dim allSummaries As New List(Of String)()
				For Each fullProfileName As String In profilesToProcess
					Dim profileSummary As String = Me.ProcessProfileForYear(si, fullProfileName, scenario, year, maxMonth)
					allSummaries.Add(profileSummary)
				Next

				' Final summary
				Dim finalSummary As String = String.Join(Environment.NewLine & "========================================" & Environment.NewLine, allSummaries)
				BRApi.ErrorLog.LogMessage(si, "INFO: Workflow_Year_Autoload - All processing complete.")

				Return finalSummary

		End Select

		Return Nothing

	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function

''' <summary>
''' Processes a single workflow profile for all months of a given year.
''' </summary>
Private Function ProcessProfileForYear(ByVal si As SessionInfo, ByVal fullProfileName As String, ByVal scenario As String, ByVal year As String, ByVal maxMonth As Integer) As String
	Dim successfulMonths As New List(Of String)
	Dim failedMonths As New List(Of String)
	Dim skippedMonths As New List(Of String)

	Try
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Processing profile: " & fullProfileName & ", Year: " & year & ", Months: 1-" & maxMonth.ToString())

		' OPTIMIZATION: Get entity codes ONCE
		Dim entityCodes As String = Me.GetWorkflowEntityCodes(si, fullProfileName)
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Entity codes for " & fullProfileName & ": " & entityCodes)

		' OPTIMIZATION: Get ALL periods with data in ONE query
		Dim periodsWithData As HashSet(Of String) = Me.GetPeriodsWithData(si, entityCodes, year)
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Periods with data for " & fullProfileName & ": " & String.Join(", ", periodsWithData))

		' Execute for months 1 to maxMonth
		For month As Integer = 1 To maxMonth
			Dim timeName As String = year & "M" & month.ToString()

			Try
				If Not periodsWithData.Contains(timeName) Then
					skippedMonths.Add(timeName & " (no data in DWH)")
					If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "SKIPPED: " & fullProfileName & " - " & timeName & " - No data available in DWH table")
					Continue For
				End If

				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Processing " & fullProfileName & " for " & timeName)

				Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, fullProfileName, scenario, timeName)

				If wfClusterPk IsNot Nothing Then
					Dim completed As Boolean = Me.ExecuteWorkflowSteps(si, wfClusterPk)

					If completed Then
						successfulMonths.Add(timeName)
						If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "SUCCESS: " & fullProfileName & " - " & timeName & " completed successfully")
					Else
						failedMonths.Add(timeName)
						If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "FAILED: " & fullProfileName & " - " & timeName & " did not complete successfully")
					End If
				Else
					failedMonths.Add(timeName & " (WF not found)")
					If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: Workflow not found for " & fullProfileName & " - " & timeName)
				End If

			Catch exMonth As Exception
				failedMonths.Add(timeName & " (" & exMonth.Message & ")")
				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: " & fullProfileName & " - " & timeName & " - " & exMonth.Message)
			End Try
		Next

	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, "ERROR: ProcessProfileForYear failed for " & fullProfileName & ": " & ex.Message)
	End Try

	' Build summary for this profile
	Dim summary As String = "Execution Summary for " & fullProfileName & " - Year " & year & Environment.NewLine
	summary &= "Successful (" & successfulMonths.Count.ToString() & "): " & String.Join(", ", successfulMonths) & Environment.NewLine
	summary &= "Skipped (" & skippedMonths.Count.ToString() & "): " & String.Join(", ", skippedMonths) & Environment.NewLine
	summary &= "Failed (" & failedMonths.Count.ToString() & "): " & String.Join(", ", failedMonths)

	BRApi.ErrorLog.LogMessage(si, summary)
	Return summary
End Function

#End Region 'Main

#Region "Helper Functions"

''' <summary>
''' Gets all workflow profiles marked for AutoLoad (Text2='AutoLoad' and Active=1),
''' filtered by the specified workflow profile suffix.
''' </summary>
''' <param name="workflowProfileSuffix">Workflow profile suffix to match (e.g., "2_Automatic_Import_SAP")</param>
''' <returns>List of full workflow profile names (e.g., "Portugal.2_Automatic_Import_SAP")</returns>
Private Function GetAutoLoadWorkflowProfiles(ByVal si As SessionInfo, ByVal workflowProfileSuffix As String) As List(Of String)
	Dim profiles As New List(Of String)()
	
	Try
		Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			Dim sapFilter As String = If(IncludeiScala, "", $"AND wpfh.ProfileName LIKE '%{workflowProfileSuffix.Replace("'", "''")}'") 
			
			Dim sql As String = $"
				SELECT wpfh.ProfileName
				FROM dbo.WorkflowProfileHierarchy wpfh
				INNER JOIN dbo.WorkflowProfileAttributes wpa ON wpfh.ProfileKey = wpa.ProfileKey
				WHERE wpfh.CubeName = 'Cube_100_Group'
				  AND wpfh.ProfileType = '50'
				  AND wpa.ScenarioTypeID = '0'
				  {sapFilter}
				GROUP BY wpfh.ProfileName, wpfh.ProfileKey
				HAVING 
				  SUM(CASE WHEN wpa.AttributeIndex = '20000' AND wpa.ProfileAttributeValue = 'AutoLoad' THEN 1 ELSE 0 END) > 0
				  AND SUM(CASE WHEN wpa.AttributeIndex = '1300' AND wpa.ProfileAttributeValue = '1' THEN 1 ELSE 0 END) > 0
				ORDER BY wpfh.ProfileName"
			
			Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, sql, Nothing, Nothing, False)
			
			For Each row As DataRow In dt.Rows
				Dim profileName As String = row("ProfileName").ToString()
				If Not String.IsNullOrEmpty(profileName) Then
					profiles.Add(profileName)
				End If
			Next
			
			If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, $"INFO: GetAutoLoadWorkflowProfiles - Suffix: {workflowProfileSuffix}, Found {profiles.Count} profiles: {String.Join(", ", profiles)}")
		End Using
	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, "ERROR: GetAutoLoadWorkflowProfiles failed: " & ex.Message)
	End Try
	
	Return profiles
End Function

''' <summary>
''' Gets the entity codes (Text1 attribute) for a workflow profile. 
''' Cached to avoid repeated queries for the same profile.
''' </summary>
''' <param name="fullProfileName">Full workflow profile name (e.g., "Poland.2_Automatic_Import_SAP")</param>
''' <returns>Comma-separated entity codes or empty string if not found</returns>
Private Function GetWorkflowEntityCodes(ByVal si As SessionInfo, ByVal fullProfileName As String) As String
	Try
		' Use OneStream API to get Workflow Profile attributes (Better approach than SQL)
		Dim wpInfo As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, fullProfileName) 
		
		If wpInfo Is Nothing Then
			If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "WARNING: GetWorkflowEntityCodes - Profile not found: " & fullProfileName)
			Return ""
		End If
		
		' Get Text1 attribute value for Actual scenario (ScenarioTypeId.Actual = 0)
		Dim text1Value As String = wpInfo.GetAttributeValue(ScenarioTypeId.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text1)
		
		If String.IsNullOrEmpty(text1Value) Then
			If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "WARNING: GetWorkflowEntityCodes - No Text1 attribute found for profile: " & fullProfileName)
			Return ""
		End If
		
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: GetWorkflowEntityCodes - Profile: " & fullProfileName & ", Text1: " & text1Value)
		Return text1Value
		
	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, "WARNING: GetWorkflowEntityCodes failed for " & fullProfileName & ": " & ex.Message)
		Return ""
	End Try
End Function

''' <summary>
''' Gets all periods that have data in DWH for the specified entities and year.
''' Returns a HashSet for O(1) lookup performance.
''' Single query replaces 12 individual queries (one per month).
''' </summary>
''' <param name="entityCodes">Comma-separated entity codes from workflow Text1 attribute</param>
''' <param name="year">Year to check (e.g., "2025")</param>
''' <returns>HashSet of periods with data (e.g., "2025M1", "2025M6")</returns>
Private Function GetPeriodsWithData(ByVal si As SessionInfo, ByVal entityCodes As String, ByVal year As String) As HashSet(Of String)
	Dim periodsWithData As New HashSet(Of String)()
	
	Try
		If String.IsNullOrEmpty(entityCodes) Then
			' Validation aligned with Workflow_Autoload: If no entity codes, skip (do not identify any period with data)
			If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "WARNING: GetPeriodsWithData - No entity codes provided. Skipping execution for all months (No Data).")
			Return periodsWithData
		End If
		
		Dim entityInClause As String = BuildEntityInClause(entityCodes)
		
		Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			' Single query to get ALL periods with data for the year
			Dim sql As String = $"
				SELECT DISTINCT [Time] 
				FROM XFC_DWH_FIN_MIX 
				WHERE [Entity] IN ({entityInClause}) 
				  AND [Time] LIKE '{year}M%'"
			
			Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, sql, Nothing, Nothing, False)
			
			For Each row As DataRow In dt.Rows
				Dim period As String = row("Time").ToString()
				If Not String.IsNullOrEmpty(period) Then
					periodsWithData.Add(period)
				End If
			Next
			
			If ActiveLogs Then
				BRApi.ErrorLog.LogMessage(si, "INFO: GetPeriodsWithData - Year: " & year & ", Entities: " & entityCodes & ", PeriodsFound: " & String.Join(", ", periodsWithData))
			End If
		End Using
	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, "WARNING: GetPeriodsWithData failed for year " & year & ": " & ex.Message)
		' Validation aligned with Workflow_Autoload: On error, assume NO DATA to prevent running blindly
		' Previous behavior was: For month As Integer = 1 To 12 : periodsWithData.Add(...) : Next
	End Try
	
	Return periodsWithData
End Function

''' <summary>
''' Checks if there is data in the DWH table for the specified workflow profile and time period.
''' Uses the Text1 attribute from the Workflow Profile to get the entity codes (same logic as CON_DWH_FIN_MIX).
''' </summary>
''' <param name="fullProfileName">Full workflow profile name (e.g., "Poland.2_Automatic_Import_SAP")</param>
''' <param name="scenario">Scenario name (e.g., "Actual")</param>
''' <param name="timePeriod">Time period in format YYYYMn (e.g., "2025M1", "2025M12")</param>
''' <returns>True if data exists, False otherwise</returns>
Private Function HasDataInDWH(ByVal si As SessionInfo, ByVal fullProfileName As String, ByVal scenario As String, ByVal timePeriod As String) As Boolean
	Try
		Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			' First, get the entity codes from Text1 attribute (AttributeIndex = 19000) of the Workflow Profile
			' ScenarioTypeID = 0 is Actual
			Dim sqlGetText1 As String = $"
				SELECT wpa.ProfileAttributeValue
				FROM dbo.WorkflowProfileHierarchy wpfh
				INNER JOIN dbo.WorkflowProfileAttributes wpa ON wpfh.ProfileKey = wpa.ProfileKey
				WHERE wpfh.ProfileName = '{fullProfileName.Replace("'", "''")}'
				  AND wpa.ScenarioTypeID = 0
				  AND wpa.AttributeIndex = 19000"
			
			Dim dtText1 As DataTable = BRApi.Database.GetDataTable(dbConn, sqlGetText1, Nothing, Nothing, False)
			
			If dtText1.Rows.Count = 0 Then
				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "WARNING: HasDataInDWH - No Text1 attribute found for profile: " & fullProfileName)
				Return True ' Allow import attempt if no entity codes configured
			End If
			
			Dim entityCodes As String = dtText1.Rows(0)("ProfileAttributeValue").ToString()
			
			If String.IsNullOrEmpty(entityCodes) Then
				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "WARNING: HasDataInDWH - Empty Text1 value for profile: " & fullProfileName)
				Return True ' Allow import attempt if no entity codes configured
			End If
			
			' Build the IN clause from entity codes (comma-separated)
			Dim entityInClause As String = BuildEntityInClause(entityCodes)
			
			' Query to count rows for these entities and time
			Dim sql As String = $"SELECT COUNT(*) as RecordCount FROM XFC_DWH_FIN_MIX WHERE [Entity] IN ({entityInClause}) AND [Time] = '{timePeriod.Replace("'", "''")}'"
			
			Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, sql, Nothing, Nothing, False)
			
			Dim recordCount As Integer = 0
			If dt.Rows.Count > 0 Then
				recordCount = Convert.ToInt32(dt.Rows(0)("RecordCount"))
			End If
			
			Dim hasData As Boolean = (recordCount > 0)
			
			If ActiveLogs Then
				BRApi.ErrorLog.LogMessage(si, "INFO: HasDataInDWH - Profile: " & fullProfileName & ", Entities: " & entityCodes & ", Time: " & timePeriod & ", RecordCount: " & recordCount.ToString() & ", HasData: " & hasData.ToString())
			End If
			
			Return hasData
		End Using
	Catch ex As Exception
		' If there's an error checking, log it and return True to attempt the import anyway
		BRApi.ErrorLog.LogMessage(si, "WARNING: HasDataInDWH check failed for " & fullProfileName & "/" & timePeriod & ": " & ex.Message)
		Return True
	End Try
End Function

''' <summary>
''' Builds the IN clause for Entity filter from comma-separated string
''' </summary>
Private Function BuildEntityInClause(ByVal entityList As String) As String
	If String.IsNullOrEmpty(entityList) Then
		Return "''"
	End If
	
	Dim elements As String() = entityList.Split(New Char() {","c}, StringSplitOptions.RemoveEmptyEntries)
	Dim sb As New System.Text.StringBuilder()
	
	For i As Integer = 0 To elements.Length - 1
		If i > 0 Then sb.Append(", ")
		sb.Append("'")
		sb.Append(elements(i).Trim().Replace("'", "''"))
		sb.Append("'")
	Next
	
	Return sb.ToString()
End Function

''' <summary>
''' Gets a parameter value from the NameValuePairs
''' </summary>
Private Function GetParameterValue(ByVal args As ExtenderArgs, ByVal paramName As String, ByVal defaultValue As String) As String
	Try
		If args.NameValuePairs IsNot Nothing AndAlso args.NameValuePairs.ContainsKey(paramName) Then
			Return args.NameValuePairs(paramName).ToString()
		End If
	Catch
	End Try
	Return defaultValue
End Function

''' <summary>
''' Executes the workflow steps: Import, Validate and Load Cube (Process skipped)
''' </summary>
Private Function ExecuteWorkflowSteps(ByVal si As SessionInfo, ByVal wfClusterPk As WorkflowUnitClusterPk) As Boolean
	Dim emptyByteArray As Byte() = New Byte() {}
	Dim wfDescription As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, wfClusterPk)

	Try
		' =====================================================
		' STEP 1: IMPORT (Parse and Transform)
		' =====================================================
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Step 1 - Import - Starting for " & wfDescription)

		Dim impProcessInfo As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, wfClusterPk, "", emptyByteArray, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, False)
		If impProcessInfo.Status <> WorkflowStatusTypes.Completed Then
			BRApi.ErrorLog.LogMessage(si, "ERROR: " & wfDescription & " - Import step did not complete. Status: " & impProcessInfo.Status.ToString())
			Return False
		End If
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Step 1 - Import - Completed for " & wfDescription)

		' =====================================================
		' STEP 2: VALIDATE (Transformation + Intersections)
		' =====================================================
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Step 2 - Validate - Starting for " & wfDescription)

		' Validate Transformation (Mapping)
		Dim valTranProcessInfo As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfClusterPk, True)
		If valTranProcessInfo.Status <> WorkflowStatusTypes.Completed Then
			BRApi.ErrorLog.LogMessage(si, "ERROR: " & wfDescription & " - Validate Transformation did not complete. Status: " & valTranProcessInfo.Status.ToString())
			Return False
		End If

		' Validate Intersections
		Dim valIntersectProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfClusterPk, True)
		If valIntersectProcessInfo.Status <> WorkflowStatusTypes.Completed Then
			BRApi.ErrorLog.LogMessage(si, "ERROR: " & wfDescription & " - Validate Intersections did not complete. Status: " & valIntersectProcessInfo.Status.ToString())
			Return False
		End If
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Step 2 - Validate - Completed for " & wfDescription)

		' =====================================================
		' STEP 3: LOAD CUBE
		' =====================================================
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Step 3 - Load Cube - Starting for " & wfDescription)

		Dim lcProcessInfo = BRApi.Import.Process.LoadCube(si, wfClusterPk)
		If lcProcessInfo.Status <> WorkflowStatusTypes.Completed Then
			BRApi.ErrorLog.LogMessage(si, "ERROR: " & wfDescription & " - Load Cube did not complete. Status: " & lcProcessInfo.Status.ToString())
			Return False
		End If
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Step 3 - Load Cube - Completed for " & wfDescription)

		' =====================================================
		' STOP HERE - Do not Process Cube
		' =====================================================
		If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Steps Import, Validate and Load Cube completed for " & wfDescription & ". Skipping Process Cube step.")

		' All steps completed successfully
		Return True

	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, "ERROR: " & wfDescription & " - Exception: " & ex.Message)
		Return False
	End Try
End Function

#End Region 'Helper Functions

	End Class
End Namespace