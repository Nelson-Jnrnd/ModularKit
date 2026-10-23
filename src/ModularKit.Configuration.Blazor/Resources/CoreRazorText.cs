using System.Globalization;
using System.Reflection;
using System.Resources;

namespace ModularKit.Configuration.Blazor.Resources;

internal static class CoreRazorText
{
    private static readonly ResourceManager ResourceManager = new(
        "ModularKit.Configuration.Blazor.CoreRazorUi",
        typeof(global::ModularKit.Configuration.Blazor.CoreRazorUi).GetTypeInfo().Assembly);

    internal static string T(string key)
    {
        return ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    internal static string T(string key, params object?[] args)
    {
        string format = T(key);
        return string.Format(CultureInfo.CurrentCulture, format, args);
    }
}

