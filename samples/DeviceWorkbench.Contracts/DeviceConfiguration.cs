using System.ComponentModel.DataAnnotations;
using ModularKit.Configuration;
using ModularKit.Configuration.References;
using ModularKit.Configuration.Serialization;
using ModularKit.Configuration.View;

namespace DeviceWorkbench.Contracts;

[SerializationTopic(SerializationTopics.Config)]
[Display(Name = "Workbench")]
public sealed class Workbench : ConfigEntity
{
    [SerializationTopic(SerializationTopics.Config)]
    [ComponentAssociation("Devices", AssociationType.Composition)]
    public List<Device> Devices { get; set; } = [];

    [SerializationTopic(SerializationTopics.Config)]
    [ComponentAssociation("Selected device", AssociationType.Aggregation)]
    public ConfigRef<Device> SelectedDevice { get; set; } = new();
}

[SerializationTopic(SerializationTopics.Config)]
public abstract class Device : ConfigEntity { }

public sealed record ExamplePluginInfo(string Label);
