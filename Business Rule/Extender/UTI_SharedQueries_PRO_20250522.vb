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
Imports DocumentFormat.OpenXml
Imports DocumentFormat.OpenXml.Packaging
Imports DocumentFormat.OpenXml.Spreadsheet

Namespace OneStream.BusinessRule.Extender.UTI_SharedQueries_PRO_20250522
	
	Public Class shared_queries
	
        Public Function AllQueries(
						ByVal si As SessionInfo,
						ByVal query As String, _
						Optional factory As String = "", _
						Optional month As String = "", _
						Optional year As String = "", _
						Optional scenario As String = "Actual", _
						Optional scenarioRef As String = "", _						
						Optional time As String = "" , _
						Optional view As String = "", _	
						Optional type As String = "", _
						Optional product As String = "", _
						Optional currency As String = "", _
						Optional monthFCT As String = "2"
						) As Object
            Try
				
				Dim sql As String = String.Empty
				
				#Region "Cost Distribution"				
					
					#Region "All"
				If query = "DistribucionCostes"
					
					sql = $"
							
							WITH accountMapping as (
								SELECT 
									account_type
									, id as mng_account
							
								FROM XFC_PLT_MASTER_Account
							),
							
							costsData as (
							
								SELECT
									B.nature
									, A.id_costcenter
									, B.id_averagegroup as id_averagegroup
									, A.id_account
									, C.account_type
									, SUM(A.value) as TotalAmount
								
								FROM XFC_PLT_FACT_Costs A
								
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
									ON A.id_costcenter = B.id
									AND B.scenario = 'Actual'
									AND A.date BETWEEN B.start_date AND B.end_date
								
								LEFT JOIN accountMapping C
									ON A.id_account = C.mng_account
								
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND B.type = 'A'
									AND A.id_account <> 'Others'
								
								GROUP BY A.id_costcenter, B.id_averagegroup, B.nature, A.id_account, C.account_type
					
								HAVING NULLIF(SUM(A.value),0) <> 0
							),
					
							othSharedCosts as (
							
								SELECT
									B.nature as nature
									, B.id_averagegroup as id_averagegroup
									, A.id_costcenter
									, A.id_account
									, C.account_type
									, SUM(A.value) as TotalAmount
									, B.id_averagegroup as destination
									, '-1' as id_product
									, '0' as KeyAllocation
									, '0' as ActivityWeight_TAJ 
									, '0' as ActivityWeight_TSO 
									, '0' as AuxiliarWeight_TAJ
									, '0' as AuxiliarWeight_TSO
									, SUM(A.value) as SharedCost_TAJ
									, '0' as SharedCost_TSO
					
								FROM XFC_PLT_FACT_Costs A
								
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
									ON A.id_costcenter = B.id
									AND B.scenario = 'Actual'
									AND A.date BETWEEN B.start_date AND B.end_date
								
								LEFT JOIN accountMapping C
									ON A.id_account = C.mng_account
								
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND B.type <> 'A'
									AND A.id_account <> 'Others'
								
								GROUP BY B.nature, A.id_costcenter, B.id_averagegroup, A.id_account, C.account_type
								
								HAVING NULLIF(SUM(A.value),0) <> 0
							),
						
					-- CP ------------------------------------------------------------------------------------
							-- Actividad por cada Grupo y CeCo
							totalActivityGMCeCo AS (
								SELECT 
									MONTH(date) as mes,
									id_averagegroup,
									id_costcenter,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
									COALESCE(SUM(activity_tso), 0) AS total_activity_tso
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, id_costcenter, uotype, MONTH(date)
								),
								
								-- Pesos CP dividiendo por el total de actividad
								productionData AS (
								SELECT 
									A.id_averagegroup
									, A.id_costcenter
									, A.id_product
									, A.uotype
									, SUM(A.activity_taj) as activity_taj
									, SUM(A.activity_tso) as activity_tso
									, COALESCE(SUM(A.activity_taj) / NULLIF(B.total_activity_taj, 0), 0) AS activity_weight_taj
									, COALESCE(SUM(A.activity_tso) / NULLIF(B.total_activity_tso, 0), 0) AS activity_weight_tso
								
								FROM XFC_PLT_FACT_Production A
								
								LEFT JOIN totalActivityGMCeCo B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.id_costcenter = B.id_costcenter
									AND A.uotype = B.uotype
									AND MONTH(A.date) = B.mes
								
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
								
								GROUP BY MONTH(A.date), A.id_averagegroup, A.id_costcenter, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
							),
							
							cpSharedCosts as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin 
									, A.id_costcenter as CeCo 
									, A.id_account
									, A.account_type
									, A.TotalAmount  
									, A.id_averagegroup as destination
									, B.id_product
									, '0' as KeyAllocation
									, B.activity_weight_taj as ActivityWeight_TAJ 
									, B.activity_weight_tso as ActivityWeight_TSO 
									, '0' as AuxiliarWeight_TAJ
									, '0' as AuxiliarWeight_TSO
									, A.TotalAmount*B.activity_weight_taj as SharedCost_TAJ
									, A.TotalAmount*B.activity_weight_tso as SharedCost_TSO
								
								FROM costsData A
								
								LEFT JOIN productionData B
									ON A.id_averagegroup = B.id_averagegroup
									AND ( A.id_costcenter = B.id_costcenter OR B.id_costcenter = '-1')
									AND (NULLIF(activity_weight_taj,0) <> 0 OR NULLIF(activity_weight_tso,0) <> 0)
					
								WHERE 1=1
									AND A.nature = 'CP'
									AND ((A.account_type = 1 and B.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and B.uotype = 'UO2') OR (B.uotype IS NULL))  	
									
							),
					
					-- CAT ------------------------------------------------------------------------------------
							-- Actividad por cada Grupo
							totalActivityGM AS (
								SELECT 
									MONTH(date) as mes,
									id_averagegroup,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
									COALESCE(SUM(activity_tso), 0) AS total_activity_tso
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, uotype, MONTH(date)
								),
							
							-- Pesos CAT dividiendo por la actividad total de GM
							catWeights as (
								SELECT 
									A.id_averagegroup
									, A.id_product
									, A.uotype
									, CASE
										WHEN B.total_activity_taj = 0 THEN 0
										ELSE SUM(A.activity_taj) / B.total_activity_taj 
									END AS activity_weight_cat_taj
									, CASE 
										WHEN B.total_activity_tso = 0 THEN 0 
										ELSE SUM(A.activity_tso) / B.total_activity_tso 
									END AS activity_weight_cat_tso
							
								FROM XFC_PLT_FACT_Production A
					
								LEFT JOIN totalActivityGM B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.uotype = B.uotype
							
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND NULLIF(A.id_averagegroup,'') <> ''
							
								GROUP BY A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
							),														
							
							catAllocationKeysTotal as (
								SELECT 
									scenario
									, id_factory
									, date
									, id_costcenter
									, costnature
									, SUM(percentage) as percentageTotal

								FROM XFC_PLT_AUX_AllocationKeys
								
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
								
								GROUP BY scenario, id_factory, date, id_costcenter, costnature			
							), 
					
							catAllocationKeys as (
								SELECT 
									A.scenario
									, A.id_factory
									, A.date
									, A.id_costcenter
									, A.id_averagegroup
									, A.costnature
									, A.percentage / B.percentageTotal as percentage

								FROM XFC_PLT_AUX_AllocationKeys A
					
								LEFT JOIN catAllocationKeysTotal B
									ON 	A.scenario = B.scenario
									AND	A.id_factory = B.id_factory 	
									AND	A.date = B.date 	
									AND	A.id_costcenter = B.id_costcenter 
									AND	A.costnature = B.costnature 	
								
								WHERE 1=1
									AND A.scenario = '{scenario}'
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
								
								GROUP BY A.scenario, A.id_factory, A.date, A.id_costcenter, A.id_averagegroup, A.costnature, A.percentage, B.percentageTotal
							),
								
							catSharedCostsGM as (
								SELECT 
									A.nature
									, A.id_averagegroup
									, A.id_costcenter
									, A.id_account
									, A.account_type
									, A.TotalAmount
									, B.id_averagegroup as destination
									, B.percentage as percentage
									, A.TotalAmount*(B.percentage) as GM_SharedCosts
									, ISNULL(B.id_averagegroup, '-1') as filterAcc
					
								FROM costsData A								
				
								LEFT JOIN catAllocationKeys B
									ON A.id_costcenter = B.id_costcenter
									AND A.account_type = B.costnature
									AND B.scenario = '{scenario}'		
									AND B.id_factory = '{factory}'
									AND YEAR(B.date) = {year}
									AND MONTH(B.date) = {month}
					
								WHERE 1=1									
									AND A.nature = 'CAT'

							),
							
							catSharedCosts as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.TotalAmount
									, A.destination
									, B.id_product
									, A.percentage as KeyAllocation -- GM_Weight_TAJ o TSO
									, B.activity_weight_cat_taj as ActivityWeight_TAJ  -- GM | REF
									, B.activity_weight_cat_tso as ActivityWeight_TSO -- GM | REF
									,'0' as AuxiliarWeight_TAJ -- GM_Weight_TAJ
									,'0' as AuxiliarWeight_TSO -- GM_Weight_TAJ
									, A.TotalAmount*A.percentage*B.activity_weight_cat_taj as SharedCosts_TAJ
									, A.TotalAmount*A.percentage*B.activity_weight_cat_tso as SharedCosts_TSO
							
								FROM catSharedCostsGM A
							
								LEFT JOIN catWeights B
									ON A.destination = B.id_averagegroup
							
								WHERE 1=1
									AND A.filterAcc <> '-1'
									AND ((A.account_type = 1 and B.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and B.uotype = 'UO2'))  							
							
							),							
														
					-- CAA ------------------------------------------------------------------------------------
					
							caaSharedCosts_aux1 as (
					
								SELECT 	nature
										, id_averagegroup
										, id_costcenter
										, id_account
										, account_type
										, SUM(TotalAmount) as TotalAmount
															
								FROM (
								
									-- CAA SharedCosts
										SELECT 
											A.nature
											, A.id_averagegroup
											, A.id_costcenter
											, A.id_account
											, A.account_type
											, A.TotalAmount
																	
										FROM costsData A
																																								
										WHERE 1=1
											AND A.nature = 'CAA'
										
									
										UNION
									
									-- CAT costs to be Shared
						    		    SELECT
											'CAA_CAT' as Nature
											, B.id_averagegroup as id_averagegroup
											, B.id_costcenter as id_costcenter
											, B.id_account as id_account
											, B.account_type as account_type
											, COALESCE(SUM(B.GM_SharedCosts),SUM(TotalAmount)) as TotalAmount
									        
									  	FROM catSharedCostsGM B
										
										LEFT JOIN (
												SELECT id_averagegroup, '1' as Exist					
												FROM catWeights					
												GROUP BY id_averagegroup
											) as D
											ON B.destination = D.id_averagegroup
										
										WHERE 1=1
											AND ((D.Exist IS NULL) OR B.filterAcc = '-1')											
																
										GROUP BY B.id_averagegroup, B.id_costcenter, B.id_account, B.account_type
																	
									 	UNION
									
									-- CP costs to be Shared
						    		   SELECT
											'CAA_CP' as Nature
											, C.Origin as id_averagegroup
											, C.CeCo as id_costcenter
											, C.id_account as id_account
											, C.account_type as account_type
											, C.TotalAmount as TotalAmount
									       
									 	FROM cpSharedCosts C
										
										WHERE 1=1
											AND (C.id_product IS NULL)
																
										GROUP BY C.Origin, C.CeCo, C.id_account, C.account_type, C.TotalAmount 
								
									) as caa_cpcat
					
								GROUP BY nature, id_averagegroup, id_costcenter,id_account, account_type
														    
							),
					
							-- Pesos CAA agrupación costes totales					
							costsShared_CP_CAT as (
								SELECT 
									destination
									, SUM(SharedCost_TAJ) as SharedCost_TAJ
									, SUM(SharedCost_TSO) as SharedCost_TSO
					
								FROM (
									SELECT *
																	
									FROM cpSharedCosts A
									
									WHERE 1=1
										AND account_type <> '05'
										AND A.id_product IS NOT NULL
					
									UNION
								
									SELECT *
									
									FROM catSharedCosts B
							
									WHERE 1=1
										AND account_type <> '05'
										-- AND (B.id_product IS NOT NULL)
					
								) as cp_cat	
								
								GROUP BY destination
							),
					
							costsShared_CP_CAT_Total as (
								SELECT 
									SUM(SharedCost_TAJ) AS TotalTAJ
									, SUM(SharedCost_TSO) AS TotalTSO
								
								FROM costsShared_CP_CAT
					
							),
					
							caaWeight as (
								SELECT 
									destination as id_averagegroup
									, CASE 
										WHEN SUM(B.TotalTAJ) = 0 THEN 0 
										ELSE SUM(SharedCost_TAJ) / SUM(B.TotalTAJ)
									END as caa_weight_taj
									, CASE	
										WHEN SUM(B.TotalTSO) = 0 THEN 0 
										ELSE SUM(SharedCost_TSO) / SUM(B.TotalTSO) 
									END as caa_weight_tso
								
								FROM costsShared_CP_CAT A
								
								CROSS JOIN costsShared_CP_CAT_Total B
									
								GROUP BY destination
								
							),
							
							caaSharedCosts as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.TotalAmount
									, B.id_averagegroup as Destination
									, C.id_product
									, '0' as KeyAllocation
									, C.activity_weight_cat_taj as ActivityWeight_TAJ
									, C.activity_weight_cat_tso as ActivityWeight_TSO
									, B.caa_weight_taj as AuxiliarWeight_TAJ
									, B.caa_weight_taj as AuxiliarWeight_TSO
									, A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_taj as SharedCosts_TAJ
									, A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_tso as SharedCosts_TSO									
							
								FROM caaSharedCosts_aux1 A
							
								CROSS JOIN caaWeight B
																
								LEFT JOIN catWeights C
									ON B.id_averagegroup = C.id_averagegroup
														
								WHERE 1=1
									AND ((A.account_type = 1 and C.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and C.uotype = 'UO2'))
																		
							),
								
							lastTable as (
								SELECT *							
								FROM cpSharedCosts
								WHERE 1=1
									AND id_product IS NOT NULL -- se habían mantenido para hacer la escoba
					
								UNION
								
								SELECT *							
								FROM catSharedCosts
								WHERE 1=1
									-- AND (id_product IS NOT NULL) ya filtrado en la propia consulta									
							
								UNION
								
								SELECT *							
								FROM caaSharedCosts	
					
								UNION
								
								SELECT *							
								FROM othSharedCosts	
							)
							"
