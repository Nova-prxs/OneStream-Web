# Question Bank - Section 4: Presentation (14% of the Exam)

## Objective 201.4.1: Demonstrate understanding of Cube View configurations

### Question 1 (Easy)
**201.4.1** | Difficulty: Easy

How are Cube Views known in the context of reporting in OneStream?

A) Report Engines
B) Building blocks of reporting
C) Data Visualization Tools
D) Query Designers

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views are the "building blocks of reporting" in OneStream. They can be read-only, used for data editing, and used as a Data Source for various visualization mechanisms.
</details>

---

### Question 2 (Easy)
**201.4.1** | Difficulty: Easy

What is the maximum number of dimensions that can be nested in rows and columns in a Cube View?

A) 2 in rows and 2 in columns
B) 4 in rows and 4 in columns
C) 4 in rows and 2 in columns
D) 3 in rows and 3 in columns

<details>
<summary>Show answer</summary>

**Correct answer: C)**

In a Cube View, up to 4 dimensions can be nested in rows and up to 2 dimensions in columns. This limitation is important when designing complex reports.
</details>

---

### Question 3 (Easy)
**201.4.1** | Difficulty: Easy

When defining the POV in a Cube View, which has priority: the POV slider or the POV Pane?

A) The POV Pane always has priority
B) The POV slider has priority over the POV Pane
C) Both have the same priority
D) It depends on the administrator's configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The information in the POV slider takes priority over the POV Pane. This is important when configuring Cube Views to ensure the displayed data corresponds to the correct selections.
</details>

---

### Question 4 (Medium)
**201.4.1** | Difficulty: Medium

What is the correct syntax for renaming a row or column header in a Cube View using the Name() function?

A) A#CashBalance.Name("Cash Balance")
B) A#CashBalance:Name(Cash Balance)
C) A#CashBalance|Name=Cash Balance|
D) Name(Cash Balance):A#CashBalance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct syntax is to use `:Name()` at the end of the Member Filter. For example, `A#CashBalance:Name(Cash Balance)` displays "Cash Balance" as the header instead of the technical member name.
</details>

---

### Question 5 (Medium)
**201.4.1** | Difficulty: Medium

What Cube View property must be set to `True` to allow users to edit data directly in OnePlace?

A) Is Dynamic
B) Is Visible in Profiles
C) Can Modify Data
D) Can Calculate

<details>
<summary>Show answer</summary>

**Correct answer: C)**

`Can Modify Data = True` allows the Cube View to be editable in OnePlace. If set to `False`, the Cube View is read-only, although annotations (Data Attachments) are still allowed.
</details>

---

### Question 6 (Medium)
**201.4.1** | Difficulty: Medium

**Scenario:** A developer is creating a Cube View for a Workflow that must display all entities assigned to the Workflow Profile. Where should the entity reference be placed and what syntax should be used?

A) In the POV, using E#Root.WFEntities
B) In Rows or Columns, using E#Root.WFProfileEntities
C) In the POV, using E#Root.WFProfileEntities
D) In Rows or Columns, using E#WFEntities.All

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You must use `E#Root.WFProfileEntities` in Rows or Columns, NOT in the POV. The reason is that there can be more than one entity in the Workflow Profile, and the POV only accepts one member per dimension. Placing it in rows or columns allows all entities to be expanded.
</details>

---

### Question 7 (Medium)
**201.4.1** | Difficulty: Medium

What is the format priority order in a Cube View, from lowest to highest?

A) Row settings > Column settings > Cube View Default > Application Properties
B) Application Properties > Cube View Default > Column settings > Row settings > Column Row overrides > Row Column overrides
C) Cube View Default > Application Properties > Row settings > Column settings
D) Row Column overrides > Column Row overrides > Column settings > Row settings > Cube View Default > Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The priority order from lowest to highest is: Application Properties > Cube View Default > Column settings > Row settings > Column Row overrides > Row Column overrides. The most specific settings (Row Column overrides) have the highest priority and override all others.
</details>

---

### Question 8 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A Cube View has multiple nested dimensions in rows and performance is slow. Many combinations have no data. The developer wants to improve performance by filtering rows with no data before rendering. However, some columns contain dynamically calculated data. How should Sparse Row Suppression be configured?

A) Enable Allow Sparse Row Suppression = True in General Settings; no additional configuration is needed
B) Enable Allow Sparse Row Suppression = True in General Settings; for columns with dynamic data, set Use to Determine Row Suppression = True and Allow Sparse Row Suppression = True
C) Use Suppress NoData Rows = True, which is equivalent
D) Enable Paging to limit the rows processed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Sparse Row Suppression is enabled in General Settings > Common > Allow Sparse Row Suppression = True. However, this feature cannot be applied to dynamically calculated data (calculated members and Cube View math). For columns with dynamic data, you must set `Use to Determine Row Suppression = True` and `Allow Sparse Row Suppression = True` at the column level.
</details>

---

### Question 9 (Hard)
**201.4.1** | Difficulty: Hard

When does Cube View Paging activate automatically and what are its default parameters?

A) With more than 5,000 rows; Max Rows = 1,000 and Max Seconds = 10
B) With more than 10,000 unsuppressed rows; it attempts to return up to 2,000 unsuppressed rows in a maximum of 20 seconds
C) With more than 50,000 rows; Max Rows = 10,000 and Max Seconds = 60
D) It never activates automatically; it must be enabled manually

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Paging is automatically applied to the Data Explorer view when there are more than 10,000 unsuppressed rows. It attempts to return up to 2,000 unsuppressed rows in a maximum of 20 seconds. The configurable parameters are Max Unsuppressed Rows Per Page (default -1, max 100,000) and Max Seconds To Process (default -1, max 600).
</details>

---

### Question 10 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A user needs to reference a parameter called "RegionFilter" within a Cube View used as a form in a Dashboard. What is the correct syntax and what happens if the parameter is not found in the form?

A) Use {RegionFilter} and it generates an error if not found
B) Use |!RegionFilter!| and if not found in the Form, OneStream searches the application's Dashboard Parameters
C) Use @RegionFilter@ and it uses a default value if not found
D) Use $RegionFilter$ and it displays a prompt to the user

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Parameters in Cube Views are referenced using the syntax `|!ParameterName!|`. If the parameter is not found associated with the Form, OneStream automatically searches the application's Dashboard Parameters as a fallback.
</details>

---

## Objective 201.4.2: Demonstrate understanding of Workspace and Dashboard configuration

### Question 11 (Easy)
**201.4.2** | Difficulty: Easy

What is the primary purpose of a Workspace in OneStream?

A) To store consolidated financial data
B) To act as an isolated development sandbox where teams can create, modify, and test solutions without interfering with others
C) To execute consolidation processes
D) To manage end-user security

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Workspace is a structured sandbox-like development space where teams can create, modify, and test solutions without interfering with other workflows. They promote modularity, reuse, and parallel development.
</details>

