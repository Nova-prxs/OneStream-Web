# Security and Privacy

The OneStream AI Helper extension is designed with security as a priority. This page documents the security measures, privacy practices, and safety features built into the extension.

## No Telemetry or Tracking

The extension does not collect, transmit, or store any telemetry data. Specifically:

- **No usage analytics**: The extension does not track which features you use, how often you use them, or how long your sessions last.
- **No crash reporting**: Errors and exceptions are not reported to any external service.
- **No user profiling**: No behavioral data is collected or transmitted.
- **No third-party analytics**: No analytics libraries (Google Analytics, Mixpanel, Segment, etc.) are included.

All extension functionality operates between your VS Code instance and your OneStream server. No data is sent to any other destination.

## Encrypted Connections

All communication between the extension and the OneStream server uses HTTPS/TLS encryption.

### What This Means

- **Data in transit is encrypted**: Your queries, rule source code, credentials, and all other data are encrypted during transmission using TLS (Transport Layer Security).
- **Certificate validation**: The extension validates the server's TLS certificate to prevent man-in-the-middle attacks. Self-signed certificates are supported but require explicit configuration.
- **No HTTP fallback**: The extension does not fall back to unencrypted HTTP connections. If HTTPS is not available, the connection fails.

### Requirements

- Your OneStream server must have a valid TLS certificate (self-signed or CA-signed).
- Port 443 (or your configured HTTPS port) must be accessible from your workstation.
- TLS 1.2 or later is required.

## Authentication

### Token-Based Authentication

The extension uses token-based authentication to interact with the OneStream server:

1. **Initial login**: You provide your username and password when connecting. These credentials are sent over HTTPS to the OneStream authentication endpoint.
2. **Token issuance**: The server returns an authentication token (session token).
3. **Subsequent requests**: All further communication uses the token. Your password is not sent again.
4. **Token expiry**: Tokens have a limited lifetime. When a token expires, the extension prompts you to reconnect.

### No Passwords Stored

Your password is never stored by the extension -- not in VS Code settings, not in the workspace, not on disk. The password is used only for the initial authentication request and is discarded from memory immediately after the token is received.

### Encrypted Token Storage

The authentication token is stored in VS Code's built-in Secret Storage API, which provides:

- **OS-level encryption**: On Windows, tokens are stored in the Windows Credential Manager. On macOS, they are stored in the Keychain. On Linux, they are stored using the system's secret service (e.g., GNOME Keyring or KDE Wallet).
- **Per-user isolation**: Tokens are accessible only to your VS Code instance running under your OS user account.
- **Automatic cleanup**: Tokens are removed when you disconnect or when VS Code is closed.

## SQL Safety Checks

The SQL Query Editor includes safety features to prevent accidental or unauthorized data modification.

### Default Protections

When safety is enabled (the default):

- **SELECT only**: Only `SELECT` statements are allowed. Modifying statements (`INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER`, `TRUNCATE`) are blocked.
- **WHERE clause required**: Even when safety is relaxed to allow modifications, `UPDATE` and `DELETE` statements must include a `WHERE` clause to prevent full-table operations.
- **Confirmation dialogs**: Destructive operations show a confirmation dialog before executing.
- **No PRINT or GO**: Statements that could interfere with result handling or batch processing are blocked.

### Protected Tables

You can configure a list of tables that can never be modified, regardless of the safety setting. This is a defense-in-depth measure to protect critical system tables even if safety checks are accidentally disabled.

Configure protected tables via `onestream.sql.protectedTables` in settings.

### MCP Tool Safety

The SQL tools exposed through the MCP server respect the same safety settings. An AI assistant cannot bypass SQL safety checks -- the same rules that apply in the SQL Query Editor apply to MCP tool calls.

## Data Handling

### Local Storage

The extension stores the following data locally in your workspace:

- **Business rule source files**: Downloaded as `.cs` files in your workspace folders.
- **Assembly source files**: Downloaded as `.cs` files under the `Assemblies/` folder.
- **Data Management metadata**: Stored as `.json` files under `.onestream-metadata/`.
- **Query history**: Stored in VS Code's local storage (not accessible to other extensions).
- **IntelliSense configuration**: Stored as workspace settings and `.json` files.

### What Is Not Stored

- **Passwords**: Never stored anywhere.
- **Cube data**: Not downloaded or cached unless you explicitly export it.
- **User data**: No data about other users in the system is stored.
- **Server configuration**: No server-side settings or security configurations are stored locally.

### AI Context Files

When you generate AI context documentation (e.g., `CLAUDE.md`), the generated files contain:

- Project structure descriptions.
- API surface summaries.
- Coding conventions.

They do not contain passwords, tokens, cube data, or other sensitive information. Review generated files before committing them to version control.

## System Requirements Summary

| Requirement | Details |
|---|---|
| VS Code | 1.60 or later |
| OneStream | 8.0 or later |
| Operating System | Windows, macOS, or Linux |
| Network | HTTPS access (port 443) to the OneStream server |
| TLS | Version 1.2 or later |
| Authentication | OneStream user account with appropriate permissions |

## Best Practices

### Credential Management

- Never share your OneStream credentials or authentication tokens.
- Use a strong, unique password for your OneStream account.
- Disconnect the extension when not in use to invalidate the session token.
- If you suspect your token has been compromised, disconnect and reconnect to obtain a new token.

### SQL Safety

- Keep SQL safety enabled (`onestream.sql.safetyEnabled: true`) during normal development.
- Only disable safety checks when you specifically need to run modifying queries.
- Always configure `protectedTables` for critical system tables.
- Enable `requireWhereClause` to prevent accidental full-table operations.
- Enable `confirmDestructive` for an additional layer of protection.

### File Security

- Add `.onestream-metadata/` to your `.gitignore` if you do not want metadata committed to version control.
- Review generated AI context files before committing them.
- Do not commit files containing connection URLs or environment-specific information to public repositories.
- Use VS Code's workspace trust feature to control extension behavior in untrusted workspaces.

### Network Security

- Connect to OneStream only over trusted networks.
- Use a VPN when connecting from external networks.
- Verify that your OneStream server's TLS certificate is valid and not expired.
- Report any certificate warnings to your system administrator.
