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

public partial class RouteModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    [ObservableProperty] private int _efficiency;
    [ObservableProperty] private int _activeShipments;
    [ObservableProperty] private string _status = "Active";
    [ObservableProperty] private decimal _costPerShipment;
    public string EstimatedTime { get; set; } = "";
}

public partial class LogisticsRoutesViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<RouteModel> _routes = [];

    [ObservableProperty] private int _totalRoutes;
    [ObservableProperty] private int _activeRoutes;
    [ObservableProperty] private int _totalActiveShipments;
    [ObservableProperty] private decimal _avgCostPerShipment;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<RouteModel> Routes { get; } = [];

    public LogisticsRoutesViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_routes.Count == 0)
            SeedRoutes();

        Refresh();
    }

    [RelayCommand]
    private void OptimizeRoute(RouteModel route)
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 1_500m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to optimize route. Need $1,500.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        route.Efficiency = Math.Min(100, route.Efficiency + 15);
        route.CostPerShipment = Math.Max(100m, route.CostPerShipment * 0.90m);

        StatusMessage = $"✅ Route '{route.Name}' optimized. Efficiency +15, Cost −10%. Cost: $1,500.";
        Refresh();
    }

    [RelayCommand]
    private void SuspendRoute(RouteModel route)
    {
        route.Status = route.Status == "Suspended" ? "Active" : "Suspended";
        StatusMessage = route.Status == "Suspended"
            ? $"⛔ Route '{route.Name}' suspended."
            : $"✅ Route '{route.Name}' reactivated.";
        Refresh();
    }

    [RelayCommand]
    private void AddRoute()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 8_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to add route. Need $8,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        string[] origins = ["Warehouse Hub A", "Warehouse Hub B", "Central Depot"];
        string[] destinations = ["North Region", "South Region", "East Region", "West Region", "Metro Zone"];
        string[] times = ["2h", "4h", "6h", "8h", "12h"];

        _routes.Add(new RouteModel
        {
            Name = $"Route {_routes.Count + 1}",
            Origin = origins[Random.Shared.Next(origins.Length)],
            Destination = destinations[Random.Shared.Next(destinations.Length)],
            Efficiency = Random.Shared.Next(50, 80),
            ActiveShipments = 0,
            Status = "Active",
            CostPerShipment = Random.Shared.Next(200, 600),
            EstimatedTime = times[Random.Shared.Next(times.Length)],
        });

        StatusMessage = "✅ New route added. Cost: $8,000.";
        Refresh();
    }

    [RelayCommand]
    private void OptimizeAllRoutes()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 5_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash. Need $5,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        var dept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Logistics);

        if (dept != null)
        {
            dept.Efficiency = Math.Min(100, dept.Efficiency + 10);
            dept.StressLevel = Math.Max(0, dept.StressLevel - 15);
        }

        foreach (var route in _routes.Where(r => r.Status == "Active"))
        {
            route.Efficiency = Math.Min(100, route.Efficiency + 10);
            route.CostPerShipment = Math.Max(100m, route.CostPerShipment * 0.95m);
        }

        StatusMessage = "✅ All routes optimized. Dept Efficiency +10, Stress −15. Cost: $5,000.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        // Sync active shipments across routes
        int totalShipping = company.Orders.Count(o => o.Status == OrderStatus.Shipping);
        int routeCount = _routes.Count(r => r.Status == "Active");

        if (routeCount > 0)
        {
            int perRoute = totalShipping / routeCount;
            foreach (var route in _routes.Where(r => r.Status == "Active"))
                route.ActiveShipments = perRoute;
        }

        TotalRoutes = _routes.Count;
        ActiveRoutes = _routes.Count(r => r.Status == "Active");
        TotalActiveShipments = totalShipping;
        AvgCostPerShipment = _routes.Count > 0
            ? _routes.Average(r => r.CostPerShipment)
            : 0;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Routes", Value = TotalRoutes.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Active Routes", Value = ActiveRoutes.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Shipments", Value = TotalActiveShipments.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Avg Cost/Ship", Value = $"${AvgCostPerShipment:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

        Routes.Clear();
        foreach (var r in _routes)
            Routes.Add(r);
    }

    private void SeedRoutes()
    {
        var seed = new[]
        {
            ("Route A", "Warehouse Hub A", "North Region", 75, "4h", 350m),
            ("Route B", "Warehouse Hub A", "South Region", 65, "6h", 420m),
            ("Route C", "Central Depot",   "Metro Zone",   80, "2h", 280m),
        };

        foreach (var (name, origin, dest, eff, time, cost) in seed)
        {
            _routes.Add(new RouteModel
            {
                Name = name,
                Origin = origin,
                Destination = dest,
                Efficiency = eff,
                ActiveShipments = 0,
                Status = "Active",
                CostPerShipment = cost,
                EstimatedTime = time,
            });
        }
    }
}