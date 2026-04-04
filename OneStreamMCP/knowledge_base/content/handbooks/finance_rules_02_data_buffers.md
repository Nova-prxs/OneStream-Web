# Data Buffers and the Cell Loop

## Overview

Data Buffers are the primary mechanism for bulk data processing in OneStream Consolidation Rules. They provide in-memory access to cube data for efficient reading, calculating, and writing operations.

## Core Data Buffer Types

### CurrentDataBuffer
- **Purpose**: Contains existing data from the cube
- **Access**: Read-only in Consolidation Rules
- **Content**: All data for the current Data Unit
- **Use**: Source data for calculations

### ResultDataBuffer
- **Purpose**: Receives calculated results
- **Access**: Read-write in Consolidation Rules  
- **Content**: Empty at start, populated by rule
- **Use**: Write calculated values back to cube

## The Data Buffer Cell Loop Pattern

This is the fundamental pattern for processing data in Consolidation Rules:

### The Recipe (Basic Pattern)

```vb
' 1. Get the CurrentDataBuffer
Dim currentBuffer As DataBuffer = api.Data.GetDataBuffer()

' 2. Create ResultDataBuffer
Dim resultBuffer As New DataBuffer()

' 3. Loop through CurrentDataBuffer cells
For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    
    ' 4. Get cell POV (Point of View)
    Dim accountId As Integer = cell.CellKey.AccountId
    Dim flowId As Integer = cell.CellKey.FlowId
    ' ... other dimension IDs
    
    ' 5. Perform calculation logic
    Dim calculatedValue As Decimal = cell.CellAmount * 1.1 ' Example
    
    ' 6. Write to ResultDataBuffer
    resultBuffer.SetDataCell(accountId, flowId, originId, icId, _
                            ud1Id, ud2Id, ud3Id, ud4Id, _
                            ud5Id, ud6Id, ud7Id, ud8Id, _
                            calculatedValue)
Next

' 7. Return ResultDataBuffer
Return New ConsolCalcResult(resultBuffer)
```

## Data Buffer Cell Structure

### DataBufferCell Object

Each cell in a Data Buffer contains:

#### CellKey (Dimension Member IDs)
```vb
cell.CellKey.AccountId        ' Integer ID
cell.CellKey.FlowId           ' Integer ID  
cell.CellKey.OriginId         ' Integer ID
cell.CellKey.ICId             ' Integer ID
cell.CellKey.UD1Id through UD8Id ' Integer IDs
```

#### Cell Values
```vb
cell.CellAmount              ' Decimal - the numeric value
cell.CellStatus             ' Enum - calculation status
cell.StorageType            ' Enum - how data was created
```

#### Cell Status Properties
```vb
cell.IsData                 ' Boolean - has any value
cell.IsNoData              ' Boolean - no value
cell.IsRealData            ' Boolean - actually stored
cell.IsDerivedData         ' Boolean - calculated/derived
```

## Reading Data from CurrentDataBuffer

### Method 1: Direct Cell Access

```vb
Dim buffer As DataBuffer = api.Data.GetDataBuffer()

' Get specific cell by dimension IDs
Dim cell As DataBufferCell = buffer.GetCell(accountId, flowId, originId, icId, _
                                            ud1Id, ud2Id, ud3Id, ud4Id, _
                                            ud5Id, ud6Id, ud7Id, ud8Id)

If cell IsNot Nothing AndAlso cell.IsData Then
    Dim amount As Decimal = cell.CellAmount
End If
```

### Method 2: Loop Through All Cells

```vb
Dim buffer As DataBuffer = api.Data.GetDataBuffer()

For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    Dim amount As Decimal = cell.CellAmount
    ' Process each cell
Next
```

### Method 3: Filter Cells by Criteria

```vb
Dim buffer As DataBuffer = api.Data.GetDataBuffer()

' Get member name objects for comparison
Dim salesAccount As MemberId = api.Members.GetMemberId(DimTypeId.Account, "Sales")

For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' Filter by account
    If cell.CellKey.AccountId = salesAccount.Id Then
        ' Process only Sales account cells
        Dim amount As Decimal = cell.CellAmount
    End If
Next
```

## Writing Data to ResultDataBuffer

### Basic Write Operation

```vb
Dim resultBuffer As New DataBuffer()

' Write a single cell
resultBuffer.SetDataCell(accountId, flowId, originId, icId, _
                        ud1Id, ud2Id, ud3Id, ud4Id, _
                        ud5Id, ud6Id, ud7Id, ud8Id, _
                        calculatedValue)
```

### Writing Multiple Cells

```vb
Dim resultBuffer As New DataBuffer()

' Write multiple cells in loop
For Each sourceCell As DataBufferCell In currentBuffer.DataBufferCells.Values
    Dim newValue As Decimal = sourceCell.CellAmount * 1.5
    
    resultBuffer.SetDataCell(sourceCell.CellKey.AccountId, _
                            sourceCell.CellKey.FlowId, _
                            sourceCell.CellKey.OriginId, _
                            sourceCell.CellKey.ICId, _
                            sourceCell.CellKey.UD1Id, _
                            sourceCell.CellKey.UD2Id, _
                            sourceCell.CellKey.UD3Id, _
                            sourceCell.CellKey.UD4Id, _
                            sourceCell.CellKey.UD5Id, _
                            sourceCell.CellKey.UD6Id, _
                            sourceCell.CellKey.UD7Id, _
                            sourceCell.CellKey.UD8Id, _
                            newValue)
Next
```

### Writing to Different Members

```vb
' Calculate and write to different account
Dim sourceAccountId As Integer = api.Members.GetMemberId(DimTypeId.Account, "Revenue").Id
Dim targetAccountId As Integer = api.Members.GetMemberId(DimTypeId.Account, "RevenueUSD").Id

For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    If cell.CellKey.AccountId = sourceAccountId Then
        Dim convertedValue As Decimal = cell.CellAmount * exchangeRate
        
        ' Write to different account member
        resultBuffer.SetDataCell(targetAccountId, ' Different account!
                                cell.CellKey.FlowId, _
                                cell.CellKey.OriginId, _
                                ' ... rest of dimensions
                                convertedValue)
    End If
Next
```

## Advanced Data Buffer Techniques

### GetDataBuffer with Parameters

```vb
' Get data buffer for specific POV
Dim pov As String = "A#AcctMember.F#FlowMember.O#Origin.I#ICMember"
Dim buffer As DataBuffer = api.Data.GetDataBuffer(pov)
```

### Accumulating Values

```vb
Dim resultBuffer As New DataBuffer()
Dim runningTotal As Decimal = 0

For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    runningTotal += cell.CellAmount
    
    ' Write cumulative value
    resultBuffer.SetDataCell(cell.CellKey, runningTotal)
Next
```

### Conditional Processing

```vb
Dim resultBuffer As New DataBuffer()

For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    Dim newValue As Decimal
    
    ' Conditional logic
    If cell.CellAmount > 0 Then
        newValue = cell.CellAmount * 1.1 ' 10% increase
    ElseIf cell.CellAmount < 0 Then
        newValue = cell.CellAmount * 0.9 ' 10% decrease
    Else
        newValue = 0
    End If
    
    resultBuffer.SetDataCell(cell.CellKey, newValue)
Next
```

### Cross-Dimensional Calculations

```vb
' Calculate values across multiple source members
Dim resultBuffer As New DataBuffer()

Dim revenueId As Integer = api.Members.GetMemberId(DimTypeId.Account, "Revenue").Id
Dim cogsId As Integer = api.Members.GetMemberId(DimTypeId.Account, "COGS").Id
Dim marginId As Integer = api.Members.GetMemberId(DimTypeId.Account, "GrossMargin").Id

' Get revenue and COGS data
Dim revBuffer As DataBuffer = api.Data.GetDataBuffer("A#Revenue")
Dim cogsBuffer As DataBuffer = api.Data.GetDataBuffer("A#COGS")

' Calculate margin
For Each revCell As DataBufferCell In revBuffer.DataBufferCells.Values
    ' Find matching COGS cell
    Dim cogsCell As DataBufferCell = cogsBuffer.GetCell( _
        cogsId, revCell.CellKey.FlowId, revCell.CellKey.OriginId, _
        revCell.CellKey.ICId, revCell.CellKey.UD1Id, revCell.CellKey.UD2Id, _
        revCell.CellKey.UD3Id, revCell.CellKey.UD4Id, revCell.CellKey.UD5Id, _
        revCell.CellKey.UD6Id, revCell.CellKey.UD7Id, revCell.CellKey.UD8Id)
    
    If cogsCell IsNot Nothing Then
        Dim margin As Decimal = revCell.CellAmount - cogsCell.CellAmount
        
        ' Write to margin account
        resultBuffer.SetDataCell(marginId, revCell.CellKey.FlowId, _
                                revCell.CellKey.OriginId, revCell.CellKey.ICId, _
                                revCell.CellKey.UD1Id, revCell.CellKey.UD2Id, _
                                revCell.CellKey.UD3Id, revCell.CellKey.UD4Id, _
                                revCell.CellKey.UD5Id, revCell.CellKey.UD6Id, _
                                revCell.CellKey.UD7Id, revCell.CellKey.UD8Id, _
                                margin)
    End If
Next
```

