using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using Xunit;

namespace ModularKit.Tests.Configuration;

public sealed class ConfigurationPanelSelectionGuardTests
{
    [Fact]
    public void MarkPendingDeletion_hides_the_selected_item_from_rendering()
    {
        SelectedConfigRenderGuard guard = new();
        EntityConfigElement selectedItem = new(new TestConfigEntity { Name = "Selected" });

        guard.MarkPendingDeletion(selectedItem);

        Assert.Null(guard.GetEffectiveSelectedItem(selectedItem));
    }

    [Fact]
    public void CancelPendingDeletion_restores_the_selected_item()
    {
        SelectedConfigRenderGuard guard = new();
        EntityConfigElement selectedItem = new(new TestConfigEntity { Name = "Selected" });

        guard.MarkPendingDeletion(selectedItem);
        guard.CancelPendingDeletion();

        Assert.Same(selectedItem, guard.GetEffectiveSelectedItem(selectedItem));
    }

    [Fact]
    public void OnParametersUpdated_with_null_selection_clears_pending_deletion()
    {
        SelectedConfigRenderGuard guard = new();
        EntityConfigElement selectedItem = new(new TestConfigEntity { Name = "Selected" });

        guard.MarkPendingDeletion(selectedItem);
        guard.OnParametersUpdated(null);

        Assert.Same(selectedItem, guard.GetEffectiveSelectedItem(selectedItem));
    }

    [Fact]
    public void OnParametersUpdated_with_new_selection_clears_pending_deletion()
    {
        SelectedConfigRenderGuard guard = new();
        EntityConfigElement selectedItem = new(new TestConfigEntity { Name = "Selected" });
        EntityConfigElement replacementItem = new(new TestConfigEntity { Name = "Replacement" });

        guard.MarkPendingDeletion(selectedItem);
        guard.OnParametersUpdated(replacementItem);

        Assert.Same(replacementItem, guard.GetEffectiveSelectedItem(replacementItem));
        Assert.Same(selectedItem, guard.GetEffectiveSelectedItem(selectedItem));
    }

    private sealed class TestConfigEntity : ConfigEntity
    {
    }
}

