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

Namespace OneStream.BusinessRule.Finance.SALES_Calculates
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					Case Is = FinanceFunctionType.CustomCalculate
						
						#Region "Load Sales To Own Units"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("LoadSalesToOwnUnits") Then
							'Declare target and source data buffers
							Dim targetDataBuffer As New DataBuffer
							Dim siDestinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBuffer(DataApiScriptMethodType.Calculate, "A#Sales", siDestinationInfo)
							'Process each data buffer cell to change the account member
							For Each cell As DataBufferCell In sourceDataBuffer.DataBufferCells.Values
								Dim targetCell As New DataBufferCell(cell)
								'Filter granular delivery out to prevent repeating sales
								Dim cellU1Name As String = api.Members.GetMemberName(dimType.UD1.Id, targetCell.DataBufferCellPk.UD1Id)
								Dim cellOriginName As String = api.Members.GetMemberName(dimType.Origin.Id, targetCell.DataBufferCellPk.OriginId)
								If cellU1Name <> "External_Delivery" AndAlso cellU1Name <> "Own_Delivery" AndAlso cellOriginName <> "Forms"
									targetCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account.Id,"AL_VTA_RES")
									targetCell.DataBufferCellPk.UD1Id = api.Members.GetMemberId(DimType.UD1.Id,"None")
									targetDataBuffer.SetCell(si, targetCell, True)
									'Clean uploaded sales errors for openings
