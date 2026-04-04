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
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_s4h_test,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_forex_rates,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_profit_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_cost_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_cost_center_class_to_rubric,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_work_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_accounts,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_accounts_old_to_new,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_account_bs_to_conso,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_account_pnl_to_conso,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_product,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_business_partners,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_trading_partners,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_customers_to_ud1_ic,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_pnl_transactions,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.raw_plt_fact_production_sap,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_check_pnl_accounts,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_check_bs_accounts,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.aux_check_pnl_cost_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_table_import_info,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_pnl_has_changed,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_companies,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_account_flows,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_flow_mappings,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_accounts,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_account_rubrics,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_accounts_old_to_new,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_cost_center_classes,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_cost_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_cost_centers_old_to_new,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_cost_centers_old_to_onestream_member,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_business_partners,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_organs,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_product_types,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_products,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_profit_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.master_work_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.fact_pnl_transactions,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_check_profit_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_check_cost_center_classes,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_check_pnl_accounts,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_check_bs_accounts,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_check_cost_centers_old_to_new,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_check_pnl_cost_centers,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_trading_partners,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.view_business_partners
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
