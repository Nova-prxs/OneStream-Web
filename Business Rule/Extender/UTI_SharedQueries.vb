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

Namespace OneStream.BusinessRule.Extender.UTI_SharedQueries
	
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
						Optional monthFCT As String = "0", _
						Optional accountList As String = "'All'", _
						Optional vtu_version As String = "Old", _
						Optional impactParity As String = ""
						) As Object
            Try
				
				Dim sql As String = String.Empty
				
				Dim scenarioRefForYear As String = scenarioRef
				scenarioRef = scenarioRef.Replace("_PrevYear","")
				
				#Region "COST DISTRIBUTION"				
					 
				If query.Contains("CostsDistribution_") Then
					
					Dim monthFilter As String = If(scenario="Actual", $"MONTH(date) = {month}",$"MONTH(date) > {monthFCT}")
					Dim monthFilter_A As String = If(scenario="Actual",$"MONTH(A.date) = {month}" ,$"MONTH(A.date) > {monthFCT}")
					Dim rowCount As String = String.Empty
					Dim decimales As String = If(factory="R0592" or factory="R0585", "7", "10")
					
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
										AND B.type = 'A'
									
									LEFT JOIN accountMapping C
										ON A.id_account = C.mng_account
									
									WHERE 1=1
										AND A.id_factory = '{factory}'
										AND {monthFilter_A}
										AND YEAR(A.date) = {year}
										AND A.scenario = '{scenario}'
										AND A.id_account <> 'Others'
									
									GROUP BY A.id_costcenter, B.id_averagegroup, B.nature, A.id_account, C.account_type, MONTH(A.date)
						
									HAVING NULLIF(SUM(A.value),0) <> 0
								),
					"
					#End Region
					
					Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						rowCount = (CInt(BRApi.Database.ExecuteSql(dbConn, commonQueryCosts & " aux as (SELECT 'INUTIL' AS Inutil ) " & " SELECT COUNT(*) as Rows FROM costsData", True).Rows(0)(0)) + 100 ).ToString
					End Using

					If factory = "R0592" Then
						
						commonQueryCosts = commonQueryCosts.Replace(
										"SELECT
										MONTH(A.date) as month", 
										$"	SELECT TOP({rowCount}) 
												MONTH(A.date) as month ")
										
					End If
										
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
									CONVERT(DECIMAL(18,{decimales}), ISNULL(SUM(activity_taj), 0)) AS total_activity_taj
								
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
									, CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_taj)) / CONVERT(DECIMAL(18,{decimales}), NULLIF(B.total_activity_taj, 0))  AS activity_weight_taj
								
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
								
								GROUP BY MONTH(A.date), A.id_averagegroup, A.id_product, A.uotype , A.id_costcenter, B.total_activity_taj
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
									, A.TotalAmount * B.activity_weight_taj as SharedCost_TAJ
									, B.uotype

								FROM costsData A
					
								INNER JOIN productionData B
									ON A.id_averagegroup = B.id_averagegroup
									AND (A.id_costcenter = B.id_costcenter OR B.id_costcenter = '-1')
									AND (ISNULL(activity_weight_taj,0) <> 0)
									AND A.month = B.month
					
								WHERE 1=1
									AND A.nature = 'CP'
									-- AND ((A.account_type = 1 and B.uotype = 'UO1') OR (ISNULL(A.account_type, 0) <> 1 and B.uotype = 'UO2'))  	
									
								-- GROUP BY 
								-- 	A.month
								-- 	, A.nature
								-- 	, A.id_averagegroup
								-- 	, A.id_costcenter
								-- 	, A.id_account
								-- 	, A.account_type
								-- 	, A.id_averagegroup
								-- 	, B.id_product
								-- 	, B.uotype
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
								, CONVERT(DECIMAL(18,8), SUM(SharedCost_TAJ)) as value
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
								, CONVERT(DECIMAL(18,8), SUM(SharedCost_TAJ)) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM cpSharedCosts
					
							WHERE (ISNULL(account_type, 0) <> 1 and uotype = 'UO2')
							
							GROUP BY id_account, CeCo, destination, id_product, month
					
						)
					"
					
					' BRAPI.ErrorLog.LogMessage(si, sql)
					
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
									CONVERT(DECIMAL(18,{decimales}), COALESCE(SUM(CAST(activity_taj AS DECIMAL(18,6))), 0)) AS total_activity_taj,
									CONVERT(DECIMAL(18,{decimales}), COALESCE(SUM(CAST(activity_tso AS DECIMAL(18,6))), 0)) AS total_activity_tso
								
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
										ELSE CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_taj)) / CONVERT(DECIMAL(18,{decimales}), B.total_activity_taj)
									END AS activity_weight_cat_taj
									, CASE 
										WHEN B.total_activity_tso = 0 THEN 0 
										ELSE CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_tso)) / CONVERT(DECIMAL(18,{decimales}), B.total_activity_tso)
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
									, CONVERT(DECIMAL(18,{decimales}), SUM(percentage)) as percentageTotal

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
									, CONVERT(DECIMAL(18,{decimales}), A.percentage) / CONVERT(DECIMAL(18,{decimales}), B.percentageTotal) as percentage

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
									, A.TotalAmount * B.percentage as GM_SharedCosts
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
									, A.GM_SharedCosts * B.activity_weight_cat_taj  as SharedCost_TAJ
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
								, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) as value
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
								, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) as value
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
									CONVERT(DECIMAL(18,{decimales}), COALESCE(SUM(activity_taj), 0)) AS total_activity_taj,
									CONVERT(DECIMAL(18,{decimales}), COALESCE(SUM(activity_tso), 0)) AS total_activity_tso
								
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
										ELSE CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_taj)) / CONVERT(DECIMAL(18,{decimales}), B.total_activity_taj)
									END AS activity_weight_cat_taj
									, CASE 
										WHEN B.total_activity_tso = 0 THEN 0 
										ELSE CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_tso)) / CONVERT(DECIMAL(18,{decimales}), B.total_activity_tso)
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
									, CONVERT(DECIMAL(18,{decimales}), SUM(value)) as SharedCost_TAJ
					
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
									, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) AS TotalTAJ
								
								FROM costsShared_CP_CAT
								
								GROUP BY month
					
							),
					
							caaWeight as (
								SELECT
									A.month
									, id_averagegroup
									, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) / CONVERT(DECIMAL(18,{decimales}), SUM(B.TotalTAJ)) as caa_weight_taj
								
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
									, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) as value
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
							, CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_taj)) as activity_taj
						
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
								AND A.nature + COALESCE(B.id_product,'_NoProduct') = 'CP_NoProduct'
								-- AND A.nature = 'CP'
								-- AND B.id_product IS NULL
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
							CONVERT(DECIMAL(18,{decimales}), COALESCE(SUM(activity_taj), 0)) AS total_activity_taj,
							CONVERT(DECIMAL(18,{decimales}), COALESCE(SUM(activity_tso), 0)) AS total_activity_tso
						
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
								ELSE CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_taj)) / CONVERT(DECIMAL(18,{decimales}), B.total_activity_taj)
							END AS activity_weight_cat_taj
							, CASE 
								WHEN B.total_activity_tso = 0 THEN 0 
								ELSE CONVERT(DECIMAL(18,{decimales}), SUM(A.activity_tso)) / CONVERT(DECIMAL(18,{decimales}), B.total_activity_tso)
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
								, CONVERT(DECIMAL(18,{decimales}), SUM(value)) as SharedCost_TAJ
				
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
								, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) AS TotalTAJ
							
							FROM costsShared_CP_CAT
					
							GROUP BY month
				
						),
				
						caaWeight as (
							SELECT
								A.month
								, id_averagegroup
								, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) / CONVERT(DECIMAL(18,{decimales}), SUM(B.TotalTAJ)) as caa_weight_taj
							
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
									AND A.nature + COALESCE(CAST(B.percentage AS VARCHAR),'_NoPercentage') = 'CAT_NoPercentage'
									-- AND A.nature = 'CAT'
									-- AND B.percentage IS NULL									
																		
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
									AND A.nature +'-' = 'CAT-'							
																		
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
									AND ISNULL(B.id_product,'NoProductActivity') = 'NoProductActivity'
									-- AND B.id_product IS NULL
																		
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
									, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) as value
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
									, CONVERT(DECIMAL(18,{decimales}), SUM(SharedCost_TAJ)) as value
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
									, SUM(CAST(A.value AS DECIMAL(18,6))) as SharedCost_TAJ
					
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
									AND (B.type <> 'A' OR (B.type = 'A' AND B.nature = '#')
					
										--Exception SAP4H (Scraps)
										OR (id_account = '2820'))
					
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
								, ISNULL(CeCo,'-1') as id_costcenter
								, ISNULL(destination,'-1') as id_averagegroup
								, id_product
								, SUM(CAST(SharedCost_TAJ AS DECIMAL(18,6))) as value
								, 'TAJ' as type_tso_taj
								, 'NA' as type
					
							FROM othSharedCosts
							
							GROUP BY id_account, CeCo, destination, id_product, month
					
						)
					"	
					
