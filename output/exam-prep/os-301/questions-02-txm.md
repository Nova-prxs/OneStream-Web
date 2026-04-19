# Question Bank - Section 2: Transaction Matching - TXM (28% of exam)

## Objectives
- **301.2.1:** Identify settings configurations for Transaction Matching (TXM)
- **301.2.2:** Identify options on the TXM Administration page
- **301.2.3:** Identify components of the TXM Matches page
- **301.2.4:** Identify components of the TXM Transactions page
- **301.2.5:** Identify use cases for Data Splitting

---

## Objective 301.2.1: Identify settings configurations for TXM

### Question 1 (Easy)
**301.2.1** | Difficulty: Easy

What is the primary purpose of Transaction Matching (TXM) in OneStream Financial Close?

A) To generate financial statements from raw transaction data
B) To match transactions from multiple data sources and identify discrepancies
C) To create intercompany elimination journal entries
D) To calculate foreign currency exchange rates

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Transaction Matching (TXM) is designed to match transactions from multiple data sources (such as sub-ledger to general ledger, or bank statement to cash book) and identify unmatched or discrepant items. This helps ensure data integrity and supports the reconciliation process. The other options describe different OneStream capabilities.
</details>

---

### Question 2 (Medium)
**301.2.1** | Difficulty: Medium

When configuring TXM matching rules, what does the "tolerance" setting control?

A) The maximum number of transactions that can be processed in a single batch
B) The acceptable variance threshold (amount or percentage) within which two transactions are considered a match
C) The number of days a transaction can remain unmatched before automatic deletion
D) The maximum file size for transaction import files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Tolerance settings define the acceptable variance when comparing transactions. This can be configured as an absolute amount, a percentage, or both. For example, a tolerance of $0.50 means two transactions that differ by $0.50 or less can still be matched. This accommodates rounding differences, timing variances, and minor discrepancies between source systems.
</details>

---

### Question 3 (Easy)
**301.2.1** | Difficulty: Easy

Which of the following is a valid match type in TXM?

A) One-to-One
B) Cube-to-Cube
C) Entity-to-Entity
D) Dimension-to-Dimension

<details>
<summary>Show answer</summary>

**Correct answer: A)**

One-to-One is a standard match type in TXM where a single transaction from one data source is matched to a single transaction from another source. TXM also supports One-to-Many and Many-to-Many match types. The other options (B, C, D) are not valid TXM match types.
</details>

---

### Question 4 (Medium)
**301.2.1** | Difficulty: Medium

In TXM settings, what is the difference between a One-to-Many and a Many-to-Many match type?

A) One-to-Many matches one source transaction to multiple target transactions; Many-to-Many matches multiple source transactions to multiple target transactions
B) One-to-Many is for intercompany matching; Many-to-Many is for bank reconciliation
C) One-to-Many uses exact matching only; Many-to-Many allows tolerance-based matching
D) There is no functional difference; they are interchangeable terms

<details>
<summary>Show answer</summary>

**Correct answer: A)**

One-to-Many matching pairs a single transaction from one data source with multiple transactions from another source (e.g., one payment matching to several invoices). Many-to-Many matching allows multiple transactions from both sources to be grouped and matched together (e.g., multiple payments matched to multiple invoices where the totals agree). The match type is determined by the business scenario, not the matching method.
</details>

---

### Question 5 (Hard)
**301.2.1** | Difficulty: Hard

An administrator is configuring TXM matching rules for bank reconciliation. The bank data includes transaction dates that may differ from the GL posting dates by up to 3 business days, and amounts may vary by up to $0.05 due to rounding. Which configuration best addresses both requirements?

A) Set a date tolerance of 3 calendar days and an amount tolerance of $0.05 on the matching rule
B) Configure only the amount tolerance and manually review all date discrepancies
C) Set a date tolerance of 3 business days and an amount tolerance of $0.05, ensuring the matching rule accounts for both date range and amount variance
D) Import all transactions with a standardized date to eliminate date differences

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The optimal configuration sets both a date tolerance (accounting for business days rather than just calendar days to handle weekends/holidays) and an amount tolerance to handle rounding differences. This combined approach catches legitimate matches that have minor variances in both timing and amount. Option A uses calendar days which may not account for weekends. Option B ignores date tolerance. Option D loses important date information.
</details>

