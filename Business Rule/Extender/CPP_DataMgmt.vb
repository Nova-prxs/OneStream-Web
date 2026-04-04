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

Namespace OneStream.BusinessRule.Extender.CPP_DataMgmt
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		CPP_DataMgmt
		'
		'Description:			Capital Planning Data Management Business Rule Step Code.
		'
		'Usage:					This business rule is intended to be called from a Data Management Step
		'------------------------------------------------------------------------------------------------------------
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Prepare Parameters
						Dim singleCalc As Boolean = ConvertHelper.ToBoolean(args.NameValuePairs.XFGetValue("SingleCalc"))
						Dim profileKey As Guid = New Guid(args.NameValuePairs.XFGetValue("ProfileKey"))
						Dim scenarioId As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("ScenarioId"))
						Dim timeId As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("TimeId"))
						Dim wfClusterPK As New WorkflowUnitClusterPk(profileKey, scenarioId, timeId)
						Dim batchSize As Integer = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("BatchSize", "100"))
						Dim useParallelSave As Boolean = ConvertHelper.ToInt32(args.NameValuePairs.XFGetValue("UseParallelSave", "True"))

						'Set Task Description
						Me.SetTaskDescription(si, args)

						'Execute Plan Calculation
						Dim cppHelper As New OneStream.BusinessRule.DashboardExtender.CPP_SolutionHelper.MainClass
						If singleCalc Then
							'Execute a single plan calculation (Excuted as part of Complete Workflow)
							Dim wfClusterPks As New List(Of WorkflowUnitClusterPk)
							wfClusterPks.Add(wfClusterPk)
							Dim taskActivityItemCount As Integer = cppHelper.GetTaskActivityItemCount(si, wfClusterPks)
							cppHelper.CalculateSinglePlan(si, globals, wfClusterPk, taskActivityItemCount, batchSize, useParallelSave, args)
						Else
							'Execute a user initiated calculation (Calculate Button)
							cppHelper.CalculateOneOrAllPlans(si, globals, wfClusterPk, args, True)
						End If

				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#Region "Data Management Helper Functions"

		Private Sub SetTaskDescription(ByVal si As SessionInfo, ByVal args As ExtenderArgs)
			Try

				'Update the task activity description with the Workflow Information
				Dim wfDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
				Dim description As String = "Capital Plan Calc (" & wfDesc & ")"
				Dim dml As String = "UPDATE TaskActivity Set Description = '" & SqlStringHelper.EscapeSqlString(description) & "' Where (UniqueID = '" & args.TaskActivityID.ToString & "')"
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					BRApi.Database.ExecuteActionQuery(dbConnFW, dml, False, False)
				End Using

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Sub

#End Region

	End Class
End Namespace