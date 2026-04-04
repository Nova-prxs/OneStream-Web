Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.Finance.CONT_Calc_Previsto
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
								
				Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		        Dim myWorkflowUnitPk As WorkflowUnitPk = BRApi.Workflow.General.GetWorkflowUnitPk(si)
		        Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, myWorkflowUnitPk.TimeKey)
		        Dim objTimeMemberSubComponents As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
		        Dim wfYear As String = objTimeMemberSubComponents.Year.ToString
		        Dim wfMes As String = objTimeMemberSubComponents.Month.ToString
				Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
				
				
													
					'If wfMes = "3" Then
					If api.Pov.Scenario.Name = "P03" And wfMes = "3" Then
						
						Me.Calc_Previsto_P03(si, api, globals, args)
						
						
					'Else If wfMes = "6" Then
					Else If api.Pov.Scenario.Name = "P06" Then 'And wfMes = "6" Then
						
						'brapi.ErrorLog.LogMessage(si, "1- "&CurYear)
						
						If CurYear = 2022 And wfMes = "5" Then 'En 2022 P06 se cargó con el cierre de Mayo		
							
							Me.Calc_Previsto_P06(si, api, globals, args)
							
						Else If CurYear <> 2022 And wfMes = "6" Then 'El resto de años debe funcionar igual
							
							Me.Calc_Previsto_P06(si, api, globals, args)
							
						End If
					'Else If wfMes = "9" Then
					Else If api.Pov.Scenario.Name = "P09" And wfMes = "9" Then
						
						Me.Calc_Previsto_P09(si, api, globals, args)
						
					'fas 2023-11-23_____QUITADO PORQ SE TUVO QUE COPIAR P09 EN P11
					'Else If wfMes = "10" Then
					'Else If api.Pov.Scenario.Name = "P11" And wfMes = "10" Then
					
						'Me.Calc_Previsto_P11(si, api, globals, args)
					'fas 2023-11-23_____QUITADO PORQ SE TUVO QUE COPIAR P09 EN P11
				'fas 2023-10-3_____
					Else If api.Pov.Scenario.Name = "O1"  And wfMes = "9" Then
					
						Me.Calc_Objetivo_O1(si, api, globals, args)
				'fas 2023-10-3____	
					End If
						
					
								
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
			'fas 2023-10-3___________
Sub Calc_Objetivo_O1 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
	Dim CurPreviusYear As Integer = CurYear-1
				
	If ((Not api.Entity.HasChildren()) And (api.Cons.IsLocalCurrencyforEntity())) Then
					
		If api.Pov.Time.Name = CurYear  & "M12" Then
																											
			'Calculamos el O1 M12 --> Copia del Real M9
			'fas 2023-11-15___________
			'api.Data.Calculate("S#O1   = S#R:T#" & CurPreviusYear  & "M9", "A#Pres_Aprob_Año_N.Base","F#SDC",,,,,,,,,,,,, True)
			api.Data.Calculate("S#O1   = S#R:T#" & CurPreviusYear   & "M9", "A#Valor_bruto.base","F#SDC",,,,,,,,,,,,, True)
									
		End If		
	End If																							
End Sub
		'fas 2023-10-3___________________
			
		
Sub Calc_Previsto_P03 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
				
	If ((Not api.Entity.HasChildren()) And (api.Cons.IsLocalCurrencyforEntity())) Then
					
		If api.Pov.Time.Name = CurYear & "M12" Then
																											
			'Calculamos el P03 M12 --> Copia del Real M3
			api.Data.Calculate("S#P03 = S#R:T#" & CurYear & "M3", "A#Pres_Aprob_Año_N.Base","F#SDC",,,,,,,,,,,,, True)
			'fas 2023-10-3
			api.Data.Calculate("S#P03 = S#R:T#" & CurYear & "M3", "A#Valor_bruto.base","F#SDC",,,,,,,,,,,,, True)
									
		End If		
	End If																							
End Sub	

Sub Calc_Previsto_P06 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
				
	If ((Not api.Entity.HasChildren()) And (api.Cons.IsLocalCurrencyforEntity())) Then
		
		If CurYear = 2022 Then
		'brapi.ErrorLog.LogMessage(si, "2- "&CurYear)			
			If api.Pov.Time.Name = CurYear & "M12" Then		
																				
				'Calculamos el P06 M12 --> Copia del Real M6
				api.Data.Calculate("S#P06 = S#R:T#" & CurYear & "M5", "A#Pres_Aprob_Año_N.Base","F#SDC",,,,,,,,,,,,, True)
			'fas 2023-10-3
				api.Data.Calculate("S#P06 = S#R:T#" & CurYear & "M5", "A#Valor_bruto.base","F#SDC",,,,,,,,,,,,, True)

										
			End If
		Else 
			
			If api.Pov.Time.Name = CurYear & "M12" Then		
																				
				'Calculamos el P06 M12 --> Copia del Real M6
				api.Data.Calculate("S#P06 = S#R:T#" & CurYear & "M6", "A#Pres_Aprob_Año_N.Base","F#SDC",,,,,,,,,,,,, True)
			'fas 2023-10-3
				api.Data.Calculate("S#P06 = S#R:T#" & CurYear & "M6", "A#Valor_bruto.base","F#SDC",,,,,,,,,,,,, True)
										
			End If
			
		End If	
	End If
																							
End Sub	

Sub Calc_Previsto_P09 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
				
	If ((Not api.Entity.HasChildren()) And (api.Cons.IsLocalCurrencyforEntity())) Then
		
		If api.Pov.Time.Name = CurYear & "M12" Then
																																
			'Calculamos el P09 M12 --> Copia del Real M9
			api.Data.Calculate("S#P09 = S#R:T#" & CurYear & "M9", "A#Pres_Aprob_Año_N.Base","F#SDC",,,,,,,,,,,,, True)
			'fas 2023-10-3
			api.Data.Calculate("S#P09 = S#R:T#" & CurYear & "M9", "A#Valor_bruto.base","F#SDC",,,,,,,,,,,,, True)

											
		End If		
	End If																							
End Sub

Sub Calc_Previsto_P11 (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
				
	If ((Not api.Entity.HasChildren()) And (api.Cons.IsLocalCurrencyforEntity())) Then
							
		If api.Pov.Time.Name = CurYear & "M12" Then
																															
			'Calculamos el P11 M12 --> Copia del Real M10
			api.Data.Calculate("S#P11 = S#R:T#" & CurYear & "M10", "A#Pres_Aprob_Año_N.Base","F#SDC",,,,,,,,,,,,, True)
			'fas 2023-10-3
			api.Data.Calculate("S#P11 = S#R:T#" & CurYear & "M10", "A#Valor_bruto.base","F#SDC",,,,,,,,,,,,, True)

									
		End If		
	End If																							
End Sub	

	End Class
End Namespace