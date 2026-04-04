---
title: "Journals Overview"
book: "design-reference-guide"
chapter: 11
start_page: 999
end_page: 1031
---

# Journals Overview

Journals are used to record business transactions in the accounting records of a business from an enterprise resource planning perspective. Journals are typically used in financial consolidation projects to record closing adjustments. Journals display account amounts and indicate whether the accounts are balanced. In the OneStream app, journals are written to the AdjInput member of the origin dimension. You can open or create journals from the OnePlace Workflow tab. Select a Workflow Profile, Scenario, Time Period, and then click the journal you want to open. The following image is an example of the Input Journals workflow interface.

![](images/design-reference-guide-ch11-p999-4251.png)

1. The Journal Templates pane is collapsible. 2. The Journals Grid is a listing of available journals. It is located above the Journal Header/Journal Details area. 3. Entities are located at the top of the Journals Grid. You can filter the grid by entity. 4. Journal Header properties are displayed in collapsible panes. 5. You can drag the horizontal divider up or down, or snap it to a location using the arrow buttons.

## Journals Toolbar

The Journals toolbar contains many tools for working with journals. This section describes each tool in detail.

![](images/design-reference-guide-ch11-p1000-7970.png)

## Input Journals

Once in this step, there will be two options under Workflow Journal Templates: Required and Optional. The required Journals will have to be completed before you can move on from this step.

![](images/design-reference-guide-ch11-p1000-7971.png)

Create Journal Click this to create a Journal if a template is not available. If this button is disabled, it is because the workflow profile is configured to only allow you to create journals based on existing templates.

![](images/design-reference-guide-ch11-p1001-7972.png)

Create Journal Using Template Use this to create a Journal using the selected template. Manually enter the required Journal data and Click Save.

![](images/design-reference-guide-ch11-p1001-7973.png)

Create Journal Using Excel or Comma Separated Values File Click this to load a journal entry via Excel template or CSV template. See Loading Journal Data in Data Collection Guides for more details on using these templates.

![](images/design-reference-guide-ch11-p1001-7974.png)

Copy Selected Journals Copy existing journals into other time periods and scenarios. This is useful when you are creating similar journals, managing recurring journals, or copying Actual data into budget or what-if scenarios. 1. Select the journals you want to copy and click Copy Selected Journals. You can select journals in any status but when copied, they will be set to Working status in the new time period. 2. Click the drop-down arrow in the Target Scenario field. 3. Make a selection. The selection applies to all journals in that single-batch copy. 4. For each journal, select one or more time periods. 5. Type a new journal name and description. 6. You can include attachments by placing a check in the Include Attachments checkbox. Administrators and members of the Journal Process Group can copy journals into a specified Adjustment Workflow Profile.

> **Note:** After journals are copied, WfProfile_WfUnit_Scenario_TimePeriod_Index is

appended to the journal name.

![](images/design-reference-guide-ch11-p1002-7978.png)

Delete Selected Item Delete one or more journals simultaneously. Journals in Working or Rejected status can be deleted. This saves time when you need to remove multiple journal entries quickly and efficiently. Select the boxes next to the journals you want to remove then click this button and confirm.

![](images/design-reference-guide-ch11-p1002-7975.png)

Reapply Template Settings to Journal This will clear the data from the selected Journal and return it to the original template.

![](images/design-reference-guide-ch11-p1002-7977.png)

Show UD Descriptions Check this checkbox to display descriptions for the eight UD Dimension Types. They will display next to the Dimension Type in the Journal POV and the line item headers. Descriptions are defined in the Application Properties, and can provide clarity on the purpose of the dimensions.

> **Note:** This feature applies to the Journal page only. Reports, BRApis and Excel/CSV

templates related to journals remain unchanged.

![](images/design-reference-guide-ch11-p1002-7976.png)

