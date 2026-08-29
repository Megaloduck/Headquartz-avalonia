using Headquartz.Simulation.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Phase
{
    // <summary>
    /// Published once, when DeclareGrandOpeningCommand flips Company.Phase.
    /// Exists mainly so the UI (notification feed, shell chrome, a splash
    /// moment — whatever's built later) has a single clean hook for "the
    /// real game just started" without polling Company.Phase every tick.
    /// </summary>
    public class CompanyEnteredGrandOpeningEvent : IGameEvent
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

}
