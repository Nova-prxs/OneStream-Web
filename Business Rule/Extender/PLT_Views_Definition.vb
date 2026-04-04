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

Namespace OneStream.BusinessRule.Extender.PLT_Views_Definition
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
					
				Dim sViewDrop As String = String.Empty
				Dim sViewCreate As String = String.Empty
				
				#Region "VIEW_PLT_FACT_Costs"
				
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_FACT_Costs As
					
						SELECT *
						FROM (
						    SELECT 
								F.scenario,
						        F.id_factory, 
						        F.id_costcenter, 
								F.id_account,
						        F.id_account + ' - ' + ISNULL(A.Description, '') AS account,
								A.account_type AS id_nature,
								F.id_rubric,
								RIGHT('0' + A.account_type,2) + ' - ' + ISNULL(N.Description, '') AS nature,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.value, 
						        CONCAT(F.id_account, ' | ', F.id_costcenter) AS account_costcenter
					
						    FROM XFC_PLT_FACT_Costs F
							LEFT JOIN XFC_PLT_MASTER_Account A
								ON F.id_account = A.id
							LEFT JOIN XFC_PLT_MASTER_NatureCost N
								ON A.account_type = N.id
	
						) AS SourceTable
						PIVOT (
						    SUM(value) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
						) AS PivotTable;
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region
				
				#Region "VIEW_PLT_FACT_Costs_Input"
				
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_FACT_Costs_Input As
					
						SELECT *
						FROM (
						    SELECT 
								F.scenario,
						        F.id_factory, 
						        F.id_costcenter, 
								F.id_account,
						        F.id_account + ' - ' + ISNULL(A.Description, '') AS account,
								A.account_type AS id_nature,
								RIGHT('0' + A.account_type,2) + ' - ' + ISNULL(N.Description, '') AS nature,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.value, 
						        CONCAT(F.id_account, ' | ', F.id_costcenter) AS account_costcenter
					
						    FROM XFC_PLT_FACT_Costs F
							LEFT JOIN XFC_PLT_MASTER_Account A
								ON F.id_account = A.id
							LEFT JOIN XFC_PLT_MASTER_NatureCost N
								ON A.account_type = N.id
	
						) AS SourceTable
						PIVOT (
						    SUM(value) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
						) AS PivotTable;
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region				
				
				#Region "VIEW_PLT_FACT_Costs_VTU"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_FACT_Costs_VTU As
					
					    SELECT
							F.scenario
							,F.date
					        ,F.id_factory 
							,F.id_product
					        ,ISNULL(P.Description, '') AS desc_product
					        ,F.VTU
							,F.VTU_corrected
							,CASE WHEN ISNULL(F.VTU_corrected,0) > 0 THEN ISNULL(F.VTU_corrected,0) ELSE ISNULL(F.VTU,0) END AS VTU_final
							,F.comment
				
					    FROM XFC_PLT_FACT_Costs_VTU F
						LEFT JOIN XFC_PLT_MASTER_Product P
							ON F.id_product = P.id;
	
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region				
				
				#Region "VIEW_PLT_FACT_Production_Volume"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_FACT_Production_Volume As
					
						SELECT *
						FROM (
						    SELECT 
								F.scenario,
						        F.id_factory, 
						        F.id_costcenter, 
								F.id_averagegroup, 
								F.id_workcenter, 
								F.id_product,
								'Volume' AS account,
								F.uotype,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.volume
					
						    FROM XFC_PLT_FACT_Production F								
	
						) AS SourceTable
						PIVOT (
						    SUM(volume) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
						) AS PivotTable;
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region			
				
				#Region "VIEW_PLT_FACT_Production_Allocation"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_FACT_Production_Allocation As
					
						SELECT *
						FROM (
					
						    SELECT 
								F.scenario,
						        F.id_factory, 
						        F.id_costcenter, 
								F.id_averagegroup, 
								F.id_workcenter, 
								F.id_product,
								'allocation_taj' AS account,
								F.uotype,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.allocation_taj
					
						    FROM XFC_PLT_FACT_Production F								
	
						) AS SourceTable
						PIVOT (
						    SUM(allocation_taj) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
						) AS PivotTable;
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region
				
				#Region "VIEW_PLT_FACT_Production_Activity"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_FACT_Production_Activity As
					
						SELECT *
						FROM (
					
						    SELECT 
								F.scenario,
						        F.id_factory, 
						        F.id_costcenter, 
								F.id_averagegroup, 
								F.id_workcenter, 
								F.id_product,
								'activity_taj' AS account,
								F.uotype,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.activity_taj
					
						    FROM XFC_PLT_FACT_Production F					
	
						) AS SourceTable
						PIVOT (
						    SUM(activity_taj) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
						) AS PivotTable;
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region	
				
				#Region "VIEW_PLT_AUX_Production_Planning_Volumes"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_Production_Planning_Volumes As
					
						SELECT *
						FROM (
						    SELECT 
								F.scenario,
						        F.id_factory, 
						        F.id_costcenter, 
								F.id_averagegroup, 
								F.id_product,
								ISNULL(P.description,'No Description') AS Product,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
								F.distribution,
						        F.value AS volume
					
						    FROM XFC_PLT_AUX_Production_Planning_Volumes F			
					
							LEFT JOIN XFC_PLT_MASTER_Product P
								ON F.id_product = P.id
	
						) AS SourceTable 
						PIVOT (
						    SUM(volume) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
						) AS PivotTable;
					"
									
					ExecuteSql(si, sViewCreate)
				
				#End Region					
				
				#Region "VIEW_PLT_AUX_Production_Planning_Volumes_Dist"
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_Production_Planning_Volumes_Dist As
					
						SELECT *
						FROM (
						    SELECT 
								F.scenario,
						        F.id_factory, 
						        F.id_costcenter, 
								F.id_averagegroup, 
								F.id_product,
								ISNULL(P.description,'No Description') AS Product,
								F.id_product_parent,
								F.id_product_final,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.value
					
						    FROM XFC_PLT_AUX_Production_Planning_Volumes_Dist F	
					
							LEFT JOIN XFC_PLT_MASTER_Product P
								ON F.id_product = P.id
	
						) AS SourceTable 
						PIVOT (
						    SUM(value) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12])
						) AS PivotTable;
					"
									
					ExecuteSql(si, sViewCreate)
				
				#End Region		
				
				#Region "VIEW_PLT_AUX_Production_Planning_Times"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_Production_Planning_Times As
					
						SELECT *
						FROM (
						    SELECT DISTINCT
								F.scenario,
						        F.id_factory, 
						        --F.id_costcenter, 
								F.id_averagegroup, 
								F.id_product,
								ISNULL(M.description,'No Description') AS Product,					
								F.uotype,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.value
								-- CASE WHEN F.scenario = 'RF06' THEN CAST(F.value AS DECIMAL(18,6)) * 60 ELSE F.value END AS value
					
						    FROM XFC_PLT_AUX_Production_Planning_Times F
					
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON F.id_product = M.id
					
							UNION ALL
					
						    SELECT DISTINCT
								REPLACE(F.scenario,'_Ref','') AS scenario,
						        F.id_factory, 
						        --F.id_costcenter, 
								F.id_averagegroup, 
								F.id_product,
								ISNULL(M.description,'No Description') AS Product,						
								F.uotype,
						        'R' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.value
					
						    FROM XFC_PLT_AUX_Production_Planning_Times F	
							
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON F.id_product = M.id
					
							WHERE scenario LIKE '%_Ref'
					

	
						) AS SourceTable
						PIVOT (
						    SUM(value) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12]
										 ,[R01], [R02], [R03], [R04], [R05], [R06], [R07], [R08], [R09], [R10], [R11], [R12])
						) AS PivotTable;
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region	
				
				#Region "VIEW_PLT_AUX_Production_Planning_Times_Minutes"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_Production_Planning_Times_Minutes As
					
						SELECT *
						FROM (
						    SELECT DISTINCT
								F.scenario,
						        F.id_factory, 
						        --F.id_costcenter, 
								F.id_averagegroup, 
								F.id_product,
								ISNULL(M.description,'No Description') AS Product,					
								F.uotype,
						        'M' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
						        F.value
								-- CASE WHEN F.scenario = 'RF06' THEN CAST(F.value AS DECIMAL(18,6)) * 60 ELSE F.value END AS value
					
						    FROM XFC_PLT_AUX_Production_Planning_Times F
					
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON F.id_product = M.id
					
							UNION ALL
					
						    SELECT DISTINCT
								REPLACE(F.scenario,'_Ref','') AS scenario,
						        F.id_factory, 
						        --F.id_costcenter, 
								F.id_averagegroup, 
								F.id_product,
								ISNULL(M.description,'No Description') AS Product,						
								F.uotype,
						        'R' + RIGHT('0' + CAST(MONTH(F.date) AS VARCHAR), 2) AS month, 
						        YEAR(F.date) AS year, 
								F.value
						        -- F.value * 60
					
						    FROM XFC_PLT_AUX_Production_Planning_Times F	
							
							LEFT JOIN XFC_PLT_MASTER_Product M
								ON F.id_product = M.id
					
							WHERE scenario LIKE '%_Ref'
					

	
						) AS SourceTable
						PIVOT (
						    SUM(value) 
						    FOR month IN ([M01], [M02], [M03], [M04], [M05], [M06], [M07], [M08], [M09], [M10], [M11], [M12]
										 ,[R01], [R02], [R03], [R04], [R05], [R06], [R07], [R08], [R09], [R10], [R11], [R12])
						) AS PivotTable;
					"
				
