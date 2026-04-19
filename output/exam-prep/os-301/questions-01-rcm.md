# Question Bank - Section 1: Reconciliation Control Manager - RCM (32% of exam)

## Objectives
- **301.1.1:** Identify settings configurations for Reconciliation Control Manager (RCM)
- **301.1.2:** Identify options on the RCM Reconciliation Administration page
- **301.1.3:** Identify components of the RCM Reconciliation and Reporting page

---

## Objective 301.1.1: Identify settings configurations for RCM

### Question 1 (Easy)
**301.1.1** | Difficulty: Easy

In OneStream Financial Close, what is the primary purpose of the Reconciliation Control Manager (RCM)?

A) To consolidate financial data across entities
B) To manage and track account reconciliations throughout the close process
C) To generate tax compliance reports
D) To process intercompany eliminations

<details>
<summary>Show answer</summary>

**Correct answer: B)**

RCM is specifically designed to manage and track account reconciliations throughout the financial close process. It provides a centralized framework for assigning, completing, reviewing, and certifying reconciliations. Options A, C, and D describe other OneStream capabilities not specific to RCM.
</details>

---

### Question 2 (Easy)
**301.1.1** | Difficulty: Easy

Which of the following is configured in the RCM Global Setup?

A) Transaction matching tolerance rules
B) Default reconciliation templates, attribute mappings, and control list settings
C) Intercompany elimination rules
D) Cube consolidation methods

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The RCM Global Setup is where administrators configure default reconciliation templates, attribute mappings, control list settings, and other foundational settings that govern how reconciliations behave across the application. Transaction matching tolerance rules (A) belong to TXM, while C and D are unrelated consolidation features.
</details>

---

### Question 3 (Medium)
**301.1.1** | Difficulty: Medium

When configuring RCM column settings, which statement is TRUE about the relationship between column definitions and reconciliation templates?

A) Column settings are fixed system defaults that cannot be modified
B) Each reconciliation template can define its own set of visible columns independent of global settings
C) Column settings are only applied at the entity level and cannot be set globally
D) Columns must be identical across all reconciliation templates in an application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In RCM, reconciliation templates can define their own set of visible columns, allowing different reconciliation types to display relevant information. This flexibility enables organizations to tailor views for different reconciliation purposes (e.g., balance sheet vs. bank reconciliations) while still maintaining global defaults that can be overridden at the template level.
</details>

---

### Question 4 (Medium)
**301.1.1** | Difficulty: Medium

What is the purpose of dynamic attribute mapping in RCM settings?

A) To automatically assign security roles based on user department
B) To map dimension members to reconciliation attributes such as preparer, reviewer, and risk rating
C) To dynamically create new cube dimensions during reconciliation
D) To synchronize RCM data with external ERP systems in real time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dynamic attribute mapping allows administrators to map dimension members (such as accounts or entities) to reconciliation attributes like preparer, reviewer, risk rating, reconciliation type, and template assignment. This enables automatic assignment of reconciliation properties based on the dimensional intersection, reducing manual configuration effort.
</details>

---

### Question 5 (Hard)
**301.1.1** | Difficulty: Hard

An administrator needs to configure RCM so that high-risk accounts require dual certification while low-risk accounts require only a single reviewer. Which combination of RCM settings achieves this?

A) Create separate applications for high-risk and low-risk accounts with different workflow configurations
B) Use reconciliation templates with different certification levels and assign them via attribute mapping based on risk classification
C) Configure a single template and manually override the certification requirement for each individual account
D) Export reconciliation data to Excel, apply the rules externally, and re-import the results

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach uses reconciliation templates with different certification configurations (e.g., one template requiring dual certification and another requiring single review) and then leverages attribute mapping to assign the appropriate template based on the risk classification attribute. This is scalable and maintainable. Option A is unnecessarily complex, C is not scalable, and D defeats the purpose of using RCM.
</details>

---

