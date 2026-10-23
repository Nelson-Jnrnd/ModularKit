namespace ModularKit.Plugins;

/// <summary>Gives a concrete implementation a JSON identifier independent of its CLR namespace.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PluginTypeIdAttribute(string id) : Attribute
{
    public string Id { get; } = string.IsNullOrWhiteSpace(id)
        ? throw new ArgumentException("A type identifier cannot be blank.", nameof(id)) : id;
}