---

## Objective 301.2.2: Identify options on the TXM Administration page

### Question 6 (Easy)
**301.2.2** | Difficulty: Easy

On the TXM Administration page, which option allows an administrator to define the data sources used for transaction matching?

A) Match Execution
B) Source Configuration
C) Report Builder
D) Period Lock

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Source Configuration on the TXM Administration page is where administrators define the data sources (such as GL data, bank statements, or sub-ledger extracts) that will be used in transaction matching. This includes specifying source file formats, field mappings, and source identifiers. Match Execution (A) runs the matching process, while Report Builder (C) and Period Lock (D) serve other purposes.
</details>

---

### Question 7 (Medium)
**301.2.2** | Difficulty: Medium

From the TXM Administration page, an administrator needs to configure auto-match rules. Which statement about auto-match is correct?

A) Auto-match can only use exact matching with no tolerance
B) Auto-match applies predefined matching rules automatically to incoming transactions, reducing manual matching effort
C) Auto-match replaces the need for any manual review or intervention
D) Auto-match can only be executed once per period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Auto-match applies predefined matching rules automatically when transactions are loaded or when the matching process is executed. It can include tolerance settings and multiple matching criteria. However, it does not eliminate the need for manual review (C) since some transactions may not meet auto-match criteria and will require manual matching. It can also be run multiple times (D) and supports tolerance-based matching (A).
</details>

---

### Question 8 (Medium)
**301.2.2** | Difficulty: Medium

What is the purpose of match rule prioritization on the TXM Administration page?

A) To determine which users can execute matching rules
B) To define the order in which matching rules are evaluated, with higher-priority rules applied first
C) To assign risk ratings to different types of transactions
D) To schedule matching jobs at different times of day

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Match rule prioritization determines the sequence in which rules are evaluated during the matching process. Higher-priority rules are applied first, meaning stricter or more specific rules can take precedence over broader, more lenient rules. This ensures that the best possible matches are made first before less certain matches are attempted.
</details>

---

### Question 9 (Hard)
**301.2.2** | Difficulty: Hard

An administrator has configured three auto-match rules for bank reconciliation with different priority levels. Rule 1 (highest priority) requires exact amount match and same date. Rule 2 requires exact amount match with a 2-day date tolerance. Rule 3 allows a $0.10 amount tolerance with a 5-day date tolerance. After running auto-match, some transactions matched by Rule 3 are flagged by auditors as potentially incorrect. What is the recommended administrative action?

A) Disable Rule 3 entirely and require manual matching for all remaining transactions
B) Review Rule 3 matches, adjust tolerance thresholds if needed, and consider adding additional matching criteria (such as reference number) to improve match quality
C) Increase Rule 3 priority to run before Rules 1 and 2
D) Remove all tolerance-based rules and use only exact matching

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is to review the matches produced by Rule 3 to understand which are correct and which are false positives, then refine the rule. Adding additional matching criteria (such as reference number or description matching) can improve accuracy without eliminating tolerance-based matching entirely. Disabling the rule (A) or removing tolerances (D) would increase manual work unnecessarily. Increasing priority (C) would make the problem worse by applying the loosest rule first.
</details>

---

## Objective 301.2.3: Identify components of the TXM Matches page

### Question 10 (Easy)
**301.2.3** | Difficulty: Easy

On the TXM Matches page, what information is displayed for each matched transaction pair?

A) Only the transaction amounts from both sources
B) The matched transactions from both sources including amounts, dates, references, match type, and match status
C) Only the match status (matched or unmatched)
D) The GL account code and nothing else

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The TXM Matches page displays comprehensive information about matched transaction pairs, including the transactions from both data sources, their amounts, dates, reference numbers, the match type used (One-to-One, One-to-Many, etc.), the match status, and whether the match was automatic or manual. This complete view supports review and audit requirements.
</details>

---

### Question 11 (Medium)
**301.2.3** | Difficulty: Medium

On the TXM Matches page, a user notices a match flagged with a "Tolerance" indicator. What does this mean?

A) The match has exceeded the system's processing capacity
B) The matched transactions have a variance within the configured tolerance threshold but are not an exact match
C) The match was manually forced by an administrator bypassing all rules
D) The match is pending approval from an external auditor

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A "Tolerance" indicator means the matched transactions were paired because their values fell within the configured tolerance range but were not exactly equal. For example, if the tolerance is $0.10 and one transaction is $100.00 while the other is $100.08, they would be matched with a tolerance indicator showing the $0.08 variance. This provides transparency for reviewers.
</details>

---

### Question 12 (Medium)
**301.2.3** | Difficulty: Medium

What actions can a user perform on the TXM Matches page regarding existing matches?

A) Only view matches; no modifications are allowed
B) Unmatch previously matched transactions, add comments, and approve or reject matches
C) Modify the original transaction amounts to force a match
D) Delete the source transactions from the database

<details>
<summary>Show answer</summary>

**Correct answer: B)**

On the Matches page, authorized users can unmatch transactions that were incorrectly paired (whether by auto-match or manual match), add comments to document matching decisions, and approve or reject matches as part of the review workflow. Users cannot modify original transaction amounts (C) as this would compromise data integrity, and transaction deletion (D) is handled elsewhere with appropriate controls.
</details>

---

## Objective 301.2.4: Identify components of the TXM Transactions page

### Question 13 (Easy)
**301.2.4** | Difficulty: Easy

What is the primary function of the TXM Transactions page?

A) To configure matching rules and tolerance settings
B) To view, filter, and manage individual transactions from imported data sources
C) To generate consolidated financial reports
D) To administer user security permissions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The TXM Transactions page provides a detailed view of all imported transactions, allowing users to view, filter, sort, and manage individual transactions. Users can see transaction details, identify unmatched items, and initiate manual matching from this page. Configuration (A), reporting (C), and security (D) are handled on other pages.
</details>

---

### Question 14 (Medium)
**301.2.4** | Difficulty: Medium

On the TXM Transactions page, a user filters to show only unmatched transactions. Which of the following actions can be performed on unmatched transactions?

A) Automatically delete them after 30 days
B) Manually select transactions from different sources and create a manual match
C) Change the transaction's source system classification
D) Convert unmatched transactions into journal entries automatically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

From the Transactions page, users can select unmatched transactions from different data sources and manually create matches when the auto-match process has not paired them. This is the primary workflow for handling exceptions that do not meet automated matching criteria. Automatic deletion (A) does not occur, reclassification (C) is not a standard feature, and automatic journal entry creation (D) is not performed from this page.
</details>

---

### Question 15 (Easy)
**301.2.4** | Difficulty: Easy

What information is typically displayed for each transaction on the TXM Transactions page?

A) Only the transaction amount
B) Transaction date, amount, reference, description, source, and match status
C) Only the matched partner transaction
D) The approval history of the transaction

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Transactions page displays comprehensive details for each imported transaction including the transaction date, amount, reference number, description or narrative, the data source it came from, and its current match status (matched, unmatched, or partially matched). This allows users to quickly identify and investigate transactions.
</details>

---

## Objective 301.2.5: Identify use cases for Data Splitting

### Question 16 (Medium)
**301.2.5** | Difficulty: Medium

What is the purpose of Data Splitting in the context of TXM?

A) To divide large data files into smaller chunks for faster upload
B) To split a single transaction into multiple component transactions to enable matching against multiple counterpart transactions
C) To separate matched and unmatched transactions into different reports
D) To partition the database for improved query performance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Splitting allows a single transaction to be broken into multiple component transactions so they can be individually matched against counterpart transactions. For example, a lump-sum bank deposit may need to be split into individual payment amounts to match against separate invoices. This is distinct from file splitting (A), report filtering (C), or database partitioning (D).
</details>

