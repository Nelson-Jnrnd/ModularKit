using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree;

public partial class ConfigurationTree
{
    [Parameter, EditorRequired]
    public required ConfigElement Root { get; set; }

    [Parameter]
    public ConfigElement? SelectedItem { get; set; }

    [Parameter]
    public EventCallback<ConfigElement?> SelectedItemChanged { get; set; }

    private ConfigElement? SelectedRelay
    {
        get => SelectedItem;
        set => SelectedItemChanged.InvokeAsync(value);
    }

    private IReadOnlyCollection<TreeItemData<ConfigElement?>> TreeItems { get; set; } = [];

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        TreeItems = [
            new ConfigElementNode(Root)
            {
                Expanded = true,
            }
        ];
    }

    private sealed class ConfigElementNode : TreeItemData<ConfigElement?>
    {
        public ConfigElementNode(ConfigElement config)
        {
            Value = config;
            Text = config.Text;
            Icon = config.Icon;
            if (config.Children is { } children)
            {
                List<TreeItemData<ConfigElement?>> built = new();
                foreach (ConfigElement child in children)
                {
                    built.Add(new ConfigElementNode(child));
                }
                Children = built;
            }
        }
    }
}

