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
using System.Text;

namespace Headquartz.App.ViewModels;

public partial class BudgetRowEditModel : ObservableObject
{
    public DepartmentType Type { get; set; }
    public string Name { get; set; } = "";
    public string Emoji { get; set; } = "";
    public int Efficiency { get; set; }
    public int StressLevel { get; set; }
    public int StaffCount { get; set; }

    [ObservableProperty] private decimal _budget;
}

public partial class FinanceBudgetAllocationViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private decimal _totalBudget;
    [ObservableProperty] private decimal _totalOperationalCost;
    [ObservableProperty] private decimal _companyCash;

    public ObservableCollection<BudgetRowEditModel> Departments { get; } = [];

    public FinanceBudgetAllocationViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(RefreshStats);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void Increase(BudgetRowEditModel row)
    {
        const decimal step = 1_000m;
        const decimal max = 100_000m;

        row.Budget = Math.Min(max, row.Budget + step);
        ApplyBudget(row);
        RefreshStats();
    }

    [RelayCommand]
    private void Decrease(BudgetRowEditModel row)
    {
        const decimal step = 1_000m;
        const decimal min = 1_000m;

        row.Budget = Math.Max(min, row.Budget - step);
        ApplyBudget(row);
        RefreshStats();
    }

    [RelayCommand]
    private void ResetAll()
    {
        foreach (var row in Departments)
        {
            row.Budget = 10_000m;
            ApplyBudget(row);
        }

        RefreshStats();
    }

    // ── Internal ──────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        Departments.Clear();

        foreach (var dept in company.Departments)
        {
            int staff = company.Employees.Count(e => e.Department == dept.Type);

            Departments.Add(new BudgetRowEditModel
            {
                Type = dept.Type,
                Name = dept.Type.ToString(),
                Emoji = DeptEmoji(dept.Type),
                Budget = dept.Budget,
                Efficiency = dept.Efficiency,
                StressLevel = dept.StressLevel,
                StaffCount = staff,
            });
        }

        RefreshStats();
    }

    private void RefreshStats()
    {
        var company = _simulation.Engine.Company;

        CompanyCash = company.Cash;
        TotalBudget = Departments.Sum(d => d.Budget);
        TotalOperationalCost = Departments.Sum(d => d.Budget * 0.01m);

        // Sync efficiency/stress from live engine
        foreach (var row in Departments)
        {
            var dept = company.Departments.FirstOrDefault(d => d.Type == row.Type);
            if (dept == null) continue;

            row.Efficiency = dept.Efficiency;
            row.StressLevel = dept.StressLevel;
            row.StaffCount = company.Employees.Count(e => e.Department == row.Type);
        }
    }

    private void ApplyBudget(BudgetRowEditModel row)
    {
        var dept = _simulation.Engine.Company.Departments
            .FirstOrDefault(d => d.Type == row.Type);

        if (dept != null)
            dept.Budget = row.Budget;
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