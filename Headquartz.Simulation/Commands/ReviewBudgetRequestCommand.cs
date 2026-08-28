using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Systems;

namespace Headquartz.Simulation.Commands;

/// <summary>
/// Finance-only action: approve or deny a pending BudgetRequest.
/// Approval transfers the requested amount out of company Cash and into
/// the requesting department's Budget; denial just closes the request
/// out with no funds moved. Every request is reviewed on its own —
/// there's no batch-approve, matching the "case by case" review model.
/// </summary>
public class ReviewBudgetRequestCommand : ICompanyCommand
{
    public Guid Id { get; } =
        Guid.NewGuid();

    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public string PlayerId { get; set; } =
        "";

    public Guid RequestId { get; set; }

    public bool Approve { get; set; }

    public string ReviewNote { get; set; } =
        "";

    public bool Validate(
        SimulationEngine engine)
    {
        var request = engine.Company.BudgetRequests
            .FirstOrDefault(r =>
                r.Id == RequestId &&
                r.Status == BudgetRequestStatus.Pending);

        if (request == null)
            return false;

        // Denials always go through. Approvals still need the money to
        // actually exist company-wide before it can be released.
        return !Approve || engine.Company.Cash >= request.Amount;
    }

    public void Execute(
        SimulationEngine engine)
    {
        var request = engine.Company.BudgetRequests
            .FirstOrDefault(r =>
                r.Id == RequestId &&
                r.Status == BudgetRequestStatus.Pending);

        if (request == null)
            return;

        request.ReviewedAtTick = engine.Clock.Tick;
        request.ReviewNote = ReviewNote;

        var dept = DepartmentBudgetGuard.Find(engine, request.Department);

        if (Approve && dept != null)
        {
            engine.Company.Cash -= request.Amount;
            dept.Budget += request.Amount;
            request.Status = BudgetRequestStatus.Approved;
        }
        else
        {
            request.Status = BudgetRequestStatus.Denied;
        }

        engine.Events.Publish(
            new BudgetRequestReviewedEvent { Request = request });
    }
}