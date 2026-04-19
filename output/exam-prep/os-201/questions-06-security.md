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
