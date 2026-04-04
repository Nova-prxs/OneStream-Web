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
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_work_order,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_work_order_figures,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_structure_figures,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_fxrate,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.dim_entity,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.dim_account,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_fxrate,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_serv_type,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_rev_type,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_entity_currency,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_scenario_fxratetype,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_department,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_technology,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_year_scenario_confirm,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_template_warning,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.work_order,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.work_order_figures,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.structure_figures,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.function_translate_amount,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_work_order_figures_horizontal,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_work_order_figures_ytd,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_work_order_figures_translated,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_structure_figures_horizontal,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_structure_figures_ytd,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_structure_figures_translated,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.serv_allocations_main
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
