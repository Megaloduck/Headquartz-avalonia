using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class FlowEventModel : ObservableObject
{
    public string Direction { get; set; } = "";
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
    public string Source { get; set; } = "";
    public string Destination { get; set; } = "";
    public string Timestamp { get; set; } = "";
}

public partial class WarehouseFlowViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<FlowEventModel> _flowLog = [];

    [ObservableProperty] private int _inboundToday;
    [ObservableProperty] private int _outboundToday;
    [ObservableProperty] private int _totalMovements;
    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<FlowEventModel> FlowLog { get; } = [];
    public ObservableCollection<InventoryRowModel> Inventory { get; } = [];

    public WarehouseFlowViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void OptimizeFlow()
    {
        var company = _simulation.Engine.Company;
        var dept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Warehouse);

        if (dept != null)
        {
            dept.Efficiency = Math.Min(100, dept.Efficiency + 10);
            dept.StressLevel = Math.Max(0, dept.StressLevel - 10);
        }

        _flowLog.Insert(0, new FlowEventModel
        {
            Direction = "Optimization",
            ItemName = "All Items",
            Quantity = 0,
            Source = "Warehouse",
            Destination = "Warehouse",
            Timestamp = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
        });

        StatusMessage = "✅ Flow optimized. Efficiency +10, Stress −10.";
        Refresh();
    }

    [RelayCommand]
    private void ClearBottlenecks()
    {
        var company = _simulation.Engine.Company;

        // Resolve warehouse events
        int resolved = 0;
        foreach (var ev in company.Events
                     .Where(e => e.Department == DepartmentType.Warehouse && !e.IsResolved)
                     .Take(2))
        {
            ev.IsResolved = true;
            resolved++;
        }

        StatusMessage = resolved > 0
            ? $"✅ Cleared {resolved} bottleneck event(s)."
            : "ℹ No active bottlenecks found.";

        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        var whDept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Warehouse);

        DeptEfficiency = whDept?.Efficiency ?? 0;
        DeptStress = whDept?.StressLevel ?? 0;

        // Simulate flow metrics from inventory changes
        InboundToday = Random.Shared.Next(10, 80);
        OutboundToday = Random.Shared.Next(5, 60);
        TotalMovements = _flowLog.Count;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Inbound", Value = InboundToday.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Outbound", Value = OutboundToday.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Total Movements", Value = TotalMovements.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });

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

        FlowLog.Clear();
        foreach (var f in _flowLog.Take(20))
            FlowLog.Add(f);
    }
}