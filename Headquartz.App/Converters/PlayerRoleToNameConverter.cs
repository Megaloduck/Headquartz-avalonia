using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Headquartz.Domain.Enums;

namespace Headquartz.App.Converters;

/// <summary>
/// Converts a <see cref="PlayerRole"/> enum value into a human-readable role name.
/// </summary>
public class PlayerRoleToNameConverter : IValueConverter
{
    public static readonly PlayerRoleToNameConverter Instance = new();

    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not PlayerRole role)
            return "Unknown Role";

        return role switch
        {
            PlayerRole.HumanResourcesManager => "HR Manager",
            PlayerRole.FinanceManager => "Finance Manager",
            PlayerRole.SalesManager => "Sales Manager",
            PlayerRole.MarketingManager => "Marketing Manager",
            PlayerRole.ProductionManager => "Production Manager",
            PlayerRole.WarehouseManager => "Warehouse Manager",
            PlayerRole.LogisticsManager => "Logistics Manager",
            PlayerRole.Chairman => "Board Chairman",
            _ => "Unknown Role",
        };
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture) =>
        throw new NotSupportedException();
}
