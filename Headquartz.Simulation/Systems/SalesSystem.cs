using System;
using System.Linq;

using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;

namespace Headquartz.Simulation.Systems;

public class SalesSystem : ISimulationSystem
{
    private const int ReputationPenaltyPerFailedOrder = 3;
    private const int RevenuePerDeliveredOrder = 100;

    public void Update(SimulationEngine engine)
    {
        ProcessDeliveredRevenue(engine);
        CancelOverdueOrders(engine);
    }

    // =========================================================
    // REVENUE FROM DELIVERED ORDERS
    // =========================================================

    private static void ProcessDeliveredRevenue(
        SimulationEngine engine)
    {
        int deliveredCount = engine.Company.Orders
            .Count(o => o.Status == OrderStatus.Delivered);

        engine.Company.Revenue +=
            deliveredCount * RevenuePerDeliveredOrder;
    }

    // =========================================================
    // OVERDUE ORDER CANCELLATIONS
    // =========================================================

    private static void CancelOverdueOrders(
        SimulationEngine engine)
    {
        var overdue = engine.Company.Orders
            .Where(o =>
                o.DeliveryDeadline.HasValue &&
                engine.Clock.WorldTime > o.DeliveryDeadline.Value &&
                o.Status != OrderStatus.Delivered &&
                o.Status != OrderStatus.Cancelled)
            .ToList();

        foreach (var order in overdue)
        {
            order.Status = OrderStatus.Cancelled;

            engine.Company.Reputation =
                Math.Max(0,
                    engine.Company.Reputation -
                    ReputationPenaltyPerFailedOrder);

            engine.Events.Publish(
                new OrderFailedEvent { Order = order });
        }
    }
}   