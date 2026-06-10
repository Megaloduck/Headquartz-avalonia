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

public partial class IncentiveTierModel : ObservableObject
{
    public string Name { get; set; } = "";
    public string Requirement { get; set; } = "";
    public string Reward { get; set; } = "";
    public string Emoji { get; set; } = "";
    public bool IsUnlocked { get; set; }
}

public partial class SalesIncentivesViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    [ObservableProperty] private int _totalDelivered;
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _reputation;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<IncentiveTierModel> Tiers { get; } = [];

    public SalesIncentivesViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        BuildTiers();
        Refresh();
    }

    [RelayCommand]
    private void RunSalesBonus()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 5_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash for sales bonus. Need $5,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        foreach (var emp in company.Employees
                     .Where(e => e.Department == Headquartz.Domain.Enums.DepartmentType.Sales))
        {
            emp.Morale = Math.Min(100, emp.Morale + 20);
            emp.Productivity = Math.Min(100, emp.Productivity + 10);
        }

        company.Reputation = Math.Min(100, company.Reputation + 3);

        StatusMessage = "✅ Sales bonus paid. Team morale and reputation boosted.";
        Refresh();
    }

    [RelayCommand]
    private void RunDiscountDrive()
    {
        var company = _simulation.Engine.Company;
        company.Reputation = Math.Min(100, company.Reputation + 5);
        StatusMessage = "✅ Discount drive launched. Reputation +5. More orders expected.";
        Refresh();
    }

    [RelayCommand]
    private void RunCommissionProgram()
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

        foreach (var emp in company.Employees
                     .Where(e => e.Department == Headquartz.Domain.Enums.DepartmentType.Sales))
            emp.Productivity = Math.Min(100, emp.Productivity + 15);

        StatusMessage = "✅ Commission program active. Sales productivity +15.";
        Refresh();
    }

    private void BuildTiers()
    {
        Tiers.Clear();
        Tiers.Add(new IncentiveTierModel
        {
            Name = "Bronze",
            Requirement = "10 orders delivered",
            Reward = "+5 Reputation",
            Emoji = "🥉",
        });
        Tiers.Add(new IncentiveTierModel
        {
            Name = "Silver",
            Requirement = "25 orders delivered",
            Reward = "+10 Reputation, Team Morale +10",
            Emoji = "🥈",
        });
        Tiers.Add(new IncentiveTierModel
        {
            Name = "Gold",
            Requirement = "50 orders delivered",
            Reward = "+20 Reputation, Productivity +15",
            Emoji = "🥇",
        });
        Tiers.Add(new IncentiveTierModel
        {
            Name = "Platinum",
            Requirement = "$500,000 total revenue",
            Reward = "Full team bonus, Reputation +25",
            Emoji = "💎",
        });
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;

        TotalDelivered = company.Orders
            .Count(o => o.Status == Headquartz.Domain.Enums.OrderStatus.Delivered);
        TotalRevenue = company.Revenue;
        Reputation = company.Reputation;
        CompanyCash = company.Cash;

        // Update tier unlock status
        foreach (var tier in Tiers)
        {
            tier.IsUnlocked = tier.Name switch
            {
                "Bronze" => TotalDelivered >= 10,
                "Silver" => TotalDelivered >= 25,
                "Gold" => TotalDelivered >= 50,
                "Platinum" => TotalRevenue >= 500_000m,
                _ => false,
            };
        }

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Orders Delivered", Value = TotalDelivered.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Total Revenue", Value = $"${TotalRevenue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Reputation", Value = $"{Reputation}/100" });
        Kpis.Add(new KpiCardModel { Title = "Company Cash", Value = $"${CompanyCash:N0}" });
    }
}