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

Namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.database_manteinance
	Public Class MainClass

		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
		
			Try
				Dim functionName As String = args.NameValuePairs.GetValueOrEmpty("functionName")
				Dim dataBase As String = args.NameValuePairs.GetValueOrEmpty("database")
				' dataBase = "Application"
				dataBase = "BiBlend"
				' functionName = "IndexManagement"
				
				If functionName = "JOB"
					
					#Region "JOB"
					
					' ======================================================================================================
					' 1. Analiza todos los índices CLUSTERED y NONCLUSTERED de tablas XFC_PLT
					' 2. Filtra:
					' 	- Fragmentación > 5%
					' 	- PageCount > 100
					' 3. Si Fragmentación > 30% → REBUILD
					' 4. Si Fragmentación 5%-30% → REORGANIZE
					' 5. Loguea toda la actividad
					' 6. Al final → Muestra tablas HEAP detectadas para que revises
					' ======================================================================================================
					Dim execTable As String = args.NameValuePairs.GetValueOrDefault("execTable","All")
					Dim jobReindex_v2 As String = String.Empty
					Dim tablesList As New List(Of String)
					
					If execTable = "All" Then
						
						Dim dtTables As New DataTable 
						Dim sqlTables As String = $"
							SELECT name
							FROM sys.tables
							WHERE name LIKE 'XFC_PLT_%'
						"
						
						Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)                               
		               		dtTables = BRAPi.Database.ExecuteSql(dbConnApp, sqlTables, True)		
		           		End Using  
						
						For Each row As DataRow In dtTables.Rows
							tablesList.Add(row("name").ToString())
						Next
						
					Else 
						
						tablesList.Add(execTable)	
						
					End If
					
					For Each tableName As String In tablesList
						
						#Region "Job"
						
						jobReindex_v2 = $" 
						
						    -- DECLARACIÓN DE VARIABLES NECESARIAS
						    DECLARE @TableName NVARCHAR(255)       -- Tabla que vamos a tratar
						    DECLARE @IndexName NVARCHAR(255)       -- Índice dentro de la tabla
						    DECLARE @IndexType NVARCHAR(50)        -- Tipo de índice (CLUSTERED / NONCLUSTERED)
						    DECLARE @SQL NVARCHAR(MAX)             -- Aquí generamos dinámicamente el comando SQL
						    DECLARE @Fragmentation FLOAT           -- Fragmentación actual del índice en %
						    DECLARE @PageCount INT                 -- Número de páginas que ocupa el índice
						
						    
						    -- DECLARACIÓN DEL CURSOR
						    -- Recogemos solo índices CLUSTERED y NONCLUSTERED de tablas que empiecen por XFC_PLT
						    -- Excluimos los HEAP
						    DECLARE IndexCursor CURSOR FOR
						    SELECT 
						        t.name AS TableName,
						        i.name AS IndexName,
						        i.type_desc AS IndexType
						    FROM sys.indexes i
						    INNER JOIN sys.dm_db_index_physical_stats (DB_ID(), NULL, NULL, NULL, 'SAMPLED') ips
						        ON i.object_id = ips.object_id AND i.index_id = ips.index_id
						    INNER JOIN sys.tables t ON i.object_id = t.object_id
						    WHERE t.name = '{tableName}'                 -- Solo las tablas OneStream
						    AND ips.page_count > 100                     -- Solo índices grandes
						    AND i.type_desc IN ('CLUSTERED', 'NONCLUSTERED') -- Excluimos HEAP
						    AND ips.avg_fragmentation_in_percent > 5     -- Solo índices con fragmentación apreciable (>5%)
						    
						    
						    -- ABRIMOS EL CURSOR
						    OPEN IndexCursor
						    
						    -- PRIMER REGISTRO A LEER
						    FETCH NEXT FROM IndexCursor INTO @TableName, @IndexName, @IndexType
						    
						    
						    -- BUCLE PARA RECORRER TODOS LOS ÍNDICES
						    WHILE @@FETCH_STATUS = 0
						    BEGIN
						        -- Actualizar las estadísticas de la tabla antes de realizar cualquier acción sobre los índices
						        SET @SQL = 'UPDATE STATISTICS [' + @TableName + '];'
						        EXEC(@SQL)  -- Ejecutar la actualización de estadísticas
						        
						        -- OBTENER FRAGMENTACIÓN Y Nº DE PÁGINAS DE ESTE ÍNDICE EN CONCRETO
						        SELECT @Fragmentation = avg_fragmentation_in_percent,
						               @PageCount = page_count
						        FROM sys.dm_db_index_physical_stats (DB_ID(), OBJECT_ID(@TableName), NULL, NULL, 'SAMPLED') 
						        WHERE index_id = (SELECT index_id FROM sys.indexes WHERE name = @IndexName)
						        
						        
						        -- DECISIÓN DE ACCIÓN SEGÚN LOS CRITERIOS DEFINIDOS
						        IF @Fragmentation > 30
						        BEGIN
						            -- FRAGMENTACIÓN ALTA → REBUILD
						            SET @SQL = 'ALTER INDEX [' + @IndexName + '] ON [' + @TableName + '] REBUILD WITH (ONLINE = ON);'
						        END
						        ELSE
						        BEGIN
						            -- FRAGMENTACIÓN MEDIA → REORGANIZE
						            SET @SQL = 'ALTER INDEX [' + @IndexName + '] ON [' + @TableName + '] REORGANIZE;'
						        END
						    
						        -- EJECUTAR ACCIÓN DEFINIDA
						        EXEC(@SQL)
						    
						        -- PASAMOS AL SIGUIENTE ÍNDICE
						        FETCH NEXT FROM IndexCursor INTO @TableName, @IndexName, @IndexType
						    END
						    
						    
						    -- CERRAMOS EL CURSOR Y LIBERAMOS MEMORIA
						    CLOSE IndexCursor
						    DEALLOCATE IndexCursor
						"
						#End Region
						
						If database="Application" Then
							ExecuteSql(si, jobReindex_v2) 
						Else 
							ExecuteActionSQLOnBIBlend(si,jobReindex_v2)
						End If
						
					Next
					
					#End Region
					
				Else If functionName = "IndexManagement"
					
					#Region "INDEX"			
					
						#Region "Application"
							
								Dim sqlIndex = "
										
									-- BEGIN TRY
									-- 	BEGIN TRANSACTION;
								
											-- ====================================
											-- ADD COLUMNS month_start (PERSISTED)
											-- ====================================
											IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Report_Account', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_FACT_Costs_VTU_Report_Account
												ADD month_start AS MONTH([date]) PERSISTED;
										
											IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Report_Account_Local', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_FACT_Costs_VTU_Report_Account_Local
												ADD month_start AS MONTH([date]) PERSISTED;
								
											IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Final', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_FACT_Costs_VTU_Final
												ADD month_start AS MONTH([date]) PERSISTED;
										
											IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Final_Local', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_FACT_Costs_VTU_Final_Local
												ADD month_start AS MONTH([date]) PERSISTED;
										
											IF COL_LENGTH('XFC_PLT_HIER_Nomenclature_Date_Report', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_HIER_Nomenclature_Date_Report
												ADD month_start AS MONTH([date]) PERSISTED;

											-- =========================
											-- ORIGIN TABLES (4 tablas)
											-- =========================
											
											-- XFC_PLT_FACT_Costs_VTU_Report_Account
											DROP INDEX IF EXISTS idx_origin_report_account_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Report_Account;
											DROP INDEX IF EXISTS idx_origin_report_account_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Report_Account;
											DROP INDEX IF EXISTS idx_origin_report_account_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Report_Account;
									
											CREATE INDEX idx_origin_report_account_ix_cover_for_query
											ON XFC_PLT_FACT_Costs_VTU_Report_Account (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, [date])
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
											CREATE INDEX idx_origin_report_account_ix_distinct_keys
											ON XFC_PLT_FACT_Costs_VTU_Report_Account (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account)
											INCLUDE ([date]);
									
											CREATE INDEX idx_origin_report_account_ix_keys_month_start
											ON XFC_PLT_FACT_Costs_VTU_Report_Account (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, month_start)
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
									
											-- XFC_PLT_FACT_Costs_VTU_Final
											DROP INDEX IF EXISTS idx_origin_final_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Final;
											DROP INDEX IF EXISTS idx_origin_final_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Final;
											DROP INDEX IF EXISTS idx_origin_final_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Final;
									
											CREATE INDEX idx_origin_final_ix_cover_for_query
											ON XFC_PLT_FACT_Costs_VTU_Final (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, [date])
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
											CREATE INDEX idx_origin_final_ix_distinct_keys
											ON XFC_PLT_FACT_Costs_VTU_Final (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account)
											INCLUDE ([date]);
									
											CREATE INDEX idx_origin_final_ix_keys_month_start
											ON XFC_PLT_FACT_Costs_VTU_Final (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, month_start)
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
									
											-- XFC_PLT_FACT_Costs_VTU_Report_Account_Local
											DROP INDEX IF EXISTS idx_origin_report_account_local_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
											DROP INDEX IF EXISTS idx_origin_report_account_local_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
											DROP INDEX IF EXISTS idx_origin_report_account_local_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
									
											CREATE INDEX idx_origin_report_account_local_ix_cover_for_query
											ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, [date])
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
											CREATE INDEX idx_origin_report_account_local_ix_distinct_keys
											ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account)
											INCLUDE ([date]);
									
											CREATE INDEX idx_origin_report_account_local_ix_keys_month_start
											ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, month_start)
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
									
											-- XFC_PLT_FACT_Costs_VTU_Final_Local
											DROP INDEX IF EXISTS idx_origin_final_local_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Final_Local;
											DROP INDEX IF EXISTS idx_origin_final_local_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Final_Local;
											DROP INDEX IF EXISTS idx_origin_final_local_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Final_Local;
									
											CREATE INDEX idx_origin_final_local_ix_cover_for_query
											ON XFC_PLT_FACT_Costs_VTU_Final_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, [date])
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
											CREATE INDEX idx_origin_final_local_ix_distinct_keys
											ON XFC_PLT_FACT_Costs_VTU_Final_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account)
											INCLUDE ([date]);
									
											CREATE INDEX idx_origin_final_local_ix_keys_month_start
											ON XFC_PLT_FACT_Costs_VTU_Final_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, month_start)
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
									
											-- ==================================
											-- HIERARCHY & PIVOT (2 tablas más)
											-- ==================================
									
											-- XFC_PLT_HIER_Nomenclature_Date_Report
											DROP INDEX IF EXISTS idx_hier_ix_scen_year_fact_comp_date ON XFC_PLT_HIER_Nomenclature_Date_Report;
											DROP INDEX IF EXISTS idx_hier_ix_scen_year_fact_comp_mstart ON XFC_PLT_HIER_Nomenclature_Date_Report;
									
											CREATE INDEX idx_hier_ix_scen_year_fact_comp_date
											ON XFC_PLT_HIER_Nomenclature_Date_Report (scenario, [year], id_factory, id_component, [date])
											INCLUDE (id_product_final, id_product, exp_coefficient);
									
											CREATE INDEX idx_hier_ix_scen_year_fact_comp_mstart
											ON XFC_PLT_HIER_Nomenclature_Date_Report (scenario, [year], id_factory, id_component, month_start)
											INCLUDE (id_product_final, id_product, exp_coefficient);
									
									
											-- XFC_PLT_AUX_Product_Pivot
											-- DROP INDEX IF EXISTS idx_pivot_ix_scen_year_fact_prod ON XFC_PLT_AUX_Product_Pivot;
											-- 
											-- CREATE INDEX idx_pivot_ix_scen_year_fact_prod
											-- ON XFC_PLT_AUX_Product_Pivot (scenario, [year], id_factory, id_product)
											-- INCLUDE (id_product_mapping);
									
									
											-- =============================
											-- UPDATE STATISTICS DE TABLAS
											-- =============================
									
											UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Report_Account;
											UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Final;
											UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
											UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Final_Local;
											UPDATE STATISTICS XFC_PLT_HIER_Nomenclature_Date_Report;
											-- UPDATE STATISTICS XFC_PLT_AUX_Product_Pivot;
								
									--  	COMMIT;
									--  END TRY
									--  BEGIN CATCH
									--  	ROLLBACK;
									--  END CATCH;
								"
						#End Region
						
						#Region "BiBlend"
													
								Dim sqlIndexBiBlend = "
										
									-- BEGIN TRY
									-- 	BEGIN TRANSACTION;
								
											-- ====================================
											-- ADD COLUMNS month_start (PERSISTED)
											-- ====================================								
											IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Final', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_FACT_Costs_VTU_Final
												ADD month_start AS MONTH([date]) PERSISTED;
										
											IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Final_Local', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_FACT_Costs_VTU_Final_Local
												ADD month_start AS MONTH([date]) PERSISTED;
										
											IF COL_LENGTH('XFC_PLT_HIER_Nomenclature_Date_Report', 'month_start') IS NULL
												ALTER TABLE XFC_PLT_HIER_Nomenclature_Date_Report
												ADD month_start AS MONTH([date]) PERSISTED;

											-- =========================
											-- ORIGIN TABLES (4 tablas)
											-- =========================
											
											-- XFC_PLT_FACT_Costs_VTU_Final
											DROP INDEX IF EXISTS idx_origin_final_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Final;
											DROP INDEX IF EXISTS idx_origin_final_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Final;
											DROP INDEX IF EXISTS idx_origin_final_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Final;
									
											CREATE INDEX idx_origin_final_ix_cover_for_query
											ON XFC_PLT_FACT_Costs_VTU_Final (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, [date])
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
											CREATE INDEX idx_origin_final_ix_distinct_keys
											ON XFC_PLT_FACT_Costs_VTU_Final (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account)
											INCLUDE ([date]);
									
											CREATE INDEX idx_origin_final_ix_keys_month_start
											ON XFC_PLT_FACT_Costs_VTU_Final (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, month_start)
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
											-- XFC_PLT_FACT_Costs_VTU_Final_Local
											DROP INDEX IF EXISTS idx_origin_final_local_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Final_Local;
											DROP INDEX IF EXISTS idx_origin_final_local_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Final_Local;
											DROP INDEX IF EXISTS idx_origin_final_local_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Final_Local;
									
											CREATE INDEX idx_origin_final_local_ix_cover_for_query
											ON XFC_PLT_FACT_Costs_VTU_Final_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, [date])
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
											CREATE INDEX idx_origin_final_local_ix_distinct_keys
											ON XFC_PLT_FACT_Costs_VTU_Final_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account)
											INCLUDE ([date]);
									
											CREATE INDEX idx_origin_final_local_ix_keys_month_start
											ON XFC_PLT_FACT_Costs_VTU_Final_Local (scenario, [year], id_factory, id_product, id_averagegroup, account_type, id_account, month_start)
											INCLUDE (cost_fixed, cost_variable, cost, volume, activity_UO1, activity_UO1_total);
									
									
											-- ==================================
											-- HIERARCHY & PIVOT (2 tablas más)
											-- ==================================
									
											-- XFC_PLT_HIER_Nomenclature_Date_Report
											DROP INDEX IF EXISTS idx_hier_ix_scen_year_fact_comp_date ON XFC_PLT_HIER_Nomenclature_Date_Report;
											DROP INDEX IF EXISTS idx_hier_ix_scen_year_fact_comp_mstart ON XFC_PLT_HIER_Nomenclature_Date_Report;
									
											CREATE INDEX idx_hier_ix_scen_year_fact_comp_date
											ON XFC_PLT_HIER_Nomenclature_Date_Report (scenario, [year], id_factory, id_component, [date])
											INCLUDE (id_product_final, id_product, exp_coefficient);
									
											CREATE INDEX idx_hier_ix_scen_year_fact_comp_mstart
											ON XFC_PLT_HIER_Nomenclature_Date_Report (scenario, [year], id_factory, id_component, month_start)
											INCLUDE (id_product_final, id_product, exp_coefficient);									
									
									
											-- =============================
											-- UPDATE STATISTICS DE TABLAS
											-- =============================
									
											UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Final;
											UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Final_Local;
											UPDATE STATISTICS XFC_PLT_HIER_Nomenclature_Date_Report;
								
									--  	COMMIT;
									--  END TRY
									--  BEGIN CATCH
									--  	ROLLBACK;
									--  END CATCH;
								"
								
						#End Region	
						
					#End Region
					
					If database="Application" Then 
						ExecuteSql(si, sqlIndex) 
					Else 
						ExecuteActionSQLOnBIBlend(si,sqlIndex) 
					End If
					
					#Region "INDEX DROP"
					sqlIndex = "
						BEGIN TRY
						    BEGIN TRANSACTION;
												
						    -- =============================
						    -- 1️⃣ ELIMINAR ÍNDICES (DROP INDEX)
						    -- =============================
						
						    -- XFC_PLT_FACT_Costs_VTU_Report_Account
						    DROP INDEX IF EXISTS idx_origin_report_account_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Report_Account;
						    DROP INDEX IF EXISTS idx_origin_report_account_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Report_Account;
						    DROP INDEX IF EXISTS idx_origin_report_account_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Report_Account;
						
						    -- XFC_PLT_FACT_Costs_VTU_Final
						    DROP INDEX IF EXISTS idx_origin_final_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Final;
						    DROP INDEX IF EXISTS idx_origin_final_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Final;
						    DROP INDEX IF EXISTS idx_origin_final_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Final;
						
						    -- XFC_PLT_FACT_Costs_VTU_Report_Account_Local
						    DROP INDEX IF EXISTS idx_origin_report_account_local_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
						    DROP INDEX IF EXISTS idx_origin_report_account_local_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
						    DROP INDEX IF EXISTS idx_origin_report_account_local_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
						
						    -- XFC_PLT_FACT_Costs_VTU_Final_Local
						    DROP INDEX IF EXISTS idx_origin_final_local_ix_cover_for_query ON XFC_PLT_FACT_Costs_VTU_Final_Local;
						    DROP INDEX IF EXISTS idx_origin_final_local_ix_distinct_keys ON XFC_PLT_FACT_Costs_VTU_Final_Local;
						    DROP INDEX IF EXISTS idx_origin_final_local_ix_keys_month_start ON XFC_PLT_FACT_Costs_VTU_Final_Local;
						
						    -- XFC_PLT_HIER_Nomenclature_Date_Report
						    DROP INDEX IF EXISTS idx_hier_ix_scen_year_fact_comp_date ON XFC_PLT_HIER_Nomenclature_Date_Report;
						    DROP INDEX IF EXISTS idx_hier_ix_scen_year_fact_comp_mstart ON XFC_PLT_HIER_Nomenclature_Date_Report;
						
						    -- XFC_PLT_AUX_Product_Pivot
						    DROP INDEX IF EXISTS idx_pivot_ix_scen_year_fact_prod ON XFC_PLT_AUX_Product_Pivot;						
						
						    -- =============================
						    -- 2️⃣ ELIMINAR COLUMNAS month_start
						    -- =============================
						
						    IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Report_Account', 'month_start') IS NOT NULL
						        ALTER TABLE XFC_PLT_FACT_Costs_VTU_Report_Account DROP COLUMN month_start;
						
						    IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Report_Account_Local', 'month_start') IS NOT NULL
						        ALTER TABLE XFC_PLT_FACT_Costs_VTU_Report_Account_Local DROP COLUMN month_start;
						
						    IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Final', 'month_start') IS NOT NULL
						        ALTER TABLE XFC_PLT_FACT_Costs_VTU_Final DROP COLUMN month_start;
						
						    IF COL_LENGTH('XFC_PLT_FACT_Costs_VTU_Final_Local', 'month_start') IS NOT NULL
						        ALTER TABLE XFC_PLT_FACT_Costs_VTU_Final_Local DROP COLUMN month_start;
						
						    IF COL_LENGTH('XFC_PLT_HIER_Nomenclature_Date_Report', 'month_start') IS NOT NULL
						        ALTER TABLE XFC_PLT_HIER_Nomenclature_Date_Report DROP COLUMN month_start;
						
						    PRINT '✅ Columnas calculadas month_start eliminadas correctamente.';
						
						
						    -- =============================
						    -- 3️⃣ ACTUALIZAR ESTADÍSTICAS (OPCIONAL)
						    -- =============================
						
						    UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Report_Account;
						    UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Final;
						    UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Report_Account_Local;
						    UPDATE STATISTICS XFC_PLT_FACT_Costs_VTU_Final_Local;
						    UPDATE STATISTICS XFC_PLT_HIER_Nomenclature_Date_Report;
						    UPDATE STATISTICS XFC_PLT_AUX_Product_Pivot;
						
						    -- =============================
						    -- 4️⃣ FINALIZAR TRANSACCIÓN
						    -- =============================
						    COMMIT;
						
						END TRY
						BEGIN CATCH
						    ROLLBACK;
						END CATCH;
					"
					#End Region
					
				Else If functionName = "KILL"
					
					#Region "KILL"
					
					Dim sessionID = args.NameValuePairs.GetValueOrEmpty("sessionID")
					Dim sqlKill As String = $"
						KILL {sessionID};
					"
					
					#End Region

					If database="Application" Then 
						ExecuteSql(si, sqlKill) 
					Else 
						ExecuteActionSQLOnBIBlend(si,sqlKill) 
					End If
					
				End If
					
				Return Nothing
				
			Catch ex As Exception
				Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
			End Try
			
		End Function

        Private Sub ExecuteSql(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(si)
                               
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using   
				
        End Sub			
		
        Private Sub ExecuteSqlOtherAPP(ByVal si As SessionInfo, ByVal sqlCmd As String)        
		
			Dim oar As New OpenAppResult
			Dim osi As SessionInfo = BRApi.Security.Authorization.CreateSessionInfoForAnotherApp(si, "Production", oar)
		
            Using dbConnApp As DBConnInfo = BRAPi.Database.CreateApplicationDbConnInfo(osi)
                               
                BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, True, True)
				
            End Using   
				
        End Sub					
		
				
		Private Sub ExecuteActionSQLOnBIBlend(ByVal si As SessionInfo, ByVal sqlCmd As String)
				Dim extConnName As String = "OneStream BI Blend" 
                Using dbConnApp As DBConnInfo = BRApi.Database.CreateExternalDbConnInfo(si, extConnName)          
					BRAPi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, False, True)
                End Using                                                                               
        End Sub
		
		
	End Class
End Namespace
