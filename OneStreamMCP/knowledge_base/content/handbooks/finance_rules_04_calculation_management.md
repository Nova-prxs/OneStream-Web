# Calculation Management and Testing

## Calculation Matrix

The Calculation Matrix is a documentation tool to track, manage, and communicate calculation logic across the application.

### Purpose

- **Track** all calculations in one place
- **Design** calculation logic before building
- **Test** calculations systematically
- **Transfer knowledge** to team members
- **Maintain** calculations over time

### Components

A complete Calculation Matrix should include:

#### 1. Target Member
- Account/Flow/UD member receiving calculated value
- Example: "GrossMargin", "RevenueUSD", "TaxRate"

#### 2. Calculation Type
- **Stored**: Consolidation Rule (stored in cube)
- **Dynamic**: Member Formula or Dynamic Calc Rule (calculated on-demand)

#### 3. Finance Function Type
- **Arithmetic**: Simple math operations
- **Allocation**: Distribute values across members
- **Currency**: FX conversion
- **Variance**: Comparison calculations
- **Other**: Custom logic

#### 4. Calculation Location
- **Consolidation Rule**: Name of Business Rule
- **Member Formula**: Dimension member
- **Cube View**: Specific view name

#### 5. Execution Details
- **Formula Pass**: FormulaPass1-16 (for stored calculations)
- **Execution Timing**: When it runs
- **Dependencies**: Other calculations it relies on

#### 6. Formula/Logic
- Mathematical expression or pseudocode
- Example: "Revenue - COGS"
- VB.NET code snippet for complex logic

#### 7. Dependencies
- List of members/calculations that must execute first
- Example: "Requires Revenue and COGS to be calculated"

#### 8. Scope Information
- What dimensions does it apply to?
- Any filters or conditions?
- Example: "All Entities, Actual scenario only"

### Example Calculation Matrix

```
| Target      | Type   | Function | Location        | Pass | Formula              | Dependencies    |
|-------------|--------|----------|-----------------|------|----------------------|-----------------|
| GrossMargin | Stored | Arith    | CR_Financials   | 2    | Revenue - COGS       | Revenue, COGS   |
| MarginPct   | Dynamic| Arith    | Member Formula  | -    | GrossMargin/Revenue  | GrossMargin     |
| RevenueUSD  | Stored | Currency | CR_FXConversion | 3    | Revenue * FXRate     | Revenue, Rates  |
| TaxExpense  | Stored | Arith    | CR_Tax          | 5    | TaxableInc * TaxRate | TaxableIncome   |
```

### Benefits

1. **Tracking**: See all calculations at a glance
2. **Designing**: Plan before coding
3. **Building**: Clear specification to follow
4. **Testing**: Systematic verification checklist
5. **Approving**: Business users can review logic
6. **Knowledge Transfer**: Onboard new team members
7. **Maintenance**: Understand impact of changes

## Calculation Testing

### Testing Approach

1. **Unit Testing**: Test individual calculations
2. **Integration Testing**: Test calculation dependencies
3. **Regression Testing**: Verify changes don't break existing logic
4. **Performance Testing**: Ensure acceptable execution time

### Testing Tips

#### 1. Use Test Scenarios
Create dedicated Scenario for testing:
- S#Test_Calculations
- Load known input values
- Verify calculated outputs

#### 2. Test Edge Cases
```vb
' Test division by zero
Dim result = If(denominator <> 0, numerator / denominator, 0)

' Test negative values
If amount < 0 Then
    ' Handle negative case
End If

' Test missing data
If sourceCell Is Nothing OrElse Not sourceCell.IsData Then
    Return 0
End If
```

#### 3. Test Different POVs
- Multiple entities
- Different time periods
- Various scenarios
- All relevant dimension combinations

#### 4. Compare to Expected Results
```vb
' Known input
Revenue = 1000
COGS = 600

' Expected output
GrossMargin = 400 (Revenue - COGS)
MarginPct = 0.40 (GrossMargin / Revenue)

' Verify calculation produces expected values
```

#### 5. Test Performance
- Time execution of large Data Units
- Monitor memory usage
- Check for timeout issues
- Optimize slow calculations

### Testing Tools

#### 1. Cube View Testing
- Create test Cube View
- Display input and calculated members
- Visual verification of results
- Export to Excel for detailed analysis

#### 2. Calculate with Logging
Enable detailed logging:
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    api.LogMessage("=== Starting Calculation ===")
    api.LogMessage("Data Unit: " & args.DataUnitPk.ToString())
    
    ' Log input data
    Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer()
    api.LogMessage("Input cells: " & inputBuffer.DataBufferCells.Count.ToString())
    
    ' Perform calculation
    Dim resultBuffer As New DataBuffer()
    
    For Each cell As DataBufferCell In inputBuffer.DataBufferCells.Values
        api.LogMessage("Processing Account: " & _
            api.Members.GetMemberName(DimTypeId.Account, cell.CellKey.AccountId) & _
            " Amount: " & cell.CellAmount.ToString())
        
        ' Calculation logic
        Dim result As Decimal = cell.CellAmount * 1.1
        
        resultBuffer.SetDataCell(cell.CellKey, result)
        
        api.LogMessage("Result: " & result.ToString())
    Next
    
    api.LogMessage("=== Calculation Complete ===")
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

#### 3. Calculation Drill Down
- Right-click cell in Cube View
- Select "Calculation Drill Down"
- View step-by-step calculation
- See source data and formulas

#### 4. Formula Statistics
View calculation statistics:
- Number of cells calculated
- Execution time
- Formula complexity
- Performance metrics

## Calculation Reports

### Built-in Reports

#### 1. Formula List Report
Shows all Member Formulas:
- Member name
- Formula text
- Formula Pass
- Is Consolidated setting

#### 2. Business Rules Report
Lists all Consolidation and Dynamic Calc rules:
- Rule name
- Rule type
- Formula Pass
- Associated members

#### 3. Calculation Matrix Export
Export to Excel:
- All calculations documented
- Dependencies mapped
- Execution order shown

### Custom Reports

Create Cube Views to test calculations:
```
Columns:
  C#Input (source data)
  C#Calculated (result)
  C#Variance (difference)
  C#VariancePct (% difference)

Rows:
  A#Revenue
  A#COGS
  A#GrossMargin
  A#MarginPct
```

## Calculation Maintenance

### Comments

#### Header Comments
Document rule purpose and logic:
```vb
'******************************************************************************
' Rule Name: CR_FinancialCalculations
' Purpose: Calculate gross margin, operating income, and net income
' Author: John Doe
' Date: 2024-01-15
' Formula Pass: 2
' Dependencies: Revenue, COGS, OpEx must be loaded
' Notes: Executes after data load, before allocations
'******************************************************************************

Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
```

#### Inline Comments
Explain complex logic:
```vb
' Calculate gross margin (Revenue - COGS)
Dim grossMargin As Decimal = revenue - cogs

' Apply minimum threshold - never less than zero
If grossMargin < 0 Then
    grossMargin = 0
End If

' Calculate margin percentage
' Handle division by zero case
Dim marginPct As Decimal = If(revenue <> 0, grossMargin / revenue, 0)
```

### Code Regions
Organize complex rules:
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    #Region "Initialize"
    Dim inputBuffer As DataBuffer = api.Data.GetDataBuffer()
    Dim resultBuffer As New DataBuffer()
    Dim memberCache As New Dictionary(Of String, Integer)
    #End Region
    
    #Region "Calculate Revenue Metrics"
    ' Revenue calculations here
    #End Region
    
    #Region "Calculate Expense Metrics"
    ' Expense calculations here
    #End Region
    
    #Region "Calculate Margin Metrics"
    ' Margin calculations here
    #End Region
    
    Return New ConsolCalcResult(resultBuffer)
End Function
```

### Maintenance Tips

#### 1. Version Control
- Check rules into source control
- Document changes in comments
- Maintain change log

#### 2. Naming Conventions
```vb
' Consolidation Rules
CR_FinancialCalculations
CR_FXConversion
CR_Allocations

