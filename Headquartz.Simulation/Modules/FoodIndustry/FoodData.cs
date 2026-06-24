using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Modules.FoodIndustry;

public static class FoodData
{
    public static readonly string[] Clients =
    [
        "FreshMart Grocery",
        "GreenLeaf Supermarket",
        "CityFood Distributors",
        "FarmToTable Restaurants",
        "Daily Eats Catering",
    ];

    public static readonly string[] Products =
    [
        "Fresh Bread",
        "Organic Milk",
        "Chicken Breast",
        "Leafy Greens",
        "Tomato Sauce",
        "Frozen Pizza",
    ];

    public static InventoryItem[] InitialInventory =>
    [
        new() { Id = Guid.NewGuid(), Name = "Flour",       Quantity = 800, UnitCost = 2.00m,  MinimumStock = 150, MaximumStock = 1200 },
        new() { Id = Guid.NewGuid(), Name = "Yeast",       Quantity = 200, UnitCost = 1.50m,  MinimumStock =  40, MaximumStock =  400 },
        new() { Id = Guid.NewGuid(), Name = "Milk",        Quantity = 400, UnitCost = 2.80m,  MinimumStock =  80, MaximumStock =  600 },
        new() { Id = Guid.NewGuid(), Name = "Chicken",     Quantity = 250, UnitCost = 5.00m,  MinimumStock =  50, MaximumStock =  400 },
        new() { Id = Guid.NewGuid(), Name = "Vegetables",  Quantity = 300, UnitCost = 2.00m,  MinimumStock =  60, MaximumStock =  500 },
        new() { Id = Guid.NewGuid(), Name = "Cheese",      Quantity = 150, UnitCost = 4.50m,  MinimumStock =  30, MaximumStock =  300 },
        new() { Id = Guid.NewGuid(), Name = "Packaging",   Quantity = 500, UnitCost = 0.50m,  MinimumStock = 100, MaximumStock = 1000 },
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