'					 sql = "WITH lastTable as (SELECT 'En Manteniemiento' as Message)"
					 
					#End Region
					 
					#Region "CP"
					 
				Else If query = "CostsDistribution_CP" 
					
					sql = $"
					
						
					-- COMMON ------------------------------------------------------------------------------------
						WITH accountMapping as (
									SELECT 
										account_type
										, id as mng_account
								
									FROM XFC_PLT_MASTER_Account
								),
								
								costsData as (
								
									SELECT
										B.nature
										, A.id_costcenter
										, B.id_averagegroup as id_averagegroup
										, A.id_account
										, C.account_type
										, SUM(A.value) as TotalAmount
									
									FROM XFC_PLT_FACT_Costs A
									
									LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
										ON A.id_costcenter = B.id
										AND B.scenario = 'Actual'
										AND DATEFROMPARTS({year},{month},1) BETWEEN B.start_date AND B.end_date
									
									LEFT JOIN accountMapping C
										ON A.id_account = C.mng_account
									
									WHERE 1=1
										AND A.id_factory = '{factory}'
										AND MONTH(A.date) = {month}
										AND YEAR(A.date) = {year}
										AND A.scenario = '{scenario}'
										AND B.type = 'A'
										AND A.id_account <> 'Others'
									
									GROUP BY A.id_costcenter, B.id_averagegroup, B.nature, A.id_account, C.account_type
						
									HAVING NULLIF(SUM(A.value),0) <> 0
								),
					
					-- CP ------------------------------------------------------------------------------------
							-- Actividad por cada Grupo y CeCo
							totalActivityGMCeCo AS (
								SELECT 
									MONTH(date) as mes,
									id_averagegroup,
									id_costcenter,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, id_costcenter, uotype, MONTH(date)
								),
								
								-- Pesos CP dividiendo por el total de actividad
								productionData AS (
								SELECT 
									A.id_averagegroup
									, A.id_costcenter
									, A.id_product
									, A.uotype
									, SUM(A.activity_taj) as activity_taj
									, COALESCE(SUM(A.activity_taj) / NULLIF(B.total_activity_taj, 0), 0) AS activity_weight_taj
								
								FROM XFC_PLT_FACT_Production A
								
								INNER JOIN totalActivityGMCeCo B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.id_costcenter = B.id_costcenter
									AND A.uotype = B.uotype
									AND MONTH(A.date) = B.mes
								
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
								
								GROUP BY MONTH(A.date), A.id_averagegroup, A.id_costcenter, A.id_product, A.uotype, B.total_activity_taj
							),
							
							cpSharedCosts as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin 
									, A.id_costcenter as CeCo 
									, A.id_account
									, A.account_type
									, A.id_averagegroup as destination
									, B.id_product
									, A.TotalAmount*B.activity_weight_taj as SharedCost_TAJ
								
								FROM costsData A
								
								INNER JOIN productionData B
									ON A.id_averagegroup = B.id_averagegroup
									AND ( A.id_costcenter = B.id_costcenter OR B.id_costcenter = '-1')
									AND (NULLIF(activity_weight_taj,0) <> 0)
					
								WHERE 1=1
									AND A.nature = 'CP'
									AND ((A.account_type = 1 and B.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and B.uotype = 'UO2'))  	
									
							),
					
						costsDistributionInsert as (
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, {month}, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM cpSharedCosts
							
							GROUP BY id_account, CeCo, destination, id_product
					
						)
					"
					
					#End Region
					 
					#Region "CAT"
					
				Else If query = "CostsDistribution_CAT"
						
					sql = $"
					
					-- COMMON ------------------------------------------------------------------------------------
						WITH accountMapping as (
									SELECT 
										account_type
										, id as mng_account
								
									FROM XFC_PLT_MASTER_Account
								),
								
								costsData as (
								
									SELECT
										B.nature
										, A.id_costcenter
										, B.id_averagegroup as id_averagegroup
										, A.id_account
										, C.account_type
										, SUM(A.value) as TotalAmount
									
									FROM XFC_PLT_FACT_Costs A
									
									LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
										ON A.id_costcenter = B.id
										AND B.scenario = 'Actual'
										AND DATEFROMPARTS({year},{month},1) BETWEEN B.start_date AND B.end_date
									
									LEFT JOIN accountMapping C
										ON A.id_account = C.mng_account
									
									WHERE 1=1
										AND A.id_factory = '{factory}'
										AND MONTH(A.date) = {month}
										AND YEAR(A.date) = {year}
										AND A.scenario = '{scenario}'
										AND B.type = 'A'
										AND A.id_account <> 'Others'
									
									GROUP BY A.id_costcenter, B.id_averagegroup, B.nature, A.id_account, C.account_type
						
									HAVING NULLIF(SUM(A.value),0) <> 0
								),
					
					-- CAT ------------------------------------------------------------------------------------
							-- Actividad por cada Grupo
							totalActivityGM AS (
								SELECT 
									MONTH(date) as mes,
									id_averagegroup,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
									COALESCE(SUM(activity_tso), 0) AS total_activity_tso
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, uotype, MONTH(date)
								),
							
							-- Pesos CAT dividiendo por la actividad total de GM
							catWeights as (
								SELECT 
									A.id_averagegroup
									, A.id_product
									, A.uotype
									, CASE
										WHEN B.total_activity_taj = 0 THEN 0
										ELSE SUM(A.activity_taj) / B.total_activity_taj 
									END AS activity_weight_cat_taj
									, CASE 
										WHEN B.total_activity_tso = 0 THEN 0 
										ELSE SUM(A.activity_tso) / B.total_activity_tso 
									END AS activity_weight_cat_tso
							
								FROM XFC_PLT_FACT_Production A
					
								LEFT JOIN totalActivityGM B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.uotype = B.uotype
							
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND ISNULL(A.id_averagegroup,'') <> ''
							
								GROUP BY A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
							),														
							
							catAllocationKeysTotal as (
								SELECT 
									scenario
									, id_factory
									, date
									, id_costcenter
									, costnature
									, SUM(percentage) as percentageTotal

								FROM XFC_PLT_AUX_AllocationKeys
								
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
								
								GROUP BY scenario, id_factory, date, id_costcenter, costnature			
							), 
					
							catAllocationKeys as (
								SELECT 
									A.scenario
									, A.id_factory
									, A.date
									, A.id_costcenter
									, A.id_averagegroup
									, A.costnature
									, A.percentage / B.percentageTotal as percentage

								FROM XFC_PLT_AUX_AllocationKeys A
					
								LEFT JOIN catAllocationKeysTotal B
									ON 	A.scenario = B.scenario
									AND	A.id_factory = B.id_factory 	
									AND	A.date = B.date 	
									AND	A.id_costcenter = B.id_costcenter 
									AND	A.costnature = B.costnature 	
								
								WHERE 1=1
									AND A.scenario = '{scenario}'
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
								
								GROUP BY A.scenario, A.id_factory, A.date, A.id_costcenter, A.id_averagegroup, A.costnature, A.percentage, B.percentageTotal
							),
								
							catSharedCostsGM as (
								SELECT 
									A.nature
									, A.id_averagegroup
									, A.id_costcenter
									, A.id_account
									, A.account_type
									, B.id_averagegroup as destination
									, B.percentage as percentage
									, A.TotalAmount*(B.percentage) as GM_SharedCosts
									, ISNULL(B.id_averagegroup, '-1') as filterAcc
					
								FROM costsData A								
				
								INNER JOIN catAllocationKeys B
									ON A.id_costcenter = B.id_costcenter
									AND A.account_type = B.costnature
									AND B.scenario = '{scenario}'		
									AND B.id_factory = '{factory}'
									AND YEAR(B.date) = {year}
									AND MONTH(B.date) = {month}
					
								WHERE 1=1									
									AND A.nature = 'CAT'

							),
							
							catSharedCosts as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.destination
									, B.id_product
									, A.GM_SharedCosts*B.activity_weight_cat_taj as SharedCost_TAJ
									-- , A.TotalAmount*A.percentage*B.activity_weight_cat_taj as SharedCost_TAJ
							
								FROM catSharedCostsGM A
							
								INNER JOIN catWeights B
									ON A.destination = B.id_averagegroup
							
								WHERE 1=1
									AND A.filterAcc <> '-1'
									AND ((A.account_type = 1 and B.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and B.uotype = 'UO2'))  							
							
							),
					
						costsDistributionInsert as (
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, {month}, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM catSharedCosts
							
							GROUP BY id_account, CeCo, destination, id_product
					
						)
					"
					 
					#End Region
					 
					#Region "CAA"
					
				Else If query = "CostsDistribution_CAA"
					 
					sql = $"
						
					-- COMMON ------------------------------------------------------------------------------------
					
						WITH accountMapping as (
									SELECT 
										account_type
										, id as mng_account
								
									FROM XFC_PLT_MASTER_Account
								),
								
								costsData as (
								
									SELECT
										B.nature
										, A.id_costcenter
										, B.id_averagegroup as id_averagegroup
										, A.id_account
										, C.account_type
										, SUM(A.value) as TotalAmount
									
									FROM XFC_PLT_FACT_Costs A
									
									LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
										ON A.id_costcenter = B.id
										AND B.scenario = 'Actual'
										AND DATEFROMPARTS({year},{month},1) BETWEEN B.start_date AND B.end_date
									
									LEFT JOIN accountMapping C
										ON A.id_account = C.mng_account
									
									WHERE 1=1
										AND A.id_factory = '{factory}'
										AND MONTH(A.date) = {month}
										AND YEAR(A.date) = {year}
										AND A.scenario = '{scenario}'
										AND B.type = 'A'
										AND A.id_account <> 'Others'
									
									GROUP BY A.id_costcenter, B.id_averagegroup, B.nature, A.id_account, C.account_type
						
									HAVING NULLIF(SUM(A.value),0) <> 0
								),
					
					-- CAA ------------------------------------------------------------------------------------
							-- Pesos CAT - % actividad por producto y GM
							-- Actividad por cada Grupo
							totalActivityGM AS (
								SELECT 
									MONTH(date) as mes,
									id_averagegroup,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
									COALESCE(SUM(activity_tso), 0) AS total_activity_tso
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND MONTH(date) = {month}
									AND YEAR(date) = {year}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, uotype, MONTH(date)
								),
							
							-- Pesos CAT dividiendo por la actividad total de GM
							catWeights as (
								SELECT 
									A.id_averagegroup
									, A.id_product
									, A.uotype
									, CASE
										WHEN B.total_activity_taj = 0 THEN 0
										ELSE SUM(A.activity_taj) / B.total_activity_taj 
									END AS activity_weight_cat_taj
									, CASE 
										WHEN B.total_activity_tso = 0 THEN 0 
										ELSE SUM(A.activity_tso) / B.total_activity_tso 
									END AS activity_weight_cat_tso
							
								FROM XFC_PLT_FACT_Production A
					
								LEFT JOIN totalActivityGM B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.uotype = B.uotype
							
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND NULLIF(A.id_averagegroup,'') <> ''
							
								GROUP BY A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
							),
					
							-- Pesos CAA - % Costes repartidos	por GM		
					
							costsShared_CP_CAT as (
								SELECT 
									id_averagegroup
									, SUM(value) as SharedCost_TAJ
					
								FROM XFC_PLT_FACT_CostsDistribution	A
					
								LEFT JOIN accountMapping C
									ON A.id_account = C.mng_account
									
					
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND (C.account_type <> 5 OR C.account_type <> '05')
								
								GROUP BY id_averagegroup
							),
					
							costsShared_CP_CAT_Total as (
								SELECT 
									SUM(SharedCost_TAJ) AS TotalTAJ
								
								FROM costsShared_CP_CAT
					
							),
					
							caaWeight as (
								SELECT 
									id_averagegroup
									, SUM(SharedCost_TAJ) / SUM(B.TotalTAJ) as caa_weight_taj
								
								FROM costsShared_CP_CAT A
								
								CROSS JOIN costsShared_CP_CAT_Total B
									
								GROUP BY id_averagegroup
								
							),
							
							caaSharedCosts as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, B.id_averagegroup as destination
									, C.id_product
									, A.TotalAmount
									, B.caa_weight_taj
									, C.activity_weight_cat_taj 
									, A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_taj as SharedCost_TAJ								
							
								FROM costsData A
							
								CROSS JOIN caaWeight B
																
								LEFT JOIN catWeights C
									ON B.id_averagegroup = C.id_averagegroup
														
								WHERE 1=1
									AND A.nature = 'CAA'
									AND ((A.account_type = 1 and C.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and C.uotype = 'UO2'))
																		
							),
					
							costsDistributionInsert as (
								SELECT 
									'{factory}' as id_factory
									, '{scenario}' as scenario
									, DATEFROMPARTS({year}, {month}, 1) as date
									, id_account
									, CeCo as id_costcenter
									, destination as id_averagegroup
									, id_product
									, SUM(SharedCost_TAJ) as value
									, 'TAJ' as type_tso_taj
									, 'NA' as type
					
								FROM caaSharedCosts
								
								GROUP BY id_account, CeCo, destination, id_product
					
							)
					
					"
					
					#End Region
					 
					#Region "UNALLOCATED"
					
				Else If query = "CostsDistribution_UNA"
					
					sql = $"
					
					-- COMMON ------------------------------------------------------------------------------------
					WITH accountMapping as (
						SELECT 
							account_type
							, id as mng_account
					
						FROM XFC_PLT_MASTER_Account
					),
								
					costsData as (
								
						SELECT
							B.nature
							, A.id_costcenter
							, B.id_averagegroup as id_averagegroup
							, A.id_account
							, C.account_type
							, SUM(A.value) as TotalAmount
						
						FROM XFC_PLT_FACT_Costs A
						
						LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
							ON A.id_costcenter = B.id
							AND B.scenario = 'Actual'
							AND A.date BETWEEN B.start_date AND B.end_date
						
						LEFT JOIN accountMapping C
							ON A.id_account = C.mng_account
						
						WHERE 1=1
							AND A.id_factory = '{factory}'
							AND MONTH(A.date) = {month}
							AND YEAR(A.date) = {year}
							AND A.scenario = '{scenario}'
							AND B.type = 'A'
							AND A.id_account <> 'Others'
						
						GROUP BY A.id_costcenter, B.id_averagegroup, B.nature, A.id_account, C.account_type
						
						HAVING NULLIF(SUM(A.value),0) <> 0
						
						),	
					
					-- UNALLOCATED ------------------------------------------------------------------------------------
					
					-- CP - Centros de Coste CP, para los que no ha habido producción
					
					productionData AS (
						SELECT 
							A.id_averagegroup
							, A.id_costcenter
							, A.id_product
							, A.uotype
							, SUM(A.activity_taj) as activity_taj
						
						FROM XFC_PLT_FACT_Production A
						
						WHERE 1=1
							AND A.id_factory = '{factory}'
							AND MONTH(A.date) = {month}
							AND YEAR(A.date) = {year}
							AND A.scenario = '{scenario}'
						
						GROUP BY MONTH(A.date), A.id_averagegroup, A.id_costcenter, A.id_product, A.uotype
						),
					
						cpCosts as (
							SELECT 
								A.nature
								, A.id_averagegroup as Origin
								, A.id_costcenter as CeCo
								, A.id_account
								, A.account_type
								, A.TotalAmount								
								, 'NA' as type
					
							FROM costsData A
						
							LEFT JOIN productionData B
								ON A.id_averagegroup = B.id_averagegroup
								AND (A.id_costcenter = B.id_costcenter OR B.id_costcenter = '-1')
												
							WHERE 1=1
								AND A.nature = 'CP'
								AND B.id_product IS NULL
								-- AND A.id_averagegroup NOT IN ('#')
						),
					
					-- CAT 
					-- 1. Centros de coste CAT para los que no hay llaves de reparto o GM repartidos que no han tenido producción
					-- 2. Cálculos de pesos para CAA y CAT que luego usaremos para repartir
					
					-- Pesos CAT - % actividad por producto y GM
					
						-- Actividad por cada Grupo de Medios
					totalActivityGM AS (
						SELECT 
							MONTH(date) as mes,
							id_averagegroup,
							uotype,
							COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
							COALESCE(SUM(activity_tso), 0) AS total_activity_tso
						
						FROM XFC_PLT_FACT_Production
						
						WHERE 1=1
							AND id_factory = '{factory}'
							AND MONTH(date) = {month}
							AND YEAR(date) = {year}
							AND scenario = '{scenario}'
						
						GROUP BY id_averagegroup, uotype, MONTH(date)
						),
					
						-- Pesos CAT dividiendo por la actividad total de GM
					catWeights as (
						SELECT 
							A.id_averagegroup
							, A.id_product
							, A.uotype
							, CASE
								WHEN B.total_activity_taj = 0 THEN 0
								ELSE SUM(A.activity_taj) / B.total_activity_taj 
							END AS activity_weight_cat_taj
							, CASE 
								WHEN B.total_activity_tso = 0 THEN 0 
								ELSE SUM(A.activity_tso) / B.total_activity_tso 
							END AS activity_weight_cat_tso
					
						FROM XFC_PLT_FACT_Production A
			
						LEFT JOIN totalActivityGM B
							ON A.id_averagegroup = B.id_averagegroup
							AND A.uotype = B.uotype
					
						WHERE 1=1
							AND A.id_factory = '{factory}'
							AND MONTH(A.date) = {month}
							AND YEAR(A.date) = {year}
							AND A.scenario = '{scenario}'
							AND NULLIF(A.id_averagegroup,'') <> ''
					
						GROUP BY A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
						),
				
							-- Pesos CAA - % Costes repartidos	por GM		
				
						costsShared_CP_CAT as (
							SELECT 
								id_averagegroup
								, SUM(value) as SharedCost_TAJ
				
							FROM XFC_PLT_FACT_CostsDistribution	A
					
							LEFT JOIN XFC_PLT_MASTER_Account B
								ON A.id_account = B.id
								AND B.account_type <> 5
							
							WHERE 1=1
								AND A.id_factory = '{factory}'
								AND MONTH(A.date) = {month}
								AND YEAR(A.date) = {year}
								AND A.scenario = '{scenario}'
								AND A.id_averagegroup NOT IN ('#')
							
							GROUP BY id_averagegroup
						),
				
						costsShared_CP_CAT_Total as (
							SELECT 
								SUM(SharedCost_TAJ) AS TotalTAJ
							
							FROM costsShared_CP_CAT
				
						),
				
						caaWeight as (
							SELECT 
								id_averagegroup
								, SUM(SharedCost_TAJ) / SUM(B.TotalTAJ) as caa_weight_taj
							
							FROM costsShared_CP_CAT A
							
							CROSS JOIN costsShared_CP_CAT_Total B
								
							GROUP BY id_averagegroup
							
						), 
					
						catAllocationKeys as (
							SELECT 
								A.scenario
								, A.id_factory
								, A.date
								, A.id_costcenter
								, A.id_averagegroup
								, A.costnature
								, A.percentage as percentage
						
							FROM XFC_PLT_AUX_AllocationKeys A
							
							WHERE 1=1
								AND A.scenario = '{scenario}'
								AND A.id_factory = '{factory}'
								AND MONTH(A.date) = {month}
								AND YEAR(A.date) = {year}
							
							GROUP BY A.scenario, A.id_factory, A.date, A.id_costcenter, A.id_averagegroup, A.costnature, A.percentage
						),
					
						catCosts_NoAlloc as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.TotalAmount								
									, 'NA' as type
					
								FROM costsData A
							
								LEFT JOIN catAllocationKeys B
									ON A.id_costcenter = B.id_costcenter
									AND A.account_type = B.costnature
									AND B.scenario = '{scenario}'		
									AND B.id_factory = '{factory}'
									AND YEAR(B.date) = {year}
									AND MONTH(B.date) = {month}
														
								WHERE 1=1
									AND A.nature = 'CAT'
									AND B.percentage IS NULL									
																		
							),
					
							catCosts_NoProd_1 as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter
									, A.id_account
									, A.account_type
									, A.TotalAmount	
									, B.id_averagegroup
									, A.TotalAmount * B.percentage as costToShare
							
								FROM costsData A
							
								INNER JOIN catAllocationKeys B
									ON A.id_costcenter = B.id_costcenter
									AND A.account_type = B.costnature
									AND B.scenario = '{scenario}'		
									AND B.id_factory = '{factory}'
									AND YEAR(B.date) = {year}
									AND MONTH(B.date) = {month}
														
								WHERE 1=1
									AND A.nature = 'CAT'							
																		
							),
					
							catCosts_NoProd as (
								SELECT 
									A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.costToShare as TotalAmount										
									, 'NoProd' as type
					
								FROM catCosts_NoProd_1 A
								
								LEFT JOIN productionData B
									ON A.id_averagegroup = B.id_averagegroup
														
								WHERE 1=1
									AND B.id_product IS NULL
																		
							),
					
							catCosts as (
								SELECT *
								FROM catCosts_NoAlloc
								
								UNION ALL
							
								SELECT *
								FROM catCosts_NoProd
								
							),
					
					-- UNALLOCATED ------------------------------------------------------------------------------------
							
							cp_cat_SharedCosts as (
								SELECT 
									A.nature
									, A.Origin as Origin
									, A.CeCo as CeCo
									, A.id_account
									, A.account_type
									, A.TotalAmount
									, B.id_averagegroup as Destination
									, C.id_product
									, C.activity_weight_cat_taj as ActivityWeight_TAJ
									, B.caa_weight_taj as AuxiliarWeight_TAJ
									, A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_taj as SharedCost_TAJ
									, A.type
					
								FROM (
									SELECT *
									FROM cpCosts
									
									UNION ALL
									
									SELECT *
									FROM catCosts
									) as A
								
								CROSS JOIN caaWeight B
																
								LEFT JOIN catWeights C
									ON B.id_averagegroup = C.id_averagegroup
														
								WHERE 1=1
									AND ((A.account_type = 1 and C.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and C.uotype = 'UO2'))
							),
							
							costsDistributionInsert as (
								SELECT 
									'{factory}' as id_factory
									, '{scenario}' as scenario
									, DATEFROMPARTS({year}, {month}, 1) as date
									, id_account
									, CeCo as id_costcenter
									, destination as id_averagegroup
									, id_product
									, SUM(SharedCost_TAJ) as value
									, 'TAJ' as type_tso_taj
									, type
								
								FROM cp_cat_SharedCosts
								
								GROUP BY id_account, CeCo, destination, id_product, type
					
							)
						
					"					
					
					#End Region
					 
					#Region "OTHERS"
					
				Else If query = "CostsDistribution_OTH"
					
					sql = $"
					
					-- COMMON ------------------------------------------------------------------------------------
					WITH accountMapping as (
								SELECT 
									account_type
									, id as mng_account
							
								FROM XFC_PLT_MASTER_Account
							),
					
					-- OTHERS ------------------------------------------------------------------------------------
							othSharedCosts as (
							
								SELECT
									B.nature as nature
									, B.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, C.account_type
									, B.id_averagegroup as destination
									, '-1' as id_product
									, SUM(A.value) as SharedCost_TAJ
					
								FROM XFC_PLT_FACT_Costs A
								
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
									ON A.id_costcenter = B.id
									AND B.scenario = 'Actual'
									AND A.date BETWEEN B.start_date AND B.end_date
								
								LEFT JOIN accountMapping C
									ON A.id_account = C.mng_account
								
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND MONTH(A.date) = {month}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND B.type <> 'A'
									AND A.id_account <> 'Others'
								
								GROUP BY B.nature, A.id_costcenter, B.id_averagegroup, A.id_account, C.account_type
								
								HAVING NULLIF(SUM(A.value),0) <> 0
							),
					
						costsDistributionInsert as (
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, {month}, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM othSharedCosts
							
							GROUP BY id_account, CeCo, destination, id_product
					
						)
					"	
					#End Region
					
				#End Region		
				
				#Region "Cost Distribution - Rework"				
					 
				Else If query.Contains("CostsDistribution_") Then
					
					Dim monthFilter As String = If(scenario="Actual", $"MONTH(date) = {month}",$"MONTH(date) > {monthFCT}")
					Dim monthFilter_A As String = If(scenario="Actual",$"MONTH(A.date) = {month}" ,$"MONTH(A.date) > {monthFCT}")

					#Region "Common Query"
					
					Dim commonQueryCosts As String = $"
					
					-- COMMON ------------------------------------------------------------------------------------
					
						WITH accountMapping as (
									SELECT 
										account_type
										, id as mng_account
								
									FROM XFC_PLT_MASTER_Account
								),
								
								costsData as (
								
									SELECT
										MONTH(A.date) as month
										, B.nature
										, A.id_costcenter
										, B.id_averagegroup as id_averagegroup
										, A.id_account
										, C.account_type
										, SUM(A.value) as TotalAmount
									
									FROM XFC_PLT_FACT_Costs A
									
									LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
										ON A.id_costcenter = B.id
										AND B.scenario = 'Actual'
										AND A.date BETWEEN B.start_date AND B.end_date
									
									LEFT JOIN accountMapping C
										ON A.id_account = C.mng_account
									
									WHERE 1=1
										AND A.id_factory = '{factory}'
										AND {monthFilter_A}
										AND YEAR(A.date) = {year}
										AND A.scenario = '{scenario}'
										AND B.type = 'A'
										AND A.id_account <> 'Others'
									
									GROUP BY A.id_costcenter, B.id_averagegroup, B.nature, A.id_account, C.account_type, MONTH(A.date)
						
									HAVING NULLIF(SUM(A.value),0) <> 0
								),
					"
					#End Region
					
					#Region "CP"
					 
				 	If query = "CostsDistribution_CP_NEW" 
					
					sql = $"				
						
					{commonQueryCosts}
					
					-- CP ------------------------------------------------------------------------------------
							-- Actividad por cada Grupo y CeCo
							totalActivityGMCeCo AS (
								SELECT 
									MONTH(date) as month,
									id_averagegroup,
									id_costcenter,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND {monthFilter}
									AND YEAR(date) = {year}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, uotype, MONTH(date), id_costcenter
								),
								
								-- Pesos CP dividiendo por el total de actividad
								productionData AS (
								SELECT 
									MONTH(A.date) As month
									, A.id_averagegroup
									, A.id_costcenter
									, A.id_product
									, A.uotype
									, SUM(A.activity_taj) as activity_taj
									, COALESCE(SUM(A.activity_taj) / NULLIF(B.total_activity_taj, 0), 0) AS activity_weight_taj
								
								FROM XFC_PLT_FACT_Production A
								
								INNER JOIN totalActivityGMCeCo B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.id_costcenter = B.id_costcenter
									AND A.uotype = B.uotype
									AND MONTH(A.date) = B.month
								
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND {monthFilter_A}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
								
								GROUP BY MONTH(A.date), A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj , A.id_costcenter
							),
							
							cpSharedCosts as (
								SELECT
									A.month as month
									, A.nature
									, A.id_averagegroup as Origin 
									, A.id_costcenter as CeCo 
									, A.id_account
									, A.account_type
									, A.id_averagegroup as destination
									, B.id_product
									, A.TotalAmount*B.activity_weight_taj as SharedCost_TAJ
									, B.uotype

								FROM costsData A
					
								INNER JOIN productionData B
									ON A.id_averagegroup = B.id_averagegroup
									AND (A.id_costcenter = B.id_costcenter OR B.id_costcenter = '-1')
									AND (NULLIF(activity_weight_taj,0) <> 0)
									AND A.month = B.month
					
								WHERE 1=1
									AND A.nature = 'CP'
									-- AND ((A.account_type = 1 and B.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and B.uotype = 'UO2'))  	
									
							),
					
						costsDistributionInsert as (
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, month, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM cpSharedCosts
					
							WHERE (account_type = 1 and uotype = 'UO1')
							
							GROUP BY id_account, CeCo, destination, id_product, month
					
							UNION ALL		
					
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, month, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM cpSharedCosts
					
							WHERE (ISNULL(account_type, 0) <> 1 and uotype = 'UO2')
							
							GROUP BY id_account, CeCo, destination, id_product, month
					
						)
					"
					
					#End Region
					 
					#Region "CAT"
					
				Else If query = "CostsDistribution_CAT_NEW"
						
					sql = $"
					
					{commonQueryCosts}
					
					-- CAT ------------------------------------------------------------------------------------
							-- Actividad por cada Grupo
							totalActivityGM AS (
								SELECT 
									MONTH(date) as month,
									id_averagegroup,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
									COALESCE(SUM(activity_tso), 0) AS total_activity_tso
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND {monthFilter}
									AND YEAR(date) = {year}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, uotype, MONTH(date)
								),
							
							-- Pesos CAT dividiendo por la actividad total de GM
							catWeights as (
								SELECT
									MONTH(A.date) as month
									, A.id_averagegroup
									, A.id_product
									, A.uotype
									, CASE
										WHEN B.total_activity_taj = 0 THEN 0
										ELSE SUM(A.activity_taj) / B.total_activity_taj 
									END AS activity_weight_cat_taj
									, CASE 
										WHEN B.total_activity_tso = 0 THEN 0 
										ELSE SUM(A.activity_tso) / B.total_activity_tso 
									END AS activity_weight_cat_tso
							
								FROM XFC_PLT_FACT_Production A
					
								LEFT JOIN totalActivityGM B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.uotype = B.uotype
									AND MONTH(A.date) = B.month
					
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND {monthFilter_A}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND NULLIF(A.id_averagegroup,'') <> ''
							
								GROUP BY MONTH(A.date), A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
							),														
							
							catAllocationKeysTotal as (
								SELECT 
									scenario
									, id_factory
									, date
									, id_costcenter
									, costnature
									, SUM(percentage) as percentageTotal

								FROM XFC_PLT_AUX_AllocationKeys
								
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND {monthFilter}
									AND YEAR(date) = {year}
								
								GROUP BY scenario, id_factory, date, id_costcenter, costnature			
							), 
					
							catAllocationKeys as (
								SELECT 
									A.scenario
									, A.id_factory
									, A.date
									, A.id_costcenter
									, A.id_averagegroup
									, A.costnature
									, A.percentage / B.percentageTotal as percentage

								FROM XFC_PLT_AUX_AllocationKeys A
					
								LEFT JOIN catAllocationKeysTotal B
									ON 	A.scenario = B.scenario
									AND	A.id_factory = B.id_factory 	
									AND	A.date = B.date 	
									AND	A.id_costcenter = B.id_costcenter 
									AND	A.costnature = B.costnature 	
								
								WHERE 1=1
									AND A.scenario = '{scenario}'
									AND A.id_factory = '{factory}'
									AND {monthFilter_A}
									AND YEAR(A.date) = {year}
								
								GROUP BY A.scenario, A.id_factory, A.date, A.id_costcenter, A.id_averagegroup, A.costnature, A.percentage, B.percentageTotal
							),
								
							catSharedCostsGM as (
								SELECT
									A.month
									, A.nature
									, A.id_averagegroup
									, A.id_costcenter
									, A.id_account
									, A.account_type
									, B.id_averagegroup as destination
									, B.percentage as percentage
									, A.TotalAmount*(B.percentage) as GM_SharedCosts
									, ISNULL(B.id_averagegroup, '-1') as filterAcc
					
								FROM costsData A								
				
								INNER JOIN catAllocationKeys B
									ON A.id_costcenter = B.id_costcenter
									AND A.account_type = B.costnature
									AND B.scenario = '{scenario}'		
									AND B.id_factory = '{factory}'
									AND YEAR(B.date) = {year}
									AND A.month = MONTH(B.date)
					
								WHERE 1=1									
									AND A.nature = 'CAT'

							),
							
							catSharedCosts as (
								SELECT
									A.month
									, A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.destination
									, B.id_product
									, A.GM_SharedCosts*B.activity_weight_cat_taj as SharedCost_TAJ
									, B.uotype
									-- , A.TotalAmount*A.percentage*B.activity_weight_cat_taj as SharedCost_TAJ
							
								FROM catSharedCostsGM A
							
								INNER JOIN catWeights B
									ON A.destination = B.id_averagegroup
									AND A.month = B.month
							
								WHERE 1=1
									AND A.filterAcc <> '-1'
									-- AND ((A.account_type = 1 and B.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and B.uotype = 'UO2'))  							
							
							),
					
						costsDistributionInsert as (
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, month, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM catSharedCosts
					
							WHERE (account_type = 1 and uotype = 'UO1')
					
							GROUP BY id_account, CeCo, destination, id_product, month
					
							UNION ALL
					
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, month, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM catSharedCosts
					
							WHERE (ISNULL(account_type, 0) <> 1 and uotype = 'UO2')
					
							GROUP BY id_account, CeCo, destination, id_product, month
					
					
						)
					"
					 
					#End Region
										 
					#Region "CAA"
					
				Else If query = "CostsDistribution_CAA_NEW"
					 
					sql = $"
					
					{commonQueryCosts}
					
					-- CAA ------------------------------------------------------------------------------------
							-- Pesos CAT - % actividad por producto y GM
							-- Actividad por cada Grupo
							totalActivityGM AS (
								SELECT 
									MONTH(date) as month,
									id_averagegroup,
									uotype,
									COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
									COALESCE(SUM(activity_tso), 0) AS total_activity_tso
								
								FROM XFC_PLT_FACT_Production
								
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									AND {monthFilter}
									AND scenario = '{scenario}'
								
								GROUP BY id_averagegroup, uotype, MONTH(date)
								),
							
							-- Pesos CAT dividiendo por la actividad total de GM
							catWeights as (
								SELECT
									MONTH(A.date) as month
									, A.id_averagegroup
									, A.id_product
									, A.uotype
									, CASE
										WHEN B.total_activity_taj = 0 THEN 0
										ELSE SUM(A.activity_taj) / B.total_activity_taj 
									END AS activity_weight_cat_taj
									, CASE 
										WHEN B.total_activity_tso = 0 THEN 0 
										ELSE SUM(A.activity_tso) / B.total_activity_tso 
									END AS activity_weight_cat_tso
							
								FROM XFC_PLT_FACT_Production A
					
								LEFT JOIN totalActivityGM B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.uotype = B.uotype
									AND MONTH(A.date) = B.month
							
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND {monthFilter_A}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND NULLIF(A.id_averagegroup,'') <> ''
							
								GROUP BY MONTH(A.date), A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
							),
					
							-- Pesos CAA - % Costes repartidos	por GM		
					
							costsShared_CP_CAT as (
								SELECT
									MONTH(A.date) as month
									, id_averagegroup
									, SUM(value) as SharedCost_TAJ
					
								FROM XFC_PLT_FACT_CostsDistribution_Raw_{factory}	A
					
								LEFT JOIN accountMapping C
									ON A.id_account = C.mng_account									
					
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND YEAR(A.date) = {year}
									AND {monthFilter_A}
									AND A.scenario = '{scenario}'
									AND (C.account_type <> 5 OR C.account_type <> '05')
								
								GROUP BY MONTH(A.date), id_averagegroup
							),
					
							costsShared_CP_CAT_Total as (
								SELECT
									month
									, SUM(SharedCost_TAJ) AS TotalTAJ
								
								FROM costsShared_CP_CAT
								
								GROUP BY month
					
							),
					
							caaWeight as (
								SELECT
									A.month
									, id_averagegroup
									, SUM(SharedCost_TAJ) / SUM(B.TotalTAJ) as caa_weight_taj
								
								FROM costsShared_CP_CAT A
								
								LEFT JOIN costsShared_CP_CAT_Total B
									ON A.month = B.month
					
								GROUP BY id_averagegroup, A.month
								
							),
							
							caaSharedCosts as (
								SELECT
									A.month
									, A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, B.id_averagegroup as destination
									, C.id_product
									, A.TotalAmount
									, B.caa_weight_taj
									, C.activity_weight_cat_taj 
									, A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_taj as SharedCost_TAJ	
									, C.uotype
							
								FROM costsData A
							
								LEFT JOIN caaWeight B
									ON A.month = B.month
					
								LEFT JOIN catWeights C
									ON B.id_averagegroup = C.id_averagegroup
									AND A.month = C.month
									AND B.month = C.month
														
								WHERE 1=1
									AND A.nature = 'CAA'
									-- AND ((A.account_type = 1 and C.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and C.uotype = 'UO2'))
																		
							),
					
							costsDistributionInsert as (
								SELECT 
									'{factory}' as id_factory
									, '{scenario}' as scenario
									, DATEFROMPARTS({year}, month, 1) as date
									, id_account
									, CeCo as id_costcenter
									, destination as id_averagegroup
									, id_product
									, SUM(SharedCost_TAJ) as value
									, 'TAJ' as type_tso_taj
									, 'NA' as type
					
								FROM caaSharedCosts
					
								WHERE (account_type = 1 and uotype = 'UO1')
								
								GROUP BY id_account, CeCo, destination, id_product, month
					
								UNION ALL
					
								SELECT 
									'{factory}' as id_factory
									, '{scenario}' as scenario
									, DATEFROMPARTS({year}, month, 1) as date
									, id_account
									, CeCo as id_costcenter
									, destination as id_averagegroup
									, id_product
									, SUM(SharedCost_TAJ) as value
									, 'TAJ' as type_tso_taj
									, 'NA' as type
					
								FROM caaSharedCosts
					
								WHERE (ISNULL(account_type, 0) <> 1 and uotype = 'UO2')
								
								GROUP BY id_account, CeCo, destination, id_product, month
							)
					
					"

					#End Region
					 
					#Region "UNALLOCATED"
					
				Else If query = "CostsDistribution_UNA_NEW"
					
					sql = $"
					
					{commonQueryCosts}
					
					-- UNALLOCATED ------------------------------------------------------------------------------------
					
					-- CP - Centros de Coste CP, para los que no ha habido producción
					
					productionData AS (
						SELECT
							MONTH(A.date) as month
							, A.id_averagegroup
							, A.id_costcenter
							, A.id_product
							, A.uotype
							, SUM(A.activity_taj) as activity_taj
						
						FROM XFC_PLT_FACT_Production A
						
						WHERE 1=1
							AND A.id_factory = '{factory}'
							AND {monthFilter_A}
							AND YEAR(A.date) = {year}
							AND A.scenario = '{scenario}'
							AND A.activity_taj <> 0
					
						GROUP BY MONTH(A.date), A.id_averagegroup, A.id_costcenter, A.id_product, A.uotype
						),
					
						cpCosts as (
							SELECT
								A.month
								, A.nature
								, A.id_averagegroup as Origin
								, A.id_costcenter as CeCo
								, A.id_account
								, A.account_type
								, A.TotalAmount								
								, 'NA' as type
					
							FROM costsData A
						
							LEFT JOIN productionData B
								ON A.id_averagegroup = B.id_averagegroup
								AND (A.id_costcenter = B.id_costcenter OR B.id_costcenter = '-1')
								AND A.month = B.month
												
							Where 1=1
								AND A.nature = 'CP'
								AND B.id_product IS NULL
								-- AND A.id_averagegroup NOT IN ('#')
						),
					
					-- CAT 
					-- 1. Centros de coste CAT para los que no hay llaves de reparto o GM repartidos que no han tenido producción
					-- 2. Cálculos de pesos para CAA y CAT que luego usaremos para repartir
					
					-- Pesos CAT - % actividad por producto y GM
					
						-- Actividad por cada Grupo de Medios
					totalActivityGM AS (
						SELECT 
							MONTH(date) as month,
							id_averagegroup,
							uotype,
							COALESCE(SUM(activity_taj), 0) AS total_activity_taj,
							COALESCE(SUM(activity_tso), 0) AS total_activity_tso
						
						FROM XFC_PLT_FACT_Production
						
						WHERE 1=1
							AND id_factory = '{factory}'
							AND {monthFilter}
							AND YEAR(date) = {year}
							AND scenario = '{scenario}'
						
						GROUP BY id_averagegroup, uotype, MONTH(date)
						),
					
						-- Pesos CAT dividiendo por la actividad total de GM
					catWeights as (
						SELECT
							MONTH(A.date) as month
							, A.id_averagegroup
							, A.id_product
							, A.uotype
							, CASE
								WHEN B.total_activity_taj = 0 THEN 0
								ELSE SUM(A.activity_taj) / B.total_activity_taj 
							END AS activity_weight_cat_taj
							, CASE 
								WHEN B.total_activity_tso = 0 THEN 0 
								ELSE SUM(A.activity_tso) / B.total_activity_tso 
							END AS activity_weight_cat_tso
					
						FROM XFC_PLT_FACT_Production A
			
						LEFT JOIN totalActivityGM B
							ON A.id_averagegroup = B.id_averagegroup
							AND A.uotype = B.uotype
							AND MONTH(A.date) = B.month
					
						WHERE 1=1
							AND A.id_factory = '{factory}'
							AND {monthFilter_A}
							AND YEAR(A.date) = {year}
							AND A.scenario = '{scenario}'
							AND ISNULL(A.id_averagegroup,'') <> ''
					
						GROUP BY MONTH(A.date), A.id_averagegroup, A.id_product, A.uotype, B.total_activity_taj, B.total_activity_tso
						),
				
							-- Pesos CAA - % Costes repartidos	por GM		
				
						costsShared_CP_CAT as (
							SELECT
								MONTH(A.date) as month
								, id_averagegroup
								, SUM(value) as SharedCost_TAJ
				
							FROM XFC_PLT_FACT_CostsDistribution_Raw_{factory}	A
					
							LEFT JOIN XFC_PLT_MASTER_Account B
								ON A.id_account = B.id
								AND B.account_type <> 5
							
							WHERE 1=1
								AND A.id_factory = '{factory}'
								AND {monthFilter_A}
								AND YEAR(A.date) = {year}
								AND A.scenario = '{scenario}'
								AND A.id_averagegroup NOT IN ('#')
							
							GROUP BY id_averagegroup, MONTH(A.date)
						),
				
						costsShared_CP_CAT_Total as (
							SELECT
								month
								, SUM(SharedCost_TAJ) AS TotalTAJ
							
							FROM costsShared_CP_CAT
					
							GROUP BY month
				
						),
				
						caaWeight as (
							SELECT
								A.month
								, id_averagegroup
								, SUM(SharedCost_TAJ) / SUM(B.TotalTAJ) as caa_weight_taj
							
							FROM costsShared_CP_CAT A
							
							LEFT JOIN costsShared_CP_CAT_Total B
								ON A.month = B.month
					
							GROUP BY id_averagegroup, A.month
							
						), 
					
						catAllocationKeys as (
							SELECT 
								A.scenario
								, A.id_factory
								, A.date
								, A.id_costcenter
								, A.id_averagegroup
								, A.costnature
								, A.percentage as percentage
						
							FROM XFC_PLT_AUX_AllocationKeys A
							
							WHERE 1=1
								AND A.scenario = '{scenario}'
								AND A.id_factory = '{factory}'
								AND {monthFilter_A}
								AND YEAR(A.date) = {year}
							
							GROUP BY A.scenario, A.id_factory, A.date, A.id_costcenter, A.id_averagegroup, A.costnature, A.percentage
						),
					
						catCosts_NoAlloc as (
								SELECT
									A.month
									, A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.TotalAmount								
									, 'NA' as type
					
								FROM costsData A
							
								LEFT JOIN catAllocationKeys B
									ON A.id_costcenter = B.id_costcenter
									AND A.account_type = B.costnature
									AND B.scenario = '{scenario}'		
									AND B.id_factory = '{factory}'
									AND YEAR(B.date) = {year}
									AND MONTH(B.date) = A.month
														
								WHERE 1=1
									AND A.nature = 'CAT'
									AND B.percentage IS NULL									
																		
							),
					
							catCosts_NoProd_1 as (
								SELECT
									A.month
									, A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter
									, A.id_account
									, A.account_type
									, A.TotalAmount	
									, B.id_averagegroup
									, A.TotalAmount * B.percentage as costToShare
							
								FROM costsData A
							
								INNER JOIN catAllocationKeys B
									ON A.id_costcenter = B.id_costcenter
									AND A.account_type = B.costnature
									AND B.scenario = '{scenario}'		
									AND B.id_factory = '{factory}'
									AND YEAR(B.date) = {year}
									AND MONTH(B.date) = A.month
														
								WHERE 1=1
									AND A.nature = 'CAT'							
																		
							),
					
							catCosts_NoProd as (
								SELECT
									A.month
									, A.nature
									, A.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, A.account_type
									, A.costToShare as TotalAmount										
									, 'NoProd' as type
					
								FROM catCosts_NoProd_1 A
								
								LEFT JOIN productionData B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.month = B.month
														
								WHERE 1=1
									AND B.id_product IS NULL
																		
							),
					
							catCosts as (
								SELECT *
								FROM catCosts_NoAlloc
								
								UNION ALL
							
								SELECT *
								FROM catCosts_NoProd
								
							),
					
					-- UNALLOCATED ------------------------------------------------------------------------------------
							
							cp_cat_SharedCosts as (
								SELECT
									A.month
									, A.nature
									, A.Origin as Origin
									, A.CeCo as CeCo
									, A.id_account
									, A.account_type
									, C.uotype
									, A.TotalAmount
									, B.id_averagegroup as Destination
									, C.id_product
									, C.activity_weight_cat_taj as ActivityWeight_TAJ
									, B.caa_weight_taj as AuxiliarWeight_TAJ
									, A.TotalAmount * B.caa_weight_taj * C.activity_weight_cat_taj as SharedCost_TAJ
									, A.type
					
								FROM (
									SELECT *
									FROM cpCosts
									
									UNION ALL
									
									SELECT *
									FROM catCosts
									) as A
								
								INNER JOIN caaWeight B
									ON A.month = B.month
																
								INNER JOIN catWeights C
									ON B.id_averagegroup = C.id_averagegroup
									AND A.month = C.month
									AND B.month = C.month
														
							),
							
							costsDistributionInsert as (
								SELECT 
									'{factory}' as id_factory
									, '{scenario}' as scenario
									, DATEFROMPARTS({year}, month, 1) as date
									, id_account
									, CeCo as id_costcenter
									, destination as id_averagegroup
									, id_product
									, SUM(SharedCost_TAJ) as value
									, 'TAJ' as type_tso_taj
									, type
								
								FROM cp_cat_SharedCosts
								
								WHERE 1=1
									AND (account_type = 1 and uotype = 'UO1')
								
								GROUP BY id_account, CeCo, destination, id_product, type, month
								
								UNION ALL
					
								SELECT 
									'{factory}' as id_factory
									, '{scenario}' as scenario
									, DATEFROMPARTS({year}, month, 1) as date
									, id_account
									, CeCo as id_costcenter
									, destination as id_averagegroup
									, id_product
									, SUM(SharedCost_TAJ) as value
									, 'TAJ' as type_tso_taj
									, type
								
								FROM cp_cat_SharedCosts
								
								WHERE 1=1
									AND (ISNULL(account_type, 0) <> 1 and uotype = 'UO2')
								
								GROUP BY id_account, CeCo, destination, id_product, type, month
							)
						
					"					
					#End Region
					 
					#Region "OTHERS"
					
				Else If query = "CostsDistribution_OTH_NEW"
					
					sql = $"
					
					-- COMMON ------------------------------------------------------------------------------------
					WITH accountMapping as (
								SELECT 
									account_type
									, id as mng_account
							
								FROM XFC_PLT_MASTER_Account
							),
					
					-- OTHERS ------------------------------------------------------------------------------------
							othSharedCosts as (
							
								SELECT
									MONTH(A.date) as month
									, B.nature as nature
									, B.id_averagegroup as Origin
									, A.id_costcenter as CeCo
									, A.id_account
									, C.account_type
									, B.id_averagegroup as destination
									, '-1' as id_product
									, SUM(A.value) as SharedCost_TAJ
					
								FROM XFC_PLT_FACT_Costs A
								
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
									ON A.id_costcenter = B.id
									AND B.scenario = 'Actual'
									AND A.date BETWEEN B.start_date AND B.end_date
								
								LEFT JOIN accountMapping C
									ON A.id_account = C.mng_account
								
								WHERE 1=1
									AND A.id_factory = '{factory}'
									AND {monthFilter_A}
									AND YEAR(A.date) = {year}
									AND A.scenario = '{scenario}'
									AND (B.type <> 'A' OR (B.type = 'A' AND B.nature = '#'))
									AND A.id_account <> 'Others'
								
								GROUP BY MONTH(A.date), B.nature, A.id_costcenter, B.id_averagegroup, A.id_account, C.account_type
								
								HAVING NULLIF(SUM(A.value),0) <> 0
							),
					
						costsDistributionInsert as (
							SELECT 
								'{factory}' as id_factory
								, '{scenario}' as scenario
								, DATEFROMPARTS({year}, month, 1) as date
								, id_account
								, CeCo as id_costcenter
								, destination as id_averagegroup
								, id_product
								, SUM(SharedCost_TAJ) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM othSharedCosts
							
							GROUP BY id_account, CeCo, destination, id_product, month
					
						)
					"	
					#End Region
					
						End If
						
				#End Region
					
				#Region "Activity TSO"				
													
				ElseIf query = "ActivityTSO"					
					
					sql = $"
								WITH activityTSO AS (
					
									SELECT
										date
										, MONTH(F.date) AS month
										, F.id_averagegroup
										, F.id_product
										, F.id_costcenter
										, F.uotype
										, F.volume
										, F.volume * CASE WHEN COALESCE(B.value,0) <> 0 THEN ISNULL(B.value,0)
													      ELSE F.allocation_taj END AS activity_tso
					
									FROM (	
											SELECT
												date
												, MONTH(date) AS month
												, id_averagegroup
												, id_product
												, id_costcenter
												, uotype
												, volume
												, allocation_taj					
											FROM XFC_PLT_FACT_Production								
											WHERE scenario = '{scenario}'
											AND YEAR(date) = {year}						
											AND id_factory = '{factory}'	
											AND activity_taj <> 0 ) F
	
									LEFT JOIN (	
											SELECT DISTINCT
												MONTH(date) AS month
												, id_averagegroup
												, id_product
												, uotype
												, value
											FROM XFC_PLT_AUX_Production_Planning_Times_NewProducts						
											WHERE scenario = '{scenarioRef}'
											AND YEAR(date) = {year}						
											AND id_factory = '{factory}'	
											AND value <> 0 ) A
		
										ON F.month = A.month
										AND F.id_averagegroup = A.id_averagegroup
										AND F.id_product = A.id_product
										AND F.uotype = A.uotype					
					
									LEFT JOIN (	
											SELECT DISTINCT
												MONTH(date) AS month
												, id_averagegroup
												, id_product
												, uotype
												, value
											FROM XFC_PLT_AUX_Production_Planning_Times						
											WHERE scenario = '{scenarioRef}'
											AND YEAR(date) = {year}						
											AND id_factory = '{factory}'	
											AND value <> 0 ) B
		
										ON F.month = B.month
										AND F.id_averagegroup = B.id_averagegroup
										AND F.id_product = B.id_product
										AND F.uotype = B.uotype
					
								)
					"
					
				#End Region
				
				#Region "Activity TSO (All Factories)"				
													
				ElseIf query = "ActivityTSO_All_Factories"					
					
					sql = $"
								WITH activityTSO AS (
					
									SELECT
										F.id_factory
										, date
										, MONTH(F.date) AS month
										, F.id_averagegroup
										, F.id_product
										, F.id_costcenter
										, F.uotype
										, F.volume
										, F.volume * CASE WHEN COALESCE(B.value,0) <> 0 THEN ISNULL(B.value,0)
													      ELSE F.allocation_taj END AS activity_tso
					
									FROM (	
											SELECT
												id_factory
												, date
												, MONTH(date) AS month
												, id_averagegroup
												, id_product
												, id_costcenter
												, uotype
												, volume
												, allocation_taj					
											FROM XFC_PLT_FACT_Production								
											WHERE scenario = '{scenario}'
											AND YEAR(date) = {year}						
											AND activity_taj <> 0 ) F
	
									LEFT JOIN (	
											SELECT DISTINCT
												id_factory
												, MONTH(date) AS month
												, id_averagegroup
												, id_product
												, uotype
												, value
											FROM XFC_PLT_AUX_Production_Planning_Times_NewProducts						
											WHERE scenario = '{scenarioRef}'
											AND YEAR(date) = {year}																	
											AND value <> 0 ) A
		
										ON F.id_factory = A.id_factory
										AND F.month = A.month
										AND F.id_averagegroup = A.id_averagegroup
										AND F.id_product = A.id_product
										AND F.uotype = A.uotype					
					
									LEFT JOIN (	
											SELECT DISTINCT
												id_factory
												, MONTH(date) AS month
												, id_averagegroup
												, id_product
												, uotype
												, value
											FROM XFC_PLT_AUX_Production_Planning_Times						
											WHERE scenario = '{scenarioRef}'
											AND YEAR(date) = {year}						
											AND value <> 0 ) B
		
										ON F.id_factory = B.id_factory
										AND F.month = B.month
										AND F.id_averagegroup = B.id_averagegroup
										AND F.id_product = B.id_product
										AND F.uotype = B.uotype
					
								)
					"
					
				#End Region					
				
				#Region "Activity ALL"				
													
				ElseIf query = "ActivityALL"					
					
							Dim sql_ActivityTSO = AllQueries(si, "ActivityTSO", factory, month, year, scenario, scenarioRef, time)	
							
							sql = sql_ActivityTSO & $"
							
								, ActivityALL AS (
							
									-- ACTIVITY TAJ
							
									SELECT 
										F.id_factory AS [Id Factory]
										,F.id_averagegroup AS [Id GM]
										,F.id_costcenter AS [Id Cost Center]
										,F.id_product AS [Id Product]
										,F.uotype AS Time
										,F.date AS Date
										,F.volume AS Volume									
										,F.activity_taj AS [Activity TAJ]
										,NULL AS [Activity TSO]
										,0 AS [Volume Ref]
										,NULL AS [Activity TAJ Ref]
																
									FROM XFC_PLT_FACT_Production F
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON F.id_product = P.id							
									--LEFT JOIN XFC_PLT_HIER_Product H
									--	ON F.id_product = H.id
									
									WHERE F.scenario = '{scenario}'
									AND YEAR(F.date) = {year}		
									AND F.id_factory = '{factory}'
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
									AND (F.activity_taj <> 0 OR F.activity_tso <> 0)
						
								UNION ALL
							
									-- ACTIVITY TSO
				
									SELECT 
										'{factory}' AS [Id Factory]
										,F.id_averagegroup AS [Id GM]
										,F.id_costcenter AS [Id Cost Center]
										,F.id_product AS [Id Product]							
										,F.uotype AS Time
										,F.date AS Date
										,0 AS Volume										
										,NULL AS [Activity TAJ]
										,F.activity_tso AS [Activity TSO]
										,0 AS [Volume Ref]
										,NULL AS [Activity TAJ Ref]
																
									FROM activityTSO F
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON F.id_product = P.id							
									--LEFT JOIN XFC_PLT_HIER_Product H
									--	ON F.id_product = H.id
									
									WHERE 1=1
									AND (F.uotype = '{time}' OR '{time}' = 'All')								
					
								UNION ALL
							
									-- ACTIVITY TAJ REF
						
									SELECT 
										F.id_factory AS [Id Factory]
										,F.id_averagegroup AS [Id GM]
										,F.id_costcenter AS [Id Cost Center]
										,F.id_product AS [Id Product]							
										,F.uotype AS Time
										,F.date AS Date
										,0 AS Volume
										,NULL AS [Activity TAJ]
										,NULL AS [Activity TSO]
										,F.volume AS [Volume Ref]
										,F.activity_taj  AS [Activity TAJ Ref]
																
									FROM XFC_PLT_FACT_Production F
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON F.id_product = P.id							
									--LEFT JOIN XFC_PLT_HIER_Product H
									--	ON F.id_product = H.id				
									
									WHERE F.scenario = '{scenarioRef}'
									AND YEAR(F.date) = {year}		
									AND F.id_factory = '{factory}'
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
									AND (F.activity_taj <> 0 OR F.activity_tso <> 0)
									
								)
					"
					
				#End Region			
				
				#Region "Activity ALL (All Factories)"				
													
				ElseIf query = "ActivityALL_All_Factories"					
					
							Dim sql_ActivityTSO = AllQueries(si, "ActivityTSO_All_Factories", "", month, year, scenario, scenarioRef, time)	
							
							sql = sql_ActivityTSO & $"
							
								, ActivityALL AS (
							
									-- ACTIVITY TAJ
							
									SELECT 
										F.id_factory AS [Id Factory]
										,F.id_averagegroup AS [Id GM]
										,F.id_costcenter AS [Id Cost Center]
										,F.id_product AS [Id Product]
										,F.uotype AS Time
										,F.date AS Date
										,F.volume AS Volume									
										,F.activity_taj AS [Activity TAJ]
										,NULL AS [Activity TSO]
										,0 AS [Volume Ref]
										,NULL AS [Activity TAJ Ref]
																
									FROM XFC_PLT_FACT_Production F
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON F.id_product = P.id							
									--LEFT JOIN XFC_PLT_HIER_Product H
									--	ON F.id_product = H.id
									
									WHERE F.scenario = '{scenario}'
									AND YEAR(F.date) = {year}		
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
									AND (F.activity_taj <> 0 OR F.activity_tso <> 0)
						
								UNION ALL
							
									-- ACTIVITY TSO
				
									SELECT 
										F.id_factory AS [Id Factory]
										,F.id_averagegroup AS [Id GM]
										,F.id_costcenter AS [Id Cost Center]
										,F.id_product AS [Id Product]							
										,F.uotype AS Time
										,F.date AS Date
										,0 AS Volume										
										,NULL AS [Activity TAJ]
										,F.activity_tso AS [Activity TSO]
										,0 AS [Volume Ref]
										,NULL AS [Activity TAJ Ref]
																
									FROM activityTSO F
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON F.id_product = P.id							
									--LEFT JOIN XFC_PLT_HIER_Product H
									--	ON F.id_product = H.id
									
									WHERE 1=1
									AND (F.uotype = '{time}' OR '{time}' = 'All')								
					
								UNION ALL
							
									-- ACTIVITY TAJ REF
						
									SELECT 
										F.id_factory AS [Id Factory]
										,F.id_averagegroup AS [Id GM]
										,F.id_costcenter AS [Id Cost Center]
										,F.id_product AS [Id Product]							
										,F.uotype AS Time
										,F.date AS Date
										,0 AS Volume
										,NULL AS [Activity TAJ]
										,NULL AS [Activity TSO]
										,F.volume AS [Volume Ref]
										,F.activity_taj  AS [Activity TAJ Ref]
																
									FROM XFC_PLT_FACT_Production F
									LEFT JOIN XFC_PLT_MASTER_Product P
										ON F.id_product = P.id							
									--LEFT JOIN XFC_PLT_HIER_Product H
									--	ON F.id_product = H.id				
									
									WHERE F.scenario = '{scenarioRef}'
									AND YEAR(F.date) = {year}		
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
									AND (F.activity_taj <> 0 OR F.activity_tso <> 0)
									
								)
					"
					
				#End Region					

				#Region "OLD Activity Index - Actual"				
													
				ElseIf query = "ActivityIndexActual"					
					
							Dim sql_ActivityTSO = AllQueries(si, "ActivityTSO", factory, month, year, scenario, scenarioRef, time)	
							
							sql = sql_ActivityTSO & $"
													
								, activityTAJref AS (
									SELECT
										MONTH(F.date) AS month
										, id_averagegroup
										, uotype
										, SUM(activity_taj) AS activity_taj		
					
									FROM XFC_PLT_FACT_Production F
								
									WHERE F.scenario = '{scenarioRef}'
									AND YEAR(F.date) = {year}					
									AND F.id_factory = '{factory}'		
									AND F.activity_taj <> 0
					
									GROUP BY id_averagegroup, uotype, MONTH(F.date)
								)	
							
								, activityTAJ AS (
									SELECT
										MONTH(F.date) AS month
										, id_averagegroup
										, uotype
										, SUM(activity_taj) AS activity_taj		
					
									FROM XFC_PLT_FACT_Production F
								
									WHERE F.scenario = '{scenario}'
									AND YEAR(F.date) = {year}					
									AND F.id_factory = '{factory}'		
									AND F.activity_taj <> 0
					
									GROUP BY id_averagegroup, uotype, MONTH(F.date)
								)	
														
								, activityTSOSum AS (
									SELECT
										month
										, id_averagegroup
										, uotype
										, SUM(activity_tso) AS activity_tso
					
									FROM activityTSO F								
					
									GROUP BY id_averagegroup, uotype, month
								)															
					
								, activityIndex AS (
							
								SELECT
									A.month
									, A.id_averagegroup
									, A.uotype
									, A.activity_tso / B.activity_taj AS coefficient_tso
									, A.activity_tso / C.activity_taj AS activity_index
							
								FROM activityTSOSum A	
							
								LEFT JOIN activityTAJ B
									ON A.id_averagegroup = B.id_averagegroup
									AND A.uotype = B.uotype
									AND A.month = B.month								
							
								LEFT JOIN activityTAJref C
									ON A.id_averagegroup = C.id_averagegroup
									AND A.uotype = C.uotype
									AND A.month = C.month				
								
								)
					
							"
					
				#End Region					
					
				#Region "OLD Activity Index - Plan"				
													
				ElseIf query = "OLD_ActivityIndexPlan"					
					
					sql = $"
								WITH sceData AS (
									SELECT *
									FROM XFC_PLT_FACT_Production F
								
									WHERE F.scenario = '{scenario}'
									AND YEAR(F.date) = {year}		
									--AND MONTH(F.date) >= {month}						
									AND F.id_factory = '{factory}'	
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
									AND F.activity_taj <> 0
								)
							
								, refData AS (
									SELECT *
									FROM XFC_PLT_FACT_Production F
								
									WHERE F.scenario = '{scenarioRef}'
									AND YEAR(F.date) = {year}	
									AND F.id_factory = '{factory}'		
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
								)
							
								, sceDataTSO AS (
									SELECT 
										F.*
										,F.volume * R.allocation_taj AS activity_times_ref
																
									FROM sceData F
									LEFT JOIN refData R
										ON F.uotype = R.uotype	
										--AND F.id_costcenter = R.id_costcenter
										AND F.id_averagegroup = R.id_averagegroup
										AND F.id_product = R.id_product
										AND F.date = R.date
																		
								)
					
								, ActivityIndex AS (
							
								SELECT 
									CONCAT('M', RIGHT(CONCAT('0', CONVERT(VARCHAR(50), MONTH(A.date))), 2)) AS month							
									,A.id_factory
									,A.id_costcenter
									,A.id_averagegroup
									,A.uotype AS time
									,A.activity_taj 
									,A.activity_tso		
									,R.activity_taj AS activity_taj_ref
									,CASE WHEN ISNULL(A.activity_tso,0) = 0 THEN 1
										ELSE A.activity_tso / R.activity_taj END * 100 AS coefficient_ref
							
									FROM sceDataTSO A																
									LEFT JOIN refData R
										ON A.uotype = R.uotype	
										--AND A.id_costcenter = R.id_costcenter
										AND A.id_averagegroup = R.id_averagegroup
										AND A.id_product = R.id_product
										AND A.date = R.date															
									
								)
					
							"
					
				#End Region				
	
				#Region "VTU Data Monthly"				
													
				ElseIf query = "VTU_data_monthly"					
					
					Dim sFilterProduct As String = If(product = "", $"AND N.id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")
																  
					Dim sql_Rate = AllQueries(si, "RATE", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)	
													
					sql = sql_Rate.Replace(", fxRate ","WITH fxRate ") & $"		
					
							-- COST DATA
							
							, factCost as (
							       
								SELECT
									MONTH(F.date) AS month
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center					
									,SUM(F.value) * R1.rate as cost 									
								
								FROM XFC_PLT_FACT_CostsDistribution F
								
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
							    	ON F.id_costcenter = C.id
									--AND C.scenario = '{scenario}'
									AND C.scenario = 'Actual'
									AND F.date BETWEEN C.start_date AND C.end_date
					
					            LEFT JOIN XFC_PLT_MASTER_Account A
					                ON F.id_account = A.id     			
					
								LEFT JOIN fxRate R1
									ON MONTH(F.date) = R1.month					
								
								WHERE 1=1
									AND F.id_factory = '{factory}'
									AND YEAR(F.date) = {year}							
								
									AND F.scenario = '{scenario}'
									AND C.type = 'A'
									AND F.type_tso_taj = '{type}'
								
								GROUP BY 
									MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id		
									,R1.rate
							)
							
							-- VOLUME DATA
							
							, factProduction as (
							       
								SELECT
									MONTH(F.date) AS month
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) AS activity_TAJ
									,CASE WHEN SUM(F.volume) = 0 THEN 0  ELSE SUM(F.activity_TAJ) / SUM(F.volume) END AS allocation_TAJ
								
								FROM XFC_PLT_FACT_Production F
								
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
							    	ON F.id_costcenter = C.id
									--AND C.scenario = '{scenario}'
									AND C.scenario = 'Actual'
									AND F.date BETWEEN C.start_date AND C.end_date
					
								LEFT JOIN XFC_PLT_MASTER_AverageGroup A
									ON F.id_averagegroup = A.id					
								
								WHERE 1=1
									AND F.id_factory =  '{factory}'
									AND YEAR(F.date) = {year}												
								
									AND F.scenario = '{scenario}'
									AND C.type = 'A'
								
								GROUP BY
									MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT month
										,id_product_gm
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_product_gm, uotype, month
							
							)
							
							-- PRODUCT - COMPONENTS
							
							, Product_Component_Recursivity as (
							
								SELECT 
									N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								WHERE 1=1
								{sFilterProduct}
								--AND N.id_factory = '{factory}'
					
								UNION ALL
								
								SELECT 
									N.id_factory
									, R.id_product_final AS id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product = R.Id_component
									AND N.id_factory = R.id_factory			
					
								WHERE N.id_factory = '{factory}'
							)
					
							, factVTU AS (
					
							SELECT
								'M' + RIGHT('0' + CAST(C.month AS VARCHAR), 2) AS month
								, C.id_product
								, C.id_averagegroup
								, C.account_type					
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total
								, V2.activity_TAJ AS activity_UO2
								, T2.activity_TAJ AS activity_UO2_total						
								
							FROM ( 	SELECT month, id_product, id_averagegroup, account_type, SUM(cost) AS cost 
									FROM factCost
									GROUP BY month, id_product, id_averagegroup, account_type ) C
					
							LEFT JOIN factProduction V
							  	ON C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON C.month = T.month
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'			
					
							LEFT JOIN factProduction V2
							  	ON C.month = V2.month
								AND C.id_product = V2.id_product
								AND C.id_averagegroup = V2.id_averagegroup
								AND V2.uotype = 'UO2'	
							LEFT JOIN activityProduct AS T2
								ON C.month = T2.month
							    AND V2.id_product_gm = T2.id_product_gm
								AND T.uotype = 'UO2'	
							)
					
							"
							
				#End Region								
				
				#Region "VTU Data"				
													
				ElseIf query = "VTU_data"
						
					Dim day As String = String.Empty					
					If CInt(month) = 2 Then 
						day = "28"
					Else If CInt(month) <= 6 Then
						day = If((CInt(month) Mod 2) = 0, "30", "31")
					Else
						day = If((CInt(month) Mod 2) = 0, "31", "30")
					End If	
							
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")
																  
					Dim sql_Rate = AllQueries(si, "RATE", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)	
													
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"		
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								AND  (('{view}' = 'Periodic' AND MONTH(date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(date) <= {month}))
							
								AND YEAR(V.date) = {year} 						
								AND id_factory = '{factory}'
								AND scenario = '{scenario}'
					       )					
					
							-- COST DATA
							
							, factCost as (
							       
								SELECT
									F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * ISNULL(P.value,0)) / 100 AS cost_variable
									,SUM(F.value * R1.rate) as cost 																	
					
						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND id_factory = '{factory}'
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
						        INNER JOIN XFC_PLT_FACT_CostsDistribution F 
						           	ON F.id_costcenter = C.id				
					
					            LEFT JOIN XFC_PLT_MASTER_Account A
					                ON F.id_account = A.id     			
					
								LEFT JOIN fxRate R1
									ON MONTH(F.date) = R1.month		
					
						       	LEFT JOIN PercVarAccount P
						           	ON F.id_account = P.id_account						          	
									AND MONTH(F.date) = P.month					
								
								WHERE 1=1
									AND F.id_factory = '{factory}'
									AND YEAR(F.date) = {year}
								
									AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
								
									AND F.scenario = '{scenario}'
									AND F.type_tso_taj = '{type}'
									AND F.value <> 0
								
								GROUP BY 
									F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id		
									--,R1.rate
							)
							
							-- VOLUME DATA
							
							, factProductionFiltered as (
							  	SELECT * 
							  	FROM XFC_PLT_FACT_Production
							    WHERE id_factory =  '{factory}'
								AND YEAR(date) = {year}     
							    
								AND  (('{view}' = 'Periodic' AND MONTH(date) = {month})
								OR
								('{view}' = 'YTD' AND MONTH(date) <= {month}))
								
								AND scenario = '{scenario}'
								AND activity_TAJ <> 0
							   )					
							
							, factProduction as (
							       
								SELECT
									F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ
									--,CASE WHEN SUM(F.volume) = 0 THEN 0  ELSE SUM(F.activity_TAJ) / SUM(F.volume) END AS allocation_TAJ

						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND id_factory = '{factory}'
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
					          	INNER JOIN factProductionFiltered  F 
					              	ON F.id_costcenter = C.id           
					
						        INNER JOIN (SELECT id_factory, id_product, id_averagegroup, uotype, MAX(id_costcenter) AS id_costcenter
								  			FROM factProductionFiltered 
								  			GROUP BY id_factory, id_product, id_averagegroup, uotype) F2
									ON F.id_factory = F2.id_factory
									AND F.id_product = F2.id_product
									AND F.id_averagegroup = F2.id_averagegroup
									AND F.id_costcenter = F2.id_costcenter
									AND F.uotype  = F2.uotype 										
					
								LEFT JOIN XFC_PLT_MASTER_AverageGroup A
									ON F.id_averagegroup = A.id
								
								WHERE 1=1
									AND F.id_factory =  '{factory}'
									AND YEAR(F.date) = {year}					
								
									AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
								
									AND F.scenario = '{scenario}'
									AND activity_TAJ <> 0
								
								GROUP BY
									F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_product_gm
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_product_gm, uotype 
							
							)
							
							-- PRODUCT - COMPONENTS
							
							, Product_Component_Recursivity as (
							
								SELECT 
									N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST(N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature_Date N
								WHERE 1=1
									AND N.id_factory = '{factory}'		
									AND '{year}-{month}-{day}' BETWEEN N.start_date AND N.end_date
									{sFilterProduct}
					
								UNION ALL
								
								SELECT
									N.id_factory
									, R.id_product_final AS id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST((R.exp_coefficient * N.coefficient * N.prorata) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature_Date N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product = R.Id_component
									AND N.id_factory = R.id_factory		
								WHERE N.id_factory = '{factory}'
									AND '{year}-{month}-{day}' BETWEEN N.start_date AND N.end_date
							)
							
							, factVTU AS (
					
							SELECT 
								C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total
								, V2.activity_TAJ AS activity_UO2
								, T2.activity_TAJ AS activity_UO2_total					
								
							FROM ( 	SELECT id_product, id_averagegroup, account_type, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									GROUP BY id_product, id_averagegroup, account_type ) C
					
							LEFT JOIN factProduction V
							  	ON C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'			
					
							LEFT JOIN factProduction V2
							  	ON C.id_product = V2.id_product
								AND C.id_averagegroup = V2.id_averagegroup
								AND V2.uotype = 'UO2'	
							LEFT JOIN activityProduct AS T2
							    ON V2.id_product_gm = T2.id_product_gm
								AND T2.uotype = 'UO2'	
							)
					
							"
							
				#End Region		
				
				#Region "VTU Data (09/05/2025)"				
													
				ElseIf query = "VTU_data_OLD"					
					
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")
																  
					Dim sql_Rate = AllQueries(si, "RATE", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)	
													
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"		
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								AND  (('{view}' = 'Periodic' AND MONTH(date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(date) <= {month}))
							
								AND YEAR(V.date) = {year} 						
								AND id_factory = '{factory}'
								AND scenario = '{scenario}'
					       )					
					
							-- COST DATA
							
							, factCost as (
							       
								SELECT
									F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * ISNULL(P.value,0)) / 100 AS cost_variable
									,SUM(F.value * R1.rate) as cost 																	
					
						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND id_factory = '{factory}'
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
						        INNER JOIN XFC_PLT_FACT_CostsDistribution F 
						           	ON F.id_costcenter = C.id				
					
					            LEFT JOIN XFC_PLT_MASTER_Account A
					                ON F.id_account = A.id     			
					
								LEFT JOIN fxRate R1
									ON MONTH(F.date) = R1.month		
					
						       	LEFT JOIN PercVarAccount P
						           	ON F.id_account = P.id_account						          	
									AND MONTH(F.date) = P.month					
								
								WHERE 1=1
									AND F.id_factory = '{factory}'
									AND YEAR(F.date) = {year}
								
									AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
								
									AND F.scenario = '{scenario}'
									AND F.type_tso_taj = '{type}'
									AND F.value <> 0
								
								GROUP BY 
									F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id		
									--,R1.rate
							)
							
							-- VOLUME DATA
							
							, factProduction as (
							       
								SELECT
									F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ
									--,CASE WHEN SUM(F.volume) = 0 THEN 0  ELSE SUM(F.activity_TAJ) / SUM(F.volume) END AS allocation_TAJ

						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND id_factory = '{factory}'
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
						        INNER JOIN XFC_PLT_FACT_Production F 
						           	ON F.id_costcenter = C.id											
					
								LEFT JOIN XFC_PLT_MASTER_AverageGroup A
									ON F.id_averagegroup = A.id
								
								WHERE 1=1
									AND F.id_factory =  '{factory}'
									AND YEAR(F.date) = {year}					
								
									AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
								
									AND F.scenario = '{scenario}'
									AND activity_TAJ <> 0
								
								GROUP BY
									F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_product_gm
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_product_gm, uotype 
							
							)
							
							-- PRODUCT - COMPONENTS
							
							, Product_Component_Recursivity as (
							
								SELECT 
									N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								WHERE 1=1
								AND N.id_factory = '{factory}'		
								{sFilterProduct}
					
								UNION ALL
								
								SELECT
									N.id_factory
									, R.id_product_final AS id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product = R.Id_component
									AND N.id_factory = R.id_factory		
								WHERE N.id_factory = '{factory}'
							)
							
							, factVTU AS (
					
							SELECT 
								C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total
								, V2.activity_TAJ AS activity_UO2
								, T2.activity_TAJ AS activity_UO2_total					
								
							FROM ( 	SELECT id_product, id_averagegroup, account_type, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									GROUP BY id_product, id_averagegroup, account_type ) C
					
							LEFT JOIN factProduction V
							  	ON C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'			
					
							LEFT JOIN factProduction V2
							  	ON C.id_product = V2.id_product
								AND C.id_averagegroup = V2.id_averagegroup
								AND V2.uotype = 'UO2'	
							LEFT JOIN activityProduct AS T2
							    ON V2.id_product_gm = T2.id_product_gm
								AND T2.uotype = 'UO2'	
							)
					
							"
							
				#End Region												
				
				#Region "VTU Data ALL Factories"				
													
				ElseIf query = "VTU_data_All_Factories"										
				
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")				
					
					month = Right("0" & month,2)
					Dim dateStart As String = year & "-01-01"
					Dim dateEnd As String = year & "-" & month & "-01"						
					
					Dim day As String = String.Empty					
					If CInt(month) = 2 Then 
						day = "28"
					Else If CInt(month) <= 6 Then
						day = If((CInt(month) Mod 2) = 0, "30", "31")
					Else
						day = If((CInt(month) Mod 2) = 0, "31", "30")
					End If	
					
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", "", year, scenario, scenarioRef, time, "", "Existing", "", currency)	
													
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"	
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT id_factory, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
							
								WHERE 1=1
							
								AND  (('{view}' = 'Periodic' AND MONTH(date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(date) <= {month}))
							
								AND YEAR(V.date) = {year} 		
					
								AND scenario = '{scenario}'					
					       )					
					
							-- COST DATA
							
							, factCost as (
							       
								SELECT
									F.id_factory
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * ISNULL(P.value,0)) / 100 AS cost_variable
									,SUM(F.value * R1.rate) as cost 																	
					
						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
						        INNER JOIN XFC_PLT_FACT_CostsDistribution F 
						           	ON F.id_costcenter = C.id				
					
					            LEFT JOIN XFC_PLT_MASTER_Account A
					                ON F.id_account = A.id     			
					
								LEFT JOIN fxRate R1
									ON MONTH(F.date) = R1.month	
									AND F.id_factory = R1.id_factory
					
						       	LEFT JOIN PercVarAccount P
						           	ON F.id_account = P.id_account						          	
									AND F.id_factory = P.id_factory
									AND MONTH(F.date) = P.month					
								
								WHERE F.scenario = '{scenario}'
					
									AND YEAR(F.date) = {year}
								
									AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
					
									AND F.type_tso_taj = 'TAJ'					
									AND F.value <> 0
									--AND A.account_type IN (1,2,3,4,5)
								
								GROUP BY 
									F.id_factory
									,F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id		
									--,R1.rate
							)
							
							-- VOLUME DATA
					
							, factProductionFiltered as (
							  	SELECT * 
							  	FROM XFC_PLT_FACT_Production
								WHERE
									(('{view}' = 'Periodic' AND MONTH(date) = {month})
									OR
									('{view}' = 'YTD' AND MONTH(date) <= {month}))
								
								AND YEAR(date) = {year}   
								AND scenario = '{scenario}'
								AND activity_TAJ <> 0
							   )					
							
							, factProduction as (
							       
								SELECT
									F.id_factory			
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ
									--,CASE WHEN SUM(F.volume) = 0 THEN 0  ELSE SUM(F.activity_TAJ) / SUM(F.volume) END AS allocation_TAJ

						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
					          	INNER JOIN factProductionFiltered  F 
					              	ON F.id_costcenter = C.id           
					
						        INNER JOIN (SELECT id_factory, id_product, id_averagegroup, uotype, MAX(id_costcenter) AS id_costcenter
								  			FROM factProductionFiltered 
								  			GROUP BY id_factory, id_product, id_averagegroup, uotype) F2
									ON F.id_factory = F2.id_factory
									AND F.id_product = F2.id_product
									AND F.id_averagegroup = F2.id_averagegroup
									AND F.id_costcenter = F2.id_costcenter
									AND F.uotype  = F2.uotype 															
					
								LEFT JOIN XFC_PLT_MASTER_AverageGroup A
									ON F.id_averagegroup = A.id
									AND F.id_factory = A.id_factory
							
								GROUP BY
									F.id_factory
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_factory
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, id_product_gm, uotype 
							
							)
							
							-- PRODUCT - COMPONENTS
							
							, Product_Component_Recursivity as (
							
								SELECT 
									N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									-- , CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient
									, CAST(N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								-- FROM XFC_PLT_HIER_Nomenclature N
								FROM XFC_PLT_HIER_Nomenclature_Date N
								WHERE 1=1	
									{sFilterProduct}
									AND '{year}-{month}-{day}' BETWEEN N.start_date AND N.end_date
					
								UNION ALL
								
								SELECT
									N.id_factory
									, R.id_product_final AS id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									-- , CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient
									, CAST((R.exp_coefficient * N.coefficient * N.prorata) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								-- FROM XFC_PLT_HIER_Nomenclature N
								FROM XFC_PLT_HIER_Nomenclature_Date N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product = R.Id_component
									AND N.id_factory = R.id_factory	
								
								WHERE '{year}-{month}-{day}' BETWEEN N.start_date AND N.end_date									
							)
							
							, factVTU AS (
					
							SELECT 
								C.id_factory
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total
								, V2.activity_TAJ AS activity_UO2
								, T2.activity_TAJ AS activity_UO2_total					
								
							FROM ( 	SELECT id_factory, id_product, id_averagegroup, account_type, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									GROUP BY id_factory, id_product, id_averagegroup, account_type ) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'			
					
							LEFT JOIN factProduction V2
							  	ON C.id_factory = V2.id_factory
								AND C.id_product = V2.id_product
								AND C.id_averagegroup = V2.id_averagegroup
								AND V2.uotype = 'UO2'	
							LEFT JOIN activityProduct AS T2
							    ON V2.id_factory = T2.id_factory
								AND V2.id_product_gm = T2.id_product_gm
								AND T2.uotype = 'UO2'	
							)
					
							"
							
				#End Region	
				
				#Region "VTU Data ALL Factories (09/05/2025)"				
													
				ElseIf query = "VTU_data_All_Factories_OLD"										
				
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")				
					
					month = Right("0" & month,2)
					Dim dateStart As String = year & "-01-01"
					Dim dateEnd As String = year & "-" & month & "-01"						
					
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", "", year, scenario, scenarioRef, time, "", "Existing", "", currency)	
													
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"		
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT id_factory, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
							
								WHERE date BETWEEN '{dateStart}' AND '{dateEnd}'					
								AND scenario = '{scenario}'
					       )					
					
							-- COST DATA
							
							, factCost as (
							       
								SELECT
									F.id_factory
									,F.date
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * ISNULL(P.value,0)) / 100 AS cost_variable
									,SUM(F.value * R1.rate) as cost 																	
					
						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
						        INNER JOIN XFC_PLT_FACT_CostsDistribution F 
						           	ON F.id_costcenter = C.id				
					
					            LEFT JOIN XFC_PLT_MASTER_Account A
					                ON F.id_account = A.id     			
					
								LEFT JOIN fxRate R1
									ON MONTH(F.date) = R1.month	
									AND F.id_factory = R1.id_factory
					
						       	LEFT JOIN PercVarAccount P
						           	ON F.id_account = P.id_account						          	
									AND F.id_factory = P.id_factory
									AND MONTH(F.date) = P.month					
								
								WHERE F.date BETWEEN '{dateStart}' AND '{dateEnd}'	
									AND F.scenario = '{scenario}'
									AND F.type_tso_taj = 'TAJ'					
									AND F.value <> 0
								
								GROUP BY 
									F.id_factory
									,F.date
									,F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id		
									--,R1.rate
							)
							
							-- VOLUME DATA									
							
							, factProduction as (
							       
								SELECT
									F.id_factory
									,DATEFROMPARTS({year},MONTH(F.date),1) AS date					
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ
									--,CASE WHEN SUM(F.volume) = 0 THEN 0  ELSE SUM(F.activity_TAJ) / SUM(F.volume) END AS allocation_TAJ

						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
					          	INNER JOIN XFC_PLT_FACT_Production F 
					              	ON F.id_costcenter = C.id           															
					
								LEFT JOIN XFC_PLT_MASTER_AverageGroup A
									ON F.id_averagegroup = A.id
									AND F.id_factory = A.id_factory
					
								WHERE YEAR(F.date) = {year}
									AND MONTH(F.date) <= {month}
								
									AND F.scenario = '{scenario}'
									AND F.activity_TAJ <> 0
							
								GROUP BY
									F.id_factory
									,DATEFROMPARTS({year},MONTH(F.date),1)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_factory
										,date
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, date, id_product_gm, uotype 
							
							)
							
							-- PRODUCT - COMPONENTS
							
							, Product_Component_Recursivity as (
							
								SELECT 
									N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								WHERE 1=1	
								{sFilterProduct}
					
								UNION ALL
								
								SELECT
									N.id_factory
									, R.id_product_final AS id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product = R.Id_component
									AND N.id_factory = R.id_factory		
							)
							
							, factVTU AS (
					
							SELECT 
								C.id_factory
								, C.date
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total
								, V2.activity_TAJ AS activity_UO2
								, T2.activity_TAJ AS activity_UO2_total					
								
							FROM ( 	SELECT id_factory, date, id_product, id_averagegroup, account_type, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									GROUP BY id_factory, date, id_product, id_averagegroup, account_type ) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.date = V.date
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.date = T.date
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'			
					
							LEFT JOIN factProduction V2
							  	ON C.id_factory = V2.id_factory
								AND C.date = V2.date
								AND C.id_product = V2.id_product
								AND C.id_averagegroup = V2.id_averagegroup
								AND V2.uotype = 'UO2'	
							LEFT JOIN activityProduct AS T2
							    ON V2.id_factory = T2.id_factory
								AND V2.date = T2.date
								AND V2.id_product_gm = T2.id_product_gm
								AND T2.uotype = 'UO2'	
							)
					
							"
							
				#End Region					
				
				#Region "VTU 12 Months"				
													
				ElseIf query = "VTU_12_Months"					
					
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")
																  
					Dim sql_Rate = AllQueries(si, "RATE_2_Years", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)	
													
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"		
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT V.date, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
								AND YEAR(V.date) = {year} OR YEAR(V.date) = {year} - 1						
								AND id_factory = '{factory}'
								AND scenario = '{scenario}'
					       )						
					
							-- COST DATA
							
							, factCost as (
							       
								SELECT
									F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center		
									,SUM(cost_fixed) as cost_fixed	
									,SUM(cost_variable) as cost_variable	
									,SUM(cost) as cost 											
								
								FROM (
									SELECT
										DATEFROMPARTS({year},{month},1) AS date
										,id_product
										,id_averagegroup
										,id_costcenter
										,F.id_account
										,SUM(F.value * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
										,SUM(F.value * ISNULL(P.value,0)) / 100 AS cost_variable
										,SUM(F.value * R1.rate) as cost 
					
									FROM XFC_PLT_FACT_CostsDistribution F
					
									LEFT JOIN fxRate R1
										ON MONTH(F.date) = R1.month		
						
							       	LEFT JOIN PercVarAccount P
							           	ON F.id_account = P.id_account						          	
										AND F.date = P.date	
					
									WHERE 1=1
									AND id_factory = '{factory}'
									
									AND ((YEAR(F.date) = {year} AND MONTH(F.date) <= {month}) 
									  OR (YEAR(F.date) = {year} - 1 AND MONTH(F.date) > {month}))
								
									AND scenario = '{scenario}'
									AND type_tso_taj = '{type}'					
					
									GROUP BY                  
										id_product
										,id_averagegroup
										,id_costcenter
										,F.id_account) F
								
								INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
							    	ON F.id_costcenter = C.id
									--AND C.scenario = '{scenario}'
									AND C.scenario = 'Actual'
									AND C.type = 'A'
									AND DATEFROMPARTS({year},{month},1) BETWEEN C.start_date AND C.end_date
					
					            LEFT JOIN XFC_PLT_MASTER_Account A
					                ON F.id_account = A.id     			
													
								GROUP BY 
									F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id											
							)
							
							-- VOLUME DATA
							
							, factProduction as (
							       
								SELECT
									F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ
								
								FROM (
									SELECT
										DATEFROMPARTS({year},{month},1) AS date
										,id_product
										,id_averagegroup
										,id_costcenter
										,uotype 
										,SUM(volume) as volume
										,SUM(activity_TAJ) as activity_TAJ
					
									FROM XFC_PLT_FACT_Production
									
									WHERE 1=1
									AND id_factory =  '{factory}'				
								
									AND ((YEAR(date) = {year} AND MONTH(date) <= {month}) 
									  OR (YEAR(date) = {year} - 1 AND MONTH(date) > {month}))
								
									AND scenario = '{scenario}'
									
									AND activity_TAJ <> 0
					
									GROUP BY
										id_product
										,id_averagegroup
										,id_costcenter
										,uotype 
									) F
								
								INNER JOIN XFC_PLT_MASTER_CostCenter_Hist C
							    	ON F.id_costcenter = C.id
									--AND C.scenario = '{scenario}'
									AND C.scenario = 'Actual'
									AND C.type = 'A'
									AND DATEFROMPARTS({year},{month},1) BETWEEN C.start_date AND C.end_date									
					
								LEFT JOIN XFC_PLT_MASTER_AverageGroup A
									ON F.id_averagegroup = A.id

								GROUP BY
									F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_product_gm, uotype 
							
							)
							
							-- PRODUCT - COMPONENTS
							
							, Product_Component_Recursivity as (
							
								SELECT 
									N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								WHERE 1=1
								AND N.id_factory = '{factory}'		
								{sFilterProduct}
					
								UNION ALL
								
								SELECT
									N.id_factory
									, R.id_product_final AS id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								FROM XFC_PLT_HIER_Nomenclature N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product = R.Id_component
									AND N.id_factory = R.id_factory		
								WHERE N.id_factory = '{factory}'
							)
							
							, factVTU AS (
					
							SELECT 
								C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total
								, V2.activity_TAJ AS activity_UO2
								, T2.activity_TAJ AS activity_UO2_total					
								
							FROM ( 	SELECT id_product, id_averagegroup, account_type, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									GROUP BY id_product, id_averagegroup, account_type ) C														
					
							LEFT JOIN factProduction V
							  	ON C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'			
					
							LEFT JOIN factProduction V2
							  	ON C.id_product = V2.id_product
								AND C.id_averagegroup = V2.id_averagegroup
								AND V2.uotype = 'UO2'	
							LEFT JOIN activityProduct AS T2
							    ON V2.id_product_gm = T2.id_product_gm
								AND T2.uotype = 'UO2'	
							)
					
							"
							
				#End Region					

				#Region "RATE"
				
				Else If query = "RATE"
							
							Dim scenarioTypeId As Integer = BRApi.Finance.Dim.GetDimPk(si, "Scenarios").DimTypeId
							
							' RATE Scenario
							Dim scenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, scenarioTypeId, scenario)
							Dim type_rate As String = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, scenarioId).Name
