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

Namespace OneStream.BusinessRule.DashboardStringFunction.CR_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetFlowText1") Then
					Return  Me.GetFlowText1(si, globals, api, args)
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetFlowText1 (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
			 		
				Dim parametersReport As String = String.Empty
				Dim selectedAccount As String = String.Empty
				Dim selectedScenario As String = String.Empty
				Dim selectedTime As String = String.Empty
				
				Dim flowText1 As String = String.Empty
				
				'Get the account parameter
				selectedAccount = args.NameValuePairs.XFGetValue("P_Account", String.Empty)
				'Get the Scenario parameter
				selectedScenario = args.NameValuePairs.XFGetValue("P_Scenario", String.Empty)
				'Get the time parameter
				selectedTime = args.NameValuePairs.XFGetValue("P_Time", String.Empty)
				
				
				Dim mbrAccountID As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId.Account, selectedAccount)
				Dim textPropertyIndex As Integer = 1
				Dim varyByScenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, BRApi.Finance.Members.GetMemberId(si,DimType.Scenario.Id, selectedScenario)).Id
				Dim varyByTimeId As Integer = BRApi.Finance.Members.GetMemberId(si, DimType.Time.Id, selectedTime)
				
				' Get the flow ID based in the Text1 property of the account
				flowText1  = BRApi.Finance.Account.Text(si, mbrAccountID, 1, varyByScenarioTypeId, varyByTimeId)
				
				
				parametersReport = "F#" & flowText1 & ".Base.Remove(F#F00,F#F14)"
				
				'BRApi.ErrorLog.LogMessage(si," --------------- parametersReport - " &parametersReport)
				
				Return parametersReport

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace