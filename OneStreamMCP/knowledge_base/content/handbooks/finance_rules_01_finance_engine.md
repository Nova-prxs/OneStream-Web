# Finance Engine & Calculation Fundamentals

## Finance Engine Basics

### What is the Finance Engine?
The Finance Engine is OneStream's core calculation processing system that executes all financial calculations, consolidations, and data transformations within a cube.

### Data Unit Calculation Sequence (DUCS)

#### Definition
The DUCS is the ordered sequence in which calculations are executed for a Data Unit. A **Data Unit** is defined as all data cells within a unique combination of:
- Cube
- Scenario  
- Time
- Consolidation
- Entity

#### DUCS Execution Order
The DUCS executes calculations in a specific order:

1. **FormulaPass 1-16** (Consolidation Rules & Member Formulas)
   - Pass 1 executes first
   - Pass 16 executes last
   - Each pass runs completely before the next begins
   - Used to control calculation dependencies

2. **Consolidation Processing**
   - Entity hierarchy aggregation
   - Parent member calculation
   - Roll-up logic

3. **Currency Translation**
   - Converts amounts between currencies
   - Uses exchange rates from currency dimension
   - Applies translation rules

4. **Intercompany Eliminations**
   - Eliminates intercompany transactions
   - Balances intercompany accounts
   - Applies elimination rules

### Triggering the DUCS

#### Consolidation vs Calculation
- **Calculation**: Executes ONLY the DUCS for selected Data Unit
- **Consolidation**: Executes DUCS PLUS:
  - Aggregates and stores data at Parent Members in Entity hierarchy
  - Executes Currency Translations
  - Executes Intercompany Eliminations
  - Executes the DUCS

#### Where to Trigger
- Data Management
- Cube Views
- Dashboard Button
- Workflow Process Step

### Consolidation Rules

#### Location
Stored in: **Business Rules > Consolidation Rules**

#### Structure
```vb
' REQUIRED signature
Public Function Main(ByVal si As SessionInfo, _
                     ByVal globals As BRGlobals, _
                     ByVal api As Object, _
                     ByVal args As ConsolCalculateArgs) As ConsolCalcResult
```

#### Formula Pass Property
- Set on Consolidation Rule properties
- Determines when rule executes in DUCS
- Range: FormulaPass1 through FormulaPass16
- Lower passes execute first

#### Scope
Consolidation Rules execute at **Data Unit level**:
- Process entire Data Unit at once
- Access all Account, Flow, Origin, IC, UD1-8 combinations
- Most performant for bulk calculations

### Member Formulas

#### Location
Assigned to dimension members:
- Account members
- Flow members  
- UD members (UD1-UD8)

#### Formula Pass Property
- Set on member properties
- Must be FormulaPass1-16 to be included in DUCS
- Default "Conditional" means NOT included

#### Is Consolidated Property
CRITICAL setting for Member Formulas:
- **Conditional (Default)**: Data will NOT be consolidated
- **True**: Data WILL be consolidated
- **False**: Data explicitly not consolidated

Most use cases require this to be **True**

#### Scope
Member Formulas execute at **Cell level**:
- Process one cell at a time
- Limited to single member's data
- Less performant than Consolidation Rules for bulk operations

### Consolidation Rules vs Member Formulas

| Aspect | Consolidation Rules | Member Formulas |
|--------|-------------------|-----------------|
| **Execution Level** | Data Unit (bulk) | Cell by cell |
| **Performance** | Faster for bulk operations | Slower for many cells |
| **Code Location** | Business Rules | Member properties |
| **Complexity** | Can be very complex | Usually simpler |
| **Maintenance** | Centralized | Distributed across members |
| **Visibility** | Requires opening rule | Visible in member properties |
| **Best For** | Complex logic, bulk operations | Simple formulas, specific members |

### Formula Pass Strategy

#### Common Patterns

**Sequential Dependencies**
```
Pass 1: Calculate base amounts
Pass 2: Calculate percentages from Pass 1 results
Pass 3: Calculate variances from Pass 2 results
```

**Parallel Processing**
```
Pass 1: Group A calculations (independent)
Pass 1: Group B calculations (independent)  
Pass 2: Calculations depending on both A and B
```

#### Best Practices
- Use lower passes (1-3) for most calculations
- Reserve higher passes (14-16) for final adjustments
- Document pass usage in Calculation Matrix
- Keep dependencies clear and minimal

### Data Buffer Overview

#### CurrentDataBuffer
- Contains existing data in the cube
- Read-only in Consolidation Rules
- Source for calculations

#### ResultDataBuffer  
- Receives calculated values
- Written by Consolidation Rules
- Stored back to cube after DUCS

See dedicated Data Buffer documentation for detailed usage.

### Storage Types

All calculated data receives a Storage Type:

- **Input**: Manually entered data
- **Journal**: Journal entry data
- **Calculation**: From Consolidation Rules
- **DurableCalculation**: Cached calculation results
- **Consolidation**: From entity aggregation
- **Translation**: From currency translation
- **StoredButNoActivity**: Derived data placeholder

### Cell Status

Each cell has multiple status properties:
- **IsData**: Has any value (including zero)
- **IsNoData**: No value stored
- **IsRealData**: Actually stored value
- **IsDerivedData**: Calculated or system-generated
- **Storage Type**: How data was created

### Performance Considerations

#### Consolidation Rule Advantages
- Process entire Data Unit in one operation
- Minimize database round trips
- Use Data Buffers for bulk processing
- Optimal for complex cross-account logic

#### Member Formula Advantages  
- Simple to create and maintain
- No VB.NET coding required
- Good for simple single-member calculations
- Easy to audit and understand

#### When to Use What
- **Use Consolidation Rules when**:
  - Processing many accounts/members
  - Complex multi-step logic
  - Cross-dimensional calculations
  - Performance is critical
  
- **Use Member Formulas when**:
  - Simple arithmetic on single member
  - One-off calculations
  - Non-technical users need to maintain
  - Calculation is member-specific

## Key Takeaways

1. DUCS controls calculation execution order through Formula Passes
2. Consolidation Rules operate at Data Unit level (fast, bulk)
3. Member Formulas operate at Cell level (simple, specific)
4. FormulaPass property determines execution sequence (1-16)
5. "Is Consolidated" property critical for Member Formulas
6. Consolidation = DUCS + Aggregation + Translation + Eliminations
7. Calculation = DUCS only
8. Data Buffers are the primary mechanism for bulk calculations
9. Choose calculation location based on complexity and performance needs
10. Document pass usage and dependencies clearly

## Related Topics
- Data Buffers (see finance_rules_02_data_buffers.md)
- Calculation Management (see finance_rules_03_calculation_management.md)
- Common Rule Examples (see finance_rules_04_rule_examples.md)
- Debugging (see finance_rules_05_debugging.md)
