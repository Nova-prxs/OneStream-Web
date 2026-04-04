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

Namespace OneStream.BusinessRule.Finance.GC_Calculates
	Public Class MainClass
		
		'Reference business rule to get functions
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
								
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate
						
						Dim brandAccountsFilter As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "GC_prm_Accounts_Brand_Filter")
						Dim occupancyAccountsFilter As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "GC_prm_Accounts_Occupancy_Filter")
						
						#Region "Brand Get Criteria"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BrandGetCriteria") Then
							
							'Get all the account members for general costs and loop through them
							Dim accountDict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Accounts", brandAccountsFilter)
							Dim occupancyAccountDict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Accounts", occupancyAccountsFilter)
							
							For Each account As KeyValuePair(Of String, MemberInfo) In accountDict
							
								'Get account name and declare variables for criteria and if cpi is applied
								Dim accountName As String = account.Key
								Dim accountCriteria As Integer
								Dim cpiApplied As Integer = 0
								
								'Skip some account names
								If accountName <> "(Bypass)" AndAlso accountName <> "~"
									
									'Define Criteria and if CPI is applied based on the account name
									Select Case accountName

										Case "AL_OI_APO", "GG_SUM_ELE", "GG_SUM_GAS"
											accountCriteria = 1
										Case "AL_OI_OT", "GG_SUM_ELE", "GG_SUM_GAS", "GG_OPS_CAR", "AL_OC_REN", "AL_OC_COM", "AL_OC_OTR", "AL_OC_SUB", "GG_EXT_CC"
											accountCriteria = 2
											cpiApplied = 1
										Case "AL_PUB_INV", "AL_PUB_OT", "GG_OPS_CON", "GG_OPS_REC", "GG_EXT_MOT", "GG_EXT_MOT", "GG_MANT_CO", "62300019", "62300022"
											accountCriteria = 3
										Case "AL_PUB_INV", "AL_PUB_OT", "AL_GG_VIA", "GG_SUM_AGU", "GG_OPS_CON", "GG_OPS_REC", "GG_OPS_LIM", "GG_OPS_UNI", "GG_EXT_SPI", "GG_EXT_LIM", "GG_EXT_MOT", "GG_EXT_MOT", "GG_EXT_REG", "GG_EXT_TTE", "GG_EXT_AUD", "GG_EXT_ABO", "GG_EXT_ARC", "GG_EXT_MEN", "GG_EXT_OTR", "GG_MANT_PR", "GG_MANT_CO", "GG_MANT_MO", "GG_VEH_REN", "GG_VEH_PEA", "GG_VEH_GAS", "62500001", "62500400", "GG_OTR_TRI", "GG_OTR_OFI", "GG_OTR_FOR", "GG_OTR_TES", "GG_OTR_REN", "GG_OTR_COM", "GG_OTR_INC", "GG_OTR_REP", "GG_OTR_VAR", "GG_EXT_LAV", "62300024"
											accountCriteria = 4
											cpiApplied = 1
										Case "GG_IT_TEL", "GG_IT_HIL", "GG_IT_MOB", "GG_IT_INT", "GG_IT_SER", "GG_IT_REP", "GG_IT_LIC", "62900015", "62300055", "62300080", "62300090", "GG_EXT_PAG"
											accountCriteria = 5
										Case "AL_ROY", "AL_PUB_FM", "AL_PUB_LSM"
											accountCriteria = 6
										Case "62300020", "62300023"
											accountCriteria = 7
										Case "AL_OC_REN"
											accountCriteria = 8
											cpiApplied = 1
										Case "AL_OC_REN"
											accountCriteria = 9
											cpiApplied = 1
										Case "AL_OI_ROY"
											accountCriteria = 10
											
									End Select
									
									'Input variables in the cube
									Api.Data.Calculate($"A#{accountName}:F#Criteria_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {accountCriteria.ToString}",True)
									Api.Data.Calculate($"A#{accountName}:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {cpiApplied.ToString}",True)
									
								End If
							
							Next
							
						#End Region
						
						#Region "Brand Initialize"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("BrandInitialize") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim costType As String = args.CustomCalculateArgs.NameValuePairs("p_costtype")
							
							'Get all the account members for general costs and loop through them
							Dim accountDict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Accounts", brandAccountsFilter)
							Dim iTime As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, sYear)
							Dim sText1 As String = BRApi.Finance.Entity.Text(si, Api.Pov.Entity.MemberId,2,0,iTime)
								
							If(sText1.Equals("Comparables") _
							Or sText1.Equals("Cerradas") _
							Or sText1.Equals("Reformas") _
							Or sText1.Equals("Reformas Año Anterior")) Then


							
								For Each account As KeyValuePair(Of String, MemberInfo) In accountDict
								
									'Get account name
									Dim accountName As String = account.Key
									
									'Skip some account names
									If accountName <> "(Bypass)" AndAlso accountName <> "~"	
										Me.Recalculate(si, globals, api, args, accountName, sScenario, sYear, sBrand, iForecastMonth, costType)
									End If
							
							   Next
						   End If
							
						#End Region
						
						#Region "Occupancy Others Get Criteria"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("OccupancyOthersGetCriteria") Then
							
							'Get all the account members for occupancy and loop through them
							Dim accountDict As Dictionary(Of String, MemberInfo) = Me.CreateMemberLookupUsingFilter(si, "Accounts", occupancyAccountsFilter)
							
							For Each account As KeyValuePair(Of String, MemberInfo) In accountDict
							
								'Get account name and declare variables for criteria and if cpi is applied
								Dim accountName As String = account.Key
								Dim accountCriteria As Integer
								Dim cpiApplied As Integer = 0
								
								'Skip some account names
								If accountName <> "(Bypass)" AndAlso accountName <> "~"
									
									'Define Criteria and if CPI is applied based on the account name
									Select Case accountName

										Case "AL_OI_APO","GG_SUM_ELE","GG_SUM_GAS"
											accountCriteria = 1
										Case "GG_SUM_ELE","GG_SUM_GAS","GG_OPS_CAR","AL_OC_REN","AL_OC_COM","AL_OC_OTR","AL_OC_SUB"
											accountCriteria = 2
											cpiApplied = 1
										Case "AL_OI_OT", "AL_PUB_INV", "AL_PUB_OT", "AL_GG_VIA", "AL_ROY", "GG_SUM_AGU", "GG_OPS_CON", "GG_OPS_REC", "GG_OPS_LIM", "GG_OPS_UNI", "GG_EXT_SPI", "GG_EXT_LIM", "GG_EXT_MOT", "GG_EXT_MOT", "GG_EXT_CC", "GG_EXT_REG", "GG_EXT_TTE", "GG_EXT_AUD", "GG_EXT_ABO", "GG_EXT_ARC", "GG_EXT_MEN", "GG_EXT_OTR", "GG_MANT_PR", "GG_MANT_CO", "GG_MANT_MO", "GG_VEH_REN", "GG_VEH_PEA", "GG_VEH_GAS", "GG_OTR_PRI", "GG_OTR_TRI", "GG_OTR_OFI", "GG_OTR_FOR", "GG_OTR_TES", "GG_OTR_REN", "GG_OTR_COM", "GG_OTR_INC", "GG_OTR_REP", "GG_OTR_VAR", "62300019", "62300022"
											accountCriteria = 3
										Case "AL_OI_OT", "AL_PUB_INV", "AL_PUB_OT", "AL_GG_VIA", "GG_SUM_AGU", "GG_OPS_CON", "GG_OPS_REC", "GG_OPS_LIM", "GG_OPS_UNI", "GG_EXT_SPI", "GG_EXT_LIM", "GG_EXT_MOT", "GG_EXT_MOT", "GG_EXT_CC", "GG_EXT_REG", "GG_EXT_TTE", "GG_EXT_AUD", "GG_EXT_ABO", "GG_EXT_ARC", "GG_EXT_MEN", "GG_EXT_OTR", "GG_MANT_PR", "GG_MANT_CO", "GG_MANT_MO", "GG_VEH_REN", "GG_VEH_PEA", "GG_VEH_GAS", "GG_OTR_PRI", "GG_OTR_TRI", "GG_OTR_OFI", "GG_OTR_FOR", "GG_OTR_TES", "GG_OTR_REN", "GG_OTR_COM", "GG_OTR_INC", "GG_OTR_REP", "GG_OTR_VAR", "GG_EXT_LAV", "62300024"
											accountCriteria = 4
											cpiApplied = 1
										Case "GG_IT_TEL", "GG_IT_HIL", "GG_IT_MOB", "GG_IT_INT", "GG_IT_SER", "GG_IT_REP", "GG_IT_LIC", "62900015", "62300055", "62300080", "62300090", "GG_EXT_PAG"
											accountCriteria = 5
										Case "AL_PUB_FM", "AL_PUB_LSM"
											accountCriteria = 6
										Case "62300020", "62300023"
											accountCriteria = 7
										Case "AL_OC_REN"
											accountCriteria = 8
											cpiApplied = 1
										Case "AL_OC_REN"
											accountCriteria = 9
											cpiApplied = 1
										Case "AL_OI_ROY"
											accountCriteria = 10
											
									End Select
									
									'Input variables in the cube
									Api.Data.Calculate($"A#{accountName}:F#Criteria_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {accountCriteria.ToString}",True)
									Api.Data.Calculate($"A#{accountName}:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {cpiApplied.ToString}",True)
									
								End If
							
							Next
							
						#End Region
						
						#Region "Occupancy Rents Get Criteria"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("OccupancyRentsGetCriteria") Then
							
							'Get parameters
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							
							'Get account name and declare variables for criteria and if cpi is applied
							Dim accountName As String = "62100001"
							Dim accountCriteria As Integer = 4
							Dim cpiApplied As Integer = IIf(sScenario = "Forecast" AndAlso sBrand <> "P_SB_BEL" AndAlso sBrand <> "P_SB_FR" AndAlso sBrand <> "P_SB_HOL", 0, 1)
								
							'Input variables in the cube
							Api.Data.Calculate($"A#{accountName}:F#Variable_Rent_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 0",True)
							Api.Data.Calculate($"A#{accountName}:F#Criteria_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {accountCriteria.ToString}",True)
							Api.Data.Calculate($"A#{accountName}:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {cpiApplied.ToString}",True)
							
						#End Region
						
						#Region "Occupancy Rents Recalculate"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("OccupancyRentsRecalculate") Then
							
							'Get parameters
							Dim sAccountFixedRent As String = "62100001"
							Dim sAccountVariableRent As String = "62110000"
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim iForecastMonth As Integer = If(sScenario = "Forecast",CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month")),1)
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
							
							Dim isOpen As Boolean = Me.IsOpen(si, globals, api, args)
							
							'Initialize Rents
							If(iMonth >= iForecastMonth) Then
								If (isOpen) Then
									Me.RecalculateRents(si, globals, api, args, sAccountFixedRent, sAccountVariableRent, sScenario, sYear, sBrand, iForecastMonth)
								Else
									Api.Data.ClearCalculatedData($"A#{sAccountFixedRent}:F#None:O#Forms",True,True,True,True)
									Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#None:O#Forms",True,True,True,True)
								End If
							End If
							
						#End Region
						
						#Region "Occupancy Rents Initialize"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("OccupancyRentsInitialize") Then
							
							'Get parameters
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim costType As String = args.CustomCalculateArgs.NameValuePairs("p_costtype")
							
							'Initialize Rents
							Me.InitializeRents(si, globals, api, args, sAccount, sScenario, sYear, sBrand, iForecastMonth, costType)
							
						#End Region
						
						#Region "Recalculate"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate") Then
							
							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")					
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim costType As String = args.CustomCalculateArgs.NameValuePairs("p_costtype")
																								
							Me.Recalculate(si, globals, api, args, sAccount, sScenario, sYear, sBrand, iForecastMonth, costType)
							
						#End Region						
							
						#Region "Royalty Income Calculation"

						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeSetUnitPercInit") Then
						    ' Obtener los parámetros de entrada
						    Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
						    Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
						    Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
						    Dim sPeriod As String = Api.Pov.Time.Name
						    Dim sUnitPerc As String 
						    Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
						    Dim sMonth As Integer = sPeriod.Substring(5)
							Dim sEntity As String = Api.Pov.Entity.Name
							Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, sMonth, sEntity)
						
						    ' Decidir si se debe calcular
						    If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso sMonth >= iForecastMonth) Then
								
						        ' Obtener el porcentaje genérico
						        Dim cellData = api.Data.GetDataCell($"C#Local:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccount}:F#Perc_Generic_Input:O#Forms:I#None:U1#None")
								
						        If cellData IsNot Nothing Then
									
						            sUnitPerc = cellData.CellAmount.ToString(cultureInfo.InvariantCulture) ' Convertir a porcentaje real
									brapi.ErrorLog.LogMessage(si, sUnitPerc)
						        Else
						          brapi.ErrorLog.LogMessage(si, "Error: No se encontró el porcentaje genérico.")  
						        End If
						       
								Dim datoInicio As String = "A#Sales:F#None:U1#Top:O#Top"
						        ' Realizar el cálculo y asignar el resultado
						        If sUnitPerc > 0 AndAlso Not isClosed Then
									
									Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Initial_Result:O#Forms",True,True,True,True)
						            api.Data.Calculate($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = ({sUnitPerc}/100)*{datoInicio}*(-1)", True)
'									BRApi.ErrorLog.LogMessage(si, "Perc: " & api.Data.GetDataCell($"{sUnitPerc}").CellAmount.ToString)
'									BRApi.ErrorLog.LogMessage(si, "Inicio: " & api.Data.GetDataCell($"{datoInicio}").CellAmount.ToString)
'									BRApi.ErrorLog.LogMessage(si, "Calculo: " & api.Data.GetDataCell($"{sUnitPerc}*{datoInicio}*(-1)").CellAmount.ToString)
									
						        End If
							
						    End If
						
						End If
						
						#End Region
						
					    #Region "Royaltie Income Recalculate"
						
					   If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeRecalculate") Then
						
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim sEntity As String = Api.Pov.Entity.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
	                        Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
						    Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")		
						    Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))											
							
						    Dim sMonth As Integer = sPeriod.Substring(5)							
							
							Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
														
						    ' Decidir si se debe calcular
						    If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso sMonth >= iForecastMonth) Then
															
								Dim sCalc2 As String = $"A#AL_ROY:F#None:O#Forms = A#AL_ROY:F#Amount_Initial_Result:O#Top"	
								
								Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#AL_ROY:F#Perc_Variation_Input:O#Top").CellAmount
								Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#AL_ROY:F#Amount_Variation_Input:O#Top").CellAmount
								
		                        If dPerc_Variation <> 0 And dAmount_Variation <> 0 Then 
									sCalc2 = $"A#AL_ROY:F#None:O#Forms = (A#AL_ROY:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})) + ({dAmount_Variation.ToString().Replace(",",".")})"
						        ElseIf dPerc_Variation <> 0 Then 
									sCalc2 = $"A#AL_ROY:F#None:O#Forms = A#AL_ROY:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
								ElseIf dAmount_Variation <> 0 Then 
									sCalc2 = $"A#AL_ROY:F#None:O#Forms = A#AL_ROY:F#Amount_Initial_Result:O#Top + {dAmount_Variation.ToString().Replace(",",".")}"		
								End If	
								
								Api.Data.ClearCalculatedData($"A#AL_ROY:F#None",True,True,True,True)
								If Not isClosed Then Api.Data.Calculate(sCalc2,True)
								#End Region 
						
							End If
							
						End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
		#Region "Main Functions"
		
		#Region "Recalculate"
				
		Sub Recalculate(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sScenario As String, ByVal sYear As String, ByVal sBrand As String, ByVal iForecastMonth As Integer, ByVal costType As String)		
			
			'List of accounts in occupancy others
			Dim occupancyAccounts As New List(Of String) From {"AL_OC_COM", "AL_OC_OTR", "AL_OC_SUB"}
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			Dim sCountry As String
			Dim countryDict As New Dictionary(Of String, String) From { {"España", "ESP"},
																		{"Bélgica", "BEL"},
																		{"Andorra", "AND"},
																		{"Francia", "FR"},
																		{"Holanda", "HOL"},
																		{"Luxemburgo", "LUX"},
																		{"Portugal", "PT"},
																		{"", "ESP"}
																		}
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				Dim selectQuery As String = $"	SELECT country
												FROM XFC_CEBES
												WHERE cebe = '{sEntity}'"
			
				Dim dt As New DataTable
				dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
				If dt.Rows.Count = 0 Then
'					BRApi.ErrorLog.LogMessage(si, sEntity)
					sCountry = "ESP"
				Else					
					sCountry = countryDict(dt.Rows(0)("country").ToString)
				End If
			
			End Using
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = sYear
			Dim sScenarioSource As String = sScenario
			If sScenario = "Budget" Then
				sActualYear = (Integer.Parse(sYear) - 1).ToString()
				sScenarioSource = "Forecast"
			End If
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()			

			'Get sales amount
			Dim salesAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}:V#Periodic:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
			
			If (sScenario = "Budget") _
			Or (sScenario = "Forecast" And iMonth >= iForecastMonth) Then
			
				'Determine the origin entity for avg calc based on the account
				Dim originEntity As String = If(occupancyAccounts.Contains(sAccount), sEntity, $"{sBrand}_Input")
				Dim isOpen As Boolean = Me.IsOpen(si, globals, api, args)						
				
				If((Not occupancyAccounts.Contains(sAccount)) Or IsOpen)
					
'				If (sEntity = "F001068")
'					BRApi.ErrorLog.LogMessage(si, "Period: " & Api.Pov.Time.Name)
'				End If						
					
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
					    Dim key As String = $"S#{sScenario}:T#{sActualYear}M{mes}:E#{originEntity}:A#{sAccount}:F#Selected:O#Top"
					
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
					    accountSelected = $"A#{sAccount}:E#{originEntity}"
					Else
					    accountSelected = $"A#Default_Avarage:E#{sBrand}_Input"
					End If
					
					' Build the formula with the selected account
					For mes = 1 To 12
					    Dim suffixMes As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#{sAccount}:F#None:O#Top"
					    Dim suffixOrigen As String = $"T#{sActualYear}M{mes}:{accountSelected}:F#Selected:O#Top"
					    Dim suffixdenominator As String = $"T#{sActualYear}M{mes}:{accountSelected}:F#Selected:O#Top" 
					
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
	'				BRApi.ErrorLog.LogMessage(si, "Ecuación final generada: " & sCalcAvg)
					
					
					Dim sCalcAvgPercSales As String = String.Empty
					Dim targetSales As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms"
					
					Dim numeratorSales As String = "( "
					Dim denominatorSales As String = "( "
	
					
					' Build the formula with the selected account
					For mes = 1 To 12
					    Dim suffixMesSales As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#{sAccount}:U1#None:F#None:O#Top"
					    Dim suffixOrigenSales As String = $"T#{sActualYear}M{mes}:{accountSelected}:F#Selected:U1#None:O#Top"
						Dim suffixSales As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top" 
					    Dim suffixdenominatorSales As String = $"T#{sActualYear}M{mes}:{accountSelected}:U1#None:F#Selected:O#Top" 
					
					    If mes < 12 Then
					        numeratorSales &= $"S#{sScenarioSource}:{suffixMesSales} * S#{sScenario}:{suffixOrigenSales} + "
					        denominatorSales &= $"S#{sScenarioSource}:{suffixSales} * S#{sScenario}:{suffixdenominatorSales} +  "
					    Else
					        numeratorSales &= $"S#{sScenarioSource}:{suffixMesSales} * S#{sScenario}:{suffixOrigenSales} )"
					        denominatorSales &= $"S#{sScenarioSource}:{suffixSales} * S#{sScenario}:{suffixdenominatorSales} )"
					    End If
					Next    
					
					
					
					' Final Formula
					sCalcAvgPercSales = $"{targetSales} = {numeratorSales} / {denominatorSales} * S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top"
	'                BRApi.ErrorLog.LogMessage(si, "Ecuación final generada: " & sCalcAvgPercSales)
	
							
					
					Dim sCalcSameMonthAA, sCalcSameMonthAA_PercSales, sCalcSameMonthAA_PercSales_Delivery, sCalcSameMonthAA_PercSales_OwnDelivery, sCalcSameMonthAA_PercSales_ExternalDelivery As String
					If (sScenario = "Budget")
	
						sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms =	
					
										S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:O#Top"		
						
						sCalcSameMonthAA_PercSales = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top	"		
						
						sCalcSameMonthAA_PercSales_Delivery = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Delivery:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Delivery:O#Top"	
						
						sCalcSameMonthAA_PercSales_OwnDelivery = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Own_Delivery:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Own_Delivery:O#Top"	
						
						sCalcSameMonthAA_PercSales_ExternalDelivery = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Forecast:T#{sActualYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#External_Delivery:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#External_Delivery:O#Top"						
					Else
				
						sCalcSameMonthAA = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms =	
					
										S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:O#Top"
						
						sCalcSameMonthAA_PercSales = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top	"									
						
						sCalcSameMonthAA_PercSales_Delivery = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Delivery:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Delivery:O#Top	"		
						
						sCalcSameMonthAA_PercSales_OwnDelivery = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#Own_Delivery:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Own_Delivery:O#Top	"		
						
						sCalcSameMonthAA_PercSales_ExternalDelivery = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top
										/ S#Actual:T#{sActualPrevYear}M{iMonth.toString}:E#{sEntity}:A#Sales:F#None:U1#External_Delivery:O#Top
						
										* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#External_Delivery:O#Top	"		
					End If					
						
					Dim sCalcCustomEuro As String = $"E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms = E#{sBrand}_Input:A#{sAccount}:F#Custom_GC:O#Top"
					
					Dim sCalcCustomPerc As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
					
										E#{sBrand}_Input:A#{sAccount}:F#Custom_GC:U1#None:O#Top
										* -1			
										* (E#{sEntity}:A#Sales:F#None:U1#Top:O#Top / 100)"		
						
					Dim sCalc1 As String = String.Empty
					
					'Check if account is in other rent accounts to use entity criteria instead of brand
					Dim otherRentAccountList As New List(Of String) From {	"AL_OC_COM",
																			"AL_OC_OTR",
																			"AL_OC_SUB"
																			}
					Dim iCriteria As Integer = IIf(	otherRentAccountList.Contains(sAccount), _
													CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sEntity}:F#Criteria_GC:T#{sYear}M12:O#Top").CellAmount), _
													CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria_GC:T#{sYear}M12:O#Top").CellAmount))
	     
					Select Case iCriteria.toString
						'1 = Input
						Case "1"  
							sCalc1 = sCalc1	
						'2 = Same Month Prev. Year
						Case "2"
							sCalc1 = sCalcSameMonthAA
						'3 = Sales % Average
						Case "3"
							sCalc1 = sCalcAvgPercSales
						'4 = € Average
						Case "4"
							sCalc1 = sCalcAvg
						'5 = € Custom
						Case "5"
							sCalc1 = sCalcCustomEuro
						'6 = % Custom
						Case "6"
							sCalc1 = sCalcCustomPerc
						'7 = Total Delivery Sales %
						Case "7"
							sCalc1 = sCalcSameMonthAA_PercSales_Delivery
						'8 = Own Delivery Sales %
						Case "8"
							sCalc1 = sCalcSameMonthAA_PercSales_OwnDelivery
						'9 = External Delivery Sales %
						Case "9"
							sCalc1 = sCalcSameMonthAA_PercSales_ExternalDelivery
					End Select
					
					'Get CPI amount
					Dim CPIAmount As String = Api.Data.GetDataCell($"E#{sCountry}:C#Local:S#Actual:T#{sYear}M12:V#Periodic:A#{costType}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString.Replace(",", ".")
					'Apply CPI member depends on cost type
					Dim iCPI As Integer
					If costType = "CPI_Rent" Then
						
						iCPI = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sEntity}:T#{sYear}M12:V#Periodic:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
					
					Else
						
						iCPI = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:T#{sYear}M12:V#Periodic:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
						
					End If
					'Apply CPI
					sCalc1 = IIf(Not String.IsNullOrEmpty(sCalc1) AndAlso iCPI = 1, $"{sCalc1}) * (1 + ({iCPI} * ({CPIAmount} / 100)))".Replace("=","= ("), sCalc1)
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Initial_Result:O#Forms",True,True,True,True)
					If Not isClosed Then Api.Data.Calculate(sCalc1,True)
					
					Dim customCellAmount As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Initial_Result:O#Forms").CellAmount
					
					'Control Custom input types to prevent null data intersections
					If (iCriteria = 5 OrElse iCriteria = 6) AndAlso Not isClosed
						'Get custom cell amount, if it's 0, put a 0 in the cell assuming it's a null value
						If customCellAmount = 0 Then api.Data.Calculate($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 0")
					End If							
					
					If customCellAmount < -100000000 Then 
						'BRApi.ErrorLog.LogMessage(si, "sCalc: " & $"A#{sAccount}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = -1000000")
						api.Data.Calculate($"A#{sAccount}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = -100000000",True)  
					End If							
					
					'2. FINAL
					
					Dim sCalc2 As String = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top"	
					
					Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Variation_Input:O#Top").CellAmount
					Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Variation_Input:O#Top").CellAmount
					
					If dPerc_Variation <> 0 And dAmount_Variation <> 0 Then 
						
						sCalc2 = $"A#{sAccount}:F#None:O#Forms = (A#{sAccount}:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")}))+ ({dAmount_Variation.ToString().Replace(",",".")})"
			        ElseIf dPerc_Variation <> 0 Then 
						sCalc2 = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
					ElseIf dAmount_Variation <> 0 Then 
						If customCellAmount <> 0 Then
							sCalc2 = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top + {dAmount_Variation.ToString().Replace(",",".")}"	
						Else
							sCalc2 = $"A#{sAccount}:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {dAmount_Variation.ToString().Replace(",",".")}"	
						End If
					End If		
	
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#None",True,True,True,True)			
					If Not isClosed Then Api.Data.Calculate(sCalc2,True)	
					
				Else
					Api.Data.ClearCalculatedData($"A#{sAccount}:F#None",True,True,True,True)
				End If
			End If

		End Sub
		
		#End Region
		
		#Region "Initialize Rents"
				
		Sub InitializeRents(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccount As String, ByVal sScenario As String, ByVal sYear As String, ByVal sBrand As String, ByVal iForecastMonth As Integer, ByVal costType As String)		
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			Dim sCountry As String
			Dim countryDict As New Dictionary(Of String, String) From { {"España", "ESP"},
																		{"Bélgica", "BEL"},
																		{"Andorra", "AND"},
																		{"Francia", "FR"},
																		{"Holanda", "HOL"},
																		{"Luxemburgo", "LUX"},
																		{"Portugal", "PT"},
																		{"", "ESP"}
																		}
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				Dim selectQuery As String = $"	SELECT country
												FROM XFC_CEBES
												WHERE cebe = '{sEntity}'"
				
				sCountry = countryDict(BRApi.Database.ExecuteSql(dbConn, selectQuery, False).Rows(0)("country").ToString)
			
			End Using
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = IIf(sScenario = "Forecast", sYear, (Integer.Parse(sYear) - 1).ToString())
			Dim sScenarioSource As String = IIf(sScenario = "Forecast", "Actual", "Forecast")
			Dim sActualPrevYear As String = (Integer.Parse(sActualYear) - 1).ToString()
			Dim sCalcAvg As String
			Dim target As String = $"S#{sScenario}:T#{sYear}M12:E#{sEntity}:A#{sAccount}:F#Fixed_Rent_GC:O#Forms"
				
				Dim numerator As String = "( "
				Dim denominator As String = "( "
				
				' Dictionary for avoiding multiples calls to the cube  
				Dim dataCache As New Dictionary(Of String, Double)
				
				'  Flag for deciding if there is a 1.00 in A#{sAccount}, it means if there is any month selected
				Dim monthselected As Boolean = False
				
				' Check if there is a 1.00 in A#{sAccount}
				For mes = 1 To 12
				    Dim key As String = $"S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:A#62100001:F#Selected:O#Top"
				
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
				    accountSelected = $"A#Default_Avarage"
				End If		

			'Different scenario sources depending on scenario
			If sScenario = "Forecast" Then

               For mes = 1 To 12
				    Dim suffixMes As String = $"T#{sActualYear}M{mes}:E#{sEntity}:A#62100001:F#None:O#Top"
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
			
			Else
				
				'Build a formula string modifying the source scenario depending on the forecast month
				Dim avgString As String = ""
				Dim denomString As String = "( "
				
				For mes = 1 To 12
				    Dim sourceScenario As String = IIf(mes < iForecastMonth, "Actual", sScenarioSource)
				    
				    ' Build the numerator
				    If String.IsNullOrEmpty(avgString) Then
				        avgString = $"( S#{sourceScenario}:T#{sActualYear}M{mes}:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top"
				    Else
				        avgString &= $" + S#{sourceScenario}:T#{sActualYear}M{mes}:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top"
				    End If
				    
				    ' Build the denominator
				    If mes < 12 Then
				        denomString &= $"S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top + "
				    Else
				        denomString &= $"S#{sScenario}:T#{sActualYear}M{mes}:E#{sBrand}_Input:{accountSelected}:F#Selected:O#Top )"
				    End If
				Next
				
				sCalcAvg = $"S#{sScenario}:T#{sYear}M12:E#{sEntity}:A#{sAccount}:F#Fixed_Rent_GC:O#Forms = {avgString}) / {denomString}"
				

			End If
			
			Dim sCalc1 As String = sCalcAvg
			
			'Get Actual rent amount
			Dim actualRentAmount As String = Api.Data.GetDataCell($"E#{sEntity}:S#Actual:T#{sYear}M{iForecastMonth - 1}:V#YTD:A#AL_OC_REN:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None * -1").CellAmount.ToString.Replace(",", ".")
			
'			'Get CPI amount and if it applies
'			Dim CPIAmount As String = Api.Data.GetDataCell($"E#{sCountry}:C#Local:S#Actual:T#{sYear}M12:V#Periodic:A#{costType}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString.Replace(",", ".")
'			Dim iCPI As Integer
'			iCPI = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sEntity}:T#{sYear}M12:V#Periodic:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
			
'			'Apply CPI only to the non actual rent amount
'			sCalc1 = IIf(Not String.IsNullOrEmpty(sCalc1) AndAlso iCPI = 1, $"{sCalc1}) * (1 + ({iCPI} * ({CPIAmount} / 100)))".Replace("=","= ("), sCalc1)
			
			Api.Data.ClearCalculatedData($"A#{sAccount}:F#Fixed_Rent_GC:O#Forms",True,True,True,True)
			Api.Data.Calculate(sCalc1,True)

		End Sub
		
		#End Region
		
		#Region "Recalculate Rents"
				
		Sub RecalculateRents(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal sAccountFixedRent As String, ByVal sAccountVariableRent As String, ByVal sScenario As String, ByVal sYear As String, ByVal sBrand As String, ByVal iForecastMonth As Integer)		
			
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim sEntity As String = Api.Pov.Entity.Name
			
			Dim iMonth As Integer = sPeriod.Substring(5)
			
			Dim sActualYear As String = IIf(sScenario = "Budget", (Integer.Parse(sYear) - 1).ToString(), sYear)
			Dim sScenarioSource As String = "Actual"
			Dim sActualPrevYear As String =  (Integer.Parse(sActualYear) - 1).ToString()
			
			Dim sCountry As String
			Dim countryDict As New Dictionary(Of String, String) From { {"España", "ESP"},
																		{"Bélgica", "BEL"},
																		{"Andorra", "AND"},
																		{"Francia", "FR"},
																		{"Holanda", "HOL"},
																		{"Luxemburgo", "LUX"},
																		{"Portugal", "PT"},
																		{"", "ESP"}
																		}
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				Dim selectQuery As String = $"	SELECT country
												FROM XFC_CEBES
												WHERE cebe = '{sEntity}'"
				
				sCountry = countryDict(BRApi.Database.ExecuteSql(dbConn, selectQuery, False).Rows(0)("country").ToString)
			End Using
			
			Dim CPIAmount As String = Api.Data.GetDataCell($"E#{sCountry}:C#Local:S#Actual:T#{sYear}M12:V#Periodic:A#CPI_Rent:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim iCPI As Integer
						iCPI = CInt(Api.Data.GetDataCell($"A#62100001:S#{sScenario}:E#{sEntity}:T#{sYear}M12:V#Periodic:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
			
			'Get fixed rent and sales amounts
			Dim prefixedRentMonthlyAmount As Decimal= Api.Data.GetDataCell($"S#{sScenario}:T#{sYear}M12:E#{sEntity}:A#{sAccountFixedRent}:F#Fixed_Rent_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim fixedRentMonthlyAmount As Decimal = IIf(Not String.IsNullOrEmpty(iCPI),prefixedRentMonthlyAmount * (1 + (iCPI * CPIAmount / 100)),prefixedRentMonthlyAmount)
			Dim fixedRentYearlyAmount As Decimal = fixedRentMonthlyAmount * 12
			Dim salesYearlyAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}:V#Periodic:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim salesCurrentMonthPeriodicAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M{iMonth}:V#Periodic:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim salesPrevMonthYTDAmount As Decimal = 0
			If iMonth <> 1 Then salesPrevMonthYTDAmount = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M{iMonth - 1}:V#YTD:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim variableRentMonthlyAmount As Decimal
			
			'Get if it's closed
			Dim isClosed As Boolean = SharedFunctionsBR.IsClosed(si, sYear, iMonth, sEntity)
			
			'Control if they have variable rent
			Dim iVariableRent As Integer = CInt(Api.Data.GetDataCell($"A#{sAccountFixedRent}:E#{sEntity}:T#{sYear}M12:V#Periodic:F#Variable_Rent_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
			'Bracket one and two will be used later in formula choosing
			Dim bracketOnePerc As Decimal
			Dim bracketTwoPerc As Decimal
			If iVariableRent = 1 Then
				
				'Calculate variable rent
				'Get main percentage
				Dim mainVariablePerc As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccountFixedRent}:F#Variable_Rent_Perc_GC:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				'Get brackets information
				Dim bracketOneAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccountFixedRent}:F#Bracket_1_Sales_GC:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				bracketOnePerc = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccountFixedRent}:F#Bracket_1_Perc_GC:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				Dim bracketTwoAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccountFixedRent}:F#Bracket_2_Sales_GC:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				bracketTwoPerc = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccountFixedRent}:F#Bracket_2_Perc_GC:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				Dim bracketThreeAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccountFixedRent}:F#Bracket_3_Sales_GC:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				Dim bracketThreePerc As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccountFixedRent}:F#Bracket_3_Perc_GC:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
				
				'Create a list of brackets
				Dim brackets As New List(Of Bracket) From {
					New Bracket(0, bracketOneAmount, (bracketOnePerc / 100)),
					New Bracket(bracketOneAmount, bracketTwoAmount, (bracketTwoPerc / 100)),
					New Bracket(bracketTwoAmount, bracketThreeAmount, (bracketThreePerc / 100))
				}
				
				If mainVariablePerc = 0 Then
					
					variableRentMonthlyAmount = Me.CalculateBracketAmountMonthly(si, salesPrevMonthYTDAmount, salesCurrentMonthPeriodicAmount, brackets)
					
				Else
					
					variableRentMonthlyAmount = mainVariablePerc / 100 * -salesCurrentMonthPeriodicAmount
					
				End If
				
			End If
			
			'Difference between fixed rent and variable rent
			variableRentMonthlyAmount = Math.Min(0, variableRentMonthlyAmount - fixedRentMonthlyAmount)
			
			'Declare fixed rent calculation
			Dim sCalcFixedRent = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccountFixedRent}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =	
					
										{fixedRentMonthlyAmount.ToString.Replace(",", ".")}"
			Dim sCalcVariableRent As String		
			
			If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth)  Then
				
				'1 INITIAL
				'Monthly calculation depends on variable rent
				If iVariableRent = 1 Then sCalcVariableRent = $"
					S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =	
					{variableRentMonthlyAmount.ToString.Replace(",", ".")}
				"
						
				Api.Data.ClearCalculatedData($"A#{sAccountFixedRent}:F#None:O#Forms",True,True,True,True)
				If Not isClosed Then Api.Data.Calculate(sCalcFixedRent,True)
				Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms",True,True,True,True)
				If Not isClosed Then Api.Data.Calculate(sCalcVariableRent,True)
				
				'2. FINAL
				
				Dim sCalc2 As String = $"A#{sAccountVariableRent}:F#None:O#Forms = A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Top"
				
				Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#None:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc2,True)
				
			End If

		End Sub
		
		#End Region
	
		#Region "IsOpen"		
		
		Function IsOpen(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Boolean		
			
			'Get the parameters and declare frequently used variables
			Dim sEntity As String = Api.Pov.Entity.Name

			'Get the mirror entity, opening month and year of the entity						
			Dim openingDay As Integer
			Dim openingMonth As Integer
			Dim openingYear As Integer
			Dim mirrorEntity As String
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim selectQuery As String = $"
					SELECT
						DAY(od.OpenDate) AS day,
						MONTH(od.OpenDate) AS month,
						YEAR(od.OpenDate) AS year,
						Mirror
					FROM XFC_Opening o
					INNER JOIN XFC_OpenDate od ON o.cebe = od.cebe
					WHERE o.CeBe = '{sEntity}'"								
				
				Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
				
				'If there is no opening date for that entity don't do anything
				If dt.Rows.Count > 0 AndAlso dt.Rows(0)("year") <> 1900 Then
					'Get date
					openingDay = dt.Rows(0)("day")
					openingMonth = dt.Rows(0)("month")
					openingYear = dt.Rows(0)("year")
					mirrorEntity = dt.Rows(0)("Mirror")
				Else
					Return True
				End If
				
			End Using

			Dim openingPeriod As Integer = openingYear * 100 + openingMonth
			Dim sPeriod As String = Api.Pov.Time.Name
			Dim iMonth As Integer = sPeriod.Substring(5)
			Dim iYear As Integer = sPeriod.Substring(0,4)
			Dim currentPeriod As Integer = iYear * 100 + iMonth
			
'			BrApi.ErrorLog.LogMessage(si, "openingPeriod: " & openingPeriod.ToString)
			Return currentPeriod >= openingPeriod

		End Function				
		
		#End Region
		
		#End Region
		
		#Region "Helper Functions"
		
		#Region "Is Closed"
		
		Public Function IsClosed(ByVal si As SessionInfo, ByVal currentYear As String, ByVal currentMonth As Integer, ByVal sEntity As String) As Boolean
		
			'Declare closing datatable variable
			Dim closingDt As DataTable
		
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
				'Select the closing month and year for that entity and populate the datatable
				Dim selectString As String = $"	SELECT TOP(1)
													MONTH(close_date) + 1 AS month,
													YEAR(close_date) AS year
												FROM XFC_CEBESClosings
												WHERE cebe = '{sEntity}'"
				
				closingDt = BRApi.Database.ExecuteSql(dbConn, selectString, False)
				
			End Using
				
			'Get the month and year
			Dim closingMonth As Integer
			Dim closingYear As Integer
			
			If closingDt.Rows.Count > 0 Then
				
				closingMonth = CInt(closingDt.Rows(0)("month"))
				closingYear = CInt(closingDt.Rows(0)("year"))
				
			Else
				
				closingMonth = 12
				closingYear = 2100
				
			End If
			
			'Control if month is 13 to change month to 1 and year to the next year
			If closingMonth = 13 Then
				
				closingMonth = 1
				closingYear += 1
				
			End If
			
			'Return True if current year and month are equal or over closing year and month, else False
			If currentYear >= closingYear And currentMonth >= closingMonth Then
				
				Return True
				
			Else
				
				Return False
				
			End If
		
		End Function
		
		#End Region
		
		#Region "Member Lookup"

		Public Function CreateMemberLookupUsingFilter(ByVal si As SessionInfo, ByVal dimensionName As String, ByVal memberFilter As String) As Dictionary(Of String, MemberInfo)			
			Try
				'Define the dictionary that will act as the lookup (Note, the last part of the declaration makes the look case insensitive)
				Dim memLookup As New Dictionary(Of String, MemberInfo)(StringComparer.InvariantCultureIgnoreCase)
				
				'Execute the filter and check the result
				Dim memList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, dimensionName, memberFilter, True)
				If Not memList Is Nothing Then
					'Loop over the members and add them to the lookup dictionary (Keyed by name)
					For Each memInfo As MemberInfo In memList
						memLookup.Add(memInfo.Member.Name, memInfo)		
					Next											
				End If									
					
				'Add the bypass/unmapped members (Not a real member, but we do not to add / suspense items mapped to bypass)
				memLookup.Add(StageConstants.TransformationGeneral.BypassRow, Nothing)
				memLookup.Add(StageConstants.TransformationGeneral.DimUnmapped, Nothing)
				
				Return memLookup
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#End Region
		
		#Region "Calculate Bracket Amount"
		
		Function CalculateBracketAmountYearly(yearlySalesAmount As Decimal, brackets As List(Of Bracket), fixedAmount As Decimal) As Decimal
		
	        Dim total As Decimal = 0
			Dim counter As Integer = 0
			
	        For Each bracket As Bracket In brackets
				
	            If yearlySalesAmount > bracket.Min AndAlso bracket.Max <> 0 Then
					
	                Dim applicableAmount As Decimal = Math.Min(yearlySalesAmount, bracket.Max) - bracket.Min
					
					'Case when first bracket has 0%, use the fixed amount
	                total += IIf(counter = 0 AndAlso bracket.Percentage = 0, fixedAmount, applicableAmount * bracket.Percentage)
					
	            End If
				
				counter += 1
				
	        Next
	
	        Return - total
			
	    End Function
		
		Function CalculateBracketAmountMonthly(ByVal si As SessionInfo, salesPrevMonthYTDAmount As Decimal, salesCurrentMonthPeriodicAmount As Decimal,  brackets As List(Of Bracket)) As Decimal
		
	        Dim total As Decimal = 0
			Dim counter As Integer = 0
			
			Dim salesCurrentMonthYTDAmount As Decimal = salesCurrentMonthPeriodicAmount + salesPrevMonthYTDAmount
			
	        For Each bracket As Bracket In brackets
				
	            If salesCurrentMonthYTDAmount > bracket.Min AndAlso salesPrevMonthYTDAmount < bracket.Max Then
					
	                Dim applicableAmount As Decimal = Math.Min(salesCurrentMonthYTDAmount, bracket.Max) - Math.Max(bracket.Min, salesPrevMonthYTDAmount)
	                total += applicableAmount * bracket.Percentage
					
	            End If
				
				counter += 1
				
	        Next
	
	        Return - total
			
	    End Function
		
		#End Region
		
		#End Region
		
		#End Region
		
		#Region "Structures"
		
		'Bracket Object
		Public Structure Bracket
		    Public Min As Decimal
		    Public Max As Decimal
		    Public Percentage As Decimal
		
		    Public Sub New(min As Decimal, max As Decimal, percentage As Decimal)
		        Me.Min = min
		        Me.Max = max
		        Me.Percentage = percentage
		    End Sub
		End Structure

		#End Region
	
	End Class
End Namespace