using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class ClientModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Industry { get; set; } = "";
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public int SatisfactionScore { get; set; }
    public string Status { get; set; } = "Active";
    public string LastOrderDate { get; set; } = "";
}

public partial class SalesClientsViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<ClientModel> _clients = [];

    [ObservableProperty] private int _totalClients;
    [ObservableProperty] private int _activeClients;
    [ObservableProperty] private int _atRiskClients;
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private string _statusMessage = "";
    [ObservableProperty] private string _searchText = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<ClientModel> Clients { get; } = [];

    public SalesClientsViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_clients.Count == 0)
            SeedClients();

        Refresh();
    }

    partial void OnSearchTextChanged(string value) => Refresh();

    [RelayCommand]
    private void AddClient()
    {
        string[] names = ["Vertex Corp", "NovaTech", "BlueWave", "IronClad Ltd", "Crestview Co",
                          "Meridian Group", "Atlas Holdings", "Sterling Dynamics", "Apex Solutions", "FutureBuild"];
        string[] industries = ["Manufacturing", "Technology", "Retail", "Logistics", "Finance"];

        _clients.Add(new ClientModel
        {
            Name = names[Random.Shared.Next(names.Length)],
            Industry = industries[Random.Shared.Next(industries.Length)],
            TotalOrders = 0,
            TotalRevenue = 0,
            SatisfactionScore = Random.Shared.Next(60, 95),
            Status = "Active",
            LastOrderDate = _simulation.Engine.Clock.WorldTime.ToString("MM/dd"),
        });

        StatusMessage = "✅ New client added to portfolio.";
        Refresh();
    }

    [RelayCommand]
    private void ContactClient(ClientModel client)
    {
        client.SatisfactionScore = Math.Min(100, client.SatisfactionScore + 5);
        StatusMessage = $"✅ Contacted {client.Name}. Satisfaction +5.";
        Refresh();
    }

    [RelayCommand]
    private void MarkAtRisk(ClientModel client)
    {
        client.Status = "At Risk";
        StatusMessage = $"⚠ {client.Name} marked as at-risk.";
        Refresh();
    }

    private void Refresh()
    {
        // Sync orders from simulation into client data
        var company = _simulation.Engine.Company;

        foreach (var client in _clients)
        {
            var orders = company.Orders
                .Where(o => o.ClientName == client.Name)
                .ToList();

            client.TotalOrders = orders.Count;
            client.TotalRevenue = orders.Sum(o => o.Quantity * o.UnitPrice);

            if (orders.Count > 0)
                client.LastOrderDate = orders
                    .Max(o => o.CreatedAt)
                    .ToString("MM/dd");
        }

        TotalClients = _clients.Count;
        ActiveClients = _clients.Count(c => c.Status == "Active");
        AtRiskClients = _clients.Count(c => c.Status == "At Risk" || c.SatisfactionScore < 40);
        TotalRevenue = _clients.Sum(c => c.TotalRevenue);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Total Clients", Value = TotalClients.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Active", Value = ActiveClients.ToString() });
        Kpis.Add(new KpiCardModel { Title = "At Risk", Value = AtRiskClients.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Client Revenue", Value = $"${TotalRevenue:N0}" });

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _clients
            : _clients.Where(c => c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        Clients.Clear();
        foreach (var c in filtered.OrderBy(c => c.SatisfactionScore))
            Clients.Add(c);
    }

    private void SeedClients()
    {
        string[] names = ["Global Retail Corp", "TechNova Industries", "BlueStar Logistics",
                          "Apex Manufacturing", "FutureGrid Systems"];
        string[] industries = ["Retail", "Technology", "Logistics", "Manufacturing", "Energy"];

        for (int i = 0; i < 5; i++)
        {
            _clients.Add(new ClientModel
            {
                Name = names[i],
                Industry = industries[i],
                TotalOrders = Random.Shared.Next(1, 10),
                TotalRevenue = Random.Shared.Next(5, 50) * 1_000m,
                SatisfactionScore = Random.Shared.Next(40, 95),
                Status = i == 4 ? "At Risk" : "Active",
                LastOrderDate = DateTime.Now.AddDays(-Random.Shared.Next(1, 30)).ToString("MM/dd"),
            });
        }
    }
}