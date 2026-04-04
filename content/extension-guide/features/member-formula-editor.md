# Member Formula Editor (Beta)

The Member Formula Editor lets you write and validate formulas assigned to dimension members directly within VS Code. It supports VB.NET syntax highlighting, compile-based validation, and dimension-aware editing with support for all OneStream formula types.

> **Beta Notice**: This feature is in beta. Some aspects of the interface and behavior may change in future releases.

## Overview

In OneStream, dimension members can have formulas assigned to them that control how their values are calculated. These formulas are written in VB.NET syntax and execute during calculation passes. The Member Formula Editor brings this editing experience into VS Code, replacing the need to use the OneStream web interface for formula work.

## Opening the Editor

Access the Member Formula Editor through the Dimension Metadata viewer:

1. Open the Dimension Metadata viewer (`Ctrl+Alt+D`).
2. Navigate to the member whose formula you want to edit.
3. Click the member, then click the **Edit Formula** button in the member detail panel.

The formula editor opens as a WebView panel.

## Formula Types

The editor supports all OneStream formula types. Select the formula type from the dropdown at the top of the editor:

| Formula Type | Description |
|---|---|
| **DynamicCalc** | Dynamically calculates the member's value each time it is requested. The value is not stored in the database. |
| **DynamicCalcTextInput** | Like DynamicCalc, but allows users to input text values that are stored while the numeric value is calculated. |
| **FormulaPass1** | Executes during calculation pass 1. Use this for formulas that depend only on base-level input data. |
| **FormulaPass2** | Executes during calculation pass 2. Use this for formulas that depend on results from pass 1. |
| **FormulaPass3** | Executes during calculation pass 3. |
| **FormulaPass4** | Executes during calculation pass 4. |
| **FormulaPass5** | Executes during calculation pass 5. |
| **FormulaPass6** | Executes during calculation pass 6. |
| **FormulaPass7** | Executes during calculation pass 7. |
| **FormulaPass8** | Executes during calculation pass 8. |
| **FormulaPass9** | Executes during calculation pass 9. |
| **FormulaPass10** | Executes during calculation pass 10. |
| **FormulaPass11** | Executes during calculation pass 11. |
| **FormulaPass12** | Executes during calculation pass 12. |
| **FormulaPass13** | Executes during calculation pass 13. |
| **FormulaPass14** | Executes during calculation pass 14. |
| **FormulaPass15** | Executes during calculation pass 15. |
| **FormulaPass16** | Executes during calculation pass 16. |

The formula type determines when during the calculation cycle the formula runs. Choose the type based on the data dependencies of your formula.

## VB.NET Syntax Highlighting

The formula editor provides VB.NET syntax highlighting for:

- **Keywords**: `If`, `Then`, `Else`, `End If`, `Dim`, `As`, `Return`, etc.
- **String literals**: Quoted strings are highlighted.
- **Comments**: Lines starting with `'` are recognized as comments.
- **Numbers**: Numeric literals are highlighted.
- **OneStream objects**: Common objects like `api`, `si`, `HS` are highlighted as recognized identifiers.

## Formula Validation

Click the **Validate** button to compile-check your formula without saving it. The validation:

1. Sends the formula to the OneStream server for compilation.
2. Returns any errors or warnings with line numbers.
3. Displays errors inline in the editor and in the Problems panel.

This lets you catch syntax errors and type mismatches before saving the formula to the member.

## Formula and DrillDown Toggle

Members can have two types of formulas:

- **Formula**: The calculation formula that computes the member's value.
- **DrillDown**: A separate formula that defines what happens when a user drills down on the member's value in a report.

Use the toggle at the top of the editor to switch between editing the Formula and the DrillDown formula. Each has its own separate code area.

## Time-Based Attributes

When editing formulas for Time dimension members, additional context is provided:

- The editor shows the time period's start date, end date, and frequency.
- Formula suggestions include time-related functions like `TimePeriod`, `StartDate`, `EndDate`.
- Validation accounts for time-specific constraints.

## Dimension-Aware UI

The editor adapts based on which dimension the member belongs to:

- **Account dimension**: Shows account type, sign behavior, and related properties.
- **Entity dimension**: Shows currency and entity-specific attributes.
- **Scenario dimension**: Shows scenario type and related workflow properties.
- **Time dimension**: Shows temporal attributes and time-related functions.

This context helps you write formulas that are correct for the member's dimensional context.

## Unsaved Changes Protection

If you have unsaved changes in the formula editor and attempt to:

- Close the editor tab
- Navigate to a different member
- Switch between Formula and DrillDown
- Close VS Code

A confirmation dialog appears warning you about unsaved changes. You can choose to save, discard, or cancel the action.

## Member Properties Editor

Below the formula editing area, the editor displays the member's properties in an editable form. You can modify:

- **Description**: The human-readable description of the member.
- **Data Storage**: How the member stores data (Store, Never Share, Share Data, etc.).
- **Account Type**: For Account dimension members (Revenue, Expense, Asset, Liability, etc.).
- **Custom Attributes**: Any user-defined attributes.

Changes to properties are saved together with formula changes when you click **Save**.

## Workflow

1. Open the Dimension Metadata viewer.
2. Find the member you want to edit.
3. Click **Edit Formula** to open the formula editor.
4. Select the formula type.
5. Write or modify the formula.
6. Click **Validate** to check for errors.
7. Fix any errors reported.
8. Click **Save** to save the formula to the member on the server.
