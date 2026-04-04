---
title: "Chapter 6 - Data Integration"
book: "foundation-handbook"
chapter: 6
start_page: 192
end_page: 213
---

# Data Integration

Originally written by John Von Allmen, updated by Joakim Kulan

![](images/foundation-handbook-ch06-p192-2132.png)

The ability to consume data from any data source, and load or integrate data into software, is probably the biggest problem most software companies do not consider when creating an application. Most software packages usually only have one way to load a data file, or force a specific format (in order to get a file into the system, or a separate product), and there is usually only one way to get the data out in order to reuse the data. That concept really changes with the use of OneStream. With OneStream, you receive a very powerful toolbox to help manage and coordinate all your data. You will learn in this chapter that there are many different options to get data into the OneStream system, and that you can reuse and leverage both the source and target data that was created by OneStream. This chapter will not cover using the data parser and business rules, but cover the concepts of how to get and use the data into OneStream. OneStream has a wide variety of tools to load and map any dataset, and there is virtually no limitation on the type of data that can be consumed and analyzed or reported in OneStream. Not only does OneStream help coordinate disparate data sources, but it becomes a data source itself to reuse and repurpose data for other processes such as account reconciliation or other reporting or analysis needs. MarketPlace solutions (found on the Solution Exchange) such as People Planning, allow you to collect data for planning and place it into tables consistently, instead of having hundreds of different Excel files doing different functions. Account reconciliation (RCM inside OFC) allows you to use the same data for the trial balances to do the reconciliations. Analytic Blend allows for collecting much more granular data inside OneStream as well, in a relational table, with ties to the cube metadata and power to do aggregations and advanced data transformation. Then, to help report on this data, there are some really powerful FDX (Fast Data eXtract) APIs available to use. OneStream becomes a complete ecosystem of data.

## Data Quality

The creators of OneStream have been at the forefront of data quality since the early 2000s. UpStream Software was created by OneStream’s founder, Tom Shea, to load Hyperion Enterprise and provide an audit trail and mapping tool. UpStream gave everyone a consistent, repeatable process to load data, and provided gratification in the form of a digital ‘fish’ when they completed a step. You may ask, why a fish? Because the data flows upstream from the local general ledgers to corporate, just like a fish swims upstream. UpStream was also a tool for both accountants and complex integrators. While other ETL tools required consultants and others to rebuild the same basic platform repeatedly to get data into systems, UpStream already had the workflow process done, and the steps were easy for basic accountants and administrators who wanted to bring up new datasets quickly. There were products from Hyperion that would take a file and map it and prepare output, but – in that process – you lost all visibility as to what the output data used as its mapping rule, and how many line items were summed into the final amount. In 2006, with over 300 successful customers and the need for an audit trail – required by auditors and customers – Hyperion purchased UpStream Software and rebranded it Financial Data Quality Management (FDM). The main problem FDM was tasked with solving was the moving of data between Actual and Forecast. How do you take data from Hyperion Financial Management (HFM) and move it into Essbase, and take data from Essbase and move it back into HFM? This disconnect was part of the problem that OneStream wanted to solve and has solved with one platform of OneStream. You can use extensibility to have different levels, and metadata in Actual and Forecast, and have everything in one product. You no longer have to group and maintain two different products with two different metadata structures. Early in 2010, when building the platform from the ground up, the data quality process was reimagined. With years of experience, consistent problems could be solved easily by using settings and making them part of out-of-the-box functionality for a basic administrator to use and consume data files. The common tasks that required scripting engine assistance were reimagined to be solved with settings. The power of a scripting engine still exists, and is still used as a solution to solve the most complex problems. Most of the problems were solved because data resided in one system and didn’t have to go back and forth between separate consolidation and planning systems.

## Staging (Data Quality Engine)

All of the data being loaded into OneStream starts out in a file, in a system, or in a database table. The challenge is to get that data out of disparate sources and into one common system. The data parsing engine of OneStream consumes the data from the source files and systems, and places the data into staging tables. The staging tables have the consumed source data, the mapping used, and the target dimension. These tables are the basis of the audit trail and the drill back. The data is cleansed to be in a consistent format for consumption by the cube, or used in other areas of the product, such as reporting. When FEMA gets ready to help people after a natural disaster, they have staging areas – places they work from to coordinate and manage the response. OneStream does the same thing by having a staging area, which we will refer to throughout this chapter as Stage. The source data that is prepared and staged, and outputted in this system, can be reused in any format to solve any complex data problem. The aggregation of the source data can then be reused for another purpose. One process needs data aggregated one way, but the financial or planning dataset needs the same data aggregated in a different way. It’s the same data, just looked at in a different slice. Through every stage of data consumption, there is a complete log and audit trail that satisfies the completeness objectives of any audit system.

