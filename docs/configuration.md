# Configuration

`ModularKit.Configuration` models named object trees, references and configuration/state serialization.

## Define a model

```csharp
using ModularKit.Configuration;
using ModularKit.Configuration.References;
using ModularKit.Configuration.Serialization;
using ModularKit.Configuration.View;

[SerializationTopic(SerializationTopics.Config)]
public sealed class Installation : ConfigEntity
{
    [SerializationTopic(SerializationTopics.Config)]
    [ComponentAssociation("Devices", AssociationType.Composition)]
    public List<Device> Devices { get; set; } = [];

    [SerializationTopic(SerializationTopics.Config)]
    [ComponentAssociation("Primary device", AssociationType.Aggregation)]
    public ConfigRef<Device> PrimaryDevice { get; set; } = new();
}

[SerializationTopic(SerializationTopics.Config)]
public abstract class Device : ConfigEntity { }
```

Composition owns children; references link objects already owned elsewhere. See the [sample](../samples/DeviceWorkbench/README.md) for a concrete device implementation.

## Lifecycle

1. Construct or deserialize the tree, then call `root.WireUp(null)` to assign parents, resolve references and check required direct associations.
2. Rewire from the root after structural edits.
3. Validate fields and application rules, then call `PrepareConfigReferencesForPersistence()` before saving.

Derived objects using the protected `LoggerFactory` property require `root.SetLoggerFactory(factory)` before wire-up. Trees must be acyclic, with one owner per entity. Editing and wire-up mutate objects; the host coordinates concurrent access and recovery.

## Names and references

`Path` joins ancestor names with `/`. Names must be unique among siblings and cannot contain `/`, `,` or `|`. Renaming a wired entity repairs references into its subtree; changing a `[Display]` label does not change identity. Moving objects between owners requires application handling.

`ConfigRef<T>` allows no selection. `RequiredConfigRef<T>` rejects clearing an assigned value; missing required or unresolved references block persistence with `ConfigIntegrityException`.

```json
{ "PrimaryDevice": { "Path": "Installation/Sensor 1" } }
```

Only the path is serialized. `Value` resolves to the existing descendant object; references to the root are unsupported. Broken paths remain available for diagnosis. `ClearBrokenConfigReferences()` explicitly clears broken optional references and returns descriptions.

## Attributes

| Attribute | Purpose |
| --- | --- |
| `SerializationTopic` | Includes a type or property in configuration, state or a custom topic |
| `ComponentAssociation` | Composition creates an owned object; aggregation selects an existing one |
| `Display` | Label, optionally from resources |
| `Required`, `Range`, other DataAnnotations | Editor field validation |
| `Unique` | Editor sibling uniqueness; `Group` combines fields |
| `ConfigFormHidden` | Hides a property from generated forms |
| `ConfigTreeIcon` | Icon for the optional editor |
| `Inline` | Displays a component inline |
| `AutoNamed` | Generates an invariant name on creation |
| `ScalarOptions` | Choices supplied by an `IScalarOptionsProvider` |

Name/path rules are enforced by `ConfigEntity`. Other field validation must also be applied by hosts that bypass the editor.

## Configuration and state JSON

```csharp
var options = new SerializationOptionsProvider(baseJsonOptions);
string config = JsonSerializer.Serialize(root, options.Config);
string state = JsonSerializer.Serialize(root, options.State);
```

Both types and properties need the matching topic; unmarked members are omitted. Use plain `JsonSerializerOptions` for ordinary objects. Serialization uses reflection, respects nullable annotations and requires direct component associations marked as required.

When updating a model, preserve member names and reference paths. Load representative saved files, rewire and check references, then save to a new path and compare values before replacing installation files. Type-identifier changes use [plugin aliases](plugins.md#persisted-type-identifiers).
