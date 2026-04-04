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

Namespace OneStream.BusinessRule.DashboardStringFunction.ALL_ParamHelper
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetModelName") Then
					'XFBR(ALL_ParamHelper, GetModelName)
					Dim strProfileName As String = BRApi.Workflow.Metadata.GetProfile(si, si.WorkflowClusterPk.ProfileKey).Name
					Dim strModelName As String = strProfileName.Split(".").Last
					Return strModelName
					
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetDashbordAdmin") Then
					'XFBR(ALL_ParamHelper, GetDashbordAdmin, UserType=|!dl_UserType_ALL!|)
					Dim strUserType As String = args.NameValuePairs.XFGetValue("UserType", String.Empty)
					
					Dim strResult As String = String.Empty
					If strUserType.XFEqualsIgnoreCase("admin") Then
						strResult = "*"
					Else
						strResult = "0"
					End If 
					
					Return strResult
					
				ElseIf args.FunctionName.XFEqualsIgnoreCase("GetDashbordUser") Then
					'XFBR(ALL_ParamHelper, GetDashbordUser, UserType=|!dl_UserType_ALL!|)
					Dim strUserType As String = args.NameValuePairs.XFGetValue("UserType", String.Empty)
					
					Dim strResult As String = String.Empty
					If strUserType.XFEqualsIgnoreCase("user") Then
						strResult = "*"
					Else
						strResult = "0"
					End If 
					
					Return strResult
					
				End If

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		
	End Class
End Namespace