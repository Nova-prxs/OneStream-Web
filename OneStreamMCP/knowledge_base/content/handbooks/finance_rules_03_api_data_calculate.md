# Chapter 3: api.Data.Calculate (ADC)

> **Source**: OneStream Finance Rules and Calculations Handbook by Jon Golembiewski (2022)
> **Pages**: 51-92 | **Critical Chapter**: Core Calculation API

## Overview

The `api.Data.Calculate` function (commonly abbreviated as **ADC**) is the most critical and commonly used API in OneStream Finance calculations. This function performs Data Buffer math operations - allowing you to multiply, divide, add, and subtract entire Data Buffers of hundreds or thousands of data cells with a single equation.

---

## Table of Contents

1. [A Quick Revisit](#a-quick-revisit)
2. [ADC Syntax](#adc-syntax)
3. [Data Buffer Math](#data-buffer-math)
4. [Unbalanced Buffers and Functions](#unbalanced-buffers-and-functions)
5. [Dimension Filters](#dimension-filters)
6. [Custom Calculate](#custom-calculate)
7. [Api.Pov Functions](#apipov-functions)
8. [Linking to a Dashboard](#linking-to-a-dashboard)
9. [Advanced Filtering with Eval](#advanced-filtering-with-eval)
10. [EventArgs](#eventargs)
11. [Eval2 Function](#eval2-function)
12. [Conclusion](#conclusion)

---

## A Quick Revisit

Before diving deep into the ADC function, let's revisit core concepts from Chapter 2:

### Data Unit and Data Buffer Recap

- **Data Unit Dimensions**: Time, Scenario, Entity, Consolidation
- **Data Buffer Dimensions**: Account, Flow, Origin, Intercompany, UD1-UD8, UD9-UD16
- **Data Buffers** are essentially dictionaries of data cells, like a spreadsheet
- ADC functions operate on these Data Buffers

### Key Principle

When working with ADC functions:
- Data Units define the POV context (filtered at runtime)
- Data Buffers contain the actual values being calculated
- Result Data Buffers inherit dimensional detail from source buffers

---

## ADC Syntax

### Basic Syntax

```vbnet
api.Data.Calculate("A#DestinationAccount = A#SourceAccount1 * A#SourceAccount2")
```

### Full Member Script Syntax

```vbnet
api.Data.Calculate("A#Sales:F#EndBal:O#Import:I#None:U1#None:U2#None:U3#None:U4#None:U5#None:U6#None:U7#None:U8#None = A#Price * A#Volume")
```

### Key Components

| Component | Description |
|-----------|-------------|
| Destination (left side) | Where result Data Buffer is saved |
| Source (right side) | Data Buffers used in calculation |
| Operators | `*`, `/`, `+`, `-` for Data Buffer math |
| Member Script | Dimension#Member format (e.g., `A#Sales`) |

### Using String Variables for Defaults

```vbnet
' Define source and destination defaults
Dim srcDefaults As String = ":F#EndBal:O#Top:I#Top"
Dim dstDefaults As String = ":F#EndBal:O#Import:I#None"

' Use in ADC
api.Data.Calculate("A#Sales" & dstDefaults & " = RemoveZeros(A#Price" & srcDefaults & " * A#Volume" & srcDefaults & ")")
```

> **Best Practice**: Using string variables improves consistency, reduces errors, and makes Calculations more readable.

---

## Data Buffer Math

### How Data Buffer Math Works

When you execute an ADC like `A#Sales = A#Price * A#Volume`:

1. **Retrieve**: A#Price Data Buffer retrieved from storage
2. **Retrieve**: A#Volume Data Buffer retrieved from storage  
3. **Math**: Data Buffer math multiplies the two buffers together
4. **Create**: New Result Data Buffer created
5. **Save**: Result Data Buffer saved to Cube

### Matching Cells

Data Buffer math operates on cells with **matching dimensional intersections**:

```
Price Cell (U1#BugSpray, U2#None) × Volume Cell (U1#BugSpray, U2#None) = Sales Cell (U1#BugSpray, U2#None)
```

If cells don't match dimensionally, no calculation occurs for those intersections.

### Supported Operations

| Operator | Operation |
|----------|-----------|
| `*` | Multiplication |
| `/` | Division |
| `+` | Addition |
| `-` | Subtraction |
| `()` | Grouping for order of operations |

### Complex Formulas

```vbnet
api.Data.Calculate("A#GrossProfit = A#Revenue - (A#COGS + A#DirectLabor)")
api.Data.Calculate("A#Margin = A#GrossProfit / A#Revenue")
```

---

## Unbalanced Buffers and Functions

### What Are Unbalanced Buffers?

Unbalanced Data Buffers occur when source buffers have **different dimensional granularity**.

### Example: Budget Calculation with Spread Percentages

- **Spread Data Buffer**: Has data by Account only (no UD1 detail)
- **Budget Amount**: Has data by Account and UD1 detail

```vbnet
' This creates unbalanced buffers
api.Data.Calculate("A#BudgetResult = A#Budget * A#SpreadPercent")
' Error: Unbalanced Data Buffers
```

### Solution: Use `FilterMembers` Function

```vbnet
' Filter to align dimensional detail
api.Data.Calculate("A#BudgetResult = A#Budget * FilterMembers(A#SpreadPercent, U1#Top)")
```

The `FilterMembers` function reduces the granularity of one buffer to match another.

### Key Functions for Buffer Alignment

| Function | Purpose |
|----------|---------|
| `FilterMembers(Buffer, Filter)` | Reduces buffer to specific members |
| `RemoveZeros(Buffer)` | Removes NoData and Zero cells |
| `RemoveNoData(Buffer)` | Removes only NoData cells |

### Double Unbalanced Scenario

When **two different dimensions** are unbalanced:

```vbnet
' Volume by UD1 (Products), Drivers by UD2 (Cost Centers)
' Requires DBCL approach (see Chapter 4)
```

---

## Dimension Filters

### Purpose

Dimension filters restrict which cells are included in the calculation result, reducing scope and improving performance.

### Syntax

```vbnet
api.Data.Calculate("A#Sales = A#Price * A#Volume", _
    "", "", "", "", "U1#Insects.Base", "", "", "", "", "", "", "")
```

> **Note**: Each dimension has a specific position. Use IntelliSense to ensure correct placement.

### Member Filter Builder

The **Member Filter Builder** is a GUI tool within OneStream for creating Member Filters:

- Access through Dimension management
- Provides syntax validation
- Shows expansion examples
- Helps create complex filters

### Common Member Filter Patterns

```vbnet
' Base members only
"A#IncomeStatement.Base"

' Base members with property filter
"A#IncomeStatement.Base.Where(AccountType = Revenue)"

' Name-based filter
"U1#Top.Base.Where(Name Contains Bug)"

' Multiple filters with comma
"U1#Insects.Base, U1#Arachnids.Base"
```

### Filter Position Reference

```
Position 1:  Account filter
Position 2:  Flow filter
Position 3:  Origin filter
Position 4:  IC filter
Position 5:  UD1 filter
Position 6:  UD2 filter
Position 7:  UD3 filter
Position 8:  UD4 filter
Position 9:  UD5 filter
Position 10: UD6 filter
Position 11: UD7 filter
Position 12: UD8 filter
```

### Important Rules

1. **Base Members Only**: Filters should contain Base members for optimal performance
2. **No Duplicates**: Member Lists cannot contain the same member twice (causes Dictionary error)
3. **Know Your Hierarchies**: Effective filtering requires understanding dimension hierarchies

---

## Collapsing Detail

### What is Collapsing?

Removing dimensional detail by specifying a destination member for "open" dimensions.

### Example

```vbnet
' Without collapsing - detail preserved
api.Data.Calculate("A#Sales = A#Price * A#Volume")

' With collapsing - Origin and Flow collapsed
api.Data.Calculate("A#Sales:O#Import:F#EndBal = A#Price:O#Top:F#Top * A#Volume:O#Top:F#Top")
```

### Origin Dimension Best Practices

**O#Import vs O#Forms**:

| Option | Advantage | Disadvantage |
|--------|-----------|--------------|
| O#Import | Preserves adjustment audit trail | May conflict with imported data |
| O#Forms | No conflict with imports | Loses adjustment audit trail |

> **Recommendation**: Use separate UD or Flow member to separate imported vs calculated data

### Always Collapse When Possible

Collapsing reduces:
- Database size (fewer records)
- Processing time
- Potential data integrity issues

**Commonly Collapsed Dimensions**: Origin, Flow, Intercompany

---

## Remove NoData/Zeros

### Functions

| Function | Removes |
|----------|---------|
| `RemoveNoData` | Cells with CellAmount of NoData |
| `RemoveZeros` | NoData cells AND cells with stored Zero |

### Usage

```vbnet
api.Data.Calculate("A#Sales = RemoveZeros(A#Price * A#Volume)")
```

### Why Remove Zeros?

Data Buffers can contain:
- **Stored Data**: Real entered/imported values
- **Derived Data**: Calculated by aggregation
- **StoredButNoActivity**: Stored zeros

Without RemoveZeros, calculated results may contain unnecessary zero records that:
- Bloat database size
- Impact performance
- Create confusing audit trails

### Example Scenario

In a YTD scenario, some periods may have no activity:
- January: Data for all products
- February: Data for only one product

Without RemoveZeros: 6 cells created (4 with zero amounts)
With RemoveZeros: 1 cell created (only real data)

---

## Durable Data

### Storage Types

| Type | Cleared by DUCS | Use Case |
|------|-----------------|----------|
| Calculation | Yes | Standard DUCS calculations |
| DurableCalculation | No | Custom Calculate results |

### Setting Durable Data

```vbnet
' isDurableData parameter (last Boolean argument)
api.Data.Calculate("A#Sales = A#Price * A#Volume", _
    "", "", "", "", "", "", "", "", "", "", "", "", True)
```

### Clear Calculated Data Function

When using durable data, always include a clear script:

```vbnet
' Clear before recalculating
api.Data.ClearCalculatedData( _
    True,   ' clearConsolidatedData
    True,   ' clearTranslatedData  
    True,   ' clearDurableData
    "A#Sales:U1#Top.Base", "", "", "", "", "", "", "", "", "", "", "")

' Then calculate
api.Data.Calculate("A#Sales = A#Price * A#Volume", _
    "", "", "", "", "U1#Top.Base", "", "", "", "", "", "", "", True)
```

> **Critical**: Mirror the destination Member Script and filters in both ClearCalculatedData and ADC functions.

---

## Custom Calculate

### When to Use Custom Calculate

Custom Calculate is used when:
- Calculations run outside the DUCS workflow
- User-triggered calculations are needed
- Durable data storage is required
- Dashboard integration is needed

### Key Differences from DUCS

| Feature | DUCS | Custom Calculate |
|---------|------|------------------|
| Data Clearing | Automatic | Manual (ClearCalculatedData) |
| Storage Type | Calculation | DurableCalculation |
| Trigger | Workflow | User/Dashboard/Sequence |
| POV Control | System | User-defined |

### Custom Calculate Structure

```vbnet
' Get POV from Data Management Step
Dim pov As String = api.Pov.UD1.Name

' Clear previous data
api.Data.ClearCalculatedData(True, True, True, _
    "A#Sales:U1#" & pov, "", "", "", "", "", "", "", "", "", "", "")

' Calculate with durable storage
api.Data.Calculate("A#Sales:U1#" & pov & " = A#Price * A#Volume", _
    "", "", "", "", "", "", "", "", "", "", "", "", True)
```

---

## Api.Pov Functions

### Purpose

Reference POV settings in calculations to limit scope dynamically.

### Data Unit POV (Works in DUCS)

```vbnet
Dim entityName As String = api.Pov.Entity.Name
Dim timeName As String = api.Pov.Time.Name
Dim scenarioName As String = api.Pov.Scenario.Name
```

### Account-Level POV (Custom Calculate Only)

In Custom Calculate, Account-level dimensions can be set via:

1. Data Management Step properties
2. Dashboard parameters

```vbnet
' Reference POV set in Data Management Step
Dim productPov As String = api.Pov.UD1.Name

api.Data.Calculate("A#Sales:U1#" & productPov & " = A#Price * A#Volume")
```

---

## Linking to a Dashboard

### Overview

Custom Calculate functions can be linked to Dashboards for user-driven calculations.

### Dashboard Components

1. **Combo Box**: User selects parameter (e.g., Product)
2. **Button**: Triggers Custom Calculate
3. **Cube View**: Displays results

### Combo Box Setup

```
Member List Parameter: U1#Insects.Base
Selected Value: Passed to calculation
```

### Button Configuration

```
Action: Execute Data Management Sequence
Name Value Pairs: ProductSelection=[SelectedUD1Value]
```

### Data Management Step Configuration

```
UD1 POV Field: [ProductSelection]
```

### Execution Flow

1. User selects Product from combo box
2. User clicks Calculate button
3. Parameter passed to Data Management Step
4. Calculation runs for selected Product only
5. Cube View refreshes to show results

> **Benefit**: Reduced calculation scope = faster execution + user control

---

## Advanced Filtering with Eval

### What is Eval?

The `Eval` function provides the ability to evaluate individual data cells within a Data Buffer, enabling:
- Cell Amount filtering
- Complex logic on Dimension Members
- Per-cell manipulation before calculation

### Basic Eval Syntax

```vbnet
api.Data.Calculate("A#Sales = (Eval)A#Price * A#Volume", _
    "", "", "", "", "", "", "", "", "", "", "", "", _
    False, AddressOf OnEvalDataBuffer)
```

### The OnEvalDataBuffer Subfunction

```vbnet
Private Sub OnEvalDataBuffer(ByVal api As FinanceRulesApi, _
    ByVal evalName As String, _
    ByVal eventArgs As EvalDataBufferEventArgs)
    
    ' Clear result buffer
    eventArgs.ResultDataBuffer.Cells.Clear()
    
    ' Get source buffer
    Dim sourceBuffer As DataBuffer = eventArgs.DataBuffer
    If sourceBuffer.Cells.Count = 0 Then Exit Sub
    
    ' Loop through cells
    For Each sourceCell As DataBufferCell In sourceBuffer.Cells
        ' Apply custom logic
        If sourceCell.CellAmount <= 500 Then
            eventArgs.ResultDataBuffer.SetCell(sourceCell, False)
        End If
    Next
    
End Sub
```

### Eval Use Case: Filter by Cell Amount

**Requirement**: Exclude all Price data cells with amounts greater than 500

**Before Eval**: 6 cells in Price buffer (one with amount > 500)
**After Eval**: 5 cells (high-price cell filtered out)

### Eval Execution Flow

1. A#Price Data Buffer retrieved from storage
2. **Eval function filters A#Price Data Buffer**
3. A#Volume Data Buffer retrieved from storage
4. Data Buffer math performed (Price × Volume)
5. Result Data Buffer saved to Cube

### Code Breakdown

```vbnet
Private Sub OnEvalDataBuffer(ByVal api As FinanceRulesApi, _
    ByVal evalName As String, _
    ByVal eventArgs As EvalDataBufferEventArgs)
    
    ' Lines 360-364: Clear result and check source
    eventArgs.ResultDataBuffer.Cells.Clear()
    Dim priceBuffer As DataBuffer = eventArgs.DataBuffer
    If priceBuffer.Cells.Count = 0 Then Exit Sub
    
    ' Lines 365-373: Loop and filter
    For Each cell As DataBufferCell In priceBuffer.Cells
        ' Check cell amount
        If cell.CellAmount <= 500 Then
            ' Add to result (passes filter)
            eventArgs.ResultDataBuffer.SetCell(cell, False)
        End If
    Next
    
End Sub
```

---

## EventArgs

### Purpose

`EventArgs` is passed to the Eval subfunction and contains:
- Source Data Buffer
- Destination Info
- Result Data Buffer

### Accessing EventArgs Objects

```vbnet
' Get source Data Buffer
Dim sourceBuffer As DataBuffer = eventArgs.DataBuffer

' Get destination info
Dim destInfo As DataBufferDestinationInfo = eventArgs.DestinationInfo

' Get result buffer (to add filtered cells)
Dim resultBuffer As DataBuffer = eventArgs.ResultDataBuffer
```

### EventArgs Properties

| Property | Type | Description |
|----------|------|-------------|
| DataBuffer | DataBuffer | Source buffer being evaluated |
| DataBuffer1 | DataBuffer | First buffer (Eval2) |
| DataBuffer2 | DataBuffer | Second buffer (Eval2) |
| DestinationInfo | DataBufferDestinationInfo | Destination settings |
| ResultDataBuffer | DataBuffer | Result buffer for filtered cells |

---

## Eval2 Function

### Purpose

Eval2 allows analyzing and comparing **two Data Buffers** simultaneously.

### Syntax

```vbnet
api.Data.Calculate("A#Advertising = (Eval2)A#BudgetSales, A#ActualSales", _
    "", "", "", "", "", "", "", "", "", "", "", "", _
    False, AddressOf OnEvalDataBuffer2)
```

### Use Case: New vs Existing Product Detection

**Scenario**: Budget Advertising expense at different rates:
- Existing Products: 10% of Sales
- New Products: 20% of Sales

**Logic**: Compare Budget Sales to Actual Sales - if product exists in Budget but not Actuals, it's a new product.

### OnEvalDataBuffer2 Implementation

```vbnet
Private Sub OnEvalDataBuffer2(ByVal api As FinanceRulesApi, _
    ByVal evalName As String, _
    ByVal eventArgs As EvalDataBufferEventArgs)
    
    ' Clear result buffer
    eventArgs.ResultDataBuffer.Cells.Clear()
    
    ' Get both buffers
    Dim budgetBuffer As DataBuffer = eventArgs.DataBuffer1
    Dim actualBuffer As DataBuffer = eventArgs.DataBuffer2
    
    If budgetBuffer.Cells.Count = 0 Then Exit Sub
    If actualBuffer.Cells.Count = 0 Then Exit Sub
    
    ' Loop through Budget buffer
    For Each budgetCell As DataBufferCell In budgetBuffer.Cells
        ' Check if exists in Actuals
        Dim actualCell As DataBufferCell = actualBuffer.GetCell(budgetCell.DataBufferCellPk)
        
        Dim resultCell As DataBufferCell = budgetCell.Clone()
        
        If actualCell Is Nothing Then
            ' New Product - 20% rate
            resultCell.CellAmount = budgetCell.CellAmount * 0.2
        Else
            ' Existing Product - 10% rate
            resultCell.CellAmount = budgetCell.CellAmount * 0.1
        End If
        
        eventArgs.ResultDataBuffer.SetCell(resultCell, False)
    Next
    
End Sub
```

### Eval2 Code Breakdown

**Lines 558-562**: Initialize and validate
```vbnet
eventArgs.ResultDataBuffer.Cells.Clear()
Dim budgetBuffer As DataBuffer = eventArgs.DataBuffer1
Dim actualBuffer As DataBuffer = eventArgs.DataBuffer2
If budgetBuffer.Cells.Count = 0 Then Exit Sub
If actualBuffer.Cells.Count = 0 Then Exit Sub
```

**Lines 563-567**: Loop and compare
```vbnet
For Each cell1 As DataBufferCell In budgetBuffer.Cells
    Dim cell2 As DataBufferCell = actualBuffer.GetCell(cell1.DataBufferCellPk)
```

**Lines 568-576**: Apply logic
```vbnet
    If cell2 Is Nothing Then
        ' New product - higher advertising rate
        resultCell.CellAmount = cell1.CellAmount * 0.2
    Else
        ' Existing product - standard rate
        resultCell.CellAmount = cell1.CellAmount * 0.1
    End If
    eventArgs.ResultDataBuffer.SetCell(resultCell, False)
Next
```

---

## Conclusion

The `api.Data.Calculate` function is the cornerstone of OneStream financial calculations. Its power lies in:

### Key Strengths

1. **Data Buffer Math**: Process thousands of cells with one equation
2. **Built-in Filtering**: Dimension filters and Member Filters
3. **Flexibility**: RemoveZeros, FilterMembers, Durable Data options
4. **Advanced Control**: Eval/Eval2 for per-cell manipulation
5. **Integration**: Dashboard linking for user-driven calculations

### When to Use What

| Scenario | Approach |
|----------|----------|
| Simple calculation | Basic ADC |
| Different granularity | FilterMembers |
| User-triggered | Custom Calculate + Dashboard |
| Cell-level logic | Eval function |
| Compare two buffers | Eval2 function |
| Ultimate flexibility | DBCL (Chapter 4) |

### Best Practices Summary

1. ✅ Always use `RemoveZeros` to prevent unnecessary records
2. ✅ Collapse dimensions when detail isn't needed
3. ✅ Use string variables for consistent defaults
4. ✅ Match ClearCalculatedData filters with ADC filters
5. ✅ Know your hierarchies for effective filtering
6. ✅ Use Custom Calculate for user-driven processes
7. ✅ Consider Eval for complex per-cell logic

### Next Steps

For scenarios requiring even more flexibility than ADC provides, see **Chapter 4: The Data Buffer Cell Loop (DBCL)** - the manual, long-hand approach to Data Buffer manipulation.

---

## Quick Reference

### Essential ADC Pattern

```vbnet
' Standard pattern for most calculations
Dim src As String = ":F#EndBal:O#Top:I#Top"
Dim dst As String = ":F#EndBal:O#Import:I#None"

api.Data.ClearCalculatedData(True, True, True, _
    "A#Sales" & dst & ":U1#Products.Base", "", "", "", "", "", "", "", "", "", "", "")

api.Data.Calculate("A#Sales" & dst & " = RemoveZeros(A#Price" & src & " * A#Volume" & src & ")", _
    "", "", "", "", "U1#Products.Base", "", "", "", "", "", "", "", True)
```

### Eval Pattern

```vbnet
' Eval pattern for cell-level filtering
api.Data.Calculate("A#Result = (Eval)A#Source * A#Other", _
    "", "", "", "", "", "", "", "", "", "", "", "", _
    False, AddressOf OnEvalDataBuffer)

Private Sub OnEvalDataBuffer(ByVal api As FinanceRulesApi, _
    ByVal evalName As String, _
    ByVal eventArgs As EvalDataBufferEventArgs)
    
    eventArgs.ResultDataBuffer.Cells.Clear()
    
    For Each cell As DataBufferCell In eventArgs.DataBuffer.Cells
        If [YourCondition] Then
            eventArgs.ResultDataBuffer.SetCell(cell, False)
        End If
    Next
End Sub
```

---

*This chapter content extracted from "OneStream Finance Rules and Calculations Handbook" by Jon Golembiewski (2022), pages 51-92.*
