# OneStream Performance and Reporting Best Practices

## Overview
Performance optimization and effective reporting design are critical for user adoption and system efficiency in OneStream.

## Cube View Design Best Practices

### The Five-Second Rule
> "If a Cube View takes longer than five seconds to render, it's a good idea to see if you can improve performance by moving some dimensions around, slightly redesigning it, or breaking it into a number of smaller Cube Views."

**Performance Threshold**: Cube Views should render in < 5 seconds

### Cube View Anchoring Strategies

Choose ONE consistent anchoring strategy across all Cube Views to reduce user confusion.

#### Option 1: Workflow Anchor (Recommended for Data Entry)
**How it works:**
- POV references workflow for Scenario, Time, and possibly Entity
- User doesn't need to manually select these dimensions
- System uses workflow context automatically

**Pros:**
- Simplified user experience
- Automatic context from workflow
- Best for data entry forms
- Users don't need to manage POV

**Cons:**
- Viewing past periods requires changing workflow
- Less flexible for ad-hoc reporting

**Best For:**
- All data input forms
- Workflow-driven processes
- Month-end close activities

#### Option 2: User POV Anchor
**How it works:**
- User opens POV pane to select dimensions
- Full control over what dimensions to view

**Pros:**
- Maximum flexibility
- Can view any time period without changing workflow
- Good for ad-hoc analysis

**Cons:**
- User must remember to check/update POV
- Can cause confusion if POV doesn't match workflow
- More complex for end users

**Best For:**
- Management reporting
- Ad-hoc analysis
- Cross-period comparisons

#### Option 3: Runtime Parameters (Recommended Combined Approach)
**How it works:**
- User prompted for dimensions at runtime
- Pop-up window for dimension selection

**Pros:**
- Clear user intention
- No hidden POV settings
- Works well with workflow anchor
- Intuitive for users

**Cons:**
- Extra click for users
- Can be tedious for frequently-run reports

**Best For:**
- Reports (not data entry)
- Cross-entity analysis
- Scheduled reports with variations

### Recommended Combined Strategy
```
✅ Workflow Anchor + Runtime Parameters
   - Data entry forms: Anchored on workflow (no parameters)
   - Reports: Workflow anchor + runtime parameters for flexibility
   - Users never need to touch POV pane
   - Fastest user adoption
```

### POV Design Tip
> "Leave all dimensions that have been defined in the rows or columns as blank in the Cube View POV pane. This provides administrators or super-users with a very quick visual of what dimensions are going to need to be defined in the rows and columns."

**Guideline:**
- Dimensions in rows/columns = Leave blank in POV
- Quick visual indicator of Cube View structure
- Easier maintenance

### Build Dynamically
**Anti-Pattern:** Hard-coded Cube Views
```
❌ Bad: A#Sales_2024
✅ Good: A#Sales.Base where Time=POV
```

**Exceptions:**
- Document any hard-coded Cube Views
- Ensure administrators know maintenance requirements
- Keep list minimal

### Data Unit Efficiency
**Performance Impact:**
> "The Data Unit doesn't only drive efficiencies in business rules, it also drives efficiency in rendering Cube Views."

**Best Practice:**
- **Minimize Data Unit dimensions in rows**
- Consider alternative layouts
- Break large reports into smaller ones
- Use parameters to reduce dataset size

**Data Unit Dimensions:**
- Scenario
- Time  
- Entity
- (Application-specific dimensions)

**Performance Comparison:**
```
❌ Slower:
   Rows: Entity (100 members)
   Columns: Accounts (50 members)
   = 100 Data Units loaded

✅ Faster:
   Rows: Accounts (50 members)
   Columns: Time (12 months)
   Parameters: Entity selection
   = 1-5 Data Units loaded
```

### Cube View Extender Rules
Use for advanced formatting and dynamic behavior:
- Varying images by entity
- Moving/removing headers by context
- Advanced conditional formatting
- Dynamic row/column manipulation

```vbnet
' Example: Dynamic footer based on entity
Public Function DynamicFooter(ByVal si As SessionInfo, _
    ByVal cv As CubeView) As CubeViewExtenderResult
    
    Dim entity As String = cv.POV.Entity.Name
    
    If entity.Contains("EMEA") Then
        cv.Footer = "Note: All amounts in EUR"
    ElseIf entity.Contains("APAC") Then
        cv.Footer = "Note: All amounts in Local Currency"
    Else
        cv.Footer = "Note: All amounts in USD"
    End If
    
    Return New CubeViewExtenderResult()
End Function
```

## Dashboard Design Best Practices

### When to Use Dashboards vs. Cube Views

**Use Dashboards when:**
1. **Data Location**: Data stored outside financial model or in external database
2. **Multiple Reports**: Series of related reports organized together
3. **User Actions**: High level of interaction (filtering, drilling, calculations)
4. **Blended Data**: Multiple data sources need to be combined
5. **Advanced Analytics**: Complex calculations or visualizations

**Use Cube Views when:**
- Simple data display from one cube
- Standard financial reporting
- Data entry forms
- Quick ad-hoc queries

### Dashboard Purpose Determination

**Critical Planning Phase:**
> "All dashboards begin with meticulous planning and a detailed blueprint. The more detailed you are, the better your dashboard-building experience will be."

**Planning Checklist:**
- [ ] Define intended audience
- [ ] Determine user interaction needs
- [ ] Identify data sources (cube, database, external)
- [ ] Outline reporting objectives
- [ ] Design layout mockup
- [ ] List required parameters
- [ ] Define data adapters needed
- [ ] Plan component types

### Data Consumption Types

#### 1. Static Analysis
**Characteristics:**
- Minimal user interaction
- "What you see is what you get"
- Collection of reports in one dashboard
- Users can navigate but not manipulate

**Use Cases:**
- Financial report packages for management
- Workflow task reporting
- Compliance reports
- Board presentations

**Example:**
```
Dashboard: Monthly Financial Package
├── Tab 1: Income Statement
├── Tab 2: Balance Sheet
├── Tab 3: Cash Flow
├── Tab 4: Variance Analysis
└── Tab 5: Key Metrics
```

#### 2. Interactive Analytics
**Characteristics:**
- High user interaction
- Customized views
- Drill-down capabilities
- Dynamic filtering

**Use Cases:**
- Executive dashboards
- Operational analytics
- Self-service reporting
- Investigative analysis

**Interaction Types:**
- Row/column modification
- Drill through to details
- Filter application
- Parameter changes
- Cross-report navigation

**Example:**
```
Dashboard: Sales Analytics
- Parameters: Region, Product, Time Period
- Component 1: Summary metrics (responds to parameters)
- Component 2: Trend chart (drill to detail)
- Component 3: Top 10 customers (filterable)
- Component 4: Source detail (driven by cell selection)
```

### Dashboard Components

#### Data Adapters
- Generate custom datasets
- Query cubes or databases
- Aggregate and transform data
- Provide data to components

**Best Practices:**
- Create reusable adapters
- Optimize queries (limit result sets)
- Cache results when appropriate
- Test performance with production volumes

#### Components
- Display data from adapters
- Control user interactions
- Provide visualizations
- Support drill-through

**Component Types:**
- Grids
- Charts (bar, line, pie, etc.)
- Cards (KPI displays)
- Custom HTML/JavaScript

#### Parameters
- Make dashboards dynamic
- Control adapter queries
- Filter component displays
- Improve user experience

**Parameter Design:**
- Provide sensible defaults
- Use cascading parameters when appropriate
- Limit required parameters
- Clear labels and tooltips

### Dashboard Layout Best Practices

**Visual Hierarchy:**
1. Most important information at top
2. Summary before detail
3. Left-to-right, top-to-bottom flow
4. Consistent spacing and alignment

**Responsive Design:**
- Test at different screen resolutions
- Avoid horizontal scrolling
- Use collapsible sections for detail
- Mobile considerations for OnePlace

**Color and Formatting:**
- Use corporate color scheme
- Highlight exceptions and alerts
- Consistent fonts and sizes
- Adequate white space

## Performance Optimization Strategies

### Cube View Performance

#### 1. Optimize Row Definitions
```
❌ Slow: 
   E#Root.Descendants
   (1000+ entities)

✅ Fast:
   E#Region1.Descendants
   (50 entities with parameter)
```

#### 2. Limit Member Expansions
```
❌ Slow:
   A#Assets.Descendants (500 accounts)

✅ Fast:
   A#Assets.Children (10 account groups)
   With drill-down to descendants
```

#### 3. Use Suppress Options
- Suppress zeros
- Suppress no data
- Suppress by data type

#### 4. Leverage Member Filters
- Filter at dimension level
- Use attribute filters
- Apply relationship filters

### Dashboard Performance

#### 1. Adapter Optimization
```vbnet
' Efficient data adapter
Public Function OptimizedAdapter(ByVal si As SessionInfo, _
    ByVal api As DashboardAPI, _
    ByVal args As DataAdapterArgs) As DataTable
    
    ' Limit result set
    Dim maxRows As Integer = 1000
    
    ' Use targeted POV
    Dim pov As New POV(
        Scenario:="Actual",
        Time:=api.Parms("SelectedPeriod"),
        Entity:=api.Parms("SelectedEntity")
    )
    
    ' Project only needed columns
    Dim columns As String() = {"Account", "Amount", "Variance"}
    
    ' Execute with limits
    Return api.Data.GetDataTable(pov, columns, maxRows)
End Function
```

#### 2. Component Efficiency
- Limit visible rows (use paging)
- Disable unnecessary features
- Use lazy loading for details
- Cache component state

#### 3. Parameter Efficiency
- Reduce cascading depth
- Cache parameter options
- Use async loading when appropriate
- Provide search for large lists

### General Performance Guidelines

#### Database Queries
- Use WHERE clauses to limit data
- Index frequently queried columns
- Avoid SELECT *
- Use stored procedures when appropriate

#### Business Rule Performance
- Minimize API calls
- Batch operations
- Use efficient data structures
- Profile and optimize hotspots

#### Caching Strategies
- Cache dimension members
- Cache frequently-used calculations
- Use session variables
- Clear cache appropriately

## Testing and Validation

### Performance Testing
- Test with production-like data volumes
- Measure render times for all Cube Views
- Load test dashboards with concurrent users
- Profile slow components

### User Acceptance Testing
- Test with actual end users
- Verify intuitive navigation
- Validate calculation accuracy
- Confirm print/export quality

### Regression Testing
- Retest after metadata changes
- Verify performance after updates
- Check cross-browser compatibility
- Test security access

## Monitoring and Maintenance

### Performance Monitoring
- Track Cube View render times
- Monitor dashboard load times
- Review error logs
- Analyze user behavior patterns

### Regular Maintenance
- Archive old report versions
- Clean up unused Cube Views/dashboards
- Update documentation
- Refactor inefficient code

## Anti-Patterns to Avoid

### Cube Views
❌ **All dimensions in rows**: Slow performance
❌ **No suppress options**: Displays unnecessary rows
❌ **Hard-coded members**: Maintenance nightmare
❌ **Mixed anchoring**: User confusion
❌ **> 5 second render**: Poor user experience

### Dashboards
❌ **Too much data**: Overloaded adapters
❌ **No parameters**: Inflexible, loads everything
❌ **Complex navigation**: Users get lost
❌ **Cluttered layout**: Information overload
❌ **No testing**: Production surprises

## Quick Reference

### Cube View Performance Checklist
- [ ] Renders in < 5 seconds
- [ ] Minimal Data Unit dimensions in rows
- [ ] Suppress zeros/no data enabled
- [ ] Dynamic member definitions
- [ ] Consistent anchoring strategy
- [ ] POV properly configured
- [ ] Tested with production data volumes

### Dashboard Performance Checklist
- [ ] Data adapters optimized
- [ ] Result sets limited (< 1000 rows ideal)
- [ ] Components load efficiently
- [ ] Parameters have defaults
- [ ] Layout responsive
- [ ] Interactions intuitive
- [ ] Tested with concurrent users

## References
- OneStream Foundation Handbook, Chapter 10: Reporting
- Cube View Design Guide
- Dashboard Development Guide
- Performance Tuning Guide
