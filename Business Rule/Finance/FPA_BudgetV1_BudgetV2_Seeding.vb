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

Namespace OneStream.BusinessRule.Finance.FPA_BudgetV1_BudgetV2_Seeding
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate
					

					If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BudgetV1_To_BudgetV2_Seeding") Then
														
							'Clear seeded data 
							Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#8.base", Ud8Filter:="U8#IFRS16_Plan_Src, U8#Pre_IFRS16_Plan, U8#Pre_IFRS16_Plan_Src, U8#Price_Src, U8#Volume_Src, U8#Unit_Cost_Src, U8#Copayment")
							Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#8_1_2_1_1_2.base, A#8_1_2_1_1_3.base, A#IntExpNonDedAdj_Tax, A#ManFees_Tax, A#NOL_Tax, A#OthAdj_Tax, A#TaxRate_Tax, A#ManualAddRed_Tax", Ud8Filter:="U8#Pre_ifrs16_plan")
							Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#Non_Financial.base", Ud8Filter:="U8#Volume")
							Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#0.base", FlowFilter:="F#F_TotMov.Base")
							Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#311000, A#320000, A#430800", Ud8Filter:="U8#Capitalized")

'''''							Api.Data.ClearCalculatedData(True,True,True,True,AccountFilter:="A#320000, A#400000, A#610000, A#610050, A#613000, A#613050, A#731100, A#750500, A#780000, A#Tech_Purchases", FlowFilter:="F#F_TotMov.Base")

''					'Run seeding calculation
					api.Data.Calculate("S#Budgetv2:O#Import:F#None:I#None:U4#None:U5#None:U7#None:U8#IFRS16_Plan_Src = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#IFRS16_Plan)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
					
					api.Data.Calculate("S#Budgetv2:O#Import:F#None:I#None:U4#None:U5#None:U7#None:U8#Pre_IFRS16_Plan_Src = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Pre_IFRS16_Plan)", AccountFilter:="A#8.base",isDurableCalculatedData:=True) ', AccountFilter:="A#8_1_1_1_1.base, A#8_1_1_1_2_1_1.base, A#8_1_1_2_1_1.base, A#8_1_2_1_1_1.base, A#8_1_1_2_1_2.base",isDurableCalculatedData:=True)
						
'					api.Data.Calculate("S#Budgetv2:O#Import:F#None:I#None:U4#None:U5#None:U7#None:U8#Pre_IFRS16_Plan_Src = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Pre_IFRS16_Plan)", AccountFilter:="A#8.base",isDurableCalculatedData:=True) ', AccountFilter:="A#8_1_1_1_1.base, A#8_1_1_1_2_1_1.base, A#8_1_1_2_1_1.base, A#8_1_2_1_1_1.base, A#8_1_1_2_1_2.base",isDurableCalculatedData:=True)
				
					api.Data.Calculate("S#Budgetv2:O#Import:F#None:I#None:U4#None:U5#None:U7#None:U8#Price_Src = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Price)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
					
					api.Data.Calculate("S#Budgetv2:O#Import:F#None:I#None:U4#None:U5#None:U7#None:U8#Volume_Src = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Volume)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)

					api.Data.Calculate("S#Budgetv2:O#Import:F#None:I#None:U4#None:U5#None:U7#None:U8#Unit_Cost_Src = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Unit_Cost)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
					
					api.Data.Calculate("S#Budgetv2:O#Import:F#None:I#None:U4#None:U5#None:U7#None:U8#Copayment = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Copayment)", AccountFilter:="A#8.base",isDurableCalculatedData:=True)
					
					'(technical) tax accounts and other PL lines below BTL, without % increase functionality. Can go directly to their final dimensionality
''					api.Data.Calculate("S#Budgetv2:O#Import:I#None:F#None:U4#None:U5#None:U7#None:U8#Pre_IFRS16_Plan = RemoveNodata(S#Budgetv1:O#Top:I#None:F#Top:U4#None:U5#Top:U7#Top:U8#Pre_IFRS16_Plan)", AccountFilter:=", A#8_1_2_1_1_2.base, A#8_1_2_1_1_3.base, A#IntExpNonDedAdj_Tax, A#ManFees_Tax, A#NOL_Tax, A#OthAdj_Tax, A#TaxRate_Tax, A#ManualAddRed_Tax",isDurableCalculatedData:=True)
					api.Data.Calculate("S#Budgetv2:O#Import:I#None:U4#None:U8#Pre_IFRS16_Plan = RemoveNodata(S#Budgetv1:O#Top:I#None:U4#None:U8#Pre_IFRS16_Plan)", AccountFilter:="A#IntExpNonDedAdj_Tax, A#ManFees_Tax, A#NOL_Tax, A#OthAdj_Tax, A#TaxRate_Tax, A#ManualAddRed_Tax",isDurableCalculatedData:=True)''A#8_1_2_1_1_2.base, A#8_1_2_1_1_3.base, 
					
					'IFRS16 Drivers. Using U8#Capitalized
					api.Data.Calculate("S#Budgetv2:O#Import:I#None:F#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#Capitalized = RemoveNodata(S#Budgetv1:O#Top:I#None:F#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#Capitalized)", AccountFilter:= "A#311000, A#320000, A#430800",isDurableCalculatedData:=True)
					
					'statistics 
					api.Data.Calculate("S#Budgetv2:O#Import:I#None:U4#None:U8#Volume = RemoveNodata(S#Budgetv1:O#Top:I#None:U4#None:U8#Volume)", AccountFilter:="A#Non_Financial.base",isDurableCalculatedData:=True)
					
					'balance sheet movements
					'updated for capex to keep the level of detail for ud2, Andreas Nederhoed 1 April 2025
					api.Data.Calculate("S#Budgetv2:O#Import:I#None:U3#None:U4#None = RemoveZeros(S#Budgetv1:O#Top:I#None:U3#Top:U4#Top)", AccountFilter:="A#0.base.remove(970000)",FlowFilter:="F#F_TotMov.Base",isDurableCalculatedData:=True) 'all bs mov except retained earnings
''''					api.Data.Calculate("S#Budgetv2:O#Import:I#None:U2#None:U3#None:U4#None = RemoveNodata(S#Budgetv1:O#Top:I#None:U2#Top:U3#Top:U4#Top)", AccountFilter:="A#0.base.remove(970000)",FlowFilter:="F#F_TotMov.Base",isDurableCalculatedData:=True) 'all bs mov except retained earnings

