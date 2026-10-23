using System.Text.Json;

namespace ModularKit.Configuration.Serialization;

public sealed class SerializationOptionsProvider : ISerializationOptionsProvider
{
    public JsonSerializerOptions Base { get; }
    public JsonSerializerOptions Config { get; }
    public JsonSerializerOptions State { get; }

    public SerializationOptionsProvider(JsonSerializerOptions baseOptions)
    {
        if (baseOptions is null)
        {
            throw new ArgumentException("Base JSON options must be supplied.", nameof(baseOptions));
        }
        Base = baseOptions;
        Config = TopicJsonSerializerOptionsFactory.Create(SerializationTopics.Config, Base);
        State = TopicJsonSerializerOptionsFactory.Create(SerializationTopics.State, Base);
    }
}

