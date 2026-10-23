using System.Reflection;

namespace ModularKit.Plugins;

/// <summary>Host-owned extension points, assembly locations and persisted type aliases.</summary>
public sealed class PluginManagerOptions
{
    public string[] Assemblies { get; set; } = [];
    public string SettingsSectionPath { get; set; } = "Plugins:Settings";
    public string BaseDirectory { get; set; } = AppContext.BaseDirectory;
    public IList<Type> BaseTypes { get; } = new List<Type>();
    public IList<Assembly> KnownAssemblies { get; } = new List<Assembly>();

    /// <summary>Additional read-only JSON discriminator aliases, for old namespaces or renamed types.</summary>
    public IDictionary<string, Type> TypeAliases { get; } = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
}