---

### Question 12 (Easy)
**201.4.2** | Difficulty: Easy

What are the two levels of security in a Workspace?

A) Read Group and Write Group
B) Access Group and Maintenance Group
C) Viewer Group and Editor Group
D) User Group and Admin Group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The two security levels are: Access Group (users who can view the Workspace and its dashboards but NOT modify them) and Maintenance Group (users who can access, view AND modify the Workspace and its objects).
</details>

---

### Question 13 (Easy)
**201.4.2** | Difficulty: Easy

What does the Default Workspace contain?

A) Only administrator Workspaces
B) All legacy objects (pre-Workspaces) and Cube View Groups/Data Management Groups that are not in a new Workspace
C) Only system Dashboards
D) A backup copy of all Workspaces

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Default Workspace contains all legacy objects (created before the Workspaces functionality). Cube View Groups and Data Management Groups that are not assigned to a new Workspace are located under the Default Maintenance Unit of the Default Workspace.
</details>

---

### Question 14 (Medium)
**201.4.2** | Difficulty: Medium

What two conditions must be met for a Workspace to share its objects with another Workspace?

A) Both Workspaces must have the same Access Group
B) The source Workspace must have Is Shareable = True, AND the target Workspace must list the source name in Shared Workspace Names
C) Only a global sharing enable by the administrator is needed
D) The source Workspace must export an XML file that the target imports

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Two conditions are required: (1) the source Workspace must have the property `Is Shareable Workspace = True`, and (2) the target Workspace must list the source Workspace name in its `Shared Workspace Names` property. The Default Workspace is always shared without needing to be listed.
</details>

---

### Question 15 (Medium)
**201.4.2** | Difficulty: Medium

**Scenario:** A QA team needs to review the dashboards of two different Workspaces (Annual Operating Plan and Close and Consolidation) without being able to modify anything. What security configuration is appropriate?

A) Give them the ManageApplicationWorkspaces role
B) Assign them to the Maintenance Group of both Workspaces
C) Assign them to the Access Group of both Workspaces
D) Give them the WorkspaceAdminPage role

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Access Group allows viewing the Workspace and its dashboards without being able to modify anything (content appears greyed out, indicating read-only). The Maintenance Group would allow modification, which is not appropriate for QA. The roles mentioned in other options grant administrative permissions, not read-only access.
</details>

---

### Question 16 (Medium)
**201.4.2** | Difficulty: Medium

What syntax is used to reference a Workspace Substitution Variable?

A) @WorkspaceVar.VariableName@
B) |WSSVSuffixName|
C) ${WSSV_VariableName}
D) #WSVar(VariableName)#

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspace Substitution Variables use the WSSV prefix with the syntax `|WSSVSuffixName|`. For example, `|WSSVDeveloper|` would reference a Workspace variable that stores the developer's name.
</details>

---

### Question 17 (Medium)
**201.4.2** | Difficulty: Medium

What are the 5 types of Data Adapters available in Workspaces?

A) SQL, REST, ODBC, File, Memory
B) Cube View, Cube View MD, Method, SQL, BI-Blend
C) Grid, Chart, Table, Query, Custom
D) Direct, Cached, Streaming, Batch, Real-time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The 5 types of Data Adapters are: Cube View (preconfigured, the most common), Cube View MD (multidimensional fact table for BI Viewer), Method (Business Rule or out-of-the-box queries), SQL (database queries), and BI-Blend (interface for BI Blend tables).
</details>

---

### Question 18 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** Two developers work in separate Workspaces. Both have created a Dashboard called "FinancialOverview". Is this valid and why?

A) No, Dashboard names must be unique across the entire application
B) Yes, because Workspaces allow reusing the same object name as long as they are in separate Workspaces
C) Only if one is a Shortcut to the other
D) Only if they are in the same Maintenance Unit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspaces allow using the same object name multiple times, as long as they are in separate Workspaces. This applies to Maintenance Units, Dashboards, Cube Views, Data Management Jobs, Components, Data Adapters, Files, Strings, and Assemblies. This is one of the key benefits of Workspaces for resolving naming conflicts.
</details>

---

### Question 19 (Hard)
**201.4.2** | Difficulty: Hard

What are the three Security Roles specific to Workspaces and what permission does each grant?

A) WorkspaceRead (view), WorkspaceWrite (modify), WorkspaceAdmin (administer)
B) AdministerApplicationWorkspaceAssemblies (create/modify Assemblies), ManageApplicationWorkspaces (create/modify Workspaces), WorkspaceAdminPage (access to Workspaces page in Application tab)
C) WorkspaceCreator (create), WorkspaceEditor (edit), WorkspaceViewer (view)
D) DeveloperRole (development), QARole (testing), AdminRole (administration)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three Security Roles are: AdministerApplicationWorkspaceAssemblies (allows creating and modifying Workspace Assemblies), ManageApplicationWorkspaces (allows creating and modifying Workspaces), and WorkspaceAdminPage (a UI role that provides access to the Workspaces page from the Application tab).
</details>

---

### Question 20 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** An administrator notices that Cube Views created in a new Workspace do not appear on the Cube Views page of the Application tab. Developers report they can only see these Cube Views from within the Workspace itself. What is the explanation?

A) The Cube Views have a Profile configuration error
B) Cube Views created outside the Default Workspace are NOT accessible from the Application tab's Cube View page; this is expected behavior
C) The Workspace does not have correct permissions
D) The application needs to be restarted for them to appear

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is expected and documented behavior: Cube Views created outside the Default Workspace are NOT accessible from the Application tab's Cube Views page. Only Cube Views in the Default Workspace appear there. Cube Views in other Workspaces are accessed from the corresponding Workspace. However, Dashboard Profiles and Cube View Profiles are global and remain accessible.
</details>

---

## Objective 201.4.3: Describe the steps to create a Report Book

### Question 21 (Easy)
**201.4.3** | Difficulty: Easy

What are the three types of Report Books available in OneStream?

A) Word, Excel, PowerPoint
B) PDF Books, Excel Books, Zip File Books
C) HTML Books, PDF Books, CSV Books
D) Standard Books, Custom Books, Template Books

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three types of Report Books are: PDF Books (extension .pdfBook), Excel Books (extension .xlBook), and Zip File Books (extension .zipBook).
</details>

---

### Question 22 (Easy)
**201.4.3** | Difficulty: Easy

What is the file extension of a PDF Book in OneStream?

A) .pdfReport
B) .xfDoc.pdfBook
C) .onestream.pdf
D) .rptBook.pdf

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct extension for a PDF Book is `ReportBookName.xfDoc.pdfBook`. Similarly, Excel Books use `.xfDoc.xlBook` and Zip File Books use `.xfDoc.zipBook`.
</details>

---

### Question 23 (Easy)
**201.4.3** | Difficulty: Easy

What is the recommended method for downloading a complete Report Book as PDF?

A) Print > Save as PDF
B) Export > PDF
C) Download Combined PDF File
D) Save > PDF Format

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Download Combined PDF File is the recommended method for downloading books as PDF. This option is available in the Book Preview Toolbar and generates a combined PDF of all pages in the book.
</details>

---

### Question 24 (Medium)
**201.4.3** | Difficulty: Medium

**Scenario:** A developer is creating a large Report Book with many Cube Views and notices that performance is slow when generating the book. What configuration can improve performance?

A) Reduce the number of pages
B) Set Determine Parameters from Content = False and manually specify the Required Input Parameters
C) Use only Excel Books instead of PDF Books
D) Remove all Loops

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For large books, setting `Determine Parameters from Content = False` and manually specifying the required parameters in `Required Input Parameters` (comma-separated list) improves performance. When this property is True, the system analyzes all content to determine parameters, which is expensive for large books.
</details>

---

### Question 25 (Medium)
**201.4.3** | Difficulty: Medium

What are the three types of Loop Definition available in a Report Book?

A) For Each, While, Do-While
B) Comma Separated List, Dashboard Parameter, Member Filter
C) Sequential, Parallel, Recursive
D) Entity Loop, Time Loop, Scenario Loop

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three types of Loop Definition are: Comma Separated List (comma-separated values like `Houston, Clubs, [Houston Heights]`), Dashboard Parameter (parameter name), and Member Filter (using Member Filter syntax like `E#Frankfurt, E#Houston` or `E#[NA Clubs].Base`).
</details>

---

### Question 26 (Medium)
**201.4.3** | Difficulty: Medium

What variable is used to reference the current value of a Loop in a Report Book?

A) |CurrentLoop|
B) |LoopValue|
C) |Loop1Variable|
D) |Iterator1|

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The variable `|Loop1Variable|` references the current value of the first Loop. For nested Loops, use `|Loop2Variable|`, `|Loop3Variable|`, and `|Loop4Variable|`. Additionally, `|Loop1Display|` returns the description and `|Loop1Index|` returns the numeric index.
</details>

---

### Question 27 (Medium)
**201.4.3** | Difficulty: Medium

What types of items can be added to a Report Book?

A) Only Cube Views and Dashboards
B) File, Excel Export Item, Report, Loop, Conditional Statements, and Change Parameters
C) Only PDF and Excel files
D) Only Reports and Charts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The types of items that can be added include: File (with various Source Types), Excel Export Item (for XLBooks), Report (Cube View, Dashboard Chart, Dashboard Report, etc.), Loop, Conditional Statements (If, Else If, Else), and Change Parameters.
</details>

---

### Question 28 (Hard)
**201.4.3** | Difficulty: Hard

**Scenario:** A developer needs to create a Report Book that generates a financial report for each entity in the "NA Clubs" group, but must include an additional page only for the "Frankfurt" entity. What combination of items should they use?

A) A Loop with a Comma Separated List of all entities with Frankfurt at the end
B) A Loop with Member Filter `E#[NA Clubs].Base`, a Change Parameters inside the Loop, and an If Statement with condition `(|Loop1Variable| = [Frankfurt])` for the additional page
C) Two separate Report Books combined manually
D) A Loop with Dashboard Parameter and a manual filter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct solution combines a Loop with Member Filter `E#[NA Clubs].Base` to iterate through each entity, a Change Parameters inside the Loop to update the POV with `E#[|Loop1Variable|]`, and an If Statement with the condition `(|Loop1Variable| = [Frankfurt])` to add additional content only for Frankfurt. If Statements allow conditional logic within Loops.
</details>

---

### Question 29 (Hard)
**201.4.3** | Difficulty: Hard

**Scenario:** A team needs to create an Excel Book that exports data from multiple Cube Views, each on a separate tab. What type of item should they use and what important limitation should they consider?

A) Use Report items with Cube View type; there are no limitations
B) Use Excel Export Items specifying each Cube View; Excel Books do NOT support Report items or File items
C) Use File items pointing to Excel exports; there are no limitations
D) Use Loop items with automatic exports; Excel Books do not support Loops

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Excel Books (extension .xlBook), you must use Excel Export Items, where each Cube View is exported to a separate tab. The important limitation is that Excel Books do NOT support Report items or File items. They can only contain Excel Export Items.
</details>

---

### Question 30 (Hard)
**201.4.3** | Difficulty: Hard

What are the available Source Type options when adding a File item to a Report Book?

A) Local File, Network File, Cloud File
B) URL, Application Workspace File, System Workspace File, Application Database File, System Database File, File Share File
C) OneDrive, SharePoint, Local, FTP
D) Import File, Export File, Template File

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available Source Types for File items are: URL, Application Workspace File, System Workspace File, Application Database File, System Database File, and File Share File. These options allow including files from multiple locations within the OneStream ecosystem.
</details>

---

### Question 31 (Easy)
**201.4.1** | Difficulty: Easy

How are system Substitution Variables referenced in a Cube View?

A) With braces: {WFTime}
B) With pipes: |WFTime|
C) With dollar signs: $WFTime$
D) With brackets: [WFTime]

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Substitution Variables are referenced using pipes (vertical bars). For example: `|WFTime|`, `|UserName|`, `|POVTime|`. The prefixes indicate the source of the value: WF (Workflow), POV (Cube POV), Global, etc.
</details>

---

### Question 32 (Medium)
**201.4.1** | Difficulty: Medium

What Member Filter Builder function is used to get the prior period of the current POV?

A) T#Previous1
B) T#POVPrior1
C) T#LastPeriod
D) T#POVMinus1

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The function `T#POVPrior1` returns the prior period of the current POV. Time Functions include POV, WF, Global, and General types. Another similar function is `T#YearPrior1(|PovTime|)` to get the prior year.
</details>

---

### Question 33 (Medium)
**201.4.1** | Difficulty: Medium

What types of Spreading are available when right-clicking a cell in an editable Cube View?

A) Only Copy and Paste
B) Fill, Clear Data, Factor, Accumulate, Even Distribution, Proportional Distribution, 445/454/544 Distribution
C) Only Even Distribution and Proportional Distribution
D) Linear, Exponential, Logarithmic

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available Spreading types are: Fill, Clear Data, Factor, Accumulate, Even Distribution, Proportional Distribution, and calendar distribution types 445/454/544. These allow distributing values in different ways across the Cube View cells.
</details>

---

### Question 34 (Hard)
**201.4.2** | Difficulty: Hard

What are the Dashboard types available in OneStream and what is the difference between Top Level and Embedded?

