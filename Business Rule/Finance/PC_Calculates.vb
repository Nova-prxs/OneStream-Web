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

Namespace OneStream.BusinessRule.Finance.PC_Calculates
	
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
						Dim sCategory As String = args.CustomCalculateArgs.NameValuePairs("p_category")									
															
						If(sText1.Equals("Comparables") _
						Or sText1.Equals("Cerradas") _
						Or sText1.Equals("Reformas") _
						Or sText1.Equals("Reformas Año Anterior")) Then	
						
							If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate") Then
								Me.Recalculate_Costs(si, globals, api, args, sAccount, sCategory)
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SetBrandAdjustments") Then									
								Me.SetBrandAdjustments(si, globals, api, args, sAccount)	
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate_Hours") Then									
								Me.Recalculate_Hours(si, globals, api, args, sAccount, sCategory)	
								If (sCategory.Equals("Management") And Not sAccount.Equals("productive_hs_total"))
									Me.Recalculate_Hours(si, globals, api, args, "productive_hs_total", sCategory)
								Else If (Not sCategory.Equals("Management") And Not sAccount.Equals("total_hs"))
									Me.Recalculate_Hours(si, globals, api, args, "total_hs", sCategory)
								End If									
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate_Productivity") Then								
								Me.Recalculate_Productivity(si, globals, api, args, sAccount, sCategory)	
								sCategory = If (sCategory = "None", "Base", sCategory)								
								Me.Recalculate_Hours(si, globals, api, args, "productive_hs_total", sCategory)	
								Me.Recalculate_Hours(si, globals, api, args, "holidays", sCategory)
								Me.Recalculate_Hours(si, globals, api, args, "total_hs", sCategory)
								If (sCategory.Equals("Base"))
									Me.Recalculate_Costs(si, globals, api, args, "EB_SAL_FLX", sCategory)	
									Me.Recalculate_Costs(si, globals, api, args, "EB_SS", sCategory)	
								Else
									Me.Recalculate_Costs(si, globals, api, args, "RE_SAL_FLX", sCategory)	
									Me.Recalculate_Costs(si, globals, api, args, "RE_SS", sCategory)										
								End If
							Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate_HourCosts") Then	
								Me.Recalculate_HourCosts(si, globals, api, args, sAccount, sCategory)									
							End If
													
						End If
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate_Productivity_Brand") Then
							Me.Recalculate_Productivity_Brand(si, globals, api, args, sAccount, sCategory)
						End If
						
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
	
		

		
		Sub Recalculate_Costs(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sCategory As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")					
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = sYear
			Dim sScenarioSource As String = sScenario
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
				sScenarioSource = "Forecast"
			Else
				sScenarioSource = "Actual"
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = IIf(sAccount.Contains("_SAL_"),sAccount.Replace("_FLX",""),sAccount)

			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
										
				' 1. INITIAL			

                Dim sCalcAvg As String = String.Empty
				
				Dim target As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms"
				
				Dim numerator As String = "( "
				Dim denominator As String = "( "
				
				' Dictionary for avoiding multiples calls to the cube  
				Dim dataCache As New Dictionary(Of String, Double)
				
				'  Flag for deciding if there is a 1.00 in A#{sAccount}, it means if there is any month selected
				Dim monthselected As Boolean = False
				
				' Check if there is a 1.00 in A#{sAccount}
				For mes = 1 To 12
				    Dim key As String = $"S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top"
				
				    ' Save the value in the dictionary  
				    If Not dataCache.ContainsKey(key) Then
				        Dim cell = api.Data.GetDataCell(key)
				        dataCache(key) = If(cell IsNot Nothing, cell.CellAmount, 0)
				    End If
				
				    ' If a 1.00 is found, flag will be actived 
				    If dataCache(key) = 1.00 Then
				        monthselected = True
				    End If
				Next
				
				' Decide the account in all the months
				Dim accountSelected As String
				If monthselected Then
				    accountSelected = $"A#{sAccount}" 
				Else
				    accountSelected = "A#Default_Avarage_PC" 
				End If
				
				' Build the formula with the selected account
				For mes = 1 To 12
				    Dim suffixMes As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#{sAccountActual}:F#None:O#Top"
				    Dim suffixOrigen As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top"
				    Dim suffixdenominator As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top" 
				
				    If mes < 12 Then
				        numerator &= $"S#{sScenarioSource}:{suffixMes} * S#{sScenario}:{suffixOrigen} + "
				        denominator &= $"S#{sScenario}:{suffixdenominator} + "
				    Else
				        numerator &= $"S#{sScenarioSource}:{suffixMes} * S#{sScenario}:{suffixOrigen} )"
				        denominator &= $"S#{sScenario}:{suffixdenominator} )"
				    End If
				Next    			
				
				' Final Formula
				sCalcAvg = $"{target} = {numerator} / {denominator}"
				
				Dim sCalcSameMonthAA As String
				If (sScenario = "Budget")

					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms =	
				
									S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:O#Top"		
				Else
			
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms =	
				
									S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccountActual}:F#None:O#Top"
				End If	
				
				Dim sCalcInput As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 0"	
								
				Dim sCalcSalary As String = $"	S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#None =
												S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#HourCosts:F#PC_Import_Hours:O#Top:U2#{sCategory}
												* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#hs_for_HourCosts:F#PC_Import_Hours:O#Top:U2#{sCategory}"	
								
				Dim sCalcIT As String = $"	S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#None =
											(S#Actual:T#{sActualYear}:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None
											/
											S#Actual:T#{sActualYear}:E#{sEntity}:A#IT_long_term_leave:F#None:O#Top:U2#{sCategory} )
											*
											S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#IT_long_term_leave:F#None:O#Top:U2#{sCategory} "
								
'				Dim sCalcSS As String = $"	S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#None =
				
'											( (S#{sScenarioSource}:T#{sActualYear}M1:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M2:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M3:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M4:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M5:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M6:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M7:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M8:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M9:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M10:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M11:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ (S#{sScenarioSource}:T#{sActualYear}M12:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)	)			
								
'											/
				
'											( ((S#{sScenarioSource}:T#{sActualYear}M1:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M1:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M2:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M2:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M3:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M3:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M4:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M4:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M5:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M5:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M6:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M6:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M7:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M7:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M8:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M8:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M9:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M9:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M10:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M10:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M11:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M11:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None)
'											+ ((S#{sScenarioSource}:T#{sActualYear}M12:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory} + S#{sScenarioSource}:T#{sActualYear}M12:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}) * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#None) )
																						
'											*
'											S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#total_hs:F#None:O#Top:U2#{sCategory}"		
				

                Dim sCalcSS As String = String.Empty
				Dim targetSS As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U2#None:O#Forms"
				
				Dim numeratorSS As String = "( "
				Dim denominatorSS As String = "( "

				
				' Build the formula with the selected account
				For mes = 1 To 12
				    Dim suffixMesSS As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#{sAccount}:U2#None:F#None:O#Top"
				    Dim suffixOrigenSS As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:U2#None:O#Top"
					Dim suffixSS As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}" 
				    Dim suffixdenominatorSS As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}" 
					Dim suffixdenominatorSS2 As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top:U2#None" 
				
				    If mes < 12 Then
				        numeratorSS &= $"S#{sScenarioSource}:{suffixMesSS} * S#{sScenario}:{suffixOrigenSS} + "
				        denominatorSS &= $"(S#{sScenarioSource}:{suffixSS} + S#{sScenarioSource}:{suffixdenominatorSS}) * S#{sScenario}:{suffixdenominatorSS2} +  "
				    Else
				        numeratorSS &= $"S#{sScenarioSource}:{suffixMesSS} * S#{sScenario}:{suffixOrigenSS} )"
				        denominatorSS &= $"(S#{sScenarioSource}:{suffixSS} + S#{sScenarioSource}:{suffixdenominatorSS})* S#{sScenario}:{suffixdenominatorSS2} )"
				    End If
				Next    
				

				' Final Formula
				sCalcSS = $"{targetSS} = {numeratorSS} / {denominatorSS} * S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#total_hs:F#None:U2#{sCategory}:O#Top"
'				BRApi.ErrorLog.LogMessage(si, "sCalcSS: " & sCalcSS)
				
				Dim sCalcBON As String = $"	S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#None =
											(S#Actual:T#{sActualYear}:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#None
											/
											S#Actual:T#{sActualYear}:E#{sEntity}:A#IT_long_term_leave:F#None:O#Top:U2#{sCategory})
											*
											S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#IT_long_term_leave:F#None:O#Top:U2#{sCategory}"
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcAvg: " & sCalcAvg)
					
				Dim sCalc1 As String = sCalcAvg
				Dim iCriteria As Integer = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria_PC:T#{sActualYear}M12:O#Top").CellAmount)
				'BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
				Select Case iCriteria.toString
					Case "1"  
						sCalc1 = sCalcAvg	
					Case "2"
						sCalc1 = sCalcSameMonthAA
					Case "3"
						sCalc1 = String.Empty
					Case "4"
						sCalc1 = sCalcSalary
					Case "5"
						sCalc1 = sCalcIT
					Case "6"
						sCalc1 = sCalcSS
					Case "7"
						sCalc1 = sCalcBON
				End Select
				
				'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Initial_Result:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc1,True)							
				
				'2. FINAL
				
				Dim sCalc2 As String = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top"	
				
				Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Variation_Input:O#Top").CellAmount
				Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Variation_Input:O#Top").CellAmount
				
				Dim customCellAmount2 As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Initial_Result:O#Top").CellAmount									
				
				If customCellAmount2 < -1000000 Then 					
					api.Data.Calculate($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = -1000000",True)  
				End If					
				
		
				If dPerc_Variation <> 0 And dAmount_Variation <> 0 Then 	
					sCalc2 = $"A#{sAccount}:F#None:O#Forms = (A#{sAccount}:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")}))+ ({dAmount_Variation.ToString().Replace(",",".")})"
			    ElseIf dPerc_Variation <> 0 Then 
						sCalc2 = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
				ElseIf dAmount_Variation <> 0 Then 
					If customCellAmount2 <> 0 Then
						sCalc2 = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top + {dAmount_Variation.ToString().Replace(",",".")}"	
					Else
						sCalc2 = $"A#{sAccount}:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {dAmount_Variation.ToString().Replace(",",".")}"	
					End If
				End If				
						

				'BRApi.ErrorLog.LogMessage(si, "Calc2: " & sCalc2)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc2,True)		
			
