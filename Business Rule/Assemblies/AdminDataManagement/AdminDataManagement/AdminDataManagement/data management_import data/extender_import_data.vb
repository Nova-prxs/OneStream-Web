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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_data
	Public Class MainClass
		
		'Reference "UTI_SharedFunctions" Business Rule
		Dim UTISharedFunctionsBR As New OneStream.BusinessRule.Finance.UTI_SharedFunctions.MainClass
		
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						'Get parameters, create full path and declare column mapping dictionary
						Dim tableName As String = args.NameValuePairs("p_table")
						Dim method As String = args.NameValuePairs("p_method")
						Dim filesType As String = args.NameValuePairs("p_files_type")
						Dim delimiter As String = String.Empty
						If filesType.ToLower = "delimited" Then delimiter = args.NameValuePairs("p_delimiter")
						Dim location = FileSystemLocation.ApplicationDatabase
						Dim sourcePath As String = args.NameValuePairs("p_folder")
						Dim targetFinalPath As String = "Processed"
						Dim targetPath As String = $"{sourcePath}/{targetFinalPath}"
						Dim columnMappingDict As New Dictionary(Of String, String)
						Dim fileName = args.NameValuePairs("p_file_name")
						
						'Get column mapping dictionary
						columnMappingDict = Me.GetColumnMappingDict(si, tableName)
						
						'Get file names in the delimited files folder
						Dim folderFileNames As List(Of NameAndAccessLevel) = BRApi.FileSystem.GetAllFileNames(
							si,
							location,
							sourcePath,
							XFFileType.All,
							False, False, False
						)
						
						'Handle number of files
						If folderFileNames.Count < 1 Then
							Throw New Exception($"No files found on Path: '{sourcePath}'")
						End If
						
						'Create data table from files folder
						Dim dt As New DataTable 
						
						'Loop through all the files and get the correct one
						Dim isFileInFolder As Boolean = False
						Dim fileFullName As String = String.Empty
						For Each folderFileName As NameAndAccessLevel In folderFileNames
							If Not folderFileName.Name.ToLower.Contains(fileName.ToLower) Then Continue For
								
							If filesType.ToLower = "delimited" Then
								'Get file content as string
								Dim fileString As String = system.Text.Encoding.UTF8.GetString(
									BRApi.FileSystem.GetFile(
										si,
										location,
										folderFileName.Name,
										True, True
									).XFFile.ContentFileBytes
								)
								'Convert delimited file to dt
								dt = UTISharedFunctionsBR.CreateDataTableFromDelimitedString(si, fileString, delimiter, True)
							Else If filesType.ToLower = "excel" Then
								'Get file content as stream
								Dim fileStream As Stream = New MemoryStream(
									BRApi.FileSystem.GetFile(
										si,
										location,
										folderFileName.Name,
										True, True
									).XFFile.ContentFileBytes
								)
								'Convert excel file to dt
								dt = UTISharedFunctionsBR.CreateDataTableFromExcelFile(si, fileStream, True)
							Else
								Throw New Exception("error importing data: file type must be 'delimited' or 'excel'")
							End If
							isFileInFolder = True
							fileFullName = folderFileName.Name
							fileName = folderFileName.Name.Split("/").Last()
						Next
						If Not isFileInFolder Then Throw New Exception("File for the import not found, make sure that you uploaded the correct file." & vbCrLf &
							$"Name must contain '{fileName}'.")
						
						'Map and filter columns in DataTable
						dt = UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict)
						
						Try
							'Load data table to custom table
							UTISharedFunctionsBR.LoadDataTableToCustomTable(si, tableName, dt, method)
							'Run up population queries for dependent tables
							Me.RunPopulationQueries(si, tableName, targetPath, fileName)
							
						Catch ex As Exception
							BRApi.FileSystem.DeleteFile(si, location, fileFullName)
							Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
						End Try
							
						'Copy file to processed folder and delete original file
						'Create processed folder if necessary
						BRApi.FileSystem.CreateFullFolderPathIfNecessary(si, FileSystemLocation.ApplicationDatabase, sourcePath, targetFinalPath)
						UTISharedFunctionsBR.CopyFile(si, location, fileName, sourcePath, fileName, targetPath)
						BRApi.FileSystem.DeleteFile(si, location, fileFullName)
						
					Case Is = ExtenderFunctionType.ExecuteExternalDimensionSource
				End Select

				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helper Functions"
		
		#Region "Get Column Mapping Dictionary"
		
		Private Function GetColumnMappingDict(ByVal si As SessionInfo, ByVal tableName As String) As Dictionary(Of String, String)
			'Declare new dictionary
			Dim columnMappingDict As Dictionary(Of String, String)
		
			'Control table column mapping dictionaries
			
			#Region "Master Companies"
			
			If tableName = "XFC_MAIN_MASTER_Companies" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Soc.", "id"},
			        {"Nombre de sociedad GL", "name"},
					{"Mon.", "currency_code"},
					{"N.I.F. comunitario", "tax_id"}
				}
				
			#End Region
			
			#Region "Raw Profit Centers"
			
			Else If tableName = "XFC_MAIN_RAW_ProfitCenters" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Soc.", "company_id"},
			        {"CeBe", "id"},
					{"Denominación", "description"}
				}
				
			#End Region
			
			#Region "Raw Cost Centers"
			
			Else If tableName = "XFC_MAIN_RAW_CostCenters" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Cost Ctr", "id"},
			        {"Name", "description"},
					{"CCtC", "class_id"},
					{"Cost Center Category", "class_description"},
					{"Profit Ctr", "profit_center_id"},
					{"Valid From", "start_date"},
					{"to", "end_date"},
					{"Bloqueado", "is_blocked"}
				}
				
			#End Region
			
			#Region "Raw Work Centers"
			
			Else If tableName = "XFC_MAIN_RAW_WorkCenters" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Workcenter", "id"},
			        {"Cost Center", "cost_center_id"},
					{"Start Date", "start_date"},
					{"End Date", "end_date"}
				}
				
			#End Region
			
			#Region "Raw Accounts"
			
			Else If tableName = "XFC_MAIN_RAW_Accounts" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Cuenta", "name"},
			        {"Descripcion", "description"},
					{"Flujo", "flow"}
				}
				
			#End Region
			
			#Region "Raw Products"
			
			Else If tableName = "XFC_MAIN_RAW_Products" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Nº de Material", "id"},
			        {"Descripcion Material", "description"},
					{"Tipo de material", "type_id"},
					{"Descripcion tipo de", "type_description"}
				}
				
			#End Region
			
			#Region "Master Organs"
			
			Else If tableName = "XFC_MAIN_MASTER_Organs" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Origin", "origin"},
			        {"Name", "name"},
					{"Organ Type", "type_name"},
					{"Organ Name", "commercial_name"}
				}
				
			#End Region
			
			#Region "Raw Business Partners"
			
			Else If tableName = "XFC_MAIN_RAW_BusinessPartners" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Interl.comercial", "id"},
			        {"Nombre 1", "name"},
					{"Clave de país/región", "country"},
					{"Cliente", "customer"}
				}
				
			#End Region
			
			#Region "Raw Trading Partners"
			
			Else If tableName = "XFC_MAIN_RAW_TradingPartners" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"NºSoc.", "id"},
			        {"Nombre de la sociedad GL", "name"}
				}
				
			#End Region
			
			#Region "Raw Account BS to Conso"
			
			Else If tableName = "XFC_MAIN_RAW_AccountBSToConso" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"RUBRIC", "rubric"},
					{"S4/HANA 8D", "account_name"}
				}
				
			#End Region
			
			#Region "Raw Account PnL to Conso"
			
			Else If tableName = "XFC_MAIN_RAW_AccountPnLToConso" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"S4H", "account_name"},
					{"A", "A"},
					{"B", "B"},
					{"C", "C"},
					{"D", "D"},
					{"E", "E"},
					{"F", "F"},
					{"G", "G"},
					{"H", "H"},
					{"I", "I"},
					{"J", "J"},
					{"K", "K"},
					{"L", "L"},
					{"M", "M"},
					{"N", "N"},
					{"O", "O"},
					{"P", "P"},
					{"Q", "Q"},
					{"0", "0"},
					{"R", "R"},
					{"S", "S"},
					{"T", "T"},
					{"U", "U"},
					{"V", "V"},
					{"W", "W"},
					{"X", "X"},
					{"Y", "Y"},
					{"1", "1"},
					{"2", "2"},
					{"Z", "Z"},
					{"PD", "PD"},
					{"PC", "PC"},
					{"None", "none"}
				}
				
			#End Region
			
			#Region "Raw Accounts Old To New"
			
			Else If tableName = "XFC_MAIN_RAW_AccountsOldToNew" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"ALCOR", "name_old"},
					{"S4H", "name"}
				}
				
			#End Region
			
			#Region "Raw Cost Center Class To Rubric"
			
			Else If tableName = "XFC_MAIN_RAW_CostCenterClassToRubric" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"id", "id"},
					{"depreciation_account", "depreciation_account"},
					{"asset_account", "asset_account"}
				}
				
			#End Region
			
			#Region "Master Flow Mappings"
			
			Else If tableName = "XFC_MAIN_MASTER_FlowMappings" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"sap flow", "sap_flow"},
					{"os flow", "flow"}
				}
				
			#End Region
			
			#Region "Master Cost Centers Old to New"
			
			Else If tableName = "XFC_MAIN_MASTER_CostCentersOldToNew" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"COST CENTER SAP4H", "id"},
					{"Cost center ALCOR", "id_old"}
				}
				
			#End Region
			
			#Region "Master Cost Centers Old to OneStream Member"
			
			Else If tableName = "XFC_MAIN_MASTER_CostCentersOldToOneStreamMember" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"UD3", "onestream_member_name"},
					{"Cost Center Old", "id_old"}
				}
				
			#End Region
			
			#Region "Raw Forex Rates"
			
			Else If tableName = "XFC_MAIN_RAW_ForexRates" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"De", "source"},
					{"A", "target"},
					{"Válido de", "date"},
					{"Tipo cambio", "rate"}
				}
				
			#End Region
			
			#Region "Raw Customers To UD1 IC"
			
			Else If tableName = "XFC_MAIN_RAW_CustomersToUD1IC" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Interl.comercial", "id"},
					{"UD1_Customer_OneStream", "customer_member_name"},
					{"IC_Interco_OneStream", "ic_member_name"}
				}
				
			#End Region
			
			#Region "Raw PnL Transactions"
			
			Else If tableName = "XFC_MAIN_RAW_PnLTransactions" Then
				columnMappingDict = New Dictionary(Of String, String) From {
					{"Fiscal Year", "year"},
					{"Period", "month"},
					{"Company Code", "company_id"},
					{"GL Account", "account_name"},
					{"Cost Center", "cost_center_id"},
					{"Profit Center", "profit_center_id"},
					{"Orden CO", "order_id"},
					{"Vendor", "supplier_id"},
					{"Client", "customer_id"},
					{"Trading partner", "trading_partner_id"},
					{"Material", "product_id"},
					{"Amount in Local Curr", "amount"}
				}
				
			#End Region
			
			Else
				Throw ErrorHandler.LogWrite(si, New XFException($"There is no column mapping dictionary for table {tableName}"))
			End If
			
			Return columnMappingDict
				
		End Function
		
		#End Region
		
		#Region "Run Population Queries"
		
		Public Sub RunPopulationQueries(ByVal si As SessionInfo, ByVal tableName As String, ByVal folderPath As String, ByVal fileName As String)
			'Get migration queries
			Dim populationQueries As New List(Of String)
			'Control table name
			
			#Region "Raw Profit Centers"
			
			If tableName = "XFC_MAIN_RAW_ProfitCenters" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_ProfitCenters AS target
					USING (
					    SELECT id, description, company_id
						FROM XFC_MAIN_RAW_ProfitCenters
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        description = source.description,
					        company_id = source.company_id
					WHEN NOT MATCHED THEN
					    INSERT (id, description, company_id)
					    VALUES (source.id, source.description, source.company_id);
					")
				
			#End Region
			
			#Region "Raw Cost Centers"
			
			Else If tableName = "XFC_MAIN_RAW_CostCenters" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_CostCenters AS target
					USING (
					    SELECT DISTINCT id, COALESCE(description, 'No description') AS description, class_id, profit_center_id, start_date, end_date,
							CASE
								WHEN is_blocked = 'NO' THEN 0
								ELSE 1
							END AS is_blocked
						FROM XFC_MAIN_RAW_CostCenters
					) AS source
					ON target.id = source.id
						AND target.start_date = source.start_date
					WHEN MATCHED THEN
					    UPDATE SET
					        description = source.description,
					        class_id = source.class_id,
					        profit_center_id = source.profit_center_id,
					        end_date = source.end_date,
					        is_blocked = source.is_blocked
					WHEN NOT MATCHED THEN
					    INSERT (id, description, class_id, profit_center_id, start_date, end_date, is_blocked)
					    VALUES (source.id, source.description, source.class_id, source.profit_center_id, source.start_date, source.end_date, source.is_blocked);
				
					UPDATE cc1
					SET cc1.end_date = DATEADD(DAY, -1, cc2.start_date)
					FROM XFC_MAIN_MASTER_CostCenters cc1
					INNER JOIN XFC_MAIN_MASTER_CostCenters cc2
					ON cc1.id = cc2.id
						AND cc1.start_date < cc2.start_date
						AND cc1.end_date > cc2.start_date;
					")
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_CostCenterClasses AS target
					USING (
					    SELECT DISTINCT class_id AS id, class_description AS description
						FROM XFC_MAIN_RAW_CostCenters
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        description = source.description
					WHEN NOT MATCHED THEN
					    INSERT (id, description)
					    VALUES (source.id, source.description);
					")
				
			#End Region
			
			#Region "Raw Work Centers"
			
			Else If tableName = "XFC_MAIN_RAW_WorkCenters" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_WorkCenters AS target
					USING (
					    SELECT id, cost_center_id, start_date, end_date
						FROM XFC_MAIN_RAW_WorkCenters
						WHERE cost_center_id IS NOT NULL AND cost_center_id <> ''
					) AS source
					ON target.id = source.id
					AND target.cost_center_id = source.cost_center_id
					AND target.start_date = source.start_date
					WHEN MATCHED THEN
					    UPDATE SET
					        end_date = source.end_date
					WHEN NOT MATCHED THEN
					    INSERT (id, cost_center_id, start_date, end_date)
					    VALUES (source.id, source.cost_center_id, source.start_date, source.end_date);
				
					UPDATE wc1
					SET wc1.end_date = DATEADD(DAY, -1, wc2.start_date)
					FROM XFC_MAIN_MASTER_WorkCenters wc1
					INNER JOIN XFC_MAIN_MASTER_WorkCenters wc2
					ON wc1.id = wc2.id
						AND wc1.start_date < wc2.start_date
						AND wc1.end_date > wc2.start_date;
					")
				
			#End Region
			
			#Region "Raw Accounts"
			
			Else If tableName = "XFC_MAIN_RAW_Accounts" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_Accounts AS target
					USING (
					    SELECT DISTINCT name, description
						FROM XFC_MAIN_RAW_Accounts
					) AS source
					ON target.name = source.name
					WHEN MATCHED THEN
					    UPDATE SET
					        description = source.description
					WHEN NOT MATCHED THEN
					    INSERT (name, description)
					    VALUES (source.name, source.description);
					")
				populationQueries.Add("
					TRUNCATE TABLE XFC_MAIN_MASTER_AccountFlows;
				
					INSERT INTO XFC_MAIN_MASTER_AccountFlows (account_name, flow)
				    SELECT DISTINCT name AS account_name, flow
					FROM XFC_MAIN_RAW_Accounts
					WHERE flow IS NOT NULL AND flow <> '';
					")
				
			#End Region
			
			#Region "Raw Products"
			
			Else If tableName = "XFC_MAIN_RAW_Products" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_Products AS target
					USING (
					    SELECT DISTINCT id, description, type_id
						FROM XFC_MAIN_RAW_Products
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        description = source.description,
					        type_id = source.type_id
					WHEN NOT MATCHED THEN
					    INSERT (id, description, type_id)
					    VALUES (source.id, source.description, source.type_id);
					")
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_ProductTypes AS target
					USING (
					    SELECT DISTINCT type_id AS id, type_description AS description
						FROM XFC_MAIN_RAW_Products
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        description = source.description
					WHEN NOT MATCHED THEN
					    INSERT (id, description)
					    VALUES (source.id, source.description);
					")
				
			#End Region
			
			#Region "Raw Business Partners"
			
			Else If tableName = "XFC_MAIN_RAW_BusinessPartners" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_BusinessPartners AS target
					USING (
					    SELECT DISTINCT id, name, country, 'business' AS type
						FROM XFC_MAIN_RAW_BusinessPartners
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        name = source.name,
					        country = source.country
					WHEN NOT MATCHED THEN
					    INSERT (id, name, country, type)
					    VALUES (source.id, source.name, source.country, source.type);
					")
				
			#End Region
			
			#Region "Raw Trading Partners"
			
			Else If tableName = "XFC_MAIN_RAW_TradingPartners" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_BusinessPartners AS target
					USING (
					    SELECT DISTINCT id, name, 'trading' AS type
						FROM XFC_MAIN_RAW_TradingPartners
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        name = source.name
					WHEN NOT MATCHED THEN
					    INSERT (id, name, type)
					    VALUES (source.id, source.name, source.type);
					")
				
			#End Region
			
			#Region "Raw Account BS to Conso"
			
			Else If tableName = "XFC_MAIN_RAW_AccountBSToConso" Then
				populationQueries.Add("
					DELETE FROM XFC_MAIN_MASTER_AccountRubrics
					WHERE account_name NOT LIKE '6%' AND account_name NOT LIKE '7%';
				
					INSERT INTO XFC_MAIN_MASTER_AccountRubrics (account_name, conso_rubric, cost_center_class_id)
					SELECT DISTINCT account_name, rubric AS conso_rubric, 'none' AS cost_center_class_id
					FROM XFC_MAIN_RAW_AccountBSToConso
					WHERE rubric IS NOT NULL AND rubric NOT IN ('N/A', '', '0');
					")
				BRApi.Utilities.QueueDataMgmtSequence(
					si,
					BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "AdminDataManagement"),
					"AdminDataManagement_UpdateTransformationRules",
					New Dictionary(Of String, String) From {{"p_table_name", tableName}}
				)
				
			#End Region
			
			#Region "Raw Account PnL to Conso"
			
			Else If tableName = "XFC_MAIN_RAW_AccountPnLToConso" Then
				populationQueries.Add("
					DELETE FROM XFC_MAIN_MASTER_AccountRubrics
					WHERE account_name LIKE '6%' OR account_name LIKE '7%';
				
					INSERT INTO XFC_MAIN_MASTER_AccountRubrics (account_name, conso_rubric, cost_center_class_id)
				    SELECT DISTINCT account_name, conso_rubric, cost_center_class_id
					FROM (
						SELECT *
						FROM (
							SELECT *, ROW_NUMBER() OVER (PARTITION BY account_name ORDER BY account_name) AS rn
							FROM XFC_MAIN_RAW_AccountPnLToConso
						) AS subquery
						WHERE rn = 1
					) AS src
					UNPIVOT (
						conso_rubric FOR cost_center_class_id IN (
							[A], [B], [C], [D], [E], [F], [G], [H], [I], [J], [K], [L], [M], [N], [O], [P], [Q],
							[0], [R], [S], [T], [U], [V], [W], [X], [Y], [1], [2], [Z], [PD], [PC], [none]
						)
					) AS apnlc
					WHERE conso_rubric IS NOT NULL AND conso_rubric NOT IN ('N/A', '', '0');
					")
				
			#End Region
			
			#Region "Raw Accounts Old To New"
			
			Else If tableName = "XFC_MAIN_RAW_AccountsOldToNew" Then
				populationQueries.Add("
					DELETE FROM XFC_MAIN_MASTER_AccountsOldToNew;
				
					INSERT INTO XFC_MAIN_MASTER_AccountsOldToNew (name, name_old)
					SELECT name, name_old
					FROM (
					    SELECT name, name_old, ROW_NUMBER() OVER (PARTITION BY name ORDER BY name_old) AS rn
						FROM XFC_MAIN_RAW_AccountsOldToNew
						WHERE name IS NOT NULL AND name NOT IN ('N/A', '')
					) AS subquery
					WHERE rn = 1;
					")
				
			#End Region
			
			#Region "Raw Cost Center Class To Rubric"
			
			Else If tableName = "XFC_MAIN_RAW_CostCenterClassToRubric" Then
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_CostCenterClasses AS target
					USING (
					    SELECT DISTINCT id, depreciation_account, asset_account
						FROM XFC_MAIN_RAW_CostCenterClassToRubric
						WHERE (depreciation_account IS NOT NULL AND depreciation_account <> '')
							OR (asset_account IS NOT NULL AND asset_account <> '')
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        depreciation_account = source.depreciation_account,
					        asset_account = source.asset_account;
					")
				
			#End Region
			
			#Region "Master Flow Mappings"
			
			Else If tableName = "XFC_MAIN_MASTER_FlowMappings" Then
				BRApi.Utilities.QueueDataMgmtSequence(
					si,
					BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "AdminDataManagement"),
					"AdminDataManagement_UpdateTransformationRules",
					New Dictionary(Of String, String) From {{"p_table_name", tableName}}
				)
				
			#End Region
			
			#Region "Master Cost Centers Old to OneStream Member"
			
			Else If tableName = "XFC_MAIN_MASTER_CostCentersOldToOneStreamMember" Then
				BRApi.Utilities.QueueDataMgmtSequence(
					si,
					BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "AdminDataManagement"),
					"AdminDataManagement_UpdateTransformationRules",
					New Dictionary(Of String, String) From {{"p_table_name", tableName}}
				)
				
			#End Region
			
			#Region "Raw Customers To UD1 IC"
			
			Else If tableName = "XFC_MAIN_RAW_CustomersToUD1IC" Then
				populationQueries.Add("
					UPDATE XFC_MAIN_MASTER_BusinessPartners
					SET customer_member_name = '',
						ic_member_name = ''
					
					MERGE INTO XFC_MAIN_MASTER_BusinessPartners AS target
					USING (
					    SELECT DISTINCT id, customer_member_name, ic_member_name
						FROM XFC_MAIN_RAW_CustomersToUD1IC
						WHERE (customer_member_name IS NOT NULL AND customer_member_name <> '')
							OR (ic_member_name IS NOT NULL AND ic_member_name <> '')
					) AS source
					ON target.id = source.id
					WHEN MATCHED THEN
					    UPDATE SET
					        customer_member_name = source.customer_member_name,
					        ic_member_name = source.ic_member_name;
					")
				BRApi.Utilities.QueueDataMgmtSequence(
					si,
					BRApi.Dashboards.Workspaces.GetWorkspaceIDFromName(si, False, "AdminDataManagement"),
					"AdminDataManagement_UpdateTransformationRules",
					New Dictionary(Of String, String) From {{"p_table_name", tableName}}
				)
				
			#End Region
			
			#Region "Raw PnL Transactions"
			
			Else If tableName.Contains("XFC_MAIN_RAW_PnLTransactions") Then
				populationQueries.Add("
					DELETE FROM XFC_MAIN_FACT_PnLTransactions
					WHERE year IN (
						SELECT DISTINCT year
						FROM XFC_MAIN_RAW_PnLTransactions
					) AND month IN (
						SELECT DISTINCT month
						FROM XFC_MAIN_RAW_PnLTransactions
					) AND company_id IN (
						SELECT DISTINCT company_id
						FROM XFC_MAIN_RAW_PnLTransactions
					);
				
					INSERT INTO XFC_MAIN_FACT_PnLTransactions (
						year, month, company_id, account_name, cost_center_id, profit_center_id, business_partner_id, order_id, product_id, amount
					)
					SELECT year, month, company_id, account_name, cost_center_id, profit_center_id, business_partner_id, order_id, product_id,
						SUM(amount) AS amount
					FROM (
						SELECT
							year, month, company_id, account_name,
							COALESCE(cost_center_id, 'none') AS cost_center_id,
							COALESCE(profit_center_id, 'none') AS profit_center_id,
							COALESCE(supplier_id, customer_id, trading_partner_id, 'none') AS business_partner_id,
							COALESCE(order_id, 'none') AS order_id,
							COALESCE(product_id, 'none') AS product_id,
							amount
						FROM XFC_MAIN_RAW_PnLTransactions
						WHERE account_name LIKE '6%' OR account_name LIKE '7%'
					) AS subquery
					GROUP BY year, month, company_id, account_name, cost_center_id, profit_center_id, business_partner_id, order_id, product_id;
				")
				populationQueries.Add("
					MERGE INTO XFC_MAIN_MASTER_PnLHasChanged AS target
					USING (
					   	SELECT DISTINCT year, month, company_id
						FROM XFC_MAIN_RAW_PnLTransactions
					) AS source
					ON target.year = source.year
					AND target.month = source.month
					AND target.company_id = source.company_id
					WHEN MATCHED THEN
					    UPDATE SET
					        fpna = 1,
					        pct = 1
					WHEN NOT MATCHED THEN
					INSERT (year, month, company_id, fpna, pct)
					VALUES (source.year, source.month, source.company_id, 1, 1);
				")
				
				Dim uti_sharedqueries As New OneStream.BusinessRule.Extender.UTI_SharedQueries.shared_queries		
				uti_sharedqueries.update_Log(si, "Actual", "R0671", "XFC_MAIN_FACT_PnLTransactions","Import")
											
			#End Region
			

			End If
			
			Using dbConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
				'Populate tables and update table info
				For Each query In populationQueries
					BRApi.Database.ExecuteSql(dbConn, query, False)
				Next
				Dim dbParamInfos As New List(Of DbParamInfo) From {
					New DbParamInfo("paramTable", tableName),
					New DbParamInfo("paramUsername", si.UserName),
					New DbParamInfo("paramFolderPath", folderPath),
					New DbParamInfo("paramFileName", fileName)
				}
				BRApi.Database.ExecuteSql(
					dbConn,
					"
					MERGE INTO XFC_MAIN_MASTER_TableImportInfo AS target
					USING (
					    SELECT
					        @paramTable AS table_name,
					        @paramUsername AS username,
							@paramFolderPath AS folder_path,
							@paramFileName AS file_name,
					        GETUTCDATE() AS last_import_date
					) AS source
					ON target.table_name = source.table_name
					WHEN MATCHED THEN
					    UPDATE SET
					        username = source.username,
					        last_import_date = source.last_import_date,
							folder_path = source.folder_path,
							file_name = source.file_name
					WHEN NOT MATCHED THEN
					    INSERT (table_name, username, last_import_date, folder_path, file_name)
					    VALUES (source.table_name, source.username, source.last_import_date, source.folder_path, source.file_name);
					",
					dbParamInfos,
					False
				)
			End Using
		End Sub
		
		Private Function GetPopulationQueries(ByVal si As SessionInfo, ByVal tables As List(Of Object)) As List(Of String)
			'Declare list of queries
			Dim queries As New List(Of String)
			
			'Loop through all the tables to get the queries
			For Each table In tables
				queries.Add(table.GetPopulationQuery(si, "up"))
			Next
			
            Return queries
		End Function
		
		#End Region
		
		#End Region
		
	End Class
End Namespace
