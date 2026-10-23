using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ModularKit.Plugins;

/// <summary>
/// Defines the contract for a plugin
/// </summary>
public interface IPlugin
{
    string Id { get; }
    string Name { get; }
}

public interface IConfigurablePlugin : IPlugin
{
    void Configure(IServiceCollection services, IConfiguration configuration);
}
