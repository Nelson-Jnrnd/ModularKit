# Plugins

`ModularKit.Plugins` loads trusted DLLs at application startup and discovers concrete implementations for host-declared abstract classes or interfaces. It has no dependency on Configuration or Blazor.

## Define a plugin

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularKit.Plugins;

public sealed class ExamplePlugin : IConfigurablePlugin
{
    public string Id => "example.devices";
    public string Name => "Example devices";

    public void Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(new DeviceDefaults(configuration["DefaultHost"] ?? "localhost"));
    }
}

public sealed record DeviceDefaults(string Host);
```

Plugin entry points require a public parameterless constructor. Their `Configure` method receives `Plugins:Settings:<plugin-id>` by default. Register dependencies there; resolve them after the host builds its service provider. Plugin IDs must be nonempty and unique across loaded assemblies.

## Register with the host

```csharp
services.AddModularKitPlugins(configuration, options =>
{
    options.Assemblies = ["plugins/Example.Plugin/Example.Plugin.dll"];
    options.BaseTypes.Add(typeof(Device));
    // Optional assemblies containing implementations already referenced by the host:
    options.KnownAssemblies.Add(typeof(BuiltInDevice).Assembly);
    options.TypeAliases.Add("Old.Namespace.Device", typeof(BuiltInDevice));
});
```

| Option | Meaning |
| --- | --- |
| `Assemblies` | Plugin DLL paths; relative paths are resolved against `BaseDirectory` |
| `BaseDirectory` | Defaults to `AppContext.BaseDirectory` |
| `BaseTypes` | Host-owned extension points: abstract classes or interfaces, no open generics |
| `KnownAssemblies` | Additional assemblies scanned for implementations; their plugin entry points are not executed unless also listed in `Assemblies` |
| `SettingsSectionPath` | Defaults to `Plugins:Settings` |
| `TypeAliases` | Extra persisted identifiers accepted on reads; canonical identifiers remain unchanged |

Only implementations in loaded, known, or base-type assemblies are registered. Reflection-load errors, missing DLLs, duplicate plugin IDs, ambiguous type identifiers and configuration errors stop registration. Starting a partially configured application should be an explicit host decision.

## Choose and create an implementation

```csharp
IPluginManager plugins = provider.GetRequiredService<IPluginManager>();
foreach (PluginTypeOption choice in plugins.GetTypeOptions(typeof(Device)))
    Console.WriteLine(choice.DisplayName);
object device = plugins.CreateInstance(typeof(Device), "example.tcp-device");
```

Instance creation uses `ActivatorUtilities`, so constructor dependencies can come from DI. The caller owns returned instances; they are not automatically disposed by the container. JSON deserialization uses System.Text.Json's constructors, not this DI factory. Persisted configuration types therefore also need to be constructible by System.Text.Json; the sample uses parameterless constructors and writable properties.

The manager is resolved transiently over a shared startup registry. Resolve it from the current DI scope when implementations need scoped services. A manager resolved from the root provider cannot safely instantiate scoped dependencies.

## Stable JSON identifiers

```csharp
[PluginTypeId("example.tcp-device")]
public sealed class TcpDevice : Device { }
```

The registry writes `$type` with this identifier. Without the attribute it uses `Type.FullName`. Renaming a namespace then requires an explicit read alias. Aliases do not create duplicate choices in the editor and do not change what is written.

```json
{
  "$type": "example.tcp-device",
  "Name": "Sensor 1"
}
```

Use `IPluginManager.BaseJsonSerializerOptions` for ordinary polymorphic JSON. If Configuration is also used, pass those options into `SerializationOptionsProvider`. Configure options before their first serialization; System.Text.Json makes them read-only after use.

## Deployment and limits

- Plugins run with the process's full permissions. Only load trusted assemblies.
- The current loader uses `Assembly.LoadFrom`, with no unload, hot replacement or dependency-version isolation. Restart after changing a plugin.
- Put host/plugin contracts in shared assemblies. Do not ship a different version of a shared contract beside each plugin.
- Deploy plugin dependencies so the runtime can resolve them; arbitrary isolated dependency trees are not supported by this loader. The sample plugin needs only assemblies already referenced by its host.
- No runtime API compatibility negotiation is implemented. Build and test plugins against the same ModularKit and shared-contract versions as the host.
- Trimming and Native AOT are not supported for reflection-discovered types.
- Call `AddModularKitPlugins` once before building the service provider. `Initialize()` is idempotent; it does not rescan assemblies.
