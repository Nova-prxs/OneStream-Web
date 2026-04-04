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

Namespace OneStream.BusinessRule.Extender.PLT_ExecuteSQL_6
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sQuery1 As String = 
					"
				
					--*** TIME OF PRESENCE ***
				
					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_AUX_TimePresence' AND ColumnName = 'shift';
					--INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_AUX_TimePresence', 'shift', 'varchar', 50, 0, 0, 1, '', 0, '2025-10-29','PRD1_c001406a_Production_20240919.131807',6);
				
					--UPDATE XFC_PLT_AUX_TimePresence SET shift = '2x8 B'
					--
					--ALTER TABLE XFC_PLT_AUX_TimePresence
					--ALTER COLUMN shift VARCHAR(50) NOT NULL;				
					--
					--ALTER TABLE XFC_PLT_AUX_TimePresence
					--DROP CONSTRAINT PK_XFC_PLT_AUX_TimePresence_1;
					--
					--ALTER TABLE XFC_PLT_AUX_TimePresence
					--ADD CONSTRAINT PK_XFC_PLT_AUX_TimePresence_1 PRIMARY KEY (date, scenario, id_factory, wf_type, id_costcenter, shift);
				
					"

'				ExecuteSql(si, sQuery1)	
					
				Dim sQuery2 As String = 
					"
				
					--*** DAILY HOURS ***
				
					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_AUX_DailyHours_Stored' AND ColumnName = 'shift';
					--INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_AUX_DailyHours_Stored', 'shift', 'varchar', 50, 0, 0, 1, '', 0, '2025-10-29', 'PRD1_c001406a_Production_20240919.131807',7);
				
					--UPDATE XFC_PLT_AUX_DailyHours_Stored SET shift = '2x8 B'
					--
					--ALTER TABLE XFC_PLT_AUX_DailyHours_Stored
					--ALTER COLUMN shift VARCHAR(50) NOT NULL;				
					--
					--ALTER TABLE XFC_PLT_AUX_DailyHours_Stored
					--DROP CONSTRAINT PK_XFC_PLT_AUX_DailyHours_Stored_1;
					--
					--ALTER TABLE XFC_PLT_AUX_DailyHours_Stored
					--ADD CONSTRAINT PK_XFC_PLT_AUX_DailyHours_Stored_1 PRIMARY KEY (date, scenario, id_factory, wf_type, id_indicator, id_costcenter, shift);
				
					"

'				ExecuteSql(si, sQuery2)			
				
				Dim sQuery3 As String = 
					"
				
					--*** CALENDAR ***
				
					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_PLT_AUX_Calendar' AND ColumnName = 'id_costcenter';
					--INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_PLT_AUX_Calendar', 'id_costcenter', 'varchar', 50, 0, 0, 1, '', 0, '2025-10-29','PRD1_c001406a_Production_20240919.131807',6);
				
					--UPDATE XFC_PLT_AUX_Calendar SET id_costcenter = '-1'
					--
					--ALTER TABLE XFC_PLT_AUX_Calendar
					--ALTER COLUMN id_costcenter VARCHAR(50) NOT NULL;				
					--
					--ALTER TABLE XFC_PLT_AUX_Calendar
					--DROP CONSTRAINT PK_XFC_PLT_AUX_Calendar_1;
					--
					--ALTER TABLE XFC_PLT_AUX_Calendar
					--ADD CONSTRAINT PK_XFC_PLT_AUX_Calendar_1 PRIMARY KEY (date, scenario, id_factory, id_template, id_indicator, id_costcenter);
				
					"

'				ExecuteSql(si, sQuery3)						
				
				Dim sQuery4 As String = 
					"
				
					--*** HEADCOUNT ***
				
					--DELETE FROM XFW_TDM_TablesDefinition WHERE TableName = 'XFC_MAIN_FACT_Headcount' AND ColumnName = 'scenario';
					--INSERT INTO XFW_TDM_TablesDefinition VALUES ('XFC_MAIN_FACT_Headcount', 'scenario', 'varchar', 50, 0, 0, 1, '', 0, '2025-10-29','PRD1_c001406a_Production_20240919.131807',0);
				
					--UPDATE XFC_MAIN_FACT_Headcount SET scenario = 'Actual'
					
					ALTER TABLE XFC_MAIN_FACT_Headcount
					ALTER COLUMN scenario VARCHAR(50) NOT NULL;	
				
					ALTER TABLE XFC_MAIN_FACT_Headcount
					ALTER COLUMN costcenter_id VARCHAR(50) NOT NULL;					
					
					ALTER TABLE XFC_MAIN_FACT_Headcount
					DROP CONSTRAINT PK_XFC_MAIN_FACT_Headcount_1;
					
					ALTER TABLE XFC_MAIN_FACT_Headcount
					ADD CONSTRAINT PK_XFC_MAIN_FACT_Headcount_1 PRIMARY KEY (scenario, year, month, company_id, employee_id, costcenter_id);
				
					"

'				ExecuteSql(si, sQuery4)					
				
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