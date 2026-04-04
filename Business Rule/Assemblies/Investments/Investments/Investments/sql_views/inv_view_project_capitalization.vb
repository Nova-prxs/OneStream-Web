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
	Public Class inv_view_project_capitalization

        'Declare view and table name
		Dim viewName As String = "XFC_INV_VIEW_project_capitalization"
		Dim tableName As String = "XFC_INV_FACT_project_capitalization"

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
								    pma.project_id,
								    pma.scenario,
								    pma.year,
								    pma.amount_m1,
								    pma.amount_m2,
								    pma.amount_m3,
								    pma.amount_m4,
								    pma.amount_m5,
								    pma.amount_m6,
								    pma.amount_m7,
								    pma.amount_m8,
								    pma.amount_m9,
								    pma.amount_m10,
								    pma.amount_m11,
								    pma.amount_m12,
								    CASE
								        WHEN mp.project_id IS NOT NULL THEN 1
								        ELSE 0
								    END AS is_modified,
								    p.project_name AS project_name,
								    p.company_name AS company_name,
								    p.company_id AS company_id,
								    p.type AS type,
								    p.aggregate AS aggregate,
								    p.site AS site,
								    p.budget_owner AS budget_owner,
								    p.ma_date AS ma_date,
								    p.start_date AS start_date,
								    p.end_date AS end_date,
								    p.is_real AS is_real,
								    p.poi_poe,
								    (
								        COALESCE(pma.amount_m1, 0) + COALESCE(pma.amount_m2, 0) + COALESCE(pma.amount_m3, 0) +
								        COALESCE(pma.amount_m4, 0) + COALESCE(pma.amount_m5, 0) + COALESCE(pma.amount_m6, 0) +
								        COALESCE(pma.amount_m7, 0) + COALESCE(pma.amount_m8, 0) + COALESCE(pma.amount_m9, 0) +
								        COALESCE(pma.amount_m10, 0) + COALESCE(pma.amount_m11, 0) + COALESCE(pma.amount_m12, 0)
								    ) AS amount_year_capitalization,
								    p.total_requirement,
								    COALESCE(
								        (
								            SELECT TOP 1 ffigs.amount_ytd
								            FROM XFC_INV_FACT_project_financial_figures ffigs
								            WHERE ffigs.project_id = pma.project_id AND ffigs.year = pma.year AND ffigs.month = 12
								                AND ffigs.type = ''Requirement'' AND ffigs.scenario = ''Actual''
								            ORDER BY ffigs.month DESC
								        ), 0
								    ) AS requirement_ytd,
								    COALESCE(
								        (
								            SELECT TOP 1 ffigs.amount_ytd
								            FROM XFC_INV_FACT_project_financial_figures ffigs
								            WHERE ffigs.project_id = pma.project_id AND ffigs.year = pma.year AND ffigs.month = 12
								                AND ffigs.type = ''Delivered'' AND ffigs.scenario = ''Actual''
								            ORDER BY ffigs.month DESC
								        ), 0
								    ) AS delivered_ytd,
								    pad.cost_center_type AS Cost_Center_Type,
								    pad.entity_member AS Cost_Center_Entity_Member,
								    pad.asset_account AS Capitalization_Account_Rubric
								FROM (
								    SELECT 
								        project_id, scenario, year,
								        COALESCE([1], 0) AS amount_m1, COALESCE([2], 0) AS amount_m2, COALESCE([3], 0) AS amount_m3,
								        COALESCE([4], 0) AS amount_m4, COALESCE([5], 0) AS amount_m5, COALESCE([6], 0) AS amount_m6,
								        COALESCE([7], 0) AS amount_m7, COALESCE([8], 0) AS amount_m8, COALESCE([9], 0) AS amount_m9,
								        COALESCE([10], 0) AS amount_m10, COALESCE([11], 0) AS amount_m11, COALESCE([12], 0) AS amount_m12
								    FROM 
								        (SELECT project_id, scenario, year, month, amount FROM XFC_INV_FACT_project_capitalization) AS SourceTable 
								    PIVOT
								    (
								        SUM(amount) FOR month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
								    ) AS PivotTable
								) AS pma
								LEFT JOIN XFC_INV_MASTER_project p ON pma.project_id = p.project
								LEFT JOIN XFC_INV_AUX_modified_projection mp ON pma.project_id = mp.project_id 
								    AND pma.scenario = mp.scenario 
								    AND pma.year = mp.year 
								    AND mp.type = ''capitalization''
								LEFT JOIN (
								    SELECT project_id, year, scenario
								    FROM (
								        SELECT 
								            project_id, year, scenario,
								            ROW_NUMBER() OVER (PARTITION BY project_id, year ORDER BY CAST(REPLACE(LEFT(scenario, 4), ''RF'', '''') AS INTEGER) DESC) AS rn
								        FROM XFC_INV_FACT_project_capitalization 
								        WHERE scenario LIKE ''RF%''
								    ) AS subquery_rfy_def
								    WHERE rn = 1
								) rfy ON pma.project_id = rfy.project_id AND pma.year - 1 = rfy.year
								LEFT JOIN (
								    SELECT
								        po.project_id,
								        CASE 
								            WHEN new_cc_direct.id IS NOT NULL THEN po.cost_center_id
								            
								            WHEN mapping.id IS NOT NULL 
								                 AND (new_cc_mapped.end_date IS NULL OR new_cc_mapped.end_date >= GETDATE()) 
								            THEN mapping.id
								            
								            -- 3. Si no hay mapeo válido, mantener original
								            ELSE po.cost_center_id
								        END AS cost_center_id,
								        
								        -- Cost center type con lógica dinámica
								        COALESCE(
								            new_cc_direct.class_id,      -- Si es cost center directo new
								            new_cc_mapped.class_id,      -- Si tiene mapeo válido
								            inv_cc.cost_center_type      -- Si es cost center original
								        ) AS cost_center_type,
								        
								        inv_cc.entity_member,
								        
								        cc_type.asset_account
								        
								    FROM (
								        SELECT project_id, cost_center_id
								        FROM (
								            SELECT 
								                project_id, cost_center_id,
								                ROW_NUMBER() OVER (
								                    PARTITION BY project_id ORDER BY initial_value DESC
								                ) AS rn
								            FROM XFC_INV_MASTER_asset
								        ) AS subquery
								        WHERE rn = 1
								    ) po
								    
								
								    LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_direct ON po.cost_center_id = new_cc_direct.id
								    
								    LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew mapping ON po.cost_center_id = mapping.id_old
												
								    LEFT JOIN XFC_MAIN_MASTER_costCenters new_cc_mapped ON mapping.id = new_cc_mapped.id
								    
								    LEFT JOIN XFC_INV_MASTER_Cost_Center inv_cc ON po.cost_center_id = inv_cc.id
								    
								
								    LEFT JOIN XFC_INV_MASTER_Cost_center_type cc_type ON 
								        COALESCE(
								            new_cc_direct.class_id,      -- Si es cost center new directo
								            new_cc_mapped.class_id,      -- Si tiene mapeo válido
								            inv_cc.cost_center_type      -- Si es cost center original
								        ) = cc_type.id
								        
								) pad ON p.project = pad.project_id;
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
