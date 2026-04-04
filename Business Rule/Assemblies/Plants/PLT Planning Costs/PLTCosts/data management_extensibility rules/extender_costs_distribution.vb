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
Imports System.Threading.Tasks


Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_costs_distribution
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						Dim user As String = si.UserName	
						
						Dim shared_queries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries						
						Dim shared_BiBlend As New OneStream.BusinessRule.Extender.UTI_SharedBiBlend.shared_BiBlend						
						Dim shared_functions As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
						Dim sql As String = String.Empty	
						Dim sqlDelete As String = String.Empty	
						Dim sqlFactsInsert As String = String.Empty
						
						#Region "Insert Small"
							
							Dim sw As New System.Diagnostics.Stopwatch()
							
							sw.Start()
							
							' Coger los parámetros buenos en el DA
							Dim tableName As String = "XFC_PLT_FACT_CostsDistribution"
							
							Dim month As String = args.NameValuePairs("month")
							Dim year As String = args.NameValuePairs("year")
							Dim scenario As String = args.NameValuePairs("scenario")
							Dim factory As String = args.NameValuePairs("factory")	
							Dim insertNature As String = args.NameValuePairs("insertNature")
							Dim sMonthFirst As String = If (scenario.StartsWith("RF"), args.NameValuePairs("month_first_forecast"), 1)							
							Dim cecoNatureFilter As String = String.Empty
							
							Dim listInsertNature As New List(Of String) From {"CP", "CAT", "CAA", "UNA", "OTH"}							
							Dim sharedQuery As String = $"CostsDistribution_{insertNature}_NEW"
							
							Dim dt As New DataTable	
							
'					Dim accion As String = "SI"		
'					If accion = "No" Then	
					
						#Region "Create RAW Table"
								
								Dim sqlCreate =	$"
								
									IF EXISTS (SELECT * FROM sys.tables WHERE name = 'XFC_PLT_FACT_CostsDistribution_Raw_{factory}')
									    DROP TABLE XFC_PLT_FACT_CostsDistribution_Raw_{factory}

									    CREATE TABLE XFC_PLT_FACT_CostsDistribution_Raw_{factory} (
									        id_factory VARCHAR(255) NOT NULL,
									        scenario VARCHAR(255) NOT NULL,
									        date DATE NOT NULL,
									        id_account VARCHAR(255) NOT NULL,
									        id_costcenter VARCHAR(255) NOT NULL,
									        id_averagegroup VARCHAR(255) NOT NULL,
									        id_product VARCHAR(255) NOT NULL,
									        value DECIMAL(38, 10) NOT NULL,
									        type_tso_taj VARCHAR(255) NOT NULL
									    );

								"
								
								#End Region
							
						Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							BRApi.Database.ExecuteActionQuery(dbConn, sqlCreate, True, False)
						End Using
						
						#Region "Distribución según escenario"
							
							If scenario = "Actual" And insertNature <> "All" Then
								
								#Region "Actual - Delete"
								
								' Lanzar el reparto solo para el mes actual cerrado
								If (insertNature = "CP" Or insertNature = "CAT" Or insertNature = "CAA") Then
									cecoNatureFilter = $"AND nature = '{insertNature}'"
								Else If insertNature = "UNA"
									cecoNatureFilter = $"AND nature = 'UNA' -- IN('CP', 'CAT', 'CAA')"							
								Else If insertNature = "OTH"
									cecoNatureFilter = $"AND nature NOT IN('CP', 'CAT', 'CAA')"					
								End If															
								sqlDelete = 
									$"
									-- 1- Delete Previo
								
										DELETE A
										
										FROM XFC_PLT_FACT_CostsDistribution A
										
										LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist B
											ON A.id_costcenter = B.id
											AND B.scenario = 'Actual' -- '{scenario}'
											AND A.date BETWEEN B.start_date AND B.end_date
								
										WHERE 1=1
											AND A.scenario = '{scenario}'
											AND A.id_factory = '{factory}'
											AND YEAR(A.date) = {year}
											AND MONTH(A.date) = {month}
											{cecoNatureFilter}
									"	
									
								#End Region
								
								#Region "Actual - Insert"
								
								sql = shared_queries.AllQueries(si, sharedQuery, factory, month, year, scenario)									
								sql = sql & costsDistributionInsert(insertNature, factory)
									
									Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
										brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"Borrando Costes {insertNature}", 30)
'										BRApi.Database.ExecuteSql(dbConn, sqlDelete, True)
										brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"Insertando Costes {insertNature}", 60)
'										BRApi.Database.ExecuteSql(dbConn, sql, True)
'										brapi.ErrorLog.LogMessage(si, sql)
									End Using
									
									sql = String.Empty
									
								#End Region
							
							Else If scenario = "Actual" And insertNature = "All" Then
								
								
								Dim message As String = $"{vbTab}{vbTab}INSERTED - {scenario}{vbLf}{insertNature} Costs Distribution{vbLf}Exec.Time: "
								Dim messageLog As String = $"{vbTab}{vbTab} INSERTED DATA"
								Dim messageLogS As String = String.Empty
								Dim percentage As Decimal = 100.00/6.00
								
								Dim time As Double = sw.Elapsed.TotalSeconds
								
								For Each nature As String In listInsertNature
									
									#Region "Insert - RAW Table"
									
									messageLogS = $"{messageLog}{vbLf}Calculating {nature} costs..."
									brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"{messageLogS}", CInt(percentage))
									
									time = sw.Elapsed.TotalSeconds
									
									sharedQuery = $"CostsDistribution_{nature}_NEW"
																	
									' Lanzar el reparto solo para el mes actual cerrado
									sql = shared_queries.AllQueries(si, sharedQuery, factory, month, year, scenario)							
									sql = sql & costsDistributionInsert(nature, factory)

									#End Region												
									
									Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
										BRApi.Database.ExecuteActionQuery(dbConn, sql, True, False)
									End Using									
																							
									time = sw.Elapsed.TotalSeconds - time
									percentage = percentage + (100.00/6.00)
									
									message = $"{message}{vbLf} {nature}: {time.ToString("0.00")}s"
									messageLog = $"{messageLog}{vbLf}Inserted {nature}: {time.ToString("0.00")}s"
									messageLogS = String.Empty
									
								Next
								
								#Region "FACT Table"	
								Dim monthDeleteFilter As String = If(user = "MiguelTest", $"MONTH(date) <= {month}", $"MONTH(date) = {month}")
								
									#Region "Delete + Insert"
								sqlFactsInsert = $"
									DELETE FROM XFC_PLT_FACT_CostsDistribution
									
									WHERE 1=1
										AND scenario = '{scenario}'
										AND id_factory = '{factory}'
										AND YEAR(date) = {year}
										AND MONTH(date) = {month};
									
									INSERT INTO XFC_PLT_FACT_CostsDistribution (id_factory, scenario, date, id_account, id_costcenter, id_averagegroup, id_product, value, type_tso_taj)
									
									SELECT 
										id_factory
										, scenario
										, date
										, id_account
										, id_costcenter
										, id_averagegroup
										, id_product
										, SUM(value)
										, type_tso_taj
									
									FROM XFC_PLT_FACT_CostsDistribution_Raw_{factory}
									
									GROUP BY id_factory
										, scenario
										, date
										, id_account
										, id_costcenter
										, id_averagegroup
										, id_product
										, type_tso_taj
								;

								"
								#End Region
									
									#Region "Merge"
'								sqlFactsInsert = $"
								
'									MERGE INTO XFC_PLT_FACT_CostsDistribution AS TARGET
								
'									USING (
'										SELECT 
'											id_factory,
'											scenario,
'											date,
'											id_account,
'											id_costcenter,
'											id_averagegroup,
'											id_product,
'											type_tso_taj,
'											SUM(value) AS value
'										FROM XFC_PLT_FACT_CostsDistribution_Raw_{factory}
'										GROUP BY 
'											id_factory,
'											scenario,
'											date,
'											id_account,
'											id_costcenter,
'											id_averagegroup,
'											id_product,
'											type_tso_taj
'									) AS SOURCE								
'									ON
'										TARGET.id_factory = SOURCE.id_factory AND
'										TARGET.scenario   = SOURCE.scenario AND
'										TARGET.date       = SOURCE.date AND
'										TARGET.id_account = SOURCE.id_account AND
'										TARGET.id_costcenter = SOURCE.id_costcenter AND
'										TARGET.id_averagegroup = SOURCE.id_averagegroup AND
'										TARGET.id_product = SOURCE.id_product AND
'										TARGET.type_tso_taj = SOURCE.type_tso_taj
									
'									WHEN MATCHED THEN
'										UPDATE SET 
'											value = SOURCE.value
									
