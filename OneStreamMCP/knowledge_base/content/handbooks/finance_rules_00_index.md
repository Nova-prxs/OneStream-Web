# OneStream Finance Rules and Calculations Handbook

## Overview

This handbook provides comprehensive guidance on creating, managing, and optimizing financial calculations in OneStream. Based on the "OneStream Finance Rules and Calculations Handbook" by Jon Golembiewski.

## Table of Contents

### Part 1: Fundamentals
1. **[Finance Engine Basics](finance_rules_01_finance_engine.md)**
   - Finance Engine architecture
   - Data Unit Calculation Sequence (DUCS)
   - FormulaPass execution order
   - Consolidation Rules vs Member Formulas
   - Triggering calculations
   - Storage Types and Cell Status

2. **[Data Buffers and the Cell Loop](finance_rules_02_data_buffers.md)**
   - CurrentDataBuffer and ResultDataBuffer
   - The Cell Loop pattern
   - Reading and writing data efficiently
   - DataBufferCell structure
   - Advanced Data Buffer techniques
   - Performance best practices

### Part 2: Core Calculation APIs
3. **[api.Data.Calculate (ADC)](finance_rules_03_api_data_calculate.md)** ⭐ **NEW**
   - ADC Syntax and Data Buffer Math
   - Unbalanced Buffers and FilterMembers
   - Dimension Filters and Member Filters
   - Custom Calculate and isDurable
   - Dashboard Integration
   - Eval and Eval2 Functions
   - EventArgs for Cell-Level Logic

4. **[Data Buffer Cell Loop (DBCL)](finance_rules_04_data_buffer_cell_loop.md)** ⭐ **NEW**
   - DBCL Components and Pattern
   - GetDataCell Methods (3 approaches)
   - Setting Result Cell Properties
   - When to Use DBCL vs ADC
   - Double Unbalanced Scenarios
   - Performance Optimization

### Part 3: Reporting and Management
5. **[Reporting Calculations (Dynamic)](finance_rules_03_reporting_calculations.md)**
   - Member Formulas
   - Dynamic Calculation Business Rules
   - Cube View calculations
   - Cell-by-cell execution
   - Common functions (Variance, Time, etc.)

6. **[Calculation Management](finance_rules_04_calculation_management.md)**
   - Calculation Matrix
   - Testing strategies
   - Code maintenance and Comments
   - Time functions and Hierarchies
   - SQL Tables for Dynamic Logic

### Part 4: Troubleshooting & Optimization
7. **[Troubleshooting and Performance](finance_rules_07_troubleshooting.md)** ⭐ **NEW**
   - Task Activity and Error Log
   - Logging Techniques (LogDataBuffer, Stopwatch)
   - Calculation Drill Down
   - Common Errors and Solutions
   - Performance Optimization
   - Hardware, Cube Design, Formula Efficiency
   - Global Variables and Formula Variables
   - DimConstants Best Practices

8. **[Debugging Techniques](finance_rules_05_debugging.md)**
   - Additional debugging strategies
   - Calculate with Logging
   - Performance monitoring

### Part 5: Real-World Examples
9. **[Common Rule Examples (Complete)](finance_rules_08_common_examples.md)** ⭐ **NEW**
   - Things to Avoid (Anti-Patterns)
   - Balance Sheet and Flow Calculations
   - Consolidation Rules (EPU, NCI)
   - Seeding Rules (Forecast from Actuals)
   - Allocation Calculations
   - Variance Analysis Calculations
   - Complete Code Templates

10. **[Additional Examples](finance_rules_06_rule_examples.md)**
    - Arithmetic calculations (Gross Margin)
    - Percentage calculations
    - Currency conversion
    - FX rate effects
    - Conditional calculations

## Key Concepts

### DUCS (Data Unit Calculation Sequence)
The ordered sequence in which calculations execute for a Data Unit. Controlled by FormulaPass properties (1-16).

### Data Unit
All data cells within a unique combination of Cube, Scenario, Time, Consolidation, and Entity dimensions.

### Data Buffer
In-memory structure for efficient bulk data processing. Prevents slow cell-by-cell database calls.

### Formula Pass
Execution order for calculations (FormulaPass1-16). Lower passes execute first, enabling calculation dependencies.

### Cell Status
Properties indicating data state: IsData, IsNoData, IsRealData, IsDerivedData, StorageType.

### Stored vs Dynamic Calculations
- **Stored**: Execute during DUCS, results saved to cube (Consolidation Rules)
- **Dynamic**: Execute on-demand when viewing, not saved (Member Formulas, Dynamic Calc Rules)

## Critical Best Practices

