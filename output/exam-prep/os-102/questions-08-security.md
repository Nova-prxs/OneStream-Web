# Question Bank - Section 8: Working with Security (12% of exam)

## Objectives
- **102.8.1:** Add and deactivate users, understand naming conventions
- **102.8.2:** Configure Cube Security to control data access

---

## Objective 102.8.1: User add/deactivate and naming conventions

### Question 1 (Easy)
**102.8.1** | Difficulty: Easy

Where does an Administrator add a new user to the OneStream application?

A) Application Properties > General Tab
B) Security tab > User Management
C) Workflow tab > User Assignments
D) Cube tab > User Properties

<details>
<summary>Show answer</summary>

**Correct answer: B)**

New users are added through the Security tab under User Management. This is the central location for creating user accounts, assigning security groups, and managing user properties such as authentication method and default application settings.
</details>

---

### Question 2 (Easy)
**102.8.1** | Difficulty: Easy

What is the recommended approach when an employee leaves the organization and their OneStream access should be removed?

A) Delete the user account immediately
B) Deactivate the user account to preserve the audit trail
C) Change the user's password to prevent access
D) Remove the user from all security groups but keep the account active

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The recommended approach is to deactivate the user account rather than delete it. Deactivation prevents the user from logging in while preserving all audit trail records, workflow signoffs, and historical activity associated with the account. Deleting a user could compromise audit integrity.
</details>

---

### Question 3 (Medium)
**102.8.1** | Difficulty: Medium

Which authentication method allows OneStream to integrate with an organization's existing Active Directory for user login?

A) Local authentication only
B) Windows Authentication / Single Sign-On (SSO) via Active Directory integration
C) Two-factor authentication via SMS
D) Certificate-based authentication only

<details>
<summary>Show answer</summary>

**Correct answer: B)**

OneStream supports Windows Authentication and Single Sign-On (SSO) through Active Directory integration. This allows users to authenticate using their existing corporate credentials, simplifying user management and improving security by leveraging the organization's existing identity infrastructure.
</details>

---

### Question 4 (Medium)
**102.8.1** | Difficulty: Medium

An Administrator is setting up users and security groups. Which naming convention best practice should be followed?

A) Use short, cryptic abbreviations to save space
B) Use descriptive names that clearly indicate the purpose, role, or scope (e.g., "SG_Finance_DataEntry" for a security group)
C) Use only numeric identifiers for security groups
D) Name security groups after individual users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Best practice for naming conventions is to use descriptive, consistent names that clearly indicate purpose, role, or scope. For example, prefixing security groups with "SG_" followed by the department and role (e.g., "SG_Finance_DataEntry") makes it immediately clear what the group is for. This improves maintainability and reduces errors when managing security.
</details>

---

### Question 5 (Medium)
**102.8.1** | Difficulty: Medium

What is the purpose of Security Groups in OneStream?

A) To group Cube dimensions together
B) To organize users into logical groups and assign shared access permissions, rather than assigning permissions to individual users
C) To group Workflow Steps together
D) To define report categories

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Security Groups allow administrators to organize users into logical groups based on role, department, or function, and then assign access permissions to the group rather than to each individual user. This simplifies security management because when permissions change, the group is updated rather than each individual user account.
</details>

---

### Question 6 (Hard)
**102.8.1** | Difficulty: Hard

An Administrator needs to grant a contractor temporary access for three months. What is the best approach in OneStream?

A) Create a regular user account and set a calendar reminder to deactivate it later
B) Create the user account with an expiration date (if supported) and assign it to a security group with limited permissions appropriate for the contractor's role
C) Share an existing user's credentials with the contractor
D) Grant the contractor administrator-level access for convenience

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The best approach is to create a dedicated user account for the contractor with appropriately limited security group assignments. The Administrator should configure an expiration mechanism and assign only the minimum permissions needed for the contractor's specific tasks. Sharing credentials violates security best practices and audit requirements, and granting administrator access violates the principle of least privilege.
</details>

---

## Objective 102.8.2: Cube Security

### Question 7 (Easy)
**102.8.2** | Difficulty: Easy

What does Cube Security control in OneStream?

A) Which users can log into the application
B) Which dimension member intersections a user or group can view or edit within a specific Cube
C) Which dashboards a user can access
D) Which Workflow Steps are visible

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Security controls access to data at the dimension member level within a Cube. It determines which Entity, Account, Scenario, and other dimension member intersections a user or security group can read, write, or have no access to. This provides granular data-level security beyond application login and Workflow access.
</details>

---

### Question 8 (Medium)
**102.8.2** | Difficulty: Medium

An Administrator needs to ensure that the Finance team can only view and edit data for the "Finance" entity and its children, while the Sales team can only access the "Sales" entity. Where is this configured?

A) Application Properties > Entity Settings
B) Cube Security, by assigning Entity-level read/write permissions to each team's Security Group
C) Workflow Profile assignments only
D) Dashboard security settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

