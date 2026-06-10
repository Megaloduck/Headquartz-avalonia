using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class MarketInsightModel : ObservableObject
{
    public string Category { get; set; } = "";
    public string Finding { get; set; } = "";
    public string Impact { get; set; } = "";
    public string Recommendation { get; set; } = "";
    public string DiscoveredAt { get; set; } = "";
}

public partial class MarketingResearchViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<MarketInsightModel> _insights = [];

    [ObservableProperty] private int _marketDemand;
    [ObservableProperty] private int _competitorThreat;
    [ObservableProperty] private int _brandAwareness;
    [ObservableProperty] private int _customerSatisfaction;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<MarketInsightModel> Insights { get; } = [];

    public MarketingResearchViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void RunCustomerSurvey()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 2_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash. Need $2,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        int satisfaction = Math.Clamp(
            company.Reputation + Random.Shared.Next(-10, 10), 0, 100);

        CustomerSatisfaction = satisfaction;

        _insights.Add(new MarketInsightModel
        {
            Category = "Customer Feedback",
            Finding = $"Survey: {satisfaction}% customer satisfaction score",
            Impact = satisfaction < 50 ? "High" : "Low",
            Recommendation = satisfaction < 50
                ? "Improve delivery times and product quality"
                : "Maintain current service standards",
            DiscoveredAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
        });

        StatusMessage = $"✅ Customer survey complete. Satisfaction: {satisfaction}%. Cost: $2,000.";
        Refresh();
    }

    [RelayCommand]
    private void RunCompetitorAnalysis()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 3_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash. Need $3,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        int threat = Random.Shared.Next(20, 80);
        CompetitorThreat = threat;

        _insights.Add(new MarketInsightModel
        {
            Category = "Competitor Intelligence",
            Finding = $"Competitor threat level assessed at {threat}/100",
            Impact = threat > 60 ? "High" : "Medium",
            Recommendation = threat > 60
                ? "Increase marketing spend and differentiation"
                : "Monitor competitor activity quarterly",
            DiscoveredAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
        });

        StatusMessage = $"✅ Competitor analysis done. Threat level: {threat}/100. Cost: $3,000.";
        Refresh();
    }

    [RelayCommand]
    private void RunDemandForecast()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 1_500m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash. Need $1,500.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        int demand = Math.Clamp(
            company.Reputation + Random.Shared.Next(-15, 25), 0, 100);

        MarketDemand = demand;

        _insights.Add(new MarketInsightModel
        {
            Category = "Demand Analysis",
            Finding = $"Market demand index: {demand}/100",
            Impact = demand < 30 ? "Critical" : "Informational",
            Recommendation = demand < 30
                ? "Launch promotional campaigns immediately"
                : "Stable demand — maintain pipeline velocity",
            DiscoveredAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
        });

        StatusMessage = $"✅ Demand forecast complete. Index: {demand}/100. Cost: $1,500.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        BrandAwareness = Math.Clamp(company.Reputation + 10, 0, 100);
        if (MarketDemand == 0) MarketDemand = Math.Clamp(company.Reputation, 10, 90);
        if (CustomerSatisfaction == 0) CustomerSatisfaction = Math.Clamp(company.Reputation, 20, 95);
        if (CompetitorThreat == 0) CompetitorThreat = Random.Shared.Next(20, 60);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Market Demand", Value = $"{MarketDemand}/100" });
        Kpis.Add(new KpiCardModel { Title = "Competitor Threat", Value = $"{CompetitorThreat}/100" });
        Kpis.Add(new KpiCardModel { Title = "Brand Awareness", Value = $"{BrandAwareness}/100" });
        Kpis.Add(new KpiCardModel { Title = "Satisfaction", Value = $"{CustomerSatisfaction}/100" });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

        Insights.Clear();
        foreach (var i in _insights.OrderByDescending(i => i.DiscoveredAt))
            Insights.Add(i);
    }
}