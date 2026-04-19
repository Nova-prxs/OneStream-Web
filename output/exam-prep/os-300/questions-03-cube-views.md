# Question Bank - Section 3: Cube Views (23% of exam)

## Objectives
- **300.3.1:** Configure Cube View properties for reporting
- **300.3.2:** Apply formatting features to Cube Views
- **300.3.3:** Implement DrillDown and Drill-back functionality in Cube Views
- **300.3.4:** Use XFBRs (eXtensible Finance Business Rules) within Cube Views
- **300.3.5:** Configure sparse row suppression in Cube Views
- **300.3.6:** Implement alternate language descriptions in Cube Views

---

## Objective 300.3.1: Configure Cube View properties for reporting

### Question 1 (Easy)
**300.3.1** | Difficulty: Easy

What is a Cube View in OneStream?

A) A 3D graphical chart component
B) A grid-based reporting component that displays financial data from the cube with configurable rows, columns, and POV settings
C) A database view created in SQL Server
D) A virtual reality interface for data visualization

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Cube View is OneStream's grid-based reporting component that retrieves and displays data from the cube. It allows administrators to configure rows, columns, and Point of View (POV) settings to present financial and operational data in a structured format. It is not a 3D chart (A), a SQL database view (C), or a VR interface (D).
</details>

---

### Question 2 (Easy)
**300.3.1** | Difficulty: Easy

Which of the following is a key property that must be set when creating a new Cube View?

A) The server's IP address
B) The Row and Column dimension assignments that define the data layout
C) The user's email address
D) The Windows registry key for OneStream

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When creating a Cube View, the Row and Column dimension assignments are essential properties that define which dimensions appear on rows and columns. These determine the data layout and structure of the report. Server IP (A), email addresses (C), and registry keys (D) are not Cube View properties.
</details>

---

### Question 3 (Medium)
**300.3.1** | Difficulty: Medium

A Cube View is configured with Account on rows and Time on columns. The POV is set to a specific Scenario and Entity. What happens if the user changes the Entity in the POV bar?

A) The Cube View structure changes so Entity appears on rows
B) The Cube View refreshes and displays data for the newly selected Entity while keeping Account on rows and Time on columns
C) The Cube View becomes read-only
D) Nothing happens; the POV is locked after initial configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When the POV Entity changes, the Cube View refreshes its data to reflect the new Entity context while maintaining the same structural layout (Account on rows, Time on columns). The POV acts as a filter context for the data. The structure does not change (A), it does not become read-only (C), and the POV is not locked (D).
</details>

---

## Objective 300.3.2: Apply formatting features to Cube Views

### Question 4 (Easy)
**300.3.2** | Difficulty: Easy

Which formatting feature allows you to display negative numbers in red in a Cube View?

A) Row height adjustment
B) Conditional formatting rules based on cell values
C) Column sorting
D) Dashboard background color settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Conditional formatting rules allow you to define visual formatting (such as red font color) based on cell values (e.g., when a value is less than zero). Row height (A) and column sorting (C) do not affect number color. Dashboard background (D) applies to the entire dashboard, not individual cells.
</details>

---

### Question 5 (Medium)
**300.3.2** | Difficulty: Medium

An administrator wants to apply different number formats to different rows in a Cube View -- percentages for ratio accounts and currency for monetary accounts. How is this best achieved?

A) Create two separate Cube Views, one for each format
B) Use row-level formatting or Account dimension member properties to assign different number format strings per row
C) Set a single global number format and ask users to mentally convert
D) Export the data to Excel and format it there

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream Cube Views support row-level formatting where different number format strings can be applied based on the row member or account type. Account dimension properties can drive whether a row shows as percentage, currency, or other formats. Creating separate Cube Views (A) is unnecessarily complex. A single global format (C) cannot handle mixed formats. Exporting to Excel (D) defeats the purpose of in-application reporting.
</details>

---

### Question 6 (Medium)
**300.3.2** | Difficulty: Medium

Which of the following formatting features is available in OneStream Cube Views?

