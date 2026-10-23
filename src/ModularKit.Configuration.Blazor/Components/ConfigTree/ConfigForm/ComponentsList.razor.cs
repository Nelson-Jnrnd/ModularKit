using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree;
using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;

public partial class ComponentsList<T> where T : ConfigEntity
{
    [Parameter, EditorRequired]
    public required IList<T> ObjectsToEdit { get; set; }

    /// <summary>
    /// Callback to create/add a new item to the collection.
    /// Parent is responsible for adding the item.
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback OnAddRequested { get; set; }

    /// <summary>
    /// Callback to delete the currently selected items from the collection.
    /// Parent is responsible for applying deletion rules.
    /// </summary>
    [Parameter, EditorRequired]
    public EventCallback<IReadOnlyList<T>> OnDeleteRequested { get; set; }

    [CascadingParameter]
    private ConfigEditGate Gate { get; set; } = ConfigEditGate.Unrestricted;

    private bool IsReadOnly => Gate.IsReadOnly;

    private IReadOnlyCollection<T> SelectedObjects = [];

    private Task OnAddClicked() => OnAddRequested.InvokeAsync();

    private Task OnDeleteClicked() => OnDeleteRequested.InvokeAsync([.. SelectedObjects]);
}

