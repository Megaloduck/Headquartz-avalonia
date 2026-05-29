using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Events;

public class PayrollFailedEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public decimal TotalPayroll { get; set; }
    public decimal Shortfall { get; set; }
}