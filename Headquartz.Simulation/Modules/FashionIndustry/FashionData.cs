using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Modules.FashionIndustry;

public static class FashionData
{
    public static readonly string[] Clients =
    [
        "Boutique Chain A",
        "Department Store Group",
        "Online Fashion Retailer",
        "Wholesale Distributor",
        "Pop-Up Event Partners",
    ];

    public static readonly string[] Products =
    [
        "Summer Dress",
        "Denim Jacket",
        "Athletic Sneakers",
        "Designer Handbag",
        "Graphic T-Shirt",
    ];

    public static InventoryItem[] InitialInventory =>
    [
        new() { Id = Guid.NewGuid(), Name = "Cotton Fabric",     Quantity = 500, UnitCost =  5.00m,  MinimumStock = 100, MaximumStock = 1000 },
        new() { Id = Guid.NewGuid(), Name = "Denim Rolls",       Quantity = 300, UnitCost =  8.00m,  MinimumStock =  60, MaximumStock =  600 },
        new() { Id = Guid.NewGuid(), Name = "Leather Hides",     Quantity = 100, UnitCost = 45.00m,  MinimumStock =  20, MaximumStock =  200 },
        new() { Id = Guid.NewGuid(), Name = "Sole Units",        Quantity = 250, UnitCost = 12.00m,  MinimumStock =  50, MaximumStock =  500 },
        new() { Id = Guid.NewGuid(), Name = "Zippers & Buttons", Quantity =2000, UnitCost =  0.20m,  MinimumStock = 400, MaximumStock = 4000 },
    ];

    public static Employee[] InitialEmployees =>
    [
        new() { Id = Guid.NewGuid(), Name = "Alice",  Role = EmployeeRole.Manager,    Department = DepartmentType.Marketing,      Salary = 5_200, Morale = 80, Productivity = 82 },
        new() { Id = Guid.NewGuid(), Name = "Bob",    Role = EmployeeRole.Worker,     Department = DepartmentType.Warehouse,      Salary = 2_600, Morale = 60, Productivity = 70 },
        new() { Id = Guid.NewGuid(), Name = "Carol",  Role = EmployeeRole.Supervisor, Department = DepartmentType.Production,     Salary = 3_800, Morale = 65, Productivity = 75 },
        new() { Id = Guid.NewGuid(), Name = "David",  Role = EmployeeRole.Worker,     Department = DepartmentType.Logistics,      Salary = 2_700, Morale = 55, Productivity = 68 },
        new() { Id = Guid.NewGuid(), Name = "Emma",   Role = EmployeeRole.Manager,    Department = DepartmentType.Sales,          Salary = 4_800, Morale = 80, Productivity = 82 },
        new() { Id = Guid.NewGuid(), Name = "Frank",  Role = EmployeeRole.Worker,     Department = DepartmentType.Sales,          Salary = 2_800, Morale = 70, Productivity = 72 },
        new() { Id = Guid.NewGuid(), Name = "Grace",  Role = EmployeeRole.Worker,     Department = DepartmentType.HumanResources, Salary = 2_700, Morale = 68, Productivity = 71 },
        new() { Id = Guid.NewGuid(), Name = "Henry",  Role = EmployeeRole.Worker,     Department = DepartmentType.Production,     Salary = 2_700, Morale = 60, Productivity = 65 },
    ];
}
