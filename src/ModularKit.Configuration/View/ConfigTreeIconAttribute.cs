namespace ModularKit.Configuration.View;

/// <summary>
/// Specifies the icon to display for this type or component-slot property in the configuration tree.
/// </summary>
/// <remarks>
/// <paramref name="iconKey"/> must be the exact name of a public static field on
/// <c>MudBlazor.Icons.Material.Filled</c> (e.g., <c>"House"</c>, <c>"Water"</c>).
/// When placed on a property it takes precedence over the class-level attribute of the
/// property's declared type.  Unknown keys produce no icon at runtime.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class ConfigTreeIconAttribute(string iconKey) : Attribute
{
    public string IconKey { get; } = iconKey;
}

