# Question Bank - Section 7: Completing the Workflow (10% of exam)

## Objectives
- **102.7.1:** Understand how to process, confirm, and certify data within a Workflow
- **102.7.2:** Use workflow administration tools for managing workflow lifecycle

---

## Objective 102.7.1: Process, confirm, and certify data

### Question 1 (Easy)
**102.7.1** | Difficulty: Easy

What is the correct order of Workflow status progression for a typical data submission process?

A) Certified > Confirmed > Started > Not Started
B) Not Started > Started > Confirmed > Certified
C) Started > Not Started > Certified > Confirmed
D) Confirmed > Started > Certified > Not Started

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The standard Workflow status progression follows a sequential order: Not Started (initial state), Started (work in progress), Confirmed (data has been reviewed and confirmed by the preparer), and Certified (final approval by a reviewer or manager). Each status transition represents a step forward in the data submission lifecycle.
</details>

---

### Question 2 (Easy)
**102.7.1** | Difficulty: Easy

What does "Confirming" a Workflow Step signify?

A) The data has been deleted
B) The user has reviewed and verified that the data entry for that Step is complete and accurate
C) The Workflow has been permanently closed
D) The data has been exported to a report

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Confirming a Workflow Step signifies that the user (typically the data preparer) has reviewed the entered data and verified that it is complete and accurate. This transitions the Step from "Started" to "Confirmed" status, indicating it is ready for the next level of review or certification.
</details>

---

### Question 3 (Medium)
**102.7.1** | Difficulty: Medium

What happens to data entry capabilities when a Workflow Step is Certified?

A) Data entry remains fully available
B) Data entry is locked; no further changes can be made without first un-certifying the Step
C) Only administrators can enter data
D) Data is automatically archived and removed from the Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a Workflow Step is Certified, data entry is locked for that Step's entities and the associated dimension intersections. No further changes can be made unless an administrator or authorized user un-certifies (reopens) the Step, returning it to a prior status. This ensures data integrity after the approval process.
</details>

---

### Question 4 (Medium)
**102.7.1** | Difficulty: Medium

An Administrator wants to lock and certify all steps in a Workflow for all periods, using a single batch operation. Which screen should the Administrator visit to do this?

A) Application Properties > General Tab
B) Workflow Multi-Period Processing
C) Cube Properties > Lock Settings
D) Dimension Library > Time Dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workflow Multi-Period Processing allows administrators to perform batch operations across multiple periods and Workflow Steps simultaneously. This includes certifying, locking, or changing the status of all Steps across multiple periods in a single operation, rather than processing each period and step individually.
</details>

---

### Question 5 (Hard)
**102.7.1** | Difficulty: Hard

A Workflow Step has been Certified, but a data error is discovered that requires correction. What is the correct procedure to allow the correction?

A) Delete the Workflow and recreate it
B) The administrator must un-certify the Step (moving it back to Confirmed or Started status), the user corrects the data, and then the Step must be re-confirmed and re-certified
C) Edit the data directly in the database, bypassing the Workflow
D) Create a new Scenario to enter the corrected data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct procedure is for an administrator to un-certify the Step, which moves it back to a prior status (Confirmed or Started), allowing data modifications. After the correction is made, the Step must go through the confirmation and certification process again. This maintains the audit trail and ensures proper review of all changes. Direct database edits bypass controls and are never recommended.
</details>

---

## Objective 102.7.2: Workflow administration tools

### Question 6 (Easy)
**102.7.2** | Difficulty: Easy

Which tool allows an Administrator to view the current status of all entities within a Workflow Step?

A) Cube Viewer
B) Workflow Status dashboard / Workflow Monitor
C) Application Log
D) Dimension Browser

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Status dashboard (also called the Workflow Monitor) provides administrators with an overview of all entities and their current status within each Workflow Step. This tool shows which entities are Not Started, Started, Confirmed, or Certified, enabling administrators to track progress and identify bottlenecks.
</details>

---

### Question 7 (Medium)
**102.7.2** | Difficulty: Medium

An Administrator needs to reopen a Certified Workflow Step for a single entity without affecting other entities. Is this possible?

A) No, un-certifying affects all entities in the Step
B) Yes, the Administrator can un-certify at the individual entity level within the Workflow Step
C) No, the entire Workflow must be restarted
D) Yes, but only by deleting and recreating the entity

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream allows administrators to manage Workflow status at the individual entity level. An Administrator can un-certify a specific entity within a Workflow Step without affecting the status of other entities. This granular control is essential for making targeted corrections without disrupting the entire Workflow process.
</details>

