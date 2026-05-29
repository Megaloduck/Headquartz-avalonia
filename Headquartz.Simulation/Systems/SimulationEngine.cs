using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

using Headquartz.Simulation.Events;
using Headquartz.Simulation.Ticks;
using Headquartz.Simulation.Commands;

using Headquartz.Shared.Networking;

namespace Headquartz.Simulation.Systems;

public class SimulationEngine
{
    public Company Company { get; }
    public SimulationClock Clock { get; }
    public EventBus Events { get; }
    public CommandProcessor Commands { get; }

    private readonly PeriodicTimer _timer;
    private readonly List<ISimulationSystem> _systems = [];
    private readonly EventSystem _eventSystem;

    public event Action? OnUpdated;

    public SimulationEngine()
    {
        Company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "Headquartz Industries",
            Cash = 100_000,
            Reputation = 50,
        };

        Events = new EventBus();
        Commands = new CommandProcessor();

        SeedDepartments();
        SeedEmployees();
        SeedInventory();

        Clock = new SimulationClock();
        _timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        _eventSystem = new EventSystem();

        RegisterEventHandlers();
        RegisterSystems();
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public async Task StartAsync()
    {
        while (await _timer.WaitForNextTickAsync())
            Update();
    }

    public CompanySnapshot CreateSnapshot() => new()
    {
        Cash = Company.Cash,
        Reputation = Company.Reputation,
        EmployeeCount = Company.Employees.Count,
        TaskCount = Company.Tasks.Count,
        OrderCount = Company.Orders.Count,
        Tick = Clock.Tick,
        WorldTime = Clock.WorldTime,
    };

    // =========================================================
    // MAIN UPDATE LOOP
    // =========================================================

    private void Update()
    {
        Commands.Process(this);
        Clock.Advance();

        foreach (var system in _systems)
            system.Update(this);

        ProcessPayroll();
        ProcessInventory();
        ProcessOrders();
        GenerateRandomOrders();
        GenerateDepartmentTasks();
        AssignEmployeesToTasks();
        ProcessTasks();
        GenerateRandomEvents();
        CleanupCompletedTasks();

        OnUpdated?.Invoke();
    }

    // =========================================================
    // SYSTEM REGISTRATION
    // =========================================================

    private void RegisterSystems()
    {
        _systems.Add(new FinanceSystem());
        _systems.Add(new HumanResourcesSystem());
        _systems.Add(new SalesSystem());
        _systems.Add(new WarehouseSystem());
        _systems.Add(new ProductionSystem());
        _systems.Add(new MarketingSystem());
        _systems.Add(new LogisticsSystem());
        _systems.Add(new CascadeSystem());      // must run last
    }

    // =========================================================
    // EVENT REGISTRATION
    // =========================================================

    private void RegisterEventHandlers()
    {
        Events.Subscribe<OrderCreatedEvent>(HandleOrderCreated);
        Events.Subscribe<OrderFailedEvent>(HandleOrderFailed);
        Events.Subscribe<InventoryLowEvent>(HandleInventoryLow);
        Events.Subscribe<PayrollProcessedEvent>(HandlePayrollProcessed);
        Events.Subscribe<PayrollFailedEvent>(HandlePayrollFailed);
        Events.Subscribe<TaskCreatedEvent>(HandleTaskCreated);
        Events.Subscribe<TaskCompletedEvent>(HandleTaskCompleted);
        Events.Subscribe<EmployeeResignedEvent>(HandleEmployeeResigned);
        Events.Subscribe<CashCrisisEvent>(HandleCashCrisis);
        Events.Subscribe<DepartmentCrisisEvent>(HandleDepartmentCrisis);
    }

    // =========================================================
    // PAYROLL
    // =========================================================

    private void ProcessPayroll()
    {
        if (Clock.Tick % 10 != 0) return;

        decimal payroll = Company.Employees.Sum(e => e.Salary);

        if (Company.Cash < payroll)
        {
            // Payroll failure — publish before deducting
            Events.Publish(new PayrollFailedEvent
            {
                TotalPayroll = payroll,
                Shortfall = payroll - Company.Cash,
            });

            // Still deduct what we have (go into deficit)
            Company.Cash -= payroll;
            Company.Expenses += payroll;
        }
        else
        {
            Company.Cash -= payroll;
            Company.Expenses += payroll;

            Events.Publish(new PayrollProcessedEvent
            {
                TotalPayroll = payroll
            });
        }
    }

    // =========================================================
    // INVENTORY
    // =========================================================

    private void ProcessInventory()
    {
        foreach (var item in Company.Inventory)
        {
            item.Quantity -= Random.Shared.Next(0, 5);
            item.Quantity = Math.Max(0, item.Quantity);

            if (item.Quantity <= item.MinimumStock)
                Events.Publish(new InventoryLowEvent { Item = item });
        }
    }

    // =========================================================
    // ORDERS
    // =========================================================

    private void ProcessOrders()
    {
        foreach (var order in Company.Orders)
        {
            // Skip terminal states
            if (order.Status is OrderStatus.Delivered or
                                OrderStatus.Cancelled)
                continue;

            order.Status = order.Status switch
            {
                OrderStatus.Pending => OrderStatus.Approved,
                OrderStatus.Approved => OrderStatus.InProduction,
                OrderStatus.InProduction => OrderStatus.ReadyForShipment,
                OrderStatus.ReadyForShipment => OrderStatus.Shipping,
                OrderStatus.Shipping => OrderStatus.Delivered,
                _ => order.Status,
            };

            if (order.Status == OrderStatus.Delivered)
                Company.Reputation = Math.Min(100, Company.Reputation + 1);
        }
    }

    // =========================================================
    // ORDER GENERATION — reputation-driven demand
    // =========================================================

    private void GenerateRandomOrders()
    {
        // Base 10% + up to 50% more from reputation
        double chance = 0.10 + (Company.Reputation / 100.0) * 0.50;

        if (Random.Shared.NextDouble() > chance) return;

        GenerateOrder();
    }

    private void GenerateOrder()
    {
        // Deadline scales with reputation: good rep = tighter deadlines
        int deadlineDays = Random.Shared.Next(5, 14);

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            ClientName = GetRandomClientName(),
            ProductName = GetRandomProductName(),
            Quantity = Random.Shared.Next(10, 100),
            UnitPrice = Random.Shared.Next(50, 150),
            Status = OrderStatus.Pending,
            CreatedAt = Clock.WorldTime,
            DeliveryDeadline = Clock.WorldTime.AddDays(deadlineDays),
        };

        Company.Orders.Add(order);

        Events.Publish(new OrderCreatedEvent { Order = order });
    }

    // =========================================================
    // TASK SYSTEM
    // =========================================================

    private void GenerateDepartmentTasks()
    {
        if (Random.Shared.NextDouble() < 0.75) return;

        var departments = Enum.GetValues<DepartmentType>();
        var department = departments[Random.Shared.Next(departments.Length)];

        // Don't generate tasks for non-operational departments
        var dept = Company.Departments
            .FirstOrDefault(d => d.Type == department);

        if (dept != null && !dept.IsOperational) return;

        int duration = Random.Shared.Next(3, 10);

        var task = new CompanyTask
        {
            Id = Guid.NewGuid(),
            Name = GenerateTaskName(department),
            Description = "Operational department task",
            Department = department,
            Priority = (TaskPriority)Random.Shared.Next(0, 4),
            Status = CompanyTaskStatus.Pending,
            RequiredEmployees = Random.Shared.Next(1, 4),
            AssignedEmployees = 0,
            Progress = 0,
            DurationTicks = duration,
            RemainingTicks = duration,
            BudgetCost = Random.Shared.Next(1_000, 5_000),
        };

        Company.Tasks.Add(task);

        Events.Publish(new TaskCreatedEvent { Task = task });
    }

    private void AssignEmployeesToTasks()
    {
        foreach (var task in Company.Tasks)
        {
            if (task.Status != CompanyTaskStatus.Pending) continue;

            var available = Company.Employees
                .Where(e => !e.IsAssigned && e.Department == task.Department)
                .ToList();

            if (available.Count == 0)
            {
                task.IsBlocked = true;
                continue;
            }

            task.IsBlocked = false;

            int assigned = Math.Min(task.RequiredEmployees, available.Count);

            foreach (var emp in available.Take(assigned))
                emp.IsAssigned = true;

            task.AssignedEmployees = assigned;
            task.Status = CompanyTaskStatus.Assigned;
        }
    }

    private void ProcessTasks()
    {
        foreach (var task in Company.Tasks)
        {
            if (task.Status == CompanyTaskStatus.Completed) continue;
            if (task.AssignedEmployees <= 0) continue;

            // Productivity of assigned employees affects task speed
            var deptEmployees = Company.Employees
                .Where(e => e.Department == task.Department && e.IsAssigned)
                .ToList();

            double avgProductivity = deptEmployees.Count > 0
                ? deptEmployees.Average(e => e.Productivity) / 100.0
                : 0.5;

            // Minimum 1 tick of progress even at low productivity
            if (Random.Shared.NextDouble() < Math.Max(0.2, avgProductivity))
            {
                task.Status = CompanyTaskStatus.InProgress;
                task.RemainingTicks--;
            }

            task.Progress = 1.0 - ((double)task.RemainingTicks / task.DurationTicks);

            if (task.RemainingTicks <= 0)
            {
                task.Status = CompanyTaskStatus.Completed;
                task.Progress = 1.0;

                Events.Publish(new TaskCompletedEvent { Task = task });
            }
        }
    }

    private void CleanupCompletedTasks()
    {
        if (Company.Tasks.Count <= 50) return;

        Company.Tasks.RemoveAll(t =>
            t.Status == CompanyTaskStatus.Completed);
    }

    // =========================================================
    // RANDOM EVENTS
    // =========================================================

    private void GenerateRandomEvents()
    {
        if (Random.Shared.NextDouble() < 0.95) return;

        _eventSystem.Update(Company);
    }

    // =========================================================
    // EVENT HANDLERS
    // =========================================================

    private void HandleOrderCreated(OrderCreatedEvent e)
    {
        decimal revenue = e.Order.Quantity * e.Order.UnitPrice;
        Company.Revenue += revenue;
        Company.Cash += revenue;
    }

    private void HandleOrderFailed(OrderFailedEvent e)
    {
        // Reputation already penalised in SalesSystem
        // Add a visible company event for dashboards
        Company.Events.Add(new CompanyEvent
        {
            Title = "Order Cancelled",
            Description = $"Order for {e.Order.ClientName} missed deadline.",
            Severity = Domain.Enums.EventSeverity.Medium,
            Department = Domain.Enums.DepartmentType.Sales,
            RemainingTicks = 20,
        });
    }

    private void HandleInventoryLow(InventoryLowEvent e)
    {
        Company.Reputation =
            Math.Max(0, Company.Reputation - 1);

        Company.Cash -= e.Item.UnitCost * 20;
    }

    private void HandlePayrollProcessed(PayrollProcessedEvent e)
    {
        // Successful payroll boosts morale
        foreach (var emp in Company.Employees)
            emp.Morale = Math.Clamp(emp.Morale + 1, 0, 100);
    }

    private void HandlePayrollFailed(PayrollFailedEvent e)
    {
        // Severe morale crash across all employees
        foreach (var emp in Company.Employees)
            emp.Morale = Math.Clamp(emp.Morale - 15, 0, 100);

        Company.Reputation = Math.Max(0, Company.Reputation - 5);

        Company.Events.Add(new CompanyEvent
        {
            Title = "Payroll Failed",
            Description =
                $"Could not cover payroll. Shortfall: ${e.Shortfall:N0}",
            Severity = Domain.Enums.EventSeverity.Critical,
            Department = Domain.Enums.DepartmentType.Finance,
            RemainingTicks = 30,
        });
    }

    private void HandleTaskCreated(TaskCreatedEvent e)
    {
        Company.Expenses += e.Task.BudgetCost;
    }

    private void HandleTaskCompleted(TaskCompletedEvent e)
    {
        Company.Reputation =
            Math.Min(100, Company.Reputation + 1);

        Company.Cash += Random.Shared.Next(1_000, 5_000);

        // Free up assigned employees
        foreach (var emp in Company.Employees
                     .Where(e2 => e2.Department == e.Task.Department))
            emp.IsAssigned = false;
    }

    private void HandleEmployeeResigned(EmployeeResignedEvent e)
    {
        Company.Reputation = Math.Max(0, Company.Reputation - 2);

        Company.Events.Add(new CompanyEvent
        {
            Title = "Employee Resigned",
            Description =
                $"{e.Employee.Name} ({e.Employee.Department}) left due to low morale.",
            Severity = Domain.Enums.EventSeverity.High,
            Department = e.Employee.Department,
            RemainingTicks = 25,
        });
    }

    private void HandleCashCrisis(CashCrisisEvent e)
    {
        Company.Events.Add(new CompanyEvent
        {
            Title = "Cash Crisis",
            Description =
                $"Company cash is critically low: ${e.CashBalance:N0}",
            Severity = Domain.Enums.EventSeverity.Critical,
            Department = Domain.Enums.DepartmentType.Finance,
            RemainingTicks = 40,
        });
    }

    private void HandleDepartmentCrisis(DepartmentCrisisEvent e)
    {
        Company.Events.Add(new CompanyEvent
        {
            Title = "Department Crisis",
            Description =
                $"{e.Department} is at critical stress ({e.StressLevel}%).",
            Severity = Domain.Enums.EventSeverity.High,
            Department = e.Department,
            RemainingTicks = 30,
        });
    }

    // =========================================================
    // SEEDING
    // =========================================================

    private void SeedDepartments()
    {
        Company.Departments =
        [
            new() { Type = DepartmentType.HumanResources, Budget = 10_000, Efficiency = 50 },
            new() { Type = DepartmentType.Finance,        Budget = 15_000, Efficiency = 60 },
            new() { Type = DepartmentType.Sales,          Budget = 12_000, Efficiency = 55 },
            new() { Type = DepartmentType.Marketing,      Budget = 12_000, Efficiency = 50 },
            new() { Type = DepartmentType.Production,     Budget = 25_000, Efficiency = 70 },
            new() { Type = DepartmentType.Warehouse,      Budget = 10_000, Efficiency = 50 },
            new() { Type = DepartmentType.Logistics,      Budget = 15_000, Efficiency = 60 },
        ];
    }

    private void SeedEmployees()
    {
        Company.Employees =
        [
            new() { Id = Guid.NewGuid(), Name = "Alice",  Role = EmployeeRole.Manager,    Department = DepartmentType.Finance,        Salary = 5_000, Morale = 75, Productivity = 80 },
            new() { Id = Guid.NewGuid(), Name = "Bob",    Role = EmployeeRole.Worker,     Department = DepartmentType.Warehouse,      Salary = 2_500, Morale = 60, Productivity = 70 },
            new() { Id = Guid.NewGuid(), Name = "Carol",  Role = EmployeeRole.Supervisor, Department = DepartmentType.Production,     Salary = 3_500, Morale = 65, Productivity = 75 },
            new() { Id = Guid.NewGuid(), Name = "David",  Role = EmployeeRole.Worker,     Department = DepartmentType.Logistics,      Salary = 2_600, Morale = 55, Productivity = 68 },
            new() { Id = Guid.NewGuid(), Name = "Emma",   Role = EmployeeRole.Manager,    Department = DepartmentType.Marketing,      Salary = 4_800, Morale = 80, Productivity = 82 },
            new() { Id = Guid.NewGuid(), Name = "Frank",  Role = EmployeeRole.Worker,     Department = DepartmentType.Sales,          Salary = 2_800, Morale = 70, Productivity = 72 },
            new() { Id = Guid.NewGuid(), Name = "Grace",  Role = EmployeeRole.Worker,     Department = DepartmentType.HumanResources, Salary = 2_700, Morale = 68, Productivity = 71 },
            new() { Id = Guid.NewGuid(), Name = "Henry",  Role = EmployeeRole.Worker,     Department = DepartmentType.Production,     Salary = 2_600, Morale = 60, Productivity = 65 },
        ];
    }

    private void SeedInventory()
    {
        Company.Inventory =
        [
            new() { Id = Guid.NewGuid(), Name = "Steel",       Quantity = 500, UnitCost = 10, MinimumStock = 100, MaximumStock = 1_000 },
            new() { Id = Guid.NewGuid(), Name = "Plastic",     Quantity = 300, UnitCost =  5, MinimumStock =  50, MaximumStock =   800 },
            new() { Id = Guid.NewGuid(), Name = "Electronics", Quantity = 150, UnitCost = 25, MinimumStock =  40, MaximumStock =   400 },
        ];
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static string GenerateTaskName(DepartmentType dept) =>
        dept switch
        {
            DepartmentType.HumanResources => "Employee Recruitment",
            DepartmentType.Finance => "Budget Review",
            DepartmentType.Sales => "Client Negotiation",
            DepartmentType.Marketing => "Marketing Campaign",
            DepartmentType.Production => "Production Batch",
            DepartmentType.Warehouse => "Inventory Sorting",
            DepartmentType.Logistics => "Delivery Coordination",
            _ => "General Task",
        };

    private static string GetRandomClientName()
    {
        string[] clients =
        [
            "Global Retail Corp", "TechNova Industries",
            "BlueStar Logistics", "Apex Manufacturing",
            "FutureGrid Systems",
        ];
        return clients[Random.Shared.Next(clients.Length)];
    }

    private static string GetRandomProductName()
    {
        string[] products =
        [
            "Industrial Widget", "Smart Controller",
            "Metal Housing",     "Control Board",
            "Sensor Module",
        ];
        return products[Random.Shared.Next(products.Length)];
    }
}