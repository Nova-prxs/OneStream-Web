# Chapter 4: The Data Buffer Cell Loop (DBCL)

> **Source**: OneStream Finance Rules and Calculations Handbook by Jon Golembiewski (2022)
> **Pages**: 87-97 | **Advanced Topic**: Manual Data Buffer Manipulation

## Overview

When ultimate flexibility is necessary—whether for tackling complex logic or improving Calculation performance—the `api.Data.Calculate` function may prove inadequate. The **Data Buffer Cell Loop (DBCL)** provides complete control over Data Buffer manipulation.

What it lacks in simplicity, the DBCL makes up for in flexibility. Think of it as the long-hand, manual way of doing what the ADC function does behind the scenes.

---

## Table of Contents

1. [The Recipe](#the-recipe)
2. [Complete Example](#complete-example)
3. [Component Breakdown](#component-breakdown)
4. [GetDataCell Methods](#getdatacell-methods)
5. [Setting Result Cell Properties](#setting-result-cell-properties)
6. [When to Use DBCL](#when-to-use-dbcl)
7. [DBCL vs Eval Comparison](#dbcl-vs-eval-comparison)
8. [Advanced Patterns](#advanced-patterns)

---

## The Recipe

### Ingredients

The DBCL technique requires these components:

| Component | Purpose |
|-----------|---------|
| New DataBuffer object | Holds result cells |
| DestinationInfo object | Specifies where results are saved |
| api.Data.GetDataBuffer or GetDataBufferUsingFormula | Retrieves source data |
| For Each loop | Iterates through cells |
| api.Data.GetDataCell | Retrieves additional cell values |
| Data Buffer Cell objects | Individual cell manipulation |
| api.Data.SetDataBuffer | Saves results to Cube |

### Directions

1. Create new Data Buffer and DestinationInfo objects (set aside for later)
2. Create a source Data Buffer and loop through each cell
3. Inside the loop, create Result Data Buffer Cell and modify properties
4. Add Result Data Buffer Cell to the new Data Buffer from step 1
5. Exit loop and write the new Data Buffer to the Cube
6. Execute and enjoy freshly-calculated data

---

## Complete Example

This example replicates `api.Data.Calculate("A#Sales = A#Price * A#Volume")`:

```vbnet
' Step 1: Create Result Data Buffer and DestinationInfo
Dim resultDataBuffer As New DataBuffer
Dim destinationInfo As New DataBufferDestinationInfo("A#Sales:F#EndBal:O#Import:I#None")

' Step 2: Get Volume Account Member ID (before loop for performance)
Dim volumeAcctId As Integer = api.Members.GetMemberId(DimType.Account, "Volume")

' Step 3: Get Source Data Buffer
Dim priceBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula( _
    "RemoveZeros(A#Price:O#Top:F#Top:I#Top)")

' Step 4: Loop through source cells
For Each sourceCell As DataBufferCell In priceBuffer.Cells
    
    ' Create result cell from source (inherits all properties)
    Dim resultCell As New DataBufferCell(sourceCell)
    
    ' Get Volume data cell
    Dim volumeCell As DataCell = api.Data.GetDataCell( _
        "A#Volume:O#Top:F#Top:I#Top" & _
        ":U1#" & sourceCell.GetDimensionName(DimType.UD1) & _
        ":U2#" & sourceCell.GetDimensionName(DimType.UD2))
    
    ' Set result cell Account
    resultCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account, "Sales")
    
    ' Calculate and set result amount
    resultCell.CellAmount = sourceCell.CellAmount * volumeCell.CellAmount
    
    ' Add to Result Data Buffer
    resultDataBuffer.SetCell(resultCell, False)
    
Next

' Step 5: Write to Cube
api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
```

---

## Component Breakdown

### ResultDataBuffer

A Data Buffer is a Dictionary of data cells—like a spreadsheet. The Result Data Buffer starts empty and fills with cells during the loop.

```vbnet
Dim resultDataBuffer As New DataBuffer
```

### DestinationInfo

Specifies Dimension Members for where the Result Data Buffer will be written. Equivalent to the left side of an ADC formula.

```vbnet
' Basic - just Account destination
Dim destInfo As New DataBufferDestinationInfo("A#Sales")

' Full specification
Dim destInfo As New DataBufferDestinationInfo( _
    "A#Sales:F#EndBal:O#Import:I#None:U1#None:U2#None")
```

> **Note**: DestinationInfo will overwrite individual cell Dimension settings if both are used for the same Dimension.

### GetDataBufferUsingFormula

Retrieves Data Buffers with formula support:

```vbnet
' Simple retrieval
Dim buffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#Price")

' With formulas and functions
Dim buffer As DataBuffer = api.Data.GetDataBufferUsingFormula( _
    "RemoveZeros(A#Price:O#Top:F#Top * A#Adjustment:O#Top:F#Top)")

' Using FilterMembers
Dim buffer As DataBuffer = api.Data.GetDataBufferUsingFormula( _
    "FilterMembers(A#Spread, U1#Top)")
```

### For Each Loop

Iterates through each data cell in the source buffer:

```vbnet
For Each sourceCell As DataBufferCell In priceBuffer.Cells
    ' Process each cell
    ' Access: sourceCell.CellAmount, sourceCell.DataBufferCellPk, sourceCell.CellStatus
Next
```

### Result Cell Creation

Create result cells that inherit source properties:

```vbnet
' Inherit from source (recommended)
Dim resultCell As New DataBufferCell(sourceCell)

' Or create blank and set all properties manually
Dim resultCell As New DataBufferCell()
```

> **Important**: Declare result cell inside loop with `New` to avoid reusing the same cell for each iteration.

---

## GetDataCell Methods

Three methods to retrieve data cells, each with different performance characteristics:

### Method 1: Using Source Cell Member Names

```vbnet
Dim volumeCell As DataCell = api.Data.GetDataCell( _
    "A#Volume:F#EndBal:O#Top:I#Top" & _
    ":U1#" & sourceCell.GetDimensionName(DimType.UD1) & _
    ":U2#" & sourceCell.GetDimensionName(DimType.UD2) & _
    ":U3#" & sourceCell.GetDimensionName(DimType.UD3) & _
    ":U4#" & sourceCell.GetDimensionName(DimType.UD4))
```

**Performance**: Slowest - names must be retrieved then converted to IDs

### Method 2: Using Source Cell IDs to Build DataCellPk

```vbnet
' Get Account ID before loop
Dim volumeAcctId As Integer = api.Members.GetMemberId(DimType.Account, "Volume")

' Inside loop - build DataCellPk
Dim cellPk As New DataCellPk()
cellPk.AccountId = volumeAcctId
cellPk.FlowId = DimConstants.Flow.EndBal
cellPk.OriginId = DimConstants.Origin.Input
cellPk.ICId = DimConstants.IC.None
cellPk.UD1Id = sourceCell.DataBufferCellPk.UD1Id
cellPk.UD2Id = sourceCell.DataBufferCellPk.UD2Id
cellPk.UD3Id = sourceCell.DataBufferCellPk.UD3Id
cellPk.UD4Id = sourceCell.DataBufferCellPk.UD4Id

Dim volumeCell As DataCell = api.Data.GetDataCell(cellPk)
```

**Performance**: Best - IDs already in memory

### Method 3: Convert DataBufferCellPk to Member Script

```vbnet
' Create MemberScriptBuilder from source cell
Dim msBuilder As New MemberScriptBuilder()
api.Members.ApplyDataBufferCellPkToMemberScriptBuilder(msBuilder, sourceCell.DataBufferCellPk)

' Modify specific dimensions
msBuilder.SetAccountName("Volume")
msBuilder.SetOriginName("Top")
msBuilder.SetFlowName("Top")

' Get data cell using built script
Dim volumeCell As DataCell = api.Data.GetDataCell(msBuilder.GetMemberScript())
```

**Performance**: Second-best

### DimConstants Reference

For system-defined Dimension Members:

```vbnet
' Origin Members
DimConstants.Origin.Input
DimConstants.Origin.Import
DimConstants.Origin.Forms
DimConstants.Origin.Calc

' IC Members
DimConstants.IC.None
DimConstants.IC.Top

' Flow Members
DimConstants.Flow.EndBal
DimConstants.Flow.Beginning
```

### api.Members.GetMemberId

For user-created Members:

```vbnet
Dim salesAcctId As Integer = api.Members.GetMemberId(DimType.Account, "Sales")
Dim bugSprayId As Integer = api.Members.GetMemberId(DimType.UD1, "BugSpray")
```

---

## Setting Result Cell Properties

### Setting Dimension Members

**Method 1: Using Member IDs (faster)**

```vbnet
resultCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account, "Sales")
resultCell.DataBufferCellPk.OriginId = DimConstants.Origin.Import
resultCell.DataBufferCellPk.FlowId = DimConstants.Flow.EndBal
resultCell.DataBufferCellPk.ICId = DimConstants.IC.None
```

**Method 2: Using SetDimension Extension (slower but readable)**

```vbnet
resultCell.SetDimension(DimType.Account, "Sales")
resultCell.SetDimension(DimType.Origin, "Import")
resultCell.SetDimension(DimType.Flow, "EndBal")
```

### Setting Cell Amount

```vbnet
' Direct calculation
resultCell.CellAmount = sourceCell.CellAmount * volumeCell.CellAmount

' With logic
If volumeCell.CellAmount <> 0 Then
    resultCell.CellAmount = sourceCell.CellAmount / volumeCell.CellAmount
Else
    resultCell.CellAmount = 0
End If
```

### Setting Cell Status

```vbnet
' Standard data cell
resultCell.CellStatus = api.Data.CreateDataCellStatus(False, False)

' NoData cell (for clearing)
resultCell.CellStatus = api.Data.CreateDataCellStatus(True, False)

' Invalid cell
resultCell.CellStatus = api.Data.CreateDataCellStatus(False, True)
```

**CreateDataCellStatus Parameters**:
- First Boolean: IsNoData
- Second Boolean: IsInvalid

### Adding Cell to Result Buffer

```vbnet
' AccumulateIfCellAlreadyExists = False (default - last cell overwrites)
resultDataBuffer.SetCell(resultCell, False)

' AccumulateIfCellAlreadyExists = True (sum with existing)
resultDataBuffer.SetCell(resultCell, True)
```

> **Important**: Use `True` when multiple source cells may write to the same destination intersection.

---

## When to Use DBCL

### Flexibility Use Cases

The DBCL enables logic not possible with ADC:

**Transforming Dimensions**

```vbnet
' Map source UD1 to different destination UD1 using mapping table
Dim mappedUD1Id As Integer = GetMappedMemberId(sourceCell.DataBufferCellPk.UD1Id)
resultCell.DataBufferCellPk.UD1Id = mappedUD1Id
```

**Analyzing Cell Status or Amounts**

```vbnet
For Each cell As DataBufferCell In sourceBuffer.Cells
    Select Case cell.CellStatus.ToString()
        Case "Stored"
            ' Handle stored data
        Case "Derived"
            ' Handle derived data
        Case Else
            ' Handle other cases
    End Select
Next
```

### Performance Advantages

One DBCL can replace multiple ADC functions:

```vbnet
' Instead of multiple ADC calls:
' api.Data.Calculate("A#Sales = ...")
' api.Data.Calculate("A#Returns = ...")
' api.Data.Calculate("A#NetSales = ...")

' Use single DBCL with multiple result cells
For Each sourceCell As DataBufferCell In salesBuffer.Cells
    ' Sales result cell
    Dim salesCell As New DataBufferCell(sourceCell)
    salesCell.DataBufferCellPk.AccountId = salesAcctId
    salesCell.CellAmount = sourceCell.CellAmount * volumeCell.CellAmount
    resultDataBuffer.SetCell(salesCell, False)
    
    ' Returns result cell (1% of Sales)
    Dim returnsCell As New DataBufferCell(sourceCell)
    returnsCell.DataBufferCellPk.AccountId = returnsAcctId
    returnsCell.CellAmount = salesCell.CellAmount * 0.01
    resultDataBuffer.SetCell(returnsCell, False)
Next

' Single write to database
api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
```

> **Key Insight**: Multiple result cells written once vs multiple ADC functions writing multiple times.

---

## DBCL vs Eval Comparison

| Feature | DBCL | Eval |
|---------|------|------|
| Integration with ADC | No (standalone) | Yes (within ADC) |
| Multiple result cells | Easy | Complex |
| Learning curve | Steeper | Moderate |
| Debugging | More visible | Hidden in ADC |
| Double unbalanced | Handles well | Very complex |

### When DBCL is Clearly Better

**Double Unbalanced Scenario**:

Volume by UD1 (Products), Drivers by UD2 (Cost Centers):

```vbnet
' This fails with ADC (Unbalanced Data Buffer error)
' api.Data.Calculate("A#ShippingExpense = A#Volume:U2#None * A#ShippingDrivers:U1#None")

' DBCL solution with nested loops
Dim volumeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#Volume")
Dim driversBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#ShippingDrivers")

For Each volumeCell As DataBufferCell In volumeBuffer.Cells
    For Each driverCell As DataBufferCell In driversBuffer.Cells
        
        Dim resultCell As New DataBufferCell()
        
        ' UD1 from Volume, UD2 from Driver
        resultCell.DataBufferCellPk.UD1Id = volumeCell.DataBufferCellPk.UD1Id
        resultCell.DataBufferCellPk.UD2Id = driverCell.DataBufferCellPk.UD2Id
        resultCell.DataBufferCellPk.AccountId = shippingExpenseAcctId
        
        ' Other dimensions
        resultCell.DataBufferCellPk.FlowId = DimConstants.Flow.EndBal
        resultCell.DataBufferCellPk.OriginId = DimConstants.Origin.Import
        resultCell.DataBufferCellPk.ICId = DimConstants.IC.None
        
        resultCell.CellAmount = volumeCell.CellAmount * driverCell.CellAmount
        resultCell.CellStatus = api.Data.CreateDataCellStatus(False, False)
        
        resultDataBuffer.SetCell(resultCell, True)
        
    Next
Next
```

---

## Advanced Patterns

### Clearing Data with DBCL

```vbnet
' Loop over data to clear
Dim clearBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#OldData:O#Forms")

For Each cell As DataBufferCell In clearBuffer.Cells
    Dim clearCell As New DataBufferCell(cell)
    ' Set to NoData
    clearCell.CellStatus = api.Data.CreateDataCellStatus(True, False)
    resultDataBuffer.SetCell(clearCell, False)
Next

api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
```

### Conditional Dimension Transformation

```vbnet
For Each sourceCell As DataBufferCell In sourceBuffer.Cells
    Dim resultCell As New DataBufferCell(sourceCell)
    
    ' Conditional destination based on source dimension
    Dim sourceUD1Name As String = sourceCell.GetDimensionName(DimType.UD1)
    
    If sourceUD1Name.Contains("Premium") Then
        resultCell.DataBufferCellPk.AccountId = premiumSalesAcctId
    Else
        resultCell.DataBufferCellPk.AccountId = standardSalesAcctId
    End If
    
    resultDataBuffer.SetCell(resultCell, False)
Next
```

### Multiple Source Buffers

```vbnet
Dim priceBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#Price:O#Top:F#Top")
Dim volumeBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#Volume:O#Top:F#Top")
Dim discountBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#Discount:O#Top:F#Top")

For Each priceCell As DataBufferCell In priceBuffer.Cells
    ' Get matching cells from other buffers
    Dim volumeCell As DataBufferCell = volumeBuffer.GetCell(priceCell.DataBufferCellPk)
    Dim discountCell As DataBufferCell = discountBuffer.GetCell(priceCell.DataBufferCellPk)
    
    If volumeCell IsNot Nothing Then
        Dim resultCell As New DataBufferCell(priceCell)
        resultCell.DataBufferCellPk.AccountId = salesAcctId
        
        Dim baseAmount As Decimal = priceCell.CellAmount * volumeCell.CellAmount
        
        ' Apply discount if exists
        If discountCell IsNot Nothing Then
            resultCell.CellAmount = baseAmount * (1 - discountCell.CellAmount)
        Else
            resultCell.CellAmount = baseAmount
        End If
        
        resultDataBuffer.SetCell(resultCell, False)
    End If
Next
```

---

## Performance Considerations

### Best Practices for Performance

1. **Get Member IDs Before Loop**
   ```vbnet
   ' GOOD - called once
   Dim salesId As Integer = api.Members.GetMemberId(DimType.Account, "Sales")
   For Each cell In buffer.Cells
       resultCell.DataBufferCellPk.AccountId = salesId
   Next
   
   ' BAD - called every iteration
   For Each cell In buffer.Cells
       resultCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account, "Sales")
   Next
   ```

2. **Use IDs Instead of Names**
   ```vbnet
   ' GOOD - faster
   resultCell.DataBufferCellPk.AccountId = salesId
   
   ' SLOWER
   resultCell.SetDimension(DimType.Account, "Sales")
   ```

3. **Use DestinationInfo for Common Dimensions**
   ```vbnet
   ' Efficient - applied once to entire buffer
   Dim destInfo As New DataBufferDestinationInfo("A#Sales:F#EndBal:O#Import:I#None")
   ```

4. **Single SetDataBuffer Call**
   ```vbnet
   ' GOOD - one database write
   api.Data.SetDataBuffer(resultDataBuffer, destinationInfo)
   
   ' BAD - multiple writes
   For Each cell In buffer.Cells
       api.Data.SetDataBuffer(singleCellBuffer, destinationInfo)
   Next
   ```

---

## Complete DBCL Template

```vbnet
Public Sub DBCLCalculation(ByVal si As SessionInfo, ByVal globals As BRGlobals, _
    ByVal api As FinanceRulesApi, ByVal args As FinanceRulesArgs)
    
    Try
        ' 1. Clear previous calculated data
        api.Data.ClearCalculatedData(True, True, True, _
            "A#Sales:U1#Products.Base", "", "", "", "", "", "", "", "", "", "", "")
        
        ' 2. Create Result Data Buffer and DestinationInfo
        Dim resultDataBuffer As New DataBuffer
        Dim destinationInfo As New DataBufferDestinationInfo( _
            "A#Sales:F#EndBal:O#Import:I#None")
        
        ' 3. Cache Member IDs (before loop)
        Dim salesAcctId As Integer = api.Members.GetMemberId(DimType.Account, "Sales")
        Dim volumeAcctId As Integer = api.Members.GetMemberId(DimType.Account, "Volume")
        
        ' 4. Get Source Data Buffer
        Dim priceBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula( _
            "RemoveZeros(A#Price:O#Top:F#Top:I#Top)")
        
        If priceBuffer.Cells.Count = 0 Then Exit Sub
        
        ' 5. Loop through source cells
        For Each sourceCell As DataBufferCell In priceBuffer.Cells
            
            ' Create result cell
            Dim resultCell As New DataBufferCell(sourceCell)
            
            ' Build DataCellPk for Volume lookup
            Dim volumeCellPk As New DataCellPk()
            volumeCellPk.AccountId = volumeAcctId
            volumeCellPk.FlowId = DimConstants.Flow.EndBal
            volumeCellPk.OriginId = DimConstants.Origin.Input
            volumeCellPk.ICId = DimConstants.IC.None
            volumeCellPk.UD1Id = sourceCell.DataBufferCellPk.UD1Id
            volumeCellPk.UD2Id = sourceCell.DataBufferCellPk.UD2Id
            
            ' Get Volume cell
            Dim volumeCell As DataCell = api.Data.GetDataCell(volumeCellPk)
            
            ' Set result properties
            resultCell.DataBufferCellPk.AccountId = salesAcctId
            resultCell.DataBufferCellPk.FlowId = DimConstants.Flow.EndBal
            resultCell.DataBufferCellPk.OriginId = DimConstants.Origin.Import
            resultCell.DataBufferCellPk.ICId = DimConstants.IC.None
            
            ' Calculate amount
            resultCell.CellAmount = sourceCell.CellAmount * volumeCell.CellAmount
            resultCell.CellStatus = api.Data.CreateDataCellStatus(False, False)
            
            ' Add to result buffer
            resultDataBuffer.SetCell(resultCell, False)
            
        Next
        
        ' 6. Write to Cube
        If resultDataBuffer.Cells.Count > 0 Then
            api.Data.SetDataBuffer(resultDataBuffer, destinationInfo, _
                "", "", "", "", "", "", "", "", "", "", "", "", True)
        End If
        
    Catch ex As Exception
        Throw ErrorHandler.LogWrite(si, New XFException(si, ex))
    End Try
    
End Sub
```

---

## Summary

The Data Buffer Cell Loop provides:

- **Ultimate Flexibility**: Complete control over cell manipulation
- **Performance Benefits**: Multiple results with single database write
- **Complex Logic Support**: Handle scenarios ADC cannot
- **Double Unbalanced Solution**: Nested loops solve cross-dimensional calculations

### Decision Guide

| Use ADC When | Use DBCL When |
|--------------|---------------|
| Simple calculations | Complex transformations |
| Balanced buffers | Double unbalanced scenarios |
| Standard filtering | Custom cell-level logic |
| Quick implementation | Performance optimization needed |

---

*This chapter content extracted from "OneStream Finance Rules and Calculations Handbook" by Jon Golembiewski (2022), pages 87-97.*
