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

Namespace OneStream.BusinessRule.Finance.CopyFlows
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				
				Me.CopyFlows(si, api, globals, args)	
				
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Sub CopyFlows (ByVal si As SessionInfo, ByVal api As FinanceRulesApi, ByVal globals As BRGlobals, ByVal args As FinanceRulesArgs)
					
			'Dim timeId As Integer = api.Pov.Time.MemberPk.MemberId
			'Dim CurYear As Integer = api.Time.GetYearFromId(timeId)
			'Dim PrevYear As Integer = CurYear - 1
			'Dim periodMultiplier As Integer = TimeDimHelper.GetSubComponentsFromId(api.Pov.Time.MemberId).Month
			'Dim DATOM12 As String = (CurYear & "M12")
			'Dim prevperiodMultiplier As Integer = periodMultiplier - 1
			'Dim PrevYear1 As Integer = PrevYear - 1
			Dim ScenarioType As String = api.Scenario.GetScenarioType.ToString 'NOVA
			
			api.Data.ClearCalculatedData("F#F10",True,True,True,True)
			api.Data.ClearCalculatedData("F#F01",True,True,True,True)
			
			Dim CuentasBS As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "BS"))
			Dim CuentasEQT As List(Of Member) = api.Members.GetBaseMembers(api.Pov.AccountDim.DimPk, api.Members.GetMemberId(dimtype.Account.Id, "EQUITY"))
			
			If ScenarioType.Equals("Budget") And api.Pov.Time.Name = "2023M12" Then
				If Not(api.Pov.Entity.Name.Equals("R1303001") Or api.Pov.Entity.Name.Equals("R1303")) Then
				'If (api.Pov.Entity.Name.Equals("R1303001") Or api.Pov.Entity.Name.Equals("R1303")) Then
					
					For Each Account As Member In CuentasBS 
						If Not(CuentasEQT.Contains(Account)) Then
							api.Data.Calculate("F#F01:A#" & Account.Name & " = F#F99i:A#" & Account.Name,,,,,,,,,,,,,,, True)
						End If
					Next
					
					api.Data.Calculate("F#F10:A#3001080C = F#F99i:A#3001080C",,,,,,,,,,,,,,, True)
					api.Data.Calculate("F#F10:A#3001090C = F#F99i:A#3001090C",,,,,,,,,,,,,,, True)
					
				End If
			
			ElseIf ScenarioType.Equals("Forecast") And api.Pov.Time.Name = "2023M12" Then
				
				api.Data.ClearCalculatedData("F#F10",True,True,True,True)
				api.Data.ClearCalculatedData("F#F91",True,True,True,True)
				

'Las cuentas de Equity Entidades EUR se copia manual al F01
'Las cuentas de Equity Entidades No EUR ya se ha hecho manualmente por el form EquityFX
				
				For Each Account As Member In CuentasBS 
					If Not(Account.Name.Equals("3001080C")) Then
						api.Data.Calculate("F#F01:A#" & Account.Name & " = F#F99i:A#" & Account.Name,,,,,,,,,,,,,,, True)
					End If
				Next
				
				api.Data.Calculate("F#F10:A#3001080C = F#F99i:A#3001080C",,,,,,,,,,,,,,, True)
				api.Data.Calculate("F#F10:A#3001090C = F#F99i:A#3001090C",,,,,,,,,,,,,,, True)
			
				
			ElseIf ScenarioType.Equals("Actual") And api.Pov.Time.Name = "2023M12" Then
				
				For Each Account As Member In CuentasEQT 
					If Not(Account.Name.Equals("3001080C") And Account.Name.Equals("3001090C")) Then
						api.Data.Calculate("F#F01:A#" & Account.Name & " = F#F99i:A#" & Account.Name,,,,,,,,,,,,,,, True)
					End If
				Next
				
			End If
	

		
		End Sub
	End Class
End Namespace