Export Export journals from within the journals page. This is useful when you are more comfortable using a spreadsheet tool to edit journal data. You can export data from one area in OneStream and import it into another area or application. 1. Click Export to open the Select Journals to Export dialog. Workflow profile and scenario are locked. 2. Type a Time Filter using the T# syntax. For example, T#2023M1, or for multiple time periods, T#2023M1,T#2023M2. 3. Type a Journal Status. Select a single journal status or select All to include all statuses. 4. Type a Journal Name. This optional step will find all journals that contain the text you type. 5. Click OK to generate an XLSX file with journals that match the selected criteria. 6. In the File Explorer dialog, browse to where you want to save the XLSX file. Data from all journals that match the selection criteria is saved. Journal Entry Checkboxes Select multiple journal entries in the same state (e.g., completed, posted, approved, etc.) in order to Submit, Post, Approve, or Quick Post them at the same time. Select multiple journal entries in either the Working or Rejected status to delete them at the same time.

![](images/design-reference-guide-ch11-p1003-7980.png)

![](images/design-reference-guide-ch11-p1003-7979.png)

Validate You can ensure that dimension members selected in the journal are valid cube intersections based on constraints configured in the dimension library. If validation errors exist, they will display in a Validation pane allowing you to view them as you make edits to the journal. This helps you save time by validating entries earlier in the process. You must have the Journal Processing security role to use this feature and dimension member constraints must first be configured in the dimension library. To use this feature: 1. Open a journal that is in Working status. 2. Click Validate. If invalid data is found, the Validation pane will display messages that detail each entry error.

![](images/design-reference-guide-ch11-p1004-7982.png)

Once a Journal is completed, select Quick Post or Post. When all required Journals and any optional Journals are finished, click Complete Workflow and a green check will appear for the Journals Workflow Step.

![](images/design-reference-guide-ch11-p1004-7981.png)

Depending on the security configuration, there are multiple options for Journals. If full security is in place, the end user will be able to create and submit. The approver will approve or reject, then the end user can post and complete the Workflow. Journal Line Items Journal data is entered into line items. You can add line items to reflect the Dimensions they are adjusting. Any dimensions not in line items must be configured in the Journal POV. To select a dimension member in journal line items: 1. Double-click the Entity, Account, Flow IC, or UD1-UD8 field. 2. If you know the member name, type it into the text field. Repeat for all fields as needed. 3. If you do not know the member name, use the Select Member dialog to locate the member.

|Col1|NOTE: If the dimension members have a description, the members are displayed<br>in the journal line items. For each dimension, you can set a preference to hide or<br>show the description in journal line items. These preferences are saved on your<br>local machine in OneStream user settings.|
|---|---|

You can filter journal line items by clicking the filter icon in the column headings. A drop-down selection box appears where you can choose from various filtering options.

![](images/design-reference-guide-ch11-p1005-7983.png)

This is useful when you are working with journals that contain a large amount of line items, making it easier to locate a specific subset of line items. There are four line item actions buttons that sit just above the line item entries that aid in editing the journal line items: l Add: Creates a new journal line item. If an existing line is already selected, Add copies the selection. If no line is selected, the new line item is blank. l Up: Moves the selected item up in the list. l Down: Moves the selected item down in the list. l Remove: Deletes the selected line item.

### Process

Once in this step, click Process Cube in order to process the loaded data.

![](images/design-reference-guide-ch11-p1006-7985.png)

This icon performs No Calculate and all the standard Calculate, Translate, and Consolidate options using Calculation Definitions. Each added line item can be filtered by entity for reviewer level processes. Once the Cube is processed, the Process task will change from blue to green and move to the Confirm task. No Calculate can be used to allow the assignment of Data Management Sequences.

### Confirm

![](images/design-reference-guide-ch11-p1006-7984.png)

This step runs the Confirmation Rules defined for this particular Workflow. This will immediately inform users if they have passed or failed the data quality rules. The two types of statuses for this step are Warning or Error. Warning means the user is outside of the threshold, but it will not stop the process. Error means the user is outside of the threshold and this will stop the process and turn this step to red. If anything has failed, revisit one or many of the previous steps to make sure the data is accurate and complete.  Once the data has passed all quality rules, the Confirm task will change from blue to green and move to the Certify task.

### Certify

This is typically the final step in the Workflow process. This certifies and completes the phase of the Workflow.

![](images/design-reference-guide-ch11-p1007-7987.png)

Some questions may need to be answered regarding the processes. Click each set of questions under the Questionnaires area.  Answer the questions by clicking in the response cell and selecting the correct answer.  Comments can also be added in order to explain the answers. The status will be displayed on the right. When this is completed, click on the Set Questionnaire Status icon and select Completed and then OK.

![](images/design-reference-guide-ch11-p1007-7986.png)

After each group of questions is completed, the Set Certification Status icon will be enabled. Click this and select Certify in order to certify the data as complete and accurate. This will give the final green check for the month being processed and the data can now be trusted as complete and accurate by any stakeholder that is analyzing this information.

![](images/design-reference-guide-ch11-p1007-7988.png)

This is a one click option to expedite the Certify process.  No questions need to be answered. This will give the final green check for the month being processed and the data can now be trusted as complete and accurate by any stakeholder that is analyzing this information. This data can now be used for Consolidations by users or managers responsible for this workflow. They can look at data as it moves up and perform their own top side adjustments, confirmations, and certifications at as many levels as is appropriate for the organization.

![](images/design-reference-guide-ch11-p1007-7989.png)

Click on Dependent Status to see the status of each required Workflow task to ensure they are all completed. This will display the Workflow Profile name and all input types, the Workflow Channel, the status of each input type, the last step completed for each input type, the percentage of each step that is OK, In Process, Not Started, and steps with errors, and a record of when the last activity took place for each step.  See Right-Click Options for details on these right-click functions.

![](images/design-reference-guide-ch11-p1008-7990.png)

For more details on Workflow, see Workflow in Workflow.

### Multi-Period Processing

Click on the Year in the Navigation Pane to enact Multi-Period Processing options:

![](images/design-reference-guide-ch11-p1008-7991.png)

From here, perform multiple workflow tasks for one to many time periods, as shown above.

## Analysis Pane

In each workflow profile for a defined period there will be an Analysis Pane under the Status Pane. For example: under Process, Confirm, and Certify tasks.

### Cube Views And Dashboards For Analysis

This is where data can be viewed and analyzed in cube views and dashboards. If there is a grid in any dashboard or cube view, these are available for further drill down or annotation by right clicking the cell. See Using OnePlace Cube Views and Using OnePlace Dashboards for more details. To view the Cube Calculation Status, click on Cube Views| Calculation Status to show a data grid presenting the Calculation Status of the current active Workflow POV. This will be available at the total Monthly (not an Origin process) Review, Process, or Certify tasks. See Calculation Status in About the Financial Model for more details on this feature.

### Intercompany Matching

IC Matching which will show any Intercompany discrepancies. If the button is red in the status column, click the item to see the details. Each Intercompany counterparty will be visible along with counterparties with an Intercompany variance.  By clicking on the counterparty, details including the Reporting Currency, Entity Currency, and Partner Currency will be visible. Select the Intercompany Partner to see the status of the Intercompany issue and any annotations the partner may have made. Select the Difference row to see both parties’ statuses and annotations.

![](images/design-reference-guide-ch11-p1009-7992.png)

A red triangle indicator displays in the cell when data is dynamically derived. This generally occurs when the intercompany matching report performs currency translations. Right click on the Entity row to perform the following functions: Set IC Transaction Status This allows the user to update the status by selecting Not Started, Loaded, Adjusting, Disputed, Finalized. The user may also include comments for the counterparty to see. Show Partner Workflow Status This allows the user to see the Workflow status of the counterparty. Show/Hide Dimension Details This allows the user to see the Dimension details for the Intercompany accounts. Drill Down This will allow the user to drill down on the Dimensions in order to get more information about the Intercompany data. See Drill Down in Using OnePlace Cube Views for more details on this feature. See Intercompany Eliminations in About the Financial Model for more details on Intercompany.

## Create A New Journal

There are three ways to create a journal as shown in the image below.

![](images/design-reference-guide-ch11-p1010-4282.png)

1. Create Journal: Create a new blank journal. 2. Create Journal Using Selected Template: Create a new journal based on the selected template. 3. Create Journal Using File: Create a new journal using data from either an Excel file or a CSV file. In the following sections, you will create journals using the three methods listed above.

### Create Journal

To display the Journals toolbar and create a new blank journal: 1. In the OneStream app, click the OnePlace tab. 2. Click Workflow to expand. 3. Click the existing Workflow Profile to change it if necessary. a. In the Select Workflow View dialog, click the Workflow Profile tab and make a selection. b. Click the Scenario tab and make a selection. c. Click the Year tab and make a selection. d. Click OK. 4. Expand the period tree, locate the time period, and click Journals.

![](images/design-reference-guide-ch11-p1011-7993.png)

5. On the Journals toolbar, click Create Journal. 6. In the Journal Properties editor, under General, type a name for the new journal. 7. Under Journal, set properties as needed. 8. Under Point of View, set properties as needed. 9. Click Save. The new journal is added to the Journals pane to the right of the journal properties section.

### Create Journal Using Selected Template

Templates can be built for journals to accelerate repetitive journal creation. They allow for control over which properties are configured by the user. They can be used to create new journals. 1. In the OneStream app, click the OnePlace tab. 2. Click Workflow to expand. 3. Click the existing Workflow Profile to change it if necessary. a. In the Select Workflow View dialog, click the Workflow Profile tab and make a selection. b. Click the Scenario tab and make a selection. c. Click the Year tab and make a selection. d. Click OK. 4. Expand the period tree, locate the time period, and click Journals.

![](images/design-reference-guide-ch11-p1011-7993.png)

5. In the Workflow Journal Templates pane, click a journal template to select. This will be the journal on which you will base the template. 6. On the Journals toolbar, click Create Journal Using Selected Template. It will automatically be given a name but you can change this if necessary. 7. Under Journal Template, set properties as needed. 8. Under Journal, set properties as needed. 9. Under Point of View, set properties as needed. 10. Click Save. The new journal is added to the Journals pane to the right of the journal properties section.

### Create Journal Using File

1. In the OneStream app, click the OnePlace tab. 2. Click Workflow to expand. 3. Click the existing Workflow Profile to change it if necessary. a. In the Select Workflow View dialog, click the Workflow Profile tab and make a selection. b. Click the Scenario tab and make a selection. c. Click the Year tab and make a selection. d. Click OK. 4. Expand the period tree, locate the time period, and click Journals.

![](images/design-reference-guide-ch11-p1011-7993.png)

5. On the Journals toolbar, click Create Journal Using Excel (xlsx) or Comma Separated Values (csv) File. 6. In the Open dialog, browse to the file and click Open. 7. Under Journal Template, set properties as needed. 8. Under Journal, set properties as needed. 9. Under Point of View, set properties as needed. 10. Click Save. The new journal is added to the Journals pane to the right of the journal properties section.

## Open A Journal

You can open any journals you have created from the Journals pane in the OneStream app. 1. In the OneStream app, click the OnePlace tab. 2. Click Workflow to expand. 3. Click the existing Workflow Profile to change it if necessary. a. In the Select Workflow View dialog, click the Workflow Profile tab and make a selection. b. Click the Scenario tab and make a selection. c. Click the Year tab and make a selection. d. Click OK. 4. Expand the period tree, locate the time period, and click Journals.

