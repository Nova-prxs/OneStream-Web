# Question Bank - Section 5: Tools (9% of exam)

## Objective 201.5.1: Data Management Steps and Their Use Cases

### Question 1 (Easy)
**201.5.1** | Difficulty: Easy

What is the correct hierarchical structure of Data Management, from highest to lowest level?

A) Step > Sequence > Group > Profile
B) Profile > Group > Sequence > Step
C) Group > Profile > Sequence > Step
D) Profile > Sequence > Group > Step

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The hierarchical structure is: Profile (organizes Groups for presentation) > Group (top-level container) > Sequence (ordered series of Steps) > Step (individual unit of work). Option A is inverted, and options C and D mix the order incorrectly.
</details>

---

### Question 2 (Easy)
**201.5.1** | Difficulty: Easy

What are the 6 built-in Data Management Step types in OneStream?

A) Calculate, Clear Data, Copy Data, Custom Calculate, Execute Business Rule, Export Data
B) Calculate, Clear Data, Copy Data, Import Data, Execute Business Rule, Export Data
C) Calculate, Clear Data, Copy Data, Custom Calculate, Reset Scenario, Export Data
D) Calculate, Clear Data, Move Data, Custom Calculate, Execute Business Rule, Export Report

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The 6 built-in types are: Calculate, Clear Data, Copy Data, Custom Calculate, Execute Business Rule, and Export Data. Import Data is not a Step type (B). Reset Scenario is an additional Step but not one of the 6 main types (C). Move Data and Export Report are not built-in main Step types (D).
</details>

---

### Question 3 (Medium)
**201.5.1** | Difficulty: Medium

What is the main difference between "Calculate" and "Consolidate" as calculation types within a Calculate Step?

A) Calculate executes translations; Consolidate only executes local calculations
B) Calculate executes calculation at the Entity level within the Local member; Consolidate executes Calculate, Translate, and completes calculations across the entire Consolidation Dimension
C) Both do the same thing but Consolidate is faster
D) Calculate works with a single Cube; Consolidate works with multiple Cubes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Calculate executes calculation at the Entity level within the Local member of the Consolidation Dimension, without translating or consolidating. Consolidate is the most complete: it executes Calculate, then Translate, and finally completes calculations across the entire Consolidation Dimension. Option A reverses the functions, and option D does not reflect the actual behavior.
</details>

---

### Question 4 (Medium)
**201.5.1** | Difficulty: Medium

An administrator needs to run a quick calculation on a slice of data without generating audit information for each cell. What type of Step should they use?

A) Calculate
B) Execute Business Rule
C) Custom Calculate
D) Copy Data

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Custom Calculate executes a calculation on a slice of data within one or more Data Units using a Finance Business Rule. Its main advantage is that it does not create audit information for each cell, which makes it faster than Copy Data. Calculate executes built-in calculations but generates audit information. Execute Business Rule runs an Extensibility Business Rule, not a calculation on slices.
</details>

---

### Question 5 (Medium)
**201.5.1** | Difficulty: Medium

**Scenario:** A user tries to execute a Custom Calculate Step but receives an error and the Step fails. The user has access to the Cube and the corresponding Scenario. What is the most likely cause of the failure?

A) The user does not have the ModifyData role
B) The user is not a member of the Manage Data Group of the Scenario
C) The Custom Calculate Step does not support parameters
D) The Cube does not have the Consolidation Dimension configured

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To execute a Custom Calculate Step, the user must be a member of the Manage Data Group of the Scenario; otherwise, the Step will fail. The ModifyData role (A) controls data modification in general but is not the specific cause of Custom Calculate failure. Custom Calculate does support parameters (C). The Consolidation Dimension is not a specific requirement for Custom Calculate (D).
</details>

---

### Question 6 (Hard)
**201.5.1** | Difficulty: Hard

What happens to data saved with Durable Storage during a normal Calculate?

A) It is automatically deleted as part of the calculation process
B) It is recalculated and overwritten with the new values
C) It is not cleared; it is only removed with an explicit ClearCalculated or Force
D) It is moved to the audit archive before being deleted

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Data saved with Durable Storage persists during a normal Calculate and is not cleared. It is only removed with an explicit ClearCalculated or by using the Force Calculate option. This allows certain calculated data to be maintained across multiple calculation cycles.
</details>

---

### Question 7 (Hard)
**201.5.1** | Difficulty: Hard

**Scenario:** An administrator needs to completely clear a Scenario including data, audit information, Workflow Status, and Calculation Status. What step should they execute and what precaution should they take?

A) Clear Data Step with all options selected; no special precautions needed
B) Reset Scenario Step; it is recommended to make a backup before executing
C) Execute Business Rule with a cleanup script; verify user permissions
D) Copy Data Step with an empty destination; make sure the Cube is unlocked

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Reset Scenario is the correct Step because it clears data, audit information, Workflow Status, and Calculation Status. It is more aggressive than Clear Data (which only clears data). A backup is recommended before executing because the operation is irreversible. Additionally, it requires the user to be a member of the Manage Data Group of the Scenario. Clear Data (A) does not clear Workflow Status or Calculation Status.
</details>

---

### Question 8 (Easy)
**201.5.1** | Difficulty: Easy

Where is the email notification configured upon completion or failure of a Data Management Sequence?

A) At the individual Step
B) At the Sequence
C) At the Group
D) At the Profile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The email notification is configured at the Sequence level, not at the individual Step, Group, or Profile. The Sequence allows configuring notifications on completion, on failure, or both, and requires an email server to be configured.
</details>

---

### Question 9 (Hard)
**201.5.1** | Difficulty: Hard

What are the default Queueing values in a Data Management Sequence for the Maximum % CPU Utilization and the Maximum Queued Time Before Canceling?

A) 80% CPU and 30 minutes
B) 70% CPU and 20 minutes
C) 50% CPU and 15 minutes
D) 90% CPU and 10 minutes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The default Queueing values are: Maximum % CPU Utilization of 70% and Maximum Queued Time Before Canceling of 20 minutes. These values control the server CPU utilization and the maximum time a task can be queued before being canceled.
</details>

---

### Question 10 (Medium)
**201.5.1** | Difficulty: Medium

In a Clear Data Step, what are the granular options available to select which data to clear?

A) Clear All Data, Clear Partial Data, Clear Calculated Data
B) Clear Imported Data, Clear Forms Data, Clear Adjustment Data and Delete Journals, Clear Data Cell Attachments
C) Clear Cube Data, Clear Scenario Data, Clear Entity Data
D) Clear Input Data, Clear Output Data, Clear Temporary Data, Clear Archived Data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The granular options of Clear Data are: Clear Imported Data (Origin: Import), Clear Forms Data (Origin: Forms), Clear Adjustment Data and Delete Journals (Origin: Adjustments), and Clear Data Cell Attachments. These options allow selectively clearing data based on its origin.
</details>

---

## Objective 201.5.2: Excel Add-In Functionality

### Question 11 (Easy)
**201.5.2** | Difficulty: Easy

What are the three main components of the OneStream Excel Add-In?

A) Ribbon, Toolbar, Status Bar
B) Task Pane, OneStream Ribbon, Error Logs
C) Menu Bar, Quick Access, Data Panel
D) Navigator, Formula Bar, Data Explorer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three main components of the Excel Add-In are: Task Pane (task panel with POV, Quick Views, and Documents), OneStream Ribbon (ribbon with all functionality), and Error Logs (stored locally in AppData, automatically deleted after 60 days).
</details>

---

### Question 12 (Easy)
**201.5.2** | Difficulty: Easy

What is the recommended Excel calculation mode setting when working with the OneStream Add-In for best performance?

A) Automatic
B) Automatic Except for Data Tables
C) Manual
D) Semi-Automatic

<details>
<summary>Show answer</summary>

**Correct answer: C)**

It is recommended to set the Excel calculation mode to Manual for best performance when working with the OneStream Add-In. This prevents Excel from automatically recalculating all formulas every time a change is made, which can be very slow with many XF functions.
</details>

---

### Question 13 (Medium)
**201.5.2** | Difficulty: Medium

What is the difference between Refresh Sheet and Refresh Workbook in the Excel Add-In?

A) There is no difference; both refresh the entire workbook
B) Refresh Sheet refreshes only the active sheet and clears dirty cells only on that sheet; Refresh Workbook refreshes all sheets and clears dirty cells on all of them
C) Refresh Sheet only updates formulas; Refresh Workbook updates data and formulas
D) Refresh Sheet is slower because it validates each cell; Refresh Workbook is faster with bulk updating

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Refresh Sheet refreshes only the active sheet and clears dirty cells only on the selected sheet, showing parameters from the current sheet. Refresh Workbook refreshes the entire workbook, clears dirty cells on all sheets, and shows all parameters. The same logic applies to Submit Sheet vs Submit Workbook.
</details>

---

### Question 14 (Medium)
**201.5.2** | Difficulty: Medium

Which XF function of the Excel Add-In uses Member Script (e.g., A#Sales:E#Texas) instead of individual parameters for each dimension?

A) XFGetCell
B) XFGetMemberProperty
C) XFGetCellUsingScript
D) XFGetCell5

<details>
<summary>Show answer</summary>

**Correct answer: C)**

XFGetCellUsingScript uses Member Script (e.g., A#Sales:E#Texas) instead of individual parameters for each dimension. XFGetCell and XFGetCell5 require individual parameters for each dimension (20 and 5 dimensions respectively). XFGetMemberProperty retrieves dimension member properties, not cell data.
</details>

---

### Question 15 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** A user creates a Cube View in Excel with custom formulas in some cells. After performing Submit and Refresh, the formulas disappear. What option should they have enabled to keep the formulas?

A) Preserve Excel Format
B) Retain Formulas in Cube View Content
C) Dynamically Highlight Evaluated Cells
D) Include Header

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Retain Formulas in Cube View Content allows Excel formulas in Cube View cells to be maintained after Submit and Refresh. Cells with different values are shown as "dirty cells" (yellow). Preserve Excel Format (A) maintains the visual format, not the formulas. Dynamically Highlight Evaluated Cells (C) updates cells without needing a Refresh but does not preserve formulas.
</details>

---

### Question 16 (Medium)
**201.5.2** | Difficulty: Medium

How many dimensions does the XFGetCell function support as parameters?

A) 5
B) 10
C) 15
D) 20

<details>
<summary>Show answer</summary>

**Correct answer: D)**

XFGetCell supports 20 dimensions as parameters: Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1-UD8. If only 5 UD dimensions are needed, you can use XFGetCell5 which limits UD to 5.
</details>

---

### Question 17 (Hard)
**201.5.2** | Difficulty: Hard

What is the performance limitation of Table Views in the Excel Add-In and what restriction do they have regarding data operations?

A) Maximum of 5000 KB of data and they do not perform deletes
B) Maximum of 8000 KB of data and they only update existing records, they do not perform inserts
C) Maximum of 10000 KB of data and they do not perform updates
D) Maximum of 8000 KB of data and they only perform inserts, not updates

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Table Views have a performance limit of approximately 8000 KB of data per Table View, and they only update existing records; they do not perform inserts. They are defined through Spreadsheet-type Business Rules, and the administrator controls security and write-back through Business Rules.
</details>

---

### Question 18 (Easy)
**201.5.2** | Difficulty: Easy

How is the OneStream Excel Add-In registered in Microsoft Excel?

A) It is installed as a VSTO plugin
B) It is registered as a COM Add-in via File > Options > Add-ins > COM Add-ins
C) It is automatically downloaded when opening Excel
D) It is installed from the Microsoft Store

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The OneStream Excel Add-In is registered as a COM Add-in. The ribbon is added via File > Options > Add-ins > COM Add-ins > OneStreamExcelAddIn. It is not a VSTO plugin, it is not automatically downloaded, nor is it installed from the Microsoft Store.
</details>

