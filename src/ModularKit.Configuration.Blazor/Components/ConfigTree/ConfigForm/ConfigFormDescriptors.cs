using ModularKit.Configuration.View;
using System.Reflection;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;

internal sealed record ConfigFormDescriptor(
    IReadOnlyList<ConfigScalarFieldDescriptor> ScalarFields,
    IReadOnlyList<ConfigComponentSlotDescriptor> ComponentSlots);

internal sealed record ConfigScalarFieldDescriptor(
    PropertyInfo Property,
    string DisplayName,
    Type UnderlyingType,
    TypeCode Code,
    int SmColumns = 6,
    int MdColumns = 4,
    Type? OptionsProviderType = null);

internal sealed record ConfigComponentSlotDescriptor(
    PropertyInfo Property,
    string DisplayName,
    ConfigComponentSlotKind Kind,
    Type ComponentType,
    AssociationType AssociationType,
    bool IsRequired,
    bool IsInline,
    string? Icon = null)
{
    public bool SupportsSelectExisting => AssociationType is AssociationType.Aggregation;
    public bool CanDelete => !IsRequired;
    public bool CanReplace => IsRequired;
}

internal enum ConfigComponentSlotKind
{
    Component,
    SingleReference,
    ReferenceList
}