![](images/design-reference-guide-ch11-p1011-7993.png)

The Workflow Journal Templates pane and the Journals pane display.

![](images/design-reference-guide-ch11-p1016-4295.png)

5. In the Journals pane, click the journal you would like to view. You can new set journal properties as needed and enter line items into the journal.

## Rename A Journal

Journals that are in Working or Rejected status can be renamed by administrators or members of the workflow profile Journal Process Group security role. Follow the steps below to rename a journal: 1. Select a journal that is in Working or Rejected status. If you select a journal of another status, or more than one journal, the Rename Selected Journal button is disabled.

![](images/design-reference-guide-ch11-p1016-4296.png)

. 2. Click Rename Selected Journal in the toolbar 3. In the Rename Journal dialog box, type a unique journal name. 4. Click Save. The journal name is updated. If you type a name that already exists, you will be prompted to type a unique name.

|Col1|NOTE: The system will not append any additional workflow profile information to<br>the new journal name.|
|---|---|

## Delete A Journal

You can delete journals that you no longer use or need. 1. Navigate to the journal you would like to delete: a. In the OneStream app, click the OnePlace tab. b. Click Workflow to expand. c. Click the existing Workflow Profile to change it if necessary. i. In the Select Workflow View dialog, click the Workflow Profile tab and make a selection. ii. Click the Scenario tab and make a selection. iii. Click the Year tab and make a selection. iv. Click OK. d. Expand the period tree, locate the time period, and click Journals.

![](images/design-reference-guide-ch11-p1011-7993.png)

2. In the Journals pane, place a tick into the box next to the journal you want to delete. You can delete more than one journal at a time by ticking all of the necessary boxes.

![](images/design-reference-guide-ch11-p1018-4301.png)

3. Click Delete Selected Item. 4. In the confirmation dialog, click Yes.

## Loading Data Into Journals

There are several ways to load data into a journal, including the following: l Journal Excel template l Journal CSV template l OneStream Workflow process l Extensibility Business Rules (BRApi)

## Filter Journals

You can filter journal entries on the entity name and columns, and you can combine multiple filters to refine the list of journals according to your needs. For example, you can filter journals by entity and journal type to see only those journals that match the filter criteria. Filtering helps auditors, accountants, and other stakeholders, quickly identify specific transactions and reduces the time spent on searching for information, especially when investigating entries. To filter journals by entity: 1. On the Journals list toolbar, click the Filter button at the top left corner.

![](images/design-reference-guide-ch11-p1020-4306.png)

2. In the filter dialog box, type all or part of the entity name in the entity field. This filter is a “Contains Text” filter, meaning it will match any entity name that contains the entered text. 3. Click Apply. The journal list displays only those journals that contain the specified filter string in their entity name. To filter journals by column: 1. Click the Filter icon in the column header that you want to filter. 2. In the Filter dialog box, choose between the following two options: l In the upper part of the dialog box, select one or more items to filter by distinct values found in the respective column. l In the lower part of the dialog box, set your own filtering criteria. 3. Click the Filter icon to apply the filter. To clear filters: 1. Click on the filter icon that you want to clear. 2. Click the Clear Filter button.

## Bulk Selection Of Time Periods For Copying

You can select time periods for all journals in a copy job simultaneously. This streamlines the journal copy process, and avoids having to select time periods individually for each journal. 1. Navigate to the Input Journals page. 2. Select the journals you want to copy. 3. Click Copy Journals. 4. At the top of the Copy Dialog box, select the appropriate target time periods.

![](images/design-reference-guide-ch11-p1021-4309.png)

5. Click OK.

## Recurring Journals

A recurring journal is a journal entry that repeats at regular intervals (like monthly or annually) for routine transactions. It is commonly used for expenses that repeat over time on a consistent basis. Recurring journal entries can be automated in accounting systems to: l maintain consistency of financial statements l ensure accurate recording of routine transactions l reduce effort while minimizing the risk of errors

