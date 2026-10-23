using ModularKit.Configuration;
using ModularKit.Configuration.Blazor.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModularKit.Configuration.Blazor.Services.ItemSelector;

internal static class ConfigSelectorTitleBuilder
{
    private static readonly string DefaultDraftName = CoreRazorText.T("Untitled");

    internal static string Build(ConfigEntity owner, string displayName, Type componentType)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(componentType);

        string ownerLabel = ShouldUseOwnerTypeLabel(owner)
            ? ResolveTypeDisplayName(owner.GetType())
            : owner.Name;

        return CoreRazorText.T("Select {0} ({1}) for {2}", displayName, ResolveTypeDisplayName(componentType), ownerLabel);
    }

    private static bool ShouldUseOwnerTypeLabel(ConfigEntity owner)
    {
        if (string.IsNullOrWhiteSpace(owner.Name))
            return true;

        return owner.Parent is null
            && string.Equals(owner.Name, DefaultDraftName, StringComparison.Ordinal);
    }

    internal static string ResolveTypeDisplayName(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        DisplayAttribute? display = type.GetCustomAttribute<DisplayAttribute>(inherit: true);
        string? name = display?.GetName();

        return string.IsNullOrWhiteSpace(name) ? type.Name : name;
    }
}

