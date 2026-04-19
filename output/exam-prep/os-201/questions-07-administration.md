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
