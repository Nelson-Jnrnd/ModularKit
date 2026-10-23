using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using ModularKit.Configuration.Blazor.Resources;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree;

public partial class ConfigurationPanelContainer
{
    private readonly SelectedConfigRenderGuard _selectedConfigRenderGuard = new();

    [CascadingParameter]
    private ConfigEditGate Gate { get; set; } = ConfigEditGate.Unrestricted;

    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    [Parameter, EditorRequired]
    public required ConfigElement? SelectedItem { get; set; }

    [Parameter, EditorRequired]
    public EventCallback OnConfigChanged { get; set; }

    [Parameter]
    public EventCallback<ConfigEntity> OnOpenRequested { get; set; }

    [Parameter]
    public EventCallback<ConfigElement> OnItemAdded { get; set; }

    [Parameter]
    public EventCallback OnItemDeleted { get; set; }

    private ConfigElement? EffectiveSelectedItem =>
        _selectedConfigRenderGuard.GetEffectiveSelectedItem(SelectedItem);

    private bool IsReadOnly => Gate.IsReadOnly;

    private IReadOnlyList<BreadcrumbItem> BreadcrumbItems =>
        EffectiveSelectedItem is null
            ? []
            : [.. GetSelectedItemPath()
                .Select(segment => new BreadcrumbItem(segment, href: null, disabled: true))];

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        _selectedConfigRenderGuard.OnParametersUpdated(SelectedItem);
    }

    private async Task OnDeleteClicked()
    {
        ConfigElement? selectedItem = SelectedItem;
        if (selectedItem is not { IsDeletable: true, Parent: IConfigCollectionParent collectionParent })
            return;

        if (!await Gate.BeginEditAsync())
            return;

        if (selectedItem is EntityConfigElement entity && ConfigDeletionGuard.GetExternalReferences(entity.Data).Count > 0)
        {
            await DialogService.ShowMessageBoxAsync(CoreRazorText.T("Delete blocked"),
                CoreRazorText.T("The item or one of its children is still referenced."), CoreRazorText.T("OK"));
            return;
        }

        bool? confirmed = await DialogService.ShowMessageBoxAsync(
            CoreRazorText.T("Delete item"),
            CoreRazorText.T("Are you sure you want to delete \"{0}\"?", selectedItem.Text),
            yesText: CoreRazorText.T("Delete"),
            cancelText: CoreRazorText.T("Cancel"));

        if (confirmed != true)
        {
            _selectedConfigRenderGuard.CancelPendingDeletion();
            return;
        }

        _selectedConfigRenderGuard.MarkPendingDeletion(selectedItem);
        collectionParent.RemoveChildElement(selectedItem);
        await OnItemDeleted.InvokeAsync();
    }

    private IEnumerable<string> GetSelectedItemPath()
    {
        return EffectiveSelectedItem switch
        {
            EntityConfigElement entity => ConfigUiDisplayNames.GetPathSegments(entity.Data.Path),
            CollectionConfigElement collection => ConfigUiDisplayNames.GetPathSegments($"{collection.Owner.Path}/{collection.Text}"),
            _ => []
        };
    }
}

internal sealed class SelectedConfigRenderGuard
{
    private ConfigElement? _pendingDeletion;

    internal ConfigElement? GetEffectiveSelectedItem(ConfigElement? selectedItem)
    {
        return ReferenceEquals(selectedItem, _pendingDeletion)
            ? null
            : selectedItem;
    }

    internal void MarkPendingDeletion(ConfigElement? selectedItem)
    {
        _pendingDeletion = selectedItem;
    }

    internal void CancelPendingDeletion()
    {
        _pendingDeletion = null;
    }

    internal void OnParametersUpdated(ConfigElement? selectedItem)
    {
        if (!ReferenceEquals(selectedItem, _pendingDeletion))
            _pendingDeletion = null;
    }
}

