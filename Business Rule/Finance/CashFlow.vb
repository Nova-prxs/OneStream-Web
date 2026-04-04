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

Namespace OneStream.BusinessRule.Finance.CashFlow
	Public Class MainClass
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
		
		Public Function CashflowCalc(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal strFlowname As String) As String
'		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs, ByVal strFlowname As String) As Object
			Try
				Dim BoolLocalCurrency As Boolean = api.Cons.IsLocalCurrencyForEntity
				Dim BoolForeignCur As Boolean = api.Cons.IsForeignCurrencyForEntity
				Dim OwnerPostAdj As Boolean = api.pov.cons.name.equals("OwnerPostAdj")
				Dim OwnerPreAdj As Boolean = api.pov.cons.name.equals("OwnerPreAdj") 'Included by DAGO on 20250407 for the adjs posted to match Cube
				Dim Elimination As Boolean = api.pov.cons.name.equals("Elimination")
'				Dim BoolElim As Boolean = api.Cons.IsForeignCurrencyForEntity
				Dim BaseEntity As Boolean = Not api.Entity.HasChildren
				Dim ParentEntity As Boolean = api.Entity.HasChildren
				Dim Source As String = Nothing
				Dim Destination As String = Nothing
				Dim Formula As String = Nothing
				Dim AccountFilter As String = Nothing
				Dim TopPOVSource As String = ":V#YTD:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top"
				Dim TopPOVSource_ACQ As String = ":V#YTD:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Acquisition_P:U6#Top:U7#Top"
				Dim TopPOVDestination As String = ":V#YTD:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None"
				Dim ParentFLOW As String = Nothing
'				Dim TopPOVDestination As String = ":F#Cashflow_dummy:V#YTD:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None"
				Dim CloRateLYM12 As Decimal = Nothing
				Dim CloRate As Decimal = Nothing
				Dim AVGRate As Decimal = Nothing
				Dim strAccountName As String= strFlowname
				Dim strSourceAcc As String = ""
				Dim strSourceFlow As String = ""
				Dim currMonthNum As Integer = api.Time.GetPeriodNumFromId(api.Pov.Time.MemberId)
				If BoolForeignCur
					CloRateLYM12 = api.FxRates.GetCalculatedFxRate(api.FxRates.GetFxRateTypeForAssetLiability, api.Time.GetLastPeriodInPriorYear).XFToStringForFormula
					CloRate = api.FxRates.GetCalculatedFxRate(api.FxRates.GetFxRateTypeForAssetLiability).XFToStringForFormula
					AVGRate = api.FxRates.GetCalculatedFxRate(api.FxRates.GetFxRateTypeForRevenueExp).XFToStringForFormula
				End If 
				
				'Szilvia Sarusi-Kis 16/10/2025 If (api.Pov.Scenario.Name.XFContainsIgnoreCase("Budget") Or api.Pov.Scenario.Name.XFContainsIgnoreCase("Q_FCST")  Or api.Pov.Scenario.Name.XFContainsIgnoreCase("5YP")) AndAlso Not strFlowname.XFEqualsIgnoreCase("F_Retained_Earnings_Mov_1")
				If (api.Pov.Scenario.Name.XFContainsIgnoreCase("5YP")) AndAlso Not strFlowname.XFEqualsIgnoreCase("F_Retained_Earnings_Mov_1")
					Exit Function 
				End If	
'				If api.Pov.Entity.Name.XFEqualsIgnoreCase("L_Del")
				Select Case strFlowname
						#Region "______________Translation_New_____________"
						Case "FX_OPE"	
							If BoolForeignCur 
								AccountFilter = "A#2_2.Base, A#1.Base"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"(C#Local:F#F_OPE{TopPOVSource}*{CloRate})-F#F_OPE{TopPOVSource}"
								Formula = $"{Destination}=RemoveNodata({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "FX_MOV"	
							If BoolForeignCur 
								AccountFilter = "A#2_2.Base, A#1.Base"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"(C#Local:F#F_GL_Load_BS{TopPOVSource}*{CloRate})-F#F_GL_Load_BS{TopPOVSource}"
'								Source = $"(C#Local:F#F_GL_Load_BS{TopPOVSource}*{CloRate})-F#F_GL_Load_BS{TopPOVSource}-F#F_Acquisitions{TopPOVSource}"
'								Source = $"(C#Local:F#F_GL_Load_BS{TopPOVSource}*{CloRate})-F#F_GL_Load_BS{TopPOVSource}"
								Formula = $"{Destination}=RemoveNodata({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "FX_ACQ"	
							If BoolForeignCur 
								AccountFilter = "A#2_2.Base, A#1.Base"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
'								Source = $"(C#Local:F#FX_ACQ{TopPOVSource}*{CloRate})-F#FX_ACQ{TopPOVSource}"
								Source = $"(C#Local:F#F_Acquisitions{TopPOVSource}*{CloRate})-F#F_Acquisitions{TopPOVSource}"
								Formula = $"{Destination}=RemoveNodata({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						
						#End Region
						
				'__________________Consolidated financial statements _ Start __________________		
						
							#Region "______________	10_1_1_1_1_1	Loss from continuing operations before income tax	_____________"
							Case "10_1_1_1_1_1"	
								If BoolLocalCurrency Or BoolForeignCur
									'5_1_1 - Profit/(loss) before tax
									strSourceFlow = "Top_Generic_Flows"
									Source = $"-A#5_1_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_1_2	(Gain)/loss on disposal of property, plant and equipment	_____________"	
							Case "10_1_1_1_1_2"	
								If BoolLocalCurrency Or BoolForeignCur
'								5_1_1_9 - Impairment loss on property, plant and equipment (net of reversal)
									strSourceFlow = "Top"
									Source = $"	A#5_1_1_9:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
								
								#End Region
							#Region "______________	10_1_1_1_1_3	(Gain)/loss on disposal of subsidiaries	_____________"	
							Case "10_1_1_1_1_3"	
								If BoolLocalCurrency Or BoolForeignCur
									'5_1_1_10_5 - Gains/(losses) on disposals of shareholdings and activities

									strSourceFlow = "Top"
									Source = $"	A#5_1_1_10_5:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							
							#End Region
							#Region "______________	10_1_1_1_1_4	Depreciation and Amortisation	_____________"	
							Case "10_1_1_1_1_4"	
								If BoolLocalCurrency Or BoolForeignCur
									'5_1_1_6 - Depreciation expenses (including depreciations on rights of use)
									'5_1_1_7 - Amortization expenses									
									strSourceFlow = "Top"
									Source = $"	A#5_1_1_6:F#{strSourceFlow}{TopPOVSource}
												+A#5_1_1_7:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_1_5	Miscellaneous profit and loss items without cash effect	_____________"	
							Case "10_1_1_1_1_5"	
								If BoolLocalCurrency Or BoolForeignCur
									'11_1 - Non-current assets
									'11_2 - Current assets
									'12_1 - Total equity
									'12_2 - Total liabilities
									'MISC_CF - MISC_CF
									'strSourceFlow = "Top"
									Source = $"	-A#11_1:F#FX_Tot{TopPOVSource}
												-A#11_2:F#FX_Tot{TopPOVSource}
												-A#12_1:F#FX_Tot{TopPOVSource}
												-A#12_2:F#FX_Tot{TopPOVSource}
												+A#MISC_CF:F#F_ADD{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_1_6	Interest income	_____________"	
							Case "10_1_1_1_1_6"	
								If BoolLocalCurrency Or BoolForeignCur
									'5_1_1_10_1 - Interest income
									strSourceFlow = "Top"
									Source = $"	A#5_1_1_10_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_1_7	Interest expense	_____________"	
							Case "10_1_1_1_1_7"	
								If BoolLocalCurrency Or BoolForeignCur
									'5_1_1_10_2 - Interest expense
									strSourceFlow = "Top"
									Source = $"	A#5_1_1_10_2:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
								
							#End Region
							#Region "______________	10_1_1_1_2_1_1	Non-Financial_____________"	
							Case "10_1_1_1_2_1_1"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Flow_4"
									'11_1_7 - Other non-current non financial assets
									Source = $"	-A#11_1_7:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
								
							#End Region
					'		#Region "______________	10_1_1_1_2_1_2	Financial_____________"	
					'		Case "10_1_1_1_2_1_2"	
					'			If BoolLocalCurrency Or BoolForeignCur
					'				'strSourceFlow = "Flow_4"
					'				'11_1_6 - Other non-current financial assets
					'				Source = $" -A#11_1_6:F#F_ADD{TopPOVSource} 
					'						    -A#11_1_6:F#F_DISP{TopPOVSource}"
					'				Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
					'				Formula = $"{Destination}=RemoveNodata({source})"
					'				api.Data.Calculate(Formula, accountFilter:=AccountFilter)
					'			End If
								
					'		#End Region
							#Region "______________	10_1_1_1_2_2	Increase/(decrease) in inventories	_____________"	
							Case "10_1_1_1_2_2"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Flow_4"
									'11_2_1 Inventories
									Source = $"	-A#11_2_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							
							#End Region
							#Region "______________	10_1_1_1_2_3_1	BS movement in Increase/(decrease) in trade and notes receivable	_____________"	
							Case "10_1_1_1_2_3_1"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Flow_4"
									'11_2_2 - Trade and notes receivable
									Source = $"	-A#11_2_2:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If					
							#End Region
							#Region "______________	10_1_1_1_2_3_2	PL bad debt account	_____________"	
							Case "10_1_1_1_2_3_2"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Top"
'									5_1_1_5_16 - Provisions for bad debt
									Source = $" -A#5_1_1_5_16:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If	
							#End Region
							#Region "______________	10_1_1_1_2_4_1	Other current financial assets	_____________"	
							Case "10_1_1_1_2_4_1"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_5 - Other current financial assets 
									'strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_5:F#F_ADD{TopPOVSource}
												-A#11_2_5:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_4_2	VAT and taxes recoverable other than income tax	_____________"	
							Case "10_1_1_1_2_4_2"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_7 -  VAT and taxes recoverable other than income tax 
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_7:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_4_3	Advances	_____________"	
							Case "10_1_1_1_2_4_3"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_8 - Advances
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_8:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
								#End Region
							#Region "______________	10_1_1_1_2_4_4	Other current non-financial assets	_____________"	
							Case "10_1_1_1_2_4_4"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_9 - Other current non-financial assets
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_9:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_5 Increase in employee retirement and post-employment benefit obligations	_____________"	
							Case "10_1_1_1_2_5"
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_1_2 - Employee retirement and post-employment benefit obligations
									'strSourceFlow = "Top"
									Source = $"	-A#12_2_1_2:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If	
							#End Region
							#Region "______________	10_1_1_1_2_6 (Decrease) in provisions_____________"	
							Case "10_1_1_1_2_6"
								If BoolLocalCurrency Or BoolForeignCur
'									 12_2_2_1 - Current  Provisions 
' 									 12_2_1_1 - Non-current Provisions 
									strSourceFlow = "F_DISP"
									Source = $"	-A#12_2_2_1:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_1_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_7_1 Non-Financial	_____________"	
							Case "10_1_1_1_2_7_1"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_1_3 - Other non-current non financial liabilities
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_1_3:F#{strSourceFlow}{TopPOVSource}
												+A#815000:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							
					'		#Region "______________	10_1_1_1_2_7_2 Financial	_____________"	
					'		Case "10_1_1_1_2_7_2"	
					'			If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_11 - Other current financial liabilities
'									strSourceFlow = "Flow_4"
					'				Source = $"	-A#12_2_2_11:F#F_ADD{TopPOVSource}
					'							-A#12_2_2_11:F#F_DISP{TopPOVSource}"
					'				Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
					'				Formula = $"{Destination}=RemoveNodata({source})"
					'				api.Data.Calculate(Formula, accountFilter:=AccountFilter)
					'			End If
					'		#End Region
							
							#Region "______________	10_1_1_1_2_8 (Decrease)/increase in trade and notes payable	_____________"	
							Case "10_1_1_1_2_8"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_2 - Trade and notes payable
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_2:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_9_1 VAT and taxes payable other than income taxes	_____________"	
							Case "10_1_1_1_2_9_1"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_8 -  VAT and taxes payable other than income taxes
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_8:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_9_2 Accruals	_____________"	
							Case "10_1_1_1_2_9_2"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_6 - Accruals
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_6:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_9_3 Other current non financial liabilities	_____________"	
							Case "10_1_1_1_2_9_3"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_5 - Other current non financial liabilities 
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_5:F#{strSourceFlow}{TopPOVSource}
												+A#715000:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_1_2_9_4 Personal related liabilities	_____________"	
							Case "10_1_1_1_2_9_4"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_3 - Personal related liabilities
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_3:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_2_1 BS movement in CIT payable and receivable	_____________"	
							Case "10_1_1_2_1"	
								If BoolLocalCurrency Or BoolForeignCur
							'	11_2_6 - Tax receivables
							'	12_2_2_7 - Tax payables
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_6:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_2_7:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_1_2_2 PL corporate income tax line	_____________"	
							Case "10_1_1_2_2"	
								If BoolLocalCurrency Or BoolForeignCur
							'	5_1_2 - Income tax before capital operations
							'   480100
									strSourceFlow = "Top"
									Source = $"	-A#5_1_2:F#{strSourceFlow}{TopPOVSource}
												+A#480100:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
'							#Region "______________	10_1_1_3 - Net cash from continuing operations  provided by operating activities	_____________"	Case "10_1_1_3"	#End Region
'							#Region "______________	10_1_1_4 - Net cash from discontinued operations (used in) provided by operating activities	_____________"	Case "10_1_1_4"	#End Region
							#Region "______________	10_1_2_1_1 M&A openning balance	_____________"	
							Case "10_1_2_1_1"	
								If BoolLocalCurrency Or BoolForeignCur
							'	 12_1 - Total equity
							'		 800000 - Related party loans payable
									strSourceFlow = "F_Acquisitions"
									Source = $"	A#12_1:F#{strSourceFlow}{TopPOVSource}
											"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	10_1_2_1_2 M&A TD	_____________"	
							Case "10_1_2_1_2"	
								If BoolLocalCurrency Or BoolForeignCur
							'	 11_1_1_2 - Trademarks
									strSourceFlow = "F_Acquisitions"
									Source = $"	-A#11_1_1_2:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	10_1_2_1_3 M&A CV	_____________"	
							Case "10_1_2_1_3"	
								If BoolLocalCurrency Or BoolForeignCur
							'	 11_1_1_3 - Contract value
									strSourceFlow = "F_Acquisitions"
									Source = $"	-A#11_1_1_3:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	10_1_2_1_4 M&A DTA/DTL	_____________"	
							Case "10_1_2_1_4"	
								If BoolLocalCurrency Or BoolForeignCur
							'	  11_1_5 - Deferred tax assets 
							' 	  12_2_1_4 - Deferred tax liabilities 
									strSourceFlow = "F_Acquisitions"
									Source = $"	A#11_1_5:F#{strSourceFlow}{TopPOVSource}
									            -A#12_2_1_4:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	10_1_2_1_5 M&A Goodwill	_____________"	
							Case "10_1_2_1_5"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	 11_1_2 - Goodwill 
							'		strSourceFlow = "F_Acquisitions"
									Source = $"	-A#11_1_2:F#F_Acquisitions{TopPOVSource}"
							'					-A#11_1_2:F#F_ADD{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region									
							#Region "______________	10_1_2_1_6 Cash from M&A	_____________"	
							Case "10_1_2_1_6"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	 11_2_4 - Cash and cash equivalents 
									strSourceFlow = "F_Acquisitions"
									Source = $"	A#11_2_4:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	10_1_2_1_7 Deferred considerations	_____________"	
							Case "10_1_2_1_7"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	  12_2_1_8 - Deferred consideration non-current 
 							'	  12_2_2_12 - Deferred consideration current 
							'		strSourceFlow = "F_Acquisitions"
									Source = $"	A#12_2_1_8:F#F_Acquisitions{TopPOVSource}
												-A#12_2_1_8:F#F_DISP{TopPOVSource}
									            -A#12_2_1_8:F#F_ADD{TopPOVSource} 
												+A#12_2_2_12:F#F_Acquisitions{TopPOVSource}
												-A#12_2_2_12:F#F_DISP{TopPOVSource}
									            -A#12_2_2_12:F#F_ADD{TopPOVSource}"
									           
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	10_1_2_2_1 Additions in PPE	_____________"	
							Case "10_1_2_2_1"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	   11_1_3_1 - Land and buildings 
							'	   11_1_3_2 - Medical equipment 
							'	   11_1_3_3 - Other equipment 
							'	   11_1_3_4 - Fixed assets under construction 
							'	   11_1_3_5 - Other 
									strSourceFlow = "F_ADD"		
									Source = $"	-A#11_1_3_1:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_2:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_3:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_4:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_5:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region							
							#Region "______________	10_1_2_2_2 Additions in ROU	_____________"	
							Case "10_1_2_2_2"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	   11_1_3_6 - ROU
									strSourceFlow = "F_ADD"		
									Source = $"	-A#11_1_3_6:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	10_1_2_2_3 Compensation of investment suppliers	_____________"	
							Case "10_1_2_2_3"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	   12_2_2_4 -  Investment suppliers
									strSourceFlow = "Flow_4"		
									Source = $"	-A#12_2_2_4:F#{strSourceFlow}{TopPOVSource}
												-A#715000:F#{strSourceFlow}{TopPOVSource}
												-A#815000:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	10_1_2_3_1 Disposals in PPE	_____________"	
							Case "10_1_2_3_1"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	    11_1_3_1 - Land and buildings 
							' 	    11_1_3_2 - Medical equipment 
							' 	    11_1_3_3 - Other equipment 
							' 	    11_1_3_4 - Fixed assets under construction 
							' 	    11_1_3_5 - Other 
									strSourceFlow = "F_DISP"		
									Source = $"	-A#11_1_3_1:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_2:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_3:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_4:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_3_5:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	10_1_2_3_2 Disposals in ROU	_____________"	
							Case "10_1_2_3_2"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	    11_1_3_6 - ROU
									strSourceFlow = "F_DISP"		
									Source = $"	-A#11_1_3_6:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	10_1_2_3_3 Gain/loss on disposal of property, plant and equipment	_____________"	
							Case "10_1_2_3_3"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	    5_1_1_9 - (Gain)/loss on property, plant and equipment & intangibles
									strSourceFlow = "Top"		
									Source = $"	-A#5_1_1_9:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	10_1_2_4 Acquisition of intangible assets including patents and trademarks	_____________"	
							Case "10_1_2_4"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	    11_1_1 - Intangible assets
									strSourceFlow = "F_ADD"		
									Source = $"	-A#11_1_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_2_5 Disposal of intangible assets including patents and trademarks	_____________"	
							Case "10_1_2_5"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	    11_1_1 - Intangible assets
									strSourceFlow = "F_DISP"		
									Source = $"	-A#11_1_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region				
							#Region "______________	10_1_3_1 Proceeds from issue of equity capital	_____________"	
							Case "10_1_3_1"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	    Capital_Increase
									strSourceFlow = "F_ADD"		
									Source = $"A#Capital_Increase:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_3_2 Proceeds from long-term borrowings, finance lease (ST+LT)	_____________"	
							Case "10_1_3_2"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	     12_2_1_5 - Lease liabilities non-current 
							'		 12_2_1_6 - Bank borrowings non-current 
							'     	 12_2_1_7 - Other non-current financial liabilities
							'		 12_2_2_9 - Lease liabilities current 
							'		 12_2_2_10 - Bank borrowings current 
							'		 11_2_3 - Short-term interest bearing advances 
							'		 800000 - Related party loans payable
							'		 11_1_6 - Other non-current financial assets
									strSourceFlow = "F_ADD"		
									Source = $"	-A#12_2_1_5:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_1_6:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_1_7:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_2_9:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_2_10:F#{strSourceFlow}{TopPOVSource}
												-A#11_2_3:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_6:F#F_ADD{TopPOVSource}
												-A#12_2_2_11:F#F_ADD{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_3_3 (Repayment) of long-term borrowings, finance lease (including current portion)	_____________"	
							Case "10_1_3_3"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	      12_2_1_5 - Lease liabilities non-current 
 							'		  12_2_1_6 - Bank borrowings non-current 
							'     	  12_2_1_7 - Other non-current financial liabilities
							'		  11_1_6 - Other non-current financial assets
									strSourceFlow = "F_DISP"		
									Source = $"	-A#12_2_1_5:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_1_6:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_1_7:F#{strSourceFlow}{TopPOVSource}
												-A#11_1_6:F#F_DISP{TopPOVSource}
												-A#12_2_2_11:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	10_1_3_4 (Repayment of) overdrafts and short-term bank borrowings	_____________"	
							Case "10_1_3_4"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	       12_2_2_9 - Lease liabilities current 
 							'		   12_2_2_10 - Bank borrowings current 
									strSourceFlow = "F_DISP"		
									Source = $"	-A#12_2_2_9:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_2_10:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
'							#Region "______________	10_1_3_5 Proceeds from other financial liabilities	_____________"	Case "10_1_3_5"	#End Region	
'							#Region "______________	10_1_3_6 (Repayment) of other financial liabilities	_____________"	Case "10_1_3_6"	#End Region	
'							#Region "______________	10_1_3_7 Interest received (other than interest earned on long-term investments)	_____________"	Case "10_1_3_7"	#End Region	
'							#Region "______________	10_1_3_8 (Interest paid)	_____________"	Case "10_1_3_8"	#End Region	
'							#Region "______________	10_1_3_9 Net cash from continuing operations provided by financing activities	_____________"	Case "10_1_3_9"	#End Region	
							#Region "______________	10_2 Net effect of currency translation on cash and cash equivalents	_____________"	
							Case "10_2"	
								If BoolLocalCurrency Or BoolForeignCur
							' 	       11_2_4 - Cash and cash equivalents
									strSourceFlow = "FX_Tot"		
									Source = $"	-A#11_2_4:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
'							__________________Consolidated financial statements _ End __________________	
							
'							__________________Managerial Cash Flow _ Start __________________
							#Region "______________	14_1_1 - Eq. Leases	_____________"	
							Case "14_1_1"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Top"
									'8_1_1_1_1_4_1_3_1
									Source = $"	-A#8_1_1_1_1_4_1_3_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							
							#End Region
							#Region "______________	14_1_2 - EBIDTA	_____________"	
							Case "14_1_2"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Top"
									'8_1_1_1_1
									Source = $"	-A#8_1_1_1_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							
							#End Region
							#Region "______________	14_2_1 - Increase/(decrease) in inventories	_____________"	
							Case "14_2_1"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Flow_4"
									'11_2_1 Inventories
									Source = $"	-A#11_2_1:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							
							#End Region
							#Region "______________	14_2_2_1 - BS movement in Increase/(decrease) in trade and notes receivable	_____________"	
							Case "14_2_2_1"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Flow_4"
									'11_2_2 - Trade and notes receivable
									'520000
									'Indirect_Elim_TradeRec
									'123403
									Source = $"	-A#11_2_2:F#{strSourceFlow}{TopPOVSource}
												+A#520000:F#{strSourceFlow}{TopPOVSource}
												+A#Indirect_Elim_TradeRec:F#{strSourceFlow}{TopPOVSource}
												+A#123403:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If					
							#End Region
							#Region "______________	14_2_2_2 - PL bad debt account	_____________"	
							Case "14_2_2_2"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Top"
'									5_1_1_5_16 - Provisions for bad debt
									Source = $" -A#5_1_1_5_16:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If	
							#End Region							
							#Region "______________	14_2_3 - (Decrease)/increase in trade and notes payable 	_____________"	
							Case "14_2_3"	
								If BoolLocalCurrency Or BoolForeignCur
									'12_2_2_2 - Trade and notes payable
									'720000
									'714000
									'430150
									'430800
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_2:F#{strSourceFlow}{TopPOVSource}
												+A#720000:F#{strSourceFlow}{TopPOVSource}
												+A#714000:F#{strSourceFlow}{TopPOVSource}
												+A#430150:F#Top{TopPOVSource}
												+A#430800:F#Top{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_3_1 - Increase in employee retirement and post-employment benefit obligations 	_____________"	
							Case "14_3_1"
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_1_2 - Employee retirement and post-employment benefit obligations
									'strSourceFlow = "Top"
									Source = $"	-A#12_2_1_2:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If	
							#End Region
							#Region "______________	14_3_2 - Decrease in provisions  	_____________"	
							Case "14_3_2"
								If BoolLocalCurrency Or BoolForeignCur
'									 2_2_2_1 - Current  Provisions 
' 									 2_2_1_1 - Non-current Provisions 
									'strSourceFlow = "Top"
									Source = $"	-A#2_2_2_1:F#F_DISP{TopPOVSource}
												-A#2_2_1_1:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If	
							#End Region
							#Region "______________	14_3_3_1 - Non-Financial	_____________"	
							Case "14_3_3_1"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_1_3 - Other non-current non financial liabilities
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_1_3:F#{strSourceFlow}{TopPOVSource}
												+A#815000:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
'							#Region "______________	14_3_3_2 - Financial	_____________"	
'							Case "14_3_3_2"	
'								If BoolLocalCurrency Or BoolForeignCur
''									12_2_2_11 - Other current financial liabilities
''									strSourceFlow = "Flow_4"
'									Source = $"	-A#12_2_2_11:F#F_ADD{TopPOVSource}
'												-A#12_2_2_11:F#F_DISP{TopPOVSource}"
'									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveNodata({source})"
'									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'								End If
'							#End Region
							#Region "______________	14_3_4_1 - VAT and taxes payable other than income taxes	_____________"	
							Case "14_3_4_1"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_8 -  VAT and taxes payable other than income taxes
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_8:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_3_4_2 - Accruals	_____________"	
							Case "14_3_4_2"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_6 - Accruals
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_6:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_3_4_3 - Other current non financial liabilities	_____________"	
							Case "14_3_4_3"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_5 - Other current non financial liabilities 
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_5:F#{strSourceFlow}{TopPOVSource}
												+A#715000:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_3_4_4 - Personal related liabilities	_____________"	
							Case "14_3_4_4"	
								If BoolLocalCurrency Or BoolForeignCur
'									12_2_2_3 - Personal related liabilities
									strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_2_3:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_3_5_1 - Non-Financial_____________"	
							Case "14_3_5_1"	
								If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "Flow_4"
									'11_1_7 - Other non-current non financial assets
									Source = $"	-A#11_1_7:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
								
							#End Region
'							#Region "______________	14_3_5_2 - Financial_____________"	
'							Case "14_3_5_2"	
'								If BoolLocalCurrency Or BoolForeignCur
'									'strSourceFlow = "Flow_4"
'									'11_1_6 - Other non-current financial assets
'									Source = $" -A#11_1_6:F#F_ADD{TopPOVSource} 
'											    -A#11_1_6:F#F_DISP{TopPOVSource}"
'									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveNodata({source})"
'									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'								End If
								
'							#End Region							
							#Region "______________	14_3_6_1 - Other current financial assets 	_____________"	
							Case "14_3_6_1"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_5 - Other current financial assets 
									'strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_5:F#F_ADD{TopPOVSource}
												-A#11_2_5:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_3_6_2 - VAT and taxes recoverable other than income tax	_____________"	
							Case "14_3_6_2"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_7 -  VAT and taxes recoverable other than income tax 
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_7:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_3_6_3 - Advances	_____________"	
							Case "14_3_6_3"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_8 - Advances
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_8:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
								#End Region
							#Region "______________	14_3_6_4 - Other current non-financial assets	_____________"	
							Case "14_3_6_4"	
								If BoolLocalCurrency Or BoolForeignCur
'									11_2_9 - Other current non-financial assets
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_9:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	14_4_1 - Tangible	_____________"	
							Case "14_4_1"	
								If BoolLocalCurrency Or BoolForeignCur
'									A#CAPEX_TANG_M
									strSourceFlow = "F_ADD"
									Source = $"	-A#CAPEX_TANG_M:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	14_4_2 - Finance Lease	_____________"	
							Case "14_4_2"	
								If BoolLocalCurrency Or BoolForeignCur
'									A#CAPEX_FINLEASE_M
									strSourceFlow = "F_ADD"
									Source = $"	-A#CAPEX_FINLEASE_M:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
'							#Region "______________	14_4_3 - Operating Lease	_____________"	
'							Case "14_4_3"	
'								If BoolLocalCurrency Or BoolForeignCur
''									A#CAPEX_OPELEASE_M
'									strSourceFlow = "Top"
'									Source = $"	-A#CAPEX_OPELEASE_M:F#{strSourceFlow}{TopPOVSource}"
'									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveNodata({source})"
'									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'								End If
'							#End Region								
							#Region "______________	14_4_4 - IT	_____________"	
							Case "14_4_4"	
								If BoolLocalCurrency Or BoolForeignCur
'									A#CAPEX_IT_M
									strSourceFlow = "F_ADD"
									Source = $"	-A#CAPEX_IT_M:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_4_5 - INV SUP M	_____________"	
							Case "14_4_5"	
								If BoolLocalCurrency Or BoolForeignCur
'									A#INV_SUP_M
									strSourceFlow = "F_ADD"
									Source = $"	-A#INV_SUP_M:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							
							#Region "______________	14_5 - One-offs	_____________"	
							Case "14_5"	
								If BoolLocalCurrency Or BoolForeignCur
'									8_1_1_1_2 - Below - EBITDA
									strSourceFlow = "Top"
									Source = $"	-A#8_1_1_1_2:F#{strSourceFlow}{TopPOVSource}
												+A#BTL:F#F_ADD{TopPOVSource}	   		   
												+A#431000:F#{strSourceFlow}{TopPOVSource} 
												+A#431100:F#{strSourceFlow}{TopPOVSource}" 
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region							
							#Region "______________	 14_6_1 - Interests 	_____________"	
							Case "14_6_1"	
							If BoolLocalCurrency Or BoolForeignCur
'									strSourceFlow = "Top"
									Source = $"	-A#750500:F#F_ADD{TopPOVSource}
												-A#750500:F#F_DISP{TopPOVSource}
												-A#750600:F#F_ADD{TopPOVSource}
												-A#750600:F#F_DISP{TopPOVSource}
												-A#750502:F#F_ADD{TopPOVSource}
												-A#750502:F#F_DISP{TopPOVSource}
												-A#750503:F#F_ADD{TopPOVSource}
												-A#750503:F#F_DISP{TopPOVSource}
												-A#750504:F#F_ADD{TopPOVSource}
												-A#750504:F#F_DISP{TopPOVSource}
												-A#750501:F#F_ADD{TopPOVSource}
												-A#750501:F#F_DISP{TopPOVSource}
												-A#430150:F#Top{TopPOVSource}
												-A#430800:F#Top{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	 14_6_2 - Banking Cost 	_____________"	
							Case "14_6_2"	
							If BoolLocalCurrency Or BoolForeignCur
'									strSourceFlow = "Top"
									Source = $"	-A#431000:F#Top{TopPOVSource}
												-A#431100:F#Top{TopPOVSource}
												-A#BankingCost_Plan:F#Top{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	 14_7 - Other 	_____________"	
							Case "14_7"	
								If BoolLocalCurrency Or BoolForeignCur
							'	MISC_CF
									strSourceFlow = "F_ADD"
									Source = $"	A#MISC_CF:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	14_8_1 - BS movement in CIT payable and receivable	_____________"	
							Case "14_8_1"	
								If BoolLocalCurrency Or BoolForeignCur
							'	11_2_6 - Tax receivables
							'	12_2_2_7 - Tax payables
									strSourceFlow = "Flow_4"
									Source = $"	-A#11_2_6:F#{strSourceFlow}{TopPOVSource}
												-A#12_2_2_7:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							#Region "______________	14_8_2 - PL corporate income tax line	_____________"	
							Case "14_8_2"	
								If BoolLocalCurrency Or BoolForeignCur
							'	5_1_2 - Income tax before capital operations
							'   480100
							'   PPADT480 
									strSourceFlow = "Top"
									Source = $"	-A#5_1_2:F#{strSourceFlow}{TopPOVSource}
												+A#480100:F#{strSourceFlow}{TopPOVSource}
												+A#PPADT480:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							
							#Region "______________	14_9_1 - IC RECEIVABLES	_____________"	
							Case "14_9_1"	
								If BoolLocalCurrency Or BoolForeignCur
							'	520000
							'	Indirect_Elim_TradeRec
							'	123403
									strSourceFlow = "Flow_4"
									Source = $"	-A#520000:F#{strSourceFlow}{TopPOVSource}
												-A#Indirect_Elim_TradeRec:F#{strSourceFlow}{TopPOVSource}
												-A#123403:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							
							#Region "______________	14_9_2 - IC PAYABLES	_____________"	
							Case "14_9_2"	
								If BoolLocalCurrency Or BoolForeignCur
							'	720000
							'	714000
									strSourceFlow = "Flow_4"
									Source = $"	-A#720000:F#{strSourceFlow}{TopPOVSource}
												-A#714000:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	14_10 - Mapex Eq Leases	_____________"	'IAA - Added as per Szilvia Sarusi-Kis request "Management  CF Calculation Changes" on 2025-11-13 
							Case "14_10"	
								If BoolLocalCurrency Or BoolForeignCur
							'	Mapex_Eq_Leases
									strSourceFlow = "F_ADD"
									Source = $"	A#CAPEX_OPELEASE_M:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_1_1 - Tangible	_____________"	
							Case "15_1_1"	
								If BoolLocalCurrency Or BoolForeignCur
							'	A#CAPEX_TANG_P
									strSourceFlow = "F_ADD"
									Source = $"	-A#CAPEX_TANG_P:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_1_2 - Finance Lease	_____________"	
							Case "15_1_2"	
								If BoolLocalCurrency Or BoolForeignCur
							'	A#CAPEX_FINLEASE_P
									strSourceFlow = "F_ADD"
									Source = $"	-A#CAPEX_FINLEASE_P:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
'							#Region "______________	15_1_3 - Operating Lease	_____________"	
'							Case "15_1_3"	
'								If BoolLocalCurrency Or BoolForeignCur
'							'	A#CAPEX_OPELEASE_P
'									strSourceFlow = "Top"
'									Source = $"	-A#CAPEX_OPELEASE_P:F#{strSourceFlow}{TopPOVSource}"
'									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveNodata({source})"
'									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'								End If
'							#End Region	
							#Region "______________	15_1_4 - IT	_____________"	
							Case "15_1_4"	
								If BoolLocalCurrency Or BoolForeignCur
							'	A#CAPEX_IT_P
									strSourceFlow = "F_ADD"
									Source = $"	-A#CAPEX_IT_P:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_1_5 - INV SUP P	_____________"	
							Case "15_1_5"	
								If BoolLocalCurrency Or BoolForeignCur
							'	A#INV_SUP_P
									strSourceFlow = "F_ADD"
									Source = $"	-A#INV_SUP_P:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_2_1 - M&A openning balance	_____________"	
							Case "15_2_1"	
								If BoolLocalCurrency Or BoolForeignCur
							'	12_1 - Total equity
							'		strSourceFlow = "Top"
									Source = $"	A#12_1:F#F_Acquisitions{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_2_2 - M&A TD	_____________"	
							Case "15_2_2"	
								If BoolLocalCurrency Or BoolForeignCur
							'	11_1_1_2 - Trademarks
							'		strSourceFlow = "Top"
									Source = $"	-A#11_1_1_2:F#F_Acquisitions{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_2_3 - M&A CV	_____________"	
							Case "15_2_3"	
								If BoolLocalCurrency Or BoolForeignCur
							'	11_1_1_3 - Contract value
							'		strSourceFlow = "Top"
									Source = $"	-A#11_1_1_3:F#F_Acquisitions{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	15_2_4 - M&A DTA/DTL	_____________"	
							Case "15_2_4"	
								If BoolLocalCurrency Or BoolForeignCur
							'	 11_1_5 - Deferred tax assets 
                            '    12_2_1_4 - Deferred tax liabilities 
							'		strSourceFlow = "Top"
									Source = $"	A#11_1_5:F#F_Acquisitions{TopPOVSource}
												-A#12_2_1_4:F#F_Acquisitions{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	15_2_5 - M&A Goodwill	_____________"	
							Case "15_2_5"	
								If BoolLocalCurrency Or BoolForeignCur
							'	 11_1_2 - Goodwill
							'		strSourceFlow = "Top"
									Source = $"	-A#11_1_2:F#F_Acquisitions{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	15_3_7 - Cash from M&A	_____________"	
							Case "15_3_7"	
								If BoolLocalCurrency Or BoolForeignCur
							'	 11_2_4 - Cash and cash equivalents
							'		strSourceFlow = "Top"
									Source = $"	A#11_2_4:F#F_Acquisitions{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_2_7 - Deferred considerations	_____________"	
							Case "15_2_7"	
								If BoolLocalCurrency Or BoolForeignCur
							'	 12_2_1_8 - Deferred consideration non-current
							'	 12_2_2_12 - Deferred consideration current
							'		strSourceFlow = "Top"
									Source = $"	A#12_2_1_8:F#F_Acquisitions{TopPOVSource}
												-A#12_2_1_8:F#F_DISP{TopPOVSource}
												-A#12_2_1_8:F#F_ADD{TopPOVSource} 
												+A#12_2_2_12:F#F_Acquisitions{TopPOVSource}
												-A#12_2_2_12:F#F_DISP{TopPOVSource}
												-A#12_2_2_12:F#F_ADD{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_3_1 - Loans	_____________"	
							Case "15_3_1"	
								If BoolLocalCurrency Or BoolForeignCur
							'		strSourceFlow = "Flow_4"
									Source = $"	-A#12_2_1_6:F#F_DISP{TopPOVSource}
												-A#12_2_1_6:F#F_ADD{TopPOVSource}
												-A#12_2_2_10:F#F_DISP{TopPOVSource}
												-A#12_2_2_10:F#F_ADD{TopPOVSource}
												-A#11_2_3:F#F_DISP{TopPOVSource}
												-A#11_2_3:F#F_ADD{TopPOVSource}
												+A#521000:F#F_DISP{TopPOVSource}
												+A#521000:F#F_ADD{TopPOVSource}
												+A#522000:F#F_DISP{TopPOVSource}
												+A#522000:F#F_ADD{TopPOVSource}
												+A#538200:F#F_DISP{TopPOVSource}
												+A#538200:F#F_ADD{TopPOVSource}
												+A#538210:F#F_DISP{TopPOVSource}
												+A#538210:F#F_ADD{TopPOVSource}
												+A#538400:F#F_DISP{TopPOVSource}
												+A#538400:F#F_ADD{TopPOVSource}
												+A#538500:F#F_DISP{TopPOVSource}
												+A#538500:F#F_ADD{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	15_3_2 - Others	_____________"	
							Case "15_3_2"	
								If BoolLocalCurrency Or BoolForeignCur
							'		strSourceFlow = "Flow_4"
									Source = $"	-A#11_1_6:F#F_ADD{TopPOVSource}
												-A#11_1_6:F#F_DISP{TopPOVSource}
												-A#12_2_2_11:F#F_ADD{TopPOVSource}
												-A#12_2_2_11:F#F_DISP{TopPOVSource}
												+A#600100:F#F_ADD{TopPOVSource}
												+A#600100:F#F_DISP{TopPOVSource}
												+A#600000:F#F_ADD{TopPOVSource}
												+A#600000:F#F_DISP{TopPOVSource}
												+A#721000:F#F_ADD{TopPOVSource}
												+A#721000:F#F_DISP{TopPOVSource}
												+A#722000:F#F_ADD{TopPOVSource}
												+A#722000:F#F_DISP{TopPOVSource}
												+A#738210:F#F_ADD{TopPOVSource}
												+A#738210:F#F_DISP{TopPOVSource}
												+A#738200:F#F_ADD{TopPOVSource}
												+A#738200:F#F_DISP{TopPOVSource}
												+A#750500:F#F_ADD{TopPOVSource}
												+A#750500:F#F_DISP{TopPOVSource}
												+A#750600:F#F_ADD{TopPOVSource}
												+A#750600:F#F_DISP{TopPOVSource}
												+A#750502:F#F_ADD{TopPOVSource}
												+A#750502:F#F_DISP{TopPOVSource}
												+A#750503:F#F_ADD{TopPOVSource}
												+A#750503:F#F_DISP{TopPOVSource}
												+A#750504:F#F_ADD{TopPOVSource}
												+A#750504:F#F_DISP{TopPOVSource}
												+A#750501:F#F_ADD{TopPOVSource}
												+A#750501:F#F_DISP{TopPOVSource}
												+A#738400:F#F_ADD{TopPOVSource}
												+A#738400:F#F_DISP{TopPOVSource}
												+A#738500:F#F_ADD{TopPOVSource}
												+A#738500:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region	
							#Region "______________	15_3_3 - Capital Increase	_____________"	
							Case "15_3_3"	
								If BoolLocalCurrency Or BoolForeignCur
							' Capital_Increase
									strSourceFlow = "F_ADD"
									Source = $"	A#Capital_Increase:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region								
							#Region "______________	15_3_4 - Intercompany Loans	_____________"	
							Case "15_3_4"	
								If BoolLocalCurrency Or BoolForeignCur
							'		strSourceFlow = "Flow_4"
									Source = $"	-A#721000:F#F_DISP{TopPOVSource}
												-A#721000:F#F_ADD{TopPOVSource}
												-A#722000:F#F_DISP{TopPOVSource}
												-A#722000:F#F_ADD{TopPOVSource}
												-A#738210:F#F_DISP{TopPOVSource}
												-A#738210:F#F_ADD{TopPOVSource}
												-A#738200:F#F_DISP{TopPOVSource}
												-A#738200:F#F_ADD{TopPOVSource}
												-A#600100:F#F_DISP{TopPOVSource}
												-A#600100:F#F_ADD{TopPOVSource}
												-A#600000:F#F_DISP{TopPOVSource}
												-A#600000:F#F_ADD{TopPOVSource}
												-A#600200:F#F_DISP{TopPOVSource}
												-A#600200:F#F_ADD{TopPOVSource}
												-A#521000:F#F_DISP{TopPOVSource}
												-A#521000:F#F_ADD{TopPOVSource}
												-A#522000:F#F_DISP{TopPOVSource}
												-A#522000:F#F_ADD{TopPOVSource}
												-A#12_2_1_7:F#F_DISP{TopPOVSource}
												-A#12_2_1_7:F#F_ADD{TopPOVSource}
												-A#538200:F#F_DISP{TopPOVSource}
												-A#538200:F#F_ADD{TopPOVSource}
												-A#538210:F#F_DISP{TopPOVSource}
												-A#538210:F#F_ADD{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region							
							#Region "______________	 15_3_5 - Intercompany Deposits 	_____________"	
							Case "15_3_5"	
							If BoolLocalCurrency Or BoolForeignCur
							'		strSourceFlow = "Flow_4"
									Source = $"	-A#538400:F#F_DISP{TopPOVSource}
												-A#538400:F#F_ADD{TopPOVSource}
												-A#538500:F#F_DISP{TopPOVSource}
												-A#538500:F#F_ADD{TopPOVSource}
												-A#738400:F#F_ADD{TopPOVSource}
												-A#738400:F#F_DISP{TopPOVSource}
												-A#738500:F#F_ADD{TopPOVSource}
												-A#738500:F#F_DISP{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region		
'							#Region "______________	15_3_6 - Lease Liabilities	_____________"	
'							Case "15_3_6"	
'								If BoolLocalCurrency Or BoolForeignCur
'							'		strSourceFlow = "Flow_4"
'									Source = $"	-A#12_2_1_5:F#F_DISP{TopPOVSource}
'												-A#12_2_1_5:F#F_ADD{TopPOVSource}
'												-A#12_2_2_9:F#F_DISP{TopPOVSource}
'												-A#12_2_2_9:F#F_ADD{TopPOVSource}"
'									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveNodata({source})"
'									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'								End If
'							#End Region	
							#Region "______________	 15_4 - Propex Eq. Leases 	_____________"	'IAA - Added as per Szilvia Sarusi-Kis request "Management  CF Calculation Changes" on 2025-11-13 
							Case "15_4"	
							If BoolLocalCurrency Or BoolForeignCur
									strSourceFlow = "F_ADD"
									Source = $"	A#CAPEX_OPELEASE_P:F#{strSourceFlow}{TopPOVSource}"
									Destination = $"A#{strAccountName}:F#CF_Calc{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNodata({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region		
							
'							__________________Managerial Cash Flow _ End __________________
							
							
						End Select
'					End If

