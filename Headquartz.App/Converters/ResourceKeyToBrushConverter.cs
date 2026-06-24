using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace Headquartz.App.Converters;

public class ResourceKeyToBrushConverter : IValueConverter
{
    public static readonly ResourceKeyToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string key || string.IsNullOrWhiteSpace(key))
            return AvaloniaProperty.UnsetValue;

        // Try to find the resource in the current application
        if (Application.Current?.Resources.TryGetResource(key, null, out var resource) == true)
        {
            if (resource is IBrush brush)
                return brush;
        }

        // Fallback: try to find in the theme dictionaries
        foreach (var dict in Application.Current?.Resources.MergedDictionaries ?? [])
        {
            if (dict.TryGetResource(key, null, out var themeResource) && themeResource is IBrush themeBrush)
                return themeBrush;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