' Dynamic Calc Rules
DC_MarginCalculations
DC_VarianceAnalysis

' Member Formulas
Clear descriptive names in formula text
```

#### 3. Modular Design
Break complex rules into functions:
```vb
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
    
    Dim resultBuffer As New DataBuffer()
    
    ' Call helper functions
    CalculateRevenue(api, resultBuffer)
    CalculateExpenses(api, resultBuffer)
    CalculateMargins(api, resultBuffer)
    
    Return New ConsolCalcResult(resultBuffer)
End Function

Private Sub CalculateRevenue(api As Object, resultBuffer As DataBuffer)
    ' Revenue calculation logic
End Sub

Private Sub CalculateExpenses(api As Object, resultBuffer As DataBuffer)
    ' Expense calculation logic
End Sub

Private Sub CalculateMargins(api As Object, resultBuffer As DataBuffer)
    ' Margin calculation logic
End Sub
```

#### 4. Configuration Over Code
Use Member properties for flexibility:
```vb
' Read calculation factor from member property
Dim account As Member = api.Members.GetMember(DimTypeId.Account, accountName)
Dim factor As Decimal = Decimal.Parse(account.GetCustomProperty("CalcFactor"))

' Apply factor
Dim result As Decimal = inputValue * factor
```

#### 5. Error Handling
```vb
Try
    ' Calculation logic
    
Catch ex As XFException
    ' OneStream-specific error
    api.LogMessage("OneStream Error: " & ex.Message)
    Throw
    
Catch ex As Exception
    ' General error
    api.LogMessage("General Error: " & ex.Message)
    Throw New Exception("Calculation failed: " & ex.Message, ex)
End Try
```

## Time Functions in Calculations

### Common Time Functions

#### Prior Periods
```vb
' Prior 1 period
= A#Revenue.Prior1

' Prior 2 periods
= A#Revenue.Prior2
```

#### Year Functions
```vb
' Prior year same period
= A#Revenue.PriorYear1

' Prior 2 years
= A#Revenue.PriorYear2

' Year to date
= A#Revenue.YTD
```

#### Quarter Functions
```vb
' Quarter to date
= A#Revenue.QTD

' Specific quarter
= A#Revenue.Q1
```

#### In Business Rules
```vb
' Get prior month value
Dim priorMonth As String = api.Time.GetPriorTimeId(timeDimPk, 1)
Dim priorValue As Decimal = api.Data.GetDataCell("T#" & priorMonth & ".A#Revenue")

' Get YTD range
Dim ytdRange As List(Of MemberId) = api.Time.GetYTDRange(timeDimPk)
Dim ytdTotal As Decimal = 0

For Each timeMember In ytdRange
    ytdTotal += api.Data.GetDataCell("T#" & timeMember.Name & ".A#Revenue")
Next
```

## Substitution Variables in Calculations

### Usage
```vb
' Member Formula
= A#Actual * |!SV_ExchangeRate!|

' Business Rule
Dim exchangeRate As Decimal = api.SubVars.GetSubVarValue("SV_ExchangeRate")
Dim result As Decimal = actualValue * exchangeRate
```

### Common Use Cases
- Exchange rates
- Allocation percentages
- Threshold values
- Business parameters
- Environment-specific values

## Key Takeaways

1. Calculation Matrix is essential for documentation and management
2. Test calculations systematically with known inputs/outputs
3. Use logging for troubleshooting
4. Comment code thoroughly - headers and inline
5. Organize code with regions
6. Use modular design for complex rules
7. Version control calculation rules
8. Test edge cases (zero, negative, missing data)
9. Monitor performance
10. Maintain clear naming conventions

## Related Topics
- Finance Engine Basics (see finance_rules_01_finance_engine.md)
- Data Buffers (see finance_rules_02_data_buffers.md)
- Reporting Calculations (see finance_rules_03_reporting_calculations.md)
- Debugging (see finance_rules_05_debugging.md)
