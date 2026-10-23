using ModularKit.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree.ConfigNodes;

/// <summary>
/// Config tree node for a single <see cref="ConfigEntity"/>.
/// Child nodes are discovered by reflection via <see cref="ConfigElementFactory"/>.
/// </summary>
public sealed class EntityConfigElement : ConfigElement
{
    private readonly ConfigEntity _entity;
    private readonly Type _declaredType;
    private readonly string? _propertyName;

    public EntityConfigElement(ConfigEntity entity, Type? declaredType = null, string? propertyName = null)
    {
        _entity = entity;
        _declaredType = declaredType ?? entity.GetType();
        _propertyName = propertyName;
        Text = entity.Name;
        Icon = ConfigTreeIconResolver.Resolve(entity.GetType());
        Presentation = ConfigPresentationMetadata.Create(Text, _declaredType, entity.GetType(), _propertyName);
        Children = ConfigElementFactory.BuildChildren(entity);
    }

    public ConfigEntity Data => _entity;

    public override string? Description
    {
        get
        {
            Type type = _entity.GetType();
            DisplayAttribute? display = type.GetCustomAttributes<DisplayAttribute>(inherit: true).FirstOrDefault();
            return display?.GetName() ?? type.Name;
        }
    }
    public override void RefreshFromDomain()
    {
        Text = _entity.Name;
        Presentation = ConfigPresentationMetadata.Create(Text, _declaredType, _entity.GetType(), _propertyName);
        Children = ConfigElementFactory.BuildChildren(_entity);
        base.RefreshFromDomain();
    }
}

