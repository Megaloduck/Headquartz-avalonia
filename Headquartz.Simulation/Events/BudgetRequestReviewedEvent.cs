using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

public class BudgetRequestReviewedEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public BudgetRequest Request { get; set; } = null!;
}