---

### Question 19 (Hard)
**201.5.2** | Difficulty: Hard

What are the limitations of the OneStream Spreadsheet application compared to the Excel Add-In?

A) It does not support Cube Views or Quick Views
B) It does not support Macros, Solver, Document Properties, Undo/Redo, some 3D chart types, or references to external workbooks
C) It only allows reading data, not writing
D) It does not support XF functions or In-Sheet Actions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Spreadsheet application has similar functionality to the Excel Add-In but without needing Excel installed. Its limitations include: no support for Macros, Solver, Document Properties, Undo/Redo, some 3D chart types, and no ability to reference external workbooks. It does support Cube Views, Quick Views, data writing, and XF functions.
</details>

---

### Question 20 (Medium)
**201.5.2** | Difficulty: Medium

What are In-Sheet Actions in the Excel Add-In and what operations do they allow executing?

A) Excel macros that run automatically when opening the sheet
B) Buttons on the sheet that allow executing Refresh, Submit, and Data Management Sequence without leaving the sheet
C) Special formulas that calculate data in real time
D) Links to external dashboards that open in the browser

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In-Sheet Actions are configurable buttons on the Excel sheet that allow executing Refresh, Submit, and Data Management Sequence without leaving the sheet. They are configured with label, Submit option (Sheet/Workbook/Nothing), Data Management Sequence, Parameters, Refresh, and custom colors.
</details>

---

## Objective 201.5.3: Application Properties

### Question 21 (Easy)
**201.5.3** | Difficulty: Easy

Where are Application Properties accessed in OneStream?

A) System > Tools > Application Properties
B) Application > Cube > Properties
C) Application > Tools > Application Properties
D) Application > Workflow > Properties

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Application Properties are accessed via Application > Tools > Application Properties. This is where the default properties of the application are configured, including Global POV, Company Information, Currencies, Formatting, and more.
</details>

---

### Question 22 (Easy)
**201.5.3** | Difficulty: Easy

What do the Global Scenario and Global Time properties control in Application Properties?

A) The Scenario and Time used for automatic calculations
B) The default Scenario and Time that users will see in Workflow
C) The maximum Scenario and Time allowed in the application
D) The Scenario and Time used exclusively for reports

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Global Scenario and Global Time define the default Scenario and time period that users will see in Workflow. These values are configured in the General Properties section and can be enforced for all users through the Enforce Global POV option.
</details>

---

### Question 23 (Medium)
**201.5.3** | Difficulty: Medium

What happens when the "Enforce Global POV" property is set to True?

A) Only administrators can change the Scenario and Time
B) The Global Scenario and Global Time are enforced for all users
C) All data modifications in the application are blocked
D) Cube View filters are disabled for all users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Enforce Global POV is set to True, the Global Scenario and Global Time are enforced for all users. This means no user can change these values and everyone works with the same global point of view. It does not block data modifications or disable Cube View filters.
</details>

---

### Question 24 (Medium)
**201.5.3** | Difficulty: Medium

In Application Properties, what number format should be used to display values without decimals?

A) N1
B) N0
C) #,###,0
D) 0.00

<details>
<summary>Show answer</summary>

**Correct answer: B)**

N0 is the format for displaying values without decimals. Formats N1 through N6 show 1 through 6 decimals respectively. Format #,###,0 (C) is a custom format with separators, and 0.00 (D) is not a valid OneStream format.
</details>

---

### Question 25 (Hard)
**201.5.3** | Difficulty: Hard

**Scenario:** An administrator needs to add a currency that is not in the predefined list in Application Properties. What should they do?

A) Manually create the currency in the Currencies section
B) Import the currency from an XML file
C) Contact OneStream Support to have it added
D) Add the currency in System > Configuration > Currencies

<details>
<summary>Show answer</summary>

**Correct answer: C)**

If a currency is needed that is not listed in the predefined options of Application Properties, OneStream Support must be contacted to have it added. All currencies used must be listed in Application Properties to be used on the Entity, for currency translation, or for rate entry, including pre-Euro and discontinued currencies for historical data.
</details>

---

### Question 26 (Hard)
**201.5.3** | Difficulty: Hard

What is the function of the "Lock after Certify" property in Application Properties and where is it located?

A) In General Properties > Transformation; it blocks editing Business Rules after certification
B) In Certification Properties; it automatically locks the Workflow after certification
C) In General Properties > Certification; it automatically locks after certification in Workflow
D) In Dimension Properties; it locks dimensions after certification

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Lock after Certify is located in the Certification section of General Properties. When set to True, it automatically locks the Workflow after certification is completed. This prevents additional changes to data that has already been certified.
</details>

---

### Question 27 (Easy)
**201.5.3** | Difficulty: Easy

Where does the Company Name configured in Application Properties appear?

A) On the OneStream login page
B) On reports automatically generated from Cube Views
C) In the browser window title
D) In the footer of all Dashboards

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Company Name configured in Application Properties appears on reports automatically generated from Cube Views. Along with the Logo File (PNG format, approximately 50 pixels high), these elements are displayed in Cube Views and Reports.
</details>

---

### Question 28 (Medium)
**201.5.3** | Difficulty: Medium

What does the "Allow Loads Before Workflow View Year" property control in Application Properties?

A) It allows loading data from years prior to the application creation year
B) It allows loading data to periods prior to the current Workflow year
C) It allows loading historical data from CSV files
D) It allows viewing data from prior years in Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "Allow Loads Before Workflow View Year" property, when set to True, allows loading data to periods prior to the current Workflow year. Similarly, "Allow Loads After Workflow View Year" allows loading data to periods after. Both are found in the Transformation section of General Properties.
</details>

---

### Question 29 (Hard)
**201.5.3** | Difficulty: Hard

In the custom number format of Application Properties, what does the format "#,###,0;(#,###,0);0" represent?

