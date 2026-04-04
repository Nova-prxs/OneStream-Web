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
using Microsoft.Data.SqlClient;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.new_scenarios
{
	public class MainClass
	{
		public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					case DashboardExtenderFunctionType.LoadDashboard:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement Load Dashboard logic here.
							if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize && args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
							{
								var loadDashboardTaskResult = new XFLoadDashboardTaskResult();
								loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = false;
								loadDashboardTaskResult.ModifiedCustomSubstVars = null;
								return loadDashboardTaskResult;
							}
						}
						break;
						
						
					case DashboardExtenderFunctionType.ComponentSelectionChanged:
						
						if (args.FunctionName.XFEqualsIgnoreCase("BotonNuevoEscenario"))
						{
							//Get new scenario
							string sQuery = "SELECT 'Forecast_V' + CAST(MAX(CAST(SUBSTRING(escenario, 11, LEN(escenario)) AS INT)) + 1 AS VARCHAR(10)) AS siguiente_escenario FROM XFC_HR_MASTER_GLB_Escenarios WHERE escenario LIKE 'Forecast_V%';";
							using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
								{
									DataTable dt = BRApi.Database.ExecuteSql(dbConn, sQuery, true);
									string nuevo_escenario = dt.Rows[0][0].ToString();
									BRApi.Dashboards.Parameters.SetLiteralParameterValue(si, false, "prm_WPP_Escenario_Nuevo", nuevo_escenario);
								}
							
			                return null;  
							
						}
						
						else if (args.FunctionName.XFEqualsIgnoreCase("BotonCrearEscenario"))
						{
							// Obtener parámetros del dashboard
							string param_escenario = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, "prm_WPP_Escenario_Nuevo");
							string param_descripcion = args.NameValuePairs["descripcion"];
							string param_tipo_escenario = args.NameValuePairs["tipo"];
							
							// Obtener el diccionario
							var dashDic = args.SelectionChangedTaskInfo.CustomSubstVarsWithUserSelectedValues;
							
							// Comprobar si existe la clave "prm_WPP_Escenario_Descripcion"
							if (dashDic.ContainsKey("prm_WPP_Escenario_Descripcion"))
							{
//							    // Leer el valor
//							    string x = dashDic["Param1"]?.ToString();
								BRApi.ErrorLog.LogMessage(si, "entra");
								
							    // Escribir un nuevo valor
							    dashDic["prm_WPP_Escenario_Descripcion"] = "";
							}
							
							var scstr = new XFSelectionChangedTaskResult();
							
							scstr.ChangeCustomSubstVarsInDashboard = true;
							
							scstr.ModifiedCustomSubstVars = dashDic;
							
							
							// Query SQL
							string sQuery = $@"
							INSERT INTO XFC_HR_MASTER_GLB_Escenarios (
							    periodo,
							    tipo,
							    escenario,
							    descripcion,
							    fecha
							)
							VALUES (
							    2026,
							    '{param_tipo_escenario}',
							    '{param_escenario}',
							    '{param_descripcion}',
							    GETDATE()
							);";

	
							
							using (DbConnInfoApp dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
								{
									BRApi.Database.ExecuteActionQuery(dbConn, sQuery, true, true);
								}
							
			                return scstr;  
							
						}
						
							
						break;
						
					case DashboardExtenderFunctionType.SqlTableEditorSaveData:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							// Implement SQL Table Editor Save Data logic here.
							// Save the data rows.
							// XFSqlTableEditorSaveDataTaskInfo saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
							// using (DbConnInfo dbConn = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName))
							// {
								// dbConn.BeginTrans();
								// BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, true, false, false);
								// dbConn.CommitTrans();
							// }

							var saveDataTaskResult = new XFSqlTableEditorSaveDataTaskResult();
							saveDataTaskResult.IsOK = true;
							saveDataTaskResult.ShowMessageBox = false;
							saveDataTaskResult.Message = "";
							saveDataTaskResult.CancelDefaultSave = false; // Note: Use True if we already saved the data rows in this Business Rule.
							return saveDataTaskResult;
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
