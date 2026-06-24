using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Modules.BeverageIndustry;

public static class BeverageData
{
    public static readonly string[] Clients =
    [
        "Beverage Bros",
        "RefreshMart",
        "ThirstyNation",
        "Bar & Grill Supply",
        "Corner Store Chain",
    ];

    public static readonly string[] Products =
    [
        "Sparkling Water",
        "Craft Beer",
        "Orange Juice",
        "Energy Drink",
        "Iced Tea",
        "Wine Bottle",
    ];

    public static InventoryItem[] InitialInventory =>
    [
        new() { Id = Guid.NewGuid(), Name = "Water",              Quantity = 1000, UnitCost = 0.10m,  MinimumStock = 200, MaximumStock = 2000 },
        new() { Id = Guid.NewGuid(), Name = "Sugar",              Quantity =  600, UnitCost = 0.80m,  MinimumStock = 100, MaximumStock = 1000 },
        new() { Id = Guid.NewGuid(), Name = "Fruit Concentrate",  Quantity =  300, UnitCost = 3.50m,  MinimumStock =  60, MaximumStock =  500 },
        new() { Id = Guid.NewGuid(), Name = "Hops",               Quantity =  200, UnitCost = 5.00m,  MinimumStock =  40, MaximumStock =  400 },
        new() { Id = Guid.NewGuid(), Name = "Barley",             Quantity =  400, UnitCost = 1.20m,  MinimumStock =  80, MaximumStock =  800 },
        new() { Id = Guid.NewGuid(), Name = "Bottles",            Quantity =  800, UnitCost = 0.30m,  MinimumStock = 150, MaximumStock = 1500 },
        new() { Id = Guid.NewGuid(), Name = "Labels",             Quantity = 1000, UnitCost = 0.05m,  MinimumStock = 200, MaximumStock = 2000 },
    ];

    public static Employee[] InitialEmployees =>
    [
        new() { Id = Guid.NewGuid(), Name = "Alice",  Role = EmployeeRole.Manager,    Department = DepartmentType.Production,     Salary = 5_500, Morale = 75, Productivity = 80 },
        new() { Id = Guid.NewGuid(), Name = "Bob",    Role = EmployeeRole.Worker,     Department = DepartmentType.Warehouse,      Salary = 2_600, Morale = 60, Productivity = 70 },
        new() { Id = Guid.NewGuid(), Name = "Carol",  Role = EmployeeRole.Supervisor, Department = DepartmentType.Production,     Salary = 3_800, Morale = 65, Productivity = 75 },
        new() { Id = Guid.NewGuid(), Name = "David",  Role = EmployeeRole.Worker,     Department = DepartmentType.Logistics,      Salary = 2_700, Morale = 55, Productivity = 68 },
        new() { Id = Guid.NewGuid(), Name = "Emma",   Role = EmployeeRole.Manager,    Department = DepartmentType.Sales,          Salary = 4_800, Morale = 80, Productivity = 82 },
        new() { Id = Guid.NewGuid(), Name = "Frank",  Role = EmployeeRole.Worker,     Department = DepartmentType.Sales,          Salary = 2_800, Morale = 70, Productivity = 72 },
        new() { Id = Guid.NewGuid(), Name = "Grace",  Role = EmployeeRole.Worker,     Department = DepartmentType.HumanResources, Salary = 2_700, Morale = 68, Productivity = 71 },
        new() { Id = Guid.NewGuid(), Name = "Henry",  Role = EmployeeRole.Worker,     Department = DepartmentType.Production,     Salary = 2_700, Morale = 60, Productivity = 65 },
    ];
}
