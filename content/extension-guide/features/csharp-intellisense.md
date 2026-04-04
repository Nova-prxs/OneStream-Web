# C# IntelliSense

The C# IntelliSense feature provides full autocomplete, type information, and documentation for OneStream's API when editing business rules and assemblies. It understands OneStream-specific types like `BRApi`, `SessionInfo`, and their shorthand aliases `api` and `si`.

## What It Provides

### Autocomplete

As you type C# code in a business rule or assembly file, the extension provides autocomplete suggestions for:

- **OneStream API methods**: All methods available on `BRApi` (aliased as `api`), including `api.Finance`, `api.Data`, `api.Security`, and more.
- **Session properties**: All properties on `SessionInfo` (aliased as `si`), such as `si.AppName`, `si.UserName`, `si.ScenarioName`, etc.
- **OneStream types**: Enumerations, data classes, interfaces, and other types from the OneStream SDK.
- **Method signatures**: Parameter names, types, and return types for every method.
- **Documentation**: XML documentation comments appear in the autocomplete tooltip, explaining what each method does and what each parameter means.

### Version-Specific APIs

OneStream's API surface changes between versions. The IntelliSense system is version-aware:

| OneStream Version | API Coverage |
|---|---|
| 7.4 | Legacy API surface |
| 8.0 | Current API surface with new methods and types |
| 9.0+ | Latest API surface including preview features |

The extension detects which version your connected environment runs and provides the matching API definitions. You can also override this with the `onestream.intellisense.version` setting.

## Cross-Platform Support

### Windows (with DLLs)

On Windows, if you have access to OneStream DLL files (the compiled .NET assemblies from the OneStream installation), IntelliSense can resolve types directly from them. This provides the most complete and accurate type information.

To configure DLL-based IntelliSense:

1. Obtain the OneStream DLL files from your server or installation package.
2. Place them in a folder on your machine.
3. Set `onestream.intellisense.dllPath` to the folder path.
4. Run **OneStream: Setup C# IntelliSense** from the Command Palette.

### macOS and Linux (without DLLs)

On macOS and Linux, or on Windows without DLLs, the extension uses embedded type data to provide IntelliSense. This covers all public API types and methods with documentation.

The embedded data is updated with each extension release to match the latest OneStream version. While it covers the vast majority of the API, DLL-based IntelliSense on Windows may provide slightly more complete type resolution for edge cases.

### macOS and Linux (with DLLs)

If you do have access to OneStream DLLs on macOS or Linux (for example, via a network share or copied files), you can still point the extension to them. The extension will attempt to read type information from the DLLs using cross-platform .NET metadata readers.

## Setup

### Automatic Setup

Run **OneStream: Setup C# IntelliSense** from the Command Palette. The extension will:

1. Detect your operating system and OneStream version.
2. Generate an `omnisharp.json` configuration file in your workspace.
3. Configure additional assembly references if DLLs are available.
4. Optionally restart OmniSharp to pick up the new configuration.

### Manual Configuration

If you need to customize the setup, you can edit the following settings directly:

- `onestream.intellisense.dllPath`: Path to a folder containing OneStream DLL files.
- `onestream.intellisense.additionalReferences`: Array of paths to additional .NET assemblies to include in IntelliSense.
- `onestream.intellisense.version`: Override the detected OneStream version.

### OmniSharp Restart

After changing IntelliSense settings, you may need to restart OmniSharp (the C# language server) for changes to take effect. You can do this by:

- Setting `onestream.intellisense.omnisharpRestart` to `true` (the extension restarts OmniSharp automatically after setup).
- Running **OmniSharp: Restart OmniSharp** from the Command Palette manually.

## Local-Only Mode

If you want IntelliSense to work without any server connection (for offline development or in air-gapped environments), enable `onestream.intellisense.localOnly`. In this mode, the extension uses only the embedded type data and any local DLLs you have configured. No network requests are made for IntelliSense.

## Settings Reference

| Setting | Type | Default | Description |
|---|---|---|---|
| `onestream.intellisense.enabled` | boolean | `true` | Enable or disable C# IntelliSense for OneStream types |
| `onestream.intellisense.dllPath` | string | `""` | Path to folder containing OneStream DLL files |
| `onestream.intellisense.autoSetup` | boolean | `true` | Automatically configure IntelliSense when connecting to a new environment |
| `onestream.intellisense.omnisharpRestart` | boolean | `true` | Restart OmniSharp after IntelliSense setup changes |
| `onestream.intellisense.additionalReferences` | array | `[]` | Additional .NET assembly paths to include in IntelliSense |
| `onestream.intellisense.version` | string | `""` | Override the detected OneStream version (e.g., `"8.0"`, `"9.0"`) |
| `onestream.intellisense.versionPaths` | object | `{}` | Map of OneStream versions to DLL folder paths for multi-version support |
| `onestream.intellisense.localOnly` | boolean | `false` | Use only local type data without server connection |
