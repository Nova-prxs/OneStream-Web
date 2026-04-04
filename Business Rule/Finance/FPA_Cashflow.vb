Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

Namespace OneStream.BusinessRule.Finance.FPA_Cashflow
	Public Class MainClass
'		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
'			Try

		Dim ActiveLogs As Boolean = False
	
		#Region "Main"
			Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
				Try
					Select Case api.FunctionType
					End Select
					Return Nothing
				Catch ex As Exception
					Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				End Try
			End Function
		#End Region
		
				Public Function FPACashflowCalc(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal str_FPA_CF_Account As String) As String
				Try	
					
'''''''''''''''''''''''''''''''''''''''''''''''' Variable Declaration ''''''''''''''''''''''''''''''''''''''''''''''''''' ''' 
					
	'				Dim BoolElim As Boolean = api.Cons.IsForeignCurrencyForEntity
	
					Dim Source As String = Nothing
					Dim Source2 As String = Nothing
					Dim Source3	 As String = Nothing
					Dim Source4	 As String = Nothing
					Dim Destination As String = Nothing
					Dim Formula As String = Nothing
					Dim AccountFilter As String = Nothing

					Dim BoolLocalCurrency As Boolean = api.Cons.IsLocalCurrencyForEntity
					Dim BoolForeignCur As Boolean = api.Cons.IsForeignCurrencyForEntity
					Dim BaseEntity As Boolean = Not api.Entity.HasChildren
					'19 November 2024, AN: cashflow should only be calculated third party for the source. 
'					Dim TopPOVSource As String = ":V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top"
					Dim TopPOVSource As String = ":V#YTD:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#Top"
					Dim TopPOVDestination As String = ":V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None"
					
					Dim PovScenario As String = api.Pov.Scenario.Name
					
			If PovScenario.Contains("Q_FCST") Or PovScenario.Contains("YP") Or PovScenario.Contains("Budget") Then
					
'''''''''''''''''''''''''''''''''''''''''''''''' End Variable Declaration '''''''''''''''''''''''''''''''''''''''''''''''
					
				Select Case str_FPA_CF_Account
					
					#Region "______________Financing________9_1______"
						Case "9_1"
							'CF Financing = (A#2_2_2_5:F#closing - A#2_2_2_5:F#opening) +(A#2_2_1_5:F#closing - A#2_2_1_5:F#opening) YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims Not mentioned. 
							If BoolLocalCurrency
'								Formula: Formula To specific codes In data staging area
'								Formula Pass 9
'								Source = $"A#2_2_2_5:F#F_CLO{TopPOVSource}-A#2_2_2_5:F#F_OPE{TopPOVSource}"
'								Source2 = $"A#2_2_1_5:F#F_CLO{TopPOVSource}-A#2_2_1_5:F#F_OPE{TopPOVSource}"
'								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros(-{source}+{source2})"
								'update to use movement of the month instead clo-ope because if loan is repaid closing becomes 0 and 0-0 doesnt represent the mov
								Source = $"(A#2_2_2_5:F#F_TotMov{TopPOVSource}:V#periodic)"
								Source2 = $"(A#2_2_1_5:F#F_TotMov{TopPOVSource}:V#periodic)"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}:V#periodic"
								Formula = $"{Destination}=RemoveZeros(-({source}+{source2}))"
								
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
								
								
							End If				
					#End Region	
					
					#Region "______________M&A CAPEX____9_2_1__________"
'					M&A capex =  (A#2_2_2_7:F#F_Clo:UD5#Top -A#2_2_2_7:F#F_Ope:UD5#Top) +   (A#740000:F#F_Clo:UD5#Top -A#740000:F#F_Ope:UD5#Top) +  (A#820000:F#F_Clo:UD5#Top -A#820000:F#F_Ope:UD5#Top) 
					
						Case "9_2_1"
							If BoolLocalCurrency
'								Formula Pass 9
								Source = $"(A#2_2_2_7:F#F_Clo{TopPOVSource} -A#2_2_2_7:F#F_Ope{TopPOVSource})" 'other curr fin liab
'								Source2 = $"(A#740000:F#F_Clo{TopPOVSource} -A#740000:F#F_Ope{TopPOVSource})"
								Source3 = $"(A#820000:F#F_Clo{TopPOVSource} -A#820000:F#F_Ope{TopPOVSource})"  'deferred cons non curr
								'1_1_2
'								Source4 = $"(A#1_1_2:F#F_Goodwill_Mov_5{TopPOVSource})"  
								'24Jul24 update to pull full goodwill movement
								Source4 = $"(A#1_1_2:F#F_Clo{TopPOVSource} -A#1_1_2:F#F_Ope{TopPOVSource})"  'goodwill
								'18NOV2024, AN deducted account: 750500 Accrued interesests
								Source2 = $"(A#750500:F#Top{TopPOVSource} -A#750500:F#F_Ope{TopPOVSource})"  'goodwill
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source}+{source2}+{source3}+{source4})"
'								Formula = $"{Destination}=RemoveZeros({source}+{source3}-{source4})"
								Formula = $"{Destination}=RemoveZeros(-{source}-{source3}-{source4}+{source2})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If


					#End Region	
					
					#Region "______________Financial Lease_____9_2_2_1_________"
						Case "9_2_2_1"
'							 {TopPOVSource} CF FinLeases Propex =  -(A#731100:F#Closing:UD5#P - A#731100:F#Opening:UD5#P); YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims Not mentioned. 
	                        If BoolLocalCurrency And BaseEntity Then
'	                            Source = $"(A#731100:F#F_CLO:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Project_P - A#731100:F#F_OPE:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Project_P)"
'	                            Source = $"(A#731100:F#F_TotMov:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Project_P:UD7#TOP - A#731100:F#F_TotMov:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Project_P:UD7#IT)"
	                            'BERO 30Jul24 updated to pull only PAYMENT flow
								Source = $"(A#731100:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Project_P:UD7#TOP - A#731100:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Project_P:UD7#IT )"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None"
	                            Formula = $"{Destination} = RemoveZeros(-({Source}))"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
	                        End If

					#End Region	
					
					#Region "______________Operating Leases______9_2_2_2________"
					
'					CF OpLeases Propex = #8_1_1_1_1_3_1_3_1_1:F#Top:UD1#LFL (ud1 base med centers with text1 = povyear orselse text1= povyear-1)
						Case "9_2_2_2"
'							brapi.ErrorLog.LogMessage(si, "Scenario >>>>>> " & povScenario )
							If Not api.POV.Scenario.Name.ToString = "Actual" Then
							If BoolLocalCurrency And BaseEntity and not api.pov.Entity.Name.XFContainsIgnoreCase("_Country") Then
							If api.pov.Entity.Name.XFContainsIgnoreCase("_Input_OP")
							        ' First we get the POV year as a string (ex. "2024")
							        Dim povYear As Integer = Api.Time.GetYearFromId(api.Pov.Time.MemberId)
							        Dim povPriorYear As Integer = Api.Time.GetYearFromId(api.Pov.Time.MemberId) - 1
'									brapi.ErrorLog.LogMessage(si, "povYEAR> " & povYear & "   povPriorYear> " & povPriorYear)
'									brapi.ErrorLog.LogMessage(si, "IS CALCULATING 9222 >>>>>> " & povScenario )
							        ' Second we get the dimension ID in DimPk format to feed the getbasemembers expression
									
									Dim sCountry As String = ""
									Dim sWorkflowName As String = api.pov.entity.Name
									If sWorkflowName.Contains("_") Then
										sCountry = sWorkflowName.Split("_")(0)
											Else
										sCountry = ""
									End If
									
									''''
									If scountry = "Czech" Then
										scountry= "Czech_Republic"
									Else If scountry  = "Northern" Then
										scountry= "Northern_Ireland"
									End If 
									
									Dim UD1PK As String = $"UD1_200_{scountry}"
									Dim TopUd1 As String = scountry
									
									If scountry = "Ireland" Then
										UD1PK= $"UD1_200_{scountry}"
										TopUd1= "Republic_of_Ireland"
									Else If scountry = "Northern_Ireland" Then
										UD1PK= "UD1_200_Ireland"
										TopUd1= "Northern_Ireland"
									Else If scountry = "UK" Then
										UD1PK= $"UD1_200_{scountry}"
										TopUd1= "United_Kingdom"
									End If
									''''
'									brapi.ErrorLog.LogMessage(si, "sCountry> " & sCountry & "   sWorkflowName> " & sWorkflowName)

'									Dim UD1DimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, $"UD1_200_{scountry}")			
									Dim dimensionID As DimPk = BRApi.Finance.Dim.GetDimPk(si, UD1PK)		
'									brapi.ErrorLog.LogMessage(si, "Dimension ID " & dimensionID.ToString )
'									' Create the dimension name string separately to check its value
'									Dim dimensionName As String = $"UD1_200_{sCountry}"
'									Console.WriteLine(dimensionName) ' Debugging: Print the dimension name to ensure it's correct

'									' Call the GetDim method with the interpolated string
'									Dim dimensionID As DimPk = api.Dimensions.GetDim(dimensionName).DimPk
									
									
'							        Dim dimensionID As DimPk = api.Dimensions.GetDim($"UD1_200_Spain").DimPk

							        ' Third get the member list of UD1 base members
									
							        Dim ud1BaseMembers As IList(Of Member) = api.Members.GetBaseMembers(dimensionID, api.Members.GetMemberId(dimtype.UD1.Id, TopUd1) )

									 If ActiveLogs Then 
										' Process the filtered members as needed
								        LogFilteredMemberIDs(si, ud1BaseMembers)
									End If
									
							        ' Use the FilterMembersByText1 function to get the filtered list for the current and prior year
							        Dim filteredMembers As IList(Of Member) = FilterMembersByText1(api, ud1BaseMembers, povYear, povPriorYear)
	
							        If ActiveLogs Then 
										LogFilteredMemberIDs(si, filteredMembers) 
								End If
									
								Dim SourceCount As Integer = 0
'									For Each MemberInList As Member In filteredMembers 
'										Dim memberName As String = MemberInList.Name
'										Source += $"+(A#613000:F#FPA_MOV_Tech:V#YTD:O#Top:I#None:U1#{memberName}:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top)"
'										SourceCount += 1
'										BRApi.ErrorLog.LogMessage(si, "SourceCount " & SourceCount & " Source " & Source)
'									Next

								If Not filteredMembers.Count = 0 Then 

									For Each MemberInList As Member In filteredMembers 
									    Dim memberName As String = MemberInList.Name
										
									    If SourceCount > 0 Then
									        source += " + "
									    End If
										
									    source += $"(-A#8_1_1_1_1_4_1_3_1_1:F#Top:V#YTD:O#Top:I#Top:U1#{memberName}:U2#Top:U3#Top:U4#Top)"
									    SourceCount += 1
'									    BRApi.ErrorLog.LogMessage(si, "SourceCount " & SourceCount & " Source " & source)
										'13Sep24 BeRo updated To IC top In source
									Next
										Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None"
										Formula = $"{Destination}=RemoveZeros({source})"
										api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
									
								End If		
									
							End If
						End If
						End If
					#End Region	
					
					#Region "______________Cash purchase______9.2.2.3________"
'					CF CashPurchases Propex= A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:UD5#P:UD7#Top
'					-A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:UD5#P:UD7#IT; YTD calculation; UD1TOP,UD2top,UD3top,UD4#None; other dims Not mentioned. 
						Case "9_2_2_3"
							If BoolLocalCurrency
'								Formula Pass 9
'								Source = $"A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#P:U7#Top"
'								Source2 = $"A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#P:U7#IT"
'								Source = $"A#Tech_Payments:F#None:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#P:U7#Top"
'								Source2 = $"A#Tech_Payments:F#None:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#P:U7#IT" F_Leases_ST_LT_Mov_3
								'Cb#Cube_200_Romania:E#Romania_Input_OP:P#?:C#RON:S#Budgetv1:T#2025M2:V#Periodic:A#780000:F#F_Leases_ST_LT_Mov_3:O#Import:I#None:U1#E819:U2#19:U3#None:U4#None:U5#M:U6#RO049:U7#IT:U8#Pre_IFRS16_Plan
								Source = $"A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#Project_P:U7#Top"
								Source2 = $"A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#Project_P:U7#IT"
					
'								Source = $"A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#Project_P:U7#Top"
'								Source2 = $"A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#Project_P:U7#IT-A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#M:U7#IT"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#none"
								Formula = $"{Destination}=RemoveZeros(-({source}-{source2}))"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If

					#End Region	
					
					#Region "______________IT______9.2.2.4________"
'					CF IT propex = A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:UD5#P:UD7#IT; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims Not mentioned. 
						Case "9_2_2_4"
							If BoolLocalCurrency
'								Formula: Formula To specific codes In data staging area
'								Formula Pass 9
								Source = $"A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Project_P:UD7#IT"
								Source2 = $"A#731100:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#Project_P:U7#IT"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:UD7#None"
								Formula = $"{Destination}=RemoveZeros(-({source}+{source2}))"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If

					
					#End Region	
					
					#Region "______________Tax _______9_2_3_0_______"
						Case "9_2_3_0"
							If BoolLocalCurrency
'								Source = $"A#8_1_2_1_1_2_1_1:F#Top{TopPOVSource} 
'								+ A#1_1_9:F#F_Clo{TopPOVSource} 
'								- A#1_1_9:F#F_Ope{TopPOVSource} 
'								+ A#2_2_2_3:F#F_CIT_receivable_payable_Mov_3{TopPOVSource}
'								-(A#2_2_1_3:F#F_Clo{TopPOVSource} 
'								- A#2_2_1_3:F#F_Ope{TopPOVSource})"
								
								'02Jul2024 Change to ONLY include tax payment and not other accounts 
								'18Jul2024 change to pull periodic value of payments
'								 TopPOVSource = ":V#Periodic:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#Top"
'								 TopPOVDestination  = ":V#Periodic:O#Import:I#None:U1#None:U2#None:U3#None:U4#None"
								 
'								Source = $"A#2_2_2_3:F#F_CIT_receivable_payable_Mov_3{TopPOVSource}"
								Source = $"(A#2_2_2_3:F#F_CIT_receivable_payable_Mov_3{TopPOVSource} + A#2_2_2_3:F#FPA_MOV_TECH{TopPOVSource}) - A#2_2_2_3:F#F_CIT_receivable_payable_Mov_1{TopPOVSource}:O#FORMS" ' 05Nov24 update to pull manual additions and payments movement instead of payments only, adding also additions decreases manual mov (o#forms only)
								
								
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros(-{source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If		
							

					#End Region	
					
					#Region "______________ Others_______9_2_3_1_______"
'					CF tax_others =((A#1_1_5:F#Closing - A#1_1_5:F#opening)+(A#1_1_7:F#Closing - A#1_1_7:F#opening)+(A#1_1_11:F#Closing - A#1_1_11:F#opening)+(A#1_1_12:F#Closing - A#1_1_12:F#opening)+(A#1_2_7:F#Closing - A#1_2_7:F#opening) + (A#1_2_8:F#Closing - A#1_2_8:F#opening) + (A#2_1:F#Closing - A#2_1:F#opening) + (A#2_2_1_1:F#Closing - A#2_2_1_1:F#opening) + (A#2_2_1_2:F#Closing - A#2_2_1_2:F#opening) + (A#2_2_1_7:F#Closing - A#2_2_1_7:F#opening) - A#8_1:F#Top + A#8_1_2_1_1_2_1_1:F#Top) + ( A#8_1_2_1_1_3_1_1:F#Top + (A#1_1_9:F#Closing - A#1_1_9:F#opening) + (A#2_2_1_3:F#closing - A#2_2_1_3:F#opening)); YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims not mentioned.
						Case "9_2_3_1"
							If BoolLocalCurrency
								 '04Sep24 request to change sign of assets and liab, added - sign before brackets of assets, and - for liab.  
'								Source = $"((A#1_1_5:F#F_Clo{TopPOVSource} 
'								- A#1_1_5:F#F_Ope{TopPOVSource}
'								+ A#1_1_7:F#F_Clo{TopPOVSource} 
'								- A#1_1_7:F#F_Ope{TopPOVSource}
'								+ A#1_1_11:F#F_Clo{TopPOVSource} 
'								- A#1_1_11:F#F_Ope{TopPOVSource}
'								+ A#1_1_12:F#F_Clo{TopPOVSource} 
'								- A#1_1_12:F#F_Ope{TopPOVSource}
'								+ A#1_2_7:F#F_Clo{TopPOVSource} 
'								- A#1_2_7:F#F_Ope{TopPOVSource} 
'								+ A#1_2_8:F#F_Clo{TopPOVSource} 
'								- A#1_2_8:F#F_Ope{TopPOVSource}) 
'								-(A#2_1:F#F_Clo{TopPOVSource} 
'								- A#2_1:F#F_Ope{TopPOVSource} 
'								+ A#2_2_1_1:F#F_Clo{TopPOVSource} 
'								- A#2_2_1_1:F#F_Ope{TopPOVSource} 
'								+ A#2_2_1_2:F#F_Clo{TopPOVSource} 
'								- A#2_2_1_2:F#F_Ope{TopPOVSource} 
'								+ A#2_2_1_7:F#F_Clo{TopPOVSource} 
'								- A#2_2_1_7:F#F_Ope{TopPOVSource})
'								+ A#980000:F#F_TotMov{TopPOVSource} 
'								+A#2_1_2:F#F_TotMov{TopPOVSource} 
'								+ A#8_1:F#Top{TopPOVSource}))"								 
								 Source = $"(-(A#1_1_5:F#F_Clo{TopPOVSource} 
								- A#1_1_5:F#F_Ope{TopPOVSource}
								+ A#1_1_7:F#F_Clo{TopPOVSource} 
								- A#1_1_7:F#F_Ope{TopPOVSource}
								+ A#1_1_11:F#F_Clo{TopPOVSource} 
								- A#1_1_11:F#F_Ope{TopPOVSource}
								+ A#1_1_12:F#F_Clo{TopPOVSource} 
								- A#1_1_12:F#F_Ope{TopPOVSource}
								+ A#1_2_7:F#F_Clo{TopPOVSource} 
								- A#1_2_7:F#F_Ope{TopPOVSource} 
								+ A#1_2_8:F#F_Clo{TopPOVSource} 
								- A#1_2_8:F#F_Ope{TopPOVSource}
								 + A#1_2_3:F#F_Clo{TopPOVSource} 
								- A#1_2_3:F#F_Ope{TopPOVSource}
								  + A#1_2_4:F#F_Clo{TopPOVSource} 
								- A#1_2_4:F#F_Ope{TopPOVSource}
								  + A#1_2_6:F#F_Clo{TopPOVSource} 
								- A#1_2_6:F#F_Ope{TopPOVSource}
								  + A#1_1_4:F#F_Clo{TopPOVSource} 
								- A#1_1_4:F#F_Ope{TopPOVSource}
								  + A#1_1_6:F#F_Clo{TopPOVSource} 
								- A#1_1_6:F#F_Ope{TopPOVSource}
								  + A#1_1_8:F#F_Clo{TopPOVSource} 
								- A#1_1_8:F#F_Ope{TopPOVSource}
								  + A#1_1_10:F#F_Clo{TopPOVSource} 
								- A#1_1_10:F#F_Ope{TopPOVSource}
								 ) 
								-(A#2_1:F#F_Clo{TopPOVSource} 
								- A#2_1:F#F_Ope{TopPOVSource} 
								+ A#2_2_1_1:F#F_Clo{TopPOVSource} 
								- A#2_2_1_1:F#F_Ope{TopPOVSource} 
								+ A#2_2_1_2:F#F_Clo{TopPOVSource} 
								- A#2_2_1_2:F#F_Ope{TopPOVSource} 
								+ A#2_2_1_7:F#F_Clo{TopPOVSource} 
								- A#2_2_1_7:F#F_Ope{TopPOVSource}
								 + A#2_2_2_1:F#F_Clo{TopPOVSource} 
								- A#2_2_2_1:F#F_Ope{TopPOVSource}
								  + A#2_2_2_6:F#F_Clo{TopPOVSource} 
								- A#2_2_2_6:F#F_Ope{TopPOVSource} 
								 + A#2_1_3:F#F_Clo{TopPOVSource} 
								- A#2_1_3:F#F_Ope{TopPOVSource}
								 )
								+ A#980000:F#F_TotMov{TopPOVSource} 
								+A#2_1_2:F#F_TotMov{TopPOVSource}) 
								+ A#8_1:F#Top{TopPOVSource})"
								'Bero 19Jul24 take out fx impact by deducting A#980000
								'Bero 31Jul24 take out minority interest by deducting A#2_1_2

								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If		
							

					#End Region	
					
					#Region "______________Interest_______9_2_3_2_______"
'					CF int = A#8_1_1_2_1_2_1_1:F#TOP + A#8_1_2_1_1_1_1_1:F#TOP; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims Not mentioned. 
						Case "9_2_3_2"
							If BoolLocalCurrency
'								Formula Pass 9
								'13Sep24 BeRo updated to IC top in source
								Source = $"(A#8_1_1_2_1_2_1_1:F#TOP:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None + A#8_1_2_1_1_1_1_1:F#TOP:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None)"
								'18NOV2024, AN deducted account: 750500 Accrued interesests
								Source2 = $"(A#750500:F#Top{TopPOVSource} -A#750500:F#F_Ope{TopPOVSource})"  
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros(-{source}-{source2})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If



					#End Region	
					
					#Region "______________One-offs & Projects_____9.2.3.3.1_________"
'					CF OneOffs = A#8_1_2_1_1_3_1_3:F#TOP + A#8_1_1_1_2_1_1:F#TOP; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims Not mentioned. 
						Case "9_2_3_3_1"
							If BoolLocalCurrency
'								Source = $"-(A#8_1_2_1_1_3_1_3:F#Top:V#YTD:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#None + A#8_1_1_1_2_1_1:F#Top:V#YTD:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#None)"
								'13Sep24 updated to IC top. Put back to Ic#none after confirmation of Pablo G.
								Source = $"-(A#8_1_2_1_1_3_1_3:F#Top:V#YTD:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#None + A#8_1_1_1_2_1_1:F#Top:V#YTD:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#None)"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If				
	
					#End Region	
					
					#Region "______________IT________9.2.3.3.2.1______"
'					CF IT Mapex = -A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:UD5#M:UD7#IT; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims not mentioned.  
						Case "9_2_3_3_2_1"
							If BoolLocalCurrency
'								 api.LogMessage("Berocheck 9_2_3_3_2_1 ")
								
'								Source = $"A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Maintenance_P:U7#IT"
								Source = $"(A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Maintenance_P:UD7#IT)" 'U6#None
								Source2 = $"A#731100:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None:U5#Maintenance_P:U7#IT" 'U6#None
'								Source = "100"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None" 'U6#None
								Formula = $"{Destination}=RemoveZeros(-({Source}+{Source2}))"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If				


					#End Region	
					
					#Region "______________Financial Lease______9_2_3_3_2_2________"
'					CF FinLeases Mapex =  -(A#7311000:F#Closing:UD5#Top -A#7311000:F#Opening:UD5#Top )- A#CF FinLease Growth (line 383 in this file); YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims not mentioned. 
						Case "9_2_3_3_2_2"
							If BoolLocalCurrency
'								Formula: Formula To specific codes In data staging area
'								Formula Pass 9
'								Source = $"(A#731100:F#F_TotMov:V#YTD:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#Top:UD5#ML:U7#Top)"
'								Source2 = $"A#731100:F#F_TotMov:V#YTD:O#Top:I#None:U1#Top:U2#Top:U3#Top:U4#Top:U5#ML:U7#IT"
'								 'BERO 30Jul24 updated to pull only PAYMENT flow. 13Sep24 BeRo updated to IC top in source
								Source = $"(A#731100:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:UD5#ML:U7#Top)"
								Source2 = $"A#731100:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#ML:U7#IT"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None"
								Formula = $"{Destination}=RemoveZeros(-({source}-{source2}))"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If

					#End Region	
					
					#Region "______________Operating Lease____9_2_3_3_2_3_________"
'					CF OpLeases Mapex = A#8_1_1_1_1_3_1_3_1_1:F#Top - A#CF OPLease Growth (line 382 in this file); YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims not mentioned. 
						Case "9_2_3_3_2_3"
							If BoolLocalCurrency
'								Formula: Formula To specific codes In data staging area
'								Formula Pass 9 - 13Sep24 BeRo updated to IC top in source
								Source = $"-A#8_1_1_1_1_4_1_3_1_1:F#Top:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None"
								Source2 = $"A#9_2_2_2:F#FPA_CF_Calc:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source}-{source2})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
							End If
					#End Region	
					
					#Region "______________Cash Purchase_________9_2_3_3_2_4_____"
'					CF CashPurchases Mapex= A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:UD5#M:UD7#Top-A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:UD5#M:UD7#IT
						Case "9_2_3_3_2_4"
							If BoolLocalCurrency
'								Source = $"A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#M:U7#Top"
'								Source2 = $"A#780000:F#F_Other_Current_non_financial_liabilities_Mov_1:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#M:U7#IT"
'								Source = $"A#Tech_Payments:F#None:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#M:U7#Top"
'								Source2 = $"A#Tech_Payments:F#None:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#M:U7#IT" F_Leases_ST_LT_Mov_3
								Source = $"A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Maintenance_P:U7#Top"
								Source2 = $"A#780000:F#F_Leases_ST_LT_Mov_3:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Maintenance_P:U7#IT"
								'Tech_Payments
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U7#None"
								Formula = $"{Destination}=RemoveZeros(-({source}-{source2}))"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
'								brapi.ErrorLog.LogMessage(si, "Source:" & Source)
'								brapi.ErrorLog.LogMessage(si, "Source2:" & Source2)
							End If
					#End Region	

					#Region "_____________Other current non financial liabilities ______9.2.3.3.3_________"
'					CF OthNFL = A#2_2_2_8:F#Closing - A#2_2_2_8:F#Opening; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims not mentioned.  
						Case "9_2_3_3_3"
							If BoolLocalCurrency
'								Formula Pass 11
								Source = $"(A#2_2_2_8:F#F_CLO{TopPOVSource}-A#2_2_2_8:F#F_OPE{TopPOVSource})"
								'Exclude 780000 to avoid duplicated impact on CF
								Dim Source1 As String = $"(A#780000:F#F_CLO{TopPOVSource}-A#780000:F#F_OPE{TopPOVSource})"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveNodata(-({source}-{Source1}))"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If				
					#End Region	
					
					#Region "______________Trade and notes payable_______9.2.3.3.4.1_______"
'					CF TP = A#2_2_2_2:F#Closing - A#2_2_2_2:F#Opening; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims not mentioned. 
						Case "9_2_3_3_4_1"
							If BoolLocalCurrency
'								Formula Pass 11
								Source = $"(A#2_2_2_2:F#F_CLO{TopPOVSource}-A#2_2_2_2:F#F_OPE{TopPOVSource})"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros(-{source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If	
					#End Region	
					
					#Region "_____________Trade and notes receivable________9.2.3.3.4.2_______"
'					CF TR = A#1_2_2:F#Closing - A#1_2_2:F#Opening; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims Not mentioned.
						Case "9_2_3_3_4_2"
							If BoolLocalCurrency
'								Formula Pass 11
								Source = $"(A#1_2_2:F#F_CLO{TopPOVSource}-A#1_2_2:F#F_OPE{TopPOVSource})"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros(-{source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If	

					#End Region	
								
					#Region "______________Inventories______9.2.3.3.4.3________"
						Case "9_2_3_3_4_3"
'							CF Inventories = A#1_2_1:F#Closing - A#1_2_1:F#Opening; YTD calculation; UD1top,UD2top,UD3top,UD4#None; other dims not mentioned. 
							If BoolLocalCurrency
'								Formula Pass 11
								Source = $"(A#1_2_1:F#F_CLO{TopPOVSource}-A#1_2_1:F#F_OPE{TopPOVSource})"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros(-{source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#Top_ExcCF.base")
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN") '03Jul24 onsite update to calculate only preifrs16
							End If	

					
					#End Region	
					
					#Region "______________EBITDA excl. leases______9.2.3.3.5________"
						Case "9_2_3_3_5"
'							Formula: Pull EBITDA_excl_Eq_Lease
							If BoolLocalCurrency
'								Formula Pass 11
								Source = $"A#EBITDA_excl_Eq_Lease:F#Top{TopPOVSource}:U8#PRE_IFRS16_PLAN"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}:U8#PRE_IFRS16_PLAN"
								Formula = $"{Destination}=RemoveZeros({source}*-1)"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#PRE_IFRS16_PLAN")
								
'								'03Jul24 update to separate pre and ifrs16 calcs of this CF line
								Source = $"A#311000:F#Top{TopPOVSource}:U8#IFRS16_PLAN" 'Needs to be updated with the Acc from IFRS16 model that Pablo G. requested
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc{TopPOVDestination}:U8#IFRS16_PLAN"
								Formula = $"{Destination}=RemoveZeros({source}*-1)"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#IFRS16_PLAN")
								
								
							End If	

					#End Region		
					
					#Region "______________IFRS______9.2.3.3.0________"
						Case "9_2_3_3_0"
'							Formula: Pull EBITDA_excl_Eq_Lease with opposite sign
							If BoolLocalCurrency
'								Formula Pass 12
								Source = $"(-A#9_2_3_3_5:F#9_Cashflow_FPA:U8#IFRS16_PLAN{TopPOVSource} - A#9_2_3_3_2:F#9_Cashflow_FPA:U8#IFRS16_PLAN{TopPOVSource})"
								Destination = $"A#{str_FPA_CF_Account}:F#FPA_CF_Calc:U8#IFRS16_PLAN{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter, Ud8Filter:="U8#IFRS16_PLAN")
							End If	

					#End Region		
				
					
				End Select
				
		 End If 'If PovScenario.Contains("QFCST") Or PovScenario.Contains("YP") Or PovScenario.Contains("Budget")
								
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		' Define the FilterMembersByText1 function
		Public Function FilterMembersByText1(ByVal api As FinanceRulesApi, members As IList(Of Member), text1Value1 As Integer, text1Value2 As Integer) As IList(Of Member)
		    Dim filteredMembers As New List(Of Member)

		    For Each member As Member In members
		        ' Assuming the Text1 property is obtained via an API call
		        Dim text1 As String = Api.UD1.Text(member.MemberId, 1)
		        Dim text1AsInteger As Integer
		        If text1 IsNot Nothing AndAlso Integer.TryParse(text1, text1AsInteger) Then
		            If text1AsInteger = text1Value1 OrElse text1AsInteger = text1Value2 Then
		                filteredMembers.Add(member)
		            End If
		        End If
		    Next

		    Return filteredMembers
		End Function
		' Define the LogFilteredMemberIDs function
		Public Sub LogFilteredMemberIDs(si As SessionInfo, members As IList(Of Member))
		    Dim memberIds As New List(Of String)
		    For Each member As Member In members
		        memberIds.Add("U1#" & member.Name.ToString())
		    Next

		    ' Join the member IDs into a single string with comma separation
		    Dim logMessage As String = "For GridView: " & String.Join(",", memberIds)

		    ' Log the final message
		    BRApi.ErrorLog.LogMessage(si, logMessage)
		End Sub
		
		
	End Class
End Namespace