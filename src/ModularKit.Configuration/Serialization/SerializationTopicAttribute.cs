namespace ModularKit.Configuration.Serialization;

/// <summary>
/// Specifies the serialization topic associated with a property for custom serialization scenarios.
/// </summary>
/// <remarks>Apply this attribute to a property to indicate its logical grouping or topic during
/// serialization. This can be used by serializers to organize or filter properties based on their assigned topic.
/// Only one topic can be assigned per property, and the attribute is inherited by derived classes.</remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = true)]
public sealed class SerializationTopicAttribute : Attribute
{
    public string Topic { get; }
    public SerializationTopicAttribute(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or whitespace.", nameof(topic));

        Topic = topic;
    }
}

