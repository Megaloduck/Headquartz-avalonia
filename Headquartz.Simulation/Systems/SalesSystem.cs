using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Systems;

public class SalesSystem
    : ISimulationSystem
{
    public void Update(
        SimulationEngine engine)
    {
        var completedOrders =
            engine.Company.Orders
                .Count(o =>
                    o.Status ==
                    OrderStatus.Delivered);

        engine.Company.Revenue +=
            completedOrders * 100;
    }
}