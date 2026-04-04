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

Namespace OneStream.BusinessRule.Finance.CONS_Mensualizacion
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
							Me.Calc_Mensualizacion(si, api, globals, args)										
								
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
		
	Sub Calc_Mensualizacion (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim sScenarioType As String = api.Pov.ScenarioType.Name	
	
		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)		
		Dim ObjetivoM12 As String = (CurYear & "M12")
		Dim PriorYear As Integer = CurYear -1
				
		'Definimos el multiplicador de meses
		Dim periodMultiplier As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
		'Adaptamos el multiplicador para P03, aplica desde Abril
		Dim P03Multiplier As Integer = (periodMultiplier - 3)
		'Adaptamos el multiplicador para P06, aplica desde Julio
		Dim P06Multiplier As Integer = (periodMultiplier - 6)
		'Adaptamos el multiplicador para P09, aplica desde Octubre
		Dim P09Multiplier As Integer = (periodMultiplier - 9)
		
		api.Data.ClearCalculatedData(True, True, True, True,,,,,,,,"UD4#Repartir_Mensualizacion",,,,)	
		
		If (Not api.Entity.HasChildren() And (api.Cons.IsLocalCurrencyforEntity() Or api.Pov.Cons.Name = "OwnerPreAdj" Or api.Pov.Cons.Name = "OwnerPostAdj")) Then 			 
			
			'2023 - Nueva pregunta por typo de escenario
			If (api.Pov.ScenarioType.Name = "Budget" _ 
			Or api.Pov.ScenarioType.Name = "Forecast" ) Then
			
			'If (api.Pov.Scenario.Name = "O1" _ 
			'Or api.Pov.Scenario.Name = "O2" _
			'Or api.Pov.Scenario.Name = "O3" _
			'Or api.Pov.Scenario.Name = "P03" _
			'Or api.Pov.Scenario.Name = "P06" _
			'Or api.Pov.Scenario.Name = "P09" _
			'Or api.Pov.Scenario.Name = "P11") Then			
										
				Dim DatoRealUltimo = api.Data.GetDataCell("A#CR_Z:S#R:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
				Dim DatoP11Ultimo = api.Data.GetDataCell("A#CR_Z:S#P11:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
				Dim DatoP09Ultimo = api.Data.GetDataCell("A#CR_Z:S#P09:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
				Dim DatoP06Ultimo = api.Data.GetDataCell("A#CR_Z:S#P06:T#POVPriorYearM12:O#Top:I#Top:V#YTD:F#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
				
	
				Dim ScenarioBase As String
				
					If DatoRealUltimo <> 0 Then
						ScenarioBase = "R"
						
					Else If DatoP11Ultimo <> 0 Then
						ScenarioBase = "P11"
						
					Else If DatoP09Ultimo <> 0 Then
						ScenarioBase = "P09"
						
					Else If DatoP06Ultimo <> 0 Then
						ScenarioBase = "P06"
						
					Else 
						ScenarioBase = "P03"
						
					End If
										
''xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx				
		'OBJETIVO: O1, O2 or O3 CDR, ANX, INVER_DESINVER, INMO_GESTION, RH, CV y CD

				If Not api.Pov.Time.Name = CurYear.ToString() & "M12" Then		
																																								
					'CDR					
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#CDR.Base",,,,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#R_CDR.Base",,,,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#730100",,,,,,,,,,,,,, True)
	
					'129000
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#129000",,,,,,,,,,,,,, True)					

					'ANX
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#ANX.Base",,,,,,,,,,,,,, True)
					
					'INVER_DESINVER
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#INVER_DESINV.Base",,,,,,,,,,,,,, True)
					
					'INMO_GESTION
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "/12 * " & periodMultiplier & "", "A#INMO_GESTION.Base",,,,,,,,,,,,,, True)
					
					'RH - FOTO
					api.Data.ClearCalculatedData(True, True, True, True,"A#RH.Base.Where(Text2 Contains Foto).Remove(RH021)",,,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + (((T#" & ObjetivoM12 & ":UD4#None - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#RH.Base.Where(Text2 Contains Foto)",,,,,,,,,,,,,, True)
															
					'RH - NO FOTO
					api.Data.ClearCalculatedData(True, True, True, True,"A#RH.Base.Where(Text2 DoesNotContain Foto).Remove(RH021)",,,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#None:T#" & ObjetivoM12 & " /12 * " & periodMultiplier & "", "A#RH.Base.Where(Text2 DoesNotContain Foto).Remove(RH021)",,,,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#RetSocG:T#" & ObjetivoM12 & " /12 * " & periodMultiplier & "", "A#RH.Base.Where(Text2 DoesNotContain Foto).Remove(RH021)",,,,,,,,,,,,,, True)
				
					'CV
					api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & " /12 * " & periodMultiplier & "", "A#CV.Base.Where(Text1 Contains Mensualizacion)",,,,,,,,,,,,,, True)
					
						'CV expepcion CV_CF
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV_Ventas","F#APV",,,,,,,,,,,,, True)
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV_FACT","F#APV",,,,,,,,,,,,, True)
						
						'CV expepcion CVXXX
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV001",,,,,,,,,,,,,, True)
						api.Data.Calculate("UD4#Repartir_Mensualizacion = UD4#Gestion:T#" & ObjetivoM12 & "", "A#CV006",,,,,,,,,,,,,, True)
						
					'borramos datos previos de CD
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD003B.Base",,,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD004B.Base",,,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD005B.Base",,,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD.Base",,,,,,,"UD4#Repartir_Mensualizacion",,,,)
										
					'CD003B
					'api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + (((T#" & ObjetivoM12 & ":UD4#None - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD003B.Base",,,,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal) / 12) * " & periodMultiplier & ")", "A#CD003B.Base",,,,,,,,,,,,,, True)
					
					'CD004B
					'api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + (((T#" & ObjetivoM12 & ":UD4#None - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD004B.Base",,,,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal) / 12) * " & periodMultiplier & ")", "A#CD004B.Base",,,,,,,,,,,,,, True)
						
					'CD005B
					'api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + (((T#" & ObjetivoM12 & ":UD4#None - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD005B.Base",,,,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Legal) / 12) * " & periodMultiplier & ")", "A#CD005B.Base",,,,,,,,,,,,,, True)
						
					'CD
					'api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None + (((T#" & ObjetivoM12 & ":UD4#None - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#None) / 12) * " & periodMultiplier & ")", "A#CD.Base",,,,,,,,,,,,,, True)
					api.Data.Calculate("UD4#Repartir_Mensualizacion = S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Gestion + (((T#" & ObjetivoM12 & ":UD4#Gestion - S#" & ScenarioBase & ":T#" & PriorYear.ToString & "M12:UD4#Gestion) / 12) * " & periodMultiplier & ")", "A#CD.Base",,,,,,,,,,,,,, True)
																																																						
					'borramos todos los MOV de CD para que se calculen bien a la primera consolidacion
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD003B.base","F#MOV",,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD004B.base","F#MOV",,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD005B.base","F#MOV",,,,,,"UD4#Repartir_Mensualizacion",,,,)
					api.Data.ClearCalculatedData(True, True, True, True,"A#CD.base","F#MOV",,,,,,"UD4#Repartir_Mensualizacion",,,,)
				
				End If																					
																
			End If 'Scenario
				
	End If 'is local or Legal
		
End Sub	
		
		
	End Class
End Namespace