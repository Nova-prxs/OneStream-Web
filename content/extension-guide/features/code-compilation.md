# Code Compilation

The Code Compilation feature lets you compile OneStream assemblies and individual business rules directly from VS Code, with errors and warnings reported in the VS Code Problems panel. You can click on any error to jump directly to the source line.

## Prerequisites

Code compilation requires the **OCE (OneStream Compilation Engine) component** to be installed on your OneStream server. Without this component, compilation commands will not be available.

Check with your OneStream administrator to confirm that the OCE component is deployed and accessible.

## Compiling Assemblies

To compile a workspace assembly:

1. Open any file from the assembly you want to compile.
2. Press `Ctrl+K Ctrl+B` or run **OneStream: Compile Assembly** from the Command Palette.
3. The extension sends all source files in the assembly to the server for compilation.
4. Results appear in the VS Code Problems panel.

### What Happens During Compilation

1. **Auto Push**: Before compilation, the extension automatically pushes any unsaved or modified assembly files to the server. This ensures the server compiles the latest version of your code.
2. **Server Compilation**: The OCE component on the server compiles the assembly against the OneStream SDK and any referenced assemblies.
3. **Result Reporting**: Compilation errors, warnings, and informational messages are returned and displayed in the VS Code Problems panel.

## Compiling Individual Rules

You can also compile individual business rules without compiling an entire assembly. The extension supports compiling:

- **Finance Rules**: Calculations, consolidations, translations.
- **Extender Rules**: Custom business logic.
- **Connector Rules**: Data integration logic.
- **Parser Rules**: File parsing and transformation logic.

To compile a single rule:

1. Open the rule file in the editor.
2. Run **OneStream: Compile Rule** from the Command Palette.
3. The rule is pushed to the server and compiled.
4. Results appear in the Problems panel.

## VS Code Problems Panel Integration

All compilation results are reported through VS Code's built-in Problems panel, providing a consistent experience with other language tools.

### Error Display

Each compilation error shows:

- **Severity**: Error, Warning, or Information icon.
- **Message**: The compiler's error or warning message.
- **File**: The source file where the issue was found.
- **Line and Column**: The exact position in the source code.

### Click to Navigate

Click any error or warning in the Problems panel to jump directly to the problematic line in the source file. The cursor is positioned at the exact column where the issue was detected.

This makes fixing compilation errors fast -- you can click through each error, fix it, and recompile without leaving the editor.

### Filtering

The Problems panel supports filtering by:

- **Severity**: Show only errors, only warnings, or all.
- **File**: Show issues for a specific file only.
- **Text**: Search through error messages.

## Auto Push Before Compile

When you trigger compilation, the extension automatically pushes the current state of your files to the server before compiling. This ensures that the compilation reflects your latest local changes.

If the push fails (for example, due to a network issue), the compilation is aborted and you are notified of the push failure.

## Keyboard Shortcuts

| Shortcut | Command | Description |
|---|---|---|
| `Ctrl+K Ctrl+B` | OneStream: Compile Assembly | Compile the assembly containing the current file |

## Workflow Tips

### Compile-Fix-Compile Cycle

1. Press `Ctrl+K Ctrl+B` to compile.
2. Review errors in the Problems panel.
3. Click an error to navigate to the source.
4. Fix the issue.
5. Press `Ctrl+K Ctrl+B` again.
6. Repeat until the build succeeds.

### Pre-Push Validation

Use compilation as a validation step before pushing changes to production. Compile locally first, fix all errors, then push the verified code.
