using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public class dynamic_grid_view : IWsasDynamicGridV800
    {
        private const string MappingTable = "XFC_HR_MASTER_Mapping";

        #region "IWsasDynamicGridV800 Implementation"

        public XFDynamicGridGetDataResult GetDynamicGridData(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardDynamicGridArgs args)
        {
            try
            {
                if (brGlobals == null || workspace == null || args == null)
                    return null;

                string componentName = args.Component.Name;

                switch (componentName)
                {
                    case "dgv_amd_TableImports":
                        return GetTableImportsGrid(si, args);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        public XFDynamicGridSaveDataResult SaveDynamicGridData(SessionInfo si, BRGlobals brGlobals, DashboardWorkspace workspace, DashboardDynamicGridArgs args)
        {
            try
            {
                // Read-only grid — no save logic needed
                return null;
            }
            catch (Exception ex)
            {
                throw new XFException(si, ex);
            }
        }

        #endregion

        #region "Grid: Table Imports"

        private XFDynamicGridGetDataResult GetTableImportsGrid(SessionInfo si, DashboardDynamicGridArgs args)
        {
            // ── Read table name: try CustomSubstVars → NameValuePairs → BRApi ──
            string tableName = GetCustomSubstVar(args, "prm_amd_Component_TableImport_TableName");

            if (string.IsNullOrEmpty(tableName))
                tableName = GetNvpValue(args, "prm_amd_Component_TableImport_TableName");

            if (string.IsNullOrEmpty(tableName))
                tableName = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "prm_amd_Component_TableImport_TableName");

            if (string.IsNullOrEmpty(tableName))
            {
                // Diagnostic dump — show all available data sources
                var diag = new StringBuilder("DynamicGrid: no table name received.");
                diag.Append(" CSV:[");
                if (args.CustomSubstVars != null)
                    foreach (var kv in args.CustomSubstVars)
                        diag.Append(kv.Key).Append("=").Append(kv.Value ?? "").Append(";");
                diag.Append("] NVP:[");
                if (args.NameValuePairs != null)
                    foreach (var kv in args.NameValuePairs)
                        diag.Append(kv.Key).Append("=").Append(kv.Value ?? "").Append(";");
                diag.Append("]");
                throw new XFException(si, new Exception(diag.ToString()));
            }

            // Validate table name format (only alphanumeric, underscore — prevent injection)
            if (!IsValidSqlIdentifier(tableName))
                throw new XFException(si, new Exception($"DynamicGrid: invalid table name '{tableName}'."));

            string tableFilters = GetCustomSubstVar(args, "prm_amd_Component_TableImport_TableFilters");
            if (string.IsNullOrEmpty(tableFilters))
                tableFilters = GetNvpValue(args, "prm_amd_Component_TableImport_TableFilters");
            if (string.IsNullOrEmpty(tableFilters))
                tableFilters = BRApi.Dashboards.Parameters.GetLiteralParameterValue(si, false, Guid.Empty, "prm_amd_Component_TableImport_TableFilters");

            // ── Pagination args ──
            DashboardDynamicGridGetDataArgs getDataArgs = args.GetDataArgs;
            int pageSize = (getDataArgs.PageSize > 0) ? getDataArgs.PageSize : 50;
            int startRow = (getDataArgs.StartRowIndex >= 0) ? getDataArgs.StartRowIndex : 0;

            // ── Get column mapping from XFC_HR_MASTER_Mapping ──
            List<ColumnMapping> columnMappings;
            using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            {
                columnMappings = GetColumnMappings(si, dbConn, tableName);

                // Fallback: if no mappings, auto-discover from INFORMATION_SCHEMA
                if (columnMappings.Count == 0)
                    columnMappings = GetColumnsFromSchema(si, dbConn, tableName);
            }

            if (columnMappings.Count == 0)
                throw new XFException(si, new Exception($"DynamicGrid: no columns found for table '{tableName}'."));

            // Validate all column names
            foreach (var col in columnMappings)
            {
                if (!IsValidSqlIdentifier(col.TargetColumn))
                    throw new XFException(si, new Exception($"DynamicGrid: invalid column name '{col.TargetColumn}'."));
            }

            // ── Build WHERE clause ──
            StringBuilder whereClause = new StringBuilder();

            // Apply dashboard table filters (from template parameters)
            if (!string.IsNullOrEmpty(tableFilters))
            {
                // Clean up brackets that come from template parameters format [filter_expression]
                string cleanedFilters = tableFilters.Trim().TrimStart('[').TrimEnd(']').Trim();
                if (!string.IsNullOrEmpty(cleanedFilters))
                {
                    whereClause.Append(" WHERE ");
                    whereClause.Append(cleanedFilters);
                }
            }

            // Apply column filters from grid UI (user search in column headers)
            if (getDataArgs.PagedGridColumnFilters != null && getDataArgs.PagedGridColumnFilters.Count > 0)
            {
                // Validate that filtered columns exist in our mapping (whitelist)
                HashSet<string> validColumns = new HashSet<string>(
                    columnMappings.Select(m => m.TargetColumn),
                    StringComparer.OrdinalIgnoreCase
                );

                foreach (PagedGridColumnFilter filter in getDataArgs.PagedGridColumnFilters)
                {
                    filter.EscapeSqlStrings();

                    if (!validColumns.Contains(filter.PropertyName))
                        continue;

                    string filterSql = BuildColumnFilterSql(filter);
                    if (string.IsNullOrEmpty(filterSql))
                        continue;

                    if (whereClause.Length == 0)
                        whereClause.Append(" WHERE ");
                    else
                        whereClause.Append(" AND ");

                    whereClause.Append(filterSql);
                }
            }

            // ── Build ORDER BY clause ──
            string orderByClause = BuildOrderByClause(getDataArgs, columnMappings);

            // ── Build column list ──
            string columnList = string.Join(", ", columnMappings.Select(m => "[" + m.TargetColumn + "]"));

            // ── Execute queries ──
            int totalRows = 0;
            DataTable dtData = null;

            using (DbConnInfo dbConn = BRApi.Database.CreateApplicationDbConnInfo(si))
            {
                // Count total rows (for pager)
                string countSql = $"SELECT COUNT(*) FROM [{tableName}] WITH(NOLOCK){whereClause}";
                DataTable dtCount = BRApi.Database.ExecuteSql(dbConn, countSql, false);
                totalRows = Convert.ToInt32(dtCount.Rows[0][0]);

                // Paginated data query
                string dataSql = $"SELECT {columnList} FROM [{tableName}] WITH(NOLOCK){whereClause} {orderByClause} OFFSET {startRow} ROWS FETCH NEXT {pageSize} ROWS ONLY";
                dtData = BRApi.Database.ExecuteSql(dbConn, dataSql, false);
            }

            // ── Build column definitions ──
            List<XFDynamicGridColumnDefinition> columnDefs = new List<XFDynamicGridColumnDefinition>();
            foreach (ColumnMapping mapping in columnMappings)
            {
                string displayName = !string.IsNullOrEmpty(mapping.DisplayName)
                    ? mapping.DisplayName
                    : mapping.TargetColumn;

                columnDefs.Add(new XFDynamicGridColumnDefinition(
                    string.Empty,                          // customParameters
                    mapping.TargetColumn,                  // columnName (must match DataTable)
                    displayName,                           // description (header text)
                    XFGridColumnDisplayType.Standard,      // columnDisplayType
                    TriStateBool.TrueValue,                // isVisible
                    TriStateBool.FalseValue,               // allowUpdates (read-only)
                    TriStateBool.TrueValue,                // isFromTable
                    string.Empty,                          // filterByColumnName
                    string.Empty,                          // parameterName
                    TriStateBool.TrueValue,                // showSearch
                    XFColor.Transparent,                   // backgroundColor
                    string.Empty,                          // defaultValue
                    TriStateBool.FalseValue,               // isMultilineText
                    "",                                    // dataFormatString
                    150                                    // width
                ));
            }

            // ── Build XFDataTable ──
            XFDataTable xfDataTable = new XFDataTable(si, dtData, null, dtData.Rows.Count);
            xfDataTable.TotalNumRowsInOriginalDataTable = totalRows;

            return new XFDynamicGridGetDataResult(xfDataTable, columnDefs, DataAccessLevel.ReadOnly);
        }

        #endregion

        #region "Helper: Column Mapping"

        private class ColumnMapping
        {
            public string TargetColumn { get; set; }
            public string DisplayName { get; set; }
        }

        /// <summary>
        /// Gets column mappings from XFC_HR_MASTER_Mapping for the given table.
        /// </summary>
        private List<ColumnMapping> GetColumnMappings(SessionInfo si, DbConnInfo dbConn, string tableName)
        {
            var mappings = new List<ColumnMapping>();

            var dbParams = new List<DbParamInfo>
            {
                new DbParamInfo("@tableName", tableName)
            };

            DataTable dt = BRApi.Database.ExecuteSql(
                dbConn,
                $"SELECT target_column, display FROM [{MappingTable}] WITH(NOLOCK) WHERE table_name = @tableName",
                dbParams,
                false
            );

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    mappings.Add(new ColumnMapping
                    {
                        TargetColumn = row["target_column"].ToString().Trim(),
                        DisplayName = row["display"] == DBNull.Value ? null : row["display"].ToString().Trim()
                    });
                }
            }

            return mappings;
        }

        /// <summary>
        /// Fallback: discover columns from INFORMATION_SCHEMA when no mapping rows exist.
        /// </summary>
        private List<ColumnMapping> GetColumnsFromSchema(SessionInfo si, DbConnInfo dbConn, string tableName)
        {
            var mappings = new List<ColumnMapping>();

            var dbParams = new List<DbParamInfo>
            {
                new DbParamInfo("@tableName", tableName)
            };

            DataTable dt = BRApi.Database.ExecuteSql(
                dbConn,
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName ORDER BY ORDINAL_POSITION",
                dbParams,
                false
            );

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    string colName = row["COLUMN_NAME"].ToString().Trim();
                    mappings.Add(new ColumnMapping
                    {
                        TargetColumn = colName,
                        DisplayName = null
                    });
                }
            }

            return mappings;
        }

        #endregion

        #region "Helper: CustomSubstVars & NameValuePairs"

        private string GetCustomSubstVar(DashboardDynamicGridArgs args, string key)
        {
            if (args == null || args.CustomSubstVars == null)
                return string.Empty;

            string value;
            if (args.CustomSubstVars.TryGetValue(key, out value))
                return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

            return string.Empty;
        }

        private string GetNvpValue(DashboardDynamicGridArgs args, string key)
        {
            if (args == null || args.NameValuePairs == null)
                return string.Empty;

            string value;
            if (args.NameValuePairs.TryGetValue(key, out value))
                return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

            return string.Empty;
        }

        #endregion

        #region "Helper: SQL Building"

        /// <summary>
        /// Validates that identifier contains only letters, digits, and underscores — prevents SQL injection for table/column names.
        /// </summary>
        private bool IsValidSqlIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                return false;
            // Allow Unicode letters (accented chars like á, é, í, ñ, etc.)
            return Regex.IsMatch(identifier, @"^[\p{L}_][\p{L}\p{N}_]*$");
        }

        /// <summary>
        /// Builds a SQL WHERE fragment for a single column filter from the grid UI.
        /// </summary>
        private string BuildColumnFilterSql(PagedGridColumnFilter filter)
        {
            if (string.IsNullOrEmpty(filter.PropertyName))
                return null;

            string columnRef = "[" + filter.PropertyName + "]";

            // Handle distinct items filter (checklist selection)
            if (filter.DistinctItems != null && filter.DistinctItems.Count > 0)
            {
                var escaped = filter.DistinctItems
                    .Select(item => "'" + item.Replace("'", "''") + "'");
                return columnRef + " IN (" + string.Join(", ", escaped) + ")";
            }

            // Build filter from operator+value pairs
            string condition1 = BuildSingleCondition(columnRef, filter.FilterOperator1, filter.FilterValue1);
            string condition2 = BuildSingleCondition(columnRef, filter.FilterOperator2, filter.FilterValue2);

            if (!string.IsNullOrEmpty(condition1) && !string.IsNullOrEmpty(condition2))
            {
                string connector = filter.UseORVersusAND ? " OR " : " AND ";
                return "(" + condition1 + connector + condition2 + ")";
            }

            return condition1 ?? condition2;
        }

        private string BuildSingleCondition(string columnRef, DbOperator op, string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            string safeValue = (value ?? string.Empty).Replace("'", "''");

            switch (op)
            {
                case DbOperator.IsEqualTo:
                    return columnRef + " = '" + safeValue + "'";
                case DbOperator.IsNotEqualTo:
                    return columnRef + " <> '" + safeValue + "'";
                case DbOperator.Contains:
                    return columnRef + " LIKE '%" + safeValue + "%'";
                case DbOperator.DoesNotContain:
                    return columnRef + " NOT LIKE '%" + safeValue + "%'";
                case DbOperator.StartsWith:
                    return columnRef + " LIKE '" + safeValue + "%'";
                case DbOperator.EndsWith:
                    return columnRef + " LIKE '%" + safeValue + "'";
                case DbOperator.IsGreaterThan:
                    return columnRef + " > '" + safeValue + "'";
                case DbOperator.IsGreaterThanOrEqualTo:
                    return columnRef + " >= '" + safeValue + "'";
                case DbOperator.IsLessThan:
                    return columnRef + " < '" + safeValue + "'";
                case DbOperator.IsLessThanOrEqualTo:
                    return columnRef + " <= '" + safeValue + "'";
                case DbOperator.In:
                    return columnRef + " IN ('" + safeValue + "')";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Builds ORDER BY clause from grid sort requests. Falls back to first column if none specified.
        /// </summary>
        private string BuildOrderByClause(DashboardDynamicGridGetDataArgs getDataArgs, List<ColumnMapping> columnMappings)
        {
            HashSet<string> validColumns = new HashSet<string>(
                columnMappings.Select(m => m.TargetColumn),
                StringComparer.OrdinalIgnoreCase
            );

            if (getDataArgs.PagedGridDbOrderBys != null && getDataArgs.PagedGridDbOrderBys.Count > 0)
            {
                var orderParts = new List<string>();
                foreach (DbOrderBy orderBy in getDataArgs.PagedGridDbOrderBys)
                {
                    orderBy.EscapeSqlStrings();

                    if (!validColumns.Contains(orderBy.ColumnName))
                        continue;

                    string direction = orderBy.IsAscending ? "ASC" : "DESC";
                    orderParts.Add("[" + orderBy.ColumnName + "] " + direction);
                }

                if (orderParts.Count > 0)
                    return "ORDER BY " + string.Join(", ", orderParts);
            }

            // Default: ORDER BY first column (required for OFFSET/FETCH)
            return "ORDER BY [" + columnMappings[0].TargetColumn + "]";
        }

        #endregion
    }
}
