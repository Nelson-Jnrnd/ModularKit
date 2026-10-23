namespace ModularKit.Configuration;

/// <summary>
/// Marks a property as unique among siblings of the entity's direct parent.
/// </summary>
/// <remarks>
/// Without <see cref="Group"/>, the property must be unique on its own.
/// With <see cref="Group"/>, the combination of all properties sharing the
/// same group name forms a composite uniqueness constraint — a sibling whose
/// full group-tuple matches this entity's group-tuple counts as a duplicate.
/// Mirrors EF Core's <c>[Index(nameof(X), nameof(Y))]</c> pattern.
/// </remarks>
/// <example>
/// <code>
/// [Unique]
/// public string Name { get; set; }
///
/// [Unique(Group = "Connection", Message = "This IP address is already used.")]
/// public string Host { get; set; }
///
/// [Unique(Group = "Connection", Message = "This IP address is already used.")]
/// public int Port { get; set; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class UniqueAttribute : Attribute
{
    /// <summary>
    /// Optional group name. Multiple properties sharing the same group form
    /// a composite uniqueness constraint. Null means the property is unique
    /// on its own.
    /// </summary>
    public string? Group { get; init; }

    /// <summary>
    /// Optional user-facing error message displayed when a duplicate is
    /// detected. Defaults to "This value is already used." Typically set
    /// once per group so all member fields surface the same message.
    /// </summary>
    public string? Message { get; init; }
}

