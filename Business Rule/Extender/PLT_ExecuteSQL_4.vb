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

Namespace OneStream.BusinessRule.Extender.PLT_ExecuteSQL_4
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
			
'				Dim sQuery As String = 
'				"			
'					TRUNCATE TABLE XFC_PLT_HIER_Nomenclature_Date_Report
				
'					DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_HIER_Nomenclature_Date_Report';
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'scenario'			, 'varchar', 50		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',0);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'year'				, 'varchar', 4		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',1);									
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'id_factory'			, 'varchar', 50 	, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',2);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'date'				, 'date'   , NULL 	, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',3);							
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'id_product_final'	, 'varchar', 50		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',4);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'id_product'			, 'varchar', 300	, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',5);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'id_component'		, 'varchar', 50		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',6);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'coefficient'			, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',7);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'exp_coefficient'		, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',8);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_HIER_Nomenclature_Date_Report', 'Level'				, 'int'	   , NULL	, 0 , 0 	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',9);
				
'					TRUNCATE TABLE XFC_PLT_FACT_Costs_VTU_Report
				
'					DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_FACT_Costs_VTU_Report';
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'scenario'			, 'varchar', 50		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',0);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'year'				, 'varchar', 4		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',1);				
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'id_factory'			, 'varchar', 50 	, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',2);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'date'				, 'date'   , NULL 	, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',3);						
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'id_product'			, 'varchar', 50		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',4);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'id_averagegroup'		, 'varchar', 50		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',5);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'account_type'		, 'varchar', 50		, 0 , 0		, 0, '', 1, '2025-06-16','PRD1_c001406a_Production_20240919.131807',6);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'cost_fixed'			, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',7);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'cost_variable'		, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',8);				
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'cost'				, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',9);								
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'volume'				, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',10);												
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'activity_UO1'		, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',11);								
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report', 'activity_UO1_total'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-06-16','PRD1_c001406a_Production_20240919.131807',12);												
'				"
				
'				Dim sQuery As String = "				
'					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_FACT_Costs_VTU_Report_Account' AND ColumnName = 'VTU_unit_per'
'					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_FACT_Costs_VTU_Report_Account' AND ColumnName = 'VTU_unit_ytd'
				
'					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_FACT_Costs_VTU_Report_Account_Local' AND ColumnName = 'VTU_unit_per'
'					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_FACT_Costs_VTU_Report_Account_Local' AND ColumnName = 'VTU_unit_ytd'
				
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account', 'VTU_unit_per_var'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',14);												
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account', 'VTU_unit_per_fix'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',15);																
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account', 'VTU_unit_ytd_var'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',16);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account', 'VTU_unit_ytd_fix'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',17);				
				
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account_Local', 'VTU_unit_per_var'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',14);												
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account_Local', 'VTU_unit_per_fix'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',15);																
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account_Local', 'VTU_unit_ytd_var'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',16);
'					INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_FACT_Costs_VTU_Report_Account_Local', 'VTU_unit_ytd_fix'	, 'decimal', NULL	, 6 , 18	, 1, '', 0, '2025-09-12','PRD1_c001406a_Production_20240919.131807',17);				
'				"	

'				Dim sQuery As String = "
'					TRUNCATE TABLE XFC_PLT_AUX_DailyHours_Stored
					
