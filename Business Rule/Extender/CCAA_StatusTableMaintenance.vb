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

Namespace OneStream.BusinessRule.Extender.CCAA_StatusTableMaintenance
	
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
			Dim sQuery As String = String.Empty
			Dim sQuery2 As String = String.Empty
			
			' When the list of manual notes is updated, the status tables need to be modified
			
			' ONE OF THE TWO REGIONS MUST BE COMMENTED
			
			#Region "NEW MANUAL NOTE"
			' INSERT NOTE INFORMATION HERE
			
			Dim code As String = "100" 		'Example: "16B"
			Dim name As String = "Prueba"		'Example: "Detail of other liabilities"
			
			
			
			
			Dim EntityList As New List(Of Tuple(Of String, String)) From {
			    New Tuple(Of String, String)("Brazil", "R1303"),
			    New Tuple(Of String, String)("Argentina", "R0592"),
			    New Tuple(Of String, String)("Chile", "R0585"),
			    New Tuple(Of String, String)("Holding", "R1300"),
			    New Tuple(Of String, String)("Spain", "R1301"),
			    New Tuple(Of String, String)("Portugal", "R0671"),
			    New Tuple(Of String, String)("Romania", "R0611"),
			    New Tuple(Of String, String)("Turkey", "R1302")
			}

			
			Dim MonthList As New List(Of String) From{"M1", "M2", "M3", "M4", "M5", "M6", "M7", "M8", "M9", "M10", "M11", "M12"}
			
			Dim ActualYear As Integer = DateTime.Now.Year
			
			
			Dim YearList As New List(Of String)
			
			For y As Integer = 2024 To ActualYear
			    YearList.Add(y.ToString())
			Next
			
			For Each m In MonthList
				For Each y In YearList
					For Each entityPair In EntityList
						Dim entity As String = entityPair.Item1
						Dim entitycode As String = entityPair.Item2
						
						Dim tableName As String = "XFC_CCAA_CubeView_Status_" + entity + "_" + y + m
						'brapi.ErrorLog.LogMessage(si,$"{tableName}")
						
						
						sQuery = "
						IF OBJECT_ID('" + tableName + "', 'U') IS NOT NULL
						BEGIN
							INSERT INTO " & tableName & " (id, note_name, country, last_save_name, last_save_date, send_status, send_name, send_date, check_status, check_name, check_date) VALUES
							('" + entitycode + " - " + code + "', '" + code + ": " + name + "', '" + entity + "', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00')
						END
						"
						
						ExecuteSql(si, sQuery)
						
						Dim GroupTableName As String = "XFC_CCAA_CubeView_Status_Group_" + y + m
						
						sQuery2 = "
						IF OBJECT_ID('" + GroupTableName + "', 'U') IS NOT NULL
						BEGIN
							INSERT INTO " & GroupTableName & " (id, note_name, country, last_save_name, last_save_date, send_status, send_name, send_date, check_status, check_name, check_date) VALUES
							('" + entitycode + " - " + code + "', '" + code + ": " + name + "', '" + entity + "', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00', 'Not Started', '-', '2025-10-30 13:20:00')
						END
						"
						
						ExecuteSql(si, sQuery2)
						
					Next
					
				Next	
			Next
			
			#End Region
			
			#Region "MANUAL NOTE REMOVED"
'			' INSERT REMOVED NOTE INFORMATION
			
'			Dim code As String = "100" 		'Example: "16B"
			
			
'			Dim EntityList As New List(Of Tuple(Of String, String)) From {
'			    New Tuple(Of String, String)("Brazil", "R1303"),
'			    New Tuple(Of String, String)("Argentina", "R0592"),
'			    New Tuple(Of String, String)("Chile", "R0585"),
'			    New Tuple(Of String, String)("Holding", "R1300"),
'			    New Tuple(Of String, String)("Spain", "R1301"),
'			    New Tuple(Of String, String)("Portugal", "R0671"),
'			    New Tuple(Of String, String)("Romania", "R0611"),
'			    New Tuple(Of String, String)("Turkey", "R1302")
'			}
			
'			Dim MonthList As New List(Of String) From{"M1", "M2", "M3", "M4", "M5", "M6", "M7", "M8", "M9", "M10", "M11", "M12"}
			
'			Dim ActualYear As Integer = DateTime.Now.Year
			
'			Dim YearList As New List(Of String)
			
'			For y As Integer = 2024 To ActualYear
'			    YearList.Add(y.ToString())
'			Next
			
'			For Each m In MonthList
'			    For Each y In YearList
'			        For Each entityPair In EntityList
'			            Dim entity As String = entityPair.Item1
'			            Dim entitycode As String = entityPair.Item2
			

'			            Dim tableName As String = "XFC_CCAA_CubeView_Status_" & entity & "_" & y & m
			
'			            sQuery = "
'			            IF OBJECT_ID('" & tableName & "', 'U') IS NOT NULL
'			            BEGIN
'			                DELETE FROM " & tableName & " WHERE id = '" & entitycode & " - " & code & "'
'			            END
'			            "
'			            ExecuteSql(si, sQuery)
			
'			            Dim GroupTableName As String = "XFC_CCAA_CubeView_Status_Group_" & y & m
			
'			            sQuery2 = "
'			            IF OBJECT_ID('" & GroupTableName & "', 'U') IS NOT NULL
'			            BEGIN
'			                DELETE FROM " & GroupTableName & " WHERE id = '" & entitycode & " - " & code & "'
'			            END
'			            "
'			            ExecuteSql(si, sQuery2)
			
'			        Next
'			    Next
'			Next
			
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