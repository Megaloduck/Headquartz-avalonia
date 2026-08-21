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
    /// Label for the confirm button at the bottom of the pre-meeting panel.
    /// Multiplayer players return to the Lobby to ready up; solo players
    /// go straight into the simulation.
    /// </summary>
    [ObservableProperty] private string _confirmButtonLabel = "Enter Simulation";

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<DepartmentSelectionCard> Cards { get; } = [];

    private static readonly (PlayerRole Role, string Title, string Dept,
        string Emoji, string Color, string Responsibilities)[] RoleData =
    [
        (PlayerRole.HumanResourcesManager,
            "HR Manager", "Human Resources", "👥", "#8B5CF6",
            "Workforce · Hiring · Morale · Training · Payroll · Resignations"),

        (PlayerRole.FinanceManager,
            "Finance Manager", "Finance", "💰", "#10B981",
            "Budgets · Cash Flow · Loans · Payroll Risk · Audits"),

        (PlayerRole.SalesManager,
            "Sales Manager", "Sales", "📈", "#3B82F6",
            "Revenue · Orders · Clients · Pipeline · Deadlines"),

        (PlayerRole.MarketingManager,
            "Marketing Manager", "Marketing", "📣", "#F59E0B",
            "Campaigns · Brand · Reputation · Research · Demand"),

        (PlayerRole.ProductionManager,
            "Production Manager", "Production", "🏭", "#EF4444",
            "Manufacturing · Maintenance · Quality and Quantity Controls"),

        (PlayerRole.WarehouseManager,
            "Warehouse Manager", "Warehouse", "📦", "#F97316",
            "Inventory · Stock Levels · Resource Planning · Storage"),

        (PlayerRole.LogisticsManager,
            "Logistics Manager", "Logistics", "🚚", "#06B6D4",
            "Shipments · Routes · Delivery SLAs · Fleet and Vehicle Controls"),

        (PlayerRole.Chairman,
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
        var takenRoles = config?.Players
            .Where(p => !p.IsLocalPlayer && p.AssignedRole.HasValue)
            .Select(p => p.AssignedRole!.Value)
            .ToHashSet() ?? new HashSet<PlayerRole>();

        foreach (var (role, title, dept, emoji, color, resp) in RoleData)
        {
            bool taken = takenRoles.Contains(role);
            string? takenBy = config?.Players
                .FirstOrDefault(p => p.AssignedRole == role && !p.IsLocalPlayer)?.Username;

            Cards.Add(new DepartmentSelectionCard
            {
                Role = role,
                Title = title,
                Department = dept,
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

        // Show initial budget for selected role
        InitialBudgetDisplay = card.Role switch
        {
            PlayerRole.FinanceManager => "Controls the company treasury",
            PlayerRole.HumanResourcesManager => "Manages workforce budget",
            PlayerRole.ProductionManager => "Allocates production resources",
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
        // Multiplayer players (host or joiner) never left the Lobby to get
        // here except via the host's CompanySetup detour or a joiner's
        // direct connect — either way, "back" means the Lobby.
        // Solo players came from CompanySetup and should return there.
        if (_flow.SessionConfig?.IsMultiplayer == true)
            _flow.NavigateTo(OnboardingScreen.Lobby);
        else
            _flow.NavigateTo(OnboardingScreen.CompanySetup);
    }
}