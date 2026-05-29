using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Events;

public class DepartmentCrisisEvent : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public DepartmentType Department { get; set; }
    public int StressLevel { get; set; }
}
