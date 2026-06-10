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

public partial class StorageZoneModel : ObservableObject
{
    public string ZoneName { get; set; } = "";
    public string ZoneType { get; set; } = "";
    [ObservableProperty] private int _capacity;
    [ObservableProperty] private int _used;
    [ObservableProperty] private string _assignedItem = "";
    [ObservableProperty] private string _status = "Active";

    public double FillPercent => Capacity > 0
        ? Math.Clamp((double)Used / Capacity, 0, 1)
        : 0;
}

public partial class WarehouseStorageViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<StorageZoneModel> _zones = [];

    [ObservableProperty] private int _totalCapacity;
    [ObservableProperty] private int _usedCapacity;
    [ObservableProperty] private int _availableCapacity;
    [ObservableProperty] private int _zoneCount;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<StorageZoneModel> Zones { get; } = [];

    public WarehouseStorageViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_zones.Count == 0)
            SeedZones();

        Refresh();
    }

    [RelayCommand]
    private void AddZone()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 5_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to add storage zone. Need $5,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        string[] types = ["Bulk Storage", "Cold Storage", "Secure Storage", "Overflow"];

        _zones.Add(new StorageZoneModel
        {
            ZoneName = $"Zone {(char)('A' + _zones.Count)}",
            ZoneType = types[Random.Shared.Next(types.Length)],
            Capacity = 500,
            Used = 0,
            AssignedItem = "Unassigned",
            Status = "Active",
        });

        StatusMessage = "✅ New storage zone added. Cost: $5,000.";
        Refresh();
    }

    [RelayCommand]
    private void ReorganizeZone(StorageZoneModel zone)
    {
        var company = _simulation.Engine.Company;
        var dept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Warehouse);

        if (dept != null)
            dept.Efficiency = Math.Min(100, dept.Efficiency + 5);

        zone.Used = Math.Max(0, zone.Used - 20);

        StatusMessage = $"✅ Zone {zone.ZoneName} reorganized. Space freed.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        // Sync used capacity with actual inventory
        foreach (var zone in _zones)
        {
            var item = company.Inventory.FirstOrDefault(i => i.Name == zone.AssignedItem);
            if (item != null)
                zone.Used = Math.Min(zone.Capacity, item.Quantity);
        }

        TotalCapacity = _zones.Sum(z => z.Capacity);
        UsedCapacity = _zones.Sum(z => z.Used);
        AvailableCapacity = TotalCapacity - UsedCapacity;
        ZoneCount = _zones.Count;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Capacity", Value = TotalCapacity.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Used", Value = UsedCapacity.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Available", Value = AvailableCapacity.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Zones", Value = ZoneCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

        Zones.Clear();
        foreach (var z in _zones)
            Zones.Add(z);
    }

    private void SeedZones()
    {
        var items = new[] { "Steel", "Plastic", "Electronics" };
        var types = new[] { "Bulk Storage", "Bulk Storage", "Secure Storage" };

        for (int i = 0; i < 3; i++)
        {
            _zones.Add(new StorageZoneModel
            {
                ZoneName = $"Zone {(char)('A' + i)}",
                ZoneType = types[i],
                Capacity = 400 + i * 100,
                Used = Random.Shared.Next(50, 300),
                AssignedItem = items[i],
                Status = "Active",
            });
        }
    }
}