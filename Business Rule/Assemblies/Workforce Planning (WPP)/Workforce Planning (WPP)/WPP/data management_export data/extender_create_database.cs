using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.CSharp;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.Extender.extender_create_database
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, ExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case ExtenderFunctionType.Unknown:
						break;
					case ExtenderFunctionType.ExecuteDataMgmtBusinessRuleStep:
//						var dbHelper = new Workspace.__WsNamespacePrefix.__WsAssemblyName.create_database();
//						var sharedFunction = new Workspace.__WsNamespacePrefix.__WsAssemblyName.ExportTables();
//						int iAction = args.NameValuePairs.GetValueOrDefault("p_action", "0").XFConvertToInt();
//						dbHelper.CreateDatabase(si, iAction);
						
						int iAction = args.NameValuePairs.GetValueOrDefault("p_action", "0").XFConvertToInt();
						var schemaHelper = new SchemaCsvSqlHelper();
						var csvFolder = "Documents/Users/nova/WPP/DatabaseSchema";
						var outFolder = "Documents/Users/nova/WPP/DatabaseSchema";
						var alterTemplateFolder = "Documents/Users/nova/WPP/DatabaseSchema/AlterTemplates";
						var alterGeneratedFolder = "Documents/Users/nova/WPP/DatabaseSchema/AlterTemplates";
						
						BRApi.ErrorLog.LogMessage(si, $"Schema action: {iAction}");

						switch (iAction)
						{
							case 1:
								// SOLO generar CREATE en memoria
								schemaHelper.GenerateCreateSchemaScript(si, csvFolder);
								break;

							case 2:
								// Generar CREATE + DELETE en memoria
								schemaHelper.GenerateCreateAndDeleteScripts(si, csvFolder);
								break;

							case 3:
								// SOLO generar DELETE en memoria
								schemaHelper.GenerateDeleteSchemaScript(si, csvFolder);
								break;

							case 4:
								// Guardar CREATE manual
								schemaHelper.SaveScriptToApplicationDb(
									si,
									outFolder,
									"Manual_Create.sql",
									schemaHelper.GenerateCreateSchemaScript(si, csvFolder)
								);
								break;

							case 5:
								// Ejecutar DELETE manual (generado al vuelo)
								schemaHelper.ExecuteScript(si, schemaHelper.GenerateDeleteSchemaScript(si, csvFolder));
								break;

							case 6:
								// Generar + guardar SOLO CREATE
								schemaHelper.GenerateAndSaveScript(si, csvFolder, outFolder, "Generated_CreateSchema.sql");
								break;

							case 7:
								// Generar + guardar CREATE y DELETE
								schemaHelper.GenerateAndSaveScripts(si, csvFolder, outFolder, "Generated_CreateSchema.sql", "Generated_DeleteData.sql");
								break;

							case 8:
								// Generar + guardar + ejecutar CREATE
								schemaHelper.GenerateSaveAndExecute(si, csvFolder, outFolder, "Generated_CreateSchema.sql");
								break;

							case 9:
								// Generar + guardar + ejecutar DELETE
								schemaHelper.GenerateSaveAndExecuteDelete(si, csvFolder, outFolder, "Generated_DeleteData.sql");
								break;

							case 10:
								// Ejecutar DELETE directo desde CSV
								schemaHelper.ExecuteDeleteOnlyFromCsv(si, csvFolder);
								break;

							case 11:
								// SOLO generar DROP en memoria
								schemaHelper.GenerateDropSchemaScript(si, csvFolder);
								break;

							case 12:
								// Generar + guardar DROP
								schemaHelper.GenerateAndSaveDropScript(si, csvFolder, outFolder, "Generated_DropSchema.sql");
								break;

							case 13:
								// Generar + guardar + ejecutar DROP
								schemaHelper.GenerateSaveAndExecuteDrop(si, csvFolder, outFolder, "Generated_DropSchema.sql");
								break;

							case 14:
								// Guardar los 3 scripts: CREATE + DELETE + DROP
								schemaHelper.GenerateAndSaveScripts(si, csvFolder, outFolder, "Generated_CreateSchema.sql", "Generated_DeleteData.sql");
								schemaHelper.GenerateAndSaveDropScript(si, csvFolder, outFolder, "Generated_DropSchema.sql");
								break;
								
							case 15:
								// Generar los 3 archivos y ejecutar CREATE
								schemaHelper.GenerateAndSaveScripts(si, csvFolder, outFolder, "Generated_CreateSchema.sql", "Generated_DeleteData.sql");
								schemaHelper.GenerateAndSaveDropScript(si, csvFolder, outFolder, "Generated_DropSchema.sql");
								schemaHelper.ExecuteScript(si, schemaHelper.GenerateCreateSchemaScript(si, csvFolder));
								break;

							case 16:
								// Generar los 3 archivos y ejecutar DROP + CREATE
								schemaHelper.GenerateAndSaveScripts(si, csvFolder, outFolder, "Generated_CreateSchema.sql", "Generated_DeleteData.sql");
								schemaHelper.GenerateAndSaveDropScript(si, csvFolder, outFolder, "Generated_DropSchema.sql");
								schemaHelper.ExecuteScript(si, schemaHelper.GenerateDropSchemaScript(si, csvFolder));
								schemaHelper.ExecuteScript(si, schemaHelper.GenerateCreateSchemaScript(si, csvFolder));
								break;

							case 17:
								// Solo ejecutar CREATE
								schemaHelper.ExecuteScript(si, schemaHelper.GenerateCreateSchemaScript(si, csvFolder));
								break;

							case 18:
								// Solo ejecutar DROP
								schemaHelper.ExecuteScript(si, schemaHelper.GenerateDropSchemaScript(si, csvFolder));
								break;

							case 19:
								// Solo ejecutar DELETE
								schemaHelper.ExecuteScript(si, schemaHelper.GenerateDeleteSchemaScript(si, csvFolder));
								break;

							case 20:
								// Procesar plantillas XLSX de ALTER TABLE y renombrar a _Ok/_Error
								List<SchemaCsvSqlHelper.AlterTemplateProcessResult> alterResults =
									schemaHelper.ProcessAlterTemplatesFromFolder(si, alterTemplateFolder, alterGeneratedFolder, true);

								foreach (SchemaCsvSqlHelper.AlterTemplateProcessResult result in alterResults)
								{
									BRApi.ErrorLog.LogMessage(si, $"ALTER TEMPLATE [{result.Status}] {result.FileName} -> {result.Message}");
								}
								break;
								
							default:
								throw new ArgumentOutOfRangeException(nameof(iAction), iAction, "Acción no soportada.");
						}
						break;
				}
				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}
	}
}
