# Question Bank - Section 2: Dashboard Parameters (18% of exam)

## Objectives
- **300.2.1:** Identify and describe the different parameter types available in OneStream dashboards
- **300.2.2:** Apply parameter formatting across different views and components
- **300.2.3:** Implement Member List expansion in dashboard parameters
- **300.2.4:** Configure Dashboard Extender Business Rules for parameter customization
- **300.2.5:** Implement dynamic POV, Column, and Row changes using parameters
- **300.2.6:** Configure parameter-dashboard interlinking for navigation between dashboards

---

## Objective 300.2.1: Identify and describe the different parameter types

### Question 1 (Easy)
**300.2.1** | Difficulty: Easy

Which of the following is a standard parameter type available in OneStream dashboards?

A) SQL Parameter
B) Bound List Parameter
C) Binary Parameter
D) XML Parameter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream dashboards support several parameter types including Bound List Parameters, Text Box Parameters, Check Box Parameters, and others. Bound List Parameters allow users to select from a predefined or dynamic list of values. SQL Parameter (A), Binary Parameter (C), and XML Parameter (D) are not standard OneStream dashboard parameter types.
</details>

---

### Question 2 (Easy)
**300.2.1** | Difficulty: Easy

What is the purpose of a Text Box parameter on a OneStream dashboard?

A) To display a static image on the dashboard
B) To allow users to enter free-form text input that can be used by dashboard components or Business Rules
C) To create a hyperlink to an external website
D) To define a new dimension in the application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Text Box parameter provides a free-form text input field where users can type values. These values can then be consumed by dashboard components, Business Rules, or filters. It is not for displaying images (A), creating hyperlinks (C), or defining dimensions (D).
</details>

---

### Question 3 (Medium)
**300.2.1** | Difficulty: Medium

An administrator needs a parameter that allows users to select a single member from the Entity dimension. Which parameter type and configuration is most appropriate?

A) A Text Box parameter where users type the Entity member name manually
B) A Bound List parameter bound to the Entity dimension with single-select enabled
C) A Check Box parameter with one checkbox per Entity
D) A Bound List parameter bound to the Time dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Bound List parameter bound to the Entity dimension with single-select is the correct choice for selecting a single Entity member. A Text Box (A) would require users to know and type exact member names, which is error-prone. Check Boxes (C) are not practical for large dimension lists. Binding to the Time dimension (D) would show the wrong members.
</details>

---

## Objective 300.2.2: Apply parameter formatting across views

### Question 4 (Medium)
**300.2.2** | Difficulty: Medium

When formatting a parameter to display member descriptions instead of member names, which property should be configured?

A) The DataType property of the parameter
B) The Display property or MemberDisplay setting of the parameter to show descriptions
C) The Font property of the dashboard layout
D) The Column Width property of the Cube View

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To display member descriptions instead of internal member names, you configure the Display or MemberDisplay property of the parameter. This controls whether users see codes, names, descriptions, or a combination. DataType (A) controls data format, not display labels. Font (C) and Column Width (D) affect visual formatting, not member label display.
</details>

---

### Question 5 (Easy)
**300.2.2** | Difficulty: Easy

What happens when a parameter's formatting is set to show "Name - Description" for dimension members?

A) Only the member's numeric ID is displayed
B) Both the member's internal name and its description are displayed, separated by a dash
C) The parameter becomes read-only
D) The member description replaces the member name in the database

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When set to "Name - Description," the parameter displays both the member's internal name and its human-readable description (e.g., "E001 - North America"). This does not show numeric IDs (A), make the parameter read-only (C), or modify database values (D).
</details>

---

## Objective 300.2.3: Implement Member List expansion

### Question 6 (Medium)
**300.2.3** | Difficulty: Medium

What is the purpose of Member List expansion in a dashboard parameter?

A) To compress dimension hierarchies into a flat list for faster processing
B) To dynamically expand a member list expression (such as children or descendants) into the individual members it resolves to
C) To increase the maximum number of characters allowed in a text parameter
D) To duplicate member records across multiple cubes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Member List expansion dynamically resolves expressions like "Children of [Total Entity]" into the actual list of child members. This allows parameters to present dynamically generated lists based on hierarchy relationships. It does not compress hierarchies (A), expand character limits (C), or duplicate records (D).
</details>

---

### Question 7 (Hard)
**300.2.3** | Difficulty: Hard

A parameter is configured with a Member List expression that uses `Descendants("TotalEntity", 2)`. The hierarchy has 3 levels below TotalEntity. What members will appear in the parameter dropdown?

A) Only TotalEntity itself
B) Only the immediate children of TotalEntity (level 1)
C) Members at level 1 and level 2 below TotalEntity
D) All members at all levels including TotalEntity

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The `Descendants` function with a depth parameter of 2 returns all descendants down to 2 levels below the specified member. This includes the immediate children (level 1) and their children (level 2), but not TotalEntity itself (A/D) and not the third level below (which would require depth 3). Level 1 only (B) would be the result of `Children` instead.
</details>

---

## Objective 300.2.4: Configure Dashboard Extender Business Rules

### Question 8 (Medium)
**300.2.4** | Difficulty: Medium

What is the primary purpose of a Dashboard Extender Business Rule?

A) To extend the physical size of the dashboard canvas beyond the default pixel limit
B) To provide custom server-side logic that populates, filters, or modifies dashboard parameter values and behavior
C) To add extra CPU cores to the application server for dashboard rendering
D) To convert dashboards from OneStream format to Microsoft Excel format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dashboard Extender Business Rules allow developers to write custom server-side logic (in VB.NET or C#) that can populate parameter lists, apply custom filtering, validate user selections, and modify dashboard behavior dynamically. They do not change canvas size (A), add CPU cores (C), or convert to Excel (D).
</details>

---

### Question 9 (Hard)
**300.2.4** | Difficulty: Hard

A Dashboard Extender Business Rule is configured to populate a parameter based on the user's security profile. However, all users see the same list regardless of their profile. What is the most likely cause?

A) The Business Rule is using a hardcoded member list instead of referencing the current user's security context via the API
B) The dashboard cache timeout is set to zero
C) The parameter is configured as a Text Box instead of a Bound List
D) The OneStream server does not support user-based filtering

<details>
<summary>Show answer</summary>

**Correct answer: A)**

If all users see the same parameter list, the Business Rule is likely using a hardcoded list rather than querying the current user's security context through the OneStream API (e.g., using `si.UserName` or security-related API calls). A cache timeout of zero (B) would mean no caching, which would not cause this issue. If it were a Text Box (C), there would be no dropdown list at all. OneStream fully supports user-based filtering (D).
</details>

---

## Objective 300.2.5: Dynamic POV, Column, and Row changes

### Question 10 (Medium)
**300.2.5** | Difficulty: Medium

How can a dashboard parameter dynamically change the columns displayed in a Cube View?

A) By binding the Cube View's column dimension to the parameter so that changing the parameter value updates which members appear as columns
B) By exporting the Cube View to Excel and manually rearranging columns
C) By restarting the OneStream application server after each parameter change
D) By editing the database schema to add new column tables

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Dashboard parameters can be bound to Cube View properties including column definitions. When a user changes the parameter value, the Cube View dynamically updates its columns to reflect the new selection. This is a core feature of OneStream's parameterized dashboards. Exporting to Excel (B), restarting servers (C), and editing database schemas (D) are not valid approaches.
</details>

---

### Question 11 (Easy)
**300.2.5** | Difficulty: Easy

Which dashboard element is most commonly used to allow end users to change the Point of View (POV) in a OneStream dashboard?

A) A static label component
B) The POV bar with bound dimension parameters
C) A chart legend
D) The dashboard background image

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The POV bar is the standard mechanism for allowing end users to change the Point of View (Scenario, Time, Entity, etc.) on a dashboard. It contains bound dimension parameters that drive the data context for all linked components. Labels (A), chart legends (C), and background images (D) do not provide POV selection capability.
</details>

---

## Objective 300.2.6: Parameter-dashboard interlinking

### Question 12 (Medium)
**300.2.6** | Difficulty: Medium

What does parameter-dashboard interlinking enable in OneStream?

A) It allows parameters from one dashboard to drive navigation to and pass context into another dashboard
B) It merges two dashboards into a single combined dashboard permanently
C) It creates a backup copy of the dashboard parameters
D) It links OneStream dashboards to third-party BI tools via ODBC

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Parameter-dashboard interlinking enables navigation between dashboards while passing parameter context (such as the current POV selections or a selected member) from the source dashboard to the target dashboard. This creates a seamless drill-through experience. It does not merge dashboards (B), create backups (C), or link to third-party tools via ODBC (D).
</details>

---

### Question 13 (Hard)
**300.2.6** | Difficulty: Hard

A developer sets up interlinking so that clicking a row in Dashboard A opens Dashboard B with the selected Entity. However, Dashboard B always opens with the default Entity instead of the selected one. What is the most likely issue?

A) Dashboard B does not have an Entity dimension in its application
B) The interlink configuration does not map the source parameter (selected Entity) to the corresponding target parameter on Dashboard B
C) OneStream does not support passing Entity parameters between dashboards
D) The developer needs to restart the application pool after each interlink configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When the target dashboard ignores the passed parameter, it typically means the interlink configuration is missing the parameter mapping between the source and target dashboards. The source parameter carrying the selected Entity must be explicitly mapped to the corresponding parameter on Dashboard B. OneStream fully supports passing parameters between dashboards (C). Application pool restarts (D) are not required for configuration changes. The Entity dimension exists (A) since Dashboard B can display entities with the default.
</details>

---

### Question 14 (Easy)
**300.2.1** | Difficulty: Easy

What is a Check Box parameter in a OneStream dashboard used for?

A) To display a clickable map of the organization's offices
B) To provide a true/false or on/off toggle that can control dashboard behavior such as showing or hiding a component
C) To create a database checkpoint for recovery purposes
D) To verify that the server's SSL certificate is valid

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Check Box parameter provides a boolean (true/false) toggle on the dashboard. It is commonly used to control dashboard behavior such as showing/hiding components, enabling/disabling features, or toggling between display modes. It is not for maps (A), database checkpoints (C), or SSL validation (D).
</details>

---

### Question 15 (Medium)
**300.2.1** | Difficulty: Medium

A developer needs a parameter that allows users to select multiple Account members simultaneously to filter a Cube View. Which parameter type and configuration is most appropriate?

A) A Text Box parameter where users type all account names separated by commas
B) A Bound List parameter configured for multi-select, bound to the Account dimension
C) A single Check Box parameter
D) A date picker parameter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Bound List parameter with multi-select enabled and bound to the Account dimension allows users to select multiple Account members from a dropdown. This is cleaner and less error-prone than typing comma-separated values in a Text Box (A). A single Check Box (C) can only represent one boolean value. A date picker (D) is for date selection, not dimension members.
</details>

---

### Question 16 (Medium)
**300.2.2** | Difficulty: Medium

A parameter displays Entity members as "E001, E002, E003" but users want to see "North America, Europe, Asia Pacific" instead. What formatting change is needed?

A) Rename the Entity members in the dimension metadata to remove the codes
B) Change the parameter's Display property to show Description instead of Name
C) Change the dashboard's background color
D) Export the parameter values to a CSV file and reimport them

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Changing the parameter's Display property from Name to Description will show the human-readable descriptions ("North America," "Europe," etc.) instead of the internal member codes. Renaming dimension members (A) would affect the entire application and is unnecessary. Background color (C) and CSV export (D) are unrelated.
</details>

---

### Question 17 (Hard)
**300.2.3** | Difficulty: Hard

A Member List expansion expression uses `Base("Revenue")` to populate a parameter. Users report that newly added child accounts under Revenue do not appear in the parameter dropdown until the application is reprocessed. Why?

A) The `Base` function is deprecated and no longer works
B) Member List expressions resolve against the current dimension metadata; newly added members require metadata processing or cube refresh to become visible in the hierarchy
C) The parameter has a maximum of 50 members
D) New accounts can only be added during system maintenance windows

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The `Base` function resolves against the dimension metadata hierarchy at runtime. When new child accounts are added to the metadata but the dimension has not been reprocessed, the hierarchy relationships are not yet updated in the cube, so the new members do not appear in the expansion. There is no member limit (C), the function is not deprecated (A), and accounts can be added anytime (D).
</details>

---

### Question 18 (Easy)
**300.2.5** | Difficulty: Easy

What happens to a Cube View when a user changes the Scenario parameter in the dashboard's POV bar?

A) The Cube View is permanently deleted
B) The Cube View refreshes to display data for the newly selected Scenario
C) The Cube View switches to display a chart instead of a grid
D) Nothing happens; Scenario changes do not affect Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When the Scenario parameter in the POV bar changes, any Cube View bound to that parameter refreshes and displays data for the newly selected Scenario. This is core POV-driven behavior in OneStream. The Cube View is not deleted (A), does not change format (C), and is designed to respond to POV changes (D).
</details>

---

### Question 19 (Medium)
**300.2.4** | Difficulty: Medium

A Dashboard Extender Business Rule needs to dynamically filter a parameter's member list based on the current Time period selected in the POV. Which API object provides access to the current POV context within the Business Rule?

A) The Windows Registry
B) The SessionInfo (si) object, which provides access to the current user's POV selections and session context
C) The server's system clock
D) A hardcoded XML configuration file

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The SessionInfo (si) object in OneStream Business Rules provides access to the current user's session context, including POV selections for Scenario, Time, Entity, and other dimensions. This allows Dashboard Extender rules to dynamically filter parameter lists based on the current POV. The Windows Registry (A), system clock (C), and static XML files (D) do not provide session-specific POV context.
</details>

---

### Question 20 (Medium)
**300.2.5** | Difficulty: Medium

A dashboard has a Cube View with Time on columns. The developer wants users to toggle between showing monthly periods and quarterly periods using a parameter. How should this be implemented?

A) Create two separate dashboards, one for monthly and one for quarterly
B) Bind the Cube View's column member list to a parameter that switches between a monthly member list expression and a quarterly member list expression
C) Ask users to manually type "Monthly" or "Quarterly" into a text field on each use
D) Hardcode both monthly and quarterly columns side by side in the same Cube View

<details>
<summary>Show answer</summary>

**Correct answer: B)**

By binding the Cube View's column member list to a parameter that controls the time granularity, users can toggle between monthly and quarterly views dynamically. The parameter value drives which member list expression is used for columns. Separate dashboards (A) duplicate maintenance. Manual text entry (C) is error-prone. Hardcoding both (D) creates an unnecessarily wide and cluttered view.
</details>

---

### Question 21 (Hard)
**300.2.6** | Difficulty: Hard

A developer configures parameter interlinking between three dashboards in a chain: Dashboard A passes Entity to Dashboard B, and Dashboard B passes Account to Dashboard C. When navigating from A to B to C, Dashboard C receives the Account correctly but loses the Entity context originally selected in Dashboard A. How should this be fixed?

A) OneStream cannot support three-level dashboard navigation
B) Configure Dashboard B to also pass the Entity parameter through to Dashboard C, so that all required context is propagated across the full chain
C) Combine all three dashboards into a single dashboard
D) Remove the Account parameter from Dashboard C

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In multi-level interlinking, each dashboard must explicitly pass all required parameters to the next. If Dashboard B only passes Account to Dashboard C but not Entity, the Entity context is lost. The fix is to configure Dashboard B's interlink to also map and pass the Entity parameter to Dashboard C. OneStream supports multi-level navigation (A). Combining dashboards (C) or removing parameters (D) are not appropriate solutions.
</details>

---

### Question 22 (Easy)
**300.2.2** | Difficulty: Easy

What is the purpose of applying a sort order to a Bound List parameter's member list?

A) To determine the order in which the database processes transactions
B) To control the sequence in which members appear in the parameter's dropdown, making it easier for users to find values
C) To sort the cells within a Cube View automatically
D) To prioritize which server processes the request first

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Applying a sort order to a Bound List parameter controls how members appear in the dropdown list -- alphabetically, by hierarchy order, or by a custom sequence. This improves usability by helping users find the desired member quickly. It does not affect database processing (A), Cube View cell sorting (C), or server prioritization (D).
</details>

---

### Question 23 (Medium)
**300.2.3** | Difficulty: Medium

What is the difference between using `Children("TotalEntity")` and `Descendants("TotalEntity")` in a Member List expansion for a parameter?

A) There is no difference; both return the same members
B) `Children` returns only the immediate level-1 children, while `Descendants` returns all members at every level below TotalEntity
C) `Children` returns only leaf-level members, while `Descendants` returns only parent members
D) `Children` is used for Account dimension only, while `Descendants` is used for Entity dimension only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

`Children` returns only the immediate (level-1) children of the specified member, while `Descendants` returns all members at every level below the specified member (children, grandchildren, and so on). They are not equivalent (A), `Children` is not limited to leaf members (C), and both functions work across all dimensions (D).
</details>

---

### Question 24 (Hard)
**300.2.4** | Difficulty: Hard

A Dashboard Extender Business Rule is configured to populate a Bound List parameter. The rule executes successfully and returns a populated list, but the parameter dropdown appears empty on the dashboard. What is the most likely cause?

A) The OneStream server does not have enough disk space
B) The Business Rule is populating the result in the wrong return format or the parameter is not correctly bound to the Dashboard Extender rule's output method
C) Bound List parameters cannot be populated by Business Rules
D) The user does not have a valid email address configured

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If a Dashboard Extender rule executes successfully but the dropdown remains empty, the most likely issue is a mismatch between the rule's output format and what the parameter expects. The rule may be writing results to the wrong property, using an incorrect method signature, or the parameter's binding to the Extender output is misconfigured. Disk space (A) would cause broader errors, BRs can populate parameters (C), and email addresses (D) are unrelated.
</details>

---

### Question 25 (Medium)
**300.2.6** | Difficulty: Medium

When configuring parameter interlinking, what is the difference between passing a parameter value as a "fixed value" versus a "dynamic value"?

A) There is no difference; all values are treated the same
B) A fixed value passes a hardcoded constant regardless of user selection, while a dynamic value passes the current value of a source parameter based on the user's selection
C) Fixed values are stored in the database, while dynamic values are stored in memory only
D) Fixed values work only in production, while dynamic values work only in development

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When configuring interlinks, a fixed value always passes the same hardcoded constant to the target dashboard regardless of user interaction. A dynamic value passes the current runtime value of a source parameter, reflecting the user's actual selection. This distinction enables flexible navigation scenarios. They are not treated the same (A), storage location is not the differentiator (C), and both work in all environments (D).
</details>
