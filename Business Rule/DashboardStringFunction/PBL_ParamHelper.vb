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

Namespace OneStream.BusinessRule.DashboardStringFunction.PBL_ParamHelper
	Public Class MainClass
		
#Region "Main"
		'------------------------------------------------------------------------------------------------------------
		'Reference Code: 		PBL_ParamHelper 
		'
		'Description:			Conditional Parameter helper functions
		'
		'Usage:					Used to provide conditional parameter processing functions that allow a parameter  
		'						value to be interpreted and subtituted with a different string.
		'
		'GetBlockImage			XFBR(PBL_ParamHelper, GetBlockImage)
		'
		'GetBlockText			XFBR(PBL_ParamHelper, GetBlockText)
		'
		'GetContentDescription
		'	Parameter Example:	XFBR(PBL_ParamHelper, GetContentDescription, SelectedDashboard=|!SelectedContent_PBL!|)
		'
		'GetUserIsAdminOrMgr
		'	Parameter Example:	XFBR(PBL_ParamHelper, GetUserIsAdminOrMgr)				
		'		
		'Created By:			OneStream Software
		'Date Created:			04-12-2018	
		'------------------------------------------------------------------------------------------------------------		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetBlockImage") Then
					Dim pblHelper As New OneStream.BusinessRule.DashboardExtender.PBL_SolutionHelper.MainClass
					Return pblHelper.GetBlockImageName(si, "BRUpdate")

				Else If args.FunctionName.XFEqualsIgnoreCase("GetBlockText") Then
					Dim pblHelper As New OneStream.BusinessRule.DashboardExtender.PBL_SolutionHelper.MainClass
					Return pblHelper.GetBlockStatusText(si, "BRUpdate")
				
				Else If args.FunctionName.XFEqualsIgnoreCase("GetContentDescription") Then
					'Get an alternate description for the supplied dashboard name
					Return Me.GetContentDescription(si, globals, api, args)

				Else If (args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdminOrMgr")) Then
					Return Me.GetUserAdminOrManager(si, globals, api, args)
					
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
			Dim dashboardName As String = args.NameValuePairs.XFGetValue("SelectedDashboard", "No Selection")
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

	Public Function GetUserAdminOrManager(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As String
		Try

			Dim pblHelper As New OneStream.BusinessRule.DashboardExtender.PBL_SolutionHelper.MainClass				
			If pblHelper.IsUserAdminOrSolutionManager(si) Then
				Return "True"
			Else
				Return "False"
			End If		
			
		Catch ex As Exception
			Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
		End Try			
	End Function	

#End Region

	End Class
End Namespace