# Dimension Metadata

The Dimension Metadata viewer lets you browse OneStream dimension hierarchies, inspect member properties, search for specific members, and export dimension data to Excel or CSV. It provides a tree view of your application's dimensional structure directly within VS Code.

## Opening the Viewer

Open the Dimension Metadata viewer using any of these methods:

- **Keyboard shortcut**: `Ctrl+Alt+D`
- **Command Palette**: `Ctrl+Shift+P`, then search for **OneStream: Open Dimension Viewer**
- **OneStream Explorer**: Click any dimension in the sidebar tree

## Tree View

The main interface is a hierarchical tree showing dimensions and their members. The tree starts with the top-level dimensions in your OneStream application:

- **Entity**
- **Account**
- **Scenario**
- **Time**
- **Flow**
- **UD1 through UD8** (User-Defined dimensions)
- Any additional custom dimensions

### Expanding Members

Click the expand arrow next to any dimension or member to see its children. The tree loads children on demand, so only the levels you expand are fetched from the server.

The initial expand level is configurable. By default, the tree auto-expands 2 levels deep when you open a dimension. You can change this to anywhere from 1 to 10 levels.

### Member Icons

Members display icons indicating their type:

- **Parent members**: Folder icon, indicating they have children.
- **Leaf members**: Document icon, indicating no children.
- **Shared members**: Link icon, indicating they reference another member in the hierarchy.

## Search and Filter

Type in the search box at the top of the tree view to find members by name or description. The search:

- Filters the tree to show only matching members and their parent path.
- Is case-insensitive.
- Matches partial strings.
- Updates results as you type.

Clear the search to return to the full tree view.

## Member Properties and Attributes

Click a member to view its properties in the detail panel. Properties include:

- **Name**: The member's unique name.
- **Description**: Human-readable description.
- **Member Type**: Base, Shared, or other type indicators.
- **Data Storage**: Store, Never Share, Share Data, etc.
- **Account Type**: Revenue, Expense, Asset, Liability, etc. (for Account dimension members).
- **Currency**: The member's currency setting (for Entity members).
- **Custom Attributes**: Any user-defined attributes associated with the member.

### Time-Based Attributes

For Time dimension members, additional temporal properties are shown:

- **Start Date**: The beginning of the time period.
- **End Date**: The end of the time period.
- **Frequency**: The time frequency (Year, Quarter, Month, etc.).

## Export Options

### Export to Excel (XLSX)

Export a dimension or sub-tree to an Excel file:

1. Right-click a dimension or member in the tree.
2. Select **Export to Excel**.
3. Choose a save location.

The Excel export includes:

- **Collapsible outline**: The hierarchy is represented using Excel's grouping/outline feature. You can expand and collapse levels directly in Excel.
- **All properties**: Each member's properties are included as columns.
- **Formatting**: Headers are bold and columns are auto-sized.

### Export to CSV

Export to a flat CSV file:

1. Right-click a dimension or member in the tree.
2. Select **Export to CSV**.
3. Choose a save location.

The CSV includes one row per member with columns for the member name, parent, description, level, and all properties. The hierarchy is represented by a Level column and indentation in the Name column.

## Drill from Selected Member

To view only a specific sub-tree:

1. Click a member in the tree to select it.
2. Right-click and select **Drill from Here**, or click the drill icon in the toolbar.
3. The tree reloads showing only the selected member and its descendants.

This is useful when you want to focus on a specific part of a large hierarchy without scrolling through the entire dimension.

To return to the full dimension view, click the **Show All** button that appears in the toolbar when a drill filter is active.

## Caching

Dimension data is cached locally to improve performance when navigating the tree. The cache is shared across sessions and refreshes automatically based on the timeout setting.

- **Cache Hit**: When you expand a node that is in the cache and not expired, the children appear instantly.
- **Cache Miss**: When the cache is expired or the node has not been loaded before, a brief loading indicator appears while data is fetched from the server.
- **Manual Refresh**: Click the refresh button in the toolbar to clear the cache and reload from the server.

## Settings Reference

| Setting | Type | Default | Range | Description |
|---|---|---|---|---|
| `onestream.dimension.expandLevel` | number | `2` | 1--10 | Number of hierarchy levels to auto-expand when opening a dimension |
| `onestream.dimension.cacheTimeout` | number | `300` | 60--3600 | Seconds before cached dimension data expires |
