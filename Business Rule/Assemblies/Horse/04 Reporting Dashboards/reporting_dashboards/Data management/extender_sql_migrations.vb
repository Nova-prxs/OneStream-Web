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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_sql_migrations
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			'Get assembly migrations class
			Dim migrationsClass As New Workspace.__WsNamespacePrefix.__WsAssemblyName.Migrations()
			
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get function name and convert to lowercase for robust comparison
						Dim functionName As String = args.NameValuePairs("p_function").ToLower()
						
						#Region "Up Migration"
						
						If functionName = "upmigration"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Execute migration queries
								For Each query In migrationsClass.GetMigrationQueries(si, "up")
									BRApi.Database.ExecuteSql(dbConn, query, False)
								Next
								'Populate tables
								For Each query As String In migrationsClass.GetPopulationQueries(si, "up")
									If Not query.Contains("INTO XFC_INV_FACT") AndAlso
										Not query.Contains("INTO XFC_INV_MASTER") Then BRApi.Database.ExecuteSql(dbConn, query, False)
								Next 
							End Using
							
						#End Region
						
						#Region "Create Tables"
						
						Else If functionName = "createtables"
							
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Execute migration queries
								For Each query In migrationsClass.GetMigrationQueries(si, "up")
									BRApi.Database.ExecuteSql(dbConn, query, False)
								Next
							End Using
							
						#End Region
						
						#Region "Populate Tables"
						
						Else If functionName = "populatetables"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Populate tables
								For Each query In migrationsClass.GetPopulationQueries(si, "up")
									BRApi.Database.ExecuteSql(dbConn, query, False)
								Next 
							End Using
						
						#End Region
						
						#Region "Delete Tables"
						
						Else If functionName = "deletetables"
							Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
								'Populate tables
								For Each query In migrationsClass.GetMigrationQueries(si, "down")
									BRApi.Database.ExecuteSql(dbConn, query, False)
								Next 
							End Using
						
						#End Region
						
						End If
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
						' ... (código sin cambios) ...
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace