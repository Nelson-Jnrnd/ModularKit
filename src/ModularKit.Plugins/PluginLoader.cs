using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularKit.Plugins.Serialization;

namespace ModularKit.Plugins;

internal sealed class PluginLoader(PluginManagerOptions options, ILogger<PluginLoader> logger)
{
    private readonly List<Assembly> _assemblies = [];
    private readonly List<IPlugin> _plugins = [];

    internal void LoadPlugins(IEnumerable<string> paths)
    {
        HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
        foreach (string path in paths)
        {
            string fullPath = Path.GetFullPath(path, options.BaseDirectory);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Plugin assembly does not exist.", fullPath);
            Assembly assembly = Assembly.LoadFrom(fullPath);
            if (_assemblies.Contains(assembly)) continue;
            _assemblies.Add(assembly);
            foreach (Type type in assembly.GetTypes().Where(t => typeof(IPlugin).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters))
            {
                IPlugin plugin = (IPlugin)Activator.CreateInstance(type)!;
                if (string.IsNullOrWhiteSpace(plugin.Id) || !ids.Add(plugin.Id))
                    throw new InvalidOperationException($"Invalid or duplicate plugin identifier '{plugin.Id}'.");
                _plugins.Add(plugin);
                logger.LogInformation("Loaded plugin {PluginId} from {AssemblyPath}.", plugin.Id, fullPath);
            }
        }
    }

    internal void ConfigurePlugins(IServiceCollection services, IConfigurationSection settings)
    {
        foreach (IConfigurablePlugin plugin in _plugins.OfType<IConfigurablePlugin>())
            plugin.Configure(services, settings.GetSection(plugin.Id));
    }

    internal IReadOnlyDictionary<Type, PolymorphicTypeMapping> CreateMappings()
    {
        Assembly[] assemblies = [.. _assemblies.Concat(options.KnownAssemblies)
            .Concat(options.BaseTypes.Select(t => t.Assembly)).Distinct()];
        return PolymorphicTypeRegistryBuilder.Build(options.BaseTypes, assemblies, options.TypeAliases);
    }
}