### Question 6 (Medium)
**301.1.1** | Difficulty: Medium

Which setting in RCM controls whether a reconciliation requires preparer sign-off before it can be submitted for review?

A) The BalCheck threshold configuration
B) The certification workflow settings on the reconciliation template
C) The AutoRec matching tolerance
D) The column visibility settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Certification workflow settings on the reconciliation template define the required sign-off steps, including whether a preparer must complete and sign off on a reconciliation before it can progress to the reviewer stage. BalCheck (A) relates to balance validation, AutoRec (C) is for automatic reconciliation, and column visibility (D) is a display setting.
</details>

---

### Question 7 (Hard)
**301.1.1** | Difficulty: Hard

When configuring control lists in RCM, what is the impact of enabling the "Enforce Segregation of Duties" option?

A) It prevents any user from accessing the RCM module without administrator approval
B) It ensures that the same user cannot serve as both preparer and reviewer/certifier on the same reconciliation
C) It restricts all reconciliation modifications to a single designated user per entity
D) It disables automated reconciliation features to require manual review of all accounts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Enabling segregation of duties in the control list settings ensures that the same individual cannot act as both the preparer and the reviewer or certifier on the same reconciliation. This is a critical internal control that prevents a single person from both creating and approving a reconciliation, reducing fraud and error risk. This is a standard financial close best practice.
</details>

---

## Objective 301.1.2: Identify options on the RCM Reconciliation Administration page

### Question 8 (Easy)
**301.1.2** | Difficulty: Easy

On the RCM Reconciliation Administration page, which action allows an administrator to assign reconciliation templates to specific accounts?

A) Export to Excel
B) Template Assignment
C) Run Consolidation
D) Lock Period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Template Assignment on the Reconciliation Administration page allows administrators to assign specific reconciliation templates to accounts or groups of accounts. This determines which reconciliation format and workflow each account will follow. The other options either do not exist in this context or serve different purposes.
</details>

---

### Question 9 (Medium)
**301.1.2** | Difficulty: Medium

From the RCM Reconciliation Administration page, an administrator needs to bulk-update the preparer assignment for 200 accounts. What is the most efficient approach?

A) Edit each account reconciliation individually through the administration page
B) Use the attribute mapping feature or bulk update functionality to assign preparers based on dimensional attributes
C) Delete all reconciliations and recreate them with the correct preparer
D) Export to a flat file, modify externally, and reload through a data management step

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The attribute mapping feature or bulk update capabilities on the Administration page allow administrators to efficiently update preparer assignments for large numbers of accounts based on dimensional attributes (such as entity or account group). This is far more efficient than individual edits (A), destructive recreation (C), or external processing (D).
</details>

---

### Question 10 (Easy)
**301.1.2** | Difficulty: Easy

What does the "Refresh" action on the RCM Administration page accomplish?

A) It deletes all existing reconciliation data and starts fresh
B) It updates the reconciliation control list to reflect current dimension member changes and attribute mappings
C) It restores the page to factory default settings
D) It sends email notifications to all assigned preparers

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Refresh action updates the reconciliation control list to reflect any changes in dimension members, attribute mappings, or template assignments. This ensures the reconciliation list stays current when accounts are added, removed, or reclassified. It does not delete data (A), reset settings (C), or send notifications (D).
</details>

---

### Question 11 (Medium)
**301.1.2** | Difficulty: Medium

On the RCM Administration page, what is the purpose of the "Period Management" options?

A) To define the fiscal year calendar for the entire OneStream application
B) To open, close, and manage reconciliation periods including locking completed periods
C) To set up recurring journal entries
D) To configure data extraction schedules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Period Management on the RCM Administration page allows administrators to open reconciliation periods for preparers to work on, close periods when all reconciliations are complete, and lock periods to prevent further changes. This is distinct from the application-level fiscal calendar (A) and unrelated to journal entries (C) or data extraction (D).
</details>

---

### Question 12 (Hard)
**301.1.2** | Difficulty: Hard