'							Dim type_rate As String = "AverageRate"

							' RATE Scenario Reference
							Dim type_rate_ref As String = String.Empty
							If scenarioRef <> String.Empty Then
								Dim scenarioRefId As Integer = BRApi.Finance.Members.GetMemberId(si, scenarioTypeId, scenarioRef)
								type_rate_ref = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, scenarioRefId).Name
'								Dim type_rate_ref As String = "BudV3AvgRate"
							End If

							sql = $"
							
							, fxRateRef AS (
								SELECT 
									M.month
									, CASE WHEN '{factory}' = 'R0529002' THEN 1 ELSE ISNULL(FX.rate,1) END AS rate
									, ISNULL(FX.rate,1) AS rate_exception_calc	
									
								FROM (
											SELECT 1 AS month  UNION ALL SELECT 2  UNION ALL SELECT 3  UNION ALL SELECT 4 
											UNION ALL SELECT 5 UNION ALL SELECT 6  UNION ALL SELECT 7  UNION ALL SELECT 8 
											UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M  
								LEFT JOIN (
											SELECT month, rate
											FROM XFC_MAIN_AUX_fxrate R
											INNER JOIN XFC_PLT_MASTER_Factory F
												ON R.source = F.currency						
											WHERE F.id = '{factory}'
											AND year = {year} 
											AND type = '{type_rate_ref}'
											AND target = '{currency}') FX
									ON M.month = FX.month							
							)			
							
							, fxRate AS (
								SELECT 
									M.month
									, CASE WHEN '{factory}' = 'R0529002' THEN 1 ELSE ISNULL(FX.rate,1) END AS rate
									, ISNULL(FX.rate,1) AS rate_all
 									, CASE WHEN '{factory}' = 'R0529002' THEN ISNULL(FX.rate,1) / ISNULL(R2.rate_exception_calc,1)  ELSE 1 END AS rate_exception
								FROM (
											SELECT 1 AS month  UNION ALL SELECT 2  UNION ALL SELECT 3  UNION ALL SELECT 4 
											UNION ALL SELECT 5 UNION ALL SELECT 6  UNION ALL SELECT 7  UNION ALL SELECT 8 
											UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M  
								LEFT JOIN (
											SELECT month, rate
											FROM XFC_MAIN_AUX_fxrate R
											INNER JOIN XFC_PLT_MASTER_Factory F
												ON R.source = F.currency						
											WHERE F.id = '{factory}'
											AND year = {year} 
											AND type = '{type_rate}'
											AND target = '{currency}') FX
									ON M.month = FX.month	
								
								LEFT JOIN fxRateRef R2
 									ON FX.month = R2.month
							)							
							"
							
				#End Region
				
				#Region "RATE All Factories"
				
				Else If query = "RATE_All_Factories"
							
							Dim scenarioTypeId As Integer = BRApi.Finance.Dim.GetDimPk(si, "Scenarios").DimTypeId
							
							' RATE Scenario
							Dim scenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, scenarioTypeId, scenario)
							Dim type_rate As String = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, scenarioId).Name
