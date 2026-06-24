using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Modules.EntertainmentIndustry;

public static class EntertainmentData
{
    public static readonly string[] Clients =
    [
        "Stadium Events Co",
        "Streaming Platform X",
        "Merch Distributors",
        "Talent Agency Partners",
        "Festival Organizers",
    ];

    public static readonly string[] Products =
    [
        "Concert Ticket",
        "Streaming License",
        "Merchandise Bundle",
        "VIP Experience",
        "Digital Album",
    ];

    public static InventoryItem[] InitialInventory =>
    [
        new() { Id = Guid.NewGuid(), Name = "Stage Props",       Quantity =  50, UnitCost = 200.00m,  MinimumStock = 10, MaximumStock = 100 },
        new() { Id = Guid.NewGuid(), Name = "Merch Inventory",   Quantity = 300, UnitCost =   8.00m,  MinimumStock = 50, MaximumStock = 500 },
        new() { Id = Guid.NewGuid(), Name = "Sound Equipment",   Quantity =  20, UnitCost =1500.00m,  MinimumStock =  5, MaximumStock =  40 },
        new() { Id = Guid.NewGuid(), Name = "Lighting Gear",     Quantity =  30, UnitCost = 800.00m,  MinimumStock =  5, MaximumStock =  50 },
    ];

    public static Employee[] InitialEmployees =>
    [
        new() { Id = Guid.NewGuid(), Name = "Alice",  Role = EmployeeRole.Manager,    Department = DepartmentType.Marketing,      Salary = 5_500, Morale = 80, Productivity = 82 },
        new() { Id = Guid.NewGuid(), Name = "Bob",    Role = EmployeeRole.Worker,     Department = DepartmentType.Sales,          Salary = 2_800, Morale = 65, Productivity = 70 },
        new() { Id = Guid.NewGuid(), Name = "Carol",  Role = EmployeeRole.Supervisor, Department = DepartmentType.Production,     Salary = 4_000, Morale = 70, Productivity = 75 },
        new() { Id = Guid.NewGuid(), Name = "David",  Role = EmployeeRole.Worker,     Department = DepartmentType.Logistics,      Salary = 2_700, Morale = 60, Productivity = 68 },
        new() { Id = Guid.NewGuid(), Name = "Emma",   Role = EmployeeRole.Manager,    Department = DepartmentType.Sales,          Salary = 5_000, Morale = 80, Productivity = 85 },
        new() { Id = Guid.NewGuid(), Name = "Frank",  Role = EmployeeRole.Worker,     Department = DepartmentType.Marketing,      Salary = 3_000, Morale = 70, Productivity = 72 },
        new() { Id = Guid.NewGuid(), Name = "Grace",  Role = EmployeeRole.Worker,     Department = DepartmentType.HumanResources, Salary = 2_700, Morale = 68, Productivity = 71 },
        new() { Id = Guid.NewGuid(), Name = "Henry",  Role = EmployeeRole.Worker,     Department = DepartmentType.Production,     Salary = 2_800, Morale = 60, Productivity = 65 },
    ];
}
