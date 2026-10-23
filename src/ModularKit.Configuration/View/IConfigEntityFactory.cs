using ModularKit.Configuration;

namespace ModularKit.Configuration.View;

/// <summary>
/// Creates <see cref="ConfigEntity"/> instances for the configuration tree.
/// <list type="bullet">
///   <item>For abstract types: opens a plugin-picker dialog so the user can choose an implementation.</item>
///   <item>For concrete types: creates the instance directly without a dialog.</item>
///   <item><see cref="CreateForAttachmentAsync"/> returns a new entity that is fully completed and wired for attachment under an owner.</item>
/// </list>
/// </summary>
public interface IConfigEntityFactory
{
    /// <summary>
    /// Creates a new entity of the given type.
    /// Returns <c>null</c> if the user cancels (dialog) or creation fails.
    /// </summary>
    Task<ConfigEntity?> CreateAsync(Type type, ConfigEntity? owner = null, string? displayName = null);

    /// <summary>
    /// Opens a selection dialog for choosing an existing entity from <paramref name="candidates"/>.
    /// </summary>
    Task<ConfigEntity?> SelectExistingAsync(ConfigEntity owner, string displayName, Type componentType, IEnumerable<ConfigEntity> candidates);

    /// <summary>
    /// Creates a new entity of the given type, completes any required associations, and wires it under
    /// <paramref name="owner"/> only if the flow completes successfully.
    /// </summary>
    Task<ConfigEntity?> CreateForAttachmentAsync(Type type, ConfigEntity owner, string? displayName = null);
}

