Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports Microsoft.Data.SqlClient
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Web.UI.WebControls
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
Imports OneStream.BusinessRule.DashboardExtender.CAT_ObjectModel

Namespace OneStream.BusinessRule.Extender.CAT_ExecuteCopy
	Public Class MainClass
		ReadOnly _catHelper As New OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass

		Public Sub New()
		End Sub

		Public Function Main(si As SessionInfo, globals As BRGlobals, api As Object, args As ExtenderArgs) As Object
			Dim objManageApp As New ManageAppInfo
			Dim executeResponse As String = String.Empty
			Dim serviceNowResultString As String = String.Empty
			Select Case args.FunctionType
				Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
					Try
						'Prepare Parameters
						Dim targetAppName As String = args.NameValuePairs.XFGetValue("TargetAppName", String.Empty)
						Dim targetIsProductionApplication As String = args.NameValuePairs.XFGetValue("TargetIsProductionApplication", "False") = "True"
						Dim newApplicationName As String = args.NameValuePairs.XFGetValue("NewApplicationName", String.Empty)
						Dim isNewApplication As Boolean = args.NameValuePairs.XFGetValue("IsNewApplication", "False") = "True"
						Dim sourceSchemaName As String = args.NameValuePairs.XFGetValue("SourceSchemaName", String.Empty)
						Dim targetSchemaName As String = args.NameValuePairs.XFGetValue("TargetSchemaName", String.Empty)
						Dim makeProductionApplication As Boolean = args.NameValuePairs.XFGetValue("IsProductionApplication", "False") = "True"
						Dim newDatabaseName As String = args.NameValuePairs.XFGetValue("NewDatabaseName", String.Empty)
						Dim sourceAppName As String = args.NameValuePairs.XFGetValue("SourceAppName", String.Empty)
						Dim retentionLtrValue As String = args.NameValuePairs.XFGetValue("RetentionLTRValue", String.Empty)
						Dim retentionStrValue As String = args.NameValuePairs.XFGetValue("PITRDays", String.Empty)
						Dim envType As String = args.NameValuePairs.XFGetValue("EnvType", String.Empty)
						Dim resetIIS As Boolean = args.NameValuePairs.XFGetValue("Reset", "False") = "True"
						Dim serviceNowRequestId As String = args.NameValuePairs.XFGetValue("ServiceNowRequestId", String.Empty)
						Dim runningTime As Decimal = 0D
						Dim newApplicationId As String = String.Empty
						Dim identity As Integer = 0
						Dim description As String = String.Format(If(isNewApplication, "Creating a new application named {0}.", "Replacing existing application {1} with {0}."), newApplicationName, targetAppName)
						' Set the task activity description
						UpdateTaskActivity(si, args, description, TaskActivityStatus.Unknown)

						' Populate the ManageApp object
						objManageApp.Username = si.AuthToken.UserName
						objManageApp.UserEmailAddress = BRApi.Security.Admin.GetUser(si, si.AuthToken.UserName).User.Email
						objManageApp.TaskType = If(isNewApplication, "Create", "Replace")
						objManageApp.NewAppName = newApplicationName
						objManageApp.NewAppSchemaName = newDatabaseName
						objManageApp.RemovedAppName = If(targetAppName = "&lt;Create New Application&gt;", "", targetAppName)
						objManageApp.RemovedAppSchemaName = targetSchemaName
						objManageApp.SourceSchemaName = sourceSchemaName
						objManageApp.SourceApplicationName = sourceAppName
						objManageApp.IsProductionApplication = makeProductionApplication
						objManageApp.TargetIsProductionApplication = targetIsProductionApplication = "True"
						objManageApp.DeleteOnDate = String.Empty
						objManageApp.EnvironmentType = envType
						objManageApp.ResetIIS = resetIIS
						objManageApp.ServiceNowRequestId = serviceNowRequestId

						If objManageApp.TaskType = "Create" Then
							identity = _catHelper.UpdateHistoryLog(si, objManageApp, Nothing, False)
							objManageApp.Id = identity
						Else
							' This is a replace so set the deleteOn Date formatted for a tag
							objManageApp.DeleteOnDate = _catHelper.GetDeleteOnDate(si, True)
							identity = _catHelper.UpdateHistoryLog(si, objManageApp, Convert.ToDateTime(_catHelper.GetDeleteOnDate(si, False)), False)
							objManageApp.Id = identity
						End If

						' ********* Perform Copy/Replace operation ************************
						executeResponse = _catHelper.ExecuteCopy(si, newDatabaseName, newApplicationName, sourceSchemaName, makeProductionApplication)

						If executeResponse = "CreateDatabaseAsCopy" Then
							' Copy request was sent, check if the copy has completed
							While Not IsCopyOperationComplete(si, newDatabaseName, False)
								System.Threading.Thread.Sleep(1 * 60000)
								runningTime += 1
								' Set a 5 hour limit to do the copy/replace operation
								If runningTime &gt; 300 Then
									Dim ex As New Exception("Operation timed out.") With {
										.Source = "CopyOperation"
									}
									Throw New CatException("Application Create/Replace exceeded 5 hours and has timed out." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0063].", ex)
								End If
							End While
							' Copy operation completed and the database is verified to exist
							' Add the new application name to the list in the session for checking/preventing duplicate app names
							Dim appNames As String = _catHelper.GetSessionStateValue(si, "ApplicationNames", "")
							_catHelper.SetSessionStateValue(si, "ApplicationNames", appNames &amp; "," &amp; newApplicationName)

							' Update the retention policies on the database
							If objManageApp.IsProductionApplication Then
								' This is a production application, need to add LTR to the new database
								executeResponse = _catHelper.ApplyRetentionPolicyToDatabase(si, newDatabaseName, DashboardExtender.CAT_SolutionHelper.MainClass.BackupRetentionPolicyType.LongTermRetention, retentionLtrValue)
								' Pause for 30 seconds to give a chance for operation to finish
								System.Threading.Thread.Sleep(1 * 30000)
							End If
							' Always add STR to the database
							executeResponse = _catHelper.ApplyRetentionPolicyToDatabase(si, newDatabaseName, DashboardExtender.CAT_SolutionHelper.MainClass.BackupRetentionPolicyType.ShortTermRetention, retentionStrValue)
							' Pause for 30 seconds to give a chance for operation to finish
							System.Threading.Thread.Sleep(1 * 30000)

							If objManageApp.TaskType = "Replace" Then
								' Remove LTR from replaced database if replaced database is prod
								If objManageApp.TargetIsProductionApplication Then
									executeResponse = _catHelper.ApplyRetentionPolicyToDatabase(si, targetSchemaName, DashboardExtender.CAT_SolutionHelper.MainClass.BackupRetentionPolicyType.None, String.Empty)
									' Pause for 30 seconds to give a chance for operation to finish
									System.Threading.Thread.Sleep(1 * 30000)
								End If
							End If
						Else
							' An error occurred when creating the copy job
							objManageApp.ErrorReason = executeResponse
							objManageApp.EndTime = DateTime.UtcNow
							objManageApp.Result = "Failed"
							' Update the history log with error
							_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
							Dim ex As New CatException("Could not start application copy operation." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0061]")
							ex.Source = "BeforeDatabaseCreate"
							Throw New CatException("Could not start application copy operation." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0061]", ex)
						End If

						' Configure diagnostic settings on the database
						executeResponse = _catHelper.ConfigureDiagnosticSettings(si, newDatabaseName)
						' Pause for 15 seconds to give a chance for operation to finish
						System.Threading.Thread.Sleep(1 * 15000)
						
						' Update tags on databases
						If objManageApp.TaskType = "Replace" Then
							' Set a deleteOn tag on the target database
							executeResponse = _catHelper.UpdateDatabaseTags(si, objManageApp, DashboardExtender.CAT_SolutionHelper.MainClass.RequestBodyAction.DeleteTarget)
						End If
						
						' Update database tables with the new or changed application information
						' Update the SystemInfo table in the application database
						newApplicationId = _catHelper.UpdateApplicationDatabase(si, newDatabaseName, newApplicationName, newApplicationName, targetSchemaName, objManageApp.TaskType)
						' Update the Framework database tables
						_catHelper.UpdateFrameworkDatabase(si, newDatabaseName, newApplicationName, newApplicationId, objManageApp.TaskType, targetSchemaName)

						' Update the history log
						objManageApp.EndTime = DateTime.UtcNow
						objManageApp.Result = "Success"
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)

						' Update ServiceNow with close request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.CloseChangeRequest)
							If Not serviceNowResultString = "Success" Then
								Dim ex As New CatException("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0040]")
								ex.Source = "ServiceNowCloseFailure"
								Throw New CatException("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0040]", ex)
							End If
						End If
						' Reset IIS
						If objManageApp.ResetIIS Then
							' Pause the job for 1 minute to allow any database updates to complete
							System.Threading.Thread.Sleep(1 * 60000)
							_catHelper.PerformIISReset(si, objManageApp.EnvironmentType)
						End If

					Catch ex As CatException When ex.InnerException.Source = "CopyOperation"
						BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, "Application Create/Replace exceeded 5 hours and has timed out." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0063].")
						' Update the history log with an error
						objManageApp.ErrorReason = ex.Message
						objManageApp.Result = "Failed"
						objManageApp.EndTime = DateTime.UtcNow
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							Dim statusDescription As String = String.Empty
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError, statusDescription)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, String.Format("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] {0}. {1}", serviceNowResultString, statusDescription))
							End If
						End If
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)

					Catch ex As CatException When ex.InnerException.Source = "ApplyDiagnostics"
						' Update the database created with a deleteOn tag
						_catHelper.UpdateDatabaseTags(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.RequestBodyAction.DeleteNewApp)
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							Dim statusDescription As String = String.Empty
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError, statusDescription)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, String.Format("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] {0}. {1}", serviceNowResultString, statusDescription))
							End If
						End If
						BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, "Could not apply diagnostic settings to the database." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0067]")
						' Update the history log with an error
						objManageApp.ErrorReason = ex.Message
						objManageApp.Result = "Failed"
						objManageApp.EndTime = DateTime.UtcNow
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)

						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					Catch ex As CatException When ex.InnerException.Source = "IISResetFailure"
						BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, ex.Message)
						objManageApp.ErrorReason = ex.Message
						objManageApp.Result = "Success"
						objManageApp.EndTime = DateTime.UtcNow
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Completed)
					Catch ex As CatException When ex.InnerException.Source = "ServiceNowFailure"
						BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, ex.Message)
						' Update the history log with an error
						objManageApp.ErrorReason = ex.Message
						objManageApp.Result = "Failed"
						objManageApp.EndTime = DateTime.UtcNow
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					Catch ex As CatException When ex.InnerException.Source = "ServiceNowCloseFailure"
						' Update the history log with a success
						' Failures closing SNOW ticket handled by cloud team manually
						objManageApp.ErrorReason = String.Empty
						objManageApp.Result = "Success"
						objManageApp.EndTime = DateTime.UtcNow
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Completed)
						' Reset IIS
						If objManageApp.ResetIIS Then
							' Pause the job for 1 minute to allow any database updates to complete
							System.Threading.Thread.Sleep(1 * 30000)
							_catHelper.PerformIISReset(si, objManageApp.EnvironmentType)
						End If
					Catch ex As CatException When ex.InnerException.Source = "GetDatabaseByName"
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							Dim statusDescription As String = String.Empty
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError, statusDescription)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, String.Format("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] {0}. {1}", serviceNowResultString, statusDescription))
							End If
							objManageApp.ErrorReason = ex.Message
							objManageApp.Result = "Failed"
							objManageApp.EndTime = DateTime.UtcNow
							_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						End If
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					Catch ex As CatException When ex.InnerException.Source = "BeforeDatabaseCreate"
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							Dim statusDescription As String = String.Empty
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError, statusDescription)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, String.Format("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] {0}. {1}", serviceNowResultString, statusDescription))
							End If
						End If
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					Catch ex As CatException When ex.InnerException.Source = "RetentionPolicies"
						' Update the database created with a deleteOn tag
						_catHelper.UpdateDatabaseTags(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.RequestBodyAction.DeleteNewApp)
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							Dim statusDescription As String = String.Empty
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError, statusDescription)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, String.Format("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] {0}. {1}", serviceNowResultString, statusDescription))
							End If
						End If
						BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, "Could not update the retention policies on the new application." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0064]")
						objManageApp.ErrorReason = ex.Message
						objManageApp.Result = "Failed"
						objManageApp.EndTime = DateTime.UtcNow
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					Catch ex As CatException When ex.InnerException.Source = "UpdateFrameworkDatabase"
						' Update the database created with a deleteOn tag
						_catHelper.UpdateDatabaseTags(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.RequestBodyAction.DeleteNewApp)
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							Dim statusDescription As String = String.Empty
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError, statusDescription)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, String.Format("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] {0}. {1}", serviceNowResultString, statusDescription))
							End If
						End If
						BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, "Could not update the framework database." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0024] " &amp; ex.InnerException.Message)
						objManageApp.ErrorReason = ex.Message
						objManageApp.Result = "Failed"
						objManageApp.EndTime = DateTime.UtcNow
						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					Catch ex As CatException
						' Update the database created with a deleteOn tag
						_catHelper.UpdateDatabaseTags(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.RequestBodyAction.DeleteNewApp)
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							Dim statusDescription As String = String.Empty
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError, statusDescription)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, String.Format("Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] {0}. {1}", serviceNowResultString, statusDescription))
							End If
						End If
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					Catch ex As Exception
						ex.Source = "ExecuteCopyMain"
						BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, ex.Message)
						objManageApp.ErrorReason = ex.Message
						objManageApp.Result = "Failed"
						objManageApp.EndTime = DateTime.UtcNow

						_catHelper.UpdateHistoryLog(si, objManageApp, Nothing, True)
						'Close the SNOW request
						If objManageApp.ServiceNowRequestId IsNot String.Empty Then
							serviceNowResultString = _catHelper.ServiceNowRequestCall(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.ServiceNowType.ReportError)
							If Not serviceNowResultString = "Success" Then
								BRApi.ErrorLog.LogError(si, XFErrorLevel.Error, "Communication error." &amp; vbNewLine &amp; "" &amp; vbNewLine &amp; "Contact OneStream Support and provide the following reference code for assistance. [OS-CAT-0042] " &amp; serviceNowResultString)
							End If
						End If
						' Only update the database tag on the target database
						_catHelper.UpdateDatabaseTags(si, objManageApp, OneStream.BusinessRule.DashboardExtender.CAT_SolutionHelper.MainClass.RequestBodyAction.DeleteNewApp)
						' Update the task activity
						UpdateTaskActivity(si, args, String.Empty, TaskActivityStatus.Failed)
					End Try
			End Select

			Return Nothing
		End Function



		''' &lt;summary&gt;
		''' Check the operation completion at a specific interval.
		''' &lt;/summary&gt;
		''' &lt;param name="si"&gt;&lt;/param&gt;
		''' &lt;param name="databaseName"&gt;&lt;/param&gt;
		''' &lt;param name="isUpdateDeleteOnTag"&gt;&lt;/param&gt;
		''' &lt;returns&gt;&lt;/returns&gt;
		Private Function IsCopyOperationComplete(si As SessionInfo, databaseName As String, isUpdateDeleteOnTag As Boolean) As Boolean
			' Check to see if the database has been created
			Dim subId As String = _catHelper.GetSessionStateValue(si, "SubscriptionId", String.Empty)
			Dim resourceGroupName As String = _catHelper.GetSessionStateValue(si, "ResourceGroupName", String.Empty)
			Dim serverName As String = _catHelper.GetSessionStateValue(si, "SourceServerName", String.Empty)
			Dim targetDatabase As DatabaseValue = _catHelper.GetDatabaseByName(si, databaseName, subId, resourceGroupName, serverName)

			If targetDatabase Is Nothing Then
				Return False
			Else
				If isUpdateDeleteOnTag Then
					If String.IsNullOrWhiteSpace(targetDatabase.Tags.DeleteOn) Then
						Return False
					Else
						Return True
					End If
				Else
					Return True
				End If
			End If
		End Function

		''' &lt;summary&gt;
		''' Updates the description in the Task Activity table
		''' &lt;/summary&gt;
		''' &lt;param name="si"&gt;&lt;/param&gt;
		''' &lt;param name="args"&gt;&lt;/param&gt;
		''' &lt;param name="description"&gt;&lt;/param&gt;
		Private Shared Sub UpdateTaskActivity(si As SessionInfo, args As ExtenderArgs, description As String, taskStatus As TaskActivityStatus)
			Dim sql As New StringBuilder
			If taskStatus = TaskActivityStatus.Failed Then
				sql.Append(String.Format("UPDATE [dbo].[TaskActivity] SET [TaskActivityStatus] = '{0}' WHERE [UniqueID] = '{1}'", SqlStringHelper.EscapeSqlString(TaskActivityStatus.Failed), args.TaskActivityID.ToString()))
			Else
				If Not String.IsNullOrWhiteSpace(description) Then
					sql.Append(String.Format("UPDATE [dbo].[TaskActivity] SET [Description] = '{0}' WHERE [UniqueID] = '{1}'", SqlStringHelper.EscapeSqlString(description), args.TaskActivityID.ToString()))
				Else
					sql.Append(String.Format("UPDATE [dbo].[TaskActivity] SET [TaskActivityStatus] = '{0}' WHERE [UniqueID] = '{1}'", SqlStringHelper.EscapeSqlString(TaskActivityStatus.Completed), args.TaskActivityID.ToString()))
				End If
			End If
			Try
				Using frameworkConn As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					Dim dt As DataTable = BRApi.Database.ExecuteSql(frameworkConn, sql.ToString, False)
				End Using
			Catch ex As SqlException
				ex.Source = "UpdateTaskActivity"
				Throw New CatException("There was an error updating the Task Activity List.", ex)
			Catch ex As DataException
				ex.Source = "UpdateTaskActivity"
				Throw New CatException("There was an error updating the Task Activity List.", ex)
			Catch ex As Exception
				ex.Source = "UpdateTaskActivity"
				Throw New CatException("There was an error updating the Task Activity List.", ex)
			End Try
		End Sub

	End Class
End Namespace