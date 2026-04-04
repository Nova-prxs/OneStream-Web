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

Namespace OneStream.BusinessRule.Extender.PLT_Functions_Definition
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				' Funciones para VTU - Costs Calculations	
				Dim sFunctionCreate As String = String.Empty
				
				sFunctionCreate = "
					CREATE FUNCTION dbo.PLT_GetMappedFactory (@prm_PLT_factory VARCHAR(50))
					RETURNS VARCHAR(50)
					AS
					BEGIN
					    DECLARE @MappedValue VARCHAR(50);
					
					    -- Asignar el valor correspondiente según el valor de @prm_PLT_factory
					    SET @MappedValue = 
					        CASE 
					            WHEN @prm_PLT_factory = 'R0671' THEN '0671STE'
					            WHEN @prm_PLT_factory = 'R0548913' THEN '1301003'
					            WHEN @prm_PLT_factory = 'R0548914' THEN '1301002'
					            WHEN @prm_PLT_factory = 'R0483003' THEN '1303%' 
					            WHEN @prm_PLT_factory = 'R0529002' THEN '1301003'
					            WHEN @prm_PLT_factory = 'R0045106' THEN '1301003'
					            WHEN @prm_PLT_factory = 'R0585' THEN '1301003'
					            WHEN @prm_PLT_factory = 'R0592' THEN '1301003'
					            ELSE ''  -- Puedes definir qué hacer cuando no haya coincidencia
					        END;
					
					    -- Retorna el valor mapeado
					    RETURN @MappedValue;
					END;

				"
				ExecuteSql(si, sFunctionCreate)
				
				#Region "VTU Old Functions"
				
'				' OWN - Periodic	
'				sFunctionCreate = "													
'					CREATE OR ALTER FUNCTION dbo.fn_PLT_VTU_Own_Periodic ()
'					RETURNS TABLE
'					AS
'					RETURN
'					(
'					    SELECT
'							F.id_factory
'					      	, MONTH(F.[date]) as [month]
'					      	, FA.[description] as desc_factory
'							, F.id_averagegroup
'					      	, F.id_product
'					      	, M.[description] as desc_product
'							, NULL as id_component
'					      	, CASE WHEN F.account_type BETWEEN 1 AND 5
'					             THEN RIGHT('0' + CAST(F.account_type AS varchar(2)),2) + '-' + ISNULL(N.[description],'')
'					             ELSE '99-Others'
'							  END as nature
'					      	, v.variability
'					      	, CASE WHEN F.volume = 0 OR F.activity_UO1 = 0
'					             THEN 0
'					             ELSE v.cost_amount / F.volume * F.activity_UO1 / F.activity_UO1_total
'					          END as VTU_Periodic
'					      	, CAST(0 AS decimal(18,6)) as VTU_YTD
				
'					    FROM dbo.XFC_PLT_FACT_Costs_VTU_Report F
				
'					    CROSS APPLY (VALUES
'					                    ('F', F.cost_fixed),
'					                    ('V', F.cost_variable)
'					                ) v(variability, cost_amount)
				
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Factory FA ON FA.id = F.id_factory 
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Product M ON M.id = F.id_product
'					    LEFT JOIN dbo.XFC_PLT_MASTER_NatureCost N ON N.id = F.account_type
'					);			
'				"
						
'				ExecuteSql(si, sFunctionCreate)
				
