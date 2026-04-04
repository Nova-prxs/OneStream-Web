# Common Consolidation Rule Examples

## Overview

This document provides practical, real-world examples of common calculation patterns in OneStream Consolidation Rules.

## Example 1: Simple Arithmetic Calculations

### Gross Margin Calculation

**Business Requirement:**
Calculate Gross Margin = Revenue - COGS

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Get member IDs
    Dim revenueId As Integer = api.Members.GetMemberId(DimTypeId.Account, "Revenue").Id
    Dim cogsId As Integer = api.Members.GetMemberId(DimTypeId.Account, "COGS").Id
    Dim marginId As Integer = api.Members.GetMemberId(DimTypeId.Account, "GrossMargin").Id
    
    ' Get data buffers
    Dim revenueBuffer As DataBuffer = api.Data.GetDataBuffer("A#Revenue")
    Dim cogsBuffer As DataBuffer = api.Data.GetDataBuffer("A#COGS")
    Dim resultBuffer As New DataBuffer()
    
    ' Calculate margin for each cell
    For Each revCell As DataBufferCell In revenueBuffer.DataBufferCells.Values
        ' Find matching COGS cell
        Dim cogsCell As DataBufferCell = cogsBuffer.GetCell( _
            cogsId, revCell.CellKey.FlowId, revCell.CellKey.OriginId, _
            revCell.CellKey.ICId, revCell.CellKey.UD1Id, revCell.CellKey.UD2Id, _
            revCell.CellKey.UD3Id, revCell.CellKey.UD4Id, revCell.CellKey.UD5Id, _
            revCell.CellKey.UD6Id, revCell.CellKey.UD7Id, revCell.CellKey.UD8Id)
        
        ' Calculate margin
        Dim marginAmount As Decimal = revCell.CellAmount
        If cogsCell IsNot Nothing AndAlso cogsCell.IsData Then
            marginAmount -= cogsCell.CellAmount
        End If
        
        ' Write result
        resultBuffer.SetDataCell(marginId, revCell.CellKey.FlowId, _
            revCell.CellKey.OriginId, revCell.CellKey.ICId, _
            revCell.CellKey.UD1Id, revCell.CellKey.UD2Id, _
            revCell.CellKey.UD3Id, revCell.CellKey.UD4Id, _
            revCell.CellKey.UD5Id, revCell.CellKey.UD6Id, _
            revCell.CellKey.UD7Id, revCell.CellKey.UD8Id, marginAmount)
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

## Example 2: Percentage Calculations

### Margin Percentage

**Business Requirement:**
Calculate Margin % = Gross Margin / Revenue

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Get member IDs
    Dim revenueId As Integer = api.Members.GetMemberId(DimTypeId.Account, "Revenue").Id
    Dim marginId As Integer = api.Members.GetMemberId(DimTypeId.Account, "GrossMargin").Id
    Dim marginPctId As Integer = api.Members.GetMemberId(DimTypeId.Account, "MarginPercent").Id
    
    ' Get data
    Dim revenueBuffer As DataBuffer = api.Data.GetDataBuffer("A#Revenue")
    Dim marginBuffer As DataBuffer = api.Data.GetDataBuffer("A#GrossMargin")
    Dim resultBuffer As New DataBuffer()
    
    ' Calculate percentage
    For Each revCell As DataBufferCell In revenueBuffer.DataBufferCells.Values
        If revCell.CellAmount <> 0 Then  ' Avoid division by zero
            ' Find matching margin cell
            Dim marginCell As DataBufferCell = marginBuffer.GetCell( _
                marginId, revCell.CellKey.FlowId, revCell.CellKey.OriginId, _
                revCell.CellKey.ICId, revCell.CellKey.UD1Id, revCell.CellKey.UD2Id, _
                revCell.CellKey.UD3Id, revCell.CellKey.UD4Id, revCell.CellKey.UD5Id, _
                revCell.CellKey.UD6Id, revCell.CellKey.UD7Id, revCell.CellKey.UD8Id)
            
            If marginCell IsNot Nothing AndAlso marginCell.IsData Then
                ' Calculate percentage
                Dim percentage As Decimal = (marginCell.CellAmount / revCell.CellAmount) * 100
                
                ' Write result
                resultBuffer.SetDataCell(marginPctId, revCell.CellKey.FlowId, _
                    revCell.CellKey.OriginId, revCell.CellKey.ICId, _
                    revCell.CellKey.UD1Id, revCell.CellKey.UD2Id, _
                    revCell.CellKey.UD3Id, revCell.CellKey.UD4Id, _
                    revCell.CellKey.UD5Id, revCell.CellKey.UD6Id, _
                    revCell.CellKey.UD7Id, revCell.CellKey.UD8Id, percentage)
            End If
        End If
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

## Example 3: Currency Conversion

### Convert Local Currency to USD

**Business Requirement:**
Convert all local currency amounts to USD using exchange rates

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Get entity currency
    Dim entityId As Integer = args.DataUnitPk.EntityId
    Dim entity As Member = api.Members.GetMember(DimTypeId.Entity, entityId)
    Dim localCurrency As String = entity.GetCustomProperty("LocalCurrency")
    
    ' Get exchange rate from substitution variable or rate table
    Dim exchangeRate As Decimal = GetExchangeRate(api, localCurrency, "USD")
    
    ' Get source data
    Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer()
    Dim resultBuffer As New DataBuffer()
    
    ' Convert each cell
    For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
        ' Get account type to determine if rate is Average or Period End
        Dim account As Member = api.Members.GetMember(DimTypeId.Account, cell.CellKey.AccountId)
        Dim accountType As String = account.GetCustomProperty("AccountType")
        
        ' Apply appropriate exchange rate
        Dim rateToUse As Decimal = exchangeRate
        If accountType = "BalanceSheet" Then
            ' Use period-end rate for balance sheet
            rateToUse = GetExchangeRate(api, localCurrency, "USD", "PeriodEnd")
        Else
            ' Use average rate for P&L
            rateToUse = GetExchangeRate(api, localCurrency, "USD", "Average")
        End If
        
        ' Convert amount
        Dim convertedAmount As Decimal = cell.CellAmount * rateToUse
        
        ' Write to result buffer
        resultBuffer.SetDataCell(cell.CellKey, convertedAmount)
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function

Private Function GetExchangeRate(api As Object, fromCurrency As String, toCurrency As String, Optional rateType As String = "Average") As Decimal
    ' Retrieve exchange rate from rate table or substitution variable
    Dim svName As String = $"FX_{fromCurrency}_to_{toCurrency}_{rateType}"
    Dim rate As Decimal = api.SubVars.GetSubVarValue(svName)
    Return rate
End Function
```

## Example 4: Variance Calculations

### Actual vs Budget Variance

**Business Requirement:**
Calculate Actual vs Budget variance and variance percentage

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Get scenario IDs
    Dim actualScenario As Integer = api.Members.GetMemberId(DimTypeId.Scenario, "Actual").Id
    Dim budgetScenario As Integer = api.Members.GetMemberId(DimTypeId.Scenario, "Budget").Id
    
    ' Get flow IDs
    Dim periodicFlow As Integer = api.Members.GetMemberId(DimTypeId.Flow, "Periodic").Id
    Dim varianceFlow As Integer = api.Members.GetMemberId(DimTypeId.Flow, "ActualvsBudget").Id
    Dim variancePctFlow As Integer = api.Members.GetMemberId(DimTypeId.Flow, "ActualvsBudgetPct").Id
    
    ' Get actual data
    Dim actualBuffer As DataBuffer = api.Data.GetDataBuffer("S#Actual.F#Periodic")
    
    ' Create result buffer
    Dim resultBuffer As New DataBuffer()
    
    ' Calculate variance for each actual cell
    For Each actualCell As DataBufferCell In actualBuffer.DataBufferCells.Values
        ' Get corresponding budget value
        Dim budgetValue As Decimal = api.Data.GetDataCell( _
            "S#Budget" & _
            ".T#" & api.Members.GetMemberName(DimTypeId.Time, actualCell.CellKey.TimeId) & _
            ".E#" & api.Members.GetMemberName(DimTypeId.Entity, actualCell.CellKey.EntityId) & _
            ".A#" & api.Members.GetMemberName(DimTypeId.Account, actualCell.CellKey.AccountId) & _
            ".C#" & api.Members.GetMemberName(DimTypeId.Consol, actualCell.CellKey.ConsolId) & _
            ".F#Periodic" & _
            ".O#" & api.Members.GetMemberName(DimTypeId.Origin, actualCell.CellKey.OriginId) & _
            ".I#" & api.Members.GetMemberName(DimTypeId.IC, actualCell.CellKey.ICId))
        
        ' Calculate variance
        Dim variance As Decimal = actualCell.CellAmount - budgetValue
        
        ' Write variance
        resultBuffer.SetDataCell(actualCell.CellKey.AccountId, varianceFlow, _
            actualCell.CellKey.OriginId, actualCell.CellKey.ICId, _
            actualCell.CellKey.UD1Id, actualCell.CellKey.UD2Id, _
            actualCell.CellKey.UD3Id, actualCell.CellKey.UD4Id, _
            actualCell.CellKey.UD5Id, actualCell.CellKey.UD6Id, _
            actualCell.CellKey.UD7Id, actualCell.CellKey.UD8Id, variance)
        
        ' Calculate variance percentage
        If budgetValue <> 0 Then
            Dim variancePct As Decimal = (variance / budgetValue) * 100
            
            resultBuffer.SetDataCell(actualCell.CellKey.AccountId, variancePctFlow, _
                actualCell.CellKey.OriginId, actualCell.CellKey.ICId, _
                actualCell.CellKey.UD1Id, actualCell.CellKey.UD2Id, _
                actualCell.CellKey.UD3Id, actualCell.CellKey.UD4Id, _
                actualCell.CellKey.UD5Id, actualCell.CellKey.UD6Id, _
                actualCell.CellKey.UD7Id, actualCell.CellKey.UD8Id, variancePct)
        End If
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

## Example 5: Allocation

### Allocate Corporate Overhead to Entities

**Business Requirement:**
Allocate corporate overhead expenses to entities based on revenue proportion

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Get corporate overhead amount
    Dim overheadAmount As Decimal = api.Data.GetDataCell("E#Corporate.A#Overhead")
    
    ' Get total company revenue (for allocation basis)
    Dim totalRevenue As Decimal = api.Data.GetDataCell("E#Total_Company.A#Revenue")
    
    If totalRevenue = 0 Then
        Return New ConsolCalcResult(Nothing) ' No allocation if no revenue
    End If
    
    ' Get all entities (except Corporate)
    Dim entities As List(Of Member) = api.Members.GetChildren(DimTypeId.Entity, "Total_Company")
    
    Dim resultBuffer As New DataBuffer()
    Dim overheadAccountId As Integer = api.Members.GetMemberId(DimTypeId.Account, "AllocatedOverhead").Id
    Dim periodicFlowId As Integer = api.Members.GetMemberId(DimTypeId.Flow, "Periodic").Id
    Dim originId As Integer = api.Members.GetMemberId(DimTypeId.Origin, "Allocation").Id
    Dim icId As Integer = api.Members.GetMemberId(DimTypeId.IC, "None").Id
    
    ' Allocate to each entity based on its revenue
    For Each entity In entities
        If entity.Name <> "Corporate" Then ' Don't allocate back to Corporate
            ' Get entity revenue
            Dim entityRevenue As Decimal = api.Data.GetDataCell("E#" & entity.Name & ".A#Revenue")
            
            ' Calculate allocation
            Dim allocationPct As Decimal = entityRevenue / totalRevenue
            Dim allocatedAmount As Decimal = overheadAmount * allocationPct
            
            ' Write allocation
            resultBuffer.SetDataCell(overheadAccountId, periodicFlowId, originId, icId, _
                0, 0, 0, 0, 0, 0, 0, 0, allocatedAmount)
        End If
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

## Example 6: FX Rate Effect Analysis

### Calculate FX Rate Impact on Revenue

**Business Requirement:**
Calculate the impact of exchange rate changes on revenue (FX rate effect)

**Formula:**
FX Rate Effect = (Current Period Amount at Prior Period Rate) - (Current Period Amount at Current Period Rate)

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Get time periods
    Dim currentPeriod As String = api.Members.GetMemberName(DimTypeId.Time, args.DataUnitPk.TimeId)
    Dim priorPeriod As String = api.Time.GetPriorTimeId(args.DataUnitPk.TimeDimPk, 1)
    
    ' Get exchange rates
    Dim currentRate As Decimal = api.Data.GetDataCell("A#FX_Rate.T#" & currentPeriod)
    Dim priorRate As Decimal = api.Data.GetDataCell("A#FX_Rate.T#" & priorPeriod)
    
    If priorRate = 0 OrElse currentRate = 0 Then
        Return New ConsolCalcResult(Nothing) ' Can't calculate FX effect without rates
    End If
    
    ' Get current period local currency data
    Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer("F#Periodic.C#Local")
    Dim resultBuffer As New DataBuffer()
    
    ' Get FX effect flow member ID
    Dim fxEffectFlowId As Integer = api.Members.GetMemberId(DimTypeId.Flow, "FXRateEffect").Id
    
    ' Calculate FX effect for each account
    For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
        ' Amount at current rate (already stored)
        Dim amountAtCurrentRate As Decimal = cell.CellAmount * currentRate
        
        ' Amount at prior rate (for comparison)
        Dim amountAtPriorRate As Decimal = cell.CellAmount * priorRate
        
        ' FX Rate Effect = difference
        Dim fxEffect As Decimal = amountAtPriorRate - amountAtCurrentRate
        
        ' Write FX effect
        resultBuffer.SetDataCell(cell.CellKey.AccountId, fxEffectFlowId, _
            cell.CellKey.OriginId, cell.CellKey.ICId, _
            cell.CellKey.UD1Id, cell.CellKey.UD2Id, _
            cell.CellKey.UD3Id, cell.CellKey.UD4Id, _
            cell.CellKey.UD5Id, cell.CellKey.UD6Id, _
            cell.CellKey.UD7Id, cell.CellKey.UD8Id, fxEffect)
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

## Example 7: Conditional Calculations

### Tax Calculation with Rate Tiers

**Business Requirement:**
Calculate tax based on tiered rates:
- First $50,000: 10%
- Next $50,000: 15%
- Above $100,000: 20%

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Tax tiers
    Dim tier1Limit As Decimal = 50000
    Dim tier2Limit As Decimal = 100000
    Dim tier1Rate As Decimal = 0.10
    Dim tier2Rate As Decimal = 0.15
    Dim tier3Rate As Decimal = 0.20
    
    ' Get taxable income
    Dim taxableIncomeId As Integer = api.Members.GetMemberId(DimTypeId.Account, "TaxableIncome").Id
    Dim taxExpenseId As Integer = api.Members.GetMemberId(DimTypeId.Account, "TaxExpense").Id
    
    Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer("A#TaxableIncome")
    Dim resultBuffer As New DataBuffer()
    
    For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
        Dim income As Decimal = cell.CellAmount
        Dim tax As Decimal = 0
        
        ' Calculate tiered tax
        If income <= 0 Then
            tax = 0
        ElseIf income <= tier1Limit Then
            ' All in tier 1
            tax = income * tier1Rate
        ElseIf income <= tier2Limit Then
            ' Tier 1 + partial tier 2
            tax = (tier1Limit * tier1Rate) + ((income - tier1Limit) * tier2Rate)
        Else
            ' All three tiers
            tax = (tier1Limit * tier1Rate) + _
                  ((tier2Limit - tier1Limit) * tier2Rate) + _
                  ((income - tier2Limit) * tier3Rate)
        End If
        
        ' Write tax expense
        resultBuffer.SetDataCell(taxExpenseId, cell.CellKey.FlowId, _
            cell.CellKey.OriginId, cell.CellKey.ICId, _
            cell.CellKey.UD1Id, cell.CellKey.UD2Id, _
            cell.CellKey.UD3Id, cell.CellKey.UD4Id, _
            cell.CellKey.UD5Id, cell.CellKey.UD6Id, _
            cell.CellKey.UD7Id, cell.CellKey.UD8Id, tax)
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

## Example 8: YTD Accumulation

### Calculate Year-to-Date Values

**Business Requirement:**
Calculate YTD values from periodic data

**Implementation:**
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    ' Get current time period
    Dim currentTimePk As MemberId = args.DataUnitPk.TimeDimPk
    
    ' Get all periods in YTD range
    Dim ytdPeriods As List(Of MemberId) = api.Time.GetYTDRange(currentTimePk)
    
    ' Get periodic flow and YTD flow IDs
    Dim periodicFlowId As Integer = api.Members.GetMemberId(DimTypeId.Flow, "Periodic").Id
    Dim ytdFlowId As Integer = api.Members.GetMemberId(DimTypeId.Flow, "YTD").Id
    
    ' Get all accounts to process
    Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer("F#Periodic")
    
    ' Group cells by account and other dimensions (except time)
    Dim cellGroups As New Dictionary(Of String, List(Of DataBufferCell))
    
    For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
        Dim key As String = $"{cell.CellKey.AccountId}_{cell.CellKey.OriginId}_{cell.CellKey.ICId}"
        If Not cellGroups.ContainsKey(key) Then
            cellGroups(key) = New List(Of DataBufferCell)
        End If
        cellGroups(key).Add(cell)
    Next
    
    ' Calculate YTD for each group
    Dim resultBuffer As New DataBuffer()
    
    For Each group In cellGroups
        Dim ytdTotal As Decimal = 0
        
        ' Sum all periods in YTD range
        For Each period In ytdPeriods
            Dim periodName As String = period.Name
            Dim periodValue As Decimal = api.Data.GetDataCell( _
                "T#" & periodName & _
                ".A#" & api.Members.GetMemberName(DimTypeId.Account, group.Value(0).CellKey.AccountId) & _
                ".F#Periodic")
            
            ytdTotal += periodValue
        Next
        
        ' Write YTD total
        Dim firstCell As DataBufferCell = group.Value(0)
        resultBuffer.SetDataCell(firstCell.CellKey.AccountId, ytdFlowId, _
            firstCell.CellKey.OriginId, firstCell.CellKey.ICId, _
            firstCell.CellKey.UD1Id, firstCell.CellKey.UD2Id, _
            firstCell.CellKey.UD3Id, firstCell.CellKey.UD4Id, _
            firstCell.CellKey.UD5Id, firstCell.CellKey.UD6Id, _
            firstCell.CellKey.UD7Id, firstCell.CellKey.UD8Id, ytdTotal)
    Next
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

## Key Patterns Summary

### 1. **Cross-Account Calculations**
Read from multiple accounts, calculate, write to target account

### 2. **Currency Conversion**
Apply exchange rates to local currency amounts

### 3. **Variance Analysis**
Compare scenarios/periods, calculate differences and percentages

### 4. **Allocations**
Distribute amounts based on driver (revenue, headcount, etc.)

### 5. **FX Rate Effects**
Analyze impact of rate changes period-over-period

### 6. **Conditional Logic**
Apply different calculations based on conditions (tiers, types, thresholds)

### 7. **Time-Based Calculations**
YTD, QTD, rolling averages, period-over-period

### 8. **Aggregations**
Sum, average, or accumulate values across dimensions

## Best Practices from Examples

1. **Cache Member IDs** - Look up once, reuse in loop
2. **Check for zero** - Always handle division by zero
3. **Null checks** - Verify cells exist before using
4. **Use Data Buffers** - Never api.Data.GetDataCell in loops  
5. **Clear comments** - Document business logic
6. **Error handling** - Try/Catch around risky operations
7. **Logging** - Log key steps for debugging
8. **Modular code** - Break complex logic into functions

## Related Topics
- Finance Engine Basics (see finance_rules_01_finance_engine.md)
- Data Buffers (see finance_rules_02_data_buffers.md)
- Debugging (see finance_rules_05_debugging.md)
