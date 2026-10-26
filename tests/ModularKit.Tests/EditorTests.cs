using Bunit;
using DeviceWorkbench.Contracts;
using DeviceWorkbench.Plugin;
using Microsoft.Extensions.DependencyInjection;
using ModularKit.Configuration;
using ModularKit.Configuration.Blazor;
using ModularKit.Configuration.Blazor.Components.ConfigTree;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;
using ModularKit.Configuration.View;
using MudBlazor.Services;
using Xunit;

namespace ModularKit.Tests;

public class EditorTests
{
    [Fact]
    public void Discovers_scalar_fields_and_reference_slots_without_domain_types()
    {
        var sensor = ConfigFormDescriptorFactory.GetOrCreate(typeof(TcpSensor));
        Assert.Contains(sensor.ScalarFields, field => field.Property.Name=="Host");
        Assert.Contains(sensor.ScalarFields, field => field.Property.Name=="Port");
        var workbench = ConfigFormDescriptorFactory.GetOrCreate(typeof(Workbench));
        Assert.Contains(workbench.ComponentSlots, slot => slot.Property.Name=="SelectedDevice" && slot.SupportsSelectExisting);
    }

    [Fact]
    public async Task Edit_gate_can_reject_mutations()
    {
        ConfigEditGate gate = new(() => true, () => Task.FromResult(false));
        Assert.True(gate.IsReadOnly);
        Assert.False(await gate.BeginEditAsync());
    }

    [Fact]
    public async Task Renders_editable_sensor_fields()
    {
        await using var context = new BunitContext();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        context.Services.AddMudServices();
        context.Services.AddModularKitConfigurationEditor();
        context.Services.AddSingleton<IConfigTypeCatalog, EmptyCatalog>();
        var form = context.Render<DynamicConfigForm<TcpSensor>>(p => p.Add(c => c.ObjectToEdit,new TcpSensor { Name="Sensor",Host="localhost" }));
        Assert.Contains("Host",form.Markup);
        Assert.Contains("Port",form.Markup);
        Assert.Contains("localhost",form.Markup);
        form.FindAll("input").Single(input => input.GetAttribute("value") == "localhost").Change("device.example");
        form.WaitForAssertion(() => Assert.Equal("device.example", form.Instance.ObjectToEdit.Host));
    }

    [Fact]
    public void Configuration_tree_preserves_the_object_to_select()
    {
        var sensor = new TcpSensor { Name="Sensor" };
        var model = new Workbench { Name="Bench", Devices=[sensor] };
        model.WireUp(null);
        var tree = new EntityConfigElement(model);
        Assert.Same(sensor,tree.FindInChildren(sensor)!.Data);
    }

    [Fact]
    public void Collection_icon_overrides_item_icon_and_subtypes_inherit_device_icon()
    {
        var sensor = new TcpSensor { Name = "Sensor" };
        var model = new Workbench { Name = "Workbench", Devices = [sensor] };
        model.WireUp(null);
        var tree = new EntityConfigElement(model);
        Assert.Equal(MudBlazor.Icons.Material.Filled.Hub, tree.Icon);
        Assert.Equal(MudBlazor.Icons.Material.Filled.DevicesOther, Assert.Single(tree.Children).Icon);
        Assert.Equal(MudBlazor.Icons.Material.Filled.Sensors, tree.FindInChildren(sensor)!.Icon);
    }

    private sealed class EmptyCatalog : IConfigTypeCatalog
    {
        public IReadOnlyList<ConfigTypeOption> GetTypeOptions(Type baseType) => [];
        public object CreateInstance(Type baseType,string discriminator) => throw new NotSupportedException();
    }

    [Fact]
    public void Both_deletion_routes_reject_a_referenced_device()
    {
        var sensor = new TcpSensor { Name = "Sensor" };
        var model = new Workbench { Name = "Bench", Devices = [sensor] };
        model.WireUp(null);
        model.SelectedDevice.Value = sensor;
        var tree = new EntityConfigElement(model);
        var collection = Assert.IsType<CollectionConfigElement>(Assert.Single(tree.Children));
        Assert.NotEmpty(ConfigDeletionGuard.GetExternalReferences(sensor));
        Assert.Throws<InvalidOperationException>(() => collection.RemoveChildElement(collection.Children[0]));
        Assert.Single(model.Devices);
        model.SelectedDevice.Value = null;
        collection.RemoveChildElement(collection.Children[0]);
        Assert.Empty(model.Devices);
    }
}
