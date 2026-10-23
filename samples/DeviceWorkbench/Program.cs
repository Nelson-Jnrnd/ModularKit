using DeviceWorkbench.Contracts;
using DeviceWorkbench.Components;
using DeviceWorkbench.Services;
using ModularKit.Configuration.Blazor;
using ModularKit.Configuration.Serialization;
using ModularKit.Configuration.View;
using ModularKit.Persistence.Json;
using ModularKit.Plugins;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddModularKitPlugins(builder.Configuration, options =>
{
    options.Assemblies = ["plugins/DeviceWorkbench.Plugin.dll"];
    options.BaseTypes.Add(typeof(Device));
});
builder.Services.AddModularKitConfigurationEditor();
builder.Services.AddScoped<IConfigTypeCatalog, PluginConfigTypeCatalog>();
builder.Services.AddSingleton<IJsonFileStore, JsonFileStore>();
builder.Services.AddSingleton<ISerializationOptionsProvider>(services =>
    new SerializationOptionsProvider(services.GetRequiredService<IPluginManager>().BaseJsonSerializerOptions!));
var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();
