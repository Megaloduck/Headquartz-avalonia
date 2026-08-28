using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Converters;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Commands;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class HRRecruitmentViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Form fields ───────────────────────────────────────────

    [ObservableProperty] private string _employeeName = "";
    [ObservableProperty] private EmployeeRole _selectedRole = EmployeeRole.Worker;
    [ObservableProperty] private DepartmentType _selectedDepartment = DepartmentType.Production;
    [ObservableProperty] private decimal _salary = 2_500m;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private bool _isSuccess;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private decimal _hireCost;

    // ── KPIs ──────────────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];

    // ── Dropdown sources ──────────────────────────────────────

    public ObservableCollection<EmployeeRole> AvailableRoles { get; } = [];
    public ObservableCollection<DepartmentType> AvailableDepartments { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public HRRecruitmentViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        foreach (var r in Enum.GetValues<EmployeeRole>())
            AvailableRoles.Add(r);

        foreach (var d in Enum.GetValues<DepartmentType>())
            AvailableDepartments.Add(d);

        Refresh();

        _simulation.Engine.OnUpdated += Refresh;
    }

    partial void OnSalaryChanged(decimal value)
    {
        HireCost = value * 2;
        UpdateKpis();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        CompanyCash = company.Cash;
        HireCost = Salary * 2;

        UpdateKpis();
    }

    private void UpdateKpis()
    {
        var company = _simulation.Engine.Company;

        Kpis.Clear();

        Kpis.Add(new KpiCardModel
        {
            Title = "Available Cash",
            Value = $"${CompanyCash:N0}"
        });

        Kpis.Add(new KpiCardModel
        {
            Title = "Hire Cost (2× salary)",
            Value = $"${HireCost:N0}"
        });

        // Additional useful KPIs
        int totalEmployees = company.Employees.Count;
        Kpis.Add(new KpiCardModel
        {
            Title = "Total Employees",
            Value = totalEmployees.ToString("N0")
        });

        decimal avgSalary = company.Employees.Any()
            ? company.Employees.Average(e => e.Salary)
            : 0;
        Kpis.Add(new KpiCardModel
        {
            Title = "Avg Salary",
            Value = avgSalary > 0 ? $"${avgSalary:N0}" : "N/A"
        });

        // Department count
        int deptCount = company.Employees
            .Select(e => e.Department)
            .Distinct()
            .Count();
        Kpis.Add(new KpiCardModel
        {
            Title = "Departments",
            Value = deptCount.ToString("N0")
        });
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void Hire()
    {
        if (string.IsNullOrWhiteSpace(EmployeeName))
        {
            StatusMessage = "❌ Please enter an employee name.";
            IsSuccess = false;
            return;
        }

        if (Salary < 500m)
        {
            StatusMessage = "❌ Minimum salary is $500.";
            IsSuccess = false;
            return;
        }

        if (_simulation.Engine.Company.Cash < Salary * 2)
        {
            StatusMessage = $"❌ Insufficient funds. Need ${Salary * 2:N0} (2× first month).";
            IsSuccess = false;
            return;
        }

        _simulation.Engine.Commands.Enqueue(new HireEmployeeCommand
        {
            PlayerId = "hr-manager",
            EmployeeName = EmployeeName,
            Role = SelectedRole,
            Department = SelectedDepartment,
            Salary = Salary,
        });

        StatusMessage = $"✅ {EmployeeName} hired as {SelectedRole} in {DepartmentTypeToNameConverter.GetDisplayName(SelectedDepartment)}.";
        IsSuccess = true;
        EmployeeName = "";

        Refresh();
    }

    [RelayCommand]
    private void RandomName()
    {
        string[] names =
        [
            "Jordan", "Morgan", "Taylor", "Casey",  "Riley",
            "Alex",   "Sam",    "Jamie",  "Drew",   "Quinn",
            "Avery",  "Blake",  "Reese",  "Logan",  "Skylar",
            "Emery",  "Finley", "Harper", "Kendall","Parker",
        ];
        EmployeeName = names[Random.Shared.Next(names.Length)];
    }
}