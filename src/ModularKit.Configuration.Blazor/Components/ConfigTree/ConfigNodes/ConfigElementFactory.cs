using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree;
using ModularKit.Configuration.Serialization;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;

/// <summary>
/// Static factory that scans <see cref="ConfigEntity"/> properties and builds
/// child <see cref="ConfigElement"/> arrays for <see cref="EntityConfigElement"/>.
/// Results are cached per type to avoid repeated reflection.
/// </summary>
internal static class ConfigElementFactory
{
    private static readonly ConcurrentDictionary<Type, ChildPropertyDescriptor[]> _propertyCache = new();

    internal static ConfigElement[] BuildChildren(ConfigEntity entity)
    {
        ChildPropertyDescriptor[] descriptors = _propertyCache.GetOrAdd(entity.GetType(), ScanProperties);
        var children = new List<ConfigElement>();

        foreach (ChildPropertyDescriptor descriptor in descriptors)
        {
            object? value = descriptor.Property.GetValue(entity);

            if (descriptor.Kind == ChildKind.Single && value is ConfigEntity child)
            {
                children.Add(new EntityConfigElement(child, descriptor.DeclaredType, descriptor.PropertyDisplayName));
            }
            else if (descriptor.Kind == ChildKind.Collection && value is IList list)
            {
                children.Add(new CollectionConfigElement(
                    entity,
                    list,
                    descriptor.DeclaredType,
                    descriptor.PropertyDisplayName,
                    descriptor.Icon));
            }
        }

        return [.. children];
    }

    private static ChildPropertyDescriptor[] ScanProperties(Type type)
    {
        var descriptors = new List<ChildPropertyDescriptor>();

        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || property.GetIndexParameters().Length != 0)
                continue;

            if (Attribute.IsDefined(property, typeof(JsonIgnoreAttribute)))
                continue;

            bool hasConfigTopic = property.GetCustomAttributes<SerializationTopicAttribute>()
                .Any(attribute => attribute.Topic == SerializationTopics.Config);
            if (!hasConfigTopic)
                continue;

            Type propertyType = property.PropertyType;
            string propertyDisplayName = ConfigUiDisplayNames.GetPropertyDisplayName(property);

            if (TryGetListItemType(propertyType, out Type? listItemType))
            {
                descriptors.Add(new ChildPropertyDescriptor(
                    property,
                    ChildKind.Collection,
                    listItemType,
                    propertyDisplayName,
                    ConfigTreeIconResolver.Resolve(listItemType)));
                continue;
            }

            if (typeof(ConfigEntity).IsAssignableFrom(propertyType))
            {
                descriptors.Add(new ChildPropertyDescriptor(
                    property,
                    ChildKind.Single,
                    propertyType,
                    propertyDisplayName,
                    string.Empty));
            }
        }

        return [.. descriptors];
    }

    private static bool TryGetListItemType(Type type, out Type itemType)
    {
        itemType = null!;
        foreach (Type iface in type.GetInterfaces().Prepend(type))
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IList<>))
            {
                Type candidate = iface.GetGenericArguments()[0];
                if (typeof(ConfigEntity).IsAssignableFrom(candidate))
                {
                    itemType = candidate;
                    return true;
                }
            }
        }

        return false;
    }

    private enum ChildKind
    {
        Single,
        Collection
    }

    private sealed record ChildPropertyDescriptor(
        PropertyInfo Property,
        ChildKind Kind,
        Type DeclaredType,
        string PropertyDisplayName,
        string Icon);
}