'							Dim type_rate As String = "AverageRate"

							' RATE Scenario Reference
							Dim type_rate_ref As String = String.Empty
							If scenarioRef <> String.Empty Then
								Dim scenarioRefId As Integer = BRApi.Finance.Members.GetMemberId(si, scenarioTypeId, scenarioRef)
								type_rate_ref = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, scenarioRefId).Name
'								Dim type_rate_ref As String = "BudV3AvgRate"
							End If
					
							sql = $"
							, fxRateRef AS (
								SELECT 
									M.month
									, F.id AS id_factory
									, CASE WHEN F.id = 'R0529002' THEN 1 ELSE ISNULL(FX.rate,1) END AS rate									
									, ISNULL(FX.rate,1) AS rate_exception_calc
								FROM (
											SELECT 1 AS month  UNION ALL SELECT 2  UNION ALL SELECT 3  UNION ALL SELECT 4 
											UNION ALL SELECT 5 UNION ALL SELECT 6  UNION ALL SELECT 7  UNION ALL SELECT 8 
											UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M 
								
								CROSS JOIN XFC_PLT_MASTER_Factory F							
							
								LEFT JOIN (
           									SELECT month, F.id, rate
											FROM XFC_MAIN_AUX_fxrate R	
											INNER JOIN XFC_PLT_MASTER_Factory F
												ON R.source = F.currency								
											WHERE year = {year} 
											AND type = '{type_rate_ref}'
											AND target = '{currency}') FX
									ON M.month = FX.month	
									AND F.id = FX.id							
							)	
							
							, fxRate AS (
								SELECT 
									M.month
									, F.id AS id_factory
									, CASE WHEN F.id = 'R0529002' THEN 1 ELSE ISNULL(FX.rate,1) END AS rate
									, ISNULL(FX.rate,1) AS rate_all
 									, CASE WHEN F.id = 'R0529002' THEN ISNULL(FX.rate,1) / ISNULL(R2.rate_exception_calc,1)  ELSE 1 END AS rate_exception
								FROM (
											SELECT 1 AS month  UNION ALL SELECT 2  UNION ALL SELECT 3  UNION ALL SELECT 4 
											UNION ALL SELECT 5 UNION ALL SELECT 6  UNION ALL SELECT 7  UNION ALL SELECT 8 
											UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M  
								
								CROSS JOIN XFC_PLT_MASTER_Factory F
							
								LEFT JOIN (
           									SELECT month, F.id, rate
											FROM XFC_MAIN_AUX_fxrate R	
											INNER JOIN XFC_PLT_MASTER_Factory F
												ON R.source = F.currency	
											WHERE year = {year} 
											AND type = '{type_rate}'
											AND target = '{currency}') FX
									ON M.month = FX.month
									AND F.id = FX.id
							
								LEFT JOIN fxRateRef R2
 									ON FX.month = R2.month	
									AND FX.id = R2.id_factory
							)
											
							"
							
				#End Region	
				
				#Region "RATE 2 years"
				
				Else If query = "RATE_2_Years"							
					
							Dim scenarioTypeId As Integer = BRApi.Finance.Dim.GetDimPk(si, "Scenarios").DimTypeId
							
							' RATE Scenario
							Dim scenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, scenarioTypeId, scenario)
							Dim type_rate As String = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, scenarioId).Name
