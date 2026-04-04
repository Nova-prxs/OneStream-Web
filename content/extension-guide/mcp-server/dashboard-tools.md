# Dashboard Tools

Dashboard tools let the AI assistant browse and inspect OneStream dashboard structures, components, and parameters.

## get_dashboard_tree

Returns the hierarchical tree structure of dashboards in the application.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `application` | string | No | -- | Application name. Uses the current connected application if omitted. |
| `depth` | number | No | `3` | Maximum depth of the tree to return (1 = top level only) |

### Return Value

Returns a tree structure of dashboard groups and dashboards:

```json
{
  "application": "GolfStream",
  "tree": [
    {
      "name": "Financial Reporting",
      "type": "group",
      "children": [
        {
          "name": "Income Statement",
          "type": "dashboard",
          "id": "dash-001",
          "description": "Monthly income statement with drill-down",
          "children": [
            {
              "name": "Revenue Detail",
              "type": "component",
              "componentType": "DataGrid"
            }
          ]
        }
      ]
    }
  ]
}
```

### Usage Example

```
Show me the dashboard structure for the Financial Reporting group.
```

---

## search_dashboards

Searches dashboards by name or description.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Search text to match against dashboard names and descriptions |
| `maxResults` | number | No | `25` | Maximum number of results to return |

### Return Value

Returns an array of matching dashboards:

```json
[
  {
    "name": "Income Statement",
    "id": "dash-001",
    "group": "Financial Reporting",
    "description": "Monthly income statement with drill-down",
    "componentCount": 5,
    "parameterCount": 3
  }
]
```

### Usage Example

```
Find all dashboards related to budgeting.
```

---

## get_dashboard_detail

Returns detailed information about a specific dashboard, including all its components, layout, and configuration.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `dashboardName` | string | Yes | -- | Name of the dashboard to inspect |

### Return Value

Returns the full dashboard definition:

```json
{
  "name": "Income Statement",
  "id": "dash-001",
  "group": "Financial Reporting",
  "description": "Monthly income statement with drill-down",
  "components": [
    {
      "name": "RevenueGrid",
      "type": "DataGrid",
      "position": {"row": 0, "col": 0, "rowSpan": 1, "colSpan": 2},
      "dataSource": "RevenueQuery",
      "parameters": ["Scenario", "Entity", "Time"]
    },
    {
      "name": "ExpenseChart",
      "type": "Chart",
      "position": {"row": 1, "col": 0, "rowSpan": 1, "colSpan": 1},
      "chartType": "Bar",
      "dataSource": "ExpenseQuery"
    }
  ],
  "parameters": [
    {"name": "Scenario", "type": "Member", "dimension": "Scenario", "default": "Actual"},
    {"name": "Entity", "type": "Member", "dimension": "Entity", "default": "Corporate"},
    {"name": "Time", "type": "Member", "dimension": "Time", "default": "Current"}
  ]
}
```

### Usage Example

```
Show me the full configuration of the Income Statement dashboard.
```

---

## get_parameter_detail

Returns detailed information about a specific dashboard parameter.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `parameterName` | string | Yes | -- | Name of the parameter to inspect |
| `dashboardName` | string | No | -- | Scope the search to a specific dashboard |

### Return Value

```json
{
  "name": "Scenario",
  "type": "Member",
  "dimension": "Scenario",
  "defaultValue": "Actual",
  "allowedValues": ["Actual", "Budget", "Forecast"],
  "required": true,
  "description": "Select the scenario to report on",
  "usedByDashboards": ["Income Statement", "Balance Sheet", "Cash Flow"]
}
```

### Usage Example

```
What are the details of the Entity parameter, and which dashboards use it?
```

---

## search_parameters

Searches across all dashboard parameters by name, dimension, or type.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Search text to match against parameter names and descriptions |
| `dimension` | string | No | -- | Filter to parameters for a specific dimension |
| `maxResults` | number | No | `25` | Maximum number of results |

### Return Value

```json
[
  {
    "name": "Entity",
    "dimension": "Entity",
    "type": "Member",
    "dashboardCount": 12,
    "dashboards": ["Income Statement", "Balance Sheet", "Cash Flow"]
  }
]
```

### Usage Example

```
Find all dashboard parameters that reference the Entity dimension.
```

---

## trace_parameter_flow

Traces how a parameter value flows through a dashboard -- from user selection through components to data queries.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `parameterName` | string | Yes | -- | Name of the parameter to trace |
| `dashboardName` | string | Yes | -- | Dashboard to trace the parameter through |

### Return Value

```json
{
  "parameter": "Scenario",
  "dashboard": "Income Statement",
  "flow": [
    {"step": "User Selection", "component": "ScenarioDropdown", "action": "User selects value"},
    {"step": "Parameter Binding", "component": "RevenueGrid", "binding": "Scenario filter"},
    {"step": "Data Query", "component": "RevenueGrid", "query": "WHERE Scenario = @Scenario"},
    {"step": "Parameter Binding", "component": "ExpenseChart", "binding": "Scenario filter"}
  ]
}
```

### Usage Example

```
Trace how the Scenario parameter flows through the Income Statement dashboard.
```
