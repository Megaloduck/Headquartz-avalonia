using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Modules.Base;
using Headquartz.Simulation.Systems;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.FoodIndustry;

/// <summary>
/// Food industry simulation context.
/// Key differences: perishable inventory (spoilage), cold-chain logistics,
/// short deadlines, high volume / low margin orders, ingredient-based production.
/// </summary>
public class FoodIndustryContext : IndustrySimulationContextBase
{
    public override IndustryType IndustryType => IndustryType.Food;
    public override string IndustryName => "Food";
    public override string IndustryDescription => "Perishable goods with spoilage risk. FIFO inventory and cold-chain logistics are critical. Tight margins driven by shelf-life pressure.";
    public override string IndustryEmoji => "🍞";

    public override bool HasPerishableInventory => true;

    public override IndustryProfile GetProfile() => new()
    {
        StartingReputation = 55,
        CashMultiplier = 0.95m,
        EventFrequencyModifier = 1.05,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Production] = 10,
            [DepartmentType.Warehouse] = 10,
            [DepartmentType.Logistics] = 5,
            [DepartmentType.Marketing] = -5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Production] = 5_000,
            [DepartmentType.Warehouse] = 5_000,
        },
        EmployeeCountMultiplier = 1.1,
    };

    public override IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances() => new Dictionary<DepartmentType, DepartmentRelevance>
    {
        [DepartmentType.Production] = DepartmentRelevance.Critical,
        [DepartmentType.Warehouse] = DepartmentRelevance.Critical,
        [DepartmentType.Logistics] = DepartmentRelevance.Important,
        [DepartmentType.Finance] = DepartmentRelevance.Standard,
        [DepartmentType.Sales] = DepartmentRelevance.Important,
        [DepartmentType.Marketing] = DepartmentRelevance.Standard,
        [DepartmentType.HumanResources] = DepartmentRelevance.Standard,
        [DepartmentType.Management] = DepartmentRelevance.Standard,
    };

    public override IReadOnlyList<InventoryItem> GetInitialInventory() => FoodData.InitialInventory;
    public override IReadOnlyList<Employee> GetInitialEmployees() => FoodData.InitialEmployees;

    public override string GetRandomClientName() => FoodData.Clients[Random.Shared.Next(FoodData.Clients.Length)];
    public override string GetRandomProductName() => FoodData.Products[Random.Shared.Next(FoodData.Products.Length)];

    public override int GetOrderQuantity() => Random.Shared.Next(50, 200);
    public override decimal GetOrderUnitPrice() => Random.Shared.Next(2, 10);
    public override int GetOrderDeadlineDays() => Random.Shared.Next(2, 5);

    public override string GetTaskName(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Batch Cooking",
        DepartmentType.Warehouse => "Cold Storage Sort",
        DepartmentType.Logistics => "Cold Chain Delivery",
        DepartmentType.Sales => "Retail Contract",
        DepartmentType.Marketing => "Freshness Campaign",
        DepartmentType.Finance => "Margin Analysis",
        DepartmentType.HumanResources => "Kitchen Staff Hiring",
        _ => base.GetTaskName(dept),
    };

    public override string GetTaskDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Prepare a production batch with strict hygiene protocols.",
        DepartmentType.Warehouse => "Organize FIFO rotation and check cold storage temperatures.",
        DepartmentType.Logistics => "Coordinate refrigerated transport for perishable goods.",
        DepartmentType.Sales => "Negotiate supply contracts with grocery chains.",
        DepartmentType.Marketing => "Run a freshness-focused campaign highlighting local sourcing.",
        DepartmentType.Finance => "Review perishable write-offs and adjust margin targets.",
        DepartmentType.HumanResources => "Recruit and certify food-handling staff.",
        _ => base.GetTaskDescription(dept),
    };

    // ── Spoilage: inventory depletes faster and some items rot ──

    public override void ProcessInventory(SimulationEngine engine)
    {
        foreach (var item in engine.Company.Inventory)
        {
            // Perishable items deplete 2-8 units per tick (spoilage)
            int depletion = Random.Shared.Next(2, 8);
            item.Quantity -= depletion;
            item.Quantity = Math.Max(0, item.Quantity);

            if (item.Quantity <= item.MinimumStock)
                engine.Events.Publish(new InventoryLowEvent { Item = item });
        }
    }

    // ── Production consumes ingredients, not just generic "first item" ──

    public override void ConsumeProductionResources(SimulationEngine engine)
    {
        // A food production batch consumes flour, yeast, and packaging
        var flour = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Flour");
        var yeast = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Yeast");
        var packaging = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Packaging");

        if (flour != null) flour.Quantity -= 10;
        if (yeast != null) yeast.Quantity -= 2;
        if (packaging != null) packaging.Quantity -= 5;
    }

    // ── Warehouse: cold storage restock costs more ──

    public override void ProcessWarehouseRestock(SimulationEngine engine, InventoryItem item)
    {
        // Cold storage items cost 20% more to restock
        decimal premium = item.Name is "Milk" or "Chicken" or "Vegetables" or "Cheese" ? 1.2m : 1.0m;
        engine.Company.Cash -= item.UnitCost * 10 * premium;
        item.Quantity += 25;
    }

    // ── Events: food-specific flavor ──

    public override string GetEventTitle(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Health Inspection Alert",
        DepartmentType.Warehouse => "Cold Storage Failure",
        DepartmentType.Logistics => "Delivery Truck Breakdown",
        DepartmentType.Sales => "Major Retailer Contract Loss",
        DepartmentType.Marketing => "Food Safety Recall Campaign",
        DepartmentType.Finance => "Spoilage Write-Off Spike",
        DepartmentType.HumanResources => "Staff Food-Poisoning Incident",
        _ => base.GetEventTitle(dept),
    };

    public override string GetEventDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "A health inspector flagged sanitation protocols.",
        DepartmentType.Warehouse => "Refrigeration unit failed; check spoilage immediately.",
        DepartmentType.Logistics => "Refrigerated truck broke down; reroute deliveries.",
        DepartmentType.Sales => "A major grocery chain paused orders pending audit.",
        DepartmentType.Marketing => "A social media post questioned product freshness.",
        DepartmentType.Finance => "Perishable write-offs exceeded the quarterly budget.",
        DepartmentType.HumanResources => "Multiple staff called in sick; production may stall.",
        _ => base.GetEventDescription(dept),
    };
}
