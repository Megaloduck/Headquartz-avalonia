using Headquartz.Simulation;
using Headquartz.Simulation.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headquartz.App.Services;

public class SimulationService
{
    public SimulationEngine Engine { get; }

    /// <summary>Default constructor — uses Manager difficulty profile.</summary>
    public SimulationService()
        : this(SimulationProfile.Manager) { }

    /// <summary>Profile-aware constructor used after onboarding completes.</summary>
    public SimulationService(SimulationProfile profile)
    {
        Engine = new SimulationEngine(profile);
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