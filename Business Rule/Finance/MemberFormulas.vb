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

Namespace OneStream.BusinessRule.Finance.MemberFormulas
	
	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			
			Try
				
				Dim sFunction As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Function")
				
				'If (Not api.Entity.HasChildren) And api.Cons.IsLocalCurrencyForEntity() Then

					If sFunction = "CashFlowFormulas" Then
						
						'Me.CashFlowFormulas(si, api, globals, args)
						Me.CashFlowMaps(si, api, globals, args)
						
					Else If sFunction = "Account_F_Mapping" Then
					
						Me.AccountMapping(si, api, globals, args)
				
					End If
					
				'End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
'Seccion CF buscar abajo lines 352
		
#Region "Member Formulas for Ownership Parameters"
		Sub AcctParameter (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ByRef account As String, Optional ByRef number As Decimal = 100)
	'		If Not api.Members.GetMemberId(dimtypeid.Account, account).Equals(DimConstants.Unknown) Then
	'			If api.Entity.HasChildren Then
	'				api.Data.Calculate("V#YTD:A#" & account & ":F#None:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = " & number.ToString(CultureInfo.InvariantCulture))
	'				For Each oParentMember As Member In api.Members.GetParents(api.Pov.EntityDim.DimPk, api.Pov.Entity.MemberId, False)
	'					If api.Entity.IsIC(oParentMember.MemberId) Then
	'						api.Data.Calculate("V#YTD:A#" & account & ":F#None:O#Import:I#" & oParentMember.Name & ":U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = " & number.ToString(CultureInfo.InvariantCulture))
	'					End If	
	'				Next
	'			End If	
	'		End If
		'api.Data.ClearCalculatedData(true,true,true,"A#PctCON,A#PctOWN,A#PctMIN,A#Method")
		End Sub
	 
		'Dim FSK As New OneStream.BusinessRule.Finance.XFW_FSK_MemberFormulas.MainClass
		'FSK.AcctMinParameter(si, api, globals, args, "PctMin", "PctCon", "PctOwn")
		Sub AcctMinParameter (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs, ByRef minAccount As String, ByRef conAccount As String, ByRef ownAccount As String)
			If Not (api.Members.GetMemberId(dimtypeid.Account, minAccount).Equals(DimConstants.Unknown) _
			 OrElse api.Members.GetMemberId(dimtypeid.Account, conAccount).Equals(DimConstants.Unknown)  _
			 OrElse api.Members.GetMemberId(dimtypeid.Account, ownAccount).Equals(DimConstants.Unknown)) Then
			api.data.calculate("A#" & minAccount & "=A#" & conAccount & "-A#" & ownAccount)
			End If
		End Sub
#End Region	

