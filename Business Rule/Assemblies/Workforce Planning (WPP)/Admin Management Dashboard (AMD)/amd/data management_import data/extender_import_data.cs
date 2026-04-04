using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_import_data
{
	public class MainClass
	{
		private const string SystemDefaultDecimalSeparator = ",";
		private const string SystemDefaultThousandsSeparator = ".";

		//Reference "UTI_SharedFunctionsCS" Business Rule
		OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass UTISharedFunctionsBR = new OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass();

		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case ExtenderFunctionType.Unknown:
						break;

					case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
						//Get parameters
						string tableName = args.NameValuePairs["p_table"];
						string tableNameRaw = tableName + "_Raw";
						string filesType = args.NameValuePairs["p_files_type"];
						string delimiter = string.Empty;
						if (filesType.Equals("Delimited", StringComparison.OrdinalIgnoreCase)) delimiter = this.DecodeDelimiter(args.NameValuePairs["p_delimiter"]);
						string fullPath = args.NameValuePairs["p_folder"];
						string method = args.NameValuePairs["p_method"];
						//EtlGenerationOptions etlOptions = this.GetEtlGenerationOptions(tableName); 
						EtlGenerationOptions etlOptions = this.GetEtlGenerationOptions2(si, tableName); //Config from XFC_HR_ETL_TableConfig
						Dictionary<string, string> columnMappingDict = new Dictionary<string, string>();

						//Log received parameters for diagnostics
						BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] p_table='{tableName}', p_files_type='{filesType}', p_delimiter='{delimiter}' (length={delimiter.Length}), p_folder='{fullPath}', p_method='{method}'");

						//Create data table from files folder
						DataTable dt = new DataTable();
						if (filesType.Equals("Excel", StringComparison.OrdinalIgnoreCase))
						{
							BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 1: Creating DataTable from Excel...");
							dt = UTISharedFunctionsBR.CreateDataTableFromExcelFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase);
						}
						else if (filesType.Equals("Delimited", StringComparison.OrdinalIgnoreCase))
						{
							if (string.IsNullOrEmpty(delimiter))
								throw ErrorHandler.LogWrite(si, new XFException($"Delimiter is empty. All received params: {string.Join("; ", args.NameValuePairs.Keys.Select(k => k + "=[" + args.NameValuePairs[k] + "]"))}"));
							BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] STEP 1: Creating DataTable from Delimited with char='{delimiter[0]}'...");
							dt = UTISharedFunctionsBR.CreateDataTableFromDelimitedFilesFolder(si, fullPath, FileSystemLocation.ApplicationDatabase, delimiter[0]);
						}
						else
						{
							throw ErrorHandler.LogWrite(si, new XFException("Files type must be 'Excel' or 'Delimited'"));
						}
						BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] STEP 1 DONE: DataTable rows={dt.Rows.Count}, cols={dt.Columns.Count}");

						//Get column mapping dictionary (after DataTable creation to allow dynamic mapping)
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 2: Getting column mapping...");
						columnMappingDict = this.GetColumnMappingDict(si, tableName, dt);
						BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] STEP 2 DONE: {columnMappingDict.Count} mappings");

						//Map and filter columns in DataTable
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 3: MapAndFilterColumns...");
						dt = (DataTable)UTISharedFunctionsBR.MapAndFilterColumnsInDataTable(si, dt, columnMappingDict);
						BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] STEP 3 DONE: cols={dt.Columns.Count}");

						//Capture file names before deletion for import tracking
						string importedFileName = string.Empty;
						try
						{
							List<XFFileInfoEx> importedFiles = BRApi.FileSystem.GetFilesInFolder(
								si, FileSystemLocation.ApplicationDatabase, fullPath, XFFileType.All, new List<string>());
							if (importedFiles != null && importedFiles.Count > 0)
							{
								importedFileName = string.Join("; ", importedFiles.Select(f => f.XFFileInfo.Name));
							}
						}
						catch (Exception)
						{
							BRApi.ErrorLog.LogMessage(si, "[extender_import_data] WARNING: Could not capture file names for import tracking.");
						}
						BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] Captured imported file(s): '{importedFileName}'");

						//Remove all files in folder
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 4: Deleting files...");
						UTISharedFunctionsBR.DeleteAllFilesInFolder(si, fullPath, FileSystemLocation.ApplicationDatabase);
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 4 DONE");

						//Create a Raw Table
						string sCreateRaw = @$"
						DECLARE @sql AS VARCHAR(MAX)
					    -- Construir la declaración CREATE TABLE dinámica
					    SET @sql = '
							DROP TABLE IF EXISTS {tableNameRaw};
							CREATE TABLE {tableNameRaw} ('
					
					    -- Obtener los nombres de las columnas de la tabla original
					    SELECT @sql = @sql + COLUMN_NAME + ' VARCHAR(150), '
					    FROM INFORMATION_SCHEMA.COLUMNS
					    WHERE TABLE_NAME = '{tableName}'  -- Reemplaza con tu tabla original
					
					    -- Eliminar la última coma
					    SET @sql = LEFT(@sql, LEN(@sql) - 1)
					
					    -- Cerrar el paréntesis
						SET @sql = @sql + ')'
					
					    -- Ejecutar la consulta dinámica
					    EXEC(@sql)
						";
						ExecuteSql(si, sCreateRaw);

						//Load data table to custom table
						BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] STEP 5: LoadDataTableToCustomTable method='{method}'...");
						UTISharedFunctionsBR.LoadDataTableToCustomTable(si, tableNameRaw, dt, method);
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 5 DONE - Import complete");

						//Load raw into final data table
						BRApi.ErrorLog.LogMessage(si, $"[extender_import_data] STEP 6: Load raw into final data table...");
						string sETL = GenerateEtlSql(si, tableName, tableNameRaw, etlOptions);
						BRApi.ErrorLog.LogMessage(si, sETL);
						ExecuteSql(si, sETL);
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 6 DONE - Import complete");

						//Update import tracking table
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 7: Updating import tracking table...");
						this.UpdateTableImportInfo(si, tableName, fullPath, importedFileName);
						BRApi.ErrorLog.LogMessage(si, "[extender_import_data] STEP 7 DONE");

						break;

					case ExtenderFunctionType.ExecuteExternalDimensionSource:
						break;
				}

				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}

		#region "Helper Functions"

		#region "Decode Delimiter"

		private string DecodeDelimiter(string raw)
		{
			switch (raw.Trim().ToUpperInvariant())
			{
				case "PIPE": return "|";
				case "TAB": return "\t";
				case "SEMICOLON": return ";";
				default: return raw;
			}
		}

		#endregion

		#region "Get Column Mapping Dictionary"

		private Dictionary<string, string> GetColumnMappingDict(SessionInfo si, string tableName, DataTable sourceData)
		{
			//Dynamic mapping: query XFC_HR_MASTER_Mapping for explicit renames, lowercase for the rest
			var columnMappingDict = new Dictionary<string, string>();

			//Get explicit mappings from mapping table
			var explicitMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				var dbParamInfos = new List<DbParamInfo>
					{
						new DbParamInfo("paramTableName", tableName)
					};
				DataTable mappingDt = BRApi.Database.ExecuteSql(
					dbConn,
					@"SELECT source_column, target_column
						FROM XFC_HR_MASTER_Mapping WITH(NOLOCK)
						WHERE table_name = @paramTableName",
					dbParamInfos,
					false
				);
				if (mappingDt != null)
				{
					foreach (DataRow row in mappingDt.Rows)
					{
						explicitMappings[row["source_column"].ToString().Trim()] = row["target_column"].ToString().Trim();
					}
				}
			}

			//Build mapping dictionary: explicit rename if exists, otherwise lowercase
			foreach (DataColumn col in sourceData.Columns)
			{
				string sourceColName = col.ColumnName.Trim();
				if (explicitMappings.ContainsKey(sourceColName))
				{
					columnMappingDict[sourceColName] = explicitMappings[sourceColName];
				}
				else
				{
					columnMappingDict[sourceColName] = sourceColName.ToLower();
				}
			}

			return columnMappingDict;
		}

		#endregion

		#region "ETL"

		// =====================================================================================================
		// ETL GUIDE (READ THIS FIRST)
		// 1) Add or adjust table config in GetTableEtlConfigurations().
		// 2) Prefer FloatColumns + FloatColumnsNumericFormat for simple numeric formatting by table.
		// 3) Use NumericColumnFormats only for specific column exceptions.
		// 4) For period columns (e.g. yyyymm), set DateColumnInputFormats and MonthStartDateColumns.
		// 5) Keep ETL mode = "dynamic" unless you explicitly need "hardcoded" or "custom" behavior.
		// 6) For custom ETL, create your new SQL in "ETL Helper" -> "ETL - Hardcoded Templates"
		// =====================================================================================================

		#region "ETL Configuration"

		/// <summary>
		/// Central ETL catalog.
		/// Add new table loads here so other developers have one obvious place to maintain behavior.
		/// </summary>
		/// <returns>Dictionary tableName -> ETL configuration.</returns>
		private Dictionary<string, TableEtlConfiguration> GetTableEtlConfigurations()
		{
		    // Configuración por defecto para todas las tablas
		    var defaultConfig = new TableEtlConfiguration
		    {
		        EtlMode = "dynamic",
		        PreDeleteKeyColumns = new List<string>(),
		        FloatColumnsNumericFormat = null,
		        NumericColumnFormats = new Dictionary<string, NumericFormatConfiguration>(StringComparer.OrdinalIgnoreCase),
		        FileDefaultDateInputFormats = new List<string> { "auto" },
		        DateColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
		        DateColumnInputFormats = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase),
		        MonthStartDateColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase),
		        FloatColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		    };
		
		    return new Dictionary<string, TableEtlConfiguration>(StringComparer.OrdinalIgnoreCase)
		    {
		        ["DEFAULT"] = defaultConfig,
		
		        // Ejemplo de tabla con overrides
		        ["XFC_HR_Helper"] = new TableEtlConfiguration
		        {
		            PreDeleteKeyColumns = new List<string> { "periodo", "id_empleado" },
		            FloatColumnsNumericFormat = new NumericFormatConfiguration { DecimalSeparator = ",", ThousandsSeparator = "." },
		            NumericColumnFormats = new Dictionary<string, NumericFormatConfiguration>(StringComparer.OrdinalIgnoreCase)
		            {
		                ["importe_es"] = new NumericFormatConfiguration { DecimalSeparator = ",", ThousandsSeparator = "." },
		                ["importe_us"] = new NumericFormatConfiguration { DecimalSeparator = ".", ThousandsSeparator = "," }
		            },
		            FileDefaultDateInputFormats = new List<string> { "auto", "dd/mm/yyyy", "yyyy-mm-dd" },
		            FloatColumns = new HashSet<string>(new[] { "importe_es", "importe_us", "porcentaje" }, StringComparer.OrdinalIgnoreCase),
		            DateColumns = new HashSet<string>(new[] { "periodo", "fecha_mov" }, StringComparer.OrdinalIgnoreCase),
		            DateColumnInputFormats = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
		            {
		                ["periodo"] = new List<string> { "yyyymm" },
		                ["fecha_mov"] = new List<string> { "dd/mm/yyyy", "yyyy-mm-dd" }
		            },
		            MonthStartDateColumns = new HashSet<string>(new[] { "periodo" }, StringComparer.OrdinalIgnoreCase)
		        },
		
		        ["XFC_HR_MASTER_GLB_Empleados"] = new TableEtlConfiguration
		        {
		            PreDeleteKeyColumns = new List<string> { "periodo" },
		            FloatColumnsNumericFormat = new NumericFormatConfiguration { DecimalSeparator = ",", ThousandsSeparator = "." },
		            FileDefaultDateInputFormats = new List<string> { "dd.mm.yyyy" },
		            DateColumnInputFormats = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
		            {
		                ["periodo"] = new List<string> { "yyyymm" }
		            },
		            MonthStartDateColumns = new HashSet<string>(new[] { "periodo" }, StringComparer.OrdinalIgnoreCase),
		            FloatColumns = new HashSet<string>(new[]
		            {
		                "horas_jornada_contrato",
		                "porcentaje_jornada_contrato",
		                "hora_anuales_contrato"
		            }, StringComparer.OrdinalIgnoreCase)
		        }
		    };
		}
		
		

		/// <summary>
		/// Gets effective ETL options for a destination table.
		/// </summary>
		/// <param name="tableName">Destination table name configured in DM parameter p_table.</param>
		/// <returns>Resolved options ready for ETL SQL generation.</returns>
		private EtlGenerationOptions GetEtlGenerationOptions(string tableName)
		{
			return this.GetDefaultEtlOptionsByTable(tableName);
		}

		/// <summary>
		/// Resolves table configuration and creates a runtime-safe options object.
		/// Falls back to DEFAULT when the table is not explicitly configured.
		/// </summary>
		/// <param name="tableName">Destination table name.</param>
		/// <returns>Materialized options used by ETL generation methods.</returns>
		private EtlGenerationOptions GetDefaultEtlOptionsByTable(string tableName)
		{
			Dictionary<string, TableEtlConfiguration> tableConfigs = this.GetTableEtlConfigurations();
			if (!tableConfigs.TryGetValue(tableName, out TableEtlConfiguration tableConfig))
			{
				if (!tableConfigs.TryGetValue("DEFAULT", out tableConfig))
				{
					return new EtlGenerationOptions();
				}
			}

			return new EtlGenerationOptions
			{
			    EtlMode = tableConfig.EtlMode,
			
			    PreDeleteKeyColumns = new List<string>(tableConfig.PreDeleteKeyColumns),
			
			    FloatColumnsNumericFormat = this.CloneNumericFormat(tableConfig.FloatColumnsNumericFormat),
			
			    NumericColumnFormats = tableConfig.NumericColumnFormats.ToDictionary(
			        kvp => kvp.Key,
			        kvp => this.CloneNumericFormat(kvp.Value),
			        StringComparer.OrdinalIgnoreCase),
				    
			    FileDefaultDateInputFormats = new List<string>(tableConfig.FileDefaultDateInputFormats),
			    //SqlDefaultDateInputFormats = new List<string>(tableConfig.SqlDefaultDateInputFormats),
			
			    FloatColumns = new HashSet<string>(tableConfig.FloatColumns, StringComparer.OrdinalIgnoreCase),
			
			    DateColumns = new HashSet<string>(tableConfig.DateColumns, StringComparer.OrdinalIgnoreCase),
			
			    DateColumnInputFormats = tableConfig.DateColumnInputFormats.ToDictionary(
			        kvp => kvp.Key,
			        kvp => new List<string>(kvp.Value),
			        StringComparer.OrdinalIgnoreCase),
			
			    MonthStartDateColumns = new HashSet<string>(tableConfig.MonthStartDateColumns, StringComparer.OrdinalIgnoreCase)
			};
		}

		/// <summary>
		/// Authoring-time ETL configuration model.
		/// This is the object developers edit when onboarding new source tables.
		/// </summary>
		private sealed class TableEtlConfiguration
		{
		    /// <summary>
		    /// ETL mode: dynamic (recommended), hardcoded.
		    /// </summary>
		    public string EtlMode { get; set; } = "dynamic";
		
		    /// <summary>
		    /// Key columns used by pre-delete logic before insert.
		    /// </summary>
		    public List<string> PreDeleteKeyColumns { get; set; } = new List<string>();
		
		    /// <summary>
		    /// Shortcut numeric format automatically applied to all columns listed in FloatColumns.
		    /// </summary>
		    public NumericFormatConfiguration FloatColumnsNumericFormat { get; set; } = null;
		
		    /// <summary>
		    /// Optional per-column numeric formats (highest priority).
		    /// </summary>
		    public Dictionary<string, NumericFormatConfiguration> NumericColumnFormats { get; set; } 
		        = new Dictionary<string, NumericFormatConfiguration>(StringComparer.OrdinalIgnoreCase);
		
		
		    /// <summary>
		    /// Default date formats when source is FILE (Excel/Delimited).
		    /// </summary>
		    public List<string> FileDefaultDateInputFormats { get; set; } = new List<string> { "auto" };
		
//		    /// <summary>
//		    /// Default date formats when source is SQL (SIC).
//		    /// </summary>
//		    public List<string> SqlDefaultDateInputFormats { get; set; } = new List<string> { "auto" };
		
		    /// <summary>
		    /// Columns that should receive numeric cleanup for float/decimal values.
		    /// </summary>
		    public HashSet<string> FloatColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		
		    /// <summary>
		    /// Restricts date parsing scope. Empty set means all date-type columns.
		    /// </summary>
		    public HashSet<string> DateColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		
		    /// <summary>
		    /// Per-column date format list (e.g. periodo -> yyyymm).
		    /// </summary>
		    public Dictionary<string, List<string>> DateColumnInputFormats { get; set; } 
		        = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		
		    /// <summary>
		    /// Columns forced to first day of month after parsing.
		    /// Useful for period fields such as yyyymm.
		    /// </summary>
		    public HashSet<string> MonthStartDateColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}

		#endregion

		#region "ETL Helper"

		#region "ETL - SQL Generation"

		/// <summary>
		/// Main ETL SQL selector by mode (custom/hardcoded/dynamic).
		/// </summary>
		private string GenerateEtlSql(SessionInfo si, string tableName, string tableNameRaw, EtlGenerationOptions etlOptions)
		{
		    var tableConfigs = this.GetTableEtlConfigurations();
		    TableEtlConfiguration config = tableConfigs.ContainsKey(tableName)
		        ? tableConfigs[tableName]
		        : tableConfigs["DEFAULT"];
		
		    if (string.Equals(config.EtlMode, "hardcoded", StringComparison.OrdinalIgnoreCase))
		    {
		        return this.GenerateHardcodedEtlSql(si, tableName, tableNameRaw, etlOptions);
		    }
		
		    return this.GenerateDynamicEtlSql(si, tableName, tableNameRaw, etlOptions);
		}
		#endregion

		#region "ETL - Dynamic SQL"

		/// <summary>
		/// Builds dynamic ETL SQL based on destination metadata and configured transformations.
		/// </summary>
		private string GenerateDynamicEtlSql(SessionInfo si, string tableName, string tableNameRaw, EtlGenerationOptions options)
		{
			List<SqlColumnDefinition> destinationColumns = this.GetTableColumns(si, tableName);
			if (destinationColumns.Count == 0)
			{
				throw ErrorHandler.LogWrite(si, new XFException($"No columns found for destination table '{tableName}'."));
			}

			List<string> insertColumns = destinationColumns.Select(c => this.QuoteSqlIdentifier(c.ColumnName)).ToList();
			List<string> selectExpressions = destinationColumns.Select(c => this.BuildColumnExpression(c, tableName, options)).ToList();
			List<string> preDeleteKeys = this.GetEffectivePreDeleteKeys(options, destinationColumns);

			var sql = new StringBuilder();
			sql.AppendLine(";WITH SRC AS (");
			sql.AppendLine($"    SELECT {string.Join(", ", selectExpressions)}");
			sql.AppendLine($"    FROM {this.QuoteSqlIdentifier(tableNameRaw)}");
			sql.AppendLine(")");

			if (preDeleteKeys.Count > 0)
			{
				sql.AppendLine(this.BuildPreDeleteSql(tableName, preDeleteKeys));
			}

			sql.AppendLine($"INSERT INTO {this.QuoteSqlIdentifier(tableName)} ({string.Join(", ", insertColumns)})");
			sql.AppendLine($"SELECT {string.Join(", ", selectExpressions)}");
			sql.AppendLine($"FROM {this.QuoteSqlIdentifier(tableNameRaw)};");
			sql.AppendLine($"--IF OBJECT_ID('{tableNameRaw}', 'U') IS NOT NULL DROP TABLE {this.QuoteSqlIdentifier(tableNameRaw)};");

			BRApi.ErrorLog.LogMessage(si, $"[extender_import_data_SIC] Dynamic ETL generated for table '{tableName}'.");
			return sql.ToString();
		}

		/// <summary>
		/// Returns valid key columns for pre-delete (configured keys intersect destination columns).
		/// </summary>
		private List<string> GetEffectivePreDeleteKeys(EtlGenerationOptions options, List<SqlColumnDefinition> destinationColumns)
		{
			if (options.PreDeleteKeyColumns == null || options.PreDeleteKeyColumns.Count == 0)
			{
				return new List<string>();
			}

			HashSet<string> destinationNames = new HashSet<string>(destinationColumns.Select(c => c.ColumnName), StringComparer.OrdinalIgnoreCase);
			return options.PreDeleteKeyColumns
				.Where(col => !string.IsNullOrWhiteSpace(col) && destinationNames.Contains(col))
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();
		}

		/// <summary>
		/// Creates DELETE SQL joined by configured keys to prevent duplicate loads.
		/// </summary>
		private string BuildPreDeleteSql(string tableName, List<string> keyColumns)
		{
			string quotedTable = this.QuoteSqlIdentifier(tableName);
			List<string> quotedKeys = keyColumns.Select(this.QuoteSqlIdentifier).ToList();
			string joinCondition = string.Join(" AND ", quotedKeys.Select(k => $"((T.{k} = K.{k}) OR (T.{k} IS NULL AND K.{k} IS NULL))"));
			string keyNotNullFilter = string.Join(" AND ", quotedKeys.Select(k => $"K.{k} IS NOT NULL"));

			var sql = new StringBuilder();
			sql.AppendLine("DELETE T");
			sql.AppendLine($"FROM {quotedTable} T");
			sql.AppendLine("INNER JOIN (");
			sql.AppendLine($"    SELECT DISTINCT {string.Join(", ", quotedKeys)}");
			sql.AppendLine("    FROM SRC");
			sql.AppendLine(") K");
			sql.AppendLine($"ON {joinCondition}");
			sql.AppendLine($"WHERE {keyNotNullFilter};");

			return sql.ToString();
		}
		#endregion

		#region "ETL - Hardcoded Templates"

		private string GenerateHardcodedEtlSql(SessionInfo si, string tableName, string tableNameRaw, EtlGenerationOptions options)
		{
			string sQuery = $@"INSERT INTO {tableName} SELECT * FROM {tableNameRaw}";
			if (tableName == "XFC_HR_MASTER_GLB_Empleados")
			{
				#region "XFC_HR_MASTER_GLB_Empleados"
				sQuery = @$"							
							DELETE FROM XFC_HR_MASTER_GLB_Empleados
							WHERE periodo IN (
								SELECT CONVERT(VARCHAR(150), CAST(SUBSTRING(periodo, 1, 4) AS INT)) + '-' + 
							    RIGHT('00' + CAST(SUBSTRING(periodo, 5, 2) AS VARCHAR(2)), 2) + '-01' AS periodo
								FROM XFC_HR_MASTER_GLB_Empleados_Raw
								)
							
							INSERT INTO XFC_HR_MASTER_GLB_Empleados (
							    id_empleado,
							    periodo,
							    area_personal,
							    id_puesto,
							    id_unidad_organizativa,
							    status,
							    marca,
							    divP,
							    unidad,
							    texto_unidad,
							    nombre_apellido,
							    horas_jornada_contrato,
							    porcentaje_jornada_contrato,
							    hora_anuales_contrato,
							    grupo_profesional,
							    sociedad,
							    sexo,
							    fecha_nacimiento,
							    provincia_tributacion,
							    tipo_contrato,
							    CNAE,
							    clave_ocupacion,
							    id_cebe_alta,
							    texto_categoria,
							    texto_subcategoría,
							    texto_puesto_trabajo,
							    region,
							    zona,
							    sub_zona,
							    antiguedad,
							    fecha_baja,
							    motivo_baja,
							    liberados,
							    pais
							)
							SELECT
							    id_empleado,
							    -- Convertir el periodo 'yyyymm' a el primer día del mes 'yyyy-mm-01'
							    CONVERT(VARCHAR(150), CAST(SUBSTRING(periodo, 1, 4) AS INT)) + '-' + 
							    RIGHT('00' + CAST(SUBSTRING(periodo, 5, 2) AS VARCHAR(2)), 2) + '-01' AS periodo,
							    area_personal,
							    id_puesto,
							    id_unidad_organizativa,
							    status,
							    marca,
							    divP,
							    unidad,
							    texto_unidad,
							    nombre_apellido,
							   	-- Convertir valores de tipo FLOAT, primero quitar '.' y luego sustituir ',' por '.'
							    CAST(REPLACE(REPLACE(CAST(horas_jornada_contrato AS VARCHAR(150)), '.', ''), ',', '.') AS VARCHAR(150)) AS horas_jornada_contrato,
							    CAST(REPLACE(REPLACE(CAST(porcentaje_jornada_contrato AS VARCHAR(150)), '.', ''), ',', '.') AS VARCHAR(150)) AS porcentaje_jornada_contrato,
							    CAST(REPLACE(REPLACE(CAST(hora_anuales_contrato AS VARCHAR(150)), '.', ''), ',', '.') AS VARCHAR(150)) AS hora_anuales_contrato,
							    grupo_profesional,
							    sociedad,
							    sexo,
							    -- Convertir fechas a formato VARCHAR(150)
							    CAST(fecha_nacimiento AS VARCHAR(150)),
							    provincia_tributacion,
							    tipo_contrato,
							    CNAE,
							    clave_ocupacion,
							    id_cebe_alta,
							    texto_categoria,
							    texto_subcategoría,
							    texto_puesto_trabajo,
							    region,
							    zona,
							    sub_zona,
							    -- Convertir fechas a formato VARCHAR(150)
							    CAST(antiguedad AS VARCHAR(150)),
							    CAST(fecha_baja AS VARCHAR(150)),
							    motivo_baja,
							    liberados,
							    pais
							FROM XFC_HR_MASTER_GLB_Empleados_Raw;
							
							DROP TABLE XFC_HR_MASTER_GLB_Empleados_Raw;
				";
				#endregion

			}
			else if (tableName == "XFC_HR_MASTER_GLB_Conceptos")
			{

			}
			else if (tableName == "XFC_HR_MASTER_CS_Estructura")
			{

			}
			else if (tableName == "XFC_HR_RAW_GLB_NominaTeorica")
			{

			}

			return sQuery;
		}


		#endregion

		#region "ETL - Table Metadata"

		private List<SqlColumnDefinition> GetTableColumns(SessionInfo si, string tableName)
		{
			using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				var dbParamInfos = new List<DbParamInfo>
				{
					new DbParamInfo("paramTableName", tableName)
				};

				DataTable dt = BRApi.Database.ExecuteSql(
					dbConn,
					@"SELECT
						COLUMN_NAME,
						DATA_TYPE,
						CHARACTER_MAXIMUM_LENGTH,
						NUMERIC_PRECISION,
						NUMERIC_SCALE,
						DATETIME_PRECISION,
						ORDINAL_POSITION
					FROM INFORMATION_SCHEMA.COLUMNS WITH(NOLOCK)
					WHERE TABLE_NAME = @paramTableName
					ORDER BY ORDINAL_POSITION",
					dbParamInfos,
					false);

				var columns = new List<SqlColumnDefinition>();
				if (dt != null)
				{
					foreach (DataRow row in dt.Rows)
					{
						columns.Add(new SqlColumnDefinition
						{
							ColumnName = row["COLUMN_NAME"].ToString(),
							DataType = row["DATA_TYPE"].ToString(),
							CharacterMaxLength = this.GetNullableInt(row["CHARACTER_MAXIMUM_LENGTH"]),
							NumericPrecision = this.GetNullableInt(row["NUMERIC_PRECISION"]),
							NumericScale = this.GetNullableInt(row["NUMERIC_SCALE"]),
							DateTimePrecision = this.GetNullableInt(row["DATETIME_PRECISION"]),
							OrdinalPosition = Convert.ToInt32(row["ORDINAL_POSITION"], CultureInfo.InvariantCulture)
						});
					}
				}

				return columns;
			}
		}

		private int? GetNullableInt(object value)
		{
			if (value == null || value == DBNull.Value)
			{
				return null;
			}

			return Convert.ToInt32(value, CultureInfo.InvariantCulture);
		}
		#endregion

		#region "ETL - Column Transformations"

		private string BuildColumnExpression(SqlColumnDefinition column, string tableName, EtlGenerationOptions options)
		{
			string columnRef = this.QuoteSqlIdentifier(column.ColumnName);
			string dataType = (column.DataType ?? string.Empty).Trim().ToLowerInvariant();

			if (this.IsDateType(dataType))
			{
				return this.BuildDateExpression(columnRef, column.ColumnName, column, options) + " AS " + columnRef;
			}

			if (this.IsNumericType(dataType))
			{
				//bool applyFloatCleanup = this.ShouldApplyFloatCleanup(tableName, column.ColumnName, options);
				bool applyFloatCleanup = true;
				return this.BuildNumericExpression(columnRef, tableName, column.ColumnName, column, applyFloatCleanup, options) + " AS " + columnRef;
			}

			return $"{columnRef} AS {columnRef}";
		}

		private bool IsDateType(string dataType)
		{
			return dataType == "date"
				|| dataType == "datetime"
				|| dataType == "datetime2"
				|| dataType == "smalldatetime"
				|| dataType == "datetimeoffset";
		}

		private bool IsNumericType(string dataType)
		{
			return dataType == "float"
				|| dataType == "real"
				|| dataType == "decimal"
				|| dataType == "numeric"
				|| dataType == "money"
				|| dataType == "smallmoney"
				|| dataType == "int"
				|| dataType == "bigint"
				|| dataType == "smallint"
				|| dataType == "tinyint";
		}

