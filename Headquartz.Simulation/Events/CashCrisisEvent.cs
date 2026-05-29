using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Events;

public class CashCrisisEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public decimal CashBalance { get; set; }
}