'									WHEN NOT MATCHED THEN
'										INSERT (
'											id_factory, scenario, date, id_account, id_costcenter, id_averagegroup, id_product, value, type_tso_taj
'										) VALUES (
'											SOURCE.id_factory, SOURCE.scenario, SOURCE.date, SOURCE.id_account, SOURCE.id_costcenter, SOURCE.id_averagegroup, SOURCE.id_product, SOURCE.value, SOURCE.type_tso_taj
'										)
								
'									WHEN NOT MATCHED BY SOURCE 
'										AND scenario = '{scenario}' 
'										AND id_factory = '{factory}' 
'										AND YEAR(date) = {year} 
'										AND MONTH(date) = {month}
'									THEN DELETE;
'								"
									#End Region
									
								#End Region
								
								brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"Insert data in Final Table", percentage)								
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlFactsInsert, True, False)
								End Using
								
								message = $"{message}{vbLf}Total: {sw.Elapsed.TotalSeconds.ToString("0.00")}s"
																								
							Else ' Lanzar los meses abiertos de un BUD/FCST
		
								#Region "RAW Table - Insert"
																
								Dim messageLog As String = $"{vbTab}{vbTab} INSERTED DATA{vbLf}Completed Natures: "
								Dim messageLogS As String = String.Empty
								Dim percentage As Decimal = 10.00/6.00
								Dim percentageSub As Decimal = percentage

								' Para inicializar todos los meses en un budget o forecast
								

								For Each nature As String In listInsertNature
									
									messageLogS = $"{messageLog}{vbLf}Calculating {nature}..."
									brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"{messageLogS}", percentage)
								
									sharedQuery = $"CostsDistribution_{nature}_NEW"
									
									' Lanzar el reparto solo para el mes actual cerrado
									sql = shared_queries.AllQueries(si, sharedQuery, factory, month, year, scenario,,,,,,,sMonthFirst-1)							
									sql = sql & costsDistributionInsert(nature, factory)

								#End Region
								
									BRAPI.ErrorLog.LogMessage(si,$"{nature}:" & sql)												
									Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
										BRApi.Database.ExecuteActionQuery(dbConn, sql, True, False)
									End Using	
																		
									percentage = percentage + (10.00/6.00)
									messageLog = $"{messageLog}{nature},"
									messageLogS = String.Empty
								Next								
								
								#Region "FACT Table"		
								
									#Region "Delete + Insert"
									
								sqlFactsInsert = $"
								
									DELETE FROM XFC_PLT_FACT_CostsDistribution
									
									WHERE 1=1
										AND scenario = '{scenario}'
										AND id_factory = '{factory}'
										AND YEAR(date) = {year}
										AND MONTH(date) >= {sMonthFirst};
								
									INSERT INTO XFC_PLT_FACT_CostsDistribution (id_factory, scenario, date, id_account, id_costcenter, id_averagegroup, id_product, value, type_tso_taj)
									
									SELECT 
										id_factory
										, scenario
										, date
										, id_account
										, id_costcenter
										, id_averagegroup
										, id_product
										, SUM(value)
										, type_tso_taj
									
									FROM XFC_PLT_FACT_CostsDistribution_Raw_{factory}
									
									GROUP BY id_factory
										, scenario
										, date
										, id_account
										, id_costcenter
										, id_averagegroup
										, id_product
										, type_tso_taj;
								
									-- DROP TABLE XFC_PLT_FACT_CostsDistribution_Raw_{factory};
								"
								#End Region
									
									#Region "Merge"
									
'								sqlFactsInsert = $"
								
'									MERGE INTO XFC_PLT_FACT_CostsDistribution AS TARGET
								
'									USING (
'										SELECT 
'											id_factory,
'											scenario,
'											date,
'											id_account,
'											id_costcenter,
'											id_averagegroup,
'											id_product,
'											type_tso_taj,
'											SUM(value) AS value
'										FROM XFC_PLT_FACT_CostsDistribution_Raw
'										GROUP BY 
'											id_factory,
'											scenario,
'											date,
'											id_account,
'											id_costcenter,
'											id_averagegroup,
'											id_product,
'											type_tso_taj
'									) AS SOURCE								
'									ON
'										TARGET.id_factory = SOURCE.id_factory AND
'										TARGET.scenario   = SOURCE.scenario AND
'										TARGET.date       = SOURCE.date AND
'										TARGET.id_account = SOURCE.id_account AND
'										TARGET.id_costcenter = SOURCE.id_costcenter AND
'										TARGET.id_averagegroup = SOURCE.id_averagegroup AND
'										TARGET.id_product = SOURCE.id_product AND
'										TARGET.type_tso_taj = SOURCE.type_tso_taj
									
'									WHEN MATCHED THEN
'										UPDATE SET 
'											value = SOURCE.value
									
'									WHEN NOT MATCHED THEN
'										INSERT (
'											id_factory, scenario, date, id_account, id_costcenter, id_averagegroup, id_product, value, type_tso_taj
'										) VALUES (
'											SOURCE.id_factory, SOURCE.scenario, SOURCE.date, SOURCE.id_account, SOURCE.id_costcenter, SOURCE.id_averagegroup, SOURCE.id_product, SOURCE.value, SOURCE.type_tso_taj
'										)
								
'									WHEN NOT MATCHED BY SOURCE 
'										AND scenario = '{scenario}' 
'										AND id_factory = '{factory}' 
'										AND YEAR(date) = {year} 
'										AND MONTH(date) > {sMonthFirst}
'									THEN DELETE;
'								"
									#End Region
									
								#End Region
								
								brapi.TaskActivity.UpdateRunningTaskActivityAndCheckIfCanceled(si, args, $"Insert data in Final Table", percentage)								
								
								Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
									BRApi.Database.ExecuteActionQuery(dbConn, sqlFactsInsert, True, False)
								End Using
										
							End If
							
							#End Region 

							sw.Stop()
							
							brapi.ErrorLog.LogMessage(si, $"COST DISTRIBUTION 0: {scenario}, {year}, {month}, {factory}")
							
							shared_queries.update_FactVTU_Report_NewTables(si, scenario, year, month, factory)	
							
							brapi.ErrorLog.LogMessage(si, $"COST DISTRIBUTION 1: {scenario}, {year}, {month}, {factory}")
							
							Dim tareas As New List(Of Task)
								tareas.Add(Task.Run(Function() shared_BiBlend.CopyTable(si, "XFC_PLT_FACT_Costs_VTU_Final", factory, month, year, scenario)))
								tareas.Add(Task.Run(Function() shared_BiBlend.CopyTable(si, "XFC_PLT_FACT_Costs_VTU_Final_Local",factory, month, year, scenario)))
'								tareas.Add(Task.Run(Function() shared_BiBlend.CopyTable(si, "XFC_PLT_FACT_Costs_VTU_Report_Account", factory, month, year, scenario)))
'								tareas.Add(Task.Run(Function() shared_BiBlend.CopyTable(si, "XFC_PLT_FACT_Costs_VTU_Report_Account_Local", factory, month, year, scenario)))
							Task.WaitAll(tareas.ToArray())
							
							brapi.ErrorLog.LogMessage(si, $"COST DISTRIBUTION 2: {scenario}, {year}, {month}, {factory}")
							
							' shared_queries.update_FactVTU_Report_NewTables(si, scenario, year, month, "All")	 	
							' shared_queries.update_Nomenclature_Report(si, "RF09", year, month)
							
							Return Nothing
							
						#End Region
						
						
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
			
			#Region "PopUp"
			
		Public Function popUp(ByVal message As String) As XFSelectionChangedTaskResult
			
			Dim selectionChangedTaskResult As New XFSelectionChangedTaskResult()

			selectionChangedTaskResult.IsOK = True
			selectionChangedTaskResult.ShowMessageBox = True
			selectionChangedTaskResult.Message = $"{message}"							
			Return selectionChangedTaskResult
			
		End Function
		
			#End Region
			
			#Region "CostDistribution Insert"
			
		Public Function costsDistributionInsert(ByVal cecoNature As String, ByVal factory As String) As String
			
			Dim sqlInsert As String = String.Empty			

			If cecoNature = "NN"
				' Nada Aún
			Else 
				sqlInsert = $"
							INSERT INTO XFC_PLT_FACT_CostsDistribution_Raw_{factory} (id_factory, scenario, date, id_account, id_costcenter, id_averagegroup, id_product, value, type_tso_taj)
							
							SELECT 
								id_factory
								, scenario
								, date
								, id_account
								, id_costcenter
								, id_averagegroup
								, id_product
								, COALESCE(CAST(value AS DECIMAL(18,6)), 0) AS value
								, type_tso_taj
							
							FROM costsDistributionInsert
									
						"
			End If
			
			Return sqlInsert
			
		End Function
		
			#End Region
			
		#End Region		
		
	End Class
End Namespace
