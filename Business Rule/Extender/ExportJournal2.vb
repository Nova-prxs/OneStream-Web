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

Namespace OneStream.BusinessRule.Extender.ExportJournal2
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					
				Case Is = DashboardExtenderFunctionType.LoadDashboard
'						If (args.FunctionName.XFEqualsIgnoreCase("OnLoadMainDashboard")) Then	
'							If args.LoadDashboardTaskInfo.Reason = LoadDashboardReasonType.Initialize And args.LoadDashboardTaskInfo.Action = LoadDashboardActionType.BeforeFirstGetParameters Then
'								'Initialize Parameters
'								Dim loadDashboardTaskResult As New XFLoadDashboardTaskResult()
'								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = True
'								loadDashboardTaskResult.ModifiedCustomSubstVars.Add("WorkflowProfileListAdjSource_JRF", Me.GetUserWFSetting(si, "Profile"))
'								loadDashboardTaskResult.ModifiedCustomSubstVars.Add("WorkflowScenarioListSource_JRF", Me.GetUserWFSetting(si, "Scenario"))
'								loadDashboardTaskResult.ModifiedCustomSubstVars.Add("WorkflowTimeListSource_JRF", Me.GetUserWFSetting(si, "Time"))

'								loadDashboardTaskResult.ModifiedCustomSubstVars.Add("SelectWorking_JRF", Me.GetUserWFSetting(si, "Working"))
'								loadDashboardTaskResult.ModifiedCustomSubstVars.Add("SelectSubmit_JRF", Me.GetUserWFSetting(si, "Submit"))
'								loadDashboardTaskResult.ModifiedCustomSubstVars.Add("SelectApprove_JRF", Me.GetUserWFSetting(si, "Approve"))
'								loadDashboardTaskResult.ModifiedCustomSubstVars.Add("SelectPost_JRF", Me.GetUserWFSetting(si, "Post"))	   
						
'							Return loadDashboardTaskResult
									      
				       
'					End If
'				End If
					End Select
				    

				Return Nothing
				
				'Import, Save and Process a Journal created in an Excel Template
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si, Nothing)
				    Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si, Nothing)	
				
				   ' We define Path and FileName			        
				        Dim networkPath As String = "\\INGCORONESWEB\OneStream\Fileshare\Applications\Ingeteam_DES\DataManagement\Export"                '<- Network path to journal file						
						Dim fileName As String = "ExportJournal.csv"
						
						'Dim fileName As String = "EX_" & DateTime.UtcNow.ToString("yyyyMMdd") & "_" & wfProfileName & "_WF" & wfYear & wfMonth & ".csv"   '<- Journal file name				       
						Dim filePath As String = networkPath & "\" & fileName
				                
				        'Export				        
'				        Dim csv As String = BRApi.Journals.Data.ExportJournalsToCsv(si, filePath, wfProfileName, scenarioName, "T#WFPrior1", "Posted")
						
						End Using
						End Using
						
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace