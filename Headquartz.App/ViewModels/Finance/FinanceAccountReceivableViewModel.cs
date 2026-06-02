using System;
using System.Collections.ObjectModel;
using System.Linq;

using Avalonia.Threading;

using CommunityToolkit.Mvvm.ComponentModel;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

namespace Headquartz.App.ViewModels;

public partial class ARLineModel : ObservableObject
{
    public string ClientName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Status { get; set; } = "";
    public decimal Value { get; set; }
    public string Deadline { get; set; } = "";
    public bool IsOverdue { get; set; }
    public int TicksInPipe { get; set; }
}

public partial class FinanceAccountReceivableViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private decimal _totalAR;
    [ObservableProperty] private decimal _collectedRevenue;
    [ObservableProperty] private decimal _pendingRevenue;
    [ObservableProperty] private int _overdueCount;
    [ObservableProperty] private int _atRiskCount;

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<ARLineModel> Pipeline { get; } = [];

    public FinanceAccountReceivableViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        // Collected = delivered orders
        CollectedRevenue = company.Orders
            .Where(o => o.Status == OrderStatus.Delivered)
            .Sum(o => o.Quantity * o.UnitPrice);

        // Pending = active orders (not yet delivered or cancelled)
        PendingRevenue = company.Orders
            .Where(o => o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled)
            .Sum(o => o.Quantity * o.UnitPrice);

        TotalAR = CollectedRevenue + PendingRevenue;
        OverdueCount = company.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled);

        AtRiskCount = company.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            o.DeliveryDeadline.Value <= clock.WorldTime.AddDays(2) &&
            o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total AR", Value = $"${TotalAR:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Collected", Value = $"${CollectedRevenue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Pending", Value = $"${PendingRevenue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Overdue", Value = OverdueCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Due Soon (2d)", Value = AtRiskCount.ToString() });

        Pipeline.Clear();
        foreach (var order in company.Orders
                     .Where(o => o.Status is not OrderStatus.Cancelled)
                     .OrderBy(o => o.DeliveryDeadline)
                     .Take(40))
        {
            bool overdue = order.DeliveryDeadline.HasValue &&
                           clock.WorldTime > order.DeliveryDeadline.Value &&
                           order.Status is not OrderStatus.Delivered;

            // Approximate ticks in pipeline from order creation
            int ticksIn = (int)((clock.WorldTime - order.CreatedAt).TotalMinutes / 15);

            Pipeline.Add(new ARLineModel
            {
                ClientName = order.ClientName,
                ProductName = order.ProductName,
                Status = order.Status.ToString(),
                Value = order.Quantity * order.UnitPrice,
                Deadline = order.DeliveryDeadline.HasValue
                    ? order.DeliveryDeadline.Value.ToString("MM/dd HH:mm")
                    : "—",
                IsOverdue = overdue,
                TicksInPipe = Math.Max(0, ticksIn),
            });
        }
    }
}