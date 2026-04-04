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
using OneStreamWorkspacesApi;
using OneStreamWorkspacesApi.V800;

namespace Workspace.__WsNamespacePrefix.__WsAssemblyName
{
    public class dynamic_dasboard_service : IWsasDynamicDashboardsV800
    {
        public WsDynamicDashboardEx GetEmbeddedDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx parentDynamicComponentEx, Dashboard storedDashboard, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    #region Table Import
                    if (storedDashboard.Name.XFEqualsIgnoreCase("00_amd_Component_TableImport"))
                    {
                        // Declare dictionary of template parameters
                        var nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>();
                        // Get parameters
                        string paramTableName = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_import_table_name");
                        string paramTableFilters = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_table_filters");
                        string paramCheckViews = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_check_views");
                        string paramCheckDashboards = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_check_dashboards");
                        string paramIsEditable = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_is_editable");
                        string paramHasFile = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_has_file");
                        string paramOnlyEdit = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_only_edit");
                        string paramEndpoint = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_import_endpoint");

                        // Set default params
                        parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Check";
                        parentDynamicComponentEx.TemplateSubstVars["p_user"] = "No data";
                        parentDynamicComponentEx.TemplateSubstVars["p_last_update_date"] = "No data";
                        parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] = "Mappings required";
                        parentDynamicComponentEx.TemplateSubstVars["p_table_pop_up"] = "SeeTable";
                        parentDynamicComponentEx.TemplateSubstVars["p_file_buttons_dashboard"] = "FileButtons_ImportData";
                        parentDynamicComponentEx.TemplateSubstVars["p_table_filters"] = "";

                        // Configure table filters if provided
                        if (!string.IsNullOrEmpty(paramTableFilters))
                        {
                            parentDynamicComponentEx.TemplateSubstVars["p_table_filters"] = "WHERE " + paramTableFilters;
                        }

                        if (string.IsNullOrEmpty(paramTableName))
                            throw new Exception("Error loading table import component: no table name specified");

                        if (!string.IsNullOrEmpty(paramEndpoint))
                        {
                            parentDynamicComponentEx.TemplateSubstVars["p_file_buttons_dashboard"] =
                                paramHasFile == "1"
                                    ? "FileButtons_ImportData_RESTAPI_Download"
                                    : "FileButtons_ImportData_RESTAPI";
                        }

                        using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
                        {
                            var dbParamInfos = new List<DbParamInfo>
                            {
                                new DbParamInfo("paramTable", paramTableName)
                            };
                            DataTable dt;

                            // Get table import information
                            try
                            {
                                dt = BRApi.Database.ExecuteSql(
                                    dbConn,
                                    @"
                                    SELECT *
                                    FROM XFC_HR_MASTER_TableImportInfo WITH(NOLOCK)
                                    WHERE table_name = @paramTable
                                    ",
                                    dbParamInfos,
                                    false
                                );

                                // Control if table has been uploaded
                                if (dt == null || dt.Rows.Count == 0)
                                {
                                    parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Error";
                                    parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                        "Import information for this table not found\r\n" +
                                        "Please, import a file.";
                                    goto AfterViewCheck;
                                }
                                else
                                {
                                    parentDynamicComponentEx.TemplateSubstVars["p_user"] =
                                        dt.Rows[0]["username"].ToString();
                                    parentDynamicComponentEx.TemplateSubstVars["p_last_update_date"] =
                                        string.Format("{0:yyyy/MM/dd HH:mm}", dt.Rows[0]["last_import_date"]);
                                    parentDynamicComponentEx.TemplateSubstVars["p_last_imported_file_full_name"] =
                                        dt.Rows[0]["folder_path"].ToString() + "/" + dt.Rows[0]["file_name"].ToString();
                                }
                            }
                            catch (Exception ex)
                            {
                                parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Error";
                                string errorString = ex.ToString();
                                if (errorString.Contains("Invalid object name"))
                                    errorString = "Information Table does not exist";
                                parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                    $"Error getting table info: {errorString}\r\n" +
                                    "Please, talk with an administrator.";
                                goto AfterViewCheck;
                            }

                            // Get view if provided
                            if (!string.IsNullOrEmpty(paramCheckViews))
                            {
                                string[] checkViewArray = paramCheckViews.Split(',');
                                string[] checkDashboardArray = paramCheckDashboards.Split(',');

                                // Check number of check views and check dashboards
                                if (checkViewArray.Length > checkDashboardArray.Length || string.IsNullOrEmpty(paramCheckDashboards))
                                {
                                    parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Error";
                                    parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                        "There are more Check Views than Check Dashboards.\r\n" +
                                        "Please, talk with an administrator.";
                                    goto AfterViewCheck;
                                }

                                int i = 0;
                                // Check if there are any transformations not found
                                foreach (string checkView in checkViewArray)
                                {
                                    try
                                    {
                                        dt = BRApi.Database.ExecuteSql(
                                            dbConn,
                                            $@"
                                            SELECT *
                                            FROM {checkView.Trim()} WITH(NOLOCK)
                                            ",
                                            dbParamInfos,
                                            false
                                        );

                                        if (dt != null && dt.Rows.Count > 0)
                                        {
                                            string checkDashboard = checkDashboardArray[i].Trim();
                                            parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Warning";
                                            parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                                "Transformations not found.\r\n" +
                                                "Click to go to check dashboard.";
                                            parentDynamicComponentEx.TemplateSubstVars["p_check_dashboard"] = checkDashboard;
                                            break;
                                        }

                                        i++;
                                    }
                                    catch (Exception ex)
                                    {
                                        parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Error";
                                        string errorString = ex.ToString();
                                        if (errorString.Contains("Invalid object name"))
                                            errorString = $"Check view '{checkView}' does not exist";
                                        parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                            $"Error getting check view: {errorString}\r\n" +
                                            "Please, talk with an administrator.";
                                        break;
                                    }
                                }
                            }
                        AfterViewCheck:;
                        }

