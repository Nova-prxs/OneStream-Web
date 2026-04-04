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
	Public Class datamanagement_service
		Implements IWsasDataManagementStepV800

        Public Sub ProcessDataManagementStep(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal workspace As DashboardWorkspace, _
            ByVal args As ExtenderArgs) Implements IWsasDataManagementStepV800.ProcessDataManagementStep
            Try
                If(brGlobals IsNot Nothing) AndAlso(workspace IsNot Nothing) AndAlso(args?.DataMgmtArgs IsNot Nothing) Then
                    ' Implement Data Management Step logic here...
                End If
            Catch ex As Exception
				Throw New XFException(si, ex)
            End Try
        End Sub

	End Class
End Namespace
