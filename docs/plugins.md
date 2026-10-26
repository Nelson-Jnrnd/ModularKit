# Plugins

`ModularKit.Plugins` loads trusted DLLs at startup and discovers implementations of host-declared abstract classes or interfaces.

## Define and register a plugin

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

Entry points need a public parameterless constructor and a unique, nonempty ID. `Configure` receives `Plugins:Settings:<plugin-id>` by default and runs before the service provider is built.

```csharp
services.AddModularKitPlugins(configuration, options =>
{
    options.Assemblies = ["plugins/Example.Plugin.dll"];
    options.BaseTypes.Add(typeof(Device));
});
```

`Device` is an abstract class or interface in an assembly shared by the host and plugin. See the [working sample](../samples/DeviceWorkbench/README.md).

| Option | Meaning |
| --- | --- |
| `Assemblies` | Plugin DLL paths, relative to `BaseDirectory` unless absolute |
| `BaseDirectory` | Defaults to `AppContext.BaseDirectory` |
| `BaseTypes` | Extension points; open generics are unsupported |
| `KnownAssemblies` | Additional assemblies scanned for implementations, without running entry points |
| `SettingsSectionPath` | Defaults to `Plugins:Settings` |
| `TypeAliases` | Additional JSON identifiers accepted on reads |

Missing assemblies, reflection errors, duplicate IDs and configuration failures stop registration.

## Create implementations

```csharp
IPluginManager plugins = provider.GetRequiredService<IPluginManager>();
var choices = plugins.GetTypeOptions(typeof(Device));
object device = plugins.CreateInstance(typeof(Device), "example.tcp-device");
```

Creation uses DI from the scope resolving the manager. Resolve it inside the current scope for scoped dependencies; callers own and dispose created instances. JSON deserialization uses System.Text.Json constructors, so persisted types must also be constructible by that serializer.

## Persisted type identifiers

```csharp
[PluginTypeId("example.tcp-device")]
public sealed class TcpDevice : Device { }
```

The serializer writes this identifier as `$type`; without the attribute it uses `Type.FullName`. Use `BaseJsonSerializerOptions` for polymorphic JSON, or pass those options to Configuration's `SerializationOptionsProvider`.

To read an older identifier after renaming a type:

```csharp
options.TypeAliases.Add("Old.Namespace.Device", typeof(TcpDevice));
```

Aliases do not change the canonical identifier written. Changing that identifier can make new files unreadable by older application versions; retain rollback copies and test saved fixtures before updating installations.

## Deployment limits

- Plugins run with full process permissions. Only load trusted assemblies.
- Loading uses `Assembly.LoadFrom`: restart to replace plugins; unloading, hot reload and dependency-version isolation are unsupported.
- Deploy resolvable dependencies and consistent shared-contract versions. Rebuild plugins when contracts change; no compatibility negotiation is provided.
- Register once before building DI. `Initialize()` does not rescan assemblies. Configure JSON options before first use.
- Reflection-based discovery does not support trimming or Native AOT.
