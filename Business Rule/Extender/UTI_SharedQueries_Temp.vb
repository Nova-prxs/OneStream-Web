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

Namespace OneStream.BusinessRule.Extender.UTI_SharedQueries_Temp
	
	Public Class MainClass
	
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object

			Try
				
				Dim sql As String = String.Empty
				
				Dim year As String = "2025"
				
				Dim sScenarioList() As String = {"Actual"}
				Dim sOriginTableList() As String = {"XFC_PLT_FACT_Costs_VTU_Report_Test"}																		
																						
				For Each scenario As String In sScenarioList	
					
					For Each originTable As String In sOriginTableList
				
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
								    FROM {originTable}
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
								)
							
								, OwnBase AS (
								    SELECT
								        '{scenario}' AS scenario
								        , F.id_factory
								        , MONTH(F.date) AS month
										, F.id_averagegroup
								        , F.id_product								        
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
							
								        , SUM(CAST(F.cost_fixed AS DECIMAL(28,10)))   		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   							ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_fix_ytd
								        , SUM(CAST(F.cost_variable AS DECIMAL(28,10))) 		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   							ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_var_ytd
								        , SUM(CAST(F.volume AS DECIMAL(28,10)))        		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   							ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_vol_ytd
								        , SUM(CAST(F.activity_UO1 AS DECIMAL(28,10)))  		OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   							ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1_ytd
								        , SUM(CAST(F.activity_UO1_total AS DECIMAL(28,10))) OVER (PARTITION BY F.id_factory, F.id_averagegroup, F.id_product, F.account_type, F.id_account --, F.cost_center
								                                   							ORDER BY F.date ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS sum_uo1t_ytd
								    
									FROM XFC_PLT_FACT_Costs_VTU_Report_All_Months F
		
								)
								
								,OwnRows AS (				
					
								    SELECT  o.scenario, o.id_factory, o.month, o.id_averagegroup, o.id_product, o.id_account, o.account_type,
								            CASE WHEN v.metric='PER' AND v.variability = 'F' THEN v.value ELSE 0 END AS VTU_Per_Fix,
											CASE WHEN v.metric='PER' AND v.variability = 'V' THEN v.value ELSE 0 END AS VTU_Per_Var,
								            CASE WHEN v.metric='YTD' AND v.variability = 'F' THEN v.value ELSE 0 END AS VTU_YTD_Fix,
											CASE WHEN v.metric='YTD' AND v.variability = 'V' THEN v.value ELSE 0 END AS VTU_YTD_Var
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
					
								,OwnRowsSum AS (
								
									SELECT  scenario, id_factory, month, id_averagegroup, id_product, id_account, account_type,
										SUM(VTU_Per_Fix) AS VTU_Per_Fix,
										SUM(VTU_Per_Var) AS VTU_Per_Var,
										SUM(VTU_YTD_Fix) AS VTU_YTD_Fix,
										SUM(VTU_YTD_Var) AS VTU_YTD_Var
									FROM OwnRows
									GROUP BY scenario, id_factory, month, id_averagegroup, id_product, id_account, account_type
					
								)
					
								-- INSERT INTO {originTable} (scenario, year, id_factory, date, id_product, id_averagegroup, account_type, id_account, cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_Total, VTU_unit_per_fix, VTU_unit_per_var, VTU_unit_ytd_fix, VTU_unit_ytd_var) 
								-- SELECT OWN.scenario, '{year}', OWN.id_factory, DATEFROMPARTS({year},month,1), OWN.id_product, OWN.id_averagegroup, OWN.account_type, OWN.id_account, 0, 0, 0, 0, 0, 0, 0, 0, OWN.VTU_YTD_Fix, OWN.VTU_YTD_Var
								-- FROM OwnRowsSum OWN 
								-- LEFT JOIN {originTable} ORI 
								-- 	ON ORI.scenario = OWN.scenario
								-- 	AND ORI.id_factory = OWN.id_factory
								-- 	AND MONTH(ORI.date) = OWN.month
								-- 	AND ORI.id_product = OWN.id_product
								-- 	AND ORI.id_averagegroup = OWN.id_averagegroup
								-- 	AND ORI.id_account = OWN.id_account
								-- WHERE ORI.scenario IS NULL
								-- AND (OWN.VTU_YTD_Fix <> 0  OR OWN.VTU_YTD_Var <> 0)
						
								UPDATE ORI
									SET VTU_unit_per_fix = VTU_Per_Fix
									  , VTU_unit_per_var = VTU_Per_Var
									  , VTU_unit_ytd_fix = VTU_YTD_Fix
									  , VTU_unit_ytd_var = VTU_YTD_Var
								
								FROM {originTable} ORI
								INNER JOIN OwnRowsSum OWN
									ON ORI.scenario = OWN.scenario
									AND ORI.id_factory = OWN.id_factory
									AND MONTH(ORI.date) = OWN.month
									AND ORI.id_product = OWN.id_product
									AND ORI.id_averagegroup = OWN.id_averagegroup
									AND ORI.id_account = OWN.id_account
								WHERE ORI.scenario = '{scenario}'
								AND ORI.year = '{year}'
						"
					
					Next
					
				Next
				
'				Dim sql2 As String = $"
'					INSERT INTO XFC_PLT_FACT_Costs_VTU_Report_Test
'					SELECT * FROM XFC_PLT_FACT_Costs_VTU_Report_Account_Local WHERE scenario = 'Actual'"
				
				BRApi.ErrorLog.LogMessage(si, "sql: " & sql)			
			
				Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)						
					BRApi.Database.ExecuteSql(oDbConn, sql, False)												
				End Using
				
                Return Nothing
				
            Catch ex As Exception
                Throw New XFException(si, ex)
			End Try
			
        End Function
	
	End Class
	
End Namespace