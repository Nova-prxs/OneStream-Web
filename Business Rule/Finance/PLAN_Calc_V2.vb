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

Namespace OneStream.BusinessRule.Finance.PLAN_Calc
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					Case Is = FinanceFunctionType.CustomCalculate
					Dim Central_rule As New OneStream.BusinessRule.Finance.Central_rule.MainClass
					Dim FilterTwoStrings As List(Of KeyValuePair(Of String, String)) = New List(Of KeyValuePair(Of String, String))
					Dim IncreasePercentage As Decimal = 0	
					Dim UD8NameDatacell As String = ""
					Dim UD8NameSource As String = ""
					Dim UD8NameDestination As String = ""
					Dim intFlowid As Integer = 0
					Dim Filter As String = ""
					Dim SourcePOV As String = ""
					Dim DestinationPOV As String = ""
					Dim Source As String = ""
					Dim Destination As String = ""
					Dim Formula As String = ""
					
					'Get Workflow Scenario
					Dim WFScenario As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
					
					'Get Workflow Year
					Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
					Dim wfYear As Integer = BRApi.Finance.Time.GetYearFromId(si, myWorkflowUnitPk.TimeKey)
					Dim wfYearNext1 As Integer = BRApi.Finance.Time.GetYearFromId(si, wfyear +1)
					Dim wfYearNext2 As Integer = BRApi.Finance.Time.GetYearFromId(si, wfyear +2)
					Dim wfYearNext3 As Integer = BRApi.Finance.Time.GetYearFromId(si, wfyear +3)
					Dim wfYearNext4 As Integer = BRApi.Finance.Time.GetYearFromId(si, wfyear +4)
					Dim wfYearNext5 As Integer = BRApi.Finance.Time.GetYearFromId(si, wfyear +5)
					Dim strUD8name As String = ""
					Dim accountFilter As String = ""
					Dim Lstofdrivers As New List(Of String)
					'Step 1: LFL cohorts increase
					'Increase applies to: PRICES, VOLUMES, UNIT PRICES
					'Medical Salaries and SGA lines recalculated based on the new Revenue (after increased prices and volumes) as a % of them.
					Dim UD1LFLCentersnonHQ As String = "U1#Root.Base.where(text1 <> '"& wfYear &"' and text1 <> '"& wfYearNext1 &"' and text1 <> '"& wfYearNext2 &"' and text1 <> '"& wfYearNext3 &"' and text1 <> '"& wfYearNext4 &"' and text1 <> '"& wfYearNext5 &"' and text5 <> 'HQ')"
					Dim sourceDataBuffer As DataBuffer = Nothing
					Dim DestinationInfo As ExpressionDestinationInfo = Nothing
					Dim copayment As Decimal = 0 
					
					If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BUDv1_Country_Increase") Then
					
#Region "BUDv1 Country increase"

						'Calc for the Price for the OP File, because there is not Price included
'						Source = "Divide(F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan, -F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Budgetv1_Load)"
'						Destination ="F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Price_Budgetv1_Load"
'						Formula = $"{Destination}=RemoveZeros({source})"
'						api.Data.Calculate(Formula,isDurableCalculatedData:=True)

						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#8_1_1_1_1_1_1.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Volume", "A#8_1_1_1_1_1_1.Base, A#8_1_1_1_1_1_2_1_1.Base, A#8_1_1_1_1_1_2_1_2.Base, A#8_1_1_1_1_1_2_1_4, A#8_1_1_1_1_1_2_1_5.Base, A#8_1_1_1_1_1_2_1_6.Base, A#8_1_1_1_1_1_2_1_7.Base, A#203000"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost", "A#8_1_1_1_1_1_2_1_1.Base, A#8_1_1_1_1_1_2_1_2.Base, A#8_1_1_1_1_1_2_1_4, A#8_1_1_1_1_1_2_1_5.Base, A#8_1_1_1_1_1_2_1_6.Base, A#8_1_1_1_1_1_2_1_7.Base, A#203000"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							UD8NameDatacell = $"{item.key}_Budgetv1_Load"
							UD8NameSource = $"{item.key}_Budgetv1_Load"
							UD8NameDestination = $"{item.key}_Src"
							accountFilter = item.Value
							IncreasePercentage = 1+api.Data.GetDataCell($"T#{api.Time.GetYearFromId}M1:V#Periodic:A#Driver:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
							Source = $"V#Periodic:O#Import:F#Top:UD5#Top:UD7#Top:UD8#{UD8NameSource}"
							Destination = $"V#Periodic:O#Import:F#None:UD5#None:UD7#None:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(Destination, True, False, False, True)
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									cell.CellAmount = (cell.CellAmount * IncreasePercentage)
									If cell.CellAmount <> 0
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, ud1Filter:=UD1LFLCentersnonHQ, isDurableCalculatedData:=True)
							End If	
						Next
						
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("COS", "A#203010, A#8_1_1_1_1_1_2_1_8.Base, A#8_1_1_1_1_1_2_2.Base, A#8_1_1_1_1_1_2_3.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Medical_Salaries", "A#8_1_1_1_1_2.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("SGA", "A#8_1_1_1_1_3.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("EBITDA", "A#8_1_1_2.Base"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							UD8NameDatacell = item.Key
							UD8NameSource = item.Key
							UD8NameDestination = $"Pre_IFRS16_Plan"
							AccountFilter = item.Value
							IncreasePercentage = 1+api.Data.GetDataCell($"T#{api.Time.GetYearFromId}M1:V#Periodic:A#Driver:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
							Source = $"V#Periodic:O#Import:F#Top:UD5#Top:UD7#Top:UD8#{UD8NameSource}"
							Destination = $"V#Periodic:O#Import:F#None:UD5#None:UD7#None:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(Destination, True, False, False, True)
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									cell.CellAmount = (cell.CellAmount * IncreasePercentage)
									If cell.CellAmount <> 0
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, ud1Filter:=UD1LFLCentersnonHQ, isDurableCalculatedData:=True)
							End If	
						Next
#End Region
					
					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Price_Volume_UnitC_Calc") Then
						brapi.ErrorLog.LogMessage(si,"test")
#Region "BUD QFC: Move price and volume the correct flow, ud5 and ud7"
					FilterTwoStrings.Clear
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Volume", "A#8_1_1_1_1_1_1.Base"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#8_1_1_1_1_1_1.Base"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost", "A#8_1_1_1_1_1_2_1_1.Base, A#8_1_1_1_1_1_2_1_2.Base, A#8_1_1_1_1_1_2_1_4, A#8_1_1_1_1_1_2_1_5.Base, A#8_1_1_1_1_1_2_1_6.Base, A#8_1_1_1_1_1_2_1_7.Base, A#203000"))
					
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
						api.Data.ClearCalculatedData(Destination, True, False, False, True)
						sourceDataBuffer = Api.Data.GetDataBufferUsingFormula(Source)
						DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
						If Not sourceDataBuffer Is Nothing Then
							Dim resultDataBuffer As DataBuffer = New DataBuffer()
							For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
								intFlowid = api.Members.GetMemberId(dimtype.Flow.Id, api.UD1.Text(cell.DataBufferCellPk.UD1Id,1))
								If intFlowid <> -1
									Cell.DataBufferCellPk.FlowId = intFlowid
								End If 
								Cell.DataBufferCellPk.UD5Id = api.UD1.GetDefaultUDMemberId(cell.DataBufferCellPk.UD1Id, dimtype.UD5.Id)
								Cell.DataBufferCellPk.UD7Id = api.UD1.GetDefaultUDMemberId(cell.DataBufferCellPk.UD1Id, dimtype.UD7.Id)
								If cell.CellAmount <> 0
									resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
								End If
							Next
							api.Data.SetDataBuffer(resultDataBuffer, DestinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
						End If	
					Next
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Revenue_UnitCost_Calc") Then
						
#Region "Revenue and Unit cost Recalculation"
						
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#8_1_1_1_1_1_1.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost", "A#8_1_1_1_1_1_2_1_1.Base, A#8_1_1_1_1_1_2_1_2.Base, A#8_1_1_1_1_1_2_1_4, A#8_1_1_1_1_1_2_1_5.Base, A#8_1_1_1_1_1_2_1_6.Base, A#8_1_1_1_1_1_2_1_7.Base, A#203000"))
					
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							Source = $"V#Periodic:U8#{item.key}:O#Top * V#Periodic:U8#Volume:O#Top"
							Destination = "V#Periodic:U8#Pre_IFRS16_Plan:O#Import"
							accountFilter = item.Value
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud1Filter:= UD1LFLCentersnonHQ, ud8Filter:="U8#Pre_IFRS16_Plan")
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										Copayment = 1 + api.Data.GetDataCell($"V#Periodic:{Central_rule.BufName(SI, API, CELL, "A")}:F#Top:O#Top:I#Top:{Central_rule.BufName(SI, API, CELL, "U1")}:{Central_rule.BufName(SI, API, CELL, "U2")}:{Central_rule.BufName(SI, API, CELL, "U3")}:U4#Top:U5#Top:U6#Top:U7#Top:U8#Copayment").CellAmount					
										cell.CellAmount = cell.CellAmount * Copayment
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter)
							End If
						Next 
						
						
						'voluntary phi : (1-copay%) * Price * Volume
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#122000"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							Source = $"V#Periodic:U8#{item.key}:O#Top * V#Periodic:U8#Volume:O#Top"
							Destination = "V#Periodic:U8#Pre_IFRS16_Plan:O#Import"
							accountFilter = item.Value
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud1Filter:= UD1LFLCentersnonHQ, ud8Filter:="U8#Pre_IFRS16_Plan")
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										Copayment = 1 - api.Data.GetDataCell($"V#Periodic:{Central_rule.BufName(SI, API, CELL, "A")}:F#Top:O#Top:I#Top:{Central_rule.BufName(SI, API, CELL, "U1")}:{Central_rule.BufName(SI, API, CELL, "U2")}:{Central_rule.BufName(SI, API, CELL, "U3")}:U4#Top:U5#Top:U6#Top:U7#Top:U8#Copayment").CellAmount					
										cell.CellAmount = cell.CellAmount * Copayment
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter)
							End If
						Next 
						
						'Co-payment for PHI/ Quota/ Non-quota NHS : copay% * Price * Volume
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#123000, A#100000, A#8_1_1_1_1_1_1_1_7.base"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							Source = $"V#Periodic:U8#{item.key}:O#Top * V#Periodic:U8#Volume:O#Top"
							Destination = "V#Periodic:U8#Pre_IFRS16_Plan:O#Import"
							accountFilter = item.Value
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud1Filter:= UD1LFLCentersnonHQ, ud8Filter:="U8#Pre_IFRS16_Plan")
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										Copayment =  api.Data.GetDataCell($"V#Periodic:{Central_rule.BufName(SI, API, CELL, "A")}:F#Top:O#Top:I#Top:{Central_rule.BufName(SI, API, CELL, "U1")}:{Central_rule.BufName(SI, API, CELL, "U2")}:{Central_rule.BufName(SI, API, CELL, "U3")}:U4#Top:U5#Top:U6#Top:U7#Top:U8#Copayment").CellAmount					
										cell.CellAmount = cell.CellAmount * Copayment
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter)
							End If
						Next 
						
#End Region
							

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Revenue_UnitCost_Calc") Then
						
#Region "COGS ..."
						
							'Define list of all UD1 base members
							'POV for COGS component: CO2
							Dim UD2DimPK As DimPk = api.Pov.UD2dim.DimPk
							Dim UD2List As List(Of member) = api.Members.GetBaseMembers(UD2DimPK,"Products")
							Dim Filter As String = "U3#NoChnl:U5#None"
							Dim SourcePOV As String = "C#Local:V#Periodic:F#None:O#BeforeADJ:U1#None:U4#None:U6#None:U7#None"
							Dim DestPOV As String = "V#Periodic:F#None:O#Import:U1#None:U4#None:U6#None:U7#None"
							Dim Price As Decimal = 1 + api.Data.GetDataCell("A#Perc_Waste_OWB:I#None:U2#None:U3#NoChnl:U5#None:U8#None:" & SourcePOV).CellAmount.XFToStringForFormula
'							Dim Destination As String ""
							For Each UD2Member In UD2List
								Dim UD2text1 As String =  api.UD2.Text(UD2Member.MemberId, 1)
								Dim UD2text2 As String =  api.UD2.Text(UD2Member.MemberId, 2)
								Dim UD2text5 As String =  api.UD2.Text(UD2Member.MemberId, 5)
								Dim UD2text6 As String =  api.UD2.Text(UD2Member.MemberId, 6)
								Dim Variable1 As String =  ""
								Dim Variable2 As String =  ""
								Dim Variable3 As String =  ""
								Dim Variable4 As String =  ""
								If UD2text2.XFEqualsIgnoreCase("Owb") And isnumeric(UD2text6)
									Variable1 = $"A#VolumePC_P:F#None:O#BeforeAdj:I#Top:U1#None:U2#{UD2Member.Name}:U4#None:U6#None:U7#None:U8#PC_Top"
									Variable2 = UD2text6
									Variable3 = Price
									Variable4 = $"A#Price_Owb:F#None:O#BeforeAdj:I#Top:U1#None:U2#None:U4#{UD2text5}:U6#{UD2text1}:U7#{UD2text2}:U8#None"
									Destination = $"A#Owb_P:F#None:O#Import:I#None:U1#None:U2#{UD2Member.Name}:U4#None:U6#None:U7#None:U8#None"
									If Variable2 <> 0 And 
									Variable3 <> 0 And
									api.Members.GetMemberId(dimtype.UD4.Id, UD2text5) <> -1 And
									api.Members.GetMemberId(dimtype.UD6.Id, UD2text1) <> -1 And
									api.Members.GetMemberId(dimtype.UD7.Id, UD2text2) <> -1
									Source = $"MultiplyUnbalanced({Variable1}, {Variable4}:{Filter}, Filter)*{Variable2}*{Variable3}"
									Api.Data.Calculate($"{Destination}={Source}")
									End If
								End If 
							Next



#End Region





					End If
				End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace