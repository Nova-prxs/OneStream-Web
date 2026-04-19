# Question Bank - Section 1: Building Blocks of Administration (11% of exam)

## Objectives
- **102.1.1:** Identify how to find documentation and solutions in OneStream resources
- **102.1.2:** Understand the Solution Exchange and One Community for knowledge sharing
- **102.1.3:** Describe how to extract and deploy applications

---

## Objective 102.1.1: Finding documentation and solutions

### Question 1 (Easy)
**102.1.1** | Difficulty: Easy

Which OneStream resource provides a searchable library of pre-built solutions, templates, and MarketPlace applications?

A) OneStream Navigator
B) Solution Exchange
C) OneStream Developer Portal
D) Application Server Console

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Solution Exchange is OneStream's primary repository for pre-built solutions, templates, and MarketPlace applications that administrators can download and deploy. The Navigator is a client interface, the Developer Portal focuses on extensibility documentation, and the Application Server Console is for server management.
</details>

---

### Question 2 (Easy)
**102.1.1** | Difficulty: Easy

Where can an Administrator access product documentation, training materials, and submit support cases for OneStream?

A) Application Properties panel
B) Solution Exchange only
C) One Community (community.onestream.com)
D) Windows Event Viewer

<details>
<summary>Show answer</summary>

**Correct answer: C)**

One Community (community.onestream.com) is the centralized portal where administrators can access product documentation, training materials, knowledge base articles, and submit support cases. Solution Exchange is specifically for downloading solutions. Application Properties and Event Viewer serve different purposes.
</details>

---

### Question 3 (Medium)
**102.1.2** | Difficulty: Medium

An Administrator needs to find a pre-built financial close solution that includes dashboards and business rules. Which is the most efficient first step?

A) Build the solution from scratch using the Design Studio
B) Search the Solution Exchange for a MarketPlace solution
C) Post a request on ServiceNow
D) Export an application from another OneStream environment

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Solution Exchange contains MarketPlace solutions that are pre-built, tested, and ready to deploy. This is the most efficient starting point before building from scratch. ServiceNow is for support tickets, not solution discovery. Exporting from another environment assumes one already exists.
</details>

---

### Question 4 (Medium)
**102.1.2** | Difficulty: Medium

Which category of items can be found on the OneStream Solution Exchange?

A) Only XF MarketPlace solutions
B) Only Dashboard templates
C) MarketPlace solutions, Starter Kits, and example applications
D) Only business rule code samples

<details>
<summary>Show answer</summary>

**Correct answer: C)**

The Solution Exchange hosts a variety of content including full MarketPlace solutions (like People Planning, Account Reconciliations), Starter Kits that provide foundational configurations, and example applications. It is not limited to a single category of content.
</details>

---

### Question 5 (Easy)
**102.1.3** | Difficulty: Easy

What file format is used when extracting a OneStream application for deployment to another environment?

A) .csv
B) .xml
C) .zip
D) .xfapp

<details>
<summary>Show answer</summary>

**Correct answer: C)**

OneStream applications are extracted as .zip files that contain all the application components including metadata, business rules, dashboards, and configuration settings. This zip package can then be loaded into another environment.
</details>

---

### Question 6 (Medium)
**102.1.3** | Difficulty: Medium

When extracting an application from OneStream, which of the following is included in the extraction package?

A) Only dimension members and hierarchies
B) Only business rules and dashboards
C) Application metadata, dimensions, business rules, dashboards, and configuration settings
D) Only data stored in the Cube

<details>
<summary>Show answer</summary>

**Correct answer: C)**

An application extraction includes the full application metadata: dimensions and their members, business rules, dashboards, workflow profiles, security settings, and configuration. Actual data stored in the Cube is not included in the extraction package; data must be migrated separately.
</details>

---

### Question 7 (Hard)
**102.1.3** | Difficulty: Hard

An Administrator extracts an application from a Development environment and needs to load it into a Production environment. Before loading, the Administrator notices the Production environment has a different server configuration. Which statement is correct?

A) The application cannot be loaded if server configurations differ
B) The extraction file must be manually edited to match the Production server settings
C) Application extractions are environment-independent; server-specific settings are configured separately after loading
D) The Administrator must first replicate the Development server configuration in Production

<details>
<summary>Show answer</summary>

**Correct answer: C)**

OneStream application extractions are designed to be environment-independent. The extraction contains application-level metadata and configuration, not server-specific settings. After loading the application into Production, the Administrator configures environment-specific settings such as database connections and server parameters separately.
</details>

---

### Question 8 (Medium)
**102.1.1** | Difficulty: Medium

An Administrator encounters an error during a data load process and needs to find a resolution. What is the recommended order of troubleshooting resources?

A) Contact OneStream Support first, then check documentation
B) Check the OneStream Knowledge Base on One Community, review product documentation, then contact Support if needed
C) Post the error on a public forum, then check the Solution Exchange
D) Restart the application server, then check the error log

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended troubleshooting approach is to first check the Knowledge Base on One Community for known issues and resolutions, then review the product documentation for correct configuration, and finally contact OneStream Support if the issue persists. This follows best practices for efficient issue resolution.
</details>

---

### Question 9 (Hard)
**102.1.3** | Difficulty: Hard

An Administrator wants to deploy only specific components (such as a single dashboard and its associated business rules) from one environment to another, rather than the entire application. Which approach should the Administrator use?

A) Extract the full application and delete unwanted components after loading
B) Use the Component Extract feature to selectively extract specific items
C) Manually recreate the components in the target environment
D) Copy the database tables directly between environments

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream provides a Component Extract feature that allows administrators to selectively extract specific components such as individual dashboards, business rules, or dimension configurations. This is more efficient and safer than extracting the entire application when only specific components need to be migrated.
</details>

---

### Question 10 (Easy)
**102.1.2** | Difficulty: Easy

What is the primary purpose of OneStream's One Community platform?

A) To host OneStream application servers in the cloud
B) To provide a collaboration platform for knowledge sharing, documentation, and support
C) To serve as the primary data storage for OneStream applications
D) To manage user licenses and subscriptions

<details>
<summary>Show answer</summary>

**Correct answer: B)**

One Community is OneStream's collaboration platform where users and administrators can access documentation, knowledge base articles, training resources, discussion forums, and submit support cases. It serves as the central hub for the OneStream user community.
</details>

---

### Question 11 (Easy)
**102.1.1** | Difficulty: Easy

Which type of OneStream resource should an Administrator consult first when learning how to configure a new feature?

A) Third-party blog posts
B) OneStream product documentation on One Community
C) The Windows Server event log
D) The SQL Server management console

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream product documentation available on One Community is the authoritative source for learning how to configure features. It contains step-by-step guides, configuration references, and best practices maintained by OneStream. Third-party resources may be outdated or inaccurate, and system-level tools do not provide application configuration guidance.
</details>

---

### Question 12 (Medium)
**102.1.2** | Difficulty: Medium

An Administrator downloads a MarketPlace solution from the Solution Exchange. What should the Administrator do before deploying it to a Production environment?

A) Deploy it directly to Production to save time
B) Deploy it to a Development or Test environment first, validate its functionality, and then promote to Production
C) Email the solution file to all users for review
D) Rename all the components in the solution to match company naming standards before any testing

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Best practice dictates that any solution downloaded from the Solution Exchange should first be deployed to a non-production environment (Development or Test) for validation and configuration. This allows the Administrator to verify functionality, adjust settings, and resolve any conflicts before impacting the Production environment.
</details>

---

### Question 13 (Medium)
**102.1.3** | Difficulty: Medium

When loading an application extraction into a target environment, what happens if dimension members in the extraction conflict with existing members in the target?

A) The target environment members are automatically deleted and replaced
B) The load fails entirely with no changes applied
C) The Administrator is presented with conflict resolution options to merge, skip, or overwrite components
D) Conflicting members are renamed with a numeric suffix

<details>
<summary>Show answer</summary>

**Correct answer: C)**

When loading an application extraction that contains components conflicting with existing ones in the target environment, OneStream provides conflict resolution options. The Administrator can choose to merge new items with existing ones, skip components that already exist, or overwrite them. This gives the Administrator control over how the target environment is updated.
</details>

---

### Question 14 (Hard)
**102.1.3** | Difficulty: Hard

An Administrator needs to migrate an application between two environments that are running different versions of OneStream. Which statement is correct?

