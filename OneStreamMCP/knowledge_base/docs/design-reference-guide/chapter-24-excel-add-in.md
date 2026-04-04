---
title: "Excel Add-In"
book: "design-reference-guide"
chapter: 24
start_page: 1998
end_page: 2111
---

# Excel Add-In

The Excel Add-In is an alternative way to enter, update, manage, query and analyze application data, using Excel spreadsheets and workbooks. You can use the Excel Add-In to: l Query and submit data into the application. l Change the cube point of view. l Interact with forms. l Leverage multiple cube views. l Establish a live connection to cube views in the application. l Drill through to source data. l Create functions to return, amend, or submit application data. l Refresh sheets and workbooks when changes occur in the application. You can launch Excel documents from: l The Navigation pane o The OnePlace tab: Through public and user-specific access to application functionality and documents. o The Application tab: Through the Spreadsheet tool. o The System tab: Using export features for system administrators. l Workflow forms that have attached templates. l Cell or data unit attachments. l Dashboards l Cube views l File Explorer

## Connect The Excel Add-In

Before you start using Excel Add-In, you must add the OneStream ribbon to your Excel program.

### Add The OneStream Ribbon

1. In an Excel workbook, navigate to File > Options to open the Excel Options dialog. 2. Select Add-ins from the menu in the left pane. Click the Manage drop down menu at the bottom of the dialog box and select COM Add-ins and then click Go.

![](images/design-reference-guide-ch24-p2000-7515.png)

3. Click the box next to OneStreamExcelAddIn and then click OK.

![](images/design-reference-guide-ch24-p2000-7516.png)

4. The OneStream ribbon is displayed at the top of your workbook page.

![](images/design-reference-guide-ch24-p2001-7519.png)

### Log Into The Excel Add-In

1. From the OneStream ribbon in Excel, click Log In.

![](images/design-reference-guide-ch24-p2001-7520.png)

2. In the Server Address field, click the ellipsis to add or select the URL for the server.

![](images/design-reference-guide-ch24-p2001-7521.png)

3. Select an available connection or add a new connection by entering the URL and description and clicking Add. After you select a connection, click OK.

![](images/design-reference-guide-ch24-p2002-7524.png)

4. Click Connect to authenticate the server. 5. Enter your username and password and click Logon. 6. In the application field, select an application and click Open Application. After completing the authentication files and opening the application, your username and application name display in your Excel workbook, indicating that you are logged into the Excel Add-In.

![](images/design-reference-guide-ch24-p2002-7525.png)

### Update The Excel Add-In

If you use the Excel Add-In provided with the Windows application, update the Excel Add-In as part of your client update. See Client Updater. Otherwise, update the Excel Add-In by uninstalling then reinstalling it as described in the Installation Guide.

### Compatibility

OneStream makes every effort to support complex environments and maintain compatibility with other add-ins, by testing our Excel Add-in using a standard desktop deployment. There are factors outside our control which can negatively impact compatibility, so we cannot make a guarantee or warranty of service for compatibility.

## Add-In Components

The Excel Add-In contains three main components: l Task Pane: Used to quickly access POV, Quick Views, and Documents. l OneStream Ribbon: Use the OneStream menu item in the ribbon to access data from the platform. l Error Logs: Use Error Logs to troubleshoot Add-In related issues.

### Task Pane

The Excel Add-in Task pane includes the POV, Quick Views and Documents tabs as shown below. l POV: Identifies the Global POV, Workflow POV, and Cube POV. This tab is linked to the POV in your application. From here, you can make updates to the cube POV. l Quick Views: Lets you build and edit quick views in the Excel Add-in. See Quick Views. l Documents: Displays public or user-specific application or system documents. Access and manage documents with the File Explorer.

![](images/design-reference-guide-ch24-p2004-7533.png)

### OneStream Ribbon

After installing the Excel Add-In, there will be a OneStream menu item and a ribbon in Excel. The ribbon contains these categories: l OneStream l Explore l Analysis l Refresh l Calculation l Submit l Spreading l File Operations l General l Tasks

> **Note:** When working in Excel, decimals are automatically truncated after the ninth

character in a cell or a function.

#### OneStream

This displays the current user and application. A user can log off or logon to a different application by clicking this icon.

![](images/design-reference-guide-ch24-p2006-7548.png)

#### Explore

The Explore category enables you to quickly build a Quick View, Cube View, or Table View in Spreadsheet or Excel.

![](images/design-reference-guide-ch24-p2006-7549.png)

#### Quick Views

![](images/design-reference-guide-ch24-p2006-7550.png)

Create Quick View This will create a new Quick View in the worksheet’s selected cell. Copy Selected Quick View This will copy the selected Quick View in order to paste a version of it in another spreadsheet or workbook. Paste Quick View This will paste the copied quick view to the current spreadsheet or another workbook. Create Quick View Using POV from Selected Cell This will create a new Quick View based on the current POV from the selected cell.  This can be done using a Quick View cell’s POV or a Cube View cell’s POV. See Quick Views for more information.

#### Cube Views

![](images/design-reference-guide-ch24-p2007-8034.png)

Cube View Connections Create a new Cube View Connection in the selected cell. . Selection Styles Create selection styles for Cube Views. See Selection Styles for Cube Views. Manage Selection Styles This enables you to modify available selection styles.

#### Table Views

![](images/design-reference-guide-ch24-p2008-7557.png)

Select Table Views to insert a Table View into your Excel sheet.

#### Analysis

Data Attachments This pulls up the Data Attachments dialog to show existing comments or attachments on a selected cell, or to allow data attachment edits. Cell Detail Enter Cell Detail for a Cube View or Quick View data cell. See Cell Detail in OnePlace Cube Views in the Design and Reference Guide. Drill Down Drill down on a specific cell in order to see more details or gather more information. See Drill Down in OnePlace Cube Views in the Design and Reference Guide. Copy POV from Data Cell This captures the Point Of View of the currently selected cell. After clicking this option, the Paste POV As XFGetCell becomes available and the Copy POV From Data Cell goes to gray.  The ability to paste this into another cell is now available and OneStream will automatically convert this into an XFGetCell formula with all of the appropriate Parameters. Paste POV As XFGetCell This option is only available after clicking Copy POV From Data Cell. After clicking this option, OneStream will convert the copied cell into an XFGetCell formula. Click Refresh Data to retrieve the data. Convert to XFGetCells This will convert an existing Quick View into an XFGetCells. After clicking this option, OneStream will prompt with the following: Are you sure you want to convert all of the data in Quick View ‘Name of the Quick View’ to XFGetCells?  By clicking OK, the Quick View definition will be deleted and converted to XFGetCells.

#### Refresh

Refresh Sheet This pulls down updated data from the server and only refreshes the selected worksheet. Refresh Workbook This pulls down updated data from the server and refreshes the entire Excel workbook.

|Col1|Refresh Sheet|Refresh Workbook|
|---|---|---|
|**Function Behavior**|Refreshes the selected tab<br>only|Refreshes all tabs in the file|
|**Data Impacts**|Clears all dirty cells on the<br>selected tab only|Clears all dirty cells on all<br>tabs regardless of selected<br>tab|
|**Parameter Impacts**<br>**(CV Only)**|Prompts the user with any<br>Parameters used on the<br>selected tab|Prompts the user with all the<br>Parameters used in the<br>workbook|

#### Calculation

Consolidate/Translate/Calculate If permission is granted, these calculations can be performed on the selected cell.

#### Submit

Submit Sheet After editing data in Excel, click this icon to send it back to OneStream for Cube Views, Quick Views, XFSetCells, and Table Views on the active sheet. Submit Workbook After editing data in Excel, click this icon to send it back to OneStream.  This icon will send data back for every tab in the Excel workbook.

|Col1|Submit Sheet|Submit Workbook|
|---|---|---|
|**Function**<br>**Behavior**|Identifies data changes on the<br>selected table and stores these<br>changes to the database|Identifies data changes on every<br>tab and stores these changes to<br>the database|
|**Data Impacts**|Submits all data for the selected tab<br>only|Submits all data for all tabs|
|**Parameter**<br>**Impacts**<br>**(CV Only)**|No prompts|No prompts|

#### Spreading

![](images/design-reference-guide-ch24-p2011-7564.png)

Enables users to see what type of spreading was used to spread data values over several columns or rows without having to type in each cell’s values.

#### Spreading Types

Fill This fills each selected data cell with the value in the Amount to Spread property. Clear Data This clears all data within the selected cells. Factor Multiply all cells by the specified rate. Accumulate This takes the first selected cell’s value and multiplies it by the rate specified. It then takes that value, multiplies it by the specified rate and places it in the second cell selected, and does this for all selected cells.  For example, four cells are selected and the first cell has a value of 900. Even Distribution This takes the Amount to Spread and distributes it evenly across the selected cells. 445 Distribution This takes the Amount to Spread and distributes it with a weight of 4 to the first two selected cells and a weight of 5 to the third cell. 454 Distribution This takes the Amount to Spread and distributes it with a weight of 4 to the first selected cell, a weight of 5 to the second cell and a weight of 4 to the third. 544 Distribution This takes the Amount to Spread and distributes it with a weight of 5 to the first selected cell and a weight of 4 to the second and third cells.

![](images/design-reference-guide-ch24-p2012-7567.png)

The Accumulate Spreading is setup as follows with a rate of 1.5:

![](images/design-reference-guide-ch24-p2012-7568.png)

When the spreading is applied the outcome is as follows:

![](images/design-reference-guide-ch24-p2012-7569.png)

Each cell’s value is a factor of the previous cell amount. Proportional Distribution This takes the selected cell’s value, multiplies it by the specified Amount to Spread, and then divides it by the total sum of all selected cells. If all the cells have a zero value, the Amount to Spread will behave like an Even Distribution.

![](images/design-reference-guide-ch24-p2013-7572.png)

A proportional amount of 50,000 is applied to the cells:

![](images/design-reference-guide-ch24-p2013-7573.png)

Result:

![](images/design-reference-guide-ch24-p2013-7574.png)

#### Spreading Properties

Amount to Spread Specify the value to spread over the selected cells.  The value defaults to the last cell selected. The way the amount in this field spreads varies by Spreading Type. Rate (Factor and Accumulate Spreading Types Only) Enter a rate to multiply by a cell value. Retain Amount in Flagged Input Cells Users can flag specific cells in order to retain the data within the cell. If this property is set to True, spreading will not apply to the selected flagged cells. Include Flagged Readonly Cells in Totals Set this to True to include locked base-level cell values when calculating spreading totals. True is the default. Flag Selected Cells Flags selected cells so the original amount in the cell is retained during the spreading process. Clear Flags Select this to clear any flagged cells.

#### File Operations

File Explorer Use File Explorer to upload and download files.

![](images/design-reference-guide-ch24-p2015-7579.png)

![](images/design-reference-guide-ch24-p2015-7580.png)

Create Folder This creates a new folder under the selected folder on the left-hand side of the File Explorer pane.

![](images/design-reference-guide-ch24-p2015-7581.png)

Delete Selected Folder/File This deletes the selected folder on the left-hand side of the File Explorer pane or the selected file.

![](images/design-reference-guide-ch24-p2015-7582.png)

Edit Selected Folder/Edit Selected File Information This edits the Description, Maintenance Group, and Access Group for the selected folder or file.

![](images/design-reference-guide-ch24-p2015-7583.png)

Upload File This uploads the selected file and allows the user to save.

![](images/design-reference-guide-ch24-p2015-7584.png)

Download Selected File This downloads the selected file and allows the user to save.

![](images/design-reference-guide-ch24-p2016-7588.png)

Download Selected File’s Content File This downloads the selected file’s content file and allows the user to save. Save to Server Use this option to save your file to the OneStream folder structure. You can save files to any location matching your security access. If the file was opened from a OneStream location, it will be saved back to that location. If it wasn't, File Explorer will open. Save Offline Copy Use this to save an offline copy of the current worksheet without the functions. Users without the Excel Add-In can open this copy and see the saved values.

> **Tip:** To prevent removal of values and saved data from Platform Version 9.0.1 offline

Excel files, set Retain All Formatting when Saving Offline to True, or change the Excel options for Formulas > Calculation Options from Automatic to Manual.

#### General

![](images/design-reference-guide-ch24-p2016-8035.png)

Object Lookup Use the Object Lookup to insert objects from OneStream into Excel such as Foreign Exchange Rate Types when building formulas. If creating an Extensible Document in Excel, users can use the Object Lookup to insert Parameters, Substitution Variables, or Image Content. See Object Lookup in Presenting Data With Books, Cube Views and Dashboards for more details on this feature. In-Sheet Actions Create buttons to execute a Data Management Sequence, Submit data, or Refresh data, without leaving the sheet. See In-Sheet Actions. Parameters Insert a new parameter to quickly filter Cube Views and Table Views or manage existing parameters in the workbook. See Parameters. Select Member Select a Dimension Type from the drop-down list in order to view the Members of that Dimension. Select a Member of the hierarchy, and the Member name will display in the selected cell. Preferences Set Preferences for how to interact with and display your OneStream data. Settings include:

![](images/design-reference-guide-ch24-p2018-7595.png)

General l Enable Macros for OneStream Event Processing: If set to True, this enables Excel macros for OneStream API calls. The default is False. l Invalidate Old Data When Workbook is Opened: If set to True, this will force a data refresh on the opened workbook. The default is False. l Use Minimal Calculation for Refresh: This is for Excel Add-In only, not the Spreadsheet feature in OneStream Windows App. The default is True this will only calculate formulas and Excel functions in the active sheet. Set to False to revert to a full calculation of all workbooks and all sheets.

|Col1|NOTE: Performance is best when Excel is set to use Manual Calculation Mode.|
|---|---|

l Use Multithreading for Cube View Workbook Refresh: When set to True, processing steps for collecting and applying formulas to Cube Views runs concurrently for multiple worksheets, allowing for faster refresh rates. This is for Excel Add- In only, not the Spreadsheet feature in OneStream Windows App. Use Multithreading for Retain Formulas: When set to True, processing steps for refreshing Cube Views will run concurrently across multiple worksheets, allowing for faster refresh rates. This is for Excel Add-In only, not the Spreadsheet feature in OneStream Windows App. l Disable Interactive User During Refresh: Accounts for a known Excel situation when running on certain touchscreen hardware. If the Refresh Sheet or Refresh Workbook is pressed but the cells containing functions do not complete their calculations when processed, change the Disable Interactive User During Refresh setting under Preferences to True.

|Col1|NOTE: Setting this to True may result in incompatibility issues with other Excel<br>Add-ins.|
|---|---|

l Retain All Formatting when Saving Offline: This is for Excel Add-In only, not Spreadsheet. The default is False to derive basic formatting and better performance. Set this to True to obtain all character by character formatting, this will force a data refresh on the opened workbook. l Preserve Hidden Rows and Columns: This setting becomes active when the option Retain All Formatting when Saving Offline is set to True. When set to True, the rows and columns remain hidden when the workbook is saved offline. Rows and columns are visible when set to False. l Use Add-In Compatibility Filter: When True, only cell selection change events, such as keystrokes or mouse clicks, are allowed. Third-party add-ins and macros cannot change cells. When False, users, third-party add-ins, and macros can make cell changes. l Force Add-In Compatibility for Registration: When set to True, this setting forces a re- registration of the OneStream Add-In. This should be used when the Add-In is turned off on start-up by external add-ins. Quick View Double-Click Behavior l Default Expansion for Rows/Columns: This determines what level of expansion displays when a user double-clicks a Quick View Row or Column Header. NextLevel is the default setting and allows multiple expansion paths when a user double clicks a row or column header. There is also the ability to double-click an expanded item to collapse it again. This feature only works with the NextLevel setting. Default Display Settings for New Quick Views and Default Suppression Settings for New Quick Views. See Quick Views in Getting Started with the Excel Add-In. Display Context Pane To display the OneStream task pane on the right-hand side of the screen, check this box. To hide the task pane, disable the box. Default Settings for Preserve Formatting l Preserve Excel Formatting by Default (Quick Views): When set to True, formatting will be preserved by default for Quick Views. The default setting is False. l Preserve Excel Formatting by Default (Cube Views): When set to True, formatting will be preserved by default for Cube Views. The default setting is False. l Preserve Excel Formatting by Default (Table Views): When set to True, formatting will be preserved by default for Table Views. The default setting is False.

#### Tasks

Task Activity View all running, scheduled, and completed tasks. Automatic

![](images/design-reference-guide-ch24-p2021-7603.png)

The Excel Calculation icon has the option of Automatic, Automatic Except for Data Tables, and Manual. It is recommended that the Calculation be set to Manual when using OneStream] spreadsheets because the Automatic setting results in an Excel re-calculation every time a OneStream’s interactive workbook changes data (e.g., when navigating a Quick View).  However, this is not forced because a user might prefer Excel’s Automatic calculation, especially when there is not a significant amount of OneStream data in the workbook.

### Error Logs

The Excel Add-In stores error log details on your local drive in the AppData folder. This allows customers and customer support to retrieve log details when experiencing Excel Add-In related issues, resulting in decreased time to resolution. Error logs in the folder will automatically be deleted after 60 days.

## Work With Data

The Excel Add-In enables you to quickly build and work with data including through Cube Views, Quick Views, Table Views, and XF Retrieves.

### Cube Views

#### Cube View Connection Settings

When you create a new Cube View Connection, you can populate the following settings:

|Property|Description|
|---|---|
|**Name**|The name is created for you if you select the Cube<br>View first. If you type a name first and then select the<br>Cube View, the typed name is retained.|
|**Refers To**|This setting specifies the starting cell or range in your<br>spreadsheet for the Cube View. It establishes the<br>default cell or range reference for the Cube View<br>Connection.|

|Cube View|Click the ellipsis to look up and add a Cube View to<br>make a Cube View Connection with.|
|---|---|
|**Resize Initial Column Widths**<br>**Using Cube View Settings**|Selected by default, this option automatically re-<br>sizes column widths based on the settings from the<br>Cube View selected.|
|**Insert or Delete Rows When**<br>**Resizing Cube View Content**|Select this option if you plan to stack everything<br>vertically on your worksheet. This automatically adds<br>or deletes rows as a Cube View expands and<br>contracts.|
|**Insert or Delete Columns When**<br>**Resizing Cube View Content**|Select this option if you plan to stack everything<br>horizontally on your worksheet. This automatically<br>adds or deletes columns as a Cube View expands<br>and contracts.|
|**Include Cube View Header**|This option includes the header on the Cube View.|
|**Retain Formulas in Cube View**<br>**Content**|This option allows you to enter formulas in the Cube<br>View (Excel or Spreadsheet) and retain those<br>formulas before and after submission of the sheet or<br>workbook.|
|**Dynamically Evaluate Highlighted**<br>**Cells**|This option is only available if the previous option is<br>selected. This highlights a cell if the formula<br>reference has changed without having to refresh a<br>spreadsheet.|

|Preserve Excel Format|This option enables you to preserve native Excel<br>formatting changes made to Cube Views. When<br>enabled, formatting changes made via native Excel<br>formatting will be retained.|
|---|---|
|**Add Parameter Selectors to Sheet**|Selecting this option will generate parameter<br>selectors for all parameters in the selected Cube<br>View, including any dependent parameters. These<br>parameter selectors can be drop-down menus or<br>free-form cell inputs. This option must be selected<br>during the creation process and cannot be added<br>retroactively. To edit parameters added during the<br>creation of a new Cube View Connection, navigate to<br>**OneStream** >**General** >** Parameters** from the Excel<br>or Spreadsheet ribbon.<br>**NOTE:** If parameters are created using<br>Add Parameter Selectors to Sheet, they will<br>need to be manually deleted from the<br>Parameters ribbon if the associated Cube<br>View Connection is deleted. Deleting the<br>connection alone will orphan the<br>parameters that were added to the sheet.|

#### Add A Cube View To Excel

Add a Cube View to an Excel sheet using the below steps: 1. Click Cube Views > Cube View Connections.

![](images/design-reference-guide-ch24-p2007-8034.png)

2. From this window, the cube views added to an Excel workbook can be managed. You can add, remove, edit, or go to styles. Click Add, to add a new Cube View.

![](images/design-reference-guide-ch24-p2025-7612.png)

3. Name the connection and then choose the Cube View.

