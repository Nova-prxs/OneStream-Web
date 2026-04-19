# Question Bank - Section 3: Integration (18% of exam)

## Objectives
- **301.3.1:** Manage integration between RCM and TXM components

---

## Objective 301.3.1: Manage integration between RCM and TXM components

### Question 1 (Easy)
**301.3.1** | Difficulty: Easy

What is the primary benefit of integrating RCM and TXM within OneStream Financial Close?

A) It eliminates the need for any manual reconciliation work
B) It allows transaction matching results to flow directly into account reconciliations, providing a unified close process
C) It replaces the general ledger with a transaction-based accounting system
D) It enables real-time stock market data feeds into reconciliations

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The integration between RCM and TXM allows transaction matching results from TXM to flow directly into the corresponding account reconciliations in RCM. This creates a unified financial close process where matched and unmatched transactions automatically populate reconciliation detail items, reducing manual data entry and ensuring consistency between the two components.
</details>

---

### Question 2 (Easy)
**301.3.1** | Difficulty: Easy

In the integrated RCM-TXM workflow, where do unmatched transactions from TXM typically appear?

A) They are automatically deleted after the matching process
B) They appear as outstanding detail items on the corresponding RCM reconciliation
C) They are exported to an external system for separate processing
D) They are hidden from view until the next period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Unmatched transactions from TXM flow into RCM as outstanding or open detail items on the corresponding account reconciliation. This ensures that preparers and reviewers are aware of all unreconciled items and can investigate or document them as part of the reconciliation process. They are not deleted (A), exported (C), or hidden (D).
</details>

---

### Question 3 (Medium)
**301.3.1** | Difficulty: Medium

How do detail items function in the integration between RCM and TXM?

A) Detail items are static text fields used only for documentation purposes
B) Detail items serve as the bridge between TXM transaction data and RCM reconciliations, representing individual reconciling items such as outstanding transactions, timing differences, and adjustments
C) Detail items are only used in RCM and have no connection to TXM
D) Detail items represent cube dimension members used for data aggregation

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Detail items are the key integration point between TXM and RCM. They represent individual reconciling items on an account reconciliation and can be populated automatically from TXM matching results. Matched transactions may appear as cleared items, while unmatched transactions become outstanding items. Detail items can also include timing differences, adjustments, and manual entries, providing the complete picture of the reconciliation.
</details>

---

### Question 4 (Medium)
**301.3.1** | Difficulty: Medium

When TXM matching results are integrated with RCM, what happens to the reconciliation balance when a previously unmatched transaction is subsequently matched?

A) The reconciliation balance does not change because matching has no impact on RCM
B) The detail item representing the unmatched transaction is cleared or removed from the outstanding items, updating the reconciliation balance accordingly
C) The reconciliation must be manually recalculated by the preparer
D) A new reconciliation is created to replace the original one

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a previously unmatched transaction is matched in TXM, the integration updates the corresponding detail item in RCM. The item moves from outstanding to cleared status, and the reconciliation balance is updated to reflect that the item has been resolved. This dynamic linkage ensures the reconciliation always reflects the current state of transaction matching.
</details>

---

### Question 5 (Hard)
**301.3.1** | Difficulty: Hard

An organization uses both RCM and TXM for bank account reconciliation. The TXM component matches bank transactions to GL entries, while RCM manages the overall reconciliation workflow. During the close process, a preparer notices that the RCM reconciliation shows a $2,500 unexplained variance, but all TXM transactions appear matched. What is the most likely integration-related cause?

A) The TXM matching engine has a calculation error
B) Transactions were matched in TXM using tolerance settings, and the cumulative tolerance variances across multiple matches total $2,500
C) The RCM system is using data from a different period than TXM
D) The preparer does not have sufficient security permissions to view the matched transactions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When TXM matches transactions using tolerance settings, each individual match may have a small variance that falls within the acceptable threshold. However, when these variances are aggregated across many transactions, they can accumulate to a significant total. For example, 50 matches each with a $50 tolerance variance would produce a cumulative $2,500 difference. The preparer should review the tolerance variances in the TXM Matches page and account for them as a reconciling item in RCM.
</details>

---

### Question 6 (Easy)
**301.3.1** | Difficulty: Easy

In the integrated Financial Close solution, which component is responsible for the workflow approval process (prepare, review, certify)?

A) TXM handles all workflow approvals
B) RCM manages the workflow approval process for reconciliations
C) Workflow approvals are handled by an external system
D) Both RCM and TXM have independent workflow approval processes that are not connected

<details>
<summary>Show answer</summary>

**Correct answer: B)**

RCM manages the overall reconciliation workflow including the prepare, review, and certify approval stages. TXM provides the transaction matching data that feeds into the reconciliation, but the workflow governance (who prepares, who reviews, who certifies, and the sign-off process) is managed through RCM. This clear separation of concerns keeps the workflow centralized.
</details>

---

### Question 7 (Medium)
**301.3.1** | Difficulty: Medium

What is the recommended sequence for running TXM and RCM processes during a period close?

A) Run RCM certification first, then execute TXM matching
B) Execute TXM transaction import and matching first, then allow RCM preparers to work with the resulting detail items on their reconciliations
C) Run both processes simultaneously with no dependency
D) Run RCM and TXM in alternating cycles until all items are resolved

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended workflow is to first import transactions and execute TXM matching so that matched and unmatched results flow into RCM as detail items. Then RCM preparers can work with complete information when preparing their reconciliations. Certifying RCM first (A) would mean reconciliations lack TXM data. Running simultaneously (C) may result in incomplete data. Alternating cycles (D) is unnecessarily complex.
</details>

---

### Question 8 (Hard)
**301.3.1** | Difficulty: Hard

An administrator is designing the integration between RCM and TXM for a multi-entity organization where different entities use different bank formats. What design consideration is critical for ensuring seamless data flow between the two components?

A) All entities must use the same bank file format to ensure TXM compatibility
B) The TXM source configuration must accommodate multiple file formats with consistent field mapping to standardized transaction attributes that RCM detail items can consume
C) Each entity should use a completely separate RCM and TXM instance with no integration
D) Bank files should be manually reformatted outside OneStream before import

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The critical design consideration is configuring TXM source definitions to handle multiple input formats while mapping them to a consistent set of standardized transaction attributes. This ensures that regardless of the bank file format, the resulting transactions are normalized and can flow seamlessly into RCM detail items. Requiring a single format (A) is impractical in multi-entity environments, separate instances (C) eliminate integration benefits, and manual reformatting (D) introduces error and inefficiency.
</details>

---

### Question 9 (Medium)
**301.3.1** | Difficulty: Medium

How does the integration between RCM and TXM handle period carryforward of unmatched transactions?

A) Unmatched transactions are automatically deleted at period end
B) Unmatched transactions from TXM carry forward to the next period and continue to appear as outstanding detail items in RCM until they are matched or manually resolved
C) Unmatched transactions must be manually re-entered in each new period
D) Carryforward is not supported; all transactions must be resolved within the current period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The integration supports period carryforward, meaning unmatched transactions from TXM carry forward into subsequent periods and remain as outstanding detail items on the RCM reconciliation. This reflects real-world scenarios where some items (like outstanding checks or deposits in transit) may take multiple periods to clear. The items persist until they are matched in TXM or manually resolved in RCM.
</details>

---

### Question 10 (Medium)
**301.3.1** | Difficulty: Medium

Which of the following best describes the data flow direction between TXM and RCM in the integrated solution?

A) Data flows only from RCM to TXM
B) Data flows bidirectionally with equal dependency in both directions
C) Transaction matching results flow from TXM into RCM as detail items, while RCM provides the reconciliation context and workflow governance
D) There is no data flow; RCM and TXM operate as completely independent modules

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The primary data flow is from TXM to RCM: TXM processes and matches transactions, and the results (both matched and unmatched items) flow into RCM as detail items populating the reconciliations. RCM provides the overarching reconciliation framework, workflow management, and certification process. While there is a relationship between both components, the transactional data primarily flows from TXM into RCM's reconciliation structure.
</details>

