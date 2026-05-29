using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headquartz.App.ViewModels;

public partial class FinanceDashboardViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private decimal _cash;
    [ObservableProperty] private decimal _revenue;
    [ObservableProperty] private decimal _expenses;
    [ObservableProperty] private decimal _profit;
    [ObservableProperty] private decimal _nextPayroll;
    [ObservableProperty] private bool _isCashCritical;
    [ObservableProperty] private bool _isPayrollAtRisk;
    [ObservableProperty] private string _cashStatus = "";

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<BudgetRowModel> DepartmentBudgets { get; } = [];
    public ObservableCollection<EventViewModel> Events { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public FinanceDashboardViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void TakeLoan()
    {
        const decimal loanAmount = 25_000m;

        _simulation.Engine.Company.Cash += loanAmount;
        _simulation.Engine.Company.Expenses += loanAmount * 0.1m; // 10% cost

        Refresh();
    }

    [RelayCommand]
    private void CutBudgets()
    {
        foreach (var dept in _simulation.Engine.Company.Departments)
            dept.Budget = Math.Max(1_000m, dept.Budget * 0.90m);

        Refresh();
    }

    [RelayCommand]
    private void FreezeBonuses()
    {
        // Reduces expected payroll by capping salary draw this cycle
        foreach (var emp in _simulation.Engine.Company.Employees)
            emp.Morale = Math.Max(0, emp.Morale - 5);

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        Cash = company.Cash;
        Revenue = company.Revenue;
        Expenses = company.Expenses;
        Profit = company.Profit;

        NextPayroll = company.Employees.Sum(e => e.Salary);
        IsCashCritical = company.Cash < 0;
        IsPayrollAtRisk = company.Cash < NextPayroll;

        CashStatus = company.Cash switch
        {
            < 0 => "CRITICAL",
            < 20_000 => "LOW",
            < 50_000 => "CAUTION",
            _ => "HEALTHY",
        };

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${Cash:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Revenue", Value = $"${Revenue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Expenses", Value = $"${Expenses:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Profit", Value = $"${Profit:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Next Payroll", Value = $"${NextPayroll:N0}" });

        // Department budgets
        DepartmentBudgets.Clear();
        foreach (var dept in company.Departments)
        {
            DepartmentBudgets.Add(new BudgetRowModel
            {
                DepartmentName = dept.Type.ToString(),
                Budget = dept.Budget,
                Efficiency = dept.Efficiency,
                StressLevel = dept.StressLevel,
                StatusText = dept.StressLevel switch
                {
                    > 80 => "⚠ Critical",
                    > 50 => "~ Stressed",
                    _ => "✓ Stable",
                },
            });
        }

        // Finance events
        Events.Clear();
        foreach (var ev in company.Events
                     .Where(e => e.Department == DepartmentType.Finance && !e.IsResolved)
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