'				If api.Time.GetYearFromId>=2025 Or (api.Time.GetYearFromId=2024 And currMonthNum=10) Or strFlowname.XFEqualsIgnoreCase("F_Retained_Earnings_Mov_1") Or
				If api.Time.GetYearFromId>=2025 Or strFlowname.XFEqualsIgnoreCase("F_Retained_Earnings_Mov_1") Or
					(api.Time.GetYearFromId >= 2023 And api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual_Dummy"))

'				If api.Time.GetYearFromId>=2024 Or strFlowname.XFEqualsIgnoreCase("F_Retained_Earnings_Mov_1") Or
'					(api.Time.GetYearFromId >= 2023 And api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual"))
					
'					If BoolLocalCurrency And BaseEntity Or BoolForeignCur 

'					If BoolLocalCurrency And BaseEntity Or BoolForeignCur Or OwnerPostAdj Or 'Commented out by DAGO on 20250407 to include OwnerPreAdj
					If BoolLocalCurrency And BaseEntity Or BoolForeignCur Or (OwnerPostAdj Or OwnerPostAdj) Or 
					(strFlowname.XFEqualsIgnoreCase("F_Retained_Earnings_Mov_1") And api.Pov.Cons.Name.XFEqualsIgnoreCase("Elimination"))

						Select Case strFlowname
						#Region "______________INTANGIBLE ASSETS______________"
						Case "F_Intangibles_Mov_1"	
							If BoolForeignCur 
							'	F_Intangibles_Mov_1 - Currency translation differences Gross Value
							'	Formula= (( Balance December previous year (accounts PPACC631+PPACE631+PPATM632+613000+613050+613090+SC6130+SC6135+631000+631100+631200+PPAC6311+PPAE6311+PPAT6322)/December previous year closing FX)
							'	-( Balance December previous year (accounts PPACC631+PPACE631+PPATM632+613000+613050+613090+SC6130+SC6135+631000+631100+631200+PPAC6311+PPAE6311+PPAT6322)/current month closing FX))
							'	Formula Pass 12
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								AccountFilter = "A#PPACC631, A#PPACE631, A#PPATM632, A#613000, A#613050, A#613090, A#SC6130, A#SC6135, A#631000, A#631100, A#631200, A#PPAC6311, A#PPAE6311, A#PPAT6322, A#1_1_1_4"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Intangibles_Mov_2"
							If BoolLocalCurrency
							'	"F_Intangibles_Mov_2 - Additions Gross value
							'	Formula :Current month YTD-December previous closing YTD -F_Intangibles_Movements
							'	Formula Pass 10
							'	AccountFilter = "A#PPACC631, A#PPACE631, A#PPATM632, A#613000, A#613050, A#613090, A#SC6130, A#SC6135, A#631000, A#631100, A#631200, A#PPAC6311, A#PPAE6311, A#PPAT6322, A#1_1_1_4"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								AccountFilter ="A#613000, A#SC6130, A#PPATM632, A#631000, A#PPACC631, A#PPACE631, A#1_1_1_4"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Intangibles_Mov_3"
							If BoolLocalCurrency
							'	"F_Intangibles_Mov_3 - Acquisition through business combination Gross Value
							'	Formula: Formula To specific codes In data staging area
							'	Formula Pass 9
								AccountFilter = "A#PPACC631, A#PPACE631, A#PPATM632, A#613000, A#613050, A#613090, A#SC6130, A#SC6135, A#631000, A#631100, A#631200, A#PPAC6311, A#PPAE6311, A#PPAT6322"
								Source = $"F#F_CLO{TopPOVSource_ACQ}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If

						Case "F_Intangibles_Mov_7"
							If BoolForeignCur 
							'	'"F_Intangibles_Mov_7 - Currency translation differences Acc Amortization
                             ' 	Formula=((Charge Of the year Amortization ( accounts 401000, 402000,PPATM402,PPACV402,PPACC402,PPACE402,PPACV400)/December previous year closing FX)-Charge Of the year Amortization 
                              ' 	( accounts 401000, 402000,PPATM402,PPACV402,PPACC402,PPACE402,PPACV400)/current month average FX))+ 
                              '	((Impairment losses recognised (reversed) ( accounts 450100, 452000,452100)/December previous year closing FX)-(Impairment losses recognised (reversed) ( accounts 450100, 452000,452100)/current month average FX))
								AccountFilter = "A#PPATM402, A#PPACV402, A#PPACC402, A#PPACE402, A#PPACV400"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If

						Case "F_Intangibles_Mov_8"
							If BoolLocalCurrency
							'	F_Intangibles_Mov_8 - Charge Of the year Amortization
							'	 Formula: P&L YTD selected accounts
								Source = $"-A#8_1_1_2_1_1_1_2:F#Top{TopPOVSource}"
								Destination = $"A#1_1_1_4:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If 
						Case "F_Intangibles_Mov_9"
							If BoolLocalCurrency
						'	F_Intangibles_Mov_9 - Impairment losses recognised (reversed) A#450100,A#452100,A#452000
								Source = $"-A#450100:F#Top{TopPOVSource}-A#452100:F#Top{TopPOVSource}-A#452000:F#Top{TopPOVSource}"
								Destination = $"A#1_1_1_4:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_Intangibles_Mov_10"
							If BoolLocalCurrency
							'	"F_Intangibles_Mov_10 - Acquisition through business combination Acc Amortization
							'	Formula: Formula to specific codes in data staging area
							'	Formula Pass 9
								AccountFilter = "A#PPATM402, A#PPACV402, A#PPACC402, A#PPACE402, A#PPACV400"
								Source = $"F#F_CLO{TopPOVSource_ACQ}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If

						Case "F_Intangibles_Mov_14"
							If BoolLocalCurrency
								'F_Intangibles_Mov_14 - Other changes during the period Acc Amortization: differences between BS and P&L
								'Formula :(Current month YTD Acc depreciation & Impairments-December previous closing YTD Acc depreciation & Impairments)-F_Intangibles Mov. 8-F_Intangibles Mov. 9
								'Formula pass 10
								AccountFilter = "A#613050, A#613090, A#SC6135, A#PPAT6322, A#631100, A#631200, A#PPAC6311, A#PPAE6311"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Intangibles_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_Intangibles_Mov_15"
							If BoolLocalCurrency
							'	F_Intangibles_Mov_15 - Gain/loss on intangible sales
                             '	GL Accounts is not created yet
							'	AccountFilter = "A#"
							'	Source = $"F#None:C#Local{TopPOVSource}"
							'	Destination = $"F#{strFlowname}{TopPOVDestination}"
							'	Formula = $"{Destination}=RemoveZeros({source})"
							'	api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						#End Region	
							
						#Region "______________GOODWILL______________"
						Case "F_Goodwill_Mov_1"
						'	F_Goodwill_Mov_1 - Currency translation differences Gross Value
						'	Formula= (( Balance December previous year (account 630000)/
						'	December previous year closing FX)-( Balance December previous year (account630000)/current month closing FX))
							If BoolForeignCur
								AccountFilter = "A#1_1_2.base.remove(A#630100)" '"A#630000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Goodwill_Mov_2"
						'	F_Goodwill_Mov_2 - Recognized through business combination Gross Value
						'	Formula= Formula: Current month YTD-December previous closing YTD
							If BoolLocalCurrency
								AccountFilter = "A#1_1_2.base.remove(A#630100)" '"A#630000"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Goodwill_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
'						'Case "F_Goodwill_Mov_3"
'							'F_Goodwill_Mov_3 - Derecognized on disposal of a susbidiary (-) Gross Value
'							'Formula= P&L GL not created
'							If BoolLocalCurrency
'								AccountFilter = ""
'								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Goodwill_Movements{TopPOVSource}"
'								Destination = $"F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'							End If
						Case "F_Goodwill_Mov_6"
'							F_Goodwill_Mov_6 - Currency translation differences Acc Impairment
'							Formula= Formula=+ ((Impairment losses recognised (reversed) ( accounts 630100)
'							/December previous year closing FX)-(Impairment losses recognised (reversed) ( accounts 630100)/current month average FX)
'							Comment BH: Formula needs updated:'
							If BoolForeignCur
								AccountFilter = "A#630100"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Goodwill_Mov_7"
'							F_Goodwill_Mov_7 - Impairment losses for the year
'							Formula= P&L YTD selected accounts
							If BoolLocalCurrency
								Source = $"-A#452000:F#Top{TopPOVSource}"
								Destination = $"A#630100:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						Case "F_Goodwill_Mov_10"
							If BoolLocalCurrency
'								F_Goodwill_Mov_10 - Impairment losses for the year
'								Formula= :(Current month YTD Acc depreciation & Impairments-December previous closing YTD Acc depreciation & Impairments)-F_Goodwill Mov.7
'								Formula pass 10
								AccountFilter = "A#630100"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Goodwill_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
							
						#End Region
							
						#Region "______________PROPERTY PLANT And EQUIPMENT______________"
						Case "F_Property_plant_and_equipment_Mov_1"
							If BoolLocalCurrency
'								F_Property_plant_and_equipment_Mov_1- Additions Gross Value
'								Formula= :Formula :Current month YTD-December previous closing YTD
'								Formula pass 10
								AccountFilter = "A#610000,A#610002,A#611000,A#611100,A#612000,A#612100,A#616000,A#614000,A#614999,A#612006,A#612002,A#614011,A#614090,A#PPABA610,A#624000,A#123463,A#123461,A#123462,A#123464,A#123465,A#123466,A#123467"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Property_plant_and_equipment_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 

						Case "F_Property_plant_and_equipment_Mov_3"
							If BoolForeignCur
'								F_Property_plant_and_equipment_Mov_3- Currency translation differences Gross Value
'								Formula= :Formula= (( Balance December previous year (accounts PPABA610,610000,610002,611000,611100,612000,612100,616000,614000,614999,612006,615000)/December previous year closing FX)-
'								(Balance December previous year (accounts PPABA610, 610000,610002,611000,611100,612000,612100,616000,614000,614999,612006,615000)/current month closing FX))
'								Formula pass 12
								AccountFilter = "A#610000, A#610002, A#PPABA610, A#611000, A#612000, A#612006"', A#620000, A#621000, A#622000, A#PPARO624"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Property_plant_and_equipment_Mov_4"
							If BoolLocalCurrency
'								F_Property_plant_and_equipment_Mov_4 - Impairment losses For the year
'								Formula= Disposals (-) Gross Value
'								AccountFilter = "A#450100, A#452000, A#452100"
'								Formula pass 9
								Source = $"-A#450100:F#Top{TopPOVSource}-A#452000:F#Top{TopPOVSource}-A#452100:F#Top{TopPOVSource}"
								Destination = $"A#612056:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)', accountFilter:=AccountFilter)
							End If

						Case "F_Property_plant_and_equipment_Mov_6"
							If BoolLocalCurrency
'							F_Property_plant_and_equipment_Mov_6 - Depreciation charge For the year
'							Formula= P&L YTD selected accounts
'							AccountFilter = "A#400000, A#PPABU403, A#403050"
								Source = $"-A#400000:F#Top{TopPOVSource}-A#PPABU403:F#Top{TopPOVSource}-A#403050:F#Top{TopPOVSource}"
								Destination = $"A#612056:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)', accountFilter:=AccountFilter)
							End If
						Case "F_Property_plant_and_equipment_Mov_7"
							If BoolLocalCurrency
'							F_Property_plant_and_equipment_Mov_7 - Impairment losses recognized/(reversed)
'							Formula= P&L YTD selected accounts

'								AccountFilter = "A#450000,A#PPABA403,A#PPACV403"
								Source = $"-A#450000:F#Top{TopPOVSource}-A#PPABA403:F#Top{TopPOVSource}-A#PPACV403:F#Top{TopPOVSource}"
								Destination = $"A#612056:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)', accountFilter:=AccountFilter)
							End If
						Case "F_Property_plant_and_equipment_Mov_8"
							If BoolForeignCur
							'F_Property_plant_and_equipment_Mov_8- Currency translation differences Acc Depreciation/Impairment
							'Formula= :Formula=((Charge of the year Amortization ( accounts PPABA615,610050,611050,611150,612050,612150,612056)/December previous year closing FX)-
							'Charge Of the year Amortization ( accounts PPABA615,610050,611050,611150,612050,612150,612056)/current month average FX))+ ((Impairment losses recognised (reversed)
							'( accounts 610090,611090,611190,612090,612190,612096)/December previous year closing FX)-(Impairment losses recognised (reversed) ( accounts 610090,611090,611190,612090,612190,612096)/current month average FX))
								AccountFilter = "A#1_1_3_1.base.Remove(A#610000, A#610002, A#PPABA610, A#611000, A#612000, A#612006, A#620000, A#621000, A#622000, A#PPARO624)"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Property_plant_and_equipment_Mov_12"
						'	F_Property_plant_and_equipment_Mov_12 - Other changes during the period  Acc Depreciation/Impairment: differences between BS And P&L
						'	Formula= Formula:(Current month YTD Acc depreciation & Impairments-December previous closing YTD Acc depreciation & Impairments)-F_Property, plant And equipment Mov.6-F_Property, plant And equipment Mov.7
						'	Comment BH: How To check accounts?'
							If BoolLocalCurrency
								AccountFilter = "A#1_1_3_1.Base.Remove(A#610000, A#610002, A#PPABA610, A#611000, A#612000, A#612006, A#620000, A#621000, A#622000, A#PPARO624)"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Property_plant_and_equipment_Movements{TopPOVSource}+{Destination}"
								
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Property_plant_and_equipment_Mov_13"
							If BoolLocalCurrency
						'	F_Property_plant_and_equipment_Mov_13 - Gain/loss On FA sale
						'	Formula= P&L YTD selected accounts
			
								AccountFilter = "A#6_1_5_2_2_1_9.Base"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#Top{TopPOVSource}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							
'								Source = $"-A#410200:F#Top{TopPOVSource}"
'								Destination = $"A#612056:F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula)', accountFilter:=AccountFilter)
							End If 
						#End Region 

						#Region "______________BANK LOANS AND OTHER LOANS______________"
					'		F_Bank_loans_and_other_loans_LT_Mov_1	F_Bank_loans_and_other_loans_LT_Mov_4	F_Bank_loans_and_other_loans_LT_Mov_5	F_Bank_loans_and_other_loans_LT_Mov_6
						Case "F_Bank_loans_and_other_loans_LT_Mov_1"
							If BoolLocalCurrency
					'			F_Bank_loans_and_other_loans_LT_Mov_1- Additions Gross Value
					'			Formula= :Formula :Current month YTD-December previous closing YTD
					'			Formula pass 10
								AccountFilter = "A#2_2_1_5_1.Base,A#2_2_1_5_6.Base,A#123535,A#123536"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Bank_loans_and_other_loans_LT_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_Bank_loans_and_other_loans_LT_Mov_4"
							If BoolLocalCurrency
					'			F_Bank_loans_and_other_loans_LT_Mov_1- Additions Gross Value
					'			Formula= :Formula :Current month YTD-December previous closing YTD
					'			Formula pass 9
								Source = $"-A#433000:F#Top{TopPOVSource.Replace("I#Top","I#None")}"
								Destination = $"A#813000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
								
								Source = $"-A#433000:F#Top{TopPOVSource.Replace(":I#Top","")}"
								Destination = $"A#810500:F#{strFlowname}{TopPOVDestination.Replace(":I#None","")}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula,icfilter:="I#ICEntities.Base")
							End If 
						Case "F_Bank_loans_and_other_loans_LT_Mov_5"
							If BoolLocalCurrency
						'		F_Bank_loans_and_other_loans_LT_Mov_1- Additions Gross Value
						'		Formula= :Formula :Current month YTD-December previous closing YTD
						'		Formula pass 9
								Source = $"-A#435000:F#Top{TopPOVSource.Replace("I#Top","I#None")}"
								Destination = $"A#814000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
								
								Source = $"-A#435000:F#Top{TopPOVSource.Replace(":I#Top","")}"
								Destination = $"A#810600:F#{strFlowname}{TopPOVDestination.Replace(":I#None","")}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, icfilter:="I#ICEntities.Base")
							End If 
						
						Case "F_Bank_loans_and_other_loans_LT_Mov_6"
							If BoolForeignCur
					'		F_Bank_loans_and_other_loans_LT_Mov_6- Currency translation differences
					'		Formula pass 12
								AccountFilter  = "A#2_2_1_5_1.Base, A#2_2_1_5_6.Base"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Bank_loans_and_other_loans_LT_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If

							#End Region 
							
						#Region "______________ROU Right of Use______________"
							
						Case "F_RoU_Mov_1"
							If BoolLocalCurrency 
							'	F_RoU_Mov_1 - Additions Gross Value
							'	Formula= Formula :Current month YTD-December previous closing YTD
								AccountFilter = "A#620000,A#621000,A#622000,A#623000,A#PPARO624"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_RoU_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_RoU_Mov_3"
							If BoolForeignCur
							'	F_RoU_Mov_3 - Currency translation differences Gross Value
							'	Formula= Formula= (( Balance December previous year (accounts 620000,621000,622000,623000,624000)/December previous year closing FX)-
							'	( Balance December previous year (accounts 620000,621000,622000,623000,624000)/current month closing FX))
								AccountFilter = "A#620000, A#621000, A#622000, A#623000, A#624000, A#PPARO624"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_RoU_Mov_6"
							If BoolLocalCurrency 
							'	F_RoU_Mov_6 - Depreciation charge For the year
							'	Formula= Formula P&L YTD selected accounts
							'	AccountFilter = "A#PPAROU403"
								Source = $"-A#PPAROU403:F#Top{TopPOVSource}-A#403000:F#Top{TopPOVSource}"
								Destination = $"A#PPAR6245:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						Case "F_RoU_Mov_7"
							If BoolLocalCurrency 
						'		F_RoU_Mov_7 - Impairment losses recognized/(reversed)
						'		Formula= Formula P&L YTD selected accounts								
						'		AccountFilter = "A#451000"
								Source = $"-A#451000:F#Top{TopPOVSource}"
								Destination = $"A#PPARO624:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If

						Case "F_RoU_Mov_8"
							If BoolForeignCur 
						'		F_RoU_Mov_8- Currency translation differences Acc Depreciation/Impairment
						'		Formula= :Formula=((Charge Of the year Amortization ( accounts PPARO624,PPAR6245,620050,621050,622050,624050)/current month average FX)-
						'		(Charge Of the year Amortization ( accounts PPARO624,PPAR6245,620090,621090,622090)/current month average FX))+ ((Impairment losses recognised (reversed) ( accounts 620090,621090,622090)/December previous year closing FX)-
						'		(Impairment losses recognised (reversed) ( accounts 620090,621090,622090)/current month average FX))
								AccountFilter = "A#1_1_3_2.base.Remove(A#620000, A#621000, A#622000, A#623000, A#624000, A#PPARO624)"'1_1_3_2"A#PPAR6245,A#620050,A#620090,A#621050,A#621090,A#622050,A#622090,A#624050"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_RoU_Mov_12"
							If BoolLocalCurrency
						'		F_RoU_Mov_12 - Other changes during the period  Acc Depreciation/Impairment: differences between BS And P&L
						'		Formula :(Current month YTD Acc depreciation & Impairments-December previous closing YTD Acc depreciation & Impairments)-F_RoU Mov.6-F_RoU Mov.7
						'		Formula pass 10
								AccountFilter = "A#1_1_3_2.base.Remove(A#620000, A#621000, A#622000, A#623000, A#624000, A#PPARO624)"'1_1_3_2"A#PPAR6245,A#620050,A#620090,A#621050,A#621090,A#622050,A#622090,A#624050"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_RoU_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_RoU_Mov_13"
							If BoolLocalCurrency 
					'			F_RoU_Mov_13 - Gain/loss On FA sale
					'			Formula= P&L YTD selected accounts
								Source = $"-A#411000:F#Top{TopPOVSource}"
								Destination = $"A#PPARO624:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						#End Region
						
						#Region "______________Equity Investment______________"
						Case "F_Equity_investments_Mov_1"
							 If BoolLocalCurrency
					'			F_Equity_investments_Mov_1 - Additions
					'			Formula= Current month YTD-December previous closing YTD
					'			AccountFilter = "A#650000,A#123508"
								AccountFilter = "A#1_1_7.Base"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Equity_investments_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_Equity_investments_Mov_2"
							If BoolLocalCurrency
					'		F_Equity_investments_Mov_2 - Impairment
					'		Formula= P&L YTD selected accounts
					'		A#4_1_1_2
								Source = $"-A#4_1_1_2:F#Top{TopPOVSource}"
								Destination = $"A#650000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						Case "F_Equity_investments_Mov_3"
							If BoolLocalCurrency
					'			F_Equity_investments_Mov_3 - Disposals at acquisition cost
					'			Formula= P&L YTD selected accounts
								Source = $"-A#4_1_1_1:F#Top{TopPOVSource}"
								Destination = $"A#650000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						Case "F_Equity_investments_Mov_4"
					'		F_Equity_investments_Mov_4 - Currency translation differences 
					'		Formula= Formula= (( Balance December previous year (accounts 650000)/December previous year closing FX)-
					'		( Balance December previous year (accounts650000)/current month closing FX))
					'		Comment BH: How To check accounts?'
							If BoolForeignCur
								AccountFilter = "A#650000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
							
						#Region "______________DEFERRED TAX______________"
						Case "F_Deferred_tax_Mov_1"
							If BoolLocalCurrency
					'			F_Deferred_tax_Mov_1 - Additions/decreases
								AccountFilter = "A#1_1_9.BASE, A#2_2_1_3.BASE"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Deferred_tax_Movements{TopPOVSource}+{Destination}"
								
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Deferred_tax_Mov_3"
							If BoolForeignCur
					'			F_Deferred_tax_Mov_3 - Currency translation differences
								AccountFilter = "A#1_1_9.BASE, A#2_2_1_3.BASE"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Deferred_tax_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
							
						Case "F_Deferred_tax_Mov_4"
					'		F_Deferred_tax_Mov_4 - 480100:Deferred tax expense: difference between BS And P&L
					'		Formula= Formula :Current month YTD-December previous closing YTD-F_Deferred tax Mov.1-F_Deferred tax Mov.2-F_Deferred tax Mov.3
							If BoolLocalCurrency '480100
								AccountFilter = ""
								Source = $"-A#480100:F#Top{TopPOVSource}"
								Destination = $"A#850000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)', accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________Other non current financial assets______________"
				         Case "F_Other_non_current_financial_assets_Mov_1"
					'		F_Other_non-current_financial_assets_Mov_1 - Additions/decreases
					'		Formula= Current month YTD-December previous closing YTD
					'		Comment BH: How To check accounts?'
							If BoolLocalCurrency
								AccountFilter = "A#1_1_11_3.Base, A#1_1_11_2.Base, A#1_1_11_6.Base, A#123512" '"A#600100,A#673000,A#676300,A#600000,A#600200,A#123402,A#123403,A#123512,A#674000,A#600300,A#678000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
				         Case "F_Other_non_current_financial_assets_Mov_3"
					'		F_Other_non-current_financial_assets_Mov_3 - Currency translation differences 
					'		Formula= Formula= (( Balance December previous year (accounts 600100,673000,676300,600000,600200,674000,123402,123403, )
					'		/December previous year closing FX)-( Balance December previous year (accounts 600100,673000,676300,600000,600200,674000,123402,123403)/current month closing FX))
							If BoolForeignCur
								AccountFilter = "A#1_1_11_3.Base, A#1_1_11_2.Base, A#1_1_11_6.Base"'"A#600100, A#673000, A#676300, A#600000, A#600200, A#674000, A#123402, A#123403"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region	
							
						#Region "______________Other non current non financial assets_____________"
				         Case "F_Other_non_current_non_financial_assets_Mov_1"
							 If BoolLocalCurrency
						'   		F_Other_non-current_non_financial_assets_Mov_1 - Additions/decreases
						'		Formula= Current month YTD-December previous closing YTD
								AccountFilter = "A#671000,A#672000,A#676100,A#676200,A#677000,A#677100,A#Z67620,A#123513"
								Destination = $"F#{strFlowname}{TopPOVDestination}"			
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Other_non_current_non_financial_assets_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
				         Case "F_Other_non_current_non_financial_assets_Mov_3"
						'	F_Other_non-current_non_financial_assets_Mov_3 - Currency translation differences 
						'	Formula= Formula= (( Balance December previous year (accounts ,671000,672000,676100,676200,676200,676200,677000,680000,Z67620)/December previous year closing FX)-
						'	( Balance December previous year (accounts ,671000,672000,676100,676200,676200,676200,677000,680000,Z67620)/current month closing FX))
							If BoolForeignCur
								AccountFilter = "A#1_1_12_2.Base"'"A#671000, A#672000, A#676100, A#676200, A#676200, A#676200, A#677000, A#680000, A#Z67620"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
						     End If
						#End Region	
						
						#Region "______________Inventories______________"
			             Case "F_Inventories_Mov_1"
							If BoolLocalCurrency 
								'F_ Inventories Mov.1 - Additions/decreases
								'Formula= Current month YTD-December previous closing YTD
								'Formulapass 10
								AccountFilter = "A#1_2_1.Base" '"A#561000,A#560010,A#560000,A#562000,A#123514"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Inventories_Movements{TopPOVSource}+{Destination}"
								Api.Data.ClearCalculatedData(Destination, True, True, True)
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
				         Case "F_Inventories_Mov_3"
							 If BoolForeignCur
								'F_ Inventories Mov.3 - Currency translation differences 
'								Formula= (( Balance December previous year (accounts 561000,560000,562000,560010,123514)/December previous year closing FX)-
'								( Balance December previous year (accounts 561000,560000,562000,560010,123514)/current month closing FX))'
'								Formulapass 12
								AccountFilter = "A#1_2_1.Base"' "A#561000,A#560000,A#562000A#560010#123514"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-({ParentFLOW}{TopPOVSource}-{Destination})"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
		                Case "F_Inventories_Mov_4"
							If BoolLocalCurrency 
'								'F_ Inventories Mov.4 -Reversals Of write-downs
'								'Formula= P&L GL Not created
'								FormulaPass9
'								AccountFilter = "A#"
'								Source = 
'								Destination = $"F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
							#End Region
							
						#Region "______________Trade Receivables______________"
							
						 Case "F_Trade_receivables_Mov_1"
							 If BoolLocalCurrency 
					'	   		F_Trade_receivables_Mov_1 - Additions/decreases
					'			Formula= Current month YTD-December previous closing YTD
						'		AccountFilter = "A#1_2_2_1.Base"
								AccountFilter = "A#510000,A#513000,A#515000,A#520000,A#521000,A#522000,A#123404,A#510020,A#541100"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"'
								Source = $"F#F_CLO{TopPOVSource}-(F#F_OPE{TopPOVSource})-{ParentFLOW}{TopPOVSource}+{Destination})"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
				         Case "F_Trade_receivables_Mov_3"
							 If BoolForeignCur
					'			F_Trade_receivables_Mov_3 - Currency translation differences 
					'			Formula= Formula= Formula= (( Balance December previous year (accounts 510000,511000,513000,515000,520000,521000,522000,123404)/December previous year closing FX)-
					'			( Balance December previous year (accounts 510000,511000,513000,515000,520000,521000,522000,123404)/current month closing FX))
							
								AccountFilter = "A#1_2_2_1.Base"' "A#510000, A#511000, A#513000, A#515000, A#520000, A#521000, A#522000, A#123404,A#512000, A#512100, A#514000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
						     End If
						Case "F_Trade_receivables_Mov_4"
							 If BoolLocalCurrency 
					'			F_Trade_receivables_Mov_4 - Dividends
					'			Formula= P&L YTD selected accounts
					'			AccountFilter = "A#436000,A#437000,A#438000"
								Source = $"-A#436000:F#Top{TopPOVSource}-A#437000:F#Top{TopPOVSource}-A#438000:F#Top{TopPOVSource}"
								Destination = $"A#521000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If 
						Case "F_Trade_receivables_Mov_5"
							If BoolLocalCurrency
					'			F_Trade_receivables_Mov_5 - Grants
					'			Formula= P&L YTD selected accounts
								Source = $"-A#430700:F#Top{TopPOVSource}"
								Destination = $"A#511000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						Case "F_Trade_receivables_Mov_6"
							If BoolLocalCurrency
					'			F_Trade_receivables_Mov_6 - Interest income - other investments
					'			Formula= P&L YTD selected accounts
								Source = $"-A#430850:F#Top{TopPOVSource}"
								Destination = $"A#510000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If 
				     	Case "F_Trade_receivables_Mov_8"
							If BoolLocalCurrency
					'			F_Trade_receivables_provision_Mov_1 - Impairment losses recognised On receivables
					'			Formula= P&L YTD selected accounts
								Source = $"-A#380000:F#Top{TopPOVSource}"
								Destination = $"A#512000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						Case "F_Trade_receivables_Mov_9"
							If BoolLocalCurrency
					'	   		F_Trade_receivables_Mov_2 - Amounts written off As uncollectible
					'			Formula= Current month YTD-December previous closing YTD Accounts 512100
					'			AccountFilter = "A#514000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_Trade_receivables_Mov_10"
							If BoolLocalCurrency
					'			F_Trade_receivables_provision_Mov_3 - P&L selected accounts
					'			Formula= P&L YTD selected accounts
								Source = $"-A#380100:F#Top{TopPOVSource}"
								Destination = $"A#512000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						Case "F_Trade_receivables_Mov_11"
							If BoolLocalCurrency
					'			F_Trade_receivables_provision_Mov_4 - 512000 Accounts receivable provision: difference between BS And P&L
					'			Formula= Formula :Current month YTD-December previous closing YTD-F_ Trade receivables provision Mov.1-F_ Trade receivables provision Mov.2-F_ Trade receivables provision Mov.3
								AccountFilter = "A#512000, A#512100"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id, "Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_Trade_receivables_Mov_12"
							If BoolLocalCurrency
					  	'	F_Trade_receivables_Mov_5 - 514000 Discounting revenue provision
						'	Formula= Formula :Current month YTD-December previous closing YTD Accounts 514000
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next

								AccountFilter = "A#514000"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
