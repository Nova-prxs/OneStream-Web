#Region "Imports"
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
#End Region '"

'Extract from the BR extensibilty for parallel run
'1- A DM Sequence called AQS_Autoload_Parallel01 runs a Step called AQS_Autoload_Parallel02
'2- The Step AQS_Autoload_Parallel02 runs the BR RunAutoloadParallel
'3- The BR RunAutoloadParallel then loops to all WF and runs many DM Sequence AQS_Autoload_Parallel03
'4- The DM Sequence AQS_Autoload_Parallel03 then runs many import Steps AQS_Autoload_Parallel04 in parallele.

'This BR AQS_Autoload_Parallel04 is the one launching the loop in step 4.
'To use it we assigned a parameter sProfileID =|!sProfileID!| in the DM Step calling this BR.

Namespace OneStream.BusinessRule.Extender.AQS_Autoload_Parallel04
	Public Class MainClass
		Dim ActiveLogs As Boolean = False
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep

						If args.DataMgmtArgs.CurrentStep.Step.Name = "AQS_Autoload_Parallel04" Then
							Dim sProfileID As String = args.NameValuePairs.XFGetValue("sProfileID")
							Dim parts() As String = sProfileID.Split("_"c)
							
''							'WFP NAME : Get Current WorkFlow Profile Name
							Dim sProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, guid.Parse(parts(0))).Name
							
'							'SCENARIO : Get Dimension type id for a specific member
							Dim iDimTypeScenario As Integer = DimType.Scenario.Id						
							Dim sScenarioName As String = BRApi.Finance.Members.GetMemberName(si, iDimTypeScenario, parts(1))

							
							'TIME : Get Dimension type id for a specific member
							Dim iDimTypeTime As Integer = DimType.Time.id
							Dim sTimeName As String = BRApi.Finance.Members.GetMemberName(si, iDimTypeTime, parts(2))
							
							If ActiveLogs Then brapi.ErrorLog.LogMessage(si, "INFO : sTimeName > " & sTimeName & "  sScenarioName > " & sScenarioName  & "  sProfileName > " & sProfileName)
							
							'WFPK : Get the Workflow Profile Unit PK
							Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, sProfileName, sScenarioName, sTimeName)

							AutoCompleteLoadAndProcessWF(si, wfClusterPk)
							
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "AutoCompleteLoadAndProcessWF"
Public Function AutoCompleteLoadAndProcessWF(ByVal si As SessionInfo, ByVal pksWFLoadStep As WorkflowUnitClusterPk)
    Dim completed As Boolean = False
    Dim emptyByteArray As Byte() = New Byte() {}

    ' Check if the workflow step is not nothing
    If Not pksWFLoadStep Is Nothing Then
        ' Execute IMPORT-VALIDATE-LOAD-PROCESS workflow
        Dim impProcessInfo As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, pksWFLoadStep, "", emptyByteArray, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, False)
        If impProcessInfo.Status <> WorkflowStatusTypes.Completed Then
			If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: Import process did not complete successfully.")
            Return False
        End If

        ' Validate Transformation (Mapping)
        Dim valTranProcessInfo As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, pksWFLoadStep, True)
        If valTranProcessInfo.Status <> WorkflowStatusTypes.Completed Then
           If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: Validation of transformation did not complete successfully.")
            Return False
        End If

        ' Validate Intersections
        Dim valIntersectProcessInfo = BRApi.Import.Process.ValidateIntersections(si, pksWFLoadStep, True)
        If valIntersectProcessInfo.Status <> WorkflowStatusTypes.Completed Then
            If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: Validation of intersections did not complete successfully.")
            Return False
        End If

        ' Load the cube
        Dim lcProcessInfo = BRApi.Import.Process.LoadCube(si, pksWFLoadStep)
        If lcProcessInfo.Status <> WorkflowStatusTypes.Completed Then
            If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "ERROR: Load cube process did not complete successfully.")
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

    If ActiveLogs Then BRApi.ErrorLog.LogMessage(si, "--------completed : " & completed.ToString)
    Return completed
End Function		
#End Region '	AutoCompleteLoadAndProcessWF	
	End Class
	
	
End Namespace