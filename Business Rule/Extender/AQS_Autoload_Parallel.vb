Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine


'1- A DM Sequence called AQS_Autoload_Parallel01 runs a Step called AQS_Autoload_Parallel02
'2- The Step AQS_Autoload_Parallel02 runs the BR RunAutoloadParallel
'3- The BR RunAutoloadParallel then loops to all WF and runs many DM Sequence AQS_Autoload_Parallel03
'4- The DM Sequence AQS_Autoload_Parallel03 then runs many import Steps AQS_Autoload_Parallel04 in parallele.


Namespace OneStream.BusinessRule.Extender.AQS_Autoload_Parallel
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType


					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep

						If args.DataMgmtArgs.CurrentStep.Step.Name = "AQS_Autoload_Parallel02" Then
							BRApi.ErrorLog.LogMessage(si, "AQS_Autoload_Parallel02")
							RunAutoloadParallel(si, args)
						End If
					

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Sub RunAutoloadParallel(ByVal si As SessionInfo, args As ExtenderArgs)
			BRApi.ErrorLog.LogMessage(si, "AQS_Autoload_Parallel02")

			Dim wfScenario As String = "Actual"

			Dim currentDate As DateTime = DateTime.Now
			' Get current month and year
			Dim currentYear As Integer = currentDate.Year
			Dim currentMonth As Integer = currentDate.Month
			' Get 6th working day of current month
			Dim sixthWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 6)
			' Get 5th working day of next month
			Dim nextMonth As Integer = If(currentMonth = 12, 1, currentMonth + 1)
			Dim nextYear As Integer = If(currentMonth = 12, currentYear + 1, currentYear)
			Dim fifthWorkingDayNextMonth As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 5)
			' Determine which month to use based on working day range
			Dim workingDay As Integer = GetWorkingDays(New DateTime(currentYear, currentMonth, 1), currentDate)
			Dim wfTime As String

			If workingDay >= 6 AndAlso workingDay <= GetWorkingDays(sixthWorkingDay, fifthWorkingDayNextMonth) Then
				wfTime = currentYear & "M" & currentMonth
			Else
				' Use previous month if not within working day range
				currentMonth -= 1
				If currentMonth = 0 Then
					currentMonth = 12
					currentYear -= 1
				End If
				wfTime = currentYear & "M" & currentMonth.ToString("00")
			End If

			'Dim wfTime As String = BRApi.Workflow.General.GetGlobalTime(si)
			Dim sWFname As String = ""


			' Get Workflow Profiles to Execute
			Dim wfList As List(Of WorkflowUnitClusterPk) = Me.GetWFSteps(si, wfScenario, wfTime)

			If wfList IsNot Nothing AndAlso wfList.Count > 0 Then
				For Each pksWFLoadStep As WorkflowUnitClusterPk In wfList
					Dim sProfileID As String = pksWFLoadStep.UniqueStringId
					Dim dictWF As New Dictionary(Of String,String)
					dictWF.Add("sProfileID", sProfileID)
					brapi.Utilities.StartDataMgmtSequence(si, "AQS_Autoload_Parallel03", dictWF)
				Next
			End If
		End Sub


'-------------------------------------------------------------------------------------------------
'-----------------------------------HELPER FUNCTIONS---------------------------------------------
'-------------------------------------------------------------------------------------------------
	
		
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
                                    "
				'Create the list of WorkflowUnitClusterPks
				Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
					Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sSQL, False)
						For Each dr As DataRow In dt.Rows
'							Dim year As String = "2024" 'Left(timeName, 4)

'20240319 Time by working days
				' Get current date
				    Dim currentDate As DateTime = DateTime.Now
				    ' Get current month and year
				    Dim currentYear As Integer = currentDate.Year
				    Dim currentMonth As Integer = currentDate.Month
				    ' Get 6th working day of current month
				    Dim sixthWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 6)
'				    Dim fifteenthhWorkingDay As DateTime = GetNthWorkingDayOfMonth(currentYear, currentMonth, 15) 'TEST
				    ' Get 5th working day of next month
				    Dim nextMonth As Integer = If(currentMonth = 12, 1, currentMonth + 1)
				    Dim nextYear As Integer = If(currentMonth = 12, currentYear + 1, currentYear)
				    Dim fifthWorkingDayNextMonth As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 5)
'				    Dim fourteenthWorkingDayNextMonth  As DateTime = GetNthWorkingDayOfMonth(nextYear, nextMonth, 14) 'TEST
				    ' Determine which month to use based on working day range
				    Dim workingDay As Integer = GetWorkingDays(New DateTime(currentYear, currentMonth, 1), currentDate)
				    Dim wfTime As String
				    If workingDay >= 6 AndAlso workingDay <= GetWorkingDays(sixthWorkingDay, fifthWorkingDayNextMonth) Then
'				    If workingDay >= 15 AndAlso workingDay <= GetWorkingDays(fifteenthhWorkingDay, fourteenthWorkingDayNextMonth ) Then 'TEST
				        wfTime = currentYear & "M" & currentMonth
				    Else
				        ' Use previous month if not within working day range
				        currentMonth -= 1
				        If currentMonth = 0 Then
				            currentMonth = 12
				            currentYear -= 1
				        End If
				        wfTime = currentYear & "M" & currentMonth.ToString("00")
				    End If
'20240319 End of Time by working days


							'    Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), scenarioName, timeName)
'							Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), "Actual", "2024M2")
'							Dim TimeParameter As String = "|!LV_T_Refresh_Time!|"
'							Dim wfTime As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, True, TimeParameter)
							Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), "Actual", wfTime)
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

		
		
		
	End Class
End Namespace