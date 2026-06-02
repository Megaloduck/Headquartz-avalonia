using System;
using System.Collections.Generic;
using System.Text;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models.Onboarding;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;

namespace Headquartz.App.ViewModels;

public partial class CompanySetupViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;

    // ── Form ──────────────────────────────────────────────────

    [ObservableProperty] private string _companyName = "";
    [ObservableProperty] private IndustryType _selectedIndustry = IndustryType.Manufacturing;
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
            IndustryType.Manufacturing => "Physical goods production. Inventory and logistics are critical.",
            IndustryType.Technology => "Fast-paced software development. Talent and marketing dominate.",
            IndustryType.Retail => "High order volume. Customer satisfaction and sales pressure.",
            IndustryType.Logistics => "Transportation focused. Route management and delivery SLAs.",
            IndustryType.Finance => "Capital-heavy operations. Loan and budget management.",
            IndustryType.Healthcare => "Regulated environment. HR compliance and supply chain.",
            IndustryType.Energy => "Infrastructure operations. Production efficiency focused.",
            IndustryType.Media => "Campaign-heavy. Marketing and brand reputation driven.",
            _ => "",
        };
    }

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

        _flow.ConfirmCompanySetup(name, SelectedIndustry, SelectedDifficulty, InitialCapital);
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
            (IndustryType.Manufacturing, "🏭", "Manufacturing"),
            (IndustryType.Technology,    "💻", "Technology"),
            (IndustryType.Retail,        "🛍️", "Retail"),
            (IndustryType.Logistics,     "🚚", "Logistics"),
            (IndustryType.Finance,       "💰", "Finance"),
            (IndustryType.Healthcare,    "🏥", "Healthcare"),
            (IndustryType.Energy,        "⚡", "Energy"),
            (IndustryType.Media,         "📺", "Media"),
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
            (GameDifficulty.Trainee,    "🟢", "Trainee",   "$150k start"),
            (GameDifficulty.Manager,    "🟡", "Manager",   "$100k start"),
            (GameDifficulty.Director,   "🟠", "Director",  "$60k start"),
            (GameDifficulty.Chairman,   "🔴", "Chairman",  "$30k start"),
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
