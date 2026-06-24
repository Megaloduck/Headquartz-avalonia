using Headquartz.Simulation;
using Headquartz.Simulation.Systems;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquartz.App.Services;

public class SimulationService
{
    public SimulationEngine Engine { get; }

    public SimulationService()
        : this(SimulationProfile.Manager, IndustryType.Food) { }

    public SimulationService(SimulationProfile profile, IndustryType industry = IndustryType.Food)
    {
        Engine = new SimulationEngine(profile, industry);
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
