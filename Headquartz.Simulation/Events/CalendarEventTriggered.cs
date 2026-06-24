using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

/// <summary>
/// Published when a scheduled calendar event starts (e.g., Grand Opening, Black Friday).
/// Handled by the <see cref="SimulationEngine"/> to apply department effects and create a <see cref="CompanyEvent"/>.
/// </summary>
public class CalendarEventTriggered : IGameEvent
{
    public DateTime Timestamp { get; } = DateTime.UtcNow;

    public CalendarEventDefinition Definition { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }
}