### Performance
1. **ALWAYS use Data Buffers for bulk operations**
   - Never use api.Data.GetDataCell() in loops
   - Get entire Data Buffer once, process in memory
   
2. **Cache Member ID lookups**
   - Look up member IDs before loops
   - Reuse cached IDs in iterations

3. **Filter early**
   - Skip unnecessary cells at start of loop
   - Don't process data you won't use

### Code Quality
1. **Handle edge cases**
   - Check for division by zero
   - Verify cells exist (null checks)
   - Handle missing data gracefully

2. **Document thoroughly**
   - Header comments explain purpose
   - Inline comments clarify complex logic
   - Maintain change logs

3. **Test systematically**
   - Unit test individual calculations
   - Integration test dependencies
   - Verify edge cases
   - Monitor performance

### Maintenance
1. **Use Calculation Matrix**
   - Document all calculations
   - Track dependencies
   - Plan execution order

2. **Version control**
   - Check rules into source control
   - Document changes
   - Track modifications

3. **Modular design**
   - Break complex rules into functions
   - Reuse common logic
   - Keep functions focused

## Common Patterns

### Pattern 1: Basic Cell Loop
```vb
Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer()
Dim resultBuffer As New DataBuffer()

For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
    Dim result As Decimal = cell.CellAmount * factor
    resultBuffer.SetDataCell(cell.CellKey, result)
Next

Return New ConsolCalcResult(resultBuffer)
```

### Pattern 2: Cross-Account Calculation
```vb
Dim accountABuffer As DataBuffer = api.Data.GetDataBuffer("A#AccountA")
Dim accountBBuffer As DataBuffer = api.Data.GetDataBuffer("A#AccountB")
Dim resultBuffer As New DataBuffer()

For Each cellA As DataBufferCell In accountABuffer.DataBufferCells.Values
    Dim cellB As DataBufferCell = accountBBuffer.GetCell(...)
    If cellB IsNot Nothing Then
        Dim result = cellA.CellAmount - cellB.CellAmount
        resultBuffer.SetDataCell(targetAccountId, ..., result)
    End If
Next
```

### Pattern 3: Conditional Processing
```vb
For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
    Dim result As Decimal
    
    If cell.CellAmount > threshold Then
        result = cell.CellAmount * highRate
    Else
        result = cell.CellAmount * lowRate
    End If
    
    resultBuffer.SetDataCell(cell.CellKey, result)
Next
```

## Troubleshooting Quick Reference

| Issue | Check | Solution |
|-------|-------|----------|
| No results | Formula Pass | Set to FormulaPass1-16 |
| Data not consolidating | Is Consolidated | Set to True |
| Slow performance | API calls in loop | Use Data Buffers |
| Inconsistent results | Pass order | Use sequential passes |
| Circular reference | Dependencies | Break dependency chain |
| Division error | Zero check | If denominator <> 0 |
| Null reference | Cell existence | If cell IsNot Nothing |

## Learning Path

### Beginner
1. Start with finance_rules_01_finance_engine.md
2. Learn Data Buffers (finance_rules_02_data_buffers.md)
3. Study simple examples (finance_rules_06_rule_examples.md)
4. Practice with test scenarios

### Intermediate
1. Master Reporting Calculations (finance_rules_03_reporting_calculations.md)
2. Implement Calculation Matrix (finance_rules_04_calculation_management.md)
3. Build complex calculations
4. Optimize performance

### Advanced
1. Advanced debugging (finance_rules_05_debugging.md)
2. Complex multi-pass calculations
3. Custom frameworks and utilities
4. Performance tuning at scale

## Additional Resources

### In This MCP
- OneStream Foundation Handbook (best_practices_*.md files)
- RESTFast Integration (restfast_*.md files)
- Business Rules documentation
- Extensibility Rules documentation

### External Resources
- OneStream Design & Reference Guide
- OneStream Community
- OneStream University courses
- OneStream MarketPlace examples

## Document Information

**Source**: OneStream Finance Rules and Calculations Handbook (Jon Golembiewski, 2022)
**Processed**: 2024-11-15
**Version**: 1.0
**MCP Integration**: OneStream_MAC v1.0

## Quick Links

- [Finance Engine Basics →](finance_rules_01_finance_engine.md)
- [Data Buffers →](finance_rules_02_data_buffers.md)
- [Reporting Calculations →](finance_rules_03_reporting_calculations.md)
- [Calculation Management →](finance_rules_04_calculation_management.md)
- [Debugging →](finance_rules_05_debugging.md)
- [Rule Examples →](finance_rules_06_rule_examples.md)

---

**Note**: This handbook focuses on Finance Rules and Calculations. For overall platform best practices, see the OneStream Foundation Handbook files in this knowledge base.
