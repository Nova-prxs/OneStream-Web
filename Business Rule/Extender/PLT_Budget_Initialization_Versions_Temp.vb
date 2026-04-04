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

Namespace OneStream.BusinessRule.Extender.PLT_Budget_Initialization_Versions_Temp
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sScenarioSource As String = "Budget_V1"
				Dim sScenarioTarget As String = "Budget_V2"
				Dim sYear As String = "2026"		
		
				
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