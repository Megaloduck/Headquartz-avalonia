using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Ticks;

public class SimulationClock
{
    public long Tick { get; private set; }

    public DateTime WorldTime { get; private set; }

    public event Action? OnTick;

    public SimulationClock()
    {
        WorldTime =
            new DateTime(2026, 1, 1, 8, 0, 0);
    }

    public void Advance()
    {
        Tick++;

        WorldTime =
            WorldTime.AddMinutes(15);

        OnTick?.Invoke();
    }
}
