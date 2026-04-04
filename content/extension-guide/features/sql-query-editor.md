# SQL Query Editor

The SQL Query Editor lets you execute SQL queries directly against OneStream databases from within VS Code. It includes schema IntelliSense, multiple connection support, query history, CSV export, and configurable safety checks.

## Opening the Editor

There are three ways to open the SQL Query Editor:

| Method | Shortcut | Behavior |
|---|---|---|
| Open SQL Query Editor | `Ctrl+Shift+Q` | Opens in the default layout (configured via settings) |
| Open SQL (Beside) | `Ctrl+Shift+Alt+Q` | Opens beside the current editor |
| Quick SQL Query | `Ctrl+K Ctrl+Q` | Opens a quick input box for a one-off query |

You can also open the editor from the Command Palette by searching for **OneStream: Open SQL Query Editor**.

## Editor Layout

The SQL Query Editor is a WebView panel with the following areas:

- **Query Input**: A text area where you write your SQL. Supports multi-line queries.
- **Schema Sidebar**: A collapsible panel on the left showing database tables, columns, and stored procedures. Click any item to insert it into the query.
- **Results Table**: Displays query results in a scrollable, sortable table below the query input.
- **Toolbar**: Contains buttons for Execute, Export CSV, Clear, and connection selection.

## Database Connections

The editor supports three connection types:

### Framework Database

The OneStream framework database contains system-level tables such as user accounts, security settings, and server configuration. This connection is available automatically when you are connected to a OneStream environment.

### Application Database

The application database contains data specific to the currently selected OneStream application, including cube data, dimensions, members, and business rule metadata. Switch applications using the dropdown in the toolbar.

### External Connections

If your OneStream environment has configured external database connections, they will also appear in the connection dropdown. These let you query external systems (data warehouses, staging databases, etc.) directly from the editor.

## Schema IntelliSense

The Schema IntelliSense feature provides autocomplete suggestions as you type SQL queries.

### What It Provides

- **Table names**: All tables in the selected database, filtered as you type.
- **Column names**: When you type a table name followed by a dot, column names for that table appear.
- **Stored procedures**: Available stored procedures are listed with their parameter signatures.
- **Aliases**: If you define a table alias (e.g., `FROM Users u`), the editor recognizes `u.` as referring to the Users table.

### Schema Loading

When you first open the editor or switch connections, the schema is loaded from the database. This can take a few seconds for large databases. A loading indicator appears in the schema sidebar during this process.

The schema load timeout is configurable (default: 120 seconds). If your database has a very large number of tables, you may need to increase this value.

### Schema Sidebar

The schema sidebar displays all tables and their columns in a tree view. You can:

- **Expand/collapse** tables to see their columns.
- **Click** a table or column name to insert it at the cursor position in the query.
- **Search** using the filter box at the top to find specific tables or columns.
- **Collapse the sidebar** to gain more space for the query editor. The sidebar state persists across sessions.

## Executing Queries

Type your SQL query and press the **Execute** button in the toolbar, or use the keyboard shortcut associated with the editor.

### Query Behavior

- Only one query executes at a time. If a query is running, the Execute button is disabled.
- Results appear in the table below the query input.
- Row count and execution time are displayed in the status area.
- If the query returns no results (for example, an UPDATE or INSERT when allowed), a confirmation message appears instead of a results table.

### Query Timeout

Queries have a configurable execution timeout (default: 120 seconds). If a query exceeds this timeout, it is cancelled and an error message is displayed. You can adjust this in settings.

## Query History

Every query you execute is saved to the query history. Access it by clicking the **History** button in the toolbar.

### History Features

- **Search**: Filter history entries by query text.
- **Re-run**: Click any history entry to load it into the query input. Click Execute to run it again.
- **Timestamps**: Each entry shows when it was executed.
- **Connection context**: History entries record which database connection was used.

History is stored locally and persists across VS Code sessions.

## CSV Export

After executing a query, click the **Export CSV** button to save the results as a CSV file. A file save dialog appears letting you choose the destination. The export includes all columns and rows from the result set.

## Safety Checks

The SQL Query Editor includes a safety system to prevent accidental data modification or destructive operations.

### Default Behavior

By default, safety checks are **enabled** with the following rules:

- **SELECT only**: Only `SELECT` statements are allowed. `INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `TRUNCATE`, and other modifying statements are blocked.
- **No PRINT statements**: `PRINT` statements are stripped or blocked because they can interfere with result handling.
- **No GO statements**: Batch separators (`GO`) are not supported. Write single queries.

### Configurable Safety

When safety is enabled but you need to run modifying queries, you can configure exceptions:

| Setting | Description |
|---|---|
| `onestream.sql.safetyEnabled` | Master toggle for all safety checks (default: `true`) |
| `onestream.sql.allowedTablePrefixes` | Array of table name prefixes that can be modified (e.g., `["Staging_", "Temp_"]`) |
| `onestream.sql.allowedTables` | Array of specific table names that can be modified |
| `onestream.sql.protectedTables` | Array of table names that can never be modified, even if safety is off |
| `onestream.sql.requireWhereClause` | Require a WHERE clause on UPDATE and DELETE statements (default: `true`) |
| `onestream.sql.confirmDestructive` | Show a confirmation dialog before executing destructive statements (default: `true`) |

### Protected Tables

Even when safety checks are relaxed, protected tables are always blocked from modification. By default, this includes critical system tables. You can add additional tables to the protected list.

## Layout Options

The SQL Query Editor can open in different positions relative to your current editor layout:

| Layout | Setting Value | Behavior |
|---|---|---|
| Current | `current` | Replaces the current editor tab |
| Beside | `beside` | Opens in a new editor column to the right |
| Below | `below` | Opens in a panel below the current editor |
| Active | `active` | Opens in the currently active editor group |

Set the default layout with `onestream.sql.defaultLayout`.

## Compact Mode

On smaller screens or laptops, enable **Compact Mode** to reduce padding and font sizes in the SQL Query Editor interface. This provides more room for the query input and results table.

Enable it with `onestream.sql.compactMode`.

## Settings Reference

All SQL Query Editor settings:

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.sql.defaultLayout` | string | `current` | Where the editor opens: `current`, `beside`, `below`, or `active` |
| `onestream.sql.schemaLoadTimeout` | number | `120` | Seconds to wait for schema loading (range: 30--600) |
| `onestream.sql.queryExecutionTimeout` | number | `120` | Seconds to wait for query execution (range: 30--600) |
| `onestream.sql.schemaSidebarCollapsed` | boolean | `false` | Whether the schema sidebar starts collapsed |
| `onestream.sql.schemaSidebarWidth` | number | `280` | Width of the schema sidebar in pixels (range: 150--600) |
| `onestream.sql.compactMode` | boolean | `false` | Reduce UI padding for smaller screens |
| `onestream.sql.safetyEnabled` | boolean | `true` | Enable SQL safety checks |
| `onestream.sql.allowedTablePrefixes` | array | `[]` | Table name prefixes allowed for modification |
| `onestream.sql.allowedTables` | array | `[]` | Specific table names allowed for modification |
| `onestream.sql.protectedTables` | array | `[]` | Table names that can never be modified |
| `onestream.sql.requireWhereClause` | boolean | `true` | Require WHERE on UPDATE/DELETE |
| `onestream.sql.confirmDestructive` | boolean | `true` | Confirm before destructive operations |
