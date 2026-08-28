using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Systems;

namespace Headquartz.Simulation.Commands;

/// <summary>
/// Raised by any department when its own Budget can't cover something it
/// needs (a hire, a task, a purchase). Does not move money — Finance must
/// review and approve or deny it case-by-case via
/// ReviewBudgetRequestCommand. This is the mechanical hook for
/// "structured pressure": a department can't just spend past its
/// allocation, it has to ask, and Finance has to act on it.
/// </summary>
public class RequestBudgetIncreaseCommand : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public DepartmentType Department { get; set; }

    public decimal Amount { get; set; }

    public string Reason { get; set; } =
        "";

    public bool Validate(
        SimulationEngine engine)
    {
        return Amount > 0;
    }

    public void Execute(
        SimulationEngine engine)
    {
        var request = new BudgetRequest
        {
            Id = Guid.NewGuid(),

            Department = Department,

            RequestedBy = PlayerId,

            Amount = Amount,

            Reason = Reason,

            Status = BudgetRequestStatus.Pending,

            RequestedAtTick = engine.Clock.Tick,
        };

        engine.Company.BudgetRequests.Add(request);

        engine.Events.Publish(
            new BudgetRequestSubmittedEvent { Request = request });
    }
}