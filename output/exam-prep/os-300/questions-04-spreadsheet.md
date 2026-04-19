# Question Bank - Section 4: Spreadsheet (12% of exam)

## Objectives
- **300.4.1:** Describe and configure Spreadsheet Reports usage in OneStream dashboards

---

## Objective 300.4.1: Describe and configure Spreadsheet Reports usage

### Question 1 (Easy)
**300.4.1** | Difficulty: Easy

What is the Spreadsheet component in OneStream primarily used for?

A) Managing server hardware configurations
B) Creating Excel-like tabular reports within OneStream dashboards that can combine data from multiple sources
C) Writing Visual Basic scripts for data processing
D) Configuring network firewall rules for the application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Spreadsheet component in OneStream provides an Excel-like reporting experience within dashboards. It allows users and developers to create tabular reports that can pull data from the cube, perform calculations, and combine multiple data sources into a single report layout. It is not for server management (A), scripting (C), or network configuration (D).
</details>

---

### Question 2 (Easy)
**300.4.1** | Difficulty: Easy

Which of the following is a key advantage of using a Spreadsheet report within a OneStream dashboard instead of exporting data to Excel?

A) Spreadsheet reports can run macros from the local computer
B) Spreadsheet reports maintain live connection to the cube data and update dynamically with POV changes
C) Spreadsheet reports support VBA programming
D) Spreadsheet reports automatically email themselves to all users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Spreadsheet reports within OneStream dashboards maintain a live connection to the cube data. When users change the POV or parameters, the report updates dynamically without requiring export/import cycles. They do not run local macros (A), support VBA (C), or auto-email (D).
</details>

---

### Question 3 (Medium)
**300.4.1** | Difficulty: Medium

When designing a Spreadsheet report, how does a developer reference cube data in a cell?

A) By typing a SQL SELECT statement directly into the cell
B) By using OneStream's GetDataCell or adapter functions that specify the dimension members for the data intersection
C) By pasting values copied from an external Excel workbook
D) By linking to a CSV file stored on the server

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Developers reference cube data in Spreadsheet cells using OneStream functions such as GetDataCell or adapter-based functions that specify the exact dimension member intersection (Scenario, Time, Account, Entity, etc.). This creates a live formula that retrieves data from the cube. SQL statements (A), pasted values (C), and CSV links (D) are not the standard approach.
</details>

---

### Question 4 (Medium)
**300.4.1** | Difficulty: Medium

A Spreadsheet report needs to display both actual financial data from the cube and manually entered commentary text in the same layout. How can this be achieved?

A) It cannot; Spreadsheet reports only support numeric data
B) By using data cells for cube data and standard text cells for commentary, combining both in the same Spreadsheet layout
C) By creating two separate dashboards and placing them side by side
D) By converting all numeric data to text format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Spreadsheet reports in OneStream support mixed content. Developers can configure some cells to pull live cube data while other cells contain static text, labels, or commentary. This flexibility allows rich report layouts combining financial data with narrative context. Spreadsheets are not limited to numeric data (A), and splitting into two dashboards (C) or converting data types (D) is unnecessary.
</details>

---

### Question 5 (Medium)
**300.4.1** | Difficulty: Medium

Which of the following is true about the relationship between Spreadsheet reports and dashboard parameters?

A) Spreadsheet reports cannot interact with dashboard parameters
B) Spreadsheet reports can reference dashboard parameters to dynamically change which data intersections are displayed
C) Spreadsheet reports override all dashboard parameters with their own fixed values
D) Dashboard parameters must be deleted before adding a Spreadsheet report

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Spreadsheet reports can reference dashboard parameters, allowing data references within the spreadsheet to change dynamically based on user selections. For example, changing the Entity parameter in the POV bar updates all entity-dependent cells in the Spreadsheet. They do not ignore parameters (A), override them (C), or require their deletion (D).
</details>

---

### Question 6 (Hard)
**300.4.1** | Difficulty: Hard

A developer creates a Spreadsheet report with multiple tabs representing different reporting views. The report works correctly in design mode but shows incorrect data at runtime when the POV is changed. What is the most likely cause?

A) OneStream does not support multi-tab Spreadsheet reports
B) Some cell formulas on secondary tabs are using hardcoded member references instead of parameter-bound references, so they do not respond to POV changes
C) The user's screen resolution is incompatible with multiple tabs
D) The server ran out of RAM during rendering

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a Spreadsheet works in design mode but shows incorrect data at runtime after POV changes, the most common cause is that some cell references use hardcoded dimension members instead of parameter-bound references. The hardcoded cells do not update when the POV changes. Multi-tab Spreadsheets are supported (A), screen resolution (C) does not affect data, and RAM issues (D) would cause errors, not incorrect data.
</details>

---

### Question 7 (Easy)
**300.4.1** | Difficulty: Easy

What formatting capabilities are available in OneStream Spreadsheet reports?

A) No formatting is available; all data appears in plain text
B) Cell formatting including fonts, colors, borders, number formats, and merged cells similar to Excel
C) Only bold and italic text
D) Only background colors without any text formatting

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream Spreadsheet reports provide rich formatting capabilities similar to Microsoft Excel, including font styles, colors, borders, number formats, cell merging, and alignment options. This enables professional report presentation within the dashboard. They are not limited to plain text (A), basic formatting only (C), or only colors (D).
</details>

---

### Question 8 (Medium)
**300.4.1** | Difficulty: Medium

An administrator needs to create a board-level financial report that includes variance analysis, sparkline-style trends, and conditional color coding. Which OneStream reporting component is most suitable?

A) A plain Text Box parameter
B) The Spreadsheet component, which supports calculations, conditional formatting, and flexible layout design
C) A single Bound List parameter
D) The Workspace navigation panel

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Spreadsheet component is ideal for complex board-level reports that require variance calculations, conditional formatting (color coding), and flexible layout design. It provides the Excel-like capability needed for sophisticated financial presentations. Text Box parameters (A), Bound List parameters (C), and navigation panels (D) are not reporting components.
</details>

---

### Question 9 (Hard)
**300.4.1** | Difficulty: Hard

A Spreadsheet report retrieves data using GetDataCell functions for hundreds of intersections. Users complain the report takes over 30 seconds to load. What is the most effective optimization strategy?

A) Remove all formatting from the Spreadsheet
B) Replace individual GetDataCell calls with a range-based data retrieval approach or use a Data Adapter to bulk-load data, reducing the number of round trips to the server
C) Increase the font size so fewer rows are visible on screen
D) Convert the Spreadsheet into a static image

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Performance issues with Spreadsheets often stem from many individual cell-by-cell data retrieval calls, each requiring a server round trip. The solution is to use range-based or bulk data retrieval approaches (such as Data Adapters) that fetch multiple data points in fewer round trips. Removing formatting (A) provides negligible performance improvement. Increasing font size (C) does not reduce data fetching. Converting to a static image (D) eliminates the live data capability.
</details>

---

### Question 10 (Medium)
**300.4.1** | Difficulty: Medium

How can a Spreadsheet report be made available for end users to print or export?

A) Users must take a screenshot of the browser window
B) OneStream provides built-in export options that allow users to export Spreadsheet reports to Excel or PDF directly from the dashboard
C) The administrator must manually email the report to each user
D) Spreadsheet reports cannot be exported or printed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream provides built-in export functionality that allows users to export Spreadsheet reports to Excel or PDF directly from the dashboard interface. This eliminates the need for screenshots (A), manual distribution (C), and supports the common need for offline report sharing. Spreadsheets are fully exportable (D).
</details>

---

### Question 11 (Easy)
**300.4.1** | Difficulty: Easy

What is a Table View in the context of OneStream Spreadsheet reporting?

A) A view that displays database table structures from SQL Server
B) A tabular display within a Spreadsheet that presents data in a structured row-and-column format, often resembling a simple data grid
C) A physical table placed in a conference room for report review
D) A view that shows only the Spreadsheet's metadata properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Table View within OneStream Spreadsheet reporting presents data in a structured, grid-like row-and-column format. It provides a clean tabular layout for displaying financial or operational data within the Spreadsheet component. It is not a SQL database structure view (A), a physical object (C), or a metadata display (D).
</details>

---

### Question 12 (Medium)
**300.4.1** | Difficulty: Medium

When using the OneStream Excel Add-In, what is its primary advantage over standard Spreadsheet reports within dashboards?

