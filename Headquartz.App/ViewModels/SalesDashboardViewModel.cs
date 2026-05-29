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
using Headquartz.Simulation.Commands;

namespace Headquartz.App.ViewModels;

public partial class SalesDashboardViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── KPIs ─────────────────────────────────────────────────

    [ObservableProperty] private int _activeOrders;
    [ObservableProperty] private int _deliveredOrders;
    [ObservableProperty] private int _cancelledOrders;
    [ObservableProperty] private int _overdueOrders;
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _reputation;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<OrderRowModel> Orders { get; } = [];
    public ObservableCollection<EventViewModel> Events { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public SalesDashboardViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void GenerateLead()
    {
        var cmd = new CreateTaskCommand
        {
            PlayerId = "sales-manager",
            Name = "Client Negotiation",
            Description = "Active lead pursuit initiated",
            Department = DepartmentType.Sales,
            Priority = Domain.Enums.TaskPriority.High,
            DurationTicks = 5,
            BudgetCost = 1_500m,
        };

        _simulation.Engine.Commands.Enqueue(cmd);
    }

    [RelayCommand]
    private void RunDiscountCampaign()
    {
        // Boosts reputation slightly to attract more orders
        _simulation.Engine.Company.Reputation =
            System.Math.Min(100,
                _simulation.Engine.Company.Reputation + 5);

        Refresh();
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        TotalRevenue = company.Revenue;
        Reputation = company.Reputation;

        ActiveOrders = company.Orders.Count(o =>
            o.Status is not OrderStatus.Delivered and
                        not OrderStatus.Cancelled);

        DeliveredOrders = company.Orders.Count(o =>
            o.Status == OrderStatus.Delivered);

        CancelledOrders = company.Orders.Count(o =>
            o.Status == OrderStatus.Cancelled);

        OverdueOrders = company.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status is not OrderStatus.Delivered and
                        not OrderStatus.Cancelled);

        // KPIs
        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Active Orders", Value = ActiveOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Delivered", Value = DeliveredOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Cancelled", Value = CancelledOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Overdue", Value = OverdueOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Revenue", Value = $"${TotalRevenue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Reputation", Value = $"{Reputation}/100" });

        // Orders list (most recent 25, active first)
        Orders.Clear();
        foreach (var order in company.Orders
                     .Where(o => o.Status != OrderStatus.Delivered)
                     .OrderBy(o => o.DeliveryDeadline)
                     .Take(25))
        {
            bool overdue = order.DeliveryDeadline.HasValue &&
                           clock.WorldTime > order.DeliveryDeadline.Value;

            Orders.Add(new OrderRowModel
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
                     .Where(e => e.Department == DepartmentType.Sales && !e.IsResolved)
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