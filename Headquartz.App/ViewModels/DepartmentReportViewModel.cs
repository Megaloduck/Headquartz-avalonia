using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class DepartmentReportViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;
    private readonly DepartmentType _deptType;

    // ── Header ────────────────────────────────────────────────

    [ObservableProperty] private string _deptName = "";
    [ObservableProperty] private string _deptEmoji = "";
    [ObservableProperty] private int _efficiency;
    [ObservableProperty] private int _stressLevel;
    [ObservableProperty] private int _staffCount;
    [ObservableProperty] private bool _isOperational;
    [ObservableProperty] private string _statusLabel = "";

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<ReportMetricModel> Metrics { get; } = [];
    public ObservableCollection<EventViewModel> RecentEvents { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public DepartmentReportViewModel(
        SimulationService simulation,
        DepartmentType deptType)
    {
        _simulation = simulation;
        _deptType = deptType;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        var dept = company.Departments
            .FirstOrDefault(d => d.Type == _deptType);

        DeptName = _deptType.ToString();
        DeptEmoji = Emoji(_deptType);
        Efficiency = dept?.Efficiency ?? 0;
        StressLevel = dept?.StressLevel ?? 0;
        IsOperational = dept?.IsOperational ?? false;
        StaffCount = company.Employees.Count(e => e.Department == _deptType);

        StatusLabel = StressLevel switch
        {
            > 80 => "⚠ Critical",
            > 50 => "~ Stressed",
            _ => "✓ Stable",
        };

        // Build department-specific metrics
        Metrics.Clear();
        BuildMetrics(company, clock);

        // Recent dept events (last 10)
        RecentEvents.Clear();
        foreach (var ev in company.Events
                     .Where(e => e.Department == _deptType)
                     .OrderByDescending(e => e.CreatedAt)
                     .Take(10))
        {
            RecentEvents.Add(new EventViewModel
            {
                Id = ev.Id,
                Title = ev.Title,
                Description = ev.Description,
                Severity = ev.Severity,
                RemainingTicks = ev.IsResolved ? 0 : ev.RemainingTicks,
            });
        }
    }

    // ── Metric builders ───────────────────────────────────────

    private void BuildMetrics(
        Headquartz.Domain.Entities.Company company,
        Headquartz.Simulation.Ticks.SimulationClock clock)
    {
        switch (_deptType)
        {
            case DepartmentType.HumanResources:
                BuildHRMetrics(company); break;
            case DepartmentType.Finance:
                BuildFinanceMetrics(company); break;
            case DepartmentType.Sales:
                BuildSalesMetrics(company, clock); break;
            case DepartmentType.Marketing:
                BuildMarketingMetrics(company); break;
            case DepartmentType.Production:
                BuildProductionMetrics(company); break;
            case DepartmentType.Warehouse:
                BuildWarehouseMetrics(company); break;
            case DepartmentType.Logistics:
                BuildLogisticsMetrics(company, clock); break;
        }
    }

    private void BuildHRMetrics(Headquartz.Domain.Entities.Company c)
    {
        int total = c.Employees.Count;
        int atRisk = c.Employees.Count(e => e.Morale <= 15);
        int avgMoral = total > 0 ? (int)c.Employees.Average(e => e.Morale) : 0;
        int avgProd = total > 0 ? (int)c.Employees.Average(e => e.Productivity) : 0;
        decimal payroll = c.Employees.Sum(e => e.Salary);

        Add("Total Employees", total.ToString(), "headcount");
        Add("Monthly Payroll", $"${payroll:N0}", "per cycle");
        Add("Avg Morale", $"{avgMoral}%", atRisk > 0 ? $"⚠ {atRisk} at risk" : "stable", atRisk > 0);
        Add("Avg Productivity", $"{avgProd}%", "workforce output");
        Add("HR Events", c.Events.Count(e => e.Department == DepartmentType.HumanResources && !e.IsResolved).ToString(), "unresolved");
    }

    private void BuildFinanceMetrics(Headquartz.Domain.Entities.Company c)
    {
        int activeLoans = c.Loans.Count(l => !l.IsRepaid);
        decimal totalDebt = c.Loans.Where(l => !l.IsRepaid).Sum(l => l.TotalOwed);

        Add("Cash Balance", $"${c.Cash:N0}", c.Cash < 0 ? "⚠ NEGATIVE" : "liquid", c.Cash < 0);
        Add("Total Revenue", $"${c.Revenue:N0}", "cumulative");
        Add("Total Expenses", $"${c.Expenses:N0}", "cumulative");
        Add("Net Profit", $"${c.Profit:N0}", c.Profit < 0 ? "⚠ LOSS" : "surplus", c.Profit < 0);
        Add("Active Loans", activeLoans.ToString(), $"debt: ${totalDebt:N0}", activeLoans > 0);
    }

    private void BuildSalesMetrics(
        Headquartz.Domain.Entities.Company c,
        Headquartz.Simulation.Ticks.SimulationClock clock)
    {
        int active = c.Orders.Count(o => o.Status is not Domain.Enums.OrderStatus.Delivered and not Domain.Enums.OrderStatus.Cancelled);
        int delivered = c.Orders.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered);
        int cancelled = c.Orders.Count(o => o.Status == Domain.Enums.OrderStatus.Cancelled);
        int overdue = c.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status is not Domain.Enums.OrderStatus.Delivered and not Domain.Enums.OrderStatus.Cancelled);

        Add("Active Orders", active.ToString(), "in pipeline");
        Add("Delivered", delivered.ToString(), "fulfilled");
        Add("Cancelled", cancelled.ToString(), $"overdue: {overdue}", cancelled > 0);
        Add("Total Revenue", $"${c.Revenue:N0}", "cumulative");
        Add("Reputation", $"{c.Reputation}/100", c.Reputation < 30 ? "⚠ critical" : "score", c.Reputation < 30);
    }

    private void BuildMarketingMetrics(Headquartz.Domain.Entities.Company c)
    {
        var dept = c.Departments.FirstOrDefault(d => d.Type == DepartmentType.Marketing);
        int campaigns = c.Tasks.Count(t => t.Department == DepartmentType.Marketing &&
                                           t.Status != Domain.Enums.CompanyTaskStatus.Completed);

        Add("Active Campaigns", campaigns.ToString(), "running");
        Add("Reputation", $"{c.Reputation}/100", "brand score");
        Add("Dept Budget", $"${dept?.Budget:N0}", "allocated");
        Add("Dept Efficiency", $"{dept?.Efficiency}%", "output level");
    }

    private void BuildProductionMetrics(Headquartz.Domain.Entities.Company c)
    {
        int active = c.Tasks.Count(t => t.Department == DepartmentType.Production &&
                                           t.Status != Domain.Enums.CompanyTaskStatus.Completed);
        int blocked = c.Tasks.Count(t => t.Department == DepartmentType.Production && t.IsBlocked);
        int completed = c.Tasks.Count(t => t.Department == DepartmentType.Production &&
                                           t.Status == Domain.Enums.CompanyTaskStatus.Completed);

        Add("Active Tasks", active.ToString(), "in progress");
        Add("Blocked", blocked.ToString(), "no workers", blocked > 0);
        Add("Completed", completed.ToString(), "this session");
        Add("Inventory Units", c.Inventory.Sum(i => i.Quantity).ToString(), "total stock");
    }

    private void BuildWarehouseMetrics(Headquartz.Domain.Entities.Company c)
    {
        int lowStock = c.Inventory.Count(i => i.Quantity < i.MinimumStock);
        decimal value = c.Inventory.Sum(i => i.Quantity * i.UnitCost);

        Add("Total Units", c.Inventory.Sum(i => i.Quantity).ToString(), "in stock");
        Add("Low Stock Items", lowStock.ToString(), "below minimum", lowStock > 0);
        Add("Inventory Value", $"${value:N0}", "at unit cost");
        Add("Item Types", c.Inventory.Count.ToString(), "tracked items");
    }

    private void BuildLogisticsMetrics(
        Headquartz.Domain.Entities.Company c,
        Headquartz.Simulation.Ticks.SimulationClock clock)
    {
        int inTransit = c.Orders.Count(o => o.Status is Domain.Enums.OrderStatus.Shipping or Domain.Enums.OrderStatus.ReadyForShipment);
        int delivered = c.Orders.Count(o => o.Status == Domain.Enums.OrderStatus.Delivered);
        int overdue = c.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status == Domain.Enums.OrderStatus.Shipping);

        Add("In Transit", inTransit.ToString(), "active shipments");
        Add("Delivered", delivered.ToString(), "completed");
        Add("Overdue", overdue.ToString(), "past deadline", overdue > 0);
        Add("On-Time Rate", delivered + inTransit > 0
            ? $"{(delivered * 100 / Math.Max(1, delivered + overdue))}%"
            : "N/A", "delivery performance");
    }

    private void Add(string label, string value, string sub = "", bool isAlert = false)
    {
        Metrics.Add(new ReportMetricModel
        {
            Label = label,
            Value = value,
            SubLabel = sub,
            IsAlert = isAlert,
        });
    }

    private static string Emoji(DepartmentType t) => t switch
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