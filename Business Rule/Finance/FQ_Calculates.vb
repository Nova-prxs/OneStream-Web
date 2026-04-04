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

Namespace OneStream.BusinessRule.Finance.FQ_Calculates
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
								
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate
						
						#Region "Royalty Income Recalculate Init"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeRecalculateInit") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnits As String = args.CustomCalculateArgs.NameValuePairs("p_units").Replace(".", ",")
							Dim sUnitsOpenings As String = args.CustomCalculateArgs.NameValuePairs("p_units_openings").Replace(".", ",")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
'							BRAPi.ErrorLog.LogMessage(si, "sUnitsOpenings: " & sUnitsOpenings)
							'Dictionary to map the brand with the franchises parent member
							Dim brandToFQParentDict As New Dictionary(Of String, String) From {
								{"F_BK", "F_BK_Input"},
								{"F_DP", "T002736"},
								{"F_FH", "F001053"},
								{"F_FHV", "F_FHV_Input"},
								{"F_FR", "FQSDH_FRID"},
								{"F_GI", "SF018352"},
								{"F_GI_PT", "FQSDH_GINO"},
								{"F_SB", "SF172926"},
								{"F_SB_BEL", "FQSDH_SBUX"},
								{"F_SB_FR", "FQSDH_SBUX"},
								{"F_SB_HO", "FQSDH_SBUX"},
								{"F_SB_PT", "SF222921"},
								{"F_VI", "SF018350"}
							}
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																							{"p_scenario", sScenario},																
																							{"p_year", sYear},																
																							{"p_brand", sBrand},																
																							{"p_brand_unit", brandToFQParentDict(sBrand)},																
																							{"p_account", sAccount},																
																							{"p_forecast_month", iForecastMonth},
																							{"p_unit", sUnits & ", " & sUnitsOpenings}
																						}
							
							'Launch the DM
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Recalculate", customSubstVars)
							
							'Map units parameter to let it flow through the next DM
							'BRApi.ErrorLog.LogMessage(si, $"{sUnits.Replace(",", ".")}.{sUnitsOpenings.Replace(",", ".")}")
							customSubstVars("p_unit") = $"{sUnits.Replace(",", ".")}.{sUnitsOpenings.Replace(",", ".")}"
							
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Aggregate", customSubstVars)
							
						#End Region
						
						#Region "Royalty Income Recalculate"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeRecalculate") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnit As String = args.CustomCalculateArgs.NameValuePairs("p_unit")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sTime As String = api.Pov.Time.Name
							Dim sMonth As Integer = sTime.Substring(5)
							Dim flipSign As Integer = IIf(sAccount = "AL_OI_ROY", 1, -1)
							
							'Decide if the month should be calculated depending on the scenario and month
							If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso sMonth >= iForecastMonth) Then
								
								api.Data.ClearCalculatedData($"A#{sAccount}:F#FQ_Amount:O#Forms",True,True,True,True)
								api.Data.ClearCalculatedData(True,True,True,True,$"A#Sales","F#None",,,"U1#Delivery, U1#Take_Away")
								api.Data.Calculate($"A#{sAccount}:F#FQ_Amount:O#Forms:U1#None = 
														A#Sales:F#None:O#Top:U1#Top
														* ( A#{sAccount}:F#Perc_Initial_Result:O#Forms:U1#None / 100 ) * {flipSign}", True)
								
							End If
							
						#End Region
						
						#Region "Royalty Income Aggregate Init"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeAggregateInit") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnits As String = args.CustomCalculateArgs.NameValuePairs("p_units").Replace(".", ",")
							Dim sUnitsOpenings As String = args.CustomCalculateArgs.NameValuePairs("p_units_openings").Replace(".", ",")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
'							BRAPi.ErrorLog.LogMessage(si, "sUnitsOpenings: " & sUnitsOpenings)
							'Dictionary to map the brand with the franchises parent member
							Dim brandToFQParentDict As New Dictionary(Of String, String) From {
								{"F_BK", "F_BK_Input"},
								{"F_DP", "T002736"},
								{"F_FH", "F001053"},
								{"F_FHV", "F_FHV_Input"},
								{"F_FR", "FQSDH_FRID"},
								{"F_GI", "SF018352"},
								{"F_GI_PT", "FQSDH_GINO"},
								{"F_SB", "SF172926"},
								{"F_SB_BEL", "FQSDH_SBUX"},
								{"F_SB_FR", "FQSDH_SBUX"},
								{"F_SB_HO", "FQSDH_SBUX"},
								{"F_SB_PT", "SF222921"},
								{"F_VI", "SF018350"}
							}
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																							{"p_scenario", sScenario},																
																							{"p_year", sYear},																
																							{"p_brand", sBrand},																
																							{"p_brand_unit", brandToFQParentDict(sBrand)},																
																							{"p_account", sAccount},																
																							{"p_forecast_month", iForecastMonth},
																							{"p_unit", sUnits & ", " & sUnitsOpenings}
																						}
																						
							
							customSubstVars("p_unit") = $"{sUnits.Replace(",", ".")}.{sUnitsOpenings.Replace(",", ".")}"
							
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Aggregate", customSubstVars)							
							
						#End Region
						
						#Region "Royalty Income Aggregate"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeAggregate") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnit As String = args.CustomCalculateArgs.NameValuePairs("p_unit").Replace(".",",")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							
							Dim sTime As String = api.Pov.Time.Name
							Dim sMonth As Integer = sTime.Substring(5)
							
							'List with the franchises parent members
							Dim parentUnitList As New List(Of String) From {
								"SF018350", "F_BK_Input", "T002736", "F001053", "F_FHV_Input",
								"FQSDH_FRID", "SF018352", "FQSDH_GINO", "SF172926", "FQSDH_SBUX", "SF222921"
							}
							
							'Decide if the month should be calculated depending on the scenario and month
							If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso sMonth >= iForecastMonth) Then
								
								'BRApi.ErrorLog.LogMessage(si, "Units: " & sUnit)
								
								'Calculate the total cumulative amount of the brand for FQ and OP flows
								Dim FQTotalAmount As Decimal = 0.0
								Dim OPTotalAmount As Decimal = 0.0
								For Each unit In sUnit.Split(",")
									'Filter out the pov entity to prevent duplicates
									
									Dim filterUnit As String = unit.Split("#")(1).Trim()
									If Not parentUnitList.Contains(filterUnit) Then
										FQTotalAmount += api.Data.GetDataCell($"
											{unit}:C#Local:A#{sAccount}:F#FQ_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
										OPTotalAmount += api.Data.GetDataCell($"
											{unit}:C#Local:A#{sAccount}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
									End If
								Next
								
								'Put the amount into the data cells for FQ and OP flows
								api.Data.ClearCalculatedData($"A#{sAccount}:F#FQ_Amount:O#Forms",True,True,True,True)
								api.Data.Calculate($"A#{sAccount}:F#FQ_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
														{FQTotalAmount.ToString.Replace(",",".")}", True)
								api.Data.ClearCalculatedData($"A#{sAccount}:F#OP_Amount:O#Forms",True,True,True,True)
								api.Data.Calculate($"A#{sAccount}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
														{OPTotalAmount.ToString.Replace(",",".")}", True)
														
								'Recalculate flow none
								api.Data.ClearCalculatedData($"A#{sAccount}:F#None:O#Forms",True,True,True,True)
								api.Data.Calculate($"A#{sAccount}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
														A#{sAccount}:F#FQ_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
														+ A#{sAccount}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None", True)
								
								
							End If
							
						#End Region
						
						#Region "Royalty Income Set Brand Percentage Init"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeSetBrandPercInit") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnits As String = args.CustomCalculateArgs.NameValuePairs("p_units").Replace(".", ",")
							Dim sUnitsOpenings As String = args.CustomCalculateArgs.NameValuePairs("p_units_openings")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							
							'Dictionary to map the brand with the franchises parent member
							Dim brandToFQParentDict As New Dictionary(Of String, String) From {
								{"F_BK", "F_BK_Input"},
								{"F_DP", "T002736"},
								{"F_FH", "F001053"},
								{"F_FHV", "F_FHV_Input"},
								{"F_FR", "FQSDH_FRID"},
								{"F_GI", "SF018352"},
								{"F_GI_PT", "FQSDH_GINO"},
								{"F_SB", "SF172926"},
								{"F_SB_BEL", "FQSDH_SBUX"},
								{"F_SB_FR", "FQSDH_SBUX"},
								{"F_SB_HO", "FQSDH_SBUX"},
								{"F_SB_PT", "SF222921"},
								{"F_VI", "SF018350"}
							}
							
							'Get brand percentage
							Dim brandPerc As Decimal = api.Data.GetDataCell($"E#{sBrand}_Input:C#Local:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccount}:F#Perc_Generic_Input:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																							{"p_scenario", sScenario},																
																							{"p_year", sYear},																
																							{"p_brand", sBrand},																
																							{"p_brand_unit", brandToFQParentDict(sBrand)},																
																							{"p_account", sAccount},																
																							{"p_forecast_month", iForecastMonth},
																							{"p_brand_perc", brandPerc.ToString.Replace(",",".")},
																							{"p_unit_perc", brandPerc.ToString.Replace(",",".")},
																							{"p_unit", sUnits & ", " & sUnitsOpenings}
																						}
							'Launch the DMs
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_SetBrandPerc", customSubstVars)
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_SetUnitPerc", customSubstVars)
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Recalculate", customSubstVars)
							
							'Map units parameter to let it flow through the next DM
							customSubstVars("p_unit") = $"{sUnits.Replace(",", ".")}.{sUnitsOpenings.Replace(",", ".")}"
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Aggregate", customSubstVars)
							
						#End Region
						
						#Region "Royalty Income Set Brand Percentage"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeSetBrandPerc") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnit As String = args.CustomCalculateArgs.NameValuePairs("p_unit")
							Dim sBrandPerc As String = args.CustomCalculateArgs.NameValuePairs("p_brand_perc")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							
							'Calculate the data cell
							api.Data.Calculate($"S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccount}:F#Perc_Generic_Input:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {sBrandPerc}", True)
							
						#End Region
						
						#Region "Royalty Income Set Unit Percentage Init"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeSetUnitPercInit") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnits As String = args.CustomCalculateArgs.NameValuePairs("p_units").Replace(".", ",")
							Dim sUnitsOpenings As String = args.CustomCalculateArgs.NameValuePairs("p_units_openings").Replace(".", ",")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							
							'Dictionary to map the brand with the franchises parent member
							Dim brandToFQParentDict As New Dictionary(Of String, String) From {
								{"F_BK", "F_BK_Input"},
								{"F_DP", "T002736"},
								{"F_FH", "F001053"},
								{"F_FHV", "F_FHV_Input"},
								{"F_FR", "FQSDH_FRID"},
								{"F_GI", "SF018352"},
								{"F_GI_PT", "FQSDH_GINO"},
								{"F_SB", "SF172926"},
								{"F_SB_BEL", "FQSDH_SBUX"},
								{"F_SB_FR", "FQSDH_SBUX"},
								{"F_SB_HO", "FQSDH_SBUX"},
								{"F_SB_PT", "SF222921"},
								{"F_VI", "SF018350"}
							}
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																							{"p_scenario", sScenario},																
																							{"p_year", sYear},																
																							{"p_brand", sBrand},																
																							{"p_brand_unit", brandToFQParentDict(sBrand)},																
																							{"p_account", sAccount},																
																							{"p_forecast_month", iForecastMonth},
																							{"p_unit", sUnits & ", " & sUnitsOpenings}
							}
							
							'Launch the DMs
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_SetUnitPerc", customSubstVars)
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Recalculate", customSubstVars)
							
							'Map units parameter to let it flow through the next DM
							customSubstVars("p_unit") = $"{sUnits.Replace(",", ".")}.{sUnitsOpenings.Replace(",", ".")}"
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Aggregate", customSubstVars)
							
						#End Region
						
						#Region "Royalty Income Set Unit Percentage"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RoyaltyIncomeSetUnitPerc") Then

							'Get all the parameters
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							Dim sAccount As String = args.CustomCalculateArgs.NameValuePairs("p_account")
							Dim sUnit As String = args.CustomCalculateArgs.NameValuePairs("p_unit")
							Dim sUnitPerc As String = api.Data.GetDataCell($"C#Local:S#{sScenario}:T#{sYear}M12:V#Periodic:A#{sAccount}:F#Perc_Generic_Input:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString.Replace(",",".")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sTime As String = api.Pov.Time.Name
							Dim sMonth As Integer = sTime.Substring(5)
							
							'Decide if the month should be calculated depending on the scenario and month
							If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso sMonth >= iForecastMonth) Then
								
								api.Data.Calculate($"A#{sAccount}:F#Perc_Initial_Result:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {sUnitPerc}", True)
							
							End If
								
						#End Region
							
						End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Functions"
		
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
		
		#End Region
		
		#Region "Structures"

		#End Region
		
	End Class
End Namespace