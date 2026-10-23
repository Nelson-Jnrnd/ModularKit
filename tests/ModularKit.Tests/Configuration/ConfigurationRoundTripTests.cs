using System.Text.Json;
using DeviceWorkbench.Contracts;
using DeviceWorkbench.Plugin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularKit.Configuration.Serialization;
using ModularKit.Plugins;
using Xunit;

namespace ModularKit.Tests.Configuration;

public class ConfigurationRoundTripTests
{
    [Fact]
    public void Rename_and_round_trip_preserve_plugin_type_and_references()
    {
        ServiceCollection services = new();
        services.AddModularKitPlugins(new ConfigurationBuilder().Build(), options =>
        {
            options.BaseTypes.Add(typeof(Device));
            options.KnownAssemblies.Add(typeof(TcpSensor).Assembly);
        });
        using var provider = services.BuildServiceProvider();
        var json = new SerializationOptionsProvider(provider.GetRequiredService<IPluginManager>().BaseJsonSerializerOptions!);
        TcpSensor sensor = new() { Name="Sensor", Host="test", Port=123 };
        Workbench root = new() { Name="Bench", Devices=[sensor] };
        root.WireUp(null);
        root.SelectedDevice.Value = sensor;
        root.Name="Renamed bench";
        sensor.Name="Renamed sensor";
        root.WireUp(null);
        root.PrepareConfigReferencesForPersistence();
        Assert.Equal("Renamed bench/Renamed sensor",root.SelectedDevice.Path);
        string saved = JsonSerializer.Serialize(root,json.Config);
        Workbench loaded = JsonSerializer.Deserialize<Workbench>(saved,json.Config)!;
        loaded.WireUp(null);
        loaded.PrepareConfigReferencesForPersistence();
        Assert.Same(Assert.Single(loaded.Devices),loaded.SelectedDevice.Value);
        Assert.Equal("test",Assert.IsType<TcpSensor>(loaded.SelectedDevice.Value).Host);
    }

    [Fact]
    public void Configuration_and_state_topics_select_different_fields()
    {
        var options = new SerializationOptionsProvider(new JsonSerializerOptions());
        var model = new SplitModel { Endpoint="localhost", Position=12 };
        string config = JsonSerializer.Serialize(model,options.Config);
        string state = JsonSerializer.Serialize(model,options.State);
        Assert.Contains("Endpoint",config);
        Assert.DoesNotContain("Position",config);
        Assert.Contains("Position",state);
        Assert.DoesNotContain("Endpoint",state);
    }

    [SerializationTopic(SerializationTopics.Config)]
    [SerializationTopic(SerializationTopics.State)]
    public sealed class SplitModel
    {
        [SerializationTopic(SerializationTopics.Config)] public string Endpoint { get; set; } = "";
        [SerializationTopic(SerializationTopics.State)] public int Position { get; set; }
    }
}
