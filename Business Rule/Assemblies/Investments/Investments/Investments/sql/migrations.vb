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
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.main_raw_fxrate,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.main_aux_fxrate,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.function_translate_amount,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_project,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_cost_center_type,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_cost_center,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_company_holiday,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_project_cash,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_project_capitalization,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_asset,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_asset_depreciation,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_raw_comment_gap,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_asset,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_project_cash,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_project_capitalization,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_modified_projection,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_confirmed_month,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_confirmed_scenario_year,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_comment,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_comment_gap,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_date_dimension,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_budget_2025,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_company,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_company_holiday,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_cost_center_type,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_cost_center,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_project,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_project_capitalization,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_project_cash,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_project_financial_figures,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_asset,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_asset_depreciation,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_asset_category,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_cash_sop_percentage,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_view_asset_depreciation,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_view_project_financial_figures,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_view_project_cash,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_view_project_cash_ytd,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_view_project_cash_gap_summary,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_view_project_capitalization,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_project,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_fictitious_assets,
			New Workspace.__WsNamespacePrefix.__WsAssemblyName.inv_aux_cost_center_mapping_s4
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
