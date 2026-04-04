# Question Bank - Section 3: Data Collection (13% of exam)

## Objective 201.3.1: Demonstrate an understanding of the different Data Source types

### Question 1 (Easy)
**201.3.1** | Difficulty: Easy

Which of the following is NOT a main Data Source type in OneStream?

A) Fixed Files
B) Delimited Files
C) Pivot Table
D) Connector

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The four main Data Source types are: Fixed Files, Delimited Files, Connector, and Data Management Export Sequence. Pivot Table is not a Data Source type in OneStream.
</details>

---

### Question 2 (Easy)
**201.3.1** | Difficulty: Easy

Which Data Source type allows connecting directly to an external database without requiring a physical file?

A) Fixed Files
B) Delimited Files
C) Connector
D) Excel Template

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Connector type establishes a direct connection to an external database using ODBC/OLEDB, importing data without requiring an intermediate physical file.
</details>

---

### Question 3 (Easy)
**201.3.1** | Difficulty: Easy

What are the three main groups of the Origin dimension?

A) Input, Output, Calculation
B) Import, Forms, Journal Entries
C) Source, Target, Destination
D) Stage, Engine, Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Origin dimension segments data into three isolated groups: Import (data from flat files, queries, or templates), Forms (manually entered data), and Journal Entries (journal entries). These groups are isolated from each other to protect data integrity.
</details>

---

### Question 4 (Easy)
**201.3.1** | Difficulty: Easy

In what format should amounts be in source files imported into OneStream?

A) In local currency format
B) In natural sign of debit and credit
C) Always in positive values
D) In percentage format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Source files should be in the natural sign of debit and credit. This is a fundamental requirement for OneStream to correctly process imported data.
</details>

---

### Question 5 (Medium)
**201.3.1** | Difficulty: Medium

What is the difference between Tabular and Matrix data structures in a Data Source?

A) Tabular supports multiple cubes; Matrix supports only one
B) Tabular has one line per intersection with one amount; Matrix has multiple amounts per line
C) Matrix is faster than Tabular
D) Tabular only works with Excel files; Matrix with text files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In a Tabular structure, each line represents an intersection with a single amount. In a Matrix structure, each line can contain multiple amounts using rows and columns. Both structures can be used with different file types.
</details>

---

### Question 6 (Medium)
**201.3.1** | Difficulty: Medium

An administrator needs to configure a Connector Data Source to import data from an external ERP system. Which of the following is NOT an integration prerequisite?

A) 64-bit Client Data Provider installed on each Application Server
B) Connection String configured in the Server Configuration Utility
C) Read and write access credentials
D) Inventory of source systems with database type and location

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Credentials must be **read-only**, not read and write. OneStream is a pull system and does not need write permissions on the source system. The other options are valid prerequisites.
</details>

---

### Question 7 (Medium)
**201.3.1** | Difficulty: Medium

Which Connector Business Rule Information Request Type is executed when the user clicks "Load and Transform"?

A) GetFieldList
B) GetData
C) GetDrillBackTypes
D) GetDrillBack

<details>
<summary>Show answer</summary>

**Correct answer: B)**

GetData is executed when clicking "Load and Transform" and is responsible for executing data queries against the source system. GetFieldList returns the list of fields, while GetDrillBackTypes and GetDrillBack relate to the Drill Back functionality.
</details>

---

### Question 8 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** An organization needs to copy data from a cube with different dimensionality to another cube within the same OneStream application, applying transformation rules during the process. What Data Source type should they use?

A) Connector with an internal SQL query
B) Data Management Export Sequence
C) Fixed Files exported from the first cube
D) Excel Template with XFD Named Ranges

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Management Export Sequence allows moving data between OneStream cubes or scenarios with different dimensionality, applying Transformation Rules during the process. It is configured by creating a Data Source with Type = "Data Mgmt Export Sequences" and setting the Data Export Sequence Name. The other options would be unnecessarily complex or not applicable.
</details>

---

## Objective 201.3.2: Given a design specification, apply the Data Source configuration

### Question 9 (Easy)
**201.3.2** | Difficulty: Easy

Which Data Source property must be set to `True` to allow loading Excel templates?

A) Enable Excel Integration
B) Allow Dynamic Excel Loads
C) Excel Template Mode
D) Enable XFD Processing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The `Allow Dynamic Excel Loads` property must be set to `True` in the Data Source to allow loading Excel templates with Named Ranges (XFD, XFF, XFJ, XFC).
</details>

---

### Question 10 (Easy)
**201.3.2** | Difficulty: Easy

Which Named Range is used to import data to Stage in an Excel template?

A) XFF
B) XFJ
C) XFD
D) XFC

<details>
<summary>Show answer</summary>

**Correct answer: C)**

XFD is the Named Range for loading data to Stage (Import). XFF is for Forms, XFJ for Journals, and XFC for Cell Details.
</details>

---

### Question 11 (Easy)
**201.3.2** | Difficulty: Easy

Which dimension token is used to represent the Amount field in an XFD template?

A) A#
B) AM#
C) AMT#
D) $#

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The token `AMT#` represents the Amount field. You can also use `AMT.ZS#` to automatically apply zero suppression. The token `A#` corresponds to the Account dimension, not the Amount.
</details>

---

### Question 12 (Medium)
**201.3.2** | Difficulty: Medium

In an XFD template, what syntax is used to assign a static value to an entire column?

A) F#=[None]
B) F#:(None)
C) F#:[None]
D) F#:Static(None)

<details>
<summary>Show answer</summary>

**Correct answer: C)**

To set a Static Value, use the `:[value]` syntax after the token. For example, `F#:[None]` fixes the "None" member of the Flow dimension for the entire column.
</details>

---

### Question 13 (Medium)
**201.3.2** | Difficulty: Medium

Which tokens are used to obtain the Time and Scenario from the current Workflow POV in an Excel template?

A) T.W# and S.W#
B) T.C# and S.C#
C) T.G# and S.G#
D) T.WF# and S.WF#

<details>
<summary>Show answer</summary>

**Correct answer: B)**

`T.C#` and `S.C#` return the Time and Scenario from the current Workflow (Current). The tokens `T.G#` and `S.G#` return the values from the Global POV, which is different.
</details>

---

### Question 14 (Medium)
**201.3.2** | Difficulty: Medium

**Scenario:** A developer needs to configure a column in an XFD template where each row can have a different User Defined Dimension 3 member. However, some rows do not apply for this dimension. What value should be placed in the rows where UD3 does not apply?

A) Blank (leave empty)
B) N/A
C) None
D) Null

<details>
<summary>Show answer</summary>

**Correct answer: C)**

For User Defined Dimensions (UD1# through UD8#), every row must have a value. When the dimension does not apply for a specific row, you must use the value `None`. Leaving the cell empty would cause an import error.
</details>

---

### Question 15 (Medium)
**201.3.2** | Difficulty: Medium

What is the recommended best practice regarding Source ID in XFD Named Ranges?

A) Use the same Source ID for the entire Excel workbook
B) One Source ID per Named Range
C) One Source ID per each tab in the workbook
D) Source ID is not needed in Excel templates

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to use one Source ID per Named Range. The Source ID (token `SI#`) is the key for data in Stage and allows controlling the granularity of data loads and replacements.
</details>

---

### Question 16 (Hard)
**201.3.2** | Difficulty: Hard

What Bypass Settings options are available in Source Dimension Properties to skip lines during import?

A) Skip Header and Skip Footer
B) Contains at Position and Contains Within Line
C) Exclude by Value and Exclude by Range
D) Filter by Column and Filter by Row

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Bypass Settings allows skipping entire lines based on a value. The two options are: `Contains at Position` (when the value is found at a specific position) and `Contains Within Line` (when the value is found anywhere on the line).
</details>

---

### Question 17 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A team needs to configure a Data Source to import a delimited file with large data of more than 1 million records. What are the recommended Cache Page Size and Cache Pages In Memory Limit values to optimize performance?

A) Cache Page Size = 100, Cache Pages In Memory Limit = 500
B) Cache Page Size = 500, Cache Pages In Memory Limit = 2000
C) Cache Page Size = 1000, Cache Pages In Memory Limit = 5000
D) Cache Page Size = 250, Cache Pages In Memory Limit = 1000

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For large datasets of more than 1 million records, the specific recommendation is to set Cache Page Size = 500 and Cache Pages In Memory Limit = 2000. These values are documented as the optimal configuration for handling large data volumes.
</details>

---

### Question 18 (Hard)
**201.3.2** | Difficulty: Hard

In Source Dimension Properties, what function does Substitution Settings serve and how are multiple values separated?

A) Filters null values; separated by comma
B) Replaces source values (Old Value) with new values (New Value); multiple values are separated with ^
C) Concatenates values from multiple columns; separated by |
D) Converts data types; separated by ;

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Substitution Settings allows replacing source values (Old Value) with new values (New Value) during import. When multiple substitutions are needed, values are separated by the `^` character.
</details>

---

## Objective 201.3.3: Demonstrate an understanding of the Import Process Log

### Question 19 (Easy)
**201.3.3** | Difficulty: Easy

What are the three main tasks of the Import process in Workflow?

A) Extract, Transform, Load
B) Import (Load and Transform), Validate, Load
C) Parse, Map, Calculate
D) Connect, Download, Process

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three main tasks are: Import (Load and Transform) which imports data to Stage, Validate which verifies mappings and intersections, and Load which loads data from Stage to the Analytic Engine (Consolidation Engine).
</details>

---

### Question 20 (Easy)
**201.3.3** | Difficulty: Easy

What icon color in the Workflow indicates that a task completed successfully?

A) Blue
B) Yellow
C) Green
D) White

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Green indicates a successfully completed task. Blue indicates a pending task and Red indicates an error that must be corrected before continuing.
</details>

---

### Question 21 (Easy)
**201.3.3** | Difficulty: Easy

Which Load Method deletes all data from the previous Source ID and replaces it with data from the new file?

A) Append
B) Replace
C) Merge
D) Overwrite

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Replace deletes all data from the previous Source ID and replaces it with data from the new file. Append only adds new rows without modifying existing data.
</details>

---

### Question 22 (Medium)
**201.3.3** | Difficulty: Medium

**Scenario:** A user imported a file and during validation, Transformation Errors appear showing "Unassigned" in the Target Value column. What is the cause and how is it resolved?

A) The file has incorrect format; it should be reimported with a different format
B) There is no Transformation Rule to map the Source Value to a target member; a rule must be created and Retransform must be executed
C) The cube does not have the dimension configured; the dimension must be added
D) The server does not have enough memory; the service must be restarted

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Transformation Errors with "Unassigned" in the Target Value indicate that no transformation rule (map) exists for that source value. The solution is to edit or create the correct transformation rule, save, and click Retransform, which automatically re-validates the data.
</details>

---

### Question 23 (Medium)
**201.3.3** | Difficulty: Medium

What is the difference between a Transformation Error and an Intersection Error during the Validate step?

A) There is no difference, they are synonyms
B) Transformation Error indicates a missing mapping for a dimension; Intersection Error indicates that the complete combination of dimensions is not valid in the cube
C) Transformation Error is a warning; Intersection Error is a critical error
D) Transformation Error occurs during Import; Intersection Error occurs during Load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Transformation Error means a source value has no mapping rule (the dimension shows "Unassigned"). An Intersection Error means that, even though each dimension is mapped, the complete combination of dimensions is not valid. Example: a Customer mapped to a Salary Grade Account.
</details>

---

### Question 24 (Medium)
**201.3.3** | Difficulty: Medium

