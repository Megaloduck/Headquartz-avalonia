using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class CampaignModel : ObservableObject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Target { get; set; } = "";
    public decimal Budget { get; set; }
    public int ReputationGain { get; set; }
    public string Status { get; set; } = "Active";
    public int TicksRemaining { get; set; }
    public string LaunchedAt { get; set; } = "";
}

public partial class MarketingCampaignsViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<CampaignModel> _campaigns = [];

    [ObservableProperty] private int _activeCampaigns;
    [ObservableProperty] private int _completedCampaigns;
    [ObservableProperty] private decimal _totalSpent;
    [ObservableProperty] private int _reputation;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<CampaignModel> Campaigns { get; } = [];

    public MarketingCampaignsViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void LaunchSocialMedia()
        => LaunchCampaign("Social Media Blitz", "Digital", "Broad Audience", 3_000m, 5, 8);

    [RelayCommand]
    private void LaunchEmailCampaign()
        => LaunchCampaign("Email Campaign", "Direct", "Existing Clients", 1_500m, 3, 6);

    [RelayCommand]
    private void LaunchBrandAwareness()
        => LaunchCampaign("Brand Awareness Drive", "PR", "General Public", 5_000m, 8, 12);

    [RelayCommand]
    private void LaunchProductLaunch()
        => LaunchCampaign("Product Launch Event", "Event", "Industry Leads", 8_000m, 12, 10);

    [RelayCommand]
    private void CancelCampaign(CampaignModel campaign)
    {
        campaign.Status = "Cancelled";
        _campaigns.Remove(campaign);
        StatusMessage = $"⛔ Campaign '{campaign.Name}' cancelled.";
        Refresh();
    }

    private void LaunchCampaign(
        string name, string type, string target,
        decimal budget, int repGain, int durationTicks)
    {
        var company = _simulation.Engine.Company;

        if (company.Cash < budget)
        {
            StatusMessage = $"❌ Insufficient cash for {name}. Need ${budget:N0}.";
            return;
        }

        company.Cash -= budget;
        company.Expenses += budget;
        company.Reputation = Math.Min(100, company.Reputation + repGain);

        _campaigns.Add(new CampaignModel
        {
            Name = name,
            Type = type,
            Target = target,
            Budget = budget,
            ReputationGain = repGain,
            Status = "Active",
            TicksRemaining = durationTicks,
            LaunchedAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
        });

        _simulation.Engine.Commands.Enqueue(new CreateTaskCommand
        {
            PlayerId = "marketing-manager",
            Name = name,
            Department = DepartmentType.Marketing,
            Priority = TaskPriority.High,
            DurationTicks = durationTicks,
            BudgetCost = 0m,
        });

        StatusMessage = $"✅ '{name}' launched. Reputation +{repGain}. Cost: ${budget:N0}.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;
        Reputation = company.Reputation;

        // Tick down active campaigns
        foreach (var c in _campaigns.Where(c => c.Status == "Active"))
        {
            c.TicksRemaining = Math.Max(0, c.TicksRemaining - 1);
            if (c.TicksRemaining <= 0)
                c.Status = "Completed";
        }

        ActiveCampaigns = _campaigns.Count(c => c.Status == "Active");
        CompletedCampaigns = _campaigns.Count(c => c.Status == "Completed");
        TotalSpent = _campaigns.Sum(c => c.Budget);

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Active", Value = ActiveCampaigns.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Completed", Value = CompletedCampaigns.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Total Spent", Value = $"${TotalSpent:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Reputation", Value = $"{Reputation}/100" });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

        Campaigns.Clear();
        foreach (var c in _campaigns.OrderBy(c => c.Status == "Completed" ? 1 : 0))
            Campaigns.Add(c);
    }
}