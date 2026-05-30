using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;

namespace Headquartz.App.ViewModels;

public partial class HREmployeeManagementViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _totalEmployees;
    [ObservableProperty] private int _atRiskCount;
    [ObservableProperty] private decimal _totalPayroll;
    [ObservableProperty] private string _filterDepartment = "All";

    public ObservableCollection<EmployeeRowModel> Employees { get; } = [];
    public ObservableCollection<string> DepartmentFilters { get; } =
    [
        "All", "HumanResources", "Finance", "Sales",
        "Marketing", "Production", "Warehouse", "Logistics"
    ];

    public HREmployeeManagementViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    partial void OnFilterDepartmentChanged(string value) => Refresh();

    [RelayCommand]
    private void FireEmployee(EmployeeRowModel row)
    {
        var emp = _simulation.Engine.Company.Employees
            .FirstOrDefault(e => e.Id == row.Id);

        if (emp == null) return;

        _simulation.Engine.Company.Employees.Remove(emp);
        _simulation.Engine.Company.Reputation =
            Math.Max(0, _simulation.Engine.Company.Reputation - 2);

        Refresh();
    }

    [RelayCommand]
    private void BoostMorale(EmployeeRowModel row)
    {
        var emp = _simulation.Engine.Company.Employees
            .FirstOrDefault(e => e.Id == row.Id);

        if (emp == null) return;

        const decimal cost = 500m;
        if (_simulation.Engine.Company.Cash < cost) return;

        _simulation.Engine.Company.Cash -= cost;
        _simulation.Engine.Company.Expenses += cost;
        emp.Morale = Math.Min(100, emp.Morale + 20);
        emp.Productivity = Math.Min(100, emp.Productivity + 10);

        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        var source = FilterDepartment == "All"
            ? company.Employees
            : company.Employees
                .Where(e => e.Department.ToString() == FilterDepartment)
                .ToList();

        TotalEmployees = company.Employees.Count;
        AtRiskCount = company.Employees.Count(e => e.Morale <= 15);
        TotalPayroll = company.Employees.Sum(e => e.Salary);

        Employees.Clear();
        foreach (var emp in source.OrderBy(e => e.Morale))
        {
            Employees.Add(new EmployeeRowModel
            {
                Id = emp.Id,
                Name = emp.Name,
                Department = emp.Department.ToString(),
                Role = emp.Role.ToString(),
                Morale = emp.Morale,
                Productivity = emp.Productivity,
                Salary = emp.Salary,
                IsAssigned = emp.IsAssigned,
                IsLowMorale = emp.Morale <= 15,
                MoraleStatus = emp.Morale switch
                {
                    <= 10 => "🔴 Critical",
                    <= 25 => "🟠 Low",
                    <= 50 => "🟡 Cautious",
                    <= 75 => "🟢 Good",
                    _ => "✅ Excellent",
                },
            });
        }
    }
}