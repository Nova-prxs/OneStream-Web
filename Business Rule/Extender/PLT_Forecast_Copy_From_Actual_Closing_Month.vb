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

Namespace OneStream.BusinessRule.Extender.PLT_Forecast_Copy_From_Actual_Closing_Month
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sScenario As String = "RF06"
				Dim sYear As String = "2025"
				Dim sMonthClosing As String = "5"				
				
			#Region "Copy Volumes"
				
				Dim sQuery_Volumes As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_Production_Planning_Volumes
					WHERE scenario = '{sScenario}'
					AND YEAR(date) = '{sYear}'
					AND MONTH(date) = {sMonthClosing}
				
					-- 2. Insert closed months
				
					INSERT INTO XFC_PLT_AUX_Production_Planning_Volumes
					(
						scenario
						,date
						,id_factory
						,id_costcenter
						,id_averagegroup
						,id_product
						,comment
						,value
						,distribution
					)
				
					SELECT
						'{sScenario}' AS scenario
						,DATEFROMPARTS({sYear},MONTH(F.date),1) AS date
						,F.id_factory AS id_factory
						,F.id_costcenter AS id_costcenter
						,F.id_averagegroup AS id_averagegroup
						,F.id_product AS id_product
						,NULL AS comment						
						,SUM(F.volume) AS volume
						,NULL
												
					FROM XFC_PLT_FACT_Production F
					WHERE F.scenario = 'Actual'
		
					AND YEAR(F.date) = {sYear}
					AND MONTH(F.date) = {sMonthClosing}
		
					AND F.uotype = 'UO1'
		
					GROUP BY 
						DATEFROMPARTS({sYear},MONTH(F.date),1)
						,F.id_factory
						,F.id_costcenter
						,F.id_averagegroup
						,F.id_product
							
				
					UPDATE P
						SET P.distribution = A.distribution
					FROM XFC_PLT_AUX_Production_Planning_Volumes P
					INNER JOIN XFC_PLT_AUX_Production_Planning_Volumes A
						ON P.id_factory = A.id_factory
						AND P.id_product = A.id_product
						AND P.id_averagegroup = A.id_averagegroup
						AND P.id_costcenter = A.id_costcenter
						AND MONTH(A.date) = 12
					WHERE P.scenario = '{sScenario}'
					AND YEAR(P.date) = {sYear}
					AND MONTH(P.date) = {sMonthClosing}
				"
			
				ExecuteSql(si, sQuery_Volumes)
				
			#End Region
			
			#Region "Copy & Initialization Times"
				
				Dim sQuery_Times As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_Production_Planning_Times
					WHERE scenario = '{sScenario}'
					AND YEAR(date) = '{sYear}'
					AND MONTH(date) = {sMonthClosing}
				
					-- 2. Insert closed months
				
					INSERT INTO XFC_PLT_AUX_Production_Planning_Times
					(
						scenario
						,date
						,id_factory
						,id_costcenter
						,id_averagegroup
						,id_product
						,comment
						,uotype
						,value
					)
				
					SELECT 
						'{sScenario}' AS scenario
						,DATEFROMPARTS({sYear},MONTH(F.date),1) AS date
						,F.id_factory AS id_factory
						,F.id_costcenter AS id_costcenter
						,F.id_averagegroup AS id_averagegroup
						,F.id_product AS id_product
						,NULL AS comment
						,F.uotype as uotype	
						,SUM(F.activity_taj) / NULLIF(SUM(F.volume),0) * 60 AS allocation
												
					FROM XFC_PLT_FACT_Production F
					WHERE scenario = 'Actual'
			
					AND YEAR(date) = {sYear}
					AND MONTH(date) = {sMonthClosing}						
						
					------------------------------------------------
					-- 09/09/2025
					-- Comentado para que si que tenga en cuenta los registros con volumen 0 
					-- añadidos para corregir la actividad. 
					-- Portugal S4H
					------------------------------------------------
				
					-- AND volume <> 0 
					-- AND allocation_taj <> 0				
		
					GROUP BY 
						DATEFROMPARTS({sYear},MONTH(F.date),1)
						,F.id_factory
						,F.id_costcenter
						,F.id_averagegroup
						,F.id_product				
						,F.uotype
							
				"
				
				ExecuteSql(si, sQuery_Times)
				
				#Region "Custom"
				
'				Dim sMonthLastClosing As String = (CInt(sMonthFirstForecast)-1).ToString
'				Dim sQuery_Times_Init As String = String.Empty								
				
'				For iMonth As Integer = sMonthFirstForecast To 12
			 
'					sQuery_Times_Init = $"
					
'					-- 3. Initialization forecast months
				
'					INSERT INTO XFC_PLT_AUX_Production_Planning_Times
'					(
'						scenario
'						,date
'						,id_factory
'						,id_costcenter
'						,id_averagegroup
'						,id_product
'						,comment
'						,uotype
'						,value
'					)
'					SELECT
'						scenario
'						,DATEFROMPARTS({sYear},{iMonth.ToString},1) AS date
'						,id_factory
'						,id_costcenter
'						,id_averagegroup
'						,id_product
'						,comment
'						,uotype
'						,value
'					FROM XFC_PLT_AUX_Production_Planning_Times
					
'					WHERE scenario = '{sScenario}'
'					AND YEAR(date) = {sYear}
'					AND MONTH(date) = {sMonthLastClosing}
'					"
				
					'BRApi.ErrorLog.LogMessage(si, sQuery_Times_Init)
					'ExecuteSql(si, sQuery_Times_Init)
					
'				Next		
					
				#End Region
				
			#End Region
			
			#Region "Copy Production Fact Data"
				
				Dim sQuery_Fact As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_FACT_Production
					WHERE scenario = '{sScenario}'
					AND YEAR(date) = '{sYear}'
					AND MONTH(date) = {sMonthClosing}
				
					-- 2. Insert closed months
				
					INSERT INTO XFC_PLT_FACT_Production 
					(
						scenario
						,date
						,id_factory
						,id_costcenter
						,id_averagegroup
						,id_workcenter
						,id_product
						,uotype
						,volume
						,allocation_taj
						,activity_taj
						,allocation_tso
						,activity_tso
					)
								
					SELECT 
						'{sScenario}' AS scenario
						,DATEFROMPARTS(YEAR(F.date), MONTH(F.date), 1) AS date
						--,DATEFROMPARTS(2025, 1, 1) AS date
						,id_factory AS id_factory
						,id_costcenter AS id_costcenter
						,id_averagegroup AS id_averagegroup
						,'-1' AS id_workcenter
						,id_product AS id_product
						,uotype AS uotype
						,SUM(volume) AS volume
						,SUM(activity_taj) / NULLIF(SUM(volume),0) AS allocation_taj
						,SUM(activity_taj) AS activity_taj
						--,SUM(activity_tso) / NULLIF(SUM(volume),0) AS allocation_tso
						--,SUM(activity_tso) AS activity_tso
						,NULL AS allocation_tso 
						,NULL AS activity_tso
				
					FROM XFC_PLT_FACT_Production F
														
					WHERE F.scenario = 'Actual'
				
					AND YEAR(F.date) = {sYear}
					AND MONTH(F.date) = {sMonthClosing}								
				
					AND F.volume <> 0 
				
					GROUP BY
						DATEFROMPARTS(YEAR(F.date), MONTH(F.date), 1)
						,id_factory
						,id_costcenter
						,id_averagegroup						
						,id_product
						,uotype
				"
						
				ExecuteSql(si, sQuery_Fact)						
				
			#End Region	
			
			#Region "Copy Activity Index Adjust"
				
				Dim sQuery_ActivityIndex As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_ActivityIndex_Adjust
					WHERE scenario = '{sScenario}'
					AND YEAR(date) = '{sYear}'
					AND MONTH(date) = {sMonthClosing}
				
					-- 2. Insert closed months
				
					INSERT INTO XFC_PLT_AUX_ActivityIndex_Adjust
					(
						scenario
						,scenario_ref
						,date
						,id_factory
						,id_averagegroup
						,uotype
						,comment
						,value
					)
				
					SELECT
						'{sScenario}' AS scenario
						,scenario_ref
						,date
						,id_factory
						,id_averagegroup
						,uotype
						,comment
						,value
												
					FROM XFC_PLT_AUX_ActivityIndex_Adjust F
				
					WHERE F.scenario = 'Actual'
					AND YEAR(F.date) = {sYear}
					AND MONTH(F.date) = {sMonthClosing}		
				"
			
				ExecuteSql(si, sQuery_ActivityIndex)
				
			#End Region			
			
			#Region "Copy Costs"
			
				Dim sQuery_Cost As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_FACT_Costs
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) = {sMonthClosing}
					
						-- 2. Insert closed months
					
						INSERT INTO XFC_PLT_FACT_Costs
						(
							scenario
							, id_factory
							, date
							, id_costcenter
							, id_account
							, id_rubric
							, value									
						)
					
						SELECT
							'{sScenario}' AS scenario
							, id_factory
							, date		
							, id_costcenter
							, id_account
							, id_rubric
							, value
						
						FROM XFC_PLT_FACT_Costs
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) = {sMonthClosing}
				"
				
				ExecuteSql(si, sQuery_Cost)		
				
			#End Region			
			
			#Region "Copy Costs Distribution"
			
				Dim sQuery_CostDist As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_FACT_CostsDistribution
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) = {sMonthClosing}
					
						-- 2. Insert closed months
					
						INSERT INTO XFC_PLT_FACT_CostsDistribution
						(
							scenario
							, id_factory
							, date
							, id_costcenter
							, id_account
							, id_averagegroup
							, id_product
							, type_tso_taj
							, value									
						)
					
						SELECT
							'{sScenario}' AS scenario
							, id_factory
							, date		
							, id_costcenter
							, id_account
							, id_averagegroup
							, id_product
							, type_tso_taj
							, value
						
						FROM XFC_PLT_FACT_CostsDistribution
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) = {sMonthClosing}
				"
				
				ExecuteSql(si, sQuery_CostDist)		
				
			#End Region
			
			#Region "Copy % Variable"
			
				Dim sQuery_Var As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_FixedVariableCosts
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) = {sMonthClosing}
					
						-- 2. Insert closed months
					
						INSERT INTO XFC_PLT_AUX_FixedVariableCosts
						(
							scenario
							, id_factory
							, date
							, id_account
							, costnature
							, value									
						)
					
						SELECT
							'{sScenario}' AS scenario
							, id_factory
							, date
							, id_account
							, costnature
							, value		
						
						FROM XFC_PLT_AUX_FixedVariableCosts
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) = {sMonthClosing}
				"
				
				ExecuteSql(si, sQuery_Var)		
				
			#End Region
			
			#Region "Copy Effects"
			
				Dim sQuery_Eff As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_EffectsAnalysis
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) = {sMonthClosing}
					
						-- 2. Insert closed months
					
						INSERT INTO XFC_PLT_AUX_EffectsAnalysis
						(
							scenario
							, ref_scenario
							, id_factory
							, date
							, id_indicator
							, variability
							, cost_type
							, value									
						)
					
						SELECT
							'{sScenario}' AS scenario
							, ref_scenario
							, id_factory
							, date
							, id_indicator
							, variability
							, cost_type
							, value		
						
						FROM XFC_PLT_AUX_EffectsAnalysis
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) = {sMonthClosing}
				"
				
				ExecuteSql(si, sQuery_Eff)		
				
			#End Region	
			
			#Region "Copy Energy"
			
				Dim sQuery_Ene As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_EnergyVariance
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) = {sMonthClosing}
					
						-- 2. Insert closed months
					
						INSERT INTO XFC_PLT_AUX_EnergyVariance
						(
							scenario
							, id_factory
							, date
							, indicator
							, energy_type
							, value									
						)
					
						SELECT
							'{sScenario}' AS scenario
							, id_factory
							, date
							, indicator
							, energy_type
							, value		
						
						FROM XFC_PLT_AUX_EnergyVariance
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) = {sMonthClosing}
				"
				
				ExecuteSql(si, sQuery_Ene)		
				
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