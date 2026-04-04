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

Namespace OneStream.BusinessRule.Finance.OTH_Calculates
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType					
						
					Case Is = FinanceFunctionType.CustomCalculate																	
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Factories_Recalculate") Then					
							Me.Factories_Recalculate(si, globals, api, args)	
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Supplier_Recalculate") Then							
							Me.Supplier_Recalculate(si, globals, api, args)								
	
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Expenses_Initialization") Then	
							Me.Expenses_Initialization(si, globals, api, args)								
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Expenses_Recalculate") Then	
							Me.Expenses_Recalculate(si, globals, api, args)		
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Expenses_Recalculate_Generic") Then							
							Me.Expenses_Recalculate_Generic(si, globals, api, args)	
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Expenses_Recalculate_Distribution") Then							
							Me.Expenses_Recalculate_Distribution(si, globals, api, args)								

						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Depreciation_Initialization") Then					
							Me.Depreciation_Initialization(si, globals, api, args)									
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Depreciation_Recalculate") Then					
							Me.Depreciation_Recalculate(si, globals, api, args)	
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Depreciation_Recalculate_Openings") Then					
							Me.Depreciation_Recalculate_Openings(si, globals, api, args)			
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Depreciation_Recalculate_Openings_Aux") Then					
							Me.Depreciation_Recalculate_Openings_Aux(si, globals, api, args)								
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Tax_Recalculate") Then					
							Me.Tax_Recalculate(si, globals, api, args)		
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Financial_2_Recalculate") Then					
							Me.Financial_2_Recalculate(si, globals, api, args)	
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Financial_3_Recalculate") Then					
							Me.Financial_3_Recalculate(si, globals, api, args)	
							
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Financial_3_Recalculate_Distribution") Then					
							Me.Financial_3_Recalculate_Distribution(si, globals, api, args)										

						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Financial_4_Recalculate") Then					
							Me.Financial_4_Recalculate(si, globals, api, args)								
														
							
						End If							
							
				End Select

				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		Public Function GetMappingBrandGenerics(ByVal sEntity As String) As String
		
			Dim sEntityInput As String = String.Empty
			
			Select Case sEntity
				Case "F001995": sEntityInput = "P_BK_Input"		
				Case "T002994": sEntityInput = "P_DP_Input"		
				Case "F001997": sEntityInput = "P_FH_Input"		
				Case "M002997": sEntityInput = "P_FHV_Input" 'Verificar con David
				Case "SF06D_05": sEntityInput = "P_FR_Input"			
				Case "SF17SBX": sEntityInput = "P_SB_Input"		
				Case "SF01D_01": sEntityInput = "P_VI_Aux_Input"		
				Case "SF01D_07": sEntityInput = "P_GI_Input"
				Case "SF24D_07": sEntityInput = "P_GI_PT_Input"	
				Case "SF24D_21": sEntityInput = "P_SB_PT_Input"			
				Case "SF22D_21": sEntityInput = "P_SB_PT_Input"		
				Case "SF313651": sEntityInput = "P_SB_BEL_Input"		
				Case "SF293690": sEntityInput = "P_SB_FR_Input"		
				Case "SF307911": sEntityInput = "P_SB_HO_Input"		
				Case "F001401": sEntityInput = "M_OB_Input"	
				Case "T002041": sEntityInput = "M_OBDP_Input"		
				Case "M002400": sEntityInput = "M_OBV_Input"		
				Case Else: sEntityInput = sEntity
						
			End Select			
		
			Return sEntityInput
			
		End Function		

		Sub Factories_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)		
			
			Dim sEntity As String = Api.Pov.Entity.Name
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)

			Dim sEntityInput As String = String.Empty
			Select Case sEntity
				Case "ZFDP9100": sEntityInput = "M_BSF_Input"
				Case "F001401": sEntityInput = "M_OB_Input"
				Case "M002400": sEntityInput = "M_OBV_Input"
				Case "T002004": sEntityInput = "U_OBDPR_Input"
				Case "T002041": sEntityInput = "U_OBDPC_Input"
			End Select
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				If sEntityInput <> String.Empty Then
					Api.Data.ClearCalculatedData($"E#{sEntity}",True,True,True,True)	
					Dim sCalc = $"E#{sEntity} = E#{sEntityInput}"
					'BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
					Api.Data.Calculate(sCalc,True)
				End If
				
			Else			

				'Copiado de todo el real en un paso aparte
				
