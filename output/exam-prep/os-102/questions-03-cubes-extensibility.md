# Question Bank - Section 3: Cubes and Extensibility (12% of exam)

## Objectives
- **102.3.1:** Understand Dimension Extensibility and how it applies to Cubes
- **102.3.2:** Describe the usage and configuration of extensibility features
- **102.3.3:** Configure Cubes and understand the impact of Cube settings

---

## Objective 102.3.1: Dimension Extensibility

### Question 1 (Easy)
**102.3.1** | Difficulty: Easy

What is Dimension Extensibility in OneStream?

A) The ability to add unlimited members to any dimension
B) The ability to extend the standard dimension model by adding User Defined (UD) dimensions to a Cube
C) The ability to create custom calculations within dimensions
D) The ability to share dimensions across multiple applications

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Dimension Extensibility refers to the ability to extend the standard dimension model by adding User Defined (UD) dimensions to a Cube. OneStream provides up to 8 UD dimensions (UD1 through UD8) that can be configured to represent additional analytical dimensions beyond the standard set (Entity, Account, Scenario, Time, Flow, Intercompany, Consolidation).
</details>

---

### Question 2 (Medium)
**102.3.1** | Difficulty: Medium

How many User Defined (UD) dimensions can be added to a single Cube in OneStream?

A) 4
B) 6
C) 8
D) Unlimited

<details>
<summary>Show answer</summary>

**Correct answer: C)**

OneStream supports up to 8 User Defined dimensions (UD1 through UD8) per Cube. These dimensions can be named and configured to suit the application's analytical requirements, such as Product, Region, Channel, Project, or any other categorization needed.
</details>

---

### Question 3 (Medium)
**102.3.1** | Difficulty: Medium

An Administrator needs to track data by both Product and Region in addition to the standard dimensions. What is the correct approach?

A) Create sub-accounts for each Product-Region combination
B) Configure two User Defined dimensions (e.g., UD1 for Product, UD2 for Region) and assign them to the Cube
C) Create separate Cubes for each Product and Region
D) Use the Flow dimension to represent Products and Regions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach is to use User Defined dimensions. The Administrator would configure UD1 as "Product" and UD2 as "Region," populate each with the appropriate members, and assign them to the Cube. This maintains the multidimensional model and allows for proper aggregation and reporting across these additional analytical dimensions.
</details>

---

## Objective 102.3.2: Extensibility usage and configuration

### Question 4 (Easy)
**102.3.2** | Difficulty: Easy

Where are User Defined dimension members created and maintained?

A) In the Cube Properties panel
B) In the Dimension Library under the Application tab
C) In the Workflow Configuration
D) In the Dashboard Designer

<details>
<summary>Show answer</summary>

**Correct answer: B)**

User Defined dimension members are created and maintained in the Dimension Library under the Application tab, just like the standard dimensions. Each UD dimension has its own hierarchy of members that can be managed from this central location.
</details>

---

### Question 5 (Medium)
**102.3.2** | Difficulty: Medium

When a UD dimension is enabled on a Cube, what must the Administrator also configure to ensure data can be entered against the new dimension?

A) Only the dimension members need to be created
B) The dimension members must be created, the Cube must be configured to include the UD dimension, and Workflow profiles must reference the UD dimension
C) Only the Workflow profile needs to be updated
D) The dimension is automatically available for data entry once enabled

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Enabling a UD dimension requires multiple steps: creating the dimension members in the Dimension Library, configuring the Cube to include the UD dimension, and updating Workflow profiles to reference the UD dimension members for data entry. Simply enabling the dimension at one level is not sufficient for end-to-end functionality.
</details>

---

### Question 6 (Hard)
**102.3.2** | Difficulty: Hard

An Administrator has configured UD1 as "Department" on Cube1 and wants the same UD1 dimension to represent "Project" on Cube2. Is this possible, and how?

A) No, UD dimensions must have the same name and members across all Cubes
B) Yes, each Cube can independently assign a different label and member set to the same UD slot
C) Yes, but only if the Cubes are in different applications
D) No, each UD dimension can only be used once across all Cubes

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In OneStream, each Cube can independently configure its UD dimensions. The same UD slot (e.g., UD1) can represent different concepts in different Cubes. Cube1 can use UD1 as "Department" while Cube2 uses UD1 as "Project," each with its own member set. This flexibility allows different Cubes to model different analytical needs within the same application.
</details>

---

## Objective 102.3.3: Cube configurations and impact

### Question 7 (Easy)
**102.3.3** | Difficulty: Easy

What is a Cube in OneStream?

A) A visual dashboard component
B) A multidimensional data storage structure that holds financial and operational data
C) A report template used for output
D) A security role assigned to users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Cube in OneStream is a multidimensional data storage structure (also known as a data model) that holds financial and operational data. It is defined by its dimensions (Entity, Account, Scenario, Time, Flow, Intercompany, Consolidation, and optionally UD1-UD8) and is where data is loaded, calculated, and retrieved for reporting.
</details>

---

### Question 8 (Medium)
**102.3.3** | Difficulty: Medium

An Administrator creates a new Cube in the application. Which of the following must be done before data can be loaded into the new Cube?

A) Assign dimensions to the Cube, create Workflow profiles referencing the Cube, and configure data load rules
B) Only assign dimensions and the Cube is ready for data
C) Only create Workflow profiles
D) Nothing additional; the Cube is ready to use immediately

<details>
<summary>Show answer</summary>

**Correct answer: A)**

Before data can be loaded into a new Cube, the Administrator must assign the appropriate dimensions (including any UD dimensions), create or update Workflow profiles to reference the new Cube (defining which entities and scenarios map to it), and configure data load rules or import processes. All three steps are necessary for a functional Cube.
</details>

---

### Question 9 (Hard)
**102.3.3** | Difficulty: Hard

An Administrator is deciding between creating one large Cube with multiple UD dimensions versus two smaller Cubes with fewer dimensions. What is a key consideration for this decision?

A) OneStream only allows one Cube per application
B) More dimensions in a single Cube increase the data intersection density, which can impact storage and performance; separate Cubes can reduce sparsity
C) Smaller Cubes cannot share dimensions with each other
D) The number of Cubes has no impact on performance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A key design consideration is data density (sparsity). Adding more dimensions to a single Cube increases the total number of potential data intersections, many of which may be empty (sparse). This can impact storage requirements and query performance. Using separate Cubes with fewer dimensions reduces sparsity by keeping each Cube focused on related data. OneStream allows multiple Cubes per application and Cubes can share dimensions from the Dimension Library.
</details>

---

### Question 10 (Medium)
**102.3.3** | Difficulty: Medium

Which Cube setting controls whether data aggregation occurs in real-time or must be explicitly triggered by a calculation process?

A) Cube Type
B) Data Storage Type
C) Aggregation Method (Dynamic vs. Stored)
D) Cube Security Level

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Aggregation Method determines whether parent-level data is calculated dynamically when queried (Dynamic) or pre-calculated and stored (Stored). Dynamic aggregation provides always-current results but may impact query performance for large data sets. Stored aggregation requires explicit calculation processes but provides faster query response times.
</details>

---

### Question 11 (Easy)
**102.3.1** | Difficulty: Easy

What naming convention is used for User Defined dimensions in OneStream?

A) Custom1 through Custom8
B) UD1 through UD8
C) Dim1 through Dim8
D) Extra1 through Extra8

<details>
<summary>Show answer</summary>

**Correct answer: B)**

User Defined dimensions in OneStream follow the naming convention UD1 through UD8. While administrators can assign descriptive labels (such as "Product" or "Region") for display purposes, the underlying system identifiers remain UD1 through UD8.
</details>

---

### Question 12 (Medium)
**102.3.2** | Difficulty: Medium

An Administrator wants to use a UD dimension to track "Cost Center" in one Cube and leave it unused in another Cube. Is this configuration possible?

A) No, if a UD dimension is configured it must be used in all Cubes
B) Yes, UD dimensions are assigned per Cube and a dimension can be enabled on one Cube while remaining unused on another
C) No, UD dimensions are global and cannot be Cube-specific
D) Yes, but only if the unused Cube is a reporting-only Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

UD dimensions are assigned at the Cube level. An Administrator can enable UD1 as "Cost Center" on Cube1 while leaving UD1 unused on Cube2. Each Cube independently determines which UD dimensions it uses, providing flexibility to match each Cube's analytical requirements without forcing unnecessary dimensionality on other Cubes.
</details>

---

### Question 13 (Hard)
**102.3.3** | Difficulty: Hard

An Administrator notices that query performance has degraded significantly on a Cube with 6 UD dimensions. What is the most likely cause and recommended action?

A) The server needs more RAM; no design changes are needed
B) The high number of dimensions has created excessive sparsity (many empty intersections), and the Administrator should evaluate whether some UD dimensions can be consolidated or moved to a separate Cube
C) UD dimensions do not affect performance
D) The Administrator should switch all dimensions to Dynamic Calc to improve speed

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Each additional dimension multiplies the total number of potential data intersections in the Cube. With 6 UD dimensions, the Cube may have a very high number of intersections, most of which are empty (sparse). This sparsity impacts storage, memory, and query performance. The recommended action is to evaluate the Cube design and consider consolidating related dimensions or splitting the Cube into smaller, more focused Cubes with fewer dimensions.
</details>

---

### Question 14 (Easy)
**102.3.3** | Difficulty: Easy

