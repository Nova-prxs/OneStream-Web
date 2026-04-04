Imports System
Imports System.Collections.Generic
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic
Imports OneStream.Finance.Database
Imports OneStream.Finance.Engine
Imports OneStream.Shared.Common
Imports OneStream.Shared.Database
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Wcf
Imports OneStream.Stage.Database
Imports OneStream.Stage.Engine
Imports Workspace.RESTAPIFramework.RESTAPIRequestManager.Classes
Imports Newtonsoft.Json.Linq

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_rest_api
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		' Reference "RESTAPIFramework_SharedFunctions", "RESTAPIRequestManager" and "extender_import_data"
		Dim RESTAPISharedFunctionsBR As New OneStream.BusinessRule.Finance.RESTAPIFramework_SharedFunctions.MainClass
		Dim RESTAPIRequestManagerBR As New Workspace.RESTAPIFramework.RESTAPIRequestManager.Utils()
		Dim ExtenderImportDataBR As New Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_data.MainClass()
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						' Get function name
						Dim paramFunction As String = args.NameValuePairs.XFGetValue("p_function")
						If String.IsNullOrEmpty(paramFunction) Then Throw New Exception("'p_function' must be provided")
							
						Select Case paramFunction
							
						#Region "Download File"
						
						Case Is = "DownloadFile"
							
							' Get parameters
							Dim paramEndpoint As String = args.NameValuePairs.XFGetValue("p_endpoint")
							If String.IsNullOrEmpty(paramEndpoint) Then Throw New Exception("'p_endpoint' must be provided")
							Dim paramFolder As String = args.NameValuePairs.XFGetValue("p_folder")
							If String.IsNullOrEmpty(paramFolder) Then Throw New Exception("'p_folder' must be provided")
							Dim paramFileName As String = args.NameValuePairs.XFGetValue("p_file_name")
							If String.IsNullOrEmpty(paramFileName) Then Throw New Exception("'p_file_name' must be provided")
								
							' Get authentication endpoint
							Dim paramAuthenticationEndpoint As String = args.NameValuePairs.XFGetValue("p_authentication_endpoint")
							If String.IsNullOrEmpty(paramAuthenticationEndpoint) Then Throw New Exception("'p_authentication_endpoint' must be provided")
							
							' Collect authentication, execute and get token
							Dim authenticationRequest As RESTAPIRequest = RESTAPIRequestManagerBR.GetSavedRequest(si, paramAuthenticationEndpoint)
							Dim authenticationResult As HttpResult = authenticationRequest.Execute()
							Dim authenticationTokenJToken As JToken = Nothing
							If Not JObject.Parse(authenticationResult.Content).TryGetValue("access_token", authenticationTokenJToken) Then _
								Throw New Exception($"Access token not found in the request '{paramAuthenticationEndpoint}': {authenticationResult.Content}")
							
							Dim authenticationToken As String = authenticationTokenJToken.ToString()
							
							' Collect request data, modify authentication and get the file URL
							Dim fileURLRequest As RESTAPIRequest = RESTAPIRequestManagerBR.GetSavedRequest(si, paramEndpoint)
							fileURLRequest.authCredentials = New BearerTokenAuthCredentials(authenticationToken)
							Dim fileURLResult As HttpResult = fileURLRequest.Execute()
							Dim fileURLJToken As JToken = Nothing
							If Not JObject.Parse(fileURLResult.Content).TryGetValue("payload", fileURLJToken) Then _
								Throw New Exception($"URL not found in the request '{paramEndpoint}': {fileURLResult.Content}")
								
							Dim fileURL As String = fileURLJToken.ToString()
							
							' Download and save the file
							Dim webClient As New System.Net.WebClient()
							Dim fileBytes As Byte() = webClient.DownloadData(fileURL)
							
							Dim dbFileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, paramFileName, paramFolder, XFFileType.Unknown)
							dbFileInfo.ContentFileContainsData = True
							dbFileInfo.ContentFileExtension = dbFileInfo.Extension
							Dim dbFile As New XFFile(dbFileInfo, String.Empty, fileBytes)
							BRApi.FileSystem.InsertOrUpdateFile(si, dbFile)
							
						#End Region
						
						#Region "Import Data"
							
						Case Is = "ImportData"
							
							' Get parameters
							Dim paramEndpoint As String = args.NameValuePairs.XFGetValue("p_endpoint")
							If String.IsNullOrEmpty(paramEndpoint) Then Throw New Exception("'p_endpoint' must be provided")
							Dim paramImportTableName As String = args.NameValuePairs.XFGetValue("p_import_table_name")
							If String.IsNullOrEmpty(paramImportTableName) Then Throw New Exception("'p_import_table_name' must be provided")
							Dim customParameters As String = args.NameValuePairs.XFGetValue("p_custom_params")
							Dim parameterDict As New Dictionary(Of String, String)
							If customParameters.StartsWith("|!") Then customParameters = String.Empty
							If Not String.IsNullOrEmpty(customParameters) Then _
								parameterDict = UTISharedFunctionsBR.SplitQueryParams(si, customParameters)
							If parameterDict.ContainsKey("month") Then parameterDict("month") = parameterDict("month").PadLeft(3, "0"c)
								
							'-------------------------------------------------- TEMPORAL --------------------------------------------------------------------
							If parameterDict.ContainsKey("company_code") AndAlso parameterDict("company_code") <> "PT10" AndAlso parameterDict("company_code") <> "ES10" Then _
								Throw New Exception("Company code must be 'PT10'")
								
							'-------------------------------------------------- CUSTOM AUTHENTICATION -------------------------------------------------------
'							' Get authentication endpoint
'							Dim paramAuthenticationEndpoint As String = args.NameValuePairs.XFGetValue("p_authentication_endpoint")
'							If String.IsNullOrEmpty(paramAuthenticationEndpoint) Then Throw New Exception("'p_authentication_endpoint' must be provided")
							
'							' Collect authentication, execute and get token
'							Dim authenticationRequest As RESTAPIRequest = RESTAPIRequestManagerBR.GetSavedRequest(si, paramAuthenticationEndpoint)
'							Dim authenticationResult As HttpResult = authenticationRequest.Execute()
'							Dim authenticationTokenJToken As JToken = Nothing
'							If Not JObject.Parse(authenticationResult.Content).TryGetValue("access_token", authenticationTokenJToken) Then _
'								Throw New Exception($"Access token not found in the request '{paramAuthenticationEndpoint}': {authenticationResult.Content}")
							
'							Dim authenticationToken As String = authenticationTokenJToken.ToString()
							
'							' Execute populate table
'							Dim dataRequest As RESTAPIRequest = RESTAPIRequestManagerBR.GetSavedRequest(si, paramEndpoint)
'							dataRequest.authCredentials = New BearerTokenAuthCredentials(authenticationToken)
'							dataRequest.PopulateTable(si)
							'--------------------------------------------------------------------------------------------------------------------------------
							
							' Execute populate table
							Dim dataRequest As RESTAPIRequest = RESTAPIRequestManagerBR.GetSavedRequest(si, paramEndpoint)
							dataRequest.PopulateTable(si, parameterDict)
							
							' Run population query
							If paramImportTableName.StartsWith("|") OrElse String.IsNullOrEmpty(paramImportTableName) Then _
								paramImportTableName = dataRequest.TableToPopulate
							ExtenderImportDataBR.RunPopulationQueries(si, paramImportTableName, "REST API", paramEndpoint)
							
						#End Region
						
						Case Else
							Throw New Exception($"The function name '{paramFunction}' does not exist")
						End Select
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace
