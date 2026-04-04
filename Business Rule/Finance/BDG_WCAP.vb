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

Namespace OneStream.BusinessRule.Finance.BDG_WCAP
	Public Class MainClass

#Region "Funcion Inicial"
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
				
				If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then
					
					' BORRA - las UD4 de los cálculos de: AMORT, VAT, IFRS16, Interest, Personnel, Warranty, CorpRate, Payable_TSA y WCAP (está comentado)
					If sFunction = "BorrarUD_WCAP" Then
					
						Me.borradoUD_WCAP(si, api, globals, args)
						
					'CALCULATE DATA - WCAP (DSO)
					Else If sFunction = "DSO"							
						
						Me.WCAP_DSO(si, api, globals, args)	
						
					'CALCULATE DATA - WCAP PAYABLES (DPO)
					Else If sFunction = "DPO"							
						
						Me.WCAP_DPO(si, api, globals, args)
					
					'CALCULATE DATA - WCAP (DIO)
					Else If sFunction = "DIO"							
						
						Me.WCAP_DIO(si, api, globals, args)	
						
					End If							
				End If
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region
		
#Region "Borrado"
	Sub borradoUD_WCAP  (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	
		
		api.Data.ClearCalculatedData("U4#WCAP_DSO",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#WCAP_DPO",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#WCAP_DIO",True,True,True,True)			
	
	End Sub
#End Region
		
#Region "DSO"
	' Calculo DSO
	Sub WCAP_DSO (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name
	
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		
'		api.Data.ClearCalculatedData("U4#WCAP_DSO", True, True, True, True)
' 		clearCalculatedData, clearTranslatedData, clearConsolidatedData, clearDurableCalculatedData, accountFilter, flowFilter, originFilter, icFilter, ud1Filter, ud2Filter, ud3Filter, ud4Filter, ud5Filter, ud6Filter, ud7Filter, ud8Filter

		api.Data.ClearCalculatedData(True, True, True, True,,"F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#WCAP_DSO",,,,)

		'-----------------------------------------------------------------------------------
		' Calculo del importe sumando las cajas de cada pais
		'-----------------------------------------------------------------------------------
		Dim sReceivablesCountry As String = String.Empty
		
		Dim i As Integer = 0
				
		For Each entityMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sParent & ".Base", True)
								
			If i <> 0 Then
				
				sReceivablesCountry = sReceivablesCountry & " + "

			End If
			
			sReceivablesCountry = sReceivablesCountry & "RemoveZeros(E#" & entityMember.Member.Name & ":A#Receivables_Acc:F#None:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"		
			
			i = i+1
			
		 Next	
		'----------------------------------------------------------------------------------------------------------------------------------------------------------------------
		
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If api.pov.Entity.name.endswith("001") Then
					
					Dim VAT = api.Data.GetDataCell("E#" & sParent & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim WVATSales = api.Data.GetDataCell("E#" & sParent & "001:A#WVAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
						
					Dim DSOp = api.Data.GetDataCell("E#" & sEntity & ":A#DSOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					Dim DIOp = api.Data.GetDataCell("E#" & sEntity & ":A#DIOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
							
					'Borrado de cuentas con dato en WCAP
'					api.Data.ClearCalculatedData("A#Receivables_Acc_Country:U4#WCAP_DSO", True, True, True, True) 'Turnover_COUNTRY 
					api.Data.ClearCalculatedData(True, True, True, True,"A#Receivables_Acc_Country", "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#WCAP_DSO",,,,)
					
					' Cuenta temporal para tener la agrupar los Revenues y de los COGS de las cajas del pais	
					 api.Data.Calculate("A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DSO = " & sReceivablesCountry, True) 'Turnover_COUNTRY
					
					' * Se divide el calculo entre IC None e ICEntities porque el dato de las cajas se quiere guardar en la 001, tanto de entities como de ICs
					
					' ---------------------------------------------------------------------	
					' Calc ICNone
					' ---------------------------------------------------------------------		
					
					' Calculo de ventas con IVA - Turnover_VAT
					api.Data.Calculate("A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#None = 
												((A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#None * "  & WVATSales & " * (1 + " & VAT & ")) 
												+ (A#Receivables_Acc_Country:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#None * ( 1 - " & WVATSales & " )))", True)
				
					
					' Calculo de receivables -1541000CF (no se tiene en cuenta el IVA)
					api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#None = 
												A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#None / 30 * " & DSOp & "
												- A#02AC03:E#" & sParent & "001:V#Periodic:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:I#None", True)
								
					' ---------------------------------------------------------------------	
					' Calc ICEntities
					' ---------------------------------------------------------------------									
								
					Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
					Dim Turnover_COUNTRY As String = String.Empty
					Dim j As Integer = 0
					Dim parentDicc As New Dictionary(Of String, String)
					
					For Each ICMember As Member In ICList
						
						Dim sIC As String = ICMember.Name.ToString
						Dim sParentIC As String = left(ICMember.Name.ToString, 5)
						
						If Not parentDicc.ContainsKey(sParentIC) Then
							
							parentDicc.Add(sParentIC, "A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U3#Top:U4#Top:I#" & sIC &"")
						
						Else 
							
							parentDicc.Item(sParentIC) = parentDicc.Item(sParentIC) & " + " & "A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U3#Top:U4#Top:I#" & sIC &""
							
						End If
						
						j += 1
						
					Next
					
					For Each sParentIC In parentDicc.Keys

						Turnover_COUNTRY = parentDicc.Item(sParentIC)
						
						' Calculo de ventas con IVA
						api.Data.Calculate("E#"& sEntity &":A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#" & sParentIC & "001 = 
													(("& "("& Turnover_COUNTRY &")" & " * " & WVATSales & " * (1 + " & VAT & ")) + ("& "("& Turnover_COUNTRY &")" & " * ( 1 - " & WVATSales & " )))", True)
													
						api.Data.Calculate("A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#" & sParentIC & "001 = 
													A#Turnover_VAT:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#" & sParentIC & "001 /30 * " & DSOp & "
													- A#02AC03:E#" & sParent & "001:V#Periodic:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:I#" & sParentIC & "001", True)	
					Next				
					
					' ---------------------------------------------------------------------	
					' Calculos - VAT y Cash, a INone desde ITop
					' ---------------------------------------------------------------------	

					' VAT Calc					 				
					'Account Receivables VAT - 2901710
					api.Data.Calculate("A#2901710:V#Periodic:F#F20:U4#WCAP_DSO:I#None:U1#None:U2#None:U3#None = 
											A#Receivables_Acc_Country:E#" & sParent & "001:V#Periodic:F#None:U1#Top:U2#Top:U3#Top:U4#Top:I#Top * " & WVATSales & " * " & VAT & "", True)
				
					' Cash Calc - 1681000CF
					api.data.calculate("A#1681000CF:V#Periodic:F#F16:U4#WCAP_DSO:U1#None:U2#None:U3#None:I#None =
												A#Turnover_VAT:E#" & sParent & "001:V#Periodic:F#None:U1#NOne:U2#None:U3#None:U4#WCAP_DSO:I#Top 
												- A#1541000CF:V#Periodic:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DSO:I#Top" , True)
					
					' Calculo F99I Se recalcula porque el calculo de F99I se lanza antes que WCAP
					api.data.calculate("F#F99I:U4#WCAP_DSO=F#F99:U4#WCAP_DSO",True)
					
				End If 'entidad 001
			End If 'local
		End If 'has children
	End Sub
#End Region

#Region "DPO"
	' Calculo DPO
	Sub WCAP_DPO (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		
'		api.Data.ClearCalculatedData("U4#WCAP_DPO", True, True, True, True)
		api.Data.ClearCalculatedData(True, True, True, True,,"F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#WCAP_DPO",,,,)
		
		'-----------------------------------------------------------------------------------
		' Calculo del importe sumando las cajas de cada pais
		'-----------------------------------------------------------------------------------
		Dim sCOGSCountry As String = String.Empty
		Dim sOPMargin_Net As String = String.Empty
		
		Dim i As Integer = 0
				
		For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sParent & ".Base", True)
								
			If i <> 0 Then
				sCOGSCountry = sCOGSCountry & " + "
				sOPMargin_Net = sOPMargin_Net & " + "
			End If
					
			sCOGSCountry = sCOGSCountry & "RemoveZeros(E#" & miMember.Member.Name & ":A#COGS_Acc:F#None:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"				
			sOPMargin_Net = sOPMargin_Net & "RemoveZeros(E#" & miMember.Member.Name & ":A#OPMargin_Net:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:I#Top)"
			
			i = i+1
			
		Next	
			
		'----------------------------------------------------------------------------------------------------------------------------------------------------------------------
		
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If api.pov.Entity.name.endswith("001") Then
					
					Dim VAT = api.Data.GetDataCell("E#" & sParent & "001:A#VAT:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)			
					Dim WVATPurchases = api.Data.GetDataCell("E#" & sParent & "001:A#WVATP:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					Dim DPOp = api.Data.GetDataCell("E#" & sEntity & ":A#DPOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
					
					'Borrado de cuentas con dato en WCAP
					api.Data.ClearCalculatedData("A#2721000CF:F#F14:U4#None", True, True, True, True)
'					api.Data.ClearCalculatedData("A#COGS_Country:U4#WCAP_DPO", True, True, True, True)
					api.Data.ClearCalculatedData(True, True, True, True,"A#COGS_Country", "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#WCAP_DPO",,,,)
					
					' Usamos una cuenta temporal para tener la suma de los COGS de las cajas del pais	
					 api.Data.Calculate("A#COGS_Country:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DPO = " & sCOGSCountry, True)
					
					 api.Data.Calculate("A#OPMargin_Net_Country:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#None = " & sOPMargin_Net, True)
					
					' Calculo de COGS con IVA - COGS_VAT
					api.Data.Calculate("A#COGS_VAT:V#Periodic:F#None:U1#None:U2#None:U3#None:U4#WCAP_DPO = 
											((A#COGS_Country:V#Periodic:F#None:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO * " & WVATPurchases & " * (1 + " & VAT & ")) + 
											 (A#COGS_Country:V#Periodic:F#None:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO * ( 1 - " & WVATPurchases & " )))", True)											 
					
					' ---------------------------------------------------------------------	
					' Calc ICNone
					' ---------------------------------------------------------------------	
					api.Data.Calculate("A#2721000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#None = 
										((-1)*(A#COGS_VAT:V#Periodic:F#None:U1#None:U2#None:U3#Top:U4#WCAP_DPO:I#None / 30 * " & DPOp & "))
										- A#05PC05:V#Periodic:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:I#None", True)
										
					' ---------------------------------------------------------------------	
					' Calc ICEntities
					' ---------------------------------------------------------------------								
					Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
					
					' Recuperamos la moneda de la entidad principal y la moneda común
					Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
					Dim entityPOVCurrencyName As String = api.Entity.GetLocalCurrency(sEntityPOVID).Name
					Dim entityPOVCurrencyID As Integer = api.Entity.GetLocalCurrencyId(sEntityPOVID)
					Dim commonCurrencyID As Integer = Currency.EUR.Id
					
					' Definimos variables necesarias para el conseguir el tipo de cambio
					Dim rateType As FxRateType = api.FxRates.GetFxRateTypeForRevenueExp() ' Average Rate
					Dim cubeId As Integer = api.Pov.Cube.CubeId
					Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
					
					For Each ICMember As Member In ICList
						
						Dim sIC As String = ICMember.Name.ToString
						Dim sParentIC As String = left(ICMember.Name.ToString, 5)
						
						' Obtenemos la moneda de la IC entity y el tipo de cambio indirecto usando el euro como moneda común
						Dim entityICCurrencyName As String = api.Entity.GetLocalCurrency(ICMember.MemberId).Name
						Dim entityICCurrencyID As Integer = api.Entity.GetLocalCurrencyId(ICMember.MemberId)											
						Dim bsRate As String = api.FxRates.GetCalculatedFxRateEx(rateType, commonCurrencyID, timeId, entityICCurrencyID, entityPOVCurrencyID).ToString(cultureInfo.InvariantCulture)					
						
						Dim receivablesICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & 
																		":A#1541000CF:T#" & sTime & ":V#YTD:I#" & sEntity & 
																		":F#F14:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)	
			
						Dim receivablesICPAmountPERIODIC = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & 
																		":A#1541000CF:T#" & sTime & ":V#Periodic:I#" & sEntity & 
																		":F#F14:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)	
						
						Dim receivablesICPAmountFORMS = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & 
																		":A#1541000CF:T#" & sTime & ":O#Forms:V#Periodic:I#" & sEntity & 
																		":F#F14:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)	
						
		
						' Calculamos
						If (receivablesICPAmount <> 0)	Then ' Caso normal en el que hay un YTD > 0 con dato en Origin Import
							
							'Account Payables - 2721000CF (UD4 = WCAP)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP:I#{sEntity}:O#Top" &
								$"* {bsRate}",True
							)

							'Account Payables - 2721000CF (UD4 = None)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#None:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#None:I#{sEntity}:O#Top" &
								$"* {bsRate}",True
							)
							
							'Account Payables - 2721000CF (UD4 = Manual_Input)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F16:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F16:U1#None:U2#None:U3#None:U4#Manual_Input:I#{sEntity}:O#Top" &
								$"* {bsRate}",True
							)
							
						Else If (receivablesICPAmountPERIODIC <> 0) Then ' Caso normal en el que hay un YTD = 0, Periodic > 0, con dato en Origin Import
							'Account Payables - 2721000CF (UD4 = WCAP)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP:I#{sEntity}:O#Top" &
								$"* {bsRate} + 0",True
							)

							'Account Payables - 2721000CF (UD4 = None)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#None:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#None:I#{sEntity}:O#Top" &
								$"* {bsRate} + 0",True
							)
							
							'Account Payables - 2721000CF (UD4 = Manual_Input)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F16:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F16:U1#None:U2#None:U3#None:U4#Manual_Input:I#{sEntity}:O#Top" &
								$"* {bsRate}",True
							)
						
						Else If (receivablesICPAmountFORMS <> 0) Then ' El YTD=0 y No hay dato en Import, tan solo en Forms 
							'Account Payables - 2721000CF (UD4 = WCAP)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP:I#{sEntity}:O#Top" &
								$"* {bsRate}",True
							)

							'Account Payables - 2721000CF (UD4 = None)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#None:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#None:I#{sEntity}:O#Top" &
								$"* {bsRate}",True
							)
							
							'Account Payables - 2721000CF (UD4 = Manual_Input)
							api.Data.Calculate(
								$"A#2721000CF:C#{entityPOVCurrencyName}:V#YTD:F#F16:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#{sParentIC}001:O#Import =" &
								$"E#{sParentIC}001:C#{entityICCurrencyName}:A#1541000CF:V#YTD:F#F16:U1#None:U2#None:U3#None:U4#Manual_Input:I#{sEntity}:O#Top" &
								$"* {bsRate}",True
							)
																		
										
						End If				

					Next	
					
					'VAT Calc					 				
					'Account payables VAT - 1561021
					api.Data.Calculate("A#1561021:V#Periodic:F#F20:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#None = (-1)*A#COGS_Country:I#Top:V#Periodic * " & WVATPurchases & " * " & VAT & "", True)
					
					api.Data.ClearCalculatedData("A#1681000CF:V#Periodic:F#F16:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#None", True, True, True, True)

					'Cash Calc - 1681000CF											
					api.data.calculate("A#1681000CF:V#Periodic:F#F16:U1#None:U2#None:U3#None:U4#WCAP_DPO:I#None = 
											(-1)*(A#1561021:V#Periodic:F#F20:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO:I#Top -
											A#2721000CF:V#Periodic:F#F14:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO:I#Top -
											A#2721000CF:V#Periodic:F#F16:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO:I#Top -					
											A#OPMargin_Net_Country:V#Periodic:F#None:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO:I#Top	)							
											", True)

					API.Data.Calculate("U4#WCAP_DPO:F#F99I = U4#WCAP_DPO:F#F99" , True)						
				End If 'entidad 001(-1) 
			End If 'local
		End If 'has children
	End Sub
	
#End Region

#Region "DIO"

	' Calculo DIO
	Sub WCAP_DIO (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
	    
		' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
		' Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		' Dim sEntity As String = api.pov.Entity.Name
		' Dim sParent As String = left(sEntity, 5)
		' Dim sScenario As String = api.Pov.Scenario.Name
		' Dim sTime As String = api.Pov.Time.Name
		' 
		' Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		' Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		' Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		' 
		' api.Data.ClearCalculatedData("U4#WCAP_DIO", True, True, True, True)
		' 
		' If Not api.Entity.HasChildren() Then
		' 	If api.Cons.IsLocalCurrencyforEntity() Then
		' 		If api.pov.Entity.name.endswith("001") Then
		' 			
		' 			Dim DIOp = api.Data.GetDataCell("E#" & sEntity & ":A#DIOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
		' 
		' 			' ---------------------------------------------------------------------	
		' 			' Calc ICNone
		' 			' ---------------------------------------------------------------------		
		' 			' Calculo de inventarios -1501000CF (no se tiene en cuenta el IVA)
		' 			api.Data.Calculate("A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DIO:I#None= 
		' 										((A#COGS_Country:V#Periodic:F#Top:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO:I#Top * (-1) / 30 * " & DIOp & ") 
		' 										- A#02AC01:V#Periodic:F#F00:U1#Top:U2#Top:U3#Top:U4#Top:I#Top))", True)
		' 								
		' 			api.Data.Calculate("A#1681000CF:V#YTD:F#F30:U1#None:U2#None:U3#None:U4#WCAP_DIO:I#None= 
		' 										(-1)*A#1501000CF:V#YTD:F#F14:U1#Top:U2#Top:U3#Top:U4#WCAP_DIO:I#Top", True)	
		' 
		' 			' Calculo F99I Se recalcula porque el calculo de F99I se lanza antes que WCAP
		' 			api.data.calculate("F#F99I:U4#WCAP_DIO=F#F99:U4#WCAP_DIO",True)	
		' 			
		' 		End If 'entidad 001
		' 	End If 'local
		' End If 'has children
		'
		' DUDA: En el MeFcstNumber, para el Budget, que mes debo poner, entendiendo que es para uno solo.
		' --------------------------------------------------------------------------------------------------------------------------------------
		
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
		Dim sEntity As String = api.pov.Entity.Name
		Dim sParent As String = left(sEntity, 5)
		Dim sScenario As String = api.Pov.Scenario.Name
		Dim sTime As String = api.Pov.Time.Name
		Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		Dim YearCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(0))
		Dim priorPeriod As String = $"{YearCorriente}M{MesCorriente - 1}"
		Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
		Dim year As String = sTime.Substring(0,4)
		Dim Flujo As String = String.Empty
		
		If ScenarioType = "Budget" Then
			Flujo = "F00"	
			MesFcstNumber = "1"
		Else If (ScenarioType = "Forecast" And sMes > MesFcstNumber) Then		
			Flujo = "F99"
		End If
		
'		api.Data.ClearCalculatedData("U4#WCAP_DIO", True, True, True, True)
		api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#WCAP_DIO",,,,)
		
		If Not api.Entity.HasChildren() Then
			If api.Cons.IsLocalCurrencyforEntity() Then
				If api.pov.Entity.name.endswith("001") And (ScenarioType = "Budget" Or (ScenarioType = "Forecast" And sMes > MesFcstNumber)) Then
					
					Dim DIOp = api.Data.GetDataCell("E#" & sEntity & ":A#DIOp:S#" & sScenario & ":T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)

					' ---------------------------------------------------------------------	
					' Calc ICNone
					' ---------------------------------------------------------------------		
					' Calculo de inventarios -1501000CF (no se tiene en cuenta el IVA)
					api.Data.Calculate("A#1501000CF:V#YTD:F#F14:U1#None:U2#None:U3#None:U4#WCAP_DIO:I#None= 
												(( (-1) * A#COGS_Country:V#Periodic:F#Top:U1#Top:U2#Top:U3#Top:U4#WCAP_DPO:I#Top / 30 * " & DIOp & ") 
												- A#02AC01:V#Periodic:F#" & Flujo & ":T#"& year &"M"& MesFcstNumber &":U1#Top:U2#Top:U3#Top:U4#Top:I#Top))", True)
										
					api.Data.Calculate("A#1681000CF:V#YTD:F#F16:U1#None:U2#None:U3#None:U4#WCAP_DIO:I#None= 
												(-1)*A#1501000CF:V#YTD:F#F14:U1#Top:U2#Top:U3#Top:U4#WCAP_DIO:I#Top", True)	


					' Calculo F99I Se recalcula porque el calculo de F99I se lanza antes que WCAP
					api.data.calculate("F#F99I:U4#WCAP_DIO=F#F99:U4#WCAP_DIO",True)	
					
				End If 'entidad 001
			End If 'local
		End If 'has children

	End Sub
	
#End Region

	End Class
End Namespace