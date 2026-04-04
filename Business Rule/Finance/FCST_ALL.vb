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

Namespace OneStream.BusinessRule.Finance.FCST_ALL
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				' ---------------------------------------------------
				' GLOBAL VARIABLES
				' ---------------------------------------------------
				
				Dim sItemID As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("ItemID")	
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")		
				
				' ---------------------------------------------------
				' FUNCTIONS
				' ---------------------------------------------------
				
				' BORRA - las UD4 de los cálculos de: AMORT, VAT, IFRS16, Interest, Personnel, Warranty, CorpRate, Payable_TSA y WCAP (está comentado)
					If sFunction = "BorrarUDs" Then
					
						Me.borradoUD(si, api, globals, args)
						
					Else If sFunction = "BorrarDrivers" Then
						
						Me.borradoDrivers(si, api, globals, args)
						
					End If				
				
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then
					
					' BORRA - las UD4 de los cálculos de: AMORT, VAT, IFRS16, Interest, Personnel, Warranty, CorpRate, Payable_TSA y WCAP (está comentado)
					If sFunction = "BorrarUDs" Then
					
						Me.borradoUD(si, api, globals, args)
											
					Else If sFunction = "P_X_Q_Manual_ADJ"							
						
						Me.P_X_Q_Manual_ADJ(si, api, globals, args)	
					'	Me.Clear_Vol_Prod(si, api, globals, args)	
						
					'CALCULATE DATA - volume * unitary Material Cost
					Else If sFunction = "P_X_Q_ICP"							
						
						Me.P_X_Q_ICP(si, api, globals, args)	
					
						
					'COPY A#Vol_Sales and A#Vol_Prod from Horse cube to Analytics cube	
					Else If sFunction = "Copy_Volumes_Analytics"							
						
						Me.Copy_Volumes_Analytics(si, api, globals, args)
						
					'CALCULATE - Personnel, Interest, IFRS16, Payables TSA, Warranty, AMORT, Corp Rate, VAT					
					Else If sFunction = "Others" Then
					
						Me.Others(si, api, globals, args)
						
					'CALCULATE - Royalties				
					Else If sFunction = "Royalties" Then
						
						Me.Royalties(si, api, globals, args)
					
					'CALCULATE - Engeniering
					Else If sFunction = "Engineering" Then
						
						Me.Engineering(si, api, globals, args)	
					
					'CALCULATE - Loans			
					Else If sFunction = "Loans" Then
					
						Me.Loans(si, api, globals, args)	
					'CALCULATE - CorpRate2				
					Else If sFunction = "CorpRate2" Then
					
						Me.othersCorp(si, api, globals, args)	
					
					'CALCULATE - Change in CIT Automatism
					Else If sFunction = "CIT_Automatism" Then
					
						Me.CIT_Automatism(si, api, globals, args)
						
					End If							
				End If
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#Region "Borrado"
	Sub borradoUD  (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	
		
		api.Data.ClearCalculatedData("U4#WCAP_DSO",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#WCAP_DPO",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#WCAP_DIO",True,True,True,True)			
		api.Data.ClearCalculatedData("U4#PERS",True,True,True,True)
		api.Data.ClearCalculatedData("U4#INTER",True,True,True,True)
		api.Data.ClearCalculatedData("U4#WRTY",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#IFRS16",True,True,True,True)
		api.Data.ClearCalculatedData("U4#CORP",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#AMORT",True,True,True,True)
		api.Data.ClearCalculatedData("U4#DSOTSA",True,True,True,True)
		api.Data.ClearCalculatedData("U4#VAT",True,True,True,True)
		api.Data.ClearCalculatedData("U4#OTHFIN",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#OTHOP",True,True,True,True)
		api.Data.ClearCalculatedData("U4#ROYAL", True, True, True, True)
		api.Data.ClearCalculatedData("U4#CAPEXRD", True, True, True, True)
		api.Data.ClearCalculatedData("U4#DEFINCOME", True, True, True, True)
		api.Data.ClearCalculatedData("U4#LOANS", True, True, True, True)
		api.Data.ClearCalculatedData("U4#VOLUMES", True, True, True, True)
		api.Data.ClearCalculatedData("U4#CIT", True, True, True, True)
		api.Data.ClearCalculatedData("U4#CITPY", True, True, True, True)
		
	End Sub
	
		Sub borradoDrivers  (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	
								
			api.Data.ClearCalculatedData("A#Driver_Customs",True,True,True,True)
			api.Data.ClearCalculatedData("A#Driver_Deferred_income",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_Engineering_Cost",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_Engineering_Revenue",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_IFRS16",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_Logistic",True,True,True,True)
			api.Data.ClearCalculatedData("A#Driver_Material_Cost",True,True,True,True)
			api.Data.ClearCalculatedData("A#Driver_Packaging",True,True,True,True)
			api.Data.ClearCalculatedData("A#Driver_Revenue",True,True,True,True)
			api.Data.ClearCalculatedData("A#Driver_Royalties",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_Specific_Tooling",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_Startup_Cost",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_Vendor_Tooling_SET",True,True,True,True)
			api.Data.ClearCalculatedData("A#Driver_VTU_DirectLabor",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_VTU_IndirectProd",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Driver_VTU_Amort",True,True,True,True)
			api.Data.ClearCalculatedData("A#Driver_Warranty",True,True,True,True)	
			
			api.Data.ClearCalculatedData("A#Volume",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Vol_Sales",True,True,True,True)	
			api.Data.ClearCalculatedData("A#Vol_Prod",True,True,True,True)
		
		End Sub	
	
#End Region

#Region "P*Q ICP"

	' Calculo para la cuenta de material costs, cuyo ICP se calcula en base a las ventas que se calculan en la funcion de arriba
	Sub P_X_Q_ICP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
		
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
	
		If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
	
			If api.Cons.IsLocalCurrencyforEntity() Then
	   					
			
				' Unitary Material Costs(551103) 
			api.Data.ClearCalculatedData("A#Unit_Material_Cost:O#Import",True,True,True,True)
			api.Data.ClearCalculatedData("A#551103:O#Import",True,True,True,True)
			api.Data.ClearCalculatedData("A#551115:O#Import",True,True,True,True)
			' api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#PERS",,,,)
			
			' La ICP la obtenemos de la parte de ventas
			
			Dim EntityList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
			Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
			
			Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
			Dim sEntityPOV As String = api.Pov.Entity.Name	
			Dim sTime As String = api.Pov.Time.Name		
			
			' Recuperamos la moneda de la entidad principal y la moneda común
			Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
			Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
			Dim commonCurrencyID As Integer = Currency.EUR.Id
			
			' Definimos variables necesarias para el conseguir el tipo de cambio
			Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
			Dim cubeId As Integer = api.Pov.Cube.CubeId
			Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId

			If sEntityPOV = "R1300005" Or sEntityPOV = "R1300006" Or sEntityPOV = "R1300007" Then
		
				' Recorremos el listado de empresas para comprobar si alguna tiene es ICP contra la entity que se pasa en el POV
				For Each ICMember As Member In ICList
					
					Dim sIC As String = ICMember.Name.ToString
	
					' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
					Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
					Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
					Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)
						
					' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
					' Dim salesICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#541107:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					' Calculamos
					' If (salesICPAmount <> 0)					
					' 	api.Data.Calculate(
					' 		"E#" & sEntityPOV & ":C#" & entityPOVCurrencyName & ":V#YTD:A#551103:O#Import:U3#None:I#" & sIC & " = " &
					' 		"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#541107:O#Top:U3#None:I#" & sEntityPOV &
					' 		"* (-1) * " & revRate.ToString.Replace(",","."),True)
					' 	
					' End If
					' --------------------------------------------------------------------------------------------------------------------------------------

					Dim salesICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#541107:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U3#Top:U4#Projections:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					' Calculamos
					If (salesICPAmount <> 0)					
						api.Data.Calculate(
							"E#" & sEntityPOV & ":C#" & entityPOVCurrencyName & ":V#YTD:A#551103:O#Import:U4#VOLUMES:I#" & sIC & " = " &
							"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#541107:O#Top:U4#Projections:I#" & sEntityPOV &" * (-1) * " & revRate.ToString.Replace(",","."),True
							)
						
					End If

				Next	
				
				' Ajustamos el importe de coste con terceros, quitando la parte ICP que viene de ventas

				' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
				' api.Data.Calculate("A#551103:O#Import:I#None = A#Material_Cost_Temp:O#Import:I#None - A#551103:O#Import:I#ICEntities",True)
				' --------------------------------------------------------------------------------------------------------------------------------------
				api.Data.Calculate("A#551103:O#Import:I#None:U4#VOLUMES = A#Material_Cost_Temp:O#Import:I#None:U4#VOLUMES - A#551103:O#Import:I#ICEntities:U4#Projections",True)
							
				
			Else 	
		
				' Recorremos el listado de empresas para comprobar si alguna tiene es ICP contra la entity que se pasa en el POV
				For Each ICMember As Member In ICList
					
					Dim sIC As String = ICMember.Name.ToString
	
					' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
					Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
					Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
					Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)

					' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------	
					' Dim salesICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#541107:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					' 
					' ' Calculamos
					' If (salesICPAmount <> 0)					
					' 	api.Data.Calculate(
					' 		"E#" & sEntityPOV & ":C#" & entityPOVCurrencyName & ":V#YTD:A#551115:O#Import:U3#None:I#" & sIC & " = " &
					' 		"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#541107:O#Top:U3#None:I#" & sEntityPOV &
					' 		"* (-1) * " & revRate.ToString.Replace(",","."),True)
					' 	
					' End If
					' --------------------------------------------------------------------------------------------------------------------------------------

					Dim salesICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#541107:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U3#Top:U4#Projections:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					' Calculamos
					If (salesICPAmount <> 0)					
						api.Data.Calculate(
							"E#" & sEntityPOV & ":C#" & entityPOVCurrencyName & ":V#YTD:A#551115:O#Import:U4#VOLUMES:I#" & sIC & " = " &
							"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#541107:O#Top:U4#Projections:I#" & sEntityPOV &" * (-1) * " & revRate.ToString.Replace(",","."),True
							)						
					End If
						
				Next

				' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------	
				' Ajustamos el importe de coste con terceros, quitando la parte ICP que viene de ventas
				' api.Data.Calculate("A#551115:O#Import:I#None = A#Material_Cost_Temp:O#Import:I#None - A#551115:O#Import:I#ICEntities",True)
				' --------------------------------------------------------------------------------------------------------------------------------------
				
				api.Data.Calculate("A#551115:O#Import:I#None:U4#VOLUMES = A#Material_Cost_Temp:O#Import:I#None:U4#VOLUMES - A#551115:O#Import:I#ICEntities:U4#Projections",True)
				
				
			End If
					
			End If	
		
		End If
			
	End Sub
#End Region

#Region "Copy Volumes Analytics" 

	Sub Copy_Volumes_Analytics (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
		
		'Clear scenario calculated data
		Dim targetDataBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#" & api.Pov.Scenario.Name  & ",A#Vol_Sales, A#Vol_Prod)")
		
		UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer)
	
		If (ScenarioType = "Forecast" And sMes <= MesFcstNumber) Then
				
				
				'Declare target and source data buffers
				Dim siDestinationInfo1 As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("S#" & api.Pov.Scenario.Name)
				Dim sourceDataBuffer1 As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#Actual,A#Vol_Sales, A#Vol_Prod)")
				
				'Process data buffer
				api.Data.SetDataBuffer(sourceDataBuffer1,siDestinationInfo1,,,,,,,,,,,,,True)	
				
		Else 
		

				'Clear scenario calculated data
				'Dim targetDataBuffer2 As DataBuffer = api.Data.GetDataBufferUsingFormula($"FilterMembers(Cb#Analytics:S#" & api.Pov.Scenario.Name  & ",U4#None,A#Vol_Prod,A#Vol_Sales)")
				
				'UTISharedFunctionsBR.ClearDataBuffer(si,api,targetDataBuffer2)
				
				'Declare target and source data buffers
				Dim siDestinationInfo2 As ExpressionDestinationInfo = api.Data.GetExpressionDestinationInfo("U4#None")
				Dim sourceDataBuffer2 As DataBuffer = api.Data.GetDataBufferUsingFormula($"RemoveZeros(FilterMembers(Cb#Horse:S#" & api.Pov.Scenario.Name  & ",U4#VOLUMES, A#Vol_Prod,A#Vol_Sales))")
				
				'Process data buffer
				api.Data.SetDataBuffer(sourceDataBuffer2,siDestinationInfo2,,,,,,,,,,,,,True)
				
		End If
				
		
End Sub


#End Region

#Region "P*Q Manual Adjustments"

	
	Sub P_X_Q_Manual_ADJ (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
		
		If api.Cons.IsLocalCurrencyforEntity() Then
   							
				
			api.Data.ClearCalculatedData("A#541107:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511065:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#556107:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511064:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511051:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511056:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#551103:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#551115:O#Forms:U4#VOL_ADJ",True,True,True,True)
			
			' Sales Volume x Price				
			api.Data.Calculate("A#541107:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#541107:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
	  		' Prod Volume x UnitLogistic
			api.Data.Calculate("A#5511065:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#5511065:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
			'Sales Volume x UnitWarranty
			api.Data.Calculate("A#556107:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#556107:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
			' Prod Volume x Unitary Customs
			api.Data.Calculate("A#5511064:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#5511064:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
			'Production Volume x Unitary Variable Transformation Value 51
			api.Data.Calculate("A#5511051:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#5511051:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
			'Production Volume x Unitary Variable Transformation Value 56
			api.Data.Calculate("A#5511056:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#5511056:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
			' Unitary Material Costs(551103) & (551115)

			Dim sEntityPOV As String = api.Pov.Entity.Name	
			
			If sEntityPOV = "R1300005" Or sEntityPOV = "R1300006" Or sEntityPOV = "R1300007" Then
				api.Data.Calculate("A#551103:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#551103:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
 			Else	
				api.Data.Calculate("A#551115:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#551115:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
			End If
	
		End If	
	End Sub	


#End Region
		
#Region "Other Calcs"

	#Region "Others"
		Sub Others (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
			api.Data.ClearCalculatedData("U4#PERS",True,True,True,True)
			api.Data.ClearCalculatedData("U4#INTER",True,True,True,True)
			api.Data.ClearCalculatedData("U4#WRTY",True,True,True,True)	
			api.Data.ClearCalculatedData("U4#IFRS16",True,True,True,True)
			api.Data.ClearCalculatedData("U4#CORP",True,True,True,True)	
			api.Data.ClearCalculatedData("U4#AMORT",True,True,True,True)
			api.Data.ClearCalculatedData("U4#DSOTSA",True,True,True,True)
			api.Data.ClearCalculatedData("U4#VAT",True,True,True,True)	
			api.Data.ClearCalculatedData("U4#OTHFIN",True,True,True,True)	
			api.Data.ClearCalculatedData("U4#OTHOP",True,True,True,True)
		    api.Data.ClearCalculatedData("U4#CAPEXRD", True, True, True, True)
			api.Data.ClearCalculatedData("U4#DEFINCOME",True,True,True,True)	
			api.Data.ClearCalculatedData("U4#LOANS",True,True,True,True)
			api.Data.ClearCalculatedData("U4#CIT",True,True,True,True)
			api.Data.ClearCalculatedData("U4#CITPY",True,True,True,True)
			
			' ----------------------------------------------------------------------------------------------------------------
			' VARIABLES
			' ----------------------------------------------------------------------------------------------------------------
			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
			Dim sScenario As String = api.Pov.Scenario.Name
			Dim sTime As String = api.Pov.Time.Name '2024M9 -->  T#{year}M{(sMes+1).ToString()}
			Dim year As String = sTime.Substring(0,4)
			Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))	
			Dim sEntity As String = api.pov.Entity.Name ' R1301002
			Dim sEntityParent As String = Left(sEntity,5) ' R1301
			Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods '2 en feb
			
			' ----------------------------------------------------------------------------------------------------------------
			' GLOBAL ASSUMPTIONS
			' ----------------------------------------------------------------------------------------------------------------
			
			' Personnel
			Dim salaryPayments = api.Data.GetDataCell("E#" & sEntity & ":A#Salary_pays:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
			Dim bonusProvision = api.Data.GetDataCell("E#" & sEntityParent & "001:A#Bonus%:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
'			Dim priorYearBonusPayment = api.Data.GetDataCell("E#" & sEntityParent & "001:A#Bonus_pay:T#"& sTime &":F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None").CellAmount.ToString(cultureinfo.InvariantCulture)
			
			' Corp Rate
			Dim corpRate As String = api.Data.GetDataCell("E#"& sEntityParent &"001:C#Local:S#"& sScenario &":T#"& sTime &":V#Periodic:A#Corp_Tax:F#None:O#Forms:I#None:U2#None:U3#None:U4#Manual_Input:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
		
			' ----------------------------------------------------------------------------------------------------------------
			'  AGGREGATORS 
			'		Serán los que pasen por el for each para agregar los valores de los hijos en la 001
			' ----------------------------------------------------------------------------------------------------------------
			
			' Personnel
			Dim totalPersonnel As String = String.Empty
			Dim totalPersonnelRF As String = String.Empty
			Dim menstotalPersonnel As String = String.Empty
			Dim priorYearBPCell As String = String.Empty
			
			' Interest
			Dim logistic As String = String.Empty
			Dim interestPaid As String = String.Empty
			Dim financialDebt As String = String.Empty
			Dim financialDebtYTD As String = String.Empty
			Dim financialDebtYTDant As String = String.Empty
			Dim financialIncome As String = String.Empty
			
			' IFRS16
			Dim interest As String = String.Empty
			
			' Payables TSA
			Dim subcontratingIng As String = String.Empty
			Dim subcontratingIngYTD6 As String = String.Empty
			Dim subcontratingIngYTD12 As String = String.Empty
			Dim subcontratingIng_IC As String = String.Empty
			Dim subcontratingIngYTD6_IC As String = String.Empty
			Dim subcontratingIngYTD12_IC As String = String.Empty
			
			' Warranty
			Dim warrantyOther As String = String.Empty
			
			' VAT
			
			' Corp Rate
			Dim dataEBT As String = String.Empty
			Dim data411 As String = String.Empty
			
			
			'AMORT
			Dim intangAssets As String = String.Empty
			Dim capitDevelop As String = String.Empty
			Dim rightUse As String = String.Empty
			Dim sw As String = String.Empty
			Dim depreciation As String = String.Empty
			Dim provisions As String = String.Empty
			
			'CAPEXRD
			Dim CapexRD114 As String = String.Empty
			Dim CapexRD118 As String = String.Empty
			
			'Other Financial 
			Dim othFinancial As String = String.Empty
			
			'Other Operating
			Dim othOperatingCash As String = String.Empty
			Dim othOperatingProvision As String = String.Empty
			Dim othOperatingIndustrial As String = String.Empty
			Dim othOperatingIntellectual As String = String.Empty
			
			'Deferred Income
			Dim defIncome As String = String.Empty
			
			'Loans
			Dim stLoanLiability As String = String.Empty
			Dim stLoanAsset As String = String.Empty
			Dim ltLoanLiability As String = String.Empty
			Dim ltLoanAsset As String = String.Empty
			
			Dim stInterestLiability As String = String.Empty
			Dim stInterestAsset As String = String.Empty
			Dim ltInterestLiability As String = String.Empty
			Dim ltInterestAsset As String = String.Empty
			
			Dim i As Integer = 0
			
			For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True) 'R1301.BASE
			
				If i <> 0 Then
				
					totalPersonnel = totalPersonnel & " + "
					totalPersonnelRF = totalPersonnelRF & " + "
					menstotalPersonnel = menstotalPersonnel & " + "
					priorYearBPCell = priorYearBPCell & " + "
					
					logistic = logistic & " + "
					interestPaid = interestPaid & " + "
					financialDebt = financialDebt & " + "
					financialDebtYTD = financialDebtYTD & " + "
					financialDebtYTDant = financialDebtYTDant & " + "
					financialIncome = financialIncome & " + "
					
					interest = interest & " + "
					
					subcontratingIng = subcontratingIng & " + "
					subcontratingIngYTD6 = subcontratingIngYTD6 & " + "
					subcontratingIngYTD12 = subcontratingIngYTD12 & " + "
					subcontratingIng_IC = subcontratingIng_IC & " + "
					subcontratingIngYTD6_IC = subcontratingIngYTD6_IC & " + "
					subcontratingIngYTD12_IC = subcontratingIngYTD12_IC & " + "
					
					warrantyOther = warrantyOther & " + "
					
					dataEBT = dataEBT & " + "
					data411 = data411 & " + "
					
					
					intangAssets = intangAssets & " + "
					capitDevelop = capitDevelop & " + "
					rightUse = rightUse & " + "
					sw = sw & " + "
					depreciation = depreciation & " + "
					provisions = provisions & " + "
					
					CapexRD114 = CapexRD114 & " + "
					CapexRD118 = CapexRD118 & " + "
					
					othFinancial = othFinancial & " + "
					
					othOperatingCash = othOperatingCash  & " + "
					othOperatingProvision = othOperatingProvision & " + "
					othOperatingIndustrial = othOperatingIndustrial & " + "
					othOperatingIntellectual = othOperatingIntellectual & " + "	
					
					defIncome = defIncome & " + "
					
					stLoanLiability = stLoanLiability   & " + "
					stLoanAsset = stLoanAsset  & " + "
					ltLoanLiability = ltLoanLiability   & " + "
					ltLoanAsset = ltLoanAsset   & " + "
					
					stInterestLiability = stInterestLiability   & " + "
					stInterestAsset = stInterestAsset   & " + "
					ltInterestLiability  = ltInterestLiability   & " + "
					ltInterestAsset   = ltInterestAsset   & " + "
					
				End If
				
				' Personnel
				totalPersonnel = totalPersonnel & "RemoveZeros(E#" & miMember.Member.Name & ":A#Personnel_Total:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &":V#Periodic:O#Top:I#Top)"												
				totalPersonnelRF = totalPersonnelRF & "(RemoveZeros(E#" & miMember.Member.Name & ":A#Personnel_Total:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &":V#Periodic:O#Top:I#Top) - RemoveZeros(E#" & miMember.Member.Name & ":A#Personnel_Total:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesFcstNumber &":V#YTD:O#Top:I#Top))"												
				priorYearBPCell = priorYearBPCell & "RemoveZeros(E#" & miMember.Member.Name & ":A#Bonus_pay:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top)"																	
				menstotalPersonnel = menstotalPersonnel & "RemoveZeros(E#" & miMember.Member.Name & ":A#Personnel_Total:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top)" 
				
				' Interest	
				logistic = logistic & "E#" & miMember.Member.Name & ":A#554403:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top"
				interestPaid = interestPaid & "E#" & miMember.Member.Name & ":A#49122:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top"
				financialDebt = financialDebt & "E#" & miMember.Member.Name & ":A#49121:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top"	' Dato del mes			
				financialDebtYTD = financialDebtYTD & "E#" & miMember.Member.Name & ":A#49121:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M"& sMes &":V#YTD:O#Top:I#Top" ' Acumulado hasta ese mes								
				financialDebtYTDant = financialDebtYTDant & "E#" & miMember.Member.Name & ":A#49121:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes-3) &":V#YTD:O#Top:I#Top" ' Acumulado hasta el trimestre anterior
				financialIncome = financialIncome & "E#" & miMember.Member.Name & ":A#4911:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top"
				
				' IFRS16				
				interest = interest & "RemoveZeros(E#" & miMember.Member.Name & ":A#49124:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				
				' Payables TSA								
				
				subcontratingIng = subcontratingIng & "RemoveZeros(E#" & miMember.Member.Name & ":A#572109:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#551125:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top)"
				subcontratingIngYTD6 = subcontratingIngYTD6 & "RemoveZeros(E#" & miMember.Member.Name & ":A#572109:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M6:V#YTD:O#Top:I#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#551125:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M6:V#YTD:O#Top:I#Top)"
				subcontratingIngYTD12 = subcontratingIngYTD12 & "RemoveZeros(E#" & miMember.Member.Name & ":A#572109:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M12:V#YTD:O#Top:I#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#551125:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M12:V#YTD:O#Top:I#Top)"
				
				subcontratingIng_IC = subcontratingIng_IC & "RemoveZeros(E#" & miMember.Member.Name & ":A#572109:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#551125:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				subcontratingIngYTD6_IC = subcontratingIngYTD6_IC & "RemoveZeros(E#" & miMember.Member.Name & ":A#572109:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M6:V#YTD:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#551125:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M6:V#YTD:O#Top)"
				subcontratingIngYTD12_IC = subcontratingIngYTD12_IC & "RemoveZeros(E#" & miMember.Member.Name & ":A#572109:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M12:V#YTD:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#551125:F#None:U2#Top:U3#Top:U4#Top:T#"& year &"M12:V#YTD:O#Top)"
				
				' Warranty				
				warrantyOther = warrantyOther & "RemoveZeros(E#" & miMember.Member.Name & ":A#556000:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"									
				
				' Corp Rate			
				dataEBT = dataEBT & "RemoveZeros(E#" & miMember.Member.Name & ":A#EBT:F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"												
				data411 = data411 & "RemoveZeros(E#" & miMember.Member.Name & ":A#411:F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"												
				
				' AMORT
				intangAssets = intangAssets & "RemoveZeros(E#" & miMember.Member.Name & ":A#552406:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#557103:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#563109:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"													
				capitDevelop = capitDevelop & "RemoveZeros(E#" & miMember.Member.Name & ":A#572115:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"
				rightUse = rightUse & "RemoveZeros(E#" & miMember.Member.Name & ":A#563107G:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#5631071:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)+RemoveZeros(E#" & miMember.Member.Name & ":A#552307:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"
				'rightUse = rightUse & "RemoveZeros(E#" & miMember.Member.Name & ":A#563107G:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"
				sw = sw & "RemoveZeros(E#" & miMember.Member.Name & ":A#5631072:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"
				depreciation = depreciation & 	"RemoveZeros(E#" & miMember.Member.Name & ":A#55110590:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top) 
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#551107:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#551108:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top) 
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#552108:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top) 
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#49123:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top) 
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#551118:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#55110696:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#5621072:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)
												+ RemoveZeros(E#" & miMember.Member.Name & ":A#572107:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"
				
				provisions = provisions & "RemoveZeros(E#" & miMember.Member.Name & ":A#555103R:F#None:U2#Top:U3#Top:U4#Top:V#Periodic:O#Top)"
				
				' CapexRD				
				CapexRD114 = CapexRD114 & "RemoveZeros(E#" & miMember.Member.Name & ":A#572114:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"											
				CapexRD118 = CapexRD118 & "RemoveZeros(E#" & miMember.Member.Name & ":A#572118:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"

				' Other Financial
				othFinancial = othFinancial & "RemoveZeros(E#" & miMember.Member.Name & ":A#492:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
					
				' Other Operating
				othOperatingCash = othOperatingCash & "RemoveZeros(E#" & miMember.Member.Name & ":A#481:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#482:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)+ RemoveZeros(E#" & miMember.Member.Name & ":A#486:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#487:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#488:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#4890:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#4892:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#4895:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)+ RemoveZeros(E#" & miMember.Member.Name & ":A#4899:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				othOperatingProvision = othOperatingProvision & "RemoveZeros(E#" & miMember.Member.Name & ":A#483:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#484:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#489:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#4893:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top) + RemoveZeros(E#" & miMember.Member.Name & ":A#4896:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				othOperatingIndustrial = othOperatingIndustrial & "RemoveZeros(E#" & miMember.Member.Name & ":A#4897:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				othOperatingIntellectual = othOperatingIntellectual & "RemoveZeros(E#" & miMember.Member.Name & ":A#4898:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"	
				
				'Deferred Income
				defIncome = defIncome & "RemoveZeros(E#" & miMember.Member.Name & ":A#541141:F#None:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"	
				
				'Loans - Origenes
				stLoanLiability = stLoanLiability & "RemoveZeros(E#" & miMember.Member.Name & ":A#2561010C:I#Top:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				ltLoanLiability = ltLoanLiability  & "RemoveZeros(E#" & miMember.Member.Name & ":A#2141010C:I#Top:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
							

				'Intereses - Origenes
				stInterestLiability = stInterestLiability  & "RemoveZeros(E#" & miMember.Member.Name & ":A#2561020C:I#Top:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				ltInterestLiability  = ltInterestLiability  & "RemoveZeros(E#" & miMember.Member.Name & ":A#2141020C:I#Top:F#None:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top)"
				
				i = i + 1
			
			Next
			
			' ----------------------------------------------------------------------------------------------------------------
			'  PERSONNEL - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			'Scenario Forecast 
			If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
				
			' 1- Formato de los agregados
			totalPersonnel = "("& totalPersonnel &")"
			Dim mensPersonnel = "("& menstotalPersonnel &")"
			priorYearBPCell = "("& priorYearBPCell &")"
			Dim priorYearBonusPayment = "E#" & sEntityParent & "001:A#Bonus_pay:T#"& sTime &":F#None:O#Forms:U1#None:U2#None:U3#None:U4#Projections:I#None"
			
			' 2- Calculos Previos
			Dim payrollSSD = "("& totalPersonnel & " * (1-" & bonusProvision & ")) / " & salaryPayments	
			Dim extraP13 = payrollSSD & "/12"
			Dim extraP14 = payrollSSD & "/6"
			
			Dim bonusP = "(-1*("& totalPersonnel &"*"& bonusProvision & ") / 12)"

			Dim dif12 = mensPersonnel &"+( -"& payrollSSD &"+"& bonusP &")"
			Dim dif13 = mensPersonnel &"+( -"& payrollSSD &"+"& bonusP & "-" & extraP13 &")"
			Dim dif14 = mensPersonnel &"+( -"& payrollSSD &"+"& bonusP & "-" & extraP14 &")"

			' 3- Insertar valores
			If sEntity.EndsWith("001") Then

				' Caja - 1681000CF (Sueldo, pago del bonus py, diferencia o sobrante)
				' Paga Extra - 29018000			
				If left(salaryPayments,2) = "14" Then	
'					Dim valorDiferencia = api.Data.GetDataCell(dif14).CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim valorDiferencia As String = dif14
					
					If (sTime = year & "M6" Or sTime = year & "M12") Then
						
						' Caja - 1681000CF (Sueldo, Paga Extra, Bonus PY, Diferencia) 
						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "2*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
						
						' Bonus - 2901100 (Provisión de Bonus y Bonus PY) - (Provisión y Pago)
						api.Data.Calculate("E#"& sEntity &":A#2901100:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = "& bonusP &" - " & priorYearBPCell & " - " & extraP14 & " + 6*" & extraP14, True)
						
					Else
						
						' Caja - 1681000CF (Sueldo, Bonus PY, Diferencia) 
						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
						
						' Bonus - 2901100 (Provisión de Bonus y Bonus PY) - (Provisión)
						api.Data.Calculate("E#"& sEntity &":A#2901100:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & bonusP & " - " & priorYearBPCell & " - " & extraP14 , True)
						
					End If
					
				Else If left(salaryPayments,2) = "13" Then	
'					Dim valorDiferencia = api.Data.GetDataCell(dif13).CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim valorDiferencia As String = dif13
						
					If (sTime = year & "M12") Then
						
						' Caja - 1681000CF (Sueldo, Paga Extra, Bonus PY, Diferencia) 
						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "2*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)

						' Bonus - 2901100 (Provisión de Bonus y Bonus PY) -(Provisión y Pago)	
						api.Data.Calculate("E#"& sEntity &":A#2901100:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = "& bonusP &" - " & priorYearBPCell & " -" & extraP13 & " + 12*" & extraP13, True)
						
					Else
						
						' Caja - 1681000CF (Sueldo, Bonus PY, Diferencia) 
						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
						
						' Bonus - 2901100 (Provisión de Bonus y Bonus PY) - (Provisión)
						api.Data.Calculate("E#"& sEntity &":A#2901100:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = "& bonusP &" - " & priorYearBPCell & "-" & extraP13, True)
						
					End If
					
				Else If left(salaryPayments,2) = "12" Then
'					Dim valorDiferencia = api.Data.GetDataCell(dif12).CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim valorDiferencia As String = dif12
				
					' Caja - 1681000CF (Sueldo, Bonus PY, Diferencia) 

					api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)

					' MACS 2025/07/21 - OLD ---------------------------------------------------------------------------------------------------------------					
					' api.Data.Calculate("A#1681000CF:F#F30:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
					' --------------------------------------------------------------------------------------------------------------------------------------
					
					' Bonus - 2901100 (Provisión de Bonus y Bonus PY)
					api.Data.Calculate("E#"& sEntity &":A#2901100:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = "& bonusP &" - " & priorYearBPCell, True)
										
				End If
				
				api.Data.Calculate("U4#PERS:F#F99I = U4#PERS:F#F99" , True)	
			End If
			
			End If 

			' ----------------------------------------------------------------------------------------------------------------
			'  INTEREST - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------			

			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then

				' 1- Insertar datos
				If sTime = year & "M3" Then

					 api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import:I#None = -("& financialDebt &")", True)					 
					 api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F30:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import:I#None = ("& financialDebtYTD &")", True) 
'					 api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = ("& financialIncome &") + ("& interestPaid &")", True)
					 api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = (("& logistic &") + ("& financialDebtYTD &")) + (("& financialIncome &") + ("& interestPaid &"))", True)

				Else If sTime = year &"M6" Or sTime = year &"M9" Or sTime = year &"M12" Then
						
					 api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import:I#None = -("& financialDebt &")", True)
					 api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F30:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import:I#None = ("& financialDebtYTD &")-("& financialDebtYTDant &")", True)
'					 api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = ("& financialIncome &")+ ("& interestPaid &")", True)
					 api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = (("& logistic &") + ("& financialDebtYTD &")-("& financialDebtYTDant &")) + (("& financialIncome &")+ ("& interestPaid &"))", True)
					
				Else					
					 api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import:I#None = -("& financialDebt &")", True)		 
'					 api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = ("& financialIncome &")+ ("& interestPaid &")", True)
					 api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import:I#None = ("& logistic &") + (("& financialIncome &") + ("& interestPaid &"))", True)		
					 
				End If
				api.Data.Calculate("U4#INTER:F#F99I = U4#INTER:F#F99" , True)	
				
			End If
			
			' ----------------------------------------------------------------------------------------------------------------
			'  IFRS16 - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------			
			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
				
				' 1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#2531020C:F#F20:U2#None:U3#None:U4#IFRS16:T#"& sTime &":V#Periodic:O#Import = -1*("& interest &")", True)
				api.Data.Calculate("U4#IFRS16:F#F99I = U4#IFRS16:F#F99" , True)	
				
			End If
						
			' ----------------------------------------------------------------------------------------------------------------
			'  PAYABLES_TSA - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
				
				'1- Insertar datos
				api.Data.Calculate("A#2721020:F#F20:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = -("& subcontratingIng_IC &")", True)
				
				If sTime = year & "M6"				
					
					' Al vacíar la 2721020 (F30) no valía con sCalc2 aunque el dato e intersecciones estén bien
					' para solucionarlo 'vaciamos' con el dato acumulado (YTD) que tiene esa misma cuenta a Junio
					
					api.Data.Calculate("A#2721020:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#YTD:O#Import = ((" & subcontratingIngYTD6_IC &"))", True)
					'api.Data.Calculate("A#1681000CF:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#YTD:O#Import = -1*(A#2721020:F#F20:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":O#Import:V#YTD)", True)
					api.Data.Calculate("A#1681000CF:F#F16:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#YTD:O#Import:I#None = ((" & subcontratingIngYTD6 &"))", True)
				
				Else If sTime = year & "M12" Then
					
					api.Data.Calculate("A#2721020:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = (("& subcontratingIngYTD12_IC & ") - (" & subcontratingIngYTD6_IC &"))", True)
					api.Data.Calculate("A#1681000CF:F#F16:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import:I#None = (("& subcontratingIngYTD12 & ") - (" & subcontratingIngYTD6 &"))", True)
					
				End If	
				api.Data.Calculate("U4#DSOTSA:F#F99I = U4#DSOTSA:F#F99" , True)	
				
			End If
									
			' ----------------------------------------------------------------------------------------------------------------
			'  WARRANTY - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
		
			If sEntity.EndsWith("001")  And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
			
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#2061210:F#F25:U2#None:U3#None:U4#WRTY:T#"& sTime &":V#Periodic:O#Import = -1*( "& warrantyOther &")", True)
				api.Data.Calculate("U4#WRTY:F#F99I = U4#WRTY:F#F99" , True)	
				
			End If		
			
			' ----------------------------------------------------------------------------------------------------------------
			'  VAT - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001")  And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
				
				' Dato del Primer mes Cerrado
				Dim real29_YTD_Prior As Decimal = api.Data.GetDataCell("E#" & sEntity & ":A#2901710:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) &":V#YTD:O#Top:I#Top").CellAmount
				Dim real15_YTD_Prior As Decimal = api.Data.GetDataCell("E#" & sEntity & ":A#1561021:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) &":V#YTD:O#Top:I#Top").CellAmount

					' ' If sEntity = "R1300001" Then brapi.ErrorLog.LogMessage(si, $"{sMes}: {real29_YTD_Prior} - {real15_YTD_Prior} = {real29_YTD_Prior-real15_YTD_Prior}")
				If real29_YTD_Prior > real15_YTD_Prior Then
					' If sEntity = "R1300001" Then brapi.ErrorLog.LogMessage(si, $"{sMes}: PASO")
					' Vaciar 29
					api.Data.Calculate("E#" & sEntity & ":A#2901710:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& year &"M"& sMes &":V#Periodic:O#Import:I#None = 
						(-1) * E#" & sEntity & ":A#2901710:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) &":V#YTD:O#Top:I#Top
					", True)
					
					' Vaciar 15
					api.Data.Calculate("E#" & sEntity & ":A#1561021:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& year &"M"& sMes &":V#Periodic:O#Import:I#None = 
						(-1) * E#" & sEntity & ":A#1561021:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) &":V#YTD:O#Top:I#Top
					", True)
					
					
					' Caja 
					api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = 
						(-1) * E#" & sEntity & ":A#2901710:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) &":V#YTD:O#Top:I#Top
						+ E#" & sEntity & ":A#1561021:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) &":V#YTD:O#Top:I#Top
					", True)
						
				End If
				
				#Region "OLD Version"
				
'''''				Dim numberSeq() As Integer = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11} 
'''''				Dim mesInicio As Integer
'''''				'Dim mesFinal As Integer 
											
'''''					If( (MesFcstNumber - 1) = 0) Then 
						
'''''						mesInicio = 1
						
'''''					Else  
						
'''''						mesInicio = MesFcstNumber-1
						
'''''					End If
					
'''''					' ANALISIS 
'''''					' 	- Primer mes de cálculo - sMes = 4 
'''''					' 	- MesFcstNumber = 3
'''''					' 		Primer bucle -> mesInicio: 2 | mesFinal: 2
'''''					' 		Primer bucle -> mesInicio: 2 | mesFinal: 2
						
'''''					' Dato del Primer mes Cerrado
'''''					Dim real29 As String = api.Data.GetDataCell("E#" & sEntity & ":A#2901710:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesFcstNumber &":V#YTD:O#Top:I#Top").CellAmount.ToString(CultureInfo.InvariantCulture)
'''''					Dim real23 As String = api.Data.GetDataCell("E#" & sEntity & ":A#1561021:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesFcstNumber &":V#YTD:O#Top:I#Top").CellAmount.ToString(CultureInfo.InvariantCulture)
					
'''''					For Each mesFinal As Integer In numberSeq
					
'''''						If mesFinal >= mesInicio Then 							
					
'''''							' Datos calculados del WCAP
'''''							Dim suma29_ACT As String = "E#" & sEntity & ":A#2901710:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& mesFinal.ToString() &":V#YTD:O#Top:I#Top"
'''''							Dim suma29_LAST As String = "E#" & sEntity & ":A#2901710:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& mesInicio.ToString() &":V#YTD:O#Top:I#Top"
'''''							Dim suma29_PERIODIC As String = "E#" & sEntity & ":A#2901710:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& mesFinal.ToString() &":V#Periodic:O#Top:I#Top"
					
'''''							Dim suma23_ACT As String = "E#" & sEntity & ":A#1561021:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& mesFinal.ToString() &":V#YTD:O#Top:I#Top"
'''''							Dim suma23_LAST As String = "E#" & sEntity & ":A#1561021:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& mesInicio.ToString() &":V#YTD:O#Top:I#Top"
'''''							Dim suma23_PERIODIC As String = "E#" & sEntity & ":A#1561021:F#F99:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& mesInicio.ToString() &":V#Periodic:O#Top:I#Top"
							
'''''							Dim valor29 As String = String.Empty
'''''							Dim valor15 As String = String.Empty
'''''							Dim caja As String = String.Empty
'''''							Dim cajaMesAnterior As String = String.Empty									
									
'''''							If mesInicio = mesFinal And mesFinal = 1 Then
								
'''''								valor29 = suma29_ACT &" + "& real29
'''''								valor15 = suma23_ACT &" + "& real23
'''''								caja = "("& suma29_ACT &" - "& suma23_ACT &") + ("& real29 &" - "& real23 &")"
								
'''''							Else If  mesInicio = mesFinal And mesFinal > 1 Then
								
'''''								valor29 = suma29_PERIODIC
'''''								valor15 = suma23_PERIODIC
'''''								caja = "("& valor29 &"-"& valor15 &")"
					
'''''							Else If mesInicio <> mesFinal And mesInicio = 1 Then
											
'''''								valor29 = "("& suma29_ACT &" + "& real29 &")"
'''''								valor15 = "("& suma23_ACT &" + "& real23 &")"
'''''								caja = "("& valor29 &"-"& valor15 &")"
							
'''''							Else If mesInicio <> mesFinal And mesInicio <> 1 Then
											
'''''								valor29 = "("& suma29_ACT &"-"& suma29_LAST &")"
'''''								valor15 = "("& suma23_ACT &"-"& suma23_LAST &")"
'''''								caja = "("& valor29 &"-"& valor15 &")"
								
'''''							End If
						
''''''							If sEntity.Contains("R1301") And sMes = "4" Then											
''''''								BRAPI.ErrorLog.LogMessage(si, $"Mes Inicio {mesInicio} - Mes Final: {mesFinal}")
''''''								BRAPI.ErrorLog.LogMessage(si,sEntity &": Valor 29 - "& api.Data.GetDataCell(valor29).CellAmount.ToString(cultureInfo.InvariantCulture) &" | Valor 15 - "& api.Data.GetDataCell(valor15).CellAmount.ToString(cultureInfo.InvariantCulture))
''''''								BRAPI.ErrorLog.LogMessage(si,sEntity &": Valor 29 - "& valor29 &" | Valor 15 - "& valor15)
''''''								' BRAPI.ErrorLog.LogMessage(si,sEntity &": Real 29 - "& real29 &" | Real 15 - "& real23)
''''''								BRAPI.ErrorLog.LogMessage(si,sEntity &": Caja - "& api.Data.GetDataCell(caja).CellAmount.ToString(cultureInfo.InvariantCulture))
''''''							End If	
							
'''''							If (api.Data.GetDataCell(caja).CellAmount.ToString(cultureInfo.InvariantCulture).Contains("-")) = True Then
'''''								' Aquí el valor es negativo y no hacemos nada
					
'''''							Else If (api.Data.GetDataCell(caja).CellAmount.ToString(cultureInfo.InvariantCulture)) = "0" Then
								
'''''							Else
'''''								If mesFinal+1 = sMes Then
					
'''''									' Logica Para vaciar
'''''									api.Data.Calculate("E#"& sEntity &":A#2901710:F#F16:U1#None::U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = -1*("& valor29 &")", True)
'''''									api.Data.Calculate("E#"& sEntity &":A#1561021:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = -1*(" & valor15 &")", True)
'''''									api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = -1*("& caja &")", True)
						
'''''								Else
'''''									mesInicio = mesFinal + 1									
'''''								End If
'''''							End If
'''''						End If
					
'''''					Next
''' 
					#End Region
				
					
				API.Data.Calculate("U4#VAT:F#F99I = U4#VAT:F#F99" , True)
			End If
			
			' ----------------------------------------------------------------------------------------------------------------
			' CORP RATE - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			 If sEntity = "R0585001"
'				brapi.ErrorLog.LogMessage(si,sEntity.ToString)
'				brapi.ErrorLog.LogMessage(si,corpRate.ToString)
'				brapi.ErrorLog.LogMessage(si,api.Data.GetDataCell("A#EBT:E#"& sEntity &":F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:I#Top * (-1) * " & corpRate).CellAmount)
'				brapi.ErrorLog.LogMessage(si,api.Data.GetDataCell("A#EBT:E#"& sEntity &":C#EUR:F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:I#Top * (-1) * " & corpRate).CellAmount)
			End If
			
			
			api.Data.Calculate("E#"& sEntity &":A#4111:F#None:U1#None:U2#None:U3#None:U4#CORP:V#Periodic:O#Import:I#None = A#EBT:E#"& sEntity &":F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:I#Top * (-1) * " & corpRate, True)
			'api.Data.Calculate("E#"& sEntity &":A#4111:F#None:U1#None:U2#None:U3#None:U4#CORP:V#Periodic:O#Import:I#None =  " & corpRate, True)
			
			' ----------------------------------------------------------------------------------------------------------------
			'  AMORT - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
				If api.Cons.IsLocalCurrencyForEntity() Then
					api.Data.ClearCalculatedData("U4#AMORT",True,True,True,True)
					
					' 1009904 - Intangible Assets Amortization (Suma de 55406, 572107)
					' api.Data.ClearCalculatedData("A#1009904:F#F25:O#Import:U2#None:U3#None:U4#AMORT",True,True,True,True)
					
					api.Data.Calculate("A#1009904:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & intangAssets &")", True)
				
					' 1021004C - Capitalised Development Expenses Amortization (Suma de 572115)
					' api.Data.ClearCalculatedData("A#1021004C:F#F25:O#Import:U2#None:U3#None:U4#AMORT",True,True,True,True)   
					
					api.Data.Calculate("A#1021004C:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & capitDevelop &")", True)
					
					' 1039104 - Right of Use of Leased Assets Amortization (Suma de 552307, 563107) -
					' api.Data.ClearCalculatedData("A#1039104:F#F25:O#Import",True,True,True,True)
					
					api.Data.Calculate("A#1039104:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & rightUse &")", True)
					
					' 1009204 - Software Amortization (Suma de  5631072) -
					api.Data.ClearCalculatedData("A#1009204:F#F25:O#Import",True,True,True,True)
					
					api.Data.Calculate("A#1009204:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & sw &")", True)
					
					' 1041004C - Depreciation (Suma de 55110590, 551107, 551108, 552108, 49123, 572107) -	 y 
					' api.Data.ClearCalculatedData("A#1041004C:F#F25:U2#None:U3#None:U4#AMORT",True,True,True,True)
					api.Data.Calculate("A#1041004CF:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & depreciation &")", True)
			
					' 1501004C - Provisions/Depreciation of Inventories (Suma de 555103R) 
					' ' api.Data.ClearCalculatedData("A#1501004C:F#F25:U2#None:U3#None:U4#AMORT",True,True,True,True)
					
					api.Data.Calculate("A#1501004CF:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & provisions &")", True)
					
					api.Data.Calculate("U4#AMORT:F#F99I = U4#AMORT:F#F99" , True)
				End If	
			End If
				
			' ----------------------------------------------------------------------------------------------------------------
			'  CAPEXRD - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
			
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#1021020C:F#F20:U2#None:U3#None:U4#CAPEXRD:T#"& sTime &":V#Periodic:O#Import = ( "& CapexRD114 &")", True)
				api.Data.Calculate("E#"& sEntity &":A#1049300:F#F20:U2#None:U3#None:U4#CAPEXRD:T#"& sTime &":V#Periodic:O#Import = ( "& CapexRD118 &")", True)
				
				api.Data.Calculate("U4#CAPEXRD:F#F99I = U4#CAPEXRD:F#F99" , True)	
				
			End If

			' ----------------------------------------------------------------------------------------------------------------
			'  OTHER FINANCIAL - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
			
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#OTHFIN:I#None:T#"& sTime &":V#Periodic:O#Import = ( "& othFinancial &")", True)
				
				api.Data.Calculate("U4#OTHFIN:F#F99I = U4#OTHFIN:F#F99" , True)	
				
			End If	
			
			' ----------------------------------------------------------------------------------------------------------------
			'  OTHER OPERATING - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
			
'				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:I#None:O#Import = ( "& othOperatingCash &")", True)
				api.Data.Calculate("E#"& sEntity &":A#2061430:F#F25:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = (-1)*( "& othOperatingProvision &")", True)
				api.Data.Calculate("E#"& sEntity &":A#1049314:F#F25:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = ( "& othOperatingIndustrial &")", True)
				api.Data.Calculate("E#"& sEntity &":A#1009404:F#F25:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = ( "& othOperatingIntellectual &")", True)

				api.Data.Calculate("U4#OTHOP:F#F99I = U4#OTHOP:F#F99" , True)	
				
			
			End If	
			
			' ----------------------------------------------------------------------------------------------------------------
			'  DEFERRED INCOME - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			
			If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
	
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#2801600:F#F30:U1#None:U2#None:U3#None:U4#DEFINCOME:I#None:T#"& sTime &":V#Periodic:O#Import = (-1) * ("& defIncome &")", True)
				api.Data.Calculate("U4#DEFINCOME:F#F99I = U4#DEFINCOME:F#F99" , True)	
			
			End If
			

		End Sub
	#End Region	
	
	#Region "Loans"

			' ----------------------------------------------------------------------------------------------------------------
			'  LOANS - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			Sub Loans (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
				Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
				Dim sScenario As String = api.Pov.Scenario.Name
				Dim sTime As String = api.Pov.Time.Name 
				Dim year As String = sTime.Substring(0,4)
				Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))	
				Dim sEntity As String = api.pov.Entity.Name
				Dim sEntityParent As String = Left(sEntity,5)			
				Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
						
				If sEntity.EndsWith("R1300001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber ) Then
	
						Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
						
						Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
						Dim sEntityPOV As String = api.Pov.Entity.Name	
						Dim sMesAnterior As String = 1
					
						If(sMes = 1) Then 
					
							sMesAnterior = 1
					
						Else If (ScenarioType = "Forecast" And sMes > 1) Then  
					
							sMesAnterior = sMes - 1
					
						End If
						
	'					Dim sMesAnterior As String = sMes - 1
						
						' Recuperamos la moneda de la entidad principal y la moneda común
						Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
						Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
						Dim commonCurrencyID As Integer = Currency.EUR.Id
						
						' Definimos variables necesarias para el conseguir el tipo de cambio
						Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
						Dim cubeId As Integer = api.Pov.Cube.CubeId
						Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	
						For Each ICMember As Member In ICList
							
							Dim sIC As String = ICMember.Name.ToString
							
						' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
							Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
							Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
							Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)
							
							Dim difMesCP = api.Data.GetDataCell("(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2561010C:O#Top:I#R1300001:F#F99:U4#Top)-(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2561010C:O#Top:I#R1300001:F#F99:U4#Top)").CellAmount
							Dim difMesLP = api.Data.GetDataCell("(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2141010C:O#Top:I#R1300001:F#F99:U4#Top)-(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2141010C:O#Top:I#R1300001:F#F99:U4#Top)").CellAmount
							Dim difMesCPInt = api.Data.GetDataCell("(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2561020C:O#Top:I#R1300001:F#F99:U4#Top)-(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2561010C:O#Top:I#R1300001:F#F99:U4#Top)").CellAmount
							Dim difMesLPInt = api.Data.GetDataCell("(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2141020C:O#Top:I#R1300001:F#F99:U4#Top)-(E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2141010C:O#Top:I#R1300001:F#F99:U4#Top)").CellAmount
							
							'LOANS CORTO PLAZO
						'	If(difMesCP <> 0 ) Then
								api.Data.Calculate(
									"E#R1300001:C#" & entityPOVCurrencyName & ":V#Periodic:A#1641000C:O#Import:F#F16:I#" & sIC & ":U4#LOANS = " &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2561010C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",",".")  & "-" &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2561010C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",","."), True)
	 
						'	End If
							'LOANS LARGO PLAZO
						'	If(difMesLP <> 0 ) Then
								api.Data.Calculate(
									"E#R1300001:C#" & entityPOVCurrencyName & ":V#Periodic:A#1221000C:O#Import:F#F20:I#" & sIC & ":U4#LOANS = " &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2141010C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",",".")  & "-" &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2141010C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",","."), True)
						'	End If
							'INTERESES CORTO PLAZO
						'	If(difMesCPInt <> 0 ) Then
								api.Data.Calculate(
									"E#R1300001:C#" & entityPOVCurrencyName & ":V#Periodic:A#1641020C:O#Import:F#F16:I#" & sIC & ":U4#LOANS = " &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2561020C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",",".")  & "-" &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2561020C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",","."), True)
	 
						'	End If
							'INTERESES LARGO PLAZO
						'	If(difMesLPInt <> 0 ) Then
								api.Data.Calculate(
									"E#R1300001:C#" & entityPOVCurrencyName & ":V#Periodic:A#1221020C:O#Import:F#F20:I#" & sIC & ":U4#LOANS = " &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2141020C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",",".")  & "-" &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2141020C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",","."), True)
							'End If
							'CAJA
								api.Data.Calculate(
									"E#R1300001:V#Periodic:C#" & entityPOVCurrencyName & ":A#1681000CF:O#Import:F#F16:I#None:U4#LOANS = " &
									"E#R1300001:V#Periodic:A#1641000C:O#Import:F#F16:I#Top:U4#LOANS * (-1) +" &
									"E#R1300001:V#Periodic:A#1221000C:O#Import:F#F20:I#Top:U4#LOANS * (-1) +" &
									"E#R1300001:V#Periodic:A#1641020C:O#Import:F#F16:I#Top:U4#LOANS * (-1) +" &
									"E#R1300001:V#Periodic:A#1221020C:O#Import:F#F20:I#Top:U4#LOANS * (-1)", True)	
						Next
							
							api.Data.Calculate("U4#OTHOP:F#F99I = U4#LOANS:F#F99" , True)
				End If
		End Sub

	#End Region
	
	#Region "Royaties"
			' ----------------------------------------------------------------------------------------------------------------
			'  Royalties - CALCULATIONS - SUB
			' ----------------------------------------------------------------------------------------------------------------
			
			Sub Royalties (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
				Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
				Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
				Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
				
				api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,"U4#ROYAL",,,,)
				
				
			Dim EntityList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
			Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
			
			Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
			Dim sEntityPOV As String = api.Pov.Entity.Name	
			Dim sTime As String = api.Pov.Time.Name					
			
			' Recuperamos la moneda de la entidad principal y la moneda común
			Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
			Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
			Dim commonCurrencyID As Integer = Currency.EUR.Id
			
			' Definimos variables necesarias para el conseguir el tipo de cambio
			Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
			Dim cubeId As Integer = api.Pov.Cube.CubeId
			Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
			
			' Recorremos el listado de empresas para comprobar si alguna tiene es ICP contra la entity que se pasa en el POV
			For Each ICMember As Member In ICList
				
				Dim sIC As String = ICMember.Name.ToString

				' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
				Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
				Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
				Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)
			

				Dim ReveneuICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#552209:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)

				' Calculamos
				' If (ReveneuICPAmount <> 0)	
				If sEntityPOV = "R1300002"	
'					If sIC = "R0585001" Then brapi.ErrorLog.LogMessage(si, $"{sTime} -> tasa: {revRate}, cantidad: {ReveneuICPAmount}" )
						
					api.Data.Calculate(
						"E#R1300002:C#" & entityPOVCurrencyName & ":V#YTD:A#541114:O#Import:I#" & sIC & ":U1#Horse_Generic:U2#None:U3#None:U4#ROYAL = " &
						"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#552209:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Projections * (-1) * " & revRate.ToString.Replace(",","."), True
						)
						
				End If	
			Next
 
'				If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
					
'					api.Data.ClearCalculatedData("U4#ROYAL", True, True, True, True)
					

'					Dim UD1MemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#Top.base", True)
'					Dim ICMemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "IC", "I#Top.base", True)
'					Dim UD2MemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Product", "U2#Top.base", True)
					
'					Dim sEntityMemberList() As String = {"R1301002", "R1301003", "R1302001", "R1303001", "R0611002", "R0611003", "R0671001", "R0592001", "R0592009", "R0585001"}
'					Dim UD1Renault_Children As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#Renault_Grou.base", True)				
'					api.Data.ClearCalculatedData("A#552209:U4#ROYAL", True, True, True, True)
					
''					' ------- PRIMERA PARTE -------					
''					Dim sEntityMember As String = api.Pov.Entity.Name
''					Dim sEntityMember_001 As String = api.Pov.Entity.Name.Substring(0,5) & "001"
					
''					For Each UD1Member As MemberInfo In UD1MemberList
						
''					   	Dim Royalties_paid As String = String.Empty
''						Dim Royalties_pct As String = String.Empty
''						Dim percentage As String = String.Empty
''						Dim sumaICs As String = String.Empty
							
''						percentage = api.Data.GetDataCell("E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U3#None:U4#Manual_Input:O#Top").CellAmount.ToString(cultureInfo.InvariantCulture)
						
''						For Each UD2Member As MemberInfo In UD2MemberList ' 
							
''							sumaICs = String.Empty
''							Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#" & UD1Member.Member.Name & ":U2#Top:U3#None:U4#Top:O#Top)").CellAmount			
							
''							' Si hay valor en Top UD2 lo evalua si no pasa a la sigueinte UD1 - Para hacerlo más rápido
''							If (Royalties_paid <> 0) Then
								
''								Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U3#None:U4#Top:O#Top)").CellAmount
''								Royalties_pct = api.Data.GetDataCell("(E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U4#Manual_Input:O#Top)").CellAmount							
								
''					        	For Each ICMember As MemberInfo In ICMemberList
''									If sumaICs = String.Empty Then
''										sumaICs = $"E#{sEntityMember}:A#541107:I#{ICMember.Member.Name}:U1#{UD1Member.Member.Name}:U2#{UD2Member.Member.Name}:U3#Top:U4#Top:O#Top"
''									Else
''										sumaICs = $"{sumaICs} + E#{sEntityMember}:A#541107:I#{ICMember.Member.Name}:U1#{UD1Member.Member.Name}:U2#{UD2Member.Member.Name}:U3#Top:U4#Top:O#Top"
''									End If
''								Next
								
''								' Para cada cruce UD1 - UD2 suma todos los valores de las ICS y se llevan a UD4#ROYAL  I#R1300002
'''								api.Data.Calculate("E#" & sEntityMember & ":A#552209:I#R1300002:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U3#None:U4#ROYAL:O#Import = 
'''													("& sumaICs &")*" & percentage & "* (-1)", True)
								
''							End If
''			       		 Next
''				 	 Next
					 
'					' ------- SEGUNDA PARTE -------					
'					 ' La ICP la obtenemos de la parte de ventas
			
'					Dim EntityList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
'					Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
					
'					Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
'					Dim sEntityPOV As String = api.Pov.Entity.Name	
'					Dim sTime As String = api.Pov.Time.Name		
					
					
'					' Recuperamos la moneda de la entidad principal y la moneda común
'					Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
'					'Dim entityPOVCurrencyName As String = "EUR"
'					Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
'					Dim commonCurrencyID As Integer = Currency.EUR.Id
					
'					' Definimos variables necesarias para el conseguir el tipo de cambio
'					Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
'					Dim cubeId As Integer = api.Pov.Cube.CubeId
'					Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
					
'					' Recorremos el listado de empresas para comprobar si alguna tiene es ICP contra la entity que se pasa en el POV
'					For Each ICMember As Member In ICList
						
'						Dim sIC As String = ICMember.Name.ToString
		
'						' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
'						Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
'						Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
'						Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)							
'						Dim ReveneuICPAmount As String = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#552209:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
									
'						' Calculamos
'						'(ReveneuICPAmount <> 0) And
'						If sEntityPOV = "R1300002"	And entityPOVCurrencyName="EUR"
'							'BRApi.ErrorLog.LogMessage(si, "Cruce: " + sTime + " " + entityPOVCurrencyName + " " + sIC )
'							'BRApi.ErrorLog.LogMessage(si, "1")
'							api.Data.Calculate(
'								"E#R1300002:C#" & entityPOVCurrencyName & ":V#YTD:A#541114:O#Import:I#" & sIC & ":U1#Horse_Generic:U2#None:U3#None:U4#ROYAL = " &
'								"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#552209:O#Top:I#R1300002:U1#Top:U2#Top:U3#Top:U4#Projections * (-1) * " & revRate.ToString.Replace(",","."), True)
								
'						End If	
						
						
					
					'Next
				
					#Region "Versión 1"
''''				   For Each UD1Member As MemberInfo In UD1MemberList ' UD1 - Hola
					   
''''				        For Each ICMember As MemberInfo In ICMemberList
							
''''							Dim Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#" & UD1Member.Member.Name & ":U2#Top:U3#None:U4#Top:O#Top)").CellAmount
''''							' Dim Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#" & UD1Member.Member.Name & ":U2#Top:U4#Top:O#Top)").CellAmount
							
''''							If (Royalties_paid <> 0) Then
								
''''								For Each UD2Member As MemberInfo In UD2MemberList ' 
									
''''									Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U3#None:U4#Top:O#Top)").CellAmount
									
''''									Dim Royalties_pct = api.Data.GetDataCell("(E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U4#None:O#Top)").CellAmount

''''	'									If (Royalties_paid <> 0) And (Royalties_pct <> 0) Then
	
''''										api.Data.Calculate("E#" & sEntityMember & ":A#552209:I#R1300002:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U3#None:U4#ROYAL:O#Import = (E#" & sEntityMember & ":A#541107:I#" & ICMember.Member.Name & ":U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U3#None:U4#Top:O#Top) * (E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U3#None:U4#None:O#Top) * (-1)", True)
''''							        	'api.Data.Calculate("E#" & sEntityMember & ":A#552209:I#R1300002:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U4#ROYAL:O#Import = (E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U4#Top:O#Top)", True)
									
''''	'									End If
''''								Next			
''''							End If
''''			       		 Next
''''				 	 Next
				
				
'''''				' La ICP la obtenemos de la parte de ventas
			
'''''				Dim EntityList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
'''''				Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
				
'''''				Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
'''''				Dim sEntityPOV As String = api.Pov.Entity.Name	
'''''				Dim sTime As String = api.Pov.Time.Name		
				
				
'''''				' Recuperamos la moneda de la entidad principal y la moneda común
'''''				Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
'''''				Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
'''''				Dim commonCurrencyID As Integer = Currency.EUR.Id
				
'''''				' Definimos variables necesarias para el conseguir el tipo de cambio
'''''				Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
'''''				Dim cubeId As Integer = api.Pov.Cube.CubeId
'''''				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
				
'''''				' Recorremos el listado de empresas para comprobar si alguna tiene es ICP contra la entity que se pasa en el POV
'''''				For Each ICMember As Member In ICList
					
'''''					Dim sIC As String = ICMember.Name.ToString
	
'''''					' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
'''''					Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
'''''					Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
'''''					Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)
					
'''''	'					BRAPI.ErrorLog.LogMessage(si, $"{entityICCurrencyName} to {entityPOVCurrencyName}: {revRate}")
						
'''''					Dim ReveneuICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#552209:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
								
'''''					' Calculamos
'''''					If (ReveneuICPAmount <> 0)					
'''''						api.Data.Calculate(
'''''							"E#R1300002:C#" & entityPOVCurrencyName & ":V#YTD:A#541114:O#Import:I#" & sIC & ":U1#Horse_Generic:U2#None:U3#None:U4#ROYAL = " &
'''''							"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#552209:O#Top:I#R1300002:U1#Top:U2#Top:U3#None:U4#ROYAL * (-1) * " & revRate.ToString.Replace(",","."), True)
'''''					End If	
					
'''''				Next

		#End Region

 
			'End If
		End Sub
	#End Region
 
	#Region "Royaties OLD"
	
''''#Region "Royaties"
''''			' ----------------------------------------------------------------------------------------------------------------
''''			'  Royalties - CALCULATIONS - SUB
''''			' ----------------------------------------------------------------------------------------------------------------
			
''''			Sub Royalties (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
''''			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
''''			Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
''''			Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
			
''''			If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then			
''''				api.Data.ClearCalculatedData("U4#ROYAL", True, True, True, True)
''''				'Dim sEntityMemberList() As String = {"R1301002", "R1301003", "R1302001", "R1303001", "R0611002", "R0611003", "R0671001", "R0592001", "R0592009", "R0585001"}
''''				Dim UD1MemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#Top.base", True)
''''				'Dim ICMemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "IC", "I#Top.base", True)
'''''				Dim UD1Renault_Children As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#Renault_Grou.base", True)
''''				Dim UD2MemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Product", "U2#Top.base", True)
				
			
''''				'api.Data.ClearCalculatedData("A#552209:U4#ROYAL", True, True, True, True)
								
''''				Dim sEntityMember As String = api.Pov.Entity.Name
''''				Dim sEntityMember_001 As String = api.Pov.Entity.Name.Substring(0,5) & "001"
				
'''''				   For Each UD1Member As MemberInfo In UD1MemberList
'''''				       For Each ICMember As MemberInfo In ICMemberList
'''''							Dim Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#" & UD1Member.Member.Name & ":U2#Top:U4#Top:O#Top)").CellAmount
'''''							Dim Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#Top:U2#Top:U3#None:U4#Top:O#Top)").CellAmount
'''''							If (Royalties_paid <> 0) Then
'''''								For Each UD2Member As MemberInfo In UD2MemberList
'''''									Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#Top:U2#Top:U3#None:U4#Top:O#Top)").CellAmount
'''''									Dim Royalties_pct = api.Data.GetDataCell("(E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U3#None:U4#None:O#Top)").CellAmount
									
''''''									If (Royalties_paid <> 0) And (Royalties_pct <> 0) Then

'''''							            api.Data.Calculate("E#" & sEntityMember & ":A#552209:I#R1300002:U1#Top:U2#Top:U3#None:U4#ROYAL:O#Import = (E#" & sEntityMember & ":A#541107:I#Top:U1#Top:U2#Top:U3#None:U4#Top:O#Top) * (E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U4#None:O#Top) * (-1)", True)
'''''							        	'api.Data.Calculate("E#" & sEntityMember & ":A#552209:I#R1300002:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U4#ROYAL:O#Import = (E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U4#Top:O#Top)", True)
									
''''''									End If
'''''								Next			
'''''							End If
'''''			       		 Next
'''''				  Next
				
				
''''				' La ICP la obtenemos de la parte de ventas
			
''''				Dim EntityList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
''''				Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
				
''''				Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
''''				Dim sEntityPOV As String = api.Pov.Entity.Name	
''''				Dim sTime As String = api.Pov.Time.Name		
				
				
''''				' Recuperamos la moneda de la entidad principal y la moneda común
''''				Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
''''				Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
''''				Dim commonCurrencyID As Integer = Currency.EUR.Id
				
''''				' Definimos variables necesarias para el conseguir el tipo de cambio
''''				Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
''''				Dim cubeId As Integer = api.Pov.Cube.CubeId
''''				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
				
''''				' Recorremos el listado de empresas para comprobar si alguna tiene es ICP contra la entity que se pasa en el POV
''''				For Each ICMember As Member In ICList
					
''''					Dim sIC As String = ICMember.Name.ToString
	
''''					' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
''''					Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
''''					Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
''''					Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)
					
''''	'					BRAPI.ErrorLog.LogMessage(si, $"{entityICCurrencyName} to {entityPOVCurrencyName}: {revRate}")
						
''''					Dim ReveneuICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#552209:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U3#None:U4#Top:U5#Top:U6#Top:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
								
''''					' Calculamos
''''					If (ReveneuICPAmount <> 0)					
''''						api.Data.Calculate(
''''							"E#R1300002:C#" & entityPOVCurrencyName & ":V#YTD:A#541114:O#Import:I#" & sIC & ":U1#Horse_Generic:U2#None:U3#None:U4#ROYAL = " &
''''							"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#552209:O#Top:I#R1300002:U1#Top:U2#Top:U3#None:U4#ROYAL * (-1) * " & revRate.ToString.Replace(",","."), True)
''''					End If	
					
''''				Next
''''			End If
''''		End Sub
''''	#End Region
''' 
	#End Region
	
	#Region "CorpRate 2"
		' ----------------------------------------------------------------------------------------------------------------
		'  CORPRATE_2 - CALCULATIONS - SUB
		' ----------------------------------------------------------------------------------------------------------------
			
		Sub othersCorp (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
			' ----------------------------------------------------------------------------------------------------------------
			' VARIABLES
			' ----------------------------------------------------------------------------------------------------------------
			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
			Dim sScenario As String = api.Pov.Scenario.Name
			Dim sTime As String = api.Pov.Time.Name 
			Dim year As String = sTime.Substring(0,4)
			Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))	
			Dim sEntity As String = api.pov.Entity.Name
			Dim sEntityParent As String = Left(sEntity,5)			
			Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
			
			If (ScenarioType = "Forecast" And sMes >= MesFcstNumber) Then
'				api.Data.ClearCalculatedData("U4#CORP:A#2901720",True,True,True,True)	
				
'				' Corp Rate
				Dim data411 As String = String.Empty
				Dim i As Integer = 0
				
				For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
				
					If i <> 0 Then
						data411 = data411 & " + "
					End If
					
					' Corp Rate														
					data411 = data411 & "RemoveZeros(E#" & miMember.Member.Name & ":A#411:F#None:U1#Top:U2#Top:U3#Top:U4#Top:T#"& sTime &":V#Periodic:O#Top:I#Top)"												
	
					i = i + 1
					
'					Dim Value_411 As Integer = api.Data.GetDataCell(data411).CellAmount.ToString(cultureInfo.InvariantCulture)
'					BRAPI.ErrorLog.LogMessage(si, Value_411)
					
				Next
				
					
				
				If sEntity.EndsWith("001") And (ScenarioType = "Budget" Or (ScenarioType = "Forecast" And sMes > MesFcstNumber)) Then				
					api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CORP:T#"& sTime &":V#Periodic:O#Import:I#None = (-1)*(" & data411 &")", True)
					api.Data.Calculate("U4#CORP:F#F99I = U4#CORP:F#F99" , True)	
				End If
			End If
			
		End Sub
	#End Region		
	
#End Region

#Region "New Calculations"
			' ----------------------------------------------------------------------------------------------------------------
			'  New Calculations - CALCULATIONS - SUB
			' ----------------------------------------------------------------------------------------------------------------
			
			Sub Engineering (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			
				Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
				Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
				Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
				
				If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
					
					api.Data.ClearCalculatedData("U4#ENG", True, True, True, True)
					
					' Listado de cuentas origen y destino según la Entidad sIC de la que se recogerá el dato
					' CuentaOrigen: CuentasDestino [Holding 1, Holding 2, Paises]
					Dim accountsList As New Dictionary (Of String, List(Of String)) From {	
						{"5631085"	,	New List(Of String) From {"56310888", "5631086", "5631086"}},	' Administrative Service Fees / Administrative Global Functions
			            {"5621087"	, 	New List(Of String) From {"56210888", "5621084", "5621084"}},	' Sales Service Fees / Sales Global Functions
			            {"552308"	, 	New List(Of String) From {"552308",	"552308", "552308"}}, 		' Industrial Performance Service Fees / Industrial Performance Global Functions
			            {"554408F"	, 	New List(Of String) From {"554408F", "554408F", "554408F"}}, 	' Supply Chain Service Fees / Supply Chain Global Functions
			            {"5541098"	, 	New List(Of String) From {"5541098", "5541098", "5541098"}}, 	' Quality Service Fees / Quality Global Functions
			            {"541109"	, 	New List(Of String) From {"572120", "572120", "572120"}}, 		' Producto: Recharge To HPS / Proceso: Recharge To others / Proceso: Recharge To countries
			            {"5511253"	, 	New List(Of String) From {"5511253", "5511253", "5511253"}}, 	' ISIT - Manuf
			            {"572105"	, 	New List(Of String) From {"572105", "572105", "572105"}}, 		' ISIT - R&D -Producto / ISIT - R&D -Proceso
			            {"485"		, 	New List(Of String) From {"485", "485", "485"}}	 				' APCE - SAP Project
					}
						
					Dim orgnAccount As String = String.Empty
					Dim destAccount As String = String.Empty
					Dim numAccount As Integer = Nothing
					
					' ------- Calculo IPC -------					
					 ' La ICP la obtenemos de la parte de ventas
			
					Dim EntityList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
					Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
					
					Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
					Dim sEntityPOV As String = api.Pov.Entity.Name	
					Dim sTime As String = api.Pov.Time.Name		
					
					
					' Recuperamos la moneda de la entidad principal y la moneda común
					Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
					Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
					Dim commonCurrencyID As Integer = Currency.EUR.Id
					
					' Definimos variables necesarias para el conseguir el tipo de cambio
					Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
					Dim cubeId As Integer = api.Pov.Cube.CubeId
					Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
					
					' Recorremos el listado de empresas para comprobar si alguna tiene es ICP contra la entity que se pasa en el POV
					For Each ICMember As Member In ICList
						
						Dim sIC As String = ICMember.Name.ToString
		
						' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
						Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
						Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
						Dim revRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)							
						
						For Each orgnAccount In accountsList.Keys
							
							numAccount = If(sIC = "R1300001", 0, If(sIC = "R1300002", 1, 2))
							destAccount = accountsList(orgnAccount)(numAccount)
							
							api.Data.Calculate(
								"E#" & sEntityPOV & ":C#" & entityPOVCurrencyName & ":V#YTD:A#"& destAccount &":O#Import:I#"& sIC &":U4#ENG = " &
							 	"E#"& sIC &":C#" & entityICCurrencyName & ":V#YTD:A#"& orgnAccount &":O#Top:I#" & sEntityPOV & ":U4#Manual_Input * (-1) * " & revRate.ToString.Replace(",","."), True)
							
						Next
						
					Next		
					
			End If
		End Sub
	#End Region
	
#Region "Change in CIT Automatism"
	Sub CIT_Automatism (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
			' ----------------------------------------------------------------------------------------------------------------
			' VARIABLES
			' ----------------------------------------------------------------------------------------------------------------
			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
			Dim sScenario As String = api.Pov.Scenario.Name
			Dim sTime As String = api.Pov.Time.Name 
			Dim year As String = sTime.Substring(0,4)
			Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))	
			Dim sEntity As String = api.pov.Entity.Name
			Dim sEntityParent As String = Left(sEntity,5)			
			Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods 'ÚLTIMO MES DE ACTUAL
		
			
			#Region "Chile"
			
			If sEntityParent = "R0585"
				
				If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
	'				api.Data.ClearCalculatedData("U4#CORP:A#2901720",True,True,True,True)	
					

					Dim data541000 As String = String.Empty
					Dim i As Integer = 0
					
					For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
					
						If i <> 0 Then
							data541000 = data541000 & " + "
						End If
						
						If sMes = MesFcstNumber + 1
							data541000 = data541000 & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#541000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) & ":V#Periodic:O#Top:I#Top)"
						Else
							data541000 = data541000 & "RemoveZeros(E#" & miMember.Member.Name & ":A#541000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& (sMes -1) & ":V#Periodic:O#Top:I#Top)"
						End If
							
						i = i + 1
						
	'					Dim Value_411 As Integer = api.Data.GetDataCell(data411).CellAmount.ToString(cultureInfo.InvariantCulture)
	'					BRAPI.ErrorLog.LogMessage(si, Value_411)
						
					Next
					
					Dim data1561040 As String = String.Empty
					data1561040 = "RemoveZeros(E#" & sEntityParent & "001:A#1561040:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M1:V#YTD:O#Top:I#Top)"
					
					Dim Value_541000 As Integer = api.Data.GetDataCell(data541000).CellAmount
					Dim Value_1561040 As Integer = api.Data.GetDataCell(data1561040).CellAmount
					Dim CIT_Percentage As String = String.Empty
					

					
					If Value_541000 >= 0 
						
						CIT_Percentage = "RemoveZeros(E#" & sEntity & ":A#CIT:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:T#"& year &"M"& sMes & ":V#Periodic:O#BeforeAdj:I#Top)"

						If sEntity.EndsWith("001") And (ScenarioType = "Budget" Or (ScenarioType = "Forecast" And sMes > MesFcstNumber)) Then
							api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = (-1)*(" & data541000 &")*(" & CIT_Percentage & ")", True)
							api.Data.Calculate("E#"& sEntityParent &"001:A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = (" & data541000 &")*(" & CIT_Percentage & ")", True)

						End If
					Else
						If sEntity.EndsWith("001") And (ScenarioType = "Budget" Or (ScenarioType = "Forecast" And sMes > MesFcstNumber)) Then
							api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = (0)*(" & data541000 &")*(" & CIT_Percentage & ")", True)
							api.Data.Calculate("E#"& sEntityParent &"001:A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = (0)*(" & data541000 &")*(" & CIT_Percentage & ")", True)
						End If
					End If
					
					If sMes = 4 
						If Value_1561040 >=0  
							
							api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:T#"& sTime &":V#YTD:O#Import:I#None = (-1)*(" & data1561040 &")", True)
							api.Data.Calculate("E#"& sEntityParent &"001:A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CITPY:T#"& sTime &":V#YTD:O#Import:I#None = (-1)*(" & data1561040 &")", True)
							
						End If
							
					End If
					
				End If
				
				
				
			End If	
			#End Region
			
			#Region "Argentina"
			If sEntityParent = "R0592"
				
				If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
						
						'brapi.ErrorLog.LogMessage(si,"Entra")
						
						Dim dataEBT As String = String.Empty
						Dim dataEBT_LY As String = String.Empty
						Dim data4111 As String = String.Empty
						Dim CIT_Percentage_Arg As String = String.Empty
						Dim Value_EBT As Integer = 0
						Dim EBTtest As String = String.Empty
						Dim data1561040 As String = String.Empty
						Dim i As Integer = 0
						
						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
					
							If i <> 0 Then
								data4111 = data4111 & " + "
								dataEBT = dataEBT & " + "
								dataEBT_LY = dataEBT_LY & " + "
							End If
							
							If sMes <=4
								data4111 = data4111 & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#4111:C#Local:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year-2) &"M12:V#YTD:O#Top:I#Top)"
								dataEBT = dataEBT & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:C#Local:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year-2) &"M12:V#YTD:O#Top:I#Top)"
							
							Else If sMes >= 7
								data4111 = data4111 & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#4111:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year-1) &"M12:V#YTD:O#Top:I#Top)"
								dataEBT = dataEBT & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year-1) &"M12:V#YTD:O#Top:I#Top)"
							End If
							
							dataEBT_LY = dataEBT_LY & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year-1) &"M12:V#YTD:O#Top:I#Top)"
								
							i = i + 1
							
						Next
						
					
						If (sEntity.EndsWith("001") And  sMes <=4) 
							Value_EBT = api.Data.GetDataCell(dataEBT_LY).CellAmount
							
							If Value_EBT>0
								EBTtest="1"
							Else
								EBTtest="0"
							End If	
							
							CIT_Percentage_Arg = "RemoveZeros(E#" & sEntity & ":A#CIT:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:T#"& year &"M"& sMes & ":V#Periodic:O#BeforeAdj:I#Top)"
							'brapi.ErrorLog.LogMessage(si,api.Data.GetDataCell("("& EBTtest &")*(" & data4111 &")*(" & CIT_Percentage_Arg & ")").CellAmount)
'							brapi.ErrorLog.LogMessage(si,sTime)
'							brapi.ErrorLog.LogMessage(si,EBTtest)
'							brapi.ErrorLog.LogMessage(si,api.Data.GetDataCell(data4111).CellAmount)
'							brapi.ErrorLog.LogMessage(si,api.Data.GetDataCell(CIT_Percentage_Arg).CellAmount)
'							brapi.ErrorLog.LogMessage(si,api.Data.GetDataCell("("& EBTtest &")*(" & data4111 &")*(" & CIT_Percentage_Arg & ")").CellAmount)
							api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = ("& EBTtest &")*(" & data4111 &")*(" & CIT_Percentage_Arg & ")", True)
							api.Data.Calculate("E#"& sEntityParent &"001:A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = ("& EBTtest &")*(-1)*(" & data4111 &")*(" & CIT_Percentage_Arg & ")", True)

						Else If (sEntity.EndsWith("001") And  sMes >4 And sMes< 7) 
							
							data1561040 = "RemoveZeros(E#" & sEntityParent & "001:A#1561040:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M1:V#YTD:O#Top:I#Top)"
							Dim Value_1561040 As Integer = api.Data.GetDataCell(data1561040).CellAmount
							
							If Value_1561040>0
'								brapi.ErrorLog.LogMessage(si,sTime)
'								brapi.ErrorLog.LogMessage(si,Value_1561040)
								api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:T#"& sTime &":V#YTD:O#Import:I#None = (-1)*(" & data1561040 &")", True)
								api.Data.Calculate("E#"& sEntityParent &"001:A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CITPY:T#"& sTime &":V#YTD:O#Import:I#None = (-1)*(" & data1561040 &")", True)
							End If
							
						
						Else If (sEntity.EndsWith("001") And  sMes >= 7) 
							Value_EBT = api.Data.GetDataCell(dataEBT_LY).CellAmount

							If Value_EBT>0
								EBTtest="1"
							Else
								EBTtest="0"
							End If
							
							CIT_Percentage_Arg = "RemoveZeros(E#" & sEntity & ":A#CIT:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:T#"& year &"M"& sMes & ":V#Periodic:O#BeforeAdj:I#Top)"

							api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = ("& EBTtest &")*(" & data4111 &")*(" & CIT_Percentage_Arg & ")", True)
							api.Data.Calculate("E#"& sEntityParent &"001:A#1561040:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& sTime &":V#Periodic:O#Import:I#None = ("& EBTtest &")*(-1)*(" & data4111 &")*(" & CIT_Percentage_Arg & ")", True)
								
					End If	
					
					
					
				End If
			End If
			#End Region
			
			#Region "Spain/Holding"
			
			If sEntityParent = "R1300" Or sEntityParent = "R1301"
				
		
				
				If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
					
					
					
					Dim i As Integer = 0
					
					'INSTALLMENT PAYMENT
					
					Dim dataEBT_Ene_Mar As String = String.Empty
					Dim data556000_Ene_Mar As String = String.Empty
					
					i = 0
					For Each MesEBT As String In New List(Of String) From {"1", "2", "3"}
						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
						
							If i <> 0 Then
								dataEBT_Ene_Mar = dataEBT_Ene_Mar & " + "
								data556000_Ene_Mar = data556000_Ene_Mar & " + "
							End If
							
							If MesEBT < MesFcstNumber + 1
								dataEBT_Ene_Mar = dataEBT_Ene_Mar & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"	
								data556000_Ene_Mar = data556000_Ene_Mar & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
							Else
								dataEBT_Ene_Mar = dataEBT_Ene_Mar & "RemoveZeros(E#" & miMember.Member.Name & ":A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
								data556000_Ene_Mar = data556000_Ene_Mar & "RemoveZeros(E#" & miMember.Member.Name & ":A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
							End If
							i = i + 1

						Next
					Next
					
					Dim IS_Percentage = "0.24"

					
					Dim dataEBT_Ene_Sep As String = String.Empty
					Dim data556000_Ene_Sep As String = String.Empty
					
					i = 0
					For Each MesEBT As String In New List(Of String) From {"1", "2", "3", "4", "5", "6",  "7", "8", "9"}
						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
						
							If i <> 0 Then
								dataEBT_Ene_Sep = dataEBT_Ene_Sep & " + "
								data556000_Ene_Sep = data556000_Ene_Sep & " + "
							End If
							
							If MesEBT < MesFcstNumber + 1
								dataEBT_Ene_Sep = dataEBT_Ene_Sep & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"	
								data556000_Ene_Sep = data556000_Ene_Sep & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
							Else
								dataEBT_Ene_Sep = dataEBT_Ene_Sep & "RemoveZeros(E#" & miMember.Member.Name & ":A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
								data556000_Ene_Sep = data556000_Ene_Sep & "RemoveZeros(E#" & miMember.Member.Name & ":A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
							End If
							i = i + 1
							
						Next
					Next
					

					
					Dim dataEBT_Ene_Nov As String = String.Empty
					Dim data556000_Ene_Nov As String = String.Empty
					i = 0
					
					For Each MesEBT As String In New List(Of String) From {"1", "2", "3", "4", "5", "6",  "7", "8", "9", "10", "11"}
						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
						
							If i <> 0 Then
								dataEBT_Ene_Nov = dataEBT_Ene_Nov & " + "
								data556000_Ene_Nov = data556000_Ene_Nov & " + "
							End If
							
							If MesEBT < MesFcstNumber + 1
								dataEBT_Ene_Nov = dataEBT_Ene_Nov & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
								data556000_Ene_Nov = data556000_Ene_Nov & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
							Else
								dataEBT_Ene_Nov = dataEBT_Ene_Nov & "RemoveZeros(E#" & miMember.Member.Name & ":A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
								data556000_Ene_Nov = data556000_Ene_Nov & "RemoveZeros(E#" & miMember.Member.Name & ":A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& MesEBT & ":V#Periodic:O#Top:I#Top)"
							End If
							i = i + 1
							
						Next
					Next
					


					'APRIL
					If sMes = 4
						

		
							
						If sEntity.EndsWith("001") And (ScenarioType = "Budget" Or (ScenarioType = "Forecast" And sMes > MesFcstNumber)) Then
							
							If api.Data.GetDataCell("(-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & ")").CellAmount >= 0
								api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M4:V#Periodic:O#Import:I#None = (0)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & ")", True)
								api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M4:V#Periodic:O#Import:I#None = (0)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & ")", True)
							Else
								api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M4:V#Periodic:O#Import:I#None = (-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & ")", True)
								api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M4:V#Periodic:O#Import:I#None = (-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & ")", True)
							End If
						End If
					End If	
	

					'OCTOBER
					If sMes = 10		
		
			

						If sEntity.EndsWith("001") And (ScenarioType = "Budget" Or (ScenarioType = "Forecast" And sMes > MesFcstNumber)) Then
							
							If api.Data.GetDataCell("(-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & ")").CellAmount >= 0
								If api.Data.GetDataCell("(-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ")").CellAmount >= 0
									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (0)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ")", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (0)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ")", True)
								Else
									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ")", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ")", True)
								End If
							Else	
								If api.Data.GetDataCell("(-1)*(((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & "))").CellAmount >= 0
									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (0)*(((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & "))", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (0)*(((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & "))", True)
								Else
									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (-1)*(((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & "))", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M10:V#Periodic:O#Import:I#None = (-1)*(((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & "))", True)
								End If
							End If
						End If
						
					End If	

					'DECEMBER
					If sMes = 12
						
		

						If sEntity.EndsWith("001") And (ScenarioType = "Budget" Or (ScenarioType = "Forecast" And sMes > MesFcstNumber)) Then

							If api.Data.GetDataCell("(-1)*((" & dataEBT_Ene_Mar &"+"& data556000_Ene_Mar &")*" & IS_Percentage & ")").CellAmount >= 0
								
								If api.Data.GetDataCell("(-1)*((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ")").CellAmount >= 0
									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (0)*((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ")", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (0)*((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ")", True)
								Else
									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (-1)*((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ")", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (-1)*((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ")", True)
								End If
							Else 

								If api.Data.GetDataCell("(-1)*(((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & "))").CellAmount >= 0
									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (0)*(((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & "))", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (0)*(((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & "))", True)
								Else

									api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (-1)*(((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & "))", True)
									api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CIT:T#"& year &"M12:V#Periodic:O#Import:I#None = (-1)*(((" & dataEBT_Ene_Nov &"+"& data556000_Ene_Nov &")*" & IS_Percentage & ") + (-1)*((" & dataEBT_Ene_Sep &"+"& data556000_Ene_Sep &")*" & IS_Percentage & "))", True)
								End If
							End If
						End If
					End If
						
					'CIT PAYMENT
					

					'JULY
'					Dim IS_Percentage_July As String

'					IS_Percentage_July = "RemoveZeros(E#" & sEntity & ":A#CIT:F#None:U1#Top:U2#Top:U3#Top:U4#Manual_Input:T#"& year &"M7:V#Periodic:O#BeforeAdj:I#Top)"
					
'					If sMes = 7
'						Dim dataEBT_Ene_Dic_LY As String = String.Empty
'						Dim data556000_Ene_Dic_LY As String = String.Empty
						
'						i = 0
'						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
'							If i <> 0 Then
'								dataEBT_Ene_Dic_LY  = dataEBT_Ene_Dic_LY  & " + "
'								data556000_Ene_Dic_LY = data556000_Ene_Dic_LY & " + "
'							End If

'							dataEBT_Ene_Dic_LY = dataEBT_Ene_Dic_LY & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year -1) &"M12:V#YTD:O#Top:I#Top)"	
'							data556000_Ene_Dic_LY = data556000_Ene_Dic_LY & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year -1) &"M12:V#YTD:O#Top:I#Top)"
	
'							i = i + 1
'						Next
						
'						'LY Payments

'						Dim dataEBT_Ene_Nov_LY As String = String.Empty
'						Dim data556000_Ene_Nov_LY As String = String.Empty
'						i = 0
						
'						For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
						
'							If i <> 0 Then
'								dataEBT_Ene_Nov_LY = dataEBT_Ene_Nov_LY & " + "
'								data556000_Ene_Nov_LY = data556000_Ene_Nov_LY & " + "
'							End If
						
'							dataEBT_Ene_Nov_LY = dataEBT_Ene_Nov_LY & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#EBT:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year-1) &"M11:V#YTD:O#Top:I#Top)"
'							data556000_Ene_Nov_LY = data556000_Ene_Nov_LY & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:A#556000:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:T#"& (year-1) &"M11:V#YTD:O#Top:I#Top)"

'							i = i + 1
							
'						Next

						
	
							
'						If sEntity.EndsWith("001") And (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then
'							If api.Data.GetDataCell("(-1)*(((" & dataEBT_Ene_Dic_LY &"+" & data556000_Ene_Dic_LY & ")*"& IS_Percentage_July &")-((" & dataEBT_Ene_Nov_LY & "+" & data556000_Ene_Nov_LY & ")*" & IS_Percentage_July & "))").CellAmount >= 0
								
'								api.Data.Calculate("E#"& sEntityParent &"001:A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#CITPY:T#"& year &"M7:V#Periodic:O#Import:I#None = (-1)*(((" & dataEBT_Ene_Dic_LY &"+" & data556000_Ene_Dic_LY & ")*"& IS_Percentage_July &")-((" & dataEBT_Ene_Nov_LY & "+" & data556000_Ene_Nov_LY & ")*" & IS_Percentage_July & "))", True)
'								api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F16:U1#None:U2#None:U3#None:U4#CITPY:T#"& year &"M7:V#Periodic:O#Import:I#None = (-1)*(((" & dataEBT_Ene_Dic_LY &"+" & data556000_Ene_Dic_LY & ")*"& IS_Percentage_July &")-((" & dataEBT_Ene_Nov_LY & "+" & data556000_Ene_Nov_LY & ")*" & IS_Percentage_July & "))", True)
								
'							End If
'						End If
							
'					End If
						
					
				End If
				
				
			End If	
			#End Region
			
			
		End Sub
#End Region

	End Class
End Namespace