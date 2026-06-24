using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Ticks;

/// <summary>
/// Defines starting modifiers and flavour for each industry type.
/// Applied on top of the difficulty <see cref="SimulationProfile"/>.
/// </summary>
public sealed record IndustryProfile
{
    public int StartingReputation { get; init; } = 50;
    public decimal CashMultiplier { get; init; } = 1.0m;
    public double EventFrequencyModifier { get; init; } = 1.0;

    /// <summary>
    /// Efficiency delta applied to seeded departments.
    /// Key = department type, Value = delta (+/-).
    /// </summary>
    public Dictionary<DepartmentType, int> DepartmentEfficiencyDelta { get; init; } = [];

    /// <summary>
    /// Budget delta applied to seeded departments.
    /// </summary>
    public Dictionary<DepartmentType, decimal> DepartmentBudgetDelta { get; init; } = [];

    /// <summary>
    /// Multiplier for the number of starter employees (base is 8).
    /// </summary>
    public double EmployeeCountMultiplier { get; init; } = 1.0;

    public static IndustryProfile For(IndustryType type) =>
        type switch
        {
            IndustryType.Food          => Food,
            IndustryType.Beverage      => Beverage,
            IndustryType.Entertainment => Entertainment,
            IndustryType.Automotive    => Automotive,
            IndustryType.Fashion       => Fashion,
            _                          => Default,
        };

    public static IndustryProfile Default => new();

    public static IndustryProfile Food => new()
    {
        StartingReputation = 55,
        CashMultiplier = 0.95m,
        EventFrequencyModifier = 1.05,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Production] = 10,
            [DepartmentType.Warehouse]  = 10,
            [DepartmentType.Logistics]  = 5,
            [DepartmentType.Marketing]  = -5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Production] = 5_000,
            [DepartmentType.Warehouse]  = 5_000,
        },
        EmployeeCountMultiplier = 1.1,
    };

    public static IndustryProfile Beverage => new()
    {
        StartingReputation = 50,
        CashMultiplier = 1.0m,
        EventFrequencyModifier = 1.0,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Production] = 10,
            [DepartmentType.Warehouse]  = 5,
            [DepartmentType.Logistics]  = 10,
            [DepartmentType.Sales]      = 5,
            [DepartmentType.Marketing]  = 5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Production] = 4_000,
            [DepartmentType.Warehouse]  = 3_000,
        },
        EmployeeCountMultiplier = 1.0,
    };

    public static IndustryProfile Entertainment => new()
    {
        StartingReputation = 65,
        CashMultiplier = 1.05m,
        EventFrequencyModifier = 1.2,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Marketing]  = 15,
            [DepartmentType.Sales]      = 10,
            [DepartmentType.Production] = -10,
            [DepartmentType.Warehouse]  = -20,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Marketing]  = 8_000,
            [DepartmentType.Sales]      = 5_000,
            [DepartmentType.Production] = -5_000,
            [DepartmentType.Warehouse]  = -5_000,
        },
        EmployeeCountMultiplier = 0.75,
    };

    public static IndustryProfile Automotive => new()
    {
        StartingReputation = 55,
        CashMultiplier = 1.1m,
        EventFrequencyModifier = 0.9,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Production] = 15,
            [DepartmentType.Warehouse]  = 5,
            [DepartmentType.Logistics]  = 10,
            [DepartmentType.Sales]      = 5,
            [DepartmentType.Marketing]  = -5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Production] = 10_000,
            [DepartmentType.Warehouse]  = 5_000,
            [DepartmentType.Logistics]  = 5_000,
        },
        EmployeeCountMultiplier = 1.2,
    };

    public static IndustryProfile Fashion => new()
    {
        StartingReputation = 55,
        CashMultiplier = 0.9m,
        EventFrequencyModifier = 1.1,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Marketing]  = 10,
            [DepartmentType.Sales]      = 10,
            [DepartmentType.Production] = 5,
            [DepartmentType.Warehouse]  = 5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Marketing]  = 5_000,
            [DepartmentType.Sales]      = 3_000,
            [DepartmentType.Production] = 3_000,
        },
        EmployeeCountMultiplier = 1.0,
    };
}
