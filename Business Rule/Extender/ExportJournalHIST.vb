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

Namespace OneStream.BusinessRule.Extender.ExportJournalHIST
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				'Import, Save and Process a Journal created in an Excel Template
				'Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si, Nothing)
				  '  Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si, Nothing)
				       
'						Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
'						Dim wfClusterPk As New WorkflowUnitClusterPk
'						Dim currentProfileName As WorkflowProfileInfo = BRAPi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
'						Dim wfProfileName As String = currentProfileName.Name
'						Dim wfYear As Integer = BRApi.Finance.Time.GetYearFromId(si, myWorkflowUnitPk.TimeKey)
'						Dim wfMonth As Integer = BRApi.Finance.Time.GetPeriodNumFromId(si, myWorkflowUnitPk.TimeKey)
'						Dim scenarioName As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
						Dim userName As String = si.AuthToken.UserName
						
'				        Dim save As Boolean = True  
'				        Dim submit As Boolean = True              '<- True/False flag to specify if WF should be Submited
'				        Dim approve As Boolean = True             '<- True/False flag to specify if WF should be Approved
'				        Dim post As Boolean = True                '<- True/False flag to specify if WF should be Posted
'				        Dim unpostAndOverwrite As Boolean = False '<- True/False flag to specify if existing Journal should be unposted & overwritten
'				        Dim throwOnError As Boolean = True        '<- True/False flag to specify if error should be thrown if error occurs during journal import
				        
				        'Dim networkPath As String = "C:\onestream\Fileshare\Applications\Desarrolllo\DataManagement\Export\novatest\" & wfYear & "_" & wfMonth  '<- Network path to journal file
				       ' Dim networkPath As String = "C:\onestream\Fileshare\Applications\Ingeteam_PROD\DataManagement\Export\" &username  '<- Network path to journal file
				        Dim networkPath As String = "\\INGCORONESWEB\OneStream\Fileshare\Applications\Ingeteam_PROD\DataManagement\Export\"               '<- Network path to journal file						
										        
						Dim fileName As String = "EX_" & DateTime.UtcNow.ToString("yyyyMMdd") & "_" & "_" & "CONS_HIST" & "_WF" & "2019M12.csv"                           '<- Journal file name
				        Dim filePath As String = networkPath & "\" & fileName
				                
				        'Export
				        'Dim sValue As String = BRApi.Journals.Data.ExportJournalsToCsv(si, "C:\Program Files\Internet Explorer\iexplore.exe", "Carga Historicos", "R", "T#|WFYear|.Base", "Posted")
				        Dim csv As String = BRApi.Journals.Data.ExportJournalsToCsv(si, filePath, "CONS_HIST", "R", "T#2019M12", "Posted")
						
				  '  End Using
				'End Using

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace