using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class WarehouseDashboardViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private int _totalItems;
    [ObservableProperty] private int _lowStockCount;
    [ObservableProperty] private decimal _inventoryValue;
    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;
    [ObservableProperty] private bool _hasLowStock;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<InventoryRowModel> Inventory { get; } = [];
    public ObservableCollection<EventViewModel> Events { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public WarehouseDashboardViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void EmergencyRestock()
    {
        var company = _simulation.Engine.Company;
        const decimal costPerItem = 500m;

        foreach (var item in company.Inventory
                     .Where(i => i.Quantity < i.MinimumStock))
        {
            int restockAmount = item.MinimumStock - item.Quantity + 50;
            company.Cash -= restockAmount * item.UnitCost;
            company.Expenses += restockAmount * item.UnitCost;
            item.Quantity += restockAmount;
        }

        Refresh();
    }

    [RelayCommand]
    private void ReorganizeWarehouse()
    {
        var dept = _simulation.Engine.Company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Warehouse);

        if (dept != null)
        {
            dept.StressLevel = Math.Max(0, dept.StressLevel - 15);
            dept.Efficiency = Math.Min(100, dept.Efficiency + 5);
        }

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        var whDept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Warehouse);

        DeptEfficiency = whDept?.Efficiency ?? 0;
        DeptStress = whDept?.StressLevel ?? 0;
        TotalItems = company.Inventory.Sum(i => i.Quantity);
        LowStockCount = company.Inventory.Count(i => i.Quantity < i.MinimumStock);
        InventoryValue = company.Inventory.Sum(i => i.Quantity * i.UnitCost);
        HasLowStock = LowStockCount > 0;

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Units", Value = TotalItems.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Low Stock Items", Value = LowStockCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Inventory Value", Value = $"${InventoryValue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });

        // Inventory rows
        Inventory.Clear();
        foreach (var item in company.Inventory)
        {
            double fill = item.MaximumStock > 0
                ? Math.Clamp((double)item.Quantity / item.MaximumStock, 0, 1)
                : 0;

            Inventory.Add(new InventoryRowModel
            {
                Name = item.Name,
                Quantity = item.Quantity,
                MinimumStock = item.MinimumStock,
                MaximumStock = item.MaximumStock,
                UnitCost = item.UnitCost,
                IsLow = item.Quantity < item.MinimumStock,
                FillPercent = fill,
            });
        }

        // Events
        Events.Clear();
        foreach (var ev in company.Events
                     .Where(e => e.Department == DepartmentType.Warehouse && !e.IsResolved)
                     .OrderByDescending(e => e.Severity))
        {
            Events.Add(new EventViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Severity = ev.Severity,
                RemainingTicks = ev.RemainingTicks,
            });
        }
    }
}