---

### Question 8 (Medium)
**102.7.2** | Difficulty: Medium

What is the purpose of Workflow Signoff in OneStream?

A) To digitally sign exported reports
B) To provide an electronic signature and audit trail when a user confirms or certifies a Workflow Step
C) To encrypt the data in the Cube
D) To authorize new user accounts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workflow Signoff provides an electronic signature and audit trail when users confirm or certify Workflow Steps. It records who performed the action, when it was performed, and can include comments. This supports compliance requirements and provides accountability in the data submission process.
</details>

---

### Question 9 (Hard)
**102.7.2** | Difficulty: Hard

An Administrator notices that a parent entity shows "Not Started" status even though all its child entities have been Certified. What is the most likely reason?

A) There is a system error that needs to be reported
B) The parent entity has its own Workflow Step assignment and must be independently started, confirmed, and certified
C) Child entity certifications automatically roll up to the parent
D) The parent entity needs to be recalculated first

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In OneStream, parent entities can have their own Workflow Step assignments that are independent of their children. The parent entity's status does not automatically change based on child entity statuses. The parent must be independently started, reviewed, confirmed, and certified. This allows for a separate review and approval layer at the consolidated level.
</details>

---

### Question 10 (Hard)
**102.7.2** | Difficulty: Hard

An Administrator wants to generate a report showing which entities have not yet completed their Workflow Steps by the deadline. Which combination of tools provides this information?

A) Cube Viewer and Dimension Browser
B) Workflow Status Monitor filtered by status, combined with the Step deadline configuration
C) Application Properties and the Server Console
D) The Solution Exchange and One Community

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Status Monitor can be filtered to show entities by their current status (e.g., showing only "Not Started" or "Started" entities) and compared against the Step deadline configuration. This combination allows administrators to identify which entities are behind schedule and take appropriate action, such as sending reminders or escalating to management.
</details>

---

### Question 11 (Easy)
**102.7.1** | Difficulty: Easy

What is the difference between "Confirmed" and "Certified" Workflow statuses?

A) They are the same status with different names
B) Confirmed indicates the data preparer has verified the data is complete; Certified indicates a reviewer or manager has approved the data
C) Confirmed means data is locked; Certified means data is deleted
D) Confirmed is for Actual data; Certified is for Budget data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Confirmed and Certified represent two distinct levels of approval in the Workflow. Confirmed typically indicates that the data preparer has reviewed and verified the data entry is complete and accurate. Certified represents a higher-level approval where a reviewer or manager has signed off on the data, effectively locking it from further changes.
</details>

---

### Question 12 (Medium)
**102.7.2** | Difficulty: Medium

An Administrator needs to open a new fiscal period for all Workflow entities. Where is this action performed?

A) In the Cube Properties panel
B) In the Workflow administration area by opening the period, which makes the Workflow Steps available for the new period
C) In the Dimension Library by adding a new Time member
D) In Application Properties > General Tab

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Opening a new fiscal period for Workflow processing is performed in the Workflow administration area. The Administrator opens the period, which initializes the Workflow Steps (setting them to "Not Started" or "Started" based on configuration) and makes them available for data loading, entry, and processing for the new period.
</details>

---

### Question 13 (Medium)
**102.7.1** | Difficulty: Medium

Can a Workflow Step be configured to skip the Confirmed status and go directly from Started to Certified?

A) No, all status transitions must follow the standard sequence
B) Yes, depending on the Workflow configuration, Steps can be set up to allow direct certification without requiring confirmation first
C) Only for parent entities
D) Only if the Step has no data entry

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream Workflow configurations can be set up to allow flexibility in status transitions. Depending on the process requirements, a Step can be configured to allow direct certification from the Started status, bypassing the Confirmed stage. This is useful for simplified workflows where a separate confirmation step is not needed.
</details>

---

### Question 14 (Hard)
**102.7.2** | Difficulty: Hard

An Administrator performs a bulk un-certify operation across all entities for a given period to allow data corrections. What risk should the Administrator be aware of?