'					ExecuteSql(si, sViewCreate)
				
				#End Region	
				
				#Region "VIEW_PLT_AUX_Product_Type"
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_Product_Type As
					
						SELECT A.id, A.description, A.type, B.id_factory, CAST(0 AS BIT) as Modify
						
						FROM XFC_PLT_MASTER_Product A
							
						LEFT JOIN XFC_PLT_FACT_Production B		
							ON A.id = B.id_product
					
						GROUP BY A.id, A.description, A.type, B.id_factory
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region

				#Region "VIEW_PLT_AUX_Nomenclature"
					
					#Region "Functions"
					
						#Region "Nomenclature"
						
					Dim sViewCreate_f = "
						CREATE OR ALTER FUNCTION dbo.fn_Product_Nomenclature
						(
						    @productId NVARCHAR(50),
						    @factory   NVARCHAR(50),
							@dateFilter DATE
						)
						RETURNS TABLE
						AS
						RETURN
						    WITH Product_Component_Recursivity AS (
						        SELECT 
						            N.id_factory,
						            N.id_product AS id_product_final,
						            N.id_product,
						            N.id_component,
						            CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
						            CAST(N.prorata AS DECIMAL(18,6)) AS prorata,
						            CAST(N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
						            2 AS [Level]
						        FROM XFC_PLT_HIER_Nomenclature_Date N
						        WHERE N.id_product = @productId
						        	AND N.id_factory = @factory
								  	AND @dateFilter BETWEEN N.start_date AND N.end_date
						
						        UNION ALL
						
						        SELECT 
						            N.id_factory,
						            R.id_product_final,
						            N.id_product,
						            N.id_component,
						            CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
						            CAST(N.prorata AS DECIMAL(18,6)) AS prorata,
						            CAST(R.exp_coefficient * N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
						            R.[Level] + 1
						        FROM XFC_PLT_HIER_Nomenclature_Date N
						        INNER JOIN Product_Component_Recursivity R
						            ON N.id_product = R.id_component
						           	AND N.id_factory = R.id_factory
								  	AND @dateFilter BETWEEN N.start_date AND N.end_date
						    )
						    SELECT A.*, B.description 
							FROM Product_Component_Recursivity A
							LEFT JOIN XFC_PLT_MASTER_Product B
								ON A.id_component = B.id
							;
					"
					ExecuteSql(si, sViewCreate_f)
					#End Region
						
						#Region "Nomenclature Report"
						
					sViewCreate_f = "
						CREATE OR ALTER FUNCTION dbo.fn_Product_Nomenclature_Planning
						(
						    @productId NVARCHAR(50),
						    @factory   NVARCHAR(50),
						    @scenario   NVARCHAR(50),
							@dateFilter DATE
						)
						RETURNS TABLE
						AS
						RETURN
						    WITH Product_Component_Recursivity AS (
						        SELECT 
						            N.scenario,
									N.id_factory,
						            N.id_product AS id_product_final,
						            N.id_product,
						            N.id_component,
						            CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
						            CAST(N.prorata AS DECIMAL(18,6)) AS prorata,
						            CAST(N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
						            2 AS [Level]
						        FROM XFC_PLT_HIER_Nomenclature_Date_Planning N
						        WHERE N.id_product = @productId
						        	AND N.id_factory = @factory
						        	AND N.scenario = @scenario
								  	AND @dateFilter BETWEEN N.start_date AND N.end_date
						
						        UNION ALL
						
						        SELECT 
									N.scenario,
						            N.id_factory,
						            R.id_product_final,
						            N.id_product,
						            N.id_component,
						            CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
						            CAST(N.prorata AS DECIMAL(18,6)) AS prorata,
						            CAST(R.exp_coefficient * N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
						            R.[Level] + 1
						        FROM XFC_PLT_HIER_Nomenclature_Date_Planning N
						        INNER JOIN Product_Component_Recursivity R
						            ON N.id_product = R.id_component
						           	AND N.id_factory = R.id_factory
						           	AND N.scenario = R.scenario
								  	AND @dateFilter BETWEEN N.start_date AND N.end_date
						    )
						    SELECT A.*, B.description 
							FROM Product_Component_Recursivity A
							LEFT JOIN XFC_PLT_MASTER_Product B
								ON A.id_component = B.id
							;
					"
					ExecuteSql(si, sViewCreate_f)
					
					#End Region
					
					#Region "Nomenclature New Products"
					
					sViewCreate_f =	"CREATE OR ALTER FUNCTION dbo.fn_Product_Nomenclature_New
						(
						    @productId NVARCHAR(50),
						    @factory   NVARCHAR(50)
						)
						RETURNS TABLE
						AS
						RETURN
						    WITH Distinct_Nomenclature as (
								SELECT 
									id_factory
									, id_product
									, id_component
									, AVG(coefficient) as coefficient
					
								FROM XFC_PLT_AUX_NewProducts_Simulation
					
								WHERE 1=1
									AND (id_product_final <> id_component)
					
								GROUP BY 
									id_factory
									, id_product
									, id_component
							),
					
							Product_Component_Recursivity AS (
						        SELECT
						            N.id_factory,
						            N.id_product AS id_product_final,
						            N.id_product,
						            N.id_component,
						            CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
						            CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient,					
						            2 AS [Level]
					
						        FROM Distinct_Nomenclature N
						        WHERE N.id_product = @productId
						          AND N.id_factory = @factory
						
						        UNION ALL
						
						        SELECT 						            
						            N.id_factory,
						            R.id_product_final,
						            N.id_product,
						            N.id_component,
						            CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
						            CAST(R.exp_coefficient * N.coefficient AS DECIMAL(18,6)) AS exp_coefficient,
									R.[Level] + 1
					
						        FROM Distinct_Nomenclature N
						        INNER JOIN Product_Component_Recursivity R
						            ON N.id_product = R.id_component
						           AND N.id_factory = R.id_factory
						    )
						    SELECT A.*, B.description 
							FROM Product_Component_Recursivity A
							LEFT JOIN XFC_PLT_MASTER_Product B
								ON A.id_component = B.id
					
					;
					"
					ExecuteSql(si, sViewCreate_f)
					
						#End Region
						
					#End Region					
					
					#Region "View New Products"
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_NewProducts_Simulation AS
					
						SELECT 
							A.*
							, CAST(
								CASE 
									WHEN B.id IS NOT NULL THEN 1 
									ELSE 0
								END AS BIT
							) AS GM_Master
						
						FROM XFC_PLT_AUX_NewProducts_Simulation A
						
						LEFT JOIN XFC_PLT_MASTER_AverageGroup B
							ON A.id_averagegroup = B.id
					"
					ExecuteSql(si, sViewCreate)
					#End Region
					
					#Region "All Nomenclature"
					
					sViewDrop = "
					IF EXISTS (SELECT 1 FROM sys.views WHERE name = 'VIEW_PLT_AUX_Nomenclature')
						DROP VIEW IF EXISTS VIEW_PLT_AUX_Nomenclature"
					ExecuteSql(si, sViewDrop)
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_Nomenclature AS
						
						WITH Product_Component_Recursivity AS (
							
							-- Nivel raíz: todos los productos raíz posibles
							SELECT 
								N.id_factory,
								N.id_product AS id_product_final,
								N.id_product,
								N.id_component,
								CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
								CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient,
								2 AS Level
							FROM XFC_PLT_HIER_Nomenclature N
						
							UNION ALL
						
							-- Recursividad
							SELECT 
								N.id_factory,
								R.id_product_final,
								N.id_product,
								N.id_component,
								CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
								CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient,
								R.Level + 1
							FROM XFC_PLT_HIER_Nomenclature N
							INNER JOIN Product_Component_Recursivity R
								ON N.id_product = R.id_component
								AND N.id_factory = R.id_factory
						)
						
						SELECT * FROM Product_Component_Recursivity
						
						UNION ALL
						
						SELECT 
							id_factory
							, id_product_final
							, id_product_final
							, id_product_final
							, 1 AS coefficient
							, 1 AS exp_coefficient
							, 1 AS Level
					
						FROM Product_Component_Recursivity
					
						GROUP BY id_factory, id_product_final
					"
				
					ExecuteSql(si, sViewCreate)
					#End Region
					
				#End Region
				
				#Region "VIEW_PLT_AUX_Costs_Init_GM_Criteria"				
					
					sViewCreate = "
						CREATE OR ALTER VIEW VIEW_PLT_AUX_Costs_Init_GM_Criteria As
					
					    SELECT 
							F.scenario,
							F.year,
					        F.id_factory, 						         
							F.id_account,
					        F.id_account + ' - ' + ISNULL(A.Description, 'No Description') AS account,
							A.account_type AS id_nature,
							RIGHT('0' + A.account_type,2) + ' - ' + ISNULL(N.Description, 'No Description') AS nature,
					        F.criteria,
							F.inflation,
							CAST(0 AS BIT) AS modify
											
					    FROM XFC_PLT_AUX_Costs_Init_GM_Criteria F
						LEFT JOIN XFC_PLT_MASTER_Account A
							ON F.id_account = A.id
						LEFT JOIN XFC_PLT_MASTER_NatureCost N
							ON A.account_type = N.id
					"
				
					ExecuteSql(si, sViewCreate)
				
				#End Region				
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                               
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using   
				
        End Sub						
		
	End Class
	
End Namespace