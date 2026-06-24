using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts a boolean to a brush (true = highlight, false = transparent).
/// Parameter can be a resource key for the true brush (e.g., "BrushAccentPrimary").
/// </summary>
public class BoolToHighlightBrushConverter : IValueConverter
{
    public static readonly BoolToHighlightBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isHighlighted = value is true;
        if (!isHighlighted)
            return new SolidColorBrush(Colors.Transparent);

        string key = parameter?.ToString() ?? "BrushAccentPrimary";

        if (Application.Current?.Resources.TryGetResource(key, null, out var resource) == true)
        {
            if (resource is IBrush brush)
                return brush;
        }

        foreach (var dict in Application.Current?.Resources.MergedDictionaries ?? [])
        {
            if (dict.TryGetResource(key, null, out var themeResource) && themeResource is IBrush themeBrush)
                return themeBrush;
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
