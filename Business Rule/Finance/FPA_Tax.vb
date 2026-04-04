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

Namespace OneStream.BusinessRule.Finance.FPA_Tax
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType
						
					Case Is = FinanceFunctionType.CustomCalculate
						
						
			'Set variables-----------------------------------------------------------
			Dim sSource As String = ""
			Dim sSource2 As String = ""
			Dim sSource3 As String = ""
			Dim sSource4 As String = ""
			Dim sPOVSource As String = ""
			Dim sPOVDestination As String = ""
			Dim sDestination As String = ""
			Dim sDestination2 As String = ""
			Dim sCountry As String = ""
			Dim TaxRate As String = ""
			Dim sWorkflowName As String = BRApi.Workflow.Metadata.GetProfile(si,si.WorkflowClusterPk.ProfileKey).Name
			
			If sWorkflowName.Contains("_") Then
				sCountry = sWorkflowName.Split("_")(0)
					Else
				sCountry = ""
			End If
			
			If scountry = "Czech" Then
				scountry= "Czech_Republic"
			Else If scountry  = "Northern" Then
				scountry= "Northern_Ireland"
			End If
				

#Region "Tax calculations"

		'Calculate for all UD1 members within the country
		Dim UD1MemberList = $"U1#{scountry}.base"
		Dim UD1PK As String = $"UD1_200_{scountry}"
		
		If scountry = "Ireland" Then
			UD1MemberList = "U1#Republic_of_Ireland.base"
		Else If scountry = "Northern_Ireland" Then
			UD1PK= "UD1_200_Ireland"
			UD1MemberList = "U1#Northern_Ireland.base"
		Else If scountry = "UK" Then
			UD1MemberList = "U1#United_Kingdom.base"
		End If
		
		Dim UD1DimPK As DimPk = BRApi.Finance.Dim.GetDimPk(si, UD1PK)			
		Dim UD1List As List(Of MemberInfo) = BRApi.Finance.Members.GetMembersUsingFilter(si, UD1DimPK, UD1MemberList, True)
		Dim FilteredUD1List As String = ""

		For Each UD1Member In UD1List
				
			Dim UD1Membername As String = UD1Member.Member.Name
			Dim UD1MemberID As Integer = BRApi.Finance.Members.GetMemberId(si,dimtype.UD1.Id,UD1Membername)
'			Dim ScenarioId As Integer = BRApi.Workflow.General.GetScenarioTypeId(si,si.WorkflowClusterPk)
			Dim ScenarioId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, BRApi.Finance.Members.GetMemberId(si,dimtype.Scenario.Id,"Budgetv1")).id
			Dim UD1Text2_Budget As String = BRApi.Finance.UD.Text(si, dimtype.UD1.id,UD1MemberID,2,ScenarioId,si.WorkflowClusterPk.TimeKey)
			
			If UD1Text2_Budget.Contains("LE_") Then				
				
				Dim UD1Source As String = api.UD1.Text(UD1MemberID,2,ScenarioId,si.WorkflowClusterPk.TimeKey)
				Dim UD1Target As String = UD1Member.Member.Name

#Region "Tax calcs"

'Tax calculations
'------------------------------------------------------	

	If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Tax") Then

		'Calculate Earnings Before Tax (EBT)
		'------------------------------------------------------
		
		'Define POV variables
		'BERO 26jun24 updated to keep intercompany detail for EBT line
		sPOVSource = ":V#YTD:O#Top:F#Top:U1#" & UD1Source & ":U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
		sPOVDestination = ":V#Periodic:O#Import:F#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
		
		'Define source
		sSource = "-A#8_1_1" & sPOVSource & " - A#8_1_2_1_1_1_1_1" & sPOVSource & " - A#8_1_2_1_1_2_1_1" & sPOVSource & " - A#PPADT480" & sPOVSource & " - A#Z48010" & sPOVSource & " - A#8_1_1_1_2_1_1_3" & sPOVSource & " - A#8_1_2_1_1_3_1_2" & sPOVSource & " - A#8_1_2_1_1_3_1_3" & sPOVSource	
		
		'Define destination
		sDestination = "A#EBT_Tax" & sPOVDestination
		
		'Clear previously calculated data
		api.Data.ClearCalculatedData(sDestination, clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
		'Execute calc
		api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
		
		
		
		'Calculate Contract value depreciation
		'------------------------------------------------------
			
		'Define POV variables
		sPOVSource = ":V#YTD:O#Top:I#Top:F#Top:U1#" & UD1Source & ":U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
		sPOVDestination = ":V#Periodic:O#Import:I#None:F#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
		
		'Define source
		sSource = "A#8_1_1_2_1_1_1_2" & sPOVSource
		
		'Define destination
		sDestination = "A#ContrValDep_Tax" & sPOVDestination
		
		'Clear previously calculated data
		api.Data.ClearCalculatedData(sDestination, clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
		'Execute calc
		api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
		
		
		'Calculate Interest expense group debt
		'------------------------------------------------------
					
		'Define POV variables
		sPOVSource = ":V#YTD:O#Top:I#Top:F#Top:U1#" & UD1Source & ":U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
		sPOVDestination = ":V#Periodic:O#Import:I#None:F#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
		
		'Define source
		sSource = "   A#430305" & sPOVSource
		sSource &= "+ A#430300" & sPOVSource
		
		'Define destination
		sDestination = "A#IntExpGroupDebt_Tax" & sPOVDestination
		
		'Clear previously calculated data
		api.Data.ClearCalculatedData(sDestination, clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
		'Execute calc
		api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
		

		'Calculate Tax
		'------------------------------------------------------
	
		'Define POV variables
		TaxRate = "V#Periodic:A#TaxRate_Tax:F#None:O#BeforeAdj:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
		sPOVSource = ":V#Periodic:O#Top:I#Top:F#Top:U1#"               & UD1Target & ":U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#Top:U8#Top"
		sPOVDestination = ":V#Periodic:O#Import:I#None:F#None:U1#"     & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
		
		'Define source
		sSource = "A#TaxableInc_Tax:O#Top" & sPOVSource & " * " & " "& Taxrate & "/100"
		
		'Define destination
		sDestination = "A#Tax_Tax" & sPOVDestination
		
		'Clear previously calculated data
		api.Data.ClearCalculatedData(sDestination, clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
		'Execute calc
		api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
		
		
	End If 'args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Tax")
	

		
#End Region


		Dim TaxAmount As String = api.Data.GetDataCell("T#PovYear:V#YTD:A#TotalTax_tax:F#None:O#Top:I#None:U1#" &           UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText
		Dim ManualAdj As String = api.Data.GetDataCell("T#PovYear:V#YTD:A#ManualAddRed_Tax:F#None:O#BeforeAdj:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText
		Dim NOLAmount As String = api.Data.GetDataCell("T#PovYear:V#YTD:A#NOL_Tax:F#None:O#BeforeAdj:I#None:U1#" &          UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText
		Dim TaxRate1 As String = api.Data.GetDataCell("T#PovYear:V#Periodic:A#TaxRate_Tax:F#None:O#BeforeAdj:I#None:U1#" &  UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText
		Dim TaxRateDivided As String = TaxRate1 & "/100"
		Dim PovTime As String = api.Pov.Time.Name
		Dim PovScenario As String = api.Pov.Scenario.Name
		
		
#Region "Tax Spreading PL BS"

	If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Spreading") Then
		

 #Region "Taxable amount PL and BS"

'Spreading of the tax amount PL & BS
'------------------------------------------------------
		
	   Dim SpreadPeriod As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SpreadPeriod")
				
'Spreading of taxable amount
'-------------------------------------

		'Clear previously calc data
		 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)		
		'18Jul2024 bero moved clear of mov_1 to before if conditions to clear out residual data
		 api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	

If SpreadPeriod = 12 Then
			 
	#Region "Budget"		 
			 
	'If scenario is BudgetV1
	 If PovScenario.Contains("Budget") Then
			 
		'Spreading for PL account
		'------------------------
			
			'Define source
			sSource = TaxAmount & "/12"		

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			 
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
			   
		#End Region
		
	#Region "Q_FCST_6_6"
			   
	  ElseIf PovScenario.Contains("Q_FCST_6_6") Then
			   
			   If PovTime.Contains("M7") Or PovTime.Contains("M8") Or PovTime.Contains("M9") Or PovTime.Contains("M10") Or Povtime.Contains("M11") Or PovTime.Contains("M12") Then 
				   
		'Spreading for PL account
		'------------------------
			
			'Define sources
			sSource = TaxAmount

			Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			sSource3 = "("& sSource &" - " & sSource2a & ") / 6"

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

			 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			   			   
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)  
			   
		  		 End If
				 
				 
	#End Region
	
	#Region "Q_FCST_7_5"
	
		  ElseIf PovScenario.Contains("Q_FCST_7_5") Then
			   
			   If PovTime.Contains("M8") Or PovTime.Contains("M9") Or PovTime.Contains("M10") Or Povtime.Contains("M11") Or PovTime.Contains("M12") Then 
				   
		'Spreading for PL account
		'------------------------
			
			'Define source
			sSource = TaxAmount 	
			
			Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			sSource3 = "("& sSource &" - " & sSource2a & ") / 5"

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If			 
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)  
			   
		  		 End If
			
	#End Region
	
	#Region "Q_FCST_8_4"
	
			  ElseIf PovScenario.Contains("Q_FCST_8_4") Then
			   
			   If PovTime.Contains("M9") Or PovTime.Contains("M10") Or Povtime.Contains("M11") Or PovTime.Contains("M12") Then 
				   
		'Spreading for PL account
		'------------------------
			
			'Define source
			sSource = TaxAmount	
			
			Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			sSource3 = "("& sSource &" - " & sSource2a & ") / 4"

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If			 
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)  
			   
		  		 End If
	
	#End Region
	
	#Region "Q_FCST_9_3"
	
				  ElseIf PovScenario.Contains("Q_FCST_9_3") Then
			   
			   If PovTime.Contains("M10") Or Povtime.Contains("M11") Or PovTime.Contains("M12") Then 
				   
		'Spreading for PL account
		'------------------------
			
			'Define source
			sSource = TaxAmount
			
			Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			sSource3 = "("& sSource &" - " & sSource2a & ") / 3"

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		 
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)  
			   
		  		 End If
	
	#End Region
	
	#Region "Q_FCST_10_2"
	
					  ElseIf PovScenario.Contains("Q_FCST_10_2") Then
			   
			   If Povtime.Contains("M11") Or PovTime.Contains("M12") Then 
				   
		'Spreading for PL account
		'------------------------
			
			'Define source
			sSource = TaxAmount	
			
			Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			sSource3 = "("& sSource &" - " & sSource2a & ") / 2"

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	 
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)  
			   
		  		 End If
	
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
				ElseIf PovScenario.Contains("Q_FCST_11_1") Then
			   
			   If PovTime.Contains("M12") Then 
				   
		'Spreading for PL account
		'------------------------
			
			'Define source
			sSource = TaxAmount		
			
			Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			sSource3 = "("& sSource &" - " & sSource2a & ") "

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		 
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)  
			   
		  		 End If
				 
	End If 'If PovScenario.Contains(" ")
	
	#End Region
		  			   
End If 'If SpreadPeriod = 12
		
If SpreadPeriod = 4 Then 
			 
	#Region "Budget"		 			 
	'If scenario is BudgetV1
	 If PovScenario.Contains("Budget") Then
			 
			 
		'Spreading for PL account
		'------------------------
			
				'Clear previously calc data
			     api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
			     api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
				 
				If PovTime.Contains("M3") Or PovTime.Contains("M6") Or PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				
				 'Define source
				  sSource = TaxAmount & "/4"
				
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
				 
				 'Execute calc
				  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
		
		
		'Spreading for BS account
		'------------------------

		
			  'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				 
			 	End If
				
		#End Region
		
	#Region "Q_FCST_6_6"
		
	ElseIf PovScenario.Contains("Q_FCST_6_6") Then
				 
			 
		'Spreading for PL account
		'------------------------
			
				'Clear previously calc data
			     api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	

				If PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2b As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2b & ") / 2"
				
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
		
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			  'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				 
			 	End If				
	
	#End Region
	
	#Region "Q_FCST_7_5"
	
	ElseIf PovScenario.Contains("Q_FCST_7_5")
								 
			 
		'Spreading for PL account
		'------------------------
			
				'Clear previously calc data
			     api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	

				If PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") / 2"
				
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
		
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			  'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				 
			 	End If				
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
		ElseIf PovScenario.contains("Q_FCST_8_4")
			
				'Spreading for PL account
				'------------------------
			
				'Clear previously calc data
			     api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	

				If PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				
				 'Define source
				  sSource = TaxAmount 
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") / 2"
				
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
		
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			  'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				 
			 	End If				
	
	#End Region
	
	#Region "Q_FCST_9_3"
	
		ElseIf PovScenario.contains("Q_FCST_9_3") Then
				 
			 
		'Spreading for PL account
		'------------------------
			
				'Clear previously calc data
			     api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	

				If PovTime.Contains("M12") Then 
				
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") "
				
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
		
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			  'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				 
			 	End If
	
	#End Region 
	
	#Region "Q_FCST_10_2"
	
	ElseIf PovScenario.Contains("Q_FCST_10_2")
		
				'Spreading for PL account
				'------------------------
			
				'Clear previously calc data
			     api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	

				If PovTime.Contains("M12") Then 
				
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ")"
				
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
		
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			  'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				 
			 	End If
	
	
	#End Region 
	
	#Region "Q_FCST_11_1"
	
		ElseIf PovScenario.Contains("Q_FCST_11_1")
			
				'Spreading for PL account
				'------------------------
			
				'Clear previously calc data
			     api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	

				If PovTime.Contains("M12") Then 
				
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") "
				
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
		
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			  'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				 
			 	End If
				
		End If 'If PovScenario.Contains(" ")
	
	#End Region
	
End If 'SpreadPeriod = 4 Then
		
If SpreadPeriod = 2 Then
		
	#Region "Budget"	
	
	'If scenario is BudgetV1
	 If PovScenario.Contains("Budget") Then
			
			
		'Spreading for PL account
		'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If  Povtime.Contains("M6") Or PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount & "/2"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
				 'Execute calc
				  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			  
			  
		 'Spreading for BS account
		 '------------------------
		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M6") Or PovTime.Contains("M12") Then 				
			
	#End Region
	
	#Region "Q_FCST_6_6"
			
		ElseIf PovScenario.Contains("Q_FCST_6_6") Then
			
		'Spreading for PL account
		'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2c As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2c & ") / 1"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			  
			  
		 'Spreading for BS account
		 '------------------------
		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		
						
		
  #End Region
  
  	#Region "Q_FCST_7_5"
	
		ElseIf PovScenario.Contains("Q_FCST_7_5")
			
			'Spreading for PL account
			'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") / 1"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			  
			  
		 'Spreading for BS account
		 '------------------------
		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		
	
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
	ElseIf PovScenario.Contains("Q_FCST_8_4")
		
			'Spreading for PL account
			'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount 
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			      sSource3 = "("& sSource &" - " & sSource2a & ")"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
				  
			  
				 'Spreading for BS account
				 '------------------------
		 		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		
	
	
	#End Region
  
    #Region "Q_FCST_9_3"
	
			ElseIf PovScenario.Contains("Q_FCST_9_3") Then
			
		'Spreading for PL account
		'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ")"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
			  
		 'Spreading for BS account
		 '------------------------
		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		

	
	#End Region
	
	#Region "Q_FCST_10_2"
	
		ElseIf PovScenario.Contains("Q_FCST_10_2")
			
			'Spreading for PL account
			'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") "
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
			  
				  
				 'Spreading for BS account
				 '------------------------		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
	ElseIf PovScenario.Contains("Q_FCST_11_1")
		
				'Spreading for PL account
				'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				 Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				 sSource3 = "("& sSource &" - " & sSource2a & ") "
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
			  
				 'Spreading for BS account
				 '------------------------
		 	 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 	
				
		End If 'If PovScenario.Contains("Budget")
			
	#End Region
			 
End If 'SpreadPeriod = 2 Then
		
If SpreadPeriod = 1 Then	
		
	#Region "Budget"
		
		'If scenario is BudgetV1
		 If PovScenario.Contains("Budget") Then
			
		'Spreading for PL account
		'------------------------
								  
			  'Clear previously calc data
			  api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		 
			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
				 'Execute calc
				  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)				 		 
			  
			  
		'Spreading for BS account
		'------------------------
		
					  
			  'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			 'Define source
			  sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
			   
			 End If 
			 
	#End Region
	
	#Region "Q_FCST_6_6"
	
			'If scenario is Q_FCST_6_6
		    ElseIf PovScenario.Contains("Q_FCST_6_6") Then
			
		'Spreading for PL account
		'------------------------
								  
			  'Clear previously calc data
			  api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		 
			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2d As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2d & ") / 1"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If			 		 
			  
			  
		'Spreading for BS account
		'------------------------
		
					  
			  'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			 'Define source
			  sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
			   
			 End If 
	
	#End Region
	
	#Region "Q_FCST_7_5"
	
		ElseIf PovScenario.Contains("Q_FCST_7_5")
			
			'Spreading for PL account
			'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") / 1"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
			  
		 'Spreading for BS account
		 '------------------------
		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		
	
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
	ElseIf PovScenario.Contains("Q_FCST_8_4")
		
			'Spreading for PL account
			'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount 
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			      sSource3 = "("& sSource &" - " & sSource2a & ")"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
				  
			  
				 'Spreading for BS account
				 '------------------------
		 		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		
	
	
	#End Region
  
    #Region "Q_FCST_9_3"
	
			ElseIf PovScenario.Contains("Q_FCST_9_3") Then
			
		'Spreading for PL account
		'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ")"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
			  
		 'Spreading for BS account
		 '------------------------
		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		

	
	#End Region
	
	#Region "Q_FCST_10_2"
	
		ElseIf PovScenario.Contains("Q_FCST_10_2")
			
			'Spreading for PL account
			'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ")"
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
			  
				  
				 'Spreading for BS account
				 '------------------------		 
		 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 		
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
	ElseIf PovScenario.Contains("Q_FCST_11_1")
		
				'Spreading for PL account
				'------------------------
								  
			'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)

			 If PovTime.Contains("M12") Then 
				 
				 'Define source
				  sSource = TaxAmount
				  
				  Dim sSource2a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#480000:F#Top:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
				  sSource3 = "("& sSource &" - " & sSource2a & ") "
				 
				 'Define destination
				  sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan" 
				 
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If
			  
			  
				 'Spreading for BS account
				 '------------------------
		 	 			  
			   'Clear previously calc data
			    api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		
			   'Define source
			    sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Define destination
			    sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			   'Execute calc
			    api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
				
			End If 'PovTime.Contains("M12") Then 	
				
		End If 'If PovScenario.Contains(" ")
			
	#End Region

			 
End If 'SpreadPeriod = 1 Then

#End Region 'Taxable amount PL and BS
		
 #Region "Spreading of BS and PL NOLs"	
		
'Spreading of NOLs
'-------------------------------------	


		
			 'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)				
			 api.Data.ClearCalculatedData("V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)				
			 
		
If SpreadPeriod = 12 Then			 
			 
	#Region "Budget"
			 
			'If scenario is BudgetV1
			 If PovScenario.Contains("Budget") Then
			 
			 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided & "/12"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			  
	#End Region
	
	#Region "Q_FCST_6_6"
			  
			  
		 	 ElseIf PovScenario.Contains("Q_FCST_6_6") Then
				  
			If PovTime.Contains("M7") Or PovTime.Contains("M8") Or PovTime.Contains("M9") Or PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
				  
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 6"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
				  
		  	  End If  				  

		  
		  #End Region 
		  
	#Region "Q_FCST_7_5"
	
			 ElseIf PovScenario.Contains("Q_FCST_7_5") Then
				  
			If PovTime.Contains("M8") Or PovTime.Contains("M9") Or PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
				  
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 5"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
				  
		  	  End If  				  
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
		 ElseIf PovScenario.Contains("Q_FCST_8_4") Then
				  
			If PovTime.Contains("M9") Or PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
				  
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 4"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
				  
		  	  End If  				  
	
	#End Region
	
	#Region "Q_FCST_9_3"
	
			 ElseIf PovScenario.Contains("Q_FCST_9_3") Then
				  
			If PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
				  
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 3"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
				  
		  	  End If  				  			  
	
	#End Region
	
	#Region "Q_FCST_10_2"
	
				 ElseIf PovScenario.Contains("Q_FCST_10_2") Then
				  
			If PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
				  
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 2"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
				  
		  	  End If  				  
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
					 ElseIf PovScenario.Contains("Q_FCST_11_1") Then
				  
			If PovTime.Contains("M12") Then 
				  
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
				  
		  	  End If  				  
			  
		  End If 'If PovScenario.Contains("Budget")	
	
	#End Region
					
 End If
		
			 
If SpreadPeriod = 4 Then 
			
	#Region "Budget"
			
			'If scenario is BudgetV1
			 If PovScenario.Contains("Budget") Then
				 
				
			 If PovTime.Contains("M3") Or PovTime.Contains("M6") Or PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				 
				 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided & "/4"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 		 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			  
			 
			 End If
			 
	#End Region
	
	#Region "Q_FCST_6_6"
			 
		 ElseIf PovScenario.Contains("Q_FCST_6_6") Then 
			 
			 If PovTime.Contains("M9") Or PovTime.Contains("M12") Then
				 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided 
			 
			 Dim sSource3b As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3b & ") / 2"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 		 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			   
				 
			 End If
			 
	#End Region
	
	#Region "Q_FCST_7_5"
	
	ElseIf PovScenario.Contains("Q_FCST_7_5")
		
			If PovTime.Contains("M9") Or PovTime.Contains("M12") Then
				 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 2"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 		 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			   
				 
			 End If
	
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
	ElseIf PovScenario.Contains("Q_FCST_8_4")
		
			If PovTime.Contains("M9") Or PovTime.Contains("M12") Then
				 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 2"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 		 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			   
				 
			 End If	
	
	
	#End Region
	
	#Region "Q_FCST_9_3"
	
			 ElseIf PovScenario.Contains("Q_FCST_9_3") Then 
			 
			 If PovTime.Contains("M12") Then
				 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 		 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			   
				 
			 End If			 
			 
	
	#End Region
	
	#Region "Q_FCST_10_2"
	
	ElseIf PovScenario.Contains("Q_FCST_10_2")
		
			If PovTime.Contains("M12") Then
				 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 		 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			   
				 
			 End If			 
	
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
	ElseIf PovScenario.Contains("Q_FCST_11_1")
		
			If PovTime.Contains("M12") Then
				 
			'Spreading for BS account
			'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 		 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			   
				 
			 End If			 
			 
		End If 'If PovScenario.Contains(" ")
		
	
	#End Region
					 
End If
			 
			 
If SpreadPeriod = 2 Then 
			
	#Region "Budget"
			
		'If scenario is BudgetV1
		 If PovScenario.Contains("Budget") Then
			
			 If PovTime.Contains("M6") Or PovTime.Contains("M12") Then 
				 
				 
			'Spreading for BS account
			'------------------------
				 
			'Define source
			sSource = NOLAmount & "*" & TaxRateDivided & "/2"
							
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)		
			 
			 
			 'Spreading for PL account
			 '------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			  
			 
			 End If
			 
		#End Region
		
	#Region "Q_FCST_6_6"
			 
		 Else If PovScenario.Contains("Q_FCST_6_6")
			 
			 If PovTime.Contains("M12") Then
				
			'Spreading for BS account
			'------------------------
				 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			
			 Dim sSource3c As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3c & ") / 1"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 'Spreading for PL account
			 '------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
				 
			 End If

		 
		 #End Region
		 
	#Region "Q_FCST_7_5"
	
	ElseIf PovScenario.Contains("Q_FCST_7_5")
		
			If PovTime.Contains("M12") Then
				
			'Spreading for BS account
			'------------------------
				 
			'Define source
			sSource = NOLAmount & "*" & TaxRateDivided
			
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") / 1"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 'Spreading for PL account
			 '------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
				 
			 End If

	
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
	ElseIf PovScenario.Contains("Q_FCST_8_4")
		
			If PovTime.Contains("M12") Then
				
			'Spreading for BS account
			'------------------------
				 
			'Define source
			sSource = NOLAmount & "*" & TaxRateDivided
			
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ") "
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 'Spreading for PL account
			 '------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
				 
			 End If

	
	#End Region
	
	#Region "Q_FCST_9_3"
	
	ElseIf PovScenario.Contains("Q_FCST_9_3")
		
		
			If PovTime.Contains("M12") Then
				
			'Spreading for BS account
			'------------------------
				 
			'Define source
			sSource = NOLAmount & "*" & TaxRateDivided
			
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 'Spreading for PL account
			 '------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
				 
			 End If		

		 
	#End Region
	
	#Region "Q_FCST_10_2"
	
	ElseIf PovScenario.Contains("Q_FCST_10_2")
		
			If PovTime.Contains("M12") Then
				
			'Spreading for BS account
			'------------------------
				 
			'Define source
			sSource = NOLAmount & "*" & TaxRateDivided
			
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 'Spreading for PL account
			 '------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
				 
			 End If
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
		ElseIf PovScenario.Contains("Q_FCST_11_1")
			 
			 If PovTime.Contains("M12") Then
				
			'Spreading for BS account
			'------------------------
				 
			'Define source
			sSource = NOLAmount & "*" & TaxRateDivided
			
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 'Spreading for PL account
			 '------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
				 
			 End If
			 					 				 				 				 
			 
		 End If 'If PovScenario.Contains("Budget")
	
	
	#End Region
			 
