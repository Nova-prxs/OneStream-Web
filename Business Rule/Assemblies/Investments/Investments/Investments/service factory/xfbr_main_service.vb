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
	Public Class xfbr_main_service
		Implements IWsasXFBRStringV800
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions

        Public Function GetXFBRString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
			ByVal args As DashboardStringFunctionArgs) As String Implements IWsasXFBRStringV800.GetXFBRString
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
					Select args.FunctionName
					Case "GetEntityMemberFromCompanyId"
                        Return HelperFunctionsBR.GetBaseEntity(si, args.NameValuePairs("p_company"))
					Case "GetParentMemberFromCompanyId"
                        Return HelperFunctionsBR.GetParentEntity(si, args.NameValuePairs("p_company"))
					Case "GetTimeFilterForYearAndScenario"
						Dim paramForecastMonth As String = HelperFunctionsBR.GetForecastMonth(si, args.NameValuePairs("p_scenario"))
						Return HelperFunctionsBR.GetTimeFilterFromForecastMonth(si, CInt(args.NameValuePairs("p_year")), paramForecastMonth)
					Case "GetDepreciationAccounts"
						' Get depreciation accounts
						Dim dt As New DataTable()
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(
								dbConn,
								"
									SELECT DISTINCT depreciation_account AS account
									FROM XFC_INV_MASTER_cost_center_type WITH(NOLOCK)
								",
								False
							)
						End Using
						
						If dt Is Nothing OrElse dt.Rows.Count = 0 Then Return ""
						
						' Generate string with all the accounts
						Dim accountsString As String = String.Empty
						For Each row In dt.Rows
							accountsString += If(String.IsNullOrEmpty(accountsString), row("account"), $",{row("account")}")
						Next
						Return accountsString
						
						Case "GetConditionalPage"
						    Dim scenarioName As String = args.NameValuePairs("prm_Investments_Scenario")
						    
						    If scenarioName.ToUpper().StartsWith("RF") Then
						        Return "Investments_3_6_7"
						    ElseIf scenarioName.ToUpper().StartsWith("BUD") Then
						        Return "Investments_4_6_7"
						    Else
						        Return "Investments_3_6_7"
						    End If
						
                	End Select
	            End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace
