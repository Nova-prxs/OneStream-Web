using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
	#region Create Query JOB
	public class create_database
	{
        public object CreateDatabase(SessionInfo si, int iAction)
        {
            try
            {
				
				string sQueryCreate = $@"
					SET NOCOUNT ON;
						
						DECLARE @DropAll INT = {iAction};  -- =1 para borrar todo y recrear (excepto dbo.XFC_CEBES)
						
						/* ============================================================
						   DROP CONTROLADO (solo mis tablas)
						   ============================================================ */
						IF @DropAll >= 1
						BEGIN
						
						    /* ========= DROP FOREIGN KEYS PRIMERO ========= */
						
						    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_MASTER_GLB_MapeoCuentaContable_Conceptos')
						        ALTER TABLE dbo.XFC_MASTER_GLB_MapeoCuentaContable
						        DROP CONSTRAINT FK_XFC_MASTER_GLB_MapeoCuentaContable_Conceptos;
						
						    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_MASTER_GLB_Empleados_CECOAlta')
						        ALTER TABLE dbo.XFC_MASTER_GLB_Empleados
						        DROP CONSTRAINT FK_XFC_MASTER_GLB_Empleados_CECOAlta;
						
						    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_RAW_GLB_DistribCECO_CECO')
						        ALTER TABLE dbo.XFC_RAW_GLB_DistribCECO
						        DROP CONSTRAINT FK_XFC_RAW_GLB_DistribCECO_CECO;
						
						    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_RAW_GLB_NominaReal_CECOAlta')
						        ALTER TABLE dbo.XFC_RAW_GLB_NominaReal
						        DROP CONSTRAINT FK_XFC_RAW_GLB_NominaReal_CECOAlta;
						
						    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_MASTER_CS_Estructura_IDPadre')
						        ALTER TABLE dbo.XFC_MASTER_CS_Estructura
						        DROP CONSTRAINT FK_XFC_MASTER_CS_Estructura_IDPadre;
						
						    /* ========= DROP TABLES ========= */
						
						    IF OBJECT_ID('dbo.XFC_MASTER_GLB_MapeoCuentaContable','U') IS NOT NULL
						        DROP TABLE dbo.XFC_MASTER_GLB_MapeoCuentaContable;
						
						    IF OBJECT_ID('dbo.XFC_RAW_GLB_NominaTeorica','U') IS NOT NULL
						        DROP TABLE dbo.XFC_RAW_GLB_NominaTeorica;
						
						    IF OBJECT_ID('dbo.XFC_RAW_GLB_NominaReal','U') IS NOT NULL
						        DROP TABLE dbo.XFC_RAW_GLB_NominaReal;
						
						    IF OBJECT_ID('dbo.XFC_RAW_GLB_Absentismo','U') IS NOT NULL
						        DROP TABLE dbo.XFC_RAW_GLB_Absentismo;
						
						    IF OBJECT_ID('dbo.XFC_RAW_GLB_DistribCECO','U') IS NOT NULL
						        DROP TABLE dbo.XFC_RAW_GLB_DistribCECO;
						
						    IF OBJECT_ID('dbo.XFC_MASTER_GLB_Empleados','U') IS NOT NULL
						        DROP TABLE dbo.XFC_MASTER_GLB_Empleados;
						
						    IF OBJECT_ID('dbo.XFC_MASTER_CS_Estructura','U') IS NOT NULL
						        DROP TABLE dbo.XFC_MASTER_CS_Estructura;
						
						    IF OBJECT_ID('dbo.XFC_MASTER_GLB_Conceptos','U') IS NOT NULL
						        DROP TABLE dbo.XFC_MASTER_GLB_Conceptos;
						
						    IF OBJECT_ID('dbo.XFC_MASTER_GLB_CECO','U') IS NOT NULL
						        DROP TABLE dbo.XFC_MASTER_GLB_CECO;
							
							IF OBJECT_ID('dbo.XFC_WPP_MAIN_MASTER_TABLEIMPORTINFO','U') IS NOT NULL
						        DROP TABLE dbo.XFC_WPP_MAIN_MASTER_TABLEIMPORTINFO;
						END
						
						IF @DropAll = 2
						BEGIN
						
							/* ============================================================
							   1) XFC_MASTER_GLB_CECO
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_MASTER_GLB_CECO','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_MASTER_GLB_CECO (
							        [cebe]             VARCHAR(50) NOT NULL,
							        [cebe_description] VARCHAR(50) NULL,
							        CONSTRAINT [PK_XFC_MASTER_GLB_CECO] PRIMARY KEY ([cebe])
							    );
							END
							
							
							/* ============================================================
							   2) XFC_MASTER_GLB_Conceptos
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_MASTER_GLB_Conceptos','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_MASTER_GLB_Conceptos (
							        [id_concepto]           VARCHAR(50) NOT NULL,
							        [pais]                  VARCHAR(50) NOT NULL,
							        [descripcion_concepto]  VARCHAR(50) NULL,
							        [contabilidad]          VARCHAR(50) NULL,
							        CONSTRAINT [PK_XFC_MASTER_GLB_Conceptos] PRIMARY KEY ([id_concepto], [pais])
							    );
							END
							
							
							/* ============================================================
							   3) XFC_MASTER_CS_Estructura
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_MASTER_CS_Estructura','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_MASTER_CS_Estructura (
							        [id_miembro]                   INT NOT NULL,
							        [descripcion_miembro]          VARCHAR(50) NULL,
							        [tipo_estructura_organizativa] VARCHAR(50) NULL,
							        [id_padre]                     INT NULL,
							        [estado_puesto]                VARCHAR(50) NULL,
							        [colectivo]                    VARCHAR(50) NULL,
							        [id_funcion]                   VARCHAR(50) NULL,
							        CONSTRAINT [PK_XFC_MASTER_CS_Estructura] PRIMARY KEY ([id_miembro])
							    );
							END
							
							IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_MASTER_CS_Estructura_IDPadre')
							BEGIN
							    ALTER TABLE dbo.XFC_MASTER_CS_Estructura
							    ADD CONSTRAINT [FK_XFC_MASTER_CS_Estructura_IDPadre]
							    FOREIGN KEY ([id_padre])
							    REFERENCES dbo.XFC_MASTER_CS_Estructura ([id_miembro]);
							END
							
							
							/* ============================================================
							   4) XFC_MASTER_GLB_Empleados
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_MASTER_GLB_Empleados','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_MASTER_GLB_Empleados (
							        [id_empleado]                 INT NOT NULL,
							        [periodo]                     DATE NOT NULL,
							        [area_personal]               VARCHAR(50) NULL,
							        [id_puesto]                   VARCHAR(50) NULL,
							        [id_unidad_organizativa]      VARCHAR(50) NULL,
							        [status]                      VARCHAR(50) NULL,
							        [marca]                       VARCHAR(50) NULL,
							        [divP]                        VARCHAR(50) NULL,
							        [unidad]                      VARCHAR(50) NULL,
							        [texto_unidad]                VARCHAR(50) NULL,
							        [nombre_apellido]             VARCHAR(50) NULL,
							        [horas_jornada_contrato]      FLOAT NULL,
							        [porcentaje_jornada_contrato] FLOAT NULL,
							        [hora_anuales_contrato]       FLOAT NULL,
							        [grupo_profesional]           VARCHAR(50) NULL,
							        [sociedad]                    VARCHAR(50) NULL,
							        [sexo]                        VARCHAR(50) NULL,
							        [fecha_nacimiento]            DATE NULL,
							        [provincia_tributacion]       VARCHAR(50) NULL,
							        [tipo_contrato]               VARCHAR(50) NULL,
							        [CNAE]                        VARCHAR(50) NULL,
							        [clave_ocupacion]             VARCHAR(50) NULL,
							        [id_cebe_alta]                VARCHAR(50) NULL,
							        [texto_categoria]             VARCHAR(50) NULL,
							        [texto_subcategoria]          VARCHAR(50) NULL,
							        [texto_puesto_trabajo]        VARCHAR(50) NULL,
							        [region]                      VARCHAR(50) NULL,
							        [zona]                        VARCHAR(50) NULL,
							        [sub_zona]                    VARCHAR(50) NULL,
							        [antiguedad]                  DATE NULL,
							        [fecha_baja]                  DATE NULL,
							        [motivo_baja]                 VARCHAR(50) NULL,
							        [liberados]                   VARCHAR(50) NULL,
							        [pais]                        VARCHAR(50) NULL,
							        CONSTRAINT [PK_XFC_MASTER_GLB_Empleados] PRIMARY KEY ([id_empleado], [periodo])
							    );
							END
							
							IF OBJECT_ID('dbo.XFC_CEBES','U') IS NOT NULL
							AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_MASTER_GLB_Empleados_CECOAlta')
							BEGIN
							    ALTER TABLE dbo.XFC_MASTER_GLB_Empleados
							    ADD CONSTRAINT [FK_XFC_MASTER_GLB_Empleados_CECOAlta]
							    FOREIGN KEY ([id_cebe_alta])
							    REFERENCES dbo.XFC_CEBES ([cebe]);
							END
							
							
							/* ============================================================
							   5) XFC_RAW_GLB_DistribCECO
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_RAW_GLB_DistribCECO','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_RAW_GLB_DistribCECO (
							        [id_empleado]             INT NOT NULL,
							        [cebe]                    VARCHAR(50) NOT NULL,
							        [porcentaje_distribucion] FLOAT NULL,
							        [fecha_inicio]            DATE NOT NULL,
							        [fecha_fin]               DATE NULL,
							        CONSTRAINT [PK_XFC_RAW_GLB_DistribCECO]
							            PRIMARY KEY ([id_empleado], [cebe], [fecha_inicio])
							    );
							END
							
							IF OBJECT_ID('dbo.XFC_CEBES','U') IS NOT NULL
							AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_RAW_GLB_DistribCECO_CECO')
							BEGIN
							    ALTER TABLE dbo.XFC_RAW_GLB_DistribCECO
							    ADD CONSTRAINT [FK_XFC_RAW_GLB_DistribCECO_CECO]
							    FOREIGN KEY ([cebe])
							    REFERENCES dbo.XFC_CEBES ([cebe]);
							END
							
							
							/* ============================================================
							   6) XFC_RAW_GLB_Absentismo
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_RAW_GLB_Absentismo','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_RAW_GLB_Absentismo (
							        [id_empleado]        	INT NOT NULL,
							        [sub_tipo_absentismo] 	VARCHAR(50) NOT NULL,
							        [fecha_inicio]       	DATE NOT NULL,
							        [fecha_fin]          	DATE NULL,
							        CONSTRAINT [PK_XFC_RAW_GLB_Absentismo]
							            PRIMARY KEY ([id_empleado], [sub_tipo_absentismo], [fecha_inicio])
							    );
							END
							
							
							/* ============================================================
							   7) XFC_RAW_GLB_NominaReal
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_RAW_GLB_NominaReal','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_RAW_GLB_NominaReal (
							        [id_empleado]    INT NOT NULL,
							        [periodo_nomina] VARCHAR(50) NOT NULL,
							        [id_cebe_alta]   VARCHAR(50) NOT NULL,
							        [periodo_inicio] DATE NULL,
							        [periodo_fin]    DATE NULL,
							        [id_concepto]    VARCHAR(50) NOT NULL,
							        [cantidad]       FLOAT NULL,
							        [importe]        FLOAT NULL,
							        CONSTRAINT [PK_XFC_RAW_GLB_NominaReal]
							            PRIMARY KEY ([id_empleado], [id_cebe_alta], [periodo_nomina], [id_concepto])
							    );
							END
							
							IF OBJECT_ID('dbo.XFC_CEBES','U') IS NOT NULL
							AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_RAW_GLB_NominaReal_CECOAlta')
							BEGIN
							    ALTER TABLE dbo.XFC_RAW_GLB_NominaReal
							    ADD CONSTRAINT [FK_XFC_RAW_GLB_NominaReal_CECOAlta]
							    FOREIGN KEY ([id_cebe_alta])
							    REFERENCES dbo.XFC_CEBES ([cebe]);
							END
							
							
							/* ============================================================
							   8) XFC_RAW_GLB_NominaTeorica
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_RAW_GLB_NominaTeorica','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_RAW_GLB_NominaTeorica (
							        [id_empleado]  INT NOT NULL,
							        [periodo]      VARCHAR(50) NOT NULL,
							        [id_concepto]  VARCHAR(50) NOT NULL,
							        [fecha_inicio] DATE NOT NULL,
							        [fecha_fin]    DATE NULL,
							        [importe]      FLOAT NULL,
							        CONSTRAINT [PK_XFC_RAW_GLB_NominaTeorica]
							            PRIMARY KEY ([id_empleado], [periodo], [id_concepto], [fecha_inicio])
							    );
							END
							
							
							/* ============================================================
							   9) XFC_MASTER_GLB_MapeoCuentaContable
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_MASTER_GLB_MapeoCuentaContable','U') IS NULL
							BEGIN
							    CREATE TABLE dbo.XFC_MASTER_GLB_MapeoCuentaContable (
							        [id_concepto]     VARCHAR(50) NOT NULL,
							        [pais]            VARCHAR(50) NOT NULL,
							        [categoria]       VARCHAR(50) NOT NULL,
							        [agrupador]       VARCHAR(50) NULL,
							        [cuenta_contable] VARCHAR(50) NOT NULL,
							        CONSTRAINT [PK_XFC_MASTER_GLB_MapeoCuentaContable]
							            PRIMARY KEY ([id_concepto], [pais], [categoria], [cuenta_contable])
							    );
							END
							
							IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_XFC_MASTER_GLB_MapeoCuentaContable_Conceptos')
							BEGIN
							    ALTER TABLE dbo.XFC_MASTER_GLB_MapeoCuentaContable
							    ADD CONSTRAINT [FK_XFC_MASTER_GLB_MapeoCuentaContable_Conceptos]
							    FOREIGN KEY ([id_concepto], [pais])
							    REFERENCES dbo.XFC_MASTER_GLB_Conceptos ([id_concepto], [pais]);
							END
					
							/* ============================================================
							   10) XFC_WPP_MAIN_MASTER_TABLEIMPORTINFO
							   ============================================================ */
							IF OBJECT_ID('dbo.XFC_WPP_MAIN_MASTER_TABLEIMPORTINFO','U') IS NULL
							BEGIN
							    -- Tabla de control de importaciones.
							    -- Objetivo: guardar quién importó qué archivo, desde qué carpeta y cuándo.
							    -- Nota: permite NULL en todas las columnas para máxima flexibilidad operativa.
							    CREATE TABLE dbo.XFC_WPP_MAIN_MASTER_TABLEIMPORTINFO (
							        [table_name]        VARCHAR(50)  NULL, -- Nombre de la tabla destino de la importación
							        [user_name]         VARCHAR(50)  NULL, -- Usuario que ejecutó la carga
							        [folder_path]       VARCHAR(MAX) NULL, -- Ruta de carpeta origen del archivo
							        [file_name]         VARCHAR(150) NULL, -- Nombre del archivo importado
							        [last_import_date]  DATE         NULL  -- Fecha de la última importación
							    );
							END
						END
					";				
				using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
					{
						BRApi.Database.ExecuteActionQuery(dbConn, sQueryCreate, true, true);					
					}
                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
	}
	#endregion
	
	#region Schema Helper
	/// <summary>
	/// Helper para generar, guardar y ejecutar scripts SQL de esquema
	/// a partir de metadatos en CSV o plantillas Excel.
	///
	/// Objetivo principal:
	/// - Simplificar creación/alteración/borrado de tablas.
	/// - Mantener un flujo estándar y repetible para usuarios funcionales y técnicos.
	///
	/// Mapa rápido de funciones (categorizado):
	///
	/// 1) Generación de scripts
	/// - GenerateCreateSchemaScript: genera CREATE (tablas, PK, FK).
	/// - GenerateCreateAndDeleteScripts: genera CREATE + DELETE.
	/// - GenerateDeleteSchemaScript: genera DELETE de datos (orden FK-safe).
	/// - GenerateDropSchemaScript: genera DROP de esquema (FKs y tablas).
	/// - GenerateAlterSchemaScript: genera ALTER desde CSV de operaciones.
	///
	/// 2) Guardado y ejecución
	/// - SaveScriptToApplicationDb: guarda script en Application Database.
	/// - ExecuteScript: ejecuta script SQL.
	/// - GenerateAndSaveScript / GenerateAndSaveScripts: genera y guarda.
	/// - GenerateSaveAndExecute: genera, guarda y ejecuta CREATE.
	/// - GenerateSaveAndExecuteDelete: genera, guarda y ejecuta DELETE.
	/// - GenerateAndSaveDropScript / GenerateSaveAndExecuteDrop: flujo DROP.
	/// - GenerateAndSaveAlterScript / GenerateSaveAndExecuteAlter: flujo ALTER.
	///
	/// 3) Procesamiento de plantillas Excel
	/// - ProcessAlterTemplatesFromFolder: procesa plantillas "instructions/template",
	///   genera ALTER, opcionalmente ejecuta y renombra archivos a _Ok/_Error.
	///
	/// 4) Helpers internos principales
	/// - Lectura/parseo: ReadApplicationDbTextFile, ParseCsvToRows, ParseCsv,
	///   ParseTables, ParseColumns, ParsePrimaryKeys, ParseForeignKeys,
	///   ParseAlterTableOperations, ParseAlterTemplateExcel.
	/// - Construcción SQL: BuildCreateSchemaScript, BuildDeleteDataScript,
	///   BuildDropSchemaScript, BuildAlterSchemaScript.
	/// - Soporte: BuildTypeDefinition, VisitNode, Get/ParseInt/ParseBool/ParsePipeList,
	///   utilidades Excel e instrucciones, y renombrado de archivos procesados.
	/// </summary>
	public class SchemaCsvSqlHelper
	{
		/// <summary>
		/// Resultado combinado de generación de scripts principales.
		/// </summary>
		public sealed class SchemaScriptsResult
		{
			public string CreateScript { get; set; }
			public string DeleteScript { get; set; }
		}

		private sealed class TableDef
		{
			public string TableName { get; set; }
			public string SchemaName { get; set; }
			public int CreateOrder { get; set; }
			public int DropOrder { get; set; }
		}

		private sealed class ColumnDef
		{
			public string TableName { get; set; }
			public string ColumnName { get; set; }
			public int OrdinalPosition { get; set; }
			public string DataType { get; set; }
			public string Length { get; set; }
			public bool IsNullable { get; set; }
			public bool IsIdentity { get; set; }
			public string DefaultValue { get; set; }
		}

		private sealed class PrimaryKeyDef
		{
			public string TableName { get; set; }
			public string PrimaryKeyName { get; set; }
			public int KeyOrdinal { get; set; }
			public string ColumnName { get; set; }
		}

		private sealed class ForeignKeyDef
		{
			public string ForeignKeyName { get; set; }
			public string FromTable { get; set; }
			public int FromColumnOrdinal { get; set; }
			public string FromColumn { get; set; }
			public string ToTable { get; set; }
			public string ToColumn { get; set; }
			public string OnDelete { get; set; }
			public string OnUpdate { get; set; }
		}

		private sealed class AlterTableDef
		{
			public int ExecuteOrder { get; set; }
			public bool IsEnabled { get; set; }
			public string OperationType { get; set; }
			public string SchemaName { get; set; }
			public string TableName { get; set; }
			public string ColumnName { get; set; }
			public string DataType { get; set; }
			public string Length { get; set; }
			public bool IsNullable { get; set; }
			public string DefaultValue { get; set; }
			public string ConstraintName { get; set; }
			public string KeyColumns { get; set; }
			public string RefTable { get; set; }
			public string RefColumns { get; set; }
			public string OnDelete { get; set; }
			public string OnUpdate { get; set; }
		}

		/// <summary>
		/// Resultado de procesar un archivo de plantilla de alteraciones.
		/// </summary>
		public sealed class AlterTemplateProcessResult
		{
			public string FileName { get; set; }
			public bool IsSuccess { get; set; }
			public string Status { get; set; }
			public string Message { get; set; }
		}

		/// <summary>
		/// Genera solo el script de creación de esquema (CREATE TABLE + PK + FK).
		/// </summary>
		public string GenerateCreateSchemaScript(
			SessionInfo si,
			string csvFolderPath,
			string tablesFileName = "SchemaProposal_v1_Tables.csv",
			string columnsFileName = "SchemaProposal_v1_Columns.csv",
			string primaryKeysFileName = "SchemaProposal_v1_PrimaryKeys.csv",
			string foreignKeysFileName = "SchemaProposal_v1_ForeignKeys.csv")
		{
			return GenerateCreateAndDeleteScripts(
				si,
				csvFolderPath,
				tablesFileName,
				columnsFileName,
				primaryKeysFileName,
				foreignKeysFileName).CreateScript;
		}

		/// <summary>
		/// Genera script de creación y de borrado de datos en una sola operación.
		/// </summary>
		public SchemaScriptsResult GenerateCreateAndDeleteScripts(
			SessionInfo si,
			string csvFolderPath,
			string tablesFileName = "SchemaProposal_v1_Tables.csv",
			string columnsFileName = "SchemaProposal_v1_Columns.csv",
			string primaryKeysFileName = "SchemaProposal_v1_PrimaryKeys.csv",
			string foreignKeysFileName = "SchemaProposal_v1_ForeignKeys.csv")
		{
			string tablesCsv = ReadApplicationDbTextFile(si, csvFolderPath, tablesFileName);
			string columnsCsv = ReadApplicationDbTextFile(si, csvFolderPath, columnsFileName);
			string pksCsv = ReadApplicationDbTextFile(si, csvFolderPath, primaryKeysFileName);
			string fksCsv = ReadApplicationDbTextFile(si, csvFolderPath, foreignKeysFileName);

			List<TableDef> tables = ParseTables(tablesCsv);
			List<ColumnDef> columns = ParseColumns(columnsCsv);
			List<PrimaryKeyDef> primaryKeys = ParsePrimaryKeys(pksCsv);
			List<ForeignKeyDef> foreignKeys = ParseForeignKeys(fksCsv);

			return new SchemaScriptsResult
			{
				CreateScript = BuildCreateSchemaScript(tables, columns, primaryKeys, foreignKeys),
				DeleteScript = BuildDeleteDataScript(tables, foreignKeys)
			};
		}

		/// <summary>
		/// Genera script de borrado de datos respetando orden seguro por dependencias FK.
		/// </summary>
		public string GenerateDeleteSchemaScript(
			SessionInfo si,
			string csvFolderPath,
			string tablesFileName = "SchemaProposal_v1_Tables.csv",
			string foreignKeysFileName = "SchemaProposal_v1_ForeignKeys.csv")
		{
			string tablesCsv = ReadApplicationDbTextFile(si, csvFolderPath, tablesFileName);
			string fksCsv = ReadApplicationDbTextFile(si, csvFolderPath, foreignKeysFileName);

			List<TableDef> tables = ParseTables(tablesCsv);
			List<ForeignKeyDef> foreignKeys = ParseForeignKeys(fksCsv);

			return BuildDeleteDataScript(tables, foreignKeys);
		}

		/// <summary>
		/// Genera script de drop de esquema (elimina FKs primero y luego tablas).
		/// </summary>
		public string GenerateDropSchemaScript(
			SessionInfo si,
			string csvFolderPath,
			string tablesFileName = "SchemaProposal_v1_Tables.csv",
			string foreignKeysFileName = "SchemaProposal_v1_ForeignKeys.csv")
		{
			string tablesCsv = ReadApplicationDbTextFile(si, csvFolderPath, tablesFileName);
			string fksCsv = ReadApplicationDbTextFile(si, csvFolderPath, foreignKeysFileName);

			List<TableDef> tables = ParseTables(tablesCsv);
			List<ForeignKeyDef> foreignKeys = ParseForeignKeys(fksCsv);

			return BuildDropSchemaScript(tables, foreignKeys);
		}

		/// <summary>
		/// Genera script ALTER TABLE desde archivo CSV de operaciones.
		/// </summary>
		public string GenerateAlterSchemaScript(
			SessionInfo si,
			string csvFolderPath,
			string alterTableFileName = "SchemaProposal_v1_AlterTable.csv")
		{
			string alterCsv = ReadApplicationDbTextFile(si, csvFolderPath, alterTableFileName);
			List<AlterTableDef> operations = ParseAlterTableOperations(alterCsv);
			return BuildAlterSchemaScript(operations);
		}

		/// <summary>
		/// Guarda un script SQL en Application Database.
		/// </summary>
		public void SaveScriptToApplicationDb(
			SessionInfo si,
			string destinationFolder,
			string fileName,
			string script)
		{
			XFFolderEx folderEx = BRApi.FileSystem.GetFolder(
				si,
				FileSystemLocation.ApplicationDatabase,
				destinationFolder);

			byte[] bytes = Encoding.UTF8.GetBytes(script ?? string.Empty);
			XFFileInfo fileInfo = new XFFileInfo(
				FileSystemLocation.ApplicationDatabase,
				fileName,
				folderEx.XFFolder.FullName);

			fileInfo.ContentFileExtension = "sql";
			XFFile file = new XFFile(fileInfo, string.Empty, bytes);
			BRApi.FileSystem.InsertOrUpdateFile(si, file);
		}

		/// <summary>
		/// Ejecuta un script SQL sobre Application DB.
		/// </summary>
		public void ExecuteScript(SessionInfo si, string script)
		{
			using (DbConnInfo dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				BRApi.Database.ExecuteActionQuery(dbConnApp, script, true, true);
			}
		}

		/// <summary>
		/// Genera y guarda script de creación de esquema.
		/// </summary>
		public string GenerateAndSaveScript(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputFileName = "Generated_CreateSchema.sql")
		{
			string script = GenerateCreateSchemaScript(si, csvFolderPath);
			SaveScriptToApplicationDb(si, destinationFolder, outputFileName, script);
			return script;
		}

		/// <summary>
		/// Genera y guarda scripts de creación y borrado.
		/// </summary>
		public SchemaScriptsResult GenerateAndSaveScripts(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputCreateFileName = "Generated_CreateSchema.sql",
			string outputDeleteFileName = "Generated_DeleteData.sql")
		{
			SchemaScriptsResult scripts = GenerateCreateAndDeleteScripts(si, csvFolderPath);
			SaveScriptToApplicationDb(si, destinationFolder, outputCreateFileName, scripts.CreateScript);
			SaveScriptToApplicationDb(si, destinationFolder, outputDeleteFileName, scripts.DeleteScript);
			return scripts;
		}

		/// <summary>
		/// Genera, guarda y ejecuta script de creación.
		/// </summary>
		public string GenerateSaveAndExecute(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputFileName = "Generated_CreateSchema.sql")
		{
			string script = GenerateAndSaveScript(si, csvFolderPath, destinationFolder, outputFileName);
			ExecuteScript(si, script);
			return script;
		}

		/// <summary>
		/// Genera, guarda y ejecuta solo el script de borrado de datos.
		/// </summary>
		public string GenerateSaveAndExecuteDelete(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputDeleteFileName = "Generated_DeleteData.sql")
		{
			SchemaScriptsResult scripts = GenerateAndSaveScripts(
				si,
				csvFolderPath,
				destinationFolder,
				"Generated_CreateSchema.sql",
				outputDeleteFileName);

			ExecuteScript(si, scripts.DeleteScript);
			return scripts.DeleteScript;
		}

		/// <summary>
		/// Genera y guarda script de drop de esquema.
		/// </summary>
		public string GenerateAndSaveDropScript(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputFileName = "Generated_DropSchema.sql")
		{
			string script = GenerateDropSchemaScript(si, csvFolderPath);
			SaveScriptToApplicationDb(si, destinationFolder, outputFileName, script);
			return script;
		}

		/// <summary>
		/// Genera, guarda y ejecuta script de drop de esquema.
		/// </summary>
		public string GenerateSaveAndExecuteDrop(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputFileName = "Generated_DropSchema.sql")
		{
			string script = GenerateAndSaveDropScript(si, csvFolderPath, destinationFolder, outputFileName);
			ExecuteScript(si, script);
			return script;
		}

		/// <summary>
		/// Genera y guarda script ALTER TABLE.
		/// </summary>
		public string GenerateAndSaveAlterScript(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputFileName = "Generated_AlterSchema.sql")
		{
			string script = GenerateAlterSchemaScript(si, csvFolderPath);
			SaveScriptToApplicationDb(si, destinationFolder, outputFileName, script);
			return script;
		}

		/// <summary>
		/// Genera, guarda y ejecuta script ALTER TABLE.
		/// </summary>
		public string GenerateSaveAndExecuteAlter(
			SessionInfo si,
			string csvFolderPath,
			string destinationFolder,
			string outputFileName = "Generated_AlterSchema.sql")
		{
			string script = GenerateAndSaveAlterScript(si, csvFolderPath, destinationFolder, outputFileName);
			ExecuteScript(si, script);
			return script;
		}

		/// <summary>
		/// Procesa plantillas Excel desde una carpeta:
		/// - Lee instrucciones + operaciones de la plantilla.
		/// - Genera script alter.
		/// - Opcionalmente ejecuta.
		/// - Renombra archivo fuente con sufijo _Ok o _Error.
		/// </summary>
		public List<AlterTemplateProcessResult> ProcessAlterTemplatesFromFolder(
			SessionInfo si,
			string sourceFolderPath,
			string generatedScriptFolder,
			bool executeScript = true)
		{
			List<string> extensions = new List<string> { "xlsx" };
			List<XFFileInfoEx> files = BRApi.FileSystem.GetFilesInFolder(
				si,
				FileSystemLocation.ApplicationDatabase,
				sourceFolderPath,
				XFFileType.All,
				extensions);

			List<AlterTemplateProcessResult> results = new List<AlterTemplateProcessResult>();

			foreach (XFFileInfoEx fileEx in files.OrderBy(f => f.XFFileInfo.Name, StringComparer.OrdinalIgnoreCase))
			{
				string originalName = fileEx.XFFileInfo.Name;
				if (HasProcessedSuffix(originalName))
				{
					continue;
				}

				string originalPath = $"{sourceFolderPath.TrimEnd('/')}/{originalName}";
				AlterTemplateProcessResult rowResult = new AlterTemplateProcessResult
				{
					FileName = originalName,
					IsSuccess = false,
					Status = "Error",
					Message = string.Empty
				};

				try
				{
					XFFileEx file = BRApi.FileSystem.GetFile(
						si,
						FileSystemLocation.ApplicationDatabase,
						originalPath,
						true,
						true);

					Dictionary<string, string> instructions;
					List<AlterTableDef> operations = ParseAlterTemplateExcel(file.XFFile.ContentFileBytes, out instructions);

					if (operations.Count == 0)
					{
						throw new Exception("No hay operaciones válidas en la hoja template.");
					}

					bool executeFromInstructions = GetInstructionBool(instructions, "Execute", true);
					bool shouldExecute = executeScript && executeFromInstructions;

					string alterScript = BuildAlterSchemaScript(operations);
					string scriptFileName = BuildGeneratedScriptFileName(originalName, "Generated_AlterFromTemplate");
					SaveScriptToApplicationDb(si, generatedScriptFolder, scriptFileName, alterScript);

					if (shouldExecute)
					{
						ExecuteScript(si, alterScript);
					}

					string successName = BuildProcessedFileName(originalName, "_Ok");
					RenameApplicationDbFile(si, sourceFolderPath, originalName, successName, "xlsx", XFFileType.AllXFSpreadsheet);

					rowResult.IsSuccess = true;
					rowResult.Status = "Ok";
					rowResult.Message = shouldExecute
						? $"Procesado y ejecutado. Script: {generatedScriptFolder}/{scriptFileName}"
						: $"Procesado sin ejecución (Execute=0 o executeScript=false). Script: {generatedScriptFolder}/{scriptFileName}";
				}
				catch (Exception ex)
				{
					try
					{
						string errorName = BuildProcessedFileName(originalName, "_Error");
						RenameApplicationDbFile(si, sourceFolderPath, originalName, errorName, "xlsx", XFFileType.AllXFSpreadsheet);
					}
					catch
					{
					}

					rowResult.IsSuccess = false;
					rowResult.Status = "Error";
					rowResult.Message = ex.Message;
				}

				results.Add(rowResult);
			}

			return results;
		}

		/// <summary>
		/// Ejecuta únicamente el borrado de datos a partir de metadatos CSV.
		/// </summary>
		public void ExecuteDeleteOnlyFromCsv(
			SessionInfo si,
			string csvFolderPath)
		{
			string deleteScript = GenerateDeleteSchemaScript(si, csvFolderPath);
			ExecuteScript(si, deleteScript);
		}

		/// <summary>
		/// Lee el contenido de un archivo de texto desde Application DB.
		/// </summary>
		private string ReadApplicationDbTextFile(SessionInfo si, string folderPath, string fileName)
		{
			string filePath = $"{folderPath.TrimEnd('/')}/{fileName}";
			XFFileEx fileEx = BRApi.FileSystem.GetFile(
				si,
				FileSystemLocation.ApplicationDatabase,
				filePath,
				true,
				true);

			return Encoding.UTF8.GetString(fileEx.XFFile.ContentFileBytes);
		}

		/// <summary>
		/// Construye script CREATE completo:
		/// 1) tablas, 2) primary keys, 3) foreign keys.
		/// </summary>
		private string BuildCreateSchemaScript(
			List<TableDef> tables,
			List<ColumnDef> columns,
			List<PrimaryKeyDef> primaryKeys,
			List<ForeignKeyDef> foreignKeys)
		{
			Dictionary<string, TableDef> tableByName = tables
				.GroupBy(t => t.TableName, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			StringBuilder sql = new StringBuilder();
			sql.AppendLine("SET NOCOUNT ON;");
			sql.AppendLine();
			sql.AppendLine("/* =============================================");
			sql.AppendLine("   AUTO-GENERATED FROM CSV METADATA");
			sql.AppendLine("   TABLES -> COLUMNS -> PRIMARY KEYS -> FOREIGN KEYS");
			sql.AppendLine("   ============================================= */");
			sql.AppendLine();

			foreach (TableDef table in tables.OrderBy(t => t.CreateOrder))
			{
				List<ColumnDef> tableColumns = columns
					.Where(c => c.TableName.Equals(table.TableName, StringComparison.OrdinalIgnoreCase))
					.OrderBy(c => c.OrdinalPosition)
					.ToList();

				if (tableColumns.Count == 0)
				{
					continue;
				}

				sql.AppendLine($"IF OBJECT_ID('{table.SchemaName}.{table.TableName}','U') IS NULL");
				sql.AppendLine("BEGIN");
				sql.AppendLine($"    CREATE TABLE [{table.SchemaName}].[{table.TableName}] (");

				for (int i = 0; i < tableColumns.Count; i++)
				{
					ColumnDef col = tableColumns[i];
					string typeDefinition = BuildTypeDefinition(col.DataType, col.Length);
					string nullability = col.IsNullable ? "NULL" : "NOT NULL";
					string identity = col.IsIdentity ? " IDENTITY(1,1)" : string.Empty;
					string defaultClause = string.IsNullOrWhiteSpace(col.DefaultValue)
						? string.Empty
						: $" DEFAULT {col.DefaultValue}";
					string comma = i < tableColumns.Count - 1 ? "," : string.Empty;

					sql.AppendLine($"        [{col.ColumnName}] {typeDefinition}{identity} {nullability}{defaultClause}{comma}");
				}

				sql.AppendLine("    );");
				sql.AppendLine("END");
				sql.AppendLine();
			}

			foreach (IGrouping<string, PrimaryKeyDef> pkGroup in primaryKeys
				.GroupBy(pk => pk.PrimaryKeyName, StringComparer.OrdinalIgnoreCase))
			{
				PrimaryKeyDef first = pkGroup.First();
				if (!tableByName.ContainsKey(first.TableName))
				{
					continue;
				}

				TableDef table = tableByName[first.TableName];
				string pkColumns = string.Join(", ",
					pkGroup
					.OrderBy(pk => pk.KeyOrdinal)
					.Select(pk => $"[{pk.ColumnName}]"));

				sql.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE [name] = '{first.PrimaryKeyName}')");
				sql.AppendLine("BEGIN");
				sql.AppendLine($"    ALTER TABLE [{table.SchemaName}].[{table.TableName}]");
				sql.AppendLine($"    ADD CONSTRAINT [{first.PrimaryKeyName}] PRIMARY KEY ({pkColumns});");
				sql.AppendLine("END");
				sql.AppendLine();
			}

			foreach (IGrouping<string, ForeignKeyDef> fkGroup in foreignKeys
				.GroupBy(fk => fk.ForeignKeyName, StringComparer.OrdinalIgnoreCase))
			{
				ForeignKeyDef first = fkGroup.First();
				if (!tableByName.ContainsKey(first.FromTable))
				{
					continue;
				}

				TableDef fromTable = tableByName[first.FromTable];
				string toSchema = tableByName.ContainsKey(first.ToTable)
					? tableByName[first.ToTable].SchemaName
					: "dbo";

				string fromColumns = string.Join(", ",
					fkGroup
					.OrderBy(fk => fk.FromColumnOrdinal)
					.Select(fk => $"[{fk.FromColumn}]"));

				string toColumns = string.Join(", ",
					fkGroup
					.OrderBy(fk => fk.FromColumnOrdinal)
					.Select(fk => $"[{fk.ToColumn}]"));

				string onDelete = string.IsNullOrWhiteSpace(first.OnDelete) ? string.Empty : $" ON DELETE {first.OnDelete}";
				string onUpdate = string.IsNullOrWhiteSpace(first.OnUpdate) ? string.Empty : $" ON UPDATE {first.OnUpdate}";

				sql.AppendLine($"IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = '{first.ForeignKeyName}')");
				sql.AppendLine("BEGIN");
				sql.AppendLine($"    ALTER TABLE [{fromTable.SchemaName}].[{first.FromTable}]");
				sql.AppendLine($"    ADD CONSTRAINT [{first.ForeignKeyName}] FOREIGN KEY ({fromColumns})");
				sql.AppendLine($"    REFERENCES [{toSchema}].[{first.ToTable}] ({toColumns}){onDelete}{onUpdate};");
				sql.AppendLine("END");
				sql.AppendLine();
			}

			return sql.ToString();
		}

		/// <summary>
		/// Construye script de DELETE seguro en orden hijo -> padre.
		/// </summary>
		private string BuildDeleteDataScript(
			List<TableDef> tables,
			List<ForeignKeyDef> foreignKeys)
		{
			Dictionary<string, TableDef> tableByName = tables
				.GroupBy(t => t.TableName, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			Dictionary<string, List<string>> graph = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

			foreach (TableDef t in tables)
			{
				if (!graph.ContainsKey(t.TableName))
				{
					graph[t.TableName] = new List<string>();
				}
			}

			foreach (ForeignKeyDef fk in foreignKeys)
			{
				if (!graph.ContainsKey(fk.FromTable)) graph[fk.FromTable] = new List<string>();
				if (!graph.ContainsKey(fk.ToTable)) graph[fk.ToTable] = new List<string>();

				bool exists = graph[fk.FromTable].Any(x => x.Equals(fk.ToTable, StringComparison.OrdinalIgnoreCase));
				if (!exists)
				{
					graph[fk.FromTable].Add(fk.ToTable);
				}
			}

			List<string> topo = new List<string>();
			HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			foreach (string node in graph.Keys)
			{
				VisitNode(node, graph, visited, topo);
			}

			topo.Reverse();

			StringBuilder sql = new StringBuilder();
			sql.AppendLine("SET NOCOUNT ON;");
			sql.AppendLine();
			sql.AppendLine("/* =============================================");
			sql.AppendLine("   AUTO-GENERATED DELETE DATA SCRIPT");
			sql.AppendLine("   ORDER: CHILD -> PARENT (FK SAFE)");
			sql.AppendLine("   ============================================= */");
			sql.AppendLine();

			foreach (string tableName in topo)
			{
				if (!tableByName.ContainsKey(tableName))
				{
					continue;
				}

				TableDef t = tableByName[tableName];
				sql.AppendLine($"IF OBJECT_ID('{t.SchemaName}.{t.TableName}','U') IS NOT NULL");
				sql.AppendLine($"    DELETE FROM [{t.SchemaName}].[{t.TableName}];");
				sql.AppendLine();
			}

			return sql.ToString();
		}

		/// <summary>
		/// Construye script DROP eliminando primero constraints FK y luego tablas.
		/// </summary>
		private string BuildDropSchemaScript(
			List<TableDef> tables,
			List<ForeignKeyDef> foreignKeys)
		{
			Dictionary<string, TableDef> tableByName = tables
				.GroupBy(t => t.TableName, StringComparer.OrdinalIgnoreCase)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			StringBuilder sql = new StringBuilder();
			sql.AppendLine("SET NOCOUNT ON;");
			sql.AppendLine();
			sql.AppendLine("/* =============================================");
			sql.AppendLine("   AUTO-GENERATED DROP SCHEMA SCRIPT");
			sql.AppendLine("   DROPS FKs FIRST, THEN TABLES");
			sql.AppendLine("   ============================================= */");
			sql.AppendLine();

			foreach (IGrouping<string, ForeignKeyDef> fkGroup in foreignKeys
				.GroupBy(fk => fk.ForeignKeyName, StringComparer.OrdinalIgnoreCase))
			{
				ForeignKeyDef first = fkGroup.First();
				if (!tableByName.ContainsKey(first.FromTable))
				{
					continue;
				}

				TableDef fromTable = tableByName[first.FromTable];
				sql.AppendLine($"IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = '{first.ForeignKeyName}')");
				sql.AppendLine($"    ALTER TABLE [{fromTable.SchemaName}].[{first.FromTable}] DROP CONSTRAINT [{first.ForeignKeyName}];");
				sql.AppendLine();
			}

			foreach (TableDef table in tables.OrderBy(t => t.DropOrder).ThenBy(t => t.TableName, StringComparer.OrdinalIgnoreCase))
			{
				sql.AppendLine($"IF OBJECT_ID('{table.SchemaName}.{table.TableName}','U') IS NOT NULL");
				sql.AppendLine($"    DROP TABLE [{table.SchemaName}].[{table.TableName}];");
				sql.AppendLine();
			}

			return sql.ToString();
		}

		/// <summary>
		/// Construye script ALTER en modo seguro (ADD_COLUMN, ADD_PRIMARY_KEY, ADD_FOREIGN_KEY).
		/// Incluye validaciones para minimizar errores por datos existentes.
		/// </summary>
		private string BuildAlterSchemaScript(List<AlterTableDef> operations)
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendLine("SET NOCOUNT ON;");
			sql.AppendLine();
			sql.AppendLine("/* =============================================");
			sql.AppendLine("   AUTO-GENERATED ALTER TABLE SCRIPT");
			sql.AppendLine("   SAFE MODE: add columns / PK / FK when possible");
			sql.AppendLine("   ============================================= */");
			sql.AppendLine();

			foreach (AlterTableDef op in operations
				.Where(x => x.IsEnabled)
				.OrderBy(x => x.ExecuteOrder)
				.ThenBy(x => x.TableName, StringComparer.OrdinalIgnoreCase))
			{
				string schema = string.IsNullOrWhiteSpace(op.SchemaName) ? "dbo" : op.SchemaName;
				string fullTableObject = $"{schema}.{op.TableName}";
				string fullTableName = $"[{schema}].[{op.TableName}]";

				if (op.OperationType.XFEqualsIgnoreCase("ADD_COLUMN"))
				{
					string typeDefinition = BuildTypeDefinition(op.DataType, op.Length);
					string nullability = op.IsNullable ? "NULL" : "NOT NULL";
					string defaultConstraintName = string.IsNullOrWhiteSpace(op.ConstraintName)
						? $"DF_{op.TableName}_{op.ColumnName}"
						: op.ConstraintName;
					string defaultClause = string.IsNullOrWhiteSpace(op.DefaultValue)
						? string.Empty
						: $" CONSTRAINT [{defaultConstraintName}] DEFAULT {op.DefaultValue}";

					sql.AppendLine($"-- ADD_COLUMN {fullTableName}.[{op.ColumnName}]");
					sql.AppendLine($"IF OBJECT_ID('{fullTableObject}','U') IS NOT NULL");
					sql.AppendLine($"AND COL_LENGTH('{fullTableObject}','{op.ColumnName}') IS NULL");
					sql.AppendLine("BEGIN");

					if (!op.IsNullable && string.IsNullOrWhiteSpace(op.DefaultValue))
					{
						sql.AppendLine($"    IF NOT EXISTS (SELECT TOP 1 1 FROM {fullTableName})");
						sql.AppendLine($"        ALTER TABLE {fullTableName} ADD [{op.ColumnName}] {typeDefinition} {nullability};");
						sql.AppendLine("    ELSE");
						sql.AppendLine($"        PRINT 'SKIPPED ADD_COLUMN {fullTableName}.[{op.ColumnName}] -> NOT NULL sin default y la tabla tiene datos.';");
					}
					else
					{
						sql.AppendLine($"    ALTER TABLE {fullTableName} ADD [{op.ColumnName}] {typeDefinition}{defaultClause} {nullability};");
					}

					sql.AppendLine("END");
					sql.AppendLine();
				}
				else if (op.OperationType.XFEqualsIgnoreCase("ADD_PRIMARY_KEY"))
				{
					string pkName = string.IsNullOrWhiteSpace(op.ConstraintName)
						? $"PK_{op.TableName}"
						: op.ConstraintName;
					string[] keyColumns = ParsePipeList(op.KeyColumns);
					if (keyColumns.Length == 0)
					{
						continue;
					}

					string keyColumnsSql = string.Join(", ", keyColumns.Select(c => $"[{c}]"));

					sql.AppendLine($"-- ADD_PRIMARY_KEY {fullTableName} ({keyColumnsSql})");
					sql.AppendLine($"IF OBJECT_ID('{fullTableObject}','U') IS NOT NULL");
					sql.AppendLine($"AND NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE [name] = '{pkName}')");
					sql.AppendLine($"AND NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE [parent_object_id] = OBJECT_ID('{fullTableObject}') AND [type] = 'PK')");
					sql.AppendLine("BEGIN");
					sql.AppendLine($"    IF NOT EXISTS (SELECT TOP 1 1 FROM {fullTableName})");
					sql.AppendLine($"        ALTER TABLE {fullTableName} ADD CONSTRAINT [{pkName}] PRIMARY KEY ({keyColumnsSql});");
					sql.AppendLine("    ELSE");
					sql.AppendLine($"        PRINT 'SKIPPED ADD_PRIMARY_KEY {fullTableName} -> tabla con datos.';");
					sql.AppendLine("END");
					sql.AppendLine();
				}
				else if (op.OperationType.XFEqualsIgnoreCase("ADD_FOREIGN_KEY"))
				{
					string fkName = string.IsNullOrWhiteSpace(op.ConstraintName)
						? $"FK_{op.TableName}_{op.RefTable}"
						: op.ConstraintName;

					string[] fromColumns = ParsePipeList(op.KeyColumns);
					string[] toColumns = ParsePipeList(op.RefColumns);
					if (fromColumns.Length == 0 || toColumns.Length == 0 || fromColumns.Length != toColumns.Length || string.IsNullOrWhiteSpace(op.RefTable))
					{
						continue;
					}

					string refSchema = string.IsNullOrWhiteSpace(op.SchemaName) ? "dbo" : op.SchemaName;
					string refObject = $"{refSchema}.{op.RefTable}";
					string refTableName = $"[{refSchema}].[{op.RefTable}]";

					string fromColumnsSql = string.Join(", ", fromColumns.Select(c => $"[{c}]"));
					string toColumnsSql = string.Join(", ", toColumns.Select(c => $"[{c}]"));

					string onDelete = string.IsNullOrWhiteSpace(op.OnDelete) ? string.Empty : $" ON DELETE {op.OnDelete}";
					string onUpdate = string.IsNullOrWhiteSpace(op.OnUpdate) ? string.Empty : $" ON UPDATE {op.OnUpdate}";

					StringBuilder joinParts = new StringBuilder();
					for (int i = 0; i < fromColumns.Length; i++)
					{
						if (i > 0) joinParts.Append(" AND ");
						joinParts.Append($"f.[{fromColumns[i]}] = r.[{toColumns[i]}]");
					}

					string notNullFilter = string.Join(" OR ", fromColumns.Select(c => $"f.[{c}] IS NOT NULL"));

					sql.AppendLine($"-- ADD_FOREIGN_KEY {fullTableName} -> {refTableName}");
					sql.AppendLine($"IF OBJECT_ID('{fullTableObject}','U') IS NOT NULL");
					sql.AppendLine($"AND OBJECT_ID('{refObject}','U') IS NOT NULL");
					sql.AppendLine($"AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = '{fkName}')");
					sql.AppendLine("BEGIN");
					sql.AppendLine($"    IF NOT EXISTS (");
					sql.AppendLine($"        SELECT TOP 1 1");
					sql.AppendLine($"        FROM {fullTableName} f");
					sql.AppendLine($"        LEFT JOIN {refTableName} r ON {joinParts}");
					sql.AppendLine($"        WHERE ({notNullFilter}) AND r.[{toColumns[0]}] IS NULL");
					sql.AppendLine($"    )");
					sql.AppendLine($"        ALTER TABLE {fullTableName} ADD CONSTRAINT [{fkName}] FOREIGN KEY ({fromColumnsSql}) REFERENCES {refTableName} ({toColumnsSql}){onDelete}{onUpdate};");
					sql.AppendLine("    ELSE");
					sql.AppendLine($"        PRINT 'SKIPPED ADD_FOREIGN_KEY {fkName} -> existen filas huérfanas.';");
					sql.AppendLine("END");
					sql.AppendLine();
				}
			}

			return sql.ToString();
		}

		private void VisitNode(
			string node,
			Dictionary<string, List<string>> graph,
			HashSet<string> visited,
			List<string> result)
		{
			if (visited.Contains(node)) return;

			visited.Add(node);

			foreach (string parent in graph[node])
			{
				VisitNode(parent, graph, visited, result);
			}

			result.Add(node);
		}

		private string BuildTypeDefinition(string dataType, string length)
		{
			string normalizedType = (dataType ?? string.Empty).Trim().ToUpperInvariant();
			string normalizedLength = (length ?? string.Empty).Trim();

			if (string.IsNullOrWhiteSpace(normalizedLength))
			{
				return normalizedType;
			}

			if (normalizedType == "VARCHAR" || normalizedType == "NVARCHAR" || normalizedType == "CHAR" || normalizedType == "NCHAR")
			{
				return $"{normalizedType}({normalizedLength})";
			}

			return normalizedType;
		}

		private List<TableDef> ParseTables(string csvText)
		{
			List<Dictionary<string, string>> rows = ParseCsvToRows(csvText);
			List<TableDef> output = new List<TableDef>();

			foreach (Dictionary<string, string> row in rows)
			{
				output.Add(new TableDef
				{
					TableName = Get(row, "TableName"),
					SchemaName = string.IsNullOrWhiteSpace(Get(row, "SchemaName")) ? "dbo" : Get(row, "SchemaName"),
					CreateOrder = ParseInt(Get(row, "CreateOrder"), 9999),
					DropOrder = ParseInt(Get(row, "DropOrder"), 9999)
				});
			}

			return output;
		}

		private List<ColumnDef> ParseColumns(string csvText)
		{
			List<Dictionary<string, string>> rows = ParseCsvToRows(csvText);
			List<ColumnDef> output = new List<ColumnDef>();

			foreach (Dictionary<string, string> row in rows)
			{
				output.Add(new ColumnDef
				{
					TableName = Get(row, "TableName"),
					ColumnName = Get(row, "ColumnName"),
					OrdinalPosition = ParseInt(Get(row, "OrdinalPosition"), 9999),
					DataType = Get(row, "DataType"),
					Length = Get(row, "Length"),
					IsNullable = ParseBool(Get(row, "IsNullable")),
					IsIdentity = ParseBool(Get(row, "IsIdentity")),
					DefaultValue = Get(row, "DefaultValue")
				});
			}

			return output;
		}

		private List<PrimaryKeyDef> ParsePrimaryKeys(string csvText)
		{
			List<Dictionary<string, string>> rows = ParseCsvToRows(csvText);
			List<PrimaryKeyDef> output = new List<PrimaryKeyDef>();

			foreach (Dictionary<string, string> row in rows)
			{
				output.Add(new PrimaryKeyDef
				{
					TableName = Get(row, "TableName"),
					PrimaryKeyName = Get(row, "PrimaryKeyName"),
					KeyOrdinal = ParseInt(Get(row, "KeyOrdinal"), 9999),
					ColumnName = Get(row, "ColumnName")
				});
			}

			return output;
		}

		private List<ForeignKeyDef> ParseForeignKeys(string csvText)
		{
			List<Dictionary<string, string>> rows = ParseCsvToRows(csvText);
			List<ForeignKeyDef> output = new List<ForeignKeyDef>();

			foreach (Dictionary<string, string> row in rows)
			{
				output.Add(new ForeignKeyDef
				{
					ForeignKeyName = Get(row, "ForeignKeyName"),
					FromTable = Get(row, "FromTable"),
					FromColumnOrdinal = ParseInt(Get(row, "FromColumnOrdinal"), 9999),
					FromColumn = Get(row, "FromColumn"),
					ToTable = Get(row, "ToTable"),
					ToColumn = Get(row, "ToColumn"),
					OnDelete = Get(row, "OnDelete"),
					OnUpdate = Get(row, "OnUpdate")
				});
			}

			return output;
		}

		private List<AlterTableDef> ParseAlterTableOperations(string csvText)
		{
			List<Dictionary<string, string>> rows = ParseCsvToRows(csvText);
			List<AlterTableDef> output = new List<AlterTableDef>();

			foreach (Dictionary<string, string> row in rows)
			{
				output.Add(new AlterTableDef
				{
					ExecuteOrder = ParseInt(Get(row, "ExecuteOrder"), 9999),
					IsEnabled = ParseBool(Get(row, "IsEnabled")),
					OperationType = Get(row, "OperationType"),
					SchemaName = string.IsNullOrWhiteSpace(Get(row, "SchemaName")) ? "dbo" : Get(row, "SchemaName"),
					TableName = Get(row, "TableName"),
					ColumnName = Get(row, "ColumnName"),
					DataType = Get(row, "DataType"),
					Length = Get(row, "Length"),
					IsNullable = ParseBool(Get(row, "IsNullable")),
					DefaultValue = Get(row, "DefaultValue"),
					ConstraintName = Get(row, "ConstraintName"),
					KeyColumns = Get(row, "KeyColumns"),
					RefTable = Get(row, "RefTable"),
					RefColumns = Get(row, "RefColumns"),
					OnDelete = Get(row, "OnDelete"),
					OnUpdate = Get(row, "OnUpdate")
				});
			}

			return output;
		}

		private List<AlterTableDef> ParseAlterTemplateExcel(byte[] fileBytes, out Dictionary<string, string> instructions)
		{
			instructions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			List<Dictionary<string, string>> templateRows = new List<Dictionary<string, string>>();

			using (MemoryStream stream = new MemoryStream(fileBytes))
			using (SpreadsheetDocument document = SpreadsheetDocument.Open(stream, false))
			{
				WorkbookPart workbookPart = document.WorkbookPart;
				if (workbookPart == null || workbookPart.Workbook == null || workbookPart.Workbook.Sheets == null)
				{
					throw new Exception("Workbook inválido o sin hojas.");
				}

				List<Sheet> sheets = workbookPart.Workbook.Sheets.Elements<Sheet>().ToList();
				Sheet instructionsSheet = sheets.FirstOrDefault(s => string.Equals(s.Name?.Value, "instructions", StringComparison.OrdinalIgnoreCase));
				Sheet templateSheet = sheets.FirstOrDefault(s => string.Equals(s.Name?.Value, "template", StringComparison.OrdinalIgnoreCase));

				if (instructionsSheet == null)
				{
					throw new Exception("No existe la hoja 'instructions'.");
				}

				if (templateSheet == null)
				{
					throw new Exception("No existe la hoja 'template'.");
				}

				instructions = ReadInstructionsSheet(document, workbookPart, instructionsSheet);
				templateRows = ReadTemplateSheet(document, workbookPart, templateSheet);
			}

			List<AlterTableDef> output = new List<AlterTableDef>();
			foreach (Dictionary<string, string> row in templateRows)
			{
				output.Add(new AlterTableDef
				{
					ExecuteOrder = ParseInt(Get(row, "ExecuteOrder"), 9999),
					IsEnabled = ParseBool(Get(row, "IsEnabled")),
					OperationType = Get(row, "OperationType"),
					SchemaName = string.IsNullOrWhiteSpace(Get(row, "SchemaName")) ? "dbo" : Get(row, "SchemaName"),
					TableName = Get(row, "TableName"),
					ColumnName = Get(row, "ColumnName"),
					DataType = Get(row, "DataType"),
					Length = Get(row, "Length"),
					IsNullable = ParseBool(Get(row, "IsNullable")),
					DefaultValue = Get(row, "DefaultValue"),
					ConstraintName = Get(row, "ConstraintName"),
					KeyColumns = Get(row, "KeyColumns"),
					RefTable = Get(row, "RefTable"),
					RefColumns = Get(row, "RefColumns"),
					OnDelete = Get(row, "OnDelete"),
					OnUpdate = Get(row, "OnUpdate")
				});
			}

			return output;
		}

		private Dictionary<string, string> ReadInstructionsSheet(
			SpreadsheetDocument document,
			WorkbookPart workbookPart,
			Sheet sheet)
		{
			Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id.Value);
			SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
			if (sheetData == null)
			{
				return map;
			}

			foreach (Row row in sheetData.Elements<Row>())
			{
				List<Cell> cells = row.Elements<Cell>().ToList();
				if (cells.Count < 2)
				{
					continue;
				}

				string key = GetExcelCellValue(document, cells[0]).Trim();
				string value = GetExcelCellValue(document, cells[1]).Trim();

				if (string.IsNullOrWhiteSpace(key))
				{
					continue;
				}

				map[key] = value;
			}

			return map;
		}

		private List<Dictionary<string, string>> ReadTemplateSheet(
			SpreadsheetDocument document,
			WorkbookPart workbookPart,
			Sheet sheet)
		{
			WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id.Value);
			SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().FirstOrDefault();
			if (sheetData == null)
			{
				return new List<Dictionary<string, string>>();
			}

			List<Row> rows = sheetData.Elements<Row>().ToList();
			if (rows.Count == 0)
			{
				return new List<Dictionary<string, string>>();
			}

			Dictionary<int, string> headerByCol = ReadRowValuesByColumn(document, rows[0]);
			List<int> headerCols = headerByCol.Keys.OrderBy(x => x).ToList();

			List<Dictionary<string, string>> output = new List<Dictionary<string, string>>();
			for (int i = 1; i < rows.Count; i++)
			{
				Dictionary<int, string> valueByCol = ReadRowValuesByColumn(document, rows[i]);
				Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				bool hasAnyValue = false;

				foreach (int colIndex in headerCols)
				{
					string key = headerByCol[colIndex].Trim();
					if (string.IsNullOrWhiteSpace(key))
					{
						continue;
					}

					string value = valueByCol.ContainsKey(colIndex) ? valueByCol[colIndex] : string.Empty;
					if (!string.IsNullOrWhiteSpace(value))
					{
						hasAnyValue = true;
					}
					map[key] = value?.Trim() ?? string.Empty;
				}

				if (hasAnyValue)
				{
					output.Add(map);
				}
			}

			return output;
		}

		private Dictionary<int, string> ReadRowValuesByColumn(SpreadsheetDocument document, Row row)
		{
			Dictionary<int, string> output = new Dictionary<int, string>();
			foreach (Cell cell in row.Elements<Cell>())
			{
				string cellRef = cell.CellReference?.Value ?? string.Empty;
				int colIndex = GetColumnIndexFromCellReference(cellRef);
				if (colIndex < 0)
				{
					continue;
				}

				output[colIndex] = GetExcelCellValue(document, cell);
			}

			return output;
		}

		private string GetExcelCellValue(SpreadsheetDocument document, Cell cell)
		{
			if (cell == null)
			{
				return string.Empty;
			}

			string value = cell.CellValue?.InnerText ?? string.Empty;
			if (cell.DataType == null)
			{
				return value;
			}

			if (cell.DataType.Value == CellValues.SharedString)
			{
				int index;
				if (int.TryParse(value, out index) && document.WorkbookPart?.SharedStringTablePart?.SharedStringTable != null)
				{
					SharedStringItem item = document.WorkbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAtOrDefault(index);
					return item?.InnerText ?? string.Empty;
				}
			}

			if (cell.DataType.Value == CellValues.Boolean)
			{
				return value == "1" ? "true" : "false";
			}

			return value;
		}

		private int GetColumnIndexFromCellReference(string cellReference)
		{
			if (string.IsNullOrWhiteSpace(cellReference))
			{
				return -1;
			}

			int index = 0;
			for (int i = 0; i < cellReference.Length; i++)
			{
				char ch = cellReference[i];
				if (!char.IsLetter(ch))
				{
					break;
				}

				index = (index * 26) + (char.ToUpperInvariant(ch) - 'A' + 1);
			}

			return index - 1;
		}

		private bool GetInstructionBool(Dictionary<string, string> instructions, string key, bool defaultValue)
		{
			if (instructions == null || !instructions.ContainsKey(key))
			{
				return defaultValue;
			}

			string value = instructions[key];
			if (string.IsNullOrWhiteSpace(value))
			{
				return defaultValue;
			}

			string normalized = value.Trim().ToLowerInvariant();
			if (normalized == "1" || normalized == "true" || normalized == "yes" || normalized == "y")
			{
				return true;
			}

			if (normalized == "0" || normalized == "false" || normalized == "no" || normalized == "n")
			{
				return false;
			}

			return defaultValue;
		}

		private bool HasProcessedSuffix(string fileName)
		{
			if (string.IsNullOrWhiteSpace(fileName))
			{
				return false;
			}

			string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName) ?? string.Empty;
			return nameWithoutExt.EndsWith("_Ok", StringComparison.OrdinalIgnoreCase)
				|| nameWithoutExt.EndsWith("_Error", StringComparison.OrdinalIgnoreCase);
		}

		private string BuildProcessedFileName(string originalFileName, string suffix)
		{
			string extension = Path.GetExtension(originalFileName);
			string fileNameWithoutExt = Path.GetFileNameWithoutExtension(originalFileName) ?? string.Empty;
			return $"{fileNameWithoutExt}{suffix}{extension}";
		}

		private string BuildGeneratedScriptFileName(string originalFileName, string prefix)
		{
			string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string baseName = Path.GetFileNameWithoutExtension(originalFileName) ?? "AlterTemplate";
			return $"{prefix}_{baseName}_{stamp}.sql";
		}

		private void RenameApplicationDbFile(
			SessionInfo si,
			string folderPath,
			string originalFileName,
			string targetFileName,
			string extension,
			XFFileType fileType)
		{
			if (string.Equals(originalFileName, targetFileName, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}

			XFFileEx sourceFile = BRApi.FileSystem.GetFile(
				si,
				FileSystemLocation.ApplicationDatabase,
				$"{folderPath.TrimEnd('/')}/{originalFileName}",
				true,
				true);

			XFFileInfo newFileInfo = new XFFileInfo(
				FileSystemLocation.ApplicationDatabase,
				targetFileName,
				$"{folderPath.TrimEnd('/')}/");

			newFileInfo.ContentFileExtension = extension;
			newFileInfo.ContentFileContainsData = true;
			newFileInfo.XFFileType = fileType;

			XFFile copiedFile = new XFFile(newFileInfo, string.Empty, sourceFile.XFFile.ContentFileBytes);
			BRApi.FileSystem.InsertOrUpdateFile(si, copiedFile);
			BRApi.FileSystem.DeleteFile(si, FileSystemLocation.ApplicationDatabase, $"{folderPath.TrimEnd('/')}/{originalFileName}");
		}

		private List<Dictionary<string, string>> ParseCsvToRows(string csvText)
		{
			List<List<string>> rawRows = ParseCsv(csvText);
			List<Dictionary<string, string>> output = new List<Dictionary<string, string>>();
			if (rawRows.Count <= 1)
			{
				return output;
			}

			List<string> header = rawRows[0];
			if (header.Count > 0)
			{
				header[0] = header[0].TrimStart('\uFEFF');
			}

			for (int i = 1; i < rawRows.Count; i++)
			{
				List<string> row = rawRows[i];
				if (row.Count == 1 && string.IsNullOrWhiteSpace(row[0]))
				{
					continue;
				}

				Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				for (int c = 0; c < header.Count; c++)
				{
					string key = header[c]?.Trim() ?? string.Empty;
					string value = c < row.Count ? row[c] : string.Empty;
					map[key] = value?.Trim() ?? string.Empty;
				}

				output.Add(map);
			}

			return output;
		}

		private List<List<string>> ParseCsv(string text)
		{
			List<List<string>> rows = new List<List<string>>();
			List<string> currentRow = new List<string>();
			StringBuilder currentCell = new StringBuilder();
			bool inQuotes = false;

			for (int i = 0; i < text.Length; i++)
			{
				char ch = text[i];

				if (ch == '"')
				{
					if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
					{
						currentCell.Append('"');
						i++;
					}
					else
					{
						inQuotes = !inQuotes;
					}
					continue;
				}

				if (!inQuotes && ch == ',')
				{
					currentRow.Add(currentCell.ToString());
					currentCell.Clear();
					continue;
				}

				if (!inQuotes && (ch == '\n' || ch == '\r'))
				{
					if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
					{
						i++;
					}

					currentRow.Add(currentCell.ToString());
					currentCell.Clear();
					rows.Add(currentRow);
					currentRow = new List<string>();
					continue;
				}

				currentCell.Append(ch);
			}

			currentRow.Add(currentCell.ToString());
			rows.Add(currentRow);

			return rows;
		}

		private string Get(Dictionary<string, string> row, string key)
		{
			return row.ContainsKey(key) ? row[key] : string.Empty;
		}

		private int ParseInt(string value, int defaultValue)
		{
			int parsed;
			return int.TryParse(value, out parsed) ? parsed : defaultValue;
		}

		private bool ParseBool(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			string normalized = value.Trim().ToLowerInvariant();
			return normalized == "1" || normalized == "true" || normalized == "yes" || normalized == "y";
		}

		private string[] ParsePipeList(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return new string[0];
			}

			return value
				.Split('|')
				.Select(x => x.Trim())
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.ToArray();
		}
	}
	#endregion
}
