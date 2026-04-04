# Reporting Calculations (Dynamic Calculations)

## Overview

Reporting Calculations (also called Dynamic Calculations) execute on-demand when viewing data in reports, cube views, or dashboards. Unlike Consolidation Rules that store data, Reporting Calculations compute values in real-time.

## When They Run

Reporting Calculations execute:
- **On-demand**: Only when data is retrieved for viewing
- **Cell-by-cell**: Individual cell POV at a time
- **Not stored**: Results are calculated each time, not saved to cube

## Types of Reporting Calculations

### 1. Member Formulas

#### Location
Assigned to dimension members in Administration

#### Formula Pass Property
- **MUST be blank or "NoFormula"** for reporting calculations
- If set to FormulaPass1-16, executes in DUCS (not reporting)

#### Syntax
Member formulas use simple expression language:
```
' Simple arithmetic
= A#Revenue - A#COGS

' Percentages
= A#NetIncome / A#Revenue

' Parent members
= A#Revenue.Parent

' Time functions
= A#Revenue.Prior1

' Substitution variables
= A#Actual * |!SV_ExchangeRate!|
```

#### Benefits
- Simple to create - no VB.NET coding
- Easy to maintain
- Visible in member properties
- Good for straightforward calculations

#### Drawbacks
- Limited to simple expressions
- Cannot contain complex logic
- Less flexible than Business Rules
- Performance can be slower for complex formulas

### 2. Dynamic Calculations in Business Rules

#### Location
**Business Rules > Dynamic Calc Business Rules**

#### Required Signature
```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    ' Calculation logic here
End Function
```

#### Assignment
Assigned to Account/Flow/UD members via:
- Member Properties > Formula section
- Select Business Rule
- Specify function name

#### Benefits
- Full VB.NET capabilities
- Complex logic supported
- Conditional calculations
- Access to all API functions
- Can retrieve data from anywhere

#### Drawbacks
- Requires VB.NET knowledge
- More complex to maintain
- Performance consideration for large reports

### 3. Cube View Calculations

#### Location
**Cube Views > Member Formulas section**

#### Scope
- Applies only to specific Cube View
- Not stored in dimension metadata
- View-specific calculations

#### Syntax
```vb
' Define in Cube View Member Formulas
MyCalc = A#Revenue * 1.1

' Reference in Row/Column
R#MyCalc
```

#### Benefits
- View-specific logic
- No metadata changes
- Quick ad-hoc calculations
- Isolated from other views

#### Drawbacks
- Only works in that specific view
- Not reusable across views
- Hidden from dimension structure

## How Reporting Calculations Work

### Cell-by-Cell Execution

For each cell requested in a report:

1. **Determine Cell POV**
   - Scenario, Time, Entity, etc.
   - Account, Flow, Origin, IC, UD1-UD8
   
2. **Check for Formula**
   - Member Formula defined?
   - Business Rule assigned?
   
3. **Execute Calculation**
   - Parse formula or run rule
   - Retrieve required data
   - Compute result
   
4. **Return Value**
   - Return to report/view
   - Display to user

### Retrieving POV Members

In Dynamic Calc Business Rules:

```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    
    ' Get current cell POV members
    Dim scenarioName As String = calcArgs.ScenarioDimPk.Member.Name
    Dim timeName As String = calcArgs.TimeDimPk.Member.Name
    Dim entityName As String = calcArgs.EntityDimPk.Member.Name
    Dim accountName As String = calcArgs.AccountDimPk.Member.Name
    Dim flowName As String = calcArgs.FlowDimPk.Member.Name
    
    ' Perform calculation logic
    ' ...
    
    Return calculatedValue
End Function
```

## Returning Different Data Types

### Numerical Data

#### Method 1: api.Data.GetDataCell

Simple data retrieval:
```vb
' Get single cell value
Dim value As Decimal = api.Data.GetDataCell("A#Revenue.F#Periodic")

Return value
```

#### Method 2: With Formula Expression

```vb
' Calculate with formula
Dim revenue As Decimal = api.Data.GetDataCell("A#Revenue")
Dim cogs As Decimal = api.Data.GetDataCell("A#COGS")

Dim margin As Decimal = revenue - cogs

Return margin
```

#### Method 3: DataCellEx Object

Returns extended cell information:
```vb
Dim cellEx As DataCellEx = api.Data.GetDataCellEx("A#Revenue")

' Access properties
Dim amount As Decimal = cellEx.CellAmount
Dim status As DataCellExStatus = cellEx.CellStatus
Dim isData As Boolean = cellEx.IsData

Return cellEx.CellAmount
```

### Textual Data

Return text values for display:
```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    
    Dim entityName As String = calcArgs.EntityDimPk.Member.Name
    
    ' Return text based on entity
    If entityName = "NYC" Then
        Return "New York City"
    ElseIf entityName = "LAX" Then
        Return "Los Angeles"
    Else
        Return "Other Location"
    End If
End Function
```

### DataCellEx Properties

Extended cell object with additional information:

```vb
Dim cellEx As DataCellEx = api.Data.GetDataCellEx("A#Revenue")

' Numeric value
Dim amount As Decimal = cellEx.CellAmount

' Status information
Dim isData As Boolean = cellEx.IsData
Dim isNoData As Boolean = cellEx.IsNoData
Dim isRealData As Boolean = cellEx.IsRealData
Dim isDerived As Boolean = cellEx.IsDerivedData

' Storage type
Dim storageType As StorageType = cellEx.StorageType
' Values: Input, Calculation, Consolidation, Translation, etc.

' Cell status
Dim status As DataCellExStatus = cellEx.CellStatus
```

## Common Functions

### Time Functions

```vb
' Prior Period
= A#Revenue.Prior1

' Prior Year
= A#Revenue.PriorYear1

' Year To Date
= A#Revenue.YTD

' Quarter To Date
= A#Revenue.QTD

' Specific period
= A#Revenue.M1
```

### Variance Functions

Built-in variance calculations:

#### Variance
```
= Variance(A#Actual, A#Budget)
' Returns: Actual - Budget
```

#### VariancePercent
```
= VariancePercent(A#Actual, A#Budget)
' Returns: (Actual - Budget) / Budget
```

#### BetterWorse
```
= BetterWorse(A#Actual, A#Budget, "Revenue", "Expense")
' Returns: Positive if better, negative if worse
' "Revenue" means higher is better
' "Expense" means lower is better
```

#### BetterWorsePercent
```
= BetterWorsePercent(A#Actual, A#Budget, "Revenue")
' Returns: Percentage better/worse
```

### Substitution Variables

Dynamic values from system variables:

```vb
' Use substitution variable in formula
= A#Actual * |!SV_ExchangeRate!|

' In Business Rule
Dim exchangeRate As Decimal = api.SubVars.GetSubVarValue("SV_ExchangeRate")
```

## Name-Value Pairs

Return multiple values from a calculation:

```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    
    Dim revenue As Decimal = api.Data.GetDataCell("A#Revenue")
    Dim cogs As Decimal = api.Data.GetDataCell("A#COGS")
    Dim margin As Decimal = revenue - cogs
    Dim marginPct As Decimal = If(revenue <> 0, margin / revenue, 0)
    
    ' Return multiple values
    Dim result As New Dictionary(Of String, Object)
    result.Add("Margin", margin)
    result.Add("MarginPercent", marginPct)
    result.Add("Revenue", revenue)
    
    Return result
End Function
```

Access in Cube View:
```
R#MyCalc.Margin
R#MyCalc.MarginPercent
R#MyCalc.Revenue
```

## Advanced Patterns

### Conditional Logic

```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    
    Dim entityType As String = api.Entity.GetEntityProperty(calcArgs.EntityDimPk, "EntityType")
    Dim amount As Decimal = api.Data.GetDataCell("A#Revenue")
    
    ' Apply different logic based on entity type
    Select Case entityType
        Case "Corporate"
            Return amount * 1.0  ' No adjustment
        Case "Regional"
            Return amount * 0.9  ' 10% reduction
        Case "Local"
            Return amount * 0.8  ' 20% reduction
        Case Else
            Return 0
    End Select
End Function
```

### Cross-Dimensional Lookups

```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    
    ' Get data from different scenarios
    Dim actual As Decimal = api.Data.GetDataCell("S#Actual.A#Revenue")
    Dim budget As Decimal = api.Data.GetDataCell("S#Budget.A#Revenue")
    Dim forecast As Decimal = api.Data.GetDataCell("S#Forecast.A#Revenue")
    
    ' Calculate composite value
    Dim result As Decimal = (actual * 0.5) + (budget * 0.25) + (forecast * 0.25)
    
    Return result
End Function
```

### Time-Based Calculations

```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    
    Dim currentMonth As String = calcArgs.TimeDimPk.Member.Name
    
    ' Get YTD value
    Dim ytdValue As Decimal = api.Data.GetDataCell("T#" & currentMonth & ".YTD.A#Revenue")
    
    ' Get prior year same period
    Dim pyValue As Decimal = api.Data.GetDataCell("T#" & currentMonth & ".PriorYear1.A#Revenue")
    
    ' Calculate growth
    Dim growth As Decimal = If(pyValue <> 0, (ytdValue - pyValue) / pyValue, 0)
    
    Return growth
End Function
```

## Performance Considerations

### Best Practices

1. **Minimize data retrievals**
   - Cache values in variables
   - Avoid redundant api.Data calls
   
2. **Use efficient formulas**
   - Simple Member Formulas for basic math
   - Business Rules only when needed
   
3. **Scope appropriately**
   - Don't calculate more than necessary
   - Use Cube View formulas for view-specific needs
   
4. **Consider stored calculations**
   - If calculation rarely changes, use Consolidation Rule
   - Store result, don't recalculate each time
   
5. **Test with production data volumes**
   - Dynamic calcs run for every cell viewed
   - Can impact report performance significantly

### When to Use Dynamic vs Stored

**Use Dynamic Calculations when:**
- Data changes frequently
- Calculation is simple
- Only needed for specific reports
- Storage space is a concern

**Use Stored Calculations (Consolidation Rules) when:**
- Data changes infrequently
- Calculation is complex
- Used across many reports
- Performance is critical

## Common Pitfalls

1. **Circular References**
   - Account A calculates from Account B
   - Account B calculates from Account A
   - Result: Infinite loop or error
   
2. **Missing Data Checks**
   ```vb
   ' BAD: No null check
   Dim result = valueA / valueB  ' Error if valueB = 0
   
   ' GOOD: Check first
   Dim result = If(valueB <> 0, valueA / valueB, 0)
   ```

3. **Incorrect POV**
   ```vb
   ' BAD: Not considering current POV
   Dim value = api.Data.GetDataCell("A#Revenue")  ' Might not be right entity/time
   
   ' GOOD: Be explicit
   Dim value = api.Data.GetDataCell("E#" & entityName & ".A#Revenue")
   ```

4. **Performance Issues**
   - Too many API calls in loop
   - Complex calculations on every cell
   - Not caching lookups

## Testing Dynamic Calculations

### In Cube Views
1. Create test Cube View
2. Add calculated member to row/column
3. View data
4. Verify results

### With Calculate Function
1. Right-click member in metadata
2. Select "Calculate"
3. Enter test POV
4. View results and logs

### Debugging
```vb
Public Function Calculate(ByVal si As SessionInfo, _
                         ByVal globals As BRGlobals, _
                         ByVal api As Object, _
                         ByVal calcArgs As MemberScriptArgs) As Object
    
    Try
        ' Log current POV
        api.LogMessage("Calculating for: " & calcArgs.ToString())
        
        ' Calculation logic
        Dim result As Decimal = PerformCalculation()
        
        ' Log result
        api.LogMessage("Result: " & result.ToString())
        
        Return result
        
    Catch ex As Exception
        api.LogMessage("Error: " & ex.Message)
        Return Nothing
    End Try
End Function
```

## Key Takeaways

1. Reporting Calculations execute on-demand, not stored
2. Three types: Member Formulas, Business Rules, Cube View formulas
3. Execute cell-by-cell for each cell viewed
4. Member Formulas simple, Business Rules powerful
5. Can return numeric or text values
6. Use DataCellEx for extended cell information
7. Built-in functions for variance, time, etc.
8. Performance critical - minimize API calls
9. Test with realistic data volumes
10. Consider stored calculations for complex/frequent use

## Related Topics
- Finance Engine Basics (see finance_rules_01_finance_engine.md)
- Calculation Management (see finance_rules_03_calculation_management.md)
- Debugging (see finance_rules_05_debugging.md)
