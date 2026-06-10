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

public partial class PricingTierModel : ObservableObject
{
    public string ProductName { get; set; } = "";
    [ObservableProperty] private decimal _basePrice;
    [ObservableProperty] private decimal _currentPrice;
    [ObservableProperty] private int _demandScore;
    public string Strategy { get; set; } = "Standard";
}

public partial class MarketingPricingViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<PricingTierModel> _products = [];

    [ObservableProperty] private decimal _avgPrice;
    [ObservableProperty] private int _reputation;
    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<PricingTierModel> Products { get; } = [];

    public MarketingPricingViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_products.Count == 0)
            SeedProducts();

        Refresh();
    }

    [RelayCommand]
    private void ApplyPremiumPricing()
    {
        foreach (var p in _products)
        {
            p.CurrentPrice = Math.Round(p.BasePrice * 1.25m, 2);
            p.Strategy = "Premium";
        }

        _simulation.Engine.Company.Reputation =
            Math.Min(100, _simulation.Engine.Company.Reputation + 3);

        StatusMessage = "✅ Premium pricing applied (+25%). Reputation +3. May reduce order volume.";
        Refresh();
    }

    [RelayCommand]
    private void ApplyDiscountPricing()
    {
        foreach (var p in _products)
        {
            p.CurrentPrice = Math.Round(p.BasePrice * 0.80m, 2);
            p.Strategy = "Discount";
            p.DemandScore = Math.Min(100, p.DemandScore + 15);
        }

        StatusMessage = "✅ Discount pricing applied (−20%). Demand boost expected.";
        Refresh();
    }

    [RelayCommand]
    private void ApplyStandardPricing()
    {
        foreach (var p in _products)
        {
            p.CurrentPrice = p.BasePrice;
            p.Strategy = "Standard";
        }

        StatusMessage = "✅ Standard pricing restored.";
        Refresh();
    }

    [RelayCommand]
    private void ApplyDynamicPricing()
    {
        var company = _simulation.Engine.Company;

        foreach (var p in _products)
        {
            decimal repFactor = 1.0m + (company.Reputation - 50) / 200m;
            p.CurrentPrice = Math.Round(p.BasePrice * repFactor, 2);
            p.Strategy = "Dynamic";
        }

        StatusMessage = "✅ Dynamic pricing applied based on current reputation.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;
        Reputation = company.Reputation;
        TotalRevenue = company.Revenue;

        // Sync demand with reputation
        foreach (var p in _products)
            p.DemandScore = Math.Clamp(company.Reputation + Random.Shared.Next(-10, 10), 0, 100);

        AvgPrice = _products.Count > 0
            ? _products.Average(p => p.CurrentPrice)
            : 0;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Avg Price", Value = $"${AvgPrice:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Revenue", Value = $"${TotalRevenue:N0}" });
        Kpis.Add(new KpiCardModel { Title = "Reputation", Value = $"{Reputation}/100" });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

        Products.Clear();
        foreach (var p in _products)
            Products.Add(p);
    }

    private void SeedProducts()
    {
        string[] names = ["Industrial Widget", "Smart Controller", "Metal Housing", "Control Board", "Sensor Module"];
        decimal[] prices = [85m, 120m, 65m, 95m, 110m];

        for (int i = 0; i < names.Length; i++)
        {
            _products.Add(new PricingTierModel
            {
                ProductName = names[i],
                BasePrice = prices[i],
                CurrentPrice = prices[i],
                DemandScore = Random.Shared.Next(40, 80),
                Strategy = "Standard",
            });
        }
    }
}