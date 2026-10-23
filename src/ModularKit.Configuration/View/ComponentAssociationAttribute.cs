namespace ModularKit.Configuration.View;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ComponentAssociationAttribute(string displayName, AssociationType associationType) : Attribute
{
    public string DisplayName { get; } = displayName;
    public AssociationType AssociationType { get; } = associationType;
}

public enum AssociationType
{
    Aggregation,
    Composition
}

