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
    /// Fires random company events.
    /// </summary>
    /// <param name="company">The company to add events to.</param>
    /// <param name="eventFrequency">
    ///     Probability (0–1) that an event fires this tick.
    ///     Sourced from SimulationProfile.EventFrequency.
    /// </param>
    /// <param name="severityBias">
    ///     Shifts severity distribution. Negative = easier (more Low),
    ///     positive = harder (more Critical). Range roughly −1 to +1.
    ///     Sourced from SimulationProfile.SeverityBias.
    /// </param>
    public void Update(
        Company company,
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

        company.Events.Add(
            new CompanyEvent
            {
                Department = department,
                Severity = GenerateSeverity(severityBias),
                Title = GenerateTitle(department),
                Description = GenerateDescription(department),
                RemainingTicks = _random.Next(30, 120)
            });
    }

    // =========================================================
    // SEVERITY
    // =========================================================

    private EventSeverity GenerateSeverity(double bias)
    {
        // bias shifts the roll: negative makes it easier, positive harder
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
    // TITLE / DESCRIPTION
    // =========================================================

    private string GenerateTitle(DepartmentType department) =>
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

    private string GenerateDescription(DepartmentType department) =>
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