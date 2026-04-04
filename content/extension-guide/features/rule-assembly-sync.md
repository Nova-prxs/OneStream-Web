# Rule & Assembly Sync

The Rule & Assembly Sync feature lets you download business rules and assemblies from OneStream to your local workspace, edit them in VS Code with full editor capabilities, and push changes back to the server. Combined with Git, this gives you version control over your OneStream code.

## Concepts

### Rules

OneStream business rules are C# code files that run on the server. They come in several types:

- **Finance Rules**: Calculations, consolidations, translations, and other financial logic.
- **Extender Rules**: Custom business logic triggered by events or dashboards.
- **Connector Rules**: Data import/export logic for integrations.
- **Parser Rules**: Data transformation rules for file parsing.

### Assemblies

Assemblies are compiled .NET libraries that can be referenced by business rules. They are organized by workspace in OneStream. Each assembly can contain multiple C# source files.

## Pulling Rules

### Pull a Single Rule

To download the current rule you are editing:

1. Open a rule file in the editor.
2. Press `Ctrl+K Ctrl+Down` or run **OneStream: Pull Rule** from the Command Palette.
3. The local file is updated with the latest version from the server.

If the local file has unsaved changes, you will be prompted to save or discard them before the pull overwrites the file.

### Pull All Unencrypted Rules

To download all business rules from the server:

1. Open the Command Palette (`Ctrl+Shift+P`).
2. Run **OneStream: Pull All Unencrypted Rules**.
3. A progress notification shows the download status.

This downloads every business rule that is not encrypted, organized into folders by rule type. Encrypted rules are skipped because their source code cannot be read.

This operation may take several minutes in environments with many rules. You can continue working while it runs in the background.

## Pushing Rules

### Push a Single Rule

To upload your local changes to the server:

1. Open the rule file you want to push.
2. Press `Ctrl+K Ctrl+Up` or run **OneStream: Push Rule** from the Command Palette.
3. The extension uploads the file content to OneStream.

A confirmation message appears on success. If there is a conflict (the server version was modified since your last pull), you will be warned and can choose to overwrite or cancel.

### Push Behavior

- The file must be saved before pushing. Unsaved changes are not pushed.
- Only the currently open file is pushed. There is no "push all" command for safety reasons -- each push is an intentional action.
- The push replaces the server version of the rule with your local version.

## Pulling Assemblies

To download assembly source files:

1. Open the Command Palette.
2. Run **OneStream: Pull Assembly**.
3. Select the workspace that contains the assembly.
4. The assembly files are downloaded to `Assemblies/<WorkspaceName>/` in your workspace.

Assembly files are standard C# source files that you can edit, refactor, and version control just like business rules.

## Compiling Assemblies

After editing assembly files, compile them to verify they build correctly:

1. Press `Ctrl+K Ctrl+B` or run **OneStream: Compile Assembly** from the Command Palette.
2. The extension sends the assembly source to the server for compilation.
3. Results appear in the VS Code Problems panel.

Compilation requires the OCE component to be installed on the OneStream server. See the [Code Compilation](code-compilation.md) guide for details.

## Sync Status Report

The extension can show you which rules have changed locally versus on the server. This helps you understand what needs to be pushed or pulled before making changes.

When sync status checking is enabled (via settings), the extension periodically compares local file timestamps and content hashes with the server versions. Files with differences are marked in the Explorer with an indicator.

## Git Version Control Integration

Because rules and assemblies are stored as regular files in your workspace, you can use Git (or any version control system) to track changes.

### Recommended Workflow

1. **Pull** all rules from OneStream to your workspace.
2. **Initialize a Git repository** in the workspace (if not already done).
3. **Commit** the initial pull as your baseline.
4. **Edit** rules in VS Code with full IntelliSense support.
5. **Commit** your changes with descriptive messages.
6. **Push** the changed rules back to OneStream.
7. **Pull** periodically to sync server-side changes made by other users.

### Benefits

- **History**: See who changed what and when.
- **Branching**: Test changes in branches before pushing to the server.
- **Code Review**: Use pull requests to review rule changes before deploying.
- **Rollback**: Revert to any previous version if a change causes issues.

## Commands Reference

| Command | Shortcut | Description |
|---|---|---|
| OneStream: Pull Rule | `Ctrl+K Ctrl+Down` | Download the current rule from the server |
| OneStream: Pull All Unencrypted Rules | -- | Download all non-encrypted rules |
| OneStream: Push Rule | `Ctrl+K Ctrl+Up` | Upload the current rule to the server |
| OneStream: Pull Assembly | -- | Download assembly files by workspace |
| OneStream: Compile Assembly | `Ctrl+K Ctrl+B` | Compile the current assembly on the server |

## Settings Reference

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.sync.autoCheckSyncStatus` | boolean | `false` | Automatically check sync status between local files and server |
| `onestream.sync.syncCheckInterval` | number | `300` | Seconds between automatic sync status checks |
