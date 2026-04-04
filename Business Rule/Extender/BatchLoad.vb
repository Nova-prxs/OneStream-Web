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

Namespace OneStream.BusinessRule.Extender.BatchLoad_TD
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
				#Region "Public Functions"					
					Case Is = ExtenderFunctionType.Unknown, ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						Dim bolError As Boolean

						'Step 1 - Define how WF POV is determined (Globals? Static value? System Time?)
						
'						Dim Period As Integer = args.NameValuePairs("WFTime")
'						Dim startmonth As Integer = Period
'						Dim Endmonth As Integer = Period
						Dim wfTime As String = args.NameValuePairs("WFTime")
						
						'use All and it will be executed for all WFs						
						Dim WFName As String = "All" '"Eurobaltic Fishverarbeitungs GmbH"'"Oderbank Hochseefischerei GmbH, Doggerbank Seefischerei GmbH"
						Dim wfScenario As String = args.NameValuePairs("WFScenario") '"Actual"
						Dim WFLoadSTEP As String = ".Import"
						Dim loadtype As String = args.NameValuePairs("LoadType")
						Dim WFtext2 As String = Nothing
'						If Not WFName.Equals("All")
'							WFLoadSTEP = WFName & WFLoadSTEP
'						End If
						
						Dim strwfname As String =  Nothing
						
'						Step 2 - Identify WF Profile(s) (names) that need To be executed 
						Dim wfList As New List(Of String)
						For Each pksWFLoadStep As WorkflowUnitClusterPk In Me.GetWFSteps(si,wfScenario,wfTime,LoadType)
							strwfname = Brapi.Workflow.Metadata.GetProfile(si, pksWFLoadStep).Name
							WFtext2 = BRApi.Workflow.Metadata.GetProfile(si, pksWFLoadStep).GetAttribute(ScenarioTypeID.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text2).ToString
							If loadtype.XFEqualsIgnoreCase(WFtext2) Or (loadtype.XFEqualsIgnoreCase("All") And Not WFtext2 = Nothing)
'							If BRApi.Workflow.Metadata.GetProfile(si, wfClusterPk).GetAttribute(ScenarioTypeID.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text2).ToString.Equals(LoadType)
					
''							Brapi.ErrorLog.LogMessage(si, "AN " &  strwfname & " - " & BRApi.Workflow.Metadata.GetProfile(si, pksWFLoadStep).GetAttribute(ScenarioTypeID.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text2).ToString)
'						Filter On WFText4
'						Dim Teradata As String = BRApi.Workflow.Metadata.GetProfile(si, pksWFLoadStep).GetAttribute(ScenarioTypeID.Actual, SharedConstants.WorkflowProfileAttributeIndexes.Text4).ToString
'												
						'Step 3 - Generate trigger file
							Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
							Dim seq As Integer = 1

						'Add condition to only run batch for WF Profile(s) that have Text4 value " "	
'							If Teradata = "Teradata" Then
							
'							For index As Integer = startmonth To Endmonth
'								wfTime = "2021Q" & index
								Me.CreateBatchFileTrigger(FileShareFolderHelper.GetBatchHarvestFolderForApp(si, True, configSettings.FileShareRootFolder, configSettings.FileShareBatchHarvestRootFolder,si.AppToken.AppName),seq.ToString & "-" & strwfname, wfScenario, wfTime)

'							Next
							
'						Else
'							Continue For
'						End If
							End If
						Next
						
'						Step 4 - Trigger Batch Process
						'Set Processing Switches	
						Dim valTransform As Boolean = True
						Dim valIntersect As Boolean =  True
						Dim loadCube As Boolean =  True
						Dim processCube As Boolean =  True
						Dim confirm As Boolean =  True
						Dim autoCertify As Boolean = False
						
						'Execute Batch (Request 2 Parallel File Groups)
						Dim batchInfo As WorkflowBatchFileCollection = BRApi.Utilities.ExecuteFileHarvestBatch(si, wfScenario, wfTime, valTransform, valIntersect, loadCube, processCube, confirm, autoCertify, False)
						
						Dim fileGroupId As Integer = 1

						'Send the Batch results to the administrator
						If Not batchInfo Is Nothing Then
