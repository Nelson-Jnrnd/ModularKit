using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree;
using ModularKit.Configuration.Blazor.Resources;
using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;

public partial class AddComponentCard
{
    [Parameter]
    public ConfigEntity? ObjectToEdit { get; set; }

    [Parameter, EditorRequired]
    public required string DisplayName { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnDeleteRequested { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnAddRequested { get; set; }

    [Parameter]
    public EventCallback OnSelectExistingRequested { get; set; }

    [Parameter]
    public bool CanDelete { get; set; } = true;

    [Parameter]
    public bool CanReplace { get; set; }

    [Parameter]
    public EventCallback OnReplaceRequested { get; set; }

    [Parameter]
    public string ReplaceButtonLabel { get; set; } = CoreRazorText.T("Replace");

    [Parameter]
    public string ReplaceButtonIcon { get; set; } = string.Empty;

    [Parameter]
    public EventCallback OnConfigChanged { get; set; }
    [Parameter]
    public int IndentLevel { get; set; } = 0;
    [Parameter]
    public Type DeclaredType { get; set; } = typeof(ConfigEntity);

    [Parameter]
    public string? Icon { get; set; }

    [Parameter]
    public bool IsInline { get; set; }

    [Parameter]
    public EventCallback<ConfigEntity> OnOpenRequested { get; set; }

    [CascadingParameter]
    private ConfigEditGate Gate { get; set; } = ConfigEditGate.Unrestricted;

    private bool IsReadOnly => Gate.IsReadOnly;

    private Task DeleteObjectAsync() => OnDeleteRequested.InvokeAsync();

    private Task ReplaceObjectAsync() => OnReplaceRequested.InvokeAsync();

    private Task OpenObjectSelectorAsync() => OnAddRequested.InvokeAsync();

    private Task OpenExistingObjectSelectorAsync() => OnSelectExistingRequested.InvokeAsync();

    private Task OpenCardAsync()
    {
        if (ObjectToEdit is null || !OnOpenRequested.HasDelegate)
            return Task.CompletedTask;

        return OnOpenRequested.InvokeAsync(ObjectToEdit);
    }

    private string GetSummaryCardStyle() => OnOpenRequested.HasDelegate ? "cursor: pointer;" : string.Empty;

    private string GetDeclaredTypeName() => ConfigUiDisplayNames.GetTypeDisplayName(DeclaredType);

    private string GetVisibleTypeName()
    {
        return ConfigUiDisplayNames.GetVisibleTypeDisplayName(DeclaredType, ObjectToEdit?.GetType());
    }

    private string? GetTypeTooltip()
    {
        return ConfigUiDisplayNames.GetTypeDetailTooltip(DeclaredType, ObjectToEdit?.GetType());
    }

    private string GetSelectedConfigurationName()
    {
        return string.IsNullOrWhiteSpace(ObjectToEdit?.Name)
            ? CoreRazorText.T("Untitled")
            : ObjectToEdit.Name;
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