'							Dim type_rate As String = "AverageRate"

							' RATE Scenario Reference
							Dim type_rate_ref As String = String.Empty
							If scenarioRef <> String.Empty Then
								Dim scenarioRefId As Integer = BRApi.Finance.Members.GetMemberId(si, scenarioTypeId, scenarioRef)
								type_rate_ref = BRApi.Finance.Scenario.GetFxRateTypeForRevenueExpense(si, scenarioRefId).Name
'								Dim type_rate_ref As String = "BudV3AvgRate"
							End If
					
							sql = $"
							, fxRateRef AS (
								SELECT 
									Y.year
									, M.month
									, CASE WHEN '{factory}' = 'R0529002' THEN 1 ELSE ISNULL(FX.rate,1) END AS rate
									, ISNULL(FX.rate,1) AS rate_exception_calc
								FROM (
											SELECT 1 AS month  UNION ALL SELECT 2  UNION ALL SELECT 3  UNION ALL SELECT 4 
											UNION ALL SELECT 5 UNION ALL SELECT 6  UNION ALL SELECT 7  UNION ALL SELECT 8 
											UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M  
								CROSS JOIN 
									(
											SELECT {year} AS year UNION ALL SELECT {year} - 1
									) AS Y							
								LEFT JOIN (
											SELECT year, month, rate
											FROM XFC_MAIN_AUX_fxrate R
											INNER JOIN XFC_PLT_MASTER_Factory F
												ON R.source = F.currency						
											WHERE F.id = '{factory}'
											AND (year = {year} OR year = {year} - 1)
											AND type = '{type_rate_ref}'
											AND target = '{currency}') FX
									ON M.month = FX.month
									AND Y.year = FX.year
							)			
							
							, fxRate AS (
								SELECT 
									Y.year
									, M.month
									, CASE WHEN '{factory}' = 'R0529002' THEN 1 ELSE ISNULL(FX.rate,1) END AS rate
									, ISNULL(FX.rate,1) AS rate_all
 									, CASE WHEN '{factory}' = 'R0529002' THEN ISNULL(FX.rate,1) / ISNULL(R2.rate_exception_calc,1)  ELSE 1 END AS rate_exception
								FROM (
											SELECT 1 AS month  UNION ALL SELECT 2  UNION ALL SELECT 3  UNION ALL SELECT 4 
											UNION ALL SELECT 5 UNION ALL SELECT 6  UNION ALL SELECT 7  UNION ALL SELECT 8 
											UNION ALL SELECT 9 UNION ALL SELECT 10 UNION ALL SELECT 11 UNION ALL SELECT 12) M  
								CROSS JOIN 
									(
											SELECT {year} AS year UNION ALL SELECT {year} - 1
									) AS Y								
								LEFT JOIN (
											SELECT month, rate
											FROM XFC_MAIN_AUX_fxrate R
											INNER JOIN XFC_PLT_MASTER_Factory F
												ON R.source = F.currency						
											WHERE F.id = '{factory}'
											AND (year = {year} OR year = {year} - 1)
											AND type = '{type_rate}'
											AND target = '{currency}') FX
									ON M.month = FX.month	
								
								LEFT JOIN fxRateRef R2
 									ON FX.month = R2.month
									AND Y.year = FX.year
							)								
							"
							
				#End Region				
				
				#Region "Variance Analysis OLD 19/05/2025"
				
				Else If query = "VarianceAnalysisOLD"
																	
							Dim sql_ActivityIndex = AllQueries(si, "ActivityALL", factory, month, year, scenario, scenarioRef, "All")	
							Dim sql_Rate = AllQueries(si, "RATE", factory, month, year, scenario, scenarioRef, time, "", "", "", currency)	
							
							sql = sql_ActivityIndex & sql_Rate & $"		
							
							, ActivityIndex AS (
							
								SELECT
									month
									, [Id GM] AS id_averagegroup
									, Time
									, ISNULL(X.value, A.activity_index) AS activity_index
							
								FROM (
									SELECT 
										[Id Factory]
										, MONTH(A.date) AS month
										, [Id GM]
										, Time
										,CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])),0) <> 0 THEN
												CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END * 100 AS activity_index	
									
									FROM ActivityAll A								
								
					             	WHERE 1=1
								 	AND YEAR(A.date) = {year} 
							
									AND  (('{view}' = 'Periodic' AND MONTH(A.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(A.date) <= {month}))							
							
								 	AND [Id Factory] = '{factory}'							
					
									GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
								) A
							
								LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
									ON A.[Id GM] = X.id_averagegroup
									AND A.month = MONTH(X.date)
									AND A.[Id Factory] = X.id_factory
									AND A.Time = X.uotype
									AND X.scenario = '{scenario}'
									AND X.scenario_ref = '{scenarioRef}'
							)									
					       
					       , PercVarAccount AS (
					       
								SELECT DISTINCT scenario, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								AND  (('{view}' = 'Periodic' AND MONTH(date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(date) <= {month}))
							
								AND YEAR(V.date) = {year} 						
								AND id_factory = '{factory}'
					       )					       					       
					       
					       , ActFact AS (
					       
								SELECT
									F.id_factory AS id_factory
								 	, MONTH(F.date) AS Month  
									, YEAR(F.date) AS Year   
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter
									, C.nature
								 	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = '{scenario}'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id        
								
								WHERE 1=1
								AND F.scenario = '{scenario}'
								AND YEAR(F.date) = {year} 
								AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))	
								AND F.id_factory = '{factory}'
								AND CONVERT(INT,A.account_type) <> 0
					          	--AND C.type = 'A'
					       
								GROUP BY
									F.id_factory
								  	, MONTH(F.date)
									, YEAR(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description
									, C.nature
								  	, A.account_type       
					       )
					       
					       , RefFact AS (
					       
								SELECT
									F.id_factory AS id_factory
								  	, MONTH(F.date) AS Month   
									, YEAR(F.date) AS Year   
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter	
									, C.nature
								  	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = '{scenario}'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id 
								
								WHERE 1=1
								AND F.scenario = '{scenarioRef}'
								AND YEAR(F.date) = {year} 
								AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))	
								AND F.id_factory = '{factory}'
								AND CONVERT(INT,A.account_type) <> 0
								--AND C.type = 'A'
								
								GROUP BY
									F.id_factory
									, MONTH(F.date)
									, YEAR(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description		
									, C.nature
									, A.account_type
					       
					       )
					       
					       , ActRefFinalData AS (
					       
					       SELECT 
					        	id_factory
					        	, month
								, year
					        	, account_type AS id_account_type
								, id_account
								, id_costcenter
								, costcenter
								, nature
					        	, variability
					         	, SUM(act_value) AS act_value
					        	, SUM(ref_value) AS ref_value
					        	, SUM(ref_value_adj_100) AS ref_value_adj_100
					        	, SUM(ref_value_adj_semi) AS ref_value_adj_semi
					       FROM (
					       
					       -- Actual FIXED
							
					       SELECT 
					          	F.id_factory
					          	, F.month
								, F.year
					            , F.account_type
								, F.id_account
								, F.id_averagegroup
								, F.id_costcenter
								, F.costcenter
								, F.nature
					          	, 'F' AS variability
					          	, F.value * (100 - ISNULL(P.value,0)) / 100 AS act_value
					        	, 0 AS ref_value
					          	, 0 AS ref_value_adj_100
					          	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_account = P.id_account
					          	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Actual VARIABLE
							
					       SELECT 
					           	F.id_factory
					           	, F.month
								, F.year
					        	, F.account_type
								, F.id_account
								, F.id_averagegroup		
								, F.id_costcenter
								, F.costcenter		
								, F.nature
					        	, 'V' AS variability
					          	, F.value * P.value / 100 AS value
					        	, 0 AS ref_value
					         	, 0 AS ref_value_adj_100
					         	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					            ON F.id_account = P.id_account
					           	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Reference FIXED
							
					       SELECT 
					           	F.id_factory
					           	, F.month
								, F.year
					          	, F.account_type
								, F.id_account
								, F.id_averagegroup	
								, F.id_costcenter
								, F.costcenter			
								, F.nature
					           	, 'F' AS variability
					         	, 0 AS act_value
					        	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value
					         	, F.value * (100 - ISNULL(P.value,0)) / 100
									* CASE WHEN F.account_type = 1 THEN ISNULL(UO1.activity_index,100) 
									  	   WHEN F.account_type IN (2,3,4,5) THEN ISNULL(UO1.activity_index,100) 
									  	   ELSE 100 END / 100 AS ref_value_adj_100
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_semi
					       
					       FROM RefFact  F		
					       LEFT JOIN PercVarAccount P
								ON F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'
						   LEFT JOIN ActivityIndex UO2
								ON F.id_averagegroup = UO2.id_averagegroup 	
								AND F.month = UO2.month
								AND UO2.time = 'UO2'							
					       
					       UNION ALL
					       
					       -- Reference VARIABLE
							
					       SELECT 
					            F.id_factory
					            , F.month
								, F.year
					         	, F.account_type
								, F.id_account
								, F.id_averagegroup		
								, F.id_costcenter
								, F.costcenter		
								, F.nature
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, F.value * P.value / 100 AS ref_value
					          	, F.value * P.value / 100
									* CASE WHEN F.account_type = 1 THEN ISNULL(UO1.activity_index,100) 
									  	   WHEN F.account_type IN (2,3,4,5) THEN ISNULL(UO1.activity_index,100)
									  	   ELSE 1 END / 100	AS ref_value_adj_100
					         	, F.value * P.value / 100
									* CASE WHEN F.account_type = 1 THEN ISNULL(UO1.activity_index,100) 
									  	   WHEN F.account_type IN (2,3,4,5) THEN ISNULL(UO1.activity_index,100) 
									  	   ELSE 1 END / 100 AS ref_value_adj_semi
					       
					       FROM RefFact F
					       LEFT JOIN PercVarAccount P
					            ON F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'
						   LEFT JOIN ActivityIndex UO2
								ON F.id_averagegroup = UO2.id_averagegroup 		
								AND F.month = UO2.month
								AND UO2.time = 'UO2'			
					       
					       ) AS Res
					       
					       WHERE act_value <> 0 OR ref_value  <> 0
					       
					       GROUP BY 
					         id_factory
					         , month
							 , year
					         , account_type
							 , id_account
							 , id_costcenter
							 , costcenter	
							 , nature
					         , variability
					       )
							
						-- Effects Price Energy
						, effectEnergyPrice as (
							SELECT
								MONTH(C.date) as Month
								, C.id_factory as Factory
								, C.energy_type
								, C.value AS Consumo
								, P1.value AS Price1
								, P2.value AS Price2
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN ISNULL(VE.value,0) / 100
									   WHEN C.energy_type = 'G' THEN ISNULL(VG.value,0) / 100 END
									AS effect_price_variable
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN (100 - ISNULL(VE.value,0)) / 100
									   WHEN C.energy_type = 'G' THEN (100 - ISNULL(VG.value,0)) / 100 END
									AS effect_price_fixed

							FROM ( 	SELECT * 
									FROM XFC_PLT_AUX_EnergyVariance         
									WHERE 1=1
									AND id_factory = '{factory}'
									AND scenario = '{scenario}'
									AND indicator = 'Consumption' 
									AND YEAR(date) = {year}
							) C        

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P1
								ON C.id_factory = P1.id_factory
								AND C.date = P1.date
								AND C.energy_type = P1.energy_type
								AND P1.indicator = 'Price'
								AND P1.scenario = '{scenario}'

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P2
								ON C.id_factory = P2.id_factory
								AND C.date = P2.date
								AND C.energy_type = P2.energy_type
								AND P2.indicator = 'Price'
								AND P2.scenario = '{scenarioref}' 

							LEFT JOIN (SELECT 'E' AS energy_type, value, scenario, month FROM PercVarAccount WHERE id_account = '3D32') VE
								ON C.energy_type = VE.energy_type
								AND MONTH(C.date) = VE.month
								AND VE.scenario =  '{scenarioRef}' 

							LEFT JOIN (SELECT 'G' AS energy_type, value, scenario, month FROM PercVarAccount WHERE id_account = '3D34') VG
								ON C.energy_type = VG.energy_type
								AND MONTH(C.date) = VG.month
								AND VG.scenario =  '{scenarioRef}' 																
						)					       
							
						, VarianceAnalysis as (
					       SELECT DISTINCT
					       	A.id_factory
					       	, A.month
					       	, A.id_account_type
							, N.description AS desc_account_type
					       	, A.variability
							, CASE WHEN ref_value <> 0 THEN ref_value_adj_100 / ref_value * 100 END AS [Activity Index]
					       	, ref_value * R2.rate AS [Reference]
					       	, ref_value_adj_100 * R2.rate AS [Ref. 100% Adjusted]
					       	, (ref_value_adj_semi - ref_value_adj_100) * R2.rate AS [Absorption of Fixed Costs]
					       	, ref_value_adj_semi * R2.rate AS [Ref. Semi Adjusted]
					       	, SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate AS [Scope Change]
					       	, (ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0) AS [Ref. Adjusted]
					       	, ((ref_value_adj_semi * R1.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0))
							 - ((ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)) AS [Impact Parity]
					       	, (ref_value_adj_semi * R1.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0) AS [Ref. Adjusted at Actual Parity]
					       	, ISNULL(SUM(CASE WHEN id_indicator IN ('AGS','MIX','ORG','NOR','SAL', 'DIV') THEN E.value / 1000 END) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate,0) AS [Price/Salary Trend]
					       	, SUM(CASE WHEN id_indicator = 'AGS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend AGS effect]
					       	, SUM(CASE WHEN id_indicator = 'MIX' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Mix population Effect]
					       	, SUM(CASE WHEN id_indicator = 'ORG' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Organization Effect]
					       	, SUM(CASE WHEN id_indicator = 'NOR' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Noria Effect]
					       	, SUM(CASE WHEN id_indicator = 'SAL' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Wages other]
					       	, AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate AS [Price/Salary Trend Electricity Effect]
					       	, AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate AS [Price/Salary Trend Gaz Effect]
					       	, SUM(CASE WHEN id_indicator = 'CTS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Price variance other than Elec/Gaz]
					       	, 0 AS [MEC]
					       	, SUM(CASE WHEN id_indicator = 'PPR' THEN E.value / 1000 END) * R1.rate AS [Product/Process Evolution]
					       	, SUM(CASE WHEN id_indicator = 'ELB?' THEN E.value / 1000 END) * R1.rate AS [Excess Labour]
					       	, SUM(CASE WHEN id_indicator = 'UNE?' THEN E.value / 1000 END) * R1.rate AS [Unemployement]
					       	, SUM(CASE WHEN id_indicator IN ('POR','PHC','COV','DIV') THEN E.value / 1000 END) * R1.rate AS [Productivity]
					       	, SUM(CASE WHEN id_indicator = 'POR' THEN E.value / 1000 END) * R1.rate AS [Organization Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'PHC' THEN E.value / 1000 END) * R1.rate AS [Hyper Competitiveness Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'COV' THEN E.value / 1000 END) * R1.rate AS [COVID 19 Effect]
					       	, SUM(CASE WHEN id_indicator = 'DIV' THEN E.value / 1000 END) * R1.rate AS [Extra Productivity Effect]
					       	, (act_value * R1.rate) - 
								((ref_value_adj_semi * R1.rate) 
								+ ISNULL(SUM(E.value / 1000) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate,0))
								AS [GAP]							
					       	, act_value * R1.rate AS [Actual at Actual Parity]
					       
							FROM (
									SELECT id_factory, month, year, id_account_type, variability
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi		
						   			FROM ActRefFinalData
						   			GROUP BY id_factory, month, year, id_account_type, variability
								) A
			             	LEFT JOIN XFC_PLT_AUX_EffectsAnalysis E
			               		ON E.scenario = '{scenario}'
								AND E.ref_scenario = '{scenarioRef}'
								AND A.id_factory = E.id_factory
			               		AND A.month = MONTH(E.date)
								AND A.year = YEAR(E.date)
			         			AND A.variability = E.variability
			               		AND A.id_account_type = E.cost_type
							LEFT JOIN effectEnergyPrice ELE
								ON A.month = ELE.month
								AND ELE.energy_type = 'E'
								AND A.id_account_type = '3'
							LEFT JOIN effectEnergyPrice GAS
								ON A.month = GAS.month
								AND GAS.energy_type = 'G'							
								AND A.id_account_type = '3'
			             	LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON A.id_account_type = N.id   
							LEFT JOIN fxRate R1
								ON A.month = R1.month
							LEFT JOIN fxRateRef R2
								ON A.month = R2.month						

        					GROUP BY A.id_factory, A.month, id_account_type, description, R1.rate, R2.rate, A.variability, ref_value, ref_value_adj_100, ref_value_adj_semi, act_value	
						    
							-- ORDER BY id_factory, month, id_account_type, variability
							)	
						"
				#End Region	
				
				#Region "Variance Analysis"
				
				Else If query = "VarianceAnalysis"
																	
							Dim sql_ActivityIndex = AllQueries(si, "ActivityALL", factory, month, year, scenario, scenarioRef, "All")	
							Dim sql_Rate = AllQueries(si, "RATE", factory, month, year, scenario, scenarioRef, time, "", "", "", currency)	
							
							sql = sql_ActivityIndex & sql_Rate & $"
							
							, ActivityIndex As (
							
								SELECT
									month
									, [Id GM] AS id_averagegroup
									, Time
									, ISNULL(X.value, A.activity_index) AS activity_index
							
								FROM (
									SELECT 
										[Id Factory]
										, MONTH(A.date) AS month
										, [Id GM]
										, Time
										,CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])),0) <> 0 THEN
												CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END * 100 AS activity_index	
									
									FROM ActivityAll A								
								
					             	WHERE 1=1
								 	AND YEAR(A.date) = {year} 
							
									AND  (('{view}' = 'Periodic' AND MONTH(A.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(A.date) <= {month}))							
							
								 	AND [Id Factory] = '{factory}'							
					
									GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
								) A
							
								LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
									ON A.[Id GM] = X.id_averagegroup
									AND A.month = MONTH(X.date)
									AND A.[Id Factory] = X.id_factory
									AND A.Time = X.uotype
									AND X.scenario = '{scenario}'
									AND X.scenario_ref = '{scenarioRef}'
							)
							
							, PercVarAccount AS (
					       
								SELECT DISTINCT scenario, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								AND  (('{view}' = 'Periodic' AND MONTH(date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(date) <= {month}))
							
								AND YEAR(V.date) = {year} 						
								AND id_factory = '{factory}'
					       )					       					       
					       
					       , ActFact AS (
					       
								SELECT
									F.id_factory AS id_factory
								 	, MONTH(F.date) AS Month  
									, YEAR(F.date) AS Year   
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter
									, C.nature
								 	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = 'Actual'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id        
								
								WHERE 1=1
								AND F.scenario = '{scenario}'
								AND YEAR(F.date) = {year} 
								AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))	
								AND F.id_factory = '{factory}'
								AND CONVERT(INT,A.account_type) <> 0
					          	--AND C.type = 'A'
					       
								GROUP BY
									F.id_factory
								  	, MONTH(F.date)
									, YEAR(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description
									, C.nature
								  	, A.account_type       
					       )
					       
					       , RefFact AS (
					       
								SELECT
									F.id_factory AS id_factory
								  	, MONTH(F.date) AS Month   
									, YEAR(F.date) AS Year   
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter	
									, C.nature
								  	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = 'Actual'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id 
								
								WHERE 1=1
								AND F.scenario = '{scenarioRef}'
								AND YEAR(F.date) = {year} 
								AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))	
								AND F.id_factory = '{factory}'
								AND CONVERT(INT,A.account_type) <> 0
								--AND C.type = 'A'
								
								GROUP BY
									F.id_factory
									, MONTH(F.date)
									, YEAR(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description		
									, C.nature
									, A.account_type
					       
					       )
					       
					       , ActRefFinalData AS (
					       
					       SELECT 
					        	id_factory, month, year, account_type AS id_account_type, id_account, id_costcenter, costcenter, nature, variability
					         	, SUM(act_value) AS act_value
					        	, SUM(ref_value) AS ref_value
					        	, SUM(ref_value_adj_100) AS ref_value_adj_100
					        	, SUM(ref_value_adj_semi) AS ref_value_adj_semi
					       FROM (
					       
					       -- Actual FIXED
							
					       SELECT 
					          	F.id_factory, F.month, F.year, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					          	, 'F' AS variability
					          	, F.value * (100 - ISNULL(P.value,0)) / 100 AS act_value
					        	, 0 AS ref_value
					          	, 0 AS ref_value_adj_100
					          	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_account = P.id_account
					          	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Actual VARIABLE
							
					       SELECT 
					           	F.id_factory, F.month, F.year, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
								, 'V' AS variability
					          	, F.value * P.value / 100 AS value
					        	, 0 AS ref_value
					         	, 0 AS ref_value_adj_100
					         	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					            ON F.id_account = P.id_account
					           	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Reference FIXED
							
					       SELECT 
					           	F.id_factory, F.month, F.year, F.account_type, F.id_account, F.id_averagegroup	, F.id_costcenter, F.costcenter, F.nature
					           	, 'F' AS variability
					         	, 0 AS act_value
					        	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value
					         	, CASE WHEN A.id_averagegroup IS NOT NULL THEN
									F.value * (100 - ISNULL(P.value,0)) / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100							
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_semi						
					       
					       FROM RefFact F	
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup								
					       LEFT JOIN PercVarAccount P
								ON F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'							
					       
					       UNION ALL
					       
					       -- Reference VARIABLE
							
					       SELECT 
					            F.id_factory, F.month, F.year, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, F.value * P.value / 100 AS ref_value
					          	, CASE WHEN A.id_averagegroup IS NOT NULL THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100
					         	, CASE WHEN A.id_averagegroup IS NOT NULL THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_semi						
					       
					       FROM RefFact F
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup							
					       LEFT JOIN PercVarAccount P
					            ON F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'	
							
						   UNION ALL	
							
						   -- Reference FROM ACTUAL (Missing GM) - FIXED
							
					       SELECT 
					           	F.id_factory, F.month, F.year, F.account_type, F.id_account, F.id_averagegroup	, F.id_costcenter, F.costcenter, F.nature
					           	, 'F' AS variability
					         	, 0 AS act_value
					        	, 0 AS ref_value
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_100
					         	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM RefFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup
					       LEFT JOIN PercVarAccount P
					           	ON F.id_account = P.id_account
					          	AND P.scenario = '{scenario}'
								AND F.month = P.month						
						   WHERE A.id_averagegroup IS NULL		
							
						   -- Reference FROM ACTUAL (Missing GM) - VARIABLE
							
						   UNION ALL
							
					       SELECT 
					            F.id_factory, F.month, F.year, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, 0 AS ref_value
					          	, F.value * P.value / 100 AS ref_value_adj_100
					         	, F.value * P.value / 100 AS ref_value_adj_100
					       
					       FROM ActFact  F
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM RefFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup
					       LEFT JOIN PercVarAccount P
					           	ON F.id_account = P.id_account
					          	AND P.scenario = '{scenario}'
								AND F.month = P.month
						   WHERE A.id_averagegroup IS NULL									
					       
					       ) AS Res
					       
					       WHERE act_value <> 0 OR ref_value <> 0 OR ref_value_adj_100 <> 0
					       
					       GROUP BY 
					         id_factory, month, year, account_type, id_account, id_costcenter, costcenter, nature, variability
					       )
							
						-- Effects Price Energy
						, effectEnergyPrice as (
							SELECT
								MONTH(C.date) as Month
								, C.id_factory as Factory
								, C.energy_type
								, C.value AS Consumo
								, P1.value AS Price1
								, P2.value AS Price2
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN ISNULL(VE.value,0) / 100
									   WHEN C.energy_type = 'G' THEN ISNULL(VG.value,0) / 100 END
									AS effect_price_variable
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN (100 - ISNULL(VE.value,0)) / 100
									   WHEN C.energy_type = 'G' THEN (100 - ISNULL(VG.value,0)) / 100 END
									AS effect_price_fixed

							FROM ( 	SELECT * 
									FROM XFC_PLT_AUX_EnergyVariance         
									WHERE 1=1
									AND id_factory = '{factory}'
									AND scenario = '{scenario}'
									AND indicator = 'Consumption' 
									AND YEAR(date) = {year}
							) C        

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P1
								ON C.id_factory = P1.id_factory
								AND C.date = P1.date
								AND C.energy_type = P1.energy_type
								AND P1.indicator = 'Price'
								AND P1.scenario = '{scenario}'

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P2
								ON C.id_factory = P2.id_factory
								AND C.date = P2.date
								AND C.energy_type = P2.energy_type
								AND P2.indicator = 'Price'
								AND P2.scenario = '{scenarioref}' 

							LEFT JOIN (SELECT 'E' AS energy_type, value, scenario, month FROM PercVarAccount WHERE id_account = '3D32') VE
								ON C.energy_type = VE.energy_type
								AND MONTH(C.date) = VE.month
								AND VE.scenario =  '{scenarioRef}' 

							LEFT JOIN (SELECT 'G' AS energy_type, value, scenario, month FROM PercVarAccount WHERE id_account = '3D34') VG
								ON C.energy_type = VG.energy_type
								AND MONTH(C.date) = VG.month
								AND VG.scenario =  '{scenarioRef}' 																
						)					       
							
						, VarianceAnalysis as (
					       SELECT DISTINCT
					       	A.id_factory
					       	, A.month
					       	, A.id_account_type
							, N.description AS desc_account_type
					       	, A.variability
							, CASE WHEN ref_value <> 0 THEN ref_value_adj_100 / ref_value * 100 END AS [Activity Index]
					       	, ref_value * R2.rate AS [Reference]
					       	, ref_value_adj_100 * R2.rate AS [Ref. 100% Adjusted]
					       	, (ref_value_adj_semi - ref_value_adj_100) * R2.rate AS [Absorption of Fixed Costs]
					       	, ref_value_adj_semi * R2.rate AS [Ref. Semi Adjusted]
					       	, SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate AS [Scope Change]
					       	, (ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0) AS [Ref. Adjusted]
					       	, ((ref_value_adj_semi * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))
								- ((ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)) AS [Impact Parity]
					       	, (ref_value_adj_semi * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0) AS [Ref. Adjusted at Actual Parity]
					       	, ISNULL(SUM(CASE WHEN id_indicator IN ('AGS','MIX','ORG','NOR','SAL', 'DIV') THEN E.value / 1000 END) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all,0) AS [Price/Salary Trend]
					       	, SUM(CASE WHEN id_indicator = 'AGS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend AGS effect]
					       	, SUM(CASE WHEN id_indicator = 'MIX' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Mix population Effect]
					       	, SUM(CASE WHEN id_indicator = 'ORG' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Organization Effect]
					       	, SUM(CASE WHEN id_indicator = 'NOR' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Noria Effect]
					       	, SUM(CASE WHEN id_indicator = 'SAL' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Wages other]
					       	, AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all AS [Price/Salary Trend Electricity Effect]
					       	, AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all AS [Price/Salary Trend Gaz Effect]
					       	, SUM(CASE WHEN id_indicator = 'CTS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Price variance other than Elec/Gaz]
					       	, 0 AS [MEC]
					       	, SUM(CASE WHEN id_indicator = 'PPR' THEN E.value / 1000 END) * R1.rate AS [Product/Process Evolution]
					       	, SUM(CASE WHEN id_indicator = 'ELB?' THEN E.value / 1000 END) * R1.rate AS [Excess Labour]
					       	, SUM(CASE WHEN id_indicator = 'UNE?' THEN E.value / 1000 END) * R1.rate AS [Unemployement]
					       	, SUM(CASE WHEN id_indicator IN ('POR','PHC','COV','DIV') THEN E.value / 1000 END) * R1.rate AS [Productivity]
					       	, SUM(CASE WHEN id_indicator = 'POR' THEN E.value / 1000 END) * R1.rate AS [Organization Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'PHC' THEN E.value / 1000 END) * R1.rate AS [Hyper Competitiveness Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'COV' THEN E.value / 1000 END) * R1.rate AS [COVID 19 Effect]
					       	, SUM(CASE WHEN id_indicator = 'DIV' THEN E.value / 1000 END) * R1.rate AS [Extra Productivity Effect]
					       	, (act_value * R1.rate) - 
								((ref_value_adj_semi * R1.rate) 
								+ ISNULL(SUM(E.value / 1000) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all,0))
								AS [GAP]							
					       	, act_value * R1.rate AS [Actual at Actual Parity]
					       
							FROM (
									SELECT id_factory, month, year, id_account_type, variability
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi		
						   			FROM ActRefFinalData
						   			GROUP BY id_factory, month, year, id_account_type, variability
								) A
			             	LEFT JOIN XFC_PLT_AUX_EffectsAnalysis E
			               		ON E.scenario = '{scenario}'
								AND E.ref_scenario = '{scenarioRef}'
								AND A.id_factory = E.id_factory
			               		AND A.month = MONTH(E.date)
								AND A.year = YEAR(E.date)
			         			AND A.variability = E.variability
			               		AND A.id_account_type = E.cost_type
							LEFT JOIN effectEnergyPrice ELE
								ON A.month = ELE.month
								AND ELE.energy_type = 'E'
								AND A.id_account_type = '3'
							LEFT JOIN effectEnergyPrice GAS
								ON A.month = GAS.month
								AND GAS.energy_type = 'G'							
								AND A.id_account_type = '3'
			             	LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON A.id_account_type = N.id   
							LEFT JOIN fxRate R1
								ON A.month = R1.month
							LEFT JOIN fxRateRef R2
								ON A.month = R2.month						

        					GROUP BY A.id_factory, A.month, id_account_type, description, R1.rate, R2.rate, R1.rate_exception, R1.rate_all , A.variability, ref_value, ref_value_adj_100, ref_value_adj_semi, act_value	
						    
							-- ORDER BY id_factory, month, id_account_type, variability
							)	
						"
				#End Region					
				
				#Region "Variance Analysis (All Factories) OLD 19/05/2025"
				
				Else If query = "VarianceAnalysis_All_FactoriesOLD"
																	
							Dim sql_ActivityIndex = AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioRef, "All")	
							Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", month, year, scenario, scenarioRef, time, "", "", "", currency)	
							
							month = Right("0" & month,2)
							Dim fechaDesde As String = year & "-01-01"
							Dim fechaHasta As String = year & "-" & month & "-01"
							
							sql = sql_ActivityIndex & sql_Rate & $"		
							
					       , ActivityIndex AS (
					
					        SELECT A.id_factory, month, A.id_averagegroup, time, ISNULL(X.value, activity_index ) AS activity_index
					        FROM (
					
						        SELECT 
									[Id Factory] AS id_factory
						         	, MONTH(A.date) AS month
						         	, [Id GM] AS id_averagegroup
						         	, Time
						         	, CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])),0) <> 0 THEN
						           		CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END * 100 AS activity_index 
						        
						        FROM ActivityAll A      
						       
				             	WHERE YEAR(A.date) = {year} 
								AND MONTH(A.date) <= {month}     					       
					    
					       	 	GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
					
								) AS A
					
						    LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
					         	ON A.id_averagegroup = X.id_averagegroup
					         	AND A.month = MONTH(X.date)
					         	AND A.id_factory = X.id_factory
					         	AND A.Time = X.uotype
								AND X.scenario = '{scenario}'
								AND X.scenario_ref = '{scenarioRef}'
					       )       							
							
					       , PercVarAccount AS (
					       
								SELECT DISTINCT id_factory, scenario, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								
								WHERE YEAR(V.date) = {year} 
								AND MONTH(date) <= {month}
														
					       )					       					       
					       
					       , ActFact AS (
					       
								SELECT
									F.id_factory AS id_factory
								 	, MONTH(F.date) AS Month   
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter
									, C.nature
								 	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = '{scenario}'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id        
								
								WHERE 1=1
								AND F.scenario = '{scenario}'
							
								AND F.date BETWEEN '{fechaDesde}' AND '{fechaHasta}'	
							
								AND CONVERT(INT,A.account_type) <> 0

					       
								GROUP BY
									F.id_factory
								  	, MONTH(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description
									, C.nature
								  	, A.account_type       
					       )
					       
					       , RefFact AS (
					       
								SELECT
									F.id_factory AS id_factory
								  	, MONTH(F.date) AS Month    
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter	
									, C.nature
								  	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = '{scenario}'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id 
								
								WHERE 1=1
								AND F.scenario = '{scenarioRef}'							
							
								AND  F.date BETWEEN '{fechaDesde}' AND '{fechaHasta}'
							
								AND CONVERT(INT,A.account_type) <> 0
								
								GROUP BY
									F.id_factory
									, MONTH(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description		
									, C.nature
									, A.account_type
					       
					       )
					       
					       , ActRefFinalData AS (
					       
					       SELECT 
					        	id_factory
					        	, month
					        	, account_type AS id_account_type
								, id_account
								, id_costcenter
								, costcenter
								, nature
					        	, variability
					         	, SUM(act_value) AS act_value
					        	, SUM(ref_value) AS ref_value
					        	, SUM(ref_value_adj_100) AS ref_value_adj_100
					        	, SUM(ref_value_adj_semi) AS ref_value_adj_semi
					       FROM (
					       
					       -- Actual FIXED
							
					       SELECT 
					          	F.id_factory
					          	, F.month
					            , F.account_type
								, F.id_account
								, F.id_averagegroup
								, F.id_costcenter
								, F.costcenter
								, F.nature
					          	, 'F' AS variability
					          	, F.value * (100 - ISNULL(P.value,0)) / 100 AS act_value
					        	, 0 AS ref_value
					          	, 0 AS ref_value_adj_100
					          	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					          	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Actual VARIABLE
							
					       SELECT 
					           	F.id_factory
					           	, F.month
					        	, F.account_type
								, F.id_account
								, F.id_averagegroup		
								, F.id_costcenter
								, F.costcenter		
								, F.nature
					        	, 'V' AS variability
					          	, F.value * P.value / 100 AS value
					        	, 0 AS ref_value
					         	, 0 AS ref_value_adj_100
					         	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Reference FIXED
							
					       SELECT 
					           	F.id_factory
					           	, F.month
					          	, F.account_type
								, F.id_account
								, F.id_averagegroup	
								, F.id_costcenter
								, F.costcenter			
								, F.nature
					           	, 'F' AS variability
					         	, 0 AS act_value
					        	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value
					         	, F.value * (100 - ISNULL(P.value,0)) / 100
									* CASE WHEN F.account_type = 1 THEN ISNULL(UO1.activity_index,100) 
									  	   WHEN F.account_type IN (2,3,4,5) THEN ISNULL(UO1.activity_index,100) 
									  	   ELSE 100 END / 100 AS ref_value_adj_100
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_semi
					       
					       FROM RefFact  F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'
						   LEFT JOIN ActivityIndex UO2
								ON F.id_averagegroup = UO2.id_averagegroup 	
								AND F.month = UO2.month
								AND UO2.time = 'UO2'							
					       
					       UNION ALL
					       
					       -- Reference VARIABLE
							
					       SELECT 
					            F.id_factory
					            , F.month
					         	, F.account_type
								, F.id_account
								, F.id_averagegroup		
								, F.id_costcenter
								, F.costcenter		
								, F.nature
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, F.value * P.value / 100 AS ref_value
					          	, F.value * P.value / 100
									* CASE WHEN F.account_type = 1 THEN ISNULL(UO1.activity_index,100) 
									  	   WHEN F.account_type IN (2,3,4,5) THEN ISNULL(UO1.activity_index,100)
									  	   ELSE 1 END / 100	AS ref_value_adj_100
					         	, F.value * P.value / 100
									* CASE WHEN F.account_type = 1 THEN ISNULL(UO1.activity_index,100) 
									  	   WHEN F.account_type IN (2,3,4,5) THEN ISNULL(UO1.activity_index,100) 
									  	   ELSE 1 END / 100 AS ref_value_adj_semi
					       
					       FROM RefFact F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'
						   LEFT JOIN ActivityIndex UO2
								ON F.id_averagegroup = UO2.id_averagegroup 		
								AND F.month = UO2.month
								AND UO2.time = 'UO2'			
					       
					       ) AS Res
					       
					       WHERE act_value <> 0 OR ref_value  <> 0
					       
					       GROUP BY 
					         id_factory
					         , month
					         , account_type
							 , id_account
							 , id_costcenter
							 , costcenter	
							 , nature
					         , variability
					       )
							
						-- Effects Price Energy
						, effectEnergyPrice as (
							SELECT
								MONTH(C.date) as Month
								, C.id_factory as id_factory
								, C.energy_type
								, C.value AS Consumo
								, P1.value AS Price1
								, P2.value AS Price2
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN ISNULL(VE.value,0) / 100
									   WHEN C.energy_type = 'G' THEN ISNULL(VG.value,0) / 100 END
									AS effect_price_variable
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN (100 - ISNULL(VE.value,0)) / 100
									   WHEN C.energy_type = 'G' THEN (100 - ISNULL(VG.value,0)) / 100 END
									AS effect_price_fixed

							FROM ( 	SELECT * 
									FROM XFC_PLT_AUX_EnergyVariance         
									WHERE 1=1
									AND scenario = '{scenario}'
									AND indicator = 'Consumption'
									AND YEAR(date) = {year}
							
							) C        

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P1
								ON C.id_factory = P1.id_factory
								AND C.date = P1.date
								AND C.energy_type = P1.energy_type
								AND P1.indicator = 'Price'
								AND P1.scenario = '{scenario}'

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P2
								ON C.id_factory = P2.id_factory
								AND C.date = P2.date
								AND C.energy_type = P2.energy_type
								AND P2.indicator = 'Price'
								AND P2.scenario = '{scenarioref}' 

							LEFT JOIN (SELECT 'E' AS energy_type, value, id_factory, scenario, month FROM PercVarAccount WHERE id_account = '3D32') VE
								ON C.id_factory = VE.id_factory
								AND C.energy_type = VE.energy_type
								AND MONTH(C.date) = VE.month
								AND VE.scenario =  '{scenarioRef}' 

							LEFT JOIN (SELECT 'G' AS energy_type, value, id_factory, scenario, month FROM PercVarAccount WHERE id_account = '3D34') VG
								ON C.id_factory = VG.id_factory
								AND C.energy_type = VG.energy_type
								AND MONTH(C.date) = VG.month
								AND VG.scenario =  '{scenarioRef}' 																
						)					       
							
						, VarianceAnalysis as (
					       SELECT DISTINCT
					       	A.id_factory
					       	, A.month
					       	, A.id_account_type
							, N.description AS desc_account_type
					       	, A.variability
							, CASE WHEN ref_value <> 0 THEN ref_value_adj_100 / ref_value * 100 END AS [Activity Index]
					       	, ref_value * R2.rate AS [Reference]
					       	, ref_value_adj_100 * R2.rate AS [Ref. 100% Adjusted]
					       	, (ref_value_adj_semi - ref_value_adj_100) * R2.rate AS [Absorption of Fixed Costs]
					       	, ref_value_adj_semi * R2.rate AS [Ref. Semi Adjusted]
					       	, SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate AS [Scope Change]
					       	, (ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0) AS [Ref. Adjusted]
					       	, ((ref_value_adj_semi * R1.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0))
							 - ((ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)) AS [Impact Parity]
					       	, (ref_value_adj_semi * R1.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0) AS [Ref. Adjusted at Actual Parity]
					       	, ISNULL(SUM(CASE WHEN id_indicator IN ('AGS','MIX','ORG','NOR','SAL', 'DIV') THEN E.value / 1000 END) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate,0) AS [Price/Salary Trend]
					       	, SUM(CASE WHEN id_indicator = 'AGS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend AGS effect]
					       	, SUM(CASE WHEN id_indicator = 'MIX' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Mix population Effect]
					       	, SUM(CASE WHEN id_indicator = 'ORG' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Organization Effect]
					       	, SUM(CASE WHEN id_indicator = 'NOR' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Noria Effect]
					       	, SUM(CASE WHEN id_indicator = 'SAL' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Wages other]
					       	, AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate AS [Price/Salary Trend Electricity Effect]
					       	, AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate AS [Price/Salary Trend Gaz Effect]
					       	, SUM(CASE WHEN id_indicator = 'CTS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Price variance other than Elec/Gaz]
					       	, 0 AS [MEC]
					       	, SUM(CASE WHEN id_indicator = 'PPR' THEN E.value / 1000 END) * R1.rate AS [Product/Process Evolution]
					       	, SUM(CASE WHEN id_indicator = 'ELB?' THEN E.value / 1000 END) * R1.rate AS [Excess Labour]
					       	, SUM(CASE WHEN id_indicator = 'UNE?' THEN E.value / 1000 END) * R1.rate AS [Unemployement]
					       	, SUM(CASE WHEN id_indicator IN ('POR','PHC','COV','DIV') THEN E.value / 1000 END) * R1.rate AS [Productivity]
					       	, SUM(CASE WHEN id_indicator = 'POR' THEN E.value / 1000 END) * R1.rate AS [Organization Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'PHC' THEN E.value / 1000 END) * R1.rate AS [Hyper Competitiveness Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'COV' THEN E.value / 1000 END) * R1.rate AS [COVID 19 Effect]
					       	, SUM(CASE WHEN id_indicator = 'DIV' THEN E.value / 1000 END) * R1.rate AS [Extra Productivity Effect]
					       	, (act_value * R1.rate) - 
								((ref_value_adj_semi * R1.rate) 
								+ ISNULL(SUM(E.value / 1000) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate,0))
								AS [GAP]
					       	, act_value * R1.rate AS [Actual at Actual Parity]
					       
							FROM (
									SELECT id_factory, month, id_account_type, variability
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi		
						   			FROM ActRefFinalData
						   			GROUP BY id_factory, month, id_account_type, variability
								) A
			             	LEFT JOIN XFC_PLT_AUX_EffectsAnalysis E
			               		ON E.scenario = '{scenario}'
								AND E.ref_scenario = '{scenarioRef}'
								AND A.id_factory = E.id_factory
			               		AND A.month = MONTH(E.date)
								AND YEAR(E.date) = {year}
			         			AND A.variability = E.variability
			               		AND A.id_account_type = E.cost_type
							LEFT JOIN effectEnergyPrice ELE
								ON A.id_factory = ELE.id_factory
								AND A.month = ELE.month
								AND ELE.energy_type = 'E'
								AND A.id_account_type = '3'
							LEFT JOIN effectEnergyPrice GAS
								ON A.id_factory = GAS.id_factory
								AND A.month = GAS.month
								AND GAS.energy_type = 'G'							
								AND A.id_account_type = '3'
			             	LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON A.id_account_type = N.id   
							LEFT JOIN fxRate R1
								ON A.id_factory = R1.id_factory
								AND A.month = R1.month
							LEFT JOIN fxRateRef R2
								ON A.id_factory = R2.id_factory
								AND A.month = R2.month						

        					GROUP BY A.id_factory, A.month, id_account_type, description, R1.rate, R2.rate, A.variability, ref_value, ref_value_adj_100, ref_value_adj_semi, act_value	
						    
							-- ORDER BY id_factory, month, id_account_type, variability
							)	
						"
							
				#End Region		
				
				#Region "Variance Analysis (All Factories)"
				
				Else If query = "VarianceAnalysis_All_Factories"
																	
							Dim sql_ActivityIndex = AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioRef, "All")	
							Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", month, year, scenario, scenarioRef, time, "", "", "", currency)	
							
							month = Right("0" & month,2)
							Dim fechaDesde As String = year & "-01-01"
							Dim fechaHasta As String = year & "-" & month & "-01"
							
							sql = sql_ActivityIndex & sql_Rate & $"		
							
					       , ActivityIndex AS (
					
					        SELECT A.id_factory, month, A.id_averagegroup, time, ISNULL(X.value, activity_index ) AS activity_index
					        FROM (
					
						        SELECT 
									[Id Factory] AS id_factory
						         	, MONTH(A.date) AS month
						         	, [Id GM] AS id_averagegroup
						         	, Time
						         	, CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])),0) <> 0 THEN
						           		CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END * 100 AS activity_index 
						        
						        FROM ActivityAll A      
						       
				             	WHERE YEAR(A.date) = {year} 
								AND MONTH(A.date) <= {month}     					       
					    
					       	 	GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
					
								) AS A
					
						    LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
					         	ON A.id_averagegroup = X.id_averagegroup
					         	AND A.month = MONTH(X.date)
					         	AND A.id_factory = X.id_factory
					         	AND A.Time = X.uotype
								AND X.scenario = '{scenario}'
								AND X.scenario_ref = '{scenarioRef}'
					       )       							
							
					       , PercVarAccount AS (
					       
								SELECT DISTINCT id_factory, scenario, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								
								WHERE YEAR(V.date) = {year} 
								AND MONTH(date) <= {month}
														
					       )					       					       
					       
					       , ActFact AS (
					       
								SELECT
									F.id_factory AS id_factory
									, YEAR(F.date) AS Year 
								 	, MONTH(F.date) AS Month   
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter
									, C.nature
								 	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = 'Actual'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id        
								
								WHERE 1=1
								AND F.scenario = '{scenario}'
							
								AND F.date BETWEEN '{fechaDesde}' AND '{fechaHasta}'	
							
								AND CONVERT(INT,A.account_type) <> 0

					       
								GROUP BY
									F.id_factory
									, YEAR(F.date)
								  	, MONTH(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description
									, C.nature
								  	, A.account_type       
					       )
					       
					       , RefFact AS (
					       
								SELECT
									F.id_factory AS id_factory
									, YEAR(F.date) AS Year 
								  	, MONTH(F.date) AS Month    
									, F.id_account AS id_account
									, F.id_averagegroup AS id_averagegroup
									, F.id_costcenter
									, C.description AS costcenter	
									, C.nature
								  	, CONVERT(INT,A.account_type) AS account_type
									, SUM(F.value) / 1000 AS Value 
								     
								FROM XFC_PLT_FACT_CostsDistribution F
								LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
								 	ON F.id_costcenter = C.id
									AND C.scenario = 'Actual'
									AND F.date BETWEEN C.start_date AND C.end_date
								LEFT JOIN XFC_PLT_MASTER_Account A
								 	ON F.id_account = A.id 
								
								WHERE 1=1
								AND F.scenario = '{scenarioRef}'							
							
								AND  F.date BETWEEN '{fechaDesde}' AND '{fechaHasta}'
							
								AND CONVERT(INT,A.account_type) <> 0
								
								GROUP BY
									F.id_factory
									, YEAR(F.date)
									, MONTH(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description		
									, C.nature
									, A.account_type
					       
					       )
					       
					       , ActRefFinalData AS (
					       
					       SELECT 
					        	id_factory, year, month, account_type AS id_account_type, id_account, id_costcenter, costcenter, nature, variability
					         	, SUM(act_value) AS act_value
					        	, SUM(ref_value) AS ref_value
					        	, SUM(ref_value_adj_100) AS ref_value_adj_100
					        	, SUM(ref_value_adj_semi) AS ref_value_adj_semi
					       FROM (
					       
					       -- Actual FIXED
							
					       SELECT 
					          	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					          	, 'F' AS variability
					          	, F.value * (100 - ISNULL(P.value,0)) / 100 AS act_value
					        	, 0 AS ref_value
					          	, 0 AS ref_value_adj_100
					          	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					          	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Actual VARIABLE
							
					       SELECT 
					           	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					        	, 'V' AS variability
					          	, F.value * P.value / 100 AS value
					        	, 0 AS ref_value
					         	, 0 AS ref_value_adj_100
					         	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenario}'
								AND F.month = P.month
					       
					       UNION ALL
					       
					       -- Reference FIXED
							
					       SELECT 
					           	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					           	, 'F' AS variability
					         	, 0 AS act_value
					        	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value
					         	, CASE WHEN A.id_averagegroup IS NOT NULL THEN
									F.value * (100 - ISNULL(P.value,0)) / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100							
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_semi
					       
					       FROM RefFact  F
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup							
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'						
					       
					       UNION ALL
					       
					       -- Reference VARIABLE
							
					       SELECT 
					            F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, F.value * P.value / 100 AS ref_value	
					          	, CASE WHEN A.id_averagegroup IS NOT NULL THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100
					         	, CASE WHEN A.id_averagegroup IS NOT NULL THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_semi								
					       
					       FROM RefFact F
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup							
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenarioRef}'
								AND F.month = P.month
						   LEFT JOIN ActivityIndex UO1
								ON F.id_averagegroup = UO1.id_averagegroup 		
								AND F.month = UO1.month
								AND UO1.time = 'UO1'
							
						   UNION ALL	
							
						   -- Reference FROM ACTUAL (Missing GM) - FIXED
							
					       SELECT 
					           	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup	, F.id_costcenter, F.costcenter, F.nature
					           	, 'F' AS variability
					         	, 0 AS act_value
					        	, 0 AS ref_value
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_100
					         	, 0 AS ref_value_adj_semi
					       
					       FROM ActFact  F
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM RefFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenario}'
								AND F.month = P.month					
						   WHERE A.id_averagegroup IS NULL		
							
						   -- Reference FROM ACTUAL (Missing GM) - VARIABLE
							
						   UNION ALL
							
					       SELECT 
					            F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, 0 AS ref_value
					          	, F.value * P.value / 100 AS ref_value_adj_100
					         	, F.value * P.value / 100 AS ref_value_adj_100
					       
					       FROM ActFact  F
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM RefFact) A
								ON F.id_factory = A.id_factory
								AND F.month = A.month
								AND F.id_averagegroup = A.id_averagegroup
					       LEFT JOIN PercVarAccount P
					           	ON F.id_factory = P.id_factory
								AND F.id_account = P.id_account
					           	AND P.scenario = '{scenario}'
								AND F.month = P.month	
						   WHERE A.id_averagegroup IS NULL									
					       
					       ) AS Res
					       
					       WHERE act_value <> 0 OR ref_value <> 0 OR ref_value_adj_100 <> 0
					       
					       GROUP BY 
					         id_factory
							 , year
					         , month
					         , account_type
							 , id_account
							 , id_costcenter
							 , costcenter	
							 , nature
					         , variability
					       )
							
						-- Effects Price Energy
						, effectEnergyPrice as (
							SELECT
								MONTH(C.date) as Month
								, C.id_factory as id_factory
								, C.energy_type
								, C.value AS Consumo
								, P1.value AS Price1
								, P2.value AS Price2
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN ISNULL(VE.value,0) / 100
									   WHEN C.energy_type = 'G' THEN ISNULL(VG.value,0) / 100 END
									AS effect_price_variable
								, C.value * (P1.value - P2.value)
								* CASE WHEN C.energy_type = 'E' THEN (100 - ISNULL(VE.value,0)) / 100
									   WHEN C.energy_type = 'G' THEN (100 - ISNULL(VG.value,0)) / 100 END
									AS effect_price_fixed

							FROM ( 	SELECT * 
									FROM XFC_PLT_AUX_EnergyVariance         
									WHERE 1=1
									AND scenario = '{scenario}'
									AND indicator = 'Consumption'
									AND YEAR(date) = {year}
							
							) C        

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P1
								ON C.id_factory = P1.id_factory
								AND C.date = P1.date
								AND C.energy_type = P1.energy_type
								AND P1.indicator = 'Price'
								AND P1.scenario = '{scenario}'

							LEFT JOIN XFC_PLT_AUX_EnergyVariance P2
								ON C.id_factory = P2.id_factory
								AND C.date = P2.date
								AND C.energy_type = P2.energy_type
								AND P2.indicator = 'Price'
								AND P2.scenario = '{scenarioref}' 

							LEFT JOIN (SELECT 'E' AS energy_type, value, id_factory, scenario, month FROM PercVarAccount WHERE id_account = '3D32') VE
								ON C.id_factory = VE.id_factory
								AND C.energy_type = VE.energy_type
								AND MONTH(C.date) = VE.month
								AND VE.scenario =  '{scenarioRef}' 

							LEFT JOIN (SELECT 'G' AS energy_type, value, id_factory, scenario, month FROM PercVarAccount WHERE id_account = '3D34') VG
								ON C.id_factory = VG.id_factory
								AND C.energy_type = VG.energy_type
								AND MONTH(C.date) = VG.month
								AND VG.scenario =  '{scenarioRef}' 																
						)					       
							
						, VarianceAnalysis as (
					       SELECT DISTINCT
					       	A.id_factory
					       	, A.month
					       	, A.id_account_type
							, N.description AS desc_account_type
					       	, A.variability
							, CASE WHEN ref_value <> 0 THEN ref_value_adj_100 / ref_value * 100 END AS [Activity Index]
					       	, ref_value * R2.rate AS [Reference]
					       	, ref_value_adj_100 * R2.rate AS [Ref. 100% Adjusted]
					       	, (ref_value_adj_semi - ref_value_adj_100) * R2.rate AS [Absorption of Fixed Costs]
					       	, ref_value_adj_semi * R2.rate AS [Ref. Semi Adjusted]
					       	, SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate AS [Scope Change]
					       	, (ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0) AS [Ref. Adjusted]
					       	, ((ref_value_adj_semi * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))
								- ((ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)) AS [Impact Parity]
					       	, (ref_value_adj_semi * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0) AS [Ref. Adjusted at Actual Parity]
					       	, ISNULL(SUM(CASE WHEN id_indicator IN ('AGS','MIX','ORG','NOR','SAL', 'DIV') THEN E.value / 1000 END) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all,0) AS [Price/Salary Trend]
					       	, SUM(CASE WHEN id_indicator = 'AGS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend AGS effect]
					       	, SUM(CASE WHEN id_indicator = 'MIX' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Mix population Effect]
					       	, SUM(CASE WHEN id_indicator = 'ORG' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Organization Effect]
					       	, SUM(CASE WHEN id_indicator = 'NOR' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Noria Effect]
					       	, SUM(CASE WHEN id_indicator = 'SAL' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Wages other]
					       	, AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all AS [Price/Salary Trend Electricity Effect]
					       	, AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all AS [Price/Salary Trend Gaz Effect]
					       	, SUM(CASE WHEN id_indicator = 'CTS' THEN E.value / 1000 END) * R1.rate AS [Price/Salary Trend Price variance other than Elec/Gaz]
					       	, 0 AS [MEC]
					       	, SUM(CASE WHEN id_indicator = 'PPR' THEN E.value / 1000 END) * R1.rate AS [Product/Process Evolution]
					       	, SUM(CASE WHEN id_indicator = 'ELB?' THEN E.value / 1000 END) * R1.rate AS [Excess Labour]
					       	, SUM(CASE WHEN id_indicator = 'UNE?' THEN E.value / 1000 END) * R1.rate AS [Unemployement]
					       	, SUM(CASE WHEN id_indicator IN ('POR','PHC','COV','DIV') THEN E.value / 1000 END) * R1.rate AS [Productivity]
					       	, SUM(CASE WHEN id_indicator = 'POR' THEN E.value / 1000 END) * R1.rate AS [Organization Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'PHC' THEN E.value / 1000 END) * R1.rate AS [Hyper Competitiveness Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'COV' THEN E.value / 1000 END) * R1.rate AS [COVID 19 Effect]
					       	, SUM(CASE WHEN id_indicator = 'DIV' THEN E.value / 1000 END) * R1.rate AS [Extra Productivity Effect]
					       	, (act_value * R1.rate) - 
								((ref_value_adj_semi * R1.rate) 
								+ ISNULL(SUM(E.value / 1000) * R1.rate,0)
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all,0))
								AS [GAP]
					       	, act_value * R1.rate AS [Actual at Actual Parity]
					       
							FROM (
									SELECT id_factory, month, id_account_type, variability
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi		
						   			FROM ActRefFinalData
						   			GROUP BY id_factory, month, id_account_type, variability
								) A
			             	LEFT JOIN XFC_PLT_AUX_EffectsAnalysis E
			               		ON E.scenario = '{scenario}'
								AND E.ref_scenario = '{scenarioRef}'
								AND A.id_factory = E.id_factory
			               		AND A.month = MONTH(E.date)
								AND YEAR(E.date) = {year}
			         			AND A.variability = E.variability
			               		AND A.id_account_type = E.cost_type
							LEFT JOIN effectEnergyPrice ELE
								ON A.id_factory = ELE.id_factory
								AND A.month = ELE.month
								AND ELE.energy_type = 'E'
								AND A.id_account_type = '3'
							LEFT JOIN effectEnergyPrice GAS
								ON A.id_factory = GAS.id_factory
								AND A.month = GAS.month
								AND GAS.energy_type = 'G'							
								AND A.id_account_type = '3'
			             	LEFT JOIN XFC_PLT_MASTER_NatureCost N
			         			ON A.id_account_type = N.id   
							LEFT JOIN fxRate R1
								ON A.id_factory = R1.id_factory
								AND A.month = R1.month
							LEFT JOIN fxRateRef R2
								ON A.id_factory = R2.id_factory
								AND A.month = R2.month						

        					GROUP BY A.id_factory, A.month, id_account_type, description, R1.rate, R2.rate, R1.rate_exception, R1.rate_all, A.variability, ref_value, ref_value_adj_100, ref_value_adj_semi, act_value	
						    
							-- ORDER BY id_factory, month, id_account_type, variability
							)	
						"
							
				#End Region						
				
				#Region "Energy Variance"
				
				Else If query =  "EnergyVariance"
				
				' Necesita la consulta de ActivityIndex
				' Falta añadir la cuenta de Gas
				' Variables: scenario, scenarioRef, factory, year
					Dim sqlActivityIndex As String = AllQueries(si, "ActivityALL", factory, month, year, scenario, scenarioRef, "All")
					Dim sql_Rate = AllQueries(si, "RATE", factory, month, year, scenario, scenarioRef, time, "", "", "", currency)	
					
					sql = sqlActivityIndex & sql_Rate & $"
						
						, pricesActual as (
							SELECT
								MONTH(date) as Month
								, id_factory as Factory
								, CASE 
									WHEN energy_type = 'E' Then 'Electricity'
									WHEN energy_type = 'G' Then 'Gas'
									ELSE 'TBD'
								END AS Energy
								, Value as ACT_Price
						
							FROM XFC_PLT_AUX_EnergyVariance
						
							WHERE 1=1
								AND scenario = '{scenario}'
								AND id_factory = '{factory}'
								AND indicator = 'Price'
								AND YEAR(date) = {year}
						),
						
						consumptionActual as (
							SELECT
								MONTH(date) as Month
								, id_factory as Factory
								, CASE 
									WHEN energy_type = 'E' Then 'Electricity'
									WHEN energy_type = 'G' Then 'Gas'
									ELSE 'TBD'
								END AS Energy
								, Value as ACT_Consumption
						
							FROM XFC_PLT_AUX_EnergyVariance
						
							WHERE 1=1
								AND scenario = '{scenario}'
								AND id_factory = '{factory}'
								AND indicator = 'Consumption'
								AND YEAR(date) = {year}
						),
						
						pricesReference as (
							SELECT
								MONTH(date) as Month
								, id_factory as Factory
								, CASE 
									WHEN energy_type = 'E' Then 'Electricity'
									WHEN energy_type = 'G' Then 'Gas'
									ELSE 'TBD'
								END AS Energy
								, Value as REF_Price
						
							FROM XFC_PLT_AUX_EnergyVariance
						
							WHERE 1=1
								AND scenario = '{scenarioRef}'
								AND id_factory = '{factory}'
								AND indicator = 'Price'
								AND YEAR(date) = {year}
						),
						
						consumptionReference as (
							SELECT
								MONTH(date) as Month
								, id_factory as Factory
								, CASE 
									WHEN energy_type = 'E' Then 'Electricity'
									WHEN energy_type = 'G' Then 'Gas'
									ELSE 'TBD'
								END AS Energy
								, Value as REF_Consumption
						
							FROM XFC_PLT_AUX_EnergyVariance
						
							WHERE 1=1
								AND scenario = '{scenarioRef}'
								AND id_factory = '{factory}'
								AND indicator = 'Consumption'
								AND YEAR(date) = {year}
						),
											
						preCalculate as (	
						
							SELECT 
								COALESCE(A.Month, C.Month) as Month
								, COALESCE(A.Factory, C.Factory) as Factory
								, COALESCE(A.Energy, C.Energy) as Energy
								, C.REF_Price
								, D.REF_Consumption						
								, A.ACT_Price
								, B.ACT_Consumption

						
							FROM pricesActual A
						
							LEFT JOIN consumptionActual B
								ON A.Month = B.Month
								AND A.Energy = B.Energy
								AND A.Factory = B.Factory
						
							FULL JOIN pricesReference C
								ON A.Month = C.Month
								AND A.Energy = C.Energy
								AND A.Factory = C.Factory
			
							LEFT JOIN consumptionReference D
								ON C.Month = D.Month
								AND C.Energy = D.Energy
								AND C.Factory = D.Factory	
						),
						
						ActivityIndex as (
							SELECT 
								MONTH(date) AS month
								, [Id GM] AS id_averagegroup
								, Time AS uotype
								, CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) AS activity_index	
							
							FROM ActivityAll A
					
							WHERE 1=1
								AND YEAR(date) = {year} 
								AND ('{view}' = 'Periodic') OR ('{view}' = 'YTD')
								-- AND  (('{view}' = 'Periodic' AND MONTH(date) = {month}) OR ('{view}' = 'YTD' AND MONTH(date) <= {month}))							
								AND [Id Factory] = '{factory}'							
							
							GROUP BY MONTH(date), [Id GM], Time							
						),
					
						activityIndex_1 as (
							SELECT
								A.id_factory
								, YEAR(A.date) as year
								, MONTH(A.date) as month
								, A.id_account
								, A.id_averagegroup
								, C.activity_index
								, D.value/100 as variability
								, SUM(A.value) as CosteTotal_GM
								, C.uotype as time
							
							FROM XFC_PLT_FACT_CostsDistribution A
								
							LEFT JOIN ActivityIndex C
								ON A.id_averagegroup = C.id_averagegroup
								AND MONTH(A.date) = C.Month
							
							LEFT JOIN XFC_PLT_AUX_FixedVariableCosts D
								ON A.id_account = D.id_account
								AND YEAR(A.date) = YEAR(D.date)
								AND MONTH(A.date) = MONTH(D.date)
								AND A.scenario = D.scenario
						
							WHERE 1=1
								AND YEAR(A.date) = {year}
								AND A.id_factory = '{factory}'
								AND (A.id_account = '3D32' OR A.id_account = '3D34')
								AND A.scenario = '{scenarioRef}'
								AND D.scenario = '{scenarioRef}'
						
							GROUP BY A.id_factory, YEAR(A.date), MONTH(A.date), A.id_account, A.id_averagegroup, C.activity_index, C.uotype, D.value
						),
						
						energyIndex as (
							SELECT
								id_factory
								, time
								, year
								, month
								, CASE 
										WHEN id_account = '3D32' THEN 'Electricity'
										WHEN id_account = '3D34' THEN 'Gas'
								END  AS Energy
								, SUM(CosteTotal_GM) As CosteTotal
								, SUM(CosteTotal_GM * activity_index) as CosteTotalAjustado
								, (SUM(CosteTotal_GM*activity_index)/SUM(CosteTotal_GM)) AS EnergyIndex
								, variability*(SUM(CosteTotal_GM*activity_index)/SUM(CosteTotal_GM)) + (1-variability) AS EnergyIndexSemi
								, variability*(SUM(CosteTotal_GM*activity_index)/SUM(CosteTotal_GM)) AS Index_Variable
								, avg(activity_index) as ActivityIndexAverage
								, variability As Variability
								, 1-variability As Fixed
						
							FROM activityIndex_1
						
							WHERE 1=1
								-- AND activity_index < 2
					
							GROUP BY id_factory, id_account, month, year, time, variability
						),
						
						divisa as (
							SELECT R1.Month, R1.rate as TipoCambio_ACT, R2.rate as TipoCambio_REF 
							
							FROM fxRate R1
					
							LEFT JOIN fxRateRef R2
								ON R1.Month = R2.Month
							
						),
						
						EnergyVariance as (
							SELECT 
								A.Month
								, A.Factory
								, A.Energy
								, A.REF_Price * C.TipoCambio_REF as REF_Price
								, A.REF_Consumption						
								, A.ACT_Price * C.TipoCambio_ACT as ACT_Price
								, A.ACT_Consumption
								, EnergyIndexSemi
								, ACT_Consumption * (REF_Price * TipoCambio_REF) / 1000 as [ConsoActual at RefPrice]
								, ACT_Consumption * (ACT_Price * TipoCambio_ACT) / 1000 as [ConsoActual at ActPrice]
								, ACT_Consumption * (ACT_Price * TipoCambio_ACT - REF_Price * TipoCambio_REF) / 1000 as [Price Variance]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_ACT /1000 as [ConsRefSemi at RefPrice at Parity]
								, (ACT_Consumption * REF_Price / 1000) -(REF_Consumption * (EnergyIndexSemi) * REF_Price /1000) as [Consumption Vairance]
								, REF_Price * TipoCambio_ACT  as [Reference Price at Parity]
								, REF_Consumption * REF_Price * TipoCambio_ACT /1000 as [ConsoRef at RefPrice]
								, (TipoCambio_ACT - TipoCambio_REF)*ACT_Consumption*ACT_Price/1000 as [Parity Variance]
								, (ACT_Consumption * ACT_Price * TipoCambio_ACT / 1000) - (REF_Consumption * (EnergyIndexSemi) * REF_Price /1000) as [Total Variance]
								-- , B.Variability
								-- , TipoCambio_ACT 
								-- , TipoCambio_REF 
								
						
							FROM preCalculate A

							LEFT JOIN energyIndex B
								ON A.Month = B.Month						
								AND A.Factory = B.id_factory
								AND A.Energy = B.Energy
						
							LEFT JOIN divisa C
								ON A.Month = C.Month			
					
							WHERE 1=1
								AND B.time = 'UO1'
					)
					"
						
					#End Region
					
				#Region "Update FACT Costs"
				
				Else If query = "Update_Fact_Costs"
							
							sql = $"
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_RAW_CostsMonthly
								WHERE 1=1
									AND CAST([month] as INT) = 16					
					
								DELETE FROM XFC_PLT_FACT_Costs
								WHERE 1=1
									AND (id_factory = '{factory}' OR id_factory = 'TBD' OR '{factory}' = 'ALL')
									AND scenario = 'Actual'
									AND YEAR(date) = {year}						
									AND MONTH(date) = {month}
					
								-- 2- Insert statement				
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_factory, date, id_account, id_rubric, id_costcenter, value, currency)
								SELECT *
								FROM (     
									SELECT  
										'Actual' as scenario
									    , id_factory_map AS id_factory
									    , DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date
									    , mng_account as id_account
										, rubirc
									    , ISNULL(id_costcenter,'-1') as id_costcenter
									    , SUM(value_intern) as value
									    , 'Local' as currency
									FROM (
									      		SELECT A.*
														, COALESCE(B.mng_account,'Others') as mng_account
								                FROM (	SELECT *, 
															CASE
												                WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
												                WHEN CONCAT('R',id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
												                WHEN CONCAT('R',id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
												                WHEN CONCAT('R',id_factory, entity) = 'R1301003' THEN 'R0548913'
												                WHEN CONCAT('R',id_factory, entity) = 'R1301002' THEN 'R0548914'
												                WHEN CONCAT('R',id_factory, entity) LIKE '%R0585%' THEN 'R0585'
												                WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
												                WHEN CONCAT('R',id_factory, entity) LIKE '%R0592%' THEN 'R0592'
												                ELSE 'TBD' END AS id_factory_map
														FROM XFC_PLT_RAW_CostsMonthly) A
			
									            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
									             	ON A.id_costcenter = C.id
									          		AND C.scenario = 'Actual'
									          		AND DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) BETWEEN C.start_date AND C.end_date
												LEFT JOIN (
															SELECT DISTINCT costcenter_type
															FROM XFC_PLT_Mapping_Accounts_docF 
														) as E
													ON C.type = E.costcenter_type	
												LEFT JOIN (
															SELECT DISTINCT 
																cnt_account
																, mng_account
																, costcenter_type
																, costcenter_nature
																, docF
															FROM XFC_PLT_Mapping_Accounts_docF 
														) as B					
									       			ON A.num_cpte = B.cnt_account
													AND C.type = B.costcenter_type
													AND C.nature = B.costcenter_nature
													AND A.domFonc = B.docF	
			 
									      		WHERE 1=1
													AND CAST(A.[month] as INT) = {month}										
													AND A.year = {year}	
												    AND (id_factory_map = '{factory}' OR '{factory}' = 'ALL')
									      		)  AS ext
									      GROUP BY id_factory_map, [year], [month], mng_account, rubirc, id_costcenter
									) AS mapeo	
									WHERE 1=1
											
									"
							
				#End Region								
				
				End If	
				
				Return sql 
				
                Return Nothing
				
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
			
        End Function

	#Region "Graph Function"

		Private Function CreateSeries(ByVal si As SessionInfo, _
									name As String, _
									type As XFChart2SeriesType, _ 
									color As String, _
									seriesData As DataTable, _
		                            valueField As String, _
									argumentField As String, _
									Optional useSecondaryAxis As Boolean = False, _
		                            Optional showMarkers As Boolean = False, _
									Optional markerSize As Integer = 0, _
		                            Optional lineThickness As Integer = 0, _
									Optional modelType As XFChart2ModelType = Nothing, _
		                            Optional barWidth As Double = 0,
									Optional parameterField As String = "NoIndicado", _
									Optional parameterType As String = "NoIndicado") As XFSeries
			Try
				    Dim series As New XFSeries()
    				
					' Características comunes
				    series.UniqueName = name
				    series.Type = type
				    series.Color = color
				    series.UseSecondaryAxis = useSecondaryAxis
					
					' Tipo de series - Linea o de Barras
				    If type = XFChart2SeriesType.Line Then
				        series.SeriesAnimationType = XFChart2SeriesAnimationType.Line2DUnwrapHorizontally
				        If showMarkers Then
				            series.ShowMarkers = 1
				            series.MarkerSize = markerSize
				        End If
				        If lineThickness > 0 Then
				            series.LineThickness = lineThickness
				        End If
				    ElseIf type = XFChart2SeriesType.BarSideBySide Then
				        series.ModelType = modelType
				        series.BarWidth = barWidth
				    End If
					
					' Insertando los datos en la serie
				    For Each row As DataRow In seriesData.Rows
				        Dim point As New XFSeriesPoint()
				        point.Argument = row(argumentField).ToString()
				        point.Value = Convert.ToDouble(row(valueField))
						
						' Condición para variaciones
						If type = XFChart2SeriesType.Line Then
							If point.Value >= 0 Then
								point.Color = "Green"
							Else
								point.Color = "Red"
							End If		
						End If
						
						' Valores de los parámetros - Según el tipo elegido
						If parameterType = "Texto" Then
							point.ParameterValue = "" & row(parameterField) & ""
						Else If parameterType = "Texto2"
							point.ParameterValue = "'" & row(parameterField) & "'"
						Else If parameterType = "Numero" Then
							point.ParameterValue = row(parameterField)
						Else
							point.ParameterValue = parameterField
						End If
						
						' Añadir el punto
						series.AddPoint(point)
				    Next
														
				    Return series
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function

	#End Region		

	#Region "Create Excel"
	
		Public Function CreateExcel(
			ByVal si As SessionInfo, _
			Optional factory As String = "", Optional year As Integer = 0, Optional month As Integer = 0, _
			Optional scenario As String = "", Optional time As String = "", _
			Optional folderPath As String = "Documents/Public/Plants/Exports", Optional fileName As String = "Export.xlsx", Optional destPath As String = "", _
			Optional sql As String = "", Optional dt As DataTable = Nothing
			)
				' Generar un excel de cualquiera de la tablas
				
				' --------------- VARIABLES ---------------
				
				' VARIABLES
				'	Fichero
               	folderPath = "Documents/Public/Plants/Exports"
				
				Dim filePath As String = $"{folderPath}/{fileName}"
				If destPath = "" Then destPath = $"{folderPath}/{fileName}"
                Dim modified As Boolean = False
 				
				'	Consulta para poblar el excel
'				Dim dt As New DataTable 
			    Dim startRow As Integer = 2
                Dim endRow As Integer = 3		
				
			#Region "Templates"	

				' Definir el rango de filas a modificar (por ejemplo, de la fila 11 a la 100)
				Dim templateFile As Boolean = False			

				If fileName = "XFC_PLT_PLAN_CostsInput.xlsx" Then
					templateFile = True
               	 	startRow = 11
					sql = $"
							SELECT id_costcenter, id_account, M01, M02, M03, M04, M05, M06, M07, M08, M09, M10, M11, M12
							
							FROM VIEW_PLT_FACT_Costs
					
							WHERE 1=1
								AND scenario = '{scenario}'
								AND id_factory = '{factory}'
								AND year = {year}
					
					"
				Else If fileName = "XFC_PLT_PLAN_VolumesInput.xlsx" Then
					templateFile = True
               	 	startRow = 11
					sql = $" 
							SELECT *
					
							FROM VIEW_PLT_AUX_Production_Planning_Volumes
					
							WHERE 1=1
								AND scenario = '{scenario}' 
								AND id_factory = '{factory}' 
								AND year = {year}							
					"
				Else If fileName = "XFC_PLT_PLAN_AllocationsInput.xlsx" Then
					templateFile = True
               	 	startRow = 11
					sql = $" 
							SELECT *
					
							FROM VIEW_PLT_AUX_Production_Planning_Times
					
							WHERE 1=1
								AND scenario = '{scenario}' 
								AND id_factory = '{factory}' 
								AND year = {year}
								AND uotype = '{time}'
					"	
				End If				
			#End Region
			
			#Region "DataTable"
				If sql <> "" Or templateFile = True Then	
					' Creamos el DataTable con los datos
		            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)	                               
		                dt = BRAPi.Database.ExecuteSql(dbConnApp, sql, True)					
		            End Using  
				Else 
				End If
				
			#End Region	
			
                endRow = startRow + dt.Rows.Count - 1

			#Region "Modify Excel"
			
				' --------------- MODIFICACIÓN DEL EXCEL ---------------
				
                ' Verificar si el archivo existe
                Dim fileExists As Boolean = brapi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath)
                If Not fileExists Then
                    brapi.ErrorLog.LogMessage(si, "El archivo no se encontró en la ruta: " & filePath)
                    Return Nothing
                End If
				
                ' Obtener el archivo en bytes
                Dim fileBytes As Byte() = brapi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase, filePath, True, True).XFFile.ContentFileBytes
                If fileBytes Is Nothing OrElse fileBytes.Length = 0 Then
                    Return Nothing
                End If
 
                ' Cargar el archivo en un MemoryStream
                Dim memoryStream As New MemoryStream()
                memoryStream.Write(fileBytes, 0, fileBytes.Length)
                memoryStream.Position = 0
 				
                Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Open(memoryStream, True)

                    Dim workbookPart As WorkbookPart = spreadsheetDocument.WorkbookPart
 
                    ' Seleccionar la hoja "XLSX Import"
                    Dim targetSheetName As String = "XLSX Import"
                    Dim sheet As Sheet = workbookPart.Workbook.Sheets.Elements(Of Sheet)().FirstOrDefault(Function(s) s.Name.Value = targetSheetName)
                    If sheet Is Nothing Then
                        brapi.ErrorLog.LogMessage(si, "No se encontró la hoja: " & targetSheetName)
                        Return Nothing
                    End If
 
                    Dim worksheetPart As WorksheetPart = CType(workbookPart.GetPartById(sheet.Id.Value), WorksheetPart)
                    Dim sheetData As SheetData = worksheetPart.Worksheet.Elements(Of SheetData)().FirstOrDefault()

					' Eliminar el dato existente - Elegir un método, vaciar el dato o 
					
					' 🔴 PASO 1: Limpiar los valores de las celdas (sin eliminar las filas)
					For Each row As Row In sheetData.Elements(Of Row)()
					    If CInt(row.RowIndex.Value) >= startRow Or templateFile = False Then
					        For Each cell As Cell In row.Elements(Of Cell)()
					            ' Limpia el contenido de la celda
