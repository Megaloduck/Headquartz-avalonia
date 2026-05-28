using Headquartz.Simulation.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Commands;

public interface ICompanyCommand
{
    Guid Id { get; }

    DateTime Timestamp { get; }

    string PlayerId { get; }

    bool Validate(
        SimulationEngine engine);

    void Execute(
        SimulationEngine engine);
}