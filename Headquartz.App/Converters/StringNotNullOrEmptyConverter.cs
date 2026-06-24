using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Headquartz.App.Converters;

/// <summary>
/// Returns false if the string is null or empty, true otherwise.
/// Used to toggle visibility based on text presence.
/// </summary>
public class StringNotNullOrEmptyConverter : IValueConverter
{
    public static readonly StringNotNullOrEmptyConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        string? s = value?.ToString();
        return !string.IsNullOrWhiteSpace(s);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
