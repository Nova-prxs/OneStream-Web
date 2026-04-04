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
	Public Class Migrations
		
		'Get all the table classes
		Dim tables As New List(Of Object) From {
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_raw_project,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_raw_project_cash_out,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_raw_project_capitalization,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_raw_project_revenue,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_raw_plant_company,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_aux_project_internal_expense_proportion,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_plant_company,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_company,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_expense,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_revenue,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_cash_out,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_capitalization,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_project_internal_expense_proportion,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.rnd_view_project_internal_expense_proportion
		}

		#Region "Get Migration Queries"
		
        Public Function GetMigrationQueries(ByVal si As SessionInfo, ByVal type As String) As List(Of String)
            Try
				'Declare list of queries
				Dim queries As New List(Of String)
				
				'Loop through all the tables to get the queries
				For Each table In tables
					queries.Add(table.GetMigrationQuery(si, type))
				Next
				
                Return queries
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

		#Region "Get Population Queries"
		
        Public Function GetPopulationQueries(ByVal si As SessionInfo, ByVal type As String) As List(Of String)
            Try
				'Declare list of queries
				Dim queries As New List(Of String)
				
				'Loop through all the tables to get the queries
				For Each table In tables
					queries.Add(table.GetPopulationQuery(si, type))
				Next
				
                Return queries
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
