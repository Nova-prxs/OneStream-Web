# OneStream Data Management Best Practices

## Overview
Data Management in OneStream handles data integration from various sources including flat files, databases, Excel templates, forms, and journals.

## Naming Conventions

### Consistent Naming Standards
**Critical for maintainability and reusability**

| Item | Renameable | Naming Example |
|------|-----------|----------------|
| Data Sources | ✅ Yes | Entity (Michigan), Workflow (Cleveland_Plant), GL (SAP, Sage) |
| Transformation Dimensions | ✅ Yes | Global_Scenario, A_Sage, A_Cleveland |
| Transformation Rule Profiles | ✅ Yes | Entity (Michigan), GL (SAP, Sage, etc.) |
| Parser Business Rules | ⚠️ Locked | XFR_Sage_Account, XFR_Cleveland_Product |
| Form Template Groups | ✅ Yes | Data_Entry |
| Form Template Profiles | ✅ Yes | DE_GL, DE_Planning |
| Forms | ✅ Yes (v8.0+) | A_AR, B_PPE, C_LTDebt |
| Journal Template Groups | ✅ Yes | JE_Elimination |
| Journal Template Profiles | ✅ Yes | JE_EntityA |
| Journals | ✅ Yes (v8.0+) | JE_IntercoElim |

### Naming Convention Best Practices
- **Use Prefixes/Suffixes**: Categorize by type (XFR_ for transformations, JE_ for journals)
- **Be Descriptive**: Names should clearly indicate purpose
- **Plan for Reusability**: Global items should have "Global_" prefix
- **Consistent Case**: Choose camelCase or snake_case and stick to it

## Data Source Types

### 1. Fixed Files
- Typical system reports with headers and spaced data
- Easy to read visually
- Use business rules for complex fixed files

**Best Practices:**
- Validate header format before processing
- Document expected positions for each field
- Handle variable spacing gracefully

### 2. Delimited Files
- Data separated by delimiter (comma, tab, pipe, etc.)
- Watch for delimiters in data fields

**Common Issues:**
```
Company Name: "New Company, Inc."  ← Comma in data!
Amount: "1,234.56"  ← Comma in number
```

**Solutions:**
- Use text qualifiers (double quotes)
- Choose delimiter not used in data
- OneStream handles quoted fields automatically

### 3. Database Connectors
- ODBC connections with SQL queries
- XML files
- Web Service APIs
- Requires business rule scripts

**Example Pattern:**
```vbnet
' Database connector business rule
Public Function CustomDatabaseReader(ByVal si As SessionInfo, _
    ByVal globals As BRGlobals, _
    ByVal api As Object, _
    ByVal args As DataSourceEventArgs) As DataSourceExecutionResult
    
    Dim result As New DataSourceExecutionResult()
    
    Try
        ' Get connection string from parameters
        Dim connString As String = args.DataSource.ConnectionString
        
        ' Execute query with substitution variables
        Dim query As String = args.DataSource.Query
        query = query.Replace("{@EntityName}", args.DataUnit.EntityName)
        query = query.Replace("{@Period}", args.DataUnit.TimeName)
        
        ' Process results
        ' ... (query execution code)
        
        result.Status = DataSourceReturnStatus.Success
    Catch ex As Exception
        result.Status = DataSourceReturnStatus.Failed
        result.ErrorMessage = ex.Message
    End Try
    
    Return result
End Function
```

### 4. Excel Templates
- Familiar to all users
- Header tags with range names
- Multiple tables per file supported

**Range Name Types:**
- **XFD**: Data load to Stage
- **XFF**: Form data
- **XFJ**: Journal data
- **XFC**: Cell details

**Best Practices:**
- Set "Allow Dynamic Excel Loads = True" in data source
- Standardize template layout across organization
- Include data validation in Excel template
- Document template structure for users

**Example Excel Template Structure:**
```
Row 1: Tags (S#Actual, T#2024M1, E#Entity1, A#Sales, ...)
Row 2+: Data values
Range Name: XFD_SalesData
```

### 5. Forms
- Manual data input
- Use for data not in source systems
- Common uses: headcount, forecasts, allocations

**Best Practices:**
- Design intuitive layouts
- Use dropdown lists for dimensions
- Add calculation helpers
- Implement data validation rules
- Link to workflow for approvals

### 6. Journals (Adjustments)
- For adjustments without source system
- Consolidation entries
- Reclassifications and eliminations

**Best Practice Note:**
> "One best practice is to put all journal entries into the source system and reload, but in some cases, the ability to have journal entries in the system aids tasks like consolidating entries that have no ledger home."

## Workflow Data Protection and Layering

### Origin Dimension
Creates data separation and protection layers:

```
Origin Dimension Layers:
├── Import (source data from GL systems)
│   ├── Protected by Source ID
│   └── Protected by Workflow Channel
├── Forms (manual entry)
└── Adjustments (journals)
```

### Source ID Strategy
- **Purpose**: Determines how to reload data
- **Common Pattern**: Use data file name as Source ID
- **Benefit**: Allows selective reload by source

```vbnet
' Set Source ID in transformation rule
If args.FileName.Contains("SAP") Then
    args.SourceID = "SAP_GL"
ElseIf args.FileName.Contains("Workday") Then
    args.SourceID = "Workday_HCM"
End If
```

### Workflow Channel
- Adds another layer of data protection
- Can use account-level dimension (e.g., Product)
- Allows data segregation within same entity

**Example Use Case:**
> "At one company, each plant produced more than one product and had different finance people for each product (and they planned separately)."

**Configuration:**
- Entity: Cleveland_Plant
- Channels: Product_A, Product_B, Product_C
- Each channel has its own workflow and data isolation

## Reusability Strategies

### Global Transformation Rules
Dimensions that can be reused across cubes:
- **Scenario**: Usually simple 1:1 mapping (Actual → Actual)
- **Time**: Standard time mapping
- **View**: Consistent view mappings

**Naming:**
- Prefix with "Global_" for reusable rules
- Example: Global_Scenario, Global_Time

### Dynamic Data Sources
Use workflow variables for reusability:

```sql
-- SQL query with substitution variables
SELECT Account, Amount, Department
FROM FinancialData
WHERE Entity = '{@EntityName}'
  AND Period = '{@Period}'
  AND Year = '{@Year}'
```

**Benefits:**
- Same data source works for multiple entities
- Reduce maintenance
- Consistent logic across organization

## Data Attributes

### Available Fields
- **13 Mappable Dimensions**: Standard POV dimensions
- **Label Dimensions**: Additional categorization
- **20 Attribute Dimensions**: Custom metadata
- **UD Fields**: User-defined fields for special purposes

**Use Cases:**
- Capture GL account codes for reconciliation
- Store original amounts before FX translation
- Track data quality scores
- Record data source timestamps

## Performance Optimization

### Batch Processing
- Load data in batches during off-peak hours
- Use workflow stages to control timing
- Implement parallel processing where possible

### Transformation Rules
- Keep transformation logic simple
- Use direct mappings when possible
- Cache lookup tables in business rules
- Profile slow transformation rules

### Data Parser Business Rules
```vbnet
' Efficient data parsing pattern
Public Function ParseDataEfficiently(ByVal si As SessionInfo, _
    ByVal globals As BRGlobals, _
    ByVal api As Object, _
    ByVal args As DataSourceEventArgs) As DataSourceExecutionResult
    
    ' Pre-compile regex patterns (reuse across rows)
    Static accountPattern As New System.Text.RegularExpressions.Regex("^\d{4}")
    
    ' Use StringBuilder for string concatenation
    Dim sb As New System.Text.StringBuilder()
    
    ' Batch API calls instead of row-by-row
    Dim dataBuffer As New List(Of DataSourceDataRow)()
    
    For Each line In args.FileLines
        ' Process line efficiently
        If accountPattern.IsMatch(line) Then
            ' Add to batch
            dataBuffer.Add(CreateDataRow(line))
            
            ' Flush batch at threshold
            If dataBuffer.Count >= 1000 Then
                api.Data.SetDataBuffer(dataBuffer)
                dataBuffer.Clear()
            End If
        End If
    Next
    
    ' Flush remaining
    If dataBuffer.Count > 0 Then
        api.Data.SetDataBuffer(dataBuffer)
    End If
    
    Return New DataSourceExecutionResult() With {
        .Status = DataSourceReturnStatus.Success
    }
End Function
```

## Testing and Validation

### Data Quality Checks
- Validate file format before processing
- Check for required fields
- Verify data types and ranges
- Compare row counts (source vs loaded)
- Reconcile totals

### Error Handling
```vbnet
Try
    ' Data processing logic
Catch ex As OneStream.BusinessRule.Parser.ParserException
    ' Handle parser-specific errors
    BRApi.ErrorLog.LogMessage(si, "Parser Error: " & ex.Message)
    Return CreateFailedResult("Invalid file format")
Catch ex As Exception
    ' Handle general errors
    BRApi.ErrorLog.LogMessage(si, "Data Load Error: " & ex.Message)
    Return CreateFailedResult(ex.Message)
End Try
```

## Anti-Patterns to Avoid

❌ **One Data Source Per Entity**: Creates maintenance nightmare
❌ **No Source ID**: Cannot reload selectively
❌ **Complex Parser Logic**: Hard to debug and maintain
❌ **No Data Validation**: Bad data enters system
❌ **Inconsistent Naming**: Difficult to find and reuse components
❌ **No Documentation**: Nobody knows what transformations do
❌ **Hard-Coded Values**: Cannot reuse across environments

## Quick Reference

### Data Load Flow
```
1. Source Data → 2. Data Source → 3. Parser/Connector →
4. Transformation Rules → 5. Workflow → 6. Staging →
7. Validation → 8. Certification → 9. Cube
```

### Common File Extensions
- `.txt`, `.dat`, `.csv`: Flat files
- `.xlsx`, `.xlsm`: Excel templates
- `.xml`: XML data
- `.json`: JSON data (via connector)

## References
- OneStream Foundation Handbook, Chapter 6: Data Integration
- Data Management Configuration Guide
- Business Rules Development Guide
- Workflow Data Protection Guide
