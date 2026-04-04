Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine

Namespace OneStream.BusinessRule.Finance.HRS_Utilities
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			
			Try
				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
	
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then

					If sFunction = "CopyDataWithBufferF00IToF99I" Then												
						
						Me.CopyDataWithBufferF00IToF99I(si, api, globals, args)
						
			
					ElseIf sFunction = "CopyDataForecast" Then												
						
						Me.CopyDataForecast(si, api, globals, args)						
						
					ElseIf sFunction = "SetNoneFilter" Then
						
						Me.SetNoneFilter(si, api, globals, args)
						Me.SetNoneFilter2(si, api, globals, args)
						
					ElseIf sFunction = "CompleteWorkflow" Then
						
						'BRAPI.ErrorLog.LogMessage(si,"Entra")
						
						Me.CompleteWorkflow(si, globals, api, args)						
					
					End If	
					
				End If
					
				If sFunction = "CopyDataWithBufferActToFcst" Then						
											
						Me.CopyDataWithBufferActToFcst(si, api, globals, args)		
				
				
				Else If sFunction = "CopyDataWithBufferActToFcst_Brazil" Then						
											
						Me.CopyDataWithBufferActToFcst_Brazil(si, api, globals, args)	
						Me.CopyDataWithBufferActToFcst_Brazil_RF04(si, api, globals, args)
						
				Else If sFunction = "CopyDataWithBufferActToFcst_R1300003_R1300004" Then						
											
						Me.CopyDataWithBufferActToFcst_R1300003_R1300004(si, api, globals, args)		
						Me.CopyDataWithBufferActToFcst_R1300003_R1300004_RF04(si, api, globals, args)
				
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		Sub SetBalanceCrossAccountsFlows (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		

			Dim results As New DataBuffer	
			Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula("S#Budget_V1:E#Horse_Group:O#Top:F#99:T#2024")
	
			For Each db1Cell As DataBufferCell In dataToCopy.DataBufferCells.Values
				Dim resultCell As New DataBufferCell(db1Cell)				
				results.SetCell(si, resultCell, False)			

			Next
	
			Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Budget_V1:E#Horse_Group:O#Top:F#99:T#2024")
			api.Data.SetDataBuffer(results, exprDestInfo,,,,,,,,,,,,,True)
	
		End Sub
		
		Sub SetNoneFilter (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		

			Dim results As New DataBuffer	
			Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula("S#Actual:E#Horse_Group:O#Top")
	
			For Each db1Cell As DataBufferCell In dataToCopy.DataBufferCells.Values
				Dim resultCell As New DataBufferCell(db1Cell)
				resultCell.CellAmount = resultCell.CellAmount - resultCell.CellAmount
				results.SetCell(si, resultCell, False)			

			Next
	
			Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Budget_V1:E#Horse_Filter:O#Forms")
			api.Data.SetDataBuffer(results, exprDestInfo,,,,,,,,,,,,,True)
	
		End Sub		
		
		Sub SetNoneFilter2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
						
			Dim db As DataBuffer = api.Data.GetDataBufferUsingFormula("S#Budget_V1:E#Horse_Filter")
	
			Dim filters_Account As New Dictionary(Of Integer, Object)
			For Each MemberI As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account_Details", "A#BS.Base.Remove(A#3001090C,A#2901730)", True)
				filters_Account.Add(MemberI.Member.MemberId, MemberI.Member.Name)
			Next
			
			Dim filters_IC As New Dictionary(Of Integer, Object)
			For Each MemberI As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "IC", "I#R1300001, I#R1301001, I#R0671001, I#R0611001, I#R1302001, I#R0592001, I#R0585001, I#R1303001, I#None", True)
				filters_IC.Add(MemberI.Member.MemberId, MemberI.Member.Name)
			Next	
			
			
			
			db = db.GetFilteredDataBuffer(DimType.Account, filters_Account)			
			db = db.GetFilteredDataBuffer(DimType.IC, filters_IC)				
			If Not db Is Nothing Then	
			

				For Each dbc As DataBufferCell In db.DataBufferCells.Values
					
					If (Not dbc.CellStatus.IsNoData)
					
						
						Dim sAccount = api.Members.GetMemberName(DimType.Account.Id,dbc.DataBufferCellPk.AccountId)
						Dim sIC = api.Members.GetMemberName(DimType.IC.Id,dbc.DataBufferCellPk.ICId)
					
'					If sAccount = "1101000C" And sIC = "R0585001" Then 
'						Brapi.ErrorLog.LogMessage(si,sAccount)
'					End If
						'Brapi.ErrorLog.LogMessage(si,"Account: " & sAccount)
						
						api.Data.Calculate("A#" & sAccount & ":F#F15:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = A#" & sAccount & ":F#F99:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#" & sAccount & ":F#F20:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = A#" & sAccount & ":F#F99:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#" & sAccount & ":F#F25:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = A#" & sAccount & ":F#F99:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#" & sAccount & ":F#F30:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = A#" & sAccount & ":F#F99:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
						api.Data.Calculate("A#" & sAccount & ":F#F35:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = A#" & sAccount & ":F#F99:S#Budget_V1:I#" & sIC & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None")
					
					End If
					
				Next
				
			End If
	
		End Sub				
						
		Sub CopyDataWithBuffer (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ByVal sFormulaSource As String, ByVal sFormulaTarget As String)		

			Dim results As New DataBuffer	
			Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula(sFormulaSource)			
	
			For Each db1Cell As DataBufferCell In dataToCopy.DataBufferCells.Values
				Dim resultCell As New DataBufferCell(db1Cell)				
				results.SetCell(si, resultCell, False)			

			Next
	
			Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo(sFormulaTarget)
			api.Data.SetDataBuffer(results, exprDestInfo,,,,,,,,,,,,,True)
	
		End Sub	
'____________________________________________________________________________________________________________________________________________________________
'____________________________________________________________________________________________________________________________________________________________
			
'		Sub CopyDataWithBufferActToFcst (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		

'		api.Data.Calculate("S#RF06=S#Actual",True)
		
		Sub CopyDataWithBufferActToFcst (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	
		
		' Leer los parámetros SourceCopyScenario, TargetCopyScenario y TimeCopy
		Dim sourceCopyScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourceScenario")
		Dim targetCopyScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("DestinationScenario")
				
		
		' Construir la expresión de cálculo dinámica
		Dim calculationExpression As String = "S#" & targetCopyScenario  & "=S#" & sourceCopyScenario 
		
		' Ejecutar el cálculo
		api.Data.Calculate(calculationExpression, True)
	
		End Sub		
			
'____________________________________________________________________________________________________________________________________________________________
'____________________________________________________________________________________________________________________________________________________________

'____________________________________________________________________________________________________________________________________________________________
'____________________________________________________________________________________________________________________________________________________________
			
		
		Sub CopyDataWithBufferActToFcst_Brazil (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)				

		Dim sScenario As String = api.Pov.Scenario.Name	
		Dim sEntity As String = api.Pov.Entity.Name
		
			If (sScenario = "Actual_Allocations")  Then
		
				api.Data.ClearCalculatedData("E#" & sEntity & ":S#" & sScenario, True, True, True, True)
		
					If sEntity = "R1303001"  Then
						
						'Get source data buffer and declare destination info
						Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(E#R1303:C#EUR:S#Actual:P#Horse_Group:V#YTD)")
						Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
						Dim targetDataBuffer As New DataBuffer
						
						' Recuperamos ids de monedas EUR y BRL
						Dim EURId As Integer = Currency.EUR.Id
						Dim BRLId As Integer = Currency.BRL.Id
						
						' Definimos variables necesarias para el conseguir el tipo de cambio
						Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
						Dim cubeId As Integer = api.Pov.Cube.CubeId
						Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		
						' Obtenemos el tipo de cambio
						Dim conversionRate As Decimal = api.FxRates.GetCalculatedFxRate(rateType, cubeId, timeId, EURId, BRLId)
						
						For Each sourceCell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
							Dim targetCell As New DataBufferCell(sourceCell)
							targetCell.CellAmount = targetCell.CellAmount * conversionRate
							targetDataBuffer.SetCell(si, targetCell, True)
							
						Next
						'Set Data buffer
						api.Data.SetDataBuffer(targetDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						'api.Data.Calculate("E#R1303001 = E#R1303:C#EUR:S#Actual:P#Horse_Group", True)
						
					End If		
						
		'-------------------------------------------------------------------------------------------------------------------------------			
					If sEntity = "R1303001" Then
						
						api.Data.Calculate("E#" & sEntity & ":A#PctCON:I#R1303:S#Actual_Allocations = E#" & sEntity & ":A#PctCON:I#R1303:S#Actual", True)
						api.Data.Calculate("E#" & sEntity & ":A#PctOWN:I#R1303:S#Actual_Allocations = E#" & sEntity & ":A#PctOWN:I#R1303:S#Actual", True)
						api.Data.Calculate("E#" & sEntity & ":A#Method:I#R1303:S#Actual_Allocations = E#" & sEntity & ":A#Method:I#R1303:S#Actual", True)
						
					End If	
					
					If  sEntity = "R1303"  Then
						
						api.Data.Calculate("E#" & sEntity & ":A#PctCON:I#Horse_Group:S#Actual_Allocations = E#" & sEntity & ":A#PctCON:I#Horse_Group:S#Actual", True)
						api.Data.Calculate("E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#Actual_Allocations = E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#Actual", True)
						api.Data.Calculate("E#" & sEntity & ":A#Method:I#Horse_Group:S#Actual_Allocations = E#" & sEntity & ":A#Method:I#Horse_Group:S#Actual", True)
						
					End If	
						

			End If
			
		End Sub		
			
'____________________________________________________________________________________________________________________________________________________________
'____________________________________________________________________________________________________________________________________________________________

		Sub CopyDataWithBufferActToFcst_Brazil_RF04 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)				

		Dim sScenario As String = api.Pov.Scenario.Name	
		Dim sEntity As String = api.Pov.Entity.Name
		
			If  (sScenario = "RF04_Allocations") Then
		
				api.Data.ClearCalculatedData("E#" & sEntity & ":S#" & sScenario, True, True, True, True)
		
					If sEntity = "R1303001"  Then
						
						'Get source data buffer and declare destination info
						Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(E#R1303:C#EUR:S#RF04:P#Horse_Group:V#YTD)")
						Dim expDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
						Dim targetDataBuffer As New DataBuffer
						
						' Recuperamos ids de monedas EUR y BRL
						Dim EURId As Integer = Currency.EUR.Id
						Dim BRLId As Integer = Currency.BRL.Id
						
						' Definimos variables necesarias para el conseguir el tipo de cambio
						Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
						Dim cubeId As Integer = api.Pov.Cube.CubeId
						Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		
						' Obtenemos el tipo de cambio
						Dim conversionRate As Decimal = api.FxRates.GetCalculatedFxRate(rateType, cubeId, timeId, EURId, BRLId)
						
						For Each sourceCell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
							Dim targetCell As New DataBufferCell(sourceCell)
							targetCell.CellAmount = targetCell.CellAmount * conversionRate
							targetDataBuffer.SetCell(si, targetCell, True)
							
						Next
						'Set Data buffer
						api.Data.SetDataBuffer(targetDataBuffer, expDestInfo,,,,,,,,,,,,, True)
						'api.Data.Calculate("E#R1303001 = E#R1303:C#EUR:S#Actual:P#Horse_Group", True)
						
					End If		
						
		'-------------------------------------------------------------------------------------------------------------------------------			
					If sEntity = "R1303001" Then
						
						api.Data.Calculate("E#" & sEntity & ":A#PctCON:I#R1303:S#RF04_Allocations = E#" & sEntity & ":A#PctCON:I#R1303:S#RF04", True)
						api.Data.Calculate("E#" & sEntity & ":A#PctOWN:I#R1303:S#RF04_Allocations = E#" & sEntity & ":A#PctOWN:I#R1303:S#RF04", True)
						api.Data.Calculate("E#" & sEntity & ":A#Method:I#R1303:S#RF04_Allocations = E#" & sEntity & ":A#Method:I#R1303:S#RF04", True)
						
					End If	
					
					If  sEntity = "R1303"  Then
						
						api.Data.Calculate("E#" & sEntity & ":A#PctCON:I#Horse_Group:S#RF04_Allocations = E#" & sEntity & ":A#PctCON:I#Horse_Group:S#RF04", True)
						api.Data.Calculate("E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#RF04_Allocations = E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#RF04", True)
						api.Data.Calculate("E#" & sEntity & ":A#Method:I#Horse_Group:S#RF04_Allocations = E#" & sEntity & ":A#Method:I#Horse_Group:S#RF04", True)
						
					End If	
						

			End If
			
		End Sub	

'____________________________________________________________________________________________________________________________________________________________
'____________________________________________________________________________________________________________________________________________________________
			
		
		Sub CopyDataWithBufferActToFcst_R1300003_R1300004 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)				
			
		Dim sScenario As String = api.Pov.Scenario.Name	
		Dim sEntity As String = api.Pov.Entity.Name
		
			If (sScenario = "Actual_Allocations")  Then
		
				api.Data.ClearCalculatedData("E#" & sEntity & ":S#" & sScenario, True, True, True, True)
		
				If sEntity = "R1300003"  Then		
		
					api.Data.Calculate("E#R1300003 = E#R1300003:S#Actual + E#R1300004:S#Actual", True)
					
					api.Data.Calculate("E#R1300003:A#PctCON:I#R1300:S#Actual_Allocations = E#R1300003:A#PctCON:I#R1300:S#Actual", True)
					api.Data.Calculate("E#R1300003:A#PctOWN:I#R1300:S#Actual_Allocations = E#R1300003:A#PctOWN:I#R1300:S#Actual", True)
					api.Data.Calculate("E#R1300003:A#Method:I#R1300:S#Actual_Allocations = E#R1300003:A#Method:I#R1300:S#Actual", True)			

				End If	
				
				If  sEntity = "R1300"  Then
					
					api.Data.Calculate("E#" & sEntity & ":A#PctCON:I#Horse_Group:S#Actual_Allocations = E#" & sEntity & ":A#PctCON:I#Horse_Group:S#Actual", True)
					api.Data.Calculate("E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#Actual_Allocations = E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#Actual", True)
					api.Data.Calculate("E#" & sEntity & ":A#Method:I#Horse_Group:S#Actual_Allocations = E#" & sEntity & ":A#Method:I#Horse_Group:S#Actual", True)
					
				End If	
			
			End If	

		End Sub		
			
'____________________________________________________________________________________________________________________________________________________________
'____________________________________________________________________________________________________________________________________________________________
	
		Sub CopyDataWithBufferActToFcst_R1300003_R1300004_RF04 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)				
			
		Dim sScenario As String = api.Pov.Scenario.Name	
		Dim sEntity As String = api.Pov.Entity.Name
		
			If (sScenario = "RF04_Allocations")  Then
		
				api.Data.ClearCalculatedData("E#" & sEntity & ":S#" & sScenario, True, True, True, True)
		
				If sEntity = "R1300003"  Then		
		
					api.Data.Calculate("E#R1300003 = E#R1300003:S#RF04 + E#R1300004:S#RF04", True)
					
					api.Data.Calculate("E#R1300003:A#PctCON:I#R1300:S#RF04_Allocations = E#R1300003:A#PctCON:I#R1300:S#RF04", True)
					api.Data.Calculate("E#R1300003:A#PctOWN:I#R1300:S#RF04_Allocations = E#R1300003:A#PctOWN:I#R1300:S#RF04", True)
					api.Data.Calculate("E#R1300003:A#Method:I#R1300:S#RF04_Allocations = E#R1300003:A#Method:I#R1300:S#RF04", True)			

				End If	
				
				If  sEntity = "R1300"  Then
					
					api.Data.Calculate("E#" & sEntity & ":A#PctCON:I#Horse_Group:S#RF04_Allocations = E#" & sEntity & ":A#PctCON:I#Horse_Group:S#RF04", True)
					api.Data.Calculate("E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#RF04_Allocations = E#" & sEntity & ":A#PctOWN:I#Horse_Group:S#RF04", True)
					api.Data.Calculate("E#" & sEntity & ":A#Method:I#Horse_Group:S#RF04_Allocations = E#" & sEntity & ":A#Method:I#Horse_Group:S#RF04", True)
					
				End If	
			
			End If	

		End Sub		
			
'____________________________________________________________________________________________________________________________________________________________
'____________________________________________________________________________________________________________________________________________________________


		Sub CopyDataForecast (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		

			Dim sFormulaSource As String = "S#Forecast_Feb"
			Dim sFormulaTarget As String = "S#RF02A"
		
			Dim results As New DataBuffer	
			Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula(sFormulaSource)
	
			For Each db1Cell As DataBufferCell In dataToCopy.DataBufferCells.Values
				Dim resultCell As New DataBufferCell(db1Cell)				
				results.SetCell(si, resultCell, False)			

			Next
	
			Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo(sFormulaTarget)
			api.Data.SetDataBuffer(results, exprDestInfo,,,,,,,,,,,,,True)
	
		End Sub				
		
		Sub CopyDataWithBufferF00IToF99I (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		

			Dim sFormulaSource As String = "S#Budget_V1:T#2024M1:F#F00I"
			Dim sFormulaTarget As String = "S#Budget_V1:T#2023M12:F#F99I"
		
			Dim results As New DataBuffer	
			Dim dataToCopy As DataBuffer = api.Data.GetDataBufferUsingFormula(sFormulaSource)			

			For Each db1Cell As DataBufferCell In dataToCopy.DataBufferCells.Values
				Dim resultCell As New DataBufferCell(db1Cell)				
				results.SetCell(si, resultCell, False)			

			Next
	
			Dim exprDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo(sFormulaTarget)
			api.Data.SetDataBuffer(results, exprDestInfo,,,,,,,,,,,,,True)
	
		End Sub			

		Sub CompleteWorkflow(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)
			
			Try											
					
				'Initialize method level variables
				Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
				Dim noUpdateMsg As New Text.StringBuilder
				Dim noUpdateCount As Integer = 0

				'Check the Workflow status of the parent (We can't calculate plan if the parent is certified)
				Dim wfRegParent As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetParent(si, si.WorkflowClusterPk)
				Dim wfRegParentPk As New WorkflowUnitClusterPk(wfRegParent.ProfileKey, si.WorkflowClusterPk.ScenarioKey, si.WorkflowClusterPk.TimeKey)
				Dim wfRegParentStatus As WorkflowInfo = BRApi.Workflow.Status.GetWorkflowStatus(si, wfRegParentPk, False)												
				
				If Not wfRegParentStatus.AllTasksCompleted Then															

					Dim curProfile As WorkflowProfileInfo = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey)
						
					'Update workflow to COMPLETED
					Dim wfClusterDesc As String = BRApi.Workflow.General.GetWorkflowUnitClusterPkDescription(si, si.WorkflowClusterPk)
					
					Dim sWFPlanning As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name & ".Planning"	
					Dim sScenario As String = ScenarioDimHelper.GetNameFromID(si, si.WorkflowClusterPk.ScenarioKey)
					Dim sTime As String = TimeDimHelper.GetNameFromId(si.WorkflowClusterPk.TimeKey)
					Dim wfPlannning As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si, sWFPlanning, sScenario, sTime)
					BRApi.Workflow.Status.SetWorkflowStatus(si, wfPlannning, StepClassificationTypes.Workspace, WorkflowStatusTypes.Completed, StringHelper.FormatMessage(Me.m_MsgWorkflowCompleted, wfClusterDesc), "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)											
					'BRApi.Workflow.Status.SetWorkflowStatus(si, wfPlannning, StepClassificationTypes.Workspace, WorkflowStatusTypes.NotExecuted, StringHelper.FormatMessage(Me.m_MsgWorkflowCompleted, wfClusterDesc), "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)											
					BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.ProcessCube, WorkflowStatusTypes.Completed, StringHelper.FormatMessage(Me.m_MsgWorkflowCompleted, wfClusterDesc), "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)																					
					BRApi.DataQuality.Process.ExecuteProfileCertification(si,si.WorkflowClusterPk,CertSignOffStates.ProfileCertified,"Prueba1",True,True)
					'BRApi.DataQuality.Process.ExecuteQuestionnaireSignOff(si,si.WorkflowClusterPk,Guid.Empty,CertSignOffStates.ProfileCertified,"Prueba2")
					'BRApi.Workflow.Status.SetWorkflowStatus(si, si.WorkflowClusterPk, StepClassificationTypes.Certify, WorkflowStatusTypes.Completed, "Certify", "", Me.m_MsgWorkflowCompletedReasonButton, Guid.Empty)																					
									
				End If					
							
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try		
			
		End Sub		
	
		Function GetMembers (ByVal si As SessionInfo, ByVal sMemberSource As String, ByVal sTag As String, ByVal sDimension As String, ByVal sMember_NoAllocation_Filter As String) As List(Of MemberInfo)
			
			Dim MemberList As List(Of MemberInfo) = New List(Of MemberInfo)
			Dim MemberList_NoAllocation As List(Of MemberInfo) = New List(Of MemberInfo)
			
			If (sMember_NoAllocation_Filter <> String.Empty)
				BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sMember_NoAllocation_Filter, True)
			End If
			
			For Each sMember As String In sMemberSource.Split(",")
				For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, sDimension, sTag & sMember.Trim & ".Base", True)
					'Brapi.ErrorLog.LogMessage(si, "Member: " & miMember.Member.Name)
					
					If Not MemberList.Contains(miMember) And Not MemberList_NoAllocation.Contains(miMember)
						MemberList.Add(miMember)
					End If
				Next
			Next						
			
			Return MemberList
			
		End Function		
		
		Function GetFilter (ByVal MemberInfoList As List(Of MemberInfo)) As Dictionary(Of Integer, Object)
			
				Dim filters As New Dictionary(Of Integer, Object)
				For Each MemberI As MemberInfo In MemberInfoList
					filters.Add(MemberI.Member.MemberId, MemberI.Member.Name)
				Next
			
			Return filters
			
		End Function			
	
		'String Messages				
		Private m_MsgWorkflowCompleted As String = "Plan Workflow Completed: {0}"
		Private m_MsgCannotCompleteWorkflow As String = "Workflow NOT Completed: Parent Workflow has been Completed."
		Private m_MsgWorkflowCompletedReasonButton As String = "User clicked [Complete Workflow]"
		Private m_MsgCannotRevertWorkflow As String = "Workflow NOT Reverted: Parent Workflow has been Completed."
		Private m_MsgWorkflowReverted As String = "Capital Plan Workflow Reverted: {0}"
		Private m_MsgWorkflowRevertedReasonButton As String = "User clicked [Revert Workflow]"	

	End Class
	
End Namespace