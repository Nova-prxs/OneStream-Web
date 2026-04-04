# Error Log Viewer

The Error Log Viewer provides real-time access to OneStream error logs directly within VS Code. You can view full stack traces, filter by user or application, search error descriptions, and receive notifications when new errors occur.

## Opening the Viewer

Open the Error Log Viewer using any of these methods:

- **Keyboard shortcut**: `Ctrl+Alt+L`
- **Command Palette**: `Ctrl+Shift+P`, then search for **OneStream: Open Error Log Viewer**
- **OneStream Explorer**: Click the Error Logs item in the sidebar tree

## Interface Overview

The Error Log Viewer is a WebView panel with the following sections:

- **Filter Bar**: Controls for filtering logs by username, application, and time period.
- **Search Box**: Free-text search across error descriptions.
- **Error List**: A scrollable list of error entries showing timestamp, username, application, and a truncated description.
- **Detail Panel**: When you select an error entry, the full details appear here including the complete error message and stack trace.

## Viewing Error Details

Click any error entry in the list to expand its details. The detail panel shows:

- **Timestamp**: The exact date and time the error occurred.
- **Username**: The OneStream user whose action triggered the error.
- **Application**: The OneStream application context.
- **Error Description**: The full error message text.
- **Stack Trace**: The complete .NET stack trace. Click the expand button to view the full trace, or click collapse to minimize it.

### One-Click Stack Trace Expansion

Each error entry has a small expand/collapse toggle. Click it to instantly reveal or hide the full stack trace without navigating away from the error list. This makes it fast to scan through multiple errors and inspect their traces.

## Filtering

### By Username

Use the username filter dropdown to show only errors from a specific user. This is useful when debugging issues reported by a particular person.

### By Application

Use the application filter to narrow results to a specific OneStream application. If your environment has multiple applications, this helps isolate errors to the relevant context.

### By Time Period

Select a time range to limit which errors appear:

- **Last 1 hour**
- **Last 4 hours**
- **Last 24 hours**
- **Last 7 days**
- **Custom range**: Select specific start and end dates/times.

### Search

The search box filters error entries by their description text. Type a keyword or phrase and the list updates in real time to show only matching entries. The search is case-insensitive and matches partial strings.

## Real-Time Notifications

When new errors are logged in OneStream, the Error Log Viewer can notify you in VS Code. This is particularly useful during development or testing when you want immediate feedback about failures.

Notifications appear as VS Code information messages in the bottom-right corner. Clicking the notification opens the Error Log Viewer and highlights the new error.

## Record Limit

By default, the viewer loads the 10 most recent errors matching your filters. You can change this to load up to 100 records at a time.

Loading more records may increase the time needed to fetch and display results, especially over slower network connections.

## Settings Reference

| Setting | Type | Default | Range | Description |
|---|---|---|---|---|
| `onestream.errorLog.recordLimit` | number | `10` | 1--100 | Maximum number of error log records to fetch per request |
