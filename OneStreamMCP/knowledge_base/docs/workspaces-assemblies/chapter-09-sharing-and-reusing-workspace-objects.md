---
title: "Chapter 9 - Sharing and Reusing Workspace Objects"
book: "workspaces-assemblies"
chapter: 9
start_page: 226
end_page: 239
---

# Sharing And Reusing

# Workspace Objects

## What Is Workspace Sharing?

In OneStream, sharing objects between Workspaces means you can reference and reuse existing components – like dashboards, data adapters, parameters, and Cube Views – across multiple Workspaces without having to duplicate them. Communicating this powerful sharing tool to your development teams will help you realize more synergies between development efforts, and ultimately help you extend your OneStream investment.

## Do I Need To Share Workspaces?

The short answer is no, you don’t need to share Workspaces (maintaining Workspaces as private and self-contained has many advantages in terms of portability and robustness). However, there are many benefits to incorporating this practice in your application, such as making dashboard development more efficient and effective. Once you start dabbling with Workspace sharing, you’ll quickly see how this feature allows you to streamline development. Here are the benefits of sharing Workspaces within your OneStream application.

### 1. Leverage Existing Work

Instead of recreating dashboards or related objects from scratch in each Workspace: •You can link to or reference objects that already exist in another Workspace. This reduces development time, encourages reusability, and maintains consistency across projects or teams.

### 2. Eliminates Redundancy

Duplicating dashboard components across Workspaces leads to: •Multiple versions of the same object. •Increased storage use. •Difficulties in updating or maintaining consistency. Sharing allows centralized updates to be reflected automatically across all consuming Workspaces.

### 3. Ensures Consistency

When objects are shared rather than duplicated: •Everyone is working with the same version of the dashboard or Cube View. •Business logic (like calculations or parameters) is consistent across the organization. •The risk of data discrepancies or misinterpretation is significantly reduced.

### 4. Improves Governance And Control

Sharing supports better governance by: •Centralizing control of critical objects. •Enabling a single source of truth. •Making audits and updates more manageable. •Reducing errors caused by conflicting versions.

### 5. Supports Scalable Development

For organizations with multiple developers or departments: •Teams can collaborate without stepping on each other’s toes with the use of a shared Workspace – a Central Library, if you will – containing objects that are meant to be shared (or extended) for consumption by other Workspaces. •Developers can test or enhance shared components in their own Workspace before proposing changes to the central/shared version. As you begin your OneStream journey or explore ways to make your OneStream investment work for you, there are undoubtedly areas where Workspace sharing will allow you to leverage existing work and cut down development time.

## How Do I Share Workspaces?

Sharing Workspace objects can be done in several ways. As the OneStream framework continues to evolve and grow, you may find other ways of sharing Workspace objects that help facilitate more efficient development practices and streamlined application management.

### Preparation

To share objects from a Workspace, the Workspace containing the shared objects (the source Workspace) must be configured to allow sharing by setting the Workspace property Is Shareable Workspace to True.

![Figure 9.1](images/workspaces-assemblies-ch09-p227-2397.png)

Once the source Workspace is configured, objects can be shared in two ways:

#### Direct Reference

When calling an object from the source Workspace into the target Workspace, reference the object using the syntax `WorkspaceName.ObjectName`.  Direct reference example – showing the object name preceded by the source Workspace name.

![Figure 9.2](images/workspaces-assemblies-ch09-p228-2405.png)

#### Full Workspace Share

By defining the source Workspace as a Shared Workspace in the Target Workspace property, all objects can be called using their name only. Target Workspace definition example – showing the `Formulation` Workspace configured as a  fully-shared Workspace.

![Figure 9.3](images/workspaces-assemblies-ch09-p228-2407.png)

Full Workspace shared object reference – showing only the object name being called.

![Figure 9.4](images/workspaces-assemblies-ch09-p229-2414.png)

The direct reference approach has the advantage of making it clear where the definition of the object is stored. The full Workspace share makes it easier if you are planning to share multiple objects. Let’s explore two commonly used methods for sharing Workspace objects: displaying shared dashboards using embedded dashboard components, and opening dashboards in dialog boxes using button components.

### Method #1 – Embedded Dashboards

To display a shared dashboard from another Workspace on your Workspace’s dashboard, you’ll start by creating an embedded dashboard component that can be a placeholder to nest on your dashboard. The first step is to create an embedded dashboard component. Navigate to the Embedded Dashboard component list and click on the Create Dashboard Component button from the Workspace toolbar.

![Figure 9.5](images/workspaces-assemblies-ch09-p229-2417.png)

Select Embedded Dashboard from the Create Dashboard Component dialog box and click OK.

![Figure 9.6](images/workspaces-assemblies-ch09-p229-2416.png)

Name the embedded dashboard component and click Save. Once you have created the new component, click on the Edit button on the Embedded Dashboard property.

![Figure 9.7](images/workspaces-assemblies-ch09-p230-2423.png)

The object lookup dialog box opens, displaying which Workspaces are shared and to which the user has access. You may have shared Workspaces in your application that a user does not have access to, which will not be displayed until you give the user access to those specific Workspaces.

![Figure 9.8](images/workspaces-assemblies-ch09-p230-2425.png)

Start typing the name of the dashboard you want to reuse, and the lookup menu will filter it based on your search criteria. Once you find the desired dashboard, select it and click OK.

![Figure 9.9](images/workspaces-assemblies-ch09-p231-2432.png)

Navigate to the dashboard where you want to display the shared object and click on the Dashboard Components tab. Click the ➕button and select the embedded dashboard component, then click OK.

![Figure 9.10](images/workspaces-assemblies-ch09-p231-2436.png)

To test the dashboard and ensure it displays, click the View Dashboard button.

![Figure 9.11](images/workspaces-assemblies-ch09-p231-2435.png)

The result will display the shared dashboard.

![Figure 9.12](images/workspaces-assemblies-ch09-p231-2434.png)

### Method #2 - Open Dialog Box With Button

Another common method of displaying shared dashboards is using a button to open a dialog box. Create a new button component and scroll down to the User Interface Action properties. From the Selection Changed User Interface Action property, select Open Dialog With No Buttons from the drop-down menu, then select the dashboard to open in Dialog from the Edit button.

![Figure 9.13](images/workspaces-assemblies-ch09-p232-2442.png)

To test the button’s action, attach the button to your dashboard’s Dashboard Component tab using the ➕ button, and then click View Dashboard. Once you see your button on the dashboard, click it, and you should see a dialog box appear with the shared dashboard.

![Figure 9.14](images/workspaces-assemblies-ch09-p232-2444.png)

These are just some examples of how you can share objects between Workspaces, but this should give you the groundwork and inspiration to enable you to share Workspace objects to enhance your application and streamline your development.

## Sharing Workspace Objects Examples

Here are a few examples of when you might want to share Workspace objects and how to properly configure your Workspaces to allow sharing. Although these are examples of use cases for sharing Workspaces and inherited objects, as you read through the examples, just imagine how much efficiency you can bring into your application by leveraging OneStream’s Workspace sharing functionality. With this feature in your toolbox, aside from streamlining development, you can significantly reduce the number of redundant objects and build efforts for your organization.

### Example #1 – Shared Kpi Dashboard

You have two developers working on separate Workspaces. One developer is building a dynamic landing page dashboard that allows users to customize their landing page experience, and the other is building standardized KPI dashboards that will be surfaced throughout the application. The KPI dashboard has been developed to dynamically change KPI data based on a user’s Cube POV settings, and below is a sample of how the dashboard is displayed.

![Figure 9.15](images/workspaces-assemblies-ch09-p233-2451.png)

With this dashboard ready to share with the `Landing Page` Workspace, the Is Shareable  Workspace property needs to be set to True.

![Figure 9.16](images/workspaces-assemblies-ch09-p233-2453.png)

Next, we need to review the `Landing Page` Workspace and enter the `Key Performance ` `Indicators (OP_KPI2)` Workspace name in the Shared Workspace Names property to enable  the KPI dashboard to be reused on the landing page dashboards.This step is crucial to enable the systematic object lookup when referencing shared objects, as it is the driver that populates the object lookup dialog when searching for dashboards to share.

![Figure 9.17](images/workspaces-assemblies-ch09-p234-2458.png)

Next, we need to insert the KPI dashboard into the landing page and ensure it is visible and working as expected. To do this, we’ve created an embedded dashboard component and selected the KPI Dashboard named `00_Horizontal_OP_KPI2` using the dashboard object lookup  function. Below is our embedded dashboard component, where we have the selected KPI dashboard in the Embedded Dashboard property.

![Figure 9.18](images/workspaces-assemblies-ch09-p234-2461.png)

The embedded dashboard component is now available to be attached to the dashboard.

![Figure 9.19](images/workspaces-assemblies-ch09-p234-2460.png)

The final product displays the KPI dashboard from the independent Workspace embedded into our landing page dashboard.

![Figure 9.20](images/workspaces-assemblies-ch09-p235-2466.png)

### Example #2 – Shared Workflow Buttons

Your FP&A manager instructs you to add a workflow step to capture new funding requests with justification requirements during the planning process. They’ve provided requirements for the dashboard layout and functionality that will be developed in a new Workspace. You need to incorporate buttons that trigger workflow complete and revert actions to allow users to submit data and revert to make changes as necessary. While the dashboard itself is completely new, you have other dashboards utilizing the same set of workflow buttons that you want to reuse instead of creating new buttons unnecessarily. The buttons you want to reuse are attached to the dashboard named `0_WorkflowButtons_POR` that is located  in the Formulation Workspace; below is the dashboard you want to reuse.

![Figure 9.21](images/workspaces-assemblies-ch09-p235-2468.png)

In the figure below, you can see that we’ve created and attached the embedded dashboard component that references the workflow buttons that need to be on our new dashboard.

![Figure 9.22](images/workspaces-assemblies-ch09-p236-2473.png)

Upon running the dashboard, the shared buttons are displayed in our dashboard’s header. Not only are they visible, they are functional on the dashboard, all without needing to rebuild the same buttons.

![Figure 9.23](images/workspaces-assemblies-ch09-p236-2476.png)

### Example #3 – Shared Parameters

You’ve created global formatting parameters – to maintain a consistent look and feel for your users – that you want to use throughout your application. These formatting parameters are in a standalone Workspace where they are maintained by a specific user in your organization. Below is the Workspace named `Global Formatting Parameters` with the parameters that you  want to use in another Workspace. Notice that the Is Shareable Workspace property is set to True.

![Figure 9.24](images/workspaces-assemblies-ch09-p236-2475.png)

In the Workspace named `Decision Package`, you have Cube Views that you want to format to  match your organization’s prescribed formatting. Notice the `Global Formatting Parameters` Workspace name has been entered into the Shared Workspace Names property; this allows you to reuse the objects or parameters from the `Global Formatting Parameters` Workspace in the  `Decision Package` Workspace.  The Is Shareable Workspace property for the `Decision Package` Workspace is intentionally set  to False. Since this Workspace does not contain objects intended for use in other Workspaces, disabling sharing helps prevent accidental or unintended references from external Workspaces – acting as a safeguard for object integrity. To simplify: •Is Shareable Workspace controls whether the current Workspace can share its objects with other Workspaces. •Shared Workspace Names defines which external Workspaces can share their objects into the current Workspace.

![Figure 9.25](images/workspaces-assemblies-ch09-p237-2484.png)

![Figure 9.25](images/workspaces-assemblies-ch09-p237-2483.png)

Once you’ve configured both the source Workspace (`Global Formatting Parameters`) and  the target Workspace (`Decision Package`), you can use the parameters in your Workspace Cube  Views, making formatting consistent and easily managed from the single Workspace.

![Figure 9.26](images/workspaces-assemblies-ch09-p237-2481.png)

## Conclusion

Sharing Workspace objects in OneStream is an incredibly useful concept that allows you to reduce the need to recreate the same objects over and over again. For advanced developers, this allows you to seamlessly integrate various intricate solutions within your application with the ability to restrict sharing as needed. For users who are new to OneStream or just starting to explore the breadth of functionality, simply understanding how sharing Workspace objects works will give you a head start in envisioning where you can take your OneStream investment.
