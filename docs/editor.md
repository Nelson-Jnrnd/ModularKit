# Blazor configuration editor

`ModularKit.Configuration.Blazor` supplies the tree, generated forms and selection dialogs shown in the [UI tour](blazor-ui.md).

## Register and render

```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddModularKitConfigurationEditor();
builder.Services.AddScoped<IConfigTypeCatalog, YourTypeCatalog>();
```

Include MudBlazor CSS/JavaScript and its theme, popover, dialog and snackbar providers. See the sample's [App.razor](../samples/DeviceWorkbench/Components/App.razor) and [Home.razor](../samples/DeviceWorkbench/Components/Pages/Home.razor).

`IConfigTypeCatalog` provides implementation choices and creation. Use explicit registrations or adapt the plugin manager; the editor has no dependency on Plugins. Register optional `IScalarOptionsProvider` implementations through DI.

```razor
@using ModularKit.Configuration.Blazor.Components.ConfigTree
@using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes

<ConfigurationTree Root="rootNode" @bind-SelectedItem="selectedNode" />
<ConfigurationPanelContainer SelectedItem="selectedNode"
    OnConfigChanged="HandleConfigChanged" />
```

After wiring the model, create `rootNode` with `new EntityConfigElement(model)`. On changes, rewire, call `RefreshFromDomain()` and restore selection by finding the edited entity. Tree nodes are view objects, not persisted configuration.

For one object, use `DynamicConfigForm<T>` with `ObjectToEdit` and `OnConfigChanged`.

## Edit permissions

```razor
<CascadingValue Value="editGate">
    <ConfigurationPanelContainer SelectedItem="selectedNode"
        OnConfigChanged="HandleConfigChanged" />
</CascadingValue>
```

`ConfigEditGate` takes `isReadOnly` and `beginEdit` callbacks; the default permits editing. The host remains responsible for authorization, concurrency and saving. Forms mutate supplied objects directly; use a draft copy for session-wide cancel.

## Theme and icons

Use the host's `MudThemeProvider`. The sample's [theme](../samples/DeviceWorkbench/Theme/WorkbenchTheme.cs) and [stylesheet](../samples/DeviceWorkbench/wwwroot/workbench.css) show palette, typography and responsive layout.

```csharp
[ConfigTreeIcon("Sensors")]
public abstract class Device : ConfigEntity { }
```

`ConfigTreeIcon` uses a MudBlazor Material Filled icon name. Subtypes inherit icons; property attributes override them for collection navigation and component slots. The sample uses `Hub`, `DevicesOther`, `Sensors` and `Link`.

## Supported fields and language

Generated fields support strings, int, long, float, double, bool, enums and supported nullable forms. Collections use mutable lists; custom structs and dictionaries need custom UI. Metadata is cached by CLR type.

Forms apply DataAnnotations, complete required component slots during creation and offer connection tests for `IConnectionTestable` objects.

Editor text is available in English and French. Configure request localization in the host. Components updated by background events can derive from `CircuitCultureComponentBase` to preserve their circuit's language.
