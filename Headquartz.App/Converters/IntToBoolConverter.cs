using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Headquartz.App.Converters;

/// <summary>
/// Compares an int value against a threshold (parameter) and returns a boolean.
/// Default: returns true if value == 0 (useful for "no items" visibility).
/// Parameter "0" means true when value == 0.
/// Parameter ">0" means true when value > 0.
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public static readonly IntToBoolConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not int count)
            return false;

        string param = parameter?.ToString() ?? "0";

        return param switch
        {
            ">0" => count > 0,
            "==0" or "0" => count == 0,
            ">=1" => count >= 1,
            _ => count == 0,
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