'				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Forecast")
'				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual)")
'				Dim destDataBuffer As New DataBuffer
				
'				If Not sourceDataBuffer Is Nothing Then					
					
'					'Use ConvertDataBufferExtendedMembers when copying from one scenario to another scenario that has extended members
'					destDataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, "Actual", sourceDataBuffer)
'					api.Data.SetDataBuffer(destDataBuffer, destinationInfo,,,,,,,,,,,,,True)
					
'				End If
				
			End If
			
		End Sub

		Sub Supplier_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name	
			Dim sYear As String = sPeriod.Substring(0,4)	
			Dim iMonth As Integer = sPeriod.Substring(5)			
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)											
					
			Dim sEntity As String = Api.Pov.Entity.Name					
			Dim sEntityInput As String = Me.GetMappingBrandGenerics(sEntity)
			
			
			Dim sEntityActual As String = sEntityInput.Replace("_Input","")	
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then			
			
				If sEntityInput <> String.Empty Then
					Api.Data.ClearCalculatedData($"A#AL_INGPROV",True,True,True,True)	
					Dim sCalc = $"A#AL_INGPROV:F#None:E#{sEntity}:T#{sPeriod} = A#AL_INGPROV:F#None:E#{sEntityInput}:T#{sYear}M12 
																		* A#AL_INGPROV:F#Supplier_Mensualization:E#Entity_ND:T#{sPeriod}
																		/ 100"
					'BRApi.ErrorLog.LogMessage(si, $"sCalc = " & sCalc)
					Api.Data.Calculate(sCalc,True)
				End If
				
			Else				
				
				'Copiado de todo el real en un paso aparte
				
'				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Forecast")
'				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual)")
'				Dim destDataBuffer As New DataBuffer
				
'				If Not sourceDataBuffer Is Nothing Then					
					
'					'Use ConvertDataBufferExtendedMembers when copying from one scenario to another scenario that has extended members
'					destDataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, "Actual", sourceDataBuffer)
'					api.Data.SetDataBuffer(destDataBuffer, destinationInfo,,,,,,,,,,,,,True)
					
'				End If				
				
			End If
			
		End Sub
	
		Sub Expenses_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
				
			'Get all the parameters
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			
			Dim iCriteria As Integer = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#Entity_ND:U2#Input_Total:F#Criteria_OTH_Expenses:O#Forms:T#{sYear}M12").CellAmount)
			'BRApi.ErrorLog.LogMessage(si, "sCalc: " & $"A#{sAccount}:E#Entity_ND:F#Criteria_OTH_Expenses:O#Forms:T#{sYear}M12")
			'BRApi.ErrorLog.LogMessage(si, "sCriteria: " & iCriteria.ToString)
			If iCriteria.toString.Equals("1") Then 
			
				'Build a dictionary to send the parameters to a DM
				Dim customSubstVars As New Dictionary(Of String, String) From {
																			{"p_scenario", sScenario},																
																			{"p_year", sYear},																															
																			{"p_account", sAccount},																
																			{"p_forecast_month", iForecastMonth}
																		}
																		
				BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "OTH_Expenses_Recalculate_Gen", customSubstVars)
				
			Else If iCriteria.toString.Equals("2") Then 

				For Each mBrand As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#Entities_Aux_Own.Base", True)
				'For Each mBrand As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#P_BK_Input", True)										
					
					'Build a dictionary to send the parameters to a DM
					Dim customSubstVars As New Dictionary(Of String, String) From {
																			{"p_scenario", sScenario},																
																			{"p_year", sYear},																															
																			{"p_account", sAccount},
																			{"p_brand", mBrand.Member.Name.Replace("_Input","")},
																			{"p_brand_aux", mBrand.Member.Name},
																			{"p_forecast_month", iForecastMonth}
																		}				
				
					BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "OTH_Expenses_Recalculate_Dis", customSubstVars)		
			
				Next
				
			End If
			
		End Sub	
	
		Sub Expenses_Initialization(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name	
			Dim sYear As String = sPeriod.Substring(0,4)	
			Dim iMonth As Integer = sPeriod.Substring(5)						
			Dim iNumMonths As Integer = If (sScenario = "Budget", 12, 13 - iForecastMonth)
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)											
			

			
			Dim sEntity As String = Api.Pov.Entity.Name
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then			
			
				Api.Data.ClearCalculatedData($"A#{sAccount}",True,True,True,True)	
				Dim sCalc = $"A#{sAccount}:T#{sPeriod}:F#None:U2#None = A#{sAccount}:T#{sYear}M12:F#None:U2#Input_Total / {iNumMonths.toString()}"
				'BRApi.ErrorLog.LogMessage(si, $"sCalc = " & sCalc)
				Api.Data.Calculate(sCalc,True)																				
				
			End If
			
		End Sub		
				
		Sub Expenses_Recalculate_Generic(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name	
			Dim sYear As String = sPeriod.Substring(0,4)	
			Dim iMonth As Integer = sPeriod.Substring(5)						
			Dim iNumMonths As Integer = 13 - iForecastMonth
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)
			
			Dim sEntity As String = Api.Pov.Entity.Name					
			Dim sEntityInput As String = Me.GetMappingBrandGenerics(sEntity)
			
			BRApi.ErrorLog.LogMessage(si, "Entity: " & sEntity & " - Entity Input: " & sEntityInput)
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then			
			
				'Api.Data.ClearCalculatedData($"A#{sAccount}",True,True,True,True)	
				Dim sCalc = $"A#{sAccount}:F#None:E#{sEntity}:O#Forms = A#{sAccount}:F#None:E#{sEntityInput}:O#Top "
				'BRApi.ErrorLog.LogMessage(si, $"sCalc = " & sCalc)
				Api.Data.Calculate(sCalc,True)			
				
			Else
				
'				Api.Data.ClearCalculatedData($"A#{sAccount}",True,True,True,True)		
'				Dim sCalc = $"A#{sAccount}:E#{sEntity}:S#Forecast:O#Forms = A#{sAccount}:E#{sEntityActual}:S#Actual:O#Top:C#Aggregated"
'				'BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
'				Api.Data.Calculate(sCalc,True)							
				
			End If
			
		End Sub		

		Sub Expenses_Recalculate_Distribution(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim sBrand_Aux As String = args.CustomCalculateArgs.NameValuePairs("p_brand_aux")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name	
			Dim sYear As String = sPeriod.Substring(0,4)	
			Dim iMonth As Integer = sPeriod.Substring(5)						
			Dim iNumMonths As Integer = 13 - iForecastMonth
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)														
			
			Dim sEntity As String = Api.Pov.Entity.Name	
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then			
			
				Api.Data.ClearCalculatedData($"A#{sAccount}",True,True,True,True)	

'				Dim sCalc = $"A#{sAccount}:F#None:E#{sEntity}:T#{sPeriod}:U1#None:S#{sScenario}:O#Forms = 
'																	(A#{sAccount}:F#None:E#{sBrand_Aux}:T#{sPeriod}:U1#None:S#{sScenario}:O#Top)
'																	* A#AL_VENTAS:F#None:E#{sEntity}:T#{sYear}M1:U1#Top:S#Actual:O#Top:C#Aggregated
'																	/ A#AL_VENTAS:F#None:E#{sBrand}:T#{sYear}M1:U1#Top:S#Actual:O#Top:C#Aggregated"				
				
				Dim sCalc = $"A#{sAccount}:F#None:E#{sEntity}:T#{sPeriod}:U1#None:S#{sScenario}:O#Forms = 
																	(A#{sAccount}:F#None:E#{sBrand_Aux}:T#{sPeriod}:U1#None:S#{sScenario}:O#Top) 
																	* A#AL_VENTAS:F#None:E#{sEntity}:T#{sPeriod}:U1#Top:O#Top:C#Aggregated
																	/ A#AL_VENTAS:F#None:E#{sBrand}:T#{sPeriod}:U1#Top:O#Top:C#Aggregated"				
				'BRApi.ErrorLog.LogMessage(si, $"sCalc = " & sCalc)
				Api.Data.Calculate(sCalc,True)			
				
'			Else
				
'				Api.Data.ClearCalculatedData($"A#{sAccount}",True,True,True,True)		
'				Dim sCalc = $"A#{sAccount}:S#Forecast:O#Forms = A#{sAccount}:S#Actual:O#Top"
'				BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
'				Api.Data.Calculate(sCalc,True)							
				
			End If
			
		End Sub			
		
		Sub Depreciation_Initialization(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			
			Dim sActualYear As String = args.CustomCalculateArgs.NameValuePairs("p_actual_year")			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriodActualLastMonth As String = sActualYear & "M" & (iForecastMonth - 1).ToString()
			
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)			
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				Api.Data.ClearCalculatedData($"A#AL_AMORT",True,True,True,True)	
				Dim sCalc As String = $"A#AL_AMORT:F#Amount_Month:T#{sActualYear}M12:O#Forms = A#AL_AMORT:F#None:T#{sPeriodActualLastMonth}:O#Top:S#Actual"
				'BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
				Api.Data.Calculate(sCalc,True)													
				
			End If
			
		End Sub
		
		Sub Depreciation_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sActualYear As String = args.CustomCalculateArgs.NameValuePairs("p_actual_year")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim iActualLastMonth As Integer = (iForecastMonth - 1).ToString()
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)			
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				Api.Data.ClearCalculatedData($"A#AL_AMORT:F#None",True,True,True,True)	
				Dim sCalc = $"A#AL_AMORT:F#None:T#{sPeriod} = A#AL_AMORT:F#Amount_Month:T#{sActualYear}M12"
				'BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
				Api.Data.Calculate(sCalc,True)							
				
			End If
			
		End Sub					

		Sub Depreciation_Recalculate_Openings(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sActualYear As String = args.CustomCalculateArgs.NameValuePairs("p_actual_year")
			Dim iYear As String = CInt(args.CustomCalculateArgs.NameValuePairs("p_year"))
			
			Dim iForecastMonth As Integer = IIf(sScenario = "Budget",1,CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month")))
			Dim iActualLastMonth As Integer = (iForecastMonth - 1).ToString()
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)			
						
			Dim sEntities As String = String.Empty
			Dim sTimes As String = String.Empty
			Dim sEntityTime As String = String.Empty
			
			Dim dtOpenings As DataTable
		
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				Dim selectString As String = $"	SELECT C.cebe
													, C.brand
													, YEAR(ISNULL(O.opendate,C.open_date)) AS open_year
													, MONTH(ISNULL(O.opendate,C.open_date)) AS open_month
												FROM XFC_CEBES C
												LEFT JOIN XFC_OpenDate O
													ON C.cebe = O.cebe
				           	 					WHERE ((YEAR(ISNULL(O.opendate,C.open_date)) = {sActualYear} AND MONTH(ISNULL(O.opendate,C.open_date)) >= {iForecastMonth.toString})
            									OR YEAR(ISNULL(O.opendate,C.open_date)) > {sActualYear})
												AND C.unit_type IN ('Unidades Propias', 'Unidades ECI')"
				
				dtOpenings = BRApi.Database.ExecuteSql(dbConn, selectString, False)
				
				For Each dr As DataRow In dtOpenings.Rows					
					
					Dim sCebe As String = dr("cebe")
					Dim sBrand As String = dr("brand")
					Dim iOpenYear As Integer = dr("open_year")
					Dim iOpenMonth As Integer = dr("open_month")
					
					sEntities = sEntities & "E#" & sCebe & ","					
					
					For i As Integer = iForecastMonth To 12
						If (iOpenYear < iYear _
						Or (iOpenYear = iYear And i > iOpenMonth)) Then 
							sEntityTime = sEntityTime & sCebe & "-" & sBrand & "-" & iYear.ToString & "M" & i.ToString & "|"
						End If
						
					Next

'					For i As Integer = iForecastMonth To 12
'						If (iOpenYear < iYear Or i > iOpenMonth) Then 
'							sEntityTime = sEntityTime & sCebe & "-" & sBrand & "-" & iYear.ToString & "M" & i.ToString & "|"
'						End If
						
'					Next


				Next
				
				sEntities = sEntities.Substring(0, sEntities.Length - 1)
				sEntityTime = sEntityTime.Substring(0, sEntityTime.Length - 1)						
				
			End Using
			
			For i As Integer = iForecastMonth To 12
				sTimes = sTimes & "T#" & iYear.ToString & "M" & i.ToString & ","	
			Next
			sTimes = sTimes.Substring(0, sTimes.Length - 1)
							
'			BRApi.ErrorLog.LogMessage(si,"sEntityTime = " & sEntityTime)
'			BRApi.ErrorLog.LogMessage(si,"sTimes = " & sTimes)
'			BRApi.ErrorLog.LogMessage(si,"sEntities = " & sEntities)
			
			'Build a dictionary to send the parameters to a DM
			Dim customSubstVars As New Dictionary(Of String, String) From {
																			{"p_scenario", sScenario},																
																			{"p_year", iYear},	
																			{"p_actual_year", sActualYear},																			
																			{"p_forecast_month", iForecastMonth},
																			{"p_entities", sEntities},
																			{"p_times", sTimes},
																			{"p_entities_times", sEntityTime}
																		}
																		
			BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "OTH_Depreciation_Recalculate_Openings_Aux", customSubstVars)
			
		End Sub					

		Sub Depreciation_Recalculate_Openings_Aux(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)				
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sActualYear As String = args.CustomCalculateArgs.NameValuePairs("p_actual_year")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim iActualLastMonth As Integer = (iForecastMonth - 1).ToString()
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)			
			
			Dim sEntitiesTimes As String = args.CustomCalculateArgs.NameValuePairs("p_entities_times")
			'BRApi.ErrorLog.LogMessage(si,"sEntitiesTimes = " & sEntitiesTimes)
			
			Dim dtEntitiesTimes As New DataTable("EntitiesTimes")
       		dtEntitiesTimes.Columns.Add("cebe", GetType(String))
        	dtEntitiesTimes.Columns.Add("brand", GetType(String))
        	dtEntitiesTimes.Columns.Add("time", GetType(String))	
			
			For Each sEntityTime As String In sEntitiesTimes.Split("|")				
				dtEntitiesTimes.Rows.Add(sEntityTime.Split("-")(0), sEntityTime.Split("-")(1), sEntityTime.Split("-")(2))
			Next

			Dim sEntity As String = Api.Pov.Entity.Name
			Dim sBrandInput As String = String.Empty			
			
			Dim drs() As DataRow = dtEntitiesTimes.Select($"cebe = '{sEntity}' AND time = '{sPeriod}'")
			If drs.Length > 0 Then						

				Dim brandDict As New Dictionary(Of String, String) From {
																			{"Burger King", "P_BK_Input"},
																			{"Fridays", "P_FR_Input"},
																			{"Domino's Pizza", "P_DP_Input"},
																			{"Foster's Hollywood", "P_FH_Input"},
																			{"Foster's Hollywood Valencia", "P_FHV_Input"},
																			{"Starbucks", "P_SB_Input"},
																			{"Starbucks Portugal", "P_SB_PT_Input"},
																			{"Starbucks Bélgica", "P_SB_BEL_Input"},
																			{"Starbucks Francia", "P_SB_FR_Input"},
																			{"Starbucks Holanda", "P_SB_HO_Input"},
																			{"Ginos", "P_GI_Input"},
																			{"Ginos Portugal", "P_GI_PT_Input"},
																			{"VIPS", "P_VI_Aux_Input"}
																			}
																			
				sBrandInput = brandDict.Item(drs(0)("brand"))
				
				Api.Data.ClearCalculatedData($"A#AL_AMORT:F#None",True,True,True,True)	
				Dim sCalc = $"A#AL_AMORT:F#None:T#{sPeriod}:E#{sEntity} = A#AL_AMORT:F#Amount_Month:T#{sActualYear}M12:E#{sBrandInput}"
				'BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
				Api.Data.Calculate(sCalc,True)	
				
			End If								
			
		End Sub				
		
		Sub Tax_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)		
			
			Dim sEntity As String = Api.Pov.Entity.Name
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)

			Dim sEntityInput As String = String.Empty
			Select Case sEntity
				Case "F001090": sEntityInput = "ESP"
				Case "SF22090": sEntityInput = "PT"
				Case "SF29090": sEntityInput = "FR"
				Case "SF30090": sEntityInput = "HOL"
				Case "SF31090": sEntityInput = "BEL"
				Case "SF32090": sEntityInput = "LUX"					
			End Select
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				If sEntityInput <> String.Empty Then
					Api.Data.ClearCalculatedData($"E#{sEntity}",True,True,True,True)	
					Dim sCalc = $"E#{sEntity}:A#AL_IMP = E#{sEntityInput}:A#AL_IMP"
					BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
					Api.Data.Calculate(sCalc,True)
				End If
				
			Else			

				'Copiado de todo el real en un paso aparte
				
'				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Forecast")
'				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual)")
'				Dim destDataBuffer As New DataBuffer
				
'				If Not sourceDataBuffer Is Nothing Then					
					
'					'Use ConvertDataBufferExtendedMembers when copying from one scenario to another scenario that has extended members
'					destDataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, "Actual", sourceDataBuffer)
'					api.Data.SetDataBuffer(destDataBuffer, destinationInfo,,,,,,,,,,,,,True)
					
'				End If
				
			End If
			
		End Sub
	
		Sub Financial_2_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)		
			
			Dim sEntity As String = Api.Pov.Entity.Name
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)

			Dim sEntityInput As String = String.Empty
			Select Case sEntity
				Case "F001090": sEntityInput = "ESP"
				Case "SF22090": sEntityInput = "PT"
				Case "SF29090": sEntityInput = "FR"
				Case "SF30090": sEntityInput = "HOL"
				Case "SF31090": sEntityInput = "BEL"
				Case "SF32090": sEntityInput = "LUX"					
			End Select
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				If sEntityInput <> String.Empty Then
					Api.Data.ClearCalculatedData($"E#{sEntity}",True,True,True,True)	
					Dim sCalc = $"E#{sEntity}:A#{sAccount} = E#{sEntityInput}:A#{sAccount}"
					BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
					Api.Data.Calculate(sCalc,True)
				End If
				
			Else			

				'Copiado de todo el real en un paso aparte
				