'					BRApi.ErrorLog.LogMessage(si, sql)
					#End Region
					
						End If
						
				#End Region
					
				#Region "Activity TSO"				
													
				ElseIf query = "ActivityTSO"					
					
					Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
					
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
										, F.volume
											* CASE WHEN '{factory}' = 'R0483003' and ('{scenarioRef}' = 'RF06' OR '{scenarioRef}' = 'RF09') THEN
					
															CASE WHEN COALESCE(B.value,0) <> 0 THEN ISNULL(B.value,0)/costcenters_per_group
												    	  	ELSE F.allocation_taj END
													ELSE
															CASE WHEN COALESCE(B.value,0) <> 0 THEN ISNULL(B.value,0) 
												    	  	ELSE F.allocation_taj END
													END
											AS activity_tso
					
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
												, activity_tso
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
											AND YEAR(date) = {yearRef}						
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
											AND YEAR(date) = {yearRef}						
											AND id_factory = '{factory}'	
											AND value <> 0 ) B
		
										ON F.month = B.month
										AND F.id_averagegroup = B.id_averagegroup
										AND F.id_product = B.id_product
										AND F.uotype = B.uotype
					
								LEFT JOIN (
									SELECT 
										id_averagegroup , COUNT(DISTINCT id_costcenter) AS costcenters_per_group
									FROM XFC_PLT_FACT_Production								
											WHERE scenario = '{scenario}'
											AND YEAR(date) = {year}						
											AND id_factory = '{factory}'	
											AND activity_taj <> 0
									GROUP BY id_averagegroup
								) GM_CC
								ON GM_CC.id_averagegroup = F.id_averagegroup
					
								)
					"
					' BRAPI.ErrorLog.LogMessage(si, sql)
				#End Region
				
				#Region "Activity TSO (All Factories)"				
													
				ElseIf query = "ActivityTSO_All_Factories"					
					
					Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
					
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
										, F.volume  * 	CASE WHEN F.id_factory = 'R0483003' and ('{scenarioRef}' = 'RF06' OR '{scenarioRef}' = 'RF09') THEN
					
																CASE WHEN COALESCE(B.value,0) <> 0 THEN ISNULL(B.value,0)/costcenters_per_group
													    	  	ELSE F.allocation_taj END
														ELSE
																CASE WHEN COALESCE(B.value,0) <> 0 THEN ISNULL(B.value,0) 
													    	  	ELSE F.allocation_taj END
					
														END AS activity_tso
					
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
											AND YEAR(date) = {yearRef}																	
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
											AND YEAR(date) = {yearRef}						
											AND value <> 0 ) B
		
										ON F.id_factory = B.id_factory
										AND F.month = B.month
										AND F.id_averagegroup = B.id_averagegroup
										AND F.id_product = B.id_product
										AND F.uotype = B.uotype
					
									LEFT JOIN (
										SELECT 
											id_factory, MONTH(date) AS month, id_averagegroup , COUNT(DISTINCT id_costcenter) AS costcenters_per_group
										FROM XFC_PLT_FACT_Production								
												WHERE scenario = '{scenario}'
												AND YEAR(date) = {year}			
												AND activity_taj <> 0
										GROUP BY id_factory, MONTH(date), id_averagegroup
									) GM_CC
									ON GM_CC.month = F.month
									AND GM_CC.id_factory = F.id_factory 
									AND GM_CC.id_averagegroup = F.id_averagegroup					
					
								)
					
										
					"
					
				#End Region				
				
				#Region "Activity ALL"				
													
				ElseIf query = "ActivityALL"					
					
					Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
					
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
										,DATEFROMPARTS({year},MONTH(F.date),1) AS Date
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
									AND YEAR(F.date) = {yearRef}		
									AND F.id_factory = '{factory}'
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
									AND (F.activity_taj <> 0 OR F.activity_tso <> 0)
									
								)
					"
					
				#End Region			
				
				#Region "Activity ALL (All Factories)"				
													
				ElseIf query = "ActivityALL_All_Factories"					
					
					Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
					
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
										,DATEFROMPARTS({year},MONTH(F.date),1) AS Date
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
									AND YEAR(F.date) = {yearRef}		
									AND (F.uotype = '{time}' OR '{time}' = 'All')	
									AND (F.activity_taj <> 0 OR F.activity_tso <> 0)
									
								)
					"
					
				#End Region																	
				
				#Region "FACT VTU Data"				
													
				ElseIf query = "VTU_data"									
							
					Dim sql_Rate = AllQueries(si, "RATE", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)						
					Dim sql_Product_Component_Recursivity = AllQueries(si, "Product_Component_Recursivity", factory, month, year, scenario, scenarioRef, time, "", type, product, currency)			
					Dim filterMonth As String = If(view = "Periodic", $"AND MONTH(date) = {month}", $"AND MONTH(date) <= {month}")
					
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"		
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								{filterMonth}
							
								AND YEAR(V.date) = {year} 						
								AND id_factory = '{factory}'
								AND scenario = '{scenario}'
					       )		
					
					 		-- FILTER
	
      						,filterProduct AS (

							SELECT DISTINCT id_factory, id_component
           					FROM XFC_PLT_HIER_Nomenclature_Date_Report WHERE scenario = 'Actual' AND year = '{year}' AND id_factory = '{factory}' AND id_product_final = '{product}'
							AND MONTH(date) = {month}
					
           					UNION ALL
					
           					SELECT '{factory}' AS id_factory, '{product}' AS id_component
					
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
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
					
								INNER JOIN filterProduct  R
									ON  F.id_factory = R.id_factory
									AND F.id_product = R.id_component
									--AND MONTH(F.date) = R.Month    			
					
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
								
									{filterMonth}
								
									AND F.scenario = '{scenario}'
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
							  	SELECT F.* 
							  	FROM XFC_PLT_FACT_Production F
					
								INNER JOIN filterProduct  R
									ON  F.id_factory = R.id_factory
									AND F.id_product = R.id_component
									--AND MONTH(F.date) = R.Month  
					
							    WHERE F.id_factory =  '{factory}'
								AND YEAR(F.date) = {year}     
							    
								{filterMonth}
								
								AND F.scenario = '{scenario}'
								AND F.activity_TAJ <> 0
							   )					
							
							, factProduction as (
							       
								SELECT
									F.id_product
									,F.id_product + CASE WHEN NULLIF(A.pond_suffix,'') IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
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
					
								-- Añadido 2025/07/11
								HAVING SUM(F.activity_taj) <> 0
								--		
							)
							
							, activityProduct AS (
							
							      SELECT id_product_gm
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_product_gm, uotype 
							
							)
							
							{sql_Product_Component_Recursivity}
							
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
				
				#Region "FACT VTU Data With Month"				
													
				ElseIf query = "VTU_data_Month"									
							
					Dim sql_Rate = AllQueries(si, "RATE", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)						
					Dim sql_Product_Component_Recursivity = AllQueries(si, "Product_Component_Recursivity", factory, month, year, scenario, scenarioRef, time, "", type, product, currency)			
					Dim filterMonth As String = If(view = "Periodic", $"AND MONTH(date) = {month}", $"AND MONTH(date) <= {month}")
					
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"		
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								{filterMonth}
							
								AND YEAR(V.date) = {year} 						
								AND id_factory = '{factory}'
								AND scenario = '{scenario}'
					       )		
					
					 		-- FILTER
	
      						,filterProduct AS (

							SELECT DISTINCT id_factory, id_component
           					FROM XFC_PLT_HIER_Nomenclature_Date_Report WHERE scenario = 'Actual' AND year = '{year}' AND id_factory = '{factory}' AND id_product_final = '{product}'
							AND MONTH(date) = {month}
					
           					UNION ALL
					
           					SELECT '{factory}' AS id_factory, '{product}' AS id_component
					
							)									
					
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
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
					
								INNER JOIN filterProduct  R
									ON  F.id_factory = R.id_factory
									AND F.id_product = R.id_component
									--AND MONTH(F.date) = R.Month    			
					
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
								
									{filterMonth}
								
									AND F.scenario = '{scenario}'
									AND F.value <> 0
								
								GROUP BY 
									MONTH(F.date)
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
							  	SELECT F.* 
							  	FROM XFC_PLT_FACT_Production F
					
								INNER JOIN filterProduct  R
									ON  F.id_factory = R.id_factory
									AND F.id_product = R.id_component
									--AND MONTH(F.date) = R.Month  
					
							    WHERE F.id_factory =  '{factory}'
								AND YEAR(F.date) = {year}     
							    
								{filterMonth}
								
								AND F.scenario = '{scenario}'
								AND F.activity_TAJ <> 0
							   )					
							
							, factProduction as (
							       
								SELECT
									month
									,F.id_product
									,F.id_product + CASE WHEN NULLIF(A.pond_suffix,'') IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
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
					
						        INNER JOIN (SELECT MONTH(date) AS month, id_factory, id_product, id_averagegroup, uotype, MAX(id_costcenter) AS id_costcenter
								  			FROM factProductionFiltered 
								  			GROUP BY MONTH(date), id_factory, id_product, id_averagegroup, uotype) F2
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
									month
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
					
								-- Añadido 2025/07/11
								HAVING SUM(F.activity_taj) <> 0
								--		
							)
							
							, activityProduct AS (
							
							      SELECT month
										,id_product_gm
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY month, id_product_gm, uotype 
							
							)
													
							, factVTU AS (
					
							SELECT
								C.month
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.id_account
								, C.nature
								, H.type AS type_cc
								, H.nature AS nature_cc
								, C.cost_center						
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total			
								
							FROM ( 	SELECT month, id_product, id_averagegroup, account_type, id_account, nature, cost_center, SUM(cost_fixed) AS cost_fixed, SUM(cost_variable) AS cost_variable, SUM(cost) AS cost
									FROM factCost
									GROUP BY month, id_product, id_averagegroup, account_type, id_account, nature, cost_center ) C
					
							LEFT JOIN factProduction V
							  	ON C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
								ON V.month = T.month
							    AND V.id_product_gm= T.id_product_gm
								AND T.uotype = 'UO1'			
									
							LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist H
						    	ON C.cost_center = H.id
								AND H.scenario = 'Actual'
								AND DATEFROMPARTS({year},{month},1) BETWEEN H.start_date AND H.end_date			
							)
					
							"
							
				#End Region					
				
				#Region "FACT VTU Data All Months"				
													
				ElseIf query = "VTU_data_All_Months"										
				
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")									
																  
					Dim sql_Product_Component_Recursivity = AllQueries(si, "Product_Component_Recursivity", factory, month, year, scenario, scenarioRef, time, "", type, product, currency)																			  
																  
					month = Right("0" & month,2)
					Dim dateStart As String = year & "-01-01"
					Dim dateEnd As String = year & "-" & month & "-01"											
					
					Dim sql_Rate = AllQueries(si, "RATE", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)						
			
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"	
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT id_factory, MONTH(V.date) AS month, id_account, value
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
									F.id_factory
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
					
						       	LEFT JOIN PercVarAccount P
						           	ON F.id_account = P.id_account						          	
									AND F.id_factory = P.id_factory
									AND MONTH(F.date) = P.month					
								
								WHERE F.scenario = '{scenario}'
								AND F.id_factory = '{factory}'
					
									AND YEAR(F.date) = {year}
								
									AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
								
									AND F.value <> 0
									--AND A.account_type IN (1,2,3,4,5)
								
								GROUP BY 
									F.id_factory
									,MONTH(F.date)
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
								
								AND id_factory = '{factory}'
								AND YEAR(date) = {year}   
								AND scenario = '{scenario}'
								AND activity_TAJ <> 0
							   )					
							
							, factProduction as (
							       
								SELECT
									F.id_factory
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ									

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
									AND F.id_factory = A.id_factory
							
								GROUP BY
									F.id_factory
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_factory
										,month
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, month, id_product_gm, uotype 
							
							)
					
							{sql_Product_Component_Recursivity}
							
							, factVTU AS (
																			
							SELECT 
								C.id_factory
								, DATEFROMPARTS(2025,C.month,1) AS Date
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total				
								
							FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									GROUP BY id_factory, month, id_product, id_averagegroup, account_type ) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.month = T.month
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'								
							)
					
							"
							
				#End Region						

				#Region "FACT VTU Data ALL Factories"				
													
				ElseIf query = "VTU_data_All_Factories"										
				
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")				
					
					month = Right("0" & month,2)
					Dim dateStart As String = year & "-01-01"
					Dim dateEnd As String = year & "-" & month & "-01"											
					
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", "", year, scenario, scenarioRef, time, "", "Existing", "", currency)	
					Dim sql_Product_Component_Recursivity = AllQueries(si, "Product_Component_Recursivity", "", month, year, scenario, scenarioRef, time, "", "Existing", product, currency)			
													
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
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
							
							{sql_Product_Component_Recursivity}
							
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
				
				#Region "FACT VTU Data ALL Factories All Months"				
													
				ElseIf query = "VTU_data_All_Factories_All_Months"										
				
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")				
					
					month = Right("0" & month,2)
					Dim dateStart As String = year & "-01-01"
					Dim dateEnd As String = year & "-" & month & "-01"											
					
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", "", year, scenario, scenarioRef, time, "", "Existing", "", currency)	
			
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"	
					
					-- PREVIOUS
					
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
								
									AND F.value <> 0
									--AND A.account_type IN (1,2,3,4,5)
								
								GROUP BY 
									F.id_factory
									,MONTH(F.date)
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ									

						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
					          	INNER JOIN factProductionFiltered  F 
					              	ON F.id_costcenter = C.id           
					
						        INNER JOIN (SELECT id_factory, id_product, id_averagegroup, uotype, MIN(id_costcenter) AS id_costcenter
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
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_factory
										,month
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, month, id_product_gm, uotype 
							
							)
							
							, factVTU AS (
																			
							SELECT 
								C.id_factory
								, DATEFROMPARTS({year},C.month,1) AS Date
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total				
								
							FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									GROUP BY id_factory, month, id_product, id_averagegroup, account_type ) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.month = T.month
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'								
							)
					
							"
				#End Region		
				
				#Region "FACT VTU Data ALL Factories ALL Months [Account Detail]"
				
				' -----------------------------------------------------------------
				' Resultao: factVTUCC
				' -----------------------------------------------------------------
													
				ElseIf query = "VTU_data_All_Factories_All_Months_AccountDetail"										
				
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
								
									AND F.value <> 0
									--AND A.account_type IN (1,2,3,4,5)
								
								GROUP BY 
									F.id_factory
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id		
									--,R1.rate
							)
					
							/* 11-09-2025 Modificación para coincidir con All VTU */
							
							, allMonths_Costs as (
								SELECT DISTINCT month FROM factCost								
							)
							
							, all_GM_by_Product_Costs as (
								SELECT DISTINCT id_factory , id_product , id_averagegroup , account_type , id_account , nature, cost_center  
								FROM factCost
							)
							
                            , all_Combinations_GM_Product_Month_Costs_Aux as (
                                SELECT M.month, GMP.id_factory , GMP.id_product , GMP.id_averagegroup , GMP.account_type , GMP.id_account , GMP.nature, GMP.cost_center                               
                                FROM allMonths_Costs M
                                CROSS JOIN all_GM_by_Product_Costs GMP
                            )
							
							, all_GM_by_Product_FistMonth_Account_type as (
								
								-- SELECT MIN(month) AS first_month, MAX(month) AS last_month, id_factory , id_product , id_averagegroup , account_type  
								SELECT month, id_factory , id_product , id_averagegroup , account_type  
								FROM factCost
								GROUP BY id_factory , id_product , id_averagegroup , account_type, month  
								
							)
							
							, all_Combinations_GM_Product_Month_Costs as (
								SELECT A.* -- , F.month as month_filter, F.first_month, F.last_month
								FROM all_Combinations_GM_Product_Month_Costs_Aux A
								INNER JOIN all_GM_by_Product_FistMonth_Account_type F
									ON  F.id_factory = A.id_factory
									AND F.id_product = A.id_product
									AND F.id_averagegroup = A.id_averagegroup
									AND F.account_type = A.account_type
									AND A.month = F.month
									
							)
							
                            , factCostGM AS (
                                SELECT 
                                      A.id_factory
									, A.month
									, A.id_product
									, A.id_averagegroup
					         		, A.account_type
									, A.id_account
									, A.nature
									, A.cost_center
									-- , A.month_filter
									-- , A.first_month
									-- , A.last_month
									, COALESCE(B.cost_fixed , 0) AS cost_fixed
									, COALESCE(B.cost_variable , 0) AS cost_variable
									, COALESCE(B.cost , 0) AS cost
                                FROM all_Combinations_GM_Product_Month_Costs A
                                LEFT JOIN factCost B
                                    ON A.id_factory = B.id_factory
                                    AND A.month = B.month
                                    AND A.id_product = B.id_product
                                    AND A.id_averagegroup = B.id_averagegroup
                                    AND A.account_type = B.account_type
                                    AND A.id_account = B.id_account
                                    AND A.nature = B.nature
                                    AND A.cost_center = B.cost_center
                            )
					
							/* 11-09-2025 Modificación para coincidir con All VTU */ 
					
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ									

						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
					          	INNER JOIN factProductionFiltered  F 
					              	ON F.id_costcenter = C.id           
					
						        INNER JOIN (SELECT id_factory, id_product, id_averagegroup, uotype, MIN(id_costcenter) AS id_costcenter
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
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_factory
										,month
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, month, id_product_gm, uotype 
							
							)
							
							, factVTU AS (
																			
							SELECT 
								C.id_factory
								, DATEFROMPARTS(2025,C.month,1) AS Date
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.id_account
								--, C.cost_center
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total				
								
							FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, id_account 
										, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCostGM
									GROUP BY id_factory, month, id_product, id_averagegroup, account_type, id_account 
							) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.month = T.month
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'
					
							)							
					
							"
							
				#End Region
								
				#Region "FACT VTU Data ALL Factories All Months [Account Detail] - BackUp"
				
				' -----------------------------------------------------------------
				' 18/06/2025
				' 
				' Hecho para Jose, abriendo VTU_data_All_Factories_All_Months para 
				' traer el detalle de VTU según un número de cuentas seleccionada.
				'
				' Unificar con el anterior o eliminar 
				' -----------------------------------------------------------------
													
				ElseIf query = "VTU_data_All_Factories_All_Months_AccountDetail_BACKUP"										
				
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
								
									AND F.value <> 0
									--AND A.account_type IN (1,2,3,4,5)
								
								GROUP BY 
									F.id_factory
									,MONTH(F.date)
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ									

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
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_factory
										,month
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, month, id_product_gm, uotype 
							
							)
							
							, factVTU AS (
																			
							SELECT 
								C.id_factory
								, DATEFROMPARTS(2025,C.month,1) AS Date
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.id_account
								--, C.cost_center
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total				
								
							FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, id_account --, cost_center
										, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									WHERE 1=1
										--AND (id_account IN ({accountList}))
									GROUP BY id_factory, month, id_product, id_averagegroup, account_type, id_account --, cost_center 
								) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.month = T.month
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'								
							)
					
							, factVTUCC AS (
																			
							SELECT 
								C.id_factory
								, DATEFROMPARTS(2025,C.month,1) AS Date
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.id_account
								, C.cost_center
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total				
								
							FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, id_account , cost_center
										, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									WHERE 1=1
										--AND (id_account IN ({accountList}))
									GROUP BY id_factory, month, id_product, id_averagegroup, account_type, id_account , cost_center 
								) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.month = T.month
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'								
							)					
					
							"
							
				#End Region
				
				#Region "FACT VTU Data ALL Factories ALL Months [Account Detail, CC]"
				
				' -----------------------------------------------------------------
				' Resultao: factVTUCC
				' -----------------------------------------------------------------
													
				ElseIf query = "VTU_data_All_Factories_All_Months_AccountDetail_CC"										
				
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")				
					
					month = Right("0" & month,2)
					Dim dateStart As String = year & "-01-01"
					Dim dateEnd As String = year & "-" & month & "-01"											
					
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", "", year, scenario, scenarioRef, time, "", "Existing", "", currency)	
			
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"	
					
							, productFilter AS (									
								SELECT DISTINCT id_component AS id_product
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 
										scenario = '{scenario}'
										AND year = '{year}'
										AND id_factory = '{factory}'
										AND id_product_final = '{product}'
					
								UNION ALL
					
								SELECT '{product}' AS id_product
							)							
					
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
					
								INNER JOIN productFilter PF
									ON F.id_product = PF.id_product
					
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
					
									AND F.id_factory = '{factory}'
					
									AND YEAR(F.date) = {year}
								
									AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
								
									AND F.value <> 0
									--AND A.account_type IN (1,2,3,4,5)
								
								GROUP BY 
									F.id_factory
									,MONTH(F.date)
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
							  	SELECT F.* 
							  	FROM XFC_PLT_FACT_Production F
					
								INNER JOIN productFilter PF
									ON F.id_product = PF.id_product
					
								WHERE
									(('{view}' = 'Periodic' AND MONTH(F.date) = {month})
									OR
									('{view}' = 'YTD' AND MONTH(F.date) <= {month}))
								
								AND YEAR(F.date) = {year}   
								AND F.scenario = '{scenario}'
								AND F.id_factory = '{factory}'
								AND F.activity_TAJ <> 0
							   )					
							
							, factProduction as (
							       
								SELECT
									F.id_factory
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ									

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
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							
							, activityProduct AS (
							
							      SELECT id_factory
										,month
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, month, id_product_gm, uotype 
							
							)							
					
							, factVTUCC AS (
																			
							SELECT 
								C.id_factory
								, DATEFROMPARTS(2025,C.month,1) AS Date
								, C.id_product
								, C.id_averagegroup
								, C.account_type
								, C.id_account
								, C.cost_center
								, C.cost_fixed
								, C.cost_variable
								, C.cost
								, V.volume
								, V.activity_TAJ AS activity_UO1
								, T.activity_TAJ AS activity_UO1_total				
								
							FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, id_account , cost_center
										, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
									FROM factCost
									WHERE 1=1
										--AND (id_account IN ({accountList}))
									GROUP BY id_factory, month, id_product, id_averagegroup, account_type, id_account , cost_center 
								) C
					
							LEFT JOIN factProduction V
							  	ON C.id_factory = V.id_factory
								AND C.month = V.month
								AND C.id_product = V.id_product
								AND C.id_averagegroup = V.id_averagegroup
								AND V.uotype = 'UO1'
							LEFT JOIN activityProduct AS T
							    ON V.id_factory = T.id_factory
								AND V.month = T.month
								AND V.id_product_gm = T.id_product_gm
								AND T.uotype = 'UO1'								
							)					
					
							"
							
				#End Region				
				
				#Region "FACT VTU Data ALL Factories All Months [Account Detail, GM]"
				
				' -----------------------------------------------------------------
				' Resultao: factVTU
				' -----------------------------------------------------------------
													
				ElseIf query = "VTU_data_All_Factories_All_Months_AccountDetail_GM"										
				
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")				
					
					month = Right("0" & month,2)
					Dim dateStart As String = year & "-01-01"
					Dim dateEnd As String = year & "-" & month & "-01"											
					
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", "", year, scenario, scenarioRef, time, "", "Existing", "", currency)	
			
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"	
					
					-- PREVIOUS

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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_averagegroup
					         		,A.account_type
									,F.id_account
									,C.nature
									,C.id as cost_center
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
								
									AND F.value <> 0
									--AND A.account_type IN (1,2,3,4,5)
								
								GROUP BY 
									F.id_factory
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,A.account_type
									,F.id_account
									,C.nature
									,C.id		
									--,R1.rate
							)
							
							, allMonths_Costs as (
								SELECT DISTINCT month FROM factCost								
							)
					
							, all_GM_by_Product_Costs as (
								SELECT DISTINCT id_factory , id_product , id_averagegroup , account_type , id_account , nature , cost_center  FROM factCost
							)

                            , all_Combinations_GM_Product_Month_Costs as (
                                SELECT M.month, GMP.id_factory , GMP.id_product , GMP.id_averagegroup , GMP.account_type , GMP.id_account , GMP.nature , GMP.cost_center                               
                                FROM allMonths_Costs M
                                CROSS JOIN all_GM_by_Product_Costs GMP
                            )

                            , factCostGM AS (
                                SELECT 
                                      A.id_factory
									, A.month
									, A.id_product
									, A.id_averagegroup
					         		, A.account_type
									, A.id_account
									, A.nature
									, A.cost_center
									, COALESCE(B.cost_fixed , 0) AS cost_fixed
									, COALESCE(B.cost_variable , 0) AS cost_variable
									, COALESCE(B.cost , 0) AS cost
                                FROM all_Combinations_GM_Product_Month_Costs A
                                LEFT JOIN factCost B
                                    ON A.id_factory = B.id_factory
                                    AND A.month = B.month
                                    AND A.id_product = B.id_product
                                    AND A.id_averagegroup = B.id_averagegroup
                                    AND A.account_type = B.account_type
                                    AND A.id_account = B.id_account
                                    AND A.nature = B.nature
                                    AND A.cost_center = B.cost_center
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
									,MONTH(F.date) AS month
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ									

						        FROM (
										SELECT id, nature 
										FROM XFC_PLT_MASTER_CostCenter_Hist 
										WHERE scenario = 'Actual' 
										AND type = 'A'
										AND DATEFROMPARTS({year},{month},1) BETWEEN start_date AND end_date
									) C     
						
					          	INNER JOIN factProductionFiltered  F 
					              	ON F.id_costcenter = C.id           
					
						        INNER JOIN (SELECT id_factory, id_product, id_averagegroup, uotype, MIN(id_costcenter) AS id_costcenter
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
									,MONTH(F.date)
									,F.id_product
									,F.id_averagegroup
									,F.uotype 
									,A.pond_suffix
							
							)
							, allMonthsProd AS (
								SELECT DISTINCT month FROM factProduction
							)
							, all_GM_by_Product AS (
								SELECT DISTINCT id_factory, id_product, id_averagegroup, id_product_gm, uotype FROM factProduction
							)
							, all_Combinations_GM_Product_Month AS (
								SELECT M.month , GMP.id_factory, GMP.id_product, GMP.id_averagegroup, GMP.id_product_gm , GMP.uotype
								FROM allMonthsProd M 
								CROSS JOIN all_GM_by_Product GMP
							)
							, factProductionGM AS (
								SELECT 
									A.month
									, A.id_factory
									, A.id_product
									, A.id_product_gm
									, A.id_averagegroup
									, A.uotype
									, COALESCE(B.volume, 0) as volume
									, COALESCE(B.activity_TAJ, 0) as activity_TAJ
						
								FROM all_Combinations_GM_Product_Month A
								
								LEFT JOIN factProduction B
									ON 	A.month = B.month
									AND A.id_factory = B.id_factory
									AND A.id_product = B.id_product
									AND A.id_averagegroup = B.id_averagegroup
									AND A.id_product_gm = B.id_product_gm
									AND A.uotype = B.uotype
							
							)
							, activityProduct AS (
							
							      SELECT id_factory
										,month
										,id_product_gm										
										,uotype 
							     		,SUM(activity_TAJ) AS activity_TAJ
							      FROM factProduction 
							      GROUP BY id_factory, month, id_product_gm, uotype 
							
							)
							
							, factVTU AS (
																			
								SELECT 
									C.id_factory
									, DATEFROMPARTS({year},C.month,1) AS Date
									, C.id_product
									, C.id_averagegroup
									-- , V.id_product_gm
									, C.account_type
									, C.id_account
									--, C.cost_center
									, C.cost_fixed
									, C.cost_variable
									, C.cost
									, V.volume
									, V.activity_TAJ AS activity_UO1
									, T.activity_TAJ AS activity_UO1_total				
									
								FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, id_account --, cost_center
											, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
										FROM factCostGM
										WHERE 1=1
										GROUP BY id_factory, month, id_product, id_averagegroup, account_type, id_account --, cost_center 
									) C
						
								LEFT JOIN factProductionGM V
								  	ON C.id_factory = V.id_factory
									AND C.month = V.month
									AND C.id_product = V.id_product
									AND C.id_averagegroup = V.id_averagegroup
									AND V.uotype = 'UO1'
								LEFT JOIN activityProduct AS T
								    ON V.id_factory = T.id_factory
									AND V.month = T.month
									AND V.id_product_gm = T.id_product_gm
									AND T.uotype = 'UO1'								
							)

							, factVTU_GM AS (
																			
								SELECT 
									C.id_factory
									, DATEFROMPARTS({year},C.month,1) AS Date
									, C.id_product
									, C.id_averagegroup
									, C.account_type
									, C.id_account
									-- , C.cost_center
									, C.cost_fixed
									, C.cost_variable
									, C.cost
									, V.volume
									, V.activity_TAJ AS activity_UO1
									, T.activity_TAJ AS activity_UO1_total				
									
								FROM ( 	SELECT id_factory, month, id_product, id_averagegroup, account_type, id_account -- , cost_center
											, SUM(cost_fixed) AS cost_fixed , SUM(cost_variable) AS cost_variable , SUM(cost) AS cost 
										FROM factCostGM
										WHERE 1=1
										GROUP BY id_factory, month, id_product, id_averagegroup, account_type, id_account -- , cost_center 
									) C
						
								LEFT JOIN factProductionGM V
								  	ON C.id_factory = V.id_factory
									AND C.month = V.month
									AND C.id_product = V.id_product
									AND C.id_averagegroup = V.id_averagegroup
									AND V.uotype = 'UO1'
					
								LEFT JOIN activityProduct AS T
								    ON V.id_factory = T.id_factory
									AND V.month = T.month
									AND V.id_product_gm = T.id_product_gm
									AND T.uotype = 'UO1'
				
								/*
									¡ATENCIÓN! -> Quitar este WHERE en 2026
								*/
					
								-- WHERE 1=1 
								-- 	AND ('{scenario}' <> 'RF06' AND '{scenario}' <> 'Actual')
								-- 	OR (
								-- 			'{scenario}' = 'RF06' 
								-- 			AND ISNULL(V.activity_TAJ <> 0
								-- 		)
								-- 	OR (
								-- 		'{scenario}' = 'Actual' 
								-- 		AND (
								-- 				( month <= 6 AND ISNULL(V.activity_TAJ <> 0 )
								-- 			OR
								-- 				month > 6
								-- 			)
								-- 		)						
							)					
					
							"
					
				#End Region
				
				#Region "VTU 12 Months"				
													
				ElseIf query = "VTU_12_Months"					
					
					Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
																  , $"AND id_product = '{product}'")
																  
					Dim sql_Rate = AllQueries(si, "RATE_2_Years", factory, "", year, scenario, scenarioRef, time, "", "", "", currency)	
					Dim sql_Product_Component_Recursivity = AllQueries(si, "Product_Component_Recursivity", factory, month, year, scenario, scenarioRef, time, "", type, product, currency)			
													
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
										,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
										,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
							
							{sql_Product_Component_Recursivity}
							
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
				
				#Region "VTU Data - AUX tables [All Months, All Factories, Product, Component, GM, Nature, Account] - NEW TABLES - Application"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_NewTables_Application"
					
					Dim originTable As String = If(vtu_version="Old", "XFC_PLT_FACT_Costs_VTU_Report_Account", "XFC_PLT_FACT_Costs_VTU_Final")
					originTable = If(currency="EUR", originTable, originTable & "_Local")					
												
					Dim factoryFilter As String = If(factory = "" Or factory = "All", "", $" AND id_factory = '{factory}' ")
						
					Dim monthF As String = If(month="13" Or month="", "12", month)
					Dim monthFilter As String = If(month="13" Or month="", "", $"AND month = {monthF}")					
					Dim rfMonth As String = If(scenario="RF06", "6", "9")					
					
					Dim accountFilter As String = String.Empty				
					Dim productFilterList As String = String.Empty	
					Dim productFilter As String = If(productFilterList=String.Empty,
							"",
							$"
									INNER JOIN 	(
										SELECT DISTINCT id_factory, id_component as id_product
										FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario
										UNION ALL
										SELECT DISTINCT id_factory, id_product_final as id_product
										FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario
								
									) PF
									ON PF.id_product = P.id_product
									AND PF.id_factory = P.id_factory
							")

					Dim impactParityWITH 	= If(impactParity="Yes", AllQueries(si, "ImpactParityCoefficient", factory, month, year, scenario, scenarioRef, time, "", "", "", currency), "")
					Dim impactParitySELECT 	= If(impactParity="Yes", "_ImpactParity","")
					Dim impactParityCOEF 	= If(impactParity="Yes", "*ISNULL(PC.coefficientIP,0)","")
					Dim impactParityJOIN   	= If(impactParity="Yes", " LEFT JOIN ImpactParityCoefficient PC ON  PC.id_factory = F.id_factory AND PC.month = F.month_start","")
					

					sql = $"
					
								WITH XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 1=1
										AND scenario = '{scenario}'
										AND year = '{year}'
										AND MONTH(date) <= {monthF}
										{productFilterList}
										{factoryFilter}
								)

								, Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date, 1 as month_start  
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date) as date, month_start + 1 as month_start
								    FROM Months
								    WHERE MONTH(date) < {monthF}
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account
								    FROM {originTable}
									WHERE scenario = '{scenario}'
										AND year = '{year}'
										AND MONTH(date) <= {monthF}
										{factoryFilter}
								)

								{impactParityWITH}

								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.month_start AS month_start
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										, ISNULL(F.cost_fixed,0){impactParityCOEF}  as cost_fixed
										, ISNULL(F.cost_variable,0){impactParityCOEF}  as cost_variable
										, ISNULL(F.cost,0){impactParityCOEF}  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total, 0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN {originTable} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										AND MONTH(F.date) = M.month_start
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'

									{impactParityJOIN}

									{productFilter}

					
									WHERE 1=1
										{accountFilter}
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario+impactParitySELECT}' AS scenario
								        , F.id_factory
										, F.month_start AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , F.id_product AS id_component
										, F.account_type
										, F.id_account
						
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_fixed AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_variable AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) 			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                  	 							ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  	OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, -- o.cost_center
								            v.variability, 
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(o.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28, 10))
								                         ELSE CAST(o.sum_fix_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(o.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(o.sum_var_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
										
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario+impactParitySELECT}' AS scenario
								        , P.id_factory AS id_factory
										, MONTH(P.date) AS month
										, F.id_averagegroup
								        , P.id_product_final AS id_product
										, P.id_product AS id_component_parents
								        , P.id_component AS id_component
										, F.account_type
										, F.id_account
										, P.exp_coefficient 
							
								        /* ---------- PERIÓDICOS ---------- */
							
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE  CAST(F.cost_fixed AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  CAST(F.cost_variable AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
						
								       	, SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))      		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY MONTH(P.date) ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								       	, SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY MONTH(P.date) ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								       	, SUM(CAST(F.volume AS DECIMAL(28,10)))         		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY MONTH(P.date) ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								  		, SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))   		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY MONTH(P.date) ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								      	, SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10))) 	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY MONTH(P.date) ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
											ON F.id_product = P.id_component					
								            AND F.id_factory = P.id_factory
											AND MONTH(P.date) = F.month_start
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(c.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_fix_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(c.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_var_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
								)
					"
				#End Region
								
				#Region "VTU Data - AUX tables [All Months, All Factories, Product, Component, GM, Nature, Account] - NEW TABLES - BiBlend"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_NewTables"

					Dim originTable As String = If(vtu_version="Old", "XFC_PLT_FACT_Costs_VTU_Report_Account", "XFC_PLT_FACT_Costs_VTU_Final")
					originTable = If(currency="EUR", originTable, originTable & "_Local")					
												
					Dim factoryFilter As String = If(factory = "" Or factory = "All", "", $" AND id_factory = '{factory}' ")
						
					Dim monthF As String = If(month="13" Or month="", "12", month)
					Dim monthFilter As String = If(month="13" Or month="", "", $"AND month = {monthF}")					
					Dim rfMonth As String = If(scenario="RF06", "6", "9")					
					
					Dim accountFilter As String = String.Empty				
					Dim productFilterList As String = String.Empty	
					Dim productFilter As String = If(productFilterList=String.Empty,
							"",
							$"
									INNER JOIN 	(
										SELECT DISTINCT id_factory, id_component as id_product
										FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario
										UNION ALL
										SELECT DISTINCT id_factory, id_product_final as id_product
										FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario
								
									) PF
									ON PF.id_product = P.id_product
									AND PF.id_factory = P.id_factory
							")

					Dim impactParityWITH 	= If(impactParity="Yes", AllQueries(si, "ImpactParityCoefficient", factory, month, year, scenario, scenarioRef, time, "", "", "", currency), "")
					Dim impactParitySELECT 	= If(impactParity="Yes", "_ImpactParity","")
					Dim impactParityCOEF 	= If(impactParity="Yes", "*ISNULL(PC.coefficientIP,0)","")
					Dim impactParityJOIN   	= If(impactParity="Yes", " LEFT JOIN ImpactParityCoefficient PC ON  PC.id_factory = F.id_factory AND PC.month = F.month_start","")
					

					sql = $"
					
								WITH XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 1=1
										AND scenario = '{scenario}'
										AND year = '{year}'
										AND month_start <= {monthF}
										{productFilterList}
										{factoryFilter}
								)

								, Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date, 1 as month_start  
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date) as date, month_start + 1 as month_start
								    FROM Months
								    WHERE MONTH(date) < {monthF}
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account
								    FROM {originTable}
									WHERE scenario = '{scenario}'
										AND year = '{year}'
										AND month_start <= {monthF}
										{factoryFilter}
								)

								{impactParityWITH}

								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.month_start AS month_start
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										, ISNULL(F.cost_fixed,0){impactParityCOEF}  as cost_fixed
										, ISNULL(F.cost_variable,0){impactParityCOEF}  as cost_variable
										, ISNULL(F.cost,0){impactParityCOEF}  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total, 0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN {originTable} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										AND F.month_start = M.month_start
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'

									{impactParityJOIN}

									{productFilter}

					
									WHERE 1=1
										{accountFilter}
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario+impactParitySELECT}' AS scenario
								        , F.id_factory
										, F.month_start AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , F.id_product AS id_component
										, F.account_type
										, F.id_account
						
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_fixed AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_variable AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) 			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                  	 							ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  	OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, -- o.cost_center
								            v.variability, 
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(o.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28, 10))
								                         ELSE CAST(o.sum_fix_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(o.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(o.sum_var_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
										
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario+impactParitySELECT}' AS scenario
								        , P.id_factory AS id_factory
										, P.month_start AS month
										, F.id_averagegroup
								        , P.id_product_final AS id_product
										, P.id_product AS id_component_parents
								        , P.id_component AS id_component
										, F.account_type
										, F.id_account
										, P.exp_coefficient 
							
								        /* ---------- PERIÓDICOS ---------- */
							
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE  CAST(F.cost_fixed AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  CAST(F.cost_variable AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
						
								       	, SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))      		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY P.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								       	, SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY P.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								       	, SUM(CAST(F.volume AS DECIMAL(28,10)))         		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY P.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								  		, SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))   		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY P.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								      	, SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10))) 	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account
								                                   							ORDER BY P.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
											ON F.id_product = P.id_component					
								            AND F.id_factory = P.id_factory
											AND P.month_start = F.month_start
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(c.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_fix_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(c.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_var_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
								)
					"
					
					' BRAPI.ErrorLog.LogMessage(si, sql)
				#End Region
				
				#Region "VTU Data - AUX tables [All Months, All Factories, Product, Component, GM, Nature, Account] - NEW TABLES - Activity - Application"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_NewTables_Activity_Application"
					
					Dim originTable As String = If(vtu_version="Old","XFC_PLT_FACT_Costs_VTU_Report_Account", "XFC_PLT_FACT_Costs_VTU_Final")
					originTable = If(currency="EUR", originTable, originTable & "_Local")					
					Dim monthF As String = If(month="13" Or month="", "12", month)
					Dim monthFilter As String = If(month="13" Or month="", "", $"AND month = {monthF}")					
					Dim rfMonth As String = If(scenario="RF06", "6", "9")					
					If factory = "" Then factory = "All"
						
					sql = $"
					
								WITH Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date)
								    FROM Months
								    WHERE MONTH(date) < {monthF}
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account --, cost_center
								    FROM {originTable}
									WHERE scenario = '{scenario}'
										AND year = '{year}'
										AND MONTH(date) <= {monthF}
										AND (id_factory = '{factory}' OR '{factory}' = 'All') 
								)
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.date AS date
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										--, P.cost_center AS cost_center
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total, 0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN {originTable} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										AND F.date = M.date
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
										--AND F.cost_center = P.cost_center
								)
								
								, XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 1=1
										AND scenario = '{scenario}'
										AND year = '{year}'
										AND MONTH(date) <= {monthF}
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , MONTH(F.date) AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , NULL AS id_component
										, F.account_type
										, F.id_account
										, F.activity_UO1 as activity_uo1
										, F.cost_fixed as cost_fixed_no_coef_per 
										, F.cost_variable as cost_variable_no_coef_per
					
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_fixed AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_variable AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) 			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                  	 							ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  	OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, -- o.cost_center
								            v.variability
											, o.activity_uo1 as activity_UO1_per, o.sum_uo1_ytd as activity_UO1_ytd
											, o.fixed_per as sum_fix_per, o.var_per as sum_var_per, o.sum_fix_ytd, o.sum_var_ytd
											, o.cost_fixed_no_coef_per , o.cost_variable_no_coef_per 
											, 1 as exp_coefficient, 0 as level,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(o.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28, 10))
								                         ELSE CAST(o.sum_fix_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(o.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(o.sum_var_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
										
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , P.id_factory AS id_factory
								        , MONTH(P.date) AS month
										, F.id_averagegroup
								        , P.id_product_final AS id_product
										, P.id_product AS id_component_parents
								        , P.id_component AS id_component
										, F.account_type
										, F.id_account
										, F.activity_UO1
										-- , F.cost_center
										, P.exp_coefficient
										, P.level
										, F.cost_fixed as cost_fixed_no_coef_per 
										, F.cost_variable as cost_variable_no_coef_per 
										
								        /* ---------- PERIÓDICOS ---------- */
							
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE  CAST(F.cost_fixed AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  CAST(F.cost_variable AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
						
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))      		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))         		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								  		, SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))   		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10))) 	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    -- LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								    --          ON  PI.id_product = P.id_component
								    --          AND PI.scenario   = '{scenario}'
								    --          AND PI.year       = '{year}'
								    --          AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
								            -- ON F.id_product = CASE WHEN MONTH(F.date)>={rfMonth}
								            --                       THEN ISNULL(PI.id_product_mapping, P.id_component)
								            --                       ELSE P.id_component END
											ON F.id_product = P.id_component
											
								            AND F.id_factory = P.id_factory
								            AND MONTH(P.date) = MONTH(F.date)
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account, -- c.cost_center,
								            v.variability
											, c.activity_uo1 as activity_UO1_per, c.sum_uo1_ytd as activity_uo1_ytd
											, c.fixed_per as sum_fix_per, c.var_per as sum_var_per, c.sum_fix_ytd, c.sum_var_ytd
											, c.cost_fixed_no_coef_per , c.cost_variable_no_coef_per 
											, c.exp_coefficient, c.level,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(c.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_fix_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(c.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_var_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
								)
					"
				#End Region		
				
				#Region "VTU Data - AUX tables [All Months, All Factories, Product, Component, GM, Nature, Account] - NEW TABLES - Activity - BiBlend"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_NewTables_Activity"
					
					Dim originTable As String = If(vtu_version="Old","XFC_PLT_FACT_Costs_VTU_Report_Account", "XFC_PLT_FACT_Costs_VTU_Final")
					originTable = If(currency="EUR", originTable, originTable & "_Local")										
																	
					Dim factoryFilter As String = If(factory = "" Or factory = "All", "", $" AND id_factory = '{factory}' ")
						
					Dim monthF As String = If(month="13" Or month="", "12", month)
					Dim monthFilter As String = If(month="13" Or month="", "", $"AND month = {monthF}")					
					Dim rfMonth As String = If(scenario="RF06", "6", "9")					
					
					Dim accountFilter As String = String.Empty				
					Dim productFilterList As String = String.Empty	
					Dim productFilter As String = If(productFilterList=String.Empty,
							"",
							$"
									INNER JOIN 	(
										SELECT DISTINCT id_factory, id_component as id_product
										FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario
										UNION ALL
										SELECT DISTINCT id_factory, id_product_final as id_product
										FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario
								
									) PF
									ON PF.id_product = P.id_product
									AND PF.id_factory = P.id_factory
							")	
						
					sql = $"
					
								WITH XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 1=1
										AND scenario = '{scenario}'
										AND year = '{year}'
										AND month_start <= {monthF}
										{productFilterList}
										{factoryFilter}
								)

                               ,  Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date, 1 as month_start  
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date) as date, month_start + 1 as month_start
								    FROM Months
								    WHERE MONTH(date) < {monthF}
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account
								    FROM {originTable}
									WHERE scenario = '{scenario}'
										AND year = '{year}'
										AND month_start <= {monthF}
										{factoryFilter}
								)
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.month_start AS month_start
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total, 0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN {originTable} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										AND F.month_start = M.month_start
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
					
									{productFilter}

					
									WHERE 1=1
										{accountFilter}
								)							

								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , F.month_start AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , NULL AS id_component
										, F.account_type
										, F.id_account
										, F.activity_UO1 as activity_uo1
										, F.cost_fixed as cost_fixed_no_coef_per 
										, F.cost_variable as cost_variable_no_coef_per
					
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_fixed AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_variable AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) 			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                  	 							ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  	OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   								ORDER BY F.month_start ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, -- o.cost_center
								            v.variability
											, o.activity_uo1 as activity_UO1_per, o.sum_uo1_ytd as activity_UO1_ytd
											, o.fixed_per as sum_fix_per, o.var_per as sum_var_per, o.sum_fix_ytd, o.sum_var_ytd
											, o.cost_fixed_no_coef_per , o.cost_variable_no_coef_per 
											, 1 as exp_coefficient, 0 as level,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(o.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28, 10))
								                         ELSE CAST(o.sum_fix_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(o.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(o.sum_var_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
										
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , P.id_factory AS id_factory
								        , P.month_start AS month
										, F.id_averagegroup
								        , P.id_product_final AS id_product
										, P.id_product AS id_component_parents
								        , P.id_component AS id_component
										, F.account_type
										, F.id_account
										, F.activity_UO1
										-- , F.cost_center
										, P.exp_coefficient
										, P.level
										, F.cost_fixed as cost_fixed_no_coef_per 
										, F.cost_variable as cost_variable_no_coef_per 
										
								        /* ---------- PERIÓDICOS ---------- */
							
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE  CAST(F.cost_fixed AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  CAST(F.cost_variable AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
						
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))      		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY P.month_start
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY P.month_start
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))         		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY P.month_start
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								  		, SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))   		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY P.month_start
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10))) 	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY P.month_start
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
											ON F.id_product = P.id_component											
								            AND F.id_factory = P.id_factory
                                            AND P.month_start = F.month_start
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account, -- c.cost_center,
								            v.variability
											, c.activity_uo1 as activity_UO1_per, c.sum_uo1_ytd as activity_uo1_ytd
											, c.fixed_per as sum_fix_per, c.var_per as sum_var_per, c.sum_fix_ytd, c.sum_var_ytd
											, c.cost_fixed_no_coef_per , c.cost_variable_no_coef_per 
											, c.exp_coefficient, c.level,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(c.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_fix_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(c.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_var_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
									WHERE 1=1
										{monthFilter}
								)
					"
				#End Region		
				
				#Region "OLD - VTU Data"
				
				#Region "OLD - VTU Data Monthly"				
													
				ElseIf query = "VTU_data_monthly"					
				
					Dim monthStart As String 
					Dim yearStart As String
					
					If month <> "12" Then
						yearStart = (CInt(year)-1).ToString
						monthStart = (CInt(month)+1).ToString
					Else
						yearStart = year
						monthStart = "01"
					End If
								
					Dim dateStart As String = yearStart & "-" & Right("0" & monthStart,2) & "-01"	
					Dim dateEnd As String = year & "-" & Right("0" & month,2) & "-01"																	  
										  
					Dim sql_Rate = AllQueries(si, "RATE_2_Years", "", "", year, scenario, scenarioRef, time, "", "", "", currency)	
					Dim sql_Product_Component_Recursivity = AllQueries(si, "Product_Component_Recursivity", factory, "", year, scenario, scenarioRef, time, "", type, product, currency)
									
					sql = sql_Rate.Replace(", fxRateRef ","WITH fxRateRef ") & $"		
					
					       , PercVarAccount AS (
					       
								SELECT DISTINCT id_factory, YEAR(V.date) AS year, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
							
								WHERE date BETWEEN '{dateStart}' AND '{dateEnd}'					
								AND scenario = '{scenario}'
								AND id_factory = '{factory}'
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
									,SUM(F.value * R1.rate * (100 - ISNULL(P.value,0))) / 100 AS cost_fixed
									,SUM(F.value * R1.rate * ISNULL(P.value,0)) / 100 AS cost_variable
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
									AND YEAR(F.date) = R1.year
									--AND F.id_factory = R1.id_factory
					
						       	LEFT JOIN PercVarAccount P
						           	ON F.id_account = P.id_account						          	
									AND F.id_factory = P.id_factory
									AND MONTH(F.date) = P.month
									AND YEAR(F.date) = P.year
								
								WHERE F.date BETWEEN '{dateStart}' AND '{dateEnd}'	
									AND F.id_factory = '{factory}'
									AND F.scenario = '{scenario}'					
									AND F.value <> 0
									AND F.id_product <> '-1'
								
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
									,DATEFROMPARTS(YEAR(F.date),MONTH(F.date),1) AS date
									,F.id_product
									,F.id_product + CASE WHEN A.pond_suffix IS NOT NULL THEN '_' + A.pond_suffix ELSE '' END AS id_product_gm
									,F.id_averagegroup
									,F.uotype 
									,SUM(F.volume) as volume
									,SUM(F.activity_TAJ) as activity_TAJ
									--,CASE WHEN SUM(F.volume) = 0 THEN 0  ELSE SUM(F.activity_TAJ) / SUM(F.volume) END AS allocation_TAJ

						        FROM (
										SELECT DISTINCT id, nature 
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
								
								WHERE DATEFROMPARTS(YEAR(F.date),MONTH(F.date),1) BETWEEN '{dateStart}' AND '{dateEnd}'	
									AND F.id_factory = '{factory}'
									AND F.scenario = '{scenario}'
									AND activity_TAJ <> 0
								
								GROUP BY
									F.id_factory
									,DATEFROMPARTS(YEAR(F.date),MONTH(F.date),1)
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
							
							{sql_Product_Component_Recursivity}
							
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
				
				#Region "OLD - VTU Data - AUX tables [All Months, All Factories, Product, Component, GM, Nature]"
				
				ElseIf query = "VTU_data_aux_tables"
					Dim table_FactVTU As String = If(currency="EUR","XFC_PLT_FACT_Costs_VTU_Report","XFC_PLT_FACT_Costs_VTU_Report_Local")
					Dim factoryFilter As String = If(factory="" Or factory = "All", "", $"AND F.id_factory = '{factory}'")
					sql = $"
					
								WITH Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date)
								    FROM Months
								    WHERE MONTH(date) < 12
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type
								    FROM {table_FactVTU} F
									WHERE 1=1
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
										{factoryFilter}
								)
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.date AS date
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total,0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN {table_FactVTU} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.date = M.date
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
										{factoryFilter}
								)
								
								, XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report F
									WHERE 
										F.scenario = '{scenario}'
										AND F.year = '{year}'
										{factoryFilter}
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario,
								        F.id_factory,
								        MONTH(F.date) AS month,
										F.id_averagegroup,
								        F.id_product,								        
										F.id_product AS id_component_parents,
								        NULL AS id_component,
										F.account_type,
										1 AS level,
										1 AS exp_coefficient,
								
								        /* ---------- PERIÓDICOS ---------- */
							
										F.activity_UO1 AS activity_UO1_per,
										F.cost_fixed AS sum_fix_per,										
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE F.cost_fixed / F.volume * F.activity_UO1 / F.activity_UO1_total
								        END AS fixed_per,
										F.cost_variable AS sum_var_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE F.cost_variable / F.volume * F.activity_UO1/F.activity_UO1_total
								        END AS var_per,
								
								        /* ---------- ACUMULADOS ---------- */
							
										SUM(F.activity_UO1)    		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type
								                                   		ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS activity_UO1_ytd,					
								        SUM(F.cost_fixed)    		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type
								                                   		ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd,
								        SUM(F.cost_variable) 		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type
								                                   		ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd,
								        SUM(F.volume)        		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type
								                                   		ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd,
								        SUM(F.activity_UO1)  		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type
								                                   		ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd,
								        SUM(F.activity_UO1_total) 	OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type
								                                   		ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type,
								            v.variability, level, exp_coefficient, activity_UO1_per, activity_UO1_ytd,
											sum_fix_per, sum_var_per, sum_fix_ytd, sum_var_ytd,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
											CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic_Unit,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD,
											CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD_Unit
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', o.fixed_per),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE o.sum_fix_ytd/o.sum_vol_ytd * o.sum_uo1_ytd/o.sum_uo1t_ytd END),
								        ('V','PER', o.var_per),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE o.sum_var_ytd/o.sum_vol_ytd * o.sum_uo1_ytd/o.sum_uo1t_ytd END)
								    ) v(variability, metric, value)
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										F.id_averagegroup,
								        P.id_product_final AS id_product,
										P.id_product AS id_component_parents,
								        P.id_component AS id_component,
										F.account_type,
										P.level,
										P.exp_coefficient, 
							
								        /* ---------- PERIÓDICOS ---------- */
							
										F.activity_UO1 AS activity_UO1_per,
										F.cost_fixed AS sum_fix_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_fixed   /F.volume * F.activity_UO1/F.activity_UO1_total * P.exp_coefficient
								        END                                     AS fixed_per,
										CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_fixed   /F.volume * F.activity_UO1/F.activity_UO1_total
								        END                                     AS fixed_per_unit,
										F.cost_variable AS sum_var_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_variable/F.volume * F.activity_UO1/F.activity_UO1_total * P.exp_coefficient
								        END                                     AS var_per,
										CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_variable/F.volume * F.activity_UO1/F.activity_UO1_total
								        END                                     AS var_per_unit,
								        /* ---------- ACUMULADOS ---------- */
						
										SUM(F.activity_UO1)    		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS activity_UO1_ytd,										
								        SUM(F.cost_fixed)     		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd,
								        SUM(F.cost_variable) 		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd,
								        SUM(F.volume)        		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd,
								        SUM(F.activity_UO1)  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd,
								        SUM(F.activity_UO1_total) 	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								             ON  PI.id_product = P.id_component
								             AND PI.scenario   = '{scenario}'
								             AND PI.year       = '{year}'
								             AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
								             ON F.id_product = CASE WHEN MONTH(F.date)>=6
								                                   THEN ISNULL(PI.id_product_mapping, P.id_component)
								                                   ELSE P.id_component END
								            AND F.id_factory = P.id_factory
								            AND MONTH(P.date) = MONTH(F.date)
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type,
								            v.variability, level, exp_coefficient, activity_UO1_per, activity_UO1_ytd,
											sum_fix_per, sum_var_per, sum_fix_ytd, sum_var_ytd,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
											CASE WHEN v.metric='PER' THEN v.value2 ELSE 0 END AS VTU_Periodic_Unit,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD,
											CASE WHEN v.metric='YTD' THEN v.value2 ELSE 0 END AS VTU_YTD_Unit
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', c.fixed_per, c.fixed_per_unit),
								        ('F','YTD'
												, CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0 THEN 0
								                       ELSE c.sum_fix_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd * c.exp_coefficient END 
												, CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0 THEN 0
								                       ELSE c.sum_fix_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd END),
								        ('V','PER', c.var_per, c.var_per_unit),
								        ('V','YTD'
												, CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE c.sum_var_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd * c.exp_coefficient END
												, CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE c.sum_var_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd END)
								    ) v(variability, metric, value, value2)
								)
					"

				#End Region
				
				#Region "OLD - VTU Data - AUX tables [All Months, All Factories, Product, Component, GM, Nature, Account]"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables"
					sql = $"
					
								WITH Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date)
								    FROM Months
								    WHERE MONTH(date) < 12
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account --, cost_center
								    FROM XFC_PLT_FACT_Costs_VTU_Report_{factory}
									WHERE scenario = '{scenario}'
										AND year = '{year}'
								)
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.date AS date
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										--, P.cost_center AS cost_center
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total, 0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_{factory} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										--AND F.cost_center = P.cost_center
										AND F.date = M.date
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
								)
								
								, XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 
										scenario = '{scenario}'
										AND year = '{year}'
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , MONTH(F.date) AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , NULL AS id_component
										, F.account_type
										, F.id_account
										--, F.cost_center
						
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_fixed AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_variable AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, -- o.cost_center
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(o.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28, 10))
								                         ELSE CAST(o.sum_fix_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(o.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(o.sum_var_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										F.id_averagegroup,
								        P.id_product_final AS id_product,
										P.id_product AS id_component_parents,
								        P.id_component AS id_component,
										F.account_type,
										F.id_account, 
										--F.cost_center,
										P.exp_coefficient, 
							
								        /* ---------- PERIÓDICOS ---------- */
							
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE  CAST(F.cost_fixed AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS fixed_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  CAST(F.cost_variable AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS var_per,
								
								        /* ---------- ACUMULADOS ---------- */
						
								        SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))      		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd,
								       SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd,
								       SUM(CAST(F.volume AS DECIMAL(28,10)))         		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd,
								  		SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))   		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd,
								       SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								             ON  PI.id_product = P.id_component
								             AND PI.scenario   = '{scenario}'
								             AND PI.year       = '{year}'
								             AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
								             ON F.id_product = CASE WHEN MONTH(F.date)>=6
								                                   THEN ISNULL(PI.id_product_mapping, P.id_component)
								                                   ELSE P.id_component END
								            AND F.id_factory = P.id_factory
								            AND MONTH(P.date) = MONTH(F.date)
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account, -- c.cost_center,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(c.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_fix_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(c.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_var_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
								)
					"
				#End Region
														
				#Region "OLD - VTU Data - AUX tables [All Months, All Factories, Product, Component, GM, Nature, Account, CostCenter] "
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_cc"
					sql = $"
					
								WITH Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date)
								    FROM Months
								    WHERE MONTH(date) < 12
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account --, cost_center
								    FROM XFC_PLT_FACT_Costs_VTU_Report_{factory}
									
									WHERE scenario = '{scenario}'
									AND year = '{year}'
								)
					
								,filterProduct AS (							
									SELECT DISTINCT id_factory, id_component
									FROM XFC_PLT_HIER_Nomenclature_Date_Report 
									
									WHERE scenario = '{scenario}' 
									AND year = '{year}' 
									AND id_factory = '{factory}' 
									AND id_product_final = '{product}'
									AND MONTH(date) = {month}
								
									UNION ALL
								
									SELECT '{factory}' AS id_factory, '{product}' AS id_component
								)					
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.date AS date
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										--, P.cost_center AS cost_center
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total, 0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
					
									INNER JOIN filterProduct  R
										ON  P.id_factory = R.id_factory
										AND P.id_product = R.id_component  
							
									LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_{factory} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										AND F.date = M.date
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
								)
								
								, XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 
										scenario = '{scenario}'
										AND year = '{year}'
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , MONTH(F.date) AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , F.id_product AS id_component
										, F.account_type
										, F.id_account
										--, F.cost_center
						
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_fixed AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_variable AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, -- o.cost_center
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(o.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28, 10))
								                         ELSE CAST(o.sum_fix_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(o.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(o.sum_var_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										F.id_averagegroup,
								        P.id_product_final AS id_product,
										P.id_product AS id_component_parents,
								        P.id_component AS id_component,
										F.account_type,
										F.id_account, 
										--F.cost_center,
										P.exp_coefficient, 
							
								        /* ---------- PERIÓDICOS ---------- */
							
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE  CAST(F.cost_fixed AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS fixed_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  CAST(F.cost_variable AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS var_per,
								
								        /* ---------- ACUMULADOS ---------- */
						
								        SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))      		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd,
								       SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd,
								       SUM(CAST(F.volume AS DECIMAL(28,10)))         		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd,
								  		SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))   		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd,
								       SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10))) 	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								             ON  PI.id_product = P.id_component
								             AND PI.scenario   = '{scenario}'
								             AND PI.year       = '{year}'
								             AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
								             ON F.id_product = CASE WHEN MONTH(F.date)>=6
								                                   THEN ISNULL(PI.id_product_mapping, P.id_component)
								                                   ELSE P.id_component END
								            AND F.id_factory = P.id_factory
								            AND MONTH(P.date) = MONTH(F.date)
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account, -- c.cost_center,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(c.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_fix_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(c.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_var_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
								)
					"
				#End Region
				
				#End Region
				
				#Region "VTU Data - Cost Detail"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_CostDetail"
					sql = $"
					
								,Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date)
								    FROM Months
								    WHERE MONTH(date) < 12
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account , cost_center
								    FROM factVTUCC
								)					
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.date AS date
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										, P.cost_center AS cost_center
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total,0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN factVTUCC F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										AND F.cost_center = P.cost_center
										AND F.date = M.date		
					
								)			
								
								, XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 
										scenario = '{scenario}'
										AND year = '{year}'
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , MONTH(F.date) AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , NULL AS id_component
										, F.account_type
										, F.id_account
										, F.cost_center
						
								        /* ---------- PERIÓDICOS ---------- */
					
										, CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_fixed AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE CAST(F.cost_variable AS DECIMAL(28,10)) /CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10))
								         END                                        AS var_per
							
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))     		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))         		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1  AS DECIMAL(28,10))) 			OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  	OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   								ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, o.cost_center,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', CAST(o.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28, 10))
								                         ELSE CAST(o.sum_fix_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(o.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(o.sum_var_ytd AS DECIMAL(28,10))/CAST(o.sum_vol_ytd AS DECIMAL(28,10)) * CAST(o.sum_uo1_ytd AS DECIMAL(28,10))/CAST(o.sum_uo1t_ytd AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										F.id_averagegroup,
								        P.id_product_final AS id_product,
										P.id_product AS id_component_parents,
								        P.id_component AS id_component,
										F.account_type,
										F.id_account, 
										F.cost_center,
										P.exp_coefficient, 
							
								        /* ---------- PERIÓDICOS ---------- */
							
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN CAST(0 AS DECIMAL(28,10))
								             ELSE  CAST(F.cost_fixed AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS fixed_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  CAST(F.cost_variable AS DECIMAL(28,10))/CAST(F.volume AS DECIMAL(28,10)) * CAST(F.activity_UO1 AS DECIMAL(28,10))/CAST(F.activity_UO1_total AS DECIMAL(28,10)) * CAST(P.exp_coefficient AS DECIMAL(28,10))
								        END                                     AS var_per,
								
								        /* ---------- ACUMULADOS ---------- */
						
								        SUM(CAST(F.cost_fixed AS DECIMAL(28, 10)))      	OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account , F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd,
								        SUM(CAST(F.cost_variable AS	DECIMAL(28, 10)))  		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account  , F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd,
								        SUM(CAST(F.volume  AS DECIMAL(28, 10)))        		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account  , F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd,
								        SUM(CAST(F.activity_UO1 AS DECIMAL(28, 10)))   		OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account  , F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd,
								        SUM(CAST(F.activity_UO1_total AS DECIMAL(28, 10)))  OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_product, P.id_component, F.account_type, F.id_account , F.cost_center
								                                   							ORDER BY MONTH(P.date)
								                                   							ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								             ON  PI.id_product = P.id_component
								             AND PI.scenario   = '{scenario}'
								             AND PI.year       = '{year}'
								             AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
								             ON F.id_product = CASE WHEN MONTH(F.date)>=6
								                                   THEN ISNULL(PI.id_product_mapping, P.id_component)
								                                   ELSE P.id_component END
								            AND F.id_factory = P.id_factory
								            AND MONTH(P.date) = MONTH(F.date)
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account, c.cost_center,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
									CROSS APPLY (VALUES
								        ('F','PER', CAST(c.fixed_per AS DECIMAL(28,10))),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_fix_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END),
								        ('V','PER', CAST(c.var_per AS DECIMAL(28,10))),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN CAST(0 AS DECIMAL(28,10))
								                         ELSE CAST(c.sum_var_ytd AS DECIMAL(28,10))/CAST(c.sum_vol_ytd AS DECIMAL(28,10)) * CAST(c.sum_uo1_ytd AS DECIMAL(28,10))/CAST(c.sum_uo1t_ytd AS DECIMAL(28,10)) * CAST(c.exp_coefficient AS DECIMAL(28,10)) END)
								    ) v(variability, metric, value)
								)
					"
				#End Region		
				
				#Region "VTU Data - Stored"
				
				ElseIf query = "VTU_data_stored"
					
					Dim originTable As String = String.Empty
					Dim vtu_vesion As String = "Old"
					
					originTable = "XFC_PLT_FACT_Costs_VTU_Report_Test"
					
					sql = $"
					
								-- Nomenclature
					
								WITH XFC_PLT_HIER_Nomenclature_Date_Report_Scenario AS (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 
										scenario = '{scenario}'
										AND year = '{year}'
								)
					
								-- VTU Data
					
								, XFC_PLT_FACT_Costs_VTU_Report_Account_Scenario AS ( 									
					
									SELECT * 
									FROM {originTable} P								
									WHERE P.scenario = '{scenario}'
									AND P.year = '{year}'
								)
					
								-- VTU Data With Nomenclature
					
								, VTU_Data AS (
					
									-- Components
							
									SELECT '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										F.id_averagegroup,
								        P.id_product_final AS id_product,
										P.id_product AS id_component_parents,
								        P.id_component AS id_component,
										F.account_type,
										F.id_account, 
										F.VTU_unit_per_fix * P.exp_coefficient AS VTU_unit_per_fix,
										F.VTU_unit_per_var * P.exp_coefficient AS VTU_unit_per_var,
										F.VTU_unit_ytd_fix * P.exp_coefficient AS VTU_unit_ytd_fix,
										F.VTU_unit_ytd_var * P.exp_coefficient AS VTU_unit_ytd_var							
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								             ON  PI.id_product = P.id_component
								             AND PI.scenario   = '{scenario}'
								             AND PI.year       = '{year}'
								             AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_Account_Scenario F
								             ON F.id_product = CASE WHEN MONTH(F.date)>=6
								                                   THEN ISNULL(PI.id_product_mapping, P.id_component)
								                                   ELSE P.id_component END
								             AND F.id_factory = P.id_factory
								             AND MONTH(P.date) = MONTH(F.date)	
					
									WHERE VTU_unit_ytd_fix + 1 <> 1
									OR VTU_unit_ytd_var + 1 <> 1
							
									UNION ALL
							
									-- Own
							
									SELECT '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										P.id_averagegroup,
								        P.id_product AS id_product,
										P.id_product AS id_component_parents,
								        P.id_product AS id_component,
										P.account_type,
										P.id_account, 
										P.VTU_unit_per_fix,
										P.VTU_unit_per_var,
										P.VTU_unit_ytd_fix,
										P.VTU_unit_ytd_var			
					
									FROM XFC_PLT_FACT_Costs_VTU_Report_Account_Scenario P
					
									WHERE VTU_unit_ytd_fix + 1 <> 1
									OR VTU_unit_ytd_var + 1 <> 1					
							
								)
					"
				#End Region				
				
				#Region "VTU Data - Account - BackUp"
				
					#Region "VTU Data - All Months, All Factories, Product, Component, GM, Nature, Account - AUX tables"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_BACKUP"
					sql = $"
					
								WITH Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date)
								    FROM Months
								    WHERE MONTH(date) < 12
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account --, cost_center
								    FROM XFC_PLT_FACT_Costs_VTU_Report_{factory}
									WHERE scenario = '{scenario}'
										AND year = '{year}'
								)
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.date AS date
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										--, P.cost_center AS cost_center
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total,0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_{factory} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										--AND F.cost_center = P.cost_center
										AND F.date = M.date
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
								)
								
								, XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 
										scenario = '{scenario}'
										AND year = '{year}'
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , MONTH(F.date) AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , NULL AS id_component
										, F.account_type
										, F.id_account
										--, F.cost_center
						
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE F.cost_fixed    /F.volume * F.activity_UO1/F.activity_UO1_total
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE F.cost_variable /F.volume * F.activity_UO1/F.activity_UO1_total
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, -- o.cost_center
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', o.fixed_per),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE o.sum_fix_ytd/o.sum_vol_ytd * o.sum_uo1_ytd/o.sum_uo1t_ytd END),
								        ('V','PER', o.var_per),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE o.sum_var_ytd/o.sum_vol_ytd * o.sum_uo1_ytd/o.sum_uo1t_ytd END)
								    ) v(variability, metric, value)
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										F.id_averagegroup,
								        P.id_product_final AS id_product,
										P.id_product AS id_component_parents,
								        P.id_component AS id_component,
										F.account_type,
										F.id_account, 
										--F.cost_center,
										P.exp_coefficient, 
							
								        /* ---------- PERIÓDICOS ---------- */
							
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_fixed   /F.volume * F.activity_UO1/F.activity_UO1_total * P.exp_coefficient
								        END                                     AS fixed_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_variable/F.volume * F.activity_UO1/F.activity_UO1_total * P.exp_coefficient
								        END                                     AS var_per,
								
								        /* ---------- ACUMULADOS ---------- */
						
								        SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))      OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd,
								       SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd,
								       SUM(CAST(F.volume AS DECIMAL(28,10)))         OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd,
								  		SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))   OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd,
								       SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10))) OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account--, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								             ON  PI.id_product = P.id_component
								             AND PI.scenario   = '{scenario}'
								             AND PI.year       = '{year}'
								             AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
								             ON F.id_product = CASE WHEN MONTH(F.date)>=6
								                                   THEN ISNULL(PI.id_product_mapping, P.id_component)
								                                   ELSE P.id_component END
								            AND F.id_factory = P.id_factory
								            AND MONTH(P.date) = MONTH(F.date)
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account, -- c.cost_center,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', c.fixed_per),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE c.sum_fix_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd * c.exp_coefficient END),
								        ('V','PER', c.var_per),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE c.sum_var_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd * c.exp_coefficient END)
								    ) v(variability, metric, value)
								)
					"
				#End Region
				
					#Region "VTU Data - All Months, All Factories, Product, Component, GM, Nature, Account, CC - AUX tables"
				
				ElseIf query = "VTU_data_accountDetail_aux_tables_CC_BACKUP"
					sql = $"
					
								WITH Months AS (
								    SELECT DATEFROMPARTS({year}, 1, 1) AS date
								    UNION ALL
								    SELECT DATEADD(MONTH, 1, date)
								    FROM Months
								    WHERE MONTH(date) < 12
								)
							
								,Products AS (
								    SELECT DISTINCT id_factory, id_product, id_averagegroup, account_type, id_account , cost_center
								    FROM XFC_PLT_FACT_Costs_VTU_Report_{factory}
									WHERE scenario = '{scenario}'
										AND year = '{year}'
								)
							
								,XFC_PLT_FACT_Costs_VTU_Report_All_Months AS (
								    SELECT 
										  P.id_factory
										, M.date AS date
										, P.id_product as id_product
										, P.id_averagegroup as id_averagegroup
										, P.account_type as account_type
										, P.id_account as id_account
										, P.cost_center AS cost_center
										, ISNULL(F.cost_fixed,0)  as cost_fixed
										, ISNULL(F.cost_variable,0)  as cost_variable
										, ISNULL(F.cost,0)  as cost
										, ISNULL(F.volume,0)  as volume
										, ISNULL(F.activity_UO1,0)  as activity_UO1
										, ISNULL(F.activity_UO1_total,0)  as activity_UO1_total
							
								    FROM Products P
								    CROSS JOIN Months M
							
									LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_{factory} F
										ON  F.id_factory = P.id_factory
										AND F.id_product = P.id_product
										AND F.id_averagegroup = P.id_averagegroup
										AND F.account_type = P.account_type
										AND F.id_account = P.id_account
										AND F.cost_center = P.cost_center
										AND F.date = M.date
										AND F.scenario = '{scenario}'
										AND F.year = '{year}'
								)
								
								, XFC_PLT_HIER_Nomenclature_Date_Report_Scenario as (
									SELECT *
									FROM XFC_PLT_HIER_Nomenclature_Date_Report
									WHERE 
										scenario = '{scenario}'
										AND year = '{year}'
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , MONTH(F.date) AS month
										, F.id_averagegroup
								        , F.id_product								        
										, F.id_product AS id_component_parents
								        , NULL AS id_component
										, F.account_type
										, F.id_account
										, F.cost_center
						
								        /* ---------- PERIÓDICOS ---------- */
							
								       , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE F.cost_fixed    /F.volume * F.activity_UO1/F.activity_UO1_total
								         END                                        AS fixed_per
								        , CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE F.cost_variable /F.volume * F.activity_UO1/F.activity_UO1_total
								         END                                        AS var_per
								
								        /* ---------- ACUMULADOS ---------- */
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))     OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))         OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1  AS DECIMAL(28,10))) OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10)))  OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F

								)
								
								,OwnRows AS (
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup,
								            o.id_product, o.id_component_parents, o.id_component, o.account_type, o.id_account, o.cost_center,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
								    FROM OwnBase o
								    CROSS APPLY (VALUES
								        ('F','PER', o.fixed_per),
								        ('F','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE o.sum_fix_ytd/o.sum_vol_ytd * o.sum_uo1_ytd/o.sum_uo1t_ytd END),
								        ('V','PER', o.var_per),
								        ('V','YTD', CASE WHEN o.sum_vol_ytd=0 OR o.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE o.sum_var_ytd/o.sum_vol_ytd * o.sum_uo1_ytd/o.sum_uo1t_ytd END)
								    ) v(variability, metric, value)
								)
								
								
								,CompBase AS (
								    SELECT
								        '{scenario}' AS scenario,
								        P.id_factory AS id_factory,
								        MONTH(P.date) AS month,
										F.id_averagegroup,
								        P.id_product_final AS id_product,
										P.id_product AS id_component_parents,
								        P.id_component AS id_component,
										F.account_type,
										F.id_account, 
										F.cost_center,
										P.exp_coefficient, 
							
								        /* ---------- PERIÓDICOS ---------- */
							
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_fixed   /F.volume * F.activity_UO1/F.activity_UO1_total * P.exp_coefficient
								        END                                     AS fixed_per,
								        CASE WHEN F.volume=0 OR F.activity_UO1=0
								             THEN 0
								             ELSE  F.cost_variable/F.volume * F.activity_UO1/F.activity_UO1_total * P.exp_coefficient
								        END                                     AS var_per,
								
								        /* ---------- ACUMULADOS ---------- */
						
								        SUM(CAST(F.cost_fixed AS DECIMAL(28, 10)))      OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd,
								        SUM(CAST(F.cost_variable AS DECIMAL(28, 10)))  OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd,
								        SUM(CAST(F.volume  AS DECIMAL(28, 10)))        OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd,
								        SUM(CAST(F.activity_UO1 AS DECIMAL(28, 10)))   OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd,
								        SUM(CAST(F.activity_UO1_total AS DECIMAL(28, 10)))  OVER (PARTITION BY P.id_factory, F.id_averagegroup, P.id_product_final, P.id_component, F.account_type, F.id_account, F.cost_center
								                                   ORDER BY MONTH(P.date)
								                                   ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
							
									FROM XFC_PLT_HIER_Nomenclature_Date_Report_Scenario P							
							
								    LEFT JOIN XFC_PLT_AUX_Product_Pivot PI
								             ON  PI.id_product = P.id_component
								             AND PI.scenario   = '{scenario}'
								             AND PI.year       = '{year}'
								             AND PI.id_factory = P.id_factory
							
								    LEFT JOIN XFC_PLT_FACT_Costs_VTU_Report_All_Months F
								             ON F.id_product = CASE WHEN MONTH(F.date)>=6
								                                   THEN ISNULL(PI.id_product_mapping, P.id_component)
								                                   ELSE P.id_component END
								            AND F.id_factory = P.id_factory
								            AND MONTH(P.date) = MONTH(F.date)
							
								)
								
								,CompRows AS (
							
								    SELECT  c.scenario, c.id_factory, c.month, c.id_averagegroup, 
								            c.id_product, c.id_component_parents, c.id_component, c.account_type, c.id_account, c.cost_center,
								            v.variability,
								            CASE WHEN v.metric='PER' THEN v.value ELSE 0 END AS VTU_Periodic,
								            CASE WHEN v.metric='YTD' THEN v.value ELSE 0 END AS VTU_YTD
							
								    FROM CompBase c
								    CROSS APPLY (VALUES
								        ('F','PER', c.fixed_per),
								        ('F','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE c.sum_fix_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd * c.exp_coefficient END),
								        ('V','PER', c.var_per),
								        ('V','YTD', CASE WHEN c.sum_vol_ytd=0 OR c.sum_uo1_ytd=0
								                         THEN 0
								                         ELSE c.sum_var_ytd/c.sum_vol_ytd * c.sum_uo1_ytd/c.sum_uo1t_ytd * c.exp_coefficient END)
								    ) v(variability, metric, value)
								)
					"
				#End Region
				
				#End Region
				
				#Region "Product Component Recursivity"				
													
				ElseIf query = "Product_Component_Recursivity"					
												
					Dim day As String = String.Empty					
					If CInt(month) = 2 Then 
						day = "28"
					Else If CInt(month) <= 6 Then
						day = If((CInt(month) Mod 2) = 0, "30", "31")
					Else
						day = If((CInt(month) Mod 2) = 0, "31", "30")
					End If	
					
'					BRApi.ErrorLog.LogMessage(si, "type: " & type)
'					Dim type As String = "Existing"
					If (type = "Existing")		
'						BRApi.ErrorLog.LogMessage(si, "Product: " & product)
'						Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
'																  	  , $"AND id_product = '{product}'")							
																	  
						Dim sFilterProduct As String = If(product = "", $"" _ 
																  	  , $"AND id_product = '{product}'")
																	  
						Dim sFilterFactory As String = If(factory = "", $"" _ 
																  	  , $"AND N.id_factory = '{factory}'")																	  
					
						sql = $"		
												
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
									{sFilterFactory}		
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
								WHERE '{year}-{month}-{day}' BETWEEN N.start_date AND N.end_date
									{sFilterFactory}
							)
					
							"
					Else
						
						Dim sFilterProduct As String = If(product = "", "", $" AND id_product_final = '{product}'")								
						
						sql = $"
						
						    , Distinct_Nomenclature as (
						
								SELECT 
									id_factory
									, id_product_final
									, id_product
									, id_component
									, AVG(coefficient) as coefficient
					
								FROM XFC_PLT_AUX_NewProducts_Simulation
					
								WHERE 1=1
									AND scenario = '{scenario}'	
									AND year = {year}	
									AND id_factory = '{factory}'
									AND (id_product_final <> id_component)	
									{sFilterProduct}
					
								GROUP BY 
									id_factory
									, id_product_final
									, id_product
									, id_component
							)						
						
							, Product_Component_Recursivity as (							
									
								SELECT
					         		N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								FROM Distinct_Nomenclature N
								WHERE 1=1										
					
								UNION ALL
								
								SELECT
									N.id_factory
									, R.id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								FROM Distinct_Nomenclature N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product_final = R.id_product_final
									AND N.id_product = R.Id_component											
									AND N.id_factory = R.id_factory		
							)
						
							"
						
					End If
							
				#End Region		
				
				#Region "Product Component Recursivity All Months"				
													
				ElseIf query = "Product_Component_Recursivity_All_Months"																	
					
'					BRApi.ErrorLog.LogMessage(si, "type: " & type)
'					Dim type As String = "Existing"
					If (type = "Existing")		
'						BRApi.ErrorLog.LogMessage(si, "Product: " & product)
'						Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
'																  	  , $"AND id_product = '{product}'")							
																	  
						Dim sFilterProduct As String = If(product = "", $"" _ 
																  	  , $"AND id_product = '{product}'")
																	  
						Dim sFilterFactory As String = If(factory = "", $"" _ 
																  	  , $"AND N.id_factory = '{factory}'")																	  
					
						sql = $"		
												
							,Months AS (
							    SELECT EOMONTH(DATEFROMPARTS({year}, 1, 1)) AS target_date
							    UNION ALL
							    SELECT EOMONTH(DATEADD(MONTH, 1, target_date))
							    FROM Months
							    WHERE MONTH(target_date) < 12
							)
						
							,Product_Component_Recursivity AS (
							    SELECT 
							        M.target_date,
							        N.id_factory,
							        N.id_product AS id_product_final,
							        N.id_product,
							        N.id_component,
							        CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
							        CAST(N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
							        2 AS Level
							    FROM Months M
							    INNER JOIN XFC_PLT_HIER_Nomenclature_Date N
							        ON M.target_date BETWEEN N.start_date AND N.end_date
							        {sFilterFactory}
							        {sFilterProduct}
							
							    UNION ALL
							
							    SELECT 
							        M.target_date,
							        N.id_factory,
							        R.id_product_final,
							        N.id_product,
							        N.id_component,
							        CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
							        CAST(R.exp_coefficient * N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
							        R.Level + 1
							    FROM Months M
							    INNER JOIN XFC_PLT_HIER_Nomenclature_Date N
							        ON M.target_date BETWEEN N.start_date AND N.end_date
							    INNER JOIN Product_Component_Recursivity R
							        ON N.id_product = R.id_component
							        AND N.id_factory = R.id_factory
							        AND M.target_date = R.target_date
							        {sFilterFactory}
							)
					
							"
					Else
						
						Dim sFilterProduct As String = If(product = "", "", $" AND id_product_final = '{product}'")								
						
						sql = $"
						
						    , Distinct_Nomenclature as (
						
								SELECT 
									id_factory
									, id_product_final
									, id_product
									, id_component
									, AVG(coefficient) as coefficient
					
								FROM XFC_PLT_AUX_NewProducts_Simulation
					
								WHERE 1=1
									AND scenario = '{scenario}'	
									AND year = {year}	
									AND id_factory = '{factory}'
									AND (id_product_final <> id_component)	
									{sFilterProduct}
					
								GROUP BY 
									id_factory
									, id_product_final
									, id_product
									, id_component
							)						
						
							, Product_Component_Recursivity as (							
									
								SELECT
					         		N.id_factory
									, N.id_product AS id_product_final
									, N.id_product
									, N.id_component 
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST(N.coefficient AS DECIMAL(18,6)) AS exp_coefficient
									, 2 AS Level
								
								FROM Distinct_Nomenclature N
								WHERE 1=1										
					
								UNION ALL
								
								SELECT
									N.id_factory
									, R.id_product_final
									, N.id_product
									, N.id_component
									, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient
									, CAST((R.exp_coefficient * N.coefficient) AS DECIMAL(18,6)) AS exp_coefficient
									, R.Level + 1 AS Level
								
								FROM Distinct_Nomenclature N
								INNER JOIN Product_Component_Recursivity R
									ON N.id_product_final = R.id_product_final
									AND N.id_product = R.Id_component											
									AND N.id_factory = R.id_factory		
							)
						
							"
						
					End If
							
				#End Region		
				
				#Region "Product Component Report"				
													
				ElseIf query = "Product_Component_Report"																	
										
						Dim sTableNomenclature As String = If (scenario.StartsWith("RF") Or scenario.StartsWith("B"), "XFC_PLT_HIER_Nomenclature_Date_Planning","XFC_PLT_HIER_Nomenclature_Date")
						Dim sScenarioFilter As String	= If (scenario.StartsWith("RF") Or scenario.StartsWith("B"), $"AND N.scenario='{scenario}'","")
					
						sql = $"		
												
							,Months AS (
							    SELECT EOMONTH(DATEFROMPARTS({year}, 1, 1)) AS target_date
							    UNION ALL
							    SELECT EOMONTH(DATEADD(MONTH, 1, target_date))
							    FROM Months
							    WHERE MONTH(target_date) < 12
							)
						
							,Product_Component_Recursivity AS (
							    SELECT 
							        M.target_date,
							        N.id_factory,
							        N.id_product AS id_product_final,
									CONVERT(VARCHAR(MAX),N.id_product) AS id_product,
							        N.id_component,
							        CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
							        CAST(N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
							        2 AS Level
							    FROM Months M
							    INNER JOIN {sTableNomenclature} N
							        ON M.target_date BETWEEN N.start_date AND N.end_date
								WHERE 1=1
									{sScenarioFilter}
							    UNION ALL
							
							    SELECT 
							        M.target_date,
							        N.id_factory,
							        R.id_product_final,
							        CONCAT(R.id_product,'_',N.id_product) AS id_product,
							        N.id_component,
							        CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient,
							        CAST(R.exp_coefficient * N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient,
							        R.Level + 1
							    FROM Months M
							    INNER JOIN {sTableNomenclature} N
							        ON M.target_date BETWEEN N.start_date AND N.end_date
							    INNER JOIN Product_Component_Recursivity R
							        ON R.id_component = N.id_product
							        AND R.id_factory = N.id_factory
							        AND R.target_date = M.target_date
								WHERE 1=1
									{sScenarioFilter}
							)
						
							, Product_Component_Production AS (
						
								SELECT DISTINCT R.*
								FROM Product_Component_Recursivity R
						
								INNER JOIN (
											SELECT id_factory, MONTH(date) AS Month, id_product, SUM(Activity_TAJ) AS Activity_TAJ 
											FROM XFC_PLT_FACT_Production
											WHERE scenario = '{scenario}'
												AND Activity_TAJ <> 0
												AND YEAR(date) = {year}
											GROUP BY id_factory, MONTH(date), id_product
										) P
									ON P.id_factory = R.id_factory
									AND P.id_product = R.id_component
									AND P.month <= MONTH(R.target_date)
						
								INNER JOIN (
											SELECT id_factory, MONTH(date) AS Month, id_product, SUM(Activity_TAJ) AS Activity_TAJ 
											FROM XFC_PLT_FACT_Production
											WHERE scenario = '{scenario}'
											AND Activity_TAJ <> 0
											AND YEAR(date) = {year}
											GROUP BY id_factory, MONTH(date), id_product) P2
									ON P2.id_factory = R.id_factory
									AND P2.id_product = R.id_product_final
									AND P2.month <= MONTH(R.target_date)		
						
								WHERE R.exp_coefficient <> 0
									
							)
					
							"
							
				#End Region					
				
				#Region "Product Component No Recursivity All Months"				
													
				ElseIf query = "Product_Component_No_Recursivity_All_Months"					
												
'					BRApi.ErrorLog.LogMessage(si, "type: " & type)
'					Dim type As String = "Existing"
					If (type = "Existing")		
'						BRApi.ErrorLog.LogMessage(si, "Product: " & product)
'						Dim sFilterProduct As String = If(product = "", $"AND id_product IN (SELECT DISTINCT id_product FROM factProduction)" _ 
'																  	  , $"AND id_product = '{product}'")							
																	  
						Dim sFilterProduct As String = If(product = "", $"" _ 
																  	  , $"AND id_product = '{product}'")
																	  
						Dim sFilterFactory As String = If(factory = "", $"" _ 
																  	  , $"AND N.id_factory = '{factory}'")																	  																						  
	
						Dim sSublevels As String = String.Empty
						Dim sUnionAlls As String = String.Empty
						
						For N As Integer = 3 To 10
							
							sSublevels = sSublevels & _
								$"
							
								,Level{N} AS (
									SELECT L.target_date, L.id_factory, L.id_product_final, N.id_product, N.id_component, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient, CAST(L.exp_coefficient * N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient, L.Level + 1 AS Level
									FROM Level{N-1} L LEFT JOIN XFC_PLT_HIER_Nomenclature_Date N ON N.id_product  = L.id_component AND N.id_factory = L.id_factory AND L.target_date BETWEEN N.start_date AND N.end_date
								    {sFilterFactory})
								"
								
							sUnionAlls = sUnionAlls & _
								$"
								UNION ALL SELECT * FROM Level{N}
								"							
						Next
																		  
						sql = 																	  
						$"
						WITH Months AS (
						    SELECT EOMONTH(DATEFROMPARTS({year},1,1)) AS target_date
						    UNION ALL
						    SELECT EOMONTH(DATEADD(MONTH,1,target_date))
						    FROM Months
						    WHERE MONTH(target_date) < 12
						)
						
						,Level2 AS (
						    SELECT M.target_date, N.id_factory, N.id_product AS id_product_final, N.id_product, N.id_component, CAST(N.coefficient AS DECIMAL(18,6)) AS coefficient, CAST(N.coefficient * N.prorata AS DECIMAL(18,6)) AS exp_coefficient, 2 AS Level
						    FROM Months M INNER JOIN XFC_PLT_HIER_Nomenclature_Date N ON M.target_date BETWEEN N.start_date AND N.end_date
						    {sFilterFactory}
						    {sFilterProduct})
						
						{sSublevels}
						
						,Product_Component_Recursivity AS (
						    SELECT * FROM Level2
							{sUnionAlls}
						)																			
					"
						
					End If
							
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
							
							Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)

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
											AND year = {yearRef} 
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
							
							Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
					
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
											WHERE year = {yearRef} 
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
								
				#Region "Impact Parity Coefficient"
				
				Else If query = "ImpactParityCoefficient"
					
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", factory, month, year, scenario, scenarioRef, time, "", "", "", currency)			
					sql = $"
							{sql_Rate}

							, ImpactParityCoefficient as (
								SELECT
									R1.id_factory
									, R1.Month as month
									, (R1.rate*R1.rate_exception/R2.rate - 1) as coefficientIP
							
							FROM fxRate R1
					
							LEFT JOIN fxRateRef R2
								ON R1.Month = R2.Month
								AND R1.id_factory = R2.id_factory
							
							)					

					"

				#End Region
					
				#Region "OLD - RATE 2 years"
				
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
											SELECT year, month, rate
											FROM XFC_MAIN_AUX_fxrate R
											INNER JOIN XFC_PLT_MASTER_Factory F
												ON R.source = F.currency						
											WHERE F.id = '{factory}'
											AND (year = {year} OR year = {year} - 1)
											AND type = '{type_rate}'
											AND target = '{currency}') FX
									ON M.month = FX.month	
									AND Y.year = FX.year
								
								LEFT JOIN fxRateRef R2
 									ON M.month = R2.month
									AND Y.year = R2.year
							)								
							"
							
				#End Region				
				
				#Region "Variance Analysis"
				
				Else If query = "VarianceAnalysis"
							
							Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)														
							
							Dim filterOnlyVT As String = If (type = "Yes", "AND C.type = 'A'", "")
							
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
							
							, activityFilter AS (
									SELECT 
										[Id Factory] AS id_factory
										, MONTH(A.date) AS month
										, [Id GM] AS id_averagegroup
										, CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS activity
									FROM ActivityAll A								
								
					             	WHERE 1=1
								 	AND YEAR(A.date) = {year} 
							
									AND  (('{view}' = 'Periodic' AND MONTH(A.date) = {month})
										OR
										 ('{view}' = 'YTD' AND MONTH(A.date) <= {month}))							
							
								 	AND [Id Factory] = '{factory}'										
									AND Time = 'UO1'
					
									GROUP BY [Id Factory], MONTH(A.date), [Id GM]
							)							
							
							, PercVarAccount AS (
					       
								SELECT DISTINCT scenario, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								WHERE 1=1
							
								AND  (('{view}' = 'Periodic' AND MONTH(date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(date) <= {month}))
							
								AND ((scenario = '{scenario}' AND YEAR(V.date) = {year}) OR (scenario = '{scenarioRef}' AND YEAR(V.date) = {yearRef}))
																				
								AND id_factory = '{factory}'
					       )		
							
						   , ParityPercentages AS (
								SELECT * FROM (
									SELECT id_factory, id_account, value, 'EUR' AS currency
									FROM XFC_PLT_AUX_ParityPercentages
									WHERE scenario = 'Actual'
									AND year = {year} ) AS RES
								WHERE currency = '{currency}'
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
									, C.type
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
								--AND CONVERT(INT,A.account_type) <> 0
					          	{filterOnlyVT}
					       
								GROUP BY
									F.id_factory
								  	, MONTH(F.date)
									, YEAR(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description
									, C.nature
									, C.type
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
									, C.type
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
								AND YEAR(F.date) = {yearRef} 
								AND  (('{view}' = 'Periodic' AND MONTH(F.date) = {month})
									OR
									 ('{view}' = 'YTD' AND MONTH(F.date) <= {month}))	
								AND F.id_factory = '{factory}'
								--AND CONVERT(INT,A.account_type) <> 0
								AND F.value <> 0
								{filterOnlyVT}
								
								GROUP BY
									F.id_factory
									, MONTH(F.date)
									, YEAR(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description		
									, C.nature
									, C.type
									, A.account_type
					       
					       )
					       
					       , ActRefFinalData AS (
					       
					       SELECT 
					        	id_factory, month, year, account_type AS id_account_type, id_account, id_averagegroup, id_costcenter, costcenter, nature, variability
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
					         	, CASE WHEN A.id_averagegroup IS NOT NULL OR A2.id_averagegroup IS NOT NULL OR F.type <> 'A' THEN
									F.value * (100 - ISNULL(P.value,0)) / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100							
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_semi						
					       
					       FROM RefFact F	
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON A.id_factory = F.id_factory
								AND A.month = F.month
								AND A.id_averagegroup = F.id_averagegroup	
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM activityFilter WHERE activity <> 0) A2
								ON A2.id_factory = F.id_factory
								AND A2.month = F.month
								AND A2.id_averagegroup = F.id_averagegroup	
							
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
					          	, CASE WHEN A.id_averagegroup IS NOT NULL OR A2.id_averagegroup IS NOT NULL OR F.type <> 'A' THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100
					         	, CASE WHEN A.id_averagegroup IS NOT NULL OR A2.id_averagegroup IS NOT NULL OR F.type <> 'A' THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_semi						
					       
					       FROM RefFact F							
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON A.id_factory = F.id_factory
								AND A.month = F.month
								AND A.id_averagegroup = F.id_averagegroup	
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM activityFilter WHERE activity <> 0) A2
								ON A2.id_factory = F.id_factory
								AND A2.month = F.month
								AND A2.id_averagegroup = F.id_averagegroup	
							
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
					         	, F.value * P.value / 100 AS ref_value_adj_semi
					       
					       FROM ActFact  F
						   LEFT JOIN (SELECT id_factory, month, id_averagegroup 
									  FROM RefFact
									  GROUP BY id_factory, month, id_averagegroup 
									  HAVING SUM(value) <> 0) A
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
					         id_factory, month, year, account_type, id_account, id_averagegroup, id_costcenter, costcenter, nature, variability
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
								-- AND C.date = P2.date
								AND MONTH(C.date) = MONTH(P2.date)
								AND YEAR(P2.date) = '{yearRef}'
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
							
					       	, ((ref_value_adj_semi_parity * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))							
								- ((ref_value_adj_semi_parity * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R2.rate,0)) AS [Impact Parity]
							
					       	, (ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)							
							+ ((ref_value_adj_semi_parity * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))							
								- ((ref_value_adj_semi_parity * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R2.rate,0)) AS [Ref. Adjusted at Actual Parity]
							
					       	, ISNULL(SUM(CASE WHEN id_indicator IN ('AGS','MIX','ORG','NOR','SAL','CTS') THEN E.value / 1000 END) * R1.rate,0)
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
					       	, SUM(CASE WHEN id_indicator = 'EXC' THEN E.value / 1000 END) * R1.rate AS [Excess Labour]
					       	, SUM(CASE WHEN id_indicator = 'CHO' THEN E.value / 1000 END) * R1.rate AS [Unemployement]
					       	, SUM(CASE WHEN id_indicator IN ('POR','PHC','COV','DIV') THEN E.value / 1000 END) * R1.rate AS [Productivity]
					       	, SUM(CASE WHEN id_indicator = 'POR' THEN E.value / 1000 END) * R1.rate AS [Organization Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'PHC' THEN E.value / 1000 END) * R1.rate AS [Hyper Competitiveness Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'COV' THEN E.value / 1000 END) * R1.rate AS [COVID 19 Effect]
					       	, SUM(CASE WHEN id_indicator = 'DIV' THEN E.value / 1000 END) * R1.rate AS [Extra Productivity Effect]
							
							--ACTUAL
					       	, (act_value * R1.rate) - 
								(
								--REF. ADJUSTED
									(ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)
							
								-- IMPACT PARITY
									+ ((ref_value_adj_semi_parity * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))							
									- ((ref_value_adj_semi_parity * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R2.rate,0))													
							
								-- EFFECTS
								+ ISNULL(SUM(CASE WHEN id_indicator NOT LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)
							
								-- ELECTRICTIY / GAZ EFFECTS
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all,0)
								)
								AS [GAP]		
							
					       	, act_value * R1.rate AS [Actual at Actual Parity]
					       
							FROM (
									SELECT A.id_factory, A.month, A.id_account_type, A.variability
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi	
										, SUM(ref_value_adj_semi * ISNULL(P.value,1)) AS ref_value_adj_semi_parity
						   			FROM ActRefFinalData A
									LEFT JOIN ParityPercentages P
										ON A.id_factory = P.id_factory
										AND A.id_account = P.id_account
						   			GROUP BY A.id_factory, A.month, A.id_account_type, A.variability
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

        					GROUP BY A.id_factory, A.month, id_account_type, description, R1.rate, R2.rate, R1.rate_exception, R1.rate_all , A.variability, ref_value, ref_value_adj_100, ref_value_adj_semi, ref_value_adj_semi_parity, act_value	
						    
							-- ORDER BY id_factory, month, id_account_type, variability
							)	
						"
				#End Region					
				
				#Region "Variance Analysis (All Factories)"
				
				Else If query = "VarianceAnalysis_All_Factories"
					
							Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
																	
							Dim sql_ActivityIndex = AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioRef, "All")	
							Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", month, year, scenario, scenarioRef, time, "", "", "", currency)	
							
							month = Right("0" & month,2)
							Dim fechaDesde As String = year & "-01-01"
							Dim fechaHasta As String = year & "-" & month & "-01"
							
							Dim fechaDesdeRef As String = yearRef & "-01-01"
							Dim fechaHastaRef As String = yearRef & "-" & month & "-01"							
							
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
						           		NULLIF(CONVERT(DECIMAL(18,2),SUM([Activity TSO])),0) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END * 100 AS activity_index 
						        
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
							
							, activityFilter AS (
								SELECT 
									[Id Factory] AS id_factory
									, MONTH(A.date) AS month
									, [Id GM] AS id_averagegroup
									, CONVERT(DECIMAL(18,2),SUM([Activity TAJ])) AS activity
								FROM ActivityAll A								
							
			             		WHERE YEAR(A.date) = {year} 
								AND MONTH(A.date) <= {month}   																					
								AND Time = 'UO1'
				
								GROUP BY [Id Factory], MONTH(A.date), [Id GM]
							)								
							
					       , PercVarAccount AS (
					       
								SELECT DISTINCT id_factory, scenario, MONTH(V.date) AS month, id_account, value
								FROM XFC_PLT_AUX_FixedVariableCosts V
								
								WHERE MONTH(date) <= {month}
							
								AND ((scenario = '{scenario}' AND YEAR(V.date) = {year}) OR (scenario = '{scenarioRef}' AND YEAR(V.date) = {yearRef}))
														
					       )		
							
						   , ParityPercentages AS (
								SELECT * FROM (
									SELECT id_factory, id_account, value, 'EUR' AS currency
									FROM XFC_PLT_AUX_ParityPercentages
									WHERE scenario = 'Actual'
									AND year = {year} ) AS RES
								WHERE currency = '{currency}'
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
									, C.type
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
									, C.type
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
									, C.type
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
							
								AND  F.date BETWEEN '{fechaDesdeRef}' AND '{fechaHastaRef}'
							
								AND CONVERT(INT,A.account_type) <> 0
								AND F.value <> 0
								
								GROUP BY
									F.id_factory
									, YEAR(F.date)
									, MONTH(F.date)
									, F.id_account
									, F.id_averagegroup
									, F.id_costcenter
									, C.description		
									, C.nature
									, C.type
									, A.account_type
					       
					       )
					       
					       , ActRefFinalData AS (
					       
					       SELECT 
					        	id_factory, year, month, account_type AS id_account_type, id_account, id_averagegroup, id_costcenter, costcenter, nature, type, variability
					         	, SUM(act_value) AS act_value
					        	, SUM(ref_value) AS ref_value
					        	, SUM(ref_value_adj_100) AS ref_value_adj_100
					        	, SUM(ref_value_adj_semi) AS ref_value_adj_semi
					       FROM (
					       
					       -- Actual FIXED
							
					       SELECT 
					          	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature, F.type
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
					           	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature, F.type
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
					           	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature, F.type
					           	, 'F' AS variability
					         	, 0 AS act_value
					        	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value
					         	, CASE WHEN A.id_averagegroup IS NOT NULL OR A2.id_averagegroup IS NOT NULL OR F.type <> 'A' THEN
									F.value * (100 - ISNULL(P.value,0)) / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100							
					         	, F.value * (100 - ISNULL(P.value,0)) / 100 AS ref_value_adj_semi
					       
					       FROM RefFact  F
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON A.id_factory = F.id_factory
								AND A.month = F.month
								AND A.id_averagegroup = F.id_averagegroup	
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM activityFilter WHERE activity <> 0) A2
								ON A2.id_factory = F.id_factory
								AND A2.month = F.month
								AND A2.id_averagegroup = F.id_averagegroup	
							
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
					            F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature, F.type
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, F.value * P.value / 100 AS ref_value	
					          	, CASE WHEN A.id_averagegroup IS NOT NULL OR A2.id_averagegroup IS NOT NULL OR F.type <> 'A' THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_100
					         	, CASE WHEN A.id_averagegroup IS NOT NULL OR A2.id_averagegroup IS NOT NULL OR F.type <> 'A' THEN
									F.value * P.value / 100
									* ISNULL(UO1.activity_index,100) / 100 END AS ref_value_adj_semi								
					       
					       FROM RefFact F							
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM ActFact) A
								ON A.id_factory = F.id_factory
								AND A.month = F.month
								AND A.id_averagegroup = F.id_averagegroup	
							
						   LEFT JOIN (SELECT DISTINCT id_factory, month, id_averagegroup 
									  FROM activityFilter WHERE activity <> 0) A2
								ON A2.id_factory = F.id_factory
								AND A2.month = F.month
								AND A2.id_averagegroup = F.id_averagegroup							
							
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
					           	F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup	, F.id_costcenter, F.costcenter, F.nature, F.type
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
					            F.id_factory, F.year, F.month, F.account_type, F.id_account, F.id_averagegroup, F.id_costcenter, F.costcenter, F.nature, F.type
					         	, 'V' AS variability
					          	, 0 AS act_value
					         	, 0 AS ref_value
					          	, F.value * P.value / 100 AS ref_value_adj_100
					         	, F.value * P.value / 100 AS ref_value_adj_semi
					       
					       FROM ActFact  F
						   LEFT JOIN (SELECT id_factory, month, id_averagegroup 
									  FROM RefFact
									  GROUP BY id_factory, month, id_averagegroup 
									  HAVING SUM(value) <> 0) A
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
							 , id_averagegroup
							 , id_costcenter
							 , costcenter	
							 , nature
							 , type
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
								-- AND C.date = P2.date
								AND MONTH(C.date) = MONTH(P2.date)
								AND YEAR(P2.date) = '{yearRef}'
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
							
					       	, ((ref_value_adj_semi_parity * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))							
								- ((ref_value_adj_semi_parity * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R2.rate,0)) AS [Impact Parity]
							
					       	, (ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)							
							+ ((ref_value_adj_semi_parity * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))							
								- ((ref_value_adj_semi_parity * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R2.rate,0)) AS [Ref. Adjusted at Actual Parity]							
	
					       	, ISNULL(SUM(CASE WHEN id_indicator IN ('AGS','MIX','ORG','NOR','SAL','CTS') THEN E.value / 1000 END) * R1.rate,0)
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
					       	, SUM(CASE WHEN id_indicator = 'EXC' THEN E.value / 1000 END) * R1.rate AS [Excess Labour]
					       	, SUM(CASE WHEN id_indicator = 'CHO' THEN E.value / 1000 END) * R1.rate AS [Unemployement]
					       	, SUM(CASE WHEN id_indicator IN ('POR','PHC','COV','DIV') THEN E.value / 1000 END) * R1.rate AS [Productivity]
					       	, SUM(CASE WHEN id_indicator = 'POR' THEN E.value / 1000 END) * R1.rate AS [Organization Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'PHC' THEN E.value / 1000 END) * R1.rate AS [Hyper Competitiveness Productivity Effect]
					       	, SUM(CASE WHEN id_indicator = 'COV' THEN E.value / 1000 END) * R1.rate AS [COVID 19 Effect]
					       	, SUM(CASE WHEN id_indicator = 'DIV' THEN E.value / 1000 END) * R1.rate AS [Extra Productivity Effect]							
							
							--ACTUAL
					       	, (act_value * R1.rate) - 
								(
								--REF. ADJUSTED
									(ref_value_adj_semi * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)
							
								-- IMPACT PARITY
									+ ((ref_value_adj_semi_parity * R1.rate * R1.rate_exception) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R1.rate * R1.rate_exception,0))							
									- ((ref_value_adj_semi_parity * R2.rate) + ISNULL(SUM(CASE WHEN id_indicator LIKE 'R%' THEN E.value / 1000 END) * R2.rate,0))													
							
								-- EFFECTS
								+ ISNULL(SUM(CASE WHEN id_indicator NOT LIKE 'R%' THEN E.value / 1000 END) * R1.rate,0)
							
								-- ELECTRICTIY / GAZ EFFECTS
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN ELE.effect_price_variable ELSE ELE.effect_price_fixed END / 1000) * R1.rate_all,0) 
								+ ISNULL(AVG(CASE WHEN A.variability = 'V' THEN GAS.effect_price_variable ELSE GAS.effect_price_fixed END / 1000) * R1.rate_all,0)
								)
								AS [GAP]
							
					       	, act_value * R1.rate AS [Actual at Actual Parity]
					       
							FROM (
									SELECT A.id_factory, A.month, A.id_account_type, A.variability
										, SUM(act_value) AS act_value
										, SUM(ref_value) AS ref_value
										, SUM(ref_value_adj_100) AS ref_value_adj_100
										, SUM(ref_value_adj_semi) AS ref_value_adj_semi	
										, SUM(ref_value_adj_semi * ISNULL(P.value,1)) AS ref_value_adj_semi_parity
						   			FROM ActRefFinalData A
									LEFT JOIN ParityPercentages P
										ON A.id_factory = P.id_factory
										AND A.id_account = P.id_account
						   			GROUP BY A.id_factory, A.month, A.id_account_type, A.variability
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

        					GROUP BY A.id_factory, A.month, id_account_type, description, R1.rate, R2.rate, R1.rate_exception, R1.rate_all, A.variability, ref_value, ref_value_adj_100, ref_value_adj_semi, ref_value_adj_semi_parity, act_value	
						    
							-- ORDER BY id_factory, month, id_account_type, variability
							)	
						"
							
				#End Region								
				
				#Region "Energy Variance"
				
				Else If query =  "EnergyVariance"
				
				' Necesita la consulta de ActivityIndex
				' Falta añadir la cuenta de Gas
				' Variables: scenario, scenarioRef, factory, year
				
					Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
				
					Dim sqlActivityIndex As String = AllQueries(si, "ActivityALL", factory, month, year, scenario, scenarioRef, "All")
					Dim sql_Rate = AllQueries(si, "RATE", factory, month, year, scenario, scenarioRef, time, "", "", "", "EUR")	
					
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
								AND YEAR(date) = {yearRef}
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
								AND YEAR(date) = {yearRef}
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
									month
									, [Id GM] AS id_averagegroup
									, Time
									, ISNULL(X.value/100, A.activity_index) AS activity_index
							
								FROM (
									SELECT 
										[Id Factory]
										, MONTH(A.date) AS month
										, [Id GM]
										, Time
										,CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])),0) <> 0 THEN
												CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END AS activity_index	
									
									FROM ActivityAll A								
								
					             	WHERE 1=1
								 		AND YEAR(A.date) = {year} 
								 		AND [Id Factory] = '{factory}'							
										AND ('{view}' = 'Periodic') OR ('{view}' = 'YTD')
									
									-- AND  (('{view}' = 'Periodic' AND MONTH(A.date) = {month})
									-- 	OR
									-- 	 ('{view}' = 'YTD' AND MONTH(A.date) <= {month}))							
					
									GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
								) A
							
								LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
									ON A.[Id GM] = X.id_averagegroup
									AND A.month = MONTH(X.date)
									AND A.[Id Factory] = X.id_factory
									AND A.Time = X.uotype
									AND X.scenario = '{scenario}'
									AND X.scenario_ref = '{scenarioRef}'						
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
								, C.Time as time
							
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
								AND YEAR(A.date) = {yearRef}
								AND A.id_factory = '{factory}'
								AND (A.id_account = '3D32' OR A.id_account = '3D34')
								AND A.scenario = '{scenarioRef}'
								AND D.scenario = '{scenarioRef}'
						
							GROUP BY A.id_factory, YEAR(A.date), MONTH(A.date), A.id_account, A.id_averagegroup, C.activity_index, C.Time, D.value
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
					
							SELECT
								-- R1.id_factory
								R1.Month
								, CASE WHEN '{currency}'='Local' THEN 1 ELSE R1.rate_all END as TipoCambio_ACT
								, CASE WHEN '{currency}'='Local' THEN 1 ELSE R2.rate_exception_calc END as TipoCambio_REF
								-- , CASE WHEN '{currency}'='Local' THEN 1 ELSE R2.rate END as TipoCambio_REF
								-- , R1.rate_all as TipoCambio_ACT_Aux
								-- , R2.rate as TipoCambio_REF_Aux
								, CASE WHEN '{currency}'='EUR' THEN 1 ELSE R2.rate/R1.rate_all END as TipoCambio_COEF
							
							FROM fxRate R1
					
							LEFT JOIN fxRateRef R2
								ON R1.Month = R2.Month
								-- AND R1.id_factory = R2.id_factory
							
						),
						
						EnergyVariance as (
							SELECT 
								A.Month
								, A.Factory
								, A.Energy
								, A.REF_Price * C.TipoCambio_ACT as [Ref Price MWh in parity Analysis]
								, A.REF_Consumption	as [Ref Conso in MWh]				
								, A.ACT_Price * C.TipoCambio_ACT as [REL Price MWh in parity Analysis]
								, A.ACT_Consumption as [Actual A Conso in MWh]
								, EnergyIndexSemi as [FIP Semi-Adjustment Index (Base 100)]
								, ACT_Consumption * (REF_Price * TipoCambio_ACT) / 1000 as [Conso Actual A / Price Ref at parity Analysis]
								, ACT_Consumption * (ACT_Price * TipoCambio_ACT) / 1000 as [Conso Actual A / Price Actual at parity Anlaysis]
								, ACT_Consumption * (ACT_Price - REF_Price) * TipoCambio_ACT / 1000 as [Price Variance at parity Analysis]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_ACT /1000 as [Conso Ref semi-adjusted / Price Ref at parity Analysis]
								, '' as [Conso Actual A / Price Ref at parity Analysis - (REPETIDA)]
								, (ACT_Consumption * REF_Price * TipoCambio_ACT / 1000) - (REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_ACT /1000) as [Consumption Vairance at parity Analysis]
								, REF_Price * TipoCambio_REF * (TipoCambio_COEF)  as [Reference Price MWh at parity Ref]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_REF * (TipoCambio_COEF) /1000 as [Conso Ref semi-adj / Price Ref at parity Ref]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_ACT/1000 as [Conso Ref semi-adj / Prix Reference at parity actual]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * (TipoCambio_ACT -TipoCambio_REF * (TipoCambio_COEF))/1000 as [Parity Variance]
								, (ACT_Consumption * ACT_Price * TipoCambio_ACT / 1000) - (REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_REF * (TipoCambio_COEF) /1000) as [Total Variance in K€]
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
				
				#Region "Energy Variance - All Factories"
				
				Else If query =  "EnergyVariance_All_Factories"
				
				' Necesita la consulta de ActivityIndex
				' Falta añadir la cuenta de Gas
				' Variables: scenario, scenarioRef, factory, year
				
					Dim yearRef As String = getYearRef(si, scenario, scenarioRefForYear, year)
					
					Dim sqlActivityIndex As String = AllQueries(si, "ActivityALL_All_Factories", "", month, year, scenario, scenarioRef, "All")
					Dim sql_Rate = AllQueries(si, "RATE_All_Factories", "", month, year, scenario, scenarioRef, time, "", "", "", "EUR")	
					
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
								AND indicator = 'Price'
								AND YEAR(date) = {yearRef}
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
								AND indicator = 'Consumption'
								AND YEAR(date) = {yearRef}
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
									A.[Id Factory] as id_factory
									, month
									, [Id GM] AS id_averagegroup
									, Time
									, ISNULL(X.value/100, A.activity_index) AS activity_index
							
								FROM (
									SELECT 
										[Id Factory]
										, MONTH(A.date) AS month
										, [Id GM]
										, Time
										,CASE WHEN ISNULL(CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])),0) <> 0 THEN
												CONVERT(DECIMAL(18,2),SUM([Activity TSO])) / CONVERT(DECIMAL(18,2),SUM([Activity TAJ Ref])) END AS activity_index	
									
									FROM ActivityAll A								
								
					             	WHERE 1=1
								 		AND YEAR(A.date) = {year} 
										AND ('{view}' = 'Periodic') OR ('{view}' = 'YTD')						
					
									GROUP BY [Id Factory], MONTH(A.date), [Id GM], Time
								) A
							
								LEFT JOIN XFC_PLT_AUX_ActivityIndex_Adjust X
									ON A.[Id GM] = X.id_averagegroup
									AND A.month = MONTH(X.date)
									AND A.[Id Factory] = X.id_factory
									AND A.Time = X.uotype
									AND X.scenario = '{scenario}'
									AND X.scenario_ref = '{scenarioRef}'						
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
								, C.Time as time
							
							FROM XFC_PLT_FACT_CostsDistribution A
								
							LEFT JOIN ActivityIndex C
								ON A.id_factory = C.id_factory
								AND A.id_averagegroup = C.id_averagegroup
								AND MONTH(A.date) = C.Month
							
							LEFT JOIN XFC_PLT_AUX_FixedVariableCosts D
								ON YEAR(A.date) = YEAR(D.date)
								AND MONTH(A.date) = MONTH(D.date)
								AND A.scenario = D.scenario
								AND A.id_factory = D.id_factory
								AND A.id_account = D.id_account
						
							WHERE 1=1
								AND YEAR(A.date) = {yearRef}
								AND (A.id_account = '3D32' OR A.id_account = '3D34')
								AND A.scenario = '{scenarioRef}'
								AND D.scenario = '{scenarioRef}'
						
							GROUP BY A.id_factory, YEAR(A.date), MONTH(A.date), A.id_account, A.id_averagegroup, C.activity_index, C.Time, D.value
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
					
							SELECT
								R1.id_factory
								, R1.Month
								, CASE WHEN '{currency}'='Local' THEN 1 ELSE R1.rate_all END as TipoCambio_ACT
								, CASE WHEN '{currency}'='Local' THEN 1 ELSE R2.rate_exception_calc END as TipoCambio_REF
								-- , CASE WHEN '{currency}'='Local' THEN 1 ELSE R2.rate END as TipoCambio_REF
								-- , R1.rate_all as TipoCambio_ACT_Aux
								-- , R2.rate as TipoCambio_REF_Aux
								, CASE WHEN '{currency}'='EUR' THEN 1 ELSE R2.rate/R1.rate_all END as TipoCambio_COEF
							
							FROM fxRate R1
					
							LEFT JOIN fxRateRef R2
								ON R1.Month = R2.Month
								AND R1.id_factory = R2.id_factory
							
						),
						
						EnergyVariance as (
							SELECT 
								A.Month
								, A.Factory
								, D.description as [Factory Description]
								, A.Energy
								, A.REF_Price * C.TipoCambio_ACT as [Ref Price MWh in parity Analysis]
								, A.REF_Consumption	as [Ref Conso in MWh]				
								, A.ACT_Price * C.TipoCambio_ACT as [REL Price MWh in parity Analysis]
								, A.ACT_Consumption as [Actual A Conso in MWh]
								, EnergyIndexSemi as [FIP SemiAdjustment Index]
								, ACT_Consumption * (REF_Price * TipoCambio_ACT) / 1000 as [Conso Actual A _ Price Ref at parity Analysis]
								, ACT_Consumption * (ACT_Price * TipoCambio_ACT) / 1000 as [Conso Actual A _ Price Actual at parity Anlaysis]
								, ACT_Consumption * (ACT_Price - REF_Price) * TipoCambio_ACT / 1000 as [Price Variance at parity Analysis]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_ACT /1000 as [Conso Ref semi_adjusted _ Price Ref at parity Analysis]
								, '' as [Conso Actual A _ Price Ref at parity Analysis _ REPETIDA]
								, (ACT_Consumption * REF_Price * TipoCambio_ACT / 1000) - (REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_ACT /1000) as [Consumption Vairance at parity Analysis]
								, REF_Price * TipoCambio_REF * (TipoCambio_COEF) as [Reference Price MWh at parity Ref]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_REF * (TipoCambio_COEF)  /1000 as [Conso Ref semiadj _ Price Ref at parity Ref]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_ACT / 1000 as [Conso Ref semiadj _ Prix Reference at parity actual]
								, REF_Consumption * (EnergyIndexSemi) * REF_Price * (TipoCambio_ACT -TipoCambio_REF * (TipoCambio_COEF) ) / 1000 as [Parity Variance]
								, (ACT_Consumption * ACT_Price * TipoCambio_ACT / 1000) - (REF_Consumption * (EnergyIndexSemi) * REF_Price * TipoCambio_REF * (TipoCambio_COEF) /1000) as [Total Variance in Keur]
								-- , B.Variability
								, TipoCambio_ACT 
								, TipoCambio_REF 
								, TipoCambio_COEF
								
						
							FROM preCalculate A

							LEFT JOIN energyIndex B
								ON A.Month = B.Month						
								AND A.Factory = B.id_factory
								AND A.Energy = B.Energy
						
							LEFT JOIN divisa C
								ON A.Month = C.Month
								AND A.Factory = C.id_factory
					
							LEFT JOIN XFC_PLT_MASTER_Factory D
								ON A.Factory = D.id
					
							WHERE 1=1
								AND B.time = 'UO1'
					)
					"					
					#End Region
					
				#Region "Daily Hours"
				
				Else If query =  "DailyHours"

					sql = $"
						WITH DataHours AS (
					
							SELECT 
								D.id_factory
								, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(D.date)),2) AS month
								, D.wf_type
								, D.id_indicator
								, D.value
								, C.id AS id_costcenter
								, C.type AS type_costcenter
							
							FROM XFC_PLT_AUX_DailyHours_Stored D
						
							INNER JOIN (
							            SELECT id, type, id_factory, start_date, end_date
							            FROM XFC_PLT_MASTER_CostCenter_Hist 
							            WHERE scenario = 'Actual'  
							            --AND type = 'A' 
										) AS C
						 		ON D.id_costcenter = C.id
								AND D.id_factory = C.id_factory
								AND DATEFROMPARTS({year},MONTH(D.date),1) BETWEEN start_date AND end_date
					
							-- LEFT JOIN XFC_PLT_AUX_Calendar CA
							-- 	ON CA.scenario = '{scenario}'
							-- 	AND CA.id_factory = D.id_factory
							-- 	AND CA.date = D.date
							-- 	AND CA.id_template = D.wf_type
						
						 	WHERE D.scenario = '{scenario}'
						 	AND YEAR(D.date) = {year}
							AND ( UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR D.id_factory = '{factory}' )							   
						)
					
						, Report AS (
							
							SELECT D.*, M.*
							FROM (	SELECT DISTINCT id_factory, month, wf_type, id_costcenter, type_costcenter
									FROM DataHours  ) D
							CROSS JOIN XFC_PLT_MASTER_DailyHours_Indicator M
						)
					
						-- STORED Data							
					
						, StoredHours AS (								
					
						 	SELECT D.id_factory, D.month, D.wf_type, D.id_indicator AS indicator, id_costcenter, type_costcenter
								, CASE WHEN M.description2 LIKE '%(-)%' THEN D.value * -1 ELSE D.value END AS Value
								, M.block
					
								FROM DataHours D
								LEFT JOIN XFC_PLT_MASTER_DailyHours_Indicator M
									ON D.id_indicator = M.id							
					
						)
					
						-- CALCULATED Data
					
						, ExtraHoursNegative AS (
							SELECT S.id_factory, S.Month, S.wf_type, M.id AS indicator, M.block, S.id_costcenter, S.type_costcenter
								, SUM(S.value) * -1 AS Value
								
									
						    FROM StoredHours S
							LEFT JOIN XFC_PLT_MASTER_DailyHours_Indicator M
								ON S.indicator + '_negative' = M.id
							WHERE S.indicator = 'extra_hours' 
							GROUP BY S.id_factory, S.Month, S.wf_type, M.id, M.block, S.id_costcenter, S.type_costcenter
						)							
					
						, AllStoredHours AS (
					
										SELECT id_factory, Month, wf_type, Indicator, Block, id_costcenter, type_costcenter, Value FROM StoredHours		
							UNION ALL	SELECT id_factory, Month, wf_type, Indicator, Block, id_costcenter, type_costcenter, Value FROM ExtraHoursNegative		
						)
					
						, RequiredHours AS (
							SELECT id_factory, Month, wf_type, id_costcenter, type_costcenter
								, 'required_hours' AS Indicator 
								, SUM(value) AS Value
								, '2T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2')
							GROUP BY id_factory, Month, wf_type, id_costcenter, type_costcenter 
						)		
					
						, PaidHours AS (
							SELECT id_factory, Month, wf_type, id_costcenter, type_costcenter
								, 'paid_hours' AS Indicator 
								, SUM(value) AS Value
								, '3T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3')
							GROUP BY id_factory, Month, wf_type, id_costcenter, type_costcenter
						)			
					
						, HarbourHours AS (
							SELECT id_factory, Month, wf_type, id_costcenter, type_costcenter
								, 'harbour_hours' AS Indicator 
								, SUM(value) AS Value
								, '4T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3','4')
							GROUP BY id_factory, Month, wf_type, id_costcenter, type_costcenter 
						)								
					
						, WorkingHours AS (
							SELECT id_factory, Month, wf_type, id_costcenter, type_costcenter
								, 'working_hours' AS Indicator 
								, SUM(value) AS Value
								, '5T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3','4','5')
							GROUP BY id_factory, Month, wf_type, id_costcenter, type_costcenter 
						)		
					
						, WorkingHoursExclExtraHours AS (
							SELECT id_factory, Month, wf_type, id_costcenter, type_costcenter
								, 'working_hours_excluding_eh' AS Indicator 
								, SUM(value) AS Value
								, '6T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3','4','5','6')
							GROUP BY id_factory, Month, wf_type, id_costcenter, type_costcenter 
						)	
					
						, AllHours AS (
					
									  SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM AllStoredHours GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM RequiredHours GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM PaidHours GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM HarbourHours GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM WorkingHours GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, wf_type, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM WorkingHoursExclExtraHours GROUP BY Id_Factory, Month, Indicator, wf_type, id_costcenter, type_costcenter								
						)	
					"
						
					#End Region	
					
				#Region "Daily Hours Shift"
				
				Else If query =  "DailyHoursShift"

					sql = $"
						WITH DataHours AS (
					
							SELECT 
								D.id_factory
								, D.scenario
								, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(D.date)),2) AS month
								, D.wf_type
								, D.shift
								, D.id_indicator
								, D.value
								, C.id AS id_costcenter
								, C.type AS type_costcenter
							
							FROM XFC_PLT_AUX_DailyHours_Stored D
						
							INNER JOIN (
							            SELECT id, type, id_factory, start_date, end_date
							            FROM XFC_PLT_MASTER_CostCenter_Hist 
							            WHERE scenario = 'Actual'  
							            --AND type = 'A' 
										) AS C
						 		ON D.id_costcenter = C.id
								AND D.id_factory = C.id_factory
								AND DATEFROMPARTS({year},MONTH(D.date),1) BETWEEN start_date AND end_date					
						
						 	WHERE D.scenario IN ('{scenario}','{scenarioRef}')
						 	AND YEAR(D.date) = {year}
							AND ( UPPER('{factory}') = 'ALL' OR '{factory}' = '' OR D.id_factory = '{factory}' )							   
						)
					
						, Report AS (
							
							SELECT D.*, M.*
							FROM (	SELECT DISTINCT id_factory, scenario, month, wf_type, shift, id_costcenter, type_costcenter
									FROM DataHours  ) D
							CROSS JOIN XFC_PLT_MASTER_DailyHours_Indicator M
						)
					
						-- STORED Data							
					
						, StoredHours AS (								
					
						 	SELECT D.id_factory, D.scenario, D.month, D.wf_type, D.shift, D.id_indicator AS indicator, id_costcenter, type_costcenter
								, CASE WHEN M.description2 LIKE '%(-)%' THEN D.value * -1 ELSE D.value END AS Value
								, M.block
					
								FROM DataHours D
								LEFT JOIN XFC_PLT_MASTER_DailyHours_Indicator M
									ON D.id_indicator = M.id							
					
						)
					
						-- CALCULATED Data
					
						, ExtraHoursNegative AS (
							SELECT S.id_factory, S.scenario, S.Month, S.wf_type, S.shift, M.id AS indicator, M.block, S.id_costcenter, S.type_costcenter
								, SUM(S.value) * -1 AS Value
								
									
						    FROM StoredHours S
							LEFT JOIN XFC_PLT_MASTER_DailyHours_Indicator M
								ON S.indicator + '_negative' = M.id
							WHERE S.indicator = 'extra_hours' 
							GROUP BY S.id_factory, S.scenario, S.Month, S.wf_type, S.shift, M.id, M.block, S.id_costcenter, S.type_costcenter
						)							
					
						, AllStoredHours AS (
					
										SELECT id_factory, scenario, Month, wf_type, shift, Indicator, Block, id_costcenter, type_costcenter, Value FROM StoredHours		
							UNION ALL	SELECT id_factory, scenario, Month, wf_type, shift, Indicator, Block, id_costcenter, type_costcenter, Value FROM ExtraHoursNegative		
						)
					
						, RequiredHours AS (
							SELECT id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter
								, 'required_hours' AS Indicator 
								, SUM(value) AS Value
								, '2T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2')
							GROUP BY id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter 
						)		
					
						, PaidHours AS (
							SELECT id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter
								, 'paid_hours' AS Indicator 
								, SUM(value) AS Value
								, '3T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3')
							GROUP BY id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter
						)			
					
						, HarbourHours AS (
							SELECT id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter
								, 'harbour_hours' AS Indicator 
								, SUM(value) AS Value
								, '4T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3','4')
							GROUP BY id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter 
						)								
					
						, WorkingHours AS (
							SELECT id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter
								, 'working_hours' AS Indicator 
								, SUM(value) AS Value
								, '5T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3','4','5')
							GROUP BY id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter 
						)		
					
						, WorkingHoursExclExtraHours AS (
							SELECT id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter
								, 'working_hours_excluding_eh' AS Indicator 
								, SUM(value) AS Value
								, '6T' AS Block
									
						    FROM AllStoredHours 
							WHERE Block IN ('1','2','3','4','5','6')
							GROUP BY id_factory, scenario, Month, wf_type, shift, id_costcenter, type_costcenter 
						)	
					
						, AllHours AS (
					
									  SELECT Id_Factory, scenario, wf_type, shift, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM AllStoredHours GROUP BY Id_Factory, scenario, Month, Indicator, wf_type, shift, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, scenario, wf_type, shift, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM RequiredHours GROUP BY Id_Factory, scenario, Month, Indicator, wf_type, shift, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, scenario, wf_type, shift, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM PaidHours GROUP BY Id_Factory, scenario, Month, Indicator, wf_type, shift, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, scenario, wf_type, shift, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM HarbourHours GROUP BY Id_Factory, scenario, Month, Indicator, wf_type, shift, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, scenario, wf_type, shift, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM WorkingHours GROUP BY Id_Factory, scenario, Month, Indicator, wf_type, shift, id_costcenter, type_costcenter
							UNION ALL SELECT Id_Factory, scenario, wf_type, shift, Month, Indicator, id_costcenter, type_costcenter, SUM(Value) AS Value FROM WorkingHoursExclExtraHours GROUP BY Id_Factory, scenario, Month, Indicator, wf_type, shift, id_costcenter, type_costcenter								
						)		
					"
						
					#End Region			
					
				#Region "FTE"
				
				Else If query =  "FTE"

						  	Dim sql_DailyHours As String =  AllQueries(si, "DailyHoursShift",factory,, year, scenario, scenarioRef, "", "",,,"")
						  
							sql = sql_DailyHours & $"				  
						  				
								, calendar AS (
							
									SELECT id_factory, scenario, month, wf_type, shift, id_costcenter, SUM(value) AS value FROM (
							
										SELECT 
											id_factory
											, scenario
											, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) AS month
											, id_template AS wf_type
											, CASE WHEN id_indicator = 'JTNO' THEN '2x8 B' ELSE 'Weekend' END AS shift
											, id_costcenter
											, value AS value 
										FROM XFC_PLT_AUX_Calendar
										WHERE scenario IN ('{scenario}','{scenarioRef}')
										AND YEAR(date) = {year}
										AND id_indicator IN ('JTNO','JNTA_WE')
							
									) C
									GROUP BY id_factory, scenario, month, wf_type, shift, id_costcenter
							
								)
							
								, time_presence AS (
								
									SELECT 
										id_factory
										, scenario
										, 'M' + RIGHT('0' + CONVERT(VARCHAR,MONTH(date)),2) AS month
										, wf_type
										, id_costcenter
										, shift
										, value
									FROM XFC_PLT_Aux_TimePresence
									WHERE scenario = '{scenario}'
									AND YEAR(date) = {year}
								)
							
								, FTE_data AS (
									SELECT 
										D.id_factory
										, F.description
										, D.scenario
										, D.month
										, D.wf_type
										, D.id_costcenter
										, I.description1
										, D.shift
										, C.type	
										, D.value AS hours								
										, CA.value AS worked_days
										, T.value AS hours_shift
										, D.value / NULLIF(CA.value,0) / NULLIF(T.value,0) AS FTE
																	
									FROM 	  (	SELECT id_factory, scenario, month, wf_type, id_costcenter, Indicator, shift, SUM(value) AS value
												FROM AllHours				 		
												WHERE indicator <> 'extra_hours_negative'
												GROUP BY id_factory, scenario, month, wf_type, id_costcenter, Indicator, shift) D
								
									LEFT JOIN calendar CA
										ON CA.id_factory = D.id_factory
										AND CA.scenario = D.scenario
										AND CA.month = D.month
										AND CA.wf_type = D.wf_type
										AND CA.shift = D.shift
										AND CA.id_costcenter = D.id_costcenter
										
									LEFT JOIN time_presence T
										ON T.id_factory = D.id_factory
										AND T.scenario = D.scenario
										AND T.month = D.month
										AND T.wf_type = D.wf_type
										AND T.id_costcenter = D.id_costcenter
										AND T.shift = D.shift
										
									LEFT JOIN XFC_PLT_MASTER_Factory F
										ON D.id_factory = F.id
								
									LEFT JOIN XFC_PLT_MASTER_DailyHours_Indicator I
										ON D.indicator = I.id
										
									LEFT JOIN (
												SELECT id, type, id_factory, start_date, end_date
												FROM XFC_PLT_MASTER_CostCenter_Hist 
												WHERE scenario = 'Actual'  
												--AND type = 'A' 
												) AS C
										ON D.id_costcenter = C.id
										AND D.id_factory = C.id_factory
										AND DATEFROMPARTS({year},REPLACE(D.month,'M',''),1) BETWEEN start_date AND end_date						
								)						
					"
							