A) Animated GIF backgrounds per cell
B) Custom header styles including font, size, color, and background color for row and column headers
C) Embedded video playback within cells
D) 3D cell shading with shadow effects

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views support custom header styles including font settings, size, color, and background color for both row and column headers. This allows administrators to create professional, branded report layouts. Animated GIFs (A), embedded video (C), and 3D shading (D) are not supported Cube View features.
</details>

---

## Objective 300.3.3: Implement DrillDown and Drill-back functionality

### Question 7 (Medium)
**300.3.3** | Difficulty: Medium

What is the difference between DrillDown and Drill-back in a OneStream Cube View?

A) DrillDown navigates to lower levels within the cube hierarchy, while Drill-back traces data to its source system or journal entries
B) DrillDown exports data to PDF, while Drill-back imports data from Excel
C) There is no difference; they are interchangeable terms
D) DrillDown deletes data, while Drill-back restores it

<details>
<summary>Show answer</summary>

**Correct answer: A)**

DrillDown allows users to expand and navigate to lower levels of a dimension hierarchy within the cube (e.g., from a parent account to its children). Drill-back traces data back to its source, such as source system transactions, journal entries, or loaded data. They serve complementary but different purposes. They are not interchangeable (C) and neither deletes or exports data (B, D).
</details>

---

### Question 8 (Easy)
**300.3.3** | Difficulty: Easy

When a user double-clicks on a parent member in a Cube View row, what does DrillDown typically show?

A) A pop-up with the member's security settings
B) The child members of that parent member with their respective data values
C) The application's system log
D) A print preview of the current Cube View

<details>
<summary>Show answer</summary>

**Correct answer: B)**

DrillDown on a parent member expands the view to show the child members of that parent along with their data values. This allows users to analyze data at progressively more detailed levels of the hierarchy. It does not show security settings (A), system logs (C), or print previews (D).
</details>

---

### Question 9 (Hard)
**300.3.3** | Difficulty: Hard

A Drill-back is configured on a Cube View to show source transactions. When users click Drill-back, they see an empty result set even though data exists. Which of the following is the most likely cause?

A) The user's browser does not support Drill-back
B) The Drill-back Business Rule or data source mapping does not match the current POV intersection, or the source staging tables have been purged
C) Drill-back only works for future periods
D) The Cube View must be exported before Drill-back can function

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An empty Drill-back result typically means the Business Rule or source mapping is not correctly configured for the current data intersection, or the underlying staging/source tables have been purged or archived. Browser support (A) is not an issue for OneStream's Drill-back. Drill-back works for any period with data (C). Exporting is not a prerequisite (D).
</details>

---

## Objective 300.3.4: Use XFBRs within Cube Views

### Question 10 (Medium)
**300.3.4** | Difficulty: Medium

What is the role of an XFBR (eXtensible Finance Business Rule) in a Cube View?

A) To replace the Cube View entirely with a custom HTML page
B) To provide custom calculated rows, columns, or data transformations that go beyond standard cube data retrieval
C) To manage user authentication for the Cube View
D) To convert the Cube View into a chart automatically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFBRs in Cube Views allow developers to create custom calculations, computed rows/columns, or data transformations that supplement the standard cube data. This enables complex KPIs, variance calculations, or cross-dimensional logic. They do not replace the Cube View with HTML (A), manage authentication (C), or auto-convert to charts (D).
</details>

---

### Question 11 (Hard)
**300.3.4** | Difficulty: Hard

A developer creates an XFBR to calculate a custom variance column in a Cube View. The XFBR executes without error but the variance column displays zeros for all rows. What should the developer investigate first?

A) Whether the Cube View is saved in the correct file format
B) Whether the XFBR is reading the correct data cells and writing the calculated results to the correct output cell references
C) Whether the OneStream license includes XFBR support
D) Whether the application server's time zone matches the user's local time zone

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If an XFBR runs without error but produces zeros, the most likely issue is that the code is reading from incorrect cell references (getting nulls or zeros) or writing to the wrong output locations. The developer should verify the cell reference logic in the XFBR code. File format (A) is irrelevant, XFBRs are standard OneStream functionality (C), and time zones (D) would not cause zero values.
</details>

