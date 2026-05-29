using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

public class OrderFailedEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public SalesOrder Order { get; set; } = null!;
}
