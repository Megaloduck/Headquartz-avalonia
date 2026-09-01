using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Headquartz.App.Converters;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Simulation.Schedulers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Headquartz.App.ViewModels;

public partial class HRPayrollViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private decimal _totalPayroll;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private int _ticksUntilPayroll;
    [ObservableProperty] private bool _canCoverPayroll;
    [ObservableProperty] private bool _isPayrollCritical;
    [ObservableProperty] private int _employeeCount;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<EmployeeRowModel> Employees { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public HRPayrollViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        TotalPayroll = company.Employees.Sum(e => e.Salary);
        CompanyCash = company.Cash;
        EmployeeCount = company.Employees.Count;
        CanCoverPayroll = company.Cash >= TotalPayroll;
        IsPayrollCritical = company.Cash < TotalPayroll;

        // Ticks cycle by payroll schedule — see PayrollSchedule (work-hour × Mon-Fri × 4week, scaled by difficulty)
        long currentTick = clock.Tick;
        long nextPayrollTick = PayrollSchedule.NextPayrollTick(currentTick, _simulation.Engine.Profile.TicksPerWorkHour);  
        TicksUntilPayroll = (int)(nextPayrollTick - currentTick);

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel
        {
            Title = "Total Payroll",
            Value = $"${TotalPayroll:N0}"
        });
        Kpis.Add(new KpiCardModel
        {
            Title = "Company Cash",
            Value = $"${CompanyCash:N0}"
        });
        Kpis.Add(new KpiCardModel
        {
            Title = "Next Payroll In",
            Value = $"{TicksUntilPayroll} ticks"
        });
        Kpis.Add(new KpiCardModel
        {
            Title = "Employees",
            Value = EmployeeCount.ToString()
        });
        Kpis.Add(new KpiCardModel
        {
            Title = "Status",
            Value = CanCoverPayroll ? "✓ Covered" : "⚠ At Risk"
        });

        // Employee list sorted by salary desc
        Employees.Clear();
        foreach (var emp in company.Employees.OrderByDescending(e => e.Salary))
        {
            Employees.Add(new EmployeeRowModel
            {
                Id = emp.Id,
                Name = emp.Name,
                Department = DepartmentTypeToNameConverter.GetDisplayName(emp.Department),
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