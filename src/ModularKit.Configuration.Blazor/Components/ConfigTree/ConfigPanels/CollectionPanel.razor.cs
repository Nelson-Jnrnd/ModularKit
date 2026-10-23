using ModularKit.Configuration.View;
using ModularKit.Configuration.References;
using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using ModularKit.Configuration.Blazor.Resources;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigPanels;

public partial class CollectionPanel : ConfigPanelBase<CollectionConfigElement>
{
    [Parameter]
    public EventCallback<ConfigElement> OnItemAdded { get; set; }

    [CascadingParameter]
    private ConfigEditGate Gate { get; set; } = ConfigEditGate.Unrestricted;

    [Inject]
    private IConfigEntityFactory Factory { get; set; } = default!;
    [Inject]
    private IDialogService DialogService { get; set; } = default!;

    protected IList<ConfigEntity> Items => ConfigElement.Items.ToList();

    protected async Task AddItem()
    {
        if (!await Gate.BeginEditAsync())
            return;

        ConfigEntity? item = await Factory.CreateForAttachmentAsync(
            ConfigElement.ItemType,
            ConfigElement.Owner,
            ConfigElement.Text);

        if (item is null)
            return;

        int newIndex = ConfigElement.Items.Count();
        ConfigElement.AddItem(item);
        await NotifyConfigChangedAsync();

        if (OnItemAdded.HasDelegate && newIndex < ConfigElement.Children.Length)
            await OnItemAdded.InvokeAsync(ConfigElement.Children[newIndex]);
    }

    protected async Task RemoveItemsAsync(IReadOnlyList<ConfigEntity> items)
    {
        if (!await Gate.BeginEditAsync())
            return;

        foreach (ConfigEntity item in items)
        {
            await RemoveItemAsync(item);
        }
    }

    private async Task RemoveItemAsync(ConfigEntity item)
    {
        IReadOnlyList<ConfigReferenceEntry> incomingReferences = ConfigDeletionGuard.GetExternalReferences(item);
        if (incomingReferences.Count > 0)
        {
            MarkupString message = BuildDeleteBlockedMessage(item, incomingReferences);
            await DialogService.ShowMessageBoxAsync(CoreRazorText.T("Delete blocked"), message, CoreRazorText.T("OK"));
            return;
        }

        ConfigElement.RemoveItem(item);
        await NotifyConfigChangedAsync();
    }

    private static MarkupString BuildDeleteBlockedMessage(ConfigEntity item, IReadOnlyList<ConfigReferenceEntry> incomingReferences)
    {
        string title = System.Net.WebUtility.HtmlEncode(item.Parent is null ? item.Name : item.Path);
        IEnumerable<string> rows = incomingReferences
            .OrderBy(r => r.OwnerPath, StringComparer.Ordinal)
            .ThenBy(r => r.PropertyName, StringComparer.Ordinal)
            .Select(r => $"<li><strong>{System.Net.WebUtility.HtmlEncode(r.OwnerPath)}</strong><br /><span>{CoreRazorText.T("Property")}: {System.Net.WebUtility.HtmlEncode(r.PropertyName)}</span></li>");

        string html =
            CoreRazorText.T("Deleting <strong>{0}</strong> is blocked because it is still referenced by other configuration items.", title) +
            $"<div style='margin-top:8px'>{CoreRazorText.T("Remove or update these references first:")}</div>" +
            $"<ul style='margin:8px 0 0 0;padding-left:20px'>{string.Join("", rows)}</ul>";

        return new MarkupString(html);
    }
}