How many Cubes can an Administrator create within a single OneStream application?

A) Only 1
B) Up to 3
C) Up to 8
D) Multiple Cubes with no fixed limit

<details>
<summary>Show answer</summary>

**Correct answer: D)**

OneStream allows multiple Cubes within a single application with no fixed upper limit. Administrators can create as many Cubes as needed to model different data sets, such as separate Cubes for financial consolidation, planning, and operational reporting. Each Cube can have its own dimension configuration.
</details>

---

### Question 15 (Medium)
**102.3.2** | Difficulty: Medium

What happens when an Administrator removes a UD dimension from a Cube that already contains data intersecting with that dimension?

A) The data is automatically preserved in a backup table
B) Data stored at intersections involving that UD dimension is lost and cannot be recovered without a prior backup
C) The UD dimension cannot be removed if data exists
D) The data is moved to the default UD member automatically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Removing a UD dimension from a Cube that contains data at intersections involving that dimension results in data loss for those intersections. The data is not automatically backed up or migrated. Administrators must ensure data is backed up or migrated before making structural changes to the Cube's dimension configuration.
</details>

---

### Question 16 (Medium)
**102.3.3** | Difficulty: Medium

Which Cube property determines the default member used when a dimension intersection is not explicitly specified during data retrieval?

A) Cube Security settings
B) The Default Member property configured for each dimension on the Cube
C) The first member in the dimension hierarchy alphabetically
D) The root member is always used by default

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Each dimension assigned to a Cube has a Default Member property. This default is used when a query or process does not explicitly specify a member for that dimension. Setting appropriate defaults ensures predictable behavior when users or processes access Cube data without specifying all dimension intersections.
</details>

---

### Question 17 (Hard)
**102.3.2** | Difficulty: Hard

An Administrator needs to create a Cube that stores headcount data by Department and Location in addition to the standard dimensions. The headcount values are always integer numbers and should not be currency-translated. What is the correct configuration approach?

A) Add headcount as an Account member in an existing financial Cube
B) Create a separate Cube with UD1 as Department and UD2 as Location, configure it as a non-financial (operational) Cube type, and disable currency translation
C) Store headcount in the Flow dimension
D) Use the Intercompany dimension for Department and Location

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach is to create a dedicated Cube for headcount data. Configuring UD1 and UD2 for Department and Location respectively, and setting the Cube type to non-financial (or operational) ensures that currency translation is not applied to integer headcount values. This separation keeps the financial Cube clean and ensures appropriate data handling for operational metrics.
</details>

---

### Question 18 (Easy)
**102.3.1** | Difficulty: Easy

Which standard dimensions are included in every OneStream Cube by default?

A) Only Entity and Account
B) Entity, Account, Scenario, Time, Flow, Intercompany, and Consolidation
C) Entity, Account, and UD1 through UD8
D) Only Entity, Account, and Time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Every OneStream Cube includes seven standard dimensions by default: Entity, Account, Scenario, Time, Flow, Intercompany, and Consolidation. These are system dimensions that provide the foundation for financial data modeling. UD dimensions (UD1 through UD8) are optional and added as needed.
</details>

---

### Question 19 (Medium)
**102.3.3** | Difficulty: Medium

An Administrator creates a second Cube in the application. Can both Cubes share the same Entity dimension members?

A) No, each Cube must have entirely separate dimension members
B) Yes, Cubes share dimensions from the Dimension Library, so both can reference the same Entity members
C) Yes, but only if they are in different Workflow Profiles
D) No, Entity members can only belong to one Cube

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cubes in OneStream share dimensions from the centralized Dimension Library. The Entity dimension (and other standard dimensions) can be referenced by multiple Cubes, meaning both Cubes can use the same Entity members. This ensures consistency across Cubes and eliminates the need to maintain duplicate dimension hierarchies.
</details>

---

### Question 20 (Hard)
**102.3.3** | Difficulty: Hard

An Administrator is configuring a Cube and must decide between setting the Account dimension to "Dense" or "Sparse" storage. What is the key factor in this decision?

A) Dense dimensions are always faster, so Account should always be Dense
B) Dense storage is suitable for dimensions where most intersections contain data; Sparse storage is better for dimensions with many empty intersections to optimize memory usage
C) There is no difference between Dense and Sparse in OneStream
D) Sparse dimensions cannot be used for Account

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The decision between Dense and Sparse storage depends on data population patterns. Dense storage is appropriate for dimensions where most member combinations contain data values (high fill rate), while Sparse storage is designed for dimensions where many intersections are empty. For the Account dimension, if most entities report against most accounts, Dense storage is efficient. If many accounts apply only to a few entities, Sparse may be more appropriate. This choice impacts memory consumption and calculation performance.
</details>