---

### Question 17 (Hard)
**301.2.5** | Difficulty: Hard

A company receives a single bank deposit of $15,000 that represents three separate customer payments of $5,000, $4,500, and $5,500. These three payments exist as individual transactions in the sub-ledger. Which TXM approach correctly handles this scenario?

A) Create a tolerance rule of $10,000 to match the $15,000 deposit against any single sub-ledger transaction
B) Use Data Splitting to split the $15,000 bank transaction into three component transactions ($5,000, $4,500, $5,500) and match each against the corresponding sub-ledger entry
C) Manually adjust the three sub-ledger transactions to each show $5,000 so they match evenly
D) Ignore the bank deposit and reconcile only the sub-ledger transactions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Splitting is the correct approach here. The $15,000 bank deposit is split into its three component amounts ($5,000, $4,500, and $5,500), and each component is then matched against the corresponding sub-ledger payment. This maintains data integrity and provides a clear audit trail. A large tolerance (A) would create false matches, adjusting source data (C) compromises integrity, and ignoring the deposit (D) leaves the reconciliation incomplete.
</details>

---

### Question 18 (Medium)
**301.2.5** | Difficulty: Medium

In which of the following scenarios is Data Splitting NOT an appropriate solution?

A) A single payment in the bank statement covers multiple vendor invoices
B) A consolidated wire transfer needs to be broken into individual subsidiary payments
C) Two transactions from the same source with identical amounts need to be distinguished
D) A payroll batch payment needs to be split into individual employee payments for matching

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Data Splitting is used to break a single transaction into component parts for matching purposes. Two separate transactions with identical amounts from the same source do not require splitting; they require additional matching criteria (such as reference numbers or dates) to distinguish them. Scenarios A, B, and D all involve decomposing a single aggregated transaction into its component parts, which is the core use case for Data Splitting.
</details>

---

### Question 19 (Easy)
**301.2.1** | Difficulty: Easy

In TXM, what is an "auto-match" rule?

A) A rule that automatically deletes unmatched transactions after a set period
B) A predefined rule that the system applies automatically to pair transactions from different sources without manual intervention
C) A rule that automatically creates new transactions to fill gaps in the data
D) A rule that sends automatic email notifications when matches are found

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An auto-match rule is a predefined matching rule configured by an administrator that the system applies automatically during the matching process. When transactions from different data sources meet the rule's criteria (such as matching amounts, dates, and references within defined tolerances), they are paired without requiring a user to manually select and match them. This significantly reduces manual effort during the close process.
</details>

---

### Question 20 (Medium)
**301.2.1** | Difficulty: Medium

When configuring a TXM matching rule, what is the purpose of specifying matching criteria fields?

A) To determine the font style used when displaying matched transactions
B) To define which transaction attributes (such as amount, date, reference number, or description) must agree between two transactions for them to be considered a match
C) To set the maximum number of users who can view matched results
D) To specify the database table where matches are stored

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Matching criteria fields define the specific transaction attributes that the system evaluates when determining whether two transactions should be paired. Common criteria include amount, transaction date, reference or check number, and description. Administrators can require exact matches on some fields while applying tolerance on others, creating a multi-dimensional matching logic that reflects real-world business scenarios.
</details>

---

### Question 21 (Medium)
**301.2.1** | Difficulty: Medium

What is the difference between "amount tolerance" and "percentage tolerance" in TXM matching rules?

A) There is no difference; both terms mean the same thing
B) Amount tolerance defines a fixed currency threshold (e.g., $0.50), while percentage tolerance defines a proportional threshold (e.g., 0.5% of the transaction amount)
C) Amount tolerance applies to credits only; percentage tolerance applies to debits only
D) Amount tolerance is for domestic transactions; percentage tolerance is for international transactions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Amount tolerance specifies a fixed currency value (e.g., transactions within $0.50 of each other are considered matching), while percentage tolerance specifies a proportional threshold relative to the transaction amount (e.g., transactions within 0.5% of each other match). Percentage tolerance is useful when transaction amounts vary widely, as a fixed tolerance may be too tight for large transactions and too loose for small ones. Both types can sometimes be combined in a single rule.
</details>

