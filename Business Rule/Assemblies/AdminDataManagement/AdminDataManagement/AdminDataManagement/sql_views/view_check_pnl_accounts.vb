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
	Public Class view_check_pnl_accounts

        'Declare view and table name
		Dim viewName As String = "XFC_MAIN_VIEW_CHECK_PnLAccounts"
		Dim tableName As String = "XFC_MAIN_MASTER_Accounts"

		#Region "Get Migration Query"
		
        Public Function GetMigrationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Migration type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					IF OBJECT_ID(N'{viewName}', N'V') IS NULL
						BEGIN
						EXEC('CREATE VIEW {viewName} AS
							WITH filtered_cost_centers AS (
								SELECT *
								FROM XFC_MAIN_MASTER_CostCenters
								WHERE end_date > GETDATE() AND (is_blocked <> 1 OR is_blocked IS NULL)
							), filtered_account_rubrics AS (
								SELECT
									fcc.id AS cost_center_id, fcc.class_id AS cost_center_class_id,
									ar.account_name, ar.conso_rubric
								FROM filtered_cost_centers fcc
								LEFT JOIN XFC_MAIN_MASTER_AccountRubrics ar
								ON fcc.class_id = ar.cost_center_class_id
								WHERE fcc.id IS NOT NULL
							), transactions_mapped_by_cost_center_class AS (
								-- First we check if the account has a rubric mapping with cost center class
								SELECT DISTINCT
									ft.company_id, ft.account_name, ft.cost_center_class_id, far.conso_rubric
								FROM (
									SELECT DISTINCT pnlt.company_id, pnlt.account_name, pnlt.cost_center_id, fcc.class_id AS cost_center_class_id
									FROM XFC_MAIN_FACT_PnLTransactions pnlt
									LEFT JOIN filtered_cost_centers fcc
									ON pnlt.cost_center_id = fcc.id
								) AS ft
								LEFT JOIN filtered_account_rubrics AS far
								ON ft.account_name = far.account_name AND ft.cost_center_id = far.cost_center_id
								WHERE far.conso_rubric IS NULL OR far.conso_rubric IN ('''', ''0'')
							)

							-- Then we check if the account has a rubric mapping with ''none'' cost center class
							SELECT 
								tmbccc.account_name, a.description AS account_description, tmbccc.company_id,
								COALESCE(tmbccc.cost_center_class_id, ''none'') AS cost_center_class_id, ar.conso_rubric
							FROM transactions_mapped_by_cost_center_class AS tmbccc
							LEFT JOIN XFC_MAIN_MASTER_AccountRubrics AS ar
							ON tmbccc.account_name = ar.account_name AND ar.cost_center_class_id = ''none''
							LEFT JOIN XFC_MAIN_MASTER_Accounts a
							ON tmbccc.account_name = a.name
							WHERE ar.conso_rubric IS NULL OR ar.conso_rubric IN ('''', ''0'');
						')
						END;
				"
				
				'Down
				Dim downQuery As String = $"
					DROP VIEW IF EXISTS {viewName};
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region
		
		#Region "Get Population Query"

        Public Function GetPopulationQuery(ByVal si As SessionInfo, type As String) As String
            Try
				'Handle type input
				If type.ToLower <> "up" AndAlso type.ToLower <> "down" Then _
					Throw New XFException(si, New Exception("Population type must be 'up' or 'down'"))
				'Up
				Dim upQuery As String = $"
					
				"
				
				'Down
				Dim downQuery As String = $"
					
				"
                Return If(type.ToLower = "up", upQuery, downQuery)
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function
		
		#End Region

	End Class
End Namespace
