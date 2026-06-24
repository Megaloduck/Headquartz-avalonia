using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Modules.Base;
using Headquartz.Simulation.Systems;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.BeverageIndustry;

public class BeverageIndustryContext : IndustrySimulationContextBase
{
    public override IndustryType IndustryType => IndustryType.Beverage;
    public override string IndustryName => "Beverage";
    public override string IndustryDescription => "Blending, bottling, and distribution. Excise taxes, seasonal demand, and distributor relationships define success.";
    public override string IndustryEmoji => "🥤";

    public override IndustryProfile GetProfile() => new()
    {
        StartingReputation = 50,
        CashMultiplier = 1.0m,
        EventFrequencyModifier = 1.0,
        DepartmentEfficiencyDelta =
        {
            [DepartmentType.Production] = 10,
            [DepartmentType.Warehouse] = 5,
            [DepartmentType.Logistics] = 10,
            [DepartmentType.Sales] = 5,
            [DepartmentType.Marketing] = 5,
        },
        DepartmentBudgetDelta =
        {
            [DepartmentType.Production] = 4_000,
            [DepartmentType.Warehouse] = 3_000,
        },
        EmployeeCountMultiplier = 1.0,
    };

    public override IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances() => new Dictionary<DepartmentType, DepartmentRelevance>
    {
        [DepartmentType.Production] = DepartmentRelevance.Critical,
        [DepartmentType.Warehouse] = DepartmentRelevance.Important,
        [DepartmentType.Logistics] = DepartmentRelevance.Critical,
        [DepartmentType.Finance] = DepartmentRelevance.Important,
        [DepartmentType.Sales] = DepartmentRelevance.Critical,
        [DepartmentType.Marketing] = DepartmentRelevance.Important,
        [DepartmentType.HumanResources] = DepartmentRelevance.Standard,
        [DepartmentType.Management] = DepartmentRelevance.Standard,
    };

    public override IReadOnlyList<InventoryItem> GetInitialInventory() => BeverageData.InitialInventory;
    public override IReadOnlyList<Employee> GetInitialEmployees() => BeverageData.InitialEmployees;

    public override string GetRandomClientName() => BeverageData.Clients[Random.Shared.Next(BeverageData.Clients.Length)];
    public override string GetRandomProductName() => BeverageData.Products[Random.Shared.Next(BeverageData.Products.Length)];

    public override int GetOrderQuantity() => Random.Shared.Next(80, 300);
    public override decimal GetOrderUnitPrice() => Random.Shared.Next(2, 15);
    public override int GetOrderDeadlineDays() => Random.Shared.Next(3, 7);

    public override string GetTaskName(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Blending & Bottling",
        DepartmentType.Warehouse => "Barrel & Bottle Storage",
        DepartmentType.Logistics => "Distributor Delivery",
        DepartmentType.Sales => "Distributor Contract",
        DepartmentType.Marketing => "Brand Campaign",
        DepartmentType.Finance => "Excise Tax Review",
        DepartmentType.HumanResources => "Brewery Staff Hiring",
        _ => base.GetTaskName(dept),
    };

    public override string GetTaskDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Run a blending and bottling batch with quality checks.",
        DepartmentType.Warehouse => "Manage barrel aging and bottle inventory rotation.",
        DepartmentType.Logistics => "Coordinate pallet deliveries to regional distributors.",
        DepartmentType.Sales => "Negotiate shelf-space and distributor contracts.",
        DepartmentType.Marketing => "Launch a lifestyle brand campaign targeting demographics.",
        DepartmentType.Finance => "Review excise tax liabilities and seasonal cash flow.",
        DepartmentType.HumanResources => "Hire certified brewers and bottling-line operators.",
        _ => base.GetTaskDescription(dept),
    };

    public override void ConsumeProductionResources(SimulationEngine engine)
    {
        var water = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Water");
        var sugar = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Sugar");
        var bottles = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Bottles");
        var labels = engine.Company.Inventory.FirstOrDefault(i => i.Name == "Labels");

        if (water != null) water.Quantity -= 50;
        if (sugar != null) sugar.Quantity -= 5;
        if (bottles != null) bottles.Quantity -= 10;
        if (labels != null) labels.Quantity -= 10;
    }

    public override string GetEventTitle(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "Batch Contamination",
        DepartmentType.Warehouse => "Barrel Leak Detected",
        DepartmentType.Logistics => "Distributor Delay",
        DepartmentType.Sales => "Distributor Contract Cancelled",
        DepartmentType.Marketing => "Brand Reputation Hit",
        DepartmentType.Finance => "Excise Tax Audit",
        DepartmentType.HumanResources => "Brewer Certification Expired",
        _ => base.GetEventTitle(dept),
    };

    public override string GetEventDescription(DepartmentType dept) => dept switch
    {
        DepartmentType.Production => "A batch failed quality control; dispose immediately.",
        DepartmentType.Warehouse => "A barrel seal failed; product may be compromised.",
        DepartmentType.Logistics => "A distributor truck was delayed; reroute pallets.",
        DepartmentType.Sales => "A major distributor paused orders after a quality complaint.",
        DepartmentType.Marketing => "A viral post criticized the brand's sustainability claims.",
        DepartmentType.Finance => "Tax authorities flagged an excise tax reporting error.",
        DepartmentType.HumanResources => "Brewer certifications expired; production may halt.",
        _ => base.GetEventDescription(dept),
    };
}
