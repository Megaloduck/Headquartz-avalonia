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

public partial class SalesOrdersViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Filters ───────────────────────────────────────────────

    [ObservableProperty] private string _activeFilter = "Active";

    public ObservableCollection<string> Filters { get; } =
    [
        "Active", "All", "Delivered", "Cancelled"
    ];

    // ── Counts ────────────────────────────────────────────────

    [ObservableProperty] private int _totalActive;
    [ObservableProperty] private int _totalDelivered;
    [ObservableProperty] private int _totalCancelled;
    [ObservableProperty] private int _totalOverdue;
    [ObservableProperty] private decimal _totalRevenue;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<OrderRowModel> Orders { get; } = [];

    // ── Constructor ───────────────────────────────────────────

    public SalesOrdersViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    partial void OnActiveFilterChanged(string value) => Refresh();

    [RelayCommand]
    private void SetFilter(string filter)
    {
        ActiveFilter = filter;
    }

    // ── Refresh ───────────────────────────────────────────────

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;
        var all = company.Orders;

        TotalActive = all.Count(o => o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled);
        TotalDelivered = all.Count(o => o.Status == OrderStatus.Delivered);
        TotalCancelled = all.Count(o => o.Status == OrderStatus.Cancelled);
        TotalOverdue = all.Count(o => o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled);
        TotalRevenue = company.Revenue;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Active", Value = TotalActive.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Delivered", Value = TotalDelivered.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Cancelled", Value = TotalCancelled.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Overdue", Value = TotalOverdue.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Revenue", Value = $"${TotalRevenue:N0}" });

        // Apply filter
        var source = ActiveFilter switch
        {
            "Delivered" => all.Where(o => o.Status == OrderStatus.Delivered),
            "Cancelled" => all.Where(o => o.Status == OrderStatus.Cancelled),
            "Active" => all.Where(o =>
                o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled),
            _ => all.AsEnumerable(),
        };

        Orders.Clear();
        foreach (var order in source.OrderBy(o => o.DeliveryDeadline).Take(50))
        {
            bool overdue = order.DeliveryDeadline.HasValue &&
                           clock.WorldTime > order.DeliveryDeadline.Value &&
                           order.Status is not OrderStatus.Delivered
                                       and not OrderStatus.Cancelled;

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
    }
}
