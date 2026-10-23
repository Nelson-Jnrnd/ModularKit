namespace ModularKit.Plugins;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PluginTypeDisplayNameAttribute : Attribute
{
    public string DisplayName { get; }
    public PluginTypeDisplayNameAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}