'							BRAPI.ErrorLog.LogMessage(si,sql)
						
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
									AND id_factory <> 'R0671'
							
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
										AND id_factory <> 'R0671'	
									"
							
				#End Region		
				
				#Region "Update FACT Costs S4H"
				
				Else If query = "Update_Fact_Costs_S4H"
							
							sql = $"
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_FACT_Costs
								WHERE 1=1
									AND id_factory = '{factory}'
									AND scenario = 'Actual'
									AND YEAR(date) = {year}						
									AND MONTH(date) = {month}
					
								-- 2- Insert statement				
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_factory, date, id_account, id_rubric, id_costcenter, value, currency)
								
								SELECT *
								FROM (  
							
									SELECT  
										'Actual' as scenario
									    , '{factory}' AS id_factory
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
												AND CAST(A.[month] as INT) = {month}										
												AND A.year = {year}	
											    AND (id_factory_map = '{factory}')
								      		)  AS ext
						
								      GROUP BY id_factory_map, [year], [month], mng_account, rubirc, id_costcenter
						
								) AS mapeo	
							
								-- UNION ALL
								-- 
								-- -- Exception (Without CC): 78.Scraps
								-- 
								-- SELECT *
								-- FROM (  
								-- 
								-- 	SELECT  
								-- 		'Actual' as scenario
								-- 	    , '{factory}' AS id_factory
								-- 	    , DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date
								-- 	    , mng_account as id_account
								-- 		, rubirc
								-- 	    , ISNULL(id_costcenter,'-1') as id_costcenter
								-- 	    , SUM(value_intern) as value
								-- 	    , 'Local' as currency
								-- 
								-- 	FROM (
								--       		SELECT A.* 
								-- 			, CASE
								-- 				WHEN num_cpte = '703100' THEN '2820' 
								-- 			  END as mng_account
								-- 
							    --             FROM (	SELECT *, 
								-- 						CASE 
								-- 			                WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
								-- 							END AS id_factory_map
								-- 					FROM XFC_PLT_RAW_CostsMonthly
								-- 
								-- 					WHERE num_cpte IN ('703100') AND id_costcenter IS NULL) A
		 						-- 
								--       		WHERE 1=1
								-- 				AND CAST(A.[month] as INT) = {month}										
								-- 				AND A.year = {year}	
								-- 			    AND (id_factory_map = '{factory}')
								--       		)  AS ext
								-- 
								--       GROUP BY id_factory_map, [year], [month], mng_account, rubirc, id_costcenter
								-- 
								-- ) AS mapeo								
					
								"
							