A) Application extractions are always compatible regardless of version differences
B) The target environment must be running the same or a newer version of OneStream than the source environment for the extraction to load successfully
C) The source environment must be running a newer version than the target
D) Version differences have no impact because extractions only contain data, not metadata

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Application extractions are forward-compatible but not backward-compatible. The target environment must be running the same version or a newer version of OneStream than the source environment. Loading an extraction from a newer version into an older version may fail due to unsupported features or metadata schema differences. This is why version alignment is an important consideration during migration planning.
</details>

---

### Question 15 (Easy)
**102.1.2** | Difficulty: Easy

What type of content can an Administrator find in the Knowledge Base on One Community?

A) Only video tutorials
B) Articles covering known issues, troubleshooting steps, configuration tips, and best practices
C) Only release notes for the latest version
D) Only billing and licensing information

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Knowledge Base on One Community contains a wide range of articles including known issues with workarounds, troubleshooting guides, configuration tips, best practices, and how-to articles. It is continuously updated and serves as a primary self-service resource for administrators and users.
</details>

---

### Question 16 (Medium)
**102.1.1** | Difficulty: Medium

An Administrator needs to submit a support case to OneStream. Which information should be included to expedite resolution?

A) Only the error message text
B) A detailed description of the issue, steps to reproduce, environment details, screenshots, and any relevant log files
C) The company's billing account number only
D) A request to call back at a convenient time

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To expedite support case resolution, the Administrator should provide a detailed description of the issue, clear steps to reproduce the problem, environment details (version, server configuration), screenshots of the error, and relevant log files. This comprehensive information allows the OneStream support team to diagnose and resolve the issue more efficiently.
</details>

---

### Question 17 (Hard)
**102.1.3** | Difficulty: Hard

An Administrator performs a Component Extract of a dashboard that references business rules and dimension members. What is included in the extraction?

A) Only the dashboard layout; all referenced components must be extracted separately
B) The dashboard and all directly referenced components such as business rules, with dependency tracking
C) The entire application including all unrelated components
D) Only the dashboard XML without any referenced objects

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Component Extract in OneStream includes the selected component (dashboard) along with its directly referenced dependencies such as business rules that the dashboard calls. The extraction tracks these dependencies to ensure the component functions correctly when loaded into the target environment. However, dimension members and their hierarchies are not included since they are considered application-level configuration.
</details>

---

### Question 18 (Medium)
**102.1.2** | Difficulty: Medium

What is a Starter Kit in the context of the Solution Exchange?

A) A fully completed production application ready for immediate use
B) A foundational application template that provides a starting point for building a specific type of solution, requiring further configuration
C) A set of hardware requirements for OneStream servers
D) A training course for new administrators

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A Starter Kit is a foundational application template available on the Solution Exchange that provides a pre-built starting point for a specific use case (e.g., financial consolidation, planning). Unlike a full MarketPlace solution, a Starter Kit requires additional configuration and customization to meet the organization's specific requirements. It accelerates implementation by providing a proven structure.
</details>

---

### Question 19 (Easy)
**102.1.1** | Difficulty: Easy

Which OneStream client interface do Administrators primarily use to configure applications, dimensions, and workflows?

A) The Excel Add-In
B) The OneStream Windows or Web client (Design Time)
C) The SQL Server Management Studio
D) The Windows Task Scheduler

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Administrators primarily use the OneStream Windows or Web client in Design Time mode to configure applications. This interface provides access to all administrative functions including dimension management, workflow configuration, business rule development, dashboard design, and security settings.
</details>

---

### Question 20 (Hard)
**102.1.3** | Difficulty: Hard

An Administrator wants to promote changes from Development to Production but the Production environment already has user-entered data. How should the Administrator handle the extraction and loading process to avoid data loss?

A) Extract the full application and load it, which will automatically preserve Production data
B) Use a Component Extract to migrate only the changed configuration components (e.g., business rules, dashboards) without affecting existing Cube data
C) Back up the Production database, delete the application, and reload everything from scratch
D) Ask all users to re-enter their data after the migration

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When promoting changes to a Production environment with existing data, the safest approach is to use Component Extract to migrate only the specific configuration changes (business rules, dashboards, form templates, etc.). Application extractions do not include Cube data, but loading a full application extraction can overwrite configurations that affect data processing. Component-level migration minimizes risk and preserves the Production data and its dependent configurations.
</details>
