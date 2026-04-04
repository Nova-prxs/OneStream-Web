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

Namespace OneStream.BusinessRule.DashboardStringFunction.CopyScenarioSecurity
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As DashboardStringFunctionArgs) As Object
			
			Dim isAdmin As Boolean = False
			Dim userName As String = si.UserName
			
			Try
				If args.FunctionName.XFEqualsIgnoreCase("GetUserIsAdmin") Then
					'If userName.Equals("aratz.nova") Or userName.Equals("NOVA") Or userName.Equals("Nathalie.Nova") Then
					If BRApi.Security.Authorization.IsUserInAdminGroup(si)
						isAdmin = True
					End If
				End If

				Return isAdmin
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace