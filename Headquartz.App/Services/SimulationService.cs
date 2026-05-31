using Headquartz.Simulation.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquartz.App.Services;

public class SimulationService
{
    public SimulationEngine Engine { get; }

    public SimulationService()
    {
        Engine = new SimulationEngine();
    }

    public async Task StartAsync()
    {
        await Engine.StartAsync();
    }

    public void SetTickSpeed(double multiplier)
    {
        Engine.SetTickSpeed(multiplier);
    }
}