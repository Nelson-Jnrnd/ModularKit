namespace ModularKit.Configuration.Blazor.Components.ConfigTree;

/// <summary>
/// Editing permission cascaded to the configuration tree and its panels. Components render from
/// <see cref="IsReadOnly"/> and call <see cref="BeginEditAsync"/> before every mutation, so the
/// host decides both what is editable and what an edit costs.
/// </summary>
public sealed class ConfigEditGate
{
    private readonly Func<bool> _isReadOnly;
    private readonly Func<Task<bool>> _beginEdit;

    /// <summary>Gate for a host that imposes no restriction, and the default when none is cascaded.</summary>
    public static ConfigEditGate Unrestricted { get; } = new(() => false, () => Task.FromResult(true));

    public ConfigEditGate(Func<bool> isReadOnly, Func<Task<bool>> beginEdit)
    {
        _isReadOnly = isReadOnly;
        _beginEdit = beginEdit;
    }

    /// <summary>True while the configuration may be read but not changed.</summary>
    public bool IsReadOnly => _isReadOnly();

    /// <summary>
    /// Asks the host to authorise a mutation about to be applied. Returns false when the caller
    /// must leave the configuration untouched.
    /// </summary>
    public Task<bool> BeginEditAsync() => _beginEdit();
}

