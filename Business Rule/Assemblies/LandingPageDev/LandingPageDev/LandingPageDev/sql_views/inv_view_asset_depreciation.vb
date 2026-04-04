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
	Public Class inv_view_asset_depreciation

        'Declare view and table name
		Dim viewName As String = "XFC_INV_VIEW_asset_depreciation"
		Dim tableName As String = "XFC_INV_FACT_asset_depreciation"

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
								SELECT
								    a.cost_center_id,
								    a.project_id,
								    a.designation,
								    a.category_id,
									ac.years_amortization_conso,
									a.activation_date,
								    a.initial_value,
							        p.project_name,
							        p.company_name,
									p.company_id,
							        p.type,
							        p.aggregate,
							        p.budget_owner,
							        ama.*,
									CASE
							            WHEN LOWER(ama.scenario) LIKE ''%budget%'' THEN
							                COALESCE(
							                    (
													SELECT SUM(amount)
													FROM XFC_INV_FACT_asset_depreciation
													WHERE
														asset_id = ama.asset_id
														AND type = ama.depreciation_type
														AND (
															year = ama.year - 1
															AND scenario = COALESCE(rfy.scenario, ''Actual'')
														) OR (
															year < ama.year - 1
															AND scenario = ''Actual''
														)
												),
							                    0
											)
							            ELSE
							                COALESCE(
							                    (
													SELECT SUM(amount)
													FROM XFC_INV_FACT_asset_depreciation
													WHERE
														asset_id = ama.asset_id
														AND type = ama.depreciation_type
														AND year < ama.year
														AND scenario = ''Actual''
												),
							                    0
							    	) END AS amount_before,
									(
										ama.amount_m1
										+ ama.amount_m2
										+ ama.amount_m3
										+ ama.amount_m4
										+ ama.amount_m5
										+ ama.amount_m6
										+ ama.amount_m7
										+ ama.amount_m8
										+ ama.amount_m9
										+ ama.amount_m10
										+ ama.amount_m11
										+ ama.amount_m12
									) AS amount_year
							    FROM (
							        SELECT 
							            asset_id,
							            scenario, 
							            year,
										type AS depreciation_type,
							            COALESCE([1], 0) AS amount_m1,
							            COALESCE([2], 0) AS amount_m2,
							            COALESCE([3], 0) AS amount_m3,
							            COALESCE([4], 0) AS amount_m4,
							            COALESCE([5], 0) AS amount_m5,
							            COALESCE([6], 0) AS amount_m6,
							            COALESCE([7], 0) AS amount_m7,
							            COALESCE([8], 0) AS amount_m8,
							            COALESCE([9], 0) AS amount_m9,
							            COALESCE([10], 0) AS amount_m10,
							            COALESCE([11], 0) AS amount_m11,
							            COALESCE([12], 0) AS amount_m12
							        FROM 
							            (SELECT asset_id, scenario, year, month, type, amount FROM {tableName}) AS SourceTable
							        PIVOT
							        (
							            SUM(amount)
							            FOR month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
							        ) AS PivotTable
							    ) AS ama
								INNER JOIN XFC_INV_MASTER_asset a ON ama.asset_id = a.id
							    INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
								LEFT JOIN XFC_INV_MASTER_asset_category ac ON a.category_id = ac.id
								LEFT JOIN (
									SELECT asset_id, year, scenario, depreciation_type
									FROM (
										SELECT
											asset_id,
									        year,
									        scenario,
											type AS depreciation_type,
									        ROW_NUMBER() OVER (
												PARTITION BY asset_id, year, type
												ORDER BY CAST(REPLACE(LEFT(scenario, 4), ''RF'', '''') AS INTEGER) DESC
											) AS rn
									    FROM XFC_INV_FACT_asset_depreciation
									    WHERE scenario LIKE ''RF%''
									) AS subquery
									WHERE rn = 1
								) rfy ON
									ama.asset_id = rfy.asset_id
									AND ama.year - 1 = rfy.year
									AND ama.depreciation_type = rfy.depreciation_type
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
