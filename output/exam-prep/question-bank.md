# OneStream OS-201 Exam - Complete Question Bank

This question bank contains **618 questions** covering all 8 sections of the OS-201: OCP OneStream Core Platform Architect exam, weighted according to exam section weights.

## Summary

| # | Section | Exam Weight | Questions |
|---|---------|:-----------:|:---------:|
| 1 | Cube | 15% | 90 |
| 2 | Workflow | 14% | 84 |
| 3 | Data Collection | 13% | 78 |
| 4 | Presentation | 14% | 84 |
| 5 | Tools | 9% | 72 |
| 6 | Security | 10% | 60 |
| 7 | Administration | 9% | 54 |
| 8 | Rules | 16% | 96 |
| | **Total** | **100%** | **618** |

## Difficulty Distribution

- **Easy**: ~20% of questions
- **Medium**: ~40% of questions
- **Hard**: ~40% of questions

---


# Question Bank - Section 1: Cube (15% of exam)

## Objectives
- **201.1.1:** Given a design specification, apply changes to Cube configuration
- **201.1.2:** Demonstrate an understanding of Cube properties that drive DUCS behavior

---

## Objective 201.1.1: Apply changes to Cube configuration

### Question 1 (Easy)
**201.1.1** | Difficulty: Easy

What is the maximum number of characters allowed for a Cube name?

A) 50 characters
B) 100 characters
C) 200 characters
D) 500 characters

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Cube name has a maximum of 100 characters. The description allows up to 200 characters. Additionally, the Cube name cannot be changed once created.
</details>

---

### Question 2 (Easy)
**201.1.1** | Difficulty: Easy

Where are Cubes created in OneStream?

A) Workflow > Cubes > New Cube
B) Application > Cube > Cubes
C) System > Configuration > Cubes
D) Data Management > Cube Setup

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cubes are created in Application > Cube > Cubes and configured in the Cube Profile. The other paths do not exist in OneStream.
</details>

---

### Question 3 (Easy)
**201.1.1** | Difficulty: Easy

Where are FX Rates stored in OneStream?

A) In each individual Cube
B) In the System Cube as a central repository
C) Directly in the SQL database
D) In the Workflow Profile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

FX Rates are stored in the System Cube as a central repository that all other Cubes reference. This avoids duplicating rates in each Cube.
</details>

---

### Question 4 (Easy)
**201.1.1** | Difficulty: Easy

What are the 4 predefined FX Rate Types in OneStream?

A) Spot Rate, Forward Rate, Budget Rate, Actual Rate
B) Average Rate, Opening Rate, Closing Rate, Historical Rate
C) Daily Rate, Weekly Rate, Monthly Rate, Yearly Rate
D) Base Rate, Market Rate, Internal Rate, External Rate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The 4 predefined FX Rate Types are: Average Rate (average rate for the period), Opening Rate (rate at the beginning of the period), Closing Rate (rate at the end of the period), and Historical Rate (rate from a specific historical date). Additional Rate Types can be created in Cube > FX Rates.
</details>

---

### Question 5 (Easy)
**201.1.1** | Difficulty: Easy

How many Dimensions does a Cube have in OneStream?

A) 8
B) 12
C) 16
D) 18

<details>
<summary>Show answer</summary>

**Correct answer: D)**

A Cube in OneStream has 18 Dimensions shaped through Extensible Dimensionality. These include Entity, Scenario, Account, Flow, UD1-UD8, Parent, IC, Time, Consolidation, Origin, and View.
</details>

---

### Question 6 (Easy)
**201.1.1** | Difficulty: Easy

What Account Type should be used for equity accounts in OneStream?

A) Equity
B) Liability
C) Balance
D) NonFinancial

<details>
<summary>Show answer</summary>

**Correct answer: B)**

There is no "Equity" Account Type in OneStream. For equity accounts, the Liability type must be used. This is an important distinction to memorize for the exam.
</details>

---

### Question 7 (Easy)
**201.1.1** | Difficulty: Easy

What is the maximum number of characters allowed for a Cube description?

A) 100 characters
B) 200 characters
C) 500 characters
D) 1,000 characters

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Cube description allows a maximum of 200 characters. Unlike the Cube name (max 100 characters), the description can be changed at any time after creation.
</details>

---

### Question 8 (Easy)
**201.1.1** | Difficulty: Easy

Which of the following is true about the Standard Time Profile in OneStream?

A) It can be renamed to match the company's fiscal calendar
B) It can be deleted if a custom Time Profile is created
C) It follows a January-to-December calendar and cannot be deleted or renamed
D) It is only used for Actual Scenarios

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Standard Time Profile follows a January-to-December calendar. It cannot be deleted or renamed. Additional custom Time Profiles can be created for different fiscal years, but the Standard profile always remains.
</details>

---

### Question 9 (Easy)
**201.1.1** | Difficulty: Easy

In the View Dimension, where is all data physically stored in the database?

A) V#Periodic
B) V#YTD
C) V#MTD
D) V#QTD

<details>
<summary>Show answer</summary>

**Correct answer: B)**

All data is always stored in V#YTD in the database. Periodic, MTD, QTD, and HTD values are calculated dynamically (included calculations) when queried and are not stored separately.
</details>

---

### Question 10 (Easy)
**201.1.1** | Difficulty: Easy

What is the maximum number of characters allowed for a Dimension Member name?

A) 100 characters
B) 200 characters
C) 500 characters
D) 1,000 characters

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Dimension Member names can be up to 500 characters. Names must be unique within a Dimension Type. It is recommended to use underscores instead of spaces and dots to avoid the need for bracket notation.
</details>

---

### Question 11 (Easy)
**201.1.1** | Difficulty: Easy

What is the default Consolidation Algorithm Type for a Cube?

A) Stored Share
B) Custom
C) Org-By-Period Elimination
D) Standard (Calc-on-the-fly Share and Hierarchy Elimination)

<details>
<summary>Show answer</summary>

**Correct answer: D)**

The default Consolidation Algorithm Type is Standard (Calc-on-the-fly Share and Hierarchy Elimination). It is the most commonly used type and only in rare circumstances would a different type be selected.
</details>

---

### Question 12 (Easy)
**201.1.1** | Difficulty: Easy

Which Cube property determines who can view the Cube?

A) Maintenance Group
B) Access Group
C) Use Parent Security
D) Data Cell Access Security

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Access Group determines who can view the Cube and is the second layer of security after application access. The default value is "Everyone." The Maintenance Group controls who can edit the Cube configuration, which is a separate property.
</details>

---

### Question 13 (Medium)
**201.1.1** | Difficulty: Medium

An architect needs different Cubes to share Dimensions but with different Constraints. Which Cube property facilitates this?

A) Time Dimension Profile
B) Cube Type
C) Access Group
D) Consolidation Algorithm Type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Type is an optional label that allows grouping similar Cubes. Its main function is to allow different Cubes to share Dimensions with different Constraints by Cube Type. Available Cube Types include Standard, Tax, Treasury, Supplemental, What If, and CubeType1-8.
</details>

---

### Question 14 (Medium)
**201.1.1** | Difficulty: Medium

Which statement about the "Is Top Level Cube for Workflow" property is correct?

A) Multiple Cubes can have this property set to True
B) Only one Cube can have this property set to True, and it is required for creating Workflow Profiles
C) This property only affects the Cube's security
D) It can be changed freely at any time without restrictions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Only one Cube can have "Is Top Level Cube for Workflow = True." This property is necessary for the Cube to create and maintain a Workflow Profile structure through a Cube Root Profile.
</details>

---

### Question 15 (Medium)
**201.1.1** | Difficulty: Medium

What is the default configuration of Rule Types for Revenue/Expenses and Assets/Liabilities in FX translation?

A) Revenue/Expenses = Direct, Assets/Liabilities = Periodic
B) Revenue/Expenses = Periodic, Assets/Liabilities = Direct
C) Both use Direct
D) Both use Periodic

<details>
<summary>Show answer</summary>

**Correct answer: B)**

By default, Revenue/Expenses use the Periodic Rule Type (which considers the translation from prior periods) and Assets/Liabilities use Direct (local value multiplied by the current period rate). This configuration reflects standard accounting practices.
</details>

---

### Question 16 (Medium)
**201.1.1** | Difficulty: Medium

What does "Is Consolidated = Conditional (True if no Formula Type)" mean for an Account Member?

A) It always consolidates data to the Parent
B) It consolidates only if the Account has no Formula Type assigned; calculated data does NOT consolidate
C) It consolidates conditionally based on the Scenario
D) It consolidates only during Force Consolidate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Conditional (True if no Formula Type)" means that input data consolidates, but data generated by Member Formulas does NOT consolidate. If you want calculated data to also consolidate, you must explicitly change this property to True. This is a common implementation error.
</details>

---

### Question 17 (Medium)
**201.1.1** | Difficulty: Medium

An Entity member is named "Quebec.City" and needs to be referenced in a formula. What is the correct syntax?

A) E#Quebec.City
B) E#[Quebec.City]
C) E#"Quebec.City"
D) E#Quebec_City

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Dimension Member names include spaces or dots, brackets must be used. The correct syntax is E#[Quebec.City]. The general recommendation is to use underscores instead of spaces and dots to avoid this requirement.
</details>

---

### Question 18 (Medium)
**201.1.1** | Difficulty: Medium

**Scenario:** A company needs its Forecast Scenario to use exchange rates from a specific year (Constant Currency). How should this be configured?

A) Create a new FX Rate Type called "ConstantRate"
B) In the Scenario, set "Use Cube FX Settings = False" and configure "Constant Year for FX Rates"
C) Modify the FX Rates in the System Cube directly
D) Use a Custom Translation Algorithm Type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Constant Currency, configure the Scenario Member: set "Use Cube FX Settings = False" to enable custom configuration, then use the "Constant Year for FX Rates" property to specify the year whose rates will be used. This is a native feature that requires no Business Rules or modifications to the System Cube.
</details>

---

### Question 19 (Medium)
**201.1.1** | Difficulty: Medium

In Cube Dimensions configuration, in which Scenario Type can the Entity and Scenario Dimensions be assigned?

A) In any Scenario Type
B) Only in the (Default) Scenario Type
C) Only in the Actual Scenario Type
D) They are assigned per Cube, not per Scenario Type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Entity and Scenario Dimensions can only be assigned in the (Default) Scenario Type. In all other Scenario Types, these dimensions are grayed out and cannot be changed. This ensures consistency across all Scenario Types.
</details>

---

### Question 20 (Medium)
**201.1.1** | Difficulty: Medium

What is the recommended best practice for unused Dimension Types in Cube Dimensions configuration?

A) Leave them set to "(Use Default)"
B) Assign RootXXXDim to each unused Dimension Type per Scenario Type
C) Delete the unused Dimension Types
D) Assign the same dimension as the Default Scenario Type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended best practice is to assign RootXXXDim to unused Dimension Types in each Scenario Type (not "Use Default"). This allows future flexibility — you can later change from RootXXXDim to a specific dimension. If you leave "Use Default" and load data, the Dimension Type becomes permanently locked to whatever the Default has.
</details>

---

### Question 21 (Medium)
**201.1.1** | Difficulty: Medium

Which Account Member property determines whether an intercompany transaction generates an elimination?

A) Is IC Account
B) Plug Account
C) Aggregation Weight
D) Allow Input

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The "Is IC Account" property determines if the Account participates in IC eliminations. Its options are: Conditional (True if Entity not same as IC), True, or False. The Plug Account is a different property that selects the account where uneliminated IC transactions are posted.
</details>

---

### Question 22 (Medium)
**201.1.1** | Difficulty: Medium

What integer value is stored in the database for the "Tax" Cube Type?

A) 0
B) 1
C) 2
D) 11

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Types are stored as integers in the dbo.Cube table. The mapping is: Standard=0, Tax=1, Treasury=2, Supplemental=3, What If=4, CubeType1=11 through CubeType8=18. The Cube Type names are functionally irrelevant and do not change system behavior.
</details>

---

### Question 23 (Medium)
**201.1.1** | Difficulty: Medium

What is the difference between Percent Consolidation and Percent Ownership on an Entity relationship?

A) They are interchangeable and have the same effect
B) Percent Consolidation directly affects consolidation math; Percent Ownership has no effect by itself and is only used in Business Rules
C) Percent Ownership affects consolidation; Percent Consolidation is only for reporting
D) Both only affect IC Elimination calculations

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Percent Consolidation is directly multiplied by entity balances during the consolidation process. Percent Ownership has no effect on consolidation by itself — it is only available for use in Business Rules for custom logic. Both properties can vary by Scenario Type and Time.
</details>

---

### Question 24 (Medium)
**201.1.1** | Difficulty: Medium

What is the default value of "Use Parent Security for Relationship Consolidation" on a Cube?

A) True — all Consolidation Members are controlled by parent security
B) False — everyone with access to Parent Entities can access all Consolidation Members
C) Conditional — depends on the Scenario Type
D) True — only for IC entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The default is False, meaning all users with access to Parent Entities can access all Consolidation Members (Share, Elimination, OwnerPostAdj, OwnerPreAdj). When set to True, access to these relationship Consolidation Members is controlled based on the security of the Parent Entity. This is configured Cube by Cube.
</details>

---

### Question 25 (Medium)
**201.1.1** | Difficulty: Medium

In the Origin Dimension hierarchy, where do data loaded via file imports appear?

A) O#Forms
B) O#Import
C) O#AdjInput
D) O#Top

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Origin Dimension has a fixed hierarchy. Data loaded via file imports goes to O#Import, while data entered through forms goes to O#Forms. Both roll up to BeforeAdj > BeforeElim > Top. Journal entries go to O#AdjInput under the Adjustments branch.
</details>

---

### Question 26 (Medium)
**201.1.1** | Difficulty: Medium

Which Entity property, when set to False, deactivates an Entity while maintaining its historical data?

A) Is Consolidated
B) Allow Adjustments
C) In Use
D) Is IC Entity

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The "In Use" property (which varies by Scenario Type and Time) deactivates an Entity when set to False. Historical data is maintained, but the Entity is ignored during Consolidation. This is useful when entities are divested or deactivated in certain periods.
</details>

---

### Question 27 (Medium)
**201.1.1** | Difficulty: Medium

What happens when the Flow Dimension property "Switch Sign" is set to True?

A) The Account Type changes for the Flow member
B) The data sign is reversed (positive becomes negative and vice versa)
C) The consolidation direction reverses
D) The translation method changes from Direct to Periodic

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When "Switch Sign" is set to True on a Flow member, the data sign is changed for that Flow member. This is different from "Switch Type" which changes the Account type attribute (e.g., Asset to Revenue) and is useful for roll forward accounts.
</details>

---

### Question 28 (Medium)
**201.1.1** | Difficulty: Medium

How many sections of security configuration exist in the Data Access tab of a Cube?

A) 1 — Data Cell Access Security
B) 2 — Data Cell Access Security and Conditional Input
C) 3 — Data Cell Access Security, Data Cell Conditional Input, and Data Management Access Security
D) 4 — Data Cell Access, Conditional Input, DM Access, and Integration Security

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Data Access tab has three sections: (1) Data Cell Access Security ("Slice Security") which controls read/write access to groups of cells using Member Filters + Access Groups, (2) Data Cell Conditional Input which controls whether all users can input data to specific cells (no Access Group — applies to everyone), and (3) Data Management Access Security which controls access during DM processes at the Data Unit level.
</details>

---

### Question 29 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A company has a Cube with an "Actual" Scenario Type that already has data loaded. The administrator needs to change the Workflow suffix for that Scenario Type. What must they do?

A) Simply change the suffix in Cube Properties
B) Delete the Cube and recreate it with the new suffix
C) Execute a Reset Scenario before changing the suffix
D) Create a new Cube with the desired suffix and migrate the data

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Once a Scenario Type has data loaded, you cannot create a new Cube Root or change the suffix for that Scenario Type. A Reset Scenario (Data Management step) is required before making changes. Option A would fail because the system prevents it, and options B and D are unnecessarily drastic.
</details>

---

### Question 30 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A consultant is designing an application with multiple business lines that require different dimensions. The application needs to consolidate data from all lines. What is the most appropriate Cube design?

A) Monolithic Cube with all dimensions
B) Super Cube (Linked Cubes) with a parent Cube and child Cubes linked via Entity Dimension
C) Multiple independent Cubes without links
D) A single Specialty Cube with all configurations

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Super Cube (Linked Cubes) design is the most common and recommended approach. It consists of a parent Cube with child Cubes linked via the Entity Dimension, which allows better use of extensibility and reduces Data Unit sizes. A Monolithic Cube is only appropriate for very small applications. Having more Cubes does not slow down the application; on the contrary, it improves performance.
</details>

---

### Question 31 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A consultant needs certain Accounts to be translated with different rates than the Cube's default, while the rest use standard rates. Which Translation Algorithm Type should be used?

A) Standard
B) Standard Using Business Rules for FX Rates
C) Custom
D) Direct

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Standard Using Business Rules for FX Rates" allows Business Rules to specify rates for specific intersections, while unspecified intersections use the Standard translation. This is ideal for translating Actual at Budget rates or applying different rates to certain Accounts. "Custom" would be excessive since it requires complete logic for all translations, and "Standard" does not allow exceptions.
</details>

---

### Question 32 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A finance team asks an architect about the correct formula for the Periodic Rule Type in currency translation. Which is correct?

A) Translated Value = Local Value x FX Rate of Current Period
B) Translated Value = Prior Period Translated + [(Current Local - Prior Local) x Current Rate]
C) Translated Value = (Local Value + Prior Local) / 2 x Average Rate
D) Translated Value = Local Value x Opening Rate + Local Value x Closing Rate / 2

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Periodic Rule Type formula is: Translated Value = Prior Period Translated + [(Current Local - Prior Local) x Current Rate]. This weighted average method considers prior periods, unlike the Direct Rule Type which simply multiplies the local value by the current period rate (option A).
</details>

---

### Question 33 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect notices slow reports due to an excessive number of Attribute Members in a UD Dimension. How many Attribute Members is considered the performance caution threshold?

A) 500
B) 1,000
C) 2,000
D) 5,000

<details>
<summary>Show answer</summary>

**Correct answer: C)**

More than approximately 2,000 Attribute Members impacts performance. Attribute Members do not store data; they calculate on-the-fly each time they are queried (using Is Attribute Member = True, Source Member For Data, Related Dimension Type, Related Property, etc.), so an excessive number degrades report performance.
</details>

---

### Question 34 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A company uses the Stored Share Consolidation Algorithm Type. An architect needs to understand the performance implications. Which statement is accurate?

A) Stored Share has no impact on performance compared to Standard
B) Stored Share reduces the number of records in Data Record tables
C) Stored Share increases records in Data Record tables, increases Data Unit size, and requires the engine to execute custom logic and write additional data
D) Stored Share only affects IC Elimination performance

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Stored Share increases records in Data Record tables, increases the size of Data Units, and requires the engine to execute custom logic and write additional data. It is used in scenarios like minority interest where Share cannot be derived solely from Percent Consolidation and requires a Business Rule with FinanceFunctionType.ConsolidateShare.
</details>

---

### Question 35 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect is configuring a Cube where certain IC Members may not be descendants in specific periods because their Percent Consolidation is zero. Which Consolidation Algorithm Type should be used?

A) Standard
B) Stored Share
C) Org-By-Period Elimination
D) Custom

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Org-By-Period Elimination considers Percent Consolidation in each relationship of the hierarchy, making it appropriate when an IC Member may not be a descendant if Percent Consolidation equals zero. The Standard algorithm does not handle this scenario, while Custom would be unnecessarily complex.
</details>

---

### Question 36 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A user configured a UD4 Dimension Type as "(Use Default)" in the Budget Scenario Type and loaded data. Now they want to assign a new Customer dimension to UD4 for Budget only. What is the expected result?

A) The change can be made freely since no specific dimension was assigned
B) The UD4 is permanently locked to whatever the Default has; a Reset Scenario is required to change it
C) The change can only be made by an administrator
D) The system will automatically migrate the data to the new dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When "(Use Default)" is used and data is loaded, the Dimension Type becomes permanently locked to whatever the Default Scenario Type has. The user must perform a Reset Scenario before making changes. This is why the recommended best practice is to always assign RootXXXDim to unused Dimension Types instead of leaving "(Use Default)."
</details>

---

### Question 37 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A company wants to set up an Entity to receive journal entries for consolidation adjustments. The "Allow Adjustments" property on the Entity is set to False. What will happen?

A) Journal entries can still be posted but won't consolidate
B) The Journals module for AdjInput will not be available for that Entity
C) Only Consolidation adjustments will be blocked
D) The Entity will be excluded from all Workflow processes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When "Allow Adjustments" is set to False, the Journals module for AdjInput is disabled for that Entity. The default value is True, which enables the Journals module. This is separate from "Allow Adjustments From Children" which controls whether OwnerPostAdj and OwnerPreAdj can be received from direct children.
</details>

---

### Question 38 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect needs to ensure that FX rate triangulation works correctly for their multi-currency application. What is the key requirement for triangulation to function?

A) All entities must use the same currency
B) FX rates must include the application's default currency
C) The Custom Translation Algorithm must be used
D) Separate FX Rate Types must be created for each currency pair

<details>
<summary>Show answer</summary>

**Correct answer: B)**

FX rate triangulation (deriving cross-rates) only works if the rates include the application's default currency. The Cube's Default Currency is used for triangulation and IC Matching. Without rates referencing the default currency, the system cannot derive cross-rates between other currency pairs.
</details>

---

### Question 39 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A company is planning their OneStream implementation. The finance team wants to use a fiscal year starting April 1st. When must the Time Dimension be configured?

A) After the application is created, in Application > Cube > Time
B) Before the application is created; it cannot be altered after creation
C) At any time; Time Profiles can be changed freely
D) During the first data load process

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Time Dimension must be customized BEFORE the application is created and CANNOT be altered after. If a different Time Dimension is needed later, a new application must be created. Custom Time Profiles for fiscal years different from the Standard January-December calendar must be set up before application creation.
</details>

---

### Question 40 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect is designing a Cube for a planning solution and needs to enable data entry in a currency different from an Entity's local currency for specific accounts. What configuration is required?

A) Change the Entity's Currency property to the desired input currency
B) Set "Use Alternate Input Currency In Flow = True" on the Account, and configure the Flow member's Flow Processing Type
C) Create a separate Cube for each currency
D) Use a Custom Translation Algorithm

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To input data in an alternate currency, set "Use Alternate Input Currency In Flow = True" on the Account, then configure the Flow member with the appropriate Flow Processing Type (such as "Is Alternate Input Currency"), select the Alternate Input Currency, and set the Source Member for Alternate Input Currency. This is a native feature that does not require separate Cubes or custom algorithms.
</details>

---

### Question 41 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A consultant has configured a Cube with Linked Cubes (Super Cube design). Where do the Cube References appear when Entity Dimensions of child Cubes create relationships with the parent Cube's Entity Dimension?

A) In the child Cube's Cube Properties tab
B) Automatically in the parent Cube's Cube References tab
C) In the Workflow Profile configuration
D) In the Integration tab of each child Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Entity Dimensions of child Cubes create relationships with the Entity Dimension of the parent Cube, the references appear automatically in the parent Cube's Cube References tab. This ensures that data consolidates properly within the parent Cube's hierarchy.
</details>

---

### Question 42 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An implementation team is debating how to handle UD Dimensions in their design. They have a large Product dimension (5,000+ members) and a small Region dimension (50 members). Which best practice should they follow for optimal performance?

A) Assign Products to UD8 and Regions to UD1
B) Assign Products to UD1 or UD2 and Regions to a higher UD number
C) Create separate Cubes for Products and Regions
D) The assignment order does not matter for performance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to assign larger and more significant dimensions to UD1 or UD2 for performance reasons. Performance is based on records stored, not on the number of possible intersections. Therefore, the large Product dimension should go to UD1 or UD2, while the smaller Region dimension should go to a higher-numbered UD.
</details>

---

### Question 43 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A company wants to restrict data entry for all users on specific Account/Entity intersections, regardless of their security group. Which section of the Data Access tab should be used?

A) Data Cell Access Security
B) Data Cell Conditional Input
C) Data Management Access Security
D) Application Security Roles

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Cell Conditional Input controls whether all users can input data to specific cells. It has the same properties as Data Cell Access Security except it does not have an Access Group — it applies to all users equally. Data Cell Access Security uses Access Groups to control access per group, while Data Management Access Security is for DM processes at the Data Unit level.
</details>

---

### Question 44 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect is setting up a Scenario with quarterly input. The Workflow Tracking Frequency was set to "Monthly" and some workflow steps have already been processed. The business now wants to change it to "Quarterly." What is the expected outcome?

A) The change can be made immediately in Scenario Properties
B) The Workflow Tracking Frequency cannot be changed because steps have been processed
C) Only an administrator can make the change
D) The change requires deleting all existing Workflow Profiles

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workflow Tracking Frequency cannot be changed if workflow steps have already been processed. This is a critical constraint to be aware of during initial configuration, as changing it later requires resetting the affected workflow steps first.
</details>

---

### Question 45 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect needs to configure an Entity so that two holding companies with overlapping subsidiaries don't double-count data when both parents consolidate the same child. What Entity relationship property should be adjusted?

A) Percent Consolidation
B) Ownership Type
C) Aggregation Weight
D) Sibling Consolidation Pass

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Aggregation Weight controls the weight of aggregation to the parent. Setting Aggregation Weight to 0 on one of the parent relationships prevents double counting when an entity appears in multiple hierarchies. The default is 1 (100%), and setting it to 0 means the entity's data does not aggregate to that particular parent.
</details>

---

### Question 46 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect is configuring the four types of Extensible Dimensionality in their application. Which option correctly describes the four types?

A) Vertical, Horizontal, Diagonal, Temporal
B) Vertical (multiple Cubes), Horizontal (UD dimensions), Workflow (Scenario Types), Platform (Cube Types)
C) Entity, Account, Scenario, Time
D) Static, Dynamic, Hybrid, Custom

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four types of Extensible Dimensionality are: Vertical (multiple Cubes to partition data), Horizontal (UD1-UD8 dimensions for additional analysis axes), Workflow (Scenario Types to support different business processes), and Platform (Cube Types to allow different Constraints per Cube).
</details>

---

### Question 47 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A company needs to disable unused Dimension Types in the Integration tab so they don't appear in Data Sources or Transformation Rules. Which statement about the Integration tab is correct?

A) Dimension Types can be deleted from the Integration tab
B) Dimension Types can be disabled (Enabled = False) so they don't appear in Data Sources or Transformation Rules
C) The Integration tab only controls Cube Dimensions, not Label Dimensions
D) Changes in the Integration tab require a Cube rebuild

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In the Integration tab, each Dimension Type has an "Enabled" property. Setting it to False prevents the Dimension Type from appearing in Data Sources and Transformation Rules. The Integration tab also manages Label Dimensions (Label, SourceID, TextValue) and Attribute Dimensions for BI Blend.
</details>

---

### Question 48 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An architect needs to configure a Scenario where the Default View is "Periodic." What is the impact of the "Retain Next Period Data Using Default View" property when set to True?

A) It has no effect when Default View is Periodic
B) Future periods will change if a prior period is altered
C) It prevents data from being entered in future periods
D) It locks all periods after the current one

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When "Retain Next Period Data Using Default View" is set to True, future periods will change if a prior period is altered. This property works in conjunction with the Default View setting (Periodic or YTD) to control how data flows between periods and affects calculations and data clearing behavior.
</details>

---

### Question 49 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** An administrator needs to lock FX Rates to prevent accidental changes during a reporting period. Which security roles are specifically related to managing FX Rate locks?

A) ApplicationAdmin and DataAdmin
B) ManageFXRates, LockFxRates, and UnLockFxRates
C) CubeAdmin and WorkflowAdmin
D) ReadOnly and ReadWrite

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The specific security roles for FX Rate management are: ManageFXRates, LockFxRates, and UnLockFxRates. These are found in Application > Security Roles. When FX Rates are locked, File Loads or XFSetRate functions will fail. Administrators have full rights by default. A Task Activity record is generated for auditing when rates are locked or unlocked.
</details>

---

### Question 50 (Hard)
**201.1.1** | Difficulty: Hard

**Scenario:** A company is deciding between Paired Cubes and Specialty Cubes for their design. Which description correctly differentiates these two Cube design options?

A) Paired Cubes use Linked Cubes; Specialty Cubes use Monolithic design
B) Paired Cubes allow split/shared entities across multiple Cubes; Specialty Cubes are limited monolithic Cubes for drivers or admin
C) Paired Cubes are for Planning; Specialty Cubes are for Consolidation
D) There is no difference between them

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Paired Cubes allow the same Entity to exist in multiple Cubes (split/shared entities), while Specialty Cubes are limited monolithic Cubes used for specific purposes like driver cubes or admin cubes. The Super Cube (Linked Cubes) design remains the most common overall approach.
</details>

---

## Objective 201.1.2: Demonstrate an understanding of Cube properties that drive DUCS behavior

### Question 51 (Easy)
**201.1.2** | Difficulty: Easy

What is a Data Unit in OneStream?

A) A single data cell at the intersection of all 18 Dimensions
B) All data cells within a unique combination of Cube, Scenario, Time, Consolidation, Entity, and Parent Dimensions
C) A collection of Business Rules assigned to a Cube
D) A report that displays consolidated data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Data Unit is defined as all data cells within a unique combination of Cube, Scenario, Time, Consolidation, Entity, and Parent Dimensions. It acts as a grouping mechanism that breaks down Cube data into smaller units that can be read or processed at the same time by the server and fit into server memory. The Finance Engine processes all data within each Data Unit at once.
</details>

---

### Question 52 (Easy)
**201.1.2** | Difficulty: Easy

At what number of records in a single Data Unit does OneStream documentation consider a "red flag"?

A) 500,000
B) 1,000,000
C) 2,000,000
D) 5,000,000

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Data Units exceeding approximately 1 million records warrant inspection. More than 2 million records is considered a "red flag" and indicates a design issue that should be addressed. The sweet spot for OneStream Data Units is between 250,000 and 500,000 records.
</details>

---

### Question 53 (Easy)
**201.1.2** | Difficulty: Easy

What is the maximum number of Finance Business Rules that can be assigned to a single Cube?

A) 4
B) 8
C) 12
D) 16

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In Cube Properties, up to 8 Finance Business Rules can be assigned to the Cube. These Business Rules execute in pairs (1-2, 3-4, 5-6, 7-8) within the DUCS, interleaved with Formula Passes.
</details>

---

### Question 54 (Easy)
**201.1.2** | Difficulty: Easy

What Account Type and Formula Type combination is required to make an Account Member a DynamicCalc member?

A) Only the Formula Type needs to be set to DynamicCalc
B) Only the Account Type needs to be set to DynamicCalc
C) Both the Account Type AND the Formula Type must be set to DynamicCalc
D) The Account Type must be Revenue and the Formula Type must be DynamicCalc

<details>
<summary>Show answer</summary>

**Correct answer: C)**

For Account Members, both the Account Type and the Formula Type must be set to DynamicCalc for the member to function as a Dynamic Calculation. For Flow and UD dimensions, only the Formula Type needs to be DynamicCalc. This is a common source of confusion.
</details>

---

### Question 55 (Easy)
**201.1.2** | Difficulty: Easy

What is the recommended "sweet spot" for Data Unit size in OneStream?

A) 10,000 to 50,000 records
B) 50,000 to 100,000 records
C) 250,000 to 500,000 records
D) 1,000,000 to 2,000,000 records

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The sweet spot for OneStream Data Units is between 250,000 and 500,000 records. As you increase above that number, performance deteriorates at an increasing rate. Data Units exceeding 1 million records warrant inspection, and above 2 million is a red flag.
</details>

---

### Question 56 (Easy)
**201.1.2** | Difficulty: Easy

What does a Calculation Status of "CN" indicate?

A) Calculation Not Available
B) Calculation Needed — Data Has Changed
C) Calculation Normal — No Issues
D) Consolidation Not Required

<details>
<summary>Show answer</summary>

**Correct answer: B)**

CN stands for "Calculation Needed, Data Has Changed." After the DUCS runs on a Data Unit, its Calculation Status changes to OK (No Calculation Needed). When data changes — through Form input, data load, or Journal — the status changes to CN, indicating recalculation is needed.
</details>

---

### Question 57 (Easy)
**201.1.2** | Difficulty: Easy

How many Storage Types exist in OneStream for Cube data?

A) 4
B) 5
C) 7
D) 10

<details>
<summary>Show answer</summary>

**Correct answer: C)**

There are 7 Storage Types in OneStream: Input, Journal, Calculation, DurableCalculation, Consolidation, Translation, and StoredButNoActivity. Each data cell — whether Real or Derived — will have one of these Storage Types.
</details>

---

### Question 58 (Easy)
**201.1.2** | Difficulty: Easy

How many Formula Passes are available in the DUCS?

A) 4
B) 8
C) 12
D) 16

<details>
<summary>Show answer</summary>

**Correct answer: D)**

There are 16 Formula Passes available (FormulaPass1 through FormulaPass16). They execute in groups of 4 within the DUCS, interleaved with pairs of Business Rules. Within each Formula Pass, Account formulas run first, then Flow formulas, then UD1 through UD8 formulas.
</details>

---

### Question 59 (Medium)
**201.1.2** | Difficulty: Medium

Why is the DUCS described as "all or nothing"?

A) Because it either succeeds completely or rolls back all changes
B) Because all steps run each time — you cannot choose to execute only specific steps like a single Formula Pass
C) Because it processes all Cubes in the application simultaneously
D) Because it requires all 8 Business Rules to be assigned before it can run

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The DUCS is "all or nothing" — all steps run each time, no matter what. You cannot choose to execute only Formula Pass 1 and bypass the other steps. This ensures the entire Data Unit is always completely processed before data is consolidated to its Parent Data Unit, and that calculation order and data integrity are never compromised.
</details>

---

### Question 60 (Medium)
**201.1.2** | Difficulty: Medium

What is the difference between a Calculation and a Consolidation in OneStream?

A) A Calculation processes Parent Entities; a Consolidation processes Base Entities
B) A Calculation only executes the DUCS for the selected Data Unit; a Consolidation executes the DUCS plus aggregation, currency translations, and IC eliminations
C) A Calculation runs Custom Calculate rules; a Consolidation runs the DUCS
D) There is no difference — they are synonymous terms

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Calculation simply executes the DUCS for the selected Data Unit. A Consolidation executes the DUCS and does several additional actions: aggregating and storing data at Parent Members in the Entity hierarchy, executing Currency Translations, and executing Intercompany Eliminations.
</details>

---

### Question 61 (Medium)
**201.1.2** | Difficulty: Medium

Why might a consolidation become slower when the top of the Entity hierarchy has very few sibling entities?

A) Because fewer sibling entities mean more data per entity
B) Because OneStream processes sibling entities in parallel — fewer siblings means fewer threads and less parallelism
C) Because the DUCS runs more Formula Passes at the top level
D) Because IC eliminations are more complex with fewer entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream processes sibling entities in parallel (multi-threaded). When there are fewer siblings at the top of the hierarchy, fewer threads are used, resulting in less parallelism and slower processing. This is why Entity hierarchy design can impact consolidation performance.
</details>

---

### Question 62 (Medium)
**201.1.2** | Difficulty: Medium

For a Planning solution, which approach is recommended for running Calculations and aggregating data?

A) Run all Calculations in the DUCS using Business Rules and consolidate using Standard Consolidation
B) Use Custom Calculate Functions (outside the DUCS) and aggregate using C#Aggregated
C) Use only Member Formulas with FormulaPass1 and consolidate monthly
D) Run Dynamic Calculations for all accounts to avoid storing data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Planning solutions, the recommendation is to primarily use Custom Calculate Functions (outside the DUCS) with Entity Aggregation using C#Aggregated. This is because Planning is iterative — numbers are constantly changed, multiple users work within the same Data Unit, and having users constantly triggering full DUCS runs would strain the server. Consolidation solutions, by contrast, should primarily run Calculations in the DUCS.
</details>

---

### Question 63 (Medium)
**201.1.2** | Difficulty: Medium

What are the key differences when running a Calculation on C#Aggregated compared to a Standard Consolidation?

A) C#Aggregated runs the DUCS at all entities and performs full IC elimination
B) C#Aggregated only executes the DUCS at Base Entities, performs no IC elimination, no Share/Ownership calculations, and only uses Direct Method Translation
C) C#Aggregated skips the DUCS entirely and only aggregates data
D) C#Aggregated is identical to Standard Consolidation but runs faster

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When using C#Aggregated, a modified version of the Standard Consolidation algorithm is executed. The key differences are: the DUCS is only executed at Base Entities; no Intercompany elimination logic is performed; no Share or Ownership Calculations are performed; data is stored at C#Aggregated at Parent Entities; and only Direct Method Translation is performed.
</details>

---

### Question 64 (Medium)
**201.1.2** | Difficulty: Medium

During a full Consolidation, how many times does the DUCS run for each Entity?

A) Once — for the Local Consolidation Member only
B) Once for each Consolidation Member in the hierarchy — up to 7 times (Local, C#Local, Elim, C#Elim, Adj, C#Adj, Contribution)
C) Exactly 3 times — Local, Elimination, and Contribution
D) It depends on the number of Business Rules assigned

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Consolidation Dimension is part of the Data Unit, meaning the DUCS runs for each Consolidation Member being processed. During a full Consolidation, this can include Local, C#Local, Elimination, C#Elimination, Adjustments, C#Adjustments, and Contribution — up to 7 times. However, some Finance Function Types only run at certain Consolidation Members.
</details>

---

### Question 65 (Medium)
**201.1.2** | Difficulty: Medium

What happens to Member Formulas assigned to the same Formula Pass during the DUCS?

A) They execute sequentially in alphabetical order
B) They execute in parallel (multithread)
C) They execute in order of Account, Flow, then UD1-UD8
D) Only the first formula in the pass executes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Formulas running in the same Formula Pass multithread (run in parallel). This is one benefit of Member Formulas — they can leverage parallel execution. However, this means formulas within the same pass should not depend on each other's results. Within each pass, Account formulas run first, then Flow, then UD1 through UD8, but within each dimension type, formulas of the same pass run in parallel.
</details>

---

### Question 66 (Medium)
**201.1.2** | Difficulty: Medium

How is a Custom Calculate Function triggered?

A) From a Cube View button or Data Management Step
B) Only from a Data Management Step or Sequence
C) From any Workflow Process Step or Dashboard
D) Automatically during every Consolidation

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Custom Calculate Function can only be triggered via a Data Management Step or Sequence. You create a step with the Custom Calculate Step Type and define the Data Unit(s), Business Rule, and Function Name. Custom Calculate Functions cannot be called from a Cube View. They can be called from a Workflow Process Step only via a Data Management sequence.
</details>

---

### Question 67 (Medium)
**201.1.2** | Difficulty: Medium

What is the default value of the "Clear Calculated Data During Calc" Scenario property, and what does it control?

A) True by default; it controls whether the DUCS clears previously calculated data in its first step
B) False by default; it controls whether data is exported after calculation
C) True by default; it controls whether journals are cleared during consolidation
D) False by default; it prevents any data from being modified during calculation

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The "Clear Calculated Data During Calc" setting on the Scenario controls whether the first step of the DUCS — clearing previously calculated data — is executed. When set to True (the default for consolidation scenarios), OneStream clears previously calculated data based on cell Storage Type before recalculating. This does not clear Durable Data.
</details>

---

### Question 68 (Medium)
**201.1.2** | Difficulty: Medium

Member Formulas can only execute which two Finance Function Types?

A) Calculate and Custom Calculate
B) Calculate and Consolidation
C) Calculate and DynamicCalc
D) Custom Calculate and DynamicCalc

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Member Formulas only support the Calculate and DynamicCalc Finance Function Types. If you need to run a Custom Calculate function, you must use a Business Rule. This is a key difference between Business Rules (which support all Finance Function Types) and Member Formulas.
</details>

---

### Question 69 (Medium)
**201.1.2** | Difficulty: Medium

What is the purpose of the Sibling Consolidation Pass property on an Entity?

A) It determines the order in which Business Rules run during the DUCS
B) It ensures that certain sibling entities are consolidated and calculated after other sibling entities — typically used for Equity Pickup
C) It controls whether an entity participates in Intercompany Eliminations
D) It sets the number of parallel threads used for consolidation

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Sibling Consolidation Pass is typically used for holding companies related to Equity Pickup calculations. Setting Pass 2 or greater ensures the entity is consolidated and calculated after other sibling entities (which default to Pass 1). This is needed when calculated data for one entity depends on calculated data from other sibling entities, such as layered ownership structures.
</details>

---

### Question 70 (Medium)
**201.1.2** | Difficulty: Medium

What is the key difference between GetDataBuffer and GetDataBufferUsingFormula?

A) GetDataBuffer is faster but less accurate
B) GetDataBufferUsingFormula supports FilterMembers and RemoveZeros parameters; GetDataBuffer does not
C) GetDataBuffer can only be used in Business Rules; GetDataBufferUsingFormula can be used anywhere
D) They are identical — GetDataBufferUsingFormula is just a newer version

<details>
<summary>Show answer</summary>

**Correct answer: B)**

GetDataBufferUsingFormula supports FilterMembers and RemoveZeros parameters that GetDataBuffer does not. FilterMembers allows filtering the data buffer to specific members, and RemoveZeros eliminates zero-value cells. These additional parameters make GetDataBufferUsingFormula more flexible for targeted calculations.
</details>

---

### Question 71 (Medium)
**201.1.2** | Difficulty: Medium

What is the default setting for NoData Calculate Settings on a Cube, and when should it be changed?

A) True for all Consolidation Members; change to False for Planning scenarios
B) False for all Consolidation Members except Local; set to True when copying data from another period or scenario
C) True for Local only; change to False to improve performance
D) False for all; never needs to be changed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

NoData Calculate Settings controls whether the Finance Engine runs calculations on empty Data Units for each Consolidation Member. The default is False for all except Local. It should be set to True when calculations need to run even on empty Data Units — typically when copying data from another period or scenario into the current Data Unit.
</details>

---

### Question 72 (Medium)
**201.1.2** | Difficulty: Medium

What is the difference between the Periodic and YTD settings for the Consolidation View property on a Scenario?

A) Periodic stores data by period; YTD stores cumulative data
B) Periodic is required for Org-by-Period implementations; YTD enhances consolidation performance. All data is always stored as YTD regardless of this setting.
C) YTD is required for Org-by-Period; Periodic is the default
D) There is no functional difference — it is only a display preference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Consolidation View determines the standard view of the consolidation. Setting it to Periodic is required for Org-by-Period implementations, while YTD enhances consolidation performance. Importantly, all numbers are always stored as YTD regardless of this setting — the property controls how the consolidation engine processes the data, not how it is physically stored.
</details>

---

### Question 73 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** An administrator notices that certain Data Units show a Calculation Status of OK, but suspects the data may be stale. What mechanism allows them to recalculate regardless of Calculation Status?

A) Running a Custom Calculate, which always ignores Calculation Status
B) Using Force Consolidation or Force Calculation, which processes all dependent Data Units and ignores Calculation Status
C) Setting the NoData Calculate property to True on the Cube
D) Changing the Clear Calculated Data During Calc setting to False

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Force Consolidation or Force Calculation processes all dependent Data Units and ignores Calculation Status. A normal Consolidate/Calculate checks Calculation Status first and will not run the DUCS if the status is OK. Force Calculate is useful when an administrator suspects stale data or wants to ensure all calculations are current. Note that Custom Calculate also does not check Calculation Status (answer A), but the question asks specifically about recalculating existing DUCS-based calculations, making Force Calculate the most appropriate answer.
</details>

---

### Question 74 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A developer is asked to explain the exact execution order within the DUCS. Which sequence is correct?

A) BR 1-8 → Formula Passes 1-16 → Clear Data → Scenario Member Formula → Reverse Translations
B) Clear Data → Reverse Translations → Scenario Member Formula → BR 1-2 → FP 1-4 → BR 3-4 → FP 5-8 → BR 5-6 → FP 9-12 → BR 7-8 → FP 13-16
C) Clear Data → Scenario Member Formula → Reverse Translations → BR 1-2 → FP 1-4 → BR 3-4 → FP 5-8 → BR 5-6 → FP 9-12 → BR 7-8 → FP 13-16
D) Scenario Member Formula → Clear Data → BR 1-2 → FP 1-4 → Reverse Translations → BR 3-8 → FP 5-16

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The exact DUCS order is: (1) Clear previously calculated data, (2) Run Scenario Member Formula, (3) Perform reverse Translations, (4) Execute BR 1-2, (5) Execute FP 1-4, (6) Execute BR 3-4, (7) Execute FP 5-8, (8) Execute BR 5-6, (9) Execute FP 9-12, (10) Execute BR 7-8, (11) Execute FP 13-16. The interleaving of Business Rules and Formula Passes in pairs/groups of 4 is a key design feature that allows calculation dependencies to be managed across the sequence.
</details>

---

### Question 75 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A developer assigns 6 Account Member Formulas to FormulaPass3 and 4 Account Member Formulas to FormulaPass4. During DUCS execution, how do these formulas execute relative to each other?

A) All 10 formulas run sequentially in the order they appear in the Dimension Library
B) The 6 FormulaPass3 formulas run in parallel, then the 4 FormulaPass4 formulas run in parallel after FormulaPass3 completes
C) All 10 formulas run in parallel since they are in the same Formula Pass group (FP 1-4)
D) FormulaPass3 and FormulaPass4 run in parallel with each other

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Member Formulas assigned to the same Formula Pass multithread (run in parallel). FormulaPass3 and FormulaPass4 are different passes that execute sequentially within the FP 1-4 group. So the 6 FormulaPass3 formulas run in parallel with each other first, and once completed, the 4 FormulaPass4 formulas run in parallel with each other. Formulas within the same pass should not depend on each other's results.
</details>

---

### Question 76 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A statutory consolidation solution requires Trial Balance calculations that feed Flow calculations, which in turn drive Cash Flow calculations. Should the team use DUCS-based Business Rules or Custom Calculate?

A) Custom Calculate — it is always faster and more flexible
B) DUCS-based Business Rules — the DUCS handles clearing data, dependencies, and ensures complete processing before Parent consolidation
C) A mix of both in the same Data Unit for different calculation types
D) Neither — Dynamic Calculations are the best approach for statutory consolidation

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Consolidation solutions should primarily run Calculations in the DUCS. The DUCS removes guesswork — it automatically handles clearing previously calculated data, dependent Data Units, and ensures calculation order is never compromised. This is critical for statutory consolidation where Trial Balance → Flow → Cash Flow dependencies exist and data integrity is paramount. Planning solutions, by contrast, should primarily use Custom Calculate with C#Aggregated.
</details>

---

### Question 77 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A company uses C#Aggregated for their Planning Cube. The CFO asks why Intercompany Eliminations are not appearing at Parent Entities. What is the explanation?

A) C#Aggregated requires a separate elimination rule to be written
B) C#Aggregated by design does not perform any Intercompany elimination logic — it also skips Share/Ownership calculations and only performs Direct Method Translation
C) The IC Entity flag was not set correctly on the entities
D) C#Aggregated only runs eliminations if the Two Pass Elimination property is enabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When using C#Aggregated, a modified version of the Standard Consolidation algorithm is executed. By design, no Intercompany elimination logic is performed, no Share or Ownership Calculations are performed, the DUCS only executes at Base Entities, and only Direct Method Translation is performed. If Elimination, Translation, or complex ownership requirements exist, the full Standard Consolidation must be used instead.
</details>

---

### Question 78 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A team is troubleshooting a consolidation performance issue. They want to see the detailed timing of each DUCS step. Which tool should they use, and what is the trade-off?

A) Use the Calculation Status report — no performance impact
B) Use Calculate With Logging — it provides detailed timing for each DUCS step but adds significant processing time
C) Use the Cube View audit trail — it records all calculation events automatically
D) Enable the Application Log — it records DUCS steps with no additional overhead

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Calculate With Logging (or Consolidate With Logging) provides detailed timing information for each step of the DUCS, helping administrators identify bottlenecks. However, it adds significant processing time to the operation. This tool is diagnostic in nature and should not be left on for routine operations. It can be run from Data Management.
</details>

---

### Question 79 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A developer writes a shared Business Rule containing global helper functions that are called from Account Member Formulas. The Member Formulas fail with an error. What property must be set on the Business Rule?

A) The Business Rule must be set to Finance Function Type = Calculate
B) The "Contains Global Functions for Formulas" property must be set to True on the Business Rule
C) The Business Rule must be assigned to Formula Pass 1 on the Cube
D) The "Allow Cross-Data Unit References" property must be enabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In the Properties tab of the Business Rule, the "Contains Global Functions for Formulas" property must be set to True. This is required for any Business Rule that contains functions intended to be called from Member Formulas. Without this setting, Member Formulas cannot reference functions defined in the Business Rule.
</details>

---

### Question 80 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A developer runs a Custom Calculate rule that writes data, and then a Consolidation is triggered on the same Scenario. The Custom Calculated data disappears. What went wrong, and how should it be fixed?

A) The Custom Calculate rule had a bug that deleted the data
B) The DUCS first step clears calculated data — the developer should have set isDurable = True to use the DurableCalculation Storage Type, which survives the clear step
C) The Consolidation always overwrites Custom Calculate data regardless of settings
D) The developer should have run the Consolidation first, then the Custom Calculate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The first step of the DUCS is "Clear previously calculated data" based on cell Storage Type. Standard calculated data is cleared during this step. To protect Custom Calculated data from being cleared, the developer should set isDurable = True when writing the data. This stores it with the DurableCalculation Storage Type, which is not cleared during the DUCS clear step. Additionally, a Clear Data Script should be added at the top of every Custom Calculate rule to handle its own cleanup.
</details>

---

### Question 81 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A company copies Actuals data into a Forecast scenario as seed data. After running calculations, some entities that had no data still need calculations to run (to copy data from the source). The NoData Calculate Setting is set to False. What change is needed?

A) Enable Force Calculate on all Data Units
B) Set the NoData Calculate Setting to True for the relevant Consolidation Members so the Finance Engine processes empty Data Units
C) Load dummy zero values into all empty Data Units
D) Change the Calculation Status manually to CN for each Data Unit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The NoData Calculate Setting controls whether the Finance Engine runs calculations on empty Data Units per Consolidation Member. The default is False (except for Local). When calculations need to run on empty Data Units — such as when copying data from another period or scenario — this setting must be changed to True for the relevant Consolidation Members. This ensures the DUCS runs even when the target Data Unit has no existing data.
</details>

---

### Question 82 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A company has a layered ownership structure where Houston Heights has investments in South Houston, and Downtown Houston has investments in Houston Heights. During consolidation, which entity needs to calculate first, and how is this configured?

A) Downtown Houston calculates first using the Priority property
B) Houston Heights must calculate before Downtown Houston; this is configured using the Sibling Consolidation Pass property — setting Houston Heights to Pass 1 (default) and Downtown Houston to Pass 2
C) South Houston calculates last using the Consolidation Sequence property
D) The order does not matter — OneStream handles dependencies automatically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In a layered ownership structure, Houston Heights needs to be calculated first to properly reflect its balances before Downtown Houston is calculated (since Downtown Houston has investments in Houston Heights). This is configured using the Sibling Consolidation Pass setting. Houston Heights stays at Pass 1 (default), and Downtown Houston is set to Pass 2 or greater. This ensures Downtown Houston is consolidated and calculated after Houston Heights has completed.
</details>

---

### Question 83 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** An architect notices that a Parent Entity Data Unit has grown to over 2 million records after adding UD dimensions. What are the recommended approaches to address this?

A) Increase server memory and processor speed
B) Look to break up the Entity or Cube to spread out the data, and review dimension design to reduce the number of intersections
C) Delete unused UD members to reduce the Data Unit size
D) Switch to Analytic Blend tables for all data storage

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Data Units exceed 2 million records (a red flag), the recommended approaches are: first, look to see if you can break up the Entity or Cube to spread out the data; second, review the dimension design. Adding UD dimensions can create millions of aggregation points. Dense account or custom dimensions result in slower performance. OneStream also treats a zero as data, so avoid loading or calculating cells with hard-coded zero values. Consider whether some data would be better served in Analytic Blend tables rather than the Cube.
</details>

---

### Question 84 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A team notices that Dynamic Calculations for certain UD members are not executing. The Formula Type is set to DynamicCalc on those members. What could be the issue specific to Account members?

A) Dynamic Calculations are not supported on UD dimension members
B) For Account members specifically, both the Account Type AND the Formula Type must be set to DynamicCalc — only setting the Formula Type is insufficient
C) The Business Rule containing the DynamicCalc logic is not assigned to the Cube
D) DynamicCalc members cannot reference data from other members

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Account members, both the Account Type and the Formula Type must be set to DynamicCalc. This is a common configuration mistake — setting only the Formula Type to DynamicCalc without changing the Account Type will not enable Dynamic Calculation behavior. For Flow and UD dimension members, only the Formula Type needs to be set to DynamicCalc.
</details>

---

### Question 85 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A Hybrid Scenario is configured to share Actuals data into a Forecast scenario. Users report that the shared data is read-only and cannot be modified. Is this expected behavior?

A) No — shared data should always be editable in the target scenario
B) Yes — the "Share Data from Source Scenario" binding type shares a read-only Cube data set from the source Scenario; use "Copy Input Data from Source Scenario" if editable data is needed
C) No — the administrator needs to unlock the Forecast scenario for editing
D) Yes — all Hybrid Scenario data is always read-only regardless of binding type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "Share Data from Source Scenario" Data Binding Type shares a read-only Cube data set from the source Scenario to the target Scenario. If the team needs editable data in the Forecast, they should use "Copy Input Data from Source Scenario" instead, which copies base-level Cube data from the source, making it editable. A third option — "Copy Input Data from Business Rule" — copies data based on a Finance Business Rule.
</details>

---

### Question 86 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** An implementation team needs to use Org-by-Period consolidation for a complex ownership structure. Which Consolidation View setting is required on the Scenario, and why?

A) YTD — because all data is stored as YTD
B) Periodic — Org-by-Period implementations require the Consolidation View to be set to Periodic, although all numbers are still stored as YTD
C) Both Periodic and YTD must be configured on different scenarios
D) The Consolidation View setting has no impact on Org-by-Period

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Org-by-Period implementations, the Consolidation View must be set to Periodic. This tells the consolidation engine to process work by period, which is essential for Org-by-Period ownership changes. However, all numbers are always stored as YTD internally — the setting controls how the consolidation engine processes the data. Setting it to YTD (the alternative) enhances consolidation performance but does not support Org-by-Period.
</details>

---

### Question 87 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A consolidation of 500 entities completes 90% quickly but the last 10% takes disproportionately longer. What is the most likely architectural explanation?

A) The last 10% of entities have more data
B) As the consolidation moves up the hierarchy, there are fewer sibling entities at each level — fewer siblings means fewer parallel threads, so the top-level entities process sequentially rather than in parallel
C) The server runs out of memory for the last entities
D) Business Rules become slower at Parent Entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream processes sibling entities in parallel (multi-threaded). At the base of the hierarchy, there are typically many siblings, enabling high parallelism. As consolidation moves up the hierarchy, there are progressively fewer siblings at each level, meaning fewer parallel threads. At the top levels, there may be only a handful of entities processing one at a time. This creates a characteristic pattern where the bulk of entities process quickly, but the last few at the top take disproportionately longer.
</details>

---

### Question 88 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A consolidation scenario requires that certain IC elimination entries be calculated after the initial consolidation pass, using data that was just consolidated. Which property enables this?

A) Sibling Consolidation Pass set to Pass 2
B) Two Pass Elimination — it allows elimination entries to be calculated in a second pass after initial consolidation
C) NoData Calculate set to True for the Elimination Consolidation Member
D) Custom Calculate with isDurable = True

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Two Pass Elimination is a Scenario property that enables elimination entries to be calculated in a second pass after the initial consolidation. This is necessary when elimination calculations depend on data that is only available after the first consolidation pass has completed. It ensures that complex IC elimination requirements can reference freshly consolidated data.
</details>

---

### Question 89 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A database administrator asks about how OneStream physically stores Cube data across years. How are the data tables organized?

A) All data is stored in a single DataRecord table partitioned by year
B) Data is stored in separate DataRecord tables per year — from DataRecord1996 through DataRecord2100 — with each table containing the data cells for all periods of that specific year
C) Data is stored in one table per Cube, with a Year column for filtering
D) Data is stored in one table per Entity per Year

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Numeric Cube data is stored by year in one of 105 fully normalized fact tables from DataRecord1996 to DataRecord2100. Each record contains the coordinates and numeric amounts for all stored data, including Dimension Member information, Cell Amount, and Cell Status. Each record contains the data cells for all periods of the specific year represented by that table. These tables are accessible to Administrator-level Users through Database in the System tab.
</details>

---

### Question 90 (Hard)
**201.1.2** | Difficulty: Hard

**Scenario:** A developer needs to decide between using Business Rules and Member Formulas for a large volume of calculations. What is the key behavioral difference regarding parallelism during the DUCS?

A) Business Rules run in parallel; Member Formulas run sequentially
B) Both Business Rules and Member Formulas run in parallel within the same Formula Pass
C) Business Rules execute sequentially within their assigned pair (e.g., BR 1 then BR 2); Member Formulas in the same Formula Pass run in parallel (multithread)
D) Neither supports parallel execution — all calculations are single-threaded

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Business Rules execute sequentially within their assigned pair in the DUCS (e.g., BR 1 runs, then BR 2 runs). Member Formulas assigned to the same Formula Pass multithread (run in parallel), which can provide a performance advantage when there are many independent calculations. However, this parallelism means formulas in the same pass should not depend on each other's results. For large calculation volumes, Business Rules can be easier to manage since all calculations are in one place, while Member Formulas offer better organizational structure for maintenance.
</details>

---

---


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

---


# Question Bank - Section 3: Data Collection (13% of exam)

## Objective 201.3.1: Demonstrate an understanding of the different Data Source types

### Question 1 (Easy)
**201.3.1** | Difficulty: Easy

Which of the following is NOT a main Data Source type in OneStream?

A) Fixed Files
B) Delimited Files
C) Pivot Table
D) Connector

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The four main Data Source types are: Fixed Files, Delimited Files, Connector, and Data Management Export Sequence. Pivot Table is not a Data Source type in OneStream.
</details>

---

### Question 2 (Easy)
**201.3.1** | Difficulty: Easy

Which Data Source type allows connecting directly to an external database without requiring a physical file?

A) Fixed Files
B) Delimited Files
C) Connector
D) Excel Template

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Connector type establishes a direct connection to an external database using ODBC/OLEDB, importing data without requiring an intermediate physical file.
</details>

---

### Question 3 (Easy)
**201.3.1** | Difficulty: Easy

What are the three main groups of the Origin dimension?

A) Input, Output, Calculation
B) Import, Forms, Journal Entries
C) Source, Target, Destination
D) Stage, Engine, Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Origin dimension segments data into three isolated groups: Import (data from flat files, queries, or templates), Forms (manually entered data), and Journal Entries (journal entries). These groups are isolated from each other to protect data integrity.
</details>

---

### Question 4 (Easy)
**201.3.1** | Difficulty: Easy

In what format should amounts be in source files imported into OneStream?

A) In local currency format
B) In natural sign of debit and credit
C) Always in positive values
D) In percentage format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Source files should be in the natural sign of debit and credit. This is a fundamental requirement for OneStream to correctly process imported data.
</details>

---

### Question 5 (Medium)
**201.3.1** | Difficulty: Medium

What is the difference between Tabular and Matrix data structures in a Data Source?

A) Tabular supports multiple cubes; Matrix supports only one
B) Tabular has one line per intersection with one amount; Matrix has multiple amounts per line
C) Matrix is faster than Tabular
D) Tabular only works with Excel files; Matrix with text files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In a Tabular structure, each line represents an intersection with a single amount. In a Matrix structure, each line can contain multiple amounts using rows and columns. Both structures can be used with different file types.
</details>

---

### Question 6 (Medium)
**201.3.1** | Difficulty: Medium

An administrator needs to configure a Connector Data Source to import data from an external ERP system. Which of the following is NOT an integration prerequisite?

A) 64-bit Client Data Provider installed on each Application Server
B) Connection String configured in the Server Configuration Utility
C) Read and write access credentials
D) Inventory of source systems with database type and location

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Credentials must be **read-only**, not read and write. OneStream is a pull system and does not need write permissions on the source system. The other options are valid prerequisites.
</details>

---

### Question 7 (Medium)
**201.3.1** | Difficulty: Medium

Which Connector Business Rule Information Request Type is executed when the user clicks "Load and Transform"?

A) GetFieldList
B) GetData
C) GetDrillBackTypes
D) GetDrillBack

<details>
<summary>Show answer</summary>

**Correct answer: B)**

GetData is executed when clicking "Load and Transform" and is responsible for executing data queries against the source system. GetFieldList returns the list of fields, while GetDrillBackTypes and GetDrillBack relate to the Drill Back functionality.
</details>

---

### Question 8 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** An organization needs to copy data from a cube with different dimensionality to another cube within the same OneStream application, applying transformation rules during the process. What Data Source type should they use?

A) Connector with an internal SQL query
B) Data Management Export Sequence
C) Fixed Files exported from the first cube
D) Excel Template with XFD Named Ranges

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Management Export Sequence allows moving data between OneStream cubes or scenarios with different dimensionality, applying Transformation Rules during the process. It is configured by creating a Data Source with Type = "Data Mgmt Export Sequences" and setting the Data Export Sequence Name. The other options would be unnecessarily complex or not applicable.
</details>

---

## Objective 201.3.2: Given a design specification, apply the Data Source configuration

### Question 9 (Easy)
**201.3.2** | Difficulty: Easy

Which Data Source property must be set to `True` to allow loading Excel templates?

A) Enable Excel Integration
B) Allow Dynamic Excel Loads
C) Excel Template Mode
D) Enable XFD Processing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The `Allow Dynamic Excel Loads` property must be set to `True` in the Data Source to allow loading Excel templates with Named Ranges (XFD, XFF, XFJ, XFC).
</details>

---

### Question 10 (Easy)
**201.3.2** | Difficulty: Easy

Which Named Range is used to import data to Stage in an Excel template?

A) XFF
B) XFJ
C) XFD
D) XFC

<details>
<summary>Show answer</summary>

**Correct answer: C)**

XFD is the Named Range for loading data to Stage (Import). XFF is for Forms, XFJ for Journals, and XFC for Cell Details.
</details>

---

### Question 11 (Easy)
**201.3.2** | Difficulty: Easy

Which dimension token is used to represent the Amount field in an XFD template?

A) A#
B) AM#
C) AMT#
D) $#

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The token `AMT#` represents the Amount field. You can also use `AMT.ZS#` to automatically apply zero suppression. The token `A#` corresponds to the Account dimension, not the Amount.
</details>

---

### Question 12 (Medium)
**201.3.2** | Difficulty: Medium

In an XFD template, what syntax is used to assign a static value to an entire column?

A) F#=[None]
B) F#:(None)
C) F#:[None]
D) F#:Static(None)

<details>
<summary>Show answer</summary>

**Correct answer: C)**

To set a Static Value, use the `:[value]` syntax after the token. For example, `F#:[None]` fixes the "None" member of the Flow dimension for the entire column.
</details>

---

### Question 13 (Medium)
**201.3.2** | Difficulty: Medium

Which tokens are used to obtain the Time and Scenario from the current Workflow POV in an Excel template?

A) T.W# and S.W#
B) T.C# and S.C#
C) T.G# and S.G#
D) T.WF# and S.WF#

<details>
<summary>Show answer</summary>

**Correct answer: B)**

`T.C#` and `S.C#` return the Time and Scenario from the current Workflow (Current). The tokens `T.G#` and `S.G#` return the values from the Global POV, which is different.
</details>

---

### Question 14 (Medium)
**201.3.2** | Difficulty: Medium

**Scenario:** A developer needs to configure a column in an XFD template where each row can have a different User Defined Dimension 3 member. However, some rows do not apply for this dimension. What value should be placed in the rows where UD3 does not apply?

A) Blank (leave empty)
B) N/A
C) None
D) Null

<details>
<summary>Show answer</summary>

**Correct answer: C)**

For User Defined Dimensions (UD1# through UD8#), every row must have a value. When the dimension does not apply for a specific row, you must use the value `None`. Leaving the cell empty would cause an import error.
</details>

---

### Question 15 (Medium)
**201.3.2** | Difficulty: Medium

What is the recommended best practice regarding Source ID in XFD Named Ranges?

A) Use the same Source ID for the entire Excel workbook
B) One Source ID per Named Range
C) One Source ID per each tab in the workbook
D) Source ID is not needed in Excel templates

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to use one Source ID per Named Range. The Source ID (token `SI#`) is the key for data in Stage and allows controlling the granularity of data loads and replacements.
</details>

---

### Question 16 (Hard)
**201.3.2** | Difficulty: Hard

What Bypass Settings options are available in Source Dimension Properties to skip lines during import?

A) Skip Header and Skip Footer
B) Contains at Position and Contains Within Line
C) Exclude by Value and Exclude by Range
D) Filter by Column and Filter by Row

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Bypass Settings allows skipping entire lines based on a value. The two options are: `Contains at Position` (when the value is found at a specific position) and `Contains Within Line` (when the value is found anywhere on the line).
</details>

---

### Question 17 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A team needs to configure a Data Source to import a delimited file with large data of more than 1 million records. What are the recommended Cache Page Size and Cache Pages In Memory Limit values to optimize performance?

A) Cache Page Size = 100, Cache Pages In Memory Limit = 500
B) Cache Page Size = 500, Cache Pages In Memory Limit = 2000
C) Cache Page Size = 1000, Cache Pages In Memory Limit = 5000
D) Cache Page Size = 250, Cache Pages In Memory Limit = 1000

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For large datasets of more than 1 million records, the specific recommendation is to set Cache Page Size = 500 and Cache Pages In Memory Limit = 2000. These values are documented as the optimal configuration for handling large data volumes.
</details>

---

### Question 18 (Hard)
**201.3.2** | Difficulty: Hard

In Source Dimension Properties, what function does Substitution Settings serve and how are multiple values separated?

A) Filters null values; separated by comma
B) Replaces source values (Old Value) with new values (New Value); multiple values are separated with ^
C) Concatenates values from multiple columns; separated by |
D) Converts data types; separated by ;

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Substitution Settings allows replacing source values (Old Value) with new values (New Value) during import. When multiple substitutions are needed, values are separated by the `^` character.
</details>

---

## Objective 201.3.3: Demonstrate an understanding of the Import Process Log

### Question 19 (Easy)
**201.3.3** | Difficulty: Easy

What are the three main tasks of the Import process in Workflow?

A) Extract, Transform, Load
B) Import (Load and Transform), Validate, Load
C) Parse, Map, Calculate
D) Connect, Download, Process

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three main tasks are: Import (Load and Transform) which imports data to Stage, Validate which verifies mappings and intersections, and Load which loads data from Stage to the Analytic Engine (Consolidation Engine).
</details>

---

### Question 20 (Easy)
**201.3.3** | Difficulty: Easy

What icon color in the Workflow indicates that a task completed successfully?

A) Blue
B) Yellow
C) Green
D) White

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Green indicates a successfully completed task. Blue indicates a pending task and Red indicates an error that must be corrected before continuing.
</details>

---

### Question 21 (Easy)
**201.3.3** | Difficulty: Easy

Which Load Method deletes all data from the previous Source ID and replaces it with data from the new file?

A) Append
B) Replace
C) Merge
D) Overwrite

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Replace deletes all data from the previous Source ID and replaces it with data from the new file. Append only adds new rows without modifying existing data.
</details>

---

### Question 22 (Medium)
**201.3.3** | Difficulty: Medium

**Scenario:** A user imported a file and during validation, Transformation Errors appear showing "Unassigned" in the Target Value column. What is the cause and how is it resolved?

A) The file has incorrect format; it should be reimported with a different format
B) There is no Transformation Rule to map the Source Value to a target member; a rule must be created and Retransform must be executed
C) The cube does not have the dimension configured; the dimension must be added
D) The server does not have enough memory; the service must be restarted

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Transformation Errors with "Unassigned" in the Target Value indicate that no transformation rule (map) exists for that source value. The solution is to edit or create the correct transformation rule, save, and click Retransform, which automatically re-validates the data.
</details>

---

### Question 23 (Medium)
**201.3.3** | Difficulty: Medium

What is the difference between a Transformation Error and an Intersection Error during the Validate step?

A) There is no difference, they are synonyms
B) Transformation Error indicates a missing mapping for a dimension; Intersection Error indicates that the complete combination of dimensions is not valid in the cube
C) Transformation Error is a warning; Intersection Error is a critical error
D) Transformation Error occurs during Import; Intersection Error occurs during Load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Transformation Error means a source value has no mapping rule (the dimension shows "Unassigned"). An Intersection Error means that, even though each dimension is mapped, the complete combination of dimensions is not valid. Example: a Customer mapped to a Salary Grade Account.
</details>

---

### Question 24 (Medium)
**201.3.3** | Difficulty: Medium

Which right-click context menu option in Import allows navigating back to the source system?

A) View Source Document
B) View Processing Log
C) Drill Back
D) Export

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Drill Back allows navigating to the source system, but it is only available when using a Connector Data Source. View Source Document opens the imported source file, and View Processing Log shows information about when and how the file was imported.
</details>

---

### Question 25 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** A team uses a Workflow with multiple Source IDs to load data from different departments separately. The administrator needs to reload all data for a period. Why should they NOT use the Load Method "Replace Background (All Time, All Source IDs)"?

A) Because this method does not support multiple periods
B) Because this method always deletes ALL Source IDs, preventing partial replacement by department
C) Because this method requires a Connector Data Source
D) Because this method only works with Excel files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Replace Background (All Time, All Source IDs) always deletes ALL Source IDs, so it cannot be used when the Workflow uses multiple Source IDs for partial replacement. If the team loads departments separately with different Source IDs, this method would delete data from all departments and only load data from the current file.
</details>

---

### Question 26 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** An auditor needs to verify who executed a data import, when it was performed, and how long it took. Which Workflow functionality should they use?

A) View Processing Log from the Import menu
B) Audit Workflow Process from the Workflow channel's context menu
C) Data Unit Statistics
D) View Transformation Rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Audit Workflow Process (accessible by right-clicking on any Workflow channel) provides a complete audit of each task, including date and time, the user who executed the process, duration, errors encountered, and lock history. Although View Processing Log also shows information, Audit Workflow Process offers the most comprehensive audit.
</details>

---

### Question 27 (Hard)
**201.3.3** | Difficulty: Hard

What is the correct file name format for Batch Processing in the Harvest directory?

A) WFProfileName_ScenarioName_TimeName_LoadMethod.csv
B) FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt
C) ProfileName.ScenarioName.TimeName.LoadMethod.dat
D) Import_FileID_WFProfile_Scenario_Time.txt

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct format for Batch Processing is `FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt`. Files must be placed in the Harvest directory with this specific format using hyphens as separators.
</details>

---

## Objective 201.3.4: Demonstrate an understanding of Transformation Rules

### Question 28 (Easy)
**201.3.4** | Difficulty: Easy

What is the correct execution order of Transformation Rules during the Validate step?

A) One-to-One > Composite > Range > List > Mask > Source Derivative > Target Derivative
B) Source Derivative > One-to-One > Composite > Range > List > Mask > Target Derivative
C) Mask > List > Range > Composite > One-to-One > Source Derivative > Target Derivative
D) One-to-One > Range > List > Mask > Composite > Derivatives

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct order is: Source Derivative > One-to-One > Composite > Range > List > Mask > Target Derivative. This order is important because it determines which rule is applied first when multiple rules could match the same value.
</details>

---

### Question 29 (Easy)
**201.3.4** | Difficulty: Easy

Which Transformation Rule type is the only one valid for the Scenario, Time, and View dimensions?

A) Composite
B) Mask
C) One-to-One
D) Range

<details>
<summary>Show answer</summary>

**Correct answer: C)**

One-to-One is the only Transformation Rule type valid for the Scenario, Time, and View dimensions. These critical dimensions require explicit mapping from one source member to one target member.
</details>

---

### Question 30 (Easy)
**201.3.4** | Difficulty: Easy

In a List rule, what character is used as the separator between members?

A) Comma (,)
B) Semicolon (;)
C) Pipe (|)
D) Hyphen (-)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

List rules use the semicolon (;) as the separator. For example: `41137;41139;41145` maps all these accounts to the same target.
</details>

---

### Question 31 (Medium)
**201.3.4** | Difficulty: Medium

What is the key difference between how Composite and Range rules process matches?

A) Composite is faster than Range
B) Composite stops when it finds the first match; Range does not stop and applies the last range processed
C) Range stops when it finds a match; Composite applies all matches
D) There is no difference in processing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is a critical difference: Composite rules stop when they find the first match, while Range rules do NOT stop when they find a match and apply the last range processed. This is important when designing rules to avoid unexpected results.
</details>

---

### Question 32 (Medium)
**201.3.4** | Difficulty: Medium

What separator is used in a Range rule to define the lower and upper limits?

A) Hyphen (-)
B) Tilde (~)
C) Colon (:)
D) Pipe (|)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Range rules use the tilde (~) as the separator between the lower and upper limits. For example: `11202~11209` defines a range of values. Additionally, ranges use character sets, not integers.
</details>

---

### Question 33 (Medium)
**201.3.4** | Difficulty: Medium

**Scenario:** A developer needs to map all accounts that begin with "27" to a specific target, regardless of how many characters follow. What rule type and syntax is most efficient?

A) Mask with `27??`
B) Mask with `27*`
C) Range with `2700~2799`
D) Composite with `A#[27*]`

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Mask with `27*` is the most efficient option. The `*` wildcard captures any number of characters and has low cost (simple update pass through). Using `?` would be more costly because it requires table scans. The range would not capture accounts like "27999" if the upper limit is not adjusted correctly.
</details>

---

### Question 34 (Medium)
**201.3.4** | Difficulty: Medium

What types of Derivative Rules exist based on how they are stored in Stage?

A) Temporal and Permanent
B) Interim, Interim (Exclude Calc), Final, Final (Exclude Calc), and Check Rule
C) Draft and Published
D) Source and Target only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The types are: Interim (not stored in Stage, can be used in subsequently run derivative rules), Interim (Exclude Calc), Final (stored in Stage and available to be mapped to a target member), Final (Exclude Calc), and Check Rule (a custom validation rule applied during the Validate task).
</details>

---

### Question 35 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** An implementation team is experiencing performance issues with their Transformation Rules. They currently use many Mask rules with `?` wildcards on both sides (source and target), and several Conditional rules. What strategies should they apply to optimize performance?

A) Add more memory to the server and increase the timeout
B) Replace `?` rules with One-to-One, Range, or List when possible; split large Conditional lists into smaller lists; limit criteria in Mask Conditional
C) Run the rules during night hours to avoid impact
D) Convert all rules to Derivatives for more flexibility

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Performance best practices include: using One-to-One, Range, and List whenever possible (minimal cost), avoiding `?` on both sides which are extremely costly, splitting large Conditional lists into smaller lists for optimal memory utilization, and keeping criteria restrictive in Mask Conditional. Derivatives are the most costly rules, so converting everything to Derivatives would worsen performance.
</details>

---

### Question 36 (Hard)
**201.3.4** | Difficulty: Hard

In a Mask rule, which of the following statements is correct?

A) The `?` wildcard can be used in both Source and Target Value
B) The `*` wildcard as a prefix in Target Value is supported
C) The `?` wildcard is NOT supported in Target Value, and `*` as a prefix in Target Value is NOT supported
D) Both wildcards are interchangeable in Source and Target

<details>
<summary>Show answer</summary>

**Correct answer: C)**

In Mask rules, the `?` wildcard is NOT supported in Target Value, and `*` as a prefix in Target Value is also NOT supported. However, `*` as a suffix in Target Value yields the Source Value. These restrictions are important when designing Mask rules.
</details>

---

### Question 37 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A consultant needs to create a rule that conditionally maps accounts based on the combination of the source account and entity. For example, accounts "199X-XXX" (where X is any character) must be mapped differently if they belong to entity "Texas". What rule type and syntax should be used?

A) One-to-One with multiple entries
B) Composite with syntax `A#[199?-???*]:E#[Texas]`
C) Range with entity condition
D) List with entity filter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Composite rule allows conditional mapping using dimensional tags with the syntax `D#[value]`. The syntax `A#[199?-???*]:E#[Texas]` uses dimensional tags for Account and Entity, where `?` represents one character and `*` represents any number of characters. The Composite rule stops when it finds the first match.
</details>

---

### Question 38 (Medium)
**201.3.4** | Difficulty: Medium

How are Transformation Rules organized to be assigned to a Workflow?

A) They are assigned directly to the Data Source
B) They are grouped into Transformation Rule Groups, then into Transformation Rule Profiles, which are assigned to Workflow Profiles
C) They are created directly within the Workflow
D) They are stored in external XML files

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Transformation Rules are organized into Groups, which are grouped into Transformation Rule Profiles, which are assigned to Workflow Profiles (Application > Workflow Profiles > Import > Integration Settings). The "Create and Populate Rule Profile" button can automatically create a Group and Profile with the same name.
</details>

---

### Question 39 (Easy)
**201.3.4** | Difficulty: Easy

Which of the following Transformation Rule types have the lowest processing cost (green)?

A) Derivative and Composite
B) One-to-One, Range, and List
C) Mask with ? and Conditional
D) Source Derivative and Target Derivative

<details>
<summary>Show answer</summary>

**Correct answer: B)**

One-to-One, Range, and List have the lowest cost (green) because they perform a simple update pass through to the database. Mask with `*` on one side also has low cost. Derivatives and Conditional rules are the most costly.
</details>

---

### Question 40 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A developer needs to validate that the imported trial balance is balanced. The validation must sum certain source accounts to verify the balance. What rule type and Derivative Type should be used, and why?

A) Target Derivative with Final type, because it operates on already transformed data
B) Source Derivative with Check Rule type, because it operates on inbound source data and can implement custom validations during Validate
C) One-to-One with Business Rule logic
D) Composite with balance condition

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Source Derivative is appropriate because it operates on inbound source data and can create new records in Stage to sum accounts and verify the balance. The "Check Rule" Derivative Type is specifically a custom validation rule that is applied during the Validate task. Source Derivatives execute first in the rule order, which is necessary for this type of validation.
</details>

---

## NEW QUESTIONS (41-78)

## Objective 201.3.1: Demonstrate an understanding of the different Data Source types

### Question 41 (Easy)
**201.3.1** | Difficulty: Easy

OneStream is described as what type of data system in relation to source systems?

A) A push system that receives data sent from source systems
B) A pull system that extracts data from databases and source systems
C) A hybrid push-pull system
D) A batch-only processing system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream is a pull system — it pulls data from databases and source systems. Data is not pushed to OneStream data tables. The ability to load from a direct connection means you can refresh data by simply pushing a button, and the consumption is instantaneous.
</details>

---

### Question 42 (Easy)
**201.3.1** | Difficulty: Easy

In OneStream, journals are written to which member of the Origin dimension?

A) Import
B) Forms
C) AdjInput
D) JournalInput

<details>
<summary>Show answer</summary>

**Correct answer: C)**

In the OneStream app, journals are written to the AdjInput member of the Origin dimension. This is separate from the Import and Forms members, providing data isolation and protection.
</details>

---

### Question 43 (Medium)
**201.3.1** | Difficulty: Medium

Which of the following correctly describes the purpose of the Origin dimension in OneStream?

A) It tracks the chronological order of data imports
B) It combines data protection, data layering, and isolation by segmenting data into separate buckets for each entity
C) It identifies which user loaded the data
D) It classifies data by geographical origin

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Origin dimension combines the concepts of data protection and data layering and isolation. It acts as a barrier to protect how data is loaded in different forms — Import, Forms, and Journal Entries are kept in isolated groups so that, for example, importing data does not overwrite data entered through forms.
</details>

---

### Question 44 (Medium)
**201.3.1** | Difficulty: Medium

What does the Connector Business Rule Information Request Type `GetFieldList` return?

A) A list of all records in the external database
B) A list of available field names in the external Data Source as a list of VB.Net Strings
C) The connection string used for the database
D) A list of drill-back options

<details>
<summary>Show answer</summary>

**Correct answer: B)**

GetFieldList is called by the Data Source designer screen when the user selects a Connector Data Source or one of its defined Dimensions. It returns a list of available fields in the external Data Source as a list of VB.Net Strings [List(Of String)].
</details>

---

### Question 45 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** A company uses Oracle as their ERP database and needs to set up a Connector Data Source. The consultant is preparing the integration prerequisites. Which of the following statements about the connection setup is correct?

A) Oracle connections only require the connection string; no additional software is needed on the application servers
B) Oracle Database integrations require that all Oracle Source System TNS Profile details need to be in place on each of the OneStream application servers
C) Oracle connections can use 32-bit client data providers since OneStream supports both architectures
D) The connection string for Oracle is identical to the SQL Server connection string

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Oracle Database integrations, all Oracle Source System TNS Profile details need to be in place on each of the OneStream application servers. Additionally, a 64-bit source system client data provider must be installed because OneStream is a Microsoft .NET application with a 64-bit architecture.
</details>

---

### Question 46 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** An architect is evaluating the best approach for creating a prototype of external data queries before building the Connector Business Rule. What is the recommended best practice?

A) Write the Connector Business Rule directly and test it with live data
B) Create a set of Dashboard Data Adapters in a new Dashboard Maintenance Unit named with the EXS prefix to prototype queries
C) Use the OneStream REST API to test queries externally
D) Build a temporary Data Source with sample files first

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to create a set of Dashboard Data Adapters to be used as a test bed. You should create a new Dashboard Maintenance Unit named "EXS The Connector Name" where the EXS prefix stands for External System, providing administrators with an immediate understanding of the Maintenance Unit's contents.
</details>

---

### Question 47 (Medium)
**201.3.1** | Difficulty: Medium

Which of the following drill-back visualization types opens a website in an external browser based on variables from the source data row?

A) DataGrid
B) TextMessage
C) WebUrl
D) WebUrlPopOutDefaultBrowser

<details>
<summary>Show answer</summary>

**Correct answer: D)**

WebUrlPopOutDefaultBrowser opens a website or custom HTML web content in an external browser. From the Stage Import data grid, you right-click on a data record, select Drill Back, and choose WebUrlPopOutDefaultBrowser to launch a standard browser session based on variables from the data row.
</details>

---

### Question 48 (Easy)
**201.3.1** | Difficulty: Easy

What is the primary purpose of the Data Management Export Sequence Data Source type?

A) To export data from OneStream to external CSV files
B) To move and export data within OneStream, such as copying data between cubes or scenarios
C) To create backup copies of OneStream databases
D) To import data from external REST APIs

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Management Export Sequences are used in Import workflows to move and export data. For example, they can copy data from one OneStream cube or scenario to another (even when cubes or scenarios contain different dimensionality), or export OneStream data to an external source system while applying transformation rules through the workflow.
</details>

---

## Objective 201.3.2: Given a design specification, apply the Data Source configuration

### Question 49 (Medium)
**201.3.2** | Difficulty: Medium

In a Fixed File Data Source, what do the Start Position and Length properties define for a Source dimension?

A) The starting row and number of rows to read
B) The numerical starting point for a line item and how many characters will be taken from that position
C) The column header position and total column width
D) The starting byte offset and total file length

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Start Position is the numerical representation of the starting point for a line item, and Length defines how many characters will be taken from the start position. For example, a Fixed data source with a start position of 20 and a length of five will start with the 20th character and include the next five characters.
</details>

---

### Question 50 (Medium)
**201.3.2** | Difficulty: Medium

In the Source Dimension Properties, what is the difference between the "Text" and "Stored Text" Data Types?

A) Text reads from a file; Stored Text reads from a database
B) Text reads the value from the file as defined in position settings; Stored Text overrides position settings and forces a constant value for every line
C) Stored Text saves the value permanently; Text is temporary
D) There is no functional difference between them

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "Text" Data Type reads the value from the file as defined in the position settings. The "Stored Text" Data Type overrides the position settings and forces the value to be a constant value for every line. This is useful when a dimension should have the same value for all imported records.
</details>

---

### Question 51 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A developer is configuring an Excel template to load data for multiple time periods in a single import using a Matrix-style layout. Which token syntax should be used for the Amount columns with zero suppression?

A) AMT#:[2023M1], AMT#:[2023M2], etc. with separate Named Ranges per period
B) AMT.ZS# combined with T# tokens in the column headers for each period
C) AMT# with a separate T# column for each row
D) AMOUNT# with period suffixes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For a Matrix-style Excel import template, you use AMT.ZS# (Amount with zero suppression) on the AMT columns, combined with T# tokens in the column headers specifying each time period. The .ZS extension applies zero suppression to the import, and the Matrix format allows multiple amounts per line across different time periods.
</details>

---

### Question 52 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A Data Source is configured for a Delimited File, and the imported file contains amounts with commas as thousand separators, periods as decimal indicators, and some values have a leading minus sign for negatives. Which Numeric Settings must be configured?

A) Only the Decimal Indicator
B) Thousand Indicator = ",", Decimal Indicator = ".", and Negative Sign Indicator = "-"
C) Currency Indicator = "$" and Factor Value = 1
D) Only the Negative Sign Indicator

<details>
<summary>Show answer</summary>

**Correct answer: B)**

All three Numeric Settings must be configured: the Thousand Indicator set to "," to handle the comma separator in values like 1,000; the Decimal Indicator set to "." for decimal separation; and the Negative Sign Indicator set to "-" to correctly identify negative values. These settings ensure OneStream properly parses the numeric amounts.
</details>

---

### Question 53 (Medium)
**201.3.2** | Difficulty: Medium

Which Source Dimension Data Type should be used when the Time value must come from the current Workflow POV rather than from the data file?

A) Text
B) Stored DataKey Text
C) Current DataKey Time
D) Global DataKey Time

<details>
<summary>Show answer</summary>

**Correct answer: C)**

"Current DataKey Time" uses the Time value from the current Workflow POV. This is different from "Global DataKey Time" which uses the Time value from the Global POV, and "Text" or "Stored DataKey Text" which read or store fixed values from the file.
</details>

---

### Question 54 (Medium)
**201.3.2** | Difficulty: Medium

What is the purpose of the Leading Fill Value and Trailing Fill Value in Text Fill Settings of a Source Dimension?

A) They add padding characters to the values read from the file upon import
B) They remove characters from the beginning and end of values
C) They define the alignment of text in Stage tables
D) They replace special characters in the imported values

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Leading Fill Value adds characters before whatever value is brought in from the file (e.g., Lead Fill Mask = xxx, data value = 00, results value = x00). Trailing Fill Value adds characters after the value (e.g., Trail Fill Mask = xxx, Data Value = 00, Results Value = 00x). These are useful for padding or formatting source values during import.
</details>

---

### Question 55 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A team is configuring a Data Management Export Sequence to copy actuals data from a financial close cube (FinRptg) to a planning cube (MgmtRptg) with different dimensionality. The FinRptg legal entity data needs to map to MgmtRptg's UD1 dimension, and FinRptg's UD1 cost center data needs to map to MgmtRptg's Entity dimension. Which setting must be TRUE on the Import Workflow Profile to allow this data to load successfully?

A) Allow Cross-Cube Mapping
B) Can Load Unrelated Entities
C) Enable Multi-Cube Integration
D) Allow Dynamic Dimension Mapping

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When configuring an Import Workflow Profile for a Data Management Export Sequence that loads data to multiple entities, the "Can Load Unrelated Entities" setting must be set to True. Additionally, Profile Active must be set to True, and the Workflow Name should include Import, Validate, and Load tasks.
</details>

---

### Question 56 (Easy)
**201.3.2** | Difficulty: Easy

Which Named Range prefix is used to load data into custom SQL Server tables from an Excel template?

A) XFD
B) XFF
C) XFT
D) XFC

<details>
<summary>Show answer</summary>

**Correct answer: C)**

XFT is the Named Range prefix for loading data into custom SQL Server tables. XFD is for Stage data, XFF is for Forms, XFJ is for Journals, and XFC is for Cell Details.
</details>

---

### Question 57 (Hard)
**201.3.2** | Difficulty: Hard

**Scenario:** A consultant needs to configure a Connector Data Source where the external query results should be processed like a Delimited File, using all the same built-in processing capabilities including Complex Expressions and Business Rules on dimensions. What is the correct approach?

A) Set Connector Uses Files to True and manually parse the data
B) Map fields from the external data query results to Dimensions, which creates processing behavior similar to a Delimited File
C) Export the query results to a CSV file first, then configure a Delimited File Data Source
D) Use a Parser Business Rule to convert the Connector data to a delimited format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Fields from the external data query results are mapped to Dimensions, creating a processing behavior similar to a Delimited File. This enables a Connector Data Source to use all the same built-in processing capabilities available with file-based Data Sources, including Complex Expressions and Business Rules on dimensions. This design methodology helps write the Connector Business Rule in a way that requires very little maintenance.
</details>

---

## Objective 201.3.3: Demonstrate an understanding of the Import Process Log

### Question 58 (Medium)
**201.3.3** | Difficulty: Medium

What is the purpose of the Source ID field in the data import process?

A) It identifies the user who uploaded the file
B) It is a key for data imported into Stage that controls how data is reloaded and segmented
C) It stores the original file name for audit purposes only
D) It assigns a sequential number to each imported record

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Source ID is a field in the data source that allows data to be tagged to determine how to reload data. It is a key for data imported into Stage and enables controlling the granularity of data loads. Reloading data with the same Source ID replaces only that subset, while different Source IDs keep data segments isolated.
</details>

---

### Question 59 (Medium)
**201.3.3** | Difficulty: Medium

When exporting data from OneStream through a Data Management Export Sequence, which of the following is NOT a valid method to retrieve the transformed data from Stage tables?

A) Workflow Event Handler business rule
B) Transformation Event Handler business rule
C) Directly querying the Stage SQL tables from an external application
D) Data Adapter with OneStream REST API

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Valid methods to retrieve transformed data from Stage include: Workflow Event Handler (runs code when workflow completes a step), Transformation Event Handler (runs code at various points from Import through Load), Extender Rule (ad hoc or scheduled data retrieval), and Data Adapter with OneStream REST API. Directly querying Stage SQL tables from an external application is not a recommended or supported method.
</details>

---

### Question 60 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** A company needs to automate their monthly data import process. They want files to be automatically loaded into OneStream when placed in a specific directory. What is the correct file name example for a batch file that replaces data for the Detroit Import workflow profile, Actual scenario, January 2011?

A) Detroit_Import_Actual_2011M1_Replace.txt
B) 1TB-Detroit;Import-Actual-2011M1-R.txt
C) Detroit-Import-Actual-2011M1-Replace.csv
D) TB_Detroit_Import_Actual_2011M1_R.txt

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct format follows the pattern `FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt`. The example `1TB-Detroit;Import-Actual-2011M1-R.txt` shows: FileID = 1TB, WFProfileName = Detroit;Import, ScenarioName = Actual, TimeName = 2011M1, LoadMethod = R (Replace). Files must be placed in the Harvest directory.
</details>

---

### Question 61 (Easy)
**201.3.1** | Difficulty: Easy

What does the Confirm task in the Workflow process do?

A) Confirms that the data file was received
B) Runs the Confirmation Rules and informs users if they passed or failed data quality rules
C) Sends a confirmation email to the administrator
D) Confirms the connection to the source system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Confirm task runs the Confirmation Rules defined for the particular Workflow. It immediately informs users if they have passed or failed the data quality rules. The two types of statuses are Warning (outside threshold but does not stop the process) and Error (outside threshold and stops the process, turning the step to red).
</details>

---

### Question 62 (Medium)
**201.3.3** | Difficulty: Medium

During the Import process, what is the difference between loading data to Stage versus a Direct Load (Non-Stage)?

A) There is no functional difference; both provide the same audit trail
B) Stage loads provide a full audit trail with source data, mapping, and target dimensions; Direct Loads bypass Stage tables for faster loading but with less audit capability
C) Direct Loads are only for Excel templates
D) Stage loads are only for Connector Data Sources

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Stage loads process data through the mapping engine with a complete audit trail including source data, mapping used, and target dimensions. Direct Loads (Non-Stage) bypass the overhead of storing source information in Stage tables, loading data quicker into the cube but without full audit capabilities. Direct Loads are recommended for forecasting data, flash data, or what-if purposes rather than regular monthly reporting.
</details>

---

### Question 63 (Hard)
**201.3.3** | Difficulty: Hard

**Scenario:** An implementation team is loading data and notices that a 10-digit string account number like `2300028932` contains embedded information: `23000` is the account, `28` is the geography, and `932` is the product. They only need to map the account portion. What approach is recommended for optimizing the data source and mapping?

A) Use a Composite rule to parse all three segments simultaneously
B) Take only the first five digits and use simple one-to-one mapping; alternatively, break the dimension up with separators (like `23000_28_932`) for flexibility with wildcard mapping
C) Load the full 10-digit string and create Range rules for each combination
D) Write a Connector Business Rule to split the string before import

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is to take only the first five digits if only the account information is needed, making the mapping easy as one-to-one. If the dimension needs other information, breaking the dimension up and adding separators (such as `23000_28_932`) provides flexibility with wildcard mapping. For example, `23000*_9*` could map every account with `23000` and a product starting with `9` to a specific account.
</details>

---

## Objective 201.3.4: Demonstrate an understanding of Transformation Rules

### Question 64 (Easy)
**201.3.4** | Difficulty: Easy

Which characters are reserved for operations in Transformation Rules and will cause errors if used in Source or Target Members?

A) @, #, $
B) !, ?, |
C) &, %, ^
D) <, >, =

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The characters !, ?, and | are used in operations for Transformation Rules. These characters will cause errors if used with Source Members or Target Members.
</details>

---

### Question 65 (Medium)
**201.3.4** | Difficulty: Medium

In a Range rule, what happens when a source member falls within multiple overlapping ranges?

A) An error is thrown and the mapping fails
B) The member is mapped to the first range that matches
C) The member is mapped to the last range processed
D) The member is flagged as "Unassigned"

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Range rules do not stop processing when a record meets the criteria. If the ranges overlap and a member falls within multiple ranges, that member will be mapped only to the final range that is processed. Therefore, for a member in multiple ranges, you should set the order so that the last rule run is the one you want applied.
</details>

---

### Question 66 (Medium)
**201.3.4** | Difficulty: Medium

When entering a Range rule expression, how should character sets be balanced to ensure the full range is captured?

A) Always use numeric values only
B) Use balanced character sets; for example, a range of 4 to 3000 must be entered as 0004~3000
C) Pad the upper limit with zeros
D) Use the Length property to specify character count

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Range rules use character sets, not integers. To ensure the full range is captured, you must use balanced character sets. For example, a range of 4 to 3000 must be entered as `0004~3000` under Rule Expression. Without balanced characters, the range comparison may not work as expected.
</details>

---

### Question 67 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A developer needs to create a derivative rule that aggregates all accounts starting with "11" into a new Account called "Cash" and stores it in Stage for mapping. Then, accounts starting with "12" should aggregate into "AR" but only as a temporary value for use in subsequent derivative rules. What Derivative Types should be used?

A) Both should use Final type
B) Cash = Final, AR = Interim
C) Cash = Interim, AR = Final
D) Both should use Interim type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For "Cash" (A#[11*]=Cash), the Final derivative type should be used because it needs to be stored in Stage and available to be mapped to a target member. For "AR" (A#[12*]=AR), the Interim derivative type should be used because it will not be stored in Stage but can be used within other subsequently run derivative rules as a temporary aggregation.
</details>

---

### Question 68 (Medium)
**201.3.4** | Difficulty: Medium

What does the "Create and Populate Rule Profile" toolbar button do in Transformation Rules?

A) Creates only a Transformation Rule Group
B) Creates a Transformation Rule Group and a Transformation Rule Profile with the same name, and the Profile is already populated with each Dimension Rule Group
C) Creates a Workflow Profile with transformation rules pre-configured
D) Imports rules from a TRX file and creates a profile automatically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Create and Populate Rule Profile" creates a Transformation Rule Group and a Transformation Rule Profile with the same name. The Rule Profile will already be populated with each Dimension Rule Group and updates as the Group is updated. This is a convenience feature that simplifies the setup process.
</details>

---

### Question 69 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A Composite rule for the Entity dimension is configured with the following three rules to map entities based on account names: Rule 1 (Order 1): `E#[*]:A#[H???]` maps to Rhode Island; Rule 2 (Order 2): `E#[*]:A#[H????]` maps to Maine; Rule 3 (Order 3): `E#[*]:A#[H?????????]` is set to bypass. For a record with Account = "Heads" (5 characters), which entity will it be mapped to?

A) Rhode Island
B) Maine
C) It will be bypassed
D) It will show as Unassigned

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Composite rules stop processing when a record meets the criteria and are processed in order. "Heads" has 5 characters. Rule 1 (`H???`) matches accounts with H + 3 characters (4 total) — "Heads" has 5 characters so it does not match. Rule 2 (`H????`) matches accounts with H + 4 characters (5 total) — "Heads" matches, so processing stops and it maps to Maine. Rule 3 is never evaluated.
</details>

---

### Question 70 (Medium)
**201.3.4** | Difficulty: Medium

What is the purpose of the Logical Operator property in Transformation Rule mapping types like Composite, Range, List, and Mask?

A) It defines the mathematical operations to apply to amounts
B) It extends a normal mapping rule with VB.NET scripting functionality
C) It sets the Boolean condition for rule execution
D) It determines the sort order of rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Logical Operator provides the ability to extend a normal mapping rule with VB.NET scripting functionality. The options are: None (default — no script), Business Rule (when .NET scripting is available in the Business Rule Library), and Complex Expression (when .NET scripting is needed only within that specific dimension).
</details>

---

### Question 71 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A derivative rule creates new rows in Stage by adding a suffix to account names and changing entities. The rule expression is: `A#[1000~1999]<<New_:E#[Tex*]=TX`. For a record with Account = "1010" and Entity = "Texas", what will the new Stage row contain?

A) Account = "1010", Entity = "TX"
B) Account = "New_1010", Entity = "TX"
C) Account = "1010_New", Entity = "Texas"
D) Account = "New_1010", Entity = "Texas"

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The rule `A#[1000~1999]<<New_:E#[Tex*]=TX` creates a new row in Stage for any Account between 1000 and 1999, adding the prefix "New_" (the `<<` syntax adds a prefix). So Account 1010 becomes "New_1010". For entities starting with "Tex", the entity in the new row is set to "TX". The original row remains unchanged.
</details>

---

### Question 72 (Medium)
**201.3.4** | Difficulty: Medium

What is the recommended approach for naming global Transformation Rules that can be reused across multiple cubes?

A) Name them with the cube name as a prefix
B) Name them as "global" since Scenario, Time, and View dimensions can only have one-to-one mapping and can typically be reused across cubes
C) Create unique names for each workflow profile
D) Use numeric identifiers for universal rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The dimension rules for Scenario, Time, and View should be named as "global" because they can be reused over multiple cubes. These three dimensions can only have one-to-one mapping, so keeping them simple and reusable (e.g., one group for each if they don't overlap with different strings) is a best practice.
</details>

---

### Question 73 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** An administrator wants to export Transformation Rules from one application and import them into another. They plan to use TRX files. Which limitation should they be aware of?

A) TRX files cannot export more than 1000 rules
B) TRX files do not support Logical Operators and Complex Expressions; for those, XML load/extract should be used
C) TRX files are only compatible with One-to-One rules
D) TRX files must be converted to CSV before import

<details>
<summary>Show answer</summary>

**Correct answer: B)**

TRX file export creates a comma-delimited file which is valid only for certain Transformation Rule designs. Properties such as Logical Operators and Complex Expressions are not supported. For complete Transformation Rule management including these properties, Application Tools Load/Extract of application XML files should be used.
</details>

---

### Question 74 (Medium)
**201.3.1** | Difficulty: Medium

What are the four major considerations when starting a data integration project in OneStream?

A) Budget, Timeline, Resources, Technology
B) Inventory Files and Sources, Historical Data, How Much Data, Performance of the Data Load
C) Data Format, Connection Type, Security, Validation
D) Source System, Target Cube, Mapping Rules, Workflow Configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four major considerations are: (1) Inventory Files and Sources — understanding all data sources and how data is prepared, (2) Historical Data — determining how much history needs to be reconciled (typically two years), (3) How Much Data — balancing the amount of data in the ledger vs. the cube, and (4) Performance of the Data Load — considering data volume and tuning processing times and mapping.
</details>

---

### Question 75 (Hard)
**201.3.4** | Difficulty: Hard

**Scenario:** A consultant is reviewing mapping performance for a large dataset. The current rules include Map Mask (Conditional), Map List (Conditional), and Derivative rules. All are processing slowly. Which statement about the processing cost of these rules is accurate?

A) Conditional rules are low-cost because they only add a filter condition
B) All three types require returning a record set with all dimension fields back to the application server, consuming significant data transfer and memory
C) Derivative rules are the fastest because they execute in the database
D) List (Conditional) rules are faster than standard List rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Map Mask (Conditional), Map List (Conditional), and Derivative rules all use significant data transfer and memory utilization. They are all required to return a record set with all dimension fields back to the application server to perform conditional mapping or derive calculated rows. Derivatives additionally execute a SQL Statement that pulls ALL dimension fields and insert new records one by one.
</details>

---

### Question 76 (Medium)
**201.3.2** | Difficulty: Medium

When loading journal data via an Excel template, which specific tokens are used for debit and credit amounts?

A) AMT# for both debits and credits
B) AMTDR# for debits and AMTCR# for credits
C) DR# for debits and CR# for credits
D) DEBIT# and CREDIT#

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When loading journal entries via Excel, the specific tokens for amounts are AMTDR# for the debited amount and AMTCR# for the credited amount. This is different from standard data imports which use AMT# for a single amount field.
</details>

---

### Question 77 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** A company wants to spread their data load for better performance. They currently have one data load with 1 million records taking ten minutes. An architect suggests breaking the load into five separate groups. Why would this approach improve performance?

A) Because smaller files consume less disk space
B) Because the parser is sequential, but by breaking data into separate groups, the parser can be used by multiple data load and workflow processes simultaneously
C) Because OneStream has a file size limit of 200,000 records
D) Because transformation rules run faster on smaller datasets regardless of parallel processing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The parser is sequential for a single data load, but by breaking the data into separate groups (commonly split by business units/entities), the parser can be used by multiple data load and workflow processes. The same data that takes ten minutes in one load could be loaded in two minutes by breaking it into five separate groups, because they can be processed in parallel.
</details>

---

### Question 78 (Hard)
**201.3.1** | Difficulty: Hard

**Scenario:** A team is designing the journal workflow process. They need to understand the Journal Balance Type options to configure journal templates correctly. An entity has intercompany transactions that must balance by each individual entity in a multi-entity journal. Which Journal Balance Type should they select?

A) Balanced
B) Balance by Entity
C) Unbalanced
D) Balanced by Intercompany

<details>
<summary>Show answer</summary>

**Correct answer: B)**

"Balance by Entity" requires that debits and credits in a multi-entity journal must balance for each entity. This is different from "Balanced" which requires the entire set of journal lines to balance between debits and credits, and "Unbalanced" which does not perform a balance check and is normally used for one-sided journals.
</details>

---

---


# Question Bank - Section 4: Presentation (14% of the Exam)

## Objective 201.4.1: Demonstrate understanding of Cube View configurations

### Question 1 (Easy)
**201.4.1** | Difficulty: Easy

How are Cube Views known in the context of reporting in OneStream?

A) Report Engines
B) Building blocks of reporting
C) Data Visualization Tools
D) Query Designers

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views are the "building blocks of reporting" in OneStream. They can be read-only, used for data editing, and used as a Data Source for various visualization mechanisms.
</details>

---

### Question 2 (Easy)
**201.4.1** | Difficulty: Easy

What is the maximum number of dimensions that can be nested in rows and columns in a Cube View?

A) 2 in rows and 2 in columns
B) 4 in rows and 4 in columns
C) 4 in rows and 2 in columns
D) 3 in rows and 3 in columns

<details>
<summary>Show answer</summary>

**Correct answer: C)**

In a Cube View, up to 4 dimensions can be nested in rows and up to 2 dimensions in columns. This limitation is important when designing complex reports.
</details>

---

### Question 3 (Easy)
**201.4.1** | Difficulty: Easy

When defining the POV in a Cube View, which has priority: the POV slider or the POV Pane?

A) The POV Pane always has priority
B) The POV slider has priority over the POV Pane
C) Both have the same priority
D) It depends on the administrator's configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The information in the POV slider takes priority over the POV Pane. This is important when configuring Cube Views to ensure the displayed data corresponds to the correct selections.
</details>

---

### Question 4 (Medium)
**201.4.1** | Difficulty: Medium

What is the correct syntax for renaming a row or column header in a Cube View using the Name() function?

A) A#CashBalance.Name("Cash Balance")
B) A#CashBalance:Name(Cash Balance)
C) A#CashBalance|Name=Cash Balance|
D) Name(Cash Balance):A#CashBalance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct syntax is to use `:Name()` at the end of the Member Filter. For example, `A#CashBalance:Name(Cash Balance)` displays "Cash Balance" as the header instead of the technical member name.
</details>

---

### Question 5 (Medium)
**201.4.1** | Difficulty: Medium

What Cube View property must be set to `True` to allow users to edit data directly in OnePlace?

A) Is Dynamic
B) Is Visible in Profiles
C) Can Modify Data
D) Can Calculate

<details>
<summary>Show answer</summary>

**Correct answer: C)**

`Can Modify Data = True` allows the Cube View to be editable in OnePlace. If set to `False`, the Cube View is read-only, although annotations (Data Attachments) are still allowed.
</details>

---

### Question 6 (Medium)
**201.4.1** | Difficulty: Medium

**Scenario:** A developer is creating a Cube View for a Workflow that must display all entities assigned to the Workflow Profile. Where should the entity reference be placed and what syntax should be used?

A) In the POV, using E#Root.WFEntities
B) In Rows or Columns, using E#Root.WFProfileEntities
C) In the POV, using E#Root.WFProfileEntities
D) In Rows or Columns, using E#WFEntities.All

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You must use `E#Root.WFProfileEntities` in Rows or Columns, NOT in the POV. The reason is that there can be more than one entity in the Workflow Profile, and the POV only accepts one member per dimension. Placing it in rows or columns allows all entities to be expanded.
</details>

---

### Question 7 (Medium)
**201.4.1** | Difficulty: Medium

What is the format priority order in a Cube View, from lowest to highest?

A) Row settings > Column settings > Cube View Default > Application Properties
B) Application Properties > Cube View Default > Column settings > Row settings > Column Row overrides > Row Column overrides
C) Cube View Default > Application Properties > Row settings > Column settings
D) Row Column overrides > Column Row overrides > Column settings > Row settings > Cube View Default > Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The priority order from lowest to highest is: Application Properties > Cube View Default > Column settings > Row settings > Column Row overrides > Row Column overrides. The most specific settings (Row Column overrides) have the highest priority and override all others.
</details>

---

### Question 8 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A Cube View has multiple nested dimensions in rows and performance is slow. Many combinations have no data. The developer wants to improve performance by filtering rows with no data before rendering. However, some columns contain dynamically calculated data. How should Sparse Row Suppression be configured?

A) Enable Allow Sparse Row Suppression = True in General Settings; no additional configuration is needed
B) Enable Allow Sparse Row Suppression = True in General Settings; for columns with dynamic data, set Use to Determine Row Suppression = True and Allow Sparse Row Suppression = True
C) Use Suppress NoData Rows = True, which is equivalent
D) Enable Paging to limit the rows processed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Sparse Row Suppression is enabled in General Settings > Common > Allow Sparse Row Suppression = True. However, this feature cannot be applied to dynamically calculated data (calculated members and Cube View math). For columns with dynamic data, you must set `Use to Determine Row Suppression = True` and `Allow Sparse Row Suppression = True` at the column level.
</details>

---

### Question 9 (Hard)
**201.4.1** | Difficulty: Hard

When does Cube View Paging activate automatically and what are its default parameters?

A) With more than 5,000 rows; Max Rows = 1,000 and Max Seconds = 10
B) With more than 10,000 unsuppressed rows; it attempts to return up to 2,000 unsuppressed rows in a maximum of 20 seconds
C) With more than 50,000 rows; Max Rows = 10,000 and Max Seconds = 60
D) It never activates automatically; it must be enabled manually

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Paging is automatically applied to the Data Explorer view when there are more than 10,000 unsuppressed rows. It attempts to return up to 2,000 unsuppressed rows in a maximum of 20 seconds. The configurable parameters are Max Unsuppressed Rows Per Page (default -1, max 100,000) and Max Seconds To Process (default -1, max 600).
</details>

---

### Question 10 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A user needs to reference a parameter called "RegionFilter" within a Cube View used as a form in a Dashboard. What is the correct syntax and what happens if the parameter is not found in the form?

A) Use {RegionFilter} and it generates an error if not found
B) Use |!RegionFilter!| and if not found in the Form, OneStream searches the application's Dashboard Parameters
C) Use @RegionFilter@ and it uses a default value if not found
D) Use $RegionFilter$ and it displays a prompt to the user

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Parameters in Cube Views are referenced using the syntax `|!ParameterName!|`. If the parameter is not found associated with the Form, OneStream automatically searches the application's Dashboard Parameters as a fallback.
</details>

---

## Objective 201.4.2: Demonstrate understanding of Workspace and Dashboard configuration

### Question 11 (Easy)
**201.4.2** | Difficulty: Easy

What is the primary purpose of a Workspace in OneStream?

A) To store consolidated financial data
B) To act as an isolated development sandbox where teams can create, modify, and test solutions without interfering with others
C) To execute consolidation processes
D) To manage end-user security

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Workspace is a structured sandbox-like development space where teams can create, modify, and test solutions without interfering with other workflows. They promote modularity, reuse, and parallel development.
</details>

---

### Question 12 (Easy)
**201.4.2** | Difficulty: Easy

What are the two levels of security in a Workspace?

A) Read Group and Write Group
B) Access Group and Maintenance Group
C) Viewer Group and Editor Group
D) User Group and Admin Group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The two security levels are: Access Group (users who can view the Workspace and its dashboards but NOT modify them) and Maintenance Group (users who can access, view AND modify the Workspace and its objects).
</details>

---

### Question 13 (Easy)
**201.4.2** | Difficulty: Easy

What does the Default Workspace contain?

A) Only administrator Workspaces
B) All legacy objects (pre-Workspaces) and Cube View Groups/Data Management Groups that are not in a new Workspace
C) Only system Dashboards
D) A backup copy of all Workspaces

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Default Workspace contains all legacy objects (created before the Workspaces functionality). Cube View Groups and Data Management Groups that are not assigned to a new Workspace are located under the Default Maintenance Unit of the Default Workspace.
</details>

---

### Question 14 (Medium)
**201.4.2** | Difficulty: Medium

What two conditions must be met for a Workspace to share its objects with another Workspace?

A) Both Workspaces must have the same Access Group
B) The source Workspace must have Is Shareable = True, AND the target Workspace must list the source name in Shared Workspace Names
C) Only a global sharing enable by the administrator is needed
D) The source Workspace must export an XML file that the target imports

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Two conditions are required: (1) the source Workspace must have the property `Is Shareable Workspace = True`, and (2) the target Workspace must list the source Workspace name in its `Shared Workspace Names` property. The Default Workspace is always shared without needing to be listed.
</details>

---

### Question 15 (Medium)
**201.4.2** | Difficulty: Medium

**Scenario:** A QA team needs to review the dashboards of two different Workspaces (Annual Operating Plan and Close and Consolidation) without being able to modify anything. What security configuration is appropriate?

A) Give them the ManageApplicationWorkspaces role
B) Assign them to the Maintenance Group of both Workspaces
C) Assign them to the Access Group of both Workspaces
D) Give them the WorkspaceAdminPage role

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Access Group allows viewing the Workspace and its dashboards without being able to modify anything (content appears greyed out, indicating read-only). The Maintenance Group would allow modification, which is not appropriate for QA. The roles mentioned in other options grant administrative permissions, not read-only access.
</details>

---

### Question 16 (Medium)
**201.4.2** | Difficulty: Medium

What syntax is used to reference a Workspace Substitution Variable?

A) @WorkspaceVar.VariableName@
B) |WSSVSuffixName|
C) ${WSSV_VariableName}
D) #WSVar(VariableName)#

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspace Substitution Variables use the WSSV prefix with the syntax `|WSSVSuffixName|`. For example, `|WSSVDeveloper|` would reference a Workspace variable that stores the developer's name.
</details>

---

### Question 17 (Medium)
**201.4.2** | Difficulty: Medium

What are the 5 types of Data Adapters available in Workspaces?

A) SQL, REST, ODBC, File, Memory
B) Cube View, Cube View MD, Method, SQL, BI-Blend
C) Grid, Chart, Table, Query, Custom
D) Direct, Cached, Streaming, Batch, Real-time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The 5 types of Data Adapters are: Cube View (preconfigured, the most common), Cube View MD (multidimensional fact table for BI Viewer), Method (Business Rule or out-of-the-box queries), SQL (database queries), and BI-Blend (interface for BI Blend tables).
</details>

---

### Question 18 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** Two developers work in separate Workspaces. Both have created a Dashboard called "FinancialOverview". Is this valid and why?

A) No, Dashboard names must be unique across the entire application
B) Yes, because Workspaces allow reusing the same object name as long as they are in separate Workspaces
C) Only if one is a Shortcut to the other
D) Only if they are in the same Maintenance Unit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspaces allow using the same object name multiple times, as long as they are in separate Workspaces. This applies to Maintenance Units, Dashboards, Cube Views, Data Management Jobs, Components, Data Adapters, Files, Strings, and Assemblies. This is one of the key benefits of Workspaces for resolving naming conflicts.
</details>

---

### Question 19 (Hard)
**201.4.2** | Difficulty: Hard

What are the three Security Roles specific to Workspaces and what permission does each grant?

A) WorkspaceRead (view), WorkspaceWrite (modify), WorkspaceAdmin (administer)
B) AdministerApplicationWorkspaceAssemblies (create/modify Assemblies), ManageApplicationWorkspaces (create/modify Workspaces), WorkspaceAdminPage (access to Workspaces page in Application tab)
C) WorkspaceCreator (create), WorkspaceEditor (edit), WorkspaceViewer (view)
D) DeveloperRole (development), QARole (testing), AdminRole (administration)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three Security Roles are: AdministerApplicationWorkspaceAssemblies (allows creating and modifying Workspace Assemblies), ManageApplicationWorkspaces (allows creating and modifying Workspaces), and WorkspaceAdminPage (a UI role that provides access to the Workspaces page from the Application tab).
</details>

---

### Question 20 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** An administrator notices that Cube Views created in a new Workspace do not appear on the Cube Views page of the Application tab. Developers report they can only see these Cube Views from within the Workspace itself. What is the explanation?

A) The Cube Views have a Profile configuration error
B) Cube Views created outside the Default Workspace are NOT accessible from the Application tab's Cube View page; this is expected behavior
C) The Workspace does not have correct permissions
D) The application needs to be restarted for them to appear

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is expected and documented behavior: Cube Views created outside the Default Workspace are NOT accessible from the Application tab's Cube Views page. Only Cube Views in the Default Workspace appear there. Cube Views in other Workspaces are accessed from the corresponding Workspace. However, Dashboard Profiles and Cube View Profiles are global and remain accessible.
</details>

---

## Objective 201.4.3: Describe the steps to create a Report Book

### Question 21 (Easy)
**201.4.3** | Difficulty: Easy

What are the three types of Report Books available in OneStream?

A) Word, Excel, PowerPoint
B) PDF Books, Excel Books, Zip File Books
C) HTML Books, PDF Books, CSV Books
D) Standard Books, Custom Books, Template Books

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three types of Report Books are: PDF Books (extension .pdfBook), Excel Books (extension .xlBook), and Zip File Books (extension .zipBook).
</details>

---

### Question 22 (Easy)
**201.4.3** | Difficulty: Easy

What is the file extension of a PDF Book in OneStream?

A) .pdfReport
B) .xfDoc.pdfBook
C) .onestream.pdf
D) .rptBook.pdf

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct extension for a PDF Book is `ReportBookName.xfDoc.pdfBook`. Similarly, Excel Books use `.xfDoc.xlBook` and Zip File Books use `.xfDoc.zipBook`.
</details>

---

### Question 23 (Easy)
**201.4.3** | Difficulty: Easy

What is the recommended method for downloading a complete Report Book as PDF?

A) Print > Save as PDF
B) Export > PDF
C) Download Combined PDF File
D) Save > PDF Format

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Download Combined PDF File is the recommended method for downloading books as PDF. This option is available in the Book Preview Toolbar and generates a combined PDF of all pages in the book.
</details>

---

### Question 24 (Medium)
**201.4.3** | Difficulty: Medium

**Scenario:** A developer is creating a large Report Book with many Cube Views and notices that performance is slow when generating the book. What configuration can improve performance?

A) Reduce the number of pages
B) Set Determine Parameters from Content = False and manually specify the Required Input Parameters
C) Use only Excel Books instead of PDF Books
D) Remove all Loops

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For large books, setting `Determine Parameters from Content = False` and manually specifying the required parameters in `Required Input Parameters` (comma-separated list) improves performance. When this property is True, the system analyzes all content to determine parameters, which is expensive for large books.
</details>

---

### Question 25 (Medium)
**201.4.3** | Difficulty: Medium

What are the three types of Loop Definition available in a Report Book?

A) For Each, While, Do-While
B) Comma Separated List, Dashboard Parameter, Member Filter
C) Sequential, Parallel, Recursive
D) Entity Loop, Time Loop, Scenario Loop

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three types of Loop Definition are: Comma Separated List (comma-separated values like `Houston, Clubs, [Houston Heights]`), Dashboard Parameter (parameter name), and Member Filter (using Member Filter syntax like `E#Frankfurt, E#Houston` or `E#[NA Clubs].Base`).
</details>

---

### Question 26 (Medium)
**201.4.3** | Difficulty: Medium

What variable is used to reference the current value of a Loop in a Report Book?

A) |CurrentLoop|
B) |LoopValue|
C) |Loop1Variable|
D) |Iterator1|

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The variable `|Loop1Variable|` references the current value of the first Loop. For nested Loops, use `|Loop2Variable|`, `|Loop3Variable|`, and `|Loop4Variable|`. Additionally, `|Loop1Display|` returns the description and `|Loop1Index|` returns the numeric index.
</details>

---

### Question 27 (Medium)
**201.4.3** | Difficulty: Medium

What types of items can be added to a Report Book?

A) Only Cube Views and Dashboards
B) File, Excel Export Item, Report, Loop, Conditional Statements, and Change Parameters
C) Only PDF and Excel files
D) Only Reports and Charts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The types of items that can be added include: File (with various Source Types), Excel Export Item (for XLBooks), Report (Cube View, Dashboard Chart, Dashboard Report, etc.), Loop, Conditional Statements (If, Else If, Else), and Change Parameters.
</details>

---

### Question 28 (Hard)
**201.4.3** | Difficulty: Hard

**Scenario:** A developer needs to create a Report Book that generates a financial report for each entity in the "NA Clubs" group, but must include an additional page only for the "Frankfurt" entity. What combination of items should they use?

A) A Loop with a Comma Separated List of all entities with Frankfurt at the end
B) A Loop with Member Filter `E#[NA Clubs].Base`, a Change Parameters inside the Loop, and an If Statement with condition `(|Loop1Variable| = [Frankfurt])` for the additional page
C) Two separate Report Books combined manually
D) A Loop with Dashboard Parameter and a manual filter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct solution combines a Loop with Member Filter `E#[NA Clubs].Base` to iterate through each entity, a Change Parameters inside the Loop to update the POV with `E#[|Loop1Variable|]`, and an If Statement with the condition `(|Loop1Variable| = [Frankfurt])` to add additional content only for Frankfurt. If Statements allow conditional logic within Loops.
</details>

---

### Question 29 (Hard)
**201.4.3** | Difficulty: Hard

**Scenario:** A team needs to create an Excel Book that exports data from multiple Cube Views, each on a separate tab. What type of item should they use and what important limitation should they consider?

A) Use Report items with Cube View type; there are no limitations
B) Use Excel Export Items specifying each Cube View; Excel Books do NOT support Report items or File items
C) Use File items pointing to Excel exports; there are no limitations
D) Use Loop items with automatic exports; Excel Books do not support Loops

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Excel Books (extension .xlBook), you must use Excel Export Items, where each Cube View is exported to a separate tab. The important limitation is that Excel Books do NOT support Report items or File items. They can only contain Excel Export Items.
</details>

---

### Question 30 (Hard)
**201.4.3** | Difficulty: Hard

What are the available Source Type options when adding a File item to a Report Book?

A) Local File, Network File, Cloud File
B) URL, Application Workspace File, System Workspace File, Application Database File, System Database File, File Share File
C) OneDrive, SharePoint, Local, FTP
D) Import File, Export File, Template File

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available Source Types for File items are: URL, Application Workspace File, System Workspace File, Application Database File, System Database File, and File Share File. These options allow including files from multiple locations within the OneStream ecosystem.
</details>

---

### Question 31 (Easy)
**201.4.1** | Difficulty: Easy

How are system Substitution Variables referenced in a Cube View?

A) With braces: {WFTime}
B) With pipes: |WFTime|
C) With dollar signs: $WFTime$
D) With brackets: [WFTime]

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Substitution Variables are referenced using pipes (vertical bars). For example: `|WFTime|`, `|UserName|`, `|POVTime|`. The prefixes indicate the source of the value: WF (Workflow), POV (Cube POV), Global, etc.
</details>

---

### Question 32 (Medium)
**201.4.1** | Difficulty: Medium

What Member Filter Builder function is used to get the prior period of the current POV?

A) T#Previous1
B) T#POVPrior1
C) T#LastPeriod
D) T#POVMinus1

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The function `T#POVPrior1` returns the prior period of the current POV. Time Functions include POV, WF, Global, and General types. Another similar function is `T#YearPrior1(|PovTime|)` to get the prior year.
</details>

---

### Question 33 (Medium)
**201.4.1** | Difficulty: Medium

What types of Spreading are available when right-clicking a cell in an editable Cube View?

A) Only Copy and Paste
B) Fill, Clear Data, Factor, Accumulate, Even Distribution, Proportional Distribution, 445/454/544 Distribution
C) Only Even Distribution and Proportional Distribution
D) Linear, Exponential, Logarithmic

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available Spreading types are: Fill, Clear Data, Factor, Accumulate, Even Distribution, Proportional Distribution, and calendar distribution types 445/454/544. These allow distributing values in different ways across the Cube View cells.
</details>

---

### Question 34 (Hard)
**201.4.2** | Difficulty: Hard

What are the Dashboard types available in OneStream and what is the difference between Top Level and Embedded?

A) Only Standard and Custom exist
B) Use Default, Top Level, Top Level Without Parameter Prompts, Embedded, Embedded Dynamic, Embedded Dynamic Repeater, Custom Control; Top Level is exposed in the OnePlace main menu while Embedded is nested inside another Dashboard
C) Only Top Level and Embedded; both are accessible from OnePlace
D) Standard, Premium, Enterprise; they differ only in the number of components

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Dashboard types are: Use Default, Top Level (exposed in the OnePlace main menu), Top Level Without Parameter Prompts (without prompting for parameters), Embedded (nested inside another Dashboard, not directly visible in OnePlace), Embedded Dynamic (for Workspace Assembly objects), Embedded Dynamic Repeater (multiple instances with Template Repeat Items), and Custom Control (uses Event Listeners).
</details>

---

### Question 35 (Easy)
**201.4.2** | Difficulty: Easy

What type of Data Adapter is the most common in Workspaces?

A) SQL
B) Method
C) Cube View
D) BI-Blend

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Cube View type Data Adapter is the most common. It uses a preconfigured Cube View as a data source for Dashboard components.
</details>

---

### Question 36 (Medium)
**201.4.2** | Difficulty: Medium

What are the 6 types of Parameters available in Workspaces?

A) Text, Number, Date, Boolean, List, Custom
B) Literal Value, Input Value, Delimited List, Bound List, Member List, Member Dialog
C) String, Integer, Float, Array, Object, Enum
D) Simple, Complex, Dynamic, Static, Computed, Reference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The 6 types of Parameters are: Literal Value (explicit value), Input Value (holding parameter), Delimited List (manual list), Bound List (member lists using Method Types), Member List (uses Member Filter Builder), and Member Dialog (list with selection tree).
</details>

---

### Question 37 (Easy)
**201.4.3** | Difficulty: Easy

After creating a Report Book, what are the ways to use it?

A) It can only be viewed in the Application tab
B) Add it to other books, dashboards (via Book Viewer or File Viewer), send it by email via OneStream Parcel Service, generate it via Data Management, store it on FileShare/desktop/dashboard file
C) It can only be exported as PDF
D) It can only be viewed in OnePlace

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Report Books are very versatile: they can be added to other books, incorporated into dashboards via Book Viewer or File Viewer, sent by email via OneStream Parcel Service, generated by running a Data Management sequence, and stored on FileShare, desktop, or as a dashboard file.
</details>

---

### Question 38 (Medium)
**201.4.3** | Difficulty: Medium

**Scenario:** A developer has a Loop in a Report Book that iterates through entities. They need the report's POV to change to the current Loop entity without modifying the original Cube View. What should they use and what is the syntax?

A) Edit the Cube View directly for each entity
B) Add a Change Parameters inside the Loop with type POV and syntax such as E#[|Loop1Variable|]:A#Sales
C) Create a Cube View Shortcut for each entity
D) Use an external Dashboard Parameter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You should use Change Parameters inside the Loop with type POV. The syntax `E#[|Loop1Variable|]:A#Sales` changes the entity to the current Loop value without modifying the original Cube View. Change Parameters is required within Loops to update the POV variable.
</details>

---

### Question 39 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A user needs to create a Cube View that displays the children of "Income Statement" excluding members whose name contains "Tax". What Member Filter syntax should they use?

A) A#[Income Statement].Children NOT Tax
B) A#[Income Statement].Children.Remove(A#[Income Statement].Children.Where(Name Contains Tax))
C) A#[Income Statement].Descendants.Where(Name Contains Tax).Exclude
D) A#[Income Statement].Children, -A#Tax

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Member Filter Builder supports expansion functions like `.Children` and removal functions with `.Remove()`. Combined with `.Where(Name Contains Tax)`, you can get the list of children and remove those containing "Tax" in their name. Where clauses allow filtering by Name Contains, StartsWith, EndsWith, HasChildren, etc.
</details>

---

### Question 40 (Hard)
**201.4.3** | Difficulty: Hard

What is the correct syntax for an If Statement in a Report Book that includes multiple conditions?

A) IF Loop1Variable = Frankfurt AND Loop1Variable = Houston
B) (|Loop1Variable| = [Frankfurt]) Or (|Loop1Variable| = [Houston])
C) |Loop1Variable| == "Frankfurt" || |Loop1Variable| == "Houston"
D) If(Loop1Variable, Frankfurt, Houston)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct syntax uses parentheses for each condition and Or/And logical operators. For example: `(|Loop1Variable| = [Frankfurt]) Or (|Loop1Variable| = [Houston])`. You can also combine different variables: `(|!UserName!| = Administrator) Or (|!UserName!| = JSmith)`.
</details>

---

## NEW QUESTIONS (41-84)

## Objective 201.4.1: Cube View configurations (continued)

### Question 41 (Easy)
**201.4.1** | Difficulty: Easy

What keyboard shortcut saves data in a Cube View within OnePlace?

A) CTRL+Enter
B) CTRL+S
C) F5
D) CTRL+D

<details>
<summary>Show answer</summary>

**Correct answer: B)**

CTRL+S is the keyboard shortcut to save data in a Cube View. This is important when working with editable Cube Views (`Can Modify Data = True`) where users enter or modify data.
</details>

---

### Question 42 (Medium)
**201.4.1** | Difficulty: Medium

What are the scaling shortcuts available when entering data into an editable Cube View?

A) K for hundred, M for thousand, B for million
B) k for thousand, m for million, b for billion
C) k for ten, m for hundred, b for thousand
D) There are no scaling shortcuts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The scaling shortcuts are: k = 1,000 (thousand), m = 1,000,000 (million), and b = 1,000,000,000 (billion). For example, entering "5k" sets the cell value to 5,000.
</details>

---

### Question 43 (Easy)
**201.4.1** | Difficulty: Easy

What property must be set to `True` on a Cube View to enable it for use with a Dynamic Cube View Service via Workspace Assembly?

A) Can Modify Data
B) Is Visible in Profiles
C) Is Dynamic
D) Allow Sparse Row Suppression

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Setting `Is Dynamic = True` on a Cube View enables the Dynamic Cube View Service, allowing the Cube View to be manipulated programmatically through a Workspace Assembly.
</details>

---

### Question 44 (Medium)
**201.4.1** | Difficulty: Medium

Which GetDataCell expression types are available for Cube View calculated columns and rows?

A) Sum, Average, Count, Min, Max
B) Variance, VariancePercent, BWDiff, BWPercent, Divide, Subtraction, Addition, BR# (custom function)
C) Only Addition and Subtraction
D) Average, Median, StdDev, Percentile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

GetDataCell expressions include: Variance, VariancePercent, BWDiff (budget vs. actual difference), BWPercent, Divide, Subtraction, Addition, and BR# for custom Business Rule functions. These are used to create calculated columns and rows within Cube Views.
</details>

---

### Question 45 (Medium)
**201.4.1** | Difficulty: Medium

What is the purpose of the `CVMathOnly` option when configuring a Cube View column?

A) It makes the column editable for math input only
B) It hides the column from display but keeps it available for math expressions in other columns
C) It restricts the column to integer values only
D) It enables formula auditing for the column

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The `CVMathOnly` option hides a column from the rendered display but keeps it available for use in math expressions by other columns or rows. This is useful when intermediate calculation columns are needed but should not be visible to end users.
</details>

---

### Question 46 (Medium)
**201.4.1** | Difficulty: Medium

When configuring Navigation Links in a Cube View, which properties must be set and what parameter syntax is used?

A) Enable Navigation = True; uses {dimensionName} syntax
B) Enable Report Navigation Link = True and Include Default NavLink Parameters = True; uses |!dimensionNavLink!| syntax
C) Allow Drill = True; uses @dimension@ syntax
D) Navigation Mode = Active; uses $NavParam$ syntax

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To enable Navigation Links, set `Enable Report Navigation Link = True` on the row/column, and `Include Default NavLink Parameters = True` to automatically generate dimension parameters. The default NavLink parameters use the `|!dimensionNavLink!|` syntax pattern.
</details>

---

### Question 47 (Medium)
**201.4.1** | Difficulty: Medium

What property must be enabled on a Data Adapter to allow drill-down from Cube Views displayed in dashboards?

A) Allow Drill Through = True
B) Include Row Navigation Link = True
C) Enable Data Exploration = True
D) Cube View Drill Mode = Active

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Setting `Include Row Navigation Link = True` on the Data Adapter enables drill-down navigation from Cube Views rendered within dashboard components. This creates clickable links on rows that navigate to linked reports.
</details>

---

### Question 48 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A developer is building a Cube View with 3 nested dimensions in rows. They want the first two levels to be automatically expanded when the Cube View loads, but the third level should be collapsed. Additionally, the Cube View will be shown inside a dashboard and needs red tick marks on cells that have data attachments. What configuration is needed?

A) Set TreeExpansionLevel = 3 and Include Cell Attachment Status = True
B) Set TreeExpansionLevel = 2 and Include Cell Attachment Status = True
C) Set AutoExpand = 2 and Show Attachments = True
D) Set ExpandDepth = 2 and AttachmentIndicator = Enabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

TreeExpansionLevel controls how many nested row levels are automatically expanded (values 1-4). Setting it to 2 expands the first two levels while keeping the third collapsed. `Include Cell Attachment Status = True` displays a red tick mark on cells that have data attachments. Both settings are configured in the Cube View properties.
</details>

---

### Question 49 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A Cube View needs conditional formatting so that cells with no data are displayed with a grey background, even-numbered rows have a light blue background, and cells where the amount is negative are shown in red font. Which conditional formatting property filters should the developer use?

A) NoDataStyle, AlternateRows, NegativeValues
B) IsNoData for grey background, IsRowNumberEven for light blue, CellAmount for red font on negatives
C) EmptyCell, RowParity, ValueSign
D) DataStatus, RowIndex, CellValue

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Conditional formatting property filters include: `IsNoData` to identify cells without data, `IsRowNumberEven` to target alternating rows, and `CellAmount` to apply formatting based on cell values (e.g., negative amounts). Other available filters include IsRealData, MemberName, HeaderDisplayText, IndentLevel, and ExpandedRowNum.
</details>

---

### Question 50 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A developer has two Cube Views that share the same row definitions — one for Budget data and one for Actual data. Maintaining the same rows in both views is becoming error-prone. What feature should they use to ensure consistency?

A) Create a Template Cube View and link both views to it
B) Use Cube View row sharing — define the rows in one Cube View and reference them from the other
C) Export and import the row definitions between views
D) Use a Business Rule to synchronize the rows at runtime

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views support row sharing and column sharing, where row or column definitions are defined in one Cube View and referenced from another. This ensures consistency across multiple views and eliminates the need for duplicate maintenance.
</details>

---

### Question 51 (Medium)
**201.4.1** | Difficulty: Medium

In a Cube View, what are the available Cell Types that can be configured for data entry?

A) Only Text Box
B) Text Box (default/numerical), Combo Box (with List Parameter), Date, and Date Time
C) Text, Number, Currency, Percentage
D) Input, Dropdown, Calendar, Checkbox

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available Cell Types for Cube View cells are: Text Box (default, for numerical data entry), Combo Box (used in conjunction with a List Parameter for dropdown selection), Date, and Date Time. The Combo Box type requires an associated parameter to populate the dropdown options.
</details>

---

### Question 52 (Medium)
**201.4.1** | Difficulty: Medium

What Cube View Visibility settings are available in Cube View Profiles?

A) Visible and Hidden
B) Always, a specific location, or Never
C) Public, Private, Restricted
D) Global, Workspace, Local

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube View Profiles support three Visibility settings: Always (visible in all contexts), a specific location (visible only in a designated area), or Never (hidden from all views). These profiles are global — they are not workspace-specific.
</details>

---

### Question 53 (Easy)
**201.4.1** | Difficulty: Easy

What math shortcut keywords can be used when entering data in a Cube View cell?

A) Only + and - symbols
B) add, sub, div, mul, increase/in/gr, decrease/de, percent/per, power/pow
C) plus, minus, times, divideby
D) There are no math shortcuts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Views support math shortcut keywords: add, sub (subtract), div (divide), mul (multiply), increase/in/gr (increase by percentage), decrease/de (decrease by percentage), percent/per (percentage), and power/pow. For example, typing "add 100" adds 100 to the current cell value.
</details>

---

### Question 54 (Hard)
**201.4.1** | Difficulty: Hard

**Scenario:** A developer is building a Cube View that uses CVC (Cube View Column) and CVR (Cube View Row) math expressions. They have Column A showing Actual values and Column B showing Budget values. They want Column C to show the variance (Actual - Budget) and Column D to show variance percentage, but Column B should be hidden from the final output since it is only needed for calculations. How should this be configured?

A) Delete Column B after creating the math expressions
B) Set Column B to `CVMathOnly`, configure Column C using a Subtraction GetDataCell referencing columns A and B, and Column D using VariancePercent GetDataCell
C) Use a Business Rule to hide Column B at runtime
D) Create Column C with manual formulas and remove Column B

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Set Column B to `CVMathOnly` to hide it from display while keeping it available for calculations. Column C uses a Subtraction (or Variance) GetDataCell expression referencing the Actual (A) and Budget (B) columns. Column D uses VariancePercent GetDataCell. The CVMathOnly option is specifically designed for intermediate calculation columns that should not be visible to users.
</details>

---

## Objective 201.4.2: Workspace and Dashboard configuration (continued)

### Question 55 (Easy)
**201.4.2** | Difficulty: Easy

What component type is used to prevent parameter prompts from appearing and pass parameters silently between dashboards?

A) Hidden Parameter
B) Supplied Parameter
C) Silent Parameter
D) Pass-Through Parameter

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Supplied Parameter component is a hidden component that prevents parameter prompts from appearing. It passes parameters silently from one dashboard to another, allowing seamless navigation without user interaction for parameter selection.
</details>

---

### Question 56 (Medium)
**201.4.2** | Difficulty: Medium

Where does the Logo component in a dashboard pull its image from?

A) A file uploaded to the Workspace
B) Application Properties
C) The user's local machine
D) A URL specified in the component

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Logo component pulls its image from Application Properties. This ensures that the application logo is consistent across all dashboards without requiring separate image management. This is one of the 38 available component types in OneStream dashboards.
</details>

---

### Question 57 (Medium)
**201.4.2** | Difficulty: Medium

What is the best practice for naming and sorting parameters in a dashboard maintenance unit?

A) Use sequential numbering and alphabetical sort
B) Prefix names with "Param", use staggered sort order with gaps of 20, and avoid special characters or dashes
C) Use short cryptic names and sort by creation date
D) No naming convention is needed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Best practices for parameters include: prefixing names with "Param" (e.g., ParamEntity, ParamAccount), using a staggered sort order with gaps of 20 (e.g., 20, 40, 60) to allow insertion of new parameters later, and avoiding special characters or dashes in parameter names.
</details>

---

### Question 58 (Medium)
**201.4.2** | Difficulty: Medium

What happens when a parameter uses the `|!!ParameterName!!|` syntax (double exclamation marks) instead of `|!ParameterName!|`?

A) It makes the parameter required
B) It references the display item of a Delimited List parameter instead of the value item
C) It creates a parameter with validation
D) It makes the parameter case-sensitive

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The double exclamation mark syntax `|!!ParameterName!!|` references the display item of a Delimited List parameter, while the single exclamation mark syntax `|!ParameterName!|` references the value item. This distinction is important when the display text differs from the underlying value in a Delimited List.
</details>

---

### Question 59 (Medium)
**201.4.2** | Difficulty: Medium

How are nested parameters used in OneStream dashboards?

A) Parameters are placed inside other parameters physically
B) One parameter references another parameter to provide granular filtering — the selection in one parameter narrows the options in another
C) Parameters are grouped into folders
D) Nested parameters are created using XML configuration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Nested parameters allow one parameter to reference another for granular filtering. For example, a Region parameter selection can filter the options available in an Entity parameter, so only entities belonging to the selected region are shown.
</details>

---

### Question 60 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer in Workspace "ProjectAlpha" needs to display a dashboard from Workspace "SharedServices" inside one of their dashboards. The SharedServices Workspace has `Is Shareable Workspace = True`, and ProjectAlpha lists "SharedServices" in its `Shared Workspace Names`. What are the two methods to display the shared dashboard?

A) Copy the dashboard to ProjectAlpha and modify it
B) Use an Embedded Dashboard component or a Button component configured to Open Dialog, referencing the dashboard with `SharedServices.DashboardName` syntax
C) Export from SharedServices and import to ProjectAlpha
D) Create a Cube View that references the shared dashboard

<details>
<summary>Show answer</summary>

**Correct answer: B)**

There are two methods to display shared dashboards from another Workspace: (1) Use an Embedded Dashboard component that references the shared dashboard, or (2) Use a Button component configured with the Open Dialog action. Both methods can reference the shared dashboard using Direct Reference syntax: `WorkspaceName.ObjectName` (e.g., `SharedServices.DashboardName`).
</details>

---

### Question 61 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** An architect is designing a dashboard solution and needs to decide between using the Embedded Dynamic Repeater (no-code) approach and the Embedded Dynamic (code-based) approach for generating dynamic components. What key difference determines which approach to use?

A) The Embedded Dynamic Repeater is faster; the code-based approach is slower
B) The Embedded Dynamic Repeater uses Component Template Repeat Items with `~!templateParam!~` syntax for settings that accept text values, while the code-based approach uses Assembly Services and can modify settings that do not accept text values (such as save actions, POV actions, and navigation actions)
C) The code-based approach is only for SQL queries
D) There is no functional difference; it is a matter of personal preference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Embedded Dynamic Repeater is a no-code approach using Component Template Repeat Items and template parameters with `~!templateParam!~` syntax. It works well for settings that accept text values. The code-based Embedded Dynamic approach uses Assembly Services (Dynamic Dashboard Service) and can modify settings that don't accept text values, such as save actions, POV actions, and navigation actions. The code-based approach provides more flexibility for complex scenarios.
</details>

---

### Question 62 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer is configuring a Dynamic Dashboard using the code-based approach with Workspace Assemblies. They need to set the Workspace Assembly Service property on the dashboard. What is the correct syntax for this property, and what are the key methods they will need to implement?

A) Service = "AssemblyName"; methods: CreateDashboard(), RenderDashboard()
B) Workspace Assembly Service = "AssemblyName.FactoryName"; key methods include GetEmbeddedDynamicDashboard, GetDynamicComponentsForDynamicDashboards, GetDynamicAdaptersForDynamicComponent, GetDynamicCubeViewForDynamicAdapter, GetDynamicParametersForDynamicComponent
C) Assembly Reference = "Namespace.Class"; methods: Initialize(), Execute()
D) Dynamic Service = "WorkspaceName.ServiceName"; methods: BuildDashboard(), LoadComponents()

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Workspace Assembly Service property uses the syntax `AssemblyName.FactoryName`. The key methods in the Dynamic Dashboard Service include: `GetEmbeddedDynamicDashboard` (entry point), `GetDynamicComponentsForDynamicDashboards` (defines components), `GetDynamicAdaptersForDynamicComponent` (defines data adapters), `GetDynamicCubeViewForDynamicAdapter` (defines cube views), and `GetDynamicParametersForDynamicComponent` (defines parameters). These methods work together to programmatically generate dashboard content.
</details>

---

### Question 63 (Medium)
**201.4.2** | Difficulty: Medium

What is the template parameter syntax used in Embedded Dynamic Repeater dashboards, and how does it differ from standard parameter syntax?

A) Both use |!ParameterName!| — there is no difference
B) Template parameters use `~!templateParam!~` while standard parameters use `|!ParameterName!|`
C) Template parameters use {{templateParam}} while standard parameters use |!ParameterName!|
D) Template parameters use <%templateParam%> while standard parameters use |!ParameterName!|

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Embedded Dynamic Repeater dashboards use template parameter syntax `~!templateParam!~` (tilde and exclamation marks) for Component Template Repeat Items, which is different from the standard parameter syntax `|!ParameterName!|` (pipes and exclamation marks) used in regular dashboards and Cube Views.
</details>

---

### Question 64 (Medium)
**201.4.2** | Difficulty: Medium

What fields on a Workspace are used to store string values that can be referenced in Workspace Assemblies?

A) Variables 1-8
B) Text1 through Text8
C) String Fields A-H
D) Custom Properties 1-8

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspaces have Text1 through Text8 fields that store string values. These values can be referenced in Workspace Assemblies, providing a way to pass configuration data from the Workspace to assembly code without hardcoding values.
</details>

---

### Question 65 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer is extracting a Workspace for deployment to another environment. What happens when a Workspace is extracted, and what items are included in the package?

A) Only dashboards are exported as individual XML files
B) Extracting a Workspace packages all related items — dashboards, cube views, data management jobs, assemblies, parameters, components, data adapters, files, and strings — in a single XML file
C) Only the Workspace configuration is exported; objects must be exported separately
D) A ZIP file is created with separate folders for each object type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extracting a Workspace packages all related items into a single XML file. This includes dashboards, cube views, data management jobs, assemblies, parameters, components, data adapters, files, strings, and maintenance units. This comprehensive packaging simplifies deployment and migration between environments.
</details>

---

### Question 66 (Easy)
**201.4.2** | Difficulty: Easy

How many component types are available for building dashboards in OneStream?

A) 12
B) 25
C) 38
D) 50

<details>
<summary>Show answer</summary>

**Correct answer: C)**

There are 38 component types available for building dashboards in OneStream. These range from data display components (Cube View, BI Viewer, Chart, Pivot Grid) to input components (Text Box, Combo Box, Date Selector) to layout components (Label, Image, Logo) and more.
</details>

---

### Question 67 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer needs to use Workspace Assemblies across different Workspaces. What Workspace properties enable cross-referencing of assemblies between Workspaces?

A) Shared Assembly Names and Assembly Access Group
B) Namespace Prefix and Imports Namespace 1-8
C) Assembly Sharing = True and Target Workspace Names
D) Cross-Reference Mode and Assembly Path

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Namespace Prefix property and the Imports Namespace 1-8 properties on a Workspace enable cross-referencing of assemblies between Workspaces. The Namespace Prefix identifies the Workspace's assembly namespace, while Imports Namespace 1-8 allow referencing assembly namespaces from other Workspaces.
</details>

---

## Objective 201.4.3: Report Book configuration (continued)

### Question 68 (Easy)
**201.4.3** | Difficulty: Easy

What are the types of Report items that can be added to a PDF Report Book?

A) Only Cube View Reports
B) Cube View, Dashboard Chart, Dashboard Report, System Dashboard Chart, System Dashboard Report
C) PDF, Word, Excel, PowerPoint
D) Table, Chart, Image, Text

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Report item types available for PDF Report Books include: Cube View, Dashboard Chart, Dashboard Report, System Dashboard Chart, and System Dashboard Report. These allow incorporating various OneStream visualization outputs into the book.
</details>

---

### Question 69 (Medium)
**201.4.3** | Difficulty: Medium

What are the four types of Change Parameters items available in a Report Book?

A) Entity, Account, Time, Scenario
B) Workflow, POV, Variables (up to 10), Parameters
C) Input, Output, Filter, Sort
D) User, Role, Group, Application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four types of Change Parameters items are: Workflow (changes the Workflow context), POV (changes Point of View dimensions), Variables (up to 10 custom variables), and Parameters (changes dashboard parameter values). These allow dynamic modification of the report context during book generation.
</details>

---

### Question 70 (Hard)
**201.4.3** | Difficulty: Hard

**Scenario:** A developer has a Report Book with a Loop that iterates through 50 entities using Member Filter. Inside the loop, they need to display the entity description as a header on each page. What Loop variable provides the description, and what variable provides the numeric position of the current iteration?

A) |Loop1Name| for description and |Loop1Count| for position
B) |Loop1Display| for description and |Loop1Index| for numeric position
C) |Loop1Description| for description and |Loop1Position| for position
D) |Loop1Label| for description and |Loop1Number| for position

<details>
<summary>Show answer</summary>

**Correct answer: B)**

`|Loop1Display|` returns the description (display text) of the current Loop member, while `|Loop1Index|` returns the numeric index (position) of the current iteration. Combined with `|Loop1Variable|` which returns the member name/value, these three variables provide complete context about the current loop iteration.
</details>

---

## Objective 201.4.4: Describe Extensible Documents (XFDs, Quick Views)

### Question 71 (Easy)
**201.4.4** | Difficulty: Easy

What file types are supported by the Extensible Documents framework in OneStream?

A) Only Word and Excel
B) Word (.xfdoc.docx), Excel (.xfdoc.xlsx), PowerPoint (.xfdoc.pptx), and Text (.xfdoc.txt)
C) PDF, Word, Excel, HTML
D) Only Excel and Text

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Extensible Documents framework supports four file types: Word documents (.xfdoc.docx), Excel workbooks (.xfdoc.xlsx), PowerPoint presentations (.xfdoc.pptx), and Text files (.xfdoc.txt). This framework integrates OneStream with Microsoft Office and text files.
</details>

---

### Question 72 (Easy)
**201.4.4** | Difficulty: Easy

What types of content can be inserted into an Extensible Document?

A) Only text and numbers
B) Cube Views, Excel Sheets/Named Ranges, Word documents, Rich Text documents, Text documents, and Reports
C) Only Cube Views
D) Only images and charts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Extensible Documents can incorporate multiple content types: Cube Views, Excel Sheets and Named Ranges, Word documents, Rich Text documents, Text documents, and Reports. This makes them versatile for creating comprehensive financial documents.
</details>

---

### Question 73 (Easy)
**201.4.4** | Difficulty: Easy

What is the maximum size for an Excel Named Range that can be inserted into an Extensible Document?

A) 100 rows x 10 columns
B) 500 rows x 50 columns
C) 1,000 rows x 100 columns
D) There is no limit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Excel Named Ranges inserted into Extensible Documents have a maximum size of 500 rows by 50 columns. This limitation should be considered when designing Excel-based content for extensible documents.
</details>

---

### Question 74 (Medium)
**201.4.4** | Difficulty: Medium

What is the XFCell function used for in Extensible Documents, and where is it typically used?

A) XFCell formats cells in Excel documents
B) XFCell retrieves a single cell's data for use in text-based documents (Word and PowerPoint), using syntax like `XFCell(A#20500:E#Clubs)`
C) XFCell creates new cells in a Cube View
D) XFCell validates cell data types

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFCell retrieves single cell data from OneStream for use in text-based documents such as Word and PowerPoint. The syntax specifies dimension members to identify the data point, for example: `XFCell(A#20500:E#Clubs)`. This allows embedding individual data values within narrative text documents.
</details>

---

### Question 75 (Medium)
**201.4.4** | Difficulty: Medium

What is the difference between XFGetCell and XFGetCellVolatile in Excel-based Extensible Documents?

A) XFGetCell is for text; XFGetCellVolatile is for numbers
B) XFGetCell is the standard Excel retrieve function; XFGetCellVolatile is used when charts need to refresh with updated data
C) XFGetCell reads data; XFGetCellVolatile writes data
D) There is no functional difference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFGetCell is the standard function for retrieving data from OneStream into Excel cells. XFGetCellVolatile serves the same data retrieval purpose but is specifically used when charts need to refresh with updated data, as the volatile flag forces Excel to recalculate and refresh chart visualizations.
</details>

---

### Question 76 (Medium)
**201.4.4** | Difficulty: Medium

How are Document Variables inserted into Word-based Extensible Documents?

A) Using OneStream-specific tags in the document
B) Using Quick Parts > Field > DocVariable in Word or the Text Editor
C) Using macros in the Word template
D) Using the Insert > Reference menu

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Document Variables are inserted into Word-based Extensible Documents using Quick Parts > Field > DocVariable. This standard Word functionality integrates with OneStream's extensible document framework to dynamically populate document content.
</details>

---

### Question 77 (Medium)
**201.4.4** | Difficulty: Medium

What keyboard shortcut toggles the display of field codes in an Extensible Document, and what merge switch preserves formatting?

A) CTRL+F9 to toggle; \*FORMAT to preserve
B) Alt+F9 (or Alt+Fn+F9) to toggle; \*MERGEFORMAT to preserve formatting
C) F5 to toggle; \*KEEP to preserve
D) CTRL+Shift+F to toggle; \*STYLE to preserve

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Alt+F9 (or Alt+Fn+F9 on some keyboards) toggles the Show/Hide Field Codes display in Word-based extensible documents. The `\*MERGEFORMAT` switch preserves the existing formatting of the field when the document is refreshed with new data.
</details>

---

### Question 78 (Hard)
**201.4.4** | Difficulty: Hard

**Scenario:** A developer is creating a Word-based Extensible Document that includes images whose content should change based on document variable arguments. How should images be configured to reference document variable arguments in a Word Extensible Document?

A) Use the Image Source property to reference the variable
B) Use the Alt Text property of the image to reference document variable arguments
C) Use a VBA macro to swap image sources
D) Create separate images for each variable value

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In Word-based Extensible Documents, the Alt Text property of an image is used to reference document variable arguments. This allows images to be dynamically associated with data from OneStream through the extensible document framework.
</details>

---

### Question 79 (Hard)
**201.4.4** | Difficulty: Hard

**Scenario:** A developer needs to create a Word Extensible Document that includes both Cube Views and individual data values embedded in paragraph text. For the Cube Views, they want to use content controls. What type of content control is available for Word, and what tool provides the correct syntax for all extensible document elements?

A) Data Binding Content Controls; use the Formula Builder for syntax
B) Rich Text Content Controls for Cube Views, dashboard reports, Word/RTF/TXT documents; use the Object Lookup dialog for all extensible document syntax
C) Plain Text Content Controls; use the Member Filter Builder for syntax
D) ActiveX Controls; use the Code Editor for syntax

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Rich Text Content Controls are available exclusively in Word-based extensible documents. They can contain Cube Views, dashboard reports, Word documents, Rich Text documents, and text documents. The Object Lookup dialog provides the correct syntax for all extensible document elements, serving as a central reference for building extensible document expressions.
</details>

---

### Question 80 (Hard)
**201.4.4** | Difficulty: Hard

**Scenario:** A developer is tasked with creating an Excel-based Extensible Document that needs to display a Cube View in one section and individual cell values in another, with a chart that automatically updates when data refreshes. Which functions should they use for the individual cells and the chart data source?

A) Use XFCell for individual cells and standard Excel chart references
B) Use XFGetCell for individual cell retrieval and XFGetCellVolatile for cells used as chart data sources to ensure charts refresh properly
C) Use XFRetrieve for all data and XFChart for chart data
D) Use GetData for cells and RefreshChart for charts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For individual cell data retrieval in Excel-based Extensible Documents, use XFGetCell. For cells that serve as data sources for charts, use XFGetCellVolatile. The volatile version forces Excel to recalculate on refresh, which triggers chart updates. Standard XFGetCell values may not trigger chart refreshes because Excel optimization may skip recalculating non-volatile cells.
</details>

---

### Question 81 (Medium)
**201.4.4** | Difficulty: Medium

How do you refresh an Extensible Document in the Text Editor within OneStream?

A) Press F5
B) Click the Refresh Document button in the OneStream ribbon within the Text Editor
C) Right-click and select Refresh
D) Close and reopen the document

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To refresh an Extensible Document, click the Refresh Document button available in the OneStream ribbon within the Text Editor. This updates all dynamic content (Cube Views, XFCell values, document variables, etc.) with the latest data from OneStream.
</details>

---

### Question 82 (Medium)
**201.4.2** | Difficulty: Medium

According to Foundation Handbook best practices, what is the "5-second rule" for Cube View performance?

A) Users should wait at least 5 seconds before interacting with a Cube View
B) If a Cube View takes more than 5 seconds to load, it should be optimized for better performance
C) Cube Views should auto-refresh every 5 seconds
D) Data entry changes are saved after a 5-second delay

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "5-second rule" is a performance guideline from the Foundation Handbook: if a Cube View takes more than 5 seconds to load, it should be optimized. Optimization strategies include minimizing Data Unit dimensions in rows, using Sparse Row Suppression, and ensuring efficient member filter queries.
</details>

---

### Question 83 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A solution architect is planning dashboards for a new OneStream implementation. They need to categorize their dashboard requirements. According to the Foundation Handbook, what are the three types of dashboard data consumption, and how do they differ?

A) Input, Output, and Analysis
B) Static Analysis (fixed views for review), Interactive Analytics (user-driven exploration with filters and drill-downs), and Functional Interaction (data input, workflow actions, task execution)
C) Real-Time, Batch, and On-Demand
D) Summary, Detail, and Transactional

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Foundation Handbook identifies three dashboard data consumption types: Static Analysis (fixed views presenting data for review without user interaction), Interactive Analytics (user-driven exploration with filters, drill-downs, and parameter changes), and Functional Interaction (dashboards that enable data input, workflow actions, and task execution). Understanding these categories helps architects design appropriate dashboard solutions.
</details>

---

### Question 84 (Hard)
**201.4.2** | Difficulty: Hard

**Scenario:** A developer is setting up dynamic components using the code-based Dynamic Dashboard approach. When dynamic components are generated at runtime, they receive a name suffix. What is the naming convention for dynamic components, and what key classes are used in the Dynamic Dashboard Service?

A) Components get a "_generated" suffix; uses DashboardBuilder and ComponentFactory classes
B) Components get a suffix of `_dynamic_"Template Name Suffix"`; key classes include WsDynamicComponent, WsDynamicCollection, WsDynamicDashboardEx, WsDynamicRepeatArgs, WsDynamicParameterCollection, and WsDynamicComponentRepeatArgs
C) Components get a "_runtime" suffix; uses DynamicPanel and DynamicControl classes
D) Components get no suffix; uses StandardComponent and StandardAdapter classes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dynamic components generated at runtime receive a suffix of `_dynamic_"Template Name Suffix"` appended to the base component name. The key classes in the Dynamic Dashboard Service include: WsDynamicComponent (defines a component), WsDynamicCollection (collection of dynamic objects), WsDynamicDashboardEx (extended dashboard definition), WsDynamicRepeatArgs (repeat arguments), WsDynamicParameterCollection (parameter collection), and WsDynamicComponentRepeatArgs (component repeat arguments). These classes provide the programmatic foundation for building dynamic dashboards.
</details>

---


# Question Bank - Section 5: Tools (9% of exam)

## Objective 201.5.1: Data Management Steps and Their Use Cases

### Question 1 (Easy)
**201.5.1** | Difficulty: Easy

What is the correct hierarchical structure of Data Management, from highest to lowest level?

A) Step > Sequence > Group > Profile
B) Profile > Group > Sequence > Step
C) Group > Profile > Sequence > Step
D) Profile > Sequence > Group > Step

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The hierarchical structure is: Profile (organizes Groups for presentation) > Group (top-level container) > Sequence (ordered series of Steps) > Step (individual unit of work). Option A is inverted, and options C and D mix the order incorrectly.
</details>

---

### Question 2 (Easy)
**201.5.1** | Difficulty: Easy

What are the 6 built-in Data Management Step types in OneStream?

A) Calculate, Clear Data, Copy Data, Custom Calculate, Execute Business Rule, Export Data
B) Calculate, Clear Data, Copy Data, Import Data, Execute Business Rule, Export Data
C) Calculate, Clear Data, Copy Data, Custom Calculate, Reset Scenario, Export Data
D) Calculate, Clear Data, Move Data, Custom Calculate, Execute Business Rule, Export Report

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The 6 built-in types are: Calculate, Clear Data, Copy Data, Custom Calculate, Execute Business Rule, and Export Data. Import Data is not a Step type (B). Reset Scenario is an additional Step but not one of the 6 main types (C). Move Data and Export Report are not built-in main Step types (D).
</details>

---

### Question 3 (Medium)
**201.5.1** | Difficulty: Medium

What is the main difference between "Calculate" and "Consolidate" as calculation types within a Calculate Step?

A) Calculate executes translations; Consolidate only executes local calculations
B) Calculate executes calculation at the Entity level within the Local member; Consolidate executes Calculate, Translate, and completes calculations across the entire Consolidation Dimension
C) Both do the same thing but Consolidate is faster
D) Calculate works with a single Cube; Consolidate works with multiple Cubes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Calculate executes calculation at the Entity level within the Local member of the Consolidation Dimension, without translating or consolidating. Consolidate is the most complete: it executes Calculate, then Translate, and finally completes calculations across the entire Consolidation Dimension. Option A reverses the functions, and option D does not reflect the actual behavior.
</details>

---

### Question 4 (Medium)
**201.5.1** | Difficulty: Medium

An administrator needs to run a quick calculation on a slice of data without generating audit information for each cell. What type of Step should they use?

A) Calculate
B) Execute Business Rule
C) Custom Calculate
D) Copy Data

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Custom Calculate executes a calculation on a slice of data within one or more Data Units using a Finance Business Rule. Its main advantage is that it does not create audit information for each cell, which makes it faster than Copy Data. Calculate executes built-in calculations but generates audit information. Execute Business Rule runs an Extensibility Business Rule, not a calculation on slices.
</details>

---

### Question 5 (Medium)
**201.5.1** | Difficulty: Medium

**Scenario:** A user tries to execute a Custom Calculate Step but receives an error and the Step fails. The user has access to the Cube and the corresponding Scenario. What is the most likely cause of the failure?

A) The user does not have the ModifyData role
B) The user is not a member of the Manage Data Group of the Scenario
C) The Custom Calculate Step does not support parameters
D) The Cube does not have the Consolidation Dimension configured

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To execute a Custom Calculate Step, the user must be a member of the Manage Data Group of the Scenario; otherwise, the Step will fail. The ModifyData role (A) controls data modification in general but is not the specific cause of Custom Calculate failure. Custom Calculate does support parameters (C). The Consolidation Dimension is not a specific requirement for Custom Calculate (D).
</details>

---

### Question 6 (Hard)
**201.5.1** | Difficulty: Hard

What happens to data saved with Durable Storage during a normal Calculate?

A) It is automatically deleted as part of the calculation process
B) It is recalculated and overwritten with the new values
C) It is not cleared; it is only removed with an explicit ClearCalculated or Force
D) It is moved to the audit archive before being deleted

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Data saved with Durable Storage persists during a normal Calculate and is not cleared. It is only removed with an explicit ClearCalculated or by using the Force Calculate option. This allows certain calculated data to be maintained across multiple calculation cycles.
</details>

---

### Question 7 (Hard)
**201.5.1** | Difficulty: Hard

**Scenario:** An administrator needs to completely clear a Scenario including data, audit information, Workflow Status, and Calculation Status. What step should they execute and what precaution should they take?

A) Clear Data Step with all options selected; no special precautions needed
B) Reset Scenario Step; it is recommended to make a backup before executing
C) Execute Business Rule with a cleanup script; verify user permissions
D) Copy Data Step with an empty destination; make sure the Cube is unlocked

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Reset Scenario is the correct Step because it clears data, audit information, Workflow Status, and Calculation Status. It is more aggressive than Clear Data (which only clears data). A backup is recommended before executing because the operation is irreversible. Additionally, it requires the user to be a member of the Manage Data Group of the Scenario. Clear Data (A) does not clear Workflow Status or Calculation Status.
</details>

---

### Question 8 (Easy)
**201.5.1** | Difficulty: Easy

Where is the email notification configured upon completion or failure of a Data Management Sequence?

A) At the individual Step
B) At the Sequence
C) At the Group
D) At the Profile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The email notification is configured at the Sequence level, not at the individual Step, Group, or Profile. The Sequence allows configuring notifications on completion, on failure, or both, and requires an email server to be configured.
</details>

---

### Question 9 (Hard)
**201.5.1** | Difficulty: Hard

What are the default Queueing values in a Data Management Sequence for the Maximum % CPU Utilization and the Maximum Queued Time Before Canceling?

A) 80% CPU and 30 minutes
B) 70% CPU and 20 minutes
C) 50% CPU and 15 minutes
D) 90% CPU and 10 minutes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The default Queueing values are: Maximum % CPU Utilization of 70% and Maximum Queued Time Before Canceling of 20 minutes. These values control the server CPU utilization and the maximum time a task can be queued before being canceled.
</details>

---

### Question 10 (Medium)
**201.5.1** | Difficulty: Medium

In a Clear Data Step, what are the granular options available to select which data to clear?

A) Clear All Data, Clear Partial Data, Clear Calculated Data
B) Clear Imported Data, Clear Forms Data, Clear Adjustment Data and Delete Journals, Clear Data Cell Attachments
C) Clear Cube Data, Clear Scenario Data, Clear Entity Data
D) Clear Input Data, Clear Output Data, Clear Temporary Data, Clear Archived Data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The granular options of Clear Data are: Clear Imported Data (Origin: Import), Clear Forms Data (Origin: Forms), Clear Adjustment Data and Delete Journals (Origin: Adjustments), and Clear Data Cell Attachments. These options allow selectively clearing data based on its origin.
</details>

---

## Objective 201.5.2: Excel Add-In Functionality

### Question 11 (Easy)
**201.5.2** | Difficulty: Easy

What are the three main components of the OneStream Excel Add-In?

A) Ribbon, Toolbar, Status Bar
B) Task Pane, OneStream Ribbon, Error Logs
C) Menu Bar, Quick Access, Data Panel
D) Navigator, Formula Bar, Data Explorer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three main components of the Excel Add-In are: Task Pane (task panel with POV, Quick Views, and Documents), OneStream Ribbon (ribbon with all functionality), and Error Logs (stored locally in AppData, automatically deleted after 60 days).
</details>

---

### Question 12 (Easy)
**201.5.2** | Difficulty: Easy

What is the recommended Excel calculation mode setting when working with the OneStream Add-In for best performance?

A) Automatic
B) Automatic Except for Data Tables
C) Manual
D) Semi-Automatic

<details>
<summary>Show answer</summary>

**Correct answer: C)**

It is recommended to set the Excel calculation mode to Manual for best performance when working with the OneStream Add-In. This prevents Excel from automatically recalculating all formulas every time a change is made, which can be very slow with many XF functions.
</details>

---

### Question 13 (Medium)
**201.5.2** | Difficulty: Medium

What is the difference between Refresh Sheet and Refresh Workbook in the Excel Add-In?

A) There is no difference; both refresh the entire workbook
B) Refresh Sheet refreshes only the active sheet and clears dirty cells only on that sheet; Refresh Workbook refreshes all sheets and clears dirty cells on all of them
C) Refresh Sheet only updates formulas; Refresh Workbook updates data and formulas
D) Refresh Sheet is slower because it validates each cell; Refresh Workbook is faster with bulk updating

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Refresh Sheet refreshes only the active sheet and clears dirty cells only on the selected sheet, showing parameters from the current sheet. Refresh Workbook refreshes the entire workbook, clears dirty cells on all sheets, and shows all parameters. The same logic applies to Submit Sheet vs Submit Workbook.
</details>

---

### Question 14 (Medium)
**201.5.2** | Difficulty: Medium

Which XF function of the Excel Add-In uses Member Script (e.g., A#Sales:E#Texas) instead of individual parameters for each dimension?

A) XFGetCell
B) XFGetMemberProperty
C) XFGetCellUsingScript
D) XFGetCell5

<details>
<summary>Show answer</summary>

**Correct answer: C)**

XFGetCellUsingScript uses Member Script (e.g., A#Sales:E#Texas) instead of individual parameters for each dimension. XFGetCell and XFGetCell5 require individual parameters for each dimension (20 and 5 dimensions respectively). XFGetMemberProperty retrieves dimension member properties, not cell data.
</details>

---

### Question 15 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** A user creates a Cube View in Excel with custom formulas in some cells. After performing Submit and Refresh, the formulas disappear. What option should they have enabled to keep the formulas?

A) Preserve Excel Format
B) Retain Formulas in Cube View Content
C) Dynamically Highlight Evaluated Cells
D) Include Header

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Retain Formulas in Cube View Content allows Excel formulas in Cube View cells to be maintained after Submit and Refresh. Cells with different values are shown as "dirty cells" (yellow). Preserve Excel Format (A) maintains the visual format, not the formulas. Dynamically Highlight Evaluated Cells (C) updates cells without needing a Refresh but does not preserve formulas.
</details>

---

### Question 16 (Medium)
**201.5.2** | Difficulty: Medium

How many dimensions does the XFGetCell function support as parameters?

A) 5
B) 10
C) 15
D) 20

<details>
<summary>Show answer</summary>

**Correct answer: D)**

XFGetCell supports 20 dimensions as parameters: Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1-UD8. If only 5 UD dimensions are needed, you can use XFGetCell5 which limits UD to 5.
</details>

---

### Question 17 (Hard)
**201.5.2** | Difficulty: Hard

What is the performance limitation of Table Views in the Excel Add-In and what restriction do they have regarding data operations?

A) Maximum of 5000 KB of data and they do not perform deletes
B) Maximum of 8000 KB of data and they only update existing records, they do not perform inserts
C) Maximum of 10000 KB of data and they do not perform updates
D) Maximum of 8000 KB of data and they only perform inserts, not updates

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Table Views have a performance limit of approximately 8000 KB of data per Table View, and they only update existing records; they do not perform inserts. They are defined through Spreadsheet-type Business Rules, and the administrator controls security and write-back through Business Rules.
</details>

---

### Question 18 (Easy)
**201.5.2** | Difficulty: Easy

How is the OneStream Excel Add-In registered in Microsoft Excel?

A) It is installed as a VSTO plugin
B) It is registered as a COM Add-in via File > Options > Add-ins > COM Add-ins
C) It is automatically downloaded when opening Excel
D) It is installed from the Microsoft Store

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The OneStream Excel Add-In is registered as a COM Add-in. The ribbon is added via File > Options > Add-ins > COM Add-ins > OneStreamExcelAddIn. It is not a VSTO plugin, it is not automatically downloaded, nor is it installed from the Microsoft Store.
</details>

---

### Question 19 (Hard)
**201.5.2** | Difficulty: Hard

What are the limitations of the OneStream Spreadsheet application compared to the Excel Add-In?

A) It does not support Cube Views or Quick Views
B) It does not support Macros, Solver, Document Properties, Undo/Redo, some 3D chart types, or references to external workbooks
C) It only allows reading data, not writing
D) It does not support XF functions or In-Sheet Actions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Spreadsheet application has similar functionality to the Excel Add-In but without needing Excel installed. Its limitations include: no support for Macros, Solver, Document Properties, Undo/Redo, some 3D chart types, and no ability to reference external workbooks. It does support Cube Views, Quick Views, data writing, and XF functions.
</details>

---

### Question 20 (Medium)
**201.5.2** | Difficulty: Medium

What are In-Sheet Actions in the Excel Add-In and what operations do they allow executing?

A) Excel macros that run automatically when opening the sheet
B) Buttons on the sheet that allow executing Refresh, Submit, and Data Management Sequence without leaving the sheet
C) Special formulas that calculate data in real time
D) Links to external dashboards that open in the browser

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In-Sheet Actions are configurable buttons on the Excel sheet that allow executing Refresh, Submit, and Data Management Sequence without leaving the sheet. They are configured with label, Submit option (Sheet/Workbook/Nothing), Data Management Sequence, Parameters, Refresh, and custom colors.
</details>

---

## Objective 201.5.3: Application Properties

### Question 21 (Easy)
**201.5.3** | Difficulty: Easy

Where are Application Properties accessed in OneStream?

A) System > Tools > Application Properties
B) Application > Cube > Properties
C) Application > Tools > Application Properties
D) Application > Workflow > Properties

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Application Properties are accessed via Application > Tools > Application Properties. This is where the default properties of the application are configured, including Global POV, Company Information, Currencies, Formatting, and more.
</details>

---

### Question 22 (Easy)
**201.5.3** | Difficulty: Easy

What do the Global Scenario and Global Time properties control in Application Properties?

A) The Scenario and Time used for automatic calculations
B) The default Scenario and Time that users will see in Workflow
C) The maximum Scenario and Time allowed in the application
D) The Scenario and Time used exclusively for reports

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Global Scenario and Global Time define the default Scenario and time period that users will see in Workflow. These values are configured in the General Properties section and can be enforced for all users through the Enforce Global POV option.
</details>

---

### Question 23 (Medium)
**201.5.3** | Difficulty: Medium

What happens when the "Enforce Global POV" property is set to True?

A) Only administrators can change the Scenario and Time
B) The Global Scenario and Global Time are enforced for all users
C) All data modifications in the application are blocked
D) Cube View filters are disabled for all users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Enforce Global POV is set to True, the Global Scenario and Global Time are enforced for all users. This means no user can change these values and everyone works with the same global point of view. It does not block data modifications or disable Cube View filters.
</details>

---

### Question 24 (Medium)
**201.5.3** | Difficulty: Medium

In Application Properties, what number format should be used to display values without decimals?

A) N1
B) N0
C) #,###,0
D) 0.00

<details>
<summary>Show answer</summary>

**Correct answer: B)**

N0 is the format for displaying values without decimals. Formats N1 through N6 show 1 through 6 decimals respectively. Format #,###,0 (C) is a custom format with separators, and 0.00 (D) is not a valid OneStream format.
</details>

---

### Question 25 (Hard)
**201.5.3** | Difficulty: Hard

**Scenario:** An administrator needs to add a currency that is not in the predefined list in Application Properties. What should they do?

A) Manually create the currency in the Currencies section
B) Import the currency from an XML file
C) Contact OneStream Support to have it added
D) Add the currency in System > Configuration > Currencies

<details>
<summary>Show answer</summary>

**Correct answer: C)**

If a currency is needed that is not listed in the predefined options of Application Properties, OneStream Support must be contacted to have it added. All currencies used must be listed in Application Properties to be used on the Entity, for currency translation, or for rate entry, including pre-Euro and discontinued currencies for historical data.
</details>

---

### Question 26 (Hard)
**201.5.3** | Difficulty: Hard

What is the function of the "Lock after Certify" property in Application Properties and where is it located?

A) In General Properties > Transformation; it blocks editing Business Rules after certification
B) In Certification Properties; it automatically locks the Workflow after certification
C) In General Properties > Certification; it automatically locks after certification in Workflow
D) In Dimension Properties; it locks dimensions after certification

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Lock after Certify is located in the Certification section of General Properties. When set to True, it automatically locks the Workflow after certification is completed. This prevents additional changes to data that has already been certified.
</details>

---

### Question 27 (Easy)
**201.5.3** | Difficulty: Easy

Where does the Company Name configured in Application Properties appear?

A) On the OneStream login page
B) On reports automatically generated from Cube Views
C) In the browser window title
D) In the footer of all Dashboards

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Company Name configured in Application Properties appears on reports automatically generated from Cube Views. Along with the Logo File (PNG format, approximately 50 pixels high), these elements are displayed in Cube Views and Reports.
</details>

---

### Question 28 (Medium)
**201.5.3** | Difficulty: Medium

What does the "Allow Loads Before Workflow View Year" property control in Application Properties?

A) It allows loading data from years prior to the application creation year
B) It allows loading data to periods prior to the current Workflow year
C) It allows loading historical data from CSV files
D) It allows viewing data from prior years in Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "Allow Loads Before Workflow View Year" property, when set to True, allows loading data to periods prior to the current Workflow year. Similarly, "Allow Loads After Workflow View Year" allows loading data to periods after. Both are found in the Transformation section of General Properties.
</details>

---

### Question 29 (Hard)
**201.5.3** | Difficulty: Hard

In the custom number format of Application Properties, what does the format "#,###,0;(#,###,0);0" represent?

A) Format for thousands with decimal point; format for millions; format for units
B) Format for positive numbers with thousands separator; format for negative numbers in parentheses; format for zeros
C) Format for integers; format for decimals; format for percentages
D) Format for local currency; format for foreign currency; format for base currency

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Custom formats with three sections use the pattern: positives;negatives;zeros. The format "#,###,0;(#,###,0);0" shows positive numbers with thousands separator, negative numbers in parentheses with thousands separator, and zeros as "0". Additionally, trailing spaces can be included in the positive format to compensate for the negative parentheses and achieve vertical alignment.
</details>

---

### Question 30 (Medium)
**201.5.3** | Difficulty: Medium

What does the "UD Dimension Type for Workflow Channels" property control in Application Properties?

A) It defines which UD dimensions are displayed in Workflow reports
B) It controls an additional dimension for independent locking per channel in Workflow
C) It assigns UD dimensions to specific Cubes within the Workflow
D) It determines how many UD dimensions can be used in the application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "UD Dimension Type for Workflow Channels" property controls an additional dimension for independent locking per channel. This allows, for example, planning to be done by Entity and by Product independently, enabling Workflow Channels with different locking levels.
</details>

---

## Objective 201.5.4: Creating a Scheduled Task

### Question 31 (Easy)
**201.5.4** | Difficulty: Easy

What type of object is scheduled in the OneStream Task Scheduler?

A) Individual Data Management Steps
B) Data Management Sequences
C) Business Rules
D) Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Task Scheduler schedules Data Management Sequences, not individual Steps, Business Rules, or Cube Views. Steps are organized within Sequences, and it is the Sequences that are selected when creating a scheduled task.
</details>

---

### Question 32 (Easy)
**201.5.4** | Difficulty: Easy

Where is the Task Scheduler accessed in OneStream?

A) System > Tools > Task Scheduler
B) Application > Workflow > Task Scheduler
C) Application > Tools > Task Scheduler
D) System > Administration > Task Scheduler

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Task Scheduler is accessed via Application > Tools > Task Scheduler. It is an application-level tool, not a system tool. To access it, the user needs the Application User Interface Role "TaskSchedulerPage".
</details>

---

### Question 33 (Medium)
**201.5.4** | Difficulty: Medium

What is the maximum number of retries that can be configured in the Advanced tab of a Scheduled Task if it fails?

A) 1
B) 2
C) 3
D) 5

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The maximum number of configurable retries in the Advanced tab is 3. If the task fails, it will retry up to the specified number of times before being definitively marked as failed.
</details>

---

### Question 34 (Medium)
**201.5.4** | Difficulty: Medium

**Scenario:** A user with the "TaskScheduler" role tries to perform a Load/Extract of scheduled tasks but cannot. Why?

A) The Task Scheduler does not support Load/Extract
B) The TaskScheduler role does not allow Load/Extract; the ManageTaskScheduler role is needed
C) The user needs the additional ApplicationLoadExtractPage role
D) Only the Administrator can perform Load/Extract of any type

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The TaskScheduler role allows creating tasks and editing own tasks, viewing all tasks, but cannot perform Load/Extract. To perform Load/Extract of scheduled tasks, the ManageTaskScheduler role is needed, which also allows creating, viewing, editing, and deleting all tasks.
</details>

---

### Question 35 (Hard)
**201.5.4** | Difficulty: Hard

In what time zone are scheduled tasks stored in OneStream and how are they presented to the user?

A) They are stored and presented in the server time zone
B) They are stored in UTC and presented in the user's local time zone
C) They are stored in the user's local time zone and presented in UTC
D) They are stored and presented in the time zone configured in Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Scheduled tasks are stored internally in UTC but are presented to the user in their local time zone. When the user configures the start date and time, they do so in their local time, but the system converts and saves it in UTC to ensure consistency regardless of the user's location.
</details>

---

### Question 36 (Easy)
**201.5.4** | Difficulty: Easy

What are the available scheduling frequencies in the Task Scheduler?

A) One Time, Hourly, Daily, Weekly
B) One Time, Minutes, Daily, Weekly, Monthly
C) Continuous, Hourly, Daily, Weekly, Monthly, Yearly
D) One Time, Daily, Weekly, Monthly, Quarterly

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available frequencies are: One Time (single execution), Minutes (recurring every 5-180 minutes, with option to limit with Time From/To), Daily, Weekly (select days and frequency), and Monthly (select days and frequency). There is no Hourly, Continuous, Quarterly, or Yearly option.
</details>

---

### Question 37 (Medium)
**201.5.4** | Difficulty: Medium

What is the difference between the TaskScheduler and ManageTaskScheduler roles?

A) TaskScheduler can create and edit all tasks; ManageTaskScheduler can only view them
B) TaskScheduler can create and edit own tasks; ManageTaskScheduler can manage all tasks and perform Load/Extract
C) Both roles are identical in functionality
D) TaskScheduler is a System Role; ManageTaskScheduler is an Application Role

<details>
<summary>Show answer</summary>

**Correct answer: B)**

TaskScheduler allows creating tasks, editing own tasks, and viewing all tasks, but cannot perform Load/Extract or change the task name. ManageTaskScheduler allows creating, viewing, editing, and deleting all tasks, and can perform Load/Extract, but also cannot change the task name. Both are Application Security Roles.
</details>

---

### Question 38 (Hard)
**201.5.4** | Difficulty: Hard

**Scenario:** A scheduled task that had been working correctly now consistently fails. The administrator verifies that the Sequence exists and has Steps configured. What else should they check as a possible cause?

A) That the server time zone has not changed
B) That the Task Scheduler Validation Frequency has not expired the task
C) That the Sequence has at least one Step configured and that the user's permissions (Manage Data Group of the Scenario) are still valid
D) That the associated Cube View has not been deleted

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Although the Sequence exists, you must verify that it has at least one Step (if the Sequence has no Step, the job will fail) and that the user's permissions are still valid, including membership in the Manage Data Group of the Scenario for Steps like Custom Calculate or Reset Scenario. You should also check the Expire Date/Time and the Enabled state of the task.
</details>

---

### Question 39 (Hard)
**201.5.4** | Difficulty: Hard

How does a scheduled task appear in Task Activity and Logon Activity respectively?

A) Task Type: "Scheduled Task" / Client Module: "Task Scheduler"
B) Task Type: "Data Management Scheduled Task" / Client Module: "Scheduler"
C) Task Type: "Auto Task" / Client Module: "Background Service"
D) Task Type: "Data Management" / Client Module: "Automated"

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In Task Activity, the Task Type is shown as "Data Management Scheduled Task". In Logon Activity, the Client Module appears as "Scheduler". These specific identifiers allow distinguishing scheduled tasks from manually executed ones.
</details>

---

### Question 40 (Medium)
**201.5.4** | Difficulty: Medium

What checkbox can only be modified by an Administrator in the configuration of a Scheduled Task?

A) Enabled
B) Administration Enabled (Enabled by Manager)
C) Auto Retry
D) Send Notification

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Administration Enabled (also known as "Enabled by Manager") is a checkbox that only the Administrator can change. The regular Enabled checkbox can be changed by any user with Task Scheduler permissions, but Administration Enabled requires administrator privileges.
</details>

---

## Objective 201.5.5: Correct Use of Load/Extract

### Question 41 (Easy)
**201.5.5** | Difficulty: Easy

In what format are artifacts imported and exported via Load/Extract?

A) JSON
B) CSV
C) XML
D) SQL

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Load/Extract uses XML format to import and export application or system artifacts. The exception is the Application Zip File which uses ZIP format, and FX Rate CSV which uses CSV format. But the standard format for artifact definitions is XML.
</details>

---

### Question 42 (Easy)
**201.5.2** | Difficulty: Easy

What is the main typical use of Load/Extract in OneStream?

A) Creating backups of transactional data
B) Moving configurations between environments (Development > Test > Production)
C) Exporting data for external reports
D) Synchronizing data between applications in real time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The main typical use of Load/Extract is to move configurations between environments, following the flow Development > Test > Production. It is also used for configuration backups and artifact migration, but it is not a real-time replication tool nor is it designed for exporting transactional data for reports.
</details>

---

### Question 43 (Medium)
**201.5.5** | Difficulty: Medium

What does an Application Zip File include when extracted?

A) Everything including data and FX Rates
B) Everything except data (Data) and FX Rates
C) Only metadata and Business Rules
D) Only Dashboards and Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An Application Zip File includes everything except Data and FX Rates. It creates a complete copy of the application with metadata, Business Rules, Cube Views, Data Management, Workflow Profiles, Dashboards, etc., but does not include stored data or exchange rates.
</details>

---

### Question 44 (Medium)
**201.5.5** | Difficulty: Medium

**Scenario:** An administrator needs to export the application's exchange rates in a format that can be easily opened in Excel. What file type should they select in Application Load/Extract?

A) FX Rates
B) FX Rate CSV
C) Application Zip File
D) Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

FX Rate CSV exports exchange rates in CSV format with the columns FxRateType, Time, SourceCurrency, DestCurrency, Amount, and HasData, which can be easily opened in Excel. FX Rates (A) exports in XML format by FX Rate Type and Time Period, which is less accessible for Excel.
</details>

---

### Question 45 (Hard)
**201.5.5** | Difficulty: Hard

What happens when loading a Workflow Profiles XML that does not contain all existing properties in the destination environment?

A) Missing properties are kept unchanged
B) Old properties not in the XML are automatically cleaned
C) The load fails with a validation error
D) An automatic backup is created before the load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When loading a Workflow Profiles XML, old properties that are not included in the XML file are automatically cleaned. This means the loaded XML completely replaces the existing Workflow Profiles configuration, so it is important to ensure the XML contains all desired configuration.
</details>

---

### Question 46 (Easy)
**201.5.5** | Difficulty: Easy

Who has default access to the Load/Extract functionality?

A) All users (Everyone)
B) Only Administrators
C) Users with the ModifyData role
D) Users with the ManageData role

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Only Administrators have default access to the Load/Extract functionality. For other users to access it, they must be assigned the corresponding user interface roles (ApplicationLoadExtractPage or SystemLoadExtractPage) along with the necessary security roles.
</details>

---

### Question 47 (Hard)
**201.5.5** | Difficulty: Hard

What are the three operations available in Load/Extract and what does each do?

A) Import (load XML), Export (export XML), Backup (create backup copy)
B) Load (import XML), Extract (export XML), Extract and Edit (extract and edit the XML immediately)
C) Upload (upload file), Download (download file), Sync (synchronize)
D) Load (import XML), Extract (export XML), Compare (compare versions)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three operations are: Load (import an XML file with artifact definitions), Extract (export artifacts to an XML file), and Extract and Edit (extract and edit the XML immediately). The latter is useful for making quick modifications to the XML before loading it into another environment.
</details>

---

### Question 48 (Medium)
**201.5.5** | Difficulty: Medium

What artifacts can be extracted in the "Metadata" category of Application Load/Extract?

A) Only dimensions and their members
B) Business Rules, Time Dimension Profiles, Dimensions, and Cubes
C) Only Cubes and their configurations
D) Dimensions, Cubes, Scenarios, and Entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Metadata category includes Business Rules, Time Dimension Profiles, Dimensions, and Cubes. It covers both dimension definitions and business rules and time profiles associated with the application metadata.
</details>

---

### Question 49 (Hard)
**201.5.5** | Difficulty: Hard

An administrator needs to export only the Extensibility Rules of an application. What should they select and why?

A) Metadata, because Extensibility Rules are part of the metadata
B) Extensibility Rules, because other types of Business Rules are exported with their associated object
C) Application Zip File, because it is the only way to include Business Rules
D) Execute Business Rule, because Extensibility Rules are a type of Step

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You should select "Extensibility Rules" in Application Load/Extract. This extraction type only includes Extensibility Rules because other types of Business Rules (such as Finance, Spreadsheet, etc.) are exported together with their associated object (Data Management, Cube Views, etc.). This separation allows for specific export of extensibility rules.
</details>

---

### Question 50 (Medium)
**201.5.5** | Difficulty: Medium

What is the recommended best practice before deploying changes to a production environment using Load/Extract?

A) Perform Load/Extract directly from development to production during low-activity hours
B) Extract changes from the development environment, evaluate in a test environment, then apply in production
C) Create a copy of the XML file and save it as a backup
D) Deactivate all users before loading changes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to always deploy and test changes first in a development environment, then extract those changes and evaluate them in a test environment before applying them in production. It is also recommended to consider making a copy of the production database to test changes and avoid deploying during periods of high load or activity.
</details>

---

## Objective 201.5.6: Difference Between Application and System Load/Extract

### Question 51 (Easy)
**201.5.6** | Difficulty: Easy

Where is Application Load/Extract located and where is System Load/Extract located?

A) Both in System > Tools > Load/Extract
B) Application Load/Extract in Application > Tools > Load/Extract; System Load/Extract in System > Tools > Load/Extract
C) Both in Application > Tools > Load/Extract
D) Application Load/Extract in Application > Administration; System Load/Extract in System > Administration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application Load/Extract is located in Application > Tools > Load/Extract and works with artifacts specific to one application. System Load/Extract is located in System > Tools > Load/Extract and works with framework/system artifacts shared across all applications.
</details>

---

### Question 52 (Easy)
**201.5.6** | Difficulty: Easy

What type of artifacts are managed with System Load/Extract?

A) Metadata, Business Rules, Cube Views, and Data Management
B) Security (Users, Groups, Roles), System Dashboards, and Logs
C) FX Rates, Workflow Profiles, and Transformation Rules
D) Cubes, Scenarios, and Entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

System Load/Extract manages framework/system artifacts: Security (System Roles, Users, Security Groups, Exclusion Groups), System Dashboards (Maintenance Units and Profiles), and Logs (Error Log, Task Activity, Logon Activity with Start/End Time ranges). The other mentioned artifacts belong to Application Load/Extract.
</details>

---

### Question 53 (Medium)
**201.5.6** | Difficulty: Medium

**Scenario:** An administrator needs to move user and security group configuration to a new environment. What tool should they use and what precaution should they take with Unique IDs?

A) Application Load/Extract; keep Unique IDs checked
B) System Load/Extract; uncheck "Extract Unique IDs" to avoid duplicate ID conflicts in the new environment
C) System Load/Extract; keep Unique IDs checked to preserve integrity
D) Application Load/Extract; uncheck "Extract Unique IDs"

<details>
<summary>Show answer</summary>

**Correct answer: B)**

System Load/Extract should be used because users and groups are system/framework artifacts. When moving to another environment, "Extract Unique IDs" should be unchecked to avoid errors with duplicate IDs, since the new environment may have different IDs assigned to other objects.
</details>

---

### Question 54 (Medium)
**201.5.6** | Difficulty: Medium

What Application User Interface Role is needed to access Application Load/Extract and which for System Load/Extract?

A) ApplicationToolsPage and SystemToolsPage
B) ApplicationLoadExtractPage and SystemLoadExtractPage
C) ManageApplication and ManageSystem
D) ApplicationAdminPage and SystemAdminPage

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Application Load/Extract, the ApplicationLoadExtractPage role (Application User Interface Role) plus Administrator role or equivalent is needed. For System Load/Extract, the SystemLoadExtractPage role (System User Interface Role) is needed, and for Security you also need ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles.
</details>

---

### Question 55 (Hard)
**201.5.6** | Difficulty: Hard

On which database does each type of Load/Extract operate?

A) Both operate on the Application Database
B) Application Load/Extract operates on the Application Database; System Load/Extract operates on the Framework (System) Database
C) Both operate on the Framework Database
D) Application Load/Extract operates on the Framework Database; System Load/Extract operates on the Application Database

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application Load/Extract operates on the Application Database, which contains artifacts specific to an individual application. System Load/Extract operates on the Framework (System) Database, which contains shared system artifacts such as users, groups, roles, and configurations that apply to all applications.
</details>

---

### Question 56 (Easy)
**201.5.6** | Difficulty: Easy

What is the scope of each type of Load/Extract?

A) Application: the entire system; System: a specific application
B) Application: a specific application; System: the entire environment/system
C) Both: the entire complete system
D) Both: only the active application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application Load/Extract has scope over a specific application (its metadata, Rules, Views, Dashboards, etc.). System Load/Extract has scope over the entire environment/system (Security, System Dashboards, Logs that are shared across all applications).
</details>

---

### Question 57 (Hard)
**201.5.6** | Difficulty: Hard

**Scenario:** A non-Administrator user needs to perform Load/Extract of system Security. What specific roles do they need to have assigned?

A) Only SystemLoadExtractPage
B) SystemLoadExtractPage, ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles
C) AdministerApplication and SystemLoadExtractPage
D) ManageSystemSecurityUsers and SystemPane

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For a non-Administrator user to perform Load/Extract of system Security, they need four roles: SystemLoadExtractPage (UI Role to access the page), ManageSystemSecurityUsers (to manage users), ManageSystemSecurityGroups (to manage groups), and ManageSystemSecurityRoles (to manage roles). Without the three Manage System Security roles, they cannot complete the operation.
</details>

---

### Question 58 (Medium)
**201.5.6** | Difficulty: Medium

What artifacts can be extracted as part of System Load/Extract for Logs?

A) Only Error Log
B) Error Log, Task Activity, and Logon Activity, all with Start/End Time range
C) Error Log and Task Activity without time filters
D) All system logs without time limit

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In System Load/Extract, three types of logs can be extracted: Error Log, Task Activity, and Logon Activity. All three require a Start/End Time range to filter the records to export. This is useful for auditing and system activity analysis.
</details>

---

### Question 59 (Hard)
**201.5.6** | Difficulty: Hard

What special consideration must be taken when performing Security XML loads by non-Administrator users?

A) The XML file must be digitally signed
B) The security must pre-exist in the destination environment to validate the changes
C) Only users can be loaded, not groups or roles
D) A server restart is required after the load

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For Security XML loads performed by non-Administrator users, the security must pre-exist in the destination environment so the system can validate the changes. This is a security measure that ensures a non-Administrator user cannot create security configurations that do not previously exist, limiting their modification capability to already established elements.
</details>

---

### Question 60 (Medium)
**201.5.6** | Difficulty: Medium

What is the file format used by Application Load/Extract to export a complete application compared to individual artifacts?

A) Both use XML format
B) The complete application uses ZIP format; individual artifacts use XML format
C) The complete application uses JSON format; individual artifacts use XML
D) Both use ZIP format

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Application Zip File (complete application) is exported in ZIP format and contains everything except data and FX Rates. Individual artifacts (Metadata, Cube Views, Data Management, etc.) are exported in XML format. System Load/Extract always uses XML format.
</details>

---

## Additional Questions: Tools Section Review

### Question 61 (Easy)
**201.5.1** | Difficulty: Easy

How many types of Business Rules exist in OneStream, and where are they found?

A) 6 types, under System > Tools > Business Rules
B) 9 types, under Application > Tools > Business Rules
C) 12 types, under Application > Administration > Business Rules
D) 9 types, under System > Administration > Business Rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

There are nine types of Business Rules found under Application > Tools > Business Rules. The nine types are: Finance, Parser, Connector, Conditional, Derivative, Cube View Extender, Dashboard Dataset, Dashboard Extender, and Dashboard XFBRString. Additionally, Extensibility Rules (Extender and Event Handlers) exist as a separate category requiring Full Administrator rights to edit.
</details>

---

### Question 62 (Easy)
**201.5.3** | Difficulty: Easy

What level of access is required to edit Extensibility Rules in OneStream?

A) Any user with the ModifyData role
B) Members of the Business Rules Maintenance security group
C) Full Administrator rights
D) Users with the EncryptBusinessRule security role

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Full Administrator rights are required to edit Extensibility Rules. This is explicitly stated in the documentation as an important requirement, distinguishing Extensibility Rules from other Business Rule types that can be maintained by users assigned to the rule's Maintenance security group.
</details>

---

### Question 63 (Easy)
**201.5.6** | Difficulty: Easy

What are the default and maximum values for the Profiler's "Number of Minutes to Run" and "Number of Hours to Retain Before Deletion" settings?

A) Default 10 minutes / max 30 minutes; default 12 hours / max 72 hours
B) Default 20 minutes / max 60 minutes; default 24 hours / max 168 hours
C) Default 30 minutes / max 120 minutes; default 48 hours / max 336 hours
D) Default 15 minutes / max 45 minutes; default 24 hours / max 120 hours

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Profiler defaults to 20 minutes to run with a maximum of 60 minutes, and 24 hours retention before deletion with a maximum of 168 hours. Values entered over the maximum are automatically reset to the default value. The Profiler session is deleted on the first server restart after the deletion time has passed.
</details>

---

### Question 64 (Medium)
**201.5.2** | Difficulty: Medium

What are the available Spreading types in the OneStream Excel Add-In?

A) Fill, Clear Data, Factor, Even Distribution, Proportional Distribution
B) Fill, Clear Data, Factor, Accumulate, Even Distribution, 445, 454, 544, Proportional Distribution
C) Fill, Factor, Accumulate, Even Distribution, Weighted Distribution
D) Fill, Clear Data, Factor, Even Distribution, 445, 544, Custom Distribution

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The nine Spreading types are: Fill (fills each cell with the specified amount), Clear Data (clears all data), Factor (multiplies by a rate), Accumulate (compounds each cell by the rate from the previous), Even Distribution (distributes evenly), 445 Distribution (weights 4-4-5), 454 Distribution (weights 4-5-4), 544 Distribution (weights 5-4-4), and Proportional Distribution (distributes proportionally based on existing values).
</details>

---

### Question 65 (Medium)
**201.5.2** | Difficulty: Medium

Why do Quick Views sometimes display more columns than expected when multiple dimensions are placed in the column area?

A) Quick Views have a bug that duplicates columns
B) Quick Views multiply members across dimensions because they have a single column set, producing a Cartesian product
C) Quick Views automatically add calculated variance columns
D) Quick Views expand hidden members by default

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Quick Views have a single row set and a single column set. When multiple dimensions with multiple members are placed in columns, they produce a Cartesian product (multiplication). For example, 2 Scenarios and 2 UD8 members result in 4 columns instead of the desired 3. This is known as the asymmetry issue. Cube Views solve this by allowing independent columns and rows, each with their own Member Filters.
</details>

---

### Question 66 (Medium)
**201.5.2** | Difficulty: Medium

What happens when you use the "Convert to XFGetCells" function on an existing Quick View?

A) The Quick View is preserved alongside the new XFGetCell formulas
B) The Quick View definition is deleted and converted to XFGetCell formulas; this conversion cannot be undone
C) The Quick View is converted to a Cube View with XFGetCell references
D) The Quick View is temporarily hidden and can be restored at any time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When you click Convert to XFGetCells, OneStream prompts with a confirmation message. By clicking OK, the Quick View definition is deleted and converted to XFGetCell formulas. This is a one-way conversion: there is no way to convert XFGetCells back into a Quick View. The formatting from the original Quick View is retained on the converted cells.
</details>

---

### Question 67 (Medium)
**201.5.6** | Difficulty: Medium

What are the supported file size limits for the OneStream File Explorer in the Windows Application?

A) Uploads: up to 100 MB; Downloads: up to 500 MB
B) Uploads (Applications and System): up to 300 MB; Uploads (Content): up to 2 GB; Downloads: up to 2 GB
C) Uploads: up to 1 GB; Downloads: up to 1 GB
D) Uploads (Applications and System): up to 500 MB; Uploads (Content): up to 5 GB; Downloads: up to 5 GB

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In the Windows Application, the supported file sizes are: Uploads to Applications and System folders up to 300 MB, Uploads to Content folders up to 2 GB, and Downloads from both Application/System and Content folders up to 2 GB. The Contents folders are auto-generated within both the Application and System folders and are intended to store files larger than 300 MB.
</details>

---

### Question 68 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** An administrator formats a Cube View in Excel using native Excel formatting (font colors, borders, etc.) but the formatting disappears after Refresh. What is the correct approach to make formatting persist on Cube Views, and what is the order of precedence for formatting styles?

A) Use the "Lock Formatting" button in the OneStream ribbon; precedence is Styles > Conditional > Selection Styles
B) Use Selection Styles or Preserve Excel Format; the order of precedence from lowest to highest is: Default CV Style < Custom CV Format < Conditional Formatting < Selection Styles < Styles
C) Use only native Excel conditional formatting, which is the only formatting that persists; there is no precedence order
D) Enable "Preserve Excel Format" in the Cube View Connection settings; precedence does not apply when this is enabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Native Excel formatting alone will not persist after a Refresh on Cube Views. The correct approaches are: enable Preserve Excel Format in the Cube View Connection, use Selection Styles (which associate Excel formatting with the Cube View), or use Styles (based on named ranges). The exception is native conditional formatting, which works and persists without any special setup. The order of precedence from lowest to highest priority is: Default Cube View Style < Custom Cube View Format < Conditional Formatting < Selection Styles < Styles.
</details>

---

### Question 69 (Hard)
**201.5.1** | Difficulty: Hard

**Scenario:** A developer wants to encrypt a Business Rule to protect its logic. What security role is required, what happens after encryption, and what should they be careful about?

A) The AdministerApplication role is required; the rule becomes hidden from all users; the password is stored in the database and can be recovered
B) The EncryptBusinessRule security role is required; the editor displays "Business Rule Is Encrypted" in read-only mode; if the password is forgotten, OneStream Support must be contacted to decrypt it
C) No special role is required; any user with Maintenance access can encrypt; the rule can be decrypted by any Administrator
D) The DatabaseAdmin role is required; the encrypted rule cannot be viewed or executed; the password can be reset from Application Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To encrypt a Business Rule, the user must be assigned the EncryptBusinessRule security role. Once encrypted, the editor displays "Business Rule Is Encrypted" message text and enters read-only mode. The Decrypt button appears in the menu bar. It is critically important to remember and record the password because if it is forgotten, the Business Rule cannot be decrypted without the assistance of OneStream Support.
</details>

---

### Question 70 (Hard)
**201.5.6** | Difficulty: Hard

**Scenario:** An IT administrator needs to check the overall health of the OneStream environment, including web servers, application servers, and database servers. What tool should they use, and what is a key access limitation?

A) System > Tools > Database; accessible from any browser
B) System > Tools > Environment; it is only accessible via the OneStream Windows App
C) System > Logging > Task Activity; accessible from any client
D) Application > Tools > System Health; it requires the SystemMonitor security role

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Environment page (System > Tools > Environment) is used to check the overall health of the environment, which contains Web Servers, Mobile Web Servers, Application Server Sets, and Database Servers. It checks connection status and configuration, and allows users to monitor the environment, isolate bottlenecks, view properties and configuration changes, and scale application servers. A key limitation is that the Environment page is only accessible via the OneStream Windows App.
</details>

---

### Question 71 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** A finance team member creates reports in both the Excel Add-In and the OneStream Spreadsheet application. They want to know where Spreadsheet files can be saved and whether files are portable between the two tools. What is correct?

A) Spreadsheet files can only be saved to the OneStream File System; files are not portable between Excel and Spreadsheet
B) Spreadsheet files can be saved to Local Folder, OneStream File System, Application Workspace, or System Workspace; files are portable between Excel and Spreadsheet
C) Spreadsheet files can only be saved to Local Folder or OneStream File System; files are portable only from Spreadsheet to Excel, not the reverse
D) Spreadsheet files can be saved to any location; files are portable but lose all formatting when transferred

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Spreadsheet feature offers four save locations: Local Folder, OneStream File System, Application Workspace File, and System Dashboard File (System Workspace). Files are fully portable between Excel and Spreadsheet. A file authored in Spreadsheet can be saved locally and opened with the Excel Add-In, and vice versa. This portability works without issues, though the administrator may prefer to design in one tool and distribute via the other.
</details>

---

### Question 72 (Hard)
**201.5.2** | Difficulty: Hard

**Scenario:** A OneStream developer needs to retrieve a single data intersection using an XF function. They are choosing between XFGetCell, XFGetCell5, and XFGetCellUsingScript. Which statement accurately describes the differences between these three functions?

A) XFGetCell supports 20 dimension parameters; XFGetCell5 supports 5 total dimensions; XFGetCellUsingScript uses named ranges instead of dimension parameters
B) XFGetCell supports all 20 dimension parameters individually; XFGetCell5 limits User Defined Dimensions to 5 instead of 8; XFGetCellUsingScript uses Member Script syntax (e.g., E#Houston:A#Sales) instead of individual parameters
C) XFGetCell and XFGetCell5 are identical except for performance; XFGetCellUsingScript requires a Business Rule to execute
D) XFGetCell retrieves data from one Cube; XFGetCell5 retrieves from up to 5 Cubes simultaneously; XFGetCellUsingScript retrieves from any data source including external databases

<details>
<summary>Show answer</summary>

**Correct answer: B)**

XFGetCell provides a separate parameter for each of the 20 dimensions (Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1-UD8), requiring every dimension to be defined. XFGetCell5 has the same functionality but limits the User Defined Dimensions to 5 instead of 8, reducing the number of parameters. XFGetCellUsingScript uses Member Script syntax (e.g., E#Houston:A#Sales:S#Actual) to specify the intersection in a concatenated string format, rather than requiring individual parameters for each dimension.
</details>

---

---


# Question Bank - Section 6: Security (10% of exam)

## Objective 201.6.1: Data Cell Access

### Question 1 (Easy)
**201.6.1** | Difficulty: Easy

What are the four application security layers in OneStream?

A) Authentication, Authorization, Encryption, Auditing
B) Workflow Security, Entity Security, Account Security, Security Roles
C) User Security, Group Security, Role Security, Data Security
D) Login Security, Application Security, Cube Security, Cell Security

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four application security layers are: Workflow Security (controls who executes processes), Entity Security (controls read/write access to entity data), Account Security (controls visibility of dimension members), and Security Roles (determines what data can be accessed or edited). These layers work together to provide granular security.
</details>

---

### Question 2 (Easy)
**201.6.1** | Difficulty: Easy

What is the correct order of the security verification flow when a user attempts to access data?

A) Entity > Cube > Scenario > Data Cell Access > Workflow
B) OpenApplication > Cube Access > Scenario Access > Entity Access > Data Cell Access > Workflow Verification
C) Workflow > Entity > Cube > Scenario > Data Cell Access
D) OpenApplication > Scenario > Entity > Cube > Workflow > Data Cell Access

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct flow is: 1) OpenApplication, 2) Cube Access, 3) Scenario Access, 4) Entity Access, 5) Data Cell Access Security (Slice Security), 6) Workflow Verification. If any of these steps results in "no access," the process stops immediately.
</details>

---

### Question 3 (Medium)
**201.6.1** | Difficulty: Medium

What happens if a user has the ViewAllData role with respect to Data Cell Access Security?

A) Data Cell Access Security restricts them normally
B) Data Cell Access Security does not restrict them; they have read access to the entire Cube without exception
C) They can only see data for Entities to which they have access
D) They can see all data but cannot export it

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If a user has the ViewAllData role, Data Cell Access Security does not restrict them. This role grants read access to the entire Cube without exception. To restrict data for a user with ViewAllData, you would have to build out all entity access individually and then decrease it, which makes this role very powerful and one that should be assigned with caution.
</details>

---

### Question 4 (Medium)
**201.6.1** | Difficulty: Medium

In Data Cell Access Security, what is the fundamental principle regarding increasing and decreasing access?

A) It can increase access that was not initially granted
B) First access is granted, then decreased to a subset, and optionally access can be increased to specific intersections
C) It can only decrease access, never increase
D) Access is determined solely by the first matching rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The fundamental principle of Data Cell Access is: first access is granted (through Users and Groups), then decreased to a subset, and optionally access can be increased to specific intersections. Important: it cannot increase access that was not initially granted. Security is applied in sequential order.
</details>

---

### Question 5 (Hard)
**201.6.1** | Difficulty: Hard

**Scenario**: An administrator configures Data Cell Access rules in a Cube. The first rule decreases access to "No Access" for certain accounts. The second rule attempts to increase access for a subset of those accounts with the behavior "Increase Access And Stop". What happens when the second rule is evaluated?

A) The second rule has no effect because the first already denied access
B) If the cell matches the filter of the second rule, access is increased and all subsequent rules are ignored
C) Both rules are applied simultaneously and the most restrictive one wins
D) The second rule completely overwrites the first rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The behavior "Increase Access And Stop" means that if the cell matches the filter of that rule, access is increased and all subsequent rules are ignored. The order of rules is critical in Data Cell Access as they are evaluated sequentially. This behavior allows creating specific exceptions within general restrictions.
</details>

---

### Question 6 (Medium)
**201.6.1** | Difficulty: Medium

What is the difference between Data Cell Access Security and Data Cell Conditional Input?

A) They are the same thing with different names
B) Data Cell Access is group-based security; Data Cell Conditional Input impacts ALL users and is not security per se
C) Data Cell Access is for reading; Conditional Input is for writing
D) Data Cell Access is configured in the Cube; Conditional Input is configured in the Scenario

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Cell Access Security is a group-based security layer that controls granular access to data intersections. Data Cell Conditional Input is not security per se but rather a functional restriction that impacts ALL users without group distinction. It is used to make cells read-only at specific intersections, such as restricting Trial Balance data from being loaded through the Forms origin.
</details>

---

### Question 7 (Hard)
**201.6.1** | Difficulty: Hard

What Cube Properties setting controls access to relationship members in the Consolidation Dimension, and what is its behavior when set to True?

A) "Use Entity Security for Consolidation" - True: Consolidation members inherit security from the Entity
B) "Use Parent Security for Relationship Consolidation Dimension Members" - True: Rights to relationship members are determined by rights to the entity's immediate parent
C) "Enable Consolidation Security" - True: Additional security checks are activated
D) "Restrict Consolidation Access" - True: Only administrators can see consolidation members

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The setting is "Use Parent Security for Relationship Consolidation Dimension Members". When set to False (default), the user's rights to the entity control access to all consolidation dimension members. When set to True, rights to relationship members are determined by rights to the entity's immediate parent. If a user does not have access to the parent, they lose access to relationship members and can only see Local and Translated. ViewAllData users and Administrators are not affected.
</details>

---

### Question 8 (Easy)
**201.6.1** | Difficulty: Easy

What two main security groups does each Entity have?

A) Admin Group and User Group
B) Read Data Group and Read/Write Data Group
C) View Group and Edit Group
D) Access Group and Maintenance Group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Each Entity has two main security groups: Read Data Group (read-only access to data) and Read/Write Data Group (read and write access). OneStream also supports a second Read Data Group and second Read/Write Data Group. The recommended naming convention is XXXX_View (read) and XXXX_Mod (write).
</details>

---

### Question 9 (Hard)
**201.6.1** | Difficulty: Hard

**Scenario**: An administrator configures a Display Member Group for an Account dimension member and sets Display Access to "Nobody". An advanced user attempts to access the data for that account using XFGetCell in the Excel Add-In. What happens?

A) The user cannot access the data because Display Access is set to "Nobody"
B) The user can access the data because Display Member Group controls visibility in lists, not data access
C) The system returns a security error
D) The Excel Add-In blocks the request before sending it to the server

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Display Member Group controls the visibility of dimension members in lists, not data access. Setting Display Access to "Nobody" hides the member from Member Filters, but does not restrict access to the data. A user may not see the member in a list but could access the data via XFGetCell or freeform entry. It should not be confused with data security.
</details>

---

### Question 10 (Medium)
**201.6.1** | Difficulty: Medium

Where is Data Cell Access Security configured in OneStream?

A) Application > Tools > Security Roles
B) Application > Cube > Cubes > Data Access > Data Cell Conditional Input
C) System > Security > Data Cell Access
D) Application > Workflow > Data Access

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Data Cell Access Security is configured in Application > Cube > Cubes > Data Access > Data Cell Conditional Input. Each rule has: Member Filter (dimension intersection to restrict), In Filter (behavior), Access Level (Read Only, No Access, etc.) and Behavior (Decrease Access, Increase Access, Increase Access And Stop, etc.).
</details>

---

## Objective 201.6.2: Security Roles (Application, UI, System)

### Question 11 (Easy)
**201.6.2** | Difficulty: Easy

What is the fundamental difference between Application Security Roles and Application User Interface Roles?

A) There is no difference; they are the same
B) Application Security Roles control management/action on objects; Application User Interface Roles control page access
C) Application Security Roles are for administrators; Application User Interface Roles are for end users
D) Application Security Roles are configured in System; Application User Interface Roles in Application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application Security Roles control who can manage or execute actions on objects and data within the application (e.g., ManageMetadata, ModifyData). Application User Interface Roles control access to pages within the application (e.g., CubeViewsPage, BusinessRulesPage). Both role types work together: the UI Role provides visibility to the page and the Security Role provides management capability.
</details>

---

### Question 12 (Easy)
**201.6.2** | Difficulty: Easy

When a new application is created in OneStream, which two roles do NOT default to "Administrator"?

A) ModifyData (Nobody) and ViewAllData (Nobody)
B) OpenApplication (Everyone) and AdministratorDatabase (Nobody)
C) ManageData (Nobody) and ManageMetadata (Nobody)
D) OpenApplication (Everyone) and ModifyData (Everyone)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a new application is created, all role defaults are Administrator except OpenApplication (assigned to Everyone so that all users can open the application) and AdministratorDatabase (assigned to Nobody because bulk deletion of metadata and data is a dangerous operation that Administrators do not have automatically).
</details>

---

### Question 13 (Medium)
**201.6.2** | Difficulty: Medium

Which role allows a user to modify data in an application and can be left assigned to "Everyone" if the rest of the security is properly configured?

A) ManageData
B) ViewAllData
C) ModifyData
D) AdministerApplication

<details>
<summary>Show answer</summary>

**Correct answer: C)**

ModifyData allows modifying data in the application. Without this role, the user is read-only. Its default is Everyone and it can be left at Everyone if the rest of the security (Entity Security, Scenario Security, Workflow, etc.) is properly configured, since those additional layers will control who can actually write data.
</details>

---

### Question 14 (Medium)
**201.6.2** | Difficulty: Medium

**Scenario**: A user has the Application User Interface Role "CubeViewsPage" assigned but does not have the Application Security Role "ManageCubeViews". What can the user do?

A) Can view and create Cube Views without restriction
B) Can access the Cube Views page but cannot create or manage Cube Views
C) Cannot see the Cube Views page at all
D) Can create Cube Views but cannot modify existing ones

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The user can access the Cube Views page (thanks to the UI Role CubeViewsPage) but cannot create or manage Cube Views (because they lack the Security Role ManageCubeViews). This is a key example of how both role types work together: the UI Role provides visibility and the Security Role provides management capability.
</details>

---

### Question 15 (Hard)
**201.6.2** | Difficulty: Hard

What is the correct hierarchy of access from greatest to least power in OneStream?

A) Access Group > Maintenance Group > System Security Role
B) System Security Role > Maintenance Group > Access Group
C) Maintenance Group > System Security Role > Access Group
D) System Security Role > Access Group > Maintenance Group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The hierarchy from greatest to least power is: 1) System Security Role (highest privilege, no need to be in Access or Maintenance Group), 2) Maintenance Group (can view, create, edit, and delete objects, no need to be in Access Group), 3) Access Group (can only view the object and read its contents). This hierarchy is important for understanding access levels.
</details>

---

### Question 16 (Easy)
**201.6.2** | Difficulty: Easy

Which System Security Role defaults to "Nobody" and is NOT automatically granted to the Administrator?

A) ManageSystemSecurityUsers
B) ManageFileShare
C) ManageSystemConfiguration
D) ViewAllLogonActivity

<details>
<summary>Show answer</summary>

**Correct answer: C)**

ManageSystemConfiguration defaults to Nobody and is NOT automatically granted to the Administrator. This is an additional security measure since changing server configurations can have significant impact on the entire system. It must be explicitly assigned even for Administrators.
</details>

---

### Question 17 (Medium)
**201.6.2** | Difficulty: Medium

What are the three File Share Security Roles and what level of access does each provide?

A) ReadFileShare (read), WriteFileShare (write), AdminFileShare (administration)
B) ManageFileShareContents (full access), AccessFileShareContents (view and download), RetrieveFileShareContents (access only via application)
C) ViewFileShare (view), EditFileShare (edit), DeleteFileShare (delete)
D) FileShareAdmin (admin), FileShareUser (user), FileShareGuest (guest)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three File Share Security Roles are: ManageFileShareContents (full access to Contents folder: create, upload, download, delete), AccessFileShareContents (only view and download in Contents folder), and RetrieveFileShareContents (does not see the folder in File Explorer but accesses via the application such as Dashboards and Business Rules).
</details>

---

### Question 18 (Hard)
**201.6.2** | Difficulty: Hard

Which Application Security Role allows bulk deletion of metadata and data through the Database page, and why do Administrators not have automatic access?

A) ManageData - because bulk deletion could affect system integrity
B) AdministratorDatabase - because it is a dangerous operation and Administrators do not have automatic access to this role
C) AdministerApplication - because it requires special system permissions
D) ManageMetadata - because bulk deletion requires additional approval

<details>
<summary>Show answer</summary>

**Correct answer: B)**

AdministratorDatabase allows bulk deletion of metadata and data via the Database page. Its default is "Nobody" and Administrators do NOT have automatic access. This is intentional as a protective measure, since bulk deletion is a potentially destructive operation that must be explicitly assigned.
</details>

---

### Question 19 (Hard)
**201.6.2** | Difficulty: Hard

**Scenario**: A user needs to access System Configuration to make changes. What combination of roles do they need?

A) Only ManageSystemConfiguration
B) Only SystemConfigurationPage
C) ManageSystemConfiguration (System Security Role) and SystemConfigurationPage (System User Interface Role)
D) AdministerApplication and SystemPane

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The user needs both roles: ManageSystemConfiguration (System Security Role, defaults to Nobody) to have the ability to make changes, and SystemConfigurationPage (System User Interface Role, defaults to Administrator Group for read-only access) to be able to access the page. Important note: SystemConfigurationPage alone gives read-only access, and ManageSystemConfiguration is not automatically granted with SystemConfigurationPage.
</details>

---

### Question 20 (Medium)
**201.6.2** | Difficulty: Medium

Which Cube View properties should be set to False for non-administrator users who only need to view reports?

A) Visible in Profiles
B) Can Modify Data, Can Calculate, Can Translate, Can Consolidate
C) Enable Security
D) Allow Data Entry

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The properties Can Modify Data, Can Calculate, Can Translate, and Can Consolidate should be set to False in the Cube View properties for non-administrator users. This ensures the Cube View is used only as a read-only report and users cannot modify data or execute calculations from it.
</details>

---

## Objective 201.6.3: Troubleshoot Security-Related Issues

### Question 21 (Easy)
**201.6.3** | Difficulty: Easy

What is the first step in the diagnostic flow when a user cannot access something in OneStream?

A) Verify the Application Security Roles
B) Verify that the user is enabled (Is Enabled = True)
C) Verify Cube access
D) Verify Workflow Security

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The first step is to verify that the user is enabled: System > Security > Users > Is Enabled = True. If the user is disabled, no other verification will be relevant. You should also check the Logon Inactivity Threshold, as if Remaining Allowed Inactivity = 0 Days, the user has been disabled due to inactivity.
</details>

---

### Question 22 (Easy)
**201.6.3** | Difficulty: Easy

What are the User Types available in OneStream?

A) Admin, Power User, Standard User, Guest
B) Interactive, View, Restricted, Third Party Access, Financial Close
C) Full Access, Limited Access, Read Only, API Only
D) Premium, Standard, Basic, Trial

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The User Types are: Interactive (full access to all features and tools), View (data, reports, and dashboards only), Restricted (limitations on some OneStream Solution features), Third Party Access (access via third-party application, cannot change data), and Financial Close (Account Reconciliation and Transaction Matching). Important: User Type is a license type and does NOT control security access.
</details>

---

### Question 23 (Medium)
**201.6.3** | Difficulty: Medium

**Scenario**: A user reports that they can see the Workflow but gets an error when trying to validate and load data. They have the Workflow Execution Group assigned. What is the most likely cause?

A) They do not have the ModifyData role
B) They do not have Entity Read/Write access for the Workflow's entity
C) The Workflow is locked
D) They do not have the ManageData role

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If a user has Workflow Execution but does not have Entity Read/Write access, they will see the Workflow but get an error when validating/loading data. Access to the Workflow allows viewing the interface, but data loading also requires write permissions on the Entity. This is a common troubleshooting scenario.
</details>

---

### Question 24 (Medium)
**201.6.3** | Difficulty: Medium

What three special characteristics does the Administrator user have in OneStream regarding security?

A) Can create users, can change roles, can restart the system
B) Cannot be disabled or deleted, is not affected by Logon Inactivity Threshold, bypasses all application security
C) Has access to all pages, can export all data, can modify System Configuration
D) Can create applications, can delete applications, can manage servers

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Administrator user: 1) Cannot be disabled or deleted, 2) Is not affected by Logon Inactivity Threshold, 3) Bypasses all application security (this cannot be changed). Assigning other groups to roles does not revoke the Administrator's access. For sensitive data, Event Handlers and BRAPI calls are needed to exclude the Administrator.
</details>

---

### Question 25 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: An administrator configures the Logon Inactivity Threshold to 30 days in the Application Server Configuration. What must they do after making this change, and which users are not affected by this setting?

A) Restart the application; does not affect users with the ViewAllData role
B) Perform an IIS reset; does not affect the Administrator user
C) No additional action needed; affects all users without exception
D) Restart the Scheduler service; does not affect external users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

After configuring the Logon Inactivity Threshold, an IIS reset is required for the change to take effect. This setting does not apply to the Administrator user, who can never be disabled due to inactivity. It applies to both native and external users. If a user reaches 0 days of allowed inactivity, they receive an error when attempting to log in and must be manually re-enabled.
</details>

---

### Question 26 (Easy)
**201.6.3** | Difficulty: Easy

How do you access a user's group information to see all their groups and understand their access?

A) System > Security > Users > Permissions Tab
B) Application > Tools > Security Roles > User Details
C) Using the "Show All Parent Groups for User" tool
D) System > Logging > Logon Activity

<details>
<summary>Show answer</summary>

**Correct answer: C)**

"Show All Parent Groups for User" is a useful tool for viewing all of a user's groups and understanding their complete access, including groups inherited through nesting. This is fundamental for troubleshooting since groups can be nested hierarchically and child groups inherit access from the parent group.
</details>

---

### Question 27 (Medium)
**201.6.3** | Difficulty: Medium

How do Exclusion Groups work and what is the correct order for excluding specific users?

A) They are configured with Allow/Deny Access; put the general group first (Allow) and individual users after (Deny)
B) Users are added to a blacklist; order does not matter
C) They are configured with Include/Exclude; put individual users first (Exclude) and the general group after (Include)
D) Separate groups are created for each excluded user

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Exclusion Groups are configured with members as "Allow Access" or "Deny Access". The order matters because access is evaluated based on the order specified, regardless of the user's group membership. To exclude specific users from a general group: put the general group first (Allow), then the individual users (Deny). This allows granting access to almost everyone except a few specific users.
</details>

---

### Question 28 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: A non-Administrator user has the ManageSystemSecurityUsers role. What are the limitations of this role?

A) Cannot create new users, can only modify existing ones
B) Cannot create/modify/delete administrators, cannot add themselves to groups or roles, cannot delete their own account, cannot grant Manage System Security to others
C) Cannot modify external users, can only manage native users
D) Can only view the user list, cannot make changes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A user with ManageSystemSecurityUsers can create, modify, and delete users, but has important restrictions: cannot create/modify/delete administrators, cannot add themselves to groups or roles, cannot delete their own account, and cannot grant Manage System Security to others. These limitations prevent privilege escalation.
</details>

---

### Question 29 (Hard)
**201.6.3** | Difficulty: Hard

What tool can be used for bulk loading of security and what template file is used?

A) Application > Tools > Import Security with SecurityImport.csv file
B) System > Tools > Load/Extract with SecurityTemplate.xlsx from the SampleTemplates OneStream Solution to generate XML
C) System > Administration > Bulk Import with Users.xml file
D) Application > Tools > Data Sources with SecurityData.xlsx file

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For bulk loading of security, use System > Tools > Load/Extract. The SecurityTemplate.xlsx from the SampleTemplates OneStream Solution can be used to generate the necessary XML. To load: Load > Browse XML. To extract: Extract > File Type: Security. When extracting, you can choose Items to Extract: Users, Security Groups, All, and you should uncheck Extract Unique IDs when moving between environments.
</details>

---

### Question 30 (Medium)
**201.6.3** | Difficulty: Medium

**Scenario**: A user can see Dashboards in the list but when they open a specific one, the data cells show "NoAccess". What should be verified?

A) Only the Dashboard Profile access
B) Only the user's Application Security Roles
C) The user's access to the Entity (Read Data Group) or the Data Cell Access rules
D) The Dashboard Component configuration

<details>
<summary>Show answer</summary>

**Correct answer: C)**

If a user sees the Dashboard but cells show "NoAccess", the problem is not Dashboard access but access to the underlying data. You should verify the user's Entity Read Data Group and/or Data Cell Access (Slice Security) rules. If a Dashboard points to data the user cannot see, it may show NoAccess, blank cells, or "No Data Series".
</details>

---

### Question 31 (Easy)
**201.6.3** | Difficulty: Easy

What happens if a user is not assigned to any Workflow Group?

A) They see the Workflow with read-only data
B) They see all available Workflows
C) They will not see the Workflow; they will only see Cube Root
D) They can see the Workflow but cannot execute actions

<details>
<summary>Show answer</summary>

**Correct answer: C)**

If a user does not have the Workflow Group assigned, they will not even see the Workflow; they will only see Cube Root. The Workflow Group is necessary for the Workflow to appear in the user's interface. This differs from having the Workflow Execution Group but not having Entity access, where the user does see the Workflow but gets errors when operating.
</details>

---

### Question 32 (Medium)
**201.6.3** | Difficulty: Medium

Can security access be assigned directly to individual users in OneStream?

A) Yes, it can be assigned directly to users and to groups
B) No, access is determined solely through System Security Roles assigned to Groups
C) Yes, but only for Application Security Roles
D) No, except for the Administrator user

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You cannot assign access to individual users directly to tools and artifacts. All access is determined through System Security Roles assigned to Groups. Groups can be nested hierarchically, and child groups inherit access from the parent group. Groups cannot be externally authenticated.
</details>

---

### Question 33 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: A non-Administrator user has the roles ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles. What effect does combining these three roles have?

A) No additional effect; each role functions independently
B) The three roles combined amplify capabilities, and additionally the user gains automatic access to SystemAdministrationLogon and SystemPane UI Roles. They can also perform Load/Extract of Security with SystemLoadExtractPage
C) Combined they equal the complete Administrator access
D) Only two of the three are needed to obtain amplified capabilities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Having more than one Manage System Security role amplifies the user's capabilities. Users with these roles gain automatic access to SystemAdministrationLogon and SystemPane UI Roles. For Load/Extract of Security, all three roles plus the SystemLoadExtractPage UI Role are required. However, they do not equal complete Administrator access, as they still have restrictions (cannot modify the Administrators group, etc.).
</details>

---

### Question 34 (Hard)
**201.6.3** | Difficulty: Hard

How often are changes made in System Configuration automatically applied and where are they recorded?

A) Immediately; recorded in the Error Log
B) Every 2 minutes without needing an IIS restart; recorded in the Audit tab
C) Require an IIS restart; recorded in Task Activity
D) Every 5 minutes; recorded in Logon Activity

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Changes to System Configuration are automatically applied every 2 minutes without needing to restart IIS. They are recorded in the Audit tab of System Configuration. The configuration has 6 categories: General, Environment, Memory, Multithreading, Recycling, and Database Server Connections.
</details>

---

### Question 35 (Medium)
**201.6.3** | Difficulty: Medium

What are the external authentication providers supported by OneStream?

A) Only Microsoft Active Directory and LDAP
B) Microsoft Active Directory (MSAD), LDAP, OpenID Connect (Azure AD/Entra ID, Okta, PingFederate), SAML 2.0 (Okta, PingFederate, ADFS, Salesforce)
C) Only Azure AD and Okta
D) Microsoft Active Directory, Google Auth, Amazon Cognito, Auth0

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream supports multiple external authentication providers: Microsoft Active Directory (MSAD), LDAP, OpenID Connect (with Azure AD/Microsoft Entra ID, Okta, PingFederate), and SAML 2.0 (with Okta, PingFederate, ADFS, Salesforce). In OneStream-hosted environments, OneStream IdentityServer is used. Native authentication uses External Provider = (Not Used) with an internal password.
</details>

---

### Question 36 (Easy)
**201.6.3** | Difficulty: Easy

Where is the Logon Inactivity Threshold configured to disable inactive users?

A) Application > Tools > Application Properties
B) System > Security > Users
C) Application Server Configuration > Authentication > Security
D) System > Administration > System Roles

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Logon Inactivity Threshold is configured in Application Server Configuration > Authentication > Security. It is specified in days and applies to both native and external users. After configuring it, an IIS reset is required. It does not apply to the Administrator user.
</details>

---

### Question 37 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: An administrator needs to prevent the Administrator user from viewing sensitive People Planning data. What options do they have?

A) Assign the Administrator to a group with restricted access
B) Use Data Cell Access Security to restrict the Administrator
C) Use Event Handlers and BRAPI calls to exclude the Administrator from viewing certain data
D) Revoke the ViewAllData role from the Administrator

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Administrator bypasses all application security, including Data Cell Access Security, and this cannot be changed. The only way to exclude the Administrator from viewing certain sensitive data (such as People Planning) is through programmatic Event Handlers and BRAPI calls. It cannot be restricted with conventional roles or groups.
</details>

---

### Question 38 (Medium)
**201.6.3** | Difficulty: Medium

A user needs to view Dashboards in the application. What combination of access do they need?

A) Only access to the Dashboard Group
B) Only access to the Dashboard Profile
C) Access to both the Dashboard Profile AND the Dashboard Group
D) Only the Application Security Role ManageApplicationDashboards

<details>
<summary>Show answer</summary>

**Correct answer: C)**

To view Dashboards, the user needs access to both the Dashboard Profile and the Dashboard Group. If they have access to the Group but not the Profile, they will not see the Dashboards in OnePlace. Both levels of access are necessary for the Dashboard to appear in the user's interface.
</details>

---

### Question 39 (Hard)
**201.6.3** | Difficulty: Hard

What are the recommended best practices for security configuration in OneStream?

A) Apply maximum security from the beginning and then relax it as needed
B) Do not over-apply security (it is easier to add later), maintain consistent naming convention, use group nesting without overcomplicating, set Access Group to Everyone + Maintenance to Administrators for most objects
C) Assign all roles to all users and restrict individually
D) Create one group per user for maximum granularity

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Best practices include: do not over-apply security (it is easier to add later than to unravel), maintain a consistent naming convention (e.g., V_Entity for view, M_Entity for modification), use group nesting to ease administration without overcomplicating, and configure Access Group to Everyone + Maintenance to Administrators as standard practice for most objects like Confirmation Rules, Certification Questions, and Form/Journal Templates.
</details>

---

### Question 40 (Medium)
**201.6.3** | Difficulty: Medium

What information can be verified in Logon Activity for security troubleshooting?

A) Only the user's name and login time
B) The login method (Client Module) showing whether it was via Scheduler, Windows App, etc.
C) Only authentication errors
D) Only failed login attempts

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Logon Activity (System > Logging) shows the user's login method through the Client Module field, which indicates whether the login was via Scheduler, Windows App, Excel Add-In, etc. This information is valuable for troubleshooting as it allows identifying where the user is attempting to access from and correlating with potential security issues.
</details>

---

## New Questions (41-60)

### Question 41 (Easy)
**201.6.1** | Difficulty: Easy

What happens to the security verification process if a user fails any one of the security checkpoints when attempting to access data?

A) The system continues checking the remaining layers and provides partial access
B) The process stops immediately and the user is denied access
C) The system logs the failure but still grants read-only access
D) The user is prompted to re-authenticate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If any of the security verification steps (OpenApplication, Cube Access, Scenario Access, Entity Access, Data Cell Access, Workflow Verification) results in "no access," the process stops immediately. There is no partial access — the security layers are evaluated sequentially and all must pass for the user to proceed.
</details>

---

### Question 42 (Easy)
**201.6.2** | Difficulty: Easy

What is the default assignment of the OpenApplication security role when a new application is created?

A) Administrator
B) Nobody
C) Everyone
D) A custom security group must be specified

<details>
<summary>Show answer</summary>

**Correct answer: C)**

When a new application is created, the OpenApplication role defaults to Everyone, allowing all users to open the application. If only specific users should have access, an administrator can create a security group, assign users to it, and then change the OpenApplication role to that specific group.
</details>

---

### Question 43 (Easy)
**201.6.3** | Difficulty: Easy

How are users configured for native authentication in OneStream?

A) External Authentication Provider is set to "Native" and a password is entered
B) External Authentication Provider is set to "(Not Used)" and a password is entered in Internal Provider Password
C) External Authentication Provider is set to "OneStream" and no password is needed
D) Native authentication requires a separate configuration file

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For native authentication, when creating a user in System > Security, the External Authentication Provider is set to "(Not Used)" and the user's password is entered in the Internal Provider Password field. The first time the user logs in, they can change their password. For external authentication, the appropriate provider is selected and the External Provider User Name must match the identity provider.
</details>

---

### Question 44 (Medium)
**201.6.1** | Difficulty: Medium

When designing Entity security, what should be created first and why?

A) The Read Data Group, because reporting is more important than data loading
B) The Read/Write Data Group, because it is needed for data loading in Workflows
C) Both groups simultaneously, because they depend on each other
D) The Workflow Execution Group, because workflow controls all access

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Entity Read/Write Data Group should be designed first because it is needed for data loading in Workflows. The Workflow Execution Security Group should be assigned to all the Entities' Read/Write Security Group for the Workflow to gain loading access to the Entities. View groups are set up afterward based on how users need to view data, by segment or region.
</details>

---

### Question 45 (Medium)
**201.6.2** | Difficulty: Medium

What is the relationship between the Access Group and Maintenance Group for an object in OneStream?

A) A user must be in both groups to see and edit an object
B) A user in the Maintenance Group does not need to be in the Access Group — they can create, edit, and delete objects
C) A user in the Access Group can also edit objects if they have an Application Security Role
D) The Maintenance Group only provides read access with edit capabilities for metadata

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Users in the Maintenance Group can see, create, edit, and delete objects, and they do not need to be in the Access Group. Users in the Access Group can only view the object and read its contents. Users with a System Security Role for that object type have the highest privilege and do not need to be in either group.
</details>

---

### Question 46 (Medium)
**201.6.1** | Difficulty: Medium

In the context of Entity security, what must be done for a user to view data at a parent-level Entity?

A) The user only needs access to the parent Entity's Read Data Group
B) All child Entities' View Groups below the parent must be assigned to the parent-level Entity View Group
C) The user needs the ViewAllData role to see parent-level data
D) Parent-level data is automatically visible to anyone with child Entity access

<details>
<summary>Show answer</summary>

**Correct answer: B)**

All the Entities' View Groups below the Parent must be assigned to the Parent Level Entity View Group in order to gain access to data at the Parent Level Entity and view Entities below it. This ensures that users can see both the rolled-up data at the parent and the child-level detail, based on their group assignments.
</details>

---

### Question 47 (Medium)
**201.6.2** | Difficulty: Medium

What three system security roles must a non-Administrator user have to perform a Load/Extract of security data?

A) ManageSystemSecurityUsers, ManageFileShare, and SystemLoadExtractPage
B) ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles (plus SystemLoadExtractPage UI Role)
C) ManageSystemConfiguration, ManageSystemDatabase, and ManageFileShare
D) AdministerApplication, ManageSystemSecurityUsers, and ManageSystemSecurityGroups

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Load and Extract of Security requires a user to have all three Manage System Security roles: ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles, as well as the System User Interface Role SystemLoadExtractPage. Validation during the load compares the current state of security in the target environment to the changed state from the source file.
</details>

---

### Question 48 (Medium)
**201.6.3** | Difficulty: Medium

When copying a user in OneStream, what does the "Copy References made by parent groups" option do?

A) It copies all security roles from the original user
B) It adds the new user to the original user's groups except exclusion groups
C) It copies the user's dashboard and Cube View access
D) It duplicates the user's complete security profile including exclusion groups

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When copying a user and selecting "Copy References made by parent groups," the new user is added to the original user's groups except exclusion groups. This is a convenient way to replicate a user's group-based access without manually assigning each group, while exclusion groups remain separate to prevent unintended access denials.
</details>

---

### Question 49 (Hard)
**201.6.1** | Difficulty: Hard

**Scenario**: An organization has a data loader who needs to load data for two entities (HoustonHeights and SouthHouston) within the same workflow but should NOT be able to certify the workflow parent (Houston). How should security be designed?

A) Assign the user to the Entity Read/Write groups and remove the Workflow Execution group
B) Create a single workflow group (e.g., WF_HoustonWorkflow), nest the Entity Read/Write groups (M_HoustonHeights, M_SouthHouston) into it, assign the workflow execution group, but do NOT assign the Certification Signoff group
C) Assign the user to the Administrator group and restrict certification via Data Cell Access
D) Use Data Cell Conditional Input to prevent certification

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to create a workflow group (e.g., WF_HoustonWorkflow) and nest the entity modification groups into it. The user is assigned to this single workflow group, which provides both entity write access and workflow execution access. The Certification Signoff group is assigned separately to the certifier role, not the data loader. This design keeps security clean and leverages group nesting effectively.
</details>

---

### Question 50 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: A user has been granted ViewAllData access and is also assigned to a Data Cell Access (slice) security group that restricts P&L account visibility for a specific entity. Will the slice security restriction apply?

A) Yes, slice security always overrides role-based access
B) No, because ViewAllData bypasses Data Cell Access Security entirely
C) Yes, but only for entities to which the user does not have Entity Read/Write access
D) It depends on the order of the Data Cell Access rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Once a user is in the ViewAllData role group, Data Cell Access (slice) security does not apply to them. The ViewAllData role grants unrestricted read access to all data in all cubes. To restrict such a user to specific data subsets, you would need to remove them from ViewAllData and instead build out entity-by-entity access with slice security to decrease visibility where needed.
</details>

---

### Question 51 (Hard)
**201.6.2** | Difficulty: Hard

**Scenario**: A non-Administrator user has the ManageSystemSecurityGroups role. They attempt to modify the Administrators group by adding a new user to it. What happens?

A) The modification succeeds because the user has the ManageSystemSecurityGroups role
B) The modification fails because users with this role cannot modify the Administrators group or assign users to groups that establish Administrator privileges
C) The modification succeeds but requires approval from an Administrator
D) The modification fails only if the user being added is already in another group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Users with the ManageSystemSecurityGroups role cannot modify the Administrators group, assign users to a group that establishes Administrator privileges, modify their own membership in other groups, or modify the parent group of a group in which they are a member. These restrictions prevent unauthorized privilege escalation.
</details>

---

### Question 52 (Medium)
**201.6.2** | Difficulty: Medium

What is the recommended best practice for controlling access to Confirmation Rules?

A) Create specific security groups for each Confirmation Rule Group
B) Set Access to Everyone and Maintenance to Administrators for both Confirmation Rule Groups and Profiles
C) Use Application Security Roles to manage individual rule access
D) Restrict access through Data Cell Access rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice for Confirmation Rules is to set Access to Everyone and Maintenance to Administrators for both Confirmation Rule Groups and Profiles. Runtime access to Confirmation Rules depends on the Workflow Profile to which they are assigned — if a user has Workflow Execution Access, they can execute them. The same approach applies to Certification Questions and Form/Journal Templates.
</details>

---

### Question 53 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: An administrator extracts security from a development environment to load into a production environment using Load/Extract. What critical setting should they uncheck, and why?

A) Uncheck "Include Groups" to avoid overwriting production groups
B) Uncheck "Extract Unique IDs" because unique IDs are environment-specific and can cause conflicts in the target environment
C) Uncheck "Include Passwords" for security compliance
D) Uncheck "Extract Roles" to preserve production role assignments

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When extracting security to move between environments, "Extract Unique IDs" should be unchecked. Unique IDs are specific to each environment, and loading them into a different environment can cause conflicts and errors. The XML load file should contain user and group properties without environment-specific identifiers so they can be properly created in the target environment.
</details>

---

### Question 54 (Easy)
**201.6.1** | Difficulty: Easy

What is the purpose of the Scenario dimension's security groups?

A) They control which users can create new scenarios
B) They have both Read Data Group and Read/Write Data Group, controlling who can view or modify data within a scenario
C) They only control workflow execution access
D) They determine which time periods are accessible

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Scenario dimension has both Read Data Group and Read/Write Data Group, similar to the Entity dimension. These security groups control who can view data (Read) and who can modify data (Read/Write) within each scenario. This is one of the security checkpoints in the data access verification flow, checked after Cube access and before Entity access.
</details>

---

### Question 55 (Medium)
**201.6.2** | Difficulty: Medium

What are the three ways to add users and groups to a OneStream application?

A) Manual entry, CSV import, and Active Directory sync
B) Define them manually in System Security, bulk import using XML load file, and use APIs (BRApi)
C) Manual entry, LDAP sync, and Excel import
D) Dashboard creation, Data Management sequences, and manual entry

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Users and groups can be added to a OneStream application in three ways: 1) Define them manually in System Security, 2) Load them in a bulk import using an XML load file generated from the SecurityTemplate.xlsx provided by the SampleTemplates OneStream Solution, and 3) Use APIs via BRApi functions such as CopyUser, DeleteUser, SaveUser, and CopyGroup.
</details>

---

### Question 56 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: An administrator wants to restrict data input so that Trial Balance data cannot be loaded through the Forms origin, while allowing it through the Import origin. How should this be implemented?

A) Create a Data Cell Access rule restricting Forms origin access for the user group
B) Use Data Cell Conditional Input to set the Trial Balance account intersection with the Forms origin to Read Only
C) Remove the ModifyData role from the forms users
D) Configure the Workflow to prevent Forms access

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is accomplished using Data Cell Conditional Input (not Data Cell Access Security). Navigate to Application > Cube > Cubes > Data Access > Data Cell Conditional Input, create a rule with the dimension intersection restricting loading to the Trial Balance account through the Forms origin, set the behavior to "In Filter" and the Access Level to "Read Only." This impacts all users and is not group-based — it is a functional restriction, not a security mechanism.
</details>

---

### Question 57 (Hard)
**201.6.1** | Difficulty: Hard

**Scenario**: An organization needs to allow a historical "Preserve" scenario to bypass all Data Cell Conditional Input rules that restrict current-period data entry. How can this be achieved?

A) Remove all Data Cell Conditional Input rules and recreate them with scenario filters
B) Create a new Data Cell Conditional Input rule for the Preserve scenario at the top of the list with "Increase Access And Stop" behavior and Read Only access
C) Assign the Preserve scenario to a special security group that bypasses rules
D) Create a separate cube for the Preserve scenario without any rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Create a new Data Cell Conditional Input rule for the entire Preserve scenario, set the behavior to "Increase Access And Stop" and the Access Level to Read Only, then position this rule at the top of the Data Cell Conditional Input list. The "Increase Access And Stop" behavior means that if the current cell matches the Preserve scenario filter, access is increased and all subsequent rules are ignored, effectively bypassing the restrictions for that scenario.
</details>

---

### Question 58 (Medium)
**201.6.2** | Difficulty: Medium

What is the purpose of the "Encrypt System Business Rules" system security role?

A) It encrypts all data transmitted between the server and client
B) It allows a user to encrypt/decrypt a business rule from the Business Rule screen in the System tab to obfuscate the contents from all users
C) It encrypts the database connection strings
D) It enables SSL/TLS encryption for the application

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The "Encrypt System Business Rules" role allows a user to encrypt/decrypt a rule from the Business Rule screen in the System tab to obfuscate the contents of the rule from all users. This is useful when business rules contain proprietary logic or sensitive formulas that should not be visible to other users, even administrators viewing the rule code.
</details>

---

### Question 59 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: A company undergoes a corporate reorganization and needs to remove a small number of users from accessing specific dashboards, while keeping access for everyone else. The affected users are members of several groups that provide dashboard access. What is the most efficient approach?

A) Remove each user from every group that provides dashboard access
B) Create an Exclusion Group with the affected users set to "Deny Access" and all other groups set to "Allow Access," then assign this exclusion group to the relevant dashboard roles
C) Create a new security group without the affected users and reassign all dashboard access
D) Disable the affected users and create new accounts with reduced access

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Creating an Exclusion Group is the most efficient approach. Add the groups that should retain access set to "Allow Access" and then add the specific users to "Deny Access" below those groups. The order is critical: the general groups must be listed first with "Allow Access," and the individual users listed below with "Deny Access." This avoids the time-consuming task of removing users from multiple groups in the hierarchy.
</details>

---

### Question 60 (Hard)
**201.6.3** | Difficulty: Hard

**Scenario**: A user needs to manage Transformation Rules for a specific location's Account Transformation Rule Group, but should not have access to modify core or corporate-level Transformation Rule Groups. How should object-level security be configured?

A) Assign the user to the ManageTransformationRules Application Security Role
B) Set Access to Everyone and Maintenance to Administrators for core Transformation Rule Groups; assign the user's group to both Access and Maintenance for the location-specific group; and block access to the Maintenance screen for non-administrators
C) Create a separate application for the location-specific rules
D) Use Data Cell Access to restrict transformation rule editing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practice is to set Access to Everyone and Maintenance to Administrators for most core, shared, or corporate Transformation Rule Groups. For specific Transformation Rule Groups — such as an Account Transformation Rule Group for a specific location — assign the appropriate user groups to both Access and Maintenance. Additionally, block access to the general Maintenance screen for anyone except administrators, as this could potentially allow users more access than needed.
</details>

---

---


# Question Bank - Section 7: Administration (9% of exam)

## Objective 201.7.1: Demonstrate an understanding of the items available on the system tab

### Question 1 (Easy)
**201.7.1** | Difficulty: Easy

From which location are Error Logs accessed in OneStream?

A) Application > Tools > Error Logs
B) System > Logging > Error Logs
C) System > Tools > Error Logs
D) OnePlace > Logging > Error Logs

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Error Logs are located at **System > Logging > Error Logs**. The Logging section of the System tab contains Logon Activity, Task Activity, and Error Logs. Option A is incorrect because Error Logs are not under Application. Option C is incorrect because Tools contains Database, Environment, and Profiler, not logs. Option D is incorrect because OnePlace is for Workflow navigation, not administration.
</details>

---

### Question 2 (Easy)
**201.7.1** | Difficulty: Easy

Which column in Logon Activity indicates the type of client from which a user connected (for example, Excel)?

A) Client Version
B) Client IP Address
C) Client Module
D) Primary App Server

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The **Client Module** column shows the type of client used for the connection, such as Excel. Client Version shows the software version, Client IP Address shows the IP address, and Primary App Server shows the server handling the session.
</details>

---

### Question 3 (Easy)
**201.7.1** | Difficulty: Easy

What are the three storage roots available in File Explorer?

A) Public, Private, Shared
B) Application Database, System Database, File Share
C) Incoming, Outgoing, Content
D) Local, Network, Cloud

<details>
<summary>Show answer</summary>

**Correct answer: B)**

File Explorer has three storage roots: **Application Database** (documents for the current application), **System Database** (documents for the entire system), and **File Share** (a self-service directory external to the databases). The other options do not represent the correct storage roots.
</details>

---

### Question 4 (Easy)
**201.7.1** | Difficulty: Easy

What function allows an administrator to disconnect a user session from Logon Activity?

A) Clear Logon Activity
B) End User Session
C) Logoff Selected Session
D) Terminate Connection

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The **Logoff Selected Session** function allows an administrator to disconnect any active user session from Logon Activity. Clear Logon Activity only clears the historical log and does not disconnect active sessions.
</details>

---

### Question 5 (Easy)
**201.7.1** | Difficulty: Easy

Where is the Use Detailed Logging property configured for Cube Views?

A) In the Application Server Configuration File
B) In the System tab under Logging
C) Individually in each Cube View, in the Designer and Advanced tabs
D) In the global Task Activity configuration

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The **Use Detailed Logging** property is configured **individually in each Cube View**, in the Designer and Advanced tabs. Previously, this setting resided in the App Server Config file and TALogCubeViews, but it was moved to each individual Cube View.
</details>

---

### Question 6 (Easy)
**201.7.1** | Difficulty: Easy

What is the maximum file size that can be uploaded through File Share Content folders using the Windows App?

A) 300 MB
B) 500 MB
C) 1 GB
D) 2 GB

<details>
<summary>Show answer</summary>

**Correct answer: D)**

**File Share Content folders** support files up to **2 GB** when using the Windows App. Uploads to Application/System Database are limited to 300 MB. This distinction is important for administrators handling large files.
</details>

---

### Question 7 (Medium)
**201.7.1** | Difficulty: Medium

An administrator needs certain file extensions to be allowed in File Explorer. Where should the Whitelist File Extensions be configured, and what additional action is required?

A) In System > Tools > File Explorer; no additional action is required
B) In the OneStream Application Server Configuration File (XFAppServerConfig); requires an IIS restart
C) In Application > Security > File Permissions; requires logging out and back in
D) In System > Logging > Settings; requires a database service restart

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Whitelist File Extensions** are configured in the **OneStream Application Server Configuration File** (XFAppServerConfig) and require an **IIS restart** after saving the changes. Cloud customers must contact OneStream support to have this configuration change made.
</details>

---

### Question 8 (Medium)
**201.7.1** | Difficulty: Medium

What are the main sections available in System > Tools > Environment?

A) Logging, Security, Profiles, Reports
B) Monitoring, Web Servers, Application Server Sets, Database Servers
C) Users, Groups, Roles, Permissions
D) Cubes, Dimensions, Workflows, Data Management

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Environment area includes sections such as **Monitoring** (real-time KPIs), **Web Servers**, **Mobile Web Servers**, **Web to App Server Connections**, **Application Server Sets**, **Application Server Behavior/Hardware**, and **Database Servers**. Additionally, Environment is only accessible via the **OneStream Windows App**, not from a browser.
</details>

---

### Question 9 (Medium)
**201.7.1** | Difficulty: Medium

An administrator notices that the Task Activity icon is blinking. What does this mean?

A) There is a critical system error that requires immediate attention
B) One or more background tasks have been running for more than 10 seconds
C) A consolidation task has completed successfully
D) The application server needs to be restarted

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Task Activity icon **blinks** when background tasks have been running for more than **10 seconds**. This serves as a visual indicator to let the user know there are active processes. It does not indicate errors or a need for restart.
</details>

---

### Question 10 (Medium)
**201.7.1** | Difficulty: Medium

Which of the following items can be exported using Load/Extract System Artifacts?

A) Business Rules, Member Formulas, Cube Views
B) Security (System Roles, Users, Security Groups), System Dashboards, Error Log, Task Activity, Logon Activity
C) Dimensions, Cubes, Scenarios, Workflows
D) Data Management Steps, Transformation Rules, Connector Data Sources

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Load/Extract System Artifacts allows exporting/importing in XML format the following items: **Security** (System Roles, Users, Security Groups, Exclusion Groups), **System Dashboards**, **Error Log**, **Task Activity**, and **Logon Activity**. This function is available only to System Administrators.
</details>

---

### Question 11 (Medium)
**201.7.1** | Difficulty: Medium

Regarding the Profiler, what is the maximum session runtime and how long are results retained?

A) Maximum 30 minutes of runtime, 48 hours of retention
B) Maximum 60 minutes of runtime, 168 hours of retention
C) Maximum 120 minutes of runtime, 72 hours of retention
D) Maximum 45 minutes of runtime, 336 hours of retention

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Profiler has a maximum of **60 minutes** of runtime (Number of Minutes to Run, default 20) and a maximum of **168 hours** (7 days) of retention before deletion (Number of Hours to Retain Before Deletion, default 24). Additionally, multiple users running the Profiler simultaneously can impact application performance.
</details>

---

### Question 12 (Medium)
**201.7.1** | Difficulty: Medium

What Security Roles are related to the Profiler, and what do they allow?

A) AdminProfiler (run and view) and ViewProfiler (view only)
B) ManageProfiler (run sessions) and ProfilerPage (view the page, but cannot run sessions)
C) RunProfiler (run sessions) and ReadProfiler (view results)
D) ProfilerAdmin (full access) and ProfilerUser (limited access)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The two Profiler Security Roles are: **ManageProfiler** (allows running profiling sessions) and **ProfilerPage** (allows viewing the Profiler page but not running sessions). This distinction is important for controlling who can impact system performance.
</details>

---

### Question 13 (Medium)
**201.7.1** | Difficulty: Medium

What happens when Pause is used on a Web to App Server connection?

A) All currently running tasks on that server are stopped immediately
B) The server is prevented from accepting new tasks from the queue, but in-progress tasks continue until completion
C) All users connected to that server are disconnected
D) The IIS on the application server is restarted

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Pause** on an Application Server stops the acceptance of new tasks from the queue but allows in-progress tasks to finish. This enables controlled maintenance without interrupting active processes. Resume restores the acceptance of new tasks.
</details>

---

### Question 14 (Medium)
**201.7.1** | Difficulty: Medium

An administrator is moving security configurations between environments using Load/Extract System Artifacts. Some security records already exist in the destination environment. What should be done with the Extract Unique IDs option?

A) Leave the option checked to maintain consistency
B) Uncheck the option to avoid errors with existing records
C) Delete all existing records before importing
D) The option is not relevant in this scenario

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You should **uncheck** the **Extract Unique IDs** option when moving security between environments where records already exist, to avoid unique ID duplication errors. If checked, the original unique IDs are extracted, which can cause conflicts with existing records in the destination.
</details>

---

### Question 15 (Hard)
**201.7.1** | Difficulty: Hard

**Scenario**: An administrator is configuring environment monitoring in Azure. They need to configure automatic application server recycling. Where is the Automatic Recycle Service configured, and what are the key parameters?

A) In System > Tools > Environment > Azure Configurations > Environment Monitoring; parameters: URL, Number of Running Hours Before Automatic Recycle (default 24.0), Start/End Hour (UTC), Maximum Minutes to Pause
B) In Application > Settings > Server Management; parameters: Recycle Interval, Max Connections, Timeout Period
C) In System > Logging > Server Configuration; parameters: Recycle Frequency, Cool Down Period, Max Users
D) In OneStream Server Configuration Utility; parameters: Auto Recycle Timer, Service Endpoint, Queue Limit

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The Automatic Recycle Service is configured in **System > Tools > Environment > Azure Configurations > Environment Monitoring**. The key parameters are: URL of the service, **Number of Running Hours Before Automatic Recycle** (default 24.0; 0.0 to disable), **Start/End Hour** in UTC, and **Maximum Minutes to Pause**. This configuration is specific to Azure environments.
</details>

---

## Objective 201.7.2: Demonstrate an understanding of logging capabilities

### Question 16 (Easy)
**201.7.2** | Difficulty: Easy

What is the first place an administrator should check to determine if a Consolidation completed successfully or failed?

A) Error Logs
B) Task Activity
C) Profiler
D) Database Tools

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Task Activity** is the first place to check whether a Calculation/Consolidation completed successfully or failed. You can filter by Task Type and drill down into child steps to see error details. Error Logs contain all system errors, not just Calculation errors, so Task Activity is more specific.
</details>

---

### Question 17 (Easy)
**201.7.2** | Difficulty: Easy

Which of the following commands is recommended for writing messages to the Error Log from a Finance Business Rule with better performance?

A) BRApi.ErrorLog.LogMessage
B) api.LogMessage
C) Console.WriteLine
D) Debug.Print

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You should use **api.LogMessage** in Finance Business Rules because it has better performance. **BRApi.ErrorLog.LogMessage** is available in ALL rule types but opens new database connections, which is less efficient in Finance Rules that are multi-threaded.
</details>

---

### Question 18 (Easy)
**201.7.2** | Difficulty: Easy

What are the possible Task Status values in Task Activity?

A) Running, Paused, Stopped, Queued
B) Completed, Failed, Canceling, Canceled, Running
C) Success, Error, Warning, Pending
D) Active, Inactive, Complete, Error

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Task Status values are: **Completed**, **Failed**, **Canceling**, **Canceled**, and **Running**. These statuses allow the administrator to quickly identify the outcome of any task in the system.
</details>

---

### Question 19 (Easy)
**201.7.2** | Difficulty: Easy

What text appears in Cube View cells when a user cancels loading the next page (Next Page)?

A) #ERROR
B) #CANCELLED
C) #REFRESH
D) #N/A

<details>
<summary>Show answer</summary>

**Correct answer: C)**

When loading the next page (Next Page) in a Cube View or Quick View is canceled, cells display the text **#REFRESH**. This indicates that the data was not fully loaded and needs to be refreshed.
</details>

---

### Question 20 (Medium)
**201.7.2** | Difficulty: Medium

A developer wants to measure how long a specific section of their code takes in a Business Rule. What VB.NET tool should they use, and what is required?

A) System.Timers.Timer; no additional imports required
B) System.Diagnostics.Stopwatch; requires adding Imports System.Diagnostics to the header
C) System.DateTime.Now; calculate the difference manually
D) api.Performance.MeasureTime; native OneStream function

<details>
<summary>Show answer</summary>

**Correct answer: B)**

You use the **System.Diagnostics.Stopwatch** class from VB.NET. You must add **Imports System.Diagnostics** to the code header. It is started with `Stopwatch.StartNew()` and elapsed time is queried with `.Elapsed`. It is useful for identifying which part of the code consumes the most time.
</details>

---

### Question 21 (Medium)
**201.7.2** | Difficulty: Medium

**Scenario**: A user reports that their Calculation produces no results. The administrator checks Task Activity and the task shows as Completed with no errors. What is the most likely cause, and what tool should be used to diagnose it?

A) The server is overloaded; use Environment Monitoring
B) The dimensions do not match in the Data Buffers; use LogDataBuffer to verify Common Members
C) The user lacks permissions; check Security Settings
D) The Cube View is not configured correctly; check Dashboard Settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a Calculation completes without errors but produces no results, the most likely cause is that **dimensions do not match in the Data Buffers** (different Common Members). You should use **LogDataBuffer** to inspect the Data Buffers, verify Common Members, and adjust the Member Scripts. LogDataBuffer requires an API, a string for the name, and an integer for max number of cells (recommended 100).
</details>

---

### Question 22 (Medium)
**201.7.2** | Difficulty: Medium

What important warning exists about using Calculate With Logging / Consolidate With Logging?

A) It is only available to System Administrators
B) It adds significant processing time to the operation
C) It overwrites previously calculated data
D) It only works with Custom Calculate steps

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Consolidate/Calculate With Logging** adds **significant processing time** to the operation. While it is the key tool for troubleshooting DUCS performance (allowing drill down into each step with duration), it should be used with caution in production environments. It is only available for Calculations that run within the DUCS.
</details>

---

### Question 23 (Medium)
**201.7.2** | Difficulty: Medium

A developer receives the error "Given Key Not Present in Dictionary" when running a Data Management Step. What is the most likely cause and the solution?

A) The Business Rule name is misspelled; correct the name
B) The parameters are not defined in the Data Management Step; use XFGetValue with a default value and verify the parameters
C) The database table does not exist; create the table
D) The user does not have permissions to run the step; assign permissions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The error "Given Key Not Present in Dictionary" occurs when the code tries to access **parameters that are not defined in the Data Management Step**. The solution is to use **XFGetValue** with a default value to handle missing parameters and verify that all required parameters are configured in the DM Step.
</details>

---

### Question 24 (Medium)
**201.7.2** | Difficulty: Medium

What is the difference between the three Tiers that can appear in the Error Log?

A) Development, Testing, Production
B) App Server, Web Server, Client
C) Database, Application, Presentation
D) Internal, External, Hybrid

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three Tiers in the Error Log are: **App Server** (application server errors), **Web Server** (web server errors), and **Client** (client-side errors). This classification helps quickly identify in which layer of the architecture the error originated.
</details>

---

### Question 25 (Medium)
**201.7.2** | Difficulty: Medium

**Scenario**: A Cube View that is included as a component in a Dashboard takes a long time to load, but no cancellation dialog with a progress bar appears. How can the user cancel the load?

A) Press Escape on the keyboard
B) Close the browser and reopen the application
C) Use Task Activity to cancel each individual Cube View of the Dashboard
D) Right-click the component and select Cancel

<details>
<summary>Show answer</summary>

**Correct answer: C)**

For **Dashboards with Cube View Components**, no pop-up cancellation dialog appears. The user must go to **Task Activity** to cancel each individual Cube View. This is different from the behavior of standalone Cube Views, where a dialog does appear after 10 seconds.
</details>

---

## Objective 201.7.3: Troubleshoot common issues using system administration tools

### Question 26 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: A developer is debugging a Business Rule that produces inconsistent results in its calculations. The numbers are correct individually, but the final aggregation is incorrect. What is the most likely cause, and what is the solution?

A) Data type error; convert all values to Decimal
B) Formula Pass issue where there are dependencies between formulas in the same pass; move one formula to Formula Pass 16 to isolate, then correctly reassign the passes
C) Rounding error; increase decimal precision
D) Server concurrency issue; restart the Application Server

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Inconsistent results usually indicate a **Formula Pass issue** where dependencies exist between formulas that are in the same pass. Since formulas in the same pass run in parallel (multithreaded), one formula may use data that has not yet been calculated by another formula in the same pass. The solution is to move a formula to **Formula Pass 16** to isolate the problem and then reassign the passes respecting the order of dependencies.
</details>

---

### Question 27 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: A developer experiences a "Data Explosion" error during a Calculation. What causes this error, and what are the recommended solutions?

A) Too many users running consolidations simultaneously; limit concurrent users
B) The dimensions in the source are not present in the destination of the Data Buffer, generating exponential combinations; never use #All, use Unbalanced functions, or collapse dimensions
C) The database has exceeded its maximum capacity; increase storage
D) The Formula Passes exceed the limit of 16; reduce the number of formulas

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A "Data Explosion" occurs when **dimensions in the source are not present in the destination** of the Data Buffer, generating exponential cell combinations. The solutions are: **never use #All** in scripts, use **Unbalanced functions** (MultiplyUnbalanced, etc.) to handle Data Buffers with different Common Members, or **collapse dimensions** by including them in all operands of the formula.
</details>

---

### Question 28 (Hard)
**201.7.3** | Difficulty: Hard

An administrator needs to use the Profiler to diagnose a performance issue. What filters are available, and how are wildcards used in the Profiler?

A) Filters by user and application; wildcards: % for any string, _ for one character
B) Include filters: Top Level Methods, Adapters, Business Rules, Formulas, Assembly Factory, Assembly Methods; wildcards: ? for any character, * for any string, separated by comma
C) Filters by date and time; wildcards: not available
D) Filters by error type; wildcards: + to include, - to exclude

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Profiler offers **Include** filters: Top Level Methods, Adapters, Business Rules, Formulas, Assembly Factory, and Assembly Methods. Available **wildcards** are: **?** (any single character), **\*** (any string of characters), separated by comma. Post Processing allows calculating Cumulative Durations for performance analysis.
</details>

---

### Question 29 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator receives the error "Invalid Destination Script" when running a Calculation within the DUCS. The code contains an api.Data.Calculate with a formula string that references Entity on the left side (destination). What is the problem, and what is the solution?

A) The formula string has a syntax error; check parentheses
B) Data Unit Dimensions (such as Entity) must not be included in the destination Member Script (left side); use If statements to filter Data Units and only include Account-level Dimensions in the destination
C) The Entity does not exist in the dimension; verify the name
D) The Cube does not have the Business Rule assigned; assign it to the Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The error "Invalid Destination Script" occurs when **Data Unit Dimensions** (such as Entity, Scenario, Time, Consolidation) are included in the **destination Member Script** (left side) of an api.Data.Calculate. Only **Account-level Dimensions** (Account, Flow, Origin, IC, UD1-UD8) can appear in the destination. To control which Data Units are processed, use **If statements** to filter (e.g., `If api.Entity.Name = "EntityX" Then...`).
</details>

---

### Question 30 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator is configuring Task Load Balancing in Azure. They need to understand the key parameters. Which of the following descriptions is correct?

A) Maximum Queued Processing Interval defines the maximum number of queued tasks; Number Past Metric Reading controls monitoring frequency
B) Maximum Queued Time is the maximum time before canceling a queued task; Number Past Metric Reading For Analysis defines how many past readings are analyzed; Maximum Average CPU Utilization defines the CPU threshold
C) Maximum Queued Time is the total allowed execution time for a task; Task Logging Only logs only errors
D) Maximum Average CPU Utilization defines the minimum required CPU; Detailed Logging is activated automatically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Task Load Balancing parameters are: **Maximum Queued Processing Interval** (processing interval), **Number Past Metric Reading For Analysis** (how many past metric readings are analyzed for decision-making), **Maximum Queued Time** (maximum time before canceling a queued task), **Maximum Average CPU Utilization** (maximum CPU threshold), **Task Logging Only**, and **Detailed Logging**. These parameters are critical for load balancing in Azure environments.
</details>

---

### Question 31 (Easy)
**201.7.1** | Difficulty: Easy

What types of information are displayed in the Error Log grid?

A) Description, Error Time, Error Level, User, Application, Tier, App Server
B) Error Code, Stack Trace, Error Type, Server Name, Resolution
C) Date, Category, Priority, Module, Fix Applied
D) Timestamp, Severity, Component, Thread ID, Memory Usage

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The Error Log grid displays: **Description** (brief description of the error), **Error Time** (when the error occurred), **Error Level** (such as Unknown, Fatal, Warning), **User** (user ID), **Application** (which application the user was in), **Tier** (App Server, Web Server, or Client), and **App Server** (the application server connected at the time of the error).
</details>

---

### Question 32 (Easy)
**201.7.1** | Difficulty: Easy

How can Task Activity be accessed in the OneStream web client?

A) Only through System > Logging > Task Activity
B) Only through the Task Activity icon at the top right of the web client
C) Either by clicking Task Activity at the top right of the web client or by navigating to System > Logging > Task Activity
D) Through Application > Tools > Task Activity

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Task Activity can be accessed in two ways: by clicking the **Task Activity** icon at the **top right section of the web client**, or by navigating to **System > Logging > Task Activity**. Both methods provide the same functionality.
</details>

---

### Question 33 (Easy)
**201.7.2** | Difficulty: Easy

What columns in Task Activity show CPU utilization details for a task?

A) CPU Start and CPU End
B) Queued CPU and Start CPU
C) Server CPU and Client CPU
D) Initial CPU and Final CPU

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Task Activity displays **Queued CPU** (the % CPU utilization when the task was initiated) and **Start CPU** (the % CPU utilization when the job began from the queue). These values help administrators understand the server load at the time a task was processed.
</details>

---

### Question 34 (Easy)
**201.7.1** | Difficulty: Easy

Where are public documents saved by an administrator for users to access during their close process?

A) Application > Documents > Public
B) System Tab > Documents
C) OnePlace > File Explorer > Public
D) System > Tools > File Share > Public

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An administrator can save public documents or templates in the **Systems Tab > Documents**, where only administrators have access. However, users can access these public documents through **OnePlace**.
</details>

---

### Question 35 (Medium)
**201.7.1** | Difficulty: Medium

What is the purpose of the Contents folders within the File Share Application and System folders?

A) They store temporary files during data imports
B) They are auto-generated secureable storage areas intended to store files larger than 300 MB, and can be used in place of the File Explorer application database
C) They hold configuration backup files for disaster recovery
D) They store system logs and audit trails

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Both the Application and System folders contain an auto-generated folder named **Contents** intended to store files **larger than 300 MB**. This folder is a secureable storage area that can be used in place of the File Explorer application database. It is managed and secured by System Security Roles, where Administrator and ManageFileShare roles have full rights.
</details>

---

### Question 36 (Medium)
**201.7.1** | Difficulty: Medium

What roles are required for a non-Administrator to load and extract users and groups using Load/Extract System Artifacts?

A) ManageSystemSecurityUsers only
B) SystemLoadandExtractPage, ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles
C) Administrator and ManageFileShare
D) ProfilerPage and ManageProfiler

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To let non-Administrators load and extract users, they must be granted these roles: **SystemLoadandExtractPage** (a User Interface role), **ManageSystemSecurityUsers**, **ManageSystemSecurityGroups**, and **ManageSystemSecurityRoles**. All four roles are required to perform the full load/extract operation.
</details>

---

### Question 37 (Medium)
**201.7.2** | Difficulty: Medium

When a cube view takes longer than 10 seconds to run in the OneStream application, what happens?

A) The cube view is automatically canceled and an error is logged
B) A dialog box displays an indeterminate progress bar and the ability to cancel the cube view or close the pop-up dialog
C) The Task Activity icon immediately turns red
D) The user is redirected to Task Activity to monitor progress

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When running a cube view through Data Explorer or clicking the refresh icon, if it takes **longer than 10 seconds**, a **dialog box displays an indeterminate progress bar** with the ability to **Cancel Task** or **Close**. If the user clicks Close, the dialog closes, Task Activity blinks, and the report opens when completed. If Cancel Task is clicked, the report will not run.
</details>

---

### Question 38 (Medium)
**201.7.1** | Difficulty: Medium

Which of the following external identity providers are supported by OneStream for user authentication in a self-hosted environment?

A) Microsoft Active Directory, LDAP, SAML 2.0, and three OpenID Connect providers (Azure AD, Okta, PingFederate)
B) Only Microsoft Active Directory and LDAP
C) Azure AD, Google Authentication, and Facebook Login
D) SAML 2.0 only, through any compatible identity provider

<details>
<summary>Show answer</summary>

**Correct answer: A)**

OneStream supports the following external identity providers for self-hosted environments: **Microsoft Active Directory (MSAD)**, **Lightweight Directory Access Protocol (LDAP)**, three **OpenID Connect (OIDC)** identity providers (**Azure AD [Microsoft Entra ID]**, **Okta**, **PingFederate**), and **SAML 2.0** identity providers (e.g., Okta, PingFederate, ADFS, and Salesforce).
</details>

---

### Question 39 (Medium)
**201.7.1** | Difficulty: Medium

When creating a user in OneStream, what User Type options are available?

A) Admin, Standard, ReadOnly, API
B) Interactive, View, Restricted, Third Party Access, Financial Close
C) Full Access, Limited Access, External, Service Account
D) Power User, Business User, Report Viewer, Developer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The available User Types are: **Interactive** (can use all features and tools), **View** (can access data, reports, and dashboards but cannot load, calculate, consolidate, certify, or change data), **Restricted** (cannot use some OneStream Solution features), **Third Party Access** (can access applications via third-party application using a named account), and **Financial Close** (can use Account Reconciliation and Transaction Matching).
</details>

---

### Question 40 (Medium)
**201.7.1** | Difficulty: Medium

How does a user configure external authentication when creating a user in OneStream?

A) Set External Authentication Provider to the appropriate IdP and enter the user's email in the Internal Provider Password field
B) Set External Authentication Provider to the appropriate external IdP and enter the user name configured in the external identity provider in the External Provider User Name field
C) Set External Authentication Provider to Not Used and enter the IdP URL in the External Provider User Name field
D) External authentication is configured only at the server level, not per user

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For external authentication, select the appropriate **External Authentication Provider** from the dropdown (e.g., Azure AD), and enter the user name as configured in the external identity provider in the **External Provider User Name** field. This name must be unique and match the user name in the IdP. For native authentication, select (Not Used) and enter the password in Internal Provider Password.
</details>

---

### Question 41 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator notices that the IIS memory manager is executing at a high rate on a daily basis, as shown in the Error Log entries. The entries indicate that the Data Cache Memory Manager is frequently removing Data Units from the cache. What does this indicate, and what is the recommended solution?

A) The database server needs more disk space; increase storage allocation
B) The application servers require additional RAM resources to handle the analytic model and data volumes being processed
C) The consolidation servers are processing too many concurrent tasks; reduce the task queue
D) The application needs to be upgraded to a newer version of OneStream

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If the memory manager is executing at a high rate on a daily basis, this is an indication that the **servers require additional RAM resources** to handle the analytic model and data volumes being processed. The memory manager removes the oldest Data Units from the analytic cache to free up memory. A typical Error Log entry shows the before and after counts of Data Units and records in cache.
</details>

---

### Question 42 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator is tuning the performance of a Stage load process. The source dataset contains 2 million records, and the default Workflow Profile cache page size settings are in place (20,000 cache page size, 200 cache pages in memory limit). What should the administrator do?

A) Keep the default settings since they can handle up to 4 million records
B) Decrease the cache page size to 2,000 and increase pages in memory to 2,000 for the same total capacity
C) Increase the cache page size to 100,000 and keep cache pages in memory limit at 200, allowing for up to 20 million records in memory
D) Reduce the number of records per import by splitting the file into smaller batches

<details>
<summary>Show answer</summary>

**Correct answer: C)**

For large source datasets (2 million records), the cache page size should be adjusted to **100,000** and the cache pages in memory limit set to **200**, allowing for up to 20 million records in memory. If the cache page size is not large enough, the process will begin writing data records to temp files on disk, which is inefficient. Option B is wrong because setting a small cache page size (2,000) with many pages creates more pages to process and increases the risk of SQL Server deadlock issues.
</details>

---

### Question 43 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: A consultant reviews application performance reports and finds that the top-of-the-house consolidation entity has 400,000 total data records in a cube. A full-year consolidation is taking significantly longer than expected. According to OneStream best practices, what is the optimal data record threshold for the top entity, and what strategies should be considered?

A) 500,000 records is optimal; increase the number of consolidation servers
B) 250,000 total data records is optimal, equating to 3,000,000 data cells for the year; consider using extensibility to reduce Data Unit sizes, leverage the aggregation feature, and use hybrid scenarios for reporting
C) 100,000 records is optimal; reduce the number of entities in the hierarchy
D) 1,000,000 records is optimal; switch to binary data tables

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For a well-performing full-year consolidation, the top-of-the-house entity should have approximately **250,000 total data records** in a cube, equating to **3,000,000 total data cells** for the year. When data records exceed this point, consider using **extensibility** to reduce the size of Data Units, leveraging the **aggregation feature** for abnormally large data sets, and using **hybrid scenarios** for reporting performance issues related to large Data Units.
</details>

---

### Question 44 (Hard)
**201.7.1** | Difficulty: Hard

**Scenario**: An administrator wants to grant a group of non-Administrator users the ability to create and manage other users and groups, but not manage security roles. Which system security roles should be assigned to their group?

A) ManageSystemSecurityUsers and ManageSystemSecurityGroups only
B) ManageSystemSecurityUsers, ManageSystemSecurityGroups, and ManageSystemSecurityRoles
C) Administrator role only
D) ManageSystemSecurityRoles only

<details>
<summary>Show answer</summary>

**Correct answer: A)**

To let non-Administrators create and manage users and groups without managing roles, assign them **ManageSystemSecurityUsers** (create, modify, and manage users) and **ManageSystemSecurityGroups** (define, modify, and manage groups). **ManageSystemSecurityRoles** is not needed since the requirement excludes managing roles. The Administrator is always assigned to all roles regardless.
</details>

---

### Question 45 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: During a consolidation, an administrator notices the CPU utilization on the consolidation servers is only at 40-50%, while the database server is at 80-90%. The entity hierarchy has a deep structure with many intermediate parent entities in a one-to-one relationship with base entities. What is the performance issue, and how can it be addressed?

A) The consolidation servers are underpowered; upgrade their CPUs
B) The deep hierarchy with one-to-one entity relationships causes work to be done in a single-threaded manner at parent levels, shifting the load to the database server; redesign the entity hierarchy to reduce intermediate parent levels
C) The database server is undersized; add more storage
D) There are too many concurrent consolidation tasks; reduce the task queue

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A deep entity hierarchy with a **one-to-one relationship** between base and parent entities reverses the typical consolidation behavior. Extra work at parent levels is done in a **more single-threaded manner**, and the database server takes the brunt of writing data to all the parents. In a typical wide hierarchy, consolidation servers run at 90-95% and database at 40%, but this deep hierarchy reverses those roles. Reducing intermediate parent levels can improve performance.
</details>

---

### Question 46 (Medium)
**201.7.1** | Difficulty: Medium

What is the purpose of exclusion groups in OneStream security?

A) They exclude specific applications from user access
B) They grant almost everyone access except a particular group or small number of users by using Deny Access settings, and access is determined based on the exclusion order specified
C) They prevent administrators from modifying security settings
D) They automatically remove inactive users from the system

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Exclusion groups are used to grant almost everyone access **except a particular group or small number of users**. Members can be set to **Allow Access** or **Deny Access**. Access is determined based on the **exclusion order** specified, regardless of a user's membership in a group. For example, to ensure specific users cannot access artifacts, put their group first (Allow Access), then the individual users below (Deny Access).
</details>

---

### Question 47 (Easy)
**201.7.1** | Difficulty: Easy

Which of the following is true about the Administrator user in OneStream?

A) The Administrator can be disabled by other administrators
B) The Administrator cannot be disabled and is unaffected by inactivity thresholds
C) The Administrator must be re-authenticated every 30 days
D) The Administrator can only access System-level settings, not Application-level settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The **Administrator cannot be disabled** and is **unaffected by inactivity thresholds** that disable users who try to log in after a specific period of time elapses. The Administrator is assigned to all system security roles and can always manage application and system-wide users, groups, artifacts, data, and tools.
</details>

---

### Question 48 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator is configuring the Logon Inactivity Threshold to automatically disable users who have not logged in for a certain period. Where is this setting configured, and what steps are required to activate it?

A) In System > Security > Users for each individual user; no additional steps required
B) In the Server Configuration Utility under Authentication > Security, in the Logon Inactivity Threshold field; requires saving, resetting IIS, and then verifying in System > Security > Users
C) In System > Logging > Logon Activity; requires clearing the logon activity first
D) In the Application Server Configuration File under the Logging section; requires a database restart

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Logon Inactivity Threshold is configured in the **Server Configuration Utility** under **Authentication > Security**, in the **Logon Inactivity Threshold** field. After entering the number of days, you must click **Save**, then **reset IIS** in a command prompt. The setting can then be verified in **System > Security > Users** by selecting a user and reviewing their Logon Inactivity Threshold. Users who try to log in after the threshold expires receive an error.
</details>

---

### Question 49 (Medium)
**201.7.2** | Difficulty: Medium

What information is available in the Profiler Events window, and what does the Event Information button provide?

A) Only the event name and timestamp; no additional detail is available
B) Event Type, Workspace, Source, Method, Description, Entity, Cons, Duration, Server, and Thread ID; the Event Information button shows Method Inputs, Method Results, and Method Errors tabs
C) Only Business Rule names and their execution status
D) CPU utilization and memory consumption per event

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Profiler Events window displays columns including **Event Type** (Top, Queue, Adapter, Formula, BR, Factory, WSAS, Manual), **Workspace**, **Source**, **Method**, **Description**, **Entity**, **Cons**, **Duration**, **Server**, and **Thread ID**. The Event Information button provides tabs for **Method Inputs**, **Method Results**, and **Method Errors**, allowing detailed inspection of parameters passed and returned values.
</details>

---

### Question 50 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: A performance architect is analyzing a OneStream environment using the System Diagnostics (OSD) solution. They need to determine whether there are enough general application servers for the user community. What metric does the OSD solution use to calculate the recommended number of general application servers?

A) 10 concurrent users per CPU, with no deflators
B) 4.5 concurrent users per CPU, before including any deflators due to large Data Units and shared server roles
C) 2 concurrent users per server, based on total RAM available
D) 8 concurrent users per CPU, adjusted by database server capacity

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The OSD solution's Resources validation uses a calculation based on supporting **4.5 concurrent users per CPU** before including any deflators due to large Data Units and shared server roles. This generates a traffic light report that helps verify whether the environment has the appropriate number of general application servers and consolidation servers based on historical metrics.
</details>

---

### Question 51 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator is diagnosing database performance issues using the Environment page. What diagnostic capabilities are available in the Database Servers section?

A) Only connection status and configuration settings
B) SQL Deadlock information, Top SQL Commands ordered by Total Logical Reads/Writes/Worker Time, and schema-level table fragmentation reports
C) Only table row counts and database file sizes
D) Real-time query execution plans and index recommendations

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Database Servers Diagnostics tab allows running SQL diagnostic commands including **SQL Deadlock information** (listing any deadlocks on the SQL server instance) and **Top SQL Commands** ordered by Total Logical Reads, Total Logical Writes, or Total Worker Time. At the schema level, the Diagnostic tab shows a report of **current schema table fragmentation**. These tools help identify performance issues at the database level.
</details>

---

### Question 52 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator needs to deploy business rule changes to a production environment that is experiencing heavy user activity. What are the best practices for managing changes in this situation?

A) Apply the changes immediately since business rule updates are not disruptive
B) Avoid deploying changes during high load; schedule production changes during slow periods or non-work hours; consider using the Process Blocker solution to pause critical processes and allow current tasks to complete before applying changes
C) Disconnect all users first, then apply changes and restart all servers
D) Apply changes to a single server first and gradually roll them out

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Deploying changes to a production environment should be **avoided during times of high load**. Changes to business rules, confirmation rules, and metadata (especially Member Formulas) should not be performed during heavy activity. Standard environments should **schedule production changes during slow periods or non-work hours**. Large environments should consider using the **Process Blocker** Solution Exchange solution, which pauses critical processes and allows current tasks to complete before maintenance is applied.
</details>

---

### Question 53 (Medium)
**201.7.2** | Difficulty: Medium

What is the recommended frequency for IIS recycling on OneStream application servers, and why is it important?

A) Every 48 hours, to minimize service interruption
B) Every 24 hours, which is the default, to ensure good system memory health
C) Weekly, to reduce server restarts
D) Only when errors are detected in the Error Log

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A recycle of IIS is recommended **every 24 hours** for OneStream app servers, which is the **default** for customers. This is important for **good system memory health**, especially for active, global environments with data management sequences regularly being executed. It is key that servers get a chance to recycle for proper memory management.
</details>

---

### Question 54 (Hard)
**201.7.3** | Difficulty: Hard

**Scenario**: An administrator wants to enable slow formula logging to identify formulas that are impacting consolidation performance. Where is this setting configured, what is the recommended threshold, and what additional step is required?

A) In System > Logging > Settings, with a threshold of 10 seconds; no restart required
B) In the XFAppServerConfig.xml file under the multi-threading settings section, using the NumSecondsBeforeLoggingSlowFormulas setting; a formula running 3 to 5 seconds or more should be investigated; requires IISRESET on all application servers
C) In the Application > Tools > Profiler settings, with a threshold of 1 second; requires restarting the Profiler session
D) In the Cube properties under Performance settings, with a threshold of 60 seconds; requires reprocessing the cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The slow formula logging setting is located in the **XFAppServerConfig.xml** file under the **multi-threading settings section**, using the **NumSecondsBeforeLoggingSlowFormulas** setting. A formula running for **3 to 5 seconds or more** is considered very long and should be investigated. This setting writes slow formulas to the error log during consolidation. It requires an **IISRESET on all application servers** for the change to take effect.
</details>

---

---


# Question Bank - Section 8: Rules (16% of exam)

**THIS IS THE HIGHEST-WEIGHTED SECTION OF THE EXAM.**

## Objective 201.8.1: Demonstrate an understanding of the proper use case for various business rule types

### Question 1 (Easy)
**201.8.1** | Difficulty: Easy

How many Finance Business Rules can be assigned to a Cube at most?

A) 4
B) 6
C) 8
D) 16

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Up to **8 Finance Business Rules** can be assigned per Cube. These are interleaved with the 16 Formula Passes in the DUCS: Business Rules 1-2 execute before Formula Passes 1-4, BR 3-4 before passes 5-8, BR 5-6 before passes 9-12, and BR 7-8 before passes 13-16.
</details>

---

### Question 2 (Easy)
**201.8.1** | Difficulty: Easy

What type of Business Rule is the best starting point for people with no coding experience?

A) Finance Business Rule
B) Dashboard Extender Business Rule
C) Dashboard XFBR String Business Rule
D) Connector Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The **Dashboard XFBR String Business Rule** is the best starting point for people with no coding experience, as it simply **returns text based on logic** and does not require understanding Data Units, Data Buffers, or Data Explosion. It can be used in virtually any place in the application that expects text.
</details>

---

### Question 3 (Easy)
**201.8.1** | Difficulty: Easy

What type of Business Rule is used to parse debit/credit fields from a source GL file during an import?

A) Connector Business Rule
B) Derivative Business Rule
C) Parser Business Rule
D) Conditional Rule

<details>
<summary>Show answer</summary>

**Correct answer: C)**

**Parser Business Rules** are used to parse and transform incoming data during an import event. Common use cases include parsing debit/credit fields, character trimming, concatenation, and deriving a source ID from the source file name. They are configured in a Data Source Dimension.
</details>

---

### Question 4 (Easy)
**201.8.1** | Difficulty: Easy

What is the only type of Business Rule that does NOT need to be called from another artifact and triggers automatically?

A) Extender Business Rule
B) Finance Business Rule
C) Event Handler Business Rule
D) Dashboard Extender Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: C)**

**Event Handler Business Rules** are the **only type** of rule that does NOT need to be called from another artifact. They trigger automatically when a specific event occurs in the platform. There are 7 types: Transformation, Journal, Data Quality, Data Management, Forms, Workflow, and WCF.
</details>

---

### Question 5 (Easy)
**201.8.1** | Difficulty: Easy

Where is a Conditional Rule applied?

A) Directly to the Cube
B) To an individual Transformation Rule (composite, range, list, mask)
C) To a Data Management Step
D) To a Workflow Profile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Conditional Rules** are assigned to an **individual Transformation Rule** (composite, range, list, mask). They trigger when the transformation executes during an import event. It is important to remember that they are **very processing-intensive** and should be used with care.
</details>

---

### Question 6 (Easy)
**201.8.1** | Difficulty: Easy

What programming language is used for Member Formulas?

A) C# or VB.NET, depending on preference
B) VB.NET only
C) C# only
D) Python

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Member Formulas** require **VB.NET**. Business Rules can use VB.NET or C#, but Member Formulas only support VB.NET. In the case of Assemblies, the Compiler Language is defined at the Assembly level.
</details>

---

### Question 7 (Medium)
**201.8.1** | Difficulty: Medium

**Scenario:** An implementation team needs to automate GL data import from an external system, including the ability to drill back to detail in the source system. What type of Business Rule should they use?

A) Parser Business Rule
B) Extender Business Rule
C) Connector Business Rule
D) Dashboard Dataset Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: C)**

**Connector Business Rules** facilitate integrations to extract data from external databases, data warehouses, or OneStream auxiliary tables into a Workflow, and also enable **drill back** to detail data. The 4 ConnectorActionTypes are: GetFieldList, GetData, GetDrillbackTypes, and GetDrillBack. They are assigned directly to a Connector Data Source.
</details>

---

### Question 8 (Medium)
**201.8.1** | Difficulty: Medium

What are the two main functions of a Derivative Business Rule?

A) Parse data and transform dimensions
B) Derive/add records to Stage and enable data validation check rules
C) Connect to external systems and export data
D) Format reports and create dashboards

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Derivative Business Rules have two main functions: 1) **Derive or add a record** to Stage and calculate its amount (interim for validations, final to transform and load to the Cube), and 2) **Enable check rules** for data validation (pass/fail) that execute during the Validate step of the Workflow. They are assigned to a Derivative Transformation Rule.
</details>

---

### Question 9 (Medium)
**201.8.1** | Difficulty: Medium

**Scenario:** A user needs to create a button on a Dashboard that, when clicked, sends an email and executes a Workflow process. What type of Business Rule and Function Type should they use?

A) Dashboard Dataset Business Rule with LoadDashboard
B) Dashboard Extender Business Rule with ComponentSelectionChanged
C) Dashboard XFBR String Business Rule with LoadDashboard
D) Extender Business Rule with ExecuteProcess

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Dashboard Extender Business Rules** create interactive dashboards with click actions. The Function Type **ComponentSelectionChanged** triggers when a component is clicked or selected (button, combo box). The other Function Types are LoadDashboard (when the dashboard renders) and SQLTableEditorSaveData (when saving in a SQL Table Editor).
</details>

---

### Question 10 (Medium)
**201.8.1** | Difficulty: Medium

What is the main difference between an Assembly Business Rule and an Assembly Service?

A) Assembly Business Rules support Finance logic; Assembly Services do not
B) Assembly Business Rules are a 1-to-1 transition from traditional rules and do NOT support Finance Business Rules; Assembly Services support Finance logic, Dynamic Dashboards, and Dynamic Cubes
C) There is no difference; they are two names for the same thing
D) Assembly Services are only for C#; Assembly Business Rules are only for VB.NET

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Assembly Business Rules** are a direct (1-to-1) transition from traditional Business Rules to the same code but in a different location (Workspace > Maintenance Unit > Assembly), and they **do NOT support Finance Business Rules or Dynamic content**. **Assembly Services** are more advanced: they support Finance logic, Dynamic Dashboards, Dynamic Cubes, and use Service Factory for routing. OneStream recommends Assembly Services for sophisticated logic.
</details>

---

### Question 11 (Medium)
**201.8.1** | Difficulty: Medium

**Scenario:** A developer needs to create a custom PDF report from a Cube View with a company logo, page numbers, and special font formatting. What type of Business Rule should they use?

A) Dashboard XFBR String Business Rule
B) Cube View Extender Business Rule
C) Dashboard Extender Business Rule
D) Finance Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Cube View Extender Business Rules** are used to customize and format PDF reports from Cube Views (logo, page number, title, header font, word wrapping, font color, cell value, etc.). It is **IMPORTANT** to remember that the logic ONLY applies when the Cube View is run as a **PDF report**, NOT in Data Explorer grid mode.
</details>

---

### Question 12 (Medium)
**201.8.1** | Difficulty: Medium

What are the two types of Business Rules that can be called directly from a Data Management step?

A) Finance Business Rules and Parser Business Rules
B) Extender Business Rules and Finance Business Rules (via Custom Calculate)
C) Connector Business Rules and Dashboard Dataset Business Rules
D) Event Handler Business Rules and Conditional Rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The two types of Business Rules that can be called directly from a **Data Management step** are: **Extender Business Rules** (for automated tasks such as GL import, FTP, CSV export) and **Finance Business Rules** with Function Type **Custom Calculate** (which executes exclusively via a DM step of type Custom Calculate).
</details>

---

### Question 13 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** A consultant is designing a consolidation solution. The client needs: (1) retained earnings calculations per entity, (2) driver-based planning for 50 accounts, (3) automatic validation that the trial balance is balanced during import, (4) email notification when a Workflow is certified, and (5) a report with data from an auxiliary SQL table. For each need, identify the CORRECT combination of rule types.

A) (1) Member Formula, (2) Finance BR, (3) Derivative BR, (4) Event Handler BR, (5) Dashboard Dataset BR
B) (1) Finance BR, (2) Member Formula, (3) Parser BR, (4) Extender BR, (5) Connector BR
C) (1) Member Formula, (2) Finance BR, (3) Conditional Rule, (4) Dashboard Extender BR, (5) XFBR String BR
D) (1) Finance BR, (2) Finance BR, (3) Derivative BR, (4) Extender BR, (5) Dashboard Dataset BR

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The correct combination is: (1) **Member Formula** - retained earnings are member-specific calculations, ideal for Member Formulas in consolidation. (2) **Finance Business Rule** - driver-based planning spanning large groups of members benefits from Business Rules. (3) **Derivative Business Rule** - enables check rules for validation (pass/fail) during the Validate step of the Workflow. (4) **Event Handler Business Rule** - triggers automatically when an event such as Workflow certification occurs. (5) **Dashboard Dataset Business Rule** - can execute SQL queries to create custom datasets for reports.
</details>

---

### Question 14 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** A development team is migrating traditional Business Rules to Assemblies. What is the correct invocation reference for an Assembly Business Rule called "CalcRevenue" in the Assembly "FinanceCalcs" of the Workspace "CoreFinance"?

A) Assembly:FinanceCalcs/CalcRevenue
B) WS:CoreFinance/Assembly:FinanceCalcs/CalcRevenue
C) CoreFinance.FinanceCalcs.CalcRevenue
D) BR:CalcRevenue/WS:CoreFinance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The invocation reference for Assembly Business Rules follows the format: **WS:WorkspaceName/Assembly:AssemblyName/BRName**, which in this case would be **WS:CoreFinance/Assembly:FinanceCalcs/CalcRevenue**. The three forms of reference are: Traditional (`BRName` for the central repository), Assembly Business Rule (`WS:WorkspaceName/Assembly:AssemblyName/BRName`), and Assembly Service (via Service Factory routing).
</details>

---

### Question 15 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** An administrator needs to connect OneStream Cloud with a database on the client's internal network without using a VPN. Additionally, they need to read/write data in database tables directly from a Spreadsheet. What two types of Business Rules do they need?

A) Connector Business Rule and Dashboard Dataset Business Rule
B) Smart Integration Function Rule and Spreadsheet Business Rule
C) Extender Business Rule and Finance Business Rule
D) Event Handler Business Rule and Parser Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To connect OneStream Cloud with data sources on the client's network **without VPN**, a **Smart Integration Function Rule** is used (which executes remote functions via Smart Integration Connector). To read/write in database tables within the Spreadsheet tool, a **Spreadsheet Business Rule** is used (which is created directly in a Spreadsheet file via a Table View Definition).
</details>

---

## Objective 201.8.2: Demonstrate an understanding of Finance Engine basics

### Question 16 (Easy)
**201.8.2** | Difficulty: Easy

Where does the Finance Function Type Calculate execute within the DUCS?

A) In C#Translated
B) In C#Share
C) In C#Local
D) In C#Aggregated

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Finance Function Type **Calculate** executes in **C#Local** during the Calculate Local phase of the DUCS (Data Unit Calculation Sequence). C#Translated is for Translate, C#Share is for ConsolidateShare, and C#Aggregated is a faster alternative that skips IC eliminations.
</details>

---

### Question 17 (Easy)
**201.8.2** | Difficulty: Easy

What Finance Function Types do Member Formulas support?

A) Calculate, Custom Calculate, and DynamicCalc
B) Only Calculate and DynamicCalc
C) All Finance Function Types
D) Only Calculate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Member Formulas only support two Finance Function Types: **Calculate** (Stored Formula, executes during the DUCS) and **DynamicCalc** (calculated on-the-fly when referenced in a report). The remaining Function Types, including Custom Calculate, are only available in Finance Business Rules.
</details>

---

### Question 18 (Easy)
**201.8.2** | Difficulty: Easy

What type of data must a Dynamic Calculation return?

A) Only numeric values
B) Only text
C) An object (numeric or textual) using the Return statement
D) A Data Buffer

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Dynamic Calculations require a **Return** statement followed by an **object** that can be numeric or textual. They work cell-by-cell (like Excel), NOT with Data Buffers, and inherit the POV of each report cell (18 dimensions available via `api.Pov`).
</details>

---

### Question 19 (Easy)
**201.8.2** | Difficulty: Easy

What percentage of performance improvement can C#Aggregated offer compared to a normal Consolidation?

A) Up to 25% faster
B) Up to 50% faster
C) Up to 75% faster
D) Up to 90% faster

<details>
<summary>Show answer</summary>

**Correct answer: D)**

**C#Aggregated** can be up to **90% faster** than a normal Consolidation. This is because it only executes the DUCS at Base Entities, does not perform Intercompany eliminations, does not calculate Share/Ownership, and only executes Direct Method Translation. It is used mainly in Planning solutions.
</details>

---

### Question 20 (Easy)
**201.8.2** | Difficulty: Easy

What function is recommended to ALWAYS use as standard practice in all calculations with Data Buffers?

A) RemoveNulls
B) RemoveZeros
C) ClearEmptyCells
D) TrimDataBuffer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**RemoveZeros** should be used in **ALL calculations** as standard practice. It removes cells with amount 0 AND cells with Cell Status NoData. It only works with `api.Data.GetDataBufferUsingFormula`, not with `api.Data.GetDataBuffer`. There is also RemoveNoData which only removes cells with Cell Status NoData.
</details>

---

### Question 21 (Medium)
**201.8.2** | Difficulty: Medium

**Scenario:** A developer needs to execute a calculation that only affects a specific subset of accounts and does not depend on Calculation Status. The calculation must be executed on demand from a Dashboard button. What Finance Function Type should they use?

A) Calculate
B) DynamicCalc
C) Custom Calculate
D) OnDemand

<details>
<summary>Show answer</summary>

**Correct answer: C)**

**Custom Calculate** is the correct Function Type because: it executes **outside the DUCS** (only via a Data Management step), does **NOT consider Calculation Status**, only processes explicitly defined Data Units, and can be linked to **Dashboards** with buttons that execute DM Sequences with user parameters. The calculated data must be marked as `isDurable = True` so it is not cleared by the DUCS.
</details>

---

### Question 22 (Medium)
**201.8.2** | Difficulty: Medium

In the DUCS, what is the correct execution order of formulas within a Formula Pass?

A) UD1 > UD2 > ... > UD8 > Flow > Account
B) Account > Flow > UD1 > UD2 > ... > UD8
C) Flow > Account > UD1 > UD2 > ... > UD8
D) The order is random within a pass

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Within a Formula Pass in the DUCS, the execution order is: **Account formulas** first, then **Flow formulas**, then **UD1**, **UD2**, and so on through **UD8**. Additionally, members in the same pass and same dimension are **multithreaded** (run in parallel).
</details>

---

### Question 23 (Medium)
**201.8.2** | Difficulty: Medium

What are the three mandatory requirements of a Custom Calculate regarding data handling?

A) Use api.Data.GetDataBuffer, mark isDurable = False, clear at the end of the script
B) Mark data as isDurable = True, include api.Data.ClearCalculatedData at the start, and define Business Rule Name and Function Name in the DM Step
C) Use only Formula Variables, never use Dimension Filters, and execute within the DUCS
D) Clear cells manually, do not use RemoveZeros, and execute in C#Local

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three mandatory requirements of Custom Calculate are: 1) Calculated data must be marked as **isDurable = True** so it is NOT cleared by the DUCS. 2) **ALWAYS include api.Data.ClearCalculatedData** at the start of the script (it is not automatically cleared as in the DUCS). 3) Define the **Business Rule Name** and **Function Name** in the Data Management Step of type Custom Calculate.
</details>

---

### Question 24 (Medium)
**201.8.2** | Difficulty: Medium

**Scenario:** A developer needs to operate on two Data Buffers where the first has Common Members {Account, Flow} and the second has Common Members {Account, Flow, UD1}. What should they do?

A) Operate directly with api.Data.Calculate; OneStream handles it automatically
B) Use an Unbalanced function (e.g., MultiplyUnbalanced), placing the Data Buffer with MORE dimensions as the second argument
C) Convert both Data Buffers to the same format first
D) Create a third intermediate Data Buffer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When Data Buffers have different Common Members they are **Unbalanced** and OneStream throws an error if you attempt to operate on them directly. **Unbalanced** functions must be used (MultiplyUnbalanced, DivideUnbalanced, AddUnbalanced, SubtractUnbalanced) with the rule that the Data Buffer with **MORE dimensions** must always be the **second argument**.
</details>

---

### Question 25 (Medium)
**201.8.2** | Difficulty: Medium

What are the three locations where Dynamic Calculations can be placed?

A) Cube View, Dashboard, Data Management Step
B) Member Formula (Formula Type = DynamicCalc), Business Rule (Function Type = DataCell), directly in the Cube View (GetDataCell)
C) Finance Rule, Extender Rule, Connector Rule
D) Account dimension, Entity dimension, Scenario dimension

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three locations for Dynamic Calculations are: 1) **Member Formula** with Formula Type = DynamicCalc (executes whenever the member is referenced). 2) **Business Rule** with Finance Function Type = DataCell (called from Cube View with syntax `GetDataCell(BR#[...])`). 3) **Directly in the Cube View** using `GetDataCell(...)` in rows/columns, although this option is not reusable in other Cube Views.
</details>

---

### Question 26 (Medium)
**201.8.2** | Difficulty: Medium

**Scenario:** A developer has a calculation where they need to reuse the same Data Buffer in multiple api.Data.Calculate functions. What technique should they use to improve performance?

A) Copy the Data Buffer in each function
B) Use Formula Variables with api.Data.FormulaVariables.SetDataBufferVariable and reference with $VariableName
C) Execute the calculation in a loop
D) Store the Data Buffer in a global application variable

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Formula Variables** should be used by declaring the Data Buffer with `api.Data.FormulaVariables.SetDataBufferVariable` and referencing it in the formula string with **$VariableName**. The last argument True for "Use Indexes To Optimize Repeat Filtering" if reusing. This improves performance by loading the Data Buffer into memory once instead of querying it repeatedly.
</details>

---

### Question 27 (Medium)
**201.8.2** | Difficulty: Medium

What is the general rule for deciding between DUCS and Custom Calculate based on the type of solution?

A) DUCS for Planning, Custom Calculate for Consolidation
B) DUCS for Consolidation, Custom Calculate + C#Aggregated for Planning
C) DUCS for both; Custom Calculate only for reports
D) Custom Calculate for both; DUCS is obsolete

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The general rule is: **Consolidation = DUCS** (cascading dependencies, data integrity, clear & replace) and **Planning = Custom Calculate + C#Aggregated** (iterative, multiple users, selective calculation). C#Aggregated offers up to 90% better performance by skipping IC eliminations and ownership calculations, which are unnecessary in Planning.
</details>

---

### Question 28 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer runs an api.Data.Calculate inside a For Each loop iterating over 200 entities. The process takes 45 minutes. What performance error are they making and what are the correct alternatives?

A) They should use more Formula Passes; there is no problem with the loop
B) api.Data.Calculate inside loops causes multiple reads/writes to the database; they should use ADC with Dimension Filters to process all entities in a single operation, or use Data Buffer Cell Loop (DBCL)
C) They should increase the server timeout; the code is correct
D) They should split the loop into groups of 50 entities each

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Using **api.Data.Calculate inside loops** is one of the worst performance practices in OneStream because it causes **multiple reads/writes to the database** in each iteration. The correct alternatives are: use **ADC with Dimension Filters** to process all entities in a single operation, or use **Data Buffer Cell Loop (DBCL)** with the "wheelbarrow method" (accumulate in a Result Data Buffer and write once at the end). You should also never use api.Data.SetCell/SetDataCell or api.Data.ClearCalculatedData inside loops.
</details>

---

### Question 29 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer needs to compare Actual vs Budget data to identify new accounts that exist in Actual but not in Budget. They need to evaluate individual cells of TWO Data Buffers within an api.Data.Calculate. What technique should they use?

A) Eval with OnEvalDataBuffer
B) Eval2 with OnEvalDataBuffer2
C) Data Buffer Cell Loop with double For Each
D) Formula Variables with two $Variables

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Eval2** with the subfunction **OnEvalDataBuffer2** allows evaluating and comparing cells of **TWO Data Buffers** within an api.Data.Calculate. EventArgs provides access to the Data Buffer, DestinationInfo, and ResultDataBuffer. Eval (without the 2) only evaluates cells of ONE Data Buffer. This classic Eval2 use case allows identifying new products/accounts by comparing two scenarios.
</details>

---

### Question 30 (Hard)
**201.8.2** | Difficulty: Hard

**Advanced Scenario:** A developer is building a consolidation solution with the following requirements:
- Current Year Net Income calculation that takes NetIncome from YTD
- Retained Earnings Beginning Balance that copies the ending balance from the prior year only in the first period
- Activity calculation that shows the YTD change from the beginning balance
- Dynamic Beginning Balance that shows the correct balance based on the View Member (MTD, QTD, YTD)

What is the correct assignment of Formula Passes and calculation types for each?

A) All in Formula Pass 1 as Stored Formulas
B) Retained Earnings BegBal in Pass 1, Current Year Net Income in Pass 2 (both Stored, Is Consolidated = True), Activity as Stored, Beginning Balance dynamic as DynamicCalc
C) All as Custom Calculate in Data Management
D) All as Dynamic Calculations for maximum performance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct assignment is: **Retained Earnings Beginning Balance** in **Formula Pass 1** (must execute first, copies ending balance from the prior year using `api.Time.IsFirstPeriodInYear`). **Current Year Net Income** in **Formula Pass 2** with **Is Consolidated = True** and Allow Input = False (formula: `A#CurYearNetIncome:O#Import:F#EndBal = V#YTD:A#NetIncome:O#Top:F#EndBal`). **Activity (ActivityCalc)** as a Stored calculation with Aggregation Weight = 0. **BegBalDynamic** as **DynamicCalc** that evaluates the View Member (MTD, QTD, YTD) to show the correct balance. Formula Passes respect dependencies: Pass 1 first (BegBal), then Pass 2 (which may depend on BegBal).
</details>

---

### Question 31 (Hard) - BONUS
**201.8.2** | Difficulty: Hard

**Performance Scenario:** A consultant is reviewing the code of an existing solution and finds the following problems. What are the 5 best practice violations?

```
For Each entity In entityList
    api.Data.Calculate("A#Result = A#Source1 * A#Source2")
    api.Data.SetDataCell(...)
    api.Data.ClearCalculatedData(...)
    Dim rate = BRApi.Finance.Data.GetDataCell(...)
    api.LogMessage("Processing " & entity.Name)
Next
```

A) There are only 2 violations: the loop is unnecessary and RemoveZeros is missing
B) 5 violations: (1) ADC inside the loop, (2) SetDataCell inside the loop, (3) ClearCalculatedData inside the loop, (4) BRApi call in Finance Rule, (5) LogMessage not commented out for production
C) 3 violations: ADC in loop, SetDataCell in loop, missing error handling
D) 4 violations: ADC in loop, incorrect data type, missing Global Variables, uncommented code

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The 5 best practice violations are: (1) **api.Data.Calculate inside the loop** - causes multiple reads/writes to the database; use ADC with filters or DBCL instead. (2) **api.Data.SetDataCell inside the loop** - use the "wheelbarrow method" (accumulate and write once outside the loop). (3) **api.Data.ClearCalculatedData inside the loop** - must be executed BEFORE the loop, only once. (4) **BRApi call in Finance Rule** - opens new database connections causing overload in multi-threading; use API equivalents instead. (5) **LogMessage not commented out** - must be commented out or removed for production as it can impact servers.
</details>

---

### Question 32 (Hard) - BONUS
**201.8.2** | Difficulty: Hard

How many times can the DUCS execute per Entity during a full Consolidation, and why?

A) 1 time - only C#Local
B) 3 times - C#Local, C#Translated, C#Aggregated
C) Up to 7 times - because it includes Local, Translated, Share, Elimination, and other Consolidation Members
D) 16 times - once per Formula Pass

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The DUCS can execute up to **7 times** per Entity during a Consolidation. This occurs because each Consolidation Member (Local, Translated, Share, Elimination, etc.) requires its own execution of the DUCS. Each execution includes all steps: Clear, Scenario Formula, Reverse Translations, Business Rules 1-8 interleaved with Formula Passes 1-16. This is why Consolidation can be expensive and C#Aggregated (which only executes at Base Entities) offers up to 90% improvement.
</details>

---

## NEW QUESTIONS (33-96)

## Objective 201.8.1: Business Rule Types (continued)

### Question 33 (Easy)
**201.8.1** | Difficulty: Easy

How many total Business Rule types does OneStream support?

A) 6
B) 8
C) 10
D) 12

<details>
<summary>Show answer</summary>

**Correct answer: D)**

OneStream supports **12 Business Rule types**: (1) Finance, (2) Parser, (3) Connector, (4) Smart Integration Function, (5) Conditional Rule, (6) Derivative Rule, (7) Cube View Extender, (8) Dashboard Dataset, (9) Dashboard Extender, (10) Dashboard XFBR String, (11) Extensibility (Extender + Event Handler), and (12) Spreadsheet.
</details>

---

### Question 34 (Easy)
**201.8.1** | Difficulty: Easy

How many subtypes of Event Handler Business Rules exist in OneStream?

A) 3
B) 5
C) 7
D) 10

<details>
<summary>Show answer</summary>

**Correct answer: C)**

There are **7 subtypes** of Event Handler Business Rules: **Transformation**, **Journal**, **Data Quality**, **Data Management**, **Forms**, **Workflow**, and **WCF**. Each subtype triggers automatically when its corresponding event occurs in the platform.
</details>

---

### Question 35 (Easy)
**201.8.1** | Difficulty: Easy

What type of Business Rule is used to create custom datasets for Dashboard reports using SQL queries or programmatic logic?

A) Dashboard Extender Business Rule
B) Dashboard Dataset Business Rule
C) Dashboard XFBR String Business Rule
D) Cube View Extender Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Dashboard Dataset Business Rules** are used to create custom datasets for Dashboard reports. They can execute SQL queries or programmatic logic to produce data tables that serve as data sources for Dashboard components such as grids, charts, and graphs.
</details>

---

### Question 36 (Easy)
**201.8.1** | Difficulty: Easy

Which Business Rule type falls under the "Extensibility" category alongside Event Handlers?

A) Finance Business Rule
B) Extender Business Rule
C) Parser Business Rule
D) Connector Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The **Extensibility** Business Rule type (type 11) includes both **Extender Business Rules** and **Event Handler Business Rules**. Extender BRs are used for custom automated tasks (GL import, FTP, CSV export) and are called from Data Management steps, while Event Handlers trigger automatically based on platform events.
</details>

---

### Question 37 (Easy)
**201.8.1** | Difficulty: Easy

Where is the compiler language (VB.NET or C#) defined for files within an Assembly?

A) At the individual file level
B) At the Assembly level
C) At the Workspace level
D) At the Application level

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The compiler language is set at the **Assembly level**. All files within a single Assembly share the same language — either VB.NET or C#. You cannot mix languages within one Assembly.
</details>

---

### Question 38 (Medium)
**201.8.1** | Difficulty: Medium

What are the four ConnectorActionTypes available in a Connector Business Rule?

A) Connect, Import, Export, Close
B) GetFieldList, GetData, GetDrillbackTypes, GetDrillBack
C) OpenConnection, ReadData, WriteData, CloseConnection
D) Initialize, Transform, Load, Validate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The four ConnectorActionTypes in a Connector Business Rule are: **GetFieldList** (returns the list of fields available in the data source), **GetData** (extracts the actual data), **GetDrillbackTypes** (defines the available drill back report types), and **GetDrillBack** (executes the drill back to source detail). Connector BRs are assigned directly to a Connector Data Source.
</details>

---

### Question 39 (Medium)
**201.8.1** | Difficulty: Medium

What are the three Function Types available in a Dashboard Extender Business Rule?

A) LoadDashboard, RefreshDashboard, SaveDashboard
B) LoadDashboard, ComponentSelectionChanged, SQLTableEditorSaveData
C) Initialize, Execute, Finalize
D) OnLoad, OnClick, OnSave

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three Function Types in a Dashboard Extender Business Rule are: **LoadDashboard** (fires when the dashboard renders), **ComponentSelectionChanged** (fires when a user clicks or selects a component such as a button or combo box), and **SQLTableEditorSaveData** (fires when data is saved in a SQL Table Editor component).
</details>

---

### Question 40 (Medium)
**201.8.1** | Difficulty: Medium

**Scenario:** A team needs to automate a nightly process that downloads files via FTP from an external server, transforms the data, and loads it into a Workflow. What type of Business Rule should they use?

A) Connector Business Rule
B) Extender Business Rule
C) Parser Business Rule
D) Event Handler Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Extender Business Rules** are the correct choice for custom automated tasks like FTP file downloads, GL imports, CSV exports, and other complex operations. They are called from Data Management steps and can contain any arbitrary logic. In contrast, Connector BRs are specifically for data extraction with drill back capability, and Parser BRs are for transforming fields during import.
</details>

---

### Question 41 (Medium)
**201.8.1** | Difficulty: Medium

Which of the following is NOT one of the 6 Assembly Business Rule types supported by OneStream?

A) Dashboard Dataset
B) Finance
C) Connector
D) Parser

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Finance** is NOT one of the 6 Assembly Business Rule types. The 6 supported Assembly BR types are: Dashboard DataSet, Dashboard Extender, Dashboard XFBR String, Connector, Parser, and Cube View Extender. Finance Business Rules and Event Handlers are NOT supported as Assembly Business Rules — Finance logic requires Assembly Services instead.
</details>

---

### Question 42 (Medium)
**201.8.1** | Difficulty: Medium

When should a developer use a Member Formula instead of a Finance Business Rule for calculations?

A) When the calculation spans hundreds of accounts using driver-based logic
B) When the calculation is specific to individual members and can vary by Scenario Type and Time without code changes
C) When the calculation needs Custom Calculate functionality
D) When the calculation requires C# language

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Member Formulas** are best when the calculation is specific to individual members (like Retained Earnings Beginning Balance or Current Year Net Income) and when you want to **vary behavior by Scenario Type and Time without code changes** — this is configured through member properties. Finance Business Rules are better for large-scale calculations spanning many accounts. Member Formulas also do NOT support Custom Calculate and only use VB.NET.
</details>

---

### Question 43 (Medium)
**201.8.1** | Difficulty: Medium

**Scenario:** A developer wants to validate that source data loaded via a Transformation Rule meets certain conditions (e.g., the amount must be positive for a specific account range). What type of Business Rule should they attach to the Transformation Rule?

A) Parser Business Rule
B) Derivative Business Rule
C) Conditional Rule
D) Event Handler Business Rule

<details>
<summary>Show answer</summary>

**Correct answer: C)**

A **Conditional Rule** is assigned to an individual Transformation Rule (composite, range, list, mask) to apply conditional logic during data import. It evaluates conditions on the incoming data and can modify or filter records. However, Conditional Rules are **very processing-intensive** and should be used carefully. Note that Derivative BRs handle validation via check rules at the Validate step, while Conditional Rules act during the transformation itself.
</details>

---

### Question 44 (Medium)
**201.8.1** | Difficulty: Medium

What is the key difference between a Dashboard Dataset BR and a Dashboard XFBR String BR?

A) Dataset BRs return text; XFBR String BRs return data tables
B) Dataset BRs return data tables/datasets for grids and charts; XFBR String BRs return text strings for use anywhere text is expected
C) They are functionally identical
D) Dataset BRs work only with SQL; XFBR String BRs work only with VB.NET

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Dashboard Dataset BRs** return **data tables/datasets** that serve as data sources for Dashboard components like grids, charts, and graphs — they produce structured tabular data. **Dashboard XFBR String BRs** return **text strings** based on logic and can be used in virtually any place in the application that expects text. XFBR String BRs are the simplest BR type and the best starting point for non-coders.
</details>

---

### Question 45 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** A company is implementing OneStream and has the following requirements for their data integration pipeline: (a) Parse and reformat account codes from the source ERP file, (b) Apply conditional logic to filter out inactive entities during transformation, (c) Derive a balancing record to ensure debits equal credits in Stage, (d) Automatically send a notification when the data import step completes, and (e) Connect to an on-premises SQL Server database for the source data. Which combination of Business Rule types is correct?

A) (a) Parser BR, (b) Conditional Rule, (c) Derivative BR, (d) Event Handler BR (Data Management type), (e) Connector BR
B) (a) Conditional Rule, (b) Parser BR, (c) Finance BR, (d) Extender BR, (e) Smart Integration Function
C) (a) Parser BR, (b) Derivative BR, (c) Conditional Rule, (d) Dashboard Extender BR, (e) Connector BR
D) (a) Extender BR, (b) Event Handler BR, (c) Parser BR, (d) Workflow Event Handler, (e) Spreadsheet BR

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The correct combination is: (a) **Parser BR** — used to parse and reformat incoming data fields during import (e.g., reformatting account codes, trimming characters). (b) **Conditional Rule** — applied to Transformation Rules to add conditional logic that can filter records during transformation. (c) **Derivative BR** — derives/adds records to Stage (such as balancing entries) and enables check rules for validation. (d) **Event Handler BR (Data Management type)** — one of the 7 Event Handler subtypes, triggers automatically when a Data Management event occurs. (e) **Connector BR** — facilitates data extraction from external databases including SQL Server, with drill back capability.
</details>

---

### Question 46 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** A development team is migrating their solution to use Assemblies. They have the following traditional Business Rules: a Finance BR for consolidation calculations, a Dashboard Extender BR for interactive reports, an Event Handler BR for workflow notifications, and a Parser BR for data import formatting. Which of these can be migrated to Assembly Business Rules?

A) All four can be migrated to Assembly Business Rules
B) Only the Dashboard Extender BR and Parser BR can be migrated to Assembly Business Rules
C) Only the Finance BR and Event Handler BR can be migrated to Assembly Business Rules
D) None of them can be migrated to Assembly Business Rules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Only the **Dashboard Extender BR** and **Parser BR** can be migrated to Assembly Business Rules. The 6 supported Assembly BR types are: Dashboard DataSet, Dashboard Extender, Dashboard XFBR String, Connector, Parser, and Cube View Extender. **Finance BRs** and **Event Handler BRs** are NOT supported as Assembly Business Rules. Finance logic must use Assembly Services instead, and Event Handlers must remain as traditional Business Rules.
</details>

---

### Question 47 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** A consultant discovers that a client's Event Handler Business Rule for Workflow certification notifications is not triggering. The BR code is correct but nothing happens when users certify their entities. What is the most likely cause?

A) The Event Handler BR needs to be assigned to a Data Management step
B) The Event Handler BR is configured as the wrong subtype — it should be a Workflow type Event Handler, not a Data Management type
C) Event Handler BRs need to be manually called via a button click
D) The Event Handler BR needs a Connector Data Source to function

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most likely cause is that the Event Handler BR is configured as the **wrong subtype**. There are 7 Event Handler subtypes (Transformation, Journal, Data Quality, Data Management, Forms, Workflow, WCF), and each one only triggers for its specific event type. For Workflow certification notifications, the BR must be a **Workflow** type Event Handler. A Data Management type Event Handler would only trigger during Data Management events, not Workflow events. Event Handlers are the only BR type that triggers automatically — they do NOT need to be called from another artifact.
</details>

---

### Question 48 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** A company needs to build a Dashboard that displays a dynamic message showing the current user's name and role, a grid with custom KPI data from an auxiliary SQL table, and a button that triggers a data refresh process. Which three Business Rule types are needed?

A) Dashboard XFBR String BR for the message, Dashboard Dataset BR for the SQL grid, Dashboard Extender BR for the button action
B) Dashboard Extender BR for all three
C) Dashboard Dataset BR for the message and grid, Extender BR for the button
D) Finance BR for the KPIs, Dashboard Extender BR for the message, Event Handler BR for the button

<details>
<summary>Show answer</summary>

**Correct answer: A)**

The three BRs needed are: **Dashboard XFBR String BR** for the dynamic text message (returns text based on logic — ideal for displaying user name and role). **Dashboard Dataset BR** for the SQL grid (creates custom datasets from SQL queries for grids and charts). **Dashboard Extender BR** with Function Type ComponentSelectionChanged for the button action (triggers when a component is clicked to execute processes like data refresh).
</details>

---

### Question 49 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** An architect is designing the Assembly structure for a new implementation. They need to decide between Assembly Business Rules and Assembly Services. The solution requires: Finance calculations for consolidation, a dynamic dashboard that changes layout based on user security, and a Connector for an external CRM system. What is the correct approach?

A) Use Assembly Business Rules for all three requirements
B) Use Assembly Services for Finance calculations and Dynamic Dashboard; use Assembly Business Rules for the Connector
C) Use Assembly Services for all three requirements
D) Use traditional Business Rules for Finance, Assembly Business Rules for the Dashboard, and Assembly Services for the Connector

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach is: **Assembly Services** for Finance calculations (Assembly BRs do NOT support Finance BRs) and for the Dynamic Dashboard (Assembly BRs do NOT support dynamic content — Assembly Services support Dynamic Dashboards and Dynamic Cubes via Service Factory routing). **Assembly Business Rules** for the Connector (Connector is one of the 6 supported Assembly BR types: Dashboard DataSet, Dashboard Extender, Dashboard XFBR String, Connector, Parser, Cube View Extender).
</details>

---

### Question 50 (Hard)
**201.8.1** | Difficulty: Hard

**Scenario:** A team is troubleshooting dependency issues in their Assembly-based solution. Assembly "CoreCalcs" in Workspace "Finance" references classes from Assembly "SharedUtils" in the same Workspace. What must be configured for this to work?

A) Nothing special — Assemblies in the same Workspace automatically see each other
B) A dependency must be declared so that "CoreCalcs" references "SharedUtils", and both must use the same compiler language
C) The classes must be duplicated in both Assemblies
D) Assembly dependencies are not supported; all code must be in a single Assembly

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Assembly **dependencies** must be explicitly declared for one Assembly to reference another. The referencing Assembly ("CoreCalcs") must declare a dependency on "SharedUtils". Additionally, since the compiler language is set at the Assembly level and all files within an Assembly share the same language, both Assemblies must use the same compiler language for cross-referencing to work properly. Dependencies enable code reuse across Assemblies within a Workspace.
</details>

---

## Objective 201.8.2: Finance Engine Basics (continued)

### Question 51 (Easy)
**201.8.2** | Difficulty: Easy

What five dimensions define a Data Unit in OneStream?

A) Account, Flow, UD1, UD2, UD3
B) Cube, Scenario, Time, Consolidation, Entity
C) Entity, Account, Flow, Origin, UD1
D) Scenario, Entity, Account, Time, View

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A **Data Unit** is defined by five dimensions: **Cube + Scenario + Time + Consolidation + Entity** (plus Parent for consolidated entities). These are the "Data Unit dimensions" and they determine the scope of data processed during calculations. All data within a Data Unit shares these same five dimensional coordinates.
</details>

---

### Question 52 (Easy)
**201.8.2** | Difficulty: Easy

In what View dimension member is data always stored in OneStream?

A) Periodic
B) MTD
C) YTD
D) QTD

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Data is always stored at **YTD** (Year-To-Date) in OneStream. Other numeric View Members (Periodic, QTD, MTD, HTD) are derived dynamically from the stored YTD values. Text View Members include Annotation, Assumptions, AuditComment, Footnote, and VarianceExplanation.
</details>

---

### Question 53 (Easy)
**201.8.2** | Difficulty: Easy

Which of the following is a valid Storage Type in OneStream?

A) Temporary
B) DurableCalculation
C) Archived
D) Snapshot

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**DurableCalculation** is a valid Storage Type. The complete list of Storage Types in OneStream is: **Input**, **Journal**, **Calculation**, **DurableCalculation**, **Consolidation**, **Translation**, and **StoredButNoActivity**. DurableCalculation is used for Custom Calculate results that must persist through DUCS clearing.
</details>

---

### Question 54 (Easy)
**201.8.2** | Difficulty: Easy

What is the only required argument for the api.Data.Calculate function?

A) The Data Unit definition
B) The formula string
C) The source Data Buffer
D) The destination member name

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The **formula string** is the only required argument for api.Data.Calculate (ADC). All other arguments (such as Data Unit overrides or additional parameters) are optional. The formula string specifies the destination and source scripts that define what data to calculate and where to write it.
</details>

---

### Question 55 (Easy)
**201.8.2** | Difficulty: Easy

What are the "Common Members" in a Data Buffer?

A) Members that are shared between two different Data Buffers
B) Account-level dimensions that are shared for all rows in the Data Buffer
C) The most frequently used members in the Cube
D) Members that exist in every Entity

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Common Members** are the Account-level dimensions (such as Account, Flow, UD1-UD8, Origin) that are shared for all rows in a Data Buffer. They define the dimensional "shape" of the buffer. When two Data Buffers have different Common Members, they are considered "Unbalanced" and require special Unbalanced functions to operate on them together.
</details>

---

### Question 56 (Easy)
**201.8.2** | Difficulty: Easy

Which text View Members are available in OneStream?

A) Comment, Note, Description, Label, Tag
B) Annotation, Assumptions, AuditComment, Footnote, VarianceExplanation
C) Header, Footer, Summary, Detail, Total
D) Input, Output, Formula, Reference, Link

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The text View Members in OneStream are: **Annotation**, **Assumptions**, **AuditComment**, **Footnote**, and **VarianceExplanation**. These allow users to attach textual information to data cells. The numeric View Members are YTD, Periodic, QTD, MTD, and HTD.
</details>

---

### Question 57 (Medium)
**201.8.2** | Difficulty: Medium

What is the critical difference between GetDataBuffer and GetDataBufferUsingFormula?

A) GetDataBuffer is faster; GetDataBufferUsingFormula is slower
B) GetDataBuffer returns raw data without filters; GetDataBufferUsingFormula supports RemoveZeros, RemoveNoData, and FilterMembers
C) GetDataBuffer works with any Cube; GetDataBufferUsingFormula only works with the current Cube
D) There is no functional difference; they are interchangeable

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The critical difference is that **GetDataBufferUsingFormula** supports **RemoveZeros**, **RemoveNoData**, and **FilterMembers** operations, while **GetDataBuffer** does NOT. Since RemoveZeros should be used in ALL calculations as standard practice (to remove cells with amount 0 and NoData status), GetDataBufferUsingFormula is generally the preferred method for retrieving Data Buffers in calculations.
</details>

---

### Question 58 (Medium)
**201.8.2** | Difficulty: Medium

In an api.Data.Calculate formula string, which rule applies to Data Unit dimensions?

A) Data Unit dimensions can appear on both the left (destination) and right (source) sides
B) Data Unit dimensions should NEVER appear on the destination (left) side but CAN appear on the source (right) side
C) Data Unit dimensions should NEVER appear on either side
D) Data Unit dimensions must always appear on both sides

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In an ADC formula string, Data Unit dimensions (Cube, Scenario, Time, Consolidation, Entity) should **NEVER be in the destination script (left side)** because the destination is already determined by the current Data Unit context. However, they **CAN be in source scripts (right side)** to reference data from a different Scenario, Time period, Entity, etc. Placing Data Unit dimensions on the left side can cause data explosion or unexpected behavior.
</details>

---

### Question 59 (Medium)
**201.8.2** | Difficulty: Medium

Why are syntax errors in api.Data.Calculate formula strings particularly dangerous?

A) They cause the entire application to crash
B) They pass compile checks and are only revealed at runtime, making them hard to catch during development
C) They corrupt the database
D) They are automatically corrected by the system, leading to wrong results

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Syntax errors in ADC formula strings are particularly dangerous because they **pass compile checks** — the compiler cannot validate the content of a string literal. These errors are **only revealed at runtime** when the formula is actually executed. This means a developer may deploy code that compiles successfully but fails when calculations run, making thorough testing essential.
</details>

---

### Question 60 (Medium)
**201.8.2** | Difficulty: Medium

What is the DUCS "all or nothing" characteristic?

A) Either all Business Rules execute or none do
B) The entire DUCS sequence must run completely — you cannot selectively run only parts of it
C) All entities must be calculated together
D) All Formula Passes must contain formulas or none will execute

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The DUCS is **"all or nothing"** — when it executes, the entire sequence runs from start to finish. You cannot selectively run only certain steps (e.g., just Business Rule 3 or just Formula Pass 5). The full sequence always applies: Clear calculated data → Scenario Member Formula → Reverse Translations → BR 1-2 → FP 1-4 → BR 3-4 → FP 5-8 → BR 5-6 → FP 9-12 → BR 7-8 → FP 13-16. This is one reason Custom Calculate exists — for selective execution outside the DUCS.
</details>

---

### Question 61 (Medium)
**201.8.2** | Difficulty: Medium

What is the first step of the DUCS execution sequence?

A) Execute Business Rule 1
B) Execute Formula Pass 1
C) Clear calculated data
D) Execute Scenario Member Formula

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The first step of the DUCS is **Clear calculated data**. The full sequence is: Clear calculated data → Scenario Member Formula → Reverse Translations → BR 1-2 → FP 1-4 → BR 3-4 → FP 5-8 → BR 5-6 → FP 9-12 → BR 7-8 → FP 13-16. Clearing calculated data first ensures a clean slate before recalculation, which is the "clear & replace" approach.
</details>

---

### Question 62 (Medium)
**201.8.2** | Difficulty: Medium

Why should #All rarely or never be used in api.Data.Calculate formulas?

A) It is deprecated and will be removed in future versions
B) It causes data explosion by creating cells for every possible member combination
C) It only works with Custom Calculate
D) It conflicts with RemoveZeros

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**#All** should rarely or never be used in ADC formulas because it causes **data explosion** — it creates cells for every possible member combination in the dimension, which can generate an enormous number of data cells. This leads to excessive memory consumption and processing time. Instead, use specific member references or Dimension Filters to target only the members that need to be calculated.
</details>

---

### Question 63 (Medium)
**201.8.2** | Difficulty: Medium

**Scenario:** A developer needs to use a Stopwatch to measure the execution time of a specific section of their Finance Business Rule. What API should they use?

A) api.Timer.Start() and api.Timer.Stop()
B) api.Debug.Stopwatch
C) api.Performance.Measure()
D) System.Diagnostics.Stopwatch (standard .NET)

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The OneStream API provides **api.Debug.Stopwatch** for measuring execution time within Business Rules. This is specifically designed for OneStream's multi-threaded calculation environment and works with the platform's logging infrastructure. It helps identify performance bottlenecks by timing specific code sections during troubleshooting.
</details>

---

### Question 64 (Medium)
**201.8.2** | Difficulty: Medium

What is the purpose of DimConstants in OneStream calculations?

A) To define constant numeric values used in formulas
B) To replace string comparisons with strongly-typed dimension member references for better performance
C) To set fixed dimension filters that cannot be changed
D) To define the number of dimensions in a Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**DimConstants** replace string comparisons with strongly-typed dimension member references. Instead of comparing strings like `If memberName = "Revenue"`, developers use DimConstants which are faster and less error-prone. String comparisons in loops are slow and can introduce typos; DimConstants provide compile-time checking and better performance.
</details>

---

### Question 65 (Medium)
**201.8.2** | Difficulty: Medium

What is the purpose of Global Variables in the context of Finance Business Rules?

A) To share data between different applications
B) To store objects that don't change across Data Units so they are loaded only once rather than in every Data Unit calculation
C) To define global constants visible to all users
D) To persist data between different consolidation runs

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Global Variables** are used to store objects that **don't change across Data Units** (such as lookup tables, member lists, or configuration data). By loading these objects once into a Global Variable, they are available for all Data Unit calculations without repeated database queries. This significantly improves performance when the same reference data is needed for every entity/time period being calculated.
</details>

---

### Question 66 (Medium)
**201.8.2** | Difficulty: Medium

In a Dynamic Calculation, how many dimensions are accessible via api.Pov?

A) 5 (Data Unit dimensions only)
B) 10
C) 18 (all dimensions)
D) It depends on the Cube configuration

<details>
<summary>Show answer</summary>

**Correct answer: C)**

In a Dynamic Calculation, **api.Pov** provides access to all **18 dimensions**. This is different from stored calculations where only the Data Unit dimensions (Cube, Scenario, Time, Consolidation, Entity) are directly available through the api context. Dynamic Calculations inherit the full POV from each cell in the report, giving access to Account, Flow, Origin, UD1-UD8, View, and the Data Unit dimensions.
</details>

---

### Question 67 (Medium)
**201.8.2** | Difficulty: Medium

Why do Dynamic Calculations NOT aggregate to parent members?

A) It is a known bug that will be fixed
B) Because they work cell-by-cell and are calculated on-the-fly for each specific intersection — there is no stored data to aggregate
C) They do aggregate to parent members
D) Because they only work with base-level entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dynamic Calculations do **NOT aggregate to parent members** because they work **cell-by-cell** and are calculated on-the-fly for each specific dimensional intersection, similar to how Excel formulas work. Since no data is stored, there is nothing for the system to aggregate upward through the hierarchy. If aggregated values are needed, the parent member would need its own Dynamic Calculation logic to compute the desired result.
</details>

---

### Question 68 (Medium)
**201.8.2** | Difficulty: Medium

What is the "Calculation Matrix" in OneStream?

A) A system-generated grid showing all possible member combinations
B) A documentation tool that maps calculations to Formula Passes, dimensions, and dependencies
C) An internal OneStream process that determines calculation order
D) A matrix of entity-to-entity intercompany relationships

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The **Calculation Matrix** is a documentation tool used by developers to **map calculations to Formula Passes, dimensions, and dependencies**. It provides a visual overview of which calculations occur in which passes, helping ensure proper sequencing (e.g., that BegBal calculates before formulas that depend on it). It is a best practice for managing complex calculation solutions.
</details>

---

### Question 69 (Medium)
**201.8.2** | Difficulty: Medium

**Scenario:** A developer hardcodes `If api.Pov.Time.MemberId = 20210101 Then...` to check if it's January. What best practice are they violating?

A) They should use DimConstants instead
B) They should never hardcode time periods — use POV functions like api.Time.IsFirstPeriodInYear instead
C) They should use Global Variables for the date
D) They should use a Dynamic Calculation instead

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The developer is violating the best practice of **never hardcoding time periods**. Instead, they should use POV functions like `api.Time.IsFirstPeriodInYear`, `api.Time.PriorYearMemberId`, and similar time-aware functions. Hardcoded dates break when the solution is used for different years or time periods and make code non-reusable.
</details>

---

### Question 70 (Medium)
**201.8.2** | Difficulty: Medium

What are the Consolidation Algorithm Types available at the Cube level?

A) Simple, Complex, Hybrid, Custom
B) Standard, Stored Share, Org-By-Period Elimination, Custom
C) Direct, Indirect, Proportional, Full
D) Base, Translated, Eliminated, Aggregated

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Consolidation Algorithm Types available at the Cube level are: **Standard** (default consolidation method), **Stored Share** (stores ownership share data), **Org-By-Period Elimination** (handles IC eliminations that vary by period), and **Custom** (allows fully custom consolidation logic). These are configured in the Cube properties.
</details>

---

### Question 71 (Medium)
**201.8.2** | Difficulty: Medium

What troubleshooting tool in Data Explorer allows you to see the detailed breakdown of how a calculated cell's value was derived?

A) Data Audit Trail
B) Calc Drill Down
C) Cell Inspector
D) Data Trace Log

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Calc Drill Down** in Data Explorer allows you to see the detailed breakdown of how a calculated cell's value was derived. It shows the source data and formulas that contributed to the result, making it an essential troubleshooting tool for debugging calculation issues. It works alongside LogMessage and Stopwatch as the three primary troubleshooting tools.
</details>

---

### Question 72 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer is writing a Data Buffer Cell Loop (DBCL) and needs to create result cells. Where must the result cell variable be declared, and how should data be written?

A) Result cell declared outside the loop; write to database inside each iteration
B) Result cell declared INSIDE the loop (as New); write once outside the loop using SetDataBuffer
C) Result cell declared as a class-level variable; write using api.Data.Calculate
D) Result cell declared inside the loop; write inside the loop using SetDataCell

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In a DBCL, the result cell must be declared **INSIDE the loop** (as New for each iteration) because each iteration may produce a different result cell with different dimensional coordinates. However, data should be written **once outside the loop** using **SetDataBuffer** — this is the "wheelbarrow method." Writing inside the loop (using SetDataCell or SetDataBuffer) would cause multiple database writes and severe performance degradation.
</details>

---

### Question 73 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer is using the Data Buffer Cell Loop technique and needs to reference source data cells. What are the three ways to get a data cell in a DBCL?

A) GetDataCell, GetDataBuffer, GetDataBufferUsingFormula
B) Source cell member names, DataCellPk with IDs, MemberScriptBuilder
C) By Account, By Entity, By Time
D) Direct reference, Indirect reference, Formula reference

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The three ways to get a data cell in a DBCL are: 1) **Source cell member names** — directly referencing the member names from the source cell being iterated. 2) **DataCellPk with IDs** — using the primary key structure with member IDs for precise cell targeting. 3) **MemberScriptBuilder** — constructing a member script programmatically for flexible cell referencing. Each method has different use cases depending on whether you need speed, flexibility, or readability.
</details>

---

### Question 74 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer needs to evaluate individual cells within a single Data Buffer during an api.Data.Calculate operation — for example, to apply conditional logic that multiplies only cells with positive amounts by a rate. What technique should they use?

A) Data Buffer Cell Loop with For Each
B) Eval with OnEvalDataBuffer subfunction
C) Eval2 with OnEvalDataBuffer2 subfunction
D) Dimension Filters with conditional formulas

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Eval** with the **OnEvalDataBuffer** subfunction is used to evaluate individual cells of **ONE Data Buffer** within an api.Data.Calculate. The OnEvalDataBuffer subfunction fires for each cell, and EventArgs provides access to the DataBuffer, DestinationInfo, and ResultDataBuffer. This allows applying conditional logic cell-by-cell. Eval2 (with OnEvalDataBuffer2) is for comparing cells of TWO Data Buffers.
</details>

---

### Question 75 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A consolidation solution includes a Foreign Exchange (FX) translation calculation. The developer needs to multiply local currency amounts by exchange rates, but the rate Data Buffer has Common Members {Account, Flow} while the source data buffer has Common Members {Account, Flow, UD1, UD2}. The developer writes: `api.Data.Calculate("A#Result = MultiplyUnbalanced(A#Source, A#Rate)")`. What is wrong with this code?

A) MultiplyUnbalanced is not a valid function
B) The Data Buffer with MORE dimensions (Source) must be the SECOND argument, not the first
C) You cannot use MultiplyUnbalanced within api.Data.Calculate
D) The formula is correct; there is no error

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The error is that the Data Buffer with **MORE dimensions must always be the SECOND argument** in Unbalanced functions. Since Source has Common Members {Account, Flow, UD1, UD2} (more dimensions) and Rate has {Account, Flow} (fewer dimensions), the correct syntax should be: `api.Data.Calculate("A#Result = MultiplyUnbalanced(A#Rate, A#Source)")` — with Rate (fewer dimensions) first and Source (more dimensions) second.
</details>

---

### Question 76 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer writes a Finance Business Rule that uses `BRApi.Finance.Data.GetDataCell(...)` to retrieve exchange rates for each entity during consolidation. The process runs correctly for small entity sets but causes server overload when running for 500+ entities. What is the root cause?

A) The exchange rates are too large to process
B) BRApi calls in Finance Rules open new database connections, causing overload in the multi-threaded Finance calculation environment
C) GetDataCell is slower than GetDataBuffer
D) The developer should use Custom Calculate instead

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**BRApi calls in Finance Rules** open **new database connections** each time they are invoked. In the multi-threaded Finance calculation environment (where multiple Data Units are processed in parallel), hundreds of concurrent BRApi calls create an explosion of database connections, causing server overload. The correct approach is to use API equivalents (like `api.Data.GetDataCell` or `api.Data.GetDataBufferUsingFormula`) which work within the existing calculation context without opening new connections.
</details>

---

### Question 77 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer is building a Retained Earnings Beginning Balance calculation. The formula must copy the ending balance from the prior year only in the first period of each year. Which combination of API functions and member formula properties is correct?

A) Use api.Time.PriorYearMemberId in all periods; set Formula Pass = 1
B) Use api.Time.IsFirstPeriodInYear to conditionally execute; reference prior year ending balance; set Formula Pass = 1 and Is Consolidated = True
C) Use a DynamicCalc that always returns the prior year balance; no Formula Pass needed
D) Use Custom Calculate triggered by a Data Management step at year-end only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach uses **api.Time.IsFirstPeriodInYear** to conditionally execute only in the first period, references the prior year ending balance, and is assigned to **Formula Pass 1** (since other calculations like Current Year Net Income in Pass 2 may depend on it). **Is Consolidated = True** ensures the formula runs for all Consolidation Members during the DUCS, not just Local. This is a classic Member Formula pattern for beginning balance calculations.
</details>

---

### Question 78 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer creates a Member Formula with Formula Type = DynamicCalc for a "BegBalDynamic" account. Users report that the parent entity shows 0 for this account while child entities show correct values. What explains this behavior?

A) The DynamicCalc formula has a bug
B) Dynamic Calculations do NOT aggregate to parent members — the parent entity needs its own calculation logic
C) The parent entity's calculation status needs to be refreshed
D) DynamicCalc formulas only work at base entities

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Dynamic Calculations do NOT aggregate to parent members.** They work cell-by-cell and are calculated on-the-fly for each specific intersection. Since no stored data exists, there is nothing to aggregate upward. The parent entity showing 0 is expected behavior. To display a correct value at the parent level, the developer would need to either: create a separate DynamicCalc logic at the parent that sums children, or use a stored calculation approach for parent-level visibility.
</details>

---

### Question 79 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer needs to calculate allocations where corporate overhead costs are distributed to business units based on headcount percentages. The headcount percentages are stored in UD1 = "Headcount" and the overhead costs are in UD1 = "Overhead". The result should go to UD1 = "AllocatedCost". What is the most efficient approach?

A) Use a Data Buffer Cell Loop to iterate through each entity and calculate individually
B) Use api.Data.Calculate with appropriate source scripts referencing both UD1 members, leveraging ADC's ability to handle the math in a single operation
C) Use a Dynamic Calculation for real-time allocation
D) Use Custom Calculate with api.Data.ClearCalculatedData inside a loop

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most efficient approach is using **api.Data.Calculate** (ADC) with appropriate source scripts that reference both UD1 members. For example: `api.Data.Calculate("A#[Accounts]:U1#AllocatedCost = A#[Accounts]:U1#Overhead * A#[Accounts]:U1#Headcount")`. ADC processes the entire allocation in a **single operation** without loops, leveraging OneStream's optimized set-based processing. Using DBCL or loops would be unnecessarily complex, and Custom Calculate with ClearCalculatedData inside a loop is a severe anti-pattern.
</details>

---

### Question 80 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer writes the following Custom Calculate code but the calculated data disappears after the next Consolidation run. What is the most likely issue?

```
api.Data.ClearCalculatedData(...)
api.Data.Calculate("A#Forecast = A#Actual * 1.05")
api.Data.SetDataBuffer(resultBuffer)
```

A) The ClearCalculatedData call is in the wrong position
B) The data was not marked as isDurable = True, so the DUCS cleared it during the next Consolidation
C) SetDataBuffer cannot be used in Custom Calculate
D) The formula string syntax is incorrect

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most likely issue is that the data was **not marked as isDurable = True**. In Custom Calculate, all calculated data must be marked as `isDurable = True` (DurableCalculation storage type) so that it persists when the DUCS runs during the next Consolidation. Without this flag, the DUCS's first step — "Clear calculated data" — removes the Custom Calculate results. This is one of the three mandatory requirements of Custom Calculate.
</details>

---

### Question 81 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer is building a variance analysis report and needs to show the difference between Actual and Budget scenarios. They want the calculation to be available in any Cube View without needing stored data. What approach should they use, and what are the key considerations?

A) Use a Finance Business Rule with Calculate function type
B) Use a Dynamic Calculation (DynamicCalc Member Formula) that references both Scenarios using api.Pov, remembering that api.Pov provides all 18 dimensions and that the result will NOT aggregate to parents
C) Use Custom Calculate to pre-compute the variance
D) Use an Extender Business Rule to generate the variance data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A **Dynamic Calculation** (DynamicCalc Member Formula) is the correct approach because: it calculates on-the-fly without stored data, is available in any Cube View where the member is referenced, and can reference different Scenarios using **api.Pov** (which works for all **18 dimensions** in Dynamic Calcs). Key considerations: the result will **NOT aggregate to parent members** (each parent intersection must calculate independently), and Dynamic Calcs work **cell-by-cell** (not with Data Buffers), using the **Return** statement to return the result.
</details>

---

### Question 82 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer encounters a scenario where they need to "collapse detail" in a calculation — taking data at a detail level (e.g., UD1 base members) and writing results to a higher level (e.g., UD1 parent). They are using api.Data.Calculate. What is the correct approach?

A) Use #All for the UD1 dimension on the destination side
B) Use Dimension Filters in the ADC to specify the source at detail level and the destination at the parent level, ensuring parent members are on the destination (left) side only
C) This is not possible with ADC; use DBCL instead
D) Use a separate aggregation step after the calculation

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Collapsing detail** in ADC is achieved by using **Dimension Filters** to specify source data at the detail level and writing to the parent level on the destination (left) side. The destination script references the parent member while the source script references base members or uses filters. This is a standard ADC technique. Using #All on the destination would cause data explosion, which is why targeted member references and Dimension Filters are essential.
</details>

---

### Question 83 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A company uses "Relational Blending" to combine financial amounts with non-financial statistical data (e.g., headcount) in reports. What is the UD8 technique commonly used for this, and why?

A) UD8 is used as a "measure" dimension to differentiate between financial and statistical data types within the same account structure
B) UD8 is used to store exchange rates for currency translation
C) UD8 is used exclusively for intercompany eliminations
D) UD8 has no special role in Relational Blending

<details>
<summary>Show answer</summary>

**Correct answer: A)**

In **Relational Blending**, **UD8** is commonly used as a "measure" dimension to differentiate between financial and statistical data types. This technique allows mixing financial data (e.g., revenue in dollars) with non-financial data (e.g., headcount, square footage) within the same Cube structure. UD8 members might represent "Amount," "Headcount," "Rate," etc., enabling calculations that blend different measure types (like revenue per employee) in reports and dashboards.
</details>

---

### Question 84 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer needs to pass configuration parameters (like threshold values and flag settings) from a Dashboard to a Custom Calculate Finance Business Rule. What technique should they use?

A) Hard-code the values in the Business Rule
B) Use Name Value Pairs to pass parameters from the Dashboard through the Data Management step to the Business Rule
C) Store the values in a SQL table and query them
D) Use Global Variables set by the Dashboard

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Name Value Pairs** are the standard technique for passing configuration parameters from a Dashboard through a Data Management step to a Business Rule. Custom Calculate can be linked to Dashboards with user-driven parameters — the Dashboard collects user input, passes it as Name Value Pairs to the DM Sequence, and the Business Rule reads these values at runtime. This makes the calculation dynamic and configurable without code changes.
</details>

---

### Question 85 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer creates multiple result cells within a single iteration of a Data Buffer Cell Loop. Is this possible, and what is the correct pattern?

A) No, only one result cell can be created per iteration
B) Yes, multiple result cells can be created per iteration — each result cell is declared as New inside the loop and added to the ResultDataBuffer, then SetDataBuffer is called once outside the loop
C) Yes, but each result cell requires its own SetDataBuffer call inside the loop
D) Yes, but only if the result cells belong to different dimensions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Yes, **multiple result cells can be created per iteration** of a DBCL. Each result cell is declared as **New inside the loop** (with its own dimensional coordinates) and added to the ResultDataBuffer. The key rule is that **SetDataBuffer is called only ONCE outside the loop** after all iterations complete — this is the wheelbarrow method. Calling SetDataBuffer inside the loop would negate the performance benefits of DBCL.
</details>

---

### Question 86 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer is implementing a Cumulative Translation Adjustment (CTA) calculation for a multinational consolidation. The CTA represents the difference between the balance sheet translated at closing rates and the income statement translated at average rates. In which DUCS phase does this translation-related calculation typically execute?

A) C#Local (Calculate phase)
B) C#Translated (Translation phase)
C) C#Share (ConsolidateShare phase)
D) Only in Custom Calculate

<details>
<summary>Show answer</summary>

**Correct answer: B)**

CTA (Cumulative Translation Adjustment) calculations are translation-related and typically execute in the **C#Translated** phase of the DUCS. This phase handles currency Translation calculations, including converting local currency amounts to reporting currency. The Translation Algorithm Types at the Cube level (Standard, Standard Using BR for FX Rates, Custom) control how translation logic is applied. CTA captures the variance arising from translating different balance sheet and income statement items at different exchange rates.
</details>

---

### Question 87 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer needs to implement a "seeding" rule that copies Budget data from the prior year as a starting point for the new year's planning cycle. The rule should only run on demand, not during regular consolidation. What is the correct approach?

A) Use a Calculate function type in a Finance Business Rule
B) Use Custom Calculate triggered via a Data Management step, with isDurable = True, and api.Data.ClearCalculatedData at the start
C) Use a DynamicCalc Member Formula
D) Use a Member Formula with Calculate function type in Formula Pass 1

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Custom Calculate** is the correct approach because: it runs **only on demand** via a Data Management step (not during regular DUCS consolidation), does NOT consider Calculation Status (important for seeding when target may not have data yet), and can be linked to Dashboard buttons for user-triggered execution. The data must be marked **isDurable = True** so it persists through future DUCS runs, and **api.Data.ClearCalculatedData** must be included at the start to clear any previous seeded data.
</details>

---

### Question 88 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** In a consolidation solution, an Account member has the property "Is Consolidated = True" set on its Member Formula. What does this property control?

A) Whether the account is included in the consolidation hierarchy
B) Whether the Member Formula executes for ALL Consolidation Members (Local, Translated, Share, etc.) during the DUCS, not just Local
C) Whether the account data is stored or calculated dynamically
D) Whether the account participates in intercompany elimination

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Is Consolidated = True** on a Member Formula controls whether the formula executes for **ALL Consolidation Members** (Local, Translated, Share, Elimination, etc.) during the DUCS, not just the Local consolidation member. This is important for calculations like Current Year Net Income where the formula needs to produce results at every consolidation level. Without this flag, the formula would only execute during C#Local.
</details>

---

### Question 89 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer notices that members in the same Formula Pass and same dimension are executing simultaneously, causing a timing issue where one member's result depends on another member that hasn't finished calculating yet. Why is this happening and what is the fix?

A) The developer should use Custom Calculate to control execution order
B) Members in the same pass and same dimension are multi-threaded (run in parallel), so dependent members must be placed in different Formula Passes to ensure sequential execution
C) This is a system bug; members always execute sequentially
D) The developer should add explicit locks in the code

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Members in the same Formula Pass and same dimension are **multi-threaded** (run in parallel). If one member's calculation depends on another member's result, they must be placed in **different Formula Passes** to ensure sequential execution. For example, if CurYearNetIncome depends on RetainedEarnings BegBal, BegBal should be in Pass 1 and CurYearNetIncome in Pass 2. The DUCS completes all formulas in one pass before moving to the next.
</details>

---

### Question 90 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer sets Account-level POV dimensions (like specific Account ranges or UD1 members) in a Custom Calculate Data Management step's properties. What is the purpose of this configuration?

A) To define which accounts are visible in reports
B) To restrict which Account-level POV dimensions the Custom Calculate processes, so only the specified subset of data is calculated rather than the entire Data Unit
C) To set default values for the Dashboard parameters
D) To define security access for the calculated data

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Setting Account-level POV dimensions in the DM step properties **restricts which data the Custom Calculate processes**. Instead of calculating the entire Data Unit, only the specified subset of Account-level dimensions is processed. This is a key advantage of Custom Calculate over the DUCS — the ability to selectively calculate specific portions of data. This makes Custom Calculate efficient for targeted calculations like planning scenarios that only affect certain accounts.
</details>

---

### Question 91 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer uses `api.LogMessage("Debug: buffer count = " & buffer.DataBufferCells.Count)` throughout their Finance Business Rule to track execution during development. The solution works correctly in the development environment but causes performance issues when deployed to production with 2,000+ entities. What should they do?

A) Reduce the number of LogMessage calls to 5 or fewer
B) Comment out or remove ALL LogMessage calls for production, as they can significantly impact server performance in multi-threaded environments processing many entities
C) Redirect LogMessage output to a file instead of the log
D) Replace LogMessage with Console.WriteLine

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**LogMessage should be commented out or removed for production.** In a multi-threaded environment processing 2,000+ entities, each LogMessage call writes to the server log, creating significant I/O overhead. With multiple threads writing simultaneously, this can cause log contention and severely degrade performance. The best practice is to use LogMessage during development and testing, then comment out all instances before deploying to production.
</details>

---

### Question 92 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer is configuring a Cube and sees "NoData Calculate Settings" in the Cube properties. What does this setting control?

A) Whether to include NoData cells in report exports
B) How the Cube handles calculations when source data cells have no data (NoData status) — controlling whether formulas execute or skip when source data is absent
C) The default value for cells with no data
D) Whether NoData cells are visible in Cube Views

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**NoData Calculate Settings** in the Cube properties controls how the Cube handles calculations when source data cells have **NoData status**. This determines whether formulas execute or are skipped when source data is absent, which affects calculation behavior and performance. Properly configuring this setting prevents unnecessary calculations on empty data and ensures formulas produce correct results when some source data doesn't exist.
</details>

---

### Question 93 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer needs to organize a complex Finance Business Rule with 500+ lines of code that handles multiple calculation types (Balance Sheet, Income Statement, Allocations, FX). What code organization best practices should they follow?

A) Keep all code in one continuous block for easier searching
B) Use Regions to organize code sections, add descriptive comments, and use a Calculation Matrix to document the relationship between calculations and Formula Passes
C) Split the code into multiple Finance Business Rules (one per calculation type)
D) Move all code to Member Formulas for better organization

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best practices for organizing complex Finance BRs include: **Regions** to group related code sections (e.g., #Region "Balance Sheet Calculations"), **descriptive comments** explaining the purpose and dependencies of each calculation block, and a **Calculation Matrix** to document which calculations run in which Formula Passes and their dependencies. This makes the code maintainable and helps other developers understand the calculation flow. Splitting into multiple BRs is limited by the 8 BR per Cube maximum and adds complexity.
</details>

---

### Question 94 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer is implementing an Equity Pickup (EPU) calculation for a partial ownership subsidiary. The parent owns 60% of the subsidiary. In which DUCS phase does the EPU calculation typically execute, and why?

A) C#Local, because EPU is a local entity calculation
B) C#Share (ConsolidateShare phase), because EPU involves ownership percentage calculations that occur during the share/consolidation processing
C) C#Translated, because EPU involves currency conversion
D) Only via Custom Calculate, because EPU requires selective execution

<details>
<summary>Show answer</summary>

**Correct answer: B)**

**Equity Pickup (EPU)** calculations typically execute in the **C#Share (ConsolidateShare)** phase of the DUCS. This phase handles ownership percentage calculations, share of subsidiary income/equity, and related consolidation entries. Since EPU involves applying the parent's ownership percentage (60% in this case) to the subsidiary's equity and income, it naturally belongs in the Share consolidation phase. This is also why C#Aggregated (which skips Share/Ownership calculations) cannot be used for solutions requiring EPU.
</details>

---

### Question 95 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A developer has a Finance Business Rule that queries a SQL auxiliary table for mapping data. This query returns the same result for every Data Unit in the calculation. Currently the query runs once per Data Unit, causing 500 database queries for 500 entities. How should they optimize this?

A) Use Formula Variables to cache the query result
B) Store the query result in a Global Variable so it is loaded only once and reused across all Data Unit calculations
C) Move the query to a Custom Calculate to reduce frequency
D) Use DimConstants to avoid the query entirely

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The developer should store the query result in a **Global Variable**. Global Variables are designed for objects that **don't change across Data Units** — they are loaded once and persist for the duration of the calculation run. This reduces the 500 database queries to just 1 query. Formula Variables are scoped to a single api.Data.Calculate call and won't persist across Data Units. Global Variables are one of the key performance optimization techniques for Finance Business Rules.
</details>

---

### Question 96 (Hard)
**201.8.2** | Difficulty: Hard

**Scenario:** A complex consolidation solution has the following calculation dependencies: Activity calculation depends on Beginning Balance, Current Year Net Income depends on Activity, and Intercompany Elimination depends on Current Year Net Income. The developer assigns all of them to Formula Pass 1. What will happen?

A) OneStream will automatically detect dependencies and reorder the calculations
B) The calculations will produce incorrect results because members in the same Formula Pass and dimension are multi-threaded — dependent calculations must be spread across sequential Formula Passes (e.g., BegBal in Pass 1, Activity in Pass 2, CYNI in Pass 3, IC Elim handled by DUCS consolidation phase)
C) The calculations will execute correctly because OneStream processes them in alphabetical order
D) The code will fail to compile

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Placing all dependent calculations in the **same Formula Pass** will produce **incorrect results** because members in the same pass and dimension are **multi-threaded** (run in parallel). There is no guaranteed execution order within a single pass. The correct approach is to spread them across sequential Formula Passes: BegBal in Pass 1, Activity in Pass 2, CYNI in Pass 3, etc. The DUCS completes all formulas in one pass before starting the next, ensuring proper dependency ordering. IC Eliminations are handled by the DUCS consolidation phase (C#Share). This is exactly what the **Calculation Matrix** documentation tool is designed to manage.
</details>

---

---

