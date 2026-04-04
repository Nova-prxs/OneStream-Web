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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.historical_data
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						Dim sql As String = $"
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
								
							INSERT INTO XFC_PLT_AUX_TimePresence 
								(
									scenario,
								    year,
								    month,
								    company_id,
								    company_desc,
								    employee_id,
								    employee_name,
								    costcenter_id,
								    job_category,
								    location,
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
								    headcount
								)
								SELECT 
								    scenario,
								    year,
								    id_entity,
								    company_desc,
								    employee_id,
								    employee_name,
								    id_costcenter,
									CASE 
										WHEN id_labor_type = 'Directs' 		THEN 'Direct'
										WHEN id_labor_type = 'Indirects' 	THEN 'Indirect'
									END AS id_labor_type
									,
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
						
						) AS Unpivoted;
						"
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						'Add External Members
						Dim externalMembers As New List(Of NameValuePair)
						externalMembers.Add(New NameValuePair("YourMember1Name","YourMember1Value"))
						externalMembers.Add(New NameValuePair("YourMember2Name","YourMember2Value"))
						Return externalMembers
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		Private Sub ExecuteActionSQL(ByVal si As SessionInfo, ByVal sqlCmd As String)        
			Dim dt As DataTable = Nothing
	        Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)                           
	            BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, False)			
	        End Using   
	    End Sub
		
		Private Function ExecuteSQL(ByVal si As SessionInfo, ByVal sqlCmd As String)        
			Dim dt As DataTable = Nothing
	        Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)                           
	            dt = BRAPi.Database.ExecuteSql(dbConnApp, sqlCmd, True)			
	        End Using   
			Return dt	
	    End Function
	End Class
End Namespace
