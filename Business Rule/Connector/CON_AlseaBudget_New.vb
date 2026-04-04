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

Namespace OneStream.BusinessRule.Connector.CON_AlseaBudget_New
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As Object
			Try
				'Get the query information
                'Dim connectionString As String = GetConnectionString(si, globals, api)
				
				'Get the Field name list or load the data    
                Select Case args.ActionType
                    Case Is = ConnectorActionTypes.GetFieldList
						
                        'Return Field Name List
						'In case we use OneStream tables
						Dim fieldList As New List(Of String)
						fieldList.Add("Scenario")
						fieldList.Add("Entity")
						fieldList.Add("Period")
						fieldList.Add("Channel")
						fieldList.Add("Sales")
						fieldList.Add("Customers")
						
						Return fieldList
						
						'In case we use SIC
'						-------------------------------------------------------------------------------------------------------------------------
'                        Dim fieldListSQL As String = GetFieldListSQL(si, globals, api)
'                        Return api.Parser.GetFieldNameListForSQLQuery(si, DbProviderType.OLEDB, connectionString, True, fieldListSQL, False)
'						-------------------------------------------------------------------------------------------------------------------------
                        
                    Case Is = ConnectorActionTypes.GetData
						
                        'Process Data
						
						'In case we use OneStream Tables
