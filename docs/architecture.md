# Architecture

The host connects independent libraries through its own models and adapters.

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

## Configuration objects and references

Composition owns children; references point to existing objects by path.

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

The host implements `IConfigTypeCatalog` using a fixed list or a plugin registry.

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

Plugin services are registered before DI is built; deserialization uses the registered type identifiers.

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

For the save lifecycle and validation rules, see [Configuration](configuration.md#lifecycle) and [JSON persistence](persistence.md#write-lifecycle).
