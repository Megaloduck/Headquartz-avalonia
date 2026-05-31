using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class MaintenanceLogEntry : ObservableObject
{
    public string Description { get; set; } = "";
    public string Timestamp { get; set; } = "";
    public string Type { get; set; } = "";
}

public partial class ProductionMaintenanceViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;
    [ObservableProperty] private int _staffCount;
    [ObservableProperty] private bool _isCritical;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<MaintenanceLogEntry> Log { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public ProductionMaintenanceViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(RefreshStats);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void LightMaintenance()
    {
        const decimal cost = 1_000m;
        var company = _simulation.Engine.Company;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash for light maintenance.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        var dept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        if (dept != null)
        {
            dept.StressLevel = Math.Max(0, dept.StressLevel - 15);
            dept.Efficiency = Math.Min(100, dept.Efficiency + 3);
        }

        Log.Insert(0, new MaintenanceLogEntry
        {
            Description = "Light maintenance completed. −15% stress, +3% efficiency.",
            Timestamp = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
            Type = "Light",
        });

        StatusMessage = "✅ Light maintenance done. Cost: $1,000.";
        RefreshStats();
    }

    [RelayCommand]
    private void FullOverhaul()
    {
        const decimal cost = 5_000m;
        var company = _simulation.Engine.Company;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash for full overhaul.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        var dept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        if (dept != null)
        {
            dept.StressLevel = Math.Max(0, dept.StressLevel - 40);
            dept.Efficiency = Math.Min(100, dept.Efficiency + 10);
        }

        // Also create a maintenance task (production paused briefly)
        _simulation.Engine.Commands.Enqueue(new CreateTaskCommand
        {
            PlayerId = "production-manager",
            Name = "Full Overhaul",
            Description = "Complete production line overhaul",
            Department = DepartmentType.Production,
            Priority = TaskPriority.High,
            DurationTicks = 4,
            BudgetCost = 0m,
        });

        Log.Insert(0, new MaintenanceLogEntry
        {
            Description = "Full overhaul scheduled. −40% stress, +10% efficiency.",
            Timestamp = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
            Type = "Overhaul",
        });

        StatusMessage = "✅ Full overhaul initiated. Cost: $5,000.";
        RefreshStats();
    }

    [RelayCommand]
    private void EmergencyRepair()
    {
        const decimal cost = 2_500m;
        var company = _simulation.Engine.Company;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash for emergency repair.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        var dept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        if (dept != null)
        {
            dept.StressLevel = Math.Max(0, dept.StressLevel - 25);
            dept.Efficiency = Math.Min(100, dept.Efficiency + 5);
            dept.IsOperational = true;
        }

        // Resolve one production event
        var ev = company.Events
            .FirstOrDefault(e =>
                e.Department == DepartmentType.Production && !e.IsResolved);
        if (ev != null) ev.IsResolved = true;

        Log.Insert(0, new MaintenanceLogEntry
        {
            Description = "Emergency repair done. Department restored to operational.",
            Timestamp = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
            Type = "Emergency",
        });

        StatusMessage = "✅ Emergency repair complete. Cost: $2,500.";
        RefreshStats();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        RefreshStats();
    }

    private void RefreshStats()
    {
        var company = _simulation.Engine.Company;

        var dept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        DeptEfficiency = dept?.Efficiency ?? 0;
        DeptStress = dept?.StressLevel ?? 0;
        IsCritical = DeptStress > 75;
        CompanyCash = company.Cash;

        StaffCount = company.Employees
            .Count(e => e.Department == DepartmentType.Production);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });
        Kpis.Add(new KpiCardModel { Title = "Staff", Value = StaffCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });
    }
}