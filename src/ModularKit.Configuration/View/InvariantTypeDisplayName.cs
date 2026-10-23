using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace ModularKit.Configuration.View;

/// <summary>
/// Resolves the display name of a config entity type in the invariant culture.
/// </summary>
/// <remarks>
/// <see cref="DisplayAttribute.GetName"/> resolves against the ambient culture, which is right for
/// labels and dialog titles but wrong for a generated entity name: that name is persisted, forms the
/// entity's Path, keys config references, and travels to the shared monitoring database as an alert
/// source path. Resolving it per culture would give the same configuration a different identity key
/// depending on the machine that created it.
/// </remarks>
public static class InvariantTypeDisplayName
{
    /// <summary>
    /// Returns the neutral-resource display name of <paramref name="type"/>, falling back to the
    /// literal <see cref="DisplayAttribute.Name"/> when no resource backs it, then to the type name.
    /// </summary>
    public static string ForType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        DisplayAttribute? display = type.GetCustomAttribute<DisplayAttribute>(inherit: true);
        if (display is null || string.IsNullOrWhiteSpace(display.Name))
            return type.Name;

        if (display.ResourceType is null)
            return display.Name;

        ResourceManager? resourceManager = GetResourceManager(display.ResourceType);
        string? invariantName = resourceManager?.GetString(display.Name, CultureInfo.InvariantCulture);

        return string.IsNullOrWhiteSpace(invariantName) ? type.Name : invariantName;
    }

    private static ResourceManager? GetResourceManager(Type resourceType)
    {
        PropertyInfo? property = resourceType.GetProperty(
            "ResourceManager",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        return property?.GetValue(null) as ResourceManager;
    }
}