A) Only Standard and Custom exist
B) Use Default, Top Level, Top Level Without Parameter Prompts, Embedded, Embedded Dynamic, Embedded Dynamic Repeater, Custom Control; Top Level is exposed in the OnePlace main menu while Embedded is nested inside another Dashboard
C) Only Top Level and Embedded; both are accessible from OnePlace
D) Standard, Premium, Enterprise; they differ only in the number of components

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Dashboard types are: Use Default, Top Level (exposed in the OnePlace main menu), Top Level Without Parameter Prompts (without prompting for parameters), Embedded (nested inside another Dashboard, not directly visible in OnePlace), Embedded Dynamic (for Workspace Assembly objects), Embedded Dynamic Repeater (multiple instances with Template Repeat Items), and Custom Control (uses Event Listeners).
</details>

---

### Question 35 (Easy)
**201.4.2** | Difficulty: Easy

What type of Data Adapter is the most common in Workspaces?

A) SQL
B) Method
C) Cube View
D) BI-Blend

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Cube View type Data Adapter is the most common. It uses a preconfigured Cube View as a data source for Dashboard components.
</details>

---

### Question 36 (Medium)
**201.4.2** | Difficulty: Medium

What are the 6 types of Parameters available in Workspaces?

A) Text, Number, Date, Boolean, List, Custom
B) Literal Value, Input Value, Delimited List, Bound List, Member List, Member Dialog
C) String, Integer, Float, Array, Object, Enum
D) Simple, Complex, Dynamic, Static, Computed, Reference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The 6 types of Parameters are: Literal Value (explicit value), Input Value (holding parameter), Delimited List (manual list), Bound List (member lists using Method Types), Member List (uses Member Filter Builder), and Member Dialog (list with selection tree).
</details>

---

### Question 37 (Easy)
**201.4.3** | Difficulty: Easy

After creating a Report Book, what are the ways to use it?

A) It can only be viewed in the Application tab
B) Add it to other books, dashboards (via Book Viewer or File Viewer), send it by email via OneStream Parcel Service, generate it via Data Management, store it on FileShare/desktop/dashboard file
C) It can only be exported as PDF
D) It can only be viewed in OnePlace

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Books are very versatile: they can be added to other books, incorporated into dashboards via Book Viewer or File Viewer, sent by email via OneStream Parcel Service, generated by running a Data Management sequence, and stored on FileShare, desktop, or as a dashboard file.
</details>

---

### Question 38 (Medium)
**201.4.3** | Difficulty: Medium

**Scenario:** A developer has a Loop in a Report Book that iterates through entities. They need the report's POV to change to the current Loop entity without modifying the original Cube View. What should they use and what is the syntax?

A) Edit the Cube View directly for each entity
B) Add a Change Parameters inside the Loop with type POV and syntax such as E#[|Loop1Variable|]:A#Sales
C) Create a Cube View Shortcut for each entity
D) Use an external Dashboard Parameter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You should use Change Parameters inside the Loop with type POV. The syntax `E#[|Loop1Variable|]:A#Sales` changes the entity to the current Loop value without modifying the original Cube View. Change Parameters is required within Loops to update the POV variable.
</details>

---

### Question 39 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A user needs to create a Cube View that displays the children of "Income Statement" excluding members whose name contains "Tax". What Member Filter syntax should they use?

A) A#[Income Statement].Children NOT Tax
B) A#[Income Statement].Children.Remove(A#[Income Statement].Children.Where(Name Contains Tax))
C) A#[Income Statement].Descendants.Where(Name Contains Tax).Exclude
D) A#[Income Statement].Children, -A#Tax

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Member Filter Builder supports expansion functions like `.Children` and removal functions with `.Remove()`. Combined with `.Where(Name Contains Tax)`, you can get the list of children and remove those containing "Tax" in their name. Where clauses allow filtering by Name Contains, StartsWith, EndsWith, HasChildren, etc.
</details>

---

### Question 40 (Hard)
**201.4.3** | Difficulty: Hard

What is the correct syntax for an If Statement in a Report Book that includes multiple conditions?

A) IF Loop1Variable = Frankfurt AND Loop1Variable = Houston
B) (|Loop1Variable| = [Frankfurt]) Or (|Loop1Variable| = [Houston])
C) |Loop1Variable| == "Frankfurt" || |Loop1Variable| == "Houston"
D) If(Loop1Variable, Frankfurt, Houston)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct syntax uses parentheses for each condition and Or/And logical operators. For example: `(|Loop1Variable| = [Frankfurt]) Or (|Loop1Variable| = [Houston])`. You can also combine different variables: `(|!UserName!| = Administrator) Or (|!UserName!| = JSmith)`.
</details>

---

## NEW QUESTIONS (41-84)

## Objective 201.4.1: Cube View configurations (continued)

### Question 41 (Easy)
**201.4.1** | Difficulty: Easy

What keyboard shortcut saves data in a Cube View within OnePlace?

A) CTRL+Enter
B) CTRL+S
C) F5
D) CTRL+D

<details>
<summary>Show answer</summary>

**Correct answer: B)**

CTRL+S is the keyboard shortcut to save data in a Cube View. This is important when working with editable Cube Views (`Can Modify Data = True`) where users enter or modify data.
</details>

---

### Question 42 (Medium)
**201.4.1** | Difficulty: Medium

What are the scaling shortcuts available when entering data into an editable Cube View?

A) K for hundred, M for thousand, B for million
B) k for thousand, m for million, b for billion
C) k for ten, m for hundred, b for thousand
D) There are no scaling shortcuts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The scaling shortcuts are: k = 1,000 (thousand), m = 1,000,000 (million), and b = 1,000,000,000 (billion). For example, entering "5k" sets the cell value to 5,000.
</details>

---

### Question 43 (Easy)
**201.4.1** | Difficulty: Easy

What property must be set to `True` on a Cube View to enable it for use with a Dynamic Cube View Service via Workspace Assembly?

A) Can Modify Data
B) Is Visible in Profiles
C) Is Dynamic
D) Allow Sparse Row Suppression

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Setting `Is Dynamic = True` on a Cube View enables the Dynamic Cube View Service, allowing the Cube View to be manipulated programmatically through a Workspace Assembly.
</details>

---

### Question 44 (Medium)
**201.4.1** | Difficulty: Medium

Which GetDataCell expression types are available for Cube View calculated columns and rows?

A) Sum, Average, Count, Min, Max
B) Variance, VariancePercent, BWDiff, BWPercent, Divide, Subtraction, Addition, BR# (custom function)
C) Only Addition and Subtraction
D) Average, Median, StdDev, Percentile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

GetDataCell expressions include: Variance, VariancePercent, BWDiff (budget vs. actual difference), BWPercent, Divide, Subtraction, Addition, and BR# for custom Business Rule functions. These are used to create calculated columns and rows within Cube Views.
</details>

---

### Question 45 (Medium)
**201.4.1** | Difficulty: Medium

What is the purpose of the `CVMathOnly` option when configuring a Cube View column?

A) It makes the column editable for math input only
B) It hides the column from display but keeps it available for math expressions in other columns
C) It restricts the column to integer values only
D) It enables formula auditing for the column

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The `CVMathOnly` option hides a column from the rendered display but keeps it available for use in math expressions by other columns or rows. This is useful when intermediate calculation columns are needed but should not be visible to end users.
</details>

