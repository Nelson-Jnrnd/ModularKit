namespace ModularKit.Configuration.View;

/// <summary>Provides the editor's available implementations, independently of how they are discovered.</summary>
public interface IConfigTypeCatalog
{
    IReadOnlyList<ConfigTypeOption> GetTypeOptions(Type baseType);
    object CreateInstance(Type baseType, string discriminator);
}

/// <summary>A stable selection key and its user-facing label.</summary>
public sealed record ConfigTypeOption(string Discriminator, string DisplayName);
