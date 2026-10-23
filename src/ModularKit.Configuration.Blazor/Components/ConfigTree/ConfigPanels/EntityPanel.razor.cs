using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigPanels;

public partial class EntityPanel : ConfigPanelBase<EntityConfigElement>
{
    [Parameter]
    public EventCallback<ConfigEntity> OnOpenRequested { get; set; }

    protected ConfigEntity Model => ConfigElement.Data;
}

