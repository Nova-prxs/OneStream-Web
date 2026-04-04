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

Imports System.Threading.Tasks
Imports System.Threading
#End Region ' Imports

Namespace OneStream.BusinessRule.Extender.AQS_AutoLoad_Workflow
	Public Class MainClass
		
#Region "Main"

Dim ActiveLogs As Boolean = False

Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Dim sWFnameAll As String = String.Empty ' Initialize to an empty string instead of a space
			Dim failedWFNames As New List(Of String) ' To store names of workflows that failed to complete
			
			Dim wfScenario As String = "Actual" 'NOT TO USE until Actual is validated or 2025M1 starts to load.
'			Dim wfScenario As String = "Actual_Dummy"
			Dim sWorkspace As String = "AQS_100_Consolidation_Parameters"
			Dim gWorkspaceID As Guid = BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, sWorkspace)
			Dim wfTime As String = BRApi.Workflow.General.GetGlobalTime(si)

			Try
				Select Case args.FunctionType
					Case Is = ExtenderFunctionType.Unknown, ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep

#Region "Old code to take day +6"					
					' Define WF POV


''''						Dim wfTime As String '= "2024M2" ' Example value
'''''						Dim TimeParameter As String = "|!LV_T_Refresh_Time!|"
'''''						Dim wfTime As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, True, TimeParameter)

'''''20240319 Time by working days
''''''''				' Get current date
''''				    Dim currentDate As DateTime = DateTime.Now
''''''''				    ' Get current month and year
''''				    Dim currentYear As Integer = currentDate.Year
''''				    Dim currentMonth As Integer = currentDate.Month
''''''''				    ' Get 6th working day of current month
''''''''				    Dim sixthWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 6)
'''''''''				    Dim fifteenthhWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 15) 'TEST
''''''''				    ' Get 5th working day of next month
''''''''				    Dim nextMonth As Integer = If(currentMonth = 12, 1, currentMonth + 1)
''''''''				    Dim nextYear As Integer = If(currentMonth = 12, currentYear + 1, currentYear)
''''''''				    Dim fifthWorkingDayNextMonth As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 5)
'''''''''				    Dim fourteenthWorkingDayNextMonth  As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 14) 'TEST
''''''''				    ' Determine which month to use based on working day range
''''''''				    Dim workingDay As Integer = GetWorkingDays(New DateTime(currentYear, currentMonth, 1), currentDate)
''''''''				    Dim wfTime As String
''''''''				    If workingDay >= 6 AndAlso workingDay <= GetWorkingDays(sixthWorkingDay, fifthWorkingDayNextMonth) Then
'''''''''				    If workingDay >= 15 AndAlso workingDay <= GetWorkingDays(fifteenthhWorkingDay, fourteenthWorkingDayNextMonth ) Then 'TEST
''''				        wfTime = currentYear & "M" & currentMonth
''''''''				    Else
''''''''				        ' Use previous month if not within working day range
''''''''				        currentMonth -= 1
''''''''				        If currentMonth = 0 Then
''''''''				            currentMonth = 12
''''''''				            currentYear -= 1
''''''''				        End If
''''''''				        wfTime = currentYear & "M" & currentMonth.ToString("00")
''''''''				    End If
'''''20240319 End of Time by working days
						
						'We are saving the time in a parameter.
						'We do this as the task sheduler otherwise takes the user worflow time from David Gomez as the Task Scheduler is on its Name.
						'We define here the Parameter and we will use it in the Connector BR
'						brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, gWorkspaceID, "LV_T_AutoLoad_TimeVariable_AQS", wfTime)
	
