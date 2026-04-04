Imports System
Imports System.Net
Imports System.Collections.Generic
Imports System.Collections.Specialized
Imports System.Data
Imports System.Data.Common
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Web
Imports System.Runtime.Serialization.Json
Imports Newtonsoft.Json.Linq
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
Imports Workspace.__WsNamespacePrefix.__WsAssemblyName.Classes

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class Utils
		
		#Region "Execute Request"

        Public Function ExecuteRequest(
	        method As String,
	        endpoint As String,
	        urlParams As Dictionary(Of String, String),
	        headers As Dictionary(Of String, String),
	        bodyType As String,
	        body As Object,
			variables As Dictionary(Of String, String),
			Optional authCredentials As AuthCredentials = Nothing,
			Optional modifiedUrlParams As Dictionary(Of String, String) = Nothing) As HttpResult
			
			' Process variables in all components
	        Dim processedEndpoint = ReplaceVariables(endpoint, variables, True)
			Dim processedParams = MergeDictionaries(urlParams, modifiedUrlParams)
	        processedParams = ProcessDictionaryWithVariables(urlParams, variables, False)
	        Dim processedHeaders = ProcessDictionaryWithVariables(headers, variables, False)
	        Dim processedBody = ProcessBodyVariables(body, variables)
			
			' Apply authentication if provided
		    If authCredentials IsNot Nothing Then
		        authCredentials.ApplyAuthCredentials(processedHeaders, processedParams, variables)
		    End If
	        
	        Dim fullUrl = BuildUrl(processedEndpoint, processedParams)
			
	        Dim request = CreateWebRequest(method, fullUrl, processedHeaders)
	        
	        If bodyType <> "none" Then
	            ProcessRequestBody(request, bodyType, processedBody)
	        End If
	
	        Return GetResponse(request)
	    End Function
		
		#Region "Execute Request Functions"
		
		#Region "Main Execute Request Functions"
	
	    Private Function CreateWebRequest(method As String, url As String, headers As Dictionary(Of String, String)) As HttpWebRequest
	        Dim request = CType(WebRequest.Create(url), HttpWebRequest)
	        request.Method = method
	        request.Timeout = 30000
	        request.AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
	
	        ' Add headers
	        For Each header In headers
	            Select Case header.Key.ToLower()
	                Case "content-type"
	                    request.ContentType = header.Value
	                Case "accept"
	                    request.Accept = header.Value
	                Case "user-agent"
	                    request.UserAgent = header.Value
	                Case Else
	                    request.Headers.Add(header.Key, header.Value)
	            End Select
	        Next
	
	        Return request
	    End Function
	
		Private Sub ProcessRequestBody(request As HttpWebRequest, bodyType As String, body As Object)
		    If body Is Nothing Then Return
		
		    Dim content As String = String.Empty
		    Dim boundary As String = String.Empty
		    Dim bytes As Byte()
		
		    Select Case bodyType.ToLower()
		        Case "text/plain"
		            content = DirectCast(body, String)
		            request.ContentType = If(String.IsNullOrEmpty(request.ContentType), "text/plain", request.ContentType)
		
		        Case "application/x-www-form-urlencoded"
		            Dim formData = DirectCast(body, Dictionary(Of String, String))
		            content = String.Join("&", formData.Select(
		                Function(kvp) $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"))
		            request.ContentType = If(String.IsNullOrEmpty(request.ContentType), 
		                "application/x-www-form-urlencoded", request.ContentType)
		
		        Case "multipart/form-data"
		            boundary = $"----WebKitFormBoundary{Guid.NewGuid().ToString("N")}"
		            request.ContentType = $"multipart/form-data; boundary={boundary}"
		            bytes = ProcessMultipartContent(body, boundary)
		
		        Case "application/json"
		            request.ContentType = If(String.IsNullOrEmpty(request.ContentType), 
		                "application/json", request.ContentType)
		            content = ProcessJsonBody(body)
		            bytes = Encoding.UTF8.GetBytes(content)
		
		        Case Else
		            Throw New ArgumentException($"Unsupported body type: {bodyType}")
		    End Select
		
		    ' Only set bytes if not multipart (already processed)
		    If bodyType.ToLower() <> "multipart/form-data" Then
		        bytes = Encoding.UTF8.GetBytes(content)
		    End If
		
		    request.ContentLength = bytes.Length
		
		    Using stream = request.GetRequestStream()
		        stream.Write(bytes, 0, bytes.Length)
		    End Using
		End Sub
	
	    Private Function GetResponse(request As HttpWebRequest) As HttpResult
	        Try
	            Using response = CType(request.GetResponse(), HttpWebResponse)
	                Using reader = New StreamReader(response.GetResponseStream())
	                    Return New HttpResult With {
	                        .StatusCode = response.StatusCode,
	                        .Content = reader.ReadToEnd(),
	                        .Headers = response.Headers
	                    }
	                End Using
	            End Using
	        Catch ex As WebException
	            If ex.Response IsNot Nothing Then
	                Using response = CType(ex.Response, HttpWebResponse)
	                    Using reader = New StreamReader(response.GetResponseStream())
	                        Return New HttpResult With {
	                            .StatusCode = response.StatusCode,
	                            .Content = reader.ReadToEnd(),
	                            .Headers = response.Headers,
	                            .Err = ex.Message
	                        }
	                    End Using
	                End Using
	            End If
	            Return New HttpResult With {
	                .StatusCode = HttpStatusCode.BadRequest,
	                .Err = ex.Message
	            }
	        End Try
	    End Function
		
		#End Region
		
		#Region "Helper Execute Request Functions"
		
		#Region "Applying Variables"
		
		Public Function MergeDictionaries(dict1 As Dictionary(Of String, String), dict2 As Dictionary(Of String, String)) As Dictionary(Of String, String)
			If dict2 Is Nothing Then Return dict1
				
			For Each dict2kvp In dict2
				dict1(dict2kvp.Key) = dict2kvp.Value
			Next
			
			Return dict1
		End Function
		
		Public Function ProcessDictionaryWithVariables(original As Dictionary(Of String, String), variables As Dictionary(Of String, String), escape As Boolean) As Dictionary(Of String, String)
	        If original Is Nothing Then Return New Dictionary(Of String, String)
	        Return original.ToDictionary(
	            Function(kvp) kvp.Key,
	            Function(kvp) ReplaceVariables(kvp.Value, variables, escape)
	        )
	    End Function
	
	    Public Function ProcessBodyVariables(body As Object, variables As Dictionary(Of String, String)) As Object
	        If body Is Nothing Then Return Nothing
	
	        Select Case True
			    Case TypeOf body Is String
			        Return ReplaceVariables(DirectCast(body, String), variables, False)
			    Case TypeOf body Is IDictionary
			        Return ProcessDictionaryWithVariablesBody(DirectCast(body, Dictionary(Of String, String)), variables)
			    Case TypeOf body Is IEnumerable  ' Only for non-dictionary collections
			        Return ProcessListBody(DirectCast(body, IEnumerable), variables)
			    Case Else
			        Return ReplaceVariables(body.ToString(), variables, False)
			End Select
	    End Function
	
	    Public Function ProcessDictionaryWithVariablesBody(dict As IDictionary, variables As Dictionary(Of String, String)) As Dictionary(Of String, String)
	        Dim processed = New Dictionary(Of String, String)
	        For Each key In dict.Keys
	            processed(key.ToString()) = ProcessBodyVariables(dict(key), variables)
	        Next
	        Return processed
	    End Function
	
	    Public Function ProcessListBody(list As IEnumerable, variables As Dictionary(Of String, String)) As IList
	        Dim processed = New List(Of Object)()
	        For Each item In list
	            processed.Add(ProcessBodyVariables(item, variables))
	        Next
	        Return processed
	    End Function
	
	    Public Function ReplaceVariables(input As String, variables As Dictionary(Of String, String), escape As Boolean) As String
		    If String.IsNullOrEmpty(input) Then Return input
		    If variables Is Nothing OrElse variables.Count = 0 Then Return input
			
		    Return variables.Aggregate(input,
				Function(current, variable) current.Replace(
				$"{{{{{variable.Key}}}}}",
				If(escape, Uri.EscapeDataString(variable.Value), variable.Value)))
		End Function
	
	    Private Function BuildUrl(endpoint As String, params As Dictionary(Of String, String)) As String
			' Parse the endpoint
		    Dim uriBuilder As New UriBuilder(endpoint)
		
		    ' Parse existing parameters
		    Dim query = HttpUtility.ParseQueryString(uriBuilder.Query)
		    
		    ' Extract and temporarily store the $filter parameter if present
		    Dim rawFilter As String = String.Empty
		    If params.ContainsKey("$filter") Then
		        rawFilter = params("$filter")
		        params.Remove("$filter")
		    End If
		
		    ' Add new parameters, letting HttpUtility handle encoding
		    For Each param In params
		        query(param.Key) = param.Value
		    Next
		
		    ' Apply encoded query back to URI
		    uriBuilder.Query = query.ToString()
		
		    ' Manually append unencoded $filter if it exists
		    Dim finalUri As String
		    If Not String.IsNullOrEmpty(rawFilter) Then
		        If uriBuilder.Query.Length > 0 Then
		            finalUri = uriBuilder.Uri.AbsoluteUri & "&$filter=" & rawFilter
		        Else
		            finalUri = uriBuilder.Uri.AbsoluteUri & "?$filter=" & rawFilter
		        End If
		    Else
		        finalUri = uriBuilder.Uri.AbsoluteUri
		    End If
		
		    Return finalUri
	    End Function
		
		#End Region
		
		#Region "Process Body"
		
		Private Function ProcessMultipartContent(body As Object, boundary As String) As Byte()
		    Dim formData = TryCast(body, Dictionary(Of String, String))
		    If formData Is Nothing Then
		        Throw New ArgumentException("Multipart body must be a Dictionary(Of String, Object)")
		    End If
		
		    Using ms As New MemoryStream()
		        Dim encoding As Encoding = Encoding.UTF8
		        Dim newLine As String = vbCrLf
		
		        For Each kvp In formData
		            Dim partHeader = $"--{boundary}{newLine}" &
		                            $"Content-Disposition: form-data; name=""{kvp.Key}"""
	                ' Regular form field
	                partHeader &= $"{newLine}{newLine}"
	                Dim stringValue = kvp.Value.ToString()
	                Dim partData = $"{partHeader}{stringValue}{newLine}"
	                ms.Write(encoding.GetBytes(partData), 0, encoding.GetByteCount(partData))
		        Next
		
		        ' Add final boundary
		        Dim footer = $"{newLine}--{boundary}--{newLine}"
		        ms.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer))
		
		        Return ms.ToArray()
		    End Using
		End Function
		
		Private Function ProcessJsonBody(body As Object) As String
		    If TypeOf body Is String Then
		        ' Validate JSON format
		        Try
		            Dim parsed = JObject.Parse(DirectCast(body, String))
		            Return body.ToString()
		        Catch ex As Exception
		            Throw New ArgumentException("Invalid JSON string", ex)
		        End Try
		    ElseIf TypeOf body Is IDictionary Then
		        ' Serialize dictionary to JSON
		        Using ms As New MemoryStream()
		            Dim serializer = New DataContractJsonSerializer(body.GetType())
		            serializer.WriteObject(ms, body)
		            Return Encoding.UTF8.GetString(ms.ToArray())
		        End Using
		    Else
		        Throw New ArgumentException("JSON body must be a string or dictionary")
		    End If
		End Function
	
	    Private Function SerializeJson(data As Dictionary(Of String, Object)) As String
	        Using ms = New MemoryStream()
	            Dim serializer = New DataContractJsonSerializer(GetType(Dictionary(Of String, Object)))
	            serializer.WriteObject(ms, data)
	            Return Encoding.UTF8.GetString(ms.ToArray())
	        End Using
	    End Function
		
		#End Region
		
		#End Region
		
		#End Region
		
		#End Region
		
		#Region "Execute Saved Request"

        Public Function ExecuteSavedRequest(si As SessionInfo, requestName As String) As HttpResult
            Try
				Return Me.GetSavedRequest(si, requestName).Execute()
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Get Saved Request"
		
		Public Function GetSavedRequest(si As SessionInfo, requestName As String) As RESTAPIRequest
			Dim savedRequest As New RESTAPIRequest
			
			'Retrieve saved request info and set it to the saved request
			Dim savedRequestInfo As Dictionary(Of String, String) = Me.GetSavedRequestInfo(si, requestName)
			savedRequest.id = savedRequestInfo("id")
			savedRequest.name = savedRequestInfo("name")
			savedRequest.method = savedRequestInfo("method")
			savedRequest.endpoint = savedRequestInfo("endpoint")
			savedRequest.bodyType = savedRequestInfo("body_type")
			savedRequest.tableToPopulate = savedRequestInfo("table_to_populate")
			savedRequest.nextLinkKey = savedRequestInfo("next_link_key")
			savedRequest.dataKey = savedRequestInfo("data_key")
			savedRequest.topParamName = savedRequestInfo("top_param_name")
			savedRequest.skipParamName = savedRequestInfo("skip_param_name")
			savedRequest.dataLimit = savedRequestInfo("data_limit")
			
			'Retrieve saved request url params, headers, variables, auth credentials and column mappings; and set then to the saved request
			savedRequest.urlParams = Me.GetSavedRequestParams(si, requestName)
			savedRequest.headers = Me.GetSavedRequestHeaders(si, requestName)
			savedRequest.variables = Me.GetSavedRequestVariables(si, requestName)
			savedRequest.authCredentials = Me.GetSavedRequestAuthCredentials(si, requestName)
			savedRequest.columnMappings = Me.GetSavedRequestColumnMappings(si, requestName)
			
			'Retrieved saved request body
			savedRequest.body = Me.GetSavedRequestBody(si, requestName, savedRequest.bodyType)
			
			Return savedRequest
		End Function
		
		#Region "Helpers"
		
		Public Function GetSavedRequestInfo(si As SessionInfo, requestName As String) As Dictionary(Of String, String)
			Dim savedRequestInfo As New Dictionary(Of String, String)
			
			'Get dt with all the information
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim requestDt As New DataTable()
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramRequestName", requestName)
				}
				requestDt = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT id, name, method, COALESCE(endpoint, '') AS endpoint, body_type, table_to_populate, next_link_key, data_key,
							COALESCE(top_param_name, '') AS top_param_name, COALESCE(skip_param_name, '') AS skip_param_name,
							COALESCE(data_limit, 0) AS data_limit
						FROM XFC_REST_MASTER_request WITH(NOLOCK)
						WHERE name = @paramRequestName
					",
					dbParamInfos,
					False
				)
				'Check if request exists
				If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
					Throw New XFException("No saved request with name '" & requestName & "'.")
				End If
				'Set info
				savedRequestInfo("id") = requestDt.Rows(0)("id")
				savedRequestInfo("name") = requestDt.Rows(0)("name")
				savedRequestInfo("method") = requestDt.Rows(0)("method")
				savedRequestInfo("endpoint") = requestDt.Rows(0)("endpoint")
				savedRequestInfo("body_type") = requestDt.Rows(0)("body_type")
				savedRequestInfo("table_to_populate") = requestDt.Rows(0)("table_to_populate")
				savedRequestInfo("next_link_key") = requestDt.Rows(0)("next_link_key")
				savedRequestInfo("data_key") = requestDt.Rows(0)("data_key")
				savedRequestInfo("top_param_name") = requestDt.Rows(0)("top_param_name")
				savedRequestInfo("skip_param_name") = requestDt.Rows(0)("skip_param_name")
				savedRequestInfo("data_limit") = requestDt.Rows(0)("data_limit")
			End Using
			
			Return savedRequestInfo
		End Function
		
		Public Function GetSavedRequestParams(si As SessionInfo, requestName As String) As Dictionary(Of String, String)
			Dim savedRequestParams As New Dictionary(Of String, String)
			
			'Get dt with all the information
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim requestDt As New DataTable()
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramRequestName", requestName)
				}
				requestDt = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT param_key, param_value
						FROM XFC_REST_MASTER_request_param rp WITH(NOLOCK)
						INNER JOIN XFC_REST_MASTER_request r WITH(NOLOCK)
						ON rp.request_id = r.id
						WHERE name = @paramRequestName
							AND in_use = 1
					",
					dbParamInfos,
					False
				)
				'Check if request exists
				If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
					Return savedRequestParams
				End If
				'Set params
				For Each row In requestDt.Rows
					savedRequestParams(row("param_key")) = row("param_value")
				Next
			End Using
			
			Return savedRequestParams
		End Function
		
		Public Function GetSavedRequestHeaders(si As SessionInfo, requestName As String) As Dictionary(Of String, String)
			Dim savedRequestHeaders As New Dictionary(Of String, String)
			
			'Get dt with all the information
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim requestDt As New DataTable()
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramRequestName", requestName)
				}
				requestDt = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT header_key, header_value
						FROM XFC_REST_MASTER_request_header rh WITH(NOLOCK)
						INNER JOIN XFC_REST_MASTER_request r WITH(NOLOCK)
						ON rh.request_id = r.id
						WHERE name = @paramRequestName
							AND in_use = 1
					",
					dbParamInfos,
					False
				)
				'Check if request exists
				If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
					Return savedRequestHeaders
				End If
				'Set params
				For Each row In requestDt.Rows
					savedRequestHeaders(row("header_key")) = row("header_value")
				Next
			End Using
			
			Return savedRequestHeaders
		End Function
		
		Public Function GetSavedRequestVariables(si As SessionInfo, requestName As String) As Dictionary(Of String, String)
			Dim savedRequestVariables As New Dictionary(Of String, String)
			
			'Get dt with all the information
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim requestDt As New DataTable()
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramRequestName", requestName)
				}
				requestDt = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT request_id, variable_key, variable_value
						FROM XFC_REST_MASTER_request_variable rv WITH(NOLOCK)
						LEFT JOIN XFC_REST_MASTER_request r WITH(NOLOCK)
						ON rv.request_id = r.id
						WHERE (
								r.name = @paramRequestName
								OR rv.request_id = 0
							) AND in_use = 1
						ORDER BY request_id ASC
					",
					dbParamInfos,
					False
				)
				'Check if request exists
				If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
					Return savedRequestVariables
				End If
				'Set params
				For Each row In requestDt.Rows
					savedRequestVariables(row("variable_key")) = row("variable_value")
				Next
			End Using
			
			Return savedRequestVariables
		End Function
		
		Public Function GetSavedRequestBody(si As SessionInfo, requestName As String, bodyType As String) As Object
			'Get dt with all the information
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim requestDt As New DataTable()
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramRequestName", requestName)
				}
				'Control if body type is key value pairs or plain text
				If bodyType = "application/x-www-form-urlencoded" OrElse bodyType = "multipart/form-data"
					Dim savedRequestBodyDictionary As New Dictionary(Of String, String)
					requestDt = BRApi.Database.ExecuteSql(
						dbConn,
						"
							SELECT body_key, body_value
							FROM XFC_REST_MASTER_request_body_kvp rb WITH(NOLOCK)
							INNER JOIN XFC_REST_MASTER_request r WITH(NOLOCK)
							ON rb.request_id = r.id
							WHERE name = @paramRequestName
								AND in_use = 1
						",
						dbParamInfos,
						False
					)
					'Check if request exists
					If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
						Return savedRequestBodyDictionary
					End If
					'Set params
					For Each row In requestDt.Rows
						savedRequestBodyDictionary(row("body_key")) = row("body_value")
					Next
					Return savedRequestBodyDictionary
				Else
					requestDt = BRApi.Database.ExecuteSql(
						dbConn,
						"
							SELECT COALESCE(body, '') AS body
							FROM XFC_REST_MASTER_request_body_text rb WITH(NOLOCK)
							INNER JOIN XFC_REST_MASTER_request r WITH(NOLOCK)
							ON rb.request_id = r.id
							WHERE name = @paramRequestName
						",
						dbParamInfos,
						False
					)
					'Check if request exists
					If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
						Return String.Empty
					End If
					Return requestDt.Rows(0)("body").ToString
				End If
			End Using
		End Function
		
		Public Function GetSavedRequestAuthCredentials(si As SessionInfo, requestName As String) As AuthCredentials
			'Get dt with all the information
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim requestDt As New DataTable()
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramRequestName", requestName)
				}
				requestDt = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT r.auth_type AS auth_type, ra.property AS property, ra.value AS value
						FROM XFC_REST_MASTER_request_auth ra WITH(NOLOCK)
						RIGHT JOIN XFC_REST_MASTER_request r WITH(NOLOCK)
						ON ra.request_id = r.id
						WHERE name = @paramRequestName
					",
					dbParamInfos,
					False
				)
				'Check if request exists
				If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
					Throw New Exception($"The request doesn't exist.")
				End If
				'Get auth type and create a dictionary with properties
				Dim authTypeString As String = requestDt.Rows(0)("auth_type")
				Dim propertyDictionary As New Dictionary(Of String, String)
				For Each row In requestDt.Rows
					propertyDictionary(row("property").ToString) = row("value").ToString
				Next
				'Return AuthCredentials instance based on auth type controlling that properties are included
				Select Case authTypeString
					Case "none"
						Return Nothing
					Case "basic"
						If Not propertyDictionary.ContainsKey("username") OrElse Not propertyDictionary.ContainsKey("password") Then _
							Throw New Exception($"No api username or password found for request '{requestName}'.")
						Return New BasicAuthCredentials(propertyDictionary("username"), propertyDictionary("password"))
					Case "api_key"
						If Not propertyDictionary.ContainsKey("api_key") OrElse Not propertyDictionary.ContainsKey("key_location") Then _
							Throw New Exception($"No api key found for request '{requestName}'.")
						Return New ApiKeyAuthCredentials(propertyDictionary("api_key"), propertyDictionary("key_location"))
					Case "bearer_token"
						If Not propertyDictionary.ContainsKey("bearer_token") Then _
							Throw New Exception($"No bearer token found for request '{requestName}'.")
						Return New BearerTokenAuthCredentials(propertyDictionary("bearer_token"))
					Case "oauth"
						If Not propertyDictionary.ContainsKey(OAuthCredentials.CLIENT_ID) OrElse _
							Not propertyDictionary.ContainsKey(OAuthCredentials.CLIENT_SECRET) OrElse _
							Not propertyDictionary.ContainsKey(OAuthCredentials.TOKEN_URL) OrElse _
							Not propertyDictionary.ContainsKey("scope") OrElse _
							Not propertyDictionary.ContainsKey(OAuthCredentials.GRANT_TYPE) Then _
								Throw New Exception($"No OAuth properties set for request '{requestName}'.")
				        Dim credentials As New OAuthCredentials(
				            ClientId:=propertyDictionary(OAuthCredentials.CLIENT_ID),
				            ClientSecret:=propertyDictionary(OAuthCredentials.CLIENT_SECRET),
				            TokenUrl:=propertyDictionary(OAuthCredentials.TOKEN_URL),
							Scope:=propertyDictionary("scope"),
							GrantType:=GetGrantTypeFromDictionary(propertyDictionary),
							Parameters:=propertyDictionary
				        )
						Return credentials
				End Select
					Throw New Exception($"Authorization type not supported.")
			End Using
		End Function
		
		' Function to map dictionary value to GrantTypes enum
	    Private Function GetGrantTypeFromDictionary(parameters As Dictionary(Of String, String)) As OAuthCredentials.GrantTypes
	        If parameters.ContainsKey(OAuthCredentials.GRANT_TYPE) Then
	            Select Case parameters(OAuthCredentials.GRANT_TYPE).ToLower()
	                Case "client_credentials"
	                    Return OAuthCredentials.GrantTypes.ClientCredentials
	                Case "password"
	                    Return OAuthCredentials.GrantTypes.Password
	                Case "authorization_code"
	                    Return OAuthCredentials.GrantTypes.AuthorizationCode
	                Case "refresh_token"
	                    Return OAuthCredentials.GrantTypes.RefreshToken
	                Case "device_code"
	                    Return OAuthCredentials.GrantTypes.DeviceCode
	                Case Else
	                    Return OAuthCredentials.GrantTypes.Custom ' Default for unknown grant type
	            End Select
	        End If
	
	        Throw New KeyNotFoundException("The 'grant_type' key is missing in the dictionary.")
	    End Function
		
		Public Function GetSavedRequestColumnMappings(si As SessionInfo, requestName As String) As Dictionary(Of String, String)
			Dim savedRequestColumnMappings As New Dictionary(Of String, String)
			
			'Get dt with all the information
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim requestDt As New DataTable()
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramRequestName", requestName)
				}
				requestDt = BRApi.Database.ExecuteSql(
					dbConn,
					"
						SELECT source_column, target_column
						FROM XFC_REST_MASTER_request_column_mapping rcm WITH(NOLOCK)
						INNER JOIN XFC_REST_MASTER_request r WITH(NOLOCK)
						ON rcm.request_id = r.id
						WHERE name = @paramRequestName
					",
					dbParamInfos,
					False
				)
				'Check if request exists
				If requestDt Is Nothing OrElse requestDt.Rows.Count = 0 Then
					Return Nothing
				End If
				'Set params
				For Each row In requestDt.Rows
					savedRequestColumnMappings(row("source_column")) = row("target_column")
				Next
			End Using
			
			Return savedRequestColumnMappings
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
