using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;

namespace Headquartz.App.ViewModels;

public partial class FinanceLoansViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Summary ───────────────────────────────────────────────

    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private decimal _totalDebt;
    [ObservableProperty] private decimal _totalInterestPaid;
    [ObservableProperty] private int _activeLoansCount;  // Changed from _activeLoans
    [ObservableProperty] private string _statusMessage = "";

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<LoanRowModel> ActiveLoansList { get; } = [];  // Renamed
    public ObservableCollection<LoanRowModel> PaidLoansList { get; } = [];     // Renamed

    // ── Constructor ───────────────────────────────────────────

    public FinanceLoansViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void TakeLoan(decimal amount)
    {
        var company = _simulation.Engine.Company;

        var loan = new LoanRecord
        {
            Principal = amount,
            TotalOwed = amount,
            InterestRate = 0.05m,
            TakenAtTick = _simulation.Engine.Clock.Tick,
        };

        company.Loans.Add(loan);
        company.Cash += amount;
        company.Revenue += 0; // loan is not revenue

        StatusMessage = $"✅ Loan of ${amount:N0} approved. 5% interest per payroll cycle.";

        Refresh();
    }

    [RelayCommand]
    private void RepayLoan(LoanRowModel row)
    {
        var company = _simulation.Engine.Company;
        var loan = company.Loans.FirstOrDefault(l => l.Id == row.Id);

        if (loan == null || loan.IsRepaid) return;

        if (company.Cash < loan.TotalOwed)
        {
            StatusMessage = $"❌ Insufficient cash. Need ${loan.TotalOwed:N0}.";
            return;
        }

        company.Cash -= loan.TotalOwed;
        company.Expenses += loan.TotalOwed - loan.Principal; // only interest is an expense
        loan.IsRepaid = true;

        StatusMessage = $"✅ Loan repaid. ${loan.TotalOwed:N0} cleared.";

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        CompanyCash = company.Cash;
        TotalDebt = company.Loans.Where(l => !l.IsRepaid).Sum(l => l.TotalOwed);
        TotalInterestPaid = company.Loans.Sum(l => l.InterestPaid);
        ActiveLoansCount = company.Loans.Count(l => !l.IsRepaid);  // Updated

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Total Debt", Value = $"${TotalDebt:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Active Loans", Value = ActiveLoansCount.ToString() });  // Updated
        Kpis.Add(new KpiCardModel { Title = "Interest Paid", Value = $"${TotalInterestPaid:N0}" });

        ActiveLoans_Rebuild(company, clock);
        PaidLoans_Rebuild(company, clock);
    }

    private void ActiveLoans_Rebuild(
        Headquartz.Domain.Entities.Company company,
        Headquartz.Simulation.Ticks.SimulationClock clock)
    {
        ActiveLoansList.Clear();  // Updated
        foreach (var loan in company.Loans
                     .Where(l => !l.IsRepaid)
                     .OrderBy(l => l.TakenAtTick))
        {
            ActiveLoansList.Add(ToRow(loan, clock));  // Updated
        }
    }

    private void PaidLoans_Rebuild(
        Headquartz.Domain.Entities.Company company,
        Headquartz.Simulation.Ticks.SimulationClock clock)
    {
        PaidLoansList.Clear();  // Updated
        foreach (var loan in company.Loans
                     .Where(l => l.IsRepaid)
                     .OrderByDescending(l => l.TakenAtTick)
                     .Take(10))
        {
            PaidLoansList.Add(ToRow(loan, clock));  // Updated
        }
    }

    private static LoanRowModel ToRow(
        LoanRecord loan,
        Headquartz.Simulation.Ticks.SimulationClock clock) =>
        new()
        {
            Id = loan.Id,
            Principal = loan.Principal,
            TotalOwed = loan.TotalOwed,
            InterestPaid = loan.InterestPaid,
            InterestRate = loan.InterestRate,
            TakenAtTick = loan.TakenAtTick,
            TakenAtTime = clock.WorldTime
                               .AddMinutes(-15 * (clock.Tick - loan.TakenAtTick))
                               .ToString("MM/dd HH:mm"),
            IsRepaid = loan.IsRepaid,
        };
}