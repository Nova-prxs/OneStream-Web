Imports System
Imports System.Data
Imports System.Collections
Imports System.Collections.Generic
Imports OneStream.Shared.Common
Imports OneStream.Shared.Wcf
Imports OneStream.Shared.Database

Namespace OneStream.BusinessRule.DashboardStringFunction.AST_ParamHelper

	''' <summary>
	''' Provides conditional Parameter string helper functions
	''' </summary>
	''' <remarks></remarks>
	Public Class MainClass
		' Create instance objects
		Dim ReadOnly _astHelper As New OneStream.BusinessRule.DashboardExtender.AST_SolutionHelper.MainClass
		Dim ReadOnly _astHelperCommon As New OneStream.BusinessRule.DashboardExtender.AST_SolutionHelperCommon.ASTCommon
		Dim ReadOnly _astHelperLcr As New OneStream.BusinessRule.DashboardExtender.AST_SolutionHelperLCR.LogCleaner
		Dim ReadOnly _astHelperSecurity As New OneStream.BusinessRule.DashboardExtender.AST_SolutionHelperCommon.SecurityAccess

#Region "Reference"
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 			AST_ParamHelper 
		'
		'Description:				Conditional Parameter helper functions
		'
		'Usage:						Used to provide conditional parameter processing functions that allow a parameter  
		'							value to be interpreted and subtituted with a different string.
		'
		'GetContentDescription
		'	Parameter Example:		XFBR(AST_ParamHelper, GetContentDescription, SelectedDashboard=|!SelectedContent_AST!|)
		'	Description:			Retrieve the Description from the Supplied Dashboard.
		'
		'GetUserIsAdmin
		'	Parameter Example:		XFBR(AST_ParamHelper, GetUserIsAdmin)
		'	Description:			Check to see if the active user is in the Admin group.
		'
		'GetIsUserAdminOrMgr
		'	Parameter Example:		XFBR(AST_ParamHelper, GetIsUserAdminOrMgr)
		'	Description:			Check to see if the active user is in the Admin or Manage app groups.
		'
		'GetIsUserASTAdminMgr
		'	Parameter Example:		XFBR(AST_ParamHelper, GetIsUserASTAdminMgr)
		'	Description:			Check to see if the active user is in the an AST Admin.
		'
		'ShowSettingsVisibility
		'	Parameter Example:		XFBR(AST_ParamHelper, ShowSettingsVisibility)
		'	Description:			Determine whether to display the Settings icon to the user.
		'
		'GetCleanUsername
		'	Parameter Example:		XFBR(AST_ParamHelper, GetCleanUsername)
		'	Description:			Return |USERNAME| withoutsystem characters (Periods are allowed)
		'
		'GetCurrentYearForSolutionHelp
		'   Parameter Example:		XFBR(AST_ParamHelper, GetCurrentYearForSolutionHelp)
		'
		'Created By:				OneStream Software
		'Date Created:				8-16-2018
		'------------------------------------------------------------------------------------------------------------		
#End Region
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
					'Get an alternate description for the supplied dashboard name
					Return Me.GetContentDescription(si, globals, api, args)

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdmin")) Then
					Return If(BRApi.Security.Authorization.IsUserInAdminGroup(si), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetIsUserAdminMgr")) Then
					'Check to see if the user is an Admin or is in Task Management Security Group
					Return If(_astHelperSecurity.IsUserAdminOrManager(si), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetIsUserASTAdminMgr")) Then
					'Check to see if the user is an Admin or is in AST Management Security Group
					Return If(_astHelperSecurity.IsUserASTAdminMgr(si), "True", "False")

				ElseIf args.FunctionName.XFEqualsIgnoreCase("ShowSettingsVisibility") Then
					Return If(_astHelperSecurity.IsUserASTAdminMgr(si), "True", "False")

				ElseIf args.FunctionName.XFEqualsIgnoreCase("IsAccountTypeVisible") Then
					Return If(_astHelperCommon.GetSessionStateValue(si, "DimensionType", "").XFEqualsIgnoreCase("Account"), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserHasASTAccess")) Then
					'Check to see if the user is an Admin or is in AST Management Security Group
					Return If(_astHelperSecurity.UserHasASTAccess(si), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserInGroup")) Then
					' Get the username to test
					Return BRApi.Security.Authorization.IsUserInGroup(si, args.NameValuePairs.XFGetValue("GroupName", "Administrators"))

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdminOrLogManager")) Then
					Return If(_astHelperSecurity.GetUserIsAdminOrLogManager(si), "True", "False")
					
				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetDefaultSelectedContent")) Then
					Return _astHelperLcr.GetDefaultSelectedContent(si)

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetDefaultSettingsDashboard")) Then
					Return _astHelper.GetDefaultSettingsDashboard(si)

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("GetCleanUsername")) Then
					'Get the User Document Folder with the Clean Name (Consistent with Platform Folder Naming)
					Return StringHelper.RemoveSystemCharacters(si.AuthToken.UserName, True, False)

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("UserHasDimensionLibraryAccess")) Then
					Return If(_astHelperSecurity.UserHasDimensionLibraryAccess(si), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("UserHasMFBAccess")) Then
					Return If(_astHelperSecurity.UserHasMFBAccess(si), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("UserHasHOMAccess")) Then
					Return If(_astHelperSecurity.UserIsInAccessGroup(si, "StoredAccessRole_ASTHOMT"), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("UserHasBRVAccess")) Then
					Return If(_astHelperSecurity.UserIsInAccessGroup(si, "StoredAccessRole_ASTBRVT"), "True", "False")

				ElseIf (args.FunctionName.XFEqualsIgnoreCase("UserHasLCRAccess")) Then
					Return If(_astHelperSecurity.UserIsInAccessGroup(si, "StoredAccessRole_ASTLCRT"), "True", "False")

				ElseIf args.FunctionName.XFEqualsIgnoreCase("DetermineDimensionSelected") Then
					' Get the Dimension Type					
					Dim dimensionType As String = args.NameValuePairs.XFGetValue("DimensionType", "[No Selection]").Replace("'", Nothing)
					' Get the Dimension
					Return _astHelperCommon.GetSessionStateValue(si, "bl_" & dimensionType & "Dimensions_ASTMFB", "")

				Else
					BRApi.ErrorLog.LogMessage(si, "AST_ParamHelper - Unknown Function=" & args.FunctionName)
				End If
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetContentDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
			Try
				'Get the clean description for the content title
				Dim dashboardName As String = args.NameValuePairs.XFGetValue("SelectedDashboard", "[No Selection]")
				Dim subTitle As String = args.NameValuePairs.XFGetValue("SubTitle", String.Empty)
				Dim dashboardDesc As String = String.Empty
				If String.IsNullOrWhiteSpace(subTitle) Then
					dashboardDesc = GetFieldValueUsingKey(si, "Dashboard", "Name", dashboardName, "Description")
				Else 
					'If subTitle.ToLower() = "brv" Then
					If subTitle.Equals("BRV", StringComparison.InvariantCultureIgnoreCase) Then
						dashboardDesc = "Business Rule Viewer - Business Rules"
					ElseIf subTitle.Equals("MFV", StringComparison.InvariantCultureIgnoreCase) Then
						dashboardDesc = "Business Rule Viewer - Member Formulas"
					ElseIf subTitle.Equals("HIST", StringComparison.InvariantCultureIgnoreCase) Then
						dashboardDesc = "Business Rule Viewer - Historical Viewer"
					End If 
				End If

				'Make sure there is a description for the dashboard
				Return If(String.IsNullOrEmpty(dashboardDesc), "** Description Not Set for Dashboard: ** " & dashboardName, dashboardDesc.ToUpper)

			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

		Public Function GetFieldValueUsingKey(ByVal si As SessionInfo, ByVal tableName As String, keyField As String, ByVal keyValue As String, ByVal fieldName As String) As String
			Try

				Dim parameters As List(Of DbParamInfo) = New List(Of DbParamInfo) From {
					    New DbParamInfo("@keyValue", keyValue)
					    }

				If (keyValue <> String.Empty) Then
					'Create the data table to return
					Dim sql As New Text.StringBuilder
					sql.AppendLine("SELECT " & SqlStringHelper.EscapeSqlString(fieldName))
					sql.AppendLine("FROM " & SqlStringHelper.EscapeSqlString(tableName))
					sql.AppendLine("WHERE " & SqlStringHelper.EscapeSqlString(keyField) & " = @keyValue")

					'Return the specified field value
					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Using dt As DataTable = BRApi.Database.ExecuteSql(dbConnApp, sql.ToString, parameters, False)
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

	End Class

End Namespace