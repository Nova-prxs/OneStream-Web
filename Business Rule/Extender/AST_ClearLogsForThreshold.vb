Imports System
Imports System.Data
Imports System.Collections
Imports System.Collections.Generic
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database

Namespace OneStream.BusinessRule.Extender.AST_ClearLogsForThreshold

	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		AST_ClearLogsForThreshold 
		'
		'Description:			Combined Error, User Logon and Task Activity log clearing helper functions.
		'
		'Usage:					Provides low impact log clearing functions for all the three majaor log types within
		'						OneStream XF.  These methods are intended to be executed from a Data Management job as
		'						a background process.  The deletes for Task Activity are structured as single record deletes
		'						in order to prevent time-outs, excessive transaction log growth and concurrency problems.
		'
		'Created By:			OneStream Software
		'Date Created:			07-15-2014
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Dim batchSize As Integer = 100000

				Select Case args.FunctionType
					Case Is = ExtenderFunctionType.Unknown
						'Delete Default 90 Day Retention
						ClearTaskActivity(si, 90, 90, Nothing, batchSize, "(All)", "(All)")
						ClearErrorLog(si, 90, 90, Nothing, batchSize, "(All)", "(All)", String.Empty)
						ClearLogonActivity(si, 90, 90, Nothing, batchSize, "(All)", "(All)")

					Case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Delete One Month at a time starting at 27 months old, this will break delete process in to multiple Data Management Steps
						'--------------------------------------------------------------------------------------------------------------------------											
						Dim convertError As Boolean = False
						Dim retentionDays As Integer = ConvertHelper.ToInt32(args.NameValuePairs("RetentionDays"), False, 0)
						Dim clearTaskActivity As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs("ClearTaskActivity"), False, False, convertError)
						Dim clearErrorLog As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs("ClearErrorLog"), False, False, convertError)
						Dim clearLogonActivity As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs("ClearLogonActivity"), False, False, convertError)
						Dim clearLogUser As String = args.NameValuePairs.XFGetValue("ClearLogUser", String.Empty)
						Dim clearLogApplication As String = args.NameValuePairs.XFGetValue("ClearLogApplication", String.Empty)
						Dim errorLogDescription As String = args.NameValuePairs.XFGetValue("ErrorLogDescription", String.Empty)
						Dim stepThreshold As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("StepThreshold", 0), False, 0)
						Dim dmTaskItem As TaskActivityItem = GetTaskActivityItem(si)
						'Execute
						If clearTaskActivity Then 
							Me.ClearTaskActivity(si, retentionDays, stepThreshold, dmTaskItem, batchSize, clearLogUser, clearLogApplication)
						End If
						If clearErrorLog Then 
							Me.ClearErrorLog(si, retentionDays, stepThreshold, dmTaskItem, batchSize, clearLogUser, clearLogApplication, errorLogDescription)
						End If
						If clearLogonActivity Then 
							Me.ClearLogonActivity(si, retentionDays, stepThreshold, dmTaskItem, batchSize, clearLogUser, clearLogApplication)
						End If

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "Log Clearing Helper Functions"

		Private Function GetThresholdDate(ByVal si As SessionInfo, retentionDays As Integer, stepThreshold As Integer) As String
			Try
				Dim yearMonthDay As String = String.Empty

				'Make sure that we do not clear log items if this Step Threshold is less than the Threshold selected by the user
				'and do NOT allow retention days less than 30.
				If (retentionDays <> 0) AndAlso (retentionDays >= 30) AndAlso (stepThreshold <> 0) AndAlso (stepThreshold >= retentionDays) Then
					'Calculate the date threshold to used for the delete criteria
					yearMonthDay = DateTime.Now().AddDays(stepThreshold * -1).ToString("yyyy-MM-dd 00:00:00")
				End If
				Return yearMonthDay

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Sub ClearTaskActivity(ByVal si As SessionInfo, retentionDays As Integer, stepThreshold As Integer, taskItem As TaskActivityItem, batchSize As Integer, clearLogUser As String, clearLogApplication As String)
			Try
				'Initialize the Task Activity update threshold
				Dim updateTime As DateTime = DateTime.Now.AddMinutes(5)

				'Calculate the date threshold to used for the delete criteria
				Dim yearMonthDay As String = GetThresholdDate(si, retentionDays, stepThreshold)

				'Make sure that we do not clear log items if this Step Threshold is less than the Threshold selected by the user
				If Not String.IsNullOrEmpty(yearMonthDay) Then

					'Delete all log information older than the supplied date
					Dim msg As New Text.StringBuilder
					msg.Append($"Delete Task Activity ({yearMonthDay}) ")

					Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
						Dim taskCount As Long = 0
						Dim stepCount As Long = 0

						'Get count of Task Steps that match this date criteria and store in stepCount variable
						Dim sqlStep As New Text.StringBuilder
						sqlStep.Append("Select COUNT(TaskActivityStep.StepType) As StepCount ")
						sqlStep.Append("From TaskActivity INNER Join TaskActivityStep On TaskActivity.UniqueID = TaskActivityStep.TaskActivityID ")
						sqlStep.Append("Where (TaskActivity.StartTime < CONVERT(DATETIME, @yearMonthDay, 102))")
						If clearLogUser <> "(All)" Then
							sqlStep.Append("AND UserName = @clearLogUser")
						End If
						If clearLogApplication <> "(All)" Then
							sqlStep.Append("AND AppName = @clearLogApplication")
						End If

						Dim parameters As List(Of DbParamInfo) = New List(Of DbParamInfo) From {
							    New DbParamInfo("@yearMonthDay", yearMonthDay),
							    New DbParamInfo("@clearLogUser", clearLogUser),
							    New DbParamInfo("@clearLogApplication", clearLogApplication)
							    }
						Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnFW, sqlStep.ToString, parameters, True)
							stepCount = CType(dt.Rows(0).Item(0), Long)
						End Using

						'Get the list of TaskSteps and delete them one by one
						Dim deleteStatement As New Text.StringBuilder
						deleteStatement.Append($"DELETE TOP ({batchSize}) ")
						deleteStatement.Append("From TaskActivityStep ")
						deleteStatement.Append("From TaskActivity INNER Join TaskActivityStep On TaskActivity.UniqueID = TaskActivityStep.TaskActivityID ")
						deleteStatement.Append("Where (TaskActivity.StartTime < CONVERT(DATETIME, @yearMonthDay, 102)) ")
						If clearLogUser <> "(All)" Then
							deleteStatement.Append("AND UserName = @clearLogUser ")
						End If
						If clearLogApplication <> "(All)" Then
							deleteStatement.Append("AND AppName = @clearLogApplication ")
						End If

						'Now keep executing and committing this statement until the records affected count = 0
						Dim cumDeleteCount As Long = 0
						Dim rowsDeleted As Long = 0
						Dim pctComplete As Decimal = 0d

						'Keep deleting as long as the number of rows is equal to the batch size
						Do While cumDeleteCount < stepCount
							'Delete rows, but limit to batch size
							rowsDeleted = DbSql.ExecuteActionQuery(dbConnFW, CommandType.Text, deleteStatement.ToString, parameters, True)
							cumDeleteCount += rowsDeleted

							'Update TaskActivityStatus
							If taskItem IsNot Nothing Then
								If DateTime.Now >= updateTime Then
									'Reset the update time indicator
									updateTime = DateTime.Now.AddMinutes(5)
									'Update the task status
									pctComplete = Math.Round(((cumDeleteCount / stepCount) * 100), 0)
									UpdateTaskActivityStatus(si, taskItem, $"Deleted: ({cumDeleteCount}) Steps Of ({stepCount}) Total Steps", pctComplete, 100)
								End If
							End If
						Loop

						'Update task status for final delete of tasks (Set to 99%)
						UpdateTaskActivityStatus(si, taskItem, "Deleting Tasks", 99, 100)

						'Now delete all of the tasks in one call
						Dim sqlTasksDelete As New Text.StringBuilder
						sqlTasksDelete.Append("DELETE FROM TaskActivity WHERE (StartTime < CONVERT(DATETIME, @yearMonthDay, 102)) ")
						If clearLogUser <> "(All)" Then
							sqlTasksDelete.Append("AND UserName = @clearLogUser ")
						End If
						If clearLogApplication <> "(All)" Then
							sqlTasksDelete.Append("AND AppName = @clearLogApplication ")
						End If
						taskCount = BRApi.Database.ExecuteActionQuery(dbConnFW, sqlTasksDelete.ToString, parameters, True, False)

						'Update task status for final delete of tasks (Set to 100%)
						UpdateTaskActivityStatus(si, taskItem, "Deleting Tasks", 100, 100)

						'Log the delete information
						msg.Append($"Tasks = {taskCount}, ")
						msg.Append($"Steps = {stepCount}")
						If taskCount > 0 Then
							BRApi.ErrorLog.LogMessage(si, msg.ToString)
						End If

					End Using
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub

		Private Sub ClearLogonActivity(ByVal si As SessionInfo, retentionDays As Integer, stepThreshold As Integer, taskItem As TaskActivityItem, batchSize As Integer, clearLogUser As String, clearLogApplication As String)
			Try
				'Calculate the date threshold to used for the delete criteria
				Dim yearMonthDay As String = GetThresholdDate(si, retentionDays, stepThreshold)

				'Make sure that we do not clear log items if this Step Threshold is less than the Threshold selected by the user
				If (Not String.IsNullOrEmpty(yearMonthDay)) Then

					'Delete all log information older than the supplied date
					Dim msg As New Text.StringBuilder
					msg.Append($"Delete Logon Activity ({yearMonthDay}) ")

					Dim parameters As List(Of DbParamInfo) = New List(Of DbParamInfo) From {
						    New DbParamInfo("@yearMonthDay", yearMonthDay),
						    New DbParamInfo("@clearLogUser", clearLogUser),
						    New DbParamInfo("@clearLogApplication", clearLogApplication)
						    }

					Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
						Dim logonCount As Long = 0

						'Get count of Logons that match this date criteria
						Dim sqlLogons As New Text.StringBuilder
						sqlLogons.Append("SELECT COUNT(*) AS LogonCount ")
						sqlLogons.Append("FROM UserLogonActivity ")
						sqlLogons.Append("Where (LogonTime < CONVERT(DATETIME, @yearMonthDay, 102)) ")
						If clearLogUser <> "(All)" Then
							sqlLogons.Append("AND UserName = @clearLogUser")
						End If
						If clearLogApplication <> "(All)" Then
							sqlLogons.Append("AND AppName = @clearLogApplication")
						End If
						'Get the count of items to delete
						Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnFW, sqlLogons.ToString, parameters, True)
							logonCount = CType(dt.Rows(0).Item(0), Long)
						End Using

						'Now delete all of the Logons one batch at a time			
						Dim sqlLogonsDelete As New Text.StringBuilder
						sqlLogonsDelete.Append($"DELETE TOP ({batchSize}) ")
						sqlLogonsDelete.Append("FROM UserLogonActivity ")
						sqlLogonsDelete.Append("Where (LogonTime < CONVERT(DATETIME, @yearMonthDay, 102)) ")
						If clearLogUser <> "(All)" Then
							sqlLogonsDelete.Append("AND UserName = @clearLogUser")
						End If
						If clearLogApplication <> "(All)" Then
							sqlLogonsDelete.Append("AND AppName = @clearLogApplication")
						End If

						'Now keep executing and committing this statement until the records affected count = 0
						Dim cumLogonDeleteCount As Long = 0
						Dim rowsLogonDeleted As Long = 0
						Dim pctComplete As Decimal = 0d

						'Keep deleting as long as the number of rows is equal to the batch size
						Do While cumLogonDeleteCount < logonCount
							'Delete rows, but limit to batch size
							rowsLogonDeleted = DbSql.ExecuteActionQuery(dbConnFW, CommandType.Text, sqlLogonsDelete.ToString, parameters, True)
							cumLogonDeleteCount += rowsLogonDeleted

							'Update TaskActivityStatus
							If taskItem IsNot Nothing Then
								'Update the task status
								pctComplete = Math.Round(((cumLogonDeleteCount / logonCount) * 100), 0)
								UpdateTaskActivityStatus(si, taskItem, $"Deleted: ({cumLogonDeleteCount}) Logons Of ({logonCount}) Total Logons", pctComplete, 100)
								'End If
							End If
						Loop

						'Update task status for final delete (Set to 100%)
						UpdateTaskActivityStatus(si, taskItem, "Deleting Logon Activity", 100, 100)

						'Log the delete information
						msg.Append($"Logons = {logonCount}")
						If logonCount > 0 Then
							BRApi.ErrorLog.LogMessage(si, msg.ToString)
						End If
					End Using
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub

		Private Sub ClearErrorLog(ByVal si As SessionInfo, retentionDays As Integer, stepThreshold As Integer, taskItem As TaskActivityItem, batchSize As Integer, clearLogUser As String, clearLogApplication As String, errorLogDescription As String)
			Try
				'Initialize the Task Activity update threshold
				Dim updateTime As DateTime = DateTime.Now.AddMinutes(5)
				'Calculate the date threshold to used for the delete criteria
				Dim yearMonthDay As String = GetThresholdDate(si, retentionDays, stepThreshold)
				'Make sure that we do not clear log items if this Step Threshold is less than the Threshold selected by the user
				If (Not String.IsNullOrEmpty(yearMonthDay)) Then

					Dim parameters As New List(Of DbParamInfo) From {
						    New DbParamInfo("@yearMonthDay", yearMonthDay),
						    New DbParamInfo("@clearLogUser", clearLogUser),
						    New DbParamInfo("@clearLogApplication", clearLogApplication),
						    New DbParamInfo("@errorLogDescription", errorLogDescription.Trim())
						    }

					'Delete all log information older than the supplied date
					Dim msg As New Text.StringBuilder
					msg.Append($"Delete Error Log ({yearMonthDay}) ")

					Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
						Dim logCount As Long = 0

						'Get count of errors that match this date criteria and store in logCount variable
						Dim sqlErrorLog As String = "SELECT COUNT(*) AS ErrorCount
						FROM ErrorLog
						WHERE (ErrorTime < CONVERT(DATETIME, @yearMonthDay, 102))"
						If clearLogUser <> "(All)" Then
							sqlErrorLog &= " AND UserName = @clearLogUser"
						End If
						If clearLogApplication <> "(All)" Then
							sqlErrorLog &= " AND AppName = @clearLogApplication"
						End If
						If Not String.IsNullOrWhiteSpace(errorLogDescription.Trim) Then
							sqlErrorLog &= " AND Description LIKE '%' + @errorLogDescription + '%' "
						End If
						Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnFW, sqlErrorLog, parameters, True)
							logCount = CType(dt.Rows(0).Item(0), Long)
						End Using

						'Now delete all of the Error Log Entries one batch at a time
						Dim sqlLogDelete As String = $"DELETE TOP ({batchSize}) 
						FROM ErrorLog
						WHERE (ErrorTime < CONVERT(DATETIME, @yearMonthDay, 102))"
						 If clearLogUser <> "(All)" Then
							sqlLogDelete &= " AND UserName = @clearLogUser"
						End If
						If clearLogApplication <> "(All)" Then
							sqlLogDelete &= " AND AppName = @clearLogApplication"
						End If
						If errorLogDescription.Trim <> "" Then
							sqlLogDelete &= " AND Description LIKE '%' + @errorLogDescription + '%' "
						End If
						'Now keep executing and committing this statement until the records affected count = 0
						Dim cumErrorDeleteCount As Long = 0
						Dim rowsErrorDeleted As Long = 0
						Dim pctComplete As Decimal = 0d

						'Keep deleting as long as the number of rows is equal to the batch size
						Do While cumErrorDeleteCount < logCount
							'Delete rows, but limit to batch size
							rowsErrorDeleted = DbSql.ExecuteActionQuery(dbConnFW, CommandType.Text, sqlLogDelete.ToString, parameters, True)
							cumErrorDeleteCount += rowsErrorDeleted

							'Update TaskActivityStatus
							If taskItem IsNot Nothing Then
								If DateTime.Now >= updateTime Then
									'Reset the update time indicator
									updateTime = DateTime.Now.AddMinutes(5)
									'Update the task status
									pctComplete = Math.Round(((cumErrorDeleteCount / logCount) * 100), 0)
									UpdateTaskActivityStatus(si, taskItem, $"Deleted: ({cumErrorDeleteCount}) Errors Of ({logCount}) Total Errors", pctComplete, 100)
								End If
							End If
						Loop

						'Update task status for final delete (Set to 100%)
						UpdateTaskActivityStatus(si, taskItem, "Deleting Error Log", 100, 100)

						'Log the delete information
						msg.Append($"Error Log Entries = {logCount}")
						If logCount > 0 Then
							BRApi.ErrorLog.LogMessage(si, msg.ToString)
						End If
					End Using
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub

#End Region

#Region "Task Activity Helpers"

		Private Function GetTaskActivityItem(ByVal si As SessionInfo) As TaskActivityItem
			Try
				Dim taItem As TaskActivityItem = Nothing

				Dim runningTasks As TaskActivityList = EngineTaskActivity.GetTaskActivity(si, True, True, Nothing, Nothing, 0, 100)
				If Not runningTasks Is Nothing Then
					For Each taskItem As TaskActivityInfo In runningTasks.Items
						'Find the TaskID for the "Clear Logs (AST)" Task
						If taskItem.Item.Description.Equals("Clear Logs (AST)", StringComparison.InvariantCultureIgnoreCase) And (taskItem.Item.TaskActivityType = TaskActivityType.DataManagement) Then
							taItem = taskItem.Item
							Exit For
						End If
					Next
				End If

				Return taItem

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Private Sub UpdateTaskActivityStatus(ByVal si As SessionInfo, taskItem As TaskActivityItem, actionDescription As String, actionsCompleted As Decimal, numActions As Integer)
			Try
				If Not taskItem Is Nothing Then
					'Update the current Sub-Task so a long running jobs are not cancelled by the system
					EngineTaskActivity.UpdateRunningTaskActivity(si, taskItem.UniqueID, Nothing, taskItem.CurrentSubTask, taskItem.CurrentSubTaskDescription, actionDescription, actionsCompleted, numActions)
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub

#End Region

	End Class

End Namespace