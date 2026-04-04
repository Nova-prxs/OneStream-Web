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

Namespace OneStream.BusinessRule.Extender.WPP_ExecuteSQL
	Public Class MainClass
		Public Function Main(ByVal si As SessionInfo, ByVal globals As BRGlobals, ByVal api As Object, ByVal args As ExtenderArgs) As Object
			Try
				
				Dim sQuery As String = "
					TRUNCATE TABLE XFC_HR_MASTER_Mapping;
					INSERT INTO XFC_HR_MASTER_Mapping (table_name, source_column, target_column, display) VALUES
						('XFC_HR_MASTER_GLB_Empleados', 'ID_EMPLEADO',              'id_empleado',               'ID Empleado'),
						('XFC_HR_MASTER_GLB_Empleados', 'PERIODO',                  'periodo',                   'Período'),
						('XFC_HR_MASTER_GLB_Empleados', 'AREA_DE_PERSONAL',         'area_personal',             'Área de Personal'),
						('XFC_HR_MASTER_GLB_Empleados', 'ID_PUESTO',                'id_puesto',                 'ID Puesto'),
						('XFC_HR_MASTER_GLB_Empleados', 'ID_ORGEH',                 'id_unidad_organizativa',    'ID Unidad Organizativa'),
						('XFC_HR_MASTER_GLB_Empleados', 'STATUS',                   'status',                    'Status'),
						('XFC_HR_MASTER_GLB_Empleados', 'MARCA',                    'marca',                     'Marca'),
						('XFC_HR_MASTER_GLB_Empleados', 'DIVP',                     'divP',                      'División'),
						('XFC_HR_MASTER_GLB_Empleados', 'UNIDAD',                   'unidad',                    'Unidad'),
						('XFC_HR_MASTER_GLB_Empleados', 'TEXTO_UNIDAD',             'texto_unidad',              'Texto Unidad'),
						('XFC_HR_MASTER_GLB_Empleados', 'NOMBRE_Y_APELLIDO',        'nombre_apellido',           'Nombre y Apellido'),
						('XFC_HR_MASTER_GLB_Empleados', 'HORAS_JORNADA_CONTRATO',   'horas_jornada_contrato',    'Horas Jornada Contrato'),
						('XFC_HR_MASTER_GLB_Empleados', 'PORCENTAJE_JORNADA_CONTRATO', 'porcentaje_jornada_contrato', '% Jornada Contrato'),
						('XFC_HR_MASTER_GLB_Empleados', 'HORAS_ANUALES_CONTRATO',   'hora_anuales_contrato',     'Horas Anuales Contrato'),
						('XFC_HR_MASTER_GLB_Empleados', 'GRUPO_PROFESIONAL',        'grupo_profesional',         'Grupo Profesional'),
						('XFC_HR_MASTER_GLB_Empleados', 'SOCIEDAD',                 'sociedad',                  'Sociedad'),
						('XFC_HR_MASTER_GLB_Empleados', 'SEXO',                     'sexo',                      'Sexo'),
						('XFC_HR_MASTER_GLB_Empleados', 'FECHA_NACIMIENTO',         'fecha_nacimiento',          'Fecha Nacimiento'),
						('XFC_HR_MASTER_GLB_Empleados', 'PROVINCIA_TRIBUTACIÓN',    'provincia_tributacion',     'Provincia Tributación'),
						('XFC_HR_MASTER_GLB_Empleados', 'TIPO_DE_CONTRATO',         'tipo_contrato',             'Tipo de Contrato'),
						('XFC_HR_MASTER_GLB_Empleados', 'CNAE ',                    'CNAE',                      'CNAE'),
						('XFC_HR_MASTER_GLB_Empleados', 'CLAVE_DE_OCUPACION',       'clave_ocupacion',           'Clave de Ocupación'),
						('XFC_HR_MASTER_GLB_Empleados', 'CECO_ALTA',                'id_cebe_alta',              'CECO Alta'),
						('XFC_HR_MASTER_GLB_Empleados', 'TEXTO_CATEGORIA',          'texto_categoria',           'Texto Categoría'),
						('XFC_HR_MASTER_GLB_Empleados', 'TEXTO_SUBCATEGORIA',       'texto_subcategoría',        'Texto Subcategoría'),
						('XFC_HR_MASTER_GLB_Empleados', 'TEXTO_PUESTO_DE_TRABAJO',  'texto_puesto_trabajo',      'Texto Puesto de Trabajo'),
						('XFC_HR_MASTER_GLB_Empleados', 'REGION',                   'region',                    'Región'),
						('XFC_HR_MASTER_GLB_Empleados', 'ZONA',                     'zona',                      'Zona'),
						('XFC_HR_MASTER_GLB_Empleados', 'SUB_ZONA',                 'sub_zona',                  'Sub Zona'),
						('XFC_HR_MASTER_GLB_Empleados', 'ANTIGUEDAD',               'antiguedad',                'Antigüedad'),
						('XFC_HR_MASTER_GLB_Empleados', 'FECHA_BAJA',               'fecha_baja',                'Fecha Baja'),
						('XFC_HR_MASTER_GLB_Empleados', 'MOTIVO_BAJA',              'motivo_baja',               'Motivo Baja'),
						('XFC_HR_MASTER_GLB_Empleados', 'LIBERADOS',                'liberados',                 'Liberados'),
						('XFC_HR_MASTER_GLB_Empleados', 'PAIS',                     'pais',                      'País'),
						('XFC_HR_RAW_GLB_NominaTeorica', 'ID_EMPLEADO',             'id_empleado',               'ID Empleado'),
						('XFC_HR_RAW_GLB_NominaTeorica', 'PERIODO',                 'periodo',                   'Período'),
						('XFC_HR_RAW_GLB_NominaTeorica', 'ID_CONCEPTO',             'id_concepto',               'ID Concepto'),
						('XFC_HR_RAW_GLB_NominaTeorica', 'FECHA_INICIO',            'fecha_inicio',              'Fecha Inicio'),
						('XFC_HR_RAW_GLB_NominaTeorica', 'FECHA_FIN',               'fecha_fin',                 'Fecha Fin'),
						('XFC_HR_RAW_GLB_NominaTeorica', 'IMPORTE',                 'importe',                   'Importe');
					"
					
					sQuery = "
						INSERT INTO XFC_HR_MASTER_GLB_CECO (cebe, cebe_desciption)
						SELECT cebe, description FROM XFC_CEBES
					"
					
					sQuery = "DROP TABLE XFC_HR_RAW_GLB_NominaTeorica_Raw"
					sQuery = "DROP TABLE XFC_HR_MASTER_GLB_Empleados_Raw"
					
					#Region "XFC_HR_ETL_TableConfig"
'					sQuery="
'					CREATE TABLE XFC_HR_ETL_TableConfig (
'					    TableName               SYSNAME        NOT NULL PRIMARY KEY,
'					    EtlMode                 NVARCHAR(20)   NULL,
'					    CustomEtlSql            NVARCHAR(MAX)  NULL,
'					    PreDeleteKeyColumns     NVARCHAR(400)  NULL,
'					    DefaultDecimalSeparator   NVARCHAR(2)  NULL,
'					    DefaultThousandsSeparator NVARCHAR(2)  NULL,
'					    FloatDecimalSeparator     NVARCHAR(2)  NULL,
'					    FloatThousandsSeparator   NVARCHAR(2)  NULL,
'					    FloatEnabledTablesFlag    BIT          NULL,
'					    FloatColumns              NVARCHAR(400) NULL,
'					    DefaultDateInputFormats   NVARCHAR(400) NULL,
'					    DateColumns               NVARCHAR(400) NULL,
'					    DateColumnInputFormats    NVARCHAR(MAX) NULL,
'					    MonthStartDateColumns     NVARCHAR(400) NULL
'					);
					
'					INSERT INTO XFC_HR_ETL_TableConfig (
'					    TableName,
'					    EtlMode,
'					    CustomEtlSql,
'					    PreDeleteKeyColumns,
'					    DefaultDecimalSeparator,
'					    DefaultThousandsSeparator,
'					    FloatDecimalSeparator,
'					    FloatThousandsSeparator,
'					    FloatEnabledTablesFlag,
'					    FloatColumns,
'					    DefaultDateInputFormats,
'					    DateColumns,
'					    DateColumnInputFormats,
'					    MonthStartDateColumns
'					)
'					VALUES (
'					    N'XFC_HR_MASTER_GLB_Empleados',
'					    N'dynamic',
'					    NULL,
'					    N'periodo',
'					    NULL,
'					    NULL,
'					    N',',
'					    N'.',
'					    1,
'					    N'horas_jornada_contrato,porcentaje_jornada_contrato,hora_anuales_contrato',
'					    N'dd.mm.yyyy',
'					    NULL,
'					    N'periodo:yyyymm',
'					    N'periodo'
'					);
'					"
					#End Region

					
					
					sQuery = "
					INSERT INTO XFC_HR_MASTER_Mapping (table_name, source_column, target_column, display) VALUES
						('XFC_HR_MASTER_GLB_MapeoCuentaContable', 'ID_CONCEPTO',               'id_concepto',                 'ID Concepto'),
						('XFC_HR_MASTER_GLB_MapeoCuentaContable', 'PAIS',               'pais',                 'País'),
						('XFC_HR_MASTER_GLB_MapeoCuentaContable', 'SOCIEDAD',               'sociedad',                 'Sociedad'),
						('XFC_HR_MASTER_GLB_MapeoCuentaContable', 'AREA_DE_PERSONAL',               'area_de_personal',                 'Área de personal'),
						('XFC_HR_MASTER_GLB_MapeoCuentaContable', 'AGRUPADOR',               'agrupador',                 'Agrupador'),
						('XFC_HR_MASTER_GLB_MapeoCuentaContable', 'CUENTA_CONTABLE',               'cuenta_contable',                 'Cuenta Contable');
					"
					
					'Tabla Global Assumptions
					sQuery = "
						INSERT INTO XFC_HR_AUX_GLB_Assumptions (escenario, periodo, pais, id_concepto, descripcion_concepto, cantidad) VALUES
						('Forecast_V1','2026','España','PORSS','Porcentaje SS',''),
						('Forecast_V1','2026','España','BMCOT','Base máxima de cotización',''),
						('Forecast_V1','2026','España','TME10','Tramo <10%',''),
						('Forecast_V1','2026','España','T1050','Tramo 10%-50%',''),
						('Forecast_V1','2026','España','TMA50','Tramo >50%',''),
						('Forecast_V1','2026','España','PPILA','Porcentaje presupuestado PILA',''),
						('Forecast_V1','2026','España','PBOEG','Porcentaje presupuestado Bono EG',''),
						('Forecast_V1','2026','España','PBOCS','Porcentaje presupuestado Bono CS',''),
						('Forecast_V1','2026','España','HORJA','Horas jornada anual',''),
						
						('Forecast_V1','2026','Portugal','PORSS','Porcentaje SS',''),
						('Forecast_V1','2026','Portugal','BMCOT','Base máxima de cotización',''),
						('Forecast_V1','2026','Portugal','TME10','Tramo <10%',''),
						('Forecast_V1','2026','Portugal','T1050','Tramo 10%-50%',''),
						('Forecast_V1','2026','Portugal','TMA50','Tramo >50%',''),
						('Forecast_V1','2026','Portugal','PPILA','Porcentaje presupuestado PILA',''),
						('Forecast_V1','2026','Portugal','PBOEG','Porcentaje presupuestado Bono EG',''),
						('Forecast_V1','2026','Portugal','PBOCS','Porcentaje presupuestado Bono CS',''),
						('Forecast_V1','2026','Portugal','HORJA','Horas jornada anual','');
					"
					
					
					sQuery = "
					INSERT INTO XFC_HR_MASTER_GLB_Escenarios (periodo, tipo, escenario, descripcion, fecha) VALUES
					('2026','Forecast','Forecast_V4','', GETDATE());
					"


					
					sQuery = "
					INSERT INTO XFC_HR_AUX_CS_DescPuestos (
					    escenario,
					    periodo,
					    id_puesto,
					    descripcion_puesto,
					    tipo_posicion,
					    prioridad,
					    nivel_KF,
					    porcentaje_tabulador
					)
					SELECT
					    'Forecast_V1' AS escenario,
					    2026 AS periodo,
					    OBJID AS id_puesto,
					    OBJID_TEXT AS descripcion_puesto,
					    NULL AS tipo_posicion,
					    NULL AS prioridad,
					    NULL AS nivel_KF,
					    NULL AS porcentaje_tabulador
					FROM XFC_HR_MASTER_CS_Estructura
					WHERE OTYPE = 'S';
					"
					
					sQuery = "
					INSERT INTO XFC_HR_AUX_CS_SalarioNivelKF (
					    escenario,
					    periodo,
					    nivel_KF,
					    salario_nivel
					)
					SELECT
					    'Forecast_V1' AS escenario,
					    2026 AS periodo,
					    nivel_KF,
					    NULL AS salario_nivel
					FROM (
					    SELECT 8 AS nivel_KF UNION ALL
					    SELECT 9 UNION ALL
					    SELECT 10 UNION ALL
					    SELECT 11 UNION ALL
					    SELECT 12 UNION ALL
					    SELECT 13 UNION ALL
					    SELECT 14 UNION ALL
					    SELECT 15
					) t;
					"
					
					
					#Region "Conexión SIC"
