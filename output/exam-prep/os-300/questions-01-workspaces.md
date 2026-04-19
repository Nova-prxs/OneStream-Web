# Question Bank - Section 1: Workspaces (17% of exam)

## Objectives
- **300.1.1:** Explain the importance of Workspaces in OneStream
- **300.1.2:** Describe the contents of a Workspace
- **300.1.3:** Create custom embedded dashboards within Workspaces
- **300.1.4:** Troubleshoot common dashboard problems in Workspaces
- **300.1.5:** Implement dynamic repeater components in Workspaces

---

## Objective 300.1.1: Explain the importance of Workspaces in OneStream

### Question 1 (Easy)
**300.1.1** | Difficulty: Easy

What is the primary purpose of a Workspace in OneStream?

A) To store database connection strings for multiple environments
B) To provide a centralized landing page that organizes dashboards, tasks, and reports for users
C) To define security roles and user permissions at the application level
D) To configure server load-balancing settings for the OneStream platform

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspaces serve as centralized landing pages in OneStream that organize dashboards, tasks, reports, and other components into a single user-facing interface. They are not used for connection strings (A), security roles (C), or server configuration (D). Workspaces streamline the end-user experience by grouping related content together.
</details>

---

### Question 2 (Medium)
**300.1.1** | Difficulty: Medium

Which of the following best describes how Workspaces improve the end-user experience compared to navigating individual dashboards?

A) Workspaces bypass security so users can access all content without restriction
B) Workspaces consolidate dashboards, data entry forms, and workflow tasks into a single navigable interface
C) Workspaces automatically generate Cube Views for every dimension in the application
D) Workspaces replace the need for any dashboard development

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspaces improve the user experience by consolidating multiple components -- dashboards, data entry forms, workflow tasks, and reports -- into a unified, navigable interface. They do not bypass security (A), auto-generate Cube Views (C), or eliminate the need for dashboard development (D). Workspaces are a presentation and organization layer.
</details>

---

### Question 3 (Medium)
**300.1.1** | Difficulty: Medium

An administrator wants to ensure that different departments see only the dashboards and tasks relevant to their function. Which Workspace capability supports this requirement?

A) Configuring separate database connections per department
B) Assigning Workspace security so that groups see only their designated Workspace content
C) Creating a single Workspace and hiding components with CSS
D) Using the Spreadsheet component to filter content dynamically

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream allows administrators to assign security at the Workspace level, so different user groups see only the Workspace content designated for them. Creating separate database connections (A) is unrelated. Hiding with CSS (C) is not a supported or secure approach. Spreadsheet components (D) are for reporting, not content filtering.
</details>

---

## Objective 300.1.2: Describe the contents of a Workspace

### Question 4 (Easy)
**300.1.2** | Difficulty: Easy

Which of the following is NOT a standard component that can be included in a OneStream Workspace?

A) Dashboards
B) Task-related workflow items
C) SQL Server Agent jobs
D) Embedded reports

<details>
<summary>Show answer</summary>

**Correct answer: C)**

Workspaces can contain dashboards, task-related workflow items, embedded reports, and other OneStream components. SQL Server Agent jobs (C) are database-level scheduled tasks managed outside of OneStream and cannot be placed inside a Workspace.
</details>

---

### Question 5 (Easy)
**300.1.2** | Difficulty: Easy

In a OneStream Workspace, what is the role of the navigation panel?

A) It displays the application's system logs
B) It allows users to browse and switch between the dashboards, tasks, and reports contained in the Workspace
C) It provides a command-line interface for running Business Rules
D) It manages cube processing schedules

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The navigation panel in a Workspace provides users with an organized tree or menu structure to browse and switch between dashboards, tasks, and reports. It is not for system logs (A), command-line interfaces (C), or cube processing (D).
</details>

---

### Question 6 (Medium)
**300.1.2** | Difficulty: Medium

When organizing a Workspace, an administrator adds folders to group related items. What determines which items appear under each folder for an end user?

A) The alphabetical order of the dashboard names only
B) The folder structure configured by the administrator combined with the user's security assignments
C) The order in which items were created in the application
D) The file system directory structure on the OneStream server

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspace folder contents are determined by the administrator's configured folder structure and the security assignments of the user. Items the user does not have permission to view are hidden. It is not purely alphabetical (A), creation-order based (C), or tied to the server file system (D).
</details>

---

## Objective 300.1.3: Create custom embedded dashboards within Workspaces

### Question 7 (Medium)
**300.1.3** | Difficulty: Medium