'									If cellOriginName = "Forms" Then
'										targetCell.DataBufferCellPk.OriginId = api.Members.GetMemberId(DimType.Origin.Id, "Import")
'										targetCell.CellAmount = 0
'										targetDataBuffer.SetCell(si, targetCell, False)
'									End If
								End If
							Next
							'Process data buffer
							api.Data.SetDataBuffer(targetDataBuffer,siDestinationInfo,,,,,,,,,,,,,True)
							
						#End Region
						
						#Region "ECI"
						
						#Region "ECI Calculate"
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ECICalculate") Then
							'Define parameters
							Dim ParamScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim ParamForecastMonth As Integer = CInt(BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month"))
							Dim povMonth As Integer = CInt(api.Pov.Time.Name.Split("M")(1))
							
							'Get if scenario is forecast and month bigger than forecast month or scenario is budget, else scenario is actual
							If (ParamScenario = "Budget") OrElse (ParamScenario = "Forecast" AndAlso povMonth >= ParamForecastMonth) Then
								Me.CalculateECIBudget(si, globals, api, args, ParamScenario, povMonth)
							Else If ParamScenario = "Actual"
								Me.CalculateECIActual(si, globals, api, args, ParamScenario, povMonth)
							End If
							
						#End Region
						
						#Region "ECI Eliminate"
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ECIEliminate") Then
							'Define parameters
							Dim ParamScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim ParamForecastMonth As Integer = CInt(BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month"))
							Dim povMonth As Integer = CInt(api.Pov.Time.Name.Split("M")(1))
							
							'Skip all actual months in forecast scenario
							If Not (ParamScenario = "Forecast" AndAlso povMonth < ParamForecastMonth) Then
								Me.EliminateECI(si, globals, api, args, ParamScenario, povMonth)
							End If
							
						#End Region
						
						#End Region
							
						End If
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
		#Region "ECI"
		
		#Region "ECI Budget"
		
		Public Sub CalculateECIBudget(
			ByVal si As SessionInfo,
			ByVal globals As BRGlobals,
			ByVal api As FinanceRulesApi,
			ByVal args As FinanceRulesArgs,
			ByVal ParamScenario As String,
			ByVal povMonth As Integer
		)
			'Final amount calculation
			Dim finalAmountCalc As String = $"
				A#ECI_Sales:F#Amount_Initial_Result:O#Forms:U1#None =
				- A#AL_OC_REN:F#None:O#Top:U1#Top
			"
			If api.Pov.Entity.Name = "SF172494" Then brapi.ErrorLog.LogMessage(si, api.Data.GetDataCell("A#AL_OC_REN:F#None:O#Top:U1#Top").CellAmountAsText)
			api.Data.ClearCalculatedData("A#ECI_Sales:F#None:O#Forms:U1#None", True, True, True, True)
			api.Data.ClearCalculatedData("A#ECI_Sales:F#Amount_Initial_Result:O#Forms:U1#None", True, True, True, True)
			api.Data.Calculate(finalAmountCalc, True)
			If api.Pov.Entity.Name = "SF172494" Then brapi.ErrorLog.LogMessage(si, api.Data.GetDataCell("A#ECI_Sales:F#Amount_Initial_Result:O#Forms:U1#None").CellAmountAsText)
			'Set the opposite amount in the dummy occupation account
			Dim occupationAmountCalc As String = $"
				A#90000170:F#Amount_Initial_Result:O#Forms:U1#None =
				- A#ECI_Sales:F#None:O#Forms:U1#None
			"
			api.Data.ClearCalculatedData("A#90000170:F#None:O#Forms:U1#None", True, True, True, True)
			api.Data.ClearCalculatedData("A#90000170:F#Amount_Initial_Result:O#Forms:U1#None", True, True, True, True)
			api.Data.Calculate(occupationAmountCalc, True)
			
		End Sub
		
		#End Region
		
		#Region "ECI Actual"
		
		Public Sub CalculateECIActual(
			ByVal si As SessionInfo,
			ByVal globals As BRGlobals,
			ByVal api As FinanceRulesApi,
			ByVal args As FinanceRulesArgs,
			ByVal ParamScenario As String,
			ByVal povMonth As Integer
		)
			'Initial amount calculation
			Dim initialAmountCalc As String = $"
				A#90000170:F#None:O#Import:U1#None =
				A#Sales:F#None:O#Top:U1#Top
				- (A#AL_VENTAS:F#None:O#Top:U1#None)
			"
			api.Data.ClearCalculatedData("A#90000170:F#None:U1#None", True, True, True, True)
			api.Data.Calculate(initialAmountCalc, True)
			'Set the opposite amount in the dummy occupation account
			Dim occupationAmountCalc As String = $"
				A#90000171:F#None:O#Import:U1#None =
				- A#90000170:F#None:O#Top:U1#None
			"
			api.Data.ClearCalculatedData("A#90000171:F#None:U1#None", True, True, True, True)
			api.Data.Calculate(occupationAmountCalc, True)
		End Sub
		
		#End Region
		
		#Region "Eliminate ECI"
		
		Public Sub EliminateECI(
			ByVal si As SessionInfo,
			ByVal globals As BRGlobals,
			ByVal api As FinanceRulesApi,
			ByVal args As FinanceRulesArgs,
			ByVal ParamScenario As String,
			ByVal povMonth As Integer
		)
			'Get a list of all the ECI entities
			Dim memberFilter As String = "E#P_SB.Children.Where(Description Contains [ ECI ]),E#P_SB_PT.Children.Where(Description Contains [ ECI ])"
			Dim ECIMemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(
				si,
				"Entities",
				memberFilter,
				True)
				
			'Get flow depending on scenario
			Dim sourceFlow As String = If(ParamScenario = "Actual", "None", "Amount_Initial_Result")
			Dim sAccountSalesSource As String = If(ParamScenario = "Actual", "90000170", "ECI_Sales")
			Dim sAccountSalesTarget As String = If(ParamScenario = "Actual", "90000170", "AL_VTA_CON")
				
			'Sum all the entities ECI Sales amount
			Dim ECISalesSum As Decimal
			If ECIMemberList.Count > 0 Then
				For Each ECIMember As MemberInfo In ECIMemberList
					Dim ECIName As String = ECIMember.Member.Name
					'Get the ECI Sales Account cell amount
					Dim ECISalesAmount As Decimal = api.Data.GetDataCell($"E#{ECIName}:A#{sAccountSalesSource}:F#{sourceFlow}:O#Top:U1#None").CellAmount
					ECISalesSum += ECISalesAmount
				Next
			End If
				
			'Populate dummy entity accounts
			Dim salesAmountCalc As String = $"
				V#Periodic:A#{sAccountSalesTarget}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
				- {ECISalesSum.ToString.Replace(",",".")}
			"
			api.Data.ClearCalculatedData($"A#{sAccountSalesTarget}:F#None:O#Forms:U1#None", True, True, True, True)
			api.Data.Calculate(salesAmountCalc, True)
			
			Dim occupationAmountCalc As String = $"
				V#Periodic:A#90000171:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
				- A#{sAccountSalesTarget}:F#None:O#Forms:U1#None
			"
			api.Data.ClearCalculatedData($"A#90000171:F#None:O#Forms:U1#None", True, True, True, True)
			api.Data.Calculate(occupationAmountCalc, True)
		End Sub
		
		#End Region
		
		#End Region
		
		#End Region
		
	End Class
End Namespace