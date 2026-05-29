using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Commands;

namespace Headquartz.App.ViewModels;

public partial class MarketingDashboardViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private int _activeCampaigns;
    [ObservableProperty] private int _reputation;
    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;
    [ObservableProperty] private bool _isDeptCritical;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<TaskRowModel> Campaigns { get; } = [];
    public ObservableCollection<EventViewModel> Events { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public MarketingDashboardViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void LaunchCampaign()
    {
        var cmd = new CreateTaskCommand
        {
            PlayerId = "marketing-manager",
            Name = "Marketing Campaign",
            Description = "Brand awareness campaign launched",
            Department = DepartmentType.Marketing,
            Priority = Domain.Enums.TaskPriority.High,
            DurationTicks = 6,
            BudgetCost = 2_000m,
        };

        _simulation.Engine.Commands.Enqueue(cmd);
    }

    [RelayCommand]
    private void BoostBrand()
    {
        // Direct reputation boost — costs cash
        var company = _simulation.Engine.Company;

        if (company.Cash < 5_000m) return;

        company.Cash -= 5_000m;
        company.Expenses += 5_000m;
        company.Reputation = Math.Min(100, company.Reputation + 3);

        Refresh();
    }

    [RelayCommand]
    private void RunPRCampaign()
    {
        // Resolves one unresolved event in any department
        var ev = _simulation.Engine.Company.Events
            .FirstOrDefault(e => !e.IsResolved);

        if (ev != null)
            ev.IsResolved = true;

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        var mktDept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Marketing);

        Reputation = company.Reputation;
        DeptEfficiency = mktDept?.Efficiency ?? 0;
        DeptStress = mktDept?.StressLevel ?? 0;
        IsDeptCritical = DeptStress > 75;

        var mktTasks = company.Tasks
            .Where(t => t.Department == DepartmentType.Marketing)
            .ToList();

        ActiveCampaigns = mktTasks
            .Count(t => t.Status != Domain.Enums.CompanyTaskStatus.Completed);

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Active Campaigns", Value = ActiveCampaigns.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Reputation", Value = $"{Reputation}/100" });
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });

        // Campaigns
        Campaigns.Clear();
        foreach (var task in mktTasks
                     .OrderByDescending(t => t.Priority)
                     .Take(15))
        {
            Campaigns.Add(new TaskRowModel
            {
                Name = task.Name,
                Priority = task.Priority.ToString(),
                Status = task.IsBlocked ? "Blocked" : task.Status.ToString(),
                Progress = task.Progress,
                RemainingTicks = task.RemainingTicks,
                Workers = task.AssignedEmployees,
                IsBlocked = task.IsBlocked,
            });
        }

        // Events
        Events.Clear();
        foreach (var ev in company.Events
                     .Where(e => e.Department == DepartmentType.Marketing && !e.IsResolved)
                     .OrderByDescending(e => e.Severity))
        {
            Events.Add(new EventViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Severity = ev.Severity,
                RemainingTicks = ev.RemainingTicks,
            });
        }
    }
}