A) There is no risk; un-certifying is always safe
B) Un-certifying reopens all entities for data modification, potentially allowing unintended changes; the Administrator should communicate the reopening to all stakeholders and re-certify promptly after corrections are made
C) Un-certifying deletes all data for the period
D) Un-certifying permanently removes the audit trail

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Bulk un-certifying reopens all entities for modification, which means any user with write access could potentially make unintended changes during the window when the Workflow is reopened. The Administrator should communicate the reason and timeline to all stakeholders, limit the reopening window, and ensure the re-certification process is completed promptly. The audit trail is preserved and records the un-certification action.
</details>

---

### Question 15 (Easy)
**102.7.2** | Difficulty: Easy

What information does the Workflow audit trail capture when a user certifies a Step?

A) Only the date of certification
B) The user who performed the action, the date and time, and any comments provided
C) Only the entity name
D) Only whether the data was changed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow audit trail captures comprehensive information for each status transition, including who performed the action (user name), when it occurred (date and time), and any comments the user provided during the signoff. This information supports compliance, accountability, and audit requirements.
</details>

---

### Question 16 (Medium)
**102.7.1** | Difficulty: Medium

What is the purpose of the "Lock" feature in Workflow administration, as distinct from Certification?

A) Lock and Certification are identical features
B) Lock prevents any data modifications at a system level for specified periods and entities, providing a harder restriction than Certification which can be reversed through un-certification
C) Lock only applies to dashboards
D) Lock deletes data from the Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Lock feature provides a system-level data protection that prevents any data modifications for specified periods and entities. While Certification can be reversed through an un-certify action, locking adds an additional layer of protection. A locked period typically requires administrator intervention to unlock and is used after the financial close is finalized to prevent any further changes.
</details>

---

### Question 17 (Hard)
**102.7.2** | Difficulty: Hard

An Administrator is managing a Workflow with 200 entities across 5 Steps. Ten entities are behind schedule and have not completed the Data Entry Step. What is the most efficient administrative action?

A) Individually contact each of the 10 entities' users
B) Use the Workflow Status Monitor to identify the specific entities, then use Workflow administration tools to send notifications or escalate, and optionally set a deadline extension
C) Certify all entities regardless of completion status
D) Delete the incomplete entities from the Workflow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most efficient approach is to use the Workflow Status Monitor to filter and identify the 10 entities that are behind schedule, then use the Workflow administration tools to take action. This may include sending notifications to the responsible users, escalating to management, or adjusting the Step deadline if business requirements allow. Certifying incomplete entities would compromise data integrity.
</details>

---

### Question 18 (Medium)
**102.7.1** | Difficulty: Medium

After all entities in a Workflow Step have been Certified, what typically happens next in the financial close process?

A) The data is automatically deleted
B) The Administrator proceeds with consolidation, intercompany eliminations, or other post-close processes, and may lock the period to prevent further changes
C) The Workflow automatically resets to Not Started
D) Users must re-enter all data for verification

<details>
<summary>Show answer</summary>

**Correct answer: B)**

After all entities are Certified, the financial close process typically continues with consolidation runs, intercompany eliminations, currency translations, and other post-close procedures. Once these are complete and validated, the Administrator may lock the period to provide a hard protection against further modifications, finalizing the close for that period.
</details>

---

### Question 19 (Easy)
**102.7.2** | Difficulty: Easy

Can an end user (non-administrator) un-certify a Workflow Step?

A) Yes, any user can un-certify any Step
B) Typically no; un-certification is restricted to administrators or users with specific elevated permissions
C) Yes, but only during business hours
D) Yes, if they certified it originally

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Un-certification is typically restricted to administrators or users who have been granted specific elevated permissions for Workflow management. Regular end users cannot un-certify Steps because this would bypass the approval controls that Certification is designed to enforce. This restriction maintains the integrity of the approval process.
</details>

---

### Question 20 (Hard)
**102.7.2** | Difficulty: Hard

An Administrator needs to close the current fiscal year and ensure no further changes can be made to any period in that year. What is the recommended sequence of actions?

A) Delete all Workflow Profiles for the year
B) Verify all periods are Certified across all entities and Steps, run final consolidations, lock all periods for the year, and archive the period data if applicable
C) Change the application's fiscal year settings
D) Remove user access to the application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended year-end close sequence is: first verify that all periods have completed the Workflow process with all entities Certified across all Steps; then run final consolidation processes, currency translations, and any year-end adjustments; next lock all periods for the year to prevent further modifications; and finally archive period data if the organization's data retention policy requires it. This ensures a clean, auditable year-end close.
</details>