When embedding a custom dashboard into a Workspace, which property must be set to ensure the dashboard loads correctly within the Workspace frame?

A) The dashboard's DataSource property must point to an OLAP cube
B) The dashboard must be marked as an Embedded Dashboard type and referenced in the Workspace configuration
C) The dashboard's connection string must be set to localhost
D) The dashboard must be exported as a PDF before embedding

<details>
<summary>Show answer</summary>

**Correct answer: B)**

To embed a dashboard within a Workspace, the dashboard must be configured as an embedded type and properly referenced in the Workspace's configuration. Pointing to an OLAP cube (A), setting localhost (C), or exporting as PDF (D) are not related to the embedding process.
</details>

---

### Question 8 (Hard)
**300.1.3** | Difficulty: Hard

A developer creates a custom embedded dashboard that should pass the current Workspace POV (Point of View) selections to its components. Which approach correctly propagates POV context into the embedded dashboard?

A) Hardcode the POV dimension members directly in each component's properties
B) Use the Workspace's POV bar parameters, which are automatically available to embedded dashboards through parameter binding
C) Create a separate POV bar inside the embedded dashboard that operates independently of the Workspace
D) Write a SQL query to read the POV from the OneStream database on each refresh

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Embedded dashboards in a Workspace inherit the Workspace's POV bar parameters through parameter binding. This ensures that when users change Entity, Scenario, Time, or other POV selections, the embedded dashboard reflects those changes. Hardcoding (A) defeats dynamic behavior, an independent POV bar (C) would not stay synchronized, and querying SQL (D) is unnecessary and not the intended mechanism.
</details>

---

## Objective 300.1.4: Troubleshoot common dashboard problems in Workspaces

### Question 9 (Medium)
**300.1.4** | Difficulty: Medium

A user reports that a dashboard embedded in their Workspace displays a blank white area instead of data. Which of the following is the MOST likely cause?

A) The user's monitor resolution is too low
B) The dashboard references a parameter or data source that the user does not have security access to
C) The OneStream server has run out of disk space
D) The Workspace has too many folders

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A blank dashboard area is most commonly caused by missing security access to the referenced parameter, data source, or cube. Monitor resolution (A) would cause layout issues, not blank content. Disk space (C) would cause broader server errors. Too many folders (D) would not cause a blank dashboard.
</details>

---

### Question 10 (Hard)
**300.1.4** | Difficulty: Hard

After deploying a Workspace update, several users report that their dashboard data is stale and does not reflect recent data loads. Other users see the correct data. What is the most likely troubleshooting step?

A) Reinstall the OneStream client on the affected users' machines
B) Check whether the affected users are connected to a different application server that has a cached version, and clear the dashboard cache
C) Rebuild all cubes in the application
D) Delete and recreate the Workspace from scratch

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When some users see stale data while others see current data, the issue is likely related to caching -- either at the server level or the client level. Checking which application server the users are connected to and clearing the dashboard cache is the correct troubleshooting approach. Reinstalling the client (A) is excessive, rebuilding cubes (C) is unrelated, and recreating the Workspace (D) is unnecessary.
</details>

---

## Objective 300.1.5: Implement dynamic repeater components in Workspaces

### Question 11 (Medium)
**300.1.5** | Difficulty: Medium

What is a dynamic repeater component in a OneStream Workspace used for?

A) To repeat a dashboard layout for each member in a specified dimension list, generating multiple instances dynamically
B) To duplicate the entire Workspace for each user in the system
C) To automatically retry failed data loads a specified number of times
D) To create looping animations in the dashboard user interface

<details>
<summary>Show answer</summary>

**Correct answer: A)**

A dynamic repeater component repeats a dashboard layout or component for each member in a specified dimension member list. For example, it can generate a separate panel for each Entity or Account dynamically. It does not duplicate Workspaces (B), retry data loads (C), or create animations (D).
</details>

---

### Question 12 (Hard)
**300.1.5** | Difficulty: Hard

A developer configures a dynamic repeater to display a Cube View for each child member of the current Entity. However, the repeater shows only the first child and stops. What is the most likely configuration issue?

A) The Cube View is set to suppress all rows
B) The repeater's member list expression is returning only one member instead of the full list of children
C) The Entity dimension has been locked by an administrator
D) The Workspace does not support more than one repeater instance

<details>
<summary>Show answer</summary>

**Correct answer: B)**

If a repeater shows only one instance, the most likely cause is that the member list expression is returning only a single member. This could be due to an incorrect function (e.g., using a function that returns the parent instead of children, or a syntax error truncating the list). Sparse row suppression (A) would affect the Cube View content but not the repeater count. Dimension locking (C) is not a standard OneStream concept in this context. Workspaces support multiple repeater instances (D).
</details>

---

### Question 13 (Easy)
**300.1.1** | Difficulty: Easy

Which of the following users typically benefits MOST from Workspaces in OneStream?

A) Database administrators managing SQL Server instances
B) End users who need a consolidated view of dashboards, reports, and tasks relevant to their role
C) Network engineers monitoring bandwidth usage
D) External auditors who only access printed reports

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspaces are designed primarily for end users, providing them with a consolidated, role-specific view of dashboards, reports, and workflow tasks. Database administrators (A) work in SQL Server Management Studio, network engineers (C) use network tools, and external auditors (D) typically receive exported reports rather than interacting with Workspaces directly.
</details>

---

### Question 14 (Medium)
**300.1.2** | Difficulty: Medium

An administrator adds a Cube View, a Spreadsheet report, and a workflow task list to a Workspace. How are these items typically organized for end-user navigation?

A) They are placed in a random order that changes on each login
B) They are organized into a hierarchical tree or tab structure defined by the administrator, optionally grouped into folders
C) They are sorted by file size from largest to smallest
D) They must each be placed in a separate Workspace

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Workspace items are organized into a hierarchical tree, tab structure, or folder groupings defined by the administrator. This provides a logical navigation experience for end users. Items are not randomly ordered (A), sorted by file size (C), or required to be in separate Workspaces (D).
</details>

---

### Question 15 (Easy)
**300.1.3** | Difficulty: Easy

What is an embedded dashboard in the context of a OneStream Workspace?

A) A dashboard stored on an external web server and loaded via iframe
B) A dashboard that is contained within and displayed directly inside the Workspace interface
C) A dashboard that has been compressed into a ZIP file
D) A dashboard that can only be viewed on mobile devices

<details>
<summary>Show answer</summary>

**Correct answer: B)**

An embedded dashboard is one that is contained within and displayed directly inside the Workspace interface, allowing users to interact with it without leaving the Workspace. It is not loaded from an external server (A), compressed (C), or limited to mobile devices (D).
</details>

---

### Question 16 (Hard)
**300.1.3** | Difficulty: Hard

A developer creates an embedded dashboard within a Workspace that includes a parameterized Cube View. The dashboard works in standalone mode but fails to display data when embedded. What is the MOST likely cause?

A) Embedded dashboards do not support Cube Views
B) The embedded dashboard's parameter bindings are not mapped to the Workspace's POV parameters, so required context is missing
C) The Workspace has reached its maximum component limit
D) The user's browser does not support embedded content

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When an embedded dashboard works standalone but not within a Workspace, the most common cause is that parameter bindings between the Workspace POV and the embedded dashboard parameters are not properly configured. Without this mapping, the dashboard does not receive the required context to retrieve data. Cube Views are fully supported in embedded dashboards (A), there is no practical component limit causing this (C), and browser compatibility (D) is not the issue.
</details>

---

### Question 17 (Medium)
**300.1.4** | Difficulty: Medium

A user reports that a Workspace loads but one of the dashboard tabs shows an error message "Component not found." What is the MOST likely cause?

A) The user's internet connection is too slow
B) The dashboard or component referenced in the Workspace configuration has been deleted or renamed without updating the Workspace reference
C) The Workspace has too many tabs open simultaneously
D) The user needs to install a browser plugin

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A "Component not found" error typically indicates that the referenced dashboard or component has been deleted, renamed, or moved without updating the corresponding reference in the Workspace configuration. Internet speed (A) would cause timeouts, not "not found" errors. Tab count (C) and browser plugins (D) are not related to this error.
</details>

---

### Question 18 (Hard)
**300.1.4** | Difficulty: Hard

After migrating a Workspace from a development environment to production, several embedded dashboards display errors. The same dashboards work correctly in the development environment. Which troubleshooting steps should be prioritized?

A) Reinstall OneStream on the production server
B) Verify that all referenced Business Rules, parameters, dimension members, and security groups exist in the production environment, as migration may have missed dependencies
C) Change the production server's operating system
D) Ask all users to clear their browser history

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When dashboards work in development but fail in production after migration, the most likely cause is missing dependencies -- Business Rules, parameters, dimension members, or security groups that exist in development but were not migrated to production. Verifying these dependencies is the correct first troubleshooting step. Reinstalling OneStream (A), changing the OS (C), or clearing browser history (D) will not resolve missing dependency issues.
</details>

---

### Question 19 (Easy)
**300.1.5** | Difficulty: Easy

Which of the following is a valid use case for a dynamic repeater in a Workspace?

A) Generating a separate chart for each child entity under a parent, dynamically based on the hierarchy
B) Sending automated emails to all users in the system
C) Compressing database tables for storage optimization
D) Scheduling overnight batch jobs

<details>
<summary>Show answer</summary>

**Correct answer: A)**

A dynamic repeater is commonly used to generate repeated dashboard components (such as charts, Cube Views, or panels) for each member in a dimension list, such as each child entity under a parent. It does not send emails (B), compress database tables (C), or schedule batch jobs (D).
</details>

---

### Question 20 (Medium)
**300.1.5** | Difficulty: Medium

When configuring a dynamic repeater, what determines how many instances of the repeated component are rendered?

A) The number of users currently logged in
B) The number of members returned by the repeater's bound member list expression
C) The screen resolution of the user's monitor
D) A fixed number hardcoded in the OneStream platform settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The number of repeated instances is determined by the member list expression bound to the repeater. For each member the expression returns, one instance of the component is rendered. It is not based on user count (A), screen resolution (C), or a fixed platform setting (D).
</details>

---

### Question 21 (Medium)
**300.1.1** | Difficulty: Medium

Which statement about Workspace security in OneStream is correct?

A) All users in the application always see every Workspace regardless of their role
B) Workspace access can be restricted based on security groups, ensuring users only see Workspaces assigned to them
C) Workspace security is managed exclusively through Windows Active Directory group policies
D) Security cannot be applied at the Workspace level; it only applies to individual dashboards

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream allows administrators to assign Workspace access based on security groups. This means different user groups can be assigned different Workspaces, ensuring they only see content relevant to their role. Workspaces are not universally visible to all users (A), security is managed within OneStream rather than exclusively through AD group policies (C), and security can indeed be applied at the Workspace level (D).
</details>

---

### Question 22 (Hard)
**300.1.3** | Difficulty: Hard

A developer needs to embed a dashboard that dynamically adjusts its layout depending on whether the user is viewing it on a wide desktop monitor or a narrower tablet screen. Which approach is most appropriate within a Workspace?

A) Create two completely separate Workspaces, one for desktop and one for tablet
B) Use the embedded dashboard's responsive layout properties and component anchoring/docking to adjust the layout based on available screen width
C) Ask tablet users to rotate their device to landscape mode only
D) Disable tablet access to the Workspace entirely

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream dashboards support responsive layout properties and component anchoring/docking settings that allow the layout to adjust based on available screen real estate. This is the correct approach for supporting multiple screen sizes within a single embedded dashboard. Creating separate Workspaces (A) doubles maintenance effort. Requiring landscape mode (C) or disabling access (D) are poor user experience choices.
</details>

---

### Question 23 (Medium)
**300.1.4** | Difficulty: Medium

An end user reports that the navigation panel in their Workspace is missing several items that other users in the same group can see. All users belong to the same security group. What should the administrator check first?

A) Whether the user's monitor supports the correct color depth
B) Whether the user has individual-level security overrides or a different Workspace profile assignment that restricts their view
C) Whether the OneStream server needs a reboot
D) Whether the user's keyboard is functioning correctly

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a user in the same security group sees fewer items than peers, the administrator should check for individual-level security overrides, a different Workspace profile assignment, or additional security restrictions specific to that user. Monitor color depth (A), server reboots (C), and keyboard functionality (D) do not affect Workspace navigation content.
</details>

---

### Question 24 (Hard)
**300.1.5** | Difficulty: Hard

A dynamic repeater is configured to display a panel for each entity in a large list of 200+ entities. Users report extreme slowness when loading the Workspace. What is the best approach to resolve this performance issue?

A) Upgrade all users' computers to faster processors
B) Limit the repeater's member list using filters or pagination so that only a subset of entities is rendered at a time, allowing users to page through results
C) Remove all formatting from the repeated components
D) Convert the dynamic repeater to a static image

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Rendering 200+ repeated component instances simultaneously causes significant performance degradation. The best solution is to limit the member list through filters, a parameterized subset, or pagination so that only a manageable number of instances render at once. Faster processors (A) provide marginal improvement for a rendering bottleneck. Removing formatting (C) offers negligible benefit. Converting to a static image (D) eliminates the dynamic functionality entirely.
</details>
