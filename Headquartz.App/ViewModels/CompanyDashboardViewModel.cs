using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class CompanyDashboardViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Financials ────────────────────────────────────────────

    [ObservableProperty] private decimal _cash;
    [ObservableProperty] private decimal _revenue;
    [ObservableProperty] private decimal _expenses;
    [ObservableProperty] private int _reputation;
    [ObservableProperty] private int _employeeCount;
    [ObservableProperty] private int _activeTasks;
    [ObservableProperty] private int _activeOrders;
    [ObservableProperty] private long _tick;
    [ObservableProperty] private string _worldTime = "";
    [ObservableProperty] private int _companyHealth;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<DepartmentHealthModel> DepartmentHealth { get; } = [];
    public ObservableCollection<EventViewModel> RecentAlerts { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public CompanyDashboardViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var engine = _simulation.Engine;
        var company = engine.Company;

        Cash = company.Cash;
        Revenue = company.Revenue;
        Expenses = company.Expenses;
        Reputation = company.Reputation;
        EmployeeCount = company.Employees.Count;
        ActiveTasks = company.Tasks.Count(t =>
            t.Status != Domain.Enums.CompanyTaskStatus.Completed);
        ActiveOrders = company.Orders.Count(o =>
            o.Status != Domain.Enums.OrderStatus.Delivered &&
            o.Status != Domain.Enums.OrderStatus.Cancelled);
        Tick = engine.Clock.Tick;
        WorldTime = engine.Clock.WorldTime.ToString("yyyy-MM-dd HH:mm");

        // Overall health score: avg efficiency minus avg stress,
        // normalised to 0-100
        if (company.Departments.Count > 0)
        {
            double avgEff = company.Departments.Average(d => d.Efficiency);
            double avgStress = company.Departments.Average(d => d.StressLevel);
            CompanyHealth = (int)Math.Clamp((avgEff - avgStress + 100) / 2, 0, 100);
        }

        // Department health rows
        DepartmentHealth.Clear();
        foreach (var dept in company.Departments)
        {
            int staff = company.Employees.Count(e => e.Department == dept.Type);

            DepartmentHealth.Add(new DepartmentHealthModel
            {
                Name = dept.Type.ToString(),
                Emoji = DeptEmoji(dept.Type),
                Efficiency = dept.Efficiency,
                StressLevel = dept.StressLevel,
                IsOperational = dept.IsOperational,
                StaffCount = staff,
                StatusText = dept.StressLevel switch
                {
                    > 80 => "⚠ Critical",
                    > 50 => "~ Stressed",
                    _ => "✓ Stable",
                },
            });
        }

        // Recent high/critical alerts across all departments
        RecentAlerts.Clear();
        foreach (var ev in company.Events
                     .Where(e => !e.IsResolved &&
                                 (e.Severity == Domain.Enums.EventSeverity.High ||
                                  e.Severity == Domain.Enums.EventSeverity.Critical))
                     .OrderByDescending(e => e.Severity)
                     .Take(8))
        {
            RecentAlerts.Add(new EventViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Severity = ev.Severity,
                RemainingTicks = ev.RemainingTicks,
            });
        }
    }

    private static string DeptEmoji(DepartmentType type) => type switch
    {
        DepartmentType.HumanResources => "👥",
        DepartmentType.Finance => "💰",
        DepartmentType.Sales => "📈",
        DepartmentType.Marketing => "📣",
        DepartmentType.Production => "🏭",
        DepartmentType.Warehouse => "📦",
        DepartmentType.Logistics => "🚚",
        _ => "🏢",
    };
}