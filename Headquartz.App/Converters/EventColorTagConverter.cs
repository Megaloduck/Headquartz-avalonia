using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts event color tag strings (e.g., "Green", "Red", "Pink") into Avalonia brush keys.
/// Maps to existing theme brushes so the calendar event cards match the design system.
/// </summary>
public class EventColorTagConverter : IValueConverter
{
    public static readonly EventColorTagConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string tag = value?.ToString()?.ToLowerInvariant() ?? "blue";

        string brushKey = tag switch
        {
            "green" => "BrushSuccess",
            "yellow" => "BrushWarning",
            "red" => "BrushDanger",
            "pink" => "BrushAccentPrimary",
            "blue" => "BrushInfo",
            "purple" => "BrushDeptHR",
            "orange" => "BrushDeptWarehouse",
            _ => "BrushInfo",
        };

        // Try to find the resource in the current application
        if (Application.Current?.Resources.TryGetResource(brushKey, null, out var resource) == true)
        {
            if (resource is IBrush brush)
                return brush;
        }

        // Fallback: try merged dictionaries
        foreach (var dict in Application.Current?.Resources.MergedDictionaries ?? [])
        {
            if (dict.TryGetResource(brushKey, null, out var themeResource) && themeResource is IBrush themeBrush)
                return themeBrush;
        }

        return AvaloniaProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