An administrator notices that after running a Refresh on the RCM Administration page, several accounts that were previously assigned to reconciliation templates now show as "Unassigned." What is the most likely cause?

A) The reconciliation templates were deleted from the system
B) The dimension member properties or attribute mappings that drive template assignment have been modified, and the affected accounts no longer match the assignment criteria
C) The server cache has become corrupted and requires a full system restart
D) Another administrator simultaneously ran a conflicting bulk update

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When accounts become unassigned after a Refresh, the most likely cause is that the dimension member properties or the attribute mappings used for template assignment have been changed. The Refresh re-evaluates all assignments based on current conditions, so if an account's properties no longer match the criteria for its previous template assignment, it will appear as unassigned. The administrator should review recent changes to dimension members or attribute mapping rules.
</details>

---

### Question 13 (Medium)
**301.1.2** | Difficulty: Medium

Which of the following tasks can be performed from the RCM Reconciliation Administration page?

A) Creating new cube dimensions for the application
B) Viewing and managing the reconciliation control list, including filtering by status, template, and assignment
C) Modifying the OneStream platform security model
D) Building custom dashboard components

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The RCM Reconciliation Administration page provides a centralized view of the reconciliation control list with filtering and management capabilities. Administrators can filter by reconciliation status, template, preparer/reviewer assignment, and other attributes. Creating dimensions (A), modifying platform security (C), and building dashboards (D) are performed in other areas of OneStream.
</details>

---

## Objective 301.1.3: Identify components of the RCM Reconciliation and Reporting page

### Question 14 (Easy)
**301.1.3** | Difficulty: Easy

On the RCM Reconciliation and Reporting page, what does the reconciliation status indicator show?

A) The server processing load for the reconciliation engine
B) The current state of each reconciliation such as Not Started, In Progress, Completed, or Certified
C) The number of journal entries posted for each account
D) The real-time stock price of the company

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The reconciliation status indicator on the Reporting page displays the current workflow state of each reconciliation, typically including statuses such as Not Started, In Progress, Completed (pending review), and Certified. This allows managers and auditors to quickly assess the progress of the reconciliation process.
</details>

---

### Question 15 (Easy)
**301.1.3** | Difficulty: Easy

What is the purpose of BalCheck in the context of RCM Reconciliation and Reporting?

A) To verify that debit and credit entries balance within a journal
B) To validate that the reconciled balance matches the expected GL balance or source system balance
C) To check the bank account balance against the cash flow statement
D) To ensure intercompany balances eliminate to zero

<details>
<summary>Show answer</summary>

**Correct answer: B)**

BalCheck is a validation feature in RCM that compares the reconciled balance against the expected balance from the general ledger or source system. It helps identify discrepancies that need to be investigated, ensuring that the reconciliation accurately reflects the account balance. It is not specifically for journal balancing (A), bank reconciliation (C), or intercompany elimination (D).
</details>

---

### Question 16 (Medium)
**301.1.3** | Difficulty: Medium

On the RCM Reconciliation and Reporting page, what information does the AutoRec feature provide?

A) Automatic generation of financial statements from reconciliation data
B) Identification of accounts that meet predefined criteria for automatic reconciliation without manual preparer intervention
C) Automated email notifications to external auditors
D) Automatic calculation of foreign currency translation adjustments

<details>
<summary>Show answer</summary>

**Correct answer: B)**

AutoRec identifies accounts that meet predefined criteria (such as zero balances, immaterial balances, or balances within a specified threshold) and automatically marks them as reconciled without requiring manual preparer intervention. This significantly reduces the workload during the close process by allowing preparers to focus on higher-risk or more complex reconciliations.
</details>

---

### Question 17 (Hard)
**301.1.3** | Difficulty: Hard

A financial controller is reviewing the RCM Reporting page and notices that the certification completion rate is at 85% but the period close deadline is approaching. Which combination of reporting page components would best help identify the bottleneck?