![Figure 6.1](images/foundation-handbook-ch06-p194-2145.png)

## Non-Stage Data Tables

Not every record of data gets processed through the mapping engine. There are a number of datasets for other solutions that are kept outside the cube in data tables created by solutions such as People Planning or Analytic Blend. (It is an option to design a process where OneStream can write to and from the data quality engine.) Data collection tables are just database tables that are created to consume data. Excel files can be set up to load these datasets as well. This process is built into solutions, and they will come with predefined Excel templates for loading the data. There is no mapping of these items except for the type of data matching to the column. The same theory applies to staging as we create areas to prepare data for use as a consistent data structure.

### Direct Loads (Non-Stage)

When loading data into the financial cube that requires less audit trail, such as forecasting data, flash data, data used for what if purposes or whatever the use case might be, you can opt to load data through all the standard engines as a normal import, less the overhead of storing source information in the Stage tables. Yes, this will not give you full audit capabilities, so should not be used for regular monthly reporting, but it will allow you to load data quicker into the cube from the source.

## Origin Dimension

In order to understand how data is consumed by a OneStream cube, it is necessary to understand the Origin dimension. The Origin dimension is the way OneStream segments the data for each entity – into separate buckets – by three types of dataset. This dimension combines the concepts of data protection and data layering and isolation. The Origin dimension acts as a barrier to protect how data is loaded in different forms. Data loaded through import does not write over data in the form’s Origin member by isolating the three groups. • Import – data imported from a flat file or query or an Excel template. • Forms – data inputted manually or via an Excel template or an Excel add-in. • Journal Entries – OneStream journal entries that can be inputted manually, used as templates, or loaded through Excel.

![Figure 6.2](images/foundation-handbook-ch06-p195-2152.png)

## Assembling The Data Sources And Considerations

For any OneStream project, it is extremely important to understand that data needs to complete the project’s objectives in the design document. Analysis of the data sources is required to make sure they can provide the data that the cube design requires. As the model of the cube is designed, the cube should be at least 90% complete before mapping and work on the data integration begins (e.g., the work in the Stage should not begin until the prior work is substantially done). That’s not to say there will not be changes. There are always changes to a model! Your job is to minimize these changes with a thoughtful and thorough design process. After the design document is complete, there are four major considerations when starting the data integration project.

### Inventory Files And Sources

Inventory files and sources are typically part of the project where a lot of the unknowns reside: • How does each group prepare the data? • How much of it is a system? • How much is collected manually? In managing a project, the pitfalls are in the details. It is extremely important to put together an inventory and get as much information as possible from all parties. Samples of the data files, the queries, and the data itself, for example. It is important to understand the analytic model to make sure the data provided can satisfy the objectives of the reporting cube. The inventory should contain ledger information, type of data, and responsible party. The customer must give you the file or the query to prepare the data. You can typically find some economies of data sources and mapping in this exercise. Do the ledgers across the company use the same account numbering scheme? Are the output formats the same structure? A company grown by acquisition typically has a wide variety of data sources that don’t match up. The most important field of any data source is the amount. When reviewing a dataset, the amount is the most important field in any file format that is used and will determine if a record is accepted or rejected. Amount(s) can be in a single column (tabbed format) or a matrix format. OneStream can easily read either type of file – out-of-the-box – without business rules. Single Column (Tabbed)

|Sales Product|Amount|
|---|---|
|Golf Club|1,000|
|Golf Shirt|1,500|
|Golf Balls|100|
|Golf Bags|800|

Figure 6.3 Matrix Format

|Col1|Col2|Col3|Col4|Col5|
|---|---|---|---|---|
|**Products**|**Store A**|**Store B**|**Store C**|**Store D**|
|Product A|23|45|31|25|
|Product B|87|23|55|38|
|Product C|64|56|62|26|
|Product D|37|32|91|8|
|Product E|93|35|54|43|

