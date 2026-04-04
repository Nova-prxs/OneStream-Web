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

Namespace OneStream.BusinessRule.Finance.GC_Calculates_Old
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
							
							For Each account As KeyValuePair(Of String, MemberInfo) In accountDict
							
								'Get account name
								Dim accountName As String = account.Key
								
								'Skip some account names
								If accountName <> "(Bypass)" AndAlso accountName <> "~"	
									Me.Recalculate(si, globals, api, args, accountName, sScenario, sYear, sBrand, iForecastMonth, costType)
								End If
							
							Next
							
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
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							
							'Initialize Rents
							Me.RecalculateRents(si, globals, api, args, sAccountFixedRent, sAccountVariableRent, sScenario, sYear, sBrand, iForecastMonth)
							
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
										
				' 1. INITIAL			
				Dim sCalcAvg As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms =	
				
									( S#{sScenarioSource}:T#{sActualYear}M1:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M2:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M3:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M4:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M5:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M6:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M7:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top		
									+ S#{sScenarioSource}:T#{sActualYear}M8:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M9:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M10:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M11:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M12:E#{sEntity}:A#{sAccount}:F#None:O#Top * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top)
									
									/ S#{sScenario}:T#{sActualYear}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top) "	
				
				Dim sCalcAvgPercSales As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
				
									( S#{sScenarioSource}:T#{sActualYear}M1:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M2:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M3:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M4:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M5:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M6:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M7:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top			
									+ S#{sScenarioSource}:T#{sActualYear}M8:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M9:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M10:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M11:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M12:E#{sEntity}:A#{sAccount}:F#None:U1#None:O#Top * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top )
									/
									( S#{sScenarioSource}:T#{sActualYear}M1:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M2:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M3:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M4:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M5:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M6:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M7:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top			
									+ S#{sScenarioSource}:T#{sActualYear}M8:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M9:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M10:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M11:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M12:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:U1#None:O#Top )
				
									* S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:U1#Top:O#Top"						
				
				Dim sCalcSameMonthAA, sCalcSameMonthAA_PercSales, sCalcSameMonthAA_PercSales_Delivery As String
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
				End If					
					
				Dim sCalcCustomEuro As String = $"E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:O#Forms = E#{sBrand}_Input:A#{sAccount}:F#Custom_GC:O#Top"
				
				Dim sCalcCustomPerc As String = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccount}:F#Amount_Initial_Result:U1#None:O#Forms =	
				
									E#{sBrand}_Input:A#{sAccount}:F#Custom_GC:U1#None:O#Top					
									* E#{sEntity}:A#Sales:F#None:U1#Top:O#Top
				
									* -1
				
									/ 100"		
					
				Dim sCalc1 As String = String.Empty
				
				'Check if account is in other rent accounts to use entity criteria instead of brand
				Dim otherRentAccountList As New List(Of String) From {	"AL_OC_COM",
																		"AL_OC_OTR",
																		"AL_OC_SUB"
																		}
				Dim iCriteria As Integer = IIf(	otherRentAccountList.Contains(sAccount), _
												CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sEntity}:F#Criteria_GC:T#{sActualYear}M12:O#Top").CellAmount), _
												CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria_GC:T#{sActualYear}M12:O#Top").CellAmount))

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
					'7 = Delivery Sales %
					Case "7"
						sCalc1 = sCalcSameMonthAA_PercSales_Delivery
				End Select
				
				'Get CPI amount
				Dim CPIAmount As Integer = CInt(Api.Data.GetDataCell($"E#{sCountry}:C#Local:S#Actual:T#{sActualYear}M12:V#Periodic:A#{costType}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
				
				'Apply CPI member depends on cost type
				Dim iCPI As Integer
				If costType = "CPI_Rent" Then
					
					iCPI = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sEntity}:T#{sActualYear}M12:V#Periodic:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
				
				Else
					
					iCPI = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:T#{sActualYear}M12:V#Periodic:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
					
				End If
				
				'Apply CPI
				sCalc1 = IIf(Not String.IsNullOrEmpty(sCalc1) AndAlso iCPI = 1, $"{sCalc1}) * (1 + ({iCPI} * ({CPIAmount} / 100)))".Replace("=","= ("), sCalc1)
				
				Api.Data.ClearCalculatedData($"A#{sAccount}:F#Amount_Initial_Result:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc1,True)							
				
				'2. FINAL
				
			    ' Obtener el valor de initialAmountResult
			    Dim initialAmountResult As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Initial_Result:O#Top").CellAmount
			    
'			    ' Verificar si el número es extremadamente grande o pequeño
'			    If initialAmountResult > 1000000000 Or initialAmountResult < -1000000000 Then
'			        ' Escalar hacia abajo para evitar problemas de precisión
'			        initialAmountResult = initialAmountResult / 1000000000
'			    End If
			
'			    ' Redondear a 6 decimales
'			    initialAmountResult = Math.Round(initialAmountResult, 6)
			
			    ' Obtener los valores de variaciones
			    Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Perc_Variation_Input:O#Top").CellAmount
			    Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccount}:F#Amount_Variation_Input:O#Top").CellAmount
			
			    ' Declarar la fórmula sCalc2
			    Dim sCalc2 As String = ""
			
			    ' Aplicar variación porcentual
			    If Not dPerc_Variation = 0 Then
			        sCalc2 = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
			    
			    ' Aplicar variación de cantidad
			    ElseIf Not dAmount_Variation = 0 Then
			        sCalc2 = $"A#{sAccount}:F#None:O#Forms = A#{sAccount}:F#Amount_Initial_Result:O#Top + {dAmount_Variation.ToString().Replace(",",".")}"	
			    End If
			
			    ' Desescalar si fue escalado
'			    If initialAmountResult > 1000000000 Or initialAmountResult < -1000000000 Then
'			        initialAmountResult = initialAmountResult * 1000000000
'			    End If
			
			    ' Registrar el cálculo ajustado
			    'BRApi.ErrorLog.LogMessage(si, $"Cálculo ajustado: {sCalc2}")

				Api.Data.ClearCalculatedData($"A#{sAccount}:F#None:O#Forms",True,True,True,True)			
				If Not isClosed Then Api.Data.Calculate(sCalc2,True)	 
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

			'Different scenario sources depending on scenario
			If sScenario = "Forecast" Then

				sCalcAvg = $"S#{sScenario}:T#{sYear}M12:E#{sEntity}:A#{sAccount}:F#Fixed_Rent_GC:O#Forms =	
				
									( S#{sScenarioSource}:T#{sActualYear}M1:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M1:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M2:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M2:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M3:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M3:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M4:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M4:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M5:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M5:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M6:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M6:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M7:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M7:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top		
									+ S#{sScenarioSource}:T#{sActualYear}M8:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M8:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M9:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M9:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M10:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M10:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M11:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M11:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top
									+ S#{sScenarioSource}:T#{sActualYear}M12:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M12:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top)
									
									/ S#{sScenario}:T#{sActualYear}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top"	
			
			Else
				
				'Build a formula string modifying the source scenario depending on the forecast month
				Dim avgString As String = ""
				For i As Integer = 1 To 12
					
					Dim sourceScenario As String = IIf(i < iForecastMonth, "Actual", sScenarioSource)
					
					avgString += IIf(String.IsNullOrEmpty(avgString), $"( S#{sourceScenario}:T#{sActualYear}M{i}:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M{i}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top", _
									$" + S#{sourceScenario}:T#{sActualYear}M{i}:E#{sEntity}:A#62100001:F#None:O#Top * S#{sScenario}:T#{sActualYear}M{i}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top")
					
				Next
				
				'Close brackets of the formula
				avgString += ")"
				
				sCalcAvg = $"S#{sScenario}:T#{sYear}M12:E#{sEntity}:A#{sAccount}:F#Fixed_Rent_GC:O#Forms =	
									{avgString}
									/ S#{sScenario}:T#{sActualYear}:E#{sBrand}_Input:A#{sAccount}:F#Selected:O#Top"						
				
			End If
			
			Dim sCalc1 As String = sCalcAvg
			
			'Get Actual rent amount
			Dim actualRentAmount As String = Api.Data.GetDataCell($"E#{sEntity}:S#Actual:T#{sYear}M{iForecastMonth - 1}:V#YTD:A#AL_OC_REN:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None * -1").CellAmount.ToString.Replace(",", ".")
			
			'Get CPI amount and if it applies
			Dim CPIAmount As Integer = CInt(Api.Data.GetDataCell($"E#{sCountry}:C#Local:S#Actual:T#{sActualYear}M12:V#Periodic:A#{costType}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
			Dim iCPI As Integer
			iCPI = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sEntity}:T#{sYear}M12:V#Periodic:F#CPI_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount)
			
			'Apply CPI only to the non actual rent amount
			sCalc1 = IIf(Not String.IsNullOrEmpty(sCalc1) AndAlso iCPI = 1, $"{sCalc1}) * (1 + ({iCPI} * ({CPIAmount} / 100)))".Replace("=","= ("), sCalc1)
			
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
			
			'Get fixed rent and sales amounts
			Dim fixedRentMonthlyAmount As Decimal = Api.Data.GetDataCell($"S#{sScenario}:T#{sYear}M12:E#{sEntity}:A#{sAccountFixedRent}:F#Fixed_Rent_GC:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim fixedRentYearlyAmount As Decimal = fixedRentMonthlyAmount * 12
			Dim salesYearlyAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}:V#Periodic:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim salesCurrentMonthPeriodicAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M{iMonth}:V#Periodic:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim salesPrevMonthYTDAmount As Decimal = 0
			If iMonth <> 1 Then salesPrevMonthYTDAmount = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}M{iMonth - 1}:V#YTD:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
			Dim forecastSalesAmount As Decimal = Api.Data.GetDataCell($"E#{sEntity}:S#{sScenario}:T#{sYear}:V#Periodic:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
																- E#{sEntity}:S#Actual:T#{sYear}M{iForecastMonth - 1}:V#YTD:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
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

'			If (sScenario = "Budget") Then
										
				' 1. INITIAL
				'Monthly calculation depends on variable rent
'				If rentMonthlyAmount <> 0 Then
					
'					sCalcVariableRent = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =	
						
'											{rentMonthlyAmount.ToString.Replace(",", ".")}"
					
'				Else
				
'					If iVariableRent = 1 Then
				
'						sCalcVariableRent = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms:U1#None =	
						
'											S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#Sales:F#None:O#Top:U1#Top / {salesYearlyAmount.ToString.Replace(",",".")}
											
'											* {rentYearlyAmount.ToString.Replace(",",".")}"
					
'					Else
						
'						sCalcVariableRent = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =	
						
'											{rentYearlyAmount.ToString.Replace(",",".")} / 12"
						
'					End If
					
'				End If
						
'				Api.Data.ClearCalculatedData($"A#{sAccountFixedRent}:F#None:O#Forms",True,True,True,True)
'				If Not isClosed Then Api.Data.Calculate(sCalcFixedRent,True)
'				Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms",True,True,True,True)
'				If Not isClosed Then Api.Data.Calculate(sCalcVariableRent,True)
				
'				'2. FINAL
				
'				Dim sCalc2 As String = $"A#{sAccountVariableRent}:F#None:O#Forms = A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Top"
				
'				Dim dPerc_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccountVariableRent}:F#Perc_Variation_Input:O#Top").CellAmount
'				Dim dAmount_Variation As Decimal = Api.Data.GetDataCell($"A#{sAccountVariableRent}:F#Amount_Variation_Input:O#Top").CellAmount
				
'				If Not dPerc_Variation = 0 Then 
'					sCalc2 = $"A#{sAccountVariableRent}:F#None:O#Forms = A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Top * (1 + {(dPerc_Variation / 100).ToString().Replace(",",".")})"
'				Else If Not dAmount_Variation = 0 Then 
'					sCalc2 = $"A#{sAccountVariableRent}:F#None:O#Forms = A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Top + {dAmount_Variation.ToString().Replace(",",".")}"	
'				End If
				
'				Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#None:O#Forms",True,True,True,True)			
'				If Not isClosed Then Api.Data.Calculate(sCalc2,True)		
			
				If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth)  Then
					
					'1 INITIAL
					'Monthly calculation depends on variable rent
					sCalcVariableRent = $"S#{sScenario}:T#{sPeriod}:E#{sEntity}:A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =	
						
											{variableRentMonthlyAmount.ToString.Replace(",", ".")}"
							
					Api.Data.ClearCalculatedData($"A#{sAccountFixedRent}:F#None:O#Forms",True,True,True,True)
					If Not isClosed Then Api.Data.Calculate(sCalcFixedRent,True)
					Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms",True,True,True,True)
					If Not isClosed Then Api.Data.Calculate(sCalcVariableRent,True)
					
					'2. FINAL
					
					Dim sCalc2 As String = $"A#{sAccountVariableRent}:F#None:O#Forms = A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Top"
					
					Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#None:O#Forms",True,True,True,True)			
					If Not isClosed Then Api.Data.Calculate(sCalc2,True)
				
			Else

				Dim sCalcAccountFixedRent As String = $"A#{sAccountFixedRent}:F#None = A#{sAccountFixedRent}:F#None:S#Actual"
				Dim sCalcAccountVariableRent As String = $"A#{sAccountVariableRent}:F#None = A#{sAccountVariableRent}:F#None:S#Actual"
				Api.Data.ClearCalculatedData($"A#{sAccountFixedRent}:F#None:O#Forms",True,True,True,True)
				If Not isClosed Then Api.Data.Calculate(sCalcAccountFixedRent,True)
				Api.Data.ClearCalculatedData($"A#{sAccountVariableRent}:F#Amount_Initial_Result:O#Forms",True,True,True,True)
				If Not isClosed Then Api.Data.Calculate(sCalcAccountVariableRent,True)
				
			End If

		End Sub
		
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