![](images/design-reference-guide-ch24-p2026-8036.png)

4. Populate the following fields: a. Resize Initial Column Widths Using Cube View Settings is the default setting. If you disable it, you can change the columns and save the Cube View. However, if you go back into the same Cube View Connection, the check box will be enabled and you’ll need to disable it again to keep your new cube view settings. b. Insert or Delete Rows/Colums When Resizing Cube View Content: Select whether there needs to be inserted or deleted rows and/or columns when resizing. This setting will move around other content in the sheets if the size of the Cube View changed since the last refresh. c. Include Cube View Header: Choose whether to add header rows to the spreadsheet. d. Retain Formulas in Cube View Content allows you to enter formulas in the Cube View (Excel or Spreadsheet) and retain those formulas pre and post submission of the sheet or workbook. When the sheet or workbook is refreshed the formulas will remain. If the value resulting from the value is different than the value of the OneStream database, the cell will initially become a dirty cell and will turn the cell format to yellow. See Retain Formulas in Cube View Content in Excel and Spreadsheet.

|Col1|NOTE: When using external Excel workbooks, or after any updates to<br>referenced sheets within the same workbook, you must Refresh Sheet to<br>visualize the dirty cells and then Submit Sheet, unless Dynamically<br>Highlighted Evaluated Cells is turned on.|
|---|---|

e. Preserve Excel Format: This option enables you to preserve native Excel formatting changes made to Cube Views. When enabled, formatting changes made via native Excel formatting will be retained. See Preserve Formatting. 5. Click the OK button and then the Close button to view your Cube View. After the Cube View is added, it will appear on the sheet. If formatting was applied to the Cube View, see Cube Views in Presentation, the formatting will come forward into the Excel sheet.  Otherwise, apply Excel Styles.  These styles are stored in the Excel sheet and can be copied between workbooks.  For more information on Excel Styles, see Styles. If you enabled Preserve Excel Format, your formatting will remain. See Preserve Formatting.

|Col1|NOTE: In order to copy Excel spreadsheet cells into a Data Explorer Grid on the<br>web, click CTRL, select the cells desired, and then click CTRL-C. Navigate to the<br>Data Explorer Grid, select a cell, and click CTRL-V, this will paste the cells into the<br>grid. This can also be done from a Data Explorer Grid into an Excel Spreadsheet.|
|---|---|

#### Retain Formulas In Cube View Content In Excel And Spreadsheet

Retain Formulas in Cube View Content allows you to form Cube View grids of data in Excel, using the Cube Views menu function, that can be linked to other Excel models for easy submission into OneStream. This feature allows formulas, (to writeable cells,) in Excel (or Spreadsheet) for an attached Cube View to be retained on submission and retrieval instead of being replaced with the value of the represented formula. The Retain Formulas in Cube View Content feature, allows users to plan, budget or forecast and use the familiar functionality of Excel while still submitting data back to the OneStream database. Use the Retain Formulas in Cube View Content feature to enter formulas in the Cube View (Excel or Spreadsheet) and retain those formulas pre and post submission of the sheet or workbook. When the sheet or workbook is refreshed, the formulas will remain. If the value resulting from the formula differs from the existing value in the OneStream database, the cell will initially become a dirty cell and will turn the cell format to yellow.

> **Note:** When using external Excel workbooks, or after any updates to referenced

sheets within the same workbook, you must Refresh Sheet to visualize the dirty cells and then Submit Sheet, unless you’ve turned on Dynamically Highlight Evaluated Cells in the cube view. Retain Formulas in Cube View Content links to other Excel worksheets or worksheets in other Excel workbooks. 1. From the OneStream menu, select Cube Views > Cube View Connections. 2. Click Add in the Cube View Connection window or click Edit if you already have a cube view. 3. Click Retain Formulas in Cube View Content box and click the OK button.

![](images/design-reference-guide-ch24-p2029-8037.png)

4. Add the Cube View, if one is not already selected, and click the Close button.

#### Dynamically Highlight Evaluated Cells In Excel Or Spreadsheet

When Retain Formulas in Cube View Content is enabled, the option to Dynamically Highlight Evaluated Cells becomes available to enable. When it’s enabled, every time you make a change to a cell in Excel or Spreadsheet that is referenced in a Cube View, the cell will immediately update and show the update with a change in color. This cell update is called a dirty cell, which indicates that the cell value is different from the information in the OneStream database.

![](images/design-reference-guide-ch24-p2029-8037.png)

Dynamically Highlight Evaluated Cells saves you a step because the cell changes without requiring a refresh. This feature evaluates all the cells in the spreadsheet and identifies the values in the cube view that have changed relative to its original value in the database. Excel users who want to continue working in Excel to access can log in through the OneStream menu, update the cube view content and submit it to the database without leaving Excel. You can also perform these tasks in Spreadsheet within the application. You can Retain Formulas in a Cube View Content that are related to values within a function, existing workbook, sheet, other sheets, in external workbooks, and in external renamed worksheets in Excel.  Spreadsheet also offers this functionality, but it doesn’t allow you to point the cell references to external workbooks. Click Refresh Sheet to see all changes within the cube view content and then click Submit Sheet or activate Dynamically Highlight Evaluated Cells and the cell updates automatically.

![](images/design-reference-guide-ch24-p2031-7630.png)

The number of cells with formulas in the cube view determines the amount of time it takes to update the cells. You can turn the feature on or off and only use Refresh Sheet to update the values in the cells. Changes will show very quickly, no matter the size of the worksheet, when using Spreadsheet.

#### Using Retain Formulas And Dynamically Highlight Evaluated Cells

You can also use retain formulas and dynamically highlighted evaluated cells within a cube view to automatically display updated values in an existing workbook, a sheet or sheets, external workbooks, and external renamed worksheets in Excel. You can also do this in Spreadsheet within the OneStream application, however, you can’t point the cell references to external workbooks. 1. In Excel, go to the OneStream menu and Log on. 2. Click Cube Views > Cube View Connections.

![](images/design-reference-guide-ch24-p2007-8034.png)

3. Click the Add button. 4. In the Cube View Connection window, click the ellipsis next to the Cube View field.

![](images/design-reference-guide-ch24-p2026-8036.png)

5. Select your choice and click the OK button. 6. Click Retain Formulas in Cube View Content to activate Dynamically Highlight Evaluated Cells. 7. Then click Dynamically Highlight Evaluated Cells so you can see the changes as they are made. 8. Even if you don’t activate the dynamically highlight evaluated cells feature, you can click Refresh Sheet after you make changes to see them. 9. If you’re prompted, click OK once you’ve selected the parameters for the cube view. 10. Once the cube view has been added, you can click Edit to review, if needed. 11. Make changes to the sheet and press <Enter> to see the updated cell, which will change from white to yellow.

![](images/design-reference-guide-ch24-p2033-7635.png)

12. Click Submit Sheet to automatically save changes to the database.

#### Use Cases

These use cases are for both Excel and Spreadsheet unless otherwise noted. The placing of formulas or cell references. Retain Formulas can reference the following types of formulas. In all instances the formula will stay after refresh and/or submission. Cell References of individual cells of data on the same sheet.

![](images/design-reference-guide-ch24-p2034-7638.png)

Cell References to a cell on the same sheet, factored by another value.

![](images/design-reference-guide-ch24-p2034-7639.png)

Cell References to cells on other sheets. These can also be factored by another value as well.

![](images/design-reference-guide-ch24-p2035-7642.png)

Referenced cell(s) on another saved workbook can also be factored by another value.

> **Note:** This applies to Excel only.

#### Best Practices

Well-Formed Grid It is suggested to create a “Well-Formed Grid” (Root.List or Comma Separated List) in Cube Views. When using this “Well-Formed Grid” (Root.List or Comma Separated List) in Cube Views, the Excel/SpreadSheet relative (=C2) and absolute formulas (=$C$2) will be retained. However, when using these relative and absolute formulas within an Excel and Spreadsheet formula, users can use either the cell reference or text within the formula depending upon how members will be added or removed: l =VLOOKUP(D30,Sheet1!A:B,2,FALSE) will work in a List or Comma-Separated list (Well- formed grid) when Accounts are added to the end. l =VLOOKUP("52000 - Promotions",Sheet1!A:B,2,FALSE) will work in a case when a Member of a Row is moved up or down. Member Expansion Functions When using Member Expansion Functions in Cube Views for Excel and SpreadSheet, the cell being referenced within the function (Vlookup, etc), will need to be adjusted and/or referenced as text. l =VLOOKUP("52000 - Promotions",Sheet1!A:B,2,FALSE) will work in a Dynamic or when a Member of a Row is moved. l =VLOOKUP(D30,Sheet1!A:B,2,FALSE) will NOT work in a Dynamic or when a Member of a Row is moved as this is using the cell ref of D30.

#### Other Considerations

l Deselecting the Retain Formulas for Cube View Content will eliminate all formulas that were established /existed on the Cube View grid. l Pivoting the existing Dimensions of the Cube View will break formulas. l Changing the “structure” of the Cube View grid in the rows or columns will also break the formulas. For example; If you have Account, Entity, UD3 as the dimensions used in the row and switch it to UD3, Entity, Account, it will break the formulas. l Users can change the POV to select a new dimension. This will change the Cube View results but retain the existing formulas that were established. The user at this point can choose to utilize the existing formulas, modify or delete. If the original formulas are modified or deleted, the last action will be saved. l Linking a white cell (writeable cell) to another cell in a different workbook will work ONLY in Excel and NOT in Spreadsheet. l Prior to establishing links to an external workbook, the user should save the external workbook being referenced. l When the user renames or saves as the (referenced) file, the user will need to update the links to the newly created file. Updating the links on the spreadsheet should be done BEFORE doing a refresh or submit. l Formulas with cell references (VLOOKUP, INDEX(MATCH(, etc) that return errors (#N/A, #ERROR, etc) or non-numeric data will not retain the formula and return to its original value from the Cube View ; this error text cannot be converted into a number so the formulas will not retain. l If a Dimension Member Name is renamed; i.e.; “52200 – Rent” is now “52200 – Rent Commercial”, the formula will break.

#### Creating Excel Forms And Reports

Well-designed Cube Views help you create flexible and rich Excel-based forms and reports. You can retrieve capabilities through formula, but inserting Cube Views in a spreadsheet offers a richer experience with less maintenance.

#### Named Regions

Bringing a Cube View into Excel creates several Named Regions that you can select, refer to, and use with Styles. Named Regions are created for the Cube View, column headers, row headers, and data sections. If there are multiple named columns or rows in the Cube View, go to the intersection-based Named Regions and use different formatting to differentiate sections. For example, a Total row is separated from detailed data. This combination of Named Regions and Styles generates a nicely formatted report:

![](images/design-reference-guide-ch24-p2038-7649.png)

#### Cube Views In Spreadsheet

Cube views are the primary reporting tool in OneStream to display financial data for your organizational needs. You can also use cube view reporting in the Spreadsheet and Excel Add-in using cube view connections.

#### Define Cube View Connections

Cube view connections are different from exporting a cube view to Excel because the connection remains live, meaning data can be refreshed and submitted. Before defining cube view connections: l Make sure you have the appropriate security settings to access your Cube View and the Spreadsheet page in OneStream. l Make sure your cube view is in a group within a profile that has visibility set for Excel. l If using the Excel Add-in, connect to OneStream. In Excel, from the OneStream menu, click Logon. To add a cube view to Excel or Spreadsheet: 1. Select the cell that you want to place the cube view on. The uppermost left corner of your cube view will start on the cell you have selected. 2. Select Cube Views > Cube View Connections > Add from the OneStream ribbon.

#### Configure Cube View Connections

You can configure cube view connections in OneStream from the Spreadsheet functionality. 1. Click OneStream from the Spreadsheet ribbon. 2. From Cube Views, select Cube View Connections to open the cube view connections dialog box.

![](images/design-reference-guide-ch24-p2007-8034.png)

#### Add A Cube View To Excel Or Spreadsheet

1. Click Add to create a cube view connection.

![](images/design-reference-guide-ch24-p2026-8036.png)

2. Click the ellipsis next to the Cube View field to open the Object Lookup dialog box. Object Lookup enables you to search for an existing cube view. 3. Use the Filter box to search for the name of the cube view and select it from the list of results.

![](images/design-reference-guide-ch24-p2041-7656.png)

4. Click the OK button to confirm the cube view selection. 5.The Name and Refers To fields automatically populate for the selected cube view. Click the OK button to continue. 6. Click Close to exit the dialog box. Your selected cube view is now added to the spreadsheet.

![](images/design-reference-guide-ch24-p2029-8037.png)

|Col1|NOTE: See Cube Views for more context on these properties.|
|---|---|

#### Format Cube View Connections

In Spreadsheet, you can apply standard formatting to a cell, a group of cells, or to a row or column. Check the Preserve Excel Formatting setting in the Cube View Connection dialog box, or use Selection Styles and Named Ranges to format a Cube View Connection in Spreadsheet. See Formatting in Excel.

### Quick Views

Quick views are ad hoc reports that allow you to pivot, drill back, create data sets, and design workbooks to analyze data. Quick views can also be used to submit data. You can create a quick view by: l Entering members. l Copying and modifying another quick view. Quick views function similarly to cube views. See Cube Views in Spreadsheet in Excel Add-In.

#### Manage Quick Views

The Quick Views tab displays your dimensions. You can move and replace the default Time and Account dimensions. This table identifies the tasks you can perform on the Quick Views tab.

|Button|Description|
|---|---|
||Create a quick view.|
||Rebuild a quick view created by direct data entry by selecting<br>new or modified fields.|
||Rename a quick view.|
||Delete a quick view.|
||Edit quick view properties.|

![](images/design-reference-guide-ch24-p2043-7663.png)

![](images/design-reference-guide-ch24-p2043-7664.png)

![](images/design-reference-guide-ch24-p2043-7665.png)

![](images/design-reference-guide-ch24-p2043-7666.png)

![](images/design-reference-guide-ch24-p2043-7667.png)

|Button|Description|
|---|---|
||Refresh a quick view with the latest data set.|
||Undo up to 100 changes.|
||Restore deleted changes.|
||Select members, identified by dimension type or dimension, to<br>add to a quick view.|
||Clear everything except selected items.|
||Clear only a selected item.|
||Go to the next level in the hierarchy.|
||Go to the top of the hierarchy.|

![](images/design-reference-guide-ch24-p2044-7671.png)

![](images/design-reference-guide-ch24-p2044-7672.png)

![](images/design-reference-guide-ch24-p2044-7673.png)

![](images/design-reference-guide-ch24-p2044-7674.png)

![](images/design-reference-guide-ch24-p2044-7675.png)

![](images/design-reference-guide-ch24-p2044-7676.png)

![](images/design-reference-guide-ch24-p2044-7677.png)

![](images/design-reference-guide-ch24-p2044-7678.png)

|Button|Description|
|---|---|
||Go to the parent member.|
||Go to the immediate child members.|
||Go to the branch base.|

![](images/design-reference-guide-ch24-p2045-7684.png)

![](images/design-reference-guide-ch24-p2045-7685.png)

![](images/design-reference-guide-ch24-p2045-7686.png)

#### Quick View POV

If you do not select a member, the quick view POV reflects your default POV settings. User- defined descriptions also display in the quick view POV. Hover over dimension labels to view dimension descriptions. See Application Properties and User Defined Dimensions 1-8 in the Design and Reference Guide.

#### Create And Modify Quick Views

This topic describes how to define and modify quick views.

#### Create Quick Views

Create a quick view using a sample income statement account. See Find and Add Members to Quick Views and Manage Quick Views.

> **Tip:** If a Dimension Member has an alias, you can use that in place of the member

name. For information on creating a Dimension Member Alias, see Configurable Dimensions. 1. In Cell A2 specify the row: DimensionToken#MemberName A#IncomeStatement. 2. In Cell B1 specify the column: DimensionToken#MemberName T#2018M1. 3. Highlight the dimensions you entered and click Create Quick View. If you are using the Excel Add-in, click the Quick Views tab.

![](images/design-reference-guide-ch24-p2046-7691.png)

4. In the Create Quick View window, click OK. You can create quick views using the Quick Views menu on the OneStream ribbon. 1. On the OneStream ribbon, click the Quick Views drop-down menu. 2. Click Create Quick View and one will populate in the cells below.

|Col1|NOTE: When Preserve Excel Format is enabled on Quick Views, structural<br>changes such as drill down, will not adjust formatting automatically. Manual<br>reformatting may be required when modifying Quick Views.|
|---|---|

To add members to your new quick view, manually enter the dimension tokens into the cells or add them using the Quick View toolbar. 1. In cell B1 specify the column: DimensionToken# MemberName S#Actual 2. In cell C1 specify the column: DimensionToken#Member Name S#BudgetV2 3. Click on the cell where your quick view rows will begin. In this example, specify column A2. 4. Click Select Member from the quick view toolbar to the right of the screen.

![](images/design-reference-guide-ch24-p2047-7694.png)

5. From the Select Member dialog box, move dimensions to include in your quick view to the Result List using the single arrow. Click OK.

![](images/design-reference-guide-ch24-p2047-7695.png)

6. Highlight the cells with your data and click Create Quick View.

![](images/design-reference-guide-ch24-p2048-7698.png)

You can create a quick view by copying an existing quick view or expanding on the data in a quick view cell. 1. Select an existing quick view. 2. On the OneStream ribbon, click the Quick Views drop-down menu. 3. Select Copy Quick View.

![](images/design-reference-guide-ch24-p2048-7699.png)

4. Select any cell within the current sheet, workbook, or another workbook.

|Col1|NOTE: You can only paste a copied Quick View into the same workbook when<br>using Spreadsheet.|
|---|---|

5. Click Quick Views drop-down menu and select Paste Quick View.

|Col1|TIP: Paste any quick view by right-clicking > Quick View > Paste Quick View. See<br>Right-Click Options.|
|---|---|

To create a quick view based on the POV from a single cell in an existing quick view: 1. Select an intersection of data in an existing quick view. 2. On the OneStream ribbon, click the Quick Views drop-down menu. 3. Select Create Quick View Using POV From Selected Cell.

![](images/design-reference-guide-ch24-p2049-7705.png)

#### Modify Quick Views

The following section outlines how to rebuild quick views after adding or removing dimensions and members.

> **Tip:** If a Dimension Member has an alias, you can use that in place of the member

name. For information on creating a Dimension Member Alias, see Configurable Dimensions. Use a comma-separated member list in column and row cells to build the quick view. In this example, enter: 1. DimensionToken#Root: A#60999,64000,63000,65200 in A2 2. DimensionToken#Root: T#2018M1,2018M2,2018M3 in B1 3. Select the updated area and click Create Quick View. 4. Enter a quick view name and click OK. The quick view is displayed:

![](images/design-reference-guide-ch24-p2050-7709.png)

Use the member expansion functions in cells to refine the quick view. For example, enter: 1. DimensionToken#Root A#Root.Children in A2 2. DimensionToken#Root T#2018.Base in B1 3. Select the updated area, click Create Quick View and specify a name.

![](images/design-reference-guide-ch24-p2050-7710.png)

Use the member expansion functions to enter comma-separated members in cells to finalize the quick view. For example, enter: 1. DimensionToken#Root A#Root.List(60999,64000,63000,65200) in A2 2. DimensionToken#Root T#2018M1 in B1 3. Select the updated area and click Create Quick View. The quick view updates:

![](images/design-reference-guide-ch24-p2051-7713.png)

> **Note:** Check the suppression options if items do not display as expected.

#### Extend A Quick View

To add a dimension or new dimension member to a quick view, enter # before the specified dimension or member names. If the dimension is already used, # is not required. 1. Specify the data to use. For example, enter: Dimension type, such as DimensionToken#MemberName S#Actual 2. Select the area and click Rebuild Quick View.

![](images/design-reference-guide-ch24-p2051-7714.png)

The updated quick view is displayed:

![](images/design-reference-guide-ch24-p2051-7715.png)

#### Change Dimension Members

You can switch dimension members by entering another member name. You do not need to specify a dimension token if you switch to members in a dimension that is being used. For example: 1. Enter these members: 60999, 43000, and 61000 . 2. Select the members and click Rebuild Quick View.

![](images/design-reference-guide-ch24-p2052-7718.png)

Members 60999, 43000, and 61000 replace 60000 and 54500 as shown in the updated quick view:

![](images/design-reference-guide-ch24-p2052-7719.png)

You can change from one dimension to another by entering comma-separated members in the same cell. In the example below, the Account dimension is changed to the Entity dimension. 1. Above the Time dimension enter the following, separated by commas: l E#HoustonHeights l South Houston l Operating Sales A#60000

![](images/design-reference-guide-ch24-p2053-7722.png)

2. Select the area and click Rebuild Quick View. Operating Sales columns are created for the Houston entities:

![](images/design-reference-guide-ch24-p2053-7723.png)

You can add to an existing dimension's members by entering comma-separated member names in the same cell. If names are numeric or start with an apostrophe, insert a space after each comma. 1. In the same cell, enter the following members. Insert a space after each comma because the member names are numeric. l 60999 l 43000 l 61000 2. Select the area and click Rebuild Quick View.

![](images/design-reference-guide-ch24-p2054-7727.png)

Members 60999, 43000, and 61000 are added to the existing member list:

![](images/design-reference-guide-ch24-p2054-7729.png)

#### Change Dimensions

Dimensions can be replaced by modifying row and column headers with the updated dimension information and rebuilding the quick view. 1. Click the dimension to be changed, then specify the dimension to use. As shown in the example below, you could: a. Replace Account with an Entity such as E#HoustonHeights. b. Add OperatingSales A#60000 above Time. 2. Select the area and click Rebuild Quick View.

![](images/design-reference-guide-ch24-p2055-7732.png)

The updated quick view will look like this:

![](images/design-reference-guide-ch24-p2055-7733.png)

#### Using "Keep Only" When Extending A Quick View

When modifying a quick view, select the rows and columns you want to remain unchanged using the Keep Only option. This allows you to clear a quick view without losing selected data. 1. In the row below an existing dimension, add these members: 60000, 61000, and 54500. 2. Select the area and click Keep Only.

![](images/design-reference-guide-ch24-p2055-7734.png)

After updating, the quick view will look like this:

![](images/design-reference-guide-ch24-p2056-7737.png)

#### Reverse Member Display Order

You can double-click to reverse the display order of members by specifying Quick View Double- Click preferences. 1. Select Administration > Preferences. 2. In Quick View Double-Click, set Default Expansion for Rows to TreeDescendantsR.

![](images/design-reference-guide-ch24-p2056-7738.png)

#### Format And Suppress Data

Use the following to manage and customize your quick view display: l Row Header/Columns Header Text Types: Reformat columns and rows. l Suppress Repeating Member Names: Display or hide repeating member names. For example, if False, the repeating name displays in each row. l Data Style: Apply a provided or custom data style. See Styles. l Suppress Invalid Rows/Columns: Hide cells that have invalid data. l Zero Suppression Threshold: Suppress all values that are less than a number you specify, for rounding purposes. For example, specifying 499.99 suppresses any value less than that, such as 499.19.

#### Find And Add Members To Quick Views

You can add multiple members to a quick view, filtering and searching as necessary.

> **Note:** To add members while preserving the current quick view column, select the

columns to fill outside the current quick view range. After adding members, select the new column range and click Rebuild Quick View. 1. Select a dimension, then click Select Member. 2. On the Select Members dialog box, perform a task: l To find members not shown in the hierarchy, click the Search tab, enter the member name, select members, and then click Add Selected Items. l To select multiple members, press Ctrl or Shift, selecting the members, and then click Add Selected Items.

![](images/design-reference-guide-ch24-p2058-7744.png)

3. Select the area to which the members were added and click Rebuild Quick View.

### Table Views

A Table View definition for the Windows Application Spreadsheet Tool is defined in a Business Rule.  The Administrator designing the rule can define the rows and columns which should be returned to the Spreadsheet from the source table presented in the Table View. The Table View Business Rule can collect data from multiple data sources. For example, a single Spreadsheet worksheet can display a Table View which collects data from two or more sources. The Administrator has full control over the write back “save” process through Business Rules. When designing the Table View Business Rule, the BRAPI Authorization functions should be designed into the Business Rule to control access to the viewing or modifying the data.  This can be applied to the entire table or to specific cells. A workbook can contain multiple Table Views. These can be on the same worksheet or across worksheet pages. A single Business Rule file can be used to define multiple Table Views by calling the Business Rule argument, TableViewName. Additionally, a single named range can be used to manage table data cells within the Spreadsheet using user defined named ranges (XFTV_*).

> **Note:** When browsing for Table Views Business Rule, only Spreadsheet Type Rules

will be displayed in the Object Lookup dialog.

#### Table Views In Excel

Insert a Table View into Excel. 1. Click Table Views to open the Table View Definition dialog box. 2. From this window, manage the Table Views within Excel. Add, remove, edit, and refresh tables. Click the Add button to add a new Table View.

![](images/design-reference-guide-ch24-p2059-7747.png)

3. In the Table View Definition dialog box, populate the following fields: a. Name: Table View name. b. Refers To: Cell range where the Table View will exist. c. Table View Business Rule: Click the ellipses to search for and select a business rule. d. Insert or Delete Rows When Resizing Table View Content: Select this option if you plan to stack everything vertically on your worksheet. This automatically adds or deletes rows as a table view expands and contracts. e. Insert or Delete Columns When Resizing Table View Content: Select this option if you plan to stack everything horizontally on your worksheet. This automatically adds or deletes columns as a cube view expands and contracts. f. Preserve Excel Format: This option enables you to preserve native Excel formatting changes made to cube views, quick views, and table views. When enabled, formatting changes made via native Excel formatting will be retained. See Preserve Formatting. 4. Click the OK button, then the Close button to view the Table View in the Excel worksheet.

#### Table Views Spreadsheet Control

Table Views is a OneStream Windows Application only spreadsheet control.  The primary purpose of Table Views is to provide an easier method to creating Dashboards where relational data is required. The use of Table Views in Spreadsheet enables the designer to work in a more flexible environment to design a form or data collection tool. Accessed via the OneStream Windows Application Spreadsheet, the Table Views provide a client-based tool to support Dashboard forms. Table Views are not intended as an alternative to other tools, such as the SQL Table Editor or Grid Viewer, Dashboard Components.

#### Key Use

l Tool kit item for advanced OneStream Solution or dashboard designers l Designed to collect records from relational tables, or other sources l Present the information in the Spreadsheet format l Utilize client-side functionality, found in the Spreadsheet tool, such as calculations and pick-list validation lists l Update table records only

#### Design Considerations

l The current functionality is designed to update records in target tables l Does not perform inserts, to create new records l Controlling elements must be designed into the Table View Business Rule by the creator to ensure data integrity, security and performance

#### Table View Size Considerations

l Spreadsheet support of Table Views depends upon the number of rows and row content l The Spreadsheet Control does not support paging, therefore all rows and content must be returned l Performance testing and design expectations is to support approximately 8000 KB of data per Table View.

### Parameters

Generate a drop-down menu in a Cube View or Table View column based on selected parameters in the Excel Add-In or Spreadsheet to quickly filter your data. This functionality eliminates the need to manually create drop-down menus. When a parameter in the drop-down menu is changed, the worksheet will refresh to show the updated output. Parameters that are dependent on one another will updated automatically when the related parameter is altered.

> **Note:** See the Table View Business Rule Example in the Table Views Guide to set up

your business rule code to generate a nested parameter in a column in a Table View.

#### Supported Parameters

The following types of parameters are supported: l Literal l Input l Delimited l Bound l Member

#### Unsupported Parameters

The member dialog parameter is not supported.

#### Create A Parameter

Follow these steps to create a parameter driven drop-down menu for a Cube View: 1. Select the cell to display the drop-down menu. 2. In the Excel ribbon, navigate to OneStream > General > Parameters.

![](images/design-reference-guide-ch24-p2063-7758.png)

3. In the Parameters dialog box, click the Add button. 4. In the Parameter Selector Definition dialog box, populate the following fields:

![](images/design-reference-guide-ch24-p2063-7759.png)

l Parameter Name: Click the ellipsis to search for and select the parameter. The name is automatically populated based on the selected parameter.

|Col1|NOTE: It is best practice when creating parameters that the names be<br>unique, even across Workspaces. See Best Practices in Create a<br>Parameter in the Design and Reference Guide.|
|---|---|

l Workspace Name: Name of the application workspace of the parameter. This field automatically populates when a parameter is selected. l Refers To: Cell or range that will display the button. l Use Display Value: The Use Display Value setting enables you to choose which item to use in your drop-down menus. In Application > Workspaces > [...] > Parameters, you can configure Display and Value Items. For example, if you configure the following settings: o Display Items: MI, OH o Value Items: Michigan, OH Then, if Use Display Value is selected, the Display Items (MI and OH) will display in the drop-down menu. If it is clear, the Value Items (Michigan and OH) will display. l On Change Refresh: When you switch parameters, refresh the worksheet or workbook. Select Nothing to keep the sheet as is. 5. Click the OK button, then click the Close button on the Parameters dialog box.

#### Parameter Best Practices

l When working with parameters in Excel and Spreadsheet, remember that nested parameters and input parameters need to be consumed by a Cube View or Table View. Using nested, standalone parameters may generate undesirable results. l When adding dependent parameters to a worksheet manually, remember that parameters must be added in the order in which their sequence must be resolved. When using Add Parameter Selectors to Sheet, the parameters will automatically be added in their sequential orde.r

|Col1|Example: Year must be added before Month. Region<br>must be added before Sub-Region.|
|---|---|

|Col1|IMPORTANT: For dependent parameters to refresh correctly when the parent<br>parameter is changed, the Workspace containing the Cube View or dependent<br>parameter within the Spreadsheet or the Excel Add-In must be sharable. To<br>enable this, navigate to Application > Workspaces > Sharing and ensure Is<br>Sharable Workspace is set to True.|
|---|---|

### In-Sheet Actions

In-sheet actions enable you to quickly interact with your data directly in Excel and Spreadsheet. Creating a button streamlines the following actions: l Refreshing a worksheet or workbook l Submitting a worksheet or workbook l Running a data management sequence

#### Create In-Sheet Button

Build an In-Sheet Action button using these steps: 1. Navigate to the Excel worksheet you would like to place your In-Sheet Action. 2. Select a cell.

|Col1|NOTE: Make any In-Sheet Action button larger by selecting multiple cells. If a<br>button consists of more than one cell, when opened in Spreadsheet, the button<br>will revert to a single cell.|
|---|---|

3. From the ribbon, select OneStream > General > In-Sheet Actions.

![](images/design-reference-guide-ch24-p2016-8035.png)

4. In the In-Sheet Actions dialog, click the Add button. 5. In the next In-Sheet Actions dialog, populate any applicable fields: a. Label: Button text to show in cell. b. Refers To: Cell or range where the button will appear. c. Submit: Submits the Worksheet or Workbook after clicking the button. Select Nothing from the drop-down menu if you do not want to make a submission. d. Data Management Sequence: Use your In-Sheet action button to run a data management job. Click the ellipsis to browse for and select a data management sequence. e. Workspace Name: Populates with the name of the Workspace after you select a data management sequence. f. Parameters: Designate explicit values for parameters needed for your data management sequence or leave blank to resolve parameters based on sheet selections. Enter the parameter name and the value it should be set to at run time, for example, myparam = value. When adding multiple parameters, separate each parameter with a comma. For example, myparam=value1, myparam2=value2. If the value for your parameter contains a space, use brackets in your syntax. For example, myparam=[South Houston].

|Col1|IMPORTANT: Parameters can only be used if they have been resolved in<br>an existing Cube View or Table View in the sheet or workbook. To use<br>parameters that don't exist in your Cube View or Table View, add it<br>anywhere in the Cube View in OneStream, for example in the page caption<br>field or other unused field that does not impact Cube View rendering. For<br>Table Views, add the parameter to the GetCustomSubsVars section of the<br>Spreadsheet business rule.|
|---|---|

|Col1|NOTE: In-sheet actions will not prompt for unresolved parameters.|
|---|---|

g. Do not wait for task to finish: Enable this setting to continue working while the data management sequence runs in the background. h. Refresh: Refreshes the Worksheet or Workbook after clicking the button. Select Nothing from the drop-down menu if you do not want to make a submission. i. Text Color: Choose a custom button text color and input an eight character hex code, or select a color from the drop-down menu. j. Background color: Choose a custom button background color and input an eight character hex code, or select a color from the drop-down menu. 6. Click the OK button and then the Close button. Your button will appear in the cells you selected.

#### Format In-Sheet Actions

By default, In-Sheet Action buttons will display as black with white text. After initial creation, customize the display of your buttons by formatting them with custom colors and styles. Modify your button using the steps below: 1. From the ribbon, select OneStream > General > In-Sheet Actions. 2. In the In-Sheet Actions dialog box, select the button to format and click the Edit button. 3. Use the Button Theme drop-down menu to select a color.

![](images/design-reference-guide-ch24-p2068-7770.png)

4. Click OK and then Close to save changes.

### Right-Click Options

When working with data in the in Excel Add-In, right-click and navigate to OneStream to see the following available actions:

#### Cube Views

Cube View Connections Create a new Cube View connection. Selection Styles Set up a selection style. See Selection Styles. Manage Selection Styles Modify an existing selection style. See Selection Styles.

#### Quick View

For a complete overview on Quick Views, see Quick Views. Create Quick View This will create a new Quick View in the worksheet’s selected cell. Copy Quick View This will copy the selected Quick View in order to paste a version of it in another spreadsheet or workbook. Paste Quick View This will paste the copied quick view to the current spreadsheet or another workbook.P Create Quick View Using POV from Selected Cell This will create a new Quick View based on the current POV from the selected cell.  This can be done using a Quick View cell’s POV, a Cube View cell’s POV, or a XFGetCell's POV. For information on creating a Quick View using a POV from the Drill Down page, see Drill Down in "Using OnePlace Cube Views" in the Design and Reference Guide. Expand Select the cell of a Member and choose how to view its data. Select from these options: l AllTops: This returns the Top of the given Dimension. l AllBase This returns all Base Members of the given Dimension regardless of what Member is selected. l All This returns all Members in a given Dimension. l NextLevel This returns the next level of Members under the selected Member. l KeepOnly This will only keep the selected Members. l Parents This returns the direct Parents of the selected Member regardless of how many hierarchies to which the Member belongs. l Ancestors This returns all Members up the chain from the selected Member. l Children This returns the first level of Children under the selected Member. l ChildrenInclusive This returns the selected Member and its first level of Children. l Descendants This returns every Member under the selected Member in a list, not a hierarchy. l DescendantsInclusive This returns the selected Member and every Member under it in a list, not a hierarchy. l TreeDescendants This returns every Member under the selected Member in a hierarchy. l TreeDescendantsInclusive This returns the selected Member and every Member under it in a hierarchy. l Base This returns the Base level for the selected Member. Paste POV Enables you to Paste a POV into a selected cell in order to change the data within that Quick View. Apply POV from Selected Cell This enables a POV to be passed from a selected cell within a Quick View. Apply User POV This enables a Point of View to be passed between Quick Views. Clear POV This clears the POV for the selected Quick View. For the following properties, see Manage Quick Views l Undo l Redo l Options l Refresh

#### General OneStream Right-Click Options

Calculate, Translate, Consolidate If permitted, these calculations can be performed on the selected cell. These function the same as the icons in the Ribbon, but you have the choice to do Force operations and additional Logging. Select Member Select a Dimension Type from the drop down list in order to view the Members of that Dimension. Select a Member of the hierarchy, and the Member name will display in the selected cell. Copy POV from Data Cell This captures the Point of View of the currently selected cell. After clicking this option, the Paste POV As XFGetCell becomes available. Paste POV as XFGetCell This option is only available after clicking Copy POV From Data Cell. After clicking this option, OneStream converts the copied cell into an XFGetCell formula. Convert to XFGetCells This converts an existing Quick View into an XFGetCells. Allocate See Allocation in Using OnePlace Cube Views in the Design and Reference Guide. Data Attachments This opens the Data Attachments dialog so you can view existing comments or attachments on the selected cell, or to edit data attachments. See Data Attachments for Selected Cell/Data Unit in Using OnePlace Cube Views in the Design and Reference Guide. Cell Detail See Cell Detail in Using OnePlace Cube Views in the Design and Reference Guide. Cell POV Information This gives a detailed summary of the selected Members related to this intersection as well as the full Member Script used to get this value. All the major properties of these Members can be seen from this dialog. Cell Status This returns a long list of properties about a given cell. Navigate Enables you to launch a cube view or dashboard from Excel when configured. See Linked Cube Views and Linked Dashboards in the Design and Reference Guide. Drill Down See Drill Down in Using OnePlace Cube Views in the Design and Reference Guide. Spreading This enables users to enter data into an aggregate Member, like an annual time period, and spread values over several columns or rows without having to type in each cell’s values. Select from the following options: l Spread Selected Cells: See Spreading Types. l Flag Selected Cells: Flags selected cells so the original amount in the cell is retained during the spreading process. l Clear Flags: Select this to clear any flagged cells.

### Retrieve Functions

Retrieving and changing data can be done by using functions. To see the functions and their Parameters, open Excel and select the Formulas tab. Select Insert Function and select OneStreamExcelAddIn.XFFunctions where it says to Select a category. The output of the function will look something like this: =XFGetCell(A1,A2,A3,A4,A5) The equivalent functions like XFGetCell provide a separate Parameter to specify each Dimension Member without using the Member Script syntax. (e.g., E#CT:A#Sales would not be used, CT and Sales would be used in the correct Parameter for that Dimension) Here are the main functions to use:

> **Note:** If a field within the function is not needed, enter a double quote to ignore it.

XFGetCell This function retrieves data based on the Parameters supplied. Each Parameter needs to be defined. Example: XFGetCell(NoDataAs Zero, Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8) XFGetCell5 This has the same functionality as XFGetCell except it limits the User Defined Dimensions to five instead of eight. XFGetFXRate This function retrieves rates from the system. Each Parameter needs to be defined. XFGetFXRate(DisplayNoDataAsZero, FXRateType, Time, SourceCurrency, DestCurrency) XFGetCalculatedFxRate This function directly retrieves an exchange rate even if only the inverse rate exists in the system. XFGetMemberProperty This function retrieves any Dimension Member property from the Member Properties tab in the Dimension Library. Note there are no spaces used when defining property name. Example: XFGetMemberProperty(“DimTypeName”,“MemberName or Script”,“PropertyName”, “VaryByCubeTypeName”,“VaryByScenarioTypeName”,“VaryByTimeName”)

> **Note:** If the function does not need to vary by Cube Type, Scenario, or Time, enter a

double quote in order to ignore it. Example: Retrieving Currency for the Houston Entity XFGetMemberProperty(“Entity”,”Houston”,”Currency”,””,””,””) Example: Retrieving an Account Formula that only occurs in the Budget Scenario XFGetMemberProperty(“Account”,”51000”,”Formula”,””,”Budget”,””) Example: Retrieving the Short Description Property for Time Dimension XFGetMemberProperty("Time","2015M8","ShortDescription","","","") XFGetRelationshipProperty This function retrieves any Dimension relationship property from the Relationship Properties tab in the Dimension Library. XFGetRelationshipProperty(“DimTypeName”,“ParentMemberName or Script”,“ChildMemberName or Script”,“PropertyName”,“VaryByScenarioTypeName”,“VaryByTimeName”)

> **Note:** If the function does not need to vary by Cube, Scenario, or Time, enter a double

quote in order to ignore it. Example: Retrieving a Flow Members Aggregation Weight XFGetRelationshipProperty(“Flow”,”TotalBalance”,”Total Movement”,”AggregationWeight”,””,””) Example: Retrieving an Entity’s Percent Consolidation for July, 2015 XFGetRelationshipProperty(“Entity”,”Houston”,”South Houston”,”PercentConsolidation”,””,”2015M7”) XFGetHierarchyProperty This function determines whether or not a Dimension has children and returns True or False. Example: XFGetHierarchyProperty(“DimTypeName”,”DimName”,“MemberName or Script”,“PropertyName”,”PrimaryCubeName”,”ScenarioTypeNameForMembers”,”MergeMember sfromReferencedCubes”) Example: Retrieving Child  Hierarchy XFGetHierarchyProperty("entity","HoustonEntities","Houston Heights","HasChildren","Houston","Actual",FALSE) XFGetDashboardParameterValue This function is available to Excel Add-in and Spreadsheet. If that function is used within an XLSX file that is using a function like XFGetCell or XFSetCell (or similar) where these are referencing a custom parameter value (e.g. ParamEntity) that is on the Dashboard that references this Spreadsheet from within it as a Component. The practice to get this Custom Parameter value is to use XFGetDashboardParameterValue to fetch the text from that Parameter or its default value and place it in a cell on the Spreadsheet (e.g. B1). Then the cell that is using a retrieve function such as XFGetCell would reference this other cell (i.e. B1). XFGetDashboardParameterValue("myParamName", "Text For Default Value") XFGetMemberInfo This function retrieves the description in the system.  Each Parameter needs to be defined. XFGetMemberInfo(MemberInfoType, DimTypeName, MemberName, NameorDesc, NameandDesc) XFInternalGetDataFromServer This function returns True or False.  It does not take any arguments. XFSetCell This function saves data to the amount field based on the Parameters supplied.  Each Parameter needs to be defined. XFSetCell(CellValue, StoreZeroAsNoData, Cube, Entity, Parent, Cons, Scenario, Time, View, Account, Flow, Origin, IC, UD1, UD2, UD3, UD4, UD5, UD6, UD7, UD8 XFSetFXRate This function saves rates to the system.  Each Parameter needs to be defined. XFSetFXRate(Value, StoreZeroAsNoData, FXRateType, Time, SourceCurrency,DestinationCurrency) XFGetCellUsingScript XFGetMemberInfoUsingScript XFSetCellUsingScript All of the functions that have UsingScript are based on a Member Script (e.g., A#Sales:E#Texas). The multiple Parameters provide the ability to specify multiple portions of the full Member Script using different Excel cells. All of the Member Scripts in the function Parameters combine to create one Member Script. It will then use the combined Member Script to retrieve the data cell. Example:=XFGetCellUsingScript (TRUE,"GolfStream","E#Frankfurt:C#Local:S#Actual:T#2022M1:V#YTD:A#10100:F#None:O#T op:I#Top:U1#Top:U2#Top:U3#Top: U4#Top:U5#None:U6#None:U7#None:U8#None","","")

![](images/design-reference-guide-ch24-p2077-7794.png)

> **Note:** Add quotation marks to POVs not being used.

The following examples detail uses of XFGetCellUsingScript.

![](images/design-reference-guide-ch24-p2078-7797.png)

Example: =XFGetCellUsingScript(TRUE,"GolfStream", "E#[Houston Heights]:C#Local:F#None:S#Actual:T#2017Q2:V#YTD:A#69000:"&D3,"","") D3= “U6#None:U7#None:U8#None:O#Top:I#Top:U1#Top:U2#Top:U3#Top:U4#Top:U5#None”

![](images/design-reference-guide-ch24-p2078-7798.png)

Example: =XFGetCellUsingScript(TRUE,"GolfStream", "E#[Houston Heights]:C#Local:S#Actual:T#2017Q2:V#YTD:A#69000:F#None:O#Top:I#Top:U1#Top: U2#Top:U3#Top:U4#Top:U5#None:U6#None:U7#"&B7&":U8#"&B7,"","") B7=”None”

![](images/design-reference-guide-ch24-p2079-7801.png)

Example: =XFGetCellUsingScript (TRUE,$B$21,$C$22&$C$23&$C$24&$C$25&$C$27&$B$35,"A#"&$E27,"T#"&G$26) XFGetCellUsingScriptEx XFGetMemberInfoUsingScriptEx XFSetCellUsingScriptEx All of the functions that have …Ex have many more Parameters to use for combining Member Scripts (e.g., commonly used when creating another version of a function that has extra Parameters). Ex would also be used to combine many Member Script Parameters. XFSetCellLocalForms XFGetCellLocalAdjInput5 XFGetCellLocalForms5 XFGetCellLocalImport5 XFGetCellLocalOTop5 XFGetCellTransAdjInput5 XFGetCellTranForms5 XFGetCellTransImport5 XFGetCellTransOTop5 XFSetCellLocalForms5 These functions use the Consolidation and Origin Dimensions. For example, XFSetCellLocalForms is using Local Consolidation and the Forms Origin Member. The number five at the end of the functions limit the User Defined Dimensions to five instead of eight. XFGetCellUSingScriptVolatile XFGetCellVolatile XFGetFXRateVolatile XFGetMemberInfoUsingScriptExVolatile XFGetMemberInfoUsingScriptVolatile XFGetMemberInfoVolatile XFSetCellUsingScriptExVolatile XFSetCellUsingScriptVolatile XFSetCellVolatile XFSetFXRateVolatile In some cases, Excel requires a volatile function for proper refreshing, for example, some Excel Charts that reference calculated cells. XFInternalPrepareCalculationStep XFInternalSendDatatoServer XFInternaSetConnectionInfo All of the functions that begin with XFInternal only work for internal processes.

### Xf Retrieves

In an existing quick view, you can convert to formula-based retrieves (XFGetCell) on intersections of data using the Convert To XFGetCells option in the top ribbon toolbar.

![](images/design-reference-guide-ch24-p2081-7806.png)

After selecting Convert To XFGetCells, OneStream displays the following prompts: l Are you sure you want to convert all the data in Quick View ‘Name of the Quick View’ to XFGetCells? l By clicking OK, the Quick View definition will be deleted and specific intersections of data will be converted to XFGetCell formulas. This is a great way to set up a lot of formulas at once. However, these will hardcode all the dimension members in your formulas. If the member name is being displayed, the rows and columns can be synchronized to the pre-made quick view and cube view rows and columns. Your quick view can be adjusted to display the member name instead of the description through the quick view options.

> **Note:** Quick view data that has been converted to XFGetCell cannot be converted

back into a quick view. However, quick view content may be saved as a template before converting. Additionally, XF Functions can be manually entered: 1.Select a cell and click on the insert function icon. 2.Select an XFGet function from the pop-up menu.

![](images/design-reference-guide-ch24-p2082-7809.png)

3. Fill out the necessary dimensions to build your XF Retrieve. If you do not want to fill one out, place a “” in the property and it defaults to the data in your cube POV.

![](images/design-reference-guide-ch24-p2082-7810.png)

4.Click OK to save the dimensions entered and the selected cell data changes to #REFRESH. 5.If your query is in the correct syntax, data populates in the cell after refreshing the sheet.

> **Note:** If a member name has been misspelled, the cell returns an error calling out the

first misspelled member name.

#### Absolute Cell References

Absolute cell references are a functionality in the Excel Add-in that allows an exact cell reference to be kept when you copy the formula to another cell. To do this, put a $ in front of the portion of the cell reference that you want to keep. For example, if you want to keep the entire A1 cell reference to copy to another cell you would change the cell reference to $A$1. If you only want to keep the column, the cell reference would be $A1 and if you only want to keep the row it would be A$1.

#### Relative Cell References

Relative cell references are the default state of cells in an Excel workbook. The reference is relative to the location of the cell. Formulas copied to new cells retain the location information and apply it to the new set of cells. For example, if you reference A1 in cell C1 and then copy the formula into D1, the results will be a reference to B1 in cell D1 because the formula is referencing 2 columns before.

### Visual Basic For Applications (Vba) Procedures

A Sub procedure is a series of Visual Basic statements that perform a specific task. It can take arguments, such as constants, variables, or expressions that are passed by a calling the procedure. If a Sub procedure has no arguments, the Sub statement must include an empty set of parentheses. A Sub procedure begins with a Sub statement, followed by the tasks to be performed, and ends with an End Sub statement. The following snippet of VBA code represents the structure of a Sub procedure: Sub [ProcedureName] (Arguments) [Statements] End Sub OneStream Functions are used with Sub procedures to automate data submission from XFSetCells formulas.

> **Note:** To use these functions in Excel, you must have the latest version of OneStream

and its corresponding Excel add-in installed. Procedures currently supported are: l LogonAndOpenApplication - Login varies depending on the authentication type setup for your application:

|Col1|NOTE: Resource Owner Password Credentials (ROPC) is no longer supported.<br>As a result, the following ProcessSSOAuthenticationAndCreateToken has been<br>deprecated. See Legacy Single Sign On (SSO) for Bearer Token procedure.|
|---|---|

Sub SSOLogon() Set xfAddIn = Application.COMAddIns("OneStreamExcelAddIn") If Not xfAddIn Is Nothing Then If Not xfAddIn.Object Is Nothing Then ssoToken = xfAddIn.Object.ProcessSSOAuthenticationAndCreateToken ("https://golfstream.onestreamtest.com/OneStreamWeb", "user1@mycompany.com", "P@$$w0&D") xfUserName = xfAddIn.Object.GetXFUserNameFromSSOToken(ssoToken) o OneStream IdentityServer (OIS) - (url, PAT, application) Supports the use of Personal Access Tokens. Refer to the Identity and Access Management Guide for creating and managing PATS. Sub OIS_PAT_Logon() Set xfAddIn = Application.COMAddIns("OneStreamExcelAddIn") If Not xfAddIn Is Nothing Then If Not xfAddIn.Object Is Nothing Then ' OIS using PAT loggedIn = xfAddIn.Object.LogonAndOpenApplicationWithToken ("https://yoursite.onestreamcloud.com/OneStreamWeb", InsertPersonalAccessToken, "GolfStreamDemo_v36") MsgBox ("Is Logged In : " & loggedIn) End If End If End Sub o Legacy Single Sign On (SSO) - (url, ssoToken, application) Supports the use of Bearer token from your identity provider Sub SSOLogon() Set xfAddIn = Application.COMAddIns("OneStreamExcelAddIn") If Not xfAddIn Is Nothing Then If Not xfAddIn.Object Is Nothing Then ' Legacy SSO via Bearer Token Dim ssoToken As String ssoToken = <obtain Bearer token from your Identity Provider>" loggedIn = xfAddIn.Object.LogonAndOpenApplicationWithToken ("https://yoursite.onestreamcloud.com/OneStreamWeb", ssoToken, "GolfStreamDemo_ v36") MsgBox ("Is Logged In : " & loggedIn) End If End If End Sub o Native Authentication (Self-hosted) - (url, user, password, application) Support login via userid and password Sub Native_Logon() Set xfAddIn = Application.COMAddIns("OneStreamExcelAddIn") If Not xfAddIn Is Nothing Then If Not xfAddIn.Object Is Nothing Then ' Native - UserName & Password loggedIn = xfAddIn.Object.LogonAndOpenApplication ("https://yoursite.onestreamcloud.com/OneStreamWeb", UserName, Password, "GolfStreamDemo_v36") MsgBox ("Is Logged In ): " & loggedIn) End If End If End Sub l Logoff() l RefreshXFFunctions() -- refer to the following example: Sub RefreshXFFunctions() Set xfAddIn = Application.COMAddIns("OneStreamExcelAddIn") If Not xfAddIn Is Nothing Then If Not xfAddIn.Object Is Nothing Then Call xfAddIn.Object.RefreshXFFunctions End If End If End Sub l RefreshXFFunctionsForActiveWorksheet() l RefreshQuickViews() l RefreshQuickViewsForActiveWorksheet() l RefreshCubeViews() l RefreshCubeViewsForActiveWorkSheet() l ShowParametersDlg() l ShowParametersDlgForActiveWorksheet() l SubmitXFFunctions () -- Automates the data loading process and eliminates the need to open the Excel files individually and submit data manually. Using a VBA routine, files with XFSET functions that are linked to other cells, sheets, and files can be programmatically submitted to OneStream. This procedure calls only XFSetCells. Refer to the following example: Sub SubmitXFFunctionsTest() Set xfAddin = Application.COMAddIns("OneStreamExcelAddin") If Not xfAddin Is Nothing Then If Not xfAddin.Object Is Nothing Then Call xfAddin.Object.SubmitXFFunctions Call xfAddin.Object.RefreshXFFunctions End If End If End Sub

## Formatting In Excel

These formatting options are available in Excel: l Preserve Formatting: Quickly preserve native Excel formatting in Cube Views, Quick Views, and Table Views when you submit or refresh your worksheet. l Styles: Create custom styles to consistently format elements, like cells and headings. l Conditional: Set styles for conditional text and data within your Excel worksheet. l Selection Styles for Cube Views: Bring a Cube View into Excel, and format it using native

## Excel Formatting.

### Preserve Formatting In Excel

When you build a Cube View, Quick View, or Table View you have the option to enable the Preserve Excel Format setting. This setting enables you to preserve local Excel formatting without the need to create custom, conditional, or selection styles. When enabling preserve formatting on Cube Views or Table Views, we recommend disabling the Insert or delete rows or columns when resizing cube view content.

> **Note:** When Preserve Excel Format is enabled for Table Views, the DataType column

must be specified as numeric. See DataType Object for Column Fields in the Table Views Guide.

#### Enable Preserve Excel Format

To turn on the Preserve Excel Format setting, create a Quick View, Cube View, or Table View. In the respective dialog box, enable the Preserve Excel Format setting.

![](images/design-reference-guide-ch24-p2026-8036.png)

Cube View

![](images/design-reference-guide-ch24-p2089-7829.png)

Quick View

![](images/design-reference-guide-ch24-p2089-7830.png)

Table View On already created views, you can edit the view and enable the setting. When enabled, changes made to the sheet will be kept, even after you save and refresh your sheet. Additionally, you can copy your view and associated formatting will remain on the duplicated view when the Preserve Excel Format setting is enabled on the source Cube View, Quick View, or Table View.

> **Note:** When Preserve Excel Format is enabled on Quick Views, structural changes

such as drill down, will not adjust formatting automatically. Manual reformatting may be required when modifying Quick Views.

### Styles

The same standard Styles are used in Excel, however, if you want to create a new style in order to change the format of how the numbers are displayed, see the example that follows.

#### Creating A Custom Style

The following example was created in the Excel 2010 Version. On the Home Tab, click on the new style sheet.

![](images/design-reference-guide-ch24-p2090-7833.png)

Select New Cell Style.

![](images/design-reference-guide-ch24-p2090-7834.png)

A new style window will appear, click Format.

![](images/design-reference-guide-ch24-p2091-7837.png)

The format page will then display, click Custom.

![](images/design-reference-guide-ch24-p2092-7840.png)

Finally, under Type, enter the custom formatting. This example will be formatting for Millions. (#,,)

![](images/design-reference-guide-ch24-p2093-7843.png)

Click OK.

![](images/design-reference-guide-ch24-p2094-7846.png)

Type in a Style Name and click OK. Now that a new style sheet has been created in Excel, it can be assigned to a Quick view. This is the Quick View before the formatting:

![](images/design-reference-guide-ch24-p2094-7847.png)

![](images/design-reference-guide-ch24-p2095-7850.png)

To add the formatting, click on the Edit Quick View Options in the Quick View Tab on the right side of the screen.

![](images/design-reference-guide-ch24-p2096-7853.png)

This is the Quick View after the update:

![](images/design-reference-guide-ch24-p2097-7856.png)

### Conditional Formatting

Use Conditional Formatting in Cube Views to visually explore and analyze data. You can highlight cells or ranges of cells, identify key values, and represent data using data bars, color scales, and icon sets that correspond to specific variations in the data. If there are any existing formats prior to applying Conditional Formatting, they will be retained if the range of cells containing the conditional formats do not meet the conditions of the rule. All styles from the cube view and the selection styles that had been previously applied to that range are overridden by conditional formatting.

#### Create Conditional Formatting In An Existing Or New Cube

View 1. Click Home > Conditional Formatting > Highlight Cells Rules > Greater Than.

![](images/design-reference-guide-ch24-p2098-7860.png)

2. Enter 6000 and click OK.

![](images/design-reference-guide-ch24-p2098-7861.png)

3. Go to OneStream and click Refresh Sheet to see that your changes have been applied. 4. If you make a change that is different than the value in the database, the cell will change to pale yellow, until you refresh or submit.

![](images/design-reference-guide-ch24-p2099-7864.png)

5. If you submit, it will revert to the formatting that was in the cube view since it is no longer greater than 6000.

![](images/design-reference-guide-ch24-p2099-7865.png)

6. If you make a change to a cell that has conditional formatting and a selection style,

![](images/design-reference-guide-ch24-p2099-7866.png)

when you submit, it will convert back to the selection style since it is no longer greater than 6000.

![](images/design-reference-guide-ch24-p2100-7869.png)

7. To add icons, go Home > Conditional Formatting > Icon Sets and select the icons to use, in this example, select the arrows.

![](images/design-reference-guide-ch24-p2100-7870.png)

8. The icons are part of the cube view.

![](images/design-reference-guide-ch24-p2101-7873.png)

9. You can also create, edit, delete, and view all conditional formatting rules.

![](images/design-reference-guide-ch24-p2101-7875.png)

10. If you save the workbook, the conditional formatting is saved.

### Selection Styles For Cube Views

#### Using Selection Styles

You can bring an existing cube view into Excel or Spreadsheet and format using functionality that is included in Excel, to create highly formatted reporting. You can also modify it locally as well as in spreadsheet. You can use existing cube view formatting, add new styles to apply changes to rows, columns, or cells, or a combination of existing styles with new styles, and add conditional formatting.

#### Creating A Selection Style

1. Log in using the OneStream menu. 2. Click Cube Views > Cube View Connections. 3. Click the Add button. 4. Select the cube view, click the OK button, then the Closebutton.

![](images/design-reference-guide-ch24-p2103-7880.png)

5. Select the cell, a group of cells, a column, or row to format.

![](images/design-reference-guide-ch24-p2103-7881.png)

. 6.Right-click to open Format Cells or use the Home menu to choose formatting.

![](images/design-reference-guide-ch24-p2104-7885.png)

7. After formatting, click OneStream > Cube Views > Selection Styles. 8. Enter a Name and Range to apply to the current Selection, Entire Row, or Entire Column. Then click OK.

![](images/design-reference-guide-ch24-p2104-7886.png)

9.The formatting is applied to cube view.

![](images/design-reference-guide-ch24-p2105-7889.png)

10. Save the file to save the formatting to the cube view.

#### Using An Existing Style

1. Select the cell, a group of cells, a column, or row to format, then click Cube Views > Selection Styles. 2. Click Excel Styles to select a style. If a style is not selected the Default Cube View Format is used.

![](images/design-reference-guide-ch24-p2105-7890.png)

3. On the style palette, hover to view the styles.

![](images/design-reference-guide-ch24-p2106-7893.png)

4. Click the style to use and then Apply or OK. 5. Save the file to save the cube view formatting.

#### Adding A Selection Styles Shortcut

In Excel, right-click Selection Styles then click Add to Quick Access Toolbar. This adds a shortcut to your toolbar that launches the Selection Styles window.

![](images/design-reference-guide-ch24-p2106-7895.png)

#### Reviewing Styles And Ranges

1. Click Cube Views > Manage Selection Styles. 2. Review your styles that are applied to the cube view. 3. Modify selected styles.

![](images/design-reference-guide-ch24-p2107-7898.png)

4. Enable or disable Active styles as needed and click OK. 5. Click Apply to preview the changes, or OK to apply the changes.

![](images/design-reference-guide-ch24-p2108-7901.png)

6. In Selection Styles, you will see styles that are no longer Active. Click Activate to enable them. 7. You can delete a style from the cube view but it will be available in the current workbook.

![](images/design-reference-guide-ch24-p2108-7902.png)

8. Save the file to save the cube view formatting.

#### Using Right-Click Menu Options

You can manage selection styles by right-clicking in a cube view and selecting OneStream > Cube Views > Cube View Connections > Selection Styles > Manage Selection Styles.

#### Modifying And Duplicate Styles

1. In the Home menu, click Styles and right-click a style. 2. Click Modify or Duplicate and then Format. 3. Make any necessary changes and click OK.

![](images/design-reference-guide-ch24-p2110-7907.png)

4. Save the file.

#### Merging Styles

You can use styles created in other workbooks in Excel only. 1. Open a new workbook. 2. Add a cube view. 3. Click Home > Styles then the More arrow.

![](images/design-reference-guide-ch24-p2111-7910.png)

4. Click Merge Styles, select the style, and click OK. The style is now available in the workbook.

![](images/design-reference-guide-ch24-p2111-7911.png)
