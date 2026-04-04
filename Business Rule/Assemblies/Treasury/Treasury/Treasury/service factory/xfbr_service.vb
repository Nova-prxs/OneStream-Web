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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class xfbr_service
		Implements IWsasXFBRStringV800

        Public Function GetXFBRString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
			ByVal args As DashboardStringFunctionArgs) As String Implements IWsasXFBRStringV800.GetXFBRString
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
                    If args.FunctionName.XFEqualsIgnoreCase("GetCompanyName") Then
                        ' Get the company ID parameter and clean it
                        Dim companyId As String = args.NameValuePairs("p_company")
                        If Not String.IsNullOrEmpty(companyId) Then
                            companyId = companyId.Trim()
                        End If
                        
                        ' Use helper function to get company name from database
                        Return Global.Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, companyId)
                    End If
                    
                    If args.FunctionName.XFEqualsIgnoreCase("GetTreasuryCompanyName") Then
                        ' Get the company parameter from the NameValuePairs (passed from dashboard)
                        Dim companyParam As String = args.NameValuePairs("p_company")
                        
                        If Not String.IsNullOrEmpty(companyParam) Then
                            companyParam = companyParam.Trim()
                        End If
                        
                        ' Check if empty or HTD (global/no filter) - return "Global"
                        If String.IsNullOrEmpty(companyParam) OrElse _
                           companyParam.XFEqualsIgnoreCase("HTD") OrElse _
                           companyParam = "0" Then
                            Return "Global"
                        End If
                        
                        ' Use helper function to get company name from database
                        Return Global.Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetCompanyNameFromId(si, companyParam)
                    End If
                    
                    If args.FunctionName.XFEqualsIgnoreCase("GetWeekNumber") Then
                        Dim weekParam As String = args.NameValuePairs("prm_Treasury_WeekNumber")
                        If Not String.IsNullOrEmpty(weekParam) Then
                            Return weekParam.Trim()
                        End If
                        Return ""
                    End If
                    
                    If args.FunctionName.XFEqualsIgnoreCase("GetMonthNumber") Then
                        Dim yearParam As String = args.NameValuePairs("prm_Treasury_Year")
                        Dim weekParam As String = args.NameValuePairs("prm_Treasury_WeekNumber")
                        
                        If Not String.IsNullOrEmpty(yearParam) AndAlso Not String.IsNullOrEmpty(weekParam) Then
                            Dim year As Integer
                            Dim week As Integer
                            
                            If Integer.TryParse(yearParam, year) AndAlso Integer.TryParse(weekParam, week) Then
                                Dim monthNum As Integer = Global.Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions.GetMonthFromWeek(si, year.ToString(), week)
                                Return monthNum.ToString()
                            End If
                        End If
                        Return ""
                    End If
                    
                    ' =====================================================================================
                    ' GetReportWeek: Returns a formatted string "Week {weekNumber} {year}"
                    ' Usage: XFBR(WSMU, GetReportWeek, prm_Treasury_Year=|!prm_Treasury_Year!|,prm_Treasury_WeekNumber=|!prm_Treasury_WeekNumber!|)
                    ' Returns: "Week 49 2025"
                    ' =====================================================================================
                    If args.FunctionName.XFEqualsIgnoreCase("GetReportWeek") Then
                        Dim yearParam As String = args.NameValuePairs("prm_Treasury_Year")
                        Dim weekParam As String = args.NameValuePairs("prm_Treasury_WeekNumber")
                        
                        ' Clean parameters
                        If Not String.IsNullOrEmpty(yearParam) Then yearParam = yearParam.Trim()
                        If Not String.IsNullOrEmpty(weekParam) Then weekParam = weekParam.Trim()
                        
                        Return $"Week {weekParam} {yearParam}"
                    End If
                    
                    ' =====================================================================================
                    ' GetReportParam: Returns a formatted string for report comments header
                    ' Usage: XFBR(WSMU, GetReportParam, prm_Treasury_Year=[|!prm_Treasury_Year!|],prm_Treasury_WeekNumber=[|!prm_Treasury_WeekNumber!|],prm_Treasury_scenario=[|!prm_Treasury_Scenario!|])
                    ' Returns: "comments from the {scenario} for the {week} week of the {year} year"
                    ' =====================================================================================
                    If args.FunctionName.XFEqualsIgnoreCase("GetReportParam") Then
                        Dim yearParam As String = args.NameValuePairs("prm_Treasury_Year")
                        Dim weekParam As String = args.NameValuePairs("prm_Treasury_WeekNumber")
                        Dim scenarioParam As String = args.NameValuePairs("prm_Treasury_scenario")
                        
                        ' Clean parameters
                        If Not String.IsNullOrEmpty(yearParam) Then yearParam = yearParam.Trim()
                        If Not String.IsNullOrEmpty(weekParam) Then weekParam = weekParam.Trim()
                        If Not String.IsNullOrEmpty(scenarioParam) Then scenarioParam = scenarioParam.Trim()
                        
                        ' Map scenario codes to readable names
                        Dim scenarioDisplay As String = scenarioParam
                        If Not String.IsNullOrEmpty(scenarioParam) Then
                            Select Case scenarioParam.ToUpperInvariant()
                                Case "EOM"
                                    scenarioDisplay = "End Of Month"
                                Case "EOW"
                                    scenarioDisplay = "End of Week"
                                Case "STARTWEEK"
                                    scenarioDisplay = "Start Week"
                                Case Else
                                    scenarioDisplay = scenarioParam
                            End Select
                        End If
                        
                        ' Build the result string
                        Return $"Comments from the {scenarioDisplay} for the {weekParam} week of the {yearParam}"
                    End If
	            End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace