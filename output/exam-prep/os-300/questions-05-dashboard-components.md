# Question Bank - Section 5: Dashboard Components (20% of exam)

## Objectives
- **300.5.1:** Configure and use dashboard events for interactivity
- **300.5.2:** Select appropriate components for specific use cases
- **300.5.3:** Use Report Designer (Studio) within dashboards
- **300.5.4:** Configure BI Viewer with Data Adapters for dashboard reporting

---

## Objective 300.5.1: Configure and use dashboard events

### Question 1 (Easy)
**300.5.1** | Difficulty: Easy

What is a dashboard event in OneStream?

A) A scheduled batch process that runs overnight
B) An action triggered by user interaction (such as a button click or cell selection) that causes a response in the dashboard
C) A system error message displayed to the user
D) A calendar entry in the OneStream scheduling module

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dashboard events are actions triggered by user interactions -- such as button clicks, cell selections, parameter changes, or component loads -- that cause a defined response in the dashboard. Responses can include navigating to another dashboard, refreshing data, running a Business Rule, or updating parameters. They are not batch processes (A), error messages (C), or calendar entries (D).
</details>

---

### Question 2 (Medium)
**300.5.1** | Difficulty: Medium

A developer wants to execute a Business Rule when a user clicks a button on a dashboard. Which event configuration is required?

A) Configure a Cube View DrillDown on the button
B) Assign a Button Click event that calls the Business Rule, specifying the rule name and any required parameters
C) Set the button's font color to red to indicate it runs a rule
D) Create an external Windows batch file linked to the button

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To execute a Business Rule on button click, the developer configures a Button Click event that specifies which Business Rule to invoke and passes any required parameters. This is the standard event-driven pattern in OneStream dashboards. DrillDown (A) is for data navigation, font color (C) is cosmetic, and external batch files (D) are not linked to dashboard buttons.
</details>

---

### Question 3 (Hard)
**300.5.1** | Difficulty: Hard

A dashboard has a cascading event setup: changing Parameter A triggers a refresh of Component B, which triggers an update of Component C. Users report that Component C intermittently shows stale data. What is the most likely cause?

A) The server does not support cascading events
B) The event chain has a timing issue where Component C refreshes before Component B has finished loading its updated data
C) Component C is on a different monitor than Components A and B
D) The dashboard has too many colors applied

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Intermittent stale data in cascading events typically indicates a timing or sequencing issue. Component C may be refreshing before Component B has fully completed its data load, causing it to read stale data. The developer should ensure proper event sequencing or add appropriate dependencies between the refresh events. OneStream supports cascading events (A), monitor placement (C) and colors (D) are irrelevant.
</details>

---

## Objective 300.5.2: Select appropriate components for use cases

### Question 4 (Easy)
**300.5.2** | Difficulty: Easy

Which dashboard component is best suited for displaying a financial income statement with expandable hierarchy rows?

A) A Chart component
B) A Cube View component
C) A Text Label component
D) An Image component

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Cube View is the ideal component for a financial income statement because it displays hierarchical data in a grid format with support for expandable rows (DrillDown), formatted columns, and calculated rows. Charts (A) are better for visual trends, Text Labels (C) display static text, and Images (D) display graphics.
</details>

---

### Question 5 (Medium)
**300.5.2** | Difficulty: Medium

An end user needs a dashboard that shows a revenue trend over 12 months with the ability to click on a data point and see the detailed breakdown. Which combination of components is most appropriate?

A) Two Image components side by side
B) A Chart component for the trend visualization linked via an event to a Cube View that shows the detailed breakdown
C) A single Text Label with the data typed in manually
D) Twelve separate Spreadsheet reports, one per month

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best approach combines a Chart component (for visual trend display) with a Cube View (for detailed data breakdown), linked through a dashboard event so that clicking a chart data point passes context to the Cube View. Images (A) and text labels (C) cannot display dynamic data, and twelve separate Spreadsheets (D) would be impractical and poorly designed.
</details>

---

### Question 6 (Medium)
**300.5.2** | Difficulty: Medium

When should an administrator choose a Spreadsheet component over a Cube View for a dashboard report?

A) When the report requires only a simple grid of cube data with standard row/column dimensions
B) When the report requires a highly customized layout with mixed data sources, free-form positioning, and Excel-like formatting flexibility
C) When the report has fewer than five rows
D) When the dashboard is accessed only by administrators

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Spreadsheet components are preferred when the report demands highly customized layouts, mixed data sources, free-form cell positioning, and Excel-like formatting flexibility that goes beyond what a standard Cube View offers. For simple grid layouts (A), a Cube View is sufficient and more efficient. Row count (C) and user role (D) are not deciding factors.
</details>

---

## Objective 300.5.3: Use Report Designer (Studio) within dashboards

### Question 7 (Easy)
**300.5.3** | Difficulty: Easy

What is Report Designer (Studio) in OneStream?

A) A third-party application that must be purchased separately
B) A built-in visual design tool for creating and editing dashboard layouts, components, and their properties
C) A database administration console
D) A code compiler for Business Rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Designer (Studio) is OneStream's built-in visual design tool that allows administrators and developers to create and edit dashboard layouts by placing components, configuring their properties, setting events, and defining the overall dashboard appearance. It is not third-party (A), a database console (C), or a code compiler (D).
</details>

---

### Question 8 (Medium)
**300.5.3** | Difficulty: Medium

In Report Designer (Studio), what is the function of the component property panel?

A) It displays the server's CPU usage
B) It shows and allows editing of the selected component's properties such as size, position, data binding, formatting, and event handlers
C) It lists all users currently logged into the application
D) It displays the application's error log

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The component property panel in Report Designer displays all configurable properties of the selected component, including its size, position, data binding, formatting options, visibility settings, and event handlers. This is the primary interface for configuring component behavior. It does not show CPU usage (A), user lists (C), or error logs (D).
</details>

---

### Question 9 (Hard)
**300.5.3** | Difficulty: Hard

A developer uses Report Designer to create a complex dashboard with multiple overlapping components and conditional visibility rules. During testing, one component that should be hidden based on a parameter value remains visible. What should the developer check?

A) The server's disk space
B) The component's visibility expression logic, ensuring the parameter reference and comparison values are correct and the expression evaluates as expected
C) Whether the user's browser supports HTML5
D) The order of components in the Windows Task Manager

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a conditionally visible component does not hide properly, the developer should review the visibility expression to ensure the parameter reference is correct, the comparison operator is appropriate, and the expected value matches what the parameter actually returns. Common issues include case sensitivity, wrong parameter names, or incorrect comparison logic. Disk space (A), browser compatibility (C), and Task Manager (D) are unrelated.
</details>

---

## Objective 300.5.4: Configure BI Viewer with Data Adapters

### Question 10 (Easy)
**300.5.4** | Difficulty: Easy

What is the BI Viewer component in OneStream used for?

A) Editing the application's metadata
B) Displaying interactive reports and visualizations that are powered by Data Adapters
C) Monitoring network traffic
D) Creating user accounts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The BI Viewer is a dashboard component that renders interactive reports and visualizations driven by Data Adapters. Data Adapters provide the data feed (from cubes, staging, or custom queries), and the BI Viewer presents that data in charts, grids, or other visual formats. It is not for metadata editing (A), network monitoring (C), or user management (D).
</details>

---

### Question 11 (Medium)
**300.5.4** | Difficulty: Medium

What is the role of a Data Adapter when used with the BI Viewer?

A) It converts OneStream data into a proprietary binary format
B) It serves as the data source definition that retrieves and structures data for the BI Viewer to render
C) It physically moves data between servers
D) It encrypts dashboard content for security

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Data Adapter defines the data source and query logic that retrieves, filters, and structures data for consumption by the BI Viewer. It acts as the bridge between the data store (cube, staging, relational) and the visual component. It does not convert to binary (A), move data between servers (C), or encrypt content (D).
</details>

---

### Question 12 (Hard)
**300.5.4** | Difficulty: Hard

A BI Viewer is configured with a Data Adapter that queries staging data. The visualization renders correctly for small data sets but times out for larger ones. The Data Adapter query uses no filters. What is the best remediation approach?

A) Increase the server's physical RAM to 1 TB
B) Add parameter-driven filters to the Data Adapter query to reduce the result set, and consider implementing pagination or aggregation at the query level
C) Remove the BI Viewer and ask users to query the database manually
D) Disable the timeout setting entirely so the query can run indefinitely

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best approach is to optimize the Data Adapter query by adding parameter-driven filters (e.g., by Entity, Time period, or Scenario) to reduce the data set size, and implementing aggregation at the query level rather than returning raw detail rows. Simply adding RAM (A) does not fix an unfiltered query. Removing the component (C) eliminates functionality. Disabling timeouts (D) could lead to server resource exhaustion and poor user experience.
</details>

---

### Question 13 (Medium)
**300.5.4** | Difficulty: Medium

Which of the following Data Adapter types can be used with the BI Viewer?

A) Only cube-based Data Adapters
B) Cube-based, staging-based, and custom Business Rule Data Adapters
C) Only Microsoft Access database connections
D) Only flat file (CSV) Data Adapters

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The BI Viewer supports multiple Data Adapter types including cube-based adapters (for OLAP data), staging-based adapters (for relational/staging data), and custom Business Rule adapters (for programmatically defined data sets). It is not limited to cube-only (A), Access databases (C), or flat files (D).
</details>

---

### Question 14 (Easy)
**300.5.1** | Difficulty: Easy

Which of the following is a common dashboard event type in OneStream?

A) ServerReboot event
B) ParameterChanged event that fires when a user selects a new value in a parameter dropdown
C) PrinterJam event
D) NetworkTimeout event

<details>
<summary>Show answer</summary>

**Correct answer: B)**

ParameterChanged is a common dashboard event type that fires when a user selects a new value in a parameter dropdown. This event can trigger actions such as refreshing components, running Business Rules, or navigating to other dashboards. ServerReboot (A), PrinterJam (C), and NetworkTimeout (D) are not OneStream dashboard event types.
</details>

---

### Question 15 (Medium)
**300.5.2** | Difficulty: Medium

A dashboard needs to display a KPI scorecard showing key metrics with color-coded indicators (green, yellow, red) based on performance thresholds. Which component approach is most appropriate?

A) A Text Label with manually typed colored text
B) A Cube View or Spreadsheet with conditional formatting rules that apply color-coded backgrounds or icons based on data value thresholds
C) A static image of a traffic light
D) An audio component that plays different sounds for each threshold

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Cube View or Spreadsheet with conditional formatting is the ideal approach for KPI scorecards. Conditional formatting rules can automatically apply green, yellow, or red backgrounds (or icons) based on data value thresholds, creating a dynamic, data-driven scorecard. Static text (A), static images (C), and audio (D) cannot respond dynamically to data values.
</details>

---

### Question 16 (Hard)
**300.5.1** | Difficulty: Hard

A developer configures a dashboard event to run a Business Rule when a cell is selected in a Cube View. The event fires correctly but the Business Rule receives null parameter values. What is the most likely cause?

A) The Cube View does not support cell selection events
B) The event configuration does not correctly map the selected cell's dimension context (row member, column member) to the Business Rule's expected input parameters
C) Business Rules cannot receive parameters from dashboard events
D) The user's browser is blocking the parameter transfer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a Business Rule receives null parameters from a cell selection event, the most likely cause is that the event's parameter mapping is not correctly configured to pass the selected cell's dimension context (such as the row member name or column member name) to the Business Rule's expected input parameters. Cell selection events are supported (A), Business Rules can receive event parameters (C), and browsers do not block internal parameter transfers (D).
</details>

---

### Question 17 (Easy)
**300.5.3** | Difficulty: Easy

In Report Designer (Studio), what does the component toolbox provide?

A) A set of physical tools for hardware repair
B) A palette of available components (Cube Views, Charts, Buttons, Labels, etc.) that can be dragged onto the dashboard canvas
C) A list of all database tables in the application
D) A debug console for viewing error messages

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The component toolbox in Report Designer provides a palette of available dashboard components -- Cube Views, Charts, Buttons, Labels, Spreadsheets, Parameters, and more -- that developers can drag and drop onto the dashboard design canvas. It is not for hardware tools (A), database tables (C), or debug consoles (D).
</details>

---

### Question 18 (Medium)
**300.5.2** | Difficulty: Medium

An organization needs a dashboard that displays a pie chart of revenue distribution by region and allows users to click on a slice to see detailed accounts. Which components and configuration are needed?

A) Two Text Label components
B) A Chart component configured as a pie chart bound to revenue data by region, with a cell click event that navigates to or refreshes a Cube View showing account-level detail for the selected region
C) A single Image component with a picture of a pie chart
D) A Bound List parameter showing all regions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This requires a Chart component configured as a pie chart (showing revenue by region) with an interactive event -- when a user clicks a pie slice, the selected region is passed to a Cube View (or another component) that displays account-level detail. Text Labels (A) are static, an Image (C) is not interactive, and a Bound List (D) is just a parameter, not a visualization.
</details>

---

### Question 19 (Medium)
**300.5.3** | Difficulty: Medium

When using Report Designer (Studio), what is the purpose of the component anchoring feature?

A) It physically anchors the server to the desk
B) It defines how a component resizes and repositions relative to the dashboard boundaries when the browser window is resized
C) It prevents the component from being selected by the developer
D) It locks the component's data source to a specific cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Component anchoring in Report Designer controls how a component behaves when the dashboard is resized (e.g., when the browser window changes size). Anchoring to edges (top, bottom, left, right) determines whether the component stretches, stays fixed, or repositions proportionally. It is not about physical anchoring (A), selection prevention (C), or data source locking (D).
</details>

