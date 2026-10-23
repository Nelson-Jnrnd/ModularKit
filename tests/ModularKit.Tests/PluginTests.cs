using System.Text.Json;
using DeviceWorkbench.Contracts;
using DeviceWorkbench.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularKit.Plugins;
using ModularKit.Configuration.Serialization;
using Xunit;

namespace ModularKit.Tests;

public class PluginTests
{
    private static ServiceProvider CreateHost(Action<PluginManagerOptions>? configure = null)
    {
        ServiceCollection services = new();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>
        {
            ["Plugins:Settings:sample.sensor:Label"] = "Configured by test"
        }).Build();
        services.AddModularKitPlugins(configuration, options =>
        {
            options.Assemblies = [typeof(SensorPlugin).Assembly.Location];
            options.BaseTypes.Add(typeof(Device));
            options.TypeAliases.Add("Legacy.Sensor", typeof(TcpSensor));
            configure?.Invoke(options);
        });
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Loads_assembly_configures_services_and_creates_registered_implementation()
    {
        using var host = CreateHost();
        Assert.Equal("Configured by test", host.GetRequiredService<ExamplePluginInfo>().Label);
        var plugins = host.GetRequiredService<IPluginManager>();
        var option = Assert.Single(plugins.GetTypeOptions(typeof(Device)));
        Assert.Equal("sample.tcp-sensor", option.Discriminator);
        Assert.Equal("TCP sensor", option.DisplayName);
        Assert.IsType<TcpSensor>(plugins.CreateInstance(typeof(Device), option.Discriminator));
    }

    [Fact]
    public void Reads_legacy_identifier_but_writes_only_stable_identifier()
    {
        using var host = CreateHost();
        var json = new SerializationOptionsProvider(host.GetRequiredService<IPluginManager>().BaseJsonSerializerOptions!).Config;
        Device device = JsonSerializer.Deserialize<Device>("""{"$type":"Legacy.Sensor","Name":"Sensor","Host":"test","Port":42}""", json)!;
        Assert.Equal("test", Assert.IsType<TcpSensor>(device).Host);
        string saved = JsonSerializer.Serialize(device, json);
        Assert.Contains("sample.tcp-sensor", saved);
        Assert.DoesNotContain("Legacy.Sensor", saved);
    }

    [Fact]
    public void Refuses_unknown_identifiers_instead_of_resolving_arbitrary_types()
    {
        using var host = CreateHost();
        var json = host.GetRequiredService<IPluginManager>().BaseJsonSerializerOptions!;
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Device>("""{"$type":"System.IO.FileInfo"}""", json));
    }

    [Fact]
    public void Missing_assembly_stops_startup()
    {
        Assert.Throws<FileNotFoundException>(() => CreateHost(options => options.Assemblies = [Path.Combine(Path.GetTempPath(), Guid.NewGuid()+".dll")]));
    }

    [Fact]
    public void Conflicting_alias_stops_startup()
    {
        Assert.Throws<InvalidOperationException>(() => CreateHost(options =>
        {
            options.KnownAssemblies.Add(typeof(PluginTests).Assembly);
            options.TypeAliases.Add("sample.tcp-sensor", typeof(OtherDevice));
        }));
    }

    [Fact]
    public void Plain_plugin_contract_works_without_configuration_serialization()
    {
        ServiceCollection services = new();
        services.AddModularKitPlugins(new ConfigurationBuilder().Build(), options => options.BaseTypes.Add(typeof(Message)));
        using var provider = services.BuildServiceProvider();
        var json = provider.GetRequiredService<IPluginManager>().BaseJsonSerializerOptions!;
        Message message = new TextMessage { Text = "hello" };
        string saved = JsonSerializer.Serialize(message, json);
        Assert.Equal("hello", Assert.IsType<TextMessage>(JsonSerializer.Deserialize<Message>(saved, json)).Text);
    }

    [Fact]
    public void Duplicate_plugin_identifiers_stop_startup()
    {
        ServiceCollection services = new();
        Assert.Throws<InvalidOperationException>(() => services.AddModularKitPlugins(
            new ConfigurationBuilder().Build(), options => options.Assemblies = [typeof(DuplicatePluginA).Assembly.Location]));
    }

    public abstract class Message { }
    [PluginTypeId("test.text")]
    public sealed class TextMessage : Message { public string Text { get; set; } = ""; }
    public sealed class DuplicatePluginA : IPlugin
    {
        public string Id => "duplicate";
        public string Name => "A";
    }
    public sealed class DuplicatePluginB : IPlugin
    {
        public string Id => "duplicate";
        public string Name => "B";
    }

    public sealed class OtherDevice : Device { }

    [Fact]
    public void Instance_creation_uses_the_callers_dependency_injection_scope()
    {
        ServiceCollection services = new();
        services.AddScoped<ScopedDependency>();
        services.AddModularKitPlugins(new ConfigurationBuilder().Build(), options => options.BaseTypes.Add(typeof(Widget)));
        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();
        var plugins = scope.ServiceProvider.GetRequiredService<IPluginManager>();
        var widget = Assert.IsType<InjectedWidget>(plugins.CreateInstance(typeof(Widget), typeof(InjectedWidget).FullName!));
        Assert.Same(scope.ServiceProvider.GetRequiredService<ScopedDependency>(), widget.Dependency);
    }

    public sealed class ScopedDependency { }
    public abstract class Widget { }
    public sealed class InjectedWidget(ScopedDependency dependency) : Widget
    {
        public ScopedDependency Dependency { get; } = dependency;
    }
}
