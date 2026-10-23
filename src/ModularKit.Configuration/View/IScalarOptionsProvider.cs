namespace ModularKit.Configuration.View;

/// <summary>
/// How a <see cref="ScalarOptionBadge"/> should read: <see cref="Neutral"/> for plain descriptors,
/// the others for states the operator should notice while choosing.
/// </summary>
public enum ScalarOptionBadgeKind
{
    Neutral,
    Info,
    Success,
    Warning,
}

/// <summary>
/// A short descriptor shown alongside an option (rendered as a chip) but never matched by the search
/// box, so decorations such as a role or a status do not pollute name searches.
/// </summary>
public readonly record struct ScalarOptionBadge(string Text, ScalarOptionBadgeKind Kind = ScalarOptionBadgeKind.Neutral);

/// <summary>
/// One entry of a scalar property's selection list: <paramref name="Value"/> is stored in the
/// configuration, <paramref name="Label"/> is what the operator reads and searches on, and
/// <paramref name="Badges"/> are optional descriptors shown but excluded from the search.
/// </summary>
public readonly record struct ScalarOption(
    string Value,
    string Label,
    IReadOnlyList<ScalarOptionBadge>? Badges = null);

/// <summary>
/// Supplies the options of a property marked with <see cref="ScalarOptionsAttribute"/>. Implementations
/// live outside the configuration UI and typically read from a source the configuration tree does not
/// own, such as the user directory.
/// </summary>
public interface IScalarOptionsProvider
{
    Task<IReadOnlyList<ScalarOption>> GetOptionsAsync(CancellationToken ct = default);
}

