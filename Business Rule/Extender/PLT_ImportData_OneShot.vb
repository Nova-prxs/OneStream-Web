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

Namespace OneStream.BusinessRule.Extender.PLT_ImportData_OneShot
	Public Class MainClass
		
		#Region "Main"

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try				
				
				Dim configSettings As AppServerConfigSettings = AppServerConfig.GetSettings(si)
				' ----------------------------------------------------------------------------------
				' Todo esto se convertirá en un bucle que recorra todos los ficheros
				'
				'Process the uploaded file
'					Dim filePath As String = args.NameValuePairs.XFGetValue("FilePath", Guid.Empty.ToString)
				' ----------------------------------------------------------------------------------
				
				' VARIABLES
				Dim scenario As String = args.NameValuePairs.GetValueOrDefault("Scenario", "NA")
				Dim factory As String = args.NameValuePairs.GetValueOrDefault("Factory", "NA")
				Dim year As String = args.NameValuePairs.GetValueOrDefault("Year", "NA")
				Dim month As String = args.NameValuePairs.GetValueOrDefault("Month", "NA")
				
				Dim fileName As String = args.NameValuePairs.GetValueOrDefault("FileName", String.Empty)
				
				' FOLDER PATH
				Dim folderPath As String = $"/Documents/Public/Plants/Import/{factory}"
				
				folderPath = $"/Documents/Public/Plants/Import" 
				fileName = "Nomenclature_Data.csv"
				
				If fileName = String.Empty Then
					' Recorriendo los fiecheros de las carpetas				
					Dim fileExtensions As New List(Of String) From {"csv", "txt", ""}
					Dim filesFolder As List(Of XFFileInfoEx) = BRApi.FileSystem.GetFilesInFolder(si, FileSystemLocation.ApplicationDatabase, folderPath, XFFileType.All, fileExtensions)
	
					For Each files In filesFolder
	
						fileName = files.XFFileInfo.Name	
						' WriteFileToTable(si, folderPath, fileName)	
						brapi.ErrorLog.LogMessage(si, "OneShot: " & files.XFFileInfo.Name)
						
					Next		
				Else
					Brapi.ErrorLog.LogMessage(si, folderPath &" "& fileName)
					WriteFileToTable(si, folderPath, fileName,,,year,factory,)
					brapi.ErrorLog.LogMessage(si, "Fichero Cargado: " & fileName)
				End If
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

		#End Region

		#Region "ETL"		
		
		Public Function WriteFileToTable(
										ByVal si As SessionInfo, _ 
										ByVal folderPath As String, _
										ByVal fileName As String, _
										Optional runInsert As String = "No", _
										Optional month As String = "NA", _
										Optional year As String = "NA", _
										Optional factory As String = "NA", _
										Optional scenario As String = "NA"										
										) As List(Of TableRangeContent)		
		
			Try
				
				Dim loadResults As New List(Of TableRangeContent)
				Dim filePath As String = $"{folderPath}/{fileName}"
				
				'Get the upload file from the database file system
				Dim fileInfo As XFFileEx = BRApi.FileSystem.GetFile(si, FileSystemLocation.ApplicationDatabase,filePath, True, False, False, SharedConstants.Unknown, Nothing, True)
					
				'Load Delimited File properties
				Dim dbLocation As String = "Application"
				Dim loadMethod As String = "Replace"	 ' Use an expression for partial replace -->  Merge:[(ProductName = 'SomeProduct')] 
				Dim fieldTokens As New List(Of String)
				
				'Origin and destination tables
				Dim tableName As String = String.Empty
				Dim tableDest As String = String.Empty
				
				'Queries to be used
				Dim sqlRaw As String = String.Empty
				Dim deleteQuery As String = String.Empty		
				Dim sqlMap As String = String.Empty		

	#Region "Uploads and Mappings"
	
		#Region "PRODUCTION"
		
			#Region "PRODUCTION - Actual"
		
				If fileName = "ProductionActualData.csv" Then
					
					tableName = "XFC_PLT_RAW_Production"
					tableDest = "XFC_PLT_FACT_Production"
					
					sqlRaw = $"CREATE TABLE {tableName} 
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

					fieldTokens.Add("xfText#:[id_factory]::{factory}")
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

					#Region "Mapping"					
					
					sqlMap = $"	
								DELETE FROM {tableDest}
								WHERE 1=1
									AND scenario = 'Actual'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
								
								INSERT INTO {tableDest} (id_factory, date, scenario, id_averagegroup, id_workcenter, id_costcenter, id_product, uotype, volume, allocation_taj, activity_taj)
					
								SELECT 
									id_factory
									, CONVERT(DATE, date, 112)
									, 'Actual' as scenario, id_averagegroup
									, '-1' as id_workcenter
									, id_costcenter
									, id_product
									, CASE 
							      		WHEN uotype = 'UOE1' THEN 'A10'
							     		WHEN uotype = 'UOE2' THEN 'A20'
							     		ELSE 'TBD' 
									END as uotype
									, SUM(volume) as volume 
									, CASE WHEN SUM(volume) = 0 THEN 0 ELSE SUM(volume * allocation_taj) / SUM(volume) END as allocation_taj 
									, SUM(activity_taj) as activity_taj
					
								FROM {tableName} 
					
								GROUP BY id_factory, CONVERT(DATE, date, 112), id_costcenter, id_averagegroup, id_product, uotype
							"
					#End Region
					
			#End Region		

			#Region "PRODUCTION - Budget"					
				
				#Region "Allocations"
				
				Else If fileName = "BudgetAllocation.csv" Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_ProductionBudgetTopFabAlloc 
								(
								[id_averagegroup] varchar(50) ,
								[id_product_family] varchar(50) ,
								[indicator] varchar(50) ,
								[M01] decimal(18,7),
								[M02] decimal(18,7),
								[M03] decimal(18,7),
								[M04] decimal(18,7),
								[M05] decimal(18,7),
								[M06] decimal(18,7),
								[M07] decimal(18,7),
								[M08] decimal(18,7),
								[M09] decimal(18,7),
								[M10] decimal(18,7),
								[M11] decimal(18,7),
								[M12] decimal(18,7)
								)"
					
					tableName = "XFC_PLT_RAW_ProductionBudgetTopFabAlloc"

					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[id_averagegroup]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[id_product_family]")
					fieldTokens.Add("xfText#:[dummy3]")
					fieldTokens.Add("xfText#:[dummy4]")
					fieldTokens.Add("xfText#:[indicator]")
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
					DELETE FROM XFC_PLT_AUX_Production_TopFabBudget_Allocations
					WHERE 1=1
						AND id_factory = '{factory}'
						AND YEAR(date) = {year}
					
					INSERT INTO XFC_PLT_AUX_Production_TopFabBudget_Allocations (id_averagegroup, id_product_family, date, uotype, allocation_taj, allocation_tso, id_factory)
					SELECT 
					    id_averagegroup,
					    id_product_family,
					    date,
					    UOType,
					    SUM(allocation_TAJ) AS allocation_TAJ, -- Suma los valores de allocation_TAJ
					    SUM(allocation_TSO) AS allocation_TSO,  -- Suma los valores de allocation_TSO
						'{factory}' as id_factory
					FROM (
					    SELECT 
					        id_averagegroup,
					        id_product_family,
					        DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
					        SUBSTRING(indicator, 5, 4) AS UOType, -- Extrae UOE1 o UOE2 del indicador
					        CASE 
					            WHEN indicator LIKE 'TAJ%' THEN valor
					            ELSE 0 -- Asigna 0 si el indicador no pertenece a TAJ
					        END AS allocation_TAJ,
					        CASE 
					            WHEN indicator LIKE 'TSO%' THEN valor
					            ELSE 0 -- Asigna 0 si el indicador no pertenece a TSO
					        END AS allocation_TSO
					    FROM (
					        SELECT id_averagegroup, id_product_family, indicator, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M02', M02 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M03', M03 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M04', M04 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M05', M05 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M06', M06 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M07', M07 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M08', M08 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M09', M09 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M10', M10 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M11', M11 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, indicator, 'M12', M12 FROM {tableName}
					    ) AS expanded
					    WHERE valor IS NOT NULL -- Evita valores nulos
					) AS grouped_data
					GROUP BY id_averagegroup, id_product_family, date, UOType
					"
					
				#End Region
				
				#Region "Volumes"
				
				Else If fileName = "BudgetVolumes.csv" Then
					
					sqlRaw = "CREATE TABLE XFC_PLT_RAW_ProductionBudgetTopFabVolume
								(
								[id_averagegroup] varchar(50) ,
								[id_product_family] varchar(50) ,
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
					tableName = "XFC_PLT_RAW_ProductionBudgetTopFabVolume"
					
					'Brapi.ErrorLog.LogMessage(si, fileInfo.XFFile.FileInfo.Name)

					fieldTokens.Add("xfText#:[id_product_family]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[id_averagegroup]::-1")
					fieldTokens.Add("xfText#:[dummy3]")
					fieldTokens.Add("xfText#:[dummy4]")
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
					DELETE FROM XFC_PLT_AUX_Production_TopFabBudget_Volumes
					WHERE 1=1
						AND id_factory = '{factory}'
					
					INSERT INTO XFC_PLT_AUX_Production_TopFabBudget_Volumes (id_factory, id_averagegroup, id_product_family, date, volume)

					    SELECT 
							'{factory}' as id_factory,
					        id_averagegroup,
					        id_product_family,
					        DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
					        valor
					    FROM (
					        SELECT id_averagegroup, id_product_family, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M02', M02 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M03', M03 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M04', M04 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M05', M05 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M06', M06 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M07', M07 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M08', M08 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M09', M09 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M10', M10 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M11', M11 FROM {tableName} UNION ALL
					        SELECT id_averagegroup, id_product_family, 'M12', M12 FROM {tableName}
					    ) AS expanded	
					"	
					
				#End Region
				
				#Region "Insert TopFab Activity into FACT Production"
				
				Else If fileName = "Exec - BudgetProduction" Then
					
					#Region "Old Mapping"
					sqlMap = $"
						DELETE FROM XFC_PLT_FACT_Production
							WHERE 1=1
								AND scenario = 'Budget'
								AND id_factory = '{factory}'
					
						INSERT INTO XFC_PLT_FACT_Production (scenario, date, id_factory, id_costcenter, id_averagegroup, id_workcenter, id_product, uotype, volume, allocation_taj, allocation_tso, activity_taj, activity_tso)
							
						SELECT DISTINCT
							'Budget' as scenario
							, A.date
							, A.id_factory 
							, '-1' as id_costcenter
							, A.id_averagegroup
							, '-1' as id_workcenter
							, A.id_product_family
							, CASE 
					      		WHEN uotype = 'UOE1' THEN 'A10'
					     		WHEN uotype = 'UOE2' THEN 'A20'
					     		ELSE 'TBD' 
							END as uotype
							, B.volume AS volume
							, A.allocation_taj as allocation_taj
							, A.allocation_tso as allocation_tso
							, A.allocation_taj * B.volume as ActividadTAJ 
							, A.allocation_tso * B.volume as ActividadTSO
						
						FROM XFC_PLT_AUX_Production_TopFabBudget_Volumes B
						
						INNER JOIN XFC_PLT_AUX_Production_TopFabBudget_Allocations A
							ON A.id_averagegroup = B.id_averagegroup
							AND A.id_product_family = B.id_product_family
							AND A.id_factory = B.id_factory 
							AND MONTH(A.date) = MONTH(B.date)
							AND YEAR(A.date) = YEAR(B.date)
						
						WHERE 1=1
							AND A.id_factory = '{factory}'
					"
					#End Region
					
					sqlMap = $"
						DELETE FROM XFC_PLT_FACT_Production
						WHERE 1=1
							AND scenario = 'Budget'
							-- AND id_factory = '{factory}'
					
						INSERT INTO XFC_PLT_FACT_Production (scenario, date, id_factory, id_costcenter, id_averagegroup, id_workcenter, id_product, uotype, volume, allocation_taj, allocation_tso, activity_taj, activity_tso)
						
						SELECT 
							'Budget' as scenario
							, A.date
							, A.id_factory 
							, '-1' as id_costcenter
							, A.id_averagegroup
							, '-1' as id_workcenter
							, A.id_product_family
							, COALESCE(B.uotype, C.uotype,'-1') 
							, A.volume AS volume
							, COALESCE(B.value, C.value) as allocation_taj
							, '0' as allocation_tso
							, A.volume * COALESCE(B.value, C.value) as ActividadTAJ 
							, '0' as ActividadTSO
					
						FROM XFC_PLT_AUX_Production_TopFabBudget_Volumes A
						
						LEFT JOIN XFC_PLT_AUX_Production_Planning_Times_NewProducts B
							ON A.id_factory = B.id_factory
							AND A.id_averagegroup = B.id_averagegroup
							AND A.id_product_family = B.id_product
							AND YEAR(A.date) = YEAR(B.date)
							AND MONTH(A.date) = MONTH(B.date)
							AND B.scenario = 'Budget'
					
						LEFT JOIN XFC_PLT_AUX_Production_Planning_Times C
							ON A.id_factory = C.id_factory
							AND A.id_averagegroup = C.id_averagegroup
							AND A.id_product_family = C.id_product
							AND YEAR(A.date) = YEAR(C.date)
							AND MONTH(A.date) = MONTH(C.date)
							AND (B.uotype = C.uotype OR B.uotype IS NULL)
							AND C.scenario = 'Budget'
					
						WHERE 1=1							
							-- AND A.id_factory = '{factory}'
					"
					
				#End Region
				
			#End Region	
				
		#End Region				
				
		#Region "COSTS"						
			
			#Region "COSTS (Monthly) - ALL Factories"
			
				#Region "Inser into RAW"
				
				Else If fileName = "ZSYPI" Then
					
					Dim factoryR As String = String.Empty
					Dim entityR As String = String.Empty
					
					If factory = "R0671" Then
						factoryR = "0671"
						entityR = "STE"
					Else If factory = "R0548913"
						factoryR = "1301"
						entityR = "003"						
					Else If factory = "R0548914"
						factoryR = "1301"
						entityR = "002"
					End If
					
					loadMethod = $"Replace:[id_factory = '{factoryR}', entity = '{entityR}', year = '{year}', month = '0{month}']"
					
					Dim fullPath As String = FileSystemLocation.ApplicationDatabase.ToString & filePath
					
					' Verificar si el archivo existe
					If BRApi.FileSystem.DoesFileExist(si, FileSystemLocation.ApplicationDatabase, filePath) Then
					
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
						fieldTokens.Add("xfText#:[documentF]")
						fieldTokens.Add("xfText#:[num_cpte]")
						fieldTokens.Add("xfText#:[id_costcenter]")
						fieldTokens.Add("xfText#:[num_ordre]")
						fieldTokens.Add("xfText#:[elm_OTP]")
						
						Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, newContentBytes, "|", dbLocation, tableName, loadMethod, fieldTokens, True)
	
					End If
					
				#End Region
					
				#Region "Insert Into Facts"
				
				Else If filePath.Contains("Exec - ZSYPI") Then
					
					sqlMap = $" 
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_RAW_CostsMonthly
								WHERE 1=1
									AND CAST([month] as INT) = 16					
					
								DELETE FROM XFC_PLT_FACT_Costs
								WHERE 1=1
									AND (id_factory = '{factory}' OR id_factory = 'TBD')
									AND scenario = 'Actual'
									AND YEAR(date) = {year}						
					
								-- 2- Insert statement				
					
								INSERT INTO XFC_PLT_FACT_Costs (scenario, id_factory, date, id_account, id_rubric, id_costcenter, value, currency)
					
								SELECT *
								FROM (     
									SELECT  
										'Actual' as scenario
									    , CASE
									      	WHEN CONCAT('R', id_factory, entity) LIKE '%R0611%' THEN 'R0045106'
									      	WHEN CONCAT('R',id_factory, entity) LIKE '%R1303%' THEN 'R0483003'
									      	WHEN CONCAT('R',id_factory, entity) LIKE '%R1302%' THEN 'R0529002'
									      	WHEN CONCAT('R',id_factory, entity) = 'R1301003' THEN 'R0548913'
									      	WHEN CONCAT('R',id_factory, entity) = 'R1301002' THEN 'R0548914'
									      	WHEN CONCAT('R',id_factory, entity) LIKE '%R0585%' THEN 'R0585'
									      	WHEN CONCAT('R',id_factory, entity) LIKE '%R0671%' THEN 'R0671'
									      	WHEN CONCAT('R',id_factory, entity) LIKE '%R0592%' THEN 'R0592'
									     ELSE 'TBD' END AS id_factory
									    , DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) as date
									    , mng_account as id_account
										, rubirc
									    , id_costcenter as id_costcenter
									    , SUM(value_intern) as value
									    , 'Local' as currency
					
									FROM (
					
									      		SELECT A.*
														, COALESCE(B.mng_account,'Others') as mng_account
					
									      		FROM XFC_PLT_RAW_CostsMonthly A
					
					
									            LEFT JOIN XFC_PLT_MASTER_CostCenter_Hist C
									             	ON A.id_costcenter = C.id
									          		AND C.scenario = 'Actual'
									          		AND DATEFROMPARTS(CAST([year] AS INT), CAST([month] AS INT), 1) BETWEEN C.start_date AND C.end_date
															
												LEFT JOIN (
															SELECT DISTINCT costcenter_type
															FROM XFC_PLT_Mapping_Accounts_docF 
														) as E
													ON C.type = E.costcenter_type
					
									      		-- LEFT JOIN XFC_PLT_Mapping_Accounts B
									       			-- ON A.num_cpte = B.cnt_account
													-- AND C.type = B.costcenter_type
													-- AND C.nature = B.costcenter_nature	
												
												LEFT JOIN (
															SELECT DISTINCT 
																cnt_account
																, mng_account
																, costcenter_type
																, costcenter_nature
																, docF
					
															FROM XFC_PLT_Mapping_Accounts_docF 
														) as B					
									       			ON A.num_cpte = B.cnt_account
													AND C.type = B.costcenter_type
													AND C.nature = B.costcenter_nature
													AND A.domFonc = B.docF	

									      		WHERE 1=1
													AND CAST(A.[month] as INT) <= 13										
													AND A.year = {year}	
												    AND CASE
												        WHEN CONCAT('R',A.id_factory, A.entity) LIKE '%R0611%' THEN 'R0045106'
												        WHEN CONCAT('R',A.id_factory, A.entity) LIKE '%R1303%' THEN 'R0483003'
												        WHEN CONCAT('R',A.id_factory, A.entity) LIKE '%R1302%' THEN 'R0529002'
												        WHEN CONCAT('R',A.id_factory, A.entity) = 'R1301003' THEN 'R0548913'
												        WHEN CONCAT('R',A.id_factory, A.entity) = 'R1301002' THEN 'R0548914'
												        WHEN CONCAT('R',A.id_factory, A.entity) LIKE '%R0585%' THEN 'R0585'
												        WHEN CONCAT('R',A.id_factory, A.entity) LIKE '%R0671%' THEN 'R0671'
												        WHEN CONCAT('R',A.id_factory, A.entity) LIKE '%R0592%' THEN 'R0592'
												      ELSE 'TBD' END = '{factory}'					
					
									      		)  AS ext
										
									      GROUP BY id_factory, entity, [year], [month], mng_account, rubirc, id_costcenter
					
									) AS mapeo	
					
									WHERE 1=1
										AND id_factory <> 'TBD'
											
									"	
					
