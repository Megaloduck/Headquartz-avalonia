using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class LogisticsShipmentsViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Filters ───────────────────────────────────────────────

    [ObservableProperty] private string _activeFilter = "In Transit";

    public ObservableCollection<string> Filters { get; } =
    [
        "In Transit", "All", "Overdue", "Delivered"
    ];

    // ── Counts ────────────────────────────────────────────────

    [ObservableProperty] private int _inTransit;
    [ObservableProperty] private int _overdue;
    [ObservableProperty] private int _delivered;
    [ObservableProperty] private int _deptEfficiency;
    [ObservableProperty] private int _deptStress;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<OrderRowModel> Shipments { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public LogisticsShipmentsViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    partial void OnActiveFilterChanged(string value) => Refresh();

    [RelayCommand]
    private void SetFilter(string filter) => ActiveFilter = filter;

    [RelayCommand]
    private void ExpediteShipment(OrderRowModel row)
    {
        var company = _simulation.Engine.Company;
        var order = company.Orders.FirstOrDefault(o => o.Id == row.Id);

        if (order == null || order.Status != OrderStatus.Shipping) return;

        const decimal cost = 800m;
        if (company.Cash < cost) return;

        order.Status = OrderStatus.Delivered;
        company.Cash -= cost;
        company.Expenses += cost;
        company.Reputation = Math.Min(100, company.Reputation + 1);

        Refresh();
    }

    [RelayCommand]
    private void ExpediteAll()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 800m;

        foreach (var order in company.Orders
                     .Where(o => o.Status == OrderStatus.Shipping)
                     .ToList())
        {
            if (company.Cash < cost) break;

            order.Status = OrderStatus.Delivered;
            company.Cash -= cost;
            company.Expenses += cost;
            company.Reputation = Math.Min(100, company.Reputation + 1);
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

        InTransit = company.Orders.Count(o =>
            o.Status is OrderStatus.Shipping or OrderStatus.ReadyForShipment);

        Overdue = company.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status is OrderStatus.Shipping or OrderStatus.ReadyForShipment);

        Delivered = company.Orders.Count(o => o.Status == OrderStatus.Delivered);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "In Transit", Value = InTransit.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Overdue", Value = Overdue.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Delivered", Value = Delivered.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Efficiency", Value = $"{DeptEfficiency}%" });
        Kpis.Add(new KpiCardModel { Title = "Stress", Value = $"{DeptStress}%" });

        var source = ActiveFilter switch
        {
            "Overdue" => company.Orders.Where(o =>
                o.DeliveryDeadline.HasValue &&
                clock.WorldTime > o.DeliveryDeadline.Value &&
                o.Status is OrderStatus.Shipping or OrderStatus.ReadyForShipment),
            "Delivered" => company.Orders.Where(o => o.Status == OrderStatus.Delivered),
            "In Transit" => company.Orders.Where(o =>
                o.Status is OrderStatus.Shipping or OrderStatus.ReadyForShipment),
            _ => company.Orders.AsEnumerable(),
        };

        Shipments.Clear();
        foreach (var order in source
                     .OrderBy(o => o.DeliveryDeadline)
                     .Take(40))
        {
            bool overdue = order.DeliveryDeadline.HasValue &&
                           clock.WorldTime > order.DeliveryDeadline.Value &&
                           order.Status is not OrderStatus.Delivered
                                       and not OrderStatus.Cancelled;

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
    }
}