using Microsoft.AspNetCore.Components;

namespace ModularKit.Configuration.Blazor.Components.ItemSelector;

public partial class ItemCard<T>
{
    [Parameter, EditorRequired]
    public required T Item { get; set; }

    [Parameter]
    public bool IsSelected { get; set; }

    [Parameter]
    public RenderFragment<T>? HeaderTemplate { get; set; }

    [Parameter]
    public RenderFragment<T>? ContentTemplate { get; set; }

    private string CardStyle => IsSelected
        ? "border: 2px solid var(--mud-palette-primary); background-color: var(--mud-palette-primary-hover); box-shadow: 0 0 6px var(--mud-palette-primary);"
        : "border: 2px solid var(--mud-palette-lines-default); background-color: transparent;";

}