'							BRApi.ErrorLog.LogMessage(si, "Not batchInfo Is Nothing")
							
							'Prepare the batch summary
							Dim batchSummary As New Text.StringBuilder
							batchSummary.AppendLine("_____________________________________________________________________________________")
							batchSummary.AppendLine("")
							batchSummary.AppendLine("Batch Name:			" & batchInfo.GetBatchName)
							batchSummary.AppendLine("File Group Name:		" & batchInfo.GetFileGroupName(fileGroupId))
							batchSummary.AppendLine("All File Count:		" & batchInfo.GetFileCount(fileGroupId, False))
							batchSummary.AppendLine("Processed File Count:	" & batchInfo.GetFileCount(fileGroupId, True))
							batchSummary.AppendLine("All Files Completed:	" & batchInfo.AllFilesCompleted(fileGroupId).ToString)
							batchSummary.AppendLine("All Files Loaded:	" 	  & batchInfo.AllFilesLoaded(fileGroupId).ToString)
							batchSummary.AppendLine("All Files Can Process:	" & batchInfo.AllFilesCanProcess(fileGroupId).ToString)
							batchSummary.AppendLine("All Files Processed:	" & batchInfo.AllFilesProcessed(fileGroupId).ToString)
							batchSummary.AppendLine("_____________________________________________________________________________________")
							batchSummary.AppendLine("")
							batchSummary.AppendLine("")

							'Prepare the batch detail
							Dim batchDetail As New Text.StringBuilder
							batchDetail.AppendLine("File Details:")
							batchDetail.AppendLine("_____________________________________________________________________________________")
							
							For Each xfBatchFile As WorkflowBatchFileInfo In batchInfo.FileGroups(fileGroupId)

								batchDetail.AppendLine(xfBatchFile.FileName & xfBatchFile.FileExtension)
								batchDetail.AppendLine("------------------------------------------------------------------------------------")
								batchDetail.AppendLine("Fully Processed: " & xfBatchFile.FileProcessed.ToString)

								If Not xfBatchFile.FileProcessed Then
									batchDetail.AppendLine("Failure Information:")
									batchDetail.AppendLine("********************")
									batchDetail.AppendLine(xfBatchFile.ProcessMessage)
									
									bolError = True
								End If
								
'								batchDetail.AppendLine("")
'							strwfname2 = strwfname & ", "  & strwfname2
							
							Next
							batchDetail.AppendLine("")
							batchDetail.Append(batchSummary.ToString)
							
							'Email Batch Results
'							BRApi.ErrorLog.LogMessage(si, "Batch Status: " & batchDetail.ToString)
							
							If bolError Then
								'Email errors only to ticket system
'								Me.SendMail(si, "<email address>", batchInfo.GetBatchTitleMessage, batchDetail.ToString, String.Empty)
							End If
						Else
							BRAPi.ErrorLog.LogMessage(si, "Batch Execution Failed")
						End If	
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region


#Region "Private Functions"

		Private Sub CreateBatchFileTrigger(ByVal harvestPath As String, ByVal WFName As String, ByVal wfScenario As String, ByVal wfTime As String)
			Dim path As String = harvestPath & "\" & wfName & "-" & wfScenario & "-" & wfTime & "-R.txt"

			If Not File.Exists(path) Then
				' Create a file to write to. 
				Using sw As StreamWriter = File.CreateText(path)
					sw.WriteLine("Batch File Trigger")
				End Using
			End If
		End Sub

		Private Function GetWFSteps(ByVal si As SessionInfo, ByVal scenarioName As String, ByVal WFTime As String, ByVal LoadType As String) As List(Of WorkflowUnitClusterPk)
			Try
			Dim wfClusterPks As New List(Of WorkflowUnitClusterPk)
			'Define the SQL Statement
			
			Dim WFStep As String = ".Import"
			Dim sql As New Text.StringBuilder     
			sql.Append("Select Distinct ProfileName ")
			sql.Append("From ")           
			sql.Append("WorkflowProfileHierarchy ")
			sql.Append("Where (")
			sql.Append("ProfileName LIKE '" & "%" & WFStep & "%" & "'")
			sql.Append(")")
			sql.Append("Order By ")
			sql.Append("ProfileName ")

			'Create the list of WorkflowUnitClusterPks
			Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRAPi.Database.ExecuteSql(dbConnApp, sql.ToString, False) 
					For Each dr As DataRow In dt.rows
'						Dim year As String = left(timeName,4)
						Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName"), scenarioName, WFTime)
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
		
		
#End Region 
	End Class
End Namespace