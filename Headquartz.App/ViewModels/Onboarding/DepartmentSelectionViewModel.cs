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

    // ── Collections ──────────────────────────────────────────

    public ObservableCollection<DepartmentSelectionCard> Cards { get; } = [];

    private static readonly (PlayerRole Role, string Title, string Dept,
        string Emoji, string Color, string Responsibilities, string Difficulty, int Stars)[] RoleData =
    [
        (PlayerRole.HumanResourcesManager,
            "HR Manager", "Human Resources", "👥", "#8B5CF6",
            "Hiring · Morale · Training · Payroll · Resignations",
            "Medium", 2),

        (PlayerRole.FinanceManager,
            "Finance Manager", "Finance", "💰", "#10B981",
            "Budgets · Cash Flow · Loans · Payroll Risk · Audits",
            "Hard", 3),

        (PlayerRole.SalesManager,
            "Sales Manager", "Sales", "📈", "#3B82F6",
            "Revenue · Orders · Clients · Pipeline · Deadlines",
            "Medium", 2),

        (PlayerRole.MarketingManager,
            "Marketing Manager", "Marketing", "📣", "#F59E0B",
            "Campaigns · Brand · Reputation · Research · Demand",
            "Easy", 1),

        (PlayerRole.ProductionManager,
            "Production Manager", "Production", "🏭", "#EF4444",
            "Manufacturing · Work Orders · Maintenance · Quality",
            "Hard", 3),

        (PlayerRole.WarehouseManager,
            "Warehouse Manager", "Warehouse", "📦", "#F97316",
            "Inventory · Stock Levels · Restocking · Storage",
            "Medium", 2),

        (PlayerRole.LogisticsManager,
            "Logistics Manager", "Logistics", "🚚", "#06B6D4",
            "Shipments · Routes · Delivery SLAs · Fleet",
            "Hard", 3),

        (PlayerRole.Chairman,
            "Board Chairman", "Management", "🏛️", "#EAB308",
            "Full Oversight · All Reports · Strategic Decisions",
            "Expert", 4),
    ];

    public DepartmentSelectionViewModel(OnboardingFlowService flow)
    {
        _flow = flow;

        var config = flow.SessionConfig;

        CompanyName = config?.CompanyName ?? "Unknown Co.";
        IndustryLabel = config?.Industry.ToString() ?? "";
        DifficultyLabel = config?.Difficulty.ToString() ?? "";
        CapitalDisplay = $"${config?.InitialCapital:N0}";

        BuildCards(config);
    }

    private void BuildCards(GameSessionConfig? config)
    {
        var takenRoles = config?.Players
            .Where(p => !p.IsLocalPlayer && p.AssignedRole.HasValue)
            .Select(p => p.AssignedRole!.Value)
            .ToHashSet() ?? new HashSet<PlayerRole>();

        foreach (var (role, title, dept, emoji, color, resp, diff, stars) in RoleData)
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
                DifficultyLabel = diff,
                DifficultyStars = stars,
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
        _flow.SelectDepartment(SelectedCard.Role);
    }

    [RelayCommand]
    private void GoBack()
    {
        _flow.NavigateTo(OnboardingScreen.CompanySetup);
    }
}
