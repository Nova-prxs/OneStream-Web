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

Namespace OneStream.BusinessRule.Finance.CONS_Extrapolacion
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Dim sScenarioWF As String = args.CustomCalculateArgs.NameValuePairs.XFGetValue("Scenario_WF")				
				'BRApi.ErrorLog.LogMessage(si,"Scenario: " & Api.Pov.Scenario.Name)
				'BRApi.ErrorLog.LogMessage(si,"Scenario: " & sScenarioWF)
				If sScenarioWF = "R"					
					Me.Calc_Extrapolacion(si, api, globals, args)
					'BRApi.ErrorLog.LogMessage(si,"Entra")
				End If									
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
		
Sub Calc_Extrapolacion (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

	'Definimos variables temporales
	Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
	Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
	Dim PriorYear As Integer = CurYear -1

	'Definimos el multiplicador de meses
	Dim periodMultiplier As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month

	Dim P03_M12 = api.Data.GetDataCell("A#CR_Z:S#P03:T#" & CurYear.ToString() & "M12:V#YTD:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
	Dim P06_M12 = api.Data.GetDataCell("A#CR_Z:S#P06:T#" & CurYear.ToString() & "M12:V#YTD:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
	Dim P09_M12 = api.Data.GetDataCell("A#CR_Z:S#P09:T#" & CurYear.ToString() & "M12:V#YTD:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
	Dim P011_M12 = api.Data.GetDataCell("A#CR_Z:S#P11:T#" & CurYear.ToString() & "M12:V#YTD:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount
	Dim O1_M12 = api.Data.GetDataCell("A#CR_Z:S#O1:T#" & CurYear.ToString() & "M12:V#YTD:F#Top:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Gestion:U5#Top:U6#Top:U7#Top:U8#None").CellAmount


		Dim ScenarioBase As String

			If P011_M12 <> 0 Then
				
				ScenarioBase = "P11"
				
			Else If P09_M12 <> 0 Then
				
				ScenarioBase = "P09"
				
			Else If P06_M12 <> 0 Then
				
				ScenarioBase = "P06"	
				
			Else If P03_M12 <> 0 Then
				
				ScenarioBase = "P03"
				
			Else
				ScenarioBase = "O1"
				
			End If


	If (Not api.Entity.HasChildren()) Then 
				
		If (api.Cons.IsLocalCurrencyforEntity() Or api.Pov.Cons.Name ="OwnerPreAdj" Or api.Pov.Cons.Name ="OwnerPostAdj") Then
			
			api.Data.ClearCalculatedData("S#R_Extrapolado", True, True, True, True)
		
			'CDR
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#CDR.Base",,,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#R_CDR.Base",,,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#730100",,,,,,,,,,,,,, True)
			
			'ANX
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#ANX.Base",,,,,,,,,,,,,, True)

			'INVER_DESINV
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#INVER_DESINV.Base",,,,,,,,,,,,,, True)
			
			'RH NO FOTO
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12","A#RH.Base",,,,,,,,,,,,,, True)
			
			'RH FOTO		
			api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "","A#RH.Base.Where(Text2 Contains Foto)",,,,,,,,,,,,,, True)
						
			'CV
			api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "", "A#CV_Ventas","F#APV",,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#CV_Ventas","F#INC,F#OMO",,,,,,,,,,,,, True)
			
			api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "", "A#CV_Fact","F#APV",,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#CV_Fact","F#INC,F#DIS,F#OMO",,,,,,,,,,,,, True)
			
			api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#CV_A_FCT,A#CV_B_FCT",,,,,,,,,,,,,, True)
				
			'CD003B
			'api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "", "A#CD003B.base",,,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = S#R:T#" & PriorYear.ToString & "M12 + (((S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & " - S#R:T#" & PriorYear.ToString & "M12)/ 12) * " & periodMultiplier & ")", "A#CD003B.base",,,,,,,,,,,,,, True)
				
			'CD004B
			'api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "", "A#CD004B.base",,,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = S#R:T#" & PriorYear.ToString & "M12 + (((S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & " - S#R:T#" & PriorYear.ToString & "M12)/ 12) * " & periodMultiplier & ")", "A#CD004B.base",,,,,,,,,,,,,, True)
			
			'CD005B
			'api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "", "A#CD005B.base",,,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = S#R:T#" & PriorYear.ToString & "M12 + (((S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & " - S#R:T#" & PriorYear.ToString & "M12)/ 12) * " & periodMultiplier & ")", "A#CD005B.base",,,,,,,,,,,,,, True)
		
			'CD
			'api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "", "A#CD.Base",,,,,,,,,,,,,, True)
			api.Data.Calculate("S#R_Extrapolado = S#R:T#" & PriorYear.ToString & "M12 + (((S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & " - S#R:T#" & PriorYear.ToString & "M12)/ 12) * " & periodMultiplier & ")", "A#CD.base",,,,,,,,,,,,,, True)
		
			'borramos todos los MOV para que se calculen bien a la primera
			api.Data.ClearCalculatedData("S#R_Extrapolado:F#MOV", True, True, True, True)
			
								
		End If
	Else
		'En legal tenemos q llevarnos los ajustes imputados en esa entidad
		'CV
		api.Data.Calculate("S#R_Extrapolado = S#R:T#" & CurYear.ToString() & "M" & periodMultiplier & "", "A#CV001,A#CV006",,"O#AdjInput",,,,,,,,,,,, True)
		api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#CV.base.remove(A#CV001,A#CV006)",,"O#AdjInput",,,,,,,,,,,, True)
		
		'CDR
		api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#CDR.Base",,"O#AdjInput",,,,,,,,,,,, True)
			
		'ANX
		api.Data.Calculate("S#R_Extrapolado = (S#R / " & periodMultiplier & ") * 12", "A#ANX.Base",,"O#AdjInput",,,,,,,,,,,, True)
		
	End If		
																					
		
End Sub	
		
		
	End Class
End Namespace