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

Namespace OneStream.BusinessRule.Extender.UTI_LoadFromDB_OneQuery
	Public Class MainClass
		
		'Reference business rule to get functions and variables
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
						
						'Create connections to both internal and external databases
						Using dbAppConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, dbLocation.External, "SICProSQL")						
						
							'Declare relevant variables
							Dim selectQuery As String
							Dim dt As DataTable																
									
							Dim deleteQuery As String = $"	DELETE FROM XFC_RawSales WHERE YEAR(date) = 2025 AND MONTH(date) IN (7,8,9)"
							BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)
							
							selectQuery = $"
								SELECT *
								FROM XFC_RawSales
								WHERE YEAR(date) = 2025 AND MONTH(date) IN (7,8,9)
									AND cod_channel3 NOT IN (48, 49);"
						
							'Create a datatable from the external db data
							dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
							
							'Save the external datatable to the application table
							BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, "XFC_RawSales", dt, True)
									
							
							'Update closing tables
							SharedFunctionsBR.SetDatesForClosedStores(si)
									
						End Using
						
						End Using
				
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
					End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace