# Question Bank - Section 2: Application Properties and Dimensions (17% of exam)

## Objectives
- **102.2.1:** Configure Application Properties for a OneStream application
- **102.2.2:** Create and manage Dimensions and Members

---

## Objective 102.2.1: Application Properties configuration

### Question 1 (Easy)
**102.2.1** | Difficulty: Easy

Where should an Administrator maintain currencies in OneStream?

A) Workflow tab > Workflow Properties
B) Cube tab > Cube Properties > Currency Settings
C) Application tab > Application Properties > General Tab > Currency Filter
D) Dimension tab > Currency Dimension > Properties

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Currencies are maintained in the Application tab under Application Properties on the General Tab using the Currency Filter. This is where administrators define which currencies are available across the application, including the local and reporting currencies.
</details>

---

### Question 2 (Easy)
**102.2.1** | Difficulty: Easy

Which tab in Application Properties allows an Administrator to configure the application's fiscal year settings?

A) Security Tab
B) General Tab
C) Dimensions Tab
D) Workflow Tab

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The General Tab within Application Properties contains fiscal year settings including the start month of the fiscal year, the number of periods, and time-related configurations. These settings affect how data is organized across time periods in the application.
</details>

---

### Question 3 (Medium)
**102.2.1** | Difficulty: Medium

An Administrator needs to add a new year to the OneStream application so users can begin entering budget data for the upcoming fiscal year. Where is this configured?

A) Application tab > Application Properties > Time tab
B) Dimension Library > Time dimension > Add Year
C) Cube tab > Cube Properties > Add Period
D) Workflow tab > Workflow Properties > Time Settings

<details>
<summary>Show answer</summary>

**Correct answer: A)**

New years are added through the Application Properties under the Time tab. This is where the Administrator manages the application's time periods, including adding new years and configuring period structures. The Time dimension members are then automatically generated based on these settings.
</details>

---

### Question 4 (Medium)
**102.2.1** | Difficulty: Medium

Which Application Properties setting determines how many decimal places are displayed for data values across the application?

A) The Cube-level precision setting
B) The Application Properties > General Tab > Number Format settings
C) The Dashboard display format
D) The Workflow profile data format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Number Format settings on the General Tab of Application Properties control the default decimal places and number formatting across the application. While individual dashboards and reports can override display formatting, the application-level setting provides the default behavior.
</details>

---

### Question 5 (Hard)
**102.2.1** | Difficulty: Hard

An Administrator changes the fiscal year start month in Application Properties from January to April after data has already been loaded. What is the impact?

A) No impact; data automatically realigns to the new fiscal year
B) Data is lost and must be reloaded
C) The change is blocked if data exists in the application
D) Existing data remains but time period mapping must be reviewed and data may need to be reloaded to align with the new fiscal calendar

<details>
<summary>Show answer</summary>

**Correct answer: D)**

Changing the fiscal year start month after data exists is a significant change. Existing data is not automatically realigned. The Administrator must carefully review time period mappings and may need to reload data to ensure it aligns correctly with the new fiscal calendar structure. This type of change should be thoroughly tested in a non-production environment first.
</details>

---

## Objective 102.2.2: Creating Dimensions and Members

### Question 6 (Easy)
**102.2.2** | Difficulty: Easy

Which of the following is a standard (system) dimension in every OneStream application?

A) Product
B) Customer
C) Entity
D) Region

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Entity is one of the standard system dimensions that exists in every OneStream application. Other standard dimensions include Account, Flow, Intercompany, Scenario, Time, and Consolidation. Product, Customer, and Region are examples of user-defined (UD) dimensions that may or may not exist depending on the application design.
</details>

---

### Question 7 (Easy)
**102.2.2** | Difficulty: Easy

What is the term for a dimension member that has no children beneath it in the hierarchy?

A) Parent member
B) Base member
C) Root member
D) Aggregate member

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A base member (also called a leaf member) is a dimension member that has no children. Base members are where data is typically entered or loaded. Parent members aggregate data from their children, and the root member is the topmost member in the hierarchy.
</details>

---

### Question 8 (Medium)
**102.2.2** | Difficulty: Medium

An Administrator needs to create a new Entity member called "US_West" under the parent "United_States". Which is the correct location to do this?

