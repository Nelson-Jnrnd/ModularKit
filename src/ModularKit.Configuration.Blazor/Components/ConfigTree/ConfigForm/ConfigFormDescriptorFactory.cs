using ModularKit.Configuration.References;
using ModularKit.Configuration.View;
using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using ModularKit.Configuration.Serialization;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;

internal static class ConfigFormDescriptorFactory
{
    private static readonly ConcurrentDictionary<Type, ConfigFormDescriptor> _descriptorCache = new();
    private static readonly NullabilityInfoContext _nullabilityContext = new();

    internal static ConfigFormDescriptor GetOrCreate(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        return _descriptorCache.GetOrAdd(entityType, CreateDescriptor);
    }

    private static ConfigFormDescriptor CreateDescriptor(Type entityType)
    {
        List<ConfigScalarFieldDescriptor> scalarFields = [];
        List<ConfigComponentSlotDescriptor> componentSlots = [];

        foreach (PropertyInfo property in entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (property.GetIndexParameters().Length != 0)
                continue;

            if (TryCreateScalarFieldDescriptor(property, out ConfigScalarFieldDescriptor scalarField))
                scalarFields.Add(scalarField);

            if (TryCreateComponentSlotDescriptor(property, out ConfigComponentSlotDescriptor componentSlot))
                componentSlots.Add(componentSlot);
        }

        return new ConfigFormDescriptor([.. scalarFields], [.. componentSlots]);
    }

    private static bool TryCreateScalarFieldDescriptor(PropertyInfo property, out ConfigScalarFieldDescriptor descriptor)
    {
        descriptor = null!;

        if (!property.CanRead ||
            property.SetMethod is not { IsPublic: true } ||
            property.GetCustomAttribute<ConfigFormHiddenAttribute>() is not null ||
            !HasConfigTopic(property))
        {
            return false;
        }

        Type underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (underlyingType.IsEnum)
        {
            descriptor = new ConfigScalarFieldDescriptor(
                property,
                GetDisplayName(property),
                underlyingType,
                TypeCode.Object);

            return true;
        }

        TypeCode typeCode = Type.GetTypeCode(underlyingType);
        if (typeCode is not (TypeCode.String or TypeCode.Int32 or TypeCode.Int64
            or TypeCode.Double or TypeCode.Single or TypeCode.Boolean))
            return false;

        (int smColumns, int mdColumns) = GetScalarFieldGridColumns(property);

        descriptor = new ConfigScalarFieldDescriptor(
            property,
            GetDisplayName(property),
            underlyingType,
            typeCode,
            smColumns,
            mdColumns,
            property.GetCustomAttribute<ScalarOptionsAttribute>()?.ProviderType);

        return true;
    }

    private static bool TryCreateComponentSlotDescriptor(PropertyInfo property, out ConfigComponentSlotDescriptor descriptor)
    {
        descriptor = null!;

        if (property.GetCustomAttribute<ConfigFormHiddenAttribute>() is not null)
            return false;

        ComponentAssociationAttribute? attribute = property.GetCustomAttribute<ComponentAssociationAttribute>();
        if (attribute is null)
            return false;

        if (typeof(IConfigRef).IsAssignableFrom(property.PropertyType))
        {
            Type componentType = GetComponentTypeFromRefType(property.PropertyType);
            descriptor = new ConfigComponentSlotDescriptor(
                property,
                ConfigUiDisplayNames.GetPropertyDisplayName(property),
                ConfigComponentSlotKind.SingleReference,
                componentType,
                attribute.AssociationType,
                typeof(IRequiredConfigRef).IsAssignableFrom(property.PropertyType),
                IsInline(property),
                ConfigTreeIconResolver.ResolveFromProperty(property, componentType));
            return true;
        }

        if (TryGetReferenceListComponentType(property.PropertyType, out Type? listComponentType))
        {
            descriptor = new ConfigComponentSlotDescriptor(
                property,
                ConfigUiDisplayNames.GetPropertyDisplayName(property),
                ConfigComponentSlotKind.ReferenceList,
                listComponentType!,
                attribute.AssociationType,
                false,
                IsInline(property),
                ConfigTreeIconResolver.ResolveFromProperty(property, listComponentType!));
            return true;
        }

        descriptor = new ConfigComponentSlotDescriptor(
            property,
            ConfigUiDisplayNames.GetPropertyDisplayName(property),
            ConfigComponentSlotKind.Component,
            property.PropertyType,
            attribute.AssociationType,
            IsRequiredComponent(property),
            IsInline(property),
            ConfigTreeIconResolver.ResolveFromProperty(property, property.PropertyType));
        return true;
    }

    private static bool HasConfigTopic(PropertyInfo property)
    {
        return property.GetCustomAttributes<SerializationTopicAttribute>()
            .Any(attribute => attribute.Topic.Equals(SerializationTopics.Config, StringComparison.Ordinal));
    }

    private static string GetDisplayName(PropertyInfo property)
    {
        DisplayAttribute? displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.GetName() ?? property.Name;
    }

    private static (int SmColumns, int MdColumns) GetScalarFieldGridColumns(PropertyInfo property)
    {
        DataTypeAttribute? dataType = property.GetCustomAttribute<DataTypeAttribute>();
        return dataType?.DataType == DataType.MultilineText
            ? (12, 12)
            : (6, 4);
    }

    private static bool IsInline(PropertyInfo property) =>
        property.GetCustomAttribute<InlineAttribute>() is not null;

    private static bool TryGetReferenceListComponentType(Type propertyType, out Type? componentType)
    {
        componentType = null;

        if (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(List<>))
            return false;

        Type itemType = propertyType.GetGenericArguments()[0];
        if (!typeof(IConfigRef).IsAssignableFrom(itemType))
            return false;

        componentType = GetComponentTypeFromRefType(itemType);
        return true;
    }

    private static Type GetComponentTypeFromRefType(Type refType)
    {
        Type? configRefType = refType.GetInterfaces()
            .Prepend(refType)
            .FirstOrDefault(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IConfigRef<>));

        if (configRefType is null)
            throw new InvalidOperationException($"Unable to determine referenced component type for '{refType.FullName}'.");

        return configRefType.GetGenericArguments()[0];
    }

    private static bool IsRequiredComponent(PropertyInfo property)
    {
        NullabilityInfo nullability = _nullabilityContext.Create(property);
        return nullability.ReadState is NullabilityState.NotNull
            || nullability.WriteState is NullabilityState.NotNull;
    }
}