## Performance Best Practices

### DO's
1. **Use Data Buffers for bulk operations** - Most efficient method
2. **Process entire Data Unit at once** - Minimize database calls
3. **Reuse Member ID lookups** - Cache IDs in variables
4. **Filter early** - Skip unnecessary cells quickly
5. **Batch writes** - Write to ResultDataBuffer in bulk

### DON'Ts
1. **Don't use api.Data.GetDataCell() in loops** - Very slow
2. **Don't lookup member names repeatedly** - Cache conversions
3. **Don't process cells you don't need** - Filter first
4. **Don't call api.Data.SetDataCell() directly** - Use ResultDataBuffer
5. **Don't mix Data Buffer and direct API calls** - Inconsistent performance

## Common Patterns

### Pattern 1: Simple Transformation
Transform all cells by a factor:
```vb
For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    resultBuffer.SetDataCell(cell.CellKey, cell.CellAmount * factor)
Next
```

### Pattern 2: Selective Copy
Copy only specific accounts:
```vb
For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    If accountIdList.Contains(cell.CellKey.AccountId) Then
        resultBuffer.SetDataCell(cell.CellKey, cell.CellAmount)
    End If
Next
```

### Pattern 3: Aggregation
Sum values across a dimension:
```vb
Dim totals As New Dictionary(Of Integer, Decimal)

For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    Dim key As Integer = cell.CellKey.AccountId
    If Not totals.ContainsKey(key) Then
        totals(key) = 0
    End If
    totals(key) += cell.CellAmount
Next

' Write aggregated values
For Each kvp In totals
    resultBuffer.SetDataCell(kvp.Key, flowId, originId, icId, _
                            ud1Id, ud2Id, ud3Id, ud4Id, _
                            ud5Id, ud6Id, ud7Id, ud8Id, _
                            kvp.Value)
Next
```

### Pattern 4: Currency Conversion
Convert all amounts by exchange rate:
```vb
Dim exchangeRate As Decimal = api.Data.GetExchangeRate(...)

For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
    Dim convertedAmount As Decimal = cell.CellAmount * exchangeRate
    resultBuffer.SetDataCell(cell.CellKey, convertedAmount)
Next
```

## Error Handling

```vb
Try
    Dim currentBuffer As DataBuffer = api.Data.GetDataBuffer()
    Dim resultBuffer As New DataBuffer()
    
    For Each cell As DataBufferCell In currentBuffer.DataBufferCells.Values
        Try
            ' Cell processing logic
            Dim result As Decimal = ProcessCell(cell)
            resultBuffer.SetDataCell(cell.CellKey, result)
        Catch cellEx As Exception
            ' Handle individual cell errors
            api.LogMessage("Error processing cell: " & cellEx.Message)
        End Try
    Next
    
    Return New ConsolCalcResult(resultBuffer)
    
Catch ex As Exception
    ' Handle overall errors
    Throw New Exception("Data Buffer processing failed: " & ex.Message)
End Try
```

## Key Takeaways

1. Data Buffers are in-memory structures for efficient bulk processing
2. CurrentDataBuffer is read-only source data
3. ResultDataBuffer receives calculated results
4. Cell Loop pattern is fundamental to Consolidation Rules
5. DataBufferCell contains CellKey (dimension IDs) and CellAmount
6. SetDataCell writes to ResultDataBuffer
7. Always use Data Buffers instead of api.Data.GetDataCell() in loops
8. Cache Member ID lookups outside loops
9. Filter cells early to skip unnecessary processing
10. Return ResultDataBuffer wrapped in ConsolCalcResult

## Related Topics
- Finance Engine Basics (see finance_rules_01_finance_engine.md)
- Common Rule Examples (see finance_rules_04_rule_examples.md)
- Performance Optimization (see finance_rules_05_debugging.md)
