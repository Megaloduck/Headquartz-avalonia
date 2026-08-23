using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Headquartz.Domain.Enums;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts a <see cref="DepartmentType"/> enum value into a human-readable
/// department name for player-facing display. Deliberately omits "Manager"
/// from every label (e.g. "Human Resources", not "HR Manager") to avoid
/// colliding with GameDifficulty.Manager in the shell's role/difficulty
/// badge. Management is special-cased as "Board Chairman" since that's
/// the player-facing title for the full-oversight role.
/// </summary>
public class DepartmentTypeToNameConverter : IValueConverter
{
    public static readonly DepartmentTypeToNameConverter Instance = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not DepartmentType department)
            return "Unknown Department";

        return GetDisplayName(department);
    }

    /// <summary>
    /// Single source of truth for DepartmentType display names — callable
    /// from XAML (via this converter) or plain C# (ViewModels), so raw
    /// enum names like "HumanResources" never leak into the UI.
    /// </summary>
    public static string GetDisplayName(DepartmentType department) => department switch
    {
        DepartmentType.HumanResources => "Human Resources",
        DepartmentType.Finance => "Finance",
        DepartmentType.Sales => "Sales",
        DepartmentType.Marketing => "Marketing",
        DepartmentType.Production => "Production",
        DepartmentType.Warehouse => "Warehouse",
        DepartmentType.Logistics => "Logistics",
        DepartmentType.Management => "Board Chairman",
        _ => "Unknown Department",
    };

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}