A) Format for thousands with decimal point; format for millions; format for units
B) Format for positive numbers with thousands separator; format for negative numbers in parentheses; format for zeros
C) Format for integers; format for decimals; format for percentages
D) Format for local currency; format for foreign currency; format for base currency

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Custom formats with three sections use the pattern: positives;negatives;zeros. The format "#,###,0;(#,###,0);0" shows positive numbers with thousands separator, negative numbers in parentheses with thousands separator, and zeros as "0". Additionally, trailing spaces can be included in the positive format to compensate for the negative parentheses and achieve vertical alignment.
</details>

---

### Question 30 (Medium)
**201.5.3** | Difficulty: Medium

What does the "UD Dimension Type for Workflow Channels" property control in Application Properties?

A) It defines which UD dimensions are displayed in Workflow reports
B) It controls an additional dimension for independent locking per channel in Workflow
C) It assigns UD dimensions to specific Cubes within the Workflow
D) It determines how many UD dimensions can be used in the application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "UD Dimension Type for Workflow Channels" property controls an additional dimension for independent locking per channel. This allows, for example, planning to be done by Entity and by Product independently, enabling Workflow Channels with different locking levels.
</details>

---

## Objective 201.5.4: Creating a Scheduled Task

### Question 31 (Easy)
**201.5.4** | Difficulty: Easy

What type of object is scheduled in the OneStream Task Scheduler?

A) Individual Data Management Steps
B) Data Management Sequences
C) Business Rules
D) Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Task Scheduler schedules Data Management Sequences, not individual Steps, Business Rules, or Cube Views. Steps are organized within Sequences, and it is the Sequences that are selected when creating a scheduled task.
</details>

---

### Question 32 (Easy)
**201.5.4** | Difficulty: Easy

Where is the Task Scheduler accessed in OneStream?

A) System > Tools > Task Scheduler
B) Application > Workflow > Task Scheduler
C) Application > Tools > Task Scheduler
D) System > Administration > Task Scheduler

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Task Scheduler is accessed via Application > Tools > Task Scheduler. It is an application-level tool, not a system tool. To access it, the user needs the Application User Interface Role "TaskSchedulerPage".
</details>

---

### Question 33 (Medium)
**201.5.4** | Difficulty: Medium

What is the maximum number of retries that can be configured in the Advanced tab of a Scheduled Task if it fails?

A) 1
B) 2
C) 3
D) 5

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The maximum number of configurable retries in the Advanced tab is 3. If the task fails, it will retry up to the specified number of times before being definitively marked as failed.
</details>

---

### Question 34 (Medium)
**201.5.4** | Difficulty: Medium

**Scenario:** A user with the "TaskScheduler" role tries to perform a Load/Extract of scheduled tasks but cannot. Why?

A) The Task Scheduler does not support Load/Extract
B) The TaskScheduler role does not allow Load/Extract; the ManageTaskScheduler role is needed
C) The user needs the additional ApplicationLoadExtractPage role
D) Only the Administrator can perform Load/Extract of any type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The TaskScheduler role allows creating tasks and editing own tasks, viewing all tasks, but cannot perform Load/Extract. To perform Load/Extract of scheduled tasks, the ManageTaskScheduler role is needed, which also allows creating, viewing, editing, and deleting all tasks.
</details>

---

### Question 35 (Hard)
**201.5.4** | Difficulty: Hard

In what time zone are scheduled tasks stored in OneStream and how are they presented to the user?

A) They are stored and presented in the server time zone
B) They are stored in UTC and presented in the user's local time zone
C) They are stored in the user's local time zone and presented in UTC
D) They are stored and presented in the time zone configured in Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Scheduled tasks are stored internally in UTC but are presented to the user in their local time zone. When the user configures the start date and time, they do so in their local time, but the system converts and saves it in UTC to ensure consistency regardless of the user's location.
</details>

---

### Question 36 (Easy)
**201.5.4** | Difficulty: Easy

What are the available scheduling frequencies in the Task Scheduler?

A) One Time, Hourly, Daily, Weekly
B) One Time, Minutes, Daily, Weekly, Monthly
C) Continuous, Hourly, Daily, Weekly, Monthly, Yearly
D) One Time, Daily, Weekly, Monthly, Quarterly

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available frequencies are: One Time (single execution), Minutes (recurring every 5-180 minutes, with option to limit with Time From/To), Daily, Weekly (select days and frequency), and Monthly (select days and frequency). There is no Hourly, Continuous, Quarterly, or Yearly option.
</details>

---

### Question 37 (Medium)
**201.5.4** | Difficulty: Medium

What is the difference between the TaskScheduler and ManageTaskScheduler roles?

A) TaskScheduler can create and edit all tasks; ManageTaskScheduler can only view them
B) TaskScheduler can create and edit own tasks; ManageTaskScheduler can manage all tasks and perform Load/Extract
C) Both roles are identical in functionality
D) TaskScheduler is a System Role; ManageTaskScheduler is an Application Role

<details>
<summary>Show answer</summary>

**Correct answer: B)**

TaskScheduler allows creating tasks, editing own tasks, and viewing all tasks, but cannot perform Load/Extract or change the task name. ManageTaskScheduler allows creating, viewing, editing, and deleting all tasks, and can perform Load/Extract, but also cannot change the task name. Both are Application Security Roles.
</details>

---

### Question 38 (Hard)
**201.5.4** | Difficulty: Hard

**Scenario:** A scheduled task that had been working correctly now consistently fails. The administrator verifies that the Sequence exists and has Steps configured. What else should they check as a possible cause?

A) That the server time zone has not changed
B) That the Task Scheduler Validation Frequency has not expired the task
C) That the Sequence has at least one Step configured and that the user's permissions (Manage Data Group of the Scenario) are still valid
D) That the associated Cube View has not been deleted

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Although the Sequence exists, you must verify that it has at least one Step (if the Sequence has no Step, the job will fail) and that the user's permissions are still valid, including membership in the Manage Data Group of the Scenario for Steps like Custom Calculate or Reset Scenario. You should also check the Expire Date/Time and the Enabled state of the task.
</details>

