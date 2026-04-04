# Execute Sequence

The Execute Sequence feature lets you run OneStream sequences (data loads, calculations, consolidations, and other processes) directly from VS Code. It provides a searchable list of available sequences, foreground and background execution modes, parameter configuration, and execution history.

## Prerequisites

Execute Sequence requires the **OCE assembly with RuleExecutor** to be installed on your OneStream server. This component enables the extension to trigger sequence execution remotely.

Check with your OneStream administrator to confirm that:
1. The OCE component is deployed.
2. The RuleExecutor module is included and configured.
3. Your user account has permission to execute sequences.

## Running a Sequence

### Step 1: Select a Sequence

1. Open the Command Palette (`Ctrl+Shift+P`).
2. Run **OneStream: Execute Sequence**.
3. A searchable dropdown appears listing all available sequences from the database.
4. Type to filter the list, then select the sequence you want to run.

The dropdown shows the sequence name and description. Results update as you type.

### Step 2: Choose Execution Mode

After selecting a sequence, choose how to execute it:

- **Foreground**: The sequence runs in the foreground. VS Code waits for it to complete and shows progress in real time. The UI remains responsive, but the sequence panel shows a running indicator.
- **Background**: The sequence is submitted to run in the background on the server. VS Code returns immediately and you can continue working. You receive a notification when execution completes.

### Step 3: Configure Parameters (Optional)

If the sequence requires parameters, a parameter form appears after selecting the execution mode. Parameters can be entered in two modes:

#### Grid Mode

Parameters are displayed in a table with columns for name, value, and description. Each parameter has a typed input:

- **Text fields**: For string parameters.
- **Dropdowns**: For parameters with a fixed set of values.
- **Date pickers**: For date parameters.
- **Member selectors**: For parameters that expect a dimension member.

#### Text Mode

For advanced users, switch to text mode to enter parameters as key-value pairs:

```
ScenarioName=Actual
Year=2024
Entity=Corporate
```

Text mode is useful for copying parameters between runs or scripting.

### Step 4: Execute

Click **Execute** to start the sequence. The behavior depends on the execution mode:

**Foreground execution**:
- A progress panel shows the current step and overall progress.
- Each step's status is updated in real time (Pending, Running, Success, Failed).
- If a step fails, execution stops and the error is displayed.
- Total execution time is shown upon completion.

**Background execution**:
- A notification confirms that the sequence was submitted.
- You can monitor progress in the Task Activity Viewer.
- A notification appears when execution completes (success or failure).

## Saved Sequences

The extension remembers sequences you have executed recently. Access them from the **Saved Sequences** section in the Execute Sequence panel.

### How Saved Sequences Work

- The last N sequences you executed are saved with their parameters.
- Click a saved sequence to load it into the execution form with its last-used parameters.
- Edit parameters before re-running if needed.
- Remove saved sequences by clicking the delete icon.

Saved sequences are stored locally in your VS Code workspace settings and persist across sessions.

## Execution History

The Execute Sequence panel includes a history tab showing past executions:

| Column | Description |
|---|---|
| **Sequence Name** | The name of the sequence that was executed |
| **Status** | Success, Failed, or Running |
| **Start Time** | When execution began |
| **End Time** | When execution completed (blank if running) |
| **Duration** | Total elapsed time |
| **User** | The user who initiated the execution |
| **Errors** | Error message if the execution failed (click to see details) |

### Filtering History

Filter the history by:

- **Status**: Show only successful, failed, or running executions.
- **Time range**: Last hour, last day, last week, or custom range.
- **Sequence name**: Search by name.

### Error Details

For failed executions, click the error column to see the full error message. A link to the Error Log Viewer is provided for detailed stack traces.

## Workflow Tips

### Testing Sequences

Use foreground execution when testing sequences so you can see each step's progress and immediately identify which step fails.

### Production Runs

Use background execution for production data loads and processes so you can continue working while the sequence runs.

### Parameter Reuse

Save frequently used parameter combinations by executing the sequence once. The saved sequence retains the parameters, making it easy to re-run with the same configuration.
