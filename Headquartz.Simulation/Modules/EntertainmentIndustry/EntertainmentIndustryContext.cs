using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Modules.Base;
using Headquartz.Simulation.Systems;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.EntertainmentIndustry;

public class EntertainmentIndustryContext : IndustrySimulationContextBase
{
    public override IndustryType IndustryType => IndustryType.Entertainment;
    public override string IndustryName => "Entertainment";
    public override string IndustryDescription => "Content creation, events, and digital experiences. Talent-heavy, project-based revenue, and hype-driven marketing. Production means content / event ops.";
    public override string IndustryEmoji => "🎬";

    public override bool HasPhysicalProducts => false;

    public override IndustryProfile GetProfile() => new()
    {
        StartingReputation = 65,
        CashMultiplier = 1.05m,
        EventFrequencyModifier = 1.2,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Marketing] = 15,
            [DepartmentType.Sales] = 10,
            [DepartmentType.Production] = -10,
            [DepartmentType.Warehouse] = -20,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Marketing] = 8_000,
            [DepartmentType.Sales] = 5_000,
            [DepartmentType.Production] = -5_000,
            [DepartmentType.Warehouse] = -5_000,
        },
        EmployeeCountMultiplier = 0.75,
    };

    public override IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances() => new Dictionary<DepartmentType, DepartmentRelevance>
    {
        [DepartmentType.Production] = DepartmentRelevance.Important,
        [DepartmentType.Warehouse] = DepartmentRelevance.Light,
        [DepartmentType.Logistics] = DepartmentRelevance.Important,
        [DepartmentType.Finance] = DepartmentRelevance.Standard,
        [DepartmentType.Sales] = DepartmentRelevance.Critical,
        [DepartmentType.Marketing] = DepartmentRelevance.Critical,
        [DepartmentType.HumanResources] = DepartmentRelevance.Important,
        [DepartmentType.Management] = DepartmentRelevance.Standard,
    };

    public override IReadOnlyList<InventoryItem> GetInitialInventory() => EntertainmentData.InitialInventory;
    public override IReadOnlyList<Employee> GetInitialEmployees() => EntertainmentData.InitialEmployees;

    public override string GetRandomClientName() => EntertainmentData.Clients[Random.Shared.Next(EntertainmentData.Clients.Length)];
    public override string GetRandomProductName() => EntertainmentData.Products[Random.Shared.Next(EntertainmentData.Products.Length)];

    public override int GetOrderQuantity() => Random.Shared.Next(100, 1000);
    public override decimal GetOrderUnitPrice() => Random.Shared.Next(10, 250);
    public override int GetOrderDeadlineDays() => Random.Shared.Next(1, 14);

    public override string GetTaskName(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Content Production",
        DepartmentType.Warehouse => "Props & Merch Prep",
        DepartmentType.Logistics => "Tour Routing",
        DepartmentType.Sales => "Licensing Deal",
        DepartmentType.Marketing => "Hype Campaign",
        DepartmentType.Finance => "Project Revenue Review",
        DepartmentType.HumanResources => "Talent Contract Negotiation",
        _ => base.GetTaskName(dept),
    };

    public override string GetTaskDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Produce a content piece or prepare an event stage.",
        DepartmentType.Warehouse => "Prep stage props and merch bundles for the next show.",
        DepartmentType.Logistics => "Route talent and equipment to the next venue.",
        DepartmentType.Sales => "Close a licensing or ticket distribution deal.",
        DepartmentType.Marketing => "Build hype through teasers, drops, and influencer seeding.",
        DepartmentType.Finance => "Track project revenue, royalties, and production costs.",
        DepartmentType.HumanResources => "Negotiate contracts with talent, crew, and agents.",
        _ => base.GetTaskDescription(dept),
    };

    // ── Entertainment orders skip physical shipping ──

    public override void ProcessOrders(SimulationEngine engine)
    {
        foreach (var order in engine.Company.Orders)
        {
            if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
                continue;

            // Digital / ticket orders: fast pipeline (no physical shipping)
            order.Status = order.Status switch
            {
                OrderStatus.Pending => OrderStatus.Approved,
                OrderStatus.Approved => OrderStatus.Delivered,   // Skip production & shipping
                _ => order.Status,
            };

            if (order.Status == OrderStatus.Delivered)
                engine.Company.Reputation = Math.Min(100, engine.Company.Reputation + 1);
        }
    }

    // ── Production consumes props and equipment ──

    public override void ConsumeProductionResources(SimulationEngine engine)
    {
        var props = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Stage Props");
        var sound = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Sound Equipment");
        var lighting = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Lighting Gear");

        if (props != null) props.Quantity -= 2;
        if (sound != null) sound.Quantity -= 1;
        if (lighting != null) lighting.Quantity -= 1;
    }

    // ── Inventory: equipment wears out, not depletes like consumables ──

    public override void ProcessInventory(SimulationEngine engine)
    {
        foreach (var item in engine.Company.Inventory)
        {
            // Equipment degrades slowly (1-3 units per tick = wear & tear)
            int depletion = Random.Shared.Next(1, 3);
            item.Quantity -= depletion;
            item.Quantity = Math.Max(0, item.Quantity);

            if (item.Quantity <= item.MinimumStock)
                engine.Events.Publish(new InventoryLowEvent { Item = item });
        }
    }

    // ── Events: entertainment-specific flavor ──

    public override string GetEventTitle(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Talent No-Show",
        DepartmentType.Warehouse => "Merch Stock Shortage",
        DepartmentType.Logistics => "Venue Cancellation",
        DepartmentType.Sales => "Streaming Platform Pulled Content",
        DepartmentType.Marketing => "Influencer Backlash",
        DepartmentType.Finance => "Royalty Payment Delayed",
        DepartmentType.HumanResources => "Union Crew Strike",
        _ => base.GetEventTitle(dept),
    };

    public override string GetEventDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "A lead talent cancelled rehearsal; reschedule immediately.",
        DepartmentType.Warehouse => "Merch inventory is too low for the upcoming tour dates.",
        DepartmentType.Logistics => "A venue double-booked; find an alternate location fast.",
        DepartmentType.Sales => "A streaming platform removed licensed content without notice.",
        DepartmentType.Marketing => "An influencer posted a negative review that went viral.",
        DepartmentType.Finance => "A royalty payment from a distributor is 30 days overdue.",
        DepartmentType.HumanResources => "Stage crew union called a walkout over working hours.",
        _ => base.GetEventDescription(dept),
    };
}
