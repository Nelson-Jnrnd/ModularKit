using System.Reflection;

namespace ModularKit.Configuration.View;

/// <summary>
/// Marks a config entity type that is never designated by its name: no config reference points at it
/// by name, and no operation screen labels it. Its name is generated when the entity is created and
/// is not offered for editing.
/// </summary>
/// <remarks>
/// The name still exists and still forms the entity's Path, because Path keys config references and
/// alert sources. This attribute only removes the name from the config form; it does not relax the
/// sibling-uniqueness rule enforced by the
/// <see cref="ModularKit.Configuration.ConfigEntity.Name"/> setter.
///
/// Inherited, so every subtype of a marked type — including plugin implementations — is auto-named too.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class AutoNamedAttribute : Attribute
{
    /// <summary>
    /// Returns true when <paramref name="entityType"/> or one of its base types carries the attribute.
    /// </summary>
    public static bool AppliesTo(Type entityType)
    {
        ArgumentNullException.ThrowIfNull(entityType);
        return entityType.GetCustomAttribute<AutoNamedAttribute>(inherit: true) is not null;
    }
}