---

## Objective 300.3.5: Configure sparse row suppression

### Question 12 (Medium)
**300.3.5** | Difficulty: Medium

What is sparse row suppression in a Cube View?

A) A feature that compresses the cube database to save storage space
B) A display setting that hides rows with no data (all zeros or nulls) to produce a cleaner, more concise report
C) A security feature that hides rows from unauthorized users
D) A feature that removes rows from the underlying database permanently

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Sparse row suppression hides rows that contain no meaningful data (all zeros, nulls, or no stored values) from the Cube View display. This makes reports cleaner and easier to read by showing only rows with actual data. It does not affect the database (A, D) and is not a security feature (C).
</details>

---

### Question 13 (Hard)
**300.3.5** | Difficulty: Hard

A Cube View has sparse row suppression enabled, but users report that some rows with zero values are still visible. What is the most likely explanation?

A) Sparse row suppression only works in Excel exports
B) The rows contain explicitly stored zero values rather than being truly empty (no data), and the suppression setting may be configured to suppress only NoData rows rather than zero-value rows
C) The OneStream platform does not actually support sparse row suppression
D) The Cube View is too large for suppression to function

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream distinguishes between "NoData" (no value stored) and explicitly stored zero values. If suppression is configured to hide only NoData rows, rows with stored zeros will remain visible. The administrator may need to adjust the suppression setting to also suppress zero-value rows. Sparse row suppression works in the application (A, C) and is not limited by view size (D).
</details>

---

## Objective 300.3.6: Implement alternate language descriptions

### Question 14 (Easy)
**300.3.6** | Difficulty: Easy

What does the alternate language descriptions feature in Cube Views allow?

A) Translation of the entire OneStream user interface into a different language
B) Display of dimension member descriptions in a user's preferred language, based on configured language profiles
C) Automatic translation of financial data values into foreign currencies
D) Conversion of Cube View reports into audio format in multiple languages

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Alternate language descriptions allow Cube Views to display dimension member labels (descriptions) in the user's preferred language. When language profiles are configured with translated descriptions for members, the Cube View renders those descriptions based on the user's language setting. It does not translate the UI (A), convert currencies (C), or produce audio (D).
</details>

---

### Question 15 (Medium)
**300.3.6** | Difficulty: Medium

To enable alternate language descriptions in a Cube View, which of the following must be configured in the application?

A) A separate cube for each language
B) Language-specific member descriptions in the dimension metadata and a user language preference setting
C) A third-party translation API integration
D) Separate OneStream applications for each supported language

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Alternate language descriptions require two things: the dimension metadata must contain translated descriptions for each supported language, and users must have their language preference configured. The Cube View then displays the appropriate description based on the user's setting. Separate cubes (A), translation APIs (C), and separate applications (D) are not required.
</details>

---

### Question 16 (Easy)
**300.3.1** | Difficulty: Easy

Which of the following can be placed on the rows of a Cube View?

A) Only the Account dimension
B) Any dimension available in the application, including Account, Entity, Flow, or custom dimensions
C) Only text labels with no data binding
D) Only calculated columns from an XFBR

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube View rows can be configured with any dimension available in the application. While Account is the most common dimension placed on rows, Entity, Flow, UD (user-defined) dimensions, and other standard dimensions can all be assigned to rows. The configuration is not limited to Account (A), static text (C), or XFBR columns only (D).
</details>

---

### Question 17 (Medium)
**300.3.1** | Difficulty: Medium

When configuring a Cube View, what is the purpose of the POV (Point of View) settings?

A) To define which dimensions appear on rows and columns
B) To fix specific dimension member selections that act as filters for the data displayed, for dimensions not on rows or columns
C) To set the Cube View's background color
D) To configure the server connection string

<details>
<summary>Show answer</summary>

**Correct answer: B)**

POV settings in a Cube View define fixed dimension member selections for all dimensions that are NOT placed on rows or columns. These act as filters -- for example, fixing Scenario to "Actual" and Time to "2024M12" while Account is on rows and Entity is on columns. POV does not define row/column layout (A), background color (C), or server connections (D).
</details>

---

### Question 18 (Medium)
**300.3.2** | Difficulty: Medium

A Cube View needs to visually distinguish header rows from data rows using different background colors. How is this achieved?

A) By editing the HTML source code of the dashboard
B) By applying row-level style properties or using conditional formatting rules that target header member types
C) By changing the operating system's display settings
D) By exporting to Excel and formatting there

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream Cube Views support row-level styling where administrators can assign different background colors, fonts, and formatting to header/parent rows versus detail/data rows. This is done through style properties or conditional formatting. Editing HTML (A) is not supported, OS display settings (C) affect the entire screen, and exporting to Excel (D) defeats in-app formatting purposes.
</details>

---

### Question 19 (Hard)
**300.3.3** | Difficulty: Hard

A developer configures Drill-back on a Cube View to display source journal entries. The Drill-back works correctly for the Actual scenario but returns no data for the Budget scenario, even though budget data exists. What is the most likely cause?

A) Drill-back only works for Actual scenarios by design
B) The Drill-back Business Rule or mapping is filtering by Scenario and either does not have a condition for the Budget scenario or the budget data was loaded through a different source that does not populate the drill-back staging tables
C) Budget data cannot be stored in OneStream cubes
D) The user needs to run a special report before Drill-back works for Budget

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Drill-back depends on the Business Rule logic and source data mappings. If budget data was loaded through a different integration path that does not populate the drill-back staging tables, or if the Drill-back rule has conditional logic that only handles the Actual scenario, then Budget drill-back returns empty results. Drill-back is not limited to Actual (A), Budget data is fully supported in cubes (C), and no special report is required (D).
</details>

---

### Question 20 (Easy)
**300.3.4** | Difficulty: Easy

In what programming language are XFBRs (eXtensible Finance Business Rules) typically written in OneStream?

A) Python
B) VB.NET or C#
C) JavaScript
D) SQL

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFBRs in OneStream are written in VB.NET or C#, leveraging the .NET framework. These Business Rules execute server-side within the OneStream platform. They are not written in Python (A), JavaScript (C), or pure SQL (D), although they can interact with data stores.
</details>

---

### Question 21 (Medium)
**300.3.5** | Difficulty: Medium

An administrator enables sparse row suppression on a Cube View but users complain that some rows they need to see (with legitimately zero values) are now hidden. What is the best solution?

A) Disable sparse row suppression entirely
B) Configure the suppression to suppress only NoData rows while preserving rows with explicitly stored zero values, or use member-level suppression overrides to protect specific rows
C) Ask users to manually add the missing rows each time they view the report
D) Replace the Cube View with a Spreadsheet report

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best solution is to fine-tune the suppression settings. OneStream distinguishes between NoData (truly empty) and stored zeros. Configuring suppression to hide only NoData rows, or applying member-level overrides to always show specific critical rows, preserves meaningful zero-value rows while still cleaning up truly empty ones. Disabling suppression entirely (A) clutters the report. Manual row addition (C) is impractical. Switching to a Spreadsheet (D) is unnecessary.
</details>

---

### Question 22 (Hard)
**300.3.4** | Difficulty: Hard

An XFBR is used to create a custom "Variance %" column in a Cube View that divides (Actual - Budget) by Budget. The column displays correct results for most rows but shows "#DIV/0!" for rows where Budget is zero. How should the developer address this?

A) Remove the Variance % column entirely
B) Add a conditional check in the XFBR code to handle division by zero, returning zero, N/A, or a dash when the Budget value is zero
C) Ask users to ignore the error cells
D) Set all Budget values to a minimum of 1 to avoid zero division

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The developer should add defensive coding in the XFBR to check whether the Budget denominator is zero before performing division. When Budget is zero, the code should return an appropriate value such as 0, "N/A," or a dash. Removing the column (A) eliminates useful functionality, ignoring errors (C) is unprofessional, and artificially setting budget values (D) corrupts the data.
</details>

