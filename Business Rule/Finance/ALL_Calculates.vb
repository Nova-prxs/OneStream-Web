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

Namespace OneStream.BusinessRule.Finance.ALL_Calculates
	
	Public Class MainClass
		
		'Reference business rule to get functions
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType					
						
					Case Is = FinanceFunctionType.CustomCalculate	
						
						#Region "Mapping Brand"
							
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("MappingBrand") Then
						
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")			
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")							
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))							
							Dim sBrandText As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
							
							Dim brandGroupDict As New Dictionary(Of String, String) From {
																{"Burger King", "P_BK"},
																{"Fridays", "P_FR"},
																{"Domino''s Pizza", "P_DP"},
																{"Foster''s Hollywood", "P_FH"},
																{"Foster''s Hollywood Valencia", "P_FHV"},
																{"Starbucks", "P_SB"},
																{"Starbucks Portugal", "P_SB_PT"},
																{"Starbucks Bélgica", "P_SB_BEL"},
																{"Starbucks Francia", "P_SB_PT"},
																{"Starbucks Holanda", "P_SB_HO"},
																{"Ginos", "P_GI"},
																{"Ginos Portugal", "P_GI_PT"},
																{"VIPS", "P_VI_Aux"}
																}
																
							Dim sBrand As String = brandGroupDict(sBrandText)																
							
							'Build a dictionary to send the parameters to a DM
							Dim customSubstVars As New Dictionary(Of String, String) From {
																			{"p_scenario", sScenario},																
																			{"p_year", sYear},																															
																			{"p_brand", sBrand},																
																			{"p_forecast_month", iForecastMonth}
																		}
																		
							Dim customSubstVarsVipsSmart As New Dictionary(Of String, String) From {
																			{"p_scenario", sScenario},																
																			{"p_year", sYear},																															
																			{"p_brand", "P_VS"},																
																			{"p_forecast_month", iForecastMonth}
																		}	
															
							Dim sBrandAgg As String = IIf(sBrand.Equals("P_FHV"), "M_FH", sBrand.Replace("P_","M_"))						
							Dim customSubstVarsAgg As New Dictionary(Of String, String) From {
																			{"p_scenario", sScenario},																
																			{"p_year", sYear},																															
																			{"p_brand", sBrandAgg.Replace("_Aux","")},																
																			{"p_forecast_month", iForecastMonth}
																		}																		
																																			
																		
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ALL_Recalculates", customSubstVars)															
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ALL_Aggregate", customSubstVarsAgg)
							
						#End Region
						
						#Region "Recalculate"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Recalculate") Then									
									
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")				
							
							Dim sActualYear As String = IIf (sScenario = "Budget", (Integer.Parse(sYear) - 1).ToString(), sYear)
							
							Dim iTime As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, sYear)
							Dim sText1 As String = BRApi.Finance.Entity.Text(si, Api.Pov.Entity.MemberId,2,0,iTime)		
							
							Dim sEntity As String = Api.Pov.Entity.Description
							Dim sTime As String = Api.Pov.Time.Description
							'BRAPI.ErrorLog.LogMessage(si, "Registro: " & sEntity & " - " & sTime)
							
							If(sText1.Equals("Comparables") _
							Or sText1.Equals("Cerradas") _
							Or sText1.Equals("Reformas") _
							Or sText1.Equals("Reformas Año Anterior")) Then		
							
							'01. COST OF SALES
																											
								Dim COS_Calculates As New OneStream.BusinessRule.Finance.COS_Calculates.MainClass
					
								COS_Calculates.Recalculate(si, globals, api, args, "Theoretical_Costs", "Sala")
								COS_Calculates.Recalculate(si, globals, api, args, "Theoretical_Costs", "Delivery")
								COS_Calculates.Recalculate(si, globals, api, args, "Theoretical_Costs", "Take_Away")
								
								Dim slAccountsCOS As New List(Of String) From {"Condiments","Packaging","Personnel_Cost","Merchandising","Waste","Inventory_Waste"}
												
								For Each sAccount As String In slAccountsCOS
									COS_Calculates.Recalculate(si, globals, api, args, sAccount, "None")
								Next
								
								COS_Calculates.RecalculateDifRT(si, globals, api, args, "DIF_RT", "None")
								
							'02. PERSONNEL COSTS
							
								Dim PC_Calculates As New OneStream.BusinessRule.Finance.PC_Calculates.MainClass
							
								Dim slCategories As New List(Of String) From {"Distributors","Base"}
					
								'Productivity & Hours recalc
								For Each sCategory As String In slCategories
									PC_Calculates.Recalculate_Productivity(si, globals, api, args, "Productivity", If(sCategory = "Distributors", sCategory, "None"))
									PC_Calculates.Recalculate_Hours(si, globals, api, args, "productive_hs_total", sCategory)
									PC_Calculates.Recalculate_Hours(si, globals, api, args, "total_hs", sCategory)								
								Next
								
	'							'Costs recalc
								PC_Calculates.Recalculate_Costs(si, globals, api, args, "RE_SAL_FLX", "Distributors")
								PC_Calculates.Recalculate_Costs(si, globals, api, args, "EB_SAL_FLX", "Base")
								
								'Pending AGGREGATE and recalculate productivity Brand
														
							'03. GENERAL COSTS
							
								Dim GC_Calculates As New OneStream.BusinessRule.Finance.GC_Calculates.MainClass
							
								Dim oDimPK As DimPk = api.Dimensions.GetDim("Accounts").DimPk
								Dim oMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(oDimPK, $"A#AL_OING.Children.Remove(AL_GG_VIA, AL_OI_CV, AL_OI_ROY, AL_OI_PUB, AL_OI_FEE, AL_OI_TRI, AL_OI_VIN, AL_OI_SUB),A#AL_ROY, A#AL_PUB.Children, A#AL_GG_VIA, A#GG_EXT_SEG.Children, A#GG_EXT_AGR.Children, A#GG_EXT_MOT.Children, A#AL_GG_SUM.Children, A#AL_GG_IT.Children, A#AL_GG_OPS.Children, A#AL_GG_EXT.Children.Remove(GG_EXT_SEG, GG_EXT_AGR, GG_EXT_MOT), A#AL_GG_MANT.Children, A#AL_GG_VEH.Children, A#GG_OTR_PRI.Children, A#AL_GG_OTR.Children.Remove(GG_OTR_PRI)", Nothing)

								For Each oMember As MemberInfo In oMemberList
									
									Dim sAccount As String = oMember.Member.Name					
									Dim iCriteria As Integer = CInt(Api.Data.GetDataCell($"A#{sAccount}:E#{sBrand}_Input:F#Criteria_GC:T#{sYear}M12:O#Top").CellAmount)
									If iCriteria = 3 Or iCriteria = 6 Or iCriteria = 7 Then
										'BRApi.ErrorLog.LogMessage(si, "sAccount: " & sAccount)
										GC_Calculates.Recalculate(si, globals, api, args, sAccount, sScenario, sYear, sBrand, iForecastMonth, "CPI_General")
									End If
								Next
								
							End If
							
						#End Region
						
						#Region "Recalculate Personnel Costs"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("RecalculatePersonnelCosts") Then									
									
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")				
							
							Dim sActualYear As String = IIf (sScenario = "Budget", (Integer.Parse(sYear) - 1).ToString(), sYear)
							
							Dim iTime As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, sYear)
							Dim sText1 As String = BRApi.Finance.Entity.Text(si, Api.Pov.Entity.MemberId,2,0,iTime)		
							
							Dim sEntity As String = Api.Pov.Entity.Description
							Dim sTime As String = Api.Pov.Time.Description
							
							If(sText1.Equals("Comparables") _
							Or sText1.Equals("Cerradas") _
							Or sText1.Equals("Reformas") _
							Or sText1.Equals("Reformas Año Anterior")) Then
							
								Dim PC_Calculates As New OneStream.BusinessRule.Finance.PC_Calculates.MainClass
								
								'Recalculate management costs
								'Get account members for management costs
								Dim managementAccountsMemberFilter As String = $"A#EG_SAL_FLX,A#AL_CP_EG.Children.Remove(EG_SAL,EB_VAR,RE_VAR)"
								Dim managementAccounts As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(BRApi.Finance.Dim.GetDimPk(si, "Accounts"), managementAccountsMemberFilter, Nothing)
								'Loop through each account and recalculate cost
								For Each managementAccount As MemberInfo In managementAccounts
									PC_Calculates.Recalculate_Costs(si, globals, api, args, managementAccount.Member.Name, "Management")
								Next
								
								'Recalculate management hours
								'Get account members for hours
								Dim hourAccountsMemberFilter As String = "A#total_hs, A#non_productive_hs_total.Base,A#productive_hs_total"
								Dim hourAccounts As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(BRApi.Finance.Dim.GetDimPk(si, "Accounts"), hourAccountsMemberFilter, Nothing)
								'Loop through each hour account and recalculate hours
								For Each hourAccount As MemberInfo In hourAccounts
									PC_Calculates.Recalculate_Hours(si, globals, api, args, hourAccount.Member.Name, "Management")
								Next
								'Recalculate productive hs total
								PC_Calculates.Recalculate_Hours(si, globals, api, args, "productive_hs_total", "Management")
								
								Dim slCategories As New List(Of String) From {"Distributors","Base"}
								
								'Dictionary to get the account filter depending on the category
								Dim categoryAccountDict As New Dictionary(Of String, String) From {
									{"Distributors", "RE"},
									{"Base", "EB"}
								}
								
								'Productivity, Hours and Costs recalc for each category
								For Each sCategory As String In slCategories
									PC_Calculates.Recalculate_Productivity(si, globals, api, args, "Productivity", If(sCategory = "Distributors", sCategory, "None"))
									'Loop through each hour account and recalculate hours
									For Each hourAccount As MemberInfo In hourAccounts
										PC_Calculates.Recalculate_Hours(si, globals, api, args, hourAccount.Member.Name, sCategory)
									Next
									'Recalculate Hour/Cost
									PC_Calculates.Recalculate_HourCosts(si, globals, api, args, "HourCosts", sCategory)
									'Get account members for costs
									Dim costAccountsMemberFilter As String = $"A#{categoryAccountDict(sCategory)}_SAL_FLX,A#AL_CP_{categoryAccountDict(sCategory)}.Children.Remove({categoryAccountDict(sCategory)}_SAL,EB_VAR,RE_VAR)"
									Dim costAccounts As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(BRApi.Finance.Dim.GetDimPk(si, "Accounts"), costAccountsMemberFilter, Nothing)
									'Loop through each account and recalculate costs
									For Each costAccount As MemberInfo In costAccounts
										PC_Calculates.Recalculate_Costs(si, globals, api, args, costAccount.Member.Name, sCategory)
									Next
								Next
								
								'Recalculate other accounts
								'Get member filter parameter for other personnel cost accounts and use it to get the account member infos
								Dim otherAccountsMemberFilter As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "PC_prm_Accounts_Others_Literal")
								Dim otherAccounts As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(BRApi.Finance.Dim.GetDimPk(si, "Accounts"), otherAccountsMemberFilter, Nothing)
								'Loop through each account and recalculate other costs
								For Each otherAccount As MemberInfo In otherAccounts
									PC_Calculates.Recalculate_Costs(si, globals, api, args, otherAccount.Member.Name, "")
								Next
								
							End If
						
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