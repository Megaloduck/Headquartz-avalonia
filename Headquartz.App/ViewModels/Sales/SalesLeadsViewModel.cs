using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class SalesLeadsViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    // ── Counts ────────────────────────────────────────────────

    [ObservableProperty] private int _newLeads;
    [ObservableProperty] private int _contactedLeads;
    [ObservableProperty] private int _negotiatingLeads;
    [ObservableProperty] private int _wonLeads;
    [ObservableProperty] private int _lostLeads;

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<LeadModel> Leads { get; } = [];

    // In-memory lead store (shared within session)
    private static readonly List<LeadModel> _allLeads = [];

    // ── Constructor ───────────────────────────────────────────

    public SalesLeadsViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        // Seed some initial leads if empty
        if (_allLeads.Count == 0)
            SeedLeads();

        Refresh();
    }

    // ── Commands ──────────────────────────────────────────────

    [RelayCommand]
    private void GenerateLead()
    {
        string[] companies =
        [
            "Apex Dynamics", "BlueWave Corp", "ClearPath Inc",
            "Delta Systems", "EchoTech Ltd",  "FutureBuild Co",
            "GlobalEdge",    "Horizon Group", "IronClad Ltd",
            "JetStream Inc",
        ];
        string[] products =
        [
            "Industrial Widget", "Smart Controller", "Metal Housing",
            "Control Board",     "Sensor Module",    "Power Unit",
        ];

        var lead = new LeadModel
        {
            CompanyName = companies[Random.Shared.Next(companies.Length)],
            ProductInterest = products[Random.Shared.Next(products.Length)],
            EstimatedValue = Random.Shared.Next(5, 50) * 1_000m,
            Stage = "New",
            CreatedAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
            TickAge = 0,
        };

        _allLeads.Add(lead);
        Refresh();
    }

    [RelayCommand]
    private void AdvanceStage(LeadModel lead)
    {
        lead.Stage = lead.Stage switch
        {
            "New" => "Contacted",
            "Contacted" => "Negotiating",
            "Negotiating" => "Won",
            _ => lead.Stage,
        };

        if (lead.Stage == "Won")
            ConvertToOrder(lead);

        Refresh();
    }

    [RelayCommand]
    private void MarkLost(LeadModel lead)
    {
        lead.Stage = "Lost";
        Refresh();
    }

    // ── Helpers ───────────────────────────────────────────────

    private void ConvertToOrder(LeadModel lead)
    {
        // Create a sales task and an order
        _simulation.Engine.Commands.Enqueue(new CreateTaskCommand
        {
            PlayerId = "sales-manager",
            Name = $"Close deal: {lead.CompanyName}",
            Department = DepartmentType.Sales,
            Priority = TaskPriority.High,
            DurationTicks = 3,
            BudgetCost = 500m,
        });
    }

    private void Refresh()
    {
        NewLeads = _allLeads.Count(l => l.Stage == "New");
        ContactedLeads = _allLeads.Count(l => l.Stage == "Contacted");
        NegotiatingLeads = _allLeads.Count(l => l.Stage == "Negotiating");
        WonLeads = _allLeads.Count(l => l.Stage == "Won");
        LostLeads = _allLeads.Count(l => l.Stage == "Lost");

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "New", Value = NewLeads.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Contacted", Value = ContactedLeads.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Negotiating", Value = NegotiatingLeads.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Won", Value = WonLeads.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Lost", Value = LostLeads.ToString() });

        Leads.Clear();
        foreach (var lead in _allLeads
                     .Where(l => l.Stage != "Lost" && l.Stage != "Won")
                     .OrderBy(l => l.Stage)
                     .ThenByDescending(l => l.EstimatedValue))
            Leads.Add(lead);
    }

    private void SeedLeads()
    {
        string[] companies = ["TechNova Industries", "Apex Manufacturing", "BlueStar Logistics"];
        string[] products = ["Smart Controller", "Metal Housing", "Industrial Widget"];

        for (int i = 0; i < 3; i++)
        {
            _allLeads.Add(new LeadModel
            {
                CompanyName = companies[i],
                ProductInterest = products[i],
                EstimatedValue = (i + 1) * 12_000m,
                Stage = i == 0 ? "New" : i == 1 ? "Contacted" : "Negotiating",
                CreatedAt = _simulation.Engine.Clock.WorldTime
                                     .AddMinutes(-60 * i)
                                     .ToString("MM/dd HH:mm"),
            });
        }
    }
}