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

public partial class TrackingEntryModel : ObservableObject
{
    public Guid OrderId { get; set; }
    public string ClientName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Status { get; set; } = "";
    public string CurrentLocation { get; set; } = "";
    public string Deadline { get; set; } = "";
    public bool IsOverdue { get; set; }
    public int ProgressPercent { get; set; }
    public string LastUpdated { get; set; } = "";
}

public partial class LogisticsTrackingViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _inTransit;
    [ObservableProperty] private int _delivered;
    [ObservableProperty] private int _overdue;
    [ObservableProperty] private int _pendingDispatch;
    [ObservableProperty] private string _filterStatus = "All";
    [ObservableProperty] private string _searchText = "";

    public ObservableCollection<string> StatusFilters { get; } =
    [
        "All", "In Transit", "Overdue", "Ready for Dispatch"
    ];

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<TrackingEntryModel> Shipments { get; } = [];

    public LogisticsTrackingViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    partial void OnFilterStatusChanged(string value) => Refresh();
    partial void OnSearchTextChanged(string value) => Refresh();

    [RelayCommand]
    private void SetFilter(string filter) => FilterStatus = filter;

    [RelayCommand]
    private void MarkDelivered(TrackingEntryModel entry)
    {
        var company = _simulation.Engine.Company;
        var order = company.Orders.FirstOrDefault(o => o.Id == entry.OrderId);

        if (order == null) return;

        order.Status = OrderStatus.Delivered;
        company.Reputation = Math.Min(100, company.Reputation + 1);

        StatusMessage = $"✅ {entry.ClientName} order marked as delivered. Reputation +1.";
        Refresh();
    }

    [ObservableProperty] private string _statusMessage = "";

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        InTransit = company.Orders.Count(o => o.Status == OrderStatus.Shipping);
        Delivered = company.Orders.Count(o => o.Status == OrderStatus.Delivered);
        PendingDispatch = company.Orders.Count(o => o.Status == OrderStatus.ReadyForShipment);
        Overdue = company.Orders.Count(o =>
            o.DeliveryDeadline.HasValue &&
            clock.WorldTime > o.DeliveryDeadline.Value &&
            o.Status == OrderStatus.Shipping);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "In Transit", Value = InTransit.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Delivered", Value = Delivered.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Overdue", Value = Overdue.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Pending Dispatch", Value = PendingDispatch.ToString() });

        string[] locations =
        [
            "Distribution Hub", "Regional Depot", "Last Mile", "In Transit", "Customs Clearance"
        ];

        var source = FilterStatus switch
        {
            "In Transit" => company.Orders.Where(o => o.Status == OrderStatus.Shipping),
            "Overdue" => company.Orders.Where(o =>
                o.DeliveryDeadline.HasValue &&
                clock.WorldTime > o.DeliveryDeadline.Value &&
                o.Status == OrderStatus.Shipping),
            "Ready for Dispatch" => company.Orders.Where(o =>
                o.Status == OrderStatus.ReadyForShipment),
            _ => company.Orders.Where(o =>
                o.Status is OrderStatus.Shipping or OrderStatus.ReadyForShipment),
        };

        if (!string.IsNullOrWhiteSpace(SearchText))
            source = source.Where(o =>
                o.ClientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                o.ProductName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Shipments.Clear();
        foreach (var order in source.OrderBy(o => o.DeliveryDeadline).Take(40))
        {
            bool overdue = order.DeliveryDeadline.HasValue &&
                           clock.WorldTime > order.DeliveryDeadline.Value &&
                           order.Status != OrderStatus.Delivered;

            int progress = order.Status switch
            {
                OrderStatus.Pending => 10,
                OrderStatus.Approved => 25,
                OrderStatus.InProduction => 40,
                OrderStatus.ReadyForShipment => 60,
                OrderStatus.Shipping => 80,
                OrderStatus.Delivered => 100,
                _ => 0,
            };

            Shipments.Add(new TrackingEntryModel
            {
                OrderId = order.Id,
                ClientName = order.ClientName,
                ProductName = order.ProductName,
                Status = order.Status.ToString(),
                CurrentLocation = locations[Random.Shared.Next(locations.Length)],
                Deadline = order.DeliveryDeadline.HasValue
                    ? order.DeliveryDeadline.Value.ToString("MM/dd HH:mm")
                    : "—",
                IsOverdue = overdue,
                ProgressPercent = progress,
                LastUpdated = clock.WorldTime.ToString("MM/dd HH:mm"),
            });
        }
    }
}