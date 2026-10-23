using ModularKit.Configuration;

namespace ModularKit.Configuration.References;

public interface IConfigRef
{
    string? Path { get; set; }
    Type ValueType { get; }
    ConfigEntity? Value { get; set; }
    void Resolve(ConfigEntity owner);
    void SyncPath();
}

public interface IConfigRef<TComponent> : IConfigRef where TComponent : ConfigEntity
{
    new TComponent? Value { get; set; }
}

public interface IRequiredConfigRef : IConfigRef
{
}

