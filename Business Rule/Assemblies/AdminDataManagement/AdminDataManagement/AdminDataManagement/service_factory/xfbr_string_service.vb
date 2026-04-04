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
	Public Class xfbr_string_service
		Implements IWsasXFBRStringV800

        Public Function GetXFBRString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal workspace As DashboardWorkspace, _
			ByVal args As DashboardStringFunctionArgs) As String Implements IWsasXFBRStringV800.GetXFBRString
            Try
                If (globals IsNot Nothing) AndAlso (workspace IsNot Nothing) AndAlso (args IsNot Nothing) Then
                    If args.FunctionName.XFEqualsIgnoreCase("GetCompanyCodeByEntityId") Then
						
						' Get entity id
						Dim paramEntityId As String = args.NameValuePairs("p_entity_id")
						If String.IsNullOrEmpty(paramEntityId) Then Throw New Exception("error getting company code by entity id: entity id is null")
						
						' Get company id based on entity id
						Dim dt As New DataTable
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(
								dbConn,
								"
								SELECT TOP 1 company_id
								FROM XFC_MAIN_MASTER_ProfitCenters WITH(NOLOCK)
								WHERE LEFT(entity_id, 5) = @paramEntityId
								",
								New List(Of DbParamInfo) From {
									New DbParamInfo("paramEntityId", paramEntityId)
								},
								False
							)
						End Using
						
						' Validate output
						If dt.Rows.Count = 0 Then Throw New Exception(
							$"error getting company code by entity id: no company id registered for entity id '{paramEntityId}'"
						)
						Dim companyId = dt.Rows(0)("company_id")
						If String.IsNullOrEmpty(companyId) Then Throw New Exception(
							$"error getting company code by entity id: no company id registered for entity id '{paramEntityId}'"
						)
						
                        Return companyId
                	End If
	            End If

                Return Nothing
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
End Namespace