---

### Question 39 (Hard)
**201.5.4** | Difficulty: Hard

How does a scheduled task appear in Task Activity and Logon Activity respectively?

A) Task Type: "Scheduled Task" / Client Module: "Task Scheduler"
B) Task Type: "Data Management Scheduled Task" / Client Module: "Scheduler"
C) Task Type: "Auto Task" / Client Module: "Background Service"
D) Task Type: "Data Management" / Client Module: "Automated"

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In Task Activity, the Task Type is shown as "Data Management Scheduled Task". In Logon Activity, the Client Module appears as "Scheduler". These specific identifiers allow distinguishing scheduled tasks from manually executed ones.
</details>

---

### Question 40 (Medium)
**201.5.4** | Difficulty: Medium

What checkbox can only be modified by an Administrator in the configuration of a Scheduled Task?

A) Enabled
B) Administration Enabled (Enabled by Manager)
C) Auto Retry
D) Send Notification

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Administration Enabled (also known as "Enabled by Manager") is a checkbox that only the Administrator can change. The regular Enabled checkbox can be changed by any user with Task Scheduler permissions, but Administration Enabled requires administrator privileges.
</details>

---

## Objective 201.5.5: Correct Use of Load/Extract

### Question 41 (Easy)
**201.5.5** | Difficulty: Easy

In what format are artifacts imported and exported via Load/Extract?

A) JSON
B) CSV
C) XML
D) SQL

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Load/Extract uses XML format to import and export application or system artifacts. The exception is the Application Zip File which uses ZIP format, and FX Rate CSV which uses CSV format. But the standard format for artifact definitions is XML.
</details>

---

### Question 42 (Easy)
**201.5.2** | Difficulty: Easy

What is the main typical use of Load/Extract in OneStream?

A) Creating backups of transactional data
B) Moving configurations between environments (Development > Test > Production)
C) Exporting data for external reports
D) Synchronizing data between applications in real time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The main typical use of Load/Extract is to move configurations between environments, following the flow Development > Test > Production. It is also used for configuration backups and artifact migration, but it is not a real-time replication tool nor is it designed for exporting transactional data for reports.
</details>

---

### Question 43 (Medium)
**201.5.5** | Difficulty: Medium

What does an Application Zip File include when extracted?

A) Everything including data and FX Rates
B) Everything except data (Data) and FX Rates
C) Only metadata and Business Rules
D) Only Dashboards and Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An Application Zip File includes everything except Data and FX Rates. It creates a complete copy of the application with metadata, Business Rules, Cube Views, Data Management, Workflow Profiles, Dashboards, etc., but does not include stored data or exchange rates.
</details>

---

### Question 44 (Medium)
**201.5.5** | Difficulty: Medium

**Scenario:** An administrator needs to export the application's exchange rates in a format that can be easily opened in Excel. What file type should they select in Application Load/Extract?

A) FX Rates
B) FX Rate CSV
C) Application Zip File
D) Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

FX Rate CSV exports exchange rates in CSV format with the columns FxRateType, Time, SourceCurrency, DestCurrency, Amount, and HasData, which can be easily opened in Excel. FX Rates (A) exports in XML format by FX Rate Type and Time Period, which is less accessible for Excel.
</details>

---

### Question 45 (Hard)
**201.5.5** | Difficulty: Hard

What happens when loading a Workflow Profiles XML that does not contain all existing properties in the destination environment?

A) Missing properties are kept unchanged
B) Old properties not in the XML are automatically cleaned
C) The load fails with a validation error
D) An automatic backup is created before the load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When loading a Workflow Profiles XML, old properties that are not included in the XML file are automatically cleaned. This means the loaded XML completely replaces the existing Workflow Profiles configuration, so it is important to ensure the XML contains all desired configuration.
</details>

---

### Question 46 (Easy)
**201.5.5** | Difficulty: Easy

Who has default access to the Load/Extract functionality?

A) All users (Everyone)
B) Only Administrators
C) Users with the ModifyData role
D) Users with the ManageData role

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Only Administrators have default access to the Load/Extract functionality. For other users to access it, they must be assigned the corresponding user interface roles (ApplicationLoadExtractPage or SystemLoadExtractPage) along with the necessary security roles.
</details>

---

### Question 47 (Hard)
**201.5.5** | Difficulty: Hard

What are the three operations available in Load/Extract and what does each do?

A) Import (load XML), Export (export XML), Backup (create backup copy)
B) Load (import XML), Extract (export XML), Extract and Edit (extract and edit the XML immediately)
C) Upload (upload file), Download (download file), Sync (synchronize)
D) Load (import XML), Extract (export XML), Compare (compare versions)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three operations are: Load (import an XML file with artifact definitions), Extract (export artifacts to an XML file), and Extract and Edit (extract and edit the XML immediately). The latter is useful for making quick modifications to the XML before loading it into another environment.
</details>

---

### Question 48 (Medium)
**201.5.5** | Difficulty: Medium

What artifacts can be extracted in the "Metadata" category of Application Load/Extract?

A) Only dimensions and their members
B) Business Rules, Time Dimension Profiles, Dimensions, and Cubes
C) Only Cubes and their configurations
D) Dimensions, Cubes, Scenarios, and Entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Metadata category includes Business Rules, Time Dimension Profiles, Dimensions, and Cubes. It covers both dimension definitions and business rules and time profiles associated with the application metadata.
</details>

---

### Question 49 (Hard)
**201.5.5** | Difficulty: Hard

An administrator needs to export only the Extensibility Rules of an application. What should they select and why?

A) Metadata, because Extensibility Rules are part of the metadata
B) Extensibility Rules, because other types of Business Rules are exported with their associated object
C) Application Zip File, because it is the only way to include Business Rules
D) Execute Business Rule, because Extensibility Rules are a type of Step

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You should select "Extensibility Rules" in Application Load/Extract. This extraction type only includes Extensibility Rules because other types of Business Rules (such as Finance, Spreadsheet, etc.) are exported together with their associated object (Data Management, Cube Views, etc.). This separation allows for specific export of extensibility rules.
</details>

