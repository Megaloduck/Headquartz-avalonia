using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Systems;

public class EventSystem
{
    private readonly Random
        _random = new();

    public void Update(
        Company company)
    {
        if (_random.NextDouble() > 0.15)
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

                Severity =
                    GenerateSeverity(),

                Title =
                    GenerateTitle(
                        department),

                Description =
                    GenerateDescription(
                        department),

                RemainingTicks =
                    _random.Next(30, 120)
            });
    }

    private EventSeverity GenerateSeverity()
    {
        var roll =
            _random.Next(100);

        return roll switch
        {
            < 50 =>
                EventSeverity.Low,

            < 80 =>
                EventSeverity.Medium,

            < 95 =>
                EventSeverity.High,

            _ =>
                EventSeverity.Critical
        };
    }

    private string GenerateTitle(
        DepartmentType department)
    {
        return department switch
        {
            DepartmentType.HumanResources =>
                "Employee Conflict",

            DepartmentType.Finance =>
                "Budget Overrun",

            DepartmentType.Logistics =>
                "Shipment Delay",

            DepartmentType.Marketing =>
                "Campaign Failure",

            DepartmentType.Production =>
                "Machine Breakdown",

            DepartmentType.Sales =>
                "Client Complaint",

            DepartmentType.Warehouse =>
                "Inventory Mismatch",

            _ =>
                "Operational Issue"
        };
    }

    private string GenerateDescription(
        DepartmentType department)
    {
        return department switch
        {
            DepartmentType.HumanResources =>
                "Employee morale is declining.",

            DepartmentType.Finance =>
                "Unexpected expenses detected.",

            DepartmentType.Logistics =>
                "Delivery routes disrupted.",

            DepartmentType.Marketing =>
                "Ad performance dropped sharply.",

            DepartmentType.Production =>
                "Production line halted temporarily.",

            DepartmentType.Sales =>
                "Customer satisfaction decreased.",

            DepartmentType.Warehouse =>
                "Stock count mismatch found.",

            _ =>
                "General operational issue."
        };
    }
}
