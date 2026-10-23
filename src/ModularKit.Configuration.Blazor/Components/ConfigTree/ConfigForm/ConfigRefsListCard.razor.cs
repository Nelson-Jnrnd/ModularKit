using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree;
using ModularKit.Configuration.Blazor.Resources;
using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;

public partial class ConfigRefsListCard
{
    [Parameter, EditorRequired]
    public required string DisplayName { get; set; }

    [Parameter, EditorRequired]
    public required IReadOnlyList<ConfigEntity> Items { get; set; }

    [Parameter, EditorRequired]
    public required IReadOnlyList<ConfigEntity> Candidates { get; set; }

    [Parameter, EditorRequired]
    public required EventCallback<IReadOnlyList<ConfigEntity>> OnItemsChanged { get; set; }

    [Parameter]
    public int IndentLevel { get; set; }

    [Parameter]
    public Type ComponentType { get; set; } = typeof(ConfigEntity);

    [CascadingParameter]
    private ConfigEditGate Gate { get; set; } = ConfigEditGate.Unrestricted;

    private bool IsReadOnly => Gate.IsReadOnly;

    private HashSet<ConfigEntity> _selectedItems = [];

    protected override void OnParametersSet()
    {
        _selectedItems = Items.ToHashSet();
    }

    private async Task OnSelectionChangedAsync(IEnumerable<ConfigEntity> items)
    {
        _selectedItems = items.ToHashSet();
        await OnItemsChanged.InvokeAsync(_selectedItems.ToList());
    }

    private Task ClearSelectionAsync()
    {
        _selectedItems.Clear();
        return OnItemsChanged.InvokeAsync([]);
    }

    private string GetVisibleTypeName()
    {
        Type? runtimeType = Items.Count == 1 ? Items[0].GetType() : null;
        return ConfigUiDisplayNames.GetVisibleTypeDisplayName(ComponentType, runtimeType);
    }

    private string? GetTypeTooltip()
    {
        Type? runtimeType = Items.Count == 1 ? Items[0].GetType() : null;
        return ConfigUiDisplayNames.GetTypeDetailTooltip(ComponentType, runtimeType);
    }

    private string? GetSelectedConfigurationsSummary()
    {
        if (Items.Count == 0)
            return null;

        string[] names = Items
            .Select(item => string.IsNullOrWhiteSpace(item.Name) ? CoreRazorText.T("Untitled") : item.Name)
            .Take(2)
            .ToArray();

        return Items.Count <= 2
            ? string.Join(", ", names)
            : CoreRazorText.T("{0} +{1} more", string.Join(", ", names), Items.Count - 2);
    }

    private string GetNodeStyle()
    {
        int lvl = Math.Clamp(IndentLevel, 0, 6);

        int pct = lvl switch
        {
            0 => 90,
            1 => 60,
            2 => 30,
            3 => 15,
            4 => 5,
            _ => 5,
        };

        return $"--cfg-primary-pct: {pct}%;";
    }
}

