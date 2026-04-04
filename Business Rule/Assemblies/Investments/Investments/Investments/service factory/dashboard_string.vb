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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardStringFunction.dashboard_string
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				Select args.FunctionName
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
				Case "GetEntityMemberFromCompanyId"
                    Return HelperFunctionsBR.GetBaseEntity(si, args.NameValuePairs("p_company"))
				Case "GetParentMemberFromCompanyId"
                    Return HelperFunctionsBR.GetParentEntity(si, args.NameValuePairs("p_company"))
				Case "GetTimeFilterForYearAndScenario"
					Dim paramForecastMonth As String = HelperFunctionsBR.GetForecastMonth(si, args.NameValuePairs("p_scenario"))
					Return HelperFunctionsBR.GetTimeFilterFromForecastMonth(si, CInt(args.NameValuePairs("p_year")), paramForecastMonth)
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
