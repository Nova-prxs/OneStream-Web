---
title: "Setup and Installation"
book: "financial-close-guide"
chapter: 2
start_page: 2
end_page: 17
---

# Setup and Installation

Copyright © 2026 OneStream Software LLC. All rights reserved.

All trademarks, logos, and brand names used on this website are the property of their respective

owners. This document and its contents are the exclusive property of OneStream Software LLC

and are protected under international intellectual property laws. Any reproduction, modification,

distribution or public display of this documentation, in whole or part, without written prior consent

from OneStream Software LLC is strictly prohibited.


### Table of Contents



### Table of Contents



## Overview

1


## Setup and Installation

2


## Dependencies

2

Select the Financial Close Location and Create a Development


### Application

4


### Configure Application Server Settings

5


### Install Financial Close

6


### Set up OneStream Software Financial Close

7


### Create Tables

7


### Turning Off Dashboards

8


### Review the Package Contents

8


## Account Reconciliations

8


## Transaction Matching

11

Edit the Transformation Event Handler Business Rule
11


### Table of Contents



### Multiple Instances

14


### Upgrade Considerations

17


## Account Reconciliations

18


## Settings

18


## Global Setup

19


## Control Lists

28


## Column Settings

33


## Templates

35


## Access Control

41


## Certifications

49


## Uninstall

50


## Reconciliation Administration

51


## Reconciliation Definition

52


## Reconciliation Inventory

56


## Account Groups

71


## Tracking

81


### Table of Contents



## BalCheck

82


## AutoRec

84


## Dynamic Attribute Mapping

88


## Notifications

108


## Security

111


### Reconciliations Global Admin, OneStreamAdmin and Local



### Admin Permissions

112


### Workflow Profile Security

115


### Access Groups and Reconciliation Inventory Security

115


### Workflow and Reconciliation Filtering

115


## Roles

116


## Segregation of Duties

132


## Dashboard Security

132


## Using Account Reconciliations

133


## Workflow Actions

133


### Reconciliations Page

136


### Table of Contents



### Reconciliation Workspace

144


### Analysis and Reporting

171


### Scorecard

172


### Analysis

173


### Reports

175


### Anomaly Detection

178


## Certifications

179


### AI Settings

182


### Default Detectors and Rules

193


### AI Insights

205


### Anomaly Summary Grid

210


### Anomaly Trends

213


### Multi-Currency Calculation Examples

216


### Data Loaded into Stage

216


### Multi-currency Account Groups

217


### Table of Contents



## Transaction Matching

219


## Settings

219


### Global Options

219


### User Preferences

221


## Access

221


### Match Sets

226


### Index Admin

228


## Uninstall

231


### Load Transaction Data

232


### Administration

234


### Rules

234


### Data Sets

250


### Options

254


### Reason Codes

256


## Access

258


### Data Retention

259


### Table of Contents



### Matches

265


### Rule Processing

269


### Complete or Revert Workflow

271


### Export

271


### Comments and Attachments

272


### Match Actions

274


### Unmatch Matches

275


### Multi-Match Actions

276


### Transactions

277


### Transaction Status

278


### Data Filters

285


### Edit Transactions

293


### Transaction Details

294


### Export Transactions

296


### Scorecard

298


### Analysis

301


### Table of Contents



### Data Splitting

303


### Data Splitting Setup

303


### Filters

305


### Orphaned

308


### Prepare External Files

310


### Journal Entry Manager

317


## Settings

317


### Global Options

318


### Accounting Periods

320


### Connections

327


### Notification Methods

333


## Uninstall

337


### Journals

339


### Journals

340


### Recurring Journal Definitions

359


### Grid Column Settings

372


### Table of Contents



## Templates

375


## Templates

375


### Template Access Groups

384


### ERPs

387


### ERPs

388


### Field Value Lists

400

Import and Export of Field Value Lists
401


### Audit Log

405


### Grid Toolbar

406


### Toolbar Buttons

407


### Sample Grid Toolbar

408


### Journal Workflow

409


### Posting State

410


## Integration

414


### Enable Integration

414


### Assign Match Sets

415


### Table of Contents



### Map Detail Item Information

417


### Create Detail Items

418


### From Transaction Matching

419


### From Account Reconciliations

423


### Detail Item Integration Addendum

429


### Mapping

429


### Selections

431


### Scheduling Data Management Jobs

434


### Help and Miscellaneous Information

438


### Set Optimal Display Settings

438


### Package Contents and Naming Conventions

439


### Solution Database Migration Advice

439


### OneStream Solution Modification Considerations

440


### Custom Service Factory

440


### Set Up the Custom Service Factory

441


### Change Assemblies Setting Upon Install

442


### Table of Contents



### Maintain Assembly

442


## Appendix A: Date Grouping Tolerances

444


### No Date Tolerances

445


### Post-aggregate Date Tolerances

446


### Pre-aggregate Date Tolerances

447


### Set Up Date Tolerances

448


### Post-aggregate Date Tolerances

448


### Pre-aggregate Date Tolerances

449


## Appendix B: Complex Expressions

451

Loading Transaction Data: Date Attribute Fields (Attributes 17 -

20)
451

Appendix C: Custom Filter Dashboard (Transaction Matching)
452


## Overview



## Overview


OneStream Financial Close is a set of solutions encompassing the financial close process.


### Solutions included are:


l Account Reconciliations

l Transaction Matching

l Journal Entry Manager

All solutions are installed and configured as part of OneStream Financial Close resulting in a

single creation of solution tables.

The functionality in OneStream Financial Close is fully integrated, providing the ability to use

Transaction Matching detail to support detail items in Account Reconciliations.

Integration between solutions is optional allowing each solution to be used independently.

Solutions included in OneStream Financial Close require the same minimum Platform version.


## Setup and Installation



## Setup and Installation


The following sections contain important information about installing and configuring Financial


### Close. See:


l Dependencies

l Select a Location and Create a Development Application

l Configure Application Server Settings

l Installation

l Setup

l Review the Package Contents

l Edit the Transform Event Handler Business Rule


## Dependencies


Ensure that you have the following components installed:


## Setup and Installation



### Component


### Description



### OneStream 9.0.0


Minimum OneStream Platform release required to install this release of

or later

OneStream Financial Close.

See the Upgrade Guide.

l The minimum version for upgrading OneStream Financial Close

is OFC PV7.1.0 SV204.  To download OFC PV7.1.0 SV204,

please contact OneStream Support.


## Account


If you have the solutions already installed, you must be on these


### Reconciliations


releases before installing OneStream Financial Close.

l Account Reconciliations PV 440 SV 200 or later


## Transaction



### Matching


l Transaction Matching PV 530 SV 201


### Microsoft


One of these versions of Microsoft SQL Server is required:


### SQL Server


l Standard Edition 2016 SP1 or later

l Enterprise Edition 2012 or later

IMPORTANT: Uninstall UI prior to upgrading due to the migration to the OneStream

Financial Close workspace.


## Setup and Installation



### Select the Financial Close Location and



### Create a Development Application


Before beginning the installation, decide if you want to build the solution in the Production

OneStream application or in a separate Development OneStream application. This section

provides some key considerations for each option.

Production OneStream Application: If you build the solution in a Production application, you

will not have to migrate the resulting work from a Development application. However, there are

intrinsic risks when making design changes to an application used in Production, so this option is

not advised.

NOTE: OneStream strongly recommends that you implement the solution in the

Development environment with a fresh copy of the Production application before starting

work.

Development OneStream Application: As a best practice, use the Development OneStream

application to build the solution.


### To create the Development application:


1. Ensure all the OneStream artifacts relating to OneStream Financial Close such as

Workflow Profiles and Entities are in the Production application.

2. Create new Scenarios as necessary for the individual solutions, to be used with OneStream

Financial Close.

NOTE: Scenarios for both actuals and OneStream Financial Close solutions must

be set to monthly for this to work properly.


## Setup and Installation


3. Copy your Production OneStream application to your Development environment and

rename it. This Development version will be used for your OneStream Financial Close

project.


### Configure Application Server Settings


You may need to edit the OneStream Application Server Configuration so users can create and

change data in the additional database tables. If other OneStream Solutions (such as Specialty

Planning) are already in the application, these adjustments may already exist.

Ensure that the security group settings include the users who will use and set up the solution

before proceeding.

NOTE: Group settings are applicable to all OneStream Solutions; so keep group names

generic.

1. Start the OneStream Server Configuration Utility as an Administrator.

2. Click Open Application Server Configuration File.

3. Edit the following OneStream Database Server Connections:

l Access Group for Ancillary Tables: Select a group that includes those who will access

records.

l Can Create Ancillary Tables: True

l Can Edit Ancillary Table Data: True

l Maintenance Group for Ancillary Tables: Select a group who will edit and maintain

tables.