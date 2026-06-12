using System;
using System.Collections.Generic;
using System.Text;

using System.Globalization;
using Avalonia.Data.Converters;
using Headquartz.App.Models;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts a Lucide icon name string (e.g. "LayoutDashboard")
/// into the corresponding SVG path data string for use with
/// Avalonia's <Path Data="..."/> element.
/// </summary>
public class IconPathConverter : IValueConverter
{
    public static readonly IconPathConverter Instance = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is string iconName)
            return IconPaths.Get(iconName);

        return IconPaths.Get("Square");
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}