A) Application tab > Dimensions > Entity dimension > add member under United_States
B) Cube tab > Cube Dimensions > Entity > New Member
C) Workflow tab > Entities > Add New
D) Application Properties > Entity Settings > New Member

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Entity members are created and managed in the Dimensions area under the Application tab. The Administrator navigates to the Entity dimension, locates the parent member "United_States" and adds the new child member "US_West" beneath it. This is the standard approach for managing dimension member hierarchies.
</details>

---

### Question 9 (Medium)
**102.2.2** | Difficulty: Medium

When creating a new Account dimension member, which property determines how the member aggregates with its siblings under a parent?

A) Data Storage
B) Account Type
C) Aggregation Weight
D) Consolidation Method

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Aggregation Weight property (which can be set to values like +1, -1, or 0) determines how an Account member's value is combined with its siblings when aggregating to the parent. For example, expense accounts might have a weight of +1 under a Total Expenses parent but -1 under a Net Income calculation. Account Type categorizes the account but does not directly control aggregation behavior.
</details>

---

### Question 10 (Hard)
**102.2.2** | Difficulty: Hard

An Administrator creates a new Entity member and assigns it to a Cube. Users report they cannot see the new entity in their Workflow. What is the most likely cause?

A) The Cube needs to be recalculated
B) The entity has not been assigned to the Workflow profile and/or security has not been granted
C) The application server needs to be restarted
D) The entity must first be added to the Scenario dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a new Entity member is created, it must also be included in the appropriate Workflow profile so it appears in the Workflow, and users must be granted security access to the entity. Simply creating the dimension member and assigning it to a Cube is not sufficient for it to appear in a user's Workflow view. Entities are not added to the Scenario dimension, and server restarts are not required for dimension changes.
</details>

---

### Question 11 (Medium)
**102.2.2** | Difficulty: Medium

Which dimension in OneStream is used to distinguish between different versions of data such as Actual, Budget, and Forecast?

A) Flow
B) Time
C) Scenario
D) Consolidation

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Scenario dimension is used to differentiate between data versions such as Actual, Budget, and Forecast. Each scenario represents a distinct data set that can have its own workflow, time periods, and data entry rules. Flow tracks movement types, Time handles periods, and Consolidation manages consolidation methods.
</details>

---

### Question 12 (Hard)
**102.2.2** | Difficulty: Hard

An Administrator needs to set up a dimension member so that it stores no data in the Cube but is used only for grouping and aggregation purposes. Which Data Storage property should be assigned?

A) Store Data
B) Never Share
C) Dynamic Calc
D) Shared Member

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Dynamic Calc means the member does not store data in the Cube; instead its value is calculated dynamically by aggregating its children whenever it is queried. This is appropriate for parent members used purely for grouping. Store Data would allocate storage. Never Share prevents sharing but still stores data. Shared Member is used when a member appears in multiple locations in the hierarchy.
</details>

---

### Question 13 (Easy)
**102.2.1** | Difficulty: Easy

Which Application Properties tab is used to manage the list of active Scenarios (such as Actual and Budget) in the application?

A) General Tab
B) Scenario Tab
C) Time Tab
D) Dimensions Tab

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Scenario Tab within Application Properties is where administrators manage the list of active Scenarios in the application. This includes defining which Scenarios are available (Actual, Budget, Forecast, etc.) and configuring their properties such as default frequency and data protection settings.
</details>

---

### Question 14 (Medium)
**102.2.2** | Difficulty: Medium

An Administrator needs to place the same Entity member under two different parent members in the hierarchy for reporting purposes. Which feature should be used?

A) Create two separate members with different names
B) Use a Shared Member, which allows the same base member to appear under multiple parents
C) Create an alias for the member
D) This is not possible in OneStream

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream supports Shared Members, which allow a single base member to appear under multiple parent members in the hierarchy. The shared instance references the same data as the original member, ensuring consistency across reporting views. This is useful for creating alternative roll-up structures without duplicating data.
</details>

---

### Question 15 (Medium)
**102.2.1** | Difficulty: Medium

What is the impact of modifying the application's base currency in Application Properties after data has been loaded?

A) No impact; all data automatically converts to the new base currency
B) Existing data stored in the previous base currency is not automatically reconverted; currency translation processes must be re-executed
C) The change is not allowed once any data exists
D) All data is deleted when the base currency is changed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Changing the base currency after data has been loaded does not automatically reconvert existing stored values. Currency translation processes need to be re-executed to recalculate values in the new base currency. This is a significant change that should be carefully planned and tested in a non-production environment before applying to Production.
</details>