'					Using dbConn As DbConnInfo = BRApi.Database.CreateDbConnInfo(si, dbLocation.External, "SICPreSQL")

'				    Dim sql As String = "
'				    SELECT 
'				        @@SERVERNAME AS Servidor,
'				        DB_NAME() AS BaseDeDatos,
'				        SYSTEM_USER AS Usuario,
'				        HOST_NAME() AS Host,
'				        APP_NAME() AS Aplicacion,
'				        CAST(CONNECTIONPROPERTY('client_net_address') AS VARCHAR(50)) AS IP
'				    "
				
'				    Dim dt As DataTable = BRApi.Database.ExecuteSql(dbConn, sql, False)
				
'				    If dt.Rows.Count > 0 Then
'				        Dim row = dt.Rows(0)
				
'				        BRApi.ErrorLog.LogMessage(si,
'				            "[SIC INFO] Servidor: " & row("Servidor").ToString() &
'				            " | BD: " & row("BaseDeDatos").ToString() &
'				            " | Usuario: " & row("Usuario").ToString() &
'				            " | Host: " & row("Host").ToString() &
'				            " | App: " & row("Aplicacion").ToString() &
'				            " | IP: " & row("IP").ToString()
'				        )
'				    End If
				
'				End Using
					#End Region
					
					
					sQuery = "
					DELETE FROM XFC_HR_MASTER_GLB_Escenarios
					WHERE escenario IN (
					    'Forecast_V2',
					    'Forecast_V3',
					    'Forecast_V4',
						'Forecast_V5'
					);
					"
					
					sQuery = "
					-- Crear la tabla
					CREATE TABLE XFC_HR_AMD_ParamConfig (
					    parametro VARCHAR(50),
					    valor VARCHAR(50)
					);
					
					-- Insertar el registro
					INSERT INTO XFC_HR_AMD_ParamConfig (parametro, valor)
					VALUES ('escenario_abierto', '202602');
					"
					
					#Region "XFC_HR_AUX_GLB_DescConceptos"
					sQuery = "
					INSERT INTO XFC_HR_AUX_GLB_DescConceptos
					(
					    escenario,
					    periodo,
					    id_concepto,
					    descripcion_concepto,
					    pais,
					    area_personal
					)
					SELECT
					    'Forecast_V1' AS escenario,
					    2026 AS periodo,
					    id_concepto,
					    descripcion_concepto,
					    pais,
					    'soporte' AS area_personal
					FROM XFC_HR_MASTER_GLB_Conceptos;
					"
					#End Region
					
					#Region "XFC_HR_AUX_GLB_IncrementoSalarial"
					
					sQuery = "
					-- 1. Crear dataset con periodo solo año (yyyy) y area_personal en minúsculas
					SELECT 
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_puesto_trabajo_nivel_KF
					INTO #dataset
					FROM (
					    SELECT
					        'Forecast_V1' AS escenario,
					        FORMAT(e.periodo,'yyyy') AS periodo,  -- <-- solo el año
					        LOWER(
					            CASE 
					                WHEN e.area_personal IN ('70', '71', '78', '79', '80', '81') THEN 'base'
					                WHEN e.area_personal IN ('72', '73') THEN 'gerencial'
					                WHEN e.area_personal IN ('74', '75', '76') THEN 'soporte'
					            END
					        ) AS area_personal,
					        e.marca,
					        e.texto_puesto_trabajo AS texto_puesto_trabajo_nivel_KF
					    FROM XFC_HR_MASTER_GLB_Empleados e
					    WHERE e.area_personal IN ('70', '71', '72', '73', '74', '78', '79', '80', '81')
					) x
					WHERE area_personal IN ('base', 'gerencial')  -- solo base y gerencial
					  AND marca IS NOT NULL
					  AND texto_puesto_trabajo_nivel_KF IS NOT NULL
					GROUP BY 
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_puesto_trabajo_nivel_KF;
					
					-- 2. Borrar registros que vamos a recargar (comparación insensible a mayúsculas)
					DELETE t
					FROM XFC_HR_AUX_GLB_IncrementoSalarial t
					INNER JOIN #dataset s
					    ON t.escenario = s.escenario 
					   AND t.periodo = s.periodo  -- ahora comparando solo año
					   AND LOWER(t.area_personal) = LOWER(s.area_personal)
					   AND t.marca = s.marca 
					   AND t.texto_puesto_trabajo_nivel_KF = s.texto_puesto_trabajo_nivel_KF;
					
					-- 3. Insertar dataset nuevo con periodo solo año
					INSERT INTO XFC_HR_AUX_GLB_IncrementoSalarial (
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_puesto_trabajo_nivel_KF
					)
					SELECT
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_puesto_trabajo_nivel_KF
					FROM #dataset;
					
					-- 4. Limpieza
					DROP TABLE #dataset;
					"
					
					#End Region
					
					#Region "XFC_HR_AUX_GLB_SalarioEstructura"
					
					sQuery = "
					-- 1. Crear dataset con periodo solo año y area_personal en minúsculas
					SELECT 
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_categoria
					INTO #dataset
					FROM (
					    SELECT
					        'Forecast_V1' AS escenario,
					        FORMAT(e.periodo,'yyyy') AS periodo,  -- solo el año
					        LOWER(
					            CASE 
					                WHEN e.area_personal IN ('70', '71', '78', '79', '80', '81') THEN 'base'
					                WHEN e.area_personal IN ('72', '73') THEN 'gerencial'
					            END
					        ) AS area_personal,
					        e.marca,
					        e.texto_categoria
					    FROM XFC_HR_MASTER_GLB_Empleados e
					    WHERE e.area_personal IN ('70', '71', '72', '73', '78', '79', '80', '81')
					) x
					WHERE area_personal IN ('base', 'gerencial')
					  AND marca IS NOT NULL
					  AND texto_categoria IS NOT NULL
					GROUP BY 
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_categoria;
					
					-- 2. Borrar registros que vamos a recargar (comparación insensible a mayúsculas)
					DELETE t
					FROM XFC_HR_AUX_GLB_SalarioEstructura t
					INNER JOIN #dataset s
					    ON t.escenario = s.escenario
					   AND t.periodo = s.periodo
					   AND LOWER(t.area_personal) = LOWER(s.area_personal)
					   AND t.marca = s.marca
					   AND t.texto_categoria = s.texto_categoria;
					
					-- 3. Insertar dataset nuevo con periodo solo año
					INSERT INTO XFC_HR_AUX_GLB_SalarioEstructura (
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_categoria
					)
					SELECT
					    escenario,
					    periodo,
					    area_personal,
					    marca,
					    texto_categoria
					FROM #dataset;
					
					-- 4. Limpieza
					DROP TABLE #dataset;
					"
					
					#End Region
					
					#Region "XFC_HR_AUX_GLB_Nocturnidad"
					
					sQuery = "
					-- 1. Crear dataset con periodo solo año
					SELECT
					    escenario,
					    periodo,
					    marca,
					    grupo_profesional
					INTO #dataset
					FROM (
					    SELECT
					        'Forecast_V1' AS escenario,
					        FORMAT(e.periodo,'yyyy') AS periodo,  -- solo el año
					        e.marca,
					        e.grupo_profesional
					    FROM XFC_HR_MASTER_GLB_Empleados e
					    WHERE e.area_personal IN ('70','71','72','73','78','79','80','81') -- base y gerencial
					) x
					WHERE marca IS NOT NULL
					  AND grupo_profesional IS NOT NULL
					GROUP BY
					    escenario,
					    periodo,
					    marca,
					    grupo_profesional;
					
					-- 2. Borrar registros que vamos a recargar (comparación insensible a mayúsculas)
					DELETE t
					FROM XFC_HR_AUX_GLB_Nocturnidad t
					INNER JOIN #dataset s
					    ON t.escenario = s.escenario
					   AND t.periodo = s.periodo
					   AND LOWER(t.marca) = LOWER(s.marca)
					   AND LOWER(t.grupo_profesional) = LOWER(s.grupo_profesional);
					
					-- 3. Insertar dataset nuevo con periodo solo año
					INSERT INTO XFC_HR_AUX_GLB_Nocturnidad (
					    escenario,
					    periodo,
					    marca,
					    grupo_profesional
					)
					SELECT
					    escenario,
					    periodo,
					    marca,
					    grupo_profesional
					FROM #dataset;
					
					-- 4. Limpieza
					DROP TABLE #dataset;
					"
					
					#End Region
					
					sQuery = "TRUNCATE TABLE XFC_HR_AUX_EG_TipoUnidadesFacturacion"
					
					sQuery = "
					BEGIN TRANSACTION;

					BEGIN TRY
					
					    -- 1. Añadir columna (si no existe)
					    IF COL_LENGTH('XFC_HR_ETL_TableConfig', 'SqlDefaultDateInputFormats') IS NULL
					    BEGIN
					        ALTER TABLE XFC_HR_ETL_TableConfig
					        ADD SqlDefaultDateInputFormats VARCHAR(500);
					    END
					
					    -- 2. Copiar datos usando SQL dinámico (evita error de compilación)
					    IF COL_LENGTH('XFC_HR_ETL_TableConfig', 'SqlDefaultDateInputFormats') IS NOT NULL
					    BEGIN
					        EXEC('
					            UPDATE XFC_HR_ETL_TableConfig
					            SET SqlDefaultDateInputFormats = DefaultDateInputFormats
					            WHERE SqlDefaultDateInputFormats IS NULL
					        ');
					    END
					
					    -- 3. Renombrar columna
					    IF COL_LENGTH('XFC_HR_ETL_TableConfig', 'DefaultDateInputFormats') IS NOT NULL
					    BEGIN
					        EXEC sp_rename 
					            'XFC_HR_ETL_TableConfig.DefaultDateInputFormats', 
					            'FileDefaultDateInputFormats', 
					            'COLUMN';
					    END
					
					    -- 4. Eliminar columnas obsoletas
					    IF COL_LENGTH('XFC_HR_ETL_TableConfig', 'CustomEtlSql') IS NOT NULL
					        ALTER TABLE XFC_HR_ETL_TableConfig DROP COLUMN CustomEtlSql;
					
					    IF COL_LENGTH('XFC_HR_ETL_TableConfig', 'DefaultDecimalSeparator') IS NOT NULL
					        ALTER TABLE XFC_HR_ETL_TableConfig DROP COLUMN DefaultDecimalSeparator;
					
					    IF COL_LENGTH('XFC_HR_ETL_TableConfig', 'DefaultThousandsSeparator') IS NOT NULL
					        ALTER TABLE XFC_HR_ETL_TableConfig DROP COLUMN DefaultThousandsSeparator;
					
					    IF COL_LENGTH('XFC_HR_ETL_TableConfig', 'FloatEnabledTablesFlag') IS NOT NULL
					        ALTER TABLE XFC_HR_ETL_TableConfig DROP COLUMN FloatEnabledTablesFlag;
					
					    COMMIT TRANSACTION;
					
					END TRY
					BEGIN CATCH
					    ROLLBACK TRANSACTION;
					    THROW;
					END CATCH;
					"
					
					
					
					sQuery = "
					WITH HierarchyCTE AS (

					    SELECT 
					        e.SEQNR,
					        e.OBJID,
					        e.PERID,
					        e.OBJID_TEXT,
					        e.PUP,
					        CAST('' AS NVARCHAR(MAX)) AS jerarquia,
					        0 AS Nivel
					    FROM XFC_HR_MASTER_CS_Estructura e
					    WHERE e.PUP = 0
					
					    UNION ALL
					
					    SELECT 
					        e.SEQNR,
					        e.OBJID,
					        e.PERID,
					        e.OBJID_TEXT,
					        e.PUP,
					        CASE 
					            WHEN h.jerarquia = '' THEN h.OBJID_TEXT
					            ELSE h.OBJID_TEXT + ' > ' + h.jerarquia
					        END AS jerarquia,
					        h.Nivel + 1 AS Nivel
					    FROM XFC_HR_MASTER_CS_Estructura e
					    INNER JOIN HierarchyCTE h ON e.PUP = h.SEQNR
					
					),
					
					FinalHierarchy AS (
					    SELECT 
					        OBJID,
					        PERID,
					        jerarquia,
					        ROW_NUMBER() OVER (PARTITION BY OBJID, PERID ORDER BY Nivel DESC) AS rn
					    FROM HierarchyCTE
					)
					
					UPDATE d
					SET d.jerarquia = fh.jerarquia
					FROM XFC_HR_AUX_CS_DescPuestos d
					INNER JOIN FinalHierarchy fh 
					    ON d.id_puesto = fh.OBJID        -- id_puesto = OBJID
					--    AND d.periodo = fh.PERID         -- periodo = PERID
					WHERE fh.rn = 1;				
					"

					sQuery = "
					DELETE FROM XFC_HR_AMD_ParamConfig
					WHERE parametro = 'escenario_abierto';
					"
					
					sQuery = "
					ALTER TABLE XFC_HR_ETL_TableConfig
					ADD SqlPeriodColumn NVARCHAR(50) NULL;
					"
					
					
					ExecuteSql(si, sQuery)
					
					#Region "EXPORT XFC_HR_AUX_GLB_DescConceptos"
'					' EXPORT TABLA
'					' Business Rule: Exportar tabla XFC_HR_AUX_GLB_DescConceptos a Excel en File Share
					
'					' 1. Ejecutar consulta SQL y obtener DataTable
'					Dim sql As String = "SELECT * FROM XFC_HR_AUX_GLB_DescConceptos"
'					Dim dt As DataTable
'					Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'					    dt = BRApi.Database.ExecuteSql(dbConnApp, sql, True)
'					End Using
					
'					' 2. Convertir DataTable a CSV usando coma como delimitador
'					Dim delimiter As String = ","
'					Dim sb As New System.Text.StringBuilder()
'					' Escribir encabezados
'					For i As Integer = 0 To dt.Columns.Count - 1
'					    sb.Append("""" & dt.Columns(i).ColumnName.Replace("""", """""") & """")
'					    If i < dt.Columns.Count - 1 Then sb.Append(delimiter)
'					Next
'					sb.AppendLine()
'					' Escribir filas
'					For Each row As DataRow In dt.Rows
'					    For i As Integer = 0 To dt.Columns.Count - 1
'					        sb.Append("""" & row(i).ToString().Replace("""", """""") & """")
'					        If i < dt.Columns.Count - 1 Then sb.Append(delimiter)
'					    Next
'					    sb.AppendLine()
'					Next
					
'					' 3. Prepara el contenido del archivo en UTF-8 con BOM
'					Dim utf8Bom As Byte() = New Byte() {&HEF, &HBB, &HBF}
'					Dim fileContentRaw As Byte() = System.Text.Encoding.UTF8.GetBytes(sb.ToString())
'					Dim fileContent As Byte() = New Byte(utf8Bom.Length + fileContentRaw.Length - 1) {}
'					Buffer.BlockCopy(utf8Bom, 0, fileContent, 0, utf8Bom.Length)
'					Buffer.BlockCopy(fileContentRaw, 0, fileContent, utf8Bom.Length, fileContentRaw.Length)
					
'					' 4. Define nombres y rutas
'					Dim fileName As String = "XFC_HR_AUX_GLB_DescConceptos_" & Now.ToString("yyyyMMdd_HHmmss") & ".csv"
'					Dim folderPath As String = "Documents/Users/Nova/WPP/TablesExport/"
'					Dim userName As String = si.AuthToken.UserName
					
'					' 5. Crea el objeto XFFileInfo
'					Dim fileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, fileName, folderPath)
'					fileInfo.ContentFileExtension = "csv"
'					fileInfo.Description = "Loaded by " & userName
'					fileInfo.ContentFileContainsData = True
'					fileInfo.XFFileType = True
					
'					' 6. Crea el objeto XFFile
'					Dim fileFile As New XFFile(fileInfo, String.Empty, fileContent)
					
'					' 7. Inserta o actualiza el archivo en el File Share
'					BRApi.FileSystem.InsertOrUpdateFile(si, fileFile)
					#End Region
					
					#Region "EXPORT XFC_HR_AUX_CS_DescPuestos"
'				    ' EXPORT TABLA
'				    ' Business Rule: Exportar tabla XFC_HR_AUX_CS_DescPuestos a Excel en File Share
				
'				    ' 1. Ejecutar consulta SQL y obtener DataTable
'				    Dim sql As String = "SELECT * FROM XFC_HR_AUX_CS_DescPuestos"
'				    Dim dt As DataTable
'				    Using dbConnApp As DbConnInfo = BRApi.Database.CreateApplicationDbConnInfo(si)
'				        dt = BRApi.Database.ExecuteSql(dbConnApp, sql, True)
'				    End Using
				
'				    ' 2. Convertir DataTable a CSV usando coma como delimitador
'				    Dim delimiter As String = ","
'				    Dim sb As New System.Text.StringBuilder()
'				    ' Escribir encabezados
'				    For i As Integer = 0 To dt.Columns.Count - 1
'				        sb.Append("""" & dt.Columns(i).ColumnName.Replace("""", """""") & """")
'				        If i < dt.Columns.Count - 1 Then sb.Append(delimiter)
'				    Next
'				    sb.AppendLine()
'				    ' Escribir filas
'				    For Each row As DataRow In dt.Rows
'				        For i As Integer = 0 To dt.Columns.Count - 1
'				            sb.Append("""" & row(i).ToString().Replace("""", """""") & """")
'				            If i < dt.Columns.Count - 1 Then sb.Append(delimiter)
'				        Next
'				        sb.AppendLine()
'				    Next
				
'				    ' 3. Prepara el contenido del archivo en UTF-8 con BOM
'				    Dim utf8Bom As Byte() = New Byte() {&HEF, &HBB, &HBF}
'				    Dim fileContentRaw As Byte() = System.Text.Encoding.UTF8.GetBytes(sb.ToString())
'				    Dim fileContent As Byte() = New Byte(utf8Bom.Length + fileContentRaw.Length - 1) {}
'				    Buffer.BlockCopy(utf8Bom, 0, fileContent, 0, utf8Bom.Length)
'				    Buffer.BlockCopy(fileContentRaw, 0, fileContent, utf8Bom.Length, fileContentRaw.Length)
				
'				    ' 4. Define nombres y rutas
'				    Dim fileName As String = "XFC_HR_AUX_CS_DescPuestos_" & Now.ToString("yyyyMMdd_HHmmss") & ".csv"
'				    Dim folderPath As String = "Documents/Users/Nova/WPP/TablesExport/"
'				    Dim userName As String = si.AuthToken.UserName
				
'				    ' 5. Crea el objeto XFFileInfo
'				    Dim fileInfo As New XFFileInfo(FileSystemLocation.ApplicationDatabase, fileName, folderPath)
'				    fileInfo.ContentFileExtension = "csv"
'				    fileInfo.Description = "Loaded by " & userName
'				    fileInfo.ContentFileContainsData = True
'				    fileInfo.XFFileType = True
				
'				    ' 6. Crea el objeto XFFile
'				    Dim fileFile As New XFFile(fileInfo, String.Empty, fileContent)
				
'				    ' 7. Inserta o actualiza el archivo en el File Share
'				    BRApi.FileSystem.InsertOrUpdateFile(si, fileFile)
				#End Region


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
	End Class
End Namespace