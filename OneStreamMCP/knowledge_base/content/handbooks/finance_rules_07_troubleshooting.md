# Chapter 7: Troubleshooting and Performance

> **Source**: OneStream Finance Rules and Calculations Handbook by Jon Golembiewski (2022)
> **Pages**: 143-181 | **Critical Chapter**: Debugging and Optimization

## Overview

"First get the Calculation to work, then get it to work faster!"

Both parts of that statement can be equally challenging. This chapter covers the best troubleshooting techniques for getting Calculations to work—and work fast—as well as common errors and performance optimization strategies.

---

## Table of Contents

1. [Troubleshooting Basics](#troubleshooting-basics)
2. [Task Activity](#task-activity)
3. [Logging Techniques](#logging-techniques)
4. [Calculation Drill Down](#calculation-drill-down)
5. [Common Calculation Errors](#common-calculation-errors)
6. [Performance Overview](#performance-overview)
7. [Hardware and Server Settings](#hardware-and-server-settings)
8. [Cube Design for Performance](#cube-design-for-performance)
9. [Formula Efficiency Tips](#formula-efficiency-tips)
10. [Things to Do](#things-to-do)

---

## Troubleshooting Basics

"Coding is 5% writing the actual code, and 95% testing and troubleshooting."

Understanding the tools available and common errors will give you a big head start.

### Key Troubleshooting Tools

| Tool | Purpose |
|------|---------|
| Task Activity | View task status and error details |
| Error Log | Detailed error information and custom logging |
| LogDataBuffer | Visualize Data Buffer contents |
| Stopwatch | Time code execution |
| Calculation Drill Down | Analyze DUCS step performance |

---

## Task Activity

### Accessing Task Activity

Two ways to access:

1. **Top-right menu** in desktop app
2. **System tab** > Task Activity

### Information Provided

- Task status (Success, Error, Running)
- Task type filtering
- Detailed error information
- Child steps for drill-down

### Viewing Errors

Click the **Error** icon or navigate to Task Activity to view error details:

```
Task Activity > [Failed Task] > Error Details
```

> **Note**: Task Activity is a great starting point but won't lead you far down the road of solving errors. Other techniques will need to be deployed.

---

## Logging Techniques

Logging is perhaps the **best tool** in the bag for a coder. Coding without logging is like trying to land a plane blind.

### Accessing the Error Log

```
System Tab > Error Log
```

### Error Log Contains

- Description
- Error Time
- Error Level
- User
- Application
- Server Tier
- App Server

### Writing to the Error Log

#### API Functions

| Function | Availability |
|----------|--------------|
| `api.LogMessage` | Finance Business Rules (preferred) |
| `BRApi.ErrorLog.LogMessage` | All rules |

> **Performance**: Using `api.LogMessage` instead of BRAPI equivalent generally performs better.

#### Log a String

```vbnet
api.LogMessage("My custom log message")
```

#### Log with Error Level

```vbnet
api.LogMessage("Warning message", XFErrorLevel.Warning)
api.LogMessage("Error message", XFErrorLevel.Error)
api.LogMessage("Info message", XFErrorLevel.Information)
```

#### Log a Decimal

```vbnet
Dim amount As Decimal = 12345.67
api.LogMessage("Amount: " & amount.XFToString())
```

> **Note**: Use `XFToString()` instead of `ToString()` as it is culture-invariant.

### Logging Lists

#### Method 1: String.Join

```vbnet
Dim myList As New List(Of String)
myList.Add("Item1")
myList.Add("Item2")
myList.Add("Item3")

api.LogMessage(String.Join(vbNewLine, myList))
```

#### Method 2: For Each Loop

```vbnet
Dim memberList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter( _
    api.Pov.UD1.DimPk, "U1#Products.Base", Nothing)

Dim memberNames As String = ""
For Each member As MemberInfo In memberList
    memberNames &= member.Member.Name & ", "
Next

api.LogMessage("Members: " & memberNames)
```

> **Warning**: Be careful when logging lists with For Each when there's a chance of high volume items. This can consume a lot of memory and cause server issues.

### Log Data Buffer

The `LogDataBuffer` function is extremely helpful for debugging:

```vbnet
Dim priceBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula( _
    "RemoveZeros(A#Price:O#Top:F#Top)")

priceBuffer.LogDataBuffer(api, "Price Buffer", 100)
```

**Parameters**:
- `api`: The API object
- `name`: String identifier for the log
- `maxCells`: Maximum number of cells to log (recommend 100)

**Output includes**:
- Data Buffer information
- All data cells within the buffer
- Common Members
- Cell amounts and dimension details

### Stopwatch for Performance

For longer Calculations with complex logic:

```vbnet
' Add to header
Imports System.Diagnostics

' In code
Dim sw As Stopwatch = Stopwatch.StartNew()

' ... your code here ...

api.LogMessage("Elapsed: " & sw.Elapsed.TotalSeconds & " seconds")
```

**Complete Example**:

```vbnet
Imports System.Diagnostics

Public Sub Main(...)
    Dim sw As Stopwatch = Stopwatch.StartNew()
    
    ' Get member list
    Dim members As List(Of MemberInfo) = api.Members.GetMembersUsingFilter( _
        api.Pov.UD1.DimPk, "U1#Products.Base", Nothing)
    
    api.LogMessage("Member retrieval: " & sw.Elapsed.TotalMilliseconds & "ms")
    
    sw.Restart()
    
    ' Process members
    For Each member As MemberInfo In members
        ' Processing logic
    Next
    
    api.LogMessage("Member processing: " & sw.Elapsed.TotalMilliseconds & "ms")
End Sub
```

### Rubber Duck Debugging

A popular troubleshooting method when all other options have been exhausted:

1. Articulate your problem to an inanimate object (or colleague)
2. Explain what your code is doing step by step
3. The change in perspective often reveals the solution

> As stupid as it sounds, this method works. Many times, in the midst of explaining the problem, the answer appears!

---

## Calculation Drill Down

### Enable Calculation Logging

To drill down to Calculation details, run **Calculate/Consolidate With Logging**:

**From Data Management**:
```
Execute > Calculate With Logging
```

**From Cube View**:
```
Right-click > Force Consolidate With Logging
```

### Viewing Results

1. Open Task Activity
2. Find completed task
3. Click **Child Steps**
4. Drill into each step

### Drill Down Hierarchy

```
Task Activity
  └── Data Unit (e.g., ACMEGroup 2022M5)
       └── Dependent Data Units
            └── Calculate Local
                 └── CalculateCurrencyConsMember
                      └── CreateDataUnitCache (shows data records in memory)
                      └── CalculateConsMember
                           └── Formula Passes
                                └── Individual Formulas
                                     └── ADC Function Steps
```

### Key Metrics to Watch

| Step | What It Shows |
|------|---------------|
| CreateDataUnitCache | Number of data records in memory (Data Unit Size) |
| Formula Pass duration | Time for each calculation pass |
| Individual formula | Detailed ADC function breakdown |

> **Note**: Calculation drill down is only available for DUCS calculations. Running Calculate With Logging adds significant processing time.

---

## Common Calculation Errors

### Calculation Not Producing Results

When Calculation runs without errors but produces incorrect/no results:

**Debugging Steps**:

1. Set up Cube View with source and result intersections
2. Drill down into source data cells
3. Log Data Buffers to compare dimensionality

**Common Cause**: Data Buffer dimension mismatch

```vbnet
' Log both buffers
Dim avgSalaryBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#AvgSalary")
Dim headcountBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("A#Headcount")

avgSalaryBuffer.LogDataBuffer(api, "AvgSalary", 100)
headcountBuffer.LogDataBuffer(api, "Headcount", 100)
```

**Fix**: Align dimensions in formula

```vbnet
' Before (mismatched Flow)
api.Data.Calculate("A#SalariesBenefits = A#Headcount * A#AvgSalary")

' After (explicitly aligned)
api.Data.Calculate("A#SalariesBenefits:F#EndBal = A#Headcount:F#EndBal * A#AvgSalary:F#EndBal")
```

### Inconsistent or No Results (Formula Pass Issue)

Calculation runs once = correct. Second time = different. Third time = correct again.

**Cause**: Formula Pass dependency issue

**Quick Test**: Change formula to run on **Formula Pass 16** (last pass)

```vbnet
' If this produces consistent results, you have a dependency issue
```

**Solution**: Track dependencies in Calculation Matrix and reorder Formula Passes

### Compilation Error

Syntax mistake in code. Check with **Compile** button before execution.

```
Error Information:
Line 45: ')' expected
```

**Solution**: Fix syntax and recompile

### Invalid Formula Script (Runtime Errors)

Compilation won't catch errors inside formula strings.

**Invalid Member Name**:
```vbnet
' Typo: Priec instead of Price
api.Data.Calculate("A#Sales = A#Priec * A#Volume")
```

**Error**: `Invalid script. There is no Priec Member in the Account Dimension`

**Solution**: Fix the typo

**Unclosed Parentheses**:
```vbnet
' Missing closing parenthesis after U1#None
api.Data.Calculate("A#Result = RemoveZeros(A#Source:U1#None * A#Other)")
```

**Error**: `Unmatched parenthesis in formula`

### Unbalanced Buffer Error

```vbnet
api.Data.Calculate("A#Result = A#Source1:U1#None * A#Source2:U2#None")
```

**Error**: `Unbalanced Data Buffers`

**Solution**: Use FilterMembers or DBCL (see Chapter 3-4)

### Declaring New Result Cell Outside Loop

```vbnet
' WRONG - only first cell added
Dim resultCell As New DataBufferCell(sourceCell)  ' Outside loop

For Each sourceCell In buffer.Cells
    ' resultCell reused - wrong!
    resultDataBuffer.SetCell(resultCell, False)
Next

' CORRECT - new cell each iteration
For Each sourceCell In buffer.Cells
    Dim resultCell As New DataBufferCell(sourceCell)  ' Inside loop
    resultDataBuffer.SetCell(resultCell, False)
Next
```

### Duplicate Members in Filter

```vbnet
' U1#CourseMgt is a Base of U1#Services - appears twice!
api.Data.Calculate("A#Sales = A#Revenue", _
    "", "", "", "", "U1#Services.Base, U1#CourseMgt", ...)
```

**Error**: `An item with the same key has already been added`

**Cause**: OneStream converts filters to Dictionary (no duplicates allowed)

### Undefined Members in DBCL

```vbnet
' DestinationInfo blank, no source inheritance
Dim destInfo As New DataBufferDestinationInfo("")
Dim resultCell As New DataBufferCell()  ' All dimensions default to None
```

Result: Data written to None Members (not desired)

**Solution**: Use DestinationInfo or explicitly set result cell dimensions

### Unresolved Members Error

```vbnet
Dim resultCell As New DataBufferCell(sourceCell)
' Account inherited as XFCommon - unresolved!
```

**Error**: `Common Members were unresolved in the Result Data Buffer`

**Solution**: Explicitly set Account in DestinationInfo or result cell

### Invalid Destination Script

```vbnet
' WRONG - Data Unit dimensions in destination
api.Data.Calculate("A#Sales:E#ACME:T#2022M1 = A#Price * A#Volume")
```

**Error**: `Invalid destination script`

**Solution**: Use If statements for Data Unit filtering:

```vbnet
If api.Entity.Name = "ACME" AndAlso api.Pov.Time.Name = "2022M1" Then
    api.Data.Calculate("A#Sales = A#Price * A#Volume")
End If
```

### Object Not Set to Instance

```vbnet
Dim ud1DimPk As DimPk  ' Declared but not set!
Dim members = api.Members.GetMembersUsingFilter(ud1DimPk, "U1#Top.Base", Nothing)
```

**Error**: `Object reference not set to an instance of an object`

**Solution**: Initialize the variable:

```vbnet
Dim ud1DimPk As DimPk = api.Pov.UD1.DimPk
```

> **Tip**: Pay attention to compilation warnings about unset variables!

### Given Key Not Present in Dictionary

When Custom Calculate parameters not defined:

```vbnet
Dim productParam As String = args.NameValuePairs.Item("ProductSelection")
```

**Error**: `The given key was not present in the dictionary`

**Solution**: Use XFGetValue with default:

```vbnet
Dim productParam As String = args.NameValuePairs.XFGetValue("ProductSelection", "")

If productParam <> "" Then
    api.Data.Calculate("A#Sales:U1#" & productParam & " = ...")
End If
```

And define parameter in Data Management Step.

---

## Performance Overview

### What Affects Performance

1. **Hardware and Server Settings**
2. **Cube Design**
3. **Formula Efficiency**

### Multi-Threading Concept

OneStream multi-threads sibling Data Units during Consolidation:

- All Base Entities can theoretically process simultaneously
- Consolidations reach ~90% quickly, then last 10% drags
- Percentage based on number of Data Units processed

> Understanding multi-threading is critical for performance optimization.

---

## Hardware and Server Settings

### Server Structure

| Server Type | Purpose | Threading |
|-------------|---------|-----------|
| General Application | User navigation, Cube Views, Dashboards | Single-threaded |
| Stage Application | Stage Engine (Load, Transform, Journals) | Multi-threaded |
| Consolidation Application | Finance Engine (Consolidate, Calculate) | Multi-threaded |
| Data Management | Data Management Sequences | Multi-threaded |

### Server Designation

Direct tasks to specific servers via Data Management Sequences for long-running tasks.

### CPU Specs

- **3.7 GHz** chips perform up to **2x faster** than 2.0 GHz chips
- Higher clock speeds = faster execution
- Faster processors help Parent Calculations (limited parallelism)

### Multi-Threading Settings

Configuration in Application Server Configuration File:

```
System > Environment > Server Configuration
```

Refer to OneStream Foundation Handbook for tuning details.

---

## Cube Design for Performance

### Data Unit Size and Volume

Larger Data Units = more records = longer processing time

**Reduce Data Unit Size By**:

1. Utilize Extensibility
2. Increase number of Entities
3. Don't store unnecessary data
4. Reduce data sparsity
5. Limit Dimensions

### Utilize Extensibility

Collapse detail at Parent Entities to reduce redundancy:

- Manufacturing Entity uses Manufacturing Cost Centers
- Service Entity uses Service Cost Centers
- Parent collapses both to summary Member

### Increase Entity Count

```
One Entity (all data) = Single large Data Unit
                      = No multi-threading
                      = Memory overload

Multiple Entities     = Data spread across Data Units
                      = Better multi-threading
                      = Reduced memory per Calculation
```

### Optimize Entity Hierarchies

**Avoid**:
- Flat structures with many children under one Parent
- One-to-one Entity to Parent relationships

### Don't Store Unnecessary Data

Cubes are not designed for:
- Individual employee data
- Project details
- Asset registers

Use Stage or other OneStream tools instead.

### Use C#Aggregated When Possible

Up to **90% faster** than normal Consolidation for certain processes:

```vbnet
' Use C#Aggregated instead of C#Local
' Consolidation Filter: C#Aggregated
' Calculation Type: Consolidate
```

**C#Aggregated does**:
- Currency Translation
- Share percentages
- Straight aggregation

**C#Aggregated does NOT**:
- Intercompany eliminations
- Parent Journal adjustments
- Other Consolidation Members

### Don't Force Consolidate If Unnecessary

- Regular Consolidate checks Calculation Status
- Force Consolidate processes ALL Data Units
- Only use Force when metadata significantly changed

---

## Formula Efficiency Tips

### Core Principles

1. **Don't repeat yourself** - Avoid redundancy
2. **Use performance-friendly techniques**
3. **Eliminate unnecessary data processing**

---

## Things to Do

### Use Custom Calculate When Possible

| Calculate (DUCS) | Custom Calculate |
|------------------|------------------|
| All-or-nothing | Targeted scope |
| Full DUCS execution | Outside DUCS |
| Entity-level granularity | User-driven parameters |

**Example**: Department managers calculate only their department, not entire Entity.

### Align Entity Dimensions with Calculations

Consider different Entity Dimensions for different Scenario Types if calculations don't align.

### Balance Dynamic vs Stored Calculations

| Dynamic | Stored |
|---------|--------|
| No Calculation time impact | Adds to Calculation time |
| Increases Report render time | Reports render faster |
| Good for simple logic | Good for complex logic |

### Use RemoveZeros on All Data Buffers

**Standard Practice**:

```vbnet
api.Data.Calculate("A#Sales = RemoveZeros(A#Price * A#Volume)")
```

### Limit Data Unit Scope

```vbnet
If api.Pov.Cons.Name = "Local" AndAlso api.Entity.EntityType = ScenarioEntityType.Base Then
    api.Data.Calculate("A#Sales = A#Price * A#Volume")
End If
```

### Limit Account-Level Dimension Scope

Use filters to reduce cells written:

```vbnet
api.Data.Calculate("A#Sales = A#Price * A#Volume", _
    "", "", "", "", "U1#Products.Base", ...)
```

### Use Global Variables

For values that don't change across Data Units:

```vbnet
' Check if already exists
If globals.GetObject("SpreadTable") Is Nothing Then
    Dim spreadTable As DataTable = GetSpreadData()
    globals.SetObject("SpreadTable", spreadTable)
End If

Dim cachedTable As DataTable = CType(globals.GetObject("SpreadTable"), DataTable)
```

**Key Functions**:
- `GetObject` / `SetObject` - Objects
- `GetStringValue` / `SetStringValue` - Strings
- `CType` - Type conversion

### Use Formula Variables

Reuse Data Buffers across multiple ADC functions:

```vbnet
' Declare Data Buffer
Dim priceBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula( _
    "RemoveZeros(A#Price:O#Top:F#Top)")

' Set as formula variable
api.Data.FormulaVariable.SetDataBufferVariable(priceBuffer, "PriceBuffer", True)

' Use in multiple ADC functions
api.Data.Calculate("A#Sales = $PriceBuffer * A#Volume")
api.Data.Calculate("A#GrossProfit = $PriceBuffer * A#Margin")
```

**Last parameter** `True` = Uses indexes to optimize repeat filtering

### Use DimConstants

**Better Performance and Less Error-Prone**:

```vbnet
' WRONG - String comparison
If api.Pov.Cons.Name = "Elimination" Then

' CORRECT - DimConstants
If api.Pov.Cons.MemberId = DimConstants.Cons.Elimination Then
```

DimConstants compares integers (faster) and avoids hidden character issues.

---

## Quick Reference: Troubleshooting Checklist

### When Calculation Produces No Results

1. ☐ Check Cube View for source data existence
2. ☐ Log Data Buffers to compare dimensionality
3. ☐ Verify Common Members align between buffers
4. ☐ Check Formula Pass dependencies

### When Calculation Errors

1. ☐ Check Task Activity for error details
2. ☐ Compile rule to catch syntax errors
3. ☐ Log variables before error point
4. ☐ Check for unset objects (Nothing)
5. ☐ Verify Member names in formula strings

### When Calculation is Slow

1. ☐ Run Calculate With Logging for drill down
2. ☐ Use Stopwatch to time code sections
3. ☐ Check Data Unit size (CreateDataUnitCache)
4. ☐ Verify RemoveZeros on all Data Buffers
5. ☐ Check for unnecessary Force Consolidate
6. ☐ Consider Custom Calculate for targeted scope

---

## Summary

### Troubleshooting Tools

| Tool | When to Use |
|------|-------------|
| Task Activity | First check for errors |
| Error Log | Custom logging and details |
| LogDataBuffer | Data Buffer dimension issues |
| Stopwatch | Performance timing |
| Calculation Drill Down | DUCS step analysis |

### Performance Priorities

1. **Code Optimization** - Exponential improvement
2. **Cube Design** - Fundamental impact
3. **Hardware** - Linear improvement (last resort)

### Key Performance Techniques

- Use RemoveZeros everywhere
- Limit Data Unit and Dimension scope
- Use Global Variables for repeated data
- Use Formula Variables for repeated buffers
- Use DimConstants instead of strings
- Balance Dynamic vs Stored calculations
- Consider Custom Calculate for targeted execution

---

*This chapter content extracted from "OneStream Finance Rules and Calculations Handbook" by Jon Golembiewski (2022), pages 143-181.*