'			Else							

'				Dim sCalcAccount As String = $"A#{sAccount}:F#None = A#{sAccountActual}:F#None:S#Actual"
'				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None",True,True,True,True)	
'				'BRApi.ErrorLog.LogMessage(si, "sCalcAccount: " & sCalcAccount)	
'				If Not isClosed Then Api.Data.Calculate(sCalcAccount,True)							
				
			End If

		End Sub

		Sub Recalculate_Hours(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sCategory As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")							
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))		
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = sYear
			Dim sScenarioSource As String = sScenario
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
				sScenarioSource = "Forecast"
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount

			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
										
				' 1. INITIAL			
                Dim sCalcAvg As String = String.Empty
				Dim target As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}"
				
				Dim numerator As String = "( "
				Dim denominator As String = "( "
				
				' Dictionary for avoiding multiples calls to the cube  
				Dim dataCache As New Dictionary(Of String, Double)
				
				'  Flag for deciding if there is a 1.00 in A#{sAccount}, it means if there is any month selected
				Dim monthselected As Boolean = False
				
				' Check if there is a 1.00 in A#{sAccount}
				For mes = 1 To 12
				    Dim key As String = $"S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#{sCategory}"
				
				    ' Save the value in the dictionary  
				    If Not dataCache.ContainsKey(key) Then
				        Dim cell = api.Data.GetDataCell(key)
				        dataCache(key) = If(cell IsNot Nothing, cell.CellAmount, 0)
				    End If
				
				    ' If a 1.00 is found, flag will be actived 
				    If dataCache(key) = 1.00 Then
				        monthselected = True
				    End If
				Next
				
				' Decide the account in all the months
				Dim accountSelected As String
				If monthselected Then
				    accountSelected = $"A#{sAccount}:U2#{sCategory}" 
				Else
				    accountSelected = "A#Default_Avarage_PC:U2#None" 
				End If
				
				' Build the formula with the selected account
				For mes = 1 To 12
				    Dim suffixMes As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#{sAccount}:F#PC_Import_Hours:O#Top:U2#{sCategory}"
				    Dim suffixOrigen As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top"
				    Dim suffixdenominator As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top" 
				
				    If mes < 12 Then
				        numerator &= $"S#{sScenarioSource}:{suffixMes} * S#{sScenario}:{suffixOrigen} + "
				        denominator &= $"S#{sScenario}:{suffixdenominator} + "
				    Else
				        numerator &= $"S#{sScenarioSource}:{suffixMes} * S#{sScenario}:{suffixOrigen} )"
				        denominator &= $"S#{sScenario}:{suffixdenominator} )"
				    End If
				Next    
				
				
				' Final Formula
				sCalcAvg = $"{target} = {numerator} / {denominator}"

                Dim sCalcAvgTotalHs As String = String.Empty
				Dim targetHs As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U2#{sCategory}:O#Forms"
				
				Dim numeratorHs As String = "( "
				Dim denominatorHs As String = "( "

				
				' Build the formula with the selected account
				For mes = 1 To 12
				    Dim suffixMesHs As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}"
					Dim suffixMesNoHs As String= $"T#{sActualYear}M{mes}:E#{sEntity}:A#non_productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory}"
				    Dim suffixOrigenHs As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top"
				    Dim suffixdenominatorHs As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top" 

				
				    If mes < 12 Then
				        numeratorHs &= $"(S#{sScenarioSource}:{suffixMesHs} + S#{sScenarioSource}:{suffixMesNoHs}) * S#{sScenario}:{suffixOrigenHs} + "
				        denominatorHs &= $"S#{sScenarioSource}:{suffixdenominatorHs} + "
				    Else
				        numeratorHs &= $"(S#{sScenarioSource}:{suffixMesHs} + S#{sScenarioSource}:{suffixMesNoHs}) * S#{sScenario}:{suffixOrigenHs} ) "
				        denominatorHs &= $"S#{sScenarioSource}:{suffixdenominatorHs} ) "
				    End If
				Next    
				
				
				' Final Formula
				sCalcAvgTotalHs = $"{targetHs} = {numeratorHs} / {denominatorHs} "
				
				
				Dim sCalcSameMonthAA As String
				Dim sCalcSameMonthAAManagement As String
				
				If (sScenario = "Budget")
					
					'Calc only for holidays
					sCalcSameMonthAAManagement = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory} =	
				
									S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#PC_Import_Hours:O#Top:U2#{sCategory}"						
					
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory} =	
				
									Divide(S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#PC_Import_Hours:O#Top:U2#{sCategory}
									, S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory})
					
									* S#Budget:T#{sYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#None:O#Top:U2#{sCategory}"							
				Else
						
					'Calc only for holidays
					sCalcSameMonthAAManagement = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory} =	
				
									S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#PC_Import_Hours:O#Top:U2#{sCategory}"						
					
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory} =	
				
									Divide(S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#PC_Import_Hours:O#Top:U2#{sCategory}
									, S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:O#Top:U2#{sCategory})
					
									* S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#None:O#Top:U2#{sCategory}"									
				End If	
				
				
				'Productive Hs						
				Dim sFormulaProductiveHsManagement As String = $"A#productive_hs_total:F#Amount_Initial_Result:O#Forms:U2#{sCategory} = 
									A#total_hs:F#None:O#Top:U2#{sCategory} - A#non_productive_hs_total:F#None:O#Top:U2#{sCategory}"					
				
				Dim sFormulaProductiveHsDistributors As String = $"A#productive_hs_total:F#Amount_Initial_Result:U1#None:O#Forms:U2#{sCategory} = 
									Divide(A#customers:F#None:O#Top:U1#Delivery:U2#None, A#productivity:F#None:O#Top:U1#Top:U2#{sCategory})"
				
				Dim sFormulaProductiveHsBase As String = $"A#productive_hs_total:F#Amount_Initial_Result:U1#None:O#Forms:U2#{sCategory} = 
									(Divide(A#customers:F#None:O#Top:U1#Top:U2#None, A#productivity:F#None:O#Top:U1#Top:U2#None))
									- A#productive_hs_total:F#None:U1#None:O#Forms:U2#Management
									- A#productive_hs_total:F#None:U1#None:O#Forms:U2#Distributors"

				Dim sFormulaTotalHsDistributorsBase As String = $"A#total_hs:F#Amount_Initial_Result:O#Forms:U2#{sCategory} = 
									A#productive_hs_total:F#None:O#Top:U2#{sCategory} + A#non_productive_hs_total:F#None:O#Top:U2#{sCategory}"	
								
				
				'BRApi.ErrorLog.LogMessage(si, "sCalcSameMonthAA: " & sCalcSameMonthAA)
					
				Dim sCalc1 As String = String.empty
				Dim iCriteria As Integer = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria_PC_Hours:T#{sActualYear}M12:O#Top:U2#{sCategory}").CellAmount)
				'BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
				Select Case iCriteria.toString
					Case "1"  
						If (sCategory.Equals("Management") And sAccount.Equals("total_hs"))
							BRApi.ErrorLog.LogMessage(si, "sCalcAvgTotalHs: " & sCalcAvgTotalHs)
							sCalc1 = sCalcAvgTotalHs	
						Else
							sCalc1 = sCalcAvg	
						End If
					Case "2"
						If (sCategory.Equals("Management"))
							sCalc1 = sCalcSameMonthAAManagement
						Else
							sCalc1 = sCalcSameMonthAA
						End If
					Case "3"
						If (sCategory.Equals("Management") And sAccount.Equals("productive_hs_total"))
							sCalc1 = sFormulaProductiveHsManagement
						Else If (sCategory.Equals("Distributors") And sAccount.Equals("productive_hs_total"))
							sCalc1 = sFormulaProductiveHsDistributors
						Else If (sCategory.Equals("Base") And sAccount.Equals("productive_hs_total"))
							sCalc1 = sFormulaProductiveHsBase		
						Else If (sCategory.Equals("Top") And sAccount.Equals("productive_hs_total"))
							sCalc1 = sFormulaProductiveHsBase														
						Else If (sCategory.Equals("Distributors") And sAccount.Equals("total_hs"))
							sCalc1 = sFormulaTotalHsDistributorsBase	
						Else If (sCategory.Equals("Base") And sAccount.Equals("total_hs"))
							sCalc1 = sFormulaTotalHsDistributorsBase							
						End If
				End Select
				
'				If (sEntity = "SF172403" And sPeriod = "2025M12") Then
'					BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
'					BRApi.ErrorLog.LogMessage(si, "sAccount: " & sAccount)
'					BRApi.ErrorLog.LogMessage(si, "sFormulaCriteria: " & $"A#{sAccount}:E#{sBrand}_Input:F#Criteria_PC_Hours:T#{sActualYear}M12:O#Top:U2#{sCategory}")
'					BRApi.ErrorLog.LogMessage(si, "sCalc1: " & sCalc1)	
'				End If
				
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc1,True)	
				
				'2. FINAL
				
				Dim sCalc2 As String = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory}"	
				
				Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Variation_Input:O#Top:U2#{sCategory}").CellAmount
				Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Variation_Input:O#Top:U2#{sCategory}").CellAmount
				
				If Not dPerc_Variation = 0 Then 
					sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
				Else If Not dAmount_Variation = 0 Then 
					Dim dAmount_Initial As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory}").CellAmount
					If dAmount_Initial = 0 Then 
						sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory}:F#None:U1#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None:O#None:IC#None = {dAmount_Variation.ToString().Replace(",",".")}"	
					Else
						sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} + {dAmount_Variation.ToString().Replace(",",".")}"	
					End If
				End If			
				
				'BRApi.ErrorLog.LogMessage(si, "Calc2: " & sCalc2)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U2#{sCategory}",True,True,True,True)			
				Api.Data.Calculate(sCalc2,True)
									
			Else							
				
				Dim sCalcAccount As String = $"A#{sAccount}:F#None:U2#{sCategory} = A#{sAccountActual}:F#Import_Hours:S#Actual:U2#{sCategory}"
				If sAccount.Equals("total_hs") Then
					sCalcAccount = $"A#{sAccount}:F#None:U2#{sCategory} = A#productive_hs_total:F#None:S#Actual:U2#{sCategory} + A#non_productive_hs_total:F#None:S#Actual:U2#{sCategory}"
				End If
				
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U2#{sCategory}",True,True,True,True)
				Api.Data.Calculate(sCalcAccount,True)						
				
			End If				

		End Sub
	
		Sub Recalculate_Productivity(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sCategory As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")							
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))		
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = sYear
			Dim sScenarioSource As String = sScenario
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
				sScenarioSource = "Forecast"
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			Dim sChannel As String = IIf(sCategory.Equals("Distributors"),"Delivery","Top")
			Dim sCategoryTop As String = IIf(sCategory.Equals("None"),"Top",sCategory)
			Dim sCategoryTopProd As String = IIf(sCategory.Equals("None"),"None",sCategory)
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)			

			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
										
				' 1. INITIAL			
													
				Dim sCalcSameMonthAA As String
				If (sScenario = "Budget")
					
					If (iMonth >= iForecastMonth) Then
						sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms:U2#{sCategory} =	
				
									(S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:O#Top:U2#{sCategoryTopProd})+ Divide(S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Customers:F#None:U1#{sChannel}:O#Top:U2#None
									, S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:U1#Top:O#Top:U2#{sCategoryTop})"	
					Else
						sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms:U2#{sCategory} =	
				
									Divide(S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Customers:F#None:U1#{sChannel}:O#Top:U2#None
									, S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:U1#Top:O#Top:U2#{sCategoryTop})"													
						
					End If
					
				Else
											
					sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms:U2#{sCategory} =	
				
									Divide(S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Customers:F#None:U1#{sChannel}:O#Top:U2#None
									, S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#PC_Import_Hours:U1#Top:O#Top:U2#{sCategoryTop})"											
					
				End If									
					
				Dim sCalc1 As String = String.empty
				Dim iCriteria As Integer = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria_PC_Productivity:T#{sActualYear}M12:O#Top:U2#{sCategory}").CellAmount)
				'BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
				Select Case iCriteria.toString
					Case "1"  
						sCalc1 = sCalcSameMonthAA				
				End Select
				
'				If (sEntity = "SF172405") Then
'					BRApi.ErrorLog.LogMessage(si, sCalc1)
'				End If
				
				'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
'				If (sEntity = "SF016322" And sPeriod = "2025M12")
'					BRApi.ErrorLog.LogMessage(si, "sClear: " & $"A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}")
'				End If				
				
				Api.Data.Calculate($"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Import:U2#{sCategory} =
									S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Import:U2#{sCategory}
									- S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Import:U2#{sCategory}",True)
									
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Initial_Result:U2#{sCategory}",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc1,True)							
				
				'2. FINAL
				
				Dim sCalc2 As String = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory}"	
				
				Dim dPerc_Variation_Brand As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Variation_Input:O#Top:U2#{sCategory}:E#{sBrand}_Input").CellAmount
				Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Variation_Input:O#Top:U2#{sCategory}").CellAmount
				Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Variation_Input:O#Top:U2#{sCategory}").CellAmount				
					
				If dPerc_Variation_Brand <> 0 And dPerc_Variation <> 0 And dAmount_Variation <> 0 Then
				    '  Brand Variation + Unit Percentage variation + Absolute Variation
				     sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = ((A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation_Brand / 100).ToString().Replace(",", ".")})) * (1 + {(dPerc_Variation / 100).ToString().Replace(",", ".")})) + ({dAmount_Variation.ToString().Replace(",", ".")})"
				
				ElseIf dPerc_Variation_Brand <> 0 And dPerc_Variation <> 0 Then
				    ' Brand Variation + Unit Percentage variation
				    sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = (A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation_Brand / 100).ToString().Replace(",", ".")})) * (1 + {(dPerc_Variation / 100).ToString().Replace(",", ".")})"
				
				ElseIf dPerc_Variation_Brand <> 0 And dAmount_Variation <> 0 Then
				    ' Brand Variation + Absolute Variation
				    sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = (A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation_Brand / 100).ToString().Replace(",", ".")})) + ({dAmount_Variation.ToString().Replace(",", ".")})"
				
				ElseIf dPerc_Variation <> 0 And dAmount_Variation <> 0 Then
				    '  Unit Percentage variation + Absolute Variation
				    sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = (A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation / 100).ToString().Replace(",", ".")})) + ({dAmount_Variation.ToString().Replace(",", ".")})"
				
				ElseIf dPerc_Variation_Brand <> 0 Then
				    ' Only Brand Variation 
				    sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation_Brand / 100).ToString().Replace(",", ".")})"
				
				ElseIf dPerc_Variation <> 0 Then
				    ' Only Unit Percentage variation
				    sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation / 100).ToString().Replace(",", ".")})"
				
				ElseIf dAmount_Variation <> 0 Then
					' Only Absolute Variation
					Dim customCellAmount As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory}").CellAmount
					If (customCellAmount = 0) Then	
				    	sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory}:IC#None:U1#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {dAmount_Variation.ToString().Replace(",", ".")}"
					Else
				    	sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} + {dAmount_Variation.ToString().Replace(",", ".")}"
					End If						
				    
				
				End If
				
				If sEntity = "M002698" Then
					BRApi.ErrorLog.LogMessage(si, "scalc2: " & sCalc2)
				End If
				
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U2#{sCategory}",True,True,True,True)			
				Api.Data.Calculate(sCalc2,True)	
									
			Else							
				
				Dim sCalcAccount As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms:U2#{sCategory} =	
				
									Divide(S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Customers:F#None:U1#Delivery:O#Top:U2#None
									, S#Actual:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#productive_hs_total:F#None:U1#Top:O#Top:U2#{sCategory})"

				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U2#{sCategory}",True,True,True,True)	
				'BRApi.ErrorLog.LogMessage(si, "sCalcAccount: " & sCalcAccount)	
				Api.Data.Calculate(sCalcAccount,True)							
				
			End If			

		End Sub
		
		Sub Recalculate_Productivity_Brand(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sCategory As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")							
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = sYear
			Dim sScenarioSource As String = sScenario
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
				sScenarioSource = "Forecast"
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			Dim sChannel As String = IIf(sCategory.Equals("Distributors"),"Delivery","Top")
			Dim sCategoryTop As String = IIf(sCategory.Equals("None"),"Top",sCategory)
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount
										
			' 1. FINAL		
			
'			BRApi.ErrorLog.LogMessage(si, "sEntity: " & sEntity)
'			BRApi.ErrorLog.LogMessage(si, "sPeriod: " & sPeriod)
												
			Dim sCalc1 As String = $"E#{sEntity}:A#{sAccount}:F#None:O#Forms:U1#None:U2#{sCategory} =	
			
									Divide(
										E#{sBrand}:C#Aggregated:A#Customers:F#None:U1#{sChannel}:O#Top:U2#None,
										E#{sBrand}:C#Aggregated:A#productive_hs_total:F#None:U1#Top:O#Top:U2#{sCategoryTop}
									)"
		
			
'			BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)
			Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:O#Forms:U2#{sCategory}",True,True,True,True)
			Api.Data.Calculate(sCalc1,True)

		End Sub		
		
		Sub Recalculate_HourCosts(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sCategory As String)
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")							
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = sYear
			Dim sScenarioSource As String = sScenario
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
				sScenarioSource = "Forecast"
			Else
				sScenarioSource = "Actual"
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			Dim sCuentaSalario As String = IIf(sCategory.Equals("Base"),"EB_SAL","RE_SAL")
			
			'BRApi.ErrorLog.LogMessage(si, "sAccounts: " & sAccount)
			Dim sAccountActual As String = sAccount

			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
										
				' 1. INITIAL			
							
                Dim sCalcAvg As String = String.Empty
				Dim target As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}"
				
				Dim numerator As String = "( "
				Dim denominator As String = "( "
				
				' Dictionary for avoiding multiples calls to the cube  
				Dim dataCache As New Dictionary(Of String, Double)
				
				'  Flag for deciding if there is a 1.00 in A#{sAccount}, it means if there is any month selected
				Dim monthselected As Boolean = False
				
				' Check if there is a 1.00 in A#{sAccount}
				For mes = 1 To 12
				    Dim key As String = $"S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top:U2#{sCategory}"
				
				    ' Save the value in the dictionary  
				    If Not dataCache.ContainsKey(key) Then
				        Dim cell = api.Data.GetDataCell(key)
				        dataCache(key) = If(cell IsNot Nothing, cell.CellAmount, 0)
				    End If
				
				    ' If a 1.00 is found, flag will be actived 
				    If dataCache(key) = 1.00 Then
				        monthselected = True
				    End If
				Next
				
				' Decide the account in all the months
				Dim accountSelected As String
				If monthselected Then
				    accountSelected = $"A#{sAccount}:U2#{sCategory}" 
				Else
				    accountSelected = "A#Default_Avarage_PC:U2#None" 
				End If
				
				' Build the formula with the selected account
				For mes = 1 To 12
				    Dim suffixMes As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#{sCuentaSalario}:F#None:O#Top:U2#None"
				    Dim suffixOrigen As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top"
					Dim suffixAvg As String= $"T#{sActualYear}M{mes}:E#{sEntity}:A#hs_for_HourCosts:F#PC_Import_Hours:O#Top:U2#{sCategory}"
				    Dim suffixdenominator As String = $"T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top" 
				
				    If mes < 12 Then
				        numerator &= $"S#{sScenarioSource}:{suffixMes} * S#{sScenario}:{suffixOrigen} + "
				        denominator &= $"S#{sScenarioSource}:{suffixAvg} * S#{sScenario}:{suffixdenominator} + "
				    Else
				        numerator &= $"S#{sScenarioSource}:{suffixMes} * S#{sScenario}:{suffixOrigen} )"
				        denominator &= $"S#{sScenarioSource}:{suffixAvg} * S#{sScenario}:{suffixdenominator} )"
				    End If
				Next    
				
				
				' Final Formula
				sCalcAvg = $"{target} = {numerator} / {denominator}"
'                BRApi.ErrorLog.LogMessage(si, "sCalcAvg: " & sCalcAvg)	
 
				Dim sCalc1 As String = String.empty
				Dim iCriteria As Integer = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria_PC_HourCosts:T#{sActualYear}M12:O#Top:U2#{sCategory}").CellAmount)
				'BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
				Select Case iCriteria.toString
					Case "1"  
						sCalc1 = sCalcAvg				
				End Select
				
				'BRApi.ErrorLog.LogMessage(si, "Calc1: " & sCalc1)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}",True,True,True,True)			
				Api.Data.Calculate(sCalc1,True)							
				
				'2. FINAL
				
				Dim sCalc2 As String = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory}"	
								
				Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Variation_Input:O#Top:U2#{sCategory}").CellAmount
				Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Variation_Input:O#Top:U2#{sCategory}").CellAmount
		
				Dim customCellAmount2 As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}").CellAmount									
				
				If customCellAmount2 < -100 Then 
					'BRApi.ErrorLog.LogMessage(si, "sCalc: " & $"A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}:I#None:U1#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = -1000000")
					api.Data.Calculate($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory}:I#None:U1#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = -100",True)  
				End If					
				
				If Not dPerc_Variation = 0 Then 
					sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
				Else If Not dAmount_Variation = 0 Then 
					Dim customCellAmount As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory}").CellAmount
					'BRApi.ErrorLog.LogMessage (si, "Amount: " & customCellAmount.ToString())
					If (customCellAmount = 0) Then
						sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory}:F#None:U1#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None:O#None:IC#None = {dAmount_Variation.ToString().Replace(",",".")}"	
					Else
						sCalc2 = $"A#{sAccount}:F#None:O#Forms:U2#{sCategory} = A#{sAccount}:F#Amount_Initial_Result:O#Top:U2#{sCategory} + {dAmount_Variation.ToString().Replace(",",".")}"	
					End If
					
				End If			
				
				'BRApi.ErrorLog.LogMessage(si, "Calc2: " & sCalc2)		
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:O#Forms:U2#{sCategory}",True,True,True,True)			
				Api.Data.Calculate(sCalc2,True)		
									
			Else							
				
				Dim sCalcAccount As String = $"A#{sAccount}:F#Amount_Initial_Result:O#Forms:U2#{sCategory} =	
				
									  Divide(A#RE_SAL:F#None:O#Top:U2#None, A#hs_for_HourCosts:F#None:O#Top:U2#{sCategory})"	

				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:U2#{sCategory}",True,True,True,True)	
				'BRApi.ErrorLog.LogMessage(si, "sCalcAccount: " & sCalcAccount)	
				Api.Data.Calculate(sCalcAccount,True)							
				
			End If			

		End Sub
		
		Sub SetBrandAdjustments(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String)
			'Get brand	
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			'Set calculate formulas to copy adjustments from brand to unit, clear data and calculate
			Dim percFormula As String = $"A#{sAccount}:F#Perc_Variation_Input:O#Forms = E#{sBrand}_Input:A#{sAccount}:F#Perc_Variation_Input:O#Forms"
			Dim amountFormula As String = $"A#{sAccount}:F#Amount_Variation_Input:O#Forms = E#{sBrand}_Input:A#{sAccount}:F#Amount_Variation_Input:O#Forms"
			Api.Data.ClearCalculatedData($"A#{sAccount}:F#Perc_Variation_Input:O#Forms",True,True,True,True)			
			Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Variation_Input:O#Forms",True,True,True,True)			
			Api.Data.Calculate(percFormula,True)
			Api.Data.Calculate(amountFormula,True)
		End Sub
					
		
	End Class
	
End Namespace