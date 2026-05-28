using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Entities;

namespace Headquartz.Simulation.Events;

public class InventoryLowEvent : IGameEvent
{
    public DateTime Timestamp { get; } =
        DateTime.UtcNow;

    public InventoryItem Item { get; set; } = null!;
}