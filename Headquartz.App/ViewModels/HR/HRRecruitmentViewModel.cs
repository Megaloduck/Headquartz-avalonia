using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Commands;
using System;
using System.Collections.ObjectModel;

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

        RefreshCash();

        _simulation.Engine.OnUpdated += RefreshCash;
    }

    partial void OnSalaryChanged(decimal value)
    {
        HireCost = value * 2;
    }

    private void RefreshCash()
    {
        CompanyCash = _simulation.Engine.Company.Cash;
        HireCost = Salary * 2;
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

        StatusMessage = $"✅ {EmployeeName} hired as {SelectedRole} in {SelectedDepartment}.";
        IsSuccess = true;
        EmployeeName = "";
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