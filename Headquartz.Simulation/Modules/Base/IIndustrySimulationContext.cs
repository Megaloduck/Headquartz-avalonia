using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Systems;
using Headquartz.Simulation.Ticks;

namespace Headquartz.Simulation.Modules.Base;

/// <summary>
/// Provides industry-specific simulation behavior so the same 7 departments
/// feel completely different depending on the chosen industry.
/// </summary>
public interface IIndustrySimulationContext
{
    // ── Identity ──
    IndustryType IndustryType { get; }
    string IndustryName { get; }
    string IndustryDescription { get; }
    string IndustryEmoji { get; }

    // ── Profile ──
    IndustryProfile GetProfile();
    IReadOnlyDictionary<DepartmentType, DepartmentRelevance> GetDepartmentRelevances();

    // ── Seeding ──
    IReadOnlyList<InventoryItem> GetInitialInventory();
    IReadOnlyList<Employee> GetInitialEmployees();

    // ── Order Generation ──
    string GetRandomClientName();
    string GetRandomProductName();

    /// <summary>How many units per order. E.g., Food = 50-200, Automotive = 1-5.</summary>
    int GetOrderQuantity();

    /// <summary>Price per unit. E.g., Food = $2-10, Automotive = $20,000-40,000.</summary>
    decimal GetOrderUnitPrice();

    /// <summary>Days until deadline. E.g., Food = 2-5, Automotive = 30-60.</summary>
    int GetOrderDeadlineDays();

    // ── Task Flavor ──
    string GetTaskName(DepartmentType dept);
    string GetTaskDescription(DepartmentType dept);

    // ── Per-Tick Simulation Hooks ──

    /// <summary>Called each tick. Default: depletes inventory by 0-5 units.</summary>
    void ProcessInventory(SimulationEngine engine);

    /// <summary>Called each tick. Default: advances orders through Pending→Approved→InProduction→ReadyForShipment→Shipping→Delivered.</summary>
    void ProcessOrders(SimulationEngine engine);

    /// <summary>Called each tick when an item is below minimum stock. Default: buys 25 units at cost×10.</summary>
    void ProcessWarehouseRestock(SimulationEngine engine, InventoryItem item);

    /// <summary>Called when a Production task completes. Default: consumes first inventory item by 5.</summary>
    void ConsumeProductionResources(SimulationEngine engine);

    // ── Event Flavor ──
    string GetEventTitle(DepartmentType dept);
    string GetEventDescription(DepartmentType dept);

    // ── Industry Flags ──
    bool HasPhysicalProducts { get; }
    bool HasPerishableInventory { get; }
}