A) It allows users to work within Microsoft Excel while maintaining a live connection to OneStream cube data, enabling ad-hoc analysis in a familiar environment
B) It replaces the need for OneStream entirely
C) It is faster than any other reporting method in all scenarios
D) It allows users to modify the OneStream application's metadata directly from Excel

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The Excel Add-In allows users to work within the familiar Microsoft Excel environment while maintaining a live connection to OneStream cube data. Users can build ad-hoc analyses, custom reports, and what-if scenarios using Excel's features while pulling real-time data from OneStream. It does not replace OneStream (B), is not universally faster (C), and does not allow direct metadata modification (D).
</details>

---

### Question 13 (Hard)
**300.4.1** | Difficulty: Hard

A Spreadsheet report uses conditional formatting to highlight cells where actual results deviate from budget by more than 10%. The formatting works for most cells but incorrectly highlights some cells that are within tolerance. What should the developer investigate?

A) The user's monitor calibration
B) Whether the conditional formatting formula correctly handles the absolute value of the variance and accounts for edge cases such as zero budget values or NoData cells
C) Whether the server's CPU is overclocked
D) Whether the Spreadsheet has too many colors applied

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Incorrect conditional formatting usually stems from formula logic issues. The developer should verify that the variance formula correctly calculates the absolute percentage deviation and handles edge cases: zero budget values (which cause division errors), NoData cells (which may evaluate unexpectedly), and sign conventions. Monitor calibration (A), CPU overclocking (C), and color count (D) are unrelated.
</details>

---

### Question 14 (Easy)
**300.4.1** | Difficulty: Easy

Can a Spreadsheet report in OneStream include charts or graphs?

A) No, Spreadsheet reports only support tabular data
B) Yes, Spreadsheet reports can include embedded charts that visualize the data within the Spreadsheet
C) Only if the Spreadsheet is exported to PowerPoint first
D) Charts are available only in the BI Viewer, never in Spreadsheets

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream Spreadsheet reports can include embedded charts that visualize data directly within the Spreadsheet layout. This allows developers to combine tabular data with graphical representations in a single component. They are not limited to tabular data only (A), do not require PowerPoint export (C), and charts are not exclusive to the BI Viewer (D).
</details>

---

### Question 15 (Medium)
**300.4.1** | Difficulty: Medium

A Spreadsheet report needs to display data from two different Scenarios (Actual and Budget) side by side. How is this typically implemented?

A) Create two separate Spreadsheet reports on different dashboards
B) Use GetDataCell or adapter functions with different Scenario member references in adjacent columns within the same Spreadsheet layout
C) Ask users to switch the POV back and forth and remember the values
D) Merge the two Scenarios into a single Scenario in the cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Within a single Spreadsheet layout, different columns can reference different Scenario members in their data retrieval functions. One column uses "Actual" and the adjacent column uses "Budget" in the GetDataCell Scenario parameter, creating a side-by-side comparison. Separate Spreadsheets (A) are unnecessary. Asking users to remember values (C) is impractical. Merging Scenarios (D) would corrupt the data model.
</details>

---

### Question 16 (Medium)
**300.4.1** | Difficulty: Medium

What is the role of named ranges in a OneStream Spreadsheet report?

A) They are used to configure server network settings
B) They allow developers to assign meaningful names to cell ranges, making formulas more readable and enabling easier reference in Business Rules or data retrieval functions
C) They automatically encrypt the Spreadsheet data
D) They determine the print order of multiple Spreadsheets

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Named ranges in Spreadsheet reports allow developers to assign descriptive names to specific cell ranges. This makes formulas more readable, simplifies maintenance, and enables Business Rules or adapter functions to reference ranges by name rather than cell coordinates. They are not for network settings (A), encryption (C), or print ordering (D).
</details>

---

### Question 17 (Hard)
**300.4.1** | Difficulty: Hard

A developer builds a Spreadsheet report that supports data input (write-back to the cube). Users can enter budget values in specific cells. After saving, some values are written correctly but others are silently ignored. What is the most likely cause?

A) The server ran out of memory during the save operation
B) The write-back cells that are ignored have incorrect or missing dimension intersection mappings, causing the save operation to skip those cells because it cannot determine where to write the data
C) Write-back Spreadsheets can only save up to 10 cells at a time
D) The user's keyboard is malfunctioning

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When some write-back cells save correctly and others are silently ignored, the most common cause is that the ignored cells have incorrect or missing dimension member mappings. Without a complete and valid dimension intersection (Scenario, Time, Account, Entity, etc.), the save operation cannot determine the target location in the cube and skips those cells. There is no 10-cell limit (C), memory issues (A) would cause errors rather than silent skipping, and keyboard issues (D) are irrelevant.
</details>