---

### Question 11 (Hard)
**301.3.1** | Difficulty: Hard

During an integrated RCM-TXM implementation, the project team discovers that some reconciliation accounts require transaction matching while others do not. How should the solution be designed to handle this mixed requirement?

A) Implement TXM for all accounts regardless of whether they need transaction matching
B) Use different reconciliation templates in RCM where TXM-integrated templates include detail items sourced from transaction matching and non-TXM templates use manually entered or system-loaded detail items
C) Create two completely separate Financial Close applications, one with TXM and one without
D) Disable TXM entirely and handle all matching manually within RCM

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The solution should use different reconciliation templates within RCM to accommodate both scenarios. Accounts requiring transaction matching use templates configured to receive detail items from TXM, while accounts without transaction matching needs use templates with manually entered or data-loaded detail items. This flexible approach is managed through attribute mapping to assign the appropriate template to each account. Creating separate applications (C) is unnecessarily complex, and options A and D do not address the mixed requirement efficiently.
</details>

---

### Question 12 (Easy)
**301.3.1** | Difficulty: Easy

What role do attachments play in the integrated RCM-TXM workflow?

A) Attachments are only supported in TXM and cannot be viewed in RCM
B) Supporting documents can be attached to both individual transactions in TXM and to the overall reconciliation in RCM, providing a complete audit trail
C) Attachments are automatically generated by the system and cannot be added manually
D) Attachments are stored externally and are not part of the OneStream Financial Close solution

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The integrated solution supports attachments at both the transaction level (in TXM) and the reconciliation level (in RCM). This allows users to attach supporting documentation such as bank statements, invoices, or correspondence to individual transactions as well as to the overall reconciliation. This comprehensive attachment capability supports audit requirements and provides a complete documentation trail.
</details>

---

### Question 13 (Medium)
**301.3.1** | Difficulty: Medium

When configuring the integration between RCM and TXM, what must be aligned between the two components to ensure proper data flow?

A) The color scheme and visual theme settings
B) The dimensional context (entity, account, period) so that TXM transaction results map correctly to the corresponding RCM reconciliation
C) The number of user licenses assigned to each module
D) The physical server locations for each component

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For proper integration, the dimensional context must be aligned between RCM and TXM. This means the entity, account, and period dimensions used to organize transactions in TXM must correspond to the same dimensional intersections used to define reconciliations in RCM. This alignment ensures that matched and unmatched transactions flow to the correct account reconciliation automatically.
</details>

---

### Question 14 (Hard)
**301.3.1** | Difficulty: Hard

A global organization has implemented the integrated RCM-TXM solution. During month-end close, the European subsidiaries process their bank transactions in TXM before the US subsidiaries due to time zone differences. How should the integration workflow be designed to accommodate this staggered processing?

A) Require all subsidiaries to process simultaneously regardless of time zones
B) Design the workflow so that TXM matching and RCM reconciliation progress can occur independently per entity, with the overall close dashboard reflecting real-time status across all entities
C) Process all entities in a single batch after the last time zone completes their transactions
D) Separate European and US operations into different OneStream applications

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The integration should be designed so that each entity can progress through TXM matching and RCM reconciliation independently based on their own timing. As European entities complete their TXM processing, results flow into their RCM reconciliations and they can begin the prepare-review-certify workflow. US entities follow when ready. The close dashboard provides a real-time consolidated view of progress across all entities. This approach maximizes efficiency by leveraging time zone differences rather than creating artificial constraints.
</details>

---

### Question 15 (Easy)
**301.3.1** | Difficulty: Easy

In the integrated RCM-TXM solution, which component owns the "source of truth" for whether a transaction has been matched?

