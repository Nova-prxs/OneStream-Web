# Question Bank - Section 6: Other Reporting Components (10% of exam)

## Objectives
- **300.6.1:** Identify and describe the different reporting types available in OneStream
- **300.6.2:** Make changes to Extensible Documents
- **300.6.3:** Modify Report Books for consolidated reporting

---

## Objective 300.6.1: Identify and describe reporting types

### Question 1 (Easy)
**300.6.1** | Difficulty: Easy

Which of the following is NOT a standard reporting type in OneStream?

A) Cube Views
B) Spreadsheet Reports
C) Microsoft Access Reports
D) Report Books

<details>
<summary>Show answer</summary>

**Correct answer: C)**

OneStream's standard reporting types include Cube Views, Spreadsheet Reports, Report Books, BI Viewer, and Extensible Documents. Microsoft Access Reports (C) are not a native OneStream reporting type. OneStream has its own built-in reporting components that do not rely on external desktop database tools.
</details>

---

### Question 2 (Easy)
**300.6.1** | Difficulty: Easy

What distinguishes a Cube View from a Spreadsheet report in terms of their primary use cases?

A) There is no difference; they are identical components
B) A Cube View is optimized for structured dimensional data display with hierarchy navigation, while a Spreadsheet offers free-form layout flexibility similar to Excel
C) A Cube View displays only charts, while a Spreadsheet displays only text
D) A Spreadsheet can access cube data, but a Cube View cannot

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views excel at structured dimensional reporting with features like DrillDown, hierarchy expansion, and dimension-based layout. Spreadsheet reports provide free-form layout flexibility similar to Excel, ideal for custom presentations, mixed content, and pixel-precise formatting. They are not identical (A), both can display tabular data (C), and both access cube data (D).
</details>

---

### Question 3 (Medium)
**300.6.1** | Difficulty: Medium

An organization needs to produce a monthly board package that includes an income statement, balance sheet, cash flow statement, and management commentary. Which combination of OneStream reporting components would best serve this requirement?

A) A single Text Label component containing all content
B) A Report Book that combines multiple Cube Views and Spreadsheet reports into a single distributable package
C) A folder of individual CSV files emailed manually
D) A single Cube View with all financial statements in one grid

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Report Book is designed to combine multiple reporting components (Cube Views, Spreadsheets, etc.) into a single consolidated package that can be generated, distributed, and archived. This is ideal for board packages that include multiple financial statements and commentary. A single Text Label (A) cannot contain dynamic data. CSV files (C) lack formatting and automation. A single Cube View (D) would be impractical for multiple distinct statements.
</details>

---

## Objective 300.6.2: Make changes to Extensible Documents

### Question 4 (Easy)
**300.6.2** | Difficulty: Easy

What is an Extensible Document in OneStream?

A) A file format for storing images
B) A customizable document framework that can include data entry forms, reports, and workflow steps configured through XML and Business Rules
C) A read-only system log file
D) A compressed ZIP archive of dashboard files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extensible Documents are a flexible framework in OneStream for building customizable documents that can combine data entry, reporting, calculations, and workflow. They are configured through XML definitions and can be extended with Business Rules. They are not image files (A), log files (C), or ZIP archives (D).
</details>

---

### Question 5 (Medium)
**300.6.2** | Difficulty: Medium

When modifying an Extensible Document to add a new data column, which of the following steps is typically required?

A) Reinstall the OneStream platform
B) Update the document's XML schema or configuration to define the new column, and modify any associated Business Rules that process or display the column data
C) Create a new OneStream application from scratch
D) Contact Microsoft to modify the underlying SQL Server schema

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Adding a column to an Extensible Document requires updating its configuration (typically XML-based) to define the new column's properties, data type, and behavior, and then updating any Business Rules that read, write, or calculate data in that column. Platform reinstallation (A), new applications (C), and contacting Microsoft (D) are unnecessary.
</details>

---

### Question 6 (Hard)
**300.6.2** | Difficulty: Hard

After modifying an Extensible Document's XML configuration to add validation logic, existing documents that were previously saved and approved now fail validation when reopened. What is the most likely cause and remedy?

A) The server's clock is out of sync
B) The new validation rules are retroactively applied to existing documents that were saved under the old rules; the developer should implement version-aware validation that distinguishes between legacy and newly created documents
C) Extensible Documents cannot be modified after initial deployment
D) The user needs to clear their browser cookies

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When new validation rules are added to an Extensible Document, they apply to all documents including previously saved ones. If legacy documents do not meet the new criteria, they fail validation on reopen. The remedy is to implement version-aware validation logic that applies new rules only to documents created after the change, or to provide a migration path. Clock sync (A), immutability (C), and cookies (D) are irrelevant.
</details>

---

### Question 7 (Medium)
**300.6.2** | Difficulty: Medium

Which of the following can be customized within an Extensible Document?

A) Only the document title
B) Column definitions, row structures, validation rules, calculation logic, and workflow integration
C) Only the background color
D) Only the print margins

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extensible Documents offer comprehensive customization including column definitions, row structures, cell-level validation rules, calculation logic, workflow integration, and formatting. They are highly configurable beyond just titles (A), colors (C), or margins (D).
</details>

---

## Objective 300.6.3: Modify Report Books for consolidated reporting

### Question 8 (Easy)
**300.6.3** | Difficulty: Easy

What is a Report Book in OneStream?

A) A printed textbook about OneStream
B) A feature that combines multiple reports (Cube Views, Spreadsheets, etc.) into a single consolidated output that can be generated as a PDF or Excel package
C) A database backup file
D) A log of all user activity in the application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Report Book is a OneStream feature that assembles multiple individual reports into a single consolidated output package. It can include Cube Views, Spreadsheets, and other components, and can be generated as PDF or Excel for distribution. It is not a printed book (A), backup file (C), or activity log (D).
</details>

---

### Question 9 (Medium)
**300.6.3** | Difficulty: Medium

When modifying a Report Book, an administrator wants to add a new Cube View report as the third section. What must be configured to ensure the Cube View uses the correct POV context within the Report Book?

A) The Cube View must be converted to a JPEG image first
B) The Report Book's POV settings or parameter mappings must be configured to pass the correct Scenario, Time, and Entity context to the newly added Cube View
C) The Cube View must be deleted and recreated inside the Report Book
D) No configuration is needed; Report Books ignore POV settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When adding a report to a Report Book, the POV context (Scenario, Time, Entity, etc.) must be properly mapped so that the Cube View renders data for the correct intersection. Report Books have POV settings or parameter mappings that pass context to each included report. Converting to an image (A) loses interactivity, Cube Views are referenced rather than recreated (C), and Report Books do use POV settings (D).
</details>

---

### Question 10 (Medium)
**300.6.3** | Difficulty: Medium

A Report Book is configured to generate reports for multiple entities automatically. Which Report Book feature enables this?

A) Manual copy-paste for each entity
B) The Entity loop or burst feature that iterates through a list of entities and generates a separate report section for each one
C) A single static entity hardcoded in the report
D) An external Python script that calls the OneStream API

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Books support entity bursting (or entity looping), which automatically iterates through a specified list of entities and generates a separate report section for each. This is a built-in feature for producing multi-entity board packages. Manual copy-paste (A) is impractical, hardcoding (C) defeats the purpose, and external scripts (D) are unnecessary when the feature is built in.
</details>

---

### Question 11 (Hard)
**300.6.3** | Difficulty: Hard

An administrator modifies a Report Book to add a cover page and table of contents. After generation, the page numbering is incorrect -- the cover page is numbered as page 1, but the administrator wants numbering to start on the first report page. How should this be resolved?

A) Remove the cover page entirely
B) Configure the Report Book's pagination settings to exclude the cover page and table of contents from the page numbering sequence, or set a page number offset
C) Print the report and manually renumber the pages with a pen
D) Generate two separate Report Books and staple them together

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Books provide pagination control settings that allow administrators to configure which sections are included in page numbering and to set page number offsets. The cover page and table of contents can be excluded from the sequential numbering. Removing the cover page (A) loses the desired content. Manual numbering (C) and separate books (D) are not practical solutions.
</details>

---

### Question 12 (Easy)
**300.6.3** | Difficulty: Easy

Which of the following output formats are typically supported when generating a Report Book?

A) Only plain text (.txt) files
B) PDF and Excel formats
C) Only MP3 audio files
D) Only HTML web pages

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Books in OneStream can typically be generated in PDF format (for distribution and printing) and Excel format (for further analysis and manipulation). Plain text (A), audio (C), and HTML-only (D) are not standard Report Book output formats.
</details>

---

### Question 13 (Easy)
**300.6.1** | Difficulty: Easy

Which OneStream reporting component is best suited for ad-hoc, user-driven data exploration with charts and visual analytics?

A) A static Text Label
B) The BI Viewer, which provides interactive visualizations driven by Data Adapters
C) A plain Bound List parameter
D) The Workspace navigation panel

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The BI Viewer is designed for interactive data exploration and visual analytics, powered by Data Adapters. It supports charts, grids, and other visual formats that users can interact with for ad-hoc analysis. Text Labels (A) are static, Bound List parameters (C) are selection controls, and navigation panels (D) organize content but do not display analytics.
</details>

---

### Question 14 (Medium)
**300.6.1** | Difficulty: Medium

A finance team needs three types of reports: a detailed income statement with hierarchy drill-down, a custom management summary with free-form layout, and an interactive chart dashboard. Which combination of OneStream reporting types best addresses these needs?

A) Three identical Cube Views
B) A Cube View for the income statement, a Spreadsheet report for the management summary, and a BI Viewer with charts for the interactive dashboard
C) Three Spreadsheet reports
D) A single Report Book containing all three

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Each reporting type has its strength: Cube Views excel at hierarchical dimensional data with drill-down, Spreadsheets provide free-form layout flexibility for custom summaries, and BI Viewers deliver interactive chart-based analytics. Using the right component for each need (B) is superior to using identical components for all (A, C). A Report Book (D) bundles reports for distribution but does not solve the interactive dashboard requirement.
</details>

---

### Question 15 (Hard)
**300.6.2** | Difficulty: Hard

A developer modifies an Extensible Document to add a new calculated column that computes the variance between two existing data columns. After deployment, existing documents show the new column but with empty values, while newly created documents calculate correctly. What is the most likely cause?

A) The server does not support calculations in Extensible Documents
B) Existing documents were saved with a data structure that does not include the new column's data; the calculation logic may need a migration or recalculation step to populate the new column for previously saved documents
C) Extensible Documents cannot have more than two columns
D) The developer forgot to restart the user's browser

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Previously saved Extensible Documents store their data based on the schema that existed at the time of saving. When a new calculated column is added, existing documents may not have the underlying data populated for that column. A migration script or recalculation step is needed to update historical documents. Calculations are supported (A), there is no column limit of two (C), and browser restarts (D) do not resolve data structure issues.
</details>

---

### Question 16 (Easy)
**300.6.3** | Difficulty: Easy

What is the primary benefit of using Report Books instead of distributing individual reports separately?

A) Report Books encrypt all data automatically
B) Report Books consolidate multiple reports into a single package, making it easier to generate, distribute, and archive a complete set of related reports
C) Report Books are the only way to create reports in OneStream
D) Report Books reduce the size of the cube database

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The primary benefit of Report Books is consolidation -- they assemble multiple related reports (income statements, balance sheets, variance analyses, etc.) into a single distributable package. This simplifies generation, distribution, and archival compared to managing individual reports separately. They do not auto-encrypt (A), are not the only reporting method (C), and do not affect database size (D).
</details>

---

### Question 17 (Medium)
**300.6.2** | Difficulty: Medium

When modifying an Extensible Document, what must a developer consider regarding workflow implications?

A) Extensible Documents are never connected to workflows
B) Changes to the document structure (such as adding required fields or changing validation rules) may affect in-progress workflow instances that reference the document, potentially blocking approvals or requiring re-submission
C) Workflow settings are stored in a separate application and cannot be affected
D) Only the document title affects workflows

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extensible Documents are often integrated with workflow processes. Structural changes -- such as adding required fields, modifying validation rules, or changing column definitions -- can affect documents that are currently in mid-workflow. In-progress documents may fail validation under new rules, potentially blocking approvals. Developers must consider the impact on active workflow instances. Documents are indeed connected to workflows (A), workflow settings are within the same application (C), and structural changes beyond the title matter (D).
</details>

---

### Question 18 (Medium)
**300.6.3** | Difficulty: Medium

A Report Book is configured to generate entity-level reports for 50 entities. The administrator wants to add a consolidated total report as the first section before the entity-level sections. How should this be configured?

A) Generate two separate Report Books and combine them manually
B) Add the consolidated report (using a parent or total entity) as the first section in the Report Book definition, before the entity burst/loop configuration
C) Run the entity burst and then manually insert the total page
D) This cannot be done in OneStream Report Books

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Books allow administrators to define multiple sections with different configurations. The consolidated total report can be added as a fixed section using the parent or total entity member, positioned before the entity burst section that iterates through the 50 individual entities. This produces a single output with the total first, followed by entity-level details. Separate Report Books (A) and manual insertion (C) are impractical, and OneStream supports this configuration (D).
</details>

