# Softponics migration and compatibility

## Current status

The generic implementation has been extracted into ModularKit. The original Softponics code and project references are unchanged. Softponics has not yet been rebuilt against these packages, and existing installation files have not been migrated. This separation allows the libraries to be validated before changing equipment-facing software.

## What was retained

- Configuration member names and `Config`/`State` serialization topic names.
- Path-based references and reference repair on renaming a wired subtree.
- The `$type` property and the default `Type.FullName` discriminator algorithm.
- Temporary-file persistence and `.bak` naming.
- The configuration editor's existing composition and reference-selection behaviours.

Compatibility tests use synthetic device objects and legacy discriminator aliases. They demonstrate the mechanism; they are not proof that every Softponics installation file can be loaded unchanged.

## Required integration changes

| Existing Softponics area | ModularKit replacement | Host-specific work |
| --- | --- | --- |
| `GreenTrack.Core.PluginManagement` | `ModularKit.Plugins` | Register the existing base types and plugin paths |
| `ConfigEntity`, configuration references and view metadata | `ModularKit.Configuration` | Update namespaces and preserve application-specific derived classes |
| Configuration/state serialization | `ModularKit.Configuration.Serialization` | Supply the plugin registry's JSON options |
| Configuration tree and selector components | `ModularKit.Configuration.Blazor` | Register `IConfigTypeCatalog` adapter, localization and host edit gate |
| `IJsonFileStore`, `JsonFileStore` | `ModularKit.Persistence.Json` | Retain storage paths and host version/conflict checks |

The old `IPlugin` contract and the new contract are different CLR types. Existing plugin binaries must be rebuilt; changing only the loader is insufficient. Shared contracts must use one consistent version throughout the host and plugins.

## Preserve saved files

1. Work from copies of representative configuration and state files; retain an untouched backup.
2. Inventory every persisted `$type` value and path before changing namespaces.
3. Keep domain classes in their existing namespaces where possible. If a persisted type is renamed, register its old full name in `PluginManagerOptions.TypeAliases` against the new concrete type.
4. Do not change entity names to match new display labels. Names form persisted paths.
5. Load each fixture, wire the tree and run reference-integrity checks. Confirm required associations, concrete types and referenced object identities.
6. Save to a new path and compare semantic values, not whitespace. If canonical identifiers change, record that write-side format change explicitly and plan rollback.
7. Run the original Softponics tests and simulator checks before replacing packages in an installation.

ModularKit aliases are read aliases. A new canonical identifier can make a newly saved file unreadable by the old Softponics binary. Do not overwrite the only rollback copy. The extraction does not promise automatic downgrade compatibility.

## Deliberate differences

The new loader stops registration on invalid or duplicate plugin/type definitions and plugin configuration failures. The previous loader logged some failures and continued. Configuration and serialization are independent of the plugin package, and selected operations previously internal to the monolithic Core are now public across package boundaries.

No machine-control behaviour, alert handling, identity rules or database logic has been moved into these libraries.
