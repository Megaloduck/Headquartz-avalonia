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

    private readonly List<ISimulationSystem>
        _systems = [];

    private readonly EventSystem
    _eventSystem;

    public event Action? OnUpdated;

    public SimulationEngine()
    {
        Company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "Headquartz Industries",
            Cash = 100000,
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
        {
            Update();
        }
    }
    public CompanySnapshot CreateSnapshot()
    {
        return new CompanySnapshot
        {
            Cash = Company.Cash,

            Reputation =
                Company.Reputation,

            EmployeeCount =
                Company.Employees.Count,

            TaskCount =
                Company.Tasks.Count,

            OrderCount =
                Company.Orders.Count,

            Tick =
                Clock.Tick,

            WorldTime =
                Clock.WorldTime,
        };
    }

    // =========================================================
    // MAIN UPDATE LOOP
    // =========================================================

    private void Update()
    {
        Commands.Process(this);

        Clock.Advance();

        foreach (var system in _systems)
        {
            system.Update(this);
        }

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
        _systems.Add(
            new FinanceSystem());

        _systems.Add(
            new HumanResourcesSystem());

        _systems.Add(
            new SalesSystem());

        _systems.Add(
            new WarehouseSystem());

        _systems.Add(
            new ProductionSystem());

        _systems.Add(
            new MarketingSystem());

        _systems.Add(
            new LogisticsSystem());
    }

    // =========================================================
    // EVENT REGISTRATION
    // =========================================================

    private void RegisterEventHandlers()
    {
        Events.Subscribe<OrderCreatedEvent>(
            HandleOrderCreated);

        Events.Subscribe<InventoryLowEvent>(
            HandleInventoryLow);

        Events.Subscribe<PayrollProcessedEvent>(
            HandlePayrollProcessed);

        Events.Subscribe<TaskCreatedEvent>(
            HandleTaskCreated);

        Events.Subscribe<TaskCompletedEvent>(
            HandleTaskCompleted);
    }

    // =========================================================
    // PAYROLL
    // =========================================================

    private void ProcessPayroll()
    {
        if (Clock.Tick % 10 != 0)
        {
            return;
        }

        decimal payroll =
            Company.Employees.Sum(
                e => e.Salary);

        Company.Cash -= payroll;

        Company.Expenses += payroll;

        Events.Publish(
            new PayrollProcessedEvent
            {
                TotalPayroll = payroll
            });
    }

    // =========================================================
    // INVENTORY
    // =========================================================

    private void ProcessInventory()
    {
        foreach (var item in Company.Inventory)
        {
            item.Quantity -=
                Random.Shared.Next(0, 5);

            item.Quantity =
                Math.Max(0, item.Quantity);

            if (item.Quantity <= item.MinimumStock)
            {
                Events.Publish(
                    new InventoryLowEvent
                    {
                        Item = item
                    });
            }
        }
    }

    // =========================================================
    // ORDERS
    // =========================================================

    private void ProcessOrders()
    {
        foreach (var order in Company.Orders)
        {
            switch (order.Status)
            {
                case OrderStatus.Pending:

                    order.Status =
                        OrderStatus.Approved;

                    break;

                case OrderStatus.Approved:

                    order.Status =
                        OrderStatus.InProduction;

                    break;

                case OrderStatus.InProduction:

                    order.Status =
                        OrderStatus.ReadyForShipment;

                    break;

                case OrderStatus.ReadyForShipment:

                    order.Status =
                        OrderStatus.Shipping;

                    break;

                case OrderStatus.Shipping:

                    order.Status =
                        OrderStatus.Delivered;

                    Company.Reputation += 1;

                    break;
            }
        }
    }

    private void GenerateRandomOrders()
    {
        if (Random.Shared.NextDouble() < 0.7)
        {
            return;
        }

        GenerateOrder();
    }

    private void GenerateOrder()
    {
        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),

            ClientName =
                GetRandomClientName(),

            ProductName =
                GetRandomProductName(),

            Quantity =
                Random.Shared.Next(10, 100),

            UnitPrice =
                Random.Shared.Next(50, 150),

            Status =
                OrderStatus.Pending,

            CreatedAt =
                Clock.WorldTime,

            DeliveryDeadline =
                Clock.WorldTime.AddDays(7),
        };

        Company.Orders.Add(order);

        Events.Publish(
            new OrderCreatedEvent
            {
                Order = order
            });
    }

    // =========================================================
    // TASK SYSTEM
    // =========================================================

    private void GenerateDepartmentTasks()
    {
        if (Random.Shared.NextDouble() < 0.75)
        {
            return;
        }

        var departments =
            Enum.GetValues<DepartmentType>();

        var department =
            departments[
                Random.Shared.Next(departments.Length)];

        int duration =
            Random.Shared.Next(3, 10);

        var task = new CompanyTask
        {
            Id = Guid.NewGuid(),

            Name =
                GenerateTaskName(department),

            Description =
                "Operational department task",

            Department = department,

            Priority =
                (TaskPriority)
                Random.Shared.Next(0, 4),

            Status =
                CompanyTaskStatus.Pending,

            RequiredEmployees =
                Random.Shared.Next(1, 4),

            AssignedEmployees = 0,

            Progress = 0,

            DurationTicks = duration,

            RemainingTicks = duration,

            BudgetCost =
                Random.Shared.Next(1000, 5000),
        };

        Company.Tasks.Add(task);

        Events.Publish(
            new TaskCreatedEvent
            {
                Task = task
            });
    }

    private void AssignEmployeesToTasks()
    {
        foreach (var task in Company.Tasks)
        {
            if (task.Status !=
                CompanyTaskStatus.Pending)
            {
                continue;
            }

            var availableEmployees =
                Company.Employees
                    .Where(e =>
                        !e.IsAssigned &&
                        e.Department ==
                        task.Department)
                    .ToList();

            if (availableEmployees.Count <= 0)
            {
                task.IsBlocked = true;

                continue;
            }

            int assigned =
                Math.Min(
                    task.RequiredEmployees,
                    availableEmployees.Count);

            foreach (var employee
                     in availableEmployees.Take(assigned))
            {
                employee.IsAssigned = true;
            }

            task.AssignedEmployees =
                assigned;

            task.Status =
                CompanyTaskStatus.Assigned;
        }
    }

    private void ProcessTasks()
    {
        foreach (var task in Company.Tasks)
        {
            if (task.Status ==
                CompanyTaskStatus.Completed)
            {
                continue;
            }

            if (task.AssignedEmployees <= 0)
            {
                continue;
            }

            task.Status =
                CompanyTaskStatus.InProgress;

            task.RemainingTicks--;

            task.Progress =
                1.0 -
                ((double)task.RemainingTicks /
                 task.DurationTicks);

            if (task.RemainingTicks <= 0)
            {
                task.Status =
                    CompanyTaskStatus.Completed;

                task.Progress = 1;

                Events.Publish(
                    new TaskCompletedEvent
                    {
                        Task = task
                    });
            }
        }
    }

    private void CleanupCompletedTasks()
    {
        if (Company.Tasks.Count <= 50)
        {
            return;
        }

        Company.Tasks.RemoveAll(
            t => t.Status ==
                 CompanyTaskStatus.Completed);
    }

    // =========================================================
    // RANDOM EVENTS
    // =========================================================

    private void GenerateRandomEvents()
    {
        if (Random.Shared.NextDouble() < 0.95)
        {
            return;
        }

        int reputationLoss =
            Random.Shared.Next(1, 5);

        decimal cashLoss =
            Random.Shared.Next(1000, 5000);

        Company.Reputation -= reputationLoss;

        Company.Cash -= cashLoss;

        Company.Reputation =
            Math.Max(
                0,
                Company.Reputation);
    }

    // =========================================================
    // EVENT HANDLERS
    // =========================================================

    private void HandleOrderCreated(
        OrderCreatedEvent gameEvent)
    {
        decimal revenue =
            gameEvent.Order.Quantity *
            gameEvent.Order.UnitPrice;

        Company.Revenue += revenue;

        Company.Cash += revenue;
    }

    private void HandleInventoryLow(
        InventoryLowEvent gameEvent)
    {
        Company.Reputation -= 1;

        Company.Cash -=
            gameEvent.Item.UnitCost * 20;
    }

    private void HandlePayrollProcessed(
        PayrollProcessedEvent gameEvent)
    {
        foreach (var employee
                 in Company.Employees)
        {
            employee.Morale += 1;

            employee.Morale =
                Math.Clamp(
                    employee.Morale,
                    0,
                    100);
        }
    }

    private void HandleTaskCreated(
        TaskCreatedEvent gameEvent)
    {
        Company.Expenses +=
            gameEvent.Task.BudgetCost;
    }

    private void HandleTaskCompleted(
        TaskCompletedEvent gameEvent)
    {
        Company.Reputation += 1;

        Company.Cash +=
            Random.Shared.Next(1000, 5000);

        foreach (var employee
                 in Company.Employees
                     .Where(e =>
                         e.Department ==
                         gameEvent.Task.Department))
        {
            employee.IsAssigned = false;
        }
    }

    // =========================================================
    // INITIALIZATION
    // =========================================================

    private void SeedDepartments()
    {
        Company.Departments =
        [
            new()
            {
                Type =
                    DepartmentType.HumanResources,

                Budget = 10000,

                Efficiency = 50,
            },

            new()
            {
                Type =
                    DepartmentType.Finance,

                Budget = 15000,

                Efficiency = 60,
            },

            new()
            {
                Type =
                    DepartmentType.Sales,

                Budget = 12000,

                Efficiency = 55,
            },

            new()
            {
                Type =
                    DepartmentType.Marketing,

                Budget = 12000,

                Efficiency = 50,
            },

            new()
            {
                Type =
                    DepartmentType.Production,

                Budget = 25000,

                Efficiency = 70,
            },

            new()
            {
                Type =
                    DepartmentType.Warehouse,

                Budget = 10000,

                Efficiency = 50,
            },

            new()
            {
                Type =
                    DepartmentType.Logistics,

                Budget = 15000,

                Efficiency = 60,
            },
        ];
    }

    private void SeedEmployees()
    {
        Company.Employees =
        [
            new()
            {
                Id = Guid.NewGuid(),

                Name = "Alice",

                Role =
                    EmployeeRole.Manager,

                Department =
                    DepartmentType.Finance,

                Salary = 5000,

                Morale = 75,

                Productivity = 80,
            },

            new()
            {
                Id = Guid.NewGuid(),

                Name = "Bob",

                Role =
                    EmployeeRole.Worker,

                Department =
                    DepartmentType.Warehouse,

                Salary = 2500,

                Morale = 60,

                Productivity = 70,
            },

            new()
            {
                Id = Guid.NewGuid(),

                Name = "Carol",

                Role =
                    EmployeeRole.Supervisor,

                Department =
                    DepartmentType.Production,

                Salary = 3500,

                Morale = 65,

                Productivity = 75,
            },

            new()
            {
                Id = Guid.NewGuid(),

                Name = "David",

                Role =
                    EmployeeRole.Worker,

                Department =
                    DepartmentType.Logistics,

                Salary = 2600,

                Morale = 55,

                Productivity = 68,
            },

            new()
            {
                Id = Guid.NewGuid(),

                Name = "Emma",

                Role =
                    EmployeeRole.Manager,

                Department =
                    DepartmentType.Marketing,

                Salary = 4800,

                Morale = 80,

                Productivity = 82,
            },
        ];
    }

    private void SeedInventory()
    {
        Company.Inventory =
        [
            new()
            {
                Id = Guid.NewGuid(),

                Name = "Steel",

                Quantity = 500,

                UnitCost = 10,

                MinimumStock = 100,

                MaximumStock = 1000,
            },

            new()
            {
                Id = Guid.NewGuid(),

                Name = "Plastic",

                Quantity = 300,

                UnitCost = 5,

                MinimumStock = 50,

                MaximumStock = 800,
            },

            new()
            {
                Id = Guid.NewGuid(),

                Name = "Electronics",

                Quantity = 150,

                UnitCost = 25,

                MinimumStock = 40,

                MaximumStock = 400,
            },
        ];
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private string GenerateTaskName(
        DepartmentType department)  
    {
        return department switch
        {
            DepartmentType.HumanResources =>
                "Employee Recruitment",

            DepartmentType.Finance =>
                "Budget Review",

            DepartmentType.Sales =>
                "Client Negotiation",

            DepartmentType.Marketing =>
                "Marketing Campaign",

            DepartmentType.Production =>
                "Production Batch",

            DepartmentType.Warehouse =>
                "Inventory Sorting",

            DepartmentType.Logistics =>
                "Delivery Coordination",

            _ =>
                "General Task"
        };
    }

    private string GetRandomClientName()
    {
        string[] clients =
        [
            "Global Retail Corp",
            "TechNova Industries",
            "BlueStar Logistics",
            "Apex Manufacturing",
            "FutureGrid Systems",
        ];

        return clients[
            Random.Shared.Next(
                clients.Length)];
    }

    private string GetRandomProductName()
    {
        string[] products =
        [
            "Industrial Widget",
            "Smart Controller",
            "Metal Housing",
            "Control Board",
            "Sensor Module",
        ];

        return products[
            Random.Shared.Next(
                products.Length)];
    }
}