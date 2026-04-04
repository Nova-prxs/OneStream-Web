# MCP Server Overview

The OneStream AI Helper extension includes an embedded MCP (Model Context Protocol) server that exposes OneStream operations as tools that AI coding assistants can call. This enables AI assistants to read rules, query databases, browse dashboards, and access documentation programmatically.

## What is MCP

The Model Context Protocol (MCP) is an open standard for connecting AI assistants to external data sources and tools. Instead of the AI assistant only having access to the files in your editor, MCP lets it interact with live systems -- in this case, your OneStream environment.

When an AI assistant has access to the OneStream MCP server, it can:

- Read business rule source code from the server.
- Execute SQL queries against OneStream databases.
- Browse dashboard structures and component details.
- Search OneStream documentation and knowledge base.
- Inspect repository structure and metadata.

This means you can ask your AI assistant questions like "What does the Finance rule for revenue recognition do?" or "Show me all dashboard parameters that reference the Entity dimension" and get accurate answers based on live data.

## Architecture

The MCP server runs as part of the OneStream VS Code extension. When enabled, it:

1. Starts a local server process that implements the MCP protocol.
2. Uses the same authentication and connection as the VS Code extension.
3. Exposes tools organized into 5 categories.
4. Responds to tool calls from connected AI assistants.

No additional server installation is required. The MCP server starts and stops with the VS Code extension.

## Tool Categories

The MCP server provides 36+ tools organized into 5 categories:

| Category | Tools | Purpose |
|---|---|---|
| [Connection Tools](connection-tools.md) | 4 tools | Manage connections, check health, list environments |
| [Repository Tools](repository-tools.md) | 12 tools | Read rules, search code, browse workspaces, compile |
| [Dashboard Tools](dashboard-tools.md) | 5 tools | Browse dashboard trees, inspect components and parameters |
| [Knowledge Base Tools](knowledge-base-tools.md) | 10 tools | Search docs, browse APIs, look up enums and keywords |
| [SQL Tools](sql-tools.md) | 5 tools | Execute queries, export data, search SQL scripts |

Each tool has a defined set of parameters and returns structured data that the AI assistant can interpret.

## Setup for Claude Code

To use the MCP server with Claude Code, you need to export the connection configuration and add it to Claude Code's MCP settings.

### Step 1: Export Connection

1. Connect to your OneStream environment in VS Code.
2. Open the Command Palette (`Ctrl+Shift+P`).
3. Run **OneStream: Export MCP Connection**.
4. The command copies the MCP server configuration to your clipboard.

Alternatively, if `onestream.codemcp.autoExportConnection` is enabled, the connection is exported automatically whenever you connect.

### Step 2: Configure Claude Code

Add the exported configuration to your Claude Code MCP settings. The configuration goes in your project's `.mcp.json` file or your global Claude Code configuration:

```json
{
  "mcpServers": {
    "onestream": {
      "command": "node",
      "args": ["/path/to/onestream-mcp-server.js"],
      "env": {
        "ONESTREAM_URL": "https://yourcompany.onestream.com",
        "ONESTREAM_TOKEN": "<your-auth-token>"
      }
    }
  }
}
```

The exact configuration is provided by the export command. Use the exported values directly.

### Step 3: Verify

In Claude Code, ask a question that would require OneStream access, such as:

```
What business rules are available in the current environment?
```

If the MCP server is configured correctly, Claude Code will call the appropriate tool and return results from your OneStream environment.

## Setup for Cursor

Cursor supports MCP servers through its configuration:

1. Export the MCP connection from VS Code (same as Step 1 above).
2. Add the configuration to Cursor's MCP settings (typically in `.cursor/mcp.json`).
3. Restart Cursor to pick up the new configuration.

## Auto-Export Setting

Enable `onestream.codemcp.autoExportConnection` to automatically export the MCP connection configuration whenever you connect to a OneStream environment. This keeps the MCP configuration in sync with your current connection without manual export steps.

## Settings Reference

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.codemcp.autoExportConnection` | boolean | `false` | Automatically export MCP connection when connecting to OneStream |
| `onestream.codemcp.serverPort` | number | `0` | Port for the MCP server (0 = auto-assign) |
| `onestream.codemcp.enabled` | boolean | `true` | Enable or disable the MCP server |
