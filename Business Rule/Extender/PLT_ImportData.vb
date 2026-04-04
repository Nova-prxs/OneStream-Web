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

Namespace OneStream.BusinessRule.Extender.PLT_ImportData
	
	Public Class MainClass
		
		#Region "Main"

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				
'				If args.FunctionType = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
					
					Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
					' ----------------------------------------------------------------------------------
					' Todo esto se convertirá en un bucle que recorra todos los ficheros
					'
					'Process the uploaded file
'					Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath", Guid.Empty.ToString)
					' ----------------------------------------------------------------------------------
					
					' RAW DATA
					Dim folderPath As String = "\Documents\Public\Plants\RawData\"
					Dim fileProduction As String = "ProductionData.csv"				
					Dim fileProductionBudget As String = "ProductionBudget.csv"
					Dim fileCosts As String = "CostsData.csv"
					Dim fileCostsDaily As String = "KSB1.txt"
					Dim fileCostsMonthly As String = "ZSYPI.txt"
					Dim fileProjectCosts As String = "ProjectCosts.csv"
					Dim fileCostsAllocations As String = "CostsAllocationKeys.csv"
					Dim fileCostsProd2024 As String = "Production_2024.csv"
					
					' MASTER TABLES
					Dim folderPathM As String = "\Documents\Public\Plants\Master\"
					Dim fileCostCenterMaster As String = "XFC_PLT_MASTER_CostCenter.csv"
					Dim fileCostCenterMasterHist As String = "XFC_PLT_MASTER_CostCenter_Hist.csv"
					Dim fileCostCenterBudget As String = "XFC_PLT_MASTER_CostCenterBudget.csv"
					Dim fileCostCenterHier As String = "XFC_PLT_HIER_CostCenter.csv"
					Dim fileCostCenterHist As String = "XFC_PLT_HIST_CostCenter.csv"
					Dim fileProductMaster As String = "XFC_PLT_MASTER_Product"
					Dim fileProductHier As String = "XFC_PLT_HIER_Product"
					Dim fileAccountMapping As String = "XFC_PLT_MAPPING_Accounts.csv"
					Dim fileProductDesc As String = "XFC_PLT_MASTER_Prod_Desc.csv"
					Dim fileEffects As String = "XFC_PLT_MASTER_Effects.csv"
					
					' CARGA DE DATOS
					
					' Carga de datos MAESTROS
'					Dim loadResultsCeCoMaster As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPathM & fileCostCenterMaster)										
'					Dim loadResultsCeCoMasterHist As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPathM & fileCostCenterMasterHist)
'					Dim loadResultsCeCoBudget As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPathM & fileCostCenterBudget)										
'					Dim loadResultsCeCoHist As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPathM & fileCostCenterHist)										
'					Dim loadResultsCeCoHier As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPathM & fileCostCenterHier)										
'					Me.WriteFileToTable(si, folderPathM & fileProductMaster)	
'					Me.WriteFileToTable(si, folderPathM & fileProductDesc)
'					Me.WriteFileToTable(si, folderPathM & fileProductHier)										
'					Me.WriteFileToTable(si, folderPathM & fileAccountMapping)	
'					Me.WriteFileToTable(si, folderPathM & fileEffects)	
					
'					' Carga de RAW DATA
'					Dim loadResultsProduction As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPath & fileProduction)										
'					Dim loadResultsProductionBudget As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPath & fileProductionBudget)										
'					Dim loadResultsCosts As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPath & fileCosts)	
'					Dim loadResultsCosts As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPath & fileCostsDaily)
'					Dim loadResultsCosts As List(Of TableRangeContent) = Me.WriteFileToTable(si, folderPath & fileCostsMonthly)
'					Dim loadResultsCostsImport As List(Of TableRangeContent) = Me.WriteFileToTable(si, "CostsImport")
					
					' Cargas PERSONALIZADAS					
'					WriteFileToTable(si, fileCostsMonthly, "Si")
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\R0671\TopFabBudgetCosts_SinAjustar.csv")
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\R0671\Costs_TopFab_R0671_2024_M10.csv")
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\R0548913\BudgetCosts2025_OneShot.csv",,"R0548913","2025")
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\R0548913\CostsAllocationKeys.csv",,"R0548913","2025", "Budget")
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\R0548913\EnergyVariance.csv",,"R0548913","2025", "Budget")
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\AverageGroupDescription.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\Production_Budget_Times.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\Production_Budget_Times_NewProducts.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\Costs_Budget_2025.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\Costs_Budget_2025_Cacia.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\ActualCosts2024.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\ProjectCosts.csv",,,"2025",)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\XFC_PLT_FACT_CostsDistribution_Data_CSV.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\Production2024_R0671.csv",,,,)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\" & fileCostsAllocations,,,"2024",)
'					WriteFileToTable(si, "\Documents\Public\Plants\Import\" & fileCostsProd2024,,,"2024","Actual")

'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\ActivityRF02.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\CostsDataRF02.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\CostsFixedVariableRF02.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\EfectosACT_BUD4.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\UNE_ELB.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\XFC_PLT_MASTER_CostCenter_Hist.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\TimesRF02.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\CostsAllocationKeys.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\XFC_PLT_MAPPING_Accounts_Abr.csv")
'					WriteFileToTable(si, $"\Documents\Public\Plants\Import\CostsEffectsRF02.csv")
					
'				End If
			
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

		#End Region

		#Region "ETL"		
		
		Private Function WriteFileToTable(
											ByVal si As SessionInfo, _
											ByVal filePath As String, _
											Optional runInsert As String = "No", _ 
											Optional factory As String = "R0671", _
											Optional year As String = "2025", _
											Optional scenario As String = ""
											) As List(Of TableRangeContent)		
		
			Try
				Dim loadResults As New List(Of TableRangeContent)
				
				'Get the upload "TEMP" file from the database file system
				Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase,filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
					
				'Load Delimited File
				'Note: dbLocation, TableName, LoadMethod & Field names must be defined as variables and passed into the load method
				Dim dbLocation As String = "Application" ' "Application"
				Dim loadMethod As String = "Replace"	 ' Use an expression for partial replace -->  Merge:[(ProductName = 'SomeProduct')] 								
				
'				Replace:[Escenario='Budget1' and Entidad='006'and APPTO='2024']

				Dim fieldTokens As New List(Of String)
				
				Dim tableName As String = String.Empty
				Dim sqlRaw As String = String.Empty
				Dim sqlMap As String = String.Empty		

				brapi.ErrorLog.LogMessage(si, filePath)
				
		#Region "PRODUCTION"
		
			#Region "PRODUCTION - ALL Factories"
		
				If filePath.Contains("Production_2024.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Production 
							 	(
								[id_factory] varchar(50) ,
								[date] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_product] varchar(50) ,
								[uocat] varchar(50) ,
								[uotype] varchar(50) ,
								[volume] decimal(18,0),
								[allocation_taj] decimal(18,4),
								[activity_taj] decimal(18,4),
								[allocation_tso] decimal(18,4),
								[activity_tso] decimal(18,4)
							 	)"

					tableName = "XFC_PLT_RAW_Production"	
'					tableDest = "XFC_PLT_FACT_Production"
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[date]")
					fieldTokens.Add("xfText#:[id_averagegroup]::-1")
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_product]::-1")
					fieldTokens.Add("xfText#:[product_description]::-1")
					fieldTokens.Add("xfDec#:[volume]::0")											
					fieldTokens.Add("xfText#:[uocat]::-1")
					fieldTokens.Add("xfText#:[uotype]::-1")
					fieldTokens.Add("xfDec#:[allocation_tso]::0")											
					fieldTokens.Add("xfDec#:[allocation_taj]::0")											
					fieldTokens.Add("xfDec#:[activity_tso]::0")	
					fieldTokens.Add("xfDec#:[activity_taj]::0")											
					
					sqlMap = $"	
								-- 1- Previous clear
								DELETE FROM XFC_PLT_FACT_Production
								WHERE 1=1
									AND scenario = '{scenario}'
									-- AND (id_factory IN (SELECT id_factory FROM XFC_PLT_RAW_Production GROUP BY id_factory)
									-- OR id_factory = 'R0458913' OR id_factory = 'R0548913')
									AND YEAR(date) = {year}
									
						
					
								-- 2- Insert statement
								
								INSERT INTO XFC_PLT_FACT_Production (id_factory, date, scenario, id_averagegroup, id_workcenter, id_costcenter, id_product, uotype, volume, allocation_taj, activity_taj, allocation_tso, activity_tso)
								
								SELECT *
								FROM (
								SELECT 
									CASE WHEN id_factory = 'R04548913' THEN 'R0548913'ELSE id_factory END as id_factory 
									, CONVERT(DATE, date, 112) as date
									, 'Actual' as scenario 
									, id_averagegroup 
									, '-1' as id_workcenter 
									, id_costcenter 
									, id_product
									, CASE 
										WHEN uocat = 'UO1' AND uotype <> 'P P' AND uotype <> 'CUR' THEN 'UO1'
										WHEN uocat = 'UO2' AND uotype <> 'P P' AND uotype <> 'CUR' THEN 'UO2'
										ELSE '-1' 
									END AS uotype
									, sum(volume) as volume 
									, CASE 
										WHEN SUM(volume) = 0 THEN 0 
										ELSE SUM(volume * allocation_taj) / sum(volume) 
									END as allocation_taj
									, sum(activity_taj) as activity_taj
									, CASE 
										WHEN SUM(volume) = 0 THEN 0 
										ELSE SUM(volume * allocation_tso) / sum(volume) 
									END as allocation_tso
									, sum(activity_tso) as activity_tso
					
					
								FROM XFC_PLT_RAW_Production 
					
								WHERE 1=1						
									AND YEAR(CONVERT(DATE, date, 112)) = {year}						
					
								GROUP BY id_factory, CONVERT(DATE, date, 112), id_costcenter, id_averagegroup, id_product, uocat, uotype
								) as ext
					
								WHERE 1=1
									AND uotype <> '-1'
					"

			#End Region		
					
			#Region "PRODUCTION - ALL Factories (Budget)"					
					
				Else If filePath.Contains("ProductionBudget.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Production 
							 	(
								[id_factory] varchar(50) ,
								[date] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_product] varchar(50) ,
								[uotype] varchar(50) ,
								[volume] decimal(18,0),
								[allocation_taj] decimal(18,2),
								[activity_taj] decimal(18,2),
								[allocation_tso] decimal(18,2),
								[activity_tso] decimal(18,2)
							 	)"					
					
					tableName = "XFC_PLT_RAW_Production"
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[date]")
					fieldTokens.Add("xfText#:[id_averagegroup]::-1")
					fieldTokens.Add("xfText#:[id_costcenter]::-1")
					fieldTokens.Add("xfText#:[id_product]::-1")
					fieldTokens.Add("xfText#:[uotype]::-1")
					fieldTokens.Add("xfDec#:[volume]::0")											
					fieldTokens.Add("xfDec#:[allocation_taj]::0")											
					fieldTokens.Add("xfDec#:[activity_taj]::0")											
					fieldTokens.Add("xfDec#:[allocation_tso]::0")											
					fieldTokens.Add("xfDec#:[activity_tso]::0")	
					
					sqlMap = "	INSERT INTO XFC_PLT_FACT_Production (id_factory, date, scenario, id_averagegroup, id_workcenter, id_costcenter, id_product, uotype, volume, allocation_taj, activity_taj)
					
								SELECT 
										A.id_factory, 
										CONVERT(DATE, A.date, 112), 
										'Budget' as scenario, 
										-- COALESCE(B.averagegroup, A.id_averagegroup) as id_averagegroup, 
										A.id_averagegroup,
										'-1' as id_workcenter, 
										A.id_costcenter, 
										A.id_product, 
										A.uotype, 
										sum(volume) as volume, 
										CASE WHEN SUM(volume) = 0 THEN 0 ELSE SUM(volume * allocation_taj) / sum(volume) END as allocation_taj, 
										sum(activity_taj) as activity_taj
					
								FROM XFC_PLT_RAW_Production A
					
								-- LEFT JOIN XFC_PLT_MASTER_CostCenter B
									-- ON A.id_costcenter = B.id
					
								GROUP BY A.id_factory, CONVERT(DATE, A.date, 112), A.id_costcenter, A.id_averagegroup, A.id_product, A.uotype"
			#End Region
			
			#Region "PRODUCTION - Actual 2024"
		
				Else If filePath.Contains("Production2024.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Production 
							 	(
								[id_factory] varchar(50) ,
								[date] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_product] varchar(50) ,
								[desc_product] varchar(300) ,
								[volume] decimal(18,0),					
								[uocat] varchar(50) ,
								[uotype] varchar(50) ,
								[allocation_tso] decimal(18,2),					
								[allocation_taj] decimal(18,2),
								[activity_tso] decimal(18,2),					
								[activity_taj] decimal(18,2)
							 	)"

					tableName = "XFC_PLT_RAW_Production"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[date]")
					fieldTokens.Add("xfText#:[id_averagegroup]::-1")
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_product]::-1")
					fieldTokens.Add("xfText#:[desc_product]")					
					fieldTokens.Add("xfDec#:[volume]::0")						
					fieldTokens.Add("xfText#:[uocat]::-1")
					fieldTokens.Add("xfText#:[uotype]::-1")		
					fieldTokens.Add("xfDec#:[allocation_tso]::0")					
					fieldTokens.Add("xfDec#:[allocation_taj]::0")		
					fieldTokens.Add("xfDec#:[activity_tso]::0")						
					fieldTokens.Add("xfDec#:[activity_taj]::0")																						
					
					sqlMap = "	DELETE FROM XFC_PLT_FACT_Production
								WHERE YEAR(date) = 2024
								AND id_factory IN (SELECT DISTINCT id_factory FROM XFC_PLT_RAW_Production);
					
								INSERT INTO XFC_PLT_FACT_Production (
									id_factory
									, date
									, scenario
									, id_averagegroup
									, id_workcenter
									, id_costcenter
									, id_product
									, uotype
									, volume
									, allocation_taj
									, activity_taj)
					
								SELECT 
									id_factory
									, CONVERT(DATE, date, 112)
									, 'Actual' as scenario
									, id_averagegroup
									, -1 as id_workcenter
									, id_costcenter
									, id_product
									, uocat
									, SUM(volume) as volume
									, CASE WHEN SUM(volume) = 0 THEN 0 
										   ELSE SUM(activity_taj) / SUM(volume) END as allocation_taj
									, SUM(activity_taj) as activity_taj
					
								FROM XFC_PLT_RAW_Production 
					
								GROUP BY id_factory, CONVERT(DATE, date, 112), id_costcenter, id_averagegroup, id_product, uocat"

			#End Region				
			
			#Region "PRODUCTION - Budget 2025 Times (New Products)"
		
				Else If filePath.Contains("Production_Budget_Times_NewProducts.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Production_Budget_Times_NewProducts
							 	(
								[id_factory] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[id_product] varchar(50) ,
								[M01_A10] decimal(18,6),
								[M01_A20] decimal(18,6),
								[M02_A10] decimal(18,6),
								[M02_A20] decimal(18,6),		
								[M03_A10] decimal(18,6),
								[M03_A20] decimal(18,6),	
								[M04_A10] decimal(18,6),
								[M04_A20] decimal(18,6),	
								[M05_A10] decimal(18,6),
								[M05_A20] decimal(18,6),
								[M06_A10] decimal(18,6),
								[M06_A20] decimal(18,6),							
								[M07_A10] decimal(18,6),
								[M07_A20] decimal(18,6),
								[M08_A10] decimal(18,6),
								[M08_A20] decimal(18,6),		
								[M09_A10] decimal(18,6),
								[M09_A20] decimal(18,6),	
								[M10_A10] decimal(18,6),
								[M10_A20] decimal(18,6),	
								[M11_A10] decimal(18,6),
								[M11_A20] decimal(18,6),
								[M12_A10] decimal(18,6),
								[M12_A20] decimal(18,6)					
							 	)"

					tableName = "XFC_PLT_RAW_Production_Budget_Times_NewProducts"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_averagegroup]")
					fieldTokens.Add("xfText#:[id_product]")										
					fieldTokens.Add("xfDec#:[M01_A10]::0")											
					fieldTokens.Add("xfDec#:[M01_A20]::0")	
					fieldTokens.Add("xfDec#:[M02_A10]::0")											
					fieldTokens.Add("xfDec#:[M02_A20]::0")	
					fieldTokens.Add("xfDec#:[M03_A10]::0")											
					fieldTokens.Add("xfDec#:[M03_A20]::0")	
					fieldTokens.Add("xfDec#:[M04_A10]::0")											
					fieldTokens.Add("xfDec#:[M04_A20]::0")	
					fieldTokens.Add("xfDec#:[M05_A10]::0")											
					fieldTokens.Add("xfDec#:[M05_A20]::0")	
					fieldTokens.Add("xfDec#:[M06_A10]::0")											
					fieldTokens.Add("xfDec#:[M06_A20]::0")		
					fieldTokens.Add("xfDec#:[M07_A10]::0")											
					fieldTokens.Add("xfDec#:[M07_A20]::0")	
					fieldTokens.Add("xfDec#:[M08_A10]::0")											
					fieldTokens.Add("xfDec#:[M08_A20]::0")	
					fieldTokens.Add("xfDec#:[M09_A10]::0")											
					fieldTokens.Add("xfDec#:[M09_A20]::0")	
					fieldTokens.Add("xfDec#:[M10_A10]::0")											
					fieldTokens.Add("xfDec#:[M10_A20]::0")	
					fieldTokens.Add("xfDec#:[M11_A10]::0")											
					fieldTokens.Add("xfDec#:[M11_A20]::0")	
					fieldTokens.Add("xfDec#:[M12_A10]::0")											
					fieldTokens.Add("xfDec#:[M12_A20]::0")						
					
					'Dim sqlMap2 As String = "	DELETE FROM XFC_PLT_AUX_Production_Planning_Times WHERE YEAR(date) = 2025 AND scenario = 'Budget';
					sqlMap = "	DELETE From XFC_PLT_AUX_Production_Planning_Times_NewProducts Where YEAR(Date) = 2025 And scenario = 'Budget';
					
								INSERT INTO XFC_PLT_AUX_Production_Planning_Times_NewProducts (scenario, date, id_factory, id_costcenter, id_averagegroup, id_product, uotype, comment, value)
					
								SELECT 'Budget', DATEFROMPARTS(2025,1,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M01_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,1,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M01_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,2,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M02_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,2,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M02_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,3,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M03_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,3,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M03_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,4,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M04_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,4,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M04_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,5,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M05_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,5,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M05_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,6,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M06_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,6,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M06_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,7,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M07_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,7,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M07_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,8,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M08_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,8,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M08_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,9,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M09_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,9,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M09_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,10,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M10_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,10,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M10_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,11,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M11_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,11,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M11_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL	
								SELECT 'Budget', DATEFROMPARTS(2025,12,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M12_A10 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,12,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M12_A20 FROM XFC_PLT_RAW_Production_Budget_Times_NewProducts
							"

			#End Region				
			
			#Region "PRODUCTION - Budget 2025 Times"
		
				Else If filePath.Contains("Production_Budget_Times.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Production_Budget_Times 
							 	(
								[id_factory] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[id_product] varchar(50) ,
								[M01_A10] decimal(18,6),
								[M01_A20] decimal(18,6),
								[M02_A10] decimal(18,6),
								[M02_A20] decimal(18,6),		
								[M03_A10] decimal(18,6),
								[M03_A20] decimal(18,6),	
								[M04_A10] decimal(18,6),
								[M04_A20] decimal(18,6),	
								[M05_A10] decimal(18,6),
								[M05_A20] decimal(18,6),
								[M06_A10] decimal(18,6),
								[M06_A20] decimal(18,6),							
								[M07_A10] decimal(18,6),
								[M07_A20] decimal(18,6),
								[M08_A10] decimal(18,6),
								[M08_A20] decimal(18,6),		
								[M09_A10] decimal(18,6),
								[M09_A20] decimal(18,6),	
								[M10_A10] decimal(18,6),
								[M10_A20] decimal(18,6),	
								[M11_A10] decimal(18,6),
								[M11_A20] decimal(18,6),
								[M12_A10] decimal(18,6),
								[M12_A20] decimal(18,6)					
							 	)"

					tableName = "XFC_PLT_RAW_Production_Budget_Times"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_averagegroup]")
					fieldTokens.Add("xfText#:[id_product]")										
					fieldTokens.Add("xfDec#:[M01_A10]::0")											
					fieldTokens.Add("xfDec#:[M01_A20]::0")	
					fieldTokens.Add("xfDec#:[M02_A10]::0")											
					fieldTokens.Add("xfDec#:[M02_A20]::0")	
					fieldTokens.Add("xfDec#:[M03_A10]::0")											
					fieldTokens.Add("xfDec#:[M03_A20]::0")	
					fieldTokens.Add("xfDec#:[M04_A10]::0")											
					fieldTokens.Add("xfDec#:[M04_A20]::0")	
					fieldTokens.Add("xfDec#:[M05_A10]::0")											
					fieldTokens.Add("xfDec#:[M05_A20]::0")	
					fieldTokens.Add("xfDec#:[M06_A10]::0")											
					fieldTokens.Add("xfDec#:[M06_A20]::0")		
					fieldTokens.Add("xfDec#:[M07_A10]::0")											
					fieldTokens.Add("xfDec#:[M07_A20]::0")	
					fieldTokens.Add("xfDec#:[M08_A10]::0")											
					fieldTokens.Add("xfDec#:[M08_A20]::0")	
					fieldTokens.Add("xfDec#:[M09_A10]::0")											
					fieldTokens.Add("xfDec#:[M09_A20]::0")	
					fieldTokens.Add("xfDec#:[M10_A10]::0")											
					fieldTokens.Add("xfDec#:[M10_A20]::0")	
					fieldTokens.Add("xfDec#:[M11_A10]::0")											
					fieldTokens.Add("xfDec#:[M11_A20]::0")	
					fieldTokens.Add("xfDec#:[M12_A10]::0")											
					fieldTokens.Add("xfDec#:[M12_A20]::0")						
					
					'Dim sqlMap2 As String = "	DELETE FROM XFC_PLT_AUX_Production_Planning_Times WHERE YEAR(date) = 2025 AND scenario = 'Budget';
					sqlMap = "	DELETE From XFC_PLT_AUX_Production_Planning_Times Where YEAR(Date) = 2025 And scenario = 'Budget';
					
								INSERT INTO XFC_PLT_AUX_Production_Planning_Times (scenario, date, id_factory, id_costcenter, id_averagegroup, id_product, uotype, comment, value)
					
								SELECT 'Budget', DATEFROMPARTS(2025,1,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M01_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,1,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M01_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,2,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M02_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,2,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M02_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,3,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M03_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,3,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M03_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,4,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M04_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,4,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M04_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,5,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M05_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,5,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M05_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,6,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M06_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,6,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M06_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,7,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M07_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,7,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M07_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,8,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M08_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,8,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M08_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,9,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M09_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,9,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M09_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,10,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M10_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,10,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M10_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,11,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M11_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,11,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M11_A20 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL	
								SELECT 'Budget', DATEFROMPARTS(2025,12,1), id_factory, '-1', id_averagegroup, id_product, 'A10', '', M12_A10 FROM XFC_PLT_RAW_Production_Budget_Times
								UNION ALL
								SELECT 'Budget', DATEFROMPARTS(2025,12,1), id_factory, '-1', id_averagegroup, id_product, 'A20', '', M12_A20 FROM XFC_PLT_RAW_Production_Budget_Times
							"

			#End Region					
			
			#Region "PRODUCTION - RF02 (Budget_V4)"
			
				#Region "Activity"
				Else If filePath.Contains("ActivityRF02.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Production 
							 	(
								[id_factory] varchar(50) ,
								[date] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[uocat] varchar(50) ,
								[uotype] varchar(50) ,
								[activity_taj] decimal(18,2)
							 	)"

					tableName = "XFC_PLT_RAW_Production"

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[id_averagegroup]::-1")
					fieldTokens.Add("xfText#:[uotype]")
					fieldTokens.Add("xfText#:[uocat]")
					fieldTokens.Add("xfText#:[date]")					
					fieldTokens.Add("xfDec#:[activity_taj]::0")																						
					
					sqlMap = "	DELETE FROM XFC_PLT_FACT_Production
								WHERE scenario = 'Budget_V4';
					
								INSERT INTO XFC_PLT_FACT_Production (
									id_factory
									, date
									, scenario
									, id_averagegroup
									, id_workcenter
									, id_costcenter
									, id_product
									, uotype
									, volume
									, allocation_taj
									, activity_taj)
					
								SELECT 
									id_factory
									, CAST(CONCAT(SUBSTRING(date, 4, 4), '-', SUBSTRING(date, 1, 2), '-01') AS DATE) as date
									, 'Budget_V4' as scenario
									, id_averagegroup
									, -1 as id_workcenter
									, '-1' as id_costcenter
									, '-1' as id_product
									, uocat
									, 0 as volume
									, 0 as allocation_taj
									, SUM(activity_taj) as activity_taj
					
								FROM XFC_PLT_RAW_Production 
					
								GROUP BY id_factory, CAST(CONCAT(SUBSTRING(date, 4, 4), '-', SUBSTRING(date, 1, 2), '-01') AS DATE), id_averagegroup, uocat
					"

				#End Region	
				
				#Region "Times"
				Else If filePath.Contains("TimesRF02.csv") Then
				
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_TimesRF02
							 	(
								[id_factory] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[id_product] varchar(50) ,
								[UO1_M01] decimal(18,2) ,
								[UO2_M01] decimal(18,2) ,
								[UO1_M02] decimal(18,2) ,
								[UO2_M02] decimal(18,2) ,
								[UO1_M03] decimal(18,2) ,
								[UO2_M03] decimal(18,2) ,
								[UO1_M04] decimal(18,2) ,
								[UO2_M04] decimal(18,2) ,
								[UO1_M05] decimal(18,2) ,
								[UO2_M05] decimal(18,2) ,
								[UO1_M06] decimal(18,2) ,
								[UO2_M06] decimal(18,2) ,
								[UO1_M07] decimal(18,2) ,
								[UO2_M07] decimal(18,2) ,
								[UO1_M08] decimal(18,2) ,
								[UO2_M08] decimal(18,2) ,
								[UO1_M09] decimal(18,2) ,
								[UO2_M09] decimal(18,2) ,
								[UO1_M10] decimal(18,2) ,
								[UO2_M10] decimal(18,2) ,
								[UO1_M11] decimal(18,2) ,
								[UO2_M11] decimal(18,2) ,
								[UO1_M12] decimal(18,2) ,
								[UO2_M12] decimal(18,2) 
							 	)" 

					tableName = "XFC_PLT_RAW_TimesRF02"

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_averagegroup]")
					fieldTokens.Add("xfText#:[id_product]")				
					fieldTokens.Add("xfDec#:[UO1_M01]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M01]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M02]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M02]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M03]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M03]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M04]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M04]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M05]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M05]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M06]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M06]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M07]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M07]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M08]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M08]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M09]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M09]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M10]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M10]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M11]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M11]::0")																						
					fieldTokens.Add("xfDec#:[UO1_M12]::0")																						
					fieldTokens.Add("xfDec#:[UO2_M12]::0")																						
					
					sqlMap = "	DELETE FROM XFC_PLT_AUX_Production_Planning_Times
								WHERE scenario = 'Budget_V4';
					
								INSERT INTO XFC_PLT_AUX_Production_Planning_Times (
									scenario
									, date
									, id_factory
									, id_averagegroup
									, id_costcenter
									, id_product
									, uotype
									, value
								)
					
								SELECT 
								    'Budget_V4' AS scenario,
								    DATEFROMPARTS(2025, CAST(SUBSTRING(mes, 6, 2) AS INT), 1) AS date,
								    id_factory,
								    id_averagegroup,
								    '-1' AS id_costcenter,
								    id_product,
								    CASE 
								        WHEN LEFT(mes, 3) = 'UO1' THEN 'UO1'
								        WHEN LEFT(mes, 3) = 'UO2' THEN 'UO2'
								    END AS uotype,
								    value
								FROM (
								    SELECT 
								        id_factory,
								        id_averagegroup,
								        id_product,
								        [UO1_M01], [UO1_M02], [UO1_M03], [UO1_M04], [UO1_M05], [UO1_M06],
								        [UO1_M07], [UO1_M08], [UO1_M09], [UO1_M10], [UO1_M11], [UO1_M12],
								        [UO2_M01], [UO2_M02], [UO2_M03], [UO2_M04], [UO2_M05], [UO2_M06],
								        [UO2_M07], [UO2_M08], [UO2_M09], [UO2_M10], [UO2_M11], [UO2_M12]
								    FROM XFC_PLT_RAW_TimesRF02
								) p
								UNPIVOT (
								    value FOR mes IN (
								        [UO1_M01], [UO1_M02], [UO1_M03], [UO1_M04], [UO1_M05], [UO1_M06],
								        [UO1_M07], [UO1_M08], [UO1_M09], [UO1_M10], [UO1_M11], [UO1_M12],
								        [UO2_M01], [UO2_M02], [UO2_M03], [UO2_M04], [UO2_M05], [UO2_M06],
								        [UO2_M07], [UO2_M08], [UO2_M09], [UO2_M10], [UO2_M11], [UO2_M12]
								    )
								) unpvt
					"

				#End Region
				
			#End Region
			
		#End Region				
				
		#Region "COSTS"				
				
			#Region "COSTS (File) - ALL Factories"
			
				Else If filePath.Contains("CostsData.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Costs 
								(
								[date] varchar(50) ,
								[id_factory] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_piece] varchar(50) ,
								[id_account] varchar(50) ,
								[value] decimal(18,2)
								)"
													
					tableName = "XFC_PLT_RAW_Costs"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_piece]")
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfDec#:[value]::0")
					fieldTokens.Add("xfText#:[date]")
					
					sqlMap = "	INSERT INTO XFC_PLT_FACT_Costs (id_factory, date, scenario, id_piece, id_costcenter, id_account, value)
					
								SELECT id_factory, CONVERT(DATE, DATEADD(DAY, date -2, '1900-01-01')) as date, 'Actual' as scenario, id_piece, id_costcenter, id_account, 
								SUM(value) AS value
																
								FROM XFC_PLT_RAW_Costs 
									
								GROUP BY id_factory, date, id_piece, id_costcenter, id_account
								
							"
		
			#End Region

			#Region "COSTS - Budget 2025"
		
				Else If filePath.Contains("Costs_Budget_2025_Cacia.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Costs_Budget_2025
							 	(
								[id_factory] varchar(50) ,
								[month] int ,
								[year] int ,
								[id_costcenter] varchar(50) ,
								[id_account] varchar(50) ,
								[value] decimal(18,2)				
							 	)"

					tableName = "XFC_PLT_RAW_Costs_Budget_2025"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfInt#:[month]")
					fieldTokens.Add("xfInt#:[year]")		
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_account]")					
					fieldTokens.Add("xfDec#:[value]::0")											
					
					sqlMap = "	DELETE FROM XFC_PLT_FACT_Costs 
								WHERE YEAR(date) = 2025 
								AND scenario = 'Budget'
								AND id_factory IN ('R0671');
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, date, id_factory, id_account, id_costcenter, value, currency)
					
								SELECT 
									'Budget'
									, DATEFROMPARTS(year, month, 1) AS date								
									, id_factory
									, id_account
									, id_costcenter
									, value
									, 'Local'
								FROM XFC_PLT_RAW_Costs_Budget_2025
								WHERE value <> 0
							"

			#End Region	
			
			#Region "COSTS - Budget 2025"
		
				Else If filePath.Contains("Costs_Budget_2025.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Costs_Budget_2025
							 	(
								[id_factory] varchar(50) ,
								[month] int ,
								[year] int ,
								[id_costcenter] varchar(50) ,
								[id_account] varchar(50) ,
								[value] decimal(18,2)				
							 	)"

					tableName = "XFC_PLT_RAW_Costs_Budget_2025"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfInt#:[month]")
					fieldTokens.Add("xfInt#:[year]")		
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_account]")					
					fieldTokens.Add("xfDec#:[value]::0")											
					
					sqlMap = "	DELETE FROM XFC_PLT_FACT_Costs 
								WHERE YEAR(date) = 2025 
								AND (scenario = 'Budget' OR scenario = 'Budget_V3')
								-- AND id_factory IN ('R0045106','R0483003','R0529002','R0548914','R0592','R0585');
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, date, id_factory, id_account, id_costcenter, value, currency)
					
								SELECT 
									'Budget_V3'
									, DATEFROMPARTS(year, month, 1) AS date								
									, CASE WHEN id_factory = 'R0573003' THEN 'R0592' ELSE id_factory END AS id_factory
									, id_account
									, id_costcenter
									, value
									, 'Local'
								FROM XFC_PLT_RAW_Costs_Budget_2025
								WHERE 1=1
									AND value <> 0
									AND month <> -1
								
							"

			#End Region
			
			#Region "COSTS - Budget 2025 - OLD"
		