Figure 6.4 OneStream can use any file produced by a system. There is generally no reason to pay a programmer to pull data into a specific file format for OneStream. OneStream is not just limited to files; the more common practice now is a direct connection to pull data in real time. The ability to load from a direct connection is more accurate because you can refresh by just pushing a button. The file is limited to the last time the file was run. If changes are made, the file must be rerun and then reloaded, but with a direct connection, the consumption is instantaneous. OneStream is a pull system and will pull data from databases, etc. We do not allow for a push of data to our data tables. There are many different types of data to be loaded in OneStream. The dataset should be complete for the subject the system intends to analyze. Sometimes, a trial balance might not be a complete dataset that includes all the data needed to generate a cash flow statement. An example of a deficiency in a trial balance might include fixed assets which are not detailed in the trial balance. The transactional detail could be loaded, or an input form could be used to separate the detail of the account – showing additions, disposals, and transfers. Examples of data sources which are included but which are not inclusive: • Trial balances or trial balance by department • Income statements by department or geography • Accounts payable detail • Accounts receivable detail • Fixed asset details • Forecast • Vendors • Sales by region or product • Products by SKU#

> **Tip:** Source files or trial balances should be in the natural sign of debit and credits.

• Makes it very clear what value is a debit and a credit • The file balances to zero and can be easily proven with a rule • Can use a derivative translation rule or a balance account in OneStream to test mapping for signs equaling zero

### Historical Data

The second major data integration consideration with any project is that historical data is always an unknown and a challenge to reconcile. As part of the inventory, it’s important to know how the data is going to be loaded (e.g., which method or data source). • Where is that data source coming from? • How much history is going to be reconciled? Two years of historical data is typical in an implementation project when working with only a few sources and a straightforward mapping. As I have learned – over and over again – there are many special ‘one-time’ cases. You need your customer’s involvement and participation in this to tie out the data. This is a good task for the customer to focus on, and take responsibility for, as they know their data and results better than a consultant. A common issue is that the customer has their normal day-to-day job, and your needs can get in the way. Ask your customer to provide dedicated resources for this process; work as a team but make the customer responsible for approving and reviewing the data tie out. You will probably need to leverage OneStream Excel templates in a lot of these exercises to compare and tie out data.

### How Much Data?

The third major data integration consideration in any project is a balancing act on how much data is in the ledger and how much data is in the cube. This requires discipline by the designer of the cube not to recreate everything that is in every ledger system. The cube should be summarizing data and should not be a data warehouse of every possible product combination. There is the capability to drill back to a source system. If you have set up a direct connection, then it can drill back to the source items and see what makes up a number. Customers tend to want to make a data warehouse out of a cube, but there is no need to make a cube a data warehouse. If everything has one-to-one mapping, that might be a clue that the ledger is being fully replicated. There isn’t a need to replicate the ledger or every single ledger the customer owns. OneStream is not a general ledger system. There are alternative solutions, including drill back or Analytic Blend reporting, so that the cube is not overburdened as a data warehouse. OneStream can report off data that is not contained in its ecosystem. It can create a data adaptor and pull that information into a report for presentation purposes.

### Performance Of The Data Load

The fourth major data integration consideration is the volume of data and tuning behind processing times and mapping. What works best for the data file, mapping, and scripting is sometimes a bit trial and error. Loading 2,000 records with mapping is different to loading 1 million records with mapping. There are several things to consider when looking at performance.

#### The Quality Of The Data File Or Query

Sometimes, the query is a stored procedure, which is probably very tight; and sometimes the query might be a manual query that is not as efficient. We have seen queries that try to do math in the query, and they may (or may not) be the answer. There might be economies in massaging the query to become more efficient.

#### The Complexity Of The Mapping Rules

This will be covered in more detail later in the chapter when mapping processing costs are discussed.

#### Spreading The Data Load

With large data record sets, it might make sense to separate the data into smaller chunks of data. One data load with 1 million records could take ten minutes because it’s a sequential data load. The parser is sequential, but by breaking it up, the parser can be used by multiple data load and workflow processes. The same data could be loaded in two minutes by breaking it up into five separate groups of records. Splitting the data by business units (entities in OneStream) is the most common approach here.

### Working Through The Problems

Both data sources and the mapping process, during any build, involve an element of trial and error. The first item to concentrate on is getting the data read. For that, you need a basic map. The `Import` function on the workflow does both imports and mapping. This requires a data source and  a mapping. The basic transformation map for view, scenario, and time, and a transformation rule profile with at least the dimensions being loaded from the data format having a blank profile for all the other dimensions being used, is required before a data file can be loaded. It’s okay not to do all the mapping at first. Reading data sources and mapping go together, but once you get the data source working, it’s easy to modify the data source to become tighter. Look at the data that is being imported into the Stage. If you have a 10-digit string account number, such as `2300028932`, where `23000` is the account, `28` is the geography, `932` is the product. If you  only need the account information to map, it might be good to only take the first five digits and make the mapping easy as a one-to-one. But if the dimension needs the other information, it also might be good to break the dimension up and add separators such as `23000_28_932`. This will allow some flexibility with wild card  mapping. Maybe the geography isn’t needed, though, and `23000*_9*` gets mapped to a specific account.  The separators can help the mapping by saying every account with `23000` and a product `9` goes to a  specific account. Sometimes, you just have to root around and try things out to see how data and mapping are structured, and the number of records. These combinations change all the time during build. Look to make things easy but adaptable for the future. An alternative solution could be to use a composite mapping rule that looks to the source of the product dimension or as a lookup table. Those solutions may make sense based on the volume and complexity of data. There is always more than one way to solve a problem. Large datasets that contain more than 1 million records may require some adjustment to the Workflow Profile setting: • `Cache Page Size` set to `500` • `Cache Pages In Memory Limit` set to `2000`. This combination allows more cache  pages to be processed in memory. You may have to adjust settings to find the right combination based on hardware availability. • `Cache Page Rule Breakout Interval` set to `0`. This setting can increase  performance by determining if all the transformation rules have been satisfied within a cached page, thus stopping further transformations happening in that cycle by not having to continue through the remaining transformation rules. Default is `0`, meaning all  transformation rules will be evaluated for all records in all cached pages.

![Figure 6.5](images/foundation-handbook-ch06-p199-2175.png)

## Data Load Basics Of Analytic Blend

All the data resides in OneStream and can be used (or be reported) by a data blend.

![Figure 6.6](images/foundation-handbook-ch06-p200-2181.png)

The table above is a snapshot of the entire ecosystem, and how to get and reuse data in OneStream. Each Origin dimension has its option to load data.

### Data Source Types

• Fixed files – standard repeatable data files that have text items in specific segments of information in the file. • Delimited files – files that have data separated by items such as a comma, semi-colon, or another character. • Connectors – format that connects to a database, an ODBC connection, or consumes a file prepared by a connection (such as XML) from a Rest API. • Data management export sequences – format that pulls data from other areas of OneStream. For example, pulls data from a MarketPlace solution or another cube. • Excel template – this could be a manual input or a link to one or more spreadsheets. Can be used for imports, forms, or journals. • Manual input – creates a form for data to be inputted manually. This form can be downloaded from a form in Excel and inputted. • Excel add-in – this could be a manual input or a link to one or more spreadsheets; it uses formulas to load to specific data cells. • Derivative rules – creates additional records that can be used for checking a trial balance, creating offsets or allocations. The rules can be done on source data or target data after mapping. • OneStream cubes – data prepared in the cube for consolidation and eliminations, which can be extracted and reused. • MarketPlace solutions – data in these add-on platform solutions can be extracted and put into a OneStream cube. • DB Tables – specific tables set up for Analytic Blend. The OneStream ecosystem embraces all this data, which can be reused and consumed for other projects and other purposes. Indeed, there is no limit to the data that can be used and reused in the ecosystem!

## Data Parser And Business Rules

A complete book could be written detailing data parser uses and the unlimited extended capability of business rules, but this book will leave that to the training classes and a future book. The data parser in OneStream changes based on what type of flat file is being used. Whether fixed or delimited, it has out-of-the-box capabilities to do tabbed and matrix data loads without business rule scripts. Business rules allow a full scripting engine to complete complex tasks in the dataset that cannot be done out-of-the-box. The next few pages will summarize the types of formats.

### Fixed Files

Fixed files typically have some sort of header(s), have data spaced out over the file, and are generally easy on the eyes. It’s usually a system report of some kind that is created in the source system and printed to a file. Business rules can be applied to complex fixed files to extend the capabilities of OneStream software.

![Figure 6.7](images/foundation-handbook-ch06-p201-2187.png)

### Delimited Files

Delimited files are just data separated by a character. Be careful that the delimiter does not have a use/function in a field, though. Sometimes, a customer name or an account name could be using a comma or one of the other delimiters in the name. Take a company name like New Company, Inc. The data field could have a comma and a period in it, and if used as a delimiter, it can throw off the entire file. Take the numbers in the following example. This comma-delimited file was probably generated by an Excel file, as the numbers are set in double quotations. The parsing engine of OneStream can recognize this, and there won’t be an issue with the additional commas inside the double quotation marks. OneStream will read them as amounts. Common file reading problems are addressed in a lot of OneStream’s out-of-the-box functionality, and business rules applied to complex, delimited files can extend the capability of the software.

![Figure 6.8](images/foundation-handbook-ch06-p202-2194.png)

### Connectors

Connectors are used to connect to database tables with SQL queries. ODBC connections, XML files, Webservice API’s, and a host of other possibilities are applicable. Below is a brief sample of some data from a QuickBooks trial balance. These formats require the use of business rule scripts. The use of business rules can read any dataset, and consume it based on fields.

![Figure 6.9](images/foundation-handbook-ch06-p202-2197.png)

### Data Management

Data management jobs are internal OneStream processes that can pull data as a source from another area of the product, such as a cube (plus data that was calculated within the cube), by creating data management profile steps and sequences.

![Figure 6.10](images/foundation-handbook-ch06-p202-2196.png)

## Additional Data Input Sources

### Excel Templates

For a lot of companies, Excel is the software that captures and collects a lot (if not most) data. Excel is familiar to just about everyone, and users can easily prepare data in spreadsheets. Data can be consumed by placing information into a template with header tags and a range name, and then be loaded through a workflow. The data source must set the Allow Dynamic Excel Loads property to True in order to import Excel data to the Stage. But, based on the table, data can be loaded to other members of the Origin dimension. Excel files can have multiple range names in a file and multiple tables. The column headings can be done one time on a top row and applied across all the records.

![Figure 6.11](images/foundation-handbook-ch06-p203-2202.png)

The Excel format is set up with tags on the first line and range names. The range names have specific meanings.

|XFD|Data load to Stage|
|---|---|
|XFF|Form data|
|XFJ|Journal data|
|XFC|Cell details|

Figure 6.12

### Forms

Forms are used for manual inputs. Sometimes, some information is not included in the source data or needs some detail. The example shown here is just for headcount. The creation of forms and their use in workflow will be discussed in another chapter.

![Figure 6.13](images/foundation-handbook-ch06-p204-2208.png)

### Adjustments (Journal Entries)

One best practice is to put all journal entries into the source system and reload, but in some cases, the ability to have journal entries in the system aids tasks like consolidating entries that have no ledger home.

![Figure 6.14](images/foundation-handbook-ch06-p204-2210.png)

## Workflow Data Protection And Layering

The concepts of layering and data protection are used in OneStream to separate and close off data areas from one another. Reloading a Data Unit is typically based on entity, as an entity is usually the main way that data is loaded into a cube. A number of concepts, such as workflow, data sources, and Origin dimension, come together in this diagram to show how they all work together. Data (in the import dimension) is isolated from the forms and the journals. Data in the import dimension can be isolated and protected by the source ID and/or the workflow channel. The workflow channel can use an account-level dimension member – such as product – to help separate the Data Unit into an isolated data load.

![Figure 6.15](images/foundation-handbook-ch06-p205-2217.png)

![Figure 6.15](images/foundation-handbook-ch06-p205-2215.png)

The source ID is a field in the data source that allows a field to be tagged to determine how to reload data. The workflow channel can be another tool used to parse, based on one of the account- level dimension members. At one company, each plant produced more than one product and had different finance people for each product (and they planned separately). The Workflow Chapter will cover the use of the workflow channel, adding another layer of data protection. A commonly reused source ID is the name of the data file. The next figure is an example of a snippet editor that can be added to the system from the MarketPlace, and which can extend the capabilities of the business rules with pre-written code snippets.

![Figure 6.16](images/foundation-handbook-ch06-p206-2222.png)

![Figure 6.17](images/foundation-handbook-ch06-p205-2215.png)

This figure shows how a single entity in a workflow can be carved up to reload and segment data for the entity. The Origin dimension creates a separation of data. Data reloading can be separated by different source IDs in one workflow and have multiple channels.

## Origin By Layer Of Data Protection

![Figure 6.18](images/foundation-handbook-ch06-p207-2230.png)

## Organizing And Naming Convention

With data sources, business rules, translation rules, form templates, and journal templates, it is important to think about naming conventions thoroughly. Make any naming convention consistent and understandable. Use prefixes or suffixes. Because this platform can create cubes and share dimensions from the library, certain parts can be reused and placed in a profile and used again and again. A data source based on a ledger system can be reused if it’s the same ledger and format. The same format can be reused multiple times instead of creating one by entity, or the same query could be used by having a dynamic change based on some workflow variables that will substitute variables into the query to pull differently based on workflow. We can use substitution text strings in the workflow to change how a query gets used.

![Figure 6.19](images/foundation-handbook-ch06-p207-2232.png)

There are also some global transformation rules that can easily be reused. The dimension rules for scenario, time, and view should be named as global because they can be reused over multiple cubes. These three dimensions can only have one-to-one mapping. So, make it simple, if you can, and only have one group for each one if they don’t overlap with different strings. The mapping is usually a simple actual to actual. Be careful about dimension mapping, though; if there is mapping that can be reused, then name it accordingly so you can add it to more than one profile. You can rename certain items without penalty. As you continue to build your application, and want to keep consistency in your naming convention as well as naming convention changes, you can modify some of the items easily to adapt to your naming convention. It’s important to understand what each piece of the puzzle is.

|Col1|Col2|Col3|
|---|---|---|
|**Item**|**Rename/Locked**|**Naming Example**|
|Data Sources|Rename|Entity (Michigan), Workflow<br>(Cleveland Plant), General Ledger<br>(SAP, Sage, etc.)|
|Transformation<br>Dimensions|Rename|Global Scenario<br>A_Sage<br>A_Cleveland|
|Transformation Rule<br>Profiles|Rename|Entity (Michigan), Workflow<br>(Cleveland Plant), General Ledger<br>(SAP, Sage, etc.)|
|Parser Business Rules|Locked (could change<br>name and recreate but<br>would have to be<br>reattached)|XFR_Sage_Account<br>XFR_Cleveland_Product|
|Form Template Groups|Rename|Data Entry (Data Entry)|
|Form Template Profiles|Rename|DE_GL|
|Forms|Rename (v8.0 and<br>newer)|A_AR, B_PPE, C_LTDebt|
|Journal Template Groups|Rename|JE_Elimination|
|Journal Template Profiles|Rename|JE_EntityA|
|Journals|Rename (v8.0 and<br>newer)|JE_IntercoElim|

## Attributes

There are 13 mappable dimensions, but there are also extra fields to assist in capturing other source data that can be used for other purposes. These include label dimensions, 20 attribute dimensions, and 12 attribute value dimensions. These fields can be activated on the cube and used for collecting source data; while not important for the data cube, they could be used to analyze and report on records such as SKU numbers or product numbers.

## Business Rules

Business rules enhance the ability to read a file. If a file can be read without them, don’t use them unless they benefit the mapping process. Business rules might be used to carve up a string, or join strings together when combining items for mapping. Use a `–` or `_` to separate items.

## Transformation Rules

Transformation rules are rules that map a source member to a specific target value. There are different types of transformation rules and usage examples. Think about data and needs before mapping begins. Usually, mapping is provided by a group that doesn’t have mapping expertise but who can get lucky if the users have experience or proper training. Typically, there are economies that can be gained by reviewing the types of mapping and figuring out the best use of mapping. The following table is just a recap of the different types of mapping available. Derivatives are additional rules that can be used to create records as check figures or supplementary records for mapping. Some examples include: a sum of all records to make sure the trial balance balances, testing the sign of a record or records, and the allocation or creation of offsets. Using derivatives is only one way of allocation; there are other allocation methods that can be done at the target.

|Col1|Col2|Col3|Col4|Col5|Col6|
|---|---|---|---|---|---|
|**Transformation**<br>**Rule Type**|**Transformation**|**Definition**|**Definition**|** Example**|** Example**|
|**Transformation**<br>**Rule Type**|**Rule Type**|**Rule Type**|**Rule Type**|**Rule Type**|**Rule Type**|
|Source<br>Derivative|Source<br>Derivative|Applied logic & math<br>to inbound source data|Applied logic & math<br>to inbound source data|<br>Does trial balance equal zero? Does an asset go<br>negative and need to be reclassed as a liability?|<br>Does trial balance equal zero? Does an asset go<br>negative and need to be reclassed as a liability?|
|One-to-One|One-to-One|Explicitly mapped<br>members (Scenario,<br>Time, and View can<br>only use these rules)|Explicitly mapped<br>members (Scenario,<br>Time, and View can<br>only use these rules)|Actual -> Actual<br> 05/31/2021 -> 2021M5<br> Monthly -> MTD<br> 23099 -> 23000|Actual -> Actual<br> 05/31/2021 -> 2021M5<br> Monthly -> MTD<br> 23099 -> 23000|
|Composite|Composite|Supports mapping<br>‘slices’ of members<br>such as a product|Supports mapping<br>‘slices’ of members<br>such as a product|A#51000: UD2#H* to the UD1 member of sales|A#51000: UD2#H* to the UD1 member of sales|
|Range|Range|Map a range of<br>accounts from A to Z<br>to = target.|Map a range of<br>accounts from A to Z<br>to = target.|Map accounts 21230~21239 to 20300|Map accounts 21230~21239 to 20300|
|List|List|Map a delimited list of<br>accounts to one<br>account|Map a delimited list of<br>accounts to one<br>account|Map accounts 41137;42642;42688 to account 60100|Map accounts 41137;42642;42688 to account 60100|
|Mask|Mask|Wildcard mapping<br>that takes the place of<br>one character (?) or<br>multiple characters<br>(*)|Wildcard mapping<br>that takes the place of<br>one character (?) or<br>multiple characters<br>(*)|Map accounts that start with 86 (as 86*) to 41000 or<br>*84*7* to 41000<br>Account starts with any one character, then 86 (as<br>?86*) to 42800<br>Any account to any account if there is a match:<br>* to *<br>Mask rules run less efficiently due to use of? or<br>embedded asterisks (e.g., *84*7*)|Map accounts that start with 86 (as 86*) to 41000 or<br>*84*7* to 41000<br>Account starts with any one character, then 86 (as<br>?86*) to 42800<br>Any account to any account if there is a match:<br>* to *<br>Mask rules run less efficiently due to use of? or<br>embedded asterisks (e.g., *84*7*)|

|Col1|Col2|Col3|Col4|Col5|Col6|
|---|---|---|---|---|---|
|**Transformation**<br>**Rule Type**|**Transformation**|**Definition**|**Definition**|** Example**|** Example**|
|**Transformation**<br>**Rule Type**|**Rule Type**|**Rule Type**|**Rule Type**|**Rule Type**|**Rule Type**|
|Target<br>Derivative|Target<br>Derivative|Applied logic & math<br>to post-transformed<br>Stage data|Applied logic & math<br>to post-transformed<br>Stage data|Does an aggregated mapped total need to be mapped<br>differently because it is positive or negative?|Does an aggregated mapped total need to be mapped<br>differently because it is positive or negative?|
|BlendUnit All<br>Derivative|BlendUnit All<br>Derivative|Applied logic & math<br>to post transformed<br>Blend partitioned page<br>data(Non-Stage)|Applied logic & math<br>to post transformed<br>Blend partitioned page<br>data(Non-Stage)|Executes for ALL BlendUnit pages (base or parent)|Executes for ALL BlendUnit pages (base or parent)|
|BlendUnit Base<br>Derivative|BlendUnit Base<br>Derivative|Applied logic & math<br>to post transformed<br>Blend partitioned page<br>data(Non-Stage)|Applied logic & math<br>to post transformed<br>Blend partitioned page<br>data(Non-Stage)|Executes for base BlendUnit pages only|Executes for base BlendUnit pages only|
|BlendUnit Parent<br>Derivative|BlendUnit Parent<br>Derivative|Applied logic & math<br>to post transformed<br>Blend partitioned page<br>data(Non-Stage)|Applied logic & math<br>to post transformed<br>Blend partitioned page<br>data(Non-Stage)|Executes for parent BlendUnit pages only|Executes for parent BlendUnit pages only|

Figure 6.21

## Transformation Performance

This information is taken directly from our training materials as it is the best summary of the costs of mapping. Each type of mapping rule has a cost, and small trial balances don’t have a significant issue with time. The more records, the more time they take to map. Understanding the economics of these different rules is important. Just because something is in the red doesn’t make it bad. Question marks cost a lot of processing power. They should be limited, but there might be times they make sense; if they cause the data load to take longer, it might be time to consider other mapping alternatives. Using question marks on both sides would be extremely costly as the data has to be carved up and put back together.

![Figure 6.22](images/foundation-handbook-ch06-p210-2248.png)

|Col1|Col2|
|---|---|
|**Rule Type**|**Processing**|
|Map One-To-One|Simple update pass through to the database.|
|Map Range|Simple update pass through to the database.|
|Map Range<br>(Conditional)|Uses a lot of data transfer and memory utilization. Required to return a<br>record set with all dim fields back to the app server to perform conditional<br>mapping.|
|Map List|Simple update pass through to the database.|
|Map List<br>(Conditional)|Uses a lot of data transfer and memory utilization. Required to return a<br>record set with all dim fields back to the app server to perform conditional<br>mapping. Keep LIST least restrictive. Break list into multiple, smaller lists<br>for optimal memory utilization and faster rules processing.|
|Map Mask *|Use one-sided * for low processing overhead. Simple update pass through<br>to the database.|
|Map Mask ?|Simple update pass through to the database. Use one-sided ? for<br>low/medium processing overhead.<br>Masking queries must use table scans which can hurt performance on<br>large record volumes. Keep total number of ? to a minimum. More ?<br>causes longer time for database engine to process the mask.|
|Map Mask * to *|Processes very quickly. Simple update pass through to the database.|
|Map Mask<br>(Conditional)|Uses a lot of data transfer and memory utilization. Required to return a<br>record set with all dim fields back to the app server to perform conditional<br>mapping.<br>Keep Mask criteria restrictive. Limit each query to a small chunk.<br>Example: 1*, 2*, A*, B*|
|Derivative|Uses a lot of data transfer and memory utilization.<br>Required to return a record set with all dim fields back to the app server to<br>perform conditional mapping.<br>Executes a SQL Statement that pulls ALL dimension fields from<br>worktable with a LIKE clause as main criteria and passes records back to<br>the application server.<br>Required for the application server to derive the calculated rows.<br>New records are inserted into the worktable one by one.|

