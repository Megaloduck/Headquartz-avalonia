using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Systems;

public class MarketingSystem
    : ISimulationSystem
{
    public void Update(
        SimulationEngine engine)
    {
        engine.Company.Reputation +=
            Random.Shared.Next(0, 2);

        engine.Company.Reputation =
            Math.Clamp(
                engine.Company.Reputation,
                0,
                100);
    }
}
