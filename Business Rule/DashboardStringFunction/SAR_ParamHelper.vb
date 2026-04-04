Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.DashboardStringFunction.SAR_ParamHelper
	Public Class MainClass
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		SAR_ParamHelper
		'
		'Description:			Conditional Parameter helper functions
		'
		'Usage:					Used to provide conditional parameter processing functions that allow a parameter
		'						value to be interpreted and subtituted with a different string.
		'GetContentDescription
		'	Parameter Example:	XFBR(SAR_ParamHelper, GetContentDescription, SelectedDashboard=|!SelectedMainContent_SAR!|)
		'
		'GetFieldValueUsingKey
		'	Parameter Example:	XFBR(SAR_ParamHelper, GetFieldValueUsingKey, TableName=[YourTableName], KeyField=[YourKeyFieldName], KeyValue=[YourKeyFieldValue], FieldToReturn=[YourFieldToReturn])
		'
		'GetUserIsAdminOrInManageGroup
		'	Parameter Example:	XFBR(SAR_ParamHelper, GetUserIsAdminOrInManageGroup)
		'
		'GetUserSelectedMainContent
		'	Parameter Example:	XFBR(LCR_ParamHelper, GetUserSelectedMainContent)
		'
		'Created By:			OneStream Software
		'Date Created:			01-06-2015
		'------------------------------------------------------------------------------------------------------------

#Region "Main"
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
					'Get an alternate description for the supplied dashboard name
					Return Me.GetContentDescription(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetFieldValueUsingKey") Then
					'Lookup a row from a key field value and return a different field value from the row
					Dim tableName As String = args.NameValuePairs.XFGetValue("TableName", String.Empty)
					Dim keyField As String = args.NameValuePairs.XFGetValue("KeyField", String.Empty)
					Dim keyValue As String = args.NameValuePairs.XFGetValue("KeyValue", String.Empty)
					Dim fieldToReturn As String = args.NameValuePairs.XFGetValue("FieldToReturn", String.Empty)
					Return Me.GetFieldValueUsingKey(si, tableName, keyField, keyValue, fieldToReturn)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdminOrInManageGroup") Then
					Return Me.GetUserIsAdminOrInManageGroup(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetUserSelectedMainContent") Then
					Return Me.GetUserSelectedMainContent(si, globals, api, args)

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetCurrentDate") Then
					Dim culture = New CultureInfo(si.Culture)
					Dim dtStr As String = DateTime.Today.ToString("d", culture)
					Return dtStr

				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetDateFormatString") Then
					Dim culture = New CultureInfo(si.Culture)
					Return culture.DateTimeFormat.ShortDatePattern

				Else
					Return Nothing
				End If

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
#End Region

#Region "General Helpers"

		Public Function GetFieldValueUsingKey(ByVal si As SessionInfo, ByVal tableName As String, keyField As String, ByVal keyValue As String, ByVal fieldName As String) As String
			Try

				If keyValue <> String.Empty Then
					'Create the data table to return
					Dim sql As New Text.StringBuilder
					sql.Append("SELECT " & fieldName & " ")
					sql.Append("FROM " & tableName & " ")
					sql.Append("WHERE " & keyField & " = '" & keyValue & "'")

					'Return the specified field value
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, False)
							If dt.Rows.Count = 1 Then
								Return dt.Rows(0)(0).ToString
							Else
								Dim message As String = "GetFieldValueUsingKey Failed: could not find KeyField (" & keyField & ") Value (" & keyValue & ") SQL = " & sql.ToString
								BRApi.ErrorLog.LogMessage(si, message)
								Return String.Empty
							End If
						End Using
					End Using
				Else
					Return "No Selection"
				End If
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetContentDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Get the clean description for the content title
				Dim dashboardName As String = args.NameValuePairs.XFGetValue("SelectedDashboard", "[No Selection]")
				Dim dashboardDesc As String = Me.GetFieldValueUsingKey(si, "Dashboard", "Name", dashboardName, "Description")

				'Make sure there is a description for the dashboard
				Return If(String.IsNullOrEmpty(dashboardDesc), "** Description Not Set for Dashboard: ** " & dashboardName, dashboardDesc.ToUpper)

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

#Region "Security Helpers"

		Public Function GetUserIsAdminOrInManageGroup(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim manageGroup As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredSettingSecurityAccessGroup_SART")
				'Check to see if the user is an administrator (Can be used to hide objects that are administrator only)
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					If EngineSecurity.IsUserInGroup(dbConnFW, "Administrators") Then
						'User is admin, Return True
						Return "True"
					Else
						Return If(EngineSecurity.IsUserInGroup(dbConnFW, manageGroup), "True", "False")
					End If
				End Using

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetUserSelectedMainContent(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try

				Dim manageGroup As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredSettingSecurityAccessGroup_SART")
				'Check to see if the user is an administrator (Can be used to hide objects that are administrator only)
				Using dbConnFW As DbConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
					If EngineSecurity.IsUserInGroup(dbConnFW, "Administrators") Then
						'User is admin, Return True
						Return "0_Home_SAR"
					Else
						Return If(EngineSecurity.IsUserInGroup(dbConnFW, manageGroup), "0_Home_SAR", "0_Help_SARH")
					End If
				End Using

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region
	End Class
End Namespace