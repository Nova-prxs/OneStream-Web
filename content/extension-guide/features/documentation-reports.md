# Documentation Reports

The Documentation Reports feature generates reports about your OneStream application's configuration. These reports document member text fields, member formulas, and workflow profiles, and can be exported in multiple formats.

## Available Reports

### Member Text Fields Report

Generates a report of all text fields assigned to dimension members across your application.

**Includes**:
- Dimension name
- Member name
- Text field name
- Text field value
- Last modified date

**Use case**: Audit text-based metadata attached to members, such as descriptions, notes, or custom text attributes.

### Member Formulas Report

Generates a report of all formulas assigned to dimension members.

**Includes**:
- Dimension name
- Member name
- Formula type (DynamicCalc, FormulaPass1, etc.)
- Formula code
- DrillDown formula code (if present)
- Last modified date

**Use case**: Review all calculation logic in one place, identify members with complex formulas, and audit formula coverage across dimensions.

### Workflow Profiles Report

Generates a report of all workflow profiles configured in the application.

**Includes**:
- Profile name
- Profile description
- Assigned entities
- Workflow steps and their configuration
- Certification settings
- Lock/unlock rules

**Use case**: Document the review and approval workflows in your application for compliance, auditing, or onboarding new team members.

## Generating Reports

Reports are accessed through the OneStream Explorer sidebar:

1. Open the OneStream Explorer panel (click the OneStream icon in the Activity Bar).
2. Expand the **Documentation** section in the tree.
3. Click the report you want to generate:
   - **Member Text Fields**
   - **Member Formulas**
   - **Workflow Profiles**
4. The report is generated and displayed in a new editor tab.

Report generation may take several seconds depending on the size of your application and the amount of data.

## Export Formats

Each report can be exported in four formats:

### CSV

A comma-separated values file that can be opened in Excel, Google Sheets, or any spreadsheet application.

- Click **Export CSV** in the report toolbar.
- Choose a save location.
- The file contains one row per item with all columns from the report.

### Markdown

A Markdown-formatted text file suitable for inclusion in documentation repositories or wikis.

- Click **Export Markdown** in the report toolbar.
- The output uses Markdown tables for structured data and code blocks for formula content.
- Ideal for committing to Git alongside your rules for a complete documentation set.

### Word (DOCX)

A Microsoft Word document with formatted tables and code sections.

- Click **Export Word** in the report toolbar.
- The document includes a title page, table of contents, and formatted sections for each report item.
- Suitable for sharing with stakeholders who prefer Word format.

## Workflow Tips

### Regular Documentation

Generate and export reports periodically (e.g., after each deployment) to maintain up-to-date documentation of your application's configuration.

### Compliance Audits

The Workflow Profiles report is particularly useful for compliance audits. Export it to Word format and include it in your compliance documentation package.

### Code Reviews

The Member Formulas report helps with code reviews. Export it to Markdown and commit it alongside your rule files so reviewers can see all formulas in context.
