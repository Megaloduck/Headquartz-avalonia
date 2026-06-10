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

namespace Headquartz.App.ViewModels;

public partial class PayableLineModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Vendor { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }
    public string DueDate { get; set; } = "";
    public bool IsOverdue { get; set; }
    public string Status { get; set; } = "Pending";
}

public partial class FinanceAccountPayableViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<PayableLineModel> _allPayables = [];

    [ObservableProperty] private decimal _totalPayable;
    [ObservableProperty] private decimal _overdueAmount;
    [ObservableProperty] private int _overdueCount;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<PayableLineModel> Payables { get; } = [];

    public FinanceAccountPayableViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_allPayables.Count == 0)
            SeedPayables();

        Refresh();
    }

    [RelayCommand]
    private void PayNow(PayableLineModel row)
    {
        var company = _simulation.Engine.Company;

        if (company.Cash < row.Amount)
        {
            StatusMessage = $"❌ Insufficient cash to pay {row.Vendor}.";
            return;
        }

        company.Cash -= row.Amount;
        company.Expenses += row.Amount;
        row.Status = "Paid";
        _allPayables.Remove(row);

        StatusMessage = $"✅ Paid ${row.Amount:N0} to {row.Vendor}.";
        Refresh();
    }

    [RelayCommand]
    private void PayAll()
    {
        var company = _simulation.Engine.Company;
        int paid = 0;

        foreach (var p in _allPayables.ToList())
        {
            if (company.Cash < p.Amount) break;
            company.Cash -= p.Amount;
            company.Expenses += p.Amount;
            _allPayables.Remove(p);
            paid++;
        }

        StatusMessage = paid > 0
            ? $"✅ Paid {paid} invoice(s)."
            : "❌ Insufficient cash to pay any invoices.";

        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        CompanyCash = company.Cash;
        TotalPayable = _allPayables.Sum(p => p.Amount);
        OverdueCount = _allPayables.Count(p => p.IsOverdue);
        OverdueAmount = _allPayables.Where(p => p.IsOverdue).Sum(p => p.Amount);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Payable", Value = $"${TotalPayable:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Overdue", Value = OverdueCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Overdue Amount", Value = $"${OverdueAmount:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Company Cash", Value = $"${CompanyCash:N0}" });

        Payables.Clear();
        foreach (var p in _allPayables.OrderBy(p => p.IsOverdue ? 0 : 1))
            Payables.Add(p);
    }

    private void SeedPayables()
    {
        string[] vendors = ["Office Supplies Co.", "Cloud Services Ltd.", "Equipment Rental Inc.", "Utility Provider", "Insurance Group"];
        string[] descs = ["Monthly supplies", "Server hosting", "Machinery lease", "Electricity bill", "Liability insurance"];

        for (int i = 0; i < 5; i++)
        {
            _allPayables.Add(new PayableLineModel
            {
                Vendor = vendors[i],
                Description = descs[i],
                Amount = Random.Shared.Next(500, 8000),
                DueDate = DateTime.Now.AddDays(Random.Shared.Next(-3, 14)).ToString("MM/dd"),
                IsOverdue = i < 2,
                Status = "Pending",
            });
        }
    }
}