A) BalCheck results and AutoRec statistics only
B) Status filters to show uncertified reconciliations, reviewer assignment details, and aging analysis of in-progress items
C) Column visibility settings and template configuration options
D) The export function to generate a PDF summary report

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To identify bottlenecks, the controller should use status filters to isolate uncertified reconciliations, examine reviewer assignment details to identify if specific reviewers are overloaded, and review aging information to see which reconciliations have been in progress the longest. This combination provides actionable intelligence. BalCheck and AutoRec (A) address data quality, not workflow bottlenecks. Column settings (C) and exports (D) are administrative functions that do not directly diagnose delays.
</details>

---

### Question 18 (Medium)
**301.1.3** | Difficulty: Medium

Which reporting component on the RCM Reconciliation and Reporting page allows users to drill down into the detail items supporting a reconciliation?

A) The consolidation journal viewer
B) The reconciliation detail panel, which displays supporting items, attachments, and comments
C) The cube data explorer
D) The workflow task scheduler

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The reconciliation detail panel on the Reporting page allows users to drill into the supporting detail for any reconciliation, including detail items (such as outstanding items, timing differences, and adjustments), file attachments, comments, and sign-off history. This provides the full audit trail and documentation for each reconciliation. The other options are not RCM Reporting page components.
</details>

---

### Question 19 (Easy)
**301.1.1** | Difficulty: Easy

What type of reconciliation attribute can be used to classify accounts by their level of scrutiny required during the close process?

A) Currency conversion rate
B) Risk rating (e.g., High, Medium, Low)
C) Server hostname
D) Dashboard color theme

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Risk rating is a reconciliation attribute that classifies accounts by the level of scrutiny they require. High-risk accounts may need more detailed documentation, dual certification, or more frequent reconciliation. This attribute is typically assigned via dynamic attribute mapping based on account properties such as balance materiality, account type, or historical error rates.
</details>

---

### Question 20 (Medium)
**301.1.1** | Difficulty: Medium

When configuring the RCM Global Setup, what is the effect of enabling the "Require Attachments" option on a reconciliation template?

A) The system automatically generates PDF attachments for each reconciliation
B) Preparers must attach at least one supporting document before they can submit the reconciliation for review
C) All email communications related to the reconciliation are automatically attached
D) External auditors are given direct upload access to add attachments

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Enabling the "Require Attachments" option enforces that preparers must attach supporting documentation (such as bank statements, schedules, or screenshots) before the reconciliation can progress to the review stage. This ensures adequate documentation exists for every reconciliation and supports audit readiness. The system does not auto-generate attachments (A), attach emails (C), or grant external access (D).
</details>

---

### Question 21 (Hard)
**301.1.1** | Difficulty: Hard

An organization wants to implement a reconciliation process where certain immaterial accounts are automatically reconciled each period without preparer intervention, while material accounts require full preparation and dual certification. Which RCM configuration approach accomplishes this?

A) Create a single template with conditional logic that changes behavior based on the balance at runtime
B) Configure two separate templates: one with AutoRec enabled for immaterial accounts and another with full prepare-review-certify workflow for material accounts, then use attribute mapping based on a materiality threshold to assign each account to the appropriate template
C) Disable RCM for immaterial accounts and reconcile them manually in spreadsheets
D) Set the certification level to zero for all accounts and rely on management review outside the system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach uses two distinct reconciliation templates. The AutoRec-enabled template handles immaterial accounts by automatically reconciling them when they meet predefined criteria (e.g., balance below a threshold). The full-workflow template handles material accounts with required preparation, review, and dual certification. Attribute mapping on the account dimension drives template assignment based on materiality. This is scalable, auditable, and maintains proper controls.
</details>

---

### Question 22 (Easy)
**301.1.1** | Difficulty: Easy

In RCM, what does the term "control list" refer to?

