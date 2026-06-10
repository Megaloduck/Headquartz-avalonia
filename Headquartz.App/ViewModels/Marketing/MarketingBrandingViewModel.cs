using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class BrandActivityModel : ObservableObject
{
    public string Activity { get; set; } = "";
    public string Impact { get; set; } = "";
    public string Timestamp { get; set; } = "";
}

public partial class MarketingBrandingViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _brandScore;
    [ObservableProperty] private int _reputation;
    [ObservableProperty] private int _trustLevel;
    [ObservableProperty] private int _visibilityLevel;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<BrandActivityModel> ActivityLog { get; } = [];

    public MarketingBrandingViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        Refresh();
    }

    [RelayCommand]
    private void RunRebranding()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 10_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash for rebranding. Need $10,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;
        company.Reputation = Math.Min(100, company.Reputation + 10);
        VisibilityLevel = Math.Min(100, VisibilityLevel + 20);
        TrustLevel = Math.Min(100, TrustLevel + 5);

        LogActivity("Full Rebranding", "+10 Reputation, +20 Visibility, +5 Trust");
        StatusMessage = "✅ Rebranding complete. Cost: $10,000.";
        Refresh();
    }

    [RelayCommand]
    private void RunLogoRefresh()
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
        company.Reputation = Math.Min(100, company.Reputation + 3);
        VisibilityLevel = Math.Min(100, VisibilityLevel + 8);

        LogActivity("Logo Refresh", "+3 Reputation, +8 Visibility");
        StatusMessage = "✅ Logo refreshed. Cost: $2,000.";
        Refresh();
    }

    [RelayCommand]
    private void RunSponsorshipDeal()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 5_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash. Need $5,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;
        company.Reputation = Math.Min(100, company.Reputation + 7);
        TrustLevel = Math.Min(100, TrustLevel + 10);
        VisibilityLevel = Math.Min(100, VisibilityLevel + 12);

        LogActivity("Sponsorship Deal", "+7 Reputation, +10 Trust, +12 Visibility");
        StatusMessage = "✅ Sponsorship secured. Cost: $5,000.";
        Refresh();
    }

    [RelayCommand]
    private void RunPRStatement()
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
        company.Reputation = Math.Min(100, company.Reputation + 4);
        TrustLevel = Math.Min(100, TrustLevel + 8);

        // Resolve one company event
        var ev = company.Events.FirstOrDefault(e => !e.IsResolved);
        if (ev != null) ev.IsResolved = true;

        LogActivity("PR Statement", "+4 Reputation, +8 Trust, 1 Event Resolved");
        StatusMessage = "✅ PR statement released. Cost: $1,500.";
        Refresh();
    }

    private void LogActivity(string activity, string impact)
    {
        ActivityLog.Insert(0, new BrandActivityModel
        {
            Activity = activity,
            Impact = impact,
            Timestamp = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
        });

        if (ActivityLog.Count > 15)
            ActivityLog.RemoveAt(ActivityLog.Count - 1);
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;
        Reputation = company.Reputation;

        if (TrustLevel == 0) TrustLevel = Math.Clamp(company.Reputation - 5, 0, 100);
        if (VisibilityLevel == 0) VisibilityLevel = Math.Clamp(company.Reputation + 5, 0, 100);

        BrandScore = (Reputation + TrustLevel + VisibilityLevel) / 3;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Brand Score", Value = $"{BrandScore}/100" });
        Kpis.Add(new KpiCardModel { Title = "Reputation", Value = $"{Reputation}/100" });
        Kpis.Add(new KpiCardModel { Title = "Trust", Value = $"{TrustLevel}/100" });
        Kpis.Add(new KpiCardModel { Title = "Visibility", Value = $"{VisibilityLevel}/100" });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });
    }
}