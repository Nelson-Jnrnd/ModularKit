using System.ComponentModel.DataAnnotations;
using DeviceWorkbench.Contracts;
using ModularKit.Configuration;
using ModularKit.Configuration.Serialization;
using ModularKit.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeviceWorkbench.Plugin;

[PluginTypeId("sample.tcp-sensor")]
[Display(Name = "TCP sensor")]
[SerializationTopic(SerializationTopics.Config)]
public sealed class TcpSensor : Device
{
    [SerializationTopic(SerializationTopics.Config)]
    [Required]
    [Unique(Group = "Connection")]
    public string Host { get; set; } = "localhost";

    [SerializationTopic(SerializationTopics.Config)]
    [Range(1, 65535)]
    [Unique(Group = "Connection")]
    public int Port { get; set; } = 502;
}

public sealed class SensorPlugin : IConfigurablePlugin
{
    public string Id => "sample.sensor";
    public string Name => "Example sensor plugin";
    public void Configure(IServiceCollection services, IConfiguration configuration) =>
        services.AddSingleton(new ExamplePluginInfo(configuration["Label"] ?? "Example sensor"));
}
