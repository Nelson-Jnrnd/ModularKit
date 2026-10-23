# Architecture

The application chooses the available extension points and connects the packages. A library does not know about greenhouse equipment or the host's user accounts.

## Package dependencies

An arrow means “references”. The host adapts the plugin registry to `IConfigTypeCatalog`; this does not add a dependency between the two packages.

```mermaid
flowchart TD
    Host["Application host"] --> Editor["Configuration.Blazor"]
    Editor --> Configuration["Configuration"]
    Host --> Plugins["Plugins"]
    Host --> Persistence["Persistence.Json"]
    Host --> Configuration
```

| Package | External dependencies | Does not own |
| --- | --- | --- |
| Plugins | Microsoft.Extensions dependency injection, configuration and logging abstractions; System.Text.Json | Host model, web components, plugin settings schema |
| Configuration | Logging abstractions; System.Text.Json | Plugin loading, file paths, users, database |
| Configuration.Blazor | ASP.NET Core, MudBlazor, Configuration | Persistence policy, authentication, available implementations |
| Persistence.Json | Logging abstractions; System.Text.Json | Model validation, serializers specific to an application |

## Configuration objects and references

Composition belongs in serialized child properties. A `ConfigRef` points to an object already owned elsewhere in the tree; its `Value` is not serialized again.

```mermaid
classDiagram
    class ConfigEntity {
        <<abstract>>
        +string Name
        +string Path
        +ConfigEntity Parent
        +WireUp(owner)
        +PrepareConfigReferencesForPersistence()
    }
    class Workbench {
        +List~Device~ Devices
        +ConfigRef~Device~ SelectedDevice
    }
    class Device {
        <<abstract>>
    }
    class ConfigRef {
        +string Path
        +ConfigEntity Value
    }
    ConfigEntity <|-- Workbench
    ConfigEntity <|-- Device
    Workbench "1" *-- "0..*" Device : owns
    Workbench "1" *-- "1" ConfigRef : selection
    ConfigRef --> Device : resolves path
```

## Editor extension boundary

The editor receives choices and an instance factory through a configuration-owned interface. Hosts can implement this with a fixed list, a plugin registry, or another source.

```mermaid
classDiagram
    class IConfigTypeCatalog {
        <<interface>>
        +GetTypeOptions(baseType)
        +CreateInstance(baseType, discriminator)
    }
    class DefaultConfigEntityFactory {
        +CreateAsync(type, owner, displayName)
        +CreateForAttachmentAsync(type, owner, displayName)
        +SelectExistingAsync(owner, displayName, type, candidates)
    }
    class HostAdapter
    class IPluginManager {
        <<interface>>
        +GetTypeOptions(baseType)
        +CreateInstance(baseType, discriminator)
    }
    DefaultConfigEntityFactory --> IConfigTypeCatalog
    HostAdapter ..|> IConfigTypeCatalog
    HostAdapter --> IPluginManager
```

## Startup and configuration loading

Plugin configuration runs before the DI service provider is built. JSON deserialization uses the discovered implementation registry; arbitrary `$type` names are rejected.

```mermaid
sequenceDiagram
    participant Host
    participant Plugins
    participant Services
    participant Store as JSON store
    participant Model
    Host->>Plugins: Load assemblies and declare base types
    Plugins->>Services: Register plugin services and registry
    Host->>Services: Build service provider
    Host->>Store: Load using registry and topic options
    Store-->>Host: Deserialized model
    Host->>Model: WireUp(null)
    Model-->>Host: Connected tree and resolved references
```

## Save flow

1. The host applies its edit permission and concurrency policy.
2. The editor mutates the model and raises `OnConfigChanged`.
3. The host rewires the tree and performs application validation.
4. `PrepareConfigReferencesForPersistence` rejects missing required or unresolved references.
5. The JSON store serializes with caller-supplied options and replaces the destination.

Reference integrity and field validation are different checks. A host using the model without the editor must enforce its own DataAnnotations and application rules.
