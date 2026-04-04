# Troubleshooting and Debugging Calculations

## Overview

Debugging calculations is a critical skill for OneStream developers. This guide covers common issues, debugging techniques, and performance optimization.

## Common Calculation Errors

### 1. Calculation Not Producing Results

#### Symptoms
- Rule executes without error
- No data appears in target members
- Cube View shows blank cells

#### Causes & Solutions

**Cause 1: Formula Pass Not Set**
```vb
' Member Formula
Formula Pass = <blank> or "NoFormula"  ❌ Wrong for stored calculation
Formula Pass = FormulaPass2            ✓ Correct for stored calculation
```

**Cause 2: Is Consolidated = False**
```vb
' Member Formula Properties
Is Consolidated = Conditional (default)  ❌ Data won't consolidate
Is Consolidated = True                   ✓ Data will consolidate
```

**Cause 3: Wrong Scope**
```vb
' Consolidation Rule
' Check Data Unit being calculated matches rule scope
If args.DataUnitPk.ScenarioId <> expectedScenarioId Then
    Return New ConsolCalcResult(Nothing)  ' No results for this scenario
End If
```

**Cause 4: No Source Data**
```vb
' Verify source data exists
Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer("A#Revenue")

If inputBuffer.DataBufferCells.Count = 0 Then
    api.LogMessage("WARNING: No source data found for Revenue")
    Return New ConsolCalcResult(Nothing)
End If
```

#### Debug Steps

1. **Drill Down From Cube View**
   - Right-click calculated cell
   - Select "Calculation Drill Down"
   - View execution path
   - Check if rule executed
   - See source values used

2. **Log Data Buffers**
   ```vb
   Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer()
   api.LogMessage("Input Buffer Cell Count: " & inputBuffer.DataBufferCells.Count)
   
   For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
       api.LogMessage("Account: " & _
           api.Members.GetMemberName(DimTypeId.Account, cell.CellKey.AccountId) & _
           " Amount: " & cell.CellAmount)
   Next
   
   Dim resultBuffer As New DataBuffer()
   ' ... calculation logic ...
   
   api.LogMessage("Result Buffer Cell Count: " & resultBuffer.DataBufferCells.Count)
   ```

3. **Check Workflow Status**
   - Verify Data Unit is not locked
   - Check Workflow allows data entry
   - Ensure proper security access

4. **Verify Calculation Execution**
   ```vb
   Public Function Main(ByVal si As SessionInfo, _
                        ByVal globals As BRGlobals, _
                        ByVal api As Object, _
                        ByVal args As ConsolCalculateArgs) As ConsolCalcResult
       
       api.LogMessage("=== CALCULATION STARTED ===")
       api.LogMessage("Data Unit: " & args.DataUnitPk.ToString())
       api.LogMessage("Time: " & DateTime.Now.ToString())
       
       Try
           ' Calculation logic
           api.LogMessage("=== CALCULATION COMPLETED SUCCESSFULLY ===")
       Catch ex As Exception
           api.LogMessage("=== CALCULATION FAILED ===")
           api.LogMessage("Error: " & ex.Message)
           Throw
       End Try
   End Function
   ```

### 2. Calculation Producing Inconsistent Results

#### Symptoms
- Results vary unexpectedly
- Different values for same input
- Random errors

#### Causes & Solutions

**Cause 1: Pass Order Issues**
```vb
' BAD: Calculation depends on data from same pass
' Rule A (Pass 2) calculates Margin = Revenue - COGS
' Rule B (Pass 2) calculates MarginPct = Margin / Revenue
' Problem: Rule B might run before Rule A

' GOOD: Use sequential passes
' Rule A (Pass 2) calculates Margin
' Rule B (Pass 3) calculates MarginPct  ✓ Guaranteed Margin exists
```

**Cause 2: Circular References**
```vb
' BAD: Account A uses Account B, Account B uses Account A
A#AccountA = A#AccountB * 1.1
A#AccountB = A#AccountA / 1.1
' Result: Infinite loop or error

' GOOD: Break circular dependency
A#AccountA = A#SourceData * 1.1
A#AccountB = A#SourceData / 1.1
```

**Cause 3: Member Formula Execution Order**
```vb
' Member Formulas execute in unpredictable order within same pass
' Solution: Use Consolidation Rule for dependent calculations
```

### 3. Compilation Errors

#### Invalid Formula Script

**Error: "Invalid Member Name"**
```vb
' BAD
= A#Reveune  ❌ Typo in member name

' GOOD
= A#Revenue  ✓ Correct spelling
```

**Error: "Unclosed Parentheses"**
```vb
' BAD
= (A#Revenue - A#COGS  ❌ Missing closing parenthesis

' GOOD
= (A#Revenue - A#COGS)  ✓ Balanced parentheses
```

**Error: "Invalid Operator"**
```vb
' BAD
= A#Revenue x A#Factor  ❌ Invalid operator 'x'

' GOOD
= A#Revenue * A#Factor  ✓ Use * for multiplication
```

#### VB.NET Compilation Errors