---

### Question 50 (Medium)
**201.5.5** | Difficulty: Medium

What is the recommended best practice before deploying changes to a production environment using Load/Extract?

A) Perform Load/Extract directly from development to production during low-activity hours
B) Extract changes from the development environment, evaluate in a test environment, then apply in production
C) Create a copy of the XML file and save it as a backup
D) Deactivate all users before loading changes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to always deploy and test changes first in a development environment, then extract those changes and evaluate them in a test environment before applying them in production. It is also recommended to consider making a copy of the production database to test changes and avoid deploying during periods of high load or activity.
</details>

---

## Objective 201.5.6: Difference Between Application and System Load/Extract

### Question 51 (Easy)
**201.5.6** | Difficulty: Easy

Where is Application Load/Extract located and where is System Load/Extract located?

A) Both in System > Tools > Load/Extract
B) Application Load/Extract in Application > Tools > Load/Extract; System Load/Extract in System > Tools > Load/Extract
C) Both in Application > Tools > Load/Extract
D) Application Load/Extract in Application > Administration; System Load/Extract in System > Administration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application Load/Extract is located in Application > Tools > Load/Extract and works with artifacts specific to one application. System Load/Extract is located in System > Tools > Load/Extract and works with framework/system artifacts shared across all applications.
</details>

---

### Question 52 (Easy)
**201.5.6** | Difficulty: Easy

What type of artifacts are managed with System Load/Extract?

A) Metadata, Business Rules, Cube Views, and Data Management
B) Security (Users, Groups, Roles), System Dashboards, and Logs
C) FX Rates, Workflow Profiles, and Transformation Rules
D) Cubes, Scenarios, and Entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

System Load/Extract manages framework/system artifacts: Security (System Roles, Users, Security Groups, Exclusion Groups), System Dashboards (Maintenance Units and Profiles), and Logs (Error Log, Task Activity, Logon Activity with Start/End Time ranges). The other mentioned artifacts belong to Application Load/Extract.
</details>

---

### Question 53 (Medium)
**201.5.6** | Difficulty: Medium

**Scenario:** An administrator needs to move user and security group configuration to a new environment. What tool should they use and what precaution should they take with Unique IDs?

A) Application Load/Extract; keep Unique IDs checked
B) System Load/Extract; uncheck "Extract Unique IDs" to avoid duplicate ID conflicts in the new environment
C) System Load/Extract; keep Unique IDs checked to preserve integrity
D) Application Load/Extract; uncheck "Extract Unique IDs"

<details>
<summary>Show answer</summary>

**Correct answer: B)**

System Load/Extract should be used because users and groups are system/framework artifacts. When moving to another environment, "Extract Unique IDs" should be unchecked to avoid errors with duplicate IDs, since the new environment may have different IDs assigned to other objects.
</details>

---

### Question 54 (Medium)
**201.5.6** | Difficulty: Medium

What Application User Interface Role is needed to access Application Load/Extract and which for System Load/Extract?

A) ApplicationToolsPage and SystemToolsPage
B) ApplicationLoadExtractPage and SystemLoadExtractPage
C) ManageApplication and ManageSystem
D) ApplicationAdminPage and SystemAdminPage

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Application Load/Extract, the ApplicationLoadExtractPage role (Application User Interface Role) plus Administrator role or equivalent is needed. For System Load/Extract, the SystemLoadExtractPage role (System User Interface Role) is needed, and for Security you also need ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles.
</details>

---

### Question 55 (Hard)
**201.5.6** | Difficulty: Hard

On which database does each type of Load/Extract operate?

A) Both operate on the Application Database
B) Application Load/Extract operates on the Application Database; System Load/Extract operates on the Framework (System) Database
C) Both operate on the Framework Database
D) Application Load/Extract operates on the Framework Database; System Load/Extract operates on the Application Database

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application Load/Extract operates on the Application Database, which contains artifacts specific to an individual application. System Load/Extract operates on the Framework (System) Database, which contains shared system artifacts such as users, groups, roles, and configurations that apply to all applications.
</details>

---

### Question 56 (Easy)
**201.5.6** | Difficulty: Easy

What is the scope of each type of Load/Extract?

A) Application: the entire system; System: a specific application
B) Application: a specific application; System: the entire environment/system
C) Both: the entire complete system
D) Both: only the active application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application Load/Extract has scope over a specific application (its metadata, Rules, Views, Dashboards, etc.). System Load/Extract has scope over the entire environment/system (Security, System Dashboards, Logs that are shared across all applications).
</details>

---

### Question 57 (Hard)
**201.5.6** | Difficulty: Hard

**Scenario:** A non-Administrator user needs to perform Load/Extract of system Security. What specific roles do they need to have assigned?

A) Only SystemLoadExtractPage
B) SystemLoadExtractPage, ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles
C) AdministerApplication and SystemLoadExtractPage
D) ManageSystemSecurityUsers and SystemPane

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For a non-Administrator user to perform Load/Extract of system Security, they need four roles: SystemLoadExtractPage (UI Role to access the page), ManageSystemSecurityUsers (to manage users), ManageSystemSecurityGroups (to manage groups), and ManageSystemSecurityRoles (to manage roles). Without the three Manage System Security roles, they cannot complete the operation.
</details>

---

### Question 58 (Medium)
**201.5.6** | Difficulty: Medium

What artifacts can be extracted as part of System Load/Extract for Logs?

A) Only Error Log
B) Error Log, Task Activity, and Logon Activity, all with Start/End Time range
C) Error Log and Task Activity without time filters
D) All system logs without time limit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In System Load/Extract, three types of logs can be extracted: Error Log, Task Activity, and Logon Activity. All three require a Start/End Time range to filter the records to export. This is useful for auditing and system activity analysis.
</details>

