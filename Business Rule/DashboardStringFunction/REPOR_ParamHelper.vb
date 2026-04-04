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




Namespace OneStream.BusinessRule.DashboardStringFunction.REPOR_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				
				If args.FunctionName.XFEqualsIgnoreCase("GetConsolidationAndParentMemberGroup") Then
					Return Me.GetConsolidationAndParentMemberGroup(si, globals, api, args)
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetConsolidationAndParentMemberCountry") Then
					Return Me.GetConsolidationAndParentMemberCountry(si, globals, api, args)
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetConsolidationAndParentMemberGroupDashboard") Then
					Return Me.GetConsolidationAndParentMemberGroupDashboard(si, globals, api, args)
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetConsolidationAndParentMemberGroupDashboardVariance") Then
					Return Me.GetConsolidationAndParentMemberGroupDashboardVariance(si, globals, api, args)
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetConsolidationAndParentMemberCountryDashboard") Then
					Return Me.GetConsolidationAndParentMemberCountryDashboard(si, globals, api, args)
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetConsolidationAndParentMemberCountryDashboardVariance") Then
					Return Me.GetConsolidationAndParentMemberCountryDashboardVariance(si, globals, api, args)
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetConsolidationAndParentMemberGroup (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
			
				Dim parametersReport As String = String.Empty
				Dim ConsoMember As String = String.Empty
				Dim ParentMember As String = String.Empty
				
				'Get the entity parameter
				Dim SelectedEntity As String = args.NameValuePairs.XFGetValue("P_Entity", String.Empty)
				'Get the scenario parameter
				Dim SelectedScenario As String = args.NameValuePairs.XFGetValue("P_Scenario", String.Empty)
				
				If (SelectedEntity.Equals("Horse_Group")) Then
					ConsoMember = "Local"
				Else
					ConsoMember = "Top"	
				End If
				
				'Get the number of characters (5 -> country, 8 -> Base member)
				Dim LenName As Integer = Len(SelectedEntity)
				
				If (LenName = 5) Then 'country entity selected
					ParentMember = "Horse_Group"
				Else If (LenName = 8) 'base entity selected
					ParentMember = "Horse_Flat"
					
				End If
				
				If (ParentMember.Equals("")) Then
					parametersReport = "E#" & SelectedEntity & ":S#" & SelectedScenario & ":C#" & ConsoMember
				Else
					parametersReport = "E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario & ":C#" & ConsoMember
				End If
				
				
				'BRApi.ErrorLog.LogMessage(si," --------------- " &parametersReport)
				
				Return parametersReport

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetConsolidationAndParentMemberCountry (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
			
				Dim parametersReport As String = String.Empty
				Dim ConsoMember As String = String.Empty
				Dim ParentMember As String = String.Empty
				
				'Get the entity parameter
				Dim SelectedEntity As String = args.NameValuePairs.XFGetValue("P_Entity", String.Empty)
				'Get the scenario parameter
				Dim SelectedScenario As String = args.NameValuePairs.XFGetValue("P_Scenario", String.Empty)
				
				'Get the number of characters (5 -> country, 8 -> Base member)
				Dim LenName As Integer = Len(SelectedEntity)
				
				If (LenName = 5) Then 'country entity selected
					ConsoMember = "Local"
				Else If (LenName = 8) 'base entity selected
					ParentMember = Left(SelectedEntity,5)
					ConsoMember = "Top"	
				Else ' Conso member para Horse_Group definir con Sebas
					ConsoMember = "Local"
				End If
				
				If (ParentMember.Equals("")) Or (SelectedEntity = "Horse_Group") Then
					parametersReport = "E#" & SelectedEntity & ":S#" & SelectedScenario & ":C#" & ConsoMember
				Else
					parametersReport = "E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario & ":C#" & ConsoMember
				End If
				
				
				'BRApi.ErrorLog.LogMessage(si," --------------- " &parametersReport)
				
				Return parametersReport

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetConsolidationAndParentMemberGroupDashboard (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
			
				Dim parametersReport As String = String.Empty
				Dim ConsoMember As String = String.Empty
				Dim ParentMember As String = String.Empty
				
				'Get the entity parameter
				Dim SelectedEntity As String = args.NameValuePairs.XFGetValue("P_Entity", String.Empty)
				'Get the scenario parameter
				Dim SelectedScenario As String = args.NameValuePairs.XFGetValue("P_Scenario", String.Empty)
				'Get the account parameter
				Dim SelectedAccount As String = args.NameValuePairs.XFGetValue("P_Account", String.Empty)
				
				If (SelectedEntity.Equals("Horse_Group")) Then
					ConsoMember = "Local"
				Else
					ConsoMember = "Top"	
				End If
				
				'Get the number of characters (5 -> country, 8 -> Base member)
				Dim LenName As Integer = Len(SelectedEntity)
				
				If (LenName = 5) Then 'country entity selected
					ParentMember = "Horse_Group"
				Else If (LenName = 8) 'base entity selected
					ParentMember = "Horse_Flat"
					
				End If
					
				If (ParentMember.Equals("")) Then
					parametersReport = "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":S#" & SelectedScenario & ":C#" & ConsoMember
				Else
					parametersReport = "A#" & SelectedAccount & ":E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario & ":C#" & ConsoMember
				End If
				
				
				'BRApi.ErrorLog.LogMessage(si," --------------- " &parametersReport)
				
				Return parametersReport

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetConsolidationAndParentMemberGroupDashboardVariance (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
			
				Dim parametersReport As String = String.Empty
				Dim ConsoMember As String = String.Empty
				Dim ParentMember As String = String.Empty
				
				'Get the entity parameter
				Dim SelectedEntity As String = args.NameValuePairs.XFGetValue("P_Entity", String.Empty)
				'Get the scenario 1 parameter
				Dim SelectedScenario_1 As String = args.NameValuePairs.XFGetValue("P_Scenario_1", String.Empty)
				'Get the scenario 2 parameter
				Dim SelectedScenario_2 As String = args.NameValuePairs.XFGetValue("P_Scenario_2", String.Empty)
				'Get the account parameter
				Dim SelectedAccount As String = args.NameValuePairs.XFGetValue("P_Account", String.Empty)
				
				If (SelectedEntity.Equals("Horse_Group")) Then
					ConsoMember = "Local"
				Else
					ConsoMember = "Top"	
				End If
				
				'Get the number of characters (5 -> country, 8 -> Base member)
				Dim LenName As Integer = Len(SelectedEntity)
				
				If (LenName = 5) Then 'country entity selected
					ParentMember = "Horse_Group"
				Else If (LenName = 8) 'base entity selected
					ParentMember = "Horse_Flat"
					
				End If
				
				' The parametersReport should have the text for the GetDataCell formula to calculate scenario variance (Scenario 1 - Scenario 2)
				If (ParentMember.Equals("")) Then
					parametersReport = "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":S#" & SelectedScenario_1 & ":C#" & ConsoMember & " - " & "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":S#" & SelectedScenario_2 & ":C#" & ConsoMember
				Else
					parametersReport = "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario_1 & ":C#" & ConsoMember & " - " & "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario_2 & ":C#" & ConsoMember
				End If
				
				'BRApi.ErrorLog.LogMessage(si," --------------- " &parametersReport)
				
				Return parametersReport

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetConsolidationAndParentMemberCountryDashboard (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
			
				Dim parametersReport As String = String.Empty
				Dim ConsoMember As String = String.Empty
				Dim ParentMember As String = String.Empty
				
				'Get the entity parameter
				Dim SelectedEntity As String = args.NameValuePairs.XFGetValue("P_Entity", String.Empty)
				'Get the scenario parameter
				Dim SelectedScenario As String = args.NameValuePairs.XFGetValue("P_Scenario", String.Empty)
				'Get the account parameter
				Dim SelectedAccount As String = args.NameValuePairs.XFGetValue("P_Account", String.Empty)
				
				'Get the number of characters (5 -> country, 8 -> Base member)
				Dim LenName As Integer = Len(SelectedEntity)
				
				If (LenName = 5) Then 'country entity selected
					ConsoMember = "Local"
				Else If (LenName = 8) 'base entity selected
					ParentMember = Left(SelectedEntity,5)
					ConsoMember = "Top"	
					
				End If
				
				If (ParentMember.Equals("")) Then
					parametersReport = "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":S#" & SelectedScenario & ":C#" & ConsoMember
				Else
					parametersReport = "A#" & SelectedAccount & ":E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario & ":C#" & ConsoMember
				End If
				
				
				'BRApi.ErrorLog.LogMessage(si," --------------- " &parametersReport)
				
				Return parametersReport

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Public Function GetConsolidationAndParentMemberCountryDashboardVariance (ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
			
				Dim parametersReport As String = String.Empty
				Dim ConsoMember As String = String.Empty
				Dim ParentMember As String = String.Empty
				
				'Get the entity parameter
				Dim SelectedEntity As String = args.NameValuePairs.XFGetValue("P_Entity", String.Empty)
				'Get the scenario 1 parameter
				Dim SelectedScenario_1 As String = args.NameValuePairs.XFGetValue("P_Scenario_1", String.Empty)
				'Get the scenario 2 parameter
				Dim SelectedScenario_2 As String = args.NameValuePairs.XFGetValue("P_Scenario_2", String.Empty)
				'Get the account parameter
				Dim SelectedAccount As String = args.NameValuePairs.XFGetValue("P_Account", String.Empty)
				
				'Get the number of characters (5 -> country, 8 -> Base member)
				Dim LenName As Integer = Len(SelectedEntity)
				
				If (LenName = 5) Then 'country entity selected
					ConsoMember = "Local"
				Else If (LenName = 8) 'base entity selected
					ParentMember = Left(SelectedEntity,5)
					ConsoMember = "Top"	
					
				End If
'				brapi.ErrorLog.LogMessage(si,ParentMember)
				
				' The parametersReport should have the text for the GetDataCell formula to calculate scenario variance (Scenario 1 - Scenario 2)
				If (ParentMember.Equals("")) Then
					parametersReport = "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":S#" & SelectedScenario_1 & ":C#" & ConsoMember & " - " & "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":S#" & SelectedScenario_2 & ":C#" & ConsoMember
				Else
					parametersReport = "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario_1 & ":C#" & ConsoMember & " - " & "A#" & SelectedAccount  & ":E#" & SelectedEntity & ":P#" & ParentMember & ":S#" & SelectedScenario_2 & ":C#" & ConsoMember
				End If
				
				'BRApi.ErrorLog.LogMessage(si," --------------- " &parametersReport)
				
				Return parametersReport

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
	End Class
End Namespace