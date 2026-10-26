using MudBlazor;

namespace DeviceWorkbench.Theme;

public static class WorkbenchTheme
{
    public static MudTheme Value { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2563eb",
            PrimaryContrastText = "#ffffff",
            Secondary = "#475569",
            Background = "#f4f6fa",
            Surface = "#ffffff",
            TextPrimary = "#172338",
            TextSecondary = "#59677b",
            LinesDefault = "#dbe2ec",
            Divider = "#e5eaf1",
            Success = "#18734d",
            Error = "#c52b36",
        },
        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "8px" },
        Typography = new Typography
        {
            Default = new DefaultTypography { FontFamily = ["Segoe UI", "Arial", "sans-serif"] },
        },
    };
}
