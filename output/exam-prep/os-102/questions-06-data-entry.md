# Question Bank - Section 6: Data Entry (14% of exam)

## Objectives
- **102.6.1:** Understand data entry options available in OneStream
- **102.6.2:** Configure and use Form Templates for data entry
- **102.6.3:** Set up Workflow for data entry
- **102.6.4:** Compile forms and calculate data
- **102.6.5:** Investigate and validate entered data

---

## Objective 102.6.1: Data entry options

### Question 1 (Easy)
**102.6.1** | Difficulty: Easy

Which of the following is a valid method for entering data into a OneStream Cube?

A) Directly editing the database tables
B) Using a Form Template within the Workflow
C) Modifying the Dimension Library
D) Editing Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Form Templates within the Workflow are a primary method for manual data entry in OneStream. They provide structured, controlled interfaces where users can input data at specific dimension intersections. Direct database editing is never supported; the Dimension Library and Application Properties are configuration areas, not data entry mechanisms.
</details>

---

### Question 2 (Medium)
**102.6.1** | Difficulty: Medium

Data Adapters can be attached to which Dashboard Component Type?

A) Text Box
B) Static Label
C) Chart Advanced
D) Simple Header

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Data Adapters can be attached to the Chart Advanced component type in dashboards. Data Adapters are the mechanism that connects dashboard components to Cube data, enabling dynamic data retrieval and display. Not all component types support Data Adapters; static components like labels and headers do not require data connections.
</details>

---

## Objective 102.6.2: Form Templates

### Question 3 (Easy)
**102.6.2** | Difficulty: Easy

What is a Form Template in OneStream?

A) A report exported to Excel
B) A pre-configured data entry layout that defines which rows, columns, and dimension intersections are available for user input
C) A security template for user access
D) A Workflow scheduling tool

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Form Template is a pre-configured data entry layout that defines the structure of the data entry form, including which dimension members appear on rows and columns, which cells are editable, and the point of view (POV) settings. Form Templates ensure consistent, controlled data entry across users.
</details>

---

### Question 4 (Medium)
**102.6.2** | Difficulty: Medium

When designing a Form Template, which setting controls whether a user can modify the value in a specific cell?

A) The cell's background color
B) The cell's Read Only / Input property
C) The user's security role only
D) The Workflow Step status only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Read Only / Input property at the cell level in the Form Template design determines whether a cell is editable (Input) or display-only (Read Only). While security roles and Workflow Step status also affect editability, the Form Template cell property is the primary design-time control for specifying which cells accept user input.
</details>

---

### Question 5 (Hard)
**102.6.2** | Difficulty: Hard

An Administrator creates a Form Template that includes calculated rows (e.g., Total Revenue = Product A + Product B). Where should these calculations be defined?

A) In the Cube dimension hierarchy only
B) In the Form Template using calculated member expressions or row/column formulas
C) In a separate business rule that runs after data entry
D) Calculations cannot be displayed on Form Templates

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Calculated rows and columns in Form Templates can be defined using calculated member expressions or formula rows/columns within the Form Template design. These calculations are evaluated when the form is rendered or refreshed, providing users with real-time feedback on totals and derived values during data entry. While Cube-level calculations and business rules can also contribute, the Form Template itself supports inline calculations.
</details>

---

## Objective 102.6.3: Workflow setup for data entry

### Question 6 (Easy)
**102.6.3** | Difficulty: Easy

Which Workflow Step type is typically used for manual data entry by end users?

A) Certification Step
B) Import Step
C) Input Step
D) Review Step

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Input Step is the Workflow Step type designed for manual data entry. When a user navigates to an Input Step in the Workflow, they are presented with the configured Form Template(s) where they can enter or modify data. Certification and Review Steps are for approval processes, and Import Steps are for automated data loading.
</details>

---

### Question 7 (Medium)
**102.6.3** | Difficulty: Medium

An Administrator needs to ensure that data entry is only available during a specific date range within the period. Which Workflow feature supports this?

A) Cube locking
B) Workflow Step open and close dates (deadlines)
C) Application Properties time settings
D) Dimension member effective dating

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workflow Steps can be configured with open and close dates (deadlines) that control when the Step is available for user interaction. Before the open date or after the close date, the Step is not available for data entry. This feature helps enforce submission deadlines and control the timing of data entry within the financial close or planning cycle.
</details>

---

## Objective 102.6.4: Compiling forms and calculating data

### Question 8 (Medium)
**102.6.4** | Difficulty: Medium

What does it mean to "compile" a form in OneStream?

A) To export the form to PDF format
B) To validate and process the Form Template so it generates the correct data entry layout based on current dimension members and POV settings
C) To lock the form from further editing
D) To submit the form for approval

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Compiling a form in OneStream means processing the Form Template to generate the actual data entry layout. The compilation resolves dynamic member selections, applies the current Point of View (POV) settings, and builds the form structure that users see. If dimension members change or the POV is updated, recompilation may be needed to reflect the updates.
</details>

---

### Question 9 (Hard)
**102.6.4** | Difficulty: Hard

After a user enters data on a form, the calculated totals do not immediately reflect the new values. What is the most likely reason?

A) The form has a display error
B) The data has not been saved and/or the Cube aggregation has not been triggered; the user needs to save and recalculate
C) The user does not have permission to view totals
D) Calculated members are not supported on the form

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In OneStream, data entered on a form must first be saved to the Cube. If the parent-level aggregation is configured as Stored (rather than Dynamic), a calculation or aggregation process must also be triggered to update the totals. Users should save their data and then refresh the form or trigger the calculation to see updated totals. Dynamic Calc members update automatically upon refresh.
</details>

---

## Objective 102.6.5: Investigating and validating data

### Question 10 (Easy)
**102.6.5** | Difficulty: Easy

Which feature in OneStream allows a user to trace a data value back to its source or see its component parts?

A) Dimension Browser
B) Data Drill-Through (Cell Detail / Drill Down)
C) Application Log
D) Cube Properties viewer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Drill-Through (also called Cell Detail or Drill Down) allows users to click on a data value and see its component parts, source records, or underlying detail. This is essential for data validation and auditing, enabling users to trace aggregated values back to their base-level inputs or source system records.
</details>

---

### Question 11 (Medium)
**102.6.5** | Difficulty: Medium

An Administrator wants to verify that data entered by users matches expected values before the Workflow Step is completed. Which approach is recommended?

A) Manually review all data entries in the database
B) Configure validation rules or data quality checks that run during or after data entry, flagging discrepancies
C) Wait until the data is reported and check the output
D) Ask each user to email a summary of their entries

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is to configure validation rules or data quality checks that can run during or after data entry. These validations can flag discrepancies such as missing data, values outside acceptable ranges, or imbalances. This provides automated quality control within the Workflow process itself.
</details>

---

### Question 12 (Hard)
**102.6.5** | Difficulty: Hard

A user reports that their data entry form shows different values than what was entered. The Administrator investigates and finds the correct values in the Cube. What is the most likely explanation?

A) The Cube data has been corrupted
B) The Form Template's Point of View (POV) settings may be pointing to a different dimension intersection than expected, or the form includes calculated adjustments
C) The user's browser cache is displaying old data
D) Another user has overwritten the data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When form values differ from what a user expects, the most common cause is a POV mismatch. The Form Template may be displaying data from a different Scenario, Period, Entity, or other dimension member than the user intended. Additionally, the form may include calculated adjustments, currency translations, or consolidation entries that modify the displayed values. The Administrator should verify the POV settings and any form-level calculations.
</details>

---

### Question 13 (Easy)
**102.6.1** | Difficulty: Easy

Which of the following is NOT a valid data entry method in OneStream?

A) Form Template within a Workflow Step
B) Cube View with input-enabled cells
C) Directly editing the SQL database tables
D) Data import through a Workflow data load step

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Directly editing SQL database tables is never a valid or supported data entry method in OneStream. All data entry must go through the application layer using Form Templates, Cube Views, or data import processes. Direct database manipulation bypasses validation rules, security controls, audit trails, and can corrupt data integrity.
</details>

---

### Question 14 (Medium)
**102.6.2** | Difficulty: Medium

An Administrator needs to create a Form Template where the rows display Account members and the columns display monthly Time periods. Which Form Template axis configuration is correct?

A) Rows: Time, Columns: Account
B) Rows: Account, Columns: Time
C) Rows: Entity, Columns: Account
D) Rows: Scenario, Columns: Flow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To display Account members on rows and monthly Time periods on columns, the Form Template must be configured with the Account dimension on the row axis and the Time dimension on the column axis. Other dimensions (Entity, Scenario, Flow, etc.) would be set in the Point of View (POV) to define the fixed context for the form.
</details>

---

### Question 15 (Medium)
**102.6.3** | Difficulty: Medium

An Administrator assigns a Form Template to a Workflow Input Step. What additional configuration determines which Entity members see this form?

A) The form's background color
B) The Workflow Profile entity assignments and the Step's entity scope
C) The Cube Properties
D) The Application Properties General Tab

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Profile entity assignments and the Step's entity scope determine which entities can access the Form Template. Even though the form is assigned to the Step, only entities that are included in the Workflow Profile and assigned to that specific Step will see and interact with the form during the Workflow process.
</details>

---

### Question 16 (Hard)
**102.6.4** | Difficulty: Hard

An Administrator notices that after recompiling a Form Template, some rows that previously existed no longer appear. What is the most likely cause?

A) The form data has been deleted
B) Dimension members referenced by those rows have been removed, renamed, or their hierarchy position has changed, causing the dynamic member selection to exclude them
C) The Cube has been corrupted
D) The Workflow Step has been certified

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a Form Template is recompiled, it re-evaluates dynamic member selections and hierarchy references. If dimension members have been removed, renamed, or moved in the hierarchy since the last compilation, the form may no longer include those members in its rows or columns. The Administrator should review the Form Template's member selection criteria and compare them with the current dimension hierarchy.
</details>

---

### Question 17 (Easy)
**102.6.5** | Difficulty: Easy

What is the purpose of the Point of View (POV) bar on a data entry form?

A) To display the form title
B) To allow users to select the dimensional context (Entity, Scenario, Time period, etc.) for the data being viewed or entered
C) To show the last login time
D) To navigate between Workflow Steps

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Point of View (POV) bar displays and allows selection of the dimensional context for the form. Users can change the Entity, Scenario, Time period, and other dimension members through the POV to view or enter data at different intersections. The POV determines which "slice" of the Cube the form is displaying.
</details>

---

### Question 18 (Medium)
**102.6.2** | Difficulty: Medium

An Administrator wants to restrict which Account members appear on a Form Template based on the selected Entity. Which Form Template feature supports this?

A) Static member lists only
B) Dynamic member selection using conditional expressions or member filters that reference the POV Entity
C) Cube Security settings
D) This is not possible; all forms show the same members regardless of Entity

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Form Templates support dynamic member selection, which can use conditional expressions or member filters that reference the current POV (including the selected Entity). This allows the form to display different Account members depending on which Entity the user has selected, ensuring that each entity sees only the accounts relevant to its operations.
</details>

---

### Question 19 (Hard)
**102.6.4** | Difficulty: Hard

A user enters data on a form and clicks Save. The save operation completes successfully, but when the user changes the POV to a different entity and then returns, the previously entered data is not visible. What should the Administrator investigate?

A) The database has lost the data
B) Whether the save operation wrote data to a different dimension intersection than expected (e.g., wrong Scenario or Flow member in the POV), or whether a business rule is clearing the data upon save
C) Whether the user's browser is blocking data storage
D) Whether the application server needs a restart

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When saved data is not visible upon returning to the same form view, the most common cause is that the data was saved to a different dimension intersection than the user intended. This can happen if the POV members (Scenario, Flow, or UD dimensions) were different than expected during the save operation. Additionally, post-save business rules might be moving or clearing data. The Administrator should verify the exact intersection where data was saved using a Cube View.
</details>

---

### Question 20 (Medium)
**102.6.1** | Difficulty: Medium

What is a Cube View in the context of data entry?

A) A graphical chart of Cube data
B) An ad-hoc, grid-based interface that displays Cube data at specified dimension intersections and can be configured to allow data input
C) A security configuration screen
D) A Cube backup utility

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Cube View is an ad-hoc, grid-based interface that allows users and administrators to view and interact with Cube data at specified dimension intersections. Unlike Form Templates which are pre-configured, Cube Views provide flexible, on-the-fly data exploration and can be configured to allow direct data input at base-level intersections. They are useful for both data entry and data validation.
</details>

---

### Question 21 (Easy)
**102.6.3** | Difficulty: Easy

Can multiple Form Templates be assigned to the same Workflow Input Step?

A) No, each Step can only have one Form Template
B) Yes, multiple Form Templates can be assigned to the same Step, giving users access to different data entry layouts
C) Only if the Step is a Review Step
D) Only if the forms are for different Cubes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Multiple Form Templates can be assigned to the same Workflow Input Step. This allows users to access different data entry layouts within a single Step, such as one form for revenue accounts and another for expense accounts. Users can switch between the available forms during the data entry process.
</details>

---

### Question 22 (Hard)
**102.6.5** | Difficulty: Hard

An Administrator needs to set up a validation that prevents users from confirming a Workflow Step if required accounts have zero or blank values. Where should this validation be configured?

A) In the Cube dimension properties
B) As a data quality business rule associated with the Workflow Step that checks for completeness before allowing the status transition
C) In the Form Template cell formatting
D) In the application's fiscal year settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data quality validations that must be enforced at the Workflow level are typically implemented as business rules associated with the Workflow Step. These rules execute when the user attempts to confirm or advance the Step, checking for conditions such as required accounts having non-zero values, balance checks, or other completeness criteria. If the validation fails, the status transition is blocked and the user receives an error message.
</details>
