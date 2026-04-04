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

Namespace OneStream.BusinessRule.Extender.journalsextract
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				
				Dim wfProfileName As String = String.Empty
				Dim wfScenarioName As String = String.Empty
				Dim wfTimeMemberFilter As String = String.Empty
				Dim journalStatus As String = String.Empty
				'Dim TimeNow As String = BRApi.Workflow.General.GetGlobalTime(si)
				Dim WFunitPK As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
				Dim TimeNow As String = BRApi.Finance.Time.GetNameFromId(si, WFunitPK.TimeKey)
				'-- Get the user details
				Dim objUser As UserInfo = BRApi.Security.Authorization.GetUser(si, si.AuthToken.UserName)
				Dim UserText4 As String = objUser.User.Text4
				Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
				'Dim folderPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\" & DateTime.UtcNow.ToString("yyyyMMdd") & "\journals"
				Dim folderPath As String = FileShareFolderHelper.GetDataManagementExportUsernameFolderForApp(si, True, configSettings.FileShareRootFolder, si.AppToken.AppName) & "\journals"
				If Not Directory.Exists(folderPath) Then Directory.CreateDirectory(folderPath)
				Dim serverFilePath As String = folderPath & "\DataExtract_" & TimeNow & ".csv"	
				'Dim serverFilePath2 As String = folderPath & "\DataExtract2_" & TimeNow & ".csv"	
				If File.Exists(serverFilePath) Then File.Delete(serverFilePath) 
				Dim sfilePath_tmp As String = folderPath & "\JNL_Extract_" & UserText4 & "_" & TimeNow & ".csv"	
				If File.Exists(sfilePath_tmp) Then 
					File.Delete(sfilePath_tmp)
				End If
				Dim sUD8member As String = Nothing
				Dim blnExport As Boolean = False
				Select Case args.FunctionType
					Case Is = ExtenderFunctionType.Unknown
						wfProfileName = "Hist.Adj"
						wfScenarioName = "Actual"
						wfTimeMemberFilter = "T#|WFTime|"
						journalStatus ="Posted"
				Dim sValue As String = BRApi.Journals.Data.ExportJournalsToCsv(si, serverFilePath, wfProfileName, wfScenarioName, wfTimeMemberFilter, journalStatus)
				Dim Typesofrow As String = Nothing
				'Examine each line
				For Each sLine As String In System.IO.File.ReadLines(serverFilePath)
					Dim sTmp() As String  = sline.Split(",")
					Typesofrow = sTmp(0) & "_" & sTmp(0)
'					brapi.Workflow.Metadata.GetProfile().for														
					If sTmp(0) = "!H (RowType Header)" Then
						'Only write the line out
						 file.WriteAlltext(sfilePath_tmp, sLine + Environment.NewLine)
					ElseIf sTmp(0) = "!D (RowType Detail)" Then
						'Only write the line out
						 file.AppendAlltext(sfilePath_tmp, sLine + Environment.NewLine)
					ElseIf sTmp(0) = "H" Then
						 file.AppendAlltext(sfilePath_tmp, sLine + Environment.NewLine)
					ElseIf sTmp(0) = "D" Then
						sUD8member = sTmp(15)
						'If sUD8member = "Group1" Then
							file.AppendAlltext(sfilePath_tmp, sLine + Environment.NewLine)
						'End If
					End If
				Next sLine
						
						
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						wfProfileName =args.NameValuePairs("WfProfileName")
						wfScenarioName = args.NameValuePairs("wfScenarioName")
						wfTimeMemberFilter = args.NameValuePairs("wfTimeMemberFilter")
						'journalStatus =args.NameValuePairs("journalStatus")
						journalStatus ="Posted"
						
				
						Dim sValue1 As String = BRApi.Journals.Data.ExportJournalsToCsv(si, serverFilePath, wfProfileName, wfScenarioName, wfTimeMemberFilter, journalStatus)
						For Each sLine As String In System.IO.File.ReadLines(serverFilePath)
							Dim sTmp() As String  = sline.Split(",")		
							If sTmp(0) = "!H (RowType Header)" Then
								'Only write the line out
								 file.WriteAlltext(sfilePath_tmp, sLine + Environment.NewLine)
							ElseIf sTmp(0) = "!D (RowType Detail)" Then
								'Only write the line out
								 file.AppendAlltext(sfilePath_tmp, sLine + Environment.NewLine)
							ElseIf sTmp(0) = "H" Then
								 file.AppendAlltext(sfilePath_tmp, sLine + Environment.NewLine)
							ElseIf sTmp(0) = "D" Then
								sUD8member = sTmp(15)	
					'brapi.ErrorLog.LogMessage(si, "sUD8member = " & sUD8member)
							  file.AppendAlltext(sfilePath_tmp, sLine + Environment.NewLine)
							End If

						Next sLine

					End Select

				
	
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace