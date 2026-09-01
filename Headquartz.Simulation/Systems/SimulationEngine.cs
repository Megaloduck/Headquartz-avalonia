using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;

using Headquartz.Simulation.Events;
using Headquartz.Simulation.Ticks;
using Headquartz.Simulation.Commands;
using Headquartz.Simulation.Modules.Base;

using Headquartz.Shared.Networking;

namespace Headquartz.Simulation.Systems;

public class SimulationEngine
{
    public Company Company { get; }
    public SimulationClock Clock { get; }
    public EventBus Events { get; }
    public CommandProcessor Commands { get; }

    /// <summary>
    /// The industry-specific simulation context that provides all
    /// per-industry behavior (products, events, task names, etc.).
    /// </summary>
    public IIndustrySimulationContext? IndustryContext { get; }

    /// <summary>
    /// The difficulty profile driving event frequency,
    /// cascade severity, and tick timing.
    /// </summary>
    public SimulationProfile Profile { get; }

    private volatile int _tickDelayMs;
    private readonly IndustryProfile _industryProfile;
    private readonly List<ISimulationSystem> _systems = [];
    private readonly EventSystem _eventSystem;
    private readonly CalendarEventSystem _calendarEventSystem;
    private readonly CascadeSystem _cascadeSystem;

    public event Action? OnUpdated;

    public CalendarEventSystem CalendarEvents => _calendarEventSystem;

    // =========================================================
    // CONSTRUCTORS
    // =========================================================

    /// <summary>
    /// Default constructor — uses Food as default industry.
    /// </summary>
    public SimulationEngine()
        : this(SimulationProfile.Manager, IndustryType.Food) { }

    /// <summary>
    /// Profile-aware constructor used by RootViewModel after onboarding.
    /// </summary>
    public SimulationEngine(SimulationProfile profile, IndustryType industry = IndustryType.Food)
    {
        Profile = profile;
        _tickDelayMs = profile.TickDelayMs;
        IndustryContext = IndustryModuleRegistry.GetContext(industry);
        _industryProfile = IndustryContext?.GetProfile() ?? IndustryProfile.For(industry);

        Company = new Company
        {
            Id = Guid.NewGuid(),
            Name = "Headquartz Industries",
            Industry = industry,
            Cash = profile.InitialCapital * _industryProfile.CashMultiplier,

            // Founding starts at zero reputation regardless of industry —
            // nobody knows the company exists yet. Reputation is earned
            // through Marketing/Sales during Pre-Opening and beyond, not
            // inherited from the industry template. This intentionally
            // supersedes _industryProfile.StartingReputation, which is
            // left defined (unused for this purpose) rather than removed,
            // in case a future "veteran founder" mode wants it back.
            Reputation = 0,

            Phase = CompanyPhase.PreOpening,
        };

        Events = new EventBus();
        Commands = new CommandProcessor();

        SeedDepartments();
        SeedEmployees();
        SeedInventory();

        Clock = new SimulationClock();
        _eventSystem = new EventSystem();
        _calendarEventSystem = new CalendarEventSystem();
        _cascadeSystem = new CascadeSystem();

        RegisterEventHandlers();
        RegisterSystems();
    }

    // =========================================================
    // PUBLIC
    // =========================================================

    public async Task StartAsync()
    {
        while (true)
        {
            await Task.Delay(_tickDelayMs);
            Update();
        }
    }

    /// <summary>
    /// Adjusts simulation speed at runtime.
    /// 0.5 = half speed, 1.0 = normal, 2.0 = double, etc.
    /// </summary>
    public void SetTickSpeed(double multiplier)
    {
        double safe = Math.Max(0.1, multiplier);
        _tickDelayMs = (int)(Profile.TickDelayMs / safe);
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

    /// <summary>
    /// The ambient simulation — the steady-state systems loop, random
    /// event/task/order generation, and stress cascade — is dormant
    /// during CompanyPhase.PreOpening. Founding is meant to be a calm
    /// setup window: players hire, allocate budget, and stock up
    /// through their own commands (never gated), but nothing automatic
    /// fires at them yet. DeclareGrandOpeningCommand flips the phase and
    /// switches all of it on.
    ///
    /// This single gate replaces what would otherwise be several
    /// separate, easy-to-miss bugs if departments simply started empty
    /// with the old always-on update loop:
    ///   - HumanResourcesSystem.UpdateDepartmentStress adds +10 stress
    ///     per tick to any department with zero staff — every
    ///     department, every tick, until first hire. Left unguarded
    ///     this maxes stress out in roughly one work hour and starts
    ///     firing DepartmentCrisisEvent during the exact phase where
    ///     being unstaffed is the expected starting condition.
    ///   - ProcessInventory publishes InventoryLowEvent whenever
    ///     Quantity <= MinimumStock, which is unconditionally true for
    ///     a freshly seeded catalog sitting at Quantity 0 (see
    ///     SeedInventory). HandleInventoryLow then drains Cash and dings
    ///     Reputation for every catalog item, every tick — punishing a
    ///     warehouse for being empty by design, before Warehouse has
    ///     had a chance to authorize anything.
    ///   - WarehouseSystem auto-restocks (spends Cash, adds 25 units)
    ///     any item under MinimumStock. At Quantity 0 that's every
    ///     catalog item, every tick — the game would silently stock the
    ///     warehouse on its own, which defeats "Warehouse authorizes
    ///     goods" as a deliberate founding action entirely.
    ///
    /// Payroll, manual task processing, and calendar/holiday effects
    /// stay on regardless of phase — salaries owed are owed from day
    /// one, a task a player explicitly created should still run, and
    /// real-world calendar time doesn't pause just because the company
    /// hasn't opened yet.
    /// </summary>
    private void Update()
    {
        Commands.Process(this);
        Clock.Advance();

        if (Company.Phase == CompanyPhase.GrandOpening)
        {
            foreach (var system in _systems)
                system.Update(this);
        }

        ProcessPayroll();

        if (Company.Phase == CompanyPhase.GrandOpening)
        {
            ProcessInventory();
            ProcessOrders();
            GenerateRandomOrders();
            GenerateDepartmentTasks();
        }

        AssignEmployeesToTasks();
        ProcessTasks();

        if (Company.Phase == CompanyPhase.GrandOpening)
        {
            GenerateRandomEvents();
            RunCascade();
        }

        _calendarEventSystem.Update(this);

        // Founding phase ends when the "Grand Opening" CompanyAgenda event's
        // own active window closes — see CompanyPhase.cs. Tune the length of
        // PreOpening by changing DurationDays on that definition, not here.
        if (Company.Phase == CompanyPhase.PreOpening &&
            !_calendarEventSystem.ActiveEvents.Any(a => a.Definition.Id == "grand-opening"))
        {
            Company.Phase = CompanyPhase.GrandOpening;
        }

        CleanupCompletedTasks();
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
        // CascadeSystem is NOT in _systems — called directly so we can
        // pass the profile multiplier without changing ISimulationSystem.
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
            Events.Publish(new PayrollFailedEvent
            {
                TotalPayroll = payroll,
                Shortfall = payroll - Company.Cash,
            });

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
    // INVENTORY — delegated to industry context
    // =========================================================

    private void ProcessInventory()
    {
        if (Company.Phase == CompanyPhase.PreOpening) return;
        if (IndustryContext != null)
            IndustryContext.ProcessInventory(this);
        else
        {
            // Absolute fallback
            foreach (var item in Company.Inventory)
            {
                item.Quantity -= Random.Shared.Next(0, 5);
                item.Quantity = Math.Max(0, item.Quantity);
                if (item.Quantity <= item.MinimumStock)
                    Events.Publish(new InventoryLowEvent { Item = item });
            }
        }
    }

    // =========================================================
    // ORDERS — delegated to industry context
    // =========================================================

    private void ProcessOrders()
    {
        if (IndustryContext != null)
            IndustryContext.ProcessOrders(this);
        else
        {
            // Absolute fallback
            foreach (var order in Company.Orders)
            {
                if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
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
    }

    // =========================================================
    // ORDER GENERATION — delegated to industry context
    // =========================================================

    private void GenerateRandomOrders()
    {
        double chance = 0.10 + (Company.Reputation / 100.0) * 0.50;

        if (Random.Shared.NextDouble() > chance) return;

        GenerateOrder();
    }

    private void GenerateOrder()
    {
        if (IndustryContext == null) return;

        int deadlineDays = IndustryContext.GetOrderDeadlineDays();

        var order = new SalesOrder
        {
            Id = Guid.NewGuid(),
            ClientName = IndustryContext.GetRandomClientName(),
            ProductName = IndustryContext.GetRandomProductName(),
            Quantity = IndustryContext.GetOrderQuantity(),
            UnitPrice = IndustryContext.GetOrderUnitPrice(),
            Status = OrderStatus.Pending,
            CreatedAt = Clock.WorldTime,
            DeliveryDeadline = Clock.WorldTime.AddDays(deadlineDays),
        };

        Company.Orders.Add(order);
        Events.Publish(new OrderCreatedEvent { Order = order });
    }

    // =========================================================
    // TASK SYSTEM — delegated to industry context for flavor
    // =========================================================

    private void GenerateDepartmentTasks()
    {
        if (Random.Shared.NextDouble() < 0.75) return;

        var departments = Enum.GetValues<DepartmentType>();
        var department = departments[Random.Shared.Next(departments.Length)];

        var dept = Company.Departments
            .FirstOrDefault(d => d.Type == department);

        if (dept != null && !dept.IsOperational) return;

        int duration = Random.Shared.Next(3, 10);

        var taskName = IndustryContext?.GetTaskName(department) ?? "General Task";
        var taskDesc = IndustryContext?.GetTaskDescription(department) ?? "Operational department task";

        var task = new CompanyTask
        {
            Id = Guid.NewGuid(),
            Name = taskName,
            Description = taskDesc,
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

            var deptEmployees = Company.Employees
                .Where(e => e.Department == task.Department && e.IsAssigned)
                .ToList();

            double avgProductivity = deptEmployees.Count > 0
                ? deptEmployees.Average(e => e.Productivity) / 100.0
                : 0.5;

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

                // Industry-specific resource consumption on completion
                if (task.Department == DepartmentType.Production && IndustryContext != null)
                    IndustryContext.ConsumeProductionResources(this);

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
    // RANDOM EVENTS — profile-driven, with industry flavor
    // =========================================================

    private void GenerateRandomEvents()
    {
        double frequency = Math.Clamp(
            Profile.EventFrequency * _industryProfile.EventFrequencyModifier,
            0.0, 1.0);

        if (Random.Shared.NextDouble() >= frequency) return;

        _eventSystem.Update(
            this,
            eventFrequency: 1.0,          // already gated above
            severityBias: Profile.SeverityBias);
    }

    // =========================================================
    // CASCADE — profile-driven
    // =========================================================

    private void RunCascade()
    {
        _cascadeSystem.Update(this, Profile.CascadeMultiplier);
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
        Company.Reputation = Math.Max(0, Company.Reputation - 1);
        Company.Cash -= e.Item.UnitCost * 20;
    }

    private void HandlePayrollProcessed(PayrollProcessedEvent e)
    {
        foreach (var emp in Company.Employees)
            emp.Morale = Math.Clamp(emp.Morale + 1, 0, 100);
    }

    private void HandlePayrollFailed(PayrollFailedEvent e)
    {
        foreach (var emp in Company.Employees)
            emp.Morale = Math.Clamp(emp.Morale - 15, 0, 100);

        Company.Reputation = Math.Max(0, Company.Reputation - 5);

        Company.Events.Add(new CompanyEvent
        {
            Title = "Payroll Failed",
            Description = $"Could not cover payroll. Shortfall: ${e.Shortfall:N0}",
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
        Company.Reputation = Math.Min(100, Company.Reputation + 1);
        Company.Cash += Random.Shared.Next(1_000, 5_000);

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
            Description = $"{e.Employee.Name} ({e.Employee.Department}) left due to low morale.",
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
            Description = $"Company cash is critically low: ${e.CashBalance:N0}",
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
            Description = $"{e.Department} is at critical stress ({e.StressLevel}%).",
            Severity = Domain.Enums.EventSeverity.High,
            Department = e.Department,
            RemainingTicks = 30,
        });
    }

    // =========================================================
    // SEEDING — founding state: empty departments, no staff, no
    // pre-existing stock. All delegated to industry context for shape
    // (which item types exist, what the department deltas are); the
    // actual starting quantities are zeroed here so nothing is
    // pre-built before the players do it themselves.
    // =========================================================

    private void SeedDepartments()
    {
        if (IndustryContext != null)
        {
            Company.Departments = IndustryContext.GetInitialDepartments().ToList();
            return;
        }

        // Absolute fallback — no industry context available. Same
        // founding rule applies: zero budget, zero efficiency, Finance
        // allocates from here.
        Company.Departments =
        [
            new() { Type = DepartmentType.HumanResources, Budget = 0, Efficiency = 0 },
            new() { Type = DepartmentType.Finance,        Budget = 0, Efficiency = 0 },
            new() { Type = DepartmentType.Sales,          Budget = 0, Efficiency = 0 },
            new() { Type = DepartmentType.Marketing,      Budget = 0, Efficiency = 0 },
            new() { Type = DepartmentType.Production,     Budget = 0, Efficiency = 0 },
            new() { Type = DepartmentType.Warehouse,      Budget = 0, Efficiency = 0 },
            new() { Type = DepartmentType.Logistics,      Budget = 0, Efficiency = 0 },
        ];
    }

    /// <summary>
    /// Founding starts with zero staff across every department. HR
    /// hires from here via HireEmployeeCommand, or by fulfilling a
    /// WorkforceRequest raised by another department.
    /// IIndustrySimulationContext.GetInitialEmployees() still exists
    /// and still returns each industry's flavor roster — it's just no
    /// longer consumed here to pre-populate the company. Left in place
    /// as reference data in case it's useful later (e.g. a "suggested
    /// hires" list in the recruitment UI).
    /// </summary>
    private void SeedEmployees()
    {
        Company.Employees = [];
    }

    /// <summary>
    /// Warehouse starts with the industry's item catalog (names, unit
    /// costs, min/max stock) but nothing physically stocked — every
    /// item's Quantity is zeroed. This keeps ConsumeProductionResources,
    /// the restock UI's item dropdowns, and per-item spoilage logic all
    /// working exactly as before, while genuinely representing an empty
    /// warehouse: nothing exists until Warehouse spends money to
    /// receive stock via ReceiveStock().
    ///
    /// Sitting at Quantity 0 would normally trip the low-stock event
    /// (Quantity <= MinimumStock) every tick — see the CompanyPhase
    /// gate in Update(), which skips ProcessInventory() entirely during
    /// PreOpening specifically so an intentionally empty warehouse isn't
    /// penalized for existing.
    /// </summary>
    private void SeedInventory()
    {
        if (IndustryContext == null)
        {
            Company.Inventory = [];
            return;
        }

        Company.Inventory = IndustryContext.GetInitialInventory()
            .Select(item =>
            {
                item.Quantity = 0;
                return item;
            })
            .ToList();
    }
}