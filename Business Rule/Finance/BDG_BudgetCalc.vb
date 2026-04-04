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

Namespace OneStream.BusinessRule.Finance.BDG_BudgetCalc
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				' Global Variables
				'-----------------
				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")		
				Dim sTime As String = api.Pov.Time.Name		
				Dim sScenario As String = api.Pov.Scenario.Name	
				Dim sEntity As String = api.Pov.Entity.Name	
				
				'--------------------------------
				
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then
					
						'CALCULATE DATA - volume * unitary 
					If sFunction = "Budget_P_X_Q"							
						
						Me.Budget_P_X_Q(si, api, globals, args)	
						
						'CALCULATE DATA - volume * unitary Material Cost
					Else If sFunction = "Budget_P_X_Q_ICP"							
						
						Me.Budget_P_X_Q_ICP(si, api, globals, args)	
						
						'CALCULATE DATA - WCAP
					Else If sFunction = "WCAP"							
						
						Me.WCAP(si, api, globals, args)	
						
						'CALCULATE DATA - WCAP PAYABLES
					Else If sFunction = "WCAP_PAYABLES"							
						
						Me.WCAP_PAYABLES(si, api, globals, args)	
					
					Else If sFunction = "CAPEX"
						
						Me.CAPEX_validar(si, api, globals, args)
					
					Else If sFunction = "CorpRate"
						
						Me.CorpRate_Validar(si, api, globals, args)
						
					Else If sFunction = "Ratios_Proposal"
						
						Me.Ratios_Proposal(si, api, globals, args)
						
					Else If sFunction = "DSO"
						
						Me.WCAP_DSO(si, api, globals, args)
						
					Else If sFunction = "DIO"
						
						Me.WCAP_DIO(si, api, globals, args)
					
					Else If sFunction = "Borrar_UDs"
						
						Me.borradoUD(si, api, globals, args)
						
					Else If sFunction = "DSO_DPO_DIO"
						
						Me.WCAP_DSO(si, api, globals, args)
						Me.WCAP_PAYABLES(si, api, globals, args)
						Me.WCAP_DIO(si, api, globals, args)
						Me.WCAP_DIO_2(si, api, globals, args)
						
						
					End If
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		


#Region "Corp. Rate"

'___________________________________________________________________________________________

'---------------------------------------- Corp. Rate ---------------------------------------
'___________________________________________________________________________________________
	
	Sub CorpRate (ByVal si As SessionInfo,  ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		If api.Cons.IsLocalCurrencyForEntity() Then
			
			' Declare Corporate Income Tax Rate
			Dim corpRate As Decimal = 0.25
			
			' Clear data for Account 4111 and calculate
			api.Data.ClearCalculatedData("A#4111:F#None:O#Import",True,True,True,True)
			
			api.Data.Calculate(
				"A#4111:F#None:O#Import = " &
				"A#EBT:F#None:O#Top" &
				"* {corpRate}"
			)
			
			' Clear data for Account 2901720 and calculate
			api.Data.ClearCalculatedData("A#2901720:F#None:O#Import",True,True,True,True)
			
			api.Data.Calculate(
				"A#2901720:F#F20:O#Import = " &
				"A#EBT:F#None:O#Top" &
				"* {corpRate}"
			)
			
		End If
	
	End Sub
	
	Sub CorpRate_Validar (ByVal si As SessionInfo,  ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString ' Budget
		Dim sEntity As String = api.pov.Entity.Name ' Para test R1301001 - Spain
		Dim sScenario As String = api.Pov.Scenario.Name ' Budget_V1
		Dim sTime As String = api.Pov.Time.Name ' será 2025M1,...
		Dim year As String = sTime.Substring(0,4)
		Dim sEntityParent As String = Left(sEntity,5)
		
		Dim dataEBT As String = String.Empty
			
		Dim i As Integer = 0
		For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
							
			If i <> 0 Then
				dataEBT = dataEBT & " + "
			End If
			
			dataEBT = dataEBT & "RemoveZeros(E#" & miMember.Member.Name & ":A#EBT:F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"												
			
			i = i+1
			
		Next
		
		Dim dataEBT_val = api.Data.GetDataCell(dataEBT).CellAmount.ToString(cultureinfo.InvariantCulture)
		
''		If api.Cons.IsLocalCurrencyForEntity() Then
			
			' Declare Corporate Income Tax Rate
			Dim corpRate As String = api.Data.GetDataCell("E#"& sEntityParent &"001:C#Local:S#"& sScenario &":T#"& sTime &":V#Periodic:A#Corp_Tax:F#None:O#Forms:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
			
			api.Data.ClearCalculatedData("E#"& sEntity &":A#4111:F#None:O#Import:U4#CORP",True,True,True,True)
			api.Data.Calculate("E#"& sEntity &":A#4111:F#None:O#Import:U1#None:U2#None:U3#None:U4#CORP:V#Periodic = A#EBT:F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic * (-1) *"&corpRate, True)
			

			If sEntity = "R1300001" Then
				BRAPI.ErrorLog.LogMessage(SI, corpRate)
			End If

			If sEntity.EndsWith("001") Then
				
				api.Data.ClearCalculatedData("A#2901720:F#F20:O#Import:U4#CORP",True,True,True,True)
				api.Data.ClearCalculatedData("A#1161030:F#F20:O#Import:U4#CORP",True,True,True,True)
				
				api.Data.Calculate("E#"& sEntity &":A#2901720:F#F20:O#Import:U1#None:U2#None:U3#None:U4#CORP:V#Periodic = (" & dataEBT &")*"&corpRate, True)

			End If
			
''		End If
		
	
	End Sub

#End Region	

#Region "CAPEX"
'___________________________________________________________________________________________

'---------------------------------------- CAPEX --------------------------------------------
'___________________________________________________________________________________________
	
	Sub CAPEX (ByVal si As SessionInfo,  ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		If api.Cons.IsLocalCurrencyForEntity() Then
			
			' 1001004C - Intangible Assets Amortization (Suma de 55406, 572107)
			api.Data.ClearCalculatedData("A#1001004C:F#F25:O#Import",True,True,True,True)
			
			api.Data.Calculate(
				"V#YTD:A#1001004CF:F#F25:O#Import = " &
				"V#YTD:A#552406:F#None:O#Top" &
				"+ V#YTD:A#572107:F#None:O#Top"
			)
        
	        ' 1021004C - Capitalised Development Expenses Amortization (Suma de 572115)
	        api.Data.ClearCalculatedData("A#1021004C:F#F25:O#Import",True,True,True,True)   
	        
	        api.Data.Calculate(
	            "V#YTD:A#1021004C:F#F25:O#Import = " &
	            "V#YTD:A#572115:F#None:O#Top"
	        )
	        
	        ' 1031004C - Right of Use of Leased Assets Amortization (Suma de 552307, 563107)
	        api.Data.ClearCalculatedData("A#1031004C:F#F25:O#Import",True,True,True,True)
	        
	        api.Data.Calculate(
	            "V#YTD:A#1031004CF:F#F25:O#Import = " &
	            "V#YTD:A#552307:F#None:O#Top" &
	            "+ V#YTD:A#563107:F#None:O#Top"
	        )
	        
	        ' 1041004C - Depreciation (Suma de 55110590, 551107, 551108, 552108)
	        api.Data.ClearCalculatedData("A#1041004C:F#F25:O#Import",True,True,True,True)
	        
	        api.Data.Calculate(
	            "V#YTD:A#1041004CF:F#F25:O#Import = " &
	            "V#YTD:A#55110590:F#None:O#Top" &
	            "+ V#YTD:A#551107:F#None:O#Top" &
	            "+ V#YTD:A#551108:F#None:O#Top" &
	            "+ V#YTD:A#552108:F#None:O#Top"
	        )
	
	        ' 1501004C - Provisions/Depreciation of Inventories (Suma de 555103R)
	        api.Data.ClearCalculatedData("A#1501004C:F#F25:O#Import",True,True,True,True)
	        
	        api.Data.Calculate(
	            "V#YTD:A#1501004CF:F#F25:O#Import = " &
	            "V#YTD:A#555103R:F#None:O#Top"
	        )
			
		End If
	
	End Sub

	Sub CAPEX_validar (ByVal si As SessionInfo,  ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) ' pendiente de validar
			
			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString ' Budget
			Dim sEntity As String = api.pov.Entity.Name ' Para test R1301001 - Spain
			Dim sScenario As String = api.Pov.Scenario.Name ' Budget_V1
			Dim sTime As String = api.Pov.Time.Name ' será 2025M1,...
			Dim year As String = sTime.Substring(0,4)
			Dim sEntityParent As String = Left(sEntity,5)
			
			Dim intangAssets As String = String.Empty
			Dim capitDevelop As String = String.Empty
			Dim rightUse As String = String.Empty
			Dim depreciation As String = String.Empty
			Dim provisions As String = String.Empty
				
			Dim i As Integer = 0
			For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
								
				If i <> 0 Then
					intangAssets = intangAssets & " + "
					capitDevelop = capitDevelop & " + "
					rightUse = rightUse & " + "
					depreciation = depreciation & " + "
					provisions = provisions & " + "
				End If
				
				intangAssets = intangAssets & "RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#552406:F#None:O#Top:U4#Top) + RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#572107:F#None:O#Top:U4#Top) + RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#557103:F#None:O#Top:U4#Top) + RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#563109:F#None:O#Top:U4#Top)"													
				capitDevelop = capitDevelop & "RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#572115:F#None:O#Top:U4#Top)"
				rightUse = rightUse & "RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#552307:F#None:O#Top:U4#Top) + RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#563107:F#None:O#Top:U4#Top)"
				depreciation = depreciation & "RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#55110590:F#None:O#Top:U4#Top) + RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#551107:F#None:O#Top:U4#Top) + RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#551108:F#None:O#Top:U4#Top) + RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#552108:F#None:O#Top:U4#Top)" 
				provisions = provisions & "RemoveZeros(E#" & miMember.Member.Name & ":V#Periodic:A#555103R:F#None:O#Top:U4#Top)"
				
				i = i+1
				
			Next
'			If sTime = "2025M1" Then
'				brapi.ErrorLog.LogMessage(si, depreciation)
'			End if
			If sEntity.EndsWith("001") Then
				If api.Cons.IsLocalCurrencyForEntity() Then
					api.Data.ClearCalculatedData("U4#CAPEX",True,True,True,True)
					
					' 1001004C - Intangible Assets Amortization (Suma de 55406, 572107)
					api.Data.ClearCalculatedData("A#1001004C:F#F25:O#Import:U4#CAPEX",True,True,True,True)
					
					api.Data.Calculate("V#Periodic:A#1001004CF:F#F25:O#Import:U4#CAPEX = (" & intangAssets &")", True)
				
					' 1021004C - Capitalised Development Expenses Amortization (Suma de 572115)
					api.Data.ClearCalculatedData("A#1021004C:F#F25:O#Import:U4#CAPEX",True,True,True,True)   
					
					api.Data.Calculate("V#Periodic:A#1021004C:F#F25:O#Import:U4#CAPEX = (" & capitDevelop &")", True)
					
					' 1031004C - Right of Use of Leased Assets Amortization (Suma de 552307, 563107) -
					api.Data.ClearCalculatedData("A#1031004C:F#F25:O#Import",True,True,True,True)
					
					api.Data.Calculate("V#Periodic:A#1031004CF:F#F25:O#Import:U4#CAPEX = (" & rightUse &")", True)
					
					' 1041004C - Depreciation (Suma de 55110590, 551107, 551108, 552108) -
					api.Data.ClearCalculatedData("A#1041004C:F#F25:O#Import:U4#CAPEX",True,True,True,True)
					
					api.Data.Calculate("V#Periodic:A#1041004CF:F#F25:O#Import:U4#CAPEX = (" & depreciation &")", True)
			
					' 1501004C - Provisions/Depreciation of Inventories (Suma de 555103R) 
					api.Data.ClearCalculatedData("A#1501004C:F#F25:O#Import:U4#CAPEX",True,True,True,True)
					
					api.Data.Calculate("V#Periodic:A#1501004CF:F#F25:O#Import:U4#CAPEX = (" & provisions &")", True)
					
				End If
			End If		
	End Sub
	
#End Region

#Region "P*Q"
'___________________________________________________________________________________________

'---------------------------------------- P * Q---------------------------------------------
'___________________________________________________________________________________________

	' Se calculan todas las cuentas excepto la de material cost, que se tiene que calcular en una segunda vuelta, donde ya se tengan las ventas calculadas con ICP
	Sub Budget_P_X_Q (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
		
		If api.Cons.IsLocalCurrencyforEntity() Then
   					
			' Sales Volume x Price
			api.Data.ClearCalculatedData("A#Unit_Sell_Price:O#Import",True,True,True,True)			
'				api.Data.Calculate("A#541107:O#Import = (A#Vol_Sales:O#Top * A#Unit_Sell_Price:O#Top) / 1000")
			api.Data.Calculate("A#Unit_Sell_Price:O#Import = A#541107:O#Top * 1000 / A#Vol_Sales:O#Top",True)
				
			'----------------------------------------------------------------------------------------------------	
				
			' Prod Volume x UnitLogistic
			api.Data.ClearCalculatedData("A#Unit_LCPU:O#Import",True,True,True,True)
'				api.Data.Calculate("A#5511065:O#Import = (A#Vol_Prod:O#Top * A#Unit_LCPU:O#Top) / 1000")
			api.Data.Calculate("A#Unit_LCPU:O#Import = A#5511065:O#Top * 1000 / A#Vol_Prod:O#Top * (-1)",True)
			
			'----------------------------------------------------------------------------------------------------
			
			'Sales Volume x UnitWarranty
			api.Data.ClearCalculatedData("A#Unit_Warranty:O#Import",True,True,True,True)
'				api.Data.Calculate("A#556107:O#Import:I#None = (A#Vol_Sales:O#Top:I#Top * A#Unit_Warranty:O#Top:I#None) / 1000")
			api.Data.Calculate("A#Unit_Warranty:O#Import = A#556107:O#Top * 1000 / A#Vol_Sales:O#Top * (-1)",True)
			
			'----------------------------------------------------------------------------------------------------
			
			' Prod Volume x Unitary Customs
			api.Data.ClearCalculatedData("A#Unit_Customs:O#Import",True,True,True,True)							
'				api.Data.Calculate("A#5511064:O#Import:I#None = (A#Vol_Prod:O#Top:I#None * A#Unit_Customs:O#Top:I#None) / 1000")
			api.Data.Calculate("A#Unit_Customs:O#Import = A#5511064:O#Top * 1000 / A#Vol_Prod:O#Top * (-1)",True)
			
			'----------------------------------------------------------------------------------------------------
			
			'Production Volume x Unitary Variable Transformation Value 51
			api.Data.ClearCalculatedData("A#Unit_Transformation_Value_51:O#Import",True,True,True,True)
'				api.Data.Calculate("A#551105F:O#Import = (A#Vol_Prod:O#Top * A#Unit_Transformation_Value:O#Top) / 1000")
			api.Data.Calculate("A#Unit_Transformation_Value_51:O#Import = A#5511051:O#Top * 1000 / A#Vol_Prod:O#Top * (-1)",True)

			'----------------------------------------------------------------------------------------------------
			
			'Production Volume x Unitary Variable Transformation Value 56
			api.Data.ClearCalculatedData("A#Unit_Transformation_Value_56:O#Import",True,True,True,True)
'				api.Data.Calculate("A#551105F:O#Import = (A#Vol_Prod:O#Top * A#Unit_Transformation_Value:O#Top) / 1000")
			api.Data.Calculate("A#Unit_Transformation_Value_56:O#Import = A#5511056:O#Top * 1000 / A#Vol_Prod:O#Top * (-1)",True)			
			
			'----------------------------------------------------------------------------------------------------
			
			'Production Volume x Unitary Variable Transformation Value 59
			api.Data.ClearCalculatedData("A#Unit_Transformation_Value_59:O#Import",True,True,True,True)
'				api.Data.Calculate("A#551105F:O#Import = (A#Vol_Prod:O#Top * A#Unit_Transformation_Value:O#Top) / 1000")
			api.Data.Calculate("A#Unit_Transformation_Value_59:O#Import = A#5511056:O#Top * 1000 / A#Vol_Prod:O#Top * (-1)",True)		
			
			
		End If	
	End Sub
	
	' Calculo para la cuenta de material costs, cuyo ICP se calcula en base a las ventas que se calculan en la funcion de arriba
	Sub Budget_P_X_Q_ICP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
		
		If api.Cons.IsLocalCurrencyforEntity() Then
   					
		
			' Unitary Material Costs(551103) 
			api.Data.ClearCalculatedData("A#Unit_Material_Cost:O#Import",True,True,True,True)
			api.Data.ClearCalculatedData("A#551103:O#Import",True,True,True,True)
			
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
					
'					BRAPI.ErrorLog.LogMessage(si, $"{entityICCurrencyName} to {entityPOVCurrencyName}: {revRate}")
						
					Dim salesICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#541107:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)

'								BRApi.ErrorLog.LogMessage(si, "salesICPAmount: " & salesICPAmount)
					
					' Calculamos
					If (salesICPAmount <> 0)					
						api.Data.Calculate(
							"E#" & sEntityPOV & ":C#" & entityPOVCurrencyName & ":V#YTD:A#551103:O#Import:I#" & sIC & " = " &
							"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#541107:O#Top:I#" & sEntityPOV &
							"* (-1) * " & revRate.ToString.Replace(",","."),True)
						
					End If
						
					
				Next	
				
				' Ajustamos el importe de coste con terceros, quitando la parte ICP que viene de ventas
				api.Data.Calculate("A#551103:O#Import:I#None = A#Material_Cost_Temp:O#Import:I#None - A#551103:O#Import:I#ICEntities",True)
				
				api.Data.Calculate("A#Unit_Material_Cost:O#Import = A#551103:O#Top * 1000 / A#Vol_Prod:O#Top * (-1)",True)
			
		End If	
	End Sub
#End Region

#Region "Ratios Proposal"

	Sub Ratios_Proposal (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
		Dim sEntity As String = Api.Pov.Entity.Name
		Dim sParent As String = Left(sEntity, 5)
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString 'NOVA
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)				
		Dim MesForecastNumber As String = api.Scenario.GetWorkflowNumNoInputTimePeriods.ToString
		Dim MesFcst As Integer = Integer.Parse(MesForecastNumber.ToString)
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
				
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If ScenarioType.Equals("Forecast") Then
					If api.pov.Entity.name.endswith("001") Then
						If MesCorriente <= MesFcst Then
												
						Else 'CALCULA COMO FORECAST
							
							'Cogemos no input periods del escenario
							Dim noInputPeriods As String = api.Scenario.GetWorkflowNumNoInputTimePeriods().ToString()
							
							api.Data.ClearCalculatedData("A#DSOp:U4#None", True, True, True, True)
							api.Data.ClearCalculatedData("A#DPOp:U4#None", True, True, True, True)
							api.Data.ClearCalculatedData("A#DIOp:U4#None", True, True, True, True)
														
							'DSOp
								Dim receivablesOpeningAccount As String = "A#02AC03:V#Periodic:O#Top:F#F00:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
								Dim receivablesActualAccount As String = $"S#Actual:T#{CurYear}M{MesForecastNumber}:A#02AC03:V#Periodic:O#Top:F#F99:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
								Dim receivablesAverage As String = $"( {receivablesOpeningAccount} + {receivablesActualAccount} ) / 2"
								
								'Calculamos el dato agregado por entidad
								Dim sEntityParent As String = Left(sEntity,5)
								Dim salesActualAccount As String = String.Empty
								Dim COGSActualAccount As String = String.Empty
								Dim COGMActualAccount As String = String.Empty						
								
								Dim i As Integer = 0
								For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)					
									If i <> 0 Then
										salesActualAccount = salesActualAccount & " + "
										COGSActualAccount = COGSActualAccount & " + "
										COGMActualAccount = COGMActualAccount & " + "
									End If
									
									salesActualAccount = salesActualAccount & "RemoveZeros(E#" & miMember.Member.Name & ":S#Actual:T#{CurYear}M{MesForecastNumber}:A#Receivables_Acc:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)"												
									COGSActualAccount = COGSActualAccount & "RemoveZeros(E#" & miMember.Member.Name & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#Payables_Acc:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)"
									COGMActualAccount = COGMActualAccount & "RemoveZeros(E#" & miMember.Member.Name & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#COGS_Acc:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)"
									
									i = i+1	
								Next
								
								'Dim salesActualAccount As String = $"S#Actual:E#{sParent}:T#{CurYear}M{MesForecastNumber}:A#Receivables_Acc:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
								
								Dim calculationStringDSOp As New Text.StringBuilder()
								calculationStringDSOp.Append("A#DSOp:V#Periodic:O#Import:F#None:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = ")
								calculationStringDSOp.Append($"{receivablesAverage} / {salesActualAccount} * 30 * {noInputPeriods}")
								
								api.Data.Calculate(calculationStringDSOp.ToString(), True)
							
							'DPOp
								Dim payablesOpeningAccount As String = "(A#05PC05:V#Periodic:O#Top:F#F00:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None - A#05PC05:O#Top:F#F00:I#Top:U1#Top:U2#Top:U3#Top:U4#WCAP:U5#None:U6#None:U7#None:U8#None)"
								Dim payablesActualAccount As String = $"S#Actual:T#{CurYear}M{MesForecastNumber}:A#05PC05:V#Periodic:O#Top:F#F99:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
								Dim payablesAverage As String = $"( {payablesOpeningAccount} + {payablesActualAccount} ) / 2"
						
'								Dim COGSActualAccountStringBuilder As New Text.StringBuilder()		
'								COGSActualAccountStringBuilder.Append("((E#" & sParent & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#551000:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)")
'								COGSActualAccountStringBuilder.Append("+ (E#" & sParent & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#552000:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)")
'								COGSActualAccountStringBuilder.Append("+ (E#" & sParent & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#553000:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)")
'								COGSActualAccountStringBuilder.Append("+ (E#" & sParent & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#554000:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)")
'								COGSActualAccountStringBuilder.Append("+ (E#" & sParent & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#555000:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)")
'								COGSActualAccountStringBuilder.Append("+ (E#" & sParent & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#556000:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)")
'								COGSActualAccountStringBuilder.Append("+ (E#" & sParent & ":T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":A#557000:V#Periodic:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None))")
'								Dim COGSActualAccount As String = COGSActualAccountStringBuilder.ToString()
								
								Dim calculationStringDPOp As New Text.StringBuilder()
								calculationStringDPOp.Append("A#DPOp:V#Periodic:O#Import:F#None:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = ")
								calculationStringDPOp.Append($"{payablesAverage} / {COGSActualAccount} * 30 * {noInputPeriods}")
							
								api.Data.Calculate(calculationStringDPOp.ToString(), True)
							
							'DIOp
								Dim stockOpeningAccount As String = "(A#02AC01:V#Periodic:O#Top:F#F00:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None - A#02AC01:O#Top:F#F00:I#Top:U1#Top:U2#Top:U3#Top:U4#WCAP:U5#None:U6#None:U7#None:U8#None)"
								Dim stockActualAccount As String = $"S#Actual:T#{CurYear}M{MesForecastNumber}:A#02AC01:V#Periodic:O#Top:F#F99:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
								Dim stockAverage As String = $"( {stockOpeningAccount} + {stockActualAccount} ) / 2"
'								Dim COGMActualAccountStringBuilder As New Text.StringBuilder()
'								COGMActualAccountStringBuilder.Append("((E#" & sParent & ":A#541000:T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":V#YTD:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None)")
'								COGMActualAccountStringBuilder.Append("- (E#" & sParent & ":A#GROSSMARGIN:T#" & CurYear.ToString & "M" & MesForecastNumber.ToString & ":V#YTD:O#Top:F#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None))")
'								Dim COGMActualAccount As String = COGMActualAccountStringBuilder.ToString()
								
								Dim calculationStringDIOp As New Text.StringBuilder()
								calculationStringDIOp.Append("A#DIOp:V#Periodic:O#Import:F#None:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = ")
								calculationStringDIOp.Append($"{stockAverage} / {COGMActualAccount} * 30 * {noInputPeriods}")
								
								api.Data.Calculate(calculationStringDIOp.ToString(), True)
						End If ' meses forecast
							
					End If	'entidad 001
				
				Else If ScenarioType.Equals("Budget") Then 
					
					If api.pov.Entity.name.endswith("001") Then
						'Cogemos no input periods del escenario
						Dim ScenarioFcstBase As String = "RF06"
						
						api.Data.ClearCalculatedData("A#DSOp:U4#None", True, True, True, True)
						api.Data.ClearCalculatedData("A#DPOp:U4#None", True, True, True, True)
						api.Data.ClearCalculatedData("A#DIOp:U4#None", True, True, True, True)
				
						
						'DSOp
						api.Data.Calculate("A#DSOp:V#Periodic:O#Import:F#None:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =" & _
											$"((A#02AC03:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC03:T#2024M12:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC03:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#Receivables_Acc:E#{sParent}:T#2024M12:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 12")
						
											
					
						
'						'DPOp
						api.Data.Calculate("A#DPOp:V#Periodic:O#Import:F#None:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =" & _
											$"((A#05PC05:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#05PC05:T#2024M12:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#05PC05:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M12:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 12 * -1")
						
						
						'DIOp
						api.Data.Calculate("A#DIOp:V#Periodic:O#Import:F#None:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None =" & _
											$"((A#02AC01:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M1:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M2:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M3:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M4:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M5:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M6:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M7:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M8:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M9:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M10:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + " & _
											$"(A#02AC01:T#2024M12:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None + A#02AC01:T#2024M11:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 2 * 30 / A#COGS_Acc:E#{sParent}:T#2024M12:C#EUR:S#{ScenarioFcstBase}:O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None) / 12 * -1")
						
							
					End If	'entidad 001
				End If	'escenario
			End If ' local currency
		End If	'children
		
	End Sub

#End Region

#Region "WCAP"
'___________________________________________________________________________________________

'---------------------------------------- WCAP ---------------------------------------------
'___________________________________________________________________________________________
	
	' Calculo DSO y DIO
	Sub WCAP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name
	
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		
		' Calculo del importe sumando las cajas de cada pais
		'-----------------------------------------------------------------------------------
'		Dim sReceivablesNone As String = String.Empty
'		Dim sCOGSNone As String = String.Empty
		Dim sReceivablesCountry As String = String.Empty
		Dim sCOGSCountry As String = String.Empty
		
		Dim i As Integer = 0
				
		For Each entityMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sParent & ".Base", True)
								
			If i <> 0 Then
'				sReceivablesNone = sReceivablesNone & " + "
				sReceivablesCountry = sReceivablesCountry & " + "
				sCOGSCountry = sCOGSCountry & " + "
'				sCOGSNone = sCOGSNone & " + "

			End If
			
'			sReceivablesNone = sReceivablesNone & "RemoveZeros(E#" & entityMember.Member.Name & ":A#Receivables_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#None)"	
			sReceivablesCountry = sReceivablesCountry & "RemoveZeros(E#" & entityMember.Member.Name & ":A#Receivables_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top)"	
			' MA: sReceivablesCountry = sReceivablesCountry & "RemoveZeros(E#" & entityMember.Member.Name & ":A#Receivables_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:O#Top)"	
'			sCOGSNone = sCOGSNone & "RemoveZeros(E#" & entityMember.Member.Name & ":A#COGS_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#None)"		
			sCOGSCountry = sCOGSCountry & "RemoveZeros(E#" & entityMember.Member.Name & ":A#COGS_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top)"	
			' MA: sCOGSCountry = sCOGSCountry & "RemoveZeros(E#" & entityMember.Member.Name & ":A#COGS_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:O#Top)"	
			'Brapi.ErrorLog.LogMessage(si, "sReceivablesNone: " & sReceivablesNone)				
			
			i = i+1
			
		 Next	
	



		'-----------------------------------------------------------------------------------
		
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If api.pov.Entity.name.endswith("001") Then
					
					Dim VAT = api.Data.GetDataCell("E#" & sParent & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim WVATSales = api.Data.GetDataCell("E#" & sParent & "001:A#WVAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
						
					Dim DSOp = api.Data.GetDataCell("E#" & sEntity & ":A#DSOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim DIOp = api.Data.GetDataCell("E#" & sEntity & ":A#DIOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					' brapi.ErrorLog.LogMessage(si,"Recivables: "& api.Data.GetDataCell(sReceivablesCountry).CellAmount.ToString() &" - COGS: "& api.Data.GetDataCell(sCOGSCountry).CellAmount.ToString())
				
'				If api.pov.Entity.name.endswith("001") Then ' -- MA
							
					'borrado de cuentas con dato en WCAP
					api.Data.ClearCalculatedData("U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#1541000CF:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#1501000CF:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#1681000CF:U4#WCAP:F#F20", True, True, True, True)
					api.Data.ClearCalculatedData("A#1561021:U4#WCAP", True, True, True, True)
'					api.Data.ClearCalculatedData("A#2301710:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#2901710:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#Turnover_VAT:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#Receivables_Acc_Country", True, True, True, True)
					api.Data.ClearCalculatedData("A#COGS_Country", True, True, True, True)
					
					
					
					' Usamos una cuenta temporal para tener la suma de los revenues y de los cogs de las cajas del pais	
					 api.Data.Calculate("A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP = " & sReceivablesCountry, True)
					 api.Data.Calculate("A#COGS_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP = " & sCOGSCountry, True)
					
					'_________________Se divide el calculo entre IC None e ICEntities porque el dato de las cajas se quiere guardar en la 001_____________
					
					'____________________________________________________ Calc ICNone ____________________________________________________________________
					'
					'calc F14	

					
					' Calculo de ventas con IVA - Turnover_VAT
'					api.Data.Calculate("A#Turnover_VAT:V#YTD:F#F14:U4#WCAP:I#None = ((A#Receivables_Acc:E#" & sParent & ":V#Periodic:F#None:U4#Top:I#None * " & WVATSales & " * (1 + " & VAT & ")) + (A#Receivables_Acc:E#" & sParent & ":V#Periodic:F#None:U4#Top:I#None * ( 1 - " & WVATSales & " )))", True)
'					api.Data.Calculate("A#Turnover_VAT:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None = (((" & sReceivablesNone & ") * "  & WVATSales & " * (1 + " & VAT & ")) + ((" & sReceivablesNone & ") * ( 1 - " & WVATSales & " )))", True)
					api.Data.Calculate("A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None = ((A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None * "  & WVATSales & " * (1 + " & VAT & ")) + (A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None * ( 1 - " & WVATSales & " )))", True)
				
					
					' Calculo de receivables -1541000CF (no se tiene en cuenta el IVA)
'					api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U4#WCAP:I#None = ((A#Receivables_Acc:E#" & sParent & ":V#Periodic:F#None:U4#Top:I#None /30 * " & DSOp & ") - A#1541000CF:E#" & sParent & "001:V#YTD:F#F00:U4#None:I#None))", True)
					api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None = ((A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None / 30 * " & DSOp & ") 
										- A#1541000CF:E#" & sParent & "001:V#YTD:F#F00:U1#None:U2#None:U4#None:I#None))", True)
								
					' Calculo de inventarios -1501000CF (no se tiene en cuenta el IVA)
					api.Data.Calculate("A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None= ((A#COGS_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None * (-1) / 30 * " & DIOp & ") 
										- A#1501000CF:E#" & sParent & "001:V#YTD:F#F00:U1#None:U2#None:U4#None:I#None))", True)
					
'					' Incluido también en la lista Consumption of materials and parts - A falta de validar cuando de dato
'					api.Data.Calculate("A#551104:V#YTD:F#None:U1#None:U2#None:U4#WCAP:I#None= (((" & sCOGSNone & ") * (-1) / 30 * " & DIOp & ") 
'										- A#1501000CF:E#" & sParent & "001:V#YTD:F#F00:U1#None:U2#None:U4#None:I#None))", True)	
					
'				End If ' -- MA
					api.Data.Calculate("A#551104:V#YTD:F#None:U1#None:U2#None:U4#WCAP:I#None= A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None", True)	
'					brapi.ErrorLog.LogMessage(si,sEntity &": "& api.Data.GetDataCell("A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None").CellAmount.ToString())
					'____________________________________________ Calc ICEntities ________________________________________________________________________							
					
'				If 	api.pov.Entity.name.endswith("001") Then ' -- MA
			
					Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
								
					For Each ICMember As Member In ICList
						
						Dim sIC As String = ICMember.Name.ToString
						Dim sParentIC As String = left(ICMember.Name.ToString, 5)
	
						'VAT y WVAT de Interco
						Dim VAT_IC = api.Data.GetDataCell("E#" & sParentIC & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
										
						' Calculo de ventas con IVA
'						api.Data.Calculate("A#Turnover_VAT:V#YTD:F#F14:U4#WCAP:I#" & sParentIC & "001:U1#None:U2#None = ((A#Receivables_Acc:E#" & sParent & ":V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * " & WVATSales & " * (1 + " & VAT & ")) + (A#Receivables_Acc:E#" & sParent & ":V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * ( 1 - " & WVATSales & " )))", True)
						api.Data.Calculate("A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = ((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * " & WVATSales & " * (1 + " & VAT & ")) + (A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * ( 1 - " & WVATSales & " )))", True)
										

						If (MesCorriente = 1)
						
							'calc F14
								'Account Receivables - 1541000CF
'								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 + ((A#Receivables_Acc:E#" & sParent & ":V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " /30 * " & DSOp & ")) - A#1541000CF:E#" & sParent & ":V#YTD:F#F00:U1#Top:U2#Top:U4#None:I#" & sIC & ")", True)
'								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 + ((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " /30 * " & DSOp & ")) - A#1541000CF:E#" & sParent & "001:V#YTD:F#F00:U1#Top:U2#Top:U4#None:I#" & sIC & ")", True)
								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = 
														A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 + 
														(((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * " & WVATSales & " * (1 + " & VAT & ") + 
														   A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * (1 - " & WVATSales & ")) /30 * " & DSOp & ")) 
															- A#1541000CF:E#" & sParent & "001:V#YTD:F#F00:U1#Top:U2#Top:U4#None:I#" & sIC & ")", True)
								
						Else
							'calc F14
								'Account Receivables - 1541000CF
'								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 - A#1541000CF:V#YTD:T#POVPrior1:F#F14:U1#None:U2#None:U4#WCAP:I#" & sIC & " + ((A#Receivables_Acc:E#" & sParent & ":V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " /30 * " & DSOp & ")) - A#1541000CF:E#" & sParent & ":V#YTD:F#F00:U1#None:U2#None:U4#None:I#" & sIC & ")", True)
'								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 - A#1541000CF:V#YTD:T#POVPrior1:F#F14:U1#None:U2#None:U4#WCAP:I#" & sIC & " + ((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " /30 * " & DSOp & ")) - A#1541000CF:E#" & sParent & "001:V#YTD:F#F00:U1#None:U2#None:U4#None:I#" & sIC & ")", True)
								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = 
														A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 - 
														A#1541000CF:V#YTD:T#POVPrior1:F#F14:U1#None:U2#None:U4#WCAP:I#" & sIC & " + 
														(((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * " & WVATSales & " * (1 + " & VAT & ") + 
														   A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * (1 - " & WVATSales & ")) /30 * " & DSOp & ")) 
														- A#1541000CF:V#YTD:F#F00:U1#None:U2#None:U4#None:I#" & sIC & ")", True)
							
						End If
							
					Next
					
					
					'VAT Calc					 				
						'Account Receivables VAT - 2901710
						api.Data.Calculate("A#2901710:V#Periodic:F#F20:U4#WCAP:I#None:U1#None:U2#None = 
												A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#Top * " & WVATSales & " * " & VAT & "", True)
					
					'calc Cash - 1681000CF
						'api.data.calculate("A#1681000CF:F#F15:U4#WCAP= ((A#1541000CF:F#F30:U4#WCAP + A#1541000CF:F#F16:U4#WCAP) * -1) + (A#2721000CF:F#F30:U4#WCAP + A#2721000CF:F#F16:U4#WCAP) + ((A#1501000CF:F#F30:U4#WCAP + A#1501000CF:F#F16:U4#WCAP + A#1501000CF:F#F20:U4#WCAP) * -1) ")
'						api.data.calculate("A#1681000CF:V#YTD:F#F20:U4#WCAP:U1#None:U2#None = A#1541000CF:V#YTD:F#F99:U1#None:U2#None:U4#Top - A#1541000CF:V#YTD:T#POVPrior1:F#F99:U1#None:U2#None:U4#Top", True)
'						api.data.calculate("A#1681000CF:V#YTD:F#F20:U4#WCAP:U1#None:U2#None:I#None = A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#Top -  A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#Top" , True)
					api.data.calculate("A#1681000CF:V#Periodic:F#F20:U4#WCAP:U1#None:U2#None:I#None = A#Turnover_VAT:E#" & sParent & "001:V#Periodic:F#None:U1#NOne:U2#None:U4#WCAP:I#Top - A#1541000CF:V#Periodic:F#F14:U1#None:U2#None:U4#WCAP:I#Top" , True)
					
					'calculo F99I Se recalcula porque el calculo de F99I se lanza antes que WCAP
						api.data.calculate("F#F99I:U4#WCAP=F#F99:U4#WCAP",True)
							
				End If 'entidad 001
			End If 'local
		End If 'has children
	End Sub

	' Calculo DPO
	Sub WCAP_PAYABLES (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
' MA cambio DPO ---	Consiste en cambiar la cuenta 2301710 por 1561021 y poner las cuentas COGS_Country, 1561021 con un valor positivo

		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name
	
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		
		' Calculo del importe sumando las cajas de cada pais
		'-----------------------------------------------------------------------------------
'		Dim sCOGSNone As String = String.Empty
		Dim sCOGSCountry As String = String.Empty
		
		Dim i As Integer = 0
				
		For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sParent & ".Base", True)
								
			If i <> 0 Then
'				sCOGSNone = sCOGSNone & " + "
				sCOGSCountry = sCOGSCountry & " + "
			End If
			
'			sCOGSNone = sCOGSNone & "RemoveZeros(E#" & miMember.Member.Name & ":A#COGS_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#None)"				
			sCOGSCountry = sCOGSCountry & "RemoveZeros(E#" & miMember.Member.Name & ":A#COGS_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top)"				
			
			i = i+1
			
		Next	
			
		'-----------------------------------------------------------------------------------
		
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If api.pov.Entity.name.endswith("001") Then
					
					Dim VAT = api.Data.GetDataCell("E#" & sParent & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)			
					Dim WVATPurchases = api.Data.GetDataCell("E#" & sParent & "001:A#WVATP:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					Dim DPOp = api.Data.GetDataCell("E#" & sEntity & ":A#DPOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					'borrado de cuentas con dato en WCAP
					api.Data.ClearCalculatedData("A#2721000CF:F#F14:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#2721000CF:F#F14:U4#None", True, True, True, True)
					api.Data.ClearCalculatedData("A#1681000CF:U4#WCAP:F#F30", True, True, True, True) 'salida caja
					api.Data.ClearCalculatedData("A#1561021:F#F14:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#2301710:F#F20:U4#WCAP", True, True, True, True)	'VAT
'					api.Data.ClearCalculatedData("A#2901710:F#F14:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#COGS_VAT:U4#WCAP", True, True, True, True)
					
					api.Data.ClearCalculatedData("A#COGS_Country", True, True, True, True)
					api.Data.ClearCalculatedData("A#1561021:F#F20:U4#WCAP", True, True, True, True)
					
					' Usamos una cuenta temporal para tener la suma de los COGS de las cajas del pais	
					 api.Data.Calculate("A#COGS_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP = " & sCOGSCountry, True)
' MA cambio DPO ---  api.Data.Calculate("A#COGS_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP = (-1)*" & sCOGSCountry, True)
					
'					 BRApi.ErrorLog.LogMessage(si, "sCOGSCountry: " & sCOGSCountry)
					 
					'_________________Se divide el calculo entre IC None e ICEntities porque el dato de las cajas se quiere guardar en la 001_____________
					
					'____________________________________________________ Calc ICNone ____________________________________________________________________
					'


					
					' Calculo de COGS con IVA - COGS_VAT
'					api.Data.Calculate("A#COGS_VAT:V#YTD:F#F14:U4#WCAP:I#None = ((A#COGS_Acc:E#" & sParent & ":V#Periodic:F#None:U4#Top:I#None * " & WVATPurchases & " * (1 + " & VAT & ")) + (A#COGS_Acc:E#" & sParent & ":V#Periodic:F#None:U4#Top:I#None * ( 1 - " & WVATPurchases & " )))", True)
'					api.Data.Calculate("A#COGS_VAT:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None = (((" & sCOGSNone & ") * (-1) * " & WVATPurchases & " * (1 + " & VAT & ")) + ((" & sCOGSNone & ") * (-1) * ( 1 - " & WVATPurchases & " )))", True)
					
'''					api.Data.Calculate("A#COGS_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None = 
'''											((A#COGS_Country:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#None * (-1) * " & WVATPurchases & " * (1 + " & VAT & ")) + 
'''											 (A#COGS_Country:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#None * (-1) * ( 1 - " & WVATPurchases & " )))", True)					
					api.Data.Calculate("A#COGS_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None = 
											((A#COGS_Country:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#None * " & WVATPurchases & " * (1 + " & VAT & ")) + 
											 (A#COGS_Country:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#None * ( 1 - " & WVATPurchases & " )))", True)											 

					' Calculo de payables -2721000CF (no se tiene en cuenta el IVA)
'					api.Data.Calculate("A#2721000CF:V#YTD:F#F14:U4#WCAP:I#None = ((A#COGS_Acc:E#" & sParent & ":V#Periodic:F#None:U4#Top:I#None /30 * " & DPOp & ") 
'										- A#2721000CF:E#" & sParent & "001:V#YTD:F#F00:U4#None:I#None))", True)

					api.Data.Calculate("A#2721000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None = 
										((-1)*(A#COGS_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None / 30 * " & DPOp & ") 
										- A#2721000CF:E#" & sParent & "001:V#YTD:F#F00:U1#None:U2#None:U4#None:I#None))", True)
					
'					api.Data.Calculate("A#2721000CF:V#YTD:F#F14:U4#WCAP:I#None = A#COGS_Acc:E#" & sParent & ":V#Periodic:F#None:U4#Top:I#None /30 * " & DPOp , True)
					
					'____________________________________________ Calc ICEntities ________________________________________________________________________							
					
					Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
					
					' Recuperamos la moneda de la entidad principal y la moneda común
					Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
					
					Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
					Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
					Dim commonCurrencyID As Integer = Currency.EUR.Id
					
					' Definimos variables necesarias para el conseguir el tipo de cambio
					'Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForAssetLiability() ' Average Rate
					Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
					Dim cubeId As Integer = api.Pov.Cube.CubeId
					Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
					
					For Each ICMember As Member In ICList
						
						Dim sIC As String = ICMember.Name.ToString
						Dim sParentIC As String = left(ICMember.Name.ToString, 5)
						
						' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
						Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
						Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)
						Dim bsRate As Decimal = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID)
						
						
	'					BRAPI.ErrorLog.LogMessage(si, $"{entityICCurrencyName} to {entityPOVCurrencyName}: {revRate}")
						
	
						Dim receivablesICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & 
																		":A#1541000CF:T#" & sTime & ":O#Import:V#YTD:I#" & sEntity & 
																		":F#F14:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
	
'									BRApi.ErrorLog.LogMessage(si, "receivablesICPAmount: " & receivablesICPAmount)
					
									
						' Calculamos
						If (receivablesICPAmount <> 0)					
							
							'Account Payables - 2721000CF (UD4 = WCAP)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#{sEntity}:O#Import" &
								$"* {bsRate}",True
							)	
							
							'Account Payables - 2721000CF (UD4 = None)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U4#None:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#None:I#{sEntity}:O#Import" &
								$"* {bsRate}",True
							)	
						End If
						
					
					Next	
					
					'VAT Calc					 				
						'Account payables VAT - 2301710
'''						api.Data.Calculate("A#2301710:V#Periodic:F#F20:U1#None:U2#None:U4#WCAP:I#None = A#COGS_Country:I#Top:V#Periodic * " & WVATPurchases & " * " & VAT & "", True)
						api.Data.Calculate("A#1561021:V#Periodic:F#F20:U1#None:U2#None:U4#WCAP:I#None = (-1)*A#COGS_Country:I#Top:V#Periodic * " & WVATPurchases & " * " & VAT & "", True)
						
					'calc Cash - 1681000CF
						'api.data.calculate("A#1681000CF:F#F15:U4#WCAP= ((A#1541000CF:F#F30:U4#WCAP + A#1541000CF:F#F16:U4#WCAP) * -1) + (A#2721000CF:F#F30:U4#WCAP + A#2721000CF:F#F16:U4#WCAP) + ((A#1501000CF:F#F30:U4#WCAP + A#1501000CF:F#F16:U4#WCAP + A#1501000CF:F#F20:U4#WCAP) * -1) ")
'						api.data.calculate("A#1681000CF:V#YTD:F#F30:U1#None:U2#None:U4#WCAP:I#None = A#2721000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#Top -  A#COGS_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#Top", True)
'					api.data.calculate("A#1681000CF:V#YTD:F#F30:U1#None:U2#None:U4#WCAP:I#None = 
'											A#COGS_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#Top + 
'											A#2721000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#Top", True)
					

'''					api.data.calculate("A#1681000CF:V#Periodic:F#F30:U1#None:U2#None:U4#WCAP:I#None = 
'''											(A#COGS_Country:E#" & sParent & "001:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#Top +
'''											A#2301710:V#Periodic:F#F20:U1#None:U2#None:U4#WCAP:I#None +
'''											A#2721000CF:V#Periodic:F#F14:U1#None:U2#None:U4#WCAP:I#Top)", True)
											
					api.data.calculate("A#1681000CF:V#Periodic:F#F30:U1#None:U2#None:U4#WCAP:I#None = 
											(A#COGS_Country:E#" & sParent & "001:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#Top -
											A#1561021:V#Periodic:F#F20:U1#None:U2#None:U4#WCAP:I#None +
											A#2721000CF:V#Periodic:F#F14:U1#None:U2#None:U4#WCAP:I#Top)", True)
																	
					
											
											
				End If 'entidad 001(-1) 
			End If 'local
		End If 'has children
	End Sub
	
#End Region

#Region "DSO - DPO - DIO Split Test"

	Sub WCAP_DSO (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		'-----------------------------------------------------------------------------------
		' VARIABLES INICIALES
		'-----------------------------------------------------------------------------------				
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name	
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		
		'-----------------------------------------------------------------------------------
		' RECEIVABLES y COGS_ACC (Valores acumulados)
		'-----------------------------------------------------------------------------------
		Dim sReceivablesCountry As String = String.Empty
		Dim sCOGSCountry As String = String.Empty
		
		Dim i As Integer = 0
				
		For Each entityMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sParent & ".Base", True)
								
			If i <> 0 Then

				sReceivablesCountry = sReceivablesCountry & " + "
				sCOGSCountry = sCOGSCountry & " + "


			End If

			sReceivablesCountry = sReceivablesCountry & "RemoveZeros(E#" & entityMember.Member.Name & ":A#Receivables_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top)"
			sCOGSCountry = sCOGSCountry & "RemoveZeros(E#" & entityMember.Member.Name & ":A#COGS_Acc:V#Periodic:F#None:U1#Top:U2#Top:U4#Top)"			
			
			i = i+1
			
		 Next
		 
		'-----------------------------------------------------------------------------------
		' CALCULO
		' Se divide el calculo entre IC None e ICEntities porque el dato de las cajas se quiere guardar en la 001
		'-----------------------------------------------------------------------------------

		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If api.pov.Entity.name.endswith("001") Then
					
					Dim VAT = api.Data.GetDataCell("E#" & sParent & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim WVATSales = api.Data.GetDataCell("E#" & sParent & "001:A#WVAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
						
					Dim DSOp = api.Data.GetDataCell("E#" & sEntity & ":A#DSOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim DIOp = api.Data.GetDataCell("E#" & sEntity & ":A#DIOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
							
					'Borrado de cuentas con dato en WCAP
					api.Data.ClearCalculatedData("U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#1541000CF:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#1681000CF:U4#WCAP:F#F20", True, True, True, True)
					api.Data.ClearCalculatedData("A#1561021:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#2901710:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#Turnover_VAT:U4#WCAP", True, True, True, True)
					api.Data.ClearCalculatedData("A#Receivables_Acc_Country", True, True, True, True)
					api.Data.ClearCalculatedData("A#COGS_Country", True, True, True, True)
					
					
					
					' Usamos una cuenta temporal para tener la suma de los revenues y de los cogs de las cajas del pais	
					 api.Data.Calculate("A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP = " & sReceivablesCountry, True)
					 api.Data.Calculate("A#COGS_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP = " & sCOGSCountry, True)
					
					'########################################################################################
					' Calculo con  ICNone 
					'########################################################################################
					
					'calc F14	

					' Calculo de ventas con IVA - Turnover_VAT
					api.Data.Calculate("A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None = ((A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None * "  & WVATSales & " * (1 + " & VAT & ")) + (A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None * ( 1 - " & WVATSales & " )))", True)
				
					
					' Calculo de receivables -1541000CF (no se tiene en cuenta el IVA)
					api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None = ((A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None / 30 * " & DSOp & ") 
										- A#1541000CF:E#" & sParent & "001:V#YTD:F#F00:U1#None:U2#None:U4#None:I#None))", True)
										
					'########################################################################################
					' Calculo con ICEntities 
					'########################################################################################					
			
					Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
								
					For Each ICMember As Member In ICList
						
						Dim sIC As String = ICMember.Name.ToString
						Dim sParentIC As String = left(ICMember.Name.ToString, 5)
	
						'VAT y WVAT de Interco
						Dim VAT_IC = api.Data.GetDataCell("E#" & sParentIC & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
										
						' Calculo de ventas con IVA
						api.Data.Calculate("A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = ((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * " & WVATSales & " * (1 + " & VAT & ")) + (A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * ( 1 - " & WVATSales & " )))", True)
										

						If (MesCorriente = 1)
						
							'calc F14
								'Account Receivables - 1541000CF
								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = 
														A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 + 
														(((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * " & WVATSales & " * (1 + " & VAT & ") + 
														   A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * (1 - " & WVATSales & ")) /30 * " & DSOp & ")) 
															- A#1541000CF:E#" & sParent & "001:V#YTD:F#F00:U1#Top:U2#Top:U4#None:I#" & sIC & ")", True)
								
						Else
							'calc F14
								'Account Receivables - 1541000CF
								api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 = 
														A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#" & sParentIC & "001 - 
														A#1541000CF:V#YTD:T#POVPrior1:F#F14:U1#None:U2#None:U4#WCAP:I#" & sIC & " + 
														(((A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * " & WVATSales & " * (1 + " & VAT & ") + 
														   A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#" & sIC & " * (1 - " & WVATSales & ")) /30 * " & DSOp & ")) 
														- A#1541000CF:V#YTD:F#F00:U1#None:U2#None:U4#None:I#" & sIC & ")", True)
						End If
							
					Next
					
					
					'VAT Calc					 				
						'Account Receivables VAT - 2901710
						api.Data.Calculate("A#2901710:V#Periodic:F#F20:U4#WCAP:I#None:U1#None:U2#None = 
												A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U4#Top:I#Top * " & WVATSales & " * " & VAT & "", True)
					
					'Calc Cash - 1681000CF
					api.data.calculate("A#1681000CF:V#Periodic:F#F20:U4#WCAP:U1#None:U2#None:I#None = A#Turnover_VAT:E#" & sParent & "001:V#Periodic:F#None:U1#NOne:U2#None:U4#WCAP:I#Top - A#1541000CF:V#Periodic:F#F14:U1#None:U2#None:U4#WCAP:I#Top" , True)
					
					'Calculo F99I Se recalcula porque el calculo de F99I se lanza antes que WCAP
					api.data.calculate("F#F99I:U4#WCAP=F#F99:U4#WCAP",True)
							
				End If 'entidad 001
			End If 'local
		End If 'has children		 
	
	End Sub

	Sub WCAP_DIO (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		'-----------------------------------------------------------------------------------
		' VARIABLES INICIALES
		'-----------------------------------------------------------------------------------				
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name	
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		
		Dim VAT = api.Data.GetDataCell("E#" & sParent & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
		Dim WVATSales = api.Data.GetDataCell("E#" & sParent & "001:A#WVAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
			
		Dim DSOp = api.Data.GetDataCell("E#" & sEntity & ":A#DSOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
		Dim DIOp = api.Data.GetDataCell("E#" & sEntity & ":A#DIOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
				

		'-----------------------------------------------------------------------------------
		' RATIO con el que haremos la distribución
		' COGS_Acc / COGS_Country
		'-----------------------------------------------------------------------------------
		
'		Dim ratio As String = api.data.GetDataCell("(E#"& sEntity &":A#COGS_Acc:V#Periodic:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None" &" / "& "E#"& sParent &"001:A#COGS_Country:V#Periodic:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top )").CellAmount.ToString(cultureinfo.InvariantCulture)
'		Dim ratioNumero As String = api.Data.GetDataCell(ratio).CellAmount.ToString(cultureinfo.InvariantCulture)
		
		'-----------------------------------------------------------------------------------
		' CALCULO
		'-----------------------------------------------------------------------------------
		
		api.Data.ClearCalculatedData("A#1501000CF:U4#WCAP", True, True, True, True)
		api.Data.ClearCalculatedData("A#551104:U4#WCAP", True, True, True, True)
					
		
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If api.pov.Entity.name.endswith("001") Then
					
					' Calculo de inventarios -1501000CF (no se tiene en cuenta el IVA)
					api.Data.Calculate("A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None= ((A#COGS_Country:V#Periodic:F#None:U1#None:U2#None:U4#WCAP:I#None * (-1) / 30 * " & DIOp & ") - A#1501000CF:E#" & sParent & "001:V#YTD:F#F00:U1#None:U2#None:U4#None:I#None))", True)
					api.Data.Calculate("A#551104:V#YTD:F#None:U1#None:U2#None:U4#WCAP:I#None= A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U4#WCAP:I#None", True)
					
				End If
			End If
		End If
		
		' Incluido también en la lista Consumption of materials and parts - A falta de validar cuando de dato					
		' api.Data.Calculate("E#"& sEntity &":A#551104:V#YTD:F#None:U1#None:U3#None:U2#None:U4#WCAP:I#None:O#Import = E#"& sParent &"001:A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP:I#None:O#Import * "&ratioNumero, True)

			' Tengo que preguntar si esta parte hace falta --------------------------------------- 
					'calculo F99I Se recalcula porque el calculo de F99I se lanza antes que WCAP
					'api.data.calculate("F#F99I:U4#WCAP=F#F99:U4#WCAP",True)
			' ------------------------------------------------------------------------------------
		
	End Sub
	
	Sub WCAP_DIO_2 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		'-----------------------------------------------------------------------------------
		' VARIABLES INICIALES
		'-----------------------------------------------------------------------------------				
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name	
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		
		'-----------------------------------------------------------------------------------
		' RATIO con el que haremos la distribución
		' COGS_Acc / COGS_Country
		'-----------------------------------------------------------------------------------
		
		Dim ratio As String = api.data.GetDataCell("(E#"& sEntity &":A#COGS_Acc:V#Periodic:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#None" &" / "& "E#"& sParent &"001:A#COGS_Country:V#Periodic:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top )").CellAmount.ToString(cultureinfo.InvariantCulture)
		Dim ratioNumero As String = api.Data.GetDataCell(ratio).CellAmount.ToString(cultureinfo.InvariantCulture)
		
		'-----------------------------------------------------------------------------------
		' CALCULO
		'-----------------------------------------------------------------------------------

		api.Data.ClearCalculatedData("A#551104:U4#WCAP", True, True, True, True)
					
		' Incluido también en la lista Consumption of materials and parts - A falta de validar cuando de dato					
		api.Data.Calculate("E#"& sEntity &":A#551104:V#YTD:F#None:U1#None:U3#None:U2#None:U4#WCAP:I#None:O#Import = E#"& sParent &"001:A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP:I#None:O#Import * "&ratioNumero, True)

			' Tengo que preguntar si esta parte hace falta --------------------------------------- 
					'calculo F99I Se recalcula porque el calculo de F99I se lanza antes que WCAP
					'api.data.calculate("F#F99I:U4#WCAP=F#F99:U4#WCAP",True)
			' ------------------------------------------------------------------------------------
		
	End Sub

#End Region	

#Region "Borrado"
	Sub borradoUD  (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	
		
		api.Data.ClearCalculatedData("U4#WCAP",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#PERS",True,True,True,True)
		api.Data.ClearCalculatedData("U4#INTER",True,True,True,True)
		api.Data.ClearCalculatedData("U4#WRTY",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#IFRS16",True,True,True,True)
		api.Data.ClearCalculatedData("U4#CORP",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#CAPEX",True,True,True,True)
		api.Data.ClearCalculatedData("U4#DSOTSA",True,True,True,True)
		api.Data.ClearCalculatedData("U4#VAT",True,True,True,True)	
	
	End Sub
#End Region

	End Class
End Namespace