using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Systems;

public interface ISimulationSystem
{
    void Update(
        SimulationEngine engine);
}
