using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ModularKit.Configuration.View;

namespace ModularKit.Configuration.Serialization;

/// <summary>
/// Provides factory methods for creating configured instances of <see cref="JsonSerializerOptions"/> that filter
/// serialization members based on a specified topic attribute.
/// </summary>
/// <remarks>This class is intended for internal use to support topic-based filtering during JSON
/// serialization. The generated <see cref="JsonSerializerOptions"/> will include only types and properties
/// annotated with a matching <see cref="SerializationTopicAttribute"/> for the given topic. This enables selective
/// serialization of object graphs according to topic-specific metadata.</remarks>
public static class TopicJsonSerializerOptionsFactory
{
    public static JsonSerializerOptions Create(string topic, JsonSerializerOptions? baseOptions = null)
    {
        DefaultJsonTypeInfoResolver resolver = new()
        {
            Modifiers = { typeInfo => ApplyTopicFilter(typeInfo, topic) }
        };

        baseOptions ??= new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        JsonSerializerOptions jsonOptions = new(baseOptions)
        {
            TypeInfoResolver = resolver
        };

        EnableNullableEnforcement(jsonOptions);

        return jsonOptions;
    }

    private static void ApplyTopicFilter(JsonTypeInfo typeInfo, string topic)
    {
        if (typeInfo.Kind != JsonTypeInfoKind.Object)
            return;

        // Type-level topic: if the type is explicitly marked with a different topic,
        // drop all its properties.
        if (!MatchesTopic(typeInfo.Type, topic))
        {
            typeInfo.Properties.Clear();
            return;
        }

        // Property-level topics: remove properties with a non-matching topic.
        // Enumerate a copy, because we are modifying the collection.
        foreach (JsonPropertyInfo? property in typeInfo.Properties.ToArray())
        {
            ICustomAttributeProvider? attributeProvider = property.AttributeProvider;

            if (!MatchesTopic(attributeProvider, topic))
            {
                typeInfo.Properties.Remove(property);
                continue;
            }

            if (attributeProvider is PropertyInfo propertyInfo
                && propertyInfo.IsRequiredAssociation())
            {
                MarkAsRequired(property);
            }
        }
    }

    private static void EnableNullableEnforcement(JsonSerializerOptions jsonOptions)
    {
        jsonOptions.RespectNullableAnnotations = true;
    }

    private static void MarkAsRequired(JsonPropertyInfo property)
    {
        property.IsRequired = true;
    }

    private static bool MatchesTopic(MemberInfo member, string topic)
    {
        IEnumerable<SerializationTopicAttribute> attrs = member.GetCustomAttributes<SerializationTopicAttribute>(inherit: true);
        return attrs.Any(attr => AttributeMatchesTopic(attr, topic));
    }

    private static bool MatchesTopic(ICustomAttributeProvider? attributeProvider, string topic)
    {
        if (attributeProvider is null)
            return false; // No attribute: don't keep the member

        IEnumerable<SerializationTopicAttribute> attrs = attributeProvider
            .GetCustomAttributes(typeof(SerializationTopicAttribute), inherit: true)
            .OfType<SerializationTopicAttribute>();

        return attrs.Any(attr => AttributeMatchesTopic(attr, topic));
    }

    private static bool AttributeMatchesTopic(SerializationTopicAttribute? attr, string topic)
    {
        if (attr is null)
            return false;

        return string.Equals(attr.Topic, topic, StringComparison.OrdinalIgnoreCase);
    }
}