End If
			 
			 
If SpreadPeriod = 1 Then 
			
	#Region "Budget"
	
		'If scenario is BudgetV1
		 If PovScenario.Contains("Budget") Then	
			
			 If PovTime.Contains("M12") Then 				 
				 
			'Spreading for BS account
			'------------------------
	 			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			 
			 End If
			 
	#End Region
	
	#Region "Q_FCST_6_6"
	
	ElseIf PovScenario.Contains("Q_FCST_6_6") Then	
	
			If PovTime.Contains("M12") Then 				 
				 
			'Spreading for BS account
			'------------------------
	 			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3d As Decimal = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3d & ") / 1"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			 
			 End If
	
	#End Region
	
	#Region "Q_FCST_7_5"
	
	ElseIf PovScenario.Contains("Q_FCST_7_5") Then	
		
		 If PovTime.Contains("M12") Then 				 
				 
			'Spreading for BS account
			'------------------------
	 			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			 
			 End If

			 
	#End Region
	
	#Region "Q_FCST_8_4"
	
		ElseIf PovScenario.Contains("Q_FCST_8_4") Then	
		
		 If PovTime.Contains("M12") Then 				 
				 
			'Spreading for BS account
			'------------------------
	 			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			 
			 End If
	
	#End Region
	
	#Region "Q_FCST_9_3"
	
		ElseIf PovScenario.Contains("Q_FCST_9_3") Then	
		
		 If PovTime.Contains("M12") Then 				 
				 
			'Spreading for BS account
			'------------------------
	 			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			 
			 End If

	
	#End Region
	
	#Region "Q_FCST_10_2"
	
	  ElseIf PovScenario.Contains("Q_FCST_10_2") Then	
		
		 If PovTime.Contains("M12") Then 				 
				 
			'Spreading for BS account
			'------------------------
	 			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			 
			 End If

	
	#End Region
	
	#Region "Q_FCST_11_1"
	
	  ElseIf PovScenario.Contains("Q_FCST_11_1") Then	
		
		 If PovTime.Contains("M12") Then 				 
				 
			'Spreading for BS account
			'------------------------
	 			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
			 
			 Dim sSource3a As Decimal = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#Top:U3#None:U4#None:U5#Top:U6#None:U7#Top:U8#Pre_IFRS16_Plan").CellAmountAsText
			
			 sSource3 = "("& sSource &" - " & sSource3a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If NOLAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
			 		 
			'Spreading for PL account
			'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
			 
			 End If
			 
		 End If 'If PovScenario.Contains(" ")
	
	#End Region	
	
 End If


