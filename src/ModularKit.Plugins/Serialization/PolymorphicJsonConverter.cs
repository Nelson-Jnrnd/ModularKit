using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModularKit.Plugins.Serialization;
/// <summary>
/// Provides JSON serialization and deserialization for polymorphic object hierarchies using a type discriminator
/// property. Enables correct mapping between base and derived types during JSON conversion.
/// </summary>
/// <typeparam name="TBase">The base class type for polymorphic serialization and deserialization. Must be a reference type.</typeparam>
internal sealed class PolymorphicJsonConverter<TBase> : JsonConverter<TBase> where TBase : class
{
    private readonly PolymorphicTypeMapping _mapping;
    private readonly string _typePropertyName = "$type";

    public PolymorphicJsonConverter(PolymorphicTypeMapping mapping)
    {
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }

    public override TBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Attendu un objet pour le type {_mapping.BaseType.Name}.");
        }

        using JsonDocument doc = JsonDocument.ParseValue(ref reader);
        if (!doc.RootElement.TryGetProperty(_typePropertyName, out JsonElement typeProp))
        {
            throw new JsonException($"Propriété '{_typePropertyName}' manquante pour le type polymorphe {_mapping.BaseType.Name}.");
        }

        string? discriminator = typeProp.GetString();
        if (discriminator == null || !_mapping.DiscriminatorToType.TryGetValue(discriminator, out Type? concreteType))
        {
            throw new JsonException($"Discriminateur '{discriminator}' inconnu pour le type polymorphe {_mapping.BaseType.Name}.");
        }

        string json = doc.RootElement.GetRawText();
        TBase? result = (TBase?)JsonSerializer.Deserialize(json, concreteType, options);
        return result;
    }

    public override void Write(Utf8JsonWriter writer, TBase value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        Type runtimeType = value.GetType();

        if (!_mapping.TypeToDiscriminator.TryGetValue(runtimeType, out string? discriminator))
        {
            throw new JsonException(
                $"Type concret '{runtimeType.FullName}' non enregistré dans le mapping polymorphe pour '{_mapping.BaseType.FullName}'.");
        }

        writer.WriteStartObject();

        writer.WriteString(_typePropertyName, discriminator);

        string json = JsonSerializer.Serialize(value, runtimeType, options);
        using JsonDocument doc = JsonDocument.Parse(json);
        foreach (JsonProperty prop in doc.RootElement.EnumerateObject())
        {
            if (prop.NameEquals(_typePropertyName))
                continue;

            prop.WriteTo(writer);
        }

        writer.WriteEndObject();
    }
}