A) A list of IT security controls applied to the OneStream server
B) The master list of all account reconciliations that need to be performed for a given period
C) A list of external audit findings
D) A log of all user login attempts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The control list in RCM is the comprehensive inventory of all account reconciliations that must be completed during a period. It includes information such as the account, entity, assigned preparer and reviewer, reconciliation template, risk rating, and current status. The control list is the central management tool for tracking reconciliation progress across the organization.
</details>

---

### Question 23 (Medium)
**301.1.1** | Difficulty: Medium

When setting up dynamic attribute mapping in RCM, which of the following dimension member properties can typically be used as mapping criteria?

A) The physical location of the server hosting the dimension
B) Account type, entity region, account group, or custom attributes defined on dimension members
C) The date the dimension member was last accessed by any user
D) The size of the database table storing the dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dynamic attribute mapping uses properties defined on dimension members as criteria for assigning reconciliation attributes. Common mapping sources include account type (balance sheet, P&L), entity region or department, account group classifications, and custom attributes added to dimension members. These properties allow administrators to define rules such as "all balance sheet accounts in Entity X are assigned to Preparer Y."
</details>

---

### Question 24 (Medium)
**301.1.2** | Difficulty: Medium

On the RCM Administration page, what happens when an administrator changes the reconciliation template assigned to an account that already has an in-progress reconciliation?

A) The change is applied immediately and all existing work is deleted
B) The change is queued and takes effect in the next period; the current period retains the original template assignment
C) The system prevents any template changes while a reconciliation is in progress
D) The reconciliation is automatically moved to Certified status

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To protect in-progress work, template assignment changes typically take effect in the next reconciliation period rather than disrupting the current period's active reconciliations. This ensures that preparers do not lose work or encounter unexpected format changes mid-period. Administrators should plan template changes between close cycles when possible.
</details>

---

### Question 25 (Hard)
**301.1.2** | Difficulty: Hard

An administrator needs to set up RCM for a newly acquired subsidiary with 300 accounts. The subsidiary uses different account classifications than the parent company. What is the most efficient approach using the RCM Administration page?

A) Manually configure each of the 300 accounts individually through the administration page
B) Define attribute mappings that translate the subsidiary's account classifications to the parent's reconciliation framework, run a Refresh to generate the control list, and then review and adjust any exceptions on the Administration page
C) Copy the parent company's reconciliation assignments directly to the subsidiary without any mapping
D) Create a separate RCM application exclusively for the subsidiary with its own independent templates

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most efficient approach leverages attribute mapping to automate the bulk of the configuration. By defining how the subsidiary's account classifications map to reconciliation templates, risk ratings, and preparer/reviewer assignments, the Refresh process generates the control list automatically. The administrator then reviews the results on the Administration page and adjusts any exceptions. This is far more efficient than manual configuration (A), avoids misalignment from blind copying (C), and maintains a unified close process (unlike D).
</details>

---

### Question 26 (Easy)
**301.1.2** | Difficulty: Easy

Which of the following filter options is typically available on the RCM Administration page to help manage large control lists?

A) Filter by server CPU usage
B) Filter by reconciliation status, preparer, reviewer, template, or entity
C) Filter by the color of the user's desktop theme
D) Filter by internet browser type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The RCM Administration page provides multiple filter options to help administrators manage large control lists efficiently. Commonly available filters include reconciliation status (Not Started, In Progress, Completed, Certified), assigned preparer, assigned reviewer, reconciliation template, entity, and risk rating. These filters allow targeted management of specific subsets of reconciliations.
</details>

---

### Question 27 (Medium)
**301.1.2** | Difficulty: Medium

From the RCM Administration page, what is the purpose of the "Lock Period" functionality?

A) To prevent users from logging into the system during a specific period
B) To prevent any further changes to reconciliations in a closed period, preserving the data for audit purposes
C) To lock the exchange rates used during the period
D) To restrict data imports during the close process

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Lock Period functionality prevents modifications to reconciliations after a period has been closed and certified. Once locked, preparers and reviewers cannot alter reconciliation data, detail items, attachments, or certifications for that period. This preserves data integrity for audit purposes and ensures that completed reconciliations are not inadvertently or intentionally modified after the close is finalized.
</details>

