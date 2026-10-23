namespace ModularKit.Configuration.References;

public sealed class ConfigIntegrityException : Exception
{
    public IReadOnlyList<ConfigReferenceEntry> ReferenceErrors { get; }

    public ConfigIntegrityException(IEnumerable<ConfigReferenceEntry> referenceErrors)
        : base("Configuration contains invalid references.")
    {
        ReferenceErrors = referenceErrors.ToArray();
    }
}