---

### Question 59 (Hard)
**201.5.6** | Difficulty: Hard

What special consideration must be taken when performing Security XML loads by non-Administrator users?

A) The XML file must be digitally signed
B) The security must pre-exist in the destination environment to validate the changes
C) Only users can be loaded, not groups or roles
D) A server restart is required after the load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Security XML loads performed by non-Administrator users, the security must pre-exist in the destination environment so the system can validate the changes. This is a security measure that ensures a non-Administrator user cannot create security configurations that do not previously exist, limiting their modification capability to already established elements.
</details>

---

### Question 60 (Medium)
**201.5.6** | Difficulty: Medium

What is the file format used by Application Load/Extract to export a complete application compared to individual artifacts?

A) Both use XML format
B) The complete application uses ZIP format; individual artifacts use XML format
C) The complete application uses JSON format; individual artifacts use XML
D) Both use ZIP format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Application Zip File (complete application) is exported in ZIP format and contains everything except data and FX Rates. Individual artifacts (Metadata, Cube Views, Data Management, etc.) are exported in XML format. System Load/Extract always uses XML format.
</details>

---

## Additional Questions: Tools Section Review

### Question 61 (Easy)
**201.5.1** | Difficulty: Easy

How many types of Business Rules exist in OneStream, and where are they found?

A) 6 types, under System > Tools > Business Rules
B) 9 types, under Application > Tools > Business Rules
C) 12 types, under Application > Administration > Business Rules
D) 9 types, under System > Administration > Business Rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

There are nine types of Business Rules found under Application > Tools > Business Rules. The nine types are: Finance, Parser, Connector, Conditional, Derivative, Cube View Extender, Dashboard Dataset, Dashboard Extender, and Dashboard XFBRString. Additionally, Extensibility Rules (Extender and Event Handlers) exist as a separate category requiring Full Administrator rights to edit.
</details>

---

### Question 62 (Easy)
**201.5.3** | Difficulty: Easy

What level of access is required to edit Extensibility Rules in OneStream?

A) Any user with the ModifyData role
B) Members of the Business Rules Maintenance security group
C) Full Administrator rights
D) Users with the EncryptBusinessRule security role

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Full Administrator rights are required to edit Extensibility Rules. This is explicitly stated in the documentation as an important requirement, distinguishing Extensibility Rules from other Business Rule types that can be maintained by users assigned to the rule's Maintenance security group.
</details>

---

### Question 63 (Easy)
**201.5.6** | Difficulty: Easy

What are the default and maximum values for the Profiler's "Number of Minutes to Run" and "Number of Hours to Retain Before Deletion" settings?

A) Default 10 minutes / max 30 minutes; default 12 hours / max 72 hours
B) Default 20 minutes / max 60 minutes; default 24 hours / max 168 hours
C) Default 30 minutes / max 120 minutes; default 48 hours / max 336 hours
D) Default 15 minutes / max 45 minutes; default 24 hours / max 120 hours

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Profiler defaults to 20 minutes to run with a maximum of 60 minutes, and 24 hours retention before deletion with a maximum of 168 hours. Values entered over the maximum are automatically reset to the default value. The Profiler session is deleted on the first server restart after the deletion time has passed.
</details>

---

### Question 64 (Medium)
**201.5.2** | Difficulty: Medium

What are the available Spreading types in the OneStream Excel Add-In?

A) Fill, Clear Data, Factor, Even Distribution, Proportional Distribution
B) Fill, Clear Data, Factor, Accumulate, Even Distribution, 445, 454, 544, Proportional Distribution
C) Fill, Factor, Accumulate, Even Distribution, Weighted Distribution
D) Fill, Clear Data, Factor, Even Distribution, 445, 544, Custom Distribution

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The nine Spreading types are: Fill (fills each cell with the specified amount), Clear Data (clears all data), Factor (multiplies by a rate), Accumulate (compounds each cell by the rate from the previous), Even Distribution (distributes evenly), 445 Distribution (weights 4-4-5), 454 Distribution (weights 4-5-4), 544 Distribution (weights 5-4-4), and Proportional Distribution (distributes proportionally based on existing values).
</details>

---

### Question 65 (Medium)
**201.5.2** | Difficulty: Medium

Why do Quick Views sometimes display more columns than expected when multiple dimensions are placed in the column area?

A) Quick Views have a bug that duplicates columns
B) Quick Views multiply members across dimensions because they have a single column set, producing a Cartesian product
C) Quick Views automatically add calculated variance columns
D) Quick Views expand hidden members by default

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Quick Views have a single row set and a single column set. When multiple dimensions with multiple members are placed in columns, they produce a Cartesian product (multiplication). For example, 2 Scenarios and 2 UD8 members result in 4 columns instead of the desired 3. This is known as the asymmetry issue. Cube Views solve this by allowing independent columns and rows, each with their own Member Filters.
</details>

---

### Question 66 (Medium)
**201.5.2** | Difficulty: Medium

What happens when you use the "Convert to XFGetCells" function on an existing Quick View?

A) The Quick View is preserved alongside the new XFGetCell formulas
B) The Quick View definition is deleted and converted to XFGetCell formulas; this conversion cannot be undone
C) The Quick View is converted to a Cube View with XFGetCell references
D) The Quick View is temporarily hidden and can be restored at any time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When you click Convert to XFGetCells, OneStream prompts with a confirmation message. By clicking OK, the Quick View definition is deleted and converted to XFGetCell formulas. This is a one-way conversion: there is no way to convert XFGetCells back into a Quick View. The formatting from the original Quick View is retained on the converted cells.
</details>

---

### Question 67 (Medium)
**201.5.6** | Difficulty: Medium

What are the supported file size limits for the OneStream File Explorer in the Windows Application?

