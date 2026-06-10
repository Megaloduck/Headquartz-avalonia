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

namespace Headquartz.App.ViewModels;

public partial class StockOutRecordModel : ObservableObject
{
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public string Reason { get; set; } = "";
    public string IssuedAt { get; set; } = "";
    public string IssuedTo { get; set; } = "";
}

public partial class WarehouseStockOutViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<StockOutRecordModel> _history = [];

    [ObservableProperty] private int _totalUnitsIssued;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _selectedItem = "";
    [ObservableProperty] private int _issueQuantity = 10;
    [ObservableProperty] private string _issueReason = "Production";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<StockOutRecordModel> History { get; } = [];
    public ObservableCollection<InventoryRowModel> CurrentStock { get; } = [];
    public ObservableCollection<string> AvailableItems { get; } = [];
    public ObservableCollection<string> IssueReasons { get; } =
    [
        "Production", "Quality Testing", "Write-Off", "Returns", "Transfer", "Disposal"
    ];

    public WarehouseStockOutViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void IssueStock()
    {
        if (string.IsNullOrWhiteSpace(SelectedItem))
        {
            StatusMessage = "❌ Please select an item.";
            return;
        }

        if (IssueQuantity <= 0)
        {
            StatusMessage = "❌ Quantity must be greater than 0.";
            return;
        }

        var company = _simulation.Engine.Company;
        var item = company.Inventory.FirstOrDefault(i => i.Name == SelectedItem);

        if (item == null)
        {
            StatusMessage = "❌ Item not found.";
            return;
        }

        if (item.Quantity < IssueQuantity)
        {
            StatusMessage = $"❌ Insufficient stock. Only {item.Quantity} units available.";
            return;
        }

        item.Quantity -= IssueQuantity;

        string[] departments = ["Production", "Logistics", "Sales", "Quality Control"];

        _history.Insert(0, new StockOutRecordModel
        {
            ItemName = item.Name,
            Quantity = IssueQuantity,
            Reason = IssueReason,
            IssuedAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
            IssuedTo = departments[Random.Shared.Next(departments.Length)],
        });

        if (_history.Count > 30) _history.RemoveAt(_history.Count - 1);

        StatusMessage = $"✅ Issued {IssueQuantity}x {item.Name} for {IssueReason}.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;
        TotalUnitsIssued = _history.Sum(h => h.Quantity);

        AvailableItems.Clear();
        foreach (var item in company.Inventory)
            AvailableItems.Add(item.Name);

        if (string.IsNullOrEmpty(SelectedItem) && AvailableItems.Count > 0)
            SelectedItem = AvailableItems[0];

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Units Issued", Value = TotalUnitsIssued.ToString() });
        Kpis.Add(new KpiCardModel
        {
            Title = "Low Stock Items",
            Value = company.Inventory.Count(i => i.Quantity < i.MinimumStock).ToString()
        });
        Kpis.Add(new KpiCardModel { Title = "Total Units", Value = company.Inventory.Sum(i => i.Quantity).ToString() });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

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