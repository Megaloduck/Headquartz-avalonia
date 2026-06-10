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

public partial class ProductionLineModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Product { get; set; } = "";
    [ObservableProperty] private int _efficiency;
    [ObservableProperty] private int _stressLevel;
    [ObservableProperty] private bool _isOperational;
    [ObservableProperty] private int _assignedWorkers;
    [ObservableProperty] private int _outputPerTick;
    [ObservableProperty] private string _status = "Idle";
}

public partial class ProductionLinesViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<ProductionLineModel> _lines = [];

    [ObservableProperty] private int _totalLines;
    [ObservableProperty] private int _activeLines;
    [ObservableProperty] private int _idleLines;
    [ObservableProperty] private int _offlineLines;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<ProductionLineModel> Lines { get; } = [];

    public ProductionLinesViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_lines.Count == 0)
            SeedLines();

        Refresh();
    }

    [RelayCommand]
    private void ActivateLine(ProductionLineModel line)
    {
        if (line.Status == "Active")
        {
            StatusMessage = $"⚠ Line {line.Name} is already active.";
            return;
        }

        line.Status = "Active";
        line.IsOperational = true;
        StatusMessage = $"✅ Line {line.Name} activated.";
        Refresh();
    }

    [RelayCommand]
    private void ShutdownLine(ProductionLineModel line)
    {
        line.Status = "Offline";
        line.IsOperational = false;
        StatusMessage = $"⛔ Line {line.Name} shut down.";
        Refresh();
    }

    [RelayCommand]
    private void BoostLine(ProductionLineModel line)
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 2_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to boost line. Need $2,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        line.Efficiency = Math.Min(100, line.Efficiency + 15);
        line.StressLevel = Math.Max(0, line.StressLevel - 10);
        line.OutputPerTick = Math.Min(50, line.OutputPerTick + 5);

        StatusMessage = $"✅ Line {line.Name} boosted. Efficiency +15, Stress −10.";
        Refresh();
    }

    [RelayCommand]
    private void AddLine()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 15_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to add production line. Need $15,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        string[] products = ["Industrial Widget", "Smart Controller", "Metal Housing", "Control Board", "Sensor Module"];

        _lines.Add(new ProductionLineModel
        {
            Name = $"Line {_lines.Count + 1}",
            Product = products[Random.Shared.Next(products.Length)],
            Efficiency = 50,
            StressLevel = 0,
            IsOperational = true,
            AssignedWorkers = 0,
            OutputPerTick = 10,
            Status = "Active",
        });

        StatusMessage = $"✅ New production line added. Cost: $15,000.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        // Sync worker counts from simulation
        foreach (var line in _lines)
        {
            line.AssignedWorkers = company.Employees
                .Count(e => e.Department == DepartmentType.Production && e.IsAssigned);
        }

        TotalLines = _lines.Count;
        ActiveLines = _lines.Count(l => l.Status == "Active");
        IdleLines = _lines.Count(l => l.Status == "Idle");
        OfflineLines = _lines.Count(l => l.Status == "Offline");

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Lines", Value = TotalLines.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Active", Value = ActiveLines.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Idle", Value = IdleLines.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Offline", Value = OfflineLines.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Company Cash", Value = $"${CompanyCash:N0}" });

        Lines.Clear();
        foreach (var l in _lines)
            Lines.Add(l);
    }

    private void SeedLines()
    {
        string[] products = ["Industrial Widget", "Smart Controller", "Metal Housing"];

        for (int i = 0; i < 3; i++)
        {
            _lines.Add(new ProductionLineModel
            {
                Name = $"Line {i + 1}",
                Product = products[i],
                Efficiency = Random.Shared.Next(40, 85),
                StressLevel = Random.Shared.Next(0, 50),
                IsOperational = i < 2,
                AssignedWorkers = Random.Shared.Next(0, 4),
                OutputPerTick = Random.Shared.Next(5, 20),
                Status = i == 2 ? "Idle" : "Active",
            });
        }
    }
}