---

### Question 22 (Hard)
**301.2.1** | Difficulty: Hard

An administrator is designing TXM matching rules for intercompany reconciliation where transactions are recorded in different currencies. Entity A records in USD and Entity B records in EUR. Which configuration approach correctly handles this scenario?

A) Require both entities to record transactions in the same currency before loading into TXM
B) Configure the matching rule to compare amounts after applying a defined exchange rate or use a common reporting currency field, and set an appropriate tolerance to account for rounding differences in currency conversion
C) Use only description-based matching and ignore amounts entirely
D) Create separate TXM configurations with no cross-entity matching capability

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For multi-currency intercompany matching, the matching rule should compare transaction amounts after currency conversion using a defined exchange rate, or both sources should include a common reporting currency field. A tolerance setting accommodates rounding differences that naturally arise from currency conversion. Requiring a single currency (A) may not be feasible, ignoring amounts (C) produces unreliable matches, and separate configurations (D) defeat the purpose of intercompany matching.
</details>

---

### Question 23 (Easy)
**301.2.2** | Difficulty: Easy

On the TXM Administration page, what is the purpose of "Source Definitions"?

A) To define the visual layout of the TXM dashboard
B) To specify the data sources, file formats, and field mappings used to import transaction data into TXM
C) To list all users who have access to TXM
D) To define the programming language used for custom scripts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Source Definitions on the TXM Administration page specify each data source that feeds transactions into TXM. Each definition includes the file format (CSV, fixed-width, XML, etc.), field mapping (which columns or fields correspond to which TXM transaction attributes), date and number formatting, and other parsing parameters. This ensures that data from diverse source systems is correctly imported and standardized for matching.
</details>

---

### Question 24 (Medium)
**301.2.2** | Difficulty: Medium

From the TXM Administration page, how can an administrator monitor the results of the most recent auto-match execution?

A) By checking the OneStream server event logs in the operating system
B) By reviewing the match execution results or match statistics, which show the number of transactions matched, unmatched, and any errors encountered
C) By asking individual users to report their matching results via email
D) By running a separate SQL query against the database

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The TXM Administration page provides match execution results or statistics that summarize the outcome of each auto-match run. This includes counts of successfully matched transactions, remaining unmatched transactions, transactions matched with tolerance, and any errors or exceptions. This enables administrators to quickly assess match quality and determine whether rule adjustments are needed without relying on external tools.
</details>

---

### Question 25 (Hard)
**301.2.2** | Difficulty: Hard

An administrator notices that the TXM auto-match process is producing a high number of false positives (incorrect matches) when using a Many-to-Many rule with broad tolerances. Which combination of administrative actions would most effectively reduce false positives while still capturing legitimate matches?

A) Disable auto-match entirely and require all matches to be made manually
B) Tighten the tolerance thresholds, add additional matching criteria (such as reference number or transaction description), increase the rule priority of stricter One-to-One and One-to-Many rules so they execute first, and implement a review step for tolerance-based matches
C) Increase the tolerance to capture even more potential matches
D) Remove all Many-to-Many rules and only allow One-to-One matching

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Reducing false positives requires a multi-pronged approach. Tightening tolerances reduces the variance window. Adding matching criteria (reference number, description) provides additional validation dimensions. Prioritizing stricter rules ensures the most reliable matches occur first, leaving the broader Many-to-Many rule for genuine remaining cases. A review step for tolerance matches adds human verification. This balances automation efficiency with match accuracy. Disabling auto-match (A) or removing Many-to-Many entirely (D) would increase manual work unnecessarily.
</details>

---

