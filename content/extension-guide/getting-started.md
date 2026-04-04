# Getting Started

This guide walks you through installing the OneStream AI Helper extension, connecting to your OneStream environment, and performing the initial setup so you can begin working immediately.

## System Requirements

Before installing, verify that your system meets the following requirements:

| Requirement | Minimum Version | Notes |
|---|---|---|
| VS Code | 1.60 or later | Also compatible with Cursor IDE |
| OneStream | 8.0 or later | Earlier versions may have limited feature support |
| Operating System | Windows, macOS, or Linux | All platforms supported; some features require platform-specific configuration |
| Network | HTTPS access to OneStream server | The extension communicates exclusively over HTTPS |

### Optional Requirements

- **OneStream DLLs** (Windows only): Required for full C# IntelliSense with type resolution. On macOS and Linux, IntelliSense works without DLLs using embedded type data.
- **OCE Component**: Required for code compilation and sequence execution features. This is a server-side component that must be deployed to your OneStream environment.

## Installation

You can install the extension through three methods depending on your development environment.

### Method 1: VS Code Marketplace (Recommended)

1. Open VS Code.
2. Press `Ctrl+Shift+X` (Windows/Linux) or `Cmd+Shift+X` (macOS) to open the Extensions view.
3. Search for **OneStream AI Helper**.
4. Click **Install**.
5. Reload VS Code when prompted.

### Method 2: Cursor IDE

1. Open Cursor.
2. Open the Extensions panel from the sidebar.
3. Search for **OneStream AI Helper**.
4. Click **Install**.
5. Reload Cursor when prompted.

Cursor supports all the same features as VS Code, including the MCP server integration.

### Method 3: Manual VSIX Installation

If you received the extension as a `.vsix` file (for example, during a pre-release or for offline installation):

1. Open VS Code.
2. Press `Ctrl+Shift+P` (Windows/Linux) or `Cmd+Shift+P` (macOS) to open the Command Palette.
3. Type **Extensions: Install from VSIX...** and select the command.
4. Browse to the `.vsix` file and select it.
5. Reload VS Code when prompted.

## First Connection

After installation, you need to connect to your OneStream environment.

### Step 1: Open the OneStream Sidebar

Click the **OneStream** icon in the VS Code Activity Bar (left sidebar). The OneStream Explorer panel will open.

### Step 2: Enter Connection Details

When prompted, provide the following:

- **Server URL**: The full URL of your OneStream environment (for example, `https://yourcompany.onestream.com`). Do not include trailing slashes or path segments.
- **Username**: Your OneStream login username.
- **Password**: Your OneStream password. The password is used only to obtain an authentication token and is never stored.

### Step 3: Verify Connection

Once connected, the OneStream Explorer will populate with your available applications and workspaces. The status bar at the bottom of VS Code will display the connected environment name.

If the connection fails, verify the following:
- The server URL is correct and accessible from your network.
- Your credentials are valid.
- HTTPS/TLS is properly configured on the server.

## Initial Setup

After connecting for the first time, perform these three setup steps to get the most out of the extension.

### Pull All Unencrypted Rules

This downloads all business rules from your OneStream environment to your local workspace so you can edit them in VS Code.

1. Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`).
2. Run **OneStream: Pull All Unencrypted Rules**.
3. Wait for the download to complete. A progress notification will appear.

The rules are saved into your workspace folder organized by rule type (Finance, Extender, Connector, Parser, etc.).

### Setup C# IntelliSense

This configures autocomplete and type information for OneStream's API surface.

1. Open the Command Palette.
2. Run **OneStream: Setup C# IntelliSense**.
3. The extension will generate the necessary reference files.

On Windows, if you have access to OneStream DLLs, you can point to them for richer type resolution. On macOS and Linux, IntelliSense works automatically using embedded type data.

### Setup Folder Structure

This organizes your workspace into the standard OneStream project structure.

1. Open the Command Palette.
2. Run **OneStream: Setup Folder Structure**.

The extension creates a well-organized directory layout separating rules by type, assemblies by workspace, and metadata into dedicated folders.

## Folder Structure

After setup, your workspace will have the following structure:

```
your-workspace/
  Finance/
    RuleName1.cs
    RuleName2.cs
  Extender/
    RuleName1.cs
    RuleName2.cs
  Connector/
    ...
  Parser/
    ...
  Assemblies/
    WorkspaceName/
      AssemblyFile.cs
  .onestream-metadata/
    DataManagement/
      SequenceName.json
    ...
  .vscode/
    settings.json
```

- **Finance/, Extender/, Connector/, Parser/**: Business rules organized by type.
- **Assemblies/**: Server-side assemblies organized by workspace.
- **.onestream-metadata/**: Data Management sequences and other metadata stored as JSON for Git-friendly version control.
- **.vscode/**: VS Code settings configured by the extension.

## Keyboard Shortcuts

The extension provides keyboard shortcuts for frequently used commands. All shortcuts use the `Ctrl` modifier on Windows/Linux and `Cmd` on macOS unless otherwise noted.

| Shortcut | Command | Description |
|---|---|---|
| `Ctrl+Shift+Q` | Open SQL Query Editor | Opens the SQL Query Editor in the default layout |
| `Ctrl+Shift+Alt+Q` | Open SQL (Beside) | Opens the SQL Query Editor beside the current editor |
| `Ctrl+K Ctrl+Q` | Quick SQL Query | Opens a quick input for running a SQL query |
| `Ctrl+K Ctrl+Down` | Pull Rule | Downloads the current rule from OneStream |
| `Ctrl+K Ctrl+Up` | Push Rule | Uploads the current rule to OneStream |
| `Ctrl+K Ctrl+B` | Compile | Compiles the current assembly or rule |
| `Ctrl+Alt+L` | Error Log Viewer | Opens the Error Log Viewer |
| `Ctrl+Alt+T` | Task Activity Viewer | Opens the Task Activity Viewer |
| `Ctrl+Alt+D` | Dimension Viewer | Opens the Dimension Metadata Viewer |

You can customize these shortcuts through VS Code's Keyboard Shortcuts editor (`Ctrl+K Ctrl+S`).

## Next Steps

With the initial setup complete, explore the individual feature guides:

- [SQL Query Editor](features/sql-query-editor.md) -- Run queries against OneStream databases.
- [Rule & Assembly Sync](features/rule-assembly-sync.md) -- Push and pull rules with Git integration.
- [C# IntelliSense](features/csharp-intellisense.md) -- Get autocomplete for OneStream APIs.
- [AI Assistant Integration](features/ai-assistant-integration.md) -- Connect AI coding assistants to your OneStream project.
