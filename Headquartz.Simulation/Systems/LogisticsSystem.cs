using System;
using System.Collections.Generic;
using System.Text;

using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Systems;

public class LogisticsSystem
    : ISimulationSystem
{
    public void Update(
        SimulationEngine engine)
    {
        foreach (var order
                 in engine.Company.Orders)
        {
            if (order.Status ==
                OrderStatus.ReadyForShipment)
            {
                order.Status =
                    OrderStatus.Shipping;
            }
        }
    }
}