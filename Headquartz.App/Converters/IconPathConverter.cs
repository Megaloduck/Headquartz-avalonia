using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts a Lucide-style icon name string (e.g. "LayoutDashboard") into the
/// matching "Icon{Name}" StreamGeometry resource defined in IconResources.axaml,
/// for use with Avalonia's <Path Data="..."/> element.
/// Falls back to "IconSquare" if the name isn't found.
/// </summary>
public class IconPathConverter : IValueConverter
{
    public static readonly IconPathConverter Instance = new();

    private const string FallbackKey = "IconSquare";

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        string key = value is string iconName && !string.IsNullOrWhiteSpace(iconName)
            ? $"Icon{iconName}"
            : FallbackKey;

        return ResolveGeometry(key) ?? ResolveGeometry(FallbackKey);
    }

    private static Geometry? ResolveGeometry(string key)
    {
        if (Application.Current?.Resources.TryGetResource(key, null, out var resource) == true
            && resource is Geometry geometry)
        {
            return geometry;
        }

        foreach (var dict in Application.Current?.Resources.MergedDictionaries ?? [])
        {
            if (dict.TryGetResource(key, null, out var themeResource) && themeResource is Geometry themeGeometry)
                return themeGeometry;
        }

        return null;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}   