'				Else If filePath.Contains("Costs_Budget_2025") Then
					
'					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Costs_Budget_2025
'							 	(
'								[id_factory] varchar(50) ,
'								[month] int ,
'								[year] int ,
'								[id_costcenter] varchar(50) ,
'								[id_account] varchar(50) ,
'								[value] decimal(18,2)				
'							 	)"

'					tableName = "XFC_PLT_RAW_Costs_Budget_2025"	
					
'					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

'					fieldTokens.Add("xfText#:[id_factory]")
'					fieldTokens.Add("xfInt#:[month]")
'					fieldTokens.Add("xfInt#:[year]")		
'					fieldTokens.Add("xfText#:[id_costcenter]")
'					fieldTokens.Add("xfText#:[id_account]")					
'					fieldTokens.Add("xfDec#:[value]::0")											
					
'					sqlMap = "	DELETE FROM XFC_PLT_FACT_Costs 
'								WHERE YEAR(date) = 2025 
'								AND scenario = 'Budget'
'								AND id_factory IN ('R0045106','R0483003','R0529002','R0548914','R0592','R0585');
					
'								INSERT INTO XFC_PLT_FACT_Costs (scenario, date, id_factory, id_account, id_costcenter, value, currency)
					
'								SELECT 
'									'Budget'
'									, DATEFROMPARTS(year, month, 1) AS date								
'									, CASE WHEN id_factory = 'R0573003' THEN 'R0592' ELSE id_factory END AS id_factory
'									, id_account
'									, id_costcenter
'									, value
'									, 'Local'
'								FROM XFC_PLT_RAW_Costs_Budget_2025
'								WHERE value <> 0
'							"

			#End Region			
			
			#Region "COSTS - Actual Costs 2024"			
			
				Else If filePath.contains("ActualCosts2024.csv") Then
					'BRApi.ErrorLog.LogMessage(si, "Entra")
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_TopFabActualCosts
								(
								[id_factory] varchar(50) ,
								[id_account] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					tableName = "XFC_PLT_RAW_TopFabActualCosts"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")					
					
					' Para insertarlo en la tabla de hechos de costes
					sqlMap = $"	DELETE FROM XFC_PLT_FACT_Costs
								WHERE scenario = 'Actual'							
								AND YEAR(date) = 2024
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_account, id_costcenter, date, value, id_factory)
								SELECT DISTINCT
									'Actual' as scenario,
								    id_account,
								    id_costcenter,
								    DATEFROMPARTS(2024, mes, 1) as date,
									SUM(valor) AS value,
									id_factory As factory
								FROM
								    (
								        SELECT id_factory, id_account, id_costcenter, 1 AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 2, M02 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 3, M03 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 4, M04 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 5, M05 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 6, M06 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 7, M07 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 8, M08 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 9, M09 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 10, M10 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 11, M11 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 12, M12 FROM {tableName}
								    ) AS expanded	
								
								GROUP BY id_factory, id_account, id_costcenter, mes
							"		
			#End Region	
			
			#Region "COSTS - RF02 (Budget_V4)"
			
				Else If filePath.contains("CostsDataRF02.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_CostsBudgetV4
								(
								[id_factory] varchar(50) ,
								[id_account] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					tableName = "XFC_PLT_RAW_CostsBudgetV4"	
					
					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")					
					
					' Para insertarlo en la tabla de hechos de costes
					sqlMap = $"	DELETE FROM XFC_PLT_FACT_Costs
								WHERE scenario = 'Budget_V4'
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_account, id_costcenter, date, value, id_factory)
								SELECT DISTINCT
									'Budget_V4' as scenario,
								    id_account,
								    id_costcenter,
								    DATEFROMPARTS(2025, mes, 1) as date,
									SUM(valor) AS value,
									id_factory As factory
								FROM
								    (
								        SELECT id_factory, id_account, id_costcenter, 1 AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 2, M02 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 3, M03 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 4, M04 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 5, M05 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 6, M06 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 7, M07 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 8, M08 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 9, M09 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 10, M10 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 11, M11 FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, id_costcenter, 12, M12 FROM {tableName}
								    ) AS expanded	
								
								GROUP BY id_factory, id_account, id_costcenter, mes
							"		
				Else If filePath.contains("KeyAllocationsRF02.csv") Then

			
			#End Region
			
			#Region "COSTS (Daily) - ALL Factories"
			
				Else If filePath.Contains("KSB1") Then
					
					Dim fullPath As String = FileSystemLocation.ApplicationDatabase.ToString & filePath

'					' Verificar si el archivo existe
'					If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath) Then
'						'Brapi.ErrorLog.LogMessage (si, "Entra")
					
'						Dim contentFileBytes As Byte() = fileInfo.XFFile.ContentFileBytes
						
'						' Paso 1: Convertir los bytes a texto
'						Dim fileContent As String = Encoding.UTF8.GetString(contentFileBytes)
						
'						' Paso 2: Dividir el contenido en líneas
'						Dim lines() As String = fileContent.Split({Environment.NewLine}, StringSplitOptions.None)
						
'						' Paso 3: Filtrar las líneas (ejemplo: eliminar líneas que contienen "eliminar")
'						Dim filteredLines As New List(Of String)
'						For Each line As String In lines
'							If Not line.StartsWith("|*") AndAlso Not line.StartsWith("-") Then
'								filteredLines.Add(line)
'							End If
'						Next
						
'						' Paso 4: Unir las líneas filtradas en un solo texto
'						Dim newContent As String = String.Join(Environment.NewLine, filteredLines)
						
'						' Paso 5: Convertir el nuevo contenido de texto a bytes
'						Dim newContentBytes As Byte() = Encoding.UTF8.GetBytes(newContent)

'						' Paso 6: Sobrescribir el archivo original (o trabajar con los bytes)
'						File.WriteAllBytes(fullPath, newContentBytes)
						
'					End If
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_CostsDaily
								(
								[data] varchar(500) 
								)"
													
					tableName = "XFC_PLT_RAW_CostsDaily"	
					
					fieldTokens.Add("xfText#:[data]")
					
					sqlMap = "	WITH data AS (
								
									SELECT
										'R0671' AS id_factory
										, REPLACE(TRIM(SUBSTRING(data, 271, 11)),'|','') AS date
									    , REPLACE(TRIM(SUBSTRING(data, 25, 10)),'|','') AS id_costcenter
								        , REPLACE(TRIM(SUBSTRING(data, 47, 8)),'|','') AS id_account
										, REPLACE(TRIM(SUBSTRING(data, 257, 14)),'|','') AS value
								
									FROM XFC_PLT_RAW_CostsDaily  
									WHERE data LIKE '|  %' AND data NOT LIKE '|  N%' 
								)
					
								DELETE F
								FROM XFC_PLT_FACT_CostsDaily F 
								INNER JOIN data D
									ON F.id_factory = D.id_factory
									AND F.date = CONVERT(Date, D.[date], 104)
								WHERE F.scenario = 'Actual'
										
								INSERT INTO XFC_PLT_FACT_CostsDaily (id_factory, date, scenario, id_costcenter, id_account, value)							
					
								SELECT  
									id_factory
								    , CONVERT(Date, [date], 104) as date
								    , 'Actual' as scenario
								    , id_costcenter
								    , id_account
								
									,SUM(CAST(
								        REPLACE(
								            REPLACE(
								                CASE 
								                    WHEN RIGHT(value, 1) = '-' THEN '-' + LEFT(value, LEN(value) - 1)
								                    ELSE value
								                END, 
								            '.', ''), 
								        ',', '.') 
								    AS DECIMAL(18, 2))) AS Value
								                
								FROM data 	
								GROUP BY CONVERT(Date, [date], 104), id_costcenter, id_account
								 
							"
		
			#End Region			
			
			#Region "COSTS (Monthly) - ALL Factories"
			
				Else If filePath.Contains("ZSYPI") Then
					
					Dim fullPath As String = FileSystemLocation.ApplicationDatabase.ToString & filePath
					
					' Verificar si el archivo existe
					If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath) And runInsert = "No" Then
					
						Dim contentFileBytes As Byte() = fileInfo.XFFile.ContentFileBytes
						Dim cont As Integer = 0
						Dim filteredLines As New List(Of String)
						
						' Paso 1: Convertir los bytes a texto
						Using ms As New MemoryStream(contentFileBytes)
							Dim reader As New StreamReader(ms, Encoding.UTF8)
					            ' Leer el archivo línea por línea
					            Dim line As String

					            While (reader.Peek() >= 0)
					                line = reader.ReadLine()	
									
									If line.Contains("|") And Not line.Contains("Exerc") And Not line.Contains("Total") Then
										filteredLines.Add(line)
									End If

					            End While

					    End Using
						
						' Paso 4: Unir las líneas filtradas en un solo texto
						Dim newContent As String = String.Join(Environment.NewLine, filteredLines)
						
						' Paso 5: Convertir el nuevo contenido de texto a bytes
						Dim newContentBytes As Byte() = Encoding.UTF8.GetBytes(newContent)
	
						tableName = "XFC_PLT_RAW_CostsMonthly"	
						
						fieldTokens.Add("xfText#:[dummy1]")
						fieldTokens.Add("xfText#:[year]")
						fieldTokens.Add("xfText#:[month]")
						fieldTokens.Add("xfText#:[id_factory]")
						fieldTokens.Add("xfText#:[entity]")
						fieldTokens.Add("xfText#:[domFonc]")
						fieldTokens.Add("xfText#:[code]")
						fieldTokens.Add("xfText#:[rubirc]")
						fieldTokens.Add("xfText#:[activity]")
						fieldTokens.Add("xfText#:[society]")
						fieldTokens.Add("xfText#:[cbl_account]")
						fieldTokens.Add("xfDec#:[value_intern]::0")
						fieldTokens.Add("xfDec#:[value_transact]::0")
						fieldTokens.Add("xfText#:[dev]")
'						fieldTokens.Add("xfText#:[dummy2]")
						 fieldTokens.Add("xfText#:[documentF]")
						fieldTokens.Add("xfText#:[num_cpte]")
						fieldTokens.Add("xfText#:[id_costcenter]")
						fieldTokens.Add("xfText#:[num_ordre]")
						fieldTokens.Add("xfText#:[elm_OTP]")
						
						Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, newContentBytes, "|", dbLocation, tableName, loadMethod, fieldTokens, True)
						
						tableName = String.Empty ' Para que no elimine la tabla auxiliar
						
					Else If runInsert = "Si" Then
					
						sqlMap = " INSERT INTO XFC_PLT_FACT_Costs (scenario, id_factory, date, id_account, id_costcenter, value, currency)
									
									-- CUENTA CONTABLE: Para los del perímetro VT (01, 02, 03, 04, 05)
									-- RUBRICA OS: Para el resto de perímetros
										SELECT *
										FROM (     
											SELECT  
											      'Actual' as scenario,
											      CASE
											        WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
											        WHEN CONCAT('R',id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
											        WHEN CONCAT('R',id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
											        WHEN CONCAT('R',id_factory, entity) = 'R1301003' THEN 'R0548913'
											        WHEN CONCAT('R',id_factory, entity) = 'R1301002' THEN 'R0548914'
											        WHEN CONCAT('R',id_factory, entity) LIKE '%R0585%' THEN 'R0585'
											        WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
											        WHEN CONCAT('R',id_factory, entity) LIKE '%R0592%' THEN 'R0592'
											      ELSE 'TBD' END AS id_factory,
											      DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date,
											      mng_account as id_account,
											      id_costcenter,
											      SUM(value_transact) as value,
											      'Local' as currency
											      FROM (
							
											      		SELECT A.*, COALESCE(COALESCE(B.mng_account, C.mng_account),'Others') as mng_account
							
											      		FROM XFC_PLT_RAW_CostsMonthly A
							
											      		LEFT JOIN XFC_PLT_Mapping_Accounts B
											       			ON A.num_cpte = B.cnt_account
														
														LEFT JOIN XFC_PLT_Mapping_Accounts C
															ON A.rubirc = C.os_account
														
											 
											      WHERE CAST(A.[month] as INT) <= 13
							
											      )  AS ext
											      GROUP BY id_factory, entity, [year], [month],mng_account, id_costcenter
											) AS mapeo	
									"				
					End If
					
			#End Region	
			
			#Region "TEMP - TopFab Costs R0671 M10"
			
				Else If filePath.Contains("Costs_TopFab_R0671_2024_M10.csv") Then					
					
					tableName = "XFC_PLT_AUX_Costs_TopFab"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[variability]")
					fieldTokens.Add("xfDec#:[budget_data]::0")
					fieldTokens.Add("xfDec#:[actual_data]::0")
					
					Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
									
					' Para insertarlo en la tabla de hechos de costes
					sqlMap = $"	DELETE FROM XFC_PLT_FACT_Costs
								WHERE id_factory = 'R0671'
								AND MONTH(date) = 10
								AND YEAR(date) = 2024
								--AND scenario = 'Actual'
								
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_account, id_costcenter, date, value, id_factory)					
								SELECT
									'Actual' AS scenario
									, id_account
									, id_costcenter
									, DATEFROMPARTS(2024, 10, 1) AS date
									, SUM(actual_data) AS value
									, 'R0671' AS id_factory
							
								FROM XFC_PLT_AUX_Costs_TopFab
								GROUP BY id_account, id_costcenter
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_account, id_costcenter, date, value, id_factory)					
								SELECT
									'Budget' AS scenario
									, id_account
									, id_costcenter
									, DATEFROMPARTS(2024, 10, 1) AS date
									, SUM(budget_data) AS value
									, 'R0671' AS id_factory
							
								FROM XFC_PLT_AUX_Costs_TopFab					
								GROUP BY id_account, id_costcenter	
							"					
					
			#End Region		
			
			#Region "Sevilla Budget Costs"
			
				Else If filePath.Contains("BudgetCosts2025_OneShot.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_TopFabBudgetCosts
								(
								[id_account] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					tableName = "XFC_PLT_RAW_TopFabBudgetCosts"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[dummy3]")
					fieldTokens.Add("xfText#:[dummy4]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[dummy5]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")					
					
					' Para insertarlo en la tabla de hechos de costes
					sqlMap = $"	DELETE FROM XFC_PLT_FACT_Costs
								WHERE scenario = 'Budget'							
								AND YEAR(date) = {year}
								AND id_factory = '{factory}'
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_account, id_costcenter, date, value, id_factory)
								SELECT DISTINCT
									'Budget' as scenario,
								    id_account,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									SUM(valor) AS value,
									'{factory}' As factory
								FROM
								    (
								        SELECT id_account, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded	
								
								GROUP BY id_account, id_costcenter, mes
							"		
			#End Region				
			
			#Region "Cube Data Import"		
			
				Else If filePath.Contains("CostsImport.csv") Then
					
					'Create Session Info for another app
					Dim oar As New OpenAppResult
					Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Production", oar)
					
					'Get table name and declare dt
					Dim dt As New DataTable
					
					If oar = OpenAppResult.Success Then
						
						Using mainDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
						
							'Get all the rows from external table
							Dim selectQuery As String = $"								
									WITH aux as (	
										SELECT 
											CASE
												WHEN U3 IS NULL OR U3 = '' THEN '-'
												ELSE U3 
											END as id_costcenter, 
											Sn as scenario,
											CASE 
												WHEN Tm LIKE '%.%' THEN
													CONVERT(DATE, CONCAT(LEFT(Tm, 4), '-', RIGHT('0' + CAST(CAST(PARSENAME(Tm, 1) AS INT) AS VARCHAR), 2), '-01'))
												WHEN Tm LIKE '%M%' THEN
													 CONVERT(DATE, CONCAT(LEFT(Tm, 4), '-', RIGHT('0' + REPLACE(Tm, LEFT(Tm, 5), ''), 2), '-01'))
												ELSE NULL
												END AS date,
											CASE
												WHEN Et LIKE '%R0611%' THEN 'R0045106'
												WHEN Et LIKE '%R1303%' THEN 'R0483003'
												WHEN Et LIKE '%R1302%' THEN 'R0529002'
												WHEN Et = 'R1301003' THEN 'R0548913'
												WHEN Et = 'R1301002' THEN 'R0548914'
												WHEN Et LIKE '%R0585%' THEN 'R0585'
												WHEN Et LIKE '%R0671%' THEN 'R0671'
												WHEN Et LIKE '%R0592%' THEN 'R0592'
											ELSE 'TBD'
											END as id_factory,
											Et,
											Ac as id_account,
											SUM(Am) as Am
									
										FROM StageSourceData
																											
										WHERE Sn = 'Actual'
									
										GROUP BY U3, Sn, Tm, Et, Ac
									),
									
									auxCosts as (
										SELECT 
											scenario, 
											id_factory, 
											date, 
											id_costcenter, 
											id_account, 
											'-' as id_piece, 
											SUM(Am) as value
										
										FROM aux
										
										WHERE 1 = 1
											AND id_factory <> 'TBD'
										
										GROUP BY scenario, id_costcenter, date, id_account, id_factory
									),
							
									auxFXRates as (
										SELECT 
												A.Name, 
												B.DecimalValue,
												CASE
												WHEN A.Name LIKE '%R0611%' THEN 'R0045106'
												WHEN A.Name LIKE '%R1303%' THEN 'R0483003'
												WHEN A.Name LIKE '%R1302%' THEN 'R0529002'
												WHEN A.Name = 'R1301003' THEN 'R0548913'
												WHEN A.Name = 'R1301002' THEN 'R0548914'
												WHEN A.Name LIKE '%R0585%' THEN 'R0585'
												WHEN A.Name LIKE '%R0671%' THEN 'R0671'
												WHEN A.Name LIKE '%R0592%' THEN 'R0592'
											ELSE 'TBD'
											END as id_factory
									
										FROM Member A
									
										INNER JOIN MemberProperty B
											ON A.MemberId = B.MemberId
										
										WHERE 1=1
											AND A.DimId = 6
											AND B.PropertyId = 100
											AND LEN(A.Name) > 5
									
										GROUP BY A.Name, B.DecimalValue
																			
									)
							
									"
							' Filtrar por fábricas
							
							dt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
							
							'Truncate target table
							Dim deleteQuery As String = $"
							TRUNCATE TABLE XFC_PLT_FACT_Costs
							"
							BRApi.Database.ExecuteSql(mainDbConn, deleteQuery, False)
							
							'Generate inserts
							For Each insertQuery As String In GenerateInsertSQLList(dt, "XFC_PLT_FACT_Costs")
								BRApi.Database.ExecuteSql(mainDbConn, insertQuery, False)
							Next
						
						End Using
						End Using
					End If
			#End Region
			
			#Region "TopFab Budget Costs Sin Ajustar"
			
				Else If filePath.Contains("TopFabBudgetCosts_SinAjustar.csv") Then
					
					sqlRaw = "	--DROP TABLE XFC_PLT_RAW_TopFabBudgetCosts
					
								CREATE TABLE XFC_PLT_RAW_TopFabBudgetCosts
								(
								[id_account] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					tableName = "XFC_PLT_RAW_TopFabBudgetCosts"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[dummy3]")
					fieldTokens.Add("xfText#:[dummy4]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[dummy5]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")
					
					sqlMap = $"	DELETE FROM XFC_PLT_FACT_TopFabBudget_Costs
								WHERE id_factory = '{factory}'
								AND YEAR(date) = {year}
					
								INSERT INTO XFC_PLT_FACT_TopFabBudget_Costs (id_account, id_costcenter, date, value, id_factory)
								SELECT
								    id_account,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory
								FROM
								    (
								        SELECT id_account, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded								
							"
					
					' Para insertarlo en la tabla de hechos de costes
					sqlMap = $"	DELETE FROM XFC_PLT_FACT_Costs
								WHERE id_factory = '{factory}'
								AND YEAR(date) = {year}
								AND scenario = 'Budget'
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_account, id_costcenter, date, value, id_factory)
								SELECT DISTINCT
									'Budget' as scenario,
								    id_account,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									SUM(valor) AS value,
									'{factory}' As factory
								FROM
								    (
								        SELECT id_account, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded	
								
								GROUP BY id_account, id_costcenter, mes
							"		
			#End Region						
			
			#Region "TopFab Budget Costs"
			
				Else If filePath.Contains("TopFabBudgetCosts.csv") Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_TopFabBudgetCosts
								(
								[id_account] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[M01] decimal(18,2),
								[M02] decimal(18,2),
								[M03] decimal(18,2),
								[M04] decimal(18,2),
								[M05] decimal(18,2),
								[M06] decimal(18,2),
								[M07] decimal(18,2),
								[M08] decimal(18,2),
								[M09] decimal(18,2),
								[M10] decimal(18,2),
								[M11] decimal(18,2),
								[M12] decimal(18,2)
								)"
													
					tableName = "XFC_PLT_RAW_TopFabBudgetCosts"	
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[dummy3]")
					fieldTokens.Add("xfText#:[dummy4]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[dummy5]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")
					
					sqlMap = $"	INSERT INTO XFC_PLT_FACT_TopFabBudget_Costs (id_account, id_costcenter, date, value, id_factory)
								SELECT
								    id_account,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory
								FROM
								    (
								        SELECT id_account, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded								
							"
					
					' Para insertarlo en la tabla de hechos de costes
					sqlMap = $"	INSERT INTO XFC_PLT_FACT_Costs (scenario, id_account, id_costcenter, date, value, id_factory)
								SELECT DISTINCT
									'Budget' as scenario,
								    id_account,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									SUM(valor) AS value,
									'{factory}' As factory
								FROM
								    (
								        SELECT id_account, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT id_account, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded	
								
								GROUP BY id_account, id_costcenter, mes
							"		
			#End Region	
			
			#Region "Allocation Keys"
			
				Else If filePath.Contains($"CostsAllocationKeys.csv") Then
					
					tableName = "XFC_PLT_RAW_CostsAllocations"	
					
					#Region "OLD Aux"
'					sqlRaw = $"CREATE TABLE {tableName}
'								(
'								[scenario] varchar(50) ,
'								[id_costcenter] varchar(50) ,
'								[id_averagegroup] varchar(50) ,
'								[costnature] varchar(50) ,
'								[M01] decimal(18,4),
'								[M02] decimal(18,4),
'								[M03] decimal(18,4),
'								[M04] decimal(18,4),
'								[M05] decimal(18,4),
'								[M06] decimal(18,4),
'								[M07] decimal(18,4),
'								[M08] decimal(18,4),
'								[M09] decimal(18,4),
'								[M10] decimal(18,4),
'								[M11] decimal(18,4),
'								[M12] decimal(18,4)
'								)"

'					fieldTokens.Add("xfText#:[scenario]")
'					fieldTokens.Add("xfText#:[id_costcenter]")
'					fieldTokens.Add("xfText#:[dummy1]")
'					fieldTokens.Add("xfText#:[dummy2]")
'					fieldTokens.Add("xfText#:[costnature]")
'					fieldTokens.Add("xfText#:[dummy4]")
'					fieldTokens.Add("xfText#:[id_averagegroup]")
'					fieldTokens.Add("xfText#:[dummy5]")
'					fieldTokens.Add("xfDec#:[M01]::0")
'					fieldTokens.Add("xfDec#:[M02]::0")
'					fieldTokens.Add("xfDec#:[M03]::0")
'					fieldTokens.Add("xfDec#:[M04]::0")
'					fieldTokens.Add("xfDec#:[M05]::0")
'					fieldTokens.Add("xfDec#:[M06]::0")
'					fieldTokens.Add("xfDec#:[M07]::0")
'					fieldTokens.Add("xfDec#:[M08]::0")
'					fieldTokens.Add("xfDec#:[M09]::0")
'					fieldTokens.Add("xfDec#:[M10]::0")
'					fieldTokens.Add("xfDec#:[M11]::0")
'					fieldTokens.Add("xfDec#:[M12]::0")					
					#End Region	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
								[id_factory] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[costnature] varchar(50) ,
								[M01] decimal(18,9),
								[M02] decimal(18,9),
								[M03] decimal(18,9),
								[M04] decimal(18,9),
								[M05] decimal(18,9),
								[M06] decimal(18,9),
								[M07] decimal(18,9),
								[M08] decimal(18,9),
								[M09] decimal(18,9),
								[M10] decimal(18,9),
								[M11] decimal(18,9),
								[M12] decimal(18,9)
								)"

					fieldTokens.Add("xfText#:[scenario]")
					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_costcenter]")	
					fieldTokens.Add("xfText#:[id_averagegroup]")
					fieldTokens.Add("xfText#:[costnature]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")	
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_AllocationKeys
								WHERE 1=1
									AND id_factory IN ('R0592','R0573003')
									AND YEAR(date) = 2025
									AND scenario = 'Budget_V2'
					
								-- 2- Insert statement
													
								INSERT INTO XFC_PLT_AUX_AllocationKeys (scenario, id_costcenter, id_averagegroup, costnature, date, percentage, id_factory)
								SELECT DISTINCT
								    scenario
								    , id_costcenter
								    , id_averagegroup
								    , costnature
								    , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor/100 AS percentage
									, id_factory As factory
								FROM
								    (
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M01' AS mes, M01 AS valor FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M02', M02 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M03', M03 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M04', M04 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M05', M05 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M06', M06 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M07', M07 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M08', M08 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M09', M09 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M10', M10 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M11', M11 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT DISTINCT scenario, id_factory, id_costcenter, id_averagegroup, costnature, 'M12', M12 FROM XFC_PLT_RAW_CostsAllocations
								    ) AS expanded	
												
								WHERE 1=1
									AND valor <> 0
									--AND costnature = 1
									--AND id_costcenter = 'HN05014'
									--AND id_averagegroup = 'GM05012'
							"
					
							BRApi.ErrorLog.LogMessage(si, "sqlMap: " & sqlMap)
		
			#End Region
			
			#Region "Fixed/Variable - RF02"
			
				Else If filePath.Contains("CostsFixedVariableRF02.csv") Then

					tableName = "XFC_PLT_RAW_CostsFixedVariable"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_factory] varchar(50) ,
								[id_account] varchar(50) ,
								[M01] decimal(18,9),
								[M02] decimal(18,9),
								[M03] decimal(18,9),
								[M04] decimal(18,9),
								[M05] decimal(18,9),
								[M06] decimal(18,9),
								[M07] decimal(18,9),
								[M08] decimal(18,9),
								[M09] decimal(18,9),
								[M10] decimal(18,9),
								[M11] decimal(18,9),
								[M12] decimal(18,9)
								)"

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")	
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_FixedVariableCosts
								WHERE 1=1
									AND scenario = 'Budget_V4'
					
								-- 2- Insert statement
													
								INSERT INTO XFC_PLT_AUX_FixedVariableCosts (scenario, id_factory, date, id_account, costnature,value)
								SELECT
								   'Budget_V4' as scenario
									, id_factory
									, DATEFROMPARTS(2025, mes, 1) as date
									, id_account
									, '-1' as costnature
									, value
								    
								FROM
								    (
								        SELECT id_factory, id_account, 1 AS mes, M01 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 2 AS mes, M02 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 3 AS mes, M03 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 4 AS mes, M04 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 5 AS mes, M05 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 6 AS mes, M06 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 7 AS mes, M07 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 8 AS mes, M08 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 9 AS mes, M09 AS  value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 10 AS mes, M10 AS value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 11 AS mes, M11 AS value	FROM {tableName} UNION ALL
								        SELECT id_factory, id_account, 12 AS mes, M12 AS value	FROM {tableName}
								    ) AS expanded	
												
								WHERE 1=1
									AND value <> 0
							"
					
			#End Region
			
			#Region "Variance Analysis  - ACT vs BUD 4"
			
				Else If filePath.Contains("EfectosACT_BUD4.csv") Then

					tableName = "XFC_PLT_RAW_Effects_BudgetV4"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ref_scenario] varchar(50) ,
								[id_factory] varchar(50) ,
								[month] varchar(50) ,
								[cost_type] varchar(50) ,
								[id_indicator] varchar(50) ,
								[currency] varchar(50) ,
								[variability] varchar(50) ,
								[value] decimal(18,9)
								)"

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[month]")
					fieldTokens.Add("xfText#:[cost_type]")
					fieldTokens.Add("xfText#:[id_indicator]")
					fieldTokens.Add("xfText#:[currency]")
					fieldTokens.Add("xfText#:[variability]")
					fieldTokens.Add("xfDec#:[value]::0")
					fieldTokens.Add("xfText#:[ref_scenario]")
					fieldTokens.Add("xfText#:[dummy]")
					BRAPI.ErrorLog.LogMessage(si, "EFECT")
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_EffectsAnalysis
								WHERE 1=1
									AND (ref_scenario = 'Budget_V4' AND scenario = 'Actual') 
					
								-- 2- Insert statement
													
								INSERT INTO XFC_PLT_AUX_EffectsAnalysis (scenario, ref_scenario, id_factory, date, cost_type, id_indicator, variability, value)
								SELECT
								   'Actual' as scenario
									, ref_scenario
									, id_factory
									, DATEFROMPARTS(2025, CAST(month AS INT), 1) as date
									, cost_type
									, id_indicator
									, variability
									, value
								    
								FROM {tableName}
												
								WHERE 1=1
									AND value <> 0
							"
					
			#End Region
			
			#Region "Variance Analysis  - UNE and ELB"
			
				Else If filePath.Contains("UNE_ELB.csv") Then

					tableName = "XFC_PLT_RAW_Effects_UNAELB"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ref_scenario] varchar(50) ,
								[scenario] varchar(50) ,
								[id_factory] varchar(50) ,
								[month] varchar(50) ,
								[cost_type] varchar(50) ,
								[id_indicator] varchar(50) ,
								[currency] varchar(50) ,
								[variability] varchar(50) ,
								[value] decimal(18,9)
								)"

					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[month]")
					fieldTokens.Add("xfText#:[cost_type]")
					fieldTokens.Add("xfText#:[id_indicator]")
					fieldTokens.Add("xfText#:[currency]")
					fieldTokens.Add("xfText#:[variability]")
					fieldTokens.Add("xfDec#:[value]::0")
					fieldTokens.Add("xfText#:[ref_scenario]")
					fieldTokens.Add("xfText#:[scenario]")

					sqlMap = $"	
								-- 1- Previous clear
					
								-- DELETE FROM XFC_PLT_AUX_EffectsAnalysis
								-- WHERE 1=1
								-- 	AND (
								-- 			(ref_scenario = 'Budget_V2' AND scenario = 'Actual') 
								-- 			OR 
								-- 			(ref_scenario = 'Budget_V2' AND scenario = 'Budget_V4') 
								-- 		)
								-- 	AND id_indicartor IN('UNE?', 'ELB?')
					
								-- 2- Insert statement
													
								INSERT INTO XFC_PLT_AUX_EffectsAnalysis (scenario, ref_scenario, id_factory, date, cost_type, id_indicator, variability, value)
								SELECT
									scenario
									, ref_scenario
									, id_factory
									, DATEFROMPARTS(2025, CAST(month AS INT), 1) as date
									, cost_type
									, id_indicator
									, variability
									, value
								    
								FROM {tableName}
												
								WHERE 1=1
									AND value <> 0
							"
					
			#End Region
			
			#Region "Variance Analysis  - RF02"
			
				Else If filePath.Contains("CostsEffectsRF02.csv") Then

					tableName = "XFC_PLT_RAW_Effects_BudgetV4"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[ref_scenario] varchar(50) ,
								[id_factory] varchar(50) ,
								[month] varchar(50) ,
								[cost_type] varchar(50) ,
								[id_indicator] varchar(50) ,
								[currency] varchar(50) ,
								[variability] varchar(50) ,
								[value] decimal(18,9)
								)"

					fieldTokens.Add("xfText#:[ref_scenario]")
					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[month]")
					fieldTokens.Add("xfText#:[cost_type]")
					fieldTokens.Add("xfText#:[id_indicator]")
					fieldTokens.Add("xfText#:[currency]")
					fieldTokens.Add("xfText#:[variability]")
					fieldTokens.Add("xfDec#:[value]::0")
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_EffectsAnalysis
								WHERE 1=1
									AND scenario = 'Budget_V4'
					
								-- 2- Insert statement
													
								INSERT INTO XFC_PLT_AUX_EffectsAnalysis (scenario, ref_scenario, id_factory, cost_type, id_indicator, currency, variability, value)
								SELECT
								   'Budget_V4' as scenario
									, ref_scenario
									, id_factory
									, DATEFROMPARTS(2025, CAST(month AS INT), 1) as date
									, cost_type
									, id_indicator
									, currency
									, variability
									, value
								    
								FROM {tableName}
												
								WHERE 1=1
									AND value <> 0
							"
					
			#End Region
			
			#Region "COSTS - Energy Variance"
			
				Else If filePath.Contains($"EnergyVariance.csv") Then
					
					tableName = "XFC_PLT_RAW_EnergyVariance"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[energy_type] varchar(50) ,
								[indicator] varchar(50) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
					
					fieldTokens.Add("xfText#:[energy_type]")
					fieldTokens.Add("xfText#:[indicator]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[dummy3]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_EnergyVariance
								WHERE 1=1
									AND scenario = '{scenario}'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
					
								-- 2- Insert statement
																					
								INSERT INTO XFC_PLT_AUX_EnergyVariance (scenario, energy_type, indicator, date, value, id_factory)
								SELECT
								    '{scenario}'
									, energy_type
									, indicator
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
								FROM
								    (
										SELECT energy_type, indicator, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M02', M02 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M03', M03 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M04', M04 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M05', M05 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M06', M06 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M07', M07 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M08', M08 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M09', M09 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M10', M10 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M11', M11 FROM {tableName} UNION ALL
										SELECT energy_type, indicator, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1
							"

			#End Region
			
			#Region "COSTS - StartUp y RampUp Costs"
			
				Else If filePath.Contains($"ProjectCosts.csv")Then
					
					tableName = "XFC_PLT_RAW_ProjectCosts"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_factory] varchar(50) ,
								[project_file] varchar(50) ,
								[ProjectCode] varchar(50) ,
								[CostType] varchar(50) ,
								[M01] decimal(18,4),
								[M02] decimal(18,4),
								[M03] decimal(18,4),
								[M04] decimal(18,4),
								[M05] decimal(18,4),
								[M06] decimal(18,4),
								[M07] decimal(18,4),
								[M08] decimal(18,4),
								[M09] decimal(18,4),
								[M10] decimal(18,4),
								[M11] decimal(18,4),
								[M12] decimal(18,4)
								)"
	
					fieldTokens.Add("xfText#:[project_file]")
					fieldTokens.Add("xfText#:[id_factory]")
					fieldTokens.Add("xfText#:[ProjectCode]")
					fieldTokens.Add("xfText#:[CostType]")
					fieldTokens.Add("xfDec#:[M01]::0")
					fieldTokens.Add("xfDec#:[M02]::0")
					fieldTokens.Add("xfDec#:[M03]::0")
					fieldTokens.Add("xfDec#:[M04]::0")
					fieldTokens.Add("xfDec#:[M05]::0")
					fieldTokens.Add("xfDec#:[M06]::0")
					fieldTokens.Add("xfDec#:[M07]::0")
					fieldTokens.Add("xfDec#:[M08]::0")
					fieldTokens.Add("xfDec#:[M09]::0")
					fieldTokens.Add("xfDec#:[M10]::0")
					fieldTokens.Add("xfDec#:[M11]::0")
					fieldTokens.Add("xfDec#:[M12]::0")					
					
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_Projects
								WHERE 1=1
									AND scenario = 'Budget'
									AND YEAR(date) = {year}

						
					
								-- 2- Insert statement
																					
								INSERT INTO XFC_PLT_AUX_Projects (scenario, id_project_code, offer, responsibility, cost_type, project_task, project_task_text, currency, date, value, id_factory, project_file)
								SELECT
								    'Budget' as scenario
									, ProjectCode
									, '-1' as Offer
									, '-1' as Resonsibility
									, CostType
									, '-1' as ProjectTask
									, '-1' as ProjecttaskText
									, '-1' as Currency
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, id_factory
									, project_file
								FROM
								    (
										SELECT id_factory, project_file, ProjectCode, CostType, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M02', M02 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M03', M03 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M04', M04 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M05', M05 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M06', M06 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M07', M07 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M08', M08 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M09', M09 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M10', M10 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M11', M11 FROM {tableName} UNION ALL
										SELECT id_factory, project_file, ProjectCode, CostType, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1
					
							"
			#End Region	
		
		#End Region
		
		#Region "MIGRATION"
			
			#Region "MIGRATION - Cost Distribution"
		
				Else If filePath.Contains("XFC_PLT_FACT_CostsDistribution_Data_CSV") Then

					tableName = "XFC_PLT_FACT_CostsDistribution"	

					fieldTokens.Add("xfText#:[scenario]")
					fieldTokens.Add("xfDate#:[date]")
					fieldTokens.Add("xfText#:[id_factory]")		
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[id_costcenter]")	
					fieldTokens.Add("xfText#:[id_averagegroup]")	
					fieldTokens.Add("xfText#:[id_product]")	
					fieldTokens.Add("xfDec#:[value]::0")	
					fieldTokens.Add("xfText#:[type_tso_taj]")											

			#End Region	
							
		#End Region			
			
		#Region "METADATA"
			
			#Region "Master"
			
				#Region "CostCenterHist"
				
				Else If filePath.Contains("XFC_PLT_MASTER_CostCenter_Hist.csv") Then
											
						tableName = "XFC_PLT_MASTER_CostCenter_Hist"
						
						fieldTokens.Add("xfText#:[id_factory]")						
						fieldTokens.Add("xfText#:[id]")
						fieldTokens.Add("xfText#:[description]")	
						fieldTokens.Add("xfText#:[scenario]")							
						fieldTokens.Add("xfText#:[start_date]")							
						fieldTokens.Add("xfText#:[end_date]")							
						fieldTokens.Add("xfText#:[type]")
						fieldTokens.Add("xfText#:[nature]")
						fieldTokens.Add("xfText#:[id_averagegroup]")
						fieldTokens.Add("xfText#:[function]")
						fieldTokens.Add("xfText#:[id_parent]")						
						
						BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
						
						Return loadResults
						
				#End Region			
			
				#Region "CostCenter"
				Else If filePath.Equals("XFC_PLT_MASTER_CostCenter") Then
					
					If filePath.Contains("Budget") Then
						tableName = "XFC_PLT_MASTER_CostCenterBudget"
						
						fieldTokens.Add("xfText#:[id]")
						fieldTokens.Add("xfText#:[parent_id]")
						fieldTokens.Add("xfText#:[type]")			
						
						BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
						Return loadResults
						
					Else
						
						tableName = "XFC_PLT_MASTER_CostCenter"
						
						fieldTokens.Add("xfText#:[id]")
						fieldTokens.Add("xfText#:[parent_id]")
						fieldTokens.Add("xfText#:[type]")
						fieldTokens.Add("xfText#:[description]")
						fieldTokens.Add("xfText#:[nature]")
						fieldTokens.Add("xfText#:[averagegroup]")
						fieldTokens.Add("xfText#:[factory]")
						
						BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
						Return loadResults
						
					End If
				#End Region
				
				#Region "Average Group"
				Else If filePath.Contains("AverageGroupDescription.csv") Then
											
						tableName = "XFC_PLT_MASTER_AverageGroup"
						
						fieldTokens.Add("xfText#:[id_factory]")						
						fieldTokens.Add("xfText#:[id]")
						fieldTokens.Add("xfText#:[description]")	
						fieldTokens.Add("xfText#:[id_function]")							
						fieldTokens.Add("xfText#:[function_description]")					
						
						BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
						Return loadResults
						
				#End Region					
				
				#Region "Accounts"
				
				Else If filePath.Contains("XFC_PLT_MAPPING_Accounts_Abr.csv") Then
					
					tableName = "XFC_PLT_MAPPING_Accounts_Abr"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[mont_year] varchar(50) ,
								[cnt_account] varchar(50) ,
								[costcenter_type] varchar(50),
								[costcenter_nature] varchar(50),
								[docF] varchar(50),
								[os_account] varchar(50),
								[mng_account] varchar(50)								
								)"

					fieldTokens.Add("xfText#:[mont_year]")
					fieldTokens.Add("xfText#:[cnt_account]")
					fieldTokens.Add("xfText#:[costcenter_type]")	
					fieldTokens.Add("xfText#:[costcenter_nature]")	
					fieldTokens.Add("xfText#:[docF]")	
					fieldTokens.Add("xfText#:[os_account]")	
					fieldTokens.Add("xfText#:[mng_account]")	
					
					sqlMap = $"	
								-- 1- Previous clear
					
								TRUNCATE TABLE XFC_PLT_MAPPING_Accounts_DocF;
					
								-- 2- Insert statement
								WITH datos_con_fecha AS (
 
									SELECT 
       									 *
       									 , CAST(CONCAT(SUBSTRING([mont_year], 4, 4), '-', SUBSTRING([mont_year], 1, 2), '-01') AS DATE) AS fecha_real
    								
									FROM XFC_PLT_MAPPING_Accounts_Abr
					
									),
								
								ultimos_movimientos AS (
									   
										SELECT *
									    
										FROM (
									        	SELECT 
									           		*
									            	, ROW_NUMBER() OVER (
									                	PARTITION BY 
									                	    cnt_account, 
									                	    costcenter_type, 
									                	    costcenter_nature, 
									                	    docF
									                	ORDER BY fecha_real DESC
									            	) AS rn
									       		
											FROM datos_con_fecha
									    ) sub
						
									    WHERE rn = 1
								)
					
							INSERT INTO XFC_PLT_MAPPING_Accounts_DocF (cnt_account, costcenter_type, costcenter_nature, docF, os_account, mng_account)
							
							SELECT 
								cnt_account
								, costcenter_type
								, costcenter_nature
								, docF
								, os_account
								, mng_account
					
							FROM ultimos_movimientos
								
							"
				Else If filePath.Contains("XFC_PLT_MAPPING_Accounts.csv") Then
					
						tableName = "XFC_PLT_MAPPING_Accounts"
						
						fieldTokens.Add("xfText#:[cnt_account]::-1")
						fieldTokens.Add("xfText#:[costcenter_type]")
						fieldTokens.Add("xfText#:[costcenter_nature]")
						fieldTokens.Add("xfText#:[mng_account]::-1")
						fieldTokens.Add("xfText#:[os_account]::-1")
						
						BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
						Return loadResults
						
				#End Region
				
				#Region "Product"
				
					#Region "Products IDs - System Import"
'				Else If filePath.Contains("XFC_PLT_MASTER_Product") Then
					
'					'Create Session Info for another app
'					Dim oar As New OpenAppResult
'					Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Dev", oar)
					
'					'Get table name and declare dt
'					Dim dt As New DataTable
					
'					If oar = OpenAppResult.Success Then
						
'						Using mainDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'						Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
						
'							'Get all the rows from external table
'							Dim selectQuery As String = $"
'									SELECT A.A1 as id, '' as description
									
'									FROM StageTargetData B
'									LEFT JOIN StageAttributeData A
'										ON B.Wfk = A.Wfk
'										AND B.Wtk = A.Wtk
'										AND B.Ri = A.Ri						
																		
'									WHERE 1=1
'										AND A.A1 <> ''
'									GROUP BY A.A1,
									
'									"
							
'							selectQuery = "
'							SELECT id, description
							
'							FROM XFC_PLT_HIER_Product
							
'							GROUP BY id, description
'							"
'							dt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
							
'							'Truncate target table
'							Dim deleteQuery As String = $"
'							TRUNCATE TABLE XFC_PLT_MASTER_Product
'							"
'							BRApi.Database.ExecuteSql(mainDbConn, deleteQuery, False)
							
'							'Generate inserts
'							For Each insertQuery As String In GenerateInsertSQLList(dt, "XFC_PLT_MASTER_Product")
'								BRApi.Database.ExecuteSql(mainDbConn, insertQuery, False)
'							Next
						
'						End Using
'						End Using
										
'					End If		
						#End Region
						
					#Region "Product Description - Production File"
				Else If filePath.Contains("XFC_PLT_MASTER_Prod_Desc") Then					
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_Prod_Desc
							 	(
								[id_product] varchar(50) ,
								[desc_product] varchar(500)
								)"

					tableName = "XFC_PLT_RAW_Prod_Desc"						

					fieldTokens.Add("xfText#:[id_product]")
					fieldTokens.Add("xfText#:[desc_product]")
					
					sqlMap = "	
					
								INSERT INTO XFC_PLT_MASTER_Product (id, description)
								SELECT DISTINCT id_product, desc_product 
								FROM XFC_PLT_RAW_Prod_Desc D
								LEFT JOIN XFC_PLT_MASTER_Product P
									ON D.id_product = P.id
								WHERE P.id IS NULL
					
								UPDATE P
						
									SET P.Description = R.desc_product
					
								FROM XFC_PLT_MASTER_Product P
								INNER JOIN XFC_PLT_RAW_Prod_Desc R
									ON P.id = R.id_product
								"
					#End Region
					
				#End Region
				
				#Region "Effects"
				Else If filePath.Contains("XFC_PLT_MASTER_Effects") Then
					
						tableName = "XFC_PLT_MASTER_Effects"
						
						fieldTokens.Add("xfText#:[id]")
						fieldTokens.Add("xfText#:[description]")
						
						BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
						Return loadResults							
				#End Region
				
			#End Region
			
			#Region "Hierarchy"
				
				#Region "CostsCenter"
				Else If filePath.Contains("XFC_PLT_HIER_CostCenter") Then
					tableName = "XFC_PLT_HIER_CostCenter"
						brapi.ErrorLog.LogMessage(si, "3")
					
					fieldTokens.Add("xfText#:[hierarchy]")
					fieldTokens.Add("xfText#:[id]")
					fieldTokens.Add("xfText#:[parent_id]")
					fieldTokens.Add("xfText#:[description]")
					fieldTokens.Add("xfInt#:[level]")			
					fieldTokens.Add("xfText#:[levelname]")			
					
					BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
					Return loadResults
				#End Region

				#Region "Product Hierarchy (Get Hierarchy from dimension)"
						
				Else If filePath.Contains("XFC_PLT_HIER_Product") Then
					
					'Create Session Info for another app
					Dim oar As New OpenAppResult
					Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Dev_Production", oar)
					
					'Get table name and declare dt
					Dim dt As New DataTable
					
					If oar = OpenAppResult.Success Then
						
						Using mainDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
						
							'Get all the rows from external table
							Dim selectQuery As String = $"
									SELECT 'Product' as hierarchy, A.A1 as id, B.U2T as parent_id, '' as description
									
									FROM StageTargetData B
									LEFT JOIN StageAttributeData A
										ON B.Wfk = A.Wfk
										AND B.Wtk = A.Wtk
										AND B.Ri = A.Ri						
																		
									WHERE 1=1
										AND A.A1 <> ''
									GROUP BY A.A1, B.U2T
									
									"
							dt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
							
							'Truncate target table
							Dim deleteQuery As String = $"
							TRUNCATE TABLE XFC_PLT_HIER_Product
							"
							BRApi.Database.ExecuteSql(mainDbConn, deleteQuery, False)
							
							'Generate inserts
							For Each insertQuery As String In GenerateInsertSQLList(dt, "XFC_PLT_HIER_Product")
								BRApi.Database.ExecuteSql(mainDbConn, insertQuery, False)
							Next
						
						End Using
						End Using
					End If
				
				#End Region					
				
			#End Region
			
			#Region "Historic"
			
				Else If filePath.Contains("XFC_PLT_HIST_CostCenter") Then
					tableName = "XFC_PLT_HIST_CostCenter"
					
					fieldTokens.Add("xfText#:[id]")
					fieldTokens.Add("xfText#:[parent_id]")
					fieldTokens.Add("xfDateTime#:[startdate]")
					fieldTokens.Add("xfDateTime#:[enddate]")			
					
					BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
					Return loadResults

			#End Region	
					
		#End Region
		
				End If
									
				If sqlRaw <> String.Empty Then 
					
					' 1. Creación de tabla temporal RAW
					ExecuteSql(si, "DROP TABLE IF EXISTS " & tableName)
					ExecuteSql(si, sqlRaw) 
				
					' 2. Carga de datos en tabla temporal RAW					
					Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
				
				End If
				
				' 3. Insertar los datos mapeados en la tabla FINAL
				If sqlMap <> String.Empty Then ExecuteSql(si, sqlMap)					
				
'				 4. Eliminar la tabla temporal RAW
				 If sqlRaw <> String.Empty Then ExecuteSql(si, "DROP TABLE " & tableName)											
							
				Return loadResults
		
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function			
		
        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String) 
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                                   
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using  
			
        End Sub
		
		Private Function GenerateInsertSQLList(ByVal dataTable As DataTable, ByVal tableName As String) As List(Of String)
		
		    Dim insertStatements As New List(Of String)()
		    Dim sb As New StringBuilder()
		
		    ' Validate inputs
		    If dataTable Is Nothing OrElse dataTable.Rows.Count = 0 Then
		        Throw New ArgumentException("The DataTable is empty or null.")
		    End If
		
		    If String.IsNullOrEmpty(tableName) Then
		        Throw New ArgumentException("The table name is null or empty.")
		    End If
		
		    ' Get the column names
		    Dim columnNames As String = String.Join(", ", dataTable.Columns.Cast(Of DataColumn)().[Select](Function(c) c.ColumnName))
		
		    ' Batch size limit
		    Dim batchSize As Integer = 1000
		    Dim currentBatchSize As Integer = 0
		
		    ' Build the initial part of the INSERT INTO statement
		    sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		
		    ' Iterate through each row in the DataTable
		    For i As Integer = 0 To dataTable.Rows.Count - 1
		        Dim rowValues As New List(Of String)()
		
		        For Each column As DataColumn In dataTable.Columns
		            Dim value As Object = dataTable.Rows(i)(column)
		
		            If value Is DBNull.Value Then
		                rowValues.Add("NULL")
		            ElseIf TypeOf value Is String OrElse TypeOf value Is Char Then
		                rowValues.Add($"'{value.ToString().Replace("'", "''").Replace(",", ".")}'") ' Escape single quotes
		            ElseIf TypeOf value Is DateTime Then
		                rowValues.Add($"'{CType(value, DateTime).ToString("yyyy-MM-dd HH:mm:ss.fff")}'")
		            Else
		                rowValues.Add(value.ToString().Replace(",", ".").Replace("'","''"))
		            End If
		        Next
		
		        ' Combine row values into a comma-separated string and add to StringBuilder
		        sb.AppendLine($"({String.Join(", ", rowValues)}){If(currentBatchSize < batchSize - 1 AndAlso i < dataTable.Rows.Count - 1, ",", ";")}")
		        currentBatchSize += 1
		
		        ' If the batch size limit is reached, or it's the last row, finalize the current statement and start a new one
		        If currentBatchSize = batchSize OrElse i = dataTable.Rows.Count - 1 Then
		            insertStatements.Add(sb.ToString())
		            sb.Clear()
		            If i < dataTable.Rows.Count - 1 Then
		                sb.AppendLine($"INSERT INTO {tableName} ({columnNames}) VALUES")
		            End If
		            currentBatchSize = 0
		        End If
		    Next
		
		    Return insertStatements
			
		End Function
				
		
		#End Region
		
	End Class
	
End Namespace