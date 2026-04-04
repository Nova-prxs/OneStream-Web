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
Imports OneStreamWorkspacesApi
Imports OneStreamWorkspacesApi.V800

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
	Public Class all_queries
		
		#Region "Queries"
		Public Function Query(
					ByVal queryName As String, 
					Optional ByVal args As IDictionary(Of String, String) = Nothing
					)
					
			Dim sql 		As String = String.Empty
			Dim sFactory 	As String = GetOrDefault(args, "Factory", "")
			Dim sProduct 	As String = GetOrDefault(args, "Product", "")	
			Dim sGM			As String = GetOrDefault(args, "GN", "")	
			Dim sCC			As String = GetOrDefault(args, "CC", "")	
			Dim sScenarioRef As String = GetOrDefault(args, "ScenarioRef", "")				
			Dim sTime 		As String = GetOrDefault(args, "Time", "UO2")
			Dim sTimeUnit	As String = GetOrDefault(args, "TimeUnit", "h")	
			
			sTimeUnit = If(sTimeUnit="min", "60", "1")
			Dim sTimeFilter As String = If(sTime = "All", "AND (uotype = 'UO1' OR uotype = 'UO2')", $"AND uotype = '{sTime}'")
			
			Dim factoryFilter As String = If(sFactory = "","",$"AND id_factory = '{sFactory}'")
			
			If queryName = "All_Data" Then
				sql = $"
					SELECT *
					FROM XFC_PLT_AUX_Production_Actual_Times
					WHERE 1=1
						{factoryFilter}
				"
				
			Else If queryName = "AllocationsActual"
				
				#Region "AllocationsActual - SQL"
				
				sql = $"
							
								WITH times_Filter as (
									SELECT
									'ACT' as Scenario
									, id_product
									, id_costcenter
									, id_averagegroup
									, start_date
									, end_date
									, value*{sTimeUnit} as value
									, uotype
							
								FROM XFC_PLT_AUX_Production_Actual_Times
								
								WHERE 1=1
									AND id_factory = '{sFactory}'
									AND (id_product = '{sProduct}' OR 'All' = '{sProduct}')
									{sTimeFilter}
									AND value <> 0
								)
							
								, all_dates as (
									SELECT DISTINCT all_dates as all_dates 
									FROM ( 
										SELECT DISTINCT start_date as all_dates
										FROM times_Filter
										UNION ALL
										SELECT MAX(end_date) as all_dates
										FROM times_Filter
										) DT
								)
								
								
								SELECT 
									ISNULL(DT.all_dates, '19000101') AS all_dates
									, ISNULL(T.Scenario, '#') as Scenario
									, ISNULL(T.id_costcenter +' - '+ T.id_averagegroup,'#') as id_averagegroup
									, ISNULL(T.id_costcenter, '#') as id_costcenter
									, ISNULL(T.value,0) as value
									, ISNULL(T.uotype, '#') as uotype
									
								FROM all_dates DT
								LEFT JOIN times_Filter T
									ON DT.all_dates BETWEEN T.start_date AND T.end_date
							
							"
				
				#End Region
				
			Else If queryName = "AllocationsActual_GM"
				
				#Region "AllocationsActual_GM - SQL"
				
				sql = $"
							
						WITH times_Filter as (
								SELECT
								'ACT' as Scenario
								, id_product
								, id_costcenter
								, id_averagegroup
								, start_date
								, end_date
								, value*{sTimeUnit} as value
								, uotype
								, comment
						
							FROM XFC_PLT_AUX_Production_Actual_Times
						
							WHERE 1=1
								AND id_factory = '{sFactory}'
								AND (id_product = '{sProduct}' OR 'All' = '{sProduct}')
								{sTimeFilter}
								AND value <> 0
								AND id_costcenter = '{sCC}'
							)
						
							, all_dates as (
								SELECT DISTINCT all_dates as all_dates 
								FROM ( 
									SELECT DISTINCT start_date as all_dates
									FROM times_Filter
									UNION ALL
									SELECT MAX(end_date) as all_dates
									FROM times_Filter
									) DT
							)
							
							
							SELECT 
								ISNULL(DT.all_dates, '19000101')	as all_dates
								, ISNULL(T.Scenario, '#') 			as Scenario
								, ISNULL(T.id_costcenter + ' - ' + T.id_averagegroup,'#') 	as id_averagegroup
								, ISNULL(T.id_costcenter ,'#') 		as id_costcenter
								, ISNULL(T.value,0) 				as value
								, ISNULL(T.uotype, '#') 			as uotype
								, ISNULL(T.comment, '#') 			as comment
								
							FROM all_dates DT
							LEFT JOIN times_Filter T
								ON DT.all_dates BETWEEN T.start_date AND T.end_date
							"
											
				#End Region
				
			Else If queryName = "AllocationsReference"

				#Region "AllocationsReference- SQL"
				
				sql = $"
						WITH base AS (
						    SELECT
						        scenario,
						        id_product,
						        id_averagegroup,
						        id_costcenter,
						        uotype,
						        [date],
						        /* Mantengo tu convención de bordes */
						        EOMONTH(DATEADD(MONTH, -1, [date]), 0) AS start_date,
						        EOMONTH([date], 0)                       AS end_date,
						        /* Si value es float y puede tener microdif., considera ROUND(...) */
						        value * {sTimeUnit} AS value
						    FROM XFC_PLT_AUX_Production_Planning_Times
						    WHERE 1=1
						      AND YEAR([date]) = 2025
						      AND scenario = '{sScenarioRef}'
						      AND id_factory = '{sFactory}'
						      AND (id_product = '{sProduct}' OR 'All' = '{sProduct}')
						      {sTimeFilter}
						      AND value <> 0
						),
						marcada AS (
						    SELECT
						        b.*,
						        CASE
						            WHEN LAG(b.value) OVER (
						                    PARTITION BY b.scenario, b.id_product, b.id_averagegroup, b.id_costcenter, b.uotype
						                    ORDER BY b.[date]
						                 ) IS NULL
						                 OR LAG(b.value) OVER (
						                    PARTITION BY b.scenario, b.id_product, b.id_averagegroup, b.id_costcenter, b.uotype
						                    ORDER BY b.[date]
						                 ) <> b.value
						            THEN 1 ELSE 0
						        END AS is_change
						    FROM base b
						),
						islas AS (
						    /* Suma acumulada: cada cambio inicia una nueva isla */
						    SELECT
						        m.*,
						        SUM(m.is_change) OVER (
						            PARTITION BY m.scenario, m.id_product, m.id_averagegroup, m.id_costcenter, m.uotype
						            ORDER BY m.[date]
						            ROWS UNBOUNDED PRECEDING
						        ) AS grp
						    FROM marcada m
						)

                        , times_Filter AS (
                            SELECT
                                scenario,
                                id_product,
                                id_averagegroup,
								id_costcenter,
                                uotype,
                                /* el valor constante de la isla */
                                MAX(value) AS value,
                                /* bordes de la isla según tu convención */
                                MIN(start_date) AS start_date,
                                MAX(end_date)   AS end_date
                            FROM islas
                            GROUP BY
                                scenario, id_product, id_averagegroup, id_costcenter, uotype, grp
                        )            
                        , all_dates as (
                            SELECT DISTINCT all_dates as all_dates 
                            FROM ( 
                                SELECT DISTINCT start_date as all_dates
                                FROM times_Filter
                                UNION ALL
                                SELECT MAX(end_date) as all_dates
                                FROM times_Filter
                                ) DT
                        )
		
                        SELECT 
                            ISNULL(DT.all_dates, '19000101') AS all_dates
                            , ISNULL(T.Scenario, '#') as Scenario
                            , ISNULL(T.id_costcenter +' - '+ T.id_averagegroup,'#') as id_averagegroup
                            , ISNULL(T.id_costcenter, '#') as id_costcenter
                            , ISNULL(T.value,0) as value
                            , ISNULL(T.uotype, '#') as uotype
                            
                        FROM all_dates DT
                        LEFT JOIN times_Filter T
                            ON DT.all_dates BETWEEN T.start_date AND T.end_date
				"
				#End Region
				
			Else If queryName = "CreateHistoricalData" Then
				
				#Region "CREAR - Tabla tiempos A10 o A20"
				
				sql = "
				                
					 BEGIN TRY
					    BEGIN TRANSACTION;
					                -- Limpieza de la tabla de destino
					                DELETE FROM XFC_PLT_AUX_Production_Actual_Times WHERE id_factory <> 'R0671';
					
					                -- Insertar los datos procesados
					                WITH Fechas AS (
										SELECT
											id_factory
											, id_costcenter
											, id_averagegroup
											, id_product
											, uotype
											, allocation_taj
											, MIN(date) AS date_min
											, MAX(date) AS date_max
											, ROW_NUMBER() OVER (PARTITION BY id_factory, id_costcenter, id_averagegroup, id_product, uotype ORDER BY MAX(date) DESC) AS row_num	
										FROM XFC_PLT_FACT_Production
										WHERE 1=1
												AND uotype IN ('UO1', 'UO2')
												AND scenario = 'Actual'
										GROUP BY  
												id_factory,
												id_costcenter,
												id_averagegroup,
												id_product,
												uotype,
												allocation_taj
									)
									, maxRowNum as (
										SELECT 
											id_factory
											, id_costcenter
											, id_averagegroup
											, id_product
											, uotype
											, MAX(row_num) as max_row_num	
										FROM Fechas
										GROUP BY  id_factory,
													 id_costcenter,
													 id_averagegroup,
													 id_product,
													 uotype
									)
									, startendcorrectas as (
										SELECT 
											A.id_factory
											, A.id_costcenter
											, A.id_averagegroup
											, A.id_product
											, A.uotype
											, A.allocation_taj		
											-- Generación de fechas start_date y end_date
											, CASE 
												WHEN A.row_num = 1 THEN '9999-12-31'
												ELSE date_max 
											  END AS end_date		
											, CASE 
												WHEN A.row_num = B.max_row_num THEN '1900-01-01' 
												ELSE date_min
											  END AS start_date
											 --------------------------------------------------
											, A.row_num
											, B.max_row_num
										FROM Fechas A
										LEFT JOIN maxRowNum B
											ON B.id_factory = A.id_factory
											AND B.id_costcenter = A.id_costcenter
											AND B.id_averagegroup = A.id_averagegroup
											AND B.id_product = A.id_product
									)
									, mid_dates as (
										SELECT
											A.id_factory
											, A.id_costcenter
											, A.id_averagegroup
											, A.id_product
											, A.uotype
											, A.allocation_taj
											, CASE WHEN A.row_num = A.max_row_num 
												THEN A.start_date 
												ELSE DATEADD(DAY, 1, B.end_date)
											  END AS start_date
											, A.end_date
											, A.row_num
											, A.max_row_num
										FROM startendcorrectas  A
										LEFT JOIN startendcorrectas  B
											ON B.id_factory = A.id_factory
											AND B.id_costcenter = A.id_costcenter
											AND B.id_averagegroup = A.id_averagegroup
											AND B.id_product = A.id_product
											AND B.uotype = A.uotype
											AND B.row_num = (A.row_num + 1)
									)
					 
									-- INSERT INTO la tabla de destino
									INSERT INTO XFC_PLT_AUX_Production_Actual_Times (
									    id_factory,
									    start_date,
									    end_date,
										id_product,
									    id_costcenter,
									    id_averagegroup,
									    id_workcenter,
									    uotype,
									    value
									)
									SELECT
										id_factory
										, start_date
										, end_date
										, id_product
										, id_costcenter
										, id_averagegroup
										, '-1' as id_workcenter
										, uotype
										, allocation_taj as value
									FROM mid_dates 
									WHERE id_factory <> 'R0671';
					
					    COMMIT TRANSACTION;
					 END TRY    
					    BEGIN CATCH
					        ROLLBACK TRANSACTION;
					        THROW;
					    END CATCH;
				"
				#End Region
				
			End If
			
			Return sql
			
		End Function
		#End Region
		
		#Region "Helper Functions"
		Private Function GetOrDefault(Of T)(dict As IDictionary(Of String, T), key As String, Optional def As T = Nothing) As T
		    If dict Is Nothing Then Return def
		    Dim val As T = Nothing
		    If dict.TryGetValue(key, val) Then Return val
		    Return def
		End Function
		#End Region
		
	End Class
End Namespace
