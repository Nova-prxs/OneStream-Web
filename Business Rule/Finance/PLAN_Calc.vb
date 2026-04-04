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
					Dim UD7NameSource As String = ""
					Dim UD8NameDestination As String = ""
					Dim intFlowid As Integer = 0
					Dim Filter As String = ""
					Dim SourcePOV As String = ""
					Dim DestinationPOV As String = ""
					Dim Source As String = ""
					Dim Destination As String = ""
					Dim Formula As String = ""
					
					
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
			
					
					If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Budv1_Price_Calc") Then
#Region "Budv1 Price Calc"
						
						If (api.Pov.Time.Name.Contains("2024") Or api.Pov.Time.Name.Contains("2022")) AndAlso api.Pov.Scenario.Name.Contains("Budget") 'Calculate prices upon loading the OP24. Other budgetv1 scenarios pull prices from Q_FCST
''						Calc For the Price For the OP File, because there Is Not Price included
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Price_Budgetv1_Load"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						':F#Top:O#Top:I#None:U1#DA02:U2#03:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan
						':F#Top:O#Top:I#None:U1#DA02:U2#03:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Budgetv1_Load
						':F#Top:O#Top:I#None:U1#DA02:U2#03:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Price_Budgetv1_Load
						Source = "Divide(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Budgetv1_Load, -V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Budgetv1_Load)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#8_1_1_1_1_1_1.Base", isDurableCalculatedData:=True)  'BERO added Revenue account filter to not calculate prices for cost accounts

						'BERO ADDED 03May for calculation of unit_cost pulling total volume (no volume per cost account available)
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Unit_Cost_Budgetv1_Load"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "DivideUnbalanced(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Budgetv1_Load, A#8_1_1_1_1_1_1:V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Budgetv1_Load, A#8_1_1_1_1_1_1)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005", isDurableCalculatedData:=True)  'BERO added cost account filter to not calculate unit cost for rev accounts
						
						End If
						
						
#End Region			

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("OP24_TRANSFER_RPT") Then

#Region "OP24 Transfer to final UD8s" 
						'BERO 17.05.2024 created to transfer all OP24 data from source budgetv1_load ud8s into the final ud8s used for further seeding into Q_FCST_X_Y AND REPORTING
						
						If (api.Pov.Time.Name.Contains("2024") Or api.Pov.Time.Name.Contains("2022")) AndAlso api.Pov.Scenario.Name.Contains("Budget") 'Calculate prices upon loading the OP24. Other budgetv1 scenarios pull prices from Q_FCST
														
							'Clear existing data 
							Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#8.base, A#Non_Financial.base", Ud8Filter:="U8#IFRS16_Plan, U8#Pre_IFRS16_Plan, U8#Price, U8#Volume, U8#Unit_Cost")


							'Run seeding calculation
							'Bero updates 160724 to keep ud7 and ud5 detail
'								api.Data.Calculate("O#Import:F#None:U4#None:U5#None:U7#None:U8#Pre_IFRS16_Plan = RemoveZeros(O#Top:F#Top:U4#None:U5#Top:U7#Top:U8#Pre_IFRS16_Plan_Budgetv1_Load)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
								api.Data.Calculate("O#Import:F#None:U4#None:U8#Pre_IFRS16_Plan = RemoveZeros(O#Top:F#Top:U4#None:U8#Pre_IFRS16_Plan_Budgetv1_Load)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
'								api.Data.Calculate("O#Import:F#None:U4#None:U5#None:U7#None:U8#Price = RemoveZeros(O#Top:F#Top:U4#None:U5#Top:U7#Top:U8#Price_Budgetv1_Load)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
								api.Data.Calculate("O#Import:F#None:U4#None:U8#Price = RemoveZeros(O#Top:F#Top:U4#None:U8#Price_Budgetv1_Load)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
								
								api.Data.Calculate("O#Import:F#None:I#None:U4#None:U8#Volume = RemoveZeros(O#Top:F#Top:I#None:U4#None:U8#Volume_Budgetv1_Load)", AccountFilter:="A#8.base, A#Non_Financial.Base",isDurableCalculatedData:=True)
'								api.Data.Calculate("O#Import:F#None:U4#None:U5#None:U7#None:U8#Unit_Cost = RemoveZeros(O#Top:F#Top:U4#None:U5#Top:U7#Top:U8#Unit_Cost_Budgetv1_Load)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
								api.Data.Calculate("O#Import:F#None:U4#None:U8#Unit_Cost = RemoveZeros(O#Top:F#Top:U4#None:U8#Unit_Cost_Budgetv1_Load)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
							
							
						End If
						
						
#End Region			

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BUDv1_Country_Increase") Then
					
#Region "BUDv1 Country increase"


						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#8_1_1_1_1_1_1.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Volume", "A#8_1_1_1_1_1_1.Base, A#8_1_1_1_1_1_2_1_1.Base, A#8_1_1_1_1_1_2_1_2.Base, A#8_1_1_1_1_1_2_1_4, A#8_1_1_1_1_1_2_1_5.Base, A#8_1_1_1_1_1_2_1_6.Base, A#8_1_1_1_1_1_2_1_7.Base, A#203000"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost", "A#8_1_1_1_1_1_2_1_1.Base, A#8_1_1_1_1_1_2_1_2.Base, A#8_1_1_1_1_1_2_1_4.base, A#8_1_1_1_1_1_2_1_5.Base, A#8_1_1_1_1_1_2_1_6.Base, A#8_1_1_1_1_1_2_1_7.Base, A#203000"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							UD8NameDatacell = $"{item.key}_Budgetv1_Load"
							UD8NameSource = $"{item.key}_Budgetv1_Load"
							UD8NameDestination = $"{item.key}_Src"
							accountFilter = item.Value
							IncreasePercentage = api.Data.GetDataCell($"T#{api.Time.GetYearFromId}M1:V#Periodic:A#Driver:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
							Source = $"V#Periodic:O#Import:F#Top:UD5#Top:UD7#Top:UD8#{UD8NameSource}"
							Destination = $"V#Periodic:O#Import:F#None:UD5#None:UD7#None:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(Destination, True, False, False, True)
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									Dim MedCenterYear As String = api.UD1.Text(cell.DataBufferCellPk.UD1Id,1)
									If  MedcenterYear <> Nothing AndAlso CInt(MedCenterYear) < wfYear 'LFL centers get % increase
											If  IncreasePercentage = Nothing Then
												cell.CellAmount = cell.CellAmount
											Else
												cell.CellAmount = (cell.CellAmount * ( 1+IncreasePercentage))
											End If
											If cell.CellAmount <> 0
												resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
											End If
									Else 'newer centers keep baseline value
										cell.CellAmount = cell.CellAmount 
										If cell.CellAmount <> 0
											resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
										End If
									End If	
								Next
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
							End If
						Next
						
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("COS", "A#203010, A#8_1_1_1_1_1_2_1_8.Base, A#8_1_1_1_1_1_2_2.Base, A#8_1_1_1_1_1_2_3.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Medical_Salaries", "A#8_1_1_1_1_2.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("SGA", "A#8_1_1_1_1_4.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("EBITDA", "A#8_1_1_2.Base"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("DFC", "A#210000, A#210300, A#323000, A#302000, A#302200, A#302400, A#302500, A#302600, A#302800"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							UD8NameDatacell = $"{item.key}"
'							UD8NameSource = item.Key
							UD8NameSource = "Pre_IFRS16_Plan_Budgetv1_load"
'							UD8NameDestination = $"Pre_IFRS16_Plan"
							UD8NameDestination = $"Pre_IFRS16_Plan_Src"
							AccountFilter = item.Value
							IncreasePercentage = api.Data.GetDataCell($"T#{api.Time.GetYearFromId}M1:V#Periodic:A#Driver:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
							Source = $"V#Periodic:O#Import:F#Top:UD5#Top:UD7#Top:UD8#{UD8NameSource}"
							Destination = $"V#Periodic:O#Import:F#None:UD5#None:UD7#None:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=AccountFilter,Ud8Filter:="Pre_IFRS16_Plan_Src")
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									Dim MedCenterYear As String = api.UD1.Text(cell.DataBufferCellPk.UD1Id,1)
									If  MedcenterYear <> Nothing AndAlso CInt(MedCenterYear) < wfYear 'LFL centers get % increase
												If  IncreasePercentage = Nothing Then 
													cell.CellAmount = cell.CellAmount
												Else 
													cell.CellAmount = (cell.CellAmount * ( 1+IncreasePercentage))
												End If
											If cell.CellAmount <> 0
												resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
											End If
									Else 'newer centers keep baseline value
										cell.CellAmount = cell.CellAmount 
										If cell.CellAmount <> 0
											resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
										End If
									End If	
								Next
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
							End If	
						Next
						
						
						'BERO added 15.05.2024 section to calculate revenue_src including % increase to prices and volumes.
						'PRICE_SRC * VOLUME_SRC --> ONLY INCLUDING ACCOUNTS WITH CALCULATED REVENUE WITHIN THE BUDGET SCENARIO. This is to not mess with those that use manual input
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price_Src", "A#111000,A#122000,A#100000,A#8_1_1_1_1_1_1_2_1.Base,A#120000,A#125000,A#128000,A#101000,A#101001,A#101002,A#101003,A#101004,A#101005,A#102000,A#114000,A#105000,A#113000,A#103000, A#111001, A#111002, A#111003, A#111004, A#100003, A#100004, A#100005, A#100006, A#100007, A#100009, A#100010,A#112000, A#123000, A#104000"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							Source = $"(-V#Periodic:U8#{item.key}:O#Top * V#Periodic:U8#Volume_Src:O#Top)"
							Destination = "V#Periodic:U8#Pre_IFRS16_Plan_SRC:O#Import"
							accountFilter = item.Value
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud8Filter:="U8#Pre_IFRS16_Plan_Src")
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										cell.CellAmount = cell.CellAmount 
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If
						Next 
						
						'BERO added 17.05.2024 section to calculate cost including % increase to unit cost and volumes.
						'unit_cost_SRC * total VOLUME_SRC --> ONLY INCLUDING ACCOUNTS WITH CALCULATED COST WITHIN THE BUDGET SCENARIO. This is to not mess with those that use manual input
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost_Src", "A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							Source = $"multiplyunbalanced(V#Periodic:U8#{item.key}:O#Top , A#8_1_1_1_1_1_1:V#Periodic:U8#Volume_Src:O#Top, A#8_1_1_1_1_1_1 )"
							Destination = "V#Periodic:U8#Pre_IFRS16_Plan_SRC:O#Import"
							accountFilter = item.Value
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud8Filter:="U8#Pre_IFRS16_Plan_Src")
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										cell.CellAmount = cell.CellAmount 
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If
						Next 
					
						
						
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Plan_src_Two_Plan") Then

#Region "BUD QFC: "
						AccountFilter = "A#8.Base"
						api.Data.ClearCalculatedData( True, True, True, True, Accountfilter:=AccountFilter, UD8Filter:="U8#Pre_IFRS16_Plan")
						UD8NameSource = $"Pre_IFRS16_Plan_Src"
						UD8NameDestination = $"Pre_IFRS16_Plan"

						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
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
							api.Data.SetDataBuffer(resultDataBuffer, DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
						End If
#End Region
					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("QFCST_Price_Calc") Then
						
#Region "QFCST Price Calc"

''						Calc For the Price For the QFCST after seeding data from Actuals and Budgetv2
'						Budget periods from pre_ifrs16_plan_src, Actual periods under pre_ifrs16_plan, no _SRC members in actualized months.
If Not api.Pov.Scenario.Name.XFEqualsIgnoreCase("Q_FCST_9_3") And api.Time.GetPeriodNumFromId <= 9
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Price_Src"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "Divide(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Src, -V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Src)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#8_1_1_1_1_1_1.Base", isDurableCalculatedData:=True)  'BERO added Revenue account filter to not calculate prices for cost accounts

''						Calc For the Unit_Cost For the QFCST after seeding data from Actuals and Budgetv2
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Unit_Cost_Src"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "DivideUnbalanced(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Src, A#8_1_1_1_1_1_1:V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Src, A#8_1_1_1_1_1_1)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:="A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005", isDurableCalculatedData:=True)  
End If 
						
						
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("QFCST_Price_Calc_Act") Then
						
#Region "QFCST Price Calc"

''						Calc For the Price For the QFCST after seeding data from Actuals and Budgetv2
'						Budget periods from pre_ifrs16_plan_src, Actual periods under pre_ifrs16_plan, no _SRC members in actualized months.
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Price"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "Divide(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan, -V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#8_1_1_1_1_1_1.Base", isDurableCalculatedData:=True)  'BERO added Revenue account filter to not calculate prices for cost accounts

''						Calc For the Unit_Cost For the QFCST after seeding data from Actuals and Budgetv2
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Unit_Cost"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "DivideUnbalanced(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan, A#8_1_1_1_1_1_1:V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume, A#8_1_1_1_1_1_1)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005", isDurableCalculatedData:=True)  

						
						
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("5YP_Price_Calc") Then
						
#Region "5YP Price Calc"

''						Calc For the Price For the 5YP after seeding data from Budgetv2
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Price_Src"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "Divide(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Src, -V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Src)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#8_1_1_1_1_1_1.Base", isDurableCalculatedData:=True)  'calculate price only for Revenue accounts

''						Calc For the Unit_Cost For the 5YP after seeding data from Budgetv2
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Unit_Cost_Src"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "DivideUnbalanced(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Src, A#8_1_1_1_1_1_1:V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Src, A#8_1_1_1_1_1_1)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005", isDurableCalculatedData:=True)  

						
						
#End Region
					
					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Price_Volume_UnitC_Calc") Then

#Region "BUD QFC: Move price and volume the correct flow, ud5 and ud7"
'					If api.Time.GetYearFromId >= 2025
					FilterTwoStrings.Clear
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Volume", "A#8_1_1_1_1_1_1.Base"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#8_1_1_1_1_1_1.Base"))
					api.Data.ClearCalculatedData( True, True, True, True, Accountfilter:="A#8_1_1_1_1_1_1.Base" , UD8Filter:="U8#Volume, U8#Price")

					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value

						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
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
'					End If 
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Revenue_UnitCost_Calc") Then

#Region "Revenue Recalculation"
						
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#111000, A#122000, A#100000, A#122010, A#122011"))
						'BERO comment out 12.04.24 to only calculate revenue items
					
						'COPAYMENT ACCOUNTS
						'Accounts where copayment % is deducted from the output
'						api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#Pre_IFRS16_Plan")
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							accountFilter = item.Value
							If api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget") And api.Time.GetYearFromId = 2025
								Source = $"(-V#Periodic:U8#{item.key}:O#Top * V#Periodic:U8#Volume:O#Top)"
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan:O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud1Filter:= UD1LFLCentersnonHQ, ud8Filter:="U8#Pre_IFRS16_Plan")
							Else
								Source = $"(-V#Periodic:U8#{item.key}_Src:O#Top * V#Periodic:U8#Volume_Src:O#Top)"
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan_Src:O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud8Filter:="U8#Pre_IFRS16_Plan_Src")
							End If
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										If api.Entity.Text(6).XFEqualsIgnoreCase("Updated_Copayment")
											Copayment = 1
										Else
											Copayment = 1 - api.Data.GetDataCell($"V#Periodic:{Central_rule.BufName(SI, API, CELL, "A")}:F#Top:O#Top:I#Top:{Central_rule.BufName(SI, API, CELL, "U1")}:{Central_rule.BufName(SI, API, CELL, "U2")}:{Central_rule.BufName(SI, API, CELL, "U3")}:U4#Top:U5#Top:U6#Top:U7#Top:U8#Copayment").CellAmount					
										End If 
										cell.CellAmount = cell.CellAmount * Copayment
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If
						Next 
						
						
						'PRICE * VOLUME ACCOUNTS
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#8_1_1_1_1_1_1_2_1.Base, A#120000, A#125000, A#128000, A#101000, A#101001, A#101002, A#101003, A#101004, A#101005, A#102000, A#114000, A#105000, A#113000, A#103000, A#111001, A#111002, A#111003, A#111004, A#100003, A#100004, A#100005, A#100006, A#100007, A#100009, A#100010"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							accountFilter = item.Value
							If api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget") And api.Time.GetYearFromId = 2025
								Source = $"(-V#Periodic:U8#{item.key}:O#Top * V#Periodic:U8#Volume:O#Top)"
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan:O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud8Filter:="U8#Pre_IFRS16_Plan")
							Else
								Source = $"(-V#Periodic:U8#{item.key}_Src:O#Top * V#Periodic:U8#Volume_Src:O#Top)"
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan_Src:O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud8Filter:="U8#Pre_IFRS16_Plan_Src")
							End If
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										'Copayment =  api.Data.GetDataCell($"V#Periodic:{Central_rule.BufName(SI, API, CELL, "A")}:F#Top:O#Top:I#Top:{Central_rule.BufName(SI, API, CELL, "U1")}:{Central_rule.BufName(SI, API, CELL, "U2")}:{Central_rule.BufName(SI, API, CELL, "U3")}:U4#Top:U5#Top:U6#Top:U7#Top:U8#Copayment").CellAmount					
										cell.CellAmount = cell.CellAmount ' * Copayment
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If
						Next 
						
						
						'COPAYMENT ACCOUNTS
						'BERO 01.05.2024 update to copayment account logic to calculate it based on the drivers from other accounts
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("A#112000", "A#111000")) 'copayment, revenue acc
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("A#123000", "A#122000"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("A#104000", "A#100000"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							accountFilter = item.key
							If api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget") And api.Time.GetYearFromId = 2025
								Source = $"(-V#Periodic:U8#Price:" & item.value & ":O#Top * V#Periodic:U8#Volume:"& item.value &":O#Top)"
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan:" & item.key &":O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#Pre_IFRS16_Plan")
							Else
								Source = $"(-V#Periodic:U8#Price_Src:" & item.value & ":O#Top * V#Periodic:U8#Volume_Src:"& item.value &":O#Top)"
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan_Src:" & item.key &":O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#Pre_IFRS16_Plan_Src")
							End If
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										Copayment =  api.Data.GetDataCell($"V#Periodic:{item.Value}:F#Top:O#Top:I#Top:{Central_rule.BufName(SI, API, CELL, "U1")}:{Central_rule.BufName(SI, API, CELL, "U2")}:{Central_rule.BufName(SI, API, CELL, "U3")}:U4#Top:U5#Top:U6#Top:U7#Top:U8#Copayment").CellAmount					
										cell.CellAmount = cell.CellAmount * Copayment
										cell.DataBufferCellPk.AccountId = api.Members.GetMemberId(dimtype.Account.Id,item.key)
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If
						Next 
						
					
						
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("5YP_Price_Calc") Then
						
#Region "5YP Price Calc"

''						Calc For the Price For the 5YP after seeding data from Budgetv2
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Price_Src"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "Divide(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Src, -V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Src)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#8_1_1_1_1_1_1.Base", isDurableCalculatedData:=True)  'calculate price only for Revenue accounts

''						Calc For the Unit_Cost For the 5YP after seeding data from Budgetv2
						Destination ="V#Periodic:F#None:O#Import:I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Unit_Cost_Src"
						api.Data.ClearCalculatedData(Destination,True,True,True,True)
						Source = "DivideUnbalanced(V#Periodic:F#Top:O#Top:I#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Pre_IFRS16_Plan_Src, A#8_1_1_1_1_1_1:V#Periodic:F#Top:O#Import:I#None:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Volume_Src, A#8_1_1_1_1_1_1)"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,Accountfilter:= "A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005", isDurableCalculatedData:=True)  

						
						
#End Region
					
					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BudgetV2_Pre_IFRS16_Plan") Then

#Region "BUDV2 QFC: Move price and volume the correct flow, ud5 and ud7"
'brapi.ErrorLog.LogMessage(si, " BudgetV2_Pre_IFRS16_Plan Scenario " & api.Pov.Scenario.Name & " Time " & api.Time.GetYearFromId  )
					If api.Pov.Scenario.Name.XFEqualsIgnoreCase("Budgetv2") And api.Time.GetYearFromId = 2025 'Updated Andreas
'					If api.Time.GetYearFromId = 2025

'						U8#Pre_IFRS16_Plan, U8#Pre_IFRS16_Plan_Src
'						FilterTwoStrings.Clear
'						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("", "A#8_1_1_1_1_1_1.Base"))
''						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Price", "A#8_1_1_1_1_1_1.Base"))
'						'BERO comment out 12.04.24 to only move revenue items
'						'FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost", "A#8_1_1_1_1_1_2_1_1.Base, A#8_1_1_1_1_1_2_1_2.Base, A#8_1_1_1_1_1_2_1_4, A#8_1_1_1_1_1_2_1_5.Base, A#8_1_1_1_1_1_2_1_6.Base, A#8_1_1_1_1_1_2_1_7.Base, A#203000"))
'						'ONLY FOR BUDGETV2
'						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
'brapi.ErrorLog.LogMessage(si, " BudgetV2_Pre_IFRS16_Plan " )
							AccountFilter = "A#8_1_1_1_1_1_1.Base, A#8_1_1_1_1_1_2.Base"
							Source = $"V#Periodic:O#BeforeAdj:UD8#Pre_IFRS16_Plan_Src"
							Destination = $"V#Periodic:O#Import:UD8#Pre_IFRS16_Plan"
							Dim val As Decimal= 0
'							api.Data.ClearCalculatedData( True, True, True, True, Accountfilter:=AccountFilter , UD8Filter:="U8#" & UD8NameDestination)
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
										Dim PC_CellPK2 As New DataBufferCellPk(Cell.DataBufferCellPk) 
										val = api.Data.GetDataCell($"V#Periodic:{PC_CellPK2.GetMemberScript(api).replace("U8#XFCommon","U8#Pre_IFRS16_Plan").replace("O#XFCommon","O#Import")}").CellAmount
										If Val = 0 
'brapi.ErrorLog.LogMessage(si, "val " & val  & "script:  " & PC_CellPK2.GetMemberScript(api) )
'A#370500:F#1991:O#XFCommon:I#None:U1#NB11:U2#19:U3#HQ:U4#None:U5#Open_Greenfield:U6#None:U7#Headquarters_1:U8#XFCommon
									
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
										End If
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, DestinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If	
'						Next
					End If
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("UnitCost_Calc") Then
						
#Region "BUD QFC: Move unit cost to the correct flow, ud5 and ud7"
				
					'BERO 12.04.2024 Added UnitCost transfer to flow,ud5 and ud7 of the medical center
					FilterTwoStrings.Clear
					Dim CostAccounts As String = "A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005"
''''Added Andreas 01 October 2025
''''					If api.Time.GetYearFromId > 2025
''''						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan", CostAccounts))
''''					End If 
''''Added Andreas 01 October 2025
 					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost", CostAccounts))
'						api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud8Filter:="U8#Unit_Cost_Src")
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
						api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud8Filter:=$"U8#{UD8NameDestination}")
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

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Cost_Calc") Then
						
#Region "BUD QFC: Cost Calculation" 
					'BERO 12.04.2024 Added UnitCost by Account * Total Revenue calculation for Cost of sales planning
						FilterTwoStrings.Clear
						'List of applicable cost accounts
						Dim CostAccounts As String = "A#200000, A#201000, A#204000, A#205200, A#207200, A#203000, A#206000, A#205000, A#207000, A#207005"
						Dim sourceVolume As Decimal = Nothing
						If api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget") And api.Time.GetYearFromId = 2025
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost", CostAccounts))
						Else
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Unit_Cost_Src", CostAccounts))
						End If

						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							accountFilter = item.Value
							Source = $"V#Periodic:U8#{item.key}:O#Top"
							If api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget") And api.Time.GetYearFromId = 2025
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan:O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#Pre_IFRS16_Plan")
							Else
								Destination = "V#Periodic:U8#Pre_IFRS16_Plan_Src:O#Import"
								api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#Pre_IFRS16_Plan_Src")
							End If
							
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							destinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									If cell.CellAmount <> 0
										sourcevolume = api.Data.GetDataCell($"V#Periodic:A#8_1_1_1_1_1_1:F#Top:O#Top:I#Top:{Central_rule.BufName(SI, API, CELL, "U1")}:{Central_rule.BufName(SI, API, CELL, "U2")}:{Central_rule.BufName(SI, API, CELL, "U3")}:U4#Top:U5#Top:U6#Top:U7#Top:U8#VOLUME").CellAmount					
										cell.CellAmount = cell.CellAmount * sourcevolume
										If cell.CellAmount <> 0 
											resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
										End If 
									End If
								Next
								api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If
						Next 

#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Cost_Input_Detail") Then
						
#Region "BUD QFC: Move manual input cost to the correct flow, ud5 and ud7"
					'BERO 12.04.2024 Added COST transfer to flow,ud5 and ud7 of the medical center;
					'to move manual inputs Of non-calculated cost accounts Into their full dimensionality
					FilterTwoStrings.Clear
					Dim CostAccounts As String = "A#203010, A#210000, A#210300, A#323000, A#205030, A#205031, A#205032, A#205000_cos_counter, A#204000_cos_counter, A#205020"
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan",  CostAccounts))
										
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
						api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:=$"U8#{UD8NameDestination}")
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


					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Revenue_Input_Detail") Then
						
#Region "BUD QFC: Move manual input revenue to the correct flow, ud5 and ud7"
					'BERO 15.04.2024 Added REVENUE transfer to flow,ud5 and ud7 of the medical center;
					'to move manual inputs Of non-calculated revenue accounts Into their full dimensionality
					FilterTwoStrings.Clear
					Dim CostAccounts As String = "A#121000, A#108000, A#107000, A#106000, A#130000, A#110000, A#204000_rev_counter, A#205000_rev_counter"
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan",  CostAccounts))
										
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}" 
						api.Data.ClearCalculatedData(True, False, False, True,  accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:=$"U8#{UD8NameDestination}")
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
					
					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("MD_Input_Detail") Then
						
#Region "BUD QFC: Move manual input Medical Salaries to the correct flow, ud5 and ud7"
					'YE 22.04.2024 Added Medical Salaries transfer to flow,ud5 and ud7 of the medical center;
					'to move manual inputs Of non-calculated Medical Salaries accounts Into their full dimensionality
					FilterTwoStrings.Clear
					Dim MSAccounts As String = "A#8_1_1_1_1_2_1_1_1.Base, A#8_1_1_1_1_2_1_2_1.Base, A#8_1_1_1_1_3_1_3_1.Base"
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan", MSAccounts))
										
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:=$"U8#{UD8NameDestination}")
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

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DFC_Input_Detail") Then
						
#Region "BUD QFC: Move manual input Direct Fixed Costs to the correct flow, ud5 and ud7"
					'GY 26.06.2024 Added Direct Costs transfer to flow, ud5 and ud7 of the medical center;
					'to move manual inputs Of non-calculated Medical Salaries accounts Into their full dimensionality A#210010
					FilterTwoStrings.Clear 
					Dim DCAccounts As String = "A#210000, A#210300, A#323000, A#8_1_1_1_1_3_1_3_1.Base.Remove(302100,302300,302650,302700,302750,302490,302491,302492,302493,302494,302495),A#304000,A#304200,A#304400,A#304500,A#304600,A#304800"
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan",  DCAccounts))
										
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:=$"U8#{UD8NameDestination}")
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

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SGA_Input_Detail") Then
						
#Region "BUD QFC: Move manual input SG&A to the correct flow, ud5 and ud7"
					'YE 23.04.2024 Added SG&A transfer to flow,ud5 and ud7 of the medical center;
					'to move manual inputs Of non-calculated SG&A accounts Into their full dimensionality
					FilterTwoStrings.Clear
'					Dim SGAAccounts As String = "A#8_1_1_1_1_3_1_1_1.Base, A#8_1_1_1_1_4_1_3_2_1.base,  A#8_1_1_1_1_4_1_3_4.base, A#8_1_1_1_1_4_1_1_2.Base,A#306100,A#8_1_1_1_1_4_1_11.Base,A#8_1_1_1_1_4_1_2_4_1.Base,A#314600,A#311100,A#313200,A#314300,A#314400,A#314500,A#320000,A#311000,A#315000,A#8_1_1_1_1_4_1_2_4_2.Base,A#8_1_1_1_1_4_1_9_1.Base.Remove(Z33130),A#8_1_1_1_1_4_1_4.Base,A#8_1_1_1_1_4_1_6_1.Base,A#372000,A#372100,A#372400,A#372500,A#372600,A#8_1_1_1_1_4_1_5_1.Base,A#8_1_1_1_1_4_1_5_2.Base,A#8_1_1_1_1_4_1_5_3.Base,A#8_1_1_1_1_4_1_8_1.Base,A#8_1_1_1_1_4_1_10_1.Base.Remove(Z36130,123412),A#8_1_1_1_1_4_1_12_1.Base,A#321000"
				'Updated codes BeRo 21Jun24 to represent latest account codes...
					Dim SGAAccounts As String = "A#8_1_1_1_1_4_1_1_1.Base, A#8_1_1_1_1_4_1_3_2_1.base, A#8_1_1_1_1_4_1_3_4.base, A#8_1_1_1_1_4_1_1_2.Base,A#306100,A#8_1_1_1_1_4_1_11.Base,A#8_1_1_1_1_4_1_2_4_1.Base,A#314600,A#311100,A#313200,A#314300,A#314400,A#314500,A#320000,A#311000,A#315000,A#8_1_1_1_1_4_1_2_4_2.Base,A#8_1_1_1_1_4_1_9_1.Base.Remove(Z33130),A#8_1_1_1_1_4_1_4.Base,A#8_1_1_1_1_4_1_6_1.Base,A#372000,A#372100,A#372400,A#372500,A#372600,A#8_1_1_1_1_4_1_5_1.Base,A#8_1_1_1_1_4_1_5_2.Base,A#8_1_1_1_1_4_1_5_3.Base,A#8_1_1_1_1_4_1_8_1.Base,A#8_1_1_1_1_4_1_10_1.Base.Remove(Z36130,123412),A#8_1_1_1_1_4_1_12_1.Base,A#321000"
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan",  SGAAccounts))
										
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:=$"U8#{UD8NameDestination}")
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

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BTL_Input_Detail") Then
						
#Region "BUD QFC: Move manual input BTL to the correct flow, ud5 and ud7"
					'YE 24.04.2024 Added BTL transfer to flow,ud5 and ud7 of the medical center;
					'to move manual inputs Of non-calculated BTL accounts Into their full dimensionality
					'Bero 19Jul24 added A#400000 because it has values for op24 flowing into budv2 and qfcst, otherwise seeding values dont match
					FilterTwoStrings.Clear
					Dim BTLAccounts As String = "A#470700,A#470610,A#400000,A#410000,A#410100,A#410200,A#411000,A#412000,A#452000,A#452100,A#470050,A#470090,A#470100,A#470200,A#470300,A#470400,A#470500,A#470550,A#470600,A#470650,A#470800,A#471500,A#435000,A#440000,A#8_1_1_2_1_1_1_2_1.base,A#431100,A#430809,A#430100,A#430150,A#430800,A#420000,A#8_1_2_1_1_3_1_2_1.Base"
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan",  BTLAccounts))
										
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:=$"U8#{UD8NameDestination}")
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

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DEPR_Input_Detail") Then
						
#Region "BUD QFC: Move manual input DEPR to the correct flow, ud5 and ud7"
					'YE 24.04.2024 Added BTL transfer to flow,ud5 and ud7 of the medical center;
					'to move manual inputs Of non-calculated BTL accounts Into their full dimensionality
					FilterTwoStrings.Clear
					Dim DEPRAccounts As String = "A#PPACV400,A#PPACV403,A#400004"
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Pre_IFRS16_Plan",  DEPRAccounts))
										
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = $"{item.key}_Src"
						UD8NameDestination = Item.Key
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:=$"U8#{UD8NameDestination}")
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

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("IFRS16_Percentage_Capitalized") Then
						
#Region "IFRS16_Percentage_Calc"
						
						
						Dim Counter_Timefilter As String = brapi.Finance.Scenario.Text(Si, si.WorkflowClusterPk.ScenarioKey, 1)
						Dim Month As String = ""
						Dim InputMonth As String =""
						Dim ScenarioMember As String = BRApi.Finance.Members.GetMember(si, dimtypeid.Scenario, si.WorkflowClusterPk.ScenarioKey).Name
						Dim WYear As String = Left(BRApi.Finance.Members.GetMember(si, dimtypeid.Time, si.WorkflowClusterPk.TimeKey).Name,4).ToString			
						
						'Get month for %Capitalized based on scenario
						'19/09/2024: to facilitate the seeding of IFRS16, the Input month for Budgetv1,Budgetv2, Q_FCST scenarios changed to be M12
						If ScenarioMember.XFContainsIgnoreCase("Budget") Or ScenarioMember.XFContainsIgnoreCase("Q_FCST") Or ScenarioMember.XFContainsIgnoreCase("FC_")Then
							InputMonth= $"{api.Time.GetYearFromId}M12"
'						Else If ScenarioMember.XFContainsIgnoreCase("Q_FCST") Then
						
'							If Not Counter_Timefilter = ""
'								For i As Integer = 1 To 12 Step 1
'									Month = "M" & i.ToString & ","
'									If Not Counter_Timefilter.XFContainsIgnoreCase(Month)
'										If InputMonth = ""
'											InputMonth = $"{api.Time.GetYearFromId}M" & i.ToString 
'										End If 
'									End If 
'								Next
'							End If
						Else If ScenarioMember.XFContainsIgnoreCase("5YP") Then 
							InputMonth = WYear
						Else 
							InputMonth = $"{api.Time.GetYearFromId}M12"
						End If	
						
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Capitalized", "A#311000"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Capitalized", "A#320000"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						
							UD8NameDatacell = $"{item.key}"
							UD8NameSource = "Pre_IFRS16_Plan"
							UD8NameDestination = "IFRS16_Plan_Src"'"IFRS16_Plan"
							accountFilter = item.Value
							IncreasePercentage = api.Data.GetDataCell($"T#{InputMonth}:V#Periodic:{accountFilter}:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
'							IncreasePercentage = api.Data.GetDataCell($"T#{api.Time.GetYearFromId}M1:V#Periodic:{accountFilter}:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
							
							Source = $"V#Periodic:O#Import:F#Top:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD7#Top:UD8#{UD8NameSource}" 
'							Destination = $"V#Periodic:O#BeforeAdj:F#None:UD5#None:UD7#None:UD8#{UD8NameDestination}"
							Destination = $"V#Periodic:O#Import:F#None:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan_Src")
						
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							'sourceDataBuffer.LogDataBuffer(api, "srcdatabufeer" & api.Pov.Time.Name,100)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									'Calculate	
'									Dim MedCenterYear As String = api.UD1.Text(cell.DataBufferCellPk.UD1Id,1)
'									If  MedcenterYear <> Nothing 'AndAlso CInt(MedCenterYear) < wfYear 'LFL centers get % increase ??
									cell.CellAmount = -(cell.CellAmount * IncreasePercentage)
									If cell.CellAmount <> 0
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
'									Else 'newer centers keep baseline value
'										cell.CellAmount = -cell.CellAmount 
'										If cell.CellAmount <> 0
'											resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
'										End If
'									End If	
								Next
								' resultDataBuffer.LogDataBuffer(api, "resultDataBuffer" & api.Pov.Time.Name,100)
							
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
							End If	
						Next
						
						
						
						'Other interest expenses
						FilterTwoStrings.Clear
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Other interest expenses", "A#430800"))
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							UD8NameDatacell = "Capitalized"
							UD8NameSource = "Pre_IFRS16_Plan"
							UD8NameDestination = "IFRS16_Plan_Src"'"IFRS16_Plan"
							accountFilter = item.Value
							IncreasePercentage = api.Data.GetDataCell($"T#{InputMonth}:V#Periodic:{accountFilter}:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
							
							Source = $"V#Periodic:O#top:A#2_2_1_4:F#F_TotMov:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD7#Top:UD8#{UD8NameSource}" 'Lease liabilities : Closing balance prior period
							Destination = $"V#Periodic:O#Import:{accountFilter}:F#None:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan_Src")
						
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									'Calculate	
									cell.CellAmount = -(cell.CellAmount * IncreasePercentage)
									If cell.CellAmount <> 0
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
							
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
							End If	
						Next
						
						

'						Purchases Operating Leases (capex)  : RoU
						FilterTwoStrings.Clear
'						
						If ScenarioMember.XFContainsIgnoreCase("5YP") Then
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("None", "A#621000"))
						Else
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Capitalized_salary", "A#622000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Construction", "A#620000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("IT", "A#622000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("CTC_equipment", "A#621000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Diagnostic_equipment", "A#621000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Other_medical_equipment", "A#622000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Tubes", "A#621000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Quenches", "A#622000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Cars", "A#622000"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Other", "A#622000"))
						End If
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							UD8NameDatacell = "Capitalized"
							UD8NameSource = "IFRS16_Plan"
							UD8NameDestination = "IFRS16_Plan_Src"
							UD7NameSource = $"{item.key}"
							accountFilter = $"{item.Value}"
							'% equipement rental cost A#320000
							IncreasePercentage = api.Data.GetDataCell($"T#{InputMonth}:V#Periodic:A#320000:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
'							api.LogMessage("IncreasePercentage" & IncreasePercentage)
'							Source = $"V#Periodic:O#Import:A#Tech_Purchases:F#F_TotMov:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD7#Diagnostic_equipment:UD8#IFRS16_Plan" 
'							Destination = $"V#Periodic:O#Import:A#621000:F#None:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#Diagnostic_equipment:UD8#IFRS16_Plan_Src"
							If ScenarioMember.XFContainsIgnoreCase("5YP") Then
								Source = $"V#Periodic:O#Forms:A#9_2_3_3_2_3:F#FPA_CF_ADJ:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#Pre_IFRS16_Plan + V#Periodic:O#Forms:A#9_2_2_2:F#FPA_CF_ADJ:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#Pre_IFRS16_Plan" 
							Else
								Source = $"V#Periodic:O#Import:A#Tech_Purchases:F#None:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD7#{UD7NameSource}:UD8#{UD8NameSource}" 
							End If
							
							Destination = $"V#Periodic:O#Import:{accountFilter}:F#F_RoU_Mov_1:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#{UD7NameSource}:UD8#{UD8NameDestination}"
'							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan_Src")
						    api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import",ud7Filter:="U7#"& UD7NameSource &"", ud8Filter:="U8#IFRS16_Plan_Src")
						
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							 
'							sourcedatabuffer.LogDataBuffer(api,"source buffer: ", 50)
							
							
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									'Calculate	
									cell.CellAmount = (cell.CellAmount * IncreasePercentage)
									If cell.CellAmount <> 0
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
'							resultDataBuffer.LogDataBuffer(api, "resultbuffer", 50)
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
							End If	
						Next
						
						
'						Purchases Operating Leases (capex)  : Lease Liabilities
						FilterTwoStrings.Clear
						
						If ScenarioMember.XFContainsIgnoreCase("5YP") Then
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("None", "A#811803"))
						Else
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Capitalized_salary", "A#811804"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Construction", "A#811805"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("IT", "A#811802"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("CTC_equipment", "A#811803"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Diagnostic_equipment", "A#811803"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Other_medical_equipment", "A#811800"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Tubes", "A#811803"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Quenches", "A#811800"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Cars", "A#811801"))
							FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Other", "A#811804"))
						End If
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
							UD8NameDatacell = "Capitalized"
							UD8NameSource = "IFRS16_Plan"
							UD8NameDestination = "IFRS16_Plan_Src"
							UD7NameSource = item.key
							accountFilter = item.Value
							'% equipement rental cost A#320000
							IncreasePercentage = api.Data.GetDataCell($"T#{InputMonth}:V#Periodic:A#320000:F#Top:O#Top:I#Top:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDatacell}").CellAmount
							
							If ScenarioMember.XFContainsIgnoreCase("5YP") Then
								Source = $"V#Periodic:O#Forms:A#9_2_3_3_2_3:F#FPA_CF_ADJ:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#Pre_IFRS16_Plan + V#Periodic:O#Forms:A#9_2_2_2:F#FPA_CF_ADJ:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#Pre_IFRS16_Plan" 
							Else
								Source = $"V#Periodic:O#Top:A#Tech_Purchases:F#None:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD7#{UD7NameSource}:UD8#{UD8NameSource}" 
							End If
							
							Destination = $"V#Periodic:O#Import:{accountFilter}:F#F_Leases_ST_LT_Mov_1:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#{UD7NameSource}:UD8#{UD8NameDestination}"
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, ud7Filter:="U7#"&  UD7NameSource, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan_Src")
						
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									'Calculate	
									cell.CellAmount = -(cell.CellAmount * IncreasePercentage) 
'									cell.CellAmount = (cell.CellAmount * IncreasePercentage) 
									If cell.CellAmount <> 0
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
							
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
							End If	
						Next
						
						
						'18/09/2024 : Pablo G requested to Change the A#620050 (Building RoU asset - accumulated dep)--> A#621050 (Medical equipment RoU asset - accumulated dep)	
'						Lease Liabilities Amortization : A#620050 = TOTAL SGA U8#IFRS16 - A#430800 Other interest expenses U8#IFRS16
'						Lease Liabilities Amortization : A#621050 = TOTAL SGA U8#IFRS16 - A#430800 Other interest expenses U8#IFRS16
						FilterTwoStrings.Clear
'						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Building RoU asset - accumulated dep", "A#620050"))
						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Medical equipment RoU asset - accumulated dep", "A#621050"))
						
						
						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
'							UD8NameDatacell = "Capitalized"
							UD8NameSource = "IFRS16_Plan"
							UD8NameDestination = "IFRS16_Plan_Src"
							accountFilter = item.Value
							Source = $"V#Periodic:A#8_1_1_1_1_4:O#Import:F#Top:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD8#{UD8NameSource} - V#Periodic:A#430800:O#Import:F#Top:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD8#{UD8NameDestination}"
'							Source = $"V#Periodic:A#8_1_1_1_1_3:O#Import:F#Top:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD8#{UD8NameDestination} - V#Periodic:A#430800:O#Import:F#Top:UD1#Top:UD2#Top:UD3#Top:UD4#Top:UD5#Top:UD6#Top:UD8#{UD8NameDestination}"
							Destination = $"V#Periodic:O#Import:{accountFilter}:F#F_RoU_Mov_1:UD1#None:UD2#None:UD3#None:UD4#None:UD5#None:UD6#None:UD7#None:UD8#{UD8NameDestination}"
							
							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan_Src")
						
							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
							If Not sourceDataBuffer Is Nothing Then
								Dim resultDataBuffer As DataBuffer = New DataBuffer()
								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									'Calculate	
									cell.CellAmount = cell.CellAmount
									If cell.CellAmount <> 0
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
									End If
								Next
							
								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
							End If	
						Next
						
					'PL depreciation		
					FilterTwoStrings.Clear
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("depreciation", "A#PPACV403"))
'					To add To the move region !!!
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = "IFRS16_Plan_Src"'$"{item.key}"
						UD8NameDestination = "IFRS16_Plan_Src"'$"{item.key}"
						AccountFilter = item.Value
						'18/09/2024 : Pablo G requested to Change the A#620050 (Building RoU asset - accumulated dep)--> A#621050 (Medical equipment RoU asset - accumulated dep)
'						Source = $"V#Periodic:A#620050:O#Import:F#F_RoU_Mov_1:UD8#{UD8NameSource}"'Amortization
						Source = $"V#Periodic:A#621050:O#Import:F#F_RoU_Mov_1:UD8#{UD8NameSource}"'Amortization 
						Destination = $"V#Periodic:{AccountFilter}:O#Import:F#None:UD8#{UD8NameDestination}"
						
						api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan_Src")						
						sourceDataBuffer = Api.Data.GetDataBufferUsingFormula(Source)
						DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
						If Not sourceDataBuffer Is Nothing Then
							Dim resultDataBuffer As DataBuffer = New DataBuffer()
							For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
'								intFlowid = api.Members.GetMemberId(dimtype.Flow.Id, api.UD1.Text(cell.DataBufferCellPk.UD1Id,1))
'								If intFlowid <> -1
'									Cell.DataBufferCellPk.FlowId = intFlowid
'								End If 
'								Cell.DataBufferCellPk.UD5Id = api.UD1.GetDefaultUDMemberId(cell.DataBufferCellPk.UD1Id, dimtype.UD5.Id)
'								Cell.DataBufferCellPk.UD7Id = api.UD1.GetDefaultUDMemberId(cell.DataBufferCellPk.UD1Id, dimtype.UD7.Id)
								If cell.CellAmount <> 0
									'03Jul24 change sign for positive expense acc type
									cell.CellAmount = (cell.CellAmount * -1)
									resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
								End If
							Next
							api.Data.SetDataBuffer(resultDataBuffer, DestinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
						End If	
					Next
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearDataFromAccount") Then
						
						api.Data.ClearCalculatedData(True, False, False, True,accountFilter:="A#620050", originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan")	
					
					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("IFRS16_Move") Then
					
#Region "IFRS16: Move Values To U8#IFRS16_Plan"
					
					
					FilterTwoStrings.Clear
'					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("IFRS16_Plan", "A#PPACV400,A#PPACV403,A#311000,A#320000,A#430800"))
					'Calculated lines
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Rental fees/Medical Equipment Rental / Other interest expenses", "A#311000,A#320000,A#430800"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("RoU / Lease Liabilities", "A#620000,A#621000,A#622000,A#811800,A#811801,A#811802,A#811803,A#811804,A#811805"))
					'18/09/2024 : Pablo G requested to Change the A#620050 (Building RoU asset - accumulated dep)--> A#621050 (Medical equipment RoU asset - accumulated dep)
'					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Building RoU asset - accumulated dep", "A#620050"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Medical equipment RoU asset - accumulated dep", "A#621050"))
					

					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("depreciation", "A#PPACV403"))
					
					'Manual input lines
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Revneue", "A#122000,A#123000,A#111000, A#112000,A#100000,A#104000,A#8_1_1_1_1_1_1_2_1.Base,A#120000,A#125000,A#128000,A#101000,A#101001,A#101002,A#101003,A#101004,A#101005,A#102000,A#114000,A#105000,A#113000,A#103000, A#111001, A#111002, A#111003, A#111004, A#100003, A#100004, A#100005, A#100006, A#100007, A#100009, A#100010,A#107000,A#121000,A#108000,A#106000,A#130000,A#110000,A#204000_rev_counter, A#205000_rev_counter"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("COS", "A#200000, A#201000, A#204000, A#205200, A#207200,A#203000,A#206000, A#205000, A#207000, A#207005 ,A#203010, A#210000, A#210300, A#323000, A#205030, A#205031, A#205032, A#205000_cos_counter, A#204000_cos_counter, A#205020"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("MD", "A#8_1_1_1_1_2_1_1_1.Base.Remove(300100,300300,300650,300700,300750,300490,300491,300492,300493,300494,300495),A#8_1_1_1_1_2_1_2_1.Base.Remove(301100,301300,301650,301700,301750,301490,301491,301492,301493,301494,301495),A#8_1_1_1_1_2_1_3_1.Base.Remove(302100,302300,302650,302700,302750,302490,302491,302492,302493,302494,302495)"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("SGA", "A#8_1_1_1_1_4_1_1_1.Base, A#8_1_1_1_1_4_1_3_2_1.base, A#8_1_1_1_1_4_1_3_4.base, A#8_1_1_1_1_4_1_1_2.Base,A#306100,A#8_1_1_1_1_4_1_11.Base,A#8_1_1_1_1_4_1_2_4_1.Base,A#314600,A#311100,A#313200,A#314300,A#314400,A#314500,A#320000,A#311000,A#315000,A#8_1_1_1_1_4_1_2_4_2.Base,A#8_1_1_1_1_4_1_9_1.Base.Remove(Z33130),A#8_1_1_1_1_4_1_4.Base,A#8_1_1_1_1_4_1_6_1.Base,A#372000,A#372100,A#372400,A#372500,A#372600,A#8_1_1_1_1_4_1_5_1.Base,A#8_1_1_1_1_4_1_5_2.Base,A#8_1_1_1_1_4_1_5_3.Base,A#8_1_1_1_1_4_1_8_1.Base,A#8_1_1_1_1_4_1_10_1.Base.Remove(Z36130,123412),A#8_1_1_1_1_4_1_12_1.Base,A#321000"))
					FilterTwoStrings.Add(New KeyValuePair(Of String, String)("BTL", "A#470700,A#470610,A#410000,A#410100,A#410200,A#411000,A#412000,A#452000,A#452100,A#470050,A#470090,A#470100,A#470200,A#470300,A#470400,A#470500,A#470550,A#470600,A#470650,A#470800,A#471500,A#435000,A#440000,A#8_1_1_2_1_1_1_2_1.Base,A#431100,A#420000,A#8_1_2_1_1_3_1_2_1.Base"))
					
					
					For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
						UD8NameSource = "IFRS16_Plan_Src"'$"{item.key}"
						UD8NameDestination = "IFRS16_Plan"'$"{item.key}"
						AccountFilter = item.Value
						Source = $"V#Periodic:O#BeforeAdj:UD8#{UD8NameSource}"
						Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
						api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan")						
						sourceDataBuffer = Api.Data.GetDataBufferUsingFormula(Source)
						DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
						If Not sourceDataBuffer Is Nothing Then
							Dim resultDataBuffer As DataBuffer = New DataBuffer()
							For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
'								intFlowid = api.Members.GetMemberId(dimtype.Flow.Id, api.UD1.Text(cell.DataBufferCellPk.UD1Id,1))
'								If intFlowid <> -1
'									Cell.DataBufferCellPk.FlowId = intFlowid
'								End If 
'								Cell.DataBufferCellPk.UD5Id = api.UD1.GetDefaultUDMemberId(cell.DataBufferCellPk.UD1Id, dimtype.UD5.Id)
'								Cell.DataBufferCellPk.UD7Id = api.UD1.GetDefaultUDMemberId(cell.DataBufferCellPk.UD1Id, dimtype.UD7.Id)
								If cell.CellAmount <> 0
									resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
								End If
							Next
							api.Data.SetDataBuffer(resultDataBuffer, DestinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
						End If	
					Next
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("IFRS16_Roll_Forward") Then
						
#Region "IFRS16_Roll_Forward"
						
''						Check The accounts For Roll Forward IFRS16 !! 
'						FilterTwoStrings.Clear
'						FilterTwoStrings.Add(New KeyValuePair(Of String, String)("Deferred tax assets", "A#1_1_9.Base"))
						
'						For Each item As KeyValuePair(Of String, String) In FilterTwoStrings
'							UD8NameSource = "IFRS16_Plan"
'							UD8NameDestination = "IFRS16_Plan"
'							accountFilter = item.Value
							
'							Source = $"T#POVPrior1:V#Periodic:O#Import:UD1#Top:UD2#Top:UD3#Top:UD5#Top:UD6#Top:UD7#Top:UD8#{UD8NameSource}" 
'							Destination = $"V#Periodic:O#Import:UD1#None:UD2#None:UD3#None:UD5#None:Ud6#None:UD7#None:UD8#{UD8NameDestination}"
''							Destination = $"V#Periodic:O#Import:UD8#{UD8NameDestination}"
'							api.Data.ClearCalculatedData(True, False, False, True, accountFilter:=accountFilter, originFilter:="O#Import", ud8Filter:="U8#IFRS16_Plan")
						
'							sourceDataBuffer= Api.Data.GetDataBufferUsingFormula(Source)
'							DestinationInfo = api.Data.GetExpressionDestinationInfo(Destination)
'							If Not sourceDataBuffer Is Nothing Then
'								Dim resultDataBuffer As DataBuffer = New DataBuffer()
'								For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
									
'										cell.CellAmount = cell.CellAmount 
'										If cell.CellAmount <> 0
'											resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
'										End If
										
'								Next
							
'								api.Data.SetDataBuffer(dataBuffer:=resultDataBuffer, expressionDestinationInfo:= DestinationInfo, accountFilter:=AccountFilter, isDurableCalculatedData:=True)
'							End If	
'						Next
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DXO_Movements") Then
						
#Region "DXO_Movements"
						

					'Dim Source As String = Nothing
					Dim Source2 As String = Nothing
					'Dim Destination As String = Nothing
					Dim sdaysinmonth As String = api.Time.GetNumDaysInTimePeriod(api.Pov.time.MemberId).ToString 
					'Dim Formula As String = Nothing
					SourcePOV = ":I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top"
					DestinationPOV = ":I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None" 
					
					api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#561000, A#510000, A#710000", Flowfilter:="F#F_CLO", OriginFilter:="O#Import")

						Source = $"A#561000:F#None:V#Periodic:O#Forms:U8#DXO{SourcePOV} "
						Source2 = $"(A#8_1_1_1_1_1_2:F#Top:V#Periodic:O#Top:U8#Pre_ifrs16_plan{SourcePOV} / {sdaysinmonth})"
'						Destination = $"A#561000:F#FPA_MOV_TECH:V#Periodic:O#Import:U8#Pre_ifrs16_plan{DestinationPOV}"
'						02Jul24 updated to calculate closing balance
						Destination = $"A#561000:F#F_CLO:V#YTD:O#Import:U8#Pre_ifrs16_plan{DestinationPOV}"
						Formula = $"{Destination}=RemoveZeros({source}*{source2})"
						api.Data.Calculate(Formula, isDurableCalculatedData:=True)	
											
						Source = $"A#510000:F#None:V#Periodic:O#Forms:U8#DXO{SourcePOV} "
						Source2 = $"(-A#8_1_1_1_1_1_1:F#Top:V#Periodic:O#Top:U8#Pre_ifrs16_plan{SourcePOV} / {sdaysinmonth} )"
'						Destination = $"A#510000:F#FPA_MOV_TECH:V#Periodic:O#Import:U8#Pre_ifrs16_plan{DestinationPOV}"
'						02Jul24 updated to calculate closing balance
						Destination = $"A#510000:F#F_CLO:V#YTD:O#Import:U8#Pre_ifrs16_plan{DestinationPOV}"
						Formula = $"{Destination}=RemoveZeros({source}*{source2})"
						api.Data.Calculate(Formula, isDurableCalculatedData:=True)	
						
						Source = $"A#710000:F#None:V#Periodic:O#Forms:U8#DXO{SourcePOV} "
						Source2 = $"(A#8_1_1_1_1_1_2:F#Top:V#Periodic:O#Top:U8#Pre_ifrs16_plan{SourcePOV} + A#8_1_1_1_1_4:F#Top:V#Periodic:O#Top:U8#Pre_ifrs16_plan{SourcePOV}- A#8_1_1_1_1_4_1_1:F#Top:V#Periodic:O#Top:U8#Pre_ifrs16_plan{SourcePOV}) / {sdaysinmonth} )"
'						Destination = $"A#710000:F#FPA_MOV_TECH:V#Periodic:O#Import:U8#Pre_ifrs16_plan{DestinationPOV}"
'						02Jul24 updated to calculate closing balance
						Destination = $"A#710000:F#F_CLO:V#YTD:O#Import:U8#Pre_ifrs16_plan{DestinationPOV}"
						Formula = $"{Destination}=RemoveZeros(-({source}*{source2}))"
						api.Data.Calculate(Formula, isDurableCalculatedData:=True)	
					
						'29Aug24 added calc of closing balance for ic detail of the accounts. Roll forward
						'13Sep24 updated so that it also works in Budget scenario. pull opening instead of closing prior period
						'31Oct24 updated for QFCST scenarios
'						Source = "(F#F_CLO:T#POVPRIOR1:O#TOP:V#YTD:U8#Pre_ifrs16_plan)"
						If api.Pov.Scenario.Name.Contains("Budget") Or  api.Pov.Scenario.Name.Contains("5YP") 
						Source = "(F#F_OPE:O#TOP:V#YTD:U8#Pre_ifrs16_plan)"
						ElseIf  api.Pov.Scenario.Name.Contains("Q_FCST") 
							If api.Time.IsFirstPeriodInYear()
							Source = "(F#F_OPE:O#TOP:V#YTD:U8#Pre_ifrs16_plan)"
							Else
							Source = "(F#F_CLO:T#POVPRIOR1:O#TOP:V#YTD:U8#Pre_ifrs16_plan)"
							End If
						End If
						Destination = "F#F_CLO:O#import:V#YTD:U8#Pre_ifrs16_plan"
						Formula = $"{Destination}=RemoveZeros({source})"
						api.Data.Calculate(Formula,"A#561000, A#510000, A#710000",,,"IC#Top.base.remove(None)",,,,,,,,,,,True)	
															
#End Region						
											
					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Clear_Specific_Data") Then
						
#Region "Clear Specific data"
'						AnNe: Clear data specific
						Source = "O#Forms"
						Destination = Source
						Formula = $"{Destination}={source}"
						api.Data.Calculate(Formula)
						
						api.Data.ClearCalculatedData(Source,True,True,True)
#End Region

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("QFCST_To_BudgetV1") Then 
						
#Region "Seed QFCST_X_Y to BudgetV1"

					'Clear seeded data 
					Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#8.base", Ud8Filter:="U8#Pre_ifrs16_plan_budgetv1_load, U8#Price_budgetv1_load, U8#Volume_budgetv1_load, U8#Copayment")
					Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#8_1_2_1_1_2.base, A#8_1_2_1_1_3.base", Ud8Filter:="U8#Pre_ifrs16_plan")
					Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#Non_Financial.base, A#8_1_2_1_1_3.base", Ud8Filter:="U8#volume")
					Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#0.base", FlowFilter:="F#F_TotMov.Base")
'					brapi.ErrorLog.LogMessage(si, "701")
'					'Define the source scenario through parameter		
					Dim SourceScenario As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SourceScenario")
'					brapi.ErrorLog.LogMessage(si, "SourceScenario " & SourceScenario)					

					'Run seeding calculation
'					api.Data.Calculate("S#Budgetv1:O#Import:F#None:U4#None:U5#None:U7#None:U8#IFRS16_Plan_Src = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:F#Top:U4#None:U5#Top:U7#Top:U8#IFRS16_Plan_Src)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
					
					api.Data.Calculate("S#Budgetv1:O#Import:I#None:F#None:U4#None:U5#None:U7#None:U8#Pre_ifrs16_plan_budgetv1_load = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Pre_IFRS16_Plan)", AccountFilter:="A#8_1_1_1_1.base, A#8_1_1_1_2_1_1.base, A#8_1_2_1_1_1.base,A#8_1_1_2_1_1.base, A#8_1_1_2_1_2.base",isDurableCalculatedData:=True)

					api.Data.Calculate("S#Budgetv1:O#Import:I#None:F#None:U4#None:U5#None:U7#None:U8#Price_budgetv1_load = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:F#Top:I#None:U4#None:U5#Top:U7#Top:U8#Price)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
					
					api.Data.Calculate("S#Budgetv1:O#Import:I#None:F#None:U4#None:U5#None:U7#None:U8#Volume_budgetv1_load = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:F#Top:I#None:U4#None:U5#Top:U7#Top:U8#Volume)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)

					api.Data.Calculate("S#Budgetv1:O#Import:I#None:F#None:U4#None:U5#None:U7#None:U8#Unit_Cost_budgetv1_load = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Unit_Cost)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
					
					api.Data.Calculate("S#Budgetv1:O#Import:I#None:F#None:U4#None:U5#None:U7#None:U8#Copayment = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Copayment)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)

					'tax and other PL lines below BTL, without % increase functionality. Can go directly to their final dimensionality
					api.Data.Calculate("S#Budgetv1:O#Import:I#None:F#None:U4#None:U5#None:U7#None:U8#Pre_IFRS16_Plan = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Pre_IFRS16_Plan)", AccountFilter:="A#8_1_2_1_1_2.base, A#8_1_2_1_1_3.base",isDurableCalculatedData:=True)

					
					'non financials
					api.Data.Calculate("S#Budgetv1:O#Import:I#None:U4#None:U8#volume = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:I#None:U4#None:U8#volume)", AccountFilter:="A#Non_Financial.base",isDurableCalculatedData:=True)
					
					'balance sheet movements
					api.Data.Calculate("S#Budgetv1:O#Import:I#None:U2#None:U3#None:U4#None = RemoveZeros(T#PovPrior12:S#"& SourceScenario &":O#Top:I#None:U2#Top:U3#Top:U4#Top)", AccountFilter:="A#0.base.remove(970000)",FlowFilter:="F#F_TotMov.Base",isDurableCalculatedData:=True) 'all bs except retained earnings
					
#End Region 

					ElseIf args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("FCST_To_FCST_Seeding") Then
						
#Region "Seed adjustments from fcst to fcst"

Dim TargetScenario As String = api.Pov.Scenario.Name
Dim SourceScenario As String = BRApi.Finance.Scenario.Text(si,si.WorkflowClusterPk.ScenarioKey,4)
	
If TargetScenario <> "FC_1_11" Then
	
	'clear calculated data
	Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#8.base", OriginFilter:="O#Forms")

	'Seed MFCST adjustments
	api.Data.Calculate("S#"& TargetScenario &":O#Forms = RemoveZeros(S#"& SourceScenario &":O#Forms)")
	
End If

#End Region 


					End If
				End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
'		Public Function FPAAccountCalc(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal str_FPA_Account As String) As String
'			Try	

'							Dim Source As String = Nothing
'							Dim Source2 As String = Nothing
'							Dim Destination As String = Nothing
'							Dim sdaysinmonth As String = api.Time.GetNumDaysInTimePeriod(api.Pov.time.MemberId).ToString 
'							Dim Formula As String = Nothing

'				Select Case str_FPA_Account

'	'				Case 710000
'	'					If api.Pov.Entity.Name.XFContainsIgnoreCase("Spain_Input_OP") And api.Pov.Scenario.Name.XFEqualsIgnoreCase("Budgetv1") And api.Pov.Time.Name.XFEqualsIgnoreCase("2025M1")
'	'							Source = $"A#710000:F#None:V#Periodic:O#Forms:U8#DXO "
'	'							Source2 = $"(A#8_1_1_1_1_1_2:F#Top:V#Periodic:O#Top:U8#Top + A#8_1_1_1_1_4:F#Top:V#Periodic:O#Top:U8#Top- A#8_1_1_1_1_4_1_1:F#Top:V#Periodic:O#Top:U8#Top) / {sdaysinmonth} )"
'	'							Destination = $"A#510000:F#FPA_MOV_TECH:V#Periodic:O#Import:U8#Pre_ifrs16_plan"
'	'							Formula = $"{Destination}=RemoveZeros({source}*{source2})"
'	'							api.Data.Calculate(Formula)
'	'					End If

'	'				Case 510000
'						'If api.Pov.Entity.Name.XFContainsIgnoreCase("Spain_Input_OP") And api.Pov.Scenario.Name.XFEqualsIgnoreCase("Budgetv1") And api.Pov.Time.Name.XFEqualsIgnoreCase("2025M1")
'							'	Source = $"A#510000:F#None:V#Periodic:O#Forms:U8#DXO "
'							'	Source2 = $"(A#8_1_1_1_1_1_1:F#Top:V#Periodic:O#Top:U8#Top / {sdaysinmonth} )"
'							'	Destination = $"A#510000:F#FPA_MOV_TECH:V#Periodic:O#Import:U8#Pre_ifrs16_plan"
'							'	Formula = $"{Destination}=RemoveZeros({source}*{source2})"
'							'	api.Data.Calculate(Formula)
'						'End If

'					Case 561000
'								Source = $"A#561000:F#None:V#Periodic:O#Forms:U8#DXO "
'								Source2 = $"(A#8_1_1_1_1_1_2:F#Top:V#Periodic:O#Top:U8#Top / {sdaysinmonth})"
'								Destination = $"A#561000:F#FPA_MOV_TECH:V#Periodic:O#Import:U8#Pre_ifrs16_plan"
'								Formula = $"{Destination}=RemoveZeros({source}*{source2})"
'								api.Data.Calculate(Formula)

							
'				End Select
'				Return Nothing
'			Catch ex As Exception
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
'		End Function			

		
	End Class
End Namespace