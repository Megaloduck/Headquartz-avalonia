using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Systems;

namespace Headquartz.Simulation.Commands;

/// <summary>
/// Raised by any department — reacting to a blocked task, an
/// understaffed line, or just planning ahead — to ask HR for a new hire.
/// Does not hire anyone. HR reviews it via FulfillWorkforceRequestCommand
/// (which actually creates the Employee) or DeclineWorkforceRequestCommand.
/// This is the mechanical channel for "HR fulfills hiring by other
/// department's request" — before this, HR had no way to know what any
/// other department actually needed.
/// </summary>
public class RequestWorkforceCommand : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public DepartmentType RequestingDepartment { get; set; }

    public EmployeeRole Role { get; set; }

    public string Reason { get; set; } =
        "";

    public bool Validate(
        SimulationEngine engine)
    {
        return true;
    }

    public void Execute(
        SimulationEngine engine)
    {
        var request = new WorkforceRequest
        {
            Id = Guid.NewGuid(),

            RequestingDepartment = RequestingDepartment,

            RequestedBy = PlayerId,

            Role = Role,

            Reason = Reason,

            Status = WorkforceRequestStatus.Pending,

            RequestedAtTick = engine.Clock.Tick,
        };

        engine.Company.WorkforceRequests.Add(request);

        engine.Events.Publish(
            new WorkforceRequestSubmittedEvent { Request = request });
    }
}