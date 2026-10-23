# Getting started

All examples target .NET 10. Use project references or the local packages described in the root README.

## Store a plain object

Only `ModularKit.Persistence.Json` is needed:

```csharp
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using ModularKit.Persistence.Json;

IJsonFileStore store = new JsonFileStore(NullLoggerFactory.Instance);
var file = new FileInfo("settings.json");
var options = new JsonSerializerOptions { WriteIndented = true };
await store.SaveAsync(new Settings("localhost"), file, options, CancellationToken.None);
Settings settings = await store.LoadAsync<Settings>(file, options, CancellationToken.None);

public sealed record Settings(string Host);
```

## Define a configuration model

`ConfigEntity` and serialization attributes live in `ModularKit.Configuration`:

```csharp
using ModularKit.Configuration;
using ModularKit.Configuration.Serialization;

[SerializationTopic(SerializationTopics.Config)]
public sealed class DeviceSettings : ConfigEntity
{
    [SerializationTopic(SerializationTopics.Config)]
    public string Host { get; set; } = "localhost";
}
```

Create topic-filtered JSON options using `new SerializationOptionsProvider(new JsonSerializerOptions())`. Its `Config` options include only configuration-marked members. Call `WireUp(null)` on a loaded root before using tree references.

## Add plugin implementations

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularKit.Plugins;

var services = new ServiceCollection();
services.AddModularKitPlugins(new ConfigurationBuilder().Build(), options =>
{
    options.Assemblies = ["plugins/Example.Plugin.dll"];
    options.BaseTypes.Add(typeof(Device));
});
using var provider = services.BuildServiceProvider();
IPluginManager plugins = provider.GetRequiredService<IPluginManager>();
```

`Device` is an abstract class or interface declared in an assembly shared by host and plugin. The plugin implements `IPlugin`, optionally `IConfigurablePlugin`. See [plugins](plugins.md) for a complete declaration.

For a working integration of all four packages, use [DeviceWorkbench](../samples/DeviceWorkbench/README.md). Its source is built with the solution, so it also serves as the executable documentation example.
