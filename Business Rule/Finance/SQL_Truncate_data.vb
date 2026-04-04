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

Namespace OneStream.BusinessRule.Finance.SQL_Truncate_data
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs) As Object
			Try
				Me.ClearDataTable(si)

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
	    Sub ClearDataTable (ByVal si As SessionInfo)
		
			Dim sbSQL As New Text.StringBuilder()
						
			sbSQL.Append(" SELECT Rule_Name")			
			sbSQL.Append(" FROM XFC_ALL_Allocation_Rules")																				
			
			Using dbConnApp As DbConnInfoApp = BRApi.Database.CreateApplicationDbConnInfo(si)
			                                   BRApi.Database.ExecuteSql(dbConnApp,sbSQL.ToString,False)											
            End Using
         End Sub
		 
	End Class
End Namespace