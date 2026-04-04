# AI Assistant Integration

The AI Assistant Integration feature auto-generates context documentation that helps AI coding assistants understand your OneStream project. It produces a `CLAUDE.md` file and related context files so that tools like Claude Code, Cursor, and GitHub Copilot can provide more relevant suggestions.

## What It Does

When you run the setup command, the extension generates documentation files that describe:

- **Project structure**: What the folders and files in your workspace represent.
- **Rule types**: What each business rule type (Finance, Extender, Connector, Parser) does and how it is used.
- **API surface**: Key OneStream API objects (`BRApi`, `SessionInfo`, `api`, `si`) and their most-used methods.
- **Conventions**: Naming conventions, coding patterns, and best practices specific to OneStream development.
- **Environment details**: The connected OneStream version, application name, and available dimensions.

This documentation is written in a format that AI assistants can consume to provide contextually accurate code suggestions, explanations, and completions.

## Setup

### Generating Documentation

1. Open the Command Palette (`Ctrl+Shift+P`).
2. Run **OneStream: Generate AI Context** (or **OneStream: Generate CLAUDE.md**).
3. The extension generates the context files in your workspace root.

### Generated Files

The primary file generated is `CLAUDE.md` at the workspace root. This file follows the Claude Code convention for project documentation and includes:

- A project overview.
- Folder structure explanation.
- API reference summary.
- Common patterns and anti-patterns.
- Environment-specific notes.

Additional context files may be generated depending on your configuration.

## Compatible AI Assistants

### Claude Code

Claude Code automatically reads `CLAUDE.md` when you open a terminal in the workspace. No additional configuration is needed. The generated documentation gives Claude Code full awareness of your OneStream project structure and APIs.

### Cursor

Cursor reads project documentation from multiple sources. The generated `CLAUDE.md` file is picked up automatically. You can also reference it from Cursor's `.cursorrules` file for additional emphasis:

```
@CLAUDE.md
```

### GitHub Copilot

GitHub Copilot benefits from the context files when they are open in your editor tabs. For best results:

1. Open `CLAUDE.md` in a tab.
2. Pin the tab so it stays open.
3. Copilot uses the content as context for its suggestions.

## MCP Server Integration

For deeper AI integration, the extension includes an embedded MCP (Model Context Protocol) server that gives AI assistants direct access to OneStream operations. See the [MCP Server Overview](../mcp-server/overview.md) for details on setting up and using the MCP server with your AI tools.

The MCP server provides tools that let AI assistants:

- Read and search business rules.
- Query the database.
- Browse dashboard structures.
- Access OneStream documentation.

## Keeping Documentation Updated

Regenerate the AI context documentation whenever you make significant changes to your project:

- After pulling new rules from the server.
- After adding or removing assemblies.
- After connecting to a different OneStream environment.
- After upgrading to a new OneStream version.

You can also commit the generated files to Git so all team members benefit from the same AI context.

## Privacy

The generated documentation files contain information about your OneStream project structure and API surface. They do not contain:

- Passwords or authentication tokens.
- Actual data from your OneStream databases.
- Business rule source code (unless you choose to include it).

Review the generated files before committing them to ensure they do not contain any information you consider sensitive.
