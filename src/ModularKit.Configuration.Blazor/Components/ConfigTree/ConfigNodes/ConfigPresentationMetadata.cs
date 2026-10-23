using ModularKit.Configuration.Blazor.Components.ConfigTree;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;

public sealed record ConfigPresentationMetadata(
    string ConfigurationName,
    string ConfigurationType,
    string? ExactSubtype,
    string? PropertyName)
{
    public bool HasExactSubtype => !string.IsNullOrWhiteSpace(ExactSubtype);
    public bool HasPropertyName => !string.IsNullOrWhiteSpace(PropertyName);

    internal static ConfigPresentationMetadata Create(
        string configurationName,
        Type declaredType,
        Type runtimeType,
        string? propertyName = null)
    {
        return new ConfigPresentationMetadata(
            configurationName,
            ConfigUiDisplayNames.GetTypeDisplayName(declaredType),
            ConfigUiDisplayNames.GetExactSubtypeDisplayName(declaredType, runtimeType),
            propertyName);
    }
}

