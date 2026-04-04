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

Namespace OneStream.BusinessRule.Extender.PLT_Forecast_Copy_From_Actual_Closing_Month_TEMP
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sScenario As String = "RF09"
				Dim sYear As String = "2025"
				Dim sMonthFirstForecast As String = "9"	
				Dim sFactory As String = "R0592"									
				
			#Region "Copy Calendar"
			
				Dim sQuery_Calendar As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_Calendar
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) < {sMonthFirstForecast}
						AND id_factory = '{sFactory}'
					
						-- 2. Insert closed months
					
						INSERT INTO XFC_PLT_AUX_Calendar
						(
							scenario
							, id_factory
							, date
							, id_indicator
							, id_costcenter
							, id_template
							, value									
						)
					
						SELECT
							'{sScenario}' AS scenario
							, id_factory
							, date
							, id_indicator
							, id_costcenter
							, id_template
							, value			
						
						FROM XFC_PLT_AUX_Calendar
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) < {sMonthFirstForecast}
							AND id_factory = '{sFactory}'
				"
				
				
			#End Region				
			
				ExecuteSql(si, sQuery_Calendar)		
				
			#Region "Copy Time Presence"
			
				Dim sQuery_TimePresence As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_TimePresence
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) < {sMonthFirstForecast}
						AND id_factory = '{sFactory}'
					
						-- 2. Insert closed months
					
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
							'{sScenario}' AS scenario
							, id_factory
							, date
							, id_costcenter
							, wf_type
							, shift
							, value		
						
						FROM XFC_PLT_AUX_TimePresence
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) < {sMonthFirstForecast}
							AND id_factory = '{sFactory}'
				"
				
				
			#End Region				
			
				ExecuteSql(si, sQuery_TimePresence)						
			
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