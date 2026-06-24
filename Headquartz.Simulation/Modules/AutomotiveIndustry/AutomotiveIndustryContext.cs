using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Events;
using Headquartz.Simulation.Modules.Base;
using Headquartz.Simulation.Systems;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.AutomotiveIndustry;

public class AutomotiveIndustryContext : IndustrySimulationContextBase
{
    public override IndustryType IndustryType => IndustryType.Automotive;
    public override string IndustryName => "Automotive";
    public override string IndustryDescription => "Complex assembly with JIT supply chains. Quality control, precision production, and dealer network logistics dominate. Capital-intensive with long sales cycles.";
    public override string IndustryEmoji => "🚗";

    public override IndustryProfile GetProfile() => new()
    {
        StartingReputation = 55,
        CashMultiplier = 1.1m,
        EventFrequencyModifier = 0.9,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Production] = 15,
            [DepartmentType.Warehouse] = 5,
            [DepartmentType.Logistics] = 10,
            [DepartmentType.Sales] = 5,
            [DepartmentType.Marketing] = -5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Production] = 10_000,
            [DepartmentType.Warehouse] = 5_000,
            [DepartmentType.Logistics] = 5_000,
        },
        EmployeeCountMultiplier = 1.2,
    };

    public override IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances() => new Dictionary<DepartmentType, DepartmentRelevance>
    {
        [DepartmentType.Production] = DepartmentRelevance.Critical,
        [DepartmentType.Warehouse] = DepartmentRelevance.Important,
        [DepartmentType.Logistics] = DepartmentRelevance.Critical,
        [DepartmentType.Finance] = DepartmentRelevance.Important,
        [DepartmentType.Sales] = DepartmentRelevance.Critical,
        [DepartmentType.Marketing] = DepartmentRelevance.Standard,
        [DepartmentType.HumanResources] = DepartmentRelevance.Important,
        [DepartmentType.Management] = DepartmentRelevance.Standard,
    };

    public override IReadOnlyList<InventoryItem> GetInitialInventory() => AutomotiveData.InitialInventory;
    public override IReadOnlyList<Employee> GetInitialEmployees() => AutomotiveData.InitialEmployees;

    public override string GetRandomClientName() => AutomotiveData.Clients[Random.Shared.Next(AutomotiveData.Clients.Length)];
    public override string GetRandomProductName() => AutomotiveData.Products[Random.Shared.Next(AutomotiveData.Products.Length)];

    public override int GetOrderQuantity() => Random.Shared.Next(1, 5);
    public override decimal GetOrderUnitPrice() => Random.Shared.Next(250, 40000);
    public override int GetOrderDeadlineDays() => Random.Shared.Next(30, 60);

    public override string GetTaskName(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Assembly Line Run",
        DepartmentType.Warehouse => "Parts Inventory Audit",
        DepartmentType.Logistics => "JIT Parts Delivery",
        DepartmentType.Sales => "Dealership Contract",
        DepartmentType.Marketing => "Brand Safety Campaign",
        DepartmentType.Finance => "Capex & Warranty Review",
        DepartmentType.HumanResources => "Skilled Technician Hiring",
        _ => base.GetTaskName(dept),
    };

    public override string GetTaskDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Run an assembly line shift with quality gate checks.",
        DepartmentType.Warehouse => "Audit parts inventory and organize rack systems.",
        DepartmentType.Logistics => "Coordinate just-in-time delivery of critical components.",
        DepartmentType.Sales => "Negotiate fleet and dealership sales contracts.",
        DepartmentType.Marketing => "Launch a safety and reliability-focused campaign.",
        DepartmentType.Finance => "Review capital expenditure, warranty reserves, and margins.",
        DepartmentType.HumanResources => "Recruit and certify skilled assembly technicians.",
        _ => base.GetTaskDescription(dept),
    };

    // ── Automotive: consume multiple components per assembly ──

    public override void ConsumeProductionResources(SimulationEngine engine)
    {
        var steel = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Steel Frames");
        var tires = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Tire Sets");
        var engines = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Engine Components");
        var electronics = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Electronics");
        var paint = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Paint & Coating");

        if (steel != null) steel.Quantity -= 1;
        if (tires != null) tires.Quantity -= 1;
        if (engines != null) engines.Quantity -= 1;
        if (electronics != null) electronics.Quantity -= 2;
        if (paint != null) paint.Quantity -= 3;
    }

    // ── Automotive: slow inventory depletion (parts don't spoil) ──

    public override void ProcessInventory(SimulationEngine engine)
    {
        foreach (var item in engine.Company.Inventory)
        {
            // Parts are consumed slowly; 0-2 per tick (assembly consumption)
            int depletion = Random.Shared.Next(0, 2);
            item.Quantity -= depletion;
            item.Quantity = Math.Max(0, item.Quantity);

            if (item.Quantity <= item.MinimumStock)
                engine.Events.Publish(new InventoryLowEvent { Item = item });
        }
    }

    // ── Automotive events ──

    public override string GetEventTitle(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Chip Shortage",
        DepartmentType.Warehouse => "Parts Recall",
        DepartmentType.Logistics => "Supplier Delay",
        DepartmentType.Sales => "Dealership Default",
        DepartmentType.Marketing => "Safety Scandal",
        DepartmentType.Finance => "Warranty Reserve Shortfall",
        DepartmentType.HumanResources => "Union Strike",
        _ => base.GetEventTitle(dept),
    };

    public override string GetEventDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "A critical semiconductor chip is on backorder; assembly stalls.",
        DepartmentType.Warehouse => "A supplier recalled a defective parts batch; quarantine immediately.",
        DepartmentType.Logistics => "A key supplier missed the JIT delivery window; reschedule.",
        DepartmentType.Sales => "A dealership group defaulted on payment terms.",
        DepartmentType.Marketing => "A media outlet published a safety investigation; counter-narrative needed.",
        DepartmentType.Finance => "Warranty claims exceeded reserves; set aside emergency funds.",
        DepartmentType.HumanResources => "Assembly line union called a strike over shift schedules.",
        _ => base.GetEventDescription(dept),
    };
}
