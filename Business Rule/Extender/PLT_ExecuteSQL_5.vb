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

Namespace OneStream.BusinessRule.Extender.PLT_ExecuteSQL_5
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sQuery1 As String = "		

			-- 1. RAW	
				
			DELETE 
			FROM XFC_PLT_RAW_CostsMonthly
			WHERE id_factory = '0671'
			AND entity LIKE 'STE'
			AND year = '2025'
			AND month = '7'
			AND num_cpte IN 	(SELECT DISTINCT name_old
								FROM XFC_MAIN_MASTER_AccountsOldToNew
								WHERE name IN ('61213000','61214100','61214500','61215300','61215500','61216900'));	
				
			WITH filtered_cost_centers AS (
				SELECT *
				FROM XFC_MAIN_MASTER_CostCenters
				WHERE end_date > GETDATE() AND (is_blocked <> 1 OR is_blocked IS NULL)
			)
			
			, filtered_account_rubrics AS (
				SELECT
					cc.id AS cost_center_id, ccon.id_old AS cost_center_id_old, ar.cost_center_class_id,
					ar.account_name, ar.conso_rubric
				FROM filtered_cost_centers cc
				LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew ccon
					ON cc.id = ccon.id
				LEFT JOIN XFC_MAIN_MASTER_AccountRubrics ar
					ON cc.class_id = ar.cost_center_class_id		
			)							
				
			INSERT INTO XFC_PLT_RAW_CostsMonthly
				([year]
				,[month]
				,[id_factory]
				,[entity]
				,[domFonc]
				,[code]
				,[rubirc]
				,[activity]
				,[society]
				,[cbl_account]
				,[value_intern]
				,[value_transact]
				,[dev]
				,[documentF]
				,[num_cpte]
				,[id_costcenter]
				,[num_ordre]
				,[elm_OTP])
			
			SELECT
			    F.year
			    ,F.month
       			,'0671' AS [id_factory]
       			,'STE' AS [entity]
				,NULL AS [domFonc]
				,NULL AS [code]
				,ISNULL(far.conso_rubric,'-1') AS [rubirc]
				,NULL AS [activity]
				,NULL AS [society]
				,NULL AS [cbl_account]	
				,F.amount AS [value_intern]
				,F.amount AS [value_transact]
				,NULL AS [dev]		
				,NULL AS [documentF]	
		
				,CASE 
					WHEN R.account_name IS NOT NULL THEN 
						CASE WHEN M.cnt_account IS NULL AND A.name_old IS NOT NULL THEN F.account_name + ' - ' + A.name_old + ' - No Mapping Mng Account'
						ELSE ISNULL(A.name_old, F.account_name + ' - No Mapping') 
						END 
					ELSE F.account_name + ' - Ignore' 
					END AS [num_cpte]	
		
			    ,CASE 
					WHEN F.cost_center_id = 'none' THEN NULL
					WHEN F.cost_center_id IS NOT NULL THEN ISNULL(C.id_old, F.cost_center_id + ' - No Mapping') 
					END AS [id_costcenter]
				
				,NULL AS [num_ordre]
				,NULL AS [elm_OTP]
		
			FROM XFC_MAIN_FACT_PnLTransactions F
		
			LEFT JOIN XFC_MAIN_MASTER_AccountsOldToNew A
				ON A.name = F.account_name		
			
			LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew C
				ON C.id = F.cost_center_id
		
            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C2
             	ON C2.id = C.id_old
          		AND C2.scenario = 'Actual'
          		AND DATEFROMPARTS(2025,7, 1) BETWEEN C2.start_date AND C2.end_date						
		
			LEFT JOIN (SELECT DISTINCT account_name FROM XFC_MAIN_MASTER_AccountRubrics) R
				ON R.account_name = F.account_name	
		
			LEFT JOIN (SELECT DISTINCT cnt_account FROM XFC_PLT_MAPPING_Accounts_DocF) M
				ON A.name_old = M.cnt_account	
		
			LEFT JOIN filtered_account_rubrics AS far
				ON F.account_name = far.account_name 
				AND F.cost_center_id = far.cost_center_id		

			WHERE F.company_id = 'PT10'						
				AND F.account_name IN ('61213000','61214100','61214500','61215300','61215500','61216900')
				
	"
				
								Dim sQuery2 As String = "	
				
								-- 2. FACT
				
								DELETE FROM XFC_PLT_FACT_Costs
								WHERE 1=1
									AND id_factory = 'R0671'
									AND scenario = 'Actual'
									AND YEAR(date) = 2025						
									AND MONTH(date) = 7
				
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_factory, date, id_account, id_rubric, id_costcenter, value, currency)
								
								SELECT *
								FROM (  
							
									SELECT  
										'Actual' as scenario
									    , 'R0671' AS id_factory
									    , DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date
									    , mng_account as id_account
										, ISNULL(rubirc,'-1') AS id_rubric
									    , ISNULL(id_costcenter,'-1') as id_costcenter
									    , SUM(value_intern) as value
									    , 'Local' as currency
							
									FROM (
								      		SELECT A.* , COALESCE(B.mng_account,'Others') as mng_account
						
							                FROM (	SELECT *, 
														CASE 
											                WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
															END AS id_factory_map
													FROM XFC_PLT_RAW_CostsMonthly) A
		
								            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								             	ON A.id_costcenter = C.id
								          		AND C.scenario = 'Actual'
								          		AND DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) BETWEEN C.start_date AND C.end_date							
						
											LEFT JOIN (
														SELECT  
															cnt_account																
															, costcenter_type
															, costcenter_nature
															, MAX(mng_account) AS mng_account
														FROM XFC_PLT_Mapping_Accounts_docF 
														GROUP BY cnt_account, costcenter_type, costcenter_nature		
													) as B					
								       			ON A.num_cpte = B.cnt_account
												AND C.type = B.costcenter_type
												AND C.nature = B.costcenter_nature
												--AND A.domFonc = B.docF								
		 
								      		WHERE 1=1
												AND CAST(A.[month] as INT) = 7										
												AND A.year = 2025
											    AND (id_factory_map = 'R0671')
												
								      		)  AS ext
						
								      GROUP BY id_factory_map, [year], [month], mng_account, rubirc, id_costcenter
						
								) AS mapeo	
								
								UNION ALL
							
								-- Exception (Without CC): 78.Scraps
							
								SELECT *
								FROM (  
							
									SELECT  
										'Actual' as scenario
									    , '{factory}' AS id_factory
									    , DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date
									    , mng_account as id_account
										, rubirc
									    , ISNULL(id_costcenter,'-1') as id_costcenter
									    , SUM(value_intern) as value
									    , 'Local' as currency
							
									FROM (
								      		SELECT A.* 
											, CASE
												WHEN num_cpte = '703100' THEN '2820' 
											  END as mng_account
						
							                FROM (	SELECT *, 
														CASE 
											                WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
															END AS id_factory_map
													FROM XFC_PLT_RAW_CostsMonthly
							
													WHERE num_cpte IN ('703100') AND id_costcenter IS NULL) A
		 
								      		WHERE 1=1
												AND CAST(A.[month] as INT) = 7										
												AND A.year = 2025
											    AND (id_factory_map = 'R0671')
								      		)  AS ext
						
								      GROUP BY id_factory_map, [year], [month], mng_account, rubirc, id_costcenter
						
								) AS mapeo								
							
							
				"				

				'ExecuteSql(si, sQuery1)
				
				ExecuteSql(si, sQuery2)
				
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
		
        Private Function ExecuteSqlOtherAPP(ByVal si As SessionInfo, ByVal sqlCmd As String, ByVal OtherApp As String) As DataTable
		
			Dim oar As New OpenAppResult
			Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, OtherApp, oar)
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(osi)
                               
                Dim dt As DataTable = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)
				Return dt
				
            End Using   
			
			Return Nothing
				
        End Function					
		
	End Class
	
End Namespace