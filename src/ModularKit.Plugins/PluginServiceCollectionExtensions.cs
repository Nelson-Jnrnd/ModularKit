using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ModularKit.Plugins;

public static class PluginServiceCollectionExtensions
{
    /// <summary>
    /// Loads trusted plugins before building the service provider. Invalid paths, duplicate IDs,
    /// type collisions and configuration failures abort registration rather than starting a partial host.
    /// </summary>
    public static IServiceCollection AddModularKitPlugins(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<PluginManagerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(configure);
        PluginManagerOptions options = new();
        configure(options);
        PluginLoader loader = new(options, NullLogger<PluginLoader>.Instance);
        loader.LoadPlugins(options.Assemblies);
        loader.ConfigurePlugins(services, configuration.GetSection(options.SettingsSectionPath));
        var mappings = loader.CreateMappings();
        // Resolve constructor dependencies from the scope that asks for the manager, not always
        // from the root container. The discovered mappings are shared and are not rescanned.
        services.AddTransient<IPluginManager>(provider => new PluginManager(mappings, provider));
        return services;
    }
}