#End Region '#Region "Old code to take day +6"			

						Dim sWFname As String = ""
						brapi.ErrorLog.LogMessage(si, "INFO : BR Auto Load wftime >> " & wftime)
						
						' Get Workflow Profiles to Execute
						Dim wfList As List(Of WorkflowUnitClusterPk) = Me.GetWFSteps(si, wfScenario, wfTime)

						' Check if the list is empty
						If wfList IsNot Nothing AndAlso wfList.Count > 0 Then
							For Each pksWFLoadStep As WorkflowUnitClusterPk In wfList
								Try
									sWFname = BRApi.Workflow.Metadata.GetProfile(si, pksWFLoadStep).Name
									'If Not sWFname.Contains("_Default") Then
									Dim completed As Boolean = AutoCompleteLoadAndProcessWF(si, pksWFLoadStep)
									If Not completed Then
										' If AutoCompleteLoadAndProcessWF failed, concatenate the workflow name
											If ActiveLogs Then brapi.ErrorLog.LogMessage(si, "ERROR: This Workflow Failed > " & BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, pksWFLoadStep) )
										If String.IsNullOrEmpty(sWFnameAll) Then
											sWFnameAll = sWFname
										Else
											sWFnameAll &= "; " & sWFname ' Concatenate using a delimiter for readability
										End If
									End If
								Catch
								End Try
							Next

''							'PARALLEL NOT WORKING Check if the list is empty
'							If wfList IsNot Nothing AndAlso wfList.Count > 0 Then
'								ExecuteWorkflowsInParallel2(si, wfList)
'							End If
		
						End If

						' After the loop, check if sWFnameAll is not empty, then send an email
						If Not String.IsNullOrEmpty(sWFnameAll) Then
							SendErrorEmailHTML(si, args, "ERROR: Autoload for Workflow(s): " & sWFnameAll)
						Else
							''SendErrorEmailHTML(si, args, "Error during Autoload for Workflow(s): No Workflows were found")
						End If
						If ActiveLogs Then brapi.ErrorLog.LogMessage(si, "sWFnameAll > " & sWFnameAll)

						End Select
						
'				brapi.Dashboards.Parameters.SetLiteralParameterValue(si, False, gWorkspaceID, "LV_T_AutoLoad_TimeVariable_AQS", "")

				Return Nothing
			Catch ex As Exception
				'        ' It's important to include a condition to check if sWFnameAll is empty to avoid sending empty names
				'        If String.IsNullOrEmpty(sWFnameAll) Then
				'            sWFnameAll = "No specific workflow name available. "
				'        End If
				'        SendErrorEmailHTML(si, args, sWFnameAll & "TEST " & ex.Message)
'				If ActiveLogs Then brapi.ErrorLog.LogMessage(si, "sWFnameAll > " & sWFnameAll)
'				SendErrorEmailHTML(si, args, "Error during Autoload for Workflow(s): " & ex.Message.ToString)
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region 'Main


'-------------------------------------------------------------------------------------------------
'-----------------------------------HELPER FUNCTIONS---------------------------------------------
'-------------------------------------------------------------------------------------------------

#Region "AutoCompleteLoadAndProcessWF"
Public Function AutoCompleteLoadAndProcessWF(ByVal si As SessionInfo, ByVal pksWFLoadStep As WorkflowUnitClusterPk) As Boolean
    Dim completed As Boolean = False
    Dim emptyByteArray As Byte() = New Byte() {}
	Dim pksWFLoadStepDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, pksWFLoadStep)

	Try
	
    ' Check if the workflow step is not nothing
    If Not pksWFLoadStep Is Nothing Then
        ' Execute IMPORT-VALIDATE-LOAD-PROCESS workflow
        Dim impProcessInfo As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, pksWFLoadStep, "", emptyByteArray, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, False)
        If impProcessInfo.Status <> WorkflowStatusTypes.Completed Then
			BRApi.ErrorLog.LogMessage(si, "ERROR: " & pksWFLoadStepDesc & " Import process did not complete successfully.")
            Return False
        End If

        ' Validate Transformation (Mapping)
        Dim valTranProcessInfo As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, pksWFLoadStep, True)
        If valTranProcessInfo.Status <> WorkflowStatusTypes.Completed Then
           BRApi.ErrorLog.LogMessage(si, "ERROR: " & pksWFLoadStepDesc & " Validation of transformation did not complete successfully.")
            Return False
        End If

        ' Validate Intersections
        Dim valIntersectProcessInfo = BRApi.Import.Process.ValidateIntersections(si, pksWFLoadStep, True)
        If valIntersectProcessInfo.Status <> WorkflowStatusTypes.Completed Then
           BRApi.ErrorLog.LogMessage(si, "ERROR: " & pksWFLoadStepDesc & " Validation of intersections did not complete successfully.")
            Return False
        End If

        ' Load the cube
        Dim lcProcessInfo = BRApi.Import.Process.LoadCube(si, pksWFLoadStep)
        If lcProcessInfo.Status <> WorkflowStatusTypes.Completed Then
           BRApi.ErrorLog.LogMessage(si, "ERROR: " & pksWFLoadStepDesc & " Load cube process did not complete successfully.")
            Return False
        End If

'		' Process the cube
'        Dim ProcProcessInfo = BRApi.DataQuality.Process.ExecuteProcessCube(si, wfChildClusterPk, StepClassificationTypes.ProcessCube, False)
'        If ProcProcessInfo.Status <> WorkflowStatusTypes.Completed Then
'            BRApi.ErrorLog.LogMessage(si, "ERROR: Process cube process did not complete successfully.")
'            Return False
'        End If
		
		
        ' If all steps completed successfully
        completed = True
    Else
    End If		

    If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "INFO : completed " & completed.ToString & " " & pksWFLoadStepDesc )
    Return completed
		
	Catch
		Return False
	End Try
		
End Function
#End Region 'AutoCompleteLoadAndProcessWF				
		
#Region "GetWFSteps"		
		Private Function GetWFSteps(ByVal si As SessionInfo, ByVal scenarioName As String, ByVal timeName As String) As List(Of WorkflowUnitClusterPk)
			'This function is used to get the list of WF steps imports that should run automatically. We only return this list
			Try
				Dim wfClusterPks As New List(Of WorkflowUnitClusterPk)

				''Define the SQL Statement
				Dim sql As New Text.StringBuilder
				'It is taking the list of workflow names of Import type for actuals in the cube AutoLoad, when the WF text2 is Autoload and is Active
				Dim sSQL As String = $"
								SELECT
								  wpfh.ProfileName
								FROM dbo.WorkflowProfileHierarchy wpfh
								INNER JOIN dbo.WorkflowProfileAttributes wpa ON wpfh.ProfileKey = wpa.ProfileKey
								WHERE wpfh.CubeName = 'Cube_100_Group' --Cube Root Workflow profile
								  AND wpfh.ProfileType = '50' --import type
								  AND wpa.ScenarioTypeID = '0' --actuals
								GROUP BY wpfh.ProfileName, wpfh.ProfileKey
								HAVING 
								  SUM(CASE WHEN wpa.AttributeIndex = '20000' AND wpa.ProfileAttributeValue = 'AutoLoad' THEN 1 ELSE 0 END) > 0  --20000 is the workflow text2 and its value shoudl be autoload
								  AND SUM(CASE WHEN wpa.AttributeIndex = '1300' AND wpa.ProfileAttributeValue = '1' THEN 1 ELSE 0 END) > 0  --1300 is the workflow active or not
								  ORDER BY 
								  CASE 
								    WHEN wpfh.ProfileName LIKE 'Portugal%' THEN 1
								    WHEN wpfh.ProfileName LIKE 'Italy%' THEN 2
								    ELSE 3
								  END, 
								  wpfh.ProfileName
									"
				
''''				'JUST FOR TESTING ON POLAND
''''				Dim sSQL As String = $"				
''''				Select wpfh.ProfileName
''''From dbo.WorkflowProfileHierarchy wpfh
''''INNER Join dbo.WorkflowProfileAttributes wpa On wpfh.ProfileKey = wpa.ProfileKey
''''Where wpfh.CubeName = 'Cube_100_Group' --Cube Root Workflow profile
''''And wpfh.ProfileType = '50' --import type
''''And wpa.ScenarioTypeID = '0' --actuals
''''And wpfh.ProfileName Like '%Poland%' -- Only profiles containing 'Poland'
''''Group By wpfh.ProfileName, wpfh.ProfileKey
''''HAVING 
''''  SUM(Case When wpa.AttributeIndex = '20000' AND wpa.ProfileAttributeValue = 'AutoLoad' THEN 1 ELSE 0 END) > 0  --20000 is the workflow text2 and its value should be autoload
''''  And SUM(Case When wpa.AttributeIndex = '1300' AND wpa.ProfileAttributeValue = '1' THEN 1 ELSE 0 END) > 0; --1300 is the workflow active or not

  
''''									"
  
If ActiveLogs Then brapi.ErrorLog.LogMessage(si, "INFO: GetWFSteps sSQL >> " & sSQL.ToString)				
				
				'Create the list of WorkflowUnitClusterPks
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sSQL, False)
						For Each dr As DataRow In dt.Rows
'							Dim year As String = "2024" 'Left(timeName, 4)

'''''20240319 Time by working days
''''				' Get current date
''''				    Dim currentDate As DateTime = DateTime.Now
''''				    ' Get current month and year
''''				    Dim currentYear As Integer = currentDate.Year
''''				    Dim currentMonth As Integer = currentDate.Month
''''				    ' Get 6th working day of current month
''''				    Dim sixthWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 6)
'''''				    Dim fifteenthhWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 15) 'TEST
''''				    ' Get 5th working day of next month
''''				    Dim nextMonth As Integer = If(currentMonth = 12, 1, currentMonth + 1)
''''				    Dim nextYear As Integer = If(currentMonth = 12, currentYear + 1, currentYear)
''''				    Dim fifthWorkingDayNextMonth As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 5)
'''''				    Dim fourteenthWorkingDayNextMonth  As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 14) 'TEST
''''				    ' Determine which month to use based on working day range
''''				    Dim workingDay As Integer = GetWorkingDays(New DateTime(currentYear, currentMonth, 1), currentDate)
''''				    Dim wfTime As String
''''				    If workingDay >= 6 AndAlso workingDay <= GetWorkingDays(sixthWorkingDay, fifthWorkingDayNextMonth) Then
'''''				    If workingDay >= 15 AndAlso workingDay <= GetWorkingDays(fifteenthhWorkingDay, fourteenthWorkingDayNextMonth ) Then 'TEST
''''				        wfTime = currentYear & "M" & currentMonth
''''				    Else
''''				        ' Use previous month if not within working day range
''''				        currentMonth -= 1
''''				        If currentMonth = 0 Then
''''				            currentMonth = 12
''''				            currentYear -= 1
''''				        End If
''''				        wfTime = currentYear & "M" & currentMonth.ToString("00")
''''				    End If
'''''20240319 End of Time by working days
If ActiveLogs Then brapi.ErrorLog.LogMessage(si, "INFO: GetWFSteps timeName >> " & timeName)

							'    Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), scenarioName, timeName)
'							Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), "Actual", "2024M2")
'							Dim TimeParameter As String = "|!LV_T_Refresh_Time!|"
'							Dim wfTime As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, True, TimeParameter)
'							Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), "Actual", wfTime)
							Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), scenarioName, timeName)
							
							If Not wfClusterPk Is Nothing Then
								wfClusterPks.Add(wfClusterPk)
							End If
							
						Next				
					End Using
				End Using

				Return wfClusterPks

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End  Region ''GetWFSteps
		
#Region "SendErrorEmailHTML"		
Private Sub SendErrorEmailHTML(ByVal si As SessionInfo, ByVal args As ExtenderArgs, ByVal errorMessage As String)
    Dim toEmailAddresses As New List(Of String)({"imad.ajnaou@affidea.com,sergi.segura@affidea.com,ismael.gomez@affidea.com"})
'    Dim toEmailAddresses As New List(Of String)({})
    Dim emailConnectionName As String = "OneStreamEmail"
    Dim subject As String = "Error Notification from OneStream AutoLoad"
    If ActiveLogs Then brapi.ErrorLog.LogMessage(si, "INFO : errorMessage > " & errorMessage) 
    ' Customizing the email body with HTML formatting  
Dim messageBody As String = _
<html>
    <head>
        <style>
            body {
                font-family: Arial, sans-serif;
                margin: 0;
                padding: 20px;
                color: #333;
                background-color: #f4f4f4;
            }
            .container {
                background-color: #fff;
                border: 1px solid #ddd;
                padding: 20px;
                margin: 0 auto;
                max-width: 600px;
            }
            .footer {
                font-size: 12px;
                color: #777;
            }
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



	
    Dim attachments As New List(Of String) ' Assuming attachments are handled elsewhere

    ' Send the email with HTML body
    BRApi.Utilities.SendMail(si, emailConnectionName, toEmailAddresses, subject, messageBody, True, attachments)
	
End Sub
#End Region '"SendErrorEmailHTML"	

#Region "GetWorkingDays"	
		Public Function GetWorkingDays(startDate As DateTime, endDate As DateTime) As Integer
		    Dim totalDays As Integer = endDate.Subtract(startDate).TotalDays
		    Dim totalWeekends As Integer = Math.Floor((totalDays + startDate.DayOfWeek - DayOfWeek.Saturday) / 7) * 2
		    Return CInt(totalDays - totalWeekends)
		End Function

		Public Function GetNthWorkingDayOfMonth(year As Integer, month As Integer, n As Integer) As DateTime
		    Dim targetDate As DateTime = New DateTime(year, month, 1)
		    While n > 0
		        If targetDate.DayOfWeek <> DayOfWeek.Saturday And targetDate.DayOfWeek <> DayOfWeek.Sunday Then
		            n -= 1
		        End If
		        targetDate = targetDate.AddDays(1)
		    End While
		    Return targetDate.AddDays(-1)
		End Function
'20240319

#End Region ' "GetWorkingDays"	

#Region "Parallel Workflow Execution"

Public Sub ExecuteWorkflowsInParallel(ByVal si As SessionInfo, ByVal wfList As List(Of WorkflowUnitClusterPk))
    If wfList Is Nothing OrElse wfList.Count = 0 Then
        Throw New Exception("No workflows to execute.")
    End If

    Dim tasks As New List(Of Task(Of Boolean))
    For Each pksWFLoadStep As WorkflowUnitClusterPk In wfList
        ' Start a new task for each workflow.
        tasks.Add(Task.Run(Function() AutoCompleteLoadAndProcessWF(si, pksWFLoadStep)))
    Next

    Try
        ' Wait for all tasks to complete.
        Task.WaitAll(tasks.ToArray())

        ' Check which workflows failed to complete and log them.
        For i As Integer = 0 To tasks.Count - 1
            If Not tasks(i).Result Then
                Dim failedWFName As String = BRApi.Workflow.Metadata.GetProfile(si, wfList(i)).Name
                BRApi.ErrorLog.LogMessage(si, $"Workflow failed: {failedWFName}")
            End If
        Next
    Catch aggEx As AggregateException
        For Each ex In aggEx.InnerExceptions
            If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, $"Error during workflow execution: {ex.Message}")
        Next
    End Try
End Sub

#End Region

#Region "ExecuteWorkflowsInParallel2"
Public Async Sub ExecuteWorkflowsInParallel2(ByVal si As SessionInfo, ByVal wfList As List(Of WorkflowUnitClusterPk))
    If wfList Is Nothing OrElse wfList.Count = 0 Then
        Throw New Exception("No workflows to execute.")
    End If

    ' Using SemaphoreSlim to limit to 2 concurrent tasks.
    Dim semaphore As New SemaphoreSlim(2, 2)

    ' A list to keep track of all the tasks.
    Dim taskList As New List(Of Task(Of Boolean))

    For Each pksWFLoadStep As WorkflowUnitClusterPk In wfList
        ' Use semaphore to ensure only 2 tasks run in parallel.
        Await semaphore.WaitAsync()

        ' Launching a new task for each workflow.
        Dim newTask = Task.Run(Async Function()
                                   Dim result As Boolean = False
                                   Try
                                       result = AutoCompleteLoadAndProcessWF(si, pksWFLoadStep)
                                   Finally
                                       semaphore.Release()
                                   End Try
                                   Return result
                               End Function)
        taskList.Add(newTask)
    Next

    ' Wait for all tasks to complete.
    Dim results As Boolean() = Await Task.WhenAll(taskList)

    ' Check which workflows failed to complete and log them.
    For i As Integer = 0 To results.Length - 1
        If Not results(i) Then
            Dim failedWFName As String = BRApi.Workflow.Metadata.GetProfile(si, wfList(i)).Name
            BRApi.ErrorLog.LogMessage(si, $"Workflow failed: {failedWFName}")
        End If
    Next
End Sub
#End Region 'ExecuteWorkflowsInParallel2

	End Class
End Namespace