'								BRApi.ErrorLog.LogMessage(si, sql)
							
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
			
			Dim excelBytes As Byte() = Nothing

			
			If templateFile = True Then
				
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
			
				excelBytes = memoryStream.ToArray()
				
			Else
								
			#Region "Create Excel From Scratch In-Memory"

			Dim memoryStream As New MemoryStream()
			
			' Crear un nuevo archivo Excel desde cero en el stream
			Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook)
			
			    ' Añadir partes necesarias
			    Dim workbookPart As WorkbookPart = spreadsheetDocument.AddWorkbookPart()
			    workbookPart.Workbook = New Workbook()
			
			    Dim worksheetPart As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
			    worksheetPart.Worksheet = New Worksheet(New SheetData())
			    
				' Crear estilos
			    Dim stylesPart As WorkbookStylesPart = workbookPart.AddNewPart(Of WorkbookStylesPart)()
			    stylesPart.Stylesheet = CreateStylesheet()
			    stylesPart.Stylesheet.Save()
	
			    ' Agregar hoja
			    Dim sheets As Sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(New Sheets())
			    Dim sheet As New Sheet() With {
			        .Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
			        .SheetId = 1,
			        .Name = "XLSX Import"
			    }
			    sheets.Append(sheet)
			
			    ' Obtener SheetData
			    Dim sheetData As SheetData = worksheetPart.Worksheet.GetFirstChild(Of SheetData)()
				
				' Insertar fila de encabezados
				Dim headerRow As New Row() With {.RowIndex = CType(startRow - 1, UInt32Value)}
				
				For colIndex As Integer = 0 To dt.Columns.Count - 1
				    Dim colLetter As String = GetExcelColumnName(colIndex + 1)
				    Dim cellReference As String = colLetter & (startRow - 1).ToString()
				
				    Dim headerCell As New Cell() With {
				        .CellReference = cellReference,
				        .CellValue = New CellValue(dt.Columns(colIndex).ColumnName),
				        .DataType = New EnumValue(Of CellValues)(CellValues.String),
						.StyleIndex = 1
				    }
				
				    headerRow.Append(headerCell)
				Next
				
				sheetData.Append(headerRow)
				
			    ' Insertar datos del DataTable
			    For rowIndex As Integer = 0 To dt.Rows.Count - 1
			        Dim newRow As New Row() With {.RowIndex = CType(rowIndex + startRow, UInt32Value)}
			
			        For colIndex As Integer = 0 To dt.Columns.Count - 1
			            Dim valor As String = If(IsDBNull(dt.Rows(rowIndex)(colIndex)), "", dt.Rows(rowIndex)(colIndex).ToString())
			            Dim colLetter As String = GetExcelColumnName(colIndex + 1)
			            Dim cellReference As String = colLetter & (rowIndex + startRow).ToString()
			
			            Dim cell As New Cell() With {.CellReference = cellReference}
			
			            If IsNumeric(valor) Then
			                cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString().Replace(",", "."))
			                cell.DataType = New EnumValue(Of CellValues)(CellValues.Number)
			            Else
			                cell.CellValue = New CellValue(valor)
			                cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
			            End If
						
			            newRow.Append(cell)
			        Next
			
			        sheetData.Append(newRow)

			    Next
			
			    ' Guardar workbook
				worksheetPart.Worksheet.Save()
			    workbookPart.Workbook.Save()
				
			End Using			
			
			#End Region
			
				excelBytes = memoryStream.ToArray()
			End If
			
			#Region "Create File"
				' --------------- GUARDADO FINAL DEL ARCHIVO ---------------
				
                ' Actualizar el archivo en OneStream
                Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, destPath)
                Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, excelBytes)
