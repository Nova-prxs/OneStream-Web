# Repository Tools

Repository tools let the AI assistant read, search, and manage business rules, assemblies, and workspaces in the OneStream environment.

## list_workspaces

Lists all workspaces in the connected OneStream environment.

### Parameters

This tool takes no parameters.

### Return Value

Returns an array of workspace objects:

```json
[
  {
    "name": "Default",
    "description": "Default workspace",
    "ruleCount": 45,
    "assemblyCount": 3
  }
]
```

---

## load_workspace

Loads a specific workspace, making its rules and assemblies available for other operations.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `workspace` | string | Yes | -- | Name of the workspace to load |

### Return Value

Returns workspace details including lists of rules and assemblies:

```json
{
  "name": "Default",
  "rules": ["RevenueCalc", "CostAllocation", "DataLoad"],
  "assemblies": ["CoreLibrary", "IntegrationUtils"],
  "loaded": true
}
```

---

## unload_workspace

Unloads a previously loaded workspace to free resources.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `workspace` | string | Yes | -- | Name of the workspace to unload |

### Return Value

```json
{
  "name": "Default",
  "unloaded": true
}
```

---

## list_rule_types

Lists all business rule types available in the environment.

### Parameters

This tool takes no parameters.

### Return Value

Returns an array of rule type objects:

```json
[
  {"type": "Finance", "count": 25},
  {"type": "Extender", "count": 12},
  {"type": "Connector", "count": 5},
  {"type": "Parser", "count": 3}
]
```

---

## read_business_rule

Reads the source code of a specific business rule.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `ruleName` | string | Yes | -- | Name of the business rule to read |
| `ruleType` | string | No | -- | Type of the rule (Finance, Extender, Connector, Parser). If omitted, searches all types. |

### Return Value

Returns the rule's source code and metadata:

```json
{
  "name": "RevenueCalc",
  "type": "Finance",
  "encrypted": false,
  "sourceCode": "// Revenue calculation logic\npublic void Main(...) { ... }",
  "lastModified": "2024-03-10T14:30:00Z",
  "modifiedBy": "admin"
}
```

### Usage Example

```
Show me the source code of the RevenueCalc finance rule.
```

---

## search_code

Searches across all business rule source code for a text pattern.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | The search text or pattern to find in rule source code |
| `ruleType` | string | No | -- | Filter results to a specific rule type |
| `caseSensitive` | boolean | No | `false` | Whether the search is case-sensitive |
| `maxResults` | number | No | `50` | Maximum number of results to return |

### Return Value

Returns an array of search matches:

```json
[
  {
    "ruleName": "RevenueCalc",
    "ruleType": "Finance",
    "lineNumber": 42,
    "lineContent": "decimal revenue = api.Finance.GetCellValue(...);",
    "context": "... surrounding lines ..."
  }
]
```

### Usage Example

```
Search all business rules for references to api.Finance.GetCellValue.
```

---

## get_repository_stats

Returns statistics about the repository including counts of rules, assemblies, and other artifacts.

### Parameters

This tool takes no parameters.

### Return Value

```json
{
  "totalRules": 45,
  "rulesByType": {
    "Finance": 25,
    "Extender": 12,
    "Connector": 5,
    "Parser": 3
  },
  "totalAssemblies": 3,
  "totalWorkspaces": 2,
  "encryptedRules": 5
}
```

---

## extract_workspace

Extracts all rules and assemblies from a workspace to the local filesystem.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `workspace` | string | Yes | -- | Name of the workspace to extract |
| `outputPath` | string | No | -- | Local directory path for the extracted files. Defaults to the current workspace folder. |

### Return Value

```json
{
  "workspace": "Default",
  "extractedRules": 45,
  "extractedAssemblies": 3,
  "outputPath": "/path/to/workspace"
}
```

---

## search_components

Searches for components (rules, assemblies, sequences) by name or description.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Search text to match against component names and descriptions |
| `componentType` | string | No | -- | Filter to a specific component type (rule, assembly, sequence) |
| `maxResults` | number | No | `25` | Maximum number of results |

### Return Value

```json
[
  {
    "name": "RevenueCalc",
    "type": "rule",
    "subType": "Finance",
    "description": "Calculates revenue based on entity hierarchy",
    "lastModified": "2024-03-10T14:30:00Z"
  }
]
```

---

## get_component_detail

Gets detailed information about a specific component.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `name` | string | Yes | -- | Name of the component |
| `componentType` | string | Yes | -- | Type of component (rule, assembly, sequence) |

### Return Value

Returns detailed component metadata including dependencies, last modified information, and configuration details specific to the component type.

---

## get_cross_references

Finds all references to a specific rule, assembly, or component across the environment.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `name` | string | Yes | -- | Name of the component to find references for |
| `componentType` | string | No | -- | Type of the component being referenced |

### Return Value

```json
{
  "component": "CoreLibrary",
  "referencedBy": [
    {"name": "RevenueCalc", "type": "Finance", "context": "using CoreLibrary;"},
    {"name": "CostAllocation", "type": "Finance", "context": "CoreLibrary.Calculate(...)"}
  ],
  "totalReferences": 2
}
```

### Usage Example

```
What rules reference the CoreLibrary assembly?
```

---

## get_file_info

Returns metadata about a specific file in the workspace.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `path` | string | Yes | -- | Path to the file (relative to workspace root) |

### Return Value

```json
{
  "path": "Finance/RevenueCalc.cs",
  "exists": true,
  "size": 4096,
  "lastModified": "2024-03-10T14:30:00Z",
  "syncStatus": "modified",
  "serverVersion": "2024-03-09T10:00:00Z"
}
```