Figure 6.23

## ODBC Connectors And REST API

With ledgers moving to the cloud, there are a lot of products out there that can be used to connect to cloud-based software and ledger systems directly. I have been able to connect to multiple products using ODBC connectors by third parties – for QuickBooks and other products – from vendors, including QODBC and CData. These products do take some knowledge of SQL and some time to set up, but once set up, they work on demand. They also require some knowledge of the product and tables and stored procedures. I found them to be useful but more complicated than using a REST API. However, with our Azure cloud offering, this becomes more problematic from a security point of view and is not available to customers hosting in Azure. Most cloud products have a REST API, which allows you to directly connect with the product, albeit with some barriers (e.g., IDs and passwords) that are needed at a nominal cost. There are several REST APIs that have samples. Some of those ledgers or source systems include Sage Intacct, Salesforce, Workday, SAP, Dynamics AX, and Hubspot. Some software providers ask you to buy the developer license in order to get the necessary credentials and their protections for your data; not just anyone can get to them. Credentials are typically needed to get back data in an XML format before it can be read and parsed in a connection business rule.

## Smart Integration Connector (Sic)

When either the ODBC connector or REST API service are not exposed publicly, meaning the OneStream environment cannot get to them directly, you will need to leverage the Smart Integration Connector (SIC), which is a solution that uses a local agent (SIC Gateway) to be installed inside the customer’s network and then leverages Azure Relay technology to communicate with OneStream. More information can be found in the product documentation or by attending one of our training sessions for SIC.

## Automation With Batch Process

OneStream has the ability to fully process files through the workflow by using batch processing. Files can be prepared with a naming convention that can be used to auto load. By dropping a file into the `Harvest` directory, and with a specified interval, batch files can be loaded into  OneStream. This works well for fixed, delimited, and Excel files. Blank files can be created for connector-based data sources and the data management export sequences. Field Layout:

```text
File ID-WorkflowProfileName-ScenarioName-TimeName-LoadMethod.txt
```

File Name Example:

```text
1TB-Detroit;Import-Actual-2011M1-R.txt
```

## Conclusion

Data Integration is the foundation of all the data that can be reported through OneStream. Managing data, and deciding upon the level and detail of data, is a large part of any design. Don’t unnecessarily populate the system with data you don’t need; use OneStream to analyze all the data you have. If you need to investigate, you can drill back to larger data sources and warehouses. You can get any data into this system, and roll it up and report on it in many different ways. Controlled, organized data should be your primary focus when building the platform.

## Epilogue

This photo is from September 13, 2011, when the original team was performing the first implementation of OneStream. The group was discussing metadata design for the application, and mapping from the legacy source system of Hyperion Enterprise to consume the historical data and the dimension mapping from Enterprise to OneStream.

![](images/foundation-handbook-ch06-p213-2267.png)

Our first customer wanted 14 years of data reconciled, and that was extreme. But it allowed the customer to retire its legacy system forever, and allowed us to test the system with the volume of data. The funny thing is that, as a consultant, I loaded and reconciled their prior system 14 years before that. The same data was reconciled twice, by the same person, 14 years apart. This second photo is from our May 2017 Splash Conference. It was a big moment when we arrived and saw our name – up in lights – at the Hard Rock Hotel. It was also one of those moments, as a startup, when you just have to laugh. We had just reached 100 employees, 75+ of whom were coming from different parts of the world, and all the corporate credit cards were being rejected for each US-based employee (as the pre-payment authorizations were being charged to a casino!). I had to call our bank to make sure they knew people were going to be staying there, and to authorize the charges!

![](images/foundation-handbook-ch06-p213-2265.png)
