using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

/// <summary>
/// Published for either outcome — check Request.Status to tell fulfilled
/// from declined. Mirrors BudgetRequestReviewedEvent's shape so the two
/// review workflows stay symmetric.
/// </summary>
public class WorkforceRequestReviewedEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public WorkforceRequest Request { get; set; } = null!;
}