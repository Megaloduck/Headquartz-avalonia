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

public partial class ProductionDashboardViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private int _activeTasks;
    [ObservableProperty] private int _completedTasks;
    [ObservableProperty] private int _blockedTasks;
    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;
    [ObservableProperty] private bool _isDeptCritical;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<TaskRowModel> WorkOrders { get; } = [];
    public ObservableCollection<EventViewModel> Events { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public ProductionDashboardViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void StartProductionRun()
    {
        var cmd = new CreateTaskCommand
        {
            PlayerId = "production-manager",
            Name = "Production Batch",
            Description = "Manual production run started by manager",
            Department = DepartmentType.Production,
            Priority = Domain.Enums.TaskPriority.High,
            DurationTicks = 8,
            BudgetCost = 3_000m,
        };

        _simulation.Engine.Commands.Enqueue(cmd);
    }

    [RelayCommand]
    private void ScheduleMaintenance()
    {
        // Reduces dept stress directly
        var dept = _simulation.Engine.Company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        if (dept != null)
        {
            dept.StressLevel = Math.Max(0, dept.StressLevel - 20);
            dept.Efficiency = Math.Min(100, dept.Efficiency + 5);
        }

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        var prodDept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        DeptEfficiency = prodDept?.Efficiency ?? 0;
        DeptStress = prodDept?.StressLevel ?? 0;
        IsDeptCritical = DeptStress > 75;

        var prodTasks = company.Tasks
            .Where(t => t.Department == DepartmentType.Production)
            .ToList();

        ActiveTasks = prodTasks.Count(t => t.Status != Domain.Enums.CompanyTaskStatus.Completed);
        CompletedTasks = prodTasks.Count(t => t.Status == Domain.Enums.CompanyTaskStatus.Completed);
        BlockedTasks = prodTasks.Count(t => t.IsBlocked);

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Active Orders", Value = ActiveTasks.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Completed", Value = CompletedTasks.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Blocked", Value = BlockedTasks.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });

        // Work orders
        WorkOrders.Clear();
        foreach (var task in prodTasks
                     .OrderByDescending(t => t.Priority)
                     .Take(20))
        {
            WorkOrders.Add(new TaskRowModel
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
                     .Where(e => e.Department == DepartmentType.Production && !e.IsResolved)
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