''                Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, GenerateExcelBytesFromDataTable(si, dt, "XLSX Import"))
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
		
		Private Function CreateStylesheet() As Stylesheet
		    Dim stylesheet As New Stylesheet()
		
		    ' Fonts
		    Dim fonts As New Fonts(
		        New Font(), ' Default
		        New Font( ' Font blanco
		            New Color() With {.Rgb = New HexBinaryValue() With {.Value = "FFFFFF"}},
		            New Bold()
		        )
		    )
		
		    ' Fills
		    Dim fills As New Fills(
		        New Fill(New PatternFill() With {.PatternType = PatternValues.None}),
		        New Fill(New PatternFill() With {.PatternType = PatternValues.Gray125}),
		        New Fill( ' Azul oscuro
		            New PatternFill(
		                New ForegroundColor() With {.Rgb = New HexBinaryValue() With {.Value = "156082"}},
		                New BackgroundColor() With {.Indexed = 64}
		            ) With {.PatternType = PatternValues.Solid}
		        )
		    )
		
		    ' Borders
		    Dim borders As New Borders(New Border()) ' Default border
		
		    ' CellFormats
		    Dim cellFormats As New CellFormats(
		        New CellFormat(), ' default
		        New CellFormat With {
		            .FontId = 1,
		            .FillId = 2,
		            .BorderId = 0,
		            .ApplyFont = True,
		            .ApplyFill = True
		        }
		    )
		
		    stylesheet.Append(fonts)
		    stylesheet.Append(fills)
		    stylesheet.Append(borders)
		    stylesheet.Append(cellFormats)
		
		    Return stylesheet
		End Function
		
	#End Region
	
	#End Region	
	
	#Region "Create Excel - VIEW"
		
		Public Function CreateExcel_New(
			ByVal si As SessionInfo, _
			Optional factory As String = "", Optional year As Integer = 0, Optional month As Integer = 0, _
			Optional scenario As String = "", Optional time As String = "", Optional scenario_ref As String = "", Optional view As String = "", Optional currency As String = "", Optional reference As String = "", Optional indicator As String = "", _
			Optional folderPath As String = "Documents/Public/Plants/Exports", Optional fileName As String = "Export.xlsx", Optional destPath As String = "", _
			Optional sql As String = "", Optional dt As DataTable = Nothing, Optional dtOrgn As DataTable = Nothing
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
			
			Dim excelBytes As Byte() = Nothing

			
			If templateFile = True Then
				
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
			
				excelBytes = memoryStream.ToArray()
				
			Else
				
				#Region "Create Excel From Scratch In-Memory"

				Dim memoryStream As New MemoryStream()
				
				' Crear un nuevo archivo Excel desde cero en el stream
				Using spreadsheetDocument As SpreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook)
					
				    ' Añadir partes necesarias
				    Dim workbookPart As WorkbookPart = spreadsheetDocument.AddWorkbookPart()
				    workbookPart.Workbook = New Workbook()
				
				    ' Agregar la hoja "XLSX Import"
				    Dim worksheetPartData As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
				    worksheetPartData.Worksheet = New Worksheet(New SheetData())
				    Dim sheetData As SheetData = worksheetPartData.Worksheet.GetFirstChild(Of SheetData)()
				
				    ' Agregar la hoja "Metadata"
				    Dim worksheetPartMeta As WorksheetPart = workbookPart.AddNewPart(Of WorksheetPart)()
				    worksheetPartMeta.Worksheet = New Worksheet(New SheetData())
				    Dim sheetDataMeta As SheetData = worksheetPartMeta.Worksheet.GetFirstChild(Of SheetData)()
				
				    ' Crear estilos
				    Dim stylesPart As WorkbookStylesPart = workbookPart.AddNewPart(Of WorkbookStylesPart)()
				    stylesPart.Stylesheet = CreateStylesheet()
				    stylesPart.Stylesheet.Save()
				
				    ' Agregar hojas al libro de trabajo
				    Dim sheets As Sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(New Sheets())
				    
				    ' Agregar la hoja "XLSX Import"
				    Dim sheetDataSheet As New Sheet() With {
				        .Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPartData),
				        .SheetId = 1,
				        .Name = "XLSX Import"
				    }
				    sheets.Append(sheetDataSheet)
				
				    ' Agregar la hoja "Metadata"
				    Dim sheetMeta As New Sheet() With {
				        .Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPartMeta),
				        .SheetId = 2,
				        .Name = "Metadata"
				    }
				    sheets.Append(sheetMeta)
				
				    ' ---------------- Insertar datos en la hoja "XLSX Import" ----------------
				    ' Insertar fila de encabezados en la hoja "XLSX Import"
				    Dim headerRow As New Row() With {.RowIndex = CType(startRow - 1, UInt32Value)}
				
				    For colIndex As Integer = 0 To dt.Columns.Count - 1
				        Dim colLetter As String = GetExcelColumnName(colIndex + 1)
				        Dim cellReference As String = colLetter & (startRow - 1).ToString()
						
				        Dim headerCell As New Cell() With {
				            .CellReference = cellReference,
				            .CellValue = New CellValue(dt.Columns(colIndex).ColumnName),
				            .DataType = New EnumValue(Of CellValues)(CellValues.String),
				            .StyleIndex = 1
				        }
				
				        headerRow.Append(headerCell)
				    Next
				
				    sheetData.Append(headerRow)
				
				    ' Insertar datos del DataTable en la hoja "XLSX Import"
				    For rowIndex As Integer = 0 To dt.Rows.Count - 1
				        Dim newRow As New Row() With {.RowIndex = CType(rowIndex + startRow, UInt32Value)}
				
				        For colIndex As Integer = 0 To dt.Columns.Count - 1
				            Dim valor As String = If(IsDBNull(dt.Rows(rowIndex)(colIndex)), "", dt.Rows(rowIndex)(colIndex).ToString())
				            Dim colLetter As String = GetExcelColumnName(colIndex + 1)
				            Dim cellReference As String = colLetter & (rowIndex + startRow).ToString()
							
							Dim colStringName() As String = {"id", "id_product", "id_component", "id_account"}
							Dim colName As String = dt.Columns.Item(colIndex).ColumnName
							
				            Dim cell As New Cell() With {.CellReference = cellReference}
				
				            If IsNumeric(valor) And Not colStringName.Contains(colName) Then
				                cell.CellValue = New CellValue(Convert.ToDouble(valor).ToString().Replace(",", "."))
				                cell.DataType = New EnumValue(Of CellValues)(CellValues.Number)
				            Else
				                cell.CellValue = New CellValue(valor)
				                cell.DataType = New EnumValue(Of CellValues)(CellValues.String)
				            End If
				
				            newRow.Append(cell)
				        Next
				
				        sheetData.Append(newRow)
				    Next
				
				    ' ---------------- Insertar "HOLA" en la hoja "Metadata" ----------------
				    ' Insertar "HOLA" en la primera celda de la hoja "Metadata"
'				    Dim headerRowMeta As New Row() With {.RowIndex = CType(1, UInt32Value)} ' Primer fila
'				    Dim cellMeta As New Cell() With {
'				        .CellReference = "A1", ' Primer celda
'				        .CellValue = New CellValue(factory),
'				        .DataType = New EnumValue(Of CellValues)(CellValues.String)
'				    }
'				    headerRowMeta.Append(cellMeta)
'				    sheetDataMeta.Append(headerRowMeta)

					' DataTable para los metadatos
				    Dim dtMeta As New DataTable()
				    dtMeta.Columns.Add("Filter")
				    dtMeta.Columns.Add("Value")
				
				    ' Obtener los filtros aplicados
				    Dim filtrosAplicados() As String = Split(dtOrgn.Rows(0)("FileFilter"))
				
				    ' Diccionario con los valores
				    Dim dataDictionary As New Dictionary(Of String, String) From {
				        {"Factory", factory},
				        {"Scenario", scenario},
				        {"Scenario_ref", scenario_ref},
						{"Year", year.ToString()},
				        {"Month", month.ToString()},
				        {"View", view},
				        {"Currency", currency},
				        {"Reference", reference},
						{"indicator", indicator}
				    }
				    
					
					For Each key As String In dataDictionary.Keys
					    ' Verifica si el filtro ya está en la lista de filtros aplicados
					    If Not filtrosAplicados.Contains(key) Then
					        ' Solo agregar la fila si el valor no está vacío
					        If Not String.IsNullOrEmpty(dataDictionary(key)) Then
					            ' Si no está vacío, agregamos la clave del diccionario con su valor
					            dtMeta.Rows.Add(key, dataDictionary(key))
					        End If
					    End If
					Next
				    ' Agregar filas al DataTable dtMeta
				    For Each filtro As String In filtrosAplicados
				        If dataDictionary.Keys.Contains(filtro) Then
				            dtMeta.Rows.Add(filtro, dataDictionary(filtro))
				        End If
				    Next

					Dim headerRowMeta As New Row() With {.RowIndex = CType(1, UInt32Value)} ' Primer fila
				    Dim cellHeader As New Cell() With {
				        .CellReference = "A1", ' Primer celda
				        .CellValue = New CellValue("Filter"), ' Encabezado de la primera columna
				        .DataType = New EnumValue(Of CellValues)(CellValues.String)
				    }
				    headerRowMeta.Append(cellHeader)
				
				    Dim cellValueHeader As New Cell() With {
				        .CellReference = "B1", ' Segunda celda
				        .CellValue = New CellValue("Value"), ' Encabezado de la segunda columna
				        .DataType = New EnumValue(Of CellValues)(CellValues.String)
				    }
				    headerRowMeta.Append(cellValueHeader)
				
				    ' Agregar la fila de encabezado a la hoja Metadata
				    sheetDataMeta.Append(headerRowMeta)
				
				    ' Agregar los valores de dtMeta a la hoja Metadata
				    Dim metadataRowIndex As Integer = 2 ' Empezamos en la segunda fila (después de la fila de encabezado)
				    For Each row As DataRow In dtMeta.Rows
				        Dim rowMeta As New Row() With {.RowIndex = CType(metadataRowIndex, UInt32Value)}
				
				        ' Crear la primera celda (filtro)
				        Dim cellFilter As New Cell() With {
				            .CellReference = $"A{metadataRowIndex}",
				            .CellValue = New CellValue(row("Filter").ToString()),
				            .DataType = New EnumValue(Of CellValues)(CellValues.String)
				        }
				        rowMeta.Append(cellFilter)
				
				        ' Crear la segunda celda (valor)
				        Dim cellValue As New Cell() With {
				            .CellReference = $"B{metadataRowIndex}",
				            .CellValue = New CellValue(row("Value").ToString()),
				            .DataType = New EnumValue(Of CellValues)(CellValues.String)
				        }
				        rowMeta.Append(cellValue)
				
				        ' Agregar la fila a la hoja Metadata
				        sheetDataMeta.Append(rowMeta)
				
				        ' Incrementar el índice de fila para la siguiente fila
				        metadataRowIndex += 1
				    Next
				
				    ' Guardar ambas hojas
				    worksheetPartData.Worksheet.Save()
				    worksheetPartMeta.Worksheet.Save()
				
				    ' Guardar el archivo Excel final
				    workbookPart.Workbook.Save()

					For Each filtro As String In filtrosAplicados
					    ' Convertir tanto filtro como la clave del diccionario a minúsculas para evitar problemas con mayúsculas/minúsculas
					    If dataDictionary.Keys.Contains(filtro.ToLower()) Then
					        dtMeta.Rows.Add(filtro, dataDictionary(filtro.ToLower()))
					    Else
					        ' brapi.ErrorLog.LogMessage(si, "Filtro no encontrado en el diccionario: " & filtro)
					    End If
					Next
				End Using
				
				#End Region
				
				' Convertir el contenido del MemoryStream a un byte array
				excelBytes = memoryStream.ToArray()	
				
				

			End If
			
			
			#Region "Create File"
				' --------------- GUARDADO FINAL DEL ARCHIVO ---------------
				
                ' Actualizar el archivo en OneStream
                Dim fileInfo As XFFileInfo = New XFFileInfo(FileSystemLocation.ApplicationDatabase, destPath)
                Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, excelBytes)
''                Dim fileFile As XFFile = New XFFile(fileInfo, String.Empty, GenerateExcelBytesFromDataTable(si, dt, "XLSX Import"))
				fileFile.FileInfo.ContentFileExtension = "xlsx"
				' fileFile.FileInfo.Name=
                brapi.FileSystem.InsertOrUpdateFile(si, fileFile)
				
				
'				Dim headerRowMeta As New Row() With {.RowIndex = CType(1, UInt32Value)} ' Primer fila para encabezados
'				sheetMeta.Append(headerRowMeta)
				
'				' Añadir las celdas de encabezado para "Filter" y "Value" en la primera fila
'				Dim cellFilter As New Cell() With {
'				    .CellReference = "A1", ' Primera celda de la fila
'				    .CellValue = New CellValue("Filter"),
'				    .DataType = New EnumValue(Of CellValues)(CellValues.String)
'				}
'				headerRowMeta.Append(cellFilter)
				
'				Dim cellValue As New Cell() With {
'				    .CellReference = "B1", ' Segunda celda de la fila
'				    .CellValue = New CellValue("Value"),
'				    .DataType = New EnumValue(Of CellValues)(CellValues.String)
'				}
'				headerRowMeta.Append(cellValue)
				
'				' Ahora, recorre las filas del DataTable para agregar las celdas de "Filter" y "Value"
'				Dim metadataRowIndex As Integer = 2 ' Empezar en la segunda fila después de los encabezados
				
'				For Each row As DataRow In dtMeta.Rows
'				    ' Crear una nueva fila para cada fila del DataTable
'				    Dim newRowMeta As New Row() With {.RowIndex = CType(metadataRowIndex, UInt32Value)}
'				    sheetDataMeta.Append(newRowMeta)
				
'				    ' Insertar los valores de las columnas "Filter" y "Value" en las celdas correspondientes
'				    Dim cellFilterValue As New Cell() With {
'				        .CellReference = $"A{metadataRowIndex}", ' La celda dinámica (A2, A3, A4, ...)
'				        .CellValue = New CellValue(row("Filter").ToString()), ' Valor de la columna "Filter"
'				        .DataType = New EnumValue(Of CellValues)(CellValues.String)
'				    }
'				    newRowMeta.Append(cellFilterValue)
				
'				    Dim cellValueValue As New Cell() With {
'				        .CellReference = $"B{metadataRowIndex}", ' La celda dinámica (B2, B3, B4, ...)
'				        .CellValue = New CellValue(row("Value").ToString()), ' Valor de la columna "Value"
'				        .DataType = New EnumValue(Of CellValues)(CellValues.String)
'				    }
'				    newRowMeta.Append(cellValueValue)
				
'				    ' Incrementar el índice de la fila para la siguiente fila
'				    metadataRowIndex += 1
'				Next
				
'				' Guardar la hoja con los datos de los filtros
'				worksheetPartMeta.Worksheet.Save()
				
'				' Guardar el archivo Excel final
'				workbookPart.Workbook.Save()
			#End Region
			
			Return Nothing
		End Function
		
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
	
	#Region "Create TXT"
    Public Function CreateTXT(
        ByVal si As SessionInfo, _
        Optional folderPath As String = "Documents/Public/Plants/Exports", _
        Optional fileName As String = "Export.txt", _
        Optional destPath As String = "", _
        Optional dt As DataTable = Nothing
    )
        ' Create filename and path
        Dim filepath As XFFolderEx = BRApi.FileSystem.GetFolder(si, FileSystemLocation.ApplicationDatabase, $"{folderPath}")
        
        ' Use a string builder object
        Dim filesAsString As New StringBuilder()
        
        ' Loop through the rows and create a line for each row
        For Each dr As DataRow In dt.Rows
            ' Customize this to change the format in the .txt file
            filesAsString.AppendLine(String.Join(" ", dr.ItemArray().Select(Function(item) item.ToString().Replace(",", ".")).ToArray())) ' Here, you can use spaces, tabs, etc.
        Next dr
        
        ' Convert the string to an array of byte
        Dim fileAsByte() As Byte = System.Text.Encoding.UTF8.GetBytes(filesAsString.ToString()) ' Use UTF8 for .txt files
        
        ' Save fileAsByte
        Dim fileDataInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, $"{fileName}", filepath.XFFolder.FullName)
        Dim fileData As New XFFile(fileDataInfo, String.Empty, fileAsByte)
        fileDataInfo.ContentFileExtension = "txt"
        brapi.ErrorLog.LogMessage(si, "Entro")
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
	
		Public Function ValidScenarioPeriod(ByVal si As SessionInfo, ByVal scenario As String, ByVal year As String, Optional month As String = "") As Boolean
				
			#Region "NEW - Based on Table"
			
			Dim dateVal As String = If(scenario = "Actual", $"{year}-{month}-01", $"{year}-01-01")			
			Dim sqlStatus As String = $"
				SELECT A.status
			
				FROM XFC_PLT_AUX_Scenario_Status A
				
				WHERE 1=1
					AND A.scenario = '{scenario}' 
					AND A.date = '{dateVal}' 
			"
			Dim action As String = String.Empty
			Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Dim dtTemp As DataTable = BRApi.Database.ExecuteSql(dbcon, sqlStatus, True)
				action = If(dtTemp.Rows.Count>0, dtTemp.Rows(0)(0), "Lock")
			End Using

			Return If(action="Lock", False, True)

			#End Region
			
			#Region "OLD - Based on Parameters"
			
'			Dim scenarioForecast As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_forecast")
'			Dim scenarioBudget As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_scenario_budget")
'			Dim actualYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_year_N")	
'			Dim actualMonth As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, $"Plants.prm_PLT_month_closing")
			
'			If scenario = "Actual" And year = actualYear And month = actualMonth Then
'				Return True
'			ElseIf scenario = scenarioForecast And year = actualYear Then
''				If si.UserName = "jose-luis.vazquez" Or si.UserName = "ana.olanda" Then 
'			 		Return True
''				Else 
''					Return False
''			 	End If
'				' Return True
'			ElseIf scenario = scenarioBudget And year = actualYear + 1 Then	
'				Return True				
'			End If
			
'			Return False
			
			#End Region
			
			
		End Function
		
	#End Region		
	
	#Region "Insert Costs"
	
	Public Function InsertCosts(ByVal si As SessionInfo, _
								ByVal factory As String, _
								ByVal year As String, _
								ByVal month As String, _
								ByVal contentFileBytes As Byte()
								)
		Dim filePath As String = "Documents/Public/Plants/Import"			
		Dim factoryR As String = String.Empty
		Dim entityR As String = String.Empty
		Dim replaceDecimals As Boolean = False
		Dim colExtra As Boolean = False				
		
		#Region "Factory Formating"
		
		Select Case factory 
			'Cacia
			Case "R0671"
				factoryR = "0671"
				entityR = "STE"
			'Sevilla
			Case "R0548913"
				factoryR = "1301"
				entityR = "003"						
			'Valladolid
			Case "R0548914"
				factoryR = "1301"
				entityR = "002"
			'Curitiba
			Case "R0483003"
				factoryR = "1303"
				entityR = "%"
			'Bursa Meca
			Case "R0529002"
				factoryR = "1302"
				entityR = "%"		
				replaceDecimals = True
				colExtra = True
			'Pitesti Meca
			Case "R0045106"
				factoryR = "0611"
				entityR = "%"	
			'Los Andes
			Case "R0585"
				factoryR = "0585"							
				entityR = "%"	
				replaceDecimals = True
			'Argentina
			Case "R0592"
				factoryR = "0592"
				entityR = "%"
				replaceDecimals = True
		End Select
		
		#End Region	
		
		#Region "File Bytes"
		Dim newContentBytes As Byte() = Nothing

		Dim cont As Integer = 0
		Dim filteredLines As New List(Of String)
		
		' Paso 1: Convertir los bytes a texto
		Using ms As New MemoryStream(contentFileBytes)
			Dim reader As New StreamReader(ms, Encoding.UTF8)
	            ' Leer el archivo línea por línea
	            Dim line As String
				
	            While (reader.Peek() >= 0)
	                line = reader.ReadLine()	
					
					If line.Contains("|") And Not line.Contains("Exerc") And Not line.Contains("Total") And Not line.Contains("Fisc") Then						
						filteredLines.Add(line)
'					Else
'						BRApi.ErrorLog.LogMessage(si, line)
						
					End If

	            End While

	    End Using
			
		' Paso 4: Unir las líneas filtradas en un solo texto
		Dim newContent As String = String.Join(Environment.NewLine, filteredLines)
		
		' Paso 5: Convertir el nuevo contenido de texto a bytes
		newContentBytes = Encoding.UTF8.GetBytes(newContent)
			
		#End Region
		
		#Region "Config Delimited File"
		
		'Config. for Load a Delimited File
		Dim dbLocation As String = "Application"
		Dim loadMethod As String = "Replace"	 ' Use an expression for partial replace -->  Merge:[(ProductName = 'SomeProduct')] 								
		Dim fieldTokens As New List(Of String)
		
		fieldTokens.Add("xfText#:[dummy1]")
		fieldTokens.Add("xfText#:[year]")
		fieldTokens.Add("xfText#:[month]")
		fieldTokens.Add("xfText#:[id_factory]")
		fieldTokens.Add("xfText#:[entity]")
		fieldTokens.Add("xfText#:[domFonc]")
		fieldTokens.Add("xfText#:[code]")
		fieldTokens.Add("xfText#:[rubirc]")
		fieldTokens.Add("xfText#:[activity]")
		fieldTokens.Add("xfText#:[society]")
		fieldTokens.Add("xfText#:[cbl_account]")		
		If(colExtra)
			fieldTokens.Add("xfText#:[dummy]::0")
		Else
			fieldTokens.Add("xfText#:[value_intern]::0")
		End If
		fieldTokens.Add("xfText#:[value_transact]::0")
		If(colExtra)
			fieldTokens.Add("xfText#:[value_intern]")
		End If	
		fieldTokens.Add("xfText#:[dev]")
		fieldTokens.Add("xfText#:[documentF]")
		fieldTokens.Add("xfText#:[num_cpte]")
		fieldTokens.Add("xfText#:[id_costcenter]")
		fieldTokens.Add("xfText#:[num_ordre]")
		fieldTokens.Add("xfText#:[elm_OTP]")
				
		loadMethod = "Replace"
		
		#End Region			
		
		#Region "SQL Raw"
		
		Dim sqlRawCreate As String = "
		IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='XFC_PLT_RAW_CostsMonthly_Raw' AND xtype='U')
		BEGIN
		
			CREATE TABLE XFC_PLT_RAW_CostsMonthly_Raw
				(
				[year] varchar(50) ,
				[month] varchar(50) ,
				[id_factory] varchar(50) ,
				[entity] varchar(50) ,
				[domFonc] varchar(50) ,
				[code] varchar(50) ,
				[rubirc] varchar(50) ,
				[activity] varchar(50) ,
				[society] varchar(50) ,
				[cbl_account] varchar(50) ,
				[value_intern] varchar(50) ,
				[value_transact] varchar(50) ,
				[dev] varchar(50) ,
				[documentF] varchar(500) ,
				[num_cpte] varchar(50) ,
				[id_costcenter] varchar(50) ,
				[num_ordre] varchar(50) ,
				[elm_OTP] varchar(50) 								
				);
		END
		"					
		
		'BRApi.ErrorLog.LogMessage(si, sqlRawCreate)
		Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
			BRApi.Database.ExecuteSql(oDbConn, sqlRawCreate, False)												
		End Using
			
		Dim tableName As String = "XFC_PLT_RAW_CostsMonthly_Raw"						
		Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, newContentBytes, "|", dbLocation, tableName, loadMethod, fieldTokens, True)					
		
		Dim sqlDecimal As String = "
			SELECT TOP(1) value_intern FROM XFC_PLT_RAW_CostsMonthly_Raw
		"
		
		Dim valor As String = String.Empty
		Dim caracter As Char = Nothing
		Dim formato As String = String.Empty
		
		Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)							
			 valor = (BRApi.Database.ExecuteSql(oDbConn, sqlDecimal, False))(0)(0)												
		End Using
		
		If valor.Contains("-") Then
			caracter = StrReverse(valor)(3)
			If caracter <> "," And caracter <> "." Then caracter = If(StrReverse(valor)(4) = ".", ",", ".")
 		Else
			caracter = StrReverse(valor)(2)
			If caracter <> "," And caracter <> "." Then caracter = If(StrReverse(valor)(3) = ".", ",", ".")
		End If
			
		Dim value_intern As String = If(caracter = ",","REPLACE(REPLACE(value_intern, '.', ''),',','.')","REPLACE(value_intern, ',', '')")
		Dim value_transact As String = If(caracter = ",","REPLACE(REPLACE(value_transact, '.', ''),',','.')","REPLACE(value_transact, ',', '')")		
		
		Dim sqlRawTransfer As String = $"
			DELETE 
			FROM XFC_PLT_RAW_CostsMonthly
			WHERE id_factory = '{factoryR}'
			AND entity LIKE '{entityR}'
			AND year = '{year}'
			AND CONVERT(INTEGER, month) = {month};
		
			INSERT INTO XFC_PLT_RAW_CostsMonthly
			([year],[month],[id_factory],[entity],[domFonc],[code],[rubirc],[activity],[society],[cbl_account],[value_intern],[value_transact],[dev],[documentF],[num_cpte],[id_costcenter],[num_ordre],[elm_OTP])
			
			SELECT
				[year],[month],[id_factory],[entity],[domFonc],[code],[rubirc],[activity],[society],[cbl_account]	
				, CAST(
		    		CASE 
		    		  WHEN RIGHT({value_intern}, 1) = '-' THEN '-' + LEFT({value_intern}, LEN({value_intern}) - 1)
		    		  ELSE {value_intern}
					END AS DECIMAL(18,2)
		  		) AS [value_intern]
				,CAST(
		    		CASE 
		    		  WHEN RIGHT({value_transact}, 1) = '-' THEN '-' + LEFT({value_transact}, LEN({value_transact}) - 1)
		    		  ELSE {value_transact}
					END AS DECIMAL(18,2)
		  		) AS [value_transact]
				,[dev],[documentF],[num_cpte],[id_costcenter],[num_ordre],[elm_OTP]
			FROM XFC_PLT_RAW_CostsMonthly_Raw
			WHERE id_factory = '{factoryR}'
				AND entity LIKE '{entityR}';
			
			DROP TABLE XFC_PLT_RAW_CostsMonthly_Raw;
		"
		
		Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)	
			' BRApi.ErrorLog.LogMessage(si, "Sql Raw: " & sqlRawTransfer)
		
			BRApi.Database.ExecuteSql(oDbConn, sqlRawTransfer, False)												
		End Using						
					
				#End Region

		#Region "SQL Facts OLD"				
		
'		Dim sqlMap As String = $" 
'					-- 1- Previous clear
		
'					DELETE FROM XFC_PLT_RAW_CostsMonthly
'					WHERE 1=1
'						AND CAST([month] as INT) = 16					
		
'					DELETE FROM XFC_PLT_FACT_Costs
'					WHERE 1=1
'						AND (id_factory = '{factory}' OR id_factory = 'TBD')
'						AND scenario = 'Actual'
'						AND YEAR(date) = {year}						
'						AND MONTH(date) = {month}
		
'					-- 2- Insert statement				
'					INSERT INTO XFC_PLT_FACT_Costs (scenario, id_factory, date, id_account, id_rubric, id_costcenter, value, currency)
'					SELECT *
'					FROM (     
'						SELECT  
'							'Actual' as scenario
'						    , id_factory_map AS id_factory
'						    , DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date
'						    , mng_account as id_account
'							, rubirc
'						    , id_costcenter as id_costcenter
'						    , SUM(value_intern) as value
'						    , 'Local' as currency
'						FROM (
'						      		SELECT A.*
'											, COALESCE(B.mng_account,'Others') as mng_account
'					                FROM (	SELECT *, 
'												CASE
'									                WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
'									                WHEN CONCAT('R',id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
'									                WHEN CONCAT('R',id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
'									                WHEN CONCAT('R',id_factory, entity) = 'R1301003' THEN 'R0548913'
'									                WHEN CONCAT('R',id_factory, entity) = 'R1301002' THEN 'R0548914'
'									                WHEN CONCAT('R',id_factory, entity) LIKE '%R0585%' THEN 'R0585'
'									                WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
'									                WHEN CONCAT('R',id_factory, entity) LIKE '%R0592%' THEN 'R0592'
'									                ELSE 'TBD' END AS id_factory_map
'											FROM XFC_PLT_RAW_CostsMonthly) A

'						            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
'						             	ON A.id_costcenter = C.id
'						          		AND C.scenario = 'Actual'
'						          		AND DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) BETWEEN C.start_date AND C.end_date
'									LEFT JOIN (
'												SELECT DISTINCT costcenter_type
'												FROM XFC_PLT_Mapping_Accounts_docF 
'											) as E
'										ON C.type = E.costcenter_type	
'									LEFT JOIN (
'												SELECT DISTINCT 
'													cnt_account
'													, mng_account
'													, costcenter_type
'													, costcenter_nature
'													, docF
'												FROM XFC_PLT_Mapping_Accounts_docF 
'											) as B					
'						       			ON A.num_cpte = B.cnt_account
'										AND C.type = B.costcenter_type
'										AND C.nature = B.costcenter_nature
'										AND A.domFonc = B.docF	
 
'						      		WHERE 1=1
'										AND CAST(A.[month] as INT) = {month}										
'										AND A.year = {year}	
'									    AND id_factory_map = '{factory}'					
'						      		)  AS ext
'						      GROUP BY id_factory_map, [year], [month], mng_account, rubirc, id_costcenter
'						) AS mapeo	
'						WHERE 1=1
								
'						"

		#End Region

		#Region "SQL Facts"

			Dim sqlMap As String = AllQueries(si, "Update_Fact_Costs", factory, month, year, "Actual", "", "", "", "", "", "")
	
			Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
				BRApi.Database.ExecuteSql(oDbConn, sqlMap, False)												
			End Using
		
		#End Region		
	
'		brapi.ErrorLog.LogMessage(si, "Insertado OK")
		
		Return Nothing
			
	End Function	
	
	#End Region	
	
	#Region "Insert Costs S4H"
	
	Public Function InsertCostsS4H(ByVal si As SessionInfo, _
								ByVal factory As String, _
								ByVal year As String, _
								ByVal month As String
								)
										
		Dim factoryR As String = String.Empty	
		Dim entityR As String = String.Empty	
		Dim companyR As String = String.Empty
		
		#Region "Factory Formating"
		
		Select Case factory 
			
			'Cacia
			Case "R0671"
				factoryR = "0671"
				entityR = "STE"
				companyR = "PT10"
		
		End Select	
		
		#End Region
		
		#Region "SQL SAP4H -> Raw"
				
		Dim sqlRawTransfer As String = $"	
		
			DELETE 
			FROM XFC_PLT_RAW_CostsMonthly
			WHERE id_factory = '{factoryR}'
			AND entity LIKE '{entityR}'
			AND year = '{year}'
			AND month = '{month}';
		
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
			    ,'{factoryR}' AS [id_factory]
			    ,'{entityR}' AS [entity]
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
		
			    --,F.profit_center_id
			    --,F.business_partner_id

			FROM XFC_MAIN_FACT_PnLTransactions F
		
			LEFT JOIN XFC_MAIN_MASTER_AccountsOldToNew A
				ON A.name = F.account_name		
			
			LEFT JOIN XFC_MAIN_MASTER_CostCentersOldToNew C
				ON C.id = F.cost_center_id
		
            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C2
             	ON C2.id = C.id_old
          		AND C2.scenario = 'Actual'
          		AND DATEFROMPARTS({year},{month}, 1) BETWEEN C2.start_date AND C2.end_date						
		
			LEFT JOIN (SELECT DISTINCT account_name FROM XFC_MAIN_MASTER_AccountRubrics) R
				ON R.account_name = F.account_name
		
			--LEFT JOIN (SELECT account_name, cost_center_class_id, conso_rubric FROM XFC_MAIN_MASTER_AccountRubrics) R2
			--	ON R2.account_name = F.account_name
			--	AND R2.cost_center_class_id = C2.type
		
			LEFT JOIN (SELECT DISTINCT cnt_account FROM XFC_PLT_MAPPING_Accounts_DocF) M
				ON A.name_old = M.cnt_account	
		
			LEFT JOIN filtered_account_rubrics AS far
				ON F.account_name = far.account_name 
				AND F.cost_center_id = far.cost_center_id		

			WHERE F.company_id = '{companyR}';		
		
		
			-- Update for SCRAPS Exception	
		
			UPDATE XFC_PLT_RAW_CostsMonthly 
				SET id_costcenter = 'AU75834' 
			WHERE num_cpte = '703100' 
			AND id_factory = '{factoryR}'
			AND entity LIKE '{entityR}'
			AND year = '{year}'
			AND month = '{month}';
				
		"
		
		Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)	
'			BRApi.ErrorLog.LogMessage(si, "Sql Raw: " & sqlRawTransfer)
		
			BRApi.Database.ExecuteSql(oDbConn, sqlRawTransfer, False)												
		End Using						
					
				#End Region

		#Region "SQL RAW -> Facts"

			Dim sqlMap As String = AllQueries(si, "Update_Fact_Costs_S4H", factory, month, year, "Actual", "", "", "", "", "", "")
'			BRApi.ErrorLog.LogMessage(si, "sql Map: " & sqlMap)
			
			
			Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
				BRApi.Database.ExecuteSql(oDbConn, sqlMap, False)												
			End Using
		
		#End Region		
	
'		brapi.ErrorLog.LogMessage(si, "Insertado OK")
		
		Return Nothing
			
	End Function	
	
	#End Region		
	
	#Region "Insert Costs S4H - KSB1"
	
	Public Function InsertCostsS4H_KSB1(ByVal si As SessionInfo, _
								ByVal factory As String, _
								ByVal year As String, _
								ByVal month As String
								)
										
		Dim factoryR As String = String.Empty	
		Dim entityR As String = String.Empty	
		Dim companyR As String = String.Empty
		
		#Region "Factory Formating"
		
		Select Case factory 
			
			'Cacia
			Case "R0671"
				factoryR = "0671"
				entityR = "STE"
				companyR = "PT10"
		
		End Select	
		
		#End Region	

		#Region "SQL RAW -> Facts"

			Dim sqlMap As String = AllQueries(si, "Update_Fact_Costs_S4H", factory, month, year, "Actual", "", "", "", "", "", "")
'			BRApi.ErrorLog.LogMessage(si, "sql Map: " & sqlMap)
			
			
			Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
				BRApi.Database.ExecuteSql(oDbConn, sqlMap, False)												
			End Using
		
		#End Region		
	
'		brapi.ErrorLog.LogMessage(si, "Insertado OK")
		
		Return Nothing
			
	End Function	
	
	#End Region		

	#Region "Upadate Fact VTU - Report"
	
	Public Function update_FactVTU_Report(ByVal si As SessionInfo, _
								ByVal scenario As String, _
								ByVal year As String, _
								ByVal month As String
								)
								
		Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		
			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_FACT_Costs_VTU_Report 		WHERE scenario = '{scenario}' AND year = '{year}'",True)							
			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_FACT_Costs_VTU_Report_Local 	WHERE scenario = '{scenario}' AND year = '{year}'",True)							
			month = If(scenario.Contains("RF"),"12",month)
			Dim sql_VTU_data_EUR As String = AllQueries(si, "VTU_data_All_Factories_All_Months", "", month, year, scenario, "", "", "YTD", "Existing", "", "EUR")
			Dim sql_VTU_data_Local As String = AllQueries(si, "VTU_data_All_Factories_All_Months", "", month, year, scenario, "", "", "YTD", "Existing", "", "Local")
			
			sql_VTU_data_EUR =  
				$"
				{sql_VTU_data_EUR}
			
				INSERT INTO XFC_PLT_FACT_Costs_VTU_Report 
					(id_factory
					, date
					, id_product
					, id_averagegroup
					, account_type
					, cost_fixed
					, cost_variable
					, cost
					, volume
					, activity_UO1
					, activity_UO1_total
					, scenario
					, year) 
				
				SELECT *
					, '{scenario}' as scenario
					, '{year}' as year
				FROM factVTU
				"
				
			sql_VTU_data_Local =  
				$"
				{sql_VTU_data_Local}
			
				INSERT INTO XFC_PLT_FACT_Costs_VTU_Report_Local 
					(id_factory
					, date
					, id_product
					, id_averagegroup
					, account_type
					, cost_fixed
					, cost_variable
					, cost
					, volume
					, activity_UO1
					, activity_UO1_total
					, scenario
					, year) 
				
				SELECT *
					, '{scenario}' as scenario
					, '{year}' as year
				FROM factVTU
				"
				
			BRApi.Database.ExecuteSql(dbcon,sql_VTU_data_EUR,True)
			BRApi.Database.ExecuteSql(dbcon,sql_VTU_data_Local,True)
			
		End Using
		
		Return Nothing
			
	End Function	
	
	#End Region
	
	#Region "Upadate Fact VTU - Report - NEW TABLES"
	
	Public Function update_FactVTU_Report_NewTables(ByVal si As SessionInfo, _
								ByVal scenario As String, _
								ByVal year As String, _
								ByVal month As String, _
								ByVal factory As String 
								)
								
		Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			
			Dim monthFilter As String = If(scenario="Actual", $" AND MONTH(date) = {month}", "")
			Dim factroyFilter As String = If(factory="All", "",$" AND id_factory = '{factory}' ")
			
			month = If(scenario.Contains("RF") Or scenario.Contains("Bud"),"12",month)			
			
			#Region "Insert New VTU Tables"	
											
			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_FACT_Costs_VTU_Final					WHERE scenario = '{scenario}' AND year = '{year}' {factroyFilter}",True)							
			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_FACT_Costs_VTU_Final_Local 			WHERE scenario = '{scenario}' AND year = '{year}' {factroyFilter}",True)			
			
			Dim dicInsert As New Dictionary (Of String, List(Of String)) From {
				{"New_Local", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail_GM"	, "Local"	, 	"XFC_PLT_FACT_Costs_VTU_Final_Local"}}, _
				{"New_Eur", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail_GM"	, "EUR"		, 	"XFC_PLT_FACT_Costs_VTU_Final"}}			
			}
			
			'OLD TABLES
			
'			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_FACT_Costs_VTU_Report_Account 		WHERE scenario = '{scenario}' AND year = '{year}' {factroyFilter}",True)							
'			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_FACT_Costs_VTU_Report_Account_Local 	WHERE scenario = '{scenario}' AND year = '{year}' {factroyFilter}",True)		

'			Dim dicInsert As New Dictionary (Of String, List(Of String)) From {
'				{"Old_Local", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail"		, "Local"	, 	"XFC_PLT_FACT_Costs_VTU_Report_Account_Local"}}, _
'				{"Old_Eur", 	New List(Of String) From {"VTU_data_All_Factories_All_Months_AccountDetail"		, "EUR"		, 	"XFC_PLT_FACT_Costs_VTU_Report_Account"}} 			
'			}

			For Each key As String In dicInsert.Keys
				Dim sql_VTU_data As String = AllQueries(si, dicInsert(key)(0), "", month, year, scenario, "", "", "YTD", "Existing", "", dicInsert(key)(1),)
			
				sql_VTU_data = sql_VTU_data & 
					$"
					INSERT INTO {dicInsert(key)(2)}
						(
						scenario
						, year
						, id_factory
						, date
						, id_product
						, id_averagegroup
						, account_type
						, id_account
						, cost_fixed
						, cost_variable
						, cost
						, volume
						, activity_UO1
						, activity_UO1_total) 
					
					SELECT
						'{scenario}' as scenario
						, YEAR(date) as year
						, id_factory
						, date
						, id_product
						, id_averagegroup
						, account_type
						, id_account
						, cost_fixed
						, cost_variable
						, cost
						, volume
						, activity_UO1
						, activity_UO1_total 
				
					FROM factVTU
					WHERE 1=1
						{factroyFilter}
					"
					
					BRApi.Database.ExecuteSql(dbcon,sql_VTU_data,True)
			Next	
			
		
		#End Region 
		
		End Using
		
		Return Nothing
			
	End Function	
	
	#End Region
	
	#Region "Upadate Nomenclature - Report"
	
	Public Function update_Nomenclature_Report(ByVal si As SessionInfo, _
								ByVal scenario As String, _
								ByVal year As String, _
								ByVal month As String, _
								Optional currency As String = "NO NEED", _
								Optional factory As String = ""
								)
								
		Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
		
			Dim sMonthFilter 		As String = If(month="" Or scenario.Contains("B") Or scenario.Contains("R"), "", $" AND MONTH(date)>={month} ")
			Dim sMonthFilter_Target As String = If(month="" Or scenario.Contains("B") Or scenario.Contains("R"), "", $" AND MONTH(target_date)>={month} ")
			Dim sFactoryFilter 		As String = If(factory="", "", $" AND id_factory='{factory}' ")
			
			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_HIER_Nomenclature_Date_Report WHERE scenario = '{scenario}' AND year = '{year}' {sMonthFilter} {sFactoryFilter}",True)	
			
			Dim sql_Product_Component_Report As String = AllQueries(si, "Product_Component_Report", "", month, year, scenario, "", "", "", "Existing", "", currency).Replace(",Months AS (","WITH Months as (")
			sql_Product_Component_Report =
				$"		
			
				{sql_Product_Component_Report}
			
				INSERT INTO XFC_PLT_HIER_Nomenclature_Date_Report 
					( scenario
					, year
					, id_factory		
					, date			
					, id_product_final
					, id_product		
					, id_component	
					, coefficient		
					, exp_coefficient	
					, Level	) 
				
				SELECT
					'{scenario}'
					, '{year}' as Year
					, id_factory		
					, DATEFROMPARTS({year},MONTH(target_date),1)
					, id_product_final
					, id_product		
					, id_component	
					, coefficient		
					, exp_coefficient	
					, Level							
				FROM Product_Component_Production
				WHERE 1=1
					{sMonthFilter_Target}
					{sFactoryFilter}
				"
			' BRAPI.ErrorLog.LogMessage(si, sql_Product_Component_Report)
			BRApi.Database.ExecuteSql(dbcon,sql_Product_Component_Report,True)
			
		End Using
		
		Return Nothing
			
	End Function	
	
	#End Region
	
	#Region "Upadate Nomenclature - Planning"
	
	Public Function update_Nomenclature_Planning(ByVal si As SessionInfo, _
								ByVal scenario As String, _
								ByVal year As String, _
								Optional month As String = "", _
								Optional factory As String = "All"
								)
		
		Dim filterFactory As String = If(factory="All", "", "AND id_factory='{factory}'")
		
		Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			BRApi.Database.ExecuteSql(dbcon,$"DELETE FROM XFC_PLT_HIER_Nomenclature_Date_Planning WHERE scenario = '{scenario}' {filterFactory}",True)	
			
			Dim sql_nomenclature_planning As String =
				$"		
			
					INSERT INTO XFC_PLT_HIER_Nomenclature_Date_Planning (
						scenario
						, id_factory
						, id_product
						, id_component
						, start_date
						, end_date
						, coefficient
						, prorata
					)
				
					SELECT
						'{scenario}' as scenario
						, id_factory
						, id_product
						, id_component
						, start_date
						, end_date
						, coefficient
						, prorata
				
					FROM XFC_PLT_HIER_Nomenclature_Date
				
					WHERE 1=1
						AND {year} BETWEEN YEAR(start_date) AND YEAR(end_date)  
						{filterFactory}
				"			
			BRApi.Database.ExecuteSql(dbcon,sql_nomenclature_planning,True)
			
		End Using
		
		Return Nothing
			
	End Function	
	
	#End Region
	
	#Region "Upadate Log"
	Public Function update_Log(
								ByVal si As SessionInfo, _
								ByVal scenario As String,
								ByVal id_factory As String,
								ByVal objectID As String,
								ByVal action As String
							)
		Dim actualDate As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")		
		Dim sql_log As String =
				$"
				INSERT INTO XFC_PLT_AUX_Log (scenario, id_factory, [object], action, user_name, date)
				VALUES ('{scenario}', '{id_factory}', '{objectID}', '{action}', '{si.UserName}','{actualDate}');
				"
				brapi.ErrorLog.LogMessage(si, sql_log)
		Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
			BRApi.Database.ExecuteSql(dbcon,sql_log,True)
		End Using
		Return Nothing
	
	End Function
	#End Region
	
	#Region "Get Year Ref"
	Public Function getYearRef(
								ByVal si As SessionInfo, _
								ByVal scenario As String,
								ByVal scenarioRef As String,
								ByVal year As String
							) As String
							
		Dim yearRef As String = year
		If (scenarioRef.Contains("PrevYear")) Then
			yearRef = (CInt(year)-1).ToString
		ElseIf (scenario.StartsWith("Budget")) Then
			If (scenario.StartsWith("RF") Or scenario = "Actual")
				yearRef = (CInt(year)-1).ToString
			End If
		End If
	
		Return yearRef
	End Function
	#End Region		
	
	End Class
	
End Namespace