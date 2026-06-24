using System;
using System.Collections.ObjectModel;
using System.Linq;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models.Onboarding;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;
using Headquartz.Domain.Enums;
using Headquartz.Simulation.Modules.Base;

namespace Headquartz.App.ViewModels;

public partial class CompanySetupViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;

    // ── Form ──────────────────────────────────────────────────

    [ObservableProperty] private string _companyName = "";
    [ObservableProperty] private IndustryType _selectedIndustry = IndustryType.Food;
    [ObservableProperty] private GameDifficulty _selectedDifficulty = GameDifficulty.Manager;
    [ObservableProperty] private decimal _initialCapital = 100_000m;
    [ObservableProperty] private string _errorMessage = "";
    [ObservableProperty] private bool _hasError;

    // ── Computed preview ──────────────────────────────────────

    [ObservableProperty] private string _capitalDisplay = "$100,000";
    [ObservableProperty] private string _difficultyDescription = "";
    [ObservableProperty] private string _industryDescription = "";

    // ── Selections ────────────────────────────────────────────

    public ObservableCollection<IndustryOptionModel> Industries { get; } = [];
    public ObservableCollection<DifficultyOptionModel> Difficulties { get; } = [];
    public ObservableCollection<DepartmentRelevanceModel> DepartmentRelevancePreview { get; } = [];

    private static readonly string[] SuggestedNames =
    [
        "Apex Industries", "NovaCorp", "Pinnacle Group",
        "Ironclad Ltd", "Meridian Systems", "Atlas Holdings",
        "Crestview Co", "Sterling Dynamics", "Vanguard Inc",
        "Horizon Enterprises",
    ];

    public CompanySetupViewModel(OnboardingFlowService flow)
    {
        _flow = flow;

        BuildIndustries();
        BuildDifficulties();
        UpdateDescriptions();
        BuildDepartmentRelevance();

        // Pre-fill if returning
        if (!string.IsNullOrEmpty(flow.SessionConfig?.CompanyName))
            CompanyName = flow.SessionConfig.CompanyName;
    }

    partial void OnSelectedDifficultyChanged(GameDifficulty value)
    {
        UpdateDescriptions();

        InitialCapital = value switch
        {
            GameDifficulty.Trainee => 150_000m,
            GameDifficulty.Manager => 100_000m,
            GameDifficulty.Director => 60_000m,
            GameDifficulty.Chairman => 30_000m,
            _ => 100_000m,
        };

        CapitalDisplay = $"${InitialCapital:N0}";

        foreach (var d in Difficulties)
            d.IsSelected = d.Difficulty == value;
    }

    partial void OnSelectedIndustryChanged(IndustryType value)
    {
        UpdateDescriptions();
        BuildDepartmentRelevance();

        foreach (var i in Industries)
            i.IsSelected = i.Industry == value;
    }

    private void UpdateDescriptions()
    {
        DifficultyDescription = SelectedDifficulty switch
        {
            GameDifficulty.Trainee => "More starting capital. Slower event frequency. Recommended for new players.",
            GameDifficulty.Manager => "Standard experience. Balanced events and budget.",
            GameDifficulty.Director => "Less capital. Faster crises. For experienced players.",
            GameDifficulty.Chairman => "Bare-minimum budget. Relentless chaos. Expert only.",
            _ => "",
        };

        IndustryDescription = SelectedIndustry switch
        {
            IndustryType.Food => "Perishable goods with spoilage risk. FIFO inventory and cold-chain logistics are critical. Tight margins driven by shelf-life pressure.",
            IndustryType.Beverage => "Blending, bottling, and distribution. Excise taxes, seasonal demand, and distributor relationships define success.",
            IndustryType.Entertainment => "Content creation and events. Project-based revenue, talent-heavy, and hype-driven marketing.",
            IndustryType.Automotive => "Complex assembly with JIT supply chains. Quality control and precision production dominate. Capital-intensive.",
            IndustryType.Fashion => "Trend-driven with seasonal cycles. Fast fashion requires rapid turnaround; luxury rewards brand and scarcity.",
            _ => "",
        };
    }

    private void BuildDepartmentRelevance()
    {
        DepartmentRelevancePreview.Clear();

        var module = IndustryModuleRegistry.GetModule(SelectedIndustry);
        var relevances = module?.GetDepartmentRelevances();

        foreach (var dept in Enum.GetValues<DepartmentType>())
        {
            var relevance = DepartmentRelevance.Standard;
            if (relevances?.TryGetValue(dept, out var found) == true)
                relevance = found;

            var (emoji, label, color) = relevance switch
            {
                DepartmentRelevance.Critical => ("🔴", "Critical", "BrushAlertCriticalBg"),
                DepartmentRelevance.Important => ("🟠", "Important", "BrushAlertWarningBg"),
                DepartmentRelevance.Standard => ("🟡", "Standard", "BrushAccentPrimary"),
                DepartmentRelevance.Light => ("🟢", "Light", "BrushSuccess"),
                DepartmentRelevance.Outsourced => ("⚪", "Outsourced", "BrushBorderStrong"),
                _ => ("🟡", "Standard", "BrushAccentPrimary"),
            };

            DepartmentRelevancePreview.Add(new DepartmentRelevanceModel
            {
                Department = dept,
                DepartmentName = GetDepartmentDisplayName(dept),
                Emoji = emoji,
                RelevanceLabel = label,
                Relevance = relevance,
                BrushResourceKey = color,
            });
        }
    }

    private static string GetDepartmentDisplayName(DepartmentType dept) => dept switch
    {
        DepartmentType.Management => "Management",
        DepartmentType.HumanResources => "Human Resources",
        DepartmentType.Finance => "Finance",
        DepartmentType.Sales => "Sales",
        DepartmentType.Marketing => "Marketing",
        DepartmentType.Production => "Production",
        DepartmentType.Warehouse => "Warehouse",
        DepartmentType.Logistics => "Logistics",
        _ => dept.ToString(),
    };

    [RelayCommand]
    private void RandomName()
    {
        CompanyName = SuggestedNames[Random.Shared.Next(SuggestedNames.Length)];
    }

    [RelayCommand]
    private void SelectIndustry(IndustryOptionModel model)
    {
        SelectedIndustry = model.Industry;
    }

    [RelayCommand]
    private void SelectDifficulty(DifficultyOptionModel model)
    {
        SelectedDifficulty = model.Difficulty;
    }

    [RelayCommand]
    private void Confirm()
    {
        string name = CompanyName.Trim();

        if (string.IsNullOrEmpty(name))
        {
            ErrorMessage = "Please enter a company name.";
            HasError = true;
            return;
        }

        if (name.Length < 2 || name.Length > 40)
        {
            ErrorMessage = "Company name must be 2–40 characters.";
            HasError = true;
            return;
        }

        _flow.ConfirmCompanySetup(
            name,
            SelectedIndustry,
            SelectedDifficulty,
            InitialCapital);
    }

    [RelayCommand]
    private void GoBack()
    {
        _flow.NavigateTo(OnboardingScreen.Lobby);
    }

    private void BuildIndustries()
    {
        var items = new[]
        {
            (IndustryType.Food,          "🍞", "Food"),
            (IndustryType.Beverage,      "🥤", "Beverage"),
            (IndustryType.Entertainment, "🎬", "Entertainment"),
            (IndustryType.Automotive,    "🚗", "Automotive"),
            (IndustryType.Fashion,       "👗", "Fashion"),
        };

        foreach (var (type, emoji, label) in items)
        {
            Industries.Add(new IndustryOptionModel
            {
                Industry = type,
                Emoji = emoji,
                Label = label,
                IsSelected = type == SelectedIndustry,
            });
        }
    }

    private void BuildDifficulties()
    {
        var items = new[]
        {
            (GameDifficulty.Trainee,  "🟢", "Trainee",  "$150k start"),
            (GameDifficulty.Manager,  "🟡", "Manager",  "$100k start"),
            (GameDifficulty.Director, "🟠", "Director", "$60k start"),
            (GameDifficulty.Chairman, "🔴", "Chairman", "$30k start"),
        };

        foreach (var (diff, dot, label, sub) in items)
        {
            Difficulties.Add(new DifficultyOptionModel
            {
                Difficulty = diff,
                DotEmoji = dot,
                Label = label,
                SubLabel = sub,
                IsSelected = diff == SelectedDifficulty,
            });
        }
    }
}

public partial class IndustryOptionModel : ObservableObject
{
    public IndustryType Industry { get; set; }
    public string Emoji { get; set; } = "";
    public string Label { get; set; } = "";
    [ObservableProperty] private bool _isSelected;
}

public partial class DifficultyOptionModel : ObservableObject
{
    public GameDifficulty Difficulty { get; set; }
    public string DotEmoji { get; set; } = "";
    public string Label { get; set; } = "";
    public string SubLabel { get; set; } = "";
    [ObservableProperty] private bool _isSelected;
}

public partial class DepartmentRelevanceModel : ObservableObject
{
    public DepartmentType Department { get; set; }
    public string DepartmentName { get; set; } = "";
    public string Emoji { get; set; } = "";
    public string RelevanceLabel { get; set; } = "";
    public DepartmentRelevance Relevance { get; set; }
    public string BrushResourceKey { get; set; } = "";
}