---

### Question 46 (Medium)
**201.4.1** | Difficulty: Medium

When configuring Navigation Links in a Cube View, which properties must be set and what parameter syntax is used?

A) Enable Navigation = True; uses {dimensionName} syntax
B) Enable Report Navigation Link = True and Include Default NavLink Parameters = True; uses |!dimensionNavLink!| syntax
C) Allow Drill = True; uses @dimension@ syntax
D) Navigation Mode = Active; uses $NavParam$ syntax

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To enable Navigation Links, set `Enable Report Navigation Link = True` on the row/column, and `Include Default NavLink Parameters = True` to automatically generate dimension parameters. The default NavLink parameters use the `|!dimensionNavLink!|` syntax pattern.
</details>

---

### Question 47 (Medium)
**201.4.1** | Difficulty: Medium

What property must be enabled on a Data Adapter to allow drill-down from Cube Views displayed in dashboards?

A) Allow Drill Through = True
B) Include Row Navigation Link = True
C) Enable Data Exploration = True
D) Cube View Drill Mode = Active

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Setting `Include Row Navigation Link = True` on the Data Adapter enables drill-down navigation from Cube Views rendered within dashboard components. This creates clickable links on rows that navigate to linked reports.
</details>

---

### Question 48 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A developer is building a Cube View with 3 nested dimensions in rows. They want the first two levels to be automatically expanded when the Cube View loads, but the third level should be collapsed. Additionally, the Cube View will be shown inside a dashboard and needs red tick marks on cells that have data attachments. What configuration is needed?

A) Set TreeExpansionLevel = 3 and Include Cell Attachment Status = True
B) Set TreeExpansionLevel = 2 and Include Cell Attachment Status = True
C) Set AutoExpand = 2 and Show Attachments = True
D) Set ExpandDepth = 2 and AttachmentIndicator = Enabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

TreeExpansionLevel controls how many nested row levels are automatically expanded (values 1-4). Setting it to 2 expands the first two levels while keeping the third collapsed. `Include Cell Attachment Status = True` displays a red tick mark on cells that have data attachments. Both settings are configured in the Cube View properties.
</details>

---

### Question 49 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A Cube View needs conditional formatting so that cells with no data are displayed with a grey background, even-numbered rows have a light blue background, and cells where the amount is negative are shown in red font. Which conditional formatting property filters should the developer use?

A) NoDataStyle, AlternateRows, NegativeValues
B) IsNoData for grey background, IsRowNumberEven for light blue, CellAmount for red font on negatives
C) EmptyCell, RowParity, ValueSign
D) DataStatus, RowIndex, CellValue

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Conditional formatting property filters include: `IsNoData` to identify cells without data, `IsRowNumberEven` to target alternating rows, and `CellAmount` to apply formatting based on cell values (e.g., negative amounts). Other available filters include IsRealData, MemberName, HeaderDisplayText, IndentLevel, and ExpandedRowNum.
</details>

---

### Question 50 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A developer has two Cube Views that share the same row definitions — one for Budget data and one for Actual data. Maintaining the same rows in both views is becoming error-prone. What feature should they use to ensure consistency?

A) Create a Template Cube View and link both views to it
B) Use Cube View row sharing — define the rows in one Cube View and reference them from the other
C) Export and import the row definitions between views
D) Use a Business Rule to synchronize the rows at runtime

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views support row sharing and column sharing, where row or column definitions are defined in one Cube View and referenced from another. This ensures consistency across multiple views and eliminates the need for duplicate maintenance.
</details>

---

### Question 51 (Medium)
**201.4.1** | Difficulty: Medium

In a Cube View, what are the available Cell Types that can be configured for data entry?

A) Only Text Box
B) Text Box (default/numerical), Combo Box (with List Parameter), Date, and Date Time
C) Text, Number, Currency, Percentage
D) Input, Dropdown, Calendar, Checkbox

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available Cell Types for Cube View cells are: Text Box (default, for numerical data entry), Combo Box (used in conjunction with a List Parameter for dropdown selection), Date, and Date Time. The Combo Box type requires an associated parameter to populate the dropdown options.
</details>

---

### Question 52 (Medium)
**201.4.1** | Difficulty: Medium

What Cube View Visibility settings are available in Cube View Profiles?

A) Visible and Hidden
B) Always, a specific location, or Never
C) Public, Private, Restricted
D) Global, Workspace, Local

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube View Profiles support three Visibility settings: Always (visible in all contexts), a specific location (visible only in a designated area), or Never (hidden from all views). These profiles are global — they are not workspace-specific.
</details>

---

### Question 53 (Easy)
**201.4.1** | Difficulty: Easy

What math shortcut keywords can be used when entering data in a Cube View cell?

A) Only + and - symbols
B) add, sub, div, mul, increase/in/gr, decrease/de, percent/per, power/pow
C) plus, minus, times, divideby
D) There are no math shortcuts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views support math shortcut keywords: add, sub (subtract), div (divide), mul (multiply), increase/in/gr (increase by percentage), decrease/de (decrease by percentage), percent/per (percentage), and power/pow. For example, typing "add 100" adds 100 to the current cell value.
</details>

---

### Question 54 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A developer is building a Cube View that uses CVC (Cube View Column) and CVR (Cube View Row) math expressions. They have Column A showing Actual values and Column B showing Budget values. They want Column C to show the variance (Actual - Budget) and Column D to show variance percentage, but Column B should be hidden from the final output since it is only needed for calculations. How should this be configured?

A) Delete Column B after creating the math expressions
B) Set Column B to `CVMathOnly`, configure Column C using a Subtraction GetDataCell referencing columns A and B, and Column D using VariancePercent GetDataCell
C) Use a Business Rule to hide Column B at runtime
D) Create Column C with manual formulas and remove Column B

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Set Column B to `CVMathOnly` to hide it from display while keeping it available for calculations. Column C uses a Subtraction (or Variance) GetDataCell expression referencing the Actual (A) and Budget (B) columns. Column D uses VariancePercent GetDataCell. The CVMathOnly option is specifically designed for intermediate calculation columns that should not be visible to users.
</details>

---

## Objective 201.4.2: Workspace and Dashboard configuration (continued)

### Question 55 (Easy)
**201.4.2** | Difficulty: Easy

What component type is used to prevent parameter prompts from appearing and pass parameters silently between dashboards?

A) Hidden Parameter
B) Supplied Parameter
C) Silent Parameter
D) Pass-Through Parameter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Supplied Parameter component is a hidden component that prevents parameter prompts from appearing. It passes parameters silently from one dashboard to another, allowing seamless navigation without user interaction for parameter selection.
</details>

---

### Question 56 (Medium)
**201.4.2** | Difficulty: Medium

Where does the Logo component in a dashboard pull its image from?

