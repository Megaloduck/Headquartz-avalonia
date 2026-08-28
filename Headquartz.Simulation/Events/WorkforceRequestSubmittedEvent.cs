using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

public class WorkforceRequestSubmittedEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public WorkforceRequest Request { get; set; } = null!;
}