''''''					____NEW Andreas
''''''					api.Data.Calculate("S#Budgetv2:O#Import:I#None:U3#None:U4#None = RemoveZeros(S#Budgetv1:O#Top:I#None:U3#Top:U4#Top)", AccountFilter:="A#320000, A#400000, A#610000, A#610050, A#613000, A#613050, A#731100, A#750500, A#780000, A#Tech_Purchases",FlowFilter:="F#F_TotMov.Base", isDurableCalculatedData:=True) 'all bs mov except retained earnings
''''''					____NEW Andreas
''''''					Ezequiel Chiappero, 05 March 25, balance sheet opening balance To F#F_OPE_Tech
					api.Data.Calculate( "S#Budgetv2:O#Import:I#None:F#F_OPE_tech:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None:U8#Pre_IFRS16_Plan = RemoveNoData(S#Budgetv1:O#Top:I#None:F#F_OPE_tech:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U7#Top:U8#Pre_IFRS16_Plan)",AccountFilter:="A#0.Base",FlowFilter:="F#F_OPE_tech", isDurableCalculatedData:=True)
					
'					205000
					api.Data.Calculate("S#Budgetv2:O#Import:I#None:U2#None:U3#None:U4#None:U8#Pre_IFRS16_Plan = RemoveNodata(S#Budgetv1:O#Top:I#None:U2#Top:U3#Top:U4#Top:U8#Pre_IFRS16_Plan)", AccountFilter:="A#8.base",isDurableCalculatedData:=True) 
				
	
#Region "BUDV2 QFC: Move price and volume the correct flow, ud5 and ud7"
					If api.Pov.Scenario.Name.XFEqualsIgnoreCase("Budgetv2") And api.Time.GetYearFromId = 2025
						Dim sourceDataBuffer As DataBuffer = Nothing
						Dim DestinationInfo As ExpressionDestinationInfo = Nothing
						Dim intFlowid As Integer
							Dim AccountFilter As String ="A#8_1_1_1_1_1_1.Base, A#8_1_1_1_1_1_2.Base"' "A#8.Base"' 
							Dim Source As String  = $"V#Periodic:O#BeforeAdj:UD8#Pre_IFRS16_Plan_Src"
							Dim Destination As String = $"V#Periodic:O#Import:UD8#Pre_IFRS16_Plan"
							Dim val As Decimal= 0
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
										resultDataBuffer.SetCell(api.DbConnApp.SI, cell, True)
										End If
									End If
								Next
'								api.Data.SetDataBuffer(resultDataBuffer, DestinationInfo, accountFilter:=accountFilter, isDurableCalculatedData:=True)
							End If	
					End If
#End Region
					
					
					End If 'args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BudgetV1_To_BudgetV2_Seeding")
						
					
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace