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

Namespace OneStream.BusinessRule.Extender.AQS_WF_Management
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						Dim intyear As Integer = 2025
						Dim strSQL As String = "SELECT ProfileName FROM [WorkflowProfileHierarchy] 
												WHERE ProfileName LIKE '%_PLAN%' AND ProfileName LIKE '%.PL' "
						
						Dim ListScenario As List(Of member) = BRApi.Finance.Members.GetBaseMembers(si,brapi.Finance.Dim.GetDimPk(si, "Scenario_100_Top") ,dimconstants.Root)
'						S#BUDGET_GROUP, S#Q_FCST_GROUP, S#FC_GROUP
						Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
						'Get the workflow Info object, retrieve and the Workspace Workflow task and set its status ti COMPLETED
'						Dim wfClusterPk As WorkflowInfo = si.WorkflowClusterPk
'						Dim wfTask As TaskInfo = wfStatus.GetTask(New Guid(SharedConstants.WorkflowKeys.Tasks.Workspace))
						'Update the workspace workflow to COMPLETED
						Dim wfRegClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, Si.WorkflowClusterPk)
						
						Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
						Using dt As DataTable = BRAPi.Database.ExecuteSql(dbConnApp, strSQL.ToString, False) 
						If Not dt.rows.Count = 0 
							For Each dr As DataRow In dt.rows
								For Each memScenario As Member In ListScenario
									'IF memScenario.Name.XFContainsIgnoreCase("FC_") Or 
									 If memScenario.Name.XFContainsIgnoreCase("Budgetv1") 'Or
									 'If memScenario.Name.XFContainsIgnoreCase("Q_FCST_8_4")					
										Brapi.Workflow.Locking.LockWorkflowUnit(si, BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, dr("ProfileName").ToString, memScenario.Name, intyear))
									End If 
								Next 
							Next 
						End If
						End Using
						End Using
						selectionChangedTaskResult.IsOK = True
						selectionChangedTaskResult.ShowMessageBox = False
						selectionChangedTaskResult.WorkflowWasChangedByBusinessRule = True
						Return selectionChangedTaskResult
				End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace