using Headquartz.Domain.Enums;
using Headquartz.Simulation.Modules.Base;

namespace Headquartz.Simulation.Systems;

public class ProductionSystem
    : ISimulationSystem
{
    public void Update(SimulationEngine engine)
    {
        var productionTasks =
            engine.Company.Tasks
                .Where(t =>
                    t.Department ==
                    DepartmentType.Production &&
                    t.Status ==
                    CompanyTaskStatus.Completed)
                .ToList();

        // Resource consumption is now handled in SimulationEngine.ProcessTasks
        // when tasks complete, via IndustryContext.ConsumeProductionResources().
        // This system is now a lightweight pass-through.
        // If an industry needs per-tick production behavior, it can override.

        foreach (var task in productionTasks)
        {
            // Resource consumption is now handled in SimulationEngine.ProcessTasks
            // when tasks complete, via IndustryContext.ConsumeProductionResources().
            // This system is now a lightweight pass-through.
        }
    }
}