'								cell.CellValue = New CellValue(String.Empty)								
'					            cell.DataType = New EnumValue(Of CellValues)(CellValues.String)

								cell.CellValue = Nothing
								cell.DataType = Nothing
								
					        Next
					    End If
					Next
										
					' Guardar la hoja después de limpiar
					worksheetPart.Worksheet.Save()
					
					If templateFile = False Then
						' Obtener la primera fila; si no existe, se crea
						Dim headerRow As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) CInt(r.RowIndex.Value) = 1)
						
						If headerRow Is Nothing Then
						    headerRow = New Row() With {.RowIndex = 1}
						    sheetData.Append(headerRow)
						End If
						
						' Insertar nombres de las columnas
						For colIndex As Integer = 1 To dt.Columns.Count
						    Dim colLetter As String = GetExcelColumnName(colIndex)
						    Dim cellReference As String = colLetter & "1"
						    
						    ' Buscar la celda en la fila 1
						    Dim cell As Cell = headerRow.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
						
						    ' Obtener el nombre de la columna
						    Dim valor As String = dt.Columns(colIndex - 1).ColumnName  
						
						    If cell Is Nothing Then
						        ' Si la celda no existe, la creamos
						        cell = New Cell() With {
						            .CellReference = cellReference,
						            .CellValue = New CellValue(valor),
						            .DataType = New EnumValue(Of CellValues)(CellValues.String)
						        }
						        headerRow.Append(cell) ' Se añade la celda a la fila
						    Else
						        ' Si la celda ya existe, sobrescribimos el valor
						        cell.CellValue = New CellValue(valor)
						        cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
						    End If
						Next
					End If
						
					' Insertar los datos nuevos
                    For rowIndex As Integer = startRow To endRow
						Dim dtRowIndex As Integer = rowIndex - startRow
                        ' Obtener la fila; si no existe, se crea y se agrega al SheetData
                        Dim row As Row = sheetData.Elements(Of Row)().FirstOrDefault(Function(r) CInt(r.RowIndex.Value) = rowIndex)				
													
                        If row Is Nothing Then
                            row = New Row() With {.RowIndex = CType(rowIndex, UInt32)}
                            sheetData.Append(row)
                        End If
						
                        ' Recorrer columnas del DataTable
                        For colIndex As Integer = 1 To dt.Columns.Count
							Dim valor As String = If(IsDBNull(dt.Rows(dtRowIndex)(colIndex-1)), String.Empty, dt.Rows(dtRowIndex)(colIndex-1).ToString())									
							
                            Dim colLetter As String = GetExcelColumnName(colIndex)
                            Dim cellReference As String = colLetter & rowIndex.ToString()
 