---

### Question 20 (Hard)
**300.5.4** | Difficulty: Hard

A BI Viewer is configured with a Data Adapter that returns a data set with 15 columns. The developer wants the BI Viewer to display only 5 of those columns and use the remaining 10 as hidden data for drill-through or tooltip details. How should this be configured?

A) Delete the 10 unwanted columns from the Data Adapter query
B) Configure the BI Viewer's column visibility settings to show only the 5 desired columns while retaining all 15 columns in the underlying data set for drill-through and tooltip binding
C) Create 10 separate Data Adapters, one per hidden column
D) Export the data to Excel and delete columns there

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The BI Viewer supports column visibility settings that control which columns are displayed versus hidden. Keeping all 15 columns in the data set but showing only 5 allows the hidden columns to be used for drill-through actions, tooltip details, or conditional formatting logic. Deleting columns from the query (A) removes the data entirely. Separate Data Adapters (C) and Excel export (D) are unnecessary.
</details>

---

### Question 21 (Easy)
**300.5.2** | Difficulty: Easy

Which dashboard component should be used to display a company logo or banner image at the top of a dashboard?

A) A Cube View component
B) An Image component
C) A Bound List parameter
D) A Data Adapter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An Image component is the appropriate choice for displaying static visual elements such as company logos, banners, or icons on a dashboard. Cube Views (A) are for data grids, Bound List parameters (C) are for user selection, and Data Adapters (D) are data source definitions, not visual components.
</details>

---

### Question 22 (Medium)
**300.5.1** | Difficulty: Medium

A developer configures an OnLoad event on a dashboard to run a Business Rule that pre-populates certain parameters based on the user's profile. What does the OnLoad event do?

A) It executes when the dashboard is deleted from the system
B) It fires automatically when the dashboard first loads in the user's browser, before any user interaction occurs
C) It triggers only when the user clicks a specific button
D) It runs once per year as a scheduled task

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The OnLoad event fires automatically when the dashboard first loads in the user's browser, before any manual user interaction. This makes it ideal for initializing parameters, setting default values, or running pre-load Business Rules based on the user's profile. It is not triggered by deletion (A), button clicks (C), or annual scheduling (D).
</details>

---

### Question 23 (Hard)
**300.5.3** | Difficulty: Hard

A developer creates a dashboard in Report Designer with components that use absolute pixel positioning. The dashboard looks correct on a 1920x1080 monitor but components overlap on a 1366x768 screen. What is the best approach to fix this?

A) Require all users to purchase 1920x1080 monitors
B) Redesign the dashboard using relative positioning, anchoring, and docking instead of absolute pixel coordinates, or use a responsive layout container that adjusts to available screen space
C) Reduce the font size of all components to 6pt
D) Remove components until they no longer overlap

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Using absolute pixel positioning causes layout issues across different screen resolutions. The correct approach is to redesign using relative positioning, component anchoring (which stretches or repositions components relative to dashboard edges), and docking. This ensures the dashboard adapts to different screen sizes. Requiring specific monitors (A) is impractical, reducing font size (C) affects readability, and removing components (D) reduces functionality.
</details>

---

### Question 24 (Medium)
**300.5.4** | Difficulty: Medium

What is the advantage of using a custom Business Rule Data Adapter with the BI Viewer instead of a standard cube-based Data Adapter?

A) Custom Business Rule Data Adapters are always faster than cube-based adapters
B) Custom Business Rule Data Adapters can combine data from multiple sources, apply complex transformations, and return custom-shaped result sets that go beyond what standard cube queries provide
C) Custom Business Rule Data Adapters do not require any coding
D) Custom Business Rule Data Adapters bypass all security checks

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Custom Business Rule Data Adapters allow developers to write server-side code that can combine data from multiple sources (cube, staging, external), apply complex business logic, perform custom calculations, and return a result set shaped exactly to the BI Viewer's needs. They are not always faster (A), they do require coding (C), and they respect OneStream's security framework (D).
</details>

---

### Question 25 (Easy)
**300.5.2** | Difficulty: Easy

When should a developer use a Button component on a dashboard?

A) To display a read-only financial report
B) To provide an interactive element that users can click to trigger an action such as running a Business Rule, navigating to another dashboard, or submitting data
C) To store data in the cube
D) To create a database backup

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Button components provide interactive elements on dashboards that users can click to trigger defined actions -- running Business Rules, navigating to other dashboards, submitting data entry, refreshing components, or executing workflow steps. Buttons are not for displaying reports (A), storing data directly (C), or creating backups (D).
</details>
