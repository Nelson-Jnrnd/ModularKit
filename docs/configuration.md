# Configuration

`ModularKit.Configuration` models a tree of named objects. It includes reference handling and metadata that the optional editor uses. It does not load plugins or write files.

## Declare the tree

```csharp
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
```

Imports: `ModularKit.Configuration`, `.References`, `.Serialization` and `.View`. `Device` must derive from `ConfigEntity`; it can be abstract when a host-supplied catalog provides implementations.

## Object lifecycle

1. Construct or deserialize the complete tree.
2. Optionally call `root.SetLoggerFactory(factory)` if derived runtime objects use its protected `LoggerFactory` property.
3. Call `root.WireUp(null)`. This assigns parents, resolves reference paths, checks required direct component associations and invokes reference-resolution hooks.
4. After adding or changing components, call `WireUp(null)` again from the root.
5. Apply field and application validation, then call `PrepareConfigReferencesForPersistence()` before saving.

The host owns transaction, concurrent editing and error recovery policy. Wire-up and renaming mutate the tree; they are not transactional and do not provide thread safety. Composition must be acyclic, with one owner for each entity. Use references for shared objects.

## Identity and references

`Path` is the parent path followed by `Name`, separated by `/`. Names cannot contain `/`, `,` or `|`. Names must be unique among siblings. A rename on a wired tree rewrites reference paths pointing to the renamed subtree. Changing a UI label with `[Display]` does not change object identity.

`ConfigRef<T>` allows no selection. `RequiredConfigRef<T>` rejects clearing an assigned value, and a missing target blocks persistence. References are serialized as paths; `Value` resolves to the existing object, not a cloned copy.

```json
{
  "PrimaryDevice": {
    "Path": "Installation/Sensor 1"
  }
}
```

Unresolved references retain their original paths for diagnosis. `ClearBrokenConfigReferences()` explicitly clears broken optional references and returns descriptions; required references remain errors. `PrepareConfigReferencesForPersistence()` rejects unresolved paths and missing required references with `ConfigIntegrityException`.

Paths describe composition ancestry; the current resolver targets descendants of the root. References to the root itself are not supported. Removing a subtree or moving it between owners needs host validation; rename repair does not implement arbitrary moves.

## Metadata

| Attribute | Purpose |
| --- | --- |
| `SerializationTopic` | Includes a class or property in configuration, state or a custom serialization topic |
| `ComponentAssociation` | Describes a component slot; composition creates an owned object, aggregation selects an existing one |
| `Display` | User-facing name, optionally from a resource class |
| `Required`, `Range`, other DataAnnotations | Editor field validation |
| `Unique` | Editor uniqueness validation among siblings; `Group` forms a composite constraint |
| `ConfigFormHidden` | Hides a property from generated forms |
| `ConfigTreeIcon` | An icon name understood by the optional MudBlazor editor |
| `Inline` | Displays a component inline |
| `AutoNamed` | Generates an invariant name when adding an entity |
| `ScalarOptions` | Supplies choices through an `IScalarOptionsProvider` |

`ConfigEntity` enforces its name/path rules itself. Other DataAnnotations and `Unique` fields are editor checks, not automatic domain-wide validation. A host that bypasses the editor must enforce those rules itself.

## Configuration versus state JSON

```csharp
var options = new SerializationOptionsProvider(baseJsonOptions);
string configJson = JsonSerializer.Serialize(root, options.Config);
string stateJson = JsonSerializer.Serialize(root, options.State);
```

Both types and properties must have the matching topic. Unmarked members are omitted; an unmarked object's properties become empty. This is deliberate opt-in serialization, not general-purpose DTO serialization. Use plain `JsonSerializerOptions` when that filtering is unwanted.

The serializer checks nullable annotations and required direct component associations. Attribute filtering relies on reflection. Configuration types should have public writable properties supported by System.Text.Json. See the [compatibility guide](compatibility.md) before changing persisted identifiers or paths.