#End Region 'NOLs Spreading


	End If 'args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("Spreading") Then
	
		
#End Region 'Tax Spreading PL BS

#Region "Spreading of the tax amount for CF"

'Spreading of the tax amount CF
'------------------------------------------------------

	If args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SpreadingCF") Then
		
	  Dim SpreadPeriodCF As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("SpreadPeriodCF")
	  
	  		 'Clear previously calc data
			 api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
			 api.Data.ClearCalculatedData("V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)
		  
		
If SpreadPeriodCF = 12 Then
			 
 	#Region "Budget"
			 
	'If scenario is BudgetV1		 
	If PovScenario.Contains("Budget") Then
			 
	'Spreading for CF account
	'------------------------
	
			'Tax Impact
			'------------
		
			'Define source
			sSource = TaxAmount & "/12"		
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj & "/12"		
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
	
	#End Region
	
	#Region "Q_FCST_6_6"
			 
	ElseIf PovScenario.Contains("Q_FCST_6_6")
		
			 
		 If PovTime.Contains("M7") Or PovTime.Contains("M8") Or PovTime.Contains("M9") Or PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
			
			'Define source
			sSource = TaxAmount
			
			Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			sSource3 = "("& sSource &" - " & sSource2a & ") / 6"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 6"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
			 
		 End If 'If PovTime.Contains("M6") Or PovTime.Contains("M7") etc..
			
		 
	#End Region
	
	#Region "Q_FCST_7_5"
	
		ElseIf PovScenario.Contains("Q_FCST_7_5")
		
			 
		 If PovTime.Contains("M8") Or PovTime.Contains("M9") Or PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
			
			'Define source
			sSource = TaxAmount	
			
			Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			sSource3 = "("& sSource &" - " & sSource2a & ") / 5"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 5"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		  End If 
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
			ElseIf PovScenario.Contains("Q_FCST_8_4")
		
			 
		 If PovTime.Contains("M9") Or PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
			
			'Define source
			sSource = TaxAmount
			
			Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			sSource3 = "("& sSource &" - " & sSource2a & ") / 4"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj				
						
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 4"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		  End If
		  
	
	#End Region
	
	#Region "Q_FCST_9_3"
	
				ElseIf PovScenario.Contains("Q_FCST_9_3")
		
			 
		 If PovTime.Contains("M10") Or PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
			
			'Define source
			sSource = TaxAmount				
						
			Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			sSource3 = "("& sSource &" - " & sSource2a & ") / 3"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 3"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		  End If
		   
	
	#End Region
	
	#Region "Q_FCST_10_2"
	
					ElseIf PovScenario.Contains("Q_FCST_10_2")
		
			 
		 If PovTime.Contains("M11") Or PovTime.Contains("M12") Then 
			
			'Define source
			sSource = TaxAmount	
			
			Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			sSource3 = "("& sSource &" - " & sSource2a & ") / 2"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 2"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		  End If
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
				ElseIf PovScenario.Contains("Q_FCST_11_1")		
			 
		 If PovTime.Contains("M12") Then 
			
			'Define source
			sSource = TaxAmount & "/1"		
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj 	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 1"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		  End If
		  
		 End If 'If PovScenario.Contains("Budget")	
	
	#End Region
	
		  End If 'SpreadPeriodCF = 12
		  
		  
If SpreadPeriodCF = 4 Then
			  
	#Region "Budget"
			  
			'If scenario is BudgetV1		 
			 If PovScenario.Contains("Budget") Then
			  
			 
			 If PovTime.Contains("M3") Or PovTime.Contains("M6") Or PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				 
				 
			'Spreading for CF account
			'------------------------	
			 
			'Define source
			 sSource = TaxAmount & "/4"		
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
			 		  
'		  	'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj & "/4"		
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 End If
			 
	#End Region
	
	#Region "Q_FCST_6_6"
			 
			 
		ElseIf PovScenario.Contains("Q_FCST_6_6") Then
			
			
			If PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				
				
			'Spreading for CF account
			'------------------------	
			 
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ") / 2"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 		  
'		  	'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 2"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
			 End If			  
		  
	#End Region
	
	#Region "Q_FCST_7_5"
	
	
		ElseIf PovScenario.Contains("Q_FCST_7_5") Then 
			
			
			If PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
				
				
			'Spreading for CF account
			'------------------------	
			 
			'Define source
			 sSource = TaxAmount 	
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ") / 2"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 		  
'		  	'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 2"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
			 End If			  			
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
	
	ElseIf PovScenario.Contains("Q_FCST_8_4") Then
		
		
			If PovTime.Contains("M9") Or PovTime.Contains("M12") Then 
								
				
			'Spreading for CF account
			'------------------------	
			 
			'Define source
			 sSource = TaxAmount	
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ") / 2"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 		  
'		  	'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 2"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
			 End If			  
	
	
	#End Region
	
	#Region "Q_FCST_9_3"
	
			ElseIf PovScenario.Contains("Q_FCST_9_3") Then
			
			
			If PovTime.Contains("M12") Then 
				
				
			'Spreading for CF account
			'------------------------	
			 
			'Define source
			 sSource = TaxAmount	
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 		  
'		  	'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 1"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
			 End If			 	 
	
	#End Region 
	
	#Region "Q_FCST_10_2"
			
	
		ElseIf PovScenario.Contains("Q_FCST_10_2") Then
			
				
			If PovTime.Contains("M12") Then 
				
				
			'Spreading for CF account
			'------------------------	
			 
			'Define source
			 sSource = TaxAmount		
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 		  
'		  	'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 1"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
			 End If		
	
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
	ElseIf PovScenario.Contains("Q_FCST_11_1") Then 
		
		
			If PovTime.Contains("M12") Then 
				
				
			'Spreading for CF account
			'------------------------	
			 
			'Define source
			 sSource = TaxAmount	
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
			 		  
'		  	'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ")"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
			 End If	
			 			 			 
			 			 
		  End If 'If PovScenario.Contains("Budget") Then

	#End Region
		
End If 'SpreadPeriodCF = 4
		  
		  
If SpreadPeriodCF = 2 Then
			  
	#Region "Budget"
			  
			 'If scenario is BudgetV1		 
			 If PovScenario.Contains("Budget") Then
			 
		'Spreading for CF account
		'------------------------
						 	 
			 If PovTime.Contains("M6") Or PovTime.Contains("M12") Then 
			 
			'Define source
			 sSource = TaxAmount & "/2"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj & "/2"		
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
			 End If
			 
	#End Region
	
	#Region "Q_FCST_6_6"
			 
			 
			ElseIf PovScenario.Contains("Q_FCST_6_6") Then
			
			
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ")"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		 	End If
			 
			
 #End Region 
 
 	#Region "Q_FCST_7_5"
	
	ElseIf PovScenario.Contains("Q_FCST_7_5")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ")"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		 	End If	
			
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
	ElseIf PovScenario.Contains("Q_FCST_8_4")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ")"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		 	End If
	
	#End Region
 
 	#Region "Q_FCST_9_3"
	
			ElseIf PovScenario.Contains("Q_FCST_9_3") Then
			
			
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ")"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
		 	End If
			 
	
	#End Region
	
	#Region "Q_FCST_10_2"
	
	
		ElseIf PovScenario.Contains("Q_FCST_10_2")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ") "
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ")"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		 	End If
	
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
	ElseIf PovScenario.Contains("Q_FCST_11_1")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj & "/1"
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ")"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		 	End If
			
						 
		End If 'If PovScenario.Contains("Budget") Then
	
	
	#End Region
			 
End If 'SpreadPeriodCF = 2 


If SpreadPeriodCF = 1 Then 
	
	#Region "Budget"
			  
			 'If scenario is BudgetV1		 
			 If PovScenario.Contains("Budget") Then
			 
		'Spreading for CF account
		'------------------------
						 	 
			 If  PovTime.Contains("M12") Then 
			 '21-11-2024, Andras Nederhoed: removed from if statementPovTime.Contains("M6") Or
			'Define source
			 sSource = TaxAmount 
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj		
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
			 End If
			 
	#End Region
	
	#Region "Q_FCST_6_6"
			 
			 
			ElseIf PovScenario.Contains("Q_FCST_6_6") Then
			
			
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ") / 1"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM6:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 1"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		 	End If
			 
			
 #End Region 
 
 	#Region "Q_FCST_7_5"
	
	ElseIf PovScenario.Contains("Q_FCST_7_5")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount 
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ") / 1"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If	
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM7:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") / 1"
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		 	End If	
			
	
	#End Region
	
	#Region "Q_FCST_8_4"
	
	ElseIf PovScenario.Contains("Q_FCST_8_4")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM8:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") "
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		 	End If
	
	#End Region
 
 	#Region "Q_FCST_9_3"
	
			ElseIf PovScenario.Contains("Q_FCST_9_3") Then
			
			
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj		
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM9:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") "
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		 	End If
			 
	
	#End Region
	
	#Region "Q_FCST_10_2"
	
	
	ElseIf PovScenario.Contains("Q_FCST_10_2")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount
			 
			 Dim sSource2a As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

			 sSource3 = "("& sSource &" - " & sSource2a & ")"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM10:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") "
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If
			 
		 	End If
	
	
	#End Region
	
	#Region "Q_FCST_11_1"
	
	ElseIf PovScenario.Contains("Q_FCST_11_1")
		
		
			If PovTime.Contains("M12") Then 
				
			'Define source
			 sSource = TaxAmount & "/1"
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
		If TaxAmount <> 0 Then 
			 
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
		End If		
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj	
			
'			Dim sSource2b As Decimal  = api.Data.GetDataCell("T#WFYearM11:V#YTD:A#770000:F#FPA_MOV_Tech:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan").CellAmountAsText

'			sSource3 = "("& sSource &" - " & sSource2b & ") "
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'		If ManualAdj <> 0 Then 
			 
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource3})",True)
			 