A) RCM stores the definitive match status for all transactions
B) TXM maintains the definitive match status, and the results flow into RCM as detail items
C) The match status is stored in an external database outside both RCM and TXM
D) Neither component tracks match status; it must be determined manually each period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

TXM is the authoritative source for transaction matching status. It performs the matching logic, stores match results, and then communicates those results to RCM in the form of detail items (matched, unmatched, tolerance). RCM consumes this information to populate reconciliations but does not independently determine match status. This clear ownership prevents conflicting information between the two components.
</details>

---

### Question 16 (Medium)
**301.3.1** | Difficulty: Medium

When a user manually adds a detail item directly in RCM for an account that also uses TXM integration, how does the system distinguish between TXM-sourced and manually-entered detail items?

A) There is no distinction; all detail items are treated identically
B) Detail items carry a source indicator showing whether they originated from TXM matching results or were manually entered by a preparer, preserving data lineage
C) Manually entered items are automatically deleted when TXM runs
D) TXM-sourced items appear in a different application than manual items

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Detail items in RCM carry a source attribute that indicates their origin. Items flowing from TXM are tagged as system-generated from transaction matching, while items entered directly by a preparer are tagged as manual entries. This distinction is important for audit purposes, as it allows reviewers to understand the provenance of each reconciling item and apply appropriate scrutiny. Manual items are not deleted by TXM runs (C).
</details>

---

### Question 17 (Hard)
**301.3.1** | Difficulty: Hard

An organization runs TXM matching on Day 2 of the close, but additional bank transactions arrive on Day 4. After re-running TXM with the new data, some previously unmatched items are now matched. How does the RCM integration handle this mid-period update?

A) RCM ignores any changes after the initial TXM data load
B) The newly matched items are automatically updated in the RCM reconciliation: previously outstanding detail items move to cleared status, and the reconciliation balance adjusts dynamically to reflect the updated matching results
C) The preparer must manually delete old detail items and re-enter new ones
D) The reconciliation must be restarted from scratch with a new control list entry

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The RCM-TXM integration supports dynamic updates. When TXM matching is re-run and previously unmatched transactions are now matched, the corresponding detail items in RCM are updated automatically. Items that were outstanding move to cleared, and the reconciliation balance reflects the new state. This means the RCM reconciliation always represents the current state of matching, and preparers can continue working with up-to-date information without manual intervention.
</details>

---

### Question 18 (Easy)
**301.3.1** | Difficulty: Easy

What is a "reconciling item" in the context of RCM-TXM integration?

A) A configuration setting that links RCM to TXM
B) An individual item that explains the difference between two balances, such as an outstanding transaction, timing difference, or adjustment
C) A matched transaction pair with zero variance
D) A user role that has permission to reconcile accounts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A reconciling item is an individual entry that explains part of the difference between two balances being reconciled (e.g., book balance vs. bank balance). Common reconciling items include outstanding checks (in transit), deposits in transit, timing differences, bank fees, and manual adjustments. In the integrated solution, unmatched transactions from TXM automatically become reconciling items in RCM, while matched transactions clear from the outstanding list.
</details>

---

### Question 19 (Medium)
**301.3.1** | Difficulty: Medium

In the integrated workflow, what happens to TXM tolerance variances when they flow into RCM as detail items?

A) Tolerance variances are automatically written off and do not appear in RCM
B) Tolerance variances from TXM matches appear as separate detail items or are aggregated on the RCM reconciliation, requiring the preparer to document or explain the cumulative variance
C) Tolerance variances cause the entire match to be rejected in RCM
D) RCM converts all tolerance variances to zero

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When TXM matches transactions with tolerance (non-exact matches), the variance amounts flow into RCM as reconciling detail items. Preparers must account for these variances by documenting them, categorizing them (e.g., rounding differences, conversion variances), and potentially requesting write-off approval if they are immaterial. This ensures that even small variances are tracked and do not accumulate unnoticed across periods.
</details>

---

### Question 20 (Medium)
**301.3.1** | Difficulty: Medium

Which of the following is a prerequisite for the RCM-TXM integration to function correctly?

A) Both RCM and TXM must be running on separate physical servers
B) The dimensional structure (entities, accounts, periods) must be consistent between RCM and TXM so that transaction results map to the correct reconciliation
C) All transactions must be pre-matched manually before loading into TXM
D) RCM and TXM must use different user authentication systems

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A consistent dimensional structure is the fundamental prerequisite for integration. The entity, account, and period dimensions used in TXM must align with those used in RCM so that matched and unmatched transactions flow to the correct reconciliation. If Entity "US-001" and Account "1010-Cash" exist in TXM, the same intersection must exist in RCM's control list for the detail items to populate correctly.
</details>

---

### Question 21 (Hard)
**301.3.1** | Difficulty: Hard

During the close process, a preparer completes and submits their RCM reconciliation for review. After submission, a late-arriving transaction is imported into TXM and matched, changing the reconciliation's detail items. How should the integrated workflow handle this situation?

A) The match is silently applied and the already-submitted reconciliation is updated without notification
B) The system should either prevent TXM updates from modifying submitted/certified reconciliations or flag the reconciliation for re-preparation, depending on the workflow configuration, to maintain the integrity of the review process
C) The late transaction is automatically rejected by TXM
D) The reviewer is forced to certify the reconciliation as-is, regardless of the change

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This scenario highlights a critical workflow consideration. Well-designed integrations typically either lock reconciliation detail items once submitted for review (preventing TXM updates from altering the submitted state) or have a mechanism to revert the reconciliation to a prior workflow stage when underlying data changes. The specific behavior depends on the organization's policy, but the key principle is that changes to reconciliation data after submission must be controlled to maintain the integrity of the review and certification process.
</details>

---

### Question 22 (Easy)
**301.3.1** | Difficulty: Easy

In the integrated Financial Close solution, can a single account reconciliation include both TXM-sourced detail items and manually-entered detail items?

A) No, an account must use exclusively TXM or exclusively manual detail items
B) Yes, a reconciliation can include both TXM-sourced items (from transaction matching) and manually-entered items (such as adjustments or explanatory notes) on the same reconciliation
C) Only administrators can mix item sources on a single reconciliation
D) Mixed sources are allowed but cause BalCheck to fail

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A single RCM reconciliation can include detail items from multiple sources. TXM-sourced items (matched and unmatched transactions) coexist with manually-entered items (such as preparer adjustments, accruals, or explanatory notes). This flexibility allows preparers to provide a complete reconciliation narrative that includes both system-generated matching results and additional context or adjustments that only a human preparer can provide.
</details>

---

### Question 23 (Medium)
**301.3.1** | Difficulty: Medium

How does the integrated solution handle the scenario where TXM identifies a transaction as matched but the preparer in RCM disagrees with the match?

A) The preparer has no ability to challenge TXM match results
B) The preparer can flag the concern in RCM, and authorized users can unmatch the transactions in TXM, which then updates the corresponding detail items in RCM back to outstanding status
C) The preparer must accept all TXM matches without question
D) The preparer deletes the reconciliation and starts over

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The integrated workflow supports a feedback loop. If a preparer reviewing their RCM reconciliation identifies a questionable TXM match, they can flag it. An authorized user (with TXM matching permissions) can then unmatch the transactions in TXM. The integration propagates this change back to RCM, restoring the affected detail items to outstanding status. This ensures that human judgment can override automated matching when necessary.
</details>

---

### Question 24 (Hard)
**301.3.1** | Difficulty: Hard

An organization is implementing the integrated RCM-TXM solution and needs to decide how to handle historical carryforward items when going live. There are 2,000 outstanding items from the legacy reconciliation system that need to appear on the first RCM reconciliation period. What is the recommended approach?