### Create A Standard Recurring Journal

There is a new journal type called Standard Recurring. This type lets you define the frequency of automatic journal creation. 1. Create Journal. 2. For journal type, select Standard Recurring. 3. For Recurring Journal Frequency, select one of the following: l All Time Periods l Monthly l Quarterly l Half Yearly l Yearly

|Col1|NOTE: The journal cannot recur more frequently than input periods allow. For<br>example, if you are using quarterly input periods, you cannot have a monthly<br>journal recurrence as Q1 would contain three months.|
|---|---|

4. Post a Journal. A new journal is created in the specified time period, and is a copy of the previous journal with the same naming scheme, settings, POV, and line items.

|Col1|NOTE: You must have process rights, as you are the creator of the recurring<br>journal.|
|---|---|

You will receive a confirmation notice upon successful creation. Journal import and export does not handle the Recurring Journal Frequency setting. Imports and exports that include standard recurring journals will default to All Time Periods.

## Process A Journal

After you have created or opened a journal and entered data into the ledger, there are several actions you can perform on the journal.

![](images/design-reference-guide-ch11-p1023-4317.png)

1. Validate a journal: Ensure that data is correct and that cube intersections are valid. 2. Reject a journal: Discard the journal data. 3. Quick post a journal: Apply the journal data to the associated cube. 4. Unpost: Remove the journal data from the associated cube. In the following sections, you will move a journal through the above processes.

### Validate A Journal

You can ensure that dimension members selected in the journal are valid cube intersections based on constraints configured in the dimension library. If validation errors exist, they will display in a Validation pane so that you can view them and make edits to the journal. This helps you save time by validating entries earlier in the process.

> **Note:** You must have the Journal Processing security role to use this feature and

dimension member constraints must first be configured in the dimension library. To validate a journal: 1. Open the journal that you would like to validate. 2. Click Validate. Journal data will be evaluated and if invalid data is found, the Validation pane will display a message that details each entry error.

### Reject A Journal

You can reject a journal after it has been submitted for approval. To reject a journal: 1. Open the journal that you would like to reject. 2. Click Reject. 3. In theReject Journalsprompt, type an optional reason or comment. 4. Click one of the following: a. Cancel- the prompt closes and the rejection is canceled. b. OK- the prompt closes, selected journals are rejected, and the comment (if entered) is saved to all journals. The journal status changes to Rejected. In theJournal Audit, rejection comments are displayed in theRejected Commentfield. If multiple rejections occur, only the last rejection comment is displayed.

### Post A Journal

Posting a journal in the OneStream app means adding the ledger data into the associated cube, similar to moving transaction data into a general ledger. To post journal data: 1. Open the journal that you would like to post. 2. Enter line item journal data as necessary. 3. Optional. If you would like to post more than one journal at a time, select the journals in the Journals pane by placing a tick in the selection box next to the journals. 4. Click Quick Post. A green checkmark next to the journal in the Journals pane indicates the journal has been posted.

![](images/design-reference-guide-ch11-p1025-4325.png)

Journal data is moved into the associated cube.

### Unpost A Journal

Unposting a journal in the OneStream app means removing the ledger data from the associated cube. To unpost journal data: 1. Open the journal that you would like to unpost. 2. Optional. If you would like to unpost more than one journal at a time, select the journals in the Journals pane by placing a tick in the selection box next to the journals. 3. Click Unpost. The green checkmark next to the journal in the Journals pane is removed, indicating the journal has been unposted. Journal data is removed from the associated cube.

## Journal Templates

Create pre-set Journal Templates in Journal Template Groups. After the Journal Template Groups are created, they are organized into Journal Template Profiles and then assigned to Workflow Profiles. For more details on Profiles, see Form Template Profiles. Journal details can also be loaded via an Excel or CSV template. See "Loading Journal Data" in Data Collection Guides for more information.

### Journal Template Properties

This section includes various journal template properties.

#### General

Name Name of the journal template. Description A detailed description of the journal template.

#### Journal Template

Journal Template Type A journal template can either be standard or auto approved. If auto approved, a user can create a journal from the template with limited editing ability meaning permission to change name, description, and so forth. After pressing Save, the line items are validated, and the journal skips the approval process and becomes approved. If the journal needs to be edited, it is handled like any other journal from that point forward. Someone in the Approve Journals group must reject it for the original user to edit it. Then, the regular Submit, Approve steps will occur. This approach is needed because the person who created the auto approved template only pre-approved certain numbers to be put in the system. The user does not need to be in the ApproveJournals security group to do this. They only need to be in the ProcessJournals security group which is normally the group for end users to save or submit journals. Journal Template Group This displays the journal template group that was created under the new template. Journal Requirement Level This setting works together with Frequency and determines when a journal is required. The options available are Not Used (anymore), Optional, or Required.

#### Journal Frequency

All Time Periods This allows the journal to display every period. Monthly This allows the journal to display every month. If this is for a weekly application, it will display the last week of each month. Quarterly This allows the journal to display every quarter, or four times a year. Half Yearly This allows the journal to display two times a year; once in June and December. Yearly This allows the journal to display once a year in December. Member Filter This turns on the Frequency Member filter and then the filters can be defined in that section. For example, if this journal only needs to be completed in September, use the filter T#WFYearM9. Frequency Member Filter This becomes available when Member Filter is chosen in the Forms Frequency options above, otherwise this is gray for all the others. The purpose of this option is to allow the ability to filter by time. The following settings control what the user can modify when creating a journal. Settings are True or False. Can Change Journal Settings Can Change Journal POV Can change Journal Line Items Can change Journal Amounts

#### Journal

#### Journal Type

The options are Standard, Auto Reversing, or Allocation. When posting an Auto Reversing journal, the auto reversal journal is automatically created in the next time period and set to the Approved state. The Auto Reversal journal has all the debits and credits reversed. When un-posting an Auto Reversing journal, check to make sure the auto reversal is not posted first. Ifnot, delete the Auto Reversal journal from the next time period and un-post. An Allocation journal can be set up to perform simple or more intricate allocations, such as creating the weighting of the allocation, previewing the actual allocation entries, and un-posting them if they need to be run again.

#### Journal Balance Type

Balanced The entire set of journal lines must balance between the debits and credits. Balance by Entity Debits and credits in a multi-Entity journal must balance for each entity. Unbalanced Balance check will not be performed. This is normally used for one-sided journals. Is Single Entity If True, the entity name is entered in the Journal POV and all journal lines relate to this one entity. If False, the Cube, Entity and Parent columns must be filled out for every line in the journal instance. Entity Member Filter This Member Filter will help limit the list of entities presented to the user in the Journal POV and journal lines. Point of View In order to limit the amount of setup for every journal line, the items that remain constant (e.g. Flow = None) can be set in the Journal POV instead of in every line Item.

> **Tip:** A Journal template can be repeated on a regular basis if values are placed in the

journal lines and journal’s settings require repeating upon a certain frequency.

### Journal Template Group Properties

This section lists the general group properties.

#### General (Journal Template Group)

Name The name of the Journal Template group. Description A short description of the Template group such as how or where it is used.

### Security

Access Group Members of this group have access to the Journal Template group Maintenance Group Members of this group have the authority to maintain the Journal Template group

![](images/design-reference-guide-ch09-p794-7960.png)

> **Note:** Click

to use the Security page to modify users and groups before assigning

![](images/design-reference-guide-ch09-p864-7962.png)

them. Click and begin typing the name of the group. As the first few letters are typed, groups are filtered making it easier to find and select the desired group.  Select the group, press CTRL and then double-click.
