using ModularKit.Configuration.View;
using MudBlazor;
using System.Collections.Concurrent;
using System.Reflection;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;

public static class ConfigTreeIconResolver
{
    private static readonly ConcurrentDictionary<Type, string> Cache = new();
    private static readonly ConcurrentDictionary<Type, string?> NullableCache = new();

    /// <summary>
    /// Resolves the MudBlazor icon string for a domain entity type by reading its
    /// <see cref="ConfigTreeIconAttribute"/>. Walks the inheritance chain so subtypes
    /// inherit the parent's icon (e.g., DigitalTwinClient inherits ControlSystem's icon).
    /// Falls back to <see cref="Icons.Material.Filled.Help"/> if no matching icon is found.
    /// </summary>
    public static string Resolve(Type entityType)
    {
        return Cache.GetOrAdd(entityType, static type =>
            ResolveOrNull(type) ?? Icons.Material.Filled.Help);
    }

    /// <summary>
    /// Like <see cref="Resolve"/> but returns <see langword="null"/> when no
    /// <see cref="ConfigTreeIconAttribute"/> is present anywhere in the inheritance chain,
    /// rather than falling back to the Help icon.
    /// </summary>
    public static string? ResolveOrNull(Type entityType)
    {
        return NullableCache.GetOrAdd(entityType, static type =>
        {
            Type? current = type;
            while (current is not null && current != typeof(object))
            {
                ConfigTreeIconAttribute? attr = current.GetCustomAttribute<ConfigTreeIconAttribute>(inherit: false);
                if (attr is not null)
                {
                    string? icon = typeof(Icons.Material.Filled)
                        .GetField(attr.IconKey, BindingFlags.Public | BindingFlags.Static)
                        ?.GetValue(null) as string;
                    if (icon is not null)
                        return icon;
                }
                current = current.BaseType;
            }
            return null;
        });
    }

    /// <summary>
    /// Resolves the icon for a component-slot property using a two-step lookup:
    /// <list type="number">
    ///   <item>A <see cref="ConfigTreeIconAttribute"/> placed directly on <paramref name="property"/>.</item>
    ///   <item><see cref="ResolveOrNull"/> on <paramref name="componentType"/> (class-level attribute, walks inheritance).</item>
    /// </list>
    /// Returns <see langword="null"/> when neither source provides an icon.
    /// </summary>
    public static string? ResolveFromProperty(PropertyInfo property, Type componentType)
    {
        ConfigTreeIconAttribute? propertyAttr = property.GetCustomAttribute<ConfigTreeIconAttribute>();
        if (propertyAttr is not null)
        {
            string? icon = typeof(Icons.Material.Filled)
                .GetField(propertyAttr.IconKey, BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null) as string;
            if (icon is not null)
                return icon;
        }

        return ResolveOrNull(componentType);
    }
}

