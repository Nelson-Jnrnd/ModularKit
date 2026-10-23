using System.Reflection;

namespace ModularKit.Plugins.Serialization;

internal static class PolymorphicTypeRegistryBuilder
{
    internal static IReadOnlyDictionary<Type, PolymorphicTypeMapping> Build(
        IEnumerable<Type> baseTypes,
        IEnumerable<Assembly> assemblies,
        IDictionary<string, Type> aliases)
    {
        Dictionary<Type, PolymorphicTypeMapping> result = [];
        Type[] candidates = [.. assemblies.Distinct().SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters).Distinct()];
        foreach (Type baseType in baseTypes.Distinct())
        {
            if ((!baseType.IsAbstract && !baseType.IsInterface) || baseType.ContainsGenericParameters)
                throw new ArgumentException($"Extension point '{baseType}' must be an abstract class or interface without open generic parameters.");
            PolymorphicTypeMapping mapping = new(baseType);
            foreach (Type type in candidates.Where(baseType.IsAssignableFrom))
            {
                string id = type.GetCustomAttribute<PluginTypeIdAttribute>()?.Id ?? type.FullName!;
                mapping.AddMapping(id, type);
            }
            foreach ((string alias, Type type) in aliases.Where(pair => baseType.IsAssignableFrom(pair.Value)))
            {
                if (string.IsNullOrWhiteSpace(alias) || !mapping.TypeToDiscriminator.ContainsKey(type))
                    throw new ArgumentException($"Alias '{alias}' points to a type that is not registered for '{baseType}'.");
                if (mapping.DiscriminatorToType.TryGetValue(alias, out Type? existing) && existing != type)
                    throw new InvalidOperationException($"Duplicate type identifier '{alias}'.");
                mapping.DiscriminatorToType[alias] = type;
            }
            result.Add(baseType, mapping);
        }
        foreach ((string alias, Type type) in aliases)
            if (!result.Values.Any(m => m.TypeToDiscriminator.ContainsKey(type)))
                throw new ArgumentException($"Alias '{alias}' targets an undiscovered type '{type}'.");
        return result;
    }
}
