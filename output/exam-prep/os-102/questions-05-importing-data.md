# Question Bank - Section 5: Importing Data (13% of exam)

## Objectives
- **102.5.1:** Perform manual data import using Workflow profiles

---

## Objective 102.5.1: Manual data import using Workflow profiles

### Question 1 (Easy)
**102.5.1** | Difficulty: Easy

Which Workflow Profile Type allows data to be imported into the Cube?

A) No Input
B) Review
C) Base Input
D) Parent Input

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Base Input is the Workflow Profile Type that enables data import into the Cube at the base entity level. This profile type supports both manual data entry and automated data imports through configured load processes.
</details>

---

### Question 2 (Easy)
**102.5.1** | Difficulty: Easy

What is the primary tool used to import data into OneStream through the Workflow?

A) Dashboard Data Adapter
B) Data Management connector
C) Load Data step within the Workflow
D) Direct database insert

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Load Data step within the Workflow is the primary mechanism for importing data into OneStream. This step is configured as part of the Workflow Profile and allows administrators and users to trigger data imports from configured source systems or files.
</details>

---

### Question 3 (Medium)
**102.5.1** | Difficulty: Medium

An Administrator needs to import data from a flat file (CSV) into the Cube. Which component must be configured to map the file columns to the Cube dimensions?

A) A Dashboard Data Adapter
B) A Data Import Connector with field mappings
C) The Application Properties import settings
D) A custom business rule only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Data Import Connector (also referred to as a Connector or Data Adapter in the import context) must be configured to define the source file location, file format, and the mappings between file columns and Cube dimensions (Entity, Account, Flow, etc.). This connector is then referenced by the Workflow data load step.
</details>

---

### Question 4 (Medium)
**102.5.1** | Difficulty: Medium

When importing data through a Workflow, what happens if a dimension member in the source file does not exist in the target Cube?

A) The member is automatically created in the dimension
B) The entire import fails with no data loaded
C) The record with the unmatched member is rejected and logged, while valid records may still be loaded (depending on configuration)
D) The data is loaded to a default "Unassigned" member

<details>
<summary>Show answer</summary>

**Correct answer: C)**

When a dimension member in the source data does not match any existing member in the target Cube, that specific record is rejected and the error is logged in the import results. Depending on the import configuration, valid records with matching members may still be loaded successfully. The Administrator should review the error log to identify and correct mapping issues.
</details>

---

### Question 5 (Medium)
**102.5.1** | Difficulty: Medium

Which import mode replaces all existing data for the target intersection before loading new data?

A) Merge
B) Accumulate
C) Replace
D) Append

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Replace mode clears all existing data for the target dimension intersections before loading the new data. This ensures a clean load without leftover data from previous imports. Merge mode combines new data with existing data. Accumulate adds to existing values. These modes are important to understand for ensuring data integrity during imports.
</details>

---

### Question 6 (Hard)
**102.5.1** | Difficulty: Hard

An Administrator imports data using Merge mode and notices that some account balances appear doubled. What is the most likely cause?

A) The Cube has a calculation error
B) The import was run twice without clearing the previous load, and Merge mode accumulated the values
C) The dimension members are duplicated
D) The Workflow Profile is incorrectly configured

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When using Merge mode, data values are combined with existing data at the target intersections. If the import is run twice (intentionally or accidentally) without first clearing the data, values will be accumulated, resulting in doubled balances. The Administrator should use Replace mode if a clean reload is intended, or ensure the import process includes a clear step before reloading.
</details>

---

### Question 7 (Easy)
**102.5.1** | Difficulty: Easy

Where can an Administrator review the results and any errors from a data import process?

A) The Cube Properties panel
B) The Activity Log / Process Log for the import step
C) The Application Properties General tab
D) The Dimension Library

<details>
<summary>Show answer</summary>

**Correct answer: B)**

After a data import, the Administrator can review the results in the Activity Log or Process Log associated with the import step. This log shows the number of records processed, records loaded, records rejected, and detailed error messages for any failures. This is essential for validating data import quality.
</details>

---

### Question 8 (Medium)
**102.5.1** | Difficulty: Medium

An Administrator wants to transform source data values during import (e.g., reversing signs for certain accounts). Where is this transformation configured?

