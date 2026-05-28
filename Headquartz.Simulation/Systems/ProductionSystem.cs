using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Systems;

public class ProductionSystem
    : ISimulationSystem
{
    public void Update(
        SimulationEngine engine)
    {
        var productionTasks =
            engine.Company.Tasks
                .Where(t =>
                    t.Department ==
                    DepartmentType.Production &&
                    t.Status ==
                    CompanyTaskStatus.Completed)
                .ToList();

        foreach (var task in productionTasks)
        {
            var steel =
                engine.Company.Inventory
                    .FirstOrDefault(i =>
                        i.Name == "Steel");

            if (steel != null)
            {
                steel.Quantity -= 5;
            }
        }
    }
}
