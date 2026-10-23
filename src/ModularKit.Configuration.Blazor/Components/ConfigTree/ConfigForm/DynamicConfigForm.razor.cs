using ModularKit.Configuration.References;
using ModularKit.Configuration.View;
using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Resources;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using System.ComponentModel.DataAnnotations;
using System.Collections;
using System.Reflection;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigForm;

public partial class DynamicConfigForm<T> where T : ConfigEntity
{
    [Parameter, EditorRequired]
    public required T ObjectToEdit { get; set; }
    [Parameter]
    public int IndentLevel { get; set; } = 0;
    [Parameter]
    public EventCallback OnConfigChanged { get; set; }
    [Parameter]
    public EventCallback<ConfigEntity> OnOpenRequested { get; set; }
    [Inject]
    private IConfigEntityFactory ComponentSelector { get; set; } = default!;

    [CascadingParameter]
    private ConfigEditGate Gate { get; set; } = ConfigEditGate.Unrestricted;

    private bool IsReadOnly => Gate.IsReadOnly;

    private IReadOnlyList<ConfigScalarFieldDescriptor> _scalarFields = [];
    private IReadOnlyList<ConfigComponentSlotDescriptor> _componentSlots = [];
    private string _nameDraft = string.Empty;
    private bool _isNameEditable;
    private string[] _formErrors = [];
    private MudForm? _form;
    private T? _boundObject;

    private Task NotifyConfigChangedAsync()
    {
        if (OnConfigChanged.HasDelegate)
            return OnConfigChanged.InvokeAsync();

        StateHasChanged();
        return Task.CompletedTask;
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _formErrors = [];
        if (ObjectToEdit is null)
        {
            _nameDraft = string.Empty;
            _isNameEditable = false;
            _scalarFields = [];
            _componentSlots = [];
            _boundObject = null;
            _ = _form?.ResetValidationAsync();
            return;
        }

        _isNameEditable = !AutoNamedAttribute.AppliesTo(ObjectToEdit.GetType());
        // creation and neither shown nor validated here.
        // An auto-named entity is never designated by its name, so the name is generated at

        // Only sync the draft (and reset validation) when the edited entity changes. Doing the reset
        // on every re-render would wipe the errors ValidateAsync just set on the other fields of a
        // composite [Unique] group (e.g. IP + Port), leaving only the field the user last touched red.
        if (!ReferenceEquals(ObjectToEdit, _boundObject))
        {
            _nameDraft = _isNameEditable ? ObjectToEdit.Name ?? string.Empty : string.Empty;
            _boundObject = ObjectToEdit;
            _ = _form?.ResetValidationAsync();
        }

        ConfigFormDescriptor descriptor = ConfigFormDescriptorFactory.GetOrCreate(ObjectToEdit.GetType());
        _scalarFields = descriptor.ScalarFields;
        _componentSlots = descriptor.ComponentSlots;
    }

    private string? ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return CoreRazorText.T("Name is required.");

        // Mirror the ConfigEntity.Name setter's invalid-char guard so the UI shows the error
        // (and keeps the draft) instead of CommitNameAsync silently swallowing the setter throw.
        if (value.IndexOfAny(ConfigEntity.InvalidNameCharacters) >= 0)
            return $"Name must not contain any of these characters: {string.Join(" ", ConfigEntity.InvalidNameCharacters)}";

        // If unchanged, do not flag as duplicate.
        if (string.Equals(value, ObjectToEdit.Name, StringComparison.Ordinal))
            return null;

        // Uniqueness via the [Unique] attribute on ConfigEntity.Name (single source of truth for the
        // message). Name is a singleton [Unique] group; compared candidate-aware against siblings
        // (FindFirstUniquenessViolation reads the committed entity, not this in-progress draft value).
        UniqueAttribute? unique = typeof(ConfigEntity)
            .GetProperty(nameof(ConfigEntity.Name))!
            .GetCustomAttribute<UniqueAttribute>();
        if (unique is not null
            && ObjectToEdit.Siblings.Any(s => string.Equals(s.Name, value, StringComparison.OrdinalIgnoreCase)))
        {
            return unique.Message ?? "This value is already used.";
        }

        return null;
    }

    private async Task CommitNameAsync(string value)
    {
        if (!await Gate.BeginEditAsync())
            return;

        _nameDraft = value;

        if (_form is not null)
            await _form.ValidateAsync();

        if (ValidateName(_nameDraft) is not null)
            return;

        if (string.Equals(ObjectToEdit.Name, _nameDraft, StringComparison.Ordinal))
            return;

        try
        {
            ObjectToEdit.Name = _nameDraft;
        }
        catch (ArgumentException)
        {
            return;
        }

        await NotifyConfigChangedAsync();
    }

    /// <summary>
    /// Builds a per-field validation function combining standard DataAnnotations
    /// ([Required], [RegularExpression], …) with our custom [Unique] uniqueness check.
    /// Special-cases ConfigEntity.Name to also reject invalid characters that would
    /// otherwise throw from the setter.
    /// </summary>
    private Func<object?, string?>? CreateValidationFor(PropertyInfo property)
    {
        ValidationAttribute[] validationAttributes = [.. property.GetCustomAttributes<ValidationAttribute>(inherit: true)];
        UniqueAttribute? unique = property.GetCustomAttribute<UniqueAttribute>();
        bool isEntityName = property.Name == nameof(ConfigEntity.Name)
                            && typeof(ConfigEntity).IsAssignableFrom(property.DeclaringType);

        if (validationAttributes.Length == 0 && unique is null && !isEntityName)
            return null;

        return newValue =>
        {
            // 1. Standard DataAnnotations (Required, RegularExpression, Range, StringLength, …).
            if (validationAttributes.Length > 0)
            {
                ValidationContext context = new(ObjectToEdit!) { MemberName = property.Name };
                List<ValidationResult> results = [];
                if (!Validator.TryValidateValue(newValue!, context, results, validationAttributes))
                    return results.FirstOrDefault()?.ErrorMessage;
            }

            // 2. Name-specific invalid characters (mirrors the ConfigEntity.Name setter).
            if (isEntityName && newValue is string nameValue
                && nameValue.IndexOfAny(ConfigEntity.InvalidNameCharacters) >= 0)
            {
                return $"Name must not contain any of these characters: {string.Join(" ", ConfigEntity.InvalidNameCharacters)}";
            }

            // 3. Custom [Unique] check, scoped to this property's group only.
            if (unique is not null && ObjectToEdit is not null)
            {
                string? violation = FindFirstUniquenessViolation(property);
                if (violation is not null)
                    return violation;
            }

            return null;
        };
    }

    /// <summary>
    /// Checks whether any [Unique]-marked property (or composite group) on this entity collides
    /// with a sibling of the same concrete type. Returns the user-facing message for the first
    /// violating group, or null. When scopedTo is non-null, only that property's group is inspected.
    /// </summary>
    public string? FindFirstUniquenessViolation(PropertyInfo? scopedTo = null)
    {
        if (ObjectToEdit.Parent is null)
            return null;

        string? scopedGroupKey = null;
        if (scopedTo is not null)
        {
            UniqueAttribute? scopedAttr = scopedTo.GetCustomAttribute<UniqueAttribute>();
            if (scopedAttr is null)
                return null;
            scopedGroupKey = scopedAttr.Group ?? $"__singleton__{scopedTo.Name}";
        }

        List<(PropertyInfo Prop, UniqueAttribute Attr)> uniqueProps = [];
        foreach (PropertyInfo prop in ObjectToEdit.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            UniqueAttribute? attr = prop.GetCustomAttribute<UniqueAttribute>();
            if (attr is not null)
                uniqueProps.Add((prop, attr));
        }

        if (uniqueProps.Count == 0)
            return null;

        List<IGrouping<string, (PropertyInfo Prop, UniqueAttribute Attr)>> groups = [.. uniqueProps
            .GroupBy(x => x.Attr.Group ?? $"__singleton__{x.Prop.Name}")];

        if (scopedGroupKey is not null)
            groups = [.. groups.Where(g => g.Key == scopedGroupKey)];

        ConfigEntity[] siblings = ObjectToEdit.Siblings;

        foreach (IGrouping<string, (PropertyInfo Prop, UniqueAttribute Attr)> group in groups)
        {
            List<object?> myValues = [.. group.Select(g => g.Prop.GetValue(ObjectToEdit))];

            // Skip groups with any null/empty value (incomplete key).
            if (myValues.Any(v => v is null || (v is string s && string.IsNullOrWhiteSpace(s))))
                continue;

            Type? requiredType = group.First().Prop.DeclaringType;

            foreach (ConfigEntity sibling in siblings)
            {
                if (requiredType is not null && !requiredType.IsAssignableFrom(sibling.GetType()))
                    continue;

                List<object?> siblingValues = [.. group.Select(g => g.Prop.GetValue(sibling))];
                if (ValuesEqual(myValues, siblingValues))
                    return group.First().Attr.Message ?? "This value is already used.";
            }
        }

        return null;
    }

    private static bool ValuesEqual(IReadOnlyList<object?> a, IReadOnlyList<object?> b)
    {
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] is string sa && b[i] is string sb)
            {
                if (!string.Equals(sa, sb, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            else if (!Equals(a[i], b[i]))
            {
                return false;
            }
        }
        return true;
    }


    private async Task AddAsync(ConfigComponentSlotDescriptor slot)
    {
        if (!await Gate.BeginEditAsync())
            return;

        ConfigEntity? component = await ComponentSelector.CreateForAttachmentAsync(
            slot.ComponentType,
            ObjectToEdit,
            slot.DisplayName);

        if (component is null)
            return;

        SetSlotValue(slot, component);

        await NotifyConfigChangedAsync();
    }

    private async Task DeleteAsync(ConfigComponentSlotDescriptor slot)
    {
        if (!slot.CanDelete)
            return;

        if (!await Gate.BeginEditAsync())
            return;

        if (slot.Property.GetValue(ObjectToEdit) is IConfigRef configRef)
        {
            configRef.Value = null;
        }
        else
        {
            slot.Property.SetValue(ObjectToEdit, null);
        }

        await NotifyConfigChangedAsync();
    }

    private object? GetValue(ConfigScalarFieldDescriptor field)
    {
        return field.Property.GetValue(ObjectToEdit);
    }

    private ConfigEntity? GetComponentValue(ConfigComponentSlotDescriptor slot)
    {
        object? value = slot.Property.GetValue(ObjectToEdit);
        return value switch
        {
            IConfigRef configRef => configRef.Value,
            ConfigEntity entity => entity,
            _ => null,
        };
    }

    private EventCallback<object?> CreateValueChangedCallback(ConfigScalarFieldDescriptor field)
    {
        return EventCallback.Factory.Create<object?>(this, value => SetValueAsync(field, value));
    }

    private EventCallback CreateAddRequestedCallback(ConfigComponentSlotDescriptor slot)
    {
        return EventCallback.Factory.Create(this, () => AddAsync(slot));
    }

    private EventCallback CreateDeleteRequestedCallback(ConfigComponentSlotDescriptor slot)
    {
        return EventCallback.Factory.Create(this, () => DeleteAsync(slot));
    }

    private EventCallback CreateReplaceRequestedCallback(ConfigComponentSlotDescriptor slot)
    {
        return EventCallback.Factory.Create(this, () => ReplaceAsync(slot));
    }

    private EventCallback CreateSelectExistingRequestedCallback(ConfigComponentSlotDescriptor slot)
    {
        return EventCallback.Factory.Create(this, () => SelectExistingAsync(slot));
    }

    private EventCallback<IReadOnlyList<ConfigEntity>> CreateItemsChangedCallback(ConfigComponentSlotDescriptor slot)
    {
        return EventCallback.Factory.Create<IReadOnlyList<ConfigEntity>>(this, items => SetReferenceListValuesAsync(slot, items));
    }

    private async Task SetValueAsync(ConfigScalarFieldDescriptor field, object? value)
    {
        if (!await Gate.BeginEditAsync())
            return;

        field.Property.SetValue(ObjectToEdit, value);

        // Reset-then-validate so every member of a composite [Unique] group (e.g. IP + Port) refreshes
        // its error together. MudForm auto-validates only the edited field; the reset forces the sibling
        // fields to re-render their state too, instead of just the one that changed.
        if (_form is not null)
        {
            await _form.ResetValidationAsync();
            await _form.ValidateAsync();
        }

        await NotifyConfigChangedAsync();
    }

    private async Task SelectExistingAsync(ConfigComponentSlotDescriptor slot)
    {
        if (!await Gate.BeginEditAsync())
            return;

        IEnumerable<ConfigEntity> candidates = ObjectToEdit.Root.AllDescendants
            .Where(candidate => slot.ComponentType.IsInstanceOfType(candidate));

        ConfigEntity? selected = await ComponentSelector.SelectExistingAsync(
            ObjectToEdit,
            slot.DisplayName,
            slot.ComponentType,
            candidates);

        if (selected is null)
            return;

        SetSlotValue(slot, selected);

        await NotifyConfigChangedAsync();
    }

    private Task ReplaceAsync(ConfigComponentSlotDescriptor slot)
    {
        return slot.SupportsSelectExisting
            ? SelectExistingAsync(slot)
            : AddAsync(slot);
    }

    private string GetReplaceActionLabel(ConfigComponentSlotDescriptor slot)
    {
        return slot.SupportsSelectExisting ? CoreRazorText.T("Change selection") : CoreRazorText.T("Replace");
    }

    private string GetReplaceActionIcon(ConfigComponentSlotDescriptor slot)
    {
        return slot.SupportsSelectExisting
            ? Icons.Material.Filled.SwapHoriz
            : Icons.Material.Filled.ChangeCircle;
    }

    private IReadOnlyList<ConfigEntity> GetCandidates(ConfigComponentSlotDescriptor slot)
    {
        return [.. ObjectToEdit.Root.AllDescendants.Where(candidate => slot.ComponentType.IsInstanceOfType(candidate))];
    }

    private IReadOnlyList<ConfigEntity> GetSelectedReferenceValues(ConfigComponentSlotDescriptor slot)
    {
        if (slot.Property.GetValue(ObjectToEdit) is not IEnumerable enumerable)
            return [];

        List<ConfigEntity> items = [];
        foreach (object? item in enumerable)
        {
            if (item is IConfigRef configRef && configRef.Value is ConfigEntity entity)
                items.Add(entity);
        }

        return items;
    }

    private async Task SetReferenceListValuesAsync(ConfigComponentSlotDescriptor slot, IReadOnlyList<ConfigEntity> selectedItems)
    {
        if (!await Gate.BeginEditAsync())
            return;

        IList list = CreateReferenceList(slot.Property.PropertyType);
        Type refItemType = GetReferenceItemType(slot.Property.PropertyType);

        foreach (ConfigEntity selectedItem in selectedItems.Where(slot.ComponentType.IsInstanceOfType))
        {
            IConfigRef configRef = (IConfigRef)Activator.CreateInstance(refItemType)!;
            configRef.Value = selectedItem;
            list.Add(configRef);
        }

        slot.Property.SetValue(ObjectToEdit, list);

        await NotifyConfigChangedAsync();
    }

    private static IList CreateReferenceList(Type propertyType)
    {
        return (IList)(Activator.CreateInstance(propertyType)
            ?? throw new InvalidOperationException($"Unable to create list for property type '{propertyType.FullName}'."));
    }

    private static Type GetReferenceItemType(Type propertyType)
    {
        if (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(List<>))
            throw new InvalidOperationException($"Property type '{propertyType.FullName}' is not a supported config reference list.");

        Type itemType = propertyType.GetGenericArguments()[0];
        if (!typeof(IConfigRef).IsAssignableFrom(itemType))
            throw new InvalidOperationException($"Property type '{propertyType.FullName}' is not a supported config reference list.");

        return itemType;
    }

    private void SetSlotValue(ConfigComponentSlotDescriptor slot, ConfigEntity value)
    {
        if (slot.Property.GetValue(ObjectToEdit) is IConfigRef configRef)
        {
            configRef.Value = value;
            return;
        }

        slot.Property.SetValue(ObjectToEdit, value);
    }
}

