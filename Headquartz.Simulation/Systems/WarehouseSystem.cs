using System;
using System.Collections.Generic;
using System.Text;

namespace Headquartz.Simulation.Systems;

public class WarehouseSystem
    : ISimulationSystem
{
    public void Update(
        SimulationEngine engine)
    {
        foreach (var item
                 in engine.Company.Inventory)
        {
            if (item.Quantity <
                item.MinimumStock)
            {
                engine.Company.Cash -=
                    item.UnitCost * 10;

                item.Quantity += 25;
            }
        }
    }
}
