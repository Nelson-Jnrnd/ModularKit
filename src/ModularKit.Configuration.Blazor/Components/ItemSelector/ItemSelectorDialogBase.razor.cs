using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ItemSelector;

public partial class ItemSelectorDialogBase<T>
{
    [CascadingParameter, EditorRequired]
    public required IMudDialogInstance MudDialog { get; set; }

    [Parameter, EditorRequired]
    public required IEnumerable<T> Items { get; set; }

    [Parameter, EditorRequired]
    public required T? SelectedItem { get; set; }

    [Parameter, EditorRequired]
    public required string Title { get; set; }

    [Parameter]
    public bool CanSelectNull { get; set; } = false;

    /// <summary>
    /// When set, a search field is shown and the item grid is filtered on the text this returns for
    /// each item. Left null (the default), no search field is rendered and every item stays visible.
    /// </summary>
    [Parameter]
    public Func<T, string>? SearchTextSelector { get; set; }

    [Parameter]
    public RenderFragment<T>? ItemHeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<T>? ItemContentTemplate { get; set; }

    private string _search = string.Empty;

    private IEnumerable<T> FilteredItems
    {
        get
        {
            if (SearchTextSelector is null || string.IsNullOrWhiteSpace(_search))
                return Items;

            return Items.Where(item =>
                item is not null
                && SearchTextSelector(item).Contains(_search, StringComparison.CurrentCultureIgnoreCase));
        }
    }

    private bool ConfirmButtonDisabled => SelectedItem is null && !CanSelectNull;

    private void Confirm() => MudDialog.Close(DialogResult.Ok(SelectedItem));
    private void Cancel() => MudDialog.Cancel();
    private void OnItemCardClick(T item)
    {
        if (item == null) return;
        if (item.Equals(SelectedItem))
        {
            SelectedItem = default;
        }
        else
        {
            SelectedItem = item;
        }
    }
}

