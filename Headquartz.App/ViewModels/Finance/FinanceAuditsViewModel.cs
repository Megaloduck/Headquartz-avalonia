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

public partial class AuditFindingModel : ObservableObject
{
    public string Department { get; set; } = "";
    public string Issue { get; set; } = "";
    public string Severity { get; set; } = "";
    public string Recommendation { get; set; } = "";
    public bool IsResolved { get; set; }
}

public partial class FinanceAuditsViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<AuditFindingModel> _findings = [];

    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private decimal _totalExpenses;
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _unresolvedFindings;
    [ObservableProperty] private string _auditStatus = "Clean";
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private long _lastAuditTick = 0;

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<AuditFindingModel> Findings { get; } = [];

    public FinanceAuditsViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_findings.Count == 0)
            SeedFindings();

        Refresh();
    }

    [RelayCommand]
    private void RunAudit()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 2_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient funds to run audit. Need $2,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        _findings.Clear();

        // Generate findings based on company state
        if (company.Cash < 10_000m)
            _findings.Add(new AuditFindingModel
            {
                Department = "Finance",
                Issue = "Cash reserves critically low",
                Severity = "Critical",
                Recommendation = "Seek emergency funding or cut operational costs.",
            });

        if (company.Employees.Any(e => e.Morale < 20))
            _findings.Add(new AuditFindingModel
            {
                Department = "Human Resources",
                Issue = "Multiple employees at resignation risk",
                Severity = "High",
                Recommendation = "Run morale workshop or boost compensation.",
            });

        if (company.Inventory.Any(i => i.Quantity < i.MinimumStock))
            _findings.Add(new AuditFindingModel
            {
                Department = "Warehouse",
                Issue = "Inventory below minimum stock levels",
                Severity = "Medium",
                Recommendation = "Trigger emergency restock immediately.",
            });

        if (company.Orders.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled) > 5)
            _findings.Add(new AuditFindingModel
            {
                Department = "Sales",
                Issue = "High order cancellation rate detected",
                Severity = "High",
                Recommendation = "Review delivery pipeline and logistics capacity.",
            });

        if (company.Departments.Any(d => d.StressLevel > 75))
            _findings.Add(new AuditFindingModel
            {
                Department = "Operations",
                Issue = "One or more departments at critical stress",
                Severity = "High",
                Recommendation = "Reduce task load and run maintenance.",
            });

        if (_findings.Count == 0)
            _findings.Add(new AuditFindingModel
            {
                Department = "All",
                Issue = "No significant issues found",
                Severity = "Low",
                Recommendation = "Company operations appear healthy.",
                IsResolved = true,
            });

        LastAuditTick = _simulation.Engine.Clock.Tick;
        StatusMessage = $"✅ Audit complete. {_findings.Count} finding(s). Cost: $2,000.";
        Refresh();
    }

    [RelayCommand]
    private void ResolveFinding(AuditFindingModel finding)
    {
        finding.IsResolved = true;
        StatusMessage = $"✅ Finding marked as resolved: {finding.Issue}";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        CompanyCash = company.Cash;
        TotalExpenses = company.Expenses;
        TotalRevenue = company.Revenue;
        UnresolvedFindings = _findings.Count(f => !f.IsResolved);

        AuditStatus = UnresolvedFindings switch
        {
            0 => "✓ Clean",
            <= 2 => "~ Minor Issues",
            _ => "⚠ Needs Attention",
        };

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Revenue", Value = $"${TotalRevenue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Expenses", Value = $"${TotalExpenses:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Findings", Value = UnresolvedFindings.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Audit Status", Value = AuditStatus });

        Findings.Clear();
        foreach (var f in _findings.OrderBy(f => f.IsResolved ? 1 : 0))
            Findings.Add(f);
    }

    private void SeedFindings()
    {
        _findings.Add(new AuditFindingModel
        {
            Department = "All",
            Issue = "No audit has been run yet",
            Severity = "Info",
            Recommendation = "Run an audit to assess company health.",
            IsResolved = false,
        });
    }
}