### Question 26 (Easy)
**301.2.3** | Difficulty: Easy

On the TXM Matches page, what does the "Match Type" column indicate?

A) The file type of the imported data source
B) Whether the match is One-to-One, One-to-Many, or Many-to-Many
C) The data type of the amount field (integer or decimal)
D) The type of user who created the match (admin or regular user)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Match Type column on the Matches page indicates the cardinality of the match: One-to-One (single transaction matched to single transaction), One-to-Many (one transaction matched to multiple counterparts), or Many-to-Many (multiple transactions matched to multiple counterparts). This information helps reviewers understand the nature of each match and assess whether the match type is appropriate for the business scenario.
</details>

---

### Question 27 (Medium)
**301.2.3** | Difficulty: Medium

On the TXM Matches page, what is the purpose of the "Match Confidence" or match quality indicator?

A) To show the percentage of system CPU used during the matching process
B) To indicate how closely the matched transactions align based on the matching criteria, helping reviewers prioritize which matches to verify
C) To display the user's confidence level that they entered data correctly
D) To measure network latency during the match process

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Match confidence or quality indicators help reviewers assess how strong each match is. Exact matches on all criteria would show the highest confidence, while tolerance-based matches or matches relying on fewer criteria would show lower confidence. This allows reviewers to focus their verification efforts on lower-confidence matches while trusting that high-confidence matches are reliable, making the review process more efficient.
</details>

---

### Question 28 (Medium)
**301.2.4** | Difficulty: Medium

On the TXM Transactions page, what is the purpose of the "Source" column for each transaction?

A) To display the programming language used to process the transaction
B) To identify which data source system the transaction originated from, enabling users to trace data lineage
C) To show the name of the user who uploaded the transaction
D) To indicate the server that processed the transaction

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Source column identifies the origin data system for each transaction (e.g., "GL System," "Bank of America," "Sub-Ledger"). This is essential for data lineage and traceability, allowing users to understand where each transaction came from and which source definition was used to import it. When investigating unmatched items, knowing the source helps determine which counterpart system to check.
</details>

---

### Question 29 (Easy)
**301.2.4** | Difficulty: Easy

On the TXM Transactions page, what does a "Partially Matched" status indicate?

A) The transaction was imported with incomplete data
B) The transaction has been matched to some but not all of its expected counterpart transactions
C) The matching process was interrupted by a system error
D) The transaction is pending approval from a supervisor

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A "Partially Matched" status indicates that a transaction has been paired with some of its counterpart transactions, but the full expected match is not yet complete. This commonly occurs in One-to-Many or Many-to-Many scenarios where, for example, a payment has been matched to two of its three expected invoices. The remaining unmatched portion still requires matching or investigation.
</details>

---

### Question 30 (Hard)
**301.2.4** | Difficulty: Hard

A user on the TXM Transactions page discovers 500 unmatched transactions after the auto-match process. Upon investigation, many appear to be duplicates loaded from the same source file that was accidentally imported twice. What is the correct approach to resolve this?

A) Manually match each duplicate transaction to its counterpart to clear them from the unmatched list
B) Use the TXM administration tools to identify and remove the duplicate import, then re-run the auto-match process on the corrected dataset
C) Increase the matching tolerance until the duplicates match with other transactions
D) Ignore the duplicates and proceed to close the period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach is to address the root cause by removing the duplicate import using TXM's administrative data management capabilities. Once the duplicated transactions are removed, the auto-match process should be re-run on the clean dataset. Manually matching duplicates (A) would create false matches, increasing tolerance (C) would compound the problem, and ignoring them (D) would leave the reconciliation inaccurate.
</details>

---

### Question 31 (Medium)
**301.2.5** | Difficulty: Medium

When performing a Data Split in TXM, what must the sum of the split component amounts equal?

