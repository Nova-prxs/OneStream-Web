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

Namespace OneStream.BusinessRule.Extender.PLT_Forecast_From_Forecast_Actual
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sScenarioSource As String = "RF09"
				Dim sScenarioTarget As String = "RF10"
				
				Dim sYear As String = "2025"
				
				Dim sMonthActualToCopy As String = "9"				
				
			
			#Region "Copy Times"
				
				Dim sQuery_Times As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_Production_Planning_Times
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
				
					-- 2. Insert Forecast Source + Actual Month
				
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
						'{sScenarioTarget}' AS scenario
						,date
						,id_factory
						,id_costcenter
						,id_averagegroup
						,id_product
						,comment
						,uotype
						,value
												
					FROM XFC_PLT_AUX_Production_Planning_Times F
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}
				
					UNION ALL				
												
					SELECT 
						'{sScenarioTarget}' AS scenario
						,DATEFROMPARTS({sYear},MONTH(F.date),1) AS date
						,F.id_factory AS id_factory
						,F.id_costcenter AS id_costcenter
						,F.id_averagegroup AS id_averagegroup
						,F.id_product AS id_product
						,NULL AS comment
						,F.uotype as uotype	
						,SUM(F.activity_taj) / NULLIF(SUM(F.volume),0) AS allocation
												
					FROM XFC_PLT_FACT_Production F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}
					AND MONTH(date) = {sMonthActualToCopy}									
				
					AND volume <> 0 
					-- AND allocation_taj <> 0				
		
					GROUP BY 
						DATEFROMPARTS({sYear},MONTH(F.date),1)
						,F.id_factory
						,F.id_costcenter
						,F.id_averagegroup
						,F.id_product				
						,F.uotype
							
				"			
				
			#End Region
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_Times)	
				'ExecuteSql(si, sQuery_Times)
			
			#Region "Copy Production Fact Data"
				
				Dim sQuery_Fact As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_FACT_Production
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = '{sYear}'
				
					-- 2. Insert Forecast Source + Actual Month
				
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
						'{sScenarioTarget}' AS scenario
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
				
					FROM XFC_PLT_FACT_Production F
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = '{sYear}'
					AND MONTH(date) <> {sMonthActualToCopy}
				
					UNION ALL
				
					SELECT 
						'{sScenarioTarget}' AS scenario
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
					WHERE scenario = 'Actual'
					AND YEAR(date) = '{sYear}'	
					AND MONTH(date) = {sMonthActualToCopy}
				
					AND F.volume <> 0 
				
					GROUP BY
						DATEFROMPARTS(YEAR(F.date), MONTH(F.date), 1)
						,id_factory
						,id_costcenter
						,id_averagegroup						
						,id_product
						,uotype				
				"
	
			#End Region	
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_Fact)
				'ExecuteSql(si, sQuery_Fact)						
			
			#Region "Copy Activity Index Adjust"
				
				Dim sQuery_ActivityIndex As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_ActivityIndex_Adjust
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
				
					-- 2. Insert Forecast Source + Actual Month
				
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
						'{sScenarioTarget}' AS scenario
						,scenario_ref
						,date
						,id_factory
						,id_averagegroup
						,uotype
						,comment
						,value
												
					FROM XFC_PLT_AUX_ActivityIndex_Adjust F
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}	
				
					UNION ALL
				
					SELECT
						'{sScenarioTarget}' AS scenario
						,scenario_ref
						,date
						,id_factory
						,id_averagegroup
						,uotype
						,comment
						,value
												
					FROM XFC_PLT_AUX_ActivityIndex_Adjust F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}	
					AND MONTH(date) = {sMonthActualToCopy}				
				"
			
				
			#End Region				
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_ActivityIndex)
				'ExecuteSql(si, sQuery_ActivityIndex)
			
			#Region "Copy Costs"
			
				Dim sQuery_Cost As String = $"
				
					-- 1. Previous clear
					
					DELETE FROM XFC_PLT_FACT_Costs
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
					
					-- 2. Insert Forecast Source + Actual Month
					
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
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, id_costcenter
						, id_account
						, id_rubric
						, value		
						
					FROM XFC_PLT_FACT_Costs
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}	
				
					UNION ALL
				
					SELECT
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, id_costcenter
						, id_account
						, id_rubric
						, value		
												
					FROM XFC_PLT_FACT_Costs F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}	
					AND MONTH(date) = {sMonthActualToCopy}	
				"
								
			#End Region			
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_Cost)
				'ExecuteSql(si, sQuery_Cost)		
			
			#Region "Copy Costs Distribution"
			
				Dim sQuery_CostDist As String = $"
				
					-- 1. Previous clear
					
					DELETE FROM XFC_PLT_FACT_CostsDistribution
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
					
					-- 2. Insert Forecast Source + Actual Month
					
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
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, id_costcenter
						, id_account
						, id_averagegroup
						, id_product
						, type_tso_taj
						, value			
						
					FROM XFC_PLT_FACT_CostsDistribution
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}	
				
					UNION ALL
				
					SELECT
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, id_costcenter
						, id_account
						, id_averagegroup
						, id_product
						, type_tso_taj
						, value		
												
					FROM XFC_PLT_FACT_CostsDistribution F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}	
					AND MONTH(date) = {sMonthActualToCopy}													
					
				"
								
			#End Region
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_CostDist)			
				'ExecuteSql(si, sQuery_CostDist)		
			
			#Region "Copy % Variable"
			
				Dim sQuery_Var As String = $"
				
					-- 1. Previous clear
					
					DELETE FROM XFC_PLT_AUX_FixedVariableCosts
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
					
					-- 2. Insert Forecast Source + Actual Month
					
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
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, id_account
						, costnature
						, value			
						
					FROM XFC_PLT_AUX_FixedVariableCosts
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}	
				
					UNION ALL
				
					SELECT
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, id_account
						, costnature
						, value	
												
					FROM XFC_PLT_AUX_FixedVariableCosts F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}	
					AND MONTH(date) = {sMonthActualToCopy}
					
				"
								
			#End Region
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_Var)
				'ExecuteSql(si, sQuery_Var)		
			
			#Region "Copy Effects"
			
				Dim sQuery_Eff As String = $"
				
					-- 1. Previous clear
					
					DELETE FROM XFC_PLT_AUX_EffectsAnalysis
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
					
					-- 2. Insert Forecast Source + Actual Month
					
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
						'{sScenarioTarget}' AS scenario
						, ref_scenario
						, id_factory
						, date
						, id_indicator
						, variability
						, cost_type
						, value				
						
					FROM XFC_PLT_AUX_EffectsAnalysis
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}	
				
					UNION ALL
				
					SELECT
						'{sScenarioTarget}' AS scenario
						, ref_scenario
						, id_factory
						, date
						, id_indicator
						, variability
						, cost_type
						, value	
												
					FROM XFC_PLT_AUX_EffectsAnalysis F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}	
					AND MONTH(date) = {sMonthActualToCopy}					
					
				"
								
			#End Region	
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_Eff)
				'ExecuteSql(si, sQuery_Eff)		
			
			#Region "Copy Energy"
			
				Dim sQuery_Ene As String = $"
				
					-- 1. Previous clear
					
					DELETE FROM XFC_PLT_AUX_EnergyVariance
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
					
					-- 2. Insert Forecast Source + Actual Month
					
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
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, indicator
						, energy_type
						, value				
						
					FROM XFC_PLT_AUX_EnergyVariance
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}	
				
					UNION ALL
				
					SELECT
						'{sScenarioTarget}' AS scenario
						, id_factory
						, date
						, indicator
						, energy_type
						, value	
												
					FROM XFC_PLT_AUX_EnergyVariance F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}	
					AND MONTH(date) = {sMonthActualToCopy}										

				"
				
				
			#End Region				
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_Ene)
				'ExecuteSql(si, sQuery_Ene)	
				
			#Region "Copy Hours"
			
				Dim sQuery_Hours As String = $"
				
					-- 1. Previous clear
					
					DELETE FROM XFC_PLT_AUX_DailyHours_Stored
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = {sYear}
					
					-- 2. Insert Forecast Source + Actual Month
					
					INSERT INTO XFC_PLT_AUX_DailyHours_Stored
					(
						scenario
						, date
						, id_costcenter
						, id_factory
						, id_indicator
						, wf_type
						, value								
					)
					
					SELECT
						'{sScenarioTarget}' AS scenario
						, date
						, id_costcenter
						, id_factory
						, id_indicator
						, wf_type
						, value				
						
					FROM XFC_PLT_AUX_DailyHours_Stored
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}
					AND MONTH(date) <> {sMonthActualToCopy}	
				
					UNION ALL
				
					SELECT
						'{sScenarioTarget}' AS scenario
						, date
						, id_costcenter
						, id_factory
						, id_indicator
						, wf_type
						, value		
												
					FROM XFC_PLT_AUX_DailyHours_Stored F
					WHERE scenario = 'Actual'
					AND YEAR(date) = {sYear}	
					AND MONTH(date) = {sMonthActualToCopy}										

				"
				
				
			#End Region				
			
				'BRApi.ErrorLog.LogMessage(si, sQuery_Hours)
				ExecuteSql(si, sQuery_Hours)					
			
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