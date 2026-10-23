namespace ModularKit.Plugins.Serialization;

/// <summary>
/// Provides a factory for creating instances of polymorphic types based on a discriminator value, using a registry
/// of type mappings.
/// </summary>
/// <param name="registry">A read-only dictionary that maps base types to their correslineing polymorphic type mappings. Used to resolve
/// available subtypes and instantiate objects based on discriminators.</param>
internal sealed class PolymorphicInstanceFactory(IReadOnlyDictionary<Type, PolymorphicTypeMapping> registry, Func<Type, object> activator)
{
    internal IReadOnlyList<KeyValuePair<string, Type>> GetTypeDiscriminators(Type baseType)
    {
        if (!registry.TryGetValue(baseType, out PolymorphicTypeMapping? mapping))
        {
            return [];
        }

        List<KeyValuePair<string, Type>> options = [.. mapping.DiscriminatorToType];
        return options;
    }

    internal IReadOnlyList<string> GetDiscriminators<TBase>() where TBase : class
    {
        Type baseType = typeof(TBase);

        if (!registry.TryGetValue(baseType, out PolymorphicTypeMapping? mapping))
        {
            return [];
        }

        List<string> options = [.. mapping.DiscriminatorToType.Keys];
        return options;
    }

    internal object CreateInstance(Type baseType, string discriminator)
    {
        if (!registry.TryGetValue(baseType, out PolymorphicTypeMapping? mapping))
        {
            throw new ArgumentException("base type not found");
        }
        if (mapping is null || !mapping.DiscriminatorToType.TryGetValue(discriminator, out Type? subtype))
        {
            throw new ArgumentException($"{discriminator} does not exist.");
        }
        if (subtype is null)
        {
            throw new ArgumentException("subtype could not be resolved");
        }
        object? instance = activator(subtype);
        if (instance is null)
        {
            throw new ArgumentException("Instance could not be created");
        }
        else
        {
            return instance;
        }
    }
}

