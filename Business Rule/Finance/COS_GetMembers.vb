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

Namespace OneStream.BusinessRule.Finance.COS_GetMembers
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Select Case api.FunctionType					
						
					Case Is = FinanceFunctionType.MemberList

						If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("EntitiesComparables") Then
							
							Dim oMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim sBrand As String = args.MemberListArgs.NameValuePairs("p_brand")
							Dim sYear As String = args.MemberListArgs.NameValuePairs("p_year")
							Dim oEntDimPK As DimPk = api.Dimensions.GetDim("Entities").DimPk
							Dim oEntMemberInfos As List(Of MemberInfo) = api.Members.GetMembersUsingFilter(oEntDimPK, $"E#{sBrand}.Base", Nothing)
							Dim oICMemberInfos As New List(Of MemberInfo)
							
							Dim iTime As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, sYear)
							Dim iTimePrevYear As Integer = BRApi.Finance.Members.GetMemberId(si,DimType.Time.Id, (CInt(sYear) - 1).ToString)
							
							Dim miEntityComp As MemberInfo
							For Each miEntity As MemberInfo In oEntMemberInfos
								Dim sText1 As String = BRApi.Finance.Entity.Text(si, miEntity.Member.MemberId,2,0,iTime)
								Dim sText1PrevYear As String = BRApi.Finance.Entity.Text(si, miEntity.Member.MemberId,2,0,iTimePrevYear)
								
'								If (miEntity.Member.Name.Equals("F001605")) Then
'									BRApi.ErrorLog.LogMessage(si,"sYear: " & sYear)
'									BRApi.ErrorLog.LogMessage(si,"iTime: " & iTime.ToString)
'									BRApi.ErrorLog.LogMessage(si,"sText1: " & sText1)
'									BRApi.ErrorLog.LogMessage(si,"iTimePrevYear: " & iTimePrevYear.ToString)
'									BRApi.ErrorLog.LogMessage(si,"sText1PrevYear: " & sText1PrevYear)
'								End If								
								
								If(sText1.Equals("Comparables") _
								Or sText1.Equals("Reformas") _
								Or sText1.Equals("Reformas Año Anterior")) Then	
								
									miEntityComp = New MemberInfo(api.Members.GetMember(dimtype.Entity.Id, miEntity.Member.MemberId))
									oICMemberInfos.Add(miEntityComp)
									'BRApi.ErrorLog.LogMessage(si,"Entity: " & miEntityComp.Member.Name)
								
								ElseIf(sText1.Equals("Cerradas") _
								And Not sText1PrevYear.Equals("Cerradas")) Then	
								
									miEntityComp = New MemberInfo(api.Members.GetMember(dimtype.Entity.Id, miEntity.Member.MemberId))
									oICMemberInfos.Add(miEntityComp)
									'BRApi.ErrorLog.LogMessage(si,"Entity: " & miEntityComp.Member.Name)								
								End If
								
							Next
							
							Dim oMemberList As New MemberList(oMemberListHeader, oICMemberInfos)
							Return oMemberList
							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("ForecastMonths") Then
							
							Dim sScenario As String = args.MemberListArgs.NameValuePairs("p_scenario")														
							Dim oMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim sYear As String = args.MemberListArgs.NameValuePairs("p_year")
							Dim iForecastMonth As Integer = CInt(args.MemberListArgs.NameValuePairs("p_forecast_month"))
							If (sScenario = "Actual" Or sScenario = "Budget") Then
								iForecastMonth = 1
							End If
							
							Dim miTime As New List(Of MemberInfo)
							
							Dim i As Integer    
							For i = iForecastMonth To 12
								miTime.Add(BRApi.Finance.Members.GetMemberInfo(si,dimtype.Time.Id,sYear & "M" & i.ToString))
							Next i							
							
							Dim oMemberList As New MemberList(oMemberListHeader, miTime)
							Return oMemberList				
							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("ForecastMonthsClosed") Then
							
							Dim sScenario As String = args.MemberListArgs.NameValuePairs("p_scenario")														
							Dim oMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							Dim sYear As String = args.MemberListArgs.NameValuePairs("p_year")
							Dim iForecastMonth As Integer = CInt(args.MemberListArgs.NameValuePairs("p_forecast_month"))
							If (sScenario = "Budget") Then
								iForecastMonth = 0
							End If																					
							
							Dim miTime As New List(Of MemberInfo)
							
							Dim i As Integer   
							Dim y As Integer = iForecastMonth
							For i = 1 To y-1
								miTime.Add(BRApi.Finance.Members.GetMemberInfo(si,dimtype.Time.Id,sYear & "M" & i.ToString))
							Next i							
							
							Dim oMemberList As New MemberList(oMemberListHeader, miTime)
							Return oMemberList	
							
						Else If args.MemberListArgs.MemberListName.XFEqualsIgnoreCase("ReferenceScenario") Then
							
							' Declare valid scenarios
							Dim validScenarios As New List(Of String) From {"forecast", "budget"}
							
							' Get parameters
							Dim sScenario As String = args.MemberListArgs.NameValuePairs("p_scenario")
							Dim oMemberListHeader As New MemberListHeader(args.MemberListArgs.MemberListName)
							
							' Validate Input
							If Not validScenarios.Contains(sScenario.ToLower) Then Throw New Exception($"error getting reference scenario: {sScenario} is not a valid scenario.")
							Dim refScenarioName As String = If(sScenario.ToLower = "forecast", "Actual", "Forecast")
							
							' Add scenario
							Dim scenarioMemberList As New List(Of MemberInfo)
							scenarioMemberList.Add(BRApi.Finance.Members.GetMemberInfo(si,dimtype.Scenario.Id, refScenarioName))
							
							Dim oMemberList As New MemberList(oMemberListHeader, scenarioMemberList)
							Return oMemberList
							
						End If							
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace