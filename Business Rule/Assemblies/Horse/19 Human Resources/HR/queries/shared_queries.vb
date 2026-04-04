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
	Public Class shared_queries
		
		#Region "IntelliSense - Copiar y Pegar"
''		Public Enum queryName		
''		    Test
''		    ALL_TR
''		    RAW_HC
''		    FTE
''		    Scenario_Comparison_Factory
''		    HC_FTE_Comparison
''			Historical_Data
'		End Enum
'		''' <summary>
'		''' Ejecuta la función query() de shared_queries con IntelliSense.
'		''' </summary>
'		''' <param name="si">Información de sesión actual.</param>
'		''' <param name="queryName">Nombre de la query.</param>
'		''' <param name="sMonth">Mes (opcional).</param>
'		''' <param name="sYear">Año (opcional).</param>
'		''' <param name="sScenario">Escenario (opcional).</param>
'		''' <param name="sScenarioRef">Escenario de referencia (opcional).</param>
'		''' <param name="sTestParam">Parámetro de prueba (opcional).</param>
'		''' <returns>SQL With con el nombre del queryName.</returns>
'		Public Function SharedQuery(
'		    ByVal si As SessionInfo,
'		    ByVal queryName As String,
'		    Optional sMonth As String = Nothing,
'		    Optional sYear As String = Nothing,
'		    Optional sScenario As String = Nothing,
'		    Optional sScenarioRef As String = Nothing,
'		    Optional sTestParam As String = Nothing
'		) As String
		
'		    Dim sq As New Workspace.__WsNamespacePrefix.__WsAssemblyName.shared_queries
'		    Return sq.query(si, queryName, sMonth, sYear, sScenario, sScenarioRef, sTestParam)
		
'		End Function
		#End Region

        Public Function query(
								ByVal si As SessionInfo, 
								ByVal queryName As String, 
								Optional sMonth As String = Nothing, 
								Optional sYear As String = Nothing,
								Optional sScenario As String = Nothing, 
								Optional sScenarioRef As String = Nothing,
								Optional sEntity As String = Nothing,
								Optional sCurrency As String = Nothing,
								Optional sView As String = Nothing,
								Optional sAccount As String = Nothing,		
								Optional sMonthRef As String = Nothing,									
								Optional sTestParam As String = Nothing								
							 ) As String
            Try
				Dim sql As String = String.Empty
				
				If queryName = "Test" Then
					
					sql= $"SELECT '{sTestParam}' as Message"
				
				#Region "All Transformation Rules"
				
				Else If queryName = "ALL_TR" Then
					sql = $"
						WITH ALL_TR as (
							SELECT 
								  SRG.RuleGroupName
								, SR.RuleName
								, SR.OutputValue
								, M.Description
							FROM StageRuleGroups SRG
							LEFT JOIN StageRules SR
								ON SR.RulesGroupKey = SRG.UniqueId
							LEFT JOIN Member M
								ON M.Name = SR.OutputValue
							WHERE 1=1
								AND SRG.RuleGroupName IN ('TR_Account_HeadCounts', 'TR_CostCenter', 'TR_Product_Labor_Cost_Type')	
						)
					
						, PIVOT_OUTPUT AS (
						    SELECT 
						          RuleName
						        , [TR_Account_HeadCounts]     	AS Account_HeadCounts_Value
						        , [TR_CostCenter]             	AS CostCenter_Value
						        , [TR_Product_Labor_Cost_Type] 	AS Product_Labor_Cost_Type_Value
						    FROM (
						        SELECT 
						              RuleName
						            , RuleGroupName
						            , OutputValue
						        FROM ALL_TR
						    ) AS SourceTable
						    PIVOT (
						        MAX(OutputValue)
						        FOR RuleGroupName IN (
						            [TR_Account_HeadCounts],
						            [TR_CostCenter],
						            [TR_Product_Labor_Cost_Type]
						        )
						    ) AS P1
						)

						, PIVOT_DESC AS (
						    SELECT 
						          RuleName
						        , [TR_Account_HeadCounts]     	AS Account_HeadCounts_Desc
						        , [TR_CostCenter]             	AS CostCenter_Desc
						        , [TR_Product_Labor_Cost_Type] 	AS Product_Labor_Cost_Type_Desc
						    FROM (
						        SELECT 
						              RuleName
						            , RuleGroupName
						            , Description
						        FROM ALL_TR
						    ) AS SourceTable
						    PIVOT (
						        MAX(Description)
						        FOR RuleGroupName IN (
						            [TR_Account_HeadCounts],
						            [TR_CostCenter],
						            [TR_Product_Labor_Cost_Type]
						        )
						    ) AS P2
						)

						, PIVOT_ALL_TR AS (
						    SELECT 
						        v.RuleName							AS [id_costcenter]
						        , v.Account_HeadCounts_Value 		AS [HCType]
						        , v.Product_Labor_Cost_Type_Value 	AS [CostType]
						        , v.CostCenter_Value 				AS [CCGroup]
						        , d.CostCenter_Desc  				AS [CCGroup_Desc]
						        -- , d.Account_HeadCounts_Desc
						        -- , d.Product_Labor_Cost_Type_Desc
					
						    FROM PIVOT_OUTPUT v
						    LEFT JOIN PIVOT_DESC d
						        ON v.RuleName = d.RuleName
						)
					"
				#End Region
				
				#Region "Raw HeadCounts Data"
				
				Else If queryName = "RAW_HC" Then
					
					Dim sqlTR = query(si, "ALL_TR")
					sql = $"
						
					{sqlTR}
					
					, RAW_HC as (
						SELECT
							  F.scenario
							, F.month
							, F.company_id
							, F.company_desc
							, F.costcenter_id
							, ISNULL(F.costcenter_id,'-')+' - '+ ISNULL(MC.description,'No Description') as cc_description
							, F.job_category
							, F.location
							, F.location_country
							, F.gender
							, F.global_function_organization
							, MC.type, MC.nature
							, TR.HCType
							, TR.CostType
							, ISNULL(TR.CCGroup_Desc, 'NONE') as CCGroup
							, SUM(F.headcount) as headcount
					
						FROM XFC_MAIN_FACT_HeadCount F
					
						LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist MC
							ON MC.id = F.costcenter_id
						 	AND '2025-' + CONVERT(VARCHAR(50), F.month) + '-01' BETWEEN start_date AND end_date
					
						LEFT JOIN PIVOT_ALL_TR TR
							ON TR.id_costcenter = F.costcenter_id
							
						WHERE 1=1
							AND F.year = {sYear}
							-- AND F.month = {sMonth}
					
						GROUP BY  
							  F.scenario
							, F.month
							, F.company_id
							, F.company_desc
							, F.costcenter_id
							, F.job_category
							, F.location
							, F.location_country
							, F.gender
							, F.global_function_organization
							, MC.description
							, MC.type
							, MC.nature
							, TR.HCType
							, TR.CostType
							, TR.CCGroup_Desc
					)
					"
					
				#End Region	
				
                #Region "FTE"
				
				Else If queryName="FTE_RAW" Then
					
					Dim queries_PCT As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
					Dim sql_FTE As String = queries_PCT.AllQueries(si, "FTE","",sMonth, sYear, sScenario, "", "", "",,,"")
					  				  
					sql = $"				  				  			
					
						{sql_FTE}
					
						, FTE_RAW AS (
							SELECT 
								  id_factory
								, description AS Factory
								, CONVERT(INT, REPLACE(month,'M','')) AS month
								, wf_type
								, id_costcenter
								, description1 AS [Hours Indicator]
								, shift AS Shift
								, type AS [Type Cost Center]		
								, hours AS [Hours]									
								, worked_days AS [Days by Shift]
								, hours_shift AS [Hours by Shift]
								, FTE
					
							FROM FTE_data
						)
					
					"
									
				Else If queryName = "FTE" Then
					
					Dim sql_Raw = query(si, "FTE_RAW",,sYear, sScenario,,)  
					sql = sql_Raw & $"	
					
						, FTE_1 as (
							SELECT
								  month
								, id_factory, Factory, [Hours Indicator]
								, id_costcenter, SUM(FTE) AS FTE_1, 0 AS FTE_2
							FROM FTE_RAW
							WHERE 1=1
								AND wf_type = 1								
							GROUP BY 
								  month
								, id_factory, Factory, [Hours Indicator]
								, id_costcenter
						)
					
						, FTE_2 as (
							SELECT
								  month
								, id_factory, Factory, [Hours Indicator]
								, id_costcenter, 0 AS FTE_1,  SUM(FTE) AS FTE_2
							FROM FTE_RAW
							WHERE 1=1
								AND wf_type = 2								
							GROUP BY 
								  month
								, id_factory, Factory, [Hours Indicator]
								, id_costcenter
						)
					
						, FTE as (
							SELECT 
								month, id_factory, Factory, [Hours Indicator] as HoursIndicator, id_costcenter, SUM(FTE_1) AS FTE_1, SUM(FTE_2) AS FTE_2
							FROM (
								SELECT * FROM FTE_1
								UNION ALL
								SELECT * FROM FTE_2
							) A
							GROUP BY 
								month, id_factory, Factory, [Hours Indicator], id_costcenter							
						)
					"
					
				#End Region								

				#Region "Scenario Comparison"
				
				Else If queryName = "Scenario_Comparison" Then
					
					Dim sqlRaw = query(si, "RAW_HC", sMonth, sYear)	
					
					sql = $"
						{sqlRaw}
					
						, filteredRAW as (
							SELECT *
							FROM RAW_HC
							WHERE 1=1
								AND (scenario = 'Actual' OR scenario = '{sScenarioRef}')
								AND month = {sMonth}
						)
					
						, Scenario_Comparison AS (
						    SELECT 
						        company_id
						        , company_desc
						        , costcenter_id
						        , job_category
						        , location
						        , location_country
						        , gender
						        , global_function_organization
						        , type
						        , nature
						        , HCType
						        , CostType
						        , CCGroup
						        , ISNULL([Actual], 0) AS Actual_Headcount
						        , ISNULL([{sScenarioRef}], 0) AS Ref_Headcount
						    FROM (
						        SELECT 
						            company_id
						            , company_desc
						            , costcenter_id
						            , job_category
						            , location
						            , location_country
						            , gender
						            , global_function_organization
						            , type
						            , nature
						            , HCType
						            , CostType
						            , CCGroup
						            , scenario
						            , headcount
						        FROM filteredRAW
						    ) AS SourceTable
						    PIVOT (
						        SUM(headcount)
						        FOR scenario IN ([Actual], [{sScenarioRef}])
						    ) AS Pivoted
						)
					"

					
				#End Region	
				
				#Region "HC vs FTE Comparison"
				
				Else If queryName = "HC_FTE_Comparison" Then
					
					Dim sqlRaw = query(si, "RAW_HC", sMonth, sYear).Replace("WITH ", ", ")	
					Dim sqlFTE = query(si, "FTE", sMonth, sYear, sScenario, sScenarioRef)	
					sql = $"
					
						{sqlFTE}
					
						{sqlRaw}
					
						, RAW_HC_CC AS (
							SELECT
								  month
								, company_id
								, company_desc
								, type
								, nature
								, job_category	-- Dif. el FTE en 1/2
								, HCType		-- Propiedad CC?
								, CostType		-- Propiedad CC?
								, CCGroup
								, costcenter_id
								, cc_description
								, SUM(headcount) AS headcount 
							FROM RAW_HC
							GROUP BY
								  month
								, company_id
								, company_desc
								, type
								, nature
								, job_category
								, HCType
								, CostType
								, CCGroup
								, costcenter_id
								, cc_description
						)
					
						, HC_FTE_Comparison AS (
							SELECT 
								  COALESCE(A.month			, B.month		) AS month
								, COALESCE(A.company_id		, 0671 			) AS company_id
								, COALESCE(A.company_desc	, '0'			) AS company_desc
								, COALESCE(A.type			, '0'			) AS type	
								, COALESCE(A.nature			, '0'			) AS nature
								, COALESCE(A.job_category	, '0'			) AS job_category
								, COALESCE(A.HCType			, '0'			) AS HCType
								, COALESCE(A.CostType		, '0'			) AS CostType
								, COALESCE(A.CCGroup		, '0'			) AS CCGroup
								, COALESCE(A.costcenter_id  , '0'			) AS costcenter_id
								, COALESCE(A.cc_description , '0'			) AS cc_description
								, COALESCE(A.headcount		, '0'			) AS headcount
								, B.HoursIndicator 							  AS HoursIndicator
								, B.FTE_1									  AS FTE_1
								, B.FTE_2									  AS FTE_2
					
							FROM RAW_HC_CC A
							FULL OUTER JOIN FTE B
								ON B.id_costcenter = A.costcenter_id
								AND CONVERT(INTEGER, B.month) = CONVERT(INTEGER, A.month)
								-- AND B.id_factory = A.company_id
					
							WHERE 1=1
								-- AND B.HoursIndicator = 'theoretical_working_hours'
						)
					"
					
					BRApi.ErrorLog.LogMessage(si, sql)
					
				#End Region
				
				#Region "Historical Data"
				
				Else If queryName = "Historical_Data" Then
					sql=$"
						WITH ALL_TR as (
								 SELECT 
									    SRG.RuleGroupName
									  , SR.RuleName
									  , SR.OutputValue
									  , M.Description
								 FROM StageRuleGroups SRG
								 LEFT JOIN StageRules SR
								  	ON SR.RulesGroupKey = SRG.UniqueId
								 LEFT JOIN Member M
								  	ON M.Name = SR.OutputValue
								 WHERE 1=1
								  	AND SRG.RuleGroupName IN ('TR_Account_HeadCounts', 'TR_CostCenter', 'TR_Product_Labor_Cost_Type') 
								)
							, PIVOT_OUTPUT AS (
								 SELECT 
									    RuleName
									  , [TR_Account_HeadCounts]      AS Account_HeadCounts_Value
									  , [TR_CostCenter]              AS CostCenter_Value
									  , [TR_Product_Labor_Cost_Type]  AS Product_Labor_Cost_Type_Value
								 FROM (
								  SELECT 
									     RuleName
									   , RuleGroupName
									   , OutputValue
								  FROM ALL_TR
								 ) AS SourceTable
								 PIVOT (
								  MAX(OutputValue)
								  FOR RuleGroupName IN (
									   [TR_Account_HeadCounts],
									   [TR_CostCenter],
									   [TR_Product_Labor_Cost_Type]
								  )
								 ) AS P1
							)
								
							, PIVOT_DESC AS (
								 SELECT 
									    RuleName
									  , [TR_Account_HeadCounts]      AS Account_HeadCounts_Desc
									  , [TR_CostCenter]              AS CostCenter_Desc
									  , [TR_Product_Labor_Cost_Type]  AS Product_Labor_Cost_Type_Desc
								 FROM (
								  SELECT 
									     RuleName
									   , RuleGroupName
									   , Description
								  FROM ALL_TR
								 ) AS SourceTable
								 PIVOT (
								  MAX(Description)
								  FOR RuleGroupName IN (
								   [TR_Account_HeadCounts],
								   [TR_CostCenter],
								   [TR_Product_Labor_Cost_Type]
								  )
								 ) AS P2
							)
								
							, PIVOT_ALL_TR AS (
								 SELECT 
									  v.RuleName       AS [id_costcenter]
									  , v.Account_HeadCounts_Value   AS [HCType]
									  , v.Product_Labor_Cost_Type_Value  AS [CostType]
									  , v.CostCenter_Value     AS [CCGroup]
									  , d.CostCenter_Desc      AS [CCGroup_Desc]
									  -- , d.Account_HeadCounts_Desc
									  -- , d.Product_Labor_Cost_Type_Desc
								 FROM PIVOT_OUTPUT v
								 LEFT JOIN PIVOT_DESC d
								  	ON v.RuleName = d.RuleName
							) 
							
							, Historical_Data as (
								SELECT 
								    scenario,
								    year,
								    id_entity,
								    company_desc,
								    employee_id,
								    employee_name,
								    id_costcenter,
									UD2_Labour_Cost_Type,
									UD3_CostCenter_Group,
									CASE 
										WHEN id_labor_type = 'Directs' 		THEN 'Direct'
										WHEN id_labor_type = 'Indirects' 	THEN 'Indirect'
									END AS id_labor_type,
								    location_country,
								    gender,
								    HR_cluster,
								    shared_services_center,
								    global_function_organization,
								    sup_org_from_top_1,
								    sup_org_from_top_2,
								    sup_org_from_top_3,
								    sup_org_from_top_4,
								    sup_org_from_top_5,
								    sup_org_from_top_6,
								    month,  -- El mes como columna
								    headcount -- El valor del headcount
						
								FROM (
									SELECT									
										'Budget_V2' 		AS scenario
										, 2025 				AS year
										, FHC.company_id	AS id_entity
										, FHC.company_desc	AS company_desc
										, '-1'				AS employee_id
										, '-1'				AS employee_name
										, C.id_costcenter 	AS id_costcenter
										, M2.name 			AS UD2_Labour_Cost_Type
										, M3.name 			AS UD3_CostCenter_Group
										, M1.name 			AS id_labor_type
										, '-1'				AS location_country
										, '-1'				AS gender						
										, '-1'				AS HR_cluster						
										, '-1'				AS shared_services_center						
										, '-1'				AS global_function_organization						
										, '-1'				AS sup_org_from_top_1						
										, '-1'				AS sup_org_from_top_2						
										, '-1'				AS sup_org_from_top_3						
										, '-1'				AS sup_org_from_top_4						
										, '-1'				AS sup_org_from_top_5						
										, '-1'				AS sup_org_from_top_6						
										, M1Value 			AS M01
										, M2Value 			AS M02
										, M3Value 			AS M03
										, M4Value 			AS M04
										, M5Value 			AS M05
										, M6Value 			AS M06
										, M7Value 			AS M07
										, M8Value 			AS M08
										, M9Value 			AS M09
										, M10Value			AS M10
										, M11Value			AS M11 
										, M12Value			AS M12  
									 
									FROM DataRecord2025 D
									LEFT JOIN Member ME
										ON D.EntityId = ME.MemberId
									LEFT JOIN Member M1
										ON D.UD1Id = M1.MemberId  
									LEFT JOIN Member M2
										ON D.UD2Id = M2.MemberId
									LEFT JOIN Member M3
										ON D.UD3Id = M3.MemberId
									
									LEFT JOIN (
											SELECT 
												  [CostType]
												, [CCGroup]
												, MAX([id_costcenter]) AS id_costcenter
											
											FROM PIVOT_ALL_TR 
											GROUP BY 
												  [CostType]
												, [CCGroup] 
									) C
								 		ON M2.name = C.[CostType]
								 		AND M3.name = C.[CCGroup]
						
									INNER JOIN (
										SELECT DISTINCT
											  company_id
											, company_desc
										FROM XFC_MAIN_FACT_Headcount										
									) FHC
										ON 'R'+FHC.company_id = ME.name
						
									WHERE 1=1
									AND YearId = 2025
									AND ScenarioId = 25
									AND AccountId = 7341142
									AND ConsId = 57	
							) AS SourceTable
						
						UNPIVOT (
						    headcount FOR month IN (
						        M01, M02, M03, M04, M05, M06,
						        M07, M08, M09, M10, M11, M12
						    )
						
						) AS Unpivoted
					)
					
					"
					
				#End Region
				
				#Region "Payroll Analysis"
				
				Else If queryName = "Payroll_Analysis" Then
					
					Dim dFactory As New Dictionary(Of String, String) From {
						{"All", ""},
				        {"0585", "R0585"},
						{"0671", "R0671"},
						{"0592", "R0592"},
						{"0611", "R0045106"},
						{"1302", "R0529002"},
						{"1303", "R0483003"},
						{"1300", "Holding"},
						{"1301", "R05489"}					
					}
					
					Dim sFactoryMap As String = dFactory(sEntity)
					
					Dim sMonthFilter As String = If(sView = "Periodic", $"MONTH(FC.date) = {sMonth}", $"MONTH(FC.date) <= {sMonth}")
					
					Dim sScenarioMonthFilter As String 
					Dim sScenarioRefMonthFilter As String 
					
					If(sView = "Periodic") Then
						sScenarioMonthFilter = $"(scenario = '{sScenario}' AND MONTH(date) = {sMonth})"
						sScenarioRefMonthFilter = $"(scenario = '{sScenarioRef}' AND MONTH(date) = {sMonthRef})"
					Else
						sScenarioMonthFilter = $"(scenario = '{sScenario}' AND MONTH(date) <= {sMonth})"
						sScenarioRefMonthFilter = $"(scenario = '{sScenarioRef}' AND MONTH(date) <= {sMonthRef})"
					End If
					
					Dim sAccountActual As String = If (sAccount = "0","'5511051','5511052','552301','5541091','5541092','554401','562101','563101','571101','571102','572101','5621086','5631087'",$"'{sAccount}'")							
																		
					Dim dictAccount As New Dictionary(Of String, String) From {								
						{"5511051","1A81,1A82"},
						{"5511052","2A41"},
						{"552301","1301,1302"},
						{"5541091","2817"},
						{"5541092","2816"},
						{"554401",""},
						{"562101",""},
						{"563101","2601"},
						{"571101","2001"},
						{"571102","2101"},
						{"572101","2701"},
						{"5621086",""},
						{"5631087",""}															
		    		}
					
					Dim sAccountNoActual As String
					If (sAccount = "0") Then
						sAccountNoActual = "'1A81','1A82','2A41','1301','1302','2817','2816','2601','2001','2101','2701'"
					Else
						sAccountNoActual = "'" & dictAccount(sAccount).Replace(",","','") & "'"
					End If
					
					Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries
					Dim sql_Rate = shared_queries.AllQueries(si, "RATE_All_Factories", "", sMonth, sYear, sScenario, sScenarioRef, "", "", "", "", sCurrency)					
					'Dim sql_Aux As String = query(si, "Scenario_Comparison", sMonth, sYear, sScenario, sScenarioRef)	
					Dim sql_FTE As String = shared_queries.AllQueries(si, "FTE","",sMonth, sYear, sScenario, sScenarioRef, "", "",,,"")
					sql = $"
					
						{sql_FTE}	
						
						{sql_Rate}
					
						, Costs_Raw as (
							SELECT *
							FROM XFC_PLT_FACT_Costs 
							WHERE 1=1
							AND id_factory LIKE '{sFactoryMap}%'
							AND ((scenario = 'Actual' AND id_rubric IN ({sAccountActual})) OR (scenario <> 'Actual' AND id_account IN ({sAccountNoActual})))								
							AND YEAR(date) = {sYear}					
							AND value <> 0	
						)
					
						, Costs_Data as (
							SELECT
								scenario_data
								, FC.scenario
								, FC.date
								, FC.id_factory
								, FC.id_costcenter
								, ISNULL(FC.id_costcenter,'-') +' - '+ ISNULL(MC.description,'No Description') as costcenter
								--, MA.account_type
								, MC.type as type
								, SUM(FC.value) * CASE WHEN FC.scenario = '{sScenario}' THEN R1.rate ELSE R2.rate END AS value
					
							FROM (	SELECT *, 'Cost' AS scenario_data FROM Costs_Raw WHERE {sScenarioMonthFilter}	
										UNION ALL								  
									SELECT *, 'Cost_Ref' AS scenario_data FROM Costs_Raw WHERE {sScenarioRefMonthFilter}) FC
					
							LEFT JOIN XFC_PLT_MASTER_Account MA
								ON MA.id = FC.id_account
					
							LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist MC
								ON MC.id = FC.id_costcenter
								AND MC.id_factory = FC.id_factory
								AND FC.date BETWEEN MC.start_date AND MC.end_date							
					
							LEFT JOIN fxRate R1
								ON MONTH(FC.date) = R1.month
								AND FC.id_factory = R1.id_factory
							LEFT JOIN fxRateRef R2
								ON MONTH(FC.date) = R2.month	
								AND FC.id_factory = R2.id_factory
					
							GROUP BY 
								FC.scenario_data
								, FC.scenario
								, FC.date
								, FC.id_factory
								--, MA.account_type
								, FC.id_costcenter	
								, MC.description
								, MC.type
								, R1.rate
								, R2.rate
						)
					
						, Costs_Data_For_Unitary AS (
							SELECT *, 'Cost_Per' + CASE WHEN scenario <> '{sScenario}' THEN '_Ref' ELSE '' END AS scenario_data_per
							FROM Costs_Data
							WHERE MONTH(date) = {sMonth}					
						)
					
						, FTE_Sum AS (
							SELECT 
								scenario
								, id_factory AS id_factory
								, id_costcenter AS id_costcenter
								, SUM(FTE) AS FTE
							FROM FTE_data
							WHERE ((scenario = '{sScenario}' AND REPLACE(month,'M','') = {sMonth}) 
								OR (scenario = '{sScenarioRef}' AND REPLACE(month,'M','') = {sMonthRef}))
							AND description1 = 'Paid Hours'
							GROUP BY 
								scenario
								, id_factory
								, id_costcenter
											
						)
					
						, FTE AS (	
							SELECT 
								'FTE' + CASE WHEN C.scenario <> '{sScenario}' THEN '_Ref' ELSE '' END AS scenario_data
								, C.scenario
								, C.id_factory
								, C.id_costcenter
								, C.costcenter
								--, C.account_type
								, C.type
								, SUM(FTE.FTE) AS value

							FROM Costs_Data C
							INNER JOIN FTE_Sum FTE
								ON FTE.scenario = C.scenario
								AND FTE.id_factory = C.id_factory
								AND FTE.id_costcenter = C.id_costcenter								 							
					
							WHERE FTE.FTE <> 0
					
							GROUP BY 
								C.scenario_data
								, C.scenario
								, C.id_factory
								, C.id_costcenter
								, C.costcenter
								--, C.account_type
								, C.type
						)
					
						, All_Data AS (
							SELECT 
								costcenter
								, id_costcenter
								, type AS CC_Type_Id
								, T.description AS CC_Type
								, T.group1 AS CC_Group1
								, T.group2 AS CC_Group2
								, Cost
								, Cost_Ref 
								, Cost_Per
								, Cost_Per_Ref 					
								, FTE
								, FTE_Ref 
						
							FROM (
								SELECT costcenter, id_costcenter, type, scenario_data, value FROM Costs_Data
								UNION ALL
								SELECT costcenter, id_costcenter, type, scenario_data_per, value FROM Costs_Data_For_Unitary
								UNION ALL
								SELECT costcenter, id_costcenter, type, scenario_data, value FROM FTE
							) AS SourceTable
					    	PIVOT (
					        	SUM(value)
					        	FOR scenario_data IN (Cost, Cost_Ref, Cost_Per, Cost_Per_Ref, FTE, FTE_Ref)
					   	 	) AS Pivoted										
					
							LEFT JOIN XFC_PLT_MASTER_CostCenter_Types T
								ON Pivoted.type = T.id
						)
					"	
					
					BRApi.ErrorLog.LogMessage(si, sql)
					
				#End Region				
				
				End If
				
                Return sql
				
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
        End Function

	End Class
		
End Namespace
