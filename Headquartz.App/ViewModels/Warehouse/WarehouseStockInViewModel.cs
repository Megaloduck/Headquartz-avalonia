using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class StockInRecordModel : ObservableObject
{
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string ReceivedAt { get; set; } = "";
    public string Supplier { get; set; } = "";
}

public partial class WarehouseStockInViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<StockInRecordModel> _history = [];

    [ObservableProperty] private int _totalUnitsReceived;
    [ObservableProperty] private decimal _totalSpent;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _selectedItem = "";
    [ObservableProperty] private int _orderQuantity = 50;

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<StockInRecordModel> History { get; } = [];
    public ObservableCollection<InventoryRowModel> CurrentStock { get; } = [];
    public ObservableCollection<string> AvailableItems { get; } = [];

    public WarehouseStockInViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void ReceiveStock()
    {
        if (string.IsNullOrWhiteSpace(SelectedItem))
        {
            StatusMessage = "❌ Please select an item to receive.";
            return;
        }

        if (OrderQuantity <= 0)
        {
            StatusMessage = "❌ Quantity must be greater than 0.";
            return;
        }

        var company = _simulation.Engine.Company;
        var item = company.Inventory.FirstOrDefault(i => i.Name == SelectedItem);

        if (item == null)
        {
            StatusMessage = "❌ Item not found in inventory.";
            return;
        }

        decimal totalCost = item.UnitCost * OrderQuantity;

        if (company.Cash < totalCost)
        {
            StatusMessage = $"❌ Insufficient cash. Need ${totalCost:N0}.";
            return;
        }

        company.Cash -= totalCost;
        company.Expenses += totalCost;
        item.Quantity += OrderQuantity;

        string[] suppliers = ["GlobalSupply Co.", "FastStock Ltd.", "BulkMart Inc.", "PrimeMaterials"];

        _history.Insert(0, new StockInRecordModel
        {
            ItemName = item.Name,
            Quantity = OrderQuantity,
            UnitCost = item.UnitCost,
            TotalCost = totalCost,
            ReceivedAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
            Supplier = suppliers[Random.Shared.Next(suppliers.Length)],
        });

        if (_history.Count > 30) _history.RemoveAt(_history.Count - 1);

        StatusMessage = $"✅ Received {OrderQuantity}x {item.Name}. Cost: ${totalCost:N0}.";
        Refresh();
    }

    [RelayCommand]
    private void RestockAll()
    {
        var company = _simulation.Engine.Company;
        int restocked = 0;

        foreach (var item in company.Inventory.Where(i => i.Quantity < i.MinimumStock))
        {
            int needed = item.MinimumStock - item.Quantity + 50;
            decimal cost = needed * item.UnitCost;

            if (company.Cash < cost) continue;

            company.Cash -= cost;
            company.Expenses += cost;
            item.Quantity += needed;

            _history.Insert(0, new StockInRecordModel
            {
                ItemName = item.Name,
                Quantity = needed,
                UnitCost = item.UnitCost,
                TotalCost = cost,
                ReceivedAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
                Supplier = "Auto-Restock",
            });

            restocked++;
        }

        StatusMessage = restocked > 0
            ? $"✅ Restocked {restocked} item(s) to minimum levels."
            : "ℹ All items are already above minimum stock.";

        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;
        TotalUnitsReceived = _history.Sum(h => h.Quantity);
        TotalSpent = _history.Sum(h => h.TotalCost);

        AvailableItems.Clear();
        foreach (var item in company.Inventory)
            AvailableItems.Add(item.Name);

        if (string.IsNullOrEmpty(SelectedItem) && AvailableItems.Count > 0)
            SelectedItem = AvailableItems[0];

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Units Received", Value = TotalUnitsReceived.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Total Spent", Value = $"${TotalSpent:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Company Cash", Value = $"${CompanyCash:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Item Types", Value = company.Inventory.Count.ToString() });

        CurrentStock.Clear();
        foreach (var item in company.Inventory)
        {
            double fill = item.MaximumStock > 0
                ? Math.Clamp((double)item.Quantity / item.MaximumStock, 0, 1)
                : 0;

            CurrentStock.Add(new InventoryRowModel
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

        History.Clear();
        foreach (var h in _history)
            History.Add(h);
    }
}