'						Case "F_Trade_receivables_provision_Mov_7"
'							If BoolForeignCur
'						   		'F_Trade_receivables_Mov_7 - Currency translation differences 
'								'Formula= Formula: (( Balance December previous year (accounts 512000,512100,514000)/December previous year closing FX)-
'								'( Balance December previous year (accounts 380000,380100)/current month closing FX))+(accounts 380000,380100)
'								'/December previous year closing FX)-(accounts 512000,512100,514000)/current month average FX))
'								AccountFilter = "A#512000, A#512100, A#514000"
'								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
'									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
'										ParentFLOW = "F#" & ParentMem.name
'									Continue For
'								End If
'							Next
'							Destination = $"F#{strFlowname}{TopPOVDestination}"
'							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
'							Formula = $"{Destination}=RemoveZeros({source})"
'							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'								
'							    AccountFilter = "A#380000,A#380100)"
'								Source = $"F#Top{TopPOVSource}*{CloRateLYM12}-F#Top{TopPOVSource}*{AVGRate}"
'								Destination = $"F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
'							End If
							#End Region
							
						#Region "______________Short term interest bearing______________"
						  Case "F_Short_term_interest_bearing_advances_Mov_1"
							 If BoolLocalCurrency
					'			F_Short-term interest bearing advances Mov.1 - Additions/decreases
					'			Formula= Current month YTD-December previous closing YTD
					'			AccountFilter = "A#1_2_3.Base"
								AccountFilter = "A#538200,A#538210,A#123405"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Short_term_interest_bearing_advances_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
	                      Case "F_Short_term_interest_bearing_advances_Mov_3"
							If BoolForeignCur
					'			F_Short-term interest bearing advances Mov.3 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts 538200,538210,123405)/December previous year closing FX)-
					'		( Balance December previous year (accounts 538200,538210,123405)/current month closing FX))'
							
								AccountFilter = "A#1_2_3.Base"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________Cash and cash equivalents______________"
		                 Case "F_Cash_and_cash_equivalents_Mov_1"
							 If BoolLocalCurrency
								'F_Cash and cash equivalents Mov.1 - Additions/decreases
								'Formula= Current month YTD-December previous closing YTD
						'		Formula pass 10
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id, strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								AccountFilter = "A#500000, A#502000, A#502100, A#502200, A#502300, A#502500, A#502600, A#503000, A#505000, A#506000"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Cash_and_cash_equivalents_Mov_3{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
		                 Case "F_Cash_and_cash_equivalents_Mov_2"	
							 If BoolForeignCur
					'		F_Cash And cash equivalents Mov.2 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts 500000,502000,502100,502200,502300,502500,502600,503000,505000,506000)/December previous year closing FX)-
					'		( Balance December previous year (accounts 500000,502000,502100,502200,502300,502500,502600,503000,505000,506000)/current month closing FX))'
					'			Formula pass 12
								AccountFilter = "A#500000,A#502000,A#502100,A#502200,A#502300,A#502500,A#502600,A#503000,A#505000,A#506000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________Other_current_financial_assets______________"
		                 Case "F_Other_current_financial_assets_Mov_1"
							'F_Other current  financial assets  Mov.1 - Additions/decreases
							'Formula= Current month YTD-December previous closing YTD
							'Formula pass 10
							If BoolLocalCurrency
								AccountFilter = "A#503020,A#510050,A#538300,A#570000,A#578000,A#575000,A#123519"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
					 		End If	

	                 	 Case "F_Other_current_financial_assets_Mov_2"
					'		F_Other current financial assets   Mov.2 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts  6750000)/December previous year closing FX)-
					'		( Balance December previous year (accounts  6750000)/current month closing FX))'
							If BoolForeignCur
								AccountFilter = " A#675000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
							
						#Region "______________Other_current_non_financial_assets_Mov______________"
					     Case "F_Other_current_non_financial_assets_Mov_1"
							 If BoolLocalCurrency
					'		F_Other_current_non_financial_assets_Mov_1 - Additions/decreases
					'		Formula= :Current month YTD-December previous closing YTD
							AccountFilter = "A#1_2_8_3.base,A#1_2_8_4.base,A#1_2_8_5.base,A#123520" ' "A#615000,A#533000,A#534000,A#530000,A#530100,A#531000,A#532000,A#535000,A#535100,A#536000,A#537000,A#538000,A#538100,A#530050,A#531050,A#542000,A#534001,A#540000,A#539000,A#123520"
							For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
								If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
									ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
					 		End If	
							 
	                	 Case "F_Other_current_non_financial_assets_Mov_2"
					'		F_Other_current_non_financial_assets_Mov_2 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts ,533000,534000,534001, 530000,530100,531000,532000,535000,535100,536000,537000,538000,539000)/December previous year closing FX)-
					'		( Balance December previous year (accounts ,533000,534000,534001, 530000,530100,531000,532000,535000,535100,536000,537000,538000,539000)/current month closing FX))
							If BoolForeignCur
								AccountFilter = "A#1_2_8.base"'" A#533000, A#534000, A#534001, A#530000, A#530100, A#531000, A#532000, A#535000, A#535100, A#536000, A#537000, A#538000, A#539000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If	
						#End Region
					     
						#Region "______________Other_current_non_financial_assets_Prov______________"
						Case "F_Other_current_non_financial_assets_Prov_Mov_1"
					'		F_Other_current_non_financial_assets_Prov_Mov_1 - Additions/decreases
					'		Formula= :Current month YTD-December previous closing YTD
							If BoolLocalCurrency
								AccountFilter = " A#538100"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
					 		End If	

	              	     Case "F_Other_current_non_financial_assets_Prov_Mov_2"
					'		F_Other_current_non_financial_assets_Prov_Mov_2 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts 538100)/December previous year closing FX)-( Balance December previous year (accounts 538100)/current month closing FX))
							If BoolForeignCur
								AccountFilter = " A#538100"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
					 		End If	
						#End Region	
						
						#Region "______________Provision ST LT (short term Long Term)______________"
							
			             Case "F_Provisions_ST_LT_Mov_1"
							 If BoolLocalCurrency
					'	 		F_Provisions_ST_LT_Mov_1 - Additions/decreases
					'			Formula= :Current month YTD-December previous closing YTD
								AccountFilter = "A#2_2_1_1.Base, A#2_2_2_1.Base "'"A#840000,A#840100,A#Z84010,A#840200,A#840300,A#PPACL840,A#760000,A#760100,A#760200,A#760300"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Provisions_ST_LT_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
	                     Case "F_Provisions_ST_LT_Mov_5"
					'		F_Provisions_ST_LT_Mov_5 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts PPACL840,840000,840100,Z84010,840200,840300,760000,760100,760200,760300)/December previous year closing FX)-
					'		( Balance December previous year (accounts PPACL840,840000,840100,Z84010,840200,840300,760000,760100,760200,760300)/current month closing FX)) 
							If BoolForeignCur
								AccountFilter = "A#2_2_1_1.Base, A#2_2_2_1.Base "'AccountFilter = " A#PPACL840, A#840000, A#840100, A#Z84010, A#840200, A#840300, A#760000, A#760100, A#760200, A#760300"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If	
						#End Region
							
						#Region "______________Employee_retirement_and_post_employment_benefit_obligations______________"
					     Case "F_Employee_retirement_and_post_employment_benefit_obligations_Mov_1"
							 If BoolLocalCurrency
					'			F_Employee_retirement_and_post_employment_benefit_obligations_Movements
					'			F_Employee_retirement_and_post-employment_benefit_obligations_Mov_1 - Additions/decreases
					'			Formula= :Current month YTD-December previous closing YTD
								AccountFilter = "A#2_2_1_2.Base, A#2_2_2_8_3.Base" ' "A#834000,A#834100,A#PPABP834,A#123527,A#757000"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Employee_retirement_and_post_employment_benefit_obligations_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
				    	Case "F_Employee_retirement_and_post_employment_benefit_obligations_Mov_3" 
					'		F_Employee_retirement_and_post-employment_benefit_obligations_Mov_3 - P&L selected accounts
					'		Formula= P&L YTD selected accounts
							If BoolLocalCurrency
								Source = $"-A#430600:F#Top{TopPOVSource}"
								Destination = $"A#PPABP834:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If

					    Case "F_Employee_retirement_and_post_employment_benefit_obligations_Mov_8"
					'		F_Employee_retirement_and_post-employment_benefit_obligations_Mov_8 - Currency translation differences 
					'		Formula= Formula= (( Balance December previous year (accounts 834000,834100,757000,PPABP834)/December previous year closing FX)-
					'		( Balance December previous year (accounts 834000,834100,757000,PPABP834)/current month closing FX))
							If BoolForeignCur
								AccountFilter = "A#2_2_1_2.Base, A#2_2_2_8_3.Base"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If	
				     	Case "F_Employee_retirement_and_post_employment_benefit_obligations_Mov_10"
							If BoolLocalCurrency
					'			F_Employee_retirement_and_post-employment_benefit_obligations_Mov_10 - P&L selected accounts
					'			Formula= P&L YTD selected accounts
								Source = $"-A#430500:F#Top{TopPOVSource}"
								Destination = $"A#PPABP834:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						#End Region
						
						#Region "______________Leases_ST_LT______________"
				        Case "F_Leases_ST_LT_Mov_1"
							If BoolLocalCurrency
					'			F_Leases_ST_LT_Mov_1 - Additions/decreases
					'			Formula= Current month YTD-December previous closing YTD
								
					'			AccountFilter = "A#2_2_1_4.base, A#2_2_2_4.base" '"A#811000,A#811800,A#811850,A#PPALE811,A#PPALE731,A#731000,A#731150,A#734000"
								AccountFilter = "A#2_2_1_4.base"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
	                    Case "F_Leases_ST_LT_Mov_4"
							If BoolForeignCur
					'			F_Leases_ST_LT_Mov_4 - Currency translation differences 
					'			Formula= (( Balance December previous year (accounts  811000,811800,811800,811850,PPALE811,PPALE731,731000,731150,734000)/December previous year closing FX)-
					'			( Balance December previous year (accounts 811000,811800,811800,811850,PPALE811,PPALE731,731000,731150,734000)/current month closing FX))'
					'			AccountFilter = "A#2_2_1_4.base, A#2_2_2_4.base"'"A#811000,A#811800,A#811850,A#PPALE811,A#PPALE731,A#731000,A#731150,A#734000"
								AccountFilter = "A#2_2_1_4.base"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If	
						#End Region
							
						#Region"______________Overdrafts and short term bank borrowings______________"
			            Case "F_Overdrafts_and_short_term_bank_borrowings_Mov_1"
							If BoolLocalCurrency
				 	'		F_Overdrafts_and_short-term_bank_borrowings_Mov_1 - Additions/decreases
					'		Formula= Current month YTD-December previous closing YTD
					'			AccountFilter = "A#2_2_2_5.base"'" A#700000,A#730000,A#730100,A#730200,A#738210,A#738200,A#701000,A#123408"
								AccountFilter = "A#701000,A#702000,A#123543"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Overdrafts_and_short_term_bank_borrowings_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
		                Case "F_Overdrafts_and_short_term_bank_borrowings_Mov_4"
							If BoolLocalCurrency
					'			F_Overdrafts_and_short-term_bank_borrowings_Mov_4 - P&L selected accounts
					'			Formula= P&L YTD selected accounts

								Source = $"-A#433000:F#Top{TopPOVSource}"
								Destination = $"A#700000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
 	                    Case "F_Overdrafts_and_short_term_bank_borrowings_Mov_5"
							If BoolForeignCur
					'			F_Overdrafts_and_short-term_bank_borrowings_Mov_5 - Currency translation differences 
					'			Formula= Formula= (( Balance December previous year (accounts 700000,730000,730100,730200,738210,738200,701000,123408)/December previous year closing FX)-
					'			( Balance December previous year (accounts 700000,730000,730100,730200,738210,738200,701000,123408)/current month closing FX))
							
								AccountFilter = "A#2_2_2_5.Base"'" A#700000,A#730000,A#730100,A#730200,A#738210,A#738200,A#701000,A#123408"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If	
							
							#End Region
							
						#Region"______________Other non current non financial liabilities______________"
			    	   Case "F_Other_non_current_non_financial_liabilities_Mov_1"
						   If BoolLocalCurrency
						'		F_Other_non-current_non_financial_liabilities_Mov_1 - Additions/decreases
						'		Formula= Current month YTD-December previous closing YTD							
						'		AccountFilter = "A#2_2_1_7.Base"' "A#831000,A#832000,A#833800,A#836000,A#836100,A#837000,A#838000,A#860000"
								AccountFilter = "A#831000,A#832000,A#833800,A#836000,A#836100,A#837000,A#Z83610,A#123470"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Other_non_current_non_financial_liabilities_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
				        Case "F_Other_non_current_non_financial_liabilities_Mov_2"
					'		F_Other_non-current_non_financial_liabilities_Mov_2 - Interest expense On shareholder loans
					'		Formula= P&L selected accounts
								Source = $"-A#430200:F#Top{TopPOVSource}"
								Destination = $"A#839000:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)

					    Case "F_Other_non_current_non_financial_liabilities_Mov_3"
					'		F_Other_non-current_non_financial_liabilities_Mov_3 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts 831000,832000,833800,836000,836100,837000,838000,860000)/December previous year closing FX)-
					'		( Balance December previous year (accounts 831000,832000,833800,836000,836100,837000,838000,860000)/current month closing FX))
							If BoolForeignCur
								AccountFilter =  "A#2_2_1_7.Base"'" A#831000, A#832000, A#833800, A#836000, A#836100, A#837000, A#838000, A#860000"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Other_non_current_non_financial_liabilities_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If	
						#End Region
							
						#Region "______________Trade Payables______________"
						Case "F_Trade_payables_Mov_1"
							If BoolLocalCurrency
					'			F_Trade_payables_Mov_1 - Additions/decreases
					'			Formula= Current month YTD-December previous closing YTD
								AccountFilter = "A#710000,A#712000,A#712100,A#713000,A#720000,A#721000,A#722000,A#758200,A#Z71000,A#710100,A#710500,A#711000,A#711100,A#711200,A#714200,A#714000,A#733150,A#733100,A#123407"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Trade_payables_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
	                    Case "F_Trade_payables_Mov_3"
							If BoolForeignCur
					'			F_Trade_payables_Mov_3 - Currency translation differences 
					'			Formula= Formula= (( Balance December previous year (accounts 71ROUN, 710000,712000,712100,713000,720000,721000,722000,758200,Z71000,733150,733100,123407)/December previous year closing FX)-
					'			( Balance December previous year (accounts 71ROUN, 710000,712000,712100,713000,720000,721000,722000,758200,Z71000,733150,733100,123407)/current month closing FX))
								AccountFilter = " A#71ROUN, A#710000, A#712000, A#712100, A#713000, A#720000, A#721000, A#722000, A#758200, A#Z71000, A#733150, A#733100, A#123407"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
'						Case "F_Trade_payables_Mov_4"
'							'F_Trade_payables_Mov_4 - FX
'							'Formula= P&L selected accounts
'							'A#420000 ,A#Z42400 ,A#421000 ,A#471500

'								AccountFilter = "A#420000, A#Z42400, A#3421000" 
'								Source = $"F#Top{TopPOVSource}"
'								Destination = $"F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)

'						Case "F_Trade_payables_Mov_5"
'							'F_Trade_payables_Mov_5 - Other one-off items without cash impact
'							'Formula= P&L selected accounts
'								AccountFilter = "A#471500" 
'								Source = $"F#Top{TopPOVSource}"
'								Destination = $"F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
						#End Region

						#Region "______________Trade Payables Accruals______________"
					  	Case "F_Trade_payables_Accruals_Mov_1"
					'		F_Trade_payables_Accruals_Mov_1 - Additions/decreases
					'		Formula= Current month YTD-December previous closing YTD
							If BoolLocalCurrency
								AccountFilter = "A#750000,A#750200,A#750300,A#750700,A#750710"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
	                    Case "F_Trade_payables_Accruals_Mov_3"
				'			F_Trade_payables_Accruals_Mov_3 - Currency translation differences 
				'			Formula= (( Balance December previous year (accounts  750000,750200,750300,750700,750710)/December previous year closing FX)-
				'			( Balance December previous year (accounts 750000,750200,750300,750700,750710)/current month closing FX))'
							If BoolForeignCur
								AccountFilter = " A#750000,A#750200,A#750300,A#750700,A#750710"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________Other Curr non Financial Liabilities Accruals______________"
					  	Case "F_Other_current_non_financial_liabilities_accruals_Mov_1"
			'				F_Other_current_non_financial_liabilities_accruals_Mov_1 - Additions/decreases
			'				Formula= Current month YTD-December previous closing YTD
							If BoolLocalCurrency
			'					AccountFilter = "A#756000, A#757100, A#757200, A#Z75000, A#Z99999"
								AccountFilter = "A#Z75000"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
	                    Case "F_Other_current_non_financial_liabilities_accruals_Mov_2"
							If BoolForeignCur
			'					F_Other_current_non_financial_liabilities_accruals_Mov_2 - Currency translation differences 
								AccountFilter = "A#756000, A#757100, A#757200, A#Z75000, A#Z99999"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
									Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
					#End Region
				
						#Region "______________CIT_receivable_payable______________"
		             Case "F_CIT_receivable_payable_Mov_1"
						If BoolLocalCurrency
				'			F_CIT_receivable_payable_Mov_1 - Additions/decreases
				'			Formula= Current month YTD-December previous closing YTD
				'			Formula Pass 9
							AccountFilter = "A#2_2_2_3.Base,A#1_2_8_2.Base,A#860000,A#680000" '"A#550000,A#550010,A#770000,A#770007,A#754020,A#754030,A#754040,A#123539,A#860000,A#680000"
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_CIT_receivable_payable_Movements{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
						End If 
		           	Case "F_CIT_receivable_payable_Mov_6"
						If BoolForeignCur
				'			F_CIT_receivable_payable_Mov_6 - Currency translation differences 
				'			Formula= (( Balance December previous year (accounts  550000/770000)/December previous year closing FX)-
				'			( Balance December previous year (accounts 550000/770000)/current month closing FX))'
				'			Formula Pass 12
							AccountFilter = " A#550000, A#770000"
							For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
								If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
									ParentFLOW = "F#" & ParentMem.name
								Continue For
								End If
							Next
							Destination = $"F#{strFlowname}{TopPOVDestination}"
							Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula, accountFilter:=AccountFilter)
						End If
		              Case "F_CIT_receivable_payable_Mov_7"
						  If BoolLocalCurrency
				'			F_CIT_receivable_payable_Mov_7 - 480000 CIT &; 480200 Withholding tax:CIT receivable&payableexpense: difference between BS And P&L
				'			Formula= Formula :Current month YTD-December previous closing YTD-F_CIT receivable&payableMov.1-F_CIT receivable&payableMov.2-F_CIT receivable&payableMov.3
				'			Formula Pass 9
							Destination = $"A#770000:F#{strFlowname}{TopPOVDestination}"
							Source = $"-A#480000:F#Top{TopPOVSource}-A#480200:F#Top{TopPOVSource}"
							Formula = $"{Destination}=RemoveZeros({source})"
							api.Data.Calculate(Formula)
						End If
						#End Region
						
						#Region "______________ACCRUED INTEREST______________"
	                   	Case "F_Accrued_interest_Mov_1" 
							If BoolLocalCurrency
			   	'				F_Accrued_interest_Mov_1 - Payments
				'				Formula= Formula :Current month YTD-December previous closing YTD -F_ Accrued interest  Mov.2
				'				Formula Pass 10
								AccountFilter = "A#750500"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Accrued_interest_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
		            	Case "F_Accrued_interest_Mov_2" 
							If BoolLocalCurrency
				'				F_Accrued_interest_Mov_2 - P&L selected accounts
				'				Formula= P&L YTD selected accounts
				'				Formula Pass 9
				'				AccountFilter = "A#430000"
								Source = $"-A#430000:F#Top{TopPOVSource}"
								Destination = $"A#750500:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)', accountFilter:=AccountFilter)
							End If 
		            	Case "F_Accrued_interest_Mov_4" 
							If BoolForeignCur
				'				F_Accrued_interest_Mov_4 - Currency translation differences 
				'				Formula= (( Balance December previous year (accounts  750500)/December previous year closing FX)-
				'				( Balance December previous year (accounts 750500)/current month closing FX))'
				'				Formula Pass 12
								AccountFilter = "A#750500"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
										If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
											ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________ACCRUED INTEREST LEASES______________"
		            	Case "F_Accrued_interest_Leases_Mov_1"
							If BoolLocalCurrency
							'	F_Accrued_interest_Leases_Mov_1M - Payments
							'	Formula= Formula: Current month YTD-December previous closing YTD -F_ Accrued interest Leases Mov.2
							'	Formula Pass 10
								AccountFilter = "A#750600"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Accrued_interest_Leases_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
		            	Case "F_Accrued_interest_Leases_Mov_2"
							If BoolLocalCurrency
					'			F_Accrued_interest_Leases_Mov_2 - Interest expenses On leases
					'			Formula= P&L YTD selected accounts
					'			Formula Pass 9
								Source = $"-A#430100:F#Top{TopPOVSource}-A#430150:F#Top{TopPOVSource}"
								Destination = $"A#750600:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If 

		               	Case "F_Accrued_interest_Leases_Mov_4"
							If BoolForeignCur
					'		F_Accrued_interest_Leases_Mov_4 - Currency translation differences 
					'		Formula= (( Balance December previous year (accounts  750500)/December previous year closing FX)-
					'		( Balance December previous year (accounts 750500)/current month closing FX))'
								AccountFilter = "A#750600"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
										If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
											ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________Other_current_non_financial_liabilities______________"
						Case "F_Other_current_non_financial_liabilities_Mov_1"
					'			F_Other_current_non_financial_liabilities_Mov_1 - Additions/decreases
					'			Formula= Current month YTD-December previous closing YTD
								If BoolLocalCurrency
									AccountFilter = "A#754000,A#752000,A#752001,A#753000,A#751000,A#732000,A#755000,A#758100,A#758300,A#759100,A#759200,A#759300,A#759400,A#780000,A#781000,A#782000,A#781100,A#Z99999,A#123546"
									For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
										If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
											ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
		                 Case "F_Other_current_non_financial_liabilities_Mov_2"
					'			F_Other_current_non_financial_liabilities_Mov_2 - Currency translation differences 
					'			Formula= (( Balance December previous year (accounts 754000,752000,752001, 753000,757000,751000,732000,755000,758100,758300,759100,759200,759300,759400,780000,781000,782000,Z99999)/December previous year closing FX)-
					'			( Balance December previous year (accounts 754000,752000,752001, 753000,757000,751000,732000,755000,758100,758300,759100,759200,759300,759400,780000,781000,782000,Z99999)/current month closing FX))'
								If BoolForeignCur
									AccountFilter = " A#754000, A#752000, A#752001, A#753000, A#757000, A#751000, A#732000, A#755000, A#758100,A#758300,A#759100,A#759200,A#759300,A#759400,A#780000,A#781000,A#782000,A#Z99999"
									For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
										If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
											ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
						#End Region
						
						#Region "______________Current non financial STIP and LTIP______________"
								
						Case "F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_1"
							If BoolLocalCurrency
				'				F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_1 - Additions/decreases
				'				F#F_F_Other_current_non_financial_liabilities_STIP_LTIP_Movements
				'				Formula Pass 10
								AccountFilter = "A#756100, A#756200, A#756300"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_F_Other_current_non_financial_liabilities_STIP_LTIP_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_2"
							If BoolLocalCurrency
				'				F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_2 - P&L Accrual For this year
				'				Formula= P&L YTD selected accounts	
				'				Formula Pass 9
								Source = $"-A#470600:F#Top{TopPOVSource}"
								Destination = $"A#756300:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
								
								Source = $"-A#306100:F#Top{TopPOVSource}-A#470610:F#Top{TopPOVSource}"
								Destination = $"A#756200:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If 
						Case "F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_3"
							If BoolLocalCurrency
				'				F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_3 - P&L Accrual prior year
				'				Formula= P&L YTD selected accounts
				'				Formula Pass 9
								Source = $"-A#306000:F#Top{TopPOVSource}"
								Destination = $"A#756100:F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If 
						Case "F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_4"
							If BoolForeignCur
				'				F_Other_current_non_financial_liabilities_STIP_LTIP_Mov_4 - Currency translation differences 
				'				Formula Pass 12
					
								AccountFilter = "A#756100, A#756200, A#756300"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
										If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
											ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
							
					 	#End Region
						
						#Region "______________Deferred_consideration_ST_LT_Loand_PRP______________"
			                Case "F_Deferred_consideration_ST_LT_Loand_PRP_Mov_1"
								If BoolLocalCurrency
				'					F_Deferred_consideration_ST_LT_Loand_PRP_Mov_1 - Additions
				'					Formula= Formula :Current month YTD-December previous closing YTD-F_Deferred consideration ST&LT&Loand PRP    Mov.2
									AccountFilter = "A#830000, A#820000, A#821100, A#822000, A#740000, A#741100, A#742200, A#870000, A#742300"
									Destination = $"F#{strFlowname}{TopPOVDestination}"
									Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Deferred_consideration_ST_LT_Loand_PRP_Movements{TopPOVSource}+{Destination}"
									Formula = $"{Destination}=RemoveZeros({source})"
									api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If 
	                        Case "F_Deferred_consideration_ST_LT_Loand_PRP_Mov_3"
								If BoolLocalCurrency
				'					F_Deferred_consideration_ST_LT_Loand_PRP_Mov_3 -P&L  interest
				'					Formula= P&L YTD selected accounts
									Source = $"-A#430400:F#Top{TopPOVSource}"
									Destination = $"A#742300:F#{strFlowname}{TopPOVDestination}"
									Formula = $"{Destination}=RemoveZeros({source})"
									api.Data.Calculate(Formula)
								End If
		                	Case "F_Deferred_consideration_ST_LT_Loand_PRP_Mov_4"
								If BoolLocalCurrency
				'					F_Deferred_consideration_ST_LT_Loand_PRP_Mov_4 - P&L Remeasurement
				'					Formula= P&L YTD selected accounts
									Source = $"-A#440000:F#Top{TopPOVSource}"
									Destination = $"A#742300:F#{strFlowname}{TopPOVDestination}"
									Formula = $"{Destination}=RemoveZeros({source})"
									api.Data.Calculate(Formula)
								End If 
			                Case "F_Deferred_consideration_ST_LT_Loand_PRP_Mov_5"
				'				F_Deferred_consideration_ST_LT_Loand_PRP_Mov_4 - Currency translation differences 
				'				Formula= (( Balance December previous year (accounts 830000,820000,821100,822000,740000,741100,742200)/December previous year closing FX)-
				'				( Balance December previous year (accounts 830000,820000,821100,822000,740000,741100,742200)/current month closing FX))'
								If BoolForeignCur
									AccountFilter = "A#830000, A#820000, A#821100, A#822000, A#740000, A#741100, A#742200, A#870000, A#742300"
									For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
										If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
											ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								End If
							#End Region
							
						#Region "______________Other_non_current_financial_assets_Deposits_and_guarantees______________"
	                    Case "F_Other_non_current_financial_assets_Deposits_and_guarantees_Mov_1"
							If BoolLocalCurrency
				'				F_Other_non-current_financial_assets:_Deposits_and_guarantees_Mov_1 - Other Long-term advances
				'				Formula= Current month YTD-December previous closing YTD
								AccountFilter = "A#670000, A#670100"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id, strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
		                Case "F_Other_non_current_financial_assets_Deposits_and_guarantees_Mov_4"
							If BoolForeignCur
			 	'				F_Other_non-current_financial_assets:_Deposits_and_guarantees_Mov_4 - Currency translation differences 
				'				Formula= (( Balance December previous year (accounts 670000,670100)/December previous year closing FX)-
				'				( Balance December previous year (accounts 670000,670100)/current month closing FX))'670100 
								AccountFilter = "A#670000, A#670100"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________EQUITY_____________"
	                    Case "F_Capital_Mov_1"
				'			#F_Capital_Mov_1 - Capital movements
				'			Formula= Current month YTD-December previous closing YTD
							AccountFilter = "A#2_1_1_1.Base.Where(Text3 Contains CTA),A#123471" 
							For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
								If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
									ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							If BoolLocalCurrency Or BoolForeignCur
				'				Formula= Current month YTD-December previous closing YTD
								Destination = $"F#{strFlowname}{TopPOVDestination.Replace("I#None","")}"
								Source = $"F#F_CLO{TopPOVSource.Replace("I#Top","")}-F#F_OPE{TopPOVSource.Replace("I#Top","")}-{ParentFLOW}{TopPOVSource.Replace("I#Top","")}+{Destination.Replace("I#None","")}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
		                Case "F_Share_Premium_Mov_1"
				'			#F_Share_Premium_Mov_1 - Share Premium increase/reimbursement
				'			Formula= Current month YTD-December previous closing YTD
							AccountFilter = "A#2_1_1_2.Base.Where(Text3 Contains CTA),A#123472,A#920000"
							For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
								If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
									ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							If BoolLocalCurrency Or BoolForeignCur
				'				#F_Capital_Mov_1 - Capital increase/reimbursement
				'				Formula= Current month YTD-December previous closing YTD
								Destination = $"F#{strFlowname}{TopPOVDestination.Replace("I#None","")}"
								Source = $"F#F_CLO{TopPOVSource.Replace("I#Top","")}-F#F_OPE{TopPOVSource.Replace("I#Top","")}-{ParentFLOW}{TopPOVSource.Replace("I#Top","")}+{Destination.Replace("I#None","")}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						#End Region
							
						#Region "______________Minority interest______________"
	                  	Case "F_Minority_interest_Mov_1"
				'			F_Minority_interest_Mov_1 - Changes In minority interests
				'			Formula= Current month YTD-December previous closing YTD
							If BoolLocalCurrency
								AccountFilter = "A#940000,A#123524"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
                         Case "F_Minority_interest_Mov_2"
				'			F_Minority_interest_Mov_2 - Result In minority interests
				'			Formula= P&L selected accounts
							If BoolLocalCurrency
								AccountFilter = "A#940001"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
		                Case "F_Minority_interest_Mov_4"
				'			F_Minority_interest_Mov_4 -Translation differences minority interests
				'			Formula= Current month YTD-December previous closing YTD
							If BoolForeignCur
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
									If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
										ParentFLOW = "F#" & ParentMem.name
										Continue For
									End If
								Next
								Destination = $"A#940002:F#{strFlowname}{TopPOVDestination}"
								Source = $"A#2_1_2:F#F_CLO{TopPOVSource}-A#2_1_2:F#F_OPE{TopPOVSource}-A#2_1_2:{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
							End If
						#End Region
		               
						#Region "______________Translation differences______________"
						Case "F_Translation_differences_Mov_1"
'							'F_Translation_differences_Mov_1 - Changes in Translation differencess
'							'Formula= Current month YTD-December previous closing YTD

							AccountFilter = "A#2_1_1_6.Base"
							For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
								If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
									ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							If BoolLocalCurrency
					'			#F_Capital_Mov_1 - Capital increase/reimbursement
					'			Formula= Current month YTD-December previous closing YTD
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							ElseIf BoolForeignCur
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						#End Region
						
						#Region "______________Retained Earnings______________"

		                Case "F_Retained_Earnings_Mov_1" 
'			'				F_Retained_Earnings_Mov_1 - Result Of the year
'			'				Formula= Current month YTD-December previous closing YTD
'							If BoolLocalCurrency Or api.Pov.Cons.Name.XFEqualsIgnoreCase("Elimination")
							If BoolLocalCurrency
'								Source = $"A#8:F#TOP{TopPOVSource}"
								If  api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual") And (api.Time.GetPeriodNumFromId > 10 And api.Time.GetYearFromId = 2024)
'									If api.Pov.Entity.Name.XFEqualsIgnoreCase("HU10")
'										Source = $"A#4:F#Top{TopPOVSource}"
'										Destination = $"A#970000:F#{strFlowname}{TopPOVDestination}"
'	'									Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250320 as there are 0s needed in account 970000
'										Formula = $"{Destination}={source}"
'										api.Data.Calculate(Formula)
'									Else 
										Source = $"A#4:F#Top{TopPOVSource.Replace("V#YTD","V#Periodic")}"
										Destination = $"A#970000:F#{strFlowname}{TopPOVDestination.Replace("V#YTD","V#Periodic")}"
'										Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250320 as there are 0s needed in account 970000
										Formula = $"{Destination}={source}"
										api.Data.Calculate(Formula)
'									End If
									Source = $"A#970000:F#{strFlowname}{TopPOVDestination}"
									Destination = $"A#970000:F#F_CLO{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250320 as there are 0s needed in account 970000
									Formula = $"{Destination}={source}"
									api.Data.Calculate(Formula)
									
									
								ElseIf api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual") And api.Pov.Time.Name.XFEqualsIgnoreCase("2024M10")
'									If api.Entity.GetLocalCurrency.Name("EUR")
'									If api.Entity.GetLocalCurrencyId.Equals("EUR")
										Source = $"A#4:F#Top_LC{TopPOVSource}:T#2024M9 +
										A#4:F#Top{TopPOVSource.Replace("V#YTD","V#Periodic")}"
'									Else
'										Source = $"A#4:F#Top_LC{TopPOVSource}"
'									End If 
									Destination = $"A#970000:F#{strFlowname}{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250320 as there are 0s needed in account 970000
									Formula = $"{Destination}={source}"
									api.Data.Calculate(Formula)
									
									Source = Destination 
									Destination = $"A#970000:F#F_CLO{TopPOVDestination}"
									Formula = $"{Destination}={source}"
									api.Data.Calculate(Formula)
								
								Else
									Source = $"A#4:F#TOP{TopPOVSource}"
									Destination = $"A#970000:F#{strFlowname}{TopPOVDestination}"
									Formula = $"{Destination}=RemoveNoData({source})"
									api.Data.Calculate(Formula)
									
									Source = Destination 
									Destination = $"A#970000:F#F_CLO{TopPOVDestination}"
									Formula = $"{Destination}={source}"
									api.Data.Calculate(Formula)
								End If
							End If

							If api.Cons.IsForeignCurrencyForEntity And api.Time.GetYearFromId > 2024
								Source = $"V#YTD:A#970000:F#{strFlowname}" 
								Destination = $"V#YTD:A#970000:F#F_CLO"
								Formula = $"{Destination}={source}"
								api.Data.Calculate(Formula)
							End If 
'							If BoolLocalCurrency Or api.Pov.Cons.Name.XFEqualsIgnoreCase("Elimination")
							If api.Pov.Cons.Name.XFEqualsIgnoreCase("Elimination")
'								Source = $"A#8:F#TOP{TopPOVSource}"
								If  api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual") And (api.Time.GetPeriodNumFromId > 10 And api.Time.GetYearFromId = 2024)
									Source = $"A#4:F#Top{TopPOVSource.Replace("V#YTD","V#Periodic")}"
									Destination = $"A#970000:F#{strFlowname}{TopPOVDestination.Replace("V#YTD","V#Periodic")}"
'									Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250320 as there are 0s needed in account 970000
									Formula = $"{Destination}={source}"
									api.Data.Calculate(Formula)
									
									Source = $"A#970000:F#{strFlowname}{TopPOVDestination}"
									Destination = $"A#970000:F#F_CLO{TopPOVDestination}"
'									Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250320 as there are 0s needed in account 970000
									Formula = $"{Destination}={source}"
'									api.Data.Calculate(Formula) 'Commented out by DAGO on 20250425 as the amount was replaced for NCI, previously calculated correctly
									api.Data.Calculate(Formula, UD8Filter:="UD8#Top.Base.Remove(NCI_Elim)")
									
									
								ElseIf api.Pov.Scenario.Name.XFEqualsIgnoreCase("Actual") And api.Pov.Time.Name.XFEqualsIgnoreCase("2024M10")
'									If api.Entity.GetLocalCurrency.Name("EUR")
''									If api.Entity.GetLocalCurrencyId.Equals("EUR")
'										Source = $"A#4:F#Top_LC{TopPOVSource}:T#2024M9 +
'										A#4:F#Top{TopPOVSource.Replace("V#YTD","V#Periodic")}"
'									Else
										Source = $"A#4:F#Top_LC{TopPOVSource}"
'									End If 
									Destination = $"A#970000:F#{strFlowname}{TopPOVDestination}"
''									Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250320 as there are 0s needed in account 970000
									Formula = $"{Destination}={source}"
									api.Data.Calculate(Formula)

									Source = Destination 
									Destination = $"A#970000:F#F_CLO{TopPOVDestination}"
									Formula = $"{Destination}={source}"
'									api.Data.Calculate(Formula) 'Commented out by DAGO on 20250425 as the amount was replaced for NCI, previously calculated correctly
									api.Data.Calculate(Formula, UD8Filter:="UD8#Top.Base.Remove(NCI_Elim)")
								
								Else
									Source = $"V#YTD:A#4:F#TOP{TopPOVSource}"
									Destination = $"V#YTD:A#970000:F#{strFlowname}{TopPOVDestination}"
									Formula = $"{Destination}=RemoveZeros({source})"
									api.Data.Calculate(Formula)
									
									Source = Destination 
									Destination = $"V#YTD:A#970000:F#F_CLO{TopPOVDestination}"
									Formula = $"{Destination}={source}"
									api.Data.Calculate(Formula)
								End If 

								
'								Source = $"A#8:F#TOP{TopPOVSource}"
'								Source = Destination 
''								$"A#4:F#TOP{TopPOVSource}"
'								Destination = $"A#970000:F#F_CLO{TopPOVDestination}"
''								Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250304 as there are 0s needed in account 970000
'								Formula = $"{Destination}={source}"
'								api.Data.Calculate(Formula)
								
'								Source = $"A#8:F#Top_LC{TopPOVSource}"
								Source = $"A#4:F#Top_LC{TopPOVSource}"
								Destination = $"A#970000:F#F_GL_Load_LC{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)
								
'								Source = $"A#8:F#TOP{TopPOVSource}"
								Source = $"A#4:F#TOP{TopPOVSource}"
								Destination = $"A#970000:F#F_GL_Load_BS{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250304 as there are 0s needed in account 970000
								Formula = $"{Destination}={source}"
								api.Data.Calculate(Formula)
'								If api.Pov.Time.Name.XFEqualsIgnoreCase("2041M2")
'									Source = $"F#F_GL_Load"
'									Destination = $"F#F_GL_Load"
'									Formula = $"{Destination}=RemoveZeros({source})"
'									api.Data.Calculate(Formula)
'								End If

'							ElseIf OwnerPostAdj 'Included by David on 20240610
End If
'							ElseIf (OwnerPreAdj Or OwnerPostAdj) And (BaseEntity Or ParentEntity) 'Modified by DAGO on 20250407
							If (OwnerPreAdj Or OwnerPostAdj) And (BaseEntity Or ParentEntity) 'Modified by DAGO on 20250407
'								Source = $"A#8:F#None{TopPOVSource}"
								Source = $"A#4:F#None{TopPOVSource}"
								Destination = $"A#970000:F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250321 as there are 0s needed in account 970000
								Formula = $"{Destination}={source}"
								api.Data.Calculate(Formula)
'Brapi.ErrorLog.LogMessage(si, "Row 3062 - " & "source " & api.data.getDataCell("T#2024M12:E#SubCons_NL10:P#SubCons_NL20:C#OwnerPreAdj:A#4:F#None:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Consolidation_Adj").Cellamountastext)								

'								Source = $"A#8:F#None{TopPOVSource}"
								Source = $"A#4:F#None{TopPOVSource}"
								Destination = $"A#970000:F#F_CLO{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})" 'Commented out by DAGO on 20250321 as there are 0s needed in account 970000
								Formula = $"{Destination}={source}"
								api.Data.Calculate(Formula)
'Brapi.ErrorLog.LogMessage(si, "Row 3070 - " & "source " & api.data.getDataCell("T#2024M12:E#SubCons_NL10:P#SubCons_NL20:C#OwnerPreAdj:A#4:F#None:V#YTD:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Consolidation_Adj").Cellamountastext)								
						
'							ElseIf Elimination 'Included by David on 20241230
''								Source = $"A#8:F#None{TopPOVSource}"
'								Source = $"A#4:F#None{TopPOVSource}"
'								Destination = $"A#970000:F#{strFlowname}{TopPOVDestination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula)
								
							End If
							
							Case "F_Retained_Earnings_Mov_2"
'	 						F_Retained_Earnings_Mov_2 - Others
'							Formula= Current month YTD-December previous closing YTD
							AccountFilter = "A#2_1_1_7.Base"
								For Each ParentMem As Member In api.Members.GetParents(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,strFlowname),False)
								If api.Members.IsDescendant(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id,"Top_Constraints"), ParentMem.MemberId)
									ParentFLOW = "F#" & ParentMem.name
									Continue For
								End If
							Next
							If BoolLocalCurrency
					'			#F_Capital_Mov_1 - Capital increase/reimbursement
					'			Formula= Current month YTD-December previous closing YTD
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							ElseIf BoolForeignCur
'								AccountFilter = "A#2_1_1_7.Base.Where(Text3 Contains CTA)"
'								Destination = $"F#F_Clo{TopPOVDestination}"
'								Source = $"F#F_OPE{TopPOVSource}+{ParentFLOW}{TopPOVSource}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								
'								AccountFilter = "A#2_1_1_7.Base.Where(Text3 DoesNotContain CTA)"
'								Destination = $"F#{strFlowname}{TopPOVDestination}"
'								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-{ParentFLOW}{TopPOVSource}+{Destination}"
'								Formula = $"{Destination}=RemoveZeros({source})"
'								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						#End Region
						
						#Region "______________Actuarial_valuation______________"
	                    Case "F_Actuarial_valuation_Mov_1"
					'		F_Actuarial_valuation_Movements
					'		F_Actuarial_valuation_Mov_1 -Actuarial valuation Net Of taxes
					'		Formula= Current month YTD-December previous closing YTD
					'		Comment BH: How To check accounts?'
							If BoolLocalCurrency
								AccountFilter = "A#PPABP834"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Actuarial_valuation_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_Actuarial_valuation_Mov_2"
							If BoolForeignCur
								AccountFilter = "A#PPABP834"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#F_CLO{TopPOVSource}-F#F_OPE{TopPOVSource}-F#F_Actuarial_valuation_Movements{TopPOVSource}+{Destination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						#End Region
						
						#Region "______________CFManagerial_General______________"
	                    Case "F_CFManagement_Mov_0"
							If BoolLocalCurrency
								AccountFilter = "A#410000,A#410100,A#460000,A#461000,A#470000,A#470050,A#470090,A#470100,A#470200,A#470300,A#470400,A#470500,A#470550,A#470650,A#470700,A#470800,A#471100,A#471200,A#471300,A#471400,A#472000,A#Z47090"
								Source = -$"F#Top{TopPOVSource}"'-F#F_OPE{TopPOVSource}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_CFManagement_Mov_1"
							If BoolLocalCurrency
								AccountFilter = "A#470000,A#470050,A#470090,A#470100,A#470200,A#470300,A#470500,A#470550,A#470800,A#471100,A#471200,A#471300,A#471400,A#472000,A#Z47090"
								Source = $"F#Top{TopPOVSource}"'-F#F_OPE{TopPOVSource}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						Case "F_CFManagement_Mov_2"
							If BoolLocalCurrency
								AccountFilter = "A#460000,A#461000,A#470400,A#470650,A#470700"
								Source = $"F#Top{TopPOVSource}"'-F#F_OPE{TopPOVSource}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If
						Case "F_CFManagement_Mov_3"
							If BoolLocalCurrency
								AccountFilter = "A#431100"
								Source = $"F#Top{TopPOVSource}"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
							End If 
						#End Region
						
						#Region "______________Result______________"
						Case "F_Profit_from_continued_operations_before_income_taxesand_minority"
							If BoolLocalCurrency
								AccountFilter = "A#6_1_5_2_2_1_1.Base"
								Destination = $"F#{strFlowname}{TopPOVDestination}"
								Source = $"F#Top{TopPOVSource}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula, accountFilter:=AccountFilter)
								
'								AccountFilter = "A#7_1_1_1_1_1_1_1.Base"
								Destination = $"A#7_1_1_1_1_1_1_1_1:F#{strFlowname}{TopPOVDestination}"
								Source = $"A#8_1_1_1_1:F#Top{TopPOVSource}"
								Formula = $"{Destination}=RemoveZeros({source})"
								api.Data.Calculate(Formula)', accountFilter:=AccountFilter)
							End If
						#End Region
						
						End Select
					End If 
				End If 
			Return Nothing
			Catch ex As Exception
						Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
					End Try
				End Function
	End Class
End Namespace