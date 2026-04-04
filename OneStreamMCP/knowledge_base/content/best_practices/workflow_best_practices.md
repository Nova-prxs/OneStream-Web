# OneStream Workflow Best Practices

## Overview
Workflow in OneStream manages the data collection, validation, certification, and approval processes across the organization.

## Workflow Design Best Practices

### 1. Workflow Profile Structure
- **Use Hierarchical Design**: Organize workflow profiles to mirror your organizational structure
- **Minimize Profile Count**: Consolidate similar workflows to reduce maintenance
- **Standardize Naming**: Use consistent naming conventions (e.g., "GL_Load_Monthly", "Planning_Budget_Annual")

### 2. Certification and Lock
- **Implement Progressive Locks**: Lock data at appropriate stages (Scenario, Time, Entity levels)
- **Use Certification Rules**: Define business rules to validate data before certification
- **Document Lock Hierarchy**: Clearly define which roles can lock/unlock at each level

### 3. Workflow Stages

#### Stage Configuration
- **Limit Stages**: Use 3-5 stages typically (e.g., "Not Started", "Working", "Submitted", "Approved")
- **Clear Progression**: Each stage should have a clear purpose and exit criteria
- **Approval Chains**: Configure multi-level approvals for sensitive processes

#### Stage Actions
```vbnet
' Example: Business Rule for Stage Validation
Public Function ValidateStageData(ByVal si As SessionInfo, _
    ByVal wfProfile As WorkflowProfile, _
    ByVal wfTime As WorkflowTime) As String
    
    ' Check data completeness
    If Not IsDataComplete(si, wfProfile, wfTime) Then
        Return "Data validation failed: Missing required accounts"
    End If
    
    ' Check balance
    If Not IsBalanced(si, wfProfile, wfTime) Then
        Return "Data validation failed: Trial balance does not balance"
    End If
    
    Return String.Empty ' Success
End Function
```

### 4. Notifications and Alerts
- **Email Notifications**: Configure for stage changes, certifications, and approaching deadlines
- **Dashboard Alerts**: Use workflow status cubes for real-time monitoring
- **Escalation Rules**: Implement automatic escalation for overdue items

### 5. Workflow Security
- **Role-Based Access**: Assign workflow actions to security groups, not individuals
- **Segregation of Duties**: Separate submission and approval rights
- **Audit Trail**: Enable workflow audit logging for compliance

## Performance Optimization

### Data Loading
- **Batch Processing**: Load data in batches during workflow "Not Started" stage
- **Parallel Processing**: Use parallel workflow units for large organizations
- **Off-Peak Loading**: Schedule intensive loads outside business hours

### Certification Performance
```vbnet
' Optimize certification with targeted clears
Public Sub CertifyWithOptimization(ByVal si As SessionInfo, _
    ByVal wfProfile As WorkflowProfile)
    
    ' Clear only necessary intersections
    Dim clearArgs As New DataBufferClearArgs()
    clearArgs.ScenarioTypeId = ScenarioType.Actual
    clearArgs.TimeArg = "2024M1"
    clearArgs.EntityArg = "E#ParentCompany.Base"
    
    ' Perform targeted clear before certification
    FinanceRules.DataBuffer.Clear(si, clearArgs)
    
    ' Certify
    wfProfile.Certify(si)
End Sub
```

## Common Patterns

### 1. Monthly Close Workflow
```
Stages: Not Started → Working → Submitted → Reviewed → Approved → Certified
- Lock after Approved
- Email notifications at each stage
- Validation rules before submission
```

### 2. Budget Planning Workflow
```
Stages: Not Started → Input → Reviewed → Management Review → Approved → Published
- Multiple approval levels
- Version control with scenarios
- Collaborative editing in Working stage
```

### 3. Forecast Workflow
```
Stages: Not Started → Forecasting → Submitted → Consolidated → Approved
- Rolling forecasts with time patterns
- Driver-based calculations before submission
- Comparison to actuals validation
```

## Testing Best Practices

### User Acceptance Testing (UAT)
- **Test Scripts**: Create detailed test scripts for each workflow action
- **Expected Results**: Define clear acceptance criteria
- **Traceability Matrix**: Map requirements to test cases
- **Sign-off Process**: Require formal approval before production deployment

### Roles in Testing
- **Customer**: Write and approve test scripts, coordinate user IDs
- **Testers**: Execute scripts, log issues, compare actual vs expected results
- **OneStream Team**: Provide guidance, resolve issues, update documentation

## Integration Points

### Workflow with Data Management
- Workflow status can trigger data integration jobs
- Use workflow channels to control data visibility
- Lock status affects data load permissions

### Workflow with Business Rules
- Stage change events can trigger business rules
- Certification rules validate before status changes
- Dashboard business rules can display workflow status

## Monitoring and Maintenance

### Key Metrics
- **Completion Rates**: Track on-time completion by entity/time
- **Bottlenecks**: Identify stages with longest duration
- **Exception Rates**: Monitor certification failures and lock conflicts

### Regular Reviews
- Monthly: Review workflow completion trends
- Quarterly: Optimize stage configurations based on usage
- Annually: Reassess workflow structure alignment with organization

## Anti-Patterns to Avoid

❌ **Too Many Stages**: Creates confusion and slows process
❌ **Insufficient Security**: Allows users to bypass required steps
❌ **No Validation Rules**: Allows bad data to progress through workflow
❌ **Manual Notifications**: Prone to errors; automate instead
❌ **One-Size-Fits-All**: Different processes need different workflows

## References
- OneStream Foundation Handbook, Chapter 2: Methodology and the Project
- Workflow Configuration Guide
- Security and Access Management
