using Headquartz.Simulation.Modules.Base;

namespace Headquartz.Simulation.Systems;

public class WarehouseSystem
    : ISimulationSystem
{
    public void Update(SimulationEngine engine)
    {
        var context = engine.IndustryContext;

        foreach (var item in engine.Company.Inventory)
        {
            if (item.Quantity < item.MinimumStock)
            {
                if (context != null)
                    context.ProcessWarehouseRestock(engine, item);
                else
                {
                    // Absolute fallback
                    engine.Company.Cash -= item.UnitCost * 10;
                    item.Quantity += 25;
                }
            }
        }
    }
}