'		End If	
			 
		 	End If
			
						 
		End If 'If PovScenario.Contains("Budget") Then
	
	
	#End Region
	
	
End If 'If SpreadPeriodCF = 1 Then 
	

End If 'args.CustomCalculateArgs.FunctionName.XFEqualsIgnoreCase("SpreadingCF")
	
	
#End Region 'Taxable amount CF spreading

#Region "5YP"

'Spreading of the tax amount PL & BS
'------------------------------------------

If PovScenario.Contains("5YP") Then

		'Spreading for PL account
		'------------------------
			
			'Define source
			sSource = TaxAmount	

			'Define destination
			 sDestination = "V#Periodic:A#480000:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"

			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)			 
			 
		
		'Spreading for BS account
		'------------------------
		
			   'Clear previously calc data
			   api.Data.ClearCalculatedData("V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan",clearCalculatedData:=True, clearTranslatedData:=True, clearConsolidatedData:=True, clearDurableCalculatedData:=True)	
		
			   'Define source
			   sSource2 = "-V#Periodic:A#480000:F#None:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			  
			  'Define destination
			   sDestination2 = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"	
			  
			  'Execute calc
			   api.Data.Calculate($"{sDestination2} = RemoveZeros({sSource2})",True)
			   			   
			   
'Spreading of NOLs
'-------------------------------------	

		'Spreading for BS account
		'------------------------
			 
			'Define source
			 sSource = NOLAmount & "*" & TaxRateDivided
				
			 'Define destination
			 sDestination = "V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
		'Spreading for PL account
		'------------------------
		
			 'Define source
			  sSource = "-V#Periodic:A#660009:F#F_CIT_receivable_payable_Mov_1:O#Top:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Define destinatnion
			  sDestination = "V#Periodic:A#480100:F#None:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			 
			 'Exceute calc
			  api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)
						  
			  
'Spreading for CF account
'------------------------
	
			'Tax Impact
			'------------
		
			'Define source
			sSource = TaxAmount		
				
			'Define destination
			 sDestination = "V#Periodic:A#770000:F#F_CIT_receivable_payable_Mov_3:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
			'Execute calc
			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			 
			 
'			'Manual Adj Impact
'			'-----------------
			
'			'Define source
'			sSource = ManualAdj		
				
'			'Define destination
'			 sDestination = "V#Periodic:A#770000:F#FPA_MOV_Tech:O#Import:I#None:U1#" & UD1Target & ":U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#Pre_IFRS16_Plan"
			
'			'Execute calc
'			 api.Data.Calculate($"{sDestination} = RemoveZeros({sSource})",True)	
			   

End If 'If PovScenario.Contains("5YP")
		   

#End Region 'Calculations for 5YP scenarios
	

End If 'If UD1Text2_Budget.Contains("LE_")
	
Next 'For Each UD1Member In UD1List
						
#End Region

						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace