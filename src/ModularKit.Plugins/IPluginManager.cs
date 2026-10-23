using System.Text.Json;

namespace ModularKit.Plugins;

/// <summary>
/// Defines methods and properties for managing plugins, including initialization, type discovery, and instantiation
/// based on type discriminators.
/// </summary>
public interface IPluginManager
{
    public JsonSerializerOptions? BaseJsonSerializerOptions { get; }
    public void Initialize();
    IReadOnlyList<PluginTypeOption> GetTypeOptions(Type baseType);

    object CreateInstance(Type baseType, string discriminator);
}

