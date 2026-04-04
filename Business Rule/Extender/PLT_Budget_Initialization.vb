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

Namespace OneStream.BusinessRule.Extender.PLT_Budget_Initialization
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sScenario As String = "Budget_V1"
				Dim sYear As String = "2026"			
							
				Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries	
				
			#Region "CPI Initialization"
				
				Dim sQuery_CPI As String = $"
					
					-- 1. Previous clear
				
					DELETE FROM XFC_PLT_AUX_CPI
					WHERE scenario = '{sScenario}'
					AND year = '{sYear}'
				
					-- 2. Insert
				
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0671'	,'CPI',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0548913','CPI',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0548914','CPI',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0529002','CPI',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0045106','CPI',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0483003','CPI',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0592'	,'CPI',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0585'	,'CPI',NULL)											
				
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0671'	,'AGS (WIP)',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0548913','AGS (WIP)',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0548914','AGS (WIP)',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0529002','AGS (WIP)',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0045106','AGS (WIP)',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0483003','AGS (WIP)',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0592'	,'AGS (WIP)',NULL)											
					INSERT INTO XFC_PLT_AUX_CPI (scenario,year,id_factory,id_indicator,value) VALUES ('{sScenario}','{sYear}','R0585'	,'AGS (WIP)',NULL)					
				"		
			
			#End Region
			
				'ExecuteSql(si, sQuery_CPI)

			#Region "Nomenclature Initializaton"
				
				'1. Copy NOMENCLATURE_DATE to NOMENCLATURE_PLANNING
				shared_queries.update_Nomenclature_Planning(si, sScenario, sYear, "12")
				
				'2. Initialization NOMENCLATURE_REPORT
				shared_queries.update_Nomenclature_Report(si, sScenario, sYear, "12")			

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