'                            ' Buscar la celda en la fila
                            Dim cell As Cell = row.Elements(Of Cell)().FirstOrDefault(Function(c) c.CellReference.Value = cellReference)
                            If cell Is Nothing Then

'                                 Crear la celda si no existe
								cell = New Cell()
                                cell.CellReference = cellReference
								
								If IsNumeric(valor) Then
									cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString(CultureInfo.InvariantCulture))
									cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
								Else
								    cell.CellValue = New CellValue(valor)
								    cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
								End If		
								
								row.Append(cell)
								
                            Else
								
                                ' Si ya existe, modificarla - Si es un valor numérico hacemos la conversión
								If IsNumeric(valor) Then
								    cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString(CultureInfo.InvariantCulture))
								    cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
								Else
								    cell.CellValue = New CellValue(valor)
								    cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
								End If
								
                            End If
                            modified = True
                        Next

                    Next
 
                    If modified Then
                        worksheetPart.Worksheet.Save()
                        workbookPart.Workbook.Save()
                    Else
                        brapi.ErrorLog.LogMessage(si, "No se encontró ninguna celda para modificar.")
                    End If
                End Using
				
 			#End Region
			
			#Region "Create File"
				' --------------- GUARDADO FINAL DEL ARCHIVO ---------------
				
                ' Actualizar el archivo en OneStream
                Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, destPath)
                Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, memoryStream.ToArray())