A) In the Cube dimension properties
B) In the Data Import mapping rules or transformation expressions within the Connector
C) In the Application Properties
D) Transformations can only be done after import using business rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data transformations such as sign reversal, value multiplication, or member mapping can be configured within the Data Import Connector's mapping rules and transformation expressions. These are applied during the import process before data is written to the Cube, allowing source data to be adjusted to match the application's requirements.
</details>

---

### Question 9 (Hard)
**102.5.1** | Difficulty: Hard

An Administrator needs to import data from an ERP system where the chart of accounts uses different codes than the OneStream Account dimension. What is the recommended approach to handle this mapping?

A) Rename all OneStream Account members to match the ERP codes
B) Configure member mapping tables (translation tables) in the import process to translate ERP codes to OneStream Account members
C) Create duplicate Account members with both naming conventions
D) Load data to a staging Cube first, then manually reclassify

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is to use member mapping tables (also called translation tables or lookup tables) within the import configuration. These tables define how source system codes map to OneStream dimension members, allowing the import process to automatically translate ERP account codes to the corresponding OneStream Account members. This keeps both systems' naming conventions intact and centralizes the mapping logic.
</details>

---

### Question 10 (Hard)
**102.5.1** | Difficulty: Hard

An Administrator is troubleshooting a data import that completes successfully but shows zero records loaded. The source file contains valid data. Which of the following should the Administrator check FIRST?

A) Whether the Cube has been initialized
B) Whether the dimension member mappings in the Connector match the source data values and the target Workflow Profile allows data loading for the specified entities
C) Whether the application server has sufficient memory
D) Whether the dashboard is configured to display the data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When an import completes with zero records loaded, the most common cause is a mismatch between the source data values and the dimension member mappings configured in the Connector. The Administrator should verify that source account codes, entity codes, and other dimension identifiers correctly map to existing OneStream members. Additionally, the Workflow Profile must be configured as Base Input for the target entities to accept imported data.
</details>

---

### Question 11 (Easy)
**102.5.1** | Difficulty: Easy

What is the minimum Workflow Step status required before data can be imported into a Workflow entity?

A) Not Started
B) Started
C) Confirmed
D) Certified

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Step must be in "Started" status before data can be imported. If the Step is still in "Not Started" status, the import process cannot write data to that entity. Similarly, if the Step is already Confirmed or Certified, data entry and imports are locked until the Step is reopened.
</details>

---

### Question 12 (Medium)
**102.5.1** | Difficulty: Medium

An Administrator configures a data import to load Actual data for January. Which dimension intersections must be correctly specified in the import mapping?

A) Only Entity and Account
B) Entity, Account, Scenario, Time, and Flow at minimum, plus any active UD dimensions on the Cube
C) Only Scenario and Time
D) Only the Entity dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A data import must correctly map to all required dimension intersections for the target Cube. At minimum, this includes Entity, Account, Scenario (Actual), Time (January), and Flow. If the Cube has active UD dimensions (UD1-UD8), those must also be mapped. An incomplete mapping results in rejected records because the Cube cannot determine where to store the data.
</details>

---

### Question 13 (Medium)
**102.5.1** | Difficulty: Medium

What is the difference between Merge and Replace import modes when loading data?

A) Merge deletes all existing data; Replace adds to existing data
B) Merge combines new data with existing values at each intersection; Replace clears existing data for the target scope before loading new data
C) There is no difference
D) Merge only works with CSV files; Replace only works with database sources

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Merge mode adds or updates data at each intersection without clearing existing data first, which means values for intersections not present in the source file remain unchanged. Replace mode first clears all existing data for the target scope (e.g., the specified Entity, Scenario, and Time combination) and then loads the new data. Replace ensures a clean load with no residual data from previous imports.
</details>

---

### Question 14 (Hard)
**102.5.1** | Difficulty: Hard

An Administrator sets up a scheduled data import that runs nightly. One morning, the import log shows that the import ran but loaded zero records. The source file exists and contains data. Which of the following is the LEAST likely cause?

A) The Workflow Step status was Certified, blocking data loading
B) The source file column headers changed, breaking the field mappings
C) The application server ran out of disk space
D) The Cube was deleted from the application

<details>
<summary>Show answer</summary>

**Correct answer: D)**

While all options could theoretically cause import issues, deleting an entire Cube is the least likely cause of an import loading zero records. The most common causes are the Workflow Step being in a locked status (Certified or Not Started), mapping mismatches due to source file format changes, or infrastructure issues. A deleted Cube would cause a more fundamental error rather than a zero-records-loaded result.
</details>

---

### Question 15 (Easy)
**102.5.1** | Difficulty: Easy

Which source file format is most commonly used for flat-file data imports in OneStream?

A) PDF
B) CSV (Comma-Separated Values)
C) DOCX
D) PNG

<details>
<summary>Show answer</summary>

**Correct answer: B)**

CSV (Comma-Separated Values) is the most commonly used flat-file format for data imports in OneStream. CSV files are simple, widely supported by source systems, and easily generated from ERP systems, databases, and spreadsheets. OneStream's import connectors are designed to parse CSV files efficiently with configurable delimiters and field mappings.
</details>

---

### Question 16 (Medium)
**102.5.1** | Difficulty: Medium

An Administrator wants to validate source data before committing it to the Cube. Which feature allows the import to run in a preview or test mode?

A) There is no preview mode; all imports write directly to the Cube
B) The import can be configured to run in a validation-only (dry run) mode that checks mappings and identifies errors without writing data
C) Preview mode is only available for manual data entry
D) The Administrator must create a separate test Cube for validation

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream import processes can be configured to run in a validation-only or dry run mode. This mode processes the source data through all mapping and transformation rules, identifies any errors or unmapped members, and reports the results without actually writing data to the Cube. This allows administrators to verify the import configuration before committing data.
</details>

---

### Question 17 (Hard)
**102.5.1** | Difficulty: Hard

An Administrator is loading data from multiple source systems that use different chart-of-accounts structures. Some source systems send data at a detailed level while others send summarized totals. What is the recommended approach for handling this?

A) Require all source systems to conform to a single format before sending data
B) Configure separate import connectors for each source system, each with its own mapping tables and transformation rules tailored to that source's format
C) Load all data into a single staging area and manually reclassify
D) Only accept data from the most detailed source system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is to configure separate import connectors for each source system. Each connector has its own mapping tables (translation tables) that translate the source system's codes to OneStream dimension members, and its own transformation rules to handle differences in data granularity and format. This modular approach keeps each integration maintainable and independent.
</details>

---

### Question 18 (Easy)
**102.5.1** | Difficulty: Easy

After successfully importing data, what should the Administrator do to verify the data is correct in the Cube?

A) Immediately certify the Workflow Step
B) Review the import log for any warnings, then spot-check data values using a Cube View or Form Template
C) Restart the application server
D) Export the entire Cube to Excel

<details>
<summary>Show answer</summary>

**Correct answer: B)**

After a successful import, the Administrator should first review the import log for any warnings or partial failures, then spot-check key data values using a Cube View or Form Template to confirm the loaded data matches expected values. This two-step verification ensures data quality before the Workflow progresses.
</details>

---

### Question 19 (Medium)
**102.5.1** | Difficulty: Medium

An Administrator needs to import data where the source file contains a single "Amount" column but the OneStream Cube requires separate Flow dimension members for "Opening Balance," "Activity," and "Closing Balance." How should this be handled?

A) The source system must be modified to provide separate columns
B) Configure the import mapping to assign a default Flow member (e.g., "Activity") to the imported amount, and handle Opening/Closing Balances through business rules or separate processes
C) Ignore the Flow dimension during import
D) Create a custom Cube without the Flow dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When the source data does not distinguish between Flow members, the import mapping should assign a default Flow member (typically "Activity" or "Periodic") to the imported values. Opening and Closing Balance calculations can then be handled through business rules or consolidation processes that derive these values from the Activity amounts and prior period balances. The Flow dimension cannot be ignored as it is a required dimension.
</details>

---

### Question 20 (Hard)
**102.5.1** | Difficulty: Hard

An Administrator discovers that a data import loaded negative values for Revenue accounts when positive values were expected. The source file contains positive amounts. What is the most likely cause?

A) The Cube has a system error
B) The import transformation rules include a sign-reversal expression for Revenue accounts, or the Account Type sign convention is applying an automatic adjustment
C) The source file is corrupted
D) The Workflow Profile is configured incorrectly

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most likely cause is that the import connector includes a transformation rule that reverses signs for certain account types, or the Account Type sign convention in OneStream is interpreting the source values differently than expected. For example, if Revenue accounts are configured with a specific sign convention, positive source values may be stored as negative in the Cube. The Administrator should review the import mapping transformation rules and the Account dimension member properties to verify the sign handling logic.
</details>
