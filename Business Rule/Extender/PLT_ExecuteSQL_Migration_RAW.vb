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

Namespace OneStream.BusinessRule.Extender.PLT_ExecuteSQL_Migration_RAW

	Public Class MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
					
					Dim sCreateRaw As String = $"	
						DROP TABLE IF EXISTS XFC_PLT_RAW_CostsMonthlyPRE;
					
						SELECT *
						INTO XFC_PLT_RAW_CostsMonthlyPRE
						FROM XFC_PLT_RAW_CostsMonthly
						WHERE Month = '7';
					"
					
					Dim sGetData As String = $"
						SELECT * 
						FROM XFC_PLT_RAW_CostsMonthlyPRE
					"
					
					Dim sDeleteInsert As String = $"
						
						DELETE FROM XFC_PLT_RAW_CostsMonthly 
						WHERE Month = '7'
						AND id_factory = '0671';
					
						INSERT INTO XFC_PLT_RAW_CostsMonthly 					
						SELECT *
						FROM XFC_PLT_RAW_CostsMonthlyPRE 
					"
					
'					ExecuteSql(si, sCreateRaw)
					
'					Dim dt As DataTable = ExecuteSqlOtherAPP(si, sGetData,"PREPROD")
'					UTISharedFunctionsBR.LoadDataTableToCustomTable(si, "XFC_PLT_RAW_CostsMonthlyPRE", dt, "replace")
					
					ExecuteSql(si, sDeleteInsert)
					

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