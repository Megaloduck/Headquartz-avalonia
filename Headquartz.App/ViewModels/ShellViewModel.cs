using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Headquartz.App.Models;
using Headquartz.App.Services;
using Headquartz.Domain.Enums;

using System.Collections.ObjectModel;

namespace Headquartz.App.ViewModels;

public partial class ShellViewModel
    : ViewModelBase
{
    private readonly NavigationService
        _navigation;

    private readonly SimulationService
        _simulation;

    // =========================================================
    // SPLASH STATE
    // =========================================================

    private bool _roleSelected = false;
    public bool RoleSelected
    {
        get => _roleSelected;
        set => SetProperty(ref _roleSelected, value);
    }

    public ObservableCollection<RoleCardModel>
        RoleCards
    { get; } = [];

    // =========================================================
    // SHELL STATE
    // =========================================================

    [ObservableProperty]
    private PlayerRole _currentRole =
        PlayerRole.HumanResourcesManager;

    private string _roleName = "";
    public string RoleName
    {
        get => _roleName;
        set => SetProperty(ref _roleName, value);
    }

    private ViewModelBase? _currentView;
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ObservableCollection<SidebarSection>
        SidebarSections
            { get; } = [];

    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public ShellViewModel()
    {
        _simulation = new SimulationService();

        _navigation = new NavigationService(_simulation);

        _ = _simulation.StartAsync();

        _navigation.OnViewChanged += HandleViewChanged;

        BuildRoleCards();
    }

    // =========================================================
    // SPLASH — ROLE SELECTION
    // =========================================================

    [RelayCommand]
    private void SelectRole(RoleCardModel card)
    {
        CurrentRole = card.Role;

        LoadSidebar();

        _navigation.Navigate("company");

        RoleSelected = true;
    }

    [RelayCommand]
    private void BackToRoleSelect()
    {
        RoleSelected = false;
    }

    private void BuildRoleCards()
    {
        RoleCards.Clear();

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.HumanResourcesManager,
            Title = "HR Manager",
            Department = "Human Resources",
            Description = "Hiring, morale, training & payroll",
            Emoji = "👥",
            AccentColor = "#8B5CF6",
        });

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.FinanceManager,
            Title = "Finance Manager",
            Department = "Finance",
            Description = "Budgets, cash flow & audits",
            Emoji = "💰",
            AccentColor = "#10B981",
        });

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.SalesManager,
            Title = "Sales Manager",
            Department = "Sales",
            Description = "Revenue, clients & pipeline",
            Emoji = "📈",
            AccentColor = "#3B82F6",
        });

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.MarketingManager,
            Title = "Marketing Manager",
            Department = "Marketing",
            Description = "Campaigns, branding & research",
            Emoji = "📣",
            AccentColor = "#F59E0B",
        });

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.ProductionManager,
            Title = "Production Manager",
            Department = "Production",
            Description = "Manufacturing, lines & quality",
            Emoji = "🏭",
            AccentColor = "#EF4444",
        });

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.WarehouseManager,
            Title = "Warehouse Manager",
            Department = "Warehouse",
            Description = "Inventory, stock & storage",
            Emoji = "📦",
            AccentColor = "#F97316",
        });

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.LogisticsManager,
            Title = "Logistics Manager",
            Department = "Logistics",
            Description = "Shipments, routes & fleet",
            Emoji = "🚚",
            AccentColor = "#06B6D4",
        });

        RoleCards.Add(new RoleCardModel
        {
            Role = PlayerRole.Chairman,
            Title = "Board Chairman",
            Department = "Management",
            Description = "Oversee all departments",
            Emoji = "🏛️",
            AccentColor = "#EAB308",
        });
    }

    // =========================================================
    // NAVIGATION
    // =========================================================

    [RelayCommand]
    private void Navigate(SidebarItem item)
    {
        _navigation.Navigate(item.Route);
    }

    private void HandleViewChanged()
    {
        CurrentView = _navigation.CurrentView;
    }

    // =========================================================
    // SIDEBAR
    // =========================================================

    partial void OnCurrentRoleChanged(PlayerRole value)
    {
        LoadSidebar();
    }

    private void LoadSidebar()
    {
        SidebarSections.Clear();

        RoleName = CurrentRole switch
        {
            PlayerRole.HumanResourcesManager => "HR Manager",
            PlayerRole.FinanceManager => "Finance Manager",
            PlayerRole.SalesManager => "Sales Manager",
            PlayerRole.MarketingManager => "Marketing Manager",
            PlayerRole.ProductionManager => "Production Manager",
            PlayerRole.WarehouseManager => "Warehouse Manager",
            PlayerRole.LogisticsManager => "Logistics Manager",
            PlayerRole.Chairman => "Board Chairman",
            _ => "Unknown Role",
        };

        foreach (var section in
            SidebarService.GetSections(CurrentRole))
        {
            SidebarSections.Add(section);
        }
    }
}