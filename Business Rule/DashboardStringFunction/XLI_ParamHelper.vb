Imports System
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.VisualBasic
Imports System.Windows.Forms
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Engine
Imports OneStream.Shared.Database
Imports OneStream.Stage.Engine
Imports OneStream.Stage.Database
Imports OneStream.Finance.Engine
Imports OneStream.Finance.Database

Namespace OneStream.BusinessRule.DashboardStringFunction.XLI_ParamHelper
	Public Class MainClass
		
#Region "Main"		
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 			XLI_ParamHelper 
		'
		'Description:				Conditional Parameter helper functions
		'
		'Usage:						Used to provide conditional parameter processing functions that allow a parameter  
		'							value to be interpreted and subtituted with a different string.
		'
		'GetContentDescription
		'	Parameter Example:		XFBR(XLI_ParamHelper, GetContentDescription, SelectedDashboard=|!SelectedContent_XLI!|)
		'	Description:			Retrieve the Description from the Supplied Dashboard.
		'
		'GetFieldValueUsingKey
		'	Parameter Example:		XFBR(XLI_ParamHelper, GetFieldValueUsingKey, TableName=[YourTableName], KeyField=[YourKeyFieldName], KeyValue=[YourKeyFieldValue], FieldToReturn=[YourFieldToReturn])
		'	Description:			Lookup a value from a Database Table.		
		'
		'GetUserIsAdmin
		'	Parameter Example:		XFBR(XLI_ParamHelper, GetUserIsAdmin)
		'	Description:			Check to see if the active user is in the Admin group.
		'
		'GetIsUserAdminOrMgr
		'	Parameter Example:		XFBR(XLI_ParamHelper, GetIsUserAdminOrMgr)
		'	Description:			Check to see if the active user is in the Admin or Manage app groups.
		'
		'GetCleanUsername
		'	Parameter Example:		XFBR(XLI_ParamHelper, GetCleanUsername)
		'	Description:			Return |USERNAME| withoutsystem characters (Periods are allowed)
		'		
		'Created By:				OneStream Software
		'Date Created:				2-28-2019
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
					'Get an alternate description for the supplied dashboard name
					Return Me.GetContentDescription(si, globals, api, args)
					
				Else If args.FunctionName.XFEqualsIgnoreCase("GetFieldValueUsingKey") Then	
					'Lookup a row from a key field value and return a different field value from the row
					Dim tableName As String = args.NameValuePairs.XFGetValue("TableName")
					Dim keyField As String = args.NameValuePairs.XFGetValue("KeyField")
					Dim keyValue As String = args.NameValuePairs.XFGetValue("KeyValue")
					Dim fieldToReturn As String = args.NameValuePairs.XFGetValue("FieldToReturn")
					Return Me.GetFieldValueUsingKey(si, tableName, keyField, keyValue, fieldToReturn)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdmin")) Then
					Return Me.GetUserIsAdmin(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetIsUserAdminMgr")) Then
					'Check to see if the user is an Admin or is in Task Management Security Group
					Dim XLIHelper As New OneStream.BusinessRule.DashboardExtender.XLI_SolutionHelper.MainClass
					If XLIHelper.IsUserAdminOrManager(si) Then
						Return "True"
					Else
						Return "False"
					End If

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetCleanUsername")) Then					
					'Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
					Dim allowPeriods As Boolean = True
					Dim allowSpaces As Boolean = False
					Return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, allowPeriods, allowSpaces)					

				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region		
		
#Region "Standard Helper Functions"

		Public Function GetContentDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Get the clean description for the content title
				Dim dashboardName As String = args.NameValuePairs.XFGetValue("SelectedDashboard", "[No Selection]")
				Dim dashboardDesc As String = Me.GetFieldValueUsingKey(si, "Dashboard", "Name", dashboardName, "Description")
				
				'Make sure there is a description for the dashboard
				If String.IsNullOrEmpty(dashboardDesc) Then 
					Return "** Description Not Set for Dashboard: ** " & dashboardName
				Else
					Return dashboardDesc.ToUpper()
				End If
					
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetFieldValueUsingKey(ByVal si As SessionInfo, ByVal tableName As String, keyField As String, ByVal keyValue As String, ByVal fieldName As String) As String
			Try
				
				If (keyValue <> String.Empty) Then
					'Create the data table to return
					Dim sql As New Text.StringBuilder					
					sql.Append("SELECT " & fieldName & " ")
					sql.Append("FROM " & tableName & " ")
					sql.Append("WHERE " & keyField & " = '" & keyValue & "'")
					
					'Return the specified field value
					Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
						Using dt As DataTable = BRAPi.Database.ExecuteSql(dbConnApp, sql.ToString, False)
							If dt.Rows.Count = 1 Then
								Return dt.Rows(0)(0).ToString
							Else
								Dim message As String = "GetFieldValueUsingKey Failed: could not find KeyField (" & keyField & ") Value (" & keyValue & ") SQL = " & sql.ToString 
								BRAPi.ErrorLog.LogMessage(si, message)
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

		Public Function GetUserIsAdmin(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Check to see if the user is an administrator (Can be used to hide objects that are administrator only)
				Using dbConnFW As DBConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)																			
					If EngineSecurity.IsUserInGroup(dbConnFW, "Administrators") Then					
						'User is admin, Return True
						Return "True"
					Else
						'User is NOT admin, Return False
						Return "False"
					End If
				End Using
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

#End Region

	End Class
End Namespace