'				' OWN - YTD					
'				sFunctionCreate = "
'					CREATE OR ALTER FUNCTION dbo.fn_PLT_VTU_Own_YTD ()
'					RETURNS TABLE
'					AS
'					RETURN
'					(
'					    SELECT
'					      	F.id_factory
'					     	, MONTH(MO.[date]) AS [month]
'					      	, FA.[description] AS desc_factory
'							, F.id_averagegroup
'					      	, F.id_product
'					      	, M.[description] AS desc_product
'						  	, NULL as id_component
'					      	, CASE
'					      	      WHEN F.account_type BETWEEN 1 AND 5
'					      	          THEN RIGHT('0' + CAST(F.account_type AS varchar(2)),2) + '-' + ISNULL(N.[description],'')
'					      	          ELSE '99-Others'
'					      	  END AS nature
'					      	, v.variability
'					      	, CAST(0 AS decimal(18,6)) AS VTU_Periodic
'					      	, CASE
'					      	      WHEN SUM(F.volume) = 0 OR SUM(F.activity_UO1) = 0
'					      	          THEN 0
'					      	          ELSE SUM(v.cost_amount) / SUM(F.volume) * SUM(F.activity_UO1) / SUM(F.activity_UO1_total)
'					      	  END AS VTU_YTD
				
'					    FROM (SELECT DISTINCT [date] FROM dbo.XFC_PLT_FACT_Costs_VTU_Report) MO
				
'					    LEFT JOIN dbo.XFC_PLT_FACT_Costs_VTU_Report F ON F.[date] <= MO.[date]
				
'					    CROSS APPLY (VALUES
'					                    ('F', F.cost_fixed),
'					                    ('V', F.cost_variable)
'					                ) v(variability, cost_amount)
				
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Factory FA ON FA.id = F.id_factory
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Product M ON M.id = F.id_product
'					    LEFT JOIN dbo.XFC_PLT_MASTER_NatureCost N ON N.id = F.account_type 
				
'					    GROUP BY
'					        F.id_factory
'					      , MONTH(MO.[date])
'					      , FA.[description]
'						  , F.id_averagegroup
'					      , F.id_product
'					      , M.[description]
'					      , CASE
'					            WHEN F.account_type BETWEEN 1 AND 5
'					                THEN RIGHT('0' + CAST(F.account_type AS varchar(2)),2) + '-' + ISNULL(N.[description],'')
'					                ELSE '99-Others'
'					        END
'					      , v.variability
'					);
'									"
'				ExecuteSql(si, sFunctionCreate)
				
'				' COMPONENTS - PERIODIC					
'				sFunctionCreate = "
'					CREATE OR ALTER FUNCTION dbo.fn_PLT_VTU_Comp_Periodic
'					(
'					    @scenario 	NVARCHAR(20)
'					  , @year     	INT
'					  , @monthFcst	DATE
'					)
'					RETURNS TABLE
'					AS
'					RETURN
'					(
'					    SELECT 
'					        P.id_factory
'					      , MONTH(P.[date]) AS [month]
'					      , FA.[description] AS desc_factory
'					      , F.id_averagegroup
'					      , P.id_product_final AS id_product
'					      , M.[description] AS desc_product
'					      , P.id_component
'					      , CASE
'					            WHEN F.account_type BETWEEN 1 AND 5
'					                THEN RIGHT('0' + CAST(F.account_type AS varchar(2)),2) + '-' + ISNULL(N.[description],'')
'					                ELSE '99-Others'
'					        END AS nature
'					      , v.variability
'					      , CASE
'					            WHEN F.volume = 0 OR F.activity_UO1 = 0
'					                THEN 0
'					                ELSE v.cost_amount / F.volume * F.activity_UO1 / F.activity_UO1_total * P.exp_coefficient
'					        END AS VTU_Periodic
'					      , CAST(0 AS decimal(18,6)) AS VTU_YTD
						
'					    FROM dbo.XFC_PLT_HIER_Nomenclature_Date_Report P
						
'					    LEFT JOIN dbo.XFC_PLT_AUX_Product_Pivot PI
'					    	ON PI.id_product = P.id_component
'					    	AND PI.scenario   = @scenario
'					    	AND PI.[year]     = @year
'					    	AND PI.id_factory  = P.id_factory
						
'					    LEFT JOIN dbo.XFC_PLT_FACT_Costs_VTU_Report F
'					    	ON F.id_factory = P.id_factory
'					        AND F.[date] = P.[date]
'					     	AND F.id_product = CASE WHEN F.[date] >= @monthFcst THEN ISNULL(PI.id_product_mapping, P.id_component)
'					                            ELSE P.id_component
'					                           END
						
