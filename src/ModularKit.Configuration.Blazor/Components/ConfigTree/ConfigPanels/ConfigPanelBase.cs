using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigPanels;

public abstract class ConfigPanelBase<TConfigElement> : ComponentBase
    where TConfigElement : ConfigElement
{
    [Parameter, EditorRequired]
    public required TConfigElement ConfigElement { get; set; }

    [Parameter]
    public EventCallback OnConfigChanged { get; set; }

    protected async Task NotifyConfigChangedAsync()
    {
        ConfigElement.RefreshFromDomain();

        if (OnConfigChanged.HasDelegate)
            await OnConfigChanged.InvokeAsync();
        else
            StateHasChanged();
    }
}

