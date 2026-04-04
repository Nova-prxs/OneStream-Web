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