A) A file uploaded to the Workspace
B) Application Properties
C) The user's local machine
D) A URL specified in the component

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Logo component pulls its image from Application Properties. This ensures that the application logo is consistent across all dashboards without requiring separate image management. This is one of the 38 available component types in OneStream dashboards.
</details>

---

### Question 57 (Medium)
**201.4.2** | Difficulty: Medium

What is the best practice for naming and sorting parameters in a dashboard maintenance unit?

A) Use sequential numbering and alphabetical sort
B) Prefix names with "Param", use staggered sort order with gaps of 20, and avoid special characters or dashes
C) Use short cryptic names and sort by creation date
D) No naming convention is needed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Best practices for parameters include: prefixing names with "Param" (e.g., ParamEntity, ParamAccount), using a staggered sort order with gaps of 20 (e.g., 20, 40, 60) to allow insertion of new parameters later, and avoiding special characters or dashes in parameter names.
</details>

---

### Question 58 (Medium)
**201.4.2** | Difficulty: Medium

What happens when a parameter uses the `|!!ParameterName!!|` syntax (double exclamation marks) instead of `|!ParameterName!|`?

A) It makes the parameter required
B) It references the display item of a Delimited List parameter instead of the value item
C) It creates a parameter with validation
D) It makes the parameter case-sensitive

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The double exclamation mark syntax `|!!ParameterName!!|` references the display item of a Delimited List parameter, while the single exclamation mark syntax `|!ParameterName!|` references the value item. This distinction is important when the display text differs from the underlying value in a Delimited List.
</details>

---

### Question 59 (Medium)
**201.4.2** | Difficulty: Medium

How are nested parameters used in OneStream dashboards?

A) Parameters are placed inside other parameters physically
B) One parameter references another parameter to provide granular filtering — the selection in one parameter narrows the options in another
C) Parameters are grouped into folders
D) Nested parameters are created using XML configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Nested parameters allow one parameter to reference another for granular filtering. For example, a Region parameter selection can filter the options available in an Entity parameter, so only entities belonging to the selected region are shown.
</details>

---

### Question 60 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer in Workspace "ProjectAlpha" needs to display a dashboard from Workspace "SharedServices" inside one of their dashboards. The SharedServices Workspace has `Is Shareable Workspace = True`, and ProjectAlpha lists "SharedServices" in its `Shared Workspace Names`. What are the two methods to display the shared dashboard?

A) Copy the dashboard to ProjectAlpha and modify it
B) Use an Embedded Dashboard component or a Button component configured to Open Dialog, referencing the dashboard with `SharedServices.DashboardName` syntax
C) Export from SharedServices and import to ProjectAlpha
D) Create a Cube View that references the shared dashboard

<details>
<summary>Show answer</summary>

**Correct answer: B)**

There are two methods to display shared dashboards from another Workspace: (1) Use an Embedded Dashboard component that references the shared dashboard, or (2) Use a Button component configured with the Open Dialog action. Both methods can reference the shared dashboard using Direct Reference syntax: `WorkspaceName.ObjectName` (e.g., `SharedServices.DashboardName`).
</details>

---

### Question 61 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** An architect is designing a dashboard solution and needs to decide between using the Embedded Dynamic Repeater (no-code) approach and the Embedded Dynamic (code-based) approach for generating dynamic components. What key difference determines which approach to use?

A) The Embedded Dynamic Repeater is faster; the code-based approach is slower
B) The Embedded Dynamic Repeater uses Component Template Repeat Items with `~!templateParam!~` syntax for settings that accept text values, while the code-based approach uses Assembly Services and can modify settings that do not accept text values (such as save actions, POV actions, and navigation actions)
C) The code-based approach is only for SQL queries
D) There is no functional difference; it is a matter of personal preference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Embedded Dynamic Repeater is a no-code approach using Component Template Repeat Items and template parameters with `~!templateParam!~` syntax. It works well for settings that accept text values. The code-based Embedded Dynamic approach uses Assembly Services (Dynamic Dashboard Service) and can modify settings that don't accept text values, such as save actions, POV actions, and navigation actions. The code-based approach provides more flexibility for complex scenarios.
</details>

---

### Question 62 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer is configuring a Dynamic Dashboard using the code-based approach with Workspace Assemblies. They need to set the Workspace Assembly Service property on the dashboard. What is the correct syntax for this property, and what are the key methods they will need to implement?

A) Service = "AssemblyName"; methods: CreateDashboard(), RenderDashboard()
B) Workspace Assembly Service = "AssemblyName.FactoryName"; key methods include GetEmbeddedDynamicDashboard, GetDynamicComponentsForDynamicDashboards, GetDynamicAdaptersForDynamicComponent, GetDynamicCubeViewForDynamicAdapter, GetDynamicParametersForDynamicComponent
C) Assembly Reference = "Namespace.Class"; methods: Initialize(), Execute()
D) Dynamic Service = "WorkspaceName.ServiceName"; methods: BuildDashboard(), LoadComponents()

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workspace Assembly Service property uses the syntax `AssemblyName.FactoryName`. The key methods in the Dynamic Dashboard Service include: `GetEmbeddedDynamicDashboard` (entry point), `GetDynamicComponentsForDynamicDashboards` (defines components), `GetDynamicAdaptersForDynamicComponent` (defines data adapters), `GetDynamicCubeViewForDynamicAdapter` (defines cube views), and `GetDynamicParametersForDynamicComponent` (defines parameters). These methods work together to programmatically generate dashboard content.
</details>

---

### Question 63 (Medium)
**201.4.2** | Difficulty: Medium

What is the template parameter syntax used in Embedded Dynamic Repeater dashboards, and how does it differ from standard parameter syntax?

A) Both use |!ParameterName!| — there is no difference
B) Template parameters use `~!templateParam!~` while standard parameters use `|!ParameterName!|`
C) Template parameters use {{templateParam}} while standard parameters use |!ParameterName!|
D) Template parameters use <%templateParam%> while standard parameters use |!ParameterName!|

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Embedded Dynamic Repeater dashboards use template parameter syntax `~!templateParam!~` (tilde and exclamation marks) for Component Template Repeat Items, which is different from the standard parameter syntax `|!ParameterName!|` (pipes and exclamation marks) used in regular dashboards and Cube Views.
</details>

---

### Question 64 (Medium)
**201.4.2** | Difficulty: Medium

What fields on a Workspace are used to store string values that can be referenced in Workspace Assemblies?

A) Variables 1-8
B) Text1 through Text8
C) String Fields A-H
D) Custom Properties 1-8

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspaces have Text1 through Text8 fields that store string values. These values can be referenced in Workspace Assemblies, providing a way to pass configuration data from the Workspace to assembly code without hardcoding values.
</details>

---

### Question 65 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer is extracting a Workspace for deployment to another environment. What happens when a Workspace is extracted, and what items are included in the package?

