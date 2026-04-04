# General Guide - OS-201 Exam: OCP OneStream Core Platform Architect

## Exam Description

- **Code**: OS-201
- **Level**: OneStream Certified Professional (OCP)
- **Platform**: OneStream 8.x
- **Format**: Online, with remote video proctor (requires 2 cameras: laptop + mobile with Google Meet)
- **Question types**: Multiple choice + performance-based (practical scenarios)
- **Dual monitor**: NOT allowed (external monitor is allowed if the laptop remains CLOSED)

## Sections and Weights

| # | Section | Weight | Study File |
|---|---------|--------|------------|
| 1 | **Cube** | 15% | [section-01-cube.md](section-01-cube.md) |
| 2 | **Workflow** | 14% | [section-02-workflow.md](section-02-workflow.md) |
| 3 | **Data Collection** | 13% | [section-03-data-collection.md](section-03-data-collection.md) |
| 4 | **Presentation** | 14% | [section-04-presentation.md](section-04-presentation.md) |
| 5 | **Tools** | 9% | [section-05-tools.md](section-05-tools.md) |
| 6 | **Security** | 10% | [section-06-security.md](section-06-security.md) |
| 7 | **Administration** | 9% | [section-07-administration.md](section-07-administration.md) |
| 8 | **Rules** | 16% | [section-08-rules.md](section-08-rules.md) |

## Study Strategy by Priority

### High Priority (45% of the exam)
These 3 sections represent almost half of the exam. Mastering these gives you a solid foundation:

1. **Rules (16%)** - The heaviest section. Knowing all types of Business Rules, Function Types, DUCS, Data Buffers, and api.Data.Calculate is fundamental.
2. **Cube (15%)** - Cube Properties, dimensions, FX Rates, Data Units, stored vs dynamic calculations.
3. **Workflow (14%)** / **Presentation (14%)** - Both with equal weight. Workflow Names, Profile Settings, Cube Views, Workspaces, Report Books.

### Medium Priority (23% of the exam)
4. **Data Collection (13%)** - Data Source types, XFD configuration, Import Process Log, Transformation Rules.
5. **Security (10%)** - Security roles (Application, UI, System), Data Cell Access, troubleshooting.

### Normal Priority (18% of the exam)
6. **Tools (9%)** - Data Management steps, Excel Add-in, Application Properties, Load/Extract.
7. **Administration (9%)** - System tab, Logging, troubleshooting.

## Reference Books Used

| Book | Chapters | Sections Covered |
|------|----------|-----------------|
| **Design and Reference Guide** | 24 chapters | All sections (main reference) |
| **Finance Rules and Calculations** | 8 chapters | Section 1 (Cube), Section 8 (Rules) |
| **Foundation Handbook** | 14 chapters | Sections 1, 2, 3, 6, 8 |
| **Workspaces and Assemblies** | 12 chapters | Section 4 (Presentation), Section 8 (Rules) |

## Tips for Exam Day

### Technical Preparation
- Test cameras, microphone and audio in advance
- Use Chrome, Firefox or Edge (NOT Safari or Internet Explorer)
- Disable the popup blocker
- Close all background applications
- Avoid work computers (VPN and filters may interfere)
- Complete the 5-question verification quiz BEFORE exam day

### Strategy During the Exam
- Performance-based questions require applying knowledge in real-world scenarios
- Pay attention to keywords: "given a design specification", "demonstrate", "apply"
- Higher-weight sections will have more questions - manage time proportionally
- On Rules questions, focus on the **correct use case** for each rule type
- On Cube questions, Cube Properties and DUCS details are critical

## Cross-cutting Topics (appear in multiple sections)

These concepts cross several exam sections:

- **Data Unit Calculation Sequence (DUCS)**: Sections 1 (Cube) and 8 (Rules)
- **Business Rules**: Sections 1 (Cube), 3 (Data Collection), 4 (Presentation) and 8 (Rules)
- **Workflow Profiles**: Sections 2 (Workflow), 3 (Data Collection) and 6 (Security)
- **Parameters**: Sections 4 (Presentation), 5 (Tools) and 2 (Workflow)
- **Workspaces**: Sections 4 (Presentation) and 8 (Rules - Assemblies)
- **Load/Extract**: Sections 5 (Tools) and 7 (Administration)
- **Logging and Troubleshooting**: Sections 7 (Administration), 1 (Cube) and 8 (Rules)

## Suggested Study Plan (2-3 weeks)

### Week 1: Fundamentals
- Day 1-2: Section 8 (Rules) - the densest and heaviest
- Day 3: Section 1 (Cube) - second most important
- Day 4: Section 2 (Workflow)
- Day 5: Section 4 (Presentation)

### Week 2: Supplements
- Day 1: Section 3 (Data Collection)
- Day 2: Section 6 (Security)
- Day 3: Section 5 (Tools)
- Day 4: Section 7 (Administration)
- Day 5: Review of cross-cutting topics

### Week 3: Review and Practice
- Review critical points to memorize from each section
- Practice with exam-type questions (question database - next phase)
- Focus on sections where you feel less confident


---


# Section 1: Cube (15% of the exam)

## Exam Objectives

- **201.1.1:** Given a design specification, apply changes to the Cube configuration
- **201.1.2:** Given a long-running Calculation for an Entity, analyze Data Unit statistics to determine the root cause
- **201.1.3:** Demonstrate how to configure and apply FX Rates
- **201.1.4:** Demonstrate understanding of Dimension Member configuration
- **201.1.5:** Demonstrate understanding of Stored and Dynamic Calculations

---

## Key Concepts

### Objective 201.1.1: Apply changes to the Cube configuration

#### General Cube Structure

A Cube is a multidimensional structure that contains data for reporting and analysis. It consists of **18 Dimensions** shaped through Extensible Dimensionality. Cubes are created in **Application > Cube > Cubes** and configured in the Cube Profile.

![Navigation to Dimension Library where Cube Dimensions are created and maintained](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p537-2785.png)

A Cube has the following configuration tabs:
- **Cube Properties** (general properties, security, Workflow, calculation, FX Rates, Business Rules)
- **Cube Dimensions** (Dimension assignment by Scenario Type)
- **Cube References** (linking of Linked Cubes)
- **Data Access** (data cell-level security)
- **Integration** (data load configuration)

#### Cube Properties - General

![General Cube properties showing Name, Description, Cube Type and Time Dimension Profile](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p640-3179.png)

- **Name:** Maximum 100 characters. **Cannot be changed once created.** Accessed via API: `Dim sValue As String = objCube.Name`.
- **Description:** Maximum 200 characters. Can be changed at any time.
- **Cube Type:** Optional label for grouping similar Cubes. Options:
  - Standard (default value when created), Tax, Treasury, Supplemental, What If, Cube Type 1-8
  - Cube Types are stored as integers in the `dbo.Cube` table:
    - Standard=0, Tax=1, Treasury=2, Supplemental=3, What If=4
    - CubeType1=11, CubeType2=12, CubeType3=13, CubeType4=14
    - CubeType5=15, CubeType6=16, CubeType7=17, CubeType8=18
  - Allows different Cubes to share Dimensions with different Constraints by Cube Type
  - Cube Type names are functionally irrelevant; they do not change system behavior
- **Time Dimension Profile:** Assigns the time profile (fiscal calendar) to the Cube. Dropdown list of available Time Profiles.

#### Cube Properties - Security

![Cube security section with Access Group, Maintenance Group and Use Parent Security](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p643-3188.png)

- **Access Group:** Group that can view the Cube (default: Everyone). This is the second layer of security after application access.
- **Maintenance Group:** Group that can edit Cube configuration, create new objects, and delete.
- **Use Parent Security for Relationship Consolidation:**
  - **False (default):** Everyone with access to Parent Entities can access all Consolidation Members.
  - **True:** Controls access to Consolidation Members such as Share, Elimination, OwnerPostAdj, OwnerPreAdj, based on Parent Entity security. The user can see Local and Translated but not the relationship members.
  - Determined on a Cube-by-Cube basis.

#### Cube Properties - Workflow

![Cube Workflow configuration showing Is Top Level Cube and Suffix settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p645-3195.png)

- **Is Top Level Cube for Workflow:**
  - **False** by default when a new Cube is created.
  - Must be **True** for the Cube to create and maintain Workflow Profiles through a Cube Root Profile.
  - Only **one Cube** can control the Workflow Profiles.
  - If Linked Cubes with Extensible Dimensionality are used, only 1 Cube has True; the rest have False.

- **Suffix for Varying Workflow by Scenario Type:**
  - Groups Scenarios by Scenario Type to make them available in the Workflow.
  - One setting per Scenario Type.
  - Example: assign "ACT" to Actual and "PLAN" to Budget/Forecast/Plan.

![Example of Cube Root Workflow Profiles created after configuring suffix](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p647-3200.png)

  - **IMPORTANT:** Once a Scenario has loaded data, a new Cube Root cannot be created nor can the suffix be changed for that Scenario Type. A **Reset Scenario** (Data Management step) is required to make changes.
  - If a Scenario Type is blank and has no data, a suffix can be added at any time.

#### Cube Properties - Calculation

![Cube Calculation section with Consolidation Algorithm Type, Translation Algorithm Type and Business Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p649-3211.png)

##### Consolidation Algorithm Type

Defines how Share and Elimination are treated during Consolidation:

| Algorithm Type | Share | Elimination | Use case |
|---|---|---|---|
| **Standard (Calc-on-the-fly Share and Hierarchy Elimination)** | Dynamic (not stored) | Built-in algorithms | Most commonly used. Only in rare circumstances is Standard not used. |
| **Stored Share** | Stored | Built-in algorithms | Minority interest where Share cannot be derived solely from Percent Consolidation. Requires BR with `FinanceFunctionType.ConsolidateShare`. |
| **Org-By-Period Elimination** | Dynamic | Considers Percent Consolidation at each hierarchy relationship | When an IC Member may not be a descendant if Percent Consolidation = 0. |
| **Stored Share and Org-By-Period Elimination** | Stored | Org-By-Period | Combination of both. |
| **Custom** | Via Business Rules | Via Business Rules | Fully customized logic with `ConsolidateShare` and `ConsolidateElimination`. Custom eliminations in UD Dimensions or specific intersections. |

**Performance impact of Stored Share:**
- Increases records in Data Record tables
- Increases Data Unit size
- The engine must execute custom logic and write additional data

![Consolidation Algorithm Type options in the interface](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p37-764.png)

##### Translation Algorithm Type

![Translation Algorithm Type options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p38-771.png)

| Translation Type | Description | Use case |
|---|---|---|
| **Standard** | Translation based on FX Rates assigned to the Cube/Scenario | Most commonly used. |
| **Standard Using Business Rules for FX Rates** | Standard + BR to specify rates by intersection | Translate Actual at Budget rates. Apply different rates to certain Accounts. Forecast/Constant Currency. |
| **Custom** | Translation entirely via Business Rules | Rare. Non-standard translation methods. |

##### Business Rules 1-8

![Up to 8 Finance Business Rules can be assigned to the Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p653-3221.png)

- They execute within the DUCS in logical order interleaved with Member Formulas.
- The same Finance Business Rule can be shared across Cubes.
- The BR must be created as a Finance Business Rule to appear in the list.

##### NoData Calculate Settings

![NoData Calculate Settings for each Consolidation Member](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p38-772.png)

Controls whether the Finance Engine executes Calculations against cells with no data for each Consolidation Member:
- **Default value:** False (except Local)
- **Set to True** when copying data from another Time period or Scenario
- If not set to True, empty Data Units in the Target will not execute the DUCS, even if there is data in the Source
- In most situations where data is copied, copying only `C#Local` is sufficient since the Consolidation algorithm will translate and consolidate

#### Cube Properties - FX Rates

![Cube FX Rates section with Default Currency, Rate Types and Rule Types](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p653-3222.png)

- **Default Currency:** Default reporting currency for the Cube. Used for FX Rate triangulation and IC Matching.
- **Rate Type for Revenues and Expenses:** Default AverageRate
- **Rate Type for Assets and Liabilities:** Default ClosingRate
- **Rule Type for Revenues and Expenses:** Default **Periodic**
- **Rule Type for Assets and Liabilities:** Default **Direct**
- Predefined Rate Types: AverageRate, OpeningRate, ClosingRate, HistoricalRate
- New FX Rate Types can be created in **Cube > FX Rates** (available immediately)
- **Direct:** `Translated Value = Local Value * FX Rate of the Current Period`
- **Periodic:** `Translated Value = Prior Period Translated + [(Current Local - Prior Local) * Current Rate]`

#### Cube Dimensions - Assignment

![Dimension assignment by Scenario Type in the Cube - Default](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p655-3228.png)

![Dimension assignment by Scenario Type - detail by type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p656-3231.png)

Dimensions are assigned by **Scenario Type**. Critical rules:

1. **Entity and Scenario** are only assigned in the **(Default)** Scenario Type. In the other Scenario Types they are grayed out.
2. **Recommended configuration:** Assign **RootXXXDim** to unused Dimension Types in each Scenario Type (instead of leaving "Use Default"). This allows future flexibility.

![Recommended configuration with RootXXXDim for unused dimensions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p657-3235.png)

3. **CRITICAL:** Once Dimensions are assigned and data has been loaded, they cannot be unassigned or changed. To change, a **Reset Scenario** is required.
4. Changing from RootXXXDim to a specific Dimension is a **one-time, irreversible change** if data exists.
5. If **(Use Default)** is left and data is loaded, the Dimension Type becomes locked to whatever the Default has.

**Common use scenario (Exam Use Case):**
- A user wants to add a Customer dimension in UD4 only for the Budget Scenario Type.
- If they followed the recommended configuration (RootUD4Dim in Budget), they can change UD4 from RootUD4Dim to the new dimension.
- If they left (Use Default), they must assign the new dimension in the (Default) Scenario Type, affecting ALL Scenario Types.

![Successful example of dimension change when recommended configuration was used](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p662-3254.png)

#### Cube Design Options

| Option | Description | Use case |
|---|---|---|
| **Monolithic Cube** | A single simple Cube | Only for very small applications |
| **Super Cube (Linked Cubes)** | Parent Cube with child Cubes linked via Entity Dimension | Most common design. Best use of extensibility. |
| **Paired Cubes** | Split/shared entities across multiple Cubes | Same Entity in multiple Cubes |
| **Specialty Cubes** | Limited monolithic Cubes | Driver cubes, admin cubes |

- **General rule:** More Cubes do not slow down the application; they improve performance by reducing Data Unit size.

#### Cube References

![Cube References showing Entity Dimension linking between parent and child Cubes](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p668-3282.png)

Used to link Linked Cubes:
- The parent Cube has the **Cube References** tab configured
- When Entity Dimensions from child Cubes create relationships with the parent Cube's Entity Dimension, they automatically appear in the list
- This ensures data consolidates in the parent Cube's hierarchy

#### Data Access Tab

![Data Access tab structure with three security sections](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p670-3287.png)

Three security sections at the Cube level:

**1. Data Cell Access Security ("Slice Security"):**
- Access control (No Access, Read Only, All Access) for groups of cells
- Requires Member Filters + Access Groups
- Optional **Category** for grouping rules (maximum 100 characters)
- Eight behaviors available:
  - Skip Item and Continue (default for "not in Group/not in Filter")
  - Skip Item and Stop
  - Apply Access and Continue (default for "in Group and in Filter")
  - Apply Access and Stop
  - Increase/Decrease Access and Continue/Stop
- Executes **after** Application > Cube > Entity > Scenario security
- To activate categories in an Entity: `Use Cube Data Access Security = True`

**2. Data Cell Conditional Input:**
- Controls whether **all users** can enter data in specific cells
- Same properties as Data Cell Access Security **except** it has no Access Group
- Applies to all users equally

**3. Data Management Access Security:**
- Controls access when Data Management processes are executed
- Focused on **Data Units** (Entity and Scenario), not individual cells
- Only allows Member Filters for Entity and Scenario Dimension Types

#### Integration Tab

![Integration tab showing Cube Dimensions, Label Dimensions and configuration by Scenario Type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p679-3317.png)

Controls data structure for Workflow:
- **Cube Dimensions:** Automatically assigned from the Cube Dimensions tab
- **Label Dimensions:** Label, SourceID, TextValue - auxiliary fields for Stage
- **Attribute Dimensions / Attribute Value Dimensions:** For BI Blend
- Each Dimension Type has: Cube Dimension Name (read-only), Transformation Sequence, Enabled
- **Disable** unused Dimension Types so they do not appear in Data Sources or Transformation Rules

---

### Objective 201.1.2: Analyze Data Unit statistics to determine root cause

#### What is a Data Unit

A Data Unit is the **partitioning and processing unit** of the Cube. It is defined as all data within a unique combination of:
- **Cube + Entity + Parent + Consolidation + Scenario + Time**

The Finance Engine processes all data within each Data Unit at once. It is the unit by which the Cube is partitioned and the mechanism that divides data into manageable parts that fit in server memory.

![Diagram of potential Cube intersections showing why Data Units are necessary](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch03-p62-923.png)

#### Data Unit Calculation Sequence (DUCS)

The DUCS is the series of steps that occurs every time a Calculation or Consolidation is executed:

1. Clear previously calculated data (if `Clear Calculated Data During Calc = True` in the Scenario; does not clear Durable Data)
2. Run Scenario Member Formula
3. Perform reverse Translations (Flow Members from other currencies)
4. Execute Business Rules 1 and 2
5. Execute Formula Passes 1-4 (Account > Flow > UD1 > UD2...UD8)
6. Execute Business Rules 3 and 4
7. Execute Formula Passes 5-8
8. Execute Business Rules 5 and 6
9. Execute Formula Passes 9-12
10. Execute Business Rules 7 and 8
11. Execute Formula Passes 13-16

**The DUCS is "all or nothing":** All steps execute every time; you cannot choose to execute only one step. This preserves data integrity and ensures that dependencies between calculations are never compromised.

#### Consolidation Dimension and the DUCS

The DUCS executes for each Consolidation Member. Some Finance Function Types only execute in certain Consolidation Members:

![Table showing which Finance Function Types execute in each Consolidation Member](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p40-782.png)

During a Consolidation of a Parent Entity, the DUCS can execute up to **7 times**:
1. Calculate Local Currency
2. Translate Local to Parent's Default Currency (`C#Translated`)
3. Calculate Translated Currency
4. Calculate OwnerPreAdj
5. Calculate/Default Share
6. Execute Elimination, then Calculate Elimination
7. Calculate OwnerPostAdj
8. Combine child data to parent local
9. Calculate parent local

#### Triggering the DUCS

The DUCS can be triggered from:

![Data Management step for Calculate showing Step Type and Calculation Type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p42-795.png)

1. **Data Management:** Step types Calculate or Custom Calculate with Calculation Types: Calculate, Translate, Consolidate (with Logging and Force variants)
2. **Cube Views:** Enabling Calculate/Consolidate in Cube View properties

![Cube View properties for enabling Calculate and Consolidate](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p44-810.png)

3. **Dashboard Button:** Server Task with DM Sequence or direct Calculate
4. **Workflow Process Step:** Via Calculation Definitions in Workflow Name

![Workflow Process Step with Calculation Definitions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p47-831.png)

#### Causes of slow Data Units

- **Excessive Data Unit size:** Data Units exceeding ~1 million records deserve inspection. More than 2 million are "red flags".
- **Dense Data Units at upper levels:** As data consolidates, Parent Entity Data Units become denser and slower.
- **Few threads at the top:** Sibling Entities are processed in parallel. At the top of the hierarchy there are few siblings = few threads.
- **Poorly written rules:** Business Rules that generate data at unnecessary intersections.
- **UD Dimensions with little overlap:** If UD1 has many members but little overlap between base entities, the Parent Data Unit inflates unnecessarily.
- **Unnecessary data in the Cube:** Cubes are not designed for large volumes of transactional data.

#### Calculation Status

- **OK:** No Calculation Needed, Data Has Not Changed.
- **CN:** Calculation Needed, Data Has Changed (from Form input, data load, or Journal).
- **Force Calculate/Consolidate:** Processes all dependent Data Units ignoring the Calculation Status. **Do not use unnecessarily** - wastes time processing Data Units with no changes.
- **Calculate/Consolidate normal:** First checks the Calculation Status and does not execute the DUCS if it is OK.

#### Parallel Processing

![Diagram of parallel processing of sibling Entities](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p48-838.png)

- Sibling Entities are processed simultaneously using multiple threads (multi-threading)
- The processing order between siblings may vary with each Consolidation
- Consolidations usually reach >90% quickly, and the last 10% takes longer (because there are fewer siblings at upper levels)
- **Sibling Consolidation Pass:** Allows forcing order between siblings (used for Equity Pickup). Pass 2+ forces the Entity to consolidate after siblings in Pass 1.
- **Sibling Repeat Calculation Pass:** For circular calculations between entities (circular ownership)
- **Auto Translation Currencies:** For translation to sibling currencies during Consolidation

#### Calculate With Logging

To diagnose slow Calculations:

![Calculate With Logging from Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p170-1721.png)

1. Execute **Consolidate With Logging** or **Calculate With Logging**
2. Go to Task Activity > Child Steps
3. Drill into each Data Unit to see the duration of each DUCS step

![Child Steps showing duration of each DUCS step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p173-1745.png)

4. **CreateDataUnitCache** shows the number of records brought into memory (= Data Unit size)
5. Drill into individual Formula Passes and Business Rules to find the culprit

![Detail of individual Formula Passes with duration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p174-1751.png)

**Note:** Calculate With Logging adds significant processing time.

#### Best practices for performance

- Use extensibility (multiple Cubes) to reduce Data Unit size
- Limit valid intersections with Constraints
- Use `C#Aggregated` for Planning (up to 90% faster)
- Do not store percentages or ratios (calculate them dynamically)
- Monitor Data Units larger than 1 million records
- Optimize Entity hierarchies (avoid flat structures with many children)
- Avoid 1-to-1 Entity-Parent relationships (they create unnecessary storage points)
- Do not store transactional data in Cubes (use BI Blend or Specialty Planning)
- Increase the number of Entities to distribute data and leverage multi-threading

---

### Objective 201.1.3: Configure and apply FX Rates

#### Where FX Rates are stored

FX Rates are stored in the **System Cube** as a central repository that all other Cubes reference. This avoids duplicating rates in each Cube.

#### Predefined FX Rate Types

| Rate Type | Description |
|---|---|
| **Average Rate** | Average rate for the period (first day to last day of the month) |
| **Opening Rate** | Rate at the start of the period |
| **Closing Rate** | Rate at the close of the period |
| **Historical Rate** | Historical rate from a specific date |

New FX Rate Types can be created in **Cube > FX Rates**, which automatically become available for selection in Cube Properties and Scenario Properties.

#### Configuration at the Cube level

In **Cube Properties > FX Rates:**
- **Default Currency:** Default reporting currency (used for triangulation and IC Matching)
- **Rate Type for Revenues and Expenses:** Default AverageRate
- **Rate Type for Assets and Liabilities:** Default ClosingRate
- **Rule Type for Revenues and Expenses:** Default Periodic
- **Rule Type for Assets and Liabilities:** Default Direct

#### Configuration at the Scenario level

In Scenario Member Properties > FX Rates:

![FX Rates configuration at Scenario level with Use Cube FX Settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p577-2935.png)

- **Use Cube FX Settings:**
  - **True (default):** Uses the Cube's default Rate Types
  - **False:** Enables custom configuration at the Scenario level:
    - Rate Type and Rule Type for Revenue/Expenses and Assets/Liabilities
    - **Constant Year for FX Rates:** Allows using rates from a specific year (useful for Constant Currency Scenarios)

#### Rule Types (translation methods) - Detail

| Rule Type | Formula | Use case |
|---|---|---|
| **Direct** | `Translated = Local * FX Rate` | Direct calculation with current period value and rate. Used for Assets/Liabilities by default. |
| **Periodic** | `Translated = Prior Translated + [(Current Local - Prior Local) * Current Rate]` | Weighted average method that considers prior periods. Used for Revenue/Expenses by default. |

#### Translation Algorithm Types in the Cube

- **Standard:** Uses the FX Rate Types assigned to the Cube/Scenario- **Standard Using Business Rules for FX Rates:** Allows Business Rules to specify different rates by intersection. Unspecified intersections use Standard. Examples:
  - Translate Actual at current year Budget rates
  - Apply different rates to certain Accounts
  - Rate already exists in FX Rate table under another Rate Type/Time; the BR determines it dynamically
  - Reduces maintenance by eliminating the need to copy or duplicate rates
- **Custom:** Translation entirely via Business Rules (rare)

#### Lock FX Rates

- Allows locking FX Rate Types by Time to prevent accidental changes
- File Loads or XFSetRate functions fail if an FX Rate Type is locked
- Security: Roles **ManageFXRates**, **LockFxRates**, **UnLockFxRates** in Application > Security Roles
- Administrators have full rights by default
- Task Activity is generated to audit locks/unlocks

#### Alternate Input Currency (Override in Flow Dimension)

To enter data in a currency different from the Entity's local currency:

1. In the Account: `Use Alternate Input Currency In Flow = True`
2. In the Flow Member: configure **Flow Processing Type**:

| Flow Processing Type | Description | Requires Account Setting |
|---|---|---|
| **Is Alternate Input Currency** | Only for Accounts configured with Use Alternate Input Currency = True | Yes |
| **Is Alternate Input Currency for All Accounts** | For all Accounts | No |
| **Translate using Alternate Input Currency, Input Local** | Overwrites translated value with local currency amount | Yes |
| **Translate using Alternate Input Currency, Derive Local** | Overwrites and derives local value based on rate. Do not use in trial balance. | Yes |

3. **Alternate Input Currency:** Select the currency (USD, EUR, etc.)
4. **Source Member for Alternate Input Currency:** The Flow member from which to take the value to overwrite

---

### Objective 201.1.4: Dimension Member Configuration

#### The 18 Cube Dimensions

| Token | Dimension | Type |
|---|---|---|
| E# | Entity | Configurable |
| S# | Scenario | Configurable |
| A# | Account | Configurable |
| F# | Flow | Configurable |
| U1#-U8# | User Defined 1-8 | Configurable |
| P# | Parent | Derived (from Entity) |
| I# | Intercompany | Derived (from Entity) |
| T# | Time | Configurable pre-app |
| C# | Consolidation | Via App Properties |
| O# | Origin | System-defined |
| V# | View | System-defined |

#### Dimension Library Navigation

![Dimension Library with toolbar and search options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p537-2786.png)

Dimensions are created and maintained in **Application > Cube > Dimensions**. The toolbar icons include:
- Create Dimension / Delete Dimension / Save / Rename / Move
- Create Member / Delete Member / Rename Member / Save Member
- Add Relationship / Remove Relationship
- Search Hierarchy / Collapse Hierarchy
- Navigate to Security

The available tabs are: **Members**, **Member Properties**, **Relationship Properties**, **Dimension Properties**, **Grid View**.

#### Entity Dimension - Detailed Properties

![Entity Member Properties showing all categories](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p542-2816.png)

**General:**
- Dimension Type, Dimension, Member Dimension, Id (auto-assigned, immutable), Name, Alias (alternative names, comma-delimited)

**Security:**
- Display Member Group, Read Data Group, Read Data Group 2, Read and Write Data Group, Read and Write Data Group 2
- Use Cube Data Access Security (True/False)
- Cube Data Cell Access Categories, Cube Conditional Input Categories, Cube Data Management Access Categories

**Settings:**
- **Currency:** Entity's local currency
- **Is Consolidated:** True = data consolidates to Parent; False = grouping only (improves performance)
- **Is IC Entity:** True = visible in IC Dimension for intercompany transactions. Both entities must have True to transact with each other.

**Vary By Cube Type:**
- Flow Constraint, IC Constraint, UD1-UD8 Constraints
- IC Member Filter: List of allowed IC partners
- UD1-UD8 Default: Assigns a default member to the Entity (data is directed to EntityDefault)

![Example of EntityDefault in UD with automatic cost center assignment](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p562-7957.png)

**Vary By Scenario Type:**
- **Sibling Consolidation Pass:** For holding companies (Equity Pickup). Pass 2+ forces order. Default = Pass 1.
- **Sibling Repeat Calculation Pass:** For circular ownership. Default = Use Default (no repeat).
- **Auto Translation Currencies:** List of currencies (comma-separated) for translation to siblings.

**Vary By Scenario Type and Time:**
- **In Use:** False deactivates the Entity (preserves historical data, ignored during Consolidation)
- **Allow Adjustments:** True (default) = enables Journals module for AdjInput
- **Allow Adjustments From Children:** True (default) = allows OwnerPostAdj and OwnerPreAdj from direct children
- **Text 1-8:** Custom attributes for Business Rules and Member Filters (can vary by Scenario Type and Time)

**Relationship Properties:**
- **Percent Consolidation:** Consolidation percentage to parent (100 = 100%). Can vary by Scenario Type and Time.
- **Percent Ownership:** Ownership percentage (used by Business Rules, does not affect Consolidation by itself).
- **Ownership Type:** Full Consolidation, Holding, Equity, Non-Controlling Interest, Custom (it is just a label for BRs).
- **Text 1-8:** Additional attributes in relationships.
- **Parent Sort Order:** Determines the default parent when there are multiple hierarchies.

#### Account Dimension - Detailed Properties

![Account Dimension in the Dimension Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p582-2954.png)

**Settings:**

![Account Settings showing Account Type, Formula Type, Allow Input, Is Consolidated](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p584-2968.png)

- **Account Type:** Determines how it aggregates to a Parent:
  - **Group:** Organizational only, does not aggregate data
  - **Revenue:** Aggregates as positive
  - **Expense:** Aggregates as negative (does not need to be entered as negative)
  - **Asset:** Asset tag
  - **Liability:** Liability tag (aggregates negative). **There is NO Equity type; use Liability for equity.**
  - **Flow:** Income statement-type value, has periodic and YTD. Not translated.
  - **Balance:** Balance sheet-type value at a point in time. Not translated.
  - **BalanceRecurring:** Balance sheet that does not change over time (opening balance). Not translated.
  - **NonFinancial:** Informational (headcount, square footage). Legacy. Not translated.
  - **DynamicCalc:** Calculates on-the-fly, does not need other formulas to execute.

- **Formula Type:** Determines formula behavior:
  - **FormulaPass1-16:** Controls execution order in the DUCS
  - **DynamicCalc:** Calculates every time it is displayed, does not store results
  - **DynamicCalcTextInput:** Dynamic + allows text input (Annotations)

- **Allow Input:** True (default) allows input. **False** = read-only. Set to False on Parents and calculated accounts.

- **Is Consolidated:**
  - **Conditional (True if no Formula Type) (default):** If it has a Formula Type, it is **NOT consolidated**. If it has no Formula Type, it is consolidated.
  - **True:** Always consolidated, regardless of Formula Type. **Change to True if you want to consolidate calculated data.**
  - **False:** Never consolidated. Improves performance.

- **Is IC Account:**
  - **Conditional (True if Entity not same as IC):** Most common. Prevents an Entity from recording IC with itself.
  - **True:** Always IC (allows IC with itself)
  - **False (default):** Not an IC account.

- **Use Alternate Input Currency In Flow:** True to use historical currency override.
- **Plug Account:** Select account for uneliminated IC transactions.

**Aggregation:**

![Aggregation settings by dimension](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p589-2981.png)

- Enable/disable aggregation by dimension: Entity, Consolidation, Flow, Origin, IC, UD1-UD8
- **Aggregation Weight (Relationship Properties):** Controls aggregation weight to parent.
  - Default = 1 (100%)
  - 0 = does not aggregate (avoids double counting in multiple hierarchies)
  - 0.50 = aggregates 50%

**Vary By Scenario Type and Time:**
- **In Use:** True/False (deactivate account without losing history)
- **Formula / Formula for Calculation Drill Down:** Formulas can vary by Scenario Type and Time
- **Adjustment Type:** Not Allowed, Journals, Data Entry
- **Text 1-8:** Custom attributes

#### Scenario Dimension - Detailed Properties

**Available Scenario Types (20+):**
Actual, Forecast, Operational, Variance, ScenarioType1-8, Administration, Budget, Control, Flash, FXModel, History, LongTerml, Model, Plan, Sustainability, Target, Tax, Default

- Scenario Type names are **functionally irrelevant** - they do not define system behavior
- The names and number of Scenario Types cannot be changed
- Multiple Scenarios can share the same Scenario Type

![Scenario Security settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p568-2904.png)

**Workflow Settings:**

![Scenario Workflow settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p569-2908.png)

- **Use in Workflow:** True/False. False = not visible in Workflow (but data can still be entered via Forms/Excel Add-In)
- **Workflow Tracking Frequency:** All Time Periods, Monthly, Quarterly, Half Yearly, Yearly, Range. **Cannot be changed if there are processed steps.**
- **Number of No Input Periods Per Workflow Unit:** Read-only periods (e.g.: first 3 months copied from Actual)

**Main Settings:**

![Scenario Settings with Input Frequency, Default View, etc.](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p571-2913.png)

- **Scenario Type:** Assigns the type
- **Input Frequency:** Monthly, Quarterly, Half Yearly, Yearly (can vary by year)
- **Default View:** Periodic or YTD (controls calculations and data clearing)
- **Retain Next Period Data Using Default View:** True = future periods change if the prior period is altered
- **Clear Calculated Data During Calc:** True (default) clears calculated data during the DUCS. False = clear manually.
- **Use Two Pass Elimination:** Enables `IsSecondPassEliminationCalc` in Business Rules.
- **Consolidation View:** Periodic or YTD. YTD improves Consolidation performance.

#### Flow Dimension - Detailed Properties

![Flow Dimension with simple and complex hierarchy](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p595-3005.png)

**Settings:**
- **Switch Sign:** True changes the data sign for the Flow member
- **Switch Type:** True changes the data type based on Account attribute (e.g.: Asset to Revenue). Useful for roll forward accounts.
- **Formula Type / Allow Input / Is Consolidated:** Same as Account

**Flow Processing:** For alternate currency (see FX Rates section above)

![Flow Processing settings for Alternate Input Currency](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p598-3026.png)

#### User Defined Dimensions 1-8

![UD Dimensions in the Dimension Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p602-3040.png)

- 8 dimensions available; not all need to be used
- **Assign larger/more significant dimensions to UD1 or UD2** (performance based on stored records, not on possible intersections)
- Each UD has an **EntityDefault** member (assignable per Entity in Vary By Cube Type)
- UD2-UD8 also have **UD1Default** (usable in Constraints and Transformation Rules)

**Is Attribute Member:** Converts the UD member into a dynamically calculated attribute:
- Requires: Source Member For Data, Expression Type, Related Dimension Type, Related Property, Comparison Text, Comparison Operator
- **Does not store data;** calculates on-the-fly
- **Caution:** More than ~2000 attribute members impact performance
- Does not impact Data Unit size except when Related Dimension = Entity and consolidation occurs
- Cannot receive input or have formulas
- Attribute data cannot be exported to files
- Attribute results cannot be locked for data integrity
- Can be referenced in Business Rules via `includeUDAttributeMembersWhenUsingAll = True` in GetDataBuffer

#### Dimension Member Names

- Unique within a Dimension Type (e.g.: there cannot be two GrossIncome in Account dimensions)
- Maximum 500 characters
- Use underscores instead of spaces and periods
- If they include spaces or periods, use brackets: `E#[Quebec.City]`
- **Restricted characters:** & * @ \ { } [ ] ^ , = ! > < - # % | + ? " ; /
- **Alias:** Alternative names (comma-delimited). Cannot be duplicated within the same Dimension Type.

**Reserved words:** Account, All, Cons, Consolidation, Default, DimType, Entity, EntityDefault, Flow, IC, None, Parent, POV, Root, RootXXXDim, Scenario, Time, UD1-UD8, UD1Default, Unknown, View, WF, Workflow, XFCommon, Origin

#### Origin Dimension (System-defined)

![Origin Dimension hierarchy](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p624-3118.png)

Fixed hierarchy:
- **Top** > BeforeElim > BeforeAdj > **Import** (loaded data), **Forms** (form data)
- **Adjustments** > **AdjInput** (journals/forms), **AdjConsolidated** (consolidated from children)
- **Elimination** > **DirectElim** (informational, at first common parent), **IndirectElim** (informational, prior eliminations)

#### View Dimension (System-defined)

![View Dimension hierarchy](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p627-3126.png)

- Data is always stored in **V#YTD** in the database
- Periodic, MTD, QTD, HTD are calculated dynamically (included calculations)
- **CalcStatus:** Shows the Data Unit's calculation status
- Members for text: Annotation, Assumptions, AuditComment, Footnote, VarianceExplanation

#### Time Profiles

![Time Profile configuration with Profile tab and Time Periods tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch08-p631-3140.png)

- **Standard Time Profile:** January-December calendar (cannot be deleted or renamed)
- Additional Time Profiles are created for different fiscal years
- **Profile Properties:**
  - **Name:** Maximum 100 characters, free-form
  - **Fiscal Year Start Date:** Fiscal year start date
  - **Fiscal Year is First Period's Calendar Year:** True/False (default False)
  - **Fiscal Year Month Type:** Calendar Month, Calendar Quarter, Fixed Weeks 4-4-5 / 4-5-4 / 5-4-4, Custom Start Dates
- **Format variables:** `|fyfy|` = 4-digit fiscal year, `|cycy|` = 4-digit calendar year, `|fy|`/`|cy|` = 2 digits
- **CRITICAL:** The Time Dimension is configured before creating the application and **cannot be altered afterwards**. A new application must be created if a different time dimension is required.

---

### Objective 201.1.5: Stored and Dynamic Calculations

#### Stored Calculations (in the DUCS)

They execute within the Data Unit Calculation Sequence (DUCS) when Calculate or Consolidate is launched. The resulting data **is stored** in the DataRecord database tables (tables `DataRecord1996` to `DataRecord2100`, one per year).

**Finance Business Rules:**
- Assigned to the Cube (up to 8 Business Rules)
- Have access to **Finance Function Types:**
  - `FinanceFunctionType.Calculate` (only in C#Local)
  - `FinanceFunctionType.CustomCalculate`
  - `FinanceFunctionType.Translate`
  - `FinanceFunctionType.ConsolidateShare`
  - `FinanceFunctionType.ConsolidateElimination`
  - `FinanceFunctionType.DataCell` (for Cube Views)
  - `FinanceFunctionType.MemberList`
  - `FinanceFunctionType.ConditionalInput`
  - `FinanceFunctionType.OnDemand`
- Advantages: Control via Finance Function Types, centralized logic, shared variables

![Finance Business Rule Editor showing code structure](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-720.png)

![Available Finance Function Types in Business Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-721.png)

**Member Formulas:**
- Assigned in the Formula property of Account, Flow, or UD Members
- **Formula Pass (1-16)** defines the execution order within the DUCS
- Formulas in the same Pass and same dimension execute in parallel (multi-threaded)
- Can vary by Scenario Type and/or Time without additional code
- Only support `FinanceFunctionType.Calculate` and `FinanceFunctionType.DynamicCalc`

![Member Formula Editor showing formula with Scenario Type and Time variation](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p32-728.png)

- **Is Consolidated:** Must be **True** if you want calculated data to consolidate (the default "Conditional" does NOT consolidate data with Formula Type)

![Member Formula with assigned Formula Pass](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p41-789.png)

**Difference between Consolidation vs Calculation:**
- **Calculation:** Only executes the DUCS for the selected Data Unit
- **Consolidation:** DUCS + Aggregation to Parent Entities + Currency Translations + IC Eliminations

#### Custom Calculate (outside the DUCS)

![Custom Calculate Step in Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-844.png)

- Executed only via **Data Management Step** (Custom Calculate Step Type)
- Does not execute the entire DUCS; only the defined script
- **Does not consider Calculation Status;** only the explicitly defined Data Units are processed
- Requires defining **Business Rule Name** and **Function Name**
- Optional parameters (Name Value Pairs) and Account-level Dimension POV

![Custom Calculate with Data Unit definition and parameters](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-845.png)

- Requires a **clear data script** at the beginning (`api.Data.ClearCalculatedData`) - not cleared automatically
- Data can be marked as **isDurable = True** to protect it from DUCS clearing
- Ideal for Planning; allows "surgical" calculations without reprocessing everything
- **Cannot be called** from Cube Views (only from DM or Workflow Process Step via DM)

**Data Storage Types:**
- Input, Journal, Calculation, DurableCalculation, Consolidation, Translation, StoredButNoActivity

#### Dynamic Calculations

![Dynamic Calc with Formula Type and Account Type = DynamicCalc](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch06-p118-1346.png)

- **Do not execute** in the DUCS or via Custom Calculate
- Calculated **on-the-fly** only when queried in a report (Cube View, Excel Quick View)
- Configured with `Formula Type = DynamicCalc` in the Member
- For Accounts: **Account Type AND Formula Type** must both be DynamicCalc
- **Do not store results;** calculate every time they are displayed
- **DynamicCalcTextInput:** Dynamic formula + allows text input (Annotations)
- Ideal use: Ratios, percentages, derived metrics that do not require storage
- **Caution:** Excessive use impacts report performance
- Do not execute in: Data Exports, Data Buffers, Data Unit Method Queries

#### DUCS vs. Custom Calculate - When to use each

| Characteristic | DUCS (Consolidation) | Custom Calculate |
|---|---|---|
| Scope | Entire Data Unit | Only the defined script |
| Automatic clearing | Yes (Clear Calculated Data) | No (manual with script) |
| Calculation Status | Respects OK/CN | Does not consider |
| Dependencies | Automatic | Manual |
| Data Storage | Calculation | DurableCalculation (if isDurable=True) |
| Recommended use | **Consolidation solutions** | **Planning solutions** |
| Trigger | DM, Cube View, Dashboard, Workflow | Data Management only |
| Performance | Entire DUCS every time | Only what is needed |

**General rule:** Consolidation primarily uses DUCS; Planning primarily uses Custom Calculate + `C#Aggregated`.

#### C#Aggregated

![C#Aggregated in DM showing how to specify the Consolidation Member](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p51-859.png)

- Simplified version of the Consolidation algorithm
- DUCS only executes in Base Entities
- **Does not perform:** IC Eliminations, Share, Ownership Calculations, Parent Journals
- Only Direct Method Translation
- Data is stored in `C#Aggregated` in Parent Entities
- Can be up to **90% faster** than normal Consolidation

![Data in C#Aggregated vs C#Local showing that Local has no data in Parent](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p195-1917.png)

#### Data Buffers

A Data Buffer is a subset of cells within a Data Unit. It is a VB.NET object with properties and methods.

![Anatomy of a Data Buffer showing Common Members, Cell POV, Amount, Status](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch03-p66-956.png)

- Declared with `api.Data.GetDataBuffer()` or `api.Data.GetDataBufferUsingFormula()`
- **Common Members:** Dimensions shared by all buffer rows (the non-Data Unit Dimensions)
- Useful functions: `FilterMembers`, `RemoveMembers`, `RemoveNoData`, `RemoveZeros`
- **Always** use `RemoveZeros` or `RemoveNoData` when declaring a Data Buffer (improves performance)
- `FilterMembers` and `RemoveZeros` only work with `GetDataBufferUsingFormula` (not with `GetDataBuffer`)

#### Cross-referencing between Business Rules

- Business Rules can be called from other Rules or Member Formulas
- Configure `Contains Global Functions for Formulas = True` in the shared Rule

![Configuration of Contains Global Functions for Formulas = True](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-750.png)

- Instantiate: `Dim sharedFinanceBR As New OneStream.BusinessRule.Finance.RuleName.MainClass`

---

## Critical Points to Memorize

### Cube Properties
- The Cube name **cannot be changed** after creation (maximum 100 characters)
- `Is Top Level Cube for Workflow = True` on only **one** Cube
- Once a Scenario Type has data, a new Cube Root Workflow cannot be created nor can its suffix be changed without Reset Scenario
- Consolidation Algorithm Type default: **Standard** (Calc-on-the-fly Share)
- Translation Algorithm Type default: **Standard**
- Rule Type default: Revenue/Expenses = **Periodic**, Assets/Liabilities = **Direct**
- Up to **8 Business Rules** can be assigned to the Cube
- `Clear Calculated Data During Calc` default = **True** in the Scenario
- NoData Calculate Settings: False by default (except Local). True when copying data from another period/scenario.

### Cube Dimensions
- Entity and Scenario are only assigned in the **(Default)** Scenario Type
- **Always assign RootXXXDim** to unused Dimension Types (do not leave "Use Default")
- Change from RootXXXDim to a specific Dimension = **irreversible** if data exists
- (Use Default) with data = **permanently locked**
- Once assigned and with data, the Dimension **cannot be changed** without Reset Scenario

### Data Units
- Definition: Cube + Entity + Parent + Consolidation + Scenario + Time
- Data Units > 1 million records = inspection needed
- Data Units > 2 million = red flag
- The DUCS is **all or nothing** (all steps, every time)
- The DUCS can execute up to **7 times** per Entity during Consolidation
- Sibling Entities are processed in **parallel**
- Use **Calculate With Logging** to diagnose (adds significant time)

### FX Rates
- Stored in the **System Cube** as a central repository
- 4 predefined Rate Types: Average, Opening, Closing, Historical
- Custom Rate Types can be created
- **Direct:** Value * Rate (current period) - Assets/Liabilities by default
- **Periodic:** Considers prior period translation - Revenue/Expenses by default
- **Lock FX Rates:** Prevents accidental changes; audited in Task Activity
- Scenario can override Cube FX settings with `Use Cube FX Settings = False`
- **Constant Year for FX Rates** in Scenario for Constant Currency

### Dimension Members
- Account Type: **There is no Equity type** - use Liability
- `Is Consolidated = Conditional` means calculated data is **NOT consolidated** (change to True if desired)
- DynamicCalc is not stored; calculates every time it is queried
- For Accounts: DynamicCalc requires **Account Type AND Formula Type** = DynamicCalc
- `Allow Input = False` on Parents and calculated accounts
- `Aggregation Weight = 0` avoids double counting in multiple hierarchies
- Names unique within Dimension Type; maximum 500 characters
- Data always stored in **V#YTD** (even if entered in Periodic)
- UD Attribute Members: ~2000 = performance limit. They do not store data.
- Time Dimension is configured **before** creating the application and **cannot be altered afterwards**

### Calculations
- **DUCS:** All or nothing, fixed order, automates clearing and dependencies
- **Custom Calculate:** Only via Data Management, requires manual clear script, allows isDurable
- **Dynamic Calc:** On-the-fly, not stored, only when querying reports
- **Formula Pass 1-16:** Execution order within the DUCS
- Formulas in the same Pass and same Dimension execute in **parallel** (multi-threaded)
- Business Rules execute **sequentially** as written
- Consolidation = DUCS + Aggregation + Translation + Eliminations
- Calculation = DUCS only
- Planning --> Custom Calculate + C#Aggregated (up to 90% faster)
- Consolidation --> Full DUCS

---

## Source Mapping

| Objective | Book/Chapter |
|----------|---------------|
| 201.1.1 | Design Reference Guide - Chapter 8: Cubes (Cube Properties, Cube Dimensions, Data Access, Integration); Foundation Handbook - Chapter 3: Design and Build (Cube Design Options) |
| 201.1.2 | Finance Rules - Chapter 1: Finance Engine Basics (Data Unit, DUCS, Parallel Processing, Calculation Status); Finance Rules - Chapter 2: Cube Data (Data Unit, Data Buffers); Finance Rules - Chapter 7: Troubleshooting (Calculate With Logging, Performance); Foundation Handbook - Chapter 3 (Data Unit sizing, extensibility) |
| 201.1.3 | Design Reference Guide - Chapter 8: Cubes (FX Rates, Lock FX Rates, Scenario FX Rates, Flow Processing); Finance Rules - Chapter 1 (Translation Algorithm Types); Foundation Handbook - Chapter 3 (Flow Processing, Alternate Input Currency) |
| 201.1.4 | Design Reference Guide - Chapter 8: Cubes (Entity, Account, Scenario, Flow, UD Dimensions, Origin, View, Time Profiles, Attribute Members); Foundation Handbook - Chapter 3 (Dimension Design considerations) |
| 201.1.5 | Finance Rules - Chapter 1: Finance Engine Basics (DUCS, Custom Calculate, Dynamic Calculations, C#Aggregated, Business Rules vs Member Formulas); Finance Rules - Chapter 2: Cube Data (Data Buffers, Storage Types); Foundation Handbook - Chapter 8: Rules and Calculations (DUCS detail, performance comparison) |


---

# Section 2: Workflow (14% of the exam)

## Exam Objectives

- **201.2.1:** Apply the appropriate Workflow Name for a given situation
- **201.2.2:** Apply a Calculation/Consolidation in the Workflow
- **201.2.3:** Demonstrate understanding of Workflow Profile configuration
- **201.2.4:** Discuss the primary purpose of various less commonly used Workflow configurations

---

## Key Concepts

### Objective 201.2.1: Apply the appropriate Workflow Name for a given situation

#### What is the Workflow Engine

The **Workflow Engine** is the coordinator of all activity in the system. Its main responsibilities are:

- Protect the end user from the complexities of the multidimensional analytical model
- Manage and audit data collection
- Manage and enforce the quality process along with data certification
- Manage and intelligently coordinate the consolidation process
- Deliver real-time information and guided analysis
- Create a standardized user experience with customization capability via Workspaces

The primary reason for the Workflow's existence is to feed and maintain the Analytic Cubes. Therefore, before creating a Workflow hierarchy, at least one Cube must exist marked as **Is Top Level Cube For Workflow = True**.

![Workflow main screen in OnePlace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1928-7259.png)

#### What is the Workflow Name

The **Workflow Name** controls the tasks that users must complete in the Workflow. It is configured in **Workflow Settings** of each Input Child Profile. Tasks can vary by **Scenario** and **Input Type**.

Each Input Child type (Import, Form, Journal) has its own set of available Workflow Names.

![Workflow Name configuration in Profile properties](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p813-3695.png)

![Resulting tasks based on selected Workflow Name](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p813-3696.png)

#### Workflow Names for Import

| Workflow Name | Included Tasks | Use case |
|---|---|---|
| **Import, Validate, Load** | Import file, Validate mappings/intersections, Load to Cube | Basic load without certification or processing |
| **Import, Validate, Load, Certify** | Import + Validate + Load + Certification | Load with simple certification sign-off |
| **Import, Validate, Process, Certify** | Import + Validate + Process Cube + Certification | Load with automatic calculation before certifying |
| **Import, Validate, Process, Confirm, Certify** | Import + Validate + Process + Confirmation + Certification | Complete process with Data Quality checks |
| **Central Import** | For centralized imports (corporate) | When corporate loads data to unrelated entities |
| **Workspace** | Uses a custom Dashboard as the interface | Planning workflows with custom interface |
| **Workspace, Certify** | Custom Dashboard + Certification | Workspace with certification step |
| **Import Stage Only** | Only imports data to Stage (does not load to Cube) | Exporting data from OneStream to an external system |
| **Import, Verify Stage Only** | Imports and validates in Stage only | Validation without loading to Cube |
| **Import, Verify, Certify Stage Only** | Imports, validates in Stage and certifies | Validation and certification without loading to Cube |

**Workflow Names for Direct Load (without Stage storage):**

| Workflow Name | Description |
|---|---|
| **Direct Load** | Direct in-memory load without Stage storage |
| **Direct Load, Certify** | Direct Load with certification |
| **Direct Load, Process, Certify** | Direct Load with processing and certification |
| **Direct Load, Process, Confirm, Certify** | Complete Direct Load with confirmation |

![Direct Load Workflow configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p850-3799.png)

#### Workflow Names for Forms

| Workflow Name | Included Tasks | Use case |
|---|---|---|
| **Form Input** | Data entry in form only | Simple entry without additional steps |
| **Form Input, Certify** | Entry + Certification | Entry with certification sign-off |
| **Form Input, Process, Certify** | Entry + Process Cube + Certification | Entry with calculation and certification |
| **Form Input, Process, Confirm, Certify** | Entry + Process + Confirmation + Certification | Complete process with DQ |
| **Pre-Process, Form Input** | Pre-process (prior calculation) + Entry | Pre-Process calculates data before showing the form |
| **Pre-Process, Form Input, Certify** | Pre-process + Entry + Certification | Pre-calculation + entry + certification |
| **Pre-Process, Form Input, Process, Certify** | Pre-process + Entry + Process + Certification | Complete cycle with pre-calculation |
| **Pre-Process, Form Input, Process, Confirm, Certify** | Pre-process + Entry + Process + Confirm + Certification | Most complete cycle available |
| **Central Form Input** | For centralized forms | Corporate controlling forms for other entities |
| **Workspace** | Custom Dashboard | Fully customized interface |
| **Workspace, Certify** | Custom Dashboard + Certification | Workspace with certification |

#### Workflow Names for Journals (Adjustments)

| Workflow Name | Included Tasks | Use case |
|---|---|---|
| **Journal Input** | Journal entry only | Simple entries without validation |
| **Journal Input, Certify** | Entry + Certification | Entries with sign-off |
| **Journal Input, Process, Certify** | Entry + Process + Certification | Entries with post-entry calculation |
| **Journal Input, Process, Confirm, Certify** | Entry + Process + Confirm + Certification | Complete cycle with DQ |
| **Central Journal Input** | For centralized journals | Corporate adjusting unrelated entities |
| **Workspace** | Custom Dashboard | Custom interface |
| **Workspace, Certify** | Custom Dashboard + Certification | Workspace with certification |

#### Workflow Names for Review Profiles

Review Profiles do not receive data; they only review, confirm and certify:

| Workflow Name | Description |
|---|---|
| **Process, Certify** | Executes calculation/consolidation and certifies |
| **Process, Confirm, Certify** | Executes calculation, verifies confirmation rules and certifies |

#### How to choose the correct Workflow Name (decision guide)

| Scenario | Recommended Workflow Name | Reason |
|---|---|---|
| Data load only without subsequent validation | Import, Validate, Load | Minimum steps for basic load |
| Load with simple certification | Import, Validate, Load, Certify | Adds accountability with sign-off |
| Load with automatic calculation | Import, Validate, Process, Certify | Process Cube executes DUCS automatically |
| Complete process with Data Quality | Import, Validate, Process, Confirm, Certify | Includes Confirmation Rules for DQ |
| Planning with forms | Pre-Process, Form Input, Process, Confirm, Certify | Pre-Process calculates data before showing the form |
| Review only without loading | Process, Certify or Process, Confirm, Certify | For Review Profiles |
| Custom Dashboard | Workspace or Workspace, Certify | Fully custom interface |
| Corporate controlling unrelated entities | Central Import / Central Form Input / Central Journal Input | Requires Can Load Unrelated Entities = True |
| High-frequency data without audit trail | Direct Load (appropriate variant) | In-memory, without Stage storage |
| Data export to external system | Import Stage Only | Only imports to Stage for data retrieval |

#### Workspace Dashboard Name

When a Workflow Name containing "Workspace" is selected, the **Workspace Dashboard Name** property allows defining any Dashboard from the Application Dashboards page as the Workflow interface. This is the basis for creating fully customized Workflows, commonly used in:

- People Planning
- OneStream Financial Close
- Task Manager
- Actor Workspaces

#### Workflow Tasks in OnePlace - Step-by-step procedure

The Workflow tasks in typical order:

##### 1. Import

The user imports data to the **Stage Engine**. The file is parsed into a clean tabular format.

![Import icon in Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1932-7269.png)

**Available Load Methods:**

| Method | Behavior | When to use |
|---|---|---|
| **Replace** | Deletes data from previous Source ID, replaces with new file | Standard load - replacement by Source ID |
| **Replace (All Time)** | Replaces all Workflow Units in the Workflow View | Multi-period load |
| **Replace Background (All Time, All Source IDs)** | Replaces everything in background thread. **Always deletes ALL Source IDs** | Only if there are no partial Source IDs |
| **Append** | Adds new rows without modifying existing data | When adding additional records |

![Load Method options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7272.png)

**Additional Import icons:**

![Re-Import icon - repeat import](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7273.png)

![Clear Stage icon - clear Stage data](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7274.png)

**Load controls:**

- ![Enforce Global POV](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1934-7281.png) - When displayed, loading is limited to the Global POV
- ![No loads before Workflow Year](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1934-7282.png) - Data cannot be imported to periods before the Workflow year

##### 2. Validate

Verifies two things: (1) that each data item has a map (Transformation Rule) and (2) that the dimension combination is valid in the cube.

![Validate step in Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7286.png)

**Transformation Errors:** The dimension with error, the Source Value and the Target Value (Unassigned). The system suggests a One-To-One Transformation Rule by default.

![Transformation errors screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7288.png)

**Intersection Errors:** Something is not correct with the complete data intersection (e.g.: a Customer mapped to a Salary Grade Account). To correct: edit the rule, save and click **Retransform**.

##### 3. Load

Loads the clean data from Stage to the **Analytic Engine** (Consolidation Engine).

![Load step in Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1936-7293.png)

![Load progress dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1937-7296.png)

##### 4. Pre-Process

Same as Process, can be used as the first step in the Forms channel. Executes Calculation Definitions before showing the form to the user.

##### 5. Input Forms

Manual entry with forms. Options under Workflow Forms: **Required** and **Optional**.

![Input Forms screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1938-7303.png)

Key functionalities:
- **Complete Form:** Marks a form as completed
- **Revert Form:** Reopens a form to make changes
- **Calculate:** Executes calculation for a specific form
- **Open Excel Form:** Exports form to Excel, completes and re-imports
- **Attachments:** Attach supplementary files
- **Import Form Cells:** Load Form data via Excel template or CSV

##### 6. Input Journals

Journal entry. Options under Workflow Journal Templates: **Required** and **Optional**.

![Journals toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1000-7970.png)

**Journal flow with full security:**
1. **Create** - Create journal (free-form or from template)
2. **Submit** - Submit for approval
3. **Approve** - Approve (or reject)
4. **Post** - Post to Cube

**Quick Post:** One-click option that combines Submit + Approve + Post. Available when the user has all security roles and there are no self-post/self-approval restrictions.

![Journal options in Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1000-7971.png)

**Validate Journals:** Verifies that selected members are valid intersections based on Dimension Library constraints.

![Journal validation](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1004-7982.png)

##### 7. Process

Executes **Process Cube** according to the configured Calculation Definitions.

![Process Cube icon](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1006-7985.png)

##### 8. Confirm

Executes the **Confirmation Rules**. Two types of results:
- **Warning:** Does not block the process, but alerts the user
- **Error:** Blocks the process, the step turns red

![Confirm icon](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1006-7984.png)

##### 9. Certify

Final certification with optional questionnaires.

![Certify icon with questionnaire](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1007-7987.png)

![Quick Certify - one-click certification](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1007-7989.png)

**Dependent Status:** Shows the status of each required Workflow task, including Profile Name, input types, Workflow Channel, status, percentage OK/In Process/Not Started/Errors, and last activity.

![Dependent Status screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1008-7990.png)

#### Workflow Icons and states

![Workflow status icons](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1940-7317.png)

- **Blue:** Task pending completion
- **Green:** Task completed successfully
- **Red:** Error that must be corrected before continuing
- **Gray:** Central Input task (corporate)

#### Right-Click Options in Workflow

| Option | Description |
|---|---|
| **Status and Assigned Entities** | Shows the Workflow Status of each Origin process |
| **Audit Workflow Process** | Audit of each task with date, user, duration, errors |
| **Lock/Lock Descendants** | Locks a period or Origin process |
| **Unlock/Unlock Descendants** | Unlocks (requires UnlockWorkflowUnit Security Role) |
| **Edit Transformation Rules** | Navigates to Transformation Rules to correct errors |
| **Clear All Import/Forms Data From Cube** | Clears Cube data but data remains in Stage |
| **Corporate Certification Management** | Unlock/uncertify ancestors or lock/certify descendants |
| **Corporate Data Control** | Preserve data and restore if necessary |

#### Multi-Period Processing

Clicking on the Year in the Navigation Pane activates Multi-Period Processing options that allow executing multiple Workflow tasks for one or multiple periods at once.

![Multi-Period Processing](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch11-p1008-7991.png)

---

### Objective 201.2.2: Apply Calculation/Consolidation in the Workflow

#### Calculation Definitions

All Workflow Profile types (**except Cube Root**) can have Calculation Definitions. A Calculation Definition is a set of instructions that execute when the user presses **Process Cube** during the Workflow.

Calculation Definitions are configured in the **second tab** of the Workflow Profile.

![Calculation Definitions tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p826-3738.png)

#### Calculation options in Process Cube

| Calc Type | Description | Use |
|---|---|---|
| **No Calculate** | Does not execute direct calculation; allows assigning Data Management Sequences | For custom processing via Filter Value |
| **Calculate** | Executes DUCS for the selected Data Unit | Basic entity calculation |
| **Calculate with Logging** | Calculate + detailed logging | Debugging calculations |
| **Translate** | Executes currency translation | FX Conversion |
| **Translate with Logging** | Translate + detailed logging | Debugging translation |
| **Consolidate** | Calculate + aggregation to Parents + Translations + Eliminations | Complete consolidation |
| **Consolidate with Logging** | Consolidate + detailed logging | Debugging consolidation |
| **Force Calculate** | Calculate without checking Calculation Status | When changes are not detected by status |
| **Force Calculate with Logging** | Force Calculate + detailed logging | Debugging |
| **Force Consolidate** | Consolidate without checking Calculation Status | Forces complete reprocessing |
| **Force Consolidate with Logging** | Force Consolidate + detailed logging | Debugging |

![Calculation Definitions configuration with options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p823-3729.png)

#### Entity Placeholder Variables by Profile type

##### Review Profile:
| Variable | Description |
|---|---|
| **Dependent Entities** | All Entities assigned to dependent Workflow Profiles, including Named Dependents |
| **Named Dependent Filter** | Filters specific Entities from Named Dependents (necessary because Shared Services typically have entities spanning multiple Review Profiles) |

##### Input Parent and Input Child:
| Variable | Description |
|---|---|
| **Assigned Entities** | Entities directly assigned to the Workflow Profile |
| **Loaded Entities** | Entities imported by Import Child Workflow Profiles dependent on the Input Parent |
| **Journal Input Entities** | Entities adjusted with journals by the dependent Adjustment Children |
| **User Cell Input Entities** | Entities affected by data entry from the user executing the Workflow (returns a different list per user; used in Central Input) |

**Input Child:** If it has no explicit Calculation Definition, it inherits from the Input Parent.

#### Confirmed Switch

Each Calculation Definition has a **Confirmed Switch**:
- Determines whether the defined Entities should be submitted to the **Confirmation Workflow Step**
- Gives control over which Entities are validated by Confirmation Rules
- Auto-assignment of Entities is done via (Assigned Entities) or (Loaded Entities)

![Calculation Definitions with Confirmed checkbox](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p826-3737.png)

#### Filter Value for Data Management

A Data Management Sequence can be assigned to Calculation Definitions:
1. Enter the Sequence name in **Filter Value**
2. Set **Calc Type = No Calculate**
3. Create a **DataQualityEventHandler** Extensibility Business Rule that reads the Sequence name and executes it during Process Cube

#### Consolidate vs Calculate vs Force - Detailed Comparison

| Aspect | Calculate | Consolidate | Force Calculate | Force Consolidate |
|---|---|---|---|---|
| **Scope** | Selected Data Unit only | Data Unit + Parents + Translations + Eliminations | All Data Units without checking | Everything without checking |
| **Checks Calc Status** | Yes | Yes | No | No |
| **Efficiency** | High | High | Low (processes everything) | Low (processes everything) |
| **When to use** | Basic entity calculation | Complete consolidation | FX Rate/Member Formula changes post-calculation | Extremely large Data Units |
| **Risk** | Might miss changes not reflected in status | Might miss changes not reflected | Excessive processing | Excessive and redundant processing |

#### Calculation Definitions Best Practices

- Embedding Consolidate/Calculate in most Workflow Profiles reduces wait times for reviewers and corporate consolidations
- **Consolidate/Calculate (normal):** Recommended in most cases -- checks calc status first
- **Force Consolidate/Calculate:** Only use when:
  - FX Rates or Member Formulas were changed after the last calculation
  - Extremely large Data Units where checking calc status is more costly than recalculating
- For **Planning:** Consider Custom Calculate via Data Management in the Workflow (using No Calculate + Filter Value)
- Clearly define what type of calculation/consolidation/translation should execute and when

---

### Objective 201.2.3: Workflow Profile Configuration

#### Workflow Profile Types (8 types)

##### 1. Cube Root Profile

- Defines the Workflow structure for an entire Cube or a suffix group
- Requires a Cube with `Is Top Level Cube For Workflow = True`
- When created, automatically generates a **Default Input Profile** (prefix: CubeRootName_Default)
- Controls the **Workflow state:**

![Workflow Open state](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p796-3653.png)

| State | Behavior | Data access | Recommendation |
|---|---|---|---|
| **Open** | Workflow available, lock controlled individually | From memory (cache) -- high performance | Normal operating state |
| **Closed** | High-level lock, historical snapshot stored | From database -- performance penalty | Only for major changes or discontinued operations |

![Workflow Closed state](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p797-3656.png)

**IMPORTANT:** Closing a Workflow takes a snapshot of the hierarchy and stores it in a historical table. Data must be read from the database instead of cache. Only close for discontinuations.

##### 2. Review Profile

- Review checkpoint; **has no direct relationship to Entity or Origin Member**
- Can have Calculation Definitions
- **Named Dependents:** Unique capability to establish dependency with Input Parent Profiles that are NOT direct descendants

![Named Dependent example - Canada Clubs depends on Eagle Drivers](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p798-3659.png)

##### 3. Default Input Profile

- Created automatically with the Cube Root
- **Cannot be deleted**
- Entities not explicitly assigned to other Profiles are implicitly assigned here
- Entities cannot be explicitly assigned to the Default Input

##### 4. Parent Input Profile

- For adjustments to Parent Entities (only via Forms or AdjInput)
- **Import Child is created automatically but forced inactive** (Profile Active = False)
- Only allows Forms and Journals, not Import
- Parent Entities do not need to be assigned to Parent Input unless they require adjustments

##### 5. Base Input Profile

- The **most common type** (workhorse)
- Controls all data entry methods for Base Entities
- Has all 3 types of Input Children: Import, Forms, Journals

##### 6-8. Input Child Profiles (Import, Forms, Adjustment)

- Always base Members of the Workflow hierarchy
- Can only be children of Default, Parent or Base Input Profiles
- **Mapped directly to Origin Members:**

![Relationship between Input Children and Origin Members](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p801-7961.png)

| Input Child | Origin Member | Control |
|---|---|---|
| Import Child | `O#Import` | File imports, connectors, Excel templates |
| Forms Child | `O#Forms` | Manual entry, Excel XFSetCell, Cube Views |
| Adjustment Child | `O#AdjInput` | Manual journals, templates, loaded by Excel |

- **Profile Active = False** deactivates the entire input channel
- Reimporting data does not overwrite Forms data, and vice versa (isolation by Origin)

#### Workflow Profile Toolbar

![Workflow Profile toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p792-3624.png)

| Icon | Function |
|---|---|
| Create Cube Root | Starts a new Workflow Profile hierarchy |
| Create Child | Creates Review, Base Input or Parent Input under the current Profile |
| Create Sibling | Creates a sibling of the current Profile |
| Delete | Deletes the selected Profile and its children |
| Rename | Renames a Profile or Input Type |
| Move as Child/Sibling | Moves Profiles between hierarchy positions |
| Move Up/Down | Reorders siblings |
| Work with Templates | Navigates to the Workflow Templates screen |
| Update Input Children Using Template | Applies template to Input Children |

#### Profile Properties Tab - Complete Detail

##### General
- **Name:** Profile name
- **Description:** Brief description. **If added in the Default Scenario, it is displayed in the Workflow Profile POV dialog in OnePlace**

![Workflow Profile selection with description](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1931-7266.png)

##### Security - All Properties

| Property | Function | Applies to |
|---|---|---|
| **Access Group** | Users who see the Workflow Profile at runtime | All |
| **Maintenance Group** | Users who maintain/administer | All |
| **Workflow Execution Group** | Data loaders who execute Workflow | All |
| **Certification SignOff Group** | Certifiers who sign off on the Workflow | All |
| **Journal Process Group** | Users who process journals | Journals only |
| **Journal Approval Group** | Users who approve journals | Journals only |
| **Journal Post Group** | Users who post journals | Journals only |
| **Prevent Self-Post** | Prevents posting own journals | Journals only |
| **Prevent Self-Approval** | Prevents approving own journals | Journals only |
| **Require Journal Template** | Restricts creation of free-form journals | Journals only |

**Prevent Self-Post options:**
- **True:** Journal Post Group members cannot post their own journals. Admins retain Quick Post.
- **True (includes Admins):** Admins also cannot post their own journals.
- **False (default):** No restriction.

**Prevent Self-Approval options:**
- **True:** Journal Approval Group members cannot approve their own journals. Admins retain Quick Post.
- **True (includes Admins):** Admins also cannot approve their own journals. Auto Approved and Auto Reversing journals are excluded.
- **False (default):** No restriction.

**Require Journal Template:** When True, disables the Create Journal button and prevents file uploads without a template. System and Application admins can still create free-form journals.

##### Workflow Settings

| Property | Description |
|---|---|
| **Workflow Channel** | Additional security layer linked to Workflow Channels (Application > Workflow > Workflow Channels). Default: Standard |
| **Workflow Name** | The Workflow tasks (see objective 201.2.1) |
| **Workspace Dashboard Name** | Only works with Workflow Names that contain "Workspace" |

##### Integration Settings (Import) - All Properties

| Property | Description | Default |
|---|---|---|
| **Data Source Name** | Data source (Fixed Width, Delimited, DM Export, SQL Connector) | - |
| **Transformation Profile Name** | Mapping profile (Transformation Rules) | - |
| **Import Dashboard Profile Name** | Dashboards for the Import phase | - |
| **Validate Dashboard Profile Name** | Dashboards for the Validate phase | - |
| **Is Optional Data Load** | True adds "Complete Workflow" icon to complete without loading data | False |
| **Can Load Unrelated Entities** | True allows loading data to Entities not assigned to the Input Parent | False |
| **Flow Type No Data Zero View Override** | Override for Zero No Data in Flow Accounts | YTD/Periodic |
| **Balance Type No Data Zero View Override** | Override for Zero No Data in Balance Accounts | YTD/Periodic |
| **Force Balance Accounts to YTD View** | True forces YTD View for Balance Accounts during load | False |
| **Cache Page Size** | Cache page size in records | 20000 |
| **Cache Pages In-Memory Limit** | In-memory page limit | 200 |
| **Cache Page Rule Breakout Interval** | Pause interval to check if page is mapped | 0 |

![Integration Settings configuration for YTD/MTD](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p806-3679.png)

##### Form Settings
- **Input Forms Profile Name:** Form templates. Configured in Application > Data Collection > Form Templates.

##### Journal Settings
- **Journal Template Profile Name:** Journal templates. Configured in Application > Data Collection > Journal Templates.

##### Data Quality Settings

| Property | Description |
|---|---|
| **Cube View Profile Name** | Cube Views in the Analysis Pane (Process, Confirm, Certify) |
| **Process Cube Dashboard Profile Name** | Dashboards during Process |
| **Confirmation Profile Name** | Confirmation Rules for the Confirm step |
| **Confirmation Dashboard Profile Name** | Dashboards during Confirmation |
| **Certification Profile Name** | Certification Questions for the Certify step |
| **Certification Dashboard Profile Name** | Dashboards during Certification |

##### Intercompany Matching Settings

| Property | Description |
|---|---|
| **Matching Enabled** | True/False. When True, configure Matching Parameters |
| **Currency Filter** | Reporting currency |
| **View Filter** | How IC data is displayed (V#YTD or V#Periodic) |
| **Plug Account Filter** | Plug Account for the IC match |
| **Suppress Matches** | Apply match suppression |
| **Matching Tolerance** | Tolerance for offsets (0.0 = to the penny) |
| **Entity Filter** | Workflow Entities (E#Root.WFProfileEntities) |
| **Partner Filter** | Partner Entities Member Filter |
| **Detail Dims Filter** | Account-level dimensions (Flow, Origin, UD1-UD8) |

![Intercompany Matching Parameters configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p818-3707.png)

#### Entity Assignment (third tab, Cube Root only)

- Entities are assigned to Workflow Profiles
- Only enabled for data loading profiles
- Unassigned Entities go to the Default Input Profile
- Search by "contains"
- An Entity assigned to a Profile no longer appears in the search window

![Entity Assignment screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p819-3710.png)

#### Workflow Profile Grid View

- Allows bulk changes to multiple Workflow Profiles
- Only available when **Cube Root Profile is selected**
- Allows drag & drop of columns to group data
- Can pivot the table to see exactly what is needed

![Workflow Profiles Grid View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p820-3715.png)

---

### Objective 201.2.4: Less commonly used Workflow configurations

#### Workflow Suffix Groups

Allow multiple Workflow hierarchies for a single Cube, aligned with **Scenario Types**:
- Configured in Cube Properties > Suffix for Varying Workflow by Scenario Type
- Each Scenario Type is assigned to a Suffix
- The Suffix identifies the Workflow hierarchy for that business process

![Suffix configuration by Scenario Type](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p790-3617.png)

**Example:** A Cube named FinancialReporting with three processes:

| Process | Data Source | Suffix |
|---|---|---|
| Actual | System Interfaces | Sys |
| Plan Data | Keyed/Excel per Entity | Ent |
| Other Data | Unknown | Oth |

![Cube Root selection with Suffix options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p791-3621.png)

**IMPORTANT:** Once a Scenario has data, the suffix cannot be changed nor can a new Cube Root be created for that Scenario Type. Assign suffixes at the beginning of the project.

#### Workflow Templates

Useful for building Base Input Profiles with similar configurations:

![Workflow Templates screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p821-3719.png)

**Procedure:**
1. Create template (from Default or new)
2. Customize: rename inputs, add/disable input types by Scenario, configure IC, assign Cube Views
3. Navigate to Workflow Profiles and create Base Input
4. Apply template to the new Base Input

![Template application to Base Input](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p822-3724.png)

**Critical limitation:** After applying, changes to existing template input types are **not** propagated to the Profile. Only **new** Input Types can be applied later via the Update Input Children Using Template icon.

![Update Input Children Using Template](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p823-3728.png)

#### Central Input

For corporate to make final adjustments after subsidiaries have loaded data:

![Central Input example with gray mark](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p821-3718.png)

- Configured with a Workflow Channel that shows a gray mark
- Corporate can make updates because the Workflow "owns" the Entity
- All activity is tracked in audit history
- Requires **Can Load Unrelated Entities = True**
- The Workflow Engine respects the lock/certification state of the Profile that owns the Entity

#### Named Dependents (in Review Profiles)

Allow a Review Profile to establish dependency on Input Parent Profiles that are **not** its direct descendants:
- Necessary when Shared Services loads data for entities that belong to different Review Profiles
- Example: Site A has Entity 1, 2, 3 and Site B has Entity 4, 5, 6. A reviewer is responsible for Entity 1, 2 and 6. Named Dependents resolves this.

#### Multiple Input Workflow Profiles per Type

Multiple Import/Forms/Journal Profiles of the same type can exist within a period:

![Example of 8 Form channels in a Budget](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p804-3672.png)

- Useful when multiple source systems feed the same Entities
- Different groups of people complete different Forms
- For Import with multiple siblings: **automatically handles clear and merge of data**

##### Load behavior with Multiple Import Siblings:

| Condition | Behavior |
|---|---|
| No overlap between siblings | Standard individual clear and replace |
| With overlap between siblings | Clear ALL data for both siblings, reload both using accumulate method (values are summed) |

![Behavior 2: Multiple Import Children](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p833-3756.png)

![Behavior 3: Central Input with common entities](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p834-3761.png)

#### Loading YTD and MTD in the same Workflow Profile

To load YTD data in one Origin and MTD/Periodic in another within the same Profile:

![Example Houston.Import YTD and Houston.Sales Detail MTD](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p805-3675.png)

Configure in **Integration Settings** of the Input Child:
- **Flow Type No Data Zero View Override:** Periodic
- **Balance Type No Data Zero View Override:** YTD
- **Force Balance Accounts to YTD View:** True

#### Load Overlapped Siblings

Controls the behavior of sibling Import Children for parallel processing:

![Load Overlapped Siblings configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p836-3766.png)

| Value | Behavior | When to use |
|---|---|---|
| **True (default)** | Checks overlap between sibling channels | When siblings may have overlapping data |
| **False** | Does not check overlap. The last channel processed overwrites | Improves performance in parallel processing without guaranteed overlap |

#### Data Locking

**Lock Types:**

| Type | When created | How it is cleared |
|---|---|---|
| **Explicit Lock** | When manually locking a Workflow Unit | Manual unlock |
| **Implicit Lock** | When certifying the Parent Workflow | De-certify the Parent Workflow |
| **Workflow Only Lock** | When the Profile has no assigned Entities | Manual unlock |

**Lock Granularity:**

| Level | Scope | Description |
|---|---|---|
| **Input Parent (Level 1)** | All cells of assigned Entities | Broadest lock -- locks everything |
| **Input Child (Origin Lock)** | By Origin Member (Import, Forms, AdjInput) | Lock by input channel |
| **Input Child (Workflow Channel Lock)** | By Workflow Channel + Origin | Finest granularity |

![Workflow Channel Lock diagram](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p840-3775.png)

**Key characteristic:** OneStream uses **bidirectional** control between the Workflow Engine, Staging Engine and Analytic Engine. Any attempt to update a cell directly (Forms, Excel) triggers Workflow Engine validation. This does NOT exist in unidirectional systems where the cell can be modified ignoring the Workflow state.

**Lock Security:**
- To lock: requires Workflow Execution Group membership (inheritance from ancestors)
- To unlock as non-admin: requires Workflow Execution Group + Application Security Role **UnlockWorkflowUnit**

#### Data Unit Levels in Workflow

| Level | Members | Use | Operations |
|---|---|---|---|
| **Level 1 - Cube Data Unit** | Cube, Entity, Parent, Consolidation, Scenario, Time | Broadest lock | Clear, Load, Lock, Copy, Calculate, Translate, Consolidate |
| **Level 2 - Workflow Data Unit** | + Account (default for Workflow Engine) | More granular control per account | Clear, Load, Lock |
| **Level 3 - Workflow Channel Data Unit** | + 1 User Defined dimension | Maximum granularity | Clear, Load, Lock |

The Workflow Channel Data Unit is a subset of the Workflow Data Unit. The User Defined dimension is selected in Application Properties (one per application). Common choices: Cost Center, Version.

#### Workflow Channels

Three predefined Workflow Channels:

| Channel | Purpose | Assigned to |
|---|---|---|
| **Standard** | Default with no special purpose | Account Members and Workflow Profile Input Children |
| **NoDataLock** | Removes the Member from the Workflow Channel process | Metadata Members only (Account or UDx), NOT to Workflows |
| **AllChannelInput** | Indicates the Workflow can control any Channel | Workflow Profile Input Children only, NOT to Metadata |

**Account Phasing:** Use Workflow Channels with Accounts for independent control of account groups.

![Account Phasing diagram with Workflow Channels](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p858-3824.png)

**User Defined Phasing:** Use Workflow Channels with UD Dimension for control by user dimension.

![UD Type for Workflow Channel configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p860-3829.png)

#### Workflow Stage Import Methods

| Method | Storage | Audit | Use |
|---|---|---|---|
| **Standard** | Stores source and target in Stage | Complete (drill-down, history) | Consolidations, book-of-record |
| **Direct** | In-memory, does not store details | Limited (no drill-down, max 1000 errors) | Planning, high-frequency operational data |
| **Blend** | In-memory, writes to relational tables | BI Blend specific | BI Blend, transactional data |

![Standard vs In-Memory comparison](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p843-3782.png)

**Direct Load Limitations:**
- Does not store source/target records
- Does not support drill-down from Finance Engine
- Does not support Re-Transform (data must be reloaded)
- Maximum 1000 validation errors per load
- Data files cannot load to Time/Scenarios beyond the current Workflow
- Does not support Append (Replace only)

**Volumes and limits:**

| Limit | Value |
|---|---|
| Row Limit Per Workflow | 24 million summarized records |
| Best Practice | 1 million summarized records |

#### Batch File Loading

Allows importing and processing files automatically up to certification:

1. Create **Extender Business Rule** that calls `BRApi.Utilities.ExecuteFileHarvestBatch`
2. Create **Data Management Sequence** with Business Rule Step
3. Format file names: `FileID-ProfileName-ScenarioName-TimeName-LoadMethod.txt`
4. Copy files to `Batch\Harvest` folder
5. Execute the Data Management Sequence

**File name format:**
- `aTrialBalance-Houston;Import-Actual-2011M1-R.txt`
- `;` delimits Parent and Child Profile names
- `C` = Current Scenario/Time, `G` = Global Scenario/Time
- `R` = Replace, `A` = Append

**Parallel Batch:** `BRApi.Utilities.ExecuteFileHarvestBatchParallel` allows multiple parallel processes.

#### Confirmation Rules

Control rules for verifying the validity of processed data.

**Confirmation Rules Properties:**

| Property | Description |
|---|---|
| **Scenario Type Name** | Available for a specific Scenario Type or all |
| **Order** | Processing order |
| **Rule Name** | Descriptive name (visible in Workflow) |
| **Frequency** | All Time Periods, Monthly, Quarterly, Half Yearly, Yearly, Member Filter |
| **Rule Text** | Textual description of the rule |
| **Action** | Warning (Pass) or Error (Fail) or No Action (informational) |
| **Failure Message** | Message if error (max 2000 characters) |
| **Warning Message** | Message if warning |

![Rule Formula editor for Confirmation Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch09-p867-3845.png)

#### Certification Questions

Questionnaires that users respond to when certifying data:

| Property | Description |
|---|---|
| **Order** | Display order |
| **Name** | Descriptive name |
| **Category** | Question type/category |
| **Risk Level** | Risk level (importance) |
| **Frequency** | Same options as Confirmation Rules |
| **Question Text** | Question (Yes/No response + free comments) |
| **Response Optional** | True = optional question |
| **Deactivated** | True = does not appear but history is preserved |

#### Workflow Entity Relationship Member Filters

| Filter | Returns |
|---|---|
| `E#Root.WFProfileEntities` | All Entities associated with the selected Workflow Unit |
| `E#Root.WFCalculationEntities` | Entities defined in Calculation Definitions |
| `E#Root.WFConfirmationEntities` | Entities with Confirmed Switch = True |

#### Performance Considerations for Workflow

- **Always partition by Entity** -- there is an inherent relationship between workflow and entity that aligns with data structures
- **Load time in sequential order** -- January, February, etc.
- Do not overlap Entities by Time when loading in parallel
- For large datasets (>1 million records): Cache Page Size = 500, Cache Pages In Memory Limit = 2000
- For parallel processing: consider separating each entity with its own parent
- **Load Overlapped Siblings = False** when sibling imports do not overlap
- Stage tables use 250 partitions in SQL Server (automatic, GUID-based)

#### Workflow Best Practices

- **Do not modify the Default Workflow** (except assigning Security Groups)
- Build a "Do Not Use" structure for the Default WF Profile
- **Do not close Workflows** for general use; only for discontinued operations
- Use **Grid View** under Cube Root Profile for bulk changes
- **Assign Scenario Suffixes at the beginning of the project**
- Choose a naming convention for Security Groups and maintain it
- Embedding Calculation Definitions in most Workflow Profiles (especially Planning)
- Prototype an automated workflow early, testing manually first
- Consider Workspace for Planning where traditional workflow is too restrictive
- Create a dedicated location (admin/history) for historical loads with Can Load Unrelated Entities = True
- A single owner per entity (with proxy as backup) prevents data overwriting

---

## Critical Points to Memorize

### Workflow Names
- Import has 10 Workflow Name options (Standard); Form has 11; Journal has 7; Review has 2
- Direct Load adds 4 more options for Import (without Stage)
- "Central" in the name = for corporate controlling unrelated entities
- "Pre-Process" = calculation before showing the form (Forms only)
- "Workspace" = Custom Dashboard as interface
- **No Calculate** in Process allows assigning Data Management Sequences via Filter Value
- "Stage Only" = data only reaches Stage, not Cube (useful for exporting from OneStream)

### Calculation Definitions
- Available in **all** Profile Types except Cube Root
- Input Child inherits from Input Parent if it has no explicit definition
- **Confirmed Switch** controls whether the Entity goes through Confirmation Rules
- **Filter Value + No Calculate** allows executing Data Management Sequences via DataQualityEventHandler
- Placeholder variables: Dependent Entities, Named Dependent Filter, Assigned Entities, Loaded Entities, Journal Input Entities, User Cell Input Entities
- Consolidate/Calculate checks calc status (efficient); Force does not check (excessive)

### Workflow Profile Settings
- **8 Profile types:** Cube Root, Review, Default Input, Parent Input, Base Input, Import Child, Forms Child, Adjustment Child
- Import Child --> O#Import; Forms Child --> O#Forms; Adjustment Child --> O#AdjInput
- Parent Input **does not allow Import** (Profile Active = False forced)
- Default Input **cannot be deleted** and receives unassigned Entities (implicit)
- **Profile Active = False** deactivates the entire input channel
- **Can Load Unrelated Entities = True** required for Central Input
- **Prevent Self-Post** and **Prevent Self-Approval** = separation of duties in Journals (3 options: True, True includes Admins, False)
- **Require Journal Template = True** disables Create Journal (admins excepted)

### Data Locking
- **Bidirectional:** Workflow Engine validates every attempt to update cells (does not exist in unidirectional systems)
- **Explicit Lock:** When locking a Workflow Unit
- **Implicit Lock:** When certifying the Parent Workflow
- **Workflow Only Lock:** Profile with no assigned Entities
- Granularity: Input Parent > Input Child (Origin) > Input Child (Channel)
- Closing Workflow = historical snapshot in database (performance penalty)
- To unlock as non-admin: requires Execution Group + UnlockWorkflowUnit role

### Less commonly used configurations
- **Workflow Suffix Groups:** Multiple hierarchies per Cube aligned with Scenario Types. Assign at the beginning because they cannot be changed later if data exists.
- **Workflow Templates:** Saves time but subsequent changes to existing types are not propagated. Only new Input Types.
- **Named Dependents:** Only in Review Profiles, for Shared Services with entities in different Reviews.
- **Load Overlapped Siblings = False:** Improves performance when sibling imports do not overlap.
- **Workflow Channel Data Unit:** Adds UD dimension for maximum granularity. One UD per application.
- **NoDataLock:** For metadata members that do not participate in Workflow Channel. **AllChannelInput:** For Workflow Profiles that control any channel.
- **Direct Load:** In-memory without Stage. Max 1000 errors per load. Does not support drill-down or Re-Transform.
- **Batch File Loading:** Automates import up to certification. Format: FileID-ProfileName-ScenarioName-TimeName-LoadMethod.txt

### Stage Import Methods
- **Standard:** Stores source+target in Stage. For consolidations and book-of-record.
- **Direct:** In-memory, no storage. For planning and operational data.
- **Blend:** In-memory, writes to external relational tables. For BI Blend.
- Limit per workflow: 24M summarized records. Best practice: 1M.

### Confirmation Rules and Certification Questions
- Confirmation Rules: Warning (does not block) or Error (blocks process). Configurable frequency.
- Certification Questions: Yes/No with comments. Response Optional and Deactivated available.
- Messages truncated to 2000 characters.

### Workflow Entity Member Filters
- `E#Root.WFProfileEntities` = Entities of the current Workflow Unit
- `E#Root.WFCalculationEntities` = Entities in Calculation Definitions
- `E#Root.WFConfirmationEntities` = Entities with Confirmed = True

---

## Source Mapping

| Objective | Book/Chapter |
|----------|---------------|
| 201.2.1 | Design Reference Guide - Chapter 9: Workflow (Workflow Profile Types, Base Input - Import/Form/Journal Names); Chapter 20: Using OnePlace Workflow (Workflow Tasks, Import, Validate, Load, Forms, Journals, Process, Confirm, Certify); Foundation Handbook - Chapter 7: Workflow (Workspace, Automation) |
| 201.2.2 | Design Reference Guide - Chapter 9: Workflow (Using Calculation Definitions, Calculation Definition Entity Placeholder Variables, Confirmed Switch, Filter Value); Foundation Handbook - Chapter 7: Workflow (Performance Considerations, Consolidate vs Force Consolidate) |
| 201.2.3 | Design Reference Guide - Chapter 9: Workflow (Profile Properties, Security, Integration Settings, Form Settings, Journal Settings, Data Quality Settings, IC Matching, Entity Assignment, Grid View); Chapter 20: Using OnePlace Workflow (Right-Click Options, Multi-Period) |
| 201.2.4 | Design Reference Guide - Chapter 9: Workflow (Suffix Groups, Templates, Central Input, Named Dependents, Multiple Input Profiles, Load Overlapped Siblings, Data Locking, Data Loading Behaviors, Workflow Channels, Direct Load, Blend, Batch File Loading, Confirmation Rules, Certification Questions); Foundation Handbook - Chapter 7: Workflow (Parallel processing, locking vs closing, best practices, naming conventions) |


---

# Section 3: Data Collection (13% of the exam)

## Exam Objectives

- **201.3.1:** Demonstrate understanding of the different Data Source types
- **201.3.2:** Given source data requirements, apply an XFD Data Source configuration
- **201.3.3:** Demonstrate understanding of the Import Process Log
- **201.3.4:** Given a situation, describe the Transformation Rules types, names and examples that work most efficiently

---

## Key Concepts

### 201.3.1: Data Source Types

Data Sources are blueprints of the required import types and define how to parse and import data. A Data Source can be assigned to one or many Workflow Profiles that share a common file format.

![Main Data Sources screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p872-3859.png)

#### Main Data Source Types

| Type | Description | Format | Specific Configuration |
|------|------------|--------|------------------------|
| **Fixed Files** | Files in fixed-column format with data in predefined positions. Generally reports from source systems printed to file. | Plain text with fixed columns | Start Position and Length |
| **Delimited Files** | Files with fields separated by a common character (comma, semicolon, tab, etc.). | CSV, TSV, etc. | Delimiter and Quote Character |
| **Connector** | Direct connection to an external database (ODBC/OLEDB). Imports data without requiring a physical file. | SQL Query, View, Stored Procedure | Connector Business Rule |
| **Data Management Export Sequence** | Uses a Data Management sequence instead of a file or connector. | Internal to OneStream | Data Export Sequence Name |

**Other data entry methods (not Data Sources per se):**

| Method | Description | Origin Member |
|--------|------------|---------------|
| **Excel Template** | Excel file with Named Ranges (XFD, XFF, XFJ, XFC). Requires Allow Dynamic Excel Loads = True. | Import (XFD), Forms (XFF), AdjInput (XFJ) |
| **Manual Input (Forms)** | Manual entry through forms in Cube Views or Dashboards. | Forms |
| **Excel Add-In** | Uses XFSetCell formulas to load data to specific cube cells. | Forms |

#### General Data Source Properties - Full Detail

##### General
- **Name:** Data Source name
- **Description:** Detailed description

##### Security
- **Access Group:** Members with authority to access the Data Source
- **Maintenance Group:** Members with authority to maintain the Data Source

##### Settings
- **Cube Name:** The associated cube, which dictates the available dimensions
- **Scenario Type:** Allows assignment to a specific Scenario Type or to all. If assigned to a specific one, it is only available when assigned to a Workflow Profile of that type.

##### Data Structure Settings

| Property | Description | Options |
|-----------|------------|----------|
| **Type** | Source file structure | Fixed, Delimited, Connector, Data Mgmt Export Sequences |
| **Data Structure Type** | Data format | **Tabular** (one line = one intersection with one amount) or **Matrix** (multiple amounts per line using rows and columns) |
| **Allow Dynamic Excel Loads** | Allows loading Excel templates in addition to the configured file | True/False |

#### Fixed Files - Details

Files in fixed-column format. For each dimension the following is configured:
- **Start Position:** Numeric starting position of the field
- **Length:** Number of characters to take from the starting position

![Fixed File example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p201-2187.png)

A source file can be associated with the Data Source by clicking on the toolbar and selecting fields visually by highlighting areas in red.

#### Delimited Files - Details

Files with fields separated by a delimiter character.

| Property | Description |
|-----------|------------|
| **Delimiter** | Separator character (comma, semicolon, tab, etc.) |
| **Quote Character** | Quote character to protect fields that contain the delimiter |

![Delimited file example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p202-2194.png)

**Caution:** Make sure the delimiter is not a character used in field names (e.g., "New Company, Inc." with comma delimiter).

#### Connector Data Source - Important Details

A **Connector Business Rule** defines the connection, result sets, and Drill Back capabilities.

##### Connector Information Request Types

| Request Type | Function | Returns |
|-------------|---------|---------|
| **GetFieldList** | Returns the list of available fields from the external Data Source. Executes when selecting the Connector on the Data Source screen. | List(Of String) |
| **GetData** | Executes when clicking "Load and Transform". Runs the data queries. | Data rows (fields must match GetFieldList) |
| **GetDrillBackTypes** | Returns the available Drill Back options. Executes on double-click or right-click > Drill Back. | List(Of DrillBackTypeInfo) |
| **GetDrillBack** | Executes the Drill Back type selected by the user. | DrillBackResultInfo |

##### Drill Back Types

| Type | Description |
|------|------------|
| **DataGrid** | Presents a data grid to the user |
| **TextMessage** | Presents a text message |
| **WebUrl** | Presents a website or custom HTML |
| **WebUrlPopOutDefaultBrowser** | Opens a website in the default external browser |
| **FileViewer** | Presents file content (FileShareFile, AppDBFile, SysDBFile) |

![Drill Back example on imported data](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p889-3902.png)

![Nested Drill Back to second level](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p890-3905.png)

##### Connector Integration Prerequisites

| Step | Detail |
|------|---------|
| 1. Source systems inventory | DB type, location (Oracle, SQL, DB2, etc.) |
| 2. Query method | SQL Query, SQL View, Stored Procedure |
| 3. Access credentials | Read-only against production |
| 4. 64-bit Client Data Provider | Installed on **each** Application Server |
| 5. Connection String | Configured in `XFAppServerConfig.xml` under Database Server Connections |

##### Creating a Connection String

1. Create a .udl file on the Application Server Desktop (rename .txt to .udl)
2. Configure Provider, Server, Credentials, Database
3. Save and rename to .txt to see the resulting connection string

![UDL file creation for Connection String](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p880-3877.png)

![Connection String result](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p881-3880.png)

##### Connection String Examples

| System | Connection String |
|---------|------------------|
| **SQL Server** | Provider=SQLOLEDB.1;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=DBName;Data Source=SQLSERVERNAME |
| **Oracle** | Provider=OraOLEDB.Oracle.1;Password=xxxxx;Persist Security Info=True;User ID=username;Data Source=frepro.world |
| **DB2** | Provider=IBMDA400.DataSource.1;Password=xxxxx;Persist Security Info=True;User ID=OSuser;Data Source=HUTCH400 |
| **MS Access** | Provider=Microsoft.ACE.OLEDB.12.0;Data Source=\\\\UNCFileShare\\DB1.accdb;Mode=Read |

##### Server Configuration Setup

![Server Configuration - Database Connections](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p886-3891.png)

![Connection String Settings detail](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p886-3892.png)

##### Prototyping Queries with Dashboard Data Adapters

Best practice: Create a Dashboard Maintenance Unit "EXS_ConnectorName" with Data Adapters to test queries before creating the Connector Business Rule.

![Dashboard Data Adapter for testing external queries](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p887-3895.png)

![Test query result](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p888-3899.png)

#### Data Management Export Sequence Data Source

Allows using a Data Management sequence in an Import Workflow to:
- Copy data from one cube/scenario to another cube/scenario with different dimensionality
- Export data from OneStream to an external system applying Transformation Rules

##### Use case 1: Copy data within OneStream

1. Configure a Data Management Step with Export Data type
2. Create a Data Source with Type = "Data Mgmt Export Sequences" and set the Data Export Sequence Name
3. Configure Source Dimensions
4. Create a Transformation Profile to map source to target
5. Create an Import Workflow Profile with Workflow Name = Import, Validate, Load

![Data Management Export Step configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p903-7963.png)

![Data Source configured as DM Export Sequence](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p904-3943.png)

![Source Dimensions of the DM Export Sequence](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p905-3946.png)

##### Use case 2: Export data from OneStream

1. Configure a Data Management Step with Export Data
2. Create a Data Source with DM Export Sequences
3. Create Transformation Rules with Source Value and Target Value for the external system
4. Create an Import Workflow with **Import Stage Only** (only imports to Stage)
5. Configure retrieval method: Workflow Event Handler, Transformation Event Handler, Extender Rule, or Data Adapter with REST API

![Workflow Profile for export](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p912-3968.png)

#### Origin Dimension - Fundamental Concept

The Origin dimension segments each entity's data into three isolated groups:

![Origin Dimension diagram](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p195-2152.png)

| Origin Member | Data | Entry Method |
|--------------|-------|-------------------|
| **Import** | Data imported from flat files, queries, or Excel templates (XFD) | Load and Transform in Workflow |
| **Forms** | Data entered manually or via Excel template (XFF) / Add-In | Forms in Workflow, Cube Views, XFSetCell |
| **AdjInput (Journal Entries)** | Manual journal entries, templates, or loaded via Excel (XFJ) | Journals in Workflow |

The Origin dimension acts as a barrier: reimporting data does not overwrite Forms data, and vice versa. This isolation is fundamental for data protection.

![Data protection layers by Origin](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p205-2217.png)

#### Source Data Considerations

- Source files should be in **natural debit and credit sign** (facilitates mapping and validation)
- The **Amount** field is the most important in any file; it determines whether a record is accepted or rejected
- Amounts can be in a single column (Tabular format) or in Matrix format
- OneStream is a **pull** system (extraction), it does not allow data push
- For large datasets (more than 1 million records): set Cache Page Size = 500, Cache Pages In Memory Limit = 2000, Cache Page Rule Breakout Interval = 0

![Cache configuration for large datasets](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p199-2175.png)

#### Smart Integration Connector (SIC)

Used when ODBC/REST API are not publicly exposed. Uses a local agent (SIC Gateway) and Azure Relay to connect securely to systems behind firewalls.

---

### 201.3.2: XFD Data Source Configuration (Excel Template)

#### Dimension Tokens for Import Excel Template

The template must use Named Ranges that start with **XFD** to import data to Stage. Dimension tokens must be in the first row of the Named Range.

![Excel Template example with tokens](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch06-p203-2202.png)

| Token | Meaning | Notes |
|-------|------------|-------|
| `E#` | Entity | Each row lists the entity |
| `A#` | Account | Each row lists the account |
| `AMT#` | Amount | Use `AMT.ZS#` for automatic zero suppression (also applies to Matrix) |
| `F#` | Flow | |
| `IC#` | Intercompany | |
| `C#` | Consolidation | |
| `S#` | Scenario | |
| `T#` | Time Period | |
| `V#` | View | |
| `O#` | Origin | |
| `UD1#` to `UD8#` | User Defined Dimensions | Each row must have a value, use `None` if not applicable. `UX#` or `UDX#` can also be used |
| `LB#` | Label | Reference description, not stored in the cube |
| `SI#` | Source ID | Key for data in Stage. Best practice: one Source ID per Named Range |
| `TV#` | Text Value | Stores large amounts of text |
| `A1#` to `A20#` | Attribute Dimensions | Up to 100 characters each |
| `AV1#` to `AV12#` | Attribute Value Dimensions | Numeric data |

#### Header Abbreviations

| Abbreviation | Syntax | Description |
|-------------|---------|-------------|
| **Static Value** | `F#:[None]` | Fixes a member to the entire column |
| **Business Rule** | `AMT#:[]:[RuleName]` | Executes a Business Rule to assign a value |
| **Matrix Member** | `T#:[]:[]:[2012M3]` | Repeats for each member in Matrix |
| **Current Workflow Time** | `T.C#` | Returns the current Workflow Time |
| **Current Workflow Scenario** | `S.C#` | Returns the current Workflow Scenario |
| **Global Time** | `T.G#` | Returns the Global POV Time |
| **Global Scenario** | `S.G#` | Returns the Global POV Scenario |
| **Annotation** | `TV#:[#Annotation]` | Adds an annotation row |
| **VarianceExplanation** | `TV#:[#VarianceExplanation]` | Adds a variance explanation row |

#### Named Range Types by Data Type

| Named Range | Use | Details |
|-------------|-----|---------|
| **XFD** | Data load to Stage (Import) | Dimension Tokens in the first row |
| **XFF** | Forms data | Includes Property Tokens (Form Template Name, Workflow Name, Scenario, Time) |
| **XFJ** | Journal data | Includes Property Tokens (Template Name, Journal Type, Balance Type, etc.) |
| **XFC** | Cell Details | Includes all 18 dimension members + AMT#, LIT#, AW#, CL#, LB# |

Multiple Named Ranges of the same type can be used across multiple tabs of an Excel workbook.

#### Form Excel Template - Additional Details

The first 4 rows of the XFF Named Range must include:

![Form Excel Template Property Tokens](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p972-4158.png)

| Property Token | Description |
|----------------|------------|
| **Form Template Name** | Name of the Form Template |
| **Workflow Name** | E.g.: Houston.Forms |
| **Workflow Scenario** | Actual, Budget, etc. or `\|WFScenario\|` |
| **Workflow Time** | Period or `\|WFTime\|` |

Additional Form tokens:
- `HD#` = Has Data (Yes/No)
- `AN#` = Annotation
- `AS#` = Assumption
- `AD#` = Audit Comment
- `FN#` = Footnote
- `VE#` = Variance Explanation

![XFF Named Range example with data](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p975-4169.png)

#### Journal Excel Template - Additional Details

The first 11 rows of the XFJ Named Range must include:

![Journal Excel Template Property Tokens](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p979-4186.png)

| Property Token | Description |
|----------------|------------|
| **Template Name** | Template name (empty if free-form) |
| **Name** | Journal name |
| **Description** | Description |
| **Journal Type** | Standard or Auto-reversing |
| **Balance Type** | Balanced, Balanced by Entity, Unbalanced |
| **Is Single Entity** | True/False |
| **Entity Filter** | Member Filter for Entities |
| **Consolidation Member** | Currency or Local |
| **Workflow Name** | E.g.: Houston.Journals |
| **Workflow Scenario** | Actual, etc. or `\|WFScenario\|` |
| **Workflow Time** | Period or `\|WFTime\|` (additional optional field: CubeTimeName) |

Journal tokens:
- `AMTDR#` = Debit amount
- `AMTCR#` = Credit amount

![XFJ Named Range example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p982-4199.png)

#### Source Dimension Properties - Full Detail

Each Data Source has Source Dimensions assigned with configurable properties.

##### Available Data Types

| Data Type | Description | Applies to |
|-----------|------------|----------|
| **DataKey Text** | Reads value from file according to position settings | All dimensions |
| **Stored DataKey Text** | Forces a constant value for Time on each line | Time |
| **Global DataKey Time** | Uses the Time value from the Global POV | Time |
| **Current DataKey Time** | Uses Time from the current Workflow POV | Time |
| **Current DataKey Scenario** | Uses Scenario from the current Workflow POV | Scenario |
| **Matrix DataKey Text** | For Matrix Load with multiple periods | Time (Matrix) |
| **Text** | Reads value from file according to position settings | Non-key dimensions |
| **Stored Text** | Forces a constant value for each line | Non-key dimensions |
| **Matrix Text** | For Matrix with multiple columns of the same dimension | Non-key dimensions (Matrix) |
| **Label** | Reads value as a label | Label |
| **Stored Label** | Forces a constant value as a label | Label |
| **Numeric** | Defines the numeric amount field | Amount |

##### Position Settings

| File Type | Configuration |
|----------------|---------------|
| **Fixed Files** | Start Position and Length |
| **Delimited Files** | Column number |
| **Connector** | Source Field Name (provided by the Business Rule) |

You can select visually in the attached file: highlight the area (appears in red), click on the assignment icon.

![Visual position selection tool](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p917-3982.png)

##### Logical Operator

| Option | Description |
|--------|------------|
| **None** | No script (default) |
| **Complex Expression** | .NET script available only in this dimension |
| **Business Rule** | .NET script from the Business Rule Library |

##### Numeric Settings (Amount only)

| Property | Description |
|-----------|------------|
| **Thousand Indicator** | Thousands separator character (e.g., ",") |
| **Decimal Indicator** | Decimal separator character |
| **Currency Indicator** | Currency symbol |
| **Positive Sign Indicator** | Character(s) for positive value |
| **Negative Sign Indicator** | Character(s) for negative value |
| **Debit/Credit Mid-Point Position** | If debits and credits are in the same field, the midpoint to distinguish them |
| **Factor Value** | Multiplies the imported amount (e.g., 1000 to convert from thousands) |
| **Rounding** | Not Rounded or values 1-10 |
| **Zero Suppression** | True = do not import zeros; False = import zeros |
| **Text Criteria for Valid Numbers** | Criteria for valid numbers |

##### Bypass Settings

Allows skipping entire lines based on a found value:

| Bypass Type | Description |
|------------|------------|
| **Contains at Position** | Skips the line if the Bypass Value is found at the specified position |
| **Contains Within Line** | Skips the line if the Bypass Value appears anywhere |

![Bypass Settings configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p923-4005.png)

**Tip:** For blank space bypass in Fixed files, use double brackets with the number of spaces in the Bypass Value.

##### Substitution Settings

| Property | Description |
|-----------|------------|
| **Old Value (Find)** | Value to search for. Multiple values separated with `^` |
| **New Value (Replace)** | Replacement value. Multiple values separated with `^`. For empty string: `\|Empty String\|`, `\|Null\|`, `\|Single Space\|`, `\|Space\|` |

##### Text Fill Settings

| Property | Description | Example |
|-----------|------------|---------|
| **Leading Fill Value** | Characters that precede the imported value | Mask=xxx, Value=00, Result=x00 |
| **Trailing Fill Value** | Characters that follow the imported value | Mask=xxx, Value=00, Result=00x |

##### Stored Text Settings

| Property | Description |
|-----------|------------|
| **Text Criteria to Bypass in Storage Buffer** | Value(s) that cause record bypass. Multiple with `^` |
| **Stored Value Line #** | Line number from which the repeated value is obtained |

##### Matrix Settings (only when Data Structure Type = Matrix)

| Property | Description |
|-----------|------------|
| **Matrix Header Values Line #** | Row number where to search for the Matrix dimension members |

![Matrix Settings example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p920-3990.png)

---

### 201.3.3: Import Process Log

#### The Import Process in Workflow - Detailed Step-by-Step

##### 1. Import (Load and Transform)

The system imports data to the **Stage Engine**. The file is parsed into a clean tabular format with Amount, Source ID, and each Dimension information.

![Import step in the Workflow with Load and Transform](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1932-7269.png)

**Available Load Methods:**

| Method | Behavior | Restrictions |
|--------|---------------|---------------|
| **Replace** | Deletes data from the previous Source ID and replaces with the new file | Can be done even if previous data was already loaded to the Cube |
| **Replace (All Time)** | Replaces all Workflow Units in the selected Workflow View | For multi-period |
| **Replace Background (All Time, All Source IDs)** | Replaces all in a background thread. **Always deletes ALL Source IDs** | Do NOT use if workflow uses multiple Source IDs for partial replacement |
| **Append** | Adds only new rows without modifying existing data | Only adds what was not there before |

![Load Method dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1933-7272.png)

**Additional Import step icons:**

| Icon | Function |
|-------|---------|
| Re-Import | Repeats import when there are changes and recalculation is needed |
| Clear Stage | Clears all data from Stage |
| View Last Source File | Views the last processed source file |
| View Last Log File | Views the last processing log |

##### 2. Validate

Two specific actions during Validate:

**Step 1 - Mapping verification:** OneStream verifies that each data point has a map (Transformation Rule).

**Step 2 - Intersection verification:** OneStream verifies that the dimension combination can be loaded into the cube (e.g., Intercompany restrictions).

![Validate step in the Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7286.png)

**Transformation Errors:**
- Shows the dimension with error, Source Value, and Target Value (Unassigned)
- The system suggests a One-To-One Transformation Rule by default
- You can search for the correct target using the filter in the right panel
- Save changes and click **Retransform** (re-validates automatically)

![Transformation Errors screen with Unassigned Target Value](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1935-7288.png)

**Intersection Errors:**
- Something is not correct with the complete intersection (e.g., Customer mapped to Salary Grade Account)
- To fix: click on the bad intersection, drill down to see GL/ERP Account
- Right-click > View Transformation Rules to investigate each rule per dimension
- Edit the incorrect rule, save, click Validate again

##### 3. Load

Loads the clean data from Stage to the **Analytic Engine** (Consolidation Engine).

![Load step - click Load Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1936-7293.png)

![Load Cube progress dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch20-p1937-7296.png)

#### Right-Click Options in Import

| Option | Description |
|--------|------------|
| **View Source Document** | Opens the imported source file |
| **View Processing Log** | Opens the processing log with information about when and how the data was imported |
| **View Transformation Rules** | Shows all mapping rules for the specific intersection |
| **Drill Back** | Only available with Connector Data Source; navigates to the source system |
| **Export** | Exports data to Excel XML, CSV, Text, or HTML |

#### Workflow Icons and States

| Color | Meaning |
|-------|-----------|
| **Blue** | Task pending completion |
| **Green** | Task completed successfully |
| **Red** | Error that must be fixed before continuing |

#### Global Load Controls

| Control | Description | Configuration |
|---------|------------|---------------|
| **Enforce Global POV** | Loading limited to the Global POV | Application > Tools > Application Properties |
| **Allow Loads Before Workflow View Year** | Controls loads to prior periods | Application > Tools > Application Properties |
| **Allow Loads After Workflow View Year** | Controls loads to future periods | Application > Tools > Application Properties |

#### Audit Workflow Process

Right-clicking on any Workflow channel provides a complete audit:
- Process date and time
- User who executed the process
- Process duration
- Errors that occurred
- Lock history (Lock History > Workflow Lock/Unlock)

#### Data Load Execution Steps (Clear and Replace)

When the load process is executed, the engine follows these steps:

1. **Verify Workflow status:**
   - Implicitly locked (parent workflow certified)?
   - Explicitly locked?
2. **Verify load switches:**
   - Can Load Unrelated Entities
   - Flow/Balance Type No Data Zero View Override
   - Force Balance Accounts to YTD View
3. **Analyze prior loads:**
   - Evaluates previously loaded Data Units to determine what to clear
4. **Execute clear data:**
   - Clears Workflow Data Units loaded by the Workflow Unit (at Account level)
5. **Execute load data:**
   - Loads using parallel processing per entity

---

### 201.3.4: Transformation Rules - Types, Names, and Efficiency

#### Transformation Rules Execution Order

The rules execute in the following order during the **Validate** step of the Workflow:

1. **Source Derivative Rules** (logic on incoming source data)
2. **One-to-One** (explicit mapping)
3. **Composite** (conditional mapping with dimensional tags)
4. **Range** (value range)
5. **List** (delimited list)
6. **Mask** (wildcards)
7. **Target Derivative Rules** (logic on post-transformed data)

![Transformation Rules toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p942-4066.png)

#### Transformation Rule Types - Full Detail

##### One-to-One

A source member maps to a destination member explicitly. No wildcards or scripts.

| Property | Description |
|-----------|------------|
| **Source Value** | Value from the source file |
| **Description** | Optional description |
| **Target Value** | Destination Dimension Library member |
| **Order** | Processing order (default: alphanumeric) |

**Critical rule:** It is the **only valid type for Scenario, Time, and View**. These three dimensions only support One-to-One.

**Example:** `Actual -> Actual`, `23099 -> 23000`

##### Composite

Conditional mapping using dimensional tags (`D#[value]`). Uses `*` (any number of characters) and `?` (one character).

**Important behavior:** **Stops when it finds a match.** If a record meets more than one rule, configure the Order from the most restrictive to the broadest.

**Example:** `A#[199?-???*]:E#[Texas]` -- Accounts starting with 199 with entity Texas.

![Composite example with Order from narrow to broad](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p946-4084.png)

![Composite mapping result with different targets](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p947-4088.png)

**Correct Order example:**
1. `E#[*]:A#[H???]` -- 4 characters or less (Head) -> Rhode Island
2. `E#[*]:A#[H????]` -- 5 characters or less (Heads) -> Maine
3. `E#[*]:A#[H?????????]` -- 10 characters or less (Headcount) -> Bypass

##### Range

Defines upper and lower limits with `~` as separator. Uses **character sets, not integers**.

**Important behavior:** **Does NOT stop when it finds a match.** If ranges overlap, it applies the **last range processed**. Configure Order so that the last one is the desired result.

**Example:** `11202~11209` -> Account 12000

**Critical rule:** Use balanced character sets. `4~3000` should be `0004~3000`.

##### List

Delimited list of members that map to the same destination. Uses `;` as separator.

**Example:** `41137;41139;41145` -> Account 61000

##### Mask

Wildcards: `*` = any number of characters, `?` = one character.

| Pattern | Captures | Does Not Capture |
|---------|---------|-----------|
| `27*` | 270, 2709, 27XX-009 | - |
| `27??` | 2709 | 27999, 2700-101 |
| `*000*` | Anything with 000 in the middle | - |

**Target Value restrictions:**
- `?` in Target Value **is NOT supported**
- `*` as prefix (left) in Target Value **is NOT supported**
- `*` as suffix (right) in Target Value returns the **Source value**

**Properties common to Composite, Range, List, Mask:**
- Rule Name, Description, Rule Expression, Target Value, Logical Operator, Order

##### Source Derivative Rules

Logic applied to incoming source data. Creates new records in Stage based on input data.

![Derivative Rules screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p950-4095.png)

##### Target Derivative Rules

Logic applied to post-transformed data. Creates additional records in Stage. Since they are post-transformation, Final records do not pass through Transformation Rules.

##### Lookup

Versatile lookup table for formulas, business rules, or simple lookup.

#### Derivative Types

| Derivative Type | Stored in Stage | Used in Other Derivatives | Description |
|----------------|--------------------|--------------------------|----|
| **Interim** | No | Yes | Temporary for use in subsequent derivatives |
| **Interim (Exclude Calc)** | No | No | Excluded from other derivative calculations |
| **Final** | Yes | Yes | Stored and can be mapped to target |
| **Final (Exclude Calc)** | Yes | No | Stored but excluded from other calculations |
| **Check Rule** | N/A | N/A | Custom validation rule during Validate |
| **Check Rule (Exclude Calc)** | N/A | No | Check Rule excluded from other calculations |

#### Derivative Rule Expressions - Syntax Examples

![Derivative Rule Expressions examples table](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p951-4098.png)

| Rule Expression | Type | Notes |
|----------------|------|-------|
| `A#[11*]=Cash` | Final | Accounts starting with 11 add to Cash (stored in Stage) |
| `A#[12*]=AR` | Interim | Accounts starting with 12 add to AR (not stored) |
| `A#[1300-000;Cash]=CashNoCalc` | Interim (Exclude Calc) | Cash excluded because calc excluded |
| `A#[1000~1999]<<New_:E#[Tex*]=TX` | Final | Creates new rows with "New_" prefix for accounts 1000-1999 |
| `A#[2000~2999]>>_:Liability:U2#[*]=None` | Final | Creates rows with "_Liability" suffix, UDs to None |
| `A#[3000~3999]@3:E#[Tex*]@1,1` | Final | Takes first 3 digits of Account; @1,1 = position 1 length 1 of Entity |

#### Logical Operators (Expression Types)

| Operator | Description |
|----------|------------|
| **None** (default) | No script |
| **Business Rule** | .NET script from the Business Rule Library (Derivative type) |
| **Complex Expression** | Local .NET script |
| **Multiply** | Multiplies resulting value by Math Value |
| **Divide** | Divides resulting value by Math Value |
| **Add** | Adds Math Value |
| **Subtract** | Subtracts Math Value |
| **Create If > x** | Creates derivative only if value > threshold |
| **Create If < x** | Creates derivative only if value < threshold |
| **Lag (Years/Months/Days/Hours/Minutes/Seconds)** | Returns past value according to interval |
| **Lag Change (Years/Months/Days/Hours/Minutes/Seconds/Step Back)** | Returns difference between current and past value |

![Lag Years example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p958-4113.png)

![Lag Change Step Back example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p962-4132.png)

#### Transformation Rules Performance (most to least efficient)

| Rule Type | Processing | Cost | Color |
|--------------|--------------|-------|-------|
| **One-to-One** | Simple update pass through to the DB | Minimal | Green |
| **Range** | Simple update pass through to the DB | Minimal | Green |
| **List** | Simple update pass through to the DB | Minimal | Green |
| **Mask `*` (one side)** | Simple update pass through. Low overhead | Minimal | Green |
| **Mask `*` to `*`** | Simple update pass through | Minimal | Green |
| **Mask `?`** | Simple update but **table scans**. Minimize `?` usage | Medium | Yellow |
| **Range/List/Mask (Conditional)** | Heavy data transfer and memory. Returns complete record set | High | Red |
| **Derivative** | Heavy transfer and memory. SQL with LIKE. Inserts records one by one | High | Red |

**Key performance recommendations:**
- Use **One-to-One, Range, and List** whenever possible (minimal cost)
- Rules with `?` are more expensive than `*` because they require table scans
- `?` on both sides (source and target) are extremely expensive
- **Conditional** rules (Composite, Range, List, Mask with dimensional conditions) return all fields to the app server, consuming a lot of memory
- For large lists in Conditional: split into smaller lists
- For Mask Conditional: keep restrictive criteria (e.g., `1*`, `2*`, `A*`, `B*`)
- A one-to-one map with many rules can be more efficient than a few mask rules with `?`
- If everything has one-to-one mapping, it may be a sign that the ledger is being unnecessarily replicated

#### Transformation Rule Profiles

Transformation Rule Groups are organized in **Transformation Rule Profiles**, which are then assigned to **Workflow Profiles** (Application > Workflow Profiles > Import > Integration Settings).

![Transformation Rule Profiles screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p967-4146.png)

**Quick shortcut:** The **"Create and Populate Rule Profile"** button automatically creates a Group and a Profile with the same name, already populated with each Dimension Rule Group and updates when the Group is updated.

![Create and Populate Rule Profile button](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p942-4070.png)

**TRX Files:** Groups can be exported and imported as TRX files (comma delimited). Limitation: does not support Logical Operators or Complex Expressions. For full management use Application Tools Load/Extract XML.

**Reusable global rules:** The Scenario, Time, and View dimensions only use One-to-One and can share groups across multiple cubes. Name them as "Global" for easy identification.

#### Form Templates - Configuration

##### Form Template Properties

| Property | Description |
|-----------|------------|
| **Form Type** | Cube View, Dashboard, or Spreadsheet (Windows App only) |
| **Form Requirement Level** | Not Used (deprecated), Optional, Required |
| **Form Frequency** | All Time Periods, Monthly, Quarterly, Half Yearly, Yearly, Member Filter |
| **Time Filter for Complete Form** | Filter that dictates the Complete Form frequency |

![Time Filter for Complete Form example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p938-4056.png)

##### Parameter Types for Cube View Forms

| Parameter Type | Description |
|---------------|------------|
| **Literal Value** | Hard coded value |
| **Input Value** | User can change |
| **Delimited List** | Predefined list of values |
| **Member List** | Flat list of Members |
| **Member Dialog** | Dialog with search and hierarchy |

##### Form Allocations - Advanced Distribution

Allows distributing amounts using source, destination, weights, and custom calculations:

![Allocation dialog with Source/Destination POV](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p926-4015.png)

1. **Source POV:** Origin intersection (can drag & drop from grid)
2. **Source Amount/Calculation Script:** Override of the source value with a script
3. **Destination POV:** Destination intersection
4. **Dimension Type/Member Filters:** Override of destination dimensions
5. **Weight Calculation Script:** How the distribution is weighted
6. **Destination Calculation Script:** Default = `|SourceAmount|*(|Weight|/|TotalWeight|)`
7. **Offset:** Optional offset properties
8. **Generate Allocation Data:** Preview before applying

![Applied allocation result](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p929-4024.png)

#### Applying Literal Value Parameters to Form Templates

Technique for using a single Dashboard with multiple Form Templates via parameters:

1. Design Cube Views for data entry
2. Create Dashboard with Delimited List Parameter of Cube View names
3. Create Cube View Component with parameter `|!ParameterName!|`
4. Create Supplied Parameter Component bound to the parameter
5. In Form Template, set Form Type = Dashboard and specify Name Value Pair

![Delimited List Parameter in Dashboard](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p931-4030.png)

![Name Value Pair in Form Template](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch10-p932-4036.png)

---

## Critical Points to Memorize

### Data Source Types
- **4 main Data Source types:** Fixed, Delimited, Connector, Data Management Export Sequence
- **2 data structures:** Tabular (one amount per line) and Matrix (multiple amounts per line)
- **Allow Dynamic Excel Loads = True** is necessary to load Excel templates to Stage
- OneStream is a **pull** system (extraction), it does not allow data push
- Connection String is configured in `XFAppServerConfig.xml` under Database Server Connections
- Each Application Server needs the 64-bit Client Data Provider installed
- **Connector Uses Files = True** to process files with complex format via code

### Origin Dimension
- **3 main members:** Import, Forms, AdjInput (Journal Entries) -- isolated from each other
- Reimporting does not overwrite Forms; manual entry does not overwrite Import
- The Origin dimension is fundamental for data protection and layering

### Excel Templates (Named Ranges)
- **XFD** = Import to Stage; **XFF** = Forms; **XFJ** = Journals; **XFC** = Cell Details
- Multiple Named Ranges of the same type can exist across multiple tabs of a workbook
- **Source ID** is the key for data in Stage; best practice: one Source ID per Named Range
- `AMT.ZS#` applies automatic zero suppression (also in Matrix)
- `T.C#` and `S.C#` = Current Workflow Time/Scenario; `T.G#` and `S.G#` = Global POV
- XFF requires 4 property tokens; XFJ requires 11 property tokens
- `AMTDR#` and `AMTCR#` are Journal-exclusive tokens
- For CSV templates: Column A specifies Row Type (H=Header, D=Detail)

### Import Process
- **Load Methods:** Replace, Replace (All Time), Replace Background (All Time, All Source IDs), Append
- **Replace Background** always deletes ALL Source IDs -- do not use with partial Source IDs
- **Validate** does two things: (1) verifies mapping, (2) verifies valid intersections in the cube
- **Retransform** re-validates automatically after correcting Transformation Rules
- **Enforce Global POV** limits loading to the Global POV
- Audit Workflow Process shows date, user, duration, and errors for each task

### Transformation Rules
- **Execution order:** Source Derivative > One-to-One > Composite > Range > List > Mask > Target Derivative
- **Scenario, Time, and View can ONLY use One-to-One mapping**
- **Composite** stops when it finds a match; **Range** does NOT stop (applies the last range)
- **Range** uses character sets not integers (`0004~3000` not `4~3000`)
- **`?` in Target Value is not supported**; `*` as prefix in Target is not supported; `*` as suffix in Target returns Source value
- Characters `!`, `?`, and `|` are reserved and cause errors in Source/Target Members

### Transformation Rules Performance
- **Most efficient rules:** One-to-One, Range, List, Mask with `*` on one side (simple update pass through)
- **Medium rules:** Mask with `?` (table scans)
- **Least efficient rules:** Any Conditional rule (returns complete record set), Derivatives (SQL LIKE + insert one by one)
- **Create and Populate Rule Profile:** Creates Group + Profile automatically, updates with the Group
- TRX import/export does not support Logical Operators or Complex Expressions

### Derivative Rules
- **Interim:** Not stored, used in subsequent derivatives
- **Final:** Stored in Stage, mappable to target
- **Check Rule:** Custom validation during Validate
- **Exclude Calc:** Version of each type excluded from other calculations
- **Lag/Lag Change:** Operators for historical values with granularity from Years to Seconds

### Connector Data Source
- **4 Request Types:** GetFieldList, GetData, GetDrillBackTypes, GetDrillBack
- **5 Drill Back types:** DataGrid, TextMessage, WebUrl, WebUrlPopOutDefaultBrowser, FileViewer
- Best practice: prototype queries with Dashboard Data Adapters before creating the Business Rule
- Connection String name in XFAppServerConfig.xml is used as reference in the Business Rule

### Batch Processing
- Files in the `Harvest` directory with format `FileID-WFProfileName-ScenarioName-TimeName-LoadMethod.txt`
- `;` replaces `.` to delimit Parent and Child Profile names
- `C` = Current (from the function), `G` = Global, `R` = Replace, `A` = Append

### Cache settings for large datasets (>1M records)
- Cache Page Size = 500
- Cache Pages In Memory Limit = 2000
- Cache Page Rule Breakout Interval = 0 (evaluates all rules on all pages)

---

## Source Mapping

| Objective | Book / Chapter |
|----------|-----------------|
| 201.3.1 - Data Source Types | Design Reference Guide, Ch. 10 (Data Sources, Fixed Files, Delimited Files, Connector, DM Export Sequences, Source Dimensions); Foundation Handbook, Ch. 6 (Data Source Types, Origin Dimension, Staging Engine) |
| 201.3.2 - XFD Configuration | Design Reference Guide, Ch. 10 (Loading Data With Excel Templates, Source Dimension Properties, Loading Form Data, Loading Journal Data, Loading Cell Detail); Foundation Handbook, Ch. 6 (Excel Templates, Data Parser) |
| 201.3.3 - Import Process Log | Design Reference Guide, Ch. 20 (Using OnePlace Workflow - Import, Validate, Load, Right-Click Options); Design Reference Guide, Ch. 9 (Data Load Execution Steps, Workflow Stage Import Methods); Foundation Handbook, Ch. 6 (Staging, Data Quality) |
| 201.3.4 - Transformation Rules | Design Reference Guide, Ch. 10 (Transformation Rules - One-to-One, Composite, Range, List, Mask, Derivative, Logical Operators, Derivative Types, Rule Profiles); Foundation Handbook, Ch. 6 (Transformation Performance, Working Through Problems) |
# Section 4: Presentation (14% of the exam)

## Exam Objectives

- **201.4.1:** Demonstrate understanding of Cube View configurations
- **201.4.2:** Demonstrate understanding of the scope of Workspaces
- **201.4.3:** Describe the steps to create a Report Book

---

## Key Concepts

### 201.4.1: Cube View Configurations

A Cube View is used to query data from the cube and present it to the user in various ways. They can be read-only, used for data editing, and used as a Data Source for different visualization mechanisms. They are the **"building blocks of reporting"**.

#### Where Cube Views are accessed

- Application > Presentation > Cube Views (Cube Views page)
- Workflow Task (forms)
- Workflow Analysis (analysis section)
- OnePlace > Cube Views
- Spreadsheet / Excel Add-In
- Within Workspaces (in Maintenance Units)

Cube Views can be incorporated into: Report Books, Dashboards, Form Templates, Spreadsheets, and Extensible Documents.

#### Organizational structure of Cube Views

1. **Cube View Groups:** Organize Cube Views. A group must be created before creating a Cube View.
2. **Cube View Profiles:** Organize groups. They are assigned to application areas (e.g., Workflow Profile). A group can be assigned to multiple profiles.
   - **Visibility:** Controls where Cube Views are seen (Always, specific location, or Never - only visible on the Cube Views page of the Application tab).

**Cube View Group Properties:**
- Name, Description, Workspace, Maintenance Unit
- Security: Access Group and Maintenance Group

![Cube View Toolbar with management icons](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1054-4415.png)

#### Cube View Toolbar (Application Tab)

| Icon | Function |
|------|----------|
| **Create Group** | Organize Cube Views into groups |
| **Create Profile** | Organize groups into profiles |
| **Manage Profile Members** | Assign groups to profiles |
| **Create Cube View** | Create a new Cube View within a group |
| **Delete Selected Item** | Delete a selected Cube View or group |
| **Rename Selected Item** | Rename a selected item |
| **Cancel All Changes** | Cancel unsaved changes |
| **Save** | Save changes |
| **Copy Selected Cube View** | Copy a Cube View as a template |
| **Search** | Search Cube Views |
| **Open Data Explorer** | Run and view the Cube View in Data Explorer |
| **Show Objects That Reference** | See where the selected item is used |
| **Object Lookup** | Search and select an object to copy |

#### Main components of a Cube View

| Component | Description |
|-----------|-------------|
| **POV (Point of View)** | Defines the cube and all dimensions used. It is not necessary to select dimensions used in rows or columns. Takes priority over the POV Pane. |
| **General Settings** | Row/column templates, common settings, headings, formatting, navigation links. |
| **Rows and Columns** | Determines the content of rows and columns. Up to 4 nested dimensions in rows and 2 in columns. |
| **Member Filters** | Member selection, parameters, variables, expansions, expressions, and business rules. |
| **Formatting** | Header and cell formatting (Data Explorer, Excel, Report). |
| **Report Header/Footer** | Controls what is displayed in the header/footer when run as a Data Explorer Report. |

![Cube View Properties in the Application Tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1053-4412.png)

#### POV Configuration

Each Cube View needs a POV configuration to retrieve data. You can:
- Enter information individually per field
- Copy the full Cube POV (Right-click > Copy Cube POV > Paste POV)
- Drag and drop from the POV Pane
- **Important:** The information in the POV slider takes priority over the POV Pane.

![POV Configuration in the Cube View editor](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1059-4443.png)

**Steps to configure the POV:**
1. On the Cube Views page, under Cube View Groups, select a Cube View.
2. Select the POV slider.
3. Enter information individually or move the entire Cube POV.
4. To move the entire Cube POV: Right-click Cube POV > Copy Cube POV > Right-click header > Paste POV.

#### Rows and Columns

- Use `+` to add and `-` to remove rows/columns.
- Member Filters, formatting, data suppression, and overrides can be applied at the row and column level.
- **Sharing rows/columns:** All or specific rows/columns can be shared from another Cube View (General Settings > Sharing). Changes are automatically linked.

![Rows and Columns preview grid](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1060-4446.png)

**Row/Column Sharing Options:**
- **All Rows/Columns:** Enter the name of the source Cube View whose rows/columns will be shared.
- **Specified Rows/Columns:** Reference a single row or column from the source Cube View.

**Cube View Templates:** Create a specific group for templates (e.g., "Templates_Columns") and copy Cube Views to reuse formatting and properties. Each Cube View must have a unique name.

#### Data Properties in Rows/Columns

| Property | Description |
|----------|-------------|
| **Can Modify Data** | If False, rows/columns are read-only. If True, the Cube View's False setting overrides it. |
| **Text Box** | Default numeric value for data cells |
| **Combo Box** | Uses a List Parameter to generate a dropdown in the cell |
| **Date** | Enables a calendar in the cell for date selection |
| **Date Time** | Enables calendar and time in the cell |
| **List Parameter** | Name of the parameter that generates the dropdown list |
| **Enable Report Navigation Link** | (Rows only) Enables Navigation Link Drill Downs in dashboards |
| **Dashboard to Open in Dialog** | (Rows only) Name of the dashboard or parameter for the Navigation Link |
| **Linked Cube Views** | List of Cube Views accessible via right-click on cells |

#### Member Filter Builder

A tool that simplifies the construction of complex Member Filters. It is accessed from Rows and Columns > select row/column > Member Filters tab.

![Complete Member Filter Builder dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1064-4458.png)

**Member Filter Builder Components:**

| Tab | Function |
|-----|----------|
| **Member Filter** | Field where the filter is built |
| **Dimension Tokens** | Button for each dimension that launches the selection dialog |
| **Member Expansion Functions** | Double-click to add (e.g., Children, Descendants, TreeDescendants) |
| **Where Clause** | Properties to complete the expression (e.g., Name Contains, HasChildren) |
| **Time Functions** | Only for Time dimension (e.g., `T#POVPrior1`). Types: POV, WF, Global, General |
| **Variables** | System Substitution Variables (e.g., `|POVTime|`, `|WFScenario|`, `|UserName|`) |
| **Samples** | Example syntax for complex queries |
| **Expansion** | Common expansions added to the end of filters |
| **Workflow** | Workflow member expansions used in Cube Views linked to workflow |
| **Other** | Member filter functions for calculated rows/columns or custom parameters |

**Member Filter Syntax:**
- Simple member: `T#2022`
- With expansion: `A#[Income Statement].Descendants`
- Multiple dimensions separated by `:` -> `Cb#GolfStream:E#Houston:S#Budget:T#2022M3`
- Multiple scripts separated by `,` -> `S#Actual, S#Budget, S#Forecast`

**Notable Member Expansions:**
- `Children`, `ChildrenInclusive`, `Descendants`, `TreeDescendants`
- Reverse variants: `ChildrenInclusiveR`, `TreeDescendantsR`
- `Where` clauses: filter by Text Properties, Name Contains/StartsWith/EndsWith, HasChildren, AccountType, IsIntercompany, In Use property, Has Member Formula, Security, Specific Currency
- `Remove` functions: remove members from the result
- `Parents`, `Ancestors`

**Available Where Clauses:**
- Text Properties
- Portions of name/description (StartsWith, Contains, EndsWith)
- Security
- Account Types (Account dimension only)
- Intercompany (Entity and Account dimensions)
- Specific Currency (Entity dimension only)
- In Use property
- HasChildren (only for dimensions assigned to the cube in the default Scenario Type)
- Has Member Formula

**Important Time Functions:**
- `T#POVPrior1` - Prior period of the POV
- `T#2022.Months` - All months of 2022
- `T#YearPrior1(|PovTime|)PeriodNext1(|PovTime|)` - Prior year, next period
- Time Functions pivot the member; Time Expansions extend the member

**Common Substitution Variables:**

| Variable | Description |
|----------|-------------|
| `|UserName|` | Name of the user who ran the report |
| `|CVName|` | Name of the Cube View |
| `|POVTime|`, `|WFTime|`, `|GlobalTime|` | Time member by source |
| `|WFTimeDesc|` | Description (e.g., "Feb 2022") |
| `|WFTimeShortDesc|` | Short description (e.g., "Feb") |
| `|DateDDMMYYYY|` | Current date |
| `|Text1|` | Text properties of the member |
| `|WFProfile|` | Name of the current Workflow Profile |

The prefixes indicate where the value is taken from: CV (Cube View POV), WF (Workflow), POV (Cube POV), MF (Member Filter), Global. Add `Desc` as a suffix to show the description.

#### Renaming rows/columns

Use the `:Name()` function at the end of a Member Filter:
- `A#CashBalance:Name(Cash Balance)` displays "Cash Balance"
- Double quotes around the name are optional: `A#60999:Name("Net Sales")` or `A#60999:Name(Net Sales)`
- With XFMemberProperty: `A#6009.base:Name(XFMemberProperty(DimType=Account, Member=|MFAccount|, Property=AccountType))`

#### Cube View Performance

**Database Sparsity:** It is the relationship between the volume of records and modeled dimensions. The absence of data records affects performance because it is difficult to render reports with sparse data. Use Analytic Blend, extensibility, or other frameworks to minimize it.

**Row and Column Suppression:**

![Row and column suppression options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1073-4482.png)

| Property | Description |
|----------|-------------|
| Suppress Invalid Rows/Columns | Suppresses invalid cells |
| Suppress NoData Rows/Columns | Suppresses cells with no data (set to False for data entry) |
| Suppress Zero Rows/Columns | Suppresses cells with zero |
| Use Suppression Settings on Parent Rows/Columns | Controls whether parent members use the same configuration |
| Zero Suppression Threshold | Threshold value; numbers below are treated as zero (e.g., 499.99 suppresses everything below) |
| Allow Insert Suppressed Member | Options: All, False, Nested, Innermost (rows only) |
| Use to Determine Row Suppression | (Columns only) True improves performance in large Cube Views |
| Allow Sparse Row Suppression | (Columns only) Improves performance with multiple nested dimensions in rows |

**Allow Insert Suppressed Member Values:**
- **All:** Visibility to all row expansions of the Cube View
- **False:** All expansions remain suppressed
- **Nested:** Visibility of row expansions 2 through 4
- **Innermost:** Visibility of the row expansion at the lowest level

**Sparse Row Suppression:**
- Evaluates data records and filters records without data BEFORE rendering the Cube View.
- It is enabled in General Settings > Common > `Allow Sparse Row Suppression = True`.
- **Cannot be applied to dynamically calculated data** (calculated members and Cube View math).
- For columns with dynamic data: set `Use to Determine Row Suppression = True` and `Allow Sparse Row Suppression = True`.
- Any row with a suppression setting enabled will be eligible for sparse row suppression. If no suppression is applied, sparse row suppression will not be applied.

**Steps to enable Sparse Row Suppression:**
1. Go to Cube Views > select Cube View.
2. General Settings > Common > Suppression > `Allow Sparse Row Suppression = True`.
3. Select Rows and Columns > select the row > Data tab.
4. Set the additional suppression properties to True.
5. Select the column > Data tab > `Allow Sparse Row Suppression = True`.
6. If the column has dynamic data, set `Use to Determine Row Suppression = True`.

**Cube View Paging:**

Applies only to the Data Explorer view when there are more than 10,000 unsuppressed rows.

| Evaluation | Description |
|------------|-------------|
| **Evaluation 1 - Enable Paging** | Evaluates the total potential unsuppressed rows. If < 10,000, paging is not enabled. |
| **Evaluation 2 - Paging Enabled** | If >= 10,000 rows, paging is enabled. |
| **Evaluation 3 - Paging** | Attempts to return 20 to 2,000 unsuppressed rows within a maximum of 20 seconds. |

Properties:
- `Max Unsuppressed Rows Per Page` (default -1, max 100,000): Determines how many rows are written before paginating.
- `Max Seconds To Process` (default -1, max 600): Determines how many seconds to process before paginating.

When there are nested dimensions in rows, the paging evaluation is performed on the leftmost dimension. The percentage shown is not a precise measurement because each page is generated by processing time.

#### Important Cube View General Settings

| Property | Description |
|----------|-------------|
| **Is Visible in Profiles** | If True, the Cube View is visible in the Profile; if False, only on the Cube Views page |
| **Page Caption** | Text at the top of the Data Explorer grid |
| **Is Shortcut** | Determines if it is a shortcut to another Cube View |
| **Is Dynamic** | Allows programmatic manipulation of format and data at runtime via Workspace Assembly |
| **Can Modify Data** | True = editable in OnePlace; False = read-only |
| **Can Calculate/Translate/Consolidate** | True, True (Without Force), or False |
| **Can Modify Suppression** | If True, the user can change suppression in Data Explorer |

#### Cube View with Workflow

- **Workflow Entities:** Use `E#Root.WFProfileEntities` in Rows/Columns (not in POV, because there can be more than one entity). WFProfileEntities displays the entity or entities assigned to a Workflow Profile at runtime.
- **Workflow Scenario:** Select WF Member in the POV Slider for Scenario, or use `|WFScenario|` in Rows/Columns.
- **Workflow Time:** Select WF Member in the POV Slider for Time, or use `|WFTime|` in Rows/Columns.

#### Cube View with Parameters

- Reference parameters in POV and Rows/Columns: `|!ParameterName!|`
- If a parameter is not found associated with the Form, OneStream searches the application's Dashboard Parameters.
- Parameters are used as filters to focus the displayed data.

![Example of parameter usage in the Cube View POV](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1081-4507.png)

#### Cube View Shortcuts

Allow opening the same Cube View with different parameter values without maintaining multiple Cube Views. Example: Income Statement with `ParamView = [YTD]` and another version with `ParamView = [Periodic]`. Each shortcut has the same name but a different literal parameter value.

#### Cube View Formatting

**Format priority order:**
1. Application Properties (standard report settings)
2. Cube View Default settings
3. Column settings
4. Row settings
5. Column Row overrides
6. Row Column overrides

The combination of formats and overrides determines the final cell format.

![Formatting options in the Formatting tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1083-4519.png)

**Format outputs:** Data Explorer, Excel, Report - each with specific options.

**Data Explorer Formatting:**
- Text: Font, color, size, bold, italic, NumberFormat, ZeroOffset, Scale, FlipSign, ShowPercentageSign, ShowCurrency
- Border: Background color, gridline color
- Column: IsColumnVisible, ColumnWidth

**Excel Formatting:**
- Text: Color, horizontal/vertical alignment, indent level, wrap, number format, use scale
- Border: Background color, border color/line styles
- Column: Column width

**Report Formatting:**
- Text: Color, alignment, size, underline, NoDataNumberFormat, UseNumericBinding
- Border: Background color, cell borders
- Lines: Top/bottom lines, padding, color, thickness
- Column: Column width, Row height

**Cell Format Options:**

| Property | Description |
|----------|-------------|
| NumberFormat | .NET numeric format (e.g., `#,### ;(#,###);0` for positive;negative;null) |
| ZeroOffsetForFormatting | Related to NegativeTextColor; allows displaying numbers < X in red |
| Scale | -12 to +12 (3 = thousands, 6 = millions) |
| FlipSign | Inverts positive/negative for display |
| ShowPercentageSign | Shows percentage symbol |
| ShowCurrency | Shows currency code (e.g., EUR) |
| NegativeTextColor | Color for negative numbers |
| WritableBackgroundColor | Background color for editable cells |
| ExcelNumberFormat | Specific numeric format for Excel export |
| ExcelUseScale | Whether Excel uses the Scale property |
| ReportNoDataNumberFormat | Format for NoData cells in reports (e.g., "NODATA" or "#" for blank) |
| ReportUseNumericBinding | True = numbers instead of text when exporting report to Excel |

**Detailed Header Format:**

| Property | Description |
|----------|-------------|
| RowExpansionMode | Controls expansion of nested rows in Data Explorer |
| ShowDimensionImages | False hides dimension icons in headers |
| HeaderWrapText | True = text wrap in row and column headers |
| IsColumnVisible | False hides specific columns; can be used with a parameter at runtime |
| ColumnWidth | Column width in pixels |
| ColumnHeaderWrapText | True = wrap in individual column headers (requires ColumnWidth) |
| MergeAndCenterColumnHeaders | True = center column headers |
| IsRowVisible | False hides specific rows |
| TreeExpansionLevel1-4 | Automatically expands up to 4 nested levels (0 = collapse all) |
| RowHeaderWrapText | True = wrap in individual row headers (requires Row Header Width) |
| ExcelMaxOutlineLevelOnRows/Cols | Up to 6 outline levels for Excel |
| ExcelExpandedOutlineLevelOnRows/Cols | Initial expansion level when exporting to Excel |
| ReportRowPageBreak | Applies page break in reports |

![Example of TreeExpansionLevel with levels 1, 2, and 3 expanded](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1093-4563.png)

**TreeExpansionLevels:** They are linked to the Dimension Types of Member Filter in the Designer tab and require a .Tree member expansion. For it to work, the Default Header's RowExpansionMode must be set to "Use Default". Using Collapse All or Expand All overrides this setting.

**Conditional Formatting:**

Applied to Default, Headers, Cells, Row/Column overrides. Follows the same order of operations as basic formatting.

![Conditional Formatting dialog with condition, filter, operator, and text sections](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1106-4601.png)

**Header Property Filters:**

| Filter | Description |
|--------|-------------|
| IsRowHeader | Boolean, whether it is a row header |
| IsColHeader | Boolean, whether it is a column header |
| RowName | Name of the Cube View row |
| ColName | Name of the Cube View column |
| ExpansionLevel | Expansion level for rows (1-4) and columns (1-2) |
| HeaderDisplayText | Custom descriptions with :Name() |
| MemberName | Member labels from metadata |
| MemberDescription | Member descriptions from metadata |
| MemberShortDescription | Short descriptions (Time dimension only) |
| IndentLevel | Indentation level (derived from formatting or generated by tree expansions) |

**Cell Format Property Filters:**

| Filter | Description |
|--------|-------------|
| IsNoData | Tests if there is no data |
| IsRealData | Tests stored data, ignoring Zero-View derived data |
| IsDerivedData | Tests derived data (commonly from Scenario Zero-View) |
| IsRowNumberEven | Tests if the row number is even |
| ExpandedRowNum | Count of expanded rows (zero-based, Cube View total) |
| CellAmount | Tests the cell's data amount |
| CellStorageType | Tests the data storage method |

Conditional formatting can be saved as a Literal Parameter for reuse. Supports If/ElseIf/Else for complex logic.

**Conditional "Traffic-Lighting" Example:** Applied as Cell Format. The designer chooses whether to apply to row or column. Row overrides are the final formatting layer.

**IndentLevel Example:** The IndentLevel filter dynamically formats from defined rows or expansions. Indentation is zero-based.

![IndentLevel conditional formatting example](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1111-4613.png)

#### Data Explorer - Functionalities in OnePlace

![Cube View in OnePlace with toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1953-7353.png)

**Cube View Toolbar:**
- **Consolidate:** Consolidates the Cube View data
- **Translate:** Translates the Cube View data
- **Calculate:** Runs calculation on the Cube View data
- **Row Suppression:** Use Default, Suppress Rows, Unsuppress Rows
- **Show Report:** Polished PDF-style format
- **Export to Excel:** Exports the formatted Cube View to Excel (with sequential numbering)
- **Select Parameters:** Select new parameters
- **Edit Cube View:** Launch Application tab to modify (depending on security)
- **Find Next Row:** Search for a row by keyword (works with collapsed rows)

**Data entry shortcuts:**

| Shortcut | Function | Example |
|----------|----------|---------|
| `add` | Add a number to the value | `add2k` adds 2,000 |
| `sub` | Subtract | `sub500` subtracts 500 |
| `div` | Divide | `div2` divides by 2 |
| `mul` | Multiply | `mul3` multiplies by 3 |
| `in` / `increase` / `gr` | Increase by percentage | `in10` increases 10% |
| `de` / `decrease` | Decrease by percentage | `de5` decreases 5% |
| `per` / `percent` | Calculate percentage | `per50` calculates 50% |
| `pow` / `power` | Calculate power | `pow2` squares the value |

**Entry scaling:** `k` = x1,000; `m` = x1,000,000; `b` = x1,000,000,000; `%` = divide by 100

**Hotkeys:** CTRL+S = Save; CTRL+C/V = Copy/Paste values in cells (select multiple cells with CTRL)

**Right-Click Options:**

**Expand/Collapse:** If nested rows are used, right-click on the row header to expand/collapse. Works with RowExpansionMode.

**Calculate/Translate/Consolidate:** Can be enabled/disabled individually on each Cube View. Includes Force and with Logging options.

**Spreading:**

![Accumulate Spreading example with rate 1.5](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1960-7382.png)

| Type | Description |
|------|-------------|
| **Fill** | Fills each selected cell with the value in Amount to Spread |
| **Clear Data** | Clears all data from the selected cells |
| **Factor** | Multiplies the cell value by the specified rate |
| **Accumulate** | Takes the value of the first cell, multiplies it by the rate, and applies that result to the next cell successively |
| **Even Distribution** | Distributes the Amount to Spread evenly among selected cells |
| **Proportional Distribution** | Distributes proportionally based on existing values in the cells |
| **445 Distribution** | Weight of 4 to the first 2 cells, weight of 5 to the third |
| **454 Distribution** | Weight of 4, 5, 4 respectively |
| **544 Distribution** | Weight of 5, 4, 4 respectively |

**Spreading Properties:**
- **Amount to Spread:** Value to distribute (default = last selected cell)
- **Rate:** Only for Factor and Accumulate
- **Retain Amount in Flagged Input Cells:** True = spreading does not apply to flagged cells
- **Include Flagged Read only Cells in Totals:** True (default) includes locked cells in totals
- **Flag Selected Cells:** Retains the original value during spreading
- **Clear Flags:** Clears flagged cells

The Spreading dialog can be left open during data entry. To select multiple cells, use CTRL+click. Spreading occurs first across rows and then down columns. When spreading across time periods, you can double-click on the Parent Time Member (e.g., Q1) to automatically select its base periods.

![Spreading example in Q1 with base periods](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1964-7397.png)

**Allocation:**

| Type | Description |
|------|-------------|
| **Clear Data** | Clears Form data for the specified Destination POV |
| **Even Distribution** | Even distribution among Destination Members |
| **445/454/544 Distribution** | Same logic as Spreading but for allocations |
| **Weighted Distribution** | Applies calculated weights to each Destination Member |
| **Advanced** | Similar to Weighted but allows override of 2 destination dimensions and weight control |

**Allocation Properties:**
- **Source POV:** Determines the source intersection (default = last selected cell). Can drag-and-drop from cell.
- **Source Amount or Calculation Script:** Override of the Source POV value (e.g., `A#SourcePOV*0.90`)
- **Destination POV:** Destination intersection for the allocation. One member per dimension is required.
- **Dimension Type/Dimension Type 2:** Additional dimensions not included in Destination POV (up to 2 for Clear Data and Advanced)
- **Member Filter/Member Filter 2:** Specific members that override the Destination POV
- **Save Zeroes as Not Data:** True (default) to suppress zeros when saving
- **Weight Calculation Script:** Script to calculate weights using `|SourceAmount|`, `|Weight|`, `|TotalWeight|`
- **Destination Calculation Script:** How to calculate the Weight (default: `|SourceAmount| * (|Weight|/|TotalWeight|)`)
- **Translate Destination If Different Currency:** True to translate if the destination currency is different

**Offset in Allocations:**
- **Source Transfer POV:** Transfer entry that zeroes out the Source Amount
- **Source Transfer Offset POV:** Balance entry at the Source (usually a different Account)
- **Destination Offset POV:** Balance entry at the Destination

![Allocation example with offset entries](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1970-7415.png)

**Data Attachments:**
- Added at the cell level or full Data Unit level
- Types: Standard, Annotation, Assumptions, Audit Comment, Footnote, Variance Explanation
- There can be many Standard attachments per cell, but only one of each other type
- Non-Standard types are part of the View Dimension and can be displayed in Cube View rows/columns
- **Spell Check:** Available only in Windows Application, English only, via right-click on Data Attachments

![Data Attachments dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1975-7432.png)

**Cell Detail:**
- Available in O#Forms or O#BeforeAdj
- Can be loaded via Excel/CSV template
- Includes: Amount, Aggregation Weight (e.g., -1 to invert), Classification (via Dashboard Parameter `CellDetailClassifications`), Description
- **Apply Import Offset:** For budgets, applies the reverse of the Origin Member Import
- **Remove Import Offset:** Removes a previously applied Import Offset


![Cell Detail toolbar and form](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1980-7450.png)

**Cell POV Information:** Hovering over a tick mark shows POV members in a tooltip. Right-click > Cell POV Information shows a detailed summary with copyable XFGetCell and XFCell formulas.

![Cell POV Information dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1983-7461.png)

**Cell Status:** Right-click > Cell Status shows status properties and dimensional information of the cell.

**Data Unit Statistics:** Right-click > Data Unit Statistics shows: zero cells, real, derived, NODATA, calculated, consolidated, translated, journal, input, etc.

**Drill Down:**

![Drill Down with results and bread crumb trail](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1987-7473.png)

You can drill from any cell; it **does NOT need to be base level**. White cells = base amounts. Green cells = can continue drilling.

| Option | Description |
|--------|-------------|
| **Entity Children Contribution** | (Entity only) Shows each Entity's contribution to the Parent |
| **Local Currency** | (Consolidation only) Shows Entities in local currency |
| **Member Children** | Drill to the member's children |
| **Base** | Drill to base cells (white) |
| **All Aggregated Data Below Cell** | Drill to white cells in each dimension |
| **All Stored Data in Data Unit** | Drill to all base data of the full Data Unit |
| **Copy POV from Data Cell** | Copies the POV of the selected cell |
| **Create Quick View Using POV** | Creates a Quick View based on the selected row's POV |
| **Cell POV Information** | View the full POV of the cell |
| **Cell Status** | Information about the cell (children, calculated, lock status) |
| **Calculation Inputs** | Details of formula source accounts |
| **Load Results for Imported Cell** | Drill back to imported source data |
| **Audit History for Forms/Adjustment Cell** | Drill back to Form or Journal data |

**Origin Audit Drill Down:** Right-click Origin cell (Top) > Origin Base reveals data by each Origin Member. Then you can drill into Import, Forms, or AdjInput (Journals).

**Show Cube View As Report:** Generates a polished PDF-style formatted report. Control column widths and row heights from the Cube View. Can be exported to PDF, HTML, RTF, CSV, Text, XPS, MHT, or Excel.

![Cube View as formatted Report](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch21-p1992-7488.png)

---

### 201.4.2: Workspace Scope

#### What is a Workspace?

A Workspace is a structured sandbox-type development space where teams can create, modify, and test solutions without interfering with other workflows. They store **Maintenance Units**, which serve as organizational containers that group related components, configurations, and enhancements.

![Example of Workspace organization by functional area](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p30-796.png)

#### Main purpose

- Facilitate collaboration and efficient development across teams, applications, and environments
- Promote modularity and solution reuse
- Act as controlled development containers that preserve application integrity
- Allow experimentation without impacting production systems
- Accelerate development cycles and improve efficiency

#### Hierarchical structure

```
Workspace
  -> Maintenance Units
       -> Dashboard Groups
            -> Dashboards
                 -> Components, Data Adapters, Parameters, Files, Strings
       -> Cube View Groups
            -> Cube Views
       -> Data Management Groups
            -> Sequences -> Steps
       -> Assemblies
```

![Workspace structure with Maintenance Units, Dashboard Groups, Cube View Groups, and Data Management Groups](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p45-991.png)

- **Default Workspace:** Contains all legacy objects (pre-Workspaces). Cube View Groups and Data Management Groups not in a new Workspace are located under the Default Maintenance Unit of the Default Workspace.
- **Dashboard Profiles and Cube View Profiles** are global (not specific to a Workspace) and remain in the same location.

![Global Profiles in the Workspace structure](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p45-989.png)

#### Where Cube Views are found (3 locations)

1. Default Workspace > Default Maintenance Unit (legacy Cube Views)

![Legacy Cube Views under Default Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p59-1104.png)

2. Default Workspace > Specific Maintenance Unit (new Cube Views in Default)

![Cube Views in a specific Maintenance Unit of the Default Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p60-1109.png)

3. Non-Default Workspace > Specific Maintenance Unit (Cube Views of new Workspaces)

![Cube Views in a non-default Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p60-1112.png)

**Important:** Cube Views created outside the Default Workspace CANNOT be accessed from the Cube View page of the Application tab.

#### Workspace Properties

**General:**

| Property | Description |
|----------|-------------|
| **Name** | Name of the Workspace. Conventions: by development group, functionality, workstream, person, or team. |
| **Description** | Additional context about the Workspace's purpose. |
| **Notes** | Free-form field to document completion dates, status, reminders. |
| **Substitution Variable Items** | Reusable variables within the Workspace. Accessed via (Collection) with ellipsis button. |

**Workspace Substitution Variables:**
- Syntax: `|WSSVNameSuffix|` (prefix `WSSV` followed by the suffix name)
- Example: Variable with suffix `Developer` -> referenced as `|WSSVDeveloper|`
- Created from the Substitution Variable Items dialog: click Add Item, enter Suffix and Value
- Used in dashboards, Page Caption, and other Workspace objects

![Workspace Substitution Variable Items dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p53-1043.png)

![Dashboard showing substitution variable values in the Page Caption](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p56-1080.png)

**Security (two levels):**
- **Access Group:** Users who can view the Workspace and its dashboards, but NOT modify. Grayed-out text = read-only.
- **Maintenance Group:** Users who can access, view, AND modify the Workspace and its objects. Depending on the security structure, users can belong to multiple groups or nested groups.

![Security configuration in the Workspace - Access and Maintenance Groups](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p54-1058.png)

**Sharing:**

| Property | Description |
|----------|-------------|
| **Is Shareable Workspace** | If True, other Workspaces can reuse objects from this Workspace. Facilitates "outward" sharing. |
| **Shared Workspace Names** | Comma-separated list of Workspaces from which you want to reuse objects. Facilitates "inward" sharing. Order the list according to how you want them to appear in searches. |

![Is Shareable Workspace property set to True](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p26-767.png)

![Shared Workspace Names property with tooltip](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p27-776.png)

**Requirements for sharing objects between Workspaces:**
1. The source Workspace must have `Is Shareable Workspace = True`
2. The destination Workspace must list the source Workspace name in `Shared Workspace Names`
3. The Default Workspace is always shared with any other Workspace, without needing to list it

**Assemblies:**

| Property | Description |
|----------|-------------|
| **Namespace Prefix** | Short name to reference the Workspace in Assemblies. Allows writing syntax that executes code from other Workspaces. |
| **Imports Namespace 1-8** | Reference to Assemblies of other Workspaces. When an Assembly depends on another Workspace, these properties allow dynamically referencing that dependency. |
| **Workspace Assembly Service** | Name of the Assembly Service Factory (`AssemblyName.FactoryName`). FactoryName is the class name (without extension). |

**Text 1-8:** Fields to store string values referenceable from Assemblies. Allows Assemblies to be more dynamic without needing to modify code when the value changes.

Example reference in Assembly:
```
Dim WSText1 As String = BRApi.Dashboards.Workspaces.GetWorkspace(si, False, args.PrimaryDashboard.WorkspaceID).Text1
```

#### Naming Conventions in Workspaces

The **same object name** can be used multiple times, as long as they are in separate Workspaces. This applies to: Maintenance Units, Dashboards, Cube Views, Data Management Jobs, Components, Data Adapters, Files, Strings, and Assemblies.

#### Security Roles for Workspaces

| Security Role | Type | Permission |
|---------------|------|------------|
| **AdministerApplicationWorkspaceAssemblies** | Application Security Role | Create and modify Workspace Assemblies |
| **ManageApplicationWorkspaces** | Application Security Role | Create and modify Workspaces. Creation can be limited to certain users. |
| **WorkspaceAdminPage** | Application UI Role | Access to the Workspaces page from the Application tab |

![Workspace Security Roles in Application Security Roles](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p50-1017.png)

#### Steps to create a Workspace

1. Navigate to Application tab > Presentation > Workspaces.
2. Click on Workspaces and then click on Create Workspace.
3. Type the Workspace name and click Save.
4. Configure Description and Notes (optional).
5. Configure Substitution Variables (optional).
6. Assign Access Group and Maintenance Group.
7. Configure Is Shareable Workspace and Shared Workspace Names as needed.
8. Save.

![Workspace creation steps](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p52-1032.png)

When renaming Workspaces, verify the inventory of Workspaces that list its name as a Shared Workspace to update references.

#### Security configuration example

| Group | Name | Role | Workspace |
|-------|------|------|-----------|
| DeveloperGroup1 | Eric Telhiard | Maintenance | Annual Operating Plan |
| DeveloperGroup2 | Jessica Toner | Maintenance | Close and Consolidation |
| DevelopmentQA | Tom Shea | Access | Both Workspaces |

![Maintenance user seeing only their Workspace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p34-833.png)

![QA user with Access (read-only) seeing both Workspaces](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p35-838.png)

Each developer only sees the Workspaces where they have rights. The QA user (Access) sees both Workspaces but cannot modify anything (grayed-out text = read-only).

#### Key benefits of Workspaces

| Benefit | Description |
|---------|-------------|
| **Isolated development** | Dedicated sandboxes without impacting existing dashboards |
| **Resolves naming conflicts** | Same object name in different Workspaces |
| **Eliminates environment uncertainty** | No more guesswork when migrating between sandbox and production |
| **Solution packaging** | Export/import an entire Workspace as a single XML file |
| **Parallel development** | Multiple developers work simultaneously without disruptions |
| **Real-time QA** | Testing within the same instance without environment migration |
| **Selective sharing** | Only content marked as shareable is shared |
| **Backward compatibility** | Does not require rebuilding existing solutions |

![Solution packaging - Export a single Application Workspaces item](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p29-789.png)

#### Workspace vs Traditional Development

| Aspect | Traditional | Workspaces |
|--------|-------------|------------|
| Migration | Requires taking app offline, copying DB from sandbox to production | Development, testing, and deployment within the same instance |
| Disruptions | Users blocked during update | Parallel development without affecting operations |
| Export | Multiple individual objects (Cube Views, DM jobs, etc.) | A single XML file with the entire solution |
| Naming conflicts | Requires unique names across the entire application | Allows duplicate names in separate Workspaces |
| QA | Requires migration to another environment | Testing in the same instance with refresh |

#### Workspace Filter Functionality

Workspace Filters allow users to refine the Workspaces shown in their filter selections, making navigation and organization easier. Developers can focus on their dedicated Workspaces without seeing other Workspaces.

![Workspace Filter Functionality](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch03-p56-1079.png)

#### Dashboard Types

| Type | Description |
|------|-------------|
| **Use Default** | Default standard configuration |
| **Top Level** | Dashboard exposed at the top level of the OnePlace menu. Controls which dashboard is visible in the main menu. |
| **Top Level Without Parameter Prompts** | Top Level without prompting the user for parameters. Useful for showing a specific view without giving the option to modify parameters. |
| **Embedded** | Dashboard nested within another dashboard (not directly visible in OnePlace). Most common for creating hierarchical dashboards. |
| **Embedded Dynamic** | For objects defined in Workspace Assemblies |
| **Embedded Dynamic Repeater** | Shows multiple instances of an object using Component Template Repeat Items |
| **Custom Control** | Uses dashboard templates with Event Listeners |

![Dashboard Type configured as Top Level](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch02-p40-867.png)

#### Workspace Components

**Maintenance Units:** Groups of dashboards and components. Allow separation of workstreams.

![Workspace with four Maintenance Units](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p58-1097.png)

**Dashboard Groups:** Dashboards grouped by functionality or other criteria. The top-level dashboard is exposed in OnePlace via dashboard profile.

**Data Management Groups:** Automate tasks (load data, run calculations, trigger business rules).
- **Steps:** Individual process blocks.
- **Sequences:** Ordered series of one or more steps that execute in order. The Sequence name is used to trigger actions from dashboard components.

**Data Adapters (5 types):**

| Type | Description |
|------|-------------|
| **Cube View** | Pre-configured Cube View as data source (most common). Live connection to cube data. |
| **Cube View MD** | Cube View as multidimensional fact table (for BI Viewer) |
| **Method** | Custom Business Rule or out-of-the-box queries |
| **SQL** | Queries against the application or framework database |
| **BI-Blend** | Predefined interface for querying BI Blend tables |

![Cube View type Data Adapter](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p63-1142.png)

**Parameters (6 types):**

| Type | Description |
|------|-------------|
| **Literal Value** | Explicit value, updatable via business rules or Assemblies |
| **Input Value** | Holding parameter for passing values via dashboard actions |
| **Delimited List** | Manual list of items available for selection |
| **Bound List** | Member lists and properties using Method Types |
| **Member List** | Uses Member Filter Builder to create a metadata member list |
| **Member Dialog** | Member list with selection tree (member tree component) |

![Literal Value type Parameter](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p66-1164.png)

**Components (38 types, main ones):**

| Category | Components |
|----------|------------|
| **Data & Visualization** | BI Viewer, Book Viewer, Chart (Basic), Chart (Advanced), Cube View, Data Explorer, Data Explorer Report, Gantt View, Grid View, Large Data Pivot Grid, Map, Pivot Grid, Report, Sankey Diagram, Spreadsheet |
| **Navigation & Input** | Button, Check Box, Combo Box, Date Selector, Embedded Dashboard, Filter Editor, etc. |

![List of the 38 available Component Types](/Users/aurelio.santos/Desktop/OneStreamDoc/output/workspaces-assemblies/images/workspaces-assemblies-ch04-p69-1186.png)

**Key difference: Cube View Component vs Cube View:**
The Cube View Component is an object placed on a dashboard that displays a Cube View. It allows controlling what happens when the user interacts with the Cube View (e.g., refreshing another part of the dashboard).

---

### 201.4.3: Steps to Create a Report Book

#### What is a Report Book?

Allows combining multiple reports and files into a single document. They are commonly used to create financial statements and management reporting packages. Types: **PDF Books**, **Excel Books**, **Zip File Books**.

#### File extensions

| Type | Extension |
|------|-----------|
| Excel Book | `ReportBookName.xfDoc.xlBook` |
| PDF Book | `ReportBookName.xfDoc.pdfBook` |
| Zip File Book | `ReportBookName.xfDoc.zipBook` |

#### Save locations

- File in OneStream File System (File Explorer)
- File on Local Folder
- Application Workspace File (in Maintenance Unit)
- System Workspace File

![Book Designer Toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1034-4347.png)

#### Book Properties

| Property | Description |
|----------|-------------|
| **Determine Parameters from Content** | If True, automatically determines the required parameters. For large books, set to False and specify manually (improves performance). |
| **Required Input Parameters** | Comma-separated list of parameter names (leave empty if Determine Parameters = True). |

#### Items that can be added to a Book

| Item | Description |
|------|-------------|
| **File** | File from external source (URL, Workspace File, Database File, File Share File) |
| **Excel Export Item** | For XLBooks - specify Cube View to export (each in a separate tab) |
| **Report** | Cube View, Dashboard Chart, Dashboard Report, System Dashboard Chart, System Dashboard Report |
| **Loop** | Sequence that repeats a process |
| **If / Else If / Else** | Conditional statements |
| **Change Parameters** | Customize output without altering the source |

**Note about Excel Export Items:** Report and File items are NOT supported by Excel Books and are ignored if added.

**Report Properties:**
- Cube View or Component Name: Name of the Cube View or Dashboard component
- Output Name: Name for the report in Zip books (optional)
- Include Report Margins/Report Header/Page Header/Report Footer/Page Footer: True/False

#### File Source Types

| Type | Description |
|------|-------------|
| **URL** | File from internal or external web page (full URL) |
| **Application Workspace File** | File in Maintenance Unit File Section |
| **System Workspace File** | File in System Workspace Maintenance Unit File Section |
| **Application Database File** | File in Application Database Share |
| **System Database File** | File in System Database Share |
| **File Share File** | File from the File Share |

Many file types can be added: Word, PDF, CSV, XLSX.

#### Loops

**3 Loop types:**

| Type | Description | Definition example |
|------|-------------|-------------------|
| **Comma Separated List** | Comma-separated list of values | `Houston, Clubs, [Houston Heights]` |
| **Dashboard Parameter** | Pre-configured parameter | `ParamSalesRegions` |
| **Member Filter** | Member Filter Builder based on dimension members | `E#Frankfurt, E#Houston` or `E#[NA Clubs].Base` |

**Loop Variables:**

| Variable | Description |
|----------|-------------|
| `|Loop1Variable|` to `|Loop4Variable|` | References loop values by name (up to 4 nested loops) |
| `|Loop1Display|` to `|Loop4Display|` | References values by description |
| `|Loop1Index|` to `|Loop4Index|` | Assigns sequential number starting at 1 |

#### Change Parameters

Used to enhance data in Report Books. Loops **require** Change Parameters to execute. When a Change Parameter is encountered in a loop, the Loop Variable is updated to the next value.

![Change Parameters within a Loop](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1042-4364.png)

| Category | Properties |
|----------|------------|
| **Workflow** | Change Workflow (True/False), Workflow Profile, Workflow Scenario, Workflow Time |
| **POV** | Change POV (True/False), Member Script (e.g., `E#[|Loop1Variable|]:A#Sales`). The Cube View's POV tab must NOT have members selected. |
| **Variables** | Change Variables, Variable Values (up to 10 variables: `Variable1=Red, Variable2=Large`) |
| **Parameters** | Change Parameters, Parameter Values (parameter override: `MyParam=Red, MyOtherParam=[|Loop1Variable|]`) |

**Note:** Use `!!` to display Display Items in headers. Use `!` to display Value items.

#### If Statements

Provide conditional logic to Report Books. They are frequently combined with Loops.

**Examples:**
- `(|Loop1Variable| = [Frankfurt])` - If the Loop Variable is Frankfurt
- `(|!UserName!| = Administrator)` - If the user is Administrator
- `(|!UserName!| = Administrator) Or (|!UserName!| = JSmith)` - Combine with Or/And

**Else and Else If:** Require a prior If statement. Allow additional logic.

![Loop example with If/Else If/Else](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1047-4379.png)

#### Complete steps to create a Report Book

1. **Open OneStream** and navigate to Application tab > Presentation > Books.
2. **Click on Create New Book** in the toolbar.
3. **Configure Book Properties:**
   - Determine Parameters from Content: True or False
   - Required Input Parameters: list of parameters if Determine = False
4. **Add items** with the Add Item button:
   - File, Excel Export Item, Report, Loop, If Statement, Change Parameters
5. **Configure Loops (if applicable):**
   - Select Loop Type (Comma Separated List, Dashboard Parameter, Member Filter)
   - Define Loop Definition
   - Add Loop Variables
6. **Configure Change Parameters within Loops:**
   - Workflow, POV, Variables, Parameters as needed
7. **Configure If Statements (if applicable)**
8. **Remove** unnecessary items with Remove Item.
9. **Drag and drop** to reorder items.
10. **Save** with Save As selecting location and book type (extension).
11. **Preview** in the Preview tab.
12. **Close** with Close Book.

**Note about PDF Embedded Fonts:** They can increase PDF size. Use PDF Embedded Fonts to Remove in Application Server Configuration File. Default: Arial; Calibri; Segoe UI; Tahoma; Times New Roman; Verdana.

#### Book Preview Toolbar

![Book Preview Toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch12-p1049-4397.png)

| Function | Description |
|----------|-------------|
| Page Navigation | Navigate to specific page |
| First/Last | Go to first/last page |
| Previous/Next | Navigate forward/back one page |
| **Combine All Items** | Combines all pages as a single document. Required for saving/printing the complete book. |
| **Download Combined PDF File** | Generates combined PDF using Adobe rendering (recommended method). Does not require Combine All Items. |
| Refresh | Update and select new parameters |
| Close | Close the report book in Preview |
| Open | Open report book from desktop or folder |
| Save | Save current page (or entire book if Combine All Items is active) |
| Print | Print (entire book if Combine All Items is active) |
| Find | Search for keywords |
| Zoom | Zoom in/out of the report book |

**Right-Click Options in Preview:**
- **Select Tool:** Select portions to copy/paste (CTRL+C/CTRL+V)
- **Hand Tool:** Scroll through the report
- **Marquee Zoom:** Select area to zoom in (Alt+Left to go back, Ctrl+0 for page view)
- Print, Find, Select All

#### Usage of Report Books

After creating them, they can be:
- Added to other books
- Added to dashboards via Book Viewer or File Viewer component
- Sent by email via OneStream Parcel Service
- Generated by running a Data Management sequence
- Stored in: FileShare, desktop, dashboard file

#### Dashboards in OnePlace

![Dashboards in OnePlace](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch22-p1993-7491.png)

Dashboards are combined series of reports, grids, charts, and graphics. When selecting a Dashboard, it may prompt for predefined Parameters.

**Dashboard Toolbar:**

![Dashboard Toolbar](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch22-p1994-7494.png)

| Icon | Function |
|------|----------|
| **Select Parameters** | Select specific parameters |
| **Reset Parameter Selections and Refresh** | Change parameters and refresh |
| **Edit Dashboard** | Launch Application tab to modify (depending on security) |
| **Print** | Print from web or via PDF. For complete book: right-click > Combined PDF File or PDFs In Zip File. |

---

## Critical Points to Memorize

### Cube Views
- They are the **"building blocks of reporting"** of OneStream.
- They are organized in **Groups** (container) and **Profiles** (exposure/access).
- The **POV slider takes priority** over the POV Pane.
- Up to **4 dimensions can be nested in rows** and **2 dimensions in columns**.
- **WFProfileEntities** is used in Rows/Columns (not in POV, because there can be multiple entities).
- **Substitution Variables** are referenced with pipes: `|WFTime|`, `|UserName|`, `|POVTime|`.
- **Parameters** in Cube Views are referenced with pipes and exclamation marks: `|!ParameterName!|`.
- **Sparse Row Suppression:** Improves performance by filtering records without data before rendering. Does not apply to dynamically calculated data. Enabled in General Settings AND in columns.
- **Paging:** Automatically activates with more than 10,000 unsuppressed rows. Attempts to return 20-2,000 rows in max 20 seconds.
- **Can Modify Data = False** makes the Cube View read-only, but annotations are still allowed.
- **Rename with :Name()** allows changing the displayed name in headers.
- **Conditional Formatting** follows the order: Application Properties > Default > Column > Row > Column Override > Row Override.
- **Spreading Types:** Fill, Clear Data, Factor, Accumulate, Even Distribution, Proportional, 445/454/544.
- **Allocation Types:** Clear Data, Even Distribution, 445/454/544, Weighted, Advanced.
- **Drill Down** works from any cell, does NOT need to be base level. Green cells = can continue drilling.
- **Cube Views created outside the Default Workspace** are NOT accessible from the Cube Views page of the Application tab.
- **Data Attachments:** Standard (multiple), other types (only one per cell). They are part of the View Dimension.
- **Cell Detail:** Only available in O#Forms or O#BeforeAdj.
- **TreeExpansionLevel:** 0 = collapse all, 1-4 = expand levels. Requires RowExpansionMode = "Use Default".
- **Where(HasChildren = True):** Only works for dimensions assigned to the cube in the default Scenario Type.

### Workspaces
- They are **isolated development sandboxes** within the same application.
- Two security levels: **Access Group** (view, not modify) and **Maintenance Group** (view and modify).
- **Is Shareable Workspace = True** + list in **Shared Workspace Names** of the destination Workspace to share objects.
- The **Default Workspace is always shared** with any other Workspace.
- They allow **reusing object names** in separate Workspaces.
- **3 Security Roles:** AdministerApplicationWorkspaceAssemblies, ManageApplicationWorkspaces, WorkspaceAdminPage.
- Exporting a Workspace = a single XML file with all packaged objects.
- **Workspace Substitution Variables** use the prefix `WSSV`: `|WSSVNameSuffix|`.
- They do not require rebuilding existing solutions; they are backward compatible.
- **Dashboard Profiles and Cube View Profiles are global**, not Workspace-specific.
- **Cube Views in 3 locations:** Default/Default MU, Default/Specific MU, Non-Default/Specific MU.
- **Text 1-8 fields:** Store strings for use in Assemblies without modifying code.
- **Workspace Filter:** Allows users to refine the Workspaces shown.
- **Dashboard Types:** Use Default, Top Level, Top Level Without Parameter Prompts, Embedded, Embedded Dynamic, Embedded Dynamic Repeater, Custom Control.

### Report Books
- **3 types:** PDF (`.pdfBook`), Excel (`.xlBook`), Zip (`.zipBook`).
- **Excel Books** use Excel Export Items (do NOT support Report or File items).
- **Loops:** 3 types (Comma Separated List, Dashboard Parameter, Member Filter).
- **Loop Variables:** `|Loop1Variable|` up to `|Loop4Variable|` for nesting.
- **Change Parameters** are mandatory within Loops to update the variable.
- **If Statements** allow conditional logic (can be combined with Or/And).
- **Determine Parameters from Content = False** improves performance in large books.
- **Download Combined PDF File** is the recommended method for downloading books as PDF (does not require Combine All Items).
- Files from multiple sources can be added: URL, Application/System Workspace File, Application/System Database File, File Share File.
- **PDF Embedded Fonts:** Configure in Application Server Config to reduce size.
- The `!!` labels in headers show Display Items; `!` shows Value Items.

---

## Source Mapping

| Objective | Book / Chapter |
|-----------|----------------|
| 201.4.1 - Cube View Configurations | Design Reference Guide, Ch. 12 (Build Reports Through Cube Views, Member Filter Builder, Cube View Performance, Format a Cube View, Advanced Cube Views); Ch. 21 (Using OnePlace Cube Views - Toolbar, Shortcuts, Spreading, Allocation, Drill Down, Data Attachments, Cell Detail) |
| 201.4.2 - Workspace Scope | Workspaces & Assemblies, Ch. 2 (Understanding Workspaces - Definition, Benefits, Security, Sharing, Dashboard Types); Ch. 3 (Setting Up Your Workspace - Properties, Security Roles, Creating, Filter); Ch. 4 (Components - Maintenance Units, Dashboard Groups, Cube View Groups, Data Management Groups, Data Adapters, Parameters, Components) |
| 201.4.3 - Create a Report Book | Design Reference Guide, Ch. 12 (Presenting Data Using Report Books - Book Designer, Book Properties, Loops, Change Parameters, If Statements, Create a Report Book); Ch. 22 (Using OnePlace Dashboards) |


# Section 5: Tools (9% of the exam)

## Exam Objectives

- **201.5.1**: Demonstrate an understanding of the different types of Data Management Steps and their appropriate use cases
- **201.5.2**: Demonstrate an understanding of Excel Add-in functionality
- **201.5.3**: Demonstrate an understanding of Application Properties (global, Currency, etc.)
- **201.5.4**: Demonstrate how to create a scheduled task
- **201.5.5**: Demonstrate an understanding of the proper use of load/extract
- **201.5.6**: Demonstrate an understanding of the difference between application-level and system-level load/extract tools

---

## Key Concepts

### Objective 201.5.1: Data Management Steps and their use cases

Data Management is the module that allows copying data, clearing data, and executing processes for a Cube, Scenario, Entity, and Time. The structure is hierarchical and is accessed from **Application > Tools > Data Management**.

#### Data Management Hierarchy

- **Data Management Profile**: Organizes Groups for their presentation and access by users.
- **Data Management Group**: Top-level container that groups Sequences and Steps. A Group can be assigned to multiple Profiles.
- **Data Management Sequence**: Ordered series of one or more Steps that execute in the order in which they are organized.
- **Data Management Step**: Individual unit of work with a specific type.

![Data Management interface showing the hierarchy of Groups, Sequences, and Steps](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1673-6426.png)

> **Key exam point**: Steps are created first and then assigned to Sequences. Sequences are organized within Groups, and Groups are exposed through Profiles.

#### Searching in Data Management

You can quickly search for any Data Management object using the binoculars icon:
1. Select the object type: All Items, Sequence, or Step.
2. Enter search text and click Search.
3. Optionally use "View in Hierarchy" to see the results in hierarchical context.

#### Rename, Copy, and Paste

Existing Sequences and Steps can be renamed, copied, and pasted using the toolbar. **Data Management Groups and Profiles CANNOT be copied or pasted**. When pasting a Sequence or Step in the same hierarchy, the name is suffixed with "_copy".

---

#### Sequence Properties

##### General

| Property | Description |
|----------|-------------|
| **Name** | Sequence name |
| **Description** | Optional description |
| **Data Management Group** | Group to which it belongs |
| **Application Server** | Allows assigning a specific server if the Sequence has a long-running execution |

##### Notification (Email Notification)

![Notification configuration in a Sequence](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1675-6432.png)

| Property | Description |
|----------|-------------|
| **Enable Email Notification** | Default False. When True, notifications can be configured by user/group |
| **Notification Connection** | Connection to the email server (identified automatically) |
| **Notification User and Groups** | Users and groups that will receive the notification |
| **Notification Event Type** | Not Used, On Success, On Failure, On Success or Failure |

> **Important**: If no email server is available, this functionality is disabled. The Error Log captures information about users with invalid or deactivated email accounts.

![Example Data Management notification email](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1677-6437.png)

##### Queueing (Execution Queue Control)

| Property | Default Value | Description |
|----------|---------------|-------------|
| **Use Queueing** | True | Controls server CPU utilization |
| **Maximum % CPU Utilization** | 70% | Maximum CPU allowed before the task moves from Queued to Running. Do not set below 10 or the task may never start |
| **Maximum Queued Time Before Canceling** | 20 minutes | Maximum time in queue before automatic cancellation |

> **Exam note**: Batch processing queue override other Workflow queue settings. The batch processing queue does not apply to batch script, only to the Batch screen in the Workflow.

##### Parameter Substitutions 1-8

Allow passing variables to the Sequence. Each parameter has a Name and Value. These parameters are automatically displayed if configured when the Sequence is selected in Task Scheduler.

---

#### The 6 Built-in Data Management Step Types

##### 1. Calculate

Executes built-in calculations/consolidations. A Step is created with Step Type = Calculate.

**Available calculation types:**

| Calculation Type | Description |
|-----------------|-------------|
| **Calculate** | Executes calculation at Entity level within the Local member of the Consolidation Dimension, without translating or consolidating |
| **Translate** | Executes Calculate and then translates data within the Translated member for each applicable relationship |
| **Consolidate** | Executes Calculate, Translate, and completes calculations across the entire Consolidation Dimension |
| **Force Calculate/Translate/Consolidate** | Executes as if every cell needed to be recalculated. Ignores current calculation status |
| **With Logging** | Activates detailed logging visible in Task Activity. You can drill into the log to see the time and detail of each calculation |
| **Force with Logging** | Combines Force with detailed Logging |

**Calculate Step Parameters:**

| Parameter | Description |
|-----------|-------------|
| **Cube** | Cube where the consolidation/calculation will execute |
| **Entity Filter** | Entity(ies) or Entity hierarchy combinations |
| **Parent Filter** | For alternative hierarchies, specifies the Parent |
| **Consolidated Filter** | Consolidation member(s) to include |
| **Scenario Filter** | Scenario member(s) to include |
| **Time Filter** | Time member(s) to include |

##### 2. Clear Data

Clears data from a specific Cube/Scenario/Entity/Time.

| Property | Description |
|----------|-------------|
| **Use Detailed Logging** | If True, provides additional detail in Task Activity Log |
| **Cube** | Cube where data will be cleared |
| **Entity Filter** | Entity filter |
| **Scenario Filter** | Scenario filter |
| **Time Filter** | Time filter |
| **Clear Imported Data** | Clears the Import member of the Origin Dimension (True/False) |
| **Clear Forms Data** | Clears the Forms member of the Origin Dimension (True/False) |
| **Clear Adjustment Data and Delete Journals** | Clears Adjustment members and deletes Journals (True/False) |
| **Clear Data Cell Attachments** | Clears Data Cell Attachments |

##### 3. Copy Data

Copies data between Source and Destination combinations. Allows copying between different Cubes, Entities, Scenarios, Time periods, and Views.

| Property | Description |
|----------|-------------|
| **Source Cube / Destination Cube** | Source and destination Cubes |
| **Source Entity Filter / Destination Entity Filter** | Source and destination Entity filters |
| **Source Scenario / Destination Scenario** | Source and destination Scenarios |
| **Source Time Filter / Destination Time Filter** | Source and destination Time filters |
| **Source View / Destination View** | Allows copying YTD data to Periodic and vice versa. If left blank, copies to the same View Member |
| **Copy Imported Data** | True/False to copy O#Import |
| **Copy Forms Data** | True/False to copy O#Forms |
| **Copy Adjustment Data** | True/False to copy O#Adjustments |
| **Copy Data Cell Attachments** | Include Data Cell Attachments in the copy |

> **Key point**: If the Source View and Destination View fields are left blank, data is copied to the same View Member (YTD copies to YTD, Periodic copies to Periodic).

##### 4. Custom Calculate

The typical use is **speed of calculations during data entry** and **flexibility for What-if analysis**. Instead of running a full Calculate or Consolidation, Custom Calculate executes a calculation on a slice of data within one or more Data Units.

**Key characteristics:**

- **Does not create audit information** for each affected cell, making it faster than Copy Data.
- The user must be a member of the **Manage Data Group** of the Scenario, or the Step will fail.
- Supports **Durable Storage**: Data saved with Durable is not cleared during a normal Calculate (only with explicit ClearCalculated or Force).
- Accepts Parameters in the format: `Name1=Frankfurt, Name2=[Houston Heights]`
- Supports Custom Parameters with syntax `Name3=|!myParam!|` that prompt the user at runtime.

**Custom Calculate specific properties:**

| Property | Description |
|----------|-------------|
| **Data Units** | Defines Cube, Scenario (unique), plus Entity, Parent, Consolidation, and Time filters. The Business Rule executes once per Data Unit |
| **Point of View** | Individual dimension entries not in the Data Unit (View, Account, Flow, Origin, IC, UD1-UD8). Referenced from the Business Rule |
| **Business Rule / Function Name** | Name of the Finance-Type Business Rule and contained function |

> **Critical Durable Storage concept**: When a Calculation or Consolidation runs after a Custom Calculate, the data calculated by the Step is cleared unless it was saved with a Storage Type of Durable. ClearCalculatedData is the first step in the standard calculation sequence. Durable data is ignored even during Force Calculate/Consolidate, unless a ClearCalculated function is used within the Business Rule.

##### 5. Execute Business Rule

Executes a selected Extensibility Business Rule.

| Property | Description |
|----------|-------------|
| **Business Rule** | Selection from the application's available Business Rules |
| **Parameters** | Optional field to pass parameters or variables to the Business Rule |
| **Use Detailed Logging** | If True, generates detailed log in Task Activity |

##### 6. Export Data

Exports data from the Cube to a file in the File Share.

| Property | Description |
|----------|-------------|
| **File Share File Name** | Name of the exported file |
| **Include Cube/Entity/Parent/Cons/Scenario/Time in File Name** | Options to include each dimension in the name |
| **Overwrite Existing Files** | True to overwrite existing files |
| **Include Zeroes** | Include records with zero amount |
| **Include Member Descriptions** | Include member descriptions |
| **Include Cell Annotations** | Include cell annotations |
| **Include Input Data / Calculated Data** | Include input-type and/or calculated data |
| **Data Filters** | Account, Flow, Origin, IC, UD1-8 Filter (use #All for all stored base data) |
| **Specific Data Filters** | For granular control over specific intersections |

> **Location of exported files**: When running a Sequence from Data Management, files are found at System > Tools > File Explorer > File Share > Applications > [Application] > DataManagement > Export > [Username] > [Most recent folder].

![Button to execute a Sequence or Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1694-6475.png)

---

#### Additional Steps

##### Export File
Exports an Extensible Document or Report Book to the OneStream File Share.

| Property | Description |
|----------|-------------|
| **File Share Folder** | Destination folder in File Share |
| **File Name Suffix** | Suffix for the file name |
| **Overwrite Existing Files** | True to overwrite |
| **File Source Type** | URL, Application/System Dashboard File, Application/System Database File, File Share File |
| **Process Extensible Document** | If True, processes the document; if False, displays unprocessed (for testing) |
| **Parameters** | Comma-separated list of name-value pairs |

> **Note**: An Extensible Document is a Text, Word, PowerPoint, or Excel file that uses Parameters in its content. The file name must contain `.xfDoc` before the extension (example: `StatusReport.xfDoc.docx`).

##### Export Report
Exports Dashboards as PDFs.

| Property | Description |
|----------|-------------|
| **Report File Type** | **PDFs in Zip File** (individual in ZIP) or **Combined PDF File** (a single combined PDF) |
| **Object Type** | Dashboard or Dashboard Profile |
| **Object Name** | Name of the Dashboard or Dashboard Profile |
| **Object Parameters** | Optional parameters used when generating the report |

##### Reset Scenario

Similar to Clear Data but much more aggressive. Clears data (including parent Entity data), audit information, Workflow Status, and Calculation Status as if they never existed.

**Key differences from Clear Data:**

| Aspect | Clear Data | Reset Scenario |
|--------|-----------|----------------|
| Clears data | Yes | Yes (including parent Entity data) |
| Clears audit | No | Yes |
| Clears Workflow Status | No | Yes |
| Clears Calculation Status | No | Yes |
| Creates audit of the process | Yes | No (faster) |
| Clears Durable data | No | Yes |
| Requires Manage Data Group | No | Yes |

> **Best practices for Reset Scenario**: Only change Manage Data Group from Nobody to an exclusive User Group before executing, and then change it back to Nobody. **Always back up the database** before executing Reset Scenario.

| Property | Description |
|----------|-------------|
| **Scenario** | A single Scenario to reset |
| **Reset All Time Periods** | Default True. If False, Start Year/End Year are enabled |
| **Start Year / End Year** | First and last year to clear |
| **Start Time Period in First Year** | Optional time period in the first year |

##### Execute Scenario Hybrid Source Data Copy
Copies base Cube data from a Source Scenario using the Hybrid Source Data configuration. The copy occurs when a standard calculation is run on the destination Scenario.

---

### Objective 201.5.2: Excel Add-in Functionality

The Excel Add-In is an alternative way to enter, update, manage, query, and analyze application data using Excel spreadsheets. It can be launched from multiple locations: OnePlace, Application Tab, System Tab, Workflow forms, Cell/Data Unit attachments, Dashboards, Cube Views, and File Explorer.

#### Excel Add-In Connection

##### Adding the OneStream Ribbon

1. In Excel: File > Options > Add-ins
2. In Manage, select **COM Add-ins** and click Go
3. Check **OneStreamExcelAddIn** and click OK

![Selecting COM Add-ins in Excel Options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2000-7515.png)

![Activating the OneStreamExcelAddIn](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2000-7516.png)

##### Logging In to the Excel Add-In

1. From the OneStream ribbon in Excel, click **Log In**
2. In Server Address, use the ellipsis button to add or select the server URL
3. Select an available connection or add a new one (URL + description + Add)
4. Click **Connect** to authenticate
5. Enter username and password, click **Logon**
6. Select the application and click **Open Application**

![OneStream ribbon after login showing user and application](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2002-7525.png)

##### Client Updater

The Client Updater retrieves updated software from the server when the Excel Add-In version does not match the server version. The user needs write permissions to the installation folder. If the versions do not match, click Update and then OK. Excel must be closed before proceeding.

> **Note**: A backup folder is automatically created with the files from the previous version.

#### Three Main Components of the Excel Add-In

##### 1. Task Pane

![Excel Add-In Task Pane showing POV, Quick Views, and Documents](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2004-7533.png)

| Tab | Description |
|-----|-------------|
| **POV** | Shows Global POV, Workflow POV, and Cube POV. Linked to the application POV |
| **Quick Views** | Build and edit Quick Views |
| **Documents** | Public or user-specific documents. Access and manage with File Explorer |

##### 2. OneStream Ribbon

The ribbon contains the following categories:

![OneStream ribbon in Excel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2001-7519.png)

**a) OneStream**: Shows current user and application. Login/Logoff.

**b) Explore**: Quick access to Quick Views, Cube Views, and Table Views.

![Explore section of the ribbon](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2006-7549.png)

**c) Analysis**: Data Attachments, Cell Detail, Drill Down, Copy POV, Convert to XFGetCells. The "Copy POV from Data Cell" function captures the POV of the selected cell and allows pasting it as XFGetCell.

**d) Refresh**:

| Action | Refresh Sheet | Refresh Workbook |
|--------|---------------|------------------|
| **Function** | Refreshes only the active sheet | Refreshes all sheets in the workbook |
| **Dirty Cells** | Clears dirty cells only on the selected sheet | Clears dirty cells on all sheets |
| **Parameters (CV)** | Shows parameters of the current sheet | Shows all parameters of the workbook |

**e) Calculation**: Consolidate/Translate/Calculate (if permission is granted).

**f) Submit**:

| Action | Submit Sheet | Submit Workbook |
|--------|-------------|-----------------|
| **Function** | Identifies changes on the active sheet and saves them to DB | Identifies changes on all sheets and saves them to DB |
| **Data** | Only the selected sheet | All sheets |
| **Parameters** | No prompts | No prompts |

**g) Spreading**: Data distribution over selected cells.

![Spreading options in the ribbon](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2011-7564.png)

| Spreading Type | Description |
|----------------|-------------|
| **Fill** | Fills each selected cell with the value in Amount to Spread |
| **Clear Data** | Clears all data in the selected cells |
| **Factor** | Multiplies all cells by the specified rate |
| **Accumulate** | Takes the value of the first cell, multiplies it by the rate, and places the result in the second cell, and so on |
| **Even Distribution** | Distributes the Amount to Spread evenly across the selected cells |
| **445 Distribution** | 4-4-5 weight among the three selected cells |
| **454 Distribution** | 4-5-4 weight among the three selected cells |
| **544 Distribution** | 5-4-4 weight among the three selected cells |
| **Proportional Distribution** | Multiplies the value of each cell by the Amount to Spread and divides by the total sum. If all have zero value, behaves like Even Distribution |

**Spreading Properties:**

| Property | Description |
|----------|-------------|
| **Amount to Spread** | Value to distribute (default: last selected cell) |
| **Rate** | Only for Factor and Accumulate |
| **Retain Amount in Flagged Input Cells** | If True, does not apply spreading to flagged cells |
| **Include Flagged Readonly Cells in Totals** | If True, includes locked cells in the total (default True) |
| **Flag Selected Cells** | Flags cells to retain their value during spreading |
| **Clear Flags** | Clears cell flags |

![Example of Accumulate Spreading with rate 1.5](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2012-7568.png)

**h) File Operations**: File Explorer, Save to Server, Save Offline Copy.

**i) General**: Object Lookup, In-Sheet Actions, Parameters, Select Member, Preferences.

**j) Tasks**: Task Activity.

##### 3. Error Logs

Stored locally in the AppData folder. Automatically deleted after **60 days**. Useful for troubleshooting Add-In issues.

---

#### Excel Add-In Preferences

![Excel Add-In Preferences panel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2018-7595.png)

| Preference | Description |
|------------|-------------|
| **Enable Macros for OneStream Event Processing** | Enables Excel macros for OneStream API calls (default False) |
| **Invalidate Old Data When Workbook is Opened** | Forces data refresh on open (default False) |
| **Use Minimal Calculation for Refresh** | Only calculates formulas on the active sheet (default True). Excel Add-In only |
| **Use Multithreading for Cube View Workbook Refresh** | Concurrent processing for multiple sheets (default depends on version) |
| **Retain All Formatting when Saving Offline** | Default False. If True, retrieves all formatting character by character |
| **Preserve Hidden Rows and Columns** | Active only when Retain All Formatting is True |

> **Critical exam recommendation**: It is recommended to set Excel to **Manual Calculation Mode** for better performance when using it with OneStream.

---

#### Retrieve Functions (XF Functions)

| Function | Description | Example |
|----------|-------------|---------|
| **XFGetCell** | Retrieves data with 20 dimension parameters | `XFGetCell(NoDataAsZero, Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1-UD8)` |
| **XFGetCell5** | Same as XFGetCell but limits UD to 5 | Same structure with UD1-UD5 |
| **XFSetCell** | Saves data according to parameters | `XFSetCell(CellValue, StoreZeroAsNoData, Cube, ...)` |
| **XFGetFXRate** | Retrieves exchange rates | `XFGetFXRate(DisplayNoDataAsZero, FXRateType, Time, SourceCurrency, DestCurrency)` |
| **XFSetFXRate** | Saves exchange rates | `XFSetFXRate(Value, StoreZeroAsNoData, FXRateType, Time, Source, Dest)` |
| **XFGetMemberProperty** | Retrieves dimension member properties | `XFGetMemberProperty("Entity","Houston","Currency","","","")` |
| **XFGetRelationshipProperty** | Retrieves relationship properties | `XFGetRelationshipProperty("Entity","Houston","South Houston","PercentConsolidation","","2015M7")` |
| **XFGetHierarchyProperty** | Determines if a dimension has children (True/False) | `XFGetHierarchyProperty("entity","HoustonEntities","Houston Heights","HasChildren","Houston","Actual",FALSE)` |
| **XFGetCellUsingScript** | Uses Member Script instead of individual parameters | `XFGetCellUsingScript(TRUE,"GolfStream","E#Frankfurt:C#Local:S#Actual:T#2022M1:V#YTD:A#10100:...","","")` |
| **XFGetDashboardParameterValue** | Gets Dashboard parameter value | `XFGetDashboardParameterValue("myParamName","DefaultValue")` |

> **For the exam**: If a field within the function is not needed, enter a double quote `""` to ignore it. If a member name is misspelled, the cell returns an error indicating the first misspelled name.

##### Functions with Volatile Suffix

Functions like `XFGetCellVolatile`, `XFSetCellVolatile`, etc. are needed in cases where Excel requires a volatile function to refresh correctly (for example, some Excel charts that reference calculated cells).

##### Functions with UsingScript Suffix

Functions `XFGetCellUsingScript`, `XFSetCellUsingScript`, etc. allow using Member Script (e.g., `A#Sales:E#Texas`) instead of individual parameters. Multiple Member Script parameters are combined into a single script.

---

#### Quick Views

Ad hoc reports that allow pivoting, drill back, dataset creation, and workbook design.

##### Creating Quick Views

1. Enter members with the format **DimensionToken#MemberName** (e.g., `A#IncomeStatement`, `T#2018M1`)
2. Select the area and click **Create Quick View**

![Creating a Quick View by entering dimension tokens](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2046-7691.png)

##### Member Expansion Functions

| Function | Description |
|----------|-------------|
| `Root.Children` | Direct children of Root |
| `Root.List(60999,64000,63000)` | Specific list of members |
| `.Base` | Base-level members |
| `.Descendants` | All descendants |
| `2018.Base` | Base periods of year 2018 |

##### Quick View Options (Double-Click Behavior)

The double-click behavior is configured in Preferences > Quick View Double-Click Behavior. Options include: NextLevel (default), TreeDescendants, TreeDescendantsR (reverse order), AllBase, Children, etc.

##### Conversion to XFGetCells

Existing Quick Views can be converted to XFGetCell formulas using **Convert to XFGetCells** in the ribbon. This removes the Quick View definition and creates individual formulas. The conversion **cannot be reversed**.

---

#### Cube Views in Excel

##### Cube View Connection Settings

![Cube View Connection configuration in Excel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2026-8036.png)

| Property | Description |
|----------|-------------|
| **Name** | Auto-created when the Cube View is selected |
| **Refers To** | Starting cell or range on the sheet |
| **Cube View** | Select via Object Lookup |
| **Resize Initial Column Widths** | Auto-adjust width based on the Cube View configuration (default active) |
| **Insert/Delete Rows When Resizing** | For vertical stacking. Automatically adds/removes rows |
| **Insert/Delete Columns When Resizing** | For horizontal stacking. Automatically adds/removes columns |
| **Include Cube View Header** | Include the Cube View header |
| **Retain Formulas in Cube View Content** | Retain Excel formulas in the Cube View after Submit and Refresh |
| **Dynamically Evaluate Highlighted Cells** | Only available if Retain Formulas is active. Highlights cells with different values without needing Refresh |
| **Preserve Excel Format** | Preserve native Excel formatting in the Cube View |
| **Add Parameter Selectors to Sheet** | Generates parameter selectors for the Cube View. Must be selected during creation |

##### Retain Formulas in Cube View Content

Allows forming data grids in Excel using Cube Views that can be linked to other Excel models to send data to OneStream. Formulas in writable cells are retained when Submit and Refresh are performed. If the formula value differs from the value in the DB, the cell becomes a "dirty cell" (yellow).

![Retain Formulas and Dynamically Highlight configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2029-8037.png)

**Best practices for Retain Formulas:**
- Use "Well-Formed Grid" (Root.List or comma-separated list) in Cube Views
- Formulas with VLOOKUP referencing text work better when members are moved
- Deselecting Retain Formulas will remove all existing formulas in the grid
- Pivoting existing dimensions will break the formulas
- Changing the grid structure (reordering dimensions in rows/columns) will break the formulas

##### Named Regions

When bringing a Cube View into Excel, Named Regions are automatically created for: the complete Cube View, column headers, row headers, and data sections. These regions can be used with Styles for differentiated formatting.

![Example of Named Regions and Styles combined in a report](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch24-p2038-7649.png)

---

#### Table Views

- Defined through **Spreadsheet**-type Business Rules
- Designed to collect records from relational tables
- The administrator controls security and write-back via Business Rules (using BRAPI Authorization functions)
- A workbook can contain multiple Table Views (same sheet or different sheets)
- Performance limit: ~**8000 KB** of data per Table View
- **Only update existing records; do NOT perform inserts**
- Do not support paging; all rows must be returned at the same time

---

#### In-Sheet Actions

Buttons on the sheet to execute actions directly without leaving Excel.

| Property | Description |
|----------|-------------|
| **Label** | Button text |
| **Refers To** | Cell or range where the button appears |
| **Submit** | Sheet, Workbook, or Nothing |
| **Data Management Sequence** | Sequence to execute when clicked |
| **Parameters** | Explicit values or leave blank to resolve by sheet selections. Format: `myparam=value1, myparam2=[South Houston]` |
| **Do not wait for task to finish** | Allows continuing to work while the Sequence executes |
| **Refresh** | Sheet, Workbook, or Nothing |
| **Text Color / Background Color** | Custom colors (8-character hex code or picker) |

> **Important**: Parameters can only be used if they have been resolved in an existing Cube View or Table View on the sheet or workbook. In-Sheet Actions do not prompt for unresolved parameters.

---

#### Spreadsheet (Windows Application)

Similar functionality to the Excel Add-In but integrated into the OneStream Windows App, without needing Excel installed.

**Limitations compared to Excel:**
- Does not support Macros, Solver, Document Properties
- Does not allow Undo/Redo
- Does not allow referencing external workbooks
- Some 3D chart types not available (Stacked 3-D columns, 3-D Line, Stacked 3-D bars, Cylinder/Cone/Pyramid charts)
- Copy Quick Views only within the same workbook

---

### Objective 201.5.3: Application Properties

Accessible via **Application > Tools > Application Properties**. Default application properties are configured here.

#### General Properties

##### Global Point of View

| Property | Description |
|----------|-------------|
| **Global Scenario** | Default Scenario that users will see in Workflow. An initial value must be configured even if the Transformation configuration is not used |
| **Global Time** | Default time period that users will see in Workflow |

##### Company Information

| Property | Description |
|----------|-------------|
| **Company Name** | Appears in reports automatically generated from Cube Views |
| **Logo File** | PNG format, ~50 pixels high. Appears in Cube Views and Reports |

##### Workflow Channels

**UD Dimension Type for Workflow Channels**: The Origin Dimension controls data loading, but in some cases other User Defined dimensions require their own locking layer. Example: a company plans by Entity by Product. An Entity may have five products managed by different people. Each channel can be locked separately.

##### Formatting

**Number Format**: Global number format that can be overridden by Cube Views.

| Format | Result with 10000.001 |
|--------|----------------------|
| N0 | No decimals |
| N1-N6 | 1 to 6 decimals (N2 = two decimals, etc.) |
| #,###,0 | 10,000 and -10,000 |
| #,###,0;(#,###,0);0 | 10,000 and (10,000) |
| #,###,0.00 | 10,000.00 and -10,000.00 |
| #,###,0.00;(#,###,0.00);0.00 | 10,000.00 and (10,000.00) |
| #,###,0% | 10,000% and -10,000% |

> **Exam tip**: To vertically align positive and negative numbers in reports with parentheses, include trailing spaces in the positive format. Example: `#,###,0.00 ;(#,###,0.00);0.00` (space after .00 in the positive section).

> **The N in the formats indicates they are international.**

##### Currencies

All currencies used must be listed here to be used in Entity, currency translation, or rate entry. Includes pre-Euro and discontinued currencies for historical data. If a currency not listed is needed, contact OneStream Support.

##### Transformation

![Enforce Global POV icon in Import Workflow](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1628-6298.png)

| Property | Description |
|----------|-------------|
| **Enforce Global POV** | If True, forces Global Scenario and Time for all users. Displays a special icon during Import in Workflow |
| **Allow Loads Before Workflow View Year** | If True, allows loading data to periods before the current Workflow year |
| **Allow Loads After Workflow View Year** | If True, allows loading data to periods after the current Workflow year |

##### Certification

| Property | Description |
|----------|-------------|
| **Lock after Certify** | If True, automatically locks after certification in Workflow |

#### Dimension Properties

![Dimension Properties showing Time and User Defined Dimensions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1629-6303.png)

| Property | Description |
|----------|-------------|
| **Start Year / End Year** | Defines the application's year range |
| **UD1-8 Description** | Custom description for each User Defined dimension. Visible in: POV, Dimension Library, Cube View Member Filters, Drill Down headers, Excel Add-in/Spreadsheet, Journals |

> **Key point**: The description applies to the **dimension type**, not to each individual dimension.

#### Standard Reports Properties

These settings apply when auto-generating a report from a Cube View.

| Section | Properties |
|---------|------------|
| **Logo** | Height, Bottom Margin |
| **Title** | Top Margin, Font Family, Font Size, Bold, Italic, Text Color |
| **Header Labels** | Top/Bottom Margin, Font Family, Font Size, Bold, Italic, Text Color |
| **Header Bar** | Background Color, Line Color |
| **Footer** | Text, Font Family, Font Size, Show Line, Show Date, Show Page Numbers, Line Color, Text Color |

---

### Objective 201.5.4: Creating a Scheduled Task

Task Scheduler allows scheduling Data Management Sequences for automatic execution. Access: **Application > Tools > Task Scheduler**.

![Task Scheduler main screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1708-6518.png)

#### Available Views

##### Grid View

Tabular view with filterable and groupable columns. Multiple selections can be filtered and grouped by column by dragging to the header bar.

![Task Scheduler Grid View with scheduled tasks](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1709-6521.png)

| Field | Definition |
|-------|-----------|
| **User Name** | User who created the task |
| **Name** | Task name |
| **Description** | Description |
| **Sequence** | Data Management Sequence executed |
| **Schedule** | Implemented frequency |
| **Next Start Time** | Next scheduled execution |
| **Last Start Time** | Last execution |
| **Expire Date/Time** | When the task stops executing |
| **State** | Enabled or Disabled |
| **Count** | Number of times it has executed |
| **Invalidate Date/Time** | Suspension date/time (if enabled by admin) |
| **Validate Task** | Validate task to keep it active |

##### Calendar View

Calendar view with tasks color-coded by user. Supports views: Today, Work Week, Week, Month, Timeline, Agenda. Can be grouped by user name, date, or no group.

![Calendar View with color-coded tasks](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1716-6544.png)

> The view used is remembered for the next visit to the page.

#### Step-by-Step Procedure: Creating a New Task

![New task dialog - Task Tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1728-6587.png)

**Step 1 - Task Tab:**
1. Click **Create Scheduled Task**
2. **Name**: Task name
3. **Description**: Description
4. **Start Date/Time**: Start date and time (user's local time). Saved in UTC. If not specified, defaults to current date/time
5. **Sequence**: Select the Data Management Sequence (filter by name)
6. **Parameters**: Automatically displayed if configured in the Sequence

![Sequence selection with parameters](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1730-6595.png)

**Step 2 - Schedule Tab:**

![Schedule dialog showing frequency options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1731-6598.png)

| Frequency | Detail |
|-----------|--------|
| **One Time** | Single execution based on Start Date/Time |
| **Minutes** | Recurring every 5-180 minutes. Can be limited with Time From/To |
| **Daily** | Configure recurrence frequency |
| **Weekly** | Select days and frequency. Example: Recur Every 2 weeks, On: Sunday, Monday, Friday |
| **Monthly** | Select days and frequency |

Additional properties:
- **Expire Date/Time**: When the task stops executing
- **Enabled**: Enable/disable
- **Administration Enabled (Enabled by Manager)**: Only the Administrator can change

> **Note on Minutes**: Calendar entries are created even if they fall outside the selected time range. Example: a task every 30 minutes between 2:00pm and 5:00pm will show entries all day every 30 minutes in the Calendar View.

**Step 3 - Advanced Tab:**

![Advanced Tab showing number of retries](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1732-6601.png)

Number of retries on failure: **maximum 3**.

**Step 4**: Click OK. The new task appears in Grid View and Calendar View.

#### Weekly Execution Logic Example

Start Date/Time: 5/7/2024 11:55 AM (Wednesday), Recur Every: 2 weeks, On: Sunday, Monday, Friday

| Date | Result |
|------|--------|
| Friday 5/9 | Run |
| Sunday 5/11 | Run |
| Monday 5/12 | Run |
| Friday 5/16 | Skip (second week) |
| Sunday 5/18 | Skip |
| Friday 5/23 | Run (third week = recurrence) |
| Sunday 5/25 | Run |
| Monday 5/26 | Run |

#### Task Monitoring

- **Task Activity**: The Task Type is shown as "**Data Management Scheduled Task**". The description is the task name separated by a hyphen followed by the sequence.

![Task Activity showing executed scheduled task](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1719-6557.png)

- **Logon Activity**: In System > Logon Activity, the Client Module appears as "**Scheduler**".

![Logon Activity showing login via Scheduler](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1720-6560.png)

#### Security Roles for Task Scheduler

![Application Security roles for Task Scheduler](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1722-6567.png)

| Role | Type | Permissions |
|------|------|------------|
| **TaskSchedulerPage** | Application UI Role | View the Task Scheduler page. View all tasks |
| **TaskScheduler** | Application Security Role | Create tasks, edit own, validate tasks, view all. **Cannot** load/extract or change task name |
| **ManageTaskScheduler** | Application Security Role | Create, view, edit, and delete all tasks. **Can** load/extract. Cannot change task name |

#### Load/Extract of Scheduled Tasks

If the ManageTaskScheduler role is held, Task Scheduler files can be loaded and extracted:
- **Load**: Application > Tools > Load/Extract > select file > click Load
- **Extract**: Application > Tools > Load/Extract > click Extract > File Type "Task Scheduler" > select task > Extract

---

### Objective 201.5.5: Proper Use of Load/Extract

Load/Extract allows importing and exporting sections of the application or system using XML format.

#### General Concepts

- **Load**: Import an XML file with artifact definitions
- **Extract**: Export artifacts to an XML file
- **Extract and Edit**: Extract and edit the XML immediately
- Only **Administrators** have access by default
- Used to move configurations between environments (Development > Test > Production)

![Load/Extract interface with Extract, Load, and Extract and Edit options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1764-8030.png)

#### Application Load/Extract (Application > Tools > Load/Extract)

| Artifact | Description | Location in the app |
|----------|-------------|---------------------|
| **Application Zip File** | Everything except Data and FX Rates. Creates a complete copy | Entire application |
| **Application Security Roles** | Application security roles | Application > Tools > Security Roles |
| **Application Properties** | Application properties | Application > Tools > Application Properties |
| **Workflow Channels** | Workflow Channels | Application > Workflow > Workflow Channels |
| **Metadata** | Business Rules, Time Dimension Profiles, Dimensions, Cubes. Supports change search by username and timestamp | Application > Cube > Dimension Library |
| **Cube Views** | Groups and Profiles | Application > Presentation > Cube Views |
| **Data Management** | Groups and Profiles | Application > Tools > Data Management |
| **Application Workspaces** | Maintenance Units and Profiles (Groups, Components, Adapters, Parameters) | Application > Presentation > Workspaces |
| **Confirmation Rules** | Groups and Profiles | Application > Workflow > Confirmation Rules |
| **Certification Questions** | Groups and Profiles | Application > Workflow > Certification Questions |
| **Transformation Rules** | Business Rules, Groups, and Profiles | Application > Data Collection > Transformation Rules |
| **Data Sources** | Business Rules and Data Sources | Application > Data Collection > Data Sources |
| **Form Templates** | Groups and Profiles | Application > Data Collection > Form Templates |
| **Journal Templates** | Groups and Profiles | Application > Data Collection > Journal Templates |
| **Workflow Profiles** | Profiles and Templates. When loading, old properties not in the XML are cleaned | Application > Workflow > Workflow Profiles |
| **Extensibility Rules** | Only Extensibility Rules (other types are exported with their associated object) | Application > Tools > Business Rules |
| **FX Rates** | By FX Rate Type and Time Period | Application > Cube > FX Rates |
| **FX Rate CSV** | CSV export: FxRateType, Time, SourceCurrency, DestCurrency, Amount, HasData | Application > Cube > FX Rates |
| **Task Scheduler** | Scheduled tasks | Application > Tools > Task Scheduler |

> **Key point about Workflow Profiles**: When loading via XML, the Load process cleans old property configurations that are not specified in the loaded XML. This ensures that edits made to the Profile before extraction are respected when reloading.

---

### Objective 201.5.6: Difference Between Application and System Load/Extract

#### System Load/Extract (System > Tools > Load/Extract)

![System Load/Extract interface](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1909-7196.png)

| Artifact | Description |
|----------|-------------|
| **Security** | System Roles, Users, Security Groups, Exclusion Groups. **Extract Unique IDs** option (uncheck when loading to another environment to avoid errors with duplicate IDs). Allows selecting specific Items To Extract or sections on/off |
| **System Dashboards** | Maintenance Units and Profiles (Groups, Components, Adapters, Parameters). Items To Extract with granular selection |
| **Error Log** | With Start/End Time range. "Extract All Items" checkbox for everything |
| **Task Activity** | With Start/End Time range. "Extract All Items" checkbox |
| **Logon Activity** | With Start/End Time range. "Extract All Items" checkbox |

#### Complete Comparison Table

| Aspect | Application Load/Extract | System Load/Extract |
|--------|--------------------------|---------------------|
| **Location** | Application > Tools > Load/Extract | System > Tools > Load/Extract |
| **Main artifacts** | Metadata, Rules, Cube Views, Data Mgmt, Workflows, Dashboards, FX Rates, Task Scheduler | Security (Users, Groups, Roles), System Dashboards, Logs (Error, Task Activity, Logon) |
| **Database** | Application Database | Framework (System) Database |
| **Scope** | A specific application | The entire environment/system (shared between applications) |
| **Security Role (UI)** | ApplicationLoadExtractPage | SystemLoadExtractPage |
| **Additional Security Roles** | Administrator or equivalent | ManageSystemSecurityUsers, ManageSystemSecurityGroups, ManageSystemSecurityRoles |
| **Format** | XML (or ZIP for Application Zip File) | XML |
| **Extract Unique IDs option** | Not applicable | Yes, uncheck when moving to another environment |

#### Best Practices for Load/Extract

1. **Always deploy and test changes first in a development environment**
2. Consider making a copy of the production DB to test changes
3. Extract changes from the development environment and evaluate in a test environment before production
4. **Avoid deploying during periods of high load or activity**
5. For Security XML loads by non-Administrator users: security must pre-exist in the destination environment
6. When moving Security between environments, **uncheck Extract Unique IDs** to avoid conflicts
7. For Workflow Profiles, keep in mind that Load cleans properties not included in the XML

---

## Additional System Tools (Exam Context)

### File Explorer and File Share

**File Explorer** (System > Tools > File Explorer) allows managing documents stored in the Application Database, System Database, and File Share.

**File Share** is a self-service directory that supports file storage external to OneStream databases.

#### File Share Folder Structure

| Folder | Purpose |
|--------|---------|
| **Batch / Harvest** | Automation of Connector Data Loads. Harvest is automatically cleaned when loading |
| **Content** | Secure storage for files >300 MB (up to 2 GB) |
| **Data Management** | Default folder for Data Management exports |
| **Groups** | For dashboard components with File Source Type of File Share, secured by group |
| **Incoming** | Source files for importing to Stage |
| **Internal** | Content of Application/System Database files |
| **Outgoing** | Available for custom processes |

#### File Share Permissions

- **Administrator** and **ManageFileShare** have full rights
- Non-Administrators can receive rights for modify, full access, or limited access to Content folders
- To access File Share export/import: **SystemPane** and **FileExplorerPage** roles are needed

### Environment Monitoring

Accessible via **System > Tools > Environment** (Windows App only). Allows monitoring Web Servers, Mobile Web Servers, Application Server Sets, and Database Servers.

### Profiler

A developer tool that captures every event processed in a user session (Business Rules, Formulas, Workspace Assemblies). Linked to user activity, not application-level.

| Property | Description |
|----------|-------------|
| **Number of Minutes to Run** | Default 20, max 60 |
| **Number of Hours to Retain** | Default 24, max 168 |
| **Include Top Level Methods** | Captures high-level entry points |
| **Include Adapters** | Includes Data Adapter calls |
| **Include Business Rules** | Includes Business Rules in the output |

Required roles: **ManageProfiler** (run sessions) and **ProfilerPage** (view the page).

---

## Critical Points to Memorize

### Data Management:
- The 6 Step types: **Calculate, Clear Data, Copy Data, Custom Calculate, Execute Business Rule, Export Data**
- Additional Steps: **Export File, Export Report, Reset Scenario, Execute Scenario Hybrid Source Data Copy**
- Structure: **Profile > Group > Sequence > Step**. Steps are assigned to Sequences. Sequences are organized in Groups. Groups are exposed in Profiles
- **Custom Calculate** does not create audit, is faster; requires Manage Data Group; supports Durable Storage
- **Reset Scenario** clears data, audit, Workflow Status, and Calculation Status. More aggressive than Clear Data. Even clears Durable data
- Email **Notification** is configured on the **Sequence** (not on the Step). Options: Not Used, On Success, On Failure, On Success or Failure
- **Queueing** defaults to 70% CPU and 20 minutes wait. Do not set below 10% CPU
- **Durable Storage**: Data that persists during normal Calculate; only removed with ClearCalculated, Force with ClearCalculated, or Reset Scenario
- **Groups and Profiles CANNOT be copied/pasted**; only Sequences and Steps

### Excel Add-in:
- **Three components**: Task Pane, OneStream Ribbon, Error Logs
- **Refresh Sheet** vs **Refresh Workbook**: Sheet only the active sheet; Workbook the entire file
- **Submit Sheet** vs **Submit Workbook**: Same logic as Refresh
- Key functions: **XFGetCell** (20 params), **XFSetCell**, **XFGetCellUsingScript** (Member Script), **XFGetMemberProperty**
- **Retain Formulas in Cube View Content**: Allows maintaining Excel formulas in Cube Views. Dirty cells in yellow
- **Dynamically Highlight Evaluated Cells**: Updates cells without needing Refresh. Only available if Retain Formulas is active
- **Manual Calculation Mode** is recommended in Excel for better performance
- The Add-In is registered as a **COM Add-in**
- **In-Sheet Actions**: Buttons for Submit, Refresh, and Data Management. Parameters require being resolved in an existing CV/TV
- **Table Views**: Only updates, no inserts. Limit ~8000 KB. Defined by Spreadsheet-type Business Rules
- **Error Logs**: Automatically deleted after 60 days
- **Save Offline Copy**: Saves a copy without functions for users without the Add-In
# Section 6: Security (10% of the exam)

## Exam Objectives

- **201.6.1**: Demonstrate an understanding of data cell access
- **201.6.2**: Describe the characteristics of Application Security Roles, Application User Interface Roles, and System Security Roles
- **201.6.3**: Demonstrate the ability to troubleshoot security access issues

---

## Key Concepts

### Objective 201.6.1: Data Cell Access

#### Where Security Resides in OneStream

OneStream security is stored in its own **SQL framework database**. This database is shared by **all applications** within the same OneStream environment. Ideally, clients will have separate environments for production, QA, and development, each with its own framework database. As part of the OneStream SAAS offering, clients receive **two environments** with separate framework databases.

If a company has a single environment with multiple development applications, each application can be secured to a restricted group of users through the **OpenApplication** role.

#### Four-Layer Security Approach

Application security in OneStream uses a four-layer approach:

1. **Workflow Security**: Controls who can execute Workflow processes (import, certify, approve journals).
2. **Entity Security**: Controls read/write access to entity data.
3. **Account Security**: Controls who can view specific dimension members.
4. **Security Roles**: Determines what data can be accessed or edited.

Security can be implemented on accounts or dimensions, allowing control over who can review specific dimension members. Security is determined through users and groups, where users receive specific roles to determine what data can be accessed or edited.

#### Security Verification Flow for Data Access

When a user attempts to access data, OneStream verifies in this order:

![Security verification flow - Figure 9.1 from Foundation Handbook](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p279-2727.png)

1. **OpenApplication**: The user must be in a group assigned to this role to open the application.
2. **Cube Access**: The user must be in the Access Group or Maintenance Group of the Cube.
3. **Scenario Access**: Verifies Read Data Group and Read/Write Data Group of the Scenario.
4. **Entity Access**: Verifies Read Data Group and Read/Write Data Group of the Entity.
5. **Data Cell Access Security** (Slice Security): Verifies additional filters at the Cube level.
6. **Workflow Verification**: If data is being imported, verifies that the entity belongs to an active Workflow Profile that is not locked or certified.

If **any** of these steps results in "no access," the process stops immediately.

**Critical point**: There are several ways to ensure data security. Through dimensions, different security groups are available, and an administrator decides which users belong to each group.

#### Entity Security

Entity Security controls general read/write access to entity data and controls whether Cube security should be used.

- Each Entity has two main security groups:
  - **Read Data Group**: Read-only access to entity data.
  - **Read/Write Data Group**: Read and write access.
- OneStream also supports a **second Read Data Group** and **second Read/Write Data Group**.
- **Recommended naming convention**: `XXXX_View` (read) and `XXXX_Mod` (write).

**Entity Security Design Procedure:**

1. **Design the Read/Write Data Group first** because it is needed for data loading in Workflows.
2. The Workflow Execution Security Group must be assigned to the Read/Write Security Group of all entities in the Workflow to obtain load access.
3. When configuring View Security Groups for entities, first consider how users need to view their data (by segment or region).
4. Determine whether it makes more sense to have one Entity View Group per entity, or create one Entity View Group per segment and apply it to multiple entities.
5. **All Entity View Groups below the parent** must be assigned to the parent-level Entity View Group to have access to data at the parent level.
6. Minimize the number of View Entity Security Groups where possible.

**Practical example from the Foundation Handbook - Data Loader:**

In the GolfStream example, a data loader needs access to import, validate, and load data for two entities (HoustonHeights and SouthHouston) that are part of the same workflow:

![Group configuration for data loader - Figure 9.7](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p287-2790.png)

![User-group-workflow relationship - Figure 9.8](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p288-2802.png)

The user is assigned to a single group (`WF_HoustonWorkflow`). Access to modify the entities and access the workflow is nested by assigning them to the `WF_HoustonWorkflow` group.

**Example of error when Entity Access is missing:**

If a user has the workflow group but does not have the entity modify group (`M_HoustonHeights`), they will be able to access the workflow and import the file, but will get an error when trying to validate against that entity:

![Error due to missing entity access - Figure 9.13](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p290-2820.png)

#### Scenario Security

- The Scenario has **Read Data Group** and **Read/Write Data Group**.
- Access to the scenario is verified after Cube access and before Entity.
- The scenario dimension also uses both scenario security groups and adds Display Member Group and Use Cube Data Access.

#### Data Cell Access Security ("Slice" Security)

This is an additional security layer at the Cube level that allows granular access control to specific data intersections. A "cube slice" filters a data entry form to the correct set of members, such as choosing the cost center where a user can enter data.

**Fundamental principles:**

- Data Cell Access **only works** if the user has already been granted access to an entity and its data.
- Access is first granted, then **decreased** to a subset, and then optionally **increased** to specific intersections.
- **Cannot increase access** that was not initially granted through Users and Groups.
- Security is applied in **order**: the order of the rules is critical.
- An administrator can lock more than one dimension using cube data access.

**Configuration:**

- Location: Application > Cube > Cubes > Data Access > Data Cell Conditional Input.
- Each rule has:
  - **Member Filter**: Dimension intersection to restrict.
  - **In Filter**: Behavior (In Group In Filter, etc.).
  - **Access Level**: Read Only, No Access, etc.
  - **Behavior**: Decrease Access, Increase Access, Increase Access And Stop, etc.

**"Increase Access And Stop"**: If the cell matches the filter, access is increased and all subsequent rules are ignored. This is critical for the evaluation order.

![Data Cell Conditional Input configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p501-2674.png)

![Data Cell Access behaviors](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p501-2676.png)

**Practical slice security example (Foundation Handbook):**

A data loader has access to HoustonHeights and SouthHouston, but needs to see only the Balance Sheet data from Frankfurt (not P&L). Data cell access is used to restrict:

![Data Cell Access with account restriction - Figure 9.14](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p291-2828.png)

![Before and after applying slice security - Figure 9.15](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p292-2837.png)

**Slice security order:**

![Slice security rule order - Figure 9.32](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p304-2950.png)

In Figure 9.32, the `Slice_2` group applies to the second and third lines; however, users in this group **will never reach the third line** because the second has the `Increase Access and Stop` behavior, which tells the system not to look for additional slice rules.

**Important about ViewAllData:**

- If a user has the **ViewAllData** role, Data Cell Access Security **does not restrict them**. This role grants read access to the entire Cube without exception.
- To restrict data for a user with ViewAllData, you would have to build all entity access individually and then decrease it.
- The only way to restrict what a ViewAllData user sees in a Member Filter is by setting display access to Nobody. However, this does not prevent data access via XFGetCell or freeform entry.

#### Data Cell Conditional Input

- This is **not security** per se; it is a functional restriction.
- It **impacts ALL** users, not just specific groups.
- Used to make cells read-only at specific intersections.
- Should not be confused with slice security, as it is not designed with specific users in mind.

**Example - Restricting data entry by Origin:**

There may be cases where data should be loaded through the Import origin but not through the Forms origin. For example, users load trial balance data via Import, but others submit statistical data via Forms. Data cell conditional input ensures that statistical data does not overwrite Trial Balance data.

**Step-by-step procedure:**

1. Go to Application > Cube > Cubes > Data Access.
2. Go to Data Cell Conditional Input.
3. Click to create a new line.
4. Double-click the cell to edit the member filter. Add the dimension intersection to restrict.
5. In the In Filter field, choose a behavior and the Read Only access level.

![Configuration of restriction by Origin](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p501-2674.png)

#### Bypassing Data Cell Conditional Input by Scenario

When there are many data cell conditional input rules that were not defined by scenario at the time of creation, but a scenario becomes a factor for historical data:

**Procedure:**

1. Create a new data cell conditional input rule for the entire scenario.
2. Set the behavior to **Increase Access And Stop**.
3. Set the Access Level to Read Only.
4. **Position this new rule at the beginning** (first position) of the Data Cell Conditional Input.

![Rule to bypass by scenario](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p503-2681.png)

![Rule position at the beginning](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p503-2682.png)

In this case, the Preserve Scenario has access to everything, and subsequent Data Cell Conditional access rules are ignored for Preserve.

#### Relationship Security

Controls access to relationship members in the Consolidation Dimension.

![Relationship Security configuration in Cube Properties](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p494-2654.png)

Configured in Cube Properties: **Use Parent Security for Relationship Consolidation Dimension Members**.

| Value | Behavior |
|-------|----------|
| **False** (default) | The user's rights on the entity control access to all consolidation members. This is the default security model. |
| **True** | Rights to relationship members are determined by the rights to the entity's immediate parent. |

**Procedure to change Relationship Security:**

1. From the Application tab, under Cube, click Cubes.
2. Select a Cube.
3. In the Cube Properties tab, in Use Parent Security for Relationship Consolidation Dimension Members, select True.
4. Click Save.
5. Click Refresh to refresh the application.
6. In Cube Views, go to the Designer tab and click Open Data Explorer.

![Changing Relationship Security step by step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p495-2657.png)

![Result after changing Relationship Security](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p497-2664.png)

**Observed effects:**
- With the new security activated, if the user has rights to USClubs, when viewing Houston, all consolidation members are available.
- If the user **does not have rights to Texas**, based on NoAccess to Local and Translated, Houston shows the relationship members as NoAccess.
- USClubs shows as NoAccess because the user does not have rights to the parent NAClubs.

**Important note**: This is strictly a parent-child relationship. **ViewAllData** and Administrators **are not affected** by this configuration.

![Consolidation members - Figure 9.23](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p299-2902.png)

#### Display Member Group

- Controls the **visibility** of dimension members in lists, **not data access**.
- Available in Account, Flow, and User Defined Dimensions.
- **Should not be confused with data security**: A user may not see the member in a list but could access the data via XFGetCell or freeform entry.
- Setting Display Access to "Nobody" hides the member from Member Filters, but does not restrict data.
- Both display access and workflow channels are for **functional** purposes and should not be used as a level of security in the overall design.

---

### Objective 201.6.2: Application Security Roles, Application User Interface Roles, and System Security Roles

#### Application Security Roles

Control who can **manage or execute actions** on objects and data within a specific application. These roles address everything from who can access the application, to modifying data, decertifying and unlocking workflows, to accessing and modifying artifacts.

![Application Security Roles with defaults - Figure 9.2](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p280-2734.png)

**Complete Application Security Roles table:**

| Role | Description | Default |
|------|-------------|---------|
| **AdministerApplication** | Administer the application and load ZIP files | Administrator |
| **AdministratorDatabase** | Bulk deletion of metadata and data via Database page. Administrators do NOT have automatic access | **Nobody** |
| **OpenApplication** | View and open the application | **Everyone** |
| **ModifyData** | Modify data (without this, the user is read-only). Can be left at Everyone if the rest of the security is correctly configured | Everyone |
| **ViewAllData** | View all data. Cannot be restricted with Data Cell Access | Administrator |
| **CreateAuditAttachments** | Create audit attachments | Administrator |
| **CreateFootnoteAttachments** | Create footnotes | Administrator |
| **CertifyAndLockDescendants** | Certify and lock descendants from Workflow | Administrator |
| **UnlockAndUncertifyAncestors** | Unlock and decertify ancestors | Administrator |
| **PreserveImportData** | Preserve imported data when changes are made with Workflow locked | Administrator |
| **RestoreImportData** | Restore imported data to the original state from Preserve | Administrator |
| **UnlockWorkflowUnit** | Unlock a Workflow unit (also requires Workflow Execution access) | Administrator |
| **ViewSourceDataAudit** | View the Source Data Audit Report in Import Workflow | Administrator |
| **EncryptBusinessRules** | Encrypt and decrypt Business Rules | Administrator |
| **ManageApplicationProperties** | Update Application Properties | Administrator |
| **ManageMetadata** | Edit metadata in Dimension Library | Administrator |
| **ManageFXRates** | Update FX Rates | Administrator |
| **ManageData** | Manage data (export, clear via Data Management). Administrator function | Administrator |
| **ManageCubeViews** | Create and manage Cube Views, Groups, and Profiles | Administrator |
| **ManageDataSources** | Create Data Sources | Administrator |
| **ManageTransformationRules** | Create and manage Transformation Rules | Administrator |
| **ManageConfirmationRules** | Create and manage Confirmation Rules | Administrator |
| **ManageCertificationQuestions** | Create and manage Certification Questions | Administrator |
| **ManageWorkflowChannels** | Create Workflow Channels | Administrator |
| **ManageWorkflowProfiles** | Create Workflow Profiles | Administrator |
| **ManageJournalTemplates** | Create and manage Journal Templates | Administrator |
| **ManageFormTemplates** | Create and manage Form Templates | Administrator |
| **ManageApplicationDashboards** | Create and manage Application Dashboards | Administrator |
| **ManageApplicationDatabaseFiles** | Full read/write access to files in the application DB | Administrator |
| **ManageTaskScheduler** | Full management of Task Scheduler including load/extract | Administrator |
| **TaskScheduler** | Create and edit own tasks. No load/extract | Administrator |
| **AnalyticsApi** | Access to OneStream connectors (e.g., Power BI Connector) | Administrator |

**Critical note**: When a new application is created, all role defaults are **Administrator** except **OpenApplication** (Everyone) and **AdministratorDatabase** (Nobody). These two are exceptions for less common activities where the ability to administer should be deliberate.

**About ModifyData and Everyone**: It can be set to Everyone because the combination of Cube, Scenario, Entity, and Workflow security already effectively controls who can modify data. If a user does not have write access to an entity, the ModifyData role will not grant them that access.

#### Application User Interface Roles

Control **access to pages** within the application. They grant visibility but **not management capability**. They are in the lower/right section of Application Security Roles.

| Role | Page it controls |
|------|-----------------|
| **ApplicationLoadExtractPage** | Application > Tools > Load/Extract |
| **ApplicationPropertiesPage** | Application > Tools > Application Properties |
| **ApplicationSecurityRolesPage** | Application > Tools > Security Roles |
| **BookAdminPage** | Application > Presentation > Book Designer |
| **BusinessRulesPage** | Application > Tools > Business Rules |
| **CertificationQuestionsPage** | Application > Workflow > Certification Questions |
| **ConfirmationRulesPage** | Application > Workflow > Confirmation Rules |
| **CubeAdminPage** | Application > Cube > Cube Admin |
| **CubeViewsPage** | Application > Presentation > Cube Views |
| **DashboardAdminPage** | Application > Presentation > Dashboard Admin |
| **DataManagementAdminPage** | Application > Tools > Data Management Admin |
| **DataSourcesPage** | Application > Data Collection > Data Sources |
| **DimensionLibraryPage** | Application > Cube > Dimension Library |
| **FXRatesPage** | Application > Cube > FX Rates |
| **FormTemplatesPage** | Application > Data Collection > Form Templates |
| **JournalTemplatesPage** | Application > Data Collection > Journal Templates |
| **TransformationRulesPage** | Application > Data Collection > Transformation Rules |
| **WorkflowChannelsPage** | Application > Workflow > Workflow Channels |
| **WorkflowProfilesPage** | Application > Workflow > Workflow Profiles |
| **TaskSchedulerPage** | Application > Tools > Task Scheduler |
| **SpreadsheetPage** | Application > Spreadsheet |

**Key point**: A user may have access to the page (UI Role) but NOT have permission to manage the content (Security Role). Both roles work together. Recognizing how users can be given visibility to objects in the application while restricting management access is key to any line of questioning about who needs to see elements versus who needs to update them.

**Practical example - Application Admin with limited access:**

Imagine a group `R_ApplicationAdmin` that needs to manage metadata and FX rates but is not a full administrator:
- Assign `ManageMetadata`, `ManageFXRates`, `LockFXRates`, `UnlockFXRates` to the `R_ApplicationAdmin` group.
- Also assign `DimensionLibraryPage` and `FXRatesPage` to the same group (or to a nested group).
- Keep `FXRatesPage` open to a separate group of users with read-only access.

![Example of applied roles - Figure 9.4](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p283-2754.png)

![Roles and access relationship - Figure 9.5](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p284-2763.png)

#### System Security Roles

Apply to the **entire system**, not just one application. The Administrator has all these roles by default and they cannot be revoked. System security roles are typically reserved for administrators.

![System Security Roles - Figure 9.3](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p281-2741.png)

**Complete System Security Roles table:**

| Role | Description | Default |
|------|-------------|---------|
| **ManageSystemDashboards** | Manage all System Dashboards. Also requires SystemPane Role | Administrator |
| **ManageSystemDatabaseFiles** | Full read and write on Framework database files. Links with FileExplorerPage | Administrator |
| **ManageFileShare** | Edit folders and files in File Share via File Explorer | Administrator |
| **ManageSystemConfiguration** | Change server configurations | **Nobody** (NOT automatic for Administrator) |
| **ManageProfiler** | Run Profiler sessions and view events | Administrator |
| **EncryptSystemBusinessRules** | Encrypt/decrypt System tab Business Rules | Administrator |
| **ViewAllLogonActivity** | View login activity for all users | Administrator |
| **ViewAllErrorLog** | View Error Log for all users | Administrator |
| **ViewAllTaskActivity** | View Task Activity for all users | Administrator |
| **ManageSystemSecurityUsers** | Create, modify, and delete users | Administrator |
| **ManageSystemSecurityGroups** | Define, modify, and manage groups and exclusion groups | Administrator |
| **ManageSystemSecurityRoles** | Manage assignment of System Security Roles | Administrator |

#### File Share Security Roles

| Role | Description |
|------|-------------|
| **ManageFileShareContents** | Exposes the Contents folder in File Explorer/FileShare. Full access: create, upload, download, delete folders |
| **AccessFileShareContents** | Exposes the Contents folder. Only allows viewing and downloading |
| **RetrieveFileShareContents** | The Contents folder is NOT exposed to the user in File Explorer. All files are accessible through the OneStream application (Dashboards, Business Rules) |

#### System User Interface Roles

Control access to System tab pages.

| Role | Description | Default |
|------|-------------|---------|
| **SystemAdministrationLogon** | Access to the System Administration application. Becomes available in the application list during logon | Administration |
| **SystemPane** | Access to the System Tab (bottom left) | Administrator |
| **ApplicationAdminPage** | Access to the Application Tab | Administrator |
| **SecurityAdminPage** | View (not modify) security in System > Administration. Can only change own password | Administrator |
| **SystemDashboardAdminPage** | Access to System > Administration > Dashboards. Links with ManageSystemDashboards | Administrator |
| **ApplicationServersPage** | Access to System > Tools > Application Servers | Administrator |
| **DatabasePage** | Access to System > Tools > Database | Administrator |
| **FileExplorerPage** | Access to System > Tools > File Explorer. Links with ManageSystemDatabaseFiles and ManageFileShare | Administrator |
| **SystemLoadExtractPage** | Access to System > Tools > Load/Extract (view, but not import/extract without additional roles) | Administrator |
| **ErrorLogPage** | Access to System > Logging > Error Log | Administrator |
| **LogonActivityPage** | Access to System > Logging > Logon Activity. Can view all users but cannot logoff | Administrator |
| **TaskActivityPage** | Access to System > Logging > Task Activity | Administrator |
| **TimeDimensionPage** | Access to System > Tools > Time Dimension | Administrator |
| **SystemConfigurationPage** | Access to System Configuration. Read-only. Not automatically granted with ManageSystemConfiguration | Administrator Group |
| **ProfilerPage** | View the Profiler page (without running sessions or viewing events) | Administrator |

#### Access Hierarchy (from highest to lowest power):

1. **System Security Role** (e.g., ManageSystemDashboards): Highest privilege. The user does not need to be in any Maintenance Group or Access Group to view, edit, or delete all objects of that type.
2. **Maintenance Group**: Can view, create, edit, and delete objects in Groups. Does not need to be in Access Group. Also controls Profile content.
3. **Access Group**: Can only view the object and read its content. This is the lowest level of power at the group level.

#### Application Server Configurations (System Configuration)

Application server configurations can now be performed by Administrators and advanced IT staff, eliminating the need for support calls and IIS restarts.

![General System Configurations](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1794-6831.png)

**Two roles control access:**

| Role | Type | Default | Note |
|------|------|---------|------|
| **ManageSystemConfiguration** | System Security Role | **Nobody** | NOT automatically granted to the Administrator |
| **SystemConfigurationPage** | System User Interface Role | **Administrator Group** | Read-only for all assigned |

**Key characteristics:**
- Changes are automatically applied **every 2 minutes** (no IIS restart needed).
- Changes are recorded in the **Audit** tab.
- 6 categories: General, Environment, Memory, Multithreading, Recycling, Database Server Connections.
- Memory and Multithreading must be enabled by support before they can be changed.
- To mitigate misuse, Customer Support can disable complete features, sections, and property changes via XML/App config.

**General System Configurations - Detailed Properties:**

| Property | Description |
|----------|-------------|
| Use Detailed Error Logging | When True, stack trace is shown. When False, it is not |
| User Inactivity Timeout (minutes) | Minutes before timeout due to user inactivity |
| Task Inactivity Timeout (minutes) | Minutes before timeout due to task inactivity |
| Logon Inactivity Threshold (days) | Days of inactivity before disabling user. -1 to deactivate |
| Task Scheduler Validation Frequency (days) | Days between Task Scheduler validations |
| Culture Codes | Regional configuration code (e.g., en-US) |
| White List File Extensions | File types allowed in File Explorer |
| Num Seconds Before Logging Slow Formulas | Seconds before logging slow formulas. Impacts performance |
| Number Seconds Before Logging Get Data Cells | Default 180. Only increase for debugging |

**Environment System Configurations:**

![Environment Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1797-6838.png)

| Property | Description |
|----------|-------------|
| Environment Name | Custom environment name (up to 150 characters) |
| Environment Color | Hex color for the environment name |
| Logon Agreement Type | Select Custom to display a custom message at login |
| Logon Agreement Message | Agreement message text at login |
| Full Width Banner Message | Message displayed in a top banner |
| Full Banner Display Type | Informational, Warning, Successful, or Critical |
| Can Close Full Banner | True allows closing the banner; False keeps it permanent |

![Banner types](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1799-6844.png)

**Recycling System Configurations:**

![Recycling Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1801-6855.png)

| Property | Default | Description |
|----------|---------|-------------|
| Auto Recycle Num Running Hours | 24 | Hours between automatic recycles |
| Auto Recycle Start Hour UTC | 5 | Earliest hour (UTC) for recycling |
| Auto Recycle End Hour UTC | 7 | Latest hour (UTC) for recycling |
| Auto Recycle Max Pause Time (minutes) | 30 | Minutes to pause before recycling, allowing active tasks to finish |

**Configuration Audits:**

Changes to configurations are tracked via the Audit tab, visible at System > System Configuration > Audit.

![System Configuration Audit tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1805-6868.png)

---

### Objective 201.6.3: Troubleshooting Security Access

#### Diagnostic Flow

When a user cannot access something, follow this verification order:

1. **Verify the user is enabled**: System > Security > Users > Is Enabled = True.
2. **Verify Logon Inactivity Threshold**: If Remaining Allowed Inactivity = 0 Days, the user is disabled due to inactivity.
3. **Verify OpenApplication role**: That the user's group is assigned to OpenApplication.
4. **Verify Cube access**: Access Group or Maintenance Group of the Cube.
5. **Verify Scenario security**: Read Data Group and/or Read/Write Data Group of the Scenario.
6. **Verify Entity security**: Read Data Group and/or Read/Write Data Group of the Entity.
7. **Verify Data Cell Access** (Slice Security): Review the order of rules and behaviors.
8. **Verify Workflow Security**: Workflow Execution Group, Data Group, Approver Group.
9. **Verify Application Security Roles**: ModifyData, ManageData, and specific roles.
10. **Verify Application User Interface Roles**: Access to corresponding pages.
11. **Verify System Security Roles and System User Interface Roles** if the issue is in the System tab.

#### User Creation and Management

**Step-by-step procedure for creating users:**

1. Click System > Security > Users.
2. Click Create User.
3. Enter name and description.
4. In User Type, select the purchased license type.
5. Set Is Enabled to True to activate the user.
6. Review Status and Inactivity Threshold.
7. Configure authentication (see next section).

![Role assignment for non-administrators](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch16-p1773-8031.png)

**User Types** (license type, does NOT control access):

| Type | Description |
|------|-------------|
| **Interactive** | Full access to all functions and tools |
| **View** | Data, reports, and dashboards access only. Cannot load, calculate, consolidate, certify, or change data |
| **Restricted** | Cannot use some Solution Exchange functionalities (Lease, Account Reconciliation, etc.) due to contractual limitations |
| **Third Party Access** | Access via third-party application with named account. Cannot change data, modify artifacts, or access Windows app or browser |
| **Financial Close** | Can use Account Reconciliation and Transaction Matching |

**User preference properties:**

| Property | Description |
|----------|-------------|
| Email | Email address for alerts and messages generated by business rules |
| Culture | User's regional configuration |
| Grid Rows Per Page | Number of rows per page in grids. Consider connectivity and screen resolution |
| Custom Text (Text 1-8) | Customizable texts for metadata tags, distribution filters, custom functionality |
| Group Membership | Groups to which the user belongs |

![User Preferences and Group Membership](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch16-p1776-6766.png)

**User Management:**

| Action | Description |
|--------|-------------|
| Delete Selected Item | Permanently delete the user |
| Copy Selected Item | Create new user based on the selected user's settings |
| Show all parent groups for user | View all groups the user is a member of. Useful for identifying access |

**Copying users:**
1. System > Security > Users.
2. Select user, click Copy Selected Item.
3. Enter new name.
4. Select **Copy References made by parent groups** to add the new user to the original's groups (except exclusion groups).
5. Click OK and modify settings if necessary.

#### Authentication

Two available authentication methods:

| Method | External Authentication Provider | External Provider User Name | Internal Provider Password |
|--------|----------------------------------|----------------------------|---------------------------|
| **Native** | Select (Not Used) | Leave blank | Enter password |
| **External** | Select the external IdP | Enter the username in the IdP (must be unique and match) | Not applicable |

**Supported external providers:**
- Microsoft Active Directory (MSAD)
- Lightweight Directory Access Protocol (LDAP)
- OpenID Connect (OIDC): Azure AD (Microsoft Entra ID), Okta, PingFederate
- SAML 2.0: Okta, PingFederate, ADFS, Salesforce

**Logon screens based on authentication configuration:**

| Configuration | Screen |
|--------------|--------|
| Native only | Username and password fields |
| External IdP only | "External Provider Sign In" button |
| Native + External | Username/password fields AND External Sign In button |

In environments hosted by OneStream, **OneStream IdentityServer** is used.

#### Groups and Inherited Security

**Access cannot be assigned to individual users** directly to tools and artifacts. System Security Roles assigned to Groups determine this access. Groups are created to grant customized function-based access to large numbers of users.

- Groups can be nested hierarchically. Child groups inherit access from the parent group, from a parent level downward.
- **Groups cannot be externally authenticated**.
- Removing child groups from parent groups revokes access to the tools and artifacts that the parent group provides.

**Procedure for creating Groups:**

1. System > Security.
2. Click Groups, then Create Group.
3. Enter an intuitive name and description.
4. In Group Membership, specify users or child groups, or select a parent group.
5. Click Save.

#### Exclusion Groups

For granting access to almost everyone except a few specific users.

**Procedure for creating Exclusion Groups:**

1. System > Administration > Security.
2. Click Groups, then Create Exclusion Group.
3. Enter a descriptive name (e.g., `Everyone-But-<Department>`).
4. In Group Membership, add child groups or users.
5. Set members to **Deny Access** or **Allow Access**.
6. **Use the arrows to order the exclusions carefully**.

![Example of misconfigured Exclusion Group](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch16-p1786-6806.png)

**Order is critical**: Evaluation occurs in the specified order, regardless of the user's membership. Example:
- If Amelia and Bob are in the Frankfurt Controller group, and they are listed first individually with Deny Access but Frankfurt Controller is listed after with Allow Access, **they will not be restricted** because the group's Allow overrides.
- **Correct solution**: Put Frankfurt Controller first (Allow Access), then Amelia and Bob below (Deny Access).

#### Manage System Security (for non-Administrators)

![Manage System Security roles](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1808-6878.png)

**ManageSystemSecurityUsers - Allows:**
- Create, modify, delete, and disable users
- Specify properties: General, Authentication, Preferences, Custom Text

**ManageSystemSecurityUsers - Limitations:**
- CANNOT create, modify, or delete administrators (directly or indirectly)
- CANNOT add or remove themselves from groups or roles
- CANNOT delete themselves
- CANNOT add other users to Manage System Security privileges
- To manage group membership or copy users, ManageSystemSecurityGroups is required

**ManageSystemSecurityGroups - Allows:**
- Manage groups and exclusion groups
- Add or remove group members
- Copy groups (except groups with Administrator privileges)

**ManageSystemSecurityGroups - Limitations:**
- CANNOT modify the Administrators group
- CANNOT assign users to groups with Administrator privileges
- CANNOT modify their own membership in other groups
- CANNOT modify the parent group of a group they are a member of

**ManageSystemSecurityRoles - Allows:**
- Manage system security role assignments

**ManageSystemSecurityRoles - Limitations:**
- CANNOT modify the ManageSystemSecurityRole (requires Administrator)
- CANNOT assign the Everyone or Nobody groups (requires Administrator)
- CANNOT add a group to a role of which they are a member

**Combined roles**: Having more than one Manage System Security role amplifies capabilities. For example, with Users and Groups roles together, you can copy a user or add a user to a group.

**Security Load/Extract requires:**
- ManageSystemSecurityUsers
- ManageSystemSecurityGroups
- ManageSystemSecurityRoles
- SystemLoadExtractPage (UI Role)

Users with Manage System Security automatically get access to SystemAdministrationLogon and SystemPane UI Roles.

![BRApi for security management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch17-p1811-6886.png)

**Load/Extract validation for users with Manage System Security:**
Validation occurs by comparing the current security state in the destination environment with the changed state determined by processing the source file. Therefore, security must **pre-exist** in the destination environment to determine the changed state. For new or empty environments, an **Administrator** must perform the load.

#### The Administrator User

- **Cannot be disabled** or deleted.
- **Is not affected** by Logon Inactivity Threshold.
- **Bypasses all application security**. This cannot be changed.
- Assigning other groups to roles does NOT revoke the Administrator's access.
- Important consideration: For sensitive data (e.g., People Planning), **Event Handlers and BRAPI calls** are needed to exclude the Administrator from seeing certain data, since the Administrator group has overriding authority even if all roles that default to Administrator are changed.

#### Disable Inactive Users

Allows creating an authorization policy to disable users after a specific period of login inactivity.

**Step-by-step procedure for configuring the Inactivity Threshold:**

1. Click File > Open Application Server Configuration File.

![Open configuration file](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p507-2691.png)

2. Open the configuration file.

![Configuration opened](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p507-2692.png)

3. Click Security.

![Security section of the config](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p508-2695.png)

4. Set the Logon Inactivity Threshold (days) to the number of days of inactivity before the user is disabled.

![Setting the threshold](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p508-2696.png)

5. Click OK, then Save.
6. **Reset IIS** to recognize the changes.

**User status review:**

![Reviewing Inactivity Threshold in users](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p510-2703.png)

- If **Remaining Allowed Inactivity = 0 Days**, the user no longer has access.

![User with 0 days remaining](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p511-2706.png)

- The user receives a message indicating they have been disabled when attempting to log in.

![Disabled user message](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p511-2707.png)

- To re-enable: System > Security > Users > Is Enabled = True.

![Re-enabling a user](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p512-2710.png)

- **Does not apply to the Administrator**.
- Applies to both native and external users.
- Can also be configured via System Configuration (General > Logon Inactivity Threshold). Set to -1 to deactivate.

#### Troubleshooting Workflow Security

| Workflow Group | Function | Effect if missing |
|---------------|---------|------------------|
| **Workflow Execution Group** | Allows executing Import, Forms, Journals | User does not see the Workflow (only Cube Root) |
| **Data Group** | Access to data within the Workflow | Error when validating/loading data |
| **Approver Group** | Allows certifying the Workflow | Certification buttons appear grayed out |

![Successful data loader - Figure 9.9](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p288-2805.png)

![Certification disabled for data loader - Figure 9.10](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p288-2804.png)

**Example - User without workflow group:**

If a user does not have the workflow group, they will only see the Cube Root, no workflows beneath:

![User without workflow access - Figure 9.12](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p289-2815.png)

**Example - View User with Workflow Responsibility:**

A Houston Approver needs to certify the parent workflow after reviewing figures, but does not need to load data. They need:
- Same level of workflow access but different execution level (certification)
- Only view access to the entities (not edit)

![Approver configuration - Figure 9.19](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p296-2869.png)

![Approver with correct access - Figure 9.20](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p297-2887.png)

#### Troubleshooting Dashboards and Cube Views

**Dashboards:**
- The user needs access to the **Dashboard Profile** AND the **Dashboard Group**.
- If they have access to the Group but not the Profile, they will not see the Dashboards in OnePlace.
- If they have access to the Profile but not to certain Dashboard Groups in that Profile, they will not see the Dashboards from that group.
- If a Dashboard points to data the user cannot see: it shows NoAccess, empty cell, or "No Data Series".

![Dashboard security - Figure 9.28](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p302-2929.png)

**Cube Views:**
- Cube View Groups are assigned to Cube View Profiles. Access Group is assigned in the Profile.
- Setting **Visible in Profiles** (True/False) on the Cube View itself.

![Cube View Visible in Profiles - Figure 9.29 and 9.30](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p302-2934.png)

- Cube View properties that should be **False** for non-administrator users:
  - **Can Modify Data**
  - **Can Calculate**
  - **Can Translate**
  - **Can Consolidate**

![Cube View restrictions - Figure 9.31](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch09-p303-2944.png)

#### Bulk Security Loading

**Three ways to add users and groups to an application:**

1. **Define them manually** in System Security.
2. **Batch load** using an XML file generated from the **SecurityTemplate.xlsx** of the SampleTemplates OneStream Solution (recommended option).
3. **Use APIs** (BRApi functions).

**Load procedure:**
1. System > Tools > Load/Extract.
2. Select Load and browse for the XML file.
3. Click Load.
4. Review the user/group list to verify they are correctly defined.

**Extract procedure:**
1. System > Tools > Load/Extract.
2. Click Extract, select File Type > Security.
3. In Items to Extract, select Users, Security Groups, or All.
4. **Extract Unique IDs**: If checked, unique IDs are extracted. **Uncheck when moving between environments** where records already exist to avoid duplicate errors.
5. Extract or Extract and Edit (to modify the XML).

**Requirements for non-Administrator users who need to Load/Extract Security:**
- SystemLoadExtractPage (UI Role)
- ManageSystemSecurityUsers
- ManageSystemSecurityGroups
- ManageSystemSecurityRoles

**BRApi functions available for security management:**

```
BRApi.Security.Admin.GetUsers / GetUser / SaveUser / RenameUser / DeleteUser / CopyUser
BRApi.Security.Admin.GetGroups / GetGroup / SaveGroup / RenameGroup / DeleteGroup / CopyGroup
BRApi.Security.Admin.GetExclusionGroups / GetExclusionGroup / SaveExclusionGroup / RenameExclusionGroup / DeleteExclusionGroup / CopyExclusionGroup
BRApi.Security.Admin.GetSystemRoles / GetApplicationRoles / GetRole
```

These functions are controlled by the assigned Manage System Security role. If a dashboard executes a BRApi to insert a user, the system validates that the clicking user is an Administrator or has ManageSystemSecurityUsers.

#### Restricting Users to an Application

**Step-by-step procedure:**

1. Go to System > Administration > Security.
2. Create a new security group.
3. Assign all users who should have access to the application to the new group.
4. Refresh the application so the new group appears in dropdown menus.
5. Go to Application > Tools > Security Roles > OpenApplication.

![OpenApplication configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch07-p500-2671.png)

6. Click the dropdown and select the new security group.
7. Click Save.

---

## Security Best Practices by Object Type

| Object | Access Group | Maintenance Group | Note |
|--------|-------------|-------------------|------|
| **Confirmation Rules** (Groups and Profiles) | Everyone | Administrators | Runtime access depends on the assigned Workflow Profile |
| **Certification Questions** (Groups and Profiles) | Everyone | Administrators | If the user has Workflow Execution Access, they can execute them |
| **Data Sources** | N/A (no object-level security) | N/A | Controlled by ManageDataSources role |
| **Transformation Rules** (core/shared) | Everyone | Administrators | For specific groups (e.g., by location), assign appropriate groups |
| **Form/Journal Templates** (Groups and Profiles) | Everyone | Administrators | Runtime access depends on the Workflow Profile |
| **Cube View Groups** | Everyone | Administrators + builders | Keep groups small for flexibility |
| **Dashboard Groups** | Everyone | Administrators + builders | Keep groups small; use multiple Dashboard Maintenance Units |

---

## Critical Points to Memorize

### Data Cell Access:
- Security flow: Application > Cube > Scenario > Entity > Data Cell Access > Workflow.
- **Data Cell Access** can only **decrease** already granted access; then optionally increase specific intersections.
- **ViewAllData** bypasses Data Cell Access Security completely. Data cannot be restricted for a ViewAllData user using slice.
- **Order of rules** in Data Cell Access is critical (evaluated sequentially).
- **"Increase Access And Stop"** stops evaluation of subsequent rules.
- **Data Cell Conditional Input** impacts ALL users (not group-based security).
- **Relationship Security** (Use Parent Security) is a Cube-level setting, not per user.
- Once activated, it affects ALL users except Administrators and ViewAllData.

### Security Roles:
- **Application Security Roles**: Control management/action on objects (ManageMetadata, ModifyData, etc.)
- **Application User Interface Roles**: Control page access (CubeViewsPage, BusinessRulesPage, etc.)
- Both must work together: UI Role gives visibility, Security Role gives capability.
- When a new application is created: everything defaults to Administrator except **OpenApplication** (Everyone) and **AdministratorDatabase** (Nobody).
- **ModifyData** can be left at Everyone if the rest of security is properly configured.

### System Security Roles:
- Applied to the entire system/framework, not to a single application.
- **ManageSystemConfiguration**: Default to **Nobody**, NOT automatic for Administrator.
- **ManageSystemSecurityUsers/Groups/Roles**: All three are needed for Load/Extract of Security.
- Users with Manage System Security automatically get access to SystemAdministrationLogon and SystemPane UI Roles.
- **Exclusion Groups**: Order determines access (Allow/Deny). Put the general group first (Allow), individuals after (Deny).
- Roles are NOT exclusive or limiting. If granted, users gain additional functionality.

### System Configuration:
- Changes are applied automatically every 2 minutes without IIS restart.
- 6 categories: General, Environment, Memory, Multithreading, Recycling, Database Server Connections.
- Memory and Multithreading must be enabled by support.
- Changes are automatically audited.

### Troubleshooting:
- Verify IsEnabled, Inactivity Threshold, OpenApplication, Cube Access, Scenario, Entity, Slice, Workflow.
- The Administrator **cannot be disabled** and is not affected by Inactivity Threshold.
- **User Type** (Interactive, View, Restricted, etc.) is a license type, it does NOT control access.
- If a user sees "NoAccess" in Data Explorer: verify Entity Read Data Group or Data Cell Access.
- **Show All Parent Groups for User**: Useful tool to see all of the user's groups and understand their access.
- **Logon Activity** in System > Logging shows the login method (Client Module: Scheduler, Windows App, etc.).
- If a user does not see workflows: verify that they have the Workflow Execution Group assigned.
- If a user sees entity data in reports but cannot load: verify Read/Write Data Group and Workflow Execution.

### Authentication:
- Native: External Provider = (Not Used), internal password. The first time the user logs in, they can change their password.
- External: Select provider and configure External Provider User Name (must be unique and match the IdP).
- Providers: MSAD, LDAP, Azure AD (Microsoft Entra ID), Okta, PingFederate, SAML 2.0.
- In OneStream-hosted environments, OneStream IdentityServer is used.
- The logon screen varies depending on the authentication configuration.

### Best Practices:
- Do not over-apply security; it is easier to add later than to undo.
- Maintain consistent naming conventions (e.g., `V_Entity` for view, `M_Entity` for modification, `WF_WorkflowName`).
- Use group nesting to ease administration, but do not overcomplicate.
- Access Group at Everyone + Maintenance at Administrators is the best practice for most objects.
- For Cube Views: Can Modify Data, Can Calculate, Can Translate, Can Consolidate set to False for reports.
- Assign ManageData only to administrators.
- Limit access to File Explorer; anyone with access to a folder sees everything through OnePlace.
- Design security as a natural part of implementation, not as an afterthought.
- Have a "security check" to determine how to implement security as the build progresses.
- For sensitive employee data (People Planning), plan Event Handlers and BRAPI calls from the design phase.

---

## Source Mapping

| Objective | Book/Chapter |
|-----------|-------------|
| 201.6.1 | Design Reference Guide, Chapter 7 - Implementing Security (Data Cell Access, Entity Security, Relationship Security); Foundation Handbook, Chapter 9 - Security (Data Loaders, Slice Security, View Users) |
| 201.6.2 | Design Reference Guide, Chapter 7 - Implementing Security (Application Security); Chapter 17 - System Security Roles (System Security Roles, System UI Roles, Manage System Security, Application Server Configurations); Foundation Handbook, Chapter 9 - Security (Application Security Roles, System Security Roles) |
| 201.6.3 | Design Reference Guide, Chapter 7 - Implementing Security (Security Configurations, Disable Inactive Users); Chapter 14 - User Authentication and SSO; Chapter 15 - How Users are Configured for Authentication; Chapter 16 - About Managing Users and Groups (Users, Groups, Exclusion Groups, Load/Extract, BRApi); Chapter 17 - System Security Roles (Manage System Security, Combined Roles, File Share Security); Foundation Handbook, Chapter 9 - Security (Common Types of Users, Troubleshooting examples, Relationship Security, Reporting Access) |


---


# Section 7: Administration (9% of the exam)

## Exam Objectives

- **201.7.1**: Demonstrate an understanding of the items available on the system tab
- **201.7.2**: Demonstrate troubleshooting abilities to solve issues

---

## Key Concepts

### Objective 201.7.1: Items available on the System tab

The **System** tab provides access to administration tools, logging, security, and environment management. The main items are grouped into three major areas: **Logging**, **Tools**, and **Environment**. Tab visibility is determined by the user's security configurations.

---

#### 1. LOGGING (System > Logging)

The logging system allows administrators to monitor user activity, tasks, and errors. All content is displayed in grids that can be sorted (click on column header for ascending/descending), filtered, and exported (right-click > Export, select file type).

![Sort logging grids](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1823-6922.png)

![Filter grid content](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1823-6923.png)

You can navigate between pages with page buttons (first, previous, next, last) located at the bottom left of the grid. To export data: right-click > Export > select format.

![Export grid data](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1823-6924.png)

##### Logon Activity

Shows who is connected and who disconnected.

![Logon Activity screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1824-6927.png)

**Grid columns:**

| Column | Description |
|--------|-------------|
| **Logon Status** | Shows when users logged in and logged out |
| **User** | User ID |
| **Application** | Application the user is connected to |
| **Client Module** | Client type (e.g., Excel, Windows App, Scheduler) |
| **Client Version** | Application version in use |
| **Client IP Address** | End user's IP address |
| **Logon Time** | Login timestamp |
| **Last Activity Time** | Last activity timestamp |
| **Logoff Time** | Logout timestamp |
| **Primary App Server** | Application server used |

**Administrator functions:**

| Button | Description |
|--------|-------------|
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1824-6928.png) **Logoff Selected Session** | Disconnect any user session. Only available to administrators |
| **Clear Logon Activity** | Clear all logon activity records |

**Related System Security Roles:**
- **ViewAllLogonActivity**: If the required access to the System tab and the Logon Activity page is assigned, users in the assigned group can view the logon activity of **all** users.
- **LogonActivityPage**: Allows access to the page. Can view all users but cannot log off.

##### Task Activity

Accessed from **two places**:
- **Task Activity** icon in the upper-right corner of the web client
- **System > Logging > Task Activity**

![Task Activity screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6931.png)

**Main buttons:**

| Button | Description |
|--------|-------------|
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6932.png) **Clear task activity for current user** | Clear current user's activity |
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6933.png) **Clear task activity for all users** | Clear activity for ALL users |
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6934.png) **Selected task information** | Drill down into the steps of the selected activity |
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1825-6935.png) **Running task progress** | View progress of other users' activities |

**Grid columns:**

| Column | Description |
|--------|-------------|
| **Task Type** | Type of activity (Consolidate, Process Cube, Load and Transform, Clear Stage Data, etc.) |
| **Description** | Details (POV, Multiple Data Units, etc.) |
| **Duration** | Activity duration |
| **Task Status** | Status (Completed, Failed, Canceling, Canceled, Running) |
| **User** | User ID |
| **Application** | Application where the task was processed |
| **Server** | Application server used |
| **Start Time** | Start timestamp |
| **End Time** | End timestamp |
| **Queued CPU** | CPU utilization % when the task was initiated |
| **Start CPU** | CPU utilization % when the job started from the queue |

**Child Steps**: Within the grid, there are two icons to the left of each row. If they are highlighted, you can drill down:

![Child Steps icons](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1827-6945.png)

- The **first icon** shows the child steps of a particular task.
- The **second icon** shows detailed error information when present.

**Related System Security Roles:**
- **ViewAllTaskActivity**: Users in the assigned group can view tasks and detailed child-steps through the Task Activity icon in the toolbar. They can also view it in System > Logging > Task Activity if they have access to the System tab and page.

##### Cancellation of long-running tasks

**Cube Views:**
If a Cube View takes more than **10 seconds**, a dialog appears with an indeterminate progress bar and **Cancel Task** and **Close** buttons.

![Cube View cancellation dialog](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1832-6959.png)

- If you click **Close**: the dialog closes, Task Activity flashes (indicating a background task), and the report opens when it completes.
- If you click **Cancel Task**: a cancellation message appears and the report does not run.
- If you don't click anything: the dialog closes on its own when the report finishes loading.

**Task Activity icon behavior:**
- The icon **flashes** when there are tasks running in the background that have been running for more than 10 seconds and the Task Activity dialog is not open.
- The icon **does not flash** if a Task Activity dialog is open.
- For all non-UI tasks (consolidation or other long-running tasks), the icon will start flashing within a few minutes.

![Task Activity with cancellation options](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1832-6961.png)

**Cancellation behavior table by type:**

| Type | Behavior when exceeding 10 seconds | Cancellation method |
|------|-----------------------------------|--------------------|
| **Cube View** (Data Explorer, Refresh) | Dialog with progress bar | Cancel Task or Close in the dialog |
| **Show Report** | Dialog with progress bar | Cancel Task or Close in the dialog |
| **Export to Excel** | Dialog with progress bar | Cancel Task or Close in the dialog |
| **Dashboard with Cube View Components** | **NO pop-up dialog appears** | Use Task Activity to cancel each individual Cube View |
| **Quick Views** | Via Task Activity in Excel Add-In or Spreadsheet | Running Task Progress > Cancel Task |
| **XFGetCell Refresh** | Via Task Activity | Running Task Progress > Cancel Task (shows bar with percentage) |

**Note**: Canceling the loading of the next page (Next Page) in Cube View/Quick View shows **#REFRESH** text in the cells.

**Task Activity in Excel:**

![Task Activity in Excel Add-In](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1828-6948.png)

You can access the Task Activity icon in the Excel Add-In in the Tasks section of the OneStream ribbon. Administrators can cancel other users' tasks. Non-admins have the option to show only running tasks.

In Task Activity, the Task Status column shows whether the task was canceled by a User or Administrator. Each task has its task type (Quick View, Cube View, Get Excel Data).

##### Detailed Logging (for Cube Views)

Configuration is **individual per Cube View**, in the Designer and Advanced tabs. It is **no longer** in the App Server Config file or in TALogCubeViews.

- Property **Use Detailed Logging**: default **False**.
- When **True**, individual steps and additional information about the Cube View are recorded in Task Activity.

![Use Detailed Logging location in Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1838-6982.png)

**Procedure to enable:**
1. In the Application tab, under Presentation, click Workspaces.
2. In Application Workspaces, under Workspaces, expand Default > Maintenance Unit > Default > Cube View Groups.
3. Select a Cube View.
4. Click the Designer tab and select Common under General settings.
5. Change Use Detailed Logging to True.

![Detailed Logging result in Task Activity](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1839-6986.png)

##### Cube View Paging and Quick View Paging

**Paging Controls**: Navigate between pages with arrows (first, previous, next). Each page shows page number and percentage of data loaded.

![Cube View paging controls](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1840-6989.png)

![Percentage of data loaded](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1840-6990.png)

**Paging tooltip**: Shows total rows, rows processed, unsuppressed rows on a page.

![Paging tooltip](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1840-6991.png)

**Quick View Paging**: Paging controls are now located **below the three tabs** (no longer in the Quick Views tab).

![Quick View paging controls](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1843-6998.png)

**Cube View settings that impact paging (System Configuration):**

| Property | Default | Description |
|----------|---------|-------------|
| Max Number of Expanded Cube View Rows | Server Config | Maximum rows when expanding a Cube View |
| Max Number of Unsuppressed Rows Per Cube View Page | 2000 | Maximum rows per page. Maximum value: 100,000 |
| Max Number Seconds To Process Cube View | 20 | Impacts paging behavior. Maximum value: 600 seconds |

These settings can be overridden per individual Cube View using the General Settings/Common/Paging properties on each Cube View.

**Task Activity Configurations (System Configuration):**

| Property | Default | Description |
|----------|---------|-------------|
| Log Books | True | Records in Task Activity when book items are included as steps |
| Log Cube Views | False | Records when a Cube View is opened, report is run, or exported to Excel |
| Log Quick Views | False | Records when a Quick View is created or rows/columns are moved |
| Log Get Data Cells Threshold | N/A | Records GetDataCells calls if the number of cells is >= the value |

##### Error Logs

Access: **System > Logging > Error Logs**.

![Error Logs screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1846-7011.png)

**Grid columns:**

| Column | Description |
|--------|-------------|
| **Description** | Brief error description |
| **Error Time** | Error timestamp |
| **Error Level** | Error type (Unknown, Fatal, Warning, etc.) |
| **User** | User ID |
| **Application** | Application where the error occurred |
| **Tier** | Application layer (App Server, Web Server, Client) |
| **App Server** | Application server the user was connected to |

**Functions:**

| Button | Description |
|--------|-------------|
| ![](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch18-p1847-7014.png) **Clear error log for current user** | Clear logs for the current user |
| **Clear error log for all users** | Clear logs for ALL users |

**Related System Security Roles:**
- **ViewAllErrorLog**: Users in the assigned group can view the Error Log of all users.
- **ErrorLogPage**: Allows access to the Error Log page.

---

#### 2. TOOLS (System > Tools)

##### Database

Access: **System > Tools > Database**.

![Database screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1851-7025.png)

Allows viewing database tables:
- **Application Database > Tables**: Tables of the current application
- **System Database > Tables**: System tables
- **Read-only** access to data tables, useful for debugging.
- Tables imported from MarketPlace Solutions use the schema name as a prefix (e.g., `rcm.AccessGroup`, `txm.AccessGroup`).

**Data Records**: Application Database > Tools > Data Records, to view complete system data with member filter.

**System Security Role**: **DatabasePage** allows access to this section. Only system administrators by default.

##### Environment

Access: **System > Tools > Environment** (only accessible via **OneStream Windows App**, not browser).

![Main Environment screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1852-7028.png)

The Environment page is designed to give IT users and power business users a way to manage and optimize their applications and environment. It allows monitoring the environment, isolating bottlenecks, viewing properties and configuration changes, and scaling servers and database resources.

**Main Environment sections:**

##### Monitoring

![Monitoring page](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1853-7031.png)

Provides access to real-time KPIs, interactive charts, and user activity. Instead of logging into the server to collect metrics, this page is used.

**Available actions:**
- **Open**: Access metrics file and configurations from File System or local folder.
- **Save As**: Save metrics and configurations locally or to the File System.
- **Settings**: Specify KPI metric types to monitor: Environment, Application Servers, Database Servers, Server Sets.
- **Zoom**: Zoom into part of the chart to see running or queued activities.
- **Refresh Automatically Using a Timer**: Retrieve metrics based on the Play Update Frequency interval.

**General Monitoring settings:**

| Property | Description |
|----------|-------------|
| Play Update Frequency (seconds) | Performance chart update frequency |
| Metric and Task Time Range | Amount of historical data to retrieve. Helps identify the cause of an event |
| Y-Axis Auto Range | If selected, the system sets Min/Max automatically |
| Secondary Y-Axis | For series with different value ranges or mixed data types |

**Monitoring filter settings:**

| Property | Description |
|----------|-------------|
| Filter Type | Type of servers for which to collect metrics |
| Source Filter | Filter for the Source List |
| Source List | Servers that meet the criteria |
| Result List | Items selected for metric collection |

**Collectible metrics (configured in Server Configuration Utility):**

![Metrics configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1855-7036.png)

| Category | Metrics |
|----------|---------|
| **Environment** | CPU, Task, Login |
| **Server Set** | CPU, Task |
| **Server** | Disk, Memory, Network Card |
| **SQL Server** | CPU, Page (Page Life Expectancy), Memory, Connection, Query (Deletes/Inserts), File Growth |
| **SQL Elastic Pool** | CPU, DTU, Storage, Workload |

##### Web Servers

Lists all web servers in the environment with configuration and audit.

![Web Server Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1857-7042.png)

**Configuration properties:**

| Property | Description |
|----------|-------------|
| Identity Provider | SSO provider if used |
| Server Heartbeat Update Interval (seconds) | Heartbeat update frequency |
| Name | Server name defined in the web configuration file |
| WCF Address | Full server URL |
| WCF Connection | Connection status (Ok = connected) |
| Used for General Access | True/False. Whether the server is configured for general access |
| Used for Stage Load | True/False. Whether it is configured for Stage |
| Used for Consolidation | True/False. Whether it is configured for consolidation |
| Used for Data Management | True/False. Whether it is configured for Data Management |

**Audit**: Shows property changes with Property Type, Property Name, Value From/To, Description From/To, Timestamp From/To, User From/To.

![Web Server Audit](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1859-7047.png)

##### Web to App Server Connections

Lists all combined connections from web configuration files to all application servers.

![Web to App Server Connections](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1862-7056.png)

| Action | Description |
|--------|-------------|
| **Pause** | Pause any requests to a WCF Address connection. The connection can be an Application Server or a Load Balancer |
| **Resume** | Resume requests to the connection |

Same configuration properties as Web Servers (Name, WCF Address, WCF Connection, Used for General Access/Stage Load/Consolidation/Data Management).

##### Application Server Sets

![Application Server Sets](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1864-7061.png)

Shows the environment's server sets. A red "X" indicates offline servers.

**Behavior tab:**

![Server Sets Behavior](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1865-7064.png)

- **Scale Out** (remove) and **Scale In** (add and configure): Available if Scaling Type is Manual or ManualAndBusinessRule.
- Properties configurable in Server Configuration Utility, but overridable at individual server level:
  - Process Queued Consolidation Tasks
  - Process Queued Data Management Tasks
  - Process Queued Stage Tasks
  - Queued Tasks Require Named Application Server

**Configuration tab:**

![Server Sets Configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1866-7067.png)

| Property | Description |
|----------|-------------|
| Azure Resource Group Name | (Azure Scale Set Only) Resource group name |
| Azure Scale Set Name | (Azure Scale Set Only) Scale set name in the resource group |
| Can Change Queuing Options On Servers | If True, Admins can change queuing behavior |
| Can Pause or Resume Servers | If True, the user can pause/resume from Environment |
| Can Stop or Start Servers | (Azure Scale Set Only) If True, can stop/restart |
| Maximum/Minimum Capacity | (Azure Scale Set Only) Max/min number of servers |

**Audit tab:** Identifies changes to XFAppServerConfig.xml. Changes are highlighted in **yellow**.

![Server Sets Audit](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1868-7072.png)

##### Application Server Behavior

![Application Server Behavior](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1869-7075.png)

| Action | Description |
|--------|-------------|
| **Pause** | Stops accepting new tasks from the queue, but allows already-started tasks to finish |
| **Resume** | The server resumes accepting tasks from the queue |
| **Recycle App Pool** | IIS reset for a specific server |
| **Stop** (Azure Only) | Stops the server. Continues incurring Azure compute charges. Public and internal IP are preserved |
| **Stop (Deallocate)** (Azure Only) | Stops without VM charges, but public and internal IP are removed |

The availability of these buttons depends on configurations in OneStream Server Configuration Utility.

**Additional Application Server tabs:**

| Tab | Description |
|-----|-------------|
| **Configuration** | Server configurations from Server Configuration Utility |
| **Hardware** | Machine hardware information |
| **Audit** | Hardware and configuration change history |
| **Performance** | Server and Environment metrics |

![Application Server Hardware](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1871-7081.png)

##### Database Servers (Connection Items)

Lists database connections based on Server Configuration Utility.

| Tab | Description |
|-----|-------------|
| **Behavior** | Available if SQL Server Azure and Elastic Pool are configured. Allows increasing/decreasing resources |
| **Configuration** | SQL Server configuration properties |
| **Hardware** | SQL Server hardware information |
| **Audit** | Changes to SQL Server properties |
| **Performance** | SQL Server metrics |
| **Diagnostics** | SQL diagnostic commands |

![Database Server Diagnostics](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1879-7105.png)

**Available Diagnostics:**
- **SQL Deadlock information**: Lists deadlocks on the SQL Server instance
- **Top SQL Commands**: Lists top SQL commands by Total Logical Reads, Total Logical Writes, or Total Worker Time

![Top SQL Commands](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1880-7108.png)

##### OneStream Database Servers (Schema Items)

Lists all database schemas.

| Tab | Description |
|-----|-------------|
| **Configuration** | Application-specific information |
| **Audit** | Application configuration changes |
| **Diagnostic** | Table fragmentation report for the current schema |

![Table Fragmentation Report](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1882-7115.png)

##### System Business Rules

**System Extender Business Rules** are used with Azure Server Sets for enhanced scalability at the Azure Database and Server Sets level. They allow Server and eDTU scaling manually or via Business Rules.

![Empty System Extender Business Rule](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1849-7019.png)

If System Business Rules is selected as the Scaling Type, a user-defined Extender rule determines whether scaling is needed. Environment and Scale Set metrics (for server scaling) or SQL Server Elastic Pool metrics (for database scaling) are passed to the function.

##### File Explorer

Access: System > Tools > File Explorer, or File Explorer icon in the toolbar.

![Main File Explorer](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1891-7139.png)

**Three storage roots:**

| Root | Description |
|------|-------------|
| **Application Database** | Documents of the current application |
| **System Database** | Complete system documents without affecting the current application |
| **File Share** | Self-service directory external to databases |

**Application folders:**

![Application Folders](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1895-7154.png)

| Folder | Description |
|--------|-------------|
| **Batch/Harvest** | Automation of Connector Data Loads. Harvest is automatically cleaned when loading and an archive folder is created. Requires Admin or ManageFileShare to add/delete files |
| **Content** | Secure storage for large files. Managed by System Security Roles |
| **Data Management** | Default folder for Data Management exports |
| **Groups** | Files by Security Group for dashboard components with File Source Type of File Share |
| **Incoming** | Source files for import to Stage. Also used by File Viewer Component |
| **Internal** | File content from Application/System Database |
| **Outgoing** | Available for custom processes |

**File Share:**

![File Share](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1892-7144.png)

File Share is a self-service directory that supports file storage external to OneStream databases. Files stored in an Incoming folder are only accessible through that Workflow Profile.

**Content Folders (Application and System):**
- Auto-generated folder for storing files **larger than 300 MB**.
- Secure storage area that can be used instead of the File Explorer application database.
- Permissions controlled by System Security Roles:
  - **Administrator** and **ManageFileShare**: Full rights
  - Non-Administrators are assigned via File Share Security Roles (ManageFileShareContents, AccessFileShareContents, RetrieveFileShareContents)

![Content Folder Permissions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1894-7150.png)

**Supported file sizes:**

| Interface | Uploads (App/System) | Uploads (Content) | Downloads |
|-----------|---------------------|--------------------|-----------|
| **Windows Application** | Up to 300 MB | Up to **2 GB** | Up to 2 GB |

**Saved POVs:** Can be saved to the Public (all users) or User directory. Right-click on Cube POV > Save Cube POV to Favorites.

![Save POV in File Explorer](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1892-7143.png)

##### Whitelist File Extensions

Defines which file extensions are allowed in File Explorer. Helps mitigate the risk of uploading malicious file types.

![Whitelist File Extensions configuration](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1903-7181.png)

**Step-by-step procedure:**

1. Go to Start > OneStream Software > OneStream Server Configuration Utility (Run as Administrator).
2. File > Open Application Server Configuration File.
3. Find the XFAppServerConfig.xml (typically in `C:\OneStreamShare\Config`).
4. Locate **Whitelist File Extensions** and click the ellipsis (...).
5. Click (+) and type the extension (e.g., txt).
6. Continue adding extensions.
7. Click OK, then Save.
8. **Restart IIS** (mandatory).

![Add extensions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1904-7184.png)

**Note**: Cloud clients must contact support for this change. When the whitelist is empty, any file type can be saved. When defined, files with extensions not included will be rejected when attempting to upload.

**Also configurable via System Configuration** (General > White List File Extensions), which avoids the IIS restart since changes are applied automatically every 2 minutes.

##### Load/Extract System Artifacts

For **System Administrators** only. Import/export of system sections in XML format.

![Load/Extract System Artifacts](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch13-p1764-8030.png)

**Extraction options:**

| Option | Description |
|--------|-------------|
| **Extract** | Export to XML file at specified location |
| **Load** | Import from XML file |
| **Extract and Edit** | Extract and modify the XML directly |

**Extractable items:**

| Item | Description |
|------|-------------|
| **Security** | System Roles, Users, Security Groups, Exclusion Groups. Extract Unique IDs option available |
| **System Dashboards** | Maintenance Units, Groups, Dashboard Components, Adapters, Parameters, Profiles |
| **Error Log** | With Start Time and End Time filter |
| **Task Activity** | With Start Time and End Time filter |
| **Logon Activity** | With Start Time and End Time filter |

**Extract Unique IDs**: If checked, OneStream unique IDs are extracted. When moving between environments where records already exist, **uncheck** this option to avoid errors.

##### Profiler

Tool for **developers** that captures every event processed in a user session.

![Profiler screen](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1910-7199.png)

**Security Roles:**

| Role | Type | Description |
|------|------|-------------|
| **ManageProfiler** | System Security Role | Run Profiler sessions and view Profiler Events. Default: Administrator |
| **ProfilerPage** | System UI Role | View the Profiler page. CANNOT run sessions or view events. Default: Administrator |

Administrators can terminate any active Profiler session.

**Session properties:**

| Property | Default | Max | Description |
|----------|---------|-----|-------------|
| Description | (empty) | N/A | Session description |
| Number of Minutes to Run | 20 | **60** | Maximum duration. Values > 60 reset to default |
| Number of Hours to Retain Before Deletion | 24 | **168** | Hours before deletion. Values > 168 reset to default. Deleted on first server restart after the time |

**Profiler filters:**

| Filter | Description |
|--------|-------------|
| Include Top Level Methods | Captures high-level entry points (user actions, API calls, workflow steps) |
| Include Adapters | Includes Data Adapter calls |
| Include Business Rules | Includes Business Rules |
| Include Formulas | Includes formulas |
| Include Assembly Factory | Includes Assembly Factory calls (labeled as "Factory") |
| Include Assembly Methods | Includes Assembly Method calls (labeled as "WSAS") |
| Business Rule Filter | Filter by Business Rule name |
| Formula Filter | Filter by Formula name |
| Assembly Method Filter | Filter by method name |

**Wildcards in filters:**
- `?` = any single character (e.g., `SS?B` finds SSIB, SSVB)
- `*` = any string (e.g., `*Validation` finds rules ending in Validation)
- Separated by comma for multiple filters

**Profiler Events:**

![Profiler Events window](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1916-7223.png)

| Column | Description |
|--------|-------------|
| **Event Type** | Top, Queue, Adapter, Formula, BR, Factory, WSAS, Manual |
| **Workspace** | Name of the Workspace containing the item |
| **Source** | Event origin (varies by type: rule name, adapter, formula, method) |
| **Method** | Method called |
| **Description** | Description of the event type |
| **Entity** | Associated Entity member (empty if not applicable) |
| **Cons** | Consolidation member (empty if not applicable) |
| **Duration** | Event duration |
| **Server** | Server |
| **Thread ID** | Operating system thread ID |

**Post Processing:**

![Post Processing window](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1919-7233.png)

Calculate Cumulative Durations allows viewing information in different groupings, totalizing filtered items, and viewing results. Useful for improving performance queries: filter by specific event types and group them to see which methods or events take the most time.

**Event Information:**
- **Method Inputs**: Parameters passed to the method
- **Method Result**: Result or returned values
- **Method Error**: Errors encountered during the session
- **Objects** and **Text**: Enabled for Manual event types if logged via BRApi calls

![Event Information window](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1922-7242.png)

**BRApi.Profiler Methods:**

| Method | Description |
|--------|-------------|
| `BRApi.Profiler.IsProfiling(si)` | Determines if profiling is enabled for the session |
| `BRApi.Profiler.LogMessage(si, brProfilerSettings, "Description", "Detail")` | Logs a custom message in Profiler Events |
| `BRApi.Profiler.LogObjects(si, brProfilerSettings, "Description", [objects])` | Logs object state for diagnostics |

**IMPORTANT**: Application performance may be impacted if multiple users run Profiler simultaneously or perform long-running tasks while others are running Profiler.

##### Time Dimensions

Applications can have a monthly or weekly Time Dimension.

**Time Dimension Types:**

| Type | Description |
|------|-------------|
| **Standard** | Monthly. Stores data by month. Applications created before 4.1.0 use this type |
| **StandardUsingBinaryData** | Monthly with data in binary table. Use if you may need to convert to Weekly later |
| **M12_3333_W52_445** | 12 months, 4 quarters, 52 weeks, 445 calendar |
| **M12_3333_W52_454** | 12 months, 4 quarters, 52 weeks, 454 calendar |
| **M12_3333_W52_544** | 12 months, 4 quarters, 52 weeks, 544 calendar |
| **M12_3333_W53_445** | 12 months, 4 quarters, 53 weeks, 445 calendar |
| **M12_3333_W53_454** | 12 months, 4 quarters, 53 weeks, 454 calendar |
| **M12_3333_W53_544** | 12 months, 4 quarters, 53 weeks, 544 calendar |
| **Custom** | Allows specifying months per quarter and weeks per month. Only for new applications |

**Custom properties:**
- **Use Weeks**: If True, defines Weekly Time Dimension. If False, Monthly.
- **Vary Settings By Year**: If True, specify weeks per month per year. If False, apply to all years.
- **M1-M16 Number of Weeks**: Weeks per month.

**Critical note**: Once the application is created, the Time Dimension **cannot be edited** (but can be viewed in POV). An XML file is generated that new applications use to implement the desired Time Dimension.

##### Azure Configurations

Only applicable to Azure or when using Azure Elastic Pool.

**Azure Subscription Settings**: Must be completed when using Azure Elastic Pool or Scale Sets.

![Azure Subscription Settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1883-7118.png)

**Environment Monitoring:**

![Environment Monitoring settings](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1884-7121.png)

**Key Environment Monitoring properties:**

| Property | Default | Description |
|----------|---------|-------------|
| URL for Automatic Recycle Service | N/A | Recycle service URL. Default port 50002 |
| Number of Running Hours Before Automatic Recycle | 24.0 | Recycle frequency. 0.0 to disable |
| Start Hour for Automatic Recycle (UTC) | 5 | Earliest hour to recycle (only if Running Hours = 24.0) |
| End Hour for Automatic Recycle (UTC) | 7 | Latest hour to recycle |
| Maximum Number of Minutes to Pause Before Automatic Recycle | 30 | Time to allow active tasks to finish before recycling |
| Active Check Update Interval (seconds) | N/A | Frequency of system checks (deadlocks, etc.) |
| Metric Update Interval (seconds) | N/A | Metrics update frequency |
| Server Heartbeat Update Interval (seconds) | N/A | Heartbeat update frequency |

**Task Load Balancing:**

![Task Load Balancing](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch19-p1888-7130.png)

| Property | Description |
|----------|-------------|
| Maximum Queued Processing Interval (seconds) | Frequency with which the queuing thread looks for new tasks |
| Number Past Metric Reading For Analysis | Number of metric readings for server demand analysis |
| Maximum Queued Time (minutes) | **Maximum time before canceling** a queued task |
| Maximum Average CPU Utilization | Maximum average CPU before determining a server cannot take a task |
| Task Logging Only | If True, only logs tasks picked up |
| Detailed Logging | If True, logs each entry/exit of the Task Load Balancing function |

---

#### 3. NAVIGATION (OnePlace Layout)

##### Main interface structure

![Complete OnePlace Layout](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p323-2047.png)

- **Navigation Pane**: Three tabs (System, Application, OnePlace). Visibility based on user security. Each bar can be shown pinned or with Auto Hide.
- **Home**: Large OneStream icon to navigate to the user's home screen.

##### Application Tray (top toolbar)

| Icon | Function |
|------|----------|
| **Hamburger Menu** (Navigation Pane) | Show/hide Application, System, OnePlace tabs |
| **Navigate Recent Pages** | Dialog to return to recent pages |
| **File Explorer** | Access to public folders, documents, and File Share |
| **Environment Name** | Customizable name per environment (Development, Test, Production). Configurable in Application Server Config file |
| **User ID and Application** | Shows current user and application |
| **Logon/Logoff Icon** | End Session (closes session and removes saved password) or Change Application (maintains session) |
| **Task Activity** | Shows all tasks performed |
| **Refresh Application** | Refreshes the application and verifies the first open tab |
| **Pin/Unpin Navigation Pane/POV Pane** | Show/hide panels |
| **Clipboard** | Up to 10 items (data cells, text, rule scripts) |
| **Create Windows Shortcut** | Create desktop shortcut |
| **Help** | OneStream documentation |

![Application Tray icons](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p324-2051.png)

##### Point of View (POV)

Panel on the right side of the application. Can be pinned or automatically hidden.

![Point of View panel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p330-2082.png)

| Section | Description | Editable by user |
|---------|-------------|------------------|
| **Global POV** | Scenario and Time. Configured by administrator | **No** |
| **Workflow POV** | Workflow, Scenario, Time. Based on Time Dimension Profile of the Cube assigned to the Workflow Profile | **No** |
| **Cube POV** | All Cube dimensions. Active and updatable | **Yes** |

**Save favorite POV**: Right-click on Cube POV > Save Cube POV to Favorites. Saved in Application > Documents > Users > (User Name) > Favorites.

![Select User POV](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p332-2087.png)

**User Defined Descriptions**: When hovering over a selectable POV member, the defined description is shown. Fixed (non-selectable) dimensions show the description and add "Not Used by Current Page".

##### Page Settings

| Option | Description |
|--------|-------------|
| **Refresh Page** | Refresh the current page |
| **Close Page** | Close the current page |
| **Create Shortcut** | Create shortcut in the user's Favorites folder. Works for Cube Views, Dashboards, or Application/System pages |
| **Set Current Page as Home Page** | Controls default page and pinning of Navigation/POV panels at login. Works in browser and Windows App |
| **Clear Home Page Setting** | Remove current home page setting |
| **Save Home Page Setting As Default For New Users** | Save as default for new users |
| **Close All Pages** | Close all open pages |
| **Close All Pages Except Current Page** | Close all except the current one |

**Workflow Bar**: Shows position in the Workflow process. Green = completed, blue = incomplete, white icon = current task.

##### Access to the OneStream Windows App

The Windows App is useful because:
- It updates automatically when the server is updated.
- It does not require admin rights to download or use.
- It offers robust spreadsheet functionality (may not need the Excel Add-in).

**Create desktop shortcut:**

![Create Windows Shortcut](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p318-7939.png)

##### Logon Screens

The logon screen varies depending on the environment's authentication configuration:

| Configuration | Screen |
|--------------|--------|
| Native Authentication Only | Username and password fields |
| External Identity Provider Only | External Provider Sign In button |
| Native + External | Both available |

![Native Authentication Only](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p320-2038.png)

![Native + External](/Users/aurelio.santos/Desktop/OneStreamDoc/output/design-reference-guide/images/design-reference-guide-ch03-p321-2042.png)

**Note**: SSO with external identity provider can be configured to require a one-time verification code for authentication.

---

### Objective 201.7.2: Troubleshooting abilities to solve issues

#### Troubleshooting Tools

##### Task Activity as a starting point

- First place to verify if a Calculation/Consolidation completed successfully or failed.
- Filter by Task Type.
- Drill down into child steps to see error details.
- Task Status: Completed, Failed, Canceling, Canceled, Running.

**Detailed Child Steps:**
The two icons to the left of each row allow:
1. Viewing child steps of the task (duration of each step)
2. Viewing detailed error information

##### Error Log

- **System > Logging > Error Logs**.
- It is a repository of ALL OneStream errors, not just Calculations.
- Use filters to find the relevant error.
- Contains: Description, Error Time, Error Level, User, Application, Tier, App Server.

##### Writing to the Error Log (Manual Logging)

| Method | Availability | Performance |
|--------|-------------|-------------|
| `api.LogMessage` | Finance Business Rules only | **Best performance** |
| `BRApi.ErrorLog.LogMessage` | ALL rule types | Lower performance |

You can log:
- **Strings**: Directly
- **Decimals**: Converted with `.ToString()` or `.XFToString()`
- **Lists**: With `String.Join` or `For Each`
- **Data Buffers**: With `.LogDataBuffer` (requires API, string for name, integer for max cells - **recommended 100**)

**Error Level** can be changed with the `XFErrorLevel` argument.

**CAUTION**: When writing to the error log, avoid including sensitive information. OneStream attempts to filter and redact sensitive information, replacing it with `[Redacted]`.

##### Stopwatch (timer)

- VB.NET class `System.Diagnostics.Stopwatch`.
- Add `Imports System.Diagnostics` to the header.
- `Stopwatch.StartNew()` to start, `.Elapsed` to record elapsed time.
- Useful for identifying which part of the code takes the most time.

##### Calculate With Logging (Calculation Drill Down)

- Executed from Data Management (Consolidate With Logging / Calculate With Logging) or Cube View (Force Consolidate With Logging).
- Allows drilling down into Child Steps of Task Activity to see each DUCS step with duration.
- You can see: Dependent Data Units, each Consolidation Member processed, Formula Passes, Business Rules, individual formulas.
- The `CreateDataUnitCache` step shows the number of data records in cache (Data Unit Size).
- **Only available** for Calculations that run within the DUCS. Custom Calculations are analyzed individually via Data Management.
- **WARNING**: Consolidate/Calculate With Logging adds significant processing time.

##### Profiler

- Captures Business Rules, Formulas, Workspace Assemblies events.
- View method inputs and outputs.
- Useful for performance troubleshooting.
- Post Processing allows grouping by Event Type and viewing cumulative durations.
- Limited to **60 minutes** max per session and **168 hours** of retention.
- If default values are deleted and no value is entered, Profiler defaults to 20 min/24 hrs. If zero is entered, it defaults to 1 min/1 hr.

##### Rubber Duck Debugging

- Troubleshooting method where the problem is articulated step by step to an inanimate object.
- Useful when all other options have been exhausted.

#### Environment Management - Best Practices

- **System changes** should follow best practice procedures: first deploy and test in a development environment.
- It is recommended to make a recent copy of the production database, rename it, and use it as the base for changes.
- Before deploying to production, extract changes from development and evaluate them in a separate test environment.
- **Deploying changes to production** should avoid times of high load and high activity.

**Artifacts that especially should NOT be changed in production during high activity:**
- Business Rules (especially those with Global functions)
- Confirmation Rules
- Metadata (especially with member formulas)

**Additional recommendations:**
- Standard environments: schedule production changes during slow periods or off-hours.
- Large environments: consider the **Pause** functionality within the Environment tab.
- Consider the OneStream Solution **Process Blocker**, which allows pausing critical processes for maintenance without closing the application.
- IIS has an **Idle Time-Out** setting for OneStreamAppAppPool. It must be set to **0** since OneStream has other settings for recycling IIS.
- For active and global environments with Data Management Sequences running regularly, it is recommended to **recycle IIS every 24 hours**.

#### Common Errors

| Error | Cause | Solution |
|-------|-------|----------|
| Calculation does not produce results | Dimensions do not match in Data Buffers | Use LogDataBuffer, verify Common Members, adjust Member Scripts |
| Inconsistent results | Formula Pass issue (dependencies in the same pass) | Change to Formula Pass 16 to isolate; reassign passes correctly |
| Compilation Error | Typo or incorrect syntax | See line and reason for error, correct and recompile |
| Invalid Member Name | Typo in formula string (e.g., `A#Priec`) | Correct the member name |
| Unclosed Parentheses | Unclosed parentheses in formula | Verify parenthesis matching |
| Unbalanced Buffer | Data Buffers with different Common Members | Use Unbalanced functions (MultiplyUnbalanced, etc.) |
| Data Explosion | Dimensions in source not present in destination | Never use `#All`; use Unbalanced functions or collapse dimensions |
| Object Not Set to Instance | Variable declared but not defined | Define the variable; pay attention to compilation warnings |
| Given Key Not Present in Dictionary | Parameters not defined in Data Management Step | Use `XFGetValue` with default; verify parameters in DM Step |
| Invalid Destination Script | Data Unit Dimensions in the destination Member Script | Use `If` statements to filter Data Units; only Account-level Dimensions in destination |
| Duplicate Members in Filter | Member appears twice in the filter | Verify that filters do not include duplicate members |

---

## Critical Points to Memorize

### Logging:
- **Task Activity** shows task status (Completed, Failed, Canceled) and allows canceling long-running tasks.
- Accessed from **two places**: icon in the top bar and System > Logging > Task Activity.
- The Task Activity icon **flashes** when a task has been running for more than 10 seconds in the background.
- The icon **does not flash** if a Task Activity dialog is open.
- **Detailed Logging** is configured **per individual Cube View** (not in the server config file).
- The **Error Log** is accessible in System > Logging and receives both automatic errors and manual messages via `api.LogMessage`.
- **ViewAllLogonActivity**, **ViewAllErrorLog**, **ViewAllTaskActivity** allow viewing information from all users (not just your own).
- To cancel Dashboards with Cube View Components: **there is no pop-up dialog**, use Task Activity.
- Canceling Next Page in Cube View/Quick View shows **#REFRESH** in the cells.

### Environment:
- **Environment** is only accessible via **OneStream Windows App** (not browser).
- Allows monitoring, isolating bottlenecks, viewing configurations, and scaling servers.
- **Pause** on an Application Server stops accepting new tasks from the queue, but allows in-progress tasks to finish.
- **Recycle App Pool** = IIS reset for a specific server.
- **Stop (Azure)** maintains compute charges and IPs; **Stop (Deallocate)** does not charge for VM but removes IPs.
- SQL Diagnostics: Deadlock information and Top SQL Commands (Total Logical Reads/Writes/Worker Time).
- Table Fragmentation report available in Schema Items > Diagnostic.

### Tools:
- **Database** provides **read-only** access to data tables. Useful for debugging.
- **Profiler** is limited to **60 minutes** max per session and **168 hours** of retention.
- **Whitelist File Extensions** are configured in the **Application Server Configuration File** or via System Configuration. Config file requires IIS restart; System Configuration applies automatically.
- **File Share Content folders** support files up to **2 GB** (Windows App), while Application/System Database supports up to **300 MB** (uploads).
- **Load/Extract System Artifacts** is only available for **System Administrators** and uses XML format.
- The **Extract Unique IDs** option must be unchecked when moving security between environments where records already exist.

### System Configuration:
- Changes are applied every **2 minutes** without IIS restart.
- **Recycling**: Default 24 hrs, between 05:00-07:00 UTC, 30 min pause before recycling.
- **Task Load Balancing**: Maximum Queued Time is the maximum time before canceling a queued task.
- **Cube View settings** (Max Rows, Max Unsuppressed Rows Per Page, Max Seconds) can be overridden per individual Cube View.

### Troubleshooting:
- **Calculate With Logging** is the key tool for Calculation performance troubleshooting, but adds significant time.
- Use `api.LogMessage` in Finance Rules (not `BRApi.ErrorLog.LogMessage`) for better performance.
- `LogDataBuffer` with max **100 cells** to avoid server overload.
- **Profiler** with BRApi.Profiler.LogMessage and LogObjects for advanced diagnostics.
- Do not change critical artifacts (Business Rules, Confirmation Rules, Metadata) during high activity in production.
- Recycle IIS every 24 hours for active environments with regular DM Sequences.
- IIS Idle Time-Out must be **0** for OneStreamAppAppPool.

### Navigation:
- **Navigation Pane** has 3 tabs: System, Application, OnePlace. Visibility by security.
- **Global POV** (Scenario, Time) is not editable by end user.
- **Cube POV** is active and updatable by the user.
- **Set Current Page as Home Page** controls the default page AND pinning of Navigation/POV panels.
- Works in browser and Windows App; changes transfer between both.
- **Workflow Bar**: Green = completed, blue = incomplete, white icon = current task.
- Windows App updates automatically and does not require admin rights to download.

---

## Source Mapping

| Objective | Book/Chapter |
|-----------|-------------|
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 18: Logging (Logon Activity, Task Activity, Cancel tasks, Detailed Logging, Paging, Error Logs) |
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 19: System Tools (Database, Environment, Monitoring, Web Servers, Application Server Sets, Application Server Behavior, Database Servers, File Explorer, File Share, Whitelist, Load/Extract, Profiler, Time Dimensions, Azure Configurations, Task Load Balancing) |
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 3: Navigation (OnePlace Layout, Application Tray, POV, Page Settings, Logon screens) |
| 201.7.1 (System Tab items) | Design Reference Guide - Chapter 17: System Security Roles (System Configuration, General/Environment/Recycling/Database settings, Audits) |
| 201.7.2 (Troubleshooting) | Design Reference Guide - Chapter 18: Logging (Task Activity drill down, Error Logs, Cancel mechanisms) |
| 201.7.2 (Troubleshooting) | Design Reference Guide - Chapter 19: System Tools (Environment diagnostics, Profiler events/post-processing/BRApi, Database Diagnostics) |
| 201.7.2 (Troubleshooting) | Design Reference Guide - Chapter 7: Implementing Security (Managing Environment best practices, IIS recycling) |
| 201.7.2 (Troubleshooting) | Finance Rules - Chapter 7: Troubleshooting and Performance (Error types, Logging, Calculation Drill Down, Stopwatch) |


---



# Section 8: Rules (16% of the exam)

**THIS IS THE HIGHEST-WEIGHTED SECTION OF THE EXAM.**

## Exam Objectives

- **201.8.1**: Demonstrate an understanding of the proper use case for various business rule types
- **201.8.2**: Demonstrate an understanding of the proper use case for various function types

---

## Key Concepts

### Objective 201.8.1: Use cases for the different Business Rule types

OneStream organizes Business Rules by the Engine they interact with. Each type has a specific purpose and is applied differently within the application. The Business Rule Library categorizes rules by Engine.

![Business Rule Library - categories by Engine](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p25-684.png)

![Business Rule Editor - components and key functions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p25-685.png)

**Programming language**: VB.NET is the standard language for Business Rules. C# is also available for Business Rules (but NOT for Member Formulas, which require VB.NET). The language decision depends on the client and their internal resources. OneStream recommends consistency in the chosen language for the entire application.

![Programming language options - VB.NET vs C#](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p234-2402.png)

---

#### 1. Finance Business Rules

**Purpose**: Financial calculations, member lists, custom translations, ownership and elimination logic, conditional input, on-demand rules, and much more. It is the most versatile and fundamental rule type of the Finance Engine.

**Where they are created**: Application > Tools > Business Rules > Finance.

![Finance Rules in the Business Rule Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p30-713.png)

![Finance Rules grouped - each rule is an independent object with VB.NET code](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p30-714.png)

**How they are activated**: They are assigned to the Cube (up to **8 Finance Business Rules** per Cube). They execute when a financial event occurs in the Cube (Calculation, Consolidation, Translation, or when a report is rendered).

![Assignment of up to 8 Finance Business Rules to the Cube](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p40-783.png)

**Activation exceptions**: The Finance Function Types `MemberList` and `DataCell` do not require assignment to the Cube; they can be activated directly from a Cube View. The Finance Function Type `Confirmation` can be activated from a Confirmation Rule.

**Code structure**:
- Pre-defined Namespace, Public Class, Public Function that the platform invokes.
- Each rule has access to the API library specific to the Finance Engine.
- IntelliSense available for autocompletion.
- In-Solution Documentation with context-sensitive help, snippets, and code examples.

![Code structure: pre-defined Namespace, Class, Function](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-720.png)

![Available Finance Function Types in Business Rules](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p31-721.png)

![Finance Function Types - visual diagram of use cases](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p236-2416.png)

**In-Solution Documentation and IntelliSense**:

![In-Solution Documentation - context-sensitive help and snippets](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p28-700.png)

![IntelliSense - function and parameter autocompletion](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p28-701.png)

**Advantages over Member Formulas**:
- Access to ALL Finance Function Types (especially Custom Calculate, which is NOT available in Member Formulas).
- Reuse of variables and objects throughout the entire rule.
- All Calculations in one place when volume is high.
- Ability to share logic between rules (`Contains Global Functions for Formulas = True`).
- Greater flexibility for specialized Finance Function Types: Translate, ConsolidateShare, ConsolidateElimination, ConditionalInput, OnDemand.

**Sharing logic between rules**:

![Create Shared Business Rule](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-749.png)

![Property Contains Global Functions for Formulas = True](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-750.png)

![Public functions within Shared Rule](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p35-751.png)

To call from a Member Formula:
```vb
Dim sharedFinanceBR As New
OneStream.BusinessRule.Finance.A1_SharedRules.MainClass
```

**When to use Finance Rules vs Member Formulas** (IMPORTANT for the exam):

| Criteria | Finance Business Rules | Member Formulas |
|----------|----------------------|-----------------|
| **Finance Function Types** | ALL (Calculate, Custom Calculate, Translate, Share, Elimination, DataCell, MemberList, ConditionalInput, OnDemand) | Only Calculate and DynamicCalc |
| **Language** | VB.NET or C# | VB.NET only |
| **Location** | Centralized Business Rule Library | Formula property of the individual member |
| **Variation by Scenario/Time** | Requires code (If statements) | Out-of-the-box via properties (dropdown menus) |
| **Formula Pass** | Position 1-8 is assigned in the Cube | FormulaPass 1-16 is assigned to the member |
| **Multi-threading** | Execute sequentially as written | Multithreaded within each pass (parallel) |
| **Out-of-the-box performance** | Lower (sequential) | Higher (parallel) |
| **High-volume maintenance** | Easier (all in one place) | More cumbersome (member by member) |
| **Typical use** | Planning/Forecast (driver-based, factor-based, large groups) | Consolidation/Actual (specific calculations: retained earnings, KPIs) |
| **Custom Calculate** | YES | NO |
| **Calculation Drilldown** | Not directly | YES (Formula for Calculation Drill Down) |

**General rule**:
- **Consolidation/Actual**: tend to use more **Member Formulas** (specific calculations per member: retained earnings, KPIs, DSO, DPO).
- **Planning/Forecast**: tend to use more **Business Rules** (calculations spanning large groups of members: driver-based, factor-based, zero-based).
- In practice, a **mix of both** is used in any well-configured application.

---

#### 2. Parser Business Rules

**Purpose**: Parse and transform incoming data during an import event. Common uses: parse debit/credit fields from a GL source file, character trimming, concatenation, derive a source ID from the source file name.

**How they are applied**: Configured on a Data Source Dimension by setting Logical Operator = Business Rule and defining the Parser Rule name in Logical Expression.

![Parser Rule - configuration on Data Source Dimension](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p236-2418.png)

**When it activates**: When the Stage Engine reads the configured Data Source during an import event.

---

#### 3. Connector Business Rules

**Purpose**: Facilitate integrations to extract data from external databases, data warehouses, or OneStream auxiliary tables into a Workflow. They also enable drill back to sub-ledger detail data.

**How they are applied**: Assigned directly to a Connector Data Source; activated when the user clicks Import in a Workflow. Typically use an External Connection configured on the Application Server.

**ConnectorActionTypes** (4 types - MEMORIZE):

| ConnectorActionType | Description | When it activates |
|---------------------|-------------|-------------------|
| `GetFieldList` | Defines the field names returned in the source data table | When the Stage Engine reads the Data Source during import |
| `GetData` | Processes (queries) the incoming source data | During data import |
| `GetDrillbackTypes` | Enables custom drill back to external detail | When configuring drill back types |
| `GetDrillBack` | Handles drill back processing | When the user executes drill back |

![Connector Business Rule - 4 ConnectorActionTypes](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p237-2427.png)

---

#### 4. Smart Integration Function Rules

**Purpose**: Execute remote functions for integrations with Smart Integration Connector. Enables secure connectivity between OneStream Cloud and data sources on the customer's network without VPN.

**Availability**: Private preview in 7.2, more widely used in 7.4, GA in 8.0+.

**Capabilities**:
- Secure VPN-free connectivity between OneStream Cloud and customer data sources.
- Create and manage data source connections via OneStream administration interfaces.
- Local management of database credentials and auxiliary files.
- Allows coding and centrally storing remote functions called from Connector or Extender Rules.

---

#### 5. Conditional Rules

**Purpose**: Conditionally map a source value to a destination value using code logic. Common case: set the target value dynamically based on the transformed target or source member of a Stage dimension.

**How they are applied**: Assigned to an individual Transformation Rule (composite, range, list, mask). Activated when the transformation executes during an import event.

**Key arguments**: `Args.GetTarget()`, `Args.GetSource()`.

![Conditional Rule - use of Args.GetTarget() and Args.GetSource()](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p238-2434.png)

**Other use cases**: Use metadata properties of a target dimension member to determine the mapping of another dimension. Example: map UD1 members depending on whether the record maps to an intercompany account.

**CRITICAL NOTE**: They are **very processing and performance intensive**. Use with extreme caution and only when there is no alternative.

---

#### 6. Derivative Business Rules

**Purpose**: Two main functions:
1. **Derive or add a record** to Stage and calculate its amount:
   - **Interim** = temporary, not transformed, used for check rule validations.
   - **Final** = transformed and loaded to the Cube.
2. **Enable check rules** for data validation (pass/fail) that execute in the Validate step of the Workflow (e.g., verify that the trial balance is balanced before allowing the user to complete the Validate step).

**How they are applied**: Assigned to a Derivative Transformation Rule in the Logical Operator setting. Activated on Import (record derivation) and Validate (check rules).

---

#### 7. Cube View Extender Business Rules

**Purpose**: Customize and format PDF reports from Cube Views (logo, page number, title, header font, word wrapping, font color, cell value, etc.). Practically unlimited formatting capability.

**IMPORTANT**: The logic ONLY applies when the Cube View is run as a **PDF report**. It does **NOT work** in Data Explorer grid mode.

**How they are applied**: In the Cube View, Custom Report Task = Execute Cube View Extender Business Rule, and the rule name is assigned.

![Cube View Extender - configuration in Cube View](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p239-2442.png)

---

#### 8. Dashboard Dataset Business Rules

**Purpose**: Create custom datasets and data tables for advanced parameters, reports, and dashboards. They can execute SQL queries, Method queries, and BRAPIs to build data tables from scratch or customize existing data tables.

**How they are applied** (two ways):
1. Directly in a **Data Adapter** for dashboard reports.
2. In a **Bound List Parameter** to create custom selection lists.

![Dashboard Dataset in Data Adapter](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p239-2444.png)

![Dashboard Dataset in Bound List Parameter](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p240-2452.png)

**Ideal use case**: When a custom data table is needed for a parameter, or querying a OneStream/external table for a report. If you can master this technique, you will be able to deliver virtually any custom reporting request.

---

#### 9. Dashboard Extender Business Rules

**Purpose**: Create highly customized interactive dashboards, including button click actions. Common cases: send emails, execute Workflow processes, present custom messages to users.

**Three Function Types (use cases) - MEMORIZE**:

| Function Type | Activates when... | Where it is applied |
|--------------|-------------------|---------------------|
| `LoadDashboard` | The dashboard attempts to render | Directly to the dashboard (main only, NOT nested) |
| `ComponentSelectionChanged` | A click/selection occurs on a component (button, combo box, chart, Cube View, grid view) | On almost any dashboard component |
| `SQLTableEditorSaveData` | Save is clicked in a SQL Table Editor | On a SQL Table Editor component |

![Dashboard Extender - 3 Function Types](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p240-2454.png)

**Important restriction**: The Dashboard Extender can only be activated on a **main dashboard**. It CANNOT be applied or activated on dashboards that are **nested** within a main dashboard.

---

#### 10. Dashboard XFBR String Business Rules

**Purpose**: Rules that **return text based on logic**. They can be used practically anywhere in the application that expects text: report books, formatting properties, Cube View headers, dashboard components, etc.

**Common use cases**:
- Dynamic POV member in Cube View
- Dynamic Cube View or dashboard formatting
- Row/column Member Filter in Cube View
- Shared row/column template in Cube View
- Default parameter value
- Get data cell amounts for Specialty Planning calculations

**Practical example**: Report with 3 columns (Actual, Current Forecast, Prior Forecast). The user only selects the current month. An XFBR String Rule dynamically determines which Prior Forecast based on the user's selection, eliminating the need for manual selection.

**Recommendation**: It is the **best starting point** for people without coding experience, as it does not require understanding Data Units, data buffers, or data explosion. It just returns text based on simple logic.

---

#### 11. Extensibility Business Rules

Two main types:

**a) Extender Rules**:
- Facilitate the execution of custom automated tasks.
- Common cases: automate GL data import in a Workflow, file management (FTP - pick up/place files), export datasets to CSV or other formats.
- They are one of only **two types** of Business Rules that can be called directly from a **Data Management step**. They are frequently combined with DM to create fully automated solutions.

**b) Event Handler Rules**:
- Automatically activated when a specific event occurs on the platform.
- They are the **ONLY type** of rule that does NOT need to be called from another artifact. OneStream has an Event Engine that listens for events and automatically activates the code.
- **7 Event Handler types** (MEMORIZE):
  1. **Transformation Event Handler** - intercepts transformation events (import, validate, load cube, clear Stage data)
  2. **Journal Event Handler** - journal events (submit, approve)
  3. **Data Quality Event Handler** - data quality events
  4. **Data Management Event Handler** - Data Management events
  5. **Forms Event Handler** - form events
  6. **Workflow Event Handler** - Workflow events (certify, lock, unlock)
  7. **WCF Event Handler** - WCF events
- Common cases: Scenario seeding in Process Cube, email when import fails, email when Workflow is certified, auto-create members in Dimension Library.
- They can **block processes** (e.g., prevent Workflow lock/certify, throw custom error messages).
- Available sub-events are documented in the Event Listing section of the OneStream API Guide.

---

#### 12. Spreadsheet Business Rules

**Purpose**: Read or write to OneStream database tables within the Spreadsheet tool. Enables read/write and analysis on auxiliary table data (Specialty Planning registers, custom data tables, Solution Exchange tables).

**How they are applied**: Created directly in a Spreadsheet file via a Table View Definition.

![Spreadsheet Business Rule - Table View Definition](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p243-2471.png)

---

#### 13. System Extender Business Rules

**Purpose**: Used with Azure Server Sets for scalability at the Azure Database and Server Sets level. They determine whether server or database scaling is needed.

---

#### Summary Table of All Business Rule Types

| Type | Main Purpose | Where it is applied | Activation |
|------|-------------|---------------------|------------|
| **Finance** | Financial calculations, member lists, translations | Assigned to the Cube (up to 8) | Calculation, Consolidation, Translation, Report rendering |
| **Parser** | Parse data during import | Data Source Dimension (Logical Operator = BR) | Import event |
| **Connector** | Integration with external systems + drill back | Connector Data Source | Import click in Workflow |
| **Smart Integration Function** | Remote functions without VPN | Called from Connector/Extender | From other rules |
| **Conditional** | Conditional mapping in transformations | Individual Transformation Rule | Import event (transformation) |
| **Derivative** | Derive Stage records + check rules | Derivative Transformation Rule | Import (derivation) / Validate (checks) |
| **Cube View Extender** | PDF formatting of Cube Views | Cube View (Custom Report Task) | **PDF report only** |
| **Dashboard Dataset** | Custom datasets | Data Adapter / Bound List Parameter | Dashboard rendering / Parameter |
| **Dashboard Extender** | Interactive dashboards | Dashboard / Component / SQL Table Editor | Load, Click, Save |
| **Dashboard XFBR String** | Returns text based on logic | Anywhere that expects text | When the text is needed |
| **Extensibility Extender** | Automated tasks | Data Management step | DM step execution |
| **Extensibility Event Handler** | Tasks by event | None (auto-activation) | Platform event |
| **Spreadsheet** | Read/write tables in Spreadsheet | Spreadsheet file (Table View) | Spreadsheet opening |

---

#### Member Formulas (not Business Rules, but the other category of rules)

**Purpose**: Calculations written directly on dimension members (Scenario, Flow, Account, UD1-UD8).

**Location**: **Formula** property of the member in the Dimension Library.

![Member Formula Editor - access from Dimension Library](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p32-727.png)

![Variation by Scenario Type and Time - dropdown menus](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p32-728.png)

![Formula Editor - similar to the Business Rule Editor](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p33-735.png)

**Member Formula types**:
- **Stored Formula**: executes during Calculation/Consolidation (DUCS). Assigned to a Formula Pass.
- **Dynamic Calculation (DynamicCalc)**: calculated on-the-fly when referenced in a report. Formula Type = DynamicCalc.

**Key member properties** (CRITICAL for the exam):

| Property | Description | Values / Notes |
|----------|-------------|----------------|
| **Formula Pass** | Determines execution order in the DUCS | FormulaPass 1-16. Members in the same pass and dimension run in **parallel** (multithreaded). |
| **Is Consolidated** | Controls whether calculated data is consolidated | Default: `Conditional (True if no Formula Type)` = if it has a Formula Pass, it does NOT consolidate. **Change to True** if consolidation is desired. |
| **Allow Input** | Controls whether user input is allowed | Change to **False** if the Account will only have calculated data. |
| **Formula Type** | Type of formula | `DynamicCalc` for dynamic calculations, empty for stored. |
| **Account Type** | Type of account | Must also be `DynamicCalc` for Accounts with dynamic calculations. |
| **Switch Type** | View member behavior | True = behaves like Revenue/Expense (Activity, QTD, MTD work correctly). |

![Member Formula - assigned Formula Pass property](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p41-789.png)

**Calculation Drilldown**: Can be enabled in the member's `Formula for Calculation Drill Down` property, using drill down API functions. Allows the user to see the calculation inputs when drilling down.

![Calculation Drilldown - Formula for Calculation Drill Down property](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p33-736.png)

![Calculation Drilldown - drill down API functions](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p34-743.png)

![Member Formula and Calculation Drilldown - general view](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p243-2473.png)

**Advantages**:
- Natural organization: the code is directly on the member.
- Variation by Scenario Type and/or Time without needing code.
- Multithreaded Formula Passes (members in the same pass run in parallel).
- **Best out-of-the-box performance** vs Business Rules.

**Disadvantages**:
- Only support Finance Function Types: **Calculate** and **Dynamic Calc** (not Custom Calculate, Translate, Share, Elimination, etc.).
- Formula Passes cannot vary by Scenario Type or Time (a member can only have one Formula Pass).
- With high calculation volume, going member by member can be cumbersome.
- If the logic changes, each member must be modified individually.

---

#### Assemblies (evolution of Business Rules)

**Concept**: Assemblies are the new way of organizing Business Rules within Workspaces. Almost all Business Rule types are available as Assemblies.

**Two Assembly types**:

| Type | Description | Finance Rules | Dynamic Content | Conversion |
|------|-------------|---------------|-----------------|------------|
| **Assembly Business Rules** | Direct transition from traditional Business Rules. Same code, different location (Workspace > Maintenance Unit > Assembly) | Do NOT support Finance BRs | NO | 1-to-1 conversion |
| **Assembly Services** | More advanced. Use Service Factory for routing. Recommended by OneStream for sophisticated logic | YES (Finance logic) | YES (Dynamic Dashboards, Dynamic Cubes) | Requires refactoring |

**Key Assembly benefits**:
- Organization by functional area (not by rule type).
- Improved portability between environments.
- Multithreading support.
- Development with external tools (Visual Studio).
- Enhanced security (encryption with password).
- Compiler Language is defined at the Assembly level (VB.NET or C#).
- Assembly names must be unique within the Workspace.

**Invocation reference** (3 ways):
1. **Traditional**: `BRName` (central repository).
2. **Assembly Business Rule**: `WS:WorkspaceName/Assembly:AssemblyName/BRName`.
3. **Assembly Service**: via Service Factory routing.

---

### Objective 201.8.2: Use cases for the different Finance Function Types

Finance Function Types determine WHEN and HOW logic executes within a Finance Business Rule. They are exclusive to Business Rules (Member Formulas only support Calculate and DynamicCalc).

---

#### Finance Function Types - Complete Table

| Finance Function Type | Description | Consolidation Member | When it executes |
|----------------------|-------------|---------------------|------------------|
| **Calculate** | Standard calculation within the DUCS | `C#Local` | Calculation/Consolidation |
| **Translate** | Custom currency translation logic | `C#Translated` | Consolidation (Translation) |
| **ConsolidateShare** | Custom ownership % calculation | `C#Share` | Consolidation (Share) |
| **ConsolidateElimination** | Custom IC elimination logic | `C#Elimination` | Consolidation (Elimination) |
| **CustomCalculate** | Calculation outside the DUCS (on-demand) | Defined in DM step | Only via Data Management step |
| **DynamicCalc / DataCell** | Dynamic in-memory calculation for reports | N/A (in memory) | Cube View/Report rendering |
| **MemberList** | Returns a list of members for Cube Views | N/A | Cube View row/column rendering |
| **ConditionalInput** | Controls conditional input | N/A | Data input by user |
| **OnDemand** | On-demand execution | N/A | Via Data Management or dashboard |
| **Confirmation** | Confirmation logic | N/A | Confirmation rule trigger |

![Finance Function Types vs Consolidation Members - when each one executes](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p40-782.png)

---

#### Calculate (within the DUCS) - DETAILED

Executes as part of the Data Unit Calculation Sequence (DUCS). Activated via Consolidation or Calculation of the Cube. The DUCS is **all or nothing**: ALL steps execute every time, without exception. You cannot choose to run only one Formula Pass.

**DUCS activation mechanisms** (4 ways):
1. **Data Management**: Create Step > Calculate or Consolidate Step Type.
2. **Cube View**: Enabling Calculate/Consolidate in Cube View properties.
3. **Dashboard Button**: Server Task that executes DM Sequence or Calculation directly.
4. **Workflow Process Step**: via Workflow Name with Process/Pre-Process steps.

![Data Management - Create Calculate Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p42-795.png)

![Calculation Type options: Calculate, Translate, Consolidate (with Logging and Force variations)](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p43-803.png)

![Define Data Unit dimensions in the DM Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p43-804.png)

![Cube View - enable Calculate/Consolidate in properties](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p44-810.png)

![Dashboard Button - Server Task for Calculation](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p46-824.png)

![Workflow Process Step - Calculation Definitions tab](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p47-831.png)

**Complete DUCS sequence** (MEMORIZE):
1. Clear previously calculated data (does not clear Durable Data; requires `Clear Calculated Data During Calc = True` on Scenario).
2. Run Scenario Member Formula.
3. Perform reverse Translations by calculating Flow Members from alternate currency input Flow Members.
4. Execute Business Rules 1 and 2.
5. Execute Formula Passes 1-4 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).
6. Execute Business Rules 3 and 4.
7. Execute Formula Passes 5-8 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).
8. Execute Business Rules 5 and 6.
9. Execute Formula Passes 9-12 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).
10. Execute Business Rules 7 and 8.
11. Execute Formula Passes 13-16 (Account formulas > Flow formulas > UD1 > UD2 > ... > UD8).

**Formula Passes within the DUCS**: Members in the same pass and same dimension are **multithreaded** (run in parallel). The order within a pass is: Account formulas > Flow formulas > UD1 > UD2 > ... > UD8.

**The DUCS can execute up to 7+ times per Entity during Consolidation**:
1. Calculate Local Currency (`C#Local`)
2. Translate Local to Parent's Default Currency (`C#Translated`)
3. Calculate Translated Currency
4. Calculate `OwnerPreAdj`
5. Perform Share functions (`C#Share`)
6. Execute Elimination functions (`C#Elimination`)
7. Calculate `OwnerPostAdj`
8. Combine data from child entities to parent entity's `C#Local`
9. Calculate parent entity's local currency

**Calculation Status**: Determines if a Data Unit needs to be recalculated.
- **OK** = No Calculation Needed, Data Has Not Changed.
- **CN** = Calculation Needed, Data Has Changed.
- Regular Consolidate/Calculate respects Calculation Status (does not process OK Data Units).
- **Force** Consolidate/Calculate ignores Calculation Status and processes all Data Units.

**Parallel Processing**: The Finance Engine processes Sibling Data Units in parallel (multi-threading) to optimize times. Sibling Entities are processed simultaneously using multiple threads.

![Parallel Processing - Sibling Entities processed in parallel](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p48-838.png)

---

#### Custom Calculate (outside the DUCS) - DETAILED

Executes **only** via Data Management step (Custom Calculate Step Type). It is the primary tool for Planning solutions.

![Custom Calculate Step Type in Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-844.png)

**Key differences from Calculate (DUCS)**:

| Aspect | DUCS (Calculate) | Custom Calculate |
|--------|-------------------|-----------------|
| **Activation** | Calculation, Consolidation, Cube View, Workflow | Only via DM step |
| **Scope** | Entire Data Unit (all-or-nothing) | Only what is defined in the script |
| **Calculation Status** | Verified before executing | NOT verified; only processes defined DUs |
| **Clear Data** | Automatic (step 1 of DUCS) | Manual (`api.Data.ClearCalculatedData` at the start) |
| **Durable Data** | Not applicable (DUCS clears calculated data) | `isDurable = True` to protect data |
| **Parameters** | Not supported | YES - parameters and Account-level dimension POV |
| **Typical use** | Consolidation, Actual | Planning, Forecast, on-demand |
| **Formula Pass** | Determined by position in the DUCS | N/A |

![Custom Calculate - define Data Unit, Business Rule Name, Function Name](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-845.png)

![Custom Calculate - optional parameters](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p49-846.png)

![Custom Calculate - Dimension POV Members](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p50-852.png)

**Custom Calculate DM Step configuration**:
- **Business Rule Name**: name of the Finance Business Rule.
- **Function Name**: name of the Custom Calculate function within the rule.
- **Parameters**: Optional Name Value Pairs passed to the script. Use `XFGetValue` to retrieve with default value.
- **Dimension POV Members**: Account-level dimensions referenceable via `api.Pov` in the script.
- **Entity, Consolidation, Time filters**: allow multiple Members (works like Force Calculate).

**CRITICAL Custom Calculate rules**:
1. ALWAYS include `api.Data.ClearCalculatedData` at the start of the script (it is not cleared automatically).
2. Mark data as `isDurable = True` so it is NOT cleared by the DUCS.
3. NEVER use Data Unit Dimensions in the destination script (left side of ADC).
4. Can be linked to **Dashboards** with buttons that execute DM Sequences with user parameters.

**When to use Custom Calculate vs DUCS** (FREQUENTLY ASKED EXAM QUESTION):
- **Consolidation solutions**: primarily DUCS (cascading dependencies, data integrity, clear & replace, IC eliminations).
- **Planning solutions**: primarily Custom Calculate (iterative, multiple users, selective calculation, on-demand).
- **General rule**: Consolidation = DUCS; Planning = Custom Calculate + `C#Aggregated`.

---

#### C#Aggregated - DETAILED

Faster alternative to full Consolidation. Uses a modified version of the standard Consolidation algorithm.

![C#Aggregated - fast alternative to Consolidation](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p51-858.png)

![C#Aggregated - define in Consolidation Filter of DM Step](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch02-p51-859.png)

**What `C#Aggregated` does**:
- Only executes the DUCS on **Base Entities**.
- Only direct method Translation.
- Calculates percentage ownership.
- Data is stored in `C#Aggregated` on Parent Entities.

**What `C#Aggregated` does NOT do**:
- Does NOT perform Intercompany elimination logic.
- Does NOT execute complex Share/Ownership Calculations.
- Does NOT process other Consolidation Members (Local, Translated, Share, Elimination).
- Does NOT consider Parent Journal adjustments.

**Performance**: Can be up to **90% faster** than normal Consolidation.

**Primary use**: **Planning solutions** where IC eliminations and complex ownership are not needed. Combined with Custom Calculate, it is the typical configuration for Planning.

---

#### Dynamic Calc / DataCell - DETAILED

Executes in memory when referenced in a report (Cube View, Quick View). Does NOT store data in the Cube.

**Fundamental characteristics**:
- Does NOT execute during DUCS or Data Management.
- Works cell by cell (like Excel), NOT with Data Buffers.
- Inherits the POV of each report cell (18 dimensions available via `api.Pov`).
- Requires a `Return` statement followed by an object (numeric or textual).
- Does NOT naturally aggregate to Parent Members. For Parents, a Dynamic Calc must be written directly on the Parent.

**Where to place Dynamic Calculations** (3 options):

| Location | How to configure | Advantages | Disadvantages |
|----------|-----------------|------------|---------------|
| **Member Formula** | Formula Type = DynamicCalc (Account Type also = DynamicCalc for Accounts) | Executes whenever the member is referenced. Reusable in any Cube View. Supports text. | Code tied to the individual member. |
| **Business Rule (DataCell)** | FinanceFunctionType = DataCell. Called with `GetDataCell(BR#[BRName=...,FunctionName=...])` | Flexible, not tied to a member. Supports Name Value Pairs. | More complex syntax. Does not return text. |
| **Directly in Cube View** | `GetDataCell(...)` in rows/columns. Supports substitution variables and Column/Row math. | Quick for ad-hoc calculations. | Not reusable in other Cube Views. Does not work in Excel Add-in or Spreadsheet. |

**View Members for numeric data**: YTD, Periodic, QTD, MTD, HTD.
**View Members for text data**: Annotation, Assumptions, AuditComment, Footnote, VarianceExplanation.

**Functions available in Dynamic Calcs**:
- `Divide(A, B)`: division with protection against division by zero.
- `Variance(A, B)`: (A - B) / |B|.
- `VariancePercent(A, B)`: (A - B) / |B| * 100.
- `BWDiff(A, B)` / `BWPercent(A, B)`: Better/Worse considering Account Type.
- `api.Functions.GetDSODataCell("AcctsRec", "Sales")`: Days Sales Outstanding.

**Common UD8 technique**: Store Dynamic Calcs in UD8 (least-used dimension) so they inherit the POV of all other dimensions. Does not need to be assigned to the Cube.

**Name Value Pairs**: In DataCell Business Rules, they allow passing values from the Cube View to the Business Rule via `args.DataCellArgs.NameValuePairs`. The BR logic can change based on the passed value.

**Relational Blending**: Combining Cube data with relational table data (Stage, custom tables). Three methods:
- **Drill-Back Blending** (1-to-many): via drill back to detail data.
- **Application Blending** (1-to-many): for MarketPlace solutions.
- **Model Blending** (1-to-1): via Finance Engine API.
- Key function: `api.Functions.GetStageBlendTextUsingCurrentPOV` with intelligent caching.

---

#### Key Functions and APIs for Calculations

##### api.Data.Calculate (ADC) - THE MOST USED FUNCTION

The most used API function for financial calculations. Performs Data Buffer arithmetic. A single ADC can create thousands of calculated cells.

**Basic syntax**:
```text
api.Data.Calculate("A#Result = A#Source1 * A#Source2")
```

**Overloaded** with three versions:
1. `api.Data.Calculate(FormulaString, IsDurableCalculatedData)` - basic.
2. `api.Data.Calculate(FormulaString, OnEvalDataBuffer, UserState)` - with Eval.
3. `api.Data.Calculate(FormulaString, accountFilter, flowFilter, originFilter, icFilter, ud1Filter...ud8Filter, OnEvalDataBuffer, UserState, IsDurable)` - complete with 12 filters.

**CRITICAL ADC rules** (MEMORIZE):
- The formula string is **NOT case sensitive** (`a#price` = `A#Price`).
- Syntax errors within the formula string are only detected at **runtime** (not at compilation).
- **NEVER** include Data Unit Dimensions in the destination script (left side).
- Data Unit Dimensions CAN be used in source scripts (right side).
- When referencing Entity in a source script, include `C#Local` to avoid currency issues.

##### Data Buffer Math - HOW IT WORKS

OneStream analyzes each Data Buffer, matches cells based on **Common Members** (common dimensions), and performs the arithmetic. Only cells with **identical Primary Keys** (except the Account dimension) are operated on each other. If there are no matching cells, no result is produced.

**Common Members**: Account-level dimensions that are shared by all cells in the Data Buffer. Only Data Buffers with the **same Common Members** can be operated on directly ("balanced" Data Buffers).

##### Unbalanced Buffers - IMPORTANT

Data Buffers with different common dimensions = **Unbalanced**. OneStream throws an error if you try to operate on unbalanced Data Buffers directly (to prevent **data explosion**).

**Unbalanced functions**:
- `MultiplyUnbalanced(DB1, DB2, UnbalancedScript)`
- `DivideUnbalanced(DB1, DB2, UnbalancedScript)`
- `AddUnbalanced(DB1, DB2, UnbalancedScript)`
- `SubtractUnbalanced(DB1, DB2, UnbalancedScript)`

**Critical rule**: The Data Buffer with **MORE dimensions** must always be the **second argument**.

**Trick for DivideUnbalanced with unbalanced numerator**: Convert to MultiplyUnbalanced with `Divide(1, Denominator)`.

**Trick for SubtractUnbalanced with unbalanced first operand**: Use SubtractUnbalanced with inverted operands and multiply by `-1`.

**Double-Unbalanced** (both buffers unbalanced by different dimensions): CANNOT be resolved with Unbalanced functions. Use **Data Buffer Cell Loop** with nested loops.

##### Data Explosion - WHAT IT IS AND HOW TO AVOID IT

Data explosion occurs when a formula results in the massive creation of unwanted cells. It is triggered when a source script contains Dimensions not contained in the destination script.

**`#All` should NEVER be used** in destination scripts. In the worst case it causes data explosion, in the best case there is a better alternative.

##### RemoveZeros / RemoveNoData

- `RemoveZeros`: removes cells with amount 0 AND cells with Cell Status NoData.
- `RemoveNoData`: removes only cells with Cell Status NoData.
- **Should be used in ALL calculations as standard practice**.
- Only work with `api.Data.GetDataBufferUsingFormula` (not with `api.Data.GetDataBuffer`).
- Also work with `FilterMembers` and `RemoveMembers`.

##### Dimension Filters in ADC

Optional arguments to filter cells in the resulting Data Buffer. Positions: accountFilter, flowFilter, originFilter, icFilter, ud1Filter...ud8Filter.

**Member Filter Builder**: GUI tool to build correct filters.

**Dimension Filter rules**:
- Filters should contain only **Base Members** for best performance.
- Filters with **duplicate members** cause an error (Dictionary does not allow duplicate keys).
- Example: `A#IncomeStatement.Base`, `U1#Top.Base.Where(Name Contains Bug)`.

##### Collapsing Detail

Include dimensions in ALL operands of the formula to collapse detail:
- Origin, Flow, Intercompany are almost always collapsed.
- `O#Import` preserves the user's ability to adjust via BeforeAdj.
- `O#Forms` overwrites the data.
- **Never** have imported and calculated data at the same intersection.

##### Formula Variables

Declare a Data Buffer as a reusable variable:
```text
api.Data.FormulaVariables.SetDataBufferVariable("VarName", dataBuffer, True)
```
- Reference in formula string with `$VarName`.
- Last argument `True` for "Use Indexes To Optimize Repeat Filtering" (improves performance when reusing with FilterMembers).
- Improves performance by calling the Data Buffer to memory once and reusing multiple times.

##### Eval and Eval2

- **Eval**: evaluates individual cells of ONE Data Buffer within an ADC.
- **Eval2**: evaluates and compares cells of TWO Data Buffers.
- Used via subfunction (`OnEvalDataBuffer` / `OnEvalDataBuffer2`).
- `EventArgs` provides access to the Data Buffer, DestinationInfo, and ResultDataBuffer.
- Eval use case: filter cells by Cell Amount (e.g., remove > 500).
- Eval2 use case: compare Actual vs Budget to identify new products.

##### Data Buffer Cell Loop (DBCL) - ADVANCED TECHNIQUE

Manual, long-hand method of what ADC does automatically. Maximum control and flexibility over each individual cell.

**DBCL ingredients** (MEMORIZE):
1. New DataBuffer (empty Result Data Buffer)
2. DestinationInfo (equivalent to the left side of ADC)
3. GetDataBuffer or GetDataBufferUsingFormula (source Data Buffer)
4. For Each/Next loop
5. GetDataCell (get values from other cells)
6. New Result Cell (declare INSIDE the loop as `New`)
7. SetCell (add result cell to the Result Data Buffer)
8. SetDataBuffer (write to the Cube ONCE after the loop)

**Three forms of GetDataCell**:
1. **Member Names**: string with member names. Slowest (name > ID conversion).
2. **DataCellPk** (IDs): best performance, IDs already in memory.
3. **MemberScriptBuilder**: second best performance, converts source cell PK.

**Critical DBCL rules**:
- Result cell must be declared **INSIDE the loop** as `New`. If declared outside, only the first cell is added.
- `SetCell` with `AccumulateIfCellAlreadyExists = True` if there may be duplicate intersections.
- `SetDataBuffer` is called **ONCE** after the loop (NEVER inside).
- All dimensions of the result cell must be defined (via DestinationInfo or explicitly).

**When to use DBCL** (vs ADC):
- Transform dimensions (mapping from one dimension to another).
- Analyze Cell Status or Cell Amounts (conditional logic per cell).
- Nested loops for **double unbalanced** (both buffers unbalanced by different dimensions).
- Create multiple result cells within the same loop (combine calculations, better performance).
- When logic cannot be expressed with ADC.

**DBCL Performance guidelines**:
- Move constant lookups (Member IDs, global data cells) **OUTSIDE** the loop.
- **NEVER** use `api.Data.Calculate` inside the loop (multiple reads/writes to DB).
- **NEVER** use `api.Data.SetDataBuffer` or `api.Data.SetDataCell` inside the loop.
- Write to the Cube **ONCE** after the loop (wheelbarrow method).
- Use `Count` to verify the number of cells before looping.

##### Durable Data

- Storage Type = `DurableCalculation`.
- Not cleared during the DUCS (Clear Previously Calculated Data step).
- Activated with `isDurableData = True` in ADC or DBCL.
- Almost always used with Custom Calculate.
- Always include `api.Data.ClearCalculatedData` at the start (not cleared automatically).

##### ConvertDataBufferExtendedMembers

- Converts a Data Buffer from one Cube/Scenario to the dimensionality of another.
- Automatically adds data for Extended Members to create Parent Member cells.
- Useful for copying data between Scenarios with different levels of detail (Extensibility).

---

#### Calculation Management and Maintenance

##### Calculation Matrix

Centralized document to inventory all Calculations. Should be created from the Design & Requirements phase and maintained as a **living document** throughout the project.

**Basic fields**: Name, Category, Type (stored/dynamic), Finance Function Type, Location (BR/Member Formula), Execution, Formula (description), Dependencies, Scope (Data Unit + Account-level).

![Calculation Matrix - basic information](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p145-1536.png)

![Calculation Matrix - scope information](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p146-1540.png)

**Benefits**: tracking (reduce risk of forgetting critical calculations), design (iterative review with client), building/testing/approving (progress tracking), knowledge transfer (reference for administrators).

##### Code Comments

- **Header**: name, purpose, author, date, modifications (what changed and who).
- **Inline**: comment per line or block of 3-5 lines. Explain the WHY, not just the WHAT.
- Always err on the side of "over-commenting".

![Header comments - recommended structure](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p151-1573.png)

##### Regions

`#Region "RegionName"` / `#End Region` to organize code blocks in large Business Rules. Allows expanding/collapsing sections in the Business Rule Editor.

##### Maintenance Techniques

- **Time Functions**: NEVER hardcode time periods. Use `POVPriorYear`, `POVYear`, `POVPrior1`, `POVNext1`, `POVFirstInYear`, `POVLastInYear`. Prefer `T#POVLastInYear` and `T#POVFirstInYear` over `T#POVPriorYearM12` (they work in Scenarios of any frequency).

![Time Functions available in Member Filter Builder](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p153-1591.png)

- **Alternative Hierarchies**: create grouping Parents to support Calculations and reduce code maintenance. Create under `AlternateHierarchies` (sibling of `Top`) to avoid double-counting.

- **Text Properties**: use member Text1-Text8 properties with `Where` clauses in Member Filters. Example: `U1#Top.Base.Where(Text2 = SalesCalculation)`.

- **Custom SQL Tables**: store Calculation logic/inputs in SQL tables, reference with `api.Functions.GetCustomBlendDataTable`. Maintenance is reduced to changing table fields or adding rows.

![Custom SQL Table - create in Dashboard SQL Table Editor](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch07-p157-1621.png)

##### Application Reports

- **Formula Statistics**: breakdown of members with Member Formulas/Dynamic Calcs by dimension.
- **Formula List**: each Member Formula with its syntax.

---

#### Troubleshooting and Diagnostics

##### Task Activity

Starting point to verify if a Calculation/Consolidation completed successfully. Shows the status of all server tasks with filtering by Task Type.

![Task Activity - access from top-right menu](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p161-1648.png)

##### Logging

The most important tool for debugging. Allows writing custom messages to the Error Log.

**Two logging functions**:
- `api.LogMessage` - available in Finance Business Rules (BEST performance).
- `BRApi.ErrorLog.LogMessage` - available in ALL rule types (opens new DB connection, WORST performance).
- **ALWAYS use `api.LogMessage` in Finance Rules**.

**LogDataBuffer**: Function to view the contents of a Data Buffer in the Error Log. Extremely useful for diagnosing dimension mismatch issues.

##### Stopwatch

To identify which part of the code takes the longest. Import `System.Diagnostics`, create instance with `StartNew`, log elapsed time.

##### Calculate With Logging

Enables detailed drill down into each DUCS step with duration per step. Allows identifying the exact Calculation causing slowness.

![Calculate With Logging - activate from Data Management](/Users/aurelio.santos/Desktop/OneStreamDoc/output/finance-rules/images/finance-rules-ch08-p170-1721.png)

**NOTE**: Adds significant processing time. Only use for diagnostics.

##### Common Errors (FOR THE EXAM)

| Error | Cause | Solution |
|-------|-------|----------|
| **Calculation does not produce results** | Data Buffers do not match dimensionally | LogDataBuffer to compare Common Members |
| **Inconsistent results** | Incorrect Formula Pass (unresolved dependency) | Change to FormulaPass 16 to isolate; review dependencies |
| **Compilation Error** | Typo in VB.NET syntax | Correct and compile again |
| **Invalid Formula Script** | Typo within the ADC formula string | Review member names (runtime error) |
| **Unbalanced Buffer** | Data Buffers with different Common Members | Use Unbalanced functions |
| **Data Explosion warning** | Source script has dimensions not in destination | Never use `#All`; include dimension in destination |
| **Result cell only 1 record** | New result cell declared outside the loop | Move `New` INSIDE the loop |
| **Duplicate Members in Filter** | Filter contains duplicate members (Parent + Child) | Verify no overlap |
| **Undefined Members** | DestinationInfo empty and result cell does not inherit from source | Define all dimensions explicitly |
| **Object Not Set** | Uninitialized variable used in function | Initialize variable before use |
| **Given Key Not Present** | Custom Calculate parameter not defined in DM step | Define parameter in DM step; use `XFGetValue` with default |
| **Invalid Destination Script** | Data Unit Dimension on left side of ADC | Remove; use If statements to filter Data Unit |

---

#### Calculation Performance

##### Factors that affect performance:
1. **Hardware and Server Settings**: CPU specs (3.7 GHz up to 2x faster than 2.0 GHz), multi-threading settings in Application Server Config.
2. **Cube Design**: Data Unit Size and Volume, Extensibility, number of Entities, Entity hierarchies, unnecessary data in the Cube.
3. **Formula Efficiency**: efficient code, elimination of unnecessary processing.

##### Typical Server Structure:
- **General Application Server**: navigation, Cube Views, Dashboards (single-threaded, not CPU-intensive).
- **Stage Application Server**: Stage Engine (multi-threaded, CPU-intensive).
- **Consolidation Application Server**: Finance Engine (multi-threaded, CPU-intensive).
- **Data Management Server**: DM sequences, long-running tasks.

##### Cube Design for Performance:
- **Reduce Data Unit Size**: use Extensibility, increase number of Entities, eliminate unnecessary data.
- **Optimize Entity Hierarchies**: avoid flat structures with many children; avoid 1-to-1 relationships.
- **Do not store transactional data in Cubes**: use Specialty Planning or BI Blend.
- **Align Entity Dimensions with Calculations by Scenario Type**: if Actuals calculate by Legal Entity and Planning calculates by Department, consider different Entity Dimensions.

##### Things to DO (Best Practices - MEMORIZE):
1. Use **Custom Calculate** when possible (narrower scope).
2. Align **Entity Dimensions** with Calculations by Scenario Type.
3. Use **Dynamic Calculations** instead of stored when possible (and vice versa when reports are slow).
4. Use **RemoveZeros** on ALL Data Buffers.
5. **Limit Data Unit Scope** with If statements (`api.Entity.IsBaseEntity AndAlso api.Cons.IsLocalCurrencyForEntity`).
6. **Limit Account-level Dimension Scope** with Dimension Filters.
7. Use **Global Variables** (`globals.SetObject`/`GetObject`) for variables that don't change between Data Units.
8. Use **Formula Variables** to reuse Data Buffers across multiple ADC functions.
9. Use **DimConstants** instead of string comparisons for default members.
10. Use **C#Aggregated** when possible (up to 90% faster).
11. Do NOT use Force Consolidate/Calculate unnecessarily.
12. Use `api.LogMessage` instead of `BRApi.ErrorLog.LogMessage` in Finance Rules.
13. Use **Time Functions** (never hardcode periods).

![Data Unit If condition - best practice](/Users/aurelio.santos/Desktop/OneStreamDoc/output/foundation-handbook/images/foundation-handbook-ch08-p248-2500.png)

##### Things to AVOID (MEMORIZE):
1. **api.Data.Calculate inside loops**: causes multiple reads/writes to the database. Use ADC with filters or DBCL.
2. **api.Data.SetCell/SetDataCell inside loops**: use "wheelbarrow method" (accumulate in Result Data Buffer, write once).
3. **api.Data.ClearCalculatedData inside loops**: clear BEFORE the loop.
4. **Constant lookups inside loops**: move outside the loop.
5. **api.Data.ClearCalculatedData in DUCS**: unnecessary, it is already the first step of the DUCS.
6. **Stacking ADC functions with similar logic**: condense into one with filters.
7. **BRApi calls in Finance Rules**: open new database connections, cause overload in multi-threading. Use API equivalents.
8. **Hardcode time periods**: use Time Functions.
9. **Forget to comment out/remove logging in production**: can crash servers.
10. **Copy data in the DUCS**: use Custom Calculate with isDurable = True.
11. **Transactional data in Cubes**: use Specialty Planning or BI Blend.
12. **Force Consolidate unnecessarily**: processes Data Units that haven't changed.

##### System Diagnostics (MarketPlace)
- Install in each environment.
- **Application Analysis**: design, size, and formula efficiency metrics.
- **Data Volume Statistics**: Data Units with high record counts.
- **Long Running Formulas Report**: identifies slow Calculations in the DUCS.

---

#### Common Calculation Examples

##### Balance Sheet Calculations

**Current Year Net Income**:
```text
api.Data.Calculate("A#CurYearNetIncome:O#Import:F#EndBal:I#None:U1#None:U2#None =
V#YTD:A#NetIncome:O#Top:F#EndBal:I#Top:U1#Top:U2#Top")
```
- Formula Pass 2 (after any Income Statement calculation in Pass 1).
- Is Consolidated = **True**.
- Allow Input = **False**.
- Collapses detail of Origin, Flow, IC, UD1, UD2.
- Scope: Base Entities (do not filter to Local only - must capture translated amounts).

**Retained Earnings Beginning Balance**:
- Copy ending balance from the prior year.
- Use `api.Time.IsFirstPeriodInYear` to only reference prior year once (better performance).
- For subsequent periods, carry forward from the prior period.
- Formula Pass 1 (no dependencies).
- Is Consolidated = **True**.
- Allow Input = **False**.

##### Flow Calculations

**Beginning Balance (BegBalCalcYTD)**:
- Pull ending balance from prior year in first period.
- Carry forward for subsequent periods.
- Filter to Balance Sheet Accounts only.

**BegBalDynamic**:
- Dynamic Calc that shows the correct balance based on View Member (MTD, QTD, YTD).
- Formula Type and Account Type = **DynamicCalc**.
- Solves the problem that BegBalCalcYTD always shows YTD.

**ActivityCalc**:
- YTD change in the Account from the beginning balance: EndBal - BegBal.
- Switch Type = **True** (so that MTD/QTD work correctly).
- Aggregation Weight = 0 for Beginning Balance and Activity (only EndBal aggregates to the Top Member).

##### FX Calculations (only for foreign currency Entities)

- **FXOpen**: effect of rate change on the opening balance.
- **FXMovement**: FX effect on Account activity (current movement, prior movement, override movement).
- **CTA (Cumulative Translation Adjustment)**: sum of FX from all Balance Sheet Accounts. Reported in OCI.

---

## Critical Points to Memorize

### Business Rule types and their use cases:
- **Finance**: financial calculations, assigned to the Cube (up to 8). ONLY one with access to Custom Calculate, Translate, Share, Elimination.
- **Parser**: parse data during import. Assigned to Data Source Dimension.
- **Connector**: integration with external systems + drill back. Assigned to Connector Data Source. 4 ConnectorActionTypes.
- **Smart Integration Function**: remote functions via Smart Integration Connector (without VPN).
- **Conditional**: conditional mapping in transformations. Very performance intensive.
- **Derivative**: derive records in Stage (interim/final) + validation check rules.
- **Cube View Extender**: PDF formatting of Cube Views **ONLY**.
- **Dashboard Dataset**: custom datasets. Used in Data Adapters and Bound List Parameters.
- **Dashboard Extender**: 3 function types (LoadDashboard, ComponentSelectionChanged, SQLTableEditorSaveData). Main dashboard only (not nested).
- **Dashboard XFBR String**: returns text based on logic, best starting point for beginners.
- **Extensibility Extender**: automated tasks, called from Data Management steps.
- **Extensibility Event Handler**: 7 types, automatically activated by events. **ONLY** type that does not need to be called.
- **Spreadsheet**: read/write of tables in the Spreadsheet tool.

### Finance Function Types:
- **Calculate**: within the DUCS, `C#Local`.
- **Custom Calculate**: outside the DUCS, only via DM step, requires `isDurable` + `ClearCalculatedData`.
- **DataCell/DynamicCalc**: in memory, for reports, cell by cell.
- **C#Aggregated**: up to 90% faster than Consolidation, without IC eliminations.
- **Translate/ConsolidateShare/ConsolidateElimination**: for custom Translation and Consolidation logic.

### DUCS (Data Unit Calculation Sequence):
- All or nothing; 8 Business Rules interleaved with 16 Formula Passes.
- Can execute up to **7+ times** per Entity during Consolidation (Local, Translated, Share, Elimination, etc.).
- Member Formulas are **multithreaded** within each pass.
- Business Rules execute **sequentially** as written.
- First step: Clear Previously Calculated Data (does not clear Durable Data).
- Formula Pass order within each pass: Account > Flow > UD1 > UD2 > ... > UD8.

### Data Buffers:
- Subset of cells within a Data Unit.
- Arithmetic operations only between **balanced** Data Buffers (same Common Members).
- **RemoveZeros** always.
- Unbalanced functions: second argument = Data Buffer with more dimensions.
- **NEVER** use `#All` in destination scripts.
- Double-unbalanced: use DBCL with nested loops.

### ADC vs DBCL:
- **ADC**: simpler, automatic, ideal for most calculations.
- **DBCL**: more flexible, cell-by-cell control, necessary for dimension transformations, Cell Status analysis, nested loops.
- **Performance**: one DBCL can replace multiple ADC (single write vs multiple).

### Assemblies:
- Assembly Business Rules: 1-to-1 transition, no Finance Rules, no Dynamic content.
- Assembly Services: include Finance, Dynamic content. Use Service Factory.
- Compiler Language is defined at the Assembly level (VB.NET or C#).

### Performance Best Practices:
- Consolidation = DUCS + Member Formulas.
- Planning = Custom Calculate + `C#Aggregated` + Business Rules.
- NEVER `api.Data.Calculate` inside loops.
- NEVER write to the Cube inside loops.
- ALWAYS limit Data Unit scope with If statements.
- ALWAYS use RemoveZeros.
- ALWAYS use Time Functions (do not hardcode periods).
- Use Global Variables for expensive objects that don't change between Data Units.
- Use `api.LogMessage` (not `BRApi.ErrorLog.LogMessage`) in Finance Rules.

---

## Source Mapping

| Objective | Book/Chapter |
|-----------|-------------|
| 201.8.1 (Business Rule types) | Foundation Handbook - Chapter 8: Rules and Calculations (rule types, use cases) |
| 201.8.1 (Business Rule types) | Finance Rules - Chapter 1: Introduction + Finance Engine Basics (Finance Rules, Member Formulas, Engines) |
| 201.8.1 (Business Rule types) | Workspaces & Assemblies - Chapter 5: Understanding Assemblies |
| 201.8.1 (Business Rule types) | Workspaces & Assemblies - Chapter 6: Managing Assembly Business Rules |
| 201.8.1 (Business Rule types) | Design Reference Guide - Chapter 8: Cubes (Business Rules section) |
| 201.8.2 (Function types) | Finance Rules - Chapter 1: Finance Engine Basics (DUCS, Finance Function Types, Custom Calculate, C#Aggregated) |
| 201.8.2 (Function types) | Finance Rules - Chapter 2: Cube Data (Data Cells, Data Units, Data Buffers, Storage Types) |
| 201.8.2 (Function types) | Finance Rules - Chapter 3: api.Data.Calculate (ADC, Data Buffer Math, Unbalanced, Filters, Eval, Durable Data) |
| 201.8.2 (Function types) | Finance Rules - Chapter 4: Data Buffer Cell Loop (DBCL, GetDataCell, SetDataBuffer) |
| 201.8.2 (Function types) | Finance Rules - Chapter 5: Reporting Calculations (Dynamic Calcs, GetDataCell, DataCell BR, UD8, Relational Blending) |
| 201.8.2 (Function types) | Finance Rules - Chapter 6: Managing Calculations (Calculation Matrix, Comments, Maintenance, SQL Tables) |
| 201.8.2 (Function types) | Finance Rules - Chapter 7: Troubleshooting and Performance (Logging, Errors, Performance best practices) |
| 201.8.2 (Function types) | Finance Rules - Chapter 8: Common Rule Examples (Balance Sheet, Flow, Retained Earnings, CTA, EPU) |
| 201.8.2 (Function types) | Foundation Handbook - Chapter 8: Rules and Calculations (DUCS detail, api.Data.Calculate overloads, best practices) |