A) The sum can be any amount; there is no requirement
B) The sum of the component amounts must equal the original transaction amount to maintain data integrity
C) The sum must be greater than the original amount to account for processing fees
D) The sum must be less than the original amount to create a tolerance buffer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When splitting a transaction, the sum of all component amounts must equal the original transaction amount. This is a fundamental data integrity control that ensures no value is created or lost during the split process. For example, splitting a $10,000 deposit into three components of $4,000, $3,500, and $2,500 is valid because they total $10,000. The system typically enforces this validation.
</details>

---

### Question 32 (Easy)
**301.2.5** | Difficulty: Easy

Which of the following best describes when Data Splitting should be used in TXM?

A) When two separate transactions need to be merged into one
B) When a single aggregated transaction from one source needs to be decomposed into individual components to match against individual transactions in another source
C) When transaction data needs to be archived
D) When the matching rule priority needs to be changed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Splitting is used when a single aggregated transaction (such as a batch payment, consolidated deposit, or summary journal entry) needs to be broken into its individual components so that each component can be matched against its corresponding individual transaction from another source. This is the opposite of merging (A) and is unrelated to archiving (C) or rule configuration (D).
</details>

---

### Question 33 (Medium)
**301.2.2** | Difficulty: Medium

On the TXM Administration page, what is the purpose of configuring "Match Groups"?

A) To group users by department for security purposes
B) To define logical groupings of accounts or transaction sets that should be matched together, enabling segmented matching processes
C) To set up user discussion groups for collaborative matching
D) To group server resources for load balancing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Match Groups allow administrators to define logical segments of accounts or transaction sets that should be processed together during matching. For example, all bank reconciliation accounts might form one match group while intercompany accounts form another. This segmentation enables targeted matching processes, allows different matching rules per group, and improves performance by limiting the transaction scope evaluated during each match run.
</details>

---

### Question 34 (Hard)
**301.2.1** | Difficulty: Hard

An organization processes thousands of transactions daily and needs to run TXM matching multiple times per day as new data arrives. The matching rules must handle incremental data loads where some transactions were matched in earlier runs. How should the TXM configuration handle this requirement?

A) Delete all existing matches before each new matching run and re-match everything from scratch
B) Configure matching rules to process only new/unmatched transactions in each run, preserving existing matches, and ensure that match rules are idempotent so repeated execution does not create duplicate matches
C) Run matching only once at end of day to avoid complexity
D) Export all data to an external system for matching and reimport the results

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For incremental matching scenarios, TXM should be configured to process only new and unmatched transactions during each run, leaving previously confirmed matches intact. The matching rules must be idempotent, meaning re-running them on already-matched transactions does not create duplicates or alter existing matches. This approach supports real-time or near-real-time reconciliation workflows. Deleting matches (A) wastes processing and risks losing approved matches. Running once daily (C) delays issue detection. External processing (D) adds unnecessary complexity.
</details>

---

### Question 35 (Medium)
**301.2.3** | Difficulty: Medium

On the TXM Matches page, what is the significance of a match flagged as "Manual"?

A) The match was created by the system during auto-match processing
B) The match was created by a user who manually selected and paired the transactions, rather than through automated matching rules
C) The match requires manual data entry to complete
D) The match was imported from an external system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A "Manual" flag indicates that a user explicitly selected the transactions and created the match, rather than the match being produced by an auto-match rule. Manual matches typically arise when transactions do not meet auto-match criteria but a knowledgeable user can determine they should be paired based on business context. These matches may receive additional scrutiny during review since they bypassed automated validation criteria.
</details>

---

### Question 36 (Easy)
**301.2.4** | Difficulty: Easy

On the TXM Transactions page, why would a user apply a date range filter?

A) To change the transaction dates in the source data
B) To narrow the visible transactions to a specific time period, making it easier to find and work with relevant transactions
C) To delete transactions outside the selected date range
D) To modify the system clock settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Date range filters on the Transactions page allow users to focus on transactions within a specific time window. This is particularly useful when working with large transaction volumes, investigating items from a particular close period, or isolating transactions around a specific event date. Filtering does not modify or delete data; it only controls which transactions are displayed in the current view.
</details>

---
