using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Events;

public class PayrollProcessedEvent : IGameEvent
{
    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public decimal TotalPayroll { get; set; }
}
