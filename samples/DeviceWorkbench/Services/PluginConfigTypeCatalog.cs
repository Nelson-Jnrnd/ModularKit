using ModularKit.Configuration.View;
using ModularKit.Plugins;

namespace DeviceWorkbench.Services;

// This integration belongs to the host: neither library needs to reference the other.
public sealed class PluginConfigTypeCatalog(IPluginManager plugins) : IConfigTypeCatalog
{
    public IReadOnlyList<ConfigTypeOption> GetTypeOptions(Type baseType) =>
        [.. plugins.GetTypeOptions(baseType).Select(option => new ConfigTypeOption(option.Discriminator, option.DisplayName))];

    public object CreateInstance(Type baseType, string discriminator) => plugins.CreateInstance(baseType, discriminator);
}