This is configured in Cube Security by assigning Entity-level permissions to each Security Group. The Finance Security Group would be granted read/write access to the "Finance" entity and its children, while the Sales Security Group would receive the same for the "Sales" entity. Cube Security is the primary mechanism for data-level access control by dimension member.
</details>

---

### Question 9 (Hard)
**102.8.2** | Difficulty: Hard

An Administrator sets Cube Security to grant "Read" access to a parent entity for a user, but "No Access" to one of the child entities. What data will the user see when viewing the parent entity?

A) The user sees the full aggregated value of the parent, including the restricted child
B) The user sees the parent value excluding the restricted child entity's contribution
C) The user cannot view the parent entity at all
D) The system displays an error message

<details>
<summary>Show answer</summary>

**Correct answer: A)**

In OneStream, when a user has Read access to a parent entity, they can see the aggregated value at that parent level, which includes contributions from all children including those the user cannot directly access. The security restriction on the child entity prevents the user from drilling down to or directly viewing that child's data, but the aggregated parent total is not adjusted. This is an important security consideration that administrators must understand.
</details>

---

### Question 10 (Medium)
**102.8.2** | Difficulty: Medium

Which type of Cube Security permission allows a user to view data but not modify it?

A) Write
B) Read
C) Admin
D) None

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Read permission in Cube Security allows users to view data at the specified dimension intersections but prevents them from making any changes. Write permission allows both viewing and modifying data. None (or No Access) prevents viewing entirely. Admin provides full control including the ability to manage security settings.
</details>

---

### Question 11 (Hard)
**102.8.2** | Difficulty: Hard

A user belongs to two Security Groups. Group A grants "Write" access to the "Revenue" account, and Group B grants "Read" access to the same account. What effective permission does the user have?

A) Read (most restrictive wins)
B) Write (least restrictive wins)
C) No Access (conflicting permissions cancel out)
D) The system prompts the user to choose

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In OneStream's security model, when a user belongs to multiple Security Groups with different permission levels for the same dimension member, the least restrictive (highest) permission takes effect. Since Write is a higher permission than Read, the user's effective permission is Write access to the "Revenue" account. This is important to understand when designing security group structures.
</details>

---

### Question 12 (Medium)
**102.8.2** | Difficulty: Medium

An Administrator wants to restrict which Scenarios a user can access (e.g., allowing Budget access but not Actual). Where is this restriction configured?

A) In the Scenario dimension properties
B) In Cube Security, by setting Scenario-level permissions for the user's Security Group
C) In the Workflow Profile only
D) In Application Properties > Scenario Settings

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Scenario-level access restrictions are configured in Cube Security. The Administrator assigns permissions at the Scenario dimension member level for each Security Group. For example, a Budget team's Security Group can be granted Write access to the Budget Scenario and No Access to the Actual Scenario. This ensures users only interact with the data versions relevant to their role.
</details>

---

### Question 13 (Easy)
**102.8.1** | Difficulty: Easy

What happens when a user account is deactivated in OneStream?

A) The user account and all associated data are permanently deleted
B) The user can no longer log in, but the account and all audit trail records are preserved
C) The user can still log in with read-only access
D) The user is automatically reassigned to a different security group

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When a user account is deactivated, the user can no longer authenticate or log into the application. However, the account record and all associated audit trail entries (Workflow signoffs, data change history, etc.) are fully preserved. This is why deactivation is preferred over deletion for maintaining audit integrity.
</details>

---

### Question 14 (Medium)
**102.8.1** | Difficulty: Medium

An Administrator needs to set up security for a new department with 15 users who all require the same access. What is the most efficient approach?

A) Configure permissions individually for each of the 15 users
B) Create a Security Group for the department, assign the appropriate permissions to the group, and add all 15 users as members of the group
C) Share a single user account among all 15 users
D) Grant all 15 users administrator access

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The most efficient approach is to create a Security Group, configure the necessary permissions (Cube Security, Workflow access, dashboard access) on the group, and then add the 15 users as members. This way, permissions are managed centrally on the group rather than individually on each user. When access needs change, only the group needs to be updated.
</details>

---

### Question 15 (Hard)
**102.8.2** | Difficulty: Hard

An Administrator configures Cube Security so that Security Group "SG_APAC" has Write access to the "Asia_Pacific" entity at the parent level. Does this automatically grant Write access to all child entities under "Asia_Pacific"?

A) No, each child entity must be granted access individually
B) Yes, security permissions on a parent entity automatically cascade to all descendant (child) entities unless explicitly overridden at a lower level
C) No, permissions only apply to the exact member specified
D) Yes, but only for Read access, not Write

<details>
<summary>Show answer</summary>

**Correct answer: B)**

In OneStream Cube Security, permissions assigned to a parent entity cascade down to all descendant (child) entities by default. This inheritance means that granting Write access to "Asia_Pacific" automatically provides Write access to all entities beneath it in the hierarchy (e.g., "Japan," "Australia," "China"). Administrators can override this inheritance by setting explicit permissions at a lower level if needed.
</details>

---

### Question 16 (Medium)
**102.8.1** | Difficulty: Medium

Which of the following is a recommended naming convention for Security Groups in OneStream?

A) Group1, Group2, Group3
B) A prefix indicating the type followed by a descriptive name, such as "SG_Finance_Reviewer" or "SG_HR_DataEntry"
C) The name of the first user added to the group
D) Random alphanumeric strings for security through obscurity

<details>
<summary>Show answer</summary>

**Correct answer: B)**

A clear, consistent naming convention using a prefix (such as "SG_" for Security Group) followed by descriptive identifiers for department and role makes security administration much more manageable. Names like "SG_Finance_Reviewer" immediately communicate the group's purpose, making it easier to audit security configurations and troubleshoot access issues.
</details>

---

### Question 17 (Easy)
**102.8.2** | Difficulty: Easy

Which dimension is most commonly used in Cube Security to restrict data access by organizational unit?

A) Account
B) Entity
C) Time
D) Flow

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The Entity dimension is the most commonly used dimension for Cube Security restrictions because it represents the organizational structure (business units, subsidiaries, departments). By assigning Entity-level permissions to Security Groups, administrators can ensure users only access data for their respective parts of the organization.
</details>

---

### Question 18 (Hard)
**102.8.2** | Difficulty: Hard

An Administrator needs to configure security so that a group of users can enter Budget data but can only view (not modify) Actual data. How should this be set up in Cube Security?

A) Create two separate user accounts for each user, one for Budget and one for Actual
B) Assign the Security Group Write access to the Budget Scenario member and Read access to the Actual Scenario member in Cube Security
C) Use separate applications for Budget and Actual
D) Configure Form Templates to be read-only for Actual, which is sufficient without Cube Security

<details>
<summary>Show answer</summary>

**Correct answer: B)**

The correct approach is to configure Cube Security at the Scenario dimension level. The Security Group is granted Write access to the Budget Scenario member (allowing data entry) and Read access to the Actual Scenario member (allowing viewing but not modification). This provides data-level protection that cannot be bypassed regardless of which interface the user accesses.
</details>

---

### Question 19 (Medium)
**102.8.1** | Difficulty: Medium

An Administrator is creating a new user account. Which of the following properties must be configured during user creation?

A) Only the username
B) Username, authentication method, default application, and Security Group assignment
C) Only the username and password
D) Only the Security Group assignment

<details>
<summary>Show answer</summary>

**Correct answer: B)**

When creating a new user account, the Administrator must configure several properties: the username (which must be unique), the authentication method (local or Windows/SSO), the default application, and Security Group assignments that define the user's access permissions. Without proper Security Group assignment, the user would have no access to any data or functionality.
</details>

---

### Question 20 (Medium)
**102.8.2** | Difficulty: Medium

What is the effect of assigning "None" (No Access) permission to a dimension member in Cube Security?

A) The user can view the data but cannot modify it
B) The user cannot view or access data at that dimension member intersection, and the member may not appear in their navigation
C) The user can access the data only through dashboards
D) The data is deleted for that member

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Assigning "None" or "No Access" permission to a dimension member means the user cannot view, access, or interact with data at that intersection. The member may not even appear in the user's dimension navigation or selection lists. This is the most restrictive permission level and effectively hides the data from the user.
</details>

---

### Question 21 (Hard)
**102.8.1** | Difficulty: Hard

An organization is migrating from a legacy system to OneStream and needs to provision 500 users. All users authenticate through Active Directory. What is the most efficient provisioning strategy?

A) Create each user account manually one at a time
B) Use bulk user provisioning (user import) to create accounts from a formatted file, configure AD/SSO integration for authentication, and pre-assign users to Security Groups based on their role
C) Ask each user to create their own account through a self-service portal
D) Create a single shared account for all 500 users

<details>
<summary>Show answer</summary>

**Correct answer: B)**

For large-scale user provisioning, the most efficient strategy is to use bulk user import capabilities to create accounts from a formatted file containing usernames, roles, and group assignments. Combined with Active Directory integration for authentication, this eliminates the need to manage individual passwords and enables centralized identity management. Pre-assigning Security Groups ensures users have appropriate access from day one.
</details>

---

### Question 22 (Easy)
**102.8.2** | Difficulty: Easy

Can Cube Security be configured at the Account dimension level in addition to the Entity dimension level?

A) No, Cube Security only applies to the Entity dimension
B) Yes, Cube Security can be configured for multiple dimensions including Account, Entity, Scenario, and others
C) Only if the Account dimension has fewer than 100 members
D) Only in the Enterprise edition of OneStream

<details>
<summary>Show answer</summary>

**Correct answer: B)**

Cube Security is a multi-dimensional security model that can be configured across multiple dimensions, not just Entity. Administrators can set permissions on Account members (e.g., restricting access to sensitive salary accounts), Scenario members (e.g., Budget vs. Actual), and other dimensions as needed. This provides granular, multi-dimensional data access control.
</details>