'                Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, GenerateExcelBytesFromDataTable(si, dt, "XLSX Import"))
				fileFile.FileInfo.ContentFileExtension = "xlsx"
                brapi.FileSystem.InsertOrUpdateFile(si, fileFile)
				
			#End Region
			
			Return Nothing
			
		End Function
		
		#Region "Helper Functions for Excel"
               ' Función que obtiene el valor de una celda, considerando si es SharedString
        Public Function GetCellValue(si As SessionInfo, document As SpreadsheetDocument, cell As Cell) As String
            If cell Is Nothing OrElse cell.CellValue Is Nothing Then
                Return ""
            End If
 
            Dim value As String = cell.CellValue.InnerText
            If cell.DataType IsNot Nothing AndAlso cell.DataType.Value = CellValues.SharedString Then
                Dim sharedStringTable = document.WorkbookPart.SharedStringTablePart.SharedStringTable
                value = sharedStringTable.ChildElements(CInt(value)).InnerText
            End If
 
            Return value
        End Function
 
        ' Función para crear una celda con el texto dado
		Public Function CreateCell(valor As String) As Cell
		    Dim cell As New Cell()
		    
		    If IsNumeric(valor) Then
		        Dim valorNumerico As Double = Convert.ToDouble(valor)
		        cell.CellValue = New CellValue(valorNumerico.ToString(CultureInfo.InvariantCulture))
		        cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
		    Else
		        cell.CellValue = New CellValue(valor)
		        cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
		    End If
		
		    Return cell
		End Function
 
        ' Función para obtener el nombre de la columna en Excel (1 -> A, 2 -> B, etc.)
        Public Function GetExcelColumnName(ByVal columnNumber As Integer) As String
            Dim dividend As Integer = columnNumber
            Dim columnName As String = String.Empty
            Dim modulo As Integer
 
            While dividend > 0
                modulo = (dividend - 1) Mod 26
                columnName = Chr(65 + modulo) & columnName
                dividend = CInt((dividend - modulo) \ 26)
            End While
 
            Return columnName
        End Function
 
        ' Función para insertar la celda en orden dentro de la fila
		Public Sub InsertCellInOrder(ByVal row As Row, ByVal newCell As Cell)
		    Dim refCell As Cell = Nothing
		
		    For Each cell As Cell In row.Elements(Of Cell)()
		        If String.Compare(cell.CellReference.Value, newCell.CellReference.Value, True) > 0 Then
		            refCell = cell
		            Exit For
		        End If
		    Next
		
		    If refCell IsNot Nothing Then
		        row.InsertBefore(newCell, refCell)
		    Else
		        row.Append(newCell) ' Si no hay referencia, añadir al final
		    End If
		End Sub
		
	#End Region
	
	#End Region	
	
	#Region "Create CSV"
		Public Function CreateCSV(
			ByVal si As SessionInfo, _
			Optional folderPath As String = "Documents/Public/Plants/Exports", _
			Optional fileName As String = "Export.csv", _
			Optional destPath As String = "", _
			Optional dt As DataTable = Nothing
			)
			
			'Create filename and path
	       Dim filepath As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, $"{folderPath}")
	       
	       'Use a string builder object
	       Dim filesAsString As New StringBuilder()
	       
	       'Headers as first row
	       filesAsString.AppendLine(String.Join(",", dt.Columns.Cast(Of DataColumn).select(Function(x) x.ColumnName)))
	       
	       'Loop the rows
	       For Each dr As DataRow In dt.Rows
	        'Use String.join to create a comma separated line
	        filesAsString.AppendLine(String.Join(",", dr.ItemArray().Select(Function(item) item.ToString().Replace(",", ".")).ToArray()))
	       Next dr
	       
	       'Convert the string to an array of byte
	       Dim fileAsByte() As Byte = system.Text.Encoding.Unicode.GetBytes(filesAsString.ToString)
		   
	       'Save fileAsByte
	       Dim fileDataInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, $"{fileName}", filePath.XFFolder.FullName)
	       Dim fileData As New XFFile(fileDataInfo, String.Empty, fileAsByte)
		   fileDataInfo.ContentFileExtension = "csv"
		   
	       BRApi.FileSystem.InsertOrUpdateFile(si, fileData)			
			
		End Function	
	#End Region
	
	#Region "Generate Excel Bytes from Data Table"
		
		Public Function GenerateExcelBytesFromDataTable(ByVal si As SessionInfo, ByVal dataTable As DataTable, ByVal sheetName As String) As Byte()
		    Try
		        ' Create a MemoryStream to hold the Excel file data
		        Using memoryStream As New MemoryStream()
		            ' Create a new SpreadsheetDocument in memory
		            Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook)
		                ' Add a WorkbookPart to the document
		                Dim workbookPart As WorkbookPart = spreadsheetDocument.AddWorkbookPart()
		                workbookPart.Workbook = New Workbook()
		
		                ' Add a WorksheetPart to the WorkbookPart
		                Dim worksheetPart As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
		                worksheetPart.Worksheet = New Worksheet(New SheetData())
		
		                ' Add Sheets to the Workbook
		                Dim sheets As Sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(Of Sheets)(New Sheets())
		
		                ' Append a new worksheet and associate it with the workbook
		                Dim sheet As Sheet = New Sheet() With {
		                    .Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
		                    .SheetId = 1,
		                    .Name = sheetName
		                }
		                sheets.Append(sheet)
		
		                ' Get the sheetData of the worksheet
		                Dim sheetData As SheetData = worksheetPart.Worksheet.GetFirstChild(Of SheetData)()
		
		                ' Add header row
		                Dim headerRow As Row = New Row()
		                For Each column As DataColumn In dataTable.Columns
		                    headerRow.Append(CreateCell(column.ColumnName))
		                Next
		                sheetData.Append(headerRow)
		
		                ' Add data rows
		                For Each dataRow As DataRow In dataTable.Rows
		                    Dim newRow As Row = New Row()
		                    For Each item As Object In dataRow.ItemArray
		                        newRow.Append(CreateCell(If(item IsNot Nothing, item.ToString(), String.Empty)))
		                    Next
		                    sheetData.Append(newRow)
		                Next
		
		                ' Save the workbook
		                workbookPart.Workbook.Save()
		            End Using
		
		            ' Return the MemoryStream as a byte array
		            Return memoryStream.ToArray()
		        End Using
		    Catch ex As Exception
		        Throw ErrorHandler.LogWrite(si, New XFException($"Error generating Excel file: {ex.Message}"))
		    End Try
		End Function
		
		#End Region
		
	#Region "Valid Scenario Period"
	
		Public Function ValidScenarioPeriod(ByVal si As SessionInfo, ByVal scenario As String, ByVal year As String) As Boolean
			
			Dim scenarioForecast As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_forecast")
			Dim scenarioBudget As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_budget")
			Dim actualYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_year_N")						
			
'			BRApi.ErrorLog.LogMessage(si, scenarioForecast)
			Return True

			If scenario = "Actual" And year = actualYear Then
				Return True
			ElseIf scenario = scenarioForecast And year = actualYear Then
				Return True
			ElseIf scenario = scenarioBudget And year = actualYear + 1 Then	
				Return True				
			End If
			
			Return False
			
		End Function
		
	#End Region		
	
	End Class
	
End Namespace