---

### Question 16 (Hard)
**102.2.2** | Difficulty: Hard

An Administrator creates a new Account member and assigns it an Account Type of "Revenue." What does the Account Type property control?

A) Only the display color of the member in reports
B) The sign behavior, time balance calculation method, and how the member interacts with financial logic such as variance calculations
C) Whether the member can have children
D) The security access level for the member

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Account Type property (Revenue, Expense, Asset, Liability, Equity, etc.) controls important financial behaviors including sign convention (how positive/negative values are interpreted), time balance behavior (flow vs. balance), and how the member participates in financial calculations like variance analysis. For example, a Revenue account with a positive variance is favorable, while an Expense account with a positive variance is unfavorable.
</details>

---

### Question 17 (Easy)
**102.2.2** | Difficulty: Easy

What is the topmost member in a dimension hierarchy called?

A) Base member
B) Leaf member
C) Root member
D) Shared member

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The root member is the topmost member in a dimension hierarchy. It sits at the top of the tree structure and all other members are descendants of the root. The root member typically represents the total or "All" level of the dimension, aggregating all data from its children below.
</details>

---

### Question 18 (Medium)
**102.2.2** | Difficulty: Medium

When an Administrator adds a new member to the Account dimension, which property determines whether the member stores data at the period level or as a balance (point-in-time)?

A) Data Storage
B) Time Balance property (Flow vs. Balance)
C) Aggregation Weight
D) Account Type alone

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Time Balance property determines how an Account member handles data across time periods. "Flow" accounts accumulate values across periods (e.g., Revenue is the sum of monthly amounts). "Balance" accounts represent a point-in-time value (e.g., Cash balance carries forward the ending balance). This affects how year-to-date and period-to-date calculations are performed.
</details>

---

### Question 19 (Hard)
**102.2.1** | Difficulty: Hard

An Administrator needs to configure the application so that a 13th period is available each year for audit adjustments. Where is this configured?

A) By manually adding a member to the Time dimension
B) In Application Properties > Time Tab, by configuring the number of periods per year to include an adjustment period
C) In the Workflow Profile settings
D) In the Cube Properties under Period Configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Time Tab in Application Properties allows administrators to configure the period structure for the application, including the option to add adjustment periods beyond the standard 12 monthly periods. Configuring a 13th adjustment period ensures it is systematically available across the application for year-end audit entries.
</details>

---

### Question 20 (Medium)
**102.2.2** | Difficulty: Medium

An Administrator renames an Entity dimension member from "US_East" to "US_Eastern." What must be verified after the rename?

A) Nothing; all references update automatically throughout the application
B) Business rules, dashboards, data import mappings, and Workflow profiles that reference the old name must be reviewed and updated
C) Only the Cube needs to be recalculated
D) Only security settings need to be updated

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Renaming a dimension member does not automatically update all references throughout the application. Business rules that reference the member by name, dashboard configurations, data import mapping tables, and Workflow profile assignments may still use the old name. The Administrator must review and update all these references to prevent errors in data processing and reporting.
</details>

---

### Question 21 (Easy)
**102.2.2** | Difficulty: Easy

Which of the following is an example of a User Defined (UD) dimension in OneStream?

A) Entity
B) Account
C) Product
D) Time

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Product is an example of a User Defined dimension. Entity, Account, and Time are standard (system) dimensions that exist in every OneStream application. User Defined dimensions (UD1 through UD8) are configured by administrators to represent additional analytical categories such as Product, Region, Department, or Channel.
</details>

---

### Question 22 (Hard)
**102.2.2** | Difficulty: Hard

An Administrator needs to load a large number of dimension members (500+ accounts) into the Account dimension. What is the most efficient approach?

A) Add each member manually through the Dimension Library interface one at a time
B) Use the dimension member load utility to bulk-import members from a formatted file (CSV or Excel)
C) Create a business rule to generate the members at runtime
D) Copy members from the Scenario dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For bulk loading dimension members, the most efficient approach is to use the dimension member load utility, which accepts formatted files (CSV or Excel) containing member properties such as name, parent, account type, aggregation weight, and data storage. This eliminates the manual effort of adding members one by one and reduces the risk of data entry errors during configuration.
</details>
