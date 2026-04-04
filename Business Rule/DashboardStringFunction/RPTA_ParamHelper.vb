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

Namespace OneStream.BusinessRule.DashboardStringFunction.RPTA_ParamHelper
	Public Class MainClass

#Region "Main"
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		RPTA_ParamHelper 
		'
		'Description:			Conditional Parameter helper functions
		'
		'Usage:					Used to provide conditional parameter processing functions that allow a parameter  
		'						value to be interpreted and subtituted with a different string.
		'
		'GetContentDescription
		'	Parameter Example:	XFBR(RPTA_ParamHelper,GetContentDescription,SelectedDashboard=|!SelectedContent_RPTA!|)
		'
		'GetAltViewDesc
		'	Parameter Example:	XFBR(RPTA_ParamHelper, GetTime)
		'
		'GetUserIsAdmin
		'	Parameter Example:	XFBR(RPTA_ParamHelper,GetUserIsAdmin)
		'
		'GetUserIsAdminOrReportAdmin
		'	Parameter Example:	XFBR(RPTA_ParamHelper,GetUserIsAdminOrReportAdmin)
		'
		'Created By:			OneStream
		'Date Created:			12-01-2018
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
					'Get an alternate description for the supplied dashboard name
					Return Me.GetContentDescription(si, globals, api, args)

				Else If args.FunctionName.XFEqualsIgnoreCase("GetTime") Then
					Dim wfTimeInfo As WorkflowUnitTimeInfo = BRApi.Workflow.General.GetWorkflowUnitTrackingTimeInfo(si, si.WorkflowClusterPk)
					Dim timeNames As New List(Of String)
					Dim timeInfos As List(Of NameAndId) = wfTimeInfo.GetCubeTimePeriodsAsNameAndIds(si,args.SubstVarSourceInfo.TimeDimAppInfo)
					For Each timeInfo As NameAndId In timeInfos
						timeNames.Add(timeInfo.Name)
					Next	
					Return SQLStringHelper.CreateInList(timeNames, True)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdmin")) Then
					Return Me.GetUserIsAdmin(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdminOrReportAdmin")) Then
					Return Me.GetUserIsAdminOrReportAdmin(si, globals, api, args)
					
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
#End Region

#Region "Helper Functions"

	Public Function GetContentDescription(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try
			'Get the clean description for the content title
			Dim dashboardName As String = args.NameValuePairs.XFGetValue("SelectedDashboard", "[No Selection]")
			Dim dashboardDesc As String = Me.GetFieldValueUsingKey(si, "Dashboard", "Name", dashboardName, "Description")
			
			'Make sure there is a description for the dashboard
			If String.IsNullOrEmpty(dashboardDesc) Then 
				Return "** Description Not Set for Dashboard: ** " & dashboardName
			Else
				Return dashboardDesc.ToUpper
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

	Public Function GetUserIsAdminOrReportAdmin(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try

			Dim reportSecurityGrp As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "StoredManageReportsSecurity_RPTAT")

			'Check to see if the user is an administrator (Can be used to hide objects that are administrator only)
			Using dbConnFW As DBConnInfo = BRApi.Database.CreateFrameworkDbConnInfo(si)
				If EngineSecurity.IsUserInGroup(dbConnFW, "Administrators") Then
					'User is admin, Return True
					Return "True"
				Else If EngineSecurity.IsUserInGroup(dbConnFW, reportSecurityGrp) Then
					'User is in reporting security group, Return True
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