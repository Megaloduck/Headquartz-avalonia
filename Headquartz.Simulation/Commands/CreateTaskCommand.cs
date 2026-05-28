using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Commands;

public class CreateTaskCommand
    : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public string Name { get; set; } =
        "";

    public string Description { get; set; } =
        "";

    public DepartmentType Department { get; set; }

    public TaskPriority Priority { get; set; }

    public int DurationTicks { get; set; }

    public decimal BudgetCost { get; set; }

    public bool Validate(
        SimulationEngine engine)
    {
        return engine.Company.Cash >= BudgetCost;
    }

    public void Execute(
        SimulationEngine engine)
    {
        var task = new CompanyTask
        {
            Id = Guid.NewGuid(),

            Name = Name,

            Description = Description,

            Department = Department,

            Priority = Priority,

            Status =
                CompanyTaskStatus.Pending,

            RequiredEmployees = 1,

            AssignedEmployees = 0,

            Progress = 0,

            DurationTicks = DurationTicks,

            RemainingTicks = DurationTicks,

            BudgetCost = BudgetCost,
        };

        engine.Company.Tasks.Add(task);

        engine.Company.Cash -= BudgetCost;
    }
}
