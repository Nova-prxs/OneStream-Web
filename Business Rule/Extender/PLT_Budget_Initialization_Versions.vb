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

Namespace OneStream.BusinessRule.Extender.PLT_Budget_Initialization_Versions
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sScenarioSource As String = "Budget_V1"
				Dim sScenarioTarget As String = "Budget_V2"
				Dim sYear As String = "2026"		
	
' *** PRODUCTION ***				
				
			#Region "Copy Volumes"
				
				Dim sQuery_Volumes As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_Production_Planning_Volumes
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = '{sYear}'
				
					-- 2. Insert
				
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
					)
				
					SELECT
						'{sScenarioTarget}' AS scenario
						,date
						,id_factory
						,id_costcenter
						,id_averagegroup
						,id_product
						,comment
						,value
												
					FROM XFC_PLT_AUX_Production_Planning_Volumes
				
					WHERE scenario = '{sScenarioSource}'
					AND YEAR(date) = {sYear}			
							
				"
			
				
			#End Region
			
				ExecuteSql(si, sQuery_Volumes)
			
			#Region "Copy & Initialization Times"
				
				Dim sQuery_Times As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_Production_Planning_Times
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = '{sYear}'
				
					-- 2. Insert
				
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
							
				"
				
			#End Region
			
				ExecuteSql(si, sQuery_Times)
			
			#Region "Copy Production Fact Data"
				
				Dim sQuery_Fact As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_FACT_Production
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = '{sYear}'
				
					-- 2. Insert
				
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
					AND YEAR(date) = {sYear}

				"
				
				'BRApi.ErrorLog.LogMessage(si, sQuery_Fact)				
				
			#End Region	
			
				ExecuteSql(si, sQuery_Fact)						
			
			#Region "Copy Activity Index Adjust"
				
				Dim sQuery_ActivityIndex As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_ActivityIndex_Adjust
					WHERE scenario = '{sScenarioTarget}'
					AND YEAR(date) = '{sYear}'
				
					-- 2. Insert
				
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
	
				"
			
	
			#End Region				
			
				 ExecuteSql(si, sQuery_ActivityIndex)
				 
' *** COSTS ***				 
			
			#Region "Copy Costs"
			
				Dim sQuery_Cost As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_FACT_Costs
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
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
	
				"
								
			#End Region			
			
				ExecuteSql(si, sQuery_Cost)		
			
			#Region "Copy Costs Distribution"
			
				Dim sQuery_CostDist As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_FACT_CostsDistribution
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
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

				"
								
			#End Region
			
				ExecuteSql(si, sQuery_CostDist)		
			
			#Region "Copy % Variable"
			
				Dim sQuery_Var As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_FixedVariableCosts
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
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

				"
								
			#End Region
			
				ExecuteSql(si, sQuery_Var)	
				
			#Region "Copy Key Allocation"
			
				Dim sQuery_KeyAlloc As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_AllocationKeys
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_AllocationKeys
						(
							scenario
							, id_factory
							, date
							, id_costcenter
							, costnature
							, id_averagegroup
							, percentage								
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, id_factory
							, date
							, id_costcenter
							, costnature
							, id_averagegroup
							, percentage	
						
						FROM XFC_PLT_AUX_AllocationKeys
						
						WHERE scenario = '{sScenarioSource}'
						AND YEAR(date) = {sYear}

				"
								
			#End Region
			
				ExecuteSql(si, sQuery_KeyAlloc)						
				
			#Region "Copy Projects"
			
				Dim sQuery_Proj As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_Projects
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_Projects
						(
							scenario
							, id_factory
							, date
							, cost_type
							, currency
							, id_project_code
							, offer
							, project_file
							, project_task
							, project_task_text
							, responsibility
							, value
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, id_factory
							, date
							, cost_type
							, currency
							, id_project_code
							, offer
							, project_file
							, project_task
							, project_task_text
							, responsibility
							, value	
						
						FROM XFC_PLT_AUX_Projects
						
						WHERE scenario = '{sScenarioSource}'
						AND YEAR(date) = {sYear}

				"
								
			#End Region
			
				ExecuteSql(si, sQuery_Proj)					
			
			#Region "Copy Effects"
			
				Dim sQuery_Eff As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_EffectsAnalysis
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
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
				
				"
								
			#End Region	
			
				ExecuteSql(si, sQuery_Eff)		
			
			#Region "Copy Energy"
			
				Dim sQuery_Ene As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_EnergyVariance
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
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

				"
				
				
			#End Region				
			
				ExecuteSql(si, sQuery_Ene)					
				
			#Region "Copy Costs Init GM"
			
				Dim sQuery_Costs_Init As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_Costs_Init_GM
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_Costs_Init_GM
						(
							scenario
							, scenario_ref
							, id_factory
							, date
							, id_account
							, id_costcenter
							, id_averagegroup
							, value_ref
							, activity_index
							, value_100_adjusted
							, fixed_cost_absorption
							, value_semiadjusted
							, value_semiadjusted_f
							, value_semiadjusted_v
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, scenario_ref
							, id_factory
							, date
							, id_account
							, id_costcenter
							, id_averagegroup
							, value_ref
							, activity_index
							, value_100_adjusted
							, fixed_cost_absorption
							, value_semiadjusted
							, value_semiadjusted_f
							, value_semiadjusted_v
						
						FROM XFC_PLT_AUX_Costs_Init_GM
						
						WHERE scenario = '{sScenarioSource}'
						AND YEAR(date) = {sYear}

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_Costs_Init)		
				
			#Region "Copy Costs Init GM Criteria"
			
				Dim sQuery_Costs_Init_Cri As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_Costs_Init_GM_Criteria
						WHERE scenario = '{sScenarioTarget}'
						AND year = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_Costs_Init_GM_Criteria
						(
							scenario
							, year
							, id_factory
							, id_account
							, criteria
							, inflation

						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, year
							, id_factory
							, id_account
							, criteria
							, inflation
						
						FROM XFC_PLT_AUX_Costs_Init_GM_Criteria
						
						WHERE scenario = '{sScenarioSource}'
						AND year = {sYear}

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_Costs_Init_Cri)	
				
			#Region "Copy CPI"
			
				Dim sQuery_CPI As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_CPI
						WHERE scenario = '{sScenarioTarget}'
						AND year = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_CPI
						(
							scenario
							, year
							, id_factory
							, id_indicator
							, value

						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, year
							, id_factory
							, id_indicator
							, value
						
						FROM XFC_PLT_AUX_CPI
						
						WHERE scenario = '{sScenarioSource}'
						AND year = {sYear}

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_CPI)		
				
			#Region "Copy Hourly Rate"
			
				Dim sQuery_Hourly_Rate As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_HourlyRate_Input
						WHERE scenario = '{sScenarioTarget}'
						AND year = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_HourlyRate_Input
						(
							scenario
							, year
							, id_factory
							, id_nature
							, value

						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, year
							, id_factory
							, id_nature
							, value
						
						FROM XFC_PLT_AUX_HourlyRate_Input
						
						WHERE scenario = '{sScenarioSource}'
						AND year = {sYear}

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_Hourly_Rate)					
			

' *** VTU ***		
				
			#Region "Copy VTU Final"
			
				Dim sQuery_VTU_Final As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_FACT_Costs_VTU_Final
						WHERE scenario = '{sScenarioTarget}'
						AND year = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_FACT_Costs_VTU_Final
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
							, activity_UO1_Total
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
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
							, activity_UO1_Total
						
						FROM XFC_PLT_FACT_Costs_VTU_Final
						
						WHERE scenario = '{sScenarioSource}'
						AND year = {sYear}

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_VTU_Final)		
				ExecuteActionSQLOnBIBlend(si, sQuery_VTU_Final)				
				
			#Region "Copy VTU Final Local"
			
				Dim sQuery_VTU_Final_Loc As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_FACT_Costs_VTU_Final_Local
						WHERE scenario = '{sScenarioTarget}'
						AND year = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_FACT_Costs_VTU_Final_Local
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
							, activity_UO1_Total
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
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
							, activity_UO1_Total
						
						FROM XFC_PLT_FACT_Costs_VTU_Final_Local
						
						WHERE scenario = '{sScenarioSource}'
						AND year = {sYear}

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_VTU_Final_Loc)
				ExecuteActionSQLOnBIBlend(si, sQuery_VTU_Final_Loc)
				
			#Region "Copy Nomenclature Planning"
			
				Dim sQuery_Nomenclature As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_HIER_Nomenclature_Date_Planning
						WHERE scenario = '{sScenarioTarget}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_HIER_Nomenclature_Date_Planning
						(
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
							'{sScenarioTarget}' AS scenario
							, id_factory
							, id_product
							, id_component
							, start_date
							, end_date
							, coefficient
							, prorata
						
						FROM XFC_PLT_HIER_Nomenclature_Date_Planning
						
						WHERE scenario = '{sScenarioSource}'

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_Nomenclature)	
				
			#Region "Copy Nomenclature Report"
			
				Dim sQuery_Nomenclature_Rep As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_HIER_Nomenclature_Date_Report
						WHERE scenario = '{sScenarioTarget}'
						AND year = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_HIER_Nomenclature_Date_Report
						(
							scenario
							, year
							, id_factory
							, date
							, id_product_final
							, id_product
							, id_component
							, coefficient
							, exp_coefficient
							, level
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, year
							, id_factory
							, date
							, id_product_final
							, id_product
							, id_component
							, coefficient
							, exp_coefficient
							, level
						
						FROM XFC_PLT_HIER_Nomenclature_Date_Report
						
						WHERE scenario = '{sScenarioSource}'
						AND year = {sYear}

				"
				
			#End Region				
			
				ExecuteSql(si, sQuery_Nomenclature_Rep)		
				ExecuteActionSQLOnBIBlend(si, sQuery_Nomenclature_Rep)
				
' *** HR ***						
				
			#Region "Copy Hours"
			
				Dim sQuery_Hours As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_DailyHours_Stored
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_DailyHours_Stored
						(
							scenario
							, id_factory
							, date
							, id_indicator
							, id_costcenter
							, wf_type
							, shift				
							, value									
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, id_factory
							, date
							, id_indicator
							, id_costcenter
							, wf_type
							, shift
							, value		
						
						FROM XFC_PLT_AUX_DailyHours_Stored
						
						WHERE scenario = '{sScenarioSource}'
						AND YEAR(date) = {sYear}

				"
				
				
			#End Region				
			
				ExecuteSql(si, sQuery_Hours)				
				
			#Region "Copy Time of Presence"
			
				Dim sQuery_TimePre As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_TimePresence
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_TimePresence
						(
							scenario
							, id_factory
							, date
							, id_costcenter
							, wf_type
							, shift				
							, value									
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, id_factory
							, date
							, id_costcenter
							, wf_type
							, shift
							, value		
						
						FROM XFC_PLT_AUX_TimePresence
						
						WHERE scenario = '{sScenarioSource}'
						AND YEAR(date) = {sYear}

				"
				
				
			#End Region				
			
				ExecuteSql(si, sQuery_TimePre)	
				
			#Region "Copy Calendar"
			
				Dim sQuery_Calendar As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_Calendar
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_Calendar
						(
							scenario
							, id_factory
							, date
							, id_costcenter
							, id_template
							, id_indicator		
							, value									
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, id_factory
							, date
							, id_costcenter
							, id_template
							, id_indicator	
							, value		
						
						FROM XFC_PLT_AUX_Calendar
						
						WHERE scenario = '{sScenarioSource}'
						AND YEAR(date) = {sYear}

				"
				
				
			#End Region				
			
				ExecuteSql(si, sQuery_Calendar)	
				
			#Region "Copy Balancing"
			
				Dim sQuery_Balancing As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_Balancing
						WHERE scenario = '{sScenarioTarget}'
						AND YEAR(date) = '{sYear}'
					
						-- 2. Insert
					
						INSERT INTO XFC_PLT_AUX_Balancing
						(
							scenario
							, id_factory
							, date
							, id_costcenter
							, bal_code
							, wf_type	
							, value									
						)
					
						SELECT
							'{sScenarioTarget}' AS scenario
							, id_factory
							, date
							, id_costcenter
							, bal_code
							, wf_type	
							, value		
						
						FROM XFC_PLT_AUX_Balancing
						
						WHERE scenario = '{sScenarioSource}'
						AND YEAR(date) = {sYear}

				"     
				
				#End Region
				
				ExecuteSql(si, sQuery_Balancing)																
			
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
		
        Private Sub ExecuteActionSQLOnBIBlend(ByVal si As SessionInfo, ByVal sqlCmd As String)
                'Use the name of the database, used in OneStream Server Configuration Utility >> App Server Config File >> Database Server Connections
                Dim extConnName As String = "OneStream BI Blend"
                Using dbConnApp As DBConnInfo = BRApi.Database.CreateExternalDbConnInfo(si, extConnName)          
                    BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, False, True)
                End Using                                                                              
        End Sub			
		
	End Class
	
End Namespace