---

### Question 28 (Medium)
**301.1.3** | Difficulty: Medium

On the RCM Reconciliation and Reporting page, what does the "Aging" indicator show for outstanding detail items?

A) The age of the user account that created the item
B) The number of periods an outstanding reconciling item has remained unresolved
C) The age of the OneStream application installation
D) The time since the last system backup

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Aging indicator tracks how long outstanding reconciling items have remained unresolved across periods. For example, a timing difference that has been carried forward for three periods would show an age of 3. This helps management identify stale items that may indicate underlying issues requiring investigation, rather than legitimate timing differences that should clear in the normal course of business.
</details>

---

### Question 29 (Easy)
**301.1.3** | Difficulty: Easy

What type of information can be included as a detail item on an RCM reconciliation?

A) Only numerical amounts with no description
B) Outstanding items, timing differences, adjustments, descriptions, supporting references, and categorization
C) Only the GL balance and nothing else
D) Only system-generated data that cannot be edited

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Detail items on an RCM reconciliation can include various types of reconciling information: outstanding items (such as checks or deposits not yet cleared), timing differences between systems, manual adjustments, descriptive text explaining the item, reference numbers, categorization tags, and amounts. This comprehensive detail supports the reconciliation explanation and provides audit evidence.
</details>

---

### Question 30 (Hard)
**301.1.3** | Difficulty: Hard

A manager reviewing the RCM Reporting page notices that one entity has a 95% certification rate while another entity with a similar number of accounts is at only 40%. Both entities started the close process at the same time. Which combination of Reporting page features would best diagnose the discrepancy?

A) Compare the BalCheck thresholds configured for each entity
B) Use entity-level filtering to compare the status distribution, review preparer workload across both entities, examine aging of in-progress items, and check whether AutoRec criteria differences are contributing to the gap
C) Check the server logs for hardware performance issues
D) Review the font size settings on the reporting dashboard

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A thorough diagnosis requires multiple Reporting page features. Entity-level filtering isolates each entity's reconciliations for comparison. Status distribution analysis shows where reconciliations are stalled (e.g., many stuck at "In Progress" vs. "Pending Review"). Preparer workload comparison can reveal if one entity's preparers are overloaded. Aging analysis identifies items that have been pending longest. AutoRec criteria comparison may reveal that one entity benefits from more automatic reconciliations. Together, these provide actionable insights.
</details>

---

### Question 31 (Medium)
**301.1.1** | Difficulty: Medium

What is the purpose of column settings in the RCM configuration?

A) To define the columns in the underlying database tables
B) To control which data fields are visible and in what order on the reconciliation control list and reporting views
C) To set the width of printed report columns
D) To configure spreadsheet export column headers only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Column settings in RCM control which data fields are displayed on the reconciliation control list and reporting views, as well as the order in which they appear. Administrators can show or hide columns such as preparer, reviewer, status, risk rating, BalCheck result, and custom attributes. This allows the views to be tailored for different audiences (e.g., preparers vs. management) and ensures relevant information is prominently displayed.
</details>

---

### Question 32 (Easy)
**301.1.3** | Difficulty: Easy

In RCM, what is the purpose of the certification workflow?

A) To certify that the OneStream software license is valid
B) To provide a formal sign-off process where preparers, reviewers, and certifiers confirm the accuracy of a reconciliation
C) To generate SSL certificates for secure data transmission
D) To certify that the server hardware meets minimum requirements

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The certification workflow in RCM provides a structured sign-off process for reconciliations. Preparers complete the reconciliation and sign off that the work is accurate. Reviewers verify the preparer's work and approve it. Certifiers (in dual-certification scenarios) provide a final level of sign-off. Each step is recorded with a timestamp and user identity, creating an audit trail that supports SOX compliance and other regulatory requirements.
</details>

---

### Question 33 (Hard)
**301.1.1** | Difficulty: Hard

An organization is configuring RCM access control and needs to ensure that: (1) preparers in Region A cannot view reconciliations for Region B, (2) regional managers can view all reconciliations within their region, and (3) the global controller can view and certify all reconciliations. Which access control approach best meets these requirements?

A) Create a single security role with full access and train users to only view their assigned reconciliations
B) Configure entity-based security filtering combined with role-based access control, where preparers are restricted to their assigned entities, regional managers have read access across their region's entities, and the global controller has full access across all entities
C) Create separate RCM applications for each region with no cross-access
D) Use password-protected Excel exports for each region instead of in-system access control

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach combines entity-based security filtering with role-based access control. Preparers' access is limited to their assigned entity reconciliations through entity security. Regional managers receive a broader scope covering all entities in their region with view and review permissions. The global controller receives unrestricted access with certification rights. This layered security model enforces the principle of least privilege while allowing appropriate visibility at each organizational level.
</details>

---

### Question 34 (Medium)
**301.1.2** | Difficulty: Medium

What is the significance of the "Active/Inactive" status flag on accounts in the RCM Administration page?

A) It controls whether the OneStream user account is enabled or disabled
B) It determines whether an account is included in the current period's reconciliation control list, allowing administrators to exclude accounts that are no longer relevant without deleting their history
C) It toggles the visibility of the account in the chart of accounts
D) It controls whether data can be loaded to the account in the cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Active/Inactive flag on the RCM Administration page controls whether an account appears on the reconciliation control list for the current and future periods. Setting an account to Inactive removes it from the active reconciliation workload without deleting its historical reconciliation data. This is useful when accounts are closed, merged, or no longer require reconciliation, while preserving the audit trail of past reconciliations.
</details>

---

### Question 35 (Medium)
**301.1.3** | Difficulty: Medium

On the RCM Reconciliation and Reporting page, what is the difference between the "Completed" and "Certified" reconciliation statuses?

A) There is no difference; the terms are interchangeable
B) "Completed" means the preparer has finished their work and signed off; "Certified" means the reviewer or certifier has also reviewed and approved the reconciliation
C) "Completed" is for low-risk accounts; "Certified" is for high-risk accounts
D) "Completed" applies to automated reconciliations; "Certified" applies to manual reconciliations

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Completed" and "Certified" represent different stages in the certification workflow. A reconciliation moves to "Completed" status when the preparer finishes documenting the reconciliation and signs off. It then advances to "Certified" when the assigned reviewer or certifier reviews the work and provides their approval sign-off. This distinction is important for tracking progress and ensuring that the review step is not bypassed.
</details>

---

### Question 36 (Hard)
**301.1.1** | Difficulty: Hard

An administrator is configuring BalCheck rules and needs to handle accounts where the expected balance comes from different source systems depending on the account type. Balance sheet accounts should compare against the sub-ledger, while certain cash accounts should compare against the bank statement feed. How should BalCheck be configured?

A) Use a single BalCheck rule for all accounts and manually adjust discrepancies
B) Configure different BalCheck data sources at the reconciliation template level, with the balance sheet template pointing to the sub-ledger data source and the cash account template pointing to the bank statement data source
C) Disable BalCheck for cash accounts and only validate balance sheet accounts
D) Load all source data into a single staging table and use one universal comparison

<details>
<summary>Show answer</summary>

**Correct answer: B)**

BalCheck configuration can be tailored at the reconciliation template level to use different data sources for balance validation. By assigning balance sheet accounts to a template that sources its comparison balance from the sub-ledger and cash accounts to a template sourcing from bank statement data, each account type is validated against the appropriate source. This ensures accurate balance checks regardless of the source system, without requiring manual adjustments (A) or disabling validation (C).
</details>

---