A) Uploads: up to 100 MB; Downloads: up to 500 MB
B) Uploads (Applications and System): up to 300 MB; Uploads (Content): up to 2 GB; Downloads: up to 2 GB
C) Uploads: up to 1 GB; Downloads: up to 1 GB
D) Uploads (Applications and System): up to 500 MB; Uploads (Content): up to 5 GB; Downloads: up to 5 GB

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In the Windows Application, the supported file sizes are: Uploads to Applications and System folders up to 300 MB, Uploads to Content folders up to 2 GB, and Downloads from both Application/System and Content folders up to 2 GB. The Contents folders are auto-generated within both the Application and System folders and are intended to store files larger than 300 MB.
</details>

---

### Question 68 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** An administrator formats a Cube View in Excel using native Excel formatting (font colors, borders, etc.) but the formatting disappears after Refresh. What is the correct approach to make formatting persist on Cube Views, and what is the order of precedence for formatting styles?

A) Use the "Lock Formatting" button in the OneStream ribbon; precedence is Styles > Conditional > Selection Styles
B) Use Selection Styles or Preserve Excel Format; the order of precedence from lowest to highest is: Default CV Style < Custom CV Format < Conditional Formatting < Selection Styles < Styles
C) Use only native Excel conditional formatting, which is the only formatting that persists; there is no precedence order
D) Enable "Preserve Excel Format" in the Cube View Connection settings; precedence does not apply when this is enabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Native Excel formatting alone will not persist after a Refresh on Cube Views. The correct approaches are: enable Preserve Excel Format in the Cube View Connection, use Selection Styles (which associate Excel formatting with the Cube View), or use Styles (based on named ranges). The exception is native conditional formatting, which works and persists without any special setup. The order of precedence from lowest to highest priority is: Default Cube View Style < Custom Cube View Format < Conditional Formatting < Selection Styles < Styles.
</details>

---

### Question 69 (Hard)
**201.5.1** | Difficulty: Hard

**Scenario:** A developer wants to encrypt a Business Rule to protect its logic. What security role is required, what happens after encryption, and what should they be careful about?

A) The AdministerApplication role is required; the rule becomes hidden from all users; the password is stored in the database and can be recovered
B) The EncryptBusinessRule security role is required; the editor displays "Business Rule Is Encrypted" in read-only mode; if the password is forgotten, OneStream Support must be contacted to decrypt it
C) No special role is required; any user with Maintenance access can encrypt; the rule can be decrypted by any Administrator
D) The DatabaseAdmin role is required; the encrypted rule cannot be viewed or executed; the password can be reset from Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To encrypt a Business Rule, the user must be assigned the EncryptBusinessRule security role. Once encrypted, the editor displays "Business Rule Is Encrypted" message text and enters read-only mode. The Decrypt button appears in the menu bar. It is critically important to remember and record the password because if it is forgotten, the Business Rule cannot be decrypted without the assistance of OneStream Support.
</details>

---

### Question 70 (Hard)
**201.5.6** | Difficulty: Hard

**Scenario:** An IT administrator needs to check the overall health of the OneStream environment, including web servers, application servers, and database servers. What tool should they use, and what is a key access limitation?

A) System > Tools > Database; accessible from any browser
B) System > Tools > Environment; it is only accessible via the OneStream Windows App
C) System > Logging > Task Activity; accessible from any client
D) Application > Tools > System Health; it requires the SystemMonitor security role

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Environment page (System > Tools > Environment) is used to check the overall health of the environment, which contains Web Servers, Mobile Web Servers, Application Server Sets, and Database Servers. It checks connection status and configuration, and allows users to monitor the environment, isolate bottlenecks, view properties and configuration changes, and scale application servers. A key limitation is that the Environment page is only accessible via the OneStream Windows App.
</details>

---

### Question 71 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** A finance team member creates reports in both the Excel Add-In and the OneStream Spreadsheet application. They want to know where Spreadsheet files can be saved and whether files are portable between the two tools. What is correct?

A) Spreadsheet files can only be saved to the OneStream File System; files are not portable between Excel and Spreadsheet
B) Spreadsheet files can be saved to Local Folder, OneStream File System, Application Workspace, or System Workspace; files are portable between Excel and Spreadsheet
C) Spreadsheet files can only be saved to Local Folder or OneStream File System; files are portable only from Spreadsheet to Excel, not the reverse
D) Spreadsheet files can be saved to any location; files are portable but lose all formatting when transferred

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Spreadsheet feature offers four save locations: Local Folder, OneStream File System, Application Workspace File, and System Dashboard File (System Workspace). Files are fully portable between Excel and Spreadsheet. A file authored in Spreadsheet can be saved locally and opened with the Excel Add-In, and vice versa. This portability works without issues, though the administrator may prefer to design in one tool and distribute via the other.
</details>

---

### Question 72 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** A OneStream developer needs to retrieve a single data intersection using an XF function. They are choosing between XFGetCell, XFGetCell5, and XFGetCellUsingScript. Which statement accurately describes the differences between these three functions?

A) XFGetCell supports 20 dimension parameters; XFGetCell5 supports 5 total dimensions; XFGetCellUsingScript uses named ranges instead of dimension parameters
B) XFGetCell supports all 20 dimension parameters individually; XFGetCell5 limits User Defined Dimensions to 5 instead of 8; XFGetCellUsingScript uses Member Script syntax (e.g., E#Houston:A#Sales) instead of individual parameters
C) XFGetCell and XFGetCell5 are identical except for performance; XFGetCellUsingScript requires a Business Rule to execute
D) XFGetCell retrieves data from one Cube; XFGetCell5 retrieves from up to 5 Cubes simultaneously; XFGetCellUsingScript retrieves from any data source including external databases

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFGetCell provides a separate parameter for each of the 20 dimensions (Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1-UD8), requiring every dimension to be defined. XFGetCell5 has the same functionality but limits the User Defined Dimensions to 5 instead of 8, reducing the number of parameters. XFGetCellUsingScript uses Member Script syntax (e.g., E#Houston:A#Sales:S#Actual) to specify the intersection in a concatenated string format, rather than requiring individual parameters for each dimension.
</details>

---
