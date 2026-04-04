# Task Activity Viewer

The Task Activity Viewer lets you monitor task execution history in your OneStream environment. You can view task hierarchies, filter by various criteria, inspect failed tasks with links to error logs, and export results.

## Opening the Viewer

Open the Task Activity Viewer using any of these methods:

- **Keyboard shortcut**: `Ctrl+Alt+T`
- **Command Palette**: `Ctrl+Shift+P`, then search for **OneStream: Open Task Activity Viewer**
- **OneStream Explorer**: Click the Task Activity item in the sidebar tree

## Interface Overview

The Task Activity Viewer is a WebView panel containing:

- **Filter Bar**: Dropdowns and inputs for filtering by username, application, status, and time range.
- **Search Box**: Free-text search across task descriptions.
- **Task List**: A table showing task entries with columns for status, description, username, application, start time, end time, and duration.
- **Sub-Task Panel**: When you select a task, its sub-tasks appear in a nested hierarchy below.

## Viewing Tasks

The main task list shows top-level tasks. Each row displays:

- **Status**: An icon indicating success, failure, running, or queued.
- **Description**: What the task does (e.g., "Load Data", "Run Consolidation").
- **Username**: The user who initiated the task.
- **Application**: The OneStream application context.
- **Start Time**: When the task began.
- **End Time**: When the task completed (blank if still running).
- **Duration**: Total elapsed time.

### Sub-Tasks

Click any task row to expand its sub-task hierarchy. OneStream tasks can have unlimited nesting depth -- a task can contain sub-tasks, which can contain their own sub-tasks, and so on.

Each sub-task shows the same columns as the parent task. The hierarchy is displayed with indentation to make the parent-child relationship clear.

This is especially useful for understanding complex operations like data loads or consolidations that involve many steps internally.

## Filtering

### By Username

Select a specific user from the dropdown to see only tasks they initiated.

### By Application

Filter to a specific OneStream application.

### By Status

Filter tasks by their execution status:

- **All**: Show all tasks regardless of status.
- **Success**: Only completed tasks that finished without errors.
- **Failed**: Only tasks that encountered errors.
- **Running**: Only tasks that are currently executing.
- **Queued**: Only tasks waiting to start.

### By Time Range

Select a time period:

- **Last 1 hour**
- **Last 4 hours**
- **Last 24 hours**
- **Last 7 days**
- **Custom range**: Pick specific start and end dates/times.

### By Description

Use the search box to filter tasks by their description text. The search is case-insensitive and matches partial strings.

## Failed Task Investigation

When a task shows a **Failed** status, click it to see the full error information. The detail panel shows the error message associated with the failure.

If error log entries exist for the failed task, a **View Error Logs** link appears. Clicking it opens the Error Log Viewer pre-filtered to show the relevant error entries for that task. This makes it easy to jump from a failed task directly to the detailed error information and stack trace.

## Auto-Refresh

Enable auto-refresh to have the Task Activity Viewer periodically poll for new tasks. This is useful when you are running operations and want to watch their progress without manually refreshing.

When auto-refresh is enabled:

- The viewer fetches new data at the configured interval (default: 30 seconds).
- A refresh indicator appears briefly in the toolbar during each fetch.
- New tasks appear at the top of the list.
- You can pause auto-refresh at any time by clicking the pause button.

## CSV Export

Click the **Export CSV** button in the toolbar to save the current task list (with applied filters) as a CSV file. The export includes all visible columns and respects your current filter selections.

## Settings Reference

| Setting | Type | Default | Range | Description |
|---|---|---|---|---|
| `onestream.taskActivity.recordLimit` | number | `25` | 1--1000 | Maximum number of task records to fetch per request |
| `onestream.taskActivity.autoRefresh` | boolean | `false` | -- | Automatically refresh the task list on an interval |
| `onestream.taskActivity.refreshInterval` | number | `30` | 5--300 | Seconds between auto-refresh polls |