                        // If it is editable, set edit pop up button instead of see table
                        if (!string.IsNullOrEmpty(paramIsEditable) && paramIsEditable == "1")
                        {
                            parentDynamicComponentEx.TemplateSubstVars["p_table_pop_up"] = "EditTable";
                        }

                        // If it has no file, set file buttons dashboard to transparent
                        if (!string.IsNullOrEmpty(paramOnlyEdit) && paramOnlyEdit == "1")
                        {
                            parentDynamicComponentEx.TemplateSubstVars["p_file_buttons_dashboard"] = "Transparent";
                        }

                        // Set import button visibility based on context
                        string paramContext = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_amd_context");
                        parentDynamicComponentEx.TemplateSubstVars["p_visible"] =
                            (!string.IsNullOrEmpty(paramContext) && !paramContext.XFEqualsIgnoreCase("AMD"))
                                ? "False"
                                : "True";

                        // ---------------------------------------------------------- SET UP DASHBOARD --------------------------------------------------------
                        // Set up embedded dashboard
                        WsDynamicDashboardEx contentDashboard = api.GetEmbeddedDynamicDashboard(si,
                            workspace, parentDynamicComponentEx, storedDashboard,
                            string.Empty, nextLevelTemplateSubstVarsToAdd, TriStateBool.Unknown, WsDynamicItemStateType.Unknown
                        );
                        // Save the state and return the dashboard
                        api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent,
                            contentDashboard, WsDynamicItemStateType.NotUsed
                        );
                        if (contentDashboard.DynamicDashboard.Dashboard != null) return contentDashboard;
                    }

                    #endregion

                    #region Check

                    else if (storedDashboard.Name.XFEqualsIgnoreCase("00_amd_Component_Check"))
                    {
                        // Declare dictionary of template parameters
                        var nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>();

                        // Get parameters
                        string paramTableName = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_import_table_name");
                        string paramCheckView = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_check_view");
                        string paramTableFilters = parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_table_filters");

                        // Set default params
                        parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Check";
                        parentDynamicComponentEx.TemplateSubstVars["p_user"] = "No data";
                        parentDynamicComponentEx.TemplateSubstVars["p_last_update_date"] = "No data";
                        parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] = "Mappings required";

                        // Configure table filters if provided
                        if (!string.IsNullOrEmpty(paramTableFilters))
                        {
                            paramTableFilters = "WHERE " + paramTableFilters;
                        }

                        if (string.IsNullOrEmpty(paramTableName))
                            throw new Exception("Error loading table import component: no table name specified");

                        using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
                        {
                            var dbParamInfos = new List<DbParamInfo>
                            {
                                new DbParamInfo("paramTable", paramTableName)
                            };
                            DataTable dt;

                            // Get table import information
                            try
                            {
                                dt = BRApi.Database.ExecuteSql(
                                    dbConn,
                                    @"
                                    SELECT *
                                    FROM XFC_HR_MASTER_TableImportInfo WITH(NOLOCK)
                                    WHERE table_name = @paramTable
                                    ",
                                    dbParamInfos,
                                    false
                                );

                                // Control if table has been uploaded
                                if (dt == null || dt.Rows.Count == 0)
                                {
                                    parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Error";
                                    parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                        "Import information for this table not found\r\n" +
                                        "Please, import a file.";
                                    goto AfterViewCheckSecond;
                                }
                            }
                            catch (Exception ex)
                            {
                                parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Error";
                                string errorString = ex.ToString();
                                if (errorString.Contains("Invalid object name"))
                                    errorString = "Information Table does not exist";
                                parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                    $"Error getting table info: {errorString}\r\n" +
                                    "Please, talk with an administrator.";
                                goto AfterViewCheckSecond;
                            }

                            // Get view if provided
                            if (!string.IsNullOrEmpty(paramCheckView))
                            {
                                try
                                {
                                    dt = BRApi.Database.ExecuteSql(
                                        dbConn,
                                        $@"
                                        SELECT *
                                        FROM {paramCheckView.Trim()} WITH(NOLOCK)
                                        {paramTableFilters};
                                        ",
                                        dbParamInfos,
                                        false
                                    );

                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Warning";
                                        parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                            "Transformations not found.\r\n" +
                                            "Click to open check dashboard.";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    parentDynamicComponentEx.TemplateSubstVars["p_status"] = "Error";
                                    string errorString = ex.ToString();
                                    if (errorString.Contains("Invalid object name"))
                                        errorString = $"Check view '{paramCheckView}' does not exist";
                                    parentDynamicComponentEx.TemplateSubstVars["p_warning_message"] =
                                        $"Error getting check view: {errorString}\r\n" +
                                        "Please, talk with an administrator.";
                                }
                            }
                        AfterViewCheckSecond:;
                        }

                        // ---------------------------------------------------------- SET UP DASHBOARD --------------------------------------------------------
                        // Set up embedded dashboard
                        WsDynamicDashboardEx contentDashboard = api.GetEmbeddedDynamicDashboard(si,
                            workspace, parentDynamicComponentEx, storedDashboard,
                            string.Empty, nextLevelTemplateSubstVarsToAdd, TriStateBool.Unknown, WsDynamicItemStateType.Unknown
                        );
                        // Save the state and return the dashboard
                        api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent,
                            contentDashboard, WsDynamicItemStateType.NotUsed
                        );
                        if (contentDashboard.DynamicDashboard.Dashboard != null) return contentDashboard;
                    }

                    #endregion

                    #region Dynamic Repeaters

                    else if (storedDashboard.Name.XFEqualsIgnoreCase("01_amd_Data_Repeater")
                          || storedDashboard.Name.XFEqualsIgnoreCase("01_amd_Data")
                          || storedDashboard.Name.XFEqualsIgnoreCase("01_amd_Metadata_Repeater")
                          || storedDashboard.Name.XFEqualsIgnoreCase("01_amd_Metadata"))
                    {
                        var nextLevelTemplateSubstVarsToAdd = new Dictionary<string, string>();

                        // Determine context: AMD (default) or WPP
                        // 1. Check TemplateSubstVars (available for repeater template instances)
                        // 2. Check component name suffix (e.g. "Embedded 01_amd_Data_WPP" → WPP)
                        // 3. Fallback to customSubstVarsAlreadyResolved
                        string context = "AMD";
                        string ctxFromTemplate = parentDynamicComponentEx.TemplateSubstVars != null
                            ? parentDynamicComponentEx.TemplateSubstVars.XFGetValue("p_amd_context")
                            : "";

                        if (!string.IsNullOrEmpty(ctxFromTemplate))
                        {
                            context = ctxFromTemplate;
                        }
                        else
                        {
                            // Detect context from the embedding component name suffix
                            string compName = parentDynamicComponentEx.DynamicComponent != null
                                ? parentDynamicComponentEx.DynamicComponent.ToString()
                                : "";
                            if (!string.IsNullOrEmpty(compName) && compName.IndexOf("_WPP", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                context = "WPP";
                            }
                            else if (customSubstVarsAlreadyResolved != null && customSubstVarsAlreadyResolved.ContainsKey("p_amd_context"))
                            {
                                context = customSubstVarsAlreadyResolved["p_amd_context"];
                            }
                        }

                        // Propagate context to child dashboards
                        nextLevelTemplateSubstVarsToAdd["p_amd_context"] = context;

                        // Set up embedded dashboard
                        WsDynamicDashboardEx contentDashboard = api.GetEmbeddedDynamicDashboard(si,
                            workspace, parentDynamicComponentEx, storedDashboard,
                            string.Empty, nextLevelTemplateSubstVarsToAdd, TriStateBool.Unknown, WsDynamicItemStateType.Unknown
                        );

                        // Inject ComponentTemplateRepeatItems from database into the dashboard definition
                        try
                        {
                            string dashboardName = storedDashboard.Name;
                            Dashboard dashboard = contentDashboard.DynamicDashboard.Dashboard;
                            DashboardDefinition dashDef = dashboard.GetDashboardDefinitionFromXmlData();
                            var repeatItems = dashDef.DynamicDashboardDefinition.ComponentTemplateRepeatItems;

                            // Always clear static items to prevent double-repeat between parent and _Repeater levels
                            repeatItems.Clear();

                            // Only inject DB items into _Repeater dashboards (the actual repeaters)
                            // Parent dashboards (01_amd_Data, 01_amd_Metadata) are just containers
                            bool isRepeater = dashboardName.IndexOf("_Repeater", StringComparison.OrdinalIgnoreCase) >= 0;

                            if (isRepeater)
                            {
                                using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
                                {
                                    var dbParamInfos = new List<DbParamInfo>
                                    {
                                        new DbParamInfo("dashboardName", dashboardName)
                                    };

                                    DataTable dt = BRApi.Database.ExecuteSql(
                                        dbConn,
                                        @"
                                        SELECT *
                                        FROM XFC_HR_AMD_ComponentConfig WITH(NOLOCK)
                                        WHERE dashboard_name = @dashboardName
                                          AND is_active = 1
                                        ORDER BY sort_order
                                        ",
                                        dbParamInfos,
                                        false
                                    );

                                    if (dt != null && dt.Rows.Count > 0)
                                    {
                                        foreach (DataRow row in dt.Rows)
                                        {
                                            string templateNameSuffix = row["template_name_suffix"].ToString();
                                            string templateParameterValues = string.Format(
                                                "p_public_name=[{0}], p_import_endpoint=[{1}], p_table_name=[{2}], p_import_table_name=[{3}], p_import_folder=[{4}], p_import_files_type=[{5}], p_import_method=[{6}], p_import_delimiter=[{7}], p_has_file=[{8}], p_amd_context=[{9}]",
                                                row["p_public_name"],
                                                row["p_import_endpoint"],
                                                row["p_table_name"],
                                                row["p_import_table_name"],
                                                row["p_import_folder"],
                                                row["p_import_files_type"],
                                                row["p_import_method"],
                                                row["p_import_delimiter"],
                                                row["p_has_file"],
                                                context
                                            );

                                            repeatItems.Add(new XFDynamicDashboardTemplateItemDefinition(templateNameSuffix, templateParameterValues));
                                        }
                                    }
                                }
                            }

                            dashboard.SetDashboardDefinitionXmlData(dashDef);
                        }
                        catch
                        {
                            // Fallback: if config table doesn't exist, use default behavior
                        }

                        // Save the state and return the dashboard
                        api.SaveDynamicDashboardState(si, parentDynamicComponentEx.DynamicComponent,
                            contentDashboard, WsDynamicItemStateType.NotUsed
                        );
                        if (contentDashboard.DynamicDashboard.Dashboard != null) return contentDashboard;
                    }

                    #endregion

                    else
                    {
                        return api.GetEmbeddedDynamicDashboard(si, workspace, parentDynamicComponentEx, storedDashboard, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicComponentCollection GetDynamicComponentsForDynamicDashboard(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicDashboardEx dynamicDashboardEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicComponentsForDynamicDashboard(si, workspace, dynamicDashboardEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicAdapterCollection GetDynamicAdaptersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicAdaptersForDynamicComponent(si, workspace, dynamicComponentEx, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicCubeViewEx GetDynamicCubeViewForDynamicAdapter(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicAdapterEx dynamicAdapterEx, CubeViewItem storedCubeViewItem, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicCubeViewForDynamicAdapter(si, workspace, dynamicAdapterEx, storedCubeViewItem, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public WsDynamicParameterCollection GetDynamicParametersForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace,
            DashboardMaintUnit maintUnit, WsDynamicComponentEx dynamicComponentEx, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicParametersForDynamicComponent(si, workspace, dynamicComponentEx, string.Empty, null, TriStateBool.FalseValue, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        WsDynamicCubeViewEx GetDynamicCubeViewForDynamicComponent(SessionInfo si, IWsasDynamicDashboardsApiV800 api, DashboardWorkspace workspace, DashboardMaintUnit maintUnit,
            WsDynamicComponentEx dynamicComponentEx, CubeViewItem storedCubeViewItem, Dictionary<string, string> customSubstVarsAlreadyResolved)
        {
            try
            {
                if (api != null)
                {
                    return api.GetDynamicCubeViewForDynamicComponent(si, workspace, dynamicComponentEx, storedCubeViewItem, string.Empty, null, TriStateBool.Unknown, WsDynamicItemStateType.Unknown);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }
    }
}
