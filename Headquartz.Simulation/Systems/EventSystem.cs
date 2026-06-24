using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Systems;

public class EventSystem
{
    private readonly Random _random = new();

    /// <summary>
    /// Fires random company events with industry-specific flavor.
    /// </summary>
    public void Update(
        SimulationEngine engine,
        double eventFrequency = 0.15,
        double severityBias = 0.0)
    {
        if (_random.NextDouble() > eventFrequency)
            return;

        var department =
            (DepartmentType)
            _random.Next(
                0,
                Enum.GetValues<DepartmentType>()
                    .Length);

        var context = engine.IndustryContext;

        string title = context?.GetEventTitle(department)
            ?? GenerateDefaultTitle(department);

        string description = context?.GetEventDescription(department)
            ?? GenerateDefaultDescription(department);

        engine.Company.Events.Add(
            new CompanyEvent
            {
                Department = department,
                Severity = GenerateSeverity(severityBias),
                Title = title,
                Description = description,
                RemainingTicks = _random.Next(30, 120)
            });
    }

    // =========================================================
    // SEVERITY
    // =========================================================

    private EventSeverity GenerateSeverity(double bias)
    {
        double roll = Math.Clamp(
            _random.NextDouble() + bias,
            0.0,
            0.999);

        return roll switch
        {
            < 0.50 => EventSeverity.Low,
            < 0.80 => EventSeverity.Medium,
            < 0.95 => EventSeverity.High,
            _ => EventSeverity.Critical,
        };
    }

    // =========================================================
    // DEFAULT FALLBACKS (used if no context is available)
    // =========================================================

    private static string GenerateDefaultTitle(DepartmentType department) =>
        department switch
        {
            DepartmentType.HumanResources => "Employee Conflict",
            DepartmentType.Finance => "Budget Overrun",
            DepartmentType.Logistics => "Shipment Delay",
            DepartmentType.Marketing => "Campaign Failure",
            DepartmentType.Production => "Machine Breakdown",
            DepartmentType.Sales => "Client Complaint",
            DepartmentType.Warehouse => "Inventory Mismatch",
            _ => "Operational Issue",
        };

    private static string GenerateDefaultDescription(DepartmentType department) =>
        department switch
        {
            DepartmentType.HumanResources => "Employee morale is declining.",
            DepartmentType.Finance => "Unexpected expenses detected.",
            DepartmentType.Logistics => "Delivery routes disrupted.",
            DepartmentType.Marketing => "Ad performance dropped sharply.",
            DepartmentType.Production => "Production line halted temporarily.",
            DepartmentType.Sales => "Customer satisfaction decreased.",
            DepartmentType.Warehouse => "Stock count mismatch found.",
            _ => "General operational issue.",
        };
}
