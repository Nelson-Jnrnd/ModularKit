using ModularKit.Configuration.View;
using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;
using ModularKit.Configuration.Blazor.Components.ItemSelector;
using MudBlazor;

namespace ModularKit.Configuration.Blazor.Services.ItemSelector;

public sealed class DefaultConfigEntityFactory(
    IDialogService dialogService,
    IConfigTypeCatalog pluginManager) : IConfigEntityFactory
{
    public async Task<ConfigEntity?> CreateAsync(Type type, ConfigEntity? owner = null, string? displayName = null)
    {
        if (!type.IsAbstract)
            return (ConfigEntity)Activator.CreateInstance(type)!;

        string title = owner is not null && !string.IsNullOrWhiteSpace(displayName)
            ? ConfigSelectorTitleBuilder.Build(owner, displayName, type)
            : $"Select {ConfigSelectorTitleBuilder.ResolveTypeDisplayName(type)}";

        object? instance = await TypeSelectorDialog.OpenAsync(
            dialogService,
            pluginManager,
            type,
            title,
            initialSelectedType: null);

        return instance as ConfigEntity;
    }

    public Task<ConfigEntity?> SelectExistingAsync(
        ConfigEntity owner,
        string displayName,
        Type componentType,
        IEnumerable<ConfigEntity> candidates)
    {
        string title = ConfigSelectorTitleBuilder.Build(owner, displayName, componentType);
        return GenericItemSelectorDialog<ConfigEntity>.OpenAsync(
            dialogService,
            title,
            candidates,
            null);
    }

    public async Task<ConfigEntity?> CreateForAttachmentAsync(Type type, ConfigEntity owner, string? displayName = null)
    {
        ConfigEntity? component = await CreateAsync(type, owner, displayName);
        if (component is null)
            return null;

        if (!await CompleteRequiredSlotsAsync(component, owner))
            return null;

        AssignGeneratedName(component, owner);

        component.WireUp(owner);
        return component;
    }

    /// <summary>
    /// Names a new entity after the type it instantiates, before it is attached. Naming at creation
    /// keeps sibling names — and therefore Paths, which key config references — distinct. The type
    /// name is resolved in the invariant culture, so the identity key does not depend on the culture
    /// of the machine that created the entity.
    /// </summary>
    private static void AssignGeneratedName(ConfigEntity component, ConfigEntity owner)
    {
        component.AssignGeneratedNameUnder(owner, InvariantTypeDisplayName.ForType(component.GetType()));
    }

    private async Task<bool> CompleteRequiredSlotsAsync(ConfigEntity entity, ConfigEntity attachmentOwner)
    {
        ConfigFormDescriptor descriptor = ConfigFormDescriptorFactory.GetOrCreate(entity.GetType());

        foreach (ConfigComponentSlotDescriptor slot in descriptor.ComponentSlots.Where(slot => slot.IsRequired && slot.Kind is not ConfigComponentSlotKind.ReferenceList))
        {
            ConfigEntity? currentValue = GetSlotValue(entity, slot);
            if (currentValue is not null)
            {
                if (slot.AssociationType is AssociationType.Composition && slot.Kind is ConfigComponentSlotKind.Component)
                {
                    if (!await CompleteRequiredSlotsAsync(currentValue, attachmentOwner))
                        return false;
                }

                continue;
            }

            ConfigEntity? selectedValue = await CreateOrSelectRequiredValueAsync(entity, slot, attachmentOwner);
            if (selectedValue is null)
                return false;

            SetSlotValue(entity, slot, selectedValue);
        }

        return true;
    }

    private async Task<ConfigEntity?> CreateOrSelectRequiredValueAsync(
        ConfigEntity currentOwner,
        ConfigComponentSlotDescriptor slot,
        ConfigEntity attachmentOwner)
    {
        if (slot.SupportsSelectExisting)
        {
            IEnumerable<ConfigEntity> candidates = attachmentOwner.Root.AllDescendants
                .Where(candidate => slot.ComponentType.IsInstanceOfType(candidate));

            return await SelectExistingAsync(currentOwner, slot.DisplayName, slot.ComponentType, candidates);
        }

        ConfigEntity? created = await CreateAsync(slot.ComponentType, currentOwner, slot.DisplayName);
        if (created is null)
            return null;

        AssignGeneratedName(created, currentOwner);

        return await CompleteRequiredSlotsAsync(created, attachmentOwner)
            ? created
            : null;
    }

    private static ConfigEntity? GetSlotValue(ConfigEntity owner, ConfigComponentSlotDescriptor slot)
    {
        object? value = slot.Property.GetValue(owner);
        return value switch
        {
            Configuration.References.IConfigRef configRef => configRef.Value,
            ConfigEntity entity => entity,
            _ => null,
        };
    }

    private static void SetSlotValue(ConfigEntity owner, ConfigComponentSlotDescriptor slot, ConfigEntity value)
    {
        if (slot.Property.GetValue(owner) is Configuration.References.IConfigRef configRef)
        {
            configRef.Value = value;
            return;
        }

        slot.Property.SetValue(owner, value);
    }
}

