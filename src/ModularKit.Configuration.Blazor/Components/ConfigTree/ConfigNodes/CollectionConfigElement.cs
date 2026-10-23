using ModularKit.Configuration;
using System.Collections;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;

/// <summary>
/// Config tree node for a collection of <see cref="ConfigEntity"/> items.
/// Holds the real typed list (e.g. <c>IList&lt;Device&gt;</c>) as the non-generic <see cref="IList"/>,
/// which allows in-place mutation without generics or <c>MakeGenericMethod</c>.
/// </summary>
public sealed class CollectionConfigElement : ConfigElement, IConfigCollectionParent
{
    private readonly ConfigEntity _owner;
    private readonly IList _items;
    private readonly Type _itemType;
    private readonly string _propertyName;

    public CollectionConfigElement(
        ConfigEntity owner,
        IList items,
        Type itemType,
        string text,
        string icon)
    {
        _owner = owner;
        _items = items;
        _itemType = itemType;
        _propertyName = text;
        Text = text;
        Icon = icon;
        Presentation = ConfigPresentationMetadata.Create(Text, itemType, itemType, _propertyName);
        Children = [.. Items.Select(item => (ConfigElement)new EntityConfigElement(item, itemType, _propertyName))];
    }

    public ConfigEntity Owner => _owner;
    public Type ItemType => _itemType;

    /// <summary>Items exposed as <see cref="ConfigEntity"/> for display.</summary>
    public IEnumerable<ConfigEntity> Items => _items.Cast<ConfigEntity>();

    public void AddItem(ConfigEntity item) => _items.Add(item);
    public void RemoveItem(ConfigEntity item)
    {
        if (ConfigDeletionGuard.GetExternalReferences(item).Count > 0)
            throw new InvalidOperationException("The item or one of its children is still referenced.");
        _items.Remove(item);
    }

    public void RemoveChildElement(ConfigElement child)
    {
        if (child is EntityConfigElement entityChild)
            RemoveItem(entityChild.Data);
    }

    public override void RefreshFromDomain()
    {
        Presentation = ConfigPresentationMetadata.Create(Text, _itemType, _itemType, _propertyName);
        Children = [.. Items.Select(item => (ConfigElement)new EntityConfigElement(item, _itemType, _propertyName))];
        base.RefreshFromDomain();
    }

    public override string? Description => null;
}