A) Ignore all historical items and start with a clean slate
B) Load historical outstanding items as opening detail items in RCM for the go-live period, and import the corresponding transactions into TXM so that when they eventually clear, the integration automatically updates the RCM reconciliation
C) Manually re-key all 2,000 items into TXM one by one
D) Keep the legacy system running indefinitely alongside the new system for historical items

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach loads historical outstanding items into both RCM (as opening detail items on the go-live period reconciliation) and TXM (as pre-existing unmatched transactions). This establishes continuity from the legacy system. As historical items clear in subsequent periods (e.g., an outstanding check is cashed), TXM matches them and the integration automatically updates RCM. Ignoring history (A) creates an incomplete reconciliation. Manual entry (C) is impractical at scale. Running parallel systems (D) increases cost and complexity.
</details>

---

### Question 25 (Medium)
**301.3.1** | Difficulty: Medium

In the integrated workflow, what is the recommended approach for handling transactions that the preparer determines will never be matched (e.g., a stale check that will never be cashed)?

A) Leave them as unmatched indefinitely in TXM
B) Write off the item by manually clearing it in TXM or resolving it as a detail item in RCM with appropriate documentation and approval, following the organization's write-off policy
C) Delete the transaction from TXM without any documentation
D) Create a fake matching transaction to clear it from the unmatched list

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Items that will never match (stale checks, abandoned deposits, etc.) should be resolved through a documented write-off process. This may involve clearing the item in TXM with an appropriate resolution code and documentation, or resolving the detail item directly in RCM with preparer notes and management approval. The key is that the resolution is documented, approved per the organization's policy, and creates an audit trail. Leaving items indefinitely (A), undocumented deletion (C), or creating fake data (D) are all inappropriate.
</details>

---

### Question 26 (Easy)
**301.3.1** | Difficulty: Easy

What is the benefit of having a single dashboard that shows both RCM reconciliation progress and TXM matching status?

A) It reduces the number of OneStream licenses required
B) It provides management with a unified view of the close process, showing both reconciliation completion and transaction matching progress in one place
C) It eliminates the need for separate RCM and TXM modules
D) It automatically resolves all unmatched transactions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A unified dashboard combining RCM and TXM status gives management a holistic view of the financial close process. They can see both how many reconciliations are certified (RCM) and how many transactions remain unmatched (TXM) at a glance. This enables faster identification of bottlenecks and more informed decision-making about where to focus effort during the close. It does not replace the individual modules (C) or perform automatic resolution (D).
</details>

---

### Question 27 (Medium)
**301.3.1** | Difficulty: Medium

When configuring the integration between RCM and TXM, what role does the reconciliation template play in determining how TXM data appears in the reconciliation?

A) The template has no influence on TXM data presentation
B) The reconciliation template defines the layout, categorization, and display of TXM-sourced detail items, including how matched vs. unmatched items are presented and which TXM fields are visible on the reconciliation
C) The template only controls the color scheme of TXM data
D) The template determines the TXM matching rules to be used

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The reconciliation template in RCM controls how TXM-sourced detail items are presented within the reconciliation. It defines the layout of the detail section, how matched and unmatched items are categorized and displayed, which TXM transaction fields (amount, date, reference, description) are visible, and how items are grouped. Different templates can present the same TXM data in different ways appropriate to the reconciliation type.
</details>

---

### Question 28 (Hard)
**301.3.1** | Difficulty: Hard

An organization has 200 accounts using TXM integration and 800 accounts using manual detail items only. During month-end close, they want TXM matching to complete before RCM preparers begin work on TXM-integrated accounts, but preparers should be able to start on manual-only accounts immediately. How should the workflow be designed?

A) Block all RCM work until TXM matching is complete for all accounts
B) Use workflow sequencing or business rules to open TXM-integrated reconciliation accounts only after TXM matching is complete, while allowing manual-only accounts to be available for preparation immediately when the period opens
C) Process manual accounts first, then delete them and process TXM accounts
D) Run TXM matching and RCM preparation simultaneously for all accounts with no coordination

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The workflow should use conditional account availability. Manual-only accounts open for preparation as soon as the reconciliation period opens, maximizing preparer productivity. TXM-integrated accounts remain in a "pending data" state until TXM matching completes, then automatically become available for preparation with populated detail items. This approach optimizes the close timeline by parallelizing independent work while ensuring TXM-dependent accounts have complete data before preparers begin.
</details>

