using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Modules.Base;
using Headquartz.Simulation.Systems;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.FashionIndustry;

public class FashionIndustryContext : IndustrySimulationContextBase
{
    public override IndustryType IndustryType => IndustryType.Fashion;
    public override string IndustryName => "Fashion";
    public override string IndustryDescription => "Trend-driven, seasonal cycles, and high markdown risk. Fast fashion requires rapid production turnaround; luxury rewards brand and scarcity.";
    public override string IndustryEmoji => "👗";

    public override IndustryProfile GetProfile() => new()
    {
        StartingReputation = 55,
        CashMultiplier = 0.9m,
        EventFrequencyModifier = 1.1,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Marketing] = 10,
            [DepartmentType.Sales] = 10,
            [DepartmentType.Production] = 5,
            [DepartmentType.Warehouse] = 5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Marketing] = 5_000,
            [DepartmentType.Sales] = 3_000,
            [DepartmentType.Production] = 3_000,
        },
        EmployeeCountMultiplier = 1.0,
    };

    public override IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances() => new Dictionary<DepartmentType, DepartmentRelevance>
    {
        [DepartmentType.Production] = DepartmentRelevance.Important,
        [DepartmentType.Warehouse] = DepartmentRelevance.Important,
        [DepartmentType.Logistics] = DepartmentRelevance.Important,
        [DepartmentType.Finance] = DepartmentRelevance.Standard,
        [DepartmentType.Sales] = DepartmentRelevance.Critical,
        [DepartmentType.Marketing] = DepartmentRelevance.Critical,
        [DepartmentType.HumanResources] = DepartmentRelevance.Standard,
        [DepartmentType.Management] = DepartmentRelevance.Standard,
    };

    public override IReadOnlyList<InventoryItem> GetInitialInventory() => FashionData.InitialInventory;
    public override IReadOnlyList<Employee> GetInitialEmployees() => FashionData.InitialEmployees;

    public override string GetRandomClientName() => FashionData.Clients[Random.Shared.Next(FashionData.Clients.Length)];
    public override string GetRandomProductName() => FashionData.Products[Random.Shared.Next(FashionData.Products.Length)];

    public override int GetOrderQuantity() => Random.Shared.Next(50, 300);
    public override decimal GetOrderUnitPrice() => Random.Shared.Next(35, 350);
    public override int GetOrderDeadlineDays() => Random.Shared.Next(7, 21);

    public override string GetTaskName(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Cut & Sew Run",
        DepartmentType.Warehouse => "Style/Size Matrix Sort",
        DepartmentType.Logistics => "Fast Fashion Drop Delivery",
        DepartmentType.Sales => "Wholesale / DTC Campaign",
        DepartmentType.Marketing => "Trend & Runway Campaign",
        DepartmentType.Finance => "Markdown & Cash Flow Review",
        DepartmentType.HumanResources => "Seasonal Labor Hiring",
        _ => base.GetTaskName(dept),
    };

    public override string GetTaskDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Run a cut-and-sew batch for the latest trend.",
        DepartmentType.Warehouse => "Sort inventory by style, size, and color; handle returns.",
        DepartmentType.Logistics => "Coordinate express delivery for a fast-fashion drop.",
        DepartmentType.Sales => "Pitch wholesale buyers or launch a DTC flash sale.",
        DepartmentType.Marketing => "Build hype around runway trends and influencer seeding.",
        DepartmentType.Finance => "Plan markdown cycles and review seasonal cash flow.",
        DepartmentType.HumanResources => "Hire seasonal workers for peak production months.",
        _ => base.GetTaskDescription(dept),
    };

    // ── Fashion: consume fabric and accessories ──

    public override void ConsumeProductionResources(SimulationEngine engine)
    {
        var cotton = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Cotton Fabric");
        var denim = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Denim Rolls");
        var leather = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Leather Hides");
        var soles = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Sole Units");
        var zippers = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Zippers & Buttons");

        if (cotton != null) cotton.Quantity -= 15;
        if (denim != null) denim.Quantity -= 5;
        if (leather != null) leather.Quantity -= 2;
        if (soles != null) soles.Quantity -= 3;
        if (zippers != null) zippers.Quantity -= 20;
    }

    // ── Fashion: inventory depletes at moderate pace (trend turnover) ──

    public override void ProcessInventory(SimulationEngine engine)
    {
        foreach (var item in engine.Company.Inventory)
        {
            int depletion = Random.Shared.Next(1, 5);
            item.Quantity -= depletion;
            item.Quantity = Math.Max(0, item.Quantity);

            if (item.Quantity <= item.MinimumStock)
                engine.Events.Publish(new InventoryLowEvent { Item = item });
        }
    }

    // ── Fashion events ──

    public override string GetEventTitle(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Trend Shift",
        DepartmentType.Warehouse => "Returns Avalanche",
        DepartmentType.Logistics => "Drop Delay",
        DepartmentType.Sales => "Wholesale Buyer Cancelled",
        DepartmentType.Marketing => "Influencer Backlash",
        DepartmentType.Finance => "Markdown Cycle Loss",
        DepartmentType.HumanResources => "Seasonal Labor Shortage",
        _ => base.GetEventTitle(dept),
    };

    public override string GetEventDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "A competing brand launched a hit; pivot designs immediately.",
        DepartmentType.Warehouse => "Return volume spiked post-sale; warehouse is overwhelmed.",
        DepartmentType.Logistics => "An express drop shipment missed the trend window.",
        DepartmentType.Sales => "A wholesale buyer cancelled the season order last-minute.",
        DepartmentType.Marketing => "An influencer partnership backfired publicly.",
        DepartmentType.Finance => "Markdowns exceeded the planned budget; margins are squeezed.",
        DepartmentType.HumanResources => "Peak season arrived but seasonal labor is 40% short.",
        _ => base.GetEventDescription(dept),
    };
}
