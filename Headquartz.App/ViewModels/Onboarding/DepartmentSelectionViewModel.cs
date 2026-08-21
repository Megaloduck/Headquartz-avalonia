using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Headquartz.App.Models.Onboarding;
using Headquartz.App.Services;
using Headquartz.App.ViewModels;
using Headquartz.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Headquartz.App.ViewModels;

public partial class DepartmentSelectionViewModel : ViewModelBase
{
    private readonly OnboardingFlowService _flow;

    // ── State ─────────────────────────────────────────────────

    [ObservableProperty] private string _companyName = "";
    [ObservableProperty] private string _industryLabel = "";
    [ObservableProperty] private string _difficultyLabel = "";
    [ObservableProperty] private string _capitalDisplay = "";
    [ObservableProperty] private DepartmentSelectionCard? _selectedCard;
    [ObservableProperty] private bool _hasSelection;
    [ObservableProperty] private string _initialBudgetDisplay = "";
    [ObservableProperty] private bool _showPreMeetingPanel;

    /// <summary>
    /// Multiplayer players return to the Lobby to ready up after picking;
    /// solo players go straight into the simulation.
    /// </summary>
    [ObservableProperty] private string _confirmButtonLabel = "Enter Simulation";

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<DepartmentSelectionCard> Cards { get; } = [];

    // NOTE: "Manager" is intentionally omitted from every Title here — it's
    // reserved for GameDifficulty.Manager. Management is special-cased as
    // "Board Chairman" for its player-facing title.
    private static readonly (DepartmentType Department, string Title, string DeptLabel,
        string Emoji, string Color, string Responsibilities)[] DepartmentData =
    [
        (DepartmentType.HumanResources,
            "Human Resources", "Human Resources", "👥", "#8B5CF6",
            "Workforce · Hiring · Morale · Training · Payroll · Resignations"),

        (DepartmentType.Finance,
            "Finance", "Finance", "💰", "#10B981",
            "Budgets · Cash Flow · Loans · Payroll Risk · Audits"),

        (DepartmentType.Sales,
            "Sales", "Sales", "📈", "#3B82F6",
            "Revenue · Orders · Clients · Pipeline · Deadlines"),

        (DepartmentType.Marketing,
            "Marketing", "Marketing", "📣", "#F59E0B",
            "Campaigns · Brand · Reputation · Research · Demand"),

        (DepartmentType.Production,
            "Production", "Production", "🏭", "#EF4444",
            "Manufacturing · Maintenance · Quality and Quantity Controls"),

        (DepartmentType.Warehouse,
            "Warehouse", "Warehouse", "📦", "#F97316",
            "Inventory · Stock Levels · Resource Planning · Storage"),

        (DepartmentType.Logistics,
            "Logistics", "Logistics", "🚚", "#06B6D4",
            "Shipments · Routes · Delivery SLAs · Fleet and Vehicle Controls"),

        (DepartmentType.Management,
            "Board Chairman", "Management", "🏛️", "#EAB308",
            "Full Oversight · All Reports · Strategic Decisions"),
    ];

    public DepartmentSelectionViewModel(OnboardingFlowService flow)
    {
        _flow = flow;

        var config = flow.SessionConfig;

        CompanyName = config?.CompanyName ?? "Unknown Co.";
        IndustryLabel = config?.Industry.ToString() ?? "";
        DifficultyLabel = config?.Difficulty.ToString() ?? "";
        CapitalDisplay = $"${config?.InitialCapital:N0}";

        // Multiplayer players (host or joiner) return to the Lobby after
        // picking — only the host's later "Start Game" actually launches
        // the simulation. Solo players go straight in.
        ConfirmButtonLabel = config?.IsMultiplayer == true
            ? "Confirm & Return to Lobby"
            : "Enter Simulation";

        BuildCards(config);
    }

    private void BuildCards(GameSessionConfig? config)
    {
        var takenDepartments = config?.Players
            .Where(p => !p.IsLocalPlayer && p.AssignedRole.HasValue)
            .Select(p => p.AssignedRole!.Value)
            .ToHashSet() ?? new HashSet<DepartmentType>();

        foreach (var (department, title, deptLabel, emoji, color, resp) in DepartmentData)
        {
            bool taken = takenDepartments.Contains(department);
            string? takenBy = config?.Players
                .FirstOrDefault(p => p.AssignedRole == department && !p.IsLocalPlayer)?.Username;

            Cards.Add(new DepartmentSelectionCard
            {
                Role = department,
                Title = title,
                Department = deptLabel,
                Emoji = emoji,
                AccentColor = color,
                ResponsibilitiesSummary = resp,

                IsTaken = taken,
                TakenByUsername = takenBy,
            });
        }
    }

    [RelayCommand]
    private void SelectCard(DepartmentSelectionCard card)
    {
        if (card.IsTaken) return;

        // Deselect previous
        foreach (var c in Cards)
            c.IsSelected = false;

        card.IsSelected = true;
        SelectedCard = card;
        HasSelection = true;

        // Show initial budget for selected department
        InitialBudgetDisplay = card.Role switch
        {
            DepartmentType.Finance => "Controls the company treasury",
            DepartmentType.HumanResources => "Manages workforce budget",
            DepartmentType.Production => "Allocates production resources",
            _ => $"Manages {card.Department} operations",
        };
    }

    [RelayCommand]
    private void ConfirmSelection()
    {
        if (SelectedCard == null) return;

        ShowPreMeetingPanel = true;
    }

    [RelayCommand]
    private void EnterGame()
    {
        if (SelectedCard == null) return;

        // SelectDepartment() itself decides where to go next:
        //   - multiplayer  -> back to Lobby (wait/ready)
        //   - solo         -> straight to Gameplay
        _flow.SelectDepartment(SelectedCard.Role);
    }

    [RelayCommand]
    private void GoBack()
    {
        // Multiplayer players (host or joiner) came from the Lobby, either
        // directly (joiners) or via the host's CompanySetup detour.
        // Solo players came from CompanySetup and should return there.
        if (_flow.SessionConfig?.IsMultiplayer == true)
            _flow.NavigateTo(OnboardingScreen.Lobby);
        else
            _flow.NavigateTo(OnboardingScreen.CompanySetup);
    }
}