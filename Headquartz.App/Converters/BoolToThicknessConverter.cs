using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts a boolean to a Thickness (true = value from parameter, false = 0).
/// Parameter should be a number string (e.g., "2").
/// </summary>
public class BoolToThicknessConverter : IValueConverter
{
    public static readonly BoolToThicknessConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isTrue = value is true;
        if (!isTrue)
            return new Thickness(0);

        double thickness = 2;
        if (parameter != null && double.TryParse(parameter.ToString(), out double parsed))
            thickness = parsed;

        return new Thickness(thickness);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
