using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Components.Server.Circuits;
using ModularKit.Configuration.View;
using ModularKit.Configuration.Blazor.Infrastructure;
using ModularKit.Configuration.Blazor.Services.ItemSelector;

namespace ModularKit.Configuration.Blazor;

public static class ConfigurationBlazorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the editor services. The host also registers MudBlazor services and an
    /// IConfigTypeCatalog (which can use plugins or an explicit list of implementations).
    /// </summary>
    public static IServiceCollection AddModularKitConfigurationEditor(this IServiceCollection services)
    {
        services.AddLocalization();
        services.TryAddScoped<IConfigEntityFactory, DefaultConfigEntityFactory>();
        services.TryAddScoped<CircuitCulture>();
        services.TryAddEnumerable(ServiceDescriptor.Scoped<CircuitHandler, CircuitCultureHandler>());
        return services;
    }
}