'					    CROSS APPLY (VALUES
'					                    ('F', F.cost_fixed),
'					                    ('V', F.cost_variable)
'					                ) v(variability, cost_amount)
						
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Factory  FA ON FA.id = F.id_factory
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Product  M  ON M.id = P.id_product_final
'					    LEFT JOIN dbo.XFC_PLT_MASTER_NatureCost N ON N.id = F.account_type
'					);					
'									"
'				ExecuteSql(si, sFunctionCreate)
				
'				' COMPONENTS - YTD					
'				sFunctionCreate = "
'					CREATE OR ALTER FUNCTION dbo.fn_PLT_VTU_Comp_YTD
'					(
'					    @scenario	NVARCHAR(20)
'					  , @year		INT
'					  , @monthFcst	DATE
'					)
'					RETURNS TABLE
'					AS
'					RETURN
'					(
'					    SELECT
'					        P.id_factory
'					      , MONTH(P.[date]) AS [month]
'					      , FA.[description] AS desc_factory
'					      , F.id_averagegroup
'					      , P.id_product_final AS id_product
'					      , M.[description] AS desc_product
'					      , P.id_component
'					      , CASE
'					            WHEN F.account_type BETWEEN 1 AND 5
'					                THEN RIGHT('0' + CAST(F.account_type AS varchar(2)),2) + '-' + ISNULL(N.[description],'')
'					                ELSE '99-Others'
'					        END AS nature
'					      , v.variability
'					      , CAST(0 AS decimal(18,6)) AS VTU_Periodic
'					      , CASE
'					            WHEN SUM(F.volume) = 0 OR SUM(F.activity_UO1) = 0
'					                THEN 0
'					                ELSE SUM(v.cost_amount) / SUM(F.volume) * SUM(F.activity_UO1) / SUM(F.activity_UO1_total) * P.exp_coefficient
'					        END AS VTU_YTD
				
'					    FROM dbo.XFC_PLT_HIER_Nomenclature_Date_Report P
				
'					    LEFT JOIN dbo.XFC_PLT_AUX_Product_Pivot PI
'					          ON PI.id_product = P.id_component 
'					          AND PI.scenario   = @scenario
'					          AND PI.[year]     = @year
'					          AND PI.id_factory  = P.id_factory
				
'					    LEFT JOIN dbo.XFC_PLT_FACT_Costs_VTU_Report F
'					          ON F.id_factory = P.id_factory
'					          AND F.[date] <= P.[date]
'					          AND F.id_product = CASE
'					                                WHEN F.[date] >= @monthFcst
'					                                    THEN ISNULL(PI.id_product_mapping, P.id_component)
'					                                    ELSE P.id_component
'					                              END
'					    CROSS APPLY (VALUES
'					                    ('F', F.cost_fixed),
'					                    ('V', F.cost_variable)
'					                ) v(variability, cost_amount)
				
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Factory FA ON FA.id = F.id_factory
'					    LEFT JOIN dbo.XFC_PLT_MASTER_Product M  ON M.id = P.id_product_final
'					    LEFT JOIN dbo.XFC_PLT_MASTER_NatureCost N ON N.id = F.account_type
				
'					    GROUP BY
'					        P.id_factory
'					      , MONTH(P.[date])
'					      , FA.[description]
'					      , F.id_averagegroup
'					      , P.id_product_final
'					      , M.[description]
'					      , P.id_component
'					      , P.exp_coefficient
'					      , CASE
'					            WHEN F.account_type BETWEEN 1 AND 5
'					                THEN RIGHT('0' + CAST(F.account_type AS varchar(2)),2) + '-' + ISNULL(N.[description],'')
'					                ELSE '99-Others'
'					        END
'					      , v.variability
'					);
'									"
'				ExecuteSql(si, sFunctionCreate)
				
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