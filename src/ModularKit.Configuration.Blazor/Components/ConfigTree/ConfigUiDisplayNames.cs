using ModularKit.Configuration.View;
using ModularKit.Configuration.Blazor.Resources;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ModularKit.Configuration.Blazor.Components.ConfigTree;

internal static class ConfigUiDisplayNames
{
    internal static string GetTypeDisplayName(Type type)
    {
        DisplayAttribute? display = type.GetCustomAttribute<DisplayAttribute>(inherit: true);
        return display?.GetName() ?? type.Name;
    }

    internal static string GetPropertyDisplayName(PropertyInfo property)
    {
        DisplayAttribute? display = property.GetCustomAttribute<DisplayAttribute>();
        if (!string.IsNullOrWhiteSpace(display?.GetName()))
            return display.GetName()!;

        ComponentAssociationAttribute? association = property.GetCustomAttribute<ComponentAssociationAttribute>();
        if (!string.IsNullOrWhiteSpace(association?.DisplayName))
            return CoreRazorText.T(association.DisplayName);

        return property.Name;
    }

    internal static string? GetExactSubtypeDisplayName(Type declaredType, Type runtimeType)
    {
        return declaredType == runtimeType
            ? null
            : GetTypeDisplayName(runtimeType);
    }

    internal static string GetVisibleTypeDisplayName(Type declaredType, Type? runtimeType)
    {
        return GetTypeDisplayName(runtimeType ?? declaredType);
    }

    internal static string? GetTypeDetailTooltip(Type declaredType, Type? runtimeType)
    {
        if (runtimeType is null || declaredType == runtimeType)
            return null;

        return CoreRazorText.T("Declared type: {0}", GetTypeDisplayName(declaredType));
    }

    internal static string? GetPropertyBreadcrumb(string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
            ? null
            : CoreRazorText.T("From {0}", propertyName);
    }

    internal static IReadOnlyList<string> GetPathSegments(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return [];

        return path
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}

