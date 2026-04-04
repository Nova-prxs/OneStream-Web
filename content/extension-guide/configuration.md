# Configuration Reference

This page documents all settings for the OneStream AI Helper extension, organized by category. All settings are configured in VS Code's `settings.json` file and can be accessed through the Settings UI (`Ctrl+,`).

## File and Export Settings

Settings that control file handling and export behavior.

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.files.defaultExportFormat` | string | `"csv"` | Default export format for data exports: `csv`, `xlsx`, `markdown`, or `docx` |
| `onestream.files.exportDirectory` | string | `""` | Default directory for exported files. Empty uses the workspace root. |
| `onestream.files.csvDelimiter` | string | `","` | Delimiter character for CSV exports |
| `onestream.files.csvEncoding` | string | `"utf-8"` | Character encoding for CSV exports: `utf-8`, `utf-16`, `ascii`, or `windows-1252` |
| `onestream.files.includeTimestamp` | boolean | `true` | Append a timestamp to exported file names to prevent overwriting |
| `onestream.files.openAfterExport` | boolean | `true` | Automatically open the file after exporting |
| `onestream.files.maxExportRows` | number | `100000` | Maximum number of rows to include in a single export |
| `onestream.files.wordTemplate` | string | `""` | Path to a custom Word template (.dotx) for DOCX exports |

## Sync and File Management Settings

Settings that control how rules and assemblies are synchronized between VS Code and the OneStream server.

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.sync.autoCheckSyncStatus` | boolean | `false` | Automatically check if local files are in sync with the server |
| `onestream.sync.syncCheckInterval` | number | `300` | Seconds between automatic sync status checks (range: 60--3600) |
| `onestream.sync.pullOnOpen` | boolean | `false` | Automatically pull the latest version when opening a rule file |
| `onestream.sync.confirmBeforePush` | boolean | `true` | Show a confirmation dialog before pushing a rule to the server |
| `onestream.sync.backupBeforePull` | boolean | `false` | Create a backup of the local file before pulling from the server |

## SQL Query Editor Settings

Settings that control the SQL Query Editor's behavior and appearance.

| Setting | Type | Default | Valid Values | Description |
|---|---|---|---|---|
| `onestream.sql.defaultLayout` | string | `"current"` | `current`, `beside`, `below`, `active` | Where the SQL Query Editor opens relative to the current editor |
| `onestream.sql.schemaLoadTimeout` | number | `120` | 30--600 | Seconds to wait for the database schema to load |
| `onestream.sql.queryExecutionTimeout` | number | `120` | 30--600 | Seconds to wait for a query to complete before timing out |
| `onestream.sql.schemaSidebarCollapsed` | boolean | `false` | -- | Whether the schema sidebar starts in a collapsed state |
| `onestream.sql.schemaSidebarWidth` | number | `280` | 150--600 | Width of the schema sidebar in pixels |
| `onestream.sql.compactMode` | boolean | `false` | -- | Reduce padding and font sizes for smaller screens |

## SQL Safety Settings

Settings that control the SQL Query Editor's safety features to prevent accidental data modification.

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.sql.safetyEnabled` | boolean | `true` | Master toggle for SQL safety checks. When enabled, only SELECT statements are allowed by default. |
| `onestream.sql.allowedTablePrefixes` | array | `[]` | Array of table name prefixes that are allowed for modification even when safety is enabled (e.g., `["Staging_", "Temp_"]`) |
| `onestream.sql.allowedTables` | array | `[]` | Array of specific table names that are allowed for modification even when safety is enabled |
| `onestream.sql.protectedTables` | array | `[]` | Array of table names that can never be modified, even when safety is disabled. Use this to protect critical system tables. |
| `onestream.sql.requireWhereClause` | boolean | `true` | Require a WHERE clause on UPDATE and DELETE statements. Prevents accidental full-table modifications. |
| `onestream.sql.confirmDestructive` | boolean | `true` | Show a confirmation dialog before executing any destructive SQL statement (UPDATE, DELETE, DROP, etc.) |

## Error Log and Task Activity Settings

Settings that control the Error Log Viewer and Task Activity Viewer.

| Setting | Type | Default | Valid Values | Description |
|---|---|---|---|---|
| `onestream.errorLog.recordLimit` | number | `10` | 1--100 | Maximum number of error log records to fetch per request |
| `onestream.taskActivity.recordLimit` | number | `25` | 1--1000 | Maximum number of task activity records to fetch per request |
| `onestream.taskActivity.autoRefresh` | boolean | `false` | -- | Automatically refresh the task activity list on an interval |
| `onestream.taskActivity.refreshInterval` | number | `30` | 5--300 | Seconds between auto-refresh polls when auto-refresh is enabled |

## Dimension and Metadata Viewer Settings

Settings that control the Dimension Metadata viewer's behavior and caching.

| Setting | Type | Default | Valid Values | Description |
|---|---|---|---|---|
| `onestream.dimension.expandLevel` | number | `2` | 1--10 | Number of hierarchy levels to auto-expand when opening a dimension. Higher values load more data upfront. |
| `onestream.dimension.cacheTimeout` | number | `300` | 60--3600 | Seconds before cached dimension data expires and is re-fetched from the server |

## C# IntelliSense Settings

Settings that control the C# IntelliSense feature for OneStream API autocomplete.

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.intellisense.enabled` | boolean | `true` | Enable or disable C# IntelliSense for OneStream types |
| `onestream.intellisense.dllPath` | string | `""` | Path to a folder containing OneStream DLL files for full type resolution (Windows recommended) |
| `onestream.intellisense.autoSetup` | boolean | `true` | Automatically configure IntelliSense when connecting to a new environment |
| `onestream.intellisense.omnisharpRestart` | boolean | `true` | Restart the OmniSharp language server after IntelliSense configuration changes |
| `onestream.intellisense.additionalReferences` | array | `[]` | Array of paths to additional .NET assembly files to include in IntelliSense |
| `onestream.intellisense.version` | string | `""` | Override the auto-detected OneStream version for IntelliSense (e.g., `"8.0"`, `"9.0"`) |
| `onestream.intellisense.versionPaths` | object | `{}` | Map of OneStream version strings to DLL folder paths for environments with multiple versions |
| `onestream.intellisense.localOnly` | boolean | `false` | Use only local type data and DLLs without making any server requests for IntelliSense |

## CodeMCP Server Settings

Settings that control the embedded MCP server for AI assistant integration.

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.codemcp.enabled` | boolean | `true` | Enable or disable the MCP server |
| `onestream.codemcp.autoExportConnection` | boolean | `false` | Automatically export the MCP connection configuration file whenever you connect to a OneStream environment |
| `onestream.codemcp.serverPort` | number | `0` | Port number for the MCP server. Use `0` for automatic port assignment. |
