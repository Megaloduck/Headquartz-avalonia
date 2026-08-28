using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Systems;

namespace Headquartz.Simulation.Commands;

/// <summary>
/// HR-only action: declines a pending WorkforceRequest without hiring
/// anyone. No funds move. Exists as its own command rather than a bool
/// flag on Fulfill because the two paths need genuinely different
/// inputs — fulfilling needs a name and salary, declining just needs a
/// reason.
/// </summary>
public class DeclineWorkforceRequestCommand
    : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public Guid RequestId { get; set; }

    public string ReviewNote { get; set; } =
        "";

    public bool Validate(
        SimulationEngine engine)
    {
        return engine.Company.WorkforceRequests
            .Any(r =>
                r.Id == RequestId &&
                r.Status == WorkforceRequestStatus.Pending);
    }

    public void Execute(
        SimulationEngine engine)
    {
        var request = engine.Company.WorkforceRequests
            .FirstOrDefault(r =>
                r.Id == RequestId &&
                r.Status == WorkforceRequestStatus.Pending);

        if (request == null)
            return;

        request.Status = WorkforceRequestStatus.Declined;
        request.ResolvedAtTick = engine.Clock.Tick;
        request.ReviewNote = ReviewNote;

        engine.Events.Publish(
            new WorkforceRequestReviewedEvent { Request = request });
    }
}