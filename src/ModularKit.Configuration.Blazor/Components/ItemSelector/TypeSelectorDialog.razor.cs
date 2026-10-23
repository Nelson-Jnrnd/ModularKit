using ModularKit.Configuration.View;
using ModularKit.Configuration.Blazor.Resources;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ItemSelector;

public partial class TypeSelectorDialog : ComponentBase
{
    [Inject]
    public IConfigTypeCatalog PluginManager { get; set; } = default!;

    [Parameter, EditorRequired]
    public required ConfigTypeOption? SelectedType { get; set; }

    [Parameter, EditorRequired]
    public required Type BaseType { get; set; }

    [Parameter, EditorRequired]
    public required string Title { get; set; }

    private IReadOnlyList<ConfigTypeOption> Options => PluginManager.GetTypeOptions(BaseType);

    public static async Task<object?> OpenAsync(
    IDialogService dialogService,
    IConfigTypeCatalog pluginManager,
    Type baseType,
    string title,
    ConfigTypeOption? initialSelectedType = null)
    {
        ArgumentNullException.ThrowIfNull(dialogService);

        var parameters = new DialogParameters
        {
            ["Title"] = title,
            ["SelectedType"] = initialSelectedType,
            ["BaseType"] = baseType,
        };

        IDialogReference dialog = await dialogService.ShowAsync<TypeSelectorDialog>(
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
            if (dialogResult.Data is not ConfigTypeOption typeOption)
            {
                throw new InvalidOperationException(CoreRazorText.T("Dialog returned invalid data."));
            }

            return pluginManager.CreateInstance(baseType, typeOption.Discriminator);
        }

        return null;
    }
}