#Region "Member Formulas for Flows"
		 
			Sub FlowF00 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		 
				Dim referenceBR As New OneStream.BusinessRule.Finance.FSK_OpeningScenario.MainClass	
 				Dim OpeScenario As String = referenceBR.fctOpeningScenario(si, api, args)
			
				'Calculate the Openings = CLO N-1 - Carry forward
				api.Data.ClearCalculatedData("F#F00",True,True,True)
				api.Data.Calculate("F#F00 = F#F99I:T#PovPriorYearM12:S#" & OpeScenario & "")
						
			End Sub
			
			Function FlowF00DDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		 
				'Simple formula drill down
				Dim result As New DrillDownFormulaResult()
		 
				'Define Explanation to be shown in drill title bar
				result.Explanation = "Flow F00"
		 
				'Add a row below per each drill result to be shown 
				result.SourceDataCells.Add("F#F99I:T#PovPriorYearM12")
				Return result
			End Function
		 
			Sub FlowF10(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		
				Dim sEntity As String = Api.Pov.Entity.Name
				Dim sEntityParent As String = Left(sEntity,5)
				
				Dim sCalc1 As String = String.Empty
				Dim sCalc2 As String = String.Empty
				
				Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString
				'NOVANEW
				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
				Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
				Dim MesForecastNumber As String = api.Scenario.GetWorkflowNumNoInputTimePeriods.ToString
'				Dim MesFcst As String = CurYear.ToString & "M" & MesForecastNumber.ToString
'				Dim MesCorriente As String = api.Pov.Time.Name

				Dim MesFcst As Integer = Integer.Parse(MesForecastNumber.ToString)
				Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
				
				'brapi.ErrorLog.LogMessage(si, "2 - " & MesFcst & " - Mes corr - " & MesCorriente.Name)
					
				Dim i As Integer = 0
				For Each miMember As MemberInfo In BRApi.Finance.Metadata.GetMembersUsingFilter(si, "Entities", "E#" & sEntityParent & ".Base", True)
									
					If i <> 0 Then
						sCalc1 = sCalc1 & " + "
						sCalc2 = sCalc2 & " + "
					End If
					
					sCalc1 = sCalc1 & "RemoveZeros(E#" & miMember.Member.Name & ":A#PL:F#None:I#Top:UD1#Top:UD3#Top)"
					sCalc2 = sCalc2 & "RemoveZeros(E#" & miMember.Member.Name & ":A#PL:O#AdjInput:F#None:I#Top:UD1#Top:UD3#Top)"									
					'Brapi.ErrorLog.LogMessage(si, "sCalc1: " & sCalc1)				
					'Brapi.ErrorLog.LogMessage(si, "sCalc2: " & sCalc2)			
					
					i = i+1
					
				Next				
				
				If ScenarioType.Equals("Forecast") Then
					If MesCorriente <= MesFcst Then 'IF 2024M1 < 2024M5 si es uno de los meses que no puede tocar, calcula como actual
						If Not api.Entity.HasChildren() Then
						
						 	If sEntity.EndsWith("001") Then
								
								api.Data.ClearCalculatedData("A#3001080C:F#F10",True,False,False)
						
								If  api.Cons.IsLocalCurrencyforEntity() Then
									api.Data.Calculate("A#3001080C:F#F10:I#None:UD1#None:UD3#None = " & sCalc1 )
									
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
									
								ElseIf  (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD1#None:UD3#None = " & sCalc2 )
									'NOVA - NETINCOME
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
								
								ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Then '			Queda pendiente por las cuentas de Reservas y Minoritarios
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD1#None:UD3#None = " & sCalc2 )
									'NOVA - NETINCOME
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
				
								End If									
							End If
						Else If api.Entity.HasChildren() Then
							If  api.Cons.IsLocalCurrencyforEntity()  Then 
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
							Else If (api.Pov.Cons.Name = "OwnerPreAdj") Or (api.Pov.Cons.Name = "OwnerPostAdj") Then
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
							End If
						End If
					Else 'calcula como Forecast
						If Not api.Entity.HasChildren() Then
							
						 	If sEntity.EndsWith("001") Then
								
								api.Data.ClearCalculatedData("A#3001080C:F#F10",True,False,False)
						
								If  api.Cons.IsLocalCurrencyforEntity() Then
									api.Data.Calculate("A#3001080C:F#F10:I#None:UD1#None:UD3#None = " & sCalc1 )
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD1#None:UD3#None = " & sCalc2 )
									'NOVA - NETINCOME
									api.Data.Calculate("A#3001080C:F#F99i:I#None:UD1#None:UD3#None = " & sCalc1 )
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
								
								ElseIf  (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD1#None:UD3#None = " & sCalc2 )
									'NOVA - NETINCOME
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
								
								ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Then '			Queda pendiente por las cuentas de Reservas y Minoritarios
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD1#None:UD3#None = " & sCalc2 )
									'NOVA - NETINCOME
									api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
				
								End If									
							End If
						Else If api.Entity.HasChildren() Then
							If  api.Cons.IsLocalCurrencyforEntity()  Then 
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
							Else If (api.Pov.Cons.Name = "OwnerPreAdj") Or (api.Pov.Cons.Name = "OwnerPostAdj") Then
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
							End If
						End If
					End If
				Else If ScenarioType.Equals("Actual") Or ScenarioType.Equals("Budget") Or ScenarioType.Equals("ScenarioType2") Then
					If Not api.Entity.HasChildren() Then
						
					 	If sEntity.EndsWith("001") Then
							
							api.Data.ClearCalculatedData("A#3001080C:F#F10",True,False,False)
					
							If  api.Cons.IsLocalCurrencyforEntity() Then
								api.Data.Calculate("A#3001080C:F#F10:I#None:UD1#None:UD3#None = " & sCalc1 )
								
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
								
							ElseIf  (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD1#None:UD3#None = " & sCalc2 )
								'NOVA - NETINCOME
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
							
							ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Then '			Queda pendiente por las cuentas de Reservas y Minoritarios
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD1#None:UD3#None = " & sCalc2 )
								'NOVA - NETINCOME
								api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD1#None:UD3#None = " & sCalc2 )
			
							End If									
						End If
					Else If api.Entity.HasChildren() Then
						If  api.Cons.IsLocalCurrencyforEntity()  Then 
							api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
							api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
						Else If (api.Pov.Cons.Name = "OwnerPreAdj") Or (api.Pov.Cons.Name = "OwnerPostAdj") Then
							api.Data.Calculate("A#3001080C:O#AdjInput:F#F10:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
							api.Data.Calculate("A#3001080C:O#AdjInput:F#F99i:I#None:UD3#None = RemoveZeros(A#PL:O#AdjInput:F#None:I#Top:UD3#Top)")
						End If
					End If
				End If
		 
			End Sub
			
			Function flowF10DDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
				Dim result As New DrillDownFormulaResult()
				If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then
		 
					'Define Explanation to be shown in drill title bar
					result.Explanation = "Flow F10"
		 
					'Add a row below per each drill result to be shown 
					result.SourceDataCells.Add("A#PL:F#None")
					Return result
				End If
				Return result
			End Function	
			Sub flowF80C(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
				Dim sScenarioType As String = api.Pov.ScenarioType.Name	
				Dim Time As String = api.Pov.Time.Name
				Dim Year As Integer = Time.Substring(0, 4)
				Dim Cube As String = api.Pov.Cube.Name
				If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
					'Dim Scenario As String = api.pov.scenario.name	
					Dim FX As New GlobalClass(si, api,args)
					Dim F00_TC_TCO As String = "(F#F00:C#Local{1} * " & fx.closingRateToText & "-F#F00{0})"
					Dim F05_TC_TCO As String = "(F#F05:C#Local{1} * " & FX.averageRateToText & "-F#F05:C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
					Dim F06_TC_TCO As String = "(F#F06:C#Local{1} * " & FX.averageRateToText & "-F#F06:C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
					'Dim FUS_TC_TCO As String = "(F#" & flow.FUS.Name & ":C#Local{1} * " & fx.closingRateToText & "-F#" & flow.FUS.Name & "{0})"
					'Calculate FX on Opening: OUV Local * (Closing Rate - opening Rate) and same for ENT and FTA
					Dim sExpression As String = "F#F80C{0}=" & F00_TC_TCO & " + " & F05_TC_TCO & "+ " & F06_TC_TCO & ""
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
					'SB - Aqui iria la cuenta de diferencias de cambio del F10. del ejercicio
					'api.Data.ClearCalculatedData("A#" & Account.ResC.Name & ":F#F80C",True,True,True)
		
				End If
			End Sub
			Function flowF80CDDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
				Dim result As New DrillDownFormulaResult()
		 
				If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
					Dim FX As New GlobalClass(si, api,args)
		 
					'Define Explanation to be shown in drill title bar
					result.Explanation = "Flow ECO delta to Closing Rate: " & fx.closingRateToText
		 
					Dim F00_LC_TCO As String = "F#F00:C#Local"
					Dim F05_LC_TCO As String = "F#F05:C#Local"
					Dim F06_LC_TCO As String = "F#F06:C#Local"
					'Dim FUS_LC_TCO As String = "F#" & flow.FUS.Name & ":C#Local"
		 
					Dim F00_AC_TCO As String = "F#F00"
					Dim F05_AC_TCO As String = "F#F05"
					Dim F06_AC_TCO As String = "F#F06"
					'Dim FUS_AC_TCO As String = "F#" & flow.FUS.Name
		 
					Dim sOrigin As String
					If api.Pov.Origin.MemberId.Equals(dimconstants.AdjConsolidated) Then
						sOrigin = ":O#AdjInput"
					Else	
						sOrigin = ":O#" & api.Pov.Origin.Name
					End If	
					'Add a row below per each drill result to be shown 
					result.SourceDataCells.Add(F00_LC_TCO & sOrigin)
				'xx	result.SourceDataCells.Add(F05_LC_TCO & sOrigin)
					result.SourceDataCells.Add(F00_AC_TCO)
				'xx	result.SourceDataCells.Add(F05_AC_TCO)
		 
				End If
				Return result
			End Function
			Sub flowF80M (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
				If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
					Dim FX As New GlobalClass(si, api,args)
					Dim CLO_TC_TM As String = "(F#F99I:C#Local{1} * " & FX.closingRateToText & "-F#F99I:C#Local{1} * " & FX.averageRateToText & ")"
					Dim F00_TM_TC As String = "(F#F00:C#Local{1} * " & FX.averageRateToText & "-F#F00:C#Local{1} * " & FX.closingRateToText & ")"
		 
					
				    'Calculate FX on Activity: CLO Local * (Closing Rate - Average Rate) and Reverse the impact from OUV, ENT, FTA, AFF and DIV (which are included in CLO balance)
					Dim sExpression As String = "F#F80M{0}= " & CLO_TC_TM & "+" & F00_TM_TC & ""
					'AdjInput needs to be the source for AdjCondolidated to avoid double counting
					'Function String.Format runs the expression but updates the arguments. {O} is the first arg. {1} is the second in the list
					'Ex: string.format({0},"ZZZ","XXX") => "ZZZ" and string.format({1},"ZZZ","XXX") => "XXX"
					api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
					api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
		 
				End If
			End Sub
			Function flowF80MDDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
				Dim result As New DrillDownFormulaResult()
		 
				If (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
					Dim FX As New GlobalClass(si, api,args)
		 
					'Define Explanation to be shown in drill title bar
					result.Explanation = "Flow F80M delta to Closing Rate: " & fx.closingRateToText & " with Average Rate: " & FX.averageRateToText & " and prev. Years Average Rate: " & FX.averageRatePriorYEtoText
		 
					Dim CLO_LC As String = "F#F99I:C#Local"
					Dim F00_LC As String = "F#F00:C#Local"
		 
					Dim sOrigin As String
					If api.Pov.Origin.MemberId.Equals(dimconstants.AdjConsolidated) Then
						sOrigin = ":O#AdjInput"
					Else	
						sOrigin = ":O#" & api.Pov.Origin.Name
					End If	
		 
					'Add a row below per each drill result to be shown 
					result.SourceDataCells.Add(CLO_LC & sOrigin)
					result.SourceDataCells.Add(F00_LC & sOrigin)
		 
				End If
				Return result
			End Function
			Sub flowF14(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
			    
				Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString 'NOVA
				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
				Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
				'NOVANEW
				Dim MesForecastNumber As String = api.Scenario.GetWorkflowNumNoInputTimePeriods.ToString
				Dim MesFcst As Integer = Integer.Parse(MesForecastNumber.ToString)
				Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
				If ScenarioType.XFEqualsIgnoreCase("Forecast") And CurYear < 2025 Then
					If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then
						If MesCorriente <= MesFcst Then 'CALCULA COMO ACTUAL
							api.Data.ClearCalculatedData("O#Forms:F#F14", True, True, True, True)
							api.Data.ClearCalculatedData("O#Import:F#F14", True, True, True, True)
							api.Data.Calculate("O#Forms:F#F14:V#Periodic=RemoveZeros(O#Forms:F#F99I:V#Periodic)-RemoveZeros(O#Forms:F#F99:V#Periodic)")
							api.Data.Calculate("O#Import:F#F14:V#Periodic=RemoveZeros(O#Import:F#F99I:V#Periodic)-RemoveZeros(O#Import:F#F99:V#Periodic)")
							'brapi.ErrorLog.LogMessage(si,"Actual" & MesCorriente)
						Else 'CALCULA COMO FORECAST
							api.Data.ClearCalculatedData("O#Forms:F#F14", True, True, True, True)
							api.Data.ClearCalculatedData("O#Import:F#F14", True, True, True)
							api.Data.Calculate("O#Forms:F#F14=RemoveZeros(O#Forms:F#F02)*(-1)")
							'api.Data.Calculate("O#Import:F#F14:V#Periodic=RemoveZeros(O#Import:F#F99I:V#Periodic)-RemoveZeros(O#Import:F#F99:V#Periodic)")
							'brapi.ErrorLog.LogMessage(si,"Forecast" & MesCorriente)
						End If
					End If
				Else
					If ScenarioType.Equals("Actual") Then
						If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then
						api.Data.ClearCalculatedData("O#Forms:F#F14", True, True, True, True)
						api.Data.ClearCalculatedData("O#Import:F#F14", True, True, True, True)
						'api.Data.Calculate("O#Forms:F#F14=RemoveZeros(O#Forms:F#F99I)-RemoveZeros(O#Forms:F#F99)")
						'api.Data.Calculate("O#Import:F#F14=RemoveZeros(O#Import:F#F99I)-RemoveZeros(O#Import:F#F99)")
						api.Data.Calculate("O#Forms:F#F14:V#Periodic=RemoveZeros(O#Forms:F#F99I:V#Periodic)-RemoveZeros(O#Forms:F#F99:V#Periodic)")
						api.Data.Calculate("O#Import:F#F14:V#Periodic=RemoveZeros(O#Import:F#F99I:V#Periodic)-RemoveZeros(O#Import:F#F99:V#Periodic)")
						End If
					End If
					
					If ScenarioType.Equals("ScenarioType2")  Then 
						If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then
						api.Data.ClearCalculatedData("O#Forms:F#F14", True, True, True, True)
						api.Data.ClearCalculatedData("O#Import:F#F14", True, True, True, True)
						api.Data.Calculate("O#Forms:F#F14:V#YTD=RemoveZeros(O#Forms:F#F99I:V#YTD)-RemoveZeros(O#Forms:F#F99:V#YTD)")
						api.Data.Calculate("O#Import:F#F14:V#YTD=RemoveZeros(O#Import:F#F99I:V#YTD)-RemoveZeros(O#Import:F#F99:V#YTD)")
						End If
					End If							   
				End If
			End Sub
		 
			Function flowF14DDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
		 
				Dim result As New DrillDownFormulaResult()
		 
				If (api.Cons.IsLocalCurrencyforEntity() And Not api.Entity.HasChildren()) Then
		 
					'Define Explanation to be shown in drill title bar
					result.Explanation = "Flow F14 "
		 
					'Add a row below per each drill result to be shown 
					result.SourceDataCells.Add("F#F99I")
					result.SourceDataCells.Add("F#F99")
				End If
				Return result
			End Function
			Sub FlowF02 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
				api.Data.ClearCalculatedData("F#F02",False,True,True,True)
				'api.Data.ClearCalculatedData("F#F02",False,True,True,False)
			End Sub
			Sub flowF99I(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		 		
				Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString 'NOVA
				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
				Dim CurYear As Integer = api.Time.GetYearFromId(timeId)	
				'NOVANEW
				Dim MesForecastNumber As String = api.Scenario.GetWorkflowNumNoInputTimePeriods.ToString
				Dim MesFcst As Integer = Integer.Parse(MesForecastNumber.ToString)
				Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
				
				If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
					api.Data.ClearCalculatedData("O#AdjInput:F#F99I",False,True,False)
					api.data.calculate("O#AdjInput:F#F99I=O#AdjInput:F#F99")
					'NOVA > en forecast no se carga y para darle integridad a los flujos, se calcula
					If (ScenarioType.Equals("Forecast") And CurYear > 2023) Or ScenarioType.Equals("Budget") Then
						If MesCorriente > MesFcst Then 'solo lo calcula para los mess que no son copia de actual
							api.Data.ClearCalculatedData("F#F99I",True,True,True)
							api.data.calculate("F#F99I=F#F99")
						End If
					End If
				
				Else
				    'Calculate CLO: CLO = Sum of all Flows
					api.Data.ClearCalculatedData("F#F99I",False,True,False) 'Should not have to clear local data or consolidated data, only translated as translated is calculated first by default engine.
					api.data.calculate("F#F99I=F#F99")
				End If
			End Sub	
			Function flowF99IDDP(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult
				Dim result As New DrillDownFormulaResult()
		 
				If (Not api.Entity.HasChildren() And api.Cons.IsLocalCurrencyforEntity()) Then
				Else	
					'Define Explanation to be shown in drill title bar
					result.Explanation = "Flow CLO"
					'Add a row below per each drill result to be shown 
					result.SourceDataCells.Add("F#F99")
		 
				End If
				Return result
			End Function
			
			'NOVA añadido para la conversion del DBE al tipo del año anterior
			Sub flowF05(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

				Dim CurrTime As String = api.Pov.Time.Name
				Dim CurrYear As Integer = TimeDimHelper.GetSubComponentsFromName(CurrTime).Year
				Dim Flow As Object = Me.InitFlowClass(si, api, globals)
				Dim FX As New GlobalClass(si, api,args)
		
				If Not api.Entity.HasChildren() Then
					If api.Cons.IsLocalCurrencyforEntity() Then
						'Cuando el dato es local, de momento no hacemos nada
						
'						Dim CuentasDBE As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "Capital"))
'						Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
'						Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
'			 			If CurYear > 2019 Then
'							For Each Account As Member In CuentasDBE 
'								If api.Account.Text(Account.MemberId, 1) = "DBE_AUT" Then
		
'									api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Forms:F#" & flow.Mvt.Name & "",True,True,True)
'									api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Import:F#" & flow.Mvt.Name & "",True,True,True)
		
'									api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Forms:F#" & flow.Aff.Name & "",True,True,True)
'								 	api.Data.Calculate("A#"+Account.Name.ToString+":O#Forms:F#" & flow.Aff.Name & "=RemoveZeros(A#"+Account.Name.ToString+":O#Forms:F#" & flow.CLO.Name & ")-RemoveZeros(A#"+Account.Name.ToString+":O#Forms:F#" & flow.TF.Name & ")" & "")
'									api.Data.ClearCalculatedData("A#"+Account.Name.ToString+":O#Import:F#" & flow.Aff.Name & "",True,True,True)
'								 	api.Data.Calculate("A#"+Account.Name.ToString+":O#Import:F#" & flow.Aff.Name & "=RemoveZeros(A#"+Account.Name.ToString+":O#Import:F#" & flow.CLO.Name & ")-RemoveZeros(A#"+Account.Name.ToString+":O#Import:F#" & flow.TF.Name & ")" & "")
'								End If
'							Next
'					 	End If
					
					ElseIf (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
						If(api.Pov.Entity.Name.StartsWith("R05920")) Then 'si es argentina tiene que que convertir como el rsto de flujos
							
							Dim sExpression As String = "F#" & Flow.RPY.Name & "{0}=F#" & Flow.RPY.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText
							api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
							api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
							
						Else
							
							Dim sExpression As String = "F#" & Flow.RPY.Name & "{0}=F#" & Flow.RPY.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText
							api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
							api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
							
							Dim sExpressionF06 As String = "F#" & Flow.DIV.Name & "{0}=F#" & Flow.DIV.Name & ":C#Local{1} * " & FX.averageRatePriorYEtoText
							api.data.calculate(String.Format(sExpressionF06,"",""),,,"O#Top.Base.Remove(AdjInput)")
							api.data.calculate(String.Format(sExpressionF06, ":O#AdjConsolidated",":O#AdjInput"))
						End If
						
						
						
					'No hacemos nada tampoco en ownerpostadj
						
'					ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Then
				
'						Dim Account As Object = Me.InitAccountClass(si,api, globals)
		
'						'APE reclass to DBE and moved to 120AA at OwnerPostAdj level- fs20201218
'						api.Data.ClearCalculatedData("A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
'						api.Data.Calculate("A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Ouv.Name)
'						api.Data.ClearCalculatedData("A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
'						api.Data.Calculate("A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Ouv.Name)
'						api.Data.ClearCalculatedData("A#" & Account.RsvG.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
'						api.Data.Calculate("A#" & Account.RsvG.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResG.name & ":O#AdjInput:F#" & Flow.Aff.Name)
'						api.Data.ClearCalculatedData("A#" & Account.RsvM.name & ":O#AdjInput:F#" & Flow.Aff.Name & "",True,True,True)
'						api.Data.Calculate("A#" & Account.RsvM.name & ":O#AdjInput:F#" & Flow.Aff.Name & " = -A#" & Account.ResM.name & ":O#AdjInput:F#" & Flow.Aff.Name)
					End If
				End If
			End Sub
			
	 
#End Region
		
#Region "Member formula for BS Accounts"
	
		Sub AcctRdoEj (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Account As Object = Me.InitAccountClass(si, api, globals)
		'Calc result of the year & retained earnings
		Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString 'NOVA
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)	
		
		'NOVANEW
		Dim MesForecastNumber As String = api.Scenario.GetWorkflowNumNoInputTimePeriods.ToString
		Dim MesFcst As Integer = Integer.Parse(MesForecastNumber.ToString)
		Dim MesCorriente As Integer = Integer.Parse(api.Pov.Time.Name.Split("M")(1))
		
		If ScenarioType.Equals("Forecast") Then
			If Not api.Entity.HasChildren() Then
				If api.Cons.IsLocalCurrencyforEntity() Then
					If MesCorriente <= MesFcst Then 'Lo calcula como Actual
						'BS Calculo DBE
						api.Data.Calculate("A#" & Account.ResEx.Name & ":F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":F#" & Flow.Ope.Name & "")
						api.Data.Calculate("A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")

						'NOVA Lo ponemos en la cuenta de reservas con el signo contrario para revertir el efecto
						api.Data.Calculate("A#3001060CF:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":F#" & Flow.Ope.Name & "")
						api.Data.Calculate("A#3001060CF:O#AdjInput:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
					Else 'Lo calcula como forecast solo para los meses que no son copia de Actual
	 
						'BS Calculo DBE
						api.Data.Calculate("A#" & Account.ResEx.Name & ":F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":F#" & Flow.Ope.Name & "")
						api.Data.Calculate("A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
	
						'NOVA Lo ponemos en la cuenta de reservas con el signo contrario para revertir el efecto
						api.Data.Calculate("A#3001060CF:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":F#" & Flow.Ope.Name & "")
						api.Data.Calculate("A#3001060CF:O#AdjInput:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
						
						
						'Pendiente Ver
						'Prior Year Result for Adjustments 12000AA 
						'api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "= - 1* A#" & Acct.ResEx.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "")
						'api.data.ClearCalculatedData ("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & ":U4#RetSoc1_PL", True, True, True)
						'api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Clo.Name & "= A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.TF.Name & "")
	 				End If
					
				ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Or (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
				'only for AdjInput
					If MesCorriente <= MesFcst Then 'Lo calcula como Actual
						'BS Calculo DBE
						api.Data.Calculate("A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
						'NOVA Lo ponemos en la cuenta de reservas con el signo contrario para revertir el efecto
						api.Data.Calculate("A#3001060CF:O#AdjInput:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
	 
					Else 'Lo calcula como forecast solo para los meses que no son copia de Actual
						'BS Calculo DBE
						api.Data.Calculate("A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
						'NOVA Lo ponemos en la cuenta de reservas con el signo contrario para revertir el efecto
						api.Data.Calculate("A#3001060CF:O#AdjInput:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
		 
						'Prior Year Result 12000AA
						'api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "= - 1* A#" & Acct.ResEx.Name & ":O#AdjInput:F#" & Flow.Aff.Name & "")
						'api.data.ClearCalculatedData ("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Aff.Name & ":U4#RetSoc1_PL", True, True, True) '20220412 NP Lu
						'api.Data.Calculate("A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.Clo.Name & "= A#" & Acct.RsvR.Name & ":O#AdjInput:F#" & Flow.TF.Name & "")
	 				End If
				End If
			Else
				'calcula en padres (LEGAL) los asientos en cuentas 6 y 7 en 2AGUG		
				'api.Data.Calculate("A#" & Acct.ResG.Name & ":O#AdjInput:F#" & Flow.Res.Name & ":I#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None=A#" & Acct.TopIS.Name & ":O#AdjInput:F#None:I#Top:U3#Top:U4#Legal:U5#None:U6#None:U7#Top:U8#None")
			End If
		Else If ScenarioType.Equals("Actual") Or ScenarioType.Equals("ScenarioType2") Or ScenarioType.Equals("Budget") And CurYear > 2023 Then
			If Not api.Entity.HasChildren() Then
				If api.Cons.IsLocalCurrencyforEntity() Then
	 
					'BS Calculo DBE
					api.Data.Calculate("A#" & Account.ResEx.Name & ":F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":F#" & Flow.Ope.Name & "")
					api.Data.Calculate("A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")

					'NOVA Lo ponemos en la cuenta de reservas con el signo contrario para revertir el efecto
					api.Data.Calculate("A#3001060CF:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":F#" & Flow.Ope.Name & "")
					api.Data.Calculate("A#3001060CF:O#AdjInput:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
							
				ElseIf  (api.Pov.Cons.Name = "OwnerPostAdj") Or (api.Pov.Cons.Name = "OwnerPreAdj") Or api.cons.IsForeignCurrencyForEntity Then
				'only for AdjInput
					
					'BS Calculo DBE
					api.Data.Calculate("A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.RPY.Name & "=-A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
'					'NOVA Lo ponemos en la cuenta de reservas con el signo contrario para revertir el efecto
					api.Data.Calculate("A#3001060CF:O#AdjInput:F#" & Flow.RPY.Name & "=A#" & Account.ResEx.Name & ":O#AdjInput:F#" & Flow.Ope.Name & "")
	 
				End If
			End If
		End If
	End Sub	

	Sub AcctRsvC (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim acct As Object = Me.InitAccountClass(si, api, globals)
		Dim CurrTime As String = api.Pov.Time.Name
		Dim CurrYear As Integer = TimeDimHelper.GetSubComponentsFromName(CurrTime).Year

		Dim FX As New GlobalClass(si, api,args)

		If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) And  api.Pov.Entity.Name.StartsWith("R05920") Then
			'Dim ComptesBilan As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, acct.TopBS.MemberId)
			
			api.Data.ClearCalculatedData("A#DIFCON:F#" & Flow.TDO.name &"",True,True,True)
			api.Data.ClearCalculatedData("A#DIFCON:F#" & Flow.TDM.name & "",True,True,True)
			
			'--------------------------------------------------------
		       		'II-4-2- Equity translation
		    '--------------------------------------------------------
			'Capitaux Propres hors Resultat
			Dim CPAPE_TC_TCO As String = "(A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & fx.closingRateToText & "-A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
			'Dim CPAPE_TC_TCOR As String = "(A#EquityFX:V#YTD:F#F00R:C#Local{1} * " & fx.closingRateToText & "-A#EquityFX:V#YTD:F#F00R:C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
			Dim CPDBE_TC_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.closingRatetoText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
			'Dim CPDBE_TM_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			'Dim CPDBE_TMC_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			Dim CPDIV_TC_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.closingRatetoText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
			
			
			Dim sExpression As String = "A#DIFCON:V#YTD:F#" & Flow.TDO.name & "{0}=" & CPAPE_TC_TCO  & "+" & CPDBE_TC_TM1
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
				

			Dim CPCLO_TC_TM As String = "(A#EquityFX:V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#EquityFX:V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText & ")"
			Dim CPAPE_TM_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPDBE_TM_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPDIV_TM_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPDBE_TM_TC1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.closingRatePriorYEtoText & ")"
			Dim CPDIV_TM_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			Dim CPFPE_FPL_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.ELC.name & ":C#Local{1}) * " & FX.averageRateToText & " - A#EquityFX:V#YTD:F#" & Flow.ECE.name & ":C#Local{1}) * 1)"
 
			sExpression = "A#DIFCON:V#YTD:F#" & Flow.TDM.name & "{0}=" & CPCLO_TC_TM & "+" & CPAPE_TM_TC & "+" & CPDBE_TM_TC & "+" & CPDIV_TM_TC &"+" & CPFPE_FPL_TC & "+" & CPDBE_TM_TC1 & "-" & CPDIV_TM_TM1
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

			sExpression = "A#DIFCON:V#YTD:F#" & Flow.RPY.name & "{0}=A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
			sExpression = "A#DIFCON:V#YTD:F#" & Flow.DIV.name & "{0}=A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
			
			'--------------------------------------------------------
					'II-4-1- Net income translation
			'--------------------------------------------------------
'			sExpression = "A#" & Acct.ResC.name & ":V#YTD:F#" & Flow.RPY.name & "{0}=A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
'			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
'			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			sExpression = "A#" & Acct.ResC.name & ":V#YTD:F#" & Flow.TDM.name & "{0}=A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
		Else If Not api.Entity.HasChildren() And (api.Cons.IsCurrency() And Not api.Cons.IsLocalCurrencyforEntity()) Then
			'Dim ComptesBilan As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, acct.TopBS.MemberId)
			
			api.Data.ClearCalculatedData("A#DIFCON:F#" & Flow.TDO.name &"",True,True,True)
			api.Data.ClearCalculatedData("A#DIFCON:F#" & Flow.TDM.name & "",True,True,True)
			
			'--------------------------------------------------------
		       		'II-4-2- Equity translation
		    '--------------------------------------------------------
			'Capitaux Propres hors Resultat
			Dim CPAPE_TC_TCO As String = "(A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & fx.closingRateToText & "-A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
			'Dim CPAPE_TC_TCOR As String = "(A#EquityFX:V#YTD:F#F00R:C#Local{1} * " & fx.closingRateToText & "-A#EquityFX:V#YTD:F#F00R:C#Local{1} * " & FX.closingRatePriorYEtoText & ")"
			Dim CPDBE_TC_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.closingRatetoText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
			'Dim CPDBE_TM_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			'Dim CPDBE_TMC_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			Dim CPDIV_TC_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.closingRatetoText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRatePriorYEtoText & ")"
			

			Dim sExpression As String = "A#DIFCON:V#YTD:F#" & Flow.TDO.name & "{0}=" & CPAPE_TC_TCO & "+" & CPDBE_TC_TM1 & "+" & CPDIV_TC_TM1 '& "+" & CPAPE_TC_TCOR
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
		
				
'			Dim CPCLO_TC_TM As String = "(A#EquityFX:I#Top:V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#EquityFX:I#Top:V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText & ")"
'			Dim CPAPE_TM_TC As String = "(A#EquityFX:I#Top:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:I#Top:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & fx.closingRateToText & ")"
'			Dim CPFPE_FPL_TC As String = "(A#EquityFX:I#Top:V#YTD:F#" & Flow.ELC.name & ":C#Local{1}) * " & FX.averageRateToText & " - A#EquityFX:I#Top:V#YTD:F#" & Flow.ECE.name & ":C#Local{1}) * 1)"

'			sExpression = "A#DIFCON:I#None:V#YTD:F#" & Flow.TDM.name & "{0}=" & CPCLO_TC_TM & "+" & CPAPE_TM_TC & "+" & CPFPE_FPL_TC
'			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
'			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

			Dim CPCLO_TC_TM As String = "(A#EquityFX:V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#EquityFX:V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText & ")"
			Dim CPAPE_TM_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.OPE.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPDBE_TM_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPDIV_TM_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.closingRateToText & ")"
			Dim CPDBE_TM_TC1 As String = "(A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.closingRatePriorYEtoText & ")"
			Dim CPDIV_TM_TM1 As String = "(A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.averageRateToText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText & ")"
			Dim CPFPE_FPL_TC As String = "(A#EquityFX:V#YTD:F#" & Flow.ELC.name & ":C#Local{1}) * " & FX.averageRateToText & " - A#EquityFX:V#YTD:F#" & Flow.ECE.name & ":C#Local{1}) * 1)"
 
			sExpression = "A#DIFCON:V#YTD:F#" & Flow.TDM.name & "{0}=" & CPCLO_TC_TM & "+" & CPAPE_TM_TC & "+" & CPDBE_TM_TC & "+" & CPDIV_TM_TC &"+" & CPFPE_FPL_TC & "+" & CPDBE_TM_TC1 & "-" & CPDIV_TM_TM1
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))

			sExpression = "A#DIFCON:V#YTD:F#" & Flow.RPY.name & "{0}=A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#EquityFX:V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
			sExpression = "A#DIFCON:V#YTD:F#" & Flow.DIV.name & "{0}=A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#EquityFX:V#YTD:F#" & Flow.DIV.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
			
			'--------------------------------------------------------
					'II-4-1- Net income translation
			'--------------------------------------------------------
'			sExpression = "A#" & Acct.ResC.name & ":V#YTD:F#" & Flow.RPY.name & "{0}=A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & FX.closingRatePriorYEtoText & "-A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.RPY.name & ":C#Local{1} * " & fx.averageRatePriorYEtoText
'			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
'			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
			sExpression = "A#" & Acct.ResC.name & ":V#YTD:F#" & Flow.TDM.name & "{0}=A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & fx.closingRateToText & "-A#" & Acct.ResR.name & ":V#YTD:F#" & Flow.CLO.name & ":C#Local{1} * " & FX.averageRateToText
			api.data.calculate(String.Format(sExpression,"",""),,,"O#Top.Base.Remove(AdjInput)")
			api.data.calculate(String.Format(sExpression, ":O#AdjConsolidated",":O#AdjInput"))
			
		End If
	End Sub

			Sub CashFlowFormulas (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		'If (api.Cons.IsLocalCurrencyforEntity()) Then	
			'Primero borro todo
			api.Data.ClearCalculatedData("A#TCAF01",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF17",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF02",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF08",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF03",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF04",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF05",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF09",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF11",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF13",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF14",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF06",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF10",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF12",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF15",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF16",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF18",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF19",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF20",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF21",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF22",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAF07",True,True,True,True)
			api.Data.ClearCalculatedData("A#THCAF01",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX10",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX11",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX12",True,True,True,True)
			api.Data.ClearCalculatedData("A#TDPI01",True,True,True,True)
			api.Data.ClearCalculatedData("A#TDPI02",True,True,True,True)
			api.Data.ClearCalculatedData("A#TDPI03",True,True,True,True)
			api.Data.ClearCalculatedData("A#TDPI04",True,True,True,True)
			api.Data.ClearCalculatedData("A#TDPI05",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV02LLD1",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV044",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX13",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX01",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX02",True,True,True,True)			
			api.Data.ClearCalculatedData("A#TFEX03",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX04",True,True,True,True)			
			api.Data.ClearCalculatedData("A#TFEX05",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX06",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX07",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX08",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX14",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX09",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV02FRD1",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV02PAI2",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV023",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV042",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV043",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV11",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV01",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV08",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV09",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV10",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV15",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV06",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV07",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV05",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV16",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV03",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV12",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV13",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV14",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN06",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN05",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN07",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN10",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN04",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN01",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN02",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN03",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINFI",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFIN11",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE01",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE02",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE03",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE04",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE05",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE07",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE08",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE17",True,True,True,True)
			api.Data.ClearCalculatedData("A#TTRE06",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV02",True,True,True,True)
			api.Data.ClearCalculatedData("A#TINV04",True,True,True,True)
			api.Data.ClearCalculatedData("A#TCAFAPCRES",True,True,True,True)
			api.Data.ClearCalculatedData("A#TFEX",True,True,True,True)
			
			'Calculo Para BudgetV1
			'Las cuentas de detalle para Budgetv1 estan en #UD4 Details
			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString 
			Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
			Dim CurYear As Integer = api.Time.GetYearFromId(timeId)	
			
			If ScenarioType.Equals("Budget") And CurYear = 2024 Then
				
			'Calculo
			api.Data.Calculate("A#TCAF01:F#None:UD4#CF_Autom:V#YTD = A#3001090C:F#F10:UD4#Top:V#YTD
			+A#3001080C:F#F10:UD4#Top:V#YTD
			+A#3021080C:F#F10:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF02:F#None:UD4#CF_Autom:V#YTD = -A#1001004C:F#F25:UD4#Top:V#YTD
			-A#1001004C:F#F35:UD4#Top:V#YTD
			-A#1021004C:F#F25:UD4#Top:V#YTD
			-A#1021004C:F#F35:UD4#Top:V#YTD
			-A#1041004C:F#F25:UD4#Top:V#YTD
			-A#1041004C:F#F35:UD4#Top:V#YTD
			-A#1061004C:F#F25:UD4#Top:V#YTD
			-A#1061004C:F#F35:UD4#Top:V#YTD
			-A#1071004C:F#F25:UD4#Top:V#YTD
			-A#1071004C:F#F35:UD4#Top:V#YTD
			-A#1071005C:F#F25:UD4#Top:V#YTD
			-A#1071005C:F#F35:UD4#Top:V#YTD
			-A#1091005C:F#F25:UD4#Top:V#YTD
			-A#1091005C:F#F35:UD4#Top:V#YTD
			-A#1031004C:F#F25:UD4#Top:V#YTD
			-A#1031005C:F#F25:UD4#Top:V#YTD
			-A#1031005C:F#F35:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF03:F#None:UD4#CF_Autom:V#YTD = -A#1521050C:F#F25:UD4#Top:V#YTD
			-A#1521010C:F#F25:UD4#Top:V#YTD
			-A#1521004C:F#F15:UD4#Top:V#YTD
			-A#1521020C:F#F25:UD4#Top:V#YTD
			-A#1521004C:F#F25:UD4#Top:V#YTD
			-A#1521004C:F#F37:UD4#Top:V#YTD
			-A#1521004C:F#F39:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF04:F#None:UD4#CF_Autom:V#YTD = -A#581107R:F#None:UD4#Top:V#YTD
			-A#1101004C:F#F30:UD4#Top:V#YTD
			-A#481:F#None:UD4#Top:V#YTD
			-A#482:F#None:UD4#Top:V#YTD
			-A#4892:F#None:UD4#Top:V#YTD
			-A#487:F#None:UD4#Top:V#YTD
			-A#1571004C:F#F15:UD4#Top:V#YTD
			-A#581110R:F#None:UD4#Top:V#YTD
			-A#581110R:F#None:UD4#Top:V#Periodic
			-A#1171004C:F#F15:UD4#Top:V#YTD
			-A#1101114C:F#F30:UD4#Top:V#YTD
			-A#1131004C:F#F30:UD4#Top:V#YTD
			-A#4899:F#None:UD4#Top:V#YTD
			-A#1581004C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF05:F#None:UD4#CF_Autom:V#YTD = A#2061000C:F#F15:UD4#Top:V#YTD
			+A#2041000C:F#F15:UD4#Top:V#YTD
			+A#2501000C:F#F15:UD4#Top:V#YTD
			+A#2521000C:F#F15:UD4#Top:V#YTD
			-A#4934:F#None:UD4#Top:V#YTD
			-A#2061000CF:F#F25:UD4#Top:V#YTD
			-A#2521000CF:F#F25:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF06:F#None:UD4#CF_Autom:V#YTD = -A#1111004C:F#F25:UD4#Top:V#YTD
			-A#1111004C:F#F35:UD4#Top:V#YTD
			-A#1641204C:F#F15:UD4#Top:V#YTD
			-A#1221104C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF07:F#None:UD4#CF_Autom:V#YTD = -A#1081000C:F#F06:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF08:F#None:UD4#CF_Autom:V#YTD = -A#1081000C:F#F10:UD4#Top:V#YTD
			-A#1101114C:F#F25:UD4#Top:V#YTD
			-A#1101114C:F#F35:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF09:F#None:UD4#CF_Autom:V#YTD = -A#1141000C:F#F15:UD4#Top:V#YTD
			-A#1141004C:F#F15:UD4#Top:V#YTD
			+A#2021000C:F#F15:UD4#Top:V#YTD
			-A#4117:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF11:F#None:UD4#CF_Autom:V#YTD = -A#4931:F#None:UD4#Top:V#YTD
			-A#4932:F#None:UD4#Top:V#YTD
			-A#4940:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF12:F#None:UD4#CF_Autom:V#YTD = -A#554106:F#None:UD4#Top:V#YTD
			-A#563113:F#None:UD4#Top:V#YTD
			-A#563113:F#None:UD4#Top:V#Periodic
			-A#554106:F#None:UD4#Top:V#Periodic",True)
			api.Data.Calculate("A#TCAF18:F#None:UD4#CF_Autom:V#YTD = -A#581108R:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF19:F#None:UD4#CF_Autom:V#YTD = -A#1101004C:F#F25:UD4#Top:V#YTD
			-A#1101004C:F#F35:UD4#Top:V#YTD
			-A#1131004C:F#F25:UD4#Top:V#YTD
			-A#1131004C:F#F35:UD4#Top:V#YTD
			-A#1132004C:F#F25:UD4#Top:V#YTD
			-A#1132004C:F#F35:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF20:F#None:UD4#CF_Autom:V#YTD = -A#4929:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF22:F#None:UD4#CF_Autom:V#YTD = -A#4943:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI01:F#None:UD4#CF_Autom:V#YTD = A#2601010C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI02:F#None:UD4#CF_Autom:V#YTD = A#2601010C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI03:F#None:UD4#CF_Autom:V#YTD = A#2621010C:F#F15:UD4#Top:V#YTD
			+A#2601010C:F#F15:UD4#Top:V#YTD
			+A#2621020C:F#F15:UD4#Top:V#YTD
			+A#2601110C:F#F15:UD4#Top:V#YTD
			+A#2601020C:F#F15:UD4#Top:V#YTD
			-A#1631400C:F#F15:UD4#Top:V#YTD
			+A#2631400C:F#F15:UD4#Top:V#YTD
			+A#2681010C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI04:F#None:UD4#CF_Autom:V#YTD = -A#1671000C:F#F15:UD4#Top:V#YTD
			-A#1671004C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI05:F#None:UD4#CF_Autom:V#YTD = -A#1641104C:F#F15:UD4#Top:V#YTD
			-A#1641100C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX01:F#None:UD4#CF_Autom:V#YTD = -A#1501000C:F#F15:UD4#Top:V#YTD
			-A#1501004C:F#F15:UD4#Top:V#YTD
			-A#1502000C:F#F15:UD4#Top:V#YTD
			-A#1502004C:F#F15:UD4#Top:V#YTD
			-A#1501009L:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX02:F#None:UD4#CF_Autom:V#YTD = -A#1541000C:F#F15:UD4#Top:V#YTD
			-A#1541004C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX03:F#None:UD4#CF_Autom:V#YTD = -A#1161000C:F#F15:UD4#Top:V#YTD
			-A#1161004C:F#F15:UD4#Top:V#YTD
			-A#1201000C:F#F15:UD4#Top:V#YTD
			-A#1181000C:F#F15:UD4#Top:V#YTD
			-A#1561000C:F#F15:UD4#Top:V#YTD
			-A#1621000C:F#F15:UD4#Top:V#YTD
			-A#1601000C:F#F15:UD4#Top:V#YTD
			+A#1561100:F#F15:UD4#Details:V#YTD
			+A#1561040:F#F15:UD4#Details:V#YTD
			+A#1561044:F#F15:UD4#Details:V#YTD
			+A#1561110:F#F15:UD4#Details:V#YTD
			-A#1561004C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX04:F#None:UD4#CF_Autom:V#YTD = A#2721000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX05:F#None:UD4#CF_Autom:V#YTD = A#2021000C:F#F80C:UD4#Top:V#YTD
			+A#2201000C:F#F80C:UD4#Top:V#YTD
			+A#2301000C:F#F80C:UD4#Top:V#YTD
			+A#2491100L:F#F80C:UD4#Top:V#YTD
			+A#3001040:F#F80C:UD4#Top:V#YTD
			+A#3001045:F#F80C:UD4#Top:V#YTD
			+A#3001070:F#F80M:UD4#Top:V#YTD
			+A#3001060C:F#F80M:UD4#Top:V#YTD
			+A#3001080C:F#F80M:UD4#Top:V#YTD
			+A#3001070E:F#F80M:UD4#Top:V#YTD
			+A#3021030C:F#F80M:UD4#Top:V#YTD
			+A#3021070:F#F80M:UD4#Top:V#YTD
			+A#3021080C:F#F80M:UD4#Top:V#YTD
			+A#2081000C:F#F80M:UD4#Top:V#YTD
			+A#2101000C:F#F80M:UD4#Top:V#YTD
			+A#2041000C:F#F80M:UD4#Top:V#YTD
			+A#2061000C:F#F80M:UD4#Top:V#YTD
			+A#2121010C:F#F80M:UD4#Top:V#YTD
			+A#2121020C:F#F80M:UD4#Top:V#YTD
			+A#2141010C:F#F80M:UD4#Top:V#YTD
			+A#2141020C:F#F80M:UD4#Top:V#YTD
			+A#2021000C:F#F80M:UD4#Top:V#YTD
			+A#2201000C:F#F80M:UD4#Top:V#YTD
			+A#2301000C:F#F80M:UD4#Top:V#YTD
			+A#2491100L:F#F80M:UD4#Top:V#YTD
			+A#3001040:F#F80M:UD4#Top:V#YTD
			+A#3001045:F#F80M:UD4#Top:V#YTD
			-A#1521004C:F#F80C:UD4#Top:V#YTD
			-A#1521010C:F#F80C:UD4#Top:V#YTD
			-A#1521020C:F#F80C:UD4#Top:V#YTD
			-A#1501000C:F#F80C:UD4#Top:V#YTD
			-A#1501004C:F#F80C:UD4#Top:V#YTD
			-A#1541000C:F#F80C:UD4#Top:V#YTD
			-A#1541004C:F#F80C:UD4#Top:V#YTD
			-A#1561000C:F#F80C:UD4#Top:V#YTD
			-A#1502000C:F#F80C:UD4#Top:V#YTD
			-A#1502004C:F#F80C:UD4#Top:V#YTD
			-A#1601000C:F#F80C:UD4#Top:V#YTD
			-A#1621000C:F#F80C:UD4#Top:V#YTD
			-A#1641000C:F#F80C:UD4#Top:V#YTD
			-A#1561004C:F#F80C:UD4#Top:V#YTD
			-A#1661000C:F#F80C:UD4#Top:V#YTD
			-A#1661004C:F#F80C:UD4#Top:V#YTD
			-A#1521004C:F#F80M:UD4#Top:V#YTD
			-A#1521010C:F#F80M:UD4#Top:V#YTD
			-A#1521020C:F#F80M:UD4#Top:V#YTD
			-A#1501000C:F#F80M:UD4#Top:V#YTD
			-A#1541000C:F#F80M:UD4#Top:V#YTD
			-A#1541004C:F#F80M:UD4#Top:V#YTD
			-A#1561000C:F#F80M:UD4#Top:V#YTD
			-A#1502000C:F#F80M:UD4#Top:V#YTD
			-A#1502004C:F#F80M:UD4#Top:V#YTD
			-A#1661000C:F#F80M:UD4#Top:V#YTD
			-A#1661004C:F#F80M:UD4#Top:V#YTD
			-A#1601000C:F#F80M:UD4#Top:V#YTD
			-A#1621000C:F#F80M:UD4#Top:V#YTD
			-A#1641000C:F#F80M:UD4#Top:V#YTD
			-A#1561004C:F#F80M:UD4#Top:V#YTD
			+A#2941100L:F#F80C:UD4#Top:V#YTD
			+A#2941200L:F#F80C:UD4#Top:V#YTD
			+A#2501000C:F#F80C:UD4#Top:V#YTD
			+A#2521000C:F#F80C:UD4#Top:V#YTD
			+A#2541020C:F#F80C:UD4#Top:V#YTD
			+A#2561020C:F#F80C:UD4#Top:V#YTD
			+A#2701000C:F#F80C:UD4#Top:V#YTD
			+A#2801000C:F#F80C:UD4#Top:V#YTD
			+A#2761000C:F#F80C:UD4#Top:V#YTD
			+A#2541010C:F#F80C:UD4#Top:V#YTD
			+A#2561010C:F#F80C:UD4#Top:V#YTD
			+A#2581010C:F#F80C:UD4#Top:V#YTD
			+A#2581020C:F#F80C:UD4#Top:V#YTD
			+A#2901000C:F#F80C:UD4#Top:V#YTD
			+A#2621010C:F#F80C:UD4#Top:V#YTD
			+A#2621020C:F#F80C:UD4#Top:V#YTD
			+A#2941100L:F#F80M:UD4#Top:V#YTD
			+A#2941200L:F#F80M:UD4#Top:V#YTD
			+A#2501000C:F#F80M:UD4#Top:V#YTD
			+A#2521000C:F#F80M:UD4#Top:V#YTD
			+A#2541020C:F#F80M:UD4#Top:V#YTD
			+A#2561020C:F#F80M:UD4#Top:V#YTD
			+A#2721000C:F#F80M:UD4#Top:V#YTD
			+A#2701000C:F#F80M:UD4#Top:V#YTD
			+A#2801000C:F#F80M:UD4#Top:V#YTD
			+A#2761000C:F#F80M:UD4#Top:V#YTD
			+A#2561010C:F#F80M:UD4#Top:V#YTD
			+A#2581010C:F#F80M:UD4#Top:V#YTD
			+A#2581020C:F#F80M:UD4#Top:V#YTD
			+A#2901000C:F#F80M:UD4#Top:V#YTD
			+A#2621010C:F#F80M:UD4#Top:V#YTD
			+A#2621020C:F#F80M:UD4#Top:V#YTD
			+A#2601020C:F#F80M:UD4#Top:V#YTD
			+A#2601020C:F#F80C:UD4#Top:V#YTD
			+A#2601110C:F#F80M:UD4#Top:V#YTD
			+A#2601010C:F#F80C:UD4#Top:V#YTD
			+A#2601010C:F#F80M:UD4#Top:V#YTD
			+A#1641104C:F#F80C:UD4#Top:V#YTD
			+A#1641104C:F#F80M:UD4#Top:V#YTD
			-A#1641100C:F#F80C:UD4#Top:V#YTD
			-A#1641100C:F#F80M:UD4#Top:V#YTD
			-A#1671000C:F#F80C:UD4#Top:V#YTD
			-A#1671000C:F#F80M:UD4#Top:V#YTD
			-A#1671004C:F#F80C:UD4#Top:V#YTD
			-A#1671004C:F#F80M:UD4#Top:V#YTD
			-A#1001000C:F#F80C:UD4#Top:V#YTD
			-A#1001000L:F#F80C:UD4#Top:V#YTD
			-A#1001004C:F#F80C:UD4#Top:V#YTD
			-A#1041000L:F#F80C:UD4#Top:V#YTD
			-A#1081000C:F#F80C:UD4#Top:V#YTD
			-A#1101004L:F#F80C:UD4#Top:V#YTD
			-A#1101000C:F#F80C:UD4#Top:V#YTD
			-A#1101003L:F#F80C:UD4#Top:V#YTD
			-A#1101004C:F#F80C:UD4#Top:V#YTD
			-A#1111000C:F#F80C:UD4#Top:V#YTD
			-A#1111004C:F#F80C:UD4#Top:V#YTD
			-A#1101001L:F#F80C:UD4#Top:V#YTD
			-A#1101002L:F#F80C:UD4#Top:V#YTD
			-A#1221000C:F#F80C:UD4#Top:V#YTD
			-A#1221004C:F#F80C:UD4#Top:V#YTD
			-A#1141000C:F#F80C:UD4#Top:V#YTD
			-A#1141004C:F#F80C:UD4#Top:V#YTD
			+A#3001030L:F#F80C:UD4#Top:V#YTD
			-A#1021004C:F#F80C:UD4#Top:V#YTD
			-A#1021020C:F#F80C:UD4#Top:V#YTD
			-A#1021010C:F#F80C:UD4#Top:V#YTD
			-A#1041000C:F#F80C:UD4#Top:V#YTD
			-A#1041004C:F#F80C:UD4#Top:V#YTD
			-A#1061004C:F#F80C:UD4#Top:V#YTD
			-A#1241000C:F#F80C:UD4#Top:V#YTD
			-A#1121000C:F#F80C:UD4#Top:V#YTD
			-A#1241004C:F#F80C:UD4#Top:V#YTD
			-A#1121004C:F#F80C:UD4#Top:V#YTD
			-A#1181000C:F#F80C:UD4#Top:V#YTD
			-A#1201000C:F#F80C:UD4#Top:V#YTD
			-A#1161000C:F#F80C:UD4#Top:V#YTD
			-A#1161004C:F#F80C:UD4#Top:V#YTD
			-A#1001000C:F#F80M:UD4#Top:V#YTD
			-A#1001000L:F#F80M:UD4#Top:V#YTD
			-A#1001004C:F#F80M:UD4#Top:V#YTD
			-A#1041000L:F#F80M:UD4#Top:V#YTD
			-A#1081000C:F#F80M:UD4#Top:V#YTD
			-A#1101004L:F#F80M:UD4#Top:V#YTD
			-A#1101000C:F#F80M:UD4#Top:V#YTD
			-A#1101003L:F#F80M:UD4#Top:V#YTD
			-A#1101004C:F#F80M:UD4#Top:V#YTD
			-A#1111000C:F#F80M:UD4#Top:V#YTD
			-A#1111004C:F#F80M:UD4#Top:V#YTD
			-A#1101002L:F#F80M:UD4#Top:V#YTD
			-A#1221000C:F#F80M:UD4#Top:V#YTD
			-A#1221004C:F#F80M:UD4#Top:V#YTD
			-A#1141000C:F#F80M:UD4#Top:V#YTD
			-A#1141004C:F#F80M:UD4#Top:V#YTD
			+A#3001030L:F#F80M:UD4#Top:V#YTD
			-A#1021004C:F#F80M:UD4#Top:V#YTD
			-A#1021020C:F#F80M:UD4#Top:V#YTD
			-A#1021010C:F#F80M:UD4#Top:V#YTD
			-A#1041000C:F#F80M:UD4#Top:V#YTD
			-A#1041004C:F#F80M:UD4#Top:V#YTD
			-A#1061000C:F#F80M:UD4#Top:V#YTD
			-A#1061004C:F#F80M:UD4#Top:V#YTD
			-A#1241000C:F#F80M:UD4#Top:V#YTD
			-A#1121000C:F#F80M:UD4#Top:V#YTD
			-A#1241004C:F#F80M:UD4#Top:V#YTD
			-A#1121004C:F#F80M:UD4#Top:V#YTD
			-A#1181000C:F#F80M:UD4#Top:V#YTD
			-A#1201000C:F#F80M:UD4#Top:V#YTD
			-A#1161000C:F#F80M:UD4#Top:V#YTD
			-A#1161004C:F#F80M:UD4#Top:V#YTD
			+A#3001070:F#F80C:UD4#Top:V#YTD
			+A#3001060C:F#F80C:UD4#Top:V#YTD
			+A#3001080C:F#F80C:UD4#Top:V#YTD
			+A#3001070E:F#F80C:UD4#Top:V#YTD
			+A#3021030C:F#F80C:UD4#Top:V#YTD
			+A#3021070:F#F80C:UD4#Top:V#YTD
			+A#3021080C:F#F80C:UD4#Top:V#YTD
			+A#2081000C:F#F80C:UD4#Top:V#YTD
			+A#2101000C:F#F80C:UD4#Top:V#YTD
			+A#2041000C:F#F80C:UD4#Top:V#YTD
			+A#2061000C:F#F80C:UD4#Top:V#YTD
			+A#2121010C:F#F80C:UD4#Top:V#YTD
			+A#2121020C:F#F80C:UD4#Top:V#YTD
			+A#2141010C:F#F80C:UD4#Top:V#YTD
			+A#2141020C:F#F80C:UD4#Top:V#YTD
			+A#2761000C:F#F15:UD4#Top:V#YTD
			+A#2901000C:F#F15:UD4#Top:V#YTD
			+A#2801000C:F#F15:UD4#Top:V#YTD
			+A#2301000C:F#F15:UD4#Top:V#YTD
			+A#2201000C:F#F15:UD4#Top:V#YTD
			-A#1501009L:F#F80M:UD4#Top:V#YTD
			-A#1521050C:F#F80M:UD4#Top:V#YTD
			-A#1521050C:F#F80C:UD4#Top:V#YTD
			-A#1631400C:F#F80C:UD4#Top:V#YTD
			-A#1641004C:F#F80C:UD4#Top:V#YTD
			-A#1641004C:F#F80M:UD4#Top:V#YTD
			+A#2491200L:F#F80C:UD4#Top:V#YTD
			+A#2491200L:F#F80M:UD4#Top:V#YTD
			+A#2631400C:F#F80C:UD4#Top:V#YTD
			+A#2631400C:F#F80M:UD4#Top:V#YTD
			+A#3001010C:F#F80C:UD4#Top:V#YTD
			+A#3001010C:F#F80M:UD4#Top:V#YTD
			+A#3001020C:F#F80C:UD4#Top:V#YTD
			+A#3001020C:F#F80M:UD4#Top:V#YTD
			+A#3001030C:F#F80C:UD4#Top:V#YTD
			+A#3001030E:F#F80C:UD4#Top:V#YTD
			+A#3001030E:F#F80M:UD4#Top:V#YTD
			+A#3001035C:F#F80C:UD4#Top:V#YTD
			+A#3001035C:F#F80M:UD4#Top:V#YTD
			-A#1631300C:F#F80C:UD4#Top:V#YTD
			-A#1631300C:F#F80M:UD4#Top:V#YTD
			-A#1211300C:F#F80C:UD4#Top:V#YTD
			-A#1211300C:F#F80M:UD4#Top:V#YTD
			+A#2631300C:F#F80C:UD4#Top:V#YTD
			+A#2631300C:F#F80M:UD4#Top:V#YTD
			+A#2151300C:F#F80C:UD4#Top:V#YTD
			+A#2151300C:F#F80M:UD4#Top:V#YTD
			+A#2491100L:F#F15:UD4#Top:V#YTD
			+A#2941100L:F#F15:UD4#Top:V#YTD
			-A#1501009L:F#F80C:UD4#Top:V#YTD
			+A#2901820L:F#F15:UD4#Top:V#YTD
			+A#2901820L:F#F80C:UD4#Top:V#YTD
			+A#2901820L:F#F80M:UD4#Top:V#YTD
			-A#1701000C:F#F80M:UD4#Top:V#YTD
			-A#1701000C:F#F80C:UD4#Top:V#YTD
			-A#1701004C:F#F80C:UD4#Top:V#YTD
			-A#1701004C:F#F80M:UD4#Top:V#YTD
			+A#2951000C:F#F80C:UD4#Top:V#YTD
			+A#2951000C:F#F80M:UD4#Top:V#YTD
			+A#3001036C:F#F80C:UD4#Top:V#YTD
			+A#3001036C:F#F80M:UD4#Top:V#YTD
			-A#1631400C:F#F80M:UD4#Top:V#YTD
			-A#1221020C:F#F80C:UD4#Top:V#YTD
			-A#1641020C:F#F80C:UD4#Top:V#YTD
			-A#1681020C:F#F80C:UD4#Top:V#YTD
			+A#2101020C:F#F80C:UD4#Top:V#YTD
			-A#1221020C:F#F80M:UD4#Top:V#YTD
			-A#1641020C:F#F80M:UD4#Top:V#YTD
			-A#1681020C:F#F80M:UD4#Top:V#YTD
			+A#2101020C:F#F80M:UD4#Top:V#YTD
			-A#2201000CF:F#F15:UD4#Top:V#YTD
			-A#2801800:F#F15:UD4#Details:V#YTD
			-A#2761000CF:F#F15:UD4#Top:V#YTD
			-A#2901720:F#F15:UD4#Details:V#YTD
			-A#2301720:F#F15:UD4#Details:V#YTD
			+A#2401000C:F#F80C:UD4#Top:V#YTD
			+A#2401000C:F#F80M:UD4#Top:V#YTD
			-A#1171000C:F#F80M:UD4#Top:V#YTD
			-A#1171004C:F#F80M:UD4#Top:V#YTD
			-A#1171000C:F#F80C:UD4#Top:V#YTD
			-A#1171004C:F#F80C:UD4#Top:V#YTD
			+A#3001046:F#F80C:UD4#Top:V#YTD
			+A#3001046:F#F80M:UD4#Top:V#YTD
			-A#1101100C:F#F80C:UD4#Top:V#YTD
			-A#1101100C:F#F80M:UD4#Top:V#YTD
			-A#1101110C:F#F80C:UD4#Top:V#YTD
			-A#1101110C:F#F80M:UD4#Top:V#YTD
			-A#1101114C:F#F80C:UD4#Top:V#YTD
			-A#1101114C:F#F80M:UD4#Top:V#YTD
			-A#1131000C:F#F80C:UD4#Top:V#YTD
			-A#1131000C:F#F80M:UD4#Top:V#YTD
			-A#1131004C:F#F80C:UD4#Top:V#YTD
			-A#1131004C:F#F80M:UD4#Top:V#YTD
			-A#1132000C:F#F80C:UD4#Top:V#YTD
			-A#1132000C:F#F80M:UD4#Top:V#YTD
			-A#1132004C:F#F80C:UD4#Top:V#YTD
			-A#1132004C:F#F80M:UD4#Top:V#YTD
			-A#1221100C:F#F80C:UD4#Top:V#YTD
			-A#1221100C:F#F80M:UD4#Top:V#YTD
			-A#1221104C:F#F80C:UD4#Top:V#YTD
			-A#1221104C:F#F80M:UD4#Top:V#YTD
			-A#1221030C:F#F80C:UD4#Top:V#YTD
			-A#1221030C:F#F80M:UD4#Top:V#YTD
			-A#1241100C:F#F80C:UD4#Top:V#YTD
			-A#1241100C:F#F80M:UD4#Top:V#YTD
			-A#1642100C:F#F80C:UD4#Top:V#YTD
			-A#1642100C:F#F80M:UD4#Top:V#YTD
			-A#1642030C:F#F80C:UD4#Top:V#YTD
			-A#1642030C:F#F80M:UD4#Top:V#YTD
			-A#1641204C:F#F80C:UD4#Top:V#YTD
			-A#1641204C:F#F80M:UD4#Top:V#YTD
			-A#1661100C:F#F80C:UD4#Top:V#YTD
			-A#1661100C:F#F80M:UD4#Top:V#YTD
			-A#1681030C:F#F80C:UD4#Top:V#YTD
			-A#1681030C:F#F80M:UD4#Top:V#YTD
			+A#2149921C:F#F80C:UD4#Top:V#YTD
			+A#2149922C:F#F80C:UD4#Top:V#YTD
			+A#2569921C:F#F80C:UD4#Top:V#YTD
			+A#2149921C:F#F80M:UD4#Top:V#YTD
			+A#2149922C:F#F80M:UD4#Top:V#YTD
			+A#2589921C:F#F80M:UD4#Top:V#YTD
			+A#2569921C:F#F80M:UD4#Top:V#YTD
			+A#2149900C:F#F80C:UD4#Top:V#YTD
			+A#2149920C:F#F80C:UD4#Top:V#YTD
			+A#2569900C:F#F80C:UD4#Top:V#YTD
			+A#2589900C:F#F80C:UD4#Top:V#YTD
			+A#2569920C:F#F80C:UD4#Top:V#YTD
			+A#2589920C:F#F80C:UD4#Top:V#YTD
			+A#2569930C:F#F80C:UD4#Top:V#YTD
			+A#2569950C:F#F80C:UD4#Top:V#YTD
			+A#2149900C:F#F80M:UD4#Top:V#YTD
			+A#2149920C:F#F80M:UD4#Top:V#YTD
			+A#2569900C:F#F80M:UD4#Top:V#YTD
			+A#2589900C:F#F80M:UD4#Top:V#YTD
			+A#2569920C:F#F80M:UD4#Top:V#YTD
			+A#2589920C:F#F80M:UD4#Top:V#YTD
			+A#2569930C:F#F80M:UD4#Top:V#YTD
			+A#2569950C:F#F80M:UD4#Top:V#YTD
			+A#2149940C:F#F80C:UD4#Top:V#YTD
			+A#2589930C:F#F80C:UD4#Top:V#YTD
			+A#2589940C:F#F80C:UD4#Top:V#YTD
			+A#2149930C:F#F80M:UD4#Top:V#YTD
			+A#2149940C:F#F80M:UD4#Top:V#YTD
			+A#2589930C:F#F80M:UD4#Top:V#YTD
			+A#2589940C:F#F80M:UD4#Top:V#YTD
			+A#2589921C:F#F80C:UD4#Top:V#YTD
			+A#2149930C:F#F80C:UD4#Top:V#YTD
			+A#2149910C:F#F80M:UD4#Top:V#YTD
			+A#2569910C:F#F80M:UD4#Top:V#YTD
			+A#2149910C:F#F80C:UD4#Top:V#YTD
			+A#2569910C:F#F80C:UD4#Top:V#YTD
			+A#2201000C:F#F25:UD4#Top:V#YTD
			+A#2201000C:F#F30:UD4#Top:V#YTD
			+A#2801000C:F#F25:UD4#Top:V#YTD
			+A#2801000C:F#F30:UD4#Top:V#YTD
			+A#2601110C:F#F80C:UD4#Top:V#YTD
			-A#1061000C:F#F80C:UD4#Top:V#YTD
			-A#1101001L:F#F80M:UD4#Top:V#YTD
			-A#1501004C:F#F80M:UD4#Top:V#YTD
			+A#2721000C:F#F80C:UD4#Top:V#YTD
			+A#2541010C:F#F80M:UD4#Top:V#YTD
			+A#3001030C:F#F80M:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX06:F#None:UD4#CF_Autom:V#YTD = -A#1631400C:F#F15:UD4#Top:V#YTD
			+A#2631400C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX07:F#None:UD4#CF_Autom:V#YTD = -A#1221020C:F#F15:UD4#Top:V#YTD
			-A#1641020C:F#F15:UD4#Top:V#YTD
			+A#2801800:F#F15:UD4#Details:V#YTD
			-A#1241100C:F#F15:UD4#Top:V#YTD
			-A#1661100C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX08:F#None:UD4#CF_Autom:V#YTD = -A#1631330:F#F15:UD4#Details:V#YTD
			+A#2569921C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX09:F#None:UD4#CF_Autom:V#YTD = -A#1561040:F#F15:UD4#Details:V#YTD
			-A#1561044:F#F15:UD4#Details:V#YTD
			+A#2901720:F#F15:UD4#Details:V#YTD
			+A#2301720:F#F15:UD4#Details:V#YTD",True)
			api.Data.Calculate("A#TFEX10:F#None:UD4#CF_Autom:V#YTD = -A#1521010C:F#F20:UD4#Top:V#YTD
			-A#1521050C:F#F20:UD4#Top:V#YTD
			-A#1521020C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX11:F#None:UD4#CF_Autom:V#YTD = -A#1521010C:F#F30:UD4#Top:V#YTD
			-A#1521050C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX12:F#None:UD4#CF_Autom:V#YTD = -A#1521020C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX14:F#None:UD4#CF_Autom:V#YTD = A#2131020C:F#F15:UD4#Top:V#YTD
			+A#2531020C:F#F15:UD4#Top:V#YTD
			+A#2681020C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN01:F#None:UD4#CF_Autom:V#YTD = A#2541010C:F#F20:UD4#Top:V#YTD
			+A#2121010C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN02:F#None:UD4#CF_Autom:V#YTD = A#2541010C:F#F30:UD4#Top:V#YTD
			+A#2121010C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN03:F#None:UD4#CF_Autom:V#YTD = A#2581010C:F#F15:UD4#Top:V#YTD
			+A#2081000C:F#F15:UD4#Top:V#YTD
			+A#2561010C:F#F15:UD4#Top:V#YTD
			+A#2141010C:F#F15:UD4#Top:V#YTD
			+A#2941200L:F#F15:UD4#Top:V#YTD
			+A#2941200L:F#F20:UD4#Top:V#YTD
			+A#2941200L:F#F30:UD4#Top:V#YTD
			+A#2491200L:F#F15:UD4#Top:V#YTD
			+A#2491200L:F#F20:UD4#Top:V#YTD
			+A#2491200L:F#F30:UD4#Top:V#YTD
			+A#2149900C:F#F15:UD4#Top:V#YTD
			+A#2149920C:F#F15:UD4#Top:V#YTD
			+A#2569900C:F#F15:UD4#Top:V#YTD
			+A#2589900C:F#F15:UD4#Top:V#YTD
			+A#2569920C:F#F15:UD4#Top:V#YTD
			+A#2589920C:F#F15:UD4#Top:V#YTD
			+A#2569930C:F#F15:UD4#Top:V#YTD
			+A#2569950C:F#F15:UD4#Top:V#YTD
			+A#2149910C:F#F15:UD4#Top:V#YTD
			+A#2569910C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN04:F#None:UD4#CF_Autom:V#YTD = A#3001010C:F#F40:UD4#Top:V#YTD
			+A#3001020C:F#F40:UD4#Top:V#YTD
			+A#1101001L:F#F40:UD4#Top:V#YTD
			+A#1101002L:F#F40:UD4#Top:V#YTD
			+A#3001035C:F#F40:UD4#Top:V#YTD
			+A#3001030E:F#F40:UD4#Top:V#YTD
			+A#3001030C:F#F40:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN05:F#None:UD4#CF_Autom:V#YTD = A#3021030C:F#F40:UD4#Top:V#YTD
			+A#2149930C:F#F15:UD4#Top:V#YTD
			+A#2589930C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN06:F#None:UD4#CF_Autom:V#YTD = A#3001020C:F#F06:UD4#Top:V#YTD
			+A#3001030C:F#F06:UD4#Top:V#YTD
			+A#3001060C:F#F06:UD4#Top:V#YTD
			+A#3001030E:F#F06:UD4#Top:V#YTD
			+A#3001035C:F#F06:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN07:F#None:UD4#CF_Autom:V#YTD = A#3021030C:F#F06:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN10:F#None:UD4#CF_Autom:V#YTD = -A#1561110:F#F15:UD4#Details:V#YTD",True)
			api.Data.Calculate("A#TFIN11:F#None:UD4#CF_Autom:V#YTD = -A#1031000C:F#F30:UD4#Top:V#YTD
			-A#1031004C:F#F30:UD4#Top:V#YTD
			-A#1031000C:F#F20:UD4#Top:V#YTD
			+A#2131010C:F#F15:UD4#Top:V#YTD
			+A#2531010C:F#F15:UD4#Top:V#YTD
			+A#4899:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINFI:F#None:UD4#CF_Autom:V#YTD = -A#1211300C:F#F15:UD4#Top:V#YTD
			+A#2631300C:F#F15:UD4#Top:V#YTD
			+A#2151300C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV01:F#None:UD4#CF_Autom:V#YTD = -A#1101004L:F#F20:UD4#Top:V#YTD
			-A#1101001L:F#F20:UD4#Top:V#YTD
			-A#1101003L:F#F20:UD4#Top:V#YTD
			-A#1101003L:F#F40:UD4#Top:V#YTD
			-A#1101000C:F#F20:UD4#Top:V#YTD
			-A#1101000C:F#F40:UD4#Top:V#YTD
			-A#1111000C:F#F20:UD4#Top:V#YTD
			-A#1101002L:F#F20:UD4#Top:V#YTD
			-A#1101002L:F#F40:UD4#Top:V#YTD
			-A#1101001L:F#F40:UD4#Top:V#YTD
			-A#1081000C:F#F20:UD4#Top:V#YTD
			+A#1681000C:F#F01:UD4#Top:V#YTD
			-A#1081000C:F#F40:UD4#Top:V#YTD
			+A#2149940C:F#F15:UD4#Top:V#YTD
			+A#2589940C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV023:F#None:UD4#CF_Autom:V#YTD = -A#1041000C:F#F20:UD4#Top:V#YTD
			+A#2701000C:F#F15:UD4#Top:V#YTD
			-A#1041000L:F#F20:UD4#Top:V#YTD
			-A#1561100:F#F15:UD4#Details:V#YTD
			+A#2401000C:F#F15:UD4#Top:V#YTD
			+A#2061000CF:F#F25:UD4#Top:V#YTD
			+A#2521000CF:F#F25:UD4#Top:V#YTD
			+A#4943:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV03:F#None:UD4#CF_Autom:V#YTD = -A#1111000C:F#F30:UD4#Top:V#YTD
			-A#1571000C:F#F15:UD4#Top:V#YTD
			+A#1681000C:F#F98:UD4#Top:V#YTD
			-A#1132000C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV043:F#None:UD4#CF_Autom:V#YTD = -A#1581000C:F#F15:UD4#Top:V#YTD
			+A#2761000CF:F#F15:UD4#Top:V#YTD
			-A#1171000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV044:F#None:UD4#CF_Autom:V#YTD = -A#1061000C:F#F30:UD4#Top:V#YTD
			-A#1061004C:F#F30:UD4#Top:V#YTD
			-A#1071000C:F#F30:UD4#Top:V#YTD
			-A#1071004C:F#F30:UD4#Top:V#YTD
			-A#1071005C:F#F30:UD4#Top:V#YTD
			-A#1091000C:F#F30:UD4#Top:V#YTD
			-A#1091005C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV05:F#None:UD4#CF_Autom:V#YTD = -A#1121000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV06:F#None:UD4#CF_Autom:V#YTD = -A#1241000C:F#F15:UD4#Top:V#YTD
			-A#1661000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV07:F#None:UD4#CF_Autom:V#YTD = -A#1221000C:F#F15:UD4#Top:V#YTD
			-A#1641000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV08:F#None:UD4#CF_Autom:V#YTD = -A#1131000C:F#F20:UD4#Top:V#YTD
			-A#1131000C:F#F40:UD4#Top:V#YTD
			-A#1132000C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV09:F#None:UD4#CF_Autom:V#YTD = -A#1101100C:F#F20:UD4#Top:V#YTD
			-A#1101100C:F#F40:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV10:F#None:UD4#CF_Autom:V#YTD = -A#1101110C:F#F20:UD4#Top:V#YTD
			-A#1101110C:F#F40:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV14:F#None:UD4#CF_Autom:V#YTD = -A#1101100C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV16:F#None:UD4#CF_Autom:V#YTD = -A#1221100C:F#F15:UD4#Top:V#YTD
			-A#1221030C:F#F15:UD4#Top:V#YTD
			-A#1642100C:F#F15:UD4#Top:V#YTD
			-A#1642030C:F#F15:UD4#Top:V#YTD",True)
'			api.Data.Calculate("A#TTRE01:F#None:UD4#CF_Autom:V#YTD = 
'			A#1681000C:F#F99:UD4#Top:V#YTD:T#PovPrior1
'			+A#1681020C:F#F99:UD4#Top:V#YTD:T#PovPrior1
'			+A#1681030C:F#F99:UD4#Top:V#YTD:T#PovPrior1",True)	
			api.Data.Calculate("A#TTRE01:F#None:UD4#CF_Autom:V#YTD = 
			A#1681000C:F#F00:UD4#Top:V#YTD
			+A#1681020C:F#F00:UD4#Top:V#YTD
			+A#1681030C:F#F00:UD4#Top:V#YTD",True)	
			api.Data.Calculate("A#TTRE02:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE03:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F80C:UD4#Top:V#YTD
			+A#1681000C:F#F80M:UD4#Top:V#YTD
			+A#1681020C:F#F80C:UD4#Top:V#YTD
			+A#1681020C:F#F80M:UD4#Top:V#YTD
			+A#1681030C:F#F80C:UD4#Top:V#YTD
			+A#1681030C:F#F80M:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE04:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F01:UD4#Top:V#YTD
			+A#1681000C:F#F98:UD4#Top:V#YTD
			+A#1681000C:F#F70:UD4#Top:V#YTD
			+A#1681000C:F#F91:UD4#Top:V#YTD
			+A#1681020C:F#F01:UD4#Top:V#YTD
			+A#1681020C:F#F98:UD4#Top:V#YTD
			+A#1681020C:F#F70:UD4#Top:V#YTD
			+A#1681030C:F#F01:UD4#Top:V#YTD
			+A#1681030C:F#F70:UD4#Top:V#YTD
			+A#1681030C:F#F91:UD4#Top:V#YTD
			+A#1681030C:F#F98:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE05:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F55:UD4#Top:V#YTD
			+A#1681020C:F#F55:UD4#Top:V#YTD
			+A#1681030C:F#F15:UD4#Top:V#YTD
			+A#1681030C:F#F55:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE06:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F99:UD4#Top:V#YTD
			+A#1681020C:F#F99:UD4#Top:V#YTD
			+A#1681030C:F#F99:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE07:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F50:UD4#Top:V#YTD
			+A#1681020C:F#F50:UD4#Top:V#YTD
			+A#1681030C:F#F50:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE08:F#None:UD4#CF_Autom:V#YTD = A#1681020C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV02FRD1:F#None:UD4#CF_Autom:V#YTD = -A#1021020C:F#F20:UD4#Top:V#YTD
			-A#1021010C:F#F20:UD4#Top:V#YTD",True)

		    '**************** FCF *********************************
			
			api.Data.Calculate("A#TINV02:F#None:UD4#CF_Autom:V#YTD = -A#1001000L:F#F20:UD4#Top:V#YTD
			-A#1021010L:F#F20:UD4#Top:V#YTD
			-A#1001000C:F#F20:UD4#Top:V#YTD
			-A#1021020C:F#F20:UD4#Top:V#YTD
			-A#1021010C:F#F20:UD4#Top:V#YTD
			-A#1041000C:F#F20:UD4#Top:V#YTD
			+A#2701000C:F#F15:UD4#Top:V#YTD
			-A#1061000C:F#F20:UD4#Top:V#YTD
			-A#1041000L:F#F20:UD4#Top:V#YTD
			-A#1071000C:F#F20:UD4#Top:V#YTD
			-A#1091000C:F#F20:UD4#Top:V#YTD
			-A#1561100:F#F15:UD4#Details:V#YTD
			+A#2401000C:F#F15:UD4#Top:V#YTD
			+A#2061000CF:F#F25:UD4#Top:V#YTD
			+A#2521000CF:F#F25:UD4#Top:V#YTD
			+A#4943:F#None:UD4#Top:V#YTD
			+A#49123:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV04:F#None:UD4#CF_Autom:V#YTD = A#1021010L:F#F20:UD4#Top:V#YTD
			+A#1001000L:F#F20:UD4#Top:V#YTD
			-A#1581000C:F#F15:UD4#Top:V#YTD
			-A#1061000C:F#F30:UD4#Top:V#YTD
			-A#1061004C:F#F30:UD4#Top:V#YTD
			+A#1041000L:F#F20:UD4#Top:V#YTD
			-A#1071000C:F#F30:UD4#Top:V#YTD
			-A#1071004C:F#F30:UD4#Top:V#YTD
			-A#1071005C:F#F30:UD4#Top:V#YTD
			-A#1091000C:F#F30:UD4#Top:V#YTD
			-A#1091005C:F#F30:UD4#Top:V#YTD
			+A#2761000CF:F#F15:UD4#Top:V#YTD
			-A#1171000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV042:F#None:UD4#CF_Autom:V#YTD = A#1171000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAFAPCRES:F#None:UD4#CF_Autom:V#YTD = A#485:F#None:UD4#Top:V#YTD",True)
			
			'************ CF Conso ****************************************************************************
			api.Data.Calculate("A#TCAF13:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = -A#49111:F#None:UD4#Top:UD7#None
			-A#49121:F#None:UD4#Top:UD7#None
			-A#49123:F#None:UD4#Top:UD7#None
			-A#49124:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TCAF14:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = -A#4111:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX07:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49111:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX08:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49121:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX09:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#4111:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX14:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49124:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TINV023:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49123:F#None:UD4#Top:UD7#None",True)

					Else	
	

			'El resto de Escenarios, sus cuentas de detalle estan en UD4 None
				'Calculo
			api.Data.Calculate("A#TCAF01:F#None:UD4#CF_Autom:V#YTD = A#3001090C:F#F10:UD4#Top:V#YTD
			+A#3001080C:F#F10:UD4#Top:V#YTD
			+A#3021080C:F#F10:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF02:F#None:UD4#CF_Autom:V#YTD = -A#1001004C:F#F25:UD4#Top:V#YTD
			-A#1001004C:F#F35:UD4#Top:V#YTD
			-A#1021004C:F#F25:UD4#Top:V#YTD
			-A#1021004C:F#F35:UD4#Top:V#YTD
			-A#1041004C:F#F25:UD4#Top:V#YTD
			-A#1041004C:F#F35:UD4#Top:V#YTD
			-A#1061004C:F#F25:UD4#Top:V#YTD
			-A#1061004C:F#F35:UD4#Top:V#YTD
			-A#1071004C:F#F25:UD4#Top:V#YTD
			-A#1071004C:F#F35:UD4#Top:V#YTD
			-A#1071005C:F#F25:UD4#Top:V#YTD
			-A#1071005C:F#F35:UD4#Top:V#YTD
			-A#1091005C:F#F25:UD4#Top:V#YTD
			-A#1091005C:F#F35:UD4#Top:V#YTD
			-A#1031004C:F#F25:UD4#Top:V#YTD
			-A#1031005C:F#F25:UD4#Top:V#YTD
			-A#1031005C:F#F35:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF03:F#None:UD4#CF_Autom:V#YTD = -A#1521050C:F#F25:UD4#Top:V#YTD
			-A#1521010C:F#F25:UD4#Top:V#YTD
			-A#1521004C:F#F15:UD4#Top:V#YTD
			-A#1521020C:F#F25:UD4#Top:V#YTD
			-A#1521004C:F#F25:UD4#Top:V#YTD
			-A#1521004C:F#F37:UD4#Top:V#YTD
			-A#1521004C:F#F39:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF04:F#None:UD4#CF_Autom:V#YTD = -A#481:F#None:UD4#Top:V#YTD
			-A#482:F#None:UD4#Top:V#YTD
			-A#4892:F#None:UD4#Top:V#YTD
			-A#1171004C:F#F15:UD4#Top:V#YTD
			-A#1101114C:F#F25:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF05:F#None:UD4#CF_Autom:V#YTD = A#2061000C:F#F15:UD4#Top:V#YTD
			+A#2041000C:F#F15:UD4#Top:V#YTD
			+A#2501000C:F#F15:UD4#Top:V#YTD
			+A#2521000C:F#F15:UD4#Top:V#YTD
			-A#4934:F#None:UD4#Top:V#YTD
			-A#2061000CF:F#F25:UD4#Top:V#YTD
			-A#2521000CF:F#F25:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF06:F#None:UD4#CF_Autom:V#YTD = -A#1111004C:F#F25:UD4#Top:V#YTD
			-A#1111004C:F#F35:UD4#Top:V#YTD
			-A#1641204C:F#F15:UD4#Top:V#YTD
			-A#1221104C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF07:F#None:UD4#CF_Autom:V#YTD = -A#1081000C:F#F06:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF08:F#None:UD4#CF_Autom:V#YTD = -A#1081000C:F#F10:UD4#Top:V#YTD
			-A#1101114C:F#F25:UD4#Top:V#YTD
			-A#1101114C:F#F35:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF09:F#None:UD4#CF_Autom:V#YTD = -A#1141000C:F#F15:UD4#Top:V#YTD
			+A#2021000C:F#F15:UD4#Top:V#YTD
			-A#4113:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF11:F#None:UD4#CF_Autom:V#YTD = -A#4931:F#None:UD4#Top:V#YTD
			-A#4932:F#None:UD4#Top:V#YTD
			-A#4940:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF12:F#None:UD4#CF_Autom:V#YTD = -A#554106:F#None:UD4#Top:V#YTD
			-A#563113:F#None:UD4#Top:V#YTD
			-A#563113:F#None:UD4#Top:V#Periodic
			-A#554106:F#None:UD4#Top:V#Periodic",True)
			api.Data.Calculate("A#TCAF18:F#None:UD4#CF_Autom:V#YTD = -A#581108R:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF19:F#None:UD4#CF_Autom:V#YTD = -A#1101004C:F#F25:UD4#Top:V#YTD
			-A#1101004C:F#F35:UD4#Top:V#YTD
			-A#1131004C:F#F25:UD4#Top:V#YTD
			-A#1131004C:F#F35:UD4#Top:V#YTD
			-A#1132004C:F#F25:UD4#Top:V#YTD
			-A#1132004C:F#F35:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF20:F#None:UD4#CF_Autom:V#YTD = -A#4929:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAF22:F#None:UD4#CF_Autom:V#YTD = -A#4943:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI01:F#None:UD4#CF_Autom:V#YTD = A#2601010C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI02:F#None:UD4#CF_Autom:V#YTD = A#2601010C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI03:F#None:UD4#CF_Autom:V#YTD = A#2621010C:F#F15:UD4#Top:V#YTD
			+A#2601010C:F#F15:UD4#Top:V#YTD
			+A#2621020C:F#F15:UD4#Top:V#YTD
			+A#2601110C:F#F15:UD4#Top:V#YTD
			+A#2601020C:F#F15:UD4#Top:V#YTD
			-A#1631400C:F#F15:UD4#Top:V#YTD
			+A#2631400C:F#F15:UD4#Top:V#YTD
			+A#2681010C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI04:F#None:UD4#CF_Autom:V#YTD = -A#1671000C:F#F15:UD4#Top:V#YTD
			-A#1671004C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TDPI05:F#None:UD4#CF_Autom:V#YTD = -A#1641104C:F#F15:UD4#Top:V#YTD
			-A#1641100C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX01:F#None:UD4#CF_Autom:V#YTD = -A#1501000C:F#F15:UD4#Top:V#YTD
			-A#1501004C:F#F15:UD4#Top:V#YTD
			-A#1502000C:F#F15:UD4#Top:V#YTD
			-A#1502004C:F#F15:UD4#Top:V#YTD
			-A#1501009L:F#F15:UD4#Top:V#YTD",True)
			'api.Data.Calculate("A#TFEX02:F#None:UD4#CF_Autom:V#YTD:IC#None = -A#1541000C:F#F15:UD4#Top:V#YTD:IC#Top
			api.Data.Calculate("A#TFEX02:F#None:UD4#CF_Autom:V#YTD = -A#1541000C:F#F15:UD4#Top:V#YTD
			-A#1541004C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX03:F#None:UD4#CF_Autom:V#YTD = -A#1161000C:F#F15:UD4#Top:V#YTD
			-A#1161004C:F#F15:UD4#Top:V#YTD
			-A#1201000C:F#F15:UD4#Top:V#YTD
			-A#1181000C:F#F15:UD4#Top:V#YTD
			-A#1561000C:F#F15:UD4#Top:V#YTD
			-A#1621000C:F#F15:UD4#Top:V#YTD
			-A#1601000C:F#F15:UD4#Top:V#YTD
			+A#1561100:F#F15:UD4#Top:V#YTD
			+A#1561040:F#F15:UD4#Top:V#YTD
			+A#1561044:F#F15:UD4#Top:V#YTD
			+A#1561110:F#F15:UD4#Top:V#YTD
			-A#1561004C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX04:F#None:UD4#CF_Autom:V#YTD = A#2721000C:F#F15:UD4#Top:V#YTD",True)
			
'___________________________________________________________________________________________________________________________________________			
			
			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Account_Details")
			Dim memberList_ASSETS As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"ASSETS"))
			Dim memberList_02AC07 As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"02AC07"))
			Dim memberList_LIABILITIES As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"LIABILITIES"))

			
			Dim memberString As String = ""
		
				For Each Account As Member In memberList_ASSETS
						memberString = memberString + "- A#" + Account.Name.ToString + ":F#F80C:UD4#Top:V#YTD"			
				Next

				For Each Account As Member In memberList_02AC07
						memberString = memberString + "+ A#" + Account.Name.ToString + ":F#F80C:UD4#Top:V#YTD"
				Next				
				
				For Each Account As Member In memberList_LIABILITIES
						memberString = memberString + "+ A#" + Account.Name.ToString + ":F#F80C:UD4#Top:V#YTD"
				Next		
				
			
'___________________________________________________________________________________________________________________________________________					
			api.Data.Calculate("A#TFEX:F#None:UD4#CF_Autom:V#YTD = " & memberString,True)
			'brapi.ErrorLog.LogMessage(si, memberString)								
'___________________________________________________________________________________________________________________________________________			
	

			api.Data.Calculate("A#TFEX05:F#None:UD4#CF_Autom:V#YTD = A#2201000C:F#F15:UD4#Top:V#YTD
			+A#2301000C:F#F15:UD4#Top:V#YTD
			+A#2801000C:F#F15:UD4#Top:V#YTD
			+A#2901000C:F#F15:UD4#Top:V#YTD
			-A#2901720:F#F15:UD4#Top:V#YTD	
			-A#2901730:F#F15:UD4#Top:V#YTD	
			+A#2061430:F#F20:UD4#Top:V#YTD
			+A#2061430:F#F30:UD4#Top:V#YTD
			+A#2941100L:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX06:F#None:UD4#CF_Autom:V#YTD = -A#1631400C:F#F15:UD4#Top:V#YTD
			+A#2631400C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX08:F#None:UD4#CF_Autom:V#YTD = -A#1631330:F#F15:UD4#Top:V#YTD
			+A#2569921C:F#F15:UD4#Top:V#YTD
			+A#2581020C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX09:F#None:UD4#CF_Autom:V#YTD = -A#1561040:F#F15:UD4#Top:V#YTD
			-A#1561044:F#F15:UD4#Top:V#YTD
			+A#2901720:F#F15:UD4#Top:V#YTD
			+A#2901730:F#F15:UD4#Top:V#YTD
			+A#2301720:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX10:F#None:UD4#CF_Autom:V#YTD = -A#1521010C:F#F20:UD4#Top:V#YTD
			-A#1521050C:F#F20:UD4#Top:V#YTD
			-A#1521020C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX11:F#None:UD4#CF_Autom:V#YTD = -A#1521010C:F#F30:UD4#Top:V#YTD
			-A#1521050C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX12:F#None:UD4#CF_Autom:V#YTD = -A#1521020C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFEX14:F#None:UD4#CF_Autom:V#YTD = A#2131020C:F#F15:UD4#Top:V#YTD
			+A#2531020C:F#F15:UD4#Top:V#YTD
			+A#2681020C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN01:F#None:UD4#CF_Autom:V#YTD = A#2541010C:F#F20:UD4#Top:V#YTD
			+A#2121010C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN02:F#None:UD4#CF_Autom:V#YTD = A#2541010C:F#F30:UD4#Top:V#YTD
			+A#2121010C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN03:F#None:UD4#CF_Autom:V#YTD = A#2581010C:F#F15:UD4#Top:V#YTD
			+A#2081000C:F#F15:UD4#Top:V#YTD
			+A#2561010C:F#F15:UD4#Top:V#YTD
			+A#2141010C:F#F15:UD4#Top:V#YTD
			+A#2941200L:F#F15:UD4#Top:V#YTD
			+A#2941200L:F#F20:UD4#Top:V#YTD
			+A#2941200L:F#F30:UD4#Top:V#YTD
			+A#2491200L:F#F15:UD4#Top:V#YTD
			+A#2491200L:F#F20:UD4#Top:V#YTD
			+A#2491200L:F#F30:UD4#Top:V#YTD
			+A#2149900C:F#F15:UD4#Top:V#YTD
			+A#2149920C:F#F15:UD4#Top:V#YTD
			+A#2569900C:F#F15:UD4#Top:V#YTD
			+A#2589900C:F#F15:UD4#Top:V#YTD
			+A#2569920C:F#F15:UD4#Top:V#YTD
			+A#2589920C:F#F15:UD4#Top:V#YTD
			+A#2569930C:F#F15:UD4#Top:V#YTD
			+A#2569950C:F#F15:UD4#Top:V#YTD
			+A#2149910C:F#F15:UD4#Top:V#YTD
			+A#2569910C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN04:F#None:UD4#CF_Autom:V#YTD = A#3001010C:F#F40:UD4#Top:V#YTD
			+A#3001020C:F#F40:UD4#Top:V#YTD
			+A#1101001L:F#F40:UD4#Top:V#YTD
			+A#1101002L:F#F40:UD4#Top:V#YTD
			+A#3001035C:F#F40:UD4#Top:V#YTD
			+A#3001030E:F#F40:UD4#Top:V#YTD
			+A#3001030C:F#F40:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN05:F#None:UD4#CF_Autom:V#YTD = A#3021030C:F#F40:UD4#Top:V#YTD
			+A#2149930C:F#F15:UD4#Top:V#YTD
			+A#2589930C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN06:F#None:UD4#CF_Autom:V#YTD = A#3001020C:F#F06:UD4#Top:V#YTD
			+A#3001030C:F#F06:UD4#Top:V#YTD
			+A#3001060C:F#F06:UD4#Top:V#YTD
			+A#3001030E:F#F06:UD4#Top:V#YTD
			+A#3001035C:F#F06:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN07:F#None:UD4#CF_Autom:V#YTD = A#3021030C:F#F06:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN10:F#None:UD4#CF_Autom:V#YTD = -A#1561110:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TFIN11:F#None:UD4#CF_Autom:V#YTD = -A#1031000C:F#F30:UD4#Top:V#YTD
			-A#1031004C:F#F30:UD4#Top:V#YTD
			-A#1031000C:F#F20:UD4#Top:V#YTD
			+A#2131010C:F#F15:UD4#Top:V#YTD
			+A#2531010C:F#F15:UD4#Top:V#YTD
			+A#4899:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINFI:F#None:UD4#CF_Autom:V#YTD = -A#1211300C:F#F15:UD4#Top:V#YTD
			+A#2631300C:F#F15:UD4#Top:V#YTD
			+A#2151300C:F#F15:UD4#Top:V#YTD
			-A#1631300CF:F#F15:UD4#Top:V#YTD
			+A#581108R:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV01:F#None:UD4#CF_Autom:V#YTD = -A#1101004L:F#F20:UD4#Top:V#YTD
			-A#1101001L:F#F20:UD4#Top:V#YTD
			-A#1101003L:F#F20:UD4#Top:V#YTD
			-A#1101003L:F#F40:UD4#Top:V#YTD
			-A#1101000C:F#F20:UD4#Top:V#YTD
			-A#1101000C:F#F40:UD4#Top:V#YTD
			-A#1111000C:F#F20:UD4#Top:V#YTD
			-A#1101002L:F#F20:UD4#Top:V#YTD
			-A#1101002L:F#F40:UD4#Top:V#YTD
			-A#1101001L:F#F40:UD4#Top:V#YTD
			-A#1081000C:F#F20:UD4#Top:V#YTD
			+A#1681000C:F#F01:UD4#Top:V#YTD
			-A#1081000C:F#F40:UD4#Top:V#YTD
			+A#2149940C:F#F15:UD4#Top:V#YTD
			+A#2589940C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV023:F#None:UD4#CF_Autom:V#YTD = -A#1041000C:F#F20:UD4#Top:V#YTD
			+A#2701000C:F#F15:UD4#Top:V#YTD
			-A#1041000L:F#F20:UD4#Top:V#YTD
			-A#1561100:F#F15:UD4#Top:V#YTD
			+A#2401000C:F#F15:UD4#Top:V#YTD
			+A#2061000CF:F#F25:UD4#Top:V#YTD
			+A#2521000CF:F#F25:UD4#Top:V#YTD
			+A#4943:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV03:F#None:UD4#CF_Autom:V#YTD = -A#1111000C:F#F30:UD4#Top:V#YTD
			-A#1571000C:F#F15:UD4#Top:V#YTD
			+A#1681000C:F#F98:UD4#Top:V#YTD
			-A#1132000C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV043:F#None:UD4#CF_Autom:V#YTD = -A#1581000C:F#F15:UD4#Top:V#YTD
			+A#2761000CF:F#F15:UD4#Top:V#YTD
			-A#1171000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV044:F#None:UD4#CF_Autom:V#YTD = -A#1061000C:F#F30:UD4#Top:V#YTD
			-A#1061004C:F#F30:UD4#Top:V#YTD
			-A#1071000C:F#F30:UD4#Top:V#YTD
			-A#1071004C:F#F30:UD4#Top:V#YTD
			-A#1071005C:F#F30:UD4#Top:V#YTD
			-A#1091000C:F#F30:UD4#Top:V#YTD
			-A#1091005C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV05:F#None:UD4#CF_Autom:V#YTD = -A#1121000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV06:F#None:UD4#CF_Autom:V#YTD = -A#1241000C:F#F15:UD4#Top:V#YTD
			-A#1661000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV07:F#None:UD4#CF_Autom:V#YTD = -A#1221000C:F#F15:UD4#Top:V#YTD
			-A#1641000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV08:F#None:UD4#CF_Autom:V#YTD = -A#1131000C:F#F20:UD4#Top:V#YTD
			-A#1131000C:F#F40:UD4#Top:V#YTD
			-A#1132000C:F#F20:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV09:F#None:UD4#CF_Autom:V#YTD = -A#1101100C:F#F20:UD4#Top:V#YTD
			-A#1101100C:F#F40:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV10:F#None:UD4#CF_Autom:V#YTD = -A#1101110C:F#F20:UD4#Top:V#YTD
			-A#1101110C:F#F40:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV14:F#None:UD4#CF_Autom:V#YTD = -A#1101100C:F#F30:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV16:F#None:UD4#CF_Autom:V#YTD = -A#1221100C:F#F15:UD4#Top:V#YTD
			-A#1221030C:F#F15:UD4#Top:V#YTD
			-A#1642100C:F#F15:UD4#Top:V#YTD
			-A#1642030C:F#F15:UD4#Top:V#YTD",True)
'			api.Data.Calculate("A#TTRE01:F#None:UD4#CF_Autom:V#YTD = 
'			A#1681000C:F#F99:UD4#Top:V#YTD:T#PovPrior1
'			+A#1681020C:F#F99:UD4#Top:V#YTD:T#PovPrior1
'			+A#1681030C:F#F99:UD4#Top:V#YTD:T#PovPrior1",True)	
			api.Data.Calculate("A#TTRE01:F#None:UD4#CF_Autom:V#YTD = 
			A#1681000C:F#F00:UD4#Top:V#YTD
			+A#1681020C:F#F00:UD4#Top:V#YTD
			+A#1681030C:F#F00:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE02:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE03:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F80C:UD4#Top:V#YTD
			+A#1681000C:F#F80M:UD4#Top:V#YTD
			+A#1681020C:F#F80C:UD4#Top:V#YTD
			+A#1681020C:F#F80M:UD4#Top:V#YTD
			+A#1681030C:F#F80C:UD4#Top:V#YTD
			+A#1681030C:F#F80M:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE04:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F01:UD4#Top:V#YTD
			+A#1681000C:F#F98:UD4#Top:V#YTD
			+A#1681000C:F#F70:UD4#Top:V#YTD
			+A#1681000C:F#F91:UD4#Top:V#YTD
			+A#1681020C:F#F01:UD4#Top:V#YTD
			+A#1681020C:F#F98:UD4#Top:V#YTD
			+A#1681020C:F#F70:UD4#Top:V#YTD
			+A#1681030C:F#F01:UD4#Top:V#YTD
			+A#1681030C:F#F70:UD4#Top:V#YTD
			+A#1681030C:F#F91:UD4#Top:V#YTD
			+A#1681030C:F#F98:UD4#Top:V#YTD
			+A#3001060CF:F#F01:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE05:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F55:UD4#Top:V#YTD
			+A#1681020C:F#F55:UD4#Top:V#YTD
			+A#1681030C:F#F15:UD4#Top:V#YTD
			+A#1681030C:F#F55:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE06:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F99:UD4#Top:V#YTD
			+A#1681020C:F#F99:UD4#Top:V#YTD
			+A#1681030C:F#F99:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE07:F#None:UD4#CF_Autom:V#YTD = A#1681000C:F#F50:UD4#Top:V#YTD
			+A#1681020C:F#F50:UD4#Top:V#YTD
			+A#1681030C:F#F50:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TTRE08:F#None:UD4#CF_Autom:V#YTD = A#1681020C:F#F15:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV02FRD1:F#None:UD4#CF_Autom:V#YTD = -A#1021020C:F#F20:UD4#Top:V#YTD
			-A#1021010C:F#F20:UD4#Top:V#YTD",True)

		    '**************** FCF *********************************
			api.Data.Calculate("A#TINV02:F#None:UD4#CF_Autom:V#YTD = -A#1001000L:F#F20:UD4#Top:V#YTD
			-A#1021010L:F#F20:UD4#Top:V#YTD
			-A#1001000C:F#F20:UD4#Top:V#YTD
			-A#1021020C:F#F20:UD4#Top:V#YTD
			-A#1021010C:F#F20:UD4#Top:V#YTD
			-A#1041000C:F#F20:UD4#Top:V#YTD
			+A#2701000C:F#F15:UD4#Top:V#YTD
			-A#1061000C:F#F20:UD4#Top:V#YTD
			-A#1041000L:F#F20:UD4#Top:V#YTD
			-A#1071000C:F#F20:UD4#Top:V#YTD
			-A#1091000C:F#F20:UD4#Top:V#YTD
			-A#1561100:F#F15:UD4#Top:V#YTD
			+A#2401000C:F#F15:UD4#Top:V#YTD
			+A#2061000CF:F#F25:UD4#Top:V#YTD
			+A#2521000CF:F#F25:UD4#Top:V#YTD
			+A#4943:F#None:UD4#Top:V#YTD
			+A#49123:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV04:F#None:UD4#CF_Autom:V#YTD = A#1021010L:F#F20:UD4#Top:V#YTD
			+A#1001000L:F#F20:UD4#Top:V#YTD
			-A#1581000C:F#F15:UD4#Top:V#YTD
			-A#1061000C:F#F30:UD4#Top:V#YTD
			-A#1061004C:F#F30:UD4#Top:V#YTD
			+A#1041000L:F#F20:UD4#Top:V#YTD
			-A#1071000C:F#F30:UD4#Top:V#YTD
			-A#1071004C:F#F30:UD4#Top:V#YTD
			-A#1071005C:F#F30:UD4#Top:V#YTD
			-A#1091000C:F#F30:UD4#Top:V#YTD
			-A#1091005C:F#F30:UD4#Top:V#YTD
			+A#2761000CF:F#F15:UD4#Top:V#YTD
			-A#1171000C:F#F15:UD4#Top:V#YTD
			-A#1001000C:F#F30:UD4#Top:V#YTD
			+A#481:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TINV042:F#None:UD4#CF_Autom:V#YTD = -A#1001000C:F#F30:UD4#Top:V#YTD
			+A#481:F#None:UD4#Top:V#YTD",True)
			api.Data.Calculate("A#TCAFAPCRES:F#None:UD4#CF_Autom:V#YTD = A#485:F#None:UD4#Top:V#YTD",True)
			
			'************ CF Conso ****************************************************************************
			api.Data.Calculate("A#TCAF13:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = -A#49111:F#None:UD4#Top:UD7#None
			-A#49121:F#None:UD4#Top:UD7#None
			-A#49123:F#None:UD4#Top:UD7#None
			-A#49124:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TCAF14:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = -A#4111:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX07:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49111:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX08:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49121:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX09:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#4111:F#None:UD4#Top:UD7#None
			+A#4113:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TFEX14:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49124:F#None:UD4#Top:UD7#None",True)
			api.Data.Calculate("A#TINV023:F#None:UD4#CF_Autom:UD7#CF_PL_Acc = A#49123:F#None:UD4#Top:UD7#None",True)


			End If
		'End If
		End Sub
	
#End Region

#Region "Member Formulas for IS Accounts"

Sub AcctMinInt (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)
		
		api.Data.Calculate("A#" & Acct.MinInt.Name & ":F#None=A#" & Acct.ResM.Name & ":F#" & flow.Clo.Name)

	End Sub
	
	Function AcctMinIntDDP (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs) As DrillDownFormulaResult

		Dim Flow As Object = Me.InitFlowClass(si, api, globals)
		Dim Acct As Object = Me.InitAccountClass(si, api, globals)

		'Simple formula drill down
		Dim result As New DrillDownFormulaResult()

		'Define Explanation to be shown in drill title bar
		result.Explanation = "Int Min"

		'Add a row below per each drill result to be shown 
		result.SourceDataCells.Add("A#" & Acct.ResM.Name & ":F#" & flow.Clo.Name)
		
		Return result
	End Function

#End Region	

#Region "Member Formulas for UD4"

Sub FXOverride (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		If Not api.Entity.HasChildren() Then
			If  api.Cons.IsLocalCurrencyforEntity() Then
				Dim CuentasEQTU4 As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "EQUITY"))
				For Each AccountEQU4 As Member In CuentasEQTU4 
						'Dim flowlist = New List(Of String) From {"F00C","F01C","F05C","F06C","F10C","F40C","F50C","F57C","F70C"}
						Dim FlowsConstraint As List(Of Member) = api.Members.GetBaseMembers(api.Pov.FlowDim.DimPk, api.Members.GetMemberId(dimtype.Flow.Id, "FXFlowsC"))
						For Each FlowC As Member In FlowsConstraint
							Dim ScenarioPOV As String = api.Pov.Scenario.Name
							Dim FlowCName As String = FlowC.Name
							Dim FlowCTrgt As String = Left(FlowCName.ToString, 3)
							Dim FXXC_Override = api.Data.GetDataCell("A#" & AccountEQU4.Name.ToString & ":S#" & ScenarioPOV & ":T#Pov:C#Local:O#Top:I#Top:V#YTD:F#" & FlowCName.ToString & ":U1#top:U2#top:U3#top:U4#None:U5#top:U6#top:U7#top:U8#None").CellAmount
							api.Data.ClearCalculatedData("A#" & AccountEQU4.Name.ToString & ":F#" & FlowCName.ToString & ":UD4#FXOverride", True, True, True)
							If FXXC_Override <> 0 Then
								api.Data.Calculate("A#" & AccountEQU4.Name.ToString & ":F#" & FlowCName.ToString & ":UD4#FXOverride = A#" & AccountEQU4.Name.ToString & ":F#" & FlowCTrgt.ToString & ":UD4#Top_Rules:C#EUR * (-1)")
							End If
				
						Next
				Next
			End If
		End If
							
End Sub
	


#End Region	


#Region "F_Account_and_IC_Mapping"


Sub AccountMapping(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
 
	If api.Cons.IsLocalCurrencyforEntity() Then
       
		' Obtener todos los miembros base de Balance
        Dim accountDim As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "BS"))
        Dim accountMembersEndingWithF As New List(Of Member)

		'Calculo IC Auto
		For Each AccBaseIC As Member In accountDim
			api.Data.ClearCalculatedData("A#" & AccBaseIC.Name & ":UD5#None",True,True,True,True)
			api.Data.Calculate("A#" & AccBaseIC.Name & ":UD5#None:V#YTD = A#" & AccBaseIC.Name & ":UD5#Load_IC:V#YTD", True)
			api.Data.ClearCalculatedData("A#" & AccBaseIC.Name & ":I#None:UD1#S9999:UD5#None",True,True,True,True)
			api.Data.Calculate("A#" & AccBaseIC.Name & ":I#None:UD1#S9999:UD5#None:V#YTD = A#" & AccBaseIC.Name & ":I#Top:UD1#SAP_Client:UD5#Load_IC_None:V#YTD - A#" & AccBaseIC.Name & ":I#Top:UD1#SAP_Client:UD5#Load_IC:V#YTD +  A#" & AccBaseIC.Name & ":I#None:UD1#S9999:UD5#Load_IC:V#YTD", True)
		'Brapi.ErrorLog.LogMessage(si, "0 "  & AccBaseIC.Name)	
		Next		
					
		For Each AccBase As Member In accountDim
			api.Data.ClearCalculatedData("A#" & AccBase.Name & ":UD4#None:UD5#None",True,True,True,True)
			api.Data.Calculate("A#" & AccBase.Name & ":UD4#None:UD5#None:V#YTD = " & "A#" & AccBase.Name & ":UD4#Load_Children:UD5#None:V#YTD", True)				
		Next		
		
        ' Filtrar miembros que terminan con "F"
        For Each member As Member In accountDim
            If member.Name.EndsWith("F", StringComparison.OrdinalIgnoreCase) Then
                accountMembersEndingWithF.Add(member)
            End If			
        Next
		
        ' Realizar cálculos para cada miembro "F"
        For Each memberName As Member In accountMembersEndingWithF
            Dim memberNameWithoutF As String = memberName.Name.Substring(0, memberName.Name.Length - 1)
			
			api.Data.ClearCalculatedData("A#" & memberName.Name & ":UD4#None:UD5#None",True,True,True,True)
           api.Data.Calculate("A#" & memberName.Name & ":UD4#None:UD5#None:V#YTD = " & "A#" & memberNameWithoutF.ToString & ":UD4#Load_Parent:UD5#None:V#YTD - A#" & memberNameWithoutF.ToString & ":UD4#Load_Children:UD5#None:V#YTD", True)

		Next
'____________________________________________________________________________________________________________________________________________________________		
	

		api.Data.ClearCalculatedData("A#1501004CF:UD4#None:UD5#None",True,True,True,True)
		api.Data.Calculate("A#1501004CF"  & ":UD4#None:UD5#None:V#YTD = " & "A#1501004C" & ":UD4#Load_Parent:UD5#None:V#YTD - A#1501004C"  & ":UD4#Load_Children:UD5#None:V#YTD" &
																		"-" & "A#1501004C"  & ":UD4#Load_Parent:UD5#Load_IC:V#YTD + A#1501004C" & ":UD4#Load_Children:UD5#Load_IC:V#YTD" &
																		"-" & "A#1501004C" & ":UD4#Load_Parent:UD5#Load_IC_None:V#YTD + A#1501004C" & ":UD4#Load_Children:UD5#Load_IC_None:V#YTD", True)
			
		api.Data.ClearCalculatedData("A#1501000CF:UD4#None:UD5#None",True,True,True,True)
		api.Data.Calculate("A#1501000CF"  & ":UD4#None:UD5#None:V#YTD = " & "A#1501000C" & ":UD4#Load_Parent:UD5#None:V#YTD - A#1501000C"  & ":UD4#Load_Children:UD5#None:V#YTD" &
																		"-" & "A#1501000C"  & ":UD4#Load_Parent:UD5#Load_IC:V#YTD + A#1501000C" & ":UD4#Load_Children:UD5#Load_IC:V#YTD" &
																		"-" & "A#1501000C" & ":UD4#Load_Parent:UD5#Load_IC_None:V#YTD + A#1501000C" & ":UD4#Load_Children:UD5#Load_IC_None:V#YTD", True)																													

		 For Each memberdeleate As Member In accountDim																
			api.Data.ClearCalculatedData("A#" & memberdeleate.Name & ":UD4#Load_Parent:UD5#None",True,True,True,True)
			api.Data.ClearCalculatedData("A#" & memberdeleate.Name & ":UD4#Load_Children:UD5#None",True,True,True,True)
		 Next
		
	End If																	
End Sub

#End Region	

	
#Region "Helper Functions"				
 
		Private Function InitFlowClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals) As Object
			Try
				Dim sGName As String = "FSKFlows"
				Dim Flow As Object = globals.GetObject(sGName)
				If Not Flow Is Nothing Then
					Return Flow
				Else
					Dim FSKM As New OneStream.BusinessRule.Finance.FSK_ConsolidationRule.MainClass	
					Flow = FSKM.InitFlowClass(Si, Api,Globals)
					Return Flow
				End If
 
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
 
		Private Function InitAccountClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals) As Object
			Try
				Dim sGName As String = "FSKAccounts"
				Dim Account As Object = globals.GetObject(sGName)
				If Not Account Is Nothing Then
					Return Account
				Else
					Dim FSKM As New OneStream.BusinessRule.Finance.FSK_ConsolidationRule.MainClass	
					Account = FSKM.InitAccountClass(Si, Api,Globals)
					Return Account
 
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
 
		End Function
'		Private Function InitNatureClass (ByRef Si As SessionInfo, ByRef Api As FinanceRulesApi, ByVal Globals As BRGlobals, ByVal oDimType As DimType) As Object
'			Try
'				Dim sGName As String = "FSKNature"
'				Dim Nature As Object = globals.GetObject(sGName)
'				If Not Nature Is Nothing Then
'					Return Nature
'				Else
'					Dim FSKM As New OneStream.BusinessRule.Finance.FSK_ConsolidationRule.MainClass	
'					Nature = FSKM.InitNatureClass(Si, Api,Globals, api.Pov.UD4Dim.DimPk)
'					Return Nature
'				End If
'			Catch ex As Exception
'				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
'			End Try
 
'		End Function
 
#End Region

#Region "Cashflow Calculation"
	Sub CashFlowMaps (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
		Dim sbSQLAcc As New Text.StringBuilder()
		Dim sCFAcc As String 
		Dim sOAcc As String 
		Dim sOFlow As String 
		Dim sOSign As String
		Dim sTarget As String 
		Dim sTargetAnnotation As String
		Dim sEntityPOV As Integer = api.Pov.Entity.MemberId
'		Dim sUD7Source As String 
'		Dim sUD7Target As String 
		
'---------------------------------------------------------------------------------------------------------------------------------------------------------------

		Dim memberList_CF As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"CF"))
		Dim memberList_FCF As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"FCF"))

		For Each Account As Member In memberList_CF
			Dim memberString As String = ""
			memberString = "A#" + Account.Name.ToString 
			api.Data.ClearCalculatedData(True, True, True, True,memberString)			
		Next		
		
		For Each Account As Member In memberList_FCF
			Dim memberString As String = ""
			memberString = "A#" + Account.Name.ToString 
			api.Data.ClearCalculatedData(True, True, True, True,memberString)			
		Next	

'---------------------------------------------------------------------------------------------------------------------------------------------------------------


		'If api.Cons.IsLocalCurrencyforEntity() Then
			'Select all the distinct Cashflow accounts defined in the mapping table
			sbSQLAcc.Append(" SELECT Distinct(Destination_Account)")			
			sbSQLAcc.Append(" FROM XFC_CASHFLOW_TABLE")																				
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQLAcc.ToString,False)											
						
					For Each dr As DataRow In dt.Rows
						
						sCFAcc = dr(0).toString
						'Filtering the source accounts based on the Cashflow account.
						Dim sbSQL As New Text.StringBuilder()
			
						sbSQL.Append(" SELECT *")			
						sbSQL.Append(" FROM XFC_CASHFLOW_TABLE")																				
						sbSQL.Append(" WHERE  Destination_Account = '" & sCFAcc & "'")			
						
						'The UD7 member is different for some accounts and it is not defined in the table.
'						If sCFAcc.XFEqualsIgnoreCase("TCAF13") Or sCFAcc.XFEqualsIgnoreCase("TCAF14") Or sCFAcc.XFEqualsIgnoreCase("TFEX07") _
'							Or sCFAcc.XFEqualsIgnoreCase("TFEX08") Or sCFAcc.XFEqualsIgnoreCase("TFEX09") Or sCFAcc.XFEqualsIgnoreCase("TFEX14") _
'							Or sCFAcc.XFEqualsIgnoreCase("TINV023")
'							sUD7Target = "U7#CF_PL_Acc"
'							sUD7Source = "U7#None"
'						Else
'							sUD7Target ="U7#None"
'							sUD7Source ="U7#Top"
'						End If 
						
						'sTarget = "A#"& sCFAcc & ":F#None:U4#CF_Autom:V#YTD:O#Import:I#None:U1#None:U2#None:U3#None:U5#None:U6#None:U8#None:"& sUD7Target
						sTarget = "A#"& sCFAcc & ":F#None:U4#CF_Autom:V#YTD"
						sTargetAnnotation = "A#"& sCFAcc & ":F#None:U4#CF_Autom:V#Annotation:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U5#None:U6#None:U8#None:U7#Top"
						
						'Clear data on the Calculated Cashflow account.
						'(clearCalculatedData, clearTranslatedData, clearConsolidatedData, clearDurableCalculatedData,A#,F#,O#,I#,UD1#,UD2#,UD3#,UD4#,UD5#,UD6#,UD7#,UD8#)		
						api.Data.ClearCalculatedData(True, True, True, True,"A#" & sCFAcc,,,,,,,,,,, )
							
						Using dbConnApp2 As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
							Using dt2 As DataTable = BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)
								Dim sCFCalc As String = String.Empty
								Dim sCFText As String = String.Empty 
								For Each dr2 As DataRow In dt2.Rows
									
									If sCFCalc = String.Empty Then
										sOSign = dr2(1).toString
										If sOSign.XFEqualsIgnoreCase("+") Then sOSign = String.Empty 
									Else
										sOSign = dr2(1).toString
									End If
									sOAcc = dr2(2).toString
									sOFlow = dr2(3).toString
									
									'sCFCalc = sCFCalc & sOSign & "A#"& sOAcc &":F#"& sOFlow & ":C#Local:O#Top:I#Top:V#YTD:U1#Top:U2#Top:U3#Top:U4#Top:U5#None:U6#None:U8#None:"& sUD7Source
									sCFCalc = sCFCalc & sOSign & "A#"& sOAcc &":F#"& sOFlow & ":U4#Top:V#YTD"
									sCFText = sCFText & sOsign & " A#"& sOAcc &":F#"& sOFlow & " "
								Next
								api.Data.Calculate (sTarget & " = " & sCFCalc, True)
								api.Data.SetDataAttachmentText (stargetAnnotation, sCFText, False)
								'brapi.ErrorLog.LogMessage (si, "CF Calc "& sEntityPOV & "/ " & sTarget & " = " & scfcalc )
							End Using
						End Using				
					Next				
				End Using
			End Using
			'--------------------------------------------TFEX Calc---------------------------------------------------------------
'			Dim objDimPk As DimPk = BRApi.Finance.Dim.GetDimPk(si, "Account_Details")
'			Dim memberList_ASSETS As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"ASSETS"))
'			Dim memberList_02AC07 As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"02AC07"))
'			Dim memberList_LIABILITIES As List(Of Member) = Api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk,api.Members.GetMemberId(dimtype.Account.Id,"LIABILITIES"))

'			Dim memberString As String = ""

'				For Each Account As Member In memberList_ASSETS
'						memberString = memberString + "- A#" + Account.Name.ToString + ":F#F80C:U4#Top:V#YTD" + "- A#" + Account.Name.ToString + ":F#F80M:U4#Top:V#YTD"		
'				Next			
			
'				For Each Account As Member In memberList_02AC07
'						memberString = memberString + "+ A#" + Account.Name.ToString + ":F#F80C:U4#Top:V#YTD" + "+ A#" + Account.Name.ToString + ":F#F80M:U4#Top:V#YTD"
'				Next			
			
			
'				For Each Account As Member In memberList_LIABILITIES
'						memberString = memberString + "+ A#" + Account.Name.ToString + ":F#F80C:U4#Top:V#YTD" + "+ A#" + Account.Name.ToString + ":F#F80M:U4#Top:V#YTD"
'				Next		
				
		
'___________________________________________________________________________________________________________________________________________					
'			api.Data.ClearCalculatedData(True, True, True, True,"A#TFEX")
'			api.Data.Calculate("A#TFEX:F#None:U4#CF_Autom:V#YTD = - A#ASSETS:F#F80C:U4#Top:V#YTD
'																- A#ASSETS:F#F80M:U4#Top:V#YTD	
'																+ A#02AC07:F#F80C:U4#Top:V#YTD
'																+ A#02AC07:F#F80M:U4#Top:V#YTD
'																+ A#LIABILITIES:F#F80C:U4#Top:V#YTD
'																+ A#LIABILITIES:F#F80M:U4#Top:V#YTD
'																+ A#4927:F#None:U4#Top:V#YTD
'																+ A#4928:F#None:U4#Top:V#YTD",True)										
	'		api.Data.Calculate("A#TFEX:F#None:U4#CF_Autom:V#YTD = memberString",True)

	End Sub
#End Region 

End Class

#Region "FXRate Class"
	Public Class globalClass
		Public closingRateToText As String
		Public averageRateToText As String
		Public averageRatePriorYEtoText As String
		Public closingRatePriorYEtoText As String
		Public closingRatePriorYEtoText2 As String
		Public OpeScenario As String
		Sub New (ByRef oSi As SessionInfo, ByRef oApi As FinanceRulesApi, ByVal oArgs As FinanceRulesArgs)
			Dim referenceBR As New OneStream.BusinessRule.Finance.FSK_OpeningScenario.MainClass	
			Dim opeScenario As String = referenceBR.fctOpeningScenario(oSi, oApi, oArgs)
			Dim timeId As Integer = oapi.Pov.Time.MemberPk.MemberId
			Dim timeprioryearend As Integer = oapi.Time.GetLastPeriodInPriorYear
			Dim timeprioryearend2 As Integer = oapi.Time.GetLastPeriodInPriorYear(timeprioryearend)
		    Dim rateTypeClo As FxRateType = oapi.FxRates.GetFxRateTypeForAssetLiability()
			Dim rateTypeAve As FxRateType = oapi.FxRates.GetFxRateTypeForRevenueExp()
		    Dim closingRate As Decimal =  oapi.FxRates.GetCalculatedFxRate(rateTypeClo,timeId)			
			Dim AverageRate As Decimal =  oapi.FxRates.GetCalculatedFxRate(rateTypeAve,timeId)
			Dim openingScenarioId As Integer = oapi.Members.GetMember(DimType.Scenario.Id, OpeScenario).MemberId
			Dim ClosingRateTypePriorYE As FxRateType = oapi.Scenario.GetFxRateTypeForAssetLiability(openingScenarioId)
			Dim AverageRateTypePriorYE As FxRateType = oapi.Scenario.GetFxRateTypeForRevenueExpense(openingScenarioId)		
			Dim closingRatePriorYE As Decimal = oapi.FxRates.GetCalculatedFxRate(ClosingRateTypePriorYE,timeprioryearend)
			Dim AverageRatePriorYE As Decimal = oapi.FxRates.GetCalculatedFxRate(AverageRateTypePriorYE,timeprioryearend)
			Dim closingRatePriorYE2 As Decimal = oapi.FxRates.GetCalculatedFxRate(ClosingRateTypePriorYE,timeprioryearend2)
			closingRateToText = closingRate.ToString("G",CultureInfo.InvariantCulture)
			averageRateToText = AverageRate.ToString("G",CultureInfo.InvariantCulture)	
			averageRatePriorYEtoText = AverageRatePriorYE.ToString("G",CultureInfo.InvariantCulture)
			closingRatePriorYEtoText = ClosingRatePriorYE.ToString("G",CultureInfo.InvariantCulture)
			closingRatePriorYEtoText2 = ClosingRatePriorYE2.ToString("G",CultureInfo.InvariantCulture)
		End Sub
	End Class
#End Region
	
End Namespace