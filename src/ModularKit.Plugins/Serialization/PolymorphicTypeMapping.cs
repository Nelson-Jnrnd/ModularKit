namespace ModularKit.Plugins.Serialization;

/// <summary>
/// Represents a mapping between discriminator values and derived types for polymorphic type resolution.
/// </summary>
/// <param name="baseType">The base type for which polymorphic mappings are defined. Must be a non-null type that serves as the root of the
/// polymorphic hierarchy.</param>
internal sealed class PolymorphicTypeMapping(Type baseType)
{
    public Type BaseType { get; } = baseType;
    public Dictionary<string, Type> DiscriminatorToType { get; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<Type, string> TypeToDiscriminator { get; } = [];

    public void AddMapping(string discriminator, Type type)
    {
        if (DiscriminatorToType.TryGetValue(discriminator, out Type? existing) && existing != type)
            throw new InvalidOperationException($"Duplicate type identifier '{discriminator}' for '{existing}' and '{type}'.");
        DiscriminatorToType[discriminator] = type;
        TypeToDiscriminator[type] = discriminator;
    }
}

