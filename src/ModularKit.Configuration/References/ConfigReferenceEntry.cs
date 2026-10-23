namespace ModularKit.Configuration.References;

public sealed record ConfigReferenceEntry(
    string OwnerPath,
    string PropertyName,
    string? TargetPath,
    IConfigRef Reference);

