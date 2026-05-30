using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Headquartz.App.ViewModels;

public partial class WarehouseInventoryViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private decimal _totalValue;
    [ObservableProperty] private int _lowStockCount;
    [ObservableProperty] private int _totalUnits;
    [ObservableProperty] private decimal _companyCash;

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<InventoryRowModel> Inventory { get; } = [];

    public WarehouseInventoryViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void RestockItem(InventoryRowModel row)
    {
        var item = _simulation.Engine.Company.Inventory
            .FirstOrDefault(i => i.Id == row.ItemId);

        if (item == null) return;

        const int restockQty = 50;
        decimal cost = restockQty * item.UnitCost;
        var company = _simulation.Engine.Company;

        if (company.Cash < cost) return;

        company.Cash -= cost;
        company.Expenses += cost;
        item.Quantity += restockQty;

        Refresh();
    }

    [RelayCommand]
    private void RestockAll()
    {
        var company = _simulation.Engine.Company;

        foreach (var item in company.Inventory
                     .Where(i => i.Quantity < i.MinimumStock))
        {
            int needed = item.MinimumStock - item.Quantity + 50;
            decimal cost = needed * item.UnitCost;

            if (company.Cash < cost) break;

            company.Cash -= cost;
            company.Expenses += cost;
            item.Quantity += needed;
        }

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        CompanyCash = company.Cash;
        TotalUnits = company.Inventory.Sum(i => i.Quantity);
        LowStockCount = company.Inventory.Count(i => i.Quantity < i.MinimumStock);
        TotalValue = company.Inventory.Sum(i => i.Quantity * i.UnitCost);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Units", Value = TotalUnits.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Low Stock", Value = LowStockCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Total Value", Value = $"${TotalValue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Company Cash", Value = $"${CompanyCash:N0}" });

        Inventory.Clear();
        foreach (var item in company.Inventory)
        {
            double fill = item.MaximumStock > 0
                ? Math.Clamp((double)item.Quantity / item.MaximumStock, 0, 1)
                : 0;

            Inventory.Add(new InventoryRowModel
            {
                ItemId = item.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                MinimumStock = item.MinimumStock,
                MaximumStock = item.MaximumStock,
                UnitCost = item.UnitCost,
                IsLow = item.Quantity < item.MinimumStock,
                FillPercent = fill,
            });
        }
    }
}
