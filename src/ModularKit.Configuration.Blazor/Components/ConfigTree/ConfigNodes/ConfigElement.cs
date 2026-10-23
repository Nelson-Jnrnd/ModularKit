using ModularKit.Configuration;
using MudBlazor;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;

public interface IConfigCollectionParent
{
    void RemoveChildElement(ConfigElement child);
}

public abstract class ConfigElement
{
    public virtual string Text { get; protected set; } = string.Empty;

    public virtual string? Description => null;

    public ConfigPresentationMetadata Presentation { get; protected set; } =
        ConfigPresentationMetadata.Create(string.Empty, typeof(object), typeof(object));

    public string Icon { get; protected set; } = Icons.Material.Filled.Help;

    public ConfigElement? Parent { get; internal set; }

    public bool IsDeletable => Parent is IConfigCollectionParent;

    private ConfigElement[] _children = [];
    public ConfigElement[] Children
    {
        get => _children;
        protected set
        {
            _children = value;
            foreach (ConfigElement child in _children)
                child.Parent = this;
        }
    }

    public override string ToString() => Text;

    public EntityConfigElement? FindInChildren(ConfigEntity target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (this is EntityConfigElement entityElement && ReferenceEquals(entityElement.Data, target))
            return entityElement;

        foreach (ConfigElement child in Children)
        {
            EntityConfigElement? found = child.FindInChildren(target);
            if (found is not null)
                return found;
        }

        return null;
    }

    public virtual void RefreshFromDomain()
    {
        foreach (ConfigElement child in Children)
        {
            child.RefreshFromDomain();
        }
    }
}

