using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class SalesPipelineViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _pendingOrders;
    [ObservableProperty] private int _inProductionOrders;
    [ObservableProperty] private int _readyOrders;
    [ObservableProperty] private int _shippingOrders;
    [ObservableProperty] private int _deliveredOrders;
    [ObservableProperty] private int _cancelledOrders;
    [ObservableProperty] private decimal _pipelineValue;

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<OrderRowModel> PipelineOrders { get; } = [];

    public SalesPipelineViewModel(SimulationService simulation)
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

        PendingOrders = company.Orders.Count(o => o.Status == OrderStatus.Pending);
        InProductionOrders = company.Orders.Count(o => o.Status == OrderStatus.InProduction);
        ReadyOrders = company.Orders.Count(o => o.Status == OrderStatus.ReadyForShipment);
        ShippingOrders = company.Orders.Count(o => o.Status == OrderStatus.Shipping);
        DeliveredOrders = company.Orders.Count(o => o.Status == OrderStatus.Delivered);
        CancelledOrders = company.Orders.Count(o => o.Status == OrderStatus.Cancelled);

        PipelineValue = company.Orders
            .Where(o => o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled)
            .Sum(o => o.Quantity * o.UnitPrice);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Pending", Value = PendingOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "In Production", Value = InProductionOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Ready", Value = ReadyOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Shipping", Value = ShippingOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Delivered", Value = DeliveredOrders.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Pipeline Value", Value = $"${PipelineValue:N0}" });

        PipelineOrders.Clear();
        foreach (var order in company.Orders
                     .Where(o => o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled)
                     .OrderBy(o => o.DeliveryDeadline)
                     .Take(40))
        {
            bool overdue = order.DeliveryDeadline.HasValue &&
                           clock.WorldTime > order.DeliveryDeadline.Value;

            PipelineOrders.Add(new OrderRowModel
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