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

Namespace OneStream.BusinessRule.Finance.COS_Calculates
	
	Public Class MainClass
		
		'Reference business rule to get functions
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType					
						
					Case Is = FinanceFunctionType.CustomCalculate			
							
						Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
						Dim iTime As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, sYear)
						Dim sText1 As String = BRApi.Finance.Entity.Text(si, Api.Pov.Entity.MemberId,2,0,iTime)
						
						Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
						Dim sChannel As String = args.CustomCalculateArgs.NameValuePairs("p_channel")
															
						If(sText1.Equals("Comparables") _
						Or sText1.Equals("Cerradas") _
						Or sText1.Equals("Reformas") _
						Or sText1.Equals("Reformas Año Anterior")) Then	
						
							If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate") Then
								
								If sAccount.Equals("Dif_RT") Then
									Me.RecalculateDifRT(si, globals, api, args, sAccount, sChannel)
								Else
									Me.Recalculate(si, globals, api, args, sAccount, sChannel)
								End If
								
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Initialize") Then
								
								If sAccount.Equals("Dif_RT") Then
									'Me.InitializeDifRT_CopyActual(si, globals, api, args, sAccount, sChannel)
									Me.InitializeDifRT(si, globals, api, args, sAccount, sChannel)
									Me.RecalculateDifRT(si, globals, api, args, sAccount, sChannel)
								Else
									Me.Initialize(si, globals, api, args, sAccount, sChannel)
									Me.Recalculate(si, globals, api, args, sAccount, sChannel)
								End If							
								
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyBrandToUnit") Then
								
								Me.CopyBrandToUnit(si, globals, api, args, sChannel)
								If sAccount.Equals("Dif_RT") Then
									Me.RecalculateDifRT(si, globals, api, args, sAccount, sChannel)
								Else
									Me.Recalculate(si, globals, api, args, sAccount, sChannel)
								End If
								
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RecalculateAdjustments") Then
								
								Me.RecalculateAdjustments(si, globals, api, args, sChannel)	
								
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CopyActualToForecast") Then
								
								Me.CopyActualToForecast(si, globals, api, args, sChannel)										
								
							End If
							
						End If
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

		Sub Initialize(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sChannel As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
			Else
				sActualYear = sYear
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
			Dim sChannelSales As String = "Top"
			Dim sChangeSign As String = "-1"
			
			Select Case sAccount 
				Case "Theoretical_Costs"
					sChannelSales = sChannel				
				Case "Merchandising"
					sAccountActual = "AL_CV_MER"
				Case "Packaging"
					sAccountActual = "AL_CV_CPA"
				Case "Condiments"
					sAccountActual = "AL_CV_CON"								
			End Select					
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				' 1. INITIAL			
				Dim sCalcAvg As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
									( S#Actual:T#{sActualYear}M1:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M2:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M3:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M4:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M5:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M6:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M7:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top			
									+ S#Actual:T#{sActualYear}M8:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M9:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M10:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M11:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M12:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top )
									/
									( S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M2:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M3:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M4:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M5:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M6:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M7:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top			
									+ S#Actual:T#{sActualYear}M8:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M9:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M10:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M11:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M12:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top )
				
									* 100"											
				
				Dim sCalcSameMonthAA As String
				If (sScenario = "Budget") Then
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
									S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top
									/ S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top
									* 100"
				Else
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
									S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccountActual}:F#None:U1#{sChannel}:O#Top
									/ S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top
									* 100"
				End If
				
				'Create input formula
				'Get brand percentage input
				Dim brandPerc As String = Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Perc_Generic_Input:T#{sPeriod}:U1#{sChannel}:O#Top").CellAmount.ToString.Replace(",", ".")
				
				'Get actual sales to filter out empty restaurants
				Dim actualSales As Decimal = Api.Data.GetDataCell($"S#Actual:A#Sales:E#{sEntity}:V#YTD:F#None:T#{sActualYear}M{iForecastMonth}:U1#{sChannel}:O#Top").CellAmount
				
				'Build input formula
				Dim sCalcInput As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms:I#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
												{brandPerc}"
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcSameMonthAA: " & sCalcSameMonthAA)
					
				Dim sCalc1 As String = sCalcAvg
				
				Dim iCriteria As Integer

'				If sScenario = "Forecast" Then
				    iCriteria = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria:T#{sActualYear}M12:U1#{sChannel}:O#Top").CellAmount)
				    
'				ElseIf sScenario = "Budget" Then
'				    iCriteria= CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria:T#{sYear}M12:U1#{sChannel}:O#Top").CellAmount)
			
'				End If
				
				'BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
				If iCriteria.toString.Equals("1") Then 
					sCalc1 = sCalcAvg		
				Else If iCriteria.toString.Equals("2") Then 
					sCalc1 = sCalcSameMonthAA
				Else If iCriteria.ToString.Equals("3") Or actualSales > 0 Then
					sCalc1 = sCalcInput
				End If
				
				'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc1,True)										
			
			Else
				
				Me.CopyActualToForecast(si, globals, api, args, sChannel)					
				
			End If

		End Sub
	
		Sub InitializeDifRT(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sChannel As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
			Else
				sActualYear = sYear
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
			Dim sChannelSales As String = "Top"
			Dim sChangeSign As String = "-1"
			
			Dim iAplica As Integer = IIf(sBrand.Equals("P_DP"),0,1)			
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				' 1. INITIAL			

				'Quitamos de la fórmula: AL_CV_CPR y AL_CV_MER, y añadimos a la fórmula Adjustments
				Dim sCalcAvg As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
									( (S#Actual:T#{sActualYear}M1:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M1:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M1:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M2:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M2:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M2:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M2:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M2:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M2:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M2:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M2:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M3:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M3:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M3:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M3:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M3:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M3:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M3:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M3:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M4:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M4:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M4:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M4:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M4:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M4:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M4:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M4:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M5:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M5:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M5:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M5:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M5:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M5:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M5:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M5:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M6:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M6:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M6:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M6:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M6:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M6:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M6:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M6:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M7:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M7:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M7:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M7:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M7:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M7:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M7:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M7:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top			
									+ (S#Actual:T#{sActualYear}M8:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M8:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M8:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M8:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M8:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M8:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M8:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M8:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M9:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M9:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M9:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M9:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M9:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M9:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M9:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M9:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M10:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M10:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M10:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M10:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M10:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M10:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M10:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M10:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M11:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M11:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M11:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M11:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M11:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M11:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M11:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M11:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ (S#Actual:T#{sActualYear}M12:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M12:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top - (S#Actual:T#{sActualYear}M12:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top * {iAplica.toString()}) - S#Actual:T#{sActualYear}M12:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M12:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M12:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M12:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top - S#Actual:T#{sActualYear}M12:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top )
									/
									( S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M2:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M3:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M4:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M5:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M6:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M7:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top			
									+ S#Actual:T#{sActualYear}M8:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M9:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M10:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M11:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top
									+ S#Actual:T#{sActualYear}M12:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top )
				
									* 100"			
				
				'Test con 1 mes
'				Dim sCalcAvg As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
'									( (S#Actual:T#{sActualYear}M1:E#{sEntity}:A#AL_CVTAS:F#None:U1#{sChannel}:O#Top 
'									- S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top 
'									- S#Actual:T#{sActualYear}M1:E#{sEntity}:A#AL_CV_CPA:F#None:U1#{sChannel}:O#Top 
'									- S#Actual:T#{sActualYear}M1:E#{sEntity}:A#AL_CV_CON:F#None:U1#{sChannel}:O#Top 
'									- S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Personnel_cost:F#None:U1#{sChannel}:O#Top 
'									- S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Waste:F#None:U1#{sChannel}:O#Top 
'									- S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Inventory_Waste:F#None:U1#{sChannel}:O#Top 
'									- S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Adjustments:F#None:U1#{sChannel}:O#Top) * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top)
'									/
'									( S#Actual:T#{sActualYear}M1:E#{sEntity}:A#Sales:F#None:U1#{sChannelSales}:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#{sChannel}:O#Top)
				
'									* -100"		
								
				
				
'				Dim sCalcSameMonthAA As String
'				If (iMonth < iForecastMonth)
										
'					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
'									( S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#AL_CVTAS:F#None:U1#Top:O#Top
'									- S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top
'									- S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#AL_CV_CPA:F#None:U1#Top:O#Top
'									- S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#AL_CV_CON:F#None:U1#Top:O#Top
'									- S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Personnel_cost:F#None:U1#Top:O#Top
'									- S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Waste:F#None:U1#Top:O#Top
'									- S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Inventory_Waste:F#None:U1#Top:O#Top
'									- S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Adjustments:F#None:U1#Top:O#Top)
					
'									/ S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top
'									* -100"
'				Else
'					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
'									S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Dif_RT:F#Perc_Final_Result:U1#Top:O#Top"
'				End If	
				
				Dim sCalcSameMonthAA As String
				If (sScenario = "Budget") Then
				
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
									S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Dif_RT:F#Perc_Final_Result:U1#Top:O#Top"
				Else
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms =	
				
									( S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#AL_CVTAS:F#None:U1#Top:O#Top
									- S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Theoretical_Costs:F#None:U1#Top:O#Top
									- (S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#AL_CV_CPA:F#None:U1#Top:O#Top * {iAplica.toString()})
									- S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#AL_CV_CON:F#None:U1#Top:O#Top
									- S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Personnel_cost:F#None:U1#Top:O#Top
									- S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Waste:F#None:U1#Top:O#Top
									- S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Inventory_Waste:F#None:U1#Top:O#Top
									- S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Adjustments:F#None:U1#Top:O#Top)
					
									/ S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top
									* 100"
				End If	
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcSameMonthAA: " & sCalcSameMonthAA)
					
				'Create input formula
				'Get brand percentage input
				Dim brandPerc As String = Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Perc_Generic_Input:T#{sPeriod}:U1#{sChannel}:O#Top").CellAmount.ToString.Replace(",", ".")
				
				'Get actual sales to filter out empty restaurants
				Dim actualSales As Decimal = Api.Data.GetDataCell($"S#Actual:A#Sales:E#{sEntity}:V#YTD:F#None:T#{sActualYear}M{iForecastMonth}:U1#Top:O#Top").CellAmount							
				
				'Build input formula
				Dim sCalcInput As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms:I#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
												{brandPerc}"
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcSameMonthAA: " & sCalcSameMonthAA)
					
				Dim sCalc1 As String = sCalcAvg	
				Dim iCriteria As Integer

'				If sScenario = "Forecast" Then
				    iCriteria = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria:T#{sActualYear}M12:U1#{sChannel}:O#Top").CellAmount)
'				ElseIf sScenario = "Budget" Then
'				    iCriteria= CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria:T#{sYear}M12:U1#{sChannel}:O#Top").CellAmount)
			
'				End If
				
				'BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
				
				If iCriteria.toString.Equals("1") Then 
					sCalc1 = sCalcAvg		
				Else If iCriteria.toString.Equals("2") Then 
					sCalc1 = sCalcSameMonthAA
				Else If iCriteria.ToString.Equals("3") Or actualSales > 0 Then
					sCalc1 = sCalcInput
				End If				
				
'				BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				Api.Data.Calculate(sCalc1,True)											
			
			Else
				
				Me.CopyActualToForecast(si, globals, api, args, sChannel)						
				
			End If

		End Sub		
	
		Sub InitializeDifRT_CopyActual(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sChannel As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
			Else
				sActualYear = sYear
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
			Dim sChannelSales As String = "Top"
			Dim sChangeSign As String = "-1"
			
'			Dim iAplica As Integer = IIf(sBrand.Equals("P_DP"),0,1)			
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
															
			
			Else
				
				Me.CopyActualToForecast(si, globals, api, args, sChannel)						
				
			End If

		End Sub		

		
		Public Sub Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sChannel As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
			Else
				sActualYear = sYear
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
			Dim sAccountPL As String = String.Empty
			Dim sChannelSales As String = "Top"
			Dim sChangeSign As String = "-1"
			
			Select Case sAccount 
				Case "Theoretical_Costs"
					sChannelSales = sChannel	
					sAccountPL = "AL_CV_CCB"
				Case "Condiments"
					sAccountActual = "AL_CV_CON"
					sAccountPL = "AL_CV_CON"	
				Case "Packaging"
					sAccountActual = "AL_CV_CPA"	
					sAccountPL = "AL_CV_CPA"
				Case "Personnel_Cost"
					sAccountPL = "AL_CV_CPE"					
				Case "Merchandising"
					sAccountActual = "AL_CV_MER"
					sAccountPL = "AL_CV_MER"
				Case "Waste"
					sAccountPL = "AL_CV_DES"
				Case "Inventory_Waste"
					sAccountPL = "AL_CV_DES"		
				Case "Adjustments"
					sAccountPL = "Adjustments"											
					
			End Select										
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then			
							
				'2. RAW MATERIAL
				
				Dim sPerc_RawMaterial_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_RawMaterial_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")
				If sPerc_RawMaterial_Input.Equals("") Then 
					sPerc_RawMaterial_Input = "0" 
				End If
					
				'BRApi.ErrorLog.LogMessage(si, "sPerc_RawMaterial_Input: " & sPerc_RawMaterial_Input)
				Dim sCalc2 As String = $"A#{sAccount}:F#Perc_RawMaterial_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top 
									* (1 + Divide({sPerc_RawMaterial_Input},100)) "
		
				
				'BRApi.ErrorLog.LogMessage(si, "Calc2: " & sCalc2)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_RawMaterial_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc2,True)		
				
				'3. PRICE INCREASE
				
				Dim sPerc_IncPriceExi_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_IncPriceExi_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")			
				If sPerc_IncPriceExi_Input.Equals("") Then 
					sPerc_IncPriceExi_Input = "0" 
				End If			
				Dim sPerc_IncPriceNew_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_IncPriceNew_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")			
				If sPerc_IncPriceNew_Input.Equals("") Then 
					sPerc_IncPriceNew_Input = "0" 
				End If				
				
				Dim sCalc3 As String = $"A#{sAccount}:F#Perc_IncPrice_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_RawMaterial_Result:U1#{sChannel}:O#Top
									/ (1 + Divide({sPerc_IncPriceExi_Input},100) 
										 + Divide({sPerc_IncPriceNew_Input},100)) "					
				
				'BRApi.ErrorLog.LogMessage(si, "Calc3: " & sCalc3)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_IncPrice_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc3,True)				
				
				'4. INNOVATION/PROMOTION
				
				Dim sPerc_Innovation_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Innovation_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")			
				If sPerc_Innovation_Input.Equals("") Then 
					sPerc_Innovation_Input = "0" 
				End If			
				Dim sPerc_Promotion_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Promotion_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")
				If sPerc_Promotion_Input.Equals("") Then 
					sPerc_Promotion_Input = "0" 
				End If				
				
				Dim sCalc4 As String = $"A#{sAccount}:F#Perc_InnovPromo_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_IncPrice_Result:U1#{sChannel}:O#Top + {sPerc_Innovation_Input} + {sPerc_Promotion_Input} "					
				
				'BRApi.ErrorLog.LogMessage(si, "Calc4: " & sCalc4)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_InnovPromo_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc4,True)		
				
				'5. FINAL
				
				Dim sPerc_Generic_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Generic_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")
				If sPerc_Generic_Input.Equals("") Then 
					sPerc_Generic_Input = "0" 
				End If			
				
				Dim sCalc5 As String = $"A#{sAccount}:F#Perc_Final_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_InnovPromo_Result:U1#{sChannel}:O#Top + {sPerc_Generic_Input} "					
				
				'BRApi.ErrorLog.LogMessage(si, "Calc5: " & sCalc5)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_Final_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc5,True)		
				
				Dim sCalcAmount As String = $"A#{sAccount}:F#None:U1#{sChannel}:O#Forms =
				
									A#{sAccount}:F#Perc_Final_Result:U1#{sChannel}:O#Top
									* A#Sales:F#None:U1#{sChannelSales}:O#Top
				
									/ 100"
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcAmount: " & sCalcAmount)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U1#{sChannel}:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalcAmount,True)	
				
				'6. P&L
				Dim sCalcPL As String 
				If sAccount.Equals("Waste") Or sAccount.Equals("Inventory_Waste")
					sCalcPL = $"A#{sAccountPL}:F#None:U1#None:O#Forms = A#Waste:F#None:U1#Top:O#Forms + A#Inventory_Waste:F#None:U1#Top:O#Forms"
				Else
					sCalcPL = $"A#{sAccountPL}:F#None:U1#None:O#Forms = A#{sAccount}:F#None:U1#Top:O#Forms"
				End If
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcPL: " & sCalcPL)		
				Api.Data.ClearCalculatedData($"A#{sAccountPL}:F#None:U1#None:O#Forms",True,True,True,True)
				Api.Data.Calculate(sCalcPL,True)	
				
			End If

		End Sub
		
		Sub RecalculateDifRT(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sChannel As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
			Else
				sActualYear = sYear
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
			Dim sChannelSales As String = "Top"
			Dim sChangeSign As String = "-1"						
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then			
							
				'2. RAW MATERIAL
				
				Dim sPerc_RawMaterial_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_RawMaterial_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")
				If sPerc_RawMaterial_Input.Equals("") Then 
					sPerc_RawMaterial_Input = "0" 
				End If
					
				'BRApi.ErrorLog.LogMessage(si, "sPerc_RawMaterial_Input: " & sPerc_RawMaterial_Input)
				Dim sCalc2 As String = $"A#{sAccount}:F#Perc_RawMaterial_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top 
									* (1 + Divide({sPerc_RawMaterial_Input},100)) "
		
				
				'BRApi.ErrorLog.LogMessage(si, "Calc2: " & sCalc2)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_RawMaterial_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				Api.Data.Calculate(sCalc2,True)		
				
				'3. PRICE INCREASE
				
				Dim sPerc_IncPriceExi_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_IncPriceExi_Input:U1#None:O#Top").CellAmount.ToString().Replace(",",".")			
				If sPerc_IncPriceExi_Input.Equals("") Then 
					sPerc_IncPriceExi_Input = "0" 
				End If			
				Dim sPerc_IncPriceNew_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_IncPriceNew_Input:U1#None:O#Top").CellAmount.ToString().Replace(",",".")			
				If sPerc_IncPriceNew_Input.Equals("") Then 
					sPerc_IncPriceNew_Input = "0" 
				End If				
				
				Dim sCalc3 As String = $"A#{sAccount}:F#Perc_IncPrice_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_RawMaterial_Result:U1#{sChannel}:O#Top
									/ (1 + Divide({sPerc_IncPriceExi_Input},100) 
										 + Divide({sPerc_IncPriceNew_Input},100)) "					
				
				'BRApi.ErrorLog.LogMessage(si, "Calc3: " & sCalc3)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_IncPrice_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				Api.Data.Calculate(sCalc3,True)				
				
				'4. INNOVATION/PROMOTION
				
				Dim sPerc_Innovation_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Innovation_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")			
				If sPerc_Innovation_Input.Equals("") Then 
					sPerc_Innovation_Input = "0" 
				End If			
				Dim sPerc_Promotion_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Promotion_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")
				If sPerc_Promotion_Input.Equals("") Then 
					sPerc_Promotion_Input = "0" 
				End If				
				
				Dim sCalc4 As String = $"A#{sAccount}:F#Perc_InnovPromo_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_IncPrice_Result:U1#{sChannel}:O#Top + {sPerc_Innovation_Input} + {sPerc_Promotion_Input} "					
				
				'BRApi.ErrorLog.LogMessage(si, "Calc4: " & sCalc4)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_InnovPromo_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				Api.Data.Calculate(sCalc4,True)		
				
				'5. FINAL
				
				Dim sPerc_Generic_Input As String = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Generic_Input:U1#{sChannel}:O#Top").CellAmount.ToString().Replace(",",".")
				If sPerc_Generic_Input.Equals("") Then 
					sPerc_Generic_Input = "0" 
				End If			
				
				Dim sCalc5 As String = $"A#{sAccount}:F#Perc_Final_Result:U1#{sChannel}:O#Forms =	
				
									A#{sAccount}:F#Perc_InnovPromo_Result:U1#{sChannel}:O#Top + {sPerc_Generic_Input} "					
				
				'BRApi.ErrorLog.LogMessage(si, "Calc5: " & sCalc5)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_Final_Result:U1#{sChannel}:O#Forms",True,True,True,True)			
				Api.Data.Calculate(sCalc5,True)		
				
				Dim sCalcAmount As String = $"A#{sAccount}:F#None:U1#{sChannel}:O#Forms =
				
									A#{sAccount}:F#Perc_Final_Result:U1#{sChannel}:O#Top
									* A#Sales:F#None:U1#{sChannelSales}:O#Top
				
									/ 100"
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcAmount: " & sCalcAmount)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U1#{sChannel}:O#Forms",True,True,True,True)			
				Api.Data.Calculate(sCalcAmount,True)		
				
				'6. P&L
				Dim sCalcPL As String  = $"A#AL_CV_AJU:F#None:U1#None:O#Forms = A#Dif_RT:F#None:U1#Top:O#Forms + A#Adjustments:F#None:U1#Top:O#Forms"
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcPL: " & sCalcPL)		
				Api.Data.ClearCalculatedData($"A#AL_CV_AJU:F#None:U1#None:O#Forms",True,True,True,True)
				Api.Data.Calculate(sCalcPL,True)				
				
			End If

		End Sub		
	
		Sub RecalculateAdjustments(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sChannel As String)
	
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
			Else
				sActualYear = sYear
			End If
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
			Dim sChannelSales As String = "Top"
			
			Select Case sAccount 
				Case "Theoretical_Costs"
					sChannelSales = sChannel
					sAccountActual = "AL_CV_CPR"
				Case "Merchandising"
					sAccountActual = "AL_CV_MER"
				Case "Packaging"
					sAccountActual = "AL_CV_CPA"
				Case "Condiments"
					sAccountActual = "AL_CV_CON"	
				Case "Adjustments"
					sAccountActual = "Adjustments"					
			End Select					
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
		
'				Dim sCalcAmount As String = $"A#{sAccount}:F#None:U1#None:O#Forms =
					
'										A#{sAccount}:F#Perc_Final_Result:U1#None:O#Top
'										* A#Sales:F#None:U1#Top:O#Top
					
'										/ 100"
					
'				'BRApi.ErrorLog.LogMessage(si, "sCalcAmount: " & sCalcAmount)		
'				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U1#None:O#Forms",True,True,True,True)			
'				Api.Data.Calculate(sCalcAmount,True)

				'6. P&L
				Dim sCalcPL As String  = $"A#AL_CV_AJU:F#None:U1#None:O#Forms = A#Dif_RT:F#None:U1#Top:O#Forms + A#Adjustments:F#None:U1#Top:O#Forms"
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcPL: " & sCalcPL)		
				Api.Data.ClearCalculatedData($"A#AL_CV_AJU:F#None:U1#None:O#Forms",True,True,True,True)
				Api.Data.Calculate(sCalcPL,True)	
				
			Else														
								
				Dim sCalcAccount As String = $"A#{sAccount}:F#None = A#{sAccountActual}:F#None:S#Actual"
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None",True,True,True,True)	
				'BRApi.ErrorLog.LogMessage(si, "sCalcAccount: " & sCalcAccount)	
				If Not isClosed Then Api.Data.Calculate(sCalcAccount,True)							
				
			End If				

		End Sub				
		
		Sub CopyActualToForecast(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sChannel As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))			
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
																												
			Dim slAccounts As New List(Of String) From {"Cost_of_Sales_Contable","Theoretical_Costs","Condiments","Adjustments","Packaging","Personnel_Cost","Merchandising","Waste","Inventory_Waste"}
			Dim sAccountActual As String
			
			'Copy Actual to Forecast All Accounts
			For Each sAcc As String In slAccounts
									
				Select Case sAcc 						
					Case "Cost_of_Sales_Contable"
						sAccountActual = "AL_CVTAS"								
					Case "Merchandising"
						sAccountActual = "AL_CV_MER"
					Case "Packaging"
						sAccountActual = "AL_CV_CPA"
					Case "Condiments"
						sAccountActual = "AL_CV_CON"			
					Case Else
						sAccountActual = sAcc	
				End Select					
						
				Dim sCalcAccount As String = $"A#{sAcc}:F#None = A#{sAccountActual}:F#None:S#Actual"
				Api.Data.ClearCalculatedData($"A#{sAcc}:F#None",True,True,True,True)	
				'BRApi.ErrorLog.LogMessage(si, "sCalcAccount: " & sCalcAccount)					
				Api.Data.Calculate(sCalcAccount,True)		
				
				Next
				
			Dim sCalcDifRT As String = $"A#Dif_RT:F#None:U1#None = A#AL_CVTAS:F#None:U1#Top - A#Cost_of_Sales_Aux:F#None:U1#Top + A#Merchandising:F#None:U1#Top"
			Api.Data.ClearCalculatedData($"A#Dif_RT:F#None",True,True,True,True)	
			'BRApi.ErrorLog.LogMessage(si, "sCalcDifRT: " & sCalcDifRT)	
			Api.Data.Calculate(sCalcDifRT,True)		
			
			Dim sCalcPercFinal As String = $"A#Dif_RT:F#Perc_Final_Result:U1#None:O#None = A#Dif_RT:F#None:U1#None:O#Top / A#Sales:F#None:U1#Top:O#Top * 100"					
			Api.Data.ClearCalculatedData($"A#Dif_RT:F#Perc_Final_Result",True,True,True,True)	
			'BRApi.ErrorLog.LogMessage(si, "sCalcPercFinal: " & sCalcPercFinal)	
			Api.Data.Calculate(sCalcPercFinal,True)					

		End Sub		
		
		Sub CopyBrandToUnit(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sChannel As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim sFlow As String = args.CustomCalculateArgs.NameValuePairs("p_flow")
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name	
			
			Dim sCalc1 As String = String.Empty
			Dim sCalc2 As String = String.Empty
			
			'BRApi.ErrorLog.LogMessage(si, "sFlow: " & sFlow)		
			
			Select Case sFlow
				
				Case "RawMaterial"
						
					sCalc1 = $"E#{sEntity}:A#{sAccount}:F#Perc_RawMaterial_Input:U1#{sChannel}:O#Forms = 
							   RemoveZeros(E#{sBrand}_Input:A#{sAccount}:F#Perc_RawMaterial_Input:U1#None:O#Forms) 
					
							* E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top
							/ E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top"					
			
					'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_RawMaterial_Input:U1#{sChannel}:O#Forms",True,True,True,True)			
					Api.Data.Calculate(sCalc1,True)		
					
				Case "PriceIncrease"
					
					sCalc1 = $"E#{sEntity}:A#{sAccount}:F#Perc_IncPriceExi_Input:U1#{sChannel}:O#Forms = 
							   E#{sBrand}_Input:A#{sAccount}:F#Perc_IncPriceExi_Input:T#{sYear}:U1#{sChannel}:O#Top 
					- E#{sBrand}_Input:A#{sAccount}:V#YTD:F#Perc_IncPriceExi_Input:U1#{sChannel}:O#Top"
																		
			
					'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_IncPriceExi_Input:U1#{sChannel}:O#Forms",True,True,True,True)	
					Api.Data.Calculate(sCalc1,True)		
					
					sCalc2 = $"E#{sEntity}:A#{sAccount}:F#Perc_IncPriceNew_Input:U1#{sChannel}:O#Forms= 
					           E#{sBrand}_Input:A#{sAccount}:V#YTD:F#Perc_IncPriceNew_Input:U1#{sChannel}:O#Top
					        * (E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top
							/ E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top)"
							   

'					BRApi.ErrorLog.LogMessage(si, "Calc2: " & sCalc2)		
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_IncPriceNew_Input:U1#{sChannel}:O#Forms",True,True,True,True)			
					Api.Data.Calculate(sCalc2,True)					
					
				Case "InnovationPromotion"
					
					sCalc1 = $"E#{sEntity}:A#{sAccount}:F#Perc_Innovation_Input:U1#{sChannel}:O#Forms = 
							   E#{sBrand}_Input:A#{sAccount}:F#Perc_Innovation_Input:U1#{sChannel}:O#Forms
					
							* E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top
							/ E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top"						
			
					'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_Innovation_Input:U1#{sChannel}:O#Forms",True,True,True,True)			
					Api.Data.Calculate(sCalc1,True)		
					
					sCalc2 = $"E#{sEntity}:A#{sAccount}:F#Perc_Promotion_Input:U1#{sChannel}:O#Forms = 
							   E#{sBrand}_Input:A#{sAccount}:F#Perc_Promotion_Input:U1#{sChannel}:O#Forms
					
							* E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top
							/ E#{sEntity}:A#{sAccount}:F#Perc_Initial_Result:U1#{sChannel}:O#Top"						
			
					'BRApi.ErrorLog.LogMessage(si, "Calc2: " & sCalc2)		
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_Promotion_Input:U1#{sChannel}:O#Forms",True,True,True,True)			
					Api.Data.Calculate(sCalc2,True)						
					
			End Select				
		
		End Sub
			
	End Class
	
End Namespace