A) Only dashboards are exported as individual XML files
B) Extracting a Workspace packages all related items — dashboards, cube views, data management jobs, assemblies, parameters, components, data adapters, files, and strings — in a single XML file
C) Only the Workspace configuration is exported; objects must be exported separately
D) A ZIP file is created with separate folders for each object type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extracting a Workspace packages all related items into a single XML file. This includes dashboards, cube views, data management jobs, assemblies, parameters, components, data adapters, files, strings, and maintenance units. This comprehensive packaging simplifies deployment and migration between environments.
</details>

---

### Question 66 (Easy)
**201.4.2** | Difficulty: Easy

How many component types are available for building dashboards in OneStream?

A) 12
B) 25
C) 38
D) 50

<details>
<summary>Show answer</summary>

**Correct answer: C)**

There are 38 component types available for building dashboards in OneStream. These range from data display components (Cube View, BI Viewer, Chart, Pivot Grid) to input components (Text Box, Combo Box, Date Selector) to layout components (Label, Image, Logo) and more.
</details>

---

### Question 67 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer needs to use Workspace Assemblies across different Workspaces. What Workspace properties enable cross-referencing of assemblies between Workspaces?

A) Shared Assembly Names and Assembly Access Group
B) Namespace Prefix and Imports Namespace 1-8
C) Assembly Sharing = True and Target Workspace Names
D) Cross-Reference Mode and Assembly Path

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Namespace Prefix property and the Imports Namespace 1-8 properties on a Workspace enable cross-referencing of assemblies between Workspaces. The Namespace Prefix identifies the Workspace's assembly namespace, while Imports Namespace 1-8 allow referencing assembly namespaces from other Workspaces.
</details>

---

## Objective 201.4.3: Report Book configuration (continued)

### Question 68 (Easy)
**201.4.3** | Difficulty: Easy

What are the types of Report items that can be added to a PDF Report Book?

A) Only Cube View Reports
B) Cube View, Dashboard Chart, Dashboard Report, System Dashboard Chart, System Dashboard Report
C) PDF, Word, Excel, PowerPoint
D) Table, Chart, Image, Text

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Report item types available for PDF Report Books include: Cube View, Dashboard Chart, Dashboard Report, System Dashboard Chart, and System Dashboard Report. These allow incorporating various OneStream visualization outputs into the book.
</details>

---

### Question 69 (Medium)
**201.4.3** | Difficulty: Medium

What are the four types of Change Parameters items available in a Report Book?

A) Entity, Account, Time, Scenario
B) Workflow, POV, Variables (up to 10), Parameters
C) Input, Output, Filter, Sort
D) User, Role, Group, Application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four types of Change Parameters items are: Workflow (changes the Workflow context), POV (changes Point of View dimensions), Variables (up to 10 custom variables), and Parameters (changes dashboard parameter values). These allow dynamic modification of the report context during book generation.
</details>

---

### Question 70 (Hard)
**201.4.3** | Difficulty: Hard

**Scenario:** A developer has a Report Book with a Loop that iterates through 50 entities using Member Filter. Inside the loop, they need to display the entity description as a header on each page. What Loop variable provides the description, and what variable provides the numeric position of the current iteration?

A) |Loop1Name| for description and |Loop1Count| for position
B) |Loop1Display| for description and |Loop1Index| for numeric position
C) |Loop1Description| for description and |Loop1Position| for position
D) |Loop1Label| for description and |Loop1Number| for position

<details>
<summary>Show answer</summary>

**Correct answer: B)**

`|Loop1Display|` returns the description (display text) of the current Loop member, while `|Loop1Index|` returns the numeric index (position) of the current iteration. Combined with `|Loop1Variable|` which returns the member name/value, these three variables provide complete context about the current loop iteration.
</details>

---

## Objective 201.4.4: Describe Extensible Documents (XFDs, Quick Views)

### Question 71 (Easy)
**201.4.4** | Difficulty: Easy

What file types are supported by the Extensible Documents framework in OneStream?

A) Only Word and Excel
B) Word (.xfdoc.docx), Excel (.xfdoc.xlsx), PowerPoint (.xfdoc.pptx), and Text (.xfdoc.txt)
C) PDF, Word, Excel, HTML
D) Only Excel and Text

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Extensible Documents framework supports four file types: Word documents (.xfdoc.docx), Excel workbooks (.xfdoc.xlsx), PowerPoint presentations (.xfdoc.pptx), and Text files (.xfdoc.txt). This framework integrates OneStream with Microsoft Office and text files.
</details>

---

### Question 72 (Easy)
**201.4.4** | Difficulty: Easy

What types of content can be inserted into an Extensible Document?

A) Only text and numbers
B) Cube Views, Excel Sheets/Named Ranges, Word documents, Rich Text documents, Text documents, and Reports
C) Only Cube Views
D) Only images and charts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extensible Documents can incorporate multiple content types: Cube Views, Excel Sheets and Named Ranges, Word documents, Rich Text documents, Text documents, and Reports. This makes them versatile for creating comprehensive financial documents.
</details>

---

### Question 73 (Easy)
**201.4.4** | Difficulty: Easy

What is the maximum size for an Excel Named Range that can be inserted into an Extensible Document?

A) 100 rows x 10 columns
B) 500 rows x 50 columns
C) 1,000 rows x 100 columns
D) There is no limit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Excel Named Ranges inserted into Extensible Documents have a maximum size of 500 rows by 50 columns. This limitation should be considered when designing Excel-based content for extensible documents.
</details>

---

### Question 74 (Medium)
**201.4.4** | Difficulty: Medium

What is the XFCell function used for in Extensible Documents, and where is it typically used?

A) XFCell formats cells in Excel documents
B) XFCell retrieves a single cell's data for use in text-based documents (Word and PowerPoint), using syntax like `XFCell(A#20500:E#Clubs)`
C) XFCell creates new cells in a Cube View
D) XFCell validates cell data types

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFCell retrieves single cell data from OneStream for use in text-based documents such as Word and PowerPoint. The syntax specifies dimension members to identify the data point, for example: `XFCell(A#20500:E#Clubs)`. This allows embedding individual data values within narrative text documents.
</details>

---

### Question 75 (Medium)
**201.4.4** | Difficulty: Medium

What is the difference between XFGetCell and XFGetCellVolatile in Excel-based Extensible Documents?

A) XFGetCell is for text; XFGetCellVolatile is for numbers
B) XFGetCell is the standard Excel retrieve function; XFGetCellVolatile is used when charts need to refresh with updated data
C) XFGetCell reads data; XFGetCellVolatile writes data
D) There is no functional difference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFGetCell is the standard function for retrieving data from OneStream into Excel cells. XFGetCellVolatile serves the same data retrieval purpose but is specifically used when charts need to refresh with updated data, as the volatile flag forces Excel to recalculate and refresh chart visualizations.
</details>

---

### Question 76 (Medium)
**201.4.4** | Difficulty: Medium

How are Document Variables inserted into Word-based Extensible Documents?

