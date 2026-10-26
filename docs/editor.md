# Blazor configuration editor

`ModularKit.Configuration.Blazor` provides tree navigation, form descriptors, scalar fields, component cards, selection dialogs and reference-selection workflows. It targets .NET 10 with MudBlazor 9. The supplied sample uses interactive server rendering.

See the [UI tour with screenshots](blazor-ui.md) for the generated form, plugin picker, reference selector and validation feedback.

## Register services

```csharp
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddMudServices();
builder.Services.AddModularKitConfigurationEditor();
builder.Services.AddScoped<IConfigTypeCatalog, YourTypeCatalog>();
```

The host must include MudBlazor CSS and JavaScript and render its theme, popover, dialog and snackbar providers. See the sample's [App.razor](../samples/DeviceWorkbench/Components/App.razor) and [Home.razor](../samples/DeviceWorkbench/Components/Pages/Home.razor).

`IConfigTypeCatalog` belongs to Configuration. Implement it with explicit registrations, or adapt `IPluginManager` as the sample does. The editor does not reference the Plugins package. `IScalarOptionsProvider` implementations are optional and registered through DI as `IEnumerable<IScalarOptionsProvider>`.

## Render the tree and panel

```razor
@using ModularKit.Configuration.Blazor.Components.ConfigTree
@using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes

<ConfigurationTree Root="rootNode" @bind-SelectedItem="selectedNode" />
<ConfigurationPanelContainer SelectedItem="selectedNode"
    OnConfigChanged="HandleConfigChanged" />
```

Create `rootNode` with `new EntityConfigElement(model)` after wiring the model. In `HandleConfigChanged`, rewire from the root, refresh the root node with `RefreshFromDomain()`, and preserve selection by finding the edited entity again. Save explicitly or through a host-owned persistence policy. Tree nodes are view objects and must not be serialized.

For a single object, use `<DynamicConfigForm T="YourConfigType" ObjectToEdit="model" OnConfigChanged="HandleConfigChanged" />`.

## Control edits

Cascade a `ConfigEditGate` when the host restricts editing:

```razor
<CascadingValue Value="editGate">
    <ConfigurationPanelContainer SelectedItem="selectedNode"
        OnConfigChanged="HandleConfigChanged" />
</CascadingValue>
```

The gate takes `Func<bool> isReadOnly` and `Func<Task<bool>> beginEdit`. Controls use both display state and a check before mutation. The default is unrestricted. This is a UI integration point, not a replacement for authorization of server operations or a multi-user concurrency lock.

## Theme and icons

The editor uses the host application's `MudThemeProvider`. The [sample theme](../samples/DeviceWorkbench/Theme/WorkbenchTheme.cs) defines a blue and slate palette, typography and corner radius. Its [stylesheet](../samples/DeviceWorkbench/wwwroot/workbench.css) adds the responsive page layout and component spacing.

Assign semantic icons with `ConfigTreeIcon` from `ModularKit.Configuration.View`:

```csharp
[ConfigTreeIcon("Sensors")]
public abstract class Device : ConfigEntity { }
```

The key is the name of a MudBlazor Material Filled icon. Concrete implementations inherit the icon from their base class. Put an icon attribute on a collection property to override its item type's icon in the navigation tree; the sample uses `DevicesOther` for its device collection and `Hub` for the workbench root. Reference properties can use their own icon, such as `Link`.

## What is generated

- Scalar fields: strings, int, long, float, double, bool, enums, and supported nullable forms.
- DataAnnotations, uniqueness groups, password display and provider-backed choices.
- Owned components and optional/required references.
- Selection dialogs for existing objects and abstract implementations.
- Automatic naming and completion of required component slots before attachment.
- Connection tests for objects implementing `IConnectionTestable`, with a UI timeout.

Collection editing uses mutable lists. Unsupported scalar types such as arbitrary dictionaries or custom structs need a custom UI; this version does not expose a general field-renderer registry. Field descriptors are cached by CLR type; metadata is expected to be stable during the process lifetime.

## Language

Editor resources include English and French. Labels can use DataAnnotations resource classes. Host components that render in response to background events can derive from `CircuitCultureComponentBase` to dispatch updates with their circuit's language. The host still configures request localization and its supported cultures.

## Host responsibilities

The editor mutates the supplied objects directly. The host owns save/cancel behaviour, validation of the complete model, concurrency, authorization and recovery if an edit fails. Use a draft copy if cancel must discard a whole editing session. Do not expose a singleton mutable configuration to several editors without coordinating access.
