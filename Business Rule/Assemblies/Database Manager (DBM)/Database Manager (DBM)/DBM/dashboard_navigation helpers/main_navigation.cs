using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using OneStream.Finance.Database;
using OneStream.Finance.Engine;
using OneStream.Shared.Common;
using OneStream.Shared.Database;
using OneStream.Shared.Engine;
using OneStream.Shared.Wcf;
using OneStream.Stage.Database;
using OneStream.Stage.Engine;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName.BusinessRule.DashboardExtender.main_navigation
{
	public class MainClass
	{
		//Reference "UTI_SharedFunctionsCS" Business Rule
		OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass UTISharedFunctionsBR = new OneStream.BusinessRule.Finance.UTI_SharedFunctionsCS.MainClass();
		
		public object Main(SessionInfo si, BRGlobals globals, object api, DashboardExtenderArgs args)
		{
			try
			{
				switch (args.FunctionType)
				{
					#region "Load Dashboard"
					
					case DashboardExtenderFunctionType.LoadDashboard:
						if (args.FunctionName.XFEqualsIgnoreCase("LoadDashboard"))
						{
							//Get workspace prefix
							string WorkspacePrefix = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "DBM.prm_DBM_Workspace_Prefix");
							//Declare load dashboard task result
							XFLoadDashboardTaskResult loadDashboardTaskResult = new XFLoadDashboardTaskResult();
							loadDashboardTaskResult.ChangeCustomSubstVarsInDashboard = true;
							//Declare current dashboard
							string parDBMashboard;
							
							//If first load, get default parameter, else use the subst var from prior run
							if (args.LoadDashboardTaskInfo.Reason == LoadDashboardReasonType.Initialize
								&& args.LoadDashboardTaskInfo.Action == LoadDashboardActionType.BeforeFirstGetParameters)
							{
								//Get default dashboard
								parDBMashboard = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, $"DBM.prm_{WorkspacePrefix}_MainNavigation_Default");
							}
							else
							{
								//Get current dashboard parameter from other business rule call
								parDBMashboard = args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_CurrentDashboard");
							}
							//Set current dashboard parameter
							loadDashboardTaskResult.ModifiedCustomSubstVars["prm_CurrentDashboard"] = parDBMashboard;
							
							//Loop through each level of the dashboard based on the name
							string[] dashboardSplitted = parDBMashboard.Split('_');
							for (int level = 1; level <= dashboardSplitted.Length - 1; level++)
							{
								int activeDashboardNumber = Convert.ToInt32(dashboardSplitted[level]);
								
								//Set dashboard content and active tab
								this.SetActiveContent(si, ref loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber);
								this.SetActiveTab(si, ref loadDashboardTaskResult, WorkspacePrefix, level, 20, activeDashboardNumber);
								//Set dashboard parameters
								this.SetDashboardParametersOnLoad(si, args, ref loadDashboardTaskResult, WorkspacePrefix, level, activeDashboardNumber);
							}
							
							return loadDashboardTaskResult;
						}
						break;
					
					#endregion
					
					#region "Component Selection Changed"
					
					case DashboardExtenderFunctionType.ComponentSelectionChanged:
						
						#region "Open Dashboard"
						
						if (args.FunctionName.XFEqualsIgnoreCase("OpenDashboard"))
						{
							//Get dashboard
							string parDBMashboard = args.NameValuePairs["p_dashboard"];
							//Set current dashboard
							XFSelectionChangedTaskResult selectionChangedTaskResult = new XFSelectionChangedTaskResult();
							selectionChangedTaskResult.ChangeCustomSubstVarsInDashboard = true;
							selectionChangedTaskResult.ModifiedCustomSubstVars["prm_CurrentDashboard"] = parDBMashboard;
							
							//Control dashboard parameters on click
							this.SetDashboardParametersOnClick(si, args, ref selectionChangedTaskResult, parDBMashboard);
							
							return selectionChangedTaskResult;
						}
						
						#endregion
						
						break;
					
					#endregion
					
					#region "SQL Table Editor Save Data"
					
					case DashboardExtenderFunctionType.SqlTableEditorSaveData:
						if (args.FunctionName.XFEqualsIgnoreCase("TestFunction"))
						{
							//Implement SQL Table Editor Save Data logic here.
							//Save the data rows.
							//XFSqlTableEditorSaveDataTaskInfo saveDataTaskInfo = args.SqlTableEditorSaveDataTaskInfo;
							//using (DbConnInfo dbConn = BRApi.Database.CreateDbConnInfo(si, saveDataTaskInfo.SqlTableEditorDefinition.DbLocation, saveDataTaskInfo.SqlTableEditorDefinition.ExternalDBConnName))
							//{
							//	dbConn.BeginTrans();
							//	BRApi.Database.SaveDataTableRows(dbConn, saveDataTaskInfo.SqlTableEditorDefinition.TableName, saveDataTaskInfo.Columns, saveDataTaskInfo.HasPrimaryKeyColumns, saveDataTaskInfo.EditedDataRows, true, false, false);
							//	dbConn.CommitTrans();
							//}
							
							XFSqlTableEditorSaveDataTaskResult saveDataTaskResult = new XFSqlTableEditorSaveDataTaskResult();
							saveDataTaskResult.IsOK = true;
							saveDataTaskResult.ShowMessageBox = false;
							saveDataTaskResult.Message = "";
							saveDataTaskResult.CancelDefaultSave = false; //Note: Use True if we already saved the data rows in this Business Rule.
							return saveDataTaskResult;
						}
						break;
						
					#endregion
				}

				return null;
			}
			catch (Exception ex)
			{
				throw ErrorHandler.LogWrite(si, new XFException(si, ex));
			}
		}
		
		#region "Helper Functions"
		
		#region "Set Active Content"
		
		private void SetActiveContent(
			SessionInfo si, ref XFLoadDashboardTaskResult loadDashboardTaskResult,
			string WorkspacePrefix, int level, int activeDashboardNumber
		)
		{
			//Set dynamic content parameter
			//If level is 1, set the initial dashboard, else append to the last level parameter
			if (level == 1)
			{
				loadDashboardTaskResult.ModifiedCustomSubstVars[$"prm_Dashboard_Tab_Level{level}_Content"] = $"{WorkspacePrefix}_{activeDashboardNumber}";
			}
			else
			{
				loadDashboardTaskResult.ModifiedCustomSubstVars[$"prm_Dashboard_Tab_Level{level}_Content"] =
					loadDashboardTaskResult.ModifiedCustomSubstVars[$"prm_Dashboard_Tab_Level{level - 1}_Content"] +
					$"_{activeDashboardNumber}";
			}
		}
		
		#endregion
		
		#region "Set Active Tab"
		
		private void SetActiveTab(
			SessionInfo si, ref XFLoadDashboardTaskResult loadDashboardTaskResult,
			string WorkspacePrefix, int level, int quantity, int activeDashboardNumber
		)
		{
			//Get colors for each level
			string level1Active = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "Shared.prm_Style_Colors_LeftBar_Tab1_Active");
			string level2Active = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "Shared.prm_Style_Colors_LeftBar_Tab2_Active");
			string level3Active = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "Shared.prm_Style_Colors_LeftBar_Tab3_Active");
			
			//Set colors for each level
			//Color tuple definition: (ActiveColor, InactiveColor)
			var colorDictionary = new Dictionary<int, Tuple<string, string>> {
				{1, new Tuple<string, string>(level1Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "Shared.prm_Style_Colors_LeftBarBG"))},
				{2, new Tuple<string, string>(level2Active, BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "Shared.prm_Style_Colors_LeftBarBG"))},
				{3, new Tuple<string, string>(level3Active, "White")},
				{4, new Tuple<string, string>(level3Active, "White")},
				{5, new Tuple<string, string>(level3Active, "White")}
			};
			
			//Loop through all the repeater components
			for (int i = 1; i <= quantity; i++)
			{
				//Control left tabs
				if (level == 1)
				{
					loadDashboardTaskResult.ModifiedCustomSubstVars[$"prm_{WorkspacePrefix}_{i}_LeftTabs"] =
						i == activeDashboardNumber ? $"{WorkspacePrefix}_{i}_LeftTabs" : $"{WorkspacePrefix}_Transparent";
				}
				loadDashboardTaskResult.ModifiedCustomSubstVars[$"prm_Dashboard_Tab_Level{level}_ActiveColor_{i}"] =
					i == activeDashboardNumber ? colorDictionary[level].Item1 : colorDictionary[level].Item2;
			}
		}
		
		#endregion
		
		#region "Set Dashboards Parameters On Load"
		
		private void SetDashboardParametersOnLoad(
			SessionInfo si, DashboardExtenderArgs args, ref XFLoadDashboardTaskResult loadDashboardTaskResult,
			string WorkspacePrefix, int level, int activeDashboardNumber
		)
		{
			//Control dashboard parameters
			//EXAMPLE
//			if (level == 1)
//			{
//				if (activeDashboardNumber == 3)
//				{
//					loadDashboardTaskResult.ModifiedCustomSubstVars["prm_WorkspaceTemplateName_Scenario_Main"] = "Planning";
//					if (args.LoadDashboardTaskInfo.CustomSubstVarsFromPriorRun.XFGetValue("prm_WorkspaceTemplateName_Scenario") == "Actual")
//						loadDashboardTaskResult.ModifiedCustomSubstVars["prm_WorkspaceTemplateName_Scenario"] = "Budget";
//				}
//				else
//				{
//					loadDashboardTaskResult.ModifiedCustomSubstVars["prm_WorkspaceTemplateName_Scenario_Main"] = "Actual";
//					loadDashboardTaskResult.ModifiedCustomSubstVars["prm_WorkspaceTemplateName_Scenario"] = "Actual";
//				}
//			}
		}
		
		#endregion
		
		#region "Set Dashboards Parameters On Click"
		
		private void SetDashboardParametersOnClick(
			SessionInfo si, DashboardExtenderArgs args, ref XFSelectionChangedTaskResult selectionChangedTaskResult,
			string parDBMashboard
		)
		{
			//Control dashboard parameters
			//EXAMPLE
//			if (parDBMashboard == "WorkspaceTemplateName_1_1")
//				selectionChangedTaskResult.ModifiedCustomSubstVars["prm_WorkspaceTemplateName_Request_Id"] = "";
		}
		
		#endregion
		
		#endregion
		
	}
}
