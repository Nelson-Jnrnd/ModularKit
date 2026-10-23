using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree;
using ModularKit.Configuration.Blazor.Resources;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ItemSelector;

public partial class GenericItemSelectorDialog<T>
{
    [Parameter, EditorRequired]
    public required IEnumerable<T> Items { get; set; }

    [Parameter, EditorRequired]
    public required T? SelectedItem { get; set; }

    [Parameter, EditorRequired]
    public required string Title { get; set; }

    private static bool IsConfigEntity(T item) => item is ConfigEntity;

    private static string GetHeaderText(T item)
    {
        if (item is ConfigEntity entity)
            return string.IsNullOrWhiteSpace(entity.Name) ? CoreRazorText.T("Untitled") : entity.Name;

        return item?.ToString() ?? string.Empty;
    }

    private static string? GetSubtypeText(T item)
    {
        if (item is not ConfigEntity entity)
            return null;

        return ConfigUiDisplayNames.GetTypeDisplayName(entity.GetType());
    }

    private static string? GetPathText(T item)
    {
        return item is ConfigEntity entity ? entity.Path : null;
    }

    public static async Task<T?> OpenAsync(
       IDialogService dialogService,
       string title,
       IEnumerable<T> items,
       T? initialSelectedItem)
    {
        ArgumentNullException.ThrowIfNull(dialogService);

        var parameters = new DialogParameters
        {
            ["Title"] = title,
            ["Items"] = items,
            ["SelectedItem"] = initialSelectedItem
        };

        IDialogReference dialog = await dialogService.ShowAsync<GenericItemSelectorDialog<T>>(
            title,
            parameters,
            new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
            });

        DialogResult? dialogResult = await dialog.Result;

        if (dialogResult?.Canceled is false)
        {
            if (dialogResult.Data is not T item)
            {
                throw new InvalidOperationException(CoreRazorText.T("Dialog returned invalid data."));
            }

            return item;
        }

        return default!;
    }
}

