# Chapter 8: Common Rule Examples

> **Source**: OneStream Finance Rules and Calculations Handbook by Jon Golembiewski (2022)
> **Pages**: 192-245+ | **Practical Examples**: Real-World Calculation Patterns

## Overview

This chapter covers real-world use cases for Calculations that you are likely to encounter in a typical OneStream implementation. These examples provide context around the concepts discussed in previous chapters. No two situations are the same—these patterns should be adapted to fit customer-specific requirements.

---

## Table of Contents

1. [Things to Avoid](#things-to-avoid)
2. [Balance Sheet and Flow Calculations](#balance-sheet-and-flow-calculations)
3. [Consolidation Calculations](#consolidation-calculations)
4. [Seeding Rules](#seeding-rules)
5. [Allocation Calculations](#allocation-calculations)
6. [Budget and Forecast Calculations](#budget-and-forecast-calculations)
7. [Variance Analysis Calculations](#variance-analysis-calculations)

---

## Things to Avoid

### Unnecessary Calculations in the Cube

Transactional data should NOT go into Cubes:
- Bloats Data Units
- Slows Calculations and Consolidations

**Better Options**:
- Specialty Planning MarketPlace solutions
- BI Blend for simple arithmetic
- Stage tables with summarized imports

### Copying Data in the DUCS

**Wrong Approach**:
```vbnet
' In Target Scenario formula - runs every Calculation
api.Data.Calculate("S#Fcst_M1 = S#Actuals:T#POVPrior12")
```

**Better Approach**:
```vbnet
' Custom Calculate with isDurable = True - runs once
api.Data.Calculate("S#Fcst_M1 = S#Actuals:T#POVPrior12", ..., True)
```

### ADC Inside a Loop

**Wrong**:
```vbnet
For Each member In memberList
    If member.Name = "Product1" Then
        api.Data.Calculate("A#Sales:U1#Product1 = ...")
    ElseIf member.Name = "Product2" Then
        api.Data.Calculate("A#Sales:U1#Product2 = ...")
    End If
Next
```

**Correct** - Use filters instead:
```vbnet
api.Data.Calculate("A#Sales = A#Price * A#Volume", _
    "", "", "", "", "U1#Products.Base", ...)
```

### SetCell/SetDataCell Inside Loop

**Wrong** (brick-by-brick):
```vbnet
For Each sourceCell In buffer.Cells
    Dim resultCell As New DataBufferCell(sourceCell)
    ' Processing...
    api.Data.SetDataCell(resultCell)  ' Writing inside loop!
Next
```

**Correct** (wheelbarrow method):
```vbnet
Dim resultDataBuffer As New DataBuffer

For Each sourceCell In buffer.Cells
    Dim resultCell As New DataBufferCell(sourceCell)
    ' Processing...
    resultDataBuffer.SetCell(resultCell, False)  ' Add to buffer
Next

api.Data.SetDataBuffer(resultDataBuffer, destInfo)  ' Single write!
```

### Clear Data Inside Loop

**Wrong**:
```vbnet
For Each item In items
    api.Data.ClearCalculatedData(...)  ' Clear inside loop!
    api.Data.Calculate(...)
Next
```

**Correct**:
```vbnet
api.Data.ClearCalculatedData(...)  ' Clear BEFORE loop

For Each item In items
    ' Processing...
Next
```

### Lookup Constants Inside Loop

**Wrong**:
```vbnet
For Each sourceCell In buffer.Cells
    resultCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account, "Sales")  ' Redundant!
Next
```

**Correct**:
```vbnet
Dim salesAcctId As Integer = api.Members.GetMemberId(DimType.Account, "Sales")  ' Before loop

For Each sourceCell In buffer.Cells
    resultCell.DataBufferCellPk.AccountId = salesAcctId
Next
```

### Using BRAPI Calls in Finance Rules

**Avoid when possible**:
```vbnet
' Opens new database connection - causes issues with multi-threading
BRApi.Finance.Members.GetMemberName(...)
```

**Prefer API equivalent**:
```vbnet
' More efficient
api.Members.GetMemberName(...)
```

### Hardcoding Time Periods

**Wrong**:
```vbnet
api.Data.Calculate("A#ForecastCost = A#ActualCost:T#2021M12 * A#InflationRate")
```

**Correct** - Use time functions:
```vbnet
api.Data.Calculate("A#ForecastCost = A#ActualCost:T#POVLastPeriodInYear * A#InflationRate")
```

**Wrong**:
```vbnet
If api.Time.PeriodNumber = 1 Then  ' Won't work for quarterly scenarios
```

**Correct**:
```vbnet
If api.Time.IsFirstPeriodInYear Then
```

### Forgetting to Comment Out Logging

> Your author has crashed a server or two for committing this offense!

Always comment out logging before production deployment.

---

## Balance Sheet and Flow Calculations

### Current Year Net Income (CurYearNetIncome)

Pulls YTD Net Income from Income Statement into Balance Sheet equity.

```vbnet
If api.Entity.EntityType = ScenarioEntityType.Base OrElse _
   api.Entity.HasAutoTranslationCurrency Then

    api.Data.Calculate( _
        "A#CurYearNetIncome:F#EndBal:O#Import:I#None:U1#None:U2#None = " & _
        "RemoveZeros(A#NetIncome:F#Top:O#Top:I#Top:U1#Top:U2#Top)", _
        "A#CurYearNetIncome", "", "", "", "", "", "", "", "", "", "", "")
End If
```

**Key Properties**:
- Formula Pass: FormulaPass2 (after Income Statement calculations)
- Is Consolidated: True
- Allow Input: False

### Retained Earnings Beginning Balance

```vbnet
If api.Entity.EntityType = ScenarioEntityType.Base OrElse _
   api.Entity.HasAutoTranslationCurrency Then

    Dim priorYearLastPeriod As String = api.Scenario.GetPriorYearsLastPeriodName(api.Pov.Time.MemberId)
    
    If api.Time.IsFirstPeriodInYear Then
        ' Pull from prior year
        api.Data.Calculate( _
            "A#REBegBal:F#EndBal:O#Import:I#None = " & _
            "RemoveZeros(A#RE:F#EndBal:O#Top:I#Top:T#" & priorYearLastPeriod & ")", _
            "A#REBegBal", "", "", "", "", "", "", "", "", "", "", "")
    Else
        ' Carry forward from prior period
        api.Data.Calculate( _
            "A#REBegBal:F#EndBal:O#Import:I#None = " & _
            "RemoveZeros(A#REBegBal:F#EndBal:O#Top:I#Top:T#POVPrior1)", _
            "A#REBegBal", "", "", "", "", "", "", "", "", "", "", "")
    End If
End If
```

### Flow Dimension Setup

Four main components:
- Beginning Balance
- Activity
- FX
- Ending Balance

**Aggregation Weight** = 0 for Beginning Balance and Activity (only Ending Balance aggregates to Top).

### Beginning Balance (BegBalCalcYTD)

```vbnet
If api.Entity.EntityType = ScenarioEntityType.Base OrElse _
   api.Entity.HasAutoTranslationCurrency Then

    Dim priorYearLastPeriod As String = api.Scenario.GetPriorYearsLastPeriodName(api.Pov.Time.MemberId)
    
    If api.Time.IsFirstPeriodInYear Then
        api.Data.Calculate( _
            "F#BegBalCalcYTD:O#Import:I#None = " & _
            "RemoveZeros(F#EndBal:O#Top:I#Top:T#" & priorYearLastPeriod & ")", _
            "A#BalanceSheet.Base", "", "", "", "", "", "", "", "", "", "", "")
    Else
        api.Data.Calculate( _
            "F#BegBalCalcYTD:O#Import:I#None = " & _
            "RemoveZeros(F#BegBalCalcYTD:O#Top:I#Top:T#POVPrior1)", _
            "A#BalanceSheet.Base", "", "", "", "", "", "", "", "", "", "", "")
    End If
End If
```

### Dynamic Beginning Balance

For correct display across MTD, QTD, YTD views:

```vbnet
' Member Properties: Formula Type = DynamicCalc, Account Type = DynamicCalc
' Formula:
api.Data.GetDataCell("F#EndBal:T#POVViewPrior1").CellAmount
```

### Activity Calculation

```vbnet
api.Data.Calculate( _
    "F#ActivityCalc:O#Import:I#None = " & _
    "RemoveZeros(F#EndBal:O#Top:I#Top - F#BegBal:O#Top:I#Top)", _
    "A#BalanceSheet.Base.Where(Text1 <> CTA)", "", "", "", "", "", "", "", "", "", "", "")
```

**Note**: ActivityCalc has Switch Type = True for proper sign behavior.

### FX Calculations

Only run for foreign currency Entities:

```vbnet
If api.Entity.CurrencyIdLC <> api.Entity.CurrencyIdRC Then
    ' FX calculations here
End If
```

**FXOpen** - Effect of rate changes on opening balance:
```vbnet
Dim currentRate As Decimal = api.FxRates.GetCalculatedFxRates(...).ClosingRate
Dim localCurrencyBegBal As Decimal = api.Data.GetDataCell("F#BegBalCalcYTD:C#Local").CellAmount

Dim fxOpenAmount As Decimal = (localCurrencyBegBal * currentRate) - _
    api.Data.GetDataCell("F#BegBalCalcYTD").CellAmount
```

**FXMovement** - Effect on activity:
- FX on current movement
- FX on prior movement
- FX on override movement

### CTA (Cumulative Translation Adjustment)

```vbnet
' Sum FX for each Balance Sheet Account
api.Data.Calculate( _
    "A#CTA:F#Activity.Base = " & _
    "RemoveZeros(A#BalanceSheet.Base:F#Activity.Base)", _
    "A#CTA", "F#Activity.Base", "", "", "", "", "", "", "", "", "", "")

' Sum BegBal and Activity for EndBal
api.Data.Calculate( _
    "A#CTA:F#EndBalLoad = " & _
    "RemoveZeros(A#CTA:F#BegBal + A#CTA:F#Activity)", _
    "A#CTA", "", "", "", "", "", "", "", "", "", "", "")
```

---

## Consolidation Calculations

### Equity Pickup (EPU)

For investments with 20-50% ownership (significant influence, but not control).

**Entity Setup**:
- Percent Ownership: 30% (varying by period)
- Percent Consolidation: 0%
- Ownership Type: Equity
- Sibling Consolidation Pass: Higher than subsidiaries

**Account Setup**:
- Investment Account: Text1 = "Investment"
- IC Detail enabled for investment Account

**Calculation Abstract**:
```vbnet
If api.Pov.Cons.MemberId = DimConstants.Cons.Elimination Then
    If api.Entity.GetRelationshipInfo.OwnershipType = OwnershipType.Holding Then
        Book_EPU_InvElim()
    End If
End If
```

**Post EPU Entry Pattern**:
```vbnet
' Get subsidiary Net Income
Dim subsidiaryNetIncome As Decimal = api.Functions.GetEntityAggregationDataCell( _
    icEntityName, "A#NetIncome:F#EndBal").CellAmount

' Calculate EPU amount
Dim epuAmount As Decimal = subsidiaryNetIncome * percentOwnership

' Debit: Investment in Subsidiary
' Credit: Equity in Earnings from Subsidiary
```

### Noncontrolling Interest (NCI)

For majority ownership (>50% but <100%).

**Entity Setup**:
- Percent Ownership: 70%
- Percent Consolidation: 100%
- Ownership Type: NonControllingInterest

**Key Entries**:
1. Eliminate 100% of subsidiary equity
2. Reclassify minority portion (30%) to NCI Equity section
3. Record NCI Expense on Income Statement
4. Offset to Retained Earnings

```vbnet
' Eliminate equity
' Debit: Equity Account (full amount)
' Credit: InvestmentInSubsPlug (clearing account)

' Reclassify to NCI
' Debit: InvestmentInSubsPlug
' Credit: NCI Equity (minority % × equity balance)

' NCI Expense entry
Dim nciExpense As Decimal = netIncome * (1 - percentOwnership)
' Debit: NCI Expense
' Credit: NCI Equity (current year)
```

---

## Seeding Rules

### Rule Type Selection

| Scenario | Recommended Type |
|----------|-----------------|
| One-time copy | Custom Calculate |
| Frequently changing source | Scenario Formula (DUCS) |

### Simple Copy Example

```vbnet
api.Data.Calculate("S#Fcst_M1 = RemoveZeros(S#Budget)")
```

### Forecast Seeding with Time Logic

```vbnet
' Method 1: Parse Scenario name
Dim scenarioName As String = api.Pov.Scenario.Name
Dim monthSuffix As String = Mid(scenarioName, InStr(scenarioName, "_M") + 2)
Dim seedMonths As Integer = CInt(monthSuffix)

If api.Time.PeriodNumber <= seedMonths Then
    api.Data.Calculate("A#Top = RemoveZeros(S#Actuals:A#Top)")
End If
```

```vbnet
' Method 2: Use No Input Periods property
Dim noInputPeriods As Integer = api.Scenario.GetWorkflowNumNoInputTimePeriods(api.Pov.Time.MemberId)

If api.Time.PeriodNumber <= noInputPeriods Then
    api.Data.Calculate("A#Top = RemoveZeros(S#Actuals:A#Top)")
End If
```

### Convert Extended Members

When seeding between Scenarios with different Extensibility:

```vbnet
Dim sourceBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("S#Actuals:A#Top")

' Convert to target Scenario dimensionality
Dim convertedBuffer As DataBuffer = api.Data.ConvertDataBufferExtendedMembers( _
    sourceBuffer, api.Pov.Scenario.MemberId)

' Use as formula variable
api.Data.FormulaVariable.SetDataBufferVariable(convertedBuffer, "ConvertedActuals", True)

api.Data.Calculate("A#Top = $ConvertedActuals")
```

---

## Allocation Calculations

### Using Unbalanced Functions

Allocate total SG&A based on Actual distribution:

```vbnet
' PreAllocatedSGA (single amount) allocated across SGA Accounts
api.Data.Calculate( _
    "A#SGA.Base = " & _
    "MultiplyUnbalanced(A#PreAllocatedSGA, " & _
        "DivideUnbalanced(A#SGA.Base:S#Actuals:T#POVPriorYearM12, " & _
            "A#SGA:S#Actuals:T#POVPriorYearM12))", _
    "A#SGA.Base", "", "", "", "", "", "", "", "", "", "", "")
```

**Result**: PreAllocatedSGA × (Individual Account / Total) = Allocated Amount

### Using Data Buffer Cell Loop

```vbnet
' Get allocation basis
Dim actualSGABuffer As DataBuffer = api.Data.GetDataBufferUsingFormula( _
    "RemoveZeros(A#SGA.Base:S#Actuals:T#POVPriorYearM12)")

If actualSGABuffer.Cells.Count = 0 Then Exit Sub

Dim resultDataBuffer As New DataBuffer
Dim destInfo As New DataBufferDestinationInfo("F#EndBal:O#Import:I#None")

' Get totals
Dim totalSGA As Decimal = api.Data.GetDataCell("A#SGA:S#Actuals:T#POVPriorYearM12").CellAmount
Dim preAllocatedAmount As Decimal = api.Data.GetDataCell("A#PreAllocatedSGA").CellAmount

' Loop and allocate
For Each sourceCell As DataBufferCell In actualSGABuffer.Cells
    Dim resultCell As New DataBufferCell(sourceCell)
    Dim sourceCellAmount As Decimal = sourceCell.CellAmount
    
    If totalSGA <> 0 Then
        resultCell.CellAmount = (sourceCellAmount / totalSGA) * preAllocatedAmount
    Else
        resultCell.CellAmount = 0
    End If
    
    resultDataBuffer.SetCell(resultCell, False)
Next

api.Data.SetDataBuffer(resultDataBuffer, destInfo, ..., True)
```

### Allocations Across Entities

Use BRAPI to execute Custom Calculate for other Entities:

```vbnet
' In central Entity calculation
Dim entityList As List(Of MemberInfo) = api.Members.GetMembersUsingFilter( _
    api.Entity.DimPk, "E#ACMEGroup.Base", Nothing)

For Each entity As MemberInfo In entityList
    If entity.Member.Name <> "ACME" Then  ' Skip source Entity
        
        Dim nvPairs As New Dictionary(Of String, String)
        nvPairs.Add("Entity", entity.Member.Name)
        nvPairs.Add("Time", api.Pov.Time.Name)
        nvPairs.Add("Scenario", api.Pov.Scenario.Name)
        nvPairs.Add("Cons", "Local")
        
        BRApi.Finance.Calculate.ExecuteCustomCalculateBusinessRule( _
            si, _
            "AllocateRent",  ' Rule name
            "Main",          ' Function name
            nvPairs, _
            CustomCalculateTimeType.CurrentPeriod)
    End If
Next

' Clear source data after allocation
api.Data.ClearCalculatedData(True, True, True, "A#Rent", ...)
```

---

## Budget and Forecast Calculations

Common calculation types:

| Type | Description |
|------|-------------|
| Driver-Based | Price × Quantity = Revenue |
| Factor-Based | Actual × Growth Rate = Forecast |
| Seeding | Copy Actuals into Forecast |
| Allocations | Distribute totals to detail |

These patterns use techniques from all previous chapters.

---

## Variance Analysis Calculations

### Simple Variance (Dynamic)

```vbnet
' Formula Type = DynamicCalc
' Flow Member: SimpleVariance_PY

api.Data.GetDataCell("F#EndBal").BWDiff( _
    api.Data.GetDataCell("F#EndBal:T#POVPrior12").CellAmount)
```

**BWDiff** considers Account Type:
- Revenue decrease = Negative variance
- Expense decrease = Positive variance

### Detailed Variance Types

| Variance | Description |
|----------|-------------|
| Volume | Based on quantity driver changes |
| Price | Based on price driver changes |
| FX | Based on exchange rate changes |
| Mix | Plug between total and other variances |

### FX Rate Variance

Required: Transactional currency data by Flow Member.

```vbnet
' FX Variance = (Prior Amount × Current Rate) - (Prior Amount × Prior Rate)

For Each sourceCell As DataBufferCell In transactionBuffer.Cells
    Dim flowName As String = sourceCell.GetDimensionName(DimType.Flow)
    Dim currency As String = ParseCurrencyFromFlowName(flowName)
    
    Dim priorAmount As Decimal = sourceCell.CellAmount
    Dim currentRate As Decimal = GetCurrentRate(currency)
    Dim priorRate As Decimal = GetPriorRate(currency)
    
    Dim fxVariance As Decimal = (priorAmount * currentRate) - (priorAmount * priorRate)
    
    ' Create result cell...
Next
```

---

## Quick Reference: Common Patterns

### Standard DUCS Calculation Template

```vbnet
If api.Pov.Cons.Name = "Local" AndAlso _
   api.Entity.EntityType = ScenarioEntityType.Base Then

    api.Data.Calculate( _
        "A#Result:F#EndBal:O#Import:I#None = " & _
        "RemoveZeros(A#Source1:F#Top:O#Top:I#Top * A#Source2:F#Top:O#Top:I#Top)", _
        "A#Result", "", "", "", "U1#Products.Base", "", "", "", "", "", "", "")
End If
```

### Standard Custom Calculate Template

```vbnet
' Clear first
api.Data.ClearCalculatedData(True, True, True, _
    "A#Result:U1#Products.Base", "", "", "", "", "", "", "", "", "", "", "")

' Calculate with durable data
api.Data.Calculate( _
    "A#Result:F#EndBal:O#Import:I#None = " & _
    "RemoveZeros(A#Source1 * A#Source2)", _
    "", "", "", "", "U1#Products.Base", "", "", "", "", "", "", "", True)
```

### DBCL Template

```vbnet
Dim resultDataBuffer As New DataBuffer
Dim destInfo As New DataBufferDestinationInfo("A#Result:F#EndBal:O#Import:I#None")

Dim sourceBuffer As DataBuffer = api.Data.GetDataBufferUsingFormula("RemoveZeros(A#Source)")

If sourceBuffer.Cells.Count = 0 Then Exit Sub

For Each sourceCell As DataBufferCell In sourceBuffer.Cells
    Dim resultCell As New DataBufferCell(sourceCell)
    
    ' Set Account
    resultCell.DataBufferCellPk.AccountId = api.Members.GetMemberId(DimType.Account, "Result")
    
    ' Calculate
    resultCell.CellAmount = sourceCell.CellAmount * multiplier
    
    ' Add to buffer
    resultDataBuffer.SetCell(resultCell, False)
Next

api.Data.SetDataBuffer(resultDataBuffer, destInfo, ..., True)
```

---

## Summary

This chapter provided practical examples of:

1. **Things to Avoid** - Common anti-patterns that hurt performance
2. **Balance Sheet/Flow** - Period-to-period movement tracking
3. **Consolidation** - EPU and NCI accounting entries
4. **Seeding** - Copying data between Scenarios
5. **Allocations** - Distributing data across Dimensions
6. **Variance Analysis** - Understanding Account fluctuations

### Key Takeaways

- Always write to database **outside** loops
- Cache constants **before** loops
- Use filters instead of ADC inside loops
- Use Custom Calculate for one-time operations
- Leverage unbalanced functions for allocations
- BRAPI for cross-Entity calculations
- Dynamic calculations for simple variances

---

*This chapter content extracted from "OneStream Finance Rules and Calculations Handbook" by Jon Golembiewski (2022), pages 192-245+.*