**Error: "Variable Not Declared"**
```vb
' BAD
result = inputValue * 1.1  ❌ Variable not declared

' GOOD
Dim result As Decimal = inputValue * 1.1  ✓ Declared with type
```

**Error: "Type Mismatch"**
```vb
' BAD
Dim accountId As Integer = "Revenue"  ❌ String assigned to Integer

' GOOD
Dim accountName As String = "Revenue"
Dim accountId As Integer = api.Members.GetMemberId(DimTypeId.Account, accountName).Id
```

**Error: "Object Not Set"**
```vb
' BAD
Dim cell As DataBufferCell = buffer.GetCell(...)
Dim amount = cell.CellAmount  ❌ cell might be Nothing

' GOOD
Dim cell As DataBufferCell = buffer.GetCell(...)
If cell IsNot Nothing AndAlso cell.IsData Then
    Dim amount = cell.CellAmount
End If
```

## Debugging Techniques

### 1. Calculate with Logging

**Enable in Consolidation Rule:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Log start
    api.LogMessage("=== Starting Calculation ===")
    
    ' Log input
    Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer()
    api.LogMessage($"Input cells: {inputBuffer.DataBufferCells.Count}")
    
    ' Log each calculation step
    Dim resultBuffer As New DataBuffer()
    Dim totalProcessed As Integer = 0
    
    For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
        ' Log details
        Dim accountName As String = api.Members.GetMemberName(DimTypeId.Account, cell.CellKey.AccountId)
        api.LogMessage($"Processing: {accountName} = {cell.CellAmount}")
        
        ' Perform calculation
        Dim result As Decimal = cell.CellAmount * 1.1
        
        ' Log result
        api.LogMessage($"Result: {result}")
        
        ' Write to buffer
        resultBuffer.SetDataCell(cell.CellKey, result)
        totalProcessed += 1
    Next
    
    ' Log summary
    api.LogMessage($"Total cells processed: {totalProcessed}")
    api.LogMessage("=== Calculation Complete ===")
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

**View Log:**
1. Execute Consolidation
2. System > Task Activity
3. Find calculation task
4. Click to view log details
5. Review logged messages

### 2. Calculation Drill Down

**Usage:**
1. Navigate to Cube View
2. Right-click on calculated cell
3. Select "Calculation Drill Down"
4. Review calculation path

**Information Shown:**
- Execution order (passes)
- Source data values
- Intermediate calculations
- Final result
- Rule/formula used
- Execution time

### 3. Step-Through Debugging

**For Complex Rules:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Add breakpoint logic
    Dim debugEntity As String = "TestEntity"
    Dim debugAccount As String = "TestAccount"
    
    Dim currentEntity As String = api.Members.GetMemberName(DimTypeId.Entity, args.DataUnitPk.EntityId)
    
    ' Only debug specific entities
    If currentEntity = debugEntity Then
        api.LogMessage("=== DEBUG MODE: " & debugEntity & " ===")
        
        ' Detailed logging for this entity
        Dim buffer As DataBuffer = api.Data.GetDataBuffer()
        For Each cell As DataBufferCell In buffer.DataBufferCells.Values
            Dim acct As String = api.Members.GetMemberName(DimTypeId.Account, cell.CellKey.AccountId)
            If acct = debugAccount Then
                api.LogMessage("DEBUG: Found " & debugAccount & " = " & cell.CellAmount)
                ' More detailed debugging here
            End If
        Next
    End If
    
    ' Normal calculation logic
    ' ...
End Function
```

### 4. Unit Testing

**Create Test Rule:**
```vb
Public Function TestCalculation(ByVal si As SessionInfo, _
                                ByVal globals As BRGlobals, _
                                ByVal api As Object) As String
    
    Dim results As New StringBuilder()
    results.AppendLine("=== Calculation Unit Tests ===")
    
    ' Test 1: Basic arithmetic
    Dim test1_input As Decimal = 100
    Dim test1_expected As Decimal = 110
    Dim test1_actual As Decimal = test1_input * 1.1
    Dim test1_pass As Boolean = (test1_actual = test1_expected)
    results.AppendLine($"Test 1: {If(test1_pass, "PASS", "FAIL")} - Expected {test1_expected}, Got {test1_actual}")
    
    ' Test 2: Division by zero handling
    Dim test2_numerator As Decimal = 100
    Dim test2_denominator As Decimal = 0
    Dim test2_actual As Decimal = If(test2_denominator <> 0, test2_numerator / test2_denominator, 0)
    Dim test2_pass As Boolean = (test2_actual = 0)
    results.AppendLine($"Test 2: {If(test2_pass, "PASS", "FAIL")} - Division by zero handled")
    
    ' Test 3: Data retrieval
    Try
        Dim test3_value As Decimal = api.Data.GetDataCell("A#Revenue")
        Dim test3_pass As Boolean = True
        results.AppendLine($"Test 3: PASS - Retrieved Revenue = {test3_value}")
    Catch ex As Exception
        results.AppendLine($"Test 3: FAIL - {ex.Message}")
    End Try
    
    Return results.ToString()
End Function
```

## Performance Optimization

### 1. Data Buffer vs Direct API Calls

**SLOW - Don't do this:**
```vb
' BAD: Direct API call in loop (very slow)
For Each accountName In accountList
    For Each flowName In flowList
        For Each originName In originList
            Dim value As Decimal = api.Data.GetDataCell($"A#{accountName}.F#{flowName}.O#{originName}")
            ' Process value
        Next
    Next
Next
' Result: Thousands of individual database calls
```

**FAST - Do this:**
```vb
' GOOD: Use Data Buffer (very fast)
Dim buffer As DataBuffer = api.Data.GetDataBuffer()

For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' Process all data in memory
    Dim value As Decimal = cell.CellAmount
Next
' Result: Single database call, in-memory processing
```

### 2. Member ID Caching

**SLOW:**
```vb
For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' BAD: Lookup member ID every iteration
    Dim revenueId As Integer = api.Members.GetMemberId(DimTypeId.Account, "Revenue").Id
    
    If cell.CellKey.AccountId = revenueId Then
        ' Process
    End If
Next
```

**FAST:**
```vb
' GOOD: Cache member IDs before loop
Dim revenueId As Integer = api.Members.GetMemberId(DimTypeId.Account, "Revenue").Id
Dim cogsId As Integer = api.Members.GetMemberId(DimTypeId.Account, "COGS").Id
Dim marginId As Integer = api.Members.GetMemberId(DimTypeId.Account, "GrossMargin").Id

For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' Use cached IDs
    If cell.CellKey.AccountId = revenueId Then
        ' Process
    End If
Next
```

### 3. Early Filtering

**SLOW:**
```vb
For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' Process all cells
    Dim result As Decimal = ComplexCalculation(cell)
    
    ' Filter at end
    If cell.CellKey.AccountId = targetAccountId Then
        resultBuffer.SetDataCell(cell.CellKey, result)
    End If
Next
```

**FAST:**
```vb
For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' Filter first
    If cell.CellKey.AccountId <> targetAccountId Then
        Continue For  ' Skip unnecessary processing
    End If
    
    ' Only process relevant cells
    Dim result As Decimal = ComplexCalculation(cell)
    resultBuffer.SetDataCell(cell.CellKey, result)
Next
```

### 4. Minimize Object Creation

**SLOW:**
```vb
For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' BAD: Create new object each iteration
    Dim calculator As New CalculationHelper()
    Dim result As Decimal = calculator.Calculate(cell.CellAmount)
Next
```

**FAST:**
```vb
' GOOD: Create object once
Dim calculator As New CalculationHelper()

For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' Reuse object
    Dim result As Decimal = calculator.Calculate(cell.CellAmount)
Next
```

### 5. Batch Processing

**Process data in chunks when possible:**
```vb
Dim batchSize As Integer = 1000
Dim processedCount As Integer = 0

For Each cell As DataBufferCell In buffer.DataBufferCells.Values
    ' Process cell
    resultBuffer.SetDataCell(cell.CellKey, calculatedValue)
    processedCount += 1
    
    ' Log progress in batches
    If processedCount Mod batchSize = 0 Then
        api.LogMessage($"Processed {processedCount} cells")
    End If
Next
```

## Performance Monitoring

### Measure Execution Time

```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    Dim startTime As DateTime = DateTime.Now
    api.LogMessage("Calculation started: " & startTime.ToString())
    
    ' Calculation logic
    Dim resultBuffer As New DataBuffer()
    ' ...
    
    Dim endTime As DateTime = DateTime.Now
    Dim duration As TimeSpan = endTime - startTime
    
    api.LogMessage($"Calculation completed: {endTime}")
    api.LogMessage($"Duration: {duration.TotalSeconds} seconds")
    api.LogMessage($"Cells processed: {resultBuffer.DataBufferCells.Count}")
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

### Track Performance Metrics

```vb
' Log detailed metrics
api.LogMessage("=== Performance Metrics ===")
api.LogMessage($"Input cells: {inputBuffer.DataBufferCells.Count}")
api.LogMessage($"Output cells: {resultBuffer.DataBufferCells.Count}")
api.LogMessage($"Processing rate: {resultBuffer.DataBufferCells.Count / duration.TotalSeconds} cells/second")
api.LogMessage($"Memory used: {GC.GetTotalMemory(False) / 1024 / 1024} MB")
```

## Key Takeaways

1. Use logging extensively for debugging
2. Calculation Drill Down shows execution path
3. Check Formula Pass and Is Consolidated settings
4. Verify source data exists before calculating
5. Use Data Buffers, never api.Data.GetDataCell in loops
6. Cache member ID lookups
7. Filter early to skip unnecessary processing
8. Handle null/zero cases explicitly
9. Test edge cases (zero, negative, missing data)
10. Monitor performance and optimize slow rules

## Related Topics
- Finance Engine Basics (see finance_rules_01_finance_engine.md)
- Data Buffers (see finance_rules_02_data_buffers.md)
- Calculation Management (see finance_rules_04_calculation_management.md)
- Common Rule Examples (see finance_rules_06_rule_examples.md)
