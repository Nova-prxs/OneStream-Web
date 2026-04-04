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

Namespace OneStream.BusinessRule.Extender.UTI_LoadFromDBTest
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
						
				'Create connections to both internal and external databases
				Using dbAppConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				
				Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, dbLocation.External, "SICProSQL")
				
					'Declare relevant variables
					Dim selectQuery As String
					Dim deleteQuery As String
					Dim dt As DataTable
					
					'Create a dictionary to map the names of the tables from internal (key) to external (value)
					Dim tableNameMappingDict As New Dictionary(Of String, String) From {
																							{"XFC_AccountHierarchy", "XFC_Accounts"},
																							{"XFC_Accounts", "XFC_Accounts"},
																							{"XFC_RawSales", "XFC_RawSales"},
																							{"XFC_CEBES", "XFC_CEBES"},
																							{"XFC_CEBESHierarchy", "XFC_CEBESHierarchy"},
																							{"XFC_ChannelHierarchy", "XFC_ChannelHierarchy"},
																							{"XFC_ComparativeCEBES", "XFC_Comparative_CEBES"},
																							{"XFC_ComparativeDates", "XFC_DATES"},
																							{"XFC_FILoadZParam", "XFC_FILoadZParam"},
																							{"XFC_PersonnelCost", "XFC_PersonnelCost"},
																							{"XFC_PL", "XFC_PL"},
																							{"XFC_Renovation_CEBES", "XFC_Renovation_CEBES"},
																							{"XFC_Responsible_CEBES", "XFC_Responsible_CEBES"},
																							{"XFC_TheoreticalWastePersonnelCosts", "XFC_TheoreticalCostsWastePersonnel"},
																							{"XFC_TheoreticalCosts", "XFC_TheoreticalCosts"},
																							{"XFC_Waste", "XFC_Waste"}
																						}
					
					'Loop through each table name in the dictionary
					For Each tableNameMapping As KeyValuePair(Of String, String) In tableNameMappingDict
					
						'Clean the internal table
'								deleteQuery = $"DELETE
'												FROM {tableNameMapping.Key}"
						
'								BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)
					
						'Manage not common cases
						'Pre filter in case of CEBES table, else just copy
						If tableNameMapping.Key = "XFC_CEBES" Then
							
'									selectQuery = $"SELECT
'														cebe,
'														description,
'														CASE
'															WHEN brand IN (
'																'SDH',
'																'Marcas Cerradas',
'																'Genéricos Grupo',
'																'Sin Asignar'
'																) THEN sales_brand
'															ELSE brand
'														END AS brand,
'														city,
'														company,
'														country,
'														location,
'														postal_code,
'														unit_type,
'														region,
'														state,
'														close_date,
'														open_date,
'														NULL AS sales_brand,
												
'													FROM {tableNameMapping.Value}"
							
'							deleteQuery = $"TRUNCATE TABLE {tableNameMapping.Key}"
						
'							BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)

'							selectQuery = $"SELECT
'													cebe,
'													description,
'													brand,
'													city,
'													company,
'													country,
'													location,
'													postal_code,
'													unit_type,
'													region,
'													state,
'													close_date,
'													open_date,
'													sales_brand,
'													cebe_class
											
'												FROM {tableNameMapping.Value}"
						
'							'Create a datatable from the external db data
'							dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
							
'							'Save the external datatable to the application table
'							BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
							
'						Else If tableNameMapping.Key = "XFC_ComparativeCEBES" Then
							
'							deleteQuery = $"DELETE
'										FROM {tableNameMapping.Key}"
						
'							BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)
							
'							selectQuery = $"	SELECT 
'													cebe,
'													date,
'													weeklycomparability,
'													annualcomparability,
'													descr_weelkycomparability AS desc_weeklycomparability,
'													descr_annualcomparability AS desc_annualcomparability
'												FROM {tableNameMapping.Value}"
						
'							'Create a datatable from the external db data
'							dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
							
'							'Save the external datatable to the application table
'							BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
							
'						Else If tableNameMapping.Key = "XFC_ComparativeDates" Then
					
'							deleteQuery = $"DELETE
'										FROM {tableNameMapping.Key}"
				
'							BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)
					
'							selectQuery = $"	SELECT
'													FECHA_DATE AS date,
'													FECHA_DATE_AA AS date_comparable,
'													FECHA_DATE_2AA AS date_comparable_2,
'													FECHA_DATE_3AA AS date_comparable_3,
'													FECHA_DATE_4AA AS date_comparable_4,
'													FECHA_DATE_5AA AS date_comparable_5
'												FROM {tableNameMapping.Value}"
				
'							'Create a datatable from the external db data
'							dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
					
'							'Save the external datatable to the application table
'							BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
					
'						Else If tableNameMapping.Key = "XFC_TheoreticalCosts" Then
					
'							deleteQuery = $"DELETE
'										FROM {tableNameMapping.Key}"
				
'							BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)
					
'							selectQuery = $"	SELECT
'													date,
'													cebe,
'													channel3,
'													theo_costs_sales_units AS theo_cost_sales_units
'												FROM {tableNameMapping.Value}"
				
'							'Create a datatable from the external db data
'							dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
					
'							'Save the external datatable to the application table
'							BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
							
						Else If tableNameMapping.Key = "XFC_Waste" Then
					
							deleteQuery = $"DELETE
										FROM {tableNameMapping.Key}"
				
							BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, False)
						
							selectQuery = $"SELECT date,
												cebe,
												amount
											FROM {tableNameMapping.Value}"
				
							'Create a datatable from the external db data
							dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, False)
					
							'Save the external datatable to the application table
							BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
						
						Else
							
						End If
					
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