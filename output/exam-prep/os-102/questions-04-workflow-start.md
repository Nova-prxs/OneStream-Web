# Question Bank - Section 4: Workflow Start (11% of exam)

## Objectives
- **102.4.1:** Understand the structure and components of a Workflow
- **102.4.2:** Amend and configure Workflow profiles

---

## Objective 102.4.1: Workflow structure

### Question 1 (Easy)
**102.4.1** | Difficulty: Easy

What is a Workflow in OneStream?

A) A dashboard used for data visualization
B) A structured process that guides users through data collection, validation, and submission tasks for a specific period
C) A security role that restricts user access
D) A report generation tool

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Workflow in OneStream is a structured process that organizes and guides users through tasks such as data collection, data entry, validation, review, and submission for a specific time period and scenario. It provides a framework for managing the financial close or planning cycle.
</details>

---

### Question 2 (Easy)
**102.4.1** | Difficulty: Easy

Which of the following is the correct hierarchy of Workflow components from top to bottom?

A) Workflow > Scenario > Step > Entity
B) Scenario > Workflow > Entity > Step
C) Workflow Profile > Workflow > Scenario Type > Step
D) Step > Workflow > Scenario > Profile

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Workflow hierarchy flows from Workflow Profile (the overall configuration) to Workflow (the process definition) to Scenario Type (e.g., Actual, Budget) to Step (the individual tasks within the workflow). Entities are assigned at various levels to define which organizational units participate.
</details>

---

### Question 3 (Medium)
**102.4.1** | Difficulty: Medium

What is the purpose of a Workflow Step in OneStream?

A) To define the fiscal year calendar
B) To represent a specific task or phase within the Workflow, such as data entry, review, or certification
C) To configure Cube dimensions
D) To assign user security roles

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Workflow Step represents a specific task or phase in the overall Workflow process. Steps define what action needs to be performed (e.g., data import, data entry, review, approval, certification) and can have their own status tracking, assignments, and deadlines. Steps are executed in sequence as part of the Workflow.
</details>

---

### Question 4 (Medium)
**102.4.1** | Difficulty: Medium

Which Workflow component determines which Cube and Entity members are available for data processing?

A) Workflow Step
B) Workflow Profile
C) Scenario Type
D) Workflow Dashboard

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Profile is the configuration component that determines the Cube, Entity members, and other dimension intersections available for data processing within the Workflow. It defines the scope of what data can be accessed and manipulated through the Workflow.
</details>

---

### Question 5 (Hard)
**102.4.1** | Difficulty: Hard

An Administrator needs to set up a Workflow where some entities perform data entry while other entities only perform review and certification. How should this be configured?

A) Create separate Workflows for data entry entities and review entities
B) Create a single Workflow with multiple Steps, and use Workflow assignments to control which entities participate in which Steps
C) Use Cube Security to block data entry for review-only entities
D) Create separate Scenario Types for each entity group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach is to use a single Workflow with multiple Steps and leverage Workflow assignments to control entity participation at each Step level. For example, data entry Steps can be assigned to operational entities, while review and certification Steps can be assigned to corporate or parent entities. This keeps the process unified while allowing different participation levels.
</details>

---

## Objective 102.4.2: Amending Workflow profiles

### Question 6 (Easy)
**102.4.2** | Difficulty: Easy

Where does an Administrator navigate to create or modify a Workflow Profile?

A) Application tab > Workflow > Workflow Profiles
B) Cube tab > Cube Profiles
C) Dashboard tab > Workflow Dashboard
D) Security tab > Profile Management

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Workflow Profiles are created and managed under the Application tab in the Workflow section. This is where the Administrator defines the profile's Cube assignment, entity scope, scenario configuration, and the steps that make up the Workflow.
</details>

---

### Question 7 (Medium)
**102.4.2** | Difficulty: Medium

Which Workflow Profile Type should an Administrator use to enable importing data to Entity Members in the Cube?

A) No Input
B) Review
C) Base Input
D) Certification Only

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Base Input is the Workflow Profile Type that enables data to be imported or entered at the entity level in the Cube. This profile type allows data loading through import processes and manual data entry for the assigned entities. No Input and Review types do not permit data loading, and Certification Only is used for approval steps.
</details>

---

### Question 8 (Medium)
**102.4.2** | Difficulty: Medium

An Administrator modifies a Workflow Profile to add a new entity. What additional step is required for users to access the new entity in the Workflow?

A) Restart the application server
B) Recalculate the Cube
C) Ensure the user has security access to the new entity
D) Re-extract and reload the application

<details>
<summary>Show answer</summary>

**Correct answer: C)**

After adding a new entity to a Workflow Profile, the Administrator must also ensure that users have been granted security access to the entity. Without proper security permissions, users will not see the entity in their Workflow view even though it has been added to the profile.
</details>

---

### Question 9 (Hard)
**102.4.2** | Difficulty: Hard

An Administrator has a Workflow Profile configured for the "Actual" scenario with monthly periods. The business now requires adding a "Budget" scenario to the same Workflow. What is the correct approach?

A) Modify the existing Workflow Profile to change the scenario from Actual to Budget
B) Create a new Scenario Type within the existing Workflow and configure a new Workflow Profile for Budget with its own Steps and entity assignments
C) Create an entirely new application for the Budget scenario
D) Use UD dimensions to differentiate between Actual and Budget data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach is to add a new Scenario Type (Budget) within the existing Workflow structure and create a corresponding Workflow Profile with its own Steps, entity assignments, and configuration. This allows both Actual and Budget processes to coexist within the same Workflow while maintaining their own independent configurations and timelines.
</details>

---

### Question 10 (Hard)
**102.4.2** | Difficulty: Hard

When configuring a Workflow Profile, an Administrator sets the "Auto Start" property to True. What is the effect of this setting?

A) The Workflow automatically begins data calculations when the period opens
B) The Workflow Step automatically transitions to "Started" status when the period is opened, without requiring manual initiation
C) Data is automatically imported when the Workflow starts
D) Users are automatically notified via email when the Workflow starts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Auto Start is set to True on a Workflow Profile, the associated Workflow Steps automatically transition to "Started" status when the period is opened by an administrator. This eliminates the need for users to manually initiate the Workflow Step and allows them to begin working immediately when the period is available.
</details>

---

### Question 11 (Easy)
**102.4.1** | Difficulty: Easy

What is the first status of a Workflow Step when a new period is opened?

A) Started
B) Confirmed
C) Not Started
D) Certified

<details>
<summary>Show answer</summary>

**Correct answer: C)**

When a new period is opened, all Workflow Steps begin in the "Not Started" status by default. The Step must be explicitly started (either manually by a user or automatically via the Auto Start setting) before work can begin. This ensures that administrators control when each phase of the Workflow becomes active.
</details>

---

### Question 12 (Medium)
**102.4.1** | Difficulty: Medium

An Administrator creates a Workflow with three Steps: Import, Data Entry, and Review. Can these Steps be configured to require sequential completion?

A) No, all Steps are always available simultaneously
B) Yes, Step dependencies can be configured so that a subsequent Step only becomes available after the prior Step reaches a specified status
C) No, Steps always run in parallel
D) Yes, but only if each Step is in a separate Workflow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workflow Steps can be configured with dependencies so that a Step only becomes available when the preceding Step reaches a specified status (e.g., Confirmed or Certified). This enforces sequential processing, ensuring that data is imported before entry begins and entry is complete before review starts.
</details>

---

### Question 13 (Medium)
**102.4.2** | Difficulty: Medium

What is the difference between a "Base Input" and a "Parent Input" Workflow Profile Type?

A) There is no difference; they are interchangeable
B) Base Input allows data entry at leaf (base) entity members, while Parent Input allows data entry at parent (aggregate) entity members
C) Base Input is for financial data and Parent Input is for operational data
D) Base Input is used for imports only and Parent Input is used for manual entry only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Base Input allows data to be loaded or entered at base (leaf-level) entity members, which is the standard approach for most data collection. Parent Input allows data entry directly at parent entity members, which is useful for top-down planning or entering adjustment entries at consolidated levels. The choice depends on where in the entity hierarchy data needs to be captured.
</details>

---

### Question 14 (Hard)
**102.4.2** | Difficulty: Hard

An Administrator needs to configure a Workflow where the Budget scenario uses quarterly periods while the Actual scenario uses monthly periods. How should this be set up?

A) Create two separate applications, one for each scenario
B) Configure separate Workflow Profiles for each Scenario Type, specifying the appropriate period frequency (monthly for Actual, quarterly for Budget) within each profile
C) Change the application's time settings to quarterly
D) Use the same Workflow Profile and let users select the period frequency

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Each Scenario Type within a Workflow can have its own Workflow Profile with independent configuration, including period frequency. The Actual Scenario Type can be configured with monthly Workflow Profiles, while the Budget Scenario Type uses quarterly profiles. This allows both scenarios to coexist in the same Workflow with different time granularities.
</details>

---

### Question 15 (Easy)
**102.4.1** | Difficulty: Easy

Which Workflow component type represents the highest level in the Workflow hierarchy?

A) Workflow Step
B) Scenario Type
C) Workflow Profile
D) Entity assignment

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Workflow Profile is the highest-level component in the Workflow hierarchy. It encompasses the overall Workflow definition, which contains Scenario Types, which in turn contain Steps. The Profile defines the Cube, entity scope, and overall configuration that governs the entire Workflow process.
</details>

---

### Question 16 (Medium)
**102.4.2** | Difficulty: Medium

An Administrator wants to prevent users from starting a Workflow Step until a specific calendar date. Which Workflow configuration supports this?

A) Cube lock dates
B) Workflow Step open date (schedule start)
C) Application Properties time restrictions
D) Security group time-based permissions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workflow Steps can be configured with an open date (schedule start) that controls when the Step becomes available for user interaction. Before the open date, users cannot start or interact with the Step, even if they have the necessary security permissions. This feature is commonly used to coordinate the timing of the financial close or planning cycle.
</details>

---

### Question 17 (Hard)
**102.4.1** | Difficulty: Hard

An Administrator needs to set up a Workflow where corporate entities perform an intercompany elimination step that is not applicable to operational entities. What is the most appropriate configuration?

A) Create separate Workflows for corporate and operational entities
B) Add the elimination Step to the Workflow and use entity-level Step assignments so that only corporate entities are assigned to the elimination Step
C) Include the elimination Step for all entities and instruct operational entities to skip it
D) Use Cube Security to hide the elimination Step from operational entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most appropriate approach is to add the intercompany elimination Step to the Workflow and control participation through entity-level Step assignments. Only corporate or holding entities that perform eliminations are assigned to that Step, while operational entities are excluded. This maintains a single unified Workflow while accommodating different process requirements for different entity types.
</details>

---

### Question 18 (Medium)
**102.4.2** | Difficulty: Medium

When configuring a Workflow Profile, the Administrator selects a Cube. What does this Cube assignment determine?

A) Only which dashboard is displayed
B) Which multidimensional data store the Workflow reads from and writes to, including the available dimensions and members
C) Only the security permissions for the Workflow
D) The reporting currency for the Workflow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Cube assignment on a Workflow Profile determines which multidimensional data store the Workflow interacts with. This defines the available dimensions (Entity, Account, Scenario, UD1-UD8, etc.) and their members for data loading, data entry, calculations, and reporting within the Workflow. All data operations in the Workflow target the assigned Cube.
</details>

---

### Question 19 (Easy)
**102.4.2** | Difficulty: Easy

Which type of Workflow Profile should be assigned to entities that only need to review consolidated data without entering or importing data?

A) Base Input
B) No Input
C) Parent Input
D) Data Load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

No Input is the Workflow Profile Type used for entities that do not need to enter or import data. These entities participate in the Workflow for review, confirmation, and certification purposes only. This is common for parent or corporate entities whose data comes entirely from the aggregation of their children rather than direct input.
</details>

---

### Question 20 (Hard)
**102.4.2** | Difficulty: Hard

An Administrator has configured multiple Workflow Profiles for the same Scenario and notices that some entities appear duplicated in the Workflow view. What is the most likely cause?

A) The entities have been duplicated in the Entity dimension
B) The same entity has been assigned to multiple Workflow Profiles for the same Scenario, causing overlapping assignments
C) The Cube contains duplicate data
D) The Workflow dashboard has a display error

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When an entity is assigned to multiple Workflow Profiles for the same Scenario, it can appear multiple times in the Workflow view, once for each Profile assignment. The Administrator should review the Workflow Profile entity assignments to ensure each entity is assigned to only one Profile per Scenario, unless the design intentionally requires multiple profile assignments for different Cubes.
</details>
