using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ModularKit.Plugins.Serialization;

namespace ModularKit.Plugins;

internal sealed class PluginManager : IPluginManager
{
    private readonly IReadOnlyDictionary<Type, PolymorphicTypeMapping> _mappings;
    private readonly PolymorphicInstanceFactory _factory;

    public JsonSerializerOptions BaseJsonSerializerOptions { get; }

    internal PluginManager(IReadOnlyDictionary<Type, PolymorphicTypeMapping> mappings, IServiceProvider services)
    {
        _mappings = mappings;
        _factory = new(mappings, type => ActivatorUtilities.CreateInstance(services, type));
        BaseJsonSerializerOptions = new() { WriteIndented = true };
        BaseJsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        foreach (PolymorphicTypeMapping mapping in mappings.Values)
        {
            Type converterType = typeof(PolymorphicJsonConverter<>).MakeGenericType(mapping.BaseType);
            BaseJsonSerializerOptions.Converters.Add((JsonConverter)Activator.CreateInstance(converterType, mapping)!);
        }
    }

    /// <summary>The registry is initialized at registration time; this method is idempotent for migrated callers.</summary>
    public void Initialize() { }

    public IReadOnlyList<PluginTypeOption> GetTypeOptions(Type baseType) =>
        !_mappings.TryGetValue(baseType, out var mapping) ? [] :
        [.. mapping.TypeToDiscriminator.Select(pair => new PluginTypeOption(pair.Value,
            pair.Key.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? pair.Key.Name))];

    public object CreateInstance(Type baseType, string discriminator) => _factory.CreateInstance(baseType, discriminator);
}
