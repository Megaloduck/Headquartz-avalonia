using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class LogisticsDashboardViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private int _activeShipments;
    [ObservableProperty] private int _deliveredToday;
    [ObservableProperty] private int _overdueShipments;
    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;
    [ObservableProperty] private bool _hasOverdue;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<OrderRowModel> Shipments { get; } = [];
    public ObservableCollection<EventViewModel> Events { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public LogisticsDashboardViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void ExpediteShipments()
    {
        // Advance all Shipping orders to Delivered — costs cash
        var company = _simulation.Engine.Company;
        const decimal costPerShipment = 800m;

        var shipping = company.Orders
            .Where(o => o.Status == OrderStatus.Shipping)
            .ToList();

        foreach (var order in shipping)
        {
            if (company.Cash < costPerShipment) break;

            order.Status = OrderStatus.Delivered;
            company.Cash -= costPerShipment;
            company.Expenses += costPerShipment;
            company.Reputation = Math.Min(100, company.Reputation + 1);
        }

        Refresh();
    }

    [RelayCommand]
    private void OptimizeRoutes()
    {
        var dept = _simulation.Engine.Company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Logistics);

        if (dept != null)
        {
            dept.StressLevel = Math.Max(0, dept.StressLevel - 20);
            dept.Efficiency = Math.Min(100, dept.Efficiency + 8);
        }

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        var logDept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Logistics);

        DeptEfficiency = logDept?.Efficiency ?? 0;
        DeptStress = logDept?.StressLevel ?? 0;

        ActiveShipments = company.Orders.Count(o => o.Status == OrderStatus.Shipping);
        DeliveredToday = company.Orders.Count(o => o.Status == OrderStatus.Delivered);
        OverdueShipments = company.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status == OrderStatus.Shipping);

        HasOverdue = OverdueShipments > 0;

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Active Shipments", Value = ActiveShipments.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Delivered", Value = DeliveredToday.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Overdue", Value = OverdueShipments.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });

        // Shipments list
        Shipments.Clear();
        foreach (var order in company.Orders
                     .Where(o => o.Status == OrderStatus.Shipping ||
                                 o.Status == OrderStatus.ReadyForShipment)
                     .OrderBy(o => o.DeliveryDeadline)
                     .Take(20))
        {
            bool overdue = order.DeliveryDeadline.HasValue &&
                           clock.WorldTime > order.DeliveryDeadline.Value;

            Shipments.Add(new OrderRowModel
            {
                Id = order.Id,
                ClientName = order.ClientName,
                ProductName = order.ProductName,
                Quantity = order.Quantity,
                Revenue = order.Quantity * order.UnitPrice,
                Status = order.Status.ToString(),
                Deadline = order.DeliveryDeadline.HasValue
                    ? order.DeliveryDeadline.Value.ToString("MM/dd HH:mm")
                    : "—",
                IsOverdue = overdue,
            });
        }

        // Events
        Events.Clear();
        foreach (var ev in company.Events
                     .Where(e => e.Department == DepartmentType.Logistics && !e.IsResolved)
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