'									BRApi.ErrorLog.LogMessage(si, sqlMap)
				#End Region				
				
			#End Region				
			
			#Region "COSTS - Budget Costs"			
			
				Else If fileName.Equals("BudgetCosts.csv") Then
					
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
	
			#Region "COSTS - Allocation Keys"
			
				Else If fileName = $"CostsAllocationKeys.csv" Then
					
					tableName = "XFC_PLT_RAW_CostsAllocations"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_averagegroup] varchar(50) ,
								[costnature] varchar(50) ,
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

					fieldTokens.Add("xfText#:[scenario]")
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[costnature]")
					fieldTokens.Add("xfText#:[dummy4]")
					fieldTokens.Add("xfText#:[id_averagegroup]")
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
																		
					sqlMap = $"	
								-- 1- Previous clear
					
								DELETE FROM XFC_PLT_AUX_AllocationKeys
								WHERE 1=1
									AND scenario IN (SELECT scenario FROM {tableName} GROUP BY scenario)
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
					
								-- 2- Insert statement
													
								INSERT INTO XFC_PLT_AUX_AllocationKeys (scenario, id_costcenter, id_averagegroup, costnature, date, percentage, id_factory)
								SELECT
								    scenario
								    , id_costcenter
								    , id_averagegroup
								    , CASE 
										WHEN costnature = 'MOD' THEN 1
										WHEN costnature = 'MOS' THEN 2
										WHEN costnature = 'FIP' THEN 3
										WHEN costnature = 'IT' THEN 4
										WHEN costnature = 'Amo' THEN 5
										ELSE '-1'
									END AS costnature						
								    , DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor/100 AS percentage
									, '{factory}' As factory
								FROM
								    (
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M01' AS mes, M01 AS valor FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M02', M02 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M03', M03 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M04', M04 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M05', M05 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M06', M06 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M07', M07 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M08', M08 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M09', M09 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M10', M10 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M11', M11 FROM XFC_PLT_RAW_CostsAllocations UNION ALL
								        SELECT scenario, id_costcenter, id_averagegroup, costnature, 'M12', M12 FROM XFC_PLT_RAW_CostsAllocations
								    ) AS expanded	
												
								WHERE 1=1
									AND valor <> 0
							"
							BRAPI.ErrorLog.LogMessage(si, sqlMap)
			#End Region
			
			#Region "COSTS - Fixed Variable"
			
				Else If fileName = $"CostsFixedVariable.csv" Then
					
					tableName = "XFC_PLT_RAW_CostsFixedVariable"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
								[id_account] varchar(50) ,
								[costnature] varchar(50) ,
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
					
					fieldTokens.Add("xfText#:[scenario]")
					fieldTokens.Add("xfText#:[costnature]")
					fieldTokens.Add("xfText#:[id_account]")
					fieldTokens.Add("xfText#:[dummy1]")
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
									AND scenario IN (SELECT scenario FROM {tableName} GROUP BY scenario)
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
					
								-- 2- Insert statement
																					
								INSERT INTO XFC_PLT_AUX_FixedVariableCosts (scenario, id_account, costnature, date, value, id_factory)
								SELECT
								    scenario,
								    id_account,
									CASE WHEN costnature = 'Non affect?' 
										THEN -1
										ELSE costnature 
									END AS costnature,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS percentage,
									'{factory}' As factory
								FROM
								    (
								        SELECT scenario, id_account, costnature, 'M01' AS mes, M01 AS valor FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M02', M02 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M03', M03 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M04', M04 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M05', M05 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M06', M06 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M07', M07 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M08', M08 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M09', M09 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M10', M10 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M11', M11 FROM XFC_PLT_RAW_CostsFixedVariable UNION ALL
								        SELECT scenario, id_account, costnature, 'M12', M12 FROM XFC_PLT_RAW_CostsFixedVariable
								    ) AS expanded	
					
								WHERE 1=1						
							"
		
			#End Region			
				
			#Region "COSTS - Effects Analysis"
			
				Else If fileName = $"EffectAnalysis" Then
					
					tableName = "XFC_PLT_RAW_EffectsAnalysis"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
								[ref_scenario] varchar(50) ,
								[cost_type] varchar(50) ,
								[id_indicator] varchar(50) ,
								[variability] varchar(50) ,
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
					
					fieldTokens.Add("xfText#:[scenario]")
					fieldTokens.Add("xfText#:[ref_scenario]")
					fieldTokens.Add("xfText#:[cost_type]")
					fieldTokens.Add("xfText#:[id_indicator]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[variability]")
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
					
								DELETE FROM XFC_PLT_AUX_EffectsAnalysis
								WHERE 1=1
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
					
								-- 2- Insert statement
					
								INSERT INTO XFC_PLT_AUX_EffectsAnalysis (scenario, ref_scenario, cost_type, id_indicator, variability, date, value, id_factory)
								SELECT
									scenario
								    , ref_scenario
									, cost_type
									, id_indicator
									, variability
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
								FROM
								    (
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M02', M02 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M03', M03 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M04', M04 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M05', M05 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M06', M06 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M07', M07 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M08', M08 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M09', M09 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M10', M10 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M11', M11 FROM {tableName} UNION ALL
										SELECT scenario, ref_scenario, cost_type, id_indicator, variability, 'M12', M12 FROM {tableName}
								    ) AS expanded	
					
								WHERE 1=1
							"
		
			#End Region			
			
			#Region "COSTS - Energy Variance"
			
				Else If filePath.Contains($"EnergyVariance.csv") Then
					
					tableName = "XFC_PLT_RAW_EnergyVariance"	
										
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[scenario] varchar(50) ,
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
					
					fieldTokens.Add("xfText#:[scenario]")
					fieldTokens.Add("xfText#:[energy_type]")
					fieldTokens.Add("xfText#:[indicator]")
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[dummy2]")
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
									AND (scenario IN (SELECT scenario FROM {tableName} GROUP BY scenario) OR scenario = 'Budget')
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
						
					
								-- 2- Insert statement
																					
								INSERT INTO XFC_PLT_AUX_EnergyVariance (scenario, energy_type, indicator, date, value, id_factory)
								SELECT
								    scenario
									, energy_type
									, indicator
									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
									, valor AS value
									, '{factory}' As factory
								FROM
								    (
										SELECT scenario, energy_type, indicator, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M02', M02 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M03', M03 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M04', M04 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M05', M05 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M06', M06 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M07', M07 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M08', M08 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M09', M09 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M10', M10 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M11', M11 FROM {tableName} UNION ALL
										SELECT scenario, energy_type, indicator, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
								WHERE 1=1
							"

			#End Region
			
			#Region "COSTS - StartUp Costs - No desarrollado"
			
				Else If fileName = $"StartUp.csv" Then
					
'					tableName = "XFC_PLT_RAW_StartUpCosts"	
										
'					sqlRaw = $"CREATE TABLE {tableName}
'								(
'								[ProjectCode] varchar(50) ,
'								[Offer] varchar(50) ,
'								[Resonsibility] varchar(50) ,
'								[CostType] varchar(50) ,
'								[ProjectTask] varchar(50) ,
'								[ProjecttaskText] varchar(50) ,
'								[Currency] varchar(50) ,
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
					
'					sqlMap = $"	
'								-- 1- Previous clear
					
'								DELETE FROM XFC_PLT_AUX_Projects
'								WHERE 1=1
'									AND scenario = 'Budget'
'									AND id_factory = '{factory}'
'									AND YEAR(date) = {year}
'									-- AND MONTH(date) = {month}
'									AND project_file = 'startup'
						
					
'								-- 2- Insert statement
																					
'								INSERT INTO XFC_PLT_AUX_Projects (scenario, id_project_code, offer, responsibility, cost_type, project_task, project_task_text, currency, date, value, id_factory, project_file)
'								SELECT
'								    'Budget' as scenario
'									, ProjectCode
'									, Offer
'									, Resonsibility
'									, CostType
'									, ProjectTask
'									, ProjecttaskText
'									, Currency
'									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
'									, valor AS value
'									, '{factory}' As factory
'									, 'startup' As project_file
'								FROM
'								    (
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M02', M02 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M03', M03 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M04', M04 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M05', M05 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M06', M06 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M07', M07 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M08', M08 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M09', M09 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M10', M10 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M11', M11 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M12', M12 FROM {tableName}
'								    ) AS expanded		
					
'								WHERE 1=1
'									-- AND CAST(SUBSTRING(mes, 2, 2) AS INT) = {month}						
'							"

			#End Region				
		
			#Region "COSTS - RampUp Costs - No desarrollado"
			
'				Else If fileName = $"RampUp.csv" Then
					
'					tableName = "XFC_PLT_RAW_RampUpCosts"	
										
'					sqlRaw = $"CREATE TABLE {tableName}
'								(
'								[ProjectCode] varchar(50) ,
'								[Offer] varchar(50) ,
'								[Resonsibility] varchar(50) ,
'								[CostType] varchar(50) ,
'								[ProjectTask] varchar(50) ,
'								[ProjecttaskText] varchar(50) ,
'								[Currency] varchar(50) ,
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
					
'					sqlMap = $"	
'								-- 1- Previous clear
					
'								DELETE FROM XFC_PLT_AUX_Projects
'								WHERE 1=1
'									AND scenario = 'Budget'
'									AND id_factory = '{factory}'
'									AND YEAR(date) = {year}
'									-- AND MONTH(date) = {month}
'									AND project_file = 'rampup'
						
					
'								-- 2- Insert statement
																					
'								INSERT INTO XFC_PLT_AUX_Projects (scenario, id_project_code, offer, responsibility, cost_type, project_task, project_task_text, currency, date, value, id_factory, project_file)
'								SELECT
'								    'Budget' as scenario
'									, ProjectCode
'									, Offer
'									, Resonsibility
'									, CostType
'									, ProjectTask
'									, ProjecttaskText
'									, Currency
'									, DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date
'									, valor AS value
'									, '{factory}' As factory
'									, 'rampup' as project_file
'								FROM
'								    (
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M02', M02 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M03', M03 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M04', M04 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M05', M05 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M06', M06 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M07', M07 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M08', M08 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M09', M09 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M10', M10 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M11', M11 FROM {tableName} UNION ALL
'										SELECT ProjectCode, Offer, Resonsibility, CostType, ProjectTask, ProjecttaskText, Currency, 'M12', M12 FROM {tableName}
'								    ) AS expanded		
					
'								WHERE 1=1
'									-- AND CAST(SUBSTRING(mes, 2, 2) AS INT) = {month}						
'							"

			#End Region			
				
		#End Region	
		
		#Region "HR"
		
			#Region "Time of Presence"
				Else If fileName = $"TimePresence.csv" Then
					
					tableName = "XFC_PLT_RAW_TimePresence"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[wf_type] varchar(50) ,
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
					
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[wf_type]")
					fieldTokens.Add("xfText#:[id_costcenter]")
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
					
								DELETE FROM XFC_PLT_AUX_TimePresence
								WHERE 1=1
									-- AND (scenario = '{scenario}' OR '{scenario}' = 'NA')
									AND scenario = 'Budget'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									-- AND MONTH(date) = {month}
						
					
								-- 2- Insert statement					
					
								INSERT INTO XFC_PLT_AUX_TimePresence (wf_type, id_costcenter, date, value, id_factory, scenario)
								SELECT
								    wf_type,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory,
									'Budget' As scenario
								FROM
								    (
								        SELECT wf_type, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT wf_type, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded
					
								WHERE 1=1
									-- AND CAST(SUBSTRING(mes, 2, 2) AS INT) = {month}
							"			
			#End Region
			
			#Region "Daily Hours"
				Else If fileName = $"DailyHours.csv" Then
					
					tableName = "XFC_PLT_RAW_DailyHours"	
					
					sqlRaw = $"	CREATE TABLE {tableName} 
							(
							    wf_type VARCHAR(50),
							    id_costcenter VARCHAR(50),
							    Shift VARCHAR(50),
							    date VARCHAR(50),
							    daily_working_hours DECIMAL(18,2),
							    unemployment DECIMAL(18,2),
							    gift_closing_hours DECIMAL(18,2),
							    collective_leave DECIMAL(18,2),
							    extra_hours DECIMAL(18,2),
							    no_or_suspended_contract DECIMAL(18,2),
							    training DECIMAL(18,2),
							    sick_leave DECIMAL(18,2),
							    paid_non_working_hours DECIMAL(18,2),
							    labor_relation_hours DECIMAL(18,2),
							    bank_of_hours DECIMAL(18,2),
							    days_off DECIMAL(18,2),
							    strike DECIMAL(18,2),
							    not_paid_non_working_hours DECIMAL(18,2),
							    holiday DECIMAL(18,2)
							)"
													
					
					#Region "Mapeo Largo"
					sqlMap = $"
						    -- 1- Previous clear
						    DELETE FROM XFC_PLT_AUX_DailyHours
						    WHERE 1=1
						        AND scenario = '{scenario}'
						        AND id_factory = '{factory}'
						        AND YEAR(date) = {year}
						        AND MONTH(date) = {month};
						
						    -- 2- Insert statement
						    INSERT INTO XFC_PLT_AUX_DailyHours (wf_type, id_costcenter, date, value, id_factory, scenario, id_indicator)
						    SELECT 
						        wf_type,
						        id_costcenter,
						        date,
						        value,
						        '{factory}' AS id_factory,
						        '{scenario}' AS scenario,
						        id_indicator
						    FROM (
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, daily_working_hours AS value, 'daily_working_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, unemployment AS value, 'unemployment' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, gift_closing_hours AS value, 'gift_closing_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, collective_leave AS value, 'collective_leave' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, extra_hours AS value, 'extra_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, no_or_suspended_contract AS value, 'no_or_suspended_contract' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, training AS value, 'training' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, sick_leave AS value, 'sick_leave' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, paid_non_working_hours AS value, 'paid_non_working_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, labor_relation_hours AS value, 'labor_relation_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, bank_of_hours AS value, 'bank_of_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, days_off AS value, 'days_off' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, strike AS value, 'strike' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, not_paid_non_working_hours AS value, 'not_paid_non_working_hours' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						
						        UNION ALL
						
						        SELECT wf_type, id_costcenter, CAST(date AS DATE) AS date, holiday AS value, 'holiday' AS id_indicator
						        FROM XFC_PLT_RAW_DailyHours
						        WHERE MONTH(CAST(date AS DATE)) = {month}
						    ) AS expanded
					
						WHERE 1=1
							AND value <> 0
						"

					#End Region
					
			#End Region	
			
			#Region "Workforce"
				Else If fileName = $"Workforce.csv" Then
					
					tableName = "XFC_PLT_RAW_Workforce"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[wf_type] varchar(50) ,
								[id_costcenter] varchar(50) ,
								[id_function]  decimal(18,0),
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
													
										
					fieldTokens.Add("xfText#:[dummy1]")
					fieldTokens.Add("xfText#:[wf_type]")
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[dummy2]")
					fieldTokens.Add("xfText#:[dummy3]")
					fieldTokens.Add("xfText#:[dummy4]")
					fieldTokens.Add("xfText#:[dummy5]")
					fieldTokens.Add("xfText#:[id_function]")
					fieldTokens.Add("xfText#:[dummy6]")
					fieldTokens.Add("xfText#:[dummy7]")
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
					
								DELETE FROM XFC_PLT_AUX_Workforce
								WHERE 1=1
									AND scenario = 'Budget'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									-- AND MONTH(date) = {month}
						
					
								-- 2- Insert statement	
					
					
								INSERT INTO XFC_PLT_AUX_Workforce (wf_type, id_function, id_costcenter, date, value, id_factory, scenario)
								SELECT
								    wf_type,
								    id_function,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory,
									'Budget' As scenario
								FROM
								    (
								        SELECT wf_type, id_function, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT wf_type, id_function, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded		
					
					
								WHERE 1=1
									-- AND CAST(SUBSTRING(mes, 2, 2) AS INT) = {month}					
							"			
			#End Region	
			
			#Region "Balancing"
				Else If fileName = $"Balancing.csv" Then
					
					tableName = "XFC_PLT_RAW_Balancing"	
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[wf_type] varchar(50),
								[bal_code] varchar(50) ,
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
													
					fieldTokens.Add("xfText#:[id_costcenter]")
					fieldTokens.Add("xfText#:[wf_type]")
					fieldTokens.Add("xfText#:[bal_code]")
					fieldTokens.Add("xfText#:[dummy1]")
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
					
								DELETE FROM XFC_PLT_AUX_Balancing
								WHERE 1=1
									AND scenario = 'Budget'
									AND id_factory = '{factory}'
									AND YEAR(date) = {year}
									-- AND MONTH(date) = {month}
						
					
								-- 2- Insert statement									
								INSERT INTO XFC_PLT_AUX_Balancing (wf_type, bal_code, id_costcenter, date, value, id_factory, scenario)
								SELECT
								    wf_type,
								    bal_code,
								    id_costcenter,
								    DATEADD(MONTH, CAST(SUBSTRING(mes, 2, 2) AS INT) - 1, '{year}-01-01') AS date,
									valor AS value,
									'{factory}' As factory,
									'Budget' As scenario
								FROM
								    (
								        SELECT wf_type, bal_code, id_costcenter, 'M01' AS mes, M01 AS valor FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M02', M02 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M03', M03 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M04', M04 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M05', M05 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M06', M06 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M07', M07 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M08', M08 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M09', M09 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M10', M10 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M11', M11 FROM {tableName} UNION ALL
								        SELECT wf_type, bal_code, id_costcenter, 'M12', M12 FROM {tableName}
								    ) AS expanded
					
								WHERE 1=1
									-- AND CAST(SUBSTRING(mes, 2, 2) AS INT) = {month}							
							"
					brapi.ErrorLog.LogMessage(si, sqlMap)
			#End Region				
		
		#End Region		
		
		' FALTAN por desarrollar los metadatos
		
		#Region "METADATA"
			
			#Region "Master"
			
				#Region "CostCenterHist"
				Else If filePath.Contains("XFC_PLT_MASTER_CostCenter_Hist") Then
											
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
				Else If filePath.Contains("XFC_PLT_MASTER_CostCenter") Then
					
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
				
				#Region "Accounts"
				Else If filePath.Contains("XFC_PLT_MAPPING_Accounts") Then
					
						tableName = "XFC_PLT_MAPPING_Accounts"
						
						fieldTokens.Add("xfText#:[cnt_account]")
						fieldTokens.Add("xfText#:[account_type]")
						fieldTokens.Add("xfText#:[account_subtype]")
						fieldTokens.Add("xfText#:[mng_account]")
						fieldTokens.Add("xfText#:[description]")
						fieldTokens.Add("xfText#:[os_account]::-1")
						fieldTokens.Add("xfText#:[id_costcenter]::-1")
						fieldTokens.Add("xfText#:[costcenter_type]")
						
						BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
						Return loadResults
				#End Region
				
				#Region "Product"
				
					#Region "Products IDs - System Import"
				Else If filePath.Contains("XFC_PLT_MASTER_Product") Then
					
					'Create Session Info for another app
					Dim oar As New OpenAppResult
					Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Dev", oar)
					
					'Get table name and declare dt
					Dim dt As New DataTable
					
					If oar = OpenAppResult.Success Then
						
						Using mainDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Using oDbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(osi)
						
							'Get all the rows from external table
							Dim selectQuery As String = $"
									SELECT A.A1 as id, '' as description
									
									FROM StageTargetData B
									LEFT JOIN StageAttributeData A
										ON B.Wfk = A.Wfk
										AND B.Wtk = A.Wtk
										AND B.Ri = A.Ri						
																		
									WHERE 1=1
										AND A.A1 <> ''
									GROUP BY A.A1,
									
									"
							
							selectQuery = "
							SELECT id, description
							
							FROM XFC_PLT_HIER_Product
							
							GROUP BY id, description
							"
							dt = BRApi.Database.ExecuteSql(oDbConn, selectQuery, False)
							
							'Truncate target table
							Dim deleteQueryProduct As String = $"
							TRUNCATE TABLE XFC_PLT_MASTER_Product
							"
							BRApi.Database.ExecuteSql(mainDbConn, deleteQueryProduct, False)
							
							'Generate inserts
							For Each insertQuery As String In GenerateInsertSQLList(dt, "XFC_PLT_MASTER_Product")
								BRApi.Database.ExecuteSql(mainDbConn, insertQuery, False)
							Next
						
						End Using
						End Using
										
					End If		
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
					
					#Region "Nomenclature"
					
				Else If fileName = "Nomenclature.csv" Then
					
'					loadMethod = $"Replace:[(id_factory = '{factory}')]"
					tableName = "XFC_PLT_AUX_Nomenclature"
					tableDest = "XFC_PLT_HIER_Nomenclature"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_product] varchar(50) ,
								[id_component] varchar(50) ,
								[coefficient] decimal(18,2)
								)"
					
					' fieldTokens.Add("xfText#:[dummy0]")			
					fieldTokens.Add("xfText#:[id_product]")
					fieldTokens.Add("xfText#:[dummy1]")			
					fieldTokens.Add("xfText#:[id_component]")
					fieldTokens.Add("xfDec#:[coefficient]")
					fieldTokens.Add("xfText#:[dummy2]")			
					fieldTokens.Add("xfText#:[dummy3]")			
					fieldTokens.Add("xfText#:[dummy4]")			
					fieldTokens.Add("xfText#:[dummy5]")			
					fieldTokens.Add("xfText#:[dummy6]")			
					fieldTokens.Add("xfText#:[dummy7]")			
					
					sqlMap = $"
					
						DELETE FROM {tableDest} 
						WHERE 1=1
							AND id_factory = '{factory}'
					
						INSERT INTO {tableDest} ( id_factory, id_product, id_component, coefficient)
					
						SELECT
							'{factory}' as id_factory
							, id_product
							, id_component
							, SUM(coefficient) as coefficient
						
						FROM {tableName}
					
						GROUP BY id_product, id_component
					
					"
					
				#End Region
				
					#Region "Nomenclature Dates"
					
				Else If fileName = "Nomenclature_Data.csv" Then
					
'					loadMethod = $"Replace:[(id_factory = '{factory}')]"
					tableName = "XFC_PLT_RAW_Nomenclature_Date"
					tableDest = "XFC_PLT_HIER_Nomenclature_Date"
					
					sqlRaw = $"CREATE TABLE {tableName}
								(
								[id_factory] varchar(50),
								[id_product] varchar(50),
								[id_component] varchar(50),
								[coefficient] decimal(18,2),
								[prorata] decimal(18,2),
								[start_date_raw] varchar(50),
								[end_date_raw] varchar(50) 
								)"
					
					fieldTokens.Add("xfText#:[id_factory]")			
					fieldTokens.Add("xfText#:[id_product]")
					fieldTokens.Add("xfText#:[dummy1]")	'Descripcion		
					fieldTokens.Add("xfText#:[id_component]")
					fieldTokens.Add("xfDec#:[coefficient]")
					fieldTokens.Add("xfText#:[dummy2]")	'Code plus		
					fieldTokens.Add("xfText#:[dummy3]")	'No orde		
					fieldTokens.Add("xfText#:[dummy4]")	'No sequence		
					fieldTokens.Add("xfText#:[prorata]")			
					fieldTokens.Add("xfText#:[start_date_raw]")			
					fieldTokens.Add("xfText#:[end_date_raw]")		
					
					sqlMap = $"
					
						DELETE FROM {tableDest} 
						WHERE 1=1
					
						INSERT INTO {tableDest} ( id_factory, id_product, id_component, coefficient, prorata, start_date, end_date)
					
						SELECT
							id_factory
							, id_product
							, id_component
							, SUM(coefficient) as coefficient
							, AVG(prorata)/100 as prorata
							, CONVERT(date, start_date_raw, 103) as start_date
							, CONVERT(date, end_date_raw, 103) as end_date
							-- , 0 as new_product
						
						FROM {tableName}
					
						GROUP BY 
							id_factory
							, id_product
							, id_component
							, CONVERT(date, start_date_raw, 103) 
							, CONVERT(date, end_date_raw, 103) 
					
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
							Dim deleteQueryProduct As String = $"
							TRUNCATE TABLE XFC_PLT_HIER_Product
							"
							BRApi.Database.ExecuteSql(mainDbConn, deleteQueryProduct, False)
							
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
	#End Region
	
	#Region "Ejecuciones Finales"									
				If sqlRaw <> String.Empty Then 
					
					' 1. Creación de tabla temporal RAW
					ExecuteSql(si, "DROP TABLE IF EXISTS "& tableName)
					ExecuteSql(si, sqlRaw) 
				
					' 2. Carga de datos en tabla temporal RAW					
					Dim delimitedLoadResult As TableRangeContent = BRApi.Utilities.LoadCustomTableUsingDelimitedFile(si, SourceDataOriginTypes.FromFileUpload, filePath, fileInfo.XFFile.ContentFileBytes, ";", dbLocation, tableName, loadMethod, fieldTokens, True)
				
				End If
				
				' 3. Insertar los datos mapeados en la tabla FINAL
				If sqlMap <> String.Empty Then ExecuteSql(si, sqlMap)					
				BRApi.ErrorLog.LogMessage(si, sqlMap)
				
				' 4. Eliminar la tabla temporal RAW
				' If sqlRaw <> String.Empty Then ExecuteSql(si, "DROP TABLE " & tableName)											
				
				If fileName.Contains("Exec") Then
					
					Brapi.ErrorLog.LogMessage(si, fileName & " - Insertados los datos en la tabla de echos")
					
				Else	
					
					' 5. Movemos el Fichero a cargas
					'  5.1- Recogemos la información del fichero cargado
					Dim file As XFFile = BRApi.FileSystem.GetFile(si,FileSystemLocation.ApplicationDatabase,filePath,True, True).XFFile
					
					' 5.2- Creación de un archivo igual con el nombre deseado
					Dim fileName0 As String = fileName.Split(".")(0)
					Dim fileExt As String = fileName.Split(".")(1)
					
					file.FileInfo.Name = $"{fileName0}_Loaded_{year}.{fileExt}"
					
					Brapi.FileSystem.InsertOrUpdateFile(si, file)
					
					' 5.3- Eliminamos el fichero original Cargado
					BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, filePath)
					
				End If			
				Return loadResults
	#End Region
	
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