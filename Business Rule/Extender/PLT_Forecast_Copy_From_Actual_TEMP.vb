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

Namespace OneStream.BusinessRule.Extender.PLT_Forecast_Copy_From_Actual_TEMP
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
				Dim sScenario As String = "RF09"
				Dim sYear As String = "2025"
				Dim sMonthFirstForecast As String = "9"			
'				Dim sFactory As String = "R0548914"
				
			#Region "Copy Daily Hours"
			
				Dim sQuery_Hours As String = $"
				
						-- 1. Previous clear
					
						DELETE FROM XFC_PLT_AUX_DailyHours_Stored
						WHERE scenario = '{sScenario}'
						AND YEAR(date) = '{sYear}'
						AND MONTH(date) < {sMonthFirstForecast}
					
						-- 2. Insert closed months
					
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
							'{sScenario}' AS scenario
							, id_factory
							, date
							, id_indicator
							, id_costcenter
							, wf_type
							, shift
							, value		
						
						FROM XFC_PLT_AUX_DailyHours_Stored
						
						WHERE scenario = 'Actual'
							AND YEAR(date) = {sYear}
					    	AND MONTH(date) < {sMonthFirstForecast}
				"
				
				
			#End Region				
			
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