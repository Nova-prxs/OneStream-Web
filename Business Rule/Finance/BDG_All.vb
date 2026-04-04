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

Namespace OneStream.BusinessRule.Finance.BDG_All
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				#Region "Functions"
				
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
					If sFunction = "BorrarUDs" Then					
						Me.borradoUD(si, api, globals, args)						
					'CALCULATE DATA - volume * unitary
					' Else If sFunction = "Budget_P_X_Q"				
					' 	Me.Budget_P_X_Q(si, api, globals, args)	
					'CALCULATE DATA - volume * unitary Material Cost
					Else If sFunction = "P_X_Q_ICP"							
					 	Me.Budget_P_X_Q_ICP(si, api, globals, args)	
					Else If sFunction = "P_X_Q_Manual_ADJ"							
					 	Me.P_X_Q_Manual_ADJ(si, api, globals, args)	
					' Else If sFunction = "Clear_Vol_Prod"							
					' 	Me.Clear_Vol_Prod(si, api, globals, args)		
					' CALCULATE DATA - Ratios (DSOp, DIOp, DPOp)
					' Else If sFunction = "Ratios_Proposal"
					' 	Me.Ratios_Proposal(si, api, globals, args)
					'CALCULATE DATA - WCAP (DSO-DIO)
					' Else If sFunction = "WCAP"							
					' 	Me.WCAP(si, api, globals, args)	
					'CALCULATE DATA - WCAP PAYABLES (DPO)
					' Else If sFunction = "WCAP_PAYABLES"							
					' 	Me.WCAP_PAYABLES(si, api, globals, args)
					'CALCULATE - Personnel, Interest, IFRS16, Payables TSA, Warranty, AMORT, Corp Rate, VAT					
					Else If sFunction = "Others" Then
						Me.Others(si, api, globals, args)
					'CALCULATE - Royalties				
					Else If sFunction = "Royalties" Then
					 	Me.Royalties(si, api, globals, args)
					Else If sFunction = "Engineering" Then
					 	Me.Engineering(si, api, globals, args)
					' CALCULATE - Loans				
					Else If sFunction = "Loans" Then
						Me.Loans(si, api, globals, args)	
					' CALCULATE - CorpRate2				
					Else If sFunction = "CorpRate2" Then
						Me.othersCorp(si, api, globals, args)		
					End If
					
				End If
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
			#End Region
			
			End Try
		End Function
		
#Region "Borrado"
	Sub borradoUD  (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)	

		api.Data.ClearCalculatedData("U4#ROYAL", True, True, True, True)
		api.Data.ClearCalculatedData("U4#ENG", True, True, True, True)
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
		api.Data.ClearCalculatedData("U4#CAPEXRD", True, True, True, True)
		api.Data.ClearCalculatedData("U4#DEFINCOME",True,True,True,True)	
		api.Data.ClearCalculatedData("U4#LOANS",True,True,True,True)

	End Sub
#End Region

#Region "P*Q"

	#Region "P*Q - ICP"
	
	' Calculo para la cuenta de material costs, cuyo ICP se calcula en base a las ventas que se calculan en la funcion de arriba
	Sub Budget_P_X_Q_ICP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
		
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

	End Sub

#End Region

	#Region "P*Q - Manual Adjustments"

	Sub P_X_Q_Manual_ADJ (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)		
		
		If api.Cons.IsLocalCurrencyforEntity() Then

			Dim sEntityPOV As String = api.Pov.Entity.Name	
   							
			' Clear Previo	
			api.Data.ClearCalculatedData("A#541107:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511065:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#556107:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511064:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511051:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#5511056:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#551103:O#Forms:U4#VOL_ADJ",True,True,True,True)
			api.Data.ClearCalculatedData("A#551115:O#Forms:U4#VOL_ADJ",True,True,True,True)
			
'			api.Data.ClearCalculatedData(True, True, True, True,"A#541107", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
'			api.Data.ClearCalculatedData(True, True, True, True,"A#5511065", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
'			api.Data.ClearCalculatedData(True, True, True, True,"A#556107", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
'			api.Data.ClearCalculatedData(True, True, True, True,"A#5511064", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
'			api.Data.ClearCalculatedData(True, True, True, True,"A#5511051", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
'			api.Data.ClearCalculatedData(True, True, True, True,"A#5511056", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
'			api.Data.ClearCalculatedData(True, True, True, True,"A#551103", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
'			api.Data.ClearCalculatedData(True, True, True, True,"A#551115", "F#Top.base.Where(Name DoesNotContain F00)",	"O#Forms",,,,,"U4#VOL_ADJ",,,,)
			
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
			If sEntityPOV = "R1300005" Or sEntityPOV = "R1300006" Or sEntityPOV = "R1300007" Then
				api.Data.Calculate("A#551103:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#551103:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
 			Else	
				api.Data.Calculate("A#551115:O#Forms:U4#VOL_ADJ = (A#Vol_Sales_Total:O#Forms:U4#Top * (A#551115:O#Import:U4#Top*1000/A#Vol_Sales_Total:O#Import:U4#Top)) / 1000", True)
			End If
	
		End If	

	End Sub	


#End Region

#End Region
		
#Region "Other Calcs"

	#Region "Royaties"

			' ----------------------------------------------------------------------------------------------------------------
			'  Royalties - CALCULATIONS - SUB
			' ----------------------------------------------------------------------------------------------------------------
			
			Sub Royalties (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
'			api.Data.ClearCalculatedData("U4#ROYAL", True, True, True, True)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#ROYAL",,,,)

			Dim UD1MemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Customer", "U1#Top.base", True)
			Dim ICMemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "IC", "I#Top.base", True)
			Dim UD2MemberList As List(Of MemberInfo) = BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Product", "U2#Top.base", True)

			' 1ª PARTE
			Dim sEntityMember As String = api.Pov.Entity.Name
			Dim sEntityMember_001 As String = api.Pov.Entity.Name.Substring(0,5) & "001"

			' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
			' For Each ICMember As MemberInfo In ICMemberList
			' '	Dim Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#" & UD1Member.Member.Name & ":U2#Top:U4#Top:O#Top)").CellAmount
			' 	Dim Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#Renault_Generic:U2#Top:U4#Top:O#Top)").CellAmount
			' 	If (Royalties_paid <> 0) Then
			' 		For Each UD2Member As MemberInfo In UD2MemberList
			' 			Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#Renault_Generic:U2#" & UD2Member.Member.Name & ":U4#Top:O#Top)").CellAmount
			' 			Dim Royalties_pct = api.Data.GetDataCell("(E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U4#None:O#Top)").CellAmount
			' 			
			' 			If (Royalties_paid <> 0) And (Royalties_pct <> 0) Then
'			' 						brapi.ErrorLog.LogMessage(si, "Royalties - ( " & Royalties_paid.ToString & " ) - E#" & sEntityMember & ":A#552209:I#R1300002:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U4#ROYAL:O#Import = (E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U4#Top:O#Top) * (E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U4#None:O#Top) * (-1)")
			' 				api.Data.Calculate("E#" & sEntityMember & ":A#552209:I#R1300002:U1#Renault_Generic:U2#" & UD2Member.Member.Name & ":U4#ROYAL:O#Import = (E#" & sEntityMember & ":A#541000:I#" & ICMember.Member.Name & ":U1#Renault_Generic:U2#" & UD2Member.Member.Name & ":U4#Top:O#Top) * (E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U4#None:O#Top) * (-1)", True)
			' 			End If
			' 			
			' 		Next	
			' 	End If
			' Next
			' --------------------------------------------------------------------------------------------------------------------------------------

			For Each UD1Member As MemberInfo In UD1MemberList
				
				Dim Royalties_paid As String = String.Empty
				Dim Royalties_pct As String = String.Empty
				Dim percentage As String = String.Empty
				Dim sumaICs As String = String.Empty
					
				percentage = api.Data.GetDataCell("E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U3#None:U4#Manual_Input:O#Top").CellAmount.ToString(cultureInfo.InvariantCulture)
				
				For Each UD2Member As MemberInfo In UD2MemberList ' 
					
					sumaICs = String.Empty
					Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#" & UD1Member.Member.Name & ":U2#Top:U3#None:U4#Top:O#Top)").CellAmount			
					
					If (Royalties_paid <> 0) Then
						
						Royalties_paid = api.Data.GetDataCell("(E#" & sEntityMember & ":A#541107:I#Top:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U3#None:U4#Top:O#Top)").CellAmount
						Royalties_pct = api.Data.GetDataCell("(E#" & sEntityMember_001 & ":A#Royalties:I#None:U1#None:U2#None:U4#Manual_Input:O#Top)").CellAmount							
						
						For Each ICMember As MemberInfo In ICMemberList
							If sumaICs = String.Empty Then
								sumaICs = $"E#{sEntityMember}:A#541107:I#{ICMember.Member.Name}:U1#{UD1Member.Member.Name}:U2#{UD2Member.Member.Name}:U3#Top:U4#Top:O#Top"
							Else
								sumaICs = $"{sumaICs} + E#{sEntityMember}:A#541107:I#{ICMember.Member.Name}:U1#{UD1Member.Member.Name}:U2#{UD2Member.Member.Name}:U3#Top:U4#Top:O#Top"
							End If
						Next	

						api.Data.Calculate("E#" & sEntityMember & ":A#552209:I#R1300002:U1#" & UD1Member.Member.Name & ":U2#" & UD2Member.Member.Name & ":U3#None:U4#ROYAL:O#Import = 
											("& sumaICs &")*" & percentage & "* (-1)", True)
						
					End If
				Next
			Next


			' 2ª PARTE	
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
			
				' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
				'
				'  --> Duda: En forecast, se deja abierto la UD3, eso en un GetDataCell puede generar errores, ¿es correcto así? O la cambiamos a Top que es lo que luego se hace en el calculate. En el cáclulo OLD
				' 			 se hacía el GetDataCell sobre TOP y se mantenía la apertura de UD3 en el calculate.
				'				
				' Dim ReveneuICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#552209:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
				' 			
				' ' Calculamos
				' If (ReveneuICPAmount <> 0)					
				' 	api.Data.Calculate(
				' 		"E#R1300002:C#" & entityPOVCurrencyName & ":V#YTD:A#541114:O#Import:I#" & sIC & ":U1#Horse_Generic:U2#None:U4#ROYAL = " &
				' 		"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#552209:O#Top:I#R1300002:U1#Top:U2#Top:U4#Top * (-1) * " & revRate.ToString.Replace(",","."), True)
				' End If	
				' --------------------------------------------------------------------------------------------------------------------------------------

				Dim ReveneuICPAmount = api.Data.GetDataCell("E#" & sIC & ":C#" & entityICCurrencyName & ":A#552209:T#" & sTime & ":O#Top:V#Periodic:I#" & sEntityPOV & ":F#Top:U1#Top:U2#Top:U4#Top:U5#Top:U6#Top:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)

				' Calculamos
				' If (ReveneuICPAmount <> 0)	
				If sEntityPOV = "R1300002"	
'					If sIC = "R0585001" Then brapi.ErrorLog.LogMessage(si, $"{sTime} -> tasa: {revRate}, cantidad: {ReveneuICPAmount}" )
						
					api.Data.Calculate(
						"E#R1300002:C#" & entityPOVCurrencyName & ":V#YTD:A#541114:O#Import:I#" & sIC & ":U1#Horse_Generic:U2#None:U3#None:U4#ROYAL = " &
						"E#" & sIC & ":C#" & entityICCurrencyName & ":V#YTD:A#552209:O#Top:I#R1300002:U1#Top:U2#Top:U3#Top:U4#Projections * (-1) * " & revRate.ToString.Replace(",","."), True
						)
						
				End If	

			Next
			
		End Sub

	#End Region
	
	#Region "Engineering"
	
			' ----------------------------------------------------------------------------------------------------------------
			'  Engineering - CALCULATIONS - SUB
			' ----------------------------------------------------------------------------------------------------------------
			
			Sub Engineering (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
				
'				api.Data.ClearCalculatedData("U4#ENG", True, True, True, True)
				api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#ENG",,,,)
				
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

		End Sub

	#End Region
	
	#Region "Loans"

			' ----------------------------------------------------------------------------------------------------------------
			'  LOANS - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			Sub Loans (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)


				' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
				' --------------------------------------------------------------------------------------------------------------------------------------
				
				Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
				Dim sScenario As String = api.Pov.Scenario.Name
				Dim sTime As String = api.Pov.Time.Name 
				Dim year As String = sTime.Substring(0,4)
				Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))	
				Dim sEntity As String = api.pov.Entity.Name
				Dim sEntityParent As String = Left(sEntity,5)			
				Dim MesFcstNumber As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods
						
				If sEntity.EndsWith("R1300001") And (ScenarioType = "Budget") Then
						
						api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#LOANS",,,,)
	
						Dim ICList As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.EntityDim.DimPk,api.Members.GetMemberId(dimtype.Entity.Id,"Horse_Group"))
						
						Dim sEntityPOVID As Integer = api.Pov.Entity.MemberId
						Dim sEntityPOV As String = api.Pov.Entity.Name	
						Dim sMesAnterior As String = 1
						Dim sFlujo As String = String.Empty
						
						If(sMes = 1) Then 
					
							sMesAnterior = 1
							sFlujo = "F00"
					
						Else If (ScenarioType = "Budget" And sMes > 1) Then  
					
							sMesAnterior = sMes - 1
							sFlujo = "F99"
					
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
									"E#" & sIC & ":A#2561010C:C#" & entityICCurrencyName & ":V#Periodic:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",",".")  & "-" &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2561010C:O#Top:I#R1300001:F#"& sFlujo &":U4#Top*" & revRate.ToString.Replace(",","."), True)
	 
						'	End If
							'LOANS LARGO PLAZO
						'	If(difMesLP <> 0 ) Then
								api.Data.Calculate(
									"E#R1300001:C#" & entityPOVCurrencyName & ":V#Periodic:A#1221000C:O#Import:F#F20:I#" & sIC & ":U4#LOANS = " &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:A#2141010C:O#Top:I#R1300001:F#F99:U4#Top*" & revRate.ToString.Replace(",",".")  & "-" &
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2141010C:O#Top:I#R1300001:F#"& sFlujo &":U4#Top*" & revRate.ToString.Replace(",","."), True)
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
									"E#" & sIC & ":C#" & entityICCurrencyName & ":V#Periodic:T#"& year &"M"& sMesAnterior &":A#2141020C:O#Top:I#R1300001:F#"& sFlujo &":U4#Top*" & revRate.ToString.Replace(",","."), True)
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
	
	#Region "OTHER CALCS"

		#Region "Others"

		Sub Others (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
'			api.Data.ClearCalculatedData("U4#PERS",True,True,True,True)
'			api.Data.ClearCalculatedData("U4#INTER",True,True,True,True)
'			api.Data.ClearCalculatedData("U4#WRTY",True,True,True,True)	
'			api.Data.ClearCalculatedData("U4#IFRS16",True,True,True,True)
'			api.Data.ClearCalculatedData("U4#CORP",True,True,True,True)	
'			api.Data.ClearCalculatedData("U4#AMORT",True,True,True,True)
'			api.Data.ClearCalculatedData("U4#DSOTSA",True,True,True,True)
'			api.Data.ClearCalculatedData("U4#VAT",True,True,True,True)	
'			api.Data.ClearCalculatedData("U4#OTHFIN",True,True,True,True)	
'			api.Data.ClearCalculatedData("U4#OTHOP",True,True,True,True)
'			api.Data.ClearCalculatedData("U4#DEFINCOME",True,True,True,True)
			
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#PERS",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#INTER",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#WRTY",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#IFRS16",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#CORP",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#AMORT",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#DSOTSA",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#VAT",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#OTHFIN",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#OTHOP",,,,)
			api.Data.ClearCalculatedData(True, True, True, True,, "F#Top.base.Where(Name DoesNotContain F00)",,,,,,"U4#DEFINCOME",,,,)
			
			
			#Region "All Variables"

			' ----------------------------------------------------------------------------------------------------------------
			' VARIABLES
			' ----------------------------------------------------------------------------------------------------------------
			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
			Dim sScenario As String = api.Pov.Scenario.Name
			Dim sTime As String = api.Pov.Time.Name 
			Dim year As String = sTime.Substring(0,4)
			Dim sMes As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
			Dim sPriorT As String = year &"M"& (sMes-3)	
			Dim sEntity As String = api.pov.Entity.Name
			Dim sEntityParent As String = Left(sEntity,5)
		
			
			' ----------------------------------------------------------------------------------------------------------------
			' GLOBAL ASSUMPTIONS
			' ----------------------------------------------------------------------------------------------------------------
			' Cruces - Abiertos por UD1
			Dim all_Top_Periodic As String = ":O#Top:I#Top:F#Top:U2#Top:U3#Top:U4#Manual_Input:U5#None:U6#None:U7#None:U8#None:V#Periodic"
			Dim uds_Top As String = ":U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None"
			
			' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
			' ' Personnel
			' Dim salaryPayments = api.Data.GetDataCell("E#" & sEntity & ":A#Salary_pays:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
			' Dim bonusProvision = api.Data.GetDataCell("E#" & sEntityParent & "001:A#Bonus%:T#" & sTime & ":O#Top:V#Periodic:I#Top:F#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
			' 
			' ' Corp Rate
			' Dim corpRate As String = api.Data.GetDataCell("E#"& sEntityParent &"001:C#Local:S#"& sScenario &":T#"& sTime &":V#Periodic:A#Corp_Tax:F#None:O#Forms:I#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None").CellAmount.ToString(cultureinfo.InvariantCulture)
			' --------------------------------------------------------------------------------------------------------------------------------------
					
			' Personnel
			Dim salaryPayments = api.Data.GetDataCell("E#" & sEntity & ":A#Salary_pays:T#" & sTime & all_Top_Periodic).CellAmount.ToString(cultureinfo.InvariantCulture)
			Dim bonusProvision = api.Data.GetDataCell("E#" & sEntityParent & "001:A#Bonus%:T#" & sTime & all_Top_Periodic).CellAmount.ToString(cultureinfo.InvariantCulture)

			' Corp Rate
			Dim corpRate As String = api.Data.GetDataCell("E#"& sEntityParent &"001:A#Corp_Tax:T#"& sTime &":O#Forms:I#None:F#None:U2#None:U3#None:U4#Manual_Input:U5#None:U6#None:U7#None:U8#None:V#Periodic:C#Local").CellAmount.ToString(cultureinfo.InvariantCulture)
			' Dim corpRate As String = api.Data.GetDataCell("S#"& sScenario &":C#Local:E#"& sEntityParent &"001:A#Corp_Tax:T#"& sTime &":O#Forms:I#None:F#None:U2#None:U3#None:U4#Manual_Input:U5#None:U6#None:U7#None:U8#None:V#Periodic").CellAmount.ToString(cultureinfo.InvariantCulture)
		
			' ----------------------------------------------------------------------------------------------------------------
			'  AGGREGATORS 
			'		Serán los que pasen por el for each para agregar los valores de los hijos en la 001
			' ----------------------------------------------------------------------------------------------------------------
			
			' Personnel
			Dim totalPersonnel As String = String.Empty
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
			Dim othOperatingIndust As String = String.Empty
			Dim othOperatingIntellect As String = String.Empty	
			
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

			#End Region 
			
			For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
				
				Dim memberName As String = miMember.Member.Name

				If i <> 0 Then

					#Region "SUM"

					totalPersonnel = totalPersonnel & " + "
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
					othOperatingIndust = othOperatingIndust & " + "
					othOperatingIntellect = othOperatingIntellect & " + "		
					
					defIncome = defIncome & " + "
					
					stLoanLiability = stLoanLiability   & " + "
					stLoanAsset = stLoanAsset  & " + "
					ltLoanLiability = ltLoanLiability   & " + "
					ltLoanAsset = ltLoanAsset   & " + "
					
					stInterestLiability = stInterestLiability   & " + "
					stInterestAsset = stInterestAsset   & " + "
					ltInterestLiability  = ltInterestLiability   & " + "
					ltInterestAsset   = ltInterestAsset   & " + "

					#End Region
					
				End If
				
					#Region "Dimensions"

				' Personnel
				totalPersonnel 			= totalPersonnel 		& "RemoveZeros(E#" & memberName 	&":T#"& year  			&":A#Personnel_Total"	&":O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"												
				priorYearBPCell 		= priorYearBPCell 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#Bonus_pay"			&":O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"																	
				menstotalPersonnel 		= menstotalPersonnel 	& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#Personnel_Total"	&":O#Top:I#Top:F#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' Interest	
				logistic 				= logistic 				& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#554403"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"
				interestPaid 			= interestPaid 			& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#49122"				&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"
				financialDebt 			= financialDebt 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#49121"				&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"				
				financialDebtYTD 		= financialDebtYTD 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#49121"				&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD)"								
				financialDebtYTDant 	= financialDebtYTDant 	& "RemoveZeros(E#" & memberName 	&":T#"& sPriorT 		&":A#49121"				&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD)"
				financialIncome 		= financialIncome 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4911"				&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' IFRS16						
				interest 				= interest 				& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#49124"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' Payables TSA										
				subcontratingIng 		= subcontratingIng 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#572109"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																+  RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#551125"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				subcontratingIngYTD6 	= subcontratingIngYTD6 	& "RemoveZeros(E#" & memberName 	&":T#"& year &"M6" 		&":A#572109"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD) 
																 + RemoveZeros(E#" & memberName 	&":T#"& year &"M6"		&":A#551125"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD)"

				subcontratingIngYTD12 	= subcontratingIngYTD12 & "RemoveZeros(E#" & memberName 	&":T#"& year &"M12"		&":A#572109"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD) 
																 + RemoveZeros(E#" & memberName 	&":T#"& year &"M12"		&":A#551125"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD)"


				subcontratingIng_IC 	= subcontratingIng_IC 		& "RemoveZeros(E#" & memberName &":T#"& sTime 			&":A#572109"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																+  RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#551125"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				subcontratingIngYTD6_IC = subcontratingIngYTD6_IC & "RemoveZeros(E#" & memberName 	&":T#"& year &"M6" 		&":A#572109"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD) 
																 + RemoveZeros(E#" & memberName 	&":T#"& year &"M6"		&":A#551125"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD)"

				subcontratingIngYTD12_IC= subcontratingIngYTD12_IC & "RemoveZeros(E#" & memberName 	&":T#"& year &"M12"		&":A#572109"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD) 
																 + RemoveZeros(E#" & memberName 	&":T#"& year &"M12"		&":A#551125"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#YTD)"

				' Warranty					
				warrantyOther 			= warrantyOther 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#556000"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"									

				' Corp Rate				
				dataEBT 				= dataEBT 				& "RemoveZeros(E#" & memberName 	&""						&":A#EBT"				&":O#Top:F#None:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"
				data411 				= data411 				& "RemoveZeros(E#" & memberName 	&""						&":A#411"				&":O#Top:F#None:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"												

				' AMORT	
				intangAssets 			= intangAssets 			& "RemoveZeros(E#" & memberName 	&""						&":A#552406"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#557103"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#563109"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				capitDevelop 			= capitDevelop 			& "RemoveZeros(E#" & memberName 	&""						&":A#572115"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
				' rightUse 				= rightUse 				& "RemoveZeros(E#" & memberName 	&""						&":A#552307"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
				' 												 + RemoveZeros(E#" & memberName 	&""						&":A#563107"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"
				' --------------------------------------------------------------------------------------------------------------------------------------
				
				rightUse 				= rightUse 				& "RemoveZeros(E#" & memberName 	&""						&":A#563107G"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#5631071"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)												 
																 + RemoveZeros(E#" & memberName 	&""						&":A#552307"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				sw						= sw					& "RemoveZeros(E#" & memberName 	&""						&":A#5631072"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
				' depreciation 			= depreciation 			& "RemoveZeros(E#" & memberName 	&""						&":A#55110590"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
				' 												 + RemoveZeros(E#" & memberName 	&""						&":A#551107"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
				' 												 + RemoveZeros(E#" & memberName 	&""						&":A#551108"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
				' 												 + RemoveZeros(E#" & memberName 	&""						&":A#552108"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
				' 												 + RemoveZeros(E#" & memberName 	&""						&":A#49123"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
				' 												 + RemoveZeros(E#" & memberName 	&""						&":A#572107"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"
				' --------------------------------------------------------------------------------------------------------------------------------------
				depreciation 			= depreciation 			& "RemoveZeros(E#" & memberName 	&""						&":A#55110590"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#551107"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#551108"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#552108"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#49123"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#551118"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#55110696"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#5621072"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&""						&":A#572107"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)
																 + RemoveZeros(E#" & memberName 	&""						&":A#554207"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				provisions 				= provisions 			& "RemoveZeros(E#" & memberName 	&""						&":A#555103R"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' CapexRD					
				CapexRD114 				= CapexRD114 			& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#572114"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"											
				CapexRD118 				= CapexRD118 			& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#572118"			&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' Other Financial	
				othFinancial 			= othFinancial	 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#492"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				' Other Operating	
				othOperatingCash 		= othOperatingCash 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#481"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#482"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#486"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#487"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#488"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4890"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4892"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4895"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4899"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				othOperatingProvision 	= othOperatingProvision & "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#483"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#484"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#489"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4893"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic) 
																 + RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4896"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				othOperatingIndust		= othOperatingIndust 	& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4897"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				othOperatingIntellect 	= othOperatingIntellect & "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#4898"				&":O#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"	

				'Deferred Income	
				defIncome 				= defIncome 			& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#541141"			&":O#Top:F#None:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"	

				'Loans - Origenes	
				stLoanLiability 		= stLoanLiability 		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#2561010C"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"
				ltLoanLiability 		= ltLoanLiability  		& "RemoveZeros(E#" & memberName 	&":T#"& sTime 			&":A#2141010C"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"

				'Intereses - Origenes	
				stInterestLiability 	= stInterestLiability  & "RemoveZeros(E#" & memberName		&":T#"& sTime 			&":A#2561020C"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"
				ltInterestLiability  	= ltInterestLiability  & "RemoveZeros(E#" & memberName		&":T#"& sTime 			&":A#2141020C"			&":O#Top:I#Top:F#None:U2#Top:U3#Top:U4#Top:V#Periodic)"
				
				#End Region

				i = i + 1
			
			Next
			
			#Region "Personnel - 29/09/2025"

''			' ----------------------------------------------------------------------------------------------------------------
''			'  PERSONNEL - CALCULATIONS 
''			' ----------------------------------------------------------------------------------------------------------------			
''			' 1- Formato de los agregados
''			totalPersonnel = "("& totalPersonnel &")"
''			Dim mensPersonnel = "("& menstotalPersonnel &")"
''			priorYearBPCell = "("& priorYearBPCell &")"
''			Dim priorYearBonusPayment = "E#" & sEntityParent & "001:A#Bonus_pay:T#"& sTime &":F#None:O#Forms:U1#None:U2#None:U3#None:U4#Projections:I#None"
			
''			' 2- Calculos Previos
''			Dim payrollSSD = "("& totalPersonnel & " * (1-" & bonusProvision & ")) / " & salaryPayments	
''			Dim extraP13 = payrollSSD & "/12"
''			Dim extraP14 = payrollSSD & "/6"
			
''			Dim bonusP = "(-1*("& totalPersonnel &"*"& bonusProvision & ") / 12)"

''			Dim dif12 = mensPersonnel &"+( -"& payrollSSD &"+"& bonusP &")"
''			Dim dif13 = mensPersonnel &"+( -"& payrollSSD &"+"& bonusP & "-" & extraP13 &")"
''			Dim dif14 = mensPersonnel &"+( -"& payrollSSD &"+"& bonusP & "-" & extraP14 &")"

''			' 3- Insertar valores
''			If sEntity.EndsWith("001") Then

''				' Caja - 1681000CF (Sueldo, pago del bonus py, diferencia o sobrante)
''				' Paga Extra - 29018000			
''				If left(salaryPayments,2) = "14" Then	
'''					Dim valorDiferencia = api.Data.GetDataCell(dif14).CellAmount.ToString(cultureinfo.InvariantCulture)
''					Dim valorDiferencia As String = dif14
					
''					If (sTime = year & "M6" Or sTime = year & "M12") Then
						
''						' Caja - 1681000CF (Sueldo, Paga Extra, Bonus PY, Diferencia) 
''						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "2*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
						
''						' Paga Extra - 29018000 (Provisión y Pago)
''						api.Data.Calculate("E#"& sEntity &":A#2901800:F#F20:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = -" & extraP14, True)								
''						api.Data.Calculate("E#"& sEntity &":A#2901800:F#F30:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = 6*" & extraP14, True)	
						
''					Else
						
''						' Caja - 1681000CF (Sueldo, Bonus PY, Diferencia) 
''						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
						
''						' Paga Extra - 29018000 (Provisión)
''						api.Data.Calculate("E#"& sEntity &":A#2901800:F#F20:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = -" & extraP14, True)	
						
''					End If
					
''				Else If left(salaryPayments,2) = "13" Then	
'''					Dim valorDiferencia = api.Data.GetDataCell(dif13).CellAmount.ToString(cultureinfo.InvariantCulture)
''					Dim valorDiferencia As String = dif13
						
''					If (sTime = year & "M12") Then
						
''						' Caja - 1681000CF (Sueldo, Paga Extra, Bonus PY, Diferencia) 
''						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "2*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)

''						' Paga Extra - 29018000 (Provisión y Pago)
''						api.Data.Calculate("E#"& sEntity &":A#2901800:F#F20:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = -" & extraP13, True)								
''						api.Data.Calculate("E#"& sEntity &":A#2901800:F#F30:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = 12*" & extraP13, True)	
						
''					Else
						
''						' Caja - 1681000CF (Sueldo, Bonus PY, Diferencia) 
''						api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
						
''						' Paga Extra - 29018000 (Provisión)
''						api.Data.Calculate("E#"& sEntity &":A#2901800:F#F20:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = -" & extraP13, True)	
						
''					End If
					
''				Else If left(salaryPayments,2) = "12" Then
'''					Dim valorDiferencia = api.Data.GetDataCell(dif12).CellAmount.ToString(cultureinfo.InvariantCulture)
''					Dim valorDiferencia As String = dif12
				
''					' Caja - 1681000CF (Sueldo, Bonus PY, Diferencia) 

''					api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)

''					' MACS 2025/07/21 - OLD ---------------------------------------------------------------------------------------------------------------					
''					' api.Data.Calculate("A#1681000CF:F#F30:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & "1*" & payrollSSD & "-" &priorYearBonusPayment &"+"& valorDiferencia,True)
''					' --------------------------------------------------------------------------------------------------------------------------------------
										
''				End If
				
''				If Left(salaryPayments,2) = "12" Or Left(salaryPayments,2) = "13" Or Left(salaryPayments,2) = "14"  Then
''					' Bonus - 2901100 (Provisión de Bonus y Bonus PY)
''					' api.Data.Calculate("E#"& sEntity &":A#2901100:F#F20:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = " & bonusP, True)
''					' api.Data.Calculate("E#"& sEntity &":A#2901100:F#F30:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = -1*" & priorYearBPCell, True)
''					api.Data.Calculate("E#"& sEntity &":A#2901100:F#F16:U1#None:U2#None:U3#None:U4#PERS:T#" & sTime & ":V#Periodic:O#Import:I#None = "& bonusP &" - " & priorYearBPCell, True)
					
''				End If
				
''				api.Data.Calculate("U4#PERS:F#F99I = U4#PERS:F#F99" , True)	
''			End If
			

			#End Region
			
			#Region "Personnel"

			' ----------------------------------------------------------------------------------------------------------------
			'  PERSONNEL - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------			
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
			

			#End Region
			
			#Region "Interest"

			' ----------------------------------------------------------------------------------------------------------------
			'  INTEREST - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------			
			If sEntity.EndsWith("001") Then
				
				' 1- Insertar datos
				If sTime = year &"M3" Then
					
					api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = -("& financialDebt &")", True)					 
					api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F30:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = ("& financialDebtYTD &")", True) 
					' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------	
					'  api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import = ("& financialIncome &") + ("& interestPaid &")", True)
					'  api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F30:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import = ("& logistic &") + ("& financialDebtYTD &")", True)					
					' --------------------------------------------------------------------------------------------------------------------------------------
					api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = (("& logistic &") + ("& financialDebtYTD &")) + (("& financialIncome &") + ("& interestPaid &"))", True)

				Else If sTime = year &"M6" Or sTime = year &"M9" Or sTime = year &"M12" Then
						
					api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = -("& financialDebt &")", True)
					api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F30:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = ("& financialDebtYTD &")-("& financialDebtYTDant &")", True)
					' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------	
					' api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import = ("& financialIncome &")+ ("& interestPaid &")", True)
					' api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F30:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import = ("& logistic &") + ("& financialDebtYTD &")-("& financialDebtYTDant &")", True)
					' --------------------------------------------------------------------------------------------------------------------------------------
					api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = (("& logistic &") + ("& financialDebtYTD &")-("& financialDebtYTDant &")) + (("& financialIncome &")+ ("& interestPaid &"))", True)
					
				Else					
					 
					api.Data.Calculate("E#"& sEntity &":A#2561020C:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import = -("& financialDebt &")", True)		 
					' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------	
					' api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F20:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import = ("& financialIncome &")+ ("& interestPaid &")", True)
					' api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F30:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:O#Import = ("& logistic &")", True)		
					' --------------------------------------------------------------------------------------------------------------------------------------
					api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#INTER:T#"& sTime & ":V#Periodic:I#None:O#Import:I#None = ("& logistic &") + (("& financialIncome &") + ("& interestPaid &"))", True)		
					 
				End If
				api.Data.Calculate("U4#INTER:F#F99I = U4#INTER:F#F99" , True)	
				
			End If

			#End Region

			#Region "IFRS 16"

			' ----------------------------------------------------------------------------------------------------------------
			'  IFRS16 - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------			
			If sEntity.EndsWith("001") Then
			
				' 1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#2531020C:F#F20:U2#None:U3#None:U4#IFRS16:T#"& sTime &":V#Periodic:O#Import = -1*("& interest &")", True)
				api.Data.Calculate("U4#IFRS16:F#F99I = U4#IFRS16:F#F99" , True)	
				
			End If

			#End Region

			#Region "Payables TSA"

			' ----------------------------------------------------------------------------------------------------------------
			'  PAYABLES_TSA - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") Then
				
				'1- Insertar datos
				' MACS 2025/07/21 - OLD ---------------------------------------------------------------------------------------------------------------					
				' api.Data.Calculate("A#2721020:F#F20:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = -("& subcontratingIng &")", True)
				' --------------------------------------------------------------------------------------------------------------------------------------
				api.Data.Calculate("A#2721020:F#F20:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = -("& subcontratingIng_IC &")", True)
				
				If sTime = year & "M6"				
					
					' Al vacíar la 2721020 (F30) no valía con sCalc2 aunque el dato e intersecciones estén bien
					' para solucionarlo 'vaciamos' con el dato acumulado (YTD) que tiene esa misma cuenta a Junio
					
					' MACS 2025/07/21 - OLD ---------------------------------------------------------------------------------------------------------------					
					' api.Data.Calculate("A#2721020:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = -1*(A#2721020:F#F20:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":O#Import:V#YTD)", True)
					' api.Data.Calculate("A#1681000CF:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = -1*(A#2721020:F#F20:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":O#Import:V#YTD)", True)
					' --------------------------------------------------------------------------------------------------------------------------------------
					api.Data.Calculate("A#2721020:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#YTD:O#Import = ((" & subcontratingIngYTD6_IC &"))", True)
					api.Data.Calculate("A#1681000CF:F#F16:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#YTD:O#Import:I#None = ((" & subcontratingIngYTD6 &"))", True)

				Else If sTime = year & "M12" Then
					' MACS 2025/07/21 - OLD ---------------------------------------------------------------------------------------------------------------					
					' api.Data.Calculate("A#2721020:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = (("& subcontratingIngYTD12 & ") - (" & subcontratingIngYTD6 &"))", True)
					' api.Data.Calculate("A#1681000CF:F#16:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = (("& subcontratingIngYTD12 & ") - (" & subcontratingIngYTD6 &"))", True)
					' --------------------------------------------------------------------------------------------------------------------------------------
					
					api.Data.Calculate("A#2721020:F#F30:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import = (("& subcontratingIngYTD12_IC & ") - (" & subcontratingIngYTD6_IC &"))", True)
					api.Data.Calculate("A#1681000CF:F#F16:U2#None:U3#None:U4#DSOTSA:T#"& sTime &":V#Periodic:O#Import:I#None = (("& subcontratingIngYTD12 & ") - (" & subcontratingIngYTD6 &"))", True)
					
				End If	
				api.Data.Calculate("U4#DSOTSA:F#F99I = U4#DSOTSA:F#F99" , True)	
				
			End If

			#End Region

			#Region "Warranty"

			' ----------------------------------------------------------------------------------------------------------------
			'  WARRANTY - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") Then
			
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#2061000CF:F#F25:U2#None:U3#None:U4#WRTY:T#"& sTime &":V#Periodic:O#Import = -1*( "& warrantyOther &")", True)
				api.Data.Calculate("U4#WRTY:F#F99I = U4#WRTY:F#F99" , True)	
				
			End If	

			#End Region

			#Region "VAT"

			' ----------------------------------------------------------------------------------------------------------------
			'  VAT - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------

			' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
			' Dim numberSeq() As Integer = {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12} 
			' Dim mesInicio As Integer = 1
			' 		
			' For Each mesFinal As Integer In numberSeq
			' 	
			' 	Dim suma29_ACT As String = "E#" & sEntity & ":A#2901710:F#F20:U2#Top:U3#Top:U4#WCAP:T#"& year &"M"& mesFinal.ToString() &":V#YTD:O#Top:I#Top"
			' 	Dim suma29_LAST As String = "E#" & sEntity & ":A#2901710:F#F20:U2#Top:U3#Top:U4#WCAP:T#"& year &"M"& mesInicio.ToString() &":V#YTD:O#Top:I#Top"
			' 	Dim suma29_PERIODIC As String = "E#" & sEntity & ":A#2901710:F#F20:U2#Top:U3#Top:U4#WCAP:T#"& year &"M"& mesFinal.ToString() &":V#Periodic:O#Top:I#Top"
			' 
			' 	Dim suma23_ACT As String = "E#" & sEntity & ":A#1561021:F#F20:U2#Top:U3#Top:U4#WCAP:T#"& year &"M"& mesFinal.ToString() &":V#YTD:O#Top:I#Top"
			' 	Dim suma23_LAST As String = "E#" & sEntity & ":A#1561021:F#F20:U2#Top:U3#Top:U4#WCAP:T#"& year &"M"& mesInicio.ToString() &":V#YTD:O#Top:I#Top"
			' 	Dim suma23_PERIODIC As String = "E#" & sEntity & ":A#1561021:F#F20:U2#Top:U3#Top:U4#WCAP:T#"& year &"M"& mesInicio.ToString() &":V#Periodic:O#Top:I#Top"
			' 	
			' 	Dim valor29 As String = String.Empty
			' 	Dim valor15 As String = String.Empty
			' 	Dim caja As String = String.Empty
			' 	
			' 	If mesInicio = mesFinal And mesFinal = 1 Then
			' 		
			' 		valor29 = suma29_ACT
			' 		valor15 = suma23_ACT
			' 		caja = "("& suma29_ACT &" - "& suma23_ACT &")"
			' 		
			' 	Else If  mesInicio = mesFinal And mesFinal >1 Then
			' 		
			' 		valor29 = suma29_PERIODIC
			' 		valor15 = suma23_PERIODIC
			' 		caja = "("& valor29 &"-"& valor15 &")"
			' 		If sMes = 3 Then 
			' 			' brapi.ErrorLog.LogMessage(si, "valor29 "& API.Data.GetDataCell(valor29).CellAmount &" | valor15: "& API.Data.GetDataCell(valor15).CellAmount)
			' 			' brapi.ErrorLog.LogMessage(si, "caja "& API.Data.GetDataCell(caja).CellAmount)
			' 		End If
			' 	Else If mesInicio <> mesFinal And mesInicio = 1 Then
			' 					
			' 		valor29 = "("& suma29_ACT &")"
			' 		valor15 = "("& suma23_ACT &")"
			' 		caja = "("& valor29 &"-"& valor15 &")"
			' 	
			' 	Else If mesInicio <> mesFinal And mesInicio <> 1 Then
			' 					
			' 		valor29 = "("& suma29_ACT &"-"& suma29_LAST &")"
			' 		valor15 = "("& suma23_ACT &"-"& suma23_LAST &")"
			' 		caja = "("& valor29 &"-"& valor15 &")"
			' 		
			' 	End If
			' 	
			' 	If (api.Data.GetDataCell(caja).CellAmount.ToString(cultureInfo.InvariantCulture).Contains("-")) = True Then
			' 		' Aquí el valor es negativo y no hacemos nada
			' 	Else
			' 		
			' 		If mesFinal = sMes Then
			' 			' Logica Para vaciar
			' 			api.Data.Calculate("E#"& sEntity &":A#2901710:F#F30:U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = -1*("& valor29 &")", True)
			' 			api.Data.Calculate("E#"& sEntity &":A#1561021:F#F30:U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = -1*(" & valor15 &")", True)
			' 			api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F30:U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = -1*("& caja &")", True)
			' 
			' 		Else
			' 			mesInicio = mesFinal + 1
			' 			
			' 		End If
			' 	End If
			' 	If mesFinal = sMes Then
			' 		Exit For
			' 	End If
			' Next	
			' API.Data.Calculate("U4#VAT:F#F99I = U4#VAT:F#F99" , True)			
			' --------------------------------------------------------------------------------------------------------------------------------------

			If sEntity.EndsWith("001") Then
				
				Dim sMesOrigen = If(sMes=1, 1, sMes-1)
				Dim sFlujo As String = If(sMes=1, "F00", "F99")
				' M1 - F99 YEAR-1 M12 SCENARIO.TEXT1 = F00
				
				' Dato del Primer mes Cerrado -Definir con Nathi cual es ese dato
				Dim real29_YTD_Prior As Decimal = api.Data.GetDataCell("E#" & sEntity & ":A#2901710:F#"& sFlujo &":U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& sMesOrigen &":V#YTD:O#Top:I#Top").CellAmount
				Dim real15_YTD_Prior As Decimal = api.Data.GetDataCell("E#" & sEntity & ":A#1561021:F#"& sFlujo &":U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& sMesOrigen &":V#YTD:O#Top:I#Top").CellAmount
				
				If sEntity = "R1300001" Then 
					' brapi.ErrorLog.LogMessage(si, $"{sEntity} - {sMes}: Mes Calculado {sMesOrigen} | 15: {real15_YTD_Prior} | 29: {real29_YTD_Prior}")
				End If
				
				If real29_YTD_Prior > real15_YTD_Prior Then
					
					If sEntity = "R1300001" Then 
						' brapi.ErrorLog.LogMessage(si, $"Entra - {sMes}")
					End If
					
					' Vaciar 29
					api.Data.Calculate("E#" & sEntity & ":A#2901710:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& year &"M"& sMes &":V#Periodic:O#Import:I#None = 
						(-1) * E#" & sEntity & ":A#2901710:F#"& sFlujo &":U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& sMesOrigen &":V#YTD:O#Top:I#Top
					", True)
					
					' Vaciar 15
					api.Data.Calculate("E#" & sEntity & ":A#1561021:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& year &"M"& sMes &":V#Periodic:O#Import:I#None = 
						(-1) * E#" & sEntity & ":A#1561021:F#"& sFlujo &":U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& sMesOrigen &":V#YTD:O#Top:I#Top
					", True)
						
					' Caja 
					api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U1#None:U2#None:U3#None:U4#VAT:T#"& sTime &":V#Periodic:O#Import:I#None = 
						(-1) * E#" & sEntity & ":A#2901710:F#"& sFlujo &":U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& sMesOrigen &":V#YTD:O#Top:I#Top
						+ E#" & sEntity & ":A#1561021:F#"& sFlujo &":U1#Top:U2#Top:U3#Top:U4#Top:T#"& year &"M"& sMesOrigen &":V#YTD:O#Top:I#Top
					", True)
						
				End If

			End If
			#End Region
			
			#Region "Corp Rate"

			' ----------------------------------------------------------------------------------------------------------------
			'  CORP RATE - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------

			' MACS 2025/07/21 - OLD ----------------------------------------------------------------------------------------------------------------
			' api.Data.Calculate("E#"& sEntity &":A#4111:F#None:U2#None:U3#None:U4#CORP:V#Periodic:O#Import:I#None = A#EBT:F#None:O#Top:U2#Top:U3#Top:U4#Top:V#Periodic:I#Top * (-1) *"&corpRate, True)
			' --------------------------------------------------------------------------------------------------------------------------------------

			api.Data.Calculate("E#"& sEntity &":A#4111:F#None:U1#None:U2#None:U3#None:U4#CORP:V#Periodic:O#Import:I#None = A#EBT:E#"& sEntity &":F#None:O#Top:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic:I#Top * (-1) * " & corpRate, True)
				
			#End Region 

			#Region "Amort"

			' ----------------------------------------------------------------------------------------------------------------
			'  AMORT - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") Then
				If api.Cons.IsLocalCurrencyForEntity() Then
					api.Data.ClearCalculatedData("U4#AMORT",True,True,True,True)
					
					' 1009904 - Intangible Assets Amortization (Suma de 55406, 572107)
					api.Data.Calculate("A#1009904:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & intangAssets &")", True)
				
					' 1021004C - Capitalised Development Expenses Amortization (Suma de 572115)					
					api.Data.Calculate("A#1021004C:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & capitDevelop &")", True)
					
					' 1039104 - Right of Use of Leased Assets Amortization (Suma de 552307, 563107) -					
					api.Data.Calculate("A#1039104:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & rightUse &")", True)

					' 1009204 - Software Amortization (Suma de  5631072) -
					api.Data.Calculate("A#1009204:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & sw &")", True)
					
					' 1041004C - Depreciation (Suma de 55110590, 551107, 551108, 552108) -					
					api.Data.Calculate("A#1041004CF:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & depreciation &")", True)
			
					' 1501004C - Provisions/Depreciation of Inventories (Suma de 555103R) 					
					api.Data.Calculate("A#1501004CF:F#F25:U2#None:U3#None:U4#AMORT:V#Periodic:O#Import = (" & provisions &")", True)
					
					api.Data.Calculate("U4#AMORT:F#F99I = U4#AMORT:F#F99" , True)
				End If	
			End If
			
			#End Region

			#Region "Capex - R&D"

			' ----------------------------------------------------------------------------------------------------------------
			'  CAPEXRD - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") Then
			
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#1021020C:F#F20:U2#None:U3#None:U4#CAPEXRD:T#"& sTime &":V#Periodic:O#Import = ( "& CapexRD114 &")", True)
				api.Data.Calculate("E#"& sEntity &":A#1049300:F#F20:U2#None:U3#None:U4#CAPEXRD:T#"& sTime &":V#Periodic:O#Import = ( "& CapexRD118 &")", True)
				
				api.Data.Calculate("U4#CAPEXRD:F#F99I = U4#CAPEXRD:F#F99" , True)	
				
			End If

			#End Region

			#Region "Other Financial"

			' ----------------------------------------------------------------------------------------------------------------
			'  OTHER FINANCIAL - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") Then
			
				'1- Insertar datos - ANTES SIN I NONE
				api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#OTHFIN:T#"& sTime &":V#Periodic:O#Import:I#None = ( "& othFinancial &")", True)
				
				api.Data.Calculate("U4#OTHFIN:F#F99I = U4#OTHFIN:F#F99" , True)	
				
			End If	
			
			#End Region

			#Region "Other Operating"

			' ----------------------------------------------------------------------------------------------------------------
			'  OTHER OPERATING - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			If sEntity.EndsWith("001") Then
			
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#1681000CF:F#F16:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = ( "& othOperatingCash &")", True)
'				api.Data.Calculate("E#"& sEntity &":A#2061430:F#F20:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = (-1)*( "& othOperatingProvision &")", True)
				api.Data.Calculate("E#"& sEntity &":A#2061430:F#F25:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = (-1)*( "& othOperatingProvision &")", True)
				api.Data.Calculate("E#"& sEntity &":A#1049314:F#F25:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = ( "& othOperatingIndust &")", True)
				api.Data.Calculate("E#"& sEntity &":A#1009404:F#F25:U2#None:U3#None:U4#OTHOP:T#"& sTime &":V#Periodic:O#Import = ( "& othOperatingIntellect &")", True)

				api.Data.Calculate("U4#OTHOP:F#F99I = U4#OTHOP:F#F99" , True)	
				
			End If
			
			#End Region

			#Region "Deferred Income"

			' ----------------------------------------------------------------------------------------------------------------
			'  DEFERRED INCOME - CALCULATIONS 
			' ----------------------------------------------------------------------------------------------------------------
			
			If sEntity.EndsWith("001") Then
	
				'1- Insertar datos
				api.Data.Calculate("E#"& sEntity &":A#2801600:F#F30:U1#None:U2#None:U3#None:U4#DEFINCOME:I#None:T#"& sTime &":V#Periodic:O#Import = (-1) * ("& defIncome &")", True)
				api.Data.Calculate("U4#DEFINCOME:F#F99I = U4#DEFINCOME:F#F99" , True)	
			
			End If
			
			#End Region

		End Sub

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

			' Corp Rate
			Dim data411 As String = String.Empty
			Dim i As Integer = 0
			
			For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".base", True)
			
				If i <> 0 Then
					data411 = data411 & " + "
				End If
				
				' Corp Rate														
				data411 = data411 & "RemoveZeros(E#" & miMember.Member.Name 	&":T#"& sTime 	&":A#411"	&":O#Top:I#Top:F#None:U1#Top:U2#Top:U3#Top:U4#Top:V#Periodic)"												

				i = i + 1
			
			Next
			
			If sEntity.EndsWith("001") Then				
				api.Data.Calculate("E#"& sEntityParent &"001:A#2901720:F#F20:T#"& sTime &":U1#None:U2#None:U3#None:U4#CORP:V#Periodic:O#Import:I#None = (-1)*(" & data411 &")", True)
				api.Data.Calculate("U4#CORP:F#F99I = U4#CORP:F#F99" , True)	
			End If
			
		End Sub
	#End Region		

#End Region

#End Region

	End Class
End Namespace