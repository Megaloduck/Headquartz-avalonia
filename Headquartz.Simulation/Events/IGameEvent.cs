using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Events;

public interface IGameEvent
{
    DateTime Timestamp { get; }
}