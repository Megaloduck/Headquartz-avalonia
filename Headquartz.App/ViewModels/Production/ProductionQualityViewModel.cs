using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Entities;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Headquartz.App.ViewModels;

public partial class QualityIncidentModel : ObservableObject
{
    public string Product { get; set; } = "";
    public string Issue { get; set; } = "";
    public string Severity { get; set; } = "";
    public int DefectCount { get; set; }
    public string DetectedAt { get; set; } = "";
    [ObservableProperty] private bool _isResolved;
}

public partial class ProductionQualityViewModel : ViewModelBase
{
    private readonly SimulationService _simulation;

    private static readonly List<QualityIncidentModel> _incidents = [];

    [ObservableProperty] private int _qualityScore;
    [ObservableProperty] private int _passRate;
    [ObservableProperty] private int _totalInspected;
    [ObservableProperty] private int _defectsFound;
    [ObservableProperty] private int _activeIncidents;
    [ObservableProperty] private decimal _companyCash;
    [ObservableProperty] private string _statusMessage = "";

    public ObservableCollection<KpiCardModel> Kpis { get; } = [];
    public ObservableCollection<QualityIncidentModel> Incidents { get; } = [];

    public ProductionQualityViewModel(SimulationService simulation)
    {
        _simulation = simulation;

        _simulation.Engine.OnUpdated +=
            () => Dispatcher.UIThread.Post(Refresh);

        if (_incidents.Count == 0)
            SeedIncidents();

        Refresh();
    }

    [RelayCommand]
    private void RunInspection()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 1_500m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash for quality inspection. Need $1,500.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        int inspected = Random.Shared.Next(20, 60);
        int defects = Random.Shared.Next(0, inspected / 4);
        TotalInspected += inspected;
        DefectsFound += defects;

        if (defects > 5)
        {
            string[] products = ["Industrial Widget", "Smart Controller", "Metal Housing", "Control Board"];
            string[] issues = ["Surface defects", "Calibration failure", "Weld weakness", "Component misalignment"];

            _incidents.Add(new QualityIncidentModel
            {
                Product = products[Random.Shared.Next(products.Length)],
                Issue = issues[Random.Shared.Next(issues.Length)],
                Severity = defects > 10 ? "High" : "Medium",
                DefectCount = defects,
                DetectedAt = _simulation.Engine.Clock.WorldTime.ToString("MM/dd HH:mm"),
            });

            company.Reputation = Math.Max(0, company.Reputation - 2);
            StatusMessage = $"⚠ Inspection complete. {defects} defects found. Reputation −2. Cost: $1,500.";
        }
        else
        {
            StatusMessage = $"✅ Inspection passed. {defects} minor defects. All within tolerance. Cost: $1,500.";
        }

        Refresh();
    }

    [RelayCommand]
    private void ResolveIncident(QualityIncidentModel incident)
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 2_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash to resolve incident. Need $2,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;
        incident.IsResolved = true;

        StatusMessage = $"✅ Incident resolved: {incident.Issue}. Cost: $2,000.";
        Refresh();
    }

    [RelayCommand]
    private void RunFullAudit()
    {
        var company = _simulation.Engine.Company;
        const decimal cost = 5_000m;

        if (company.Cash < cost)
        {
            StatusMessage = "❌ Insufficient cash for full quality audit. Need $5,000.";
            return;
        }

        company.Cash -= cost;
        company.Expenses += cost;

        // Resolve all incidents
        foreach (var i in _incidents)
            i.IsResolved = true;

        company.Reputation = Math.Min(100, company.Reputation + 5);

        StatusMessage = "✅ Full quality audit complete. All incidents cleared. Reputation +5. Cost: $5,000.";
        Refresh();
    }

    private void Refresh()
    {
        var company = _simulation.Engine.Company;
        CompanyCash = company.Cash;

        ActiveIncidents = _incidents.Count(i => !i.IsResolved);

        // Quality score based on dept efficiency and active incidents
        var prodDept = company.Departments
            .FirstOrDefault(d => d.Type == DepartmentType.Production);

        int baseScore = prodDept?.Efficiency ?? 50;
        QualityScore = Math.Max(0, baseScore - (ActiveIncidents * 5));

        PassRate = TotalInspected > 0
            ? (int)((double)(TotalInspected - DefectsFound) / TotalInspected * 100)
            : 100;

        Kpis.Clear();
        Kpis.Add(new KpiCardModel { Title = "Quality Score", Value = $"{QualityScore}/100" });
        Kpis.Add(new KpiCardModel { Title = "Pass Rate", Value = $"{PassRate}%" });
        Kpis.Add(new KpiCardModel { Title = "Inspected", Value = TotalInspected.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Defects", Value = DefectsFound.ToString() });
        Kpis.Add(new KpiCardModel { Title = "Active Incidents", Value = ActiveIncidents.ToString() });

        Incidents.Clear();
        foreach (var i in _incidents.OrderBy(i => i.IsResolved ? 1 : 0))
            Incidents.Add(i);
    }

    private void SeedIncidents()
    {
        _incidents.Add(new QualityIncidentModel
        {
            Product = "Industrial Widget",
            Issue = "Surface finish below spec",
            Severity = "Low",
            DefectCount = 3,
            DetectedAt = "Initial",
            IsResolved = false,
        });
    }
}