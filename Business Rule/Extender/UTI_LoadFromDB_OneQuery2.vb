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

Namespace OneStream.BusinessRule.Extender.UTI_LoadFromDB_OneQuery2
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						'Create connections to both internal and external databases
						Using dbAppConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, dbLocation.External, "SICProSQL")
						
							Dim yearIterable As Integer = 2024
							Dim maxYear As Integer = 2025
							Dim yearsDifference As Integer = maxYear - yearIterable
							Dim i As Integer
						
							'Declare relevant variables
							Dim selectQuery As String
							Dim dt As DataTable																									
									
									For i = 0 To yearsDifference
										
										Dim deleteQuery As String = $"	DELETE FROM XFC_PL 
																		WHERE LEFT(period, 4) = '{(yearIterable + i).ToString}'
																		"
	
										'Create a datatable from the external db data
										BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)											
									
										selectQuery = $"SELECT scenario,
															period,
															accounting_account,
															ceco,
															accumulated_balance,
															currency,
															accounts_chart,
															company
														FROM XFC_PL PL
										                LEFT JOIN XFC_FILoadZParam FP
                                                        ON PL.company = FP.value
														WHERE
															LEFT(period, 4) = {(yearIterable + i).ToString}
															AND accounting_account IN (
																SELECT DISTINCT
																	account_number
																FROM XFC_AccountHierarchy
																WHERE account_number LIKE '[0-9]%'
															)
															AND ceco IN (
																	SELECT DISTINCT
																		cebe
																	FROM XFC_CEBESHierarchy
															)
															AND accounts_chart IN ('RS01','CAES')
															AND (LEFT(period, 4) <> '2025' OR (LEFT(period, 4) = 2025 AND ceco NOT IN ('ZFDP9100','ZFDP9101')))
										                    AND  ( 
										                          COALESCE(FP.value, 'No Mapeado') = 'No Mapeado'
										                          OR (YEAR(FP.beginning_date) <= {(yearIterable + i).ToString} AND YEAR(FP.end_date) >= {(yearIterable + i).ToString}));"
									
									
										'Create a datatable from the external db data
										dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
										
										'Save the external datatable to the application table
										BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, "XFC_PL", dt, True)
									
									Next
										
									
						End Using
						
						End Using
						
					End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace