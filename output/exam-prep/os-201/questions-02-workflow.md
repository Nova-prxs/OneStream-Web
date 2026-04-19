# Question Bank - Section 2: Workflow (14% of exam)

## Objective 201.2.1: Apply the appropriate Workflow Name for a given situation

### Question 1 (Easy)
**201.2.1** | Difficulty: Easy

What does the Workflow Name control in OneStream?

A) The visible name of the Cube in the interface
B) The tasks that users must complete in the Workflow
C) The security access to the Workflow
D) The type of Consolidation that runs

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Name controls the tasks that users must complete in the Workflow. It is configured in the Workflow Settings of each Input Child Profile, and the tasks can vary by Scenario and Input Type.
</details>

---

### Question 2 (Easy)
**201.2.1** | Difficulty: Easy

Which Workflow Name is used when you only need to import data to the Stage without loading it to the Cube?

A) Import, Validate, Load
B) Central Import
C) Import Stage Only
D) Workspace

<details>
<summary>Show answer</summary>

**Correct answer: C)**

"Import Stage Only" only imports data to the Stage without loading it to the Cube. It is useful when data needs to be reviewed in staging before proceeding with the definitive load.
</details>

---

### Question 3 (Medium)
**201.2.1** | Difficulty: Medium

**Scenario:** A Planning team needs the system to execute a calculation before displaying the data entry form, and upon completion the users must certify their work. What is the correct Workflow Name?

A) Form Input, Certify
B) Form Input, Process, Certify
C) Pre-Process, Form Input, Certify
D) Pre-Process, Form Input, Process, Confirm, Certify

<details>
<summary>Show answer</summary>

**Correct answer: C)**

"Pre-Process, Form Input, Certify" includes a Pre-Process step (a calculation that runs before displaying the form), followed by data entry and certification. Option D also includes Pre-Process but adds Process and Confirm steps that are not required in the scenario. "Pre-Process" is exclusive to Workflow Names for Forms.
</details>

---

### Question 4 (Medium)
**201.2.1** | Difficulty: Medium

What does the word "Central" indicate in a Workflow Name like "Central Import"?

A) That data is stored on a central server
B) That it is for centralized imports where corporate controls entities belonging to other areas
C) That it is used only for the central Entity in the hierarchy
D) That it is a system default Workflow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Central" in the name indicates it is for corporate use, controlling entities that belong to other areas. Central versions exist for Import, Form Input, and Journal Input. Corporate can make final adjustments after data has been loaded by subsidiaries.
</details>

---

### Question 5 (Medium)
**201.2.1** | Difficulty: Medium

How many Workflow Name options exist for each type of Input Child?

A) Import: 10, Forms: 11, Journals: 7
B) Import: 7, Forms: 7, Journals: 7
C) Import: 5, Forms: 8, Journals: 4
D) Import: 11, Forms: 10, Journals: 7

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Import has 10 Workflow Name options, Forms has 11 options, and Journals has 7 options. Additionally, Review Profiles have 2 options (Process, Certify and Process, Confirm, Certify). Each type has specific names tailored to its needs.
</details>

---

### Question 6 (Hard)
**201.2.1** | Difficulty: Hard

**Scenario:** A company needs its subsidiaries to load data, validate it, run Process Cube, go through Data Quality validation with Confirmation Rules, and finally certify. What is the most appropriate Import Workflow Name?

A) Import, Validate, Load, Certify
B) Import, Validate, Process, Certify
C) Import, Validate, Process, Confirm, Certify
D) Import, Verify, Certify Stage Only

<details>
<summary>Show answer</summary>

**Correct answer: C)**

"Import, Validate, Process, Confirm, Certify" is the complete process that includes all required phases: import, validation of mappings/intersections, Process Cube (Calculate/Consolidate), Confirmation (Data Quality with Confirmation Rules), and Certification. Option B omits the Confirm step needed for Data Quality.
</details>

---

### Question 7 (Hard)
**201.2.1** | Difficulty: Hard

**Scenario:** The implementation team wants to use a custom Dashboard as the Workflow interface for a Journal Input process that requires certification. What Workflow Name and additional property must they configure?

A) Journal Input, Certify with Dashboard Profile Name
B) Workspace, Certify with Workspace Dashboard Name
C) Central Journal Input with Process Cube Dashboard Profile Name
D) Workspace with Confirmation Profile Name

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To use a custom Dashboard as the Workflow interface with certification, you must select the Workflow Name "Workspace, Certify" and then configure the "Workspace Dashboard Name" property to define which Dashboard from Application Dashboards will serve as the interface. This property only works with Workflow Names that contain "Workspace".
</details>

---

### Question 8 (Hard)
**201.2.1** | Difficulty: Hard

What are the available Workflow Names for Review Profiles, and why are they limited?

A) Only "Process, Certify" and "Process, Confirm, Certify" because Review Profiles do not receive data directly
B) The same as Import because they inherit their configurations
C) Only "Workspace" because reviewers use Dashboards
D) "Certify" and "Process" independently

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Review Profiles only have 2 Workflow Names: "Process, Certify" and "Process, Confirm, Certify". This limitation exists because Review Profiles do not receive data; they serve only as checkpoints for reviewing, confirming, and certifying data already loaded by other Profiles.
</details>

---

## Objective 201.2.2: Apply the appropriate Workflow Profile settings

### Question 9 (Easy)
**201.2.2** | Difficulty: Easy

Which type of Workflow Profile CANNOT have Calculation Definitions?

A) Review Profile
B) Base Input Profile
C) Cube Root Profile
D) Input Child Profile

<details>
<summary>Show answer</summary>

**Correct answer: C)**

All Workflow Profile types can have Calculation Definitions except the Cube Root Profile. Calculation Definitions are configured on the second tab of the Workflow Profile.
</details>

---

### Question 10 (Easy)
**201.2.2** | Difficulty: Easy

What happens when an Input Child Profile does not have an explicit Calculation Definition?

A) Process Cube cannot be executed
B) It inherits the Calculation Definitions from the Input Parent
C) It uses a system default Calculation Definition
D) It generates a configuration error

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If an Input Child Profile does not have an explicit Calculation Definition, it inherits those from the Input Parent. This inheritance simplifies configuration when multiple Input Children need to execute the same calculations.
</details>

---

### Question 11 (Medium)
**201.2.2** | Difficulty: Medium

What is the purpose of the "Confirmed Switch" in a Calculation Definition?

A) It confirms that the Calculation Definition is active
B) It determines whether the Entities defined should be subjected to the Confirmation Workflow Step
C) It activates the Force Calculate mode
D) It validates that FX Rates are correct

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Confirmed Switch determines whether the Entities defined in the Calculation Definition should be subjected to the Confirmation Workflow Step. This gives the application designer control over which Entities are validated by Confirmation Rules during the Workflow process.
</details>

---

### Question 12 (Medium)
**201.2.2** | Difficulty: Medium

**Scenario:** An architect needs to execute a Data Management Sequence during Process Cube in the Workflow. How should the Calculation Definition be configured?

A) Assign the Sequence directly as the Calc Type
B) Configure Calc Type = No Calculate, put the Sequence name in Filter Value, and create a DataQualityEventHandler Business Rule
C) Create a Business Rule that executes the Sequence and assign it to the Cube
D) It is not possible to execute Data Management from the Workflow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To execute a Data Management Sequence from Process Cube: (1) set Calc Type = No Calculate, (2) put the Sequence name in the Filter Value, and (3) create a DataQualityEventHandler Extensibility Business Rule that reads the Sequence name and executes it during Process Cube. "No Calculate" allows assigning Data Management Sequences without direct calculation.
</details>

---

### Question 13 (Medium)
**201.2.2** | Difficulty: Medium

What is the correct Entity placeholder variable for a Review Profile that needs to include Entities from Named Dependents?

A) Assigned Entities
B) Loaded Entities
C) Dependent Entities with Named Dependent Filter
D) User Cell Input Entities

<details>
<summary>Show answer</summary>

**Correct answer: C)**

For Review Profiles, the available variables are "Dependent Entities" (all Entities from dependent Workflow Profiles, including Named Dependents) and "Named Dependent Filter" (to filter specific Entities from Named Dependents). "Assigned Entities", "Loaded Entities", and "User Cell Input Entities" are for Input Parent and Input Child Profiles.
</details>

---

### Question 14 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** During a monthly consolidation, an administrator notices that Force Consolidate takes significantly longer than normal Consolidate. In which situation is using Force Consolidate justified over normal Consolidate?

A) Always, because it is more thorough
B) When FX Rates or Member Formulas were changed after the last calculation, since the Calculation Status does not reflect these changes
C) Only when there are data errors
D) Whenever users request it

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Force Consolidate processes ALL Data Units without checking Calculation Status, making it more expensive. It is only justified when: (1) FX Rates or Member Formulas were changed after the last calculation (these changes do not automatically update Calculation Status), or (2) Data Units are extremely large where checking calc status is costlier than simply reprocessing. In most cases, normal Consolidate is more efficient because it only processes what is necessary.
</details>

---

### Question 15 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** A company has Input Parent and Input Child Profiles. The Input Parent has Calculation Definitions with "Assigned Entities" as the placeholder. Within the same profile, there is an Import Child without its own Calculation Definitions, and a Forms Child with its own Calculation Definition using "Loaded Entities". What Entities are calculated when Process Cube is executed from each channel?

A) Import Child calculates Assigned Entities; Forms Child calculates Loaded Entities
B) Import Child calculates Loaded Entities; Forms Child calculates Assigned Entities
C) Both calculate Assigned Entities because they inherit from the Parent
D) Both calculate Loaded Entities

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The Import Child does not have its own Calculation Definition, so it inherits from the Input Parent and calculates "Assigned Entities". The Forms Child has its own Calculation Definition, so it uses "Loaded Entities" as explicitly configured. Inheritance only applies when there is no explicit definition on the Input Child.
</details>

---

### Question 16 (Hard)
**201.2.2** | Difficulty: Hard

What are the 4 Entity placeholder variables available for Input Parent and Input Child Profiles?

A) Dependent Entities, Named Dependent Filter, Assigned Entities, Loaded Entities
B) Assigned Entities, Loaded Entities, Journal Input Entities, User Cell Input Entities
C) All Entities, Base Entities, Parent Entities, Top Entity
D) Profile Entities, Cube Entities, Workflow Entities, System Entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The 4 Entity placeholder variables for Input Parent and Input Child are: (1) Assigned Entities – Entities directly assigned to the Workflow Profile, (2) Loaded Entities – Entities imported by Import Child Workflow Profiles, (3) Journal Input Entities – Entities with journal adjustments, and (4) User Cell Input Entities – Entities affected by data entry performed by the user executing the Workflow (for Central Input). "Dependent Entities" and "Named Dependent Filter" are exclusive to Review Profiles.
</details>

---

### Question 17 (Easy)
**201.2.2** | Difficulty: Easy

How many types of Workflow Profile exist in OneStream?

A) 4
B) 6
C) 8
D) 10

<details>
<summary>Show answer</summary>

**Correct answer: C)**

There are 8 types of Workflow Profile: Cube Root, Review, Default Input, Parent Input, Base Input, Import Child, Forms Child, and Adjustment Child.
</details>

---

### Question 18 (Easy)
**201.2.2** | Difficulty: Easy

Which type of Workflow Profile is the most common and serves as the "workhorse" of the system?

A) Cube Root Profile
B) Review Profile
C) Base Input Profile
D) Default Input Profile

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Base Input Profile is the most common type (workhorse). It controls all data entry methods for Base Entities and has the 3 types of Input Children: Import, Forms, and Journals.
</details>

---

### Question 19 (Medium)
**201.2.2** | Difficulty: Medium

What is the relationship between Input Child Profiles and Origin Members?

A) There is no direct relationship
B) Import Child maps to O#Import, Forms Child maps to O#Forms, Adjustment Child maps to O#AdjInput
C) All Input Children map to O#Input
D) The relationship depends on the Cube Type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Input Child Profiles map directly to Origin Members: Import Child to O#Import, Forms Child to O#Forms, and Adjustment Child to O#AdjInput. This relationship is fixed and allows each data entry channel to have its own tracking in the Origin Dimension.
</details>

---

### Question 20 (Medium)
**201.2.2** | Difficulty: Medium

**Scenario:** An administrator needs to ensure separation of duties in the Journal process. What security properties must be configured?

A) Access Group and Maintenance Group
B) Prevent Self-Post and Prevent Self-Approval
C) Workflow Execution Group only
D) Certification SignOff Group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Prevent Self-Post" (True prevents users from posting their own journals, with options True, True includes Admins, and False) and "Prevent Self-Approval" (True prevents users from approving their own journals) are the properties specifically designed to ensure separation of duties in the Journal process.
</details>

---

### Question 21 (Medium)
**201.2.2** | Difficulty: Medium

What happens to Entities that are not explicitly assigned to any Workflow Profile?

A) They generate a configuration error
B) They are automatically assigned to the Default Input Profile
C) They do not participate in the Workflow
D) They are assigned to the Cube Root Profile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Entities not explicitly assigned to other Profiles are implicitly assigned to the Default Input Profile. This Profile is automatically created with the Cube Root and cannot be deleted. That is why it is recommended to build a "Do Not Use" structure for the Default Workflow Profile.
</details>

---

### Question 22 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** A Parent Input Profile was just created. When reviewing its configuration, the administrator observes that the Import Child was created automatically but is inactive. Why does this happen and what input channels are available?

A) It is a system error; the Import Child should be active
B) It is by design: Parent Input does not allow Import (Profile Active = False forced). Only Forms and Journals are allowed
C) The Import Child is inactive because no Data Source is configured
D) All Input Children are inactive by default in any Profile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is by design. In a Parent Input Profile, the Import Child is automatically created but is forced to be inactive (Profile Active = False) because Parent Input Profiles only allow adjustments to Parent Entities via Forms or AdjInput (Journals), not data import.
</details>

---

### Question 23 (Hard)
**201.2.2** | Difficulty: Hard

What is the difference between the "Open" and "Closed" state of the Workflow in the Cube Root Profile?

A) Open allows editing and Closed is read-only, with no performance difference
B) Open: Workflow available, accessed from cache. Closed: snapshot stored in a historical table, accessed from the database with a performance penalty
C) Open means active and Closed means deleted
D) There is no functional difference; it is just a label

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Open: Workflow is available, locking is controlled at the individual Workflow Profile level, accessed from memory (cache). Closed: High-level lock, snapshot of the hierarchy stored in a historical audit table, accessed from the database (performance penalty). Workflows should only be closed for major changes or discontinued operations, never for general use.
</details>

---

### Question 24 (Easy)
**201.2.2** | Difficulty: Easy

What are Named Dependents and in which type of Workflow Profile are they used?

A) They are types of Entities and are used in Base Input Profiles
B) They are dependencies on Input Parent Profiles that are not direct descendants and are used exclusively in Review Profiles
C) They are Business Rules and are used in Cube Root Profiles
D) They are security filters and are used in all Profiles

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Named Dependents allow a Review Profile to establish a dependency on Input Parent Profiles that are not its direct descendants in the Workflow hierarchy. They are exclusive to Review Profiles and are necessary when Shared Services loads data for entities that belong to different Review Profiles.
</details>

---

### Question 25 (Easy)
**201.2.2** | Difficulty: Easy

What are the 3 types of Data Locking in the Workflow?

A) Explicit Lock, Implicit Lock, Workflow Only Lock
B) Full Lock, Partial Lock, No Lock
C) Entity Lock, Scenario Lock, Time Lock
D) Read Lock, Write Lock, Admin Lock

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The 3 types of Data Locking are: Explicit Lock (created when a Workflow Unit is locked), Implicit Lock (created when the Parent Workflow has been certified; cleared by un-certifying), and Workflow Only Lock (if the Workflow Profile has no assigned Entities, only Workflow processing is blocked).
</details>

---

### Question 26 (Medium)
**201.2.2** | Difficulty: Medium

**Scenario:** Two Import Child Profiles (siblings) load data to the same Data Unit. How does OneStream handle this situation?

A) It generates a conflict error
B) The last one to execute overwrites the first one's data
C) It automatically clears prior data and reloads both using the accumulate method
D) It only allows one of the two to execute

<details>
<summary>Show answer</summary>

**Correct answer: C)**

When two Import siblings load to the same Data Unit, OneStream automatically handles clear and merge of data: it clears prior data and reloads both using the accumulate method. If there is no overlap between the Data Units, it uses standard clear and replace. This functionality allows multiple source systems to feed the same Entities.
</details>

---

### Question 27 (Medium)
**201.2.2** | Difficulty: Medium

What happens when "Load Overlapped Siblings" is set to False?

A) Loading data from sibling Import Children is prevented
B) No overlap check is performed between sibling channels; if overlap exists, the last channel processed overwrites. This improves performance in parallel processing when there is no overlap
C) Sequential processing of siblings is forced
D) The automatic merge functionality is removed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

With "Load Overlapped Siblings = False", no overlap check is performed between sibling channels. If overlap exists, the last channel processed overwrites. This improves performance in parallel processing when it can be guaranteed that there is no overlap between the Data Units of each Import sibling.
</details>

---

### Question 28 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An administrator needs to load YTD data for Balance Accounts and Periodic data for Flow Accounts in the same Workflow Profile. What Integration Settings must be adjusted?

A) Create two separate Import Children
B) Configure Flow Type No Data Zero View Override = Periodic, Balance Type No Data Zero View Override = YTD, and Force Balance Accounts to YTD View = True
C) Change the Default View of the Scenario to Periodic
D) Use a Data Management Step to transform the views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To load YTD and Periodic data in the same Profile, configure in the Integration Settings of the Input Child: Flow Type No Data Zero View Override = Periodic, Balance Type No Data Zero View Override = YTD, and Force Balance Accounts to YTD View = True. This allows each Account type to use the correct view without needing separate profiles.
</details>

---

### Question 29 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** A Workflow Template was applied to 10 Base Input Profiles. Subsequently, an existing Import Child configuration is modified in the template. What happens to the 10 Profiles?

A) All are automatically updated
B) Changes to existing input types in the template are NOT propagated to already-created Profiles. Only new Input Types can be applied later
C) An alert is generated for the administrator to update manually
D) The Profiles are invalidated and must be recreated

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is an important limitation of Workflow Templates: after a template is applied, changes to existing input types in the template are NOT propagated to the Profile. Only new Input Types added to the template can be applied later. Administrators must update each Profile individually or use Grid View for bulk changes.
</details>

---

### Question 30 (Hard)
**201.2.2** | Difficulty: Hard

What is the correct hierarchy of Data Locking granularity in the Workflow, from highest to lowest?

A) Workflow Channel Lock > Origin Lock > Input Parent Lock
B) Input Parent Lock (Level 1) > Input Child Origin Lock > Input Child Workflow Channel Lock
C) Entity Lock > Account Lock > Cell Lock
D) Cube Lock > Scenario Lock > Time Lock

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The hierarchy of Data Locking granularity from highest to lowest is: Input Parent (Level 1) which locks all cells for assigned Entities (highest level), then Input Child Origin Lock which locks by Origin Member (Import, Forms, AdjInput), and finally Input Child Workflow Channel Lock which locks by Workflow Channel + Origin (finest granularity). OneStream uses bidirectional control between the Workflow Engine, Staging Engine, and Analytic Engine.
</details>

---

### Question 31 (Easy)
**201.2.1** | Difficulty: Easy

Which Workflow Name is available for the Import channel when the standard data load process of importing, validating maps, and loading to the Cube is required without any additional steps?

A) Import Stage Only
B) Import, Validate, Load
C) Import, Validate, Process, Certify
D) Central Import

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Import, Validate, Load" is the basic Workflow Name for the Import channel. It provides three sequential tasks: import data into the Stage engine, validate the transformation maps and intersections, and load the cleansed data from the Stage into the Cube.
</details>

---

### Question 32 (Easy)
**201.2.1** | Difficulty: Easy

What is the primary purpose of the Workflow Engine in OneStream?

A) To manage only the Consolidation process
B) To protect the end user from Analytic Model complexities and coordinate all end user activities
C) To store data in the SQL database
D) To manage only security permissions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Engine is the overall system manager coordinating all end user activities while guaranteeing the quality and transparency of source data. Its primary responsibilities include protecting the end user from Analytic Model complexities, managing data collection, enforcing the data quality and certification process, and coordinating the consolidation process.
</details>

---

### Question 33 (Easy)
**201.2.1** | Difficulty: Easy

Before a Workflow hierarchy can be created, what must exist in the application?

A) At least one Scenario must be defined
B) At least one Cube marked as "Is Top Level Cube For Workflow = True"
C) At least one Data Source must be configured
D) At least one Security Group must be created

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Before a Workflow hierarchy can be created, at least one Cube marked as "Is Top Level Cube For Workflow = True" must exist in the application. The primary reason Workflow exists is to care for and feed Analytic Cubes.
</details>

---

### Question 34 (Easy)
**201.2.1** | Difficulty: Easy

What Workflow Name options are available for the Journal (Adjustment) Input Child?

A) Journal Input; Journal Input, Certify; Journal Input, Process, Certify; Journal Input, Process, Confirm, Certify; Central Journal Input; Workspace; Workspace, Certify
B) Import, Validate, Load; Journal Input, Certify
C) Only Journal Input and Central Journal Input
D) Form Input, Process, Certify; Journal Input

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The Journal Input Child has 7 Workflow Name options: (1) Journal Input, (2) Journal Input, Certify, (3) Journal Input, Process, Certify, (4) Journal Input, Process, Confirm, Certify, (5) Central Journal Input, (6) Workspace, and (7) Workspace, Certify.
</details>

---

### Question 35 (Easy)
**201.2.2** | Difficulty: Easy

What is the role of the Access Group security property on a Workflow Profile?

A) It controls which users can execute Workflow tasks
B) It controls which users can view results in the Workflow Profile at run time
C) It controls which users can certify the Workflow
D) It controls which users can delete the Workflow Profile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Access Group controls the user or users that will have access to the Workflow Profile at run time to view results. The Workflow Execution Group controls who can execute Workflow, and the Certification SignOff Group controls who can certify.
</details>

---

### Question 36 (Easy)
**201.2.2** | Difficulty: Easy

What property on a Workflow Profile must be set to True to allow a Workflow Channel to be completed without loading data?

A) Can Load Unrelated Entities
B) Is Optional Data Load
C) Load Overlapped Siblings
D) Profile Active

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When "Is Optional Data Load" is set to True, the Workflow Channel receives an additional icon called "Complete Workflow" that allows users to complete the process if no data is loaded to that channel for a particular period.
</details>

---

### Question 37 (Easy)
**201.2.2** | Difficulty: Easy

What is the purpose of a Workflow Suffix Group?

A) It adds a suffix to the Entity name for reporting
B) It allows a single Cube to have multiple Workflow hierarchies based on different business processes
C) It changes the name displayed to end users
D) It controls the calculation order

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workflow Suffix Groups enable a single Cube to support multiple Workflow hierarchies. Suffix values are assigned to Scenario Types to identify different business processes (e.g., Actuals, Planning, Other). This allows the Workflow Engine to manage separate collection hierarchies for the same Cube.
</details>

---

### Question 38 (Medium)
**201.2.1** | Difficulty: Medium

**Scenario:** An organization uses a Forms-based planning process. They need users to enter data, run calculations, pass Data Quality checks with Confirmation Rules, and certify. Which Workflow Name for the Forms channel is the most appropriate?

A) Form Input, Certify
B) Form Input, Process, Certify
C) Form Input, Process, Confirm, Certify
D) Pre-Process, Form Input, Certify

<details>
<summary>Show answer</summary>

**Correct answer: C)**

"Form Input, Process, Confirm, Certify" includes all the required steps: data entry via Forms, Process Cube (calculations), Confirmation (Data Quality checks with Confirmation Rules), and Certification. Option B omits the Confirm step needed for the Data Quality validation.
</details>

---

### Question 39 (Medium)
**201.2.1** | Difficulty: Medium

What is the difference between "Import, Verify Stage Only" and "Import Stage Only"?

A) There is no difference; they are the same Workflow Name
B) "Import, Verify Stage Only" adds a verification step for transformation maps and intersections before keeping data in Stage, while "Import Stage Only" simply imports to Stage
C) "Import, Verify Stage Only" loads data to the Cube after verifying
D) "Import Stage Only" includes a certification step

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Import, Verify Stage Only" adds a verification (Validate) step to check transformation maps and intersections before keeping data in Stage only, without loading to the Cube. "Import Stage Only" simply imports data into Stage without any validation step. Both leave data in the Stage without loading to the Cube.
</details>

---

### Question 40 (Medium)
**201.2.2** | Difficulty: Medium

What is the purpose of the Workflow Execution Group security property?

A) It controls which users can view data in Cube Views
B) It allows users to execute Workflow tasks and, when locking, the authorization applies to all descendant Workflow Profiles
C) It controls which users can create new Workflow Profiles
D) It allows users to delete Workflow Profiles

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Execution Group is configured for data loaders and allows users to execute Workflow. When locking a Workflow Profile, the Execution Group authorization applies to all descendant Workflow Profiles regardless of the Access Group or Workflow Execution Group assigned to the descendants.
</details>

---

### Question 41 (Medium)
**201.2.2** | Difficulty: Medium

What is the purpose of the Certification SignOff Group on a Workflow Profile?

A) It controls who can import data
B) It is configured for certifiers and allows users to sign off on the Workflow, enabling separation of duties between data loaders and certifiers
C) It controls who can view certification questions
D) It automatically certifies the Workflow when data is loaded

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Certification SignOff Group is configured for certifiers and allows users to sign off on the Workflow. This group can be used to separate duties between a data loader (Workflow Execution Group) and a certifier (Certification SignOff Group).
</details>

---

### Question 42 (Medium)
**201.2.2** | Difficulty: Medium

What is the "Require Journal Template" property used for on a Workflow Profile?

A) It forces users to create journals only using Excel templates
B) It restricts users from creating free-form journals by disabling the Create Journals button, enforcing journal creation from existing templates only
C) It requires all journals to be approved before posting
D) It makes journal templates visible to all users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When set to True, "Require Journal Template" restricts users from creating new free-form journals by disabling the Create Journals button. It also prevents users from loading journal files that do not contain a journal template. System and Application administrators can still create free-form journals. This property works in combination with the Journal Process Group.
</details>

---

### Question 43 (Medium)
**201.2.2** | Difficulty: Medium

Which member filter expression returns all Entities associated with the selected Workflow Unit?

A) E#Root.WFCalculationEntities
B) E#Root.WFConfirmationEntities
C) E#Root.WFProfileEntities
D) E#Root.WFDependentEntities

<details>
<summary>Show answer</summary>

**Correct answer: C)**

E#Root.WFProfileEntities, when used in a Member Filter, returns all Entities associated with the selected Workflow Unit. E#Root.WFCalculationEntities returns Entities defined in Calculation Definitions, and E#Root.WFConfirmationEntities returns Entities where the Confirmed Switch is True.
</details>

---

### Question 44 (Medium)
**201.2.2** | Difficulty: Medium

**Scenario:** An application needs multiple Form data entry channels assigned to different groups of people for a budget process. How can this be accomplished?

A) Create multiple Cubes, each with different Forms
B) Create multiple Forms Child Input Profiles under the same Input Parent, each with different access groups
C) Create multiple Scenarios for each group
D) This is not possible in OneStream

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An application can have multiple input Workflow Profiles of the same type (Import, Forms, Journals). This allows multiple form groups to be completed by different groups of people, as different access groups are created for each. Users only see and work on what they have access to.
</details>

---

### Question 45 (Medium)
**201.2.2** | Difficulty: Medium

What are the three predefined default Workflow Channels that exist when building an application?

A) Import, Forms, Adjustments
B) Standard, NoDataLock, AllChannelInput
C) Level1, Level2, Level3
D) Primary, Secondary, Tertiary

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three predefined default Workflow Channels are: Standard (basic default channel for Account Members and Input Children), NoDataLock (for metadata Members that should not participate in a Workflow Channel grouping scheme), and AllChannelInput (indicates the Workflow Profile can control data input for any Workflow Channel).
</details>

---

### Question 46 (Medium)
**201.2.2** | Difficulty: Medium

What is the difference between the "Can Load Unrelated Entities" property set to True versus False?

A) True allows loading data to all Cubes; False restricts to one Cube
B) True allows a Workflow Profile to load data to entities not assigned to its Input Parent; False restricts loading to only assigned entities
C) True enables parallel loading; False enables sequential loading
D) True allows loading historical data; False only allows current period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When set to True, a Workflow Profile can load data to entities that are not assigned to its Input Parent Workflow Profile, which is typically used for historical data loading or Central Input scenarios. When set to False (default), loading is restricted to assigned entities only. A journal can be saved with unassigned entities but cannot be submitted, approved, or posted.
</details>

---

### Question 47 (Medium)
**201.2.2** | Difficulty: Medium

What happens when an Explicit Lock is placed on an Input Parent Workflow Profile?

A) Only the Import channel is locked
B) All data cells for the Entities assigned to the Input Parent are locked, including all Input Child Profiles regardless of Origin Member or Workflow Channel
C) Only the Forms channel is locked
D) Only Workflow processing is blocked, but data can still be entered

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When an Input Parent Workflow Profile is locked, this is a Level One Data Unit Lock — the highest level of locking. The Workflow Engine forces all data cells for the Entities assigned to the Input Parent to be locked by locking the Input Parent as well as all Input Child Profiles regardless of Origin Member binding or Workflow Channel assignment.
</details>

---

### Question 48 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** A cost center budget collection process has many users submitting data to a single Cube Data Unit. Users submit data for different cost centers to the same accounts. What design approach should be used to prevent data contention?

A) Create a separate Cube for each cost center
B) Assign the User Defined Dimension containing cost centers as the Workflow Channel dimension, allowing each entity/cost center to act as a granular and autonomous cell collection
C) Create separate Scenarios for each cost center
D) Use parallel processing with no additional configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Assigning the User Defined Dimension containing cost centers as the Workflow Channel dimension makes each entity/cost center act as a granular and autonomous cell collection. This allows individual cost centers to load, clear, and lock data with no impact to other cost centers. Only one User Defined Dimension per application can be designated for Workflow Channels.
</details>

---

### Question 49 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An architect is designing a Workflow for a large organization with 1,000 entities loading concurrently. What are the key considerations for optimizing data loading performance?

A) Use a single Workflow Profile for all entities and load sequentially
B) Use parallel processing by partitioning data by entity, keep files manageable (around 1 million rows per location), and load time periods in sequential order
C) Increase the SQL server memory and load all data at once
D) Use only Direct Load for all imports

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For optimal performance: partition data by entity to maximize parallel processing, keep files around 1 million rows per location, and load time periods in sequential order (January, then February, etc.). The inherent relationship between Workflow and Entity aligns through the data structures. Parallel processing is the primary way to improve data loading performance into the analytic model.
</details>

---

### Question 50 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** A company wants to use Workflow Channels with both Accounts and a User Defined Dimension simultaneously. An incompatibility arises between the Workflow Channel assigned to the Workflow Profile and those assigned to both dimension types. What are the two solutions?

A) Create separate applications for each dimension
B) Either assign AllChannelInput to the Workflow Profile's Workflow Channel (locking reverts to Origin level) or assign NoDataLock to one of the dimensions to remove it from the Workflow Channel process
C) Use two User Defined Dimensions for Workflow Channels
D) Remove all Workflow Channel assignments

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When both Accounts and a User Defined Dimension use Workflow Channel tagging, two solutions exist: (1) Assign AllChannelInput to the Workflow Profile, which lets it function generically but reverts locking to the Origin Member level. (2) Assign NoDataLock to either the Account or UD dimension, removing it from the Workflow Channel process and allowing it to function with any Workflow Profile regardless of channel assignment.
</details>

---

### Question 51 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** A Central HR Load Workflow Profile is configured to load data for entities owned by the Houston Workflow Profile. Houston is currently certified and locked. What happens when Central HR Load attempts to load data?

A) Central HR Load successfully loads the data because it has higher priority
B) The Workflow Engine disallows the update because Houston is certified or locked and Central HR Load must abide by the workflow and locking status of the profiles that own the entities
C) The data is loaded to a temporary staging area
D) An administrator must manually approve the load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Central HR Load does not control any entities, so it checks and abides by the workflow and locking status of the Workflow Profiles that own the entities. If Houston is certified or locked and Central HR Load tries to load an entity owned by Houston, the Workflow Engine will disallow the update, ensuring data integrity of the certified data.
</details>

---

### Question 52 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An organization needs to set up Confirmation Rules that run only quarterly during the Workflow. What Frequency setting should be selected for these rules?

A) All Time Periods
B) Monthly
C) Quarterly
D) Member Filter

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Quarterly frequency setting runs the Confirmation Rules every quarter, or four times a year. Other available frequencies include: All Time Periods (every period), Monthly (every month), Half Yearly (twice a year in June and December), Yearly (once a year in December), and Member Filter (for custom time-based filtering).
</details>

---

### Question 53 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An architect needs to design Workflow Profiles that support Account-level phasing so that Trial Balance accounts, Corporate Controlled accounts, and Statistical accounts can be locked independently. What approach should be used?

A) Create three separate Cubes
B) Create Workflow Channels for each group, tag each Account Member with the correct Workflow Channel, and tag each Input Child Profile with the Workflow Channel it should control
C) Use three separate Scenarios
D) Create three Review Profiles

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Account Phasing using Workflow Channels requires three steps: (1) Create Workflow Channels representing the groups of Accounts (e.g., Trial Balance, Corporate Controlled, Statistical). (2) Tag each Account Member with the proper group. (3) Tag each Workflow Profile Input Profile with the Workflow Channel it should control. This hard-wires the Workflow Profile to control clearing, loading, and locking behaviors of the associated Account groups.
</details>

---

## Objective 201.2.3: Demonstrate an understanding of workflows from the user perspective (OnePlace)

### Question 54 (Easy)
**201.2.3** | Difficulty: Easy

In OnePlace Workflow, what are the three components a user selects at the top of the Workflow slider to configure their Workflow View?

A) Cube, Entity, Account
B) Workflow Profile, Scenario, and Year
C) Time, Origin, Consolidation
D) Security Group, Dashboard, Report

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The top of the Workflow slider configures the Workflow View for data loading by allowing users to select three items: Workflow Profile, Scenario, and Year. A Workflow Unit is an individual period within the selected year and Scenario combination for a particular Workflow Profile.
</details>

---

### Question 55 (Easy)
**201.2.3** | Difficulty: Easy

How many Workflow Units (periods) are always displayed for selection in the OnePlace Workflow view?

A) 4
B) 6
C) 12
D) It depends on the Scenario configuration

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Twelve Workflow Units or periods are always displayed for selection, but only one can be selected and active at a given time. Each Workflow Origin step available must be completed, as well as the collective data confirmation and certification process, for a single Workflow process to be finalized.
</details>

---

### Question 56 (Easy)
**201.2.3** | Difficulty: Easy

During the Import step in OnePlace Workflow, what happens after a user successfully imports and transforms a source file?

A) The data is immediately loaded to the Cube
B) The file is parsed into a clean tabular format in the Stage engine, and the Import task changes from blue to green
C) The Workflow automatically certifies the data
D) The user must manually create transformation rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

After a user clicks Import and the data is loaded successfully, the system imports the data into the Stage engine. The file is parsed into a clean tabular format with information on the Amounts, Source ID, and each Dimension. Once loaded successfully, the Import task changes from blue to green.
</details>

---

### Question 57 (Easy)
**201.2.3** | Difficulty: Easy

What are the four Load Method options available when clicking "Load and Transform" in the Import step?

A) Clear, Insert, Update, Delete
B) Replace, Replace (All Time), Replace Background (All Time, All Source IDs), Append
C) Overwrite, Merge, Accumulate, Skip
D) Full, Partial, Incremental, Delta

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four Load Method options are: Replace (clears and replaces data for the specific Source ID), Replace (All Time) (replaces all Workflow Units in the selected view), Replace Background (All Time, All Source IDs) (replaces all Workflow Units and all Source IDs in background), and Append (adds rows not included in the previous file load without changing existing data).
</details>

---

### Question 58 (Easy)
**201.2.3** | Difficulty: Easy

What does the Validate step check in the OnePlace Workflow?

A) Only the data types of the imported values
B) First, that each piece of data has a map, and second, that dimension combinations can be loaded into the Cube based on constraints
C) Only the file format
D) Only the security permissions of the user

<details>
<summary>Show answer</summary>

**Correct answer: B)**

During the Validate step, two specific actions happen. First, OneStream checks to make sure each piece of data has a map (Transformation Rule). Next, OneStream checks to make sure the combination of Dimensions can be loaded into the Cube based on the constraints defined in the system (e.g., Intercompany constraints).
</details>

---

### Question 59 (Medium)
**201.2.3** | Difficulty: Medium

In the OnePlace Workflow Validate step, what are the two types of errors that can occur and how are they different?

A) System errors and user errors
B) Transformation Errors (unmapped data) and Intersection Errors (invalid dimension combinations)
C) Import errors and Export errors
D) Calculation errors and Consolidation errors

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Two types of errors can occur: Transformation Errors, where data is unmapped (e.g., a new account added but not mapped), showing the Source Value and unassigned Target Value; and Intersection Errors, where the entire intersection of data is invalid (e.g., a Customer Dimension mapped to a Salary Grade Account). Users can fix Transformation Errors by assigning the correct target, and Intersection Errors by investigating each dimension's target value.
</details>

---

### Question 60 (Medium)
**201.2.3** | Difficulty: Medium

In the Confirm step of the OnePlace Workflow, what are the two types of statuses that Confirmation Rules can return?

A) Pass and Fail
B) Warning and Error
C) Green and Red
D) Active and Inactive

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The two statuses for Confirmation Rules are: Warning (the user is outside the threshold, but it will not stop the process — the user can still proceed) and Error (the user is outside the threshold, the process is stopped, and the step turns red). If anything has failed, previous steps must be revisited to fix the data before proceeding.
</details>

---

### Question 61 (Medium)
**201.2.3** | Difficulty: Medium

During the Certify step in OnePlace Workflow, what are the two options available for completing certification?

A) Quick Certify and Full Certify
B) Answering questionnaires and setting certification status, OR using the one-click Quick Certify option
C) Manual certification and Automatic certification
D) Self-certification and Manager certification

<details>
<summary>Show answer</summary>

**Correct answer: B)**

There are two ways to certify: (1) Answer certification questionnaires by clicking each set of questions, responding, adding comments, setting the questionnaire status to Completed, then clicking Set Certification Status to Certify. (2) Use the one-click Quick Certify option that expedites the process without requiring questions to be answered. Both give the final green check mark.
</details>

---

### Question 62 (Medium)
**201.2.3** | Difficulty: Medium

What information does the "Dependent Status" view display in the Certify step?

A) Only the list of dependent entities
B) The Workflow Profile name, all input types, Workflow Channel, status of each input type, last step completed, percentage of OK/In Process/Not Started/Error steps, and last activity timestamp
C) Only the certification questions
D) Only the Intercompany matching status

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Dependent Status view displays comprehensive information: the Workflow Profile name and all input types, the Workflow Channel, the status of each input type, the last step completed for each, the percentage of each step that is OK, In Process, Not Started, and with errors, and a record of when the last activity took place for each step.
</details>

---

### Question 63 (Medium)
**201.2.3** | Difficulty: Medium

In OnePlace Workflow, what is Multi-Period Processing and how is it accessed?

A) It allows users to process data for a single period with multiple Scenarios
B) It allows users to perform multiple workflow tasks for one to many time periods, accessed by clicking on the Year in the Navigation Pane
C) It automatically processes all periods sequentially
D) It is only available for administrators

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Multi-Period Processing allows users to perform multiple workflow tasks for one to many time periods simultaneously. It is accessed by clicking on the Year in the Navigation Pane of the Workflow. From there, users can select and process multiple periods at once.
</details>

---

### Question 64 (Medium)
**201.2.3** | Difficulty: Medium

What right-click options are available for a Workflow month in OnePlace Workflow?

A) Only Lock and Unlock
B) Status and Assigned Entities, Audit Workflow Process, Lock/Unlock Descendants, Edit Transformation Rules, Clear Import/Forms Data, Corporate Certification Management, Corporate Data Control, and Workflow View Selection
C) Only Clear Data and Recalculate
D) Only Export and Import

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Right-click options include: Status and Assigned Entities (Workflow Status of each Origin), Audit Workflow Process (audit trail with dates, users, duration, errors), Lock/Unlock Descendants, Edit Transformation Rules, Clear All Import/Forms Data From Cube, Corporate Certification Management (unlock/uncertify ancestors or lock/certify descendants), Corporate Data Control (preserve and restore data), and Workflow View Selection.
</details>

---

### Question 65 (Medium)
**201.2.3** | Difficulty: Medium

In the OnePlace Workflow Forms step, what is the difference between Required and Optional Forms?

A) Required Forms are locked and Optional Forms are editable
B) Required Forms must be completed before the user can move on from the Forms step; Optional Forms do not block progression
C) Required Forms are created by administrators; Optional Forms are created by users
D) There is no functional difference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In the Forms step, there are two categories: Required and Optional. Required Forms must be completed before the end user can move on from the Forms step. Optional Forms can be filled out but do not block the user from progressing to the next step. Once all required Forms are completed, the user clicks "Complete Workflow."
</details>

---

### Question 66 (Medium)
**201.2.3** | Difficulty: Medium

What does the "Replace (All Time)" Load Method do compared to the standard "Replace" method?

A) It replaces data for all Cubes in the application
B) It replaces all Workflow Units in the selected Workflow View, forcing a replace of all time values in a multi-period workflow view
C) It replaces data for all Scenarios
D) It replaces data and also deletes the source file

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Replace (All Time)" replaces all Workflow Units in the selected Workflow View when working with multi-period data. It forces a replace of all time values, unlike the standard "Replace" which only clears and replaces data for the specific Source ID in the current period.
</details>

---

### Question 67 (Hard)
**201.2.3** | Difficulty: Hard

**Scenario:** An organization wants to implement a fully automated "lights-out" data loading process that runs overnight. What combination of OneStream components should be used, and what design considerations are important?

A) Only Data Management Sequences are needed
B) A combination of Workflow, Data Management, Connector Business Rules, and Data Integration, with considerations for error handling, mapping issues, and future growth
C) Only the Batch File Loading feature
D) A custom external application that bypasses Workflow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An automated lights-out process requires a combination of Workflow, Data Management, Connector Business Rules, and Data Integration. Important considerations include: planning for mapping issue handling since no one monitors the load; storing data loading errors for next-day analysis; planning for future acquisitions and growth; using connectors in a lights-out fashion. The process still uses the same mechanics as manual processing, including full auditability and drilldown functionality.
</details>

---

### Question 68 (Hard)
**201.2.3** | Difficulty: Hard

**Scenario:** An administrator needs to set up Batch File Loading for automated processing. What is the correct file naming format for batch files?

A) ProfileName_Scenario_Time.csv
B) FileID-ProfileName-ScenarioName-TimeName-LoadMethod.txt (e.g., aTrialBalance-Houston;Import-Actual-2011M1-R.txt)
C) ProfileName-Scenario-Time.xlsx
D) Any naming convention can be used

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The batch file naming format is: FileID-ProfileName-ScenarioName-TimeName-LoadMethod.txt. For example: aTrialBalance-Houston;Import-Actual-2011M1-R.txt. The semicolon (;) is used to delimit Parent and Child Profile names. The Load Method uses R for Replace and A for Append. Special substitution variables C (current) and G (global) can be used for Scenario and Time.
</details>

---

### Question 69 (Hard)
**201.2.3** | Difficulty: Hard

**Scenario:** A large retail company has a data file of approximately 6 million rows. The implementation team needs to optimize the data loading process. What design approach should be considered?

A) Load the entire file through a single Workflow Profile
B) Break the file by entity into multiple Workflow Profiles, group larger entities into their own workflows, group smaller ones together, and use parallel batch loading with the BRApi.Utilities.ExecuteFileHarvestBatchParallel function
C) Use only the Direct Load method for the entire file
D) Split the file by account

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For large files (6+ million rows), the design should break data by entity into multiple Workflow Profiles — larger entities get their own workflows, and smaller ones are grouped together. Then automated batch loading is used with the BRApi.Utilities.ExecuteFileHarvestBatchParallel function, testing the optimal number of parallel processes (starting low and increasing until server limits are reached). This maximizes throughput through parallel processing.
</details>

---

### Question 70 (Hard)
**201.2.3** | Difficulty: Hard

**Scenario:** A company is implementing Intercompany Matching within the Workflow. A user notices a red button in the status column of the IC Matching section. What information is available when investigating, and what actions can the user take?

A) Only the total Intercompany variance amount
B) Each Intercompany counterparty with variances, currency details (Reporting, Entity, Partner), and right-click options to Set IC Transaction Status, Show Partner Workflow Status, Show/Hide Dimension Details, and Drill Down
C) Only the ability to clear Intercompany data
D) Only an error message requiring administrator intervention

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When investigating, users can see each Intercompany counterparty with variances, details including Reporting Currency, Entity Currency, and Partner Currency. Right-click options include: Set IC Transaction Status (Not Started, Loaded, Adjusting, Disputed, Finalized with comments), Show Partner Workflow Status, Show/Hide Dimension Details, and Drill Down for more information.
</details>

---

### Question 71 (Hard)
**201.2.3** | Difficulty: Hard

**Scenario:** A Workflow is configured to use Journals with full security separation of duties. When "Prevent Self-Post" is set to "True (includes Admins)", what specific behavior changes occur?

A) Only the Quick Post button is removed
B) Administrators and members of the Journal Post security group cannot post their own journals; the Quick Post action is replaced with Submit, Approve, and Post action buttons for administrators and all members of Journal Process, Approval, and Post groups
C) Only non-administrators are prevented from posting
D) All journal posting is disabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When "Prevent Self-Post" is set to "True (includes Admins)", both administrators and members of the Journal Post security group cannot post their own journals. The Quick Post action is replaced with the Submit, Approve, and Post actions if you are an administrator or a member of all three groups: Journal Process Group, Journal Approval Group, and Journal Post Group. The Post button is disabled for journals where the user is the Created User, Submitted User, or Approved User.
</details>

---

### Question 72 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An organization is implementing a planning solution using Workspaces. The planning process is circular — users input drivers, calculate costs, review reports, change drivers, recalculate, and repeat before certifying. What is the recommended Workflow approach?

A) Use the standard linear Workflow with Import, Validate, Load, Process, Confirm, Certify
B) Use a Workspace Workflow Name with a custom Dashboard that provides data entry screens, reports, and calculation buttons all on the same screen
C) Create separate Workflow Profiles for each iteration
D) Use only Forms without any Workflow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For circular planning processes, a Workspace Workflow Name with a custom Dashboard is recommended. Dashboards give users a "web page" experience where data entry screens, reports, and buttons to execute tasks are all on the same screen. This contrasts with the linear Workflow design (Import > Validate > Load) used for Actuals/consolidation processes.
</details>

---

### Question 73 (Easy)
**201.2.2** | Difficulty: Easy

What happens when a new Cube Root Profile is created?

A) A Review Profile is automatically created
B) A matching Default Input Profile is automatically created, prefixed with the Cube Root Profile name
C) All Entity Members are automatically assigned
D) Calculation Definitions are automatically generated

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a new Cube Root Profile is created, the Workflow Engine automatically creates a matching Default Input Profile that is prefixed with the Cube Root Profile name (e.g., Corporate_Default). A Default Input Profile is required for each Workflow hierarchy and cannot be deleted.
</details>

---

### Question 74 (Medium)
**201.2.2** | Difficulty: Medium

What are the three primary Workflow Stage Import Methods in OneStream?

A) File Import, Database Import, Excel Import
B) Standard, Direct, and Blend
C) Manual, Automatic, and Batch
D) Real-time, Scheduled, and On-demand

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three primary methods are: Standard (highly durable and auditable, stored details targeting Finance Engine Cubes), Direct (in-memory, performance-focused, no storage of record details, targeting Finance Engine Cubes), and Blend (in-memory, high-performance import designed to blend multi-dimensional structure with transactional data, targeting external relational tables).
</details>

---

### Question 75 (Medium)
**201.2.3** | Difficulty: Medium

What does the Process Cube step accomplish during the Workflow, and what options does it perform?

A) It only runs Calculate
B) It performs No Calculate plus all standard Calculate, Translate, and Consolidate options using Calculation Definitions
C) It only loads data to the Cube
D) It only runs Translate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Process Cube step performs No Calculate and all standard Calculate, Translate, and Consolidate options using Calculation Definitions. Each added line item can be filtered by entity for reviewer-level processes. No Calculate can be used to allow the assignment of Data Management Sequences.
</details>

---

### Question 76 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An architect is choosing between Standard Import and Direct Load Import for a planning solution. What are the key limitations of Direct Load that must be considered?

A) Direct Load is slower than Standard Import
B) Direct Load does not store source and target records in Stage tables, does not support Drill-Down, does not support Re-Transform, is limited to 1000 error records per load, and cannot load data beyond the current Workflow period
C) Direct Load cannot handle files larger than 100MB
D) Direct Load requires a custom Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Direct Load limitations include: no source/target record storage in Stage tables, no Drill-Down from Finance Engine, no Re-Transform capability (data must be re-loaded), Transformation and Validation errors limited to 1000 records per load (with 50,000 total error storage limit), import files cannot contain data beyond the current Workflow period, and no Append load method. It is not appropriate where historical audit or detailed transformation analysis is required.
</details>

---

### Question 77 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** Two Import Base Input Workflow Profiles share the same entities — one imports a Trial Balance and one imports Supplemental data. Each uses a different Workflow Channel. The Trial Balance Workflow Profile is locked. What happens when the Supplemental Workflow Profile attempts to load data?

A) The load is blocked because the Trial Balance is locked
B) Since the Workflow Channels are separate, only the Supplemental data is written to the Cube; the data goes into its own set of cells with no overlap possible, so the locked Trial Balance Profile is not affected
C) Both data sets are merged and reloaded
D) An administrator must unlock the Trial Balance first

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Workflow Channels are separate (one per Import Child), the data for the selected Workflow Profile is written to the Cube independently. The data goes into its own set of cells, so it does not need to consider the other Workflow's locked status. The Workflow Channel guarantees that Trial Balance and Supplemental data can never overlap.
</details>

---

### Question 78 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An architect needs to design a Workflow structure for a Tier 1 automotive supplier with hundreds of global locations. The structure initially had many review hierarchy levels but was found to be too rigid for locking/certifying. What lesson should be applied?

A) Add more hierarchy levels for better control
B) Flatten the list to make locking cleaner, remove unnecessary hierarchy steps that exist only for visual structure, and ensure surgical locking so that data loading is locked while reviewers stay open
C) Remove all Review Profiles
D) Use only one Workflow Profile for all entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The lesson learned is to flatten the Workflow hierarchy by removing unnecessary hierarchy steps that existed only for visual structure without adding value. Locking needs to be performed surgically — locking data loading while keeping reviewers open. Overly rigid hierarchies cause problems when users are reluctant to certify because it automatically locks the workflow, requiring administrator intervention for any last-minute changes.
</details>

---

### Question 79 (Medium)
**201.2.2** | Difficulty: Medium

What is the recommended row limit per Workflow for summarized records, and what is the absolute maximum?

A) Recommended: 100,000; Maximum: 1 million
B) Recommended: 1 million summarized records; Maximum: 24 million summarized records
C) Recommended: 500,000; Maximum: 10 million
D) There is no limit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best-practice recommendation is 1 million summarized records per Workflow, while the absolute row limit per Workflow is 24 million summarized records. If a single Workflow would result in 24 million records, the best-practice solution is to parse the file across 24 partitioned Workflows to gain parallel processing performance benefits.
</details>

---

### Question 80 (Hard)
**201.2.2** | Difficulty: Hard

**Scenario:** An architect is designing a security model for the Workflow. What is the relationship between the Workflow Execution Group and the ability to lock/unlock Workflow Profiles?

A) Workflow Execution Group only controls data loading
B) To lock a Workflow Profile, you need Workflow Execution Group membership or inherited membership from an ancestor; to unlock as a non-administrator, you need both Workflow Execution Group membership and the Application Security Role UnlockWorkflowUnit
C) Only administrators can lock or unlock
D) Any user with Access Group membership can lock and unlock

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To lock a Workflow Profile, you need Workflow Execution Group membership or inherited Workflow Execution Group membership from an ancestor Workflow Profile. The Execution Group authorization applies to all descendant Profiles. To unlock as a non-administrator, you must have both membership in the Workflow Execution Group AND the Application Security Role UnlockWorkflowUnit.
</details>

---

### Question 81 (Hard)
**201.2.1** | Difficulty: Hard

**Scenario:** An organization is loading data from two different ERP systems to the same entities during the same period. They want to avoid data conflicts. What Workflow design should be used?

A) Use a single Import Child Profile for both ERPs
B) Create two Import Child sibling Profiles under the same Input Parent with different Data Sources; the Workflow Engine will automatically clear and merge using the accumulate method when overlapping Data Units are detected
C) Use separate Scenarios for each ERP
D) Load the data manually through Forms

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Creating two Import Child sibling Profiles under the same Input Parent allows multiple source systems to feed the same Entities. When either import executes the Load step, the engine checks for overlapping Data Units between siblings. If overlap exists, it clears all previously loaded data units for both siblings and reloads both using an accumulate load method. If no overlap, standard clear and replace is used.
</details>

---

### Question 82 (Medium)
**201.2.3** | Difficulty: Medium

In the OnePlace Workflow, what is the Analysis Pane and when is it available?

A) It is only available during the Import step
B) It is available in each Workflow Profile for a defined period under the Status Pane, particularly during Process, Confirm, and Certify tasks, displaying Cube Views and Dashboards for analysis
C) It is only visible to administrators
D) It replaces the Navigation Pane

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Analysis Pane is available in each Workflow Profile for a defined period under the Status Pane. It appears during Process, Confirm, and Certify tasks, where data can be viewed and analyzed in Cube Views and Dashboards. Users can also view the Cube Calculation Status by clicking on Cube Views | Calculation Status.
</details>

---

### Question 83 (Medium)
**201.2.3** | Difficulty: Medium

In the Journals step, what are the available journal actions and what statuses allow deletion of journals?

A) Only Submit and Post; journals cannot be deleted
B) Create, Create Using Template, Create Using Excel/CSV, Copy, Quick Post, Submit, Approve, Post; journals in Working or Rejected status can be deleted
C) Only Quick Post; all journals can be deleted
D) Only Post and Certify; journals in any status can be deleted

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Available actions include: Create Journal, Create Journal Using Template, Create Using Excel/CSV, Copy Selected Journals, Quick Post, Submit, Approve, Post, Delete, and Reapply Template Settings. Journals in Working or Rejected status can be deleted. Multiple journal entries in the same state can be selected for batch operations (Submit, Post, Approve, Quick Post, or Delete).
</details>

---

### Question 84 (Hard)
**201.2.3** | Difficulty: Hard

**Scenario:** An organization wants to implement extensible workflows for a combined consolidation and planning implementation. The consolidation process requires strict linear workflows with certification, while the planning process needs flexible Workspace-based workflows. How should the Workflow be designed?

A) Create one Workflow structure that serves both processes
B) Use Workflow Suffix Groups to create separate Workflow hierarchies by Scenario Type — a hierarchical structure with Review Profiles and certification for Actuals, and a flat structure using Workspace Workflow Names with custom Dashboards for Planning Scenario Types
C) Use the same Workflow Names for both processes
D) Only implement the consolidation workflow and let planners use Excel

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workflow Suffix Groups allow a single Cube to have multiple Workflow hierarchies based on Scenario Type. The consolidation (Actuals) hierarchy uses strict linear Workflows with Import, Validate, Load, Process, Confirm, and Certify steps along with Review Profiles. The planning hierarchy uses Workspace Workflow Names with custom Dashboards for a more flexible, circular user experience. Workflow Profile structures can vary completely between Scenario Types while sharing the same Cube, leveraging extensible workflows.
</details>

---
