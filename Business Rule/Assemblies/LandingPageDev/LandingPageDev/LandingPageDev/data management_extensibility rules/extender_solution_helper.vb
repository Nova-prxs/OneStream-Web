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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_solution_helper
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		'Reference "helper_functions" Business Rule
		Dim HelperFunctionsBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.helper_functions
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get function name
						Dim functionName As String = args.NameValuePairs("p_function")
						
						'Control function name
						
						#Region "Send Data To Cube Init"
						
						If functionName = "SendDataToCubeInit" Then
							'Get parameters
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							
							'Check if user has selected a company
							If parameterDict("paramCompany") = "0" Then Throw New XFException("You must select a company.")
							
							'Check if scenario is confirmed
							If Not HelperFunctionsBR.CheckScenarioYearConfirmation(
								si, parameterDict("paramCompany"), parameterDict("paramScenario"), parameterDict("paramYear")
							) Then
								Throw New XFException("You must confirm Scenario and Year.")
							End If
							
							'Define custom substitution variables for data management
							Dim customSubstVars As New Dictionary(Of String, String)
							customSubstVars("p_entity") = HelperFunctionsBR.GetBaseEntity(si, parameterDict("paramCompany"))
							customSubstVars("p_parent_entity") = HelperFunctionsBR.GetParentEntity(si, parameterDict("paramCompany"))
							customSubstVars("p_scenario") = parameterDict("paramScenario")
							customSubstVars("p_year") = parameterDict("paramYear")
							customSubstVars("p_company") = parameterDict("paramCompany")
							customSubstVars("p_time_filter") = HelperFunctionsBR.GetTimeFilterFromForecastMonth(si, CInt(parameterDict("paramYear")), paramForecastMonth)
							
							'Execute "Send Data to Cube"
							BRApi.Utilities.ExecuteDataMgmtSequence(
								si,
								BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "LandingPageDev"),
								"LandingPageDev_SendDataToCube",
								customSubstVars
							)
							
						#End Region
						
						#Region "Clear Sent Data Init"
						
						Else If functionName = "ClearSentDataInit" Then
							'Get parameters
							Dim queryParams As String = args.NameValuePairs("p_query_params")
							Dim parameterDict As Dictionary(Of String, String) = UTISharedFunctionsBR.SplitQueryParams(si, queryParams)
							Dim dbParamInfos As List(Of DbParamInfo) = UTISharedFunctionsBR.CreateQueryParams(si, queryParams)
							Dim paramForecastMonth As Integer = HelperFunctionsBR.GetForecastMonth(si, parameterDict("paramScenario"))
							
							'Check if user has selected a company
							If parameterDict("paramCompany") = "0" Then Throw New XFException("You must select a company.")
							
							'Define custom substitution variables for data management
							Dim customSubstVars As New Dictionary(Of String, String)
							customSubstVars("p_entity") = HelperFunctionsBR.GetBaseEntity(si, parameterDict("paramCompany"))
							customSubstVars("p_parent_entity") = HelperFunctionsBR.GetParentEntity(si, parameterDict("paramCompany"))
							customSubstVars("p_scenario") = parameterDict("paramScenario")
							customSubstVars("p_year") = parameterDict("paramYear")
							customSubstVars("p_company") = parameterDict("paramCompany")
							customSubstVars("p_time_filter") = HelperFunctionsBR.GetTimeFilterFromForecastMonth(si, CInt(parameterDict("paramYear")), paramForecastMonth)
							
							'Execute "Send Data to Cube"
							BRApi.Utilities.ExecuteDataMgmtSequence(
								si,
								BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "LandingPageDev"),
								"LandingPageDev_ClearSentData",
								customSubstVars
							)
						
						End If
						
						#End Region
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
