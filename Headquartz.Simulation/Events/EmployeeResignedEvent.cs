using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

public class EmployeeResignedEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Employee Employee { get; set; } = null!;
}