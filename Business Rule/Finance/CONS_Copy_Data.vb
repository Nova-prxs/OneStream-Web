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

Namespace OneStream.BusinessRule.Finance.CONS_Copy_Data
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				Me.Copiado_Datos(si, api, globals, args)	

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
	Sub Copiado_Datos(ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)

		Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
		Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
		Dim WMes As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
				
		
		
		If api.Pov.Scenario.Name = "R" Then
			
			If Not (api.Entity.HasChildren()) Then							
			
				If api.Cons.IsLocalCurrencyforEntity() Then
																														
					If api.Pov.Time.Name = CurYear.ToString() & "M7" Then
						
						'Import
						api.Data.Calculate("S#R:O#Import = S#R:T#" & CurYear.ToString & "M6:O#Import", "A#Accounts.Base","F#Top.base.remove(DBE)",,,,,,,,,,,,, True)
						'Forms
						api.Data.Calculate("S#R:O#Forms = S#R:T#" & CurYear.ToString & "M6:O#Forms", "A#Accounts.Base.remove(RH021)","F#Top.base.remove(DBE)",,,,,,,,,,,,, True)
						api.Data.Calculate("S#R:O#Forms:F#FPE = S#R:T#" & CurYear.ToString & "M6:O#Forms:F#FPE", "A#BS.Base",,,,,,,,,,,,,, True)

					ElseIf api.Pov.Time.Name = CurYear.ToString() & "M11" Then
						
						'Import
						api.Data.Calculate("S#R:O#Import = S#R:T#" & CurYear.ToString & "M10:O#Import", "A#Accounts.Base","F#Top.base.remove(DBE)",,,,,,,,,,,,, True)
						'Forms
						api.Data.Calculate("S#R:O#Forms = S#R:T#" & CurYear.ToString & "M10:O#Forms", "A#Accounts.Base.remove(RH021)","F#Top.base.remove(DBE)",,,,,,,,,,,,, True)
						api.Data.Calculate("S#R:O#Forms:F#FPE = S#R:T#" & CurYear.ToString & "M10:O#Forms:F#FPE", "A#BS.Base",,,,,,,,,,,,,, True)

					End If																					
				End If															
			End If
		End If

	End Sub
	End Class
End Namespace