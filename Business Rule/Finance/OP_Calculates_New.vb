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

Namespace OneStream.BusinessRule.Finance.OP_Calculates_New
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
		
			Try
				Select Case api.FunctionType
					
					Case Is = FinanceFunctionType.CustomCalculate

						#Region "Variables"						
						
							'Get the parameters and declare frequently used variables
							Dim sEntity As String = args.CustomCalculateArgs.NameValuePairs("p_entity")
							Dim sScenario As String = args.CustomCalculateArgs.NameValuePairs("p_scenario")
							Dim sYear As String = args.CustomCalculateArgs.NameValuePairs("p_year")
							Dim sAccounts As String = args.CustomCalculateArgs.NameValuePairs("p_accounts")
							Dim iForecastMonth As Integer = If(sScenario = "Forecast",CInt(args.CustomCalculateArgs.NameValuePairs("p_forecast_month")),1)
							Dim firstYTDMonth As String = If(sScenario = "Forecast", iForecastMonth.ToString, "1")
							Dim channelList As String() = New String(){"None", "Sala", "Take_Away", "Delivery"}
							Dim sCalcFormula As String = ""
							
							Dim sOpenDate As String = args.CustomCalculateArgs.NameValuePairs("p_open_date")
							
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
									WHERE o.CeBe = '{sEntity}' AND o.Year = {sYear} AND o.Scenario = '{sScenario}'"								
								
								Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
								
								'If there is no opening date for that entity don't do anything
								If dt.Rows.Count > 0 AndAlso dt.Rows(0)("year") <> 1900 Then
									'Get date
									openingDay = dt.Rows(0)("day")
									openingMonth = dt.Rows(0)("month")
									openingYear = dt.Rows(0)("year")
									mirrorEntity = dt.Rows(0)("Mirror")
								Else If Not args.CustomCalculateArgs.FunctionName.Contains("Clear")
									Return Nothing
								End If
							End Using

							Dim openingPeriod As Integer = openingYear * 100 + openingMonth
							Dim sPeriod As String = Api.Pov.Time.Name
							Dim iMonth As Integer = sPeriod.Substring(5)
							Dim currentPeriod As Integer = sYear * 100 + iMonth
							
							'Determine if the entity is open comparing current period with opening period and get if we are in the opening month
							Dim isOpen As Boolean = currentPeriod >= openingPeriod
							Dim isOpeningMonth As Boolean = currentPeriod = openingPeriod
							
							'Get how many percent of the opening month is open
							Dim daysInMonth As Integer = Day(DateSerial(openingYear, openingMonth + 1, 0)) 'Get how many days the month has
							Dim percOpeningMonth As Decimal = (daysInMonth - openingDay + 1) / DaysInMonth
							
						#End Region						
						
						#Region "Sales - Distribution YearTotal by Mirror"
						
						If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributeSales") Then						
								
							'Remove "_Aux" from the end of each origin account to get the target parent account
							Dim originAccount As String = "Sales_Aux"
							Dim targetAccount As String = Left(originAccount, Len(originAccount) - 4)	
							
							'Check if it's open or the opening month
							If (iMonth >= iForecastMonth AndAlso isOpen) Then				
								Dim openingSalesTotalYear As Decimal = api.Data.GetDataCell($"E#{sEntity}:T#{sYear}M12:C#Local:S#{sScenario}:A#{originAccount}:V#YTD:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
								Dim mirrorSalesTotalYear As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sYear}:S#{sScenario}:A#{targetAccount}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'								brapi.ErrorLog.LogMessage(si, "openingSalesTotalYear:" & openingSalesTotalYear)		
