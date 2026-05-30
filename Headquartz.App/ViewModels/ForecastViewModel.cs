using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Headquartz.App.Models;
using Headquartz.App.Services;

namespace Headquartz.App.ViewModels;

public partial class ForecastViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Summary ───────────────────────────────────────────────

    [ObservableProperty] private decimal _currentCash;
    [ObservableProperty] private decimal _revenuePerTick;
    [ObservableProperty] private decimal _expensePerTick;
    [ObservableProperty] private decimal _netPerTick;
    [ObservableProperty] private decimal _payrollEvery10;
    [ObservableProperty] private int _ticksUntilBankrupt;
    [ObservableProperty] private string _runwayLabel = "";
    [ObservableProperty] private bool _isCritical;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<ForecastDataPoint> Projection { get; } = [];
    public ObservableCollection<KpiCardModel> Kpis { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public ForecastViewModel(SimulationService simulation)
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

        CurrentCash = company.Cash;

        // Estimate revenue per tick from recent order flow
        int recentOrders = company.Orders
            .Count(o => o.Status == Domain.Enums.OrderStatus.Delivered);
        RevenuePerTick = recentOrders > 0
            ? Math.Min(recentOrders * 100m, 5_000m)
            : 0m;

        // Operational cost per tick
        ExpensePerTick = company.Departments.Sum(d => d.Budget * 0.01m);

        // Payroll every 10 ticks
        PayrollEvery10 = company.Employees.Sum(e => e.Salary);

        // Net per tick (excluding payroll)
        NetPerTick = RevenuePerTick - ExpensePerTick;

        // Project 30 ticks forward
        Projection.Clear();

        decimal runningCash = CurrentCash;
        long currentTick = clock.Tick;
        int bankruptTick = -1;

        for (int i = 1; i <= 30; i++)
        {
            long projectedTick = currentTick + i;
            bool isPayroll = projectedTick % 10 == 0;

            decimal tickExpense = ExpensePerTick;
            decimal tickRevenue = RevenuePerTick;

            runningCash += tickRevenue - tickExpense;

            if (isPayroll)
                runningCash -= PayrollEvery10;

            bool isNeg = runningCash < 0;

            if (isNeg && bankruptTick < 0)
                bankruptTick = i;

            Projection.Add(new ForecastDataPoint
            {
                Tick = projectedTick,
                WorldTime = clock.WorldTime.AddMinutes(15 * i).ToString("MM/dd HH:mm"),
                ProjectedCash = runningCash,
                Revenue = tickRevenue,
                Expenses = tickExpense + (isPayroll ? PayrollEvery10 : 0),
                IsNegative = isNeg,
                IsPayrollTick = isPayroll,
                CashDisplay = $"${runningCash:N0}",
            });
        }

        TicksUntilBankrupt = bankruptTick;
        IsCritical = bankruptTick is >= 0 and <= 10;

        RunwayLabel = bankruptTick switch
        {
            -1 => "✓ Solvent for 30+ ticks",
            <= 5 => $"🚨 Cash out in {bankruptTick} tick(s)!",
            <= 10 => $"⚠ Cash out in {bankruptTick} ticks",
            _ => $"Cash out in ~{bankruptTick} ticks",
        };

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Current Cash", Value = $"${CurrentCash:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Revenue / Tick", Value = $"${RevenuePerTick:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Expense / Tick", Value = $"${ExpensePerTick:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Net / Tick", Value = $"${NetPerTick:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Next Payroll", Value = $"${PayrollEvery10:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Runway", Value = RunwayLabel });
    }
}