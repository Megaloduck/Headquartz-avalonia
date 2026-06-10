using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class SLAEntryModel : ObservableObject
{
    public Guid OrderId { get; set; }
    public string ClientName { get; set; } = "";
    public string ProductName { get; set; } = "";
    public string Deadline { get; set; } = "";
    public string Status { get; set; } = "";
    public int TicksRemaining { get; set; }
    public bool IsAtRisk { get; set; }
    public bool IsBreach { get; set; }
    public decimal Penalty { get; set; }
}

public partial class LogisticsSLAViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _totalSLAs;
    [ObservableProperty] private int _onTrack;
    [ObservableProperty] private int _atRisk;
    [ObservableProperty] private int _breached;
    [ObservableProperty] private decimal _totalPenalties;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<SLAEntryModel> SLAs { get; } = [];

    public LogisticsSLAViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void ExpediteOrder(SLAEntryModel entry)
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 800m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to expedite. Need $800.";
            return;
        }

        var order = company.Orders.FirstOrDefault(o => o.Id == entry.OrderId);
        if (order == null) return;

        company.Cash -= cost;
        company.Expenses += cost;
        order.Status = OrderStatus.Delivered;
        company.Reputation = Math.Min(100, company.Reputation + 1);

        StatusMessage = $"✅ Order expedited for {entry.ClientName}. Reputation +1. Cost: $800.";
        Refresh();
    }

    [RelayCommand]
    private void PayPenalties()
    {
        var company = _simulation.Engine.Company;

        if (company.Cash < TotalPenalties)
        {
            StatusMessage = "❌ Insufficient cash to pay all penalties.";
            return;
        }

        company.Cash -= TotalPenalties;
        company.Expenses += TotalPenalties;
        company.Reputation = Math.Max(0, company.Reputation - 2);

        StatusMessage = $"✅ Paid ${TotalPenalties:N0} in SLA penalties. Reputation −2.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        var clock = _simulation.Engine.Clock;

        CompanyCash = company.Cash;

        var activeOrders = company.Orders
            .Where(o => o.Status is not OrderStatus.Delivered and not OrderStatus.Cancelled
                        && o.DeliveryDeadline.HasValue)
            .ToList();

        TotalSLAs = activeOrders.Count;

        decimal penalties = 0;

        Kpis.Clear();
        SLAs.Clear();

        int onTrack = 0, atRisk = 0, breached = 0;

        foreach (var order in activeOrders.OrderBy(o => o.DeliveryDeadline))
        {
            if (!order.DeliveryDeadline.HasValue) continue;

            var deadline = order.DeliveryDeadline.Value;
            bool isBreach = clock.WorldTime > deadline;
            TimeSpan remaining = deadline - clock.WorldTime;
            int ticksLeft = (int)(remaining.TotalMinutes / 15);
            bool isAtRisk = !isBreach && ticksLeft <= 5;

            decimal penalty = isBreach
                ? order.Quantity * order.UnitPrice * 0.10m
                : 0;

            penalties += penalty;

            if (isBreach) breached++;
            else if (isAtRisk) atRisk++;
            else onTrack++;

            SLAs.Add(new SLAEntryModel
            {
                OrderId = order.Id,
                ClientName = order.ClientName,
                ProductName = order.ProductName,
                Deadline = deadline.ToString("MM/dd HH:mm"),
                Status = order.Status.ToString(),
                TicksRemaining = Math.Max(0, ticksLeft),
                IsAtRisk = isAtRisk,
                IsBreach = isBreach,
                Penalty = penalty,
            });
        }

        OnTrack = onTrack;
        AtRisk = atRisk;
        Breached = breached;
        TotalPenalties = penalties;

        Kpis.Add(new KpiCardModel { Title = "Total SLAs", Value = TotalSLAs.ToString() });
        Kpis.Add(new KpiCardModel { Title = "On Track", Value = OnTrack.ToString() });
        Kpis.Add(new KpiCardModel { Title = "At Risk", Value = AtRisk.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Breached", Value = Breached.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Penalties", Value = $"${TotalPenalties:N0}" });
    }
}