'				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Forecast")
'				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual)")
'				Dim destDataBuffer As New DataBuffer
				
'				If Not sourceDataBuffer Is Nothing Then					
					
'					'Use ConvertDataBufferExtendedMembers when copying from one scenario to another scenario that has extended members
'					destDataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, "Actual", sourceDataBuffer)
'					api.Data.SetDataBuffer(destDataBuffer, destinationInfo,,,,,,,,,,,,,True)
					
'				End If
				
			End If
			
		End Sub

		Sub Financial_3_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
				
			'Get all the parameters
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))		

			For Each mBrand As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#Entities_Aux_Own.Base", True)
			'For Each mBrand As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#P_BK_Input", True)										
				
				'Build a dictionary to send the parameters to a DM
				Dim customSubstVars As New Dictionary(Of String, String) From {
																		{"p_scenario", sScenario},																
																		{"p_year", sYear},																															
																		{"p_account", sAccount},
																		{"p_brand", mBrand.Member.Name.Replace("_Input","")},
																		{"p_brand_aux", mBrand.Member.Name},
																		{"p_forecast_month", iForecastMonth}
																	}				
			
				BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "OTH_Financial_3_Recalculate_Dis", customSubstVars)		
		
			Next
			
		End Sub			
		
		Sub Financial_3_Recalculate_Distribution(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
				
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
			Dim sBrand_Aux As String = args.CustomCalculateArgs.NameValuePairs("p_brand_aux")
			
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name	
			Dim sYear As String = sPeriod.Substring(0,4)	
			Dim iMonth As Integer = sPeriod.Substring(5)						
			Dim iNumMonths As Integer = 13 - iForecastMonth
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)														
			
			Dim sEntity As String = Api.Pov.Entity.Name	
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then			
			
				Api.Data.ClearCalculatedData($"A#{sAccount}",True,True,True,True)	

				Dim sCalc = $"A#{sAccount}:F#None:E#{sEntity}:T#{sPeriod}:U1#None:S#{sScenario}:O#Forms = 
																	(A#{sAccount}:F#None:E#{sBrand_Aux}:T#{sPeriod}:U1#None:S#{sScenario}:O#Top)
																	* A#AL_VENTAS:F#None:E#{sEntity}:T#{sYear}M1:U1#Top:S#Actual:O#Top:C#Aggregated
																	/ A#AL_VENTAS:F#None:E#{sBrand}:T#{sYear}M1:U1#Top:S#Actual:O#Top:C#Aggregated"				
				
'				Dim sCalc = $"A#{sAccount}:F#None:E#{sEntity}:T#{sPeriod}:U1#None:S#{sScenario}:O#Forms = 
'																	(A#{sAccount}:F#None:E#{sBrand_Aux}:T#{sPeriod}:U1#None:S#{sScenario}:O#Top) 
'																	* A#AL_VENTAS:F#None:E#{sEntity}:T#{sPeriod}:U1#Top:O#Top
'																	/ A#AL_VENTAS:F#None:E#{sBrand}:T#{sPeriod}:U1#Top:O#Top:C#Aggregated"				
				BRApi.ErrorLog.LogMessage(si, $"sCalc = " & sCalc)
				Api.Data.Calculate(sCalc,True)			
				
'			Else
				
'				Api.Data.ClearCalculatedData($"A#{sAccount}",True,True,True,True)		
'				Dim sCalc = $"A#{sAccount}:S#Forecast:O#Forms = A#{sAccount}:S#Actual:O#Top"
'				BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
'				Api.Data.Calculate(sCalc,True)							
				
			End If
			
		End Sub
	
		Sub Financial_4_Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)		
		
			Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
			Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
			
			Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
			Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
			Dim sPeriod As String = Api.Pov.Time.Name			
			Dim iMonth As Integer = sPeriod.Substring(5)				
			
			'For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Account", sTag & sMember.Trim & ".Base", True)

			Dim sEntity As String = Api.Pov.Entity.Name					
			Dim sEntityInput As String = Me.GetMappingBrandGenerics(sEntity)
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				If sEntityInput <> String.Empty Then
					Api.Data.ClearCalculatedData($"E#{sEntity}",True,True,True,True)	
					Dim sCalc = $"E#{sEntity}:A#{sAccount} = E#{sEntityInput}:A#{sAccount}"
					BRApi.ErrorLog.LogMessage(si, "sCalc = " & sCalc)
					Api.Data.Calculate(sCalc,True)
				End If
				
			Else			

				'Copiado de todo el real en un paso aparte
				
'				Dim destinationInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#Forecast")
'				Dim sourceDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("FilterMembers(S#Actual)")
'				Dim destDataBuffer As New DataBuffer
				
'				If Not sourceDataBuffer Is Nothing Then					
					
'					'Use ConvertDataBufferExtendedMembers when copying from one scenario to another scenario that has extended members
'					destDataBuffer = api.Data.ConvertDataBufferExtendedMembers(api.Pov.Cube.Name, "Actual", sourceDataBuffer)
'					api.Data.SetDataBuffer(destDataBuffer, destinationInfo,,,,,,,,,,,,,True)
					
'				End If
				
			End If
			
		End Sub
	
			
	End Class
	
End Namespace