A) Using OneStream-specific tags in the document
B) Using Quick Parts > Field > DocVariable in Word or the Text Editor
C) Using macros in the Word template
D) Using the Insert > Reference menu

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Document Variables are inserted into Word-based Extensible Documents using Quick Parts > Field > DocVariable. This standard Word functionality integrates with OneStream's extensible document framework to dynamically populate document content.
</details>

---

### Question 77 (Medium)
**201.4.4** | Difficulty: Medium

What keyboard shortcut toggles the display of field codes in an Extensible Document, and what merge switch preserves formatting?

A) CTRL+F9 to toggle; \*FORMAT to preserve
B) Alt+F9 (or Alt+Fn+F9) to toggle; \*MERGEFORMAT to preserve formatting
C) F5 to toggle; \*KEEP to preserve
D) CTRL+Shift+F to toggle; \*STYLE to preserve

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Alt+F9 (or Alt+Fn+F9 on some keyboards) toggles the Show/Hide Field Codes display in Word-based extensible documents. The `\*MERGEFORMAT` switch preserves the existing formatting of the field when the document is refreshed with new data.
</details>

---

### Question 78 (Hard)
**201.4.4** | Difficulty: Hard

**Scenario:** A developer is creating a Word-based Extensible Document that includes images whose content should change based on document variable arguments. How should images be configured to reference document variable arguments in a Word Extensible Document?

A) Use the Image Source property to reference the variable
B) Use the Alt Text property of the image to reference document variable arguments
C) Use a VBA macro to swap image sources
D) Create separate images for each variable value

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In Word-based Extensible Documents, the Alt Text property of an image is used to reference document variable arguments. This allows images to be dynamically associated with data from OneStream through the extensible document framework.
</details>

---

### Question 79 (Hard)
**201.4.4** | Difficulty: Hard

**Scenario:** A developer needs to create a Word Extensible Document that includes both Cube Views and individual data values embedded in paragraph text. For the Cube Views, they want to use content controls. What type of content control is available for Word, and what tool provides the correct syntax for all extensible document elements?

A) Data Binding Content Controls; use the Formula Builder for syntax
B) Rich Text Content Controls for Cube Views, dashboard reports, Word/RTF/TXT documents; use the Object Lookup dialog for all extensible document syntax
C) Plain Text Content Controls; use the Member Filter Builder for syntax
D) ActiveX Controls; use the Code Editor for syntax

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Rich Text Content Controls are available exclusively in Word-based extensible documents. They can contain Cube Views, dashboard reports, Word documents, Rich Text documents, and text documents. The Object Lookup dialog provides the correct syntax for all extensible document elements, serving as a central reference for building extensible document expressions.
</details>

---

### Question 80 (Hard)
**201.4.4** | Difficulty: Hard

**Scenario:** A developer is tasked with creating an Excel-based Extensible Document that needs to display a Cube View in one section and individual cell values in another, with a chart that automatically updates when data refreshes. Which functions should they use for the individual cells and the chart data source?

A) Use XFCell for individual cells and standard Excel chart references
B) Use XFGetCell for individual cell retrieval and XFGetCellVolatile for cells used as chart data sources to ensure charts refresh properly
C) Use XFRetrieve for all data and XFChart for chart data
D) Use GetData for cells and RefreshChart for charts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For individual cell data retrieval in Excel-based Extensible Documents, use XFGetCell. For cells that serve as data sources for charts, use XFGetCellVolatile. The volatile version forces Excel to recalculate on refresh, which triggers chart updates. Standard XFGetCell values may not trigger chart refreshes because Excel optimization may skip recalculating non-volatile cells.
</details>

---

### Question 81 (Medium)
**201.4.4** | Difficulty: Medium

How do you refresh an Extensible Document in the Text Editor within OneStream?

A) Press F5
B) Click the Refresh Document button in the OneStream ribbon within the Text Editor
C) Right-click and select Refresh
D) Close and reopen the document

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To refresh an Extensible Document, click the Refresh Document button available in the OneStream ribbon within the Text Editor. This updates all dynamic content (Cube Views, XFCell values, document variables, etc.) with the latest data from OneStream.
</details>

---

### Question 82 (Medium)
**201.4.2** | Difficulty: Medium

According to Foundation Handbook best practices, what is the "5-second rule" for Cube View performance?

A) Users should wait at least 5 seconds before interacting with a Cube View
B) If a Cube View takes more than 5 seconds to load, it should be optimized for better performance
C) Cube Views should auto-refresh every 5 seconds
D) Data entry changes are saved after a 5-second delay

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "5-second rule" is a performance guideline from the Foundation Handbook: if a Cube View takes more than 5 seconds to load, it should be optimized. Optimization strategies include minimizing Data Unit dimensions in rows, using Sparse Row Suppression, and ensuring efficient member filter queries.
</details>

---

### Question 83 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A solution architect is planning dashboards for a new OneStream implementation. They need to categorize their dashboard requirements. According to the Foundation Handbook, what are the three types of dashboard data consumption, and how do they differ?

A) Input, Output, and Analysis
B) Static Analysis (fixed views for review), Interactive Analytics (user-driven exploration with filters and drill-downs), and Functional Interaction (data input, workflow actions, task execution)
C) Real-Time, Batch, and On-Demand
D) Summary, Detail, and Transactional

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Foundation Handbook identifies three dashboard data consumption types: Static Analysis (fixed views presenting data for review without user interaction), Interactive Analytics (user-driven exploration with filters, drill-downs, and parameter changes), and Functional Interaction (dashboards that enable data input, workflow actions, and task execution). Understanding these categories helps architects design appropriate dashboard solutions.
</details>

---

### Question 84 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer is setting up dynamic components using the code-based Dynamic Dashboard approach. When dynamic components are generated at runtime, they receive a name suffix. What is the naming convention for dynamic components, and what key classes are used in the Dynamic Dashboard Service?

A) Components get a "_generated" suffix; uses DashboardBuilder and ComponentFactory classes
B) Components get a suffix of `_dynamic_"Template Name Suffix"`; key classes include WsDynamicComponent, WsDynamicCollection, WsDynamicDashboardEx, WsDynamicRepeatArgs, WsDynamicParameterCollection, and WsDynamicComponentRepeatArgs
C) Components get a "_runtime" suffix; uses DynamicPanel and DynamicControl classes
D) Components get no suffix; uses StandardComponent and StandardAdapter classes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dynamic components generated at runtime receive a suffix of `_dynamic_"Template Name Suffix"` appended to the base component name. The key classes in the Dynamic Dashboard Service include: WsDynamicComponent (defines a component), WsDynamicCollection (collection of dynamic objects), WsDynamicDashboardEx (extended dashboard definition), WsDynamicRepeatArgs (repeat arguments), WsDynamicParameterCollection (parameter collection), and WsDynamicComponentRepeatArgs (component repeat arguments). These classes provide the programmatic foundation for building dynamic dashboards.
</details>
