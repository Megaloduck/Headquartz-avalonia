using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

namespace Headquartz.Simulation.Modules.AutomotiveIndustry;

public static class AutomotiveData
{
    public static readonly string[] Clients =
    [
        "Metro Dealership Group",
        "Fleet Solutions Inc",
        "Auto Parts Retailers",
        "Rental Car Network",
        "Export Trading Co",
    ];

    public static readonly string[] Products =
    [
        "Compact Car",
        "SUV Model X",
        "EV Battery Pack",
        "Brake Assembly",
        "Infotainment System",
    ];

    public static InventoryItem[] InitialInventory =>
    [
        new() { Id = Guid.NewGuid(), Name = "Steel Frames",       Quantity = 150, UnitCost = 800.00m,  MinimumStock =  30, MaximumStock =  300 },
        new() { Id = Guid.NewGuid(), Name = "Tire Sets",          Quantity = 200, UnitCost = 350.00m,  MinimumStock =  40, MaximumStock =  400 },
        new() { Id = Guid.NewGuid(), Name = "Engine Components",  Quantity =  80, UnitCost =2500.00m,  MinimumStock =  15, MaximumStock =  150 },
        new() { Id = Guid.NewGuid(), Name = "Electronics",        Quantity = 120, UnitCost = 400.00m,  MinimumStock =  25, MaximumStock =  250 },
        new() { Id = Guid.NewGuid(), Name = "Paint & Coating",    Quantity = 300, UnitCost =  50.00m,  MinimumStock =  60, MaximumStock =  600 },
    ];

    public static Employee[] InitialEmployees =>
    [
        new() { Id = Guid.NewGuid(), Name = "Alice",  Role = EmployeeRole.Manager,    Department = DepartmentType.Production,     Salary = 6_000, Morale = 75, Productivity = 80 },
        new() { Id = Guid.NewGuid(), Name = "Bob",    Role = EmployeeRole.Worker,     Department = DepartmentType.Warehouse,      Salary = 2_800, Morale = 60, Productivity = 70 },
        new() { Id = Guid.NewGuid(), Name = "Carol",  Role = EmployeeRole.Supervisor, Department = DepartmentType.Production,     Salary = 4_200, Morale = 65, Productivity = 75 },
        new() { Id = Guid.NewGuid(), Name = "David",  Role = EmployeeRole.Worker,     Department = DepartmentType.Logistics,      Salary = 2_800, Morale = 55, Productivity = 68 },
        new() { Id = Guid.NewGuid(), Name = "Emma",   Role = EmployeeRole.Manager,    Department = DepartmentType.Sales,          Salary = 5_200, Morale = 80, Productivity = 82 },
        new() { Id = Guid.NewGuid(), Name = "Frank",  Role = EmployeeRole.Worker,     Department = DepartmentType.Sales,          Salary = 3_000, Morale = 70, Productivity = 72 },
        new() { Id = Guid.NewGuid(), Name = "Grace",  Role = EmployeeRole.Worker,     Department = DepartmentType.HumanResources, Salary = 2_800, Morale = 68, Productivity = 71 },
        new() { Id = Guid.NewGuid(), Name = "Henry",  Role = EmployeeRole.Worker,     Department = DepartmentType.Production,     Salary = 2_800, Morale = 60, Productivity = 65 },
    ];
}
