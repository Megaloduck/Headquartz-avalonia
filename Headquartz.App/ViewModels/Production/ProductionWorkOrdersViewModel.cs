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

public partial class ProductionWorkOrdersViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Filters ───────────────────────────────────────────────

    [ObservableProperty] private string _activeFilter = "Active";

    public ObservableCollection<string> Filters { get; } =
    [
        "Active", "All", "Blocked", "Completed"
    ];

    // ── Counts ────────────────────────────────────────────────

    [ObservableProperty] private int _totalActive;
    [ObservableProperty] private int _totalBlocked;
    [ObservableProperty] private int _totalCompleted;
    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<TaskRowModel> Tasks { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public ProductionWorkOrdersViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    partial void OnActiveFilterChanged(string value) => Refresh();

    [RelayCommand]
    private void SetFilter(string filter) => ActiveFilter = filter;

    [RelayCommand]
    private void AddWorkOrder()
    {
        _simulation.Engine.Commands.Enqueue(new CreateTaskCommand
        {
            PlayerId = "production-manager",
            Name = "Production Batch",
            Description = "Manual work order",
            Department = DepartmentType.Production,
            Priority = TaskPriority.High,
            DurationTicks = 6,
            BudgetCost = 2_500m,
        });
    }

    [RelayCommand]
    private void CancelTask(TaskRowModel row)
    {
        var task = _simulation.Engine.Company.Tasks
            .FirstOrDefault(t => t.Id == row.TaskId);

        if (task == null) return;

        task.Status = CompanyTaskStatus.Cancelled;

        // Free up any assigned employees
        foreach (var emp in _simulation.Engine.Company.Employees
                     .Where(e => e.Department == DepartmentType.Production))
            emp.IsAssigned = false;

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

        var all = company.Tasks
            .Where(t => t.Department == DepartmentType.Production)
            .ToList();

        TotalActive = all.Count(t =>
            t.Status is not CompanyTaskStatus.Completed
                     and not CompanyTaskStatus.Cancelled);
        TotalBlocked = all.Count(t => t.IsBlocked);
        TotalCompleted = all.Count(t => t.Status == CompanyTaskStatus.Completed);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Active", Value = TotalActive.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Blocked", Value = TotalBlocked.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Completed", Value = TotalCompleted.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });

        var source = ActiveFilter switch
        {
            "Blocked" => all.Where(t => t.IsBlocked),
            "Completed" => all.Where(t => t.Status == CompanyTaskStatus.Completed),
            "Active" => all.Where(t =>
                t.Status is not CompanyTaskStatus.Completed
                          and not CompanyTaskStatus.Cancelled),
            _ => all.AsEnumerable(),
        };

        Tasks.Clear();
        foreach (var task in source.OrderByDescending(t => t.Priority).Take(40))
        {
            Tasks.Add(new TaskRowModel
            {
                TaskId = task.Id,
                Name = task.Name,
                Priority = task.Priority.ToString(),
                Status = task.IsBlocked ? "Blocked" : task.Status.ToString(),
                Progress = task.Progress,
                RemainingTicks = task.RemainingTicks,
                Workers = task.AssignedEmployees,
                IsBlocked = task.IsBlocked,
            });
        }
    }
}