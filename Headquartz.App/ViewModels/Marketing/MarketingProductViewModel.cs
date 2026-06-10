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

public partial class ProductResearchModel : ObservableObject
{
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public int MarketFit { get; set; }
    public decimal DevelopmentCost { get; set; }
    public int TimeToMarket { get; set; }
    public string Status { get; set; } = "Concept";
    public string StartedAt { get; set; } = "";
}

public partial class MarketingProductViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<ProductResearchModel> _pipeline = [];

    [ObservableProperty] private int _conceptCount;
    [ObservableProperty] private int _inResearchCount;
    [ObservableProperty] private int _readyCount;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<ProductResearchModel> Pipeline { get; } = [];

    public MarketingProductViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_pipeline.Count == 0)
            SeedPipeline();

        Refresh();
    }

    [RelayCommand]
    private void StartResearch(ProductResearchModel product)
    {
        var company = _simulation.Engine.Company;

        if (company.Cash < product.DevelopmentCost)
        {
            StatusMessage = $"❌ Insufficient cash. Need ${product.DevelopmentCost:N0}.";
            return;
        }

        company.Cash -= product.DevelopmentCost;
        company.Expenses += product.DevelopmentCost;
        product.Status = "In Research";

        StatusMessage = $"✅ Research started for {product.Name}. Cost: ${product.DevelopmentCost:N0}.";
        Refresh();
    }

    [RelayCommand]
    private void AdvanceToReady(ProductResearchModel product)
    {
        product.Status = "Ready";
        _simulation.Engine.Company.Reputation =
            Math.Min(100, _simulation.Engine.Company.Reputation + 5);

        StatusMessage = $"✅ {product.Name} research complete. Ready for production. Reputation +5.";
        Refresh();
    }

    [RelayCommand]
    private void AddConcept()
    {
        string[] names = ["Modular Frame Unit", "Precision Sensor Array", "Compact Drive System",
                          "Smart Interface Board", "Lightweight Housing Kit", "Adaptive Control Module"];
        string[] categories = ["Hardware", "Electronics", "Mechanical", "Software", "Assembly"];

        _pipeline.Add(new ProductResearchModel
        {
            Name = names[Random.Shared.Next(names.Length)],
            Category = categories[Random.Shared.Next(categories.Length)],
            MarketFit = Random.Shared.Next(40, 90),
            DevelopmentCost = Random.Shared.Next(3, 15) * 1_000m,
            TimeToMarket = Random.Shared.Next(5, 20),
            Status = "Concept",
            StartedAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd"),
        });

        StatusMessage = "✅ New product concept added to pipeline.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        ConceptCount = _pipeline.Count(p => p.Status == "Concept");
        InResearchCount = _pipeline.Count(p => p.Status == "In Research");
        ReadyCount = _pipeline.Count(p => p.Status == "Ready");

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Concepts", Value = ConceptCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "In Research", Value = InResearchCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Ready", Value = ReadyCount.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Cash", Value = $"${CompanyCash:N0}" });

        Pipeline.Clear();
        foreach (var p in _pipeline.OrderBy(p => p.Status == "Ready" ? 2 : p.Status == "In Research" ? 1 : 0))
            Pipeline.Add(p);
    }

    private void SeedPipeline()
    {
        _pipeline.Add(new ProductResearchModel
        {
            Name = "Next-Gen Control Board",
            Category = "Electronics",
            MarketFit = 78,
            DevelopmentCost = 8_000m,
            TimeToMarket = 12,
            Status = "Concept",
            StartedAt = "01/01",
        });
        _pipeline.Add(new ProductResearchModel
        {
            Name = "Lightweight Housing v2",
            Category = "Mechanical",
            MarketFit = 65,
            DevelopmentCost = 5_000m,
            TimeToMarket = 8,
            Status = "In Research",
            StartedAt = "01/03",
        });
    }
}