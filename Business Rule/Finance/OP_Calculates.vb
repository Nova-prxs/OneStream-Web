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

Namespace OneStream.BusinessRule.Finance.OP_Calculates
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate
						
						#Region "Distribute Init"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributeInit") Then
							
							'Get the parameters
							Dim sEntity As String = args.CustomCalculateArgs.NameValuePairs("p_entity")
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sOpenDate As String = args.CustomCalculateArgs.NameValuePairs("p_open_date")
							Dim sAccounts As String = args.CustomCalculateArgs.NameValuePairs("p_accounts")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							
							Try
								'Insert opening date if it doesn't exist
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
									Dim sInsertQuery As String  =  "UPDATE dbo.XFC_OpenDate SET 
																OpenDate = '" & sOpenDate  & "'
																Where Cebe = '" & sEntity & "'
																IF @@ROWCOUNT = 0
																BEGIN
																  	INSERT INTO XFC_OpenDate (CeBe, OpenDate) 
																	VALUES ('" & sEntity & "','" & sOpenDate & "')
		
																END;"
									BRApi.Database.ExecuteSql(dbConn,sInsertQuery,False)
									
								End Using
							Catch ex As Exception
								Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()
								selectionChangedTaskResult.IsOK = False
								selectionChangedTaskResult.ShowMessageBox = True
								selectionChangedTaskResult.Message = "Please, select an opening date for this unit first."
								Return selectionChangedTaskResult
							End Try
							'Execute distribute
							Me.ExecuteDistribute(si, sEntity, sScenario, sYear, sAccounts, iForecastMonth)
							
							'Aggregate for royalty openings
							If sAccounts = "Royalty" Then
								'Accounts to aggregate
								Dim accountsToAggregate As New List(Of String) From {
									"AL_OI_ROY",
									"AL_ROY"
								}
								'Brand and units parameters
								Dim sBrand As String = args.CustomCalculateArgs.NameValuePairs("p_brand")
								Dim sUnits As String = args.CustomCalculateArgs.NameValuePairs("p_units")
								Dim sUnitsOpenings As String = args.CustomCalculateArgs.NameValuePairs("p_units_openings")
								'Dictionary of parameters for the data management steps to aggregate
								Dim ParamDict As New Dictionary(Of String, String) From {
									{"p_scenario", sScenario},
									{"p_year", sYear},
									{"p_brand", sBrand},
									{"p_units", sUnits},
									{"p_units_openings", sUnitsOpenings}
								}
								'Execute Aggregation DM for each royalty account
								For Each account As String In accountsToAggregate
									ParamDict("p_account") = account
									BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "FQ_RoyaltyIncome_Aggregate_Init", ParamDict)
								Next
							End If
							
						#End Region
						
						#Region "Clear Opening"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearOpening") Then
							'Reference admin business rule to get functions
							Dim AdminBR As New OneStream.BusinessRule.Finance.ADMIN_Calculates.MainClass
							
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.Name},A#AL_RDI.Base,A#AL_MEXICO.Base,A#Sales)")
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							AdminBR.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
							
						#End Region
						
						#Region "Distribute"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Distribute") Then
							
							'Get the parameters and declare frequently used variables
							Dim sEntity As String = args.CustomCalculateArgs.NameValuePairs("p_entity")
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sAccounts As String = args.CustomCalculateArgs.NameValuePairs("p_accounts")
							Dim iForecastMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month"))
							Dim firstYTDMonth As String = If(sScenario = "Forecast", iForecastMonth.ToString, "1")
							Dim openingDay As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_opening_day"))
							Dim openingMonth As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_opening_month"))
							Dim openingYear As Integer = CInt(args.CustomCalculateArgs.NameValuePairs("p_opening_year"))
							Dim mirrorEntity As String = args.CustomCalculateArgs.NameValuePairs("p_mirror_entity")
							Dim sCalcFormula As String = ""
							
							Dim openingPeriod As Integer = openingYear * 100 + openingMonth
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
							Dim currentPeriod As Integer = sYear * 100 + iMonth
							
							Dim channelList As String() = New String(){"None", "Sala", "Take_Away", "Delivery"}
							Dim preopsAccountList As String() = New String(){"67800202", "67800203", "67800205", "67800208", _
																			"67800207", "67800206" }
							
							'Determine if the entity is open comparing current period with opening period and get if we are in the opening month
							Dim isOpen As Boolean = currentPeriod > openingPeriod
							Dim isOpeningMonth As Boolean = currentPeriod = openingPeriod
							
							'Get how many percent of the opening month is open
							Dim daysInMonth As Integer = Day(DateSerial(openingYear, openingMonth + 1, 0)) 'Get how many days the month has
							Dim percOpeningMonth As Decimal = (daysInMonth - openingDay + 1) / DaysInMonth
							
							'Create an origin member filter depending on the type of entity that we are distributing
							Dim originMemberfilter As String = If(sAccounts = "Own", _
																	"A#AL_EBITDA_Aux.Base.Remove(AL_OI_ROY_Aux)", _
																	"A#Sales_Aux, A#AL_OI_ROY_Aux, A#AL_ROY_Aux")
							
							'Get the origin members
							Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,"Accounts")
							Dim originMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimensionPK, originMemberfilter)
							
							'Loop though all the origin members
							For Each originMember As MemberInfo In originMemberList
								
								'Remove "_Aux" from the end of each origin member name to get the target parent member name
								Dim originMemberName As String = originMember.Member.Name
								Dim targetParentMemberName As String = Left(originMemberName, Len(originMemberName) - 4)
								
								'Get the amount from the origin and target parent member
								'Define the formula depending on the scenario
								Dim originMemberFormula As String = If(
									sScenario = "Forecast",
									$"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",
									$"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:V#Periodic:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
									- E#{sEntity}:T#{sYear}M12:S#Actual:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
								)
								Dim originMemberAmount As Decimal = api.Data.GetDataCell(originMemberFormula).CellAmount
								
'								BRApi.ErrorLog.LogMessage(si, "sPeriod: " & sPeriod.ToString & " - isOpeningMonth: " & isOpeningMonth.ToString)
'								BRApi.ErrorLog.LogMessage(si, "sPeriod: " & sPeriod.ToString & " - isOpen: " & isOpen.ToString)
								
								'Check if the account is from "PREOPS" and if the month can be elegible
								If InStr(1, "'" & Join(preopsAccountList, "'") & "'", "'" & targetParentMemberName & "'") > 0 _
									AndAlso iMonth <= openingMonth _
									AndAlso iMonth >= openingMonth - 2 _
									AndAlso sYear = openingYear Then
								
									'Check if the month should be calculated or not depending on the scenario and month
									If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Then
											
											'Build dictionary with prev and current months and their respective members
											Dim monthMemberDict As New Dictionary(Of Integer, String) From {
																												{openingMonth - 2, "OP_PrevMonth2"},
																												{openingMonth - 1, "OP_PrevMonth1"},
																												{openingMonth, "OP_OpeningMonth"}
																											}
																											
											'Get percentage of the concrete month
											Dim monthPerc As Decimal = api.Data.GetDataCell($"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:F#{monthMemberDict(iMonth)}:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None / 100").CellAmount
											'Calculate
											sCalcFormula = $"S#{sScenario}:A#{targetParentMemberName}:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																{(originMemberAmount * monthPerc).ToString.Replace(",", ".")}"
			
											'Clear data and calculate if it's open
											Api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
											Api.Data.Calculate(sCalcFormula,True)
										
									Else
										
										'Clear forms data
										Api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
										
									End If
								
								Else
									
									If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Then
										
										If originMemberName = "Sales_Aux" Then
										
											Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
												'Clean up first if data exists
												Dim deleteQuery As String = $"	DELETE 
																				FROM XFC_DailySales
																				WHERE
																					unit_code = '{sEntity}'
																					AND scenario = '{sScenario}'
																					AND year = {sYear}
																					AND MONTH(date) = {iMonth}"
												'BRApi.ErrorLog.LogMessage(si, deleteQuery)
												BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
											End Using
											
											Api.Data.ClearCalculatedData($"A#Sales:F#None:O#Forms",True,True,True,True)
											Api.Data.ClearCalculatedData($"A#Customers:F#None:O#Forms",True,True,True,True)
											Api.Data.ClearCalculatedData($"A#AL_VTA_RES:F#None:O#Forms:U1#None",True,True,True,True)
										
										End If
											
									End If
									
									'Check if it's open or the opening month
									If (isOpeningMonth OrElse isOpen) Then
										
										'Sales are calculated daily in a SQL table, then imported to the cube,
										'Cost of Sales are distributed by channel,
										'Preoperative Expenses begin before opening,
										'Other accounts are calculated directly in the cube
										If originMemberName = "Sales_Aux" Then
											'Get the amount from the target parent member
											'Define the formula depending on the scenario
											Dim targetParentMemberFormula As String = If(
												sScenario = "Forecast",
												$"E#{mirrorEntity}:T#{sYear}:S#{sScenario}:A#{targetParentMemberName}:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",
												$"E#{mirrorEntity}:T#{sYear}:S#{sScenario}:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
												- E#{mirrorEntity}:T#{sYear}:S#Actual:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
											)
											Dim targetParentMemberAmount As Decimal = api.Data.GetDataCell(targetParentMemberFormula).CellAmount
											
											'Get the transformation multiple to apply to all the base member amounts and declare the calculation formula variable
											Dim transformationMultiple As Decimal = If(targetParentMemberAmount = 0, 0, originMemberAmount / targetParentMemberAmount)
											Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")
											
											Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
											
												'Get Sales construction query
												Dim salesSQL As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_SQL_FinalSales")
											
												'Get the unit description of the opening entity
												Dim selectQuery As String = $"	SELECT description
																				FROM XFC_CEBES
																				WHERE cebe = '{sEntity}'"
												Dim unitDescription As String = BRApi.Database.ExecuteSql(dbConn, selectQuery, False).Rows(0)("description").ToString.Replace("'","''")
												
												'Create a daily filter in case it's the opening month
												Dim dailyFilter As String = If(isOpeningMonth, $"AND DAY(date) >= {openingDay}", "")
												
												'Clean up first if data exists
												Dim deleteQuery As String = $"	DELETE 
																				FROM XFC_DailySales
																				WHERE
																					unit_code = '{sEntity}'
																					AND scenario = '{sScenario}'
																					AND year = {sYear}
																					AND MONTH(date) = {iMonth}"
												'BRApi.ErrorLog.LogMessage(si, deleteQuery)
												BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
											
												'Insert the sales of the opening entity based on the mirror
												Dim insertQuery As String = $"
													INSERT INTO XFC_DailySales (
														scenario,
														year,
														brand,
														unit_code,
														unit_description,
														date,
														week_num,
														week_day,
														channel,
														sales,
														customers,
														comment
													)
													SELECT
														scenario,
														{sYear} AS year,
														brand,
														'{sEntity}' AS unit_code,
														'{unitDescription}' AS unit_description,
														date,
														week_num,
														week_day,
														channel,
														(
														  	{salesSQL}
									  					) 
														* {transformationMultipleStr} AS sales,
														ROUND(
																(
																	ISNULL(customers,0)
																	+ ISNULL(rem_customers_adj,0)
																	+ ISNULL(week_customers_prom_adj,0)
																	+ ISNULL(week_customers_camp_adj,0)
																	+ ISNULL(week_customers_remgrowth_adj,0)
																	+ ISNULL(week_customers_gen1_adj,0)
																	+ ISNULL(week_customers_gen2_adj,0)
																	+ ISNULL(week_customers_trend_adj, 0)
																)
																* {transformationMultipleStr},
																0
														) AS customers,
														'Opening' AS comment
													FROM XFC_DailySales
													WHERE
														unit_code = '{mirrorEntity}'
														AND scenario = '{sScenario}'
														AND YEAR(date) = {sYear}
														AND MONTH(date) = {iMonth}
														{dailyFilter};"
												BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
												
												'Get the sales and customers of the opening entity grouped by period and channel
												selectQuery = $"
													SELECT channel, SUM(sales) AS sales, SUM(customers) AS customers
													FROM XFC_DailySales
													WHERE
														unit_code = '{sEntity}'
														AND scenario = '{sScenario}'
														AND year = {sYear}
														AND MONTH(date) = {iMonth}
														AND Channel <> ''
													GROUP BY
														channel"
												Dim salesDt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
												
												'When existing rows, loop through all the channels to import the data
												If salesDt.Rows.Count > 0 Then
													
													For Each row As DataRow In salesDt.Rows
														'Get channel, sales and customers
														Dim rowChannel As String = row("channel").ToString.Replace(" ", "_") 'Dimension members are not using spaces
														Dim rowSales As String = If(IsNumeric(row("sales")), row("sales").ToString.Replace(",", "."), "0")
														Dim rowCustomers As String = CInt(row("customers")).ToString
														'Import the data for sales and customers
														sCalcFormula = $"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																			{rowSales}"
														Api.Data.ClearCalculatedData($"A#Sales:F#None:O#Forms:U1#{rowChannel}",True,True,True,True)
														Api.Data.Calculate(sCalcFormula,True)
														
														sCalcFormula = $"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Customers:F#None:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																			{rowCustomers}"
														Api.Data.ClearCalculatedData($"A#Customers:F#None:O#Forms:U1#{rowChannel}",True,True,True,True)
														Api.Data.Calculate(sCalcFormula,True)
														
													Next
													'Only load Own restaurants to PL, no franchises
													sCalcFormula = $"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#AL_VTA_RES:F#None:O#Forms:I#None:U1#None =
																		E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#Sala
																		+ E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#Take_Away
																		+ E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#Delivery"
													Api.Data.ClearCalculatedData($"A#AL_VTA_RES:F#None:O#Forms:U1#None",True,True,True,True)
													If sAccounts = "Own" Then Api.Data.Calculate(sCalcFormula,True) 
													
												Else
													
													Api.Data.ClearCalculatedData($"A#Sales:F#None:O#Forms",True,True,True,True)
													Api.Data.ClearCalculatedData($"A#Customers:F#None:O#Forms",True,True,True,True)
													Api.Data.ClearCalculatedData($"A#AL_VTA_RES:F#None:O#Forms:U1#None",True,True,True,True)
													
												End If
											
											End Using
											
										Else If sAccounts <> "Own" Then
											
											'Get the royalty to sales ratio
											Dim salesYearlyAmount As Decimal = api.Data.GetDataCell($"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#Sales_Aux:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
											Dim royaltySalesRatio As Decimal = originMemberAmount / salesYearlyAmount
											Dim royaltySalesRatioStr As String = royaltySalesRatio.ToString.Replace(",", ".")
										
											'Check if the month should be calculated or not depending on the scenario and month
											If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Then
												
												'Calculate the month amount using the transformation multiple
												sCalcFormula = $"E#{sEntity}:S#{sScenario}:A#{targetParentMemberName}:F#OP_Amount:O#Forms:U1#None =
																	E#{sEntity}:S#{sScenario}:A#Sales:F#None:O#Top:U1#Top
																	* {royaltySalesRatioStr}"
												
												'Clear data and calculate
												Api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#OP_Amount:O#Forms:U1#None",True,True,True,True)
												Api.Data.Calculate(sCalcFormula,True)
															
												'Recalculate flow none
												api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#None:O#Forms",True,True,True,True)
												
												
											Else
												
												'Clear forms data for actuals
												Api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
												
											End If
										Else
											'Get the amount from the target parent member
											'Define the formula depending on the scenario
											Dim targetParentMemberFormula As String = If(
												sScenario = "Forecast" OrElse originMemberName.Equals("AL_PUB_FM_Aux"),
												$"E#{mirrorEntity}:S#{sScenario}:A#{targetParentMemberName}:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",
												$"E#{mirrorEntity}:T#{sYear}:S#{sScenario}:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
												- E#{mirrorEntity}:T#{sYear}:S#Actual:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
											)
											Dim targetParentMemberAmount As Decimal = api.Data.GetDataCell(targetParentMemberFormula).CellAmount
											
											'If scenario is Forecast or account is marketing, modify origin member amount to a month amount based in a percentage
											If sScenario = "Forecast" OrElse originMemberName.Equals("AL_PUB_FM_Aux") Then 
												
												If (originMemberName.Equals("AL_OC_REN_Aux") _
												Or originMemberName.Equals("AL_OC_COM_Aux") _
												Or originMemberName.Equals("AL_OC_OTR_Aux")) Then
								
													originMemberAmount = api.Data.GetDataCell($"
													E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None / 12
													").CellAmount												
												
												Else
											
													originMemberAmount = api.Data.GetDataCell($"
													E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
													/ E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#Sales_Aux:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
													* E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
													").CellAmount
												End If
											
											End If	
											
											'Get the transformation multiple to apply to all the base member amounts and declare the calculation formula variable
											Dim transformationMultiple As Decimal = If(targetParentMemberAmount = 0, 0, originMemberAmount / targetParentMemberAmount)
											Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")
										
											'Get all the base members where the amount must be distributed
											Dim nValue As Integer = BRApi.Finance.Members.GetMemberId(si, dimensionPK.DimTypeId,targetParentMemberName)
											Dim targetBaseMemberList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,dimensionPK,nValue)
											
											'Modify transformation multiple if it's opening month and not forecast
											If isOpeningMonth AndAlso sScenario <> "Forecast" AndAlso Not originMemberName.Equals("AL_PUB_FM_Aux") Then transformationMultipleStr = (transformationMultiple * percOpeningMonth).ToString.Replace(",", ".")
											
											'Determine if it has to loop through each channel and declare u1 member name variable
											Dim isChannelAccount As Boolean = originMemberName = "AL_CVTAS_Aux"
											Dim u1MemberName As String = "None"
											
											'Loop though each target base member to calculate the amount
											For Each targetBaseMember As Member In targetBaseMemberList
												
												'Get the name of the target base member
												Dim targetBaseMemberName As String = targetBaseMember.Name
												
												'Check if we have to loop through channels
												If isChannelAccount Then
													
													'Loop through channels
													For Each channel As String In channelList
														
														'Update u1 member name and calculate data
														u1MemberName = channel
														Me.ClearAndCalculateData(si, api, sScenario, iMonth, iForecastMonth, sEntity, targetBaseMemberName, _
																			u1MemberName, mirrorEntity, transformationMultipleStr)
														
													Next
													
												Else
													
													'Calculate data
													Me.ClearAndCalculateData(si, api, sScenario, iMonth, iForecastMonth, sEntity, targetBaseMemberName, _
																			u1MemberName, mirrorEntity, transformationMultipleStr)
													
												End If
												
											Next
											
										End If
										
									End If
									
								End If
								
							Next
							
						#End Region
						
						#Region "Distribute Table Sales"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributeTableSales") Then
							
							'Get the parameters and declare frequently used variables
							Dim sEntity As String = api.Pov.Entity.Name
							Dim sScenario As String = api.Pov.Scenario.Name
							Dim sYear As String = api.Pov.Time.Name.Split("M")(0)
							Dim iForecastMonth As Integer = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month")
							Dim firstYTDMonth As String = If(sScenario = "Forecast", iForecastMonth.ToString, "1")
							Dim sCalcFormula As String = ""
							
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
									Return Nothing
								End If
							End Using
							
							Dim openingPeriod As Integer = openingYear * 100 + openingMonth
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
							Dim currentPeriod As Integer = sYear * 100 + iMonth
							
							'Determine if the entity is open comparing current period with opening period and get if we are in the opening month
							Dim isOpen As Boolean = currentPeriod > openingPeriod
							Dim isOpeningMonth As Boolean = currentPeriod = openingPeriod
							
							'Get how many percent of the opening month is open
							Dim daysInMonth As Integer = Day(DateSerial(openingYear, openingMonth + 1, 0)) 'Get how many days the month has
							Dim percOpeningMonth As Decimal = (daysInMonth - openingDay + 1) / DaysInMonth
								
							'Remove "_Aux" from the end of each origin member name to get the target parent member name
							Dim originMemberName As String = "Sales_Aux"
							Dim targetParentMemberName As String = Left(originMemberName, Len(originMemberName) - 4)
							
							'Get the amount from the origin and target parent member
							'Define the formula depending on the scenario
							Dim originMemberFormula As String = If(
								sScenario = "Forecast",
								$"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",
								$"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:V#Periodic:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
								- E#{sEntity}:T#{sYear}M12:S#Actual:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
							)
							Dim originMemberAmount As Decimal = api.Data.GetDataCell(originMemberFormula).CellAmount							
							
							'Check if it's open or the opening month
							If (isOpeningMonth OrElse isOpen) Then
								'Sales are calculated daily in a SQL table
								If originMemberName = "Sales_Aux" Then
									'Get the amount from the target parent member
									'Define the formula depending on the scenario
									Dim targetParentMemberFormula As String = If(
										sScenario = "Forecast",
										$"E#{mirrorEntity}:T#{sYear}:S#{sScenario}:A#{targetParentMemberName}:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",
										$"E#{mirrorEntity}:T#{sYear}:S#{sScenario}:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
										- E#{mirrorEntity}:T#{sYear}:S#Actual:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
									)
									Dim targetParentMemberAmount As Decimal = api.Data.GetDataCell(targetParentMemberFormula).CellAmount
									
									'Get the transformation multiple to apply to all the base member amounts and declare the calculation formula variable
									Dim transformationMultiple As Decimal = If(targetParentMemberAmount = 0, 0, originMemberAmount / targetParentMemberAmount)
									Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")
									
									Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									
										'Get Sales construction query
										Dim salesSQL As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_SQL_FinalSales")
									
										'Get the unit description of the opening entity
										Dim selectQuery As String = $"	SELECT description
																		FROM XFC_CEBES
																		WHERE cebe = '{sEntity}'"
										Dim unitDescription As String = BRApi.Database.ExecuteSql(dbConn, selectQuery, False).Rows(0)("description").ToString.Replace("'","''")
										
										'Create a daily filter in case it's the opening month
										Dim dailyFilter As String = If(isOpeningMonth, $"AND DAY(date) >= {openingDay}", "")
										
										'Clean up first if data exists
										Dim deleteQuery As String = $"	DELETE 
																		FROM XFC_DailySales
																		WHERE
																			unit_code = '{sEntity}'
																			AND scenario = '{sScenario}'
																			AND year = {sYear}
																			AND MONTH(date) = {iMonth}"
										BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
									
										'Insert the sales of the opening entity based on the mirror
										Dim insertQuery As String = $"
											INSERT INTO XFC_DailySales (
												scenario,
												year,
												brand,
												unit_code,
												unit_description,
												date,
												week_num,
												week_day,
												channel,
												sales,
												customers,
												comment
											)
											SELECT
												scenario,
												{sYear} AS year,
												brand,
												'{sEntity}' AS unit_code,
												'{unitDescription}' AS unit_description,
												date,
												week_num,
												week_day,
												channel,
												(
												  	{salesSQL}
							  					) 
												* {transformationMultipleStr} AS sales,
												ROUND(
														(
															ISNULL(customers,0)
															+ ISNULL(rem_customers_adj,0)
															+ ISNULL(week_customers_prom_adj,0)
															+ ISNULL(week_customers_camp_adj,0)
															+ ISNULL(week_customers_remgrowth_adj,0)
															+ ISNULL(week_customers_gen1_adj,0)
															+ ISNULL(week_customers_gen2_adj,0)
															+ ISNULL(week_customers_trend_adj, 0)
														)
														* {transformationMultipleStr},
														0
												) AS customers,
												'Opening' AS comment
											FROM XFC_DailySales
											WHERE
												unit_code = '{mirrorEntity}'
												AND scenario = '{sScenario}'
												AND YEAR(date) = {sYear}
												AND MONTH(date) = {iMonth}
												{dailyFilter};"
										BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
										
										'Update the sales depending on the monthly cube amount
										'Get cube monthly sales amount
										Dim cubeMonthlySales As String = api.Data.GetDataCell("A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmountAsText.Replace(",", ".")
										Dim updateQuery As String = $"
											DECLARE @totalMonthlySales DECIMAL(18,4);

											SELECT @totalMonthlySales = SUM(sales)
											FROM XFC_DailySales
											WHERE 
											    unit_code = '{sEntity}'
											    AND scenario = '{sScenario}'
											    AND year = {sYear}
											    AND MONTH(date) = {iMonth};
										
											UPDATE ds
											SET
												sales = sales * ({cubeMonthlySales} / @totalMonthlySales),
												customers = customers * ({cubeMonthlySales} / @totalMonthlySales)
											FROM XFC_DailySales ds
											WHERE
												unit_code = '{sEntity}'
												AND scenario = '{sScenario}'
												AND year = {sYear}
												AND MONTH(date) = {iMonth}
												AND channel <> '';
										"
										BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
									End Using
								End If
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
		
		#Region "Main Functions"
		
		Public Sub ExecuteDistribute(ByVal si As SessionInfo, ByVal sEntity As String, ByVal sScenario As String,
			ByVal sYear As String, ByVal sAccounts As String, ByVal iForecastMonth As Integer
		)
			'Get the mirror entity, opening month and year of the entity
			Dim openingDay As Integer
			Dim openingMonth As Integer
			Dim openingYear As Integer
			Dim mirrorEntity As String
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim selectQuery As String = $"	SELECT
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
			
					'Build a dictionary to send the parameters to a DM
					Dim customSubstVars As New Dictionary(Of String, String) From {
																					{"p_entity", sEntity},
																					{"p_scenario", sScenario},
																					{"p_year", sYear},
																					{"p_accounts", sAccounts},
																					{"p_forecast_month", iForecastMonth},
																					{"p_opening_day", openingDay},
																					{"p_opening_month", openingMonth},
																					{"p_opening_year", openingYear},
																					{"p_mirror_entity", mirrorEntity}
																				}
					
					'Launch the DM
					BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "OP_Distribute", customSubstVars)
				End If
			End Using
		
		End Sub
		#End Region
		
		#Region "Helper Functions"
		
		Public Sub ClearAndCalculateData(si As SessionInfo, api As FinanceRulesApi, sScenario As String, iMonth As Integer, iForecastMonth As Integer, _
										sEntity As String, targetBaseMemberName As String, u1MemberName As String, mirrorEntity As String, _
										transformationMultipleStr As String)
										
			'Declare sCalcFormula
			Dim sCalcFormula As String
		
			'Check if the month should be calculated or not depending on the scenario and month
			If sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth) Then
				
				'Calculate the month amount using the transformation multiple
				sCalcFormula = $"E#{sEntity}:S#{sScenario}:A#{targetBaseMemberName}:F#None:O#Forms:U1#{u1MemberName} =
									E#{mirrorEntity}:S#{sScenario}:A#{targetBaseMemberName}:F#None:O#Top:U1#{u1MemberName}
									* {transformationMultipleStr}"
			
				'Clear data and calculate
				Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#{u1MemberName}",True,True,True,True)
				Api.Data.Calculate(sCalcFormula,True)
				
			Else
				
				'Clear forms data for actuals
				Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#{u1MemberName}",True,True,True,True)
				
			End If
			
		End Sub
		
		#End Region
		
		#End Region
	End Class
End Namespace