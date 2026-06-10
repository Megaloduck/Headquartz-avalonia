using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class ResourceAllocationModel : ObservableObject
{
    public string ResourceName { get; set; } = "";
    public string Type { get; set; } = "";
    public int Available { get; set; }
    public int Allocated { get; set; }
    public int Required { get; set; }
    public string StatusText { get; set; } = "";
    public bool IsShortfall { get; set; }
}

public partial class ProductionResourcesViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _totalWorkers;
    [ObservableProperty] private int _assignedWorkers;
    [ObservableProperty] private int _availableWorkers;
    [ObservableProperty] private int _activeTasks;
    [ObservableProperty] private int _blockedTasks;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<ResourceAllocationModel> Resources { get; } = [];

    public ProductionResourcesViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void ReassignWorkers()
    {
        var company = _simulation.Engine.Company;

        // Free all production workers then reassign optimally
        foreach (var emp in company.Employees
                     .Where(e => e.Department == DepartmentType.Production))
            emp.IsAssigned = false;

        var pendingTasks = company.Tasks
            .Where(t => t.Department == DepartmentType.Production &&
                        t.Status != CompanyTaskStatus.Completed &&
                        t.Status != CompanyTaskStatus.Cancelled)
            .OrderByDescending(t => t.Priority)
            .ToList();

        var availableWorkers = company.Employees
            .Where(e => e.Department == DepartmentType.Production && !e.IsAssigned)
            .ToList();

        foreach (var task in pendingTasks)
        {
            if (!availableWorkers.Any()) break;

            int toAssign = Math.Min(task.RequiredEmployees, availableWorkers.Count);
            foreach (var emp in availableWorkers.Take(toAssign).ToList())
            {
                emp.IsAssigned = true;
                availableWorkers.Remove(emp);
            }

            task.AssignedEmployees = toAssign;
            task.IsBlocked = toAssign == 0;
        }

        StatusMessage = "✅ Workers reassigned to highest priority tasks.";
        Refresh();
    }

    [RelayCommand]
    private void RequestMoreWorkers()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 3_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to hire temp workers. Need $3,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        company.Employees.Add(new Headquartz.Domain.Entities.Employee
        {
            Id = Guid.NewGuid(),
            Name = "Temp Worker",
            Role = EmployeeRole.Worker,
            Department = DepartmentType.Production,
            Salary = 1_500m,
            Morale = 60,
            Productivity = 55,
        });

        StatusMessage = "✅ Temporary worker hired for Production. Cost: $3,000.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        var prodWorkers = company.Employees
            .Where(e => e.Department == DepartmentType.Production)
            .ToList();

        TotalWorkers = prodWorkers.Count;
        AssignedWorkers = prodWorkers.Count(e => e.IsAssigned);
        AvailableWorkers = TotalWorkers - AssignedWorkers;

        var prodTasks = company.Tasks
            .Where(t => t.Department == DepartmentType.Production)
            .ToList();

        ActiveTasks = prodTasks.Count(t =>
            t.Status != CompanyTaskStatus.Completed &&
            t.Status != CompanyTaskStatus.Cancelled);
        BlockedTasks = prodTasks.Count(t => t.IsBlocked);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Workers", Value = TotalWorkers.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Assigned", Value = AssignedWorkers.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Available", Value = AvailableWorkers.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Active Tasks", Value = ActiveTasks.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Blocked Tasks", Value = BlockedTasks.ToString() });

        Resources.Clear();

        // Workers
        Resources.Add(new ResourceAllocationModel
        {
            ResourceName = "Production Workers",
            Type = "Human",
            Available = TotalWorkers,
            Allocated = AssignedWorkers,
            Required = ActiveTasks,
            IsShortfall = AssignedWorkers < ActiveTasks,
            StatusText = AssignedWorkers < ActiveTasks ? "⚠ Understaffed" : "✓ Sufficient",
        });

        // Inventory materials
        foreach (var item in company.Inventory)
        {
            bool shortfall = item.Quantity < item.MinimumStock;
            Resources.Add(new ResourceAllocationModel
            {
                ResourceName = item.Name,
                Type = "Material",
                Available = item.Quantity,
                Allocated = Math.Max(0, item.Quantity - item.MinimumStock),
                Required = item.MinimumStock,
                IsShortfall = shortfall,
                StatusText = shortfall ? "⚠ Low Stock" : "✓ Adequate",
            });
        }

        // Budget
        var prodDept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        if (prodDept != null)
        {
            Resources.Add(new ResourceAllocationModel
            {
                ResourceName = "Production Budget",
                Type = "Financial",
                Available = (int)(prodDept.Budget / 1000),
                Allocated = (int)(prodDept.Budget * 0.7m / 1000),
                Required = (int)(prodDept.Budget / 1000),
                IsShortfall = prodDept.Budget < 5_000m,
                StatusText = prodDept.Budget < 5_000m ? "⚠ Low Budget" : "✓ Funded",
            });
        }
    }
}