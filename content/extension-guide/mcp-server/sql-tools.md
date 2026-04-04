# SQL Tools

SQL tools let the AI assistant execute queries against OneStream databases, export table data, and search for SQL scripts within the project.

## execute_sql

Executes a SQL query against a OneStream database and returns the results.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | The SQL query to execute |
| `database` | string | No | `"framework"` | Target database: `framework`, `application`, or a named external connection |
| `maxRows` | number | No | `1000` | Maximum number of rows to return |
| `timeout` | number | No | `120` | Query timeout in seconds |

### Return Value

Returns the query results as a structured object:

```json
{
  "columns": [
    {"name": "UserId", "type": "int"},
    {"name": "UserName", "type": "nvarchar"},
    {"name": "LastLogin", "type": "datetime"}
  ],
  "rows": [
    [1, "admin", "2024-03-15T10:30:00Z"],
    [2, "jsmith", "2024-03-14T08:00:00Z"]
  ],
  "rowCount": 2,
  "executionTimeMs": 45,
  "truncated": false
}
```

If the result set exceeds `maxRows`, the `truncated` field is `true` and only the first `maxRows` rows are returned.

### Safety

This tool respects the same SQL safety settings as the SQL Query Editor:

- If `onestream.sql.safetyEnabled` is `true`, only `SELECT` statements are allowed.
- Protected tables cannot be queried with modifying statements regardless of safety settings.
- The `requireWhereClause` setting applies to any `UPDATE` or `DELETE` statements (when safety is relaxed).

### Usage Example

```
Query the framework database for all users who logged in this week.
```

---

## download_table_data

Downloads all data from a specific table.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `tableName` | string | Yes | -- | Name of the table to download |
| `database` | string | No | `"framework"` | Target database: `framework`, `application`, or a named external connection |
| `whereClause` | string | No | -- | Optional WHERE clause to filter rows (without the WHERE keyword) |
| `maxRows` | number | No | `10000` | Maximum number of rows to download |

### Return Value

Returns the full table data:

```json
{
  "tableName": "Users",
  "columns": [
    {"name": "UserId", "type": "int"},
    {"name": "UserName", "type": "nvarchar"},
    {"name": "Email", "type": "nvarchar"}
  ],
  "rows": [
    [1, "admin", "admin@company.com"],
    [2, "jsmith", "jsmith@company.com"]
  ],
  "rowCount": 2,
  "totalRowsInTable": 2
}
```

### Usage Example

```
Download all rows from the ApplicationParameters table where Category is 'Finance'.
```

---

## export_tables_to_csv

Exports one or more tables to CSV files on the local filesystem.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `tables` | array | Yes | -- | Array of table names to export |
| `database` | string | No | `"framework"` | Target database |
| `outputDirectory` | string | No | -- | Directory to write CSV files to. Defaults to a `sql-exports/` folder in the workspace. |
| `includeHeaders` | boolean | No | `true` | Include column headers as the first row |

### Return Value

```json
{
  "exported": [
    {"table": "Users", "rows": 150, "file": "/path/to/sql-exports/Users.csv"},
    {"table": "Roles", "rows": 25, "file": "/path/to/sql-exports/Roles.csv"}
  ],
  "totalFiles": 2,
  "totalRows": 175
}
```

### Usage Example

```
Export the Users and Roles tables from the framework database to CSV.
```

---

## search_sql_scripts

Searches for SQL scripts and queries within the workspace files and query history.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Search text to find in SQL scripts and query history |
| `searchIn` | string | No | `"all"` | Where to search: `files` (workspace .sql files), `history` (query history), or `all` |
| `maxResults` | number | No | `25` | Maximum number of results to return |

### Return Value

```json
[
  {
    "source": "file",
    "path": "queries/monthly-report.sql",
    "lineNumber": 5,
    "content": "SELECT e.EntityName, SUM(d.Amount) as Total FROM EntityData d ...",
    "context": "-- Monthly revenue report by entity"
  },
  {
    "source": "history",
    "executedAt": "2024-03-15T10:30:00Z",
    "database": "application",
    "content": "SELECT * FROM CubeData WHERE Scenario = 'Actual'"
  }
]
```

### Usage Example

```
Find SQL queries that reference the CubeData table.
```

---

## validate_api

Validates that a SQL query is syntactically correct and safe to execute without actually running it.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | The SQL query to validate |
| `database` | string | No | `"framework"` | Target database for context-specific validation |

### Return Value

```json
{
  "valid": true,
  "queryType": "SELECT",
  "tables": ["Users", "Roles"],
  "estimatedComplexity": "low",
  "safetyCheck": {
    "passed": true,
    "warnings": []
  }
}
```

If the query has issues:

```json
{
  "valid": false,
  "errors": [
    {"message": "Table 'NonExistent' does not exist", "position": 22}
  ],
  "safetyCheck": {
    "passed": false,
    "warnings": ["DELETE statement blocked by safety settings"]
  }
}
```

### Usage Example

```
Validate this query before running it: SELECT * FROM Users WHERE Active = 1
```