'					INSERT INTO XFC_PLT_AUX_DailyHours_Stored (scenario, date, id_factory, wf_type, id_costcenter, value, id_indicator)
'					SELECT scenario, date, id_factory, wf_type, id_costcenter, SUM(value)
'						, CASE 
'								WHEN id_indicator = 'daily_working_hours' 			THEN 'theoretical_working_hours'
'								WHEN id_indicator = 'collective_leave' 				THEN 'collective_day_off'
'								WHEN id_indicator = 'days_off' 						THEN 'individual_days_off'
'								WHEN id_indicator = 'extra_hours' 					THEN 'extra_hours'
'								WHEN id_indicator = 'holiday' 						THEN 'public_holiday'
'								WHEN id_indicator = 'labor_relation_hours'			THEN 'paid_not_working'
'								WHEN id_indicator = 'paid_non_working_hours'		THEN 'paid_not_working'				
'								WHEN id_indicator = 'no_or_suspended_contract'		THEN 'not_employed'
'								WHEN id_indicator = 'not_paid_non_working_hours' 	THEN 'unpaid_not_working'
'								WHEN id_indicator = 'sick_leave'	 				THEN 'illness_and_accident'
'								WHEN id_indicator = 'strike' 						THEN 'strike'				
'								WHEN id_indicator = 'training' 						THEN 'training'
'								WHEN id_indicator = 'unemployment' 					THEN 'unemployment'
'								ELSE 'Z - ' + id_indicator END AS id_indicator
'					FROM XFC_PLT_AUX_DailyHours
'					GROUP BY scenario, date, id_factory, wf_type, id_costcenter
'						, CASE 
'								WHEN id_indicator = 'daily_working_hours' 			THEN 'theoretical_working_hours'
'								WHEN id_indicator = 'collective_leave' 				THEN 'collective_day_off'
'								WHEN id_indicator = 'days_off' 						THEN 'individual_days_off'
'								WHEN id_indicator = 'extra_hours' 					THEN 'extra_hours'
'								WHEN id_indicator = 'holiday' 						THEN 'public_holiday'
'								WHEN id_indicator = 'labor_relation_hours'			THEN 'paid_not_working'
'								WHEN id_indicator = 'paid_non_working_hours'		THEN 'paid_not_working'				
'								WHEN id_indicator = 'no_or_suspended_contract'		THEN 'not_employed'
'								WHEN id_indicator = 'not_paid_non_working_hours' 	THEN 'unpaid_not_working'
'								WHEN id_indicator = 'sick_leave'	 				THEN 'illness_and_accident'
'								WHEN id_indicator = 'strike' 						THEN 'strike'				
'								WHEN id_indicator = 'training' 						THEN 'training'
'								WHEN id_indicator = 'unemployment' 					THEN 'unemployment'
'								ELSE 'Z - ' + id_indicator END				
'				"

'				Dim sQuery As String = "
'TRUNCATE TABLE XFC_PLT_MASTER_DailyHours_Indicator				
				
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('theoretical_working_hours'		,'Theorical Working Hours (TWH)'		,'1.1.  Theorical Working Hours (TWH)'			,'1')
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('unemployment'					,'Unemployment'							,'2.1.    (-) Unemployment'						,'2')				
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('public_holiday'					,'Public holiday'						,'2.2.    (-) Public holiday'					,'2')				
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('collective_day_off'				,'Collective day off'					,'2.3.    (-) Collective day off'				,'2')						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('not_employed'					,'Not employed'							,'2.4.    (-) Not employed'						,'2')					
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('internal_transfert_out'			,'Internal transfert out'				,'2.5.    (-) Internal transfert out'			,'2')				
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('internal_transfert_in'			,'Internal transfert in'				,'2.6.    (+) Internal transfert in'			,'2')				
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('required_hours'					,'Required hours'						,'2.T.  Required hours'							,NULL)				
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('extra_hours'					,'Extra hours'							,'3.1.    (+) Extra hours'						,'3')							
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('individual_days_off'			,'Individual day off'					,'3.2.    (-) Individual day off'				,'3')
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('strike'							,'Strike'								,'3.3.    (-) Strike'							,'3')	
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('unpaid_not_working'				,'Hours not working unpaid'				,'3.4.    (-) Hours not working unpaid'			,'3')
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('paid_hours'						,'Paid hours'							,'3.T.  Paid hours'								,NULL)								
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('illness_and_accident'			,'Illness and accident'					,'4.1.    (-) Illness and accident'				,'4')				
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('harbour_hours'					,'Harbour hours'						,'4.T.  Harbour hours'							,NULL)				
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('paid_not_working'				,'Paid not working'						,'5.1.    (-) Paid not working'					,'5')			
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('working_hours'					,'Working hours'						,'5.T.  Working hours'							,NULL)		
				
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('extra_hours_negative'			,'Extra hours'							,'6.1.    (-) Extra hours'						,'3')											
																			  																																			
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('working_hours_excluding_eh'		,'Working hours excluding extra hours'	,'6.T.  Working hours excluding extra hours'	,'6')					
																																																						
'INSERT INTO XFC_PLT_MASTER_DailyHours_Indicator (id, description1, description2,block) VALUES ('training'						,'Training'								,'7.1.  Training'								,'7')
								
'				"

				Dim sQuery As String = "
		
DELETE FROM XFC_PLT_AUX_ActivityIndex_Adjust WHERE scenario = 'Actual' AND scenario_ref = 'Budget_V4' AND YEAR(date) = 2024 AND id_averagegroup = 'GM133750' AND id_factory = 'R0671'

				
				
				"
				ExecuteSql(si, sQuery)
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