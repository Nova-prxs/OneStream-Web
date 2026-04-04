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

Namespace OneStream.BusinessRule.Extender.UTI_CopyDataBetweenApps
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Dim oar As New OpenAppResult
				Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Production_BKP_20250813", oar)
				Dim tableName As String = "XFC_DailySales"
				
				If oar <> OpenAppResult.Success Then
					Throw New Exception("Error opening another app: " & oar.ToString)
				End If
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
				
					' Truncate target table
					BRApi.Database.ExecuteSql(
						dbConn,
						$"
						TRUNCATE TABLE {tableName};
						",
						False
					)
					
					Dim yearIterable As Integer = 2019
					Dim maxYear As Integer = 2027
					Dim yearsDifference As Integer = maxYear - yearIterable
					Dim i As Integer
					
					Dim selectQuery As String
					Dim dt As New DataTable
					
					For i = 0 To yearsDifference
									
						selectQuery = $"
							SELECT *
							FROM {tableName}
							WHERE 
								YEAR(date) = {(yearIterable + i).ToString};"
					
						'Create a datatable from the other app db data
						dt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
						
						'Save the other app table to the current app table
						BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableName, dt, True)
					
					Next
				
				End Using
				End Using

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace