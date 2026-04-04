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
Imports Newtonsoft.Json
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
Imports Workspace.__WsNamespacePrefix.__WsAssemblyName.Utils

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class Classes
		
		#Region "HttpResult"
	
		Public Class HttpResult
		    Public Property StatusCode As HttpStatusCode
		    Public Property Content As String
		    Public Property Headers As WebHeaderCollection
		    Public Property Err As String
				
			' Helper method to get all headers as "Key: Value"
		    Public Function GetFormattedHeaders() As String
		        If Headers Is Nothing OrElse Headers.Count = 0 Then Return String.Empty
		
		        Dim formattedHeaders As New StringBuilder()
		
		        For Each key As String In Headers.AllKeys
		            formattedHeaders.AppendLine($"{key}: {Headers(key)}")
		        Next
		
		        Return formattedHeaders.ToString().TrimEnd()
		    End Function
				
			' Helper method to get status code as an integer
		    Public Function GetStatusCode() As Integer
		        Return CType(StatusCode, Integer)
		    End Function
		End Class
		
		#End Region
		
		#Region "Auth"
		
		Public MustInherit Class AuthCredentials
		    Public MustOverride Sub ApplyAuthCredentials(ByRef headers As Dictionary(Of String, String), 
		                                     ByRef urlParams As Dictionary(Of String, String), ByRef variables As Dictionary(Of String, String))
		End Class
		
		Public Class BasicAuthCredentials
		    Inherits AuthCredentials
		    Public Property Username As String
		    Public Property Password As String
		    
		    Public Sub New(username As String, password As String)
		        Me.Username = username
		        Me.Password = password
		    End Sub
		
		    Public Overrides Sub ApplyAuthCredentials(ByRef headers As Dictionary(Of String, String), 
		                                  ByRef urlParams As Dictionary(Of String, String), ByRef variables As Dictionary(Of String, String))
				' Combine the username and password into a single string separated by a colon (e.g., "username:password").
		        Dim credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Username}:{Password}"))
				' Add the Base64-encoded credentials to the Authorization header using the "Basic" scheme.
		        headers("Authorization") = New Utils().ReplaceVariables($"Basic {credentials}", variables, False)
		    End Sub
		End Class
		
		Public Class ApiKeyAuthCredentials
		    Inherits AuthCredentials
		
		    Public Property Key As String
		    Public Property Location As String
		    Public Property HeaderName As String = "X-API-Key"
		    Public Property ParamName As String = "api_key"
		
		    Public Sub New(key As String, Optional location As String = "header")
		        Me.Key = key
		        Me.Location = location
		    End Sub
		
		    Public Overrides Sub ApplyAuthCredentials(ByRef headers As Dictionary(Of String, String), 
		                                  ByRef urlParams As Dictionary(Of String, String), ByRef variables As Dictionary(Of String, String))
		        Select Case Location
		            Case "header"
		                headers(HeaderName) = New Utils().ReplaceVariables(Key, variables, False)
		            Case "query_param"
		                urlParams(ParamName) = New Utils().ReplaceVariables(Key, variables, False)
		        End Select
		    End Sub
		End Class
		
		Public Class BearerTokenAuthCredentials
		    Inherits AuthCredentials
		    Public Property Token As String
		
		    Public Sub New(token As String)
		        Me.Token = token
		    End Sub
		
		    Public Overrides Sub ApplyAuthCredentials(ByRef headers As Dictionary(Of String, String), 
		                                  ByRef urlParams As Dictionary(Of String, String), ByRef variables As Dictionary(Of String, String))
		        headers("Authorization") = New Utils().ReplaceVariables($"Bearer {Token}", variables, False)
		    End Sub
		End Class
		
		Public Class OAuthCredentials
		    Inherits AuthCredentials
		    Public Enum GrantTypes
		        ClientCredentials
		        Password
		        AuthorizationCode
		        RefreshToken
		        DeviceCode
		        Custom
		    End Enum
		
		    ' Core OAuth properties
		    Public Property AccessToken As String
		    Public Property RefreshToken As String
		    Public Property IdToken As String
		    Public Property ClientId As String
		    Public Property ClientSecret As String
		    Public Property TokenUrl As String
		    Public Property Scope As String
		    Public Property ExpiresAt As DateTime?
		    Public Property GrantType As GrantTypes
		    Public Property Parameters As Dictionary(Of String, String)
		    
		    ' Constants for standard OAuth parameters
		    Public Const GRANT_TYPE = "grant_type"
		    Public Const CLIENT_ID = "client_id"
		    Public Const CLIENT_SECRET = "client_secret"
		    Public Const TOKEN_URL = "token_url"
			Public Const REFRESH_TOKEN = "refresh_token"
		    Public Const USERNAME = "username"
		    Public Const PASSWORD = "password"
		    Public Const CODE = "code"
		    Public Const REDIRECT_URI = "redirect_uri"
		    Public Const DEVICE_CODE = "device_code"
		
		    Public Sub New(clientId As String, 
		                  clientSecret As String, 
		                  tokenUrl As String,
						  scope As String,
		                  Optional grantType As GrantTypes = GrantTypes.ClientCredentials,
		                  Optional parameters As Dictionary(Of String, String) = Nothing)
		        Me.ClientId = clientId
		        Me.ClientSecret = clientSecret
		        Me.TokenUrl = tokenUrl
		        Me.Scope = scope
		        Me.GrantType = grantType
		        Me.Parameters = If(parameters, New Dictionary(Of String, String))
		    End Sub
		
		    Public Overrides Sub ApplyAuthCredentials(ByRef headers As Dictionary(Of String, String), 
		                                  ByRef urlParams As Dictionary(Of String, String), ByRef variables As Dictionary(Of String, String))
		        If String.IsNullOrEmpty(AccessToken) OrElse TokenExpired Then
		            RefreshAccessToken(variables)
		        End If
		        headers("Authorization") = $"Bearer {AccessToken}"
		    End Sub
		
		    Private ReadOnly Property TokenExpired As Boolean
		        Get
		            Return ExpiresAt.HasValue AndAlso ExpiresAt.Value <= DateTime.UtcNow.AddSeconds(-30) ' 30s buffer
		        End Get
		    End Property
		
		    Public Sub RefreshAccessToken(ByRef variables As Dictionary(Of String, String))
		        Dim requestBody As Dictionary(Of String, String) = BuildTokenRequestBody()
		        Dim authHeader = GetClientAuthHeader()
		
		        Dim headers = New Dictionary(Of String, String)
		
		        If Not String.IsNullOrEmpty(authHeader) Then
		            headers("Authorization") = authHeader
		        End If
		
		        Dim result = New Utils().ExecuteRequest(
		            "POST", 
		            TokenUrl, 
		            Nothing,
		            headers,
		            "application/x-www-form-urlencoded", 
		            requestBody, 
		            variables
		        )
		        HandleTokenResponse(result)
		    End Sub
		
		    Private Function BuildTokenRequestBody() As Dictionary(Of String, String)
		        Dim body = New Dictionary(Of String, String) From {
		            {GRANT_TYPE, GetGrantTypeString()},
		            {CLIENT_ID, ClientId},
					{"scope", Scope}
		        }
		
		        ' Add parameters based on grant type
		        Select Case GrantType
		            Case GrantTypes.ClientCredentials
		                body.Add(CLIENT_SECRET, ClientSecret)
		                
		            Case GrantTypes.Password
		                body.Add(USERNAME, Parameters(USERNAME))
		                body.Add(PASSWORD, Parameters(PASSWORD))
		                If Parameters.ContainsKey(CLIENT_SECRET) Then body.Add(CLIENT_SECRET, ClientSecret)
		                
		            Case GrantTypes.AuthorizationCode
		                body.Add(CODE, Parameters(CODE))
		                body.Add(REDIRECT_URI, Parameters(REDIRECT_URI))
		                If Parameters.ContainsKey(CLIENT_SECRET) Then body.Add(CLIENT_SECRET, ClientSecret)
		                
		            Case GrantTypes.RefreshToken
		                body.Add(REFRESH_TOKEN, RefreshToken)
		                
		            Case GrantTypes.DeviceCode
		                body.Add(DEVICE_CODE, Parameters(DEVICE_CODE))
		                
		            Case GrantTypes.Custom
		                For Each param In Parameters
		                    body.Add(param.Key, param.Value)
		                Next
		        End Select
		
		        ' Add additional parameters
		        For Each param In Parameters
		            If Not body.ContainsKey(param.Key) Then
		                body.Add(param.Key, param.Value)
		            End If
		        Next
		
		        Return body
		    End Function
		
		    Private Function GetGrantTypeString() As String
		        Return GrantType.ToString().ToLowerInvariant().Replace("code", "_code") _
					.Replace("credentials", "_credentials") _
					.Replace("token", "_token")
		    End Function
		
		    Private Function GetClientAuthHeader() As String
		        ' Support for client secret in header (RFC 6749 Section 2.3.1)
		        If Parameters.ContainsKey("auth_method") AndAlso Parameters("auth_method") = "header" Then
		            Dim credentials = Convert.ToBase64String(
		                Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"))
		            Return $"Basic {credentials}"
		        End If
		        Return String.Empty
		    End Function
		
		    Private Sub HandleTokenResponse(result As HttpResult)
		        If result.StatusCode = HttpStatusCode.OK Then
		            Dim tokenResponse = JObject.Parse(result.Content)
		            
		            AccessToken = tokenResponse.Value(Of String)("access_token")
		            RefreshToken = tokenResponse.Value(Of String)("refresh_token")
		            IdToken = tokenResponse.Value(Of String)("id_token")
		            
		            If tokenResponse("expires_in") IsNot Nothing Then
		                ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.Value(Of Integer)("expires_in"))
		            End If
		            
		            If String.IsNullOrEmpty(AccessToken) Then
		                Throw New InvalidOperationException("No access token in response")
		            End If
		        Else
		            Dim errorMessage = $"Token request failed: {result.Content}"
		            Throw New InvalidOperationException(errorMessage)
		        End If
		    End Sub
		
		    Public Function BuildAuthorizationUrl(authorizationEndpoint As String,
		                                         redirectUri As String,
		                                         Optional scopes As String = "",
		                                         Optional state As String = "") As String
		        Dim queryParams = New Dictionary(Of String, String) From {
		            {"response_type", "code"},
		            {"client_id", ClientId},
		            {"redirect_uri", redirectUri}
		        }
		
		        If Not String.IsNullOrEmpty(scopes) Then queryParams.Add("scope", scopes)
		        If Not String.IsNullOrEmpty(state) Then queryParams.Add("state", state)
		        
		        Return $"{authorizationEndpoint}?{String.Join("&", queryParams.Select(Function(kvp) $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"))}"
		    End Function
		End Class
		
		#End Region
		
		#Region "REST API Request"
		
		Public Class RESTAPIRequest
		    Public Property id As Integer
		    Public Property name As String
		    Public Property method As String
			Public Property endpoint As String
			Public Property urlParams As Dictionary(Of String, String)
			Public Property headers As Dictionary(Of String, String)
			Public Property bodyType As String
			Public Property body As Object
			Public Property tableToPopulate As String
			Public Property columnMappings As Dictionary(Of String, String)
			Public Property nextLinkKey As String
			Public Property topParamName As String
			Public Property skipParamName As String
			Public Property dataLimit As Integer
			Public Property dataKey As String
			Public Property variables As Dictionary(Of String, String)
			Public Property authCredentials As AuthCredentials
			
			'Function to execute the request
			Public Function Execute(Optional paramVariables As Dictionary(Of String, String) = Nothing) As HttpResult
				' Update variables based on custom variables
				variables = Me.UpdateVariables(variables, paramVariables)
				Return New Utils().ExecuteRequest(
					method, endpoint, urlParams, headers, bodyType, body, variables, authCredentials
				)
			End Function
			
			'Function to get a datatable variable with all the requests data
			Public Function GetDataTable(Optional limit As Integer = 0, Optional paramVariables As Dictionary(Of String, String) = Nothing) As DataTable
			    ' Initialize a DataTable to store the results
			    Dim dt As New DataTable()
			    Dim nextLink As String = Nothing
				' Update variables based on custom variables
				variables = Me.UpdateVariables(variables, paramVariables)
				' Update urlParams based on pagination configuration
				Dim manualPagination As Boolean = False
				Dim skipValue As Integer = 0
				If Not (Not String.IsNullOrEmpty(nextLinkKey) OrElse String.IsNullOrEmpty(topParamName) OrElse String.IsNullOrEmpty(skipParamName) OrElse dataLimit = 0) Then
					manualPagination = True
					urlParams(topParamName) = dataLimit.ToString()
					urlParams(skipParamName) = skipValue.ToString()
				End If
			
				' Initialize loop count variable
				Dim loopCount As Integer = 0
				Dim wasDataLoaded As Boolean = False
			    Do
					wasDataLoaded = False
			        ' Execute the HTTP request
			        Dim response As HttpResult = New Utils().ExecuteRequest(
			            method, If(nextLink, endpoint), urlParams, headers, bodyType, body, variables, authCredentials
			        )
			
			        ' Check if the response status code indicates success
			        If response.StatusCode <> HttpStatusCode.OK Then
			            Throw New Exception($"The response status was not successful. Status: {response.StatusCode}")
			        End If
			
			        ' Parse the response content as a JSON object
			        Dim jsonResponse As JObject = JObject.Parse(response.Content)
			
			        ' Extract data using the dataKey (colon-separated string)
			        Dim currentData As JToken = jsonResponse
			        For Each key In dataKey.Split("->")
			            currentData = currentData(key)
			            If currentData Is Nothing Then Exit For
			        Next
			
			        ' If data is found, append it to the DataTable
					If currentData IsNot Nothing Then
					    Dim dataArray As JArray
					    
					    ' Handle both arrays and single objects
					    If currentData.Type = JTokenType.Array Then
					        dataArray = CType(currentData, JArray)
					    Else
					        ' Wrap single object in an array
					        dataArray = New JArray()
					        dataArray.Add(currentData)
					    End If
						
						' Filter data keys
						Dim filteredArray As New JArray()
						For Each item As JObject In dataArray
						    Dim filteredObject As New JObject()
						    For Each key As String In columnMappings.Keys
								If key.Contains("->") Then
									Dim keyChain As String() = key.Split("->")
							        If Not item.ContainsKey(keyChain(0)) Then Continue For
									Dim currentItem As JToken = item
									For Each subKey In keyChain
					    				If currentItem.Type = JTokenType.Array Then currentItem = currentItem(0)
							            currentItem = currentItem(subKey)
							            If currentItem Is Nothing Then Exit For
									Next
							        filteredObject(key) = currentItem
								Else
							        If item.ContainsKey(key) Then
							            filteredObject(key) = item(key)
							        End If
								End If
						    Next
						    filteredArray.Add(filteredObject)
						Next
					
					    ' Convert JSON to DataTable
					    Using reader As New StringReader(filteredArray.ToString())
					        Using jsonReader As New JsonTextReader(reader)
								'Create a temporal dt of the single response and import rows to the single dt
					            Dim tempDt As DataTable = JsonConvert.DeserializeObject(Of DataTable)(filteredArray.ToString())
					            ' Initialize DataTable structure if empty
					            If dt.Columns.Count = 0 Then
					                dt = tempDt.Clone()
					            End If
					            
					            ' Import all rows
					            For Each row As DataRow In tempDt.Rows
									wasDataLoaded = True
					                dt.ImportRow(row)
					            Next
					        End Using
					    End Using
					End If
			
			        ' Update next link
					If manualPagination Then
						skipValue += dataLimit
						urlParams(skipParamName) = skipValue.ToString()
					Else
						nextLink = Me.BuildNextLink(jsonResponse)
					End If
			
					' Update loop count
					loopCount += 1
			    Loop While limit <> loopCount AndAlso ((manualPagination AndAlso wasDataLoaded) OrElse Not String.IsNullOrEmpty(nextLink))
				
				' Update columns
				Me.ModifyDataTableColumns(dt, columnMappings)
			
			    Return dt
			End Function
			
			'Function to get a datatable variable with all the requests data
			Public Sub PopulateTable(si As SessionInfo, Optional paramVariables As Dictionary(Of String, String) = Nothing)
				' Check if table is not empty
				If String.IsNullOrEmpty(tableToPopulate) OrElse tableToPopulate = "none" _
					Then Throw New Exception($"Request '{name}' has no table to populate.")
				' Update variables based on custom variables
				variables = Me.UpdateVariables(variables, paramVariables)
				' Update urlParams based on pagination configuration
				Dim manualPagination As Boolean = False
				Dim skipValue As Integer = 0
				If Not (Not String.IsNullOrEmpty(nextLinkKey) OrElse String.IsNullOrEmpty(topParamName) OrElse String.IsNullOrEmpty(skipParamName) OrElse dataLimit = 0) Then
					manualPagination = True
					urlParams(topParamName) = dataLimit.ToString()
					urlParams(skipParamName) = skipValue.ToString()
				End If
					
			    ' Initialize a nextlink to make consecutive requests and uti shared functions
			    Dim nextLink As String = Nothing
				Dim UTISharedFunctions = New OneStream.BusinessRule.Finance.RESTAPIFramework_SharedFunctions.MainClass()
			
				' Initialize method to replace at first request
				Dim replace As Boolean = True
				Dim wasDataLoaded As Boolean = False
			    Do
					wasDataLoaded = False
			        ' Execute the HTTP request
			        Dim response As HttpResult = New Utils().ExecuteRequest(
			            method, If(nextLink, endpoint), urlParams, headers, bodyType, body, variables, authCredentials
			        )
			
			        ' Check if the response status code indicates success
			        If response.StatusCode <> HttpStatusCode.OK Then
			            Throw New Exception($"The response status was not successful. Status: {response.StatusCode}")
			        End If
			
			        ' Parse the response content as a JSON object
			        Dim jsonResponse As JObject = JObject.Parse(response.Content)
			
			        ' Extract data using the dataKey (arrow separated string)
			        Dim currentData As JToken = jsonResponse
			        For Each key In dataKey.Split("->")
			            currentData = currentData(key)
			            If currentData Is Nothing Then Exit For
			        Next
			
			        ' If data is found, append it to the DataTable
					If currentData IsNot Nothing Then
					    Dim dataArray As JArray
					    
					    ' Handle both arrays and single objects
					    If currentData.Type = JTokenType.Array Then
					        dataArray = CType(currentData, JArray)
					    Else
					        ' Wrap single object in an array
					        dataArray = New JArray()
					        dataArray.Add(currentData)
					    End If
						
						' Filter data keys
						Dim filteredArray As New JArray()
						For Each item As JObject In dataArray
						    Dim filteredObject As New JObject()
						    For Each key As String In columnMappings.Keys
								If key.Contains("->") Then
									Dim keyChain As String() = key.Split("->")
							        If Not item.ContainsKey(keyChain(0)) Then Continue For
									Dim currentItem As JToken = item
									For Each subKey In keyChain
					    				If currentItem.Type = JTokenType.Array Then currentItem = currentItem(0)
							            currentItem = currentItem(subKey)
							            If currentItem Is Nothing Then Exit For
									Next
							        filteredObject(key) = currentItem
								Else
							        If item.ContainsKey(key) Then
							            filteredObject(key) = item(key)
							        End If
								End If
						    Next
						    filteredArray.Add(filteredObject)
						Next
					
					    ' Convert JSON to DataTable
					    Using reader As New StringReader(filteredArray.ToString())
					        Using jsonReader As New JsonTextReader(reader)
					            Dim tempDt As DataTable = JsonConvert.DeserializeObject(Of DataTable)(filteredArray.ToString())
								If tempDt.Rows.Count > 0 Then
									wasDataLoaded = True
							        'Update columns
									Me.ModifyDataTableColumns(tempDt, columnMappings)
						            
									'Load data table to custom table
									UTISharedFunctions.LoadDataTableToCustomTable(si, tableToPopulate, tempDt, If(replace, "replace", "merge"))
								End If
					        End Using
					    End Using
					End If
			
			        ' Update next link
					If manualPagination Then
						skipValue += dataLimit
						urlParams(skipParamName) = skipValue.ToString()
					Else
						nextLink = Me.BuildNextLink(jsonResponse)
					End If
					
					' Change replace method and was data loaded for subsequent requests
					replace = False
			    Loop While (manualPagination AndAlso wasDataLoaded) OrElse Not String.IsNullOrEmpty(nextLink)
			
			End Sub
			
			#Region "Helper Functions"
			
			Private Function BuildNextLink(jsonResponse As JToken) As String
				If String.IsNullOrEmpty(nextLinkKey) Then Return Nothing
				
				' Get the next link using nextLinkKey (colon-separated string)
		        Dim nextLinkToken As JToken = jsonResponse
		        For Each key In nextLinkKey.Split("->")
		            nextLinkToken = nextLinkToken(key)
		            If nextLinkToken Is Nothing Then Exit For
		        Next
		
		        ' Update the next link for pagination or exit if no more links are found
		        Return If(nextLinkToken?.ToString(), Nothing)
			End Function
			
			Private Sub ModifyDataTableColumns(ByRef dt As DataTable, columnMappings As Dictionary(Of String, String))
			    ' Check if columnMappings is Nothing
			    If columnMappings Is Nothing OrElse columnMappings.Count = 0 Then Exit Sub
			
			    ' Create a list to store columns to be removed
			    Dim columnsToRemove As New List(Of String)()
			
			    ' Iterate through the existing columns in the DataTable
			    For Each col As DataColumn In dt.Columns
			        If columnMappings.ContainsKey(col.ColumnName) Then
			            ' Rename the column based on the mapping
			            col.ColumnName = columnMappings(col.ColumnName)
			        Else
			            ' Add column name to the removal list if not in the dictionary
			            columnsToRemove.Add(col.ColumnName)
			        End If
			    Next
			
			    ' Remove columns that are not in the dictionary
			    For Each colName In columnsToRemove
			        dt.Columns.Remove(colName)
			    Next
			End Sub
			
			Private Function UpdateVariables(variables As Dictionary(Of String, String), paramVariables As Dictionary(Of String, String)) As Dictionary(Of String, String)
				If paramVariables Is Nothing Then Return variables
				
				For Each paramVariable In paramVariables
					variables(paramVariable.Key) = paramVariable.Value
				Next
					
				Return variables
			End Function
			
			#End Region
			
		End Class
		
		#End Region
		
	End Class
End Namespace
