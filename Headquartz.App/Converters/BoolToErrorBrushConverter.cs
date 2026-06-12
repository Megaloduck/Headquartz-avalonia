using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts a bool (IsError) to a border brush:
/// true -> danger color, false -> default border color.
/// Used for status message borders (e.g. settings form feedback).
/// </summary>
public class BoolToErrorBrushConverter : IValueConverter
{
    public static readonly BoolToErrorBrushConverter Instance = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        bool isError = value is true;

        return isError
            ? new SolidColorBrush(Color.Parse("#EF4444"))
            : new SolidColorBrush(Color.Parse("#10B981"));
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}