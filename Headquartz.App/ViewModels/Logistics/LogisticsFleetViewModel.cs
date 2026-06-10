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

public partial class VehicleModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    [ObservableProperty] private int _condition;
    [ObservableProperty] private string _status = "Available";
    [ObservableProperty] private int _activeDeliveries;
    public decimal MaintenanceCost { get; set; }
    public string LastServiced { get; set; } = "";
}

public partial class LogisticsFleetViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<VehicleModel> _fleet = [];

    [ObservableProperty] private int _totalVehicles;
    [ObservableProperty] private int _availableVehicles;
    [ObservableProperty] private int _inUseVehicles;
    [ObservableProperty] private int _maintenanceVehicles;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<VehicleModel> Fleet { get; } = [];

    public LogisticsFleetViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_fleet.Count == 0)
            SeedFleet();

        Refresh();
    }

    [RelayCommand]
    private void ServiceVehicle(VehicleModel vehicle)
    {
        var company = _simulation.Engine.Company;

        if (company.Cash < vehicle.MaintenanceCost)
        {
            StatusMessage = $"❌ Insufficient cash. Need ${vehicle.MaintenanceCost:N0}.";
            return;
        }

        company.Cash -= vehicle.MaintenanceCost;
        company.Expenses += vehicle.MaintenanceCost;

        vehicle.Condition = Math.Min(100, vehicle.Condition + 30);
        vehicle.Status = "Available";
        vehicle.LastServiced = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm");

        StatusMessage = $"✅ {vehicle.Name} serviced. Condition +30. Cost: ${vehicle.MaintenanceCost:N0}.";
        Refresh();
    }

    [RelayCommand]
    private void RetireVehicle(VehicleModel vehicle)
    {
        _fleet.Remove(vehicle);
        StatusMessage = $"✅ {vehicle.Name} retired from fleet.";
        Refresh();
    }

    [RelayCommand]
    private void AddVehicle()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 12_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash. Need $12,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        string[] types = ["Van", "Truck", "Heavy Truck", "Motorcycle", "Cargo Truck"];

        _fleet.Add(new VehicleModel
        {
            Name = $"Vehicle {_fleet.Count + 1}",
            Type = types[Random.Shared.Next(types.Length)],
            Condition = 100,
            Status = "Available",
            ActiveDeliveries = 0,
            MaintenanceCost = Random.Shared.Next(500, 2000),
            LastServiced = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
        });

        StatusMessage = "✅ New vehicle added to fleet. Cost: $12,000.";
        Refresh();
    }

    [RelayCommand]
    private void ServiceAll()
    {
        var company = _simulation.Engine.Company;
        decimal totalCost = _fleet.Sum(v => v.MaintenanceCost);

        if (company.Cash < totalCost)
        {
            StatusMessage = $"❌ Insufficient cash to service all. Need ${totalCost:N0}.";
            return;
        }

        company.Cash -= totalCost;
        company.Expenses += totalCost;

        foreach (var v in _fleet)
        {
            v.Condition = Math.Min(100, v.Condition + 30);
            v.Status = "Available";
            v.LastServiced = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm");
        }

        StatusMessage = $"✅ All vehicles serviced. Total cost: ${totalCost:N0}.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        // Degrade vehicle condition over time
        foreach (var v in _fleet)
        {
            v.Condition = Math.Max(0, v.Condition - 1);
            if (v.Condition < 20)
                v.Status = "Maintenance Required";
        }

        // Distribute active shipments across available vehicles
        int shipping = company.Orders.Count(o => o.Status == OrderStatus.Shipping);
        var available = _fleet.Where(v => v.Status == "Available").ToList();

        foreach (var v in available)
            v.ActiveDeliveries = 0;

        int idx = 0;
        for (int i = 0; i < shipping && available.Count > 0; i++)
        {
            available[idx % available.Count].ActiveDeliveries++;
            idx++;
        }

        TotalVehicles = _fleet.Count;
        AvailableVehicles = _fleet.Count(v => v.Status == "Available");
        InUseVehicles = _fleet.Count(v => v.ActiveDeliveries > 0);
        MaintenanceVehicles = _fleet.Count(v => v.Status == "Maintenance Required");

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Vehicles", Value = TotalVehicles.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Available", Value = AvailableVehicles.ToString() });
        Kpis.Add(new KpiCardModel { Title = "In Use", Value = InUseVehicles.ToString() });
        Kpis.Add(new KpiCardModel
        {
            Title = "Need Service",
            Value = MaintenanceVehicles.ToString()
        });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

        Fleet.Clear();
        foreach (var v in _fleet.OrderBy(v => v.Condition))
            Fleet.Add(v);
    }

    private void SeedFleet()
    {
        var seed = new[]
        {
            ("Van 1",       "Van",         85, 800m),
            ("Truck 1",     "Truck",       70, 1_200m),
            ("Heavy Truck", "Heavy Truck", 60, 1_800m),
            ("Van 2",       "Van",         90, 750m),
        };

        foreach (var (name, type, condition, maint) in seed)
        {
            _fleet.Add(new VehicleModel
            {
                Name = name,
                Type = type,
                Condition = condition,
                Status = condition < 20 ? "Maintenance Required" : "Available",
                ActiveDeliveries = 0,
                MaintenanceCost = maint,
                LastServiced = "01/01",
            });
        }
    }
}