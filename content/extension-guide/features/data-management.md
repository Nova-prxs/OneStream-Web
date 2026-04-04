# Data Management

The Data Management feature provides a visual editor for OneStream Data Management sequences. You can build, edit, and execute data load and transformation sequences entirely within VS Code using a WebView-based step editor with drag-and-drop support.

## Overview

OneStream Data Management sequences define multi-step data processing workflows. Each sequence contains an ordered list of steps, where each step performs a specific operation (load data, transform data, execute a rule, etc.).

The extension stores these sequences as JSON files in your workspace at `.onestream-metadata/DataManagement/`, making them Git-friendly and easy to version control.

## Step Editor

The step editor is a WebView panel that provides a visual interface for building sequences. It supports 8 step types, each with its own configuration form.

### Supported Step Types

| Step Type | Description |
|---|---|
| **Load** | Load data from a source (file, database, connector) into a cube |
| **Transform** | Apply transformations to loaded data using mapping rules |
| **Execute** | Run a business rule as part of the sequence |
| **Calculate** | Trigger a calculation pass on the cube |
| **Consolidate** | Run a consolidation process |
| **Translate** | Execute currency or intercompany translations |
| **Export** | Export data to a file or external system |
| **Custom** | A custom step type for specialized operations |

Each step type has a tailored form with the relevant configuration fields. Required fields are marked and validated before saving.

### Adding Steps

1. Click the **Add Step** button in the sequence editor toolbar.
2. Select the step type from the dropdown.
3. Fill in the configuration form.
4. Click **Save** to add the step to the sequence.

The new step appears at the bottom of the sequence. You can reorder it using drag and drop.

### Editing Steps

Click any step in the sequence list to open its configuration form. Make your changes and click **Save** to update the step. Click **Cancel** to discard changes.

### Drag and Drop Reordering

Reorder steps by clicking and dragging them to a new position in the sequence list. The execution order follows the visual order from top to bottom.

### Deleting Steps

Click the delete icon on a step to remove it from the sequence. A confirmation dialog appears to prevent accidental deletion.

## Member Selectors

Many step configurations require you to select OneStream members (dimensions, entities, accounts, etc.). The extension provides rich member selector controls.

### Tree View

Browse the member hierarchy in a tree. Expand nodes to see child members. Click a member to select it.

### Search

Type in the search box to filter members by name or description. Results update as you type.

### Multi-Select

Some fields support selecting multiple members. Check the boxes next to each member you want to include. Selected members appear as tags below the selector.

## Parameter Picker

For steps that reference Dashboard Parameters, a parameter picker is available. It shows all parameters defined in the OneStream environment with their current values. Select a parameter to reference it in the step configuration.

## Cube and Business Rule Selectors

When configuring steps that operate on cubes or execute business rules, dedicated selectors let you browse and pick from the available cubes and rules in your environment.

- **Cube Selector**: Shows all cubes in the current application with their dimensions.
- **Business Rule Selector**: Shows available rules filtered by the step type's requirements.

## Sequence Operations

### Pull from Database

Download an existing Data Management sequence from OneStream to your local workspace:

1. Open the Command Palette.
2. Run **OneStream: Pull Data Management Sequence**.
3. Select the sequence from the list.
4. The sequence is saved as a JSON file in `.onestream-metadata/DataManagement/`.

### Push to OneStream

Upload your local sequence changes to the server:

1. Open the sequence in the editor.
2. Click the **Push** button in the toolbar, or run **OneStream: Push Data Management Sequence** from the Command Palette.
3. The sequence is uploaded and replaces the server version.

## Execution with Notifications

You can execute a Data Management sequence from within the editor:

1. Open the sequence.
2. Click the **Execute** button in the toolbar.
3. The sequence begins execution on the server.
4. A notification appears when execution starts.
5. Progress updates appear as each step completes.
6. A final notification reports success or failure.

If a step fails, the notification includes the error message and a link to the Error Log Viewer for detailed diagnostics.

## JSON Storage Format

Sequences are stored as JSON files at `.onestream-metadata/DataManagement/<SequenceName>.json`. This format is designed to be:

- **Human-readable**: The JSON is formatted with indentation for easy review.
- **Git-friendly**: Changes produce meaningful diffs that are easy to review in pull requests.
- **Portable**: The JSON files can be shared between developers or environments.

### Example Structure

```json
{
  "name": "MonthlyDataLoad",
  "description": "Monthly data load from ERP system",
  "steps": [
    {
      "type": "Load",
      "name": "Load ERP Data",
      "config": {
        "source": "ERPConnector",
        "target": "FinanceCube",
        "mapping": "ERPMapping"
      }
    },
    {
      "type": "Calculate",
      "name": "Run Calculations",
      "config": {
        "cube": "FinanceCube",
        "scenario": "Actual"
      }
    }
  ]
}
```

## Version Control

Because sequences are stored as JSON files in your workspace, they are automatically tracked by Git if you have a repository initialized. This means you get:

- **Change history**: See exactly what changed in each sequence over time.
- **Branching**: Test sequence changes in branches before pushing to the server.
- **Code review**: Review sequence changes in pull requests before deploying.
- **Rollback**: Revert to any previous version of a sequence.
