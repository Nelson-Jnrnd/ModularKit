# Compatibility

Applications using ModularKit own their configuration formats and upgrade policy. Keep persisted type identifiers, member names and object paths stable when updating libraries or application models.

## Type identifiers

Polymorphic JSON uses a `$type` identifier. Give new implementations a stable `PluginTypeId` rather than relying on their CLR namespace. If a persisted type is renamed, register its old identifier as a read alias:

```csharp
options.TypeAliases.Add("Old.Namespace.Device", typeof(DeviceImplementation));
```

An alias accepts older JSON without changing the canonical identifier written for that type. If the canonical identifier changes, older application versions may no longer read newly saved files.

## Configuration paths

Entity names form persisted reference paths. Changing a `[Display]` label does not change identity, but renaming an entity does. Rename repair updates references on a wired tree; moving objects between owners requires application validation.

## Validate an update

1. Retain untouched copies of representative configuration and state files.
2. Inventory persisted `$type` values and reference paths before changing models.
3. Load the copied files, call `WireUp(null)` and check reference integrity.
4. Confirm concrete types, required associations and referenced object identities.
5. Save to a new path and compare semantic values, including reference paths.
6. Test the host and its plugins together using consistent shared-contract versions.
7. Confirm rollback before overwriting installation files.

Existing plugin binaries must be rebuilt when their shared CLR contracts change. Updating a loader alone does not make incompatible binaries compatible. See [plugins](plugins.md) for deployment limits and [configuration](configuration.md) for model lifecycle details.
