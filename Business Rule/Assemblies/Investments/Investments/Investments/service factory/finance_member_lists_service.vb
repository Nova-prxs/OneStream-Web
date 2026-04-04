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
	Public Class finance_member_lists_service
		Implements IWsasFinanceMemberListsV800

        Public Function GetMemberList(ByVal si As SessionInfo, ByVal brGlobals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As MemberList Implements IWsasFinanceMemberListsV800.GetMemberList
            Try
                Return Nothing

            Catch ex As Exception
                Throw New XFException(si, ex)
            End Try
        End Function

	End Class
End Namespace