'							    brapi.ErrorLog.LogMessage(si, "mirrorSalesTotalYear:" &  mirrorSalesTotalYear )	
								
								'Get the transformation multiple to apply to all the base member amounts and declare the calculation formula variable
								Dim transformationMultiple As Decimal = If(mirrorSalesTotalYear = 0, 0, openingSalesTotalYear / mirrorSalesTotalYear)
								Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")	

								CalcSales(si,api,sEntity,mirrorEntity,sScenario,sYear,sPeriod,iMonth,openingDay,isOpeningMonth,transformationMultipleStr,sAccounts)					
																			
							 End If
																
						#End Region
						
						#Region "Sales - Distribution Months by myself"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributeSalesModel") Then							
							
							'Remove "_Aux" from the end of each origin account to get the target parent account
							Dim originAccount As String = "Sales_Aux"
							Dim targetAccount As String = Left(originAccount, Len(originAccount) - 4)																																								
	
							Dim openingSalesMonthInput As Decimal = api.Data.GetDataCell($"E#{sEntity}:T#{sPeriod}:S#{sScenario}:A#Sales_Aux:F#OP_Amount_Adjustment:O#Forms:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
							Dim openingSalesMonth As Decimal = api.Data.GetDataCell($"E#{sEntity}:T#{sPeriod}:S#{sScenario}:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount

'							brapi.ErrorLog.LogMessage(si, "openingSalesMonth :" & openingSalesMonth )		
'							brapi.ErrorLog.LogMessage(si, "openingSalesMonthInput:" &  openingSalesMonthInput )	
'							brapi.ErrorLog.LogMessage(si, "sPeriod:" &  sPeriod )								
							
							'Check if it's open or the opening month
							 If (iMonth >= iForecastMonth AndAlso isOpen AndAlso openingSalesMonthInput <> 0)  Then 	
								
								'Get the transformation multiple to apply to all the base member amounts and declare the calculation formula variable
								Dim transformationMultiple As Decimal = If(openingSalesMonth = 0, 0, openingSalesMonthInput  / openingSalesMonth)
								Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")	
								
'                               brapi.ErrorLog.LogMessage(si, "transformationMultipleStr:" & transformationMultipleStr)
'                                brapi.ErrorLog.LogMessage(si, "iForecastMonth:" &  iForecastMonth )	
								
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
									

								
									'Insert the sales of the opening entity based on the mirror
									Dim updateQuery As String =  $"
									    UPDATE XFC_DailySales
									    SET 
									        
                                            sales= CAST(sales AS FLOAT) * {transformationMultipleStr},  -- Aplica el transformationMultiple a las ventas
									        customers = ROUND(
									            (
									                ISNULL(customers,0)
									                + ISNULL(rem_customers_adj,0)
									                + ISNULL(week_customers_prom_adj,0)
									                + ISNULL(week_customers_camp_adj,0)
									                + ISNULL(week_customers_remgrowth_adj,0)
									                + ISNULL(week_customers_gen1_adj,0)
									                + ISNULL(week_customers_gen2_adj,0)
									                + ISNULL(week_customers_trend_adj, 0)
									            ) * {transformationMultipleStr}, -- Aplica el transformationMultiple a los clientes
									            0
									        )
									    WHERE 
									        unit_code = '{sEntity}'
									        AND scenario = '{sScenario}'
									        AND YEAR(Date) = {sYear}
									        AND MONTH(date) = {iMonth};"

'									BRApi.ErrorLog.LogMessage(si, "updateQuery: " & updateQuery)
									BRApi.Database.ExecuteSql(dbConn, updateQuery, False)
									
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
									
									If (isOpeningMonth OrElse isOpen) And (sScenario = "Budget" OrElse (sScenario = "Forecast" AndAlso iMonth >= iForecastMonth))  Then 
									'When existing rows, loop through all the channels to import the data
										If salesDt.Rows.Count > 0 Then
											
											For Each row As DataRow In salesDt.Rows
												'Get channel, sales and customers
												Dim rowChannel As String = row("channel").ToString.Replace(" ", "_") 'Dimension members are not using spaces
												Dim rowSales As String = If(IsNumeric(row("sales")), row("sales").ToString.Replace(",", "."), "0")
												Dim rowCustomers As String = CInt(row("customers")).ToString
												
	                                            
												Api.Data.ClearCalculatedData($"A#Sales:F#None:O#Forms:U1#{rowChannel}",True,True,True,True)
												Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {rowSales}",True)
'												brapi.ErrorLog.LogMessage(si, "rowSales:" & rowSales)  
                                                
												
												Api.Data.ClearCalculatedData($"A#Customers:F#None:O#Forms:U1#{rowChannel}",True,True,True,True)												
												Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Customers:F#None:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {rowCustomers}",True)
												
											Next
											
											'Only load Own restaurants to PL, no franchises
										    sCalcFormula = $"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#AL_VTA_RES:F#None:O#Forms:I#None:U1#None =
																E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#Sala
																+ E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#Take_Away
																+ E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#Delivery"
											
											Api.Data.ClearCalculatedData($"A#AL_VTA_RES:F#None:O#Forms:U1#None",True,True,True,True)
											If sAccounts = "Own" Then 
												Api.Data.Calculate(sCalcFormula,True) 
											
											Else											
												Api.Data.ClearCalculatedData($"A#Sales:F#None:O#Forms",True,True,True,True)
												Api.Data.ClearCalculatedData($"A#Customers:F#None:O#Forms",True,True,True,True)
												Api.Data.ClearCalculatedData($"A#AL_VTA_RES:F#None:O#Forms:U1#None",True,True,True,True)
											End If 
						              End If

								  End If
						        
						    End Using
							End If
						
			               										
							
																
						#End Region
						
						#Region "Calculate Preopenings"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributePreops") Then
																						
'							    Dim originMemberfilter As String = "AL_PREOPS_Aux.Base"
								Dim originMemberfilter As String = "A#67800202_Aux, A#67800203_Aux, A#67800205_Aux, A#67800208_Aux, A#67800207_Aux, A#67800206_Aux" 

								'Get the origin members
								Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,"Accounts")
								Dim originMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimensionPK, originMemberfilter)
								
'								BRApi.ErrorLog.LogMessage(si, "originMemberList(0).Member.Name:" & originMemberList(0).Member.Name)
								
								'Loop though all the origin members
								For Each originMember As MemberInfo In originMemberList
									
									
									'Remove "_Aux" from the end of each origin member name to get the target parent member name
									Dim originMemberName As String = originMember.Member.Name
									Dim targetParentMemberName As String = Left(originMemberName, Len(originMemberName) - 4)
									
'									BRApi.ErrorLog.LogMessage(si, "iMonth: " & iMonth.ToString )
'									BRApi.ErrorLog.LogMessage(si, "iForecastMonth: " & iForecastMonth.ToString )
									
									If (iMonth >= iForecastMonth And CInt(sYear) = openingYear) Then
										
										If (iMonth = openingMonth Or iMonth = openingMonth - 1 Or iMonth = openingMonth - 2) Then
											
											'Get the amount from the origin and target parent member
											'Define the formula depending on the scenario
											Dim originMemberFormula As String = If(
												sScenario = "Forecast",
												$"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",
												$"E#{sEntity}:T#{sYear}M12:S#{sScenario}:A#{originMemberName}:V#Periodic:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
												- E#{sEntity}:T#{sYear}M12:S#Actual:A#{targetParentMemberName}:V#YTD:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None"
											)
											Dim originMemberAmount As Decimal = api.Data.GetDataCell(originMemberFormula).CellAmount
													
	'										BRApi.ErrorLog.LogMessage(si, "openingMonth: " & openingMonth )										
											
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
											
	'										BRApi.ErrorLog.LogMessage(si, "sCalcFormula: " & sCalcFormula )
											
											'Clear data and calculate if it's open
											Api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
											Api.Data.Calculate(sCalcFormula,True)
									
										Else
											
											'Clear forms data
								     		Api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
											
										End If																				
									
							      Else
								
								     'Clear forms data
								     Api.Data.ClearCalculatedData($"A#{targetParentMemberName}:F#None:O#Forms:U1#None",True,True,True,True)

							 End If 
							 Next

								
						#End Region
						
						#Region "Distribute Cost of Sales"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributeCostSales") Then							
							
							If (iMonth >= iForecastMonth) Then 
								'Get all the base members where the amount must be distributed
								Dim targetAccountName As String = "AL_CVTAS"
								Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,"Accounts")
								Dim originMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimensionPK, targetAccountName)
								Dim nValue As Integer = BRApi.Finance.Members.GetMemberId(si, dimensionPK.DimTypeId,targetAccountName)
								Dim targetBaseMemberList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,dimensionPK,nValue)														
								
							
								'Loop though each target base member to calculate the amount
								For Each targetBaseMember As Member In targetBaseMemberList
									
									'Get the name of the target base member
									Dim targetBaseMemberName As String = targetBaseMember.Name
																		
									'Check if is open
									If isOpen Then 
								        'Get the % of the business case 
								        Dim openingAmount As Decimal = api.Data.GetDataCell($"
											E#{sEntity}:T#{sYear}M12:S#{sScenario}:V#YTD:A#AL_CVTAS_Aux:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											/ E#{sEntity}:T#{sYear}M12:S#{sScenario}:V#YTD:A#Sales_Aux:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											* E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											").CellAmount
										
										'Get the amount of the mirror entity
										Dim mirrorAmount As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:S#{sScenario}:T#{sPeriod}:A#AL_CVTAS:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount

										'Get the transformation multiple to apply to all the base member amounts
										Dim transformationMultiple As Decimal = If(mirrorAmount = 0, 0, openingAmount / mirrorAmount)
									    Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")
										'Clear data and calculate
										Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#None",True,True,True,True)																							
										Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#{targetBaseMemberName}:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																 E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#{targetBaseMemberName}:F#None:O#Top:U1#None * {transformationMultipleStr} ",True)

	                                         
	                                    'Send data to the auxiliar accounts 
                                        Api.Data.Calculate($"A#Condiments:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_CON:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",True)
										
										Api.Data.Calculate($"A#Packaging:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_CPA:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",True)
										
										Api.Data.Calculate($"A#Personnel_Cost:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_CPE:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",True)
										
										Api.Data.Calculate($"A#Merchandising:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_MER:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",True)
										
										Api.Data.Calculate($"A#Adjustments:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#Adjustments:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None",True)
										
										Api.Data.Calculate($"A#Waste:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_DES:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											
											* E#{mirrorEntity}:A#Waste:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None 
											/ E#{mirrorEntity}:A#Waste_Total:F#None:O#Forms:IC#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											",True)	
										
										Api.Data.Calculate($"A#Inventory_Waste:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_DES:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											
											* E#{mirrorEntity}:A#Inventory_Waste:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None 
											/ E#{mirrorEntity}:A#Waste_Total:F#None:O#Forms:IC#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											",True)		
		
										Api.Data.Calculate($"A#Dif_RT:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_AJU:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											
											* E#{mirrorEntity}:A#Dif_RT:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None 
											/ (E#{mirrorEntity}:A#Dif_RT:F#None:O#Forms:IC#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
												+
												E#{mirrorEntity}:A#Adjustments:F#None:O#Forms:IC#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None)
											",True)												
										
										Api.Data.Calculate($"A#Adjustments:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											A#AL_CV_AJU:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											
											* E#{mirrorEntity}:A#Adjustments:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None 
											/ (E#{mirrorEntity}:A#Dif_RT:F#None:O#Forms:IC#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
												+
												E#{mirrorEntity}:A#Adjustments:F#None:O#Forms:IC#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None)
											",True)															
										
'										Api.Data.Calculate($"A#Dif_RT:F#Perc_Final_Result:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
'												A#Dif_RT:F#Perc_Actual:O#Top:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
'											")										
										
										'Loop through channels
										For Each channel As String In channelList

											Api.Data.Calculate($"E#{sEntity}:A#Theoretical_Costs:F#None:O#Forms:IC#None:U1#{channel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = 
											E#{sEntity}:A#AL_CV_CCB:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											
											* E#{mirrorEntity}:A#Theoretical_Costs:F#None:O#Forms:IC#None:U1#{channel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None 
											/
											E#{mirrorEntity}:A#Theoretical_Costs:F#None:O#Forms:IC#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											",True)
										Next
																															
									Else
												
										'Clear forms data for actuals
										Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#None",True,True,True,True)																				
													 
									End If
 
							Next
						
						Else
							
'							Dim sCalcDifRT As String = $"A#Dif_RT:F#None:U1#None = A#AL_CVTAS:F#None:U1#Top - A#Cost_of_Sales_Aux:F#None:U1#Top + A#Merchandising:F#None:U1#Top"
							Dim sCalcDifRT As String = $"A#Dif_RT:F#None:U1#None = 
															A#AL_CVTAS:F#None:U1#Top
															- A#Theoretical_Costs:F#None:U1#Top
															- A#AL_CV_CPA:F#None:U1#Top
															- A#AL_CV_CON:F#None:U1#Top
															- A#Personnel_Cost:F#None:U1#Top
															- A#Waste:F#None:U1#Top
															- A#Inventory_Waste:F#None:U1#Top
															- A#Adjustments:F#None:U1#Top"
							
							Api.Data.ClearCalculatedData($"A#Dif_RT:F#None",True,True,True,True)									
							Api.Data.Calculate(sCalcDifRT,True)									
							
'							If sEntity = "SF011173" Then
'								BRApi.ErrorLog.LogMessage(si, "sCalcDifRT: " & sCalcDifRT)	
'							End If

						End If
						
						Dim sCalcPercFinal As String = $"A#Dif_RT:F#Perc_Final_Result:U1#None:O#None = A#Dif_RT:F#None:U1#None:O#Top / A#Sales:F#None:U1#Top:O#Top * 100"					
						Api.Data.ClearCalculatedData($"A#Dif_RT:F#Perc_Final_Result",True,True,True,True)	
						Api.Data.Calculate(sCalcPercFinal,True)		
'						BRApi.ErrorLog.LogMessage(si, "sCalcPercFinal: " & sCalcPercFinal)	
				
					#End Region
					
						#Region "Distribute Cost of Personnel"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributeCostPersonnel") Then
						
							'Get all the base members where the amount must be distributed
							Dim targetAccountName As String = "AL_CPERS"
							Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,"Accounts")
							Dim originMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimensionPK, targetAccountName)
							Dim nValue As Integer = BRApi.Finance.Members.GetMemberId(si, dimensionPK.DimTypeId,targetAccountName)
							Dim targetBaseMemberList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,dimensionPK,nValue)														
							
							'Modify transformation multiple if it's opening month and not forecast
							If isOpeningMonth Then percOpeningMonth.ToString.Replace(",", ".")
														
							'Loop though each target base member to calculate the amount
							For Each targetBaseMember As Member In targetBaseMemberList
								
								'Get the name of the target base member
								Dim targetBaseMemberName As String = targetBaseMember.Name
									
								'Check if the month should be calculated or not depending on the scenario and month
								If  isOpen And  iMonth >= iForecastMonth Then 

										'Get the % of the business case 
								        Dim openingAmount As Decimal = api.Data.GetDataCell($"
											E#{sEntity}:T#{sYear}M12:S#{sScenario}:V#YTD:A#AL_CPERS_Aux:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											/ E#{sEntity}:T#{sYear}M12:S#{sScenario}:V#Periodic:A#Sales_Aux:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											* E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
											").CellAmount
										
										'Get the amount of the mirror entity
										Dim mirrorAmount As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#{targetBaseMemberName}:F#None:O#Top:I#None:U1#Top:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
										Dim mirrorAmountTotal As Decimal= api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#AL_CPERS:F#None:O#Top:I#None:U1#Top:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
										
										'Get the transformation multiple to apply to all the base member amounts
										Dim transformationMultiple As Decimal = If(mirrorAmount = 0, 0, mirrorAmount / mirrorAmountTotal)
									    Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")	
										Dim openingAmountStr As String = openingAmount.ToString.Replace(",", ".")	
										

										'Clear data and calculate
										Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#None",True,True,True,True)																							
										Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#{targetBaseMemberName}:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																{openingAmountStr} * {transformationMultipleStr} * 1 ",True)
										    
											
								Else											
									'Clear forms data for actuals
									Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
								 	 
								End If
									
								
							Next	 
					
							
							
						#End Region
																		
						#Region "Distrubute P&L- % Business  Case"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("DistributePL") Then
						     
							
							Dim originMemberfilter As String = ("A#AL_PUB_Aux, A#AL_OI_ROY_Aux, A#AL_ROY_Aux,A#AL_OING_Aux, A#AL_GG_SUM_Aux, A#AL_GG_IT_Aux, A#AL_GG_OPS_Aux, A#AL_GG_EXT_Aux, A#AL_GG_MANT_Aux, A#AL_GG_VEH_Aux, A#AL_GG_VIA_Aux, A#AL_GG_OTR_Aux")
							
							'Get the origin members
							Dim dimensionPK As DimPk = BRApi.Finance.Dim.GetDimPk(si,"Accounts")
							Dim originMemberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(dimensionPK, originMemberfilter)
							
							'Loop though all the origin members
							For Each originMember As MemberInfo In originMemberList
								
								'Remove "_Aux" from the end of each origin member name to get the target parent member name
								Dim originMemberName As String = originMember.Member.Name
								Dim targetParentMemberName As String = Left(originMemberName, Len(originMemberName) - 4)
'								'Get all the base members where the amount must be distributed
								Dim nValue As Integer = BRApi.Finance.Members.GetMemberId(si, dimensionPK.DimTypeId,targetParentMemberName)
								Dim targetBaseMemberList As List(Of Member) = BRApi.Finance.Members.GetBaseMembers(si,dimensionPK,nValue)
																					
								'Modify transformation multiple if it's opening month and not forecast
								If isOpeningMonth Then percOpeningMonth.ToString.Replace(",", ".")
															
								'Loop though each target base member to calculate the amount
								For Each targetBaseMember As Member In targetBaseMemberList
									
'									'Get the name of the target base member
									Dim targetBaseMemberName As String = targetBaseMember.Name
										
									'Check if the month should be calculated or not depending on the scenario and month
									If isOpen And iMonth >= iForecastMonth Then 
										
										Dim validTargetBaseMembers As List(Of String) = New List(Of String) From {"62300020", "62300023", "62300024"}
                                        Dim validOriginMembers As List(Of String) = New List(Of String) From {"AL_PUB_Aux", "AL_OI_ROY_Aux", "AL_ROY_Aux"}

                                        If validTargetBaseMembers.Contains(targetBaseMemberName) Or validOriginMembers.Contains(originMemberName) Then '% Business Case
	
											    'Get the % of the business case 
										        Dim openingAmount As Decimal = api.Data.GetDataCell($"
													E#{sEntity}:T#{sYear}M12:S#{sScenario}:V#YTD:A#{originMemberName}:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
													/ E#{sEntity}:T#{sYear}M12:S#{sScenario}:V#YTD:A#Sales_Aux:F#OP_Amount:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
													* E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None
													").CellAmount
												
												'Get the amount of the mirro entity
												Dim mirrorAmount As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#{targetParentMemberName}:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
												
												'Get the transformation multiple to apply to all the base member amounts
												Dim transformationMultiple As Decimal = If(mirrorAmount = 0, 0, openingAmount / mirrorAmount)
											    Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")																								
		
												'Clear data and calculate
												Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#None",True,True,True,True)																							
												Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#{targetBaseMemberName}:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																		 E#{mirrorEntity}:S#{sScenario}:A#{targetBaseMemberName}:F#None:O#Top:U1#None * {transformationMultipleStr} ",True)
		
		
														'Clear data and calculate
'														Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
'														Api.Data.Calculate(sCalcFormula,True)
			'											brapi.ErrorLog.LogMessage(si, "sCalcFormula:" & sCalcFormula)
                                               
									     Else '€ Business Case
											
										        Dim targetMirrorBaseAmount As String = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:IC#None:A#{targetBaseMemberName}:F#None:O#Top:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount								    

										        Dim targetMirrorParentAmount As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:IC#None:A#{targetParentMemberName}:F#None:O#Top:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
		                                        Dim transformationMultiple As Decimal = If(targetMirrorParentAmount = 0, 0,targetMirrorBaseAmount  / targetMirrorParentAmount)
												
												Dim transformationMultipleStr As String= transformationMultiple.ToString.Replace(",", ".")
												
												Dim openingParentAmount As Decimal = api.Data.GetDataCell($"E#{sEntity}:T#{sYear}M12:S#{sScenario}:IC#None:A#{originMemberName}:F#OP_Amount:O#Forms:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None / 12").CellAmount
												Dim openingParentAmountStr As String= openingParentAmount.ToString.Replace(",", ".")
'												brapi.ErrorLog.LogMessage(si, "openingParentAmountStr:" & openingParentAmountStr)		
'												brapi.ErrorLog.LogMessage(si, "targetMirrorParentAmount:" & targetMirrorParentAmount)	
'												brapi.ErrorLog.LogMessage(si, "targetMirrorBaseAmount:" & targetMirrorBaseAmount)	
												
												'Modify transformation multiple if it's opening month and not forecast
											    If isOpeningMonth Then transformationMultipleStr = (transformationMultiple * percOpeningMonth).ToString.Replace(",", ".")
													sCalcFormula = $"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#{targetBaseMemberName}:F#None:O#Forms:IC#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																			 {openingParentAmountStr} * {transformationMultipleStr}  "
													'Clear forms data for actuals
											        Api.Data.ClearCalculatedData($"A#{targetBaseMemberName}:F#None:O#Forms:U1#None",True,True,True,True)
										            Api.Data.Calculate(sCalcFormula,True)
												
									End If	
								
								End If
							
						     Next
					Next
					  		
							
							
						#End Region
																									
						#Region "Calculate Hours"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CalculateHours") Then
						         							
								Dim accounts As String () = {"total_hs", "holidays", "supplementary_hs", "productive_hs_total", "holidays", "absences", "absances_sanctions", "training", "unpaid_leave", "paid_leave", "overtime", "IT_long_term_leave"}
											
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim selectQuery As String = $"	SELECT
																	DAY(od.OpenDate) AS day,
																	MONTH(od.OpenDate) AS month,
																	YEAR(od.OpenDate) AS year,
																	Mirror
																FROM XFC_Opening o
																INNER JOIN XFC_OpenDate od ON o.cebe = od.cebe
																WHERE o.CeBe = '{sEntity}' AND o.Year = {sYear} AND o.Scenario = '{sScenario}'"
								
								Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
								
								'If there is no opening date for that entity don't do anything
								If dt.Rows.Count > 0 AndAlso dt.Rows(0)("year") <> 1900 Then
									'Get date
									mirrorEntity = dt.Rows(0)("Mirror")
							    End If
									'Build a dictionary to send the parameters to a DM
									Dim customSubstVars As New Dictionary(Of String, String) From {{"p_mirror_entity", mirrorEntity}}
								End Using
								
							
								
								'Get the hour cost from the mirror entity
								Dim totalcostmirror As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:V#Periodic:S#{sScenario}:IC#None:A#AL_CPERS:F#None:O#Top:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
								Dim totalhoursmirror As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:V#Periodic:S#{sScenario}:IC#None:A#total_hs:F#None:O#Top:U1#None:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
              
								Dim hourcostmirror As Decimal= If(totalhoursmirror = 0, 0, totalcostmirror / totalhoursmirror)
							
								'Get the amount of cost of personnel
								Dim personnelCostOrigin As Decimal = api.Data.GetDataCell($"E#{sEntity}:T#{sPeriod}:S#{sScenario}:A#AL_CPERS:F#None:O#Top:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
								
                                Dim totalhsOrigin As Decimal = If(hourcostmirror = 0, 0, personnelCostOrigin / hourcostmirror)

								Dim totalhsOriginStr As String = Decimal.Abs(totalhsOrigin).ToString.Replace(",", ".")


								'Define each sCategory
								Dim categories As String() = {"Management", "Base", "Distributors", "SM", "ASM", "SSV", "Barista"}
								
								If isOpen And iMonth >= iForecastMonth Then 
									
                                    'Loop though all the personnel accounts
									For Each account In accounts 

	
										'Loop though all the categories
										For Each sCategory As String In categories
										
											Dim hoursmirror As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#{account}:F#None:O#Top:I#None:U1#None:U2#{sCategory}:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
'											Dim totalhoursmirror As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#total_hs:F#None:O#Top:I#None:U1#None:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount

											Dim transformationMultiple As Decimal = If(totalhoursmirror = 0, 0, hoursmirror / totalhoursmirror)
											Dim transformationMultipleStr As String = transformationMultiple.ToString.Replace(",", ".")
                                            							
												' Build the formula for each category
												sCalcFormula = $"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#{account}:F#None:O#Forms:I#None:U1#None:U2#{sCategory}:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =
																{totalhsOriginStr} * {transformationMultipleStr} * 1 "
												
												' Execute the formula
											
												Api.Data.ClearCalculatedData($"A#{account}:F#None:O#Forms:U2#{sCategory}",True,True,True,True)
												Api.Data.Calculate(sCalcFormula, True)


                                 
							       Next
							   Next
	   
							   
						   End If 
						
						 #End Region
						 
						#Region "Calculate Productivity"
						 
						 Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("CalculateProductivity") Then
						 
						     If isOpen And iMonth >= iForecastMonth Then 
							  
							   'Get Avarege Ticket of the mirror entity	
							   Dim salesMirror As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
							   Dim clientsMirror As Decimal = api.Data.GetDataCell($"E#{mirrorEntity}:T#{sPeriod}:S#{sScenario}:A#Customers:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
							   Dim avgTicketMirror As Decimal= If(clientsMirror = 0, 0, salesMirror / clientsMirror) 
							   Dim avgTicketMirrorStr As String = avgTicketMirror.ToString.Replace(",", ".")
							   'Get the customers of the opening
							   Dim openingSales As Decimal= api.Data.GetDataCell($"E#{sEntity}:T#{sPeriod}:S#{sScenario}:A#Sales:F#None:O#Top:I#None:U1#Top:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
							   Dim openingSalesStr As String = openingSales.ToString.Replace(",", ".")
							   Dim customersOpening As Decimal= avgTicketMirror * openingSales  
							   Dim productivehs As Decimal= api.Data.GetDataCell($"E#{sEntity}:T#{sPeriod}:S#{sScenario}:A#productive_hs_total:F#None:O#Top:I#None:U1#Top:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount
							   Dim productivehsStr As String = productivehs.ToString.Replace(",", ".")
							   
							   Api.Data.ClearCalculatedData($"A#Productivity:F#None:O#Forms:U1#Top",True,True,True,True) 
							   Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Productivity:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None=
							              ( {openingSalesStr} *1 /  {avgTicketMirrorStr} * 1) /  {productivehsStr} * 1",True)

						   End If
						   
					   #End Region
				   						
					    #Region "Clear Opening"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearOpening") Then
							'Clean up first if data exists
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								Dim deleteQuery As String = $"	DELETE 
																FROM XFC_DailySales
																WHERE
																	unit_code = '{sEntity}'
																	AND scenario = '{sScenario}'
																	AND year = {sYear}"
								BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
							End Using
							'Reference admin business rule to get functions
							Dim AdminBR As New OneStream.BusinessRule.Finance.ADMIN_Calculates.MainClass
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.Name},A#AL_EBITDAR.Base,A#AL_PREOPS.Base,A#AL_EXTRA.Base,A#AL_AMORT,A#AL_W_OFF.Base,A#Sales,A#Customers,A#AL_VENTAS)")
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							AdminBR.ClearDataBuffer(si, api, targetDataBuffer, cleanExpDestInfo)
							
						#End Region		
						
					    #Region "Clear Opening Without Sales"
						
						Else If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("ClearOpeningWOSales") Then
							'Reference admin business rule to get functions
							Dim AdminBR As New OneStream.BusinessRule.Finance.ADMIN_Calculates.MainClass
							
							'Generate a hash set with sales account ids to filter out in the data buffer loop
							Dim memberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Accounts", "A#AL_VENTAS.Base", True)
							Dim memberIdHashSet As New HashSet(Of Integer)
							For Each member In memberList
								memberIdHashSet.Add(member.Member.MemberId)
							Next
							
							'Clear target data buffer
							Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(S#{api.Pov.Scenario.Name},A#AL_EBITDAR.Base,A#AL_PREOPS.Base,A#AL_EXTRA.Base,A#AL_AMORT,A#AL_W_OFF.Base)")
							Dim cleanExpDestInfo As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("")
							'Build clean data buffer
							Dim cleanDataBuffer As New DataBuffer
							For Each targetDataBufferCell In targetDataBuffer.DataBufferCells.Values
								If Not memberIdHashSet.Contains(targetDataBufferCell.DataBufferCellPk.AccountId) And Not targetDataBufferCell.CellStatus.IsNoData Then
									targetDataBufferCell.CellAmount = 0
									targetDataBufferCell.CellStatus = New DataCellStatus(targetDataBufferCell.CellStatus, DataCellExistenceType.NoData)
									cleanDataBuffer.SetCell(si, targetDataBufferCell)
								End If
							Next
							'Clean data buffer
							api.Data.SetDataBuffer(cleanDataBuffer, cleanExpDestInfo,,,,,,,,,,,,,True)
							
						#End Region							
					   
					End If
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function						
		
		#Region "Function - Calc Sales"
			
					Public Sub CalcSales(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal sEntity As String, ByVal mirrorEntity As String, ByVal sScenario As String, ByVal sYear As String, ByVal sPeriod As String, ByVal iMonth As Integer, ByVal openingDay As Integer, ByVal isOpeningMonth As Boolean, ByVal transformationMultipleStr As String, ByVal sAccounts As String)
		
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
									
									BRApi.ErrorLog.LogMessage(si, "insertQuery: " & insertQuery)
									BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
									
									'Get the sales and customers of the opening entity grouped by period and channel
									selectQuery = $"
										SELECT channel, SUM(sales) AS sales, SUM(customers) AS customers
										FROM XFC_DailySales
										WHERE
											unit_code = '{sEntity}'
											AND scenario IN ('Actual','{sScenario}')
											AND year = {sYear}
											AND MONTH(date) = {iMonth}
											AND Channel <> ''
										GROUP BY
											channel"
									
									Dim salesDt As DataTable = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
'									BRApi.ErrorLog.LogMessage(si, "selectQuery: " & selectQuery)

									'When existing rows, loop through all the channels to import the data
									If salesDt.Rows.Count > 0 Then
										
										For Each row As DataRow In salesDt.Rows
											'Get channel, sales and customers
											Dim rowChannel As String = row("channel").ToString.Replace(" ", "_") 'Dimension members are not using spaces
											
											Dim rowSales As String = If(IsNumeric(row("sales")), row("sales").ToString.Replace(",", "."), "0")
											Dim rowCustomers As String = CInt(row("customers")).ToString
											
											Api.Data.ClearCalculatedData($"A#Sales:F#None:O#Forms:U1#{rowChannel}",True,True,True,True)
											Api.Data.ClearCalculatedData($"A#Sales_Aux:F#None:O#Forms:U1#{rowChannel}",True,True,True,True)
											Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales:F#None:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {rowSales}",True)
											Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales_Aux:F#OP_Amount:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {rowSales}",True)
'											brapi.ErrorLog.LogMessage(si, "Data Calculate:" & ($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Sales_Aux:F#OP_Amount:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {rowSales}" ))
											Api.Data.ClearCalculatedData($"A#Customers:F#None:O#Forms:U1#{rowChannel}",True,True,True,True)
											Api.Data.Calculate($"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#Customers:F#None:O#Forms:I#None:U1#{rowChannel}:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = {rowCustomers}",True)
											
										Next
										
										'Only load Own restaurants to PL, no franchises
										Dim sCalcFormula As String = $"E#{sEntity}:S#{sScenario}:T#{sPeriod}:A#AL_VTA_RES:F#None:O#Forms:I#None:U1#None =
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
								
							End Sub
							
						#End Region
		
	End Class
						
End Namespace