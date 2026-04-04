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

Namespace OneStream.BusinessRule.Extender.UTI_ExportToDB
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				
				Select Case args.FunctionType
					
					Case Is = ExtenderFunctionType.Unknown
						
					Case Is = ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep
						
						'Get parameters
						Dim ParamForecastYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Actual_Year")
						Dim ParamBudgetYear As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Budget_Year")
						Dim ParamForecastMonth As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month")
						Dim sParamSQLFinalSales As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_SQL_FinalSales")
						Dim ParamScenario As String = args.NameValuePairs("prm_Scenario_Elegible")
						Dim ParamVersion As String = args.NameValuePairs("prm_Version_Elegible")
						Dim ParamScenarioYear As String = If(ParamScenario = "Forecast", ParamForecastYear, ParamBudgetYear)
						Dim dimTypeId As Integer = BRApi.Finance.Dim.GetDimPk(si, "Scenarios").DimTypeId
						Dim scenarioId As Integer = BRApi.Finance.Members.GetMemberId(si, dimTypeId, ParamScenario)
						Dim scenarioTypeId As Integer = BRApi.Finance.Scenario.GetScenarioType(si, scenarioId).Id
						
						'Create connections to both internal and external databases
						Using dbAppConn As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
						Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, dbLocation.External, "SICProSQL")
						
							'Declare relevant variables
							Dim selectQuery As String
							Dim insertQuery As String
							Dim deleteQuery As String
							Dim insertQueries As List(Of String)
							Dim dt As DataTable
							
							'Clear the external tables
							deleteQuery = "
								TRUNCATE TABLE XFC_DailySalesExport;
								TRUNCATE TABLE XFC_PLExport_SAP;
								TRUNCATE TABLE XFC_PLExport_BI;
							"
							
							BRApi.Database.ExecuteSql(dbConn, deleteQuery, False)
							
							'Get all the brands for the year in daily sales
							selectQuery = $"
							SELECT DISTINCT brand
							FROM XFC_DailySalesVersions
							WHERE
								year = {ParamScenarioYear}
								AND scenario = '{ParamScenario}'
								AND version = '{ParamVersion}'
							"
							dt = BRApi.Database.ExecuteSql(dbAppConn, selectQuery, False)
							
							If dt.Rows.Count > 0 Then
								'Loop through each brand to make inserts
								For Each row As DataRow In dt.Rows
									'Get brand name
									Dim ParamBrand As String = row("brand").Replace("'","''")
									'Get Daily Sales Data
									
									selectQuery = $"
										DECLARE @Year INT = {ParamForecastYear};
									
										WITH MonthlyCebeProportions AS (
										    SELECT
										        rs.unit_code AS cte_unit_code,
										        ch.desc_channel1 AS cte_channel1,
										        ch.cod_channel1 AS cte_channel1_code,
										        ch.desc_channel2 AS cte_channel2,
										        ch.cod_channel2 AS cte_channel2_code,
												-- Calculate proportion by unit
												ROUND(
											        SUM(rs.sales_net)
											        / NULLIF(SUM(SUM(rs.sales_net)) OVER (PARTITION BY rs.unit_code, ch.desc_channel1), 0),
													2
												) AS channel1_sales_proportion,
										        -- Calculate proportion by brand
												ROUND(
													SUM(SUM(rs.sales_net)) OVER (
											            PARTITION BY
											                CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
											                ch.desc_channel2
											            )
											        / NULLIF(SUM(SUM(rs.sales_net)) OVER (
											            PARTITION BY
											                CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
											                ch.desc_channel1
											            ), 0),
													2
												) AS channel1_sales_proportion_brand
										    FROM (
												SELECT *
												FROM XFC_RawSales WITH(NOLOCK)
												WHERE sales_net <> 0
													AND YEAR(date) = @Year
											) AS rs
										    INNER JOIN XFC_ChannelHierarchy ch ON rs.cod_channel3 = ch.cod_channel3
										    INNER JOIN XFC_CEBES c ON rs.unit_code = c.cebe
										    WHERE
										        ch.desc_channel1 = 'Delivery'
										        AND (
										            (c.brand IN ('SDH', 'Genéricos Grupo') AND c.sales_brand = '{ParamBrand}')
										            OR
										            (c.brand NOT IN ('SDH', 'Genéricos Grupo') AND c.brand = '{ParamBrand}')
										        )
										    GROUP BY
										        CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
										        rs.unit_code,
										        ch.desc_channel1,
										        ch.cod_channel1,
										        ch.desc_channel2,
										        ch.cod_channel2
										)
										
									    SELECT
									        ds.unit_code AS unidad,
											FORMAT(ds.date, 'yyyyMMdd') AS fecha,		
										
									        CASE
												-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
									            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN mcp.cte_channel2_code
									            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.cte_channel2_code IS NOT NULL THEN mcp2.cte_channel2_code
									            WHEN ds.channel = 'Sala' THEN 1
												WHEN ds.channel = 'Take Away' THEN 4
												ELSE 3
									        END AS salappto,
									        
									        CASE
												-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
									            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN
									                ({sParamSQLFinalSales}) * mcp.channel1_sales_proportion
									            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.channel1_sales_proportion_brand IS NOT NULL THEN
									                ({sParamSQLFinalSales}) * mcp2.channel1_sales_proportion_brand
									            ELSE
									                {sParamSQLFinalSales}
									        END AS vtabruta,
										
									        CASE
												-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
									            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN
									                ({sParamSQLFinalSales}) * mcp.channel1_sales_proportion
									            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.channel1_sales_proportion_brand IS NOT NULL THEN
									                ({sParamSQLFinalSales}) * mcp2.channel1_sales_proportion_brand
									            ELSE
									                {sParamSQLFinalSales}
									        END AS vtaneta,					
										
									        ROUND(
												CASE
													-- In delivery customers, when proportion for unit exists, use unit proportion, else use brand proportion
										            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN
										                (
										                    ISNULL(ds.customers,0) 
										                    + ISNULL(ds.rem_customers_adj,0) 
										                    + ISNULL(ds.daily_customers_adj, 0)
										                    + ISNULL(ds.week_customers_prom_adj,0) 
										                    + ISNULL(ds.week_customers_camp_adj,0) 
										                    + ISNULL(ds.week_customers_growth_adj, 0)
										                    + ISNULL(ds.week_customers_remgrowth_adj,0) 
										                    + ISNULL(ds.week_customers_gen1_adj,0) 
										                    + ISNULL(ds.week_customers_gen2_adj,0) 
										                    + ISNULL(ds.week_customers_trend_adj, 0) 
										                    + ISNULL(ds.month_customers_adj, 0)
										                )
										                * mcp.channel1_sales_proportion
										            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.channel1_sales_proportion_brand IS NOT NULL THEN
										                (
										                    ISNULL(ds.customers,0) 
										                    + ISNULL(ds.rem_customers_adj,0) 
										                    + ISNULL(ds.daily_customers_adj, 0)
										                    + ISNULL(ds.week_customers_prom_adj,0) 
										                    + ISNULL(ds.week_customers_camp_adj,0) 
										                    + ISNULL(ds.week_customers_growth_adj, 0)
										                    + ISNULL(ds.week_customers_remgrowth_adj,0) 
										                    + ISNULL(ds.week_customers_gen1_adj,0) 
										                    + ISNULL(ds.week_customers_gen2_adj,0) 
										                    + ISNULL(ds.week_customers_trend_adj, 0) 
										                    + ISNULL(ds.month_customers_adj, 0)
										                )
										                * mcp2.channel1_sales_proportion_brand
										            ELSE
										                ISNULL(ds.customers,0) 
										                + ISNULL(ds.rem_customers_adj,0) 
										                + ISNULL(ds.daily_customers_adj, 0)
										                + ISNULL(ds.week_customers_prom_adj,0) 
										                + ISNULL(ds.week_customers_camp_adj,0) 
										                + ISNULL(ds.week_customers_growth_adj, 0)
										                + ISNULL(ds.week_customers_remgrowth_adj,0) 
										                + ISNULL(ds.week_customers_gen1_adj,0) 
										                + ISNULL(ds.week_customers_gen2_adj,0) 
										                + ISNULL(ds.week_customers_trend_adj, 0) 
										                + ISNULL(ds.month_customers_adj, 0)
										        END,
												0
										) AS clientes
										
									    FROM XFC_DailySalesVersions ds
									    INNER JOIN XFC_ComparativeDates cd
									        ON cd.date = ds.date
										-- Join for unit proportions when proportion for that unit exists
									    LEFT JOIN MonthlyCebeProportions mcp
									        ON mcp.cte_unit_code = ds.unit_code
									        AND mcp.cte_channel1 = ds.channel
										-- Join for brand proportions when proportion for that unit doesn't exist
									    LEFT JOIN (
									        SELECT DISTINCT cte_channel1, cte_channel1_code, cte_channel2, cte_channel2_code, channel1_sales_proportion_brand
									        FROM MonthlyCebeProportions
									    ) mcp2
									        ON mcp2.cte_channel1 = ds.channel
									        AND mcp.cte_channel2 IS NULL
									    INNER JOIN XFC_CEBES ce 
									        ON ds.unit_code = ce.cebe
									    WHERE ds.channel <> ''
										AND ds.channel IS NOT NULL
										AND ds.scenario = '{ParamScenario}'
									    AND ds.year = {ParamScenarioYear}
								        AND ds.brand = '{ParamBrand}'
								        AND YEAR(ds.date) = {ParamScenarioYear}
										AND ds.version = '{ParamVersion}'
										"
									dt = BRApi.Database.ExecuteSql(dbAppConn, selectQuery, False)
									
									insertQueries = Me.GenerateInsertSQLList(dt, "XFC_DailySalesExport")
									
									For Each insertQuery In insertQueries
									
										'Export datatable to external db
										BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
										
									Next
								Next
							End If
							
							'Declare scenario version variable
							Dim scenarioVersion As String = $"{ParamScenario}_V{ParamVersion}"
							'Set literal parameter for scenario and time
							BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_ExportToDB_Scenario", scenarioVersion)
							BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, False, "prm_ExportToDB_Time", ParamScenarioYear)
							
							'Create CSV file with the PL data
							BRApi.Utilities.ExecuteDataMgmtSequence(si, Guid.Empty, "ADMIN_CreateCSVExportFile", Nothing)
									
							'Get file
							Dim fileName As String = "PLExport.csv"
							Dim sourceFolder As String = BRApi.Utilities.GetFileShareFolder(si, FileShareFolderTypes.ApplicationRoot, Nothing) &
								$"\Production\DataManagement\Export\{si.UserName}\CSV_Exports"
							Dim fullPath = sourceFolder & "\" & FileName
							
							'Declare the export data table
							Dim exportDt As New DataTable
							
							'Read the file and build the export data table
							Using reader As New FileIO.TextFieldParser(fullPath)
								reader.TextFieldType = FileIO.FieldType.Delimited
								reader.SetDelimiters(",")
								
								Dim headers = reader.ReadFields()
								For Each colName In headers
									exportDt.Columns.Add(colName)
								Next
								
								While Not reader.EndOfData
									Dim row = reader.ReadFields()
									Dim content As New List(Of Object)
									For Each item In Row
										content.Add(item)
									Next
									exportDt.Rows.Add(content.ToArray)
								End While
							End Using
							
							'Delete some columns of the data table
							Dim exportView As New DataView(exportDt)
							Dim cleanExportDt As DataTable = exportView.ToTable(False, "Entity", "Time", "Account", "Amount")
							
							'Rename column names to match the export sql table
							cleanExportDt.Columns("Entity").ColumnName = "unit_code"
							cleanExportDt.Columns("Time").ColumnName = "period"
							cleanExportDt.Columns("Account").ColumnName = "accounting_account"
							cleanExportDt.Columns("Amount").ColumnName = "accumulated_balance"
							
							'Modify period column to match the database format "YYYY0MM" and map the accounts
							For Each row As DataRow In cleanExportDt.Rows
								row("period") = row("period").Split("M")(0) * 1000 + row("period").Split("M")(1)
								row("accounting_account") = BRApi.Utilities.TransformText(si, row("accounting_account"), "PLANNING_Export_Accounts", True)
							Next
							
							'Generate insert queries to external database
							insertQueries = Me.GenerateInsertSQLList(cleanExportDt, "XFC_PLExport_BI")
				
							For Each insertQuery In insertQueries
								'Export datatable to external db
								BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
							Next
							
							'Insert pivoted data for SAP
							insertQuery = $"
							INSERT INTO XFC_PLExport_SAP (
								AÑO,
								unidad,
								NUM_CUENTA,
								MES1,
								MES2,
								MES3,
								MES4,
								MES5,
								MES6,
								MES7,
								MES8,
								MES9,
								MES10,
								MES11,
								MES12
							)
							SELECT 
							    year AS AÑO,  -- Extract year from the period (assuming YYYYMM format)
							    unit_code AS unidad,
							    accounting_account AS NUM_CUENTA,
							    ISNULL([01], 0) AS MES1,  -- January
							    ISNULL([02], 0) AS MES2,  -- February
							    ISNULL([03], 0) AS MES3,  -- March
							    ISNULL([04], 0) AS MES4,  -- April
							    ISNULL([05], 0) AS MES5,  -- May
							    ISNULL([06], 0) AS MES6,  -- June
							    ISNULL([07], 0) AS MES7,  -- July
							    ISNULL([08], 0) AS MES8,  -- August
							    ISNULL([09], 0) AS MES9,  -- September
							    ISNULL([10], 0) AS MES10, -- October
							    ISNULL([11], 0) AS MES11, -- November
							    ISNULL([12], 0) AS MES12  -- December
							FROM
							(
							    SELECT 
							        unit_code, 
							        LEFT(period, 4) AS year,       -- Extract year (YYYY)
							        RIGHT(period, 2) AS month,     -- Extract month (MM)
							        accounting_account,
							        accumulated_balance
							    FROM XFC_PLExport_BI
							) AS SourceTable
							PIVOT
							(
							    SUM(accumulated_balance)
							    FOR month IN ([01], [02], [03], [04], [05], [06], [07], [08], [09], [10], [11], [12])  -- Fixed months
							) AS PivotTable
							ORDER BY AÑO, unidad, NUM_CUENTA;
							"
						BRApi.Database.ExecuteSql(dbConn, insertQuery, False)
						End Using
						End Using
					End Select
				Return Nothing
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
		End Function
		
		#Region "Helpers"
		
		#Region "Get First Five Rows"
		
		Function GetFirstFiveRows(ByVal dt As DataTable) As String
	        Dim sb As New StringBuilder()
	
	        ' Check if the DataTable has any rows
	        If dt.Rows.Count = 0 Then
	            Return "DataTable is empty."
	        End If
	
	        ' Append the headers
	        For Each column As DataColumn In dt.Columns
	            sb.Append(column.ColumnName & vbTab)
	        Next
	        sb.AppendLine()
	
	        ' Append the first 5 rows or less if there are fewer rows in the DataTable
	        For i As Integer = 0 To Math.Min(4, dt.Rows.Count - 1)
	            For Each column As DataColumn In dt.Columns
	                sb.Append(dt.Rows(i)(column).ToString() & vbTab)
	            Next
	            sb.AppendLine()
	        Next
	
	        ' Return the built string
	        Return sb.ToString()
	    End Function
		
		#End Region
		
		#Region "Generate Insert SQL List"
		
		Public Function GenerateInsertSQLList(ByVal dataTable As DataTable, ByVal tableName As String) As List(Of String)
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
		                rowValues.Add(value.ToString().Replace(",", "."))
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
		
		#End Region

	End Class
End Namespace