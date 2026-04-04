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

Namespace OneStream.BusinessRule.Extender.Workflow_Autoload
	Public Class MainClass

#Region "Main"

' =====================================================================================
' Business Rule: Workflow_Autoload
' Description: Automatically executes Import, Validate, and Load workflow steps
'              for all workflows marked with Text2="AutoLoad" for the current global time.
'
' Features:
'   - Reads GlobalTime automatically if no Time parameter provided
'   - Filters workflows by Text2="AutoLoad" and Active=1
'   - Supports Country parameter for parallel execution via separate DM Sequences
'   - Checks DWH data availability before executing (optimization)
'   - Executes 3 steps: Import, Validate, Load Cube (NO Process Cube)
'   - Supports both SAP and iScala workflows
'   - Provides detailed execution summary
'   - Sends error email notifications
'
' Parameters (optional, passed via args.NameValuePairs):
'   - Scenario: (Optional) The scenario name, defaults to "Actual"
'   - Time: (Optional) Override global time (e.g., "2025M6"). If not provided, uses GetGlobalTime.
'   - Country: (Optional) Filter by country prefix (e.g., "Portugal", "Italy", "Poland").
'             If empty or "ALL", processes all AutoLoad workflows (default behavior).
'             Use this to create parallel DM Sequences per country.
'
' Example: Scenario=Actual,Time=2025M1
' Parallel Example: Create separate DM Sequences with:
'   Sequence 1: Country=Portugal
'   Sequence 2: Country=Italy
'   Sequence 3: Country=Poland
' =====================================================================================

Dim ActiveLogs As Boolean = True
Dim IncludeiScala As Boolean = False ' Set to True to include iScala workflows

Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
	Try
		Select Case args.FunctionType
			Case Is = ExtenderFunctionType.Unknown, ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep

				' Parse parameters
				Dim scenario As String = Me.GetParameterValue(args, "Scenario", "Actual")
				Dim timeParam As String = Me.GetParameterValue(args, "Time", "")
				Dim countryParam As String = Me.GetParameterValue(args, "Country", "ALL")
				
				' If no Time parameter provided, use Global Time (same as AQS_Autoload_Workflow)
				Dim wfTime As String
				If String.IsNullOrEmpty(timeParam) Then
					wfTime = BRApi.Workflow.General.GetGlobalTime(si)
				Else
					wfTime = timeParam
				End If

				BRApi.ErrorLog.LogMessage(si, $"INFO: Workflow_Autoload - Starting Execution. Time: {wfTime}, Scenario: {scenario}, Country: {countryParam}")

				' 1. Identify Workflows (filtered by Country if specified)
				Dim wfProfiles As List(Of String) = Me.GetAutoLoadWorkflowProfiles(si, countryParam)

				If wfProfiles Is Nothing OrElse wfProfiles.Count = 0 Then
					BRApi.ErrorLog.LogMessage(si, "INFO: Workflow_Autoload - No workflows found with Text2='AutoLoad'")
					Return Nothing
				End If

				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Found " & wfProfiles.Count.ToString() & " workflows marked for AutoLoad")

				' 2. Pre-fetch Entity Codes (Optimized: Get Once)
				Dim profileEntityCodes As New Dictionary(Of String, String)()
				For Each profileName As String In wfProfiles
					Dim entityCodes As String = Me.GetWorkflowEntityCodes(si, profileName)
					profileEntityCodes(profileName) = entityCodes
				Next

				' 3. Execute for the Time Period
				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: --- Processing Period: " & wfTime & " ---")
				Me.ProcessTimePeriod(si, args, wfTime, scenario, wfProfiles, profileEntityCodes)

				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Workflow_Autoload - Execution completed.")

		End Select

		Return Nothing

	Catch ex As Exception
		Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
	End Try
End Function

''' <summary>
''' Executes the AutoLoad logic for a single specific Time period
''' </summary>
Private Sub ProcessTimePeriod(ByVal si As SessionInfo, ByVal args As ExtenderArgs, ByVal wfTime As String, ByVal scenario As String, ByVal wfProfiles As List(Of String), ByVal profileEntityCodes As Dictionary(Of String, String))
	Dim successfulWFs As New List(Of String)
	Dim failedWFs As New List(Of String)
	Dim skippedWFs As New List(Of String)
	
	Try
		' A. Check Data Availability (Per Period)
		Dim profilePeriodsWithData As New Dictionary(Of String, Boolean)()
		For Each profileName As String In wfProfiles
			Dim entityCodes As String = profileEntityCodes(profileName)
			Dim hasData As Boolean = Me.HasDataInDWH(si, entityCodes, wfTime, profileName)
			profilePeriodsWithData(profileName) = hasData
		Next

		' B. Execute Workflows
		For Each profileName As String In wfProfiles
			Try
				' Check if there is data in DWH for this workflow before executing
				If Not profilePeriodsWithData(profileName) Then
					skippedWFs.Add(profileName & " (no data in DWH)")
					' If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "SKIPPED: " & profileName & " - " & wfTime & " - No data available in DWH table")
					Continue For
				End If

				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Processing " & profileName & " for " & wfTime)

				' Get the WorkflowUnitClusterPk
				Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, profileName, scenario, wfTime)

				If wfClusterPk IsNot Nothing Then
					' Execute Import, Validate, Load, Process
					Dim completed As Boolean = Me.ExecuteWorkflowSteps(si, wfClusterPk)

					If completed Then
						successfulWFs.Add(profileName)
						' If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "SUCCESS: " & profileName & " - " & wfTime & " completed successfully")
					Else
						failedWFs.Add(profileName)
						If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "FAILED: " & profileName & " - " & wfTime & " did not complete successfully")
					End If
				Else
					failedWFs.Add(profileName & " (WF not found)")
					If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: Workflow not found for " & profileName & " - " & wfTime)
				End If

			Catch exWF As Exception
				failedWFs.Add(profileName & " (" & exWF.Message & ")")
				If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: " & profileName & " - " & wfTime & " - " & exWF.Message)
			End Try
		Next

		' C. Log Summary for this Period
		Dim summary As String = "Execution Summary for AutoLoad - Time: " & wfTime & Environment.NewLine
		summary &= "Total Workflows: " & wfProfiles.Count.ToString() & Environment.NewLine
		summary &= "Successful (" & successfulWFs.Count.ToString() & "): " & String.Join(", ", successfulWFs) & Environment.NewLine
		summary &= "Skipped (" & skippedWFs.Count.ToString() & "): " & String.Join(", ", skippedWFs) & Environment.NewLine
		summary &= "Failed (" & failedWFs.Count.ToString() & "): " & String.Join(", ", failedWFs)

		BRApi.ErrorLog.LogMessage(si, summary)

		' Send error email if there were failures
		' UNCOMMENT THE FOLLOWING LINES TO ENABLE EMAIL NOTIFICATIONS:
		' If failedWFs.Count > 0 Then
		' 	SendErrorEmailHTML(si, args, "ERROR: Autoload for Workflow(s): " & String.Join("; ", failedWFs))
		' End If
		
	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, $"ERROR Processing Period {wfTime}: {ex.Message}")
	End Try
End Sub

#End Region 'Main

#Region "Helper Functions"

''' <summary>
''' Gets all workflow profiles that are marked for AutoLoad (Text2='AutoLoad' and Active=1)
''' </summary>
''' <returns>List of full workflow profile names</returns>
Private Function GetAutoLoadWorkflowProfiles(ByVal si As SessionInfo, Optional ByVal countryFilter As String = "ALL") As List(Of String)
	Dim profiles As New List(Of String)()
	
	Try
		Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			' Query to get all workflows with Text2='AutoLoad' and Active=1
			' Text2 = AttributeIndex 20000
			' Active = AttributeIndex 1300, Value = 1
			' If IncludeiScala=False, only SAP workflows are processed
			' If IncludeiScala=True, all workflows (SAP + iScala) are processed
			' Country filter: filters by ProfileName prefix (e.g., 'Portugal%')
			Dim sapFilter As String = If(IncludeiScala, "", "AND wpfh.ProfileName LIKE '%2_Automatic_Import_SAP'")
			
			' Country filter: when specified, only profiles starting with that country name
			Dim countryFilterSQL As String = ""
			If Not String.IsNullOrEmpty(countryFilter) AndAlso Not countryFilter.Equals("ALL", StringComparison.OrdinalIgnoreCase) Then
				countryFilterSQL = $"AND wpfh.ProfileName LIKE '{countryFilter.Replace("'", "''").Trim()}%'"
			End If
			
			Dim sql As String = $"
				SELECT wpfh.ProfileName
				FROM dbo.WorkflowProfileHierarchy wpfh
				INNER JOIN dbo.WorkflowProfileAttributes wpa ON wpfh.ProfileKey = wpa.ProfileKey
				WHERE wpfh.CubeName = 'Cube_100_Group'
				  AND wpfh.ProfileType = '50'
				  AND wpa.ScenarioTypeID = '0'
				  {sapFilter}
				  {countryFilterSQL}
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
			
			If ActiveLogs Then
				Dim filterDesc As String = If(String.IsNullOrEmpty(countryFilter) OrElse countryFilter.Equals("ALL", StringComparison.OrdinalIgnoreCase), "ALL countries", $"Country={countryFilter}")
				BRApi.ErrorLog.LogMessage(si, $"INFO: GetAutoLoadWorkflowProfiles - Filter: {filterDesc}, Found {profiles.Count} profiles: {String.Join(", ", profiles)}")
			End If
		End Using
	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, "ERROR: GetAutoLoadWorkflowProfiles failed: " & ex.Message)
	End Try
	
	Return profiles
End Function

''' <summary>
''' Gets the entity codes (Text1 attribute) for a workflow profile using OneStream API.
''' </summary>
''' <param name="fullProfileName">Full workflow profile name (e.g., "Poland.2_Automatic_Import_SAP")</param>
''' <returns>Comma-separated entity codes or empty string if not found</returns>
Private Function GetWorkflowEntityCodes(ByVal si As SessionInfo, ByVal fullProfileName As String) As String
	Try
		' Use OneStream API to get Workflow Profile attributes (same approach as Connector)
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
''' Checks if there is data in the DWH table for the specified entity codes and time period.
''' </summary>
''' <param name="entityCodes">Comma-separated entity codes from workflow Text1 attribute</param>
''' <param name="timePeriod">Time period in format YYYYMn (e.g., "2025M1", "2025M12")</param>
''' <param name="profileName">Workflow profile name for logging purposes</param>
''' <returns>True if data exists, False otherwise</returns>
Private Function HasDataInDWH(ByVal si As SessionInfo, ByVal entityCodes As String, ByVal timePeriod As String, ByVal profileName As String) As Boolean
	Try
		' If Text1 is empty, we cannot determine which entities to check - skip this workflow
		If String.IsNullOrEmpty(entityCodes) Then
			If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "WARNING: HasDataInDWH - No entity codes (Text1) found for profile: " & profileName & " - skipping")
			Return False
		End If
		
		Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			' Search by exact entity codes from Text1
			Dim entityInClause As String = BuildEntityInClause(entityCodes)
			Dim sql As String = $"
				SELECT TOP 1 1 
				FROM XFC_DWH_FIN_MIX 
				WHERE [Entity] IN ({entityInClause}) 
				  AND [Time] = '{timePeriod.Replace("'", "''")}'"
			
			Dim dt As DataTable = BRApi.Database.GetDataTable(dbConn, sql, Nothing, Nothing, False)
			
			Dim hasData As Boolean = (dt.Rows.Count > 0)
			
			If ActiveLogs Then
				BRApi.ErrorLog.LogMessage(si, "INFO: HasDataInDWH - Profile: " & profileName & ", Entities: " & entityCodes & ", Time: " & timePeriod & ", HasData: " & hasData.ToString())
			End If
			
			Return hasData
		End Using
	Catch ex As Exception
		' If there's an error checking, log it and return False to skip this workflow
		BRApi.ErrorLog.LogMessage(si, "WARNING: HasDataInDWH check failed for " & profileName & " - " & timePeriod & ": " & ex.Message)
		Return False
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
''' Executes the 4 workflow steps: Import, Validate, Load Cube, Process Cube
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
		' STEP 4: PROCESS CUBE (DISABLED - Not needed for AutoLoad)
		' =====================================================
		' Process cube is intentionally NOT executed during AutoLoad
		' Consolidation and calculations are handled separately
		' Dim procProcessInfo = BRApi.DataQuality.Process.ExecuteProcessCube(si, wfClusterPk, StepClassificationTypes.ProcessCube, False)
		' If procProcessInfo.Status <> WorkflowStatusTypes.Completed Then
		' 	BRApi.ErrorLog.LogMessage(si, "ERROR: " & wfDescription & " - Process Cube did not complete. Status: " & procProcessInfo.Status.ToString())
		' 	Return False
		' End If
		' If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: Step 4 - Process Cube - Completed for " & wfDescription)

		' All steps completed successfully (Import, Validate, Load)
		Return True

	Catch ex As Exception
		BRApi.ErrorLog.LogMessage(si, "ERROR: " & wfDescription & " - Exception: " & ex.Message)
		Return False
	End Try
End Function

#End Region 'Helper Functions

#Region "SendErrorEmailHTML"
''' <summary>
''' Sends an HTML formatted error notification email
''' </summary>
Private Sub SendErrorEmailHTML(ByVal si As SessionInfo, ByVal args As ExtenderArgs, ByVal errorMessage As String)
	Dim toEmailAddresses As New List(Of String)({"imad.ajnaou@affidea.com,sergi.segura@affidea.com,ismael.gomez@affidea.com"})
	Dim emailConnectionName As String = "OneStreamEmail"
	Dim subject As String = "Error Notification from OneStream AutoLoad"
	
	If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO: SendErrorEmailHTML - errorMessage > " & errorMessage)
	
	' Customizing the email body with HTML formatting
	Dim messageBody As String = _
<html>
	<head>
		<style>
			body {{
				font-family: Arial, sans-serif;
				margin: 0;
				padding: 20px;
				color: #333;
				background-color: #f4f4f4;
			}}
			.container {{
				background-color: #fff;
				border: 1px solid #ddd;
				padding: 20px;
				margin: 0 auto;
				max-width: 600px;
			}}
			.footer {{
				font-size: 12px;
				color: #777;
			}}
		</style>
	</head>
	<body>
		<div class="container">
			<img src="https://affidea.ch/wp-content/themes/affidea/assets/img/affidea-logo.svg" alt="Affidea Logo" class="logo"></img>
			<p>Dear User,</p>
			<p>An error occurred during the execution of an automated workflow process:</p>
			<p><strong><%= errorMessage %></strong></p>
			<p>Please review the workflow setup or contact your administrator for further assistance.</p>
			<p>Kind regards,</p>
			<p>OneStream Support Team</p>
			<p class="footer">This is an automated message, please do not reply directly to this email.</p>
		</div>
	</body>
</html>.ToString()

	Dim attachments As New List(Of String)
	
	' Send the email with HTML body
	BRApi.Utilities.SendMail(si, emailConnectionName, toEmailAddresses, subject, messageBody, True, attachments)
End Sub
#End Region 'SendErrorEmailHTML

	End Class
End Namespace