---

### Question 29 (Medium)
**301.3.1** | Difficulty: Medium

What audit trail information is maintained by the integrated RCM-TXM solution for each reconciliation?

A) Only the final certified balance with no supporting detail
B) Complete history including transaction import timestamps, match creation details (who, when, rule used), detail item changes, preparer/reviewer sign-off timestamps, and all comments and attachments
C) Only the preparer's name and certification date
D) Audit trails are not maintained; they must be recreated manually for auditors

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The integrated solution maintains a comprehensive audit trail covering the entire lifecycle: when transactions were imported into TXM, how and when matches were created (auto-match rule or manual), when detail items changed in RCM, all preparer and reviewer sign-offs with timestamps, comments added at each stage, and all attached supporting documents. This audit trail supports SOX compliance and external audit requirements without requiring manual reconstruction.
</details>

---

### Question 30 (Easy)
**301.3.1** | Difficulty: Easy

In the integrated solution, what triggers the creation of detail items in RCM from TXM?

A) A manual button press by the administrator for each individual transaction
B) The execution of TXM matching processes, which automatically generates or updates detail items in the linked RCM reconciliation based on match results
C) Detail items must always be created manually in RCM regardless of TXM activity
D) Detail items are created when the preparer opens the reconciliation for the first time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Detail items in RCM are created or updated automatically when TXM matching processes execute. As TXM processes transactions and determines matches and non-matches, the results flow into the linked RCM reconciliation as detail items. Matched transaction pairs may appear as cleared items, while unmatched transactions appear as outstanding items. This automation is the core value of the integration, eliminating the need for manual detail item creation for transaction-matched accounts.
</details>

---

### Question 31 (Hard)
**301.3.1** | Difficulty: Hard

A multinational organization has complex intercompany transactions where the same underlying business event generates transactions in both entities. Entity A records a payable and Entity B records a receivable. TXM matches these intercompany transactions. How should the integration be designed so that both Entity A's and Entity B's RCM reconciliations benefit from the matching?

A) Only one entity's reconciliation should receive the TXM matching results; the other must be reconciled manually
B) Configure the integration so that intercompany match results flow as detail items into both entities' RCM reconciliations, with each entity seeing the match from its own perspective (payable matched for Entity A, receivable matched for Entity B)
C) Create a third reconciliation entity that combines both perspectives
D) Perform intercompany matching outside OneStream and import results manually

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For intercompany scenarios, the integration should flow match results to both participating entities' reconciliations. Entity A's intercompany payable reconciliation shows that its transaction matched against Entity B's receivable. Entity B's reconciliation shows the reciprocal view. This dual-sided integration ensures both entities benefit from automated matching, and both preparers have visibility into the intercompany relationship. Creating a third entity (C) adds unnecessary complexity, and one-sided results (A) leave one entity without automated support.
</details>

---

### Question 32 (Medium)
**301.3.1** | Difficulty: Medium

What is the impact on the RCM reconciliation if TXM matching is not completed before the period close deadline?

A) The reconciliation period cannot be closed under any circumstances
B) RCM reconciliations for TXM-integrated accounts will show unmatched transactions as outstanding items; preparers can still complete and certify the reconciliation by documenting the unmatched items, but the reconciliation will reflect the incomplete matching state
C) TXM matching results are lost permanently if not completed before the deadline
D) The system automatically matches all remaining transactions to close the period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If TXM matching is incomplete at the close deadline, unmatched transactions remain as outstanding detail items on the RCM reconciliation. Preparers can still complete the reconciliation by documenting the outstanding items and providing explanations. The reconciliation accurately reflects the state of matching at that point. Outstanding items carry forward to the next period where they can be matched when additional data becomes available. The system does not force-match or discard data.
</details>

---