---

### Question 23 (Medium)
**300.3.6** | Difficulty: Medium

A multinational company has users in France, Germany, and Japan. They want Account member descriptions to appear in each user's native language in Cube Views. What must be configured?

A) Three separate OneStream applications, one per country
B) Alternate language descriptions for Account members in French, German, and Japanese, and each user's language preference set in their profile
C) A Google Translate API integration
D) Three separate Cube Views with hardcoded labels in each language

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream supports multiple alternate language descriptions per dimension member. The administrator enters translated descriptions for each language and sets each user's preferred language in their profile. The Cube View then automatically displays descriptions in the user's language. Separate applications (A), translation APIs (C), and separate hardcoded Cube Views (D) are unnecessary and unmaintainable approaches.
</details>

---

### Question 24 (Easy)
**300.3.2** | Difficulty: Easy

Which of the following is a valid number format that can be applied to data cells in a Cube View?

A) Cells can only display raw unformatted numbers
B) Currency format with thousands separators and decimal places (e.g., $1,234.56)
C) Cells can only display text, not numbers
D) Numbers can only be displayed as Roman numerals

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube View cells support standard number formatting including currency symbols, thousands separators, decimal places, percentage formats, and custom format strings. They are not limited to raw numbers (A), text-only (C), or Roman numerals (D).
</details>

---

### Question 25 (Medium)
**300.3.3** | Difficulty: Medium

What configuration is required to enable DrillDown functionality on a specific dimension in a Cube View?

A) DrillDown is always enabled automatically on all dimensions with no configuration needed
B) The dimension must be placed on rows or columns and the Cube View must have DrillDown enabled in its properties, with the member hierarchy properly configured in the dimension metadata
C) DrillDown requires a separate license add-on
D) DrillDown only works when the Cube View is exported to PDF

<details>
<summary>Show answer</summary>

**Correct answer: B)**

DrillDown requires that the dimension is placed on rows (or columns), the Cube View's DrillDown property is enabled, and the dimension hierarchy is properly configured in the metadata so there are child members to drill into. It is not always automatically enabled (A), does not require a separate license (C), and works within the application, not in PDF exports (D).
</details>

---

### Question 26 (Hard)
**300.3.1** | Difficulty: Hard

A Cube View is configured with Account on rows and Entity on columns. The administrator wants to add a second set of columns showing the same Entity members but for a different Scenario (Actual vs Budget side by side). How can this be achieved?

A) It is impossible to show multiple Scenarios as columns in a single Cube View
B) Configure a nested column structure that places Scenario as an outer column dimension and Entity as an inner column dimension, creating a cross-product of Scenario and Entity columns
C) Create two separate Cube Views and manually align them side by side
D) Export the data to Excel and arrange the columns there

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream Cube Views support nested (multi-dimensional) column structures where one dimension serves as the outer grouping and another as the inner detail. Placing Scenario as the outer column and Entity as the inner column creates a layout showing each Entity's data under both Actual and Budget headers. This is achievable in a single Cube View (A is incorrect). Separate aligned Cube Views (C) and Excel export (D) are workarounds, not the intended approach.
</details>

---

### Question 27 (Medium)
**300.3.5** | Difficulty: Medium

What is the performance benefit of enabling sparse row suppression in a Cube View with a large number of accounts?

A) It reduces the physical size of the database
B) It reduces the number of rows rendered in the browser, improving load time and readability by showing only rows that contain data
C) It speeds up the cube processing job
D) It increases the server's available RAM

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Sparse row suppression improves performance by reducing the number of rows that need to be rendered and transmitted to the browser. When a dimension has thousands of members but data exists for only a subset, suppression eliminates empty rows from the display, improving both load time and readability. It does not affect database size (A), cube processing speed (C), or server RAM (D).
</details>
