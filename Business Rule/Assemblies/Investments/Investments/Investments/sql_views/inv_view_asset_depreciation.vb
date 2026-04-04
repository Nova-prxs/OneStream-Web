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
								    CASE 
								        WHEN new_cc_direct.id IS NOT NULL THEN a.cost_center_id
								        
								        WHEN mapping.id IS NOT NULL 
								             AND (new_cc_mapped.end_date IS NULL OR new_cc_mapped.end_date >= GETDATE()) 
								        THEN mapping.id
								        
								        ELSE a.cost_center_id
								    END AS cost_center_id,
								    
								    -- Lógica para cost_center_type
								    COALESCE(
								        new_cc_direct.class_id,      -- Si es cost center directo new
								        new_cc_mapped.class_id,      -- Si tiene mapeo válido
								        inv_cc.cost_center_type      -- Si es cost center original
								    ) AS Cost_Center_Type,
								    
								    -- CORRECCIÓN: Lógica dinámica para entity_member usando mapeo reverso
								    COALESCE(
								        reverse_inv_cc.entity_member,    -- Si es cost center directo new, usar mapeo reverso
								        inv_cc.entity_member             -- Si es cost center original o con mapeo
								    ) AS Cost_Center_Entity_Member,
								    
								    -- Información de type desde la tabla de tipos
								    cc_type.depreciation_account AS Depreciation_Account_From_CCType,
								    cc_type.asset_account AS Asset_Account_From_CCType,
								    
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
								    ama.asset_id,
								    ama.scenario,
								    ama.year,
								    ama.depreciation_type,
								    ama.amount_m1,
								    ama.amount_m2,
								    ama.amount_m3,
								    ama.amount_m4,
								    ama.amount_m5,
								    ama.amount_m6,
								    ama.amount_m7,
								    ama.amount_m8,
								    ama.amount_m9,
								    ama.amount_m10,
								    ama.amount_m11,
								    ama.amount_m12,
								    CASE
								        WHEN LOWER(ama.scenario) LIKE ''%budget%'' THEN
								            COALESCE(
								                (
								                    SELECT SUM(dep_fact.amount)
								                    FROM XFC_INV_FACT_asset_depreciation dep_fact
								                    WHERE
								                        dep_fact.project_id = ama.project_id
								                        AND dep_fact.asset_id = ama.asset_id
								                        AND (
								                            (dep_fact.year = ama.year - 1 AND dep_fact.scenario = COALESCE(rfy.scenario, ''Actual''))
								                            OR (dep_fact.year < ama.year - 1 AND dep_fact.scenario = ''Actual'')
								                        )
								                        AND dep_fact.type = ama.depreciation_type
								                ),
								                0
								            )
								        ELSE
								            COALESCE(
								                (
								                    SELECT SUM(dep_fact.amount)
								                    FROM XFC_INV_FACT_asset_depreciation dep_fact
								                    WHERE
								                        dep_fact.project_id = ama.project_id
								                        AND dep_fact.asset_id = ama.asset_id
								                        AND dep_fact.scenario = ''Actual''
								                        AND dep_fact.type = ama.depreciation_type
								                        AND dep_fact.year < ama.year
								                ),
								                0
								            )
								    END AS amount_before,
								    (
								        ama.amount_m1 + ama.amount_m2 + ama.amount_m3 + ama.amount_m4 +
								        ama.amount_m5 + ama.amount_m6 + ama.amount_m7 + ama.amount_m8 +
								        ama.amount_m9 + ama.amount_m10 + ama.amount_m11 + ama.amount_m12
								    ) AS amount_year
								FROM (
								    SELECT
								        asset_id,
								        project_id,
								        scenario,
								        year,
								        type AS depreciation_type,
								        COALESCE([1], 0) AS amount_m1, COALESCE([2], 0) AS amount_m2, COALESCE([3], 0) AS amount_m3,
								        COALESCE([4], 0) AS amount_m4, COALESCE([5], 0) AS amount_m5, COALESCE([6], 0) AS amount_m6,
								        COALESCE([7], 0) AS amount_m7, COALESCE([8], 0) AS amount_m8, COALESCE([9], 0) AS amount_m9,
								        COALESCE([10], 0) AS amount_m10, COALESCE([11], 0) AS amount_m11, COALESCE([12], 0) AS amount_m12
								    FROM
								        (SELECT asset_id, project_id, scenario, year, month, type, amount FROM XFC_INV_FACT_asset_depreciation) AS SourceTable
								    PIVOT
								    (
								        SUM(amount)
								        FOR month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
								    ) AS PivotTable
								) AS ama
								INNER JOIN XFC_INV_MASTER_asset a ON ama.project_id = a.project_id AND ama.asset_id = a.id
								INNER JOIN XFC_INV_MASTER_project p ON a.project_id = p.project
								LEFT JOIN XFC_INV_MASTER_asset_category ac ON a.category_id = ac.id
								
								LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_direct ON a.cost_center_id = new_cc_direct.id
								
								-- 2. Buscar mapeo old->new
								LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew mapping ON a.cost_center_id = mapping.id_old
								LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_mapped ON mapping.id = new_cc_mapped.id
								
								-- 3. Mapeo reverso para cost centers que ya son new (como Aveiro)
								LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew reverse_mapping ON a.cost_center_id = reverse_mapping.id
								LEFT JOIN XFC_INV_MASTER_Cost_Center reverse_inv_cc ON reverse_mapping.id_old = reverse_inv_cc.id
								
								-- 4. Información del cost center original (para entity_member y cost_center_type si no hay mapeo)
								LEFT JOIN XFC_INV_MASTER_Cost_Center inv_cc ON a.cost_center_id = inv_cc.id
								
								-- 5. Información de tipos (depreciation_account, asset_account)
								LEFT JOIN XFC_INV_MASTER_Cost_center_type cc_type ON 
								    COALESCE(
								        new_cc_direct.class_id,      -- Si es cost center new directo
								        new_cc_mapped.class_id,      -- Si tiene mapeo válido
								        inv_cc.cost_center_type      -- Si es cost center original
								    ) = cc_type.id
								
								LEFT JOIN (
								    SELECT asset_id, project_id, year, scenario, type
								    FROM (
								        SELECT
								            asset_id,
								            project_id,
								            year,
								            scenario,
								            type,
								            ROW_NUMBER() OVER (
								                PARTITION BY asset_id, project_id, year, type
								                ORDER BY CAST(REPLACE(LEFT(scenario, 4), ''RF'', '''') AS INTEGER) DESC
								            ) AS rn
								        FROM XFC_INV_FACT_asset_depreciation
								        WHERE scenario LIKE ''RF%''
								    ) AS subquery_rfy
								    WHERE rn = 1
								) rfy ON ama.project_id = rfy.project_id
								    AND ama.asset_id = rfy.asset_id
								    AND ama.year - 1 = rfy.year
								    AND ama.depreciation_type = rfy.type;
											
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
