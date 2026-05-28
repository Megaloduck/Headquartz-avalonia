using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

public class TaskCompletedEvent : IGameEvent
{
    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public CompanyTask Task { get; set; } = null!;
}
