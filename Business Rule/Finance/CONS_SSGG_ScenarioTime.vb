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

Namespace OneStream.BusinessRule.Finance.CONS_SSGG_ScenarioTime
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				Select Case api.FunctionType
																				
					Case Is = FinanceFunctionType.MemberList
						
						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("ScenarioTime") Then
						
							Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
							Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, myWorkflowUnitPk.TimeKey)
							Dim objTimeMemberSubComponents As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
							Dim wfMes As String = objTimeMemberSubComponents.Month.ToString
							Dim wfScenarioName As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
							Dim sScenario As String = String.Empty
																
							Dim DatoO3 = api.Data.GetDataCell("A#CR_Z:S#O3:T#POVLastInYear:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
							Dim DatoO2 = api.Data.GetDataCell("A#CR_Z:S#O2:T#POVLastInYear:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
							Dim DatoO1 = api.Data.GetDataCell("A#CR_Z:S#O1:T#POVLastInYear:C#Local:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#None:U6#None:U7#None:U8#None").CellAmount
			
						
						If wfScenarioName = "R" Then	
							If wfMes < 6 Then
								
								If DatoO3 <> 0 Then
									sScenario = "O3"
								Else If DatoO2 <> 0 Then
									sScenario = "O2"
								Else 
									sScenario = "O1"
								End If																	
							
							Else If wfMes > 5 And wfMes < 12 Then
								
							 sScenario = "P06"
							 
							Else
								
							 sScenario = "R"
								
							End If
						 Else If wfScenarioName = "P03" Then	
						 	If DatoO3 <> 0 Then
								sScenario = "O3"
							Else If DatoO2 <> 0 Then
								sScenario = "O2"
							Else 
								sScenario = "O1"
							End If	
						Else If (wfScenarioName = "P06" Or wfScenarioName = "P09" Or wfScenarioName = "P11") Then
							sScenario = "P06"
						Else If (wfScenarioName = "O1" Or wfScenarioName = "O2" Or wfScenarioName = "O3") Then
							sScenario = wfScenarioName
						End If
						
						sScenario = "S#" & sScenario
												
						
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sScenario, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							
							Return objMemberList
							
'xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx													
					'AQUI INDICAMOS LA CUENTA DE INVESIONES DE CONTROLLING EN FUNCION DEL MES							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("CONT_Account_Inv") Then	
							
							Dim periodNumber As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
							Dim sScenario As String = api.Pov.Scenario.Name
							Dim sAccount As String = ""
							
							If periodNumber = 12 And sScenario = "R" Then																
								sAccount = "A#Prev_Inv:U3#[Total Clientes]:U4#None:Name(B. Inversiones - Investments)"								
							Else 								
								sAccount = "A#Pres_Aprob_Año_N:U3#[Total Clientes]:U4#None:Name(B. Inversiones - Investments)"															
							End If
							
							Dim objMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)														
							Dim objMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(args.MemberListArgs.DimPk, sAccount, Nothing)							
							Dim objMemberList As New MemberList(objMemberListHeader, objMemberInfos)
							
							Return objMemberList
							
						End If		
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
	End Class
	
End Namespace