//		private bool ShouldApplyFloatCleanup(string tableName, string columnName, EtlGenerationOptions options)
//		{
//			bool tableEnabled = options.FloatEnabledTables.Count == 0 || options.FloatEnabledTables.Contains(tableName);
//			if (!tableEnabled)
//			{
//				return false;
//			}

//			if (options.FloatColumns.Count == 0)
//			{
//				return true;
//			}

//			return options.FloatColumns.Contains(columnName);
//		}

		private List<string> ResolveDateFormats(string columnName, EtlGenerationOptions options)
		{
			if (!(options.DateColumns.Count == 0 || options.DateColumns.Contains(columnName)))
			{
				return new List<string> { "auto" };
			}

			if (options.DateColumnInputFormats.TryGetValue(columnName, out List<string> specificFormats) && specificFormats.Count > 0)
			{
				return specificFormats;
			}

			if (options.FileDefaultDateInputFormats.Count > 0)
			{
				return options.FileDefaultDateInputFormats;
			}

			return new List<string> { "auto" };
		}

		private string BuildNumericExpression(string columnRef, string tableName, string columnName, SqlColumnDefinition column, bool applyFloatCleanup, EtlGenerationOptions options)
		{
			string targetType = this.GetSqlTypeDeclaration(column);
			string rawValue = $"CONVERT(VARCHAR(4000), {columnRef})";
			string sanitized = rawValue;

			if (applyFloatCleanup)
			{
				NumericFormatConfiguration numericFormat = this.ResolveNumericFormat(tableName, columnName, options);
				if (numericFormat != null)
				{
					sanitized = this.ApplyNumericFormatToSqlExpression(rawValue, numericFormat);
				}
			}

			return $"TRY_CONVERT({targetType}, NULLIF(LTRIM(RTRIM({sanitized})), ''))";
		}

		/// <summary>
		/// Numeric format priority:
		/// 1) NumericColumnFormats[column]
		/// 2) FloatColumnsNumericFormat
		/// 3) DefaultNumericFormat
		/// 4) System default (class constants)
		/// </summary>
		private NumericFormatConfiguration ResolveNumericFormat(string tableName, string columnName, EtlGenerationOptions options)
		{
//		    if (options.NumericColumnFormats != null && options.NumericColumnFormats.TryGetValue(columnName, out var byColumn))
//		    {
//		        return byColumn;
//		    }
			
			if (options.FloatColumns != null && options.FloatColumns.Contains(columnName))
		    {
		        return options.FloatColumnsNumericFormat ?? new NumericFormatConfiguration { DecimalSeparator = ".", ThousandsSeparator = "" };
		    }
		
		
		    return this.GetSystemDefaultNumericFormat();
		}

		private NumericFormatConfiguration GetSystemDefaultNumericFormat()
		{
			return new NumericFormatConfiguration
			{
				DecimalSeparator = SystemDefaultDecimalSeparator,
				ThousandsSeparator = SystemDefaultThousandsSeparator
			};
		}

		private NumericFormatConfiguration CloneNumericFormat(NumericFormatConfiguration source)
		{
			if (source == null)
			{
				return null;
			}

			return new NumericFormatConfiguration
			{
				DecimalSeparator = source.DecimalSeparator,
				ThousandsSeparator = source.ThousandsSeparator
			};
		}

		/// <summary>
		/// Applies decimal/thousand replacements to convert text input into SQL-compatible numeric text.
		/// </summary>
		private string ApplyNumericFormatToSqlExpression(string sqlExpression, NumericFormatConfiguration numericFormat)
		{
			string normalized = sqlExpression;
			string thousandsSeparator = (numericFormat.ThousandsSeparator ?? string.Empty).Trim();
			string decimalSeparator = (numericFormat.DecimalSeparator ?? string.Empty).Trim();

			if (!string.IsNullOrEmpty(thousandsSeparator))
			{
				normalized = $"REPLACE({normalized}, '{this.EscapeSqlStringLiteral(thousandsSeparator)}', '')";
			}

			if (!string.IsNullOrEmpty(decimalSeparator) && !decimalSeparator.Equals(".", StringComparison.Ordinal))
			{
				normalized = $"REPLACE({normalized}, '{this.EscapeSqlStringLiteral(decimalSeparator)}', '.')";
			}

			return normalized;
		}

		private string BuildDateExpression(string columnRef, string columnName, SqlColumnDefinition column, EtlGenerationOptions options)
		{
			string targetType = this.GetSqlTypeDeclaration(column);
			string rawText = $"NULLIF(LTRIM(RTRIM(CONVERT(VARCHAR(4000), {columnRef}))), '')";
			List<string> configuredFormats = this.ResolveDateFormats(columnName, options);
			List<string> expandedFormats = this.ExpandDateFormats(configuredFormats);

			string parsedDateExpression = this.BuildDateParseExpression(rawText, expandedFormats);
			bool forceMonthStart = options.MonthStartDateColumns.Contains(columnName) || expandedFormats.Any(fmt => string.Equals(fmt, "yyyymm", StringComparison.OrdinalIgnoreCase));

			if (forceMonthStart)
			{
				string monthStartDate = $"CASE WHEN {parsedDateExpression} IS NULL THEN NULL ELSE DATEFROMPARTS(YEAR({parsedDateExpression}), MONTH({parsedDateExpression}), 1) END";
				return $"TRY_CONVERT({targetType}, {monthStartDate})";
			}

			return $"TRY_CONVERT({targetType}, {parsedDateExpression})";
		}

		private List<string> ExpandDateFormats(List<string> configuredFormats)
		{
			var result = new List<string>();
			foreach (string format in configuredFormats)
			{
				string normalized = (format ?? string.Empty).Trim().ToLowerInvariant();
				if (string.IsNullOrWhiteSpace(normalized))
				{
					continue;
				}

				if (normalized == "auto")
				{
					result.AddRange(new[] { "dd.mm.yyyy", "dd/mm/yyyy", "dd-mm-yyyy", "yyyymm", "yyyymmdd", "yyyy-mm-dd", "yyyy/mm/dd", "yyyy-mm-dd hh:mm:ss" });
				}
				else
				{
					result.Add(normalized);
				}
			}

			if (result.Count == 0)
			{
				result.AddRange(new[] { "dd.mm.yyyy", "dd/mm/yyyy", "dd-mm-yyyy", "yyyymm", "yyyymmdd", "yyyy-mm-dd", "yyyy/mm/dd", "yyyy-mm-dd hh:mm:ss" });
			}

			return result.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private string BuildDateParseExpression(string rawText, List<string> formats)
		{
			List<string> tries = formats
				.Select(fmt => this.BuildSingleDateTry(rawText, fmt))
				.Where(expr => !string.IsNullOrWhiteSpace(expr))
				.ToList();

			if (tries.Count == 0)
			{
				return $"TRY_CONVERT(date, {rawText})";
			}

			if (tries.Count == 1)
			{
				return tries[0];
			}

			return $"COALESCE({string.Join(", ", tries)})";
		}

		private string BuildSingleDateTry(string rawText, string format)
		{
			switch ((format ?? string.Empty).Trim().ToLowerInvariant())
			{
				case "yyyymm":
					return $"TRY_CONVERT(date, CASE WHEN LEN({rawText}) = 6 THEN CONCAT(LEFT({rawText}, 4), '-', RIGHT({rawText}, 2), '-01') ELSE NULL END, 23)";
				case "yyyymmdd":
					return $"TRY_CONVERT(date, {rawText}, 112)";
				case "ddmmyyyy":
					return $"TRY_CONVERT(date, CASE WHEN LEN({rawText}) = 8 THEN STUFF(STUFF({rawText}, 3, 0, '/'), 6, 0, '/') ELSE NULL END, 103)";
				case "dd.mm.yyyy":
					return $"TRY_CONVERT(date, {rawText}, 104)";
				case "dd/mm/yyyy":
					return $"TRY_CONVERT(date, {rawText}, 103)";
				case "dd-mm-yyyy":
					return $"TRY_CONVERT(date, {rawText}, 105)";
				case "yyyy-mm-dd":
				case "iso":
					return $"TRY_CONVERT(date, {rawText}, 23)";
				case "yyyy/mm/dd":
					return $"TRY_CONVERT(date, {rawText}, 111)";
				case "yyyy-mm-dd hh:mi:ss":
				case "yyyy-mm-dd hh:mm:ss":
					return $"TRY_CONVERT(date, {rawText}, 120)";
				default:
					return $"TRY_CONVERT(date, {rawText})";
			}

		}

		private string GetSqlTypeDeclaration(SqlColumnDefinition column)
		{
			string dataType = (column.DataType ?? string.Empty).Trim().ToLowerInvariant();

			if (dataType == "decimal" || dataType == "numeric")
			{
				int precision = column.NumericPrecision ?? 18;
				int scale = column.NumericScale ?? 2;
				return $"{dataType}({precision},{scale})";
			}

			if (dataType == "datetime2" || dataType == "datetimeoffset" || dataType == "time")
			{
				if (column.DateTimePrecision.HasValue)
				{
					return $"{dataType}({column.DateTimePrecision.Value})";
				}
			}

			return dataType;
		}
		#endregion

		#region "ETL - SQL Utilities"

		private string QuoteSqlIdentifier(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
			{
				return identifier;
			}

			if (identifier.IndexOf('.') >= 0)
			{
				string[] parts = identifier.Split('.');
				return string.Join(".", parts.Select(p => "[" + p.Trim().Replace("]", "]]") + "]"));
			}

			return "[" + identifier.Trim().Replace("]", "]]") + "]";
		}

		private string EscapeSqlStringLiteral(string value)
		{
			return (value ?? string.Empty).Replace("'", "''");
		}
		#endregion

		#region "ETL - Models"

		/// <summary>
		/// Runtime ETL options after resolving table configuration.
		/// </summary>
		private sealed class EtlGenerationOptions
		{
		    public HashSet<string> FloatColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		
		    public List<string> PreDeleteKeyColumns { get; set; } = new List<string>();
		
		    public NumericFormatConfiguration FloatColumnsNumericFormat { get; set; } = null;
		
		    public Dictionary<string, NumericFormatConfiguration> NumericColumnFormats { get; set; } 
		        = new Dictionary<string, NumericFormatConfiguration>(StringComparer.OrdinalIgnoreCase);
		
		    public List<string> FileDefaultDateInputFormats { get; set; } = new List<string> { "auto" };
//		    public List<string> SqlDefaultDateInputFormats { get; set; } = new List<string> { "auto" };
		
		    public HashSet<string> DateColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		
		    public Dictionary<string, List<string>> DateColumnInputFormats { get; set; } 
		        = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		
		    public HashSet<string> MonthStartDateColumns { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		
		    public string EtlMode { get; set; } = "dynamic";
		}

		/// <summary>
		/// SQL metadata snapshot of destination table columns.
		/// </summary>
		private sealed class SqlColumnDefinition
		{
			public string ColumnName { get; set; }
			public string DataType { get; set; }
			public int? CharacterMaxLength { get; set; }
			public int? NumericPrecision { get; set; }
			public int? NumericScale { get; set; }
			public int? DateTimePrecision { get; set; }
			public int OrdinalPosition { get; set; }
		}

		/// <summary>
		/// Numeric separators used to normalize source text to SQL numeric values.
		/// </summary>
		private sealed class NumericFormatConfiguration
		{
			public string DecimalSeparator { get; set; } = ".";
			public string ThousandsSeparator { get; set; } = string.Empty;
		}
		#endregion
		
		#region "ETL - Configuration from Database"
		// Lee la configuración ETL de una tabla desde XFC_HR_ETL_TableConfig
		private TableEtlConfiguration GetTableEtlConfigurationFromDatabase(SessionInfo si, string tableName)
		{
			using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				var dbParamInfos = new List<DbParamInfo>
				{
					new DbParamInfo("paramTableName", tableName)
				};

				DataTable dt = BRApi.Database.ExecuteSql(
					dbConn,
					@"SELECT TOP 1 *
					FROM XFC_HR_ETL_TableConfig WITH (NOLOCK)
					WHERE TableName = @paramTableName",
					dbParamInfos,
					false
				);

				if (dt == null || dt.Rows.Count == 0)
				{
					return null; // no hay config para esa tabla
				}

				DataRow row = dt.Rows[0];

				var config = new TableEtlConfiguration
				{
				    EtlMode = GetString(row, "EtlMode") ?? "dynamic",
				
				    PreDeleteKeyColumns = ParseCsvToList(GetString(row, "PreDeleteKeyColumns")),
				
				    FloatColumnsNumericFormat = BuildNumericFormat(
				        GetString(row, "FloatDecimalSeparator"),
				        GetString(row, "FloatThousandsSeparator")),
				
				    FileDefaultDateInputFormats = ParseCsvToList(GetString(row, "FileDefaultDateInputFormats")),
//				    SqlDefaultDateInputFormats = ParseCsvToList(GetString(row, "SqlDefaultDateInputFormats")),
				
				    FloatColumns = new HashSet<string>(
				        ParseCsvToList(GetString(row, "FloatColumns")),
				        StringComparer.OrdinalIgnoreCase),
				
				    DateColumns = new HashSet<string>(
				        ParseCsvToList(GetString(row, "DateColumns")),
				        StringComparer.OrdinalIgnoreCase),
				
				    DateColumnInputFormats = ParseDateColumnInputFormats(
				        GetString(row, "DateColumnInputFormats")),
				
				    MonthStartDateColumns = new HashSet<string>(
				        ParseCsvToList(GetString(row, "MonthStartDateColumns")),
				        StringComparer.OrdinalIgnoreCase)
				};
				
				return config;
			}
		}

		// ================== Helpers de parsing ==================

		private string GetString(DataRow row, string columnName)
		{
			if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
			{
				return null;
			}
			return row[columnName].ToString();
		}

		private bool? GetBool(DataRow row, string columnName)
		{
			if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
			{
				return null;
			}
			return Convert.ToBoolean(row[columnName], CultureInfo.InvariantCulture);
		}

		private List<string> ParseCsvToList(string csv)
		{
			if (string.IsNullOrWhiteSpace(csv))
			{
				return new List<string>();
			}

			return csv
				.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Trim())
				.Where(x => x.Length > 0)
				.ToList();
		}

		private Dictionary<string, List<string>> ParseDateColumnInputFormats(string raw)
		{
			var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

			if (string.IsNullOrWhiteSpace(raw))
			{
				return result;
			}

			// Formato esperado: "periodo:yyyymm|fecha_mov:dd/mm/yyyy,yyyy-mm-dd"
			string[] columnSpecs = raw.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

			foreach (string spec in columnSpecs)
			{
				string[] parts = spec.Split(new[] { ':' }, 2);
				if (parts.Length != 2)
				{
					continue;
				}

				string columnName = parts[0].Trim();
				if (string.IsNullOrEmpty(columnName))
				{
					continue;
				}

				List<string> formats = parts[1]
					.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
					.Select(f => f.Trim())
					.Where(f => f.Length > 0)
					.ToList();

				if (formats.Count > 0)
				{
					result[columnName] = formats;
				}
			}

			return result;
		}

		private NumericFormatConfiguration BuildNumericFormat(string decimalSeparator, string thousandsSeparator)
		{
			if (string.IsNullOrWhiteSpace(decimalSeparator) && string.IsNullOrWhiteSpace(thousandsSeparator))
			{
				return null;
			}

			return new NumericFormatConfiguration
			{
				DecimalSeparator = string.IsNullOrWhiteSpace(decimalSeparator)
					? SystemDefaultDecimalSeparator
					: decimalSeparator,
				ThousandsSeparator = string.IsNullOrWhiteSpace(thousandsSeparator)
					? string.Empty
					: thousandsSeparator
			};
		}

		private EtlGenerationOptions GetEtlGenerationOptions2(SessionInfo si, string tableName)
		{
		    TableEtlConfiguration tableConfig = this.GetTableEtlConfigurationFromDatabase(si, tableName)
		        ?? this.GetTableEtlConfigurationFromDatabase(si, "DEFAULT")
		        ?? new TableEtlConfiguration();
		
		    var options = new EtlGenerationOptions
		    {
		        EtlMode = tableConfig.EtlMode,
		
		        PreDeleteKeyColumns = new List<string>(tableConfig.PreDeleteKeyColumns),
		
		        FloatColumnsNumericFormat = this.CloneNumericFormat(tableConfig.FloatColumnsNumericFormat),
		
		        NumericColumnFormats = tableConfig.NumericColumnFormats.ToDictionary(
		            kvp => kvp.Key,
		            kvp => this.CloneNumericFormat(kvp.Value),
		            StringComparer.OrdinalIgnoreCase),
		
		        FileDefaultDateInputFormats = new List<string>(tableConfig.FileDefaultDateInputFormats),
//		        SqlDefaultDateInputFormats = new List<string>(tableConfig.SqlDefaultDateInputFormats),
		
		        FloatColumns = new HashSet<string>(tableConfig.FloatColumns, StringComparer.OrdinalIgnoreCase),
		
		        DateColumns = new HashSet<string>(tableConfig.DateColumns, StringComparer.OrdinalIgnoreCase),
		
		        DateColumnInputFormats = tableConfig.DateColumnInputFormats.ToDictionary(
		            kvp => kvp.Key,
		            kvp => new List<string>(kvp.Value),
		            StringComparer.OrdinalIgnoreCase),
		
		        MonthStartDateColumns = new HashSet<string>(tableConfig.MonthStartDateColumns, StringComparer.OrdinalIgnoreCase)
		    };
		
		    // Construir textos para el log
		    string preDelete = string.Join(",", options.PreDeleteKeyColumns ?? new List<string>());
		    string floatCols = string.Join(",", options.FloatColumns ?? new HashSet<string>());
		    string monthCols = string.Join(",", options.MonthStartDateColumns ?? new HashSet<string>());
		    string fileDates = string.Join(",", options.FileDefaultDateInputFormats ?? new List<string>());
//		    string sqlDates = string.Join(",", options.SqlDefaultDateInputFormats ?? new List<string>());
		    string dateFormats = string.Join(" | ",
		        options.DateColumnInputFormats.Select(kvp => $"{kvp.Key}:{string.Join(",", kvp.Value)}"));
		
		    BRApi.ErrorLog.LogMessage(si,
		        $"[extender_import_data_SIC] ETL options for '{tableName}': " +
		        $"Mode={options.EtlMode}; " +
		        $"PreDeleteKeys=[{preDelete}]; " +
		        $"FloatCols=[{floatCols}]; " +
		        $"FileDateFormats=[{fileDates}]; " +
//		        $"SqlDateFormats=[{sqlDates}]; " +
		        $"MonthStartDateColumns=[{monthCols}]; " +
		        $"DateColumnInputFormats=[{dateFormats}]");
		
		    return options;
		}
		#endregion

		#endregion

		#endregion

		#region "Update Import Tracking"

		/// <summary>
		/// Upserts a row in XFC_HR_MASTER_TableImportInfo to track the last import operation.
		/// </summary>
		private void UpdateTableImportInfo(SessionInfo si, string tableName, string folderPath, string fileName)
		{
		    using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
		    {
		        var dbParamInfos = new List<DbParamInfo>
		        {
		            new DbParamInfo("paramTableName", tableName),
		            new DbParamInfo("paramUserName", si.UserName),
		            new DbParamInfo("paramFolderPath", folderPath),
		            new DbParamInfo("paramFileName", fileName),
		            new DbParamInfo("paramImportType", "file")
		        };
		        BRApi.Database.ExecuteSql(
		            dbConn,
		            @"
		            MERGE INTO XFC_HR_MASTER_TableImportInfo WITH (HOLDLOCK) AS target
		            USING (SELECT @paramTableName AS table_name) AS source
		            ON target.table_name = source.table_name
		            WHEN MATCHED THEN
		                UPDATE SET
		                    username = @paramUserName,
		                    folder_path = @paramFolderPath,
		                    file_name = @paramFileName,
		                    last_import_date = GETDATE(),
		                    import_type = @paramImportType,
		                    period = NULL
		            WHEN NOT MATCHED THEN
		                INSERT (table_name, username, folder_path, file_name, last_import_date, import_type, period)
		                VALUES (@paramTableName, @paramUserName, @paramFolderPath, @paramFileName, GETDATE(), @paramImportType, NULL);
		            ",
		            dbParamInfos,
		            false
		        );
		    }
		}

		#endregion

		#region "ExecuteSQL"
		public void ExecuteSql(SessionInfo si, string sqlCmd)
		{
			using (var dbConnApp = BRApi.Database.CreateApplicationDbConnInfo(si))
			{
				BRApi.Database.ExecuteActionQuery(dbConnApp, sqlCmd, true, true);
			}
		}
		#endregion

		#endregion

	}
}