Which right-click context menu option in Import allows navigating back to the source system?

A) View Source Document
B) View Processing Log
C) Drill Back
D) Export

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Drill Back allows navigating to the source system, but it is only available when using a Connector Data Source. View Source Document opens the imported source file, and View Processing Log shows information about when and how the file was imported.
</details>

---

### Question 25 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** A team uses a Workflow with multiple Source IDs to load data from different departments separately. The administrator needs to reload all data for a period. Why should they NOT use the Load Method "Replace Background (All Time, All Source IDs)"?

A) Because this method does not support multiple periods
B) Because this method always deletes ALL Source IDs, preventing partial replacement by department
C) Because this method requires a Connector Data Source
D) Because this method only works with Excel files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Replace Background (All Time, All Source IDs) always deletes ALL Source IDs, so it cannot be used when the Workflow uses multiple Source IDs for partial replacement. If the team loads departments separately with different Source IDs, this method would delete data from all departments and only load data from the current file.
</details>

---

### Question 26 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** An auditor needs to verify who executed a data import, when it was performed, and how long it took. Which Workflow functionality should they use?

A) View Processing Log from the Import menu
B) Audit Workflow Process from the Workflow channel's context menu
C) Data Unit Statistics
D) View Transformation Rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Audit Workflow Process (accessible by right-clicking on any Workflow channel) provides a complete audit of each task, including date and time, the user who executed the process, duration, errors encountered, and lock history. Although View Processing Log also shows information, Audit Workflow Process offers the most comprehensive audit.
</details>

---

### Question 27 (Hard)
**201.3.3** | Difficulty: Hard

What is the correct file name format for Batch Processing in the Harvest directory?

A) WFProfileName_ScenarioName_TimeName_LoadMethod.csv
B) FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt
C) ProfileName.ScenarioName.TimeName.LoadMethod.dat
D) Import_FileID_WFProfile_Scenario_Time.txt

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct format for Batch Processing is `FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt`. Files must be placed in the Harvest directory with this specific format using hyphens as separators.
</details>

---

## Objective 201.3.4: Demonstrate an understanding of Transformation Rules

### Question 28 (Easy)
**201.3.4** | Difficulty: Easy

What is the correct execution order of Transformation Rules during the Validate step?

A) One-to-One > Composite > Range > List > Mask > Source Derivative > Target Derivative
B) Source Derivative > One-to-One > Composite > Range > List > Mask > Target Derivative
C) Mask > List > Range > Composite > One-to-One > Source Derivative > Target Derivative
D) One-to-One > Range > List > Mask > Composite > Derivatives

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct order is: Source Derivative > One-to-One > Composite > Range > List > Mask > Target Derivative. This order is important because it determines which rule is applied first when multiple rules could match the same value.
</details>

---

### Question 29 (Easy)
**201.3.4** | Difficulty: Easy

Which Transformation Rule type is the only one valid for the Scenario, Time, and View dimensions?

A) Composite
B) Mask
C) One-to-One
D) Range

<details>
<summary>Show answer</summary>

**Correct answer: C)**

One-to-One is the only Transformation Rule type valid for the Scenario, Time, and View dimensions. These critical dimensions require explicit mapping from one source member to one target member.
</details>

---

### Question 30 (Easy)
**201.3.4** | Difficulty: Easy

In a List rule, what character is used as the separator between members?

A) Comma (,)
B) Semicolon (;)
C) Pipe (|)
D) Hyphen (-)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

List rules use the semicolon (;) as the separator. For example: `41137;41139;41145` maps all these accounts to the same target.
</details>

---

### Question 31 (Medium)
**201.3.4** | Difficulty: Medium

What is the key difference between how Composite and Range rules process matches?

A) Composite is faster than Range
B) Composite stops when it finds the first match; Range does not stop and applies the last range processed
C) Range stops when it finds a match; Composite applies all matches
D) There is no difference in processing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is a critical difference: Composite rules stop when they find the first match, while Range rules do NOT stop when they find a match and apply the last range processed. This is important when designing rules to avoid unexpected results.
</details>

---

### Question 32 (Medium)
**201.3.4** | Difficulty: Medium

What separator is used in a Range rule to define the lower and upper limits?

A) Hyphen (-)
B) Tilde (~)
C) Colon (:)
D) Pipe (|)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Range rules use the tilde (~) as the separator between the lower and upper limits. For example: `11202~11209` defines a range of values. Additionally, ranges use character sets, not integers.
</details>

---

### Question 33 (Medium)
**201.3.4** | Difficulty: Medium

**Scenario:** A developer needs to map all accounts that begin with "27" to a specific target, regardless of how many characters follow. What rule type and syntax is most efficient?

A) Mask with `27??`
B) Mask with `27*`
C) Range with `2700~2799`
D) Composite with `A#[27*]`

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Mask with `27*` is the most efficient option. The `*` wildcard captures any number of characters and has low cost (simple update pass through). Using `?` would be more costly because it requires table scans. The range would not capture accounts like "27999" if the upper limit is not adjusted correctly.
</details>

---

### Question 34 (Medium)
**201.3.4** | Difficulty: Medium

What types of Derivative Rules exist based on how they are stored in Stage?

A) Temporal and Permanent
B) Interim, Interim (Exclude Calc), Final, Final (Exclude Calc), and Check Rule
C) Draft and Published
D) Source and Target only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The types are: Interim (not stored in Stage, can be used in subsequently run derivative rules), Interim (Exclude Calc), Final (stored in Stage and available to be mapped to a target member), Final (Exclude Calc), and Check Rule (a custom validation rule applied during the Validate task).
</details>

---

### Question 35 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** An implementation team is experiencing performance issues with their Transformation Rules. They currently use many Mask rules with `?` wildcards on both sides (source and target), and several Conditional rules. What strategies should they apply to optimize performance?

A) Add more memory to the server and increase the timeout
B) Replace `?` rules with One-to-One, Range, or List when possible; split large Conditional lists into smaller lists; limit criteria in Mask Conditional
C) Run the rules during night hours to avoid impact
D) Convert all rules to Derivatives for more flexibility

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Performance best practices include: using One-to-One, Range, and List whenever possible (minimal cost), avoiding `?` on both sides which are extremely costly, splitting large Conditional lists into smaller lists for optimal memory utilization, and keeping criteria restrictive in Mask Conditional. Derivatives are the most costly rules, so converting everything to Derivatives would worsen performance.
</details>

---

### Question 36 (Hard)
**201.3.4** | Difficulty: Hard

In a Mask rule, which of the following statements is correct?

A) The `?` wildcard can be used in both Source and Target Value
B) The `*` wildcard as a prefix in Target Value is supported
C) The `?` wildcard is NOT supported in Target Value, and `*` as a prefix in Target Value is NOT supported
D) Both wildcards are interchangeable in Source and Target

<details>
<summary>Show answer</summary>

**Correct answer: C)**

In Mask rules, the `?` wildcard is NOT supported in Target Value, and `*` as a prefix in Target Value is also NOT supported. However, `*` as a suffix in Target Value yields the Source Value. These restrictions are important when designing Mask rules.
</details>

---

### Question 37 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A consultant needs to create a rule that conditionally maps accounts based on the combination of the source account and entity. For example, accounts "199X-XXX" (where X is any character) must be mapped differently if they belong to entity "Texas". What rule type and syntax should be used?

A) One-to-One with multiple entries
B) Composite with syntax `A#[199?-???*]:E#[Texas]`
C) Range with entity condition
D) List with entity filter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Composite rule allows conditional mapping using dimensional tags with the syntax `D#[value]`. The syntax `A#[199?-???*]:E#[Texas]` uses dimensional tags for Account and Entity, where `?` represents one character and `*` represents any number of characters. The Composite rule stops when it finds the first match.
</details>

---

### Question 38 (Medium)
**201.3.4** | Difficulty: Medium

How are Transformation Rules organized to be assigned to a Workflow?

A) They are assigned directly to the Data Source
B) They are grouped into Transformation Rule Groups, then into Transformation Rule Profiles, which are assigned to Workflow Profiles
C) They are created directly within the Workflow
D) They are stored in external XML files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Transformation Rules are organized into Groups, which are grouped into Transformation Rule Profiles, which are assigned to Workflow Profiles (Application > Workflow Profiles > Import > Integration Settings). The "Create and Populate Rule Profile" button can automatically create a Group and Profile with the same name.
</details>

---

### Question 39 (Easy)
**201.3.4** | Difficulty: Easy

Which of the following Transformation Rule types have the lowest processing cost (green)?

A) Derivative and Composite
B) One-to-One, Range, and List
C) Mask with ? and Conditional
D) Source Derivative and Target Derivative

<details>
<summary>Show answer</summary>

**Correct answer: B)**

One-to-One, Range, and List have the lowest cost (green) because they perform a simple update pass through to the database. Mask with `*` on one side also has low cost. Derivatives and Conditional rules are the most costly.
</details>

---

### Question 40 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A developer needs to validate that the imported trial balance is balanced. The validation must sum certain source accounts to verify the balance. What rule type and Derivative Type should be used, and why?

A) Target Derivative with Final type, because it operates on already transformed data
B) Source Derivative with Check Rule type, because it operates on inbound source data and can implement custom validations during Validate
C) One-to-One with Business Rule logic
D) Composite with balance condition

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Source Derivative is appropriate because it operates on inbound source data and can create new records in Stage to sum accounts and verify the balance. The "Check Rule" Derivative Type is specifically a custom validation rule that is applied during the Validate task. Source Derivatives execute first in the rule order, which is necessary for this type of validation.
</details>

---

## NEW QUESTIONS (41-78)

## Objective 201.3.1: Demonstrate an understanding of the different Data Source types

### Question 41 (Easy)
**201.3.1** | Difficulty: Easy

OneStream is described as what type of data system in relation to source systems?

A) A push system that receives data sent from source systems
B) A pull system that extracts data from databases and source systems
C) A hybrid push-pull system
D) A batch-only processing system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream is a pull system — it pulls data from databases and source systems. Data is not pushed to OneStream data tables. The ability to load from a direct connection means you can refresh data by simply pushing a button, and the consumption is instantaneous.
</details>

---

### Question 42 (Easy)
**201.3.1** | Difficulty: Easy

In OneStream, journals are written to which member of the Origin dimension?

A) Import
B) Forms
C) AdjInput
D) JournalInput

<details>
<summary>Show answer</summary>

**Correct answer: C)**

In the OneStream app, journals are written to the AdjInput member of the Origin dimension. This is separate from the Import and Forms members, providing data isolation and protection.
</details>

---

### Question 43 (Medium)
**201.3.1** | Difficulty: Medium

Which of the following correctly describes the purpose of the Origin dimension in OneStream?

A) It tracks the chronological order of data imports
B) It combines data protection, data layering, and isolation by segmenting data into separate buckets for each entity
C) It identifies which user loaded the data
D) It classifies data by geographical origin

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Origin dimension combines the concepts of data protection and data layering and isolation. It acts as a barrier to protect how data is loaded in different forms — Import, Forms, and Journal Entries are kept in isolated groups so that, for example, importing data does not overwrite data entered through forms.
</details>

---

### Question 44 (Medium)
**201.3.1** | Difficulty: Medium

What does the Connector Business Rule Information Request Type `GetFieldList` return?

A) A list of all records in the external database
B) A list of available field names in the external Data Source as a list of VB.Net Strings
C) The connection string used for the database
D) A list of drill-back options

<details>
<summary>Show answer</summary>

**Correct answer: B)**

GetFieldList is called by the Data Source designer screen when the user selects a Connector Data Source or one of its defined Dimensions. It returns a list of available fields in the external Data Source as a list of VB.Net Strings [List(Of String)].
</details>

---

### Question 45 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** A company uses Oracle as their ERP database and needs to set up a Connector Data Source. The consultant is preparing the integration prerequisites. Which of the following statements about the connection setup is correct?

A) Oracle connections only require the connection string; no additional software is needed on the application servers
B) Oracle Database integrations require that all Oracle Source System TNS Profile details need to be in place on each of the OneStream application servers
C) Oracle connections can use 32-bit client data providers since OneStream supports both architectures
D) The connection string for Oracle is identical to the SQL Server connection string

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Oracle Database integrations, all Oracle Source System TNS Profile details need to be in place on each of the OneStream application servers. Additionally, a 64-bit source system client data provider must be installed because OneStream is a Microsoft .NET application with a 64-bit architecture.
</details>

---

### Question 46 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** An architect is evaluating the best approach for creating a prototype of external data queries before building the Connector Business Rule. What is the recommended best practice?

A) Write the Connector Business Rule directly and test it with live data
B) Create a set of Dashboard Data Adapters in a new Dashboard Maintenance Unit named with the EXS prefix to prototype queries
C) Use the OneStream REST API to test queries externally
D) Build a temporary Data Source with sample files first

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to create a set of Dashboard Data Adapters to be used as a test bed. You should create a new Dashboard Maintenance Unit named "EXS The Connector Name" where the EXS prefix stands for External System, providing administrators with an immediate understanding of the Maintenance Unit's contents.
</details>

---

### Question 47 (Medium)
**201.3.1** | Difficulty: Medium

Which of the following drill-back visualization types opens a website in an external browser based on variables from the source data row?

A) DataGrid
B) TextMessage
C) WebUrl
D) WebUrlPopOutDefaultBrowser

<details>
<summary>Show answer</summary>

**Correct answer: D)**

WebUrlPopOutDefaultBrowser opens a website or custom HTML web content in an external browser. From the Stage Import data grid, you right-click on a data record, select Drill Back, and choose WebUrlPopOutDefaultBrowser to launch a standard browser session based on variables from the data row.
</details>

---

### Question 48 (Easy)
**201.3.1** | Difficulty: Easy

What is the primary purpose of the Data Management Export Sequence Data Source type?

A) To export data from OneStream to external CSV files
B) To move and export data within OneStream, such as copying data between cubes or scenarios
C) To create backup copies of OneStream databases
D) To import data from external REST APIs

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Management Export Sequences are used in Import workflows to move and export data. For example, they can copy data from one OneStream cube or scenario to another (even when cubes or scenarios contain different dimensionality), or export OneStream data to an external source system while applying transformation rules through the workflow.
</details>

---

## Objective 201.3.2: Given a design specification, apply the Data Source configuration

### Question 49 (Medium)
**201.3.2** | Difficulty: Medium

In a Fixed File Data Source, what do the Start Position and Length properties define for a Source dimension?

A) The starting row and number of rows to read
B) The numerical starting point for a line item and how many characters will be taken from that position
C) The column header position and total column width
D) The starting byte offset and total file length

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Start Position is the numerical representation of the starting point for a line item, and Length defines how many characters will be taken from the start position. For example, a Fixed data source with a start position of 20 and a length of five will start with the 20th character and include the next five characters.
</details>

---

### Question 50 (Medium)
**201.3.2** | Difficulty: Medium

In the Source Dimension Properties, what is the difference between the "Text" and "Stored Text" Data Types?

A) Text reads from a file; Stored Text reads from a database
B) Text reads the value from the file as defined in position settings; Stored Text overrides position settings and forces a constant value for every line
C) Stored Text saves the value permanently; Text is temporary
D) There is no functional difference between them

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "Text" Data Type reads the value from the file as defined in the position settings. The "Stored Text" Data Type overrides the position settings and forces the value to be a constant value for every line. This is useful when a dimension should have the same value for all imported records.
</details>

---

### Question 51 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A developer is configuring an Excel template to load data for multiple time periods in a single import using a Matrix-style layout. Which token syntax should be used for the Amount columns with zero suppression?

A) AMT#:[2023M1], AMT#:[2023M2], etc. with separate Named Ranges per period
B) AMT.ZS# combined with T# tokens in the column headers for each period
C) AMT# with a separate T# column for each row
D) AMOUNT# with period suffixes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For a Matrix-style Excel import template, you use AMT.ZS# (Amount with zero suppression) on the AMT columns, combined with T# tokens in the column headers specifying each time period. The .ZS extension applies zero suppression to the import, and the Matrix format allows multiple amounts per line across different time periods.
</details>

---

### Question 52 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A Data Source is configured for a Delimited File, and the imported file contains amounts with commas as thousand separators, periods as decimal indicators, and some values have a leading minus sign for negatives. Which Numeric Settings must be configured?

A) Only the Decimal Indicator
B) Thousand Indicator = ",", Decimal Indicator = ".", and Negative Sign Indicator = "-"
C) Currency Indicator = "$" and Factor Value = 1
D) Only the Negative Sign Indicator

<details>
<summary>Show answer</summary>

**Correct answer: B)**

All three Numeric Settings must be configured: the Thousand Indicator set to "," to handle the comma separator in values like 1,000; the Decimal Indicator set to "." for decimal separation; and the Negative Sign Indicator set to "-" to correctly identify negative values. These settings ensure OneStream properly parses the numeric amounts.
</details>

---

### Question 53 (Medium)
**201.3.2** | Difficulty: Medium

Which Source Dimension Data Type should be used when the Time value must come from the current Workflow POV rather than from the data file?

A) Text
B) Stored DataKey Text
C) Current DataKey Time
D) Global DataKey Time

<details>
<summary>Show answer</summary>

**Correct answer: C)**

"Current DataKey Time" uses the Time value from the current Workflow POV. This is different from "Global DataKey Time" which uses the Time value from the Global POV, and "Text" or "Stored DataKey Text" which read or store fixed values from the file.
</details>

---

### Question 54 (Medium)
**201.3.2** | Difficulty: Medium

What is the purpose of the Leading Fill Value and Trailing Fill Value in Text Fill Settings of a Source Dimension?

A) They add padding characters to the values read from the file upon import
B) They remove characters from the beginning and end of values
C) They define the alignment of text in Stage tables
D) They replace special characters in the imported values

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Leading Fill Value adds characters before whatever value is brought in from the file (e.g., Lead Fill Mask = xxx, data value = 00, results value = x00). Trailing Fill Value adds characters after the value (e.g., Trail Fill Mask = xxx, Data Value = 00, Results Value = 00x). These are useful for padding or formatting source values during import.
</details>

---

### Question 55 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A team is configuring a Data Management Export Sequence to copy actuals data from a financial close cube (FinRptg) to a planning cube (MgmtRptg) with different dimensionality. The FinRptg legal entity data needs to map to MgmtRptg's UD1 dimension, and FinRptg's UD1 cost center data needs to map to MgmtRptg's Entity dimension. Which setting must be TRUE on the Import Workflow Profile to allow this data to load successfully?

A) Allow Cross-Cube Mapping
B) Can Load Unrelated Entities
C) Enable Multi-Cube Integration
D) Allow Dynamic Dimension Mapping

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When configuring an Import Workflow Profile for a Data Management Export Sequence that loads data to multiple entities, the "Can Load Unrelated Entities" setting must be set to True. Additionally, Profile Active must be set to True, and the Workflow Name should include Import, Validate, and Load tasks.
</details>

---

### Question 56 (Easy)
**201.3.2** | Difficulty: Easy

Which Named Range prefix is used to load data into custom SQL Server tables from an Excel template?

A) XFD
B) XFF
C) XFT
D) XFC

<details>
<summary>Show answer</summary>

**Correct answer: C)**

XFT is the Named Range prefix for loading data into custom SQL Server tables. XFD is for Stage data, XFF is for Forms, XFJ is for Journals, and XFC is for Cell Details.
</details>

---

### Question 57 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A consultant needs to configure a Connector Data Source where the external query results should be processed like a Delimited File, using all the same built-in processing capabilities including Complex Expressions and Business Rules on dimensions. What is the correct approach?

A) Set Connector Uses Files to True and manually parse the data
B) Map fields from the external data query results to Dimensions, which creates processing behavior similar to a Delimited File
C) Export the query results to a CSV file first, then configure a Delimited File Data Source
D) Use a Parser Business Rule to convert the Connector data to a delimited format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Fields from the external data query results are mapped to Dimensions, creating a processing behavior similar to a Delimited File. This enables a Connector Data Source to use all the same built-in processing capabilities available with file-based Data Sources, including Complex Expressions and Business Rules on dimensions. This design methodology helps write the Connector Business Rule in a way that requires very little maintenance.
</details>

---

## Objective 201.3.3: Demonstrate an understanding of the Import Process Log

### Question 58 (Medium)
**201.3.3** | Difficulty: Medium

What is the purpose of the Source ID field in the data import process?

A) It identifies the user who uploaded the file
B) It is a key for data imported into Stage that controls how data is reloaded and segmented
C) It stores the original file name for audit purposes only
D) It assigns a sequential number to each imported record

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Source ID is a field in the data source that allows data to be tagged to determine how to reload data. It is a key for data imported into Stage and enables controlling the granularity of data loads. Reloading data with the same Source ID replaces only that subset, while different Source IDs keep data segments isolated.
</details>

---

### Question 59 (Medium)
**201.3.3** | Difficulty: Medium

When exporting data from OneStream through a Data Management Export Sequence, which of the following is NOT a valid method to retrieve the transformed data from Stage tables?

A) Workflow Event Handler business rule
B) Transformation Event Handler business rule
C) Directly querying the Stage SQL tables from an external application
D) Data Adapter with OneStream REST API

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Valid methods to retrieve transformed data from Stage include: Workflow Event Handler (runs code when workflow completes a step), Transformation Event Handler (runs code at various points from Import through Load), Extender Rule (ad hoc or scheduled data retrieval), and Data Adapter with OneStream REST API. Directly querying Stage SQL tables from an external application is not a recommended or supported method.
</details>

---

### Question 60 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** A company needs to automate their monthly data import process. They want files to be automatically loaded into OneStream when placed in a specific directory. What is the correct file name example for a batch file that replaces data for the Detroit Import workflow profile, Actual scenario, January 2011?

A) Detroit_Import_Actual_2011M1_Replace.txt
B) 1TB-Detroit;Import-Actual-2011M1-R.txt
C) Detroit-Import-Actual-2011M1-Replace.csv
D) TB_Detroit_Import_Actual_2011M1_R.txt

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct format follows the pattern `FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt`. The example `1TB-Detroit;Import-Actual-2011M1-R.txt` shows: FileID = 1TB, WFProfileName = Detroit;Import, ScenarioName = Actual, TimeName = 2011M1, LoadMethod = R (Replace). Files must be placed in the Harvest directory.
</details>

---

### Question 61 (Easy)
**201.3.1** | Difficulty: Easy

What does the Confirm task in the Workflow process do?

A) Confirms that the data file was received
B) Runs the Confirmation Rules and informs users if they passed or failed data quality rules
C) Sends a confirmation email to the administrator
D) Confirms the connection to the source system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Confirm task runs the Confirmation Rules defined for the particular Workflow. It immediately informs users if they have passed or failed the data quality rules. The two types of statuses are Warning (outside threshold but does not stop the process) and Error (outside threshold and stops the process, turning the step to red).
</details>

---

### Question 62 (Medium)
**201.3.3** | Difficulty: Medium

During the Import process, what is the difference between loading data to Stage versus a Direct Load (Non-Stage)?

A) There is no functional difference; both provide the same audit trail
B) Stage loads provide a full audit trail with source data, mapping, and target dimensions; Direct Loads bypass Stage tables for faster loading but with less audit capability
C) Direct Loads are only for Excel templates
D) Stage loads are only for Connector Data Sources

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Stage loads process data through the mapping engine with a complete audit trail including source data, mapping used, and target dimensions. Direct Loads (Non-Stage) bypass the overhead of storing source information in Stage tables, loading data quicker into the cube but without full audit capabilities. Direct Loads are recommended for forecasting data, flash data, or what-if purposes rather than regular monthly reporting.
</details>

---

### Question 63 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** An implementation team is loading data and notices that a 10-digit string account number like `2300028932` contains embedded information: `23000` is the account, `28` is the geography, and `932` is the product. They only need to map the account portion. What approach is recommended for optimizing the data source and mapping?

A) Use a Composite rule to parse all three segments simultaneously
B) Take only the first five digits and use simple one-to-one mapping; alternatively, break the dimension up with separators (like `23000_28_932`) for flexibility with wildcard mapping
C) Load the full 10-digit string and create Range rules for each combination
D) Write a Connector Business Rule to split the string before import

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is to take only the first five digits if only the account information is needed, making the mapping easy as one-to-one. If the dimension needs other information, breaking the dimension up and adding separators (such as `23000_28_932`) provides flexibility with wildcard mapping. For example, `23000*_9*` could map every account with `23000` and a product starting with `9` to a specific account.
</details>

---

## Objective 201.3.4: Demonstrate an understanding of Transformation Rules

### Question 64 (Easy)
**201.3.4** | Difficulty: Easy

Which characters are reserved for operations in Transformation Rules and will cause errors if used in Source or Target Members?

A) @, #, $
B) !, ?, |
C) &, %, ^
D) <, >, =

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The characters !, ?, and | are used in operations for Transformation Rules. These characters will cause errors if used with Source Members or Target Members.
</details>

---

### Question 65 (Medium)
**201.3.4** | Difficulty: Medium

In a Range rule, what happens when a source member falls within multiple overlapping ranges?

A) An error is thrown and the mapping fails
B) The member is mapped to the first range that matches
C) The member is mapped to the last range processed
D) The member is flagged as "Unassigned"

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Range rules do not stop processing when a record meets the criteria. If the ranges overlap and a member falls within multiple ranges, that member will be mapped only to the final range that is processed. Therefore, for a member in multiple ranges, you should set the order so that the last rule run is the one you want applied.
</details>

---

### Question 66 (Medium)
**201.3.4** | Difficulty: Medium

When entering a Range rule expression, how should character sets be balanced to ensure the full range is captured?

A) Always use numeric values only
B) Use balanced character sets; for example, a range of 4 to 3000 must be entered as 0004~3000
C) Pad the upper limit with zeros
D) Use the Length property to specify character count

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Range rules use character sets, not integers. To ensure the full range is captured, you must use balanced character sets. For example, a range of 4 to 3000 must be entered as `0004~3000` under Rule Expression. Without balanced characters, the range comparison may not work as expected.
</details>

---

### Question 67 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A developer needs to create a derivative rule that aggregates all accounts starting with "11" into a new Account called "Cash" and stores it in Stage for mapping. Then, accounts starting with "12" should aggregate into "AR" but only as a temporary value for use in subsequent derivative rules. What Derivative Types should be used?

A) Both should use Final type
B) Cash = Final, AR = Interim
C) Cash = Interim, AR = Final
D) Both should use Interim type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For "Cash" (A#[11*]=Cash), the Final derivative type should be used because it needs to be stored in Stage and available to be mapped to a target member. For "AR" (A#[12*]=AR), the Interim derivative type should be used because it will not be stored in Stage but can be used within other subsequently run derivative rules as a temporary aggregation.
</details>

---

### Question 68 (Medium)
**201.3.4** | Difficulty: Medium

What does the "Create and Populate Rule Profile" toolbar button do in Transformation Rules?

A) Creates only a Transformation Rule Group
B) Creates a Transformation Rule Group and a Transformation Rule Profile with the same name, and the Profile is already populated with each Dimension Rule Group
C) Creates a Workflow Profile with transformation rules pre-configured
D) Imports rules from a TRX file and creates a profile automatically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Create and Populate Rule Profile" creates a Transformation Rule Group and a Transformation Rule Profile with the same name. The Rule Profile will already be populated with each Dimension Rule Group and updates as the Group is updated. This is a convenience feature that simplifies the setup process.
</details>

---

### Question 69 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A Composite rule for the Entity dimension is configured with the following three rules to map entities based on account names: Rule 1 (Order 1): `E#[*]:A#[H???]` maps to Rhode Island; Rule 2 (Order 2): `E#[*]:A#[H????]` maps to Maine; Rule 3 (Order 3): `E#[*]:A#[H?????????]` is set to bypass. For a record with Account = "Heads" (5 characters), which entity will it be mapped to?

A) Rhode Island
B) Maine
C) It will be bypassed
D) It will show as Unassigned

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Composite rules stop processing when a record meets the criteria and are processed in order. "Heads" has 5 characters. Rule 1 (`H???`) matches accounts with H + 3 characters (4 total) — "Heads" has 5 characters so it does not match. Rule 2 (`H????`) matches accounts with H + 4 characters (5 total) — "Heads" matches, so processing stops and it maps to Maine. Rule 3 is never evaluated.
</details>

---

### Question 70 (Medium)
**201.3.4** | Difficulty: Medium

What is the purpose of the Logical Operator property in Transformation Rule mapping types like Composite, Range, List, and Mask?

A) It defines the mathematical operations to apply to amounts
B) It extends a normal mapping rule with VB.NET scripting functionality
C) It sets the Boolean condition for rule execution
D) It determines the sort order of rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Logical Operator provides the ability to extend a normal mapping rule with VB.NET scripting functionality. The options are: None (default — no script), Business Rule (when .NET scripting is available in the Business Rule Library), and Complex Expression (when .NET scripting is needed only within that specific dimension).
</details>

---

### Question 71 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A derivative rule creates new rows in Stage by adding a suffix to account names and changing entities. The rule expression is: `A#[1000~1999]<<New_:E#[Tex*]=TX`. For a record with Account = "1010" and Entity = "Texas", what will the new Stage row contain?

A) Account = "1010", Entity = "TX"
B) Account = "New_1010", Entity = "TX"
C) Account = "1010_New", Entity = "Texas"
D) Account = "New_1010", Entity = "Texas"

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The rule `A#[1000~1999]<<New_:E#[Tex*]=TX` creates a new row in Stage for any Account between 1000 and 1999, adding the prefix "New_" (the `<<` syntax adds a prefix). So Account 1010 becomes "New_1010". For entities starting with "Tex", the entity in the new row is set to "TX". The original row remains unchanged.
</details>

---

### Question 72 (Medium)
**201.3.4** | Difficulty: Medium

What is the recommended approach for naming global Transformation Rules that can be reused across multiple cubes?

A) Name them with the cube name as a prefix
B) Name them as "global" since Scenario, Time, and View dimensions can only have one-to-one mapping and can typically be reused across cubes
C) Create unique names for each workflow profile
D) Use numeric identifiers for universal rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The dimension rules for Scenario, Time, and View should be named as "global" because they can be reused over multiple cubes. These three dimensions can only have one-to-one mapping, so keeping them simple and reusable (e.g., one group for each if they don't overlap with different strings) is a best practice.
</details>

---

### Question 73 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** An administrator wants to export Transformation Rules from one application and import them into another. They plan to use TRX files. Which limitation should they be aware of?

A) TRX files cannot export more than 1000 rules
B) TRX files do not support Logical Operators and Complex Expressions; for those, XML load/extract should be used
C) TRX files are only compatible with One-to-One rules
D) TRX files must be converted to CSV before import

<details>
<summary>Show answer</summary>

**Correct answer: B)**

TRX file export creates a comma-delimited file which is valid only for certain Transformation Rule designs. Properties such as Logical Operators and Complex Expressions are not supported. For complete Transformation Rule management including these properties, Application Tools Load/Extract of application XML files should be used.
</details>

---

### Question 74 (Medium)
**201.3.1** | Difficulty: Medium

What are the four major considerations when starting a data integration project in OneStream?

A) Budget, Timeline, Resources, Technology
B) Inventory Files and Sources, Historical Data, How Much Data, Performance of the Data Load
C) Data Format, Connection Type, Security, Validation
D) Source System, Target Cube, Mapping Rules, Workflow Configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four major considerations are: (1) Inventory Files and Sources — understanding all data sources and how data is prepared, (2) Historical Data — determining how much history needs to be reconciled (typically two years), (3) How Much Data — balancing the amount of data in the ledger vs. the cube, and (4) Performance of the Data Load — considering data volume and tuning processing times and mapping.
</details>

---

### Question 75 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A consultant is reviewing mapping performance for a large dataset. The current rules include Map Mask (Conditional), Map List (Conditional), and Derivative rules. All are processing slowly. Which statement about the processing cost of these rules is accurate?

A) Conditional rules are low-cost because they only add a filter condition
B) All three types require returning a record set with all dimension fields back to the application server, consuming significant data transfer and memory
C) Derivative rules are the fastest because they execute in the database
D) List (Conditional) rules are faster than standard List rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Map Mask (Conditional), Map List (Conditional), and Derivative rules all use significant data transfer and memory utilization. They are all required to return a record set with all dimension fields back to the application server to perform conditional mapping or derive calculated rows. Derivatives additionally execute a SQL Statement that pulls ALL dimension fields and insert new records one by one.
</details>

---

### Question 76 (Medium)
**201.3.2** | Difficulty: Medium

When loading journal data via an Excel template, which specific tokens are used for debit and credit amounts?

A) AMT# for both debits and credits
B) AMTDR# for debits and AMTCR# for credits
C) DR# for debits and CR# for credits
D) DEBIT# and CREDIT#

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When loading journal entries via Excel, the specific tokens for amounts are AMTDR# for the debited amount and AMTCR# for the credited amount. This is different from standard data imports which use AMT# for a single amount field.
</details>

---

### Question 77 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** A company wants to spread their data load for better performance. They currently have one data load with 1 million records taking ten minutes. An architect suggests breaking the load into five separate groups. Why would this approach improve performance?

A) Because smaller files consume less disk space
B) Because the parser is sequential, but by breaking data into separate groups, the parser can be used by multiple data load and workflow processes simultaneously
C) Because OneStream has a file size limit of 200,000 records
D) Because transformation rules run faster on smaller datasets regardless of parallel processing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The parser is sequential for a single data load, but by breaking the data into separate groups (commonly split by business units/entities), the parser can be used by multiple data load and workflow processes. The same data that takes ten minutes in one load could be loaded in two minutes by breaking it into five separate groups, because they can be processed in parallel.
</details>

---

### Question 78 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** A team is designing the journal workflow process. They need to understand the Journal Balance Type options to configure journal templates correctly. An entity has intercompany transactions that must balance by each individual entity in a multi-entity journal. Which Journal Balance Type should they select?

A) Balanced
B) Balance by Entity
C) Unbalanced
D) Balanced by Intercompany

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Balance by Entity" requires that debits and credits in a multi-entity journal must balance for each entity. This is different from "Balanced" which requires the entire set of journal lines to balance between debits and credits, and "Unbalanced" which does not perform a balance check and is normally used for one-sided journals.
</details>

---
