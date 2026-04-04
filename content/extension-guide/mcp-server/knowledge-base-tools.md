# Knowledge Base Tools

Knowledge base tools give the AI assistant access to OneStream documentation, API references, enumerations, and keyword definitions. These tools enable the assistant to provide accurate answers about OneStream concepts and APIs.

## search_doc_books

Searches OneStream documentation books by title or description.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Search text to match against book titles and descriptions |
| `maxResults` | number | No | `10` | Maximum number of results to return |

### Return Value

```json
[
  {
    "title": "OneStream Platform Guide",
    "bookId": "platform-guide",
    "description": "Comprehensive guide to the OneStream platform",
    "chapterCount": 24,
    "lastUpdated": "2024-02-01"
  }
]
```

### Usage Example

```
Find OneStream documentation about data integration.
```

---

## read_doc_chapter

Reads the full content of a documentation chapter.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `bookId` | string | Yes | -- | The book identifier |
| `chapterId` | string | Yes | -- | The chapter identifier within the book |

### Return Value

Returns the chapter content in Markdown format:

```json
{
  "bookTitle": "OneStream Platform Guide",
  "chapterTitle": "Data Integration Overview",
  "content": "# Data Integration Overview\n\nOneStream provides several methods for integrating data...",
  "previousChapter": {"id": "ch-03", "title": "Cube Design"},
  "nextChapter": {"id": "ch-05", "title": "Business Rules"}
}
```

---

## browse_documentation

Provides a structured overview of available documentation, organized by topic.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `topic` | string | No | -- | Filter to a specific topic area (e.g., "finance", "integration", "security") |

### Return Value

```json
{
  "topics": [
    {
      "name": "Platform",
      "books": [
        {"bookId": "platform-guide", "title": "OneStream Platform Guide", "chapterCount": 24}
      ]
    },
    {
      "name": "Finance",
      "books": [
        {"bookId": "finance-rules", "title": "Finance Rules Reference", "chapterCount": 12}
      ]
    }
  ]
}
```

---

## search_api_methods

Searches the OneStream API for methods by name, class, or description.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Search text to match against method names, class names, and descriptions |
| `className` | string | No | -- | Filter to methods of a specific class (e.g., `BRApi`, `FinanceApi`) |
| `maxResults` | number | No | `25` | Maximum number of results to return |

### Return Value

```json
[
  {
    "className": "BRApi.Finance",
    "methodName": "GetCellValue",
    "signature": "decimal GetCellValue(string cubeName, string memberFilter)",
    "description": "Retrieves the value of a specific cell in a cube.",
    "parameters": [
      {"name": "cubeName", "type": "string", "description": "Name of the cube"},
      {"name": "memberFilter", "type": "string", "description": "Member filter expression"}
    ],
    "returnType": "decimal",
    "version": "8.0+"
  }
]
```

### Usage Example

```
How do I use the GetCellValue method in OneStream?
```

---

## get_api_details

Returns detailed documentation for a specific API method or class.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `className` | string | Yes | -- | The class name (e.g., `BRApi`, `BRApi.Finance`, `SessionInfo`) |
| `methodName` | string | No | -- | Specific method name. If omitted, returns all methods for the class. |

### Return Value

```json
{
  "className": "BRApi.Finance",
  "description": "Financial operations API providing access to cube data, calculations, and financial rules.",
  "methods": [
    {
      "name": "GetCellValue",
      "signature": "decimal GetCellValue(string cubeName, string memberFilter)",
      "description": "Retrieves the value of a specific cell in a cube based on a member filter expression.",
      "parameters": [
        {"name": "cubeName", "type": "string", "required": true, "description": "Name of the cube to query"},
        {"name": "memberFilter", "type": "string", "required": true, "description": "Member filter expression (e.g., 'E#Corporate.S#Actual.T#2024M1')"}
      ],
      "returnType": "decimal",
      "version": "8.0+",
      "example": "decimal value = api.Finance.GetCellValue(\"Finance\", \"E#Corporate.S#Actual.T#2024M1.A#Revenue\");"
    }
  ]
}
```

---

## lookup_enum_definitions

Looks up OneStream enumeration definitions by name.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `enumName` | string | Yes | -- | Name of the enumeration to look up (e.g., `MemberType`, `AccountType`, `DataStorageType`) |

### Return Value

```json
{
  "name": "AccountType",
  "description": "Defines the type of an account member for financial reporting behavior.",
  "values": [
    {"name": "Revenue", "value": 0, "description": "Revenue account - credit positive"},
    {"name": "Expense", "value": 1, "description": "Expense account - debit positive"},
    {"name": "Asset", "value": 2, "description": "Asset account - debit positive"},
    {"name": "Liability", "value": 3, "description": "Liability account - credit positive"},
    {"name": "Equity", "value": 4, "description": "Equity account - credit positive"},
    {"name": "Statistical", "value": 5, "description": "Statistical account - no debit/credit behavior"}
  ]
}
```

### Usage Example

```
What are the possible values for the AccountType enum in OneStream?
```

---

## list_keywords

Lists OneStream-specific keywords and their definitions.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `category` | string | No | -- | Filter to a specific category of keywords (e.g., "finance", "dimension", "workflow") |
| `query` | string | No | -- | Search text to filter keywords |

### Return Value

```json
[
  {
    "keyword": "BRApi",
    "category": "API",
    "definition": "The primary API object for business rule development. Provides access to all OneStream operations through sub-objects (Finance, Data, Security, etc.).",
    "aliases": ["api"],
    "seeAlso": ["SessionInfo", "si"]
  },
  {
    "keyword": "SessionInfo",
    "category": "API",
    "definition": "Contains information about the current user session including user name, application, scenario, and time context.",
    "aliases": ["si"],
    "seeAlso": ["BRApi", "api"]
  }
]
```

---

## list_kb_rule_types

Lists all knowledge base rule types with descriptions of their purpose and usage patterns.

### Parameters

This tool takes no parameters.

### Return Value

```json
[
  {
    "type": "Finance",
    "description": "Rules that execute during financial operations (consolidation, translation, calculation).",
    "subTypes": ["Consolidation", "Translation", "Calculation", "Input"],
    "commonPatterns": ["Cell calculation", "Member filtering", "Cross-entity data access"]
  },
  {
    "type": "Extender",
    "description": "General-purpose rules triggered by dashboards, events, or scheduled tasks.",
    "subTypes": ["Dashboard DataSet", "Dashboard Extender", "Event Handler"],
    "commonPatterns": ["Data retrieval for dashboards", "Custom validation", "Workflow automation"]
  }
]
```

---

## semantic_search

Performs a semantic (meaning-based) search across all OneStream documentation, API references, and knowledge base content.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Natural language question or description of what you are looking for |
| `sources` | array | No | `["docs", "api", "kb"]` | Which sources to search: `docs` (documentation), `api` (API reference), `kb` (knowledge base) |
| `maxResults` | number | No | `10` | Maximum number of results to return |

### Return Value

```json
[
  {
    "source": "api",
    "title": "BRApi.Finance.GetCellValue",
    "content": "Retrieves the value of a specific cell in a cube...",
    "relevanceScore": 0.95,
    "url": "api://BRApi.Finance/GetCellValue"
  },
  {
    "source": "docs",
    "title": "Working with Cube Data",
    "content": "To retrieve data from a cube, use the Finance API...",
    "relevanceScore": 0.87,
    "url": "docs://platform-guide/ch-07"
  }
]
```

### Usage Example

```
How do I read cube data for a specific entity and time period in OneStream?
```

---

## search_repo_markdown

Searches Markdown documentation files within the workspace repository.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `query` | string | Yes | -- | Search text to find in Markdown files |
| `path` | string | No | -- | Restrict search to a specific directory path |
| `maxResults` | number | No | `25` | Maximum number of results |

### Return Value

```json
[
  {
    "filePath": "docs/architecture.md",
    "lineNumber": 42,
    "content": "The data integration pipeline uses Connector rules to...",
    "context": "## Data Integration\n\nThe data integration pipeline uses Connector rules to..."
  }
]
```

---

## list_modules

Lists available OneStream modules and their components.

### Parameters

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| `category` | string | No | -- | Filter to a specific module category |

### Return Value

```json
[
  {
    "name": "Financial Close",
    "category": "Finance",
    "description": "Manages the financial close process including consolidation and reporting.",
    "components": ["Consolidation Engine", "Translation Engine", "Intercompany Matching"]
  }
]
```

---

## refresh_index

Refreshes the knowledge base search index to include any recent documentation updates.

### Parameters

This tool takes no parameters.

### Return Value

```json
{
  "refreshed": true,
  "documentsIndexed": 156,
  "apiMethodsIndexed": 340,
  "enumsIndexed": 45,
  "durationMs": 2300
}
```