---

### Question 19 (Hard)
**300.6.2** | Difficulty: Hard

An administrator needs to modify an Extensible Document to support a new regulatory reporting requirement that adds 5 new data fields. The document is used by 200+ entities monthly, and there are currently 6 months of historical documents in the system. What is the recommended approach?

A) Delete all historical documents and start fresh
B) Add the new fields to the document configuration with appropriate defaults or optional status, implement version-aware logic so historical documents are not broken by the new requirements, and test thoroughly with a sample of existing documents before deployment
C) Create an entirely new Extensible Document and ask all users to switch
D) Add the fields only to a local Excel file and manage them outside OneStream

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is careful, backward-compatible modification: add the new fields with appropriate defaults (or make them optional for legacy documents), implement version-aware logic that applies the new requirements only to current-period documents, and test with existing documents to ensure no breakage. Deleting history (A) loses critical data, creating a new document (C) fragments reporting, and managing outside OneStream (D) creates data silos.
</details>

---

### Question 20 (Easy)
**300.6.1** | Difficulty: Easy

Which of the following reporting types in OneStream allows for workflow-integrated data entry and approval processes?

A) Cube Views
B) Extensible Documents
C) Chart components
D) Image components

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extensible Documents are uniquely designed to support workflow-integrated data entry and approval processes. They combine data entry forms with validation, calculation, and workflow steps, enabling controlled submission and approval cycles. Cube Views (A) are primarily for reporting, and Chart (C) and Image (D) components are for visualization and display only.
</details>

---

### Question 21 (Medium)
**300.6.3** | Difficulty: Medium

When a Report Book is generated, what determines the order of sections in the output?

A) Sections are always sorted alphabetically by report name
B) The order is determined by the section sequence configured in the Report Book definition by the administrator
C) Sections are ordered randomly on each generation
D) The order depends on which user generates the Report Book

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The section order in a Report Book output is determined by the sequence configured by the administrator in the Report Book definition. Sections appear in the order they are listed, giving full control over the document structure. They are not sorted alphabetically (A), randomly ordered (C), or user-dependent (D).
</details>

---

### Question 22 (Hard)
**300.6.3** | Difficulty: Hard

An administrator sets up a Report Book with entity bursting for 30 entities. The Report Book generates successfully but takes over 20 minutes to complete. Users need the output within 5 minutes. What optimization strategies should be considered?

A) Remove all entities except one
B) Optimize the underlying report queries (add filters, reduce data scope), enable parallel entity processing if supported, simplify complex formatting, and consider caching or pre-calculating commonly used data
C) Ask users to accept the 20-minute wait time
D) Switch from PDF output to plain text to save time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Performance optimization for Report Books involves multiple strategies: optimizing the underlying report queries to reduce data retrieval time, enabling parallel entity processing if the platform supports it, simplifying complex formatting that slows rendering, and pre-calculating or caching frequently used data. Removing entities (A) reduces functionality, accepting poor performance (C) is not a solution, and plain text output (D) sacrifices formatting quality.
</details>

---

### Question 23 (Medium)
**300.6.2** | Difficulty: Medium

What type of Business Rule is most commonly associated with Extensible Documents for custom logic?

A) A Data Management Business Rule
B) An Extensible Document Business Rule that handles events such as document load, save, calculate, and validate
C) A Cube View XFBR
D) A Workspace Extender Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extensible Documents use dedicated Extensible Document Business Rules that handle document-specific events: loading data, saving, calculating cells, validating input, and managing workflow transitions. These rules are specifically designed for the Extensible Document framework. Data Management rules (A) handle ETL processes, XFBRs (C) are for Cube Views, and Workspace Extenders (D) are for Workspace parameters.
</details>

---

### Question 24 (Easy)
**300.6.1** | Difficulty: Easy

Which OneStream reporting component is specifically designed to combine multiple individual reports into a single distributable output?

A) A Cube View
B) A Report Book
C) A Text Label
D) A Dashboard Parameter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Books are specifically designed to combine multiple individual reports (Cube Views, Spreadsheets, and other components) into a single consolidated output that can be generated as a PDF or Excel package for distribution. Cube Views (A) are single report components, Text Labels (C) display static text, and Dashboard Parameters (D) are input controls.
</details>