---

### Question 18 (Easy)
**300.4.1** | Difficulty: Easy

Which of the following best describes the OneStream Excel Add-In?

A) A standalone desktop application that replaces Microsoft Excel
B) A plugin installed within Microsoft Excel that allows users to connect to OneStream and retrieve or submit cube data directly from Excel
C) A web browser extension for viewing Excel files online
D) A tool for converting Excel files to PDF format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The OneStream Excel Add-In is a plugin installed within Microsoft Excel that enables a direct connection to the OneStream platform. Users can retrieve live cube data, build reports, and even submit data back to OneStream without leaving the Excel environment. It does not replace Excel (A), is not a browser extension (C), and is not a file converter (D).
</details>

---

### Question 19 (Medium)
**300.4.1** | Difficulty: Medium

An administrator needs to create a Spreadsheet report where certain rows are dynamically expanded based on the Entity hierarchy (showing one row per child entity). How can this be achieved?

A) Manually create a separate row for every possible entity in the system
B) Use a row repeater or dynamic range feature within the Spreadsheet that iterates over a member list expression to generate rows automatically
C) Export the data to a CSV and reimport it with the correct rows
D) Ask each entity's manager to add their own row manually

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Spreadsheet reports support dynamic row generation through repeater or range features that iterate over a member list expression. This automatically creates one row per member returned by the expression (e.g., children of a parent entity), keeping the report dynamic as the hierarchy changes. Manual row creation (A) is unmaintainable, CSV exports (C) break live connections, and manual user additions (D) are impractical.
</details>

---

### Question 20 (Hard)
**300.4.1** | Difficulty: Hard

A Spreadsheet report is used for a monthly close checklist that combines cube data (close status by entity) with manually entered sign-off dates. After upgrading the OneStream platform, the sign-off date cells lose their values while cube data cells continue to work. What is the most likely cause?

A) The upgrade deleted all data from the cube
B) The manually entered values were stored in a location (such as a dashboard data cache or local storage) that was cleared during the upgrade, rather than being persisted to a durable data store
C) Date fields are no longer supported after the upgrade
D) The Spreadsheet component was removed from the platform

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Manually entered values in Spreadsheet cells need to be persisted to a durable data store (such as a cube or database table) to survive platform upgrades. If the sign-off dates were stored only in a cache, session storage, or local storage that gets cleared during an upgrade, those values are lost while cube-sourced data remains intact. The cube data was not deleted (A), date fields are supported (C), and the Spreadsheet component was not removed (D).
</details>

---

### Question 21 (Medium)
**300.4.1** | Difficulty: Medium

What is the difference between a Spreadsheet report viewed within a OneStream dashboard and the same report accessed via the Excel Add-In?

A) There is no difference; they are identical in every way
B) The dashboard Spreadsheet renders within the browser with OneStream's UI controls, while the Excel Add-In version renders within Microsoft Excel, offering Excel's full feature set for ad-hoc manipulation
C) The Excel Add-In version cannot access cube data
D) The dashboard version supports more advanced formulas than Excel

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When viewed in a dashboard, the Spreadsheet renders within the OneStream web interface with its native controls. When accessed via the Excel Add-In, the same data is available within Microsoft Excel, giving users access to Excel's full feature set (pivot tables, complex charting, macros) for ad-hoc analysis. They are not identical (A), the Add-In does access cube data (C), and Excel generally has more formula capabilities (D).
</details>

---

### Question 22 (Easy)
**300.4.1** | Difficulty: Easy

Which of the following is true about cell protection in OneStream Spreadsheet reports?

A) All cells in a Spreadsheet are always editable by all users
B) Cells can be locked or protected to prevent end users from modifying certain cells while allowing input in designated data entry cells
C) Cell protection is only available when the Spreadsheet is exported to Excel
D) Cell protection requires a separate third-party add-on

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream Spreadsheet reports support cell protection, allowing developers to lock formula cells, label cells, and other non-input cells while leaving designated data entry cells editable. This prevents accidental modification of report structure. Not all cells are always editable (A), protection works within the dashboard (C), and it is a built-in feature (D).
</details>
