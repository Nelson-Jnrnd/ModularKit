using ModularKit.Configuration.View;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ItemSelector;

/// <summary>
/// Presents a scalar property's options as selectable cards, the same way plugin types are chosen,
/// instead of a drop-down list.
/// </summary>
public partial class ScalarOptionSelectorDialog
{
    [Parameter, EditorRequired]
    public required IEnumerable<ScalarOption> Options { get; set; }

    [Parameter, EditorRequired]
    public required ScalarOption SelectedOption { get; set; }

    [Parameter, EditorRequired]
    public required string Title { get; set; }

    private static Color BadgeColor(ScalarOptionBadgeKind kind) => kind switch
    {
        ScalarOptionBadgeKind.Info => Color.Info,
        ScalarOptionBadgeKind.Success => Color.Success,
        ScalarOptionBadgeKind.Warning => Color.Warning,
        _ => Color.Default,
    };

    /// <summary>
    /// Opens the selector and returns the chosen option. Returns <c>null</c> when the operator cancels
    /// (the current value must be left untouched). A confirmed selection with no card picked yields an
    /// option whose value is empty, which clears the field.
    /// </summary>
    public static async Task<ScalarOption?> OpenAsync(
        IDialogService dialogService,
        string title,
        IEnumerable<ScalarOption> options,
        string? currentValue)
    {
        ArgumentNullException.ThrowIfNull(dialogService);

        IReadOnlyList<ScalarOption> materialized = options as IReadOnlyList<ScalarOption> ?? [.. options];
        ScalarOption selected = materialized
            .FirstOrDefault(option => string.Equals(option.Value, currentValue, StringComparison.Ordinal));

        var parameters = new DialogParameters
        {
            ["Title"] = title,
            ["Options"] = materialized,
            ["SelectedOption"] = selected,
        };

        IDialogReference dialog = await dialogService.ShowAsync<ScalarOptionSelectorDialog>(
            title,
            parameters,
            new DialogOptions
            {
                MaxWidth = MaxWidth.Large,
                FullWidth = true,
            });

        DialogResult? dialogResult = await dialog.Result;

        if (dialogResult?.Canceled is not false)
            return null;

        return dialogResult.Data is ScalarOption option ? option : default(ScalarOption);
    }
}