'						-------------------------------------------------------------------------------------------------------------------------

						'Get Workflow information
						Dim wfScenario As String = ScenarioDimHelper.GetNameFromID(si, api.WorkflowUnitPk.ScenarioKey)
						Dim wfTime As String = BRApi.Finance.Time.GetNameFromId(si, api.WorkflowUnitPk.TimeKey)
						Dim oTime As TimeMemberSubComponents = BRApi.Finance.Time.GetSubComponentsFromName(si, wfTime)
						Dim wfYear As Integer = oTime.year
						Dim wfYearNext As Integer = wfYear + 1
						Dim wfYearPrev As Integer = wfYear - 1
						Dim dt As DataTable
						
						'Get brand and forecast month information and sales query
						Dim ParamBrand As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_BrandFilter_Value")
						Dim ParamForecastMonth As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_Forecast_Month")
						Dim sParamSQLFinalSales As String = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, False, "prm_SQL_FinalSales")
						
						Dim selectQuery As String = $"
							WITH FilteredDailySales AS (
								SELECT *
								FROM XFC_DailySales WITH(NOLOCK)
								WHERE channel = 'Delivery'
									AND (
							            (
							                scenario = '{wfScenario}'
							                AND year = {wfYear}
							            )
							            OR
							            (
							                scenario = 'Actual'
							                AND MONTH(date) <= {ParamForecastMonth}
							            )
								    )
							        AND brand = '{ParamBrand}'
							        AND YEAR(date) = {wfYear}
							), FilteredComparativeCEBESAux AS (
								SELECT *
								FROM XFC_ComparativeCEBESAux WITH(NOLOCK)
								WHERE year = {wfYear}
									AND desc_annualcomparability IN ('Comparables','Cerradas', 'Reformas', 'Reformas Año Anterior')
							), MonthlyCebeProportions AS (
							    SELECT
							        YEAR(rs.date) AS cte_year,
							        MONTH(rs.date) AS cte_month,
							        rs.unit_code AS cte_unit_code,
							        ch.desc_channel1 AS cte_channel1,
							        ch.cod_channel1 AS cte_channel1_code,
							        ch.desc_channel2 AS cte_channel2,
							        ch.cod_channel2 AS cte_channel2_code,
									-- Calculate proportion by unit
							        ROUND(
										SUM(rs.sales_net)
								        / NULLIF(SUM(SUM(rs.sales_net)) OVER (PARTITION BY YEAR(rs.date), MONTH(rs.date), rs.unit_code, ch.desc_channel1), 0),
										2
									) AS channel1_sales_proportion,
							        -- Calculate proportion by brand
									ROUND(
										SUM(SUM(rs.sales_net)) OVER (
								            PARTITION BY
								                YEAR(rs.date),
								                MONTH(rs.date),
								                CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
								                ch.desc_channel2
								            )
								        / NULLIF(SUM(SUM(rs.sales_net)) OVER (
								            PARTITION BY
								                YEAR(rs.date),
								                MONTH(rs.date),
								                CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
								                ch.desc_channel1
								            ), 0),
										2
									) AS channel1_sales_proportion_brand
							    FROM (
									SELECT *
									FROM XFC_RawSales WITH(NOLOCK)
									WHERE YEAR(date) BETWEEN {wfYear - 2} AND {wfYear}
										AND sales_net <> 0
								) rs
							    INNER JOIN (
									SELECT *
									FROM XFC_ChannelHierarchy WITH(NOLOCK)
									WHERE desc_channel1 = 'Delivery'
								) ch ON rs.cod_channel3 = ch.cod_channel3
							    INNER JOIN (
									SELECT *
									FROM XFC_CEBES WITH(NOLOCK)
									WHERE
										(brand IN ('SDH', 'Genéricos Grupo') AND sales_brand = '{ParamBrand}')
							            OR
							            (brand NOT IN ('SDH', 'Genéricos Grupo') AND brand = '{ParamBrand}')
								) c ON rs.unit_code = c.cebe
							    GROUP BY
							        YEAR(rs.date),
							        MONTH(rs.date),
							        CASE WHEN c.brand IN ('SDH', 'Genéricos Grupo') THEN c.sales_brand ELSE c.brand END,
							        rs.unit_code,
							        ch.desc_channel1,
							        ch.cod_channel1,
							        ch.desc_channel2,
							        ch.cod_channel2
							), ProcessedSales AS (
							    SELECT
									ds.scenario AS Scenario,
							        ds.unit_code AS Entity,
									YEAR(ds.date) * 100 + MONTH(ds.date) AS Period,		
								
							        CASE
										-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN mcp.cte_channel2
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.cte_channel2_code IS NOT NULL THEN mcp2.cte_channel2
							            WHEN ds.channel = 'Sala' THEN 'Sala'
										WHEN ds.channel = 'Take Away' THEN 'Take Away'
										ELSE 'Delivery'
							        END AS Channel,
							        
							        CASE
										-- In delivery sales, when proportion for unit exists, use unit proportion, else use brand proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NOT NULL THEN
							                ({sParamSQLFinalSales}) * mcp.channel1_sales_proportion
							            WHEN ds.channel = 'Delivery' AND mcp.cte_channel2 IS NULL AND mcp2.channel1_sales_proportion_brand IS NOT NULL THEN
							                ({sParamSQLFinalSales}) * mcp2.channel1_sales_proportion_brand
							            ELSE
							                {sParamSQLFinalSales}
							        END AS Sales,						
								
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
							        END AS Customers
								
							    FROM FilteredDailySales ds
							    INNER JOIN XFC_ComparativeDates cd WITH(NOLOCK)
							        ON cd.date = ds.date
								-- Join for unit proportions when proportion for that unit exists
							    LEFT JOIN MonthlyCebeProportions mcp WITH(NOLOCK)
							        ON mcp.cte_unit_code = ds.unit_code
							        AND (
							            -- Join condition based on scenario
							            ('{wfScenario}' = 'Forecast' AND mcp.cte_year = YEAR(cd.date_comparable) AND mcp.cte_month = MONTH(cd.date_comparable))
							            OR ('{wfScenario}' = 'Budget' AND mcp.cte_year = YEAR(cd.date_comparable_2) AND mcp.cte_month = MONTH(cd.date_comparable_2))
							        )
							        AND mcp.cte_channel1 = ds.channel
								-- Join for brand proportions when proportion for that unit doesn't exist
							    LEFT JOIN (
							        SELECT DISTINCT cte_year, cte_month, cte_channel1, cte_channel1_code, cte_channel2, cte_channel2_code, channel1_sales_proportion_brand
							        FROM MonthlyCebeProportions WITH(NOLOCK)
							    ) mcp2
							        ON mcp2.cte_channel1 = ds.channel
							        AND (
							            -- Join condition based on scenario
							            ('{wfScenario}' = 'Forecast' AND mcp2.cte_year = YEAR(cd.date_comparable) AND mcp2.cte_month = MONTH(cd.date_comparable))
							            OR ('{wfScenario}' = 'Budget' AND mcp2.cte_year = YEAR(cd.date_comparable_2) AND mcp2.cte_month = MONTH(cd.date_comparable_2))
							        )
							        AND mcp.cte_channel2 IS NULL
							    INNER JOIN XFC_CEBES ce  WITH(NOLOCK)
							        ON ds.unit_code = ce.cebe
								INNER JOIN FilteredComparativeCEBESAux cca
									ON ds.unit_code = cca.cebe
							), ScenarioSales AS (
							    -- This CTE calculates scenario sales data
							    SELECT 
							        '{wfScenario}' As Scenario,
							        ds.unit_code AS Entity,
							        YEAR(ds.date) * 100 + MONTH(ds.date) AS Period,
							        ds.channel AS Channel,
							        SUM({sParamSQLFinalSales}) As [Sales],
							        SUM(
							            ISNULL(customers,0) 
							            + ISNULL(rem_customers_adj,0) 
							            + ISNULL(week_customers_prom_adj,0) 
							            + ISNULL(week_customers_camp_adj,0) 
										+ ISNULL(week_customers_growth_adj, 0)
							            + ISNULL(week_customers_remgrowth_adj,0) 
							            + ISNULL(week_customers_gen1_adj,0) 
							            + ISNULL(week_customers_gen2_adj,0) 
							            + ISNULL(week_customers_trend_adj, 0) 
							            + ISNULL(month_customers_adj, 0)
							        ) AS Customers
							    FROM FilteredDailySales ds
							    INNER JOIN XFC_CEBES ce WITH(NOLOCK)
							        ON ds.unit_code = ce.cebe
								LEFT JOIN FilteredComparativeCEBESAux cca
									ON ds.unit_code = cca.cebe
							    WHERE
									(
									    (
											ds.scenario = '{wfScenario}'
									    	AND ds.year = {wfYear}
								    		AND cca.cebe IS NOT NULL
										)
										OR
										(
											ds.scenario = 'Actual'
											AND MONTH(ds.date) <= {ParamForecastMonth}
										)
									)
							    GROUP BY
							        ds.scenario,
							        ds.unit_code,
							        YEAR(ds.date) * 100 + MONTH(ds.date),
							        ds.channel
							)
						
							SELECT
							    Scenario,
							    Entity,
							    Period,
							    Channel,
							    SUM(Sales) AS Sales,
							    SUM(Customers) AS Customers
							FROM ProcessedSales WITH(NOLOCK)
							GROUP BY 
								Scenario,
							    Entity,
							    Period,
							    Channel
						
							UNION ALL
							
							SELECT
							    Scenario,
							    Entity,
							    Period,
							    Channel,
							    Sales,
							    Customers
							FROM ScenarioSales WITH(NOLOCK)
							"
						
						Using dbcon As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
							dt = BRApi.Database.ExecuteSql(dbcon, selectQuery, False)
							api.Parser.ProcessDataTable(si, dt, False, api.ProcessInfo)
						
						End Using
'						-------------------------------------------------------------------------------------------------------------------------
						
						'In case we use SIC
'						-------------------------------------------------------------------------------------------------------------------------
'                        Dim sourceDataSQL As String = GetSourceDataSQL(si, globals, api)
'                        api.Parser.ProcessSQLQuery(si, DbProviderType.OLEDB, connectionString, True, sourceDataSQL, False, api.ProcessInfo)
'						-------------------------------------------------------------------------------------------------------------------------
						
                        Return Nothing
 
                    Case Is = ConnectorActionTypes.GetDrillBackTypes
						
                        'Return the list of Drill Types (Options) to present to the end user
                        Return Nothing 'Me.GetDrillBackTypeList(si, globals, api, args)
 
                    Case Is = ConnectorActionTypes.GetDrillBack
                        'Process the specific Drill-Back type
                        Return Nothing 'Me.GetDrillBack(si, globals, api, args, args.DrillBackType.DisplayType, connectionString)
				
                End Select
				
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function
		
		'Create a Connection string to the External Database
        Private Function GetConnectionString(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer) As String
		
            Try
                                
                'Connection String Method
                '-----------------------------------------------------------
'                Dim connection As New Text.StringBuilder'                
'                connection.Append("Provider=SQLOLEDB.1;")
'                connection.Append("Data Source=LocalHost\MSSQLSERVER2008;")
'                connection.Append("Initial Catalog=SampleData;")
'                connection.Append("Integrated Security=SSPI")                
'                Return connection.ToString
                
                'Named External Connection
                '-----------------------------------------------------------
                Return "Revenue Mgmt System"
                
                Catch ex As Exception
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
            End Try            
        End Function    
 
        'Create the field list SQL Statement
        Private Function GetFieldListSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, 
		ByVal api As Transformer) As String
            Try
				
                'Create the SQL Statement
                Dim sql As New Text.StringBuilder
                
                sql.Append("SELECT Top(1)")
                
                Return sql.ToString
                
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try    
			
        End Function
 
        'Create the data load SQL Statement
        Private Function GetSourceDataSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Transformer) As String
		
            Try
                'Create the SQL Statement
                Dim statement As New Text.StringBuilder
                Dim selectClause As New Text.StringBuilder
                Dim fromClause As New Text.StringBuilder
                Dim whereClause As New Text.StringBuilder
                Dim orderByClause As New Text.StringBuilder
                                
                selectClause.Append("SELECT ")
                
                fromClause.Append("FROM ")
                
                whereClause.Append("WHERE ")
 
                orderByClause.Append("ORDER BY ")               
                
                'Create the full SQL Statement
                statement.Append(selectClause.ToString)
                statement.Append(fromClause.ToString)
                statement.Append(whereClause.ToString)
                statement.Append(orderByClause.ToString)
                
                Return statement.ToString
                
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try            
        End Function
 
        'Create the drill back options list
        Private Function GetDrillBackTypeList(ByVal si As SessionInfo,
 		ByVal globals As BRGlobals, ByVal api As Transformer, ByVal args As ConnectorArgs) As List(Of DrillBackTypeInfo)
		
            Try
                'Create the SQL Statement
                Dim drillTypes As New List(Of DrillBackTypeInfo)
                
                drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.FileShareFile,
 				New NameAndDesc("","")))
                drillTypes.Add(New DrillBackTypeInfo(ConnectorDrillBackDisplayTypes.DataGrid,
 				New  NameAndDesc("","")))
                        
                Return drillTypes
                
            Catch ex As Exception
					
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try            
        End Function
        
        'Execute specific drill back type
        Private Function GetDrillBack(ByVal si As SessionInfo, ByVal globals As BRGlobals,
		ByVal api As Transformer,
		ByVal args As ConnectorArgs, ByVal drillBackType As ConnectorDrillBackDisplayTypes, 
		ByVal connectionString As String) As DrillBackResultInfo
            Try
                Select Case drillBackType 
                    Case Is = ConnectorDrillBackDisplayTypes.FileShareFile
						
                        'Show FileShare File
                        Dim drillBackInfo As New DrillBackResultInfo
                        drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.FileShareFile
                        drillBackInfo.DocumentPath = Me.GetDrillBackDocPath(si, globals, api, args)      
						
                        Return drillBackInfo
 
                    Case Is = ConnectorDrillBackDisplayTypes.DataGrid
						
                        'Return Drill Back Detail
                        Dim drillBackSQL As String = GetDrillBackSQL(si, globals, api, args)
                        Dim drillBackInfo As New DrillBackResultInfo
                        drillBackInfo.DisplayType = ConnectorDrillBackDisplayTypes.DataGrid                                
                        drillBackInfo.DataTable = api.Parser.GetXFDataTableForSQLQuery(si,  
                        DbProviderType.OLEDB, connectionString, True, drillBackSQL,
 						False, args.PageSize, args.PageNumber)
						
                        Return drillBackInfo
                                            
                        Case Else
                        Return Nothing    
                End Select    
                        
            Catch ex As Exception
					
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try
			
        End Function
 
        'Create the drill back Document Path
        Private Function GetDrillBackDocPath(ByVal si As SessionInfo, 
		ByVal globals As BRGlobals,ByVal api As Transformer, ByVal args As ConnectorArgs) As String
		
            Try
                'Get the values for the source row that we are drilling back to
                Dim sourceValues As Dictionary(Of String, Object) =  
				api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
                If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then
					
                    Return "Applications/GolfStream_v24/DataManagement/RevenueMgmtInvoices/" &
					sourceValues.Item(StageConstants.MasterDimensionNames.Attribute1).ToString & ".pdf"
					
                Else
					
                    Return String.Empty
					
                End If
				
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try
			
        End Function
 
    	'Create the drill back SQL Statement
        Private Function GetDrillBackSQL(ByVal si As SessionInfo, ByVal globals As BRGlobals,
 		ByVal api As Transformer, ByVal args As ConnectorArgs) As String
		
            Try
				
                'Get the values for the source row that we are drilling back to
                Dim sourceValues As Dictionary(Of String, Object) =  
				api.Parser.GetFieldValuesForSourceDataRow(si, args.RowID)
				
                If (Not sourceValues Is Nothing) And (sourceValues.Count > 0) Then                
 
                    Dim statement As New Text.StringBuilder
                    Dim selectClause As New Text.StringBuilder
                    Dim fromClause As New Text.StringBuilder
                    Dim whereClause As New Text.StringBuilder
                    Dim orderByClause As New Text.StringBuilder
 
                    'Create the SQL Statement                    
                    selectClause.Append("SELECT ") 
                    
                    fromClause.Append("FROM ")
                    
                    whereClause.Append("WHERE ")
                    
                    orderByClause.Append("ORDER BY ")
                    
                    'Create the full SQL Statement
                    statement.Append(selectClause.ToString)
                    statement.Append(fromClause.ToString)
                    statement.Append(whereClause.ToString)
                    statement.Append(orderByClause.ToString)
					
                    Return statement.ToString
					
                Else
					
                    Return String.Empty
					
                End If
            Catch ex As Exception
				
                Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
				
            End Try   
			
        End Function
		
	End Class
End Namespace