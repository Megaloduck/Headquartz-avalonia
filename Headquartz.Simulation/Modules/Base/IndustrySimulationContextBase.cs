using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Systems;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.Base;

/// <summary>
/// Abstract base that provides sensible defaults for all simulation hooks.
/// Industries override only what makes them unique.
/// </summary>
public abstract class IndustrySimulationContextBase : IIndustrySimulationContext
{
    public abstract IndustryType IndustryType { get; }
    public abstract string IndustryName { get; }
    public abstract string IndustryDescription { get; }
    public abstract string IndustryEmoji { get; }

    public abstract IndustryProfile GetProfile();
    public abstract IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances();

    public abstract IReadOnlyList<InventoryItem> GetInitialInventory();
    public abstract IReadOnlyList<Employee> GetInitialEmployees();

    public abstract string GetRandomClientName();
    public abstract string GetRandomProductName();

    public virtual int GetOrderQuantity() => Random.Shared.Next(10, 100);
    public virtual decimal GetOrderUnitPrice() => Random.Shared.Next(50, 150);
    public virtual int GetOrderDeadlineDays() => Random.Shared.Next(5, 14);

    public virtual string GetTaskName(DepartmentType dept) => dept switch
    {
        DepartmentType.HumanResources => "Employee Recruitment",
        DepartmentType.Finance => "Budget Review",
        DepartmentType.Sales => "Client Negotiation",
        DepartmentType.Marketing => "Marketing Campaign",
        DepartmentType.Production => "Production Batch",
        DepartmentType.Warehouse => "Inventory Sorting",
        DepartmentType.Logistics => "Delivery Coordination",
        _ => "General Task",
    };

    public virtual string GetTaskDescription(DepartmentType dept) =>
        $"Operational task for the {dept} department.";

    // ── Default Per-Tick Hooks ──

    public virtual void ProcessInventory(SimulationEngine engine)
    {
        foreach (var item in engine.Company.Inventory)
        {
            item.Quantity -= Random.Shared.Next(0, 5);
            item.Quantity = Math.Max(0, item.Quantity);

            if (item.Quantity <= item.MinimumStock)
                engine.Events.Publish(new InventoryLowEvent { Item = item });
        }
    }

    public virtual void ProcessOrders(SimulationEngine engine)
    {
        foreach (var order in engine.Company.Orders)
        {
            if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
                continue;

            order.Status = order.Status switch
            {
                OrderStatus.Pending => OrderStatus.Approved,
                OrderStatus.Approved => OrderStatus.InProduction,
                OrderStatus.InProduction => OrderStatus.ReadyForShipment,
                OrderStatus.ReadyForShipment => OrderStatus.Shipping,
                OrderStatus.Shipping => OrderStatus.Delivered,
                _ => order.Status,
            };

            if (order.Status == OrderStatus.Delivered)
                engine.Company.Reputation = Math.Min(100, engine.Company.Reputation + 1);
        }
    }

    public virtual void ProcessWarehouseRestock(SimulationEngine engine, InventoryItem item)
    {
        engine.Company.Cash -= item.UnitCost * 10;
        item.Quantity += 25;
    }

    public virtual void ConsumeProductionResources(SimulationEngine engine)
    {
        var first = engine.Company.Inventory.FirstOrDefault();
        if (first != null)
            first.Quantity -= 5;
    }

    // ── Event Flavor ──

    public virtual string GetEventTitle(DepartmentType dept) => dept switch
    {
        DepartmentType.HumanResources => "Employee Conflict",
        DepartmentType.Finance => "Budget Overrun",
        DepartmentType.Logistics => "Shipment Delay",
        DepartmentType.Marketing => "Campaign Failure",
        DepartmentType.Production => "Machine Breakdown",
        DepartmentType.Sales => "Client Complaint",
        DepartmentType.Warehouse => "Inventory Mismatch",
        _ => "Operational Issue",
    };

    public virtual string GetEventDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.HumanResources => "Employee morale is declining.",
        DepartmentType.Finance => "Unexpected expenses detected.",
        DepartmentType.Logistics => "Delivery routes disrupted.",
        DepartmentType.Marketing => "Ad performance dropped sharply.",
        DepartmentType.Production => "Production line halted temporarily.",
        DepartmentType.Sales => "Customer satisfaction decreased.",
        DepartmentType.Warehouse => "Stock count mismatch found.",
        _ => "General operational issue.",
    };

    // ── Flags ──
    public virtual bool HasPhysicalProducts => true;
    public virtual bool HasPerishableInventory => false;
}
