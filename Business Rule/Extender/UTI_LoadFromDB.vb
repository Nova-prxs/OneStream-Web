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

Namespace OneStream.BusinessRule.Extender.UTI_LoadFromDB
	Public Class MainClass
		
		'Reference business rule to get functions and variables
		Dim SharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						'Create connections to both internal and external databases
						Using dbAppConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						
						Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, dbLocation.External, "SICProSQL")
						
							'Declare relevant variables
							Dim selectQuery As String
							Dim deleteQuery As String
							Dim dt As DataTable
							
							'Create a dictionary to map the names of the tables from internal (key) to external (value)
							Dim tableNameMappingDict As New Dictionary(Of String, String) From {
																									{"XFC_AccountHierarchy", "XFC_AccountHierarchy"},
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
																									{"XFC_Responsible_CEBES", "XFC_Responsible_CEBES"},
																									{"XFC_TheoreticalWastePersonnelCosts", "XFC_TheoreticalCostsWastePersonnel"},
																									{"XFC_TheoreticalCosts", "XFC_TheoreticalCosts"},
																									{"XFC_Waste", "XFC_Waste"}
																								}
																								
																								'Misma tabla 
'																								{"XFC_Remodelings", "XFC_Renovation_CEBES"},
							
							'Loop through each table name in the dictionary
							For Each tableNameMapping As KeyValuePair(Of String, String) In tableNameMappingDict
							
								Dim yearIterable As Integer = 2019
								Dim maxYear As Integer = 2027
								Dim yearsDifference As Integer = maxYear - yearIterable
								Dim i As Integer
							
'								BRApi.ErrorLog.LogMessage(si, $"Tabla: {tableNameMapping.Key}")
							
								'Clean the internal table
								deleteQuery = $"DELETE
												FROM {tableNameMapping.Key}"
								
								BRApi.Database.ExecuteSql(dbAppConn, deleteQuery, True)
							
								'Manage not common cases
								'Pre filter in case of CEBES table, else just copy
								If tableNameMapping.Key = "XFC_CEBES" Then
		
									selectQuery = $"SELECT
															cebe,
															description,
															brand,
															city,
															company,
															country,
															location,
															postal_code,
															unit_type,
															region,
															state,
															close_date,
															open_date,
															CASE 
																WHEN sales_brand = 'Unidades VIPS' THEN 'VIPS'
																WHEN sales_brand = 'Unidades VIPS Smart' THEN 'VIPS'
																ELSE sales_brand END AS sales_brand,
															cebe_class
															
														FROM {tableNameMapping.Value}"
								
									'Create a datatable from the external db data
									dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
									
									'Save the external datatable to the application table
									BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
								Else If tableNameMapping.Key = "XFC_ComparativeCEBES" Then
									
									For i = 0 To yearsDifference
									
										selectQuery = $"	SELECT 
																cebe,
																date,
																weeklycomparability,
																annualcomparability,
																descr_weelkycomparability AS desc_weeklycomparability,
																descr_annualcomparability AS desc_annualcomparability
															FROM {tableNameMapping.Value}
															WHERE YEAR(date) = {(yearIterable + i).ToString}"
									
										'Create a datatable from the external db data
										dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
										
										'Save the external datatable to the application table
										BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
									Next
									
								Else If tableNameMapping.Key = "XFC_ComparativeDates" Then
									
									selectQuery = $"
										SELECT
											FECHA_DATE AS date,
											--CASE
											--	WHEN YEAR(FECHA_DATE) = YEAR(FECHA_DATE_AA) THEN DATEADD(YEAR, -1, FECHA_DATE_AA)
											--	ELSE FECHA_DATE_AA
											FECHA_DATE_AA AS date_comparable,
											FECHA_DATE_2AA AS date_comparable_2,
											FECHA_DATE_3AA AS date_comparable_3,
											FECHA_DATE_4AA AS date_comparable_4,
											FECHA_DATE_5AA AS date_comparable_5
										FROM {tableNameMapping.Value}"
								
									'Create a datatable from the external db data
									dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
									
									'Save the external datatable to the application table
									BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
								Else If tableNameMapping.Key = "XFC_TheoreticalCosts" Then
									
									For i = 0 To yearsDifference
									
										selectQuery = $"	SELECT
																date,
																cebe,
																channel3,
																theo_costs_sales_units AS theo_cost_sales_units
															FROM {tableNameMapping.Value}
															WHERE YEAR(date) = {(yearIterable + i).ToString}"
										
										'Create a datatable from the external db data
										dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
										
										'Save the external datatable to the application table
										BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
										
									Next
		
								Else If tableNameMapping.Key = "XFC_FILoadZParam"
									
									selectQuery = $"SELECT active,
														beginning_date,
														description,
														end_date,
														madt,
														modification_date,
														NULL AS modification_hour,
														modification_user,
														parameter,
														value
													FROM {tableNameMapping.Value}"
								
									'Create a datatable from the external db data
									dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
									
									'Save the external datatable to the application table
									BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
								Else If tableNameMapping.Key = "XFC_PL"
									
									For i = 0 To yearsDifference
								
										selectQuery = $"SELECT scenario,
															period,
															accounting_account,
															ceco,
															accumulated_balance,
															currency,
															accounts_chart,
															company
														FROM {tableNameMapping.Value} PL
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
															AND (LEFT(period, 4) <> 2025 OR (LEFT(period, 4) = 2025 AND ceco NOT IN ('ZFDP9100','ZFDP9101')))
										                    AND  ( 
										                          COALESCE(FP.value, 'No Mapeado') = 'No Mapeado'
										                          OR (YEAR(FP.beginning_date) <= {(yearIterable + i).ToString} AND YEAR(FP.end_date) >= {(yearIterable + i).ToString}));"
									
										'Create a datatable from the external db data
										dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
										
										'Save the external datatable to the application table
										BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
										
									Next
									
								Else If tableNameMapping.Key = "XFC_Remodelings"
								
									selectQuery = $"SELECT 	id,
															cebe,
															'' AS description,
															renovation_start_date,
															renovation_end_date
													FROM {tableNameMapping.Value}"
								
									'Create a datatable from the external db data
									dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
									
									'Save the external datatable to the application table
									BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
								Else If tableNameMapping.Key = "XFC_Remodelings" Then
									
									selectQuery = $"	SELECT
															id,
															unit_code,
															description,
															start_date,
															end_date
														FROM {tableNameMapping.Value}"
								
									'Create a datatable from the external db data
									dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
									
									'Save the external datatable to the application table
									BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)									
									
								Else If tableNameMapping.Key = "XFC_Waste"
									
									For i = 0 To yearsDifference
								
										selectQuery = $"SELECT date,
															cebe,
															amount
														FROM {tableNameMapping.Value}
														WHERE YEAR(date) = {(yearIterable + i).ToString}"
									
										'Create a datatable from the external db data
										dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
										
										'Save the external datatable to the application table
									BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
									Next
									
								Else If tableNameMapping.Key = "XFC_RawSales"
									
									For i = 0 To yearsDifference
									
										selectQuery = $"
											SELECT *
											FROM {tableNameMapping.Value}
											WHERE 
												YEAR(date) = {(yearIterable + i).ToString}
												AND cod_channel3 NOT IN (48, 49);"
									
										'Create a datatable from the external db data
										dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
										
										'Save the external datatable to the application table
										BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
									Next
									
								Else
									
									selectQuery = $"SELECT *
													FROM {tableNameMapping.Value}"
								
									'Create a datatable from the external db data
									dt = BRApi.Database.ExecuteSql(dbConn, selectQuery, True)
									
									'Save the external datatable to the application table
									BRApi.Database.SaveCustomDataTable(si, dbLocation.Application.ToString, tableNameMapping.Key, dt, True)
									
								End If
							
							Next
						
						End Using
						
						End Using
							
						'Update closing tables
						SharedFunctionsBR.SetDatesForClosedStores(si)
						
						'Launch sequence to update members
						BRApi.Utilities.ExecuteDataMgmtSequence(
							si,
							BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "Default"),
							"AddNewMembers",
							Nothing
						)
						
						'Import data to cube
						'Get Actual year
						Dim actualYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
						
						'Get Workflow Cluster Pk for PL
						Dim profileNames As New List(Of String) From {
							"Historical.Historical_PL",
							"Historical.Historical_Sales",
							"Historical.Historical_CostofSales",
							"Historical.Historical_PersonnelHours"
						}
						
						'Run a load for each profile name
						For Each profileName As String In profileNames
							Dim wfClusterPk As WorkflowUnitClusterPk = BRApi.Workflow.General.GetWorkflowUnitClusterPk(si, profileName, "Actual", actualYear)
								
							'Clear Stage Data
							BRApi.Import.Process.ClearStageData(si, wfClusterPK, "")
			
							'Execute extraction
							Dim piLoadTransform As LoadTransformProcessInfo = BRApi.Import.Process.ExecuteParseAndTransform(si, wfClusterPK, Nothing, Nothing, TransformLoadMethodTypes.Replace, SourceDataOriginTypes.FromDirectConnection, False)
			
							'Execute validations
							Dim piValidateTransformation As ValidationTransformationProcessInfo = BRApi.Import.Process.ValidateTransformation(si, wfClusterPK, True)
							Dim piValidateIntersections As ValidateIntersectionProcessInfo = BRApi.Import.Process.ValidateIntersections(si, wfClusterPK, True)
			
							'Execute Load & Process
							Dim piLoadCube As LoadCubeProcessInfo = BRApi.Import.Process.LoadCube(si, wfClusterPK)
						Next
						
						'Launch sequence to seed reporting cube
						BRApi.Utilities.ExecuteDataMgmtSequence(
							si,
							BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "Default"),
							"SeedReportingCube",
